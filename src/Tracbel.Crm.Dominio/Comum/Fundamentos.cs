namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Marca um evento de domínio: algo relevante que aconteceu e que outras partes do sistema
/// podem querer saber. É o ponto de extensão principal — ver <c>meta.ManipuladorEvento</c>
/// no documento 04 e a seção 5 do documento 03.
/// </summary>
public interface IEventoDominio
{
    /// <summary>Quando o fato ocorreu (UTC).</summary>
    DateTime OcorreuEm { get; }
}

/// <summary>
/// Uma regra de negócio foi violada.
///
/// IMPORTANTE — a distinção que o time precisa respeitar:
/// <list type="bullet">
///   <item>Erro de NEGÓCIO ("lead já qualificado") lança esta exceção dentro da entidade,
///   ou devolve <see cref="Resultado{T}"/> na camada de aplicação. Vira HTTP 409 ou 422.</item>
///   <item>Erro de PROGRAMAÇÃO (nulo inesperado, índice fora da faixa) lança as exceções
///   normais do .NET e vira HTTP 500.</item>
/// </list>
/// Nunca o inverso. [V] no Vórtice, a API devolve <c>{"Message":"An error has occurred."}</c>
/// tanto para telefone com DDI quanto para falha real — e ninguém consegue diagnosticar nada.
/// </summary>
public sealed class RegraDeNegocioViolada : Exception
{
    /// <summary>Cria a exceção com a mensagem que o usuário final vai ler.</summary>
    /// <param name="mensagem">Em português, dizendo o que houve E o que fazer.</param>
    public RegraDeNegocioViolada(string mensagem) : base(mensagem) { }
}

/// <summary>
/// A NATUREZA de uma falha de negócio — o que a camada de borda precisa saber para responder
/// sem inventar heurística.
///
/// Existe para que a API não tenha de adivinhar o código HTTP a partir do TEXTO da mensagem.
/// [V] É a diferença medida contra o legado, cuja API devolve
/// <c>{"Message":"An error has occurred."}</c> tanto para telefone com DDI quanto para falha
/// real de infraestrutura: quem chama não consegue distinguir "corrija o campo" de
/// "tente de novo mais tarde".
/// </summary>
public enum TipoDeFalha
{
    /// <summary>Não houve falha.</summary>
    Nenhuma = 0,

    /// <summary>O que veio na entrada não serve. Quem chamou corrige e repete.</summary>
    Validacao = 1,

    /// <summary>O registro pedido não existe, ou não está ao alcance de quem pediu.</summary>
    NaoEncontrado = 2,

    /// <summary>O estado atual do registro não admite a operação.</summary>
    Conflito = 3,

    /// <summary>Outra pessoa alterou o registro antes. Recarregue e refaça.</summary>
    Concorrencia = 4,

    /// <summary>Um sistema de fora não respondeu. Não é culpa de quem chamou.</summary>
    DependenciaIndisponivel = 5,

    /// <summary>
    /// Quem pediu é conhecido, o pedido é válido, e o perfil dele não alcança o que pediu.
    ///
    /// <para>Não é <see cref="NaoEncontrado"/>: aqui não há registro a esconder — é uma visão
    /// inteira (por exemplo, a empresa consolidada) que o perfil não abre, e dizer isso é o que
    /// deixa a tela explicar o botão desligado.</para>
    /// </summary>
    SemPermissao = 6
}

/// <summary>
/// O que há de errado com UM campo — o que a tela precisa para acender o campo certo.
///
/// A regra que este tipo existe para cumprir: mensagem de erro diz O QUE CORRIGIR, campo a
/// campo. "Erro ao salvar" não é resposta.
/// </summary>
/// <param name="Campo">O nome do campo como ele chega na requisição. Ex.: <c>documento</c>.</param>
/// <param name="Mensagem">O que fazer, em português.</param>
/// <param name="ValorRecebido">O que chegou, para o usuário reconhecer o que digitou.</param>
public sealed record ErroDeCampo(string Campo, string Mensagem, string? ValorRecebido = null);

/// <summary>
/// Resultado de uma operação que pode falhar por motivo de negócio, sem usar exceção
/// para controle de fluxo.
/// </summary>
/// <typeparam name="T">O tipo devolvido em caso de sucesso.</typeparam>
public readonly struct Resultado<T>
{
    private readonly T? _valor;
    private readonly IReadOnlyList<ErroDeCampo>? _erros;

    private Resultado(bool sucesso, T? valor, string? erro, TipoDeFalha tipo, IReadOnlyList<ErroDeCampo>? erros)
    {
        EhSucesso = sucesso;
        _valor = valor;
        Erro = erro;
        Tipo = tipo;
        _erros = erros;
    }

    /// <summary>Verdadeiro quando a operação deu certo.</summary>
    public bool EhSucesso { get; }

    /// <summary>Mensagem de erro, em português, quando a operação falhou.</summary>
    public string? Erro { get; }

    /// <summary>A natureza da falha. <see cref="TipoDeFalha.Nenhuma"/> quando deu certo.</summary>
    public TipoDeFalha Tipo { get; }

    /// <summary>
    /// O detalhe campo a campo. Vazio quando a falha não é de campo — nunca nulo, para que
    /// quem consome não precise checar duas coisas.
    /// </summary>
    public IReadOnlyList<ErroDeCampo> Erros => _erros ?? [];

    /// <summary>O valor produzido. Só acesse depois de conferir <see cref="EhSucesso"/>.</summary>
    /// <exception cref="InvalidOperationException">Se a operação falhou.</exception>
    public T Valor => EhSucesso
        ? _valor!
        : throw new InvalidOperationException(
            $"Tentativa de ler o valor de um resultado que falhou. Erro: {Erro}");

    /// <summary>Cria um resultado de sucesso.</summary>
    public static Resultado<T> Ok(T valor) => new(true, valor, null, TipoDeFalha.Nenhuma, null);

    /// <summary>
    /// Falha de negócio sem detalhe de campo. Tratada como <see cref="TipoDeFalha.Validacao"/>,
    /// que é o palpite seguro: manda quem chamou corrigir a entrada em vez de sugerir que o
    /// servidor quebrou.
    /// </summary>
    public static Resultado<T> Falha(string erro) => new(false, default, erro, TipoDeFalha.Validacao, null);

    /// <summary>Falha de entrada, com o que corrigir em cada campo.</summary>
    public static Resultado<T> FalhaDeValidacao(string erro, IReadOnlyList<ErroDeCampo> erros) =>
        new(false, default, erro, TipoDeFalha.Validacao, erros);

    /// <summary>O registro não existe — ou não está ao alcance de quem pediu, que dá no mesmo.</summary>
    public static Resultado<T> NaoEncontrado(string erro) =>
        new(false, default, erro, TipoDeFalha.NaoEncontrado, null);

    /// <summary>O estado atual não admite a operação.</summary>
    public static Resultado<T> Conflito(string erro, IReadOnlyList<ErroDeCampo>? erros = null) =>
        new(false, default, erro, TipoDeFalha.Conflito, erros);

    /// <summary>Alguém alterou o registro antes. Recarregue e refaça.</summary>
    public static Resultado<T> Concorrencia(string erro) =>
        new(false, default, erro, TipoDeFalha.Concorrencia, null);

    /// <summary>Um sistema de fora não respondeu. O resto da aplicação segue de pé.</summary>
    public static Resultado<T> Indisponivel(string erro) =>
        new(false, default, erro, TipoDeFalha.DependenciaIndisponivel, null);

    /// <summary>O perfil de quem pediu não alcança o que foi pedido.</summary>
    public static Resultado<T> SemPermissao(string erro) =>
        new(false, default, erro, TipoDeFalha.SemPermissao, null);

    /// <summary>
    /// Sem permissão, dizendo qual entrada causou a recusa — a filial escolhida, por exemplo. A tela usa o
    /// campo para voltar sozinha à filial de casa, em vez de ficar presa num 403 a cada chamada.
    /// </summary>
    public static Resultado<T> SemPermissao(string erro, IReadOnlyList<ErroDeCampo> erros) =>
        new(false, default, erro, TipoDeFalha.SemPermissao, erros);
}

/// <summary>
/// Fonte de tempo do sistema.
///
/// Existe para que o teste consiga fixar "agora". Chamar <c>DateTime.UtcNow</c> direto
/// dentro da regra torna o comportamento impossível de testar de forma determinística.
/// </summary>
public interface IRelogio
{
    /// <summary>O instante atual, em UTC.</summary>
    DateTime Agora { get; }
}

/// <summary>Relógio de produção. Devolve a hora real da máquina, em UTC.</summary>
public sealed class RelogioDoSistema : IRelogio
{
    /// <inheritdoc />
    public DateTime Agora => DateTime.UtcNow;
}
