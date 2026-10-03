namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS CÓDIGOS DO IBGE QUE AS LEITURAS CITAM, COM NOME (issue 65) — um lugar só desde o plano 2 do documento 54: a apuração,
/// o potencial, a estrutura e o histórico liam a mesma lista, cada um com a sua cópia.
/// </summary>
internal static class CodigosDoIbge
{
    /// <summary>A UF de São Paulo, como o catálogo de municípios a grava.</summary>
    internal const string SaoPaulo = "SP";

    // =============================================================================================
    // Os códigos do IBGE que esta consulta cita, com nome (issue 65)
    // =============================================================================================

    internal const int CodigoDeSaoPaulo = 35;

    /// <summary>"Bovino", na classificação 79 da Pesquisa da Pecuária Municipal.</summary>
    internal const int Bovino = 2670;

    /// <summary>"Total", na classificação 12605 (potência dos tratores).</summary>
    internal const int PotenciaTotal = 113521;

    /// <summary>"Menos de 100 cv".</summary>
    internal const int PotenciaAbaixoDe100Cv = 113522;

    /// <summary>"De 100 cv e mais".</summary>
    internal const int PotenciaDe100CvEMais = 113523;

    /// <summary>"Total", na classificação 220 (grupos de área total).</summary>
    internal const int GrupoDeAreaTotal = 110085;

    /// <summary>"Total", na classificação 222 (utilização das terras, SIDRA 6881).</summary>
    internal const int UtilizacaoTotal = 110087;

    /// <summary>"Lavouras - permanentes".</summary>
    internal const int LavouraPermanente = 113470;

    /// <summary>"Lavouras - temporárias".</summary>
    internal const int LavouraTemporaria = 113471;

    /// <summary>"Lavouras - área para cultivo de flores".</summary>
    internal const int LavouraDeFlores = 40677;

    // As tabelas e variáveis do SIDRA que identificam a linha publicada do estado (issue 155). Elas
    // estão repetidas do leitor de propósito: a consulta não depende do projeto de integração, e o
    // banco guarda o número do SIDRA justamente para que a leitura seja conferível na origem.

    /// <summary>Censo Agropecuário, tratores por potência.</summary>
    internal const short TabelaDaFrotaDeTratores = 6871;

    /// <summary>"Número de tratores existentes nos estabelecimentos agropecuários" (6871).</summary>
    internal const short VariavelDeTratores = 1862;

    /// <summary>Censo Agropecuário, estabelecimentos por grupo de área total.</summary>
    internal const short TabelaDeEstabelecimentos = 6780;

    /// <summary>"Número de estabelecimentos agropecuários" (6780).</summary>
    internal const short VariavelDeEstabelecimentos = 183;

    /// <summary>Pesquisa da Pecuária Municipal, efetivo dos rebanhos.</summary>
    internal const short TabelaDoRebanho = 3939;

    /// <summary>"Efetivo dos rebanhos", em cabeças (3939).</summary>
    internal const short VariavelDoEfetivoDoRebanho = 105;

    /// <summary>
    /// OS PRODUTOS QUE CONTARIAM DUAS VEZES numa soma de culturas.
    ///
    /// <para>A classificação 782 traz "Café (em grão) Total" (40139) ao lado de "Arábica" (40140) e
    /// "Canephora" (40141), e os três vêm na mesma resposta do SIDRA. A soma mantém o total e
    /// descarta os dois detalhados — é o mesmo critério da planilha do comercial, e é o que faz o
    /// número da tela bater com o do IBGE.</para>
    /// </summary>
    internal static readonly int[] ProdutosQueDuplicamNaSoma = [40140, 40141];

    /// <summary>
    /// AS FAIXAS DE TAMANHO NO VOCABULÁRIO DO COMERCIAL, e as categorias do IBGE que compõem cada uma.
    ///
    /// <para>O IBGE publica 18 faixas, e a planilha do comercial as agrupa em nove. As 18 ficam no
    /// banco; este mapa é a leitura, e mudá-lo não pede recarga.</para>
    ///
    /// <para><b>A última faixa junta DUAS categorias do IBGE</b> — "de 2.500 a menos de 10.000 ha"
    /// (41139) e "de 10.000 ha e mais" (40645) —, porque a planilha pára em "mais de 2.500".</para>
    /// </summary>
    internal static readonly (string Rotulo, int[] Grupos)[] FaixasDoComercial =
    [
        ("Menos de 20 ha", [111543, 111544, 111545, 111546, 111547, 111548, 111549, 111550, 111551, 111552]),
        ("De 20 a 50 ha", [111553]),
        ("De 50 a 100 ha", [111554]),
        ("De 100 a 200 ha", [111555]),
        ("De 200 a 500 ha", [111556]),
        ("De 500 a 1.000 ha", [111557]),
        ("De 1.000 a 2.500 ha", [111558]),
        ("Mais de 2.500 ha", [41139, 40645]),
        ("Produtor sem área", [111560])
    ];
}
