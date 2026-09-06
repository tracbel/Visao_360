namespace Tracbel.Crm.Dominio.Metadado;

/// <summary>Que tipo de dado o campo personalizado guarda.</summary>
public enum TipoDeCampo
{
    /// <summary>Texto livre.</summary>
    Texto = 0,

    /// <summary>Número.</summary>
    Numero = 1,

    /// <summary>Data.</summary>
    Data = 2,

    /// <summary>Sim ou não.</summary>
    Booleano = 3,

    /// <summary>Item de catálogo.</summary>
    Lista = 4,

    /// <summary>Referência a outro registro.</summary>
    Referencia = 5
}

/// <summary>
/// O campo extra declarado, sem release.
///
/// [V] O Vórtice guarda campo extra em 41 COLUNAS GENÉRICAS numa tabela de 51 — nomes como
/// campo1, numero3, simNao5, literal9 — e ninguém sabe o que cada uma guarda. Aqui a
/// extensão é declarada: tem nome, tipo, rótulo e validação, e a tela se monta a partir daqui.
///
/// A definição do campo é metadado; o VALOR do campo mora numa coluna JSON binário do próprio
/// registro estendido, indexável, sem o preço do modelo entidade-atributo-valor.
/// </summary>
public sealed class CampoPersonalizado
{
    private CampoPersonalizado() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Nome da entidade estendida. Ex.: Cliente, Processo.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>Nome do campo dentro do documento JSON. Ex.: hectaresIrrigados.</summary>
    public string Campo { get; private set; } = default!;

    /// <summary>O que o usuário lê na tela.</summary>
    public string Rotulo { get; private set; } = default!;

    /// <summary>Que tipo de dado o campo guarda.</summary>
    public TipoDeCampo TipoDeCampo { get; private set; }

    /// <summary>Se o campo é nativo do produto ou acrescentado pelo negócio.</summary>
    public bool EhPersonalizado { get; private set; } = true;

    /// <summary>Se o preenchimento é obrigatório.</summary>
    public bool EhObrigatorio { get; private set; }

    /// <summary>Tamanho máximo, quando é texto.</summary>
    public int? TamanhoMaximo { get; private set; }

    /// <summary>Catálogo de onde as opções vêm, quando o tipo é lista.</summary>
    public int? CatalogoId { get; private set; }

    /// <summary>Agrupamento na tela.</summary>
    public string? Grupo { get; private set; }

    /// <summary>Ordem dentro do grupo.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>Condição de visibilidade, avaliada na tela.</summary>
    public string? CondicaoVisibilidade { get; private set; }

    /// <summary>
    /// Regras de validação do campo, como JSON binário: faixa, expressão, lista de valores.
    /// A forma varia por tipo de campo, e por isso não são colunas.
    /// </summary>
    public string? Validacao { get; private set; }

    /// <summary>Se o campo aparece no cabeçalho do registro.</summary>
    public bool NoResumo { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Declara um campo personalizado.</summary>
    public static CampoPersonalizado Criar(string entidade, string campo, string rotulo, TipoDeCampo tipoDeCampo)
        => new() { Entidade = entidade, Campo = campo, Rotulo = rotulo, TipoDeCampo = tipoDeCampo };
}

/// <summary>Em que momento da gravação o tratador roda.</summary>
public enum MomentoDoTratador
{
    /// <summary>Antes da validação, para normalizar entrada.</summary>
    PreValidacao = 0,

    /// <summary>Antes de gravar, dentro da transação.</summary>
    PreOperacao = 1,

    /// <summary>Depois de gravar, dentro ou fora da transação.</summary>
    PosOperacao = 2
}

/// <summary>
/// O gancho declarado — comportamento novo é uma classe mais uma LINHA, nunca a alteração de
/// um arquivo existente.
///
/// [DYN] O ponto de extensão do Dataverse é uma linha de tabela, não uma alteração de código:
/// liga, desliga e reordena lógica sem implantação. [V] O Vórtice não tem esse ponto: ele
/// guarda 196 comandos SQL de regra de negócio dentro de uma coluna, com nome de tabela
/// traduzido em tempo de execução — nenhuma ferramenta de dependência enxerga isso.
/// </summary>
public sealed class TratadorDeEvento
{
    private TratadorDeEvento() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O evento que dispara. Ex.: Processo.AntesDeSalvar.</summary>
    public string Evento { get; private set; } = default!;

    /// <summary>Nome do tipo que implementa o tratador. Resolvido por injeção na inicialização.</summary>
    public string TipoImplementacao { get; private set; } = default!;

    /// <summary>Em que momento da gravação roda.</summary>
    public MomentoDoTratador Momento { get; private set; }

    /// <summary>Ordem entre tratadores do mesmo evento e momento.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>Se roda fora da transação. [DYN] só existe depois da operação.</summary>
    public bool EhAssincrono { get; private set; }

    /// <summary>
    /// Só dispara se estes campos mudarem, separados por vírgula. [DYN] é o filtro de
    /// atributos, a otimização que decide o desempenho de todo o sistema.
    /// </summary>
    public string? CamposFiltro { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Declara um tratador.</summary>
    public static TratadorDeEvento Criar(string evento, string tipoImplementacao, MomentoDoTratador momento)
        => new() { Evento = evento, TipoImplementacao = tipoImplementacao, Momento = momento };
}
