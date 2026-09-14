using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
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

        var responsaveis = (await (
                from afirmacao in contexto.ResponsaveisPelosMunicipios.AsNoTracking().Where(r => r.EncerradoEm == null)
                join municipio in contexto.Municipios.AsNoTracking() on afirmacao.MunicipioId equals municipio.Id
                join usuario in contexto.Usuarios.AsNoTracking() on afirmacao.UsuarioId equals (long?)usuario.Id into usuarios
                from usuario in usuarios.DefaultIfEmpty()
                where municipio.CodigoIbge != null
                select new
                {
                    Codigo = municipio.CodigoIbge!.Value,
                    afirmacao.Papel,
                    afirmacao.Fonte,
                    afirmacao.NomeNaOrigem,
                    afirmacao.Situacao,
                    afirmacao.UsuarioId,
                    UsuarioNome = usuario == null ? null : usuario.NomeExibicao,
                    afirmacao.ImportadoEm
                })
            .ToListAsync(ct))
            .ToLookup(r => r.Codigo);

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
        var cadencias = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Natureza == NaturezaDaCarteira.Comercial)
            .Select(c => new
            {
                c.Id,
                Cadencia = contexto.LinhasDeNegocio
                    .Where(l => l.Id == c.LinhaDeNegocioId)
                    .Select(l => new { l.DiasCicloClasseA, l.DiasCicloClasseB, l.DiasCicloClasseC, l.DiasCicloClasseD })
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(c => c.Id, c => c.Cadencia, ct);

        var idsDeCarteira = cadencias.Keys.ToList();

        var vinculos = await contexto.ClienteCarteiras.AsNoTracking()
            .Where(v => v.DesvinculadoEm == null && idsDeCarteira.Contains(v.CarteiraId))
            .Select(v => new { v.CarteiraId, v.ClienteId, v.UltimaInteracaoEm })
            .ToListAsync(ct);

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
        // Potencial: a área plantada do último ano, para os produtos das regras ativas.
        // -----------------------------------------------------------------------------------------
        var regras = await contexto.RegrasDePotencial.AsNoTracking()
            .Where(r => r.EstaAtiva)
            .OrderBy(r => r.Id)
            .ToListAsync(ct);

        var produtos = regras.Select(r => r.ProdutoCodigoIbge).Distinct().ToList();
        var ano = await contexto.AreasPlantadasNosMunicipios.AsNoTracking().MaxAsync(a => (short?)a.Ano, ct);
        var areaPlantada = new Dictionary<(int Codigo, int Produto), decimal?>();

        if (ano is not null && produtos.Count > 0)
        {
            var linhas = await (
                    from linha in contexto.AreasPlantadasNosMunicipios.AsNoTracking()
                    join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                    where linha.Ano == ano && produtos.Contains(linha.ProdutoCodigoIbge) && municipio.CodigoIbge != null
                    select new { Codigo = municipio.CodigoIbge!.Value, linha.ProdutoCodigoIbge, linha.AreaPlantadaHectares })
                .ToListAsync(ct);

            foreach (var linha in linhas)
                areaPlantada[(linha.Codigo, linha.ProdutoCodigoIbge)] = linha.AreaPlantadaHectares;
        }

        // -----------------------------------------------------------------------------------------
        // A montagem: os municípios de SP da área de atuação ou com dado, depois dos filtros.
        // -----------------------------------------------------------------------------------------
        var filtrado = consulta.Regiao is not null || consulta.LojaCodigo is not null;

        var nomeOficial = municipios.Values
            .Where(m => m.CodigoIbge is not null && m.Uf == SaoPaulo)
            .GroupBy(m => m.CodigoIbge!.Value)
            .ToDictionary(g => g.Key, g => g.First().Nome);

        var itens = new List<IndicadoresDoMunicipio>();

        foreach (var codigo in area.Keys.Concat(acumuladores.Keys.Where(k => k > 0)).Distinct().Order())
        {
            var linhaDaArea = area.GetValueOrDefault(codigo);

            if (filtrado
                && (linhaDaArea is null
                    || (consulta.Regiao is not null && linhaDaArea.Regiao != consulta.Regiao)
                    || (consulta.LojaCodigo is not null && linhaDaArea.LojaCodigo != consulta.LojaCodigo)))
                continue;

            var acumulador = acumuladores.GetValueOrDefault(codigo) ?? new Acumulador();
            var afirmacoes = responsaveis[codigo].ToList();
            var cens = afirmacoes.Where(a => a.Papel == PapelNoMunicipio.Cen).ToList();

            // AS DUAS FONTES FICAM, E A COMPARAÇÃO É SÓ UM RÓTULO. "Provável mesma pessoa" não funde
            // nada: diz que os nomes diferem na grafia, e quem decide é o comercial (documento 32,
            // seção 4.3). Divergente é qualquer diferença — de grafia ou de pessoa.
            var comparacao = cens.Select(c => c.Fonte).Distinct().Count() == 2
                ? ResponsavelPeloMunicipio.CompararCen(cens[0].NomeNaOrigem, cens[0].UsuarioId, cens[1].NomeNaOrigem, cens[1].UsuarioId)
                : ComparacaoDoCen.UmaFonteSo;
            var divergente = comparacao is ComparacaoDoCen.ProvavelMesmaPessoa or ComparacaoDoCen.NomesDiferentes;

            itens.Add(new IndicadoresDoMunicipio(
                codigo,
                linhaDaArea?.Nome ?? nomeOficial.GetValueOrDefault(codigo, codigo.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                linhaDaArea?.PertenceAAdr ?? false,
                linhaDaArea is not null,
                (linhaDaArea?.Regiao ?? RegiaoDaAreaDeAtuacao.NaoInformada).ToString(),
                linhaDaArea?.LojaCodigo,
                linhaDaArea?.LojaNome,
                linhaDaArea?.LojaAtiva,
                [
                    .. afirmacoes
                        .OrderBy(a => a.Papel).ThenBy(a => a.Fonte)
                        .Select(a => new ResponsavelDeclarado(
                            a.Papel.ToString(), a.Fonte.ToString(), a.NomeNaOrigem, a.Situacao.ToString(), a.UsuarioNome, a.ImportadoEm))
                ],
                divergente,
                comparacao.ToString(),
                acumulador.Cobertura(),
                acumulador.Vendas(),
                [
                    .. regras.Select(regra =>
                    {
                        var hectares = areaPlantada.TryGetValue((codigo, regra.ProdutoCodigoIbge), out var valor) ? valor : null;
                        var maquinas = regra.MaquinasTeoricas(hectares);
                        return new PotencialTerritorial(
                            regra.ProdutoCodigoIbge, hectares, maquinas is { } m ? decimal.Round(m, 1) : null);
                    })
                ]));
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
                    r.ProdutoCodigoIbge, r.ProdutoNome, r.HectaresPorMaquina, r.ModeloDeReferencia, r.Situacao.ToString(), r.Origem))
            ],
            itens,
            foraDoMapa,
            await contexto.Enderecos.AsNoTracking().CountAsync(e => e.ExcluidoEm == null, ct),
            await contexto.Enderecos.AsNoTracking().CountAsync(e => e.ExcluidoEm == null && e.Hectares != null && e.CulturaId != null, ct),
            consulta.Visao.ToString());
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

        public CoberturaTerritorial Cobertura() => new(
            Clientes, Vinculos, Cobertos + ForaDaCadencia + NuncaContatados, Cobertos, ForaDaCadencia, NuncaContatados, SemCadencia);

        public VendasTerritoriais Vendas() => new(
            ClientesQueCompraram, decimal.Round(ValorLiquido, 2), decimal.Round(Maquina, 2),
            decimal.Round(Peca, 2), decimal.Round(Servico, 2), decimal.Round(Outros, 2));
    }
}
