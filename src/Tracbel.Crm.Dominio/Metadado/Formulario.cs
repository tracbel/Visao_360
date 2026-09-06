using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Metadado;

/// <summary>
/// A definição de um formulário.
///
/// [V] Aqui está o maior ganho isolado do modelo: o Vórtice cria UMA TABELA FÍSICA POR
/// FORMULÁRIO — 175 tabelas, 2.601 colunas, 32 delas vazias, com nomes truncados em 20
/// caracteres gerados pelo motor. Publicar formulário lá é comando de alteração de esquema;
/// aqui é inserção de linha. As 179 tabelas do subsistema viram quatro.
/// </summary>
public sealed class Formulario
{
    private Formulario() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve, em português.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Tipo de processo a que o formulário pertence. Nulo vale para qualquer fluxo.</summary>
    public int? TipoProcessoId { get; private set; }

    /// <summary>
    /// Versão publicada. Mudar o formulário publica versão nova e NÃO reescreve o passado —
    /// [V] no Vórtice, alterar o formulário altera a tabela e reescreve o histórico.
    /// </summary>
    public int VersaoPublicada { get; private set; } = 1;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cria um formulário.</summary>
    public static Formulario Criar(string codigo, string nome) => new() { Codigo = codigo, Nome = nome };
}

/// <summary>Que tipo de resposta a pergunta aceita.</summary>
public enum TipoDeResposta
{
    /// <summary>Texto livre.</summary>
    Texto = 0,

    /// <summary>Número.</summary>
    Numero = 1,

    /// <summary>Data.</summary>
    Data = 2,

    /// <summary>Sim ou não.</summary>
    Booleano = 3,

    /// <summary>Um item de catálogo. Nunca texto livre.</summary>
    Catalogo = 4,

    /// <summary>Um cliente do cadastro.</summary>
    Cliente = 5,

    /// <summary>Uma máquina, pelo chassi.</summary>
    Equipamento = 6,

    /// <summary>Vários itens de catálogo ao mesmo tempo.</summary>
    CatalogoMultiplo = 7
}

/// <summary>
/// Uma pergunta do formulário, com tipo e condição.
///
/// A pergunta condicional é DADO, não tabela nova: é ela que evita que uma variação de
/// negócio vire uma tabela de formulário a mais, como aconteceu com as três variantes de
/// gestão de crédito e as três de demonstração no Vórtice.
/// </summary>
public sealed class Pergunta
{
    private Pergunta() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O formulário a que a pergunta pertence.</summary>
    public int FormularioId { get; private set; }

    /// <summary>Posição no formulário.</summary>
    public short Ordem { get; private set; }

    /// <summary>Código estável dentro do formulário. Ex.: MOTIVO_PERDA.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O texto que o CEN lê na tela.</summary>
    public string Texto { get; private set; } = default!;

    /// <summary>Que tipo de resposta a pergunta aceita.</summary>
    public TipoDeResposta TipoDeResposta { get; private set; }

    /// <summary>O catálogo de onde as opções vêm, quando o tipo é de catálogo.</summary>
    public int? CatalogoId { get; private set; }

    /// <summary>Se a resposta é obrigatória.</summary>
    public bool EhObrigatoria { get; private set; }

    /// <summary>A pergunta de que esta depende para aparecer.</summary>
    public int? PerguntaCondicaoId { get; private set; }

    /// <summary>O valor que a pergunta de condição precisa ter para esta aparecer.</summary>
    public string? ValorCondicao { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma pergunta.</summary>
    public static Pergunta Criar(
        int formularioId,
        short ordem,
        string codigo,
        string texto,
        TipoDeResposta tipoDeResposta,
        int? catalogoId = null)
    {
        // Pergunta de catálogo sem catálogo é exatamente o campo de texto livre que o
        // documento 17 proíbe — o que produziu FINALIZADO ao lado de FINALIZADA no legado.
        var precisaDeCatalogo = tipoDeResposta is TipoDeResposta.Catalogo or TipoDeResposta.CatalogoMultiplo;
        if (precisaDeCatalogo && catalogoId is null)
            throw new RegraDeNegocioViolada(
                $"A pergunta '{codigo}' é de catálogo e precisa dizer de qual catálogo as opções vêm.");

        return new Pergunta
        {
            FormularioId = formularioId,
            Ordem = ordem,
            Codigo = codigo,
            Texto = texto,
            TipoDeResposta = tipoDeResposta,
            CatalogoId = catalogoId
        };
    }
}

/// <summary>Em que ponto o preenchimento está.</summary>
public enum SituacaoDoPreenchimento
{
    /// <summary>Começou e não terminou.</summary>
    EmAndamento = 0,

    /// <summary>Terminou.</summary>
    Concluido = 1,

    /// <summary>Foi cancelado.</summary>
    Cancelado = 2
}

/// <summary>
/// Uma resposta ao formulário, como um todo.
///
/// Junto com a resposta por pergunta, este par substitui as 175 tabelas físicas do Vórtice
/// mais a tabela de questionário — 176 tabelas viram duas.
/// </summary>
public sealed class Preenchimento
{
    private Preenchimento() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O formulário preenchido.</summary>
    public int FormularioId { get; private set; }

    /// <summary>A versão do formulário no momento do preenchimento. Congela o passado.</summary>
    public int VersaoFormulario { get; private set; }

    /// <summary>O cliente a que o preenchimento se refere.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O processo a que o preenchimento se refere.</summary>
    public long? ProcessoId { get; private set; }

    /// <summary>A tarefa cuja conclusão exigiu o preenchimento.</summary>
    public long? TarefaId { get; private set; }

    /// <summary>A interação em que o formulário foi preenchido.</summary>
    public long? InteracaoId { get; private set; }

    /// <summary>Quem preencheu.</summary>
    public long PreenchidoPorId { get; private set; }

    /// <summary>Quando terminou de preencher (UTC).</summary>
    public DateTime PreenchidoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Em que ponto está.</summary>
    public SituacaoDoPreenchimento Situacao { get; private set; } = SituacaoDoPreenchimento.EmAndamento;

    /// <summary>Cria um preenchimento.</summary>
    public static Preenchimento Criar(
        int empresaId, int formularioId, int versaoFormulario, long preenchidoPorId) => new()
    {
        EmpresaId = empresaId,
        FormularioId = formularioId,
        VersaoFormulario = versaoFormulario,
        PreenchidoPorId = preenchidoPorId
    };
}

/// <summary>
/// O valor tipado de cada pergunta.
///
/// A resposta é TIPADA, com uma coluna por tipo e restrição de verificação garantindo que
/// exatamente a coluna certa esteja preenchida. Não é um campo de texto para tudo: isso é o
/// que devolveria uma data como texto e não ordenaria.
///
/// Quando a pergunta é sobre concorrente ou chassi, a resposta é chave estrangeira — [V] o
/// Vórtice guarda chassi como texto em 30 tabelas de formulário.
/// </summary>
public sealed class Resposta
{
    private Resposta() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O preenchimento a que a resposta pertence.</summary>
    public long PreenchimentoId { get; private set; }

    /// <summary>A pergunta respondida.</summary>
    public int PerguntaId { get; private set; }

    /// <summary>Valor de texto.</summary>
    public string? ValorTexto { get; private set; }

    /// <summary>Valor numérico.</summary>
    public decimal? ValorNumero { get; private set; }

    /// <summary>Valor de data e hora (UTC).</summary>
    public DateTime? ValorData { get; private set; }

    /// <summary>Valor de sim ou não.</summary>
    public bool? ValorBooleano { get; private set; }

    /// <summary>Item de catálogo escolhido.</summary>
    public int? CatalogoItemId { get; private set; }

    /// <summary>Cliente escolhido.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>Máquina escolhida.</summary>
    public long? EquipamentoId { get; private set; }

    /// <summary>
    /// A resposta estruturada, como JSON binário.
    ///
    /// Existe para o que não cabe numa coluna escalar — escolha múltipla de catálogo, matriz
    /// de itens, anexo com metadado. É a alternativa a criar uma tabela filha por formato de
    /// resposta, que é exatamente o caminho que produziu as 175 tabelas do legado.
    /// </summary>
    public string? ValorEstruturado { get; private set; }

    /// <summary>Quando a resposta foi gravada (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Responde uma pergunta com texto.</summary>
    public static Resposta DeTexto(long preenchimentoId, int perguntaId, string valor) => new()
    {
        PreenchimentoId = preenchimentoId,
        PerguntaId = perguntaId,
        ValorTexto = valor
    };

    /// <summary>Responde uma pergunta com um item de catálogo.</summary>
    public static Resposta DeCatalogo(long preenchimentoId, int perguntaId, int catalogoItemId) => new()
    {
        PreenchimentoId = preenchimentoId,
        PerguntaId = perguntaId,
        CatalogoItemId = catalogoItemId
    };
}
