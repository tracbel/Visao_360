using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Anp;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Ibge;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CARGA DA ESTRUTURA AGROPECUÁRIA — o que já existe no território para mecanizar (issue 65).
///
/// <para>A Produção Agrícola Municipal (issue 64) diz quanto se PLANTA. Nenhuma fonte dizia o que há
/// instalado: quantos tratores, de que porte, em propriedades de que tamanho, com quanto rebanho, em
/// quantos km², e onde estão as usinas. São seis leituras públicas, de quatro pesquisas e duas
/// agências:</para>
///
/// <list type="number">
///   <item>tratores e estabelecimentos com trator, por potência — Censo Agropecuário (IBGE);</item>
///   <item>estabelecimentos por grupo de área total — Censo Agropecuário (IBGE);</item>
///   <item>efetivo do rebanho bovino — Pesquisa da Pecuária Municipal (IBGE), anual;</item>
///   <item>área territorial em km² — Censo Demográfico (IBGE);</item>
///   <item>usinas de etanol autorizadas — dados abertos da ANP;</item>
///   <item>o TOTAL que o IBGE publica para São Paulo nas quatro pesquisas (issue 155), que não é a
///   soma dos municípios e é o denominador do "% de São Paulo".</item>
/// </list>
///
/// <para><b>Cada fonte na sua transação.</b> Uma que falhe desfaz só a si mesma; as anteriores ficam,
/// e a reexecução as encontra prontas. É o mesmo desenho da carga do território.</para>
///
/// <para><b>Reexecutável.</b> Rodar de novo sobre as mesmas fontes não grava nada novo: a linha igual
/// não é reapurada. O que muda na fonte é substituído, com quem conferiu por último no carimbo.</para>
///
/// <para><b>Ela conta com o catálogo de municípios já reconhecido</b>, que a carga do território faz e
/// não muda de ano para ano. Município que a fonte cita e o catálogo não conhece vira recusa, com o
/// código, em vez de sumir.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="ibge">A leitura das quatro fontes do IBGE.</param>
/// <param name="anp">A leitura dos dados abertos da ANP.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDaEstruturaAgropecuaria(
    Func<CrmDbContext> abrirContexto,
    LeitorDaEstruturaAgropecuaria ibge,
    LeitorDaAnp anp,
    long usuarioId,
    Action<string> relatar)
{
    private const int CodigoDeSaoPaulo = 35;

    private const string FluxoDaFrota = "IBGE.FROTA_DE_TRATORES";
    private const string FluxoDosEstabelecimentos = "IBGE.ESTABELECIMENTOS_POR_AREA";
    private const string FluxoDoRebanho = "IBGE.REBANHO";
    private const string FluxoDaAreaTerritorial = "IBGE.AREA_TERRITORIAL";
    private const string FluxoDasUsinas = "ANP.USINA_DE_ETANOL";
    private const string FluxoDosTotaisDoEstado = "IBGE.TOTAIS_DO_ESTADO";

    /// <summary>"Total" na classificação 12605 — a faixa que representa o parque inteiro.</summary>
    private const int PotenciaTotal = 113521;

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];

    /// <summary>Executa as seis etapas.</summary>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>As contagens de cada etapa, na ordem em que aconteceram.</returns>
    /// <exception cref="InvalidOperationException">Quando o catálogo de municípios está vazio.</exception>
    public async Task<IReadOnlyList<(string Etapa, string Rotulo, int Valor)>> ExecutarAsync(CancellationToken ct)
    {
        var municipioPorCodigo = await MunicipiosPorCodigoAsync(ct);

        if (municipioPorCodigo.Count == 0)
            throw new InvalidOperationException(
                "Nenhum município tem código do IBGE neste banco. A estrutura agropecuária não tem " +
                "onde ser gravada: rode a carga do território inteira (--somente-territorio) antes.");

        await CarregarFrotaDeTratoresAsync(municipioPorCodigo, ct);
        await CarregarEstabelecimentosPorAreaAsync(municipioPorCodigo, ct);
        await CarregarRebanhoAsync(municipioPorCodigo, ct);
        await CarregarAreaTerritorialAsync(municipioPorCodigo, ct);
        await CarregarUsinasDeEtanolAsync(ct);

        // POR ÚLTIMO, e de propósito: a comparação com a soma dos municípios só faz sentido depois
        // que os municípios desta rodada já estão gravados.
        await CarregarTotaisDoEstadoAsync(ct);

        return _contagens;
    }

    // =============================================================================================
    // 1. Tratores por faixa de potência (Censo Agropecuário)
    // =============================================================================================

    private async Task CarregarFrotaDeTratoresAsync(
        IReadOnlyDictionary<int, int> municipioPorCodigo, CancellationToken ct)
    {
        const string etapa = "Frota de tratores (Censo Agropecuário/IBGE)";

        await using var trava = await TomarTravaAsync(FluxoDaFrota, ct);

        relatar("Lendo a frota de tratores do Censo Agropecuário (SIDRA 6871), por potência…");
        var lidas = await ibge.LerFrotaDeTratoresAsync(CodigoDeSaoPaulo, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaDoIbgeAsync(contexto, ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var anos = lidas.Select(l => l.Ano).Distinct().ToList();
        var existentes = (await contexto.FrotasDeTratoresNosMunicipios.Where(f => anos.Contains(f.Ano)).ToListAsync(ct))
            .ToDictionary(f => (f.MunicipioId, f.Ano, f.PotenciaCodigoIbge));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<FrotaDeTratoresNoMunicipio>();
        int alteradas = 0, mantidas = 0, sigilosas = 0;

        foreach (var linha in lidas)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoMunicipio, out var municipioId))
            {
                recusas.Add((linha, $"O município {linha.CodigoDoMunicipio} não está reconhecido no catálogo."));
                continue;
            }

            int? estabelecimentos, tratores;
            try
            {
                estabelecimentos = ContagemDoSidra(linha.EstabelecimentosComTratorBruto);
                tratores = ContagemDoSidra(linha.TratoresBruto);
            }
            catch (FormatException formato)
            {
                recusas.Add((linha, formato.Message));
                continue;
            }

            if (tratores is null) sigilosas++;

            if (existentes.TryGetValue((municipioId, linha.Ano, linha.PotenciaCodigo), out var existente))
            {
                if (existente.Reapurar(linha.PotenciaNome, estabelecimentos, tratores, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(FrotaDeTratoresNoMunicipio.Registrar(
                    municipioId, linha.Ano, linha.PotenciaCodigo, linha.PotenciaNome,
                    estabelecimentos, tratores, usuarioId, agora));
            }
        }

        contexto.FrotasDeTratoresNosMunicipios.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDaFrota, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = Periodo(anos);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDaFrota, lidas.Count, novas.Count + alteradas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"linhas lidas ({periodo})", lidas.Count);
        Contar(etapa, "linhas novas", novas.Count);
        Contar(etapa, "linhas reapuradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "tratores sob sigilo (\"X\") preservados como nulo", sigilosas);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // 2. Estabelecimentos por grupo de área total (Censo Agropecuário)
    // =============================================================================================

    private async Task CarregarEstabelecimentosPorAreaAsync(
        IReadOnlyDictionary<int, int> municipioPorCodigo, CancellationToken ct)
    {
        const string etapa = "Estabelecimentos por área (Censo Agropecuário/IBGE)";

        await using var trava = await TomarTravaAsync(FluxoDosEstabelecimentos, ct);

        relatar("Lendo os estabelecimentos por grupo de área total (SIDRA 6780)…");
        var lidas = await ibge.LerEstabelecimentosPorAreaAsync(CodigoDeSaoPaulo, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaDoIbgeAsync(contexto, ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var anos = lidas.Select(l => l.Ano).Distinct().ToList();
        var existentes = (await contexto.EstabelecimentosPorAreaNosMunicipios.Where(e => anos.Contains(e.Ano)).ToListAsync(ct))
            .ToDictionary(e => (e.MunicipioId, e.Ano, e.GrupoDeAreaCodigoIbge));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<EstabelecimentosPorAreaNoMunicipio>();
        int alteradas = 0, mantidas = 0;

        foreach (var linha in lidas)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoMunicipio, out var municipioId))
            {
                recusas.Add((linha, $"O município {linha.CodigoDoMunicipio} não está reconhecido no catálogo."));
                continue;
            }

            int? quantos;
            try
            {
                quantos = ContagemDoSidra(linha.ValorBruto);
            }
            catch (FormatException formato)
            {
                recusas.Add((linha, formato.Message));
                continue;
            }

            if (existentes.TryGetValue((municipioId, linha.Ano, linha.CategoriaCodigo), out var existente))
            {
                if (existente.Reapurar(linha.CategoriaNome, quantos, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(EstabelecimentosPorAreaNoMunicipio.Registrar(
                    municipioId, linha.Ano, linha.CategoriaCodigo, linha.CategoriaNome, quantos, usuarioId, agora));
            }
        }

        contexto.EstabelecimentosPorAreaNosMunicipios.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDosEstabelecimentos, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = Periodo(anos);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDosEstabelecimentos, lidas.Count, novas.Count + alteradas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"linhas lidas ({periodo})", lidas.Count);
        Contar(etapa, "linhas novas", novas.Count);
        Contar(etapa, "linhas reapuradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // 3. Efetivo do rebanho (Pesquisa da Pecuária Municipal)
    // =============================================================================================

    private async Task CarregarRebanhoAsync(
        IReadOnlyDictionary<int, int> municipioPorCodigo, CancellationToken ct)
    {
        const string etapa = "Rebanho (Pesquisa da Pecuária Municipal/IBGE)";

        await using var trava = await TomarTravaAsync(FluxoDoRebanho, ct);

        relatar("Lendo o efetivo do rebanho bovino (SIDRA 3939)…");
        var lidas = await ibge.LerRebanhoAsync(CodigoDeSaoPaulo, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaDoIbgeAsync(contexto, ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var anos = lidas.Select(l => l.Ano).Distinct().ToList();
        var existentes = (await contexto.RebanhosNosMunicipios.Where(r => anos.Contains(r.Ano)).ToListAsync(ct))
            .ToDictionary(r => (r.MunicipioId, r.Ano, r.RebanhoCodigoIbge));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<RebanhoNoMunicipio>();
        int alteradas = 0, mantidas = 0;

        foreach (var linha in lidas)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoMunicipio, out var municipioId))
            {
                recusas.Add((linha, $"O município {linha.CodigoDoMunicipio} não está reconhecido no catálogo."));
                continue;
            }

            int? cabecas;
            try
            {
                cabecas = ContagemDoSidra(linha.ValorBruto);
            }
            catch (FormatException formato)
            {
                recusas.Add((linha, formato.Message));
                continue;
            }

            if (existentes.TryGetValue((municipioId, linha.Ano, linha.CategoriaCodigo), out var existente))
            {
                if (existente.Reapurar(linha.CategoriaNome, cabecas, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(RebanhoNoMunicipio.Registrar(
                    municipioId, linha.Ano, linha.CategoriaCodigo, linha.CategoriaNome, cabecas, usuarioId, agora));
            }
        }

        contexto.RebanhosNosMunicipios.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDoRebanho, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = Periodo(anos);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDoRebanho, lidas.Count, novas.Count + alteradas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"linhas lidas ({periodo})", lidas.Count);
        Contar(etapa, "linhas novas", novas.Count);
        Contar(etapa, "linhas reapuradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // 4. Área territorial (Censo Demográfico)
    // =============================================================================================

    private async Task CarregarAreaTerritorialAsync(
        IReadOnlyDictionary<int, int> municipioPorCodigo, CancellationToken ct)
    {
        const string etapa = "Área territorial (IBGE)";

        await using var trava = await TomarTravaAsync(FluxoDaAreaTerritorial, ct);

        relatar("Lendo a área territorial dos municípios (SIDRA 4714)…");
        var lidas = await ibge.LerAreaTerritorialAsync(CodigoDeSaoPaulo, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaDoIbgeAsync(contexto, ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var anos = lidas.Select(l => l.Ano).Distinct().ToList();
        var existentes = (await contexto.AreasTerritoriaisDosMunicipios.Where(a => anos.Contains(a.Ano)).ToListAsync(ct))
            .ToDictionary(a => (a.MunicipioId, a.Ano));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<AreaTerritorialDoMunicipio>();
        int alteradas = 0, mantidas = 0;

        foreach (var linha in lidas)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoMunicipio, out var municipioId))
            {
                recusas.Add((linha, $"O município {linha.CodigoDoMunicipio} não está reconhecido no catálogo."));
                continue;
            }

            decimal? km2;
            try
            {
                // A ÁREA VEM COM PONTO DECIMAL E SEM SEPARADOR DE MILHAR — "1521.202" é a capital, com
                // 1.521,202 km². O mesmo saneamento da PAM serve, porque ele lê em cultura invariante.
                km2 = SaneamentoDeTerritorio.MedidaDoSidra(linha.AreaKm2Bruta);
            }
            catch (FormatException formato)
            {
                recusas.Add((linha, formato.Message));
                continue;
            }

            if (km2 is null or 0)
            {
                recusas.Add((linha, $"O município {linha.CodigoDoMunicipio} veio sem área territorial (\"{linha.AreaKm2Bruta}\")."));
                continue;
            }

            if (existentes.TryGetValue((municipioId, linha.Ano), out var existente))
            {
                if (existente.Reapurar(km2, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(AreaTerritorialDoMunicipio.Registrar(municipioId, linha.Ano, km2, usuarioId, agora));
            }
        }

        contexto.AreasTerritoriaisDosMunicipios.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDaAreaTerritorial, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = Periodo(anos);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDaAreaTerritorial, lidas.Count, novas.Count + alteradas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"municípios lidos ({periodo})", lidas.Count);
        Contar(etapa, "linhas novas", novas.Count);
        Contar(etapa, "linhas reapuradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // 5. Usinas de etanol (ANP)
    // =============================================================================================

    private async Task CarregarUsinasDeEtanolAsync(CancellationToken ct)
    {
        const string etapa = "Usinas de etanol (ANP)";

        await using var trava = await TomarTravaAsync(FluxoDasUsinas, ct);

        relatar("Lendo as usinas de etanol autorizadas (dados abertos da ANP)…");
        var lidas = await anp.LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            contexto, "ANP", "ANP — dados abertos de biocombustíveis", "Arquivo público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        // A ANP NÃO PUBLICA CÓDIGO DE MUNICÍPIO, só o nome em caixa alta e sem acento. O par nome → município é
        // feito uma vez e gravado (issue 154); da segunda rodada em diante, é o par que responde. Nome que
        // aparece em dois municípios de SP não casa: ambiguidade não se resolve por ordem.
        var dePara = await CorrespondenciaDeMunicipios.AbrirAsync(contexto, FluxoDasUsinas, "SP", usuarioId, ct);

        var existentes = (await contexto.UsinasDeEtanol.ToListAsync(ct)).ToDictionary(u => u.Cnpj);

        // O QUE ESTÁ NA LISTA DA ANP, recusado ou não. Uma usina cuja linha foi recusada (município ambíguo, mês fora
        // do formato) continua autorizada — só não pôde ser gravada desta vez —, e não pode ser encerrada por isso.
        var naLista = lidas.Select(l => UsinaDeEtanol.ApenasDigitos(l.Cnpj)).Where(c => c.Length == 14).ToHashSet(StringComparer.Ordinal);

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<UsinaDeEtanol>();
        var vistas = new HashSet<string>(StringComparer.Ordinal);
        var municipiosComUsina = new HashSet<int>();
        int alteradas = 0, mantidas = 0;

        foreach (var linha in lidas)
        {
            if (!dePara.Resolver(linha.Municipio, linha.Municipio, agora, out var municipioId))
            {
                recusas.Add((linha, CorrespondenciaDeMunicipios.MotivoSemPar(linha.Municipio)));
                continue;
            }

            var cnpj = UsinaDeEtanol.ApenasDigitos(linha.Cnpj);
            if (cnpj.Length != 14)
            {
                recusas.Add((linha, $"CNPJ fora do formato: \"{linha.Cnpj}\"."));
                continue;
            }

            if (!vistas.Add(cnpj))
            {
                recusas.Add((linha, $"O CNPJ {cnpj} aparece mais de uma vez no mês lido. Vale a primeira linha."));
                continue;
            }

            DateOnly mes;
            int? anidro, hidratado;
            try
            {
                mes = LeitorDaAnp.ComoData(linha.MesDeReferencia);
                anidro = ContagemDoSidra(linha.CapacidadeDeAnidroBruta);
                hidratado = ContagemDoSidra(linha.CapacidadeDeHidratadoBruta);
            }
            catch (Exception erro) when (erro is FormatException or ArgumentOutOfRangeException)
            {
                recusas.Add((linha, erro.Message));
                continue;
            }

            municipiosComUsina.Add(municipioId);

            if (existentes.TryGetValue(cnpj, out var existente))
            {
                if (existente.Reapurar(linha.RazaoSocial, municipioId, mes, anidro, hidratado, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(UsinaDeEtanol.Registrar(
                    cnpj, linha.RazaoSocial, municipioId, mes, anidro, hidratado, usuarioId, agora));
            }
        }

        // USINA QUE SAIU DA LISTA DA ANP FICA ENCERRADA, e não apagada (issue 153). Até 22/09/2026 ela era apagada,
        // com o argumento de que o histórico estava na ANP — mas a ANP publica só o cadastro de hoje, e a usina que o
        // CRM mostrou em agosto sumia sem rastro. Encerrada, ela sai da contagem vigente e continua consultável. A
        // regra — lista vazia não encerra nada; linha recusada não é usina que saiu — é do domínio.
        var saíram = UsinaDeEtanol.EncerrarAsQueSairam(existentes.Values, naLista, usuarioId, agora);

        contexto.UsinasDeEtanol.AddRange(novas);
        dePara.Gravar(contexto);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDasUsinas, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var mesLido = lidas.Count == 0 ? "sem linhas" : $"mês {lidas[0].MesDeReferencia}";
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDasUsinas, lidas.Count, novas.Count + alteradas + saíram.Count, recusas.Count, ct, mesLido);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"usinas lidas ({mesLido})", lidas.Count);
        Contar(etapa, "usinas novas", novas.Count);
        Contar(etapa, "usinas atualizadas", alteradas);
        Contar(etapa, "usinas mantidas sem mudança", mantidas);
        Contar(etapa, "usinas que saíram da lista da ANP (encerradas, não apagadas)", saíram.Count);
        Contar(etapa, "municípios distintos com usina", municipiosComUsina.Count);
        Contar(etapa, "usinas resolvidas pelo de-para já gravado", dePara.CasadosPelaCorrespondencia);
        Contar(etapa, "correspondências novas gravadas (casadas por nome)", dePara.CasadosPorNome);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // 6. Os totais que o IBGE publica para São Paulo (issue 155)
    // =============================================================================================

    /// <summary>
    /// A LINHA DO ESTADO das quatro pesquisas — o denominador do "% de São Paulo".
    ///
    /// <para><b>Por que não somar os 645 municípios.</b> Onde poucos estabelecimentos respondem, o
    /// IBGE oculta a parcela municipal e a inclui no total do estado: a soma fica abaixo do publicado,
    /// e a fatia da região sai maior do que é. A lavoura já comparava com a linha publicada; tratores
    /// e propriedades somavam — dois métodos na mesma tela.</para>
    /// </summary>
    private async Task CarregarTotaisDoEstadoAsync(CancellationToken ct)
    {
        const string etapa = "Totais publicados do estado (IBGE)";

        await using var trava = await TomarTravaAsync(FluxoDosTotaisDoEstado, ct);

        relatar("Lendo o total publicado de São Paulo nas quatro pesquisas do IBGE (nível n3)…");
        var lidas = await ibge.LerMedidasNoEstadoAsync(CodigoDeSaoPaulo, ct);

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaDoIbgeAsync(contexto, ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var anos = lidas.Select(l => l.Ano).Distinct().ToList();
        var existentes = (await contexto.MedidasDoIbgeNosEstados
                .Where(m => m.EstadoCodigoIbge == CodigoDeSaoPaulo && anos.Contains(m.Ano))
                .ToListAsync(ct))
            .ToDictionary(m => (m.TabelaDoSidra, m.VariavelDoSidra, m.Ano, m.CategoriaCodigoIbge));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<MedidaDoIbgeNoEstado>();
        int alteradas = 0, mantidas = 0, sigilosas = 0;

        foreach (var linha in lidas)
        {
            if (linha.CodigoDaUf != CodigoDeSaoPaulo)
            {
                recusas.Add((linha, $"A resposta trouxe a UF {linha.CodigoDaUf}, e a carga pediu {CodigoDeSaoPaulo}."));
                continue;
            }

            decimal? valor;
            try
            {
                valor = SaneamentoDeTerritorio.MedidaDoSidra(linha.ValorBruto);
            }
            catch (FormatException formato)
            {
                recusas.Add((linha, formato.Message));
                continue;
            }

            if (valor is null) sigilosas++;

            if (existentes.TryGetValue((linha.Tabela, linha.Variavel, linha.Ano, linha.CategoriaCodigo), out var existente))
            {
                if (existente.Reapurar(linha.CategoriaNome, valor, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(MedidaDoIbgeNoEstado.Registrar(
                    linha.CodigoDaUf, linha.Tabela, linha.Variavel, linha.Ano,
                    linha.CategoriaCodigo, linha.CategoriaNome, valor, usuarioId, agora));
            }
        }

        contexto.MedidasDoIbgeNosEstados.AddRange(novas);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDosTotaisDoEstado, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = Periodo(anos);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDosTotaisDoEstado, lidas.Count, novas.Count + alteradas, recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"medidas lidas no total do estado ({periodo})", lidas.Count);
        Contar(etapa, "linhas novas", novas.Count);
        Contar(etapa, "linhas reapuradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "medidas sob sigilo preservadas como nulo", sigilosas);
        Contar(etapa, "linhas recusadas", recusas.Count);

        // A PROVA DO QUE A ISSUE 155 AFIRMA, no próprio relatório: o publicado e a soma, lado a lado.
        // Enquanto os dois forem diferentes, usar a soma como denominador infla a fatia da região.
        await CompararTratoresComASomaAsync(contexto, etapa, ct);
    }

    /// <summary>Põe no relatório o parque publicado de São Paulo e a soma dos municípios, para conferência.</summary>
    private async Task CompararTratoresComASomaAsync(CrmDbContext contexto, string etapa, CancellationToken ct)
    {
        var publicado = await contexto.MedidasDoIbgeNosEstados.AsNoTracking()
            .Where(m => m.EstadoCodigoIbge == CodigoDeSaoPaulo
                        && m.TabelaDoSidra == LeitorDaEstruturaAgropecuaria.TabelaDaFrotaDeTratores
                        && m.VariavelDoSidra == LeitorDaEstruturaAgropecuaria.VariavelDeTratores
                        && m.CategoriaCodigoIbge == PotenciaTotal)
            .OrderByDescending(m => m.Ano)
            .Select(m => new { m.Ano, m.Valor })
            .FirstOrDefaultAsync(ct);

        if (publicado?.Valor is null) return;

        var soma = await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking()
            .Where(f => f.Ano == publicado.Ano && f.PotenciaCodigoIbge == PotenciaTotal)
            .SumAsync(f => (int?)f.Tratores, ct) ?? 0;

        Contar(etapa, $"tratores publicados para São Paulo em {publicado.Ano}", (int)publicado.Valor.Value);
        Contar(etapa, "tratores somados dos municípios (fica abaixo onde há sigilo)", soma);
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// Uma CONTAGEM do SIDRA — inteira, ao contrário das medidas da PAM, que são decimais.
    ///
    /// <para>Tratores, estabelecimentos, cabeças e m³/dia não têm casa decimal. Reaproveitar o
    /// saneamento decimal e converter no fim esconderia um erro de leitura: "1.5 trator" viraria 1 ou
    /// 2 em silêncio, em vez de parar e dizer que a resposta mudou de forma.</para>
    /// </summary>
    /// <param name="valor">O campo <c>V</c> da resposta.</param>
    /// <exception cref="FormatException">Quando o valor não é um símbolo conhecido nem um inteiro.</exception>
    private static int? ContagemDoSidra(string? valor)
    {
        var medida = SaneamentoDeTerritorio.MedidaDoSidra(valor);
        if (medida is null) return null;

        if (decimal.Truncate(medida.Value) != medida.Value)
            throw new FormatException(
                $"Esperava uma contagem inteira do IBGE e veio \"{valor}\". A resposta mudou de forma.");

        return (int)medida.Value;
    }

    /// <summary>O rótulo do período de uma rodada, para o ponto de sincronismo.</summary>
    private static string Periodo(IReadOnlyCollection<short> anos) =>
        anos.Count == 0 ? "sem linhas" : $"ano{(anos.Count > 1 ? "s" : "")} {string.Join(", ", anos.Order())}";

    private Task<TravaDeFluxo> TomarTravaAsync(string fluxo, CancellationToken ct) =>
        TravaDeFluxo.TomarAsync(abrirContexto(), fluxo, ct);

    private static Task<int> SistemaDoIbgeAsync(CrmDbContext contexto, CancellationToken ct) =>
        CargaDeTerritorio.SistemaAsync(
            contexto, "IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura", ct);

    private void Contar(string etapa, string rotulo, int valor) => _contagens.Add((etapa, rotulo, valor));

    private async Task<IReadOnlyDictionary<int, int>> MunicipiosPorCodigoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        return await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge != null)
            .ToDictionaryAsync(m => m.CodigoIbge!.Value, m => m.Id, ct);
    }
}
