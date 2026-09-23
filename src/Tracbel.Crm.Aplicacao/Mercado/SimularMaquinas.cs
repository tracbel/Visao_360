using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Potencial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// A CALCULADORA DE MÁQUINAS (issue 161) — "quantas máquinas esta área comporta, e quantas por ano".
///
/// <para><b>Ela não tem fórmula própria.</b> O critério de aceite da issue é "a mesma área de um
/// município devolve o mesmo resultado que o motor gravou para ele", e a única forma de isso continuar
/// verdadeiro depois da primeira mudança é <b>ser a mesma função</b>: a simulação troca o número da área
/// e chama <see cref="MotorDoPotencial.Simular"/>, que chama <see cref="MotorDoPotencial.Potencial"/>.
/// Sem área informada, o resultado é, literalmente, o do mapa.</para>
///
/// <para><b>A data manda nas vigências.</b> Simular com a data de março usa a regra que valia em março —
/// é o que a issue 71 construiu, e é por isso que a calculadora não guarda parâmetro nenhum.</para>
///
/// <para><b>Nada é gravado.</b> Simulação é pergunta, não registro; o que ficaria gravado seria uma
/// resposta que ninguém decidiu adotar.</para>
///
/// <para><b>É dado público</b> — IBGE e parâmetros do CRM —, o mesmo para todas as filiais: não passa
/// pela fronteira de multiempresa.</para>
/// </summary>
/// <param name="motor">As regras vigentes e a área medida.</param>
/// <param name="acesso">O contexto de acesso.</param>
/// <param name="relogio">O relógio.</param>
public sealed class SimularMaquinas(
    IRepositorioDoMotorDoPotencial motor,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>
    /// O QUE A CALCULADORA AINDA NÃO FAZ, e por quê.
    ///
    /// <para>A issue pede também "demanda ajustada e os três cenários". O ajuste vem do fator de ciclo de
    /// mercado (issue 74), que pesa preço, crédito e percepção do gestor — e <b>os pesos são a decisão
    /// D-P05, que não saiu</b>, assim como as bandas dos cenários (D-IM-05). Inventar um peso para
    /// entregar três números faria a calculadora responder com opinião do programador; a regra desta
    /// etapa é a mesma do resto: sem parâmetro decidido, o resultado diz o que falta.</para>
    /// </summary>
    public const string SobreOsCenarios =
        "O ajuste por cenário de mercado ainda não entra nesta conta: o fator de ciclo depende dos pesos " +
        "de preço, crédito e percepção do gestor, que ainda não foram decididos (D-P05), e das bandas dos " +
        "cenários (D-IM-05). O que está aqui é o potencial estrutural — o que a área comporta.";

    /// <summary>Simula o parque e a demanda anual.</summary>
    /// <param name="entrada">O município, a categoria, a data e as áreas informadas.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<ResultadoDaCalculadora>>> ExecutarAsync(
        SimulacaoDeMaquinas entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.TerritorioLer))
            return LeituraDeParametro.SemPermissao<ComProcedencia<ResultadoDaCalculadora>>(Permissoes.TerritorioLer);

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        // SEM DATA, É HOJE — e "hoje" é o dia de São Paulo, não o do relógio UTC.
        var data = string.IsNullOrWhiteSpace(entrada.Data)
            ? ParametroComVigencia.HojeNoBrasil(agora)
            : LeituraDeParametro.Data(erros, "data", entrada.Data, obrigatoria: true);

        var municipio = string.IsNullOrWhiteSpace(entrada.MunicipioCodigoIbge)
            ? null
            : LeituraDeParametro.Inteiro(erros, "municipioCodigoIbge", entrada.MunicipioCodigoIbge, "o código IBGE do município");

        var informadas = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var (item, indice) in (entrada.Areas ?? []).Select((a, i) => (a, i)))
        {
            var campo = $"areas[{indice}]";
            var codigo = erros.Obrigatorio($"{campo}.culturaCodigo", item.CulturaCodigo, "o código da cultura");
            var area = LeituraDeParametro.Numero(erros, $"{campo}.areaHectares", item.AreaHectares, true, "a área em hectares");

            if (area is < 0)
                erros.Registrar($"{campo}.areaHectares", "A área não pode ser negativa.", item.AreaHectares);

            // ÁREA GRANDE DEMAIS NÃO É SIMULAÇÃO, É ENGANO DE DIGITAÇÃO: São Paulo inteiro tem cerca de
            // 9,2 milhões de hectares plantados, e 25 milhões é a área territorial do estado.
            if (area is > 25_000_000m)
                erros.Registrar($"{campo}.areaHectares", "A área vai até 25.000.000 ha — o estado de São Paulo inteiro.", item.AreaHectares);

            if (codigo.Length > 0 && !informadas.TryAdd(codigo, area ?? 0m))
                erros.Registrar($"{campo}.culturaCodigo", $"A cultura {codigo} foi informada duas vezes.", item.CulturaCodigo);
        }

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<ResultadoDaCalculadora>>("A simulação tem campos a corrigir.");

        var catalogo = await motor.LerCatalogoAsync(data!.Value, ct);

        if (catalogo.Regras.Count == 0)
            return Resultado<ComProcedencia<ResultadoDaCalculadora>>.Falha(
                $"Não há regra de potencial vigente em {data:dd/MM/yyyy}. Cadastre a regra da cultura no Administrador " +
                "antes de simular — sem hectares por máquina, não há o que dividir.");

        var doCatalogo = catalogo.Regras
            .Select(r => r.CulturaCodigo)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // CULTURA SEM REGRA É RECUSA COM NOME, e não uma linha vazia no resultado: quem simulou digitou
        // um código e precisa saber que aquela cultura não tem parâmetro, não que "deu zero".
        foreach (var codigo in informadas.Keys.Where(c => !doCatalogo.Contains(c)).Order(StringComparer.Ordinal))
            erros.Registrar(
                "areas",
                $"A cultura {codigo} não tem regra de hectares por máquina vigente em {data:dd/MM/yyyy}. " +
                "Cadastre a regra no Administrador, ou simule uma das culturas que têm.",
                codigo);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<ResultadoDaCalculadora>>("A simulação tem campos a corrigir.");

        AreasDoMunicipio? medidas = null;
        if (municipio is { } codigoIbge)
        {
            medidas = await motor.LerAreasDoMunicipioAsync(codigoIbge, catalogo, ct);

            if (medidas is null)
                return Resultado<ComProcedencia<ResultadoDaCalculadora>>.NaoEncontrado(
                    $"Não há município com o código IBGE {codigoIbge} no catálogo.");
        }

        var regras = string.IsNullOrWhiteSpace(entrada.CategoriaCodigo)
            ? catalogo.Regras
            : [.. catalogo.Regras.Where(r => string.Equals(r.CategoriaCodigo, entrada.CategoriaCodigo, StringComparison.OrdinalIgnoreCase))];

        if (regras.Count == 0)
            return Resultado<ComProcedencia<ResultadoDaCalculadora>>.NaoEncontrado(
                $"Não há regra de potencial da categoria {entrada.CategoriaCodigo} vigente em {data:dd/MM/yyyy}.");

        var porCategoria = new List<CategoriaNaCalculadora>();
        var resultados = new List<PotencialDoRecorte>();

        foreach (var categoria in regras.GroupBy(r => (r.CategoriaCodigo, r.CategoriaNome)).OrderBy(g => g.Key.CategoriaCodigo, StringComparer.Ordinal))
        {
            var doRecorte = categoria
                .Select(r => new CulturaNoRecorte(
                    r.CulturaCodigo,
                    r.CulturaNome,
                    medidas?.AreaPorCultura.GetValueOrDefault(r.CulturaCodigo),
                    r.HectaresPorMaquina,
                    r.AnosDeRenovacao,
                    r.Confirmada,
                    r.GrupoCodigo))
                .ToList();

            // AQUI ESTÁ O ACEITE DA ISSUE: sem área informada, `Simular` com o dicionário vazio é
            // `Potencial` sobre a área medida — exatamente o que o mapa mostra para o município.
            var resultado = MotorDoPotencial.Simular(doRecorte, informadas);
            resultados.Add(resultado);

            porCategoria.Add(new CategoriaNaCalculadora(
                categoria.Key.CategoriaCodigo,
                categoria.Key.CategoriaNome,
                resultado.Parque,
                resultado.DemandaAnual,
                resultado.AreaUtilHectares,
                resultado.Estimativa,
                resultado.MotivoSemParque,
                resultado.MotivoSemDemanda,
                MotorDoPotencial.Frase(resultado),
                resultado.Parcelas));
        }

        // AS CATEGORIAS SE SOBREPÕEM NO MESMO CHÃO: as máquinas somam, a terra não.
        var total = MotorDoPotencial.Sobrepor(resultados);

        var culturas = regras
            .OrderBy(r => r.CategoriaCodigo, StringComparer.Ordinal)
            .ThenBy(r => r.CulturaCodigo, StringComparer.Ordinal)
            .Select(r => new CulturaNaCalculadora(
                r.CulturaCodigo,
                r.CulturaNome,
                r.CategoriaCodigo,
                r.CategoriaNome,
                r.HectaresPorMaquina,
                r.AnosDeRenovacao,
                r.Confirmada,
                medidas?.AreaPorCultura.GetValueOrDefault(r.CulturaCodigo),
                medidas?.AnoPorCultura.TryGetValue(r.CulturaCodigo, out var ano) == true ? ano : null,
                informadas.TryGetValue(r.CulturaCodigo, out var informada) ? informada : null))
            .ToList();

        var dados = new ResultadoDaCalculadora(
            data.Value,
            medidas?.CodigoIbge,
            medidas?.Nome,
            total.Parque,
            total.DemandaAnual,
            total.AreaUtilHectares,
            total.Estimativa,
            total.MotivoSemParque,
            total.MotivoSemDemanda,
            MotorDoPotencial.Frase(total),
            total.Parcelas,
            porCategoria,
            culturas,
            SobreOsCenarios);

        return Resultado<ComProcedencia<ResultadoDaCalculadora>>.Ok(
            ComProcedencia<ResultadoDaCalculadora>.DoNossoBanco(
                dados,
                "organizacao.RegraDePotencial · organizacao.Cultura · organizacao.GrupoDeCompartilhamento · organizacao.ProducaoAgricolaNoMunicipio",
                relogio));
    }
}
