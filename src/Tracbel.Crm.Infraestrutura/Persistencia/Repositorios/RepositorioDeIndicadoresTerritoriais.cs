using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS TRÊS MAPAS DA ADR, apurados município a município (documento 32, seção 8).
///
/// <para><b>O município do cliente é o do endereço principal</b>, pelo código IBGE do catálogo.
/// Não é o local da entrega nem o da propriedade — nenhum dos dois existe no dado —, e a tela diz
/// isso. Cliente sem município, com município sem código IBGE ou fora de São Paulo não some: vai
/// para <see cref="IndicadoresForaDoMapa"/>, e a soma do mapa com o que ficou fora dele é o total
/// da consulta. A nota sem cliente no CRM (<see cref="FaturamentoSemCliente"/>) também entra, num
/// grupo por natureza da contraparte — sem ela o total seria só o faturamento com cadastro.</para>
///
/// <para><b>Como em todo repositório, não há <c>Where</c> de empresa aqui.</b> Cliente, endereço,
/// carteira e faturamento entram pelo filtro global; área de atuação, responsáveis e área plantada
/// não têm dono de filial e não são filtrados.</para>
///
/// <para><b>UMA COMPOSIÇÃO (plano 2 do documento 54):</b> a referência vem dos três leitores guardados; o recorte,
/// dos leitores do recorte; a conta, da acumulação e da montagem, em memória. Ficam aqui a composição, os filtros
/// da tela e a lavoura do recorte.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
/// <param name="territorioDeReferencia">A área de atuação, os municípios e as lojas — dado de referência (documento 54).</param>
/// <param name="potencialDeReferencia">
/// As regras, o catálogo do motor (issue 161) e a PAM — dado de referência (documento 54).
/// </param>
/// <param name="estruturaDeReferencia">
/// O parque, as propriedades, o rebanho, as usinas e os totais do estado e da região — dado de referência (plano 2).
/// </param>
public sealed class RepositorioDeIndicadoresTerritoriais(
    CrmDbContext contexto,
    IRepositorioDoTerritorioDeReferencia territorioDeReferencia,
    IRepositorioDoPotencialDeReferencia potencialDeReferencia,
    IRepositorioDaEstruturaDeReferencia estruturaDeReferencia)
    : IRepositorioIndicadoresTerritoriais
{
    /// <summary>Sem cache — o teste de contêiner e quem monta o repositório à mão leem a referência direto do banco.</summary>
    /// <param name="contexto">O contexto do banco.</param>
    public RepositorioDeIndicadoresTerritoriais(CrmDbContext contexto)
        : this(
            contexto,
            new RepositorioDoTerritorioDeReferencia(contexto),
            new RepositorioDoPotencialDeReferencia(contexto, new RepositorioDoMotorDoPotencial(contexto)),
            new RepositorioDaEstruturaDeReferencia(contexto)) { }

    // O CATÁLOGO DO MOTOR mora em IRepositorioDoMotorDoPotencial desde a issue 161: a calculadora
    // precisa exatamente do mesmo, e duas leituras do mesmo conceito divergem no dia em que uma mudar. A PAM, as regras
    // e a conta de cada município moram no potencial de referência desde o documento 54.

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, int>> ListarFiliaisAsync(CancellationToken ct) =>
        await contexto.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);

    /// <inheritdoc />
    public async Task<IndicadoresTerritoriais> ApurarAsync(
        ConsultaDeIndicadoresTerritoriais consulta, DateTime agoraUtc, CancellationToken ct)
    {
        // A VISÃO DA EMPRESA ABRE O ALCANCE ENTRE FILIAIS, e só ela. O CrmDbContext recusa abrir
        // para quem não tem Empresa.AlcanceEntreFiliais em profundidade Organização e registra a
        // abertura no diário — o caso de uso já conferiu a permissão antes de chegar aqui.
        using var alcance = consulta.Visao == VisaoTerritorial.Empresa
            ? contexto.AbrirAlcanceEntreEmpresas("Visão consolidada da empresa nos indicadores territoriais (documento 32)")
            : null;

        // A REFERÊNCIA (documento 54): igual para todo mundo, guardada pela versão de cada assunto.
        var territorio = await territorioDeReferencia.LerAsync(ct);
        var potencial = await potencialDeReferencia.LerAsync(ParametroComVigencia.HojeNoBrasil(agoraUtc), ct);
        var estrutura = await estruturaDeReferencia.LerAsync(ct);

        // O RECORTE: o que passa pelo filtro de filial, lido na hora — cada natureza no seu leitor.
        var clientes = await new LeitorDosClientesDoRecorte(contexto).LerAsync(consulta.ResponsavelId, ct);
        var cobertura = await new LeitorDaCoberturaDoRecorte(contexto).LerAsync(ct);
        var faturamento = await new LeitorDoFaturamentoDoRecorte(contexto).LerAsync(consulta, ct);
        var art = await new LeitorDasVendasDoArtNoRecorte(contexto).LerAsync(consulta, ct);
        var parqueConectado = await new LeitorDoParqueConectado(contexto).LerAsync(ct);
        var interacaoMaisRecente = await contexto.ClienteCarteiras.AsNoTracking().MaxAsync(v => v.UltimaInteracaoEm, ct);
        var enderecos = await contexto.Enderecos.AsNoTracking().CountAsync(e => e.ExcluidoEm == null, ct);
        var enderecosComArea = await contexto.Enderecos.AsNoTracking()
            .CountAsync(e => e.ExcluidoEm == null && e.Hectares != null && e.CulturaId != null, ct);

        // A CONTA, EM MEMÓRIA: cada linha num grupo, e os municípios montados por cima da referência.
        var acumulado = AcumulacaoDoRecorte.Acumular(consulta, agoraUtc, territorio.MunicipiosPorId, clientes, cobertura, faturamento, art);
        var (indicadores, codigosDaAdr) = MontagemDosIndicadores.Montar(
            consulta, agoraUtc, territorio, potencial, estrutura, acumulado, cobertura, faturamento, art,
            parqueConectado, interacaoMaisRecente, enderecos, enderecosComArea);

        // A LAVOURA DO RECORTE depende dos municípios que entraram nele — por isso é a última leitura.
        return indicadores with { LavouraDoRecorte = await LerLavouraDoRecorteAsync(codigosDaAdr, ct) };
    }

    /// <summary>
    /// A ÁREA DE TODAS AS CULTURAS DA PAM nos municípios da ADR do recorte (issue 168) — uma leitura PRÓPRIA,
    /// que não passa pelas regras de potencial.
    ///
    /// <para><b>Cada produto no último ano em que a área DELE foi divulgada</b> (issue 152), como no resto da
    /// tela: o maior ano da tabela inteira misturaria anos em silêncio no dia em que a PAM nova entrasse
    /// incompleta. A soma é em memória — o SQLite dos testes não agrega decimal —, e o volume é de uma linha por
    /// produto e município dos anos carregados.</para>
    ///
    /// <para><b>Sigilo não vira zero:</b> o município sem área divulgada não entra na soma, e a contagem dos que
    /// entraram vai junto.</para>
    /// </summary>
    /// <param name="codigosDaAdr">Os municípios da ADR que entraram no recorte, pelo código IBGE.</param>
    /// <param name="ct">Cancelamento.</param>
    private async Task<List<AreaDoProdutoNoRecorte>> LerLavouraDoRecorteAsync(IReadOnlyList<int> codigosDaAdr, CancellationToken ct)
    {
        if (codigosDaAdr.Count == 0) return [];

        var linhas = await (
                from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                where municipio.CodigoIbge != null && codigosDaAdr.Contains(municipio.CodigoIbge!.Value)
                select new
                {
                    linha.ProdutoCodigoIbge,
                    linha.ProdutoNome,
                    linha.Ano,
                    linha.AreaPlantadaHectares,
                    linha.AreaColhidaHectares
                })
            .ToListAsync(ct);

        return
        [
            .. linhas
                .GroupBy(l => l.ProdutoCodigoIbge)
                .Select(produto =>
                {
                    var comArea = produto.Where(l => l.AreaPlantadaHectares is not null).ToList();
                    if (comArea.Count == 0) return null;

                    var ano = comArea.Max(l => l.Ano);
                    var doAno = comArea.Where(l => l.Ano == ano).ToList();
                    var colhidas = doAno.Where(l => l.AreaColhidaHectares is not null).ToList();

                    return new AreaDoProdutoNoRecorte(
                        produto.Key,
                        doAno[0].ProdutoNome,
                        ano,
                        doAno.Sum(l => l.AreaPlantadaHectares!.Value),
                        colhidas.Count == 0 ? null : colhidas.Sum(l => l.AreaColhidaHectares!.Value),
                        doAno.Count);
                })
                .Where(a => a is not null)
                .Select(a => a!)
                .OrderByDescending(a => a.AreaPlantadaHectares)
                .ThenBy(a => a.ProdutoCodigoIbge)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaParaFiltro>> ListarCategoriasDeMaquinaAsync(CancellationToken ct)
    {
        // O CATÁLOGO E O DE-PARA, e não uma lista escrita aqui: a categoria que o comercial ligar a uma linha
        // de produto aparece sozinha no filtro. Os dois cruzam por Id, sem tocar a colação da linha.
        var comLinha = await contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
            .Select(l => l.CategoriaDeMaquinaId)
            .Distinct()
            .ToListAsync(ct);

        return
        [
            .. (await contexto.CategoriasDeMaquina.AsNoTracking()
                    .Where(c => comLinha.Contains(c.Id))
                    .Select(c => new { c.Codigo, c.Nome, c.Ordem })
                    .ToListAsync(ct))
                .OrderBy(c => c.Ordem)
                .ThenBy(c => c.Codigo, StringComparer.Ordinal)
                .Select(c => new CategoriaParaFiltro(c.Codigo, c.Nome, c.Ordem))
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResponsavelDeCarteira>> ListarResponsaveisDasCarteirasAsync(
        bool empresaInteira, CancellationToken ct)
    {
        // NA VISÃO DA EMPRESA, OS RESPONSÁVEIS DE TODAS AS FILIAIS (revisão de 27/09/2026): sem abrir o alcance, a
        // lista — e a validação do filtro — ficava presa à filial do cabeçalho, e o painel da empresa inteira não
        // aceitava o CEN de outra filial. A abertura vai para o diário com o motivo, como a do painel.
        using var alcance = empresaInteira
            ? contexto.AbrirAlcanceEntreEmpresas("Responsáveis das carteiras na visão consolidada da empresa (documento 32)")
            : null;

        // AS CARTEIRAS COMERCIAIS AO ALCANCE (filtro global de filial), e o dono de cada uma. O gestor vem do
        // cadastro de usuário — hoje, em produção, nenhum responsável tem gestor cadastrado, e a tela diz isso.
        var carteiras = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Natureza == NaturezaDaCarteira.Comercial)
            .Select(c => c.ResponsavelId)
            .ToListAsync(ct);

        var ids = carteiras.Distinct().ToList();
        var usuarios = await contexto.Usuarios.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.NomeExibicao, u.Natureza, u.GestorId })
            .ToListAsync(ct);

        var idsDosGestores = usuarios.Where(u => u.GestorId is not null).Select(u => u.GestorId!.Value).Distinct().ToList();
        var gestores = idsDosGestores.Count == 0
            ? new Dictionary<long, string>()
            : await contexto.Usuarios.AsNoTracking()
                .Where(u => idsDosGestores.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.NomeExibicao, ct);

        return
        [
            .. usuarios
                .Select(u => new ResponsavelDeCarteira(
                    u.Id,
                    u.NomeExibicao,
                    u.Natureza.ToString(),
                    carteiras.Count(c => c == u.Id),
                    u.GestorId is { } gestor && gestores.TryGetValue(gestor, out var nome) ? nome : null))
                .OrderBy(r => r.Nome, StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), true))
        ];
    }
}
