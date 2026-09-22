using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Em que situação está a conta, para o filtro da lista (issue 113).</summary>
public enum SituacaoDaConta
{
    /// <summary>Ativa e liberada: entra no CRM.</summary>
    Ativa = 0,

    /// <summary>Nasceu no primeiro login e espera o administrador escolher a filial.</summary>
    AguardandoLiberacao = 1,

    /// <summary>Desativada: não entra, e nada dela foi apagado.</summary>
    Desativada = 2
}

/// <summary>O filtro da lista de usuários.</summary>
/// <param name="Situacao">A situação da conta.</param>
/// <param name="Termo">Trecho do nome ou do e-mail.</param>
/// <param name="Paginacao">A página pedida.</param>
public sealed record FiltroDeUsuarios(SituacaoDaConta Situacao, string? Termo, Paginacao Paginacao);

/// <summary>Um usuário na lista da administração.</summary>
/// <param name="Chave">A chave pública.</param>
/// <param name="Nome">O nome completo.</param>
/// <param name="NomePrincipal">O e-mail da conta Microsoft.</param>
/// <param name="Natureza">Pessoa, departamento, sistema, fornecedor ou teste.</param>
/// <param name="FilialCodigo">A filial de casa (a provisória, para quem espera liberação).</param>
/// <param name="FilialNome">O nome dela.</param>
/// <param name="EstaAtivo">Se a conta está ativa.</param>
/// <param name="AguardandoLiberacaoDesde">Desde quando espera (UTC); nulo se já foi liberada.</param>
/// <param name="UltimoLoginEm">Último acesso (UTC).</param>
/// <param name="Perfis">Os perfis concedidos e vigentes, pelo nome.</param>
public sealed record UsuarioNaAdministracao(
    Guid Chave, string Nome, string NomePrincipal, string Natureza, string FilialCodigo, string FilialNome,
    bool EstaAtivo, DateTime? AguardandoLiberacaoDesde, DateTime? UltimoLoginEm, IReadOnlyList<string> Perfis);

/// <summary>Uma concessão, com todo o histórico — inclusive a revogada e a vencida.</summary>
/// <param name="Id">O identificador da concessão.</param>
/// <param name="PerfilCodigo">O perfil.</param>
/// <param name="PerfilNome">O nome do perfil.</param>
/// <param name="FilialCodigo">A filial em que vale; nula vale em qualquer uma.</param>
/// <param name="FilialNome">O nome dela.</param>
/// <param name="Justificativa">Por que foi concedida.</param>
/// <param name="ConcedidaEm">Quando (UTC).</param>
/// <param name="ConcedidaPor">Quem concedeu.</param>
/// <param name="ExpiraEm">Até quando vale (UTC); nula é permanente.</param>
/// <param name="RevogadaEm">Quando foi revogada (UTC).</param>
/// <param name="RevogadaPor">Quem revogou.</param>
/// <param name="MotivoDaRevogacao">Por que foi revogada.</param>
/// <param name="Vigente">Se vale agora.</param>
public sealed record ConcessaoNaAdministracao(
    long Id, string PerfilCodigo, string PerfilNome, string? FilialCodigo, string? FilialNome,
    string Justificativa, DateTime ConcedidaEm, string ConcedidaPor, DateTime? ExpiraEm,
    DateTime? RevogadaEm, string? RevogadaPor, string? MotivoDaRevogacao, bool Vigente);

/// <summary>A conta e as concessões dela.</summary>
/// <param name="Usuario">A conta.</param>
/// <param name="Concessoes">Todas as concessões, as vigentes primeiro.</param>
public sealed record UsuarioDetalhadoNaAdministracao(UsuarioNaAdministracao Usuario, IReadOnlyList<ConcessaoNaAdministracao> Concessoes);

/// <summary>Um perfil que pode ser concedido, com o que ele dá.</summary>
/// <param name="Id">O identificador.</param>
/// <param name="Codigo">O código.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Descricao">Para que serve.</param>
/// <param name="EhPadrao">Se é o que todo usuário recebe (não se concede).</param>
/// <param name="Permissoes">As permissões e a profundidade de cada uma.</param>
public sealed record PerfilParaConceder(
    int Id, string Codigo, string Nome, string? Descricao, bool EhPadrao,
    IReadOnlyList<(string Codigo, Profundidade Profundidade)> Permissoes);

/// <summary>
/// A LEITURA da administração de usuários (issue 113): a lista e a conta com o histórico das concessões.
///
/// <para><c>Usuario</c> e <c>UsuarioPerfil</c> estão fora da fronteira de filial por desenho (são o que define
/// o escopo). Por isso o alcance vem de fora, em <c>filiais</c>: nulo lê todas; um conjunto lê só os usuários
/// com filial de casa nele — o <c>Usuario.Ler</c> em <see cref="Profundidade.EmpresaEAbaixo"/>.</para>
/// </summary>
public interface IRepositorioDeUsuariosDaAdministracao
{
    /// <summary>A lista, filtrada e paginada, dentro do alcance.</summary>
    Task<PaginaDe<UsuarioNaAdministracao>> ListarAsync(FiltroDeUsuarios filtro, IReadOnlySet<int>? filiais, DateTime agoraUtc, CancellationToken ct);

    /// <summary>A conta e todas as concessões, dentro do alcance; nula quando não existe ou está fora dele.</summary>
    Task<UsuarioDetalhadoNaAdministracao?> ObterAsync(Guid chave, IReadOnlySet<int>? filiais, DateTime agoraUtc, CancellationToken ct);
}

/// <summary>A GRAVAÇÃO: a conta e as concessões rastreadas, e a concessão nova. Quem grava é o <see cref="IUnidadeDeTrabalho"/>.</summary>
public interface IRepositorioDeConcessoes
{
    /// <summary>A conta, rastreada, para alterar.</summary>
    Task<Usuario?> ObterContaParaAlterarAsync(Guid chave, CancellationToken ct);

    /// <summary>As concessões da conta, rastreadas, para conceder e revogar.</summary>
    Task<IReadOnlyList<UsuarioPerfil>> ListarConcessoesParaAlterarAsync(long usuarioId, CancellationToken ct);

    /// <summary>Põe a concessão nova na unidade de trabalho.</summary>
    Task AdicionarConcessaoAsync(UsuarioPerfil concessao, CancellationToken ct);
}

/// <summary>AS REFERÊNCIAS da administração: os perfis que se concedem e as filiais que se escolhem.</summary>
public interface IRepositorioDeReferenciasDeAcesso
{
    /// <summary>Os perfis ativos, com as permissões.</summary>
    Task<IReadOnlyList<PerfilParaConceder>> ListarPerfisAsync(CancellationToken ct);

    /// <summary>A filial ativa pelo código; nula quando não existe ou está inativa.</summary>
    Task<(int Id, string Nome)?> ObterFilialAtivaAsync(string codigo, CancellationToken ct);
}