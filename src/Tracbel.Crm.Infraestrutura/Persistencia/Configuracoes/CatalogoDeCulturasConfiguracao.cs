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

        // A REFERÊNCIA DO CUSTO (D-P07, issue 159), em aberto: nasce nula, e a margem sai vazia com o
        // motivo em vez de escolher um local da CONAB por conta própria.
        b.Property(c => c.LocalDeReferenciaDoCusto).HasMaxLength(Cultura.TamanhoDoTexto).IsUnicode(true);
        b.Property(c => c.CamadaDeCustoDaMargem).HasConversion<string>().HasMaxLength(20).IsUnicode(false);

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Cultura_Codigo");

        // O DOMÍNIO FECHADO NO BANCO (documento 14, seções 4 e 6), e não só no compilador.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cultura_Segmento",
            "[Segmento] IN ('Graos','Cana','Citros','Cafe','Fruticultura','Olericultura','Algodao','Borracha','Outros')"));

        b.ToTable(t => t.HasCheckConstraint("CK_Cultura_QuilosPorUnidade", "[QuilosPorUnidade] > 0"));

        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cultura_CamadaDeCusto", "[CamadaDeCustoDaMargem] IS NULL OR [CamadaDeCustoDaMargem] IN ('Operacional','Total')"));

        // LOCAL E CAMADA ANDAM JUNTOS: a mesma regra do domínio, dita também no banco.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cultura_ReferenciaDoCusto",
            "([LocalDeReferenciaDoCusto] IS NULL AND [CamadaDeCustoDaMargem] IS NULL) " +
            "OR ([LocalDeReferenciaDoCusto] IS NOT NULL AND [CamadaDeCustoDaMargem] IS NOT NULL)"));

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

/// <summary>
/// Mapeamento de <see cref="GrupoDeCompartilhamento"/> e de
/// <see cref="CulturaNoGrupoDeCompartilhamento"/> — as culturas que dividem a mesma terra e a mesma
/// máquina (issue 160, D-IM-01).
///
/// <para><b>As duas tabelas nascem VAZIAS.</b> Quais culturas compartilham é decisão que não saiu; sem
/// grupo configurado, cada cultura soma a área dela — exatamente como era antes.</para>
/// </summary>
public sealed class GrupoDeCompartilhamentoConfiguracao : IEntityTypeConfiguration<GrupoDeCompartilhamento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GrupoDeCompartilhamento> b)
    {
        b.ToTable("GrupoDeCompartilhamento", "organizacao");
        b.HasKey(g => g.Id);
        b.Property(g => g.Id).ValueGeneratedOnAdd();

        b.Property(g => g.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(g => g.Nome).HasMaxLength(Cultura.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(g => g.CategoriaDeMaquinaId).IsRequired();
        b.Property(g => g.EstaAtivo).IsRequired();

        b.HasIndex(g => g.Codigo).IsUnique().HasDatabaseName("UX_GrupoDeCompartilhamento_Codigo");
        b.HasIndex(g => g.CategoriaDeMaquinaId);

        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(g => g.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Mapeamento de <see cref="CulturaNoGrupoDeCompartilhamento"/> — a cultura dentro do grupo.</summary>
public sealed class CulturaNoGrupoDeCompartilhamentoConfiguracao : IEntityTypeConfiguration<CulturaNoGrupoDeCompartilhamento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CulturaNoGrupoDeCompartilhamento> b)
    {
        b.ToTable("CulturaNoGrupoDeCompartilhamento", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.GrupoDeCompartilhamentoId).IsRequired();
        b.Property(c => c.CulturaId).IsRequired();

        // A MESMA CULTURA DUAS VEZES NO MESMO GRUPO seria a área contada duas vezes dentro do grupo.
        b.HasIndex(c => new { c.GrupoDeCompartilhamentoId, c.CulturaId })
            .IsUnique()
            .HasDatabaseName("UX_CulturaNoGrupoDeCompartilhamento_Grupo_Cultura");

        b.HasIndex(c => c.CulturaId);

        b.HasOne<GrupoDeCompartilhamento>().WithMany().HasForeignKey(c => c.GrupoDeCompartilhamentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cultura>().WithMany().HasForeignKey(c => c.CulturaId).OnDelete(DeleteBehavior.Restrict);
    }
}
/// <summary>
/// Mapeamento de <see cref="LinhaDeProdutoNaCategoria"/> — o último elo entre a venda de máquina e a
/// categoria de mercado (issue 69, D-P08).
///
/// <para><b>Oito linhas nascem ligadas e duas nascem sem categoria</b>, de propósito. As oito são
/// mecânicas — três tratores para Trator, a colheitadeira para Colheitadeira, a plantadeira, o
/// pulverizador e os dois implementos. As duas que ficam de fora são JULGAMENTO DE NEGÓCIO, e o comercial
/// decide:</para>
///
/// <list type="bullet">
///   <item><c>COLHEDORA_DE_CANA</c> — colher cana é outra máquina que colher grão, e o catálogo tem uma
///   categoria de colheitadeira só. Ligá-la a "Colheitadeira" mistura dois mercados; deixá-la fora tira da
///   captura por categoria justamente a máquina mais vendida na região da cana.</item>
///   <item><c>PLATAFORMA_DE_CORTE</c> — é acessório de colheitadeira, e não máquina que o produtor compra
///   sozinha. Contá-la como implemento infla a contagem de implementos com peça de outra máquina.</item>
/// </list>
///
/// <para>Enquanto as duas estiverem sem categoria, a venda delas continua no total e some só da leitura
/// POR categoria — com o nome da linha dito na tela.</para>
/// </summary>
public sealed class LinhaDeProdutoNaCategoriaConfiguracao : IEntityTypeConfiguration<LinhaDeProdutoNaCategoria>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LinhaDeProdutoNaCategoria> b)
    {
        b.ToTable("LinhaDeProdutoNaCategoria", "organizacao");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.CategoriaDeMaquinaId).IsRequired();
        // O CÓDIGO É ASCII e comparado byte a byte: ele vem do `Codificar` da carga, que já tira acento e
        // caixa. Colação binária evita que "TRATOR_MEDIO" e "trator_medio" virem duas linhas.
        b.Property(p => p.CodigoDaLinha).HasMaxLength(80).IsUnicode(false)
            .UseCollation("Latin1_General_BIN2").IsRequired();
        b.Property(p => p.Descricao).HasMaxLength(200).IsUnicode(true).IsRequired();

        // UMA LINHA PERTENCE A UMA CATEGORIA SÓ: a mesma linha em duas contaria a venda duas vezes.
        b.HasIndex(p => p.CodigoDaLinha).IsUnique().HasDatabaseName("UX_LinhaDeProdutoNaCategoria_Linha");
        b.HasIndex(p => p.CategoriaDeMaquinaId);

        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(p => p.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);

        // AS OITO MECÂNICAS. Os ids das categorias são os semeados na issue 165: 1 Trator, 2 Plantadeira,
        // 3 Colheitadeira, 4 Pulverizador, 5 Implemento, 6 Agricultura de precisão.
        b.HasData(
            new { Id = 1, CategoriaDeMaquinaId = 1, CodigoDaLinha = "TRATOR_PEQUENO", Descricao = "Trator pequeno" },
            new { Id = 2, CategoriaDeMaquinaId = 1, CodigoDaLinha = "TRATOR_MEDIO", Descricao = "Trator médio" },
            new { Id = 3, CategoriaDeMaquinaId = 1, CodigoDaLinha = "TRATOR_GRANDE", Descricao = "Trator grande" },
            new { Id = 4, CategoriaDeMaquinaId = 3, CodigoDaLinha = "COLHEITADEIRA", Descricao = "Colheitadeira" },
            new { Id = 5, CategoriaDeMaquinaId = 2, CodigoDaLinha = "PLANTADEIRA", Descricao = "Plantadeira" },
            new { Id = 6, CategoriaDeMaquinaId = 4, CodigoDaLinha = "PULVERIZADOR", Descricao = "Pulverizador" },
            new { Id = 7, CategoriaDeMaquinaId = 5, CodigoDaLinha = "IMPLEMENTO_JOHN_DEERE", Descricao = "Implemento John Deere" },
            new { Id = 8, CategoriaDeMaquinaId = 5, CodigoDaLinha = "IMPLEMENTO_OUTRAS_MARCAS", Descricao = "Implemento de outras marcas" });
    }
}
