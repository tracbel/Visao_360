using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>O corpo do cadastro de uma cultura. Números aceitam vírgula ou ponto decimal.</summary>
/// <param name="Codigo">O código estável — obrigatório na inclusão, ignorado na alteração.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Segmento">Um dos nove segmentos.</param>
/// <param name="UnidadeComercial">A unidade em que o mercado negocia.</param>
/// <param name="QuilosPorUnidade">Quantos quilos tem a unidade.</param>
/// <param name="FonteDoPreco">CONAB, SOCICANA ou vazio.</param>
/// <param name="ProdutoDoPreco">O identificador na fonte de preço; anda junto com a fonte.</param>
/// <param name="SerieDeCusto">O rótulo da série de custo da CONAB.</param>
/// <param name="EstaAtiva">Se aparece nas telas; ausente mantém como está.</param>
public sealed record NovaCultura(
    string? Codigo = null,
    string? Nome = null,
    string? Segmento = null,
    string? UnidadeComercial = null,
    string? QuilosPorUnidade = null,
    string? FonteDoPreco = null,
    string? ProdutoDoPreco = null,
    string? SerieDeCusto = null,
    bool? EstaAtiva = null);

/// <summary>
/// O CATÁLOGO DE MERCADO NA TELA (issue 165) — leitura para quem tem <c>ParametroDoPotencial.Ler</c>.
///
/// <para>Ele é a fonte única do que é uma cultura: nome, segmento, unidade comercial e de onde vêm o preço
/// e o custo dela. Antes disso, cada tela tinha a própria lista escrita no código — e cultura nova exigia
/// publicação.</para>
/// </summary>
public sealed class ObterCatalogoDoMercado(
    IRepositorioDoCatalogoDoMercado repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Lê o catálogo.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<CatalogoDoMercado>>> ExecutarAsync(CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialLer) && !acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return Resultado<ComProcedencia<CatalogoDoMercado>>.SemPermissao(
                "Ver o catálogo de culturas e categorias exige a permissão de ler os parâmetros do potencial.");

        return Resultado<ComProcedencia<CatalogoDoMercado>>.Ok(
            ComProcedencia<CatalogoDoMercado>.DoNossoBanco(
                await repositorio.LerAsync(ct),
                "organizacao.Cultura · organizacao.ProdutoDaPamNaCultura · organizacao.CategoriaDeMaquina · organizacao.ProdutoDoSicorNaCategoria",
                relogio));
    }
}

/// <summary>
/// CADASTRAR E EDITAR CULTURA (issue 165) — exige <c>ParametroDoPotencial.Administrar</c>.
///
/// <para><b>Não há exclusão.</b> Cultura que sai de cena é desligada: o histórico de preço, custo e
/// potencial continua explicando os números do passado, e apagar a cultura deixaria esses números órfãos.</para>
/// </summary>
public sealed class CadastrarCultura(
    IRepositorioDoCatalogoDoMercado repositorio,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Inclui uma cultura nova.</summary>
    /// <param name="entrada">Os valores.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<CulturaNoCatalogo>> IncluirAsync(NovaCultura entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return Resultado<CulturaNoCatalogo>.SemPermissao("Cadastrar cultura exige a permissão de administrar os parâmetros do potencial.");

        var erros = new ColetorDeErros();
        var codigo = erros.Obrigatorio("codigo", entrada.Codigo, "o código estável da cultura");
        var (nome, segmento, unidade_, quilos) = Ler(erros, entrada);
        if (erros.TemErro) return erros.Recusar<CulturaNoCatalogo>("A cultura tem campos a corrigir.");

        if (await repositorio.ObterPorCodigoAsync(codigo!, ct) is not null)
            return Resultado<CulturaNoCatalogo>.Conflito($"Já existe uma cultura com o código \"{codigo}\".");

        Cultura cultura;
        try
        {
            cultura = Cultura.Registrar(
                codigo!, nome!, segmento!.Value, unidade_!, quilos!.Value,
                entrada.FonteDoPreco, entrada.ProdutoDoPreco, entrada.SerieDeCusto);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<CulturaNoCatalogo>.Falha(erro.Message);
        }

        await repositorio.AdicionarAsync(cultura, ct);
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<CulturaNoCatalogo>.Conflito(gravou.Erro!);

        return Resultado<CulturaNoCatalogo>.Ok(await repositorio.ObterNaTelaAsync(cultura.Codigo, ct));
    }

    /// <summary>Altera uma cultura existente. O código é a identidade e não muda.</summary>
    /// <param name="codigo">O código da cultura.</param>
    /// <param name="entrada">Os valores.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<CulturaNoCatalogo>> AlterarAsync(string codigo, NovaCultura entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return Resultado<CulturaNoCatalogo>.SemPermissao("Alterar cultura exige a permissão de administrar os parâmetros do potencial.");

        if (await repositorio.ObterPorCodigoAsync(codigo, ct) is not { } cultura)
            return Resultado<CulturaNoCatalogo>.NaoEncontrado($"Não há cultura com o código \"{codigo}\".");

        var erros = new ColetorDeErros();
        var (nome, segmento, unidadeComercial, quilos) = Ler(erros, entrada);
        if (erros.TemErro) return erros.Recusar<CulturaNoCatalogo>("A cultura tem campos a corrigir.");

        try
        {
            cultura.Redefinir(
                nome!, segmento!.Value, unidadeComercial!, quilos!.Value,
                entrada.FonteDoPreco, entrada.ProdutoDoPreco, entrada.SerieDeCusto);

            if (entrada.EstaAtiva is { } ativa) cultura.DefinirAtiva(ativa);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<CulturaNoCatalogo>.Falha(erro.Message);
        }

        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<CulturaNoCatalogo>.Conflito(gravou.Erro!);

        return Resultado<CulturaNoCatalogo>.Ok(await repositorio.ObterNaTelaAsync(cultura.Codigo, ct));
    }

    /// <summary>Lê e confere o que a inclusão e a alteração têm em comum.</summary>
    private static (string? Nome, SegmentoDaCultura? Segmento, string? Unidade, decimal? Quilos) Ler(
        ColetorDeErros erros, NovaCultura entrada)
    {
        var nome = erros.Obrigatorio("nome", entrada.Nome, "o nome da cultura");
        var unidade = erros.Obrigatorio("unidadeComercial", entrada.UnidadeComercial, "a unidade comercial");
        var segmento = erros.ItemDeDominio<SegmentoDaCultura>("segmento", entrada.Segmento);
        var quilos = LeituraDeParametro.Numero(erros, "quilosPorUnidade", entrada.QuilosPorUnidade, true, "os quilos por unidade");

        return (nome, segmento, unidade, quilos);
    }
}
