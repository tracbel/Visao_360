using System.Globalization;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Saneamento;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// AS REGRAS DO CATÁLOGO DE SANEAMENTO (documento 16, seção 2) aplicadas campo a campo na carga,
/// cada uma devolvendo o que foi corrigido ou o que foi recusado.
///
/// <para><b>Por que não reaproveitar <see cref="SaneamentoDoLegado"/> inteiro:</b> aquele saneia
/// para a PONTE DE LEITURA, e registra o que corrigiu em <c>AjusteNaLeitura</c> — o tipo que sai
/// na resposta HTTP junto do dado. Aqui o destino é o banco, e a trilha é a do pipeline de
/// saneamento (<see cref="CorrecaoAplicada"/> / <see cref="CampoRejeitado"/>), que é o que fica
/// gravado. A recomposição do documento, essa sim, é a MESMA função nos dois caminhos: existe um
/// só lugar que sabe devolver o zero à esquerda ao lugar (princípio 1.2 do documento 16).</para>
///
/// <para>Toda função aqui obedece ao princípio 1.3: quando o valor não passa, o campo sai
/// <b>vazio com o motivo registrado</b> — nunca truncado, nunca adivinhado.</para>
/// </summary>
public static class SaneamentoDaCarga
{
    /// <summary>
    /// As 27 unidades federativas. É a única lista fechada que o documento 16 (seção 9) admite
    /// como restrição de verificação em vez de catálogo: ela não muda por decisão de negócio.
    /// </summary>
    private static readonly HashSet<string> UnidadesFederativas = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    /// <summary>
    /// Normaliza texto livre pelo tipo de valor do domínio, respeitando o tamanho REAL da coluna.
    ///
    /// <para>[V] O legado corta em silêncio: 44.962 linhas de histórico gravadas com exatamente
    /// 20 caracteres num campo de 150. Aqui, texto maior que a coluna é <b>recusado</b>, não
    /// truncado — a marca d'água do truncamento nunca se forma porque o truncamento não
    /// acontece.</para>
    /// </summary>
    /// <param name="campo">O nome do campo, como aparece na trilha.</param>
    /// <param name="bruto">O texto como veio da origem.</param>
    /// <param name="tamanhoMaximo">O tamanho da coluna de destino.</param>
    /// <param name="correcoes">Onde registrar a normalização aplicada.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O texto normalizado, ou nulo.</returns>
    public static string? Texto(
        string campo,
        string? bruto,
        int tamanhoMaximo,
        List<CorrecaoAplicada> correcoes,
        List<CampoRejeitado> rejeicoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        if (!TextoNormalizado.TentarCriar(bruto, out var texto, tamanhoMaximo))
        {
            rejeicoes.Add(new CampoRejeitado(
                campo, bruto,
                $"O texto não sobrevive à normalização ou passa de {tamanhoMaximo} caracteres, " +
                "que é o tamanho da coluna. Foi omitido em vez de cortado."));
            return null;
        }

        if (!string.Equals(texto.Valor, bruto, StringComparison.Ordinal))
            correcoes.Add(new CorrecaoAplicada(
                campo, bruto, texto.Valor,
                "Texto normalizado: espaço nas pontas removido, espaço duplo colapsado e " +
                "caractere invisível descartado."));

        return texto.Valor;
    }

    /// <summary>
    /// RECOMPÕE o documento partido em duas colunas numéricas e o confere.
    ///
    /// <para>Delega a recomposição a <see cref="SaneamentoDoLegado.RecomporDocumento"/> — um só
    /// ponto de normalização por tipo — e traduz o que ele registrou para a trilha do pipeline.
    /// Quando o número recomposto não passa no dígito verificador, o documento sai <b>nulo</b> e
    /// a rejeição fica escrita: quem chama decide se isso derruba a linha inteira.</para>
    /// </summary>
    /// <param name="baseNumerica">A base do documento, como o legado a guarda.</param>
    /// <param name="verificador">Os dígitos verificadores, como o legado os guarda.</param>
    /// <param name="ehPessoaFisica">Verdadeiro quando a origem diz que é pessoa física.</param>
    /// <param name="correcoes">Onde registrar a recomposição.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O documento válido, ou nulo.</returns>
    public static CpfCnpj? Documento(
        decimal? baseNumerica,
        decimal? verificador,
        bool ehPessoaFisica,
        List<CorrecaoAplicada> correcoes,
        List<CampoRejeitado> rejeicoes)
    {
        if (baseNumerica is not { } numero || numero <= 0) return null;

        var ajustes = new List<Dominio.Integracao.AjusteNaLeitura>();
        var (recomposto, confere) =
            SaneamentoDoLegado.RecomporDocumento(baseNumerica, verificador, ehPessoaFisica, ajustes);

        if (recomposto is null) return null;

        if (!confere)
        {
            rejeicoes.Add(new CampoRejeitado(
                "documento", recomposto,
                "O documento não se reconstitui num CPF/CNPJ com dígito verificador válido nem " +
                "depois de devolver os zeros à esquerda que a coluna numérica da origem perdeu. " +
                "Chutar o dígito certo não é recompor — é inventar."));
            return null;
        }

        // A recomposição só é "correção" quando algo mudou de verdade. O primeiro ajuste
        // registrado pela função de recomposição é exatamente o dos zeros devolvidos.
        var zerosDevolvidos = ajustes.FirstOrDefault(a => a.Motivo.Contains("zero", StringComparison.Ordinal));
        if (zerosDevolvidos is not null)
            correcoes.Add(new CorrecaoAplicada(
                "documento", zerosDevolvidos.ValorNoLegado, zerosDevolvidos.ValorEntregue,
                "O sistema de origem guarda o documento em duas colunas NUMÉRICAS, e número não " +
                "preserva zero à esquerda. Os zeros foram devolvidos ao lugar e o dígito " +
                "verificador confere."));

        return CpfCnpj.Criar(recomposto);
    }

    /// <summary>Normaliza e-mail pelo tipo de valor do domínio, registrando o que mudou.</summary>
    /// <param name="bruto">O e-mail como veio.</param>
    /// <param name="correcoes">Onde registrar a normalização.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O e-mail normalizado, ou nulo.</returns>
    public static string? Email(
        string? bruto, List<CorrecaoAplicada> correcoes, List<CampoRejeitado> rejeicoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        if (!Dominio.Comum.Email.TentarCriar(bruto, out var email))
        {
            rejeicoes.Add(new CampoRejeitado(
                "email", bruto,
                "Não tem forma de e-mail (falta @, falta ponto no domínio, ou tem espaço). Um " +
                "endereço assim não entrega mensagem nenhuma: foi omitido."));
            return null;
        }

        if (!string.Equals(email.Endereco, bruto, StringComparison.Ordinal))
            correcoes.Add(new CorrecaoAplicada(
                "email", bruto, email.Endereco, "E-mail normalizado: caixa baixa e espaços aparados."));

        return email.Endereco;
    }

    /// <summary>
    /// Junta DDD e número, normaliza pelo tipo <see cref="Telefone"/> e registra o que mudou.
    ///
    /// <para>[V] A coluna de telefone do legado é <c>decimal(12,0)</c> — o mesmo pecado do
    /// documento, e a razão pela qual 168 linhas têm quantidade de dígitos impossível.</para>
    /// </summary>
    /// <param name="ddd">O DDD, como texto.</param>
    /// <param name="numero">O número, como a coluna numérica o guarda.</param>
    /// <param name="correcoes">Onde registrar a normalização.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O telefone só com dígitos, ou nulo.</returns>
    public static string? Telefone(
        string? ddd, decimal? numero, List<CorrecaoAplicada> correcoes, List<CampoRejeitado> rejeicoes)
    {
        if (numero is not { } n || n <= 0) return null;

        var cru = (ddd ?? string.Empty).Trim()
                  + ((long)n).ToString(CultureInfo.InvariantCulture);

        if (!Dominio.Comum.Telefone.TentarCriar(cru, out var telefone))
        {
            rejeicoes.Add(new CampoRejeitado(
                "telefone", cru,
                "Não forma um número brasileiro válido (DDD de 11 a 99 mais 8 ou 9 dígitos). " +
                "Foi omitido em vez de gravado torto."));
            return null;
        }

        if (!string.Equals(telefone.Numero, cru, StringComparison.Ordinal))
            correcoes.Add(new CorrecaoAplicada(
                "telefone", cru, telefone.Numero,
                "Telefone normalizado para DDD + número, sem DDI e com o nono dígito completado " +
                "quando a origem tinha um celular antigo de oito dígitos."));

        return telefone.Numero;
    }

    /// <summary>Confere a UF contra as 27 unidades federativas, normalizando a caixa.</summary>
    /// <param name="bruto">A UF como veio.</param>
    /// <param name="correcoes">Onde registrar a normalização.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>A UF em maiúsculas, ou nulo.</returns>
    public static string? Uf(string? bruto, List<CorrecaoAplicada> correcoes, List<CampoRejeitado> rejeicoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var normalizada = bruto.Trim().ToUpperInvariant();

        if (!UnidadesFederativas.Contains(normalizada))
        {
            rejeicoes.Add(new CampoRejeitado(
                "uf", bruto, "Não é uma das 27 unidades federativas. Foi omitida."));
            return null;
        }

        if (!string.Equals(normalizada, bruto, StringComparison.Ordinal))
            correcoes.Add(new CorrecaoAplicada("uf", bruto, normalizada, "UF normalizada para maiúsculas."));

        return normalizada;
    }

    /// <summary>Valida o CEP pelo tipo de valor do domínio, registrando o que mudou.</summary>
    /// <param name="bruto">O CEP como veio.</param>
    /// <param name="correcoes">Onde registrar a normalização.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O CEP validado, ou nulo.</returns>
    public static Cep? CepDe(string? bruto, List<CorrecaoAplicada> correcoes, List<CampoRejeitado> rejeicoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        if (!Cep.TentarCriar(bruto, out var cep))
        {
            rejeicoes.Add(new CampoRejeitado(
                "cep", bruto,
                "Não tem os oito dígitos de um CEP, ou é um dígito só repetido — que é erro de " +
                "preenchimento, não faixa dos Correios. Foi omitido."));
            return null;
        }

        if (!string.Equals(cep.Numero, bruto.Trim(), StringComparison.Ordinal))
            correcoes.Add(new CorrecaoAplicada("cep", bruto, cep.Numero, "CEP normalizado para oito dígitos."));

        return cep;
    }

    /// <summary>
    /// Aceita a coordenada só quando o par está completo E cai dentro do Brasil.
    ///
    /// <para>Coordenada errada não tem correção sensata (documento 16, seção 2.13): ou está na
    /// faixa, ou é bruta e sai fora. (0,0) é o caso mais comum e fica no golfo da Guiné.</para>
    /// </summary>
    /// <param name="latitude">Latitude como a origem a guarda.</param>
    /// <param name="longitude">Longitude como a origem a guarda.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O par validado, ou os dois nulos.</returns>
    public static (decimal? Latitude, decimal? Longitude) Coordenada(
        decimal? latitude, decimal? longitude, List<CampoRejeitado> rejeicoes)
    {
        if (latitude is not { } lat || longitude is not { } lon) return (null, null);

        if (Dominio.Comum.Coordenada.TentarCriar((double)lat, (double)lon, out _)) return (lat, lon);

        rejeicoes.Add(new CampoRejeitado(
            "coordenada", $"{lat};{lon}",
            "A coordenada cai fora da área de atuação (Brasil) ou está em (0,0). Foi omitida — " +
            "coordenada errada não tem correção sensata."));

        return (null, null);
    }

    /// <summary>
    /// Lê um ano de um campo que na origem é TEXTO LIVRE, dentro da faixa que a máquina admite.
    /// </summary>
    /// <param name="campo">O nome do campo, para a trilha.</param>
    /// <param name="bruto">O conteúdo do campo de texto.</param>
    /// <param name="rejeicoes">Onde registrar a recusa.</param>
    /// <returns>O ano, ou nulo.</returns>
    public static short? Ano(string campo, string? bruto, List<CampoRejeitado> rejeicoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var texto = bruto.Trim();

        if (short.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ano)
            && ano is >= 1900 and <= 2100)
            return ano;

        rejeicoes.Add(new CampoRejeitado(
            campo, bruto,
            "O campo de ano da origem é texto livre e o conteúdo não é um ano entre 1900 e 2100. " +
            "Foi omitido."));

        return null;
    }

    /// <summary>
    /// Traduz o código de situação de UMA LETRA da origem para o domínio fechado do CRM.
    ///
    /// <para>[V] O legado mistura cliente, prospect e o que ninguém documentou num
    /// <c>char(1)</c>, com milhares de linhas em branco. Só duas letras têm significado
    /// confirmado pelo negócio: <c>A</c> e <c>P</c>. O resto vira <see cref="SituacaoDoCliente.Suspect"/>
    /// — que é literalmente "ainda não se sabe" — e a tradução fica registrada como correção,
    /// para ninguém confundir o que foi lido com o que foi assumido.</para>
    /// </summary>
    /// <param name="codigo">A letra como a origem a guarda.</param>
    /// <param name="correcoes">Onde registrar a tradução assumida.</param>
    /// <returns>A situação no domínio fechado.</returns>
    public static SituacaoDoCliente Situacao(string? codigo, List<CorrecaoAplicada> correcoes)
    {
        var letra = (codigo ?? string.Empty).Trim().ToUpperInvariant();

        switch (letra)
        {
            case "A": return SituacaoDoCliente.Cliente;
            case "P": return SituacaoDoCliente.Prospect;
            default:
                correcoes.Add(new CorrecaoAplicada(
                    "situacao", codigo, nameof(SituacaoDoCliente.Suspect),
                    letra.Length == 0
                        ? "A situação está em branco na origem. Entrou como Suspect — 'ainda não " +
                          "se sabe' — em vez de ser adivinhada."
                        : $"A letra '{letra}' não tem significado documentado no sistema de origem. " +
                          "Entrou como Suspect em vez de ser adivinhada."));
                return SituacaoDoCliente.Suspect;
        }
    }
}
