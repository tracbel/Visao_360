using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// O crédito rural de investimento do SICOR (issue 68).
///
/// <para>O índice único cobre a combinação inteira — dez colunas inteiras, bem abaixo do limite do SQL
/// Server. Sem ele, a releitura mensal dos dois últimos anos empilharia ~25 mil linhas a cada rodada.</para>
///
/// <para>O segundo índice é o da tela: "o crédito deste município, por período e produto".</para>
/// </summary>
public sealed class CreditoRuralDeInvestimentoConfiguracao : IEntityTypeConfiguration<CreditoRuralDeInvestimento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CreditoRuralDeInvestimento> b)
    {
        b.ToTable("CreditoRuralDeInvestimento", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Valor).HasPrecision(18, 2).IsRequired();
        b.Property(c => c.Area).HasPrecision(18, 2).IsRequired();
        b.Property(c => c.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ImportadoPorId).IsRequired();

        b.Ignore(c => c.ChaveNatural);

        b.HasIndex(c => new
            {
                c.CodigoMunicipioBcb, c.Ano, c.Mes, c.CodigoProduto, c.CodigoPrograma,
                c.CodigoSubprograma, c.CodigoFonte, c.CodigoSeguro, c.Atividade, c.CodigoModalidade
            })
            .IsUnique()
            .HasDatabaseName("UX_CreditoRuralDeInvestimento_Combinacao");

        b.HasIndex(c => new { c.MunicipioId, c.Ano, c.Mes, c.CodigoProduto })
            .IncludeProperties(c => new { c.Valor })
            .HasDatabaseName("IX_CreditoRuralDeInvestimento_Municipio_Periodo");

        b.HasIndex(c => new { c.Ano, c.Mes, c.CodigoProduto })
            .IncludeProperties(c => new { c.Valor, c.MunicipioId })
            .HasDatabaseName("IX_CreditoRuralDeInvestimento_Periodo_Produto");

        b.HasIndex(c => c.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(c => c.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_CreditoRuralDeInvestimento_Valor", "[Valor] > 0 AND [Area] >= 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_CreditoRuralDeInvestimento_Mes", "[Mes] BETWEEN 1 AND 12"));
    }
}

/// <summary>As tabelas auxiliares do SICOR (issue 68). Um item por (tipo, programa-pai, código).</summary>
public sealed class ItemDoSicorConfiguracao : IEntityTypeConfiguration<ItemDoSicor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ItemDoSicor> b)
    {
        b.ToTable("ItemDoSicor", "organizacao");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();

        b.Property(i => i.Tipo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(i => i.Descricao).HasMaxLength(300).IsUnicode(true).IsRequired();
        b.Property(i => i.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(i => i.ImportadoPorId).IsRequired();

        b.HasIndex(i => new { i.Tipo, i.CodigoPai, i.Codigo }).IsUnique().HasDatabaseName("UX_ItemDoSicor_Tipo_Codigo");
        b.HasIndex(i => i.ImportadoPorId);

        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(i => i.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ItemDoSicor_Tipo", "[Tipo] IN ('PROGRAMA', 'SUBPROGRAMA', 'FONTE', 'PRODUTO')"));
    }
}
