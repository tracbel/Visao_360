using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Processo;

/// <summary>
/// O modelo do fluxo: venda de equipamento, demonstração, aferição, cobrança.
///
/// [V] Equivale ao catálogo de 62 modelos do Vórtice, dos quais 58 estão em uso. O nome lá
/// dava a entender que era o número do processo, e não era.
/// </summary>
public sealed class TipoProcesso
{
    private TipoProcesso() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável. Ex.: VENDA_EQUIPAMENTO.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Linha de negócio do fluxo. Nulo vale para todas.</summary>
    public int? LinhaDeNegocioId { get; private set; }

    /// <summary>
    /// Versão da definição. Trocar o fluxo cria versão nova e os processos em andamento
    /// continuam na versão em que nasceram — [V] no Vórtice, trocar o tipo reescreve o
    /// histórico.
    /// </summary>
    public int Versao { get; private set; } = 1;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cria um tipo de processo.</summary>
    /// <param name="codigo">Código estável.</param>
    /// <param name="nome">Nome legível.</param>
    /// <param name="linhaDeNegocioId">Linha de negócio do fluxo. Nulo vale para todas.</param>
    /// <param name="estaAtivo">Se o fluxo continua em uso.</param>
    public static TipoProcesso Criar(
        string codigo, string nome, int? linhaDeNegocioId = null, bool estaAtivo = true) =>
        new() { Codigo = codigo, Nome = nome, LinhaDeNegocioId = linhaDeNegocioId, EstaAtivo = estaAtivo };
}

/// <summary>
/// Em que ponto o processo está — a barra que o usuário vê no topo da oportunidade.
///
/// Pertence ao tipo de processo, tem ordem e probabilidade de fechamento. Não é catálogo
/// genérico porque tem pai e tem comportamento.
/// </summary>
public sealed class Fase
{
    private Fase() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O tipo de processo a que a fase pertence.</summary>
    public int TipoProcessoId { get; private set; }

    /// <summary>Código estável dentro do tipo. Ex.: QUALIFICACAO.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Posição na barra de fases.</summary>
    public short Ordem { get; private set; }

    /// <summary>Marco do funil, para relatório.</summary>
    public string? Marco { get; private set; }

    /// <summary>Probabilidade de fechamento associada à fase, de 0 a 100.</summary>
    public short? ProbabilidadePercentual { get; private set; }

    /// <summary>Se o processo só avança com os campos obrigatórios da fase preenchidos.</summary>
    public bool ExigeCamposObrigatorios { get; private set; } = true;

    /// <summary>Se é fase de encerramento.</summary>
    public bool EhFinal { get; private set; }

    /// <summary>Cria uma fase.</summary>
    /// <param name="tipoProcessoId">O tipo de processo dono da fase.</param>
    /// <param name="codigo">Código estável dentro do tipo.</param>
    /// <param name="nome">Nome legível.</param>
    /// <param name="ordem">Posição na barra de fases.</param>
    /// <param name="ehFinal">Se é fase de encerramento.</param>
    public static Fase Criar(
        int tipoProcessoId, string codigo, string nome, short ordem, bool ehFinal = false) => new()
    {
        TipoProcessoId = tipoProcessoId,
        Codigo = codigo,
        Nome = nome,
        Ordem = ordem,
        EhFinal = ehFinal
    };
}

/// <summary>Como o contato com o cliente aconteceu.</summary>
public enum CategoriaDeInteracao
{
    /// <summary>Visita presencial.</summary>
    Visita = 0,

    /// <summary>Ligação telefônica.</summary>
    Ligacao = 1,

    /// <summary>Mensagem por WhatsApp.</summary>
    WhatsApp = 2,

    /// <summary>Mensagem por e-mail.</summary>
    Email = 3,

    /// <summary>Reunião remota.</summary>
    Remota = 4,

    /// <summary>Passo interno, sem contato com o cliente.</summary>
    Interna = 5
}

/// <summary>
/// Visita, ligação, WhatsApp, e-mail, reunião — com prazo, cor e formulário exigido.
///
/// Absorve a categoria de interação do protótipo: no Vórtice já é o mesmo campo. Tem
/// comportamento (exige georreferência? exige formulário? conta para cobertura?), logo é
/// tabela própria e não item de catálogo genérico.
///
/// [V] O catálogo equivalente do Vórtice tem 979 linhas, das quais 602 estão fora de uso na
/// MESMA lista das vivas.
/// </summary>
public sealed class TipoTarefa
{
    private TipoTarefa() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Tipo de processo a que se aplica. Nulo vale para qualquer fluxo.</summary>
    public int? TipoProcessoId { get; private set; }

    /// <summary>Como o contato acontece.</summary>
    public CategoriaDeInteracao Categoria { get; private set; } = CategoriaDeInteracao.Interna;

    /// <summary>Prazo padrão, em dias úteis.</summary>
    public short PrazoDiasUteis { get; private set; } = 1;

    /// <summary>Se a tarefa é um passo de aprovação.</summary>
    public bool EhAprovacao { get; private set; }

    /// <summary>Se a conclusão exige registro de coordenada.</summary>
    public bool ExigeGeorreferencia { get; private set; }

    /// <summary>Se a conclusão conta para o indicador de cobertura de carteira.</summary>
    public bool ContaParaCobertura { get; private set; }

    /// <summary>Cor de exibição na agenda, em hexadecimal.</summary>
    public string? Cor { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Quando o tipo foi usado pela última vez. Alimenta a higienização do catálogo.</summary>
    public DateTime? UltimoUsoEm { get; private set; }

    /// <summary>Cria um tipo de tarefa.</summary>
    /// <param name="codigo">Código estável.</param>
    /// <param name="nome">Nome legível.</param>
    /// <param name="categoria">Como o contato acontece.</param>
    /// <param name="prazoDiasUteis">
    /// Prazo padrão, em dias úteis. ZERO significa "a origem não declara prazo" — e é um valor
    /// legítimo, não um descuido: [V] nenhuma das ações em uso no sistema de origem preenche
    /// prazo, e inventar um faria a agenda inteira nascer com atraso fabricado.
    /// </param>
    /// <param name="contaParaCobertura">Se a conclusão conta para o indicador de cobertura.</param>
    /// <param name="estaAtivo">Se o tipo continua em uso.</param>
    /// <param name="ultimoUsoEm">Quando o tipo foi usado pela última vez.</param>
    public static TipoTarefa Criar(
        string codigo,
        string nome,
        CategoriaDeInteracao categoria,
        short prazoDiasUteis = 1,
        bool contaParaCobertura = false,
        bool estaAtivo = true,
        DateTime? ultimoUsoEm = null)
    {
        if (prazoDiasUteis < 0)
            throw new RegraDeNegocioViolada("Prazo de tarefa em dias úteis não é negativo.");

        return new TipoTarefa
        {
            Codigo = codigo,
            Nome = nome,
            Categoria = categoria,
            PrazoDiasUteis = prazoDiasUteis,
            ContaParaCobertura = contaParaCobertura,
            EstaAtivo = estaAtivo,
            UltimoUsoEm = ultimoUsoEm
        };
    }
}

/// <summary>O que o desfecho faz com o funil.</summary>
public enum ClasseDeResultado
{
    /// <summary>Move o processo adiante.</summary>
    Avanco = 0,

    /// <summary>Mantém o processo onde está.</summary>
    Manutencao = 1,

    /// <summary>Encerra o processo como perdido.</summary>
    Perda = 2,

    /// <summary>Encerra o processo como cancelado.</summary>
    Cancelamento = 3
}

/// <summary>
/// O desfecho que move a fase.
///
/// Tem efeito, não é rótulo. [V] O equivalente do Vórtice tem 63 colunas, 35 delas
/// prefixadas com uma sigla de controle e domínio não documentado, em 4.201 linhas.
/// </summary>
public sealed class Resultado
{
    private Resultado() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O tipo de tarefa a que o desfecho pertence.</summary>
    public int TipoTarefaId { get; private set; }

    /// <summary>Código estável dentro do tipo de tarefa.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>O que o desfecho faz com o funil.</summary>
    public ClasseDeResultado Classe { get; private set; }

    /// <summary>Fase para onde o processo vai quando este desfecho é registrado.</summary>
    public int? FaseDestinoId { get; private set; }

    /// <summary>Se exige justificativa escrita.</summary>
    public bool ExigeJustificativa { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Quando o desfecho foi usado pela última vez.</summary>
    public DateTime? UltimoUsoEm { get; private set; }

    /// <summary>Cria um resultado.</summary>
    /// <param name="tipoTarefaId">O tipo de tarefa a que o desfecho pertence.</param>
    /// <param name="codigo">Código estável dentro do tipo de tarefa.</param>
    /// <param name="nome">Nome legível.</param>
    /// <param name="classe">O que o desfecho faz com o funil.</param>
    /// <param name="estaAtivo">Se o desfecho continua em uso.</param>
    /// <param name="ultimoUsoEm">Quando o desfecho foi usado pela última vez.</param>
    public static Resultado Criar(
        int tipoTarefaId,
        string codigo,
        string nome,
        ClasseDeResultado classe,
        bool estaAtivo = true,
        DateTime? ultimoUsoEm = null) => new()
    {
        TipoTarefaId = tipoTarefaId,
        Codigo = codigo,
        Nome = nome,
        Classe = classe,
        EstaAtivo = estaAtivo,
        UltimoUsoEm = ultimoUsoEm
    };
}

/// <summary>A que família o motivo de perda pertence.</summary>
public enum CategoriaDeMotivoDePerda
{
    /// <summary>Perdemos no preço.</summary>
    Preco = 0,

    /// <summary>Perdemos no prazo de entrega.</summary>
    Prazo = 1,

    /// <summary>Perdemos no produto ou na especificação.</summary>
    Produto = 2,

    /// <summary>Perdemos no financiamento ou no crédito.</summary>
    Financiamento = 3,

    /// <summary>O cliente desistiu de comprar.</summary>
    Desistencia = 4,

    /// <summary>Perdemos para um concorrente, por outro motivo.</summary>
    Concorrencia = 5,

    /// <summary>Qualquer outro, com observação obrigatória.</summary>
    Outro = 6
}

/// <summary>
/// Por que o negócio não fechou.
///
/// Tem atributo próprio e tela de administração, logo é tabela e não item de catálogo
/// genérico. O item Outro exige observação: se ele passar de 15% dos usos num trimestre,
/// falta item no catálogo — e isso é alerta, não opinião.
/// </summary>
public sealed class MotivoDePerda
{
    private MotivoDePerda() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>A que família o motivo pertence.</summary>
    public CategoriaDeMotivoDePerda Categoria { get; private set; }

    /// <summary>Se registrar este motivo exige informar o concorrente.</summary>
    public bool ExigeConcorrente { get; private set; }

    /// <summary>Se registrar este motivo exige texto explicando.</summary>
    public bool ExigeObservacao { get; private set; }

    /// <summary>Ordem de exibição.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cria um motivo de perda.</summary>
    public static MotivoDePerda Criar(string codigo, string nome, CategoriaDeMotivoDePerda categoria) => new()
    {
        Codigo = codigo,
        Nome = nome,
        Categoria = categoria
    };
}
