using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Sistema"/> — schema <c>integracao</c>.</summary>
public sealed class SistemaConfiguracao : IEntityTypeConfiguration<Sistema>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Sistema> b)
    {
        b.ToTable("Sistema", "integracao");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedOnAdd();

        b.Property(s => s.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(s => s.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(s => s.MeioDeAcesso).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(s => s.ResponsavelTecnico).HasMaxLength(120).IsUnicode(true);
        b.Property(s => s.EstaAtivo).IsRequired();

        b.HasIndex(s => s.Codigo).IsUnique().HasDatabaseName("UX_Sistema_Codigo");
    }
}

/// <summary>
/// Mapeamento de <see cref="ChaveExterna"/>.
///
/// A unicidade por sistema, entidade e chave de origem é o que torna a gravação idempotente —
/// [V] e é o que falta nas cinco cópias do de-para do Vórtice.
/// </summary>
public sealed class ChaveExternaConfiguracao : IEntityTypeConfiguration<ChaveExterna>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChaveExterna> b)
    {
        b.ToTable("ChaveExterna", "integracao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.ChaveOrigem).HasMaxLength(200).IsUnicode(false).IsRequired();
        b.Property(c => c.SincronizadoEm).HasPrecision(3).IsRequired();

        b.HasIndex(c => new { c.SistemaId, c.Entidade, c.ChaveOrigem })
            .IsUnique()
            .HasDatabaseName("UX_ChaveExterna_Sistema_Entidade_Chave");

        b.HasIndex(c => new { c.Entidade, c.RegistroId });

        b.HasOne<Sistema>().WithMany().HasForeignKey(c => c.SistemaId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Mapeamento de <see cref="PontoDeSincronismo"/>.
///
/// É a tabela sobre a qual se escreve o alarme de atraso — a que teria avisado em abril de
/// 2025, quando a integração de faturamento parou e ninguém soube por 17 meses.
/// </summary>
public sealed class PontoDeSincronismoConfiguracao : IEntityTypeConfiguration<PontoDeSincronismo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PontoDeSincronismo> b)
    {
        b.ToTable("PontoDeSincronismo", "integracao");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.Fluxo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(p => p.UltimoValor).HasMaxLength(100).IsUnicode(false).IsRequired();
        b.Property(p => p.ProcessadoEm).HasPrecision(3).IsRequired();
        b.Property(p => p.RegistrosLidos).IsRequired();
        b.Property(p => p.RegistrosGravados).IsRequired();
        b.Property(p => p.RegistrosErro).IsRequired();
        b.Property(p => p.MinutosParaAlarme).IsRequired();

        b.HasIndex(p => p.Fluxo).IsUnique().HasDatabaseName("UX_PontoDeSincronismo_Fluxo");
        b.HasIndex(p => p.SistemaId);

        b.HasOne<Sistema>().WithMany().HasForeignKey(p => p.SistemaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_PontoDeSincronismo_Contadores",
            "[RegistrosLidos] >= 0 AND [RegistrosGravados] >= 0 AND [RegistrosErro] >= 0"));

        // Alarme desligado não existe: todo fluxo declara em quantos minutos o silêncio vira
        // incidente.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_PontoDeSincronismo_Alarme", "[MinutosParaAlarme] > 0"));
    }
}

/// <summary>Mapeamento de <see cref="MensagemDescartada"/> — a fila de descarte.</summary>
public sealed class MensagemDescartadaConfiguracao : IEntityTypeConfiguration<MensagemDescartada>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MensagemDescartada> b)
    {
        b.ToTable("MensagemDescartada", "integracao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.Fluxo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(m => m.Conteudo).HasColumnType("nvarchar(max)").IsRequired();
        b.ToTable(x => x.HasCheckConstraint("CK_MensagemDescartada_ConteudoJson", "ISJSON([Conteudo]) = 1"));
        b.Property(m => m.Erro).HasMaxLength(4000).IsUnicode(true).IsRequired();
        b.Property(m => m.Tentativas).IsRequired();
        b.Property(m => m.Tratativa).HasMaxLength(1000).IsUnicode(true);
        b.Property(m => m.DescartadaEm).HasPrecision(3).IsRequired();
        b.Property(m => m.TratadaEm).HasPrecision(3);

        // A fila que alguém precisa olhar: o que foi descartado e ainda não foi tratado.
        b.HasIndex(m => new { m.Fluxo, m.DescartadaEm }).HasFilter("[TratadaEm] IS NULL");
        b.HasIndex(m => m.TratadaPorId);

        b.HasOne<Usuario>().WithMany().HasForeignKey(m => m.TratadaPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_MensagemDescartada_Tentativas", "[Tentativas] >= 0"));

        // Uma fila de descarte que ninguém olha é igual a não ter fila: se tratou, diz quem e
        // o que fez.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_MensagemDescartada_Tratativa",
            "[TratadaEm] IS NULL OR ([TratadaPorId] IS NOT NULL AND [Tratativa] IS NOT NULL)"));
    }
}
