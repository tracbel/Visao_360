using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="AlteracaoDeCampo"/>.
///
/// PARTICIONADA POR MÊS na migração inicial, com 18 meses disponíveis. A chave primária
/// inclui a coluna de particionamento porque o SQL Server exige a coluna de particionamento
/// na chave de todo índice único da tabela particionada.
/// </summary>
public sealed class AlteracaoDeCampoConfiguracao : IEntityTypeConfiguration<AlteracaoDeCampo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AlteracaoDeCampo> b)
    {
        b.ToTable("AlteracaoDeCampo", "auditoria");
        b.HasKey(a => new { a.Id, a.AlteradoEm });
        b.Property(a => a.Id).ValueGeneratedOnAdd();

        b.Property(a => a.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(a => a.Campo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(a => a.ValorAnterior).HasMaxLength(400).IsUnicode(true);
        b.Property(a => a.ValorNovo).HasMaxLength(400).IsUnicode(true);
        b.Property(a => a.AlteradoEm).HasPrecision(3).IsRequired();

        // "O que mudou neste registro, do mais recente para trás" — a consulta da aba de
        // histórico.
        b.HasIndex(a => new { a.Entidade, a.RegistroId, a.AlteradoEm }).IsDescending(false, false, true);
        b.HasIndex(a => a.CorrelacaoId);
        b.HasIndex(a => a.AlteradoPorId);
        b.HasIndex(a => a.EmpresaId);

        // DE ONDE VEIO E O QUE ACONTECEU (documento 41, fase 2). Texto, e não número, para a trilha
        // ser legível no SSMS sem o enum do lado — é a tabela que alguém abre quando um dado "mudou
        // sozinho".
        b.Property(a => a.Origem).HasConversion<string>().HasMaxLength(12).IsUnicode(false).IsRequired();
        b.Property(a => a.Operacao).HasConversion<string>().HasMaxLength(10).IsUnicode(false).IsRequired();

        b.HasOne<Empresa>().WithMany().HasForeignKey(a => a.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(a => a.AlteradoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(a => a.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(a => a.SistemaId);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_AlteracaoDeCampo_Origem",
            "[Origem] IN ('Usuario','Integracao','Importacao','Sistema','Job')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_AlteracaoDeCampo_Operacao",
            "[Operacao] IN ('Inclusao','Alteracao','Exclusao')"));

        // Auditar sem mudança nenhuma é ruído: pelo menos um dos dois lados existe, e eles
        // são diferentes. [V] 96,8% de uma das tabelas de log do Vórtice são recarimbos de
        // um job — 617 linhas de log por processo, todas iguais.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_AlteracaoDeCampo_Mudou",
            "([ValorAnterior] IS NOT NULL OR [ValorNovo] IS NOT NULL) " +
            "AND ([ValorAnterior] IS NULL OR [ValorNovo] IS NULL OR [ValorAnterior] <> [ValorNovo])"));
    }
}

