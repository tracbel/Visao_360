using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A SIMULAÇÃO DO FATURAMENTO — <c>--somente-faturamento --simular</c>: lê o CRM e o Protheus e não grava nada. Mora num
/// arquivo próprio desde o plano 3 do documento 54, para a carga caber no teto de tamanho.
/// </summary>
internal sealed partial class CargaDeFaturamentoDoProtheus
{
    // =============================================================================================
    // A SIMULAÇÃO — lê o CRM e o Protheus, e não grava nada
    // =============================================================================================

    /// <summary>
    /// <c>--somente-faturamento --simular</c>: o que a carga gravaria, sem gravar.
    ///
    /// <para><b>Nenhuma transação é aberta e nenhum <c>SaveChanges</c> é chamado</b> — não é a
    /// transação desfeita das outras simulações, é leitura pura, porque ela roda contra o banco de
    /// produção a partir de uma estação. O sistema de origem também não é criado.</para>
    ///
    /// <para><b>Duas visões.</b> "CRM de hoje" casa a venda com os clientes que o banco tem agora.
    /// "CRM + carga da SA1" casa também com os clientes que <c>--somente-clientes-protheus</c> criaria
    /// — que é o que importa enquanto a produção tiver o cadastro vazio.</para>
    /// </summary>
    /// <param name="cadastroDaSa1">A leitura da SA1, para a segunda visão; nula dispensa a visão.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<SimulacaoDoFaturamento>> SimularAsync(
        LeitorDeClientesDoProtheus? cadastroDaSa1, CancellationToken ct)
    {
        relatar("Lendo o faturamento direto do banco do Protheus — três anos (SIMULAÇÃO: nada será gravado)…");

        var lido = await protheus.LerAsync(InicioDaJanela(DateTime.UtcNow), relatar, ct);
        if (!lido.EhSucesso) return Resultado<SimulacaoDoFaturamento>.Indisponivel(lido.Erro!);

        IReadOnlySet<string>? daSa1 = null;
        if (cadastroDaSa1 is not null)
        {
            relatar("Lendo a SA1 para projetar os clientes que a carga de clientes criaria…");
            var projetados = await DocumentosQueACargaDaSa1CriariaAsync(cadastroDaSa1, ct);
            if (!projetados.EhSucesso) relatar("  a SA1 não foi lida: " + projetados.Erro + " — só a visão do CRM de hoje.");
            else daSa1 = projetados.Valor;
        }

        return Resultado<SimulacaoDoFaturamento>.Ok(await SimularAsync(lido.Valor, daSa1, ct));
    }

    /// <summary>A simulação sobre um lote já lido — é o que o teste exercita.</summary>
    /// <param name="lote">O lote.</param>
    /// <param name="documentosDaSa1">Os documentos que a carga da SA1 criaria; nulo dispensa a segunda visão.</param>
    /// <param name="ct">Cancelamento.</param>
    internal async Task<SimulacaoDoFaturamento> SimularAsync(
        LoteDoProtheus lote, IReadOnlySet<string>? documentosDaSa1, CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var clientes = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Documento != null)
            .Select(c => new { c.Id, Documento = c.Documento!.Value.Numero, c.Situacao })
            .ToListAsync(ct);

        var porDocumento = new Dictionary<string, long>(StringComparer.Ordinal);
        var situacaoPorId = new Dictionary<long, SituacaoDoCliente>();
        foreach (var cliente in clientes)
        {
            porDocumento.TryAdd(cliente.Documento, cliente.Id);
            situacaoPorId[cliente.Id] = cliente.Situacao;
        }

        var totalDeClientes = await contexto.Clientes.AsNoTracking().CountAsync(c => c.ExcluidoEm == null, ct);

        var empresas = await contexto.Empresas.AsNoTracking()
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToListAsync(ct);
        var porCodigoDeEmpresa = empresas.ToDictionary(e => e.Codigo, e => e.Id, StringComparer.Ordinal);
        var nomeDaEmpresa = empresas.ToDictionary(e => e.Id, e => e.Nome);

        // O QUE O BANCO TEM HOJE NA JANELA, para dizer quanto seria incluído, reapurado e removido.
        var existentesComCliente = (await contexto.FaturamentoDosClientes.AsNoTracking()
                .Where(f => f.Competencia >= lote.Desde)
                .Select(f => new { f.ClienteId, f.EmpresaId, f.Competencia })
                .ToListAsync(ct))
            .Select(f => (f.ClienteId, f.EmpresaId, f.Competencia))
            .ToHashSet();
        var existentesSemCliente = (await contexto.FaturamentoSemClientes.AsNoTracking()
                .Where(f => f.Competencia >= lote.Desde)
                .Select(f => new { f.Documento, f.EmpresaId, f.Competencia })
                .ToListAsync(ct))
            .Select(f => (f.Documento, f.EmpresaId, f.Competencia))
            .ToHashSet();

        var hoje = Visao("CRM de hoje", lote, porDocumento, situacaoPorId, totalDeClientes, porCodigoDeEmpresa, nomeDaEmpresa);

        // A DIFERENÇA CONTRA O BANCO é da visão de hoje: é o que a carga faria se rodasse agora.
        var produzidosComCliente = new HashSet<(long, int, DateOnly)>();
        var produzidosSemCliente = new HashSet<(string, int, DateOnly)>();
        foreach (var linha in lote.Faturamento)
        {
            var (clienteId, empresaId) = Resolver(linha, porDocumento, porCodigoDeEmpresa);
            if (empresaId == 0) continue;
            if (clienteId != 0) produzidosComCliente.Add((clienteId, empresaId, linha.Competencia));
            else produzidosSemCliente.Add((linha.DocumentoDoCliente, empresaId, linha.Competencia));
        }

        var reapuraria = produzidosComCliente.Count(existentesComCliente.Contains) + produzidosSemCliente.Count(existentesSemCliente.Contains);
        var incluiria = produzidosComCliente.Count + produzidosSemCliente.Count - reapuraria;
        var removeria = existentesComCliente.Count(k => !produzidosComCliente.Contains(k)) + existentesSemCliente.Count(k => !produzidosSemCliente.Contains(k));

        VisaoDaSimulacao? comSa1 = null;
        if (documentosDaSa1 is not null)
        {
            // OS CLIENTES QUE A SA1 CRIARIA ganham um identificador provisório, negativo, só para a
            // curva: nascem Suspect, que é o que a carga de clientes grava.
            var ampliado = new Dictionary<string, long>(porDocumento, StringComparer.Ordinal);
            var situacoes = new Dictionary<long, SituacaoDoCliente>(situacaoPorId);
            long provisorio = 0;
            foreach (var documento in documentosDaSa1.Order(StringComparer.Ordinal))
            {
                if (ampliado.ContainsKey(documento)) continue;
                ampliado[documento] = --provisorio;
                situacoes[provisorio] = SituacaoDoCliente.Suspect;
            }

            comSa1 = Visao("CRM + carga da SA1", lote, ampliado, situacoes, totalDeClientes - (int)provisorio,
                porCodigoDeEmpresa, nomeDaEmpresa);
        }

        return new SimulacaoDoFaturamento(lote, hoje, comSa1, incluiria, reapuraria, removeria);
    }

    /// <summary>Uma visão da simulação: o lote resolvido contra um cadastro de clientes.</summary>
    private static VisaoDaSimulacao Visao(
        string nome,
        LoteDoProtheus lote,
        IReadOnlyDictionary<string, long> porDocumento,
        IReadOnlyDictionary<long, SituacaoDoCliente> situacaoPorId,
        int totalDeClientes,
        IReadOnlyDictionary<string, int> porCodigoDeEmpresa,
        IReadOnlyDictionary<int, string> nomeDaEmpresa)
    {
        var porMes = new SortedDictionary<DateOnly, Acumulado>();
        var porFilial = new SortedDictionary<string, Acumulado>(StringComparer.Ordinal);
        var comCliente = new List<(long, int, decimal)>();
        var documentosComCliente = new HashSet<string>(StringComparer.Ordinal);
        var documentosSemCliente = new HashSet<string>(StringComparer.Ordinal);
        int linhasComCliente = 0, linhasSemCliente = 0, linhasSemFilial = 0;

        foreach (var linha in lote.Faturamento)
        {
            var (clienteId, empresaId) = Resolver(linha, porDocumento, porCodigoDeEmpresa);
            var codigo = linha.CodigoDaFilial ?? "(sem filial)";
            var rotuloDaFilial = empresaId != 0 && nomeDaEmpresa.TryGetValue(empresaId, out var n) ? $"{codigo} {n}" : $"{codigo} (sem empresa no CRM)";

            foreach (var acumulado in new[] { Obter(porMes, linha.Competencia), Obter(porFilial, rotuloDaFilial) })
            {
                acumulado.Linhas++;
                acumulado.Total += linha.ValorLiquido;
                acumulado.Maquina += linha.Quebra.Maquina;
                if (empresaId == 0) acumulado.SemFilial += linha.ValorLiquido;
                else if (clienteId != 0) acumulado.ComCliente += linha.ValorLiquido;
                else acumulado.SemCliente += linha.ValorLiquido;
            }

            if (empresaId == 0) { linhasSemFilial++; continue; }

            if (clienteId != 0)
            {
                linhasComCliente++;
                documentosComCliente.Add(linha.DocumentoDoCliente);
                comCliente.Add((clienteId, empresaId, linha.ValorLiquido));
            }
            else
            {
                linhasSemCliente++;
                documentosSemCliente.Add(linha.DocumentoDoCliente);
            }
        }

        var curva = ClassificarCurva(comCliente);
        var contagem = curva.Values.GroupBy(v => v.Classe).ToDictionary(g => g.Key, g => g.Count());
        contagem[ClasseDeCliente.D] = Math.Max(0, totalDeClientes - curva.Count);

        var promovidos = curva.Keys.Count(id => situacaoPorId.TryGetValue(id, out var s) && PromoveAoFaturar(s));

        return new VisaoDaSimulacao(
            nome,
            [.. porMes.Select(p => p.Value.Linha(p.Key.ToString("yyyy-MM", CultureInfo.InvariantCulture)))],
            [.. porFilial.Select(p => p.Value.Linha(p.Key))],
            linhasComCliente, linhasSemCliente, linhasSemFilial,
            documentosComCliente.Count, documentosSemCliente.Count, promovidos, contagem);

        static Acumulado Obter<TChave>(SortedDictionary<TChave, Acumulado> onde, TChave chave) where TChave : notnull
        {
            if (!onde.TryGetValue(chave, out var acumulado)) onde[chave] = acumulado = new Acumulado();
            return acumulado;
        }
    }

    private sealed class Acumulado
    {
        public int Linhas;
        public decimal Total;
        public decimal Maquina;
        public decimal ComCliente;
        public decimal SemCliente;
        public decimal SemFilial;

        public LinhaDaSimulacao Linha(string chave) => new(chave, Linhas, Total, Maquina, ComCliente, SemCliente, SemFilial);
    }

    /// <summary>
    /// Os documentos que <c>--somente-clientes-protheus</c> transformaria em cliente — só para a simulação.
    ///
    /// <para><b>A mesma sequência de <see cref="CargaDeClientesDoProtheus"/></b>, na mesma ordem: loja
    /// principal por documento (<see cref="CargaDeClientesDoProtheus.EscolherAPrincipal"/>, a mesma
    /// função), dígito verificador, UF conhecida, município no catálogo e filial responsável na área de
    /// atuação. Aquela carga só simula dentro de uma transação desfeita — que grava e desfaz —, e esta
    /// simulação roda contra a produção sem abrir transação nenhuma; por isso a projeção é refeita
    /// aqui, em leitura pura. Se a regra de lá mudar, esta visão precisa acompanhar.</para>
    /// </summary>
    private async Task<Resultado<IReadOnlySet<string>>> DocumentosQueACargaDaSa1CriariaAsync(
        LeitorDeClientesDoProtheus cadastroDaSa1, CancellationToken ct)
    {
        var lido = await cadastroDaSa1.LerAsync(ct);
        if (!lido.EhSucesso) return Resultado<IReadOnlySet<string>>.Indisponivel(lido.Erro!);

        await using var contexto = abrirContexto();

        var municipios = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge != null)
            .Select(m => new { m.Id, CodigoIbge = m.CodigoIbge!.Value, m.Uf })
            .ToListAsync(ct);

        var municipioPorIbge = municipios.ToDictionary(m => m.CodigoIbge, m => m.Id);
        var prefixoDaUf = municipios
            .GroupBy(m => m.Uf, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().CodigoIbge / 100_000, StringComparer.OrdinalIgnoreCase);

        var comFilial = (await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                .Where(a => a.EncerradoEm == null && a.EmpresaResponsavelId != null)
                .Select(a => a.MunicipioId)
                .ToListAsync(ct))
            .ToHashSet();

        var documentos = new HashSet<string>(StringComparer.Ordinal);
        foreach (var grupo in lido.Valor.GroupBy(l => l.Documento, StringComparer.Ordinal))
        {
            var loja = CargaDeClientesDoProtheus.EscolherAPrincipal(grupo);

            if (!CpfCnpj.TentarCriar(grupo.Key, out _)) continue;
            if (string.IsNullOrWhiteSpace(loja.Uf) || !prefixoDaUf.TryGetValue(loja.Uf, out var prefixo)) continue;

            var codigoIbge = LeitorDeClientesDoProtheus.CodigoIbge(prefixo, loja.CodigoDoMunicipio);
            if (codigoIbge is null || !municipioPorIbge.TryGetValue(codigoIbge.Value, out var municipioId)) continue;
            if (!comFilial.Contains(municipioId)) continue;

            documentos.Add(grupo.Key);
        }

        relatar($"  {documentos.Count:N0} documento(s) da SA1 virariam cliente (área de atuação).");
        return Resultado<IReadOnlySet<string>>.Ok(documentos);
    }

    /// <summary>
    /// Imprime a simulação: por mês, por filial, e o que mudaria no banco.
    /// </summary>
    /// <param name="simulacao">A simulação.</param>
    /// <param name="escrever">Onde escrever.</param>
    internal static void Imprimir(SimulacaoDoFaturamento simulacao, Action<string> escrever)
    {
        var lote = simulacao.Lote;
        escrever(string.Empty);
        escrever($"Janela lida: desde {lote.Desde:dd/MM/yyyy} · nota mais recente {lote.EmissaoMaisRecente:dd/MM/yyyy} · " +
                 $"{lote.Faturamento.Count:N0} meses de faturamento por documento e filial · " +
                 $"filiais nas notas: {string.Join(", ", lote.FiliaisVistas)}.");
        escrever($"Fora da venda: {lote.ItensQueNaoSaoVenda:N0} itens que não são venda ({Reais(lote.ValorQueNaoEhVenda)}); " +
                 $"{lote.CodigosSemDocumento:N0} códigos sem documento na SA1 ({Reais(lote.ValorSemDocumento)}).");
        escrever($"No banco, se a carga rodasse agora (visão de hoje): incluiria {simulacao.Incluiria:N0}, reapuraria " +
                 $"{simulacao.Reapuraria:N0} e removeria {simulacao.Removeria:N0} linha(s).");

        foreach (var visao in new[] { simulacao.Hoje, simulacao.ComSa1 }.OfType<VisaoDaSimulacao>())
        {
            escrever(string.Empty);
            escrever($"=== {visao.Nome} ===");
            escrever($"  {visao.LinhasComCliente:N0} linha(s) em FaturamentoDoCliente ({visao.DocumentosComCliente:N0} documentos) · " +
                     $"{visao.LinhasSemCliente:N0} em FaturamentoSemCliente ({visao.DocumentosSemCliente:N0} documentos) · " +
                     $"{visao.LinhasSemFilial:N0} descartada(s) sem empresa.");
            escrever("  curva ABC: " + string.Join(" · ", visao.Curva.OrderBy(p => p.Key).Select(p => $"{p.Key} {p.Value:N0}")) +
                     $" · {visao.ClientesPromovidos:N0} promovido(s) a Cliente.");

            escrever(string.Empty);
            escrever($"  {"competência",-12}{"linhas",8}{"total R$ mi",14}{"máquina",10}{"c/ cliente",12}{"s/ cliente",12}{"s/ filial",11}");
            foreach (var linha in visao.PorMes) escrever("  " + Formatar(linha, 12));

            escrever(string.Empty);
            escrever($"  {"filial",-44}{"linhas",8}{"total R$ mi",14}{"máquina",10}{"c/ cliente",12}{"s/ cliente",12}{"s/ filial",11}");
            foreach (var linha in visao.PorFilial) escrever("  " + Formatar(linha, 44));
        }

        static string Formatar(LinhaDaSimulacao l, int largura) =>
            $"{l.Chave.PadRight(largura)}{l.Linhas,8:N0}{Mi(l.Total),14}{Mi(l.Maquina),10}{Mi(l.ComCliente),12}{Mi(l.SemCliente),12}{Mi(l.SemFilial),11}";

        static string Mi(decimal valor) => (valor / 1_000_000m).ToString("N2", CulturaDoRelatorio);
    }
}

/// <summary>Uma linha da simulação — um mês ou uma filial.</summary>
/// <param name="Chave">A competência (<c>yyyy-MM</c>) ou a filial.</param>
/// <param name="Linhas">Meses de faturamento por documento.</param>
/// <param name="Total">O valor.</param>
/// <param name="Maquina">Quanto foi máquina.</param>
/// <param name="ComCliente">Quanto casou com cliente do CRM.</param>
/// <param name="SemCliente">Quanto iria para <c>FaturamentoSemCliente</c>.</param>
/// <param name="SemFilial">Quanto seria descartado por não ter empresa no CRM.</param>
internal sealed record LinhaDaSimulacao(
    string Chave, int Linhas, decimal Total, decimal Maquina, decimal ComCliente, decimal SemCliente, decimal SemFilial);

/// <summary>Uma visão da simulação: o lote casado contra um cadastro de clientes.</summary>
internal sealed record VisaoDaSimulacao(
    string Nome,
    IReadOnlyList<LinhaDaSimulacao> PorMes,
    IReadOnlyList<LinhaDaSimulacao> PorFilial,
    int LinhasComCliente,
    int LinhasSemCliente,
    int LinhasSemFilial,
    int DocumentosComCliente,
    int DocumentosSemCliente,
    int ClientesPromovidos,
    IReadOnlyDictionary<ClasseDeCliente, int> Curva);

/// <summary>O que <c>--somente-faturamento --simular</c> apurou.</summary>
/// <param name="Lote">O que a leitura trouxe.</param>
/// <param name="Hoje">A visão contra o cadastro de hoje.</param>
/// <param name="ComSa1">A visão com os clientes que a carga da SA1 criaria; nula sem a SA1.</param>
/// <param name="Incluiria">Linhas novas, na visão de hoje.</param>
/// <param name="Reapuraria">Linhas existentes que seriam reapuradas.</param>
/// <param name="Removeria">Linhas da janela que a origem não tem mais.</param>
internal sealed record SimulacaoDoFaturamento(
    LoteDoProtheus Lote,
    VisaoDaSimulacao Hoje,
    VisaoDaSimulacao? ComSa1,
    int Incluiria,
    int Reapuraria,
    int Removeria);
