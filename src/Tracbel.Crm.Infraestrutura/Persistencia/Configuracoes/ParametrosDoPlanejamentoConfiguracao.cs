using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="ParametroDoPlanejamento"/> — a sazonalidade e os pesos do IOC, com vigência (issue 256).
///
/// <para><b>A semente são os números do protótipo da pasta 360</b> (SAZ_DEF e IOC_DEF), e a justificativa diz isso:
/// foram trazidos para o planejamento sair desde o primeiro dia, e trocá-los é uma vigência nova pela tela de
/// Configurações — sem publicação.</para>
/// </summary>
public sealed class ParametroDoPlanejamentoConfiguracao : IEntityTypeConfiguration<ParametroDoPlanejamento>
{
    /// <summary>Quando a semente passa a valer: o dia em que o Ricardo pediu o protótipo inteiro no CRM.</summary>
    internal static readonly DateOnly InicioDaSemente = new(2026, 9, 27);

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ParametroDoPlanejamento> b)
    {
        b.ToTable("ParametroDoPlanejamento", "organizacao");
        MapeamentoDeVigencia.Mapear(b, "ParametroDoPlanejamento");

        // PERCENTUAIS E PESOS COM DUAS CASAS: o que o protótipo usava e o que um formulário pede.
        foreach (var coluna in Colunas)
            b.Property<decimal>(coluna).HasPrecision(5, 2).IsRequired();

        b.HasIndex(p => p.VigenteDesde)
            .IsUnique()
            .HasFilter("[RevogadoEm] IS NULL")
            .HasDatabaseName("UX_ParametroDoPlanejamento_Vigencia");

        // A MESMA REGRA DO DOMÍNIO, DITA TAMBÉM NO BANCO: os doze meses somam 100 (com a folga do arredondamento),
        // cada mês e cada peso vão de 0 a 100 e ao menos um peso conta.
        //
        // O "+ 0" FORÇA A COMPARAÇÃO NUMÉRICA no SQLite dos testes, que guarda decimal como texto: sem ele,
        // "8.56" <= 100 é comparação de texto, dá falso, e a semente não entra. No SQL Server a conta não muda nada.
        var meses = string.Join(" + ", Meses.Select(m => $"[{m}]"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ParametroDoPlanejamento_Sazonalidade",
            $"({meses}) BETWEEN 99.95 AND 100.05"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ParametroDoPlanejamento_Faixa",
            string.Join(" AND ", Colunas.Select(c => $"[{c}] + 0 BETWEEN 0 AND 100"))));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ParametroDoPlanejamento_Pesos",
            $"({string.Join(" + ", Pesos.Select(p => $"[{p}]"))}) > 0"));

        b.HasData(new
        {
            Id = 1,
            // SAZ_DEF do protótipo, casa por casa — soma 100,00 exatos.
            SazonalidadeJaneiro = 6.52m, SazonalidadeFevereiro = 6.89m, SazonalidadeMarco = 8.56m, SazonalidadeAbril = 8.59m,
            SazonalidadeMaio = 8.87m, SazonalidadeJunho = 8.58m, SazonalidadeJulho = 7.84m, SazonalidadeAgosto = 8.75m,
            SazonalidadeSetembro = 9.15m, SazonalidadeOutubro = 10.51m, SazonalidadeNovembro = 7.44m, SazonalidadeDezembro = 8.30m,
            PesoDoPotencial = 25m, PesoDaCobertura = 20m, PesoDoCredito = 15m, PesoDaRentabilidade = 15m,
            PesoDosClientes = 10m, PesoDaRealizacao = 5m, PesoDaPenetracao = 10m,
            VigenteDesde = InicioDaSemente,
            Justificativa = "Valores do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar: a sazonalidade " +
                            "mensal dele (6,52% em janeiro a 10,51% em outubro) e os pesos do IOC — 25 potencial, 20 cobertura, " +
                            "15 crédito, 15 rentabilidade, 10 clientes, 5 realização e 10 penetração. Decisão do Ricardo de " +
                            "27/09/2026 (issue 256).",
            InformadoPorId = (long?)null,
            InformadoEm = new DateTime(2026, 9, 27, 0, 0, 0, DateTimeKind.Utc),
            RevogadoEm = (DateTime?)null,
            RevogadoPorId = (long?)null,
            MotivoDaRevogacao = (string?)null
        });
    }

    private static readonly string[] Meses =
    [
        nameof(ParametroDoPlanejamento.SazonalidadeJaneiro), nameof(ParametroDoPlanejamento.SazonalidadeFevereiro),
        nameof(ParametroDoPlanejamento.SazonalidadeMarco), nameof(ParametroDoPlanejamento.SazonalidadeAbril),
        nameof(ParametroDoPlanejamento.SazonalidadeMaio), nameof(ParametroDoPlanejamento.SazonalidadeJunho),
        nameof(ParametroDoPlanejamento.SazonalidadeJulho), nameof(ParametroDoPlanejamento.SazonalidadeAgosto),
        nameof(ParametroDoPlanejamento.SazonalidadeSetembro), nameof(ParametroDoPlanejamento.SazonalidadeOutubro),
        nameof(ParametroDoPlanejamento.SazonalidadeNovembro), nameof(ParametroDoPlanejamento.SazonalidadeDezembro)
    ];

    private static readonly string[] Pesos =
    [
        nameof(ParametroDoPlanejamento.PesoDoPotencial), nameof(ParametroDoPlanejamento.PesoDaCobertura),
        nameof(ParametroDoPlanejamento.PesoDoCredito), nameof(ParametroDoPlanejamento.PesoDaRentabilidade),
        nameof(ParametroDoPlanejamento.PesoDosClientes), nameof(ParametroDoPlanejamento.PesoDaRealizacao),
        nameof(ParametroDoPlanejamento.PesoDaPenetracao)
    ];

    private static readonly string[] Colunas = [.. Meses, .. Pesos];
}

/// <summary>
/// Mapeamento de <see cref="ShareAlvoDaCategoria"/> — o share-alvo de cada categoria de máquina, com vigência (issue 256).
///
/// <para><b>A semente é o 31% do protótipo</b> para as categorias que ele planejava — trator, plantadeira, colheitadeira,
/// pulverizador e colhedora de cana. Implemento e agricultura de precisão ficam sem share: o protótipo não os tinha, e
/// o planejamento dessas duas diz "sem share-alvo registrado" em vez de inventar um.</para>
/// </summary>
public sealed class ShareAlvoDaCategoriaConfiguracao : IEntityTypeConfiguration<ShareAlvoDaCategoria>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShareAlvoDaCategoria> b)
    {
        b.ToTable("ShareAlvoDaCategoria", "organizacao");
        MapeamentoDeVigencia.Mapear(b, "ShareAlvoDaCategoria");

        b.Property(s => s.CategoriaDeMaquinaId).IsRequired();
        b.Property(s => s.Percentual).HasPrecision(5, 2).IsRequired();

        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(s => s.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);

        // O ÍNDICE ÚNICO COMEÇA PELA CATEGORIA, e é ele que serve a chave estrangeira.
        b.HasIndex(s => new { s.CategoriaDeMaquinaId, s.VigenteDesde })
            .IsUnique()
            .HasFilter("[RevogadoEm] IS NULL")
            .HasDatabaseName("UX_ShareAlvoDaCategoria_Categoria_Vigencia");

        // "+ 0" pelo mesmo motivo da sazonalidade: comparação numérica também no SQLite dos testes.
        b.ToTable(t => t.HasCheckConstraint("CK_ShareAlvoDaCategoria_Percentual", "[Percentual] + 0 > 0 AND [Percentual] + 0 <= 100"));

        // TRATOR 1, PLANTADEIRA 2, COLHEITADEIRA 3, PULVERIZADOR 4 E COLHEDORA DE CANA 7 — os identificadores da
        // semente do catálogo (CatalogoDeCulturasConfiguracao).
        var id = 0;
        foreach (var categoria in new[] { 1, 2, 3, 4, 7 })
            b.HasData(new
            {
                Id = ++id,
                CategoriaDeMaquinaId = categoria,
                Percentual = 31m,
                VigenteDesde = ParametroDoPlanejamentoConfiguracao.InicioDaSemente,
                Justificativa = "Share-alvo padrão do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar. " +
                                "Trazido por decisão do Ricardo de 27/09/2026 (issue 256).",
                InformadoPorId = (long?)null,
                InformadoEm = new DateTime(2026, 9, 27, 0, 0, 0, DateTimeKind.Utc),
                RevogadoEm = (DateTime?)null,
                RevogadoPorId = (long?)null,
                MotivoDaRevogacao = (string?)null
            });
    }
}
