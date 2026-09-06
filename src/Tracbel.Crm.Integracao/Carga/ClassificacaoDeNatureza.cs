using System.Globalization;
using System.Text;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// SEPARA O QUE É OPERAÇÃO DO QUE É RESÍDUO — sem apagar nada.
///
/// ---------------------------------------------------------------------------------------------
/// O PROBLEMA, MEDIDO. O cadastro do legado mistura três coisas na mesma tabela e as trata como
/// iguais:
///
///  1. <b>Carteira de venda</b> — cliente atribuído a um CEN para vender.
///  2. <b>Depósito de cadastro</b> — "CADASTROS INATIVOS TRACBEL AGRO" (71 clientes), "FORA
///     ATUAÇÃO TRACBEL AGRO" (53), "COL_FUNC" (colaboradores). São gavetas, não carteiras.
///  3. <b>Resíduo de teste</b> — "TESTE APP MOBILE LITE", "TESTE_DSI", "TI_RPO" (da consultoria
///     que implantou o Vórtice).
///
/// Somadas, elas contaminam toda métrica de gerência. O caso mais visível: a MAIOR carteira do
/// sistema, com <b>5.000 clientes</b>, é da Inteligência de Mercado — e por causa dela um
/// departamento aparecia como o CEN nº 1 do ranking da diretoria, à frente de qualquer vendedor.
///
/// ---------------------------------------------------------------------------------------------
/// A REGRA É DE EVIDÊNCIA, E NÃO DE LISTA ESCRITA À MÃO. Cada classificação abaixo se apoia em
/// algo que a PRÓPRIA ORIGEM declara — a linha de negócio "DADOS CADASTRAIS", a palavra "TESTE"
/// no nome, o nome do fornecedor no responsável. Uma lista de nomes proibidos envelheceria na
/// primeira carteira nova; uma regra sobre o que o dado diz, não.
///
/// <para><b>E ela devolve o porquê.</b> Toda decisão sai com o motivo em texto, que a carga
/// registra como correção aplicada. Ninguém precisa abrir este arquivo para saber por que uma
/// carteira ficou de fora do ranking.</para>
///
/// <para><b>Classificar não é apagar.</b> Os vínculos existem, o histórico é real e continua
/// consultável. O que muda é que a tela de gerência passa a poder dizer "as 128 carteiras
/// comerciais" em vez de "as 142 carteiras", e o número passa a significar o que ele diz. O
/// negócio corrige a leitura pela tela quando ela estiver errada — e vai estar, em algum caso.</para>
/// </summary>
public static class ClassificacaoDeNatureza
{
    /// <summary>A linha de negócio do legado que não é linha de negócio: é gaveta de cadastro.</summary>
    private const string LinhaDeCadastro = "DADOS CADASTRAIS";

    /// <summary>
    /// O que a carteira é, e por quê.
    /// </summary>
    /// <param name="nome">Nome da carteira na origem.</param>
    /// <param name="linhaDeNegocio">Nome da linha de negócio dela.</param>
    /// <param name="nomeDoResponsavel">Nome do responsável, como a origem o grava.</param>
    /// <param name="correcoes">Onde a decisão é registrada, quando não é a padrão.</param>
    public static NaturezaDaCarteira DaCarteira(
        string? nome,
        string? linhaDeNegocio,
        string? nomeDoResponsavel,
        List<CorrecaoAplicada> correcoes)
    {
        var alvo = Normalizar(nome);
        var linha = Normalizar(linhaDeNegocio);
        var responsavel = Normalizar(nomeDoResponsavel);

        // TESTE VEM PRIMEIRO. Uma carteira de teste na gaveta de cadastro continua sendo teste, e
        // é a informação mais forte das duas.
        if (Contem(alvo, "TESTE") || Contem(responsavel, "TESTE"))
            return Decidir(NaturezaDaCarteira.Teste, nome, correcoes,
                "O nome da carteira ou do responsável diz TESTE. É resíduo de implantação, e "
                + "resíduo de implantação não entra em número de gerência.");

        if (Contem(responsavel, "CONSULTORIA") || Contem(responsavel, "VORTICE"))
            return Decidir(NaturezaDaCarteira.Teste, nome, correcoes,
                "O responsável é a consultoria que implantou o sistema de origem, e não um CEN "
                + "da Tracbel.");

        // A PRÓPRIA ORIGEM DECLARA que esta linha não é de venda.
        if (linha == LinhaDeCadastro)
            return Decidir(NaturezaDaCarteira.Administrativa, nome, correcoes,
                $"A linha de negócio na origem é \"{LinhaDeCadastro}\", que não é linha de venda: "
                + "é a gaveta onde o cadastro guarda quem não está em carteira de ninguém.");

        // As gavetas que se declaram no nome.
        if (Contem(alvo, "INATIVO") || Contem(alvo, "FORA ATUACAO") || Contem(alvo, "FORA_REG")
            || Contem(alvo, "SEM POTENCIAL") || Contem(alvo, "COL_FUNC"))
            return Decidir(NaturezaDaCarteira.Administrativa, nome, correcoes,
                "O nome declara que a carteira é um depósito de cadastro — inativos, fora de "
                + "atuação, sem potencial ou colaboradores —, e não uma carteira de venda.");

        return NaturezaDaCarteira.Comercial;
    }

    /// <summary>
    /// O que a conta de usuário é, e por quê.
    /// </summary>
    /// <param name="nomeExibicao">Nome como a origem o mostra.</param>
    /// <param name="login">Login na origem.</param>
    /// <param name="correcoes">Onde a decisão é registrada, quando não é a padrão.</param>
    public static NaturezaDoUsuario DoUsuario(
        string? nomeExibicao, string? login, List<CorrecaoAplicada> correcoes)
    {
        var nome = Normalizar(nomeExibicao);
        var conta = Normalizar(login);

        if (Contem(nome, "TESTE") || Contem(conta, "TESTE"))
            return Decidir(NaturezaDoUsuario.Teste, nomeExibicao, correcoes,
                "O nome ou o login diz TESTE. A conta assina registro real e por isso não é "
                + "apagada, mas não é vendedor e não entra em ranking de pessoa.");

        // O SERVIDOR DA ORIGEM ASSINA 30.755 INTERAÇÕES do recorte de 2026. Contá-lo como pessoa
        // faria dele o campeão de produtividade da empresa.
        if (conta is "VRTCSERVER" || Contem(nome, "VRTCSERVER") || Contem(nome, "IMPORT")
            || Contem(nome, "INDEFINIDO"))
            return Decidir(NaturezaDoUsuario.Sistema, nomeExibicao, correcoes,
                "É a conta com que o próprio sistema de origem registra o que ninguém digitou.");

        if (Contem(nome, "CONSULTORIA") || Contem(nome, "VORTICE") || conta.StartsWith("V:", StringComparison.Ordinal))
            return Decidir(NaturezaDoUsuario.Fornecedor, nomeExibicao, correcoes,
                "É conta do fornecedor que implantou o sistema de origem, e não de um colaborador "
                + "da Tracbel.");

        // CAIXA DE ÁREA. Tem mais de um humano atrás dela, e a classificação diz isso — mas
        // NÃO a tira dos números. A Inteligência de Mercado atende 5.000 clientes de verdade, e
        // a cobertura dessa carteira importa tanto quanto a de qualquer CEN. O que a natureza
        // resolve é outra coisa: a tela mostra ao lado do nome que aquilo é uma ÁREA, para
        // ninguém comparar o volume de um time com o de uma pessoa sem saber que está fazendo
        // isso. Quem fica de fora dos números é o que não é operação — sistema, fornecedor e
        // teste, decididos acima.
        if (Contem(nome, "INT.MERCADO") || Contem(nome, "INTELIGENCIA DE MERCADO")
            || Contem(nome, "CALL.CENTER") || Contem(nome, "CALL CENTER")
            || Contem(nome, "EXPERIENCIA.CLIENTE") || Contem(nome, "EM_ABERTO"))
            return Decidir(NaturezaDoUsuario.Departamento, nomeExibicao, correcoes,
                "É caixa de área, com mais de uma pessoa atrás dela. Continua nos números — a "
                + "carteira dela é real —, mas aparece identificada como área.");

        return NaturezaDoUsuario.Pessoa;
    }

    private static T Decidir<T>(T natureza, string? alvo, List<CorrecaoAplicada> correcoes, string motivo)
        where T : struct, Enum
    {
        correcoes.Add(new CorrecaoAplicada(
            "natureza",
            alvo,
            natureza.ToString(),
            motivo));

        return natureza;
    }

    /// <summary>Sem acento, sem caixa e sem espaço repetido — para a comparação não depender de digitação.</summary>
    private static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var semAcento = texto.Trim().Normalize(NormalizationForm.FormD);
        var construtor = new System.Text.StringBuilder(semAcento.Length);

        foreach (var caractere in semAcento)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) is UnicodeCategory.NonSpacingMark)
                continue;

            construtor.Append(char.ToUpperInvariant(caractere));
        }

        var bruto = construtor.ToString();
        while (bruto.Contains("  ", StringComparison.Ordinal))
            bruto = bruto.Replace("  ", " ", StringComparison.Ordinal);

        return bruto;
    }

    private static bool Contem(string alvo, string agulha) =>
        alvo.Contains(agulha, StringComparison.Ordinal);
}
