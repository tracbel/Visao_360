using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Marca"/> — schema <c>frota</c>.</summary>
public sealed class MarcaConfiguracao : IEntityTypeConfiguration<Marca>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Marca> b)
    {
        b.ToTable("Marca", "frota");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();
        b.Property(m => m.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(m => m.Nome).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(m => m.EhRepresentada).IsRequired();
        b.Property(m => m.EstaAtiva).IsRequired();

        b.HasIndex(m => m.Codigo).IsUnique().HasDatabaseName("UX_Marca_Codigo");
    }
}

/// <summary>Mapeamento de <see cref="Familia"/>.</summary>
public sealed class FamiliaConfiguracao : IEntityTypeConfiguration<Familia>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Familia> b)
    {
        b.ToTable("Familia", "frota");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();
        b.Property(f => f.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(f => f.Nome).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(f => f.EstaAtiva).IsRequired();

        b.HasIndex(f => new { f.MarcaId, f.Codigo }).IsUnique().HasDatabaseName("UX_Familia_Marca_Codigo");

        b.HasOne<Marca>().WithMany().HasForeignKey(f => f.MarcaId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Mapeamento de <see cref="Modelo"/>.
///
/// [V] O catálogo de modelos do Vórtice tem 4.431.168 linhas, e o plano de manutenção
/// materializado no produto cartesiano de modelo por item soma outros 7,1 milhões. Aqui é
/// catálogo de algumas centenas: o plano é regra, não linha.
/// </summary>
public sealed class ModeloConfiguracao : IEntityTypeConfiguration<Modelo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Modelo> b)
    {
        b.ToTable("Modelo", "frota");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();
        b.Property(m => m.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(m => m.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(m => m.EstaAtivo).IsRequired();

        b.HasIndex(m => m.Codigo).IsUnique().HasDatabaseName("UX_Modelo_Codigo");
        b.HasIndex(m => m.FamiliaId);

        b.HasOne<Familia>().WithMany().HasForeignKey(m => m.FamiliaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Modelo_Potencia", "[PotenciaCv] IS NULL OR [PotenciaCv] > 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Modelo_IntervaloManutencao",
            "[IntervaloManutencaoHoras] IS NULL OR [IntervaloManutencaoHoras] > 0"));
    }
}

/// <summary>Mapeamento de <see cref="Equipamento"/>.</summary>
public sealed class EquipamentoConfiguracao : IEntityTypeConfiguration<Equipamento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Equipamento> b)
    {
        b.ToTable("Equipamento", "frota");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(e => e.ChavePublica).IsUnique().HasDatabaseName("UX_Equipamento_ChavePublica");

        // O chassi é a identidade de verdade da máquina agrícola — tipo de valor validado.
        b.Property(e => e.Chassi)
            .HasConversion(c => c.Numero, s => Dominio.Comum.Chassi.Criar(s))
            .HasMaxLength(40).IsUnicode(false).IsRequired();

        b.Property(e => e.NumeroSerie).HasMaxLength(40).IsUnicode(false);
        b.Property(e => e.Placa).HasMaxLength(10).IsUnicode(false);
        b.Property(e => e.LocalizacaoDescrita).HasMaxLength(200).IsUnicode(true);
        b.Property(e => e.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Origem).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.HorimetroAtual).HasPrecision(12, 2);
        b.Property(e => e.HorimetroAtualizadoEm).HasPrecision(3);

        b.Property(e => e.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.AlteradoEm).HasPrecision(3);
        b.Property(e => e.ExcluidoEm).HasPrecision(3);

        b.HasIndex(e => e.Chassi)
            .IsUnique()
            .HasFilter("[ExcluidoEm] IS NULL")
            .HasDatabaseName("UX_Equipamento_Chassi");

        // A frota do cliente, que é o que a Visão 360 mostra — inclusive a do concorrente.
        b.HasIndex(e => new { e.ClienteId, e.Origem }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(e => e.ModeloId);
        b.HasIndex(e => e.EquipamentoPaiId);
        b.HasIndex(e => e.EquipamentoSubstitutoId);
        b.HasIndex(e => e.EnderecoId);
        b.HasIndex(e => e.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(e => e.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Modelo>().WithMany().HasForeignKey(e => e.ModeloId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Endereco>().WithMany().HasForeignKey(e => e.EnderecoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(e => e.EquipamentoPaiId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(e => e.EquipamentoSubstitutoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Equipamento_Situacao", "[Situacao] IN ('Estoque','Ativo','Vendido','Baixado')"));
        b.ToTable(x => x.HasCheckConstraint("CK_Equipamento_Origem", "[Origem] IN ('Protheus','Crm')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Equipamento_Horimetro", "[HorimetroAtual] IS NULL OR [HorimetroAtual] >= 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Equipamento_Ano",
            "([AnoFabricacao] IS NULL OR [AnoFabricacao] BETWEEN 1900 AND 2100) " +
            "AND ([AnoModelo] IS NULL OR [AnoModelo] BETWEEN 1900 AND 2100)"));

        // Máquina não é pai nem substituta de si mesma.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Equipamento_Hierarquia",
            "([EquipamentoPaiId] IS NULL OR [EquipamentoPaiId] <> [Id]) " +
            "AND ([EquipamentoSubstitutoId] IS NULL OR [EquipamentoSubstitutoId] <> [Id])"));

        b.Ignore(e => e.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="LeituraDeHorimetro"/>.</summary>
public sealed class LeituraDeHorimetroConfiguracao : IEntityTypeConfiguration<LeituraDeHorimetro>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LeituraDeHorimetro> b)
    {
        b.ToTable("LeituraDeHorimetro", "frota");
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).ValueGeneratedOnAdd();

        b.Property(l => l.Horas).HasPrecision(12, 2).IsRequired();
        b.Property(l => l.Fonte).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(l => l.LidaEm).HasPrecision(3).IsRequired();
        b.Property(l => l.CriadoEm).HasPrecision(3).IsRequired();

        // A série histórica do equipamento, do mais recente para o mais antigo.
        b.HasIndex(l => new { l.EquipamentoId, l.LidaEm }).IsDescending(false, true);

        // Uma leitura por equipamento e instante: a mesma leitura importada duas vezes não
        // vira dois pontos no gráfico.
        b.HasIndex(l => new { l.EquipamentoId, l.LidaEm })
            .IsUnique()
            .HasDatabaseName("UX_LeituraDeHorimetro_Equipamento_Instante");

        b.HasIndex(l => l.RegistradoPorId);

        b.HasOne<Equipamento>().WithMany().HasForeignKey(l => l.EquipamentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(l => l.RegistradoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_LeituraDeHorimetro_Horas", "[Horas] >= 0"));
    }
}
