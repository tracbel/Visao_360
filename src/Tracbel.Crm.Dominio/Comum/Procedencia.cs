namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// DE ONDE VEIO O DADO E QUANDO — o carimbo que acompanha toda leitura feita fora do nosso banco.
///
/// POR QUE ISTO EXISTE, e por que não é enfeite: o legado tem tabelas VIVAS e tabelas MORTAS
/// convivendo com a mesma cara. Faturamento parado desde 11/04/2025, frota parada desde
/// 24/05/2024 e cadastro de pessoa atualizado hoje respondem à mesma consulta, com o mesmo
/// formato, e nada na resposta diz qual é qual. Foi assim que 17 meses de dado velho passaram
/// por atual sem ninguém notar.
///
/// Então toda resposta da ponte carrega: qual sistema respondeu, qual objeto foi lido, quando
/// nós lemos, qual é a alteração mais recente que aquele objeto tem, e um aviso escrito quando
/// o dado está velho o bastante para a tela precisar dizer isso ao usuário.
/// </summary>
/// <param name="Sistema">Quem respondeu. Ex.: <c>Vórtice</c>.</param>
/// <param name="Objeto">O que foi lido, no vocabulário do sistema de origem.</param>
/// <param name="LidoEmUtc">Quando NÓS lemos (UTC). Não é a idade do dado — é a idade da resposta.</param>
/// <param name="DadoMaisRecenteEm">A alteração mais recente encontrada (UTC). Nulo quando o objeto não datou nada.</param>
/// <param name="EstaDesatualizado">Verdadeiro quando o dado é velho o bastante para a tela avisar.</param>
/// <param name="Aviso">O que dizer ao usuário, em português, quando há o que dizer.</param>
public sealed record Procedencia(
    string Sistema,
    string Objeto,
    DateTime LidoEmUtc,
    DateTime? DadoMaisRecenteEm,
    bool EstaDesatualizado,
    string? Aviso);

/// <summary>Fábricas de <see cref="Procedencia"/> para as origens que o CRM tem hoje.</summary>
public static class Procedencias
{
    /// <summary>O nome do nosso sistema nas respostas. Uma constante, para não haver duas grafias.</summary>
    public const string SistemaProprio = "CRM Tracbel";

    /// <summary>
    /// O carimbo do que veio do NOSSO banco.
    ///
    /// Marcar a própria casa parece redundante, e não é: a tela mistura, na mesma página, o
    /// cadastro nosso e a leitura da ponte. Se só o legado viesse carimbado, o front teria de
    /// deduzir a origem do resto pela ausência de carimbo — e dedução é exatamente o que este
    /// tipo existe para eliminar.
    /// </summary>
    /// <param name="objeto">A tabela lida. Ex.: <c>comercial.Cliente</c>.</param>
    /// <param name="agora">O instante da leitura, em UTC.</param>
    public static Procedencia DoNossoBanco(string objeto, DateTime agora) =>
        new(SistemaProprio, objeto, agora, null, false, null);
}
