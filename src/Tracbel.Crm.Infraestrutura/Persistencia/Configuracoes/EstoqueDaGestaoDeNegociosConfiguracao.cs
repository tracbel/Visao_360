using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="EquipamentoEmEstoque"/> — <c>frota.EquipamentoEmEstoque</c> (28/09/2026). Uma linha por chassi
/// interno do TOTVS; o índice é a leitura da tela — a filial, pela situação e pelo grupo.
/// </summary>
public sealed class EquipamentoEmEstoqueConfiguracao : IEntityTypeConfiguration<EquipamentoEmEstoque>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EquipamentoEmEstoque> b)
    {
        b.ToTable("EquipamentoEmEstoque", "frota");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(e => e.ChavePublica).IsUnique().HasDatabaseName("UX_EquipamentoEmEstoque_ChavePublica");

        b.Property(e => e.Chaint).HasMaxLength(EquipamentoEmEstoque.TamanhoDoChaint).IsUnicode(false).IsRequired();
        b.Property(e => e.Chassi).HasMaxLength(EquipamentoEmEstoque.TamanhoDoCodigo).IsUnicode(false);
        b.Property(e => e.Pedido).HasMaxLength(EquipamentoEmEstoque.TamanhoDoCodigo).IsUnicode(false);
        b.Property(e => e.Comar).HasMaxLength(EquipamentoEmEstoque.TamanhoDoCodigo).IsUnicode(false);
        b.Property(e => e.Situacao).HasMaxLength(EquipamentoEmEstoque.TamanhoDoTextoCurto).IsUnicode(true).IsRequired();
        b.Property(e => e.Grupo).HasMaxLength(EquipamentoEmEstoque.TamanhoDoTextoCurto).IsUnicode(true).IsRequired();
        b.Property(e => e.Descricao).HasMaxLength(EquipamentoEmEstoque.TamanhoDaDescricao).IsUnicode(true).IsRequired();
        b.Property(e => e.Configuracao).HasMaxLength(EquipamentoEmEstoque.TamanhoDaDescricao).IsUnicode(true);
        b.Property(e => e.Tipo).HasMaxLength(EquipamentoEmEstoque.TamanhoDoTextoCurto).IsUnicode(true).IsRequired();
        b.Property(e => e.AnoModelo).HasMaxLength(9).IsUnicode(false);
        b.Property(e => e.SituacaoNaFabrica).HasMaxLength(EquipamentoEmEstoque.TamanhoDoTextoCurto).IsUnicode(true);
        b.Property(e => e.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(e => e.LidaEm).HasPrecision(3).IsRequired();
        b.Property(e => e.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.AlteradoEm).HasPrecision(3);
        b.Property(e => e.ExcluidoEm).HasPrecision(3);

        b.HasIndex(e => new { e.SistemaId, e.Chaint }).IsUnique().HasDatabaseName("UX_EquipamentoEmEstoque_Sistema_Chaint");
        b.HasIndex(e => e.EmpresaId)
            .HasFilter("[ExcluidoEm] IS NULL")
            .IncludeProperties(e => new { e.Situacao, e.Grupo, e.Reservado, e.EntradaEm, e.ChegadaPrevistaEm })
            .HasDatabaseName("IX_EquipamentoEmEstoque_Empresa");

        b.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(e => e.SistemaId).OnDelete(DeleteBehavior.Restrict);

        b.Ignore(e => e.EhPedidoDeFabrica);
        b.Ignore(e => e.Eventos);
    }
}

/// <summary>
/// Mapeamento de <see cref="CoberturaDoEstoque"/> — <c>frota.CoberturaDoEstoque</c> (28/09/2026). Uma linha por recorte e
/// chave; sem <c>EmpresaId</c>, porque a GN calcula a cobertura para a empresa inteira.
/// </summary>
public sealed class CoberturaDoEstoqueConfiguracao : IEntityTypeConfiguration<CoberturaDoEstoque>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CoberturaDoEstoque> b)
    {
        b.ToTable("CoberturaDoEstoque", "frota");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Recorte).HasMaxLength(10).IsUnicode(false).IsRequired();
        b.Property(c => c.Chave).HasMaxLength(CoberturaDoEstoque.TamanhoDaChave).IsUnicode(true).IsRequired();
        b.Property(c => c.MesesDeEstoque).HasPrecision(6, 2);
        b.Property(c => c.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(c => c.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ImportadoPorId).IsRequired();
        b.Property(c => c.LidaEm).HasPrecision(3).IsRequired();
        b.Property(c => c.GeradaNaOrigemEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        b.HasIndex(c => new { c.SistemaId, c.Recorte, c.Chave }).IsUnique().HasDatabaseName("UX_CoberturaDoEstoque_Sistema_Recorte_Chave");
        b.HasIndex(c => c.ImportadoPorId);

        b.HasOne<Sistema>().WithMany().HasForeignKey(c => c.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_CoberturaDoEstoque_Recorte", "[Recorte] IN ('MES', 'GRUPO')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CoberturaDoEstoque_Competencia",
            "([Recorte] = 'MES' AND [Competencia] IS NOT NULL AND DAY([Competencia]) = 1) OR ([Recorte] = 'GRUPO' AND [Competencia] IS NULL)"));
        b.ToTable(x => x.HasCheckConstraint("CK_CoberturaDoEstoque_Valores", "[MesesDeEstoque] >= 0 AND [Vendas] >= 0"));
    }
}
