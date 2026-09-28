using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="ConferenciaDaGestaoDeNegocios"/> — <c>integracao.ConferenciaDaGestaoDeNegocios</c>
/// (28/09/2026). Uma linha por indicador, filial e mês; a rodada inteira é regravada.
/// </summary>
public sealed class ConferenciaDaGestaoDeNegociosConfiguracao : IEntityTypeConfiguration<ConferenciaDaGestaoDeNegocios>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ConferenciaDaGestaoDeNegocios> b)
    {
        b.ToTable("ConferenciaDaGestaoDeNegocios", "integracao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Indicador).HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(c => c.ApuradaEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ApuradaPorId).IsRequired();
        b.Ignore(c => c.Diferenca);

        b.HasIndex(c => new { c.SistemaId, c.Indicador, c.EmpresaId, c.Competencia }).IsUnique()
            .HasDatabaseName("UX_ConferenciaDaGestaoDeNegocios_Sistema_Indicador_Empresa_Competencia");
        b.HasIndex(c => new { c.EmpresaId, c.Competencia });
        b.HasIndex(c => c.ApuradaPorId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(c => c.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.ApuradaPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_ConferenciaDaGestaoDeNegocios_Indicador", "[Indicador] IN ('META_MAQUINAS', 'REALIZADO_MAQUINAS')"));
        b.ToTable(x => x.HasCheckConstraint("CK_ConferenciaDaGestaoDeNegocios_Competencia", "DAY([Competencia]) = 1"));
        b.ToTable(x => x.HasCheckConstraint("CK_ConferenciaDaGestaoDeNegocios_Valores", "[NaGestao] >= 0 AND [NoCrm] >= 0"));
    }
}
