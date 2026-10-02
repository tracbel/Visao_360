using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="FaturamentoDePecasNoMes"/> — <c>comercial.FaturamentoDePecasNoMes</c> (02/10/2026). Apuração por mês;
/// os índices são as leituras da tela: o cliente nos doze meses e a filial no mês.
/// </summary>
public sealed class FaturamentoDePecasNoMesConfiguracao : IEntityTypeConfiguration<FaturamentoDePecasNoMes>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FaturamentoDePecasNoMes> b)
    {
        b.ToTable("FaturamentoDePecasNoMes", "comercial");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();

        b.Property(f => f.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(f => f.ChavePublica).IsUnique().HasDatabaseName("UX_FaturamentoDePecasNoMes_ChavePublica");

        b.Property(f => f.Setor).HasMaxLength(FaturamentoDePecasNoMes.TamanhoDoTextoCurto).IsUnicode(true).IsRequired();
        b.Property(f => f.Grupo).HasMaxLength(FaturamentoDePecasNoMes.TamanhoDoTextoCurto).IsUnicode(true).IsRequired();
        b.Property(f => f.Linha).HasMaxLength(FaturamentoDePecasNoMes.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(f => f.VendedorCodigo).HasMaxLength(FaturamentoDePecasNoMes.TamanhoDoCodigo).IsUnicode(false);
        b.Property(f => f.VendedorNome).HasMaxLength(FaturamentoDePecasNoMes.TamanhoDoTexto).IsUnicode(true);
        b.Property(f => f.Quantidade).HasPrecision(18, 3);
        b.Property(f => f.ValorLiquido).HasPrecision(18, 2);
        b.Property(f => f.ValorDeDevolucoes).HasPrecision(18, 2);
        b.Property(f => f.ValorDeDesconto).HasPrecision(18, 2);
        b.Property(f => f.ValorDeTabela).HasPrecision(18, 2);
        b.Property(f => f.ApuradoEm).HasPrecision(3).IsRequired();
        b.Property(f => f.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(f => f.AlteradoEm).HasPrecision(3);
        b.Property(f => f.ExcluidoEm).HasPrecision(3);

        b.HasIndex(f => new { f.ClienteId, f.Competencia })
            .HasFilter("[ClienteId] IS NOT NULL")
            .IncludeProperties(f => new { f.Setor, f.Grupo, f.ValorLiquido })
            .HasDatabaseName("IX_FaturamentoDePecasNoMes_Cliente");
        b.HasIndex(f => new { f.EmpresaId, f.Competencia })
            .IncludeProperties(f => new { f.Setor, f.Grupo, f.ValorLiquido })
            .HasDatabaseName("IX_FaturamentoDePecasNoMes_Empresa");
        b.HasIndex(f => new { f.SistemaId, f.Competencia }).HasDatabaseName("IX_FaturamentoDePecasNoMes_Sistema_Competencia");

        b.HasOne<Empresa>().WithMany().HasForeignKey(f => f.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(f => f.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(f => f.ClienteId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_FaturamentoDePecasNoMes_Competencia", "DAY([Competencia]) = 1"));
        b.ToTable(x => x.HasCheckConstraint("CK_FaturamentoDePecasNoMes_Itens", "[Itens] > 0"));

        b.Ignore(f => f.Eventos);
    }
}

/// <summary>
/// Mapeamento de <see cref="OrcamentoDePecas"/> — <c>comercial.OrcamentoDePecas</c> (02/10/2026). Um por orçamento (filial e
/// número); os índices são as leituras da tela: os do cliente e os abertos da filial.
/// </summary>
public sealed class OrcamentoDePecasConfiguracao : IEntityTypeConfiguration<OrcamentoDePecas>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrcamentoDePecas> b)
    {
        b.ToTable("OrcamentoDePecas", "comercial");
        b.HasKey(o => o.Id);
        b.Property(o => o.Id).ValueGeneratedOnAdd();

        b.Property(o => o.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(o => o.ChavePublica).IsUnique().HasDatabaseName("UX_OrcamentoDePecas_ChavePublica");

        b.Property(o => o.ChaveNaOrigem).HasMaxLength(OrcamentoDePecas.TamanhoDaChave).IsUnicode(false).IsRequired();
        b.Property(o => o.Numero).HasMaxLength(OrcamentoDePecas.TamanhoDoNumero).IsUnicode(false).IsRequired();
        b.Property(o => o.Situacao).HasMaxLength(OrcamentoDePecas.TamanhoDoTextoCurto).IsUnicode(true).IsRequired();
        b.Property(o => o.Prazo).HasMaxLength(OrcamentoDePecas.TamanhoDoTextoCurto).IsUnicode(true);
        b.Property(o => o.Reserva).HasMaxLength(OrcamentoDePecas.TamanhoDoTextoCurto).IsUnicode(true);
        b.Property(o => o.TipoDeAtendimento).HasMaxLength(OrcamentoDePecas.TamanhoDoTextoCurto).IsUnicode(true);
        b.Property(o => o.TipoDeOrcamento).HasMaxLength(OrcamentoDePecas.TamanhoDoTextoCurto).IsUnicode(true);
        b.Property(o => o.VendedorCodigo).HasMaxLength(FaturamentoDePecasNoMes.TamanhoDoCodigo).IsUnicode(false);
        b.Property(o => o.VendedorNome).HasMaxLength(OrcamentoDePecas.TamanhoDoTexto).IsUnicode(true);
        b.Property(o => o.ValorTotal).HasPrecision(18, 2);
        b.Property(o => o.ValorDeDesconto).HasPrecision(18, 2);
        b.Property(o => o.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(o => o.LidaEm).HasPrecision(3).IsRequired();
        b.Property(o => o.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(o => o.AlteradoEm).HasPrecision(3);
        b.Property(o => o.ExcluidoEm).HasPrecision(3);

        b.HasIndex(o => new { o.SistemaId, o.ChaveNaOrigem }).IsUnique().HasDatabaseName("UX_OrcamentoDePecas_Sistema_Chave");
        b.HasIndex(o => o.ClienteId)
            .HasFilter("[ClienteId] IS NOT NULL AND [ExcluidoEm] IS NULL")
            .IncludeProperties(o => new { o.Situacao, o.OrcadoEm, o.ValidoAte, o.ValorTotal })
            .HasDatabaseName("IX_OrcamentoDePecas_Cliente");
        b.HasIndex(o => new { o.EmpresaId, o.Situacao })
            .HasFilter("[ExcluidoEm] IS NULL")
            .IncludeProperties(o => new { o.OrcadoEm, o.ValorTotal })
            .HasDatabaseName("IX_OrcamentoDePecas_Empresa_Situacao");

        b.HasOne<Empresa>().WithMany().HasForeignKey(o => o.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(o => o.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(o => o.ClienteId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_OrcamentoDePecas_Itens", "[Itens] > 0"));

        b.Ignore(o => o.EstaEmAberto);
        b.Ignore(o => o.Eventos);
    }
}
