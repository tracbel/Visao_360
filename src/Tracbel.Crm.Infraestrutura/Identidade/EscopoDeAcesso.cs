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
/// O ESCOPO DE ACESSO — as filiais que a pessoa alcança e o que ela pode fazer nelas.
///
/// <para><b>Mora num lugar só porque existem dois jeitos de saber quem é a pessoa</b> — o cabeçalho
/// provisório e o Entra ID — e UM jeito só de decidir o que ela vê. Se cada resolvedor montasse o
/// próprio escopo, bastaria alguém corrigir a fronteira de filial num deles para os dois modos
/// passarem a enxergar coisas diferentes. Quem trocar a regra troca aqui, e os dois mudam
/// juntos.</para>
/// </summary>
internal static class EscopoDeAcesso
{
    /// <summary>
    /// As permissões que todo usuário recebe, e a profundidade de cada uma.
    ///
    /// <para>É a lista mínima para cadastrar cliente e equipamento e ler catálogo — nada além. O que
    /// passa disso só chega por CONCESSÃO EXPLÍCITA: um conjunto de permissões ativo, concedido ao
    /// usuário em <c>seguranca.UsuarioConjuntoPermissao</c> e ainda não vencido (documento 05, seção
    /// 4). É assim que a visão da empresa (<c>Empresa.AlcanceEntreFiliais</c> em Organização) chega a
    /// um perfil — e quem recebe esse acesso é decisão do negócio, não desta classe.</para>
    /// </summary>
    public static readonly string[] PermissoesConcedidas =
    [
        "Cliente.Ler", "Cliente.Criar", "Cliente.Editar", "Cliente.Excluir",
        "Equipamento.Ler", "Equipamento.Criar", "Equipamento.Editar", "Equipamento.Excluir",
        "Catalogo.Ler", "Lead.Ler"
    ];

    /// <summary>
    /// Monta o contexto de acesso de um usuário já identificado.
    /// </summary>
    /// <param name="banco">Um contexto de SISTEMA — o escopo ainda não existe nesta hora.</param>
    /// <param name="usuarioId">Quem está agindo.</param>
    /// <param name="nomeExibicao">Como ele aparece no log.</param>
    /// <param name="codigoDaFilial">A filial escolhida, pelo código. Nulo usa a filial de casa.</param>
    /// <param name="empresaDeCasaId">A filial de casa do usuário, para quando nenhuma foi escolhida.</param>
    /// <param name="campoDaFilial">O nome do campo que carregou a filial, para a mensagem de erro.</param>
    /// <param name="honrarConcessoesExplicitas">
    /// Se as concessões de <c>seguranca.UsuarioConjuntoPermissao</c> valem. Com identidade provada (Entra ID),
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
        var consulta = banco.Empresas.Where(e => e.EstaAtiva);

        var empresa = string.IsNullOrWhiteSpace(codigoDaFilial)
            ? await consulta.Where(e => e.Id == empresaDeCasaId)
                .Select(e => new { e.Id, e.Caminho })
                .FirstOrDefaultAsync(ct)
            : await consulta.Where(e => e.Codigo == codigoDaFilial)
                .Select(e => new { e.Id, e.Caminho })
                .FirstOrDefaultAsync(ct);

        if (empresa is null)
            return Resultado<ContextoAcesso>.FalhaDeValidacao(
                "A API não reconhece esta filial.",
                [new ErroDeCampo(
                    campoDaFilial,
                    "Não há filial ativa com este código. Consulte /api/v1/catalogos/EMPRESA.",
                    codigoDaFilial)]);

        // AS FILIAIS QUE ELE ALCANÇA = a escolhida MAIS as abaixo dela, pelo caminho
        // materializado. É o mesmo "esta empresa e todas abaixo" que a Profundidade.EmpresaEAbaixo
        // significa, resolvido com um LIKE em vez de consulta recursiva (documento 04).
        var visiveis = await banco.Empresas
            .Where(e => e.EstaAtiva && (e.Id == empresa.Id || e.Caminho.StartsWith(empresa.Caminho + empresa.Id + "/")))
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (!visiveis.Contains(empresa.Id)) visiveis.Add(empresa.Id);

        var profundidades = PermissoesConcedidas.ToDictionary(
            p => p, _ => Profundidade.EmpresaEAbaixo, StringComparer.Ordinal);

        if (honrarConcessoesExplicitas)
        {
            // A CONCESSÃO EXPLÍCITA SOMA, e vence a maior profundidade — a regra aditiva dos conjuntos
            // de permissão (ContextoAcesso.ProfundidadeDe). Conjunto desativado e concessão vencida não
            // entram: privilégio temporário não vira permanente por esquecimento.
            var agora = DateTime.UtcNow;
            var concessoes = await (
                    from concessao in banco.ConcessoesPermissao.AsNoTracking()
                    join conjunto in banco.ConjuntosPermissao.AsNoTracking() on concessao.ConjuntoPermissaoId equals conjunto.Id
                    join item in banco.Set<ItemConjuntoPermissao>().AsNoTracking() on conjunto.Id equals item.ConjuntoPermissaoId
                    where concessao.UsuarioId == usuarioId
                          && conjunto.EstaAtivo
                          && (concessao.ExpiraEm == null || concessao.ExpiraEm > agora)
                    select new { item.CodigoPermissao, item.Profundidade })
                .ToListAsync(ct);

            foreach (var concessao in concessoes)
                if (!profundidades.TryGetValue(concessao.CodigoPermissao, out var atual) || concessao.Profundidade > atual)
                    profundidades[concessao.CodigoPermissao] = concessao.Profundidade;
        }

        return Resultado<ContextoAcesso>.Ok(new ContextoAcesso(
            usuarioId: usuarioId,
            nomeExibicao: nomeExibicao,
            empresaId: empresa.Id,
            empresasVisiveis: visiveis.ToHashSet(),
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: profundidades,
            ehServicoDeSistema: false));
    }
}
