using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O POTENCIAL NA MONTAGEM DOS INDICADORES (plano 2 do documento 54): o total do recorte, o detalhe por categoria e a
/// relevância contra São Paulo. Conta em memória, sobre o que o potencial de referência já calculou.
/// </summary>
internal static class PotencialNaMontagem
{
    /// <summary>
    /// O POTENCIAL DO RECORTE CONSULTADO (issue 72) — o total do cabeçalho e o detalhe por categoria.
    ///
    /// <para><b>O total é a soma dos municípios</b>, e não o motor rodado sobre as áreas somadas: o
    /// compartilhamento de terra acontece dentro do município. Some a coluna da tabela e dá este número.</para>
    /// </summary>
    /// <param name="categorias">As categorias de máquina em jogo, na ordem da tela.</param>
    /// <param name="porCategoria">O resultado de cada município, por categoria.</param>
    /// <param name="totaisDosMunicipios">O total de cada município, já com as categorias sobrepostas.</param>
    /// <param name="relevancia">A fatia do recorte em São Paulo.</param>
    /// <param name="relevanciaPorCultura">A fatia e a produtividade de cada cultura.</param>
    internal static PotencialDoRecorteNoMapa MontarPotencialDoRecorte(
        IReadOnlyList<(string Codigo, string Nome)> categorias,
        IReadOnlyDictionary<string, List<PotencialDoRecorte>> porCategoria,
        IReadOnlyList<PotencialDoRecorte> totaisDosMunicipios,
        RelevanciaNoEstado? relevancia,
        IReadOnlyList<RelevanciaDaCultura> relevanciaPorCultura)
    {
        var total = MotorDoPotencial.Somar(totaisDosMunicipios);

        return new PotencialDoRecorteNoMapa(
            total.Parque,
            total.DemandaAnual,
            total.AreaUtilHectares,
            total.Estimativa,
            total.MotivoSemParque,
            total.MotivoSemDemanda,
            MotorDoPotencial.Frase(total),
            totaisDosMunicipios.Count(m => m.Parque is not null),
            total.Parcelas,
            [
                .. categorias.Select(categoria =>
                {
                    var somado = MotorDoPotencial.Somar(porCategoria[categoria.Codigo]);
                    return new PotencialPorCategoria(
                        categoria.Codigo, categoria.Nome, somado.Parque, somado.DemandaAnual, somado.AreaUtilHectares,
                        somado.Estimativa, somado.MotivoSemParque, somado.MotivoSemDemanda,
                        MotorDoPotencial.Frase(somado), somado.Parcelas);
                })
            ],
            relevancia,
            relevanciaPorCultura);
    }

    /// <summary>
    /// A FATIA DO RECORTE NA LAVOURA DE SÃO PAULO — área plantada, área colhida e valor da produção.
    ///
    /// <para><b>O denominador é o que o IBGE publica para a UF</b> (issue 155), e não a soma dos
    /// municípios: o município sigiloso entra no total do estado sem aparecer embaixo.</para>
    ///
    /// <para><b>A quantidade não entra no total</b>, de propósito: cada produto vem na unidade dele, e
    /// somar tonelada com mil frutos não daria número nenhum. Ela aparece por cultura.</para>
    /// </summary>
    /// <param name="codigos">Os municípios que passaram pelo filtro.</param>
    /// <param name="producao">A lavoura inteira de cada município.</param>
    /// <param name="estado">Os totais publicados para São Paulo.</param>
    internal static RelevanciaNoEstado? RelevanciaDoRecorte(
        IReadOnlyList<int> codigos,
        IReadOnlyDictionary<int, ProducaoAgricolaDoMunicipio> producao,
        TotaisDoEstado? estado)
    {
        if (estado is null) return null;

        var doRecorte = codigos.Select(producao.GetValueOrDefault).Where(p => p is not null).ToList();

        decimal? Somar(Func<ProducaoAgricolaDoMunicipio, decimal?> campo)
        {
            var valores = doRecorte.Select(p => campo(p!)).Where(v => v is not null).ToList();
            return valores.Count == 0 ? null : valores.Sum(v => v!.Value);
        }

        return MotorDoPotencial.Relevancia(
            new MedidasDaLavoura(
                Somar(p => p.AreaPlantadaHectares), Somar(p => p.AreaColhidaHectares), null, Somar(p => p.ValorDaProducaoMilReais)),
            new MedidasDaLavoura(
                estado.AreaPlantadaHectares, estado.AreaColhidaHectares, null, estado.ValorDaProducaoMilReais));
    }

    /// <summary>
    /// A RELEVÂNCIA DE CADA CULTURA CONTRA SÃO PAULO — a aba "Relevância vs SP" do protótipo.
    ///
    /// <para><b>Aqui a quantidade entra</b>, porque os dois lados são o mesmo produto: a unidade é a
    /// mesma, e a razão de produtividade diz se a terra daqui rende mais que a média do estado.</para>
    /// </summary>
    /// <param name="codigos">Os municípios que passaram pelo filtro.</param>
    /// <param name="daRegra">As medidas de cada cultura em cada município, no ano de cada uma.</param>
    /// <param name="culturasNoEstado">As mesmas culturas no total publicado do estado.</param>
    internal static List<RelevanciaDaCultura> RelevanciaPorCultura(
        IReadOnlyList<int> codigos,
        IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio> daRegra,
        IReadOnlyList<CulturaNoEstado> culturasNoEstado)
    {
        var lista = new List<RelevanciaDaCultura>();

        foreach (var noEstado in culturasNoEstado)
        {
            var medidas = codigos
                .Select(codigo => daRegra.TryGetValue((codigo, noEstado.ProdutoCodigoIbge), out var m) ? m : (MedidasDaCulturaNoMunicipio?)null)
                .Where(m => m is not null)
                .Select(m => m!.Value)
                .ToList();

            decimal? Somar(Func<MedidasDaCulturaNoMunicipio, decimal?> campo)
            {
                var valores = medidas.Select(campo).Where(v => v is not null).ToList();
                return valores.Count == 0 ? null : valores.Sum(v => v!.Value);
            }

            var aqui = new MedidasDaLavoura(
                Somar(m => m.AreaPlantadaHectares), Somar(m => m.AreaColhidaHectares),
                Somar(m => m.QuantidadeProduzida), Somar(m => m.ValorDaProducaoMilReais));

            var emSaoPaulo = new MedidasDaLavoura(
                noEstado.AreaPlantadaHectares, noEstado.AreaColhidaHectares,
                noEstado.QuantidadeProduzida, noEstado.ValorDaProducaoMilReais);

            lista.Add(new RelevanciaDaCultura(
                noEstado.ProdutoCodigoIbge, noEstado.ProdutoNome, noEstado.Ano,
                noEstado.UnidadeDaQuantidade, noEstado.UnidadeDaProdutividade,
                aqui, emSaoPaulo, MotorDoPotencial.Relevancia(aqui, emSaoPaulo)));
        }

        return lista;
    }
}
