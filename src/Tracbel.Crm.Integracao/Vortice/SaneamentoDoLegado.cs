using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>
/// O saneamento aplicado NA LEITURA do sistema legado — e o registro do que foi corrigido.
///
/// A REGRA QUE ISTO CUMPRE é o princípio 1.4 do documento 16: toda correção automática fica
/// registrada. Nada aqui corrige em silêncio; cada ajuste vira um <see cref="AjusteNaLeitura"/>
/// que sai junto do dado, na resposta da API, para quem está olhando a tela saber que aquele
/// CNPJ não é exatamente o que está gravado lá.
/// </summary>
public static class SaneamentoDoLegado
{
    /// <summary>
    /// RECOMPÕE O DOCUMENTO que o legado partiu em dois e guardou como NÚMERO.
    ///
    /// <para><b>O defeito de origem, medido:</b> <c>GE_Pessoa</c> guarda o documento em duas
    /// colunas numéricas — a base em <c>NroCGCCPF</c> e os dígitos verificadores em
    /// <c>DigCGCCPF</c>. Coluna numérica não preserva zero à esquerda. Medição ao vivo em
    /// 04/09/2026: <b>51.523 de 71.303 pessoas jurídicas (72,3%)</b> têm o CNPJ gravado sem os
    /// zeros que a raiz+filial de 12 dígitos exige.</para>
    ///
    /// <para><b>O que esta função faz:</b> devolve os zeros ao lugar. A base é preenchida à
    /// esquerda até 12 dígitos (jurídica) ou 9 (física), o verificador até 2, e as duas partes
    /// são coladas. Um CNPJ lido como <c>19335660001</c> + <c>39</c> volta a ser
    /// <c>01933566000139</c> (documento fictício).</para>
    ///
    /// <para><b>O que ela NÃO faz, de propósito:</b> não adivinha. Se o número recomposto não
    /// passa no dígito verificador do <see cref="CpfCnpj"/>, o documento sai marcado como não
    /// conferido, com o valor recomposto à vista e o ajuste registrado — e não sai como se
    /// fosse válido. Recompor é reversível e verificável; chutar não é (documento 16,
    /// princípio 1.3: rejeitar é melhor que corrigir em silêncio).</para>
    /// </summary>
    /// <param name="baseNumerica">O que está em <c>NroCGCCPF</c>.</param>
    /// <param name="verificador">O que está em <c>DigCGCCPF</c>.</param>
    /// <param name="ehPessoaFisica">Verdadeiro quando <c>FisicaJuridica</c> é F.</param>
    /// <param name="ajustes">Onde registrar o que foi corrigido.</param>
    /// <returns>O documento recomposto, e se ele confere.</returns>
    public static (string? Documento, bool Confere) RecomporDocumento(
        decimal? baseNumerica,
        decimal? verificador,
        bool ehPessoaFisica,
        List<AjusteNaLeitura> ajustes)
    {
        if (baseNumerica is not { } numero || numero <= 0) return (null, false);

        var tamanhoDaBase = ehPessoaFisica ? 9 : 12;

        var cru = ((long)numero).ToString(System.Globalization.CultureInfo.InvariantCulture);
        var digitos = ((long)(verificador ?? 0)).ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Base maior que o esperado é dado que não cabe no formato — não dá para recompor sem
        // inventar, então sai como não conferido em vez de truncado.
        if (cru.Length > tamanhoDaBase || digitos.Length > 2) return (cru + digitos, false);

        var completo = cru.PadLeft(tamanhoDaBase, '0') + digitos.PadLeft(2, '0');
        var original = cru + digitos;

        if (completo != original)
            ajustes.Add(new AjusteNaLeitura(
                "documento",
                original,
                completo,
                $"O sistema legado guarda o documento em duas colunas NUMÉRICAS, e número não " +
                $"preserva zero à esquerda. Faltavam {completo.Length - original.Length} zero(s); " +
                "a ponte os devolveu ao lugar."));

        var confere = CpfCnpj.TentarCriar(completo, out var validado)
                      && validado.EhPessoaFisica == ehPessoaFisica;

        if (!confere)
            ajustes.Add(new AjusteNaLeitura(
                "documento",
                original,
                completo,
                "O documento recomposto NÃO passa no dígito verificador. Ele está sendo mostrado " +
                "como veio, para conferência manual — e não deve ser usado para casar cadastro."));

        return (completo, confere);
    }

    /// <summary>
    /// Junta DDD e número de telefone e normaliza pelo tipo de valor do domínio.
    ///
    /// [V] <c>FoneNro1</c> é <c>decimal</c> no legado — a mesma escolha de tipo que faz a API de
    /// lá devolver erro genérico para telefone com DDI. Aqui o número volta a ser texto, passa
    /// pelo <see cref="Telefone"/> e, se não passar, sai nulo com o motivo registrado em vez de
    /// sair torto.
    /// </summary>
    public static string? NormalizarTelefone(string? ddd, decimal? numero, List<AjusteNaLeitura> ajustes)
    {
        if (numero is not { } n || n <= 0) return null;

        var cru = (ddd ?? string.Empty).Trim()
                  + ((long)n).ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (Telefone.TentarCriar(cru, out var telefone))
        {
            if (telefone.Numero != cru)
                ajustes.Add(new AjusteNaLeitura(
                    "telefone", cru, telefone.Numero, "Telefone normalizado para DDD + número, sem DDI."));

            return telefone.Formatado();
        }

        ajustes.Add(new AjusteNaLeitura(
            "telefone", cru, string.Empty,
            "O telefone do sistema legado não forma um número brasileiro válido (DDD + 8 ou 9 " +
            "dígitos). Foi omitido em vez de ser mostrado torto."));

        return null;
    }

    /// <summary>Normaliza e-mail pelo tipo de valor do domínio, registrando o que mudou.</summary>
    public static string? NormalizarEmail(string? bruto, List<AjusteNaLeitura> ajustes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        if (Email.TentarCriar(bruto, out var email))
        {
            if (!string.Equals(email.Endereco, bruto.Trim(), StringComparison.Ordinal))
                ajustes.Add(new AjusteNaLeitura(
                    "email", bruto, email.Endereco, "E-mail normalizado (espaços e caixa)."));

            return email.Endereco;
        }

        ajustes.Add(new AjusteNaLeitura(
            "email", bruto, string.Empty,
            "O e-mail do sistema legado não tem forma de e-mail. Foi omitido."));

        return null;
    }

    /// <summary>
    /// Colapsa espaço e apara texto vindo do legado.
    ///
    /// Sem isto, nome com espaço duplo — que existe aos milhares lá — vira nome diferente do
    /// mesmo nome na hora de comparar. É a normalização da seção 2.1 do documento 16.
    /// </summary>
    public static string? Apararar(string? bruto)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var partes = bruto.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return partes.Length == 0 ? null : string.Join(' ', partes);
    }
}
