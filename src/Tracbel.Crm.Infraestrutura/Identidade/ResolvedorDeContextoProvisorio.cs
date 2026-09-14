using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Infraestrutura.Identidade;

/// <summary>
/// Como o contexto de acesso é montado HOJE, enquanto o Entra ID não entra.
///
/// ISTO É DÍVIDA NOMEADA, e está escrita aqui para ninguém confundir com desenho definitivo.
/// A fase 0 do documento 13 é a autenticação; até ela chegar, a identidade vem de dois
/// cabeçalhos HTTP, e cabeçalho é uma coisa que qualquer um escreve. Ou seja: <b>isto não
/// autentica ninguém</b>. Serve para o front ser construído contra dado real e para a fronteira
/// de multiempresa ser exercitada de verdade — não para proteger nada.
/// </summary>
/// <remarks>
/// <para><b>O que já é definitivo e não muda com o Entra ID:</b> tudo o que vem DEPOIS deste
/// arquivo. O <see cref="ContextoAcesso"/> montado aqui é exatamente o mesmo objeto que a
/// autenticação real vai montar; o filtro global, as profundidades e a via de escape já
/// consomem dele. A troca é de UMA classe.</para>
///
/// <para><b>O ponto de troca:</b> quando a autenticação entrar, nasce um
/// <c>ResolvedorDeContextoDoEntraId</c> nesta mesma pasta, lendo as reivindicações do token em
/// vez dos cabeçalhos, e o registro no <c>Program.cs</c> troca de nome. Nada mais.</para>
///
/// <para><b>As três amarras que impedem esta ponte de virar permanente por descuido:</b></para>
/// <list type="number">
///   <item>fora de Desenvolvimento, não existe valor padrão: sem cabeçalho, a requisição é
///   recusada — a API não "assume alguém";</item>
///   <item>toda requisição que cai no valor padrão sai no log como aviso, com o texto
///   <c>PROVISÓRIO</c>, e a subida da API loga o mesmo aviso uma vez;</item>
///   <item>por conta própria, a ponte concede só a lista mínima, em
///   <see cref="Profundidade.EmpresaEAbaixo"/>. A permissão que abre a fronteira entre filiais só
///   chega por concessão EXPLÍCITA gravada em <c>seguranca.UsuarioConjuntoPermissao</c>, e a ponte só
///   a honra em Desenvolvimento (<see cref="OpcoesDeContextoProvisorio.HonrarConcessoesExplicitas"/>).
///   Fora dele, alcance de organização exige identidade provada pelo Entra ID.</item>
/// </list>
/// </remarks>
public sealed class ResolvedorDeContextoProvisorio(
    DbContextOptions<CrmDbContext> opcoesDoBanco,
    IOptions<OpcoesDeContextoProvisorio> opcoes,
    ILogger<ResolvedorDeContextoProvisorio> log)
{
    /// <summary>
    /// Resolve quem está agindo, a partir do que veio nos cabeçalhos.
    ///
    /// A CONSULTA DE IDENTIDADE RODA SOB CONTEXTO DE SISTEMA, e isso não é um furo: é a única
    /// ordem possível. O escopo de acesso é montado LENDO <c>seguranca.Usuario</c> e
    /// <c>organizacao.Empresa</c>, então ele ainda não existe no momento desta leitura — é
    /// exatamente a justificativa já registrada em
    /// <c>CrmDbContext.FronteiraDeEmpresaJustificada</c> para o cadastro de usuário.
    /// </summary>
    /// <param name="usuarioInformado">O nome principal (UPN) que veio no cabeçalho.</param>
    /// <param name="empresaInformada">O código da filial que veio no cabeçalho. Ex.: 010101.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ContextoAcesso>> ResolverAsync(
        string? usuarioInformado, string? empresaInformada, CancellationToken ct)
    {
        var config = opcoes.Value;

        var upn = Escolher(usuarioInformado, config.UsuarioPadrao, config.PermitirPadrao, out var usouPadraoDeUsuario);
        var filial = Escolher(empresaInformada, config.EmpresaPadrao, config.PermitirPadrao, out var usouPadraoDeEmpresa);

        var erros = new List<ErroDeCampo>();

        if (string.IsNullOrWhiteSpace(upn))
            erros.Add(new ErroDeCampo(
                config.CabecalhoDeUsuario,
                "Informe o nome principal do usuário neste cabeçalho. " +
                "Enquanto a autenticação pelo Entra ID não entra, é assim que a API sabe quem está agindo."));

        if (string.IsNullOrWhiteSpace(filial))
            erros.Add(new ErroDeCampo(
                config.CabecalhoDeEmpresa,
                "Informe o código da filial neste cabeçalho. Ex.: 010101. " +
                "Consulte /api/v1/catalogos/EMPRESA para ver a lista."));

        if (erros.Count > 0)
            return Resultado<ContextoAcesso>.FalhaDeValidacao(
                "A API não sabe quem está chamando.", erros);

        if (usouPadraoDeUsuario || usouPadraoDeEmpresa)
            log.LogWarning(
                "PROVISÓRIO: requisição sem cabeçalho de identidade caiu no valor padrão de " +
                "desenvolvimento (usuário {Usuario}, filial {Filial}). Isto NÃO autentica " +
                "ninguém e não existe fora de Desenvolvimento — ver ResolvedorDeContextoProvisorio.",
                upn, filial);

        await using var banco = new CrmDbContext(opcoesDoBanco, ProvedorDeContextoDeSistema.Instancia);

        var usuario = await banco.Usuarios
            .Where(u => u.NomePrincipal == upn && u.EstaAtivo && u.ExcluidoEm == null)
            .Select(u => new { u.Id, u.NomeExibicao })
            .FirstOrDefaultAsync(ct);

        if (usuario is null)
            return Resultado<ContextoAcesso>.FalhaDeValidacao(
                "A API não reconhece este usuário.",
                [new ErroDeCampo(
                    config.CabecalhoDeUsuario,
                    "Não há usuário ativo com este nome principal em seguranca.Usuario.",
                    upn)]);

        // O ESCOPO É O MESMO do login pelo Entra ID, montado no mesmo lugar — ver EscopoDeAcesso.
        return await EscopoDeAcesso.MontarAsync(
            banco, usuario.Id, usuario.NomeExibicao, filial, empresaDeCasaId: 0,
            config.CabecalhoDeEmpresa, config.HonrarConcessoesExplicitas, ct);
    }

    private static string? Escolher(string? informado, string? padrao, bool permitePadrao, out bool usouPadrao)
    {
        usouPadrao = false;

        if (!string.IsNullOrWhiteSpace(informado)) return informado.Trim();
        if (!permitePadrao || string.IsNullOrWhiteSpace(padrao)) return null;

        usouPadrao = true;
        return padrao.Trim();
    }
}

/// <summary>
/// O contexto de SISTEMA — usado pela migração, pelo design time do <c>dotnet ef</c> e pela
/// consulta de identidade acima, que precisa rodar antes de existir escopo nenhum.
///
/// Ele enxerga tudo, e é por isso que existe um só, aqui, nomeado. [V] no legado o equivalente
/// é o acesso direto ao banco, que também enxerga tudo — a diferença é que lá ele não deixa
/// rastro e não tem nome.
/// </summary>
public sealed class ProvedorDeContextoDeSistema : IProvedorContextoAcesso
{
    /// <summary>A instância única. Não guarda estado.</summary>
    public static ProvedorDeContextoDeSistema Instancia { get; } = new();

    /// <inheritdoc />
    public ContextoAcesso Atual { get; } = new(
        usuarioId: 0,
        nomeExibicao: "sistema",
        empresaId: 0,
        empresasVisiveis: new HashSet<int>(),
        subordinadosIds: new HashSet<long>(),
        equipesIds: new HashSet<long>(),
        profundidades: new Dictionary<string, Profundidade>(),
        ehServicoDeSistema: true);
}
