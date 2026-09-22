using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Seguranca;

/// <summary>
/// O ESCOPO EFETIVO DE QUEM PERGUNTA — as filiais que pode escolher e o que pode fazer na filial atual
/// (fase 3 do documento 41).
/// </summary>
public sealed class ObterEscopoDeAcesso(IProvedorContextoAcesso provedor, IRepositorioDeEscopo repositorio, IRelogio relogio)
{
    /// <summary>Monta o escopo.</summary>
    /// <param name="filialPedidaRecusada">A filial do cabeçalho que não é permitida, quando houve recusa.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<EscopoDoUsuario>>> ExecutarAsync(string? filialPedidaRecusada, CancellationToken ct)
    {
        var acesso = provedor.Atual;
        var (atual, filiais) = await repositorio.FiliaisAsync(acesso, ct);
        var perfis = await repositorio.PerfisAsync(acesso, ct);

        var permissoes = acesso.Profundidades
            .Where(p => p.Value > Profundidade.Nenhum)
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => new PermissaoDoEscopo(
                p.Key,
                Permissoes.Catalogo.TryGetValue(p.Key, out var descricao) ? descricao : p.Key,
                p.Value.ToString()))
            .ToList();

        // EM "TODAS AS FILIAIS" a filial do contexto é a de casa só porque ele precisa de uma; a tela tem
        // de mostrar o que a pessoa escolheu.
        if (acesso.VeTodasAsFiliais)
            atual = new FilialDoEscopo(ContextoAcesso.CodigoDeTodasAsFiliais, "Todas as filiais", EhCasa: false);

        return Resultado<ComProcedencia<EscopoDoUsuario>>.Ok(
            ComProcedencia<EscopoDoUsuario>.DoNossoBanco(
                new EscopoDoUsuario(
                    acesso.NomeExibicao, atual, filialPedidaRecusada, filiais, permissoes,
                    PodeVerTodasAsFiliais: acesso.PodeAlcancarTodasAsEmpresas,
                    Perfis: perfis),
                "seguranca.Perfil + seguranca.UsuarioPerfil + organizacao.Empresa",
                relogio));
    }
}
