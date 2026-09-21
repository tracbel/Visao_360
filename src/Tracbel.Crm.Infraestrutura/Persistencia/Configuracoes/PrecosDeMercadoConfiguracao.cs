using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// O preço de um produto num mês (issue 66).
///
/// <para>A chave natural é <b>(Fonte, CodigoNaFonte, Praca, Nivel, Mes)</b>, com índice único: a
/// carga mensal relê 12 meses que já estão gravados, e sem esse índice cada rodada os empilharia.</para>
///
/// <para><c>decimal(18,4)</c>: o kg de ATR tem quatro casas (R$ 0,8692) e é a série de menor valor; a
/// CONAB publica duas.</para>
/// </summary>
public sealed class CotacaoDeProdutoConfiguracao : IEntityTypeConfiguration<CotacaoDeProduto>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CotacaoDeProduto> b)
    {
        b.ToTable("CotacaoDeProduto", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Fonte).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.CodigoNaFonte).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.Praca).HasMaxLength(10).IsUnicode(false).IsRequired();
        b.Property(c => c.Nivel).HasMaxLength(40).IsUnicode(true).IsRequired();
        b.Property(c => c.Produto).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(c => c.Classificacao).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Unidade).HasMaxLength(20).IsUnicode(true).IsRequired();
        b.Property(c => c.Mes).IsRequired();
        b.Property(c => c.ValorEmReais).HasPrecision(18, 4).IsRequired();
        b.Property(c => c.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ImportadoPorId).IsRequired();

        b.HasIndex(c => new { c.Fonte, c.CodigoNaFonte, c.Praca, c.Nivel, c.Mes })
            .IsUnique()
            .HasDatabaseName("UX_CotacaoDeProduto_Serie_Mes");

        b.HasIndex(c => c.ImportadoPorId);

        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_CotacaoDeProduto_Valor", "[ValorEmReais] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_CotacaoDeProduto_Mes", "DAY([Mes]) = 1"));
    }
}

/// <summary>
/// O dólar PTAX de um mês (issue 66). Uma linha por mês — o índice único é sobre ele.
/// </summary>
public sealed class CotacaoDoDolarConfiguracao : IEntityTypeConfiguration<CotacaoDoDolar>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CotacaoDoDolar> b)
    {
        b.ToTable("CotacaoDoDolar", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Mes).IsRequired();
        b.Property(c => c.ReaisPorDolar).HasPrecision(18, 4).IsRequired();
        b.Property(c => c.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ImportadoPorId).IsRequired();

        b.HasIndex(c => c.Mes).IsUnique().HasDatabaseName("UX_CotacaoDoDolar_Mes");
        b.HasIndex(c => c.ImportadoPorId);

        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_CotacaoDoDolar_Valor", "[ReaisPorDolar] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_CotacaoDoDolar_Mes", "DAY([Mes]) = 1"));
    }
}
