using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="GestorDoConsultor"/> — <c>organizacao.GestorDoConsultor</c> (28/09/2026). Uma linha por id da GN;
/// sem <c>EmpresaId</c>, porque o de-para é a definição da casa e o time de um gestor atravessa filiais.
/// </summary>
public sealed class GestorDoConsultorConfiguracao : IEntityTypeConfiguration<GestorDoConsultor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GestorDoConsultor> b)
    {
        b.ToTable("GestorDoConsultor", "organizacao");
        b.HasKey(g => g.Id);
        b.Property(g => g.Id).ValueGeneratedOnAdd();


        b.Property(g => g.ConsultorNaOrigem).HasMaxLength(GestorDoConsultor.TamanhoDaPessoa).IsUnicode(false).IsRequired();
        b.Property(g => g.GestorNaOrigem).HasMaxLength(GestorDoConsultor.TamanhoDaPessoa).IsUnicode(true).IsRequired();
        b.Property(g => g.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(g => g.LidaEm).HasPrecision(3).IsRequired();
        b.Property(g => g.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(g => g.ImportadoPorId).IsRequired();
        b.Property(g => g.ExcluidoEm).HasPrecision(3);

        b.HasIndex(g => new { g.SistemaId, g.IdNaOrigem }).IsUnique().HasDatabaseName("UX_GestorDoConsultor_Sistema_IdNaOrigem");
        b.HasIndex(g => g.ConsultorNaOrigem).HasFilter("[ExcluidoEm] IS NULL").HasDatabaseName("IX_GestorDoConsultor_Consultor");
        b.HasIndex(g => g.ImportadoPorId);

        b.HasOne<Sistema>().WithMany().HasForeignKey(g => g.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(g => g.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_GestorDoConsultor_IdNaOrigem", "[IdNaOrigem] > 0"));
    }
}

/// <summary>
/// Mapeamento de <see cref="ForecastDaGerencia"/> — <c>organizacao.ForecastDaGerencia</c> (28/09/2026). Uma linha por id da
/// GN; o índice é a leitura da rota — um intervalo de meses, com o gestor e a linha.
/// </summary>
public sealed class ForecastDaGerenciaConfiguracao : IEntityTypeConfiguration<ForecastDaGerencia>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForecastDaGerencia> b)
    {
        b.ToTable("ForecastDaGerencia", "organizacao");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();


        b.Property(f => f.GestorNaOrigem).HasMaxLength(GestorDoConsultor.TamanhoDaPessoa).IsUnicode(true).IsRequired();
        b.Property(f => f.LinhaNaOrigem).HasMaxLength(ForecastDaGerencia.TamanhoDaLinha).IsUnicode(true).IsRequired();
        b.Property(f => f.CodigoDaLinha).HasMaxLength(ForecastDaGerencia.TamanhoDaLinha).IsUnicode(false).IsRequired();
        b.Property(f => f.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(f => f.LidaEm).HasPrecision(3).IsRequired();
        b.Property(f => f.GeradaNaOrigemEm).HasPrecision(3);
        b.Property(f => f.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(f => f.ImportadoPorId).IsRequired();
        b.Property(f => f.ExcluidoEm).HasPrecision(3);

        b.HasIndex(f => new { f.SistemaId, f.IdNaOrigem }).IsUnique().HasDatabaseName("UX_ForecastDaGerencia_Sistema_IdNaOrigem");
        b.HasIndex(f => f.Competencia)
            .HasFilter("[ExcluidoEm] IS NULL")
            .IncludeProperties(f => new { f.GestorNaOrigem, f.CodigoDaLinha, f.Forecast, f.BestGuess })
            .HasDatabaseName("IX_ForecastDaGerencia_Competencia");
        b.HasIndex(f => f.ImportadoPorId);

        b.HasOne<Sistema>().WithMany().HasForeignKey(f => f.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(f => f.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_ForecastDaGerencia_Competencia", "DAY([Competencia]) = 1"));
        b.ToTable(x => x.HasCheckConstraint("CK_ForecastDaGerencia_IdNaOrigem", "[IdNaOrigem] > 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_ForecastDaGerencia_Quantidades",
            $"([Forecast] IS NULL OR [Forecast] BETWEEN 0 AND {ForecastDaGerencia.QuantidadeMaxima}) " +
            $"AND ([BestGuess] IS NULL OR [BestGuess] BETWEEN 0 AND {ForecastDaGerencia.QuantidadeMaxima})"));
    }
}

/// <summary>
/// Mapeamento de <see cref="CotaDeConsorcioVendida"/> — <c>organizacao.CotaDeConsorcioVendida</c> (28/09/2026). Uma linha
/// por grupo e cota; o índice é a leitura da meta — a filial num intervalo de meses.
/// </summary>
public sealed class CotaDeConsorcioVendidaConfiguracao : IEntityTypeConfiguration<CotaDeConsorcioVendida>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CotaDeConsorcioVendida> b)
    {
        b.ToTable("CotaDeConsorcioVendida", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(c => c.ChavePublica).IsUnique().HasDatabaseName("UX_CotaDeConsorcioVendida_ChavePublica");

        b.Property(c => c.Grupo).HasMaxLength(CotaDeConsorcioVendida.TamanhoDoCodigo).IsUnicode(false).IsRequired();
        b.Property(c => c.Cota).HasMaxLength(CotaDeConsorcioVendida.TamanhoDoCodigo).IsUnicode(false).IsRequired();
        b.Property(c => c.Contemplacao).HasMaxLength(CotaDeConsorcioVendida.TamanhoDaContemplacao).IsUnicode(true).IsRequired();
        b.Property(c => c.ConsultorNaOrigem).HasMaxLength(GestorDoConsultor.TamanhoDaPessoa).IsUnicode(false).IsRequired();
        b.Property(c => c.GestorNaOrigem).HasMaxLength(GestorDoConsultor.TamanhoDaPessoa).IsUnicode(true).IsRequired();
        b.Property(c => c.Produto).HasMaxLength(CotaDeConsorcioVendida.TamanhoDoProduto).IsUnicode(true);
        b.Property(c => c.ValorDoBem).HasPrecision(18, 2);
        b.Property(c => c.ValorDaParcela).HasPrecision(18, 2);
        b.Property(c => c.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(c => c.LidaEm).HasPrecision(3).IsRequired();
        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.AlteradoEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        b.HasIndex(c => new { c.SistemaId, c.Grupo, c.Cota }).IsUnique().HasDatabaseName("UX_CotaDeConsorcioVendida_Sistema_Grupo_Cota");
        b.HasIndex(c => new { c.EmpresaId, c.Competencia })
            .HasFilter("[ExcluidoEm] IS NULL")
            .IncludeProperties(c => new { c.ConsultorNaOrigem, c.ConsultorUsuarioId, c.GestorNaOrigem })
            .HasDatabaseName("IX_CotaDeConsorcioVendida_Empresa_Competencia");
        b.HasIndex(c => c.ConsultorUsuarioId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(c => c.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.ConsultorUsuarioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_CotaDeConsorcioVendida_Competencia", "DAY([Competencia]) = 1"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CotaDeConsorcioVendida_Valores", "([ValorDoBem] IS NULL OR [ValorDoBem] >= 0) AND ([ValorDaParcela] IS NULL OR [ValorDaParcela] >= 0)"));

        b.Ignore(c => c.Eventos);
    }
}
