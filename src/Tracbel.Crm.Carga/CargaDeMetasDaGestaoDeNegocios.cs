using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.GestaoDeNegocios;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga das metas fez (ou faria, na simulação), em número — sem nome de consultor.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDasMetasDaGestaoDeNegocios(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// AS METAS DE VENDA DA API GESTÃO DE NEGÓCIOS (decisão de 27/09/2026, #138) — <c>--somente-metas-gn</c>, a rotina
/// <c>METAS_GESTAO_NEGOCIOS</c> do orquestrador.
///
/// <para><b>É SINCRONIA de um cadastro de outra equipe.</b> Cada linha da GN vira uma <see cref="MetaDeVenda"/>, pelo
/// <c>id</c> da origem: a nova entra, a revisada muda (e a trilha guarda o antes e o depois), a que sumiu é EXCLUÍDA
/// logicamente, e a que volta é reativada na mesma linha. A segunda leitura igual grava zero.</para>
///
/// <para><b>Ler tudo, conferir, e só então gravar.</b> O cliente só devolve a leitura que fecha (total estável, linhas
/// somando o total); o leitor recusa campo ausente e id repetido; o saneamento recusa a linha sem mês, filial, linha,
/// consultor, tipo, origem ou quantidade — e a linha recusada não apaga a meta que já existia: ela fica como estava, e a
/// recusa vai para a fila de descarte com o id e o motivo, sem nome.</para>
///
/// <para><b>A trava de remoção ABORTA A CARGA INTEIRA.</b> Uma rodada que excluiria mais de
/// <see cref="FracaoMaximaDeRemocao"/> das metas vigentes (com ao menos <see cref="JanelaMinima"/> delas) é leitura que
/// veio pela metade, ou a GN renumerando os ids — não o comercial refazendo a campanha numa madrugada. Nada é gravado, e o
/// motivo fica no relatório. <c>--aceitar-remocao</c> passa por cima; ele existe SÓ no terminal — a rotina do orquestrador
/// não o tem nos modos, e uma remoção em massa precisa de alguém olhando.</para>
///
/// <para><b>Casar com o CRM</b>: a filial pelo código <c>0101NN</c>; a linha pela classificação do ART (CONSÓRCIO e USADOS
/// ficam sem classificação, que não são categoria); o consultor pela conta cujo login — a parte antes do <c>@</c> — é o
/// consultor em minúsculas, e só quando há UMA conta ativa assim.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerMetas">A leitura do cadastro de metas.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDeMetasDaGestaoDeNegocios(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<LeituraDasMetas>>> lerMetas,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>O fluxo: a trava da carga, o ponto de sincronismo (o frescor) e a fila de descarte.</summary>
    internal const string Fluxo = "GESTAO_NEGOCIOS.METAS";

    /// <summary>A maior fração das metas vigentes que uma rodada pode excluir sem abortar.</summary>
    internal const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>Abaixo disto a trava não se aplica: num cadastro pequeno (um teste, o começo), poucas saídas já passam de 20%.</summary>
    internal const int JanelaMinima = 100;

    /// <summary>Rótulo: linhas lidas.</summary>
    internal const string RotuloDeLidas = "linhas lidas do cadastro de metas";

    /// <summary>Rótulo: linhas recusadas.</summary>
    internal const string RotuloDeRecusadas = "linhas recusadas (ficam na fila de descarte, com o id e o motivo)";

    /// <summary>Rótulo: metas novas.</summary>
    internal const string RotuloDeNovas = "metas novas";

    /// <summary>Rótulo: metas revisadas.</summary>
    internal const string RotuloDeRevisadas = "metas revisadas pela origem (a trilha guarda o antes e o depois)";

    /// <summary>Rótulo: metas iguais.</summary>
    internal const string RotuloDeIguais = "metas sem mudança";

    /// <summary>Rótulo: metas excluídas.</summary>
    internal const string RotuloDeExcluidas = "metas que sumiram da origem (excluídas, nunca apagadas)";

    /// <summary>Rótulo: metas reativadas.</summary>
    internal const string RotuloDeReativadas = "metas que voltaram à origem (reativadas na mesma linha)";

    /// <summary>Rótulo: consultores sem conta.</summary>
    internal const string RotuloDeConsultoresSemConta = "consultores sem conta no CRM (só aparecem na visão da filial)";

    /// <summary>Se a rodada excluiria demais — leitura parcial.</summary>
    /// <param name="vigentes">As metas vigentes antes da rodada.</param>
    /// <param name="aExcluir">As que a rodada excluiria.</param>
    internal static bool RemocaoPassaDaTrava(int vigentes, int aExcluir) =>
        vigentes >= JanelaMinima && aExcluir > vigentes * FracaoMaximaDeRemocao;

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta: não abre transação de escrita.</param>
    /// <param name="aceitarRemocao">Passa por cima da trava de remoção — só no terminal.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDasMetasDaGestaoDeNegocios>> ExecutarAsync(bool simular, bool aceitarRemocao, CancellationToken ct)
    {
        relatar($"Lendo o cadastro de metas da API Gestão de Negócios ({LeitorDeMetasDaGestaoDeNegocios.Rota}, só GET)…");
        var lida = await lerMetas(ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDasMetasDaGestaoDeNegocios>.Indisponivel(lida.Erro!);

        // O CADASTRO VAZIO NÃO É "NINGUÉM TEM META": é uma leitura a conferir. Sincronizar contra ele excluiria todas.
        if (lida.Valor.Linhas.Count == 0)
            return Resultado<RelatorioDasMetasDaGestaoDeNegocios>.Indisponivel(
                "O cadastro de metas veio vazio. Isso não é um ano sem metas, é uma leitura a conferir — nada foi excluído e nada foi gravado.");

        var agora = relogio();
        var leitura = new LeituraDaMeta(agora, lida.Valor.GeradaEmUtc, lida.Valor.IdadeSegundos);

        Plano plano;
        await using (var leituraDoCrm = abrirContexto())
        {
            plano = await PlanejarAsync(leituraDoCrm, lida.Valor, ct);
        }

        Relatar(plano);

        if (RemocaoPassaDaTrava(plano.Vigentes, plano.AExcluir.Count))
        {
            var texto = string.Create(CultureInfo.InvariantCulture,
                $"A rodada excluiria {plano.AExcluir.Count} de {plano.Vigentes} metas vigentes (acima de {FracaoMaximaDeRemocao:P0}). " +
                $"Isso é leitura parcial, ou a GN renumerou os ids — não o comercial refazendo a campanha numa madrugada.");
            if (!aceitarRemocao)
                return Resultado<RelatorioDasMetasDaGestaoDeNegocios>.Indisponivel(
                    texto + " A CARGA INTEIRA FOI ABORTADA: nada foi gravado. Confira a API; se a remoção for mesmo o que aconteceu, " +
                    "rode no terminal com --aceitar-remocao.");

            _observacoes.Add(texto + " Aceita por --aceitar-remocao, no terminal.");
        }

        if (!plano.EsquemaDasMetas && !simular)
            return Resultado<RelatorioDasMetasDaGestaoDeNegocios>.Indisponivel(
                "O banco ainda não tem a migração MetasDaGestaoDeNegocios. Publique a versão com ela antes de ligar a rotina — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura, e nenhuma transação de escrita foi aberta.");
            return Resultado<RelatorioDasMetasDaGestaoDeNegocios>.Ok(new RelatorioDasMetasDaGestaoDeNegocios(true, _contagens, _observacoes));
        }

        await AplicarAsync(plano, leitura, lida.Valor, ct);
        return Resultado<RelatorioDasMetasDaGestaoDeNegocios>.Ok(new RelatorioDasMetasDaGestaoDeNegocios(false, _contagens, _observacoes));
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private static async Task<Plano> PlanejarAsync(CrmDbContext banco, LeituraDasMetas lida, CancellationToken ct)
    {
        var plano = new Plano { EsquemaDasMetas = await EsquemaDasMetasExisteAsync(banco, ct) };

        var empresaPorCodigo = await banco.Empresas.AsNoTracking()
            .ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);
        var linhaDeProdutoPorCodigo = await banco.LinhasDeProduto.AsNoTracking()
            .ToDictionaryAsync(l => l.Codigo, l => l.Id, StringComparer.Ordinal, ct);

        // A CONTA DO CONSULTOR: o login é a parte antes do @ do nome principal — o mesmo casamento das carteiras do Vórtice
        // e o do Entra ID no primeiro login. Só a conta ativa, e só quando é uma.
        var contasPorLogin = (await banco.Usuarios.AsNoTracking()
                .Where(u => u.ExcluidoEm == null)
                .Select(u => new { u.Id, u.NomePrincipal })
                .ToListAsync(ct))
            .GroupBy(u => ParteLocal(u.NomePrincipal), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(u => u.Id).ToList(), StringComparer.OrdinalIgnoreCase);

        plano.SistemaId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);

        var existentes = plano.EsquemaDasMetas && plano.SistemaId is { } sistema
            ? await banco.MetasDeVenda.AsNoTracking()
                .Where(m => m.SistemaId == sistema)
                .Select(m => new MetaNoCrm(m.IdNaOrigem, m.HashDaOrigem, m.ExcluidoEm != null, m.EmpresaId, m.LinhaDeProdutoId, m.ConsultorUsuarioId))
                .ToDictionaryAsync(m => m.IdNaOrigem, ct)
            : [];
        plano.Vigentes = existentes.Values.Count(m => !m.Excluida);

        var vistos = new HashSet<int>();
        foreach (var linha in lida.Linhas)
        {
            vistos.Add(linha.Id);
            var saneamento = SaneamentoDasMetas.Sanear(linha);
            var motivos = saneamento.Motivos.ToList();

            int? empresaId = null;
            if (saneamento.Meta is { } s)
            {
                if (empresaPorCodigo.TryGetValue(s.FilialCodigo, out var empresa)) empresaId = empresa;
                else motivos.Add(MotivoDeRecusaDaMeta.FilialSemEmpresaNoCrm);
            }

            if (motivos.Count > 0)
            {
                plano.Recusadas.Add((linha.Id, motivos));
                continue;
            }

            var meta = saneamento.Meta!;
            plano.Transformacoes += meta.Transformacoes.Count;

            var classificacao = ClassificacaoDoArt.ClassificarLinha(meta.LinhaNaOrigem);
            int? linhaDeProdutoId = classificacao.Situacao == SituacaoDaCorrespondencia.CorrespondenciaExata
                                    && linhaDeProdutoPorCodigo.TryGetValue(classificacao.Destino!, out var classe)
                ? classe
                : null;

            long? consultorUsuarioId = contasPorLogin.TryGetValue(meta.ConsultorNaOrigem.ToLowerInvariant(), out var contas) && contas.Count == 1
                ? contas[0]
                : null;

            var dados = new DadosDaMetaNaOrigem(
                empresaId!.Value, meta.Competencia, meta.LinhaNaOrigem, meta.CodigoDaLinha, linhaDeProdutoId, meta.ConsultorNaOrigem,
                consultorUsuarioId, meta.VendaDireta, meta.Origem, meta.Quantidade, meta.ValorUnitario, meta.Margem, meta.Hash);
            plano.Validas.Add((meta, dados));

            if (!existentes.TryGetValue(linha.Id, out var existente)) plano.Novas.Add(linha.Id);
            else
            {
                if (existente.Excluida) plano.AReativar.Add(linha.Id);
                if (existente.Hash != meta.Hash || existente.EmpresaId != dados.EmpresaId
                    || existente.LinhaDeProdutoId != dados.LinhaDeProdutoId || existente.ConsultorUsuarioId != dados.ConsultorUsuarioId)
                    plano.ARevisar.Add(linha.Id);
                else if (!existente.Excluida) plano.Iguais++;
            }
        }

        // A META QUE SUMIU DA ORIGEM — a vigente cujo id não veio. A que veio e foi recusada NÃO sai: ela fica como estava.
        plano.AExcluir.AddRange(existentes.Values.Where(m => !m.Excluida && !vistos.Contains(m.IdNaOrigem)).Select(m => m.IdNaOrigem));
        return plano;
    }

    // =============================================================================================
    // A gravação — uma transação, executando o plano
    // =============================================================================================

    private async Task AplicarAsync(Plano plano, LeituraDaMeta leitura, LeituraDasMetas lida, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema, "Gestão de Negócios — API", "API REST com chave, só leitura", ct);

        // A REVISÃO, A EXCLUSÃO E A VOLTA vão para a trilha como integração da GN; o que nasce agora tem o rastro no id da
        // origem gravado na própria linha.
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var rastreadas = await banco.MetasDeVenda.Where(m => m.SistemaId == sistemaId).ToDictionaryAsync(m => m.IdNaOrigem, ct);

        foreach (var (meta, dados) in plano.Validas)
        {
            if (!rastreadas.TryGetValue(meta.IdNaOrigem, out var existente))
            {
                banco.MetasDeVenda.Add(MetaDeVenda.Registrar(sistemaId, meta.IdNaOrigem, dados, leitura, usuarioId));
                continue;
            }

            existente.Reativar(usuarioId);
            existente.AtualizarDaOrigem(dados, leitura, usuarioId);
        }

        foreach (var id in plano.AExcluir) rastreadas[id].Excluir(usuarioId);

        await banco.SaveChangesAsync(ct);

        // AS RECUSAS DA RODADA substituem as da anterior — sem nome: o id da GN e os motivos bastam para achar a linha lá.
        await CargaDeTerritorio.SubstituirRecusasAsync(banco, Fluxo,
            [.. plano.Recusadas.Select(r => ((object)new { IdNaOrigem = r.Id, Motivos = string.Join(",", r.Motivos) }, string.Join(",", r.Motivos)))],
            ct);

        // O FRESCOR: quando o CRM leu e quando a GN gerou — é o que a tela mostra ao lado da meta.
        await CargaDeTerritorio.RegistrarRodadaAsync(
            banco, sistemaId, Fluxo, lida.Linhas.Count, plano.Novas.Count + plano.ARevisar.Count + plano.AReativar.Count + plano.AExcluir.Count,
            plano.Recusadas.Count, ct, (lida.GeradaEmUtc ?? leitura.LidaEm).ToString("O", CultureInfo.InvariantCulture));

        await transacao.CommitAsync(ct);
        relatar("Gravado. A sincronia pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
    }

    // =============================================================================================
    // O relatório — só contagens: nenhum nome de consultor sai daqui
    // =============================================================================================

    private void Relatar(Plano plano)
    {
        const string etapaDaLeitura = "1. Leitura do cadastro de metas";
        var lidas = plano.Validas.Count + plano.Recusadas.Count;
        Contar(etapaDaLeitura, RotuloDeLidas, lidas);
        Contar(etapaDaLeitura, RotuloDeRecusadas, plano.Recusadas.Count);
        foreach (var motivo in plano.Recusadas.SelectMany(r => r.Motivos).GroupBy(m => m, StringComparer.Ordinal).OrderByDescending(g => g.Count()))
            Contar(etapaDaLeitura, $"  recusa: {motivo.Key}", motivo.Count());
        Contar(etapaDaLeitura, "valor unitário ou margem ilegível (a linha entra, o valor fica vazio)", plano.Transformacoes);

        const string etapaDoConteudo = "2. O que o cadastro diz";
        var maquinas = plano.Validas.Where(v => !v.Meta.EhConsorcio).ToList();
        var consorcio = plano.Validas.Where(v => v.Meta.EhConsorcio).ToList();
        Contar(etapaDoConteudo, "linhas de meta de máquinas", maquinas.Count);
        Contar(etapaDoConteudo, "  unidades de meta de máquinas", maquinas.Sum(v => v.Meta.Quantidade));
        Contar(etapaDoConteudo, "linhas de meta de consórcio (à parte, em cotas — D-M4)", consorcio.Count);
        Contar(etapaDoConteudo, "  cotas de meta de consórcio", consorcio.Sum(v => v.Meta.Quantidade));
        foreach (var ano in maquinas.GroupBy(v => AnoFiscalDe(v.Meta.Competencia)).OrderBy(g => g.Key))
            Contar(etapaDoConteudo, $"  unidades de máquinas no FY{ano.Key}", ano.Sum(v => v.Meta.Quantidade));
        Contar(etapaDoConteudo, "linhas sem classificação de produto no CRM (CONSÓRCIO, USADOS ou linha nova)",
            plano.Validas.Count(v => v.Dados.LinhaDeProdutoId is null));
        Contar(etapaDoConteudo, "filiais com meta", plano.Validas.Select(v => v.Dados.EmpresaId).Distinct().Count());

        const string etapaDosConsultores = "3. Consultores";
        var consultores = plano.Validas.GroupBy(v => v.Meta.ConsultorNaOrigem, StringComparer.Ordinal).ToList();
        Contar(etapaDosConsultores, "consultores distintos", consultores.Count);
        Contar(etapaDosConsultores, "consultores com conta no CRM (a meta aparece para eles)", consultores.Count(g => g.First().Dados.ConsultorUsuarioId is not null));
        Contar(etapaDosConsultores, RotuloDeConsultoresSemConta, consultores.Count(g => g.First().Dados.ConsultorUsuarioId is null));
        Contar(etapaDosConsultores, "  linhas desses consultores", plano.Validas.Count(v => v.Dados.ConsultorUsuarioId is null));

        const string etapaDaSincronia = "4. Sincronia com organizacao.MetaDeVenda";
        Contar(etapaDaSincronia, "metas vigentes antes da rodada", plano.Vigentes);
        Contar(etapaDaSincronia, RotuloDeNovas, plano.Novas.Count);
        Contar(etapaDaSincronia, RotuloDeRevisadas, plano.ARevisar.Count);
        Contar(etapaDaSincronia, RotuloDeIguais, plano.Iguais);
        Contar(etapaDaSincronia, RotuloDeReativadas, plano.AReativar.Count);
        Contar(etapaDaSincronia, RotuloDeExcluidas, plano.AExcluir.Count);

        if (!plano.EsquemaDasMetas)
            _observacoes.Add("O banco lido ainda não tem a migração MetasDaGestaoDeNegocios: o plano parte de nenhuma meta gravada, que é o que o " +
                             "banco terá logo depois da migração.");
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// SE O BANCO JÁ TEM A TABELA DAS METAS. A simulação roda da estação contra a produção ANTES da publicação que traz a
    /// migração — é assim que se mede o que a rotina fará. No SQLite dos testes o modelo é sempre o de hoje.
    /// </summary>
    private static async Task<bool> EsquemaDasMetasExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'organizacao.MetaDeVenda')")
            .SingleAsync(ct) > 0;

    /// <summary>O ano fiscal de novembro a outubro, com o nome do ano em que termina — só para o relatório.</summary>
    private static int AnoFiscalDe(DateOnly competencia) => competencia.Month >= 11 ? competencia.Year + 1 : competencia.Year;

    /// <summary>O login da conta: a parte antes do <c>@</c> do nome principal.</summary>
    private static string ParteLocal(string nomePrincipal)
    {
        var arroba = nomePrincipal.IndexOf('@', StringComparison.Ordinal);
        return (arroba < 0 ? nomePrincipal : nomePrincipal[..arroba]).Trim().ToLowerInvariant();
    }

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    private sealed record MetaNoCrm(int IdNaOrigem, string Hash, bool Excluida, int EmpresaId, int? LinhaDeProdutoId, long? ConsultorUsuarioId);

    private sealed class Plano
    {
        public bool EsquemaDasMetas { get; init; }

        public int? SistemaId { get; set; }

        public int Vigentes { get; set; }

        public int Transformacoes { get; set; }

        public int Iguais { get; set; }

        public List<(MetaSaneada Meta, DadosDaMetaNaOrigem Dados)> Validas { get; } = [];

        public List<(int Id, IReadOnlyList<string> Motivos)> Recusadas { get; } = [];

        public List<int> Novas { get; } = [];

        public List<int> ARevisar { get; } = [];

        public List<int> AReativar { get; } = [];

        public List<int> AExcluir { get; } = [];
    }
}
