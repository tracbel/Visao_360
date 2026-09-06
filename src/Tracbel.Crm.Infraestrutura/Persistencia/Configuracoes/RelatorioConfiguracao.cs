using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Relatorio;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Fonte"/> — schema <c>relatorio</c>.</summary>
public sealed class FonteConfiguracao : IEntityTypeConfiguration<Fonte>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Fonte> b)
    {
        b.ToTable("Fonte", "relatorio");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();

        b.Property(f => f.Codigo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(f => f.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(f => f.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(f => f.NomeDaVisao).HasMaxLength(120).IsUnicode(false).IsRequired();
        b.Property(f => f.EntidadeRaiz).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(f => f.EhMaterializada).IsRequired();
        b.Property(f => f.AtualizadaEm).HasPrecision(3);
        b.Property(f => f.EstaAtiva).IsRequired();

        b.HasIndex(f => f.Codigo).IsUnique().HasDatabaseName("UX_Fonte_Codigo");
        b.HasIndex(f => f.SistemaId);

        b.HasOne<Sistema>().WithMany().HasForeignKey(f => f.SistemaId).OnDelete(DeleteBehavior.Restrict);

        // Fonte materializada mostra o carimbo da última atualização na tela — sem ele, o
        // usuário não sabe se está olhando dado de hoje ou de três semanas atrás. [V] é
        // exatamente o que falta nas tabelas de relatório do Vórtice, que são escritas,
        // editáveis e sem carimbo.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Fonte_Materializada", "[EhMaterializada] = 0 OR [AtualizadaEm] IS NOT NULL"));
    }
}

/// <summary>Mapeamento de <see cref="FonteCampo"/>.</summary>
public sealed class FonteCampoConfiguracao : IEntityTypeConfiguration<FonteCampo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FonteCampo> b)
    {
        b.ToTable("FonteCampo", "relatorio");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Campo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.Rotulo).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(c => c.TipoDeDado).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.PermiteAgrupar).IsRequired();
        b.Property(c => c.PermiteFiltrar).IsRequired();
        b.Property(c => c.PermiteSomar).IsRequired();
        b.Property(c => c.Ordem).IsRequired();

        b.HasIndex(c => new { c.FonteId, c.Campo }).IsUnique().HasDatabaseName("UX_FonteCampo_Fonte_Campo");

        b.HasOne<Fonte>().WithMany().HasForeignKey(c => c.FonteId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_FonteCampo_TipoDeDado",
            "[TipoDeDado] IN ('Texto','Numero','Data','Booleano','Moeda','Percentual')"));
    }
}

/// <summary>
/// Mapeamento de <see cref="Dominio.Relatorio.Relatorio"/>.
///
/// A definição é JSON binário — NUNCA SQL. [V] o motor de relatório do Vórtice guarda SQL
/// cru numa coluna de texto e aceita comandos de alteração; aqui o usuário não tem como
/// escrever consulta, e por isso não tem como furar a segurança por linha.
/// </summary>
public sealed class RelatorioConfiguracao : IEntityTypeConfiguration<Dominio.Relatorio.Relatorio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Dominio.Relatorio.Relatorio> b)
    {
        b.ToTable("Relatorio", "relatorio");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();

        b.Property(r => r.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(r => r.ChavePublica).IsUnique().HasDatabaseName("UX_Relatorio_ChavePublica");

        b.Property(r => r.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(r => r.Definicao).HasColumnType("nvarchar(max)").IsRequired();
        b.ToTable(x => x.HasCheckConstraint("CK_Relatorio_DefinicaoJson", "ISJSON([Definicao]) = 1"));
        b.Property(r => r.Visibilidade).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        b.Property(r => r.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(r => r.AlteradoEm).HasPrecision(3);
        b.Property(r => r.ExcluidoEm).HasPrecision(3);

        b.HasIndex(r => new { r.ProprietarioId, r.Nome }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(r => new { r.EmpresaId, r.Visibilidade }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(r => r.FonteId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(r => r.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Fonte>().WithMany().HasForeignKey(r => r.FonteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(r => r.ProprietarioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Relatorio_Visibilidade", "[Visibilidade] IN ('Privado','Equipe','Empresa')"));

        b.Ignore(r => r.Eventos);
    }
}
