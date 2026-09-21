using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// O custo de produção por cultura, local e safra, da CONAB (issue 67).
///
/// <para>A chave natural é <b>(Cultura, Aba)</b>: a CONAB às vezes publica dois relatórios no mesmo ano
/// para o mesmo local, e a aba é o que os distingue. Sem o índice único, a carga mensal empilharia as
/// 166 abas a cada rodada.</para>
///
/// <para>Hectare em <c>decimal(18,2)</c>; unidade em <c>decimal(18,5)</c>, porque a CONAB publica o
/// custo por tonelada de cana com cinco casas.</para>
/// </summary>
public sealed class CustoDeProducaoConfiguracao : IEntityTypeConfiguration<CustoDeProducao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustoDeProducao> b)
    {
        b.ToTable("CustoDeProducao", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Cultura).HasMaxLength(40).IsUnicode(true).IsRequired();
        b.Property(c => c.Aba).HasMaxLength(60).IsUnicode(true).IsRequired();
        b.Property(c => c.Local).HasMaxLength(60).IsUnicode(true).IsRequired();
        b.Property(c => c.Variante).HasMaxLength(20).IsUnicode(true);
        b.Property(c => c.Safra).IsRequired();
        b.Property(c => c.UnidadeComercial).HasMaxLength(30).IsUnicode(true).IsRequired();
        b.Property(c => c.Produtividade).HasPrecision(18, 2);
        b.Property(c => c.UnidadeDaProdutividade).HasMaxLength(20).IsUnicode(false);

        foreach (var ha in new[] { nameof(CustoDeProducao.CustoVariavelHa), nameof(CustoDeProducao.CustoFixoHa),
                     nameof(CustoDeProducao.CustoOperacionalHa), nameof(CustoDeProducao.RendaDeFatoresHa), nameof(CustoDeProducao.CustoTotalHa) })
            b.Property(ha).HasPrecision(18, 2);

        foreach (var unidade in new[] { nameof(CustoDeProducao.CustoVariavelUnidade), nameof(CustoDeProducao.CustoFixoUnidade),
                     nameof(CustoDeProducao.CustoOperacionalUnidade), nameof(CustoDeProducao.RendaDeFatoresUnidade), nameof(CustoDeProducao.CustoTotalUnidade) })
            b.Property(unidade).HasPrecision(18, 5);

        b.Property(c => c.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ImportadoPorId).IsRequired();

        b.HasIndex(c => new { c.Cultura, c.Aba }).IsUnique().HasDatabaseName("UX_CustoDeProducao_Cultura_Aba");

        // A consulta da tela: "a série de custo desta cultura neste local".
        b.HasIndex(c => new { c.Cultura, c.Local, c.Safra });
        b.HasIndex(c => c.MunicipioId);
        b.HasIndex(c => c.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(c => c.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_CustoDeProducao_Operacional", "[CustoOperacionalHa] > 0 AND [CustoOperacionalUnidade] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_CustoDeProducao_Total", "[CustoTotalHa] IS NULL OR [CustoTotalHa] >= [CustoOperacionalHa]"));
        b.ToTable(t => t.HasCheckConstraint("CK_CustoDeProducao_Mes", "[MesDoRelatorio] IS NULL OR [MesDoRelatorio] BETWEEN 1 AND 12"));
    }
}
