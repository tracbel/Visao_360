using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Workflow;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Workflow;

/// <summary>
/// Testes do motor de workflow.
///
/// O portão de qualidade da fase 1 exige COBERTURA DE REGRA = 100%: cada regra testada
/// disparando E não disparando. Estes testes provam que o motor cumpre o contrato que
/// torna isso possível.
///
/// O caso mais importante é <see cref="Registra_no_log_quando_a_condicao_e_falsa"/>.
/// [V] No Vórtice, a taxa de geração da ação 900 caiu de 68% (março) para 3% (agosto/2026)
/// e ninguém percebeu, porque regra que não dispara não deixa rastro.
/// </summary>
[Trait("Categoria", "Workflow")]
public class MotorWorkflowTestes
{
    // ------------------------------------------------------------------ dublês

    private sealed class RepositorioFalso(params Regra[] regras) : IRepositorioRegras
    {
        public Task<IReadOnlyList<Regra>> ObterPorGatilhoAsync(
            string evento, int? resultadoId, int? tipoProcessoId, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<Regra>>(
                regras.Where(r => r.Evento == evento).ToList());
    }

    /// <summary>Avaliador que devolve sempre o mesmo veredito, com explicação.</summary>
    private sealed class AvaliadorFalso(bool verdadeira, string explicacao) : IAvaliadorCondicao
    {
        public Task<AvaliacaoCondicao> AvaliarAsync(string expressao, ContextoRegra ctx, CancellationToken ct)
            => Task.FromResult(new AvaliacaoCondicao(verdadeira, explicacao));
    }

    /// <summary>Registro em memória — é sobre ele que as asserções são feitas.</summary>
    private sealed class RegistroFalso : IRegistroExecucaoRegra
    {
        public List<ExecucaoRegra> Gravadas { get; } = [];

        public Task GravarAsync(ExecucaoRegra execucao, CancellationToken ct)
        {
            Gravadas.Add(execucao);
            return Task.CompletedTask;
        }
    }

    private sealed class RelogioFixo : IRelogio
    {
        public DateTime Agora { get; } = new(2026, 8, 30, 12, 0, 0, DateTimeKind.Utc);
    }

    private sealed class EfeitoQueAplica : IEfeitoRegra
    {
        public string Nome => "CriarTarefa";
        public Task<ResultadoEfeito> ExecutarAsync(ContextoRegra ctx, CancellationToken ct)
            => Task.FromResult(ResultadoEfeito.Aplicado("Tarefa criada para João Mendes.", 999, 100));
    }

    private sealed class EfeitoSemDestinatario : IEfeitoRegra
    {
        public string Nome => "CriarTarefa";
        public Task<ResultadoEfeito> ExecutarAsync(ContextoRegra ctx, CancellationToken ct)
            => Task.FromResult(ResultadoEfeito.NaoAplicado(
                "Não foi possível resolver o destinatário pela expressão 'carteira.Responsavel'."));
    }

    private sealed class EfeitoQueExplode : IEfeitoRegra
    {
        public string Nome => "CriarTarefa";
        public Task<ResultadoEfeito> ExecutarAsync(ContextoRegra ctx, CancellationToken ct)
            => throw new InvalidOperationException("banco fora do ar");
    }

    // ------------------------------------------------------------------ apoio

    private static Regra RegraDeTeste(
        string codigo = "R001", string? condicao = null, bool ativa = true,
        string efeito = "CriarTarefa", bool critica = false, short ordem = 100)
    {
        var regra = Regra.Criar(
            codigo: codigo,
            nome: "Gerar tarefa de acompanhamento",
            evento: EventosWorkflow.TarefaConcluida,
            efeito: efeito,
            expressaoDestinatario: "carteira.Responsavel",
            condicao: condicao,
            ordem: ordem,
            ehCritica: critica);

        if (!ativa) regra.Desativar();
        return regra;
    }

    private static ContextoRegra Contexto() => new()
    {
        Evento = EventosWorkflow.TarefaConcluida,
        EmpresaId = 1,
        UsuarioExecutorId = 100,
        ProcessoId = 555,
        ProcessoNumero = 1582375,
        ProcessoValorEstimado = 120_000m
    };

    private static (MotorWorkflow motor, RegistroFalso registro) Montar(
        IEfeitoRegra efeito, IAvaliadorCondicao avaliador, params Regra[] regras)
    {
        var registro = new RegistroFalso();
        var motor = new MotorWorkflow(
            new RepositorioFalso(regras), avaliador, [efeito], registro, new RelogioFixo());
        return (motor, registro);
    }

    // ------------------------------------------------------------------ testes

    [Fact]
    public async Task Dispara_e_registra_quando_a_condicao_e_verdadeira()
    {
        var (motor, registro) = Montar(
            new EfeitoQueAplica(),
            new AvaliadorFalso(true, "ok"),
            RegraDeTeste(condicao: "processo.ValorEstimado > 100000"));

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Disparos.Should().ContainSingle().Which.Should().Be("R001");
        registro.Gravadas.Should().ContainSingle();
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.Disparou);
        registro.Gravadas[0].TarefaCriadaId.Should().Be(999);
    }

    [Fact]
    public async Task Registra_no_log_quando_a_condicao_e_falsa()
    {
        // ESTE É O TESTE QUE DEFINE O PROJETO.
        //
        // [V] No Vórtice, uma regra que não dispara é indistinguível de uma regra que não
        // existe: não há registro nenhum. Foi assim que a queda de 68% para 3% na geração
        // da ação 900 passou cinco meses despercebida, até o comercial reclamar de
        // processos travados — 155 de 239 em "Entrega" sem nenhuma agenda pendente.
        const string explicacao = "Condição falsa: ValorEstimado (120.000,00) não é maior que 500.000,00";

        var (motor, registro) = Montar(
            new EfeitoQueAplica(),
            new AvaliadorFalso(false, explicacao),
            RegraDeTeste(condicao: "processo.ValorEstimado > 500000"));

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Disparos.Should().BeEmpty("a condição era falsa");
        relatorio.TotalAvaliadas.Should().Be(1);

        registro.Gravadas.Should().ContainSingle("NÃO disparar também precisa deixar rastro");
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.CondicaoFalsa);
        registro.Gravadas[0].Motivo.Should().Be(explicacao,
            "o motivo em português é o que o suporte lê para diagnosticar em segundos");
    }

    [Fact]
    public async Task Registra_quando_a_regra_esta_desligada()
    {
        var (motor, registro) = Montar(
            new EfeitoQueAplica(), new AvaliadorFalso(true, "ok"),
            RegraDeTeste(ativa: false));

        await motor.AvaliarAsync(Contexto());

        registro.Gravadas.Should().ContainSingle();
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.RegraInativa);
    }

    [Fact]
    public async Task Registra_quando_o_destinatario_nao_resolve()
    {
        // [V] No Vórtice, regra que não acha destinatário simplesmente não gera a tarefa,
        // e ninguém fica sabendo. Aqui vira uma linha com motivo explícito.
        var (motor, registro) = Montar(
            new EfeitoSemDestinatario(), new AvaliadorFalso(true, "ok"), RegraDeTeste());

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Disparos.Should().BeEmpty();
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.SemDestinatario);
        registro.Gravadas[0].Motivo.Should().Contain("destinatário");
    }

    [Fact]
    public async Task Registra_quando_o_efeito_nao_tem_implementacao()
    {
        // Configuração aponta para efeito inexistente: erro de operação, precisa gritar.
        var (motor, registro) = Montar(
            new EfeitoQueAplica(), new AvaliadorFalso(true, "ok"),
            RegraDeTeste(efeito: "EfeitoQueNinguemImplementou"));

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Falhas.Should().ContainSingle();
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.Erro);
        registro.Gravadas[0].Motivo.Should().Contain("não tem implementação registrada");
    }

    [Fact]
    public async Task Uma_regra_quebrada_nao_derruba_as_outras()
    {
        // [V] No Vórtice as ações 899 e 900 caem JUNTAS, quase 1:1, enquanto 894 e 814
        // continuam gerando normalmente. Isso indica que a falha não é do motor inteiro,
        // e que ele não isola falha por regra. Aqui isola.
        var registro = new RegistroFalso();
        var motor = new MotorWorkflow(
            new RepositorioFalso(
                RegraDeTeste("R_QUEBRA", efeito: "Explode", ordem: 1),
                RegraDeTeste("R_BOA", ordem: 2)),
            new AvaliadorFalso(true, "ok"),
            [new EfeitoQueExplodeNomeado(), new EfeitoQueAplica()],
            registro, new RelogioFixo());

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Falhas.Should().ContainSingle().Which.Should().Contain("R_QUEBRA");
        relatorio.Disparos.Should().ContainSingle().Which.Should().Be("R_BOA");
        registro.Gravadas.Should().HaveCount(2, "as duas avaliações são registradas");
    }

    private sealed class EfeitoQueExplodeNomeado : IEfeitoRegra
    {
        public string Nome => "Explode";
        public Task<ResultadoEfeito> ExecutarAsync(ContextoRegra ctx, CancellationToken ct)
            => throw new InvalidOperationException("banco fora do ar");
    }

    [Fact]
    public async Task Regra_critica_aborta_mas_so_depois_de_registrar()
    {
        var registro = new RegistroFalso();
        var motor = new MotorWorkflow(
            new RepositorioFalso(RegraDeTeste("R_CRITICA", efeito: "Explode", critica: true)),
            new AvaliadorFalso(true, "ok"),
            [new EfeitoQueExplodeNomeado()],
            registro, new RelogioFixo());

        var acao = async () => await motor.AvaliarAsync(Contexto());

        await acao.Should().ThrowAsync<InvalidOperationException>();
        registro.Gravadas.Should().ContainSingle(
            "mesmo abortando, o motivo fica gravado — senão perdemos o diagnóstico");
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.Erro);
    }

    [Fact]
    public async Task Todas_as_regras_do_evento_compartilham_a_mesma_correlacao()
    {
        // A correlação é o que permite reconstruir "o que o sistema fez quando o vendedor
        // clicou em concluir" numa query só.
        var (motor, registro) = Montar(
            new EfeitoQueAplica(), new AvaliadorFalso(true, "ok"),
            RegraDeTeste("R1", ordem: 1), RegraDeTeste("R2", ordem: 2));

        await motor.AvaliarAsync(Contexto());

        registro.Gravadas.Should().HaveCount(2);
        registro.Gravadas.Select(g => g.CorrelacaoId).Distinct().Should().ContainSingle();
    }

    [Fact]
    public async Task Respeita_a_ordem_configurada()
    {
        var (motor, registro) = Montar(
            new EfeitoQueAplica(), new AvaliadorFalso(true, "ok"),
            RegraDeTeste("R_SEGUNDA", ordem: 20), RegraDeTeste("R_PRIMEIRA", ordem: 10));

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Disparos.Should().Equal("R_PRIMEIRA", "R_SEGUNDA");
    }

    [Fact]
    public async Task Regra_sem_condicao_sempre_dispara()
    {
        var (motor, registro) = Montar(
            new EfeitoQueAplica(),
            new AvaliadorFalso(false, "nunca deveria ser chamado"),
            RegraDeTeste(condicao: null));

        var relatorio = await motor.AvaliarAsync(Contexto());

        relatorio.Disparos.Should().ContainSingle();
        registro.Gravadas[0].Resultado.Should().Be(ResultadoRegra.Disparou);
    }
}
