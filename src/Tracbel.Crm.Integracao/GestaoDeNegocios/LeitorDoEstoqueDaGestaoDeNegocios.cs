using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>Uma máquina do painel "Estoque &amp; Pedidos", COMO VEIO — tudo texto. O cliente do atendimento e o custo não são lidos.</summary>
public sealed record EquipamentoNaOrigem(
    string? Chaint, string? Chassi, string? Pedido, string? Comar, string? Filial, string? Situacao, string? Grupo, string? Descricao,
    string? Configuracao, string? Tipo, string? NovoOuUsado, string? Ano, string? EntradaEm, string? ChegadaPrevistaEm,
    string? FaturamentoPrevistoEm, string? Pago, string? Reservado, string? SituacaoNaFabrica);

/// <summary>Um item da cobertura, COMO VEIO.</summary>
/// <param name="Recorte"><c>MES</c> ou <c>GRUPO</c>.</param>
/// <param name="Chave">O mês (<c>Ago/2026</c>) ou o grupo.</param>
/// <param name="Meses">Os meses de estoque.</param>
/// <param name="Vendas">As vendas do período.</param>
public sealed record ItemDaCoberturaNaOrigem(string Recorte, string? Chave, string? Meses, string? Vendas);

/// <summary>O estoque inteiro, lido e conferido.</summary>
/// <param name="Equipamentos">As máquinas do estoque e os pedidos à fábrica.</param>
/// <param name="Cobertura">A cobertura, por mês e por grupo.</param>
/// <param name="Filiais">O de-para das lojas.</param>
/// <param name="GeradaEmUtc">Quando a API gerou o estoque.</param>
/// <param name="CoberturaGeradaEmUtc">Quando a API calculou a cobertura.</param>
public sealed record LeituraDoEstoqueNaOrigem(
    IReadOnlyList<EquipamentoNaOrigem> Equipamentos,
    IReadOnlyList<ItemDaCoberturaNaOrigem> Cobertura,
    IReadOnlyList<FilialDaGestao> Filiais,
    DateTime? GeradaEmUtc,
    DateTime? CoberturaGeradaEmUtc);

/// <summary>
/// A LEITURA DO ESTOQUE E DA COBERTURA DA API GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026). Só GET.
///
/// <para><b>O estoque é lido SEM janela de datas.</b> O painel tem período pela data de entrada, e o pedido à fábrica ainda
/// não entrou: com <c>data_de</c>/<c>data_ate</c> ele devolve 459 linhas e perde os 229 pedidos; sem nada, devolve as 688
/// (medido em 28/09/2026). Se a GN passar a aplicar um período padrão, a leitura encolhe — e a trava de remoção da carga
/// aborta a rodada.</para>
///
/// <para><b>Formato diferente falha alto</b>: campo obrigatório ausente devolve o erro com os NOMES dos campos que
/// chegaram — nunca os valores.</para>
/// </summary>
/// <param name="cliente">O cliente da API.</param>
public sealed class LeitorDoEstoqueDaGestaoDeNegocios(ClienteDaGestaoDeNegocios cliente)
{
    /// <summary>A rota do estoque — sem janela de datas, de propósito.</summary>
    public const string RotaDoEstoque = "/api/v1/paineis/estoque-pedidos";

    /// <summary>A rota da cobertura.</summary>
    public const string RotaDaCobertura = "/api/v1/cobertura";

    /// <summary>Lê as três rotas; uma que falhe derruba a leitura inteira — nada é gravado pela metade.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDoEstoqueNaOrigem>> LerAsync(CancellationToken ct)
    {
        var estoque = await cliente.LerTudoAsync<JsonElement>(RotaDoEstoque, ct);
        if (!estoque.EhSucesso) return Resultado<LeituraDoEstoqueNaOrigem>.Indisponivel(estoque.Erro!);

        var cobertura = await cliente.LerDocumentoAsync(RotaDaCobertura, ct);
        if (!cobertura.EhSucesso) return Resultado<LeituraDoEstoqueNaOrigem>.Indisponivel(cobertura.Erro!);

        var filiais = await cliente.LerDocumentoAsync(FiliaisDaGestaoDeNegocios.Rota, ct);
        if (!filiais.EhSucesso) return Resultado<LeituraDoEstoqueNaOrigem>.Indisponivel(filiais.Erro!);

        var convertido = Converter(estoque.Valor.Linhas, cobertura.Valor, filiais.Valor);
        return convertido.EhSucesso
            ? Resultado<LeituraDoEstoqueNaOrigem>.Ok(convertido.Valor with { GeradaEmUtc = estoque.Valor.GeradaEmUtc })
            : convertido;
    }

    /// <summary>Converte as três respostas — e recusa a leitura inteira se uma delas não tiver o formato.</summary>
    /// <param name="estoque">As linhas do estoque.</param>
    /// <param name="cobertura">O documento da cobertura.</param>
    /// <param name="filiais">O documento do de-para das lojas.</param>
    public static Resultado<LeituraDoEstoqueNaOrigem> Converter(IReadOnlyList<JsonElement> estoque, JsonElement cobertura, JsonElement filiais)
    {
        var equipamentos = new List<EquipamentoNaOrigem>(estoque.Count);
        for (var i = 0; i < estoque.Count; i++)
        {
            var l = estoque[i];
            if (Faltando(l, "chaint", "filial", "sit_equipamento", "grupo", "descricao", "novo_usado", "maquina_ams_implemento", "pago", "reservado") is { } falta)
                return Formato(RotaDoEstoque, $"A linha {i + 1}", l, falta);
            equipamentos.Add(new EquipamentoNaOrigem(
                Texto(l, "chaint"), Texto(l, "chassis"), Texto(l, "pedido"), Texto(l, "comar"), Texto(l, "filial"), Texto(l, "sit_equipamento"),
                Texto(l, "grupo"), Texto(l, "descricao"), Texto(l, "config"), Texto(l, "maquina_ams_implemento"), Texto(l, "novo_usado"),
                Texto(l, "ano"), Texto(l, "dt_entrada"), Texto(l, "dt_fdd"), Texto(l, "dt_prev_fat"), Texto(l, "pago"), Texto(l, "reservado"),
                Texto(l, "sit_fabrica")));
        }

        var itens = new List<ItemDaCoberturaNaOrigem>();
        foreach (var (campo, recorte) in new[] { ("por_mes", "MES"), ("por_grupo", "GRUPO") })
        {
            if (cobertura.ValueKind != JsonValueKind.Object || !cobertura.TryGetProperty(campo, out var bloco)
                || bloco.ValueKind != JsonValueKind.Object || !bloco.TryGetProperty("itens", out var lista) || lista.ValueKind != JsonValueKind.Array)
                return Formato(RotaDaCobertura, $"O bloco \"{campo}\"", cobertura, "falta a lista \"itens\"");

            var posicao = 0;
            foreach (var item in lista.EnumerateArray())
            {
                posicao++;
                if (Faltando(item, "chave", "meses", "vendas") is { } falta)
                    return Formato(RotaDaCobertura, $"O item {posicao} de \"{campo}\"", item, falta);
                itens.Add(new ItemDaCoberturaNaOrigem(recorte, Texto(item, "chave"), Texto(item, "meses"), Texto(item, "vendas")));
            }
        }

        var lojas = FiliaisDaGestaoDeNegocios.Converter(filiais);
        if (!lojas.EhSucesso) return Resultado<LeituraDoEstoqueNaOrigem>.Indisponivel(lojas.Erro!);

        DateTime? coberturaGerada = Texto(cobertura, "gerado_em") is { } gerado
                                    && DateTime.TryParse(gerado, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var instante)
            ? instante.ToUniversalTime()
            : null;

        return Resultado<LeituraDoEstoqueNaOrigem>.Ok(new LeituraDoEstoqueNaOrigem(equipamentos, itens, lojas.Valor, null, coberturaGerada));
    }

    private static string? Texto(JsonElement linha, string campo) =>
        linha.ValueKind == JsonValueKind.Object && linha.TryGetProperty(campo, out var valor)
            ? valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => valor.GetRawText(),
                _ => null
            }
            : null;

    private static string? Faltando(JsonElement linha, params string[] campos)
    {
        if (linha.ValueKind != JsonValueKind.Object) return "não é um objeto";
        var ausente = campos.FirstOrDefault(c => !linha.TryGetProperty(c, out _));
        return ausente is null ? null : $"falta o campo \"{ausente}\"";
    }

    private static string Campos(JsonElement linha) =>
        linha.ValueKind == JsonValueKind.Object ? string.Join(", ", linha.EnumerateObject().Select(p => p.Name)) : linha.ValueKind.ToString();

    private static Resultado<LeituraDoEstoqueNaOrigem> Formato(string rota, string onde, JsonElement linha, string motivo) =>
        Resultado<LeituraDoEstoqueNaOrigem>.Indisponivel(
            string.Create(CultureInfo.InvariantCulture,
                $"{onde} de {rota} não tem o formato esperado ({motivo}). Campos recebidos: {Campos(linha)}. A API pode ter mudado de formato — nada foi gravado."));
}
