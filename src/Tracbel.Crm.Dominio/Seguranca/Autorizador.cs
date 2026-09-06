using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>Por que alguém enxerga (ou não) um registro. Alimenta o endpoint de explicação.</summary>
/// <param name="Permitido">O veredito.</param>
/// <param name="Motivo">A explicação em português.</param>
/// <param name="Camada">Qual camada decidiu: 2, 3 ou 4.</param>
public sealed record DecisaoAcesso(bool Permitido, string Motivo, int Camada)
{
    /// <summary>Acesso concedido, com o motivo.</summary>
    public static DecisaoAcesso Permite(string motivo, int camada) => new(true, motivo, camada);

    /// <summary>Acesso negado, com o motivo.</summary>
    public static DecisaoAcesso Nega(string motivo, int camada) => new(false, motivo, camada);
}

/// <summary>Um compartilhamento explícito já resolvido — a CAMADA 4.</summary>
/// <param name="Entidade">Ex.: <c>Processo</c>.</param>
/// <param name="RegistroId">O registro compartilhado.</param>
/// <param name="PermiteEditar">Falso = só leitura.</param>
/// <param name="Motivo">Por quê: Manual, Regra, Equipe, Hierarquia, Delegacao.</param>
public sealed record CompartilhamentoAtivo(
    string Entidade, long RegistroId, bool PermiteEditar, string Motivo);

/// <summary>
/// Aplica a ordem de avaliação das camadas de segurança.
///
/// A ORDEM É A REGRA QUE GOVERNA TUDO (documento 05, seção 2):
/// <list type="number">
///   <item>Autenticado? — resolvido antes, na API.</item>
///   <item>Tem a permissão da entidade+verbo? Não → nega. É o TETO ABSOLUTO.</item>
///   <item>A linha cabe na profundidade? Sim → permite.</item>
///   <item>Há compartilhamento explícito? Sim → permite.</item>
///   <item>Campo sensível — tratado na serialização, não aqui.</item>
/// </list>
///
/// Camadas 2, 3 e 4 são ADITIVAS: a permissão é a união, e vence a mais permissiva.
///
/// **NÃO EXISTE REGRA DE NEGAÇÃO.** [DYN] O Dataverse também não tem, de propósito: negação
/// torna a ordem de avaliação imprevisível e é a origem de metade dos bugs de autorização em
/// sistemas grandes. Quando alguém não pode ver algo, REDUZA A PROFUNDIDADE — não crie exceção
/// negativa.
/// </summary>
public sealed class Autorizador
{
    /// <summary>
    /// Decide se o usuário pode agir sobre um registro, e explica por quê.
    ///
    /// A explicação não é luxo: [DYN] a Microsoft precisou construir uma API inteira
    /// (<c>RetrieveAccessOrigin</c>) só para responder "por que este usuário vê isto".
    /// Aqui a resposta sai junto com a decisão, desde a primeira linha de código.
    /// </summary>
    /// <param name="contexto">Quem está agindo.</param>
    /// <param name="entidade">A entidade. Ex.: <c>Processo</c>.</param>
    /// <param name="verbo">O verbo. Ver <see cref="Verbos"/>.</param>
    /// <param name="registroId">O registro alvo.</param>
    /// <param name="proprietarioId">Dono do registro.</param>
    /// <param name="empresaIdDoRegistro">Empresa do registro.</param>
    /// <param name="compartilhamentos">Compartilhamentos já carregados para este usuário.</param>
    public DecisaoAcesso Pode(
        ContextoAcesso contexto,
        string entidade,
        string verbo,
        long registroId,
        long proprietarioId,
        int empresaIdDoRegistro,
        IReadOnlyCollection<CompartilhamentoAtivo>? compartilhamentos = null)
    {
        var codigo = $"{entidade}.{verbo}";

        // ---- Serviço de sistema ----
        if (contexto.EhServicoDeSistema)
            return DecisaoAcesso.Permite(
                "Execução sob identidade de sistema (registrada em aud.EventoAcesso).", 0);

        // ---- CAMADA 2 — teto absoluto ----
        var profundidade = contexto.ProfundidadeDe(codigo);
        if (profundidade == Profundidade.Nenhum)
            return DecisaoAcesso.Nega(
                $"Você não tem a permissão '{codigo}'.", 2);

        // ---- CAMADA 3 — a linha cabe no alcance? ----
        if (contexto.AlcancaRegistro(codigo, proprietarioId, empresaIdDoRegistro))
            return DecisaoAcesso.Permite(ExplicarAlcance(contexto, profundidade, proprietarioId), 3);

        // ---- CAMADA 4 — compartilhamento explícito ----
        var precisaEditar = verbo is not Verbos.Ler;

        var share = compartilhamentos?.FirstOrDefault(c =>
            c.Entidade == entidade
            && c.RegistroId == registroId
            && (!precisaEditar || c.PermiteEditar));

        if (share is not null)
            return DecisaoAcesso.Permite(
                $"Compartilhado com você ({TraduzirMotivo(share.Motivo)}), " +
                $"com permissão de {(share.PermiteEditar ? "edição" : "leitura")}.", 4);

        // ---- Nenhuma camada concedeu ----
        // A mensagem diz O QUE FALTA, não só que foi negado — é o que evita o chamado.
        return DecisaoAcesso.Nega(
            $"Você tem '{codigo}' com alcance {Traduzir(profundidade)}, " +
            "e este registro está fora dele. Peça o compartilhamento a quem é o dono, " +
            "ou solicite ampliação do seu perfil.", 3);
    }

    private static string ExplicarAlcance(ContextoAcesso ctx, Profundidade p, long proprietarioId) => p switch
    {
        Profundidade.Organizacao => "Seu perfil alcança toda a organização.",
        Profundidade.EmpresaEAbaixo => "O registro está na sua empresa ou numa filial abaixo dela.",
        Profundidade.Empresa => "O registro é da sua empresa.",
        Profundidade.Equipe when proprietarioId == ctx.UsuarioId => "Você é o proprietário.",
        Profundidade.Equipe => "O proprietário está subordinado a você na hierarquia de vendas.",
        Profundidade.Proprios => "Você é o proprietário.",
        _ => "Acesso concedido."
    };

    private static string Traduzir(Profundidade p) => p switch
    {
        Profundidade.Proprios => "apenas os seus registros",
        Profundidade.Equipe => "os seus e os da sua equipe",
        Profundidade.Empresa => "a sua empresa",
        Profundidade.EmpresaEAbaixo => "a sua empresa e as filiais abaixo",
        Profundidade.Organizacao => "toda a organização",
        _ => "nenhum"
    };

    private static string TraduzirMotivo(string motivo) => motivo switch
    {
        "Manual" => "compartilhamento manual",
        "Regra" => "por uma regra de automação",
        "Equipe" => "pela sua equipe",
        "Hierarquia" => "pela hierarquia de vendas",
        "Delegacao" => "por delegação temporária",
        _ => motivo
    };

    /// <summary>
    /// Versão que lança, para uso nos casos de uso quando o acesso é pré-requisito.
    /// </summary>
    /// <exception cref="AcessoNegado">Quando nenhuma camada concede.</exception>
    public void ExigirPermissao(
        ContextoAcesso contexto, string entidade, string verbo, long registroId,
        long proprietarioId, int empresaIdDoRegistro,
        IReadOnlyCollection<CompartilhamentoAtivo>? compartilhamentos = null)
    {
        var decisao = Pode(contexto, entidade, verbo, registroId,
            proprietarioId, empresaIdDoRegistro, compartilhamentos);

        if (!decisao.Permitido) throw new AcessoNegado(decisao.Motivo);
    }
}

/// <summary>
/// Acesso negado pelo modelo de permissão. Vira HTTP 403 — nunca 500.
/// </summary>
public sealed class AcessoNegado(string motivo) : Exception(motivo);
