using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>
/// OS PARÂMETROS DO POTENCIAL QUE VALEM NUMA DATA (issue 71). Sem data, hoje.
///
/// <para>É a pergunta que o motor (issues 72 a 74) vai fazer para cada cálculo, e é a mesma que a tela do
/// administrador (issue 77) mostra: "com que parâmetros este número foi feito?". Uma data passada devolve
/// o que valia naquela data, mesmo que já tenha sido trocado depois.</para>
/// </summary>
public sealed class ObterParametrosDoPotencial(
    IRepositorioDeParametrosDoPotencial repositorio,
    IRepositorioDeReferenciasDoPotencial referencias,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IRelogio relogio)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Lê os parâmetros vigentes.</summary>
    /// <param name="em">A data, aaaa-mm-dd. Vazia é hoje.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<ParametrosDoPotencialVigentes>>> ExecutarAsync(string? em, CancellationToken ct)
    {
        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "em", em, obrigatoria: false) ?? ParametroComVigencia.HojeNoBrasil(relogio.Agora);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<ParametrosDoPotencialVigentes>>("A data da consulta não vale.");

        var gerais = await repositorio.ListarGeraisAsync(ct);
        var regras = await repositorio.ListarRegrasAsync(ct);
        var percepcoes = await repositorio.ListarPercepcoesAsync(ct);

        var geral = ParametroComVigencia.VigenteEm(gerais, data);

        // O GRUPO É PRODUTO **E CATEGORIA** (D-P01, issue 63).
        //
        // Era só o produto, e isso escondia metade da decisão: com "um trator a cada 10 ha de café" e "uma
        // colheitadeira a cada 200 ha de café" registrados, agrupar por produto faria `VigenteEm` escolher
        // UMA das duas e a outra sumir da tela — sem erro, sem aviso, com cara de que nunca foi registrada.
        var regrasVigentes = regras.GroupBy(r => (r.ProdutoCodigoIbge, r.CategoriaDeMaquinaId))
            .Select(g => ParametroComVigencia.VigenteEm(g, data))
            .OfType<RegraDePotencial>()
            .OrderBy(r => r.ProdutoNome, StringComparer.Create(PtBr, ignoreCase: true))
            .ThenBy(r => r.CategoriaDeMaquinaId)
            .ToList();
        var percepcoesVigentes = percepcoes.GroupBy(p => p.MunicipioId)
            .Select(g => ParametroComVigencia.VigenteEm(g, data))
            .OfType<PercepcaoDoGestor>()
            .ToList();

        var usados = new ParametroComVigencia?[] { geral }.Concat(regrasVigentes).Concat(percepcoesVigentes).OfType<ParametroComVigencia>().ToList();
        var nomes = await referencias.NomesDosUsuariosAsync(Autores(usados), ct);
        var municipios = await referencias.MunicipiosAsync(percepcoesVigentes.Select(p => p.MunicipioId).ToHashSet(), ct);

        // OS RÓTULOS DO CATÁLOGO. Sem eles a tela mostraria duas linhas de "Café" idênticas, uma do trator e
        // outra da colheitadeira, sem nada que as distinguisse.
        var culturas = await catalogo.CulturasAsync(
            regrasVigentes.Select(r => r.CulturaId).OfType<int>().ToHashSet(), ct);
        var categorias = await catalogo.CategoriasDeMaquinaAsync(
            regrasVigentes.Select(r => r.CategoriaDeMaquinaId).OfType<int>().ToHashSet(), ct);

        var vigentes = new ParametrosDoPotencialVigentes(
            data,
            geral is null ? null : ParametrosGeraisDetalhe.De(geral, nomes),
            [.. regrasVigentes.Select(r => RegraDePotencialDetalhe.De(r, nomes, culturas, categorias))],
            [.. MontagemDasPercepcoes.Detalhar(percepcoesVigentes, municipios, nomes)
                .OrderBy(p => p.MunicipioNome, StringComparer.Create(PtBr, ignoreCase: true))],
            Pendencias(geral, regrasVigentes, categorias));

        return Resultado<ComProcedencia<ParametrosDoPotencialVigentes>>.Ok(
            ComProcedencia<ParametrosDoPotencialVigentes>.DoNossoBanco(
                vigentes, "organizacao.ParametroDoPotencial · organizacao.RegraDePotencial · organizacao.PercepcaoDoGestor", relogio));
    }

    /// <summary>Os usuários que aparecem como autor ou revogador.</summary>
    /// <param name="parametros">As vigências.</param>
    internal static HashSet<long> Autores(IEnumerable<ParametroComVigencia> parametros) =>
        parametros.SelectMany(p => new[] { p.InformadoPorId, p.RevogadoPorId }).OfType<long>().ToHashSet();

    /// <summary>
    /// O QUE FALTA DECIDIR, EM FRASE. O motor não usa valor padrão para o que está em aberto (documento 48,
    /// §9: "sem valor decidido, o cálculo fica vazio com o motivo"); a lista diz ao administrador o que
    /// preencher para cada parte do potencial sair.
    /// </summary>
    private static List<string> Pendencias(
        ParametroDoPotencial? geral,
        List<RegraDePotencial> regras,
        IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial> categorias)
    {
        var pendencias = new List<string>();

        if (geral is null)
        {
            pendencias.Add("Não há parâmetros gerais vigentes nesta data: sem janela, faixas e composição do crédito, nenhum índice de mercado sai.");
        }
        else
        {
            if (geral.PesoDoIndicadorDePreco is null || geral.PesoDoIndicadorDeCredito is null || geral.PesoDoIndicadorComercial is null)
                pendencias.Add("Os pesos dos três indicadores (preço, crédito e percepção comercial) estão em aberto (D-P05): sem eles, o fator de ciclo e os cenários não saem.");

            if (geral.FatorMinimo is null)
                pendencias.Add("Os limites do fator de ciclo estão em aberto (D-P05).");

            if (geral.NomeDaFaixaIntermediaria is null)
                pendencias.Add(
                    $"O nome da faixa entre {geral.LimiteDeRetracao.ToString("0.00", PtBr)} e {geral.LimiteDeAquecimento.ToString("0.00", PtBr)} " +
                    "está em aberto (D-P02): o texto diz \"= 1 anual\" e pula para \"> 1,2 aquecido\".");
        }

        if (regras.Count == 0)
            pendencias.Add("Nenhuma cultura tem regra vigente nesta data: o potencial estrutural não sai.");

        foreach (var regra in regras)
        {
            // A PENDÊNCIA DIZ DE QUAL MÁQUINA (D-P01). Com trator e colheitadeira no mesmo produto, "Café:
            // sem anos de renovação" não diz qual das duas regras precisa ser completada.
            var nome = regra.CategoriaDeMaquinaId is { } id && categorias.TryGetValue(id, out var categoria)
                ? $"{regra.ProdutoNome} · {categoria.Nome}"
                : regra.ProdutoNome;

            if (regra.AnosDeRenovacao is null)
                pendencias.Add($"{nome}: sem anos de renovação — o parque necessário sai, a demanda anual não.");

            if (regra.Situacao == SituacaoDaRegraDePotencial.AConfirmar)
                pendencias.Add($"{nome}: a regra está a confirmar, e o potencial dela sai como estimativa.");
        }

        return pendencias;
    }
}

/// <summary>Todas as vigências já registradas — a trilha legível dos parâmetros (issue 71).</summary>
public sealed class ListarHistoricoDosParametrosDoPotencial(
    IRepositorioDeParametrosDoPotencial repositorio,
    IRepositorioDeReferenciasDoPotencial referencias,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IRelogio relogio)
{
    /// <summary>Lê o histórico.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<HistoricoDosParametrosDoPotencial>>> ExecutarAsync(CancellationToken ct)
    {
        var gerais = await repositorio.ListarGeraisAsync(ct);
        var regras = await repositorio.ListarRegrasAsync(ct);
        var percepcoes = await repositorio.ListarPercepcoesAsync(ct);

        var todos = gerais.Cast<ParametroComVigencia>().Concat(regras).Concat(percepcoes);
        var nomes = await referencias.NomesDosUsuariosAsync(ObterParametrosDoPotencial.Autores(todos), ct);
        var municipios = await referencias.MunicipiosAsync(percepcoes.Select(p => p.MunicipioId).ToHashSet(), ct);

        // A TRILHA TAMBÉM PRECISA DO RÓTULO DA CATEGORIA: sem ele, duas vigências do café na mesma data — a
        // do trator e a da colheitadeira — ficam indistinguíveis no histórico, que é onde se confere quem
        // decidiu o quê.
        var culturas = await catalogo.CulturasAsync(regras.Select(r => r.CulturaId).OfType<int>().ToHashSet(), ct);
        var categorias = await catalogo.CategoriasDeMaquinaAsync(
            regras.Select(r => r.CategoriaDeMaquinaId).OfType<int>().ToHashSet(), ct);

        var historico = new HistoricoDosParametrosDoPotencial(
            [.. gerais.OrderByDescending(g => g.VigenteDesde).ThenByDescending(g => g.InformadoEm).Select(g => ParametrosGeraisDetalhe.De(g, nomes))],
            [.. regras.OrderBy(r => r.ProdutoCodigoIbge).ThenBy(r => r.CategoriaDeMaquinaId)
                .ThenByDescending(r => r.VigenteDesde).ThenByDescending(r => r.InformadoEm)
                .Select(r => RegraDePotencialDetalhe.De(r, nomes, culturas, categorias))],
            MontagemDasPercepcoes.Detalhar(
                [.. percepcoes.OrderByDescending(p => p.VigenteDesde).ThenByDescending(p => p.InformadoEm)], municipios, nomes));

        return Resultado<ComProcedencia<HistoricoDosParametrosDoPotencial>>.Ok(
            ComProcedencia<HistoricoDosParametrosDoPotencial>.DoNossoBanco(
                historico, "organizacao.ParametroDoPotencial · organizacao.RegraDePotencial · organizacao.PercepcaoDoGestor", relogio));
    }
}

/// <summary>A percepção com o município por extenso.</summary>
internal static class MontagemDasPercepcoes
{
    /// <summary>Detalha as percepções, na ordem recebida, pondo o município por nome.</summary>
    public static List<PercepcaoDoGestorDetalhe> Detalhar(
        IReadOnlyList<PercepcaoDoGestor> percepcoes,
        IReadOnlyDictionary<int, MunicipioDoParametro> municipios,
        IReadOnlyDictionary<long, string> nomes) =>
        [
            .. percepcoes.Select(p =>
            {
                var municipio = municipios.GetValueOrDefault(p.MunicipioId);
                return new PercepcaoDoGestorDetalhe(
                    municipio?.CodigoIbge ?? 0, municipio?.Nome ?? $"município {p.MunicipioId}", municipio?.Uf ?? "",
                    p.Percentual, VigenciaDoParametro.De(p, nomes));
            })
        ];
}
