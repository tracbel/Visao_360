using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Aplicacao.Comum;

/// <summary>
/// O envelope de toda resposta de leitura da API: o dado e o carimbo de origem, juntos.
///
/// A REGRA QUE ISTO IMPÕE: nenhuma leitura sai daqui sem dizer de onde veio. Vale para o nosso
/// banco e vale para a ponte do legado — o front recebe o mesmo formato nos dois casos e
/// marca a origem na tela sem precisar saber por qual rota o dado chegou.
/// </summary>
/// <typeparam name="T">O que está sendo devolvido.</typeparam>
/// <param name="Dados">O conteúdo.</param>
/// <param name="Procedencia">De onde veio e quando.</param>
public sealed record ComProcedencia<T>(T Dados, Procedencia Procedencia)
{
    /// <summary>Envelopa um dado do nosso próprio banco.</summary>
    /// <param name="dados">O conteúdo.</param>
    /// <param name="objeto">A tabela lida. Ex.: <c>comercial.Cliente</c>.</param>
    /// <param name="relogio">A fonte de tempo, injetada para o teste poder fixar "agora".</param>
    public static ComProcedencia<T> DoNossoBanco(T dados, string objeto, IRelogio relogio) =>
        new(dados, Procedencias.DoNossoBanco(objeto, relogio.Agora));
}
