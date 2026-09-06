namespace Tracbel.Crm.Dominio.Workflow;

/// <summary>O que aconteceu e pode disparar regras.</summary>
public static class EventosWorkflow
{
    /// <summary>Uma tarefa foi concluída com um desfecho.</summary>
    public const string TarefaConcluida = "TarefaConcluida";

    /// <summary>Um processo nasceu.</summary>
    public const string ProcessoCriado = "ProcessoCriado";

    /// <summary>O processo mudou de estágio.</summary>
    public const string FaseAlterada = "FaseAlterada";

    /// <summary>Um lead chegou de fora.</summary>
    public const string LeadRecebido = "LeadRecebido";

    /// <summary>Um lead foi qualificado.</summary>
    public const string LeadQualificado = "LeadQualificado";
}

/// <summary>Como a regra terminou. Grava em <c>wf.RegraExecucao.Resultado</c>.</summary>
public enum ResultadoRegra
{
    /// <summary>A regra rodou e produziu o efeito.</summary>
    Disparou = 0,

    /// <summary>A condição foi avaliada como falsa. Comportamento normal, não erro.</summary>
    CondicaoFalsa = 1,

    /// <summary>A regra existe mas está desligada.</summary>
    RegraInativa = 2,

    /// <summary>A expressão de destinatário não resolveu ninguém. Precisa de atenção.</summary>
    SemDestinatario = 3,

    /// <summary>Exceção durante a execução.</summary>
    Erro = 4,

    /// <summary>Suprimida por outra regra de maior precedência.</summary>
    Suprimida = 5
}

/// <summary>
/// Uma regra de automação: quando ACONTECE algo, SE a condição for verdadeira,
/// FAÇA um efeito, PARA alguém.
///
/// [V] A CORREÇÃO MAIS IMPORTANTE DO PROJETO ESTÁ AQUI.
///
/// No Vórtice, <c>IV_AcaoAuto</c> tem 31 colunas, das quais 9 estão 100% nulas, e
/// <b>não tem linguagem de condição</b>: a tabela que guardaria as condições
/// (<c>IV_AcaoAutoCtrl</c>) está VAZIA, e <c>UsaObjDyn = 0</c> em 100% das linhas.
/// Sem condição, o único discriminante é o par (Resultado, NroEmpresa) — e a única
/// saída para variar o comportamento é REPLICAR a regra por filial.
///
/// Medido: <b>5.958 regras para apenas 1.262 pares (Resultado, Ação) distintos</b>.
///
/// Duas colunas resolvem isso: <see cref="Condicao"/> e <see cref="ExpressaoDestinatario"/>.
/// </summary>
public sealed class Regra
{
    private Regra() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável, usado em log e em referência cruzada.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Explicação em português, para quem for manter daqui a dois anos.</summary>
    public string? Descricao { get; private set; }

    // ---- GATILHO ----

    /// <summary>O evento que dispara. Ver <see cref="EventosWorkflow"/>.</summary>
    public string Evento { get; private set; } = default!;

    /// <summary>Restringe a um tipo de fluxo. Nulo vale para qualquer um.</summary>
    public int? TipoProcessoId { get; private set; }

    /// <summary>Restringe a um tipo de tarefa.</summary>
    public int? TipoTarefaId { get; private set; }

    /// <summary>Restringe a um desfecho específico.</summary>
    public int? ResultadoId { get; private set; }

    // ---- CONDIÇÃO ----

    /// <summary>
    /// Expressão booleana avaliada em runtime contra o contexto do evento.
    /// Ex.: <c>processo.ValorEstimado &gt; 500000 &amp;&amp; conta.Situacao == 'Cliente'</c>.
    /// Nulo significa "sempre verdadeira".
    /// </summary>
    public string? Condicao { get; private set; }

    // ---- EFEITO ----

    /// <summary>Nome do efeito. Precisa bater com <c>IEfeitoRegra.Nome</c>.</summary>
    public string Efeito { get; private set; } = default!;

    /// <summary>Tipo de tarefa a criar, quando o efeito é CriarTarefa.</summary>
    public int? EfeitoTipoTarefaId { get; private set; }

    /// <summary>Fase de destino, quando o efeito é MoverFase.</summary>
    public int? EfeitoFaseId { get; private set; }

    /// <summary>Parâmetros extras do efeito, em JSON.</summary>
    public string? EfeitoParametros { get; private set; }

    /// <summary>Prazo da tarefa gerada, em dias úteis.</summary>
    public int PrazoDiasUteis { get; private set; } = 1;

    // ---- DESTINATÁRIO ----

    /// <summary>
    /// Expressão que resolve PARA QUEM vai o efeito — avaliada NO MOMENTO DA EXECUÇÃO.
    ///
    /// [V] <c>IV_AcaoAuto.SeqUsuario</c> guarda um ID FIXO na linha da regra. Duas
    /// consequências medidas: a regra 9473 mandou 150 tarefas para o mesmo usuário desde
    /// 10/04/2026; e a aprovação é gravada para o líder do CEN no momento da geração,
    /// então trocar o líder no cadastro NÃO corrige as agendas já criadas.
    ///
    /// Valores aceitos: <c>processo.Proprietario</c>, <c>carteira.Responsavel</c>,
    /// <c>gestorDe(processo.Proprietario)</c>, <c>equipe:CODIGO</c>, <c>quemExecutou</c>,
    /// <c>usuario:UPN</c> (desencorajado — é a forma que produziu as 5.958 regras).
    /// </summary>
    public string ExpressaoDestinatario { get; private set; } = default!;

    /// <summary>Ordem de execução entre regras do mesmo gatilho. Menor roda primeiro.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>
    /// Desligar sem apagar. [V] o <c>EMUSO</c> do Vórtice é um acerto e foi copiado —
    /// 234 regras estão desligadas lá sem terem sido perdidas.
    /// </summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>
    /// Se verdadeiro, falha nesta regra aborta a transação inteira.
    /// Se falso, registra e segue — que é o padrão.
    /// </summary>
    public bool EhCritica { get; private set; }

    /// <summary>Quando a regra foi criada (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quem criou. Mudança de regra é auditada permanentemente.</summary>
    public long CriadoPorId { get; private set; }

    /// <summary>Última alteração (UTC).</summary>
    public DateTime? AlteradoEm { get; private set; }

    /// <summary>Quem alterou por último.</summary>
    public long? AlteradoPorId { get; private set; }

    /// <summary>Cria uma regra. Usado pelo seed e pela tela de configuração.</summary>
    public static Regra Criar(
        string codigo,
        string nome,
        string evento,
        string efeito,
        string expressaoDestinatario,
        string? condicao = null,
        int? resultadoId = null,
        int? tipoProcessoId = null,
        int? tipoTarefaId = null,
        int? efeitoTipoTarefaId = null,
        int? efeitoFaseId = null,
        int prazoDiasUteis = 1,
        short ordem = 100,
        bool ehCritica = false,
        string? descricao = null) => new()
        {
            Codigo = codigo,
            Nome = nome,
            Descricao = descricao,
            Evento = evento,
            Efeito = efeito,
            ExpressaoDestinatario = expressaoDestinatario,
            Condicao = condicao,
            ResultadoId = resultadoId,
            TipoProcessoId = tipoProcessoId,
            TipoTarefaId = tipoTarefaId,
            EfeitoTipoTarefaId = efeitoTipoTarefaId,
            EfeitoFaseId = efeitoFaseId,
            PrazoDiasUteis = prazoDiasUteis,
            Ordem = ordem,
            EhCritica = ehCritica
        };

    /// <summary>Liga a regra.</summary>
    public void Ativar() => EstaAtiva = true;

    /// <summary>Desliga a regra, preservando-a para auditoria e para religar depois.</summary>
    public void Desativar() => EstaAtiva = false;
}

/// <summary>
/// O registro de UMA avaliação de regra.
///
/// [V] ESTA CLASSE É A RAZÃO DE O DEFEITO 899/900 NÃO PODER ACONTECER DE NOVO.
///
/// No Vórtice, a taxa de geração da ação 900 caiu de 68% (março) para 3% (agosto de 2026)
/// e ninguém percebeu, porque uma regra que não dispara não deixa rastro nenhum.
/// Aqui, TODA avaliação é gravada — inclusive, e principalmente, as que NÃO dispararam,
/// com o motivo em português.
/// </summary>
public sealed class ExecucaoRegra
{
    /// <summary>Identificador interno.</summary>
    public long Id { get; init; }

    /// <summary>Qual regra foi avaliada.</summary>
    public required int RegraId { get; init; }

    /// <summary>Agrupa todas as regras avaliadas no mesmo evento.</summary>
    public required Guid CorrelacaoId { get; init; }

    /// <summary>O evento que provocou a avaliação.</summary>
    public required string Evento { get; init; }

    /// <summary>Processo envolvido, quando houve.</summary>
    public long? ProcessoId { get; init; }

    /// <summary>Tarefa envolvida, quando houve.</summary>
    public long? TarefaId { get; init; }

    /// <summary>Interação que disparou, quando houve.</summary>
    public long? InteracaoId { get; init; }

    /// <summary>Como terminou.</summary>
    public required ResultadoRegra Resultado { get; init; }

    /// <summary>
    /// A explicação em português. É ISTO que o suporte vai ler.
    /// Ex.: "Condição falsa: ValorEstimado (120.000,00) não é maior que 500.000,00".
    /// </summary>
    public string? Motivo { get; init; }

    /// <summary>Tarefa criada pelo efeito, quando houve.</summary>
    public long? TarefaCriadaId { get; init; }

    /// <summary>Destinatário efetivamente resolvido.</summary>
    public long? DestinatarioResolvidoId { get; init; }

    /// <summary>Quanto a avaliação demorou. Alimenta o orçamento de performance.</summary>
    public required int DuracaoMs { get; init; }

    /// <summary>Quando (UTC).</summary>
    public DateTime ExecutadoEm { get; init; } = DateTime.UtcNow;
}
