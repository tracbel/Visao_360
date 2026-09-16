using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Seguranca;

// O QUE SAIU DAQUI NA FASE 1 (documento 41): a tabela `seguranca.Permissao` — o catálogo de
// permissões nomeadas (entidade + verbo). Ela nunca recebeu uma linha: o catálogo que o sistema de
// fato usa é a lista fechada de `EscopoDeAcesso.PermissoesConcedidas`, em código, conferida em
// tempo de compilação. Uma tabela vazia ao lado dela só criava a dúvida sobre qual das duas manda.
//
// O CÓDIGO da permissão continua sendo texto (`ItemConjuntoPermissao.CodigoPermissao` logo abaixo),
// exatamente como antes — nada no fluxo de autorização lia a tabela. Quando existir a tela de
// administração que edita o catálogo, ele volta pelo desenho do documento 40, na fase de identidade.

/// <summary>
/// Um conjunto de permissões — a CAMADA 2.
///
/// [SF] Permission Set: aditivo e componível. Um gerente recebe <c>CEN</c> + <c>GERENTE_VENDAS</c>,
/// e vence a permissão mais permissiva.
///
/// [V] NÃO EXISTE "usuário com permissão avulsa". Toda concessão passa por um conjunto.
/// É exatamente a permissão avulsa que produziu, no Vórtice, **370 combinações distintas de
/// 51 flags para 939 usuários** — cada pessoa virou um floco de neve, e ninguém consegue auditar
/// nem responder "o que um vendedor pode fazer?".
/// </summary>
public sealed class ConjuntoPermissao
{
    private readonly List<ItemConjuntoPermissao> _itens = [];

    private ConjuntoPermissao() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável. Ex.: <c>GERENTE_VENDAS</c>.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve, em português.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Desligar sem apagar, preservando o histórico de quem já teve.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>As permissões do conjunto, cada uma com sua profundidade.</summary>
    public IReadOnlyCollection<ItemConjuntoPermissao> Itens => _itens.AsReadOnly();

    /// <summary>Cria um conjunto vazio.</summary>
    public static ConjuntoPermissao Criar(string codigo, string nome, string? descricao = null) => new()
    {
        Codigo = codigo,
        Nome = nome,
        Descricao = descricao
    };

    /// <summary>
    /// Concede uma permissão ao conjunto, com a profundidade que ela alcança.
    /// Conceder duas vezes a mesma permissão mantém a MAIOR profundidade — a regra é aditiva.
    /// </summary>
    public ConjuntoPermissao Conceder(string codigoPermissao, Profundidade profundidade)
    {
        if (profundidade == Profundidade.Nenhum)
            throw new RegraDeNegocioViolada(
                $"Não faz sentido conceder '{codigoPermissao}' com profundidade Nenhum. " +
                "Para negar, simplesmente não conceda — o modelo não tem regra de negação.");

        var existente = _itens.FirstOrDefault(i => i.CodigoPermissao == codigoPermissao);

        if (existente is null)
            _itens.Add(ItemConjuntoPermissao.Criar(codigoPermissao, profundidade));
        else if (profundidade > existente.Profundidade)
            existente.AmpliarPara(profundidade);

        return this;
    }

    /// <summary>Desliga o conjunto sem apagá-lo.</summary>
    public void Desativar() => EstaAtivo = false;
}

/// <summary>Uma permissão dentro de um conjunto, com a profundidade que ela alcança.</summary>
public sealed class ItemConjuntoPermissao
{
    private ItemConjuntoPermissao() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Conjunto a que pertence.</summary>
    public int ConjuntoPermissaoId { get; private set; }

    /// <summary>Código da permissão concedida.</summary>
    public string CodigoPermissao { get; private set; } = default!;

    /// <summary>Até onde alcança.</summary>
    public Profundidade Profundidade { get; private set; }

    internal static ItemConjuntoPermissao Criar(string codigoPermissao, Profundidade profundidade) => new()
    {
        CodigoPermissao = codigoPermissao,
        Profundidade = profundidade
    };

    internal void AmpliarPara(Profundidade nova) => Profundidade = nova;
}

/// <summary>
/// A concessão de um conjunto a um usuário.
///
/// <see cref="ExpiraEm"/> cobre férias e substituição sem que alguém esqueça de remover depois —
/// que é como privilégio temporário vira permanente.
/// </summary>
public sealed class UsuarioConjuntoPermissao
{
    private UsuarioConjuntoPermissao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A quem foi concedido.</summary>
    public long UsuarioId { get; private set; }

    /// <summary>O conjunto concedido.</summary>
    public int ConjuntoPermissaoId { get; private set; }

    /// <summary>Quando (UTC).</summary>
    public DateTime ConcedidoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quem concedeu. Auditado permanentemente.</summary>
    public long ConcedidoPorId { get; private set; }

    /// <summary>Quando expira. Nulo = permanente.</summary>
    public DateTime? ExpiraEm { get; private set; }

    /// <summary>Concede um conjunto a um usuário.</summary>
    public static UsuarioConjuntoPermissao Conceder(
        long usuarioId, int conjuntoPermissaoId, long concedidoPorId, DateTime? expiraEm = null)
    {
        if (expiraEm is not null && expiraEm <= DateTime.UtcNow)
            throw new RegraDeNegocioViolada("A data de expiração precisa estar no futuro.");

        return new UsuarioConjuntoPermissao
        {
            UsuarioId = usuarioId,
            ConjuntoPermissaoId = conjuntoPermissaoId,
            ConcedidoPorId = concedidoPorId,
            ExpiraEm = expiraEm
        };
    }

    /// <summary>Verdadeiro se a concessão vale neste instante.</summary>
    public bool EstaVigente(DateTime agora) => ExpiraEm is null || ExpiraEm > agora;
}
