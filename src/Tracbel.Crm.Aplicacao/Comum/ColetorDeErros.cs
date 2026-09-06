using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Aplicacao.Comum;

/// <summary>
/// Junta os erros de UMA requisição, campo a campo, e só então recusa.
///
/// POR QUE NÃO RECUSAR NO PRIMEIRO ERRO: porque quem preencheu o formulário quer saber tudo o
/// que está errado de uma vez. Recusar no primeiro campo faz o usuário corrigir, reenviar,
/// descobrir o segundo, corrigir, reenviar — o mesmo trabalho repartido em cinco viagens.
///
/// POR QUE ISTO NÃO É UMA BIBLIOTECA DE VALIDAÇÃO: porque a validação de VALOR já mora nos
/// tipos de valor do domínio (<see cref="CpfCnpj"/>, <see cref="Telefone"/>,
/// <see cref="Chassi"/> e os demais), e não deve ser reescrita em regra declarativa. O que
/// falta é só um lugar para acumular a recusa deles — e é isto aqui, em trinta linhas, em vez
/// de um pacote a mais na solução (documento 22, seção 10.1).
/// </summary>
public sealed class ColetorDeErros
{
    private readonly List<ErroDeCampo> _erros = [];

    /// <summary>Houve pelo menos um erro?</summary>
    public bool TemErro => _erros.Count > 0;

    /// <summary>Registra um erro de campo.</summary>
    /// <param name="campo">O nome do campo como ele chega na requisição.</param>
    /// <param name="mensagem">O que fazer, em português.</param>
    /// <param name="valorRecebido">O que chegou.</param>
    public void Registrar(string campo, string mensagem, string? valorRecebido = null) =>
        _erros.Add(new ErroDeCampo(campo, mensagem, valorRecebido));

    /// <summary>
    /// Exige que um texto obrigatório tenha vindo. Devolve o texto já aparado, ou vazio quando
    /// faltou — e nesse caso o erro já ficou registrado.
    /// </summary>
    /// <param name="campo">O nome do campo.</param>
    /// <param name="valor">O que chegou.</param>
    /// <param name="oQueEh">Como o campo se chama para o usuário. Ex.: "a razão social".</param>
    public string Obrigatorio(string campo, string? valor, string oQueEh)
    {
        if (!string.IsNullOrWhiteSpace(valor)) return valor.Trim();

        Registrar(campo, $"Informe {oQueEh}.", valor);
        return string.Empty;
    }

    /// <summary>
    /// Converte um texto para um item de domínio fechado (um <c>enum</c>), recusando com a lista
    /// do que vale.
    ///
    /// É a regra do documento 16, seção 3.1, aplicada na API: campo com domínio não aceita texto
    /// livre. A mensagem já traz as opções, para quem chamou não precisar procurar em lugar
    /// nenhum qual era o valor certo.
    /// </summary>
    /// <typeparam name="T">O domínio fechado.</typeparam>
    /// <param name="campo">O nome do campo.</param>
    /// <param name="valor">O que chegou.</param>
    /// <returns>O item escolhido, ou nulo quando o campo faltou ou não vale.</returns>
    public T? ItemDeDominio<T>(string campo, string? valor) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Registrar(campo, $"Informe um valor. Opções: {Opcoes<T>()}.", valor);
            return null;
        }

        return Interpretar<T>(campo, valor);
    }

    /// <summary>
    /// A mesma conferência, para o campo que TEM valor padrão — filtro de listagem, ordenação,
    /// situação inicial.
    ///
    /// POR QUE É UM MÉTODO SEPARADO, e não um parâmetro opcional do anterior: um parâmetro
    /// <c>T? padrao</c> é <c>Nullable&lt;T&gt;</c>, e o C# não infere <c>T</c> a partir de um
    /// argumento não-anulável nessa posição — todo chamador teria de escrever o tipo à mão ou
    /// se contorcer numa conversão. Assinatura que obriga o chamador a se contorcer é assinatura
    /// errada; então são duas, cada uma dizendo no nome o que faz.
    ///
    /// Campo vazio devolve o padrão SEM erro (não informar é legítimo); campo preenchido com
    /// valor fora da lista devolve o padrão COM erro registrado — a recusa não é engolida, e o
    /// chamador continua com um valor utilizável enquanto termina de coletar os outros erros.
    /// </summary>
    /// <typeparam name="T">O domínio fechado.</typeparam>
    /// <param name="campo">O nome do campo.</param>
    /// <param name="valor">O que chegou.</param>
    /// <param name="padrao">O que usar quando o campo veio vazio.</param>
    public T ItemDeDominioOuPadrao<T>(string campo, string? valor, T padrao) where T : struct, Enum =>
        string.IsNullOrWhiteSpace(valor)
            ? padrao
            : Interpretar<T>(campo, valor) ?? padrao;

    private T? Interpretar<T>(string campo, string valor) where T : struct, Enum
    {
        if (Enum.TryParse<T>(valor.Trim(), ignoreCase: true, out var escolhido)
            && Enum.IsDefined(escolhido))
            return escolhido;

        Registrar(campo, $"Valor fora da lista. Opções: {Opcoes<T>()}.", valor);
        return null;
    }

    private static string Opcoes<T>() where T : struct, Enum => string.Join(", ", Enum.GetNames<T>());

    /// <summary>
    /// Lê o carimbo de concorrência que o cliente devolveu, em base64.
    ///
    /// Nulo ou vazio é aceito e significa "não conferir" — a concessão está explicada em
    /// <see cref="EntidadeBase.VersaoConfere"/>. O que NÃO é aceito é base64 inválido: isso é
    /// erro de quem chamou, e vale dizer qual campo está errado em vez de tratar como ausente.
    /// </summary>
    /// <param name="campo">O nome do campo.</param>
    /// <param name="base64">O que chegou.</param>
    public byte[]? CarimboDeVersao(string campo, string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;

        try
        {
            return Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            Registrar(campo, "A versão precisa ser exatamente o valor devolvido pela consulta do registro.", base64);
            return null;
        }
    }

    /// <summary>Fecha a coleta como falha de validação. Só chame quando <see cref="TemErro"/>.</summary>
    /// <typeparam name="T">O tipo que a operação devolveria em caso de sucesso.</typeparam>
    /// <param name="resumo">A frase de topo, que a resposta usa como título.</param>
    public Resultado<T> Recusar<T>(string resumo) =>
        Resultado<T>.FalhaDeValidacao(resumo, _erros);
}
