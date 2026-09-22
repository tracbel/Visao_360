using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>Como o par entre a fonte e o município do catálogo foi estabelecido.</summary>
public enum FormaDaCorrespondencia
{
    /// <summary>O nome da fonte, sem acento, caixa e apóstrofo, casou com um município e só um na UF.</summary>
    NomeUnicoNaUf = 0,

    /// <summary>A fonte publicou o código do IBGE e ele foi usado direto.</summary>
    CodigoDoIbgeNaFonte = 1,

    /// <summary>Alguém resolveu à mão o que o nome não resolvia.</summary>
    Manual = 2
}

/// <summary>
/// O DE-PARA ENTRE O QUE A FONTE CHAMA DE MUNICÍPIO E O MUNICÍPIO DO CATÁLOGO (issue 154).
///
/// <para><b>Por que existe.</b> Três fontes do mercado não publicam o código do IBGE: o SICOR usa o código do
/// Banco Central (São Paulo é 27 lá, e 35 no IBGE), e a ANP e as séries de custo da CONAB trazem só o nome. Até
/// 22/09/2026 cada carga refazia o casamento pelo nome a cada rodada — um nome com grafia nova deixava de casar
/// e ninguém via onde; um nome que passasse a existir em dois municípios mudava o resultado em silêncio. A
/// planilha do comercial errou 13 municípios exatamente assim, comparando nome cru.</para>
///
/// <para><b>O que esta tabela é.</b> A correspondência é feita UMA vez, gravada com a forma e a data, e depois
/// consultada pela chave: o código da fonte quando ela tem código, o nome normalizado quando não tem. A carga
/// seguinte não casa por nome de novo — ela lê o par gravado. Corrigir um par é mudar uma linha, e a mudança
/// entra na trilha de auditoria.</para>
///
/// <para><b>O que ela não faz.</b> Não inventa par: nome que casa com dois municípios, ou com nenhum, vira
/// recusa com o motivo, e aparece no painel de fontes. Adivinhar aqui seria pôr crédito de um município na
/// conta de outro.</para>
/// </summary>
public sealed class CorrespondenciaDeMunicipio
{
    private CorrespondenciaDeMunicipio() { }

    /// <summary>O tamanho da chave e do texto da fonte.</summary>
    public const int TamanhoDoTexto = 120;

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O fluxo da fonte (<c>BCB.SICOR_INVESTIMENTO</c>, <c>ANP.USINA_DE_ETANOL</c>, <c>CONAB.CUSTO_DE_PRODUCAO</c>).</summary>
    public string Fonte { get; private set; } = default!;

    /// <summary>
    /// A chave que identifica o município NA FONTE: o código dela quando existe (o do Banco Central, no SICOR),
    /// ou o nome normalizado quando não existe (ANP, séries de custo da CONAB).
    /// </summary>
    public string ChaveNaFonte { get; private set; } = default!;

    /// <summary>O texto como a fonte o publica — é o que alguém procura quando confere o par.</summary>
    public string TextoNaFonte { get; private set; } = default!;

    /// <summary>O município do catálogo.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>Como o par foi estabelecido.</summary>
    public FormaDaCorrespondencia Forma { get; private set; }

    /// <summary>Quando o par foi estabelecido ou corrigido (UTC).</summary>
    public DateTime CasadaEm { get; private set; }

    /// <summary>Quem casou — a carga, ou a pessoa que corrigiu.</summary>
    public long CasadaPorId { get; private set; }

    /// <summary>Registra um par.</summary>
    /// <param name="fonte">O fluxo da fonte.</param>
    /// <param name="chaveNaFonte">O código ou o nome normalizado da fonte.</param>
    /// <param name="textoNaFonte">O texto como a fonte publica.</param>
    /// <param name="municipioId">O município do catálogo.</param>
    /// <param name="forma">Como o par foi estabelecido.</param>
    /// <param name="casadaPorId">Quem casou.</param>
    /// <param name="agoraUtc">O instante.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta fonte, chave, texto ou município.</exception>
    public static CorrespondenciaDeMunicipio Registrar(
        string fonte, string chaveNaFonte, string textoNaFonte, int municipioId,
        FormaDaCorrespondencia forma, long casadaPorId, DateTime agoraUtc)
    {
        if (string.IsNullOrWhiteSpace(fonte))
            throw new RegraDeNegocioViolada("A correspondência precisa dizer de qual fonte ela é.");

        if (string.IsNullOrWhiteSpace(chaveNaFonte))
            throw new RegraDeNegocioViolada("A correspondência precisa da chave do município na fonte.");

        if (string.IsNullOrWhiteSpace(textoNaFonte))
            throw new RegraDeNegocioViolada("A correspondência guarda o texto da fonte, para alguém poder conferir o par.");

        if (municipioId <= 0)
            throw new RegraDeNegocioViolada("A correspondência aponta para um município do catálogo.");

        if (!Enum.IsDefined(forma))
            throw new RegraDeNegocioViolada("A forma da correspondência não é uma das conhecidas.");

        return new CorrespondenciaDeMunicipio
        {
            Fonte = fonte.Trim(),
            ChaveNaFonte = Encurtar(chaveNaFonte),
            TextoNaFonte = Encurtar(textoNaFonte),
            MunicipioId = municipioId,
            Forma = forma,
            CasadaEm = agoraUtc,
            CasadaPorId = casadaPorId
        };
    }

    /// <summary>
    /// Aponta o par para outro município, ou atualiza o texto da fonte. Devolve se mudou.
    ///
    /// <para>É o caminho da correção: a carga não reaponta sozinha um par já gravado — quem reaponta é quem
    /// decidiu, e a trilha guarda o de-para anterior.</para>
    /// </summary>
    /// <param name="municipioId">O município certo.</param>
    /// <param name="textoNaFonte">O texto como a fonte publica hoje.</param>
    /// <param name="forma">Como o par foi estabelecido agora.</param>
    /// <param name="casadaPorId">Quem corrigiu.</param>
    /// <param name="agoraUtc">O instante.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o município não vale.</exception>
    public bool Reapontar(int municipioId, string textoNaFonte, FormaDaCorrespondencia forma, long casadaPorId, DateTime agoraUtc)
    {
        if (municipioId <= 0)
            throw new RegraDeNegocioViolada("A correspondência aponta para um município do catálogo.");

        var texto = Encurtar(textoNaFonte);
        var mudou = MunicipioId != municipioId || Forma != forma || !string.Equals(TextoNaFonte, texto, StringComparison.Ordinal);

        MunicipioId = municipioId;
        TextoNaFonte = texto;
        Forma = forma;
        CasadaEm = agoraUtc;
        CasadaPorId = casadaPorId;
        return mudou;
    }

    private static string Encurtar(string texto)
    {
        var limpo = texto.Trim();
        return limpo.Length > TamanhoDoTexto ? limpo[..TamanhoDoTexto] : limpo;
    }
}
