using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// O financiamento das vendas de máquina, dos formulários do Vórtice (issue 262).
///
/// <para>Um por processo: o índice único é a chave natural. O segundo índice é o da leitura do share — o crédito rural
/// vigente, por data e município, com o valor incluído.</para>
/// </summary>
public sealed class FinanciamentoDaVendaConfiguracao : IEntityTypeConfiguration<FinanciamentoDaVenda>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FinanciamentoDaVenda> b)
    {
        b.ToTable("FinanciamentoDaVenda", "organizacao");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();

        b.Property(f => f.Formulario).HasMaxLength(FinanciamentoDaVenda.TamanhoDoFormulario).IsUnicode(false).IsRequired();
        b.Property(f => f.ValorFinanciado).HasPrecision(18, 2).IsRequired();
        b.Property(f => f.InstituicaoFinanceira).HasMaxLength(FinanciamentoDaVenda.TamanhoDoTexto).IsUnicode(true);
        b.Property(f => f.LinhaDeCredito).HasMaxLength(FinanciamentoDaVenda.TamanhoDoTexto).IsUnicode(true);
        b.Property(f => f.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsFixedLength().IsRequired();
        b.Property(f => f.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(f => f.ImportadoPorId).IsRequired();
        b.Property(f => f.ExcluidoEm).HasPrecision(3);

        b.HasIndex(f => f.ProcessoNoVortice).IsUnique().HasDatabaseName("UX_FinanciamentoDaVenda_Processo");

        b.HasIndex(f => new { f.ContaNoCreditoRural, f.PedidoEm, f.MunicipioId })
            .IncludeProperties(f => new { f.ValorFinanciado, f.LinhaDeCredito })
            .HasFilter("[ExcluidoEm] IS NULL")
            .HasDatabaseName("IX_FinanciamentoDaVenda_Credito_Periodo");

        b.HasIndex(f => f.MunicipioId);
        b.HasIndex(f => f.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(f => f.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(f => f.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_FinanciamentoDaVenda_Valor", "[ValorFinanciado] > 0 AND [ValorFinanciado] <= 20000000"));
        b.ToTable(t => t.HasCheckConstraint("CK_FinanciamentoDaVenda_Processo", "[ProcessoNoVortice] > 0"));
    }
}
