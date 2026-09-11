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
    /// As permissões concedidas, e a profundidade de cada uma.
    ///
    /// <para>É a lista mínima para cadastrar cliente e equipamento e ler catálogo — nada além. O
    /// login pelo Entra ID resolve QUEM é a pessoa; ele ainda não resolve O QUE ela pode, e não
    /// finge que resolve: as profundidades continuam as da ponte provisória até os conjuntos de
    /// permissão existirem (documento 05, seção 4). Isso vale como dívida nomeada.</para>
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
    /// <param name="ct">Cancelamento.</param>
    public static async Task<Resultado<ContextoAcesso>> MontarAsync(
        CrmDbContext banco,
        long usuarioId,
        string nomeExibicao,
        string? codigoDaFilial,
        int empresaDeCasaId,
        string campoDaFilial,
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

        return Resultado<ContextoAcesso>.Ok(new ContextoAcesso(
            usuarioId: usuarioId,
            nomeExibicao: nomeExibicao,
            empresaId: empresa.Id,
            empresasVisiveis: visiveis.ToHashSet(),
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: PermissoesConcedidas.ToDictionary(
                p => p, _ => Profundidade.EmpresaEAbaixo, StringComparer.Ordinal),
            ehServicoDeSistema: false));
    }
}
