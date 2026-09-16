using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Conjuntos de permissão e seus itens.</summary>
public sealed class ConjuntoPermissaoConfiguracao : IEntityTypeConfiguration<ConjuntoPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ConjuntoPermissao> b)
    {
        b.ToTable("ConjuntoDePermissao", "seguranca");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();
        b.Property(c => c.Codigo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.EstaAtivo).IsRequired();
        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_ConjuntoDePermissao_Codigo");

        // A coleção é privada no domínio (encapsulamento). O EF acessa pelo campo.
        b.Metadata
            .FindNavigation(nameof(ConjuntoPermissao.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.ConjuntoPermissaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Uma permissão dentro de um conjunto, com sua profundidade.</summary>
public sealed class ItemConjuntoPermissaoConfiguracao : IEntityTypeConfiguration<ItemConjuntoPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ItemConjuntoPermissao> b)
    {
        b.ToTable("ConjuntoDePermissaoItem", "seguranca");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();
        b.Property(i => i.CodigoPermissao).HasMaxLength(80).IsUnicode(false).IsRequired();

        // Gravada como string: o banco fica legível e inserir valor novo no enum
        // não reordena o que já existe.
        b.Property(i => i.Profundidade).HasConversion<string>()
            .HasMaxLength(20).IsUnicode(false).IsRequired();

        b.HasIndex(i => new { i.ConjuntoPermissaoId, i.CodigoPermissao })
            .IsUnique()
            .HasDatabaseName("UX_ConjuntoDePermissaoItem_Conjunto_Codigo");

        // Domínio fechado no banco, não só no enum do C#. "Nenhum" fica de fora de propósito:
        // ConjuntoPermissao.Conceder() já recusa conceder profundidade Nenhum — não faz
        // sentido existir uma linha dizendo "conceda isto com profundidade nenhuma". O CHECK
        // reflete a MESMA regra de negócio, não só a lista de valores do enum.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ConjuntoDePermissaoItem_Profundidade",
            "[Profundidade] IN ('Proprios','Equipe','Empresa','EmpresaEAbaixo','Organizacao')"));
    }
}

/// <summary>Concessão de conjunto a usuário, com expiração opcional.</summary>
public sealed class UsuarioConjuntoPermissaoConfiguracao : IEntityTypeConfiguration<UsuarioConjuntoPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UsuarioConjuntoPermissao> b)
    {
        b.ToTable("UsuarioConjuntoDePermissao", "seguranca");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedOnAdd();
        b.Property(u => u.ConcedidoEm).HasPrecision(3).IsRequired();
        b.Property(u => u.ExpiraEm).HasPrecision(3);
        b.HasIndex(u => new { u.UsuarioId, u.ConjuntoPermissaoId })
            .IsUnique()
            .HasDatabaseName("UX_UsuarioConjuntoDePermissao_Usuario_Conjunto");

        // [V] "IV_Agenda, IV_Historico e IV_ProcDado não têm FK para IV_Processo — 345.535
        // linhas órfãs" (documento 04, seção 1.4). ConjuntoPermissaoId apontava para
        // seguranca.conjunto_de_permissao sem FK declarada — a mesma lacuna, em miniatura.
        // Restrict, não Cascade: apagar um conjunto de permissão NUNCA deve arrastar
        // silenciosamente o histórico de concessão de um usuário.
        b.HasOne<ConjuntoPermissao>()
            .WithMany()
            .HasForeignKey(u => u.ConjuntoPermissaoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Agora que seguranca.Usuario É uma entidade mapeada, a mesma regra vale para ela —
        // e a heurística de FK por nome do documento 14, seção 6, passa a exigir esta linha.
        b.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(u => u.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

