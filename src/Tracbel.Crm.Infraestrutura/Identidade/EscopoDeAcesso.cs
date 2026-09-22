using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Infraestrutura.Identidade;

/// <summary>
/// Quem a conta Microsoft diz que é — já extraído do token, sem nada de HTTP.
///
/// <para>Existe para a infraestrutura não depender de nome de reivindicação. Quem lê o token é a
/// API; daqui para baixo, só interessa o que o Entra afirmou.</para>
/// </summary>
/// <param name="IdentidadeExterna">O identificador de objeto (<c>oid</c>). Não muda nunca.</param>
/// <param name="NomePrincipal">O nome principal (<c>preferred_username</c>).</param>
/// <param name="Email">O e-mail, quando o token traz.</param>
/// <param name="Nome">O nome de exibição, quando o token traz.</param>
public sealed record IdentidadeDoEntra(Guid IdentidadeExterna, string NomePrincipal, string? Email, string? Nome);

/// <summary>
/// O ESCOPO DE ACESSO — as filiais que a pessoa alcança e o que ela pode fazer nelas (fase 3 do
/// documento 41).
///
/// <para><b>Mora num lugar só porque existem dois jeitos de saber quem é a pessoa</b> — o cabeçalho
/// provisório e o Entra ID — e UM jeito só de decidir o que ela vê. Se cada resolvedor montasse o
/// próprio escopo, bastaria alguém corrigir a fronteira de filial num deles para os dois modos
/// passarem a enxergar coisas diferentes.</para>
///
/// <para><b>O que a pessoa pode fazer</b> é a soma do <b>perfil padrão</b> (o que todo usuário recebe;
/// Q-P2, 21/09/2026: o mínimo, sem excluir) com os <b>perfis concedidos</b> a ela e ainda vigentes. Até
/// a fase 3 isso era uma lista fixa em código que dava a todos criar, editar e excluir — sem ninguém ter
/// decidido.</para>
///
/// <para><b>Quais filiais ela pode escolher</b> (P-20, 21/09/2026): a de casa e aquelas em que tem um
/// perfil concedido; qualquer outra, 403. Até a fase 3, qualquer filial ativa passava pelo cabeçalho.
/// Quem tem a visão entre filiais em profundidade Organização escolhe qualquer uma — e também
/// <see cref="ContextoAcesso.CodigoDeTodasAsFiliais"/>, que põe todas as filiais no alcance de uma vez.</para>
/// </summary>
internal static class EscopoDeAcesso
{
    /// <summary>
    /// Monta o contexto de acesso de um usuário já identificado.
    /// </summary>
    /// <param name="banco">Um contexto de SISTEMA — o escopo ainda não existe nesta hora.</param>
    /// <param name="usuarioId">Quem está agindo.</param>
    /// <param name="nomeExibicao">Como ele aparece no log.</param>
    /// <param name="codigoDaFilial">A filial escolhida, pelo código. Nulo usa a filial de casa.</param>
    /// <param name="empresaDeCasaId">A filial de casa do usuário.</param>
    /// <param name="campoDaFilial">O nome do campo que carregou a filial, para a mensagem de erro.</param>
    /// <param name="honrarConcessoesExplicitas">
    /// Se as concessões de <c>seguranca.UsuarioPerfil</c> valem. Com identidade provada (Entra ID),
    /// sempre; pela ponte provisória, só em Desenvolvimento — cabeçalho não autentica ninguém.
    /// </param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<Resultado<ContextoAcesso>> MontarAsync(
        CrmDbContext banco,
        long usuarioId,
        string nomeExibicao,
        string? codigoDaFilial,
        int empresaDeCasaId,
        string campoDaFilial,
        bool honrarConcessoesExplicitas,
        CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        // TODAS AS FILIAIS não é uma filial: é o código que o seletor manda quando o administrador pede
        // todas de uma vez. O contexto ainda precisa de UMA filial, e ela é a de casa — o cadastro recusa
        // registro novo neste modo, então ela nunca decide onde algo nasce.
        var todasAsFiliais = string.Equals(codigoDaFilial?.Trim(), ContextoAcesso.CodigoDeTodasAsFiliais, StringComparison.OrdinalIgnoreCase);

        // ---- 1. A filial pedida existe? -------------------------------------------------------
        var consulta = banco.Empresas.Where(e => e.EstaAtiva);

        var empresa = string.IsNullOrWhiteSpace(codigoDaFilial) || todasAsFiliais
            ? await consulta.Where(e => e.Id == empresaDeCasaId)
                .Select(e => new { e.Id, e.Caminho, e.Codigo })
                .FirstOrDefaultAsync(ct)
            : await consulta.Where(e => e.Codigo == codigoDaFilial)
                .Select(e => new { e.Id, e.Caminho, e.Codigo })
                .FirstOrDefaultAsync(ct);

        if (empresa is null)
            return Resultado<ContextoAcesso>.FalhaDeValidacao(
                "A API não reconhece esta filial.",
                [new ErroDeCampo(
                    campoDaFilial,
                    "Não há filial ativa com este código. Consulte /api/v1/acesso/escopo para ver as suas.",
                    codigoDaFilial)]);

        // ---- 2. Os perfis: o padrão, mais os concedidos e vigentes ---------------------------
        var padraoId = await banco.Perfis.AsNoTracking()
            .Where(p => p.EhPadrao && p.EstaAtivo)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(ct);

        // SEM PERFIL PADRÃO NÃO HÁ ACESSO NENHUM, e isso é defeito de instalação, não de pessoa: a
        // migração da fase 3 semeia o perfil. Recusar com a explicação é melhor do que devolver um
        // contexto sem permissão nenhuma e deixar cada tela descobrir sozinha um 403 sem motivo.
        if (padraoId is null)
            return Resultado<ContextoAcesso>.SemPermissao(
                "Este banco não tem o perfil padrão (seguranca.Perfil com EhPadrao = 1). A migração da " +
                "fase 3 (PerfilDeAcessoESubstituicaoDoPapel) semeia esse perfil — confira se ela foi aplicada.");

        var concessoes = honrarConcessoesExplicitas
            ? await (
                    from concessao in banco.UsuariosPerfis.AsNoTracking()
                    join perfil in banco.Perfis.AsNoTracking() on concessao.PerfilId equals perfil.Id
                    where concessao.UsuarioId == usuarioId
                          && perfil.EstaAtivo
                          && concessao.RevogadaEm == null && (concessao.ExpiraEm == null || concessao.ExpiraEm > agora)
                    select new { concessao.PerfilId, concessao.EmpresaId })
                .ToListAsync(ct)
            : [];

        // A FILIAL DA CONCESSÃO DECIDE ONDE ELA VALE: sem filial, em qualquer uma que ele possa escolher;
        // com filial, só nela. As que valem em toda parte entram primeiro, porque é delas que sai a visão
        // entre filiais — e é ela que libera escolher qualquer filial.
        var perfisGerais = concessoes.Where(c => c.EmpresaId == null).Select(c => c.PerfilId).Append(padraoId.Value).ToHashSet();
        var profundidades = await ProfundidadesAsync(banco, perfisGerais, ct);

        // ---- 3. P-20: ele pode escolher esta filial? ------------------------------------------
        var filiaisConcedidas = concessoes.Where(c => c.EmpresaId != null).Select(c => c.EmpresaId!.Value).ToHashSet();
        var podeEscolherQualquer = profundidades.GetValueOrDefault(Permissoes.EmpresaAlcanceEntreFiliais) >= Profundidade.Organizacao;

        if (todasAsFiliais)
        {
            if (!podeEscolherQualquer)
                return Resultado<ContextoAcesso>.SemPermissao(
                    "\"Todas as filiais\" é a visão de quem administra o CRM.",
                    [new ErroDeCampo(
                        campoDaFilial,
                        $"Olhar todas as filiais de uma vez exige a permissão {Permissoes.EmpresaAlcanceEntreFiliais} " +
                        "em profundidade Organização (perfil Administrador ou Visão entre filiais). Escolha uma das suas filiais.",
                        ContextoAcesso.CodigoDeTodasAsFiliais)]);

            // TODAS, INCLUSIVE AS INATIVAS: "todas as filiais" é o que diz, e o dado de uma filial fechada
            // (Guaíra, Ituverava, Monte Alto) continua no banco. O perfil concedido só numa filial NÃO
            // entra aqui — valer em todas seria dar a ele um alcance que ninguém concedeu.
            var todas = await banco.Empresas.Select(e => e.Id).ToListAsync(ct);

            return Resultado<ContextoAcesso>.Ok(new ContextoAcesso(
                usuarioId: usuarioId,
                nomeExibicao: nomeExibicao,
                empresaId: empresa.Id,
                empresasVisiveis: todas.ToHashSet(),
                subordinadosIds: await SubordinadosAsync(banco, usuarioId, ct),
                equipesIds: new HashSet<long>(),
                profundidades: profundidades,
                ehServicoDeSistema: false,
                todasAsFiliais: true));
        }

        if (empresa.Id != empresaDeCasaId && !filiaisConcedidas.Contains(empresa.Id) && !podeEscolherQualquer)
            return Resultado<ContextoAcesso>.SemPermissao(
                "Esta filial não está entre as suas.",
                [new ErroDeCampo(
                    campoDaFilial,
                    "Você pode escolher a sua filial de casa e as filiais em que tem um perfil concedido. " +
                    "Para olhar outra, peça a concessão a quem administra os perfis.",
                    empresa.Codigo)]);

        var perfisDaFilial = concessoes
            .Where(c => c.EmpresaId == empresa.Id && !perfisGerais.Contains(c.PerfilId))
            .Select(c => c.PerfilId)
            .ToHashSet();

        if (perfisDaFilial.Count > 0)
            foreach (var (codigo, profundidade) in await ProfundidadesAsync(banco, perfisDaFilial, ct))
                if (!profundidades.TryGetValue(codigo, out var atual) || profundidade > atual)
                    profundidades[codigo] = profundidade;

        // ---- 4. O alcance: a filial escolhida e as abaixo dela --------------------------------
        // Pelo caminho materializado — o mesmo "esta empresa e todas abaixo" que a
        // Profundidade.EmpresaEAbaixo significa, resolvido com um LIKE em vez de consulta recursiva.
        var visiveis = await banco.Empresas
            .Where(e => e.EstaAtiva && (e.Id == empresa.Id || e.Caminho.StartsWith(empresa.Caminho + empresa.Id + "/")))
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (!visiveis.Contains(empresa.Id)) visiveis.Add(empresa.Id);

        // ---- 5. A hierarquia: os subordinados, por Usuario.GestorId ---------------------------
        var subordinados = await SubordinadosAsync(banco, usuarioId, ct);

        return Resultado<ContextoAcesso>.Ok(new ContextoAcesso(
            usuarioId: usuarioId,
            nomeExibicao: nomeExibicao,
            empresaId: empresa.Id,
            empresasVisiveis: visiveis.ToHashSet(),
            subordinadosIds: subordinados,
            equipesIds: new HashSet<long>(),
            profundidades: profundidades,
            ehServicoDeSistema: false));
    }

    /// <summary>
    /// A permissão e a MAIOR profundidade dela entre os perfis — a regra aditiva dos perfis (vence a mais
    /// permissiva; não existe regra de negação).
    /// </summary>
    private static async Task<Dictionary<string, Profundidade>> ProfundidadesAsync(
        CrmDbContext banco, IReadOnlySet<int> perfis, CancellationToken ct)
    {
        var itens = await banco.PerfisPermissoes.AsNoTracking()
            .Where(i => perfis.Contains(i.PerfilId))
            .Select(i => new { i.CodigoPermissao, i.Profundidade })
            .ToListAsync(ct);

        var profundidades = new Dictionary<string, Profundidade>(StringComparer.Ordinal);
        foreach (var item in itens)
            if (!profundidades.TryGetValue(item.CodigoPermissao, out var atual) || item.Profundidade > atual)
                profundidades[item.CodigoPermissao] = item.Profundidade;

        return profundidades;
    }

    /// <summary>
    /// Todos os subordinados, em qualquer nível, pela única hierarquia do modelo: <c>Usuario.GestorId</c>
    /// (documento 40). Até a fase 3 o contexto nascia com a lista vazia, e a profundidade <c>Equipe</c>
    /// era letra morta.
    ///
    /// <para>Uma consulta por nível, só entre os ativos: a hierarquia comercial tem poucos níveis, e o
    /// conjunto de visitados impede que um ciclo por erro de cadastro (A gestor de B, B gestor de A)
    /// trave a montagem do contexto.</para>
    /// </summary>
    private static async Task<IReadOnlySet<long>> SubordinadosAsync(CrmDbContext banco, long usuarioId, CancellationToken ct)
    {
        var todos = new HashSet<long>();
        var nivel = new List<long> { usuarioId };

        for (var profundidade = 0; profundidade < 20 && nivel.Count > 0; profundidade++)
        {
            var atual = nivel;
            nivel = await banco.Usuarios.AsNoTracking()
                .Where(u => u.GestorId != null && atual.Contains(u.GestorId.Value) && u.EstaAtivo && u.ExcluidoEm == null)
                .Select(u => u.Id)
                .ToListAsync(ct);

            nivel = nivel.Where(id => id != usuarioId && todos.Add(id)).ToList();
        }

        return todos;
    }
}
