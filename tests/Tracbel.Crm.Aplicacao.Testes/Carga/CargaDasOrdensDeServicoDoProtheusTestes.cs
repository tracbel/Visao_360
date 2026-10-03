using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasOrdensDeServico;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// AS ORDENS DE SERVIÇO DO PROTHEUS (02/10/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada grava uma OS por filial e número, casa o cliente pelo documento e a máquina pelo chassi
/// normalizado, soma peças e serviços na régua do BI e deixa de fora (contada) a OS de filial que o CRM não tem;</item>
/// <item>a segunda leitura igual não grava nada; o fechamento fica na trilha como integração;</item>
/// <item>a OS que some dentro da janela é excluída sem apagar; a fechada que envelheceu e saiu da leitura fica;</item>
/// <item>as travas: OS ilegível acima de 5% e remoção acima de 20% abortam tudo; --aceitar-remocao passa;</item>
/// <item>a simulação não grava nada, e a leitura vazia não é tratada como oficina sem serviço.</item>
/// </list>
/// </summary>
public sealed class CargaDasOrdensDeServicoDoProtheusTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;
    private readonly long _clienteId;
    private readonly long _maquinaId;

    public CargaDasOrdensDeServicoDoProtheusTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
        (_clienteId, _maquinaId) = SemearAOficina(db, _semente);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    // OS TESTES DA SINCRONIA EM SI PEDEM A COMPLETA: várias rodadas na mesma sexta-feira seriam curtas pela agenda, e é a
    // completa que eles descrevem. O alcance — curta ou completa — tem os testes dele, no fim do arquivo.
    private Task<Resultado<RelatorioDasOrdensDeServico>> Tentar(
        LeituraDasOrdensDeServico? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null) =>
        Sincronia(DaCarga, leitura ?? Leitura(quando ?? Agora), quando ?? Agora, _semente.Operador)
            .ExecutarAsync(simular, aceitarRemocao, completaPedida: true, CancellationToken.None);

    private async Task<RelatorioDasOrdensDeServico> Sincronizar(
        LeituraDasOrdensDeServico? leitura = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null)
    {
        var resultado = await Tentar(leitura, simular, aceitarRemocao, quando);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private async Task<List<OrdemDeServico>> Gravadas()
    {
        await using var db = Sistema();
        return await db.OrdensDeServico.IgnoreQueryFilters().AsNoTracking().OrderBy(o => o.ChaveNaOrigem).ToListAsync();
    }

    private static List<ItemDaOrdemNaOrigem> Muitas(int quantas) =>
        [.. Enumerable.Range(1, quantas).Select(i => Item("010101", i.ToString("D8"), "F", new DateOnly(2026, 6, 1), fechada: new DateOnly(2026, 6, 2)))];

    [Fact]
    public async Task A_primeira_rodada_grava_uma_OS_por_filial_e_numero_casa_cliente_e_maquina_e_soma_na_regua_do_BI()
    {
        var relatorio = await Sincronizar();

        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeOrdensLidas).Should().Be(3);
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloSemFilial).Should().Be(1, "019999 não é filial do CRM");
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloComCliente).Should().Be(1);
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloComMaquina).Should().Be(1);
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeNovas).Should().Be(2);

        var ordens = await Gravadas();
        ordens.Select(o => o.ChaveNaOrigem).Should().Equal("010101|00000001", "010105|00000002");

        var a = ordens[0];
        (a.EmpresaId, a.ClienteId, a.EquipamentoId, a.Situacao, a.AbertaEm)
            .Should().Be((_semente.RibeiraoPreto, (long?)_clienteId, (long?)_maquinaId, SituacaoDaOrdemDeServico.Aberta, new DateOnly(2026, 9, 1)));
        a.ValorDePecas.Should().Be(270m, "2 + 1 unidades a R$ 100, com o desconto de R$ 30 da peça contado uma vez");
        a.ValorDeServicos.Should().Be(285m, "o serviço repetido conta uma vez: 2 h × 150 − (30 ÷ 4) × 2");
        (a.ItensDePeca, a.ItensDeServico).Should().Be((2, 1));
        a.Chassi.Should().Be("1py6155mcss000001", "o chassi fica como o Protheus escreve; o casamento é pelo normalizado");

        var b = ordens[1];
        (b.EmpresaId, b.ClienteId, b.EquipamentoId, b.Situacao, b.FechadaEm)
            .Should().Be((_semente.Ituverava, (long?)null, (long?)null, SituacaoDaOrdemDeServico.Fechada, (DateOnly?)new DateOnly(2026, 5, 20)));
        b.ValorDeServicos.Should().Be(0m, "FDLI não é contabilizado no faturamento");

        await using var db = Sistema();
        (await db.PontosDeSincronismo.AsNoTracking().Select(p => p.Fluxo).ToListAsync()).Should().Contain(OrdemDeServico.FluxoDaCarga);
        relatorio.Observacoes.Should().Contain(o => o.Contains("OS abertas e liberadas: 1") && o.Contains("R$ 270,00") && o.Contains("R$ 285,00"));
    }

    [Fact]
    public async Task A_segunda_leitura_igual_nao_grava_nada_e_o_fechamento_fica_na_trilha_como_integracao()
    {
        await Sincronizar();
        (await Sincronizar(quando: Agora.AddHours(1))).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeIguais).Should().Be(2);

        var fechada = ItensPadrao();
        fechada[0] = fechada[0] with { StatusDaCapa = "F", FechadaEm = new DateOnly(2026, 10, 1) };
        fechada[1] = fechada[1] with { StatusDaCapa = "F", FechadaEm = new DateOnly(2026, 10, 1) };
        (await Sincronizar(Leitura(Agora, fechada), quando: Agora.AddHours(2))).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeAtualizadas).Should().Be(1);

        await using var db = Sistema();
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(t => t.Entidade == nameof(OrdemDeServico)).ToListAsync();
        trilha.Select(t => t.Campo).Should().BeEquivalentTo(nameof(OrdemDeServico.Situacao), nameof(OrdemDeServico.FechadaEm));
        trilha.Should().OnlyContain(t => t.Origem == OrigemDaOperacao.Integracao && t.SistemaId != null);
    }

    [Fact]
    public async Task A_OS_que_some_dentro_da_janela_e_excluida_e_a_fechada_que_envelheceu_fica()
    {
        var comAntiga = ItensPadrao();
        comAntiga.Add(Item("010101", "00000005", "F", new DateOnly(2023, 11, 1), fechada: new DateOnly(2023, 11, 3)));
        await Sincronizar(Leitura(Agora, comAntiga));

        // UM ANO DEPOIS a janela começa em 01/10/2024: a OS de 2023 sai da leitura por idade; a B (de 2026) some da origem.
        var umAnoDepois = Agora.AddYears(1);
        var semAsDuas = ItensPadrao().Where(i => i.NumeroOs != "00000002").ToList();
        var relatorio = await Sincronizar(Leitura(umAnoDepois, semAsDuas), quando: umAnoDepois);

        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeExcluidas).Should().Be(1);
        var ordens = await Gravadas();
        ordens.Single(o => o.Numero == "00000002").EstaExcluido.Should().BeTrue();
        ordens.Single(o => o.Numero == "00000005").EstaExcluido.Should().BeFalse("é história da máquina, e não sumiu da origem — saiu da janela");

        // E A QUE VOLTA É REATIVADA NA MESMA LINHA.
        var id = ordens.Single(o => o.Numero == "00000002").Id;
        (await Sincronizar(Leitura(umAnoDepois), quando: umAnoDepois.AddHours(1)))
            .Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeReativadas).Should().Be(1);
        (await Gravadas()).Single(o => o.Numero == "00000002").Should().Match<OrdemDeServico>(o => o.Id == id && !o.EstaExcluido);
    }

    [Fact]
    public async Task A_remocao_acima_de_vinte_por_cento_aborta_e_aceitar_remocao_passa()
    {
        await Sincronizar(Leitura(Agora, Muitas(60), []));

        var abortada = await Tentar(Leitura(Agora, Muitas(40), []), quando: Agora.AddHours(1));
        abortada.EhSucesso.Should().BeFalse();
        abortada.Erro.Should().Contain("20 de 60").And.Contain("--aceitar-remocao");
        (await Gravadas()).Should().OnlyContain(o => !o.EstaExcluido, "a rodada abortada não grava nada");

        (await Sincronizar(Leitura(Agora, Muitas(40), []), aceitarRemocao: true, quando: Agora.AddHours(2)))
            .Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeExcluidas).Should().Be(20);
    }

    [Fact]
    public async Task OS_ilegiveis_acima_de_cinco_por_cento_abortam_e_abaixo_sao_contadas()
    {
        var muitasIlegiveis = new List<ItemDaOrdemNaOrigem>
        {
            Item("010101", "00000001", "A", new DateOnly(2026, 9, 1)),
            Item("010101", "00000002", "X", new DateOnly(2026, 9, 1))
        };
        var abortada = await Tentar(Leitura(Agora, muitasIlegiveis, []));
        abortada.EhSucesso.Should().BeFalse();
        abortada.Erro.Should().Contain("ilegíveis");
        (await Gravadas()).Should().BeEmpty();

        var poucas = Muitas(30);
        poucas.Add(Item("010101", "99999999", "X", new DateOnly(2026, 6, 1)));
        (await Sincronizar(Leitura(Agora, poucas, []))).Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeRecusadas).Should().Be(1);
        (await Gravadas()).Should().HaveCount(30);
    }

    [Fact]
    public async Task A_simulacao_nao_grava_nada()
    {
        var relatorio = await Sincronizar(simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeNovas).Should().Be(2);
        (await Gravadas()).Should().BeEmpty();
    }

    [Fact]
    public async Task A_leitura_vazia_nao_e_uma_oficina_sem_servico()
    {
        await Sincronizar();

        var vazia = await Tentar(Leitura(Agora, [], []), quando: Agora.AddHours(1));

        vazia.EhSucesso.Should().BeFalse();
        vazia.Erro.Should().Contain("vazias");
        (await Gravadas()).Should().OnlyContain(o => !o.EstaExcluido);
    }

    // =============================================================================================
    // O alcance: curta nos dias comuns, completa no domingo (documento 54, passo 6)
    // =============================================================================================

    private static readonly DateTime Sabado = new(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Domingo = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);

    private async Task<(RelatorioDasOrdensDeServico Relatorio, (DateOnly Desde, DateOnly? Mudancas) Pedido)> Rodar(
        LeituraDasOrdensDeServico leitura, DateTime quando)
    {
        var pedidos = new List<(DateOnly Desde, DateOnly? Mudancas)>();
        var resultado = await Sincronia(DaCarga, leitura, quando, _semente.Operador, pedidos)
            .ExecutarAsync(simular: false, aceitarRemocao: false, completaPedida: false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return (resultado.Valor, pedidos.Single());
    }

    private async Task<DateTime?> UltimaCompleta()
    {
        await using var db = Sistema();
        return await RodadaCompleta.LerAsync(db, CargaDasOrdensDeServicoDoProtheus.FluxoDaLeituraCompleta, CancellationToken.None);
    }

    [Fact]
    public async Task Sem_completa_registrada_a_sincronia_e_completa_e_a_seguinte_em_dia_comum_e_curta_com_as_mudancas()
    {
        var primeira = await Rodar(Leitura(Agora), Agora);   // sexta, 02/10
        var segunda = await Rodar(Leitura(Agora), Sabado);   // sábado, 03/10

        primeira.Pedido.Should().Be((new DateOnly(2023, 10, 1), (DateOnly?)null));
        segunda.Pedido.Should().Be((new DateOnly(2026, 9, 1), (DateOnly?)new DateOnly(2026, 9, 1)),
            "a curta pede as abertas desde o início curto e as fechadas ou canceladas desde ele");
        primeira.Relatorio.Alcance.Should().StartWith("leitura COMPLETA");
        segunda.Relatorio.Alcance.Should().StartWith("leitura curta");
        (await UltimaCompleta()).Should().Be(Agora, "só a completa grava o ponto dela, com o instante da leitura");
    }

    [Fact]
    public async Task A_leitura_curta_nao_exclui_a_OS_antiga_que_nao_veio_e_exclui_a_recente_que_sumiu()
    {
        var comRecente = ItensPadrao();
        comRecente.Add(Item("010101", "00000004", "F", new DateOnly(2026, 9, 15), fechada: new DateOnly(2026, 9, 16)));
        await Rodar(Leitura(Agora, comRecente), Agora);

        // NO SÁBADO a origem manda só a A, aberta: a B — antiga e fechada — não entra na leitura curta; a 4, aberta em
        // setembro, sumiu da origem.
        var soA = ItensPadrao().Where(i => i.NumeroOs == "00000001").ToList();
        var (relatorio, _) = await Rodar(Leitura(Agora, soA), Sabado);

        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeExcluidas).Should().Be(1);
        var ordens = await Gravadas();
        ordens.Single(o => o.Numero == "00000002").EstaExcluido.Should().BeFalse("a leitura curta não a lê — quem decide é a completa");
        ordens.Single(o => o.Numero == "00000004").EstaExcluido.Should().BeTrue("aberta dentro da janela curta e ausente da origem");
    }

    [Fact]
    public async Task A_completa_de_domingo_conta_as_OS_corrigidas_fora_do_alcance_curto()
    {
        await Rodar(Leitura(Agora), Agora);

        // NO DOMINGO a origem corrigiu a B (antiga e fechada: o desconto da peça) e a A (aberta: o alcance curto já a veria).
        var corrigidas = ItensPadrao();
        corrigidas[2] = corrigidas[2] with { ValorDoDesconto = 5m };
        corrigidas[0] = corrigidas[0] with { Quantidade = 3m };
        var (relatorio, pedido) = await Rodar(Leitura(Domingo, corrigidas), Domingo);

        pedido.Mudancas.Should().BeNull("a completa lê como sempre leu");
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloDeAtualizadas).Should().Be(2);
        relatorio.Valor(CargaDasOrdensDeServicoDoProtheus.RotuloCorrigidasForaDoCurto).Should().Be(1, "só a B — a A, aberta, a leitura curta já veria");
        relatorio.Alcance.Should().Contain("1 OS corrigida");
    }

    [Fact]
    public async Task A_completa_abortada_nao_grava_o_ponto_da_completa()
    {
        var ilegiveis = new List<ItemDaOrdemNaOrigem>
        {
            Item("010101", "00000001", "A", new DateOnly(2026, 9, 1)),
            Item("010101", "00000002", "X", new DateOnly(2026, 9, 1))
        };

        var abortada = await Sincronia(DaCarga, Leitura(Agora, ilegiveis, []), Agora, _semente.Operador)
            .ExecutarAsync(simular: false, aceitarRemocao: false, completaPedida: false, CancellationToken.None);

        abortada.EhSucesso.Should().BeFalse();
        (await UltimaCompleta()).Should().BeNull("a próxima rodada tem de ser completa de novo");
    }
}
