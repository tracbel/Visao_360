using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasOrdensDeServico;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasPecas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// AS PEÇAS DO PROTHEUS (02/10/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>o faturamento vira uma linha por mês, filial, cliente, setor, grupo, linha e vendedor — a devolução na mesma
/// combinação desconta, a quantidade da cortesia não conta, o pneu vira o grupo PNEU, e a filial de fora fica de fora;</item>
/// <item>a segunda rodada regrava a janela inteira; o orçamento que muda de situação fica na trilha;</item>
/// <item>o orçamento que some dentro da janela é excluído e o que volta é reativado;</item>
/// <item>as travas: faturamento que encolhe à metade e orçamento ilegível acima de 5% abortam tudo;</item>
/// <item>a simulação não grava, e o faturamento vazio não apaga nada.</item>
/// </list>
/// </summary>
public sealed class CargaDasPecasDoProtheusTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;
    private readonly long _clienteId;

    public CargaDasPecasDoProtheusTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
        (_clienteId, _) = SemearAOficina(db, _semente);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private Task<Resultado<RelatorioDasPecas>> Tentar(LeituraDasPecas? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null) =>
        CargaDePecas(DaCarga, leitura ?? LeituraDePecas(quando ?? Agora), quando ?? Agora, _semente.Operador).ExecutarAsync(simular, aceitarRemocao, CancellationToken.None);

    private async Task<RelatorioDasPecas> Rodar(LeituraDasPecas? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null)
    {
        var resultado = await Tentar(leitura, simular, aceitarRemocao, quando);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private async Task<(List<FaturamentoDePecasNoMes> Faturamento, List<OrcamentoDePecas> Orcamentos)> Gravado()
    {
        await using var db = Sistema();
        return (
            await db.FaturamentosDePecasNoMes.IgnoreQueryFilters().AsNoTracking().OrderBy(f => f.EmpresaId).ThenBy(f => f.Grupo).ToListAsync(),
            await db.OrcamentosDePecas.IgnoreQueryFilters().AsNoTracking().OrderBy(o => o.ChaveNaOrigem).ToListAsync());
    }

    [Fact]
    public async Task A_primeira_rodada_soma_o_faturamento_nas_regras_do_BI_e_grava_os_orcamentos()
    {
        var relatorio = await Rodar();

        relatorio.Valor(CargaDasPecasDoProtheus.RotuloDeCombinacoes).Should().Be(3);
        relatorio.Valor(CargaDasPecasDoProtheus.RotuloDeCombinacoesComCliente).Should().Be(2);
        relatorio.Valor(CargaDasPecasDoProtheus.RotuloSemFilial).Should().Be(1, "a filial 99 não é do CRM");

        var (faturamento, orcamentos) = await Gravado();
        var pecas = faturamento.Single(f => f.Grupo == "PECAS" && f.EmpresaId == _semente.RibeiraoPreto);
        (pecas.ClienteId, pecas.Setor, pecas.ValorLiquido, pecas.ValorDeDevolucoes, pecas.Quantidade, pecas.Itens, pecas.VendedorNome)
            .Should().Be(((long?)_clienteId, "BALCAO", 800m, -200m, 2m, 3, "VENDEDOR 000123"));
        var pneus = faturamento.Single(f => f.Grupo == "PNEU");
        (pneus.Setor, pneus.ValorLiquido, pneus.Quantidade).Should().Be(("OFICINA", 500m, 0m), "a cortesia fora da garantia não movimenta estoque");
        faturamento.Single(f => f.EmpresaId == _semente.Ituverava).ClienteId.Should().BeNull("o documento não é de cliente do CRM");

        orcamentos.Select(o => o.ChaveNaOrigem).Should().Equal("01|000777", "05|000778");
        (orcamentos[0].ClienteId, orcamentos[0].ValorTotal, orcamentos[0].ValorDeDesconto, orcamentos[0].Itens, orcamentos[0].EstaEmAberto)
            .Should().Be(((long?)_clienteId, 500m, 20m, 2, true));
        orcamentos[1].EstaEmAberto.Should().BeFalse();

        relatorio.Observacoes.Should().Contain(o => o.Contains("ano fiscal 2026") && o.Contains("R$ 1.600,00"));
        await using var db = Sistema();
        (await db.PontosDeSincronismo.AsNoTracking().Select(p => p.Fluxo).ToListAsync())
            .Should().Contain([FaturamentoDePecasNoMes.FluxoDaCarga, OrcamentoDePecas.FluxoDaCarga]);
    }

    [Fact]
    public async Task A_segunda_rodada_regrava_a_janela_e_a_situacao_do_orcamento_fica_na_trilha()
    {
        await Rodar();

        var outroValor = FaturamentoPadrao();
        outroValor[0] = outroValor[0] with { ValorLiquido = 1500m };
        var orcamentos = OrcamentosPadrao();
        orcamentos[0] = orcamentos[0] with { Situacao = "Encerrado" };
        orcamentos[1] = orcamentos[1] with { Situacao = "Encerrado" };
        var segunda = await Rodar(LeituraDePecas(Agora, outroValor, orcamentos), quando: Agora.AddHours(1));

        segunda.Valor(CargaDasPecasDoProtheus.RotuloDeAtualizados).Should().Be(1);
        var (faturamento, gravados) = await Gravado();
        faturamento.Should().HaveCount(3, "a janela é regravada inteira, sem duplicar");
        faturamento.Single(f => f.Grupo == "PECAS" && f.EmpresaId == _semente.RibeiraoPreto).ValorLiquido.Should().Be(1300m);
        gravados[0].EstaEmAberto.Should().BeFalse();

        await using var db = Sistema();
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(t => t.Entidade == nameof(OrcamentoDePecas)).ToListAsync();
        trilha.Should().ContainSingle(t => t.Campo == nameof(OrcamentoDePecas.Situacao));
        trilha.Should().OnlyContain(t => t.Origem == OrigemDaOperacao.Integracao);
        (await db.AlteracoesDeCampo.AsNoTracking().AnyAsync(t => t.Entidade == nameof(FaturamentoDePecasNoMes)))
            .Should().BeFalse("o faturamento é apuração, sem trilha");
    }

    [Fact]
    public async Task O_orcamento_que_some_na_janela_e_excluido_e_o_que_volta_e_reativado()
    {
        await Rodar();

        var semOAberto = OrcamentosPadrao().Where(o => o.Numero != "000777").ToList();
        (await Rodar(LeituraDePecas(Agora, null, semOAberto), quando: Agora.AddHours(1)))
            .Valor(CargaDasPecasDoProtheus.RotuloDeExcluidos).Should().Be(1);
        (await Gravado()).Orcamentos.Single(o => o.Numero == "000777").EstaExcluido.Should().BeTrue();

        (await Rodar(quando: Agora.AddHours(2))).Valor(CargaDasPecasDoProtheus.RotuloDeReativados).Should().Be(1);
        (await Gravado()).Orcamentos.Should().OnlyContain(o => !o.EstaExcluido);
    }

    [Fact]
    public async Task O_faturamento_que_encolhe_a_menos_da_metade_aborta_e_aceitar_remocao_passa()
    {
        static List<PecasFaturadasNaOrigem> Muitas(int quantas) =>
            [.. Enumerable.Range(1, quantas).Select(i => Peca(vendedor: i.ToString("D6")))];

        await Rodar(LeituraDePecas(Agora, Muitas(120), []));

        var abortada = await Tentar(LeituraDePecas(Agora, Muitas(40), []), quando: Agora.AddHours(1));
        abortada.EhSucesso.Should().BeFalse();
        abortada.Erro.Should().Contain("40 combinações contra 120").And.Contain("--aceitar-remocao");
        (await Gravado()).Faturamento.Should().HaveCount(120, "a rodada abortada não grava nada");

        await Rodar(LeituraDePecas(Agora, Muitas(40), []), aceitarRemocao: true, quando: Agora.AddHours(2));
        (await Gravado()).Faturamento.Should().HaveCount(40);
    }

    [Fact]
    public async Task Orcamentos_ilegiveis_acima_de_cinco_por_cento_abortam()
    {
        var ilegivel = OrcamentosPadrao();
        ilegivel.Add(ItemDeOrcamento(numero: "000779", situacao: null));

        var abortada = await Tentar(LeituraDePecas(Agora, null, ilegivel));

        abortada.EhSucesso.Should().BeFalse();
        abortada.Erro.Should().Contain("ilegíveis");
        (await Gravado()).Faturamento.Should().BeEmpty();
    }

    [Fact]
    public async Task A_simulacao_nao_grava_e_o_faturamento_vazio_nao_apaga_nada()
    {
        (await Rodar(simular: true)).Simulada.Should().BeTrue();
        (await Gravado()).Faturamento.Should().BeEmpty();

        await Rodar();
        var vazia = await Tentar(LeituraDePecas(Agora, [], OrcamentosPadrao()), quando: Agora.AddHours(1));
        vazia.EhSucesso.Should().BeFalse();
        vazia.Erro.Should().Contain("vazio");
        (await Gravado()).Faturamento.Should().HaveCount(3);
    }
}
