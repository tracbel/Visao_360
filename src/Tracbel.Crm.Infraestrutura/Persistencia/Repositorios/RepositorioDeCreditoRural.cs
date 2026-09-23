using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A leitura do crédito rural do SICOR (issue 68).
///
/// <para><b>Tudo agregado no banco.</b> São 200 mil linhas; a tela precisa de ~14 anos, ~100 produtos e
/// ~600 municípios. As três consultas agrupam no SQL Server e trazem só o resultado.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDeCreditoRural(CrmDbContext contexto) : IRepositorioDeCreditoRural
{
    /// <summary>
    /// Os produtos de máquina, vindos do domínio (issue 157).
    ///
    /// <para>A lista morava aqui, no repositório de leitura. "O que conta como máquina" é decisão de
    /// negócio, e decisão de negócio não se lê num detalhe de persistência — ela está em
    /// <see cref="ParametroDoPotencial.ProdutosDeMaquinaNoSicor"/>, até a issue 165 pô-la no catálogo.</para>
    /// </summary>
    private static readonly int[] ProdutosDeMaquina = ParametroDoPotencial.ProdutosDeMaquinaNoSicor;

    /// <inheritdoc />
    public async Task<PainelDeCreditoRural> LerAsync(
        short mesesPorJanela, short? mesesDeCarencia, ParametroDoPotencial? vigente, CancellationToken ct)
    {
        var credito = contexto.CreditosRuraisDeInvestimento.AsNoTracking();

        // PROJEÇÃO ANÔNIMA E ORDEM PELA CHAVE: o EF não traduz ordenação por propriedade de um record
        // construído dentro da consulta (medido: "could not be translated"). O record é montado depois.
        var porAno = (await credito
                .GroupBy(c => c.Ano)
                .Select(g => new
                {
                    Ano = g.Key,
                    LinhasDeMaquinas = g.Count(c => ProdutosDeMaquina.Contains(c.CodigoProduto)),
                    ValorDeMaquinas = g.Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto)).Sum(c => (decimal?)c.Valor) ?? 0,
                    LinhasTotais = g.Count(),
                    ValorTotal = g.Sum(c => c.Valor),
                    UltimoMes = g.Max(c => c.Mes)
                })
                .OrderBy(a => a.Ano)
                .ToListAsync(ct))
            .Select(a => new CreditoNoAno(a.Ano, a.LinhasDeMaquinas, a.ValorDeMaquinas, a.LinhasTotais, a.ValorTotal, a.UltimoMes))
            .ToList();

        if (porAno.Count == 0) return new PainelDeCreditoRural(null, null, [], [], [], null, null);

        var ultimoAno = porAno[^1];
        var ultimoMes = new DateOnly(ultimoAno.Ano, ultimoAno.UltimoMes, 1);

        // A JANELA NÃO TERMINA NO ÚLTIMO MÊS COM DADO (issue 157). O Banco Central acrescenta contrato
        // registrado com atraso nos meses recentes: terminar ali compara 12 meses cheios com 12 que
        // ainda estão enchendo, e o crédito aparece caindo sem ter caído. Carência não decidida vale
        // zero — e a tela diz isso, em vez de a consulta descartar meses por um palpite.
        var carencia = mesesDeCarencia ?? 0;

        // AS JANELAS EM "ANO × 12 + MÊS": comparar (ano, mês) com duas colunas exigiria OR encadeado, e o
        // número único vira um BETWEEN que o índice de período atende.
        var fim = ultimoMes.Year * 12 + ultimoMes.Month - carencia;
        var inicioUltima = fim - (mesesPorJanela - 1);
        var inicioAnterior = fim - (2 * mesesPorJanela - 1);

        var janela = new JanelaDoCredito(
            ultimoMes,
            carencia,
            mesesDeCarencia is not null,
            MesDe(inicioUltima),
            MesDe(fim),
            MesDe(inicioAnterior),
            MesDe(inicioUltima - 1),
            mesesPorJanela);

        var nasJanelas = credito.Where(c => c.Ano * 12 + c.Mes >= inicioAnterior && c.Ano * 12 + c.Mes <= fim);

        var produtos = await nasJanelas
            .GroupBy(c => c.CodigoProduto)
            .Select(g => new
            {
                Codigo = g.Key,
                Linhas = g.Count(c => c.Ano * 12 + c.Mes >= inicioUltima),
                Valor = g.Where(c => c.Ano * 12 + c.Mes >= inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0,
                LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes < inicioUltima),
                ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes < inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0
            })
            .ToListAsync(ct);

        var nomes = await contexto.ItensDoSicor.AsNoTracking()
            .Where(i => i.Tipo == "PRODUTO")
            .ToDictionaryAsync(i => i.Codigo, i => i.Descricao, ct);

        var porProduto = produtos
            .Select(p => new CreditoPorProduto(
                p.Codigo,
                nomes.TryGetValue(p.Codigo, out var nome) ? nome : $"Produto {p.Codigo}",
                ProdutosDeMaquina.Contains(p.Codigo),
                new JanelasDeCredito(p.Linhas, p.Valor, p.LinhasAnteriores, p.ValorAnterior)))
            .OrderByDescending(p => p.Janelas.Valor)
            .ToList();

        var municipios = await nasJanelas
            .Where(c => ProdutosDeMaquina.Contains(c.CodigoProduto))
            .GroupBy(c => c.MunicipioId)
            .Select(g => new
            {
                MunicipioId = g.Key,
                Linhas = g.Count(c => c.Ano * 12 + c.Mes >= inicioUltima),
                Valor = g.Where(c => c.Ano * 12 + c.Mes >= inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0,
                LinhasAnteriores = g.Count(c => c.Ano * 12 + c.Mes < inicioUltima),
                ValorAnterior = g.Where(c => c.Ano * 12 + c.Mes < inicioUltima).Sum(c => (decimal?)c.Valor) ?? 0
            })
            .ToListAsync(ct);

        var ids = municipios.Select(m => m.MunicipioId).ToList();
        var catalogo = await contexto.Municipios.AsNoTracking()
            .Where(m => ids.Contains(m.Id) && m.CodigoIbge != null)
            .ToDictionaryAsync(m => m.Id, m => new { CodigoIbge = m.CodigoIbge!.Value, m.Nome }, ct);

        var daAdr = (await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                .Where(a => a.EncerradoEm == null && a.PertenceAAdr)
                .Select(a => a.MunicipioId)
                .ToListAsync(ct))
            .ToHashSet();

        // O ÍNDICE VEM DO DOMÍNIO (issue 73), e não de uma divisão feita aqui: é a mesma conta do recorte
        // e da tela. O peso e as faixas são parâmetro com vigência; sem vigência, saem as janelas sem
        // índice — o número é conta, mas "aquecido" é decisão registrada.
        IndiceDeCredito? Indice(JanelasDeCredito janelas) =>
            vigente is null
                ? null
                : IndicadoresDeMercado.Credito(
                    janelas, vigente.PesoDosContratosNoCredito, vigente.MinimoDeLinhasNoCredito, vigente);

        var porMunicipio = municipios
            .Where(m => catalogo.ContainsKey(m.MunicipioId))
            .Select(m =>
            {
                var janelas = new JanelasDeCredito(m.Linhas, m.Valor, m.LinhasAnteriores, m.ValorAnterior);
                return new CreditoDeMaquinasNoMunicipio(
                    catalogo[m.MunicipioId].CodigoIbge,
                    catalogo[m.MunicipioId].Nome,
                    daAdr.Contains(m.MunicipioId),
                    janelas,
                    Indice(janelas));
            })
            .OrderByDescending(m => m.Janelas.Valor)
            .ToList();

        // OS DOIS RECORTES, SOMADOS AQUI E NÃO NO NAVEGADOR (issue 157). São Paulo é o denominador:
        // sem ele, "R$ 300 mi na Região" pode ser um terço do estado ou um vigésimo. A soma do estado
        // inclui o município que ainda não tem código do IBGE — ele tem crédito do mesmo jeito.
        var regiao = Somar("Região (ADR)", municipios.Where(m => daAdr.Contains(m.MunicipioId))
            .Select(m => (m.Linhas, m.Valor, m.LinhasAnteriores, m.ValorAnterior)));
        var saoPaulo = Somar("São Paulo", municipios
            .Select(m => (m.Linhas, m.Valor, m.LinhasAnteriores, m.ValorAnterior)));

        regiao = regiao with { Indice = Indice(regiao.Janelas) };
        saoPaulo = saoPaulo with { Indice = Indice(saoPaulo.Janelas) };

        return new PainelDeCreditoRural(ultimoMes, janela, porAno, porProduto, porMunicipio, regiao, saoPaulo);
    }

    /// <summary>O mês (dia 1) de um número "ano × 12 + mês", que é como as janelas são comparadas.</summary>
    private static DateOnly MesDe(int numero) => new((numero - 1) / 12, (numero - 1) % 12 + 1, 1);

    /// <summary>
    /// Soma as duas janelas de um recorte e conta os municípios com linha na última.
    ///
    /// <para>Município contado é o que tem linha na janela <b>recente</b>: o que só teve crédito na
    /// anterior não é "município com crédito hoje", e contá-lo esconderia justamente a queda.</para>
    /// </summary>
    private static CreditoNoRecorte Somar(
        string recorte, IEnumerable<(int Linhas, decimal Valor, int LinhasAnteriores, decimal ValorAnterior)> linhas) =>
        linhas.Aggregate(
            new CreditoNoRecorte(recorte, 0, new JanelasDeCredito(0, 0, 0, 0)),
            (soma, m) => soma with
            {
                Municipios = soma.Municipios + (m.Linhas > 0 ? 1 : 0),
                Janelas = new JanelasDeCredito(
                    soma.Janelas.Linhas + m.Linhas,
                    soma.Janelas.Valor + m.Valor,
                    soma.Janelas.LinhasAnteriores + m.LinhasAnteriores,
                    soma.Janelas.ValorAnterior + m.ValorAnterior)
            });
}
