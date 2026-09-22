using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="Cultura"/> — o catálogo que liga o vocabulário das cinco fontes (issue 165).
///
/// <para><b>A semente é o anexo 49C</b>, medido nas fontes em 22/09/2026: as seis culturas que já estavam
/// nas telas de preço e de custo como lista fixa no código. Semear é o que permite tirar a lista de lá sem
/// a tela ficar vazia no dia da publicação.</para>
/// </summary>
public sealed class CulturaConfiguracao : IEntityTypeConfiguration<Cultura>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Cultura> b)
    {
        b.ToTable("Cultura", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(Cultura.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(c => c.Segmento).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.UnidadeComercial).HasMaxLength(Cultura.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(c => c.QuilosPorUnidade).HasPrecision(10, 3).IsRequired();
        b.Property(c => c.FonteDoPreco).HasMaxLength(20).IsUnicode(false);
        b.Property(c => c.ProdutoDoPreco).HasMaxLength(40).IsUnicode(false);
        b.Property(c => c.SerieDeCusto).HasMaxLength(Cultura.TamanhoDoTexto).IsUnicode(true);
        b.Property(c => c.EstaAtiva).IsRequired();

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Cultura_Codigo");

        // O DOMÍNIO FECHADO NO BANCO (documento 14, seções 4 e 6), e não só no compilador.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cultura_Segmento",
            "[Segmento] IN ('Graos','Cana','Citros','Cafe','Fruticultura','Olericultura','Algodao','Borracha','Outros')"));

        b.ToTable(t => t.HasCheckConstraint("CK_Cultura_QuilosPorUnidade", "[QuilosPorUnidade] > 0"));

        // FONTE E PRODUTO ANDAM JUNTOS: a mesma regra do domínio, dita também aqui.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cultura_FonteDoPreco",
            "([FonteDoPreco] IS NULL AND [ProdutoDoPreco] IS NULL) OR ([FonteDoPreco] IS NOT NULL AND [ProdutoDoPreco] IS NOT NULL)"));

        b.HasData(CatalogoSemeado.Culturas.Select((c, posicao) => new
        {
            Id = posicao + 1,
            c.Codigo,
            c.Nome,
            c.Segmento,
            c.UnidadeComercial,
            c.QuilosPorUnidade,
            c.FonteDoPreco,
            c.ProdutoDoPreco,
            c.SerieDeCusto,
            EstaAtiva = true
        }));
    }
}

/// <summary>
/// Mapeamento de <see cref="ProdutoDaPamNaCultura"/> — os produtos da classificação 782 de cada cultura.
///
/// <para>O índice único é sobre o <b>código do produto</b>, e não sobre o par: o mesmo produto em duas
/// culturas somaria a mesma terra duas vezes.</para>
/// </summary>
public sealed class ProdutoDaPamNaCulturaConfiguracao : IEntityTypeConfiguration<ProdutoDaPamNaCultura>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProdutoDaPamNaCultura> b)
    {
        b.ToTable("ProdutoDaPamNaCultura", "organizacao");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.CulturaId).IsRequired();
        b.Property(p => p.ProdutoCodigoIbge).IsRequired();
        b.Property(p => p.ProdutoNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(p => p.EntraNaSomaDaLavoura).IsRequired();

        b.HasIndex(p => p.ProdutoCodigoIbge).IsUnique().HasDatabaseName("UX_ProdutoDaPamNaCultura_Produto");
        b.HasIndex(p => p.CulturaId);

        // RESTRICT, E NÃO CASCADE: cultura não se apaga, se desliga (EstaAtiva). Cascata aqui seria uma porta
        // para o vínculo sumir junto com uma linha que o modelo nem prevê apagar.
        b.HasOne<Cultura>().WithMany().HasForeignKey(p => p.CulturaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ProdutoDaPamNaCultura_Produto", "[ProdutoCodigoIbge] > 0"));

        var linhas = new List<object>();
        var id = 1;
        for (var i = 0; i < CatalogoSemeado.Culturas.Count; i++)
            foreach (var (codigo, nome, naSoma) in CatalogoSemeado.Culturas[i].Produtos)
                linhas.Add(new
                {
                    Id = id++,
                    CulturaId = i + 1,
                    ProdutoCodigoIbge = codigo,
                    ProdutoNome = nome,
                    EntraNaSomaDaLavoura = naSoma
                });

        b.HasData(linhas);
    }
}

/// <summary>
/// Mapeamento de <see cref="CategoriaDeMaquina"/> — as seis categorias do pedido (D-IM-06, issue 165).
/// </summary>
public sealed class CategoriaDeMaquinaConfiguracao : IEntityTypeConfiguration<CategoriaDeMaquina>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CategoriaDeMaquina> b)
    {
        b.ToTable("CategoriaDeMaquina", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(Cultura.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(c => c.Ordem).IsRequired();
        b.Property(c => c.EstaAtiva).IsRequired();

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_CategoriaDeMaquina_Codigo");

        b.HasData(CatalogoSemeado.Categorias.Select((c, posicao) => new
        {
            Id = posicao + 1,
            c.Codigo,
            c.Nome,
            c.Ordem,
            EstaAtiva = true
        }));
    }
}

/// <summary>
/// Mapeamento de <see cref="ProdutoDoSicorNaCategoria"/> — o que tira a lista <c>[7080, 4860, 2700]</c>
/// do código, onde a issue 157 a deixou de passagem.
///
/// <para>Três das seis categorias nascem <b>sem produto</b>: o investimento do Banco Central não separa
/// plantadeira, pulverizador nem agricultura de precisão (anexo 49C).</para>
/// </summary>
public sealed class ProdutoDoSicorNaCategoriaConfiguracao : IEntityTypeConfiguration<ProdutoDoSicorNaCategoria>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProdutoDoSicorNaCategoria> b)
    {
        b.ToTable("ProdutoDoSicorNaCategoria", "organizacao");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.CategoriaDeMaquinaId).IsRequired();
        b.Property(p => p.CodigoProduto).IsRequired();
        b.Property(p => p.Descricao).HasMaxLength(200).IsUnicode(true).IsRequired();

        b.HasIndex(p => p.CodigoProduto).IsUnique().HasDatabaseName("UX_ProdutoDoSicorNaCategoria_Produto");
        b.HasIndex(p => p.CategoriaDeMaquinaId);

        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(p => p.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ProdutoDoSicorNaCategoria_Produto", "[CodigoProduto] > 0"));

        var linhas = new List<object>();
        var id = 1;
        for (var i = 0; i < CatalogoSemeado.Categorias.Count; i++)
            foreach (var (codigo, descricao) in CatalogoSemeado.Categorias[i].ProdutosDoSicor)
                linhas.Add(new
                {
                    Id = id++,
                    CategoriaDeMaquinaId = i + 1,
                    CodigoProduto = codigo,
                    Descricao = descricao
                });

        b.HasData(linhas);
    }
}
