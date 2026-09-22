using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>
/// UM PERFIL DE ACESSO — um conjunto nomeado de permissões, cada uma com a profundidade em que vale
/// (a CAMADA 2 do documento 05).
///
/// <para><b>Era <c>ConjuntoPermissao</c> até a fase 3</b> (documento 41; matriz de nomenclatura 41B):
/// "perfil" é o nome que o negócio usa, e a tabela estava vazia — renomear custou uma migração e
/// nenhuma transformação de dado.</para>
///
/// <para>[SF] Permission Set: aditivo e componível. Um gerente recebe <c>PADRAO</c> +
/// <c>EXCLUSAO_DE_CADASTRO</c>, e vence a permissão mais permissiva.</para>
///
/// <para>[V] NÃO EXISTE "usuário com permissão avulsa". Toda concessão passa por um perfil. É
/// exatamente a permissão avulsa que produziu, no Vórtice, <b>370 combinações distintas de 51 flags
/// para 939 usuários</b> — cada pessoa virou um floco de neve, e ninguém consegue auditar nem
/// responder "o que um vendedor pode fazer?".</para>
///
/// <para><b>O perfil padrão</b> (<see cref="EhPadrao"/>) é o que todo usuário recebe sem concessão
/// nenhuma. Substitui a lista fixa que morava em código até a fase 3 (Q-P2, 21/09/2026).</para>
/// </summary>
public sealed class Perfil
{
    private readonly List<PerfilPermissao> _permissoes = [];

    private Perfil() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável. Ex.: <c>EXCLUSAO_DE_CADASTRO</c>.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve, em português.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Desligar sem apagar, preservando o histórico de quem já teve.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>
    /// É o perfil que todo usuário recebe? Um só pode ser — o banco garante com índice único filtrado.
    /// </summary>
    public bool EhPadrao { get; private set; }

    /// <summary>As permissões do perfil, cada uma com sua profundidade.</summary>
    public IReadOnlyCollection<PerfilPermissao> Permissoes => _permissoes.AsReadOnly();

    /// <summary>Cria um perfil vazio.</summary>
    /// <param name="codigo">O código estável.</param>
    /// <param name="nome">O nome legível.</param>
    /// <param name="descricao">Para que serve.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o código ou o nome faltam.</exception>
    public static Perfil Criar(string codigo, string nome, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new RegraDeNegocioViolada("O perfil precisa de código.");
        if (string.IsNullOrWhiteSpace(nome)) throw new RegraDeNegocioViolada("O perfil precisa de nome.");

        return new Perfil { Codigo = codigo.Trim(), Nome = nome.Trim(), Descricao = descricao?.Trim() };
    }

    /// <summary>
    /// Concede uma permissão ao perfil, com a profundidade que ela alcança.
    ///
    /// <para>Conceder duas vezes a mesma permissão mantém a MAIOR profundidade — a regra é aditiva. E só
    /// se concede o que existe no catálogo (<see cref="Seguranca.Permissoes"/>): um código digitado
    /// errado viraria uma permissão que nenhuma rota confere, e o administrador acharia que concedeu
    /// alguma coisa.</para>
    /// </summary>
    /// <param name="codigoPermissao">A permissão, do catálogo.</param>
    /// <param name="profundidade">Até onde alcança.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a permissão não existe ou a profundidade é Nenhum.</exception>
    public Perfil Conceder(string codigoPermissao, Profundidade profundidade)
    {
        if (!Seguranca.Permissoes.Existe(codigoPermissao))
            throw new RegraDeNegocioViolada(
                $"A permissão '{codigoPermissao}' não existe no catálogo. Veja Permissoes.Catalogo.");

        if (profundidade == Profundidade.Nenhum)
            throw new RegraDeNegocioViolada(
                $"Não faz sentido conceder '{codigoPermissao}' com profundidade Nenhum. " +
                "Para negar, simplesmente não conceda — o modelo não tem regra de negação.");

        var existente = _permissoes.FirstOrDefault(i => i.CodigoPermissao == codigoPermissao);

        if (existente is null)
            _permissoes.Add(PerfilPermissao.Criar(codigoPermissao, profundidade));
        else if (profundidade > existente.Profundidade)
            existente.AmpliarPara(profundidade);

        return this;
    }

    /// <summary>Troca o nome e a descrição (issue 113).</summary>
    /// <param name="nome">O nome legível.</param>
    /// <param name="descricao">Para que serve.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o nome falta.</exception>
    public void Renomear(string nome, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new RegraDeNegocioViolada("O perfil precisa de nome.");
        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
    }

    /// <summary>
    /// DEFINE O CONJUNTO INTEIRO de permissões (issue 113) — o que o editor da tela manda. O que não está na lista
    /// sai; o que está entra ou muda de profundidade, para mais ou para menos.
    ///
    /// <para>Diferente de <see cref="Conceder"/>, que só soma: aqui a tela diz o estado final, e reduzir a
    /// profundidade é parte do trabalho de quem administra.</para>
    /// </summary>
    /// <param name="permissoes">O conjunto final, cada permissão do catálogo com a profundidade.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando uma permissão não existe, repete, ou vem com Nenhum.</exception>
    public void DefinirPermissoes(IReadOnlyCollection<(string Codigo, Profundidade Profundidade)> permissoes)
    {
        var repetidas = permissoes.GroupBy(p => p.Codigo, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (repetidas.Count > 0)
            throw new RegraDeNegocioViolada($"A permissão aparece mais de uma vez: {string.Join(", ", repetidas)}.");

        foreach (var (codigo, profundidade) in permissoes)
        {
            if (!Seguranca.Permissoes.Existe(codigo))
                throw new RegraDeNegocioViolada($"A permissão '{codigo}' não existe no catálogo.");
            if (profundidade == Profundidade.Nenhum)
                throw new RegraDeNegocioViolada($"'{codigo}' com profundidade Nenhum: para não dar a permissão, deixe-a fora da lista.");
        }

        var finais = permissoes.ToDictionary(p => p.Codigo, p => p.Profundidade, StringComparer.Ordinal);
        _permissoes.RemoveAll(i => !finais.ContainsKey(i.CodigoPermissao));

        foreach (var (codigo, profundidade) in finais)
        {
            var existente = _permissoes.FirstOrDefault(i => i.CodigoPermissao == codigo);
            if (existente is null) _permissoes.Add(PerfilPermissao.Criar(codigo, profundidade));
            else existente.DefinirProfundidade(profundidade);
        }
    }

    /// <summary>Religa o perfil desligado; quem tem a concessão volta a ter as permissões dele.</summary>
    public void Reativar() => EstaAtivo = true;

    /// <summary>Desliga o perfil sem apagá-lo. O perfil padrão não se desliga: todo mundo perderia o acesso.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando é o perfil padrão.</exception>
    public void Desativar()
    {
        if (EhPadrao)
            throw new RegraDeNegocioViolada(
                "O perfil padrão não pode ser desativado: é o que todo usuário recebe, e desligá-lo " +
                "trancaria o acesso de todos. Altere as permissões dele em vez disso.");

        EstaAtivo = false;
    }
}

/// <summary>Uma permissão dentro de um perfil, com a profundidade que ela alcança.</summary>
public sealed class PerfilPermissao
{
    private PerfilPermissao() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O perfil a que pertence.</summary>
    public int PerfilId { get; private set; }

    /// <summary>A permissão concedida, do catálogo em código.</summary>
    public string CodigoPermissao { get; private set; } = default!;

    /// <summary>Até onde alcança.</summary>
    public Profundidade Profundidade { get; private set; }

    internal static PerfilPermissao Criar(string codigoPermissao, Profundidade profundidade) => new()
    {
        CodigoPermissao = codigoPermissao,
        Profundidade = profundidade
    };

    internal void AmpliarPara(Profundidade nova) => Profundidade = nova;

    internal void DefinirProfundidade(Profundidade nova)
    {
        if (Profundidade != nova) Profundidade = nova;
    }
}

/// <summary>
/// A CONCESSÃO DE UM PERFIL A UM USUÁRIO — o único caminho para alguém ter mais do que o perfil padrão.
///
/// <para><b>A filial da concessão (P-20, 21/09/2026).</b> Uma concessão pode valer numa filial só
/// (<see cref="EmpresaId"/>). É isso que responde "quais filiais esta pessoa pode escolher": a de casa
/// e aquelas em que ela tem um perfil concedido; qualquer outra, 403. É o modelo do Dataverse — perfil
/// por unidade de negócio —, que o documento 05 já tomava como referência. Concessão sem filial vale em
/// qualquer filial que ela possa escolher.</para>
///
/// <para><see cref="ExpiraEm"/> cobre férias e substituição sem que alguém esqueça de remover depois —
/// que é como privilégio temporário vira permanente. <see cref="Justificativa"/> é obrigatória: a
/// concessão a pessoa real é operação registrada, com autorização explícita (R-16 do documento 46).</para>
/// </summary>
public sealed class UsuarioPerfil
{
    private UsuarioPerfil() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A quem foi concedido.</summary>
    public long UsuarioId { get; private set; }

    /// <summary>O perfil concedido.</summary>
    public int PerfilId { get; private set; }

    /// <summary>A filial em que a concessão vale; nulo vale em qualquer filial que o usuário possa escolher.</summary>
    public int? EmpresaId { get; private set; }

    /// <summary>Por que foi concedido, e por autorização de quem.</summary>
    public string Justificativa { get; private set; } = default!;

    /// <summary>Quando (UTC).</summary>
    public DateTime ConcedidoEm { get; private set; }

    /// <summary>Quem concedeu. Auditado permanentemente.</summary>
    public long ConcedidoPorId { get; private set; }

    /// <summary>Quando expira. Nulo = permanente.</summary>
    public DateTime? ExpiraEm { get; private set; }

    /// <summary>
    /// Quando foi revogada (UTC); nula enquanto vale (issue 113).
    ///
    /// <para><b>Revogar não apaga a linha.</b> Apagar levaria junto quem concedeu, por quê e até quando — e a
    /// pergunta "quem tinha acesso de administrador em março?" ficaria sem resposta. A linha fica, marcada, e
    /// deixa de valer.</para>
    /// </summary>
    public DateTime? RevogadaEm { get; private set; }

    /// <summary>Quem revogou.</summary>
    public long? RevogadaPorId { get; private set; }

    /// <summary>Por que foi revogada — obrigatório, como a justificativa da concessão.</summary>
    public string? MotivoDaRevogacao { get; private set; }

    /// <summary>Se foi revogada.</summary>
    public bool Revogada => RevogadaEm is not null;

    /// <summary>Concede um perfil a um usuário.</summary>
    /// <param name="usuarioId">A quem.</param>
    /// <param name="perfilId">O perfil.</param>
    /// <param name="justificativa">Por que, e por autorização de quem.</param>
    /// <param name="concedidoPorId">Quem concedeu.</param>
    /// <param name="agoraUtc">O instante da concessão.</param>
    /// <param name="empresaId">A filial em que vale, ou nulo para qualquer uma.</param>
    /// <param name="expiraEm">Quando expira, ou nulo para permanente.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta a justificativa ou a expiração já passou.</exception>
    public static UsuarioPerfil Conceder(
        long usuarioId, int perfilId, string justificativa, long concedidoPorId, DateTime agoraUtc,
        int? empresaId = null, DateTime? expiraEm = null)
    {
        if (string.IsNullOrWhiteSpace(justificativa))
            throw new RegraDeNegocioViolada(
                "A concessão de perfil precisa de justificativa: por que, e por autorização de quem.");

        if (expiraEm is not null && expiraEm <= agoraUtc)
            throw new RegraDeNegocioViolada("A data de expiração precisa estar no futuro.");

        return new UsuarioPerfil
        {
            UsuarioId = usuarioId,
            PerfilId = perfilId,
            EmpresaId = empresaId,
            Justificativa = justificativa.Trim(),
            ConcedidoEm = agoraUtc,
            ConcedidoPorId = concedidoPorId,
            ExpiraEm = expiraEm
        };
    }

    /// <summary>Verdadeiro se a concessão vale neste instante: não foi revogada e não expirou.</summary>
    /// <param name="agora">O instante (UTC).</param>
    public bool EstaVigente(DateTime agora) => RevogadaEm is null && (ExpiraEm is null || ExpiraEm > agora);

    /// <summary>Revoga a concessão, com motivo. A linha fica, para o histórico.</summary>
    /// <param name="porId">Quem revoga.</param>
    /// <param name="motivo">Por que — e por autorização de quem.</param>
    /// <param name="agoraUtc">O instante.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando já foi revogada ou falta o motivo.</exception>
    public void Revogar(long porId, string motivo, DateTime agoraUtc)
    {
        if (Revogada)
            throw new RegraDeNegocioViolada("Esta concessão já foi revogada.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new RegraDeNegocioViolada("A revogação precisa de motivo: por que, e por autorização de quem.");

        RevogadaEm = agoraUtc;
        RevogadaPorId = porId;
        MotivoDaRevogacao = motivo.Trim();
    }
}

/// <summary>
/// QUEM PODE CONCEDER O QUÊ — a regra que impede a escalada (issue 113).
///
/// <para>Conceder um perfil é dar as permissões dele. Quem concede só pode dar o que já tem, na mesma
/// profundidade ou maior: senão, uma conta com <c>Usuario.Administrar</c> e mais nada concederia a si — ou a
/// um colega combinado — o perfil Administrador inteiro. E ninguém concede a si mesmo, nem com o que já tem:
/// toda concessão passa por uma segunda pessoa.</para>
/// </summary>
public static class RegraDeConcessao
{
    /// <summary>
    /// As permissões do perfil que quem concede NÃO tem na profundidade exigida. Lista vazia: pode conceder.
    /// </summary>
    /// <param name="quemConcede">O contexto de quem concede.</param>
    /// <param name="permissoesDoPerfil">O que o perfil dá.</param>
    public static IReadOnlyList<string> FaltasParaConceder(
        ContextoAcesso quemConcede, IEnumerable<(string Codigo, Profundidade Profundidade)> permissoesDoPerfil) =>
        permissoesDoPerfil
            .Where(p => quemConcede.ProfundidadeDe(p.Codigo) < p.Profundidade)
            .Select(p => p.Codigo)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
}
