using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Potencial;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// A GESTÃO DE FINANCIAMENTOS — o mercado de crédito de mecanização do SICOR por município e por loja, no período, produto
/// e programa escolhidos (issue 261, a aba "Gestão de Financiamentos" do protótipo da pasta 360).
///
/// <para><b>A conta é a do índice de crédito do CRM</b> (<see cref="IndicadoresDeMercado.Credito"/>): 70% da variação das
/// linhas e 30% da do valor — o "IAM" do protótipo, com o mesmo peso —, e as faixas decididas do CRM (retraído, intermediária,
/// aquecido, superaquecido), e não os cortes de ±5% e ±20% do protótipo (princípio 3 do plano do épico 264).</para>
///
/// <para><b>A comparação é sempre com o mesmo período do ano anterior</b>, como o protótipo: respeita a sazonalidade da
/// safra. O período padrão são os 12 meses que terminam no último mês do SICOR descontada a carência — a mesma janela do
/// painel do crédito.</para>
///
/// <para><b>O share Tracbel × concorrência não está aqui</b>: é a issue 262, no painel do crédito dos Indicadores. E os
/// contratos individuais não existem no CRM — o SICOR que o Banco Central publica é agregado por município e produto.</para>
/// </summary>
public sealed class ObterFinanciamentosDoSicor(
    IRepositorioDosFinanciamentos financiamentos,
    IRepositorioDeParametrosDoPotencial parametros,
    IRelogio relogio)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Os recortes do território que a tela oferece.</summary>
    public static readonly IReadOnlyList<string> Recortes = ["adr", "norte", "noroeste", "sp", "fora"];

    /// <summary>Executa a leitura.</summary>
    /// <param name="de">O primeiro mês (aaaa-mm); nulo é o padrão.</param>
    /// <param name="ate">O último mês (aaaa-mm); nulo é o padrão.</param>
    /// <param name="produto">O código do produto de máquina do SICOR; nulo é os três.</param>
    /// <param name="programa">O código do programa; nulo é todos.</param>
    /// <param name="recorte"><c>adr</c> (padrão), <c>norte</c>, <c>noroeste</c>, <c>sp</c> ou <c>fora</c>.</param>
    /// <param name="lojaCodigo">A filial responsável; recorta dentro da ADR.</param>
    /// <param name="usina"><c>com</c> ou <c>sem</c> usina de etanol.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<FinanciamentosDoSicor>>> ExecutarAsync(
        string? de, string? ate, string? produto, string? programa, string? recorte, string? lojaCodigo, string? usina, CancellationToken ct)
    {
        var erros = new ColetorDeErros();
        var catalogo = await financiamentos.LerCatalogoAsync(ct);
        var vigente = ParametroComVigencia.VigenteEm(await parametros.ListarGeraisAsync(ct), ParametroComVigencia.HojeNoBrasil(relogio.Agora));

        int? produtoEscolhido = null;
        if (!string.IsNullOrWhiteSpace(produto))
        {
            if (int.TryParse(produto.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var lido) && catalogo.Produtos.Any(p => p.Codigo == lido))
                produtoEscolhido = lido;
            else
                erros.Registrar("produto", "O produto é um dos de máquina do SICOR — os códigos vêm em `produtos`.", produto);
        }

        int? programaEscolhido = null;
        if (!string.IsNullOrWhiteSpace(programa))
        {
            if (int.TryParse(programa.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var lido) && catalogo.Programas.Any(p => p.Codigo == lido))
                programaEscolhido = lido;
            else
                erros.Registrar("programa", "Não há crédito de máquina neste programa. Os programas vêm em `programas`.", programa);
        }

        var recorteEscolhido = string.IsNullOrWhiteSpace(recorte) ? "adr" : recorte.Trim().ToLowerInvariant();
        if (!Recortes.Contains(recorteEscolhido))
            erros.Registrar("recorte", "O recorte é adr, norte, noroeste, sp ou fora.", recorte);

        bool? comUsina = null;
        if (!string.IsNullOrWhiteSpace(usina))
        {
            comUsina = usina.Trim().ToLowerInvariant() switch { "com" => true, "sem" => false, _ => null };
            if (comUsina is null)
                erros.Registrar("usina", "O filtro de usina é \"com\" (só os municípios com usina de etanol) ou \"sem\".", usina);
        }

        DateOnly? Mes(string? texto, string campo)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;
            if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes)) return mes;
            erros.Registrar(campo, "O mês vem como aaaa-mm.", texto);
            return null;
        }

        var deLido = Mes(de, "de");
        var ateLido = Mes(ate, "ate");

        // O PADRÃO É A JANELA DO PAINEL DO CRÉDITO: os 12 meses que terminam no último mês do SICOR descontada a carência — o
        // Banco Central acrescenta contrato registrado com atraso nos meses recentes.
        var carencia = vigente?.MesesDeCarenciaDoSicor ?? 0;
        PeriodoDoSicor? periodo = null;
        if (catalogo.UltimoMes is { } ultimo && catalogo.PrimeiroMes is { } primeiro)
        {
            var fimPadrao = ultimo.AddMonths(-carencia);
            var fim = ateLido ?? (deLido is { } d && d > fimPadrao ? ultimo : fimPadrao);
            var inicio = deLido ?? fim.AddMonths(-11);
            if (fim > ultimo || fim < primeiro)
                erros.Registrar("ate", $"O SICOR vai de {primeiro:MM/yyyy} a {ultimo:MM/yyyy}.", ate);
            else if (inicio > fim)
                erros.Registrar("de", "O início do período vem antes do fim.", de);
            else
                periodo = new PeriodoDoSicor(inicio < primeiro ? primeiro : inicio, fim);
        }

        var municipios = await financiamentos.LerMunicipiosAsync(ct);
        var lojas = municipios
            .Where(m => m.PertenceAAdr && m.LojaCodigo is not null)
            .GroupBy(m => m.LojaCodigo!, StringComparer.Ordinal)
            .Select(g => new LojaDoCredito(g.Key, g.First().LojaNome ?? g.Key, g.Count()))
            .OrderBy(l => l.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();
        var lojaEscolhida = string.IsNullOrWhiteSpace(lojaCodigo)
            ? null
            : lojas.FirstOrDefault(l => string.Equals(l.Codigo, lojaCodigo.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(lojaCodigo) && lojaEscolhida is null)
            erros.Registrar("lojaCodigo", "Não há município da ADR com esta loja responsável. As lojas vêm em `lojas`.", lojaCodigo);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<FinanciamentosDoSicor>>("A consulta tem parâmetros que não valem.");

        var noRecorte = municipios
            .Where(m => recorteEscolhido switch
            {
                "adr" => m.PertenceAAdr,
                "norte" => m.PertenceAAdr && m.Regiao == nameof(RegiaoDaAreaDeAtuacao.Norte),
                "noroeste" => m.PertenceAAdr && m.Regiao == nameof(RegiaoDaAreaDeAtuacao.Noroeste),
                "fora" => !m.PertenceAAdr,
                _ => true
            })
            .Where(m => lojaEscolhida is null || (m.PertenceAAdr && m.LojaCodigo == lojaEscolhida.Codigo))
            .Where(m => comUsina is null || (m.Usinas > 0) == comUsina)
            .ToList();
        var todoOEstado = recorteEscolhido == "sp" && lojaEscolhida is null && comUsina is null;
        var codigos = noRecorte.Select(m => m.CodigoIbge).ToHashSet();

        var filtro = new FiltroDoSicor(produtoEscolhido, programaEscolhido);
        var serie = await financiamentos.LerSerieAsync(filtro, todoOEstado ? null : codigos, ct);

        FinanciamentosDoSicor resultado;
        if (periodo is null)
        {
            resultado = Vazio(catalogo, lojas, recorteEscolhido, produtoEscolhido, programaEscolhido, carencia, vigente);
        }
        else
        {
            var anterior = periodo.DoAnoAnterior();
            var porMunicipio = await financiamentos.LerPorMunicipioAsync(filtro, periodo, anterior, ct);
            resultado = Montar(
                catalogo, municipios, lojas, noRecorte, codigos, todoOEstado, recorteEscolhido, lojaEscolhida,
                produtoEscolhido, programaEscolhido, periodo, anterior, serie, porMunicipio, carencia, vigente);
        }

        return Resultado<ComProcedencia<FinanciamentosDoSicor>>.Ok(
            ComProcedencia<FinanciamentosDoSicor>.DoNossoBanco(
                resultado,
                "BCB/SICOR — crédito rural de investimento, produtos de máquina, por município e mês · área de atuação",
                relogio));
    }

    private static FinanciamentosDoSicor Montar(
        CatalogoDosFinanciamentos catalogo,
        IReadOnlyList<MunicipioNoCredito> municipios,
        List<LojaDoCredito> lojas,
        List<MunicipioNoCredito> noRecorte,
        HashSet<int> codigos,
        bool todoOEstado,
        string recorte,
        LojaDoCredito? loja,
        int? produto,
        int? programa,
        PeriodoDoSicor periodo,
        PeriodoDoSicor anterior,
        IReadOnlyList<LinhasDoSicorNoMes> serie,
        IReadOnlyList<CreditoDoMunicipioNasJanelas> porMunicipio,
        int carencia,
        ParametroDoPotencial? vigente)
    {
        IndiceDeCredito? Indice(JanelasDeCredito janelas) =>
            vigente is null ? null : IndicadoresDeMercado.Credito(janelas, vigente.PesoDosContratosNoCredito, vigente.MinimoDeLinhasNoCredito, vigente);

        // AS JANELAS DE UM PERÍODO PELA SÉRIE DO RECORTE: as linhas e o valor somados dentro de cada uma. A janela anterior que
        // começa antes do SICOR não é comparada — ela estaria incompleta, e a variação mediria a falta de meses.
        JanelasDeCredito? JanelasPelaSerie(PeriodoDoSicor recente)
        {
            var antes = recente.DoAnoAnterior();
            if (catalogo.PrimeiroMes is not { } primeiro || antes.De < primeiro) return null;
            var nele = serie.Where(m => m.Mes >= recente.De && m.Mes <= recente.Ate).ToList();
            var noAnterior = serie.Where(m => m.Mes >= antes.De && m.Mes <= antes.Ate).ToList();
            return new JanelasDeCredito(nele.Sum(m => m.Linhas), nele.Sum(m => m.Valor), noAnterior.Sum(m => m.Linhas), noAnterior.Sum(m => m.Valor));
        }

        var doPeriodo = JanelasPelaSerie(periodo);
        var noPeriodo = serie.Where(m => m.Mes >= periodo.De && m.Mes <= periodo.Ate).ToList();
        var totais = new TotaisDoCredito(
            noPeriodo.Sum(m => m.Linhas),
            noPeriodo.Sum(m => m.Valor),
            doPeriodo?.LinhasAnteriores,
            doPeriodo?.ValorAnterior,
            doPeriodo is null ? null : Indice(doPeriodo));

        var momento = new[] { 3, 6, 12 }
            .Select(meses =>
            {
                var janelas = JanelasPelaSerie(periodo.UltimosMeses(meses));
                return new MomentoDoCredito(meses, janelas, janelas is null ? null : Indice(janelas));
            })
            .ToList();

        var doEstado = porMunicipio.Aggregate(
            new JanelasDeCredito(0, 0, 0, 0),
            (s, m) => new JanelasDeCredito(
                s.Linhas + m.Janelas.Linhas, s.Valor + m.Janelas.Valor, s.LinhasAnteriores + m.Janelas.LinhasAnteriores, s.ValorAnterior + m.Janelas.ValorAnterior));

        var dados = municipios.ToDictionary(m => m.CodigoIbge);
        var linhasDoRecorte = porMunicipio
            .Where(m => m.CodigoIbge is { } c && codigos.Contains(c))
            .Where(m => m.Janelas.Linhas > 0 || m.Janelas.LinhasAnteriores > 0)
            .Select(m =>
            {
                var municipio = dados[m.CodigoIbge!.Value];
                var j = m.Janelas;
                decimal? fatia = doEstado.Valor > 0 ? 100m * j.Valor / doEstado.Valor : null;
                decimal? fatiaAntes = doEstado.ValorAnterior > 0 ? 100m * j.ValorAnterior / doEstado.ValorAnterior : null;
                return new CreditoDoMunicipio(
                    municipio.CodigoIbge,
                    municipio.Nome,
                    municipio.PertenceAAdr,
                    municipio.LojaCodigo,
                    municipio.LojaNome,
                    j,
                    Arredondar(fatia, 3),
                    fatia is { } f && fatiaAntes is { } fa ? decimal.Round(f - fa, 3) : null,
                    Indice(j));
            })
            .OrderByDescending(m => m.Janelas.Valor)
            .ThenBy(m => m.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var porLoja = linhasDoRecorte
            .Where(m => m.LojaCodigo is not null)
            .GroupBy(m => (m.LojaCodigo, m.Loja))
            .Select(g =>
            {
                var j = g.Aggregate(
                    new JanelasDeCredito(0, 0, 0, 0),
                    (s, m) => new JanelasDeCredito(
                        s.Linhas + m.Janelas.Linhas, s.Valor + m.Janelas.Valor, s.LinhasAnteriores + m.Janelas.LinhasAnteriores, s.ValorAnterior + m.Janelas.ValorAnterior));
                return new CreditoDaLoja(g.Key.LojaCodigo!, g.Key.Loja ?? g.Key.LojaCodigo!, g.Count(), j, Indice(j));
            })
            .OrderByDescending(l => l.Janelas.Valor)
            .ToList();

        // OS MUNICÍPIOS POR SITUAÇÃO: a faixa do índice de cada um. Sem base no ano anterior não há faixa — e o município novo
        // fica contado à parte, em vez de entrar como "aquecido" por ter saído do zero.
        var situacoes = new SituacoesDoCredito(
            linhasDoRecorte.Count(m => m.Indice?.Faixa == nameof(FaixaDeMercado.Retraido)),
            linhasDoRecorte.Count(m => m.Indice?.Faixa == nameof(FaixaDeMercado.Intermediaria)),
            linhasDoRecorte.Count(m => m.Indice?.Faixa == nameof(FaixaDeMercado.Aquecido)),
            linhasDoRecorte.Count(m => m.Indice?.Faixa == nameof(FaixaDeMercado.Superaquecido)),
            linhasDoRecorte.Count(m => m.Janelas.LinhasAnteriores == 0 || m.Janelas.ValorAnterior <= 0),
            linhasDoRecorte.Count(m => m.Indice?.BasePequena == true));

        RecorteNoEstado? noEstado = todoOEstado
            ? null
            : new RecorteNoEstado(
                totais.Linhas, doEstado.Linhas,
                doEstado.Linhas > 0 ? decimal.Round(100m * totais.Linhas / doEstado.Linhas, 2) : null,
                totais.Valor, doEstado.Valor,
                doEstado.Valor > 0 ? decimal.Round(100m * totais.Valor / doEstado.Valor, 2) : null);

        return new FinanciamentosDoSicor(
            catalogo.PrimeiroMes,
            catalogo.UltimoMes,
            carencia,
            vigente?.MesesDeCarenciaDoSicor is not null,
            periodo.De,
            periodo.Ate,
            periodo.Meses,
            anterior.De,
            anterior.Ate,
            produto,
            programa,
            catalogo.Produtos,
            catalogo.Programas,
            recorte,
            loja is not null ? $"Loja {loja.Nome}" : NomeDoRecorte(recorte),
            noRecorte.Count,
            lojas,
            totais,
            momento,
            [.. serie],
            noEstado,
            situacoes,
            porLoja,
            linhasDoRecorte,
            Lacunas(catalogo, periodo, vigente, doPeriodo is null));
    }

    private static FinanciamentosDoSicor Vazio(
        CatalogoDosFinanciamentos catalogo, List<LojaDoCredito> lojas, string recorte, int? produto, int? programa, int carencia,
        ParametroDoPotencial? vigente) =>
        new(
            null, null, carencia, vigente?.MesesDeCarenciaDoSicor is not null, null, null, 0, null, null, produto, programa,
            catalogo.Produtos, catalogo.Programas, recorte, NomeDoRecorte(recorte), 0, lojas,
            new TotaisDoCredito(0, 0, null, null, null), [], [], null, new SituacoesDoCredito(0, 0, 0, 0, 0, 0), [], [],
            [new MetricaSemDado("sicor", "O SICOR ainda não foi carregado: não há crédito de máquina para mostrar.")]);

    private static string NomeDoRecorte(string recorte) => recorte switch
    {
        "norte" => "Região Norte",
        "noroeste" => "Região Noroeste",
        "sp" => "Estado de São Paulo",
        "fora" => "Fora da Região Tracbel",
        _ => "Região Tracbel"
    };

    private static List<MetricaSemDado> Lacunas(
        CatalogoDosFinanciamentos catalogo, PeriodoDoSicor periodo, ParametroDoPotencial? vigente, bool semAnoAnterior)
    {
        var lacunas = new List<MetricaSemDado>
        {
            new("linhas",
                "O Banco Central não publica quantidade de contrato: cada LINHA do SICOR já é a soma dos contratos de uma combinação de município, produto e programa. O \"contratos (chassi)\" do protótipo é, aqui, a contagem de linhas — e valor ÷ linhas é o valor médio por linha, e não ticket médio."),
            new("shareTracbel",
                "O share da Tracbel contra a concorrência no crédito é a issue 262, no painel do crédito dos Indicadores Geográficos. Os contratos individuais do protótipo não existem no CRM: o SICOR publicado é agregado."),
            new("faixas",
                "A situação de cada município é a faixa do índice de crédito do CRM (70% linhas, 30% valor; retraído, intermediária, aquecido, superaquecido), e não os cortes de ±5% e ±20% do protótipo.")
        };

        if (vigente is null)
            lacunas.Add(new MetricaSemDado("indice", "Não há parâmetros do potencial vigentes: as variações aparecem, mas o índice e a faixa não."));
        else if (vigente.MesesDeCarenciaDoSicor is null)
            lacunas.Add(new MetricaSemDado(
                "carencia",
                "A carência do SICOR não foi decidida: o período padrão termina no último mês com dado, que o Banco Central ainda completa com registro atrasado."));

        if (vigente is not null && vigente.MinimoDeLinhasNoCredito is null)
            lacunas.Add(new MetricaSemDado(
                "basePequena",
                "O mínimo de linhas para o índice de um município valer como tendência (D-P03) não foi decidido: nenhum município é marcado como base pequena, e a contagem fica visível para quem lê julgar."));

        if (semAnoAnterior && catalogo.PrimeiroMes is { } primeiro)
            lacunas.Add(new MetricaSemDado(
                "anoAnterior",
                $"O mesmo período do ano anterior começa antes de {primeiro:MM/yyyy}, o primeiro mês do SICOR no CRM: as variações do período ficam vazias."));

        if (periodo.Meses < 12)
            lacunas.Add(new MetricaSemDado(
                "periodoCurto",
                "Período com menos de 12 meses: a comparação com o mesmo período do ano anterior respeita a sazonalidade, mas não fala do ano inteiro."));

        return lacunas;
    }

    private static decimal? Arredondar(decimal? valor, int casas) => valor is { } v ? decimal.Round(v, casas) : null;
}

/// <summary>A Gestão de Financiamentos consultada.</summary>
/// <param name="PrimeiroMesDoSicor">O mês mais antigo com crédito de máquina.</param>
/// <param name="UltimoMesDoSicor">O mais recente.</param>
/// <param name="MesesDeCarencia">Os meses recentes fora do período padrão (zero sem carência decidida).</param>
/// <param name="CarenciaDecidida">Se a carência é parâmetro vigente.</param>
/// <param name="De">O primeiro mês do período.</param>
/// <param name="Ate">O último.</param>
/// <param name="Meses">Quantos meses o período tem.</param>
/// <param name="AnteriorDe">O primeiro mês do mesmo período do ano anterior.</param>
/// <param name="AnteriorAte">O último.</param>
/// <param name="Produto">O produto escolhido; nulo é os três de máquina.</param>
/// <param name="Programa">O programa escolhido; nulo é todos.</param>
/// <param name="Produtos">As opções de produto.</param>
/// <param name="Programas">As opções de programa.</param>
/// <param name="Recorte">O código do recorte.</param>
/// <param name="RecorteNome">O recorte em palavras.</param>
/// <param name="MunicipiosNoRecorte">Quantos municípios o recorte tem.</param>
/// <param name="Lojas">As lojas da ADR — as opções do filtro.</param>
/// <param name="Totais">As linhas e o valor do período, com o ano anterior e o índice.</param>
/// <param name="Momento">R3, R6 e R12 terminando no fim do período.</param>
/// <param name="Serie">O recorte mês a mês, do primeiro ao último mês do SICOR.</param>
/// <param name="NoEstado">O peso do recorte em São Paulo no período; nulo quando o recorte é o estado.</param>
/// <param name="Situacoes">Os municípios do recorte pela faixa do índice.</param>
/// <param name="PorLoja">Cada loja do recorte.</param>
/// <param name="Municipios">A tabela analítica.</param>
/// <param name="Lacunas">O que a leitura não afirma, com o motivo.</param>
public sealed record FinanciamentosDoSicor(
    DateOnly? PrimeiroMesDoSicor,
    DateOnly? UltimoMesDoSicor,
    int MesesDeCarencia,
    bool CarenciaDecidida,
    DateOnly? De,
    DateOnly? Ate,
    int Meses,
    DateOnly? AnteriorDe,
    DateOnly? AnteriorAte,
    int? Produto,
    int? Programa,
    IReadOnlyList<OpcaoDoSicor> Produtos,
    IReadOnlyList<OpcaoDoSicor> Programas,
    string Recorte,
    string RecorteNome,
    int MunicipiosNoRecorte,
    IReadOnlyList<LojaDoCredito> Lojas,
    TotaisDoCredito Totais,
    IReadOnlyList<MomentoDoCredito> Momento,
    IReadOnlyList<LinhasDoSicorNoMes> Serie,
    RecorteNoEstado? NoEstado,
    SituacoesDoCredito Situacoes,
    IReadOnlyList<CreditoDaLoja> PorLoja,
    IReadOnlyList<CreditoDoMunicipio> Municipios,
    IReadOnlyList<MetricaSemDado> Lacunas);

/// <summary>Uma loja da ADR.</summary>
/// <param name="Codigo">A filial.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Municipios">Os municípios da ADR dela.</param>
public sealed record LojaDoCredito(string Codigo, string Nome, int Municipios);

/// <summary>Os números do topo.</summary>
/// <param name="Linhas">As linhas do SICOR no período.</param>
/// <param name="Valor">O valor financiado, em reais.</param>
/// <param name="LinhasAnteriores">No mesmo período do ano anterior; nulo quando ele começa antes do SICOR.</param>
/// <param name="ValorAnterior">O valor nele.</param>
/// <param name="Indice">O índice de crédito do período contra o anterior.</param>
public sealed record TotaisDoCredito(int Linhas, decimal Valor, int? LinhasAnteriores, decimal? ValorAnterior, IndiceDeCredito? Indice);

/// <summary>Um horizonte do momento do crédito.</summary>
/// <param name="Meses">3, 6 ou 12.</param>
/// <param name="Janelas">Os últimos meses do período contra os mesmos meses do ano anterior; nulo sem a janela anterior.</param>
/// <param name="Indice">O índice e as duas parcelas (linhas e valor).</param>
public sealed record MomentoDoCredito(int Meses, JanelasDeCredito? Janelas, IndiceDeCredito? Indice);

/// <summary>O peso do recorte em São Paulo.</summary>
/// <param name="Linhas">As linhas do recorte.</param>
/// <param name="LinhasDoEstado">As do estado.</param>
/// <param name="FatiaDasLinhas">Em %.</param>
/// <param name="Valor">O valor do recorte.</param>
/// <param name="ValorDoEstado">O do estado.</param>
/// <param name="FatiaDoValor">Em %.</param>
public sealed record RecorteNoEstado(int Linhas, int LinhasDoEstado, decimal? FatiaDasLinhas, decimal Valor, decimal ValorDoEstado, decimal? FatiaDoValor);

/// <summary>Os municípios do recorte pela faixa do índice.</summary>
/// <param name="Retraidos">Faixa retraído.</param>
/// <param name="Intermediarios">Faixa intermediária.</param>
/// <param name="Aquecidos">Faixa aquecido.</param>
/// <param name="Superaquecidos">Faixa superaquecido.</param>
/// <param name="SemBase">Sem crédito no mesmo período do ano anterior — novos, sem faixa.</param>
/// <param name="BasePequena">Com índice, mas abaixo do mínimo de linhas.</param>
public sealed record SituacoesDoCredito(int Retraidos, int Intermediarios, int Aquecidos, int Superaquecidos, int SemBase, int BasePequena);

/// <summary>Uma loja na tabela "por loja".</summary>
/// <param name="LojaCodigo">A filial.</param>
/// <param name="Loja">O nome.</param>
/// <param name="Municipios">Os municípios dela com crédito.</param>
/// <param name="Janelas">As linhas e o valor nas duas janelas.</param>
/// <param name="Indice">O índice e a faixa.</param>
public sealed record CreditoDaLoja(string LojaCodigo, string Loja, int Municipios, JanelasDeCredito Janelas, IndiceDeCredito? Indice);

/// <summary>Uma linha da tabela analítica.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="PertenceAAdr">Se é da ADR — fora dela, a tela marca "(fora)".</param>
/// <param name="LojaCodigo">A filial responsável.</param>
/// <param name="Loja">O nome dela.</param>
/// <param name="Janelas">As linhas e o valor nas duas janelas.</param>
/// <param name="FatiaNoEstado">Quanto do valor de São Paulo no período é deste município, em %.</param>
/// <param name="VariacaoDaFatia">Quantos pontos a fatia mudou contra o ano anterior.</param>
/// <param name="Indice">O índice, a faixa e o valor médio por linha.</param>
public sealed record CreditoDoMunicipio(
    int CodigoIbge,
    string Nome,
    bool PertenceAAdr,
    string? LojaCodigo,
    string? Loja,
    JanelasDeCredito Janelas,
    decimal? FatiaNoEstado,
    decimal? VariacaoDaFatia,
    IndiceDeCredito? Indice);
