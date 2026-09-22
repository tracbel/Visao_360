using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Territorio;

/// <summary>
/// A JANELA FECHADA DO CRÉDITO (issue 157) — o defeito que ela conserta é silencioso.
///
/// <para>A janela terminava no último mês COM DADO. O Banco Central continua acrescentando contrato
/// registrado com atraso nos meses recentes: comparar 12 meses cheios com 12 que ainda estão
/// enchendo mostra o crédito caindo sem ter caído. A carência tira os meses recentes dos DOIS lados.</para>
///
/// <para>Meses sintéticos sobre o modelo de verdade, em SQLite: o último mês com dado é agosto de
/// 2026, e há uma linha em cada mês que o teste precisa distinguir.</para>
/// </summary>
public sealed class JanelaFechadaDoCreditoTestes : IDisposable
{
    private const long Carga = 100;
    private const int Trator = 7080;
    private const int Custeio = 1000;

    // O código do município NO BANCO CENTRAL, que não é o do IBGE (issue 154). Distintos entre si,
    // que é o que a chave única do SICOR exige.
    private const int BcbDeFranca = 6482;
    private const int BcbDeCravinhos = 6483;
    private const int BcbDePrudente = 6484;
    private static readonly DateTime Agora = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly int _franca;
    private readonly int _cravinhos;
    private readonly int _prudente;

    public JanelaFechadaDoCreditoTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureCreated();

        var empresa = Empresa.Criar("010101", "Ribeirão Preto");
        db.Entry(empresa).Property(e => e.Id).CurrentValue = 1;
        db.Empresas.Add(empresa);

        var usuario = Usuario.Criar(
            Guid.NewGuid(), "carga@tracbel.com.br", "Carga", "carga", Email.Criar("carga@tracbel.com.br"), 1, criadoPorId: 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = Carga;
        db.Usuarios.Add(usuario);

        var franca = Municipio.Criar("Franca", "SP", 3516200);
        var cravinhos = Municipio.Criar("Cravinhos", "SP", 3513108);
        var prudente = Municipio.Criar("Presidente Prudente", "SP", 3541406);
        db.Municipios.AddRange(franca, cravinhos, prudente);
        db.SaveChanges();

        _franca = franca.Id;
        _cravinhos = cravinhos.Id;
        _prudente = prudente.Id;

        // FRANCA E CRAVINHOS SÃO DA ADR; Presidente Prudente é São Paulo e não é Região.
        db.MunicipiosDaAreaDeAtuacao.AddRange(
            MunicipioDaAreaDeAtuacao.Registrar(_franca, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 2, Carga, Agora),
            MunicipioDaAreaDeAtuacao.Registrar(_cravinhos, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 3, Carga, Agora));

        // O último mês com dado é 08/2026. As linhas foram escolhidas para que a carência de UM mês
        // mude o resultado: a de agosto sai da janela, e a de agosto do ano anterior entra nela.
        db.CreditosRuraisDeInvestimento.AddRange(
            Linha(_franca, BcbDeFranca, 2026, 8, Trator, 600_000m),
            Linha(_cravinhos, BcbDeCravinhos, 2026, 7, Trator, 300_000m),
            Linha(_franca, BcbDeFranca, 2025, 8, Trator, 500_000m),
            Linha(_prudente, BcbDePrudente, 2026, 8, Trator, 900_000m),

            // Custeio não é máquina: entra no total por produto, e fica fora dos recortes.
            Linha(_franca, BcbDeFranca, 2026, 8, Custeio, 70_000m));

        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    /// <summary>
    /// Uma linha do SICOR. O <b>código do BCB entra na chave única</b> — ele é o que identifica o
    /// município na fonte —, e por isso cada município precisa do seu, senão duas cidades no mesmo
    /// mês e produto colidem.
    /// </summary>
    private static CreditoRuralDeInvestimento Linha(
        int municipioId, int codigoBcb, short ano, byte mes, int produto, decimal valor) =>
        CreditoRuralDeInvestimento.Registrar(
            new CreditoRuralDeInvestimento.Chave(codigoBcb, ano, mes, produto, 154, 71, 431, 9, 1, 14),
            municipioId, valor, 0m, Carga, Agora);

    private async Task<PainelDeCreditoRural> LerAsync(short? carencia)
    {
        await using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await new RepositorioDeCreditoRural(db).LerAsync(12, carencia, CancellationToken.None);
    }

    [Fact]
    public async Task Sem_carencia_decidida_a_janela_termina_no_ultimo_mes_com_dado_e_a_resposta_diz_isso()
    {
        // D-IM-03 EM ABERTO: nenhum mês é descartado, e a tela precisa poder dizer por quê. Descartar
        // meses por um palpite seria pior do que não descartar — mudaria o número sem decisão.
        var painel = await LerAsync(null);

        painel.Janela!.CarenciaDecidida.Should().BeFalse();
        painel.Janela.MesesDeCarencia.Should().Be(0);
        painel.Janela.UltimoMesComDado.Should().Be(new DateOnly(2026, 8, 1));
        painel.Janela.Fim.Should().Be(new DateOnly(2026, 8, 1), "sem carência, o corte é o último mês com dado");
        painel.Janela.Inicio.Should().Be(new DateOnly(2025, 9, 1), "doze meses contando o de corte");
        painel.Janela.FimAnterior.Should().Be(new DateOnly(2025, 8, 1));
        painel.Janela.InicioAnterior.Should().Be(new DateOnly(2024, 9, 1));
        painel.Janela.MesesPorJanela.Should().Be(12);
    }

    [Fact]
    public async Task Com_carencia_nenhum_mes_recente_entra_na_janela()
    {
        // O CRITÉRIO DE ACEITE: "nenhum mês de carência entra". Com um mês de carência, a linha de
        // 08/2026 — R$ 600 mil em Franca — fica de fora dos dois lados da comparação.
        var painel = await LerAsync(1);

        painel.Janela!.CarenciaDecidida.Should().BeTrue();
        painel.Janela.MesesDeCarencia.Should().Be(1);
        painel.Janela.Fim.Should().Be(new DateOnly(2026, 7, 1), "agosto ficou de fora");
        painel.Janela.Inicio.Should().Be(new DateOnly(2025, 8, 1));

        // Franca só tem 08/2026 e 08/2025: a primeira saiu, e a segunda passou a ser da janela RECENTE.
        var franca = painel.PorMunicipio.Single(m => m.Nome == "Franca");
        franca.Janelas.Valor.Should().Be(500_000m, "os R$ 600 mil de agosto de 2026 não entram");
        franca.Janelas.ValorAnterior.Should().Be(0m);

        painel.SaoPaulo!.Janelas.Valor.Should().Be(800_000m, "500 mil de Franca + 300 mil de Cravinhos");
        painel.SaoPaulo.Janelas.Valor.Should().NotBe(1_800_000m, "é o número que a janela aberta daria");
    }

    [Fact]
    public async Task A_regiao_e_sao_paulo_vem_lado_a_lado_com_linhas_valor_e_a_janela_anterior()
    {
        // Sem São Paulo ao lado, "R$ 900 mil na Região" é número solto: pode ser metade do estado ou
        // um vigésimo. A tela mostrava só a Região, com um filtro "só a ADR".
        var painel = await LerAsync(null);

        painel.Regiao!.Recorte.Should().Be("Região (ADR)");
        painel.Regiao.Municipios.Should().Be(2, "Franca e Cravinhos têm linha na janela recente");
        painel.Regiao.Janelas.Linhas.Should().Be(2);
        painel.Regiao.Janelas.Valor.Should().Be(900_000m, "600 mil de Franca + 300 mil de Cravinhos");
        painel.Regiao.Janelas.ValorAnterior.Should().Be(500_000m, "os R$ 500 mil de 08/2025");

        painel.SaoPaulo!.Recorte.Should().Be("São Paulo");
        painel.SaoPaulo.Municipios.Should().Be(3, "Presidente Prudente entra no estado e não na Região");
        painel.SaoPaulo.Janelas.Valor.Should().Be(1_800_000m);

        // A fatia que a tela mostra: 900 de 1.800 é metade do crédito de máquina do estado.
        (painel.Regiao.Janelas.Valor / painel.SaoPaulo.Janelas.Valor).Should().Be(0.5m);
    }

    [Fact]
    public async Task Os_recortes_contam_so_maquina_e_o_custeio_fica_de_fora()
    {
        // "Máquina" é decisão de negócio (ParametroDoPotencial.ProdutosDeMaquinaNoSicor): custeio
        // aparece na lista de produtos, mas não engorda o crédito de máquina da Região.
        var painel = await LerAsync(null);

        painel.PorProduto.Should().Contain(p => p.Codigo == Custeio && !p.EhMaquina);
        painel.PorProduto.Single(p => p.Codigo == Trator).EhMaquina.Should().BeTrue();

        painel.SaoPaulo!.Janelas.Valor.Should().Be(1_800_000m, "os R$ 70 mil de custeio não entram");
    }

    [Fact]
    public async Task Banco_sem_credito_nao_inventa_janela()
    {
        // Fonte não carregada é ausência, e não uma janela de zeros: a tela mostra "não carregado".
        //
        // A EXCLUSÃO VAI PELO CONTEXTO DA CARGA, com filial: a linha do SICOR é auditada, e a trilha
        // recusa gravação auditada sem filial — a mesma regra que vale no servidor.
        await using var db = new CrmDbContext(_opcoes, new ContextoDeCargaDeSistema(Carga, 1, new HashSet<int> { 1 }));
        db.CreditosRuraisDeInvestimento.RemoveRange(db.CreditosRuraisDeInvestimento);
        await db.SaveChangesAsync();

        var painel = await new RepositorioDeCreditoRural(db).LerAsync(12, 2, CancellationToken.None);

        painel.UltimoMes.Should().BeNull();
        painel.Janela.Should().BeNull();
        painel.Regiao.Should().BeNull();
        painel.SaoPaulo.Should().BeNull();
    }
}
