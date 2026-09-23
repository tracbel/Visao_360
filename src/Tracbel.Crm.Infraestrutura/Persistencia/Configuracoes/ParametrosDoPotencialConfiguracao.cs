using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// O QUE TODA VIGÊNCIA DE PARÂMETRO TEM NO BANCO — data de início, justificativa, autor e revogação —,
/// mapeado igual nas três tabelas (issue 71).
///
/// <para><b>Uma vigência de pé por chave e data.</b> O índice único filtra as revogadas: revogar libera a
/// data para a vigência que corrige o erro, e as duas continuam gravadas.</para>
/// </summary>
internal static class MapeamentoDeVigencia
{
    /// <summary>Mapeia as colunas da vigência e as duas chaves para o usuário.</summary>
    /// <typeparam name="T">O parâmetro.</typeparam>
    /// <param name="b">O construtor da entidade.</param>
    /// <param name="tabela">O nome da tabela, para as restrições.</param>
    public static void Mapear<T>(EntityTypeBuilder<T> b, string tabela) where T : ParametroComVigencia
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.VigenteDesde).IsRequired();
        b.Property(p => p.Justificativa).HasMaxLength(ParametroComVigencia.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(p => p.InformadoEm).HasPrecision(3).IsRequired();
        b.Property(p => p.RevogadoEm).HasPrecision(3);
        b.Property(p => p.MotivoDaRevogacao).HasMaxLength(ParametroComVigencia.TamanhoDoTexto).IsUnicode(true);

        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.InformadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.RevogadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(p => p.InformadoPorId);
        b.HasIndex(p => p.RevogadoPorId);

        // REVOGAÇÃO É TUDO OU NADA: quando, quem e por quê. Metade dela seria uma vigência que some do
        // cálculo sem ninguém conseguir dizer por quê.
        b.ToTable(t => t.HasCheckConstraint(
            $"CK_{tabela}_Revogacao",
            "([RevogadoEm] IS NULL AND [RevogadoPorId] IS NULL AND [MotivoDaRevogacao] IS NULL) OR " +
            "([RevogadoEm] IS NOT NULL AND [RevogadoPorId] IS NOT NULL AND [MotivoDaRevogacao] IS NOT NULL)"));
    }
}

/// <summary>
/// Mapeamento de <see cref="RegraDePotencial"/> — a regra de cada cultura, com vigência.
///
/// <para><b>A semente continua sendo a regra do gerente comercial</b>, a mesma de 13/09/2026, agora como a
/// primeira vigência: sem autor (quem a informou foi o commit revisado), com o texto de origem como
/// justificativa e sem anos de renovação, que ninguém informou. A partir daqui, muda pelo administrador
/// (issue 71), e não por migração.</para>
/// </summary>
public sealed class RegraDePotencialConfiguracao : IEntityTypeConfiguration<RegraDePotencial>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RegraDePotencial> b)
    {
        b.ToTable("RegraDePotencial", "organizacao");
        MapeamentoDeVigencia.Mapear(b, "RegraDePotencial");

        b.Property(r => r.ProdutoCodigoIbge).IsRequired();
        b.Property(r => r.ProdutoNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(r => r.HectaresPorMaquina).HasPrecision(10, 2).IsRequired();
        b.Property(r => r.AnosDeRenovacao).HasPrecision(4, 1);
        b.Property(r => r.ModeloDeReferencia).HasMaxLength(60).IsUnicode(true).IsRequired();
        b.Property(r => r.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        // A LIGAÇÃO COM O CATÁLOGO (issue 165), anulável: a vigência do café de 13/09/2026 nasceu antes
        // dele, e o passado não se reescreve. A migração liga a linha existente à cultura semeada.
        b.HasOne<Cultura>().WithMany().HasForeignKey(r => r.CulturaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<CategoriaDeMaquina>().WithMany().HasForeignKey(r => r.CategoriaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(r => r.CulturaId);
        b.HasIndex(r => r.CategoriaDeMaquinaId);

        b.HasIndex(r => new { r.ProdutoCodigoIbge, r.VigenteDesde })
            .IsUnique()
            .HasFilter("[RevogadoEm] IS NULL")
            .HasDatabaseName("UX_RegraDePotencial_Produto_Vigencia");

        b.ToTable(t => t.HasCheckConstraint("CK_RegraDePotencial_Situacao", "[Situacao] IN ('AConfirmar','Confirmada')"));
        b.ToTable(t => t.HasCheckConstraint("CK_RegraDePotencial_HectaresPorMaquina", "[HectaresPorMaquina] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_RegraDePotencial_AnosDeRenovacao", "[AnosDeRenovacao] IS NULL OR [AnosDeRenovacao] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_RegraDePotencial_Produto", "[ProdutoCodigoIbge] > 0"));

        b.HasData(new
        {
            Id = 1,
            ProdutoCodigoIbge = 40139,
            ProdutoNome = "Café (em grão) Total",
            HectaresPorMaquina = 10m,
            AnosDeRenovacao = (decimal?)null,
            ModeloDeReferencia = "3036N",
            Situacao = SituacaoDaRegraDePotencial.AConfirmar,
            CulturaId = (int?)1,
            CategoriaDeMaquinaId = (int?)1,
            VigenteDesde = new DateOnly(2026, 9, 13),
            Justificativa = "Exemplo do gerente comercial no pedido de 13/09/2026: \"na cultura de café, existe " +
                            "potencial de 1 trator 3036N a cada 10 hectares\". Não confirmados: aplicabilidade, " +
                            "vigência, horizonte e arredondamento.",
            InformadoPorId = (long?)null,
            InformadoEm = new DateTime(2026, 9, 13, 0, 0, 0, DateTimeKind.Utc),
            RevogadoEm = (DateTime?)null,
            RevogadoPorId = (long?)null,
            MotivoDaRevogacao = (string?)null
        });
    }
}

/// <summary>
/// Mapeamento de <see cref="ParametroDoPotencial"/> — os parâmetros gerais do modelo, com vigência.
///
/// <para><b>A semente é o que o texto do Ricardo de 21/09/2026 decidiu</b> (comentário na issue 63), e só
/// isso: janela de 12 contra 12, 70% contratos e 30% valor, faixas 1,0 / 1,2 / 1,4 e percepção de ±5%. O
/// que está em aberto nasce vazio — pesos dos indicadores, limites do fator e o nome da faixa do meio.</para>
/// </summary>
public sealed class ParametroDoPotencialConfiguracao : IEntityTypeConfiguration<ParametroDoPotencial>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ParametroDoPotencial> b)
    {
        b.ToTable("ParametroDoPotencial", "organizacao");
        MapeamentoDeVigencia.Mapear(b, "ParametroDoPotencial");

        b.Property(p => p.MesesDaJanela).IsRequired();
        b.Property(p => p.PesoDosContratosNoCredito).HasPrecision(4, 3).IsRequired();
        b.Property(p => p.LimiteDeRetracao).HasPrecision(5, 3).IsRequired();
        b.Property(p => p.LimiteDeAquecimento).HasPrecision(5, 3).IsRequired();
        b.Property(p => p.LimiteDeSuperaquecimento).HasPrecision(5, 3).IsRequired();
        b.Property(p => p.NomeDaFaixaIntermediaria).HasMaxLength(40).IsUnicode(true);
        b.Property(p => p.LimiteDaPercepcao).HasPrecision(5, 2).IsRequired();
        b.Property(p => p.PesoDoIndicadorDePreco).HasPrecision(5, 3);
        b.Property(p => p.PesoDoIndicadorDeCredito).HasPrecision(5, 3);
        b.Property(p => p.PesoDoIndicadorComercial).HasPrecision(5, 3);
        b.Property(p => p.FatorMinimo).HasPrecision(5, 3);
        b.Property(p => p.FatorMaximo).HasPrecision(5, 3);

        b.HasIndex(p => p.VigenteDesde)
            .IsUnique()
            .HasFilter("[RevogadoEm] IS NULL")
            .HasDatabaseName("UX_ParametroDoPotencial_Vigencia");

        b.ToTable(t => t.HasCheckConstraint("CK_ParametroDoPotencial_Janela", "[MesesDaJanela] BETWEEN 1 AND 60"));
        b.ToTable(t => t.HasCheckConstraint("CK_ParametroDoPotencial_PesoDosContratos", "[PesoDosContratosNoCredito] BETWEEN 0 AND 1"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ParametroDoPotencial_Faixas",
            "[LimiteDeRetracao] > 0 AND [LimiteDeRetracao] < [LimiteDeAquecimento] AND [LimiteDeAquecimento] < [LimiteDeSuperaquecimento]"));
        b.ToTable(t => t.HasCheckConstraint("CK_ParametroDoPotencial_Percepcao", "[LimiteDaPercepcao] > 0 AND [LimiteDaPercepcao] <= 50"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ParametroDoPotencial_Fator",
            "([FatorMinimo] IS NULL AND [FatorMaximo] IS NULL) OR ([FatorMinimo] > 0 AND [FatorMinimo] < 1 AND [FatorMaximo] > 1)"));

        // A CARÊNCIA NÃO COME A JANELA INTEIRA (issue 157): descartar 12 meses de uma janela de 12
        // deixaria os dois lados vazios. A mesma regra do domínio, dita também no banco.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ParametroDoPotencial_CarenciaDoSicor",
            "[MesesDeCarenciaDoSicor] IS NULL OR ([MesesDeCarenciaDoSicor] >= 0 AND [MesesDeCarenciaDoSicor] < [MesesDaJanela])"));

        b.HasData(new
        {
            Id = 1,
            MesesDaJanela = (short)12,
            PesoDosContratosNoCredito = 0.70m,
            LimiteDeRetracao = 1.00m,
            LimiteDeAquecimento = 1.20m,
            LimiteDeSuperaquecimento = 1.40m,
            NomeDaFaixaIntermediaria = (string?)null,
            LimiteDaPercepcao = 5.00m,
            PesoDoIndicadorDePreco = (decimal?)null,
            PesoDoIndicadorDeCredito = (decimal?)null,
            PesoDoIndicadorComercial = (decimal?)null,
            FatorMinimo = (decimal?)null,
            FatorMaximo = (decimal?)null,

            // EM ABERTO (D-IM-03): quantos meses o atraso do Banco Central ocupa é medição que
            // ninguém fez. Nulo é "não decidida" — a janela não descarta mês nenhum, e a tela diz.
            MesesDeCarenciaDoSicor = (short?)null,
            VigenteDesde = new DateOnly(2026, 9, 21),
            Justificativa = "Texto-base do Ricardo de 21/09/2026 (issue 63): os últimos 12 meses contra os 12 anteriores; " +
                            "crédito com 70% de contratos e 30% de valor; < 1 retraído, > 1,2 aquecido, > 1,4 super aquecido; " +
                            "percepção do gestor de −5% a +5% por município. Em aberto: pesos dos indicadores, limites do " +
                            "fator e o nome da faixa entre 1,0 e 1,2.",
            InformadoPorId = (long?)null,
            InformadoEm = new DateTime(2026, 9, 21, 0, 0, 0, DateTimeKind.Utc),
            RevogadoEm = (DateTime?)null,
            RevogadoPorId = (long?)null,
            MotivoDaRevogacao = (string?)null
        });

        // =========================================================================================
        // A SEGUNDA VIGÊNCIA (D-P05 decidida em 23/09/2026, issue 74)
        //
        // Ela NÃO altera a de 21/09: o passado não se reescreve, e o cálculo daqueles dias continua
        // usando o que valia lá — sem pesos, portanto sem fator. A partir de 23/09 valem os pesos.
        //
        // OS NÚMEROS SÃO OS MEDIDOS NO PROTÓTIPO, e a justificativa diz isso: enquanto ela valer, todo
        // número que passa pelo fator carrega o selo de estimativa. Trocar é uma vigência nova pela tela
        // do Administrador — sem publicação.
        //
        // O PESO DA PERCEPÇÃO É 1,0, E NÃO O 0,4 DO PROTÓTIPO. Lá a percepção ia de −2 a +2 e entrava
        // dividida por 2, o que dava ±40% de efeito; a decisão D-P04 trocou a escala para ±5 pontos
        // percentuais justamente para tirar aqueles ±40%. Carregar o 0,4 junto com a escala nova daria
        // ±2% — um vigésimo do que o protótipo pretendia, e menos do que o rótulo "−5% a +5%" promete.
        // Com 1,0, o rótulo é literal (exato no crédito neutro; o crédito amplifica, ver FatorDeCiclo).
        // =========================================================================================
        b.HasData(new
        {
            Id = 2,
            MesesDaJanela = (short)12,
            PesoDosContratosNoCredito = 0.70m,
            LimiteDeRetracao = 1.00m,
            LimiteDeAquecimento = 1.20m,
            LimiteDeSuperaquecimento = 1.40m,
            NomeDaFaixaIntermediaria = (string?)null,
            LimiteDaPercepcao = 5.00m,
            PesoDoIndicadorDePreco = (decimal?)0.40m,
            PesoDoIndicadorDeCredito = (decimal?)0.50m,
            PesoDoIndicadorComercial = (decimal?)1.00m,
            FatorMinimo = (decimal?)0.40m,
            FatorMaximo = (decimal?)1.50m,
            MesesDeCarenciaDoSicor = (short?)null,
            MinimoDeLinhasNoCredito = (int?)null,
            VigenteDesde = new DateOnly(2026, 9, 23),
            // A JUSTIFICATIVA CABE EM ParametroComVigencia.TamanhoDoTexto (400). A primeira versão tinha
            // 439 e passou nos testes locais — o SQLite não valida tamanho de varchar, o SQL Server sim,
            // e quem pegou foi o teste de contêiner no CI. O detalhe longo mora no documento 48, §7.4.
            Justificativa = "D-P05 decidida em 23/09/2026 (documento 48, §5.1): pesos medidos no protótipo, a confirmar — " +
                            "0,40 no preço e na rentabilidade, 0,50 no crédito, fator entre 0,40 e 1,50. A percepção pesa " +
                            "1,00, e não os 0,40 do protótipo: a D-P04 trocou a escala de −2 a +2 para ±5 pontos " +
                            "percentuais, e 1,00 torna esse rótulo literal. O termo de troca fica fora até a issue 70.",
            InformadoPorId = (long?)null,
            InformadoEm = new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc),
            RevogadoEm = (DateTime?)null,
            RevogadoPorId = (long?)null,
            MotivoDaRevogacao = (string?)null
        });
    }
}

/// <summary>Mapeamento de <see cref="PercepcaoDoGestor"/> — o ajuste do gestor por município, com vigência.</summary>
public sealed class PercepcaoDoGestorConfiguracao : IEntityTypeConfiguration<PercepcaoDoGestor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PercepcaoDoGestor> b)
    {
        b.ToTable("PercepcaoDoGestor", "organizacao");
        MapeamentoDeVigencia.Mapear(b, "PercepcaoDoGestor");

        b.Property(p => p.MunicipioId).IsRequired();
        b.Property(p => p.Percentual).HasPrecision(5, 2).IsRequired();

        b.HasOne<Municipio>().WithMany().HasForeignKey(p => p.MunicipioId).OnDelete(DeleteBehavior.Restrict);

        // O ÍNDICE ÚNICO COMEÇA PELO MUNICÍPIO, e é ele que serve a chave estrangeira.
        b.HasIndex(p => new { p.MunicipioId, p.VigenteDesde })
            .IsUnique()
            .HasFilter("[RevogadoEm] IS NULL")
            .HasDatabaseName("UX_PercepcaoDoGestor_Municipio_Vigencia");

        // O LIMITE FINO É O DA VIGÊNCIA DOS PARÂMETROS GERAIS, conferido pelo caso de uso; o banco segura o
        // teto do próprio parâmetro, para nenhum caminho gravar um ajuste absurdo.
        b.ToTable(t => t.HasCheckConstraint("CK_PercepcaoDoGestor_Percentual", "[Percentual] BETWEEN -50 AND 50"));
    }
}
