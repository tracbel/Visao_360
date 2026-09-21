using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O CRÉDITO RURAL DE INVESTIMENTO DE UM MUNICÍPIO NUM MÊS — uma linha do SICOR, do Banco Central
/// (issue 68).
///
/// <para><b>Por que existe.</b> O texto-base do potencial usa o crédito contratado como termômetro do
/// mercado: "pegar os últimos 12 meses e dividir pelos meses anteriores", "70% de contratos e 30% do
/// valor", "valor, ticket médio e share em 25 e 26". O índice e as faixas são da issue 73; aqui entra o
/// dado, município a município.</para>
///
/// <para><b>Não é um contrato.</b> O SICOR publica a soma dos contratos de cada combinação de produto,
/// programa, subprograma, fonte, seguro, atividade e modalidade, por município e mês — sem número de
/// contrato e sem quantidade. A "linha" é a contagem que existe; é a que a planilha do comercial usa.</para>
///
/// <para><b>Não identifica cliente nem revenda.</b> Nunca dirá "este contrato foi da Tracbel" (D-P09):
/// a comparação com as vendas é agregada, por município e período.</para>
///
/// <para><b>A chave natural é a combinação inteira</b> — município do Banco Central, ano, mês, produto,
/// programa, subprograma, fonte, seguro, atividade e modalidade. Medido nos arquivos da pasta 360 (4.138
/// linhas de 2025 e 5.409 de 2026): nenhuma combinação repetida.</para>
/// </summary>
public sealed class CreditoRuralDeInvestimento
{
    private CreditoRuralDeInvestimento() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município do catálogo (IBGE), casado pelo nome.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O código do município no Banco Central — que não é o do IBGE.</summary>
    public int CodigoMunicipioBcb { get; private set; }

    /// <summary>O ano de emissão.</summary>
    public short Ano { get; private set; }

    /// <summary>O mês de emissão.</summary>
    public byte Mes { get; private set; }

    /// <summary>O produto financiado (tabela auxiliar <c>Produto</c> do SICOR).</summary>
    public int CodigoProduto { get; private set; }

    /// <summary>O programa de crédito.</summary>
    public int CodigoPrograma { get; private set; }

    /// <summary>O subprograma.</summary>
    public int CodigoSubprograma { get; private set; }

    /// <summary>A fonte de recurso.</summary>
    public int CodigoFonte { get; private set; }

    /// <summary>O tipo de seguro; −1 é "não informado", como o SICOR escreve.</summary>
    public int CodigoSeguro { get; private set; }

    /// <summary>A atividade: 1 agrícola, 2 pecuária.</summary>
    public byte Atividade { get; private set; }

    /// <summary>A modalidade de crédito.</summary>
    public int CodigoModalidade { get; private set; }

    /// <summary>O valor contratado, em reais.</summary>
    public decimal Valor { get; private set; }

    /// <summary>A área financiada, em hectares; zero quando o investimento não é por área.</summary>
    public decimal Area { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>A combinação que identifica a linha.</summary>
    public sealed record Chave(
        int CodigoMunicipioBcb, short Ano, byte Mes, int CodigoProduto, int CodigoPrograma,
        int CodigoSubprograma, int CodigoFonte, int CodigoSeguro, byte Atividade, int CodigoModalidade);

    /// <summary>A chave desta linha.</summary>
    public Chave ChaveNatural => new(
        CodigoMunicipioBcb, Ano, Mes, CodigoProduto, CodigoPrograma, CodigoSubprograma, CodigoFonte, CodigoSeguro, Atividade, CodigoModalidade);

    /// <summary>Registra uma linha do SICOR.</summary>
    /// <param name="chave">A combinação.</param>
    /// <param name="municipioId">O município do catálogo.</param>
    /// <param name="valor">O valor contratado.</param>
    /// <param name="area">A área financiada.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o mês, o ano, o valor ou a área não valem.</exception>
    public static CreditoRuralDeInvestimento Registrar(
        Chave chave, int municipioId, decimal valor, decimal area, long importadoPorId, DateTime agoraUtc)
    {
        if (chave.Mes is < 1 or > 12)
            throw new RegraDeNegocioViolada($"Mês de emissão fora de 1 a 12: {chave.Mes}.");

        if (chave.Ano is < 2000 or > 2100)
            throw new RegraDeNegocioViolada($"Ano de emissão fora do razoável: {chave.Ano}.");

        Conferir(valor, area);

        return new CreditoRuralDeInvestimento
        {
            MunicipioId = municipioId,
            CodigoMunicipioBcb = chave.CodigoMunicipioBcb,
            Ano = chave.Ano,
            Mes = chave.Mes,
            CodigoProduto = chave.CodigoProduto,
            CodigoPrograma = chave.CodigoPrograma,
            CodigoSubprograma = chave.CodigoSubprograma,
            CodigoFonte = chave.CodigoFonte,
            CodigoSeguro = chave.CodigoSeguro,
            Atividade = chave.Atividade,
            CodigoModalidade = chave.CodigoModalidade,
            Valor = valor,
            Area = area,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>
    /// Confere a linha contra uma nova leitura. O Banco Central acrescenta contrato registrado com atraso
    /// aos meses recentes — o valor da combinação muda, e a trilha de auditoria guarda o anterior.
    /// </summary>
    /// <param name="valor">O valor na nova leitura.</param>
    /// <param name="area">A área na nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o valor ou a área não valem.</exception>
    public bool Revisar(decimal valor, decimal area, long importadoPorId, DateTime agoraUtc)
    {
        Conferir(valor, area);

        if (Valor == valor && Area == area) return false;

        Valor = valor;
        Area = area;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        return true;
    }

    private static void Conferir(decimal valor, decimal area)
    {
        // VALOR ZERO NÃO É CRÉDITO: é linha vazia. Negativo não existe.
        if (valor <= 0) throw new RegraDeNegocioViolada($"O valor de uma linha de crédito é positivo; veio {valor}.");
        if (area < 0) throw new RegraDeNegocioViolada($"A área financiada não é negativa; veio {area}.");
    }
}

/// <summary>
/// UM ITEM DAS TABELAS AUXILIARES DO SICOR — programa, subprograma, fonte de recurso ou produto (issue 68).
///
/// <para>O Banco Central as publica em <c>bcb.gov.br/htms/sicor/</c>, com vigência. É o que transforma
/// "7080" em "TRATOR" e "154" em "MODERFROTA". Uma tabela só para as quatro, porque as quatro têm a
/// mesma forma — código, descrição e vigência — e o que as separa é o tipo.</para>
/// </summary>
public sealed class ItemDoSicor
{
    private ItemDoSicor() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A tabela: <c>PROGRAMA</c>, <c>SUBPROGRAMA</c>, <c>FONTE</c> ou <c>PRODUTO</c>.</summary>
    public string Tipo { get; private set; } = default!;

    /// <summary>O código, sem zeros à esquerda.</summary>
    public int Codigo { get; private set; }

    /// <summary>O programa do subprograma; zero nas demais tabelas.</summary>
    public int CodigoPai { get; private set; }

    /// <summary>A descrição publicada pelo Banco Central.</summary>
    public string Descricao { get; private set; } = default!;

    /// <summary>Início da vigência, quando publicado.</summary>
    public DateOnly? VigenteDe { get; private set; }

    /// <summary>Fim da vigência; nulo é vigente.</summary>
    public DateOnly? VigenteAte { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu o item (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Os tipos aceitos.</summary>
    public static readonly IReadOnlySet<string> Tipos = new HashSet<string>(StringComparer.Ordinal) { "PROGRAMA", "SUBPROGRAMA", "FONTE", "PRODUTO" };

    /// <summary>Registra um item.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando o tipo não existe ou a descrição falta.</exception>
    public static ItemDoSicor Registrar(
        string tipo, int codigo, int codigoPai, string descricao, DateOnly? vigenteDe, DateOnly? vigenteAte,
        long importadoPorId, DateTime agoraUtc)
    {
        if (!Tipos.Contains(tipo)) throw new RegraDeNegocioViolada($"Tabela do SICOR desconhecida: \"{tipo}\".");
        if (string.IsNullOrWhiteSpace(descricao)) throw new RegraDeNegocioViolada("O item do SICOR precisa de descrição.");

        return new ItemDoSicor
        {
            Tipo = tipo,
            Codigo = codigo,
            CodigoPai = codigoPai,
            Descricao = descricao.Trim(),
            VigenteDe = vigenteDe,
            VigenteAte = vigenteAte,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere contra uma nova leitura e diz se mudou.</summary>
    public bool Revisar(string descricao, DateOnly? vigenteDe, DateOnly? vigenteAte, long importadoPorId, DateTime agoraUtc)
    {
        if (string.IsNullOrWhiteSpace(descricao)) throw new RegraDeNegocioViolada("O item do SICOR precisa de descrição.");

        var mudou = Descricao != descricao.Trim() || VigenteDe != vigenteDe || VigenteAte != vigenteAte;
        if (!mudou) return false;

        Descricao = descricao.Trim();
        VigenteDe = vigenteDe;
        VigenteAte = vigenteAte;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        return true;
    }
}
