using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Os perfis de acesso (fase 3 do documento 41; eram <c>ConjuntoDePermissao</c>).
///
/// <para><b>A semente vem de <see cref="PerfisDeSistema"/>, pelo <c>HasData</c></b> — o mesmo caminho
/// dos catálogos de sistema. Vale na migração, em produção, e no <c>EnsureCreated</c> dos testes: um
/// perfil padrão semeado só por SQL na migração deixaria o teste sem ele, e toda rota daria 403.</para>
/// </summary>
public sealed class PerfilConfiguracao : IEntityTypeConfiguration<Perfil>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Perfil> b)
    {
        b.ToTable("Perfil", "seguranca");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();
        b.Property(c => c.Codigo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.EstaAtivo).IsRequired();
        b.Property(c => c.EhPadrao).IsRequired();

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Perfil_Codigo");

        // UM PERFIL PADRÃO SÓ. Dois seriam duas respostas para "o que todo usuário recebe?".
        b.HasIndex(c => c.EhPadrao).IsUnique().HasFilter("[EhPadrao] = 1").HasDatabaseName("UX_Perfil_Padrao");

        // O PERFIL PADRÃO NÃO SE DESLIGA — trancaria todo mundo. O domínio já recusa; o banco repete.
        b.ToTable(t => t.HasCheckConstraint("CK_Perfil_PadraoAtivo", "[EhPadrao] = 0 OR [EstaAtivo] = 1"));

        // A coleção é privada no domínio (encapsulamento). O EF acessa pelo campo.
        b.Metadata
            .FindNavigation(nameof(Perfil.Permissoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(c => c.Permissoes)
            .WithOne()
            .HasForeignKey(i => i.PerfilId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasData(PerfisDeSistema.Todos.Select(p => new
        {
            p.Id,
            p.Codigo,
            p.Nome,
            Descricao = (string?)p.Descricao,
            EstaAtivo = true,
            p.EhPadrao
        }));
    }
}

/// <summary>Uma permissão dentro de um perfil, com sua profundidade.</summary>
public sealed class PerfilPermissaoConfiguracao : IEntityTypeConfiguration<PerfilPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PerfilPermissao> b)
    {
        b.ToTable("PerfilPermissao", "seguranca");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();
        b.Property(i => i.CodigoPermissao).HasMaxLength(80).IsUnicode(false).IsRequired();

        // Gravada como string: o banco fica legível e inserir valor novo no enum
        // não reordena o que já existe.
        b.Property(i => i.Profundidade).HasConversion<string>()
            .HasMaxLength(20).IsUnicode(false).IsRequired();

        b.HasIndex(i => new { i.PerfilId, i.CodigoPermissao })
            .IsUnique()
            .HasDatabaseName("UX_PerfilPermissao_Perfil_Codigo");

        // Domínio fechado no banco, não só no enum do C#. "Nenhum" fica de fora de propósito:
        // Perfil.Conceder() já recusa conceder profundidade Nenhum.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_PerfilPermissao_Profundidade",
            "[Profundidade] IN ('Proprios','Equipe','Empresa','EmpresaEAbaixo','Organizacao')"));

        // AS PERMISSÕES DOS PERFIS SEMEADOS, com identificadores fixos: 100 × perfil + ordem. A faixa
        // separa os perfis e deixa espaço para acrescentar permissão a um perfil de sistema numa
        // migração futura sem renumerar os outros. A conta mora em PerfisDeSistema.LinhasDaSemente, que
        // também sabe pular o lugar de uma permissão retirada.
        b.HasData(PerfisDeSistema.LinhasDaSemente().Select(linha => new
        {
            linha.Id,
            linha.PerfilId,
            CodigoPermissao = linha.Codigo,
            linha.Profundidade
        }));
    }
}

/// <summary>A concessão de perfil a usuário, com filial e expiração opcionais.</summary>
public sealed class UsuarioPerfilConfiguracao : IEntityTypeConfiguration<UsuarioPerfil>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UsuarioPerfil> b)
    {
        b.ToTable("UsuarioPerfil", "seguranca");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedOnAdd();
        b.Property(u => u.Justificativa).HasMaxLength(400).IsUnicode(true).IsRequired();
        b.Property(u => u.ConcedidoEm).HasPrecision(3).IsRequired();
        b.Property(u => u.ExpiraEm).HasPrecision(3);
        b.Property(u => u.RevogadaEm).HasPrecision(3);
        b.Property(u => u.MotivoDaRevogacao).HasMaxLength(400).IsUnicode(true);

        // O MESMO PERFIL NA MESMA FILIAL, UMA VEZ ENTRE AS QUE NÃO FORAM REVOGADAS (issue 113). A revogada fica
        // para o histórico e não pode impedir uma concessão nova. O índice só cobre concessão COM filial — o
        // EF filtra a coluna anulável —; a sem filial é conferida pelo caso de uso, que recusa a repetida.
        b.HasIndex(u => new { u.UsuarioId, u.PerfilId, u.EmpresaId })
            .IsUnique()
            .HasFilter("[EmpresaId] IS NOT NULL AND [RevogadaEm] IS NULL")
            .HasDatabaseName("UX_UsuarioPerfil_Usuario_Perfil_Empresa");

        // Restrict, não Cascade: apagar um perfil NUNCA deve arrastar silenciosamente o histórico de
        // concessão de um usuário.
        b.HasOne<Perfil>().WithMany().HasForeignKey(u => u.PerfilId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(u => u.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(u => u.ConcedidoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(u => u.RevogadaPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Empresa>().WithMany().HasForeignKey(u => u.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(u => u.ConcedidoPorId);
        b.HasIndex(u => u.EmpresaId);
    }
}
