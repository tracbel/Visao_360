using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Usuario"/> — schema <c>seguranca</c>.</summary>
public sealed class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuario", "seguranca");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedOnAdd();

        b.Property(u => u.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(u => u.ChavePublica).IsUnique().HasDatabaseName("UX_Usuario_ChavePublica");

        b.Property(u => u.IdentidadeExterna).IsRequired();
        b.Property(u => u.NomePrincipal).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(u => u.NomeCompleto).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(u => u.NomeExibicao).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(u => u.Papel).HasMaxLength(40).IsUnicode(false);
        b.Property(u => u.EstaAtivo).IsRequired();

        // Texto, e não número, pela mesma razão da carteira: a tabela tem de ser legível sem o
        // código do lado.
        b.Property(u => u.Natureza)
            .HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Usuario_Natureza",
            "[Natureza] IN ('Pessoa','Departamento','Sistema','Fornecedor','Teste')"));

        // O CRM não guarda senha: a identidade vem do Entra ID. Este conversor grava só o
        // endereço, já validado pelo tipo de valor.
        b.Property(u => u.Email)
            .HasConversion(e => e.Endereco, s => Dominio.Comum.Email.Criar(s))
            .HasMaxLength(200).IsUnicode(true).IsRequired();

        b.Property(u => u.DesativadoEm).HasPrecision(3);
        b.Property(u => u.UltimoLoginEm).HasPrecision(3);
        b.Property(u => u.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(u => u.AlteradoEm).HasPrecision(3);
        b.Property(u => u.ExcluidoEm).HasPrecision(3);

        b.HasIndex(u => u.IdentidadeExterna).IsUnique().HasDatabaseName("UX_Usuario_IdentidadeExterna");
        b.HasIndex(u => u.NomePrincipal).IsUnique().HasDatabaseName("UX_Usuario_NomePrincipal");

        // A consulta da hierarquia: "quem responde a mim", só entre os ativos.
        b.HasIndex(u => u.GestorId).HasFilter("[EstaAtivo] = 1 AND [ExcluidoEm] IS NULL");
        b.HasIndex(u => u.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(u => u.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(u => u.GestorId).OnDelete(DeleteBehavior.Restrict);

        // Desativar carimba a data. Sem isto, "inativo desde quando?" não tem resposta.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Usuario_Desativacao",
            "[EstaAtivo] = 1 OR [DesativadoEm] IS NOT NULL"));

        b.Ignore(u => u.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="Equipe"/> e de sua composição.</summary>
public sealed class EquipeConfiguracao : IEntityTypeConfiguration<Equipe>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Equipe> b)
    {
        b.ToTable("Equipe", "seguranca");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(e => e.ChavePublica).IsUnique().HasDatabaseName("UX_Equipe_ChavePublica");

        b.Property(e => e.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(e => e.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.EstaAtiva).IsRequired();

        b.Property(e => e.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.AlteradoEm).HasPrecision(3);
        b.Property(e => e.ExcluidoEm).HasPrecision(3);

        b.HasIndex(e => new { e.EmpresaId, e.Nome }).IsUnique().HasDatabaseName("UX_Equipe_Empresa_Nome");

        b.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_Equipe_Tipo", "[Tipo] IN ('Proprietaria','Acesso')"));

        b.Ignore(e => e.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="EquipeMembro"/>.</summary>
public sealed class EquipeMembroConfiguracao : IEntityTypeConfiguration<EquipeMembro>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EquipeMembro> b)
    {
        b.ToTable("EquipeMembro", "seguranca");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.EhLider).IsRequired();
        b.Property(m => m.EntrouEm).HasPrecision(3).IsRequired();
        b.Property(m => m.SaiuEm).HasPrecision(3);

        // Um usuário entra uma vez por equipe enquanto o vínculo está vigente. Índice único
        // PARCIAL: sair e voltar depois é legítimo e não colide com o vínculo encerrado.
        b.HasIndex(m => new { m.EquipeId, m.UsuarioId })
            .IsUnique()
            .HasFilter("[SaiuEm] IS NULL")
            .HasDatabaseName("UX_EquipeMembro_Equipe_Usuario_Vigente");

        b.HasIndex(m => m.UsuarioId);

        b.HasOne<Equipe>().WithMany().HasForeignKey(m => m.EquipeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_EquipeMembro_Periodo", "[SaiuEm] IS NULL OR [SaiuEm] >= [EntrouEm]"));
    }
}

/// <summary>Mapeamento de <see cref="CompartilhamentoDeRegistro"/>.</summary>
public sealed class CompartilhamentoDeRegistroConfiguracao : IEntityTypeConfiguration<CompartilhamentoDeRegistro>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CompartilhamentoDeRegistro> b)
    {
        b.ToTable("CompartilhamentoDeRegistro", "seguranca");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(c => c.ChavePublica).IsUnique().HasDatabaseName("UX_CompartilhamentoDeRegistro_ChavePublica");

        b.Property(c => c.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nivel).HasConversion<string>().HasMaxLength(10).IsUnicode(false).IsRequired();
        b.Property(c => c.Motivo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.ExpiraEm).HasPrecision(3);

        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.AlteradoEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        // A pergunta que a tela faz: "o que este usuário enxerga por compartilhamento?"
        b.HasIndex(c => new { c.UsuarioId, c.Entidade }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => new { c.EquipeId, c.Entidade }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => new { c.Entidade, c.RegistroId }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipe>().WithMany().HasForeignKey(c => c.EquipeId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_CompartilhamentoDeRegistro_Nivel", "[Nivel] IN ('Leitura','Edicao')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_CompartilhamentoDeRegistro_Motivo",
            "[Motivo] IN ('Manual','Regra','Equipe','Hierarquia','Delegacao')"));

        // Exatamente um sujeito. É o que impede a linha que não concede nada a ninguém e a
        // que concede a dois ao mesmo tempo.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_CompartilhamentoDeRegistro_UmSujeito",
            "([UsuarioId] IS NOT NULL AND [EquipeId] IS NULL) OR ([UsuarioId] IS NULL AND [EquipeId] IS NOT NULL)"));

        b.Ignore(c => c.Eventos);
    }
}
