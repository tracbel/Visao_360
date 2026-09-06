using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O CONTEXTO DE ACESSO DA CARGA — um serviço de sistema, declarado como tal.
///
/// <para><b>Por que a carga não se disfarça de usuário:</b> ela grava em treze filiais na mesma
/// execução, e nenhum usuário do CRM tem — nem deve ter — esse alcance. Fingir ser o gerente de
/// Ribeirão Preto para atravessar a fronteira de multiempresa seria exatamente o furo que o
/// documento 05 existe para fechar: o alcance largo passaria a caber num usuário de gente.</para>
///
/// <para><b>O que <c>EhServicoDeSistema</c> faz, exatamente:</b> o filtro global do
/// <c>CrmDbContext</c> deixa de restringir por filial, e <c>ProfundidadeDe</c> devolve
/// Organização para toda permissão. Isto é poder de verdade, e é por isso que ele mora aqui, num
/// processo de linha de comando com escopo e horário conhecidos, e não numa requisição HTTP.</para>
///
/// <para>A alternativa — abrir <c>AbrirAlcanceEntreEmpresas</c> a cada lote — daria o mesmo
/// resultado e escreveria dezenas de milhares de linhas de diário dizendo a mesma coisa. A
/// declaração de serviço de sistema diz isso uma vez, no lugar certo.</para>
/// </summary>
internal sealed class ContextoDeCargaDeSistema : IProvedorContextoAcesso
{
    /// <summary>Monta o contexto do processo de carga.</summary>
    /// <param name="usuarioResponsavelId">A quem os registros criados ficam atribuídos.</param>
    /// <param name="empresaDeCasaId">A filial de referência, quando alguma regra precisar de uma.</param>
    /// <param name="empresasAlcancadas">As filiais que a carga vai preencher.</param>
    public ContextoDeCargaDeSistema(
        long usuarioResponsavelId, int empresaDeCasaId, IReadOnlySet<int> empresasAlcancadas) =>
        Atual = new ContextoAcesso(
            usuarioId: usuarioResponsavelId,
            nomeExibicao: "Carga do sistema legado",
            empresaId: empresaDeCasaId,
            empresasVisiveis: empresasAlcancadas,
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(StringComparer.Ordinal),
            ehServicoDeSistema: true);

    /// <inheritdoc />
    public ContextoAcesso Atual { get; }
}
