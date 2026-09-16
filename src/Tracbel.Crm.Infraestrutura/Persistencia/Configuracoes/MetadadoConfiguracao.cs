using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Catalogo"/> — schema <c>metadado</c>.</summary>
public sealed class CatalogoConfiguracao : IEntityTypeConfiguration<Catalogo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Catalogo> b)
    {
        b.ToTable("Catalogo", "metadado");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.PermiteItemNovo).IsRequired();
        b.Property(c => c.EstaAtivo).IsRequired();

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Catalogo_Codigo");

        // OS CATÁLOGOS QUE O ESQUEMA REFERENCIA, com identificador fixo — ver
        // Dominio.Metadado.CatalogosDeSistema para o porquê de o Id ser constante aqui e
        // gerado pelo banco em todo o resto (documento 14, regra 8.3).
        //
        // Sem estas oito linhas, as chaves estrangeiras compostas de papel apontariam para um
        // catálogo que não existe, e nenhuma delas poderia ser satisfeita.
        b.HasData(CatalogosDeSistema.Todos.Select(c => new
        {
            c.Id,
            c.Codigo,
            c.Nome,
            Descricao = (string?)c.Descricao,
            PermiteItemNovo = true,
            EstaAtivo = true
        }));
    }
}

/// <summary>
/// Mapeamento de <see cref="CatalogoItem"/>.
///
/// A DESCRIÇÃO USA COLLATION NÃO DETERMINÍSTICA, e é aí que mora a garantia central deste
/// schema: com unicidade por catálogo sobre essa coluna, "Preço" e "PRECO" passam a ser o
/// mesmo item — o banco recusa o segundo. [V] é exatamente o defeito que fez FINALIZADO
/// (18.416 linhas) conviver com FINALIZADA (11.563) no Vórtice, que tem zero restrições de
/// verificação em 767 tabelas.
/// </summary>
public sealed class CatalogoItemConfiguracao : IEntityTypeConfiguration<CatalogoItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CatalogoItem> b)
    {
        b.ToTable("CatalogoItem", "metadado");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();

        b.Property(i => i.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();

        b.Property(i => i.Descricao)
            .HasMaxLength(200)
            .IsUnicode(true)
            .UseCollation(CrmDbContext.ColacaoSemCaixaNemAcento)
            .IsRequired();

        b.Property(i => i.Ordem).IsRequired();
        b.Property(i => i.ExigeObservacao).IsRequired();
        b.Property(i => i.EstaAtivo).IsRequired();
        b.Property(i => i.UltimoUsoEm).HasPrecision(3);

        b.HasIndex(i => new { i.CatalogoId, i.Codigo })
            .IsUnique()
            .HasDatabaseName("UX_CatalogoItem_Catalogo_Codigo");

        // A unicidade que fecha o domínio de verdade: dois rótulos que só diferem em caixa ou
        // acento são o MESMO rótulo, e o segundo não entra.
        b.HasIndex(i => new { i.CatalogoId, i.Descricao })
            .IsUnique()
            .HasDatabaseName("UX_CatalogoItem_Catalogo_Descricao");

        // Chave candidata que permite, mais adiante, a chave estrangeira COMPOSTA descrita na
        // seção 8.11 do documento 17 — a que amarra uma coluna a um catálogo específico.
        b.HasAlternateKey(i => new { i.CatalogoId, i.Id })
            .HasName("AK_CatalogoItem_CatalogoId");

        b.HasIndex(i => new { i.CatalogoId, i.Ordem }).HasFilter("[EstaAtivo] = 1");
        b.HasIndex(i => new { i.CatalogoId, i.ItemPaiId });

        b.HasOne<Catalogo>().WithMany().HasForeignKey(i => i.CatalogoId).OnDelete(DeleteBehavior.Restrict);

        // O PAI É DO MESMO CATÁLOGO. A chave estrangeira composta contra a mesma chave
        // alternativa usada pelas colunas de papel — de graça, pelo mesmo mecanismo: sem ela,
        // um item de CULTURA podia ter como pai um item de TIPO_DOCUMENTO.
        b.HasOne<CatalogoItem>()
            .WithMany()
            .HasForeignKey(i => new { i.CatalogoId, i.ItemPaiId })
            .HasPrincipalKey(i => new { i.CatalogoId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_CatalogoItem_Ordem", "[Ordem] >= 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CatalogoItem_Pai", "[ItemPaiId] IS NULL OR [ItemPaiId] <> [Id]"));
    }
}

