using System.Globalization;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoFunilDoVortice;
using ProcessoDoCrm = Tracbel.Crm.Dominio.Processo.Processo;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A ONDA 2 DOS PROCESSOS DO VÓRTICE (documento 52 §12; decisões do Ricardo de 27/09/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>só o casado entra — o prospect, a filial fora do CRM e o processo de antes da janela ficam de fora — e nenhum
/// cadastro nasce;</item>
/// <item>P1: só o Resumo entra, como título; P2: sem conta, o dono do processo; P8: o perdido leva o motivo da venda
/// perdida principal; P10: a pendente de processo encerrado não entra;</item>
/// <item>duas rodadas sem mudança não alteram nada; a mudança na origem acompanha; o que sai é excluído, e volta;</item>
/// <item>o duplo ponteiro e a linha do funil são ligados; o último contato do vínculo não é tocado;</item>
/// <item>leitura vazia e queda de mais de 5% abortam sem gravar; a simulação não grava.</item>
/// </list>
/// <para>SQLite em memória com o modelo de verdade. O funil do cenário roda antes, como na rotina: é ele que grava as linhas
/// do funil e as vendas perdidas que a onda 2 liga.</para>
/// </summary>
public sealed class CargaDasOportunidadesDoVorticeTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Processos casados de enchimento: tirar um é 2,4%, e não a metade do universo.</summary>
    private const int Enchimento = 40;

    private const long SeqDaMaria = 501;
    private const long SeqSemConta = 999;

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDoFunil _semente;
    private readonly long _vinculoId;

    public CargaDasOportunidadesDoVorticeTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db);

        // A CONTA DA MARIA PELO SeqUsuario — o de-para que a sincronia das carteiras grava para os donos.
        var sistema = db.Sistemas.Single(s => s.Codigo == "VORTICE");
        db.ChavesExternas.Add(ChaveExterna.Criar(sistema.Id, "Usuario", _semente.Maria, SeqDaMaria.ToString(CultureInfo.InvariantCulture)));

        // UM VÍNCULO SEM ÚLTIMO CONTATO: a onda 2 não o escreve — ele é da CARTEIRAS_VORTICE.
        var vinculo = ClienteCarteira.Criar(_semente.ClienteId, _semente.CarteiraId, ClasseDeCliente.D, _semente.Operador);
        db.ClienteCarteiras.Add(vinculo);
        db.SaveChanges();
        _vinculoId = vinculo.Id;

        // O FUNIL PRIMEIRO, como na rotina: as linhas do funil e as vendas perdidas.
        Rotina(DaCarga, _semente, Funil(), Respostas(), Agora).ExecutarAsync(false, false, CancellationToken.None).GetAwaiter().GetResult()
            .EhSucesso.Should().BeTrue();
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(
        _semente.Operador, _semente.RibeiraoPreto, new HashSet<int> { _semente.RibeiraoPreto, _semente.Barretos }));

    // =============================================================================================
    // O cenário
    // =============================================================================================

    private static DocumentoDoVortice Cpf(string cpf) =>
        DocumentoDoVortice.Recompor(decimal.Parse(cpf[..9], CultureInfo.InvariantCulture), decimal.Parse(cpf[9..], CultureInfo.InvariantCulture), "F");

    private static readonly DocumentoDoVortice SemDocumento = DocumentoDoVortice.Recompor(null, null, "F");

    private static ProcessoDaOportunidadeNoVortice P(
        long numero, int nn, DateTime incluido, DocumentoDoVortice? documento = null, string? status = "EM ABERTO", string? resumo = null,
        string? login = null, int? carteira = null, DateTime? realizado = null) =>
        new(numero, 41, nn, numero + 100_000, documento ?? Cpf(CpfDoCliente), carteira, login, incluido, incluido, status, realizado,
            "NEGOCIACAO", 3, incluido.AddDays(10), incluido.AddDays(10), resumo, 500_000m, 1m, new DateOnly(2026, 12, 1), null);

    private static TarefaDaOportunidadeNoVortice T(
        long agenda, long processo, long? usuario = SeqDaMaria, DateTime? agendada = null, string realizada = "N", DateTime? realizadaEm = null,
        int? resultado = null, long? conclusao = null, long? origem = null, long? quemConcluiu = null) =>
        new(agenda, processo, usuario, 50, agendada ?? Em(2026, 10, 1), null, 2m, realizada, realizadaEm, resultado, conclusao, origem,
            quemConcluiu, "M", Em(2026, 9, 1));

    private static InteracaoDaOportunidadeNoVortice I(
        long historico, long processo, long? usuario = SeqDaMaria, int? resultado = null, long? agenda = null, bool deSistema = false,
        decimal? latitude = null, decimal? longitude = null) =>
        new(historico, processo, usuario, 50, resultado, agenda, "A", deSistema, Em(2026, 9, 10), 0m, latitude, longitude);

    private static List<ProcessoDaOportunidadeNoVortice> Processos()
    {
        var processos = new List<ProcessoDaOportunidadeNoVortice>
        {
            P(1001, 1, Em(2024, 1, 5), resumo: "  Trator 6125J para a safra  ", login: "MARIA.SOUZA", carteira: 18),
            P(1002, 1, Em(2024, 2, 1), documento: SemDocumento),
            P(2000, 1, Em(2025, 1, 1), status: "VENDA PERDIDA", realizado: Em(2025, 3, 1)),
            P(1008, 1, Em(2025, 2, 1), status: "CANCELADO", realizado: Em(2025, 4, 1)),
            P(1005, 6, Em(2024, 1, 5)),
            P(1007, 1, Em(2022, 5, 1))
        };

        for (var i = 0; i < Enchimento; i++) processos.Add(P(3000 + i, 1, Em(2025, 5, 1)));
        return processos;
    }

    private static List<TarefaDaOportunidadeNoVortice> Tarefas()
    {
        var tarefas = new List<TarefaDaOportunidadeNoVortice>
        {
            T(7001, 1001),
            T(7002, 1001, realizada: "S", realizadaEm: Em(2026, 9, 12), resultado: 900, conclusao: 8002, origem: 8001, quemConcluiu: SeqSemConta),
            T(7003, 1008),
            T(7004, 1002),
            T(7005, 1001, usuario: SeqSemConta)
        };

        for (var i = 0; i < Enchimento; i++) tarefas.Add(T(7100 + i, 3000 + i));
        return tarefas;
    }

    private static List<InteracaoDaOportunidadeNoVortice> Interacoes() =>
    [
        I(8001, 1001),
        I(8002, 1001, usuario: SeqSemConta, resultado: 900, agenda: 7002),
        I(8003, 1002),
        I(8004, 1001, usuario: null, deSistema: true, latitude: 0m, longitude: 0m)
    ];

    private static LeituraDasOportunidadesDoVortice Leitura(
        List<ProcessoDaOportunidadeNoVortice>? processos = null, List<TarefaDaOportunidadeNoVortice>? tarefas = null,
        List<InteracaoDaOportunidadeNoVortice>? interacoes = null) =>
        new(processos ?? Processos(), tarefas ?? Tarefas(), interacoes ?? Interacoes(),
            [new(31, "Prospecção", null, true), new(41, "Venda de máquinas", null, true), new(50, "Pós-venda", null, true)],
            [new(50, "Ligar para o cliente", null, true, 2m)],
            [new(900, 50, "Proposta enviada", null)],
            [new(900, true, null)],
            [new(SeqDaMaria, "maria.souza"), new(SeqSemConta, "fulano.sem.conta")]);

    private async Task<Resultado<RelatorioDasOportunidadesDoVortice>> Rodar(
        LeituraDasOportunidadesDoVortice? leitura = null, bool simular = false, bool aceitarQueda = false) =>
        await new CargaDasOportunidadesDoVortice(
                DaCarga, _ => Task.FromResult(Resultado<LeituraDasOportunidadesDoVortice>.Ok(leitura ?? Leitura())),
                _semente.DeParaDeFiliais, _semente.Operador, () => Agora, _ => { }, tamanhoDoBloco: 7)
            .ExecutarAsync(simular, aceitarQueda, CancellationToken.None);

    private async Task<RelatorioDasOportunidadesDoVortice> Sincronizar(LeituraDasOportunidadesDoVortice? leitura = null, bool aceitarQueda = false)
    {
        var resultado = await Rodar(leitura, aceitarQueda: aceitarQueda);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private static ProcessoDoCrm Processo(CrmDbContext db, long numero) =>
        db.Set<ProcessoDoCrm>().AsNoTracking().Single(p => p.Numero == numero);

    private static Tarefa? TarefaDa(CrmDbContext db, long seqAgenda)
    {
        var id = db.ChavesExternas.AsNoTracking()
            .Where(c => c.Entidade == nameof(Tarefa) && c.ChaveOrigem == seqAgenda.ToString(CultureInfo.InvariantCulture))
            .Select(c => (long?)c.RegistroId)
            .SingleOrDefault();
        return id is null ? null : db.Tarefas.AsNoTracking().Single(t => t.Id == id);
    }

    private static Interacao InteracaoDa(CrmDbContext db, long seqHistorico)
    {
        var id = db.ChavesExternas.AsNoTracking()
            .Where(c => c.Entidade == nameof(Interacao) && c.ChaveOrigem == seqHistorico.ToString(CultureInfo.InvariantCulture))
            .Select(c => c.RegistroId)
            .Single();
        return db.Interacoes.AsNoTracking().Single(i => i.Id == id);
    }

    // =============================================================================================
    // A primeira rodada
    // =============================================================================================

    [Fact]
    public async Task So_o_casado_entra_com_o_resumo_como_titulo_e_nenhum_cadastro_nasce()
    {
        var relatorio = await Sincronizar();
        relatorio.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosNoUniverso).Should().Be(3 + Enchimento, "1001, 2000, 1008 e o enchimento");

        await using var db = Sistema();
        var numeros = await db.Set<ProcessoDoCrm>().AsNoTracking().Select(p => p.Numero).ToListAsync();
        numeros.Should().NotContain([1002, 1005, 1007], "prospect, filial fora do CRM e processo de antes da janela ficam de fora");

        var casado = Processo(db, 1001);
        casado.Should().BeEquivalentTo(new
        {
            Titulo = "Trator 6125J para a safra",
            ClienteId = (long?)_semente.ClienteId,
            CarteiraId = (long?)_semente.CarteiraId,
            ProprietarioId = _semente.Maria,
            EmpresaId = _semente.RibeiraoPreto,
            Situacao = SituacaoDoProcesso.Aberto,
            Descricao = (string?)null
        }, o => o.ExcludingMissingMembers(), "P1: só o Resumo entra, aparado; a descrição não");
        casado.CriadoEm.Should().Be(Em(2024, 1, 5), "a abertura na origem é o nascimento aqui");

        // SEM RESUMO, O TÍTULO É COMPOSTO do tipo e do número.
        Processo(db, 3000).Titulo.Should().Be("Venda de máquinas · nº 3000");

        (await db.Clientes.CountAsync()).Should().Be(1);
        (await db.Carteiras.CountAsync()).Should().Be(1);
        (await db.Usuarios.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task O_perdido_leva_o_motivo_da_venda_perdida_principal_e_o_cancelado_nao_tem_motivo()
    {
        await Sincronizar();

        await using var db = Sistema();
        // O PROCESSO 2000 TEM DUAS PRINCIPAIS (dois formulários diferentes): vale a registrada por último.
        var principais = await db.VendasPerdidas.AsNoTracking()
            .Where(v => v.NumeroDoProcessoNaOrigem == 2000 && v.Papel == PapelDaVendaPerdida.Principal)
            .ToListAsync();
        principais.Should().HaveCountGreaterThan(1);
        var maisRecente = principais.OrderByDescending(v => v.RegistradaEm).First();

        var perdido = Processo(db, 2000);
        perdido.Situacao.Should().Be(SituacaoDoProcesso.Perdido);
        perdido.MotivoDePerdaId.Should().Be(maisRecente.MotivoDePerdaId, "P8: o motivo do formulário principal mais recente do mesmo processo");
        perdido.ConcorrenteId.Should().Be(maisRecente.ConcorrenteId);
        perdido.ConcluidoEm.Should().Be(Em(2025, 3, 1));

        var cancelado = Processo(db, 1008);
        cancelado.Situacao.Should().Be(SituacaoDoProcesso.Cancelado);
        cancelado.MotivoDePerdaId.Should().BeNull();
    }

    [Fact]
    public async Task A_tarefa_sem_conta_vai_ao_dono_e_a_pendente_de_processo_encerrado_nao_entra()
    {
        await Sincronizar();

        await using var db = Sistema();
        var pelaChave = TarefaDa(db, 7001)!;
        pelaChave.ResponsavelId.Should().Be(_semente.Maria, "a conta pelo SeqUsuario");
        pelaChave.Assunto.Should().Be("Ligar para o cliente", "P1: o assunto é o nome da ação, e não o texto livre da agenda");
        pelaChave.Situacao.Should().Be(SituacaoDaTarefa.Pendente);

        TarefaDa(db, 7005)!.ResponsavelId.Should().Be(_semente.Maria, "P2: sem conta, o dono do processo — a Maria, pelo login");

        var concluida = TarefaDa(db, 7002)!;
        concluida.Situacao.Should().Be(SituacaoDaTarefa.Concluida);
        concluida.ConcluidaPorId.Should().Be(_semente.Maria, "P2: quem concluiu não tem conta");
        concluida.ResultadoId.Should().NotBeNull();

        TarefaDa(db, 7003).Should().BeNull("P10: pendente de processo cancelado não entra");
        TarefaDa(db, 7004).Should().BeNull("tarefa de prospect não entra");
    }

    [Fact]
    public async Task As_interacoes_entram_o_duplo_ponteiro_e_o_funil_sao_ligados_e_o_ultimo_contato_nao_e_tocado()
    {
        await Sincronizar();

        await using var db = Sistema();
        var conclusao = InteracaoDa(db, 8002);
        conclusao.RegistradoPorId.Should().Be(_semente.Maria, "P2: o autor sem conta vira o dono do processo");
        conclusao.Assunto.Should().Be("Ligar para o cliente");
        conclusao.Detalhe.Should().BeNull("P1: o detalhe do histórico não entra");

        var doSistema = InteracaoDa(db, 8004);
        doSistema.Natureza.Should().Be(NaturezaDaInteracao.Sistema);
        doSistema.Latitude.Should().BeNull("zero-zero é 'não registrado'");

        var tarefa = TarefaDa(db, 7002)!;
        tarefa.InteracaoOrigemId.Should().Be(InteracaoDa(db, 8001).Id);
        tarefa.InteracaoConclusaoId.Should().Be(conclusao.Id);
        conclusao.TarefaId.Should().Be(tarefa.Id);

        var processo = Processo(db, 1001);
        var linhas = await db.EstagiosDoProcesso.AsNoTracking().Where(e => e.NumeroDoProcessoNaOrigem == 1001).ToListAsync();
        linhas.Should().NotBeEmpty().And.OnlyContain(e => e.ProcessoId == processo.Id, "a linha do funil ganha o processo do CRM");

        (await db.ClienteCarteiras.AsNoTracking().SingleAsync(v => v.Id == _vinculoId)).UltimaInteracaoEm.Should().BeNull(
            "o último contato é da CARTEIRAS_VORTICE");
    }

    // =============================================================================================
    // As rodadas seguintes
    // =============================================================================================

    [Fact]
    public async Task A_segunda_rodada_sem_mudanca_nao_altera_nada_e_a_mudanca_na_origem_acompanha()
    {
        await Sincronizar();
        var segunda = await Sincronizar();

        segunda.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosIncluidos).Should().Be(0);
        segunda.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosAlterados).Should().Be(0);
        segunda.Valor(CargaDasOportunidadesDoVortice.RotuloDeTarefasIncluidas).Should().Be(0);
        segunda.Valor(CargaDasOportunidadesDoVortice.RotuloDeTarefasAlteradas).Should().Be(0);
        segunda.Valor(CargaDasOportunidadesDoVortice.RotuloDeInteracoesIncluidas).Should().Be(0, "a interação só é incluída uma vez");

        // A ORIGEM ANDA: o resumo muda e a tarefa é empurrada.
        var processos = Processos();
        processos[0] = processos[0] with { Resumo = "Trator 6125J — proposta revisada" };
        var tarefas = Tarefas();
        tarefas[0] = tarefas[0] with { AgendadaParaUtc = Em(2026, 10, 15) };

        var terceira = await Sincronizar(Leitura(processos, tarefas));
        terceira.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosAlterados).Should().Be(1);
        terceira.Valor(CargaDasOportunidadesDoVortice.RotuloDeTarefasAlteradas).Should().Be(1);

        await using var db = Sistema();
        Processo(db, 1001).Titulo.Should().Be("Trator 6125J — proposta revisada");
        TarefaDa(db, 7001)!.AgendadaPara.Should().Be(Em(2026, 10, 15));
    }

    [Fact]
    public async Task O_processo_que_sai_do_universo_e_excluido_com_as_tarefas_e_volta_restaurado()
    {
        await Sincronizar();

        var semUm = Processos().Where(p => p.Numero != 3000).ToList();
        var relatorio = await Sincronizar(Leitura(semUm));
        relatorio.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosExcluidos).Should().Be(1);
        relatorio.Valor(CargaDasOportunidadesDoVortice.RotuloDeTarefasExcluidas).Should().Be(1);

        await using (var db = Sistema())
        {
            Processo(db, 3000).EstaExcluido.Should().BeTrue("exclusão lógica, nunca apagado");
            TarefaDa(db, 7100)!.EstaExcluido.Should().BeTrue();
        }

        await Sincronizar();

        await using (var db = Sistema())
        {
            Processo(db, 3000).EstaExcluido.Should().BeFalse("voltou à origem");
            TarefaDa(db, 7100)!.EstaExcluido.Should().BeFalse();
        }
    }

    // =============================================================================================
    // As travas
    // =============================================================================================

    [Fact]
    public async Task Leitura_vazia_aborta_sem_gravar()
    {
        var resultado = await Rodar(Leitura(tarefas: []));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("Leitura vazia");
        await using var db = Sistema();
        (await db.Set<ProcessoDoCrm>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Queda_de_mais_de_5_por_cento_aborta_e_o_terminal_pode_aceitar()
    {
        await Sincronizar();

        var semDez = Processos().Where(p => p.Numero is < 3000 or >= 3010).ToList();
        var recusada = await Rodar(Leitura(semDez));
        recusada.EhSucesso.Should().BeFalse();
        recusada.Erro.Should().Contain("mais do que os 5");

        await using (var db = Sistema())
            (await db.Set<ProcessoDoCrm>().CountAsync(p => p.ExcluidoEm != null)).Should().Be(0, "nada foi excluído");

        var aceita = await Sincronizar(Leitura(semDez), aceitarQueda: true);
        aceita.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosExcluidos).Should().Be(10);
        aceita.Observacoes.Should().ContainSingle(o => o.Contains("--aceitar-queda"));
    }

    [Fact]
    public async Task A_simulacao_conta_e_nao_grava()
    {
        var simulada = await Rodar(simular: true);

        simulada.EhSucesso.Should().BeTrue(simulada.Erro);
        simulada.Valor.Simulada.Should().BeTrue();
        simulada.Valor.Valor(CargaDasOportunidadesDoVortice.RotuloDeProcessosIncluidos).Should().Be(3 + Enchimento);

        await using var db = Sistema();
        (await db.Set<ProcessoDoCrm>().CountAsync()).Should().Be(0);
        (await db.Tarefas.CountAsync()).Should().Be(0);
        (await db.TiposDeProcesso.CountAsync()).Should().Be(0, "nem os catálogos");
    }
}
