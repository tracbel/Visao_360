using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Potencial;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Mercado;

/// <summary>
/// OS CENÁRIOS DE MERCADO — o planejamento da meta de cada município, do gestor com o CEN (issue 263, a aba "Cenários de
/// Mercado" do protótipo da pasta 360).
///
/// <para><b>Nenhuma conta nova de potencial.</b> O potencial, o mercado ajustado e a meta estrutural (potencial × share-alvo)
/// de cada município são os da Demanda e Previsão — as mesmas parcelas do motor, o mesmo fator de ciclo (preço, crédito e
/// percepção) e o mesmo share-alvo vigente. Os três cenários são o mercado ajustado × share com −5%, 0% e +5%, como no
/// protótipo; o realizado é a ENTREGA do ART, pela data da entrega e pelo município do comprador, a régua da meta.</para>
///
/// <para><b>O ano é o fiscal</b> (decisão do Ricardo de 02/10/2026): a meta é por município, categoria e ano fiscal, de
/// novembro a outubro, e o realizado, a média e a recomendação seguem a mesma régua. A média é a dos quatro anos fiscais
/// fechados antes do escolhido; a recomendação compara o cenário moderado com o realizado do ano fiscal anterior — o último
/// ano inteiro antes da meta.</para>
///
/// <para><b>A escolha é gravada</b> (<see cref="MetaDoCenarioNoMunicipio"/>), com o número combinado: o mercado ajustado
/// muda com o preço e o crédito, e a tela mostra o cenário de hoje ao lado do que foi combinado. Quem grava é a Gerência e
/// a Diretoria (<see cref="Permissoes.PlanejamentoGravar"/>); a gerência só nos municípios das filiais ao alcance dela.</para>
/// </summary>
public sealed class ObterCenariosDeMercado(
    ObterDemandaEPrevisao demanda,
    IRepositorioDeEntregas entregas,
    IRepositorioDosCenarios cenarios,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>As categorias que o planejamento tem, as do protótipo: tratores e colhedora de cana.</summary>
    public static readonly IReadOnlyList<string> Categorias = ["TRATOR", "COLHEDORA_DE_CANA"];

    /// <summary>Quantos anos fiscais fechados entram na média.</summary>
    public const int AnosDaMedia = 4;

    /// <summary>A diferença do conservador e do otimista para o moderado: 5% do mercado ajustado, como no protótipo.</summary>
    public const decimal VariacaoDoCenario = 0.05m;

    /// <summary>Acima de quanto do realizado anterior a meta pede uma rampa (1,5×), e o tamanho do degrau sugerido (1,3×).</summary>
    public const decimal LimiteDaMetaAtingivel = 1.5m, DegrauGradual = 1.3m;

    /// <summary>Até quantas máquinas a meta de um município sem histórico não pede validação.</summary>
    public const decimal MetaSemHistoricoAceitavel = 2m;

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Executa a leitura.</summary>
    /// <param name="categoria"><c>TRATOR</c> (padrão) ou <c>COLHEDORA_DE_CANA</c>.</param>
    /// <param name="anoFiscal">O ano fiscal da meta e do realizado; nulo é o corrente. Vai do corrente − 3 ao próximo.</param>
    /// <param name="de">O primeiro mês do intervalo do realizado, aaaa-mm; nulo é o início do ano fiscal.</param>
    /// <param name="ate">O último mês do intervalo, aaaa-mm; nulo é o mês corrente (ou o fim do ano fiscal, se já fechou).</param>
    /// <param name="regiao">Norte ou Noroeste; nulo é a ADR inteira.</param>
    /// <param name="lojaCodigo">A filial responsável; nulo é todas.</param>
    /// <param name="visao"><c>Filial</c> (padrão) ou <c>Empresa</c>.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<CenariosDeMercado>>> ExecutarAsync(
        string? categoria, string? anoFiscal, string? de, string? ate, string? regiao, string? lojaCodigo, string? visao,
        CancellationToken ct)
    {
        var agora = relogio.Agora;
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var anoCorrente = AnoFiscal.Do(mesCorrente);
        var erros = new ColetorDeErros();

        var codigo = LerCategoria(erros, categoria);
        var ano = LerAnoFiscal(erros, anoFiscal, anoCorrente);
        var deLido = LerMes(erros, "de", de);
        var ateLido = LerMes(erros, "ate", ate);

        // O INTERVALO PADRÃO é o ano fiscal escolhido até hoje; no ano que ainda não começou, o corrente até hoje.
        var anoDoIntervalo = AnoFiscal.Inteiro(ano).Inicial > mesCorrente ? anoCorrente : ano;
        var fiscalDoIntervalo = AnoFiscal.Inteiro(anoDoIntervalo);
        var intervaloDe = deLido ?? fiscalDoIntervalo.Inicial;
        var intervaloAte = ateLido ?? (fiscalDoIntervalo.Final < mesCorrente ? fiscalDoIntervalo.Final : mesCorrente);
        var primeiroMes = AnoFiscal.Inteiro(ano - AnosDaMedia).Inicial;
        if (deLido is not null && (intervaloDe < primeiroMes || intervaloDe > mesCorrente))
            erros.Registrar("de", $"O intervalo começa entre {Mes(primeiroMes)} e {Mes(mesCorrente)}.", de);
        if (ateLido is not null && (intervaloAte < primeiroMes || intervaloAte > mesCorrente))
            erros.Registrar("ate", $"O intervalo termina entre {Mes(primeiroMes)} e {Mes(mesCorrente)}.", ate);
        if (!erros.TemErro && intervaloDe > intervaloAte)
            erros.Registrar("ate", "O fim do intervalo vem depois do começo.", ate);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<CenariosDeMercado>>("A consulta tem parâmetros que não valem.");

        var lida = await demanda.ExecutarAsync(regiao, lojaCodigo, visao, codigo, ct);
        if (!lida.EhSucesso) return Repassar<ComProcedencia<CenariosDeMercado>>(lida);
        var daDemanda = lida.Valor.Dados;

        var daCategoria = await catalogo.ObterCategoriaDeMaquinaAsync(codigo, somenteAtiva: false, ct);
        var metas = daCategoria is null
            ? []
            : await cenarios.ListarAsync(daCategoria.Id, (short)ano, ct);
        var metaDe = metas.ToDictionary(m => m.CodigoIbge);
        var filialDe = await cenarios.FiliaisResponsaveisAsync(ct);

        // AS ENTREGAS DO ART, da média ao fim do ano escolhido (ou do intervalo), dos municípios do recorte e da categoria.
        // Sem venda do ART na apuração, ficam nulas — e a lacuna diz por quê.
        var semArt = daDemanda.Totais.EntreguesNoPeriodo is null;
        var fiscal = AnoFiscal.Inteiro(ano);
        var amanha = ParametroComVigencia.HojeNoBrasil(agora).AddDays(1);
        var fimDoAno = Menor(fiscal.Final.AddMonths(1), amanha);
        var fimDaLeitura = Menor(Maior(fiscal.Final, intervaloAte).AddMonths(1), amanha);
        var doRecorte = daDemanda.Municipios.Select(m => m.CodigoIbge).ToHashSet();
        var entregues = semArt
            ? []
            : (await entregas.ListarAsync(primeiroMes, fimDaLeitura, ct))
                .Where(e => e.MunicipioIbge is { } ibge && doRecorte.Contains(ibge)
                            && string.Equals(e.CategoriaCodigo, codigo, StringComparison.Ordinal))
                .ToList();
        var porMunicipio = entregues.ToLookup(e => e.MunicipioIbge!.Value);

        var anosDaMedia = Enumerable.Range(ano - AnosDaMedia, AnosDaMedia).ToList();
        var anterior = AnoFiscal.Inteiro(ano - 1);
        var (podeGravar, motivo) = PodeGravar(ano, anoCorrente);
        var profundidade = acesso.Atual.ProfundidadeDe(Permissoes.PlanejamentoGravar);

        var linhas = daDemanda.Municipios
            .Select(m =>
            {
                var doMunicipio = porMunicipio[m.CodigoIbge].ToList();
                int Contar(DateOnly inicio, DateOnly fimExclusivo) => doMunicipio.Count(e => e.EntregueEm >= inicio && e.EntregueEm < fimExclusivo);

                int? noAno = semArt ? null : Contar(fiscal.Inicial, fimDoAno);
                int? noAnoAnterior = semArt ? null : Contar(anterior.Inicial, anterior.Final.AddMonths(1));
                int? noIntervalo = semArt ? null : Contar(intervaloDe, Menor(intervaloAte.AddMonths(1), amanha));
                decimal? media = semArt
                    ? null
                    : decimal.Round(anosDaMedia.Sum(a => Contar(AnoFiscal.Inteiro(a).Inicial, AnoFiscal.Inteiro(a).Final.AddMonths(1))) / (decimal)AnosDaMedia, 2);
                int? clientes = semArt
                    ? null
                    : doMunicipio.Where(e => e.EntregueEm >= fiscal.Inicial && e.EntregueEm < fimDoAno && e.CompradorId is not null)
                        .Select(e => e.CompradorId).Distinct().Count();

                var gravavel = podeGravar && profundidade switch
                {
                    Profundidade.Organizacao => true,
                    Profundidade.Nenhum => false,
                    _ => filialDe.GetValueOrDefault(m.CodigoIbge) is { } filial && acesso.Atual.EmpresasVisiveis.Contains(filial)
                };

                return Linha(m, noAno, noAnoAnterior, noIntervalo, media, clientes, metaDe.GetValueOrDefault(m.CodigoIbge), gravavel);
            })
            .Where(l => l.Potencial is > 0 || l.RealizadoNoAno is > 0 || l.Escolha is not null)
            .OrderByDescending(l => l.AEntregar ?? -1)
            .ThenBy(l => l.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        var potencial = Numeros.SomaOuNulo(linhas.Select(l => l.Potencial));
        int? realizado = semArt ? null : linhas.Sum(l => l.RealizadoNoAno ?? 0);
        var totais = new TotaisDosCenarios(
            realizado,
            semArt ? null : linhas.Sum(l => l.RealizadoNoAnoAnterior ?? 0),
            semArt ? null : decimal.Round(linhas.Sum(l => l.MediaDosAnosAnteriores ?? 0), 1),
            semArt ? null : linhas.Sum(l => l.RealizadoNoIntervalo ?? 0),
            semArt ? null : entregues.Where(e => e.EntregueEm >= fiscal.Inicial && e.EntregueEm < fimDoAno && e.CompradorId is not null)
                .Select(e => e.CompradorId).Distinct().Count(),
            Numeros.Arredondar(potencial),
            Numeros.Arredondar(Numeros.SomaOuNulo(linhas.Select(l => l.MercadoAjustado))),
            Numeros.Arredondar(Numeros.SomaOuNulo(linhas.Select(l => l.MetaEstrutural))),
            realizado is { } r && potencial is > 0 ? decimal.Round(r / potencial.Value * 100m, 1) : null,
            Numeros.Arredondar(Numeros.SomaOuNulo(linhas.Select(l => l.AEntregar))),
            linhas.Count(l => l.Escolha is not null),
            linhas.Count,
            semArt ? null : Menor(fimDoAno, amanha).AddDays(-1));

        var resultado = new CenariosDeMercado(
            codigo,
            daDemanda.CategoriaNome,
            [.. daDemanda.Categorias.Where(c => Categorias.Contains(c.Codigo))],
            ano,
            [.. Enumerable.Range(0, AnosDaMedia + 1).Select(i => anoCorrente + 1 - i)],
            anosDaMedia,
            fiscal.Inicial > mesCorrente ? "Futuro" : fiscal.Final < mesCorrente ? "Fechado" : "Corrente",
            Mes(intervaloDe),
            Mes(intervaloAte),
            daDemanda.ShareAlvo,
            daDemanda.ShareDoPrototipo,
            podeGravar && profundidade > Profundidade.Nenhum,
            profundidade == Profundidade.Nenhum
                ? $"Gravar a meta pede a permissão {Permissoes.PlanejamentoGravar} — a Gerência e a Diretoria têm."
                : motivo,
            profundidade == Profundidade.Organizacao ? "Todos os municípios" : profundidade > Profundidade.Nenhum ? "Os municípios das filiais ao seu alcance" : null,
            totais,
            linhas,
            Lacunas(daDemanda, semArt));

        return Resultado<ComProcedencia<CenariosDeMercado>>.Ok(
            ComProcedencia<CenariosDeMercado>.DoNossoBanco(
                resultado,
                "motor do potencial · fator de ciclo · share-alvo do planejamento · frota.VendaDeMaquina (entrega do ART) · " +
                "organizacao.MetaDoCenarioNoMunicipio",
                relogio));
    }

    /// <summary>
    /// A META DO CENÁRIO HOJE num município — o número que a escolha grava. Nula quando o município não tem mercado calculado
    /// na categoria (sem regra, sem área ou sem share-alvo): aí só o cenário manual vale.
    /// </summary>
    /// <param name="codigo">A categoria.</param>
    /// <param name="codigoIbge">O município.</param>
    /// <param name="cenario">O cenário.</param>
    /// <param name="ct">Cancelamento.</param>
    internal async Task<Resultado<decimal?>> MetaDoCenarioHojeAsync(string codigo, int codigoIbge, CenarioDeMercado cenario, CancellationToken ct)
    {
        var lida = await demanda.ExecutarAsync(null, null, null, codigo, ct);
        if (!lida.EhSucesso) return Repassar<decimal?>(lida);

        var municipio = lida.Valor.Dados.Municipios.FirstOrDefault(m => m.CodigoIbge == codigoIbge);
        var (_, moderado) = Base(municipio?.AEntregarAjustada, municipio?.AEntregar);
        return Resultado<decimal?>.Ok(Cenario(moderado, cenario));
    }

    private static CenarioDoMunicipio Linha(
        DemandaDoMunicipioNaPrevisao m, int? noAno, int? noAnoAnterior, int? noIntervalo, decimal? media, int? clientes,
        MetaGravadaNoCenario? meta, bool gravavel)
    {
        var (ajustada, moderado) = Base(m.AEntregarAjustada, m.AEntregar);
        var conservador = Cenario(moderado, CenarioDeMercado.Conservador);
        var otimista = Cenario(moderado, CenarioDeMercado.Otimista);

        EscolhaDoCenario? escolha = meta is null
            ? null
            : new EscolhaDoCenario(
                meta.Cenario.ToString(),
                meta.ValorManual,
                meta.MetaNaEscolha,
                meta.Cenario == CenarioDeMercado.Manual ? null : Cenario(moderado, meta.Cenario),
                meta.GravadaPor,
                meta.GravadaEm);
        var aEntregar = escolha?.MetaCombinada ?? moderado;

        // O ENQUADRAMENTO DO NÚMERO DIGITADO: o cenário mais perto dele, e quanto ele fica do moderado (a regra do protótipo).
        string? enquadramento = null;
        decimal? doModerado = null;
        if (meta is { Cenario: CenarioDeMercado.Manual } && moderado is > 0)
        {
            var manual = meta.MetaNaEscolha;
            var (dCon, dMod, dOti) = (Math.Abs(manual - conservador!.Value), Math.Abs(manual - moderado.Value), Math.Abs(manual - otimista!.Value));
            enquadramento = dOti < dMod && dOti < dCon ? "Otimista" : dCon < dMod ? "Conservador" : "Moderado";
            doModerado = decimal.Round((manual / moderado.Value - 1) * 100m, 1);
        }

        // A RECOMENDAÇÃO DO PROTÓTIPO, contra o último ano fiscal inteiro antes da meta.
        string? recomendacao = null;
        decimal? acima = null, gradual = null;
        if (moderado is { } mod && noAnoAnterior is { } antes)
        {
            if (antes > 0 && mod > antes * LimiteDaMetaAtingivel)
            {
                recomendacao = "Gradual";
                acima = decimal.Round((mod / antes - 1) * 100m, 0);
                gradual = decimal.Round(antes * DegrauGradual, 1);
            }
            else if (antes == 0 && mod > MetaSemHistoricoAceitavel)
                recomendacao = "SemHistorico";
            else
                recomendacao = "Atingivel";
        }

        return new CenarioDoMunicipio(
            m.CodigoIbge,
            m.Nome,
            m.Regiao,
            m.LojaCodigo,
            m.Loja,
            m.CulturaPredominante,
            Efeito(m.FatorDePreco),
            Efeito(m.FatorDeCredito),
            noAno,
            noIntervalo,
            noAnoAnterior,
            media,
            clientes,
            m.DemandaEstrutural,
            m.DemandaAjustada,
            m.AEntregar,
            Share(noAno, m.DemandaEstrutural),
            Share(noAno, m.DemandaAjustada),
            Numeros.Arredondar(conservador),
            Numeros.Arredondar(moderado),
            Numeros.Arredondar(otimista),
            ajustada,
            escolha,
            Numeros.Arredondar(aEntregar),
            enquadramento,
            doModerado,
            recomendacao,
            acima,
            gradual,
            gravavel);
    }

    /// <summary>
    /// A BASE DOS CENÁRIOS: o mercado ajustado × share; sem o ajuste (fator sem dado), a meta estrutural — e a tela diz qual.
    /// </summary>
    private static (bool Ajustada, decimal? Moderado) Base(decimal? aEntregarAjustada, decimal? aEntregar) =>
        aEntregarAjustada is not null ? (true, aEntregarAjustada) : (false, aEntregar);

    private static decimal? Cenario(decimal? moderado, CenarioDeMercado cenario) => moderado is not { } m
        ? null
        : cenario switch
        {
            CenarioDeMercado.Conservador => decimal.Round(m * (1 - VariacaoDoCenario), 2),
            CenarioDeMercado.Otimista => decimal.Round(m * (1 + VariacaoDoCenario), 2),
            CenarioDeMercado.Moderado => decimal.Round(m, 2),
            _ => null
        };

    private static (bool Pode, string? Motivo) PodeGravar(int ano, int anoCorrente) => ano < anoCorrente
        ? (false, $"O {AnoFiscal.Nome(ano)} já fechou: a meta dele é história, e não se reescreve.")
        : (true, null);

    private static List<MetricaSemDado> Lacunas(DemandaEPrevisaoDaRegiao daDemanda, bool semArt)
    {
        var lacunas = daDemanda.Lacunas.ToList();
        if (semArt)
            lacunas.Add(new MetricaSemDado(
                "realizado",
                "Sem as vendas do ART no CRM, o realizado, a média, os clientes e a recomendação ficam vazios — a meta continua podendo ser escolhida."));
        lacunas.Add(new MetricaSemDado(
            "pecas",
            "Peças e serviços não entram no planejamento: o protótipo só planejava tratores e colhedora de cana, e a meta de pós-venda não tem fonte (issue 138)."));
        return lacunas;
    }

    private static string LerCategoria(ColetorDeErros erros, string? categoria)
    {
        var codigo = string.IsNullOrWhiteSpace(categoria) ? Categorias[0] : categoria.Trim().ToUpperInvariant();
        if (!Categorias.Contains(codigo))
            erros.Registrar("categoria", "Os cenários são de tratores (TRATOR) e de colhedora de cana (COLHEDORA_DE_CANA), como no protótipo.", categoria);
        return codigo;
    }

    internal static int LerAnoFiscal(ColetorDeErros erros, string? texto, int anoCorrente, int primeiro = 0)
    {
        primeiro = primeiro == 0 ? anoCorrente - AnosDaMedia + 1 : primeiro;
        if (string.IsNullOrWhiteSpace(texto)) return anoCorrente;
        if (int.TryParse(texto.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var ano) && ano >= primeiro && ano <= anoCorrente + 1)
            return ano;
        erros.Registrar("anoFiscal", $"Informe um ano fiscal de {primeiro} a {anoCorrente + 1} (2026 é nov/2025 a out/2026).", texto);
        return anoCorrente;
    }

    private static DateOnly? LerMes(ColetorDeErros erros, string campo, string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes)) return mes;
        erros.Registrar(campo, "O mês vem como aaaa-mm.", texto);
        return null;
    }

    /// <summary>Repassa a falha da leitura da demanda com o mesmo tipo — a mesma recusa, o mesmo código HTTP.</summary>
    internal static Resultado<T> Repassar<T, TOrigem>(Resultado<TOrigem> falha) => falha.Tipo switch
    {
        TipoDeFalha.SemPermissao => Resultado<T>.SemPermissao(falha.Erro!, falha.Erros),
        TipoDeFalha.NaoEncontrado => Resultado<T>.NaoEncontrado(falha.Erro!),
        TipoDeFalha.Conflito => Resultado<T>.Conflito(falha.Erro!, falha.Erros),
        TipoDeFalha.Concorrencia => Resultado<T>.Concorrencia(falha.Erro!),
        TipoDeFalha.DependenciaIndisponivel => Resultado<T>.Indisponivel(falha.Erro!),
        _ => Resultado<T>.FalhaDeValidacao(falha.Erro!, falha.Erros)
    };

    private static Resultado<T> Repassar<T>(Resultado<ComProcedencia<DemandaEPrevisaoDaRegiao>> falha) =>
        Repassar<T, ComProcedencia<DemandaEPrevisaoDaRegiao>>(falha);

    private static decimal? Efeito(decimal? fator) => fator is { } f ? decimal.Round((f - 1) * 100m, 1) : null;

    private static decimal? Share(int? realizado, decimal? base_) =>
        realizado is { } r && base_ is > 0 ? decimal.Round(r / base_.Value * 100m, 1) : null;

    private static DateOnly Menor(DateOnly a, DateOnly b) => a < b ? a : b;

    private static DateOnly Maior(DateOnly a, DateOnly b) => a > b ? a : b;

    private static string Mes(DateOnly mes) => mes.ToString("yyyy-MM", CultureInfo.InvariantCulture);
}

/// <summary>
/// A ESCOLHA DA META DE UM MUNICÍPIO nos Cenários de mercado (issue 263) — conservador, moderado, otimista ou um número.
///
/// <para><b>Quem grava:</b> quem tem <see cref="Permissoes.PlanejamentoGravar"/> — a Diretoria e o Administrador em todos os
/// municípios, a Gerência nos das filiais ao alcance dela (a filial responsável do município, na área de atuação).</para>
///
/// <para><b>O número gravado é calculado aqui</b>, e não recebido da tela: o cenário escolhido vira a meta do mercado de hoje
/// naquele município. Só o manual leva o número de quem digitou.</para>
/// </summary>
public sealed class EscolherMetaDoCenario(
    ObterCenariosDeMercado leitura,
    IRepositorioDosCenarios cenarios,
    IRepositorioDeMetasDosCenarios metas,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Grava a escolha.</summary>
    /// <param name="codigoIbge">O município, pelo código do IBGE.</param>
    /// <param name="entrada">A categoria, o ano fiscal, o cenário e, no manual, o número.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<EscolhaDoCenario>> ExecutarAsync(int codigoIbge, EscolhaDeMetaDoCenario entrada, CancellationToken ct)
    {
        var profundidade = acesso.Atual.ProfundidadeDe(Permissoes.PlanejamentoGravar);
        if (profundidade == Profundidade.Nenhum)
            return LeituraDeParametro.SemPermissao<EscolhaDoCenario>(Permissoes.PlanejamentoGravar);

        var anoCorrente = AnoFiscal.Do(AnoFiscal.MesCorrenteEmSaoPaulo(relogio.Agora));
        var erros = new ColetorDeErros();

        var codigo = string.IsNullOrWhiteSpace(entrada.Categoria) ? ObterCenariosDeMercado.Categorias[0] : entrada.Categoria.Trim().ToUpperInvariant();
        if (!ObterCenariosDeMercado.Categorias.Contains(codigo))
            erros.Registrar("categoria", "Os cenários são de tratores (TRATOR) e de colhedora de cana (COLHEDORA_DE_CANA).", entrada.Categoria);

        // SÓ O ANO CORRENTE E O PRÓXIMO: a meta de um ano que já fechou é história.
        var ano = ObterCenariosDeMercado.LerAnoFiscal(erros, entrada.AnoFiscal, anoCorrente, primeiro: anoCorrente);

        CenarioDeMercado? cenario = null;
        if (string.IsNullOrWhiteSpace(entrada.Cenario))
            erros.Registrar("cenario", "Escolha o cenário: Conservador, Moderado, Otimista ou Manual.", entrada.Cenario);
        else if (Enum.TryParse<CenarioDeMercado>(entrada.Cenario.Trim(), ignoreCase: true, out var lido) && Enum.IsDefined(lido)
                 && !int.TryParse(entrada.Cenario, out _))
            cenario = lido;
        else
            erros.Registrar("cenario", "O cenário é Conservador, Moderado, Otimista ou Manual.", entrada.Cenario);

        var valor = LeituraDeParametro.Numero(erros, "valorManual", entrada.ValorManual, cenario == CenarioDeMercado.Manual, "a meta do cenário manual, em máquinas");
        if (cenario is not null and not CenarioDeMercado.Manual && valor is not null)
            erros.Registrar("valorManual", "Só o cenário manual leva um número digitado.", entrada.ValorManual);
        if (valor is { } v && (v < 0 || v > 100_000 || v != decimal.Round(v, 2)))
            erros.Registrar("valorManual", "A meta vai de 0 a 100 mil máquinas, com até duas casas decimais.", entrada.ValorManual);

        if (erros.TemErro) return erros.Recusar<EscolhaDoCenario>("A escolha tem campos a corrigir.");

        var municipio = await cenarios.ObterMunicipioAsync(codigoIbge, ct);
        if (municipio is null)
            return Resultado<EscolhaDoCenario>.NaoEncontrado($"Não há município com o código IBGE {codigoIbge}.");
        if (!municipio.PertenceAAdr)
            return Resultado<EscolhaDoCenario>.Falha($"{municipio.Nome} não está na área de atuação da Tracbel: o planejamento é dos municípios da ADR.");

        // A GERÊNCIA PLANEJA AS FILIAIS DELA: o município é da filial responsável na área de atuação.
        if (profundidade < Profundidade.Organizacao
            && !(municipio.EmpresaResponsavelId is { } filial && acesso.Atual.EmpresasVisiveis.Contains(filial)))
            return Resultado<EscolhaDoCenario>.SemPermissao(
                $"{municipio.Nome} é de uma filial fora do seu alcance — a sua permissão {Permissoes.PlanejamentoGravar} vale para os municípios das filiais ao seu alcance.");

        var meta = valor;
        if (cenario != CenarioDeMercado.Manual)
        {
            var hoje = await leitura.MetaDoCenarioHojeAsync(codigo, codigoIbge, cenario!.Value, ct);
            if (!hoje.EhSucesso) return ObterCenariosDeMercado.Repassar<EscolhaDoCenario, decimal?>(hoje);
            meta = hoje.Valor;
            if (meta is null)
                return Resultado<EscolhaDoCenario>.Falha(
                    $"{municipio.Nome} não tem mercado calculado nesta categoria (sem regra, sem área ou sem share-alvo): use o cenário manual.");
        }

        var categoria = await catalogo.ObterCategoriaDeMaquinaAsync(codigo, somenteAtiva: false, ct);
        if (categoria is null)
            return Resultado<EscolhaDoCenario>.Falha($"A categoria {codigo} não está no catálogo.");

        var usuario = acesso.Atual.UsuarioId;
        var agora = relogio.Agora;
        var existente = await metas.ObterParaAlterarAsync(municipio.Id, categoria.Id, (short)ano, ct);
        MetaDoCenarioNoMunicipio gravada;
        try
        {
            if (existente is null)
            {
                gravada = MetaDoCenarioNoMunicipio.Escolher(
                    municipio.Id, categoria.Id, (short)ano, cenario!.Value, cenario == CenarioDeMercado.Manual ? valor : null, meta!.Value, usuario, agora);
                await metas.AdicionarAsync(gravada, ct);
            }
            else
            {
                existente.Alterar(cenario!.Value, cenario == CenarioDeMercado.Manual ? valor : null, meta!.Value, usuario, agora);
                gravada = existente;
            }
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<EscolhaDoCenario>.Falha(erro.Message);
        }

        var salvou = await unidade.SalvarAsync(ct);
        if (!salvou.EhSucesso) return Resultado<EscolhaDoCenario>.Conflito(salvou.Erro!);

        return Resultado<EscolhaDoCenario>.Ok(new EscolhaDoCenario(
            gravada.Cenario.ToString(),
            gravada.ValorManual,
            gravada.MetaNaEscolha,
            gravada.Cenario == CenarioDeMercado.Manual ? null : gravada.MetaNaEscolha,
            acesso.Atual.NomeExibicao,
            gravada.GravadaEm));
    }
}

/// <summary>O corpo da escolha — texto, para o erro dizer o que chegou.</summary>
/// <param name="Categoria"><c>TRATOR</c> (padrão) ou <c>COLHEDORA_DE_CANA</c>.</param>
/// <param name="AnoFiscal">O ano fiscal; nulo é o corrente. Só o corrente e o próximo.</param>
/// <param name="Cenario">Conservador, Moderado, Otimista ou Manual.</param>
/// <param name="ValorManual">A meta em máquinas, só no manual.</param>
public sealed record EscolhaDeMetaDoCenario(string? Categoria = null, string? AnoFiscal = null, string? Cenario = null, string? ValorManual = null);

/// <summary>Os cenários de uma categoria num ano fiscal.</summary>
/// <param name="Categoria">O código da categoria.</param>
/// <param name="CategoriaNome">O nome.</param>
/// <param name="Categorias">As categorias que o filtro oferece — tratores e colhedora de cana, as que têm demanda.</param>
/// <param name="AnoFiscal">O ano fiscal da meta (2026 é nov/2025 a out/2026).</param>
/// <param name="AnosFiscais">Os anos que o filtro oferece: o próximo, o corrente e os três anteriores.</param>
/// <param name="AnosDaMedia">Os anos fiscais fechados que entram na média.</param>
/// <param name="SituacaoDoAno"><c>Corrente</c>, <c>Fechado</c> ou <c>Futuro</c>.</param>
/// <param name="IntervaloDe">O primeiro mês do intervalo do realizado (aaaa-mm).</param>
/// <param name="IntervaloAte">O último mês (aaaa-mm).</param>
/// <param name="ShareAlvo">O share-alvo vigente da categoria, em %; nulo sem share.</param>
/// <param name="ShareDoPrototipo">Se o share ainda é a semente do protótipo, a confirmar.</param>
/// <param name="PodeGravar">Se quem lê pode gravar metas neste ano fiscal.</param>
/// <param name="PorQueNaoGrava">Por que não, quando não pode.</param>
/// <param name="AlcanceDaGravacao">Onde a permissão de gravar vale.</param>
/// <param name="Totais">Os números do topo.</param>
/// <param name="Municipios">Os municípios com potencial, realizado ou meta gravada.</param>
/// <param name="Lacunas">O que a tela pede e o dado não sustenta.</param>
public sealed record CenariosDeMercado(
    string Categoria,
    string CategoriaNome,
    IReadOnlyList<CategoriaParaFiltro> Categorias,
    int AnoFiscal,
    IReadOnlyList<int> AnosFiscais,
    IReadOnlyList<int> AnosDaMedia,
    string SituacaoDoAno,
    string IntervaloDe,
    string IntervaloAte,
    decimal? ShareAlvo,
    bool ShareDoPrototipo,
    bool PodeGravar,
    string? PorQueNaoGrava,
    string? AlcanceDaGravacao,
    TotaisDosCenarios Totais,
    IReadOnlyList<CenarioDoMunicipio> Municipios,
    IReadOnlyList<MetricaSemDado> Lacunas);

/// <summary>Os números do topo.</summary>
/// <param name="RealizadoNoAno">Máquinas entregues no ano fiscal (até hoje, no corrente).</param>
/// <param name="RealizadoNoAnoAnterior">No ano fiscal anterior, inteiro.</param>
/// <param name="MediaDosAnosAnteriores">A média por ano dos quatro anos fiscais fechados antes do escolhido.</param>
/// <param name="RealizadoNoIntervalo">No intervalo escolhido.</param>
/// <param name="Clientes">Clientes distintos que receberam máquina no ano fiscal.</param>
/// <param name="Potencial">A demanda estrutural — 100% do mercado.</param>
/// <param name="MercadoAjustado">A demanda pelo momento (fator de ciclo).</param>
/// <param name="MetaEstrutural">Potencial × share-alvo.</param>
/// <param name="RealizacaoSobrePotencial">Realizado ÷ potencial, em %.</param>
/// <param name="AEntregar">A soma das metas: a combinada onde há escolha, o moderado onde não há.</param>
/// <param name="MunicipiosComMeta">Quantos municípios têm meta gravada.</param>
/// <param name="Municipios">Quantos municípios a tabela tem.</param>
/// <param name="EntregasAte">O último dia das entregas contadas no ano.</param>
public sealed record TotaisDosCenarios(
    int? RealizadoNoAno,
    int? RealizadoNoAnoAnterior,
    decimal? MediaDosAnosAnteriores,
    int? RealizadoNoIntervalo,
    int? Clientes,
    decimal? Potencial,
    decimal? MercadoAjustado,
    decimal? MetaEstrutural,
    decimal? RealizacaoSobrePotencial,
    decimal? AEntregar,
    int MunicipiosComMeta,
    int Municipios,
    DateOnly? EntregasAte);

/// <summary>Um município no planejamento.</summary>
/// <param name="CodigoIbge">O código do IBGE.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Regiao">Norte ou Noroeste.</param>
/// <param name="LojaCodigo">A filial responsável.</param>
/// <param name="Loja">O nome dela.</param>
/// <param name="CulturaPrincipal">A cultura com o maior parque de máquinas no município.</param>
/// <param name="EfeitoDoPreco">O quanto o preço das culturas move o mercado do município, em % (o fator de ciclo).</param>
/// <param name="EfeitoDoCredito">O quanto o crédito do município o move, em %.</param>
/// <param name="RealizadoNoAno">Entregues no ano fiscal.</param>
/// <param name="RealizadoNoIntervalo">No intervalo.</param>
/// <param name="RealizadoNoAnoAnterior">No ano fiscal anterior — a base da recomendação.</param>
/// <param name="MediaDosAnosAnteriores">A média dos quatro anos fechados.</param>
/// <param name="Clientes">Clientes distintos que receberam máquina no ano.</param>
/// <param name="Potencial">A demanda estrutural.</param>
/// <param name="MercadoAjustado">A demanda pelo momento.</param>
/// <param name="MetaEstrutural">Potencial × share-alvo.</param>
/// <param name="ShareEstrutural">Realizado ÷ potencial, em % (não é share de mercado: pode passar de 100%).</param>
/// <param name="ShareAjustado">Realizado ÷ mercado ajustado, em %.</param>
/// <param name="Conservador">O mercado ajustado × share com −5%.</param>
/// <param name="Moderado">O mercado ajustado × share.</param>
/// <param name="Otimista">O mercado ajustado × share com +5%.</param>
/// <param name="BaseAjustada">Se os cenários partem do mercado ajustado; falso é a meta estrutural (fator sem dado).</param>
/// <param name="Escolha">A meta gravada; nula enquanto ninguém escolheu (vale o moderado).</param>
/// <param name="AEntregar">A meta combinada, ou o moderado sem escolha.</param>
/// <param name="Enquadramento">No manual, o cenário mais perto do número digitado.</param>
/// <param name="DiferencaParaOModerado">No manual, quanto o número fica do moderado, em %.</param>
/// <param name="Recomendacao"><c>Gradual</c>, <c>SemHistorico</c> ou <c>Atingivel</c>; nula sem mercado ou sem realizado.</param>
/// <param name="AcimaDoRealizado">No gradual, quanto o moderado fica acima do realizado anterior, em %.</param>
/// <param name="MetaGradual">No gradual, o degrau sugerido: realizado anterior × 1,3.</param>
/// <param name="Gravavel">Se quem lê pode gravar a meta deste município.</param>
public sealed record CenarioDoMunicipio(
    int CodigoIbge,
    string Nome,
    string Regiao,
    string? LojaCodigo,
    string? Loja,
    string? CulturaPrincipal,
    decimal? EfeitoDoPreco,
    decimal? EfeitoDoCredito,
    int? RealizadoNoAno,
    int? RealizadoNoIntervalo,
    int? RealizadoNoAnoAnterior,
    decimal? MediaDosAnosAnteriores,
    int? Clientes,
    decimal? Potencial,
    decimal? MercadoAjustado,
    decimal? MetaEstrutural,
    decimal? ShareEstrutural,
    decimal? ShareAjustado,
    decimal? Conservador,
    decimal? Moderado,
    decimal? Otimista,
    bool BaseAjustada,
    EscolhaDoCenario? Escolha,
    decimal? AEntregar,
    string? Enquadramento,
    decimal? DiferencaParaOModerado,
    string? Recomendacao,
    decimal? AcimaDoRealizado,
    decimal? MetaGradual,
    bool Gravavel);

/// <summary>A meta gravada de um município.</summary>
/// <param name="Cenario">Conservador, Moderado, Otimista ou Manual.</param>
/// <param name="ValorManual">O número digitado, no manual.</param>
/// <param name="MetaCombinada">O número gravado na escolha.</param>
/// <param name="MetaDoCenarioHoje">O mesmo cenário com o mercado de hoje; nulo no manual.</param>
/// <param name="GravadaPor">Quem gravou por último.</param>
/// <param name="GravadaEm">Quando (UTC).</param>
public sealed record EscolhaDoCenario(
    string Cenario, decimal? ValorManual, decimal MetaCombinada, decimal? MetaDoCenarioHoje, string? GravadaPor, DateTime GravadaEm);
