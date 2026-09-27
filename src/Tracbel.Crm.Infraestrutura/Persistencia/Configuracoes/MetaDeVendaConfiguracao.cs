using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="MetaDeVenda"/> — <c>organizacao.MetaDeVenda</c> (#138, 27/09/2026).
///
/// <para><b>Uma linha por <c>id</c> da GN</b> (<c>UX_MetaDeVenda_Sistema_IdNaOrigem</c>). A chave de negócio não é única
/// na origem, e por isso não é índice aqui: as duplicatas se somam na leitura.</para>
///
/// <para><b>Os dois índices são as duas leituras da rota</b>: a filial num intervalo de meses (o gerente, a diretoria
/// filial por filial) e o consultor num intervalo (o vendedor que só vê a própria meta). O primeiro cobre a soma sem
/// voltar à tabela, e só com as metas vigentes — a excluída fica para a trilha.</para>
/// </summary>
public sealed class MetaDeVendaConfiguracao : IEntityTypeConfiguration<MetaDeVenda>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MetaDeVenda> b)
    {
        b.ToTable("MetaDeVenda", "organizacao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(m => m.ChavePublica).IsUnique().HasDatabaseName("UX_MetaDeVenda_ChavePublica");

        b.Property(m => m.LinhaNaOrigem).HasMaxLength(MetaDeVenda.TamanhoDaLinha).IsUnicode(true).IsRequired();
        b.Property(m => m.CodigoDaLinha).HasMaxLength(MetaDeVenda.TamanhoDaLinha).IsUnicode(false).IsRequired();
        b.Property(m => m.ConsultorNaOrigem).HasMaxLength(MetaDeVenda.TamanhoDoConsultor).IsUnicode(false).IsRequired();
        b.Property(m => m.Origem).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(m => m.ValorUnitario).HasPrecision(18, 2);
        b.Property(m => m.Margem).HasPrecision(18, 4);
        b.Property(m => m.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();

        b.Property(m => m.ImportadaEm).HasPrecision(3).IsRequired();
        b.Property(m => m.AtualizadaPelaOrigemEm).HasPrecision(3);
        b.Property(m => m.LidaEm).HasPrecision(3).IsRequired();
        b.Property(m => m.GeradaNaOrigemEm).HasPrecision(3);
        b.Property(m => m.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(m => m.AlteradoEm).HasPrecision(3);
        b.Property(m => m.ExcluidoEm).HasPrecision(3);

        b.HasIndex(m => new { m.SistemaId, m.IdNaOrigem })
            .IsUnique()
            .HasDatabaseName("UX_MetaDeVenda_Sistema_IdNaOrigem");

        b.HasIndex(m => new { m.EmpresaId, m.Competencia })
            .HasFilter("[ExcluidoEm] IS NULL")
            .IncludeProperties(m => new { m.CodigoDaLinha, m.ConsultorNaOrigem, m.ConsultorUsuarioId, m.VendaDireta, m.Origem, m.Quantidade })
            .HasDatabaseName("IX_MetaDeVenda_Empresa_Competencia");

        b.HasIndex(m => new { m.ConsultorUsuarioId, m.Competencia })
            .HasDatabaseName("IX_MetaDeVenda_Consultor_Competencia");

        b.HasIndex(m => m.LinhaDeProdutoId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(m => m.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(m => m.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LinhaDeProduto>().WithMany().HasForeignKey(m => m.LinhaDeProdutoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(m => m.ConsultorUsuarioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_MetaDeVenda_Competencia", "DAY([Competencia]) = 1"));
        b.ToTable(x => x.HasCheckConstraint("CK_MetaDeVenda_Quantidade", "[Quantidade] >= 0"));
        b.ToTable(x => x.HasCheckConstraint("CK_MetaDeVenda_IdNaOrigem", "[IdNaOrigem] > 0"));
        b.ToTable(x => x.HasCheckConstraint("CK_MetaDeVenda_Origem", "[Origem] IN ('Campanha','Consorcio')"));

        b.Ignore(m => m.Eventos);
    }
}
