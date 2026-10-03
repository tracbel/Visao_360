using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CodigosDoIbge;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CriterioDasVendasDoArt;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O MUNICÍPIO AO LONGO DO TEMPO (27/09/2026) — as vendas por ano fiscal e a lavoura por ano da PAM. Saiu do repositório
/// dos indicadores no plano 2 do documento 54: o painel lê uma janela de todos os municípios, e o histórico todos os anos
/// de um só; eram duas razões para mudar no mesmo arquivo de 2.000 linhas.
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDoHistoricoDoMunicipio(CrmDbContext contexto) : IRepositorioHistoricoDoMunicipio
{
    /// <inheritdoc />
    public async Task<HistoricoDoMunicipio?> ApurarHistoricoDoMunicipioAsync(
        ConsultaDoHistoricoDoMunicipio consulta, CancellationToken ct)
    {
        // A VISÃO DA EMPRESA ABRE O ALCANCE ENTRE FILIAIS, como no painel — o caso de uso já conferiu a
        // permissão, e a abertura vai para o diário com o motivo.
        using var alcance = consulta.Visao == VisaoTerritorial.Empresa
            ? contexto.AbrirAlcanceEntreEmpresas("Histórico do município na visão consolidada da empresa (documento 32)")
            : null;

        var doCodigo = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge == consulta.CodigoIbge && m.Uf == SaoPaulo)
            .Select(m => new { m.Id, m.Nome })
            .ToListAsync(ct);
        if (doCodigo.Count == 0) return null;

        var idsDoMunicipio = doCodigo.Select(m => m.Id).ToList();

        // OS CLIENTES DO MUNICÍPIO, PELA MESMA REGRA DO PAINEL: o endereço principal de menor Id. Um cliente com
        // dois endereços principais conta onde o painel o conta — senão a ficha e a linha da tabela discordariam
        // do mesmo cliente.
        var enderecos = await (
                from endereco in contexto.Enderecos.AsNoTracking().Where(e => e.EhPrincipal && e.ExcluidoEm == null)
                join cliente in contexto.Clientes.AsNoTracking().Where(c => c.ExcluidoEm == null)
                    on endereco.ClienteId equals cliente.Id
                where contexto.Enderecos.Any(e => e.ClienteId == cliente.Id
                                                  && e.EhPrincipal
                                                  && e.ExcluidoEm == null
                                                  && e.MunicipioId != null
                                                  && idsDoMunicipio.Contains(e.MunicipioId.Value))
                select new { ClienteId = cliente.Id, cliente.EmpresaId, EnderecoId = endereco.Id, endereco.MunicipioId })
            .ToListAsync(ct);

        HashSet<long>? doResponsavel = null;
        if (consulta.ResponsavelId is { } responsavel)
            doResponsavel = (await (
                    from vinculo in contexto.ClienteCarteiras.AsNoTracking().Where(v => v.DesvinculadoEm == null)
                    join carteira in contexto.Carteiras.AsNoTracking() on vinculo.CarteiraId equals carteira.Id
                    where carteira.ExcluidoEm == null
                          && carteira.Natureza == NaturezaDaCarteira.Comercial
                          && carteira.ResponsavelId == responsavel
                    select vinculo.ClienteId)
                .ToListAsync(ct))
                .ToHashSet();

        var clientes = enderecos
            .GroupBy(e => e.ClienteId)
            .Select(g => g.MinBy(e => e.EnderecoId)!)
            .Where(e => e.MunicipioId is { } id && idsDoMunicipio.Contains(id))
            .Where(e => consulta.FilialDoClienteId is not { } filial || e.EmpresaId == filial)
            .Where(e => doResponsavel is null || doResponsavel.Contains(e.ClienteId))
            .Select(e => e.ClienteId)
            .ToHashSet();

        var ultimoMes = new DateOnly(consulta.UltimoMesFechado.Year, consulta.UltimoMesFechado.Month, 1);
        var idsDosClientes = clientes.ToList();

        var faturamento = idsDosClientes.Count == 0
            ? []
            : await contexto.FaturamentoDosClientes.AsNoTracking()
                .Where(f => f.ExcluidoEm == null
                            && f.Competencia <= ultimoMes
                            && idsDosClientes.Contains(f.ClienteId)
                            && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
                .Select(f => new { f.ClienteId, f.Competencia, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros })
                .ToListAsync(ct);

        // A COBERTURA É A DO ALCANCE INTEIRO, e não a do município: um município sem venda em 2023 não é um
        // município sem carga em 2023.
        var primeiraComCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .MinAsync(f => (DateOnly?)f.Competencia, ct);
        var primeiraSemCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .MinAsync(f => (DateOnly?)f.Competencia, ct);
        DateOnly? primeiraDoFaturamento =
            primeiraComCliente is null ? primeiraSemCliente
            : primeiraSemCliente is null ? primeiraComCliente
            : primeiraComCliente < primeiraSemCliente ? primeiraComCliente : primeiraSemCliente;

        // AS UNIDADES DO ART, pelo mesmo critério de data e pelo mesmo de-para de categoria do painel.
        var vendasDeMaquina = contexto.VendasDeMaquina.AsNoTracking()
            .Where(v => v.ExcluidoEm == null && (consulta.FilialDaVendaId == null || v.EmpresaId == consulta.FilialDaVendaId));
        var primeiraVenda = await DataDoCriterio(vendasDeMaquina, CriterioDasVendasDoArt.Criterio).MinAsync(ct);
        DateOnly? primeiroMesDoArt = primeiraVenda is { } dia ? new DateOnly(dia.Year, dia.Month, 1) : null;

        var codigoDaLinha = await contexto.LinhasDeProduto.AsNoTracking().ToDictionaryAsync(l => l.Id, l => l.Codigo, ct);
        var categoriaDaLinha = (await (
                    from ligacao in contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
                    join categoria in contexto.CategoriasDeMaquina.AsNoTracking() on ligacao.CategoriaDeMaquinaId equals categoria.Id
                    select new { ligacao.CodigoDaLinha, categoria.Codigo })
                .ToListAsync(ct))
            .ToDictionary(l => l.CodigoDaLinha, l => l.Codigo, StringComparer.Ordinal);

        var maquinas = idsDosClientes.Count == 0 || primeiroMesDoArt is null
            ? []
            : (await (
                    from venda in vendasDeMaquina.Where(v => idsDosClientes.Contains(v.CompradorId))
                    join maquina in contexto.Equipamentos.AsNoTracking().Where(e => e.ExcluidoEm == null)
                        on venda.EquipamentoId equals maquina.Id into doEquipamento
                    from maquina in doEquipamento.DefaultIfEmpty()
                    select new
                    {
                        LinhaDeProdutoId = maquina == null ? null : maquina.LinhaDeProdutoId,
                        venda.VendidaEm,
                        venda.FaturadaEm,
                        venda.EntregueEm
                    })
                .ToListAsync(ct))
                .Select(v => new
                {
                    Data = DataPeloCriterio(CriterioDasVendasDoArt.Criterio, v.VendidaEm, v.FaturadaEm, v.EntregueEm),
                    Categoria = v.LinhaDeProdutoId is { } linha && codigoDaLinha.TryGetValue(linha, out var codigo)
                                && categoriaDaLinha.TryGetValue(codigo, out var daCategoria)
                        ? daCategoria
                        : null
                })
                .Where(v => v.Data is { } data && data < ultimoMes.AddMonths(1))
                .Where(v => consulta.CategoriaDeMaquina is null || string.Equals(v.Categoria, consulta.CategoriaDeMaquina, StringComparison.Ordinal))
                .Select(v => new DateOnly(v.Data!.Value.Year, v.Data.Value.Month, 1))
                .ToList();

        AnoFiscalDoMunicipio Ano(int anoFiscal, Dominio.Comum.JanelaDeCompetencia meses)
        {
            var nome = Dominio.Comum.AnoFiscal.Nome(anoFiscal);
            var cobreVendas = primeiraDoFaturamento is { } primeira && primeira <= meses.Inicial;
            var cobreMaquinas = primeiroMesDoArt is { } primeiro && primeiro <= meses.Inicial;

            var linhas = faturamento.Where(f => meses.Contem(f.Competencia)).ToList();
            var vendas = !cobreVendas
                ? null
                : new VendasTerritoriais(
                    linhas.GroupBy(f => f.ClienteId).Count(c => c.Sum(f => f.ValorLiquido) != 0),
                    decimal.Round(linhas.Sum(f => f.ValorLiquido), 2),
                    decimal.Round(linhas.Sum(f => f.ValorEmMaquina), 2),
                    decimal.Round(linhas.Sum(f => f.ValorEmPeca), 2),
                    decimal.Round(linhas.Sum(f => f.ValorEmServico), 2),
                    decimal.Round(linhas.Sum(f => f.ValorEmOutros), 2));

            return new AnoFiscalDoMunicipio(
                anoFiscal,
                meses.Inicial,
                meses.Final,
                meses.Final < Dominio.Comum.AnoFiscal.Inteiro(anoFiscal).Final,
                vendas,
                cobreVendas
                    ? null
                    : primeiraDoFaturamento is { } desde
                        ? $"O faturamento carregado ao seu alcance começa em {Dominio.Comum.JanelaDeCompetencia.Mes(desde)}, depois do " +
                          $"começo do {nome} ({Dominio.Comum.JanelaDeCompetencia.Mes(meses.Inicial)}): o ano sairia pela metade, e " +
                          "pela metade ele se leria como queda."
                        : "Não há faturamento carregado ao alcance desta consulta.",
                cobreMaquinas ? maquinas.Count(meses.Contem) : null,
                cobreMaquinas
                    ? null
                    : primeiroMesDoArt is { } artDesde
                        ? $"A primeira venda que o ART trouxe é de {Dominio.Comum.JanelaDeCompetencia.Mes(artDesde)}, depois do " +
                          $"começo do {nome} ({Dominio.Comum.JanelaDeCompetencia.Mes(meses.Inicial)}): as unidades desse ano " +
                          "sairiam pela metade."
                        : "O ART não trouxe venda de máquina ao alcance desta consulta.");
        }

        // DO PRIMEIRO ANO FISCAL QUE ALGUMA DAS DUAS FONTES COBRE INTEIRO até o corrente. O ano que nenhuma
        // cobre não entra: ele seria uma linha de dois traços, e o motivo já está no primeiro ano que entra.
        static int? PrimeiroInteiro(DateOnly? desde) =>
            desde is not { } d ? null : d == Dominio.Comum.AnoFiscal.InicioDe(d) ? Dominio.Comum.AnoFiscal.Do(d) : Dominio.Comum.AnoFiscal.Do(d) + 1;

        var corrente = Dominio.Comum.AnoFiscal.Do(ultimoMes);
        var primeiroAno = new[] { PrimeiroInteiro(primeiraDoFaturamento), PrimeiroInteiro(primeiroMesDoArt) }
            .Where(a => a is not null)
            .Select(a => a!.Value)
            .DefaultIfEmpty(corrente)
            .Min();

        var anos = new List<AnoFiscalDoMunicipio>();
        for (var fy = Math.Min(primeiroAno, corrente); fy <= corrente; fy++)
        {
            var inteiro = Dominio.Comum.AnoFiscal.Inteiro(fy);
            anos.Add(Ano(fy, new Dominio.Comum.JanelaDeCompetencia(
                inteiro.Inicial, inteiro.Final < ultimoMes ? inteiro.Final : ultimoMes)));
        }

        // O MESMO TRECHO DO ANO ANTERIOR, quando o corrente ainda corre: é a comparação que a diretoria faz
        // (27/09/2026) — o ano fiscal até agosto contra o anterior até agosto.
        var doCorrente = anos[^1];
        var mesmoTrecho = doCorrente.EmCurso
            ? Ano(corrente - 1, new Dominio.Comum.JanelaDeCompetencia(doCorrente.Inicio, doCorrente.Fim).DoAnoAnterior())
            : null;

        return new HistoricoDoMunicipio(
            consulta.CodigoIbge,
            doCodigo[0].Nome,
            primeiraDoFaturamento,
            primeiroMesDoArt,
            anos,
            mesmoTrecho,
            await LerLavouraPorAnoAsync(idsDoMunicipio, ct));
    }

    /// <summary>
    /// A LAVOURA DE UM MUNICÍPIO EM CADA ANO DA PAM — o total e todas as culturas (issue 168).
    ///
    /// <para><b>O café entra uma vez só</b>, pelo Total: Arábica e Canephora saem da soma e da lista, como no
    /// resto da tela. <b>Sigilo não vira zero</b>: a cultura sem área divulgada não entra na lista, e o total
    /// que não tem nenhuma sai nulo.</para>
    /// </summary>
    private async Task<List<LavouraNoAno>> LerLavouraPorAnoAsync(IReadOnlyList<int> idsDoMunicipio, CancellationToken ct)
    {
        var linhas = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
            .Where(p => idsDoMunicipio.Contains(p.MunicipioId) && !ProdutosQueDuplicamNaSoma.Contains(p.ProdutoCodigoIbge))
            .Select(p => new
            {
                p.Ano,
                p.ProdutoCodigoIbge,
                p.ProdutoNome,
                p.AreaPlantadaHectares,
                p.AreaColhidaHectares,
                p.ValorDaProducaoMilReais
            })
            .ToListAsync(ct);

        static decimal? Somar(IEnumerable<decimal?> valores)
        {
            var divulgados = valores.Where(v => v is not null).ToList();
            return divulgados.Count == 0 ? null : divulgados.Sum(v => v!.Value);
        }

        return
        [
            .. linhas
                .GroupBy(l => l.Ano)
                .OrderBy(g => g.Key)
                .Select(g => new LavouraNoAno(
                    g.Key,
                    Somar(g.Select(l => l.AreaPlantadaHectares)),
                    Somar(g.Select(l => l.AreaColhidaHectares)),
                    Somar(g.Select(l => l.ValorDaProducaoMilReais)),
                    [
                        .. g.Where(l => l.AreaPlantadaHectares > 0)
                            .OrderByDescending(l => l.AreaPlantadaHectares)
                            .ThenBy(l => l.ProdutoCodigoIbge)
                            .Select(l => new CulturaDaLavoura(
                                l.ProdutoCodigoIbge, l.ProdutoNome, l.AreaPlantadaHectares, l.AreaColhidaHectares,
                                l.ValorDaProducaoMilReais))
                    ]))
        ];
    }
}
