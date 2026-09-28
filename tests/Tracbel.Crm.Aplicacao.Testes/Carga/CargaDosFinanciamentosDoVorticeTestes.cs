using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O FINANCIAMENTO DAS VENDAS DO VÓRTICE (issue 262, 28/09/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>um financiamento por processo; o cancelado, o valor absurdo e a data impossível não entram;</item>
/// <item>a cidade de SP casa com o catálogo pelo nome e o par fica gravado; fora de SP entra sem município;</item>
/// <item>a data do pedido, e a do preenchimento quando a do pedido é digitação;</item>
/// <item>o processo que é cancelado depois sai sem ser apagado, e volta na mesma linha;</item>
/// <item>a trava de remoção aborta, e --aceitar-queda passa; a simulação não grava nada.</item>
/// </list>
/// </summary>
public sealed class CargaDosFinanciamentosDoVorticeTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;
    private readonly int _franca;

    public CargaDosFinanciamentosDoVorticeTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);

        var franca = Municipio.Criar("Franca", "SP", 3516200);
        db.Municipios.AddRange(franca, Municipio.Criar("São Joaquim da Barra", "SP", 3549409));
        db.SaveChanges();
        _franca = franca.Id;
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private static FinanciamentoNoVortice Resposta(
        long processo, string formulario = "IV_Q_VENDA_EQUIPAMENTO", decimal valor = 300_000m, string? fase = "Recebimento",
        string? status = null, DateTime? pedido = null, DateTime? preenchido = null, long? seqCidade = 11, string? cidade = "FRANCA",
        string? uf = "SP", string? linha = "MODER FROTA", string? instituicao = "BANCO JOHN DEERE") =>
        new(processo, formulario, processo * 10, preenchido ?? new DateTime(2026, 3, 5), pedido ?? new DateTime(2026, 3, 1), valor,
            instituicao, linha, seqCidade, cidade, uf, fase, status);

    private static List<FinanciamentoNoVortice> Leitura() =>
    [
        Resposta(1),
        Resposta(1, formulario: "IV_Q_GESTAO_CREDITO", valor: 999m),
        Resposta(2, linha: "RECURSO PRÓPRIO", instituicao: "NÃO SE APLICA", cidade: "SÃO JOAQUIM DA BARRA", seqCidade: 12),
        Resposta(3, uf: "MG", cidade: "UBERABA", seqCidade: 13),
        Resposta(4, fase: "Cancelamento", status: "DESISTIU DA COMPRA"),
        Resposta(5, valor: 3_455_000_000m),
        Resposta(6, pedido: new DateTime(2106, 1, 1), preenchido: new DateTime(2026, 4, 2)),
        Resposta(7, cidade: "CIDADE QUE NAO EXISTE", seqCidade: 14)
    ];

    private Task<Resultado<RelatorioDosFinanciamentos>> Tentar(List<FinanciamentoNoVortice> leitura, bool simular = false, bool aceitarQueda = false) =>
        new CargaDosFinanciamentosDoVortice(
                DaCarga, _ => Task.FromResult(Resultado<IReadOnlyList<FinanciamentoNoVortice>>.Ok(leitura)), _semente.Operador, () => Agora, _ => { })
            .ExecutarAsync(simular, aceitarQueda, CancellationToken.None);

    private async Task<RelatorioDosFinanciamentos> Sincronizar(List<FinanciamentoNoVortice> leitura, bool simular = false, bool aceitarQueda = false)
    {
        var resultado = await Tentar(leitura, simular, aceitarQueda);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [Fact]
    public async Task A_primeira_rodada_grava_um_por_processo_e_deixa_de_fora_o_que_nao_e_financiamento()
    {
        // O VALOR ABSURDO É 1 DE 7 PROCESSOS — acima da trava de 5% —; a rodada inteira precisa de mais processos válidos.
        var leitura = Leitura().Concat(Enumerable.Range(100, 20).Select(p => Resposta(p))).ToList();
        var relatorio = await Sincronizar(leitura);

        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloDeProcessos).Should().Be(27);
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloDeCancelados).Should().Be(1);
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloDeValorForaDoRazoavel).Should().Be(1);
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloDataDoPreenchimento).Should().Be(1);
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloForaDeSaoPaulo).Should().Be(1);
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloSemPar).Should().Be(1);
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloDeNovas).Should().Be(25);

        await using var db = Sistema();
        var gravados = await db.FinanciamentosDaVenda.ToDictionaryAsync(f => f.ProcessoNoVortice);
        gravados.Should().HaveCount(25);
        gravados.Keys.Should().NotContain([4L, 5L]);

        gravados[1].Formulario.Should().Be("IV_Q_VENDA_EQUIPAMENTO", "o formulário mais novo vale");
        gravados[1].ValorFinanciado.Should().Be(300_000m);
        gravados[1].MunicipioId.Should().Be(_franca);
        gravados[1].ContaNoCreditoRural.Should().BeTrue();

        gravados[2].ContaNoCreditoRural.Should().BeFalse("recurso próprio não é crédito rural");
        gravados[2].MunicipioId.Should().NotBeNull("São Joaquim da Barra casa sem acento");
        gravados[3].MunicipioId.Should().BeNull("Uberaba é MG");
        gravados[7].MunicipioId.Should().BeNull();

        gravados[6].PedidoEm.Should().Be(new DateOnly(2026, 4, 2), "2106 é digitação: vale a data do preenchimento");
        gravados[6].DataDoPreenchimento.Should().BeTrue();

        (await db.CorrespondenciasDeMunicipios.CountAsync(c => c.Fonte == CargaDosFinanciamentosDoVortice.FluxoDasCidades))
            .Should().Be(2, "Franca e São Joaquim da Barra ficam casadas para as próximas rodadas");
    }

    [Fact]
    public async Task A_segunda_rodada_igual_nao_grava_nada_e_o_cancelado_depois_sai_sem_apagar()
    {
        var leitura = Enumerable.Range(1, 60).Select(p => Resposta(p)).ToList();
        await Sincronizar(leitura);

        var igual = await Sincronizar(leitura);
        igual.Valor(CargaDosFinanciamentosDoVortice.RotuloDeIguais).Should().Be(60);
        igual.Valor(CargaDosFinanciamentosDoVortice.RotuloDeNovas).Should().Be(0);

        leitura[0] = Resposta(1, fase: "Cancelamento");
        var cancelado = await Sincronizar(leitura);
        cancelado.Valor(CargaDosFinanciamentosDoVortice.RotuloDeExcluidas).Should().Be(1);

        await using (var db = Sistema())
            (await db.FinanciamentosDaVenda.SingleAsync(f => f.ProcessoNoVortice == 1)).ExcluidoEm.Should().NotBeNull();

        leitura[0] = Resposta(1);
        var voltou = await Sincronizar(leitura);
        voltou.Valor(CargaDosFinanciamentosDoVortice.RotuloDeReativadas).Should().Be(1);

        await using (var db = Sistema())
        {
            (await db.FinanciamentosDaVenda.CountAsync()).Should().Be(60, "a mesma linha volta");
            (await db.FinanciamentosDaVenda.SingleAsync(f => f.ProcessoNoVortice == 1)).ExcluidoEm.Should().BeNull();
        }
    }

    [Fact]
    public async Task A_leitura_que_encolhe_aborta_e_aceitar_queda_passa()
    {
        await Sincronizar(Enumerable.Range(1, 60).Select(p => Resposta(p)).ToList());
        var metade = Enumerable.Range(1, 30).Select(p => Resposta(p)).ToList();

        var abortada = await Tentar(metade);
        abortada.EhSucesso.Should().BeFalse();
        abortada.Erro.Should().Contain("ABORTADA");

        await using (var db = Sistema())
            (await db.FinanciamentosDaVenda.CountAsync(f => f.ExcluidoEm == null)).Should().Be(60);

        var aceita = await Sincronizar(metade, aceitarQueda: true);
        aceita.Observacoes.Should().ContainSingle(o => o.Contains("--aceitar-queda"));
    }

    [Fact]
    public async Task A_simulacao_nao_grava_nada()
    {
        var relatorio = await Sincronizar(Enumerable.Range(1, 10).Select(p => Resposta(p)).ToList(), simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDosFinanciamentosDoVortice.RotuloDeNovas).Should().Be(10);

        await using var db = Sistema();
        (await db.FinanciamentosDaVenda.CountAsync()).Should().Be(0);
        (await db.CorrespondenciasDeMunicipios.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task A_leitura_vazia_nao_exclui_nada()
    {
        var resultado = await Tentar([]);
        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("vazios");
    }
}
