using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Territorio;

/// <summary>
/// A LISTA DE MUNICÍPIOS que o campo de endereço oferece — o que torna a regra exequível.
///
/// <para>O documento 16, seção 3, diz que campo com catálogo não aceita digitação livre. A tela
/// só cumpre a parte dela se tiver de onde puxar a lista, e é isto aqui: o mesmo papel que
/// <c>ListarCatalogos</c> cumpre para origem de lead e motivo de descarte, o município tem numa
/// rota própria porque a lista tem milhares de linhas e não cabe no formato "catálogo inteiro
/// numa resposta só".</para>
///
/// <para><b>Não existe caminho para criar município por aqui</b>, e é decisão: o catálogo é a
/// lista oficial de municípios do Brasil, não um campo que cresce com o que o usuário digitou.
/// Quando o município não aparece na busca, o que falta é carga de catálogo — um chamado, não um
/// item novo criado pela tela. É a diferença entre este catálogo e os de
/// <c>metadado.Catalogo</c>, que trazem <c>permiteItemNovo = true</c>.</para>
/// </summary>
public sealed class ListarMunicipios(IRepositorioTerritorio repositorio, IRelogio relogio)
{
    /// <summary>Executa a busca.</summary>
    /// <param name="pagina">Página pedida.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="termo">O começo do nome do município.</param>
    /// <param name="uf">Filtro por estado, duas letras.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>> ExecutarAsync(
        int? pagina, int? tamanho, string? termo, string? uf, CancellationToken ct)
    {
        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var erros = new ColetorDeErros();

        // A UF É DOMÍNIO FECHADO NO BANCO (CK_Municipio_Uf), e recusar aqui o que o banco
        // recusaria evita a resposta vazia que parece "não existe cidade" quando na verdade o
        // filtro é que era inválido.
        if (!string.IsNullOrWhiteSpace(uf) && !UnidadesFederativas.Contains(uf.Trim().ToUpperInvariant()))
            erros.Registrar(
                "uf", "Não é uma das 27 unidades federativas do Brasil.", uf);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>(
                "A consulta tem parâmetros que não valem.");

        var pagi = await repositorio.ListarMunicipiosAsync(
            new ConsultaDeMunicipios(paginacao.Valor, termo, uf), ct);

        return Resultado<ComProcedencia<PaginaDe<MunicipioParaSelecao>>>.Ok(
            ComProcedencia<PaginaDe<MunicipioParaSelecao>>.DoNossoBanco(
                pagi, "organizacao.Municipio", relogio));
    }

    /// <summary>As 27 unidades federativas — a mesma lista que <c>CK_Municipio_Uf</c> enumera.</summary>
    private static readonly HashSet<string> UnidadesFederativas = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };
}

/// <summary>
/// A COBERTURA AGRUPADA POR FILIAL — o primeiro nível do agrupamento que existe de verdade.
///
/// <para><b>Não há regional aqui, e é o ponto.</b> A tela de Cobertura hoje mostra sete regionais
/// (MT Norte, GO, BA Oeste) que vieram do protótipo e não têm lastro: a tabela de regional do
/// sistema de origem existe e está vazia. O agrupamento real, preenchido, é filial e carteira —
/// e é o que esta rota entrega.</para>
///
/// <para>A métrica ausente é declarada, como manda o padrão desta camada: quando nenhuma carteira
/// da filial declara cidade, a filial aparece com zero municípios e a resposta diz por quê, em
/// vez de sugerir que a filial não atende ninguém.</para>
/// </summary>
public sealed class ObterCoberturaPorFilial(IRepositorioTerritorio repositorio, IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<CoberturaDeFilial>>>> ExecutarAsync(
        CancellationToken ct)
    {
        var filiais = await repositorio.ResumirCoberturaPorFilialAsync(ct);

        var ausentes = new List<MetricaSemDado>();

        var carteiras = filiais.Sum(f => f.Carteiras);
        var comMunicipio = filiais.Sum(f => f.CarteirasComMunicipio);

        if (filiais.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "coberturaPorFilial",
                "Não há filial com carteira ao alcance deste contexto de acesso."));
        else if (comMunicipio == 0)
            ausentes.Add(new MetricaSemDado(
                "municipiosPorFilial",
                $"Nenhuma das {carteiras} carteiras ao seu alcance declara município. O vínculo " +
                "carteira × município vem da carga do sistema de origem — sem ela, a cobertura " +
                "territorial não tem o que mostrar."));
        else if (comMunicipio < carteiras)
            ausentes.Add(new MetricaSemDado(
                "municipiosPorFilial",
                $"{carteiras - comMunicipio} das {carteiras} carteiras ao seu alcance não " +
                "declaram nenhum município no sistema de origem. A cobertura territorial abaixo " +
                "é a das que declaram, e não a da operação inteira."));

        return Resultado<ComProcedencia<Agregado<CoberturaDeFilial>>>.Ok(
            ComProcedencia<Agregado<CoberturaDeFilial>>.DoNossoBanco(
                new Agregado<CoberturaDeFilial>(filiais, ausentes),
                "organizacao.CarteiraMunicipio", relogio));
    }
}

/// <summary>
/// O TERRITÓRIO DE CADA CARTEIRA — a carteira, a filial dela e as cidades que ela atende.
///
/// <para>É o segundo nível do mesmo agrupamento, e o que a tela de Cobertura abre quando alguém
/// clica numa filial. A carteira que não declara cidade nenhuma aparece com a lista vazia, de
/// propósito: escondê-la faria a tela mostrar uma operação menor do que ela é.</para>
/// </summary>
public sealed class ListarTerritorioPorCarteira(IRepositorioTerritorio repositorio, IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="empresaCodigo">Filtro pela filial; nulo traz todas as ao alcance.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<TerritorioDeCarteira>>>> ExecutarAsync(
        string? empresaCodigo, CancellationToken ct)
    {
        var carteiras = await repositorio.ListarTerritorioPorCarteiraAsync(empresaCodigo, ct);

        if (carteiras.Count == 0 && !string.IsNullOrWhiteSpace(empresaCodigo))
            return Resultado<ComProcedencia<Agregado<TerritorioDeCarteira>>>.NaoEncontrado(
                $"Não há carteira da filial \"{empresaCodigo.Trim()}\" ao seu alcance. " +
                "Consulte /api/v1/cobertura/filiais para ver as filiais que você enxerga.");

        var semCidade = carteiras.Count(c => c.Municipios.Count == 0);

        var ausentes = new List<MetricaSemDado>();

        if (semCidade > 0)
            ausentes.Add(new MetricaSemDado(
                "municipiosDaCarteira",
                $"{semCidade} das {carteiras.Count} carteiras não têm nenhum município cadastrado " +
                "no sistema de origem. Elas aparecem com a lista vazia em vez de sumirem da " +
                "tela: a lacuna é do cadastro, não da operação."));

        return Resultado<ComProcedencia<Agregado<TerritorioDeCarteira>>>.Ok(
            ComProcedencia<Agregado<TerritorioDeCarteira>>.DoNossoBanco(
                new Agregado<TerritorioDeCarteira>(carteiras, ausentes),
                "organizacao.CarteiraMunicipio", relogio));
    }
}
