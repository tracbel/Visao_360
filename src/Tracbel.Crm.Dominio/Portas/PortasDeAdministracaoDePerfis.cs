using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Um perfil na tela de Perfis (issue 113, parte 2b).</summary>
/// <param name="Codigo">O código estável.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Descricao">Para que serve.</param>
/// <param name="EhPadrao">Se é o que todo usuário recebe.</param>
/// <param name="EhDoSistema">Se vem do código — fixo na tela; para ajustar, duplica-se.</param>
/// <param name="EstaAtivo">Se vale.</param>
/// <param name="PessoasComOPerfil">Quantas contas ativas têm uma concessão vigente dele.</param>
/// <param name="Permissoes">O que ele dá, com a profundidade.</param>
public sealed record PerfilNaAdministracao(
    string Codigo, string Nome, string? Descricao, bool EhPadrao, bool EhDoSistema, bool EstaAtivo,
    int PessoasComOPerfil, IReadOnlyList<PermissaoDoEscopo> Permissoes);

/// <summary>A leitura e a gravação dos perfis pela administração (issue 113, parte 2b).</summary>
public interface IRepositorioDePerfisDaAdministracao
{
    /// <summary>Todos os perfis, inclusive os desativados, com as permissões e quantas pessoas têm cada um.</summary>
    Task<IReadOnlyList<PerfilNaAdministracao>> ListarTodosAsync(DateTime agoraUtc, CancellationToken ct);

    /// <summary>O perfil, rastreado e com as permissões, para alterar; nulo quando não existe.</summary>
    Task<Perfil?> ObterParaAlterarAsync(string codigo, CancellationToken ct);

    /// <summary>Se já existe perfil com este código (a caixa não importa).</summary>
    Task<bool> CodigoEmUsoAsync(string codigo, CancellationToken ct);

    /// <summary>Põe o perfil novo na unidade de trabalho.</summary>
    Task AdicionarAsync(Perfil perfil, CancellationToken ct);
}
