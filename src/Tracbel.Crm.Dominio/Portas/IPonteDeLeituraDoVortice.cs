using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A PONTE DE LEITURA para o sistema legado — somente leitura, sempre com procedência.
///
/// TRÊS COMPROMISSOS, e nenhum é dispensável:
///
/// 1. <b>Somente leitura.</b> Não existe método de escrita nesta porta, e não vai existir. O
///    legado continua sendo o dono do dado dele enquanto estiver vivo; nós lemos para mostrar,
///    nunca para gravar por cima.
///
/// 2. <b>Toda resposta carrega procedência.</b> O tipo de retorno é
///    <see cref="LeituraDoLegado{T}"/>, e não a lista crua: não existe assinatura aqui que
///    devolva dado sem dizer de onde veio e quando foi lido.
///
/// 3. <b>Falhar é resultado, não exceção.</b> A ponte depende de VPN e de um servidor que não é
///    nosso. Quando ele não responde, isto devolve
///    <see cref="TipoDeFalha.DependenciaIndisponivel"/> com a frase que o usuário lê — e o
///    resto da aplicação segue de pé. Uma ponte caída NÃO pode derrubar o cadastro.
///
/// O vocabulário do legado não atravessa esta porta: as assinaturas falam de cliente, de parque
/// de máquinas e de identificador, e a tradução acontece do lado de lá, em
/// <c>Tracbel.Crm.Integracao</c>. Quando o legado morrer, apaga-se a implementação e esta
/// interface some junto.
/// </summary>
public interface IPonteDeLeituraDoVortice
{
    /// <summary>
    /// Procura clientes no legado por nome, nome fantasia ou documento.
    /// </summary>
    /// <param name="termo">O que digitar na busca. Menos de três caracteres é recusado.</param>
    /// <param name="limite">Quantas linhas no máximo.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<Resultado<LeituraDoLegado<IReadOnlyList<ClienteNoLegado>>>> BuscarClientesAsync(
        string termo, int limite, CancellationToken ct);

    /// <summary>
    /// O parque de máquinas de um cliente do legado — a lista que o CEN mantém e que continua
    /// sendo atualizada hoje.
    /// </summary>
    /// <param name="identificadorNoLegado">A chave da pessoa no sistema de origem.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<Resultado<LeituraDoLegado<IReadOnlyList<EquipamentoNoLegado>>>> ListarParqueDeMaquinasAsync(
        long identificadorNoLegado, CancellationToken ct);

    /// <summary>
    /// A ponte está de pé? Serve ao diagnóstico e é o que a tela consulta antes de oferecer as
    /// abas do legado.
    /// </summary>
    Task<Resultado<LeituraDoLegado<SaudeDaPonte>>> VerificarAsync(CancellationToken ct);
}
