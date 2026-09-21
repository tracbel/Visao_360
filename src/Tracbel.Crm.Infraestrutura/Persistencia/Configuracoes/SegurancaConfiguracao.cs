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

