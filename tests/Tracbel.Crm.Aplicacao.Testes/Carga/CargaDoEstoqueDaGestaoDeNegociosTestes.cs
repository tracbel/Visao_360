using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoEstoque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O ESTOQUE E A COBERTURA DA API GESTÃO DE NEGÓCIOS (28/09/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada cria uma máquina por chassi interno, casa a filial pelo nome, deixa a loja sem código de fora e
/// contada, e grava a cobertura por mês (a chave vira AAAA-MM) e por grupo;</item>
/// <item>a segunda leitura igual grava zero; a reserva que muda deixa a trilha como integração;</item>
/// <item>a máquina vendida (que some) é excluída e, se volta, é reativada na mesma linha; o mês que sai da janela da
/// cobertura também;</item>
/// <item>as travas: formato ilegível acima de 5% e remoção acima de 20% abortam tudo; --aceitar-remocao passa;</item>
/// <item>a simulação não grava nada, e o relatório não tem nome de ninguém.</item>
/// </list>
/// </summary>
public sealed class CargaDoEstoqueDaGestaoDeNegociosTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;

    public CargaDoEstoqueDaGestaoDeNegociosTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private Task<Resultado<RelatorioDoEstoque>> Tentar(LeituraDoEstoqueNaOrigem? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null) =>
        Sincronia(DaCarga, leitura ?? Leitura(), quando ?? Agora, _semente.Operador).ExecutarAsync(simular, aceitarRemocao, CancellationToken.None);

    private async Task<RelatorioDoEstoque> Sincronizar(LeituraDoEstoqueNaOrigem? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null)
    {
        var resultado = await Tentar(leitura, simular, aceitarRemocao, quando);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private const string Estoque = CargaDoEstoqueDaGestaoDeNegocios.EtapaDoEstoque;
    private const string Cobertura_ = CargaDoEstoqueDaGestaoDeNegocios.EtapaDaCobertura;

    [Fact]
    public async Task A_primeira_rodada_cria_as_maquinas_casa_a_filial_pelo_nome_e_grava_a_cobertura()
    {
        var relatorio = await Sincronizar();

        relatorio.Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeLidas).Should().Be(5);
        relatorio.Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloSemFilial).Should().Be(1, "a Digital não é filial do CRM");
        relatorio.Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloNoPatio).Should().Be(3);
        relatorio.Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDePedidos).Should().Be(1);
        relatorio.Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeReservadas).Should().Be(1);
        relatorio.Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeNovas).Should().Be(4);
        relatorio.Valor(Cobertura_, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeNovas).Should().Be(4);

        await using var db = Sistema();
        var maquinas = await db.EquipamentosEmEstoque.IgnoreQueryFilters().AsNoTracking().OrderBy(e => e.Chaint).ToListAsync();
        maquinas.Select(m => m.Chaint).Should().Equal("100001", "100002", "100003", "100004");
        (maquinas[0].EmpresaId, maquinas[0].Pago, maquinas[0].Reservado, maquinas[0].EntradaEm).Should().Be((_semente.Ituverava, true, false, (DateOnly?)new DateOnly(2026, 6, 1)));
        maquinas[2].EmpresaId.Should().Be(_semente.RibeiraoPreto, "\"Ribeirão Preto\" é 010101 pelo de-para das lojas");
        (maquinas[3].EhPedidoDeFabrica, maquinas[3].Chassi, maquinas[3].ChegadaPrevistaEm, maquinas[3].SituacaoNaFabrica)
            .Should().Be((true, (string?)null, (DateOnly?)new DateOnly(2026, 11, 30), "Confirmado"));

        var cobertura = await db.CoberturasDoEstoque.AsNoTracking().OrderBy(c => c.Recorte).ThenBy(c => c.Chave).ToListAsync();
        cobertura.Select(c => (c.Recorte, c.Chave)).Should().Equal(("GRUPO", "AMS"), ("GRUPO", "TRATOR 6000"), ("MES", "2026-07"), ("MES", "2026-08"));
        cobertura[3].Competencia.Should().Be(new DateOnly(2026, 8, 1));
        (cobertura[3].MesesDeEstoque, cobertura[3].Vendas).Should().Be((3.12m, 152));

        (await db.PontosDeSincronismo.AsNoTracking().Select(p => p.Fluxo).ToListAsync())
            .Should().BeEquivalentTo(EquipamentoEmEstoque.FluxoDaCarga, CoberturaDoEstoque.FluxoDaCarga);
    }

    [Fact]
    public async Task A_segunda_leitura_igual_grava_zero_e_a_reserva_que_muda_fica_na_trilha()
    {
        await Sincronizar();
        (await Sincronizar(quando: Agora.AddHours(1))).Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeIguais).Should().Be(8, "4 máquinas e 4 itens");

        var reservada = Estoque();
        reservada[0] = Maquina("100001", reservado: "Sim");
        (await Sincronizar(Leitura(reservada), quando: Agora.AddHours(2))).Valor(CargaDoEstoqueDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(1);

        await using var db = Sistema();
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(EquipamentoEmEstoque)).ToListAsync();
        trilha.Should().ContainSingle(a => a.Campo == nameof(EquipamentoEmEstoque.Reservado));
        trilha.Should().OnlyContain(a => a.Origem == OrigemDaOperacao.Integracao && a.SistemaId != null);
    }

    [Fact]
    public async Task A_maquina_vendida_sai_e_a_que_volta_e_reativada_na_mesma_linha()
    {
        await Sincronizar();
        long idDaLinha;
        await using (var db = Sistema()) idDaLinha = (await db.EquipamentosEmEstoque.IgnoreQueryFilters().AsNoTracking().SingleAsync(e => e.Chaint == "100002")).Id;

        var vendida = Estoque().Where(m => m.Chaint != "100002").ToList();
        (await Sincronizar(Leitura(vendida), quando: Agora.AddHours(1))).Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);

        (await Sincronizar(quando: Agora.AddHours(2))).Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeReativadas).Should().Be(1);
        await using (var db = Sistema())
        {
            var voltou = await db.EquipamentosEmEstoque.IgnoreQueryFilters().AsNoTracking().SingleAsync(e => e.Chaint == "100002");
            (voltou.ExcluidoEm, voltou.Id).Should().Be(((DateTime?)null, idDaLinha));
        }
    }

    [Fact]
    public async Task O_mes_que_sai_da_janela_da_cobertura_e_excluido()
    {
        await Sincronizar();

        var setembro = new List<ItemDaCoberturaNaOrigem>(Cobertura().Where(c => c.Chave != "Jul/2026")) { new("MES", "Set/2026", "2.9", "160") };
        var relatorio = await Sincronizar(Leitura(cobertura: setembro), quando: Agora.AddDays(3));

        (relatorio.Valor(Cobertura_, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeNovas), relatorio.Valor(Cobertura_, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeExcluidas))
            .Should().Be((1, 1));
        await using var db = Sistema();
        (await db.CoberturasDoEstoque.AsNoTracking().SingleAsync(c => c.Chave == "2026-07")).ExcluidoEm.Should().Be(Agora.AddDays(3));
    }

    [Fact]
    public async Task Mais_de_5_por_cento_ilegivel_aborta_e_nao_grava_nada()
    {
        var estoque = EstoqueGrande(20);
        estoque[0] = Maquina("200001", reservado: "Talvez");
        estoque[1] = Maquina("200002", entrada: "01/06/2026");

        var resultado = await Tentar(Leitura(estoque));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("2 de 20").And.Contain("ABORTADA");
        await using var db = Sistema();
        (await db.EquipamentosEmEstoque.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Remocao_acima_de_20_por_cento_aborta_como_o_painel_com_periodo_e_aceitar_remocao_passa()
    {
        // O PAINEL COM PERÍODO PERDE OS PEDIDOS À FÁBRICA — um terço das linhas. É o que a trava pega.
        await Sincronizar(Leitura(EstoqueGrande(30)));

        var parcial = Leitura(EstoqueGrande(20));
        var resultado = await Tentar(parcial, quando: Agora.AddHours(1));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("10 de 30").And.Contain("período").And.Contain("--aceitar-remocao");

        (await Sincronizar(parcial, aceitarRemocao: true, quando: Agora.AddHours(2)))
            .Valor(Estoque, CargaDoEstoqueDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(10);
    }

    [Fact]
    public async Task A_simulacao_nao_grava_nada_e_o_estoque_vazio_nao_exclui_nada()
    {
        (await Sincronizar(simular: true)).Simulada.Should().BeTrue();
        await using (var db = Sistema())
            (await db.EquipamentosEmEstoque.IgnoreQueryFilters().CountAsync()).Should().Be(0);

        await Sincronizar();
        var vazio = await Tentar(Leitura([]), quando: Agora.AddHours(1));
        vazio.EhSucesso.Should().BeFalse();
        vazio.Erro.Should().Contain("veio vazio");
        await using (var db = Sistema())
            (await db.EquipamentosEmEstoque.CountAsync(e => e.ExcluidoEm == null)).Should().Be(4);
    }

    [Theory]
    [InlineData(19, 19, false)]
    [InlineData(30, 6, false)]
    [InlineData(30, 7, true)]
    public void A_trava_de_remocao_e_20_por_cento_a_partir_de_20(int vigentes, int aExcluir, bool aborta) =>
        CargaDoEstoqueDaGestaoDeNegocios.RemocaoPassaDaTrava(vigentes, aExcluir).Should().Be(aborta);
}
