using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="OrdemDeServico"/> — <c>frota.OrdemDeServico</c> (02/10/2026). Uma linha por OS do Protheus
/// (filial e número); os índices são as três leituras da tela: as OS de um cliente, as de uma máquina e as abertas de uma
/// filial.
/// </summary>
public sealed class OrdemDeServicoConfiguracao : IEntityTypeConfiguration<OrdemDeServico>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrdemDeServico> b)
    {
        b.ToTable("OrdemDeServico", "frota");
        b.HasKey(o => o.Id);
        b.Property(o => o.Id).ValueGeneratedOnAdd();

        b.Property(o => o.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(o => o.ChavePublica).IsUnique().HasDatabaseName("UX_OrdemDeServico_ChavePublica");

        b.Property(o => o.ChaveNaOrigem).HasMaxLength(OrdemDeServico.TamanhoDaChave).IsUnicode(false).IsRequired();
        b.Property(o => o.Numero).HasMaxLength(OrdemDeServico.TamanhoDoNumero).IsUnicode(false).IsRequired();
        b.Property(o => o.Chassi).HasMaxLength(OrdemDeServico.TamanhoDoChassi).IsUnicode(false);
        b.Property(o => o.Modelo).HasMaxLength(OrdemDeServico.TamanhoDoTexto).IsUnicode(true);
        b.Property(o => o.Horimetro).HasPrecision(9, 1);
        b.Property(o => o.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(o => o.TipoDeAtendimento).HasMaxLength(OrdemDeServico.TamanhoDoTexto).IsUnicode(true);
        b.Property(o => o.ValorDePecas).HasPrecision(18, 2);
        b.Property(o => o.ValorDeServicos).HasPrecision(18, 2);
        b.Property(o => o.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(o => o.LidaEm).HasPrecision(3).IsRequired();
        b.Property(o => o.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(o => o.AlteradoEm).HasPrecision(3);
        b.Property(o => o.ExcluidoEm).HasPrecision(3);

        b.HasIndex(o => new { o.SistemaId, o.ChaveNaOrigem }).IsUnique().HasDatabaseName("UX_OrdemDeServico_Sistema_Chave");
        b.HasIndex(o => o.ClienteId)
            .HasFilter("[ClienteId] IS NOT NULL AND [ExcluidoEm] IS NULL")
            .IncludeProperties(o => new { o.Situacao, o.AbertaEm, o.ValorDePecas, o.ValorDeServicos })
            .HasDatabaseName("IX_OrdemDeServico_Cliente");
        b.HasIndex(o => o.EquipamentoId)
            .HasFilter("[EquipamentoId] IS NOT NULL AND [ExcluidoEm] IS NULL")
            .HasDatabaseName("IX_OrdemDeServico_Equipamento");
        b.HasIndex(o => new { o.EmpresaId, o.Situacao })
            .HasFilter("[ExcluidoEm] IS NULL")
            .IncludeProperties(o => new { o.AbertaEm, o.ValorDePecas, o.ValorDeServicos })
            .HasDatabaseName("IX_OrdemDeServico_Empresa_Situacao");

        b.HasOne<Empresa>().WithMany().HasForeignKey(o => o.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(o => o.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(o => o.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(o => o.EquipamentoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_OrdemDeServico_Situacao", "[Situacao] IN ('Aberta', 'Liberada', 'Fechada', 'Cancelada')"));
        b.ToTable(x => x.HasCheckConstraint("CK_OrdemDeServico_Itens", "[ItensDePeca] >= 0 AND [ItensDeServico] >= 0"));
        b.ToTable(x => x.HasCheckConstraint("CK_OrdemDeServico_Horimetro", "[Horimetro] IS NULL OR ([Horimetro] >= 0 AND [Horimetro] <= 999999)"));

        b.Ignore(o => o.ValorTotal);
        b.Ignore(o => o.EstaEmAberto);
        b.Ignore(o => o.Eventos);
    }
}
