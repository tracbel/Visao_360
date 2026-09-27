using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// O preço de uma categoria de máquina num mês (issue 70, D-P12).
///
/// <para>A chave natural é <b>(CategoriaDeMaquinaId, Mes)</b>, com índice único: a carga diária relê os meses
/// que já estão gravados, e sem ele cada rodada os empilharia.</para>
///
/// <para><c>decimal(18,2)</c>: é reais de nota fiscal.</para>
/// </summary>
public sealed class PrecoDeMaquinaNoMesConfiguracao : IEntityTypeConfiguration<PrecoDeMaquinaNoMes>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PrecoDeMaquinaNoMes> b)
    {
        b.ToTable("PrecoDeMaquinaNoMes", "organizacao");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.CategoriaDeMaquinaId).IsRequired();
        b.Property(p => p.Mes).IsRequired();
        b.Property(p => p.Mediana).HasPrecision(18, 2).IsRequired();
        b.Property(p => p.Menor).HasPrecision(18, 2).IsRequired();
        b.Property(p => p.Maior).HasPrecision(18, 2).IsRequired();
        b.Property(p => p.Notas).IsRequired();
        b.Property(p => p.Fonte).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(p => p.ImportadoPorId).IsRequired();

        b.HasIndex(p => new { p.CategoriaDeMaquinaId, p.Mes })
            .IsUnique()
            .HasDatabaseName("UX_PrecoDeMaquinaNoMes_Categoria_Mes");

        b.HasIndex(p => p.ImportadoPorId);

        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(p => p.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(p => p.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_PrecoDeMaquinaNoMes_Valores", "[Menor] > 0 AND [Menor] <= [Mediana] AND [Mediana] <= [Maior]"));
        b.ToTable(t => t.HasCheckConstraint("CK_PrecoDeMaquinaNoMes_Notas", "[Notas] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_PrecoDeMaquinaNoMes_Mes", "DAY([Mes]) = 1"));
    }
}
