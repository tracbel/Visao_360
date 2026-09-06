using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Dominio.Documento.Documento"/> — schema <c>documento</c>.</summary>
public sealed class DocumentoConfiguracao : IEntityTypeConfiguration<Dominio.Documento.Documento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Dominio.Documento.Documento> b)
    {
        b.ToTable("Documento", "documento");
        b.HasKey(d => d.Id);
        b.Property(d => d.Id).ValueGeneratedOnAdd();

        b.Property(d => d.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(d => d.ChavePublica).IsUnique().HasDatabaseName("UX_Documento_ChavePublica");

        b.Property(d => d.Nome).HasMaxLength(260).IsUnicode(true).IsRequired();
        b.Property(d => d.TipoConteudo).HasMaxLength(120).IsUnicode(false).IsRequired();
        b.Property(d => d.TamanhoBytes).IsRequired();
        b.Property(d => d.NumeroVersao).IsRequired();

        // Resumo criptográfico de 256 bits em hexadecimal: 64 caracteres, sempre.
        b.Property(d => d.ResumoConteudo).HasMaxLength(64).IsUnicode(false).IsFixedLength().IsRequired();

        // O caminho no armazenamento de objetos. NUNCA o binário no banco.
        b.Property(d => d.CaminhoArmazenamento).HasMaxLength(600).IsUnicode(true).IsRequired();

        b.Property(d => d.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(d => d.AlteradoEm).HasPrecision(3);
        b.Property(d => d.ExcluidoEm).HasPrecision(3);

        // Deduplicação: o mesmo arquivo enviado duas vezes é o mesmo resumo.
        b.HasIndex(d => d.ResumoConteudo).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(d => d.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(d => d.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        // O tipo vem do catálogo TIPO_DOCUMENTO — chave estrangeira composta, para que a coluna
        // não aceite um item de ORIGEM_LEAD (documento 21, achado I-1).
        b.LigarAoCatalogoDeSistema(
            nameof(Dominio.Documento.Documento.TipoDocumentoId), "CatalogoDoTipoDocumentoId",
            CatalogosDeSistema.TipoDeDocumento);

        b.ToTable(x => x.HasCheckConstraint("CK_Documento_Tamanho", "[TamanhoBytes] > 0"));
        b.ToTable(x => x.HasCheckConstraint("CK_Documento_NumeroVersao", "[NumeroVersao] >= 1"));
        b.ToTable(x => x.HasCheckConstraint("CK_Documento_Resumo", "[ResumoConteudo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9a-f]%'"));

        b.Ignore(d => d.Eventos);
    }
}

/// <summary>
/// Mapeamento de <see cref="Dominio.Documento.Vinculo"/>.
///
/// A exclusão em cascata aqui é DECISÃO REGISTRADA, não convenção do EF Core: o vínculo não é
/// entidade independente — sem o documento ele não significa nada. [V] é a lacuna que deixa
/// 5.302 vínculos órfãos no Vórtice e trava o sincronismo do aplicativo de campo.
/// </summary>
public sealed class VinculoDeDocumentoConfiguracao : IEntityTypeConfiguration<Dominio.Documento.Vinculo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Dominio.Documento.Vinculo> b)
    {
        b.ToTable("Vinculo", "documento");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).ValueGeneratedOnAdd();

        b.Property(v => v.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(v => v.VinculadoEm).HasPrecision(3).IsRequired();

        b.HasIndex(v => new { v.DocumentoId, v.Entidade, v.RegistroId })
            .IsUnique()
            .HasDatabaseName("UX_Vinculo_Documento_Entidade_Registro");

        // A leitura da aba de documentação: "quais arquivos deste cliente/processo/máquina?"
        b.HasIndex(v => new { v.Entidade, v.RegistroId });
        b.HasIndex(v => v.VinculadoPorId);

        b.HasOne<Dominio.Documento.Documento>()
            .WithMany()
            .HasForeignKey(v => v.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<Usuario>().WithMany().HasForeignKey(v => v.VinculadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Mapeamento de <see cref="CampoAuditado"/> — schema <c>auditoria</c>.</summary>
public sealed class CampoAuditadoConfiguracao : IEntityTypeConfiguration<CampoAuditado>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CampoAuditado> b)
    {
        b.ToTable("CampoAuditado", "auditoria");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Campo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.RetencaoMeses).IsRequired();
        b.Property(c => c.EstaAtivo).IsRequired();

        b.HasIndex(c => new { c.Entidade, c.Campo }).IsUnique().HasDatabaseName("UX_CampoAuditado_Entidade_Campo");

        // Retenção sempre positiva e com teto: auditar "para sempre" é como o Vórtice chegou a
        // 43,7 milhões de linhas de log sem política nenhuma.
        b.ToTable(x => x.HasCheckConstraint("CK_CampoAuditado_Retencao", "[RetencaoMeses] BETWEEN 1 AND 120"));
    }
}

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

        b.HasOne<Empresa>().WithMany().HasForeignKey(a => a.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(a => a.AlteradoPorId).OnDelete(DeleteBehavior.Restrict);

        // Auditar sem mudança nenhuma é ruído: pelo menos um dos dois lados existe, e eles
        // são diferentes. [V] 96,8% de uma das tabelas de log do Vórtice são recarimbos de
        // um job — 617 linhas de log por processo, todas iguais.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_AlteracaoDeCampo_Mudou",
            "([ValorAnterior] IS NOT NULL OR [ValorNovo] IS NOT NULL) " +
            "AND ([ValorAnterior] IS NULL OR [ValorNovo] IS NULL OR [ValorAnterior] <> [ValorNovo])"));
    }
}

/// <summary>
/// Mapeamento de <see cref="EventoDeAcesso"/>.
///
/// PARTICIONADA POR MÊS na migração inicial, com 24 meses disponíveis.
/// </summary>
public sealed class EventoDeAcessoConfiguracao : IEntityTypeConfiguration<EventoDeAcesso>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EventoDeAcesso> b)
    {
        b.ToTable("EventoDeAcesso", "auditoria");
        b.HasKey(e => new { e.Id, e.OcorreuEm });
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(e => e.NomePrincipal).HasMaxLength(200).IsUnicode(true);
        b.Property(e => e.Entidade).HasMaxLength(40).IsUnicode(false);
        b.Property(e => e.EnderecoIp).HasMaxLength(45).IsUnicode(false);
        b.Property(e => e.AgenteUsuario).HasMaxLength(400).IsUnicode(true);
        b.Property(e => e.Detalhe).HasMaxLength(1000).IsUnicode(true);
        b.Property(e => e.OcorreuEm).HasPrecision(3).IsRequired();

        b.HasIndex(e => new { e.UsuarioId, e.OcorreuEm }).IsDescending(false, true);
        b.HasIndex(e => new { e.Tipo, e.OcorreuEm }).IsDescending(false, true);

        // Quem VIU este registro — a pergunta que a LGPD faz.
        b.HasIndex(e => new { e.Entidade, e.RegistroId, e.OcorreuEm })
            .IsDescending(false, false, true)
            .HasFilter("[Entidade] IS NOT NULL");

        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_EventoDeAcesso_Tipo",
            "[Tipo] IN ('Login','LoginFalhou','Logout','AcessoNegado','ExportacaoDados','LeituraDadoSensivel')"));

        // Evento de leitura diz o que foi lido; os demais não precisam.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_EventoDeAcesso_Registro",
            "([Entidade] IS NULL AND [RegistroId] IS NULL) OR ([Entidade] IS NOT NULL AND [RegistroId] IS NOT NULL)"));
    }
}
