using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga do planejamento fez (ou faria, na simulação), em número — sem nome de pessoa.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDoPlanejamento(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// O PLANEJAMENTO COMERCIAL DA API GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — <c>--somente-planejamento-gn</c>,
/// o segundo modo da rotina <c>METAS_GESTAO_NEGOCIOS</c>, depois das metas.
///
/// <para><b>Três sincronias numa transação</b>: o de-para de consultores (<see cref="GestorDoConsultor"/>), o forecast da
/// gerência (<see cref="ForecastDaGerencia"/>) e as cotas de consórcio vendidas (<see cref="CotaDeConsorcioVendida"/>). A
/// linha nova entra, a revisada muda com trilha, a que sumiu é excluída sem apagar, e a que volta é reativada.</para>
///
/// <para><b>A cota que sumiu só é excluída DENTRO dos meses que a leitura cobre.</b> O painel de consórcio só traz o ano
/// fiscal corrente: quando o ano vira, as cotas do anterior somem da resposta, e isso não é cota cancelada.</para>
///
/// <para><b>As travas das metas</b>: formato ilegível demais aborta a rodada inteira; excluir mais de
/// <see cref="FracaoMaximaDeRemocao"/> do que está vigente numa tabela também — é leitura parcial, e só
/// <c>--aceitar-remocao</c>, no terminal, passa por cima.</para>
///
/// <para><b>A cota de filial que o CRM não tem</b> (Digital, que é número 0 e não tem código no TOTVS) não entra — ela não é
/// de filial nenhuma, e a fronteira de acesso é a filial. É contada à parte, e a tela da meta diz quantas são.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="ler">A leitura do planejamento.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDoPlanejamentoDaGestaoDeNegocios(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<LeituraDoPlanejamentoNaOrigem>>> ler,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>A maior fração do que está vigente numa tabela que uma rodada pode excluir sem abortar.</summary>
    internal const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>Abaixo disto a trava de remoção não se aplica: numa tabela pequena, poucas saídas já passam de 20%.</summary>
    internal const int JanelaMinima = 20;

    /// <summary>A maior fração de linhas com formato ilegível que uma rodada aceita, em cada rota.</summary>
    internal const double FracaoMaximaDeRecusa = 0.05;

    /// <summary>Rótulo: linhas do de-para lidas.</summary>
    internal const string RotuloDoTimeLido = "linhas do de-para de consultores lidas";

    /// <summary>Rótulo: linhas do forecast lidas.</summary>
    internal const string RotuloDoForecastLido = "linhas do forecast lidas";

    /// <summary>Rótulo: cotas vendidas lidas.</summary>
    internal const string RotuloDasCotasLidas = "cotas vendidas lidas (as linhas \"Realizado\" da performance)";

    /// <summary>Rótulo: cotas de filial que o CRM não tem.</summary>
    internal const string RotuloDasCotasSemFilial = "cotas de filial que o CRM não tem (Digital) — contadas à parte, não gravadas";

    /// <summary>Rótulo: novas.</summary>
    internal const string RotuloDeNovas = "novas";

    /// <summary>Rótulo: revisadas.</summary>
    internal const string RotuloDeRevisadas = "revisadas pela origem (a trilha guarda o antes e o depois)";

    /// <summary>Rótulo: iguais.</summary>
    internal const string RotuloDeIguais = "sem mudança";

    /// <summary>Rótulo: excluídas.</summary>
    internal const string RotuloDeExcluidas = "sumiram da origem (excluídas, nunca apagadas)";

    /// <summary>Rótulo: reativadas.</summary>
    internal const string RotuloDeReativadas = "voltaram à origem (reativadas na mesma linha)";

    /// <summary>Rótulo: recusadas.</summary>
    internal const string RotuloDeRecusadas = "linhas com formato ilegível (a que já existia fica como estava)";

    /// <summary>Etapa do de-para.</summary>
    internal const string EtapaDoTime = "1. De-para de consultores (gestor de cada consultor)";

    /// <summary>Etapa do forecast.</summary>
    internal const string EtapaDoForecast = "2. Forecast da gerência";

    /// <summary>Etapa do consórcio.</summary>
    internal const string EtapaDoConsorcio = "3. Cotas de consórcio vendidas";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Se a rodada excluiria demais — leitura parcial.</summary>
    /// <param name="vigentes">As linhas vigentes antes da rodada.</param>
    /// <param name="aExcluir">As que a rodada excluiria.</param>
    internal static bool RemocaoPassaDaTrava(int vigentes, int aExcluir) =>
        vigentes >= JanelaMinima && aExcluir > vigentes * FracaoMaximaDeRemocao;

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta: não abre transação de escrita.</param>
    /// <param name="aceitarRemocao">Passa por cima da trava de remoção — só no terminal.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDoPlanejamento>> ExecutarAsync(bool simular, bool aceitarRemocao, CancellationToken ct)
    {
        relatar("Lendo o planejamento da API Gestão de Negócios (de-para de consultores, forecast, performance de consórcio e filiais; só GET)…");
        var lida = await ler(ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDoPlanejamento>.Indisponivel(lida.Erro!);

        var origem = lida.Valor;
        if (origem.Time.Count == 0 && origem.Forecast.Count == 0 && origem.Cotas.Count == 0)
            return Resultado<RelatorioDoPlanejamento>.Indisponivel(
                "O de-para, o forecast e o consórcio vieram vazios. Isso é uma leitura a conferir — nada foi excluído e nada foi gravado.");

        var leitura = new LeituraDoPlanejamento(relogio(), origem.GeradaEmUtc);

        Plano plano;
        await using (var banco = abrirContexto())
        {
            plano = await PlanejarAsync(banco, origem, ct);
        }

        Relatar(plano);

        foreach (var (etapa, lidas, recusadas) in new[]
                 {
                     (EtapaDoTime, origem.Time.Count, plano.Time.Recusadas),
                     (EtapaDoForecast, origem.Forecast.Count, plano.Forecast.Recusadas),
                     (EtapaDoConsorcio, origem.Cotas.Count, plano.Cotas.Recusadas)
                 })
        {
            if (lidas > 0 && recusadas > lidas * FracaoMaximaDeRecusa)
                return Resultado<RelatorioDoPlanejamento>.Indisponivel(string.Create(CultureInfo.InvariantCulture,
                    $"{etapa}: {recusadas} de {lidas} linhas com formato ilegível (acima de {FracaoMaximaDeRecusa:P0}). A API mandou o campo noutra " +
                    $"forma — A CARGA INTEIRA FOI ABORTADA: nada foi gravado e o frescor não foi carimbado."));
        }

        foreach (var (etapa, sincronia) in new[] { (EtapaDoTime, plano.Time), (EtapaDoForecast, plano.Forecast), (EtapaDoConsorcio, plano.Cotas) })
        {
            if (!RemocaoPassaDaTrava(sincronia.Vigentes, sincronia.AExcluir.Count)) continue;
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"{etapa}: a rodada excluiria {sincronia.AExcluir.Count} de {sincronia.Vigentes} linhas vigentes (acima de {FracaoMaximaDeRemocao:P0}) — leitura parcial, ou a GN renumerou os ids.");
            if (!aceitarRemocao)
                return Resultado<RelatorioDoPlanejamento>.Indisponivel(
                    texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Se a remoção for mesmo o que aconteceu, rode no terminal com --aceitar-remocao.");
            _observacoes.Add(texto + " Aceita por --aceitar-remocao, no terminal.");
        }

        if (!plano.EsquemaExiste && !simular)
            return Resultado<RelatorioDoPlanejamento>.Indisponivel(
                "O banco ainda não tem a migração do planejamento da Gestão de Negócios. Publique a versão com ela antes — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura.");
            return Resultado<RelatorioDoPlanejamento>.Ok(new RelatorioDoPlanejamento(true, _contagens, _observacoes));
        }

        await AplicarAsync(plano, leitura, origem, ct);
        return Resultado<RelatorioDoPlanejamento>.Ok(new RelatorioDoPlanejamento(false, _contagens, _observacoes));
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private static async Task<Plano> PlanejarAsync(CrmDbContext banco, LeituraDoPlanejamentoNaOrigem origem, CancellationToken ct)
    {
        var plano = new Plano { EsquemaExiste = await EsquemaExisteAsync(banco, ct) };

        plano.SistemaId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
        var sistema = plano.EsquemaExiste ? plano.SistemaId : null;

        // ---- o de-para ----
        var timeNoCrm = sistema is { } s1
            ? await banco.GestoresDosConsultores.AsNoTracking().Where(g => g.SistemaId == s1)
                .Select(g => new NoCrm(g.IdNaOrigem.ToString(CultureInfo.InvariantCulture), g.HashDaOrigem, g.ExcluidoEm != null, null, null, null))
                .ToDictionaryAsync(g => g.Chave, StringComparer.Ordinal, ct)
            : new Dictionary<string, NoCrm>(StringComparer.Ordinal);

        foreach (var linha in origem.Time)
        {
            var consultor = linha.Consultor?.Trim().ToUpperInvariant() ?? string.Empty;
            var capitao = linha.Capitao?.Trim() ?? string.Empty;
            if (consultor.Length is 0 or > GestorDoConsultor.TamanhoDaPessoa || capitao.Length is 0 or > GestorDoConsultor.TamanhoDaPessoa)
            {
                plano.Time.Recusadas++;
                plano.Time.Vistas.Add(linha.Id.ToString(CultureInfo.InvariantCulture));
                continue;
            }

            int? filial = int.TryParse(linha.FilialNumero, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero) ? numero : null;
            var vigente = Data(linha.InicioVigencia);
            var hash = Resumir(linha.Id.ToString(CultureInfo.InvariantCulture), consultor, capitao, linha.FilialNumero, linha.InicioVigencia);
            plano.Time.Planejar(linha.Id.ToString(CultureInfo.InvariantCulture), hash, timeNoCrm, null, null);
            plano.TimeValido.Add((linha.Id, consultor, capitao, filial, vigente, hash));
        }

        plano.Time.FecharExclusoes(timeNoCrm, _ => true);

        // ---- o forecast ----
        var forecastNoCrm = sistema is { } s2
            ? await banco.ForecastsDaGerencia.AsNoTracking().Where(f => f.SistemaId == s2)
                .Select(f => new NoCrm(f.IdNaOrigem.ToString(CultureInfo.InvariantCulture), f.HashDaOrigem, f.ExcluidoEm != null, null, null, null))
                .ToDictionaryAsync(f => f.Chave, StringComparer.Ordinal, ct)
            : new Dictionary<string, NoCrm>(StringComparer.Ordinal);

        foreach (var linha in origem.Forecast)
        {
            var chave = linha.Id.ToString(CultureInfo.InvariantCulture);
            var mes = SaneamentoDasMetas.Competencia(linha.Mes);
            var gestor = linha.Gestor?.Trim() ?? string.Empty;
            var textoDaLinha = linha.Linha?.Trim() ?? string.Empty;
            var forecastOk = Quantidade(linha.Forecast, out var forecast);
            var bestGuessOk = Quantidade(linha.BestGuess, out var bestGuess);
            if (mes is null || gestor.Length is 0 or > GestorDoConsultor.TamanhoDaPessoa
                || textoDaLinha.Length is 0 or > ForecastDaGerencia.TamanhoDaLinha || !forecastOk || !bestGuessOk)
            {
                plano.Forecast.Recusadas++;
                plano.Forecast.Vistas.Add(chave);
                continue;
            }

            var hash = Resumir(chave, linha.Mes, gestor, textoDaLinha, linha.Forecast, linha.BestGuess);
            plano.Forecast.Planejar(chave, hash, forecastNoCrm, null, null);
            plano.ForecastValido.Add((linha.Id, mes.Value, gestor, textoDaLinha, forecast, bestGuess, hash));
        }

        plano.Forecast.FecharExclusoes(forecastNoCrm, _ => true);

        // ---- as cotas ----
        var empresaPorCodigo = await banco.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);
        var codigoPorNomeDeFilial = FiliaisDaGestaoDeNegocios.CodigoPorNome(origem.Filiais);

        var contasPorLogin = (await banco.Usuarios.AsNoTracking()
                .Where(u => u.ExcluidoEm == null)
                .Select(u => new { u.Id, u.NomePrincipal })
                .ToListAsync(ct))
            .GroupBy(u => MetaDeVenda.ChaveDaPessoa(ParteLocal(u.NomePrincipal)), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(u => u.Id).ToList(), StringComparer.Ordinal);

        var cotasNoCrm = sistema is { } s3
            ? (await banco.CotasDeConsorcioVendidas.AsNoTracking().Where(c => c.SistemaId == s3)
                    .Select(c => new { c.Grupo, c.Cota, c.HashDaOrigem, c.ExcluidoEm, c.EmpresaId, c.ConsultorUsuarioId, c.Competencia })
                    .ToListAsync(ct))
                .ToDictionary(c => CotaDeConsorcioVendida.Chave(c.Grupo, c.Cota),
                    c => new NoCrm(CotaDeConsorcioVendida.Chave(c.Grupo, c.Cota), c.HashDaOrigem, c.ExcluidoEm != null, c.EmpresaId, c.ConsultorUsuarioId, c.Competencia),
                    StringComparer.Ordinal)
            : new Dictionary<string, NoCrm>(StringComparer.Ordinal);

        foreach (var linha in origem.Cotas)
        {
            var grupo = linha.Grupo?.Trim() ?? string.Empty;
            var cota = linha.Cota?.Trim() ?? string.Empty;
            var chave = CotaDeConsorcioVendida.Chave(grupo, cota);
            var mes = LeitorDoPlanejamentoDaGestaoDeNegocios.Mes(linha.MesRotulo) ?? SaneamentoDasMetas.Competencia(linha.AlocadaEm);
            var alocada = Data(linha.AlocadaEm);
            var consultor = linha.Consultor?.Trim().ToUpperInvariant() ?? string.Empty;
            var gestor = linha.Gestor?.Trim() ?? string.Empty;
            var contemplacao = linha.Contemplacao?.Trim() ?? string.Empty;
            var valorOk = Valor(linha.ValorDoBem, out var valorDoBem);
            var parcelaOk = Valor(linha.ValorDaParcela, out var valorDaParcela);

            if (grupo.Length is 0 or > CotaDeConsorcioVendida.TamanhoDoCodigo || cota.Length is 0 or > CotaDeConsorcioVendida.TamanhoDoCodigo
                || mes is null || alocada is null || consultor.Length is 0 or > GestorDoConsultor.TamanhoDaPessoa
                || gestor.Length is 0 or > GestorDoConsultor.TamanhoDaPessoa || contemplacao.Length is 0 or > CotaDeConsorcioVendida.TamanhoDaContemplacao
                || !valorOk || !parcelaOk)
            {
                plano.Cotas.Recusadas++;
                plano.Cotas.Vistas.Add(chave);
                continue;
            }

            // A FILIAL PELO NOME, PELO DE-PARA DA PRÓPRIA GN: o painel escreve a loja pelo nome, e o de-para das lojas diz o
            // código do TOTVS. A cota de loja sem código (Digital) não é de filial nenhuma do CRM — e, se já foi de uma, sai
            // dela: não conta mais no realizado daquela filial.
            if (!codigoPorNomeDeFilial.TryGetValue(CodigoEstavel.De(linha.Filial ?? string.Empty), out var codigo)
                || !empresaPorCodigo.TryGetValue(codigo, out var empresaId))
            {
                plano.CotasSemFilial++;
                continue;
            }

            long? consultorUsuarioId = contasPorLogin.TryGetValue(MetaDeVenda.ChaveDaPessoa(consultor), out var contas) && contas.Count == 1
                ? contas[0]
                : null;

            var hash = Resumir(chave, linha.MesRotulo, linha.Filial, consultor, gestor, contemplacao, linha.AlocadaEm, linha.ContempladaEm,
                linha.Produto, linha.ValorDoBem, linha.ValorDaParcela);
            var dados = new DadosDaCotaNaOrigem(
                empresaId, mes.Value, alocada.Value, contemplacao, Data(linha.ContempladaEm), consultor, consultorUsuarioId, gestor,
                linha.Produto, valorDoBem, valorDaParcela, hash);

            plano.Cotas.Planejar(chave, hash, cotasNoCrm, empresaId, consultorUsuarioId);
            plano.CotasValidas.Add((grupo, cota, dados));
        }

        // SÓ DENTRO DA JANELA DA LEITURA: a cota de um mês que o painel não trouxe (o ano fiscal que virou) fica como estava.
        plano.Cotas.FecharExclusoes(cotasNoCrm, c => c.Competencia is { } competencia && origem.MesesDaPerformance.Contains(competencia));
        return plano;
    }

    // =============================================================================================
    // A gravação — uma transação, executando o plano
    // =============================================================================================

    private async Task AplicarAsync(Plano plano, LeituraDoPlanejamento leitura, LeituraDoPlanejamentoNaOrigem origem, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema, "Gestão de Negócios — API", "API REST com chave, só leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var time = await banco.GestoresDosConsultores.Where(g => g.SistemaId == sistemaId).ToDictionaryAsync(g => g.IdNaOrigem, ct);
        foreach (var (id, consultor, capitao, filial, vigente, hash) in plano.TimeValido)
        {
            if (!time.TryGetValue(id, out var existente))
            {
                banco.GestoresDosConsultores.Add(GestorDoConsultor.Registrar(sistemaId, id, consultor, capitao, filial, vigente, hash, leitura, usuarioId));
                continue;
            }

            existente.Reativar();
            existente.AtualizarDaOrigem(consultor, capitao, filial, vigente, hash, leitura);
        }

        foreach (var chave in plano.Time.AExcluir) time[int.Parse(chave, CultureInfo.InvariantCulture)].Excluir(leitura.LidaEm);

        var forecast = await banco.ForecastsDaGerencia.Where(f => f.SistemaId == sistemaId).ToDictionaryAsync(f => f.IdNaOrigem, ct);
        foreach (var (id, mes, gestor, linha, previsto, bestGuess, hash) in plano.ForecastValido)
        {
            if (!forecast.TryGetValue(id, out var existente))
            {
                banco.ForecastsDaGerencia.Add(ForecastDaGerencia.Registrar(sistemaId, id, mes, gestor, linha, previsto, bestGuess, hash, leitura, usuarioId));
                continue;
            }

            existente.Reativar();
            existente.AtualizarDaOrigem(mes, gestor, linha, previsto, bestGuess, hash, leitura);
        }

        foreach (var chave in plano.Forecast.AExcluir) forecast[int.Parse(chave, CultureInfo.InvariantCulture)].Excluir(leitura.LidaEm);

        var cotas = (await banco.CotasDeConsorcioVendidas.Where(c => c.SistemaId == sistemaId).ToListAsync(ct))
            .ToDictionary(c => CotaDeConsorcioVendida.Chave(c.Grupo, c.Cota), StringComparer.Ordinal);
        foreach (var (grupo, cota, dados) in plano.CotasValidas)
        {
            if (!cotas.TryGetValue(CotaDeConsorcioVendida.Chave(grupo, cota), out var existente))
            {
                banco.CotasDeConsorcioVendidas.Add(CotaDeConsorcioVendida.Registrar(sistemaId, grupo, cota, dados, leitura, usuarioId));
                continue;
            }

            existente.Reativar(usuarioId);
            existente.AtualizarDaOrigem(dados, leitura, usuarioId);
        }

        foreach (var chave in plano.Cotas.AExcluir) cotas[chave].Excluir(usuarioId);

        await banco.SaveChangesAsync(ct);

        // O FRESCOR DE CADA UM — é o que a tela mostra ao lado do forecast e do realizado de consórcio.
        var gerada = (origem.GeradaEmUtc ?? leitura.LidaEm).ToString("O", CultureInfo.InvariantCulture);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, GestorDoConsultor.FluxoDaCarga, origem.Time.Count,
            plano.Time.Gravadas, plano.Time.Recusadas, ct, gerada);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, ForecastDaGerencia.FluxoDaCarga, origem.Forecast.Count,
            plano.Forecast.Gravadas, plano.Forecast.Recusadas, ct, gerada);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, CotaDeConsorcioVendida.FluxoDaCarga, origem.Cotas.Count,
            plano.Cotas.Gravadas, plano.Cotas.Recusadas + plano.CotasSemFilial, ct, gerada);

        await transacao.CommitAsync(ct);
        relatar("Gravado. A sincronia pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
    }

    // =============================================================================================
    // O relatório — só contagens
    // =============================================================================================

    private void Relatar(Plano plano)
    {
        Contar(EtapaDoTime, RotuloDoTimeLido, plano.TimeValido.Count + plano.Time.Recusadas);
        Contar(EtapaDoTime, "gestores distintos", plano.TimeValido.Select(t => t.Capitao).Distinct(StringComparer.Ordinal).Count());
        Contar(EtapaDoTime, "consultores em mais de uma linha (vale a vigência mais recente)",
            plano.TimeValido.GroupBy(t => t.Consultor, StringComparer.Ordinal).Count(g => g.Count() > 1));
        RelatarSincronia(EtapaDoTime, plano.Time);

        Contar(EtapaDoForecast, RotuloDoForecastLido, plano.ForecastValido.Count + plano.Forecast.Recusadas);
        Contar(EtapaDoForecast, "meses com forecast", plano.ForecastValido.Select(f => f.Mes).Distinct().Count());
        Contar(EtapaDoForecast, "  unidades de forecast", plano.ForecastValido.Sum(f => f.Forecast ?? 0));
        Contar(EtapaDoForecast, "  linhas sem forecast informado (nulo, e não zero)", plano.ForecastValido.Count(f => f.Forecast is null));
        Contar(EtapaDoForecast, "  linhas sem best guess informado", plano.ForecastValido.Count(f => f.BestGuess is null));
        RelatarSincronia(EtapaDoForecast, plano.Forecast);

        Contar(EtapaDoConsorcio, RotuloDasCotasLidas, plano.CotasValidas.Count + plano.Cotas.Recusadas + plano.CotasSemFilial);
        Contar(EtapaDoConsorcio, RotuloDasCotasSemFilial, plano.CotasSemFilial);
        Contar(EtapaDoConsorcio, "  cotas com a conta do consultor no CRM", plano.CotasValidas.Count(c => c.Dados.ConsultorUsuarioId is not null));
        RelatarSincronia(EtapaDoConsorcio, plano.Cotas);

        if (!plano.EsquemaExiste)
            _observacoes.Add("O banco lido ainda não tem a migração do planejamento: o plano parte de nada gravado, que é o que o banco terá logo depois dela.");
    }

    private void RelatarSincronia(string etapa, Sincronia s)
    {
        Contar(etapa, RotuloDeRecusadas, s.Recusadas);
        Contar(etapa, "vigentes antes da rodada", s.Vigentes);
        Contar(etapa, RotuloDeNovas, s.Novas);
        Contar(etapa, RotuloDeRevisadas, s.ARevisar);
        Contar(etapa, RotuloDeIguais, s.Iguais);
        Contar(etapa, RotuloDeReativadas, s.AReativar);
        Contar(etapa, RotuloDeExcluidas, s.AExcluir.Count);
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>Se o banco já tem a tabela do forecast — a simulação roda antes da publicação que traz a migração.</summary>
    private static async Task<bool> EsquemaExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'organizacao.ForecastDaGerencia')")
            .SingleAsync(ct) > 0;

    /// <summary>A quantidade do forecast: vazio é "não informado"; texto que não é inteiro de 0 ao máximo é formato ilegível.</summary>
    private static bool Quantidade(string? bruto, out int? valor)
    {
        valor = null;
        if (string.IsNullOrWhiteSpace(bruto)) return true;
        if (!decimal.TryParse(bruto.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var numero)
            || numero != decimal.Truncate(numero) || numero < 0 || numero > ForecastDaGerencia.QuantidadeMaxima)
            return false;
        valor = (int)numero;
        return true;
    }

    /// <summary>Um valor em reais: vazio é nulo; texto que não é número, ou negativo, é formato ilegível.</summary>
    private static bool Valor(string? bruto, out decimal? valor)
    {
        valor = null;
        if (string.IsNullOrWhiteSpace(bruto)) return true;
        if (!decimal.TryParse(bruto.Trim(), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var numero)
            || numero is < 0 or > 999_999_999_999m)
            return false;
        valor = decimal.Round(numero, 2);
        return true;
    }

    /// <summary>Uma data <c>AAAA-MM-DD</c> (com hora ou não); nula quando não se lê.</summary>
    private static DateOnly? Data(string? bruto)
    {
        var texto = bruto?.Trim() ?? string.Empty;
        return texto.Length >= 10 && DateOnly.TryParseExact(texto[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            ? data
            : null;
    }

    /// <summary>O resumo do conteúdo lido — não é reversível para o nome de ninguém.</summary>
    private static string Resumir(params string?[] partes) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f', partes.Select(p => p ?? string.Empty)))));

    /// <summary>O login da conta: a parte antes do <c>@</c> do nome principal.</summary>
    private static string ParteLocal(string nomePrincipal)
    {
        var arroba = nomePrincipal.IndexOf('@', StringComparison.Ordinal);
        return (arroba < 0 ? nomePrincipal : nomePrincipal[..arroba]).Trim().ToLowerInvariant();
    }

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    /// <summary>Uma linha já gravada, só com o que o plano compara.</summary>
    private sealed record NoCrm(string Chave, string Hash, bool Excluida, int? EmpresaId, long? ConsultorUsuarioId, DateOnly? Competencia);

    /// <summary>A sincronia de UMA tabela: o que entra, o que muda, o que sai.</summary>
    private sealed class Sincronia
    {
        public int Vigentes { get; set; }
        public int Recusadas { get; set; }
        public int Novas { get; set; }
        public int ARevisar { get; set; }
        public int Iguais { get; set; }
        public int AReativar { get; set; }
        public List<string> AExcluir { get; } = [];
        public HashSet<string> Vistas { get; } = new(StringComparer.Ordinal);

        /// <summary>Quantas linhas a rodada grava.</summary>
        public int Gravadas => Novas + ARevisar + AReativar + AExcluir.Count;

        /// <summary>Planeja uma linha válida contra o que o CRM tem.</summary>
        public void Planejar(string chave, string hash, IReadOnlyDictionary<string, NoCrm> existentes, int? empresaId, long? consultorUsuarioId)
        {
            Vistas.Add(chave);
            if (!existentes.TryGetValue(chave, out var existente))
            {
                Novas++;
                return;
            }

            if (existente.Excluida) AReativar++;
            if (existente.Hash != hash || existente.EmpresaId != empresaId || existente.ConsultorUsuarioId != consultorUsuarioId) ARevisar++;
            else if (!existente.Excluida) Iguais++;
        }

        /// <summary>A vigente que não veio — e só quando a regra de exclusão da tabela deixa.</summary>
        public void FecharExclusoes(IReadOnlyDictionary<string, NoCrm> existentes, Func<NoCrm, bool> podeExcluir)
        {
            Vigentes = existentes.Values.Count(e => !e.Excluida);
            AExcluir.AddRange(existentes.Values.Where(e => !e.Excluida && !Vistas.Contains(e.Chave) && podeExcluir(e)).Select(e => e.Chave));
        }
    }

    private sealed class Plano
    {
        public bool EsquemaExiste { get; init; }
        public int? SistemaId { get; set; }
        public Sincronia Time { get; } = new();
        public Sincronia Forecast { get; } = new();
        public Sincronia Cotas { get; } = new();
        public int CotasSemFilial { get; set; }

        public List<(int Id, string Consultor, string Capitao, int? Filial, DateOnly? Vigente, string Hash)> TimeValido { get; } = [];
        public List<(int Id, DateOnly Mes, string Gestor, string Linha, int? Forecast, int? BestGuess, string Hash)> ForecastValido { get; } = [];
        public List<(string Grupo, string Cota, DadosDaCotaNaOrigem Dados)> CotasValidas { get; } = [];
    }
}
