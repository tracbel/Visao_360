using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// GUARDA E ABRE A CREDENCIAL DAS CONEXÕES (issue 136).
///
/// <para>No servidor, a proteção é a do Windows para a máquina (DPAPI): o que foi protegido lá só abre lá. Uma cópia
/// do banco restaurada em outra máquina traz as conexões sem as senhas — é de propósito, e a tela pede para digitar
/// de novo.</para>
/// </summary>
public interface IProtetorDeSegredos
{
    /// <summary>Protege o segredo para guardar no banco.</summary>
    byte[] Proteger(string segredo);

    /// <summary>Abre o segredo. Lança <see cref="InvalidOperationException"/> quando ele não abre nesta máquina.</summary>
    string Revelar(byte[] protegido);
}

/// <summary>De onde vem a credencial que a conexão usa.</summary>
public enum OrigemDaCredencial
{
    /// <summary>A conexão não tem credencial (fonte pública, API monitorada sem segredo).</summary>
    NaoSeAplica = 0,

    /// <summary>Gravada pela tela, protegida no banco.</summary>
    Tela = 1,

    /// <summary>Das variáveis de ambiente do servidor — a reserva de antes da tela.</summary>
    Ambiente = 2,

    /// <summary>Nem uma nem outra: a conexão não está configurada.</summary>
    Nenhuma = 3
}

/// <summary>
/// A CONEXÃO PRONTA PARA USAR OU TESTAR, com a credencial aberta — só em memória, no servidor, pelo tempo da
/// chamada. O <see cref="ToString"/> não mostra o segredo: um registro que cai num log não leva a senha junto.
/// </summary>
/// <param name="Codigo">O código da conexão.</param>
/// <param name="Tipo">Como se fala com ela.</param>
/// <param name="Origem">De onde veio a credencial.</param>
/// <param name="Endereco">A URL ou o servidor.</param>
/// <param name="Porta">A porta, quando há.</param>
/// <param name="Banco">O banco.</param>
/// <param name="Objeto">A visão lida.</param>
/// <param name="Usuario">O usuário.</param>
/// <param name="Segredo">A senha ou o segredo do cabeçalho, aberto.</param>
/// <param name="NomeDoCabecalho">O cabeçalho do segredo, na monitorada.</param>
/// <param name="StatusEsperado">O status que conta como "no ar", na monitorada.</param>
/// <param name="CadeiaDeConexao">A cadeia de conexão pronta, quando veio assim do ambiente (o Vórtice).</param>
public sealed record ConexaoResolvida(
    string Codigo, TipoDeConexao Tipo, OrigemDaCredencial Origem, string? Endereco, int? Porta, string? Banco, string? Objeto,
    string? Usuario, string? Segredo, string? NomeDoCabecalho, int StatusEsperado, string? CadeiaDeConexao)
{
    /// <summary>Sem segredo, sem usuário e sem cadeia de conexão.</summary>
    public override string ToString() => $"ConexaoResolvida {{ Codigo = {Codigo}, Tipo = {Tipo}, Origem = {Origem} }}";
}

/// <summary>O que um teste de conexão respondeu.</summary>
/// <param name="Ok">Se passou.</param>
/// <param name="Resumo">O que respondeu, sem credencial.</param>
/// <param name="LatenciaMs">Quanto levou.</param>
public sealed record ResultadoDoTesteDeConexao(bool Ok, string Resumo, int LatenciaMs);

/// <summary>
/// O BOTÃO "TESTAR" — executado no servidor, SÓ LEITURA: no Protheus, o token e uma linha; nos bancos, uma consulta
/// em sessão somente leitura; nas fontes e nas APIs monitoradas, um GET que lê só o cabeçalho da resposta.
/// </summary>
public interface ITestadorDeConexoes
{
    /// <summary>Testa a conexão. Não lança: a falha volta no resultado, sem credencial.</summary>
    Task<ResultadoDoTesteDeConexao> TestarAsync(ConexaoResolvida conexao, CancellationToken ct);
}

/// <summary>Decide que credencial a conexão usa: a da tela, se houver; senão a do ambiente do servidor.</summary>
public interface IResolvedorDeConexoes
{
    /// <summary>A conexão com a credencial aberta. Lança <see cref="InvalidOperationException"/> quando a da tela não abre.</summary>
    ConexaoResolvida Resolver(Conexao conexao);

    /// <summary>De onde viria a credencial — sem abrir nada.</summary>
    OrigemDaCredencial OrigemDe(Conexao conexao);
}

/// <summary>As conexões, para a tela e para o botão "Testar".</summary>
public interface IRepositorioDeConexoes
{
    /// <summary>Todas, as do sistema primeiro, na ordem do catálogo; as cadastradas pela tela pelo nome.</summary>
    Task<IReadOnlyList<Conexao>> ListarAsync(CancellationToken ct);

    /// <summary>A conexão, rastreada, para alterar; nula quando não existe.</summary>
    Task<Conexao?> ObterParaAlterarAsync(string codigo, CancellationToken ct);

    /// <summary>Põe uma conexão nova na unidade de trabalho.</summary>
    Task AdicionarAsync(Conexao conexao, CancellationToken ct);

    /// <summary>Põe o registro de um teste na unidade de trabalho.</summary>
    Task AdicionarVerificacaoAsync(VerificacaoDeConexao verificacao, CancellationToken ct);
}

/// <summary>As rotinas, para a tela.</summary>
public interface IRepositorioDeRotinas
{
    /// <summary>Todas, na ordem do catálogo.</summary>
    Task<IReadOnlyList<Rotina>> ListarAsync(CancellationToken ct);

    /// <summary>A rotina, rastreada, para alterar; nula quando não existe.</summary>
    Task<Rotina?> ObterParaAlterarAsync(string codigo, CancellationToken ct);
}

/// <summary>Um teste, como o histórico da tela mostra.</summary>
public sealed record VerificacaoNaTela(DateTime VerificadaEm, string? VerificadaPor, bool Ok, string Resumo, int LatenciaMs);

/// <summary>Uma execução de rotina, como o histórico da tela mostra.</summary>
public sealed record ExecucaoNaTela(
    DateTime IniciadaEm, DateTime? TerminadaEm, string Motivo, string? PedidaPor, string Resultado, int? CodigoDeSaida, string? Mensagem, string Maquina);

/// <summary>O histórico dos testes e das execuções, e os nomes de quem aparece neles.</summary>
public interface IConsultaDoHistoricoDeIntegracoes
{
    /// <summary>Os testes mais recentes de uma conexão.</summary>
    Task<IReadOnlyList<VerificacaoNaTela>> ListarVerificacoesAsync(string codigoDaConexao, int quantos, CancellationToken ct);

    /// <summary>As execuções mais recentes de uma rotina.</summary>
    Task<IReadOnlyList<ExecucaoNaTela>> ListarExecucoesAsync(string codigoDaRotina, int quantos, CancellationToken ct);

    /// <summary>O nome de cada usuário.</summary>
    Task<IReadOnlyDictionary<long, string>> NomesDosUsuariosAsync(IReadOnlyCollection<long> ids, CancellationToken ct);
}
