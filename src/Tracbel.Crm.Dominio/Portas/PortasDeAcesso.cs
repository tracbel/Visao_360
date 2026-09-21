using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>A leitura do escopo efetivo de quem está agindo (fase 3 do documento 41).</summary>
public interface IRepositorioDeEscopo
{
    /// <summary>A filial do contexto e as filiais que o usuário pode escolher.</summary>
    /// <param name="contexto">O contexto de acesso da requisição.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<(FilialDoEscopo Atual, IReadOnlyList<FilialDoEscopo> Permitidas)> FiliaisAsync(ContextoAcesso contexto, CancellationToken ct);
}

/// <summary>
/// O ESCOPO EFETIVO — "onde eu posso olhar e o que eu posso fazer aqui?".
///
/// <para>É a mitigação que o documento 41 pede para a fase de risco mais alto: se alguém perder um acesso
/// depois da fase 3, a resposta está numa rota, e não num chamado. É também o que o seletor de filial da
/// tela usa para listar só as filiais permitidas (P-20).</para>
/// </summary>
/// <param name="Usuario">O nome de quem pergunta.</param>
/// <param name="FilialAtual">A filial do contexto desta resposta.</param>
/// <param name="FilialPedidaRecusada">A filial que veio no cabeçalho e não é permitida; nulo quando não houve recusa.</param>
/// <param name="FiliaisPermitidas">As filiais que ele pode escolher.</param>
/// <param name="Permissoes">O que ele pode fazer na filial atual.</param>
public sealed record EscopoDoUsuario(
    string Usuario,
    FilialDoEscopo FilialAtual,
    string? FilialPedidaRecusada,
    IReadOnlyList<FilialDoEscopo> FiliaisPermitidas,
    IReadOnlyList<PermissaoDoEscopo> Permissoes);

/// <summary>Uma filial do escopo.</summary>
/// <param name="Codigo">O código (<c>010101</c>).</param>
/// <param name="Nome">O nome.</param>
/// <param name="EhCasa">Se é a filial de casa do usuário.</param>
public sealed record FilialDoEscopo(string Codigo, string Nome, bool EhCasa);

/// <summary>Uma permissão do escopo.</summary>
/// <param name="Codigo">O código.</param>
/// <param name="Descricao">O que ela deixa fazer.</param>
/// <param name="Profundidade">Até onde alcança.</param>
public sealed record PermissaoDoEscopo(string Codigo, string Descricao, string Profundidade);
