using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O DIMENSIONAMENTO DA ADR (issue 259) — a PAM dos 645 municípios de São Paulo e do estado em dois anos, cultura a cultura,
/// e a carteira comercial com as faixas de dias desde o último contato.
///
/// <para><b>As somas são em memória</b>, como as vendas dos Indicadores: o SQLite dos testes de API não agrega decimal. O
/// volume é o da PAM de dois anos — cada município tem algumas dezenas de produtos divulgados —, e não milhões de linhas.</para>
///
/// <para><b>A cultura de cada produto vem do catálogo</b> (<see cref="ProdutoDaPamNaCultura"/>), e não de uma lista no código:
/// o produto que entra na soma vai para a cultura dele; o detalhado de um total (Arábica e Canephora, dentro do "Café Total")
/// fica fora, para o café não contar duas vezes; o resto vai para as outras culturas.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
public sealed class RepositorioDoDimensionamento(CrmDbContext contexto) : IRepositorioDoDimensionamento
{
    private const int CodigoDeSaoPaulo = 35;
    private const string SaoPaulo = "SP";

    /// <inheritdoc />
    public async Task<IReadOnlyList<short>> AnosDaPamAsync(CancellationToken ct) =>
        await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
            .Select(p => p.Ano)
            .Distinct()
            .OrderByDescending(a => a)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<ProducaoParaODimensionamento> LerProducaoAsync(IReadOnlyCollection<short> anos, CancellationToken ct)
    {
        var anosPedidos = anos.Distinct().ToList();

        var culturas = await contexto.Culturas.AsNoTracking()
            .Where(c => c.EstaAtiva)
            .OrderBy(c => c.Id)
            .Select(c => new { c.Id, c.Codigo, c.Nome })
            .ToListAsync(ct);
        var codigoDaCultura = culturas.ToDictionary(c => c.Id, c => c.Codigo);

        // O DESTINO DE CADA PRODUTO LIGADO A UMA CULTURA: a cultura, quando ele entra na soma; nenhum (nulo), quando é o
        // detalhado de um total. Produto de cultura desativada vai para as outras, como produto sem cultura.
        var destinoDoProduto = (await contexto.ProdutosDaPamNasCulturas.AsNoTracking()
                .Select(p => new { p.CulturaId, p.ProdutoCodigoIbge, p.EntraNaSomaDaLavoura })
                .ToListAsync(ct))
            .ToDictionary(
                p => p.ProdutoCodigoIbge,
                p => !p.EntraNaSomaDaLavoura
                    ? null
                    : codigoDaCultura.GetValueOrDefault(p.CulturaId) ?? CulturaDoDimensionamento.Outras);

        var municipios = await contexto.Municipios.AsNoTracking()
            .Where(m => m.Uf == SaoPaulo && m.CodigoIbge != null)
            .Select(m => new { m.Id, Codigo = m.CodigoIbge!.Value, m.Nome })
            .ToListAsync(ct);
        var codigoDoMunicipio = municipios.ToDictionary(m => m.Id, m => m.Codigo);

        var area = (await (
                    from linha in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking().Where(a => a.EncerradoEm == null)
                    join loja in contexto.Empresas.AsNoTracking() on linha.EmpresaResponsavelId equals (int?)loja.Id into lojas
                    from loja in lojas.DefaultIfEmpty()
                    orderby linha.Id
                    select new
                    {
                        linha.MunicipioId,
                        linha.PertenceAAdr,
                        linha.Regiao,
                        LojaCodigo = loja == null ? null : loja.Codigo,
                        LojaNome = loja == null ? null : loja.Nome
                    })
                .ToListAsync(ct))
            .Where(a => codigoDoMunicipio.ContainsKey(a.MunicipioId))
            .GroupBy(a => codigoDoMunicipio[a.MunicipioId])
            .ToDictionary(g => g.Key, g => g.First());

        var usinas = (await contexto.UsinasDeEtanol.AsNoTracking()
                .Where(u => u.EncerradaEm == null)
                .Select(u => u.MunicipioId)
                .ToListAsync(ct))
            .Where(codigoDoMunicipio.ContainsKey)
            .GroupBy(id => codigoDoMunicipio[id])
            .ToDictionary(g => g.Key, g => g.Count());

        var linhasDosMunicipios = await (
                from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                where anosPedidos.Contains(linha.Ano) && municipio.Uf == SaoPaulo && municipio.CodigoIbge != null
                select new LinhaDaPam(
                    municipio.CodigoIbge,
                    linha.Ano,
                    linha.ProdutoCodigoIbge,
                    linha.AreaPlantadaHectares,
                    linha.AreaColhidaHectares,
                    linha.QuantidadeProduzida,
                    linha.ValorDaProducaoMilReais))
            .ToListAsync(ct);

        var linhasDoEstado = await contexto.ProducoesAgricolasNosEstados.AsNoTracking()
            .Where(p => p.EstadoCodigoIbge == CodigoDeSaoPaulo && anosPedidos.Contains(p.Ano))
            .Select(p => new LinhaDaPam(
                null, p.Ano, p.ProdutoCodigoIbge, p.AreaPlantadaHectares, p.AreaColhidaHectares, p.QuantidadeProduzida, p.ValorDaProducaoMilReais))
            .ToListAsync(ct);

        return new ProducaoParaODimensionamento(
            [
                .. culturas.Select(c => new CulturaDoDimensionamento(c.Codigo, c.Nome)),
                new CulturaDoDimensionamento(CulturaDoDimensionamento.Outras, "Outras")
            ],
            [
                .. municipios
                    .GroupBy(m => m.Codigo)
                    .Select(g => g.OrderBy(m => m.Id).First())
                    .Select(m =>
                    {
                        var daArea = area.GetValueOrDefault(m.Codigo);
                        return new MunicipioNoDimensionamento(
                            m.Codigo,
                            m.Nome,
                            daArea?.PertenceAAdr ?? false,
                            daArea?.Regiao.ToString(),
                            daArea?.LojaCodigo,
                            daArea?.LojaNome,
                            usinas.GetValueOrDefault(m.Codigo));
                    })
                    .OrderBy(m => m.CodigoIbge)
            ],
            Somar(linhasDosMunicipios, destinoDoProduto),
            Somar(linhasDoEstado, destinoDoProduto));
    }

    private sealed record LinhaDaPam(
        int? Codigo,
        short Ano,
        int Produto,
        decimal? AreaPlantada,
        decimal? AreaColhida,
        decimal? Quantidade,
        decimal? Valor);

    /// <summary>
    /// SOMA A PAM POR LUGAR, ANO E CULTURA. Nulo continua nulo — a soma só existe com pelo menos um valor divulgado. A
    /// quantidade soma só os produtos em toneladas: abacaxi e coco vêm em mil frutos e não têm como entrar.
    /// </summary>
    private static List<ProducaoDaCultura> Somar(IEnumerable<LinhaDaPam> linhas, IReadOnlyDictionary<int, string?> destinoDoProduto) =>
    [
        .. linhas
            .Select(l => (Linha: l, Cultura: destinoDoProduto.TryGetValue(l.Produto, out var destino) ? destino : CulturaDoDimensionamento.Outras))
            .Where(x => x.Cultura is not null)
            .GroupBy(x => (x.Linha.Codigo, x.Linha.Ano, Cultura: x.Cultura!))
            .Select(g => new ProducaoDaCultura(
                g.Key.Codigo,
                g.Key.Ano,
                g.Key.Cultura,
                Numeros.SomaOuNulo(g.Select(x => x.Linha.AreaPlantada)),
                Numeros.SomaOuNulo(g.Select(x => x.Linha.AreaColhida)),
                Numeros.SomaOuNulo(g.Where(x => UnidadesDaPam.DaQuantidade(x.Linha.Produto, x.Linha.Ano).EhMassa).Select(x => x.Linha.Quantidade)),
                Numeros.SomaOuNulo(g.Select(x => x.Linha.Valor))))
    ];

    /// <inheritdoc />
    public async Task<CarteiraParaODimensionamento> LerCarteiraAsync(
        ConsultaDaCarteiraNoDimensionamento consulta, DateTime agoraUtc, CancellationToken ct)
    {
        // A VISÃO DA EMPRESA ABRE O ALCANCE ENTRE FILIAIS, e só ela — o caso de uso já conferiu a permissão.
        using var alcance = consulta.Visao == VisaoTerritorial.Empresa
            ? contexto.AbrirAlcanceEntreEmpresas("Visão consolidada da empresa no Dimensionamento da ADR (issue 259)")
            : null;

        var municipios = await contexto.Municipios.AsNoTracking()
            .Select(m => new { m.Id, m.CodigoIbge, m.Uf })
            .ToDictionaryAsync(m => m.Id, ct);

        var daAdr = (await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                .Where(a => a.EncerradoEm == null && a.PertenceAAdr)
                .Select(a => a.MunicipioId)
                .ToListAsync(ct))
            .Where(municipios.ContainsKey)
            .Select(id => municipios[id].CodigoIbge)
            .OfType<int>()
            .ToHashSet();

        // O MUNICÍPIO DE CADA CLIENTE é o do endereço principal; mais de um principal, vale o de menor Id — a mesma regra
        // dos Indicadores, para os dois contarem o cliente no mesmo lugar.
        var linhasDeCliente = await (
                from cliente in contexto.Clientes.AsNoTracking().Where(c => c.ExcluidoEm == null)
                join endereco in contexto.Enderecos.AsNoTracking().Where(e => e.EhPrincipal && e.ExcluidoEm == null)
                    on cliente.Id equals endereco.ClienteId into enderecos
                from endereco in enderecos.DefaultIfEmpty()
                orderby cliente.Id, endereco.Id
                select new { cliente.Id, cliente.Classe, endereco.MunicipioId })
            .ToListAsync(ct);

        var clientes = new Dictionary<long, ClienteNaCarteira>();
        foreach (var linha in linhasDeCliente)
        {
            if (clientes.ContainsKey(linha.Id)) continue;

            int? codigo = linha.MunicipioId is { } id && municipios.TryGetValue(id, out var municipio) && municipio.Uf == SaoPaulo
                ? municipio.CodigoIbge
                : null;
            clientes[linha.Id] = new ClienteNaCarteira(linha.Classe, codigo, codigo is { } c && daAdr.Contains(c));
        }

        var vinculos = (await (
                    from vinculo in contexto.ClienteCarteiras.AsNoTracking().Where(v => v.DesvinculadoEm == null)
                    join carteira in contexto.Carteiras.AsNoTracking() on vinculo.CarteiraId equals carteira.Id
                    where carteira.ExcluidoEm == null
                          && carteira.Natureza == NaturezaDaCarteira.Comercial
                          && (consulta.ResponsavelId == null || carteira.ResponsavelId == consulta.ResponsavelId)
                    select new { vinculo.ClienteId, carteira.ResponsavelId, vinculo.UltimaInteracaoEm })
                .ToListAsync(ct))
            // SÓ O QUE ESTÁ NO RECORTE: o cliente ao alcance, da classe pedida e — com recorte de território — com endereço
            // num município dele.
            .Where(v => clientes.TryGetValue(v.ClienteId, out var cliente)
                        && (consulta.Classe is null || cliente.Classe == consulta.Classe)
                        && (consulta.Municipios is null || cliente.CodigoIbge is { } c && consulta.Municipios.Contains(c)))
            .ToList();

        var idsDeResponsavel = vinculos.Select(v => v.ResponsavelId).Distinct().ToList();
        var responsaveis = await contexto.Usuarios.AsNoTracking()
            .Where(u => idsDeResponsavel.Contains(u.Id))
            .Select(u => new { u.Id, u.NomeExibicao, u.Natureza })
            .ToDictionaryAsync(u => u.Id, ct);

        string NomeDe(long id) => responsaveis.TryGetValue(id, out var u) ? u.NomeExibicao : "Responsável fora do alcance desta consulta";

        // O ÚLTIMO CONTATO DE CADA CLIENTE é o mais recente dos vínculos que valem para o recorte.
        var porCliente = vinculos
            .GroupBy(v => v.ClienteId)
            .Select(g => (Id: g.Key, Cliente: clientes[g.Key], Ultimo: g.Max(v => v.UltimaInteracaoEm)))
            .ToList();

        var total = ClientesDaCarteira.Contar(porCliente.Select(c => (c.Cliente.Classe, c.Ultimo)), agoraUtc);
        var naRegiao = porCliente.Count(c => c.Cliente.NaAdr);

        var responsavelPorVinculos = vinculos
            .Where(v => clientes[v.ClienteId].CodigoIbge is not null)
            .GroupBy(v => (Codigo: clientes[v.ClienteId].CodigoIbge!.Value, v.ResponsavelId))
            .Select(g => (g.Key.Codigo, g.Key.ResponsavelId, Vinculos: g.Count()))
            .GroupBy(x => x.Codigo)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Vinculos).ThenBy(x => NomeDe(x.ResponsavelId), StringComparer.Ordinal).First().ResponsavelId);

        var porMunicipio = porCliente
            .Where(c => c.Cliente.CodigoIbge is not null)
            .GroupBy(c => c.Cliente.CodigoIbge!.Value)
            .Select(g => new CarteiraNoMunicipio(
                g.Key,
                ClientesDaCarteira.Contar(g.Select(c => (c.Cliente.Classe, c.Ultimo)), agoraUtc),
                responsavelPorVinculos.TryGetValue(g.Key, out var principal) ? NomeDe(principal) : null))
            .OrderBy(m => m.CodigoIbge)
            .ToList();

        // CADA RESPONSÁVEL CONTA O CLIENTE PELOS VÍNCULOS DELE: o último contato é o das carteiras dele, e não o de outro CEN
        // que falou com o mesmo cliente.
        var porResponsavel = vinculos
            .GroupBy(v => v.ResponsavelId)
            .Select(g =>
            {
                var dele = g.GroupBy(v => v.ClienteId).Select(c => (Cliente: clientes[c.Key], Ultimo: c.Max(v => v.UltimaInteracaoEm))).ToList();
                return new CarteiraDoResponsavel(
                    g.Key,
                    NomeDe(g.Key),
                    responsaveis.TryGetValue(g.Key, out var u) ? u.Natureza.ToString() : "NaoIdentificado",
                    dele.Count(c => c.Cliente.NaAdr),
                    dele.Count(c => !c.Cliente.NaAdr),
                    ClientesDaCarteira.Contar(dele.Select(c => (c.Cliente.Classe, c.Ultimo)), agoraUtc));
            })
            .OrderByDescending(r => r.Clientes.Clientes)
            .ThenBy(r => r.Nome, StringComparer.Ordinal)
            .ToList();

        return new CarteiraParaODimensionamento(total, naRegiao, porCliente.Count - naRegiao, porMunicipio, porResponsavel);
    }

    private sealed record ClienteNaCarteira(ClasseDeCliente? Classe, int? CodigoIbge, bool NaAdr);
}
