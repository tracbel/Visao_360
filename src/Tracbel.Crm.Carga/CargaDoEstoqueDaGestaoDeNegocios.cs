using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga do estoque fez (ou faria, na simulação), em número — sem nome de ninguém.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDoEstoque(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);

    /// <summary>A contagem de um rótulo numa etapa.</summary>
    public int Valor(string etapa, string rotulo) => Contagens.Where(c => c.Etapa == etapa && c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// O ESTOQUE E A COBERTURA DA API GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — <c>--somente-estoque-gn</c>, a
/// rotina 12 <c>ESTOQUE_GESTAO_NEGOCIOS</c>.
///
/// <para><b>Duas sincronias numa transação</b>: as máquinas do estoque e dos pedidos à fábrica
/// (<see cref="EquipamentoEmEstoque"/>, pelo chassi interno do TOTVS) e a cobertura (<see cref="CoberturaDoEstoque"/>, por
/// mês e por grupo). A linha nova entra, a revisada muda com trilha, a que sumiu (a máquina vendida, o mês que saiu da
/// janela) é excluída sem apagar, e a que volta é reativada.</para>
///
/// <para><b>As travas das metas</b>: mais de <see cref="FracaoMaximaDeRecusa"/> de formato ilegível, ou excluir mais de
/// <see cref="FracaoMaximaDeRemocao"/> do que está vigente, aborta a rodada inteira. A remoção grande é o sinal da leitura
/// que encolheu — o painel com período aplicado perde os pedidos à fábrica, um terço das linhas. Só
/// <c>--aceitar-remocao</c>, no terminal, passa por cima.</para>
///
/// <para><b>A máquina de loja que o CRM não tem</b> (sem filial, ou uma loja sem código no TOTVS) não entra: a fronteira de
/// acesso é a filial. É contada à parte.</para>
/// </summary>
internal sealed class CargaDoEstoqueDaGestaoDeNegocios(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<LeituraDoEstoqueNaOrigem>>> ler,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>A maior fração do que está vigente que uma rodada pode excluir sem abortar.</summary>
    internal const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>Abaixo disto a trava de remoção não se aplica.</summary>
    internal const int JanelaMinima = 20;

    /// <summary>A maior fração de linhas com formato ilegível que uma rodada aceita.</summary>
    internal const double FracaoMaximaDeRecusa = 0.05;

    /// <summary>Etapa das máquinas.</summary>
    internal const string EtapaDoEstoque = "1. Estoque e pedidos à fábrica";

    /// <summary>Etapa da cobertura.</summary>
    internal const string EtapaDaCobertura = "2. Cobertura em meses de estoque";

    /// <summary>Rótulo: lidas.</summary>
    internal const string RotuloDeLidas = "linhas lidas";

    /// <summary>Rótulo: máquinas de loja que o CRM não tem.</summary>
    internal const string RotuloSemFilial = "máquinas sem filial do CRM (sem filial ou loja sem código no TOTVS) — contadas à parte, não gravadas";

    /// <summary>Rótulo: no pátio.</summary>
    internal const string RotuloNoPatio = "  no pátio (estoque, remessa, consignado, em transferência)";

    /// <summary>Rótulo: pedidos à fábrica.</summary>
    internal const string RotuloDePedidos = "  pedidos à fábrica";

    /// <summary>Rótulo: reservadas.</summary>
    internal const string RotuloDeReservadas = "  reservadas";

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

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Se a rodada excluiria demais — leitura parcial.</summary>
    internal static bool RemocaoPassaDaTrava(int vigentes, int aExcluir) =>
        vigentes >= JanelaMinima && aExcluir > vigentes * FracaoMaximaDeRemocao;

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta.</param>
    /// <param name="aceitarRemocao">Passa por cima da trava de remoção — só no terminal.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDoEstoque>> ExecutarAsync(bool simular, bool aceitarRemocao, CancellationToken ct)
    {
        relatar("Lendo o estoque, a cobertura e as filiais da API Gestão de Negócios (só GET)…");
        var lida = await ler(ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDoEstoque>.Indisponivel(lida.Erro!);

        var origem = lida.Valor;
        if (origem.Equipamentos.Count == 0)
            return Resultado<RelatorioDoEstoque>.Indisponivel(
                "O estoque veio vazio. Isso é uma leitura a conferir — nada foi excluído e nada foi gravado.");

        var lidaEm = relogio();

        Plano plano;
        await using (var banco = abrirContexto())
        {
            plano = await PlanejarAsync(banco, origem, ct);
        }

        Relatar(plano);

        foreach (var (etapa, lidas, recusadas) in new[]
                 {
                     (EtapaDoEstoque, origem.Equipamentos.Count, plano.Estoque.Recusadas),
                     (EtapaDaCobertura, origem.Cobertura.Count, plano.Cobertura.Recusadas)
                 })
        {
            if (lidas > 0 && recusadas > lidas * FracaoMaximaDeRecusa)
                return Resultado<RelatorioDoEstoque>.Indisponivel(string.Create(CultureInfo.InvariantCulture,
                    $"{etapa}: {recusadas} de {lidas} linhas com formato ilegível (acima de {FracaoMaximaDeRecusa:P0}). A API mandou o campo noutra " +
                    $"forma — A CARGA INTEIRA FOI ABORTADA: nada foi gravado e o frescor não foi carimbado."));
        }

        foreach (var (etapa, sincronia) in new[] { (EtapaDoEstoque, plano.Estoque), (EtapaDaCobertura, plano.Cobertura) })
        {
            if (!RemocaoPassaDaTrava(sincronia.Vigentes, sincronia.AExcluir.Count)) continue;
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"{etapa}: a rodada excluiria {sincronia.AExcluir.Count} de {sincronia.Vigentes} linhas vigentes (acima de {FracaoMaximaDeRemocao:P0}) — leitura parcial, ou o painel passou a aplicar um período.");
            if (!aceitarRemocao)
                return Resultado<RelatorioDoEstoque>.Indisponivel(
                    texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Se a remoção for mesmo o que aconteceu, rode no terminal com --aceitar-remocao.");
            _observacoes.Add(texto + " Aceita por --aceitar-remocao, no terminal.");
        }

        if (!plano.EsquemaExiste && !simular)
            return Resultado<RelatorioDoEstoque>.Indisponivel(
                "O banco ainda não tem a migração do estoque da Gestão de Negócios. Publique a versão com ela antes — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura.");
            return Resultado<RelatorioDoEstoque>.Ok(new RelatorioDoEstoque(true, _contagens, _observacoes));
        }

        await AplicarAsync(plano, lidaEm, origem, ct);
        return Resultado<RelatorioDoEstoque>.Ok(new RelatorioDoEstoque(false, _contagens, _observacoes));
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private static async Task<Plano> PlanejarAsync(CrmDbContext banco, LeituraDoEstoqueNaOrigem origem, CancellationToken ct)
    {
        var plano = new Plano { EsquemaExiste = await EsquemaExisteAsync(banco, ct) };
        var sistema = plano.EsquemaExiste
            ? await banco.Sistemas.AsNoTracking().Where(s => s.Codigo == LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema)
                .Select(s => (int?)s.Id).FirstOrDefaultAsync(ct)
            : null;

        // ---- as máquinas ----
        var empresaPorCodigo = await banco.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);
        var codigoPorNome = FiliaisDaGestaoDeNegocios.CodigoPorNome(origem.Filiais);

        var estoqueNoCrm = sistema is { } s1
            ? (await banco.EquipamentosEmEstoque.IgnoreQueryFilters().AsNoTracking().Where(e => e.SistemaId == s1)
                    .Select(e => new { e.Chaint, e.HashDaOrigem, e.ExcluidoEm, e.EmpresaId })
                    .ToListAsync(ct))
                .ToDictionary(e => e.Chaint, e => new NoCrm(e.Chaint, e.HashDaOrigem, e.ExcluidoEm != null, e.EmpresaId), StringComparer.Ordinal)
            : new Dictionary<string, NoCrm>(StringComparer.Ordinal);

        foreach (var linha in origem.Equipamentos)
        {
            var chaint = linha.Chaint?.Trim() ?? string.Empty;
            var dados = Sanear(linha, 0);
            if (chaint.Length is 0 or > EquipamentoEmEstoque.TamanhoDoChaint || dados is null)
            {
                plano.Estoque.Recusadas++;
                if (chaint.Length > 0) plano.Estoque.Vistas.Add(chaint);
                continue;
            }

            // A FILIAL PELO NOME, PELO DE-PARA DA PRÓPRIA GN. A máquina de loja que o CRM não tem sai da filial em que estava.
            if (!codigoPorNome.TryGetValue(CodigoEstavel.De(linha.Filial ?? string.Empty), out var codigo)
                || !empresaPorCodigo.TryGetValue(codigo, out var empresaId))
            {
                plano.SemFilial++;
                continue;
            }

            dados = dados with { EmpresaId = empresaId };
            plano.Estoque.Planejar(chaint, dados.HashDaOrigem, estoqueNoCrm, empresaId);
            plano.EstoqueValido.Add((chaint, dados));
        }

        plano.Estoque.FecharExclusoes(estoqueNoCrm);

        // ---- a cobertura ----
        var coberturaNoCrm = sistema is { } s2
            ? (await banco.CoberturasDoEstoque.AsNoTracking().Where(c => c.SistemaId == s2)
                    .Select(c => new { c.Recorte, c.Chave, c.HashDaOrigem, c.ExcluidoEm })
                    .ToListAsync(ct))
                .ToDictionary(c => CoberturaDoEstoque.ChaveNatural(c.Recorte, c.Chave),
                    c => new NoCrm(CoberturaDoEstoque.ChaveNatural(c.Recorte, c.Chave), c.HashDaOrigem, c.ExcluidoEm != null, null),
                    StringComparer.Ordinal)
            : new Dictionary<string, NoCrm>(StringComparer.Ordinal);

        foreach (var item in origem.Cobertura)
        {
            DateOnly? competencia = item.Recorte == CoberturaDoEstoque.RecortePorMes ? LeitorDoPlanejamentoDaGestaoDeNegocios.Mes(item.Chave) : null;
            var chave = competencia is { } mes ? CoberturaDoEstoque.ChaveDoMes(mes) : item.Chave?.Trim() ?? string.Empty;
            var mesesOk = decimal.TryParse(item.Meses, NumberStyles.Number, CultureInfo.InvariantCulture, out var meses);
            var vendasOk = decimal.TryParse(item.Vendas, NumberStyles.Number, CultureInfo.InvariantCulture, out var vendas);
            if ((item.Recorte == CoberturaDoEstoque.RecortePorMes && competencia is null) || chave.Length is 0 or > CoberturaDoEstoque.TamanhoDaChave
                || !mesesOk || meses is < 0 or > CoberturaDoEstoque.MesesMaximos || !vendasOk || vendas < 0 || vendas != decimal.Truncate(vendas) || vendas > int.MaxValue)
            {
                plano.Cobertura.Recusadas++;
                if (chave.Length > 0) plano.Cobertura.Vistas.Add(CoberturaDoEstoque.ChaveNatural(item.Recorte, chave));
                continue;
            }

            var natural = CoberturaDoEstoque.ChaveNatural(item.Recorte, chave);
            var hash = Resumir(natural, item.Meses, item.Vendas);
            plano.Cobertura.Planejar(natural, hash, coberturaNoCrm, null);
            plano.CoberturaValida.Add((item.Recorte, chave, competencia, decimal.Round(meses, 2), (int)vendas, hash));
        }

        plano.Cobertura.FecharExclusoes(coberturaNoCrm);
        return plano;
    }

    /// <summary>
    /// O saneamento de uma máquina: os obrigatórios presentes e no tamanho, Sim/Não e NOVO/USADO reconhecidos, e as datas
    /// legíveis. Nulo é formato ilegível. A filial é resolvida depois.
    /// </summary>
    internal static DadosDoEquipamentoEmEstoque? Sanear(EquipamentoNaOrigem l, int empresaId)
    {
        static string Limpo(string? texto) => texto?.Trim() ?? string.Empty;
        static bool Cabe(string texto, int tamanho) => texto.Length <= tamanho;

        var situacao = Limpo(l.Situacao);
        var grupo = Limpo(l.Grupo);
        var descricao = Limpo(l.Descricao);
        var tipo = Limpo(l.Tipo);
        if (situacao.Length == 0 || grupo.Length == 0 || descricao.Length == 0 || tipo.Length == 0
            || !Cabe(situacao, EquipamentoEmEstoque.TamanhoDoTextoCurto) || !Cabe(grupo, EquipamentoEmEstoque.TamanhoDoTextoCurto)
            || !Cabe(descricao, EquipamentoEmEstoque.TamanhoDaDescricao) || !Cabe(tipo, EquipamentoEmEstoque.TamanhoDoTextoCurto))
            return null;

        bool? usado = Limpo(l.NovoOuUsado).ToUpperInvariant() switch { "NOVO" => false, "USADO" => true, _ => null };
        var pago = SimOuNao(l.Pago);
        var reservado = SimOuNao(l.Reservado);
        if (usado is null || pago is null || reservado is null) return null;

        if (!Data(l.EntradaEm, out var entrada) || !Data(l.ChegadaPrevistaEm, out var chegada) || !Data(l.FaturamentoPrevistoEm, out var faturamento))
            return null;

        foreach (var (texto, tamanho) in new[]
                 {
                     (l.Chassi, EquipamentoEmEstoque.TamanhoDoCodigo), (l.Pedido, EquipamentoEmEstoque.TamanhoDoCodigo),
                     (l.Comar, EquipamentoEmEstoque.TamanhoDoCodigo), (l.Configuracao, EquipamentoEmEstoque.TamanhoDaDescricao),
                     (l.Ano, 9), (l.SituacaoNaFabrica, EquipamentoEmEstoque.TamanhoDoTextoCurto)
                 })
            if (!Cabe(Limpo(texto), tamanho)) return null;

        var hash = Resumir(Limpo(l.Chaint), l.Chassi, l.Pedido, l.Comar, l.Filial, situacao, grupo, descricao, l.Configuracao, tipo, l.NovoOuUsado,
            l.Ano, l.EntradaEm, l.ChegadaPrevistaEm, l.FaturamentoPrevistoEm, l.Pago, l.Reservado, l.SituacaoNaFabrica);

        return new DadosDoEquipamentoEmEstoque(
            empresaId, Vazio(l.Chassi), Vazio(l.Pedido), Vazio(l.Comar), situacao, grupo, descricao, Vazio(l.Configuracao), tipo, usado.Value,
            Vazio(l.Ano), entrada, chegada, faturamento, pago.Value, reservado.Value, Vazio(l.SituacaoNaFabrica), hash);
    }

    // =============================================================================================
    // A gravação — uma transação, executando o plano
    // =============================================================================================

    private async Task AplicarAsync(Plano plano, DateTime lidaEm, LeituraDoEstoqueNaOrigem origem, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema, "Gestão de Negócios — API", "API REST com chave, só leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var estoque = (await banco.EquipamentosEmEstoque.IgnoreQueryFilters().Where(e => e.SistemaId == sistemaId).ToListAsync(ct))
            .ToDictionary(e => e.Chaint, StringComparer.Ordinal);
        foreach (var (chaint, dados) in plano.EstoqueValido)
        {
            if (!estoque.TryGetValue(chaint, out var existente))
            {
                banco.EquipamentosEmEstoque.Add(EquipamentoEmEstoque.Registrar(sistemaId, chaint, dados, lidaEm, usuarioId));
                continue;
            }

            existente.Reativar(usuarioId);
            existente.AtualizarDaOrigem(dados, lidaEm, usuarioId);
        }

        foreach (var chave in plano.Estoque.AExcluir) estoque[chave].Excluir(usuarioId);

        var cobertura = (await banco.CoberturasDoEstoque.Where(c => c.SistemaId == sistemaId).ToListAsync(ct))
            .ToDictionary(c => CoberturaDoEstoque.ChaveNatural(c.Recorte, c.Chave), StringComparer.Ordinal);
        foreach (var (recorte, chave, competencia, meses, vendas, hash) in plano.CoberturaValida)
        {
            if (!cobertura.TryGetValue(CoberturaDoEstoque.ChaveNatural(recorte, chave), out var existente))
            {
                banco.CoberturasDoEstoque.Add(CoberturaDoEstoque.Registrar(
                    sistemaId, recorte, chave, competencia, meses, vendas, hash, lidaEm, origem.CoberturaGeradaEmUtc, usuarioId));
                continue;
            }

            existente.Reativar();
            existente.AtualizarDaOrigem(meses, vendas, hash, lidaEm, origem.CoberturaGeradaEmUtc);
        }

        foreach (var chave in plano.Cobertura.AExcluir) cobertura[chave].Excluir(lidaEm);

        await banco.SaveChangesAsync(ct);

        var gerada = (origem.GeradaEmUtc ?? lidaEm).ToString("O", CultureInfo.InvariantCulture);
        var coberturaGerada = (origem.CoberturaGeradaEmUtc ?? lidaEm).ToString("O", CultureInfo.InvariantCulture);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, EquipamentoEmEstoque.FluxoDaCarga, origem.Equipamentos.Count,
            plano.Estoque.Gravadas, plano.Estoque.Recusadas + plano.SemFilial, ct, gerada);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, CoberturaDoEstoque.FluxoDaCarga, origem.Cobertura.Count,
            plano.Cobertura.Gravadas, plano.Cobertura.Recusadas, ct, coberturaGerada);

        await transacao.CommitAsync(ct);
        relatar("Gravado. A sincronia pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
    }

    // =============================================================================================
    // O relatório — só contagens
    // =============================================================================================

    private void Relatar(Plano plano)
    {
        Contar(EtapaDoEstoque, RotuloDeLidas, plano.EstoqueValido.Count + plano.Estoque.Recusadas + plano.SemFilial);
        Contar(EtapaDoEstoque, RotuloSemFilial, plano.SemFilial);
        Contar(EtapaDoEstoque, RotuloNoPatio, plano.EstoqueValido.Count(e => !EquipamentoEmEstoque.EhPedido(e.Dados.Situacao)));
        Contar(EtapaDoEstoque, RotuloDePedidos, plano.EstoqueValido.Count(e => EquipamentoEmEstoque.EhPedido(e.Dados.Situacao)));
        Contar(EtapaDoEstoque, RotuloDeReservadas, plano.EstoqueValido.Count(e => e.Dados.Reservado));
        RelatarSincronia(EtapaDoEstoque, plano.Estoque);

        Contar(EtapaDaCobertura, RotuloDeLidas, plano.CoberturaValida.Count + plano.Cobertura.Recusadas);
        Contar(EtapaDaCobertura, "  meses", plano.CoberturaValida.Count(c => c.Recorte == CoberturaDoEstoque.RecortePorMes));
        Contar(EtapaDaCobertura, "  grupos", plano.CoberturaValida.Count(c => c.Recorte == CoberturaDoEstoque.RecortePorGrupo));
        RelatarSincronia(EtapaDaCobertura, plano.Cobertura);

        if (!plano.EsquemaExiste)
            _observacoes.Add("O banco lido ainda não tem a migração do estoque: o plano parte de nada gravado, que é o que o banco terá logo depois dela.");
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

    private static async Task<bool> EsquemaExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'frota.EquipamentoEmEstoque')")
            .SingleAsync(ct) > 0;

    private static bool? SimOuNao(string? texto) => (texto?.Trim().ToUpperInvariant()) switch
    {
        "SIM" or "S" => true,
        "NÃO" or "NAO" or "N" => false,
        _ => null
    };

    /// <summary>Uma data <c>AAAA-MM-DD</c> (com hora ou não): vazio é nulo; texto que não se lê é formato ilegível.</summary>
    private static bool Data(string? bruto, out DateOnly? data)
    {
        data = null;
        var texto = bruto?.Trim() ?? string.Empty;
        if (texto.Length == 0) return true;
        if (texto.Length < 10 || !DateOnly.TryParseExact(texto[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lida))
            return false;
        data = lida;
        return true;
    }

    private static string? Vazio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static string Resumir(params string?[] partes) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f', partes.Select(p => p ?? string.Empty)))));

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    private sealed record NoCrm(string Chave, string Hash, bool Excluida, int? EmpresaId);

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

        public int Gravadas => Novas + ARevisar + AReativar + AExcluir.Count;

        public void Planejar(string chave, string hash, IReadOnlyDictionary<string, NoCrm> existentes, int? empresaId)
        {
            Vistas.Add(chave);
            if (!existentes.TryGetValue(chave, out var existente))
            {
                Novas++;
                return;
            }

            if (existente.Excluida) AReativar++;
            if (existente.Hash != hash || existente.EmpresaId != empresaId) ARevisar++;
            else if (!existente.Excluida) Iguais++;
        }

        public void FecharExclusoes(IReadOnlyDictionary<string, NoCrm> existentes)
        {
            Vigentes = existentes.Values.Count(e => !e.Excluida);
            AExcluir.AddRange(existentes.Values.Where(e => !e.Excluida && !Vistas.Contains(e.Chave)).Select(e => e.Chave));
        }
    }

    private sealed class Plano
    {
        public bool EsquemaExiste { get; init; }
        public Sincronia Estoque { get; } = new();
        public Sincronia Cobertura { get; } = new();
        public int SemFilial { get; set; }
        public List<(string Chaint, DadosDoEquipamentoEmEstoque Dados)> EstoqueValido { get; } = [];
        public List<(string Recorte, string Chave, DateOnly? Competencia, decimal Meses, int Vendas, string Hash)> CoberturaValida { get; } = [];
    }
}
