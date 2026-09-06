using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A gravação das VENDAS PERDIDAS — e o preenchimento do catálogo de motivos, que até aqui
/// tinha uma linha só: "não informado na origem".
///
/// <para><b>Quatro catálogos nascem desta carga</b>, e nenhum deles com valor escrito à mão:
/// motivo de perda, concorrente, tipo de equipamento e revenda concorrente saem dos valores que
/// a operação de fato usou no formulário. É a regra do projeto — nada de dado solto que a
/// pessoa digita — aplicada na entrada: <c>Codificar</c> tira acento e caixa, de modo que
/// "Condição comercial" e "Condição Comercial" viram <b>um</b> item de catálogo, e não dois.</para>
///
/// <para><b>O concorrente reusa o catálogo do processo.</b> <c>Processo.ConcorrenteId</c> já
/// aponta para o catálogo de sistema <c>CONCORRENTE</c>, e "New Holland" tem de ser a mesma
/// linha nos dois lugares — senão o relatório de para-quem-perdemos não fecha com a ficha do
/// processo.</para>
///
/// <para><b>A resposta entra mesmo sem processo.</b> O formulário aponta o processo, mas o
/// processo pode estar fora do recorte carregado; nesse caso a filial vem do cliente. Descartar
/// a resposta por isso seria descartar o motivo da perda — que é o que esta carga existe para
/// trazer.</para>
/// </summary>
internal sealed partial class CargaDeProcessoDoVortice
{
    private const string FluxoDeVendaPerdida = "VORTICE.CARGA.VENDA_PERDIDA";

    private async Task<int> GravarVendasPerdidasAsync(
        int sistemaId,
        IReadOnlyList<VendaPerdidaParaCarga> vendas,
        IReadOnlyDictionary<string, long> porChaveDeProcesso,
        IReadOnlyDictionary<string, long> porChaveDeCliente,
        int motivoNaoInformadoId,
        CancellationToken ct)
    {
        if (vendas.Count == 0) return 0;

        var mapa = await MapaDeChavesAsync(sistemaId, nameof(VendaPerdida), ct);
        var empresaPorCliente = await MapaDeEmpresaPorClienteAsync(ct);

        var porMotivo = await GarantirMotivosDePerdaAsync(vendas, ct);
        var porConcorrente = await GarantirItensDeCatalogoAsync(
            CatalogosDeSistema.Concorrente, vendas.Select(v => v.MarcaDoConcorrente), ct);
        var porTipo = await GarantirItensDeCatalogoAsync(
            CatalogosDeSistema.TipoDeEquipamento, vendas.Select(v => v.TipoDoEquipamento), ct);
        var porRevenda = await GarantirItensDeCatalogoAsync(
            CatalogosDeSistema.RevendaConcorrente, vendas.Select(v => v.RevendaDoConcorrente), ct);

        var gravadas = 0;
        var semLugar = 0;

        foreach (var bloco in vendas.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novas = new List<(string Chave, VendaPerdida Entidade)>();

            foreach (var venda in bloco)
            {
                if (mapa.ContainsKey(venda.ChaveDeOrigem)) continue;

                var processoId = venda.ChaveDoProcessoNaOrigem is { } chaveProcesso
                                 && porChaveDeProcesso.TryGetValue(chaveProcesso, out var achadoProcesso)
                    ? achadoProcesso
                    : (long?)null;

                var clienteId = venda.ChaveDoClienteNaOrigem is { } chaveCliente
                                && porChaveDeCliente.TryGetValue(chaveCliente, out var achadoCliente)
                    ? achadoCliente
                    : (long?)null;

                // A FILIAL VEM DO PROCESSO, e o cliente é o desempate. Sem nenhum dos dois a
                // resposta não tem onde morar: a filial é a fronteira de acesso do modelo, e uma
                // linha sem ela seria visível para todo mundo ou para ninguém.
                int? empresaId = null;
                if (venda.CodigoDaFilialNaOrigem is { } codigo
                    && deParaDeFiliais.TryGetValue(codigo, out var daFilial))
                    empresaId = daFilial;
                else if (clienteId is { } dono && empresaPorCliente.TryGetValue(dono, out var doCliente))
                    empresaId = doCliente;

                if (empresaId is null)
                {
                    semLugar++;
                    Recusar(nameof(VendaPerdida), "Sem filial para situar a resposta",
                        venda.ChaveDeOrigem,
                        "A resposta não aponta processo desta carga nem cliente carregado, e a " +
                        "filial é a fronteira de acesso de toda linha do modelo.", venda);
                    continue;
                }

                var entidade = VendaPerdida.Criar(
                    empresaId: empresaId.Value,
                    registradaEm: venda.RegistradaEm,
                    motivoDePerdaId: Chave(venda.Motivo) is { Length: > 0 } motivo
                                     && porMotivo.TryGetValue(motivo, out var motivoId)
                        ? motivoId
                        : motivoNaoInformadoId,
                    processoId: processoId,
                    clienteId: clienteId,
                    ocorridaEm: venda.OcorridaEm,
                    tipoDeEquipamentoId: Achar(porTipo, venda.TipoDoEquipamento),
                    concorrenteId: Achar(porConcorrente, venda.MarcaDoConcorrente),
                    revendaDoConcorrenteId: Achar(porRevenda, venda.RevendaDoConcorrente),
                    modeloDoConcorrente: venda.ModeloDoConcorrente,
                    modeloOfertado: venda.ModeloOfertado,
                    quantidade: venda.Quantidade,
                    precoDoConcorrente: venda.PrecoDoConcorrente,
                    precoOfertado: venda.PrecoOfertado,
                    participamosDaNegociacao: venda.ParticipamosDaNegociacao,
                    registradaPor: venda.RegistradaPor);

                contexto.VendasPerdidas.Add(entidade);
                novas.Add((venda.ChaveDeOrigem, entidade));
            }

            if (novas.Count > 0)
            {
                await contexto.SaveChangesAsync(ct);

                foreach (var (chave, entidade) in novas)
                {
                    contexto.ChavesExternas.Add(
                        ChaveExterna.Criar(sistemaId, nameof(VendaPerdida), entidade.Id, chave));
                    mapa[chave] = entidade.Id;
                }

                await contexto.SaveChangesAsync(ct);
                gravadas += novas.Count;
            }

            await transacao.CommitAsync(ct);
        }

        if (semLugar > 0) _decisoes[$"{nameof(VendaPerdida)} sem filial"] = semLugar;

        return gravadas;
    }

    /// <summary>O item de catálogo de um texto da origem, quando ele existe.</summary>
    private static int? Achar(IReadOnlyDictionary<string, int> catalogo, string? texto) =>
        Chave(texto) is { Length: > 0 } chave && catalogo.TryGetValue(chave, out var id) ? id : null;

    /// <summary>O código estável de um texto da origem — sem acento, sem caixa, sem espaço duplo.</summary>
    private static string Chave(string? texto) =>
        Tracbel.Crm.Integracao.Carga.SaneamentoDeProcesso.Codificar(texto, 40);

    // =============================================================================================
    // Os quatro catálogos, tirados do que a operação usou
    // =============================================================================================

    private async Task<Dictionary<string, int>> GarantirMotivosDePerdaAsync(
        IReadOnlyList<VendaPerdidaParaCarga> vendas, CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var existentes = await contexto.MotivosDePerda
            .ToDictionaryAsync(m => m.Codigo, m => m, StringComparer.Ordinal, ct);

        var novos = 0;
        foreach (var (codigo, nome) in Distintos(vendas.Select(v => v.Motivo)))
        {
            if (existentes.ContainsKey(codigo)) continue;
            contexto.MotivosDePerda.Add(MotivoDePerda.Criar(codigo, nome, CategoriaDe(codigo)));
            novos++;
        }

        if (novos > 0) await contexto.SaveChangesAsync(ct);

        return await contexto.MotivosDePerda.AsNoTracking()
            .ToDictionaryAsync(m => m.Codigo, m => m.Id, StringComparer.Ordinal, ct);
    }

    /// <summary>
    /// A que família o motivo pertence.
    ///
    /// <para>A classificação é nossa e não da origem — o formulário do Vórtice tem só o texto.
    /// Ela existe para a tela poder agrupar catorze motivos em seis famílias sem que alguém
    /// tenha de decidir isso de novo a cada relatório. O que não casa cai em
    /// <see cref="CategoriaDeMotivoDePerda.Outro"/>, que é a resposta honesta para um motivo
    /// novo que ninguém classificou ainda.</para>
    /// </summary>
    private static CategoriaDeMotivoDePerda CategoriaDe(string codigo) => codigo switch
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
    /// Garante os itens de um catálogo de sistema a partir dos valores que a origem usou.
    ///
    /// <para>Um item por valor distinto, e nada é apagado: o catálogo cresce com o que a
    /// operação escreveu e continua administrável pela tela de Taxonomias. Rodar a carga de novo
    /// não duplica — a chave é o código, que já vem sem acento e sem caixa.</para>
    /// </summary>
    private async Task<Dictionary<string, int>> GarantirItensDeCatalogoAsync(
        int catalogoId, IEnumerable<string?> textos, CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var existentes = await contexto.CatalogoItens
            .Where(i => i.CatalogoId == catalogoId)
            .ToDictionaryAsync(i => i.Codigo, i => i.Id, StringComparer.Ordinal, ct);

        var novos = 0;
        foreach (var (codigo, nome) in Distintos(textos))
        {
            if (existentes.ContainsKey(codigo)) continue;
            contexto.CatalogoItens.Add(CatalogoItem.Criar(catalogoId, codigo, nome));
            novos++;
        }

        if (novos > 0) await contexto.SaveChangesAsync(ct);

        return await contexto.CatalogoItens.AsNoTracking()
            .Where(i => i.CatalogoId == catalogoId)
            .ToDictionaryAsync(i => i.Codigo, i => i.Id, StringComparer.Ordinal, ct);
    }

    /// <summary>
    /// Os valores distintos de uma coluna de texto da origem, com código e nome de exibição.
    ///
    /// <para>O código é quem colapsa a variação de digitação; o nome guardado é a primeira grafia
    /// vista, que é a que a operação reconhece na tela.</para>
    /// </summary>
    private static IEnumerable<(string Codigo, string Nome)> Distintos(IEnumerable<string?> textos)
    {
        var vistos = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var texto in textos)
        {
            var limpo = texto?.Trim();
            if (string.IsNullOrEmpty(limpo)) continue;

            var codigo = Chave(limpo);
            if (codigo.Length == 0 || vistos.ContainsKey(codigo)) continue;

            vistos[codigo] = limpo;
        }

        return vistos.Select(p => (p.Key, p.Value));
    }
}
