using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS TRÊS MAPAS DA ADR, apurados município a município (documento 32, seção 8).
///
/// <para><b>O município do cliente é o do endereço principal</b>, pelo código IBGE do catálogo.
/// Não é o local da entrega nem o da propriedade — nenhum dos dois existe no dado —, e a tela diz
/// isso. Cliente sem município, com município sem código IBGE ou fora de São Paulo não some: vai
/// para <see cref="IndicadoresForaDoMapa"/>, e a soma do mapa com o que ficou fora dele é o total
/// da consulta. A nota sem cliente no CRM (<see cref="FaturamentoSemCliente"/>) também entra, num
/// grupo por natureza da contraparte — sem ela o total seria só o faturamento com cadastro.</para>
///
/// <para><b>Como em todo repositório, não há <c>Where</c> de empresa aqui.</b> Cliente, endereço,
/// carteira e faturamento entram pelo filtro global; área de atuação, responsáveis e área plantada
/// não têm dono de filial e não são filtrados.</para>
///
/// <para><b>Uma leitura por fonte, e a junção em memória.</b> São dezenas de milhares de vínculos,
/// não milhões; um único SQL com todas as junções faria o banco multiplicar cliente por vínculo por
/// mês de faturamento antes de agrupar.</para>
/// </summary>
public sealed class RepositorioDeIndicadoresTerritoriais(CrmDbContext contexto) : IRepositorioIndicadoresTerritoriais
{
    private const string SaoPaulo = "SP";

    private const int SemMunicipio = -1;
    private const int MunicipioSemCodigoIbge = -2;
    private const int OutraUf = -3;
    private const int ClienteDeOutraFilial = -4;
    private const int ClienteInativado = -5;
    private const int NotaSemCadastroDeCliente = -6;
    private const int NotaParaFabrica = -7;
    private const int NotaParaEmpresaDoGrupo = -8;
    private const int NotaParaOutraRevenda = -9;

    /// <summary>Grupo de quem ficou fora do recorte pedido — não é somado em lugar nenhum.</summary>
    private const int ForaDoFiltro = int.MinValue;

    // =============================================================================================
    // Os códigos do IBGE que esta consulta cita, com nome (issue 65)
    // =============================================================================================

    private const int CodigoDeSaoPaulo = 35;

    /// <summary>"Bovino", na classificação 79 da Pesquisa da Pecuária Municipal.</summary>
    private const int Bovino = 2670;

    /// <summary>"Total", na classificação 12605 (potência dos tratores).</summary>
    private const int PotenciaTotal = 113521;

    /// <summary>"Menos de 100 cv".</summary>
    private const int PotenciaAbaixoDe100Cv = 113522;

    /// <summary>"De 100 cv e mais".</summary>
    private const int PotenciaDe100CvEMais = 113523;

    /// <summary>"Total", na classificação 220 (grupos de área total).</summary>
    private const int GrupoDeAreaTotal = 110085;

    // As tabelas e variáveis do SIDRA que identificam a linha publicada do estado (issue 155). Elas
    // estão repetidas do leitor de propósito: a consulta não depende do projeto de integração, e o
    // banco guarda o número do SIDRA justamente para que a leitura seja conferível na origem.

    /// <summary>Censo Agropecuário, tratores por potência.</summary>
    private const short TabelaDaFrotaDeTratores = 6871;

    /// <summary>"Número de tratores existentes nos estabelecimentos agropecuários" (6871).</summary>
    private const short VariavelDeTratores = 1862;

    /// <summary>Censo Agropecuário, estabelecimentos por grupo de área total.</summary>
    private const short TabelaDeEstabelecimentos = 6780;

    /// <summary>"Número de estabelecimentos agropecuários" (6780).</summary>
    private const short VariavelDeEstabelecimentos = 183;

    /// <summary>Pesquisa da Pecuária Municipal, efetivo dos rebanhos.</summary>
    private const short TabelaDoRebanho = 3939;

    /// <summary>"Efetivo dos rebanhos", em cabeças (3939).</summary>
    private const short VariavelDoEfetivoDoRebanho = 105;

    /// <summary>
    /// OS PRODUTOS QUE CONTARIAM DUAS VEZES numa soma de culturas.
    ///
    /// <para>A classificação 782 traz "Café (em grão) Total" (40139) ao lado de "Arábica" (40140) e
    /// "Canephora" (40141), e os três vêm na mesma resposta do SIDRA. A soma mantém o total e
    /// descarta os dois detalhados — é o mesmo critério da planilha do comercial, e é o que faz o
    /// número da tela bater com o do IBGE.</para>
    /// </summary>
    private static readonly int[] ProdutosQueDuplicamNaSoma = [40140, 40141];

    /// <summary>
    /// AS FAIXAS DE TAMANHO NO VOCABULÁRIO DO COMERCIAL, e as categorias do IBGE que compõem cada uma.
    ///
    /// <para>O IBGE publica 18 faixas, e a planilha do comercial as agrupa em nove. As 18 ficam no
    /// banco; este mapa é a leitura, e mudá-lo não pede recarga.</para>
    ///
    /// <para><b>A última faixa junta DUAS categorias do IBGE</b> — "de 2.500 a menos de 10.000 ha"
    /// (41139) e "de 10.000 ha e mais" (40645) —, porque a planilha pára em "mais de 2.500".</para>
    /// </summary>
    private static readonly (string Rotulo, int[] Grupos)[] FaixasDoComercial =
    [
        ("Menos de 20 ha", [111543, 111544, 111545, 111546, 111547, 111548, 111549, 111550, 111551, 111552]),
        ("De 20 a 50 ha", [111553]),
        ("De 50 a 100 ha", [111554]),
        ("De 100 a 200 ha", [111555]),
        ("De 200 a 500 ha", [111556]),
        ("De 500 a 1.000 ha", [111557]),
        ("De 1.000 a 2.500 ha", [111558]),
        ("Mais de 2.500 ha", [41139, 40645]),
        ("Produtor sem área", [111560])
    ];

    /// <summary>O município que não tem nenhuma das cinco fontes carregadas.</summary>
    private static readonly EstruturaDoMunicipio EstruturaVazia =
        new(null, null, null, null, null, null, [], null, null, null, []);

    /// <summary>As medidas de UMA cultura num município — o que o mapa C mostra no balão.</summary>
    private readonly record struct MedidasDaCultura(
        decimal? AreaPlantadaHectares, decimal? AreaColhidaHectares, decimal? QuantidadeProduzida, decimal? ValorDaProducaoMilReais);

    /// <summary>O rótulo da categoria de uma regra que não declarou categoria de máquina (D-IM-06 em aberto).</summary>
    private const string SemCategoriaCodigo = "SEM-CATEGORIA";

    private const string SemCategoriaNome = "Sem categoria declarada";

    /// <summary>
    /// UMA REGRA COMO O MOTOR A LÊ (issue 72): uma linha por cultura e categoria de máquina, já com os
    /// produtos da PAM que compõem a área da cultura e com o grupo de compartilhamento.
    /// </summary>
    /// <param name="CulturaCodigo">O código da cultura no catálogo, ou <c>PAM-{produto}</c> quando a regra não está no catálogo.</param>
    /// <param name="CulturaNome">O nome de exibição.</param>
    /// <param name="CategoriaCodigo">A categoria de máquina, ou <see cref="SemCategoriaCodigo"/>.</param>
    /// <param name="CategoriaNome">O nome da categoria.</param>
    /// <param name="HectaresPorMaquina">A regra vigente.</param>
    /// <param name="AnosDeRenovacao">O ciclo de troca, quando informado.</param>
    /// <param name="Confirmada">Se o comercial confirmou a regra (D-P01).</param>
    /// <param name="ProdutosDaPam">Os produtos da classificação 782 cuja área soma esta cultura.</param>
    /// <param name="GrupoCodigo">O grupo de compartilhamento desta cultura nesta categoria, quando há.</param>
    private sealed record RegraNoMotor(
        string CulturaCodigo,
        string CulturaNome,
        string CategoriaCodigo,
        string CategoriaNome,
        decimal HectaresPorMaquina,
        decimal? AnosDeRenovacao,
        bool Confirmada,
        IReadOnlyList<int> ProdutosDaPam,
        string? GrupoCodigo);

    /// <summary>O que o motor precisa saber, mais o de-para que devolve o número de cada regra à ficha dela.</summary>
    /// <param name="Regras">Uma linha por cultura e categoria.</param>
    /// <param name="PorProdutoDaRegra">Para cada produto com regra vigente, em que categoria e cultura ele caiu.</param>
    private sealed record CatalogoDoMotor(
        IReadOnlyList<RegraNoMotor> Regras,
        IReadOnlyDictionary<int, (string Categoria, string Cultura)> PorProdutoDaRegra);

    private static readonly (int Grupo, string Codigo, string Descricao)[] GruposForaDoMapa =
    [
        (SemMunicipio, "SemMunicipio",
            "Cliente sem endereço principal, ou com o município do endereço fora do catálogo (o texto do legado que a carga não casou)."),
        (MunicipioSemCodigoIbge, "MunicipioSemCodigoIbge",
            "O endereço aponta para uma linha do catálogo sem código IBGE — segunda grafia do mesmo município ou distrito. Não há polígono para ela."),
        (OutraUf, "OutraUf",
            "Cliente com endereço principal fora de São Paulo."),
        (ClienteDeOutraFilial, "ClienteDeOutraFilial",
            "Venda ou vínculo desta filial com cliente CADASTRADO em outra filial. O endereço do cliente pertence ao cadastro da outra filial e fica fora do seu alcance; na visão da empresa este grupo não existe, porque cada venda vai para o município do cliente."),
        (ClienteInativado, "ClienteInativado",
            "Venda ou vínculo de cliente inativado no CRM. Fica somado à parte para o total fechar, sem voltar a aparecer no mapa."),
        (NotaSemCadastroDeCliente, "ContraparteSemCadastro",
            "Nota desta filial para quem não tem cadastro de cliente no CRM: cliente não cadastrado, nota sem documento ou contraparte ainda não classificada. Sem cadastro não há endereço nem município. Conta documentos distintos, não clientes."),
        (NotaParaFabrica, "RepasseDeFabrica",
            "Nota desta filial para a fábrica. Tem CFOP de venda, mas não é cliente final: fica à parte para o total da filial fechar sem inflar a venda a cliente."),
        (NotaParaEmpresaDoGrupo, "EmpresaDoGrupo",
            "Nota desta filial para outra empresa do grupo. Não é cliente final."),
        (NotaParaOutraRevenda, "OutraRevenda",
            "Nota desta filial para outra concessionária (repasse entre revendas). Não é cliente final.")
    ];

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, int>> ListarFiliaisAsync(CancellationToken ct) =>
        await contexto.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);

    /// <inheritdoc />
    public async Task<IndicadoresTerritoriais> ApurarAsync(
        ConsultaDeIndicadoresTerritoriais consulta, DateTime agoraUtc, CancellationToken ct)
    {
        // A VISÃO DA EMPRESA ABRE O ALCANCE ENTRE FILIAIS, e só ela. O CrmDbContext recusa abrir
        // para quem não tem Empresa.AlcanceEntreFiliais em profundidade Organização e registra a
        // abertura no diário — o caso de uso já conferiu a permissão antes de chegar aqui.
        using var alcance = consulta.Visao == VisaoTerritorial.Empresa
            ? contexto.AbrirAlcanceEntreEmpresas("Visão consolidada da empresa nos indicadores territoriais (documento 32)")
            : null;

        var area = await (
                from linha in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking().Where(a => a.EncerradoEm == null)
                join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                join loja in contexto.Empresas.AsNoTracking() on linha.EmpresaResponsavelId equals (int?)loja.Id into lojas
                from loja in lojas.DefaultIfEmpty()
                where municipio.CodigoIbge != null
                select new
                {
                    Codigo = municipio.CodigoIbge!.Value,
                    municipio.Nome,
                    linha.PertenceAAdr,
                    linha.Regiao,
                    LojaCodigo = loja == null ? null : loja.Codigo,
                    LojaNome = loja == null ? null : loja.Nome,
                    LojaAtiva = loja == null ? (bool?)null : loja.EstaAtiva
                })
            .ToDictionaryAsync(a => a.Codigo, ct);

        var municipios = await contexto.Municipios.AsNoTracking()
            .Select(m => new { m.Id, m.CodigoIbge, m.Uf, m.Nome })
            .ToDictionaryAsync(m => m.Id, ct);

        // -----------------------------------------------------------------------------------------
        // O grupo de cada cliente: um município de SP, ou um dos grupos fora do mapa.
        // -----------------------------------------------------------------------------------------
        var clientes = await (
                from cliente in contexto.Clientes.AsNoTracking().Where(c => c.ExcluidoEm == null)
                join endereco in contexto.Enderecos.AsNoTracking().Where(e => e.EhPrincipal && e.ExcluidoEm == null)
                    on cliente.Id equals endereco.ClienteId into enderecos
                from endereco in enderecos.DefaultIfEmpty()
                orderby cliente.Id, endereco.Id
                select new { cliente.Id, cliente.EmpresaId, cliente.Classe, endereco.MunicipioId })
            .ToListAsync(ct);

        // O CLIENTE INATIVADO AO ALCANCE não some: a venda e o vínculo dele vão para um grupo
        // próprio. Sem isso, cairiam em "cliente de outra filial" — um rótulo errado para dinheiro
        // que é desta filial.
        var inativados = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm != null)
            .ToDictionaryAsync(c => c.Id, c => c.EmpresaId, ct);

        var grupoDoCliente = new Dictionary<long, int>();
        var classeDoCliente = new Dictionary<long, ClasseDeCliente?>();
        var acumuladores = new Dictionary<int, Acumulador>();

        Acumulador Do(int grupo) =>
            acumuladores.TryGetValue(grupo, out var existente) ? existente : acumuladores[grupo] = new Acumulador();

        foreach (var cliente in clientes)
        {
            // MAIS DE UM ENDEREÇO PRINCIPAL: vale o de menor Id, e a ordenação da consulta é o que
            // torna a escolha a mesma em toda execução.
            if (grupoDoCliente.ContainsKey(cliente.Id)) continue;

            classeDoCliente[cliente.Id] = cliente.Classe;

            if (consulta.FilialDoClienteId is { } filialDoCliente && cliente.EmpresaId != filialDoCliente)
            {
                grupoDoCliente[cliente.Id] = ForaDoFiltro;
                continue;
            }

            var grupo = cliente.MunicipioId is not { } municipioId || !municipios.TryGetValue(municipioId, out var municipio)
                ? SemMunicipio
                : municipio.CodigoIbge is null
                    ? MunicipioSemCodigoIbge
                    : municipio.Uf != SaoPaulo ? OutraUf : municipio.CodigoIbge.Value;

            grupoDoCliente[cliente.Id] = grupo;
            Do(grupo).Clientes++;
        }

        // CADA LINHA CAI EM EXATAMENTE UM GRUPO, ou em nenhum quando o filtro a exclui. É o que
        // impede dupla contagem: não existe caminho que some a mesma venda em dois lugares.
        int GrupoDe(long clienteId)
        {
            if (grupoDoCliente.TryGetValue(clienteId, out var grupo)) return grupo;

            if (inativados.TryGetValue(clienteId, out var filialDoInativado))
                return consulta.FilialDoClienteId is { } filtro && filialDoInativado != filtro ? ForaDoFiltro : ClienteInativado;

            // Cliente que não está ao alcance só existe na visão da filial: é cadastro de outra.
            // Com filtro de filial do cliente, o caso de uso já garantiu que essa filial está ao
            // alcance — logo, cliente fora do alcance está fora do filtro.
            return consulta.FilialDoClienteId is null ? ClienteDeOutraFilial : ForaDoFiltro;
        }

        // -----------------------------------------------------------------------------------------
        // Cobertura: vínculo em carteira comercial, contra a cadência da linha de negócio.
        // -----------------------------------------------------------------------------------------
        var carteirasComerciais = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Natureza == NaturezaDaCarteira.Comercial)
            .Select(c => new
            {
                c.Id,
                c.ResponsavelId,
                Cadencia = contexto.LinhasDeNegocio
                    .Where(l => l.Id == c.LinhaDeNegocioId)
                    .Select(l => new { l.DiasCicloClasseA, l.DiasCicloClasseB, l.DiasCicloClasseC, l.DiasCicloClasseD })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var cadencias = carteirasComerciais.ToDictionary(c => c.Id, c => c.Cadencia);
        var responsavelDaCarteira = carteirasComerciais.ToDictionary(c => c.Id, c => c.ResponsavelId);

        var idsDeCarteira = cadencias.Keys.ToList();

        var vinculos = await contexto.ClienteCarteiras.AsNoTracking()
            .Where(v => v.DesvinculadoEm == null && idsDeCarteira.Contains(v.CarteiraId))
            .Select(v => new { v.CarteiraId, v.ClienteId, v.UltimaInteracaoEm })
            .ToListAsync(ct);

        // OS RESPONSÁVEIS REAIS SÃO OS DAS CARTEIRAS, e não nomes digitados na tela: o nome sai do cadastro
        // de usuário, pelo mesmo filtro de alcance de todo o resto.
        var idsDeResponsavel = responsavelDaCarteira.Values.Distinct().ToList();
        var responsaveisDasCarteiras = await contexto.Usuarios.AsNoTracking()
            .Where(u => idsDeResponsavel.Contains(u.Id))
            .Select(u => new { u.Id, u.NomeExibicao, u.Natureza })
            .ToDictionaryAsync(u => u.Id, ct);

        foreach (var vinculo in vinculos)
        {
            var grupoDoVinculo = GrupoDe(vinculo.ClienteId);
            if (grupoDoVinculo == ForaDoFiltro) continue;

            var acumulador = Do(grupoDoVinculo);
            var cadencia = cadencias[vinculo.CarteiraId];

            // SEM CLASSE APURADA, O VÍNCULO CONTA COMO D — a mesma regra do painel do CEN.
            short? dias = (classeDoCliente.GetValueOrDefault(vinculo.ClienteId) ?? ClasseDeCliente.D) switch
            {
                ClasseDeCliente.A => cadencia?.DiasCicloClasseA,
                ClasseDeCliente.B => cadencia?.DiasCicloClasseB,
                ClasseDeCliente.C => cadencia?.DiasCicloClasseC,
                _ => cadencia?.DiasCicloClasseD
            };

            acumulador.Vinculos++;

            var responsavel = responsavelDaCarteira[vinculo.CarteiraId];
            acumulador.VinculosPorResponsavel[responsavel] = acumulador.VinculosPorResponsavel.GetValueOrDefault(responsavel) + 1;
            if (!acumulador.CarteirasPorResponsavel.TryGetValue(responsavel, out var carteirasDoResponsavel))
                acumulador.CarteirasPorResponsavel[responsavel] = carteirasDoResponsavel = [];
            carteirasDoResponsavel.Add(vinculo.CarteiraId);

            if (dias is null) acumulador.SemCadencia++;
            else if (vinculo.UltimaInteracaoEm is null) acumulador.NuncaContatados++;
            else if (vinculo.UltimaInteracaoEm >= agoraUtc.AddDays(-dias.Value)) acumulador.Cobertos++;
            else acumulador.ForaDaCadencia++;
        }

        // -----------------------------------------------------------------------------------------
        // Vendas: as linhas mensais do período, somadas aqui.
        //
        // A SOMA É EM MEMÓRIA, e não um GroupBy no banco, por uma razão de portabilidade medida: o
        // SQLite dos testes de API não agrega decimal, e a rota ficaria sem teste de ponta a ponta.
        // O volume é o de linhas cliente × mês de um período de até 36 meses ao alcance da filial —
        // dezenas de milhares, não milhões.
        // -----------------------------------------------------------------------------------------
        var faturamento = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null
                        && f.Competencia >= consulta.CompetenciaInicial
                        && f.Competencia <= consulta.CompetenciaFinal
                        && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .Select(f => new { f.ClienteId, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros })
            .ToListAsync(ct);

        foreach (var cliente in faturamento.GroupBy(f => f.ClienteId))
        {
            var grupoDaVenda = GrupoDe(cliente.Key);
            if (grupoDaVenda == ForaDoFiltro) continue;

            var acumulador = Do(grupoDaVenda);
            var liquido = cliente.Sum(f => f.ValorLiquido);

            acumulador.ValorLiquido += liquido;
            acumulador.Maquina += cliente.Sum(f => f.ValorEmMaquina);
            acumulador.Peca += cliente.Sum(f => f.ValorEmPeca);
            acumulador.Servico += cliente.Sum(f => f.ValorEmServico);
            acumulador.Outros += cliente.Sum(f => f.ValorEmOutros);
            if (liquido != 0) acumulador.ClientesQueCompraram++;
        }

        // A NOTA SEM CLIENTE NO CRM TAMBÉM É NOTA DESTA FILIAL. Sem ela, o total da tela seria só o
        // faturamento com cliente — R$ 320,6 mi a menos em 12 meses, medido em 13/09/2026 — e a
        // conferência "mapa + fora do mapa = o que a filial faturou" nunca fecharia. Ela não tem
        // cadastro, logo não tem filial de cadastro: o filtro pela filial do cliente a exclui.
        if (consulta.FilialDoClienteId is null)
        {
            var semCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
                .Where(f => f.ExcluidoEm == null
                            && f.Competencia >= consulta.CompetenciaInicial
                            && f.Competencia <= consulta.CompetenciaFinal
                            && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
                .Select(f => new { f.Documento, f.Natureza, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros })
                .ToListAsync(ct);

            foreach (var contraparte in semCliente.GroupBy(f => (Grupo: GrupoDaNatureza(f.Natureza), f.Documento)))
            {
                var acumulador = Do(contraparte.Key.Grupo);
                var liquido = contraparte.Sum(f => f.ValorLiquido);

                acumulador.ValorLiquido += liquido;
                acumulador.Maquina += contraparte.Sum(f => f.ValorEmMaquina);
                acumulador.Peca += contraparte.Sum(f => f.ValorEmPeca);
                acumulador.Servico += contraparte.Sum(f => f.ValorEmServico);
                acumulador.Outros += contraparte.Sum(f => f.ValorEmOutros);
                if (liquido != 0) acumulador.ClientesQueCompraram++;
            }
        }

        // -----------------------------------------------------------------------------------------
        // Potencial: a área plantada do último ano, para os produtos das regras vigentes HOJE (issue 71:
        // a regra tem vigência, e a de hoje é a que o mapa aplica — uma vigência futura ainda não vale).
        // -----------------------------------------------------------------------------------------
        var hoje = ParametroComVigencia.HojeNoBrasil(agoraUtc);
        var regras = (await contexto.RegrasDePotencial.AsNoTracking()
                .Where(r => r.RevogadoEm == null && r.VigenteDesde <= hoje)
                .ToListAsync(ct))
            .GroupBy(r => r.ProdutoCodigoIbge)
            .Select(g => ParametroComVigencia.VigenteEm(g, hoje)!)
            .OrderBy(r => r.ProdutoCodigoIbge)
            .ToList();

        // O MOTOR (issue 72) lê a regra pela CULTURA do catálogo, e a área de uma cultura é a dos produtos
        // que entram na soma dela — por isso a leitura da PAM abre para eles, e não só para o produto da regra.
        var catalogoDoMotor = await LerCatalogoDoMotorAsync(regras, ct);

        var produtos = regras.Select(r => r.ProdutoCodigoIbge)
            .Concat(catalogoDoMotor.Regras.SelectMany(r => r.ProdutosDaPam))
            .Distinct().ToList();

        var ano = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking().MaxAsync(a => (short?)a.Ano, ct);
        var daRegra = new Dictionary<(int Codigo, int Produto), MedidasDaCultura>();

        // CADA CULTURA NO SEU ANO (issue 152): o último em que a área plantada DELA foi divulgada. O maior ano da
        // tabela inteira — o que valia até aqui — misturaria anos em silêncio no dia em que a PAM nova entrasse
        // incompleta: a cultura que ainda não chegou apareceria "sem dado", e não com o ano anterior dela.
        var anoDaCultura = produtos.Count == 0
            ? new Dictionary<int, short>()
            : (await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    .Where(p => produtos.Contains(p.ProdutoCodigoIbge) && p.AreaPlantadaHectares != null)
                    .GroupBy(p => p.ProdutoCodigoIbge)
                    .Select(g => new { Produto = g.Key, Ano = g.Max(p => p.Ano) })
                    .ToListAsync(ct))
                .ToDictionary(c => c.Produto, c => c.Ano);

        if (anoDaCultura.Count > 0)
        {
            var anosDasCulturas = anoDaCultura.Values.Distinct().ToList();
            var linhas = await (
                    from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                    where anosDasCulturas.Contains(linha.Ano) && produtos.Contains(linha.ProdutoCodigoIbge) && municipio.CodigoIbge != null
                    select new
                    {
                        Codigo = municipio.CodigoIbge!.Value,
                        linha.ProdutoCodigoIbge,
                        linha.Ano,
                        linha.AreaPlantadaHectares,
                        linha.AreaColhidaHectares,
                        linha.QuantidadeProduzida,
                        linha.ValorDaProducaoMilReais
                    })
                .ToListAsync(ct);

            foreach (var linha in linhas.Where(l => anoDaCultura.GetValueOrDefault(l.ProdutoCodigoIbge) == l.Ano))
                daRegra[(linha.Codigo, linha.ProdutoCodigoIbge)] = new MedidasDaCultura(
                    linha.AreaPlantadaHectares, linha.AreaColhidaHectares, linha.QuantidadeProduzida, linha.ValorDaProducaoMilReais);
        }

        var culturasNoEstado = await LerCulturasNoEstadoAsync(anoDaCultura, ct);

        // -----------------------------------------------------------------------------------------
        // A lavoura inteira: a soma de TODAS as culturas do município, no mesmo ano.
        //
        // O CAFÉ ENTRA UMA VEZ SÓ. A classificação 782 traz "Café (em grão) Total" ao lado de Arábica
        // e Canephora, e os três na mesma resposta; somar os três contaria o café duas vezes. Ficam
        // de fora os dois detalhados, e fica o total — o mesmo critério da planilha do comercial.
        //
        // A QUANTIDADE PRODUZIDA NÃO É SOMADA, de propósito: o IBGE publica cada produto na unidade
        // dele (tonelada, e mil frutos no abacaxi e no coco — UnidadesDaPam), e um total disso não teria unidade nenhuma.
        // -----------------------------------------------------------------------------------------
        var producao = new Dictionary<int, ProducaoAgricolaDoMunicipio>();

        if (ano is not null)
        {
            var somas = await (
                    from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                    join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                    where linha.Ano == ano
                          && municipio.CodigoIbge != null
                          && !ProdutosQueDuplicamNaSoma.Contains(linha.ProdutoCodigoIbge)
                    group linha by municipio.CodigoIbge!.Value into porMunicipio
                    select new
                    {
                        Codigo = porMunicipio.Key,
                        Plantada = porMunicipio.Sum(l => l.AreaPlantadaHectares),
                        Colhida = porMunicipio.Sum(l => l.AreaColhidaHectares),
                        Valor = porMunicipio.Sum(l => l.ValorDaProducaoMilReais),
                        Culturas = porMunicipio.Count(l => l.AreaPlantadaHectares > 0)
                    })
                .ToListAsync(ct);

            foreach (var soma in somas)
                producao[soma.Codigo] = new ProducaoAgricolaDoMunicipio(
                    ano.Value, soma.Plantada, soma.Colhida, soma.Valor, soma.Culturas);
        }

        var estrutura = await LerEstruturaAsync(ct);
        var totaisDoEstado = await LerTotaisDoEstadoAsync(ano, ct);

        // -----------------------------------------------------------------------------------------
        // A montagem: os municípios de SP da área de atuação ou com dado, depois dos filtros.
        // -----------------------------------------------------------------------------------------
        var filtrado = consulta.Regiao is not null || consulta.LojaCodigo is not null;

        var nomeOficial = municipios.Values
            .Where(m => m.CodigoIbge is not null && m.Uf == SaoPaulo)
            .GroupBy(m => m.CodigoIbge!.Value)
            .ToDictionary(g => g.Key, g => g.First().Nome);

        List<ResponsavelPelaCarteira> ResponsaveisDe(Acumulador acumulador) =>
        [
            .. acumulador.VinculosPorResponsavel
                .OrderByDescending(r => r.Value).ThenBy(r => r.Key)
                .Select(r => responsaveisDasCarteiras.TryGetValue(r.Key, out var usuario)
                    ? new ResponsavelPelaCarteira(usuario.NomeExibicao, usuario.Natureza.ToString(), r.Value, acumulador.CarteirasPorResponsavel[r.Key].Count)
                    : new ResponsavelPelaCarteira("Responsável fora do alcance desta consulta", "NaoIdentificado", r.Value, acumulador.CarteirasPorResponsavel[r.Key].Count))
        ];

        // -----------------------------------------------------------------------------------------
        // O MOTOR DO POTENCIAL (issue 72). Até aqui o mapa C dividia a área pela regra dentro deste
        // arquivo; agora ele pede o número ao domínio, que já desconta a terra compartilhada entre
        // culturas (issue 160), soma as categorias de máquina sem somar a terra delas, e diz por que um
        // número não saiu em vez de devolver zero.
        // -----------------------------------------------------------------------------------------
        var categoriasDoMotor = catalogoDoMotor.Regras
            .GroupBy(r => (r.CategoriaCodigo, r.CategoriaNome))
            .OrderBy(g => g.Key.CategoriaCodigo, StringComparer.Ordinal)
            .ToList();

        // A ÁREA DE UMA CULTURA É A SOMA DOS PRODUTOS DELA, cada um no ano dele. Nenhum produto com área
        // divulgada devolve NULO, e não zero: sigilo do IBGE não é ausência de lavoura.
        decimal? AreaDaCultura(int municipio, IReadOnlyList<int> produtosDaCultura)
        {
            decimal? soma = null;

            foreach (var produto in produtosDaCultura)
                if (daRegra.TryGetValue((municipio, produto), out var medida) && medida.AreaPlantadaHectares is { } plantada)
                    soma = (soma ?? 0m) + plantada;

            return soma;
        }

        List<(string Codigo, PotencialDoRecorte Resultado)> MotorDoMunicipio(int municipio) =>
        [
            .. categoriasDoMotor.Select(categoria => (
                categoria.Key.CategoriaCodigo,
                MotorDoPotencial.Potencial(
                [
                    .. categoria.Select(r => new CulturaNoRecorte(
                        r.CulturaCodigo,
                        r.CulturaNome,
                        AreaDaCultura(municipio, r.ProdutosDaPam),
                        r.HectaresPorMaquina,
                        r.AnosDeRenovacao,
                        r.Confirmada,
                        r.GrupoCodigo))
                ])))
        ];

        var porCategoriaNoRecorte = categoriasDoMotor.ToDictionary(
            g => g.Key.CategoriaCodigo, _ => new List<PotencialDoRecorte>(), StringComparer.Ordinal);

        var totaisDosMunicipios = new List<PotencialDoRecorte>();
        var codigosNoRecorte = new List<int>();

        var itens = new List<IndicadoresDoMunicipio>();

        foreach (var codigo in area.Keys.Concat(acumuladores.Keys.Where(k => k > 0)).Distinct().Order())
        {
            var linhaDaArea = area.GetValueOrDefault(codigo);

            if (filtrado
                && (linhaDaArea is null
                    || (consulta.Regiao is not null && linhaDaArea.Regiao != consulta.Regiao)
                    || (consulta.LojaCodigo is not null && linhaDaArea.LojaCodigo != consulta.LojaCodigo)))
                continue;

            codigosNoRecorte.Add(codigo);

            var doMotor = MotorDoMunicipio(codigo);
            var noMunicipio = MotorDoPotencial.Sobrepor([.. doMotor.Select(c => c.Resultado)]);
            var parcelaDe = doMotor
                .SelectMany(c => c.Resultado.Parcelas.Select(p => (Chave: (c.Codigo, p.CulturaCodigo), Parcela: p)))
                .ToDictionary(x => x.Chave, x => x.Parcela);

            foreach (var (categoria, resultado) in doMotor)
                porCategoriaNoRecorte[categoria].Add(resultado);

            if (categoriasDoMotor.Count > 0) totaisDosMunicipios.Add(noMunicipio);

            var acumulador = acumuladores.GetValueOrDefault(codigo) ?? new Acumulador();
            itens.Add(new IndicadoresDoMunicipio(
                codigo,
                linhaDaArea?.Nome ?? nomeOficial.GetValueOrDefault(codigo, codigo.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                linhaDaArea?.PertenceAAdr ?? false,
                linhaDaArea is not null,
                (linhaDaArea?.Regiao ?? RegiaoDaAreaDeAtuacao.NaoInformada).ToString(),
                linhaDaArea?.LojaCodigo,
                linhaDaArea?.LojaNome,
                linhaDaArea?.LojaAtiva,
                acumulador.Cobertura(),
                acumulador.Vendas(),
                [
                    .. regras.Select(regra =>
                    {
                        var medidas = daRegra.GetValueOrDefault((codigo, regra.ProdutoCodigoIbge));

                        // O NÚMERO VEM DO MOTOR, e não mais de uma divisão feita aqui: é a mesma conta do
                        // total da tela e da calculadora. Sem catálogo que ligue o produto — regra de um
                        // produto fora dele —, fica a divisão da própria regra, que é o que havia antes.
                        var maquinas = catalogoDoMotor.PorProdutoDaRegra.TryGetValue(regra.ProdutoCodigoIbge, out var chave)
                                       && parcelaDe.TryGetValue(chave, out var parcela)
                            ? parcela.Parque
                            : regra.MaquinasTeoricas(medidas.AreaPlantadaHectares);

                        short? anoDela = anoDaCultura.TryGetValue(regra.ProdutoCodigoIbge, out var a) ? a : null;
                        var unidade = anoDela is { } doAno ? UnidadesDaPam.DaQuantidade(regra.ProdutoCodigoIbge, doAno) : null;
                        return new PotencialTerritorial(
                            regra.ProdutoCodigoIbge,
                            medidas.AreaPlantadaHectares,
                            maquinas is { } m ? decimal.Round(m, 1) : null,
                            medidas.AreaColhidaHectares,
                            medidas.ValorDaProducaoMilReais,
                            anoDela,
                            medidas.QuantidadeProduzida,
                            unidade?.Nome,
                            UnidadesDaPam.Produtividade(medidas.QuantidadeProduzida, medidas.AreaColhidaHectares),
                            unidade?.DaProdutividade);
                    })
                ],
                ResponsaveisDe(acumulador),
                producao.GetValueOrDefault(codigo),
                estrutura.GetValueOrDefault(codigo) ?? EstruturaVazia,
                categoriasDoMotor.Count == 0
                    ? null
                    : new PotencialEstruturalDoMunicipio(
                        noMunicipio.Parque,
                        noMunicipio.DemandaAnual,
                        noMunicipio.AreaUtilHectares,
                        noMunicipio.Estimativa,
                        noMunicipio.MotivoSemParque,
                        noMunicipio.MotivoSemDemanda)));
        }

        var foraDoMapa = GruposForaDoMapa
            .Where(g => acumuladores.ContainsKey(g.Grupo))
            .Select(g => new IndicadoresForaDoMapa(
                g.Codigo, g.Descricao, acumuladores[g.Grupo].Cobertura(), acumuladores[g.Grupo].Vendas()))
            .ToList();

        return new IndicadoresTerritoriais(
            consulta.CompetenciaInicial,
            consulta.CompetenciaFinal,
            agoraUtc,
            await contexto.ClienteCarteiras.AsNoTracking().MaxAsync(v => v.UltimaInteracaoEm, ct),
            ano,
            [
                .. regras.Select(r => new RegraDePotencialAplicada(
                    r.ProdutoCodigoIbge, r.ProdutoNome, r.HectaresPorMaquina, r.ModeloDeReferencia, r.Situacao.ToString(),
                    r.Justificativa, r.VigenteDesde, r.AnosDeRenovacao))
            ],
            itens,
            foraDoMapa,
            await contexto.Enderecos.AsNoTracking().CountAsync(e => e.ExcluidoEm == null, ct),
            await contexto.Enderecos.AsNoTracking().CountAsync(e => e.ExcluidoEm == null && e.Hectares != null && e.CulturaId != null, ct),
            consulta.Visao.ToString(),
            totaisDoEstado,
            culturasNoEstado,
            categoriasDoMotor.Count == 0
                ? null
                : MontarPotencialDoRecorte(
                    [.. categoriasDoMotor.Select(g => (g.Key.CategoriaCodigo, g.Key.CategoriaNome))],
                    porCategoriaNoRecorte,
                    totaisDosMunicipios,
                    RelevanciaDoRecorte(codigosNoRecorte, producao, totaisDoEstado),
                    RelevanciaPorCultura(codigosNoRecorte, daRegra, culturasNoEstado)));
    }

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
    private static PotencialDoRecorteNoMapa MontarPotencialDoRecorte(
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
    private static RelevanciaNoEstado? RelevanciaDoRecorte(
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
    private static List<RelevanciaDaCultura> RelevanciaPorCultura(
        IReadOnlyList<int> codigos,
        IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCultura> daRegra,
        IReadOnlyList<CulturaNoEstado> culturasNoEstado)
    {
        var lista = new List<RelevanciaDaCultura>();

        foreach (var noEstado in culturasNoEstado)
        {
            var medidas = codigos
                .Select(codigo => daRegra.TryGetValue((codigo, noEstado.ProdutoCodigoIbge), out var m) ? m : (MedidasDaCultura?)null)
                .Where(m => m is not null)
                .Select(m => m!.Value)
                .ToList();

            decimal? Somar(Func<MedidasDaCultura, decimal?> campo)
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

    /// <summary>
    /// AS REGRAS VIGENTES COMO O MOTOR AS LÊ (issue 72) — o que liga cada regra à cultura do catálogo, à
    /// categoria de máquina e ao grupo de compartilhamento.
    ///
    /// <para><b>A cultura vem da regra, e quando ela não a traz, do catálogo pelo produto.</b> A coluna
    /// <c>CulturaId</c> nasceu na issue 165 e a rota de cadastro ainda não a preenche: derivar do produto
    /// pelo de-para do catálogo é o que impede uma regra registrada hoje pelo Administrador de ficar
    /// invisível para o mapa. Quando nem isso resolve — produto fora do catálogo —, a regra vale por si,
    /// com a área do produto dela: nada é descartado em silêncio.</para>
    ///
    /// <para><b>A área da cultura é a dos produtos que ENTRAM NA SOMA</b>, e não só a do produto da regra:
    /// o café tem três linhas na classificação 782 ("Total", "Arábica" e "Canephora") e é uma cultura só.
    /// Sem vínculo marcado, fica o produto da própria regra.</para>
    ///
    /// <para><b>Uma linha por cultura e categoria.</b> Duas regras da mesma cultura na mesma categoria
    /// contariam a área dela duas vezes; vale a vigência mais recente, e o empate fica com o menor código
    /// de produto — determinístico, e não "o que o banco devolveu primeiro".</para>
    /// </summary>
    /// <param name="regras">As regras vigentes hoje, uma por produto.</param>
    /// <param name="ct">Cancelamento.</param>
    private async Task<CatalogoDoMotor> LerCatalogoDoMotorAsync(IReadOnlyList<RegraDePotencial> regras, CancellationToken ct)
    {
        if (regras.Count == 0) return new CatalogoDoMotor([], new Dictionary<int, (string, string)>());

        var culturas = await contexto.Culturas.AsNoTracking().ToDictionaryAsync(c => c.Id, ct);
        var vinculos = await contexto.ProdutosDaPamNasCulturas.AsNoTracking().ToListAsync(ct);
        var categorias = await contexto.CategoriasDeMaquina.AsNoTracking().ToDictionaryAsync(c => c.Id, ct);

        var grupos = await contexto.GruposDeCompartilhamento.AsNoTracking().Where(g => g.EstaAtivo).ToListAsync(ct);
        var noGrupo = await contexto.CulturasNosGruposDeCompartilhamento.AsNoTracking().ToListAsync(ct);

        var grupoPorId = grupos.ToDictionary(g => g.Id);

        // A MESMA CULTURA EM DOIS GRUPOS DA MESMA CATEGORIA é configuração ambígua — o índice único da
        // issue 160 é por (grupo, cultura), e não impede isso. Vale o menor código, sempre o mesmo.
        var grupoDaCultura = noGrupo
            .Where(v => grupoPorId.ContainsKey(v.GrupoDeCompartilhamentoId))
            .GroupBy(v => (v.CulturaId, grupoPorId[v.GrupoDeCompartilhamentoId].CategoriaDeMaquinaId))
            .ToDictionary(
                g => g.Key,
                g => g.Select(v => grupoPorId[v.GrupoDeCompartilhamentoId].Codigo).Order(StringComparer.Ordinal).First());

        var culturaDoProduto = vinculos.ToLookup(v => v.ProdutoCodigoIbge);
        var produtosDaCultura = vinculos.Where(v => v.EntraNaSomaDaLavoura).ToLookup(v => v.CulturaId);

        var linhas = new List<(RegraDePotencial Regra, RegraNoMotor Motor)>();

        foreach (var regra in regras)
        {
            var culturaId = regra.CulturaId
                            ?? culturaDoProduto[regra.ProdutoCodigoIbge].Select(v => (int?)v.CulturaId).FirstOrDefault();

            var cultura = culturaId is { } id ? culturas.GetValueOrDefault(id) : null;

            IReadOnlyList<int> produtos = [regra.ProdutoCodigoIbge];
            if (cultura is not null)
            {
                var daCultura = produtosDaCultura[cultura.Id].Select(v => v.ProdutoCodigoIbge).Distinct().Order().ToList();
                if (daCultura.Count > 0) produtos = daCultura;
            }

            var categoria = regra.CategoriaDeMaquinaId is { } cat ? categorias.GetValueOrDefault(cat) : null;

            linhas.Add((regra, new RegraNoMotor(
                cultura?.Codigo ?? $"PAM-{regra.ProdutoCodigoIbge}",
                cultura?.Nome ?? regra.ProdutoNome,
                categoria?.Codigo ?? SemCategoriaCodigo,
                categoria?.Nome ?? SemCategoriaNome,
                regra.HectaresPorMaquina,
                regra.AnosDeRenovacao,
                regra.Situacao == SituacaoDaRegraDePotencial.Confirmada,
                produtos,
                cultura is not null && categoria is not null
                    ? grupoDaCultura.GetValueOrDefault((cultura.Id, categoria.Id))
                    : null)));
        }

        var doMotor = linhas
            .GroupBy(l => (l.Motor.CulturaCodigo, l.Motor.CategoriaCodigo))
            .Select(g => g.OrderByDescending(l => l.Regra.VigenteDesde).ThenBy(l => l.Regra.ProdutoCodigoIbge).First().Motor)
            .OrderBy(m => m.CategoriaCodigo, StringComparer.Ordinal)
            .ThenBy(m => m.CulturaCodigo, StringComparer.Ordinal)
            .ToList();

        // O DE-PARA VALE PARA TODA REGRA, inclusive a que perdeu o desempate: a ficha do produto continua
        // mostrando o número da cultura em que ele entrou, e não um traço mudo.
        var porProduto = linhas.ToDictionary(
            l => l.Regra.ProdutoCodigoIbge,
            l => (l.Motor.CategoriaCodigo, l.Motor.CulturaCodigo));

        return new CatalogoDoMotor(doMotor, porProduto);
    }

    /// <summary>
    /// AS CULTURAS DAS REGRAS NO TOTAL DE SÃO PAULO, cada uma no ano dela — o termo de comparação da ficha do
    /// município ("produtividade aqui × em SP"). A linha é a publicada pelo IBGE, não a soma dos municípios.
    /// </summary>
    private async Task<List<CulturaNoEstado>> LerCulturasNoEstadoAsync(IReadOnlyDictionary<int, short> anoDaCultura, CancellationToken ct)
    {
        if (anoDaCultura.Count == 0) return [];

        var produtos = anoDaCultura.Keys.ToList();
        var anos = anoDaCultura.Values.Distinct().ToList();

        var linhas = await contexto.ProducoesAgricolasNosEstados.AsNoTracking()
            .Where(p => p.EstadoCodigoIbge == CodigoDeSaoPaulo && produtos.Contains(p.ProdutoCodigoIbge) && anos.Contains(p.Ano))
            .ToListAsync(ct);

        return
        [
            .. linhas
                .Where(l => anoDaCultura[l.ProdutoCodigoIbge] == l.Ano)
                .OrderBy(l => l.ProdutoCodigoIbge)
                .Select(l =>
                {
                    var unidade = UnidadesDaPam.DaQuantidade(l.ProdutoCodigoIbge, l.Ano);
                    return new CulturaNoEstado(
                        l.ProdutoCodigoIbge, l.ProdutoNome, l.Ano, l.AreaPlantadaHectares, l.AreaColhidaHectares,
                        l.QuantidadeProduzida, unidade.Nome, l.ValorDaProducaoMilReais,
                        UnidadesDaPam.Produtividade(l.QuantidadeProduzida, l.AreaColhidaHectares), unidade.DaProdutividade);
                })
        ];
    }

    /// <summary>
    /// O QUE JÁ EXISTE EM CADA MUNICÍPIO — parque, propriedades, rebanho, área e usinas (issue 65).
    ///
    /// <para><b>Cada fonte traz o ANO mais recente dela, e os anos não coincidem:</b> o Censo
    /// Agropecuário é de 2017 e a Pesquisa da Pecuária Municipal é anual. Usar um ano só para as duas
    /// esvaziaria a mais recente ou envelheceria a outra — por isso cada bloco descobre o seu.</para>
    ///
    /// <para>Tudo vem indexado pelo <b>código IBGE</b>, que é a chave que o mapa usa; município sem
    /// código não entra, porque não tem polígono para colorir.</para>
    /// </summary>
    private async Task<Dictionary<int, EstruturaDoMunicipio>> LerEstruturaAsync(CancellationToken ct)
    {
        var anoDoCenso = await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking().MaxAsync(f => (short?)f.Ano, ct);
        var anoDoRebanho = await contexto.RebanhosNosMunicipios.AsNoTracking().MaxAsync(r => (short?)r.Ano, ct);
        var anoDaArea = await contexto.AreasTerritoriaisDosMunicipios.AsNoTracking().MaxAsync(a => (short?)a.Ano, ct);

        var frota = anoDoCenso is null
            ? []
            : await (from linha in contexto.FrotasDeTratoresNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDoCenso && municipio.CodigoIbge != null
                     select new
                     {
                         Codigo = municipio.CodigoIbge!.Value,
                         linha.PotenciaCodigoIbge,
                         linha.Tratores,
                         linha.EstabelecimentosComTrator
                     }).ToListAsync(ct);

        var faixas = anoDoCenso is null
            ? []
            : await (from linha in contexto.EstabelecimentosPorAreaNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDoCenso && municipio.CodigoIbge != null
                     select new { Codigo = municipio.CodigoIbge!.Value, linha.GrupoDeAreaCodigoIbge, linha.Estabelecimentos })
                .ToListAsync(ct);

        var rebanho = anoDoRebanho is null
            ? []
            : await (from linha in contexto.RebanhosNosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDoRebanho && linha.RebanhoCodigoIbge == Bovino && municipio.CodigoIbge != null
                     select new { Codigo = municipio.CodigoIbge!.Value, linha.Cabecas }).ToListAsync(ct);

        var areas = anoDaArea is null
            ? []
            : await (from linha in contexto.AreasTerritoriaisDosMunicipios.AsNoTracking()
                     join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                     where linha.Ano == anoDaArea && municipio.CodigoIbge != null
                     select new { Codigo = municipio.CodigoIbge!.Value, linha.AreaKm2 }).ToListAsync(ct);

        // SÓ AS VIGENTES: a usina que saiu da lista da ANP fica encerrada no banco (issue 153), com a data, mas não é
        // mais usina autorizada — e não entra no mapa nem na contagem.
        var usinas = await (from usina in contexto.UsinasDeEtanol.AsNoTracking()
                            join municipio in contexto.Municipios.AsNoTracking() on usina.MunicipioId equals municipio.Id
                            where municipio.CodigoIbge != null && usina.EncerradaEm == null
                            orderby usina.RazaoSocial
                            select new
                            {
                                Codigo = municipio.CodigoIbge!.Value,
                                usina.RazaoSocial,
                                usina.CapacidadeDeAnidroM3Dia,
                                usina.CapacidadeDeHidratadoM3Dia
                            }).ToListAsync(ct);

        var porCodigo = frota.Select(f => f.Codigo)
            .Concat(faixas.Select(f => f.Codigo))
            .Concat(rebanho.Select(r => r.Codigo))
            .Concat(areas.Select(a => a.Codigo))
            .Concat(usinas.Select(u => u.Codigo))
            .Distinct();

        var frotaPorCodigo = frota.ToLookup(f => f.Codigo);
        var faixasPorCodigo = faixas.ToLookup(f => f.Codigo);
        var rebanhoPorCodigo = rebanho.ToDictionary(r => r.Codigo, r => r.Cabecas);
        var areaPorCodigo = areas.ToDictionary(a => a.Codigo, a => a.AreaKm2);
        var usinasPorCodigo = usinas.ToLookup(u => u.Codigo);

        var resultado = new Dictionary<int, EstruturaDoMunicipio>();

        foreach (var codigo in porCodigo)
        {
            var linhasDaFrota = frotaPorCodigo[codigo].ToDictionary(f => f.PotenciaCodigoIbge);
            var porGrupo = faixasPorCodigo[codigo].ToDictionary(f => f.GrupoDeAreaCodigoIbge, f => f.Estabelecimentos);

            resultado[codigo] = new EstruturaDoMunicipio(
                anoDoCenso,
                linhasDaFrota.GetValueOrDefault(PotenciaTotal)?.Tratores,
                linhasDaFrota.GetValueOrDefault(PotenciaAbaixoDe100Cv)?.Tratores,
                linhasDaFrota.GetValueOrDefault(PotenciaDe100CvEMais)?.Tratores,
                linhasDaFrota.GetValueOrDefault(PotenciaTotal)?.EstabelecimentosComTrator,
                porGrupo.GetValueOrDefault(GrupoDeAreaTotal),
                Reagrupar(porGrupo),
                anoDoRebanho,
                rebanhoPorCodigo.GetValueOrDefault(codigo),
                areaPorCodigo.GetValueOrDefault(codigo),
                [
                    .. usinasPorCodigo[codigo].Select(u => new UsinaDoMunicipio(
                        u.RazaoSocial,
                        u.CapacidadeDeAnidroM3Dia is null && u.CapacidadeDeHidratadoM3Dia is null
                            ? null
                            : (u.CapacidadeDeAnidroM3Dia ?? 0) + (u.CapacidadeDeHidratadoM3Dia ?? 0)))
                ]);
        }

        return resultado;
    }

    /// <summary>
    /// As 18 faixas do IBGE nas que o comercial usa.
    ///
    /// <para><b>A conta é feita aqui, e não na carga</b>, porque o reagrupamento é uma escolha de
    /// leitura: o banco guarda as faixas originais, e mudar este mapa não pede recarga nenhuma.</para>
    ///
    /// <para><b>Nulo quando TODAS as faixas de origem vieram sob sigilo</b> — e não zero. Somar
    /// sigiloso como zero diria que não há propriedade daquele tamanho ali.</para>
    /// </summary>
    private static List<FaixaDeArea> Reagrupar(IReadOnlyDictionary<int, int?> porGrupo)
    {
        var faixas = new List<FaixaDeArea>(FaixasDoComercial.Length);

        for (var i = 0; i < FaixasDoComercial.Length; i++)
        {
            var (rotulo, grupos) = FaixasDoComercial[i];
            var presentes = grupos.Select(g => porGrupo.GetValueOrDefault(g)).Where(v => v is not null).ToList();

            faixas.Add(new FaixaDeArea(
                i + 1, rotulo, presentes.Count == 0 ? null : presentes.Sum(v => v!.Value)));
        }

        return faixas;
    }

    /// <summary>
    /// Os totais de São Paulo — o denominador de "que fatia da cultura do estado está na região".
    ///
    /// <para><b>Todos vêm do TOTAL PUBLICADO pelo IBGE</b> (issue 155): a lavoura de
    /// <c>ProducaoAgricolaNoEstado</c>, e o parque, as propriedades e o rebanho de
    /// <c>MedidaDoIbgeNoEstado</c>. O publicado não é a soma dos municípios — o valor municipal
    /// sigiloso entra nele sem aparecer embaixo —, e até a issue 155 três dos quatro eram somados,
    /// o que inflava a fatia da região justamente onde há sigilo.</para>
    ///
    /// <para><b>A soma dos municípios vai junto</b>, para a tela poder dizer quanto o sigilo esconde
    /// em vez de deixar uma diferença sem explicação para quem conferir na mão.</para>
    /// </summary>
    private async Task<TotaisDoEstado?> LerTotaisDoEstadoAsync(short? ano, CancellationToken ct)
    {
        if (ano is null) return null;

        var lavoura = await contexto.ProducoesAgricolasNosEstados.AsNoTracking()
            .Where(p => p.Ano == ano
                        && p.EstadoCodigoIbge == CodigoDeSaoPaulo
                        && !ProdutosQueDuplicamNaSoma.Contains(p.ProdutoCodigoIbge))
            .GroupBy(p => p.EstadoCodigoIbge)
            .Select(g => new
            {
                Plantada = g.Sum(p => p.AreaPlantadaHectares),
                Colhida = g.Sum(p => p.AreaColhidaHectares),
                Valor = g.Sum(p => p.ValorDaProducaoMilReais)
            })
            .FirstOrDefaultAsync(ct);

        if (lavoura is null) return null;

        var anoDoCenso = await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking().MaxAsync(f => (short?)f.Ano, ct);
        var anoDoRebanho = await contexto.RebanhosNosMunicipios.AsNoTracking().MaxAsync(r => (short?)r.Ano, ct);

        var tratores = new MedidaDoEstado(
            await PublicadoAsync(TabelaDaFrotaDeTratores, VariavelDeTratores, PotenciaTotal, anoDoCenso, ct),
            anoDoCenso is null
                ? null
                : await contexto.FrotasDeTratoresNosMunicipios.AsNoTracking()
                    .Where(f => f.Ano == anoDoCenso && f.PotenciaCodigoIbge == PotenciaTotal)
                    .SumAsync(f => (int?)f.Tratores, ct));

        var estabelecimentos = new MedidaDoEstado(
            await PublicadoAsync(TabelaDeEstabelecimentos, VariavelDeEstabelecimentos, GrupoDeAreaTotal, anoDoCenso, ct),
            anoDoCenso is null
                ? null
                : await contexto.EstabelecimentosPorAreaNosMunicipios.AsNoTracking()
                    .Where(e => e.Ano == anoDoCenso && e.GrupoDeAreaCodigoIbge == GrupoDeAreaTotal)
                    .SumAsync(e => (int?)e.Estabelecimentos, ct));

        var rebanho = new MedidaDoEstado(
            await PublicadoAsync(TabelaDoRebanho, VariavelDoEfetivoDoRebanho, Bovino, anoDoRebanho, ct),
            anoDoRebanho is null
                ? null
                : await contexto.RebanhosNosMunicipios.AsNoTracking()
                    .Where(r => r.Ano == anoDoRebanho && r.RebanhoCodigoIbge == Bovino)
                    .SumAsync(r => (int?)r.Cabecas, ct));

        return new TotaisDoEstado(
            ano.Value, lavoura.Plantada, lavoura.Valor, tratores, estabelecimentos, anoDoCenso, rebanho, anoDoRebanho,
            lavoura.Colhida);
    }

    /// <summary>
    /// O valor que o IBGE publicou para São Paulo numa célula do SIDRA.
    ///
    /// <para>O ano pedido é o que as tabelas municipais têm carregado: o total publicado de um ano que
    /// não está no mapa não é denominador de nada, e compará-lo com a soma de outro ano diria uma
    /// diferença que não existe.</para>
    /// </summary>
    private async Task<int?> PublicadoAsync(short tabela, short variavel, int categoria, short? ano, CancellationToken ct)
    {
        if (ano is null) return null;

        var valor = await contexto.MedidasDoIbgeNosEstados.AsNoTracking()
            .Where(m => m.EstadoCodigoIbge == CodigoDeSaoPaulo
                        && m.TabelaDoSidra == tabela
                        && m.VariavelDoSidra == variavel
                        && m.CategoriaCodigoIbge == categoria
                        && m.Ano == ano)
            .Select(m => m.Valor)
            .FirstOrDefaultAsync(ct);

        return valor is null ? null : (int)decimal.Round(valor.Value);
    }

    /// <summary>O grupo fora do mapa de cada natureza de contraparte sem cliente no CRM.</summary>
    private static int GrupoDaNatureza(NaturezaDoParceiro natureza) => natureza switch
    {
        NaturezaDoParceiro.Fabrica => NotaParaFabrica,
        NaturezaDoParceiro.EmpresaDoGrupo => NotaParaEmpresaDoGrupo,
        NaturezaDoParceiro.OutraRevenda => NotaParaOutraRevenda,
        _ => NotaSemCadastroDeCliente
    };

    /// <summary>O contador de um grupo enquanto clientes, vínculos e notas são percorridos.</summary>
    private sealed class Acumulador
    {
        public int Clientes;
        public int Vinculos;
        public int Cobertos;
        public int ForaDaCadencia;
        public int NuncaContatados;
        public int SemCadencia;
        public int ClientesQueCompraram;
        public decimal ValorLiquido;
        public decimal Maquina;
        public decimal Peca;
        public decimal Servico;
        public decimal Outros;
        public readonly Dictionary<long, int> VinculosPorResponsavel = [];
        public readonly Dictionary<long, HashSet<long>> CarteirasPorResponsavel = [];

        public CoberturaTerritorial Cobertura() => new(
            Clientes, Vinculos, Cobertos + ForaDaCadencia + NuncaContatados, Cobertos, ForaDaCadencia, NuncaContatados, SemCadencia);

        public VendasTerritoriais Vendas() => new(
            ClientesQueCompraram, decimal.Round(ValorLiquido, 2), decimal.Round(Maquina, 2),
            decimal.Round(Peca, 2), decimal.Round(Servico, 2), decimal.Round(Outros, 2));
    }
}
