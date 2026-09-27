using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>
/// Uma linha do cadastro de metas da API Gestão de Negócios, COMO VEIO — tudo texto, antes do saneamento, como
/// <c>RegistroDoArt</c>. Número ou texto na origem viram o mesmo texto aqui: a API pode mandar <c>"05"</c> ou <c>5</c>
/// na filial, e quem decide o que vale é o saneamento, com o motivo escrito.
/// </summary>
/// <param name="Id">O identificador da linha na GN — a chave natural (1 a 1.540, sem buraco, medido em 27/09/2026).</param>
/// <param name="Mes">O mês da meta.</param>
/// <param name="FilialNumero">O NN da filial (<c>0101NN</c>).</param>
/// <param name="Linha">A linha de produto, no vocabulário do ART, mais CONSÓRCIO.</param>
/// <param name="Consultor">O consultor, <c>NOME.SOBRENOME</c>.</param>
/// <param name="Tipo">Concessão ou Direta.</param>
/// <param name="Origem">Campanha ou Consórcio.</param>
/// <param name="Quantidade">A meta em unidades.</param>
/// <param name="ValorUnitario">O valor unitário planejado.</param>
/// <param name="Margem">A margem planejada; pode vir nula.</param>
public sealed record MetaNaOrigem(
    int Id,
    string? Mes,
    string? FilialNumero,
    string? Linha,
    string? Consultor,
    string? Tipo,
    string? Origem,
    string? Quantidade,
    string? ValorUnitario,
    string? Margem);

/// <summary>O cadastro de metas inteiro, lido e conferido.</summary>
/// <param name="Linhas">As linhas, uma por <c>id</c> da GN.</param>
/// <param name="GeradaEmUtc">Quando a API gerou a resposta.</param>
/// <param name="IdadeSegundos">A idade do espelho — nula no cadastro de metas, que é da própria GN.</param>
public sealed record LeituraDasMetas(IReadOnlyList<MetaNaOrigem> Linhas, DateTime? GeradaEmUtc, int? IdadeSegundos);

/// <summary>
/// OS NOMES DOS CAMPOS DE UMA LINHA DE <c>/api/v1/cadastros/metas</c> — num lugar só.
///
/// <para><b>Provisórios em parte.</b> O envelope e <c>id</c>, <c>filial_numero</c>, <c>consultor</c>,
/// <c>quantidade</c>, <c>valor_unitario</c> e <c>margem</c> foram vistos no mapeamento de 27/09/2026. O nome do MÊS, da
/// LINHA, do TIPO e da ORIGEM não foram registrados: ficam aqui <c>mes</c> (<c>AAAA-MM</c>), <c>linha</c>, <c>tipo</c> e
/// <c>origem</c> até o Ricardo conferir a forma da resposta (o roteiro está no PR). Se o nome for outro, a leitura
/// FALHA ALTO — dizendo os campos que chegaram — e nada é gravado; a correção é trocar o texto aqui.</para>
///
/// <para><b>Obrigatórios</b> são os que a meta não existe sem: a chave, o mês, a filial, a linha, o consultor, o tipo, a
/// origem e a quantidade. O valor unitário e a margem não entram na conta de unidades — faltando, ficam nulos.</para>
/// </summary>
internal sealed class LinhaDeMetaNoJson
{
    [JsonRequired]
    [JsonPropertyName("id")]
    public JsonElement Id { get; set; }

    [JsonRequired]
    [JsonPropertyName("mes")]
    public JsonElement Mes { get; set; }

    [JsonRequired]
    [JsonPropertyName("filial_numero")]
    public JsonElement FilialNumero { get; set; }

    [JsonRequired]
    [JsonPropertyName("linha")]
    public JsonElement Linha { get; set; }

    [JsonRequired]
    [JsonPropertyName("consultor")]
    public JsonElement Consultor { get; set; }

    [JsonRequired]
    [JsonPropertyName("tipo")]
    public JsonElement Tipo { get; set; }

    [JsonRequired]
    [JsonPropertyName("origem")]
    public JsonElement Origem { get; set; }

    [JsonRequired]
    [JsonPropertyName("quantidade")]
    public JsonElement Quantidade { get; set; }

    [JsonPropertyName("valor_unitario")]
    public JsonElement? ValorUnitario { get; set; }

    [JsonPropertyName("margem")]
    public JsonElement? Margem { get; set; }
}

/// <summary>
/// A LEITURA DO CADASTRO DE METAS DA API GESTÃO DE NEGÓCIOS — <c>/api/v1/cadastros/metas</c>, inteiro, conferido.
///
/// <para><b>O cadastro é da própria GN, e não espelho</b> (medido em 27/09/2026): nasceu da planilha "Divisão Time -
/// Campanha Anual", e daqui em diante quem manda é ele. Não traz <c>idade_segundos</c> — o atraso de ~21 h vale só para
/// os painéis.</para>
///
/// <para><b>A chave de negócio NÃO é única</b>: 1.540 linhas e 1.436 combinações de (mês, filial, linha, consultor, tipo,
/// origem) — 83 grupos com 187 linhas que se SOMAM. Por isso a chave natural é o <c>id</c> da GN, e o leitor recusa id
/// repetido: dois registros com o mesmo id são uma leitura quebrada, não duas metas.</para>
///
/// <para><b>Formato diferente falha alto</b>: campo obrigatório ausente, ou id que não é inteiro, devolve o erro com os
/// NOMES dos campos que chegaram — nunca os valores, que têm nome de pessoa.</para>
/// </summary>
/// <param name="cliente">O cliente da API.</param>
public sealed class LeitorDeMetasDaGestaoDeNegocios(ClienteDaGestaoDeNegocios cliente)
{
    /// <summary>O código do sistema em <c>integracao.Sistema</c> — o mesmo da conexão.</summary>
    public const string CodigoDoSistema = "GESTAO_NEGOCIOS";

    /// <summary>A rota do cadastro de metas.</summary>
    public const string Rota = "/api/v1/cadastros/metas";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Lê o cadastro inteiro.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDasMetas>> LerAsync(CancellationToken ct)
    {
        var lida = await cliente.LerTudoAsync<JsonElement>(Rota, ct);
        if (!lida.EhSucesso) return Resultado<LeituraDasMetas>.Indisponivel(lida.Erro!);

        var convertidas = Converter(lida.Valor.Linhas);
        if (!convertidas.EhSucesso) return Resultado<LeituraDasMetas>.Indisponivel(convertidas.Erro!);

        return Resultado<LeituraDasMetas>.Ok(new LeituraDasMetas(convertidas.Valor, lida.Valor.GeradaEmUtc, lida.Valor.IdadeSegundos));
    }

    /// <summary>
    /// Converte as linhas cruas — e recusa a leitura inteira se uma delas não tiver o formato, ou se um id se repetir.
    /// </summary>
    /// <param name="linhas">As linhas como a API mandou.</param>
    public static Resultado<IReadOnlyList<MetaNaOrigem>> Converter(IReadOnlyList<JsonElement> linhas)
    {
        var metas = new List<MetaNaOrigem>(linhas.Count);
        for (var i = 0; i < linhas.Count; i++)
        {
            var bruta = linhas[i];
            LinhaDeMetaNoJson? linha;
            try
            {
                linha = bruta.ValueKind == JsonValueKind.Object ? bruta.Deserialize<LinhaDeMetaNoJson>(Json) : null;
            }
            catch (JsonException erro)
            {
                return Formato<IReadOnlyList<MetaNaOrigem>>(i, bruta, erro.Message);
            }

            if (linha is null) return Formato<IReadOnlyList<MetaNaOrigem>>(i, bruta, "a linha não é um objeto");

            if (Texto(linha.Id) is not { } textoDoId
                || !int.TryParse(textoDoId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                return Formato<IReadOnlyList<MetaNaOrigem>>(i, bruta, "o id não é um número inteiro");

            metas.Add(new MetaNaOrigem(
                id, Texto(linha.Mes), Texto(linha.FilialNumero), Texto(linha.Linha), Texto(linha.Consultor), Texto(linha.Tipo),
                Texto(linha.Origem), Texto(linha.Quantidade), Texto(linha.ValorUnitario), Texto(linha.Margem)));
        }

        var repetidos = metas.GroupBy(m => m.Id).Where(g => g.Count() > 1).Select(g => g.Key).Order().ToList();
        if (repetidos.Count > 0)
            return Resultado<IReadOnlyList<MetaNaOrigem>>.Indisponivel(
                string.Create(CultureInfo.InvariantCulture,
                    $"O cadastro de metas veio com {repetidos.Count} id(s) repetido(s) — {string.Join(", ", repetidos.Take(10))}. O id é a chave de cada meta; repetido, é leitura quebrada. Nada foi gravado."));

        return Resultado<IReadOnlyList<MetaNaOrigem>>.Ok(metas);
    }

    /// <summary>O campo como texto: string como veio, número pelo texto cru, nulo e ausente como nulo.</summary>
    private static string? Texto(JsonElement? campo) => campo switch
    {
        null => null,
        { ValueKind: JsonValueKind.String } texto => texto.GetString(),
        { ValueKind: JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False } valor => valor.GetRawText(),
        _ => null
    };

    /// <summary>A recusa por formato: a posição, o motivo e os NOMES dos campos que chegaram — nunca os valores.</summary>
    private static Resultado<T> Formato<T>(int posicao, JsonElement bruta, string motivo)
    {
        var recebidos = bruta.ValueKind == JsonValueKind.Object
            ? string.Join(", ", bruta.EnumerateObject().Select(p => p.Name))
            : bruta.ValueKind.ToString();

        return Resultado<T>.Indisponivel(
            string.Create(CultureInfo.InvariantCulture,
                $"A linha {posicao + 1} de {Rota} não tem o formato esperado ({motivo}). Campos recebidos: {recebidos}. A API pode ter mudado de formato, ou os nomes em LinhaDeMetaNoJson precisam ser conferidos — nada foi gravado."));
    }
}
