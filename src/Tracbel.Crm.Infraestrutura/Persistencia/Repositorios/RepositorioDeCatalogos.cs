using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// As listas que alimentam os campos de seleção, lidas do banco.
///
/// TRÊS FONTES, UM FORMATO: <c>metadado.Catalogo</c> (as listas administradas pelo negócio),
/// <c>frota.Modelo</c> (o catálogo de máquina, que tem tabela própria porque tem atributo além
/// de código e descrição) e <c>organizacao.Empresa</c> (as filiais). O front recebe as três com
/// a mesma cara e não precisa saber de onde cada uma veio.
///
/// ITEM INATIVO NÃO É OFERECIDO, mas continua existindo — é a regra do documento 16, seção 3.1:
/// valor de catálogo que saiu de uso se APOSENTA, nunca se apaga, porque apagar quebraria toda
/// chave estrangeira que aponta para ele. [V] no legado, 602 das 980 ações estão fora de uso na
/// MESMA lista das 378 vivas, e não há como a tela saber quais oferecer.
/// </summary>
public sealed class RepositorioDeCatalogos(CrmDbContext contexto) : IRepositorioCatalogos
{
    /// <summary>O código do catálogo sintético que lista as filiais.</summary>
    public const string CodigoDeEmpresa = "EMPRESA";

    /// <summary>O código do catálogo sintético que lista os modelos de máquina.</summary>
    public const string CodigoDeModelo = "MODELO_EQUIPAMENTO";

    /// <summary>O código do catálogo sintético que lista as classificações de produto.</summary>
    public const string CodigoDeLinhaDeProduto = "LINHA_DE_PRODUTO";

    /// <inheritdoc />
    public async Task<IReadOnlyList<CatalogoParaSelecao>> ListarAsync(string? codigo, CancellationToken ct)
    {
        var resultado = new List<CatalogoParaSelecao>();

        if (codigo is null || (codigo != CodigoDeEmpresa && codigo != CodigoDeModelo && codigo != CodigoDeLinhaDeProduto))
            resultado.AddRange(await LerDeMetadadoAsync(codigo, ct));

        if (codigo is null || codigo == CodigoDeModelo)
            resultado.Add(await LerModelosAsync(ct));

        if (codigo is null || codigo == CodigoDeLinhaDeProduto)
            resultado.Add(await LerLinhasDeProdutoAsync(ct));

        if (codigo is null || codigo == CodigoDeEmpresa)
            resultado.Add(await LerFiliaisAsync(ct));

        // Catálogo sintético pedido nominalmente e que veio vazio não é catálogo: é lista que
        // ainda não foi semeada. Devolver a casca faria a tela mostrar um campo de seleção sem
        // nenhuma opção, o que é pior do que dizer que a lista não existe.
        return [.. resultado.Where(c => c.Itens.Count > 0 || codigo is null)];
    }

    /// <inheritdoc />
    public async Task<int?> ResolverItemAsync(int catalogoId, string codigoDoItem, CancellationToken ct)
    {
        // A CONSULTA CARREGA O CATÁLOGO NA CONDIÇÃO, e não só o código do item. É o que impede
        // um item de CULTURA de ser aceito onde se espera ORIGEM_LEAD — a mesma amarração da
        // chave estrangeira composta do documento 21, achado I-1, do lado da aplicação.
        var id = await contexto.CatalogoItens
            .Where(i => i.CatalogoId == catalogoId
                     && i.Codigo == codigoDoItem
                     && i.EstaAtivo)
            .Select(i => (int?)i.Id)
            .FirstOrDefaultAsync(ct);

        return id;
    }

    /// <inheritdoc />
    public Task<ModeloParaSelecao?> ObterModeloAsync(string codigo, CancellationToken ct) =>
        (from m in contexto.Modelos
         join f in contexto.Familias on m.FamiliaId equals f.Id
         join ma in contexto.Marcas on f.MarcaId equals ma.Id
         where m.Codigo == codigo && m.EstaAtivo
         select new ModeloParaSelecao(m.Id, m.Codigo, m.Nome, f.Nome, ma.Nome, ma.EhRepresentada))
        .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public Task<LinhaDeProdutoParaSelecao?> ObterLinhaDeProdutoAsync(string codigo, CancellationToken ct) =>
        contexto.LinhasDeProduto.AsNoTracking()
            .Where(l => l.Codigo == codigo && l.EstaAtiva)
            .Select(l => new LinhaDeProdutoParaSelecao(l.Id, l.Codigo, l.Nome, l.Porte))
            .FirstOrDefaultAsync(ct);

    private async Task<CatalogoParaSelecao> LerLinhasDeProdutoAsync(CancellationToken ct)
    {
        var linhas = await contexto.LinhasDeProduto.AsNoTracking()
            .Where(l => l.EstaAtiva)
            .OrderBy(l => l.Nome)
            .Select(l => new { l.Codigo, l.Nome, l.Porte })
            .ToListAsync(ct);

        return new CatalogoParaSelecao(
            CodigoDeLinhaDeProduto,
            "Classificação de produto",
            "A categoria comercial da máquina — trator pequeno, médio e grande, colhedora de cana, " +
            "pulverizador. Aponta para a família compatível do catálogo, sem substituí-la. O de-para " +
            "das linhas do ART é explícito (documento 35, seção 10).",
            PermiteItemNovo: false,
            [.. linhas.Select((l, ordem) => new ItemParaSelecao(l.Codigo, l.Nome, (short)(ordem + 1), false))]);
    }

    private async Task<List<CatalogoParaSelecao>> LerDeMetadadoAsync(string? codigo, CancellationToken ct)
    {
        var catalogos = contexto.Catalogos.Where(c => c.EstaAtivo);
        if (codigo is not null) catalogos = catalogos.Where(c => c.Codigo == codigo);

        var linhas = await catalogos
            .OrderBy(c => c.Codigo)
            .Select(c => new
            {
                c.Id,
                c.Codigo,
                c.Nome,
                c.Descricao,
                c.PermiteItemNovo,
                Itens = contexto.CatalogoItens
                    .Where(i => i.CatalogoId == c.Id && i.EstaAtivo)
                    .OrderBy(i => i.Ordem).ThenBy(i => i.Descricao)
                    .Select(i => new ItemParaSelecao(i.Codigo, i.Descricao, i.Ordem, i.ExigeObservacao))
                    .ToList()
            })
            .ToListAsync(ct);

        return [.. linhas.Select(c => new CatalogoParaSelecao(
            c.Codigo, c.Nome, c.Descricao, c.PermiteItemNovo, c.Itens))];
    }

    private async Task<CatalogoParaSelecao> LerModelosAsync(CancellationToken ct)
    {
        var itens = await (
            from m in contexto.Modelos
            join f in contexto.Familias on m.FamiliaId equals f.Id
            join ma in contexto.Marcas on f.MarcaId equals ma.Id
            where m.EstaAtivo && f.EstaAtiva && ma.EstaAtiva
            orderby ma.Nome, f.Nome, m.Nome
            select new ItemParaSelecao(m.Codigo, ma.Nome + " · " + f.Nome + " · " + m.Nome, (short)100, false))
            .ToListAsync(ct);

        return new CatalogoParaSelecao(
            CodigoDeModelo,
            "Modelo de equipamento",
            "Marca, família e modelo da máquina. Inclui marca de concorrente, que é o que a tela " +
            "de Cobertura precisa. Tem tabela própria porque o modelo tem atributo além de código " +
            "e descrição — potência e intervalo de manutenção.",
            PermiteItemNovo: true,
            itens);
    }

    private async Task<CatalogoParaSelecao> LerFiliaisAsync(CancellationToken ct)
    {
        // A LEITURA DAS FILIAIS ATRAVESSA A FRONTEIRA de propósito, e pode: organizacao.Empresa
        // é a tabela que DEFINE a fronteira, não uma tabela protegida por ela — quem escolhe a
        // filial de trabalho precisa ver a lista para escolher. O que a filial escolhida limita
        // é o dado de negócio, e isso continua valendo.
        var itens = await contexto.Empresas
            .Where(e => e.EstaAtiva)
            .OrderBy(e => e.Codigo)
            .Select(e => new ItemParaSelecao(e.Codigo, e.Nome, e.Nivel, false))
            .ToListAsync(ct);

        return new CatalogoParaSelecao(
            CodigoDeEmpresa,
            "Filial",
            "As filiais da Tracbel em operação. O código segue o padrão 0101NN, em que NN é o " +
            "número da empresa no sistema legado. É o seletor de contexto de multiempresa.",
            PermiteItemNovo: false,
            itens);
    }
}
