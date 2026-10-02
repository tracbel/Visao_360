using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="MetaDoCenarioNoMunicipio"/> — a meta escolhida nos Cenários de mercado (issue 263).
///
/// <para><b>Uma por município, categoria e ano fiscal</b>, e o índice único é quem garante: mudar de ideia altera a linha,
/// e a trilha guarda o antes. Duas pessoas gravando a primeira escolha do mesmo município ao mesmo tempo: a segunda bate no
/// índice e recebe o conflito, em vez de criar uma meta duplicada.</para>
///
/// <para>Sem filial na tabela, como a área de atuação: a meta é do município, e a filial é atributo dele. Quem pode gravar em
/// qual município é conferido no caso de uso, pela profundidade da permissão.</para>
/// </summary>
public sealed class MetaDoCenarioNoMunicipioConfiguracao : IEntityTypeConfiguration<MetaDoCenarioNoMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MetaDoCenarioNoMunicipio> b)
    {
        b.ToTable("MetaDoCenarioNoMunicipio", "organizacao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.AnoFiscal).IsRequired();
        b.Property(m => m.Cenario).HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired();
        b.Property(m => m.ValorManual).HasPrecision(9, 2);
        b.Property(m => m.MetaNaEscolha).HasPrecision(9, 2).IsRequired();
        b.Property(m => m.EscolhidaEm).HasPrecision(3).IsRequired();
        b.Property(m => m.AlteradaEm).HasPrecision(3);
        b.Ignore(m => m.GravadaPorId);
        b.Ignore(m => m.GravadaEm);

        // O ÍNDICE ÚNICO COMEÇA PELO MUNICÍPIO, e é ele que serve a chave estrangeira; a leitura da tela é por ano e
        // categoria, e pega todos os municípios de uma vez.
        b.HasIndex(m => new { m.MunicipioId, m.CategoriaDeMaquinaId, m.AnoFiscal })
            .IsUnique()
            .HasDatabaseName("UX_MetaDoCenarioNoMunicipio_Municipio_Categoria_Ano");
        b.HasIndex(m => new { m.AnoFiscal, m.CategoriaDeMaquinaId }).HasDatabaseName("IX_MetaDoCenarioNoMunicipio_Ano_Categoria");
        b.HasIndex(m => m.CategoriaDeMaquinaId);
        b.HasIndex(m => m.EscolhidaPorId);
        b.HasIndex(m => m.AlteradaPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(m => m.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(m => m.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(m => m.EscolhidaPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(m => m.AlteradaPorId).OnDelete(DeleteBehavior.Restrict);

        // A MESMA REGRA DO DOMÍNIO, DITA TAMBÉM NO BANCO: só o manual leva número digitado, e ele é a própria meta.
        // "+ 0" força a comparação numérica no SQLite dos testes, que guarda decimal como texto.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MetaDoCenarioNoMunicipio_Cenario",
            "[Cenario] IN ('Conservador','Moderado','Otimista','Manual')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MetaDoCenarioNoMunicipio_Manual",
            "([Cenario] = 'Manual' AND [ValorManual] IS NOT NULL AND [ValorManual] + 0 = [MetaNaEscolha] + 0) OR " +
            "([Cenario] <> 'Manual' AND [ValorManual] IS NULL)"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MetaDoCenarioNoMunicipio_Meta", "[MetaNaEscolha] + 0 BETWEEN 0 AND 100000"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MetaDoCenarioNoMunicipio_AnoFiscal", "[AnoFiscal] BETWEEN 2000 AND 2100"));

        // A ALTERAÇÃO É TUDO OU NADA: quem e quando, juntos.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MetaDoCenarioNoMunicipio_Alteracao",
            "([AlteradaPorId] IS NULL AND [AlteradaEm] IS NULL) OR ([AlteradaPorId] IS NOT NULL AND [AlteradaEm] IS NOT NULL)"));
    }
}
