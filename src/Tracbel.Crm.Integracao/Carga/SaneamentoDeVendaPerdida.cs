using Tracbel.Crm.Dominio.Processo;
using System.Globalization;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// As regras de entrada da venda perdida — o que a origem escreve e não é o que parece.
///
/// <para>Um formulário preenchido às pressas depois de perder um negócio é o pior lugar do
/// sistema para confiar em número. Estas três regras não são preciosismo: sem elas, a média de
/// diferença de preço da diretoria sai errada por ordem de grandeza, e a "venda perdida em
/// 2104" aparece no relatório do ano.</para>
///
/// <para><b>Toda correção fica registrada</b> — princípio 1.4 do documento 16. Nada é corrigido
/// em silêncio: quem abrir o relatório de carga vê o valor que entrou, o que saiu e por quê.</para>
/// </summary>
public static class SaneamentoDeVendaPerdida
{
    /// <summary>
    /// O menor valor que ainda pode ser preço de máquina agrícola.
    ///
    /// <para>A origem guarda <c>0,01</c> como preço de trator, e <c>90,00</c> como preço de
    /// colheitadeira. Não são preços com desconto: são tecla presa e campo obrigatório
    /// contornado. O corte em mil reais é conservador de propósito — abaixo dele não existe
    /// máquina, e acima dele o número entra como veio, sem juízo sobre ele.</para>
    /// </summary>
    private const decimal MenorPrecoPlausivel = 1_000m;

    /// <summary>O ano mais antigo que a operação alcança. Antes disso é digitação.</summary>
    private const int PrimeiroAnoPlausivel = 2005;

    /// <summary>
    /// O preço, quando ele é preço.
    /// </summary>
    /// <param name="campo">Nome do campo, para o registro da correção.</param>
    /// <param name="bruto">O que veio da origem.</param>
    /// <param name="correcoes">Onde a correção é registrada.</param>
    public static decimal? Preco(string campo, decimal? bruto, List<CorrecaoAplicada> correcoes)
    {
        if (bruto is null) return null;

        if (bruto.Value >= MenorPrecoPlausivel) return decimal.Round(bruto.Value, 2);

        correcoes.Add(new CorrecaoAplicada(
            campo,
            bruto.Value.ToString(CultureInfo.InvariantCulture),
            "(sem preço)",
            $"Abaixo de {MenorPrecoPlausivel:N0} não é preço de máquina agrícola — é campo " +
            "obrigatório contornado. Entra como não declarado, para não entrar na média."));

        return null;
    }

    /// <summary>
    /// A data da perda, quando ela cabe no calendário.
    /// </summary>
    /// <param name="bruta">O que veio da origem.</param>
    /// <param name="registradaEm">Quando o formulário foi preenchido.</param>
    /// <param name="correcoes">Onde a correção é registrada.</param>
    public static DateOnly? DataDaPerda(
        DateTime? bruta, DateTime registradaEm, List<CorrecaoAplicada> correcoes)
    {
        if (bruta is null) return null;

        var data = DateOnly.FromDateTime(bruta.Value);
        var limite = DateOnly.FromDateTime(registradaEm).AddDays(1);

        // Perder uma venda DEPOIS de registrar que a perdeu é impossível, e a origem tem
        // registros em 2103 e 2104 — quatro dígitos digitados torto no ano.
        if (data.Year >= PrimeiroAnoPlausivel && data <= limite) return data;

        correcoes.Add(new CorrecaoAplicada(
            "dataDaPerda",
            data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            "(sem data)",
            data > limite
                ? "A data da perda é posterior ao dia em que a perda foi registrada."
                : $"A data da perda é anterior a {PrimeiroAnoPlausivel} e não alcança esta operação."));

        return null;
    }

    /// <summary>
    /// Quantas máquinas o negócio tinha. Zero e vazio viram um.
    /// </summary>
    /// <param name="bruta">O que veio da origem.</param>
    /// <param name="correcoes">Onde a correção é registrada.</param>
    public static int Quantidade(decimal? bruta, List<CorrecaoAplicada> correcoes)
    {
        if (bruta is null or <= 0)
        {
            // Não é suposição: houve venda perdida, logo houve pelo menos uma máquina. Zero
            // aqui diria "perdemos nenhuma máquina", que é a única leitura que não faz sentido.
            correcoes.Add(new CorrecaoAplicada(
                "quantidade",
                bruta?.ToString(CultureInfo.InvariantCulture),
                "1",
                "O formulário não declarou quantidade. Houve venda perdida, então houve ao menos " +
                "uma máquina."));
            return 1;
        }

        return (int)decimal.Truncate(bruta.Value);
    }

    /// <summary>
    /// A que família o motivo pertence — a MESMA classificação da carga antiga (congelada), repetida aqui para a
    /// rotina do funil não depender dela.
    ///
    /// <para>A classificação é nossa e não da origem — o formulário do Vórtice tem só o texto. Ela existe para a tela
    /// poder agrupar os motivos em seis famílias sem que alguém decida isso de novo a cada relatório. O que não casa
    /// cai em <see cref="CategoriaDeMotivoDePerda.Outro"/>, a resposta honesta para um motivo que ninguém
    /// classificou.</para>
    /// </summary>
    /// <param name="codigo">O código do motivo, já codificado (sem acento, sem caixa).</param>
    public static CategoriaDeMotivoDePerda CategoriaDoMotivo(string codigo) => codigo switch
    {
        "PRECO" or "CONDICAO_COMERCIAL" or "VALOR_NAO_PERCEBIDO" => CategoriaDeMotivoDePerda.Preco,
        "DISPONIBILIDADE" or "CHEGOU_TARDE" or "PRAZO_DE_ENTREGA" => CategoriaDeMotivoDePerda.Prazo,
        "ESPECIFICACAO_TECNICA" or "JD_NAO_POSSUI_PRODUTO" or "OPTOU_POR_USADO"
            or "COMPARATIVO_DE_PERFORMANCE" or "CONFIGURACAO_INDISPONIVEL" => CategoriaDeMotivoDePerda.Produto,
        "CREDITO_INDEFERIDO_BANCO_JD" or "CREDITO_INDEFERIDO_BJD" or "CREDITO_INDEFERIDO_OUTROS"
            or "FINANCIAMENTO_NEGADO" => CategoriaDeMotivoDePerda.Financiamento,
        "FIEL_A_CONCORRENCIA" or "PREFERENCIA_POR_CONCORRENTE" => CategoriaDeMotivoDePerda.Concorrencia,
        "CLIENTE_ADIOU_INVESTIMENTO" or "DESISTENCIA" => CategoriaDeMotivoDePerda.Desistencia,
        _ => CategoriaDeMotivoDePerda.Outro
    };

    /// <summary>
    /// O "Sim"/"Não" do formulário, com o branco preservado como desconhecido.
    /// </summary>
    /// <param name="bruto">O que veio da origem.</param>
    public static ParticipacaoNaNegociacao SimNaoOuNada(string? bruto)
    {
        var texto = bruto?.Trim();
        if (string.IsNullOrEmpty(texto)) return ParticipacaoNaNegociacao.NaoInformado;

        // "S", "Sim", "SIM" — e nada além disso conta como sim.
        if (texto.StartsWith('S') || texto.StartsWith('s')) return ParticipacaoNaNegociacao.Sim;
        if (texto.StartsWith('N') || texto.StartsWith('n')) return ParticipacaoNaNegociacao.Nao;

        // O que não for reconhecido vira NaoInformado, e nunca "Nao": inventar uma negativa a
        // partir de lixo de digitação afirmaria que ficamos de fora de uma disputa que talvez
        // tenhamos disputado.
        return ParticipacaoNaNegociacao.NaoInformado;
    }
}
