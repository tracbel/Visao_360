using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CodigosDoIbge;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CriterioDasVendasDoArt;

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
/// <param name="contexto">O contexto do banco.</param>
/// <param name="territorioDeReferencia">A área de atuação, os municípios e as lojas — dado de referência (documento 54).</param>
/// <param name="potencialDeReferencia">
/// As regras, o catálogo do motor (issue 161) e a PAM — dado de referência (documento 54).
/// </param>
/// <param name="estruturaDeReferencia">
/// O parque, as propriedades, o rebanho, as usinas e os totais do estado e da região — dado de referência (plano 2).
/// </param>
public sealed class RepositorioDeIndicadoresTerritoriais(
    CrmDbContext contexto,
    IRepositorioDoTerritorioDeReferencia territorioDeReferencia,
    IRepositorioDoPotencialDeReferencia potencialDeReferencia,
    IRepositorioDaEstruturaDeReferencia estruturaDeReferencia)
    : IRepositorioIndicadoresTerritoriais
{
    /// <summary>Sem cache — o teste de contêiner e quem monta o repositório à mão leem a referência direto do banco.</summary>
    /// <param name="contexto">O contexto do banco.</param>
    public RepositorioDeIndicadoresTerritoriais(CrmDbContext contexto)
        : this(
            contexto,
            new RepositorioDoTerritorioDeReferencia(contexto),
            new RepositorioDoPotencialDeReferencia(contexto, new RepositorioDoMotorDoPotencial(contexto)),
            new RepositorioDaEstruturaDeReferencia(contexto)) { }

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

    /// <summary>O município que não tem nenhuma das cinco fontes carregadas.</summary>
    private static readonly EstruturaDoMunicipio EstruturaVazia =
        new(null, null, null, null, null, null, [], null, null, null, []);

    // O CATÁLOGO DO MOTOR mora em IRepositorioDoMotorDoPotencial desde a issue 161: a calculadora
    // precisa exatamente do mesmo, e duas leituras do mesmo conceito divergem no dia em que uma mudar. A PAM, as regras
    // e a conta de cada município moram no potencial de referência desde o documento 54.

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

        // O TERRITÓRIO É DADO DE REFERÊNCIA (documento 54): igual para todo mundo, guardado pela versão do assunto.
        var territorio = await territorioDeReferencia.LerAsync(ct);
        var area = territorio.Area;
        var municipios = territorio.MunicipiosPorId;

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

        // O MESMO TRECHO DO ANO ANTERIOR (decisão de 27/09/2026) acumula À PARTE, num dicionário próprio: ele
        // não pode criar grupo no mapa nem fora dele — um grupo que só vendeu no ano passado apareceria hoje
        // com R$ 0, e a conferência "mapa + fora do mapa = total" ganharia uma linha que não é deste período.
        var janela = consulta.Janela;
        var janelaAnterior = consulta.JanelaAnterior;
        var anteriores = new Dictionary<int, Acumulador>();

        Acumulador DoAnterior(int grupo) =>
            anteriores.TryGetValue(grupo, out var existente) ? existente : anteriores[grupo] = new Acumulador();

        // O MÊS A MÊS DE CADA MUNICÍPIO, nas duas janelas — é dele que sai o mini-gráfico dos cartões.
        var mensal = new Dictionary<(int Grupo, DateOnly Mes), VendasDoMes>();

        VendasDoMes NoMes(int grupo, DateOnly mes) =>
            mensal.TryGetValue((grupo, mes), out var existente) ? existente : mensal[(grupo, mes)] = new VendasDoMes();

        // O FILTRO "CEN / GESTOR": os clientes das carteiras comerciais do responsável escolhido. Quem não
        // está numa delas sai do recorte — do mapa e de fora dele —, como no filtro da filial do cliente.
        HashSet<long>? clientesDoResponsavel = null;
        if (consulta.ResponsavelId is { } responsavelEscolhido)
            clientesDoResponsavel = (await (
                    from vinculo in contexto.ClienteCarteiras.AsNoTracking().Where(v => v.DesvinculadoEm == null)
                    join carteira in contexto.Carteiras.AsNoTracking() on vinculo.CarteiraId equals carteira.Id
                    where carteira.ExcluidoEm == null
                          && carteira.Natureza == NaturezaDaCarteira.Comercial
                          && carteira.ResponsavelId == responsavelEscolhido
                    select vinculo.ClienteId)
                .ToListAsync(ct))
                .ToHashSet();

        foreach (var cliente in clientes)
        {
            // MAIS DE UM ENDEREÇO PRINCIPAL: vale o de menor Id, e a ordenação da consulta é o que
            // torna a escolha a mesma em toda execução.
            if (grupoDoCliente.ContainsKey(cliente.Id)) continue;

            classeDoCliente[cliente.Id] = cliente.Classe;

            if ((consulta.FilialDoClienteId is { } filialDoCliente && cliente.EmpresaId != filialDoCliente)
                || (clientesDoResponsavel is not null && !clientesDoResponsavel.Contains(cliente.Id)))
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
            var doGrupo = Do(grupo);
            doGrupo.Clientes++;
            doGrupo.ContarClasse(cliente.Classe);
        }

        // CADA LINHA CAI EM EXATAMENTE UM GRUPO, ou em nenhum quando o filtro a exclui. É o que
        // impede dupla contagem: não existe caminho que some a mesma venda em dois lugares.
        int GrupoDe(long clienteId)
        {
            if (grupoDoCliente.TryGetValue(clienteId, out var grupo)) return grupo;

            if (clientesDoResponsavel is not null && !clientesDoResponsavel.Contains(clienteId)) return ForaDoFiltro;

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

            // COM O FILTRO DO CEN, a cobertura é a das carteiras DELE: o mesmo cliente numa carteira de outro
            // responsável tem outro dono e outra cadência a cumprir.
            if (consulta.ResponsavelId is { } doCen && responsavelDaCarteira[vinculo.CarteiraId] != doCen) continue;

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
            acumulador.ClientesComVinculo.Add(vinculo.ClienteId);

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
        // AS DUAS JANELAS NUMA LEITURA SÓ: a pedida e o mesmo trecho do ano anterior. Os meses entre as duas —
        // setembro e outubro, no ano fiscal até agosto — não são lidos.
        var faturamento = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null
                        && ((f.Competencia >= consulta.CompetenciaInicial && f.Competencia <= consulta.CompetenciaFinal)
                            || (f.Competencia >= janelaAnterior.Inicial && f.Competencia <= janelaAnterior.Final))
                        && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .Select(f => new { f.ClienteId, f.Competencia, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros })
            .ToListAsync(ct);

        foreach (var cliente in faturamento.GroupBy(f => f.ClienteId))
        {
            var grupoDaVenda = GrupoDe(cliente.Key);
            if (grupoDaVenda == ForaDoFiltro) continue;

            // O GRUPO SÓ NASCE COM VENDA DO PERÍODO: um cliente que só comprou no ano anterior não pode abrir,
            // hoje, uma linha de R$ 0 no mapa ou fora dele.
            var doPeriodo = cliente.Where(f => janela.Contem(f.Competencia)).ToList();
            if (doPeriodo.Count > 0)
            {
                var acumulador = Do(grupoDaVenda);
                var liquido = doPeriodo.Sum(f => f.ValorLiquido);

                acumulador.ValorLiquido += liquido;
                acumulador.Maquina += doPeriodo.Sum(f => f.ValorEmMaquina);
                acumulador.Peca += doPeriodo.Sum(f => f.ValorEmPeca);
                acumulador.Servico += doPeriodo.Sum(f => f.ValorEmServico);
                acumulador.Outros += doPeriodo.Sum(f => f.ValorEmOutros);
                if (liquido != 0) acumulador.ClientesQueCompraram++;
            }

            var doAnterior = cliente.Where(f => janelaAnterior.Contem(f.Competencia)).ToList();
            if (doAnterior.Count > 0)
            {
                var acumulador = DoAnterior(grupoDaVenda);
                var liquido = doAnterior.Sum(f => f.ValorLiquido);

                acumulador.ValorLiquido += liquido;
                acumulador.Maquina += doAnterior.Sum(f => f.ValorEmMaquina);
                acumulador.Peca += doAnterior.Sum(f => f.ValorEmPeca);
                acumulador.Servico += doAnterior.Sum(f => f.ValorEmServico);
                acumulador.Outros += doAnterior.Sum(f => f.ValorEmOutros);
                if (liquido != 0) acumulador.ClientesQueCompraram++;
            }

            // O MÊS A MÊS SÓ DOS MUNICÍPIOS — é o que os cartões da ADR desenham.
            if (grupoDaVenda > 0)
                foreach (var linha in cliente)
                {
                    var mes = NoMes(grupoDaVenda, linha.Competencia);
                    mes.ValorLiquido += linha.ValorLiquido;
                    mes.Maquina += linha.ValorEmMaquina;
                    mes.PosVenda += linha.ValorEmPeca + linha.ValorEmServico;
                }
        }

        // DESDE QUANDO HÁ FATURAMENTO AO ALCANCE: é o que diz se o ano anterior pode ser comparado. Com e sem
        // cliente, porque a carga grava os dois juntos.
        var primeiraComCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .MinAsync(f => (DateOnly?)f.Competencia, ct);
        var primeiraSemCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .MinAsync(f => (DateOnly?)f.Competencia, ct);
        DateOnly? primeiraCompetenciaDoFaturamento =
            primeiraComCliente is null ? primeiraSemCliente
            : primeiraSemCliente is null ? primeiraComCliente
            : primeiraComCliente < primeiraSemCliente ? primeiraComCliente : primeiraSemCliente;

        // A NOTA SEM CLIENTE NO CRM TAMBÉM É NOTA DESTA FILIAL. Sem ela, o total da tela seria só o
        // faturamento com cliente — R$ 320,6 mi a menos em 12 meses, medido em 13/09/2026 — e a
        // conferência "mapa + fora do mapa = o que a filial faturou" nunca fecharia. Ela não tem
        // cadastro, logo não tem filial de cadastro: o filtro pela filial do cliente a exclui — e o do CEN
        // também, porque sem cadastro ela não está em carteira nenhuma.
        if (consulta.FilialDoClienteId is null && consulta.ResponsavelId is null)
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
        // AS VENDAS DE MÁQUINA EM UNIDADES — o ART (issue 69; D-P08 decidida em 24/09/2026).
        //
        // ELAS NÃO SÃO O FATURAMENTO ACIMA. Aquele é o Protheus, em REAIS; este é o ART, em MÁQUINAS.
        // A captura é uma razão de unidades sobre demanda estimada em unidades, e reais no numerador a
        // tornariam incomparável — os dois convivem, e nada aqui os soma.
        //
        // UMA VENDA É UMA MÁQUINA: cada linha do ART tem um chassi e vira uma venda. Somar a coluna de
        // quantidade da origem contaria duas vezes a máquina que ela lançasse em lote.
        //
        // AUSÊNCIA DE CARGA NÃO É AUSÊNCIA DE VENDA: com a tabela vazia ao alcance desta consulta, o
        // bloco inteiro sai NULO, e a captura sai vazia com o motivo. Zero é medida — a filial existe,
        // o ART trouxe dado e ela não vendeu máquina no período.
        // -----------------------------------------------------------------------------------------
        var vendasDeMaquina = contexto.VendasDeMaquina.AsNoTracking()
            .Where(v => v.ExcluidoEm == null
                        && (consulta.FilialDaVendaId == null || v.EmpresaId == consulta.FilialDaVendaId));

        var oArtTrouxeVenda = await contexto.VendasDeMaquina.AsNoTracking().AnyAsync(v => v.ExcluidoEm == null, ct);

        var categoriaDaLinha = new Dictionary<string, (string Codigo, string Nome, short Ordem)>(StringComparer.Ordinal);
        int vendasSemAData = 0;
        DateOnly? vendaMaisRecente = null;
        DateOnly? primeiroMesDoArt = null;
        DateTime? carregadoAte = null;

        if (oArtTrouxeVenda)
        {
            // O DE-PARA É CRUZADO EM MEMÓRIA, DE PROPÓSITO. `CodigoDaLinha` é texto ASCII com colação
            // binária e `LinhaDeProduto.Codigo` é Unicode com a colação do banco: um JOIN entre os dois
            // no SQL Server dá conflito de colação. As duas tabelas têm dezenas de linhas.
            var codigoDaLinha = await contexto.LinhasDeProduto.AsNoTracking()
                .ToDictionaryAsync(l => l.Id, l => l.Codigo, ct);

            foreach (var ligacao in await (
                         from ligacao in contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
                         join categoria in contexto.CategoriasDeMaquina.AsNoTracking()
                             on ligacao.CategoriaDeMaquinaId equals categoria.Id
                         select new { ligacao.CodigoDaLinha, categoria.Codigo, categoria.Nome, categoria.Ordem })
                     .ToListAsync(ct))
                categoriaDaLinha[ligacao.CodigoDaLinha] = (ligacao.Codigo, ligacao.Nome, ligacao.Ordem);

            var datas = DataDoCriterio(vendasDeMaquina, CriterioDasVendasDoArt.Criterio);
            vendasSemAData = await datas.CountAsync(d => d == null, ct);
            vendaMaisRecente = await datas.MaxAsync(ct);
            carregadoAte = await vendasDeMaquina.MaxAsync(v => (DateTime?)v.ImportadaEm, ct);

            // DESDE QUANDO O ART TRAZ VENDA, pelo mês da data do critério — é o que diz se o ano anterior
            // pode ser comparado em unidades. O ART começou depois do Protheus, e cada fonte tem a sua.
            if (await datas.MinAsync(ct) is { } primeiraVenda)
                primeiroMesDoArt = new DateOnly(primeiraVenda.Year, primeiraVenda.Month, 1);

            // AS DUAS JANELAS NUMA LEITURA SÓ, e a data de cada venda vem junto: a janela de cada uma — a
            // pedida ou a do ano anterior — é decidida aqui, pela data do critério.
            var vendas = await (
                    from venda in NoPeriodo(
                        vendasDeMaquina, CriterioDasVendasDoArt.Criterio, janelaAnterior.Inicial, consulta.CompetenciaFinal.AddMonths(1))
                    // A MÁQUINA ENTRA POR FORA (junção à esquerda): ela tem filial própria, e a de uma
                    // máquina reaproveitada de outra filial pode estar fora do alcance de quem lê. Numa
                    // junção comum a venda sumiria inteira; assim ela conta, e só a categoria fica sem saber.
                    join maquina in contexto.Equipamentos.AsNoTracking().Where(e => e.ExcluidoEm == null)
                        on venda.EquipamentoId equals maquina.Id into maquinas
                    from maquina in maquinas.DefaultIfEmpty()
                    select new
                    {
                        venda.CompradorId,
                        LinhaDeProdutoId = maquina == null ? null : maquina.LinhaDeProdutoId,
                        venda.VendidaEm,
                        venda.FaturadaEm,
                        venda.EntregueEm
                    })
                .ToListAsync(ct);

            foreach (var venda in vendas)
            {
                var data = DataPeloCriterio(CriterioDasVendasDoArt.Criterio, venda.VendidaEm, venda.FaturadaEm, venda.EntregueEm);
                if (data is not { } dia) continue;

                var doPeriodo = janela.Contem(dia);
                if (!doPeriodo && !janelaAnterior.Contem(dia)) continue;

                var grupoDaMaquina = GrupoDe(venda.CompradorId);
                if (grupoDaMaquina == ForaDoFiltro) continue;

                // A CATEGORIA DA MÁQUINA, pelo de-para da linha de produto — ou por que ela não tem.
                string? codigoDaCategoria = null;
                var linhaDaMaquina = venda.LinhaDeProdutoId is { } linhaId && codigoDaLinha.TryGetValue(linhaId, out var achada)
                    ? achada
                    : null;
                var semClassificacao = linhaDaMaquina is null;
                var emLinhaSemCategoria = false;
                if (linhaDaMaquina is not null)
                {
                    // LINHA SEM CATEGORIA CONTA NO TOTAL E SOME DA QUEBRA. Hoje é a plataforma de corte, que
                    // ficou sem categoria de propósito: é acessório de colheitadeira, julgamento do comercial,
                    // não omissão (documento 48, §5.3). A colhedora de cana ganhou categoria própria em 27/09/2026.
                    if (categoriaDaLinha.TryGetValue(linhaDaMaquina, out var categoria))
                        codigoDaCategoria = categoria.Codigo;
                    else
                        emLinhaSemCategoria = true;
                }

                // O FILTRO "TIPO DE PRODUTO" (a categoria): só a máquina que se SABE ser da categoria entra. A de
                // linha sem categoria ou sem classificação fica de fora — não se sabe de que tipo ela é.
                if (consulta.CategoriaDeMaquina is { } filtroDeCategoria
                    && !string.Equals(codigoDaCategoria, filtroDeCategoria, StringComparison.Ordinal))
                    continue;

                // O GRUPO SÓ NASCE COM VENDA DO PERÍODO, como no faturamento: o ano anterior acumula à parte.
                //
                // DOIS "SE" INDEPENDENTES, e não um ou-outro (revisão de 27/09/2026): numa janela de mais de doze
                // meses, os meses do meio são das DUAS janelas — o fim da anterior é o começo da pedida —, e a
                // venda deles conta nas duas, como já contava no faturamento em reais.
                void Contar(Acumulador acumulador)
                {
                    acumulador.MaquinasVendidas++;
                    if (semClassificacao) acumulador.MaquinasSemClassificacao++;
                    else if (emLinhaSemCategoria) acumulador.MaquinasEmLinhaSemCategoria++;
                    else
                        acumulador.MaquinasPorCategoria[codigoDaCategoria!] =
                            acumulador.MaquinasPorCategoria.GetValueOrDefault(codigoDaCategoria!) + 1;
                }

                if (doPeriodo) Contar(Do(grupoDaMaquina));
                if (janelaAnterior.Contem(dia)) Contar(DoAnterior(grupoDaMaquina));

                // O MÊS A MÊS É POR MÊS, e cada mês uma vez só: as duas séries leem do mesmo dicionário.
                if (grupoDaMaquina > 0)
                    NoMes(grupoDaMaquina, new DateOnly(dia.Year, dia.Month, 1)).MaquinasVendidas++;
            }
        }

        // -----------------------------------------------------------------------------------------
        // Potencial: o LEITOR DE REFERÊNCIA (documento 54) — as regras vigentes hoje, o catálogo do motor, a PAM de cada
        // cultura no seu ano e no anterior, a lavoura inteira e o estado, calculados uma vez por versão do assunto.
        // -----------------------------------------------------------------------------------------
        var hoje = ParametroComVigencia.HojeNoBrasil(agoraUtc);
        var potencial = await potencialDeReferencia.LerAsync(hoje, ct);
        var regras = potencial.Regras;
        var categoriaDaRegra = potencial.CategoriasDasRegras;
        var catalogoDoMotor = potencial.Catalogo;
        var ano = potencial.AnoDaAreaPlantada;
        var anoDaCultura = potencial.AnoDaCultura;
        var daRegra = potencial.Medidas;
        var culturasNoEstado = potencial.CulturasNoEstado;
        var producao = potencial.Producao;

        // A ESTRUTURA É DADO DE REFERÊNCIA (plano 2 do documento 54): guardada pela versão do assunto, e COPIADA aqui — a
        // vocação abaixo é do recorte desta consulta, e a guardada é de todas as telas.
        var deReferencia = await estruturaDeReferencia.LerAsync(ct);
        var estrutura = new Dictionary<int, EstruturaDoMunicipio>(deReferencia.PorMunicipio);

        // A VOCAÇÃO AGRÍCOLA (decidida em 28/09/2026): os tercis da fatia de lavoura entre os municípios da ADR — a ADR
        // inteira, com filtro ou sem, como o porte. Fora da ADR não há vocação: o corte é da área de atuação.
        var vocacoes = VocacaoAgricola.PelosTercis(estrutura
            .Where(e => area.TryGetValue(e.Key, out var daArea) && daArea.PertenceAAdr && e.Value.FatiaDeLavouraPercentual is not null)
            .ToDictionary(e => e.Key, e => e.Value.FatiaDeLavouraPercentual!.Value));
        foreach (var (codigo, vocacao) in vocacoes)
            estrutura[codigo] = estrutura[codigo] with { Vocacao = vocacao };
        var totaisDoEstado = deReferencia.TotaisDoEstado;
        var parqueConectado = await LerParqueConectadoAsync(ct);

        // -----------------------------------------------------------------------------------------
        // A montagem: os municípios de SP da área de atuação ou com dado, depois dos filtros.
        // -----------------------------------------------------------------------------------------
        var filtrado = consulta.Regiao is not null || consulta.LojaCodigo is not null;

        var nomeOficial = territorio.NomeOficialEmSp;

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
        // A CONTA DE CADA MUNICÍPIO É DO POTENCIAL DE REFERÊNCIA (documento 54): feita uma vez por versão e guardada —
        // a área da cultura, as categorias somadas sem somar a terra delas, e a demanda por categoria e cultura.
        var categoriasDoMotor = potencial.Categorias;
        List<ResultadoDaCategoria> MotorDoMunicipio(int municipio, bool doAnoAnterior = false) =>
            [.. potencial.DoMunicipio(municipio, doAnoAnterior).PorCategoria];
        List<DemandaNoMunicipio> DemandaDoMunicipio(int municipio, bool doAnoAnterior = false) =>
            [.. potencial.DoMunicipio(municipio, doAnoAnterior).Demanda];

        var porCategoriaNoRecorte = categoriasDoMotor.ToDictionary(
            c => c.Codigo, _ => new List<PotencialDoRecorte>(), StringComparer.Ordinal);

        var totaisDosMunicipios = new List<PotencialDoRecorte>();
        var codigosNoRecorte = new List<int>();
        var demandasDaAdr = new List<decimal>();

        var itens = new List<IndicadoresDoMunicipio>();

        foreach (var codigo in area.Keys.Concat(acumuladores.Keys.Where(k => k > 0)).Distinct().Order())
        {
            var linhaDaArea = area.GetValueOrDefault(codigo);
            var daAdr = linhaDaArea?.PertenceAAdr == true && categoriasDoMotor.Count > 0;

            if (filtrado
                && (linhaDaArea is null
                    || (consulta.Regiao is not null && linhaDaArea.Regiao != consulta.Regiao)
                    || (consulta.LojaCodigo is not null && linhaDaArea.LojaCodigo != consulta.LojaCodigo)))
            {
                // O PORTE É CORTADO NA ADR INTEIRA (issue 166): o município de fora do recorte não entra na tela, mas
                // entra nos tercis. O motor é conta em memória — nenhuma leitura a mais.
                if (daAdr && potencial.DoMunicipio(codigo).Sobreposto.DemandaAnual is { } fora)
                    demandasDaAdr.Add(fora);
                continue;
            }

            codigosNoRecorte.Add(codigo);

            var doMotor = MotorDoMunicipio(codigo);
            var noMunicipio = potencial.DoMunicipio(codigo).Sobreposto;
            if (daAdr && noMunicipio.DemandaAnual is { } demanda) demandasDaAdr.Add(demanda);
            var parcelaDe = doMotor
                .SelectMany(c => c.Resultado.Parcelas.Select(p => (Chave: new ChaveNoMotor(c.Codigo, p.CulturaCodigo), Parcela: p)))
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
                    // UMA LINHA POR PRODUTO, COM AS CATEGORIAS DENTRO (issue 240). A PAM é do produto — área,
                    // colheita, valor — e não se repete por categoria; as máquinas, sim: o trator e a
                    // colheitadeira da soja são duas contas sobre a mesma área, e as duas somam.
                    .. regras.GroupBy(r => r.ProdutoCodigoIbge).Select(doProduto =>
                    {
                        var produto = doProduto.Key;
                        var medidas = daRegra.GetValueOrDefault((codigo, produto));
                        var chaves = catalogoDoMotor.PorProdutoDaRegra.GetValueOrDefault(produto) ?? [];

                        var porCategoria = doProduto
                            .Select(regra =>
                            {
                                var categoria = regra.CategoriaDeMaquinaId is { } id && categoriaDaRegra.TryGetValue(id, out var doCatalogo)
                                    ? doCatalogo
                                    : new CategoriaDaRegra(
                                        RepositorioDoMotorDoPotencial.SemCategoriaCodigo,
                                        RepositorioDoMotorDoPotencial.SemCategoriaNome,
                                        short.MaxValue);

                                // O NÚMERO VEM DO MOTOR, e não de uma divisão feita aqui: é a mesma conta do
                                // total da tela e da calculadora. Sem catálogo que ligue o produto — regra de
                                // um produto fora dele —, fica a divisão da própria regra, que é o que havia antes.
                                var chave = chaves.FirstOrDefault(k => k.CategoriaCodigo == categoria.Codigo);
                                var maquinas = chave is not null && parcelaDe.TryGetValue(chave, out var parcela)
                                    ? parcela.Parque
                                    : regra.MaquinasTeoricas(medidas.AreaPlantadaHectares);

                                return (categoria.Ordem, Linha: new MaquinasTeoricasNaCategoria(
                                    categoria.Codigo, categoria.Nome, regra.HectaresPorMaquina, regra.ModeloDeReferencia, maquinas));
                            })
                            .OrderBy(x => x.Ordem)
                            .ThenBy(x => x.Linha.CategoriaCodigo, StringComparer.Ordinal)
                            .Select(x => x.Linha)
                            .ToList();

                        // A SOMA SAI DOS NÚMEROS INTEIROS, e cada linha só é arredondada para mostrar: somar
                        // os arredondados daria um total que não bate com a conta do motor.
                        decimal? total = porCategoria.Any(c => c.Maquinas is not null)
                            ? porCategoria.Sum(c => c.Maquinas ?? 0m)
                            : null;

                        short? anoDela = anoDaCultura.TryGetValue(produto, out var a) ? a : null;
                        var unidade = anoDela is { } doAno ? UnidadesDaPam.DaQuantidade(produto, doAno) : null;
                        return new PotencialTerritorial(
                            produto,
                            medidas.AreaPlantadaHectares,
                            total is { } t ? decimal.Round(t, 1) : null,
                            medidas.AreaColhidaHectares,
                            medidas.ValorDaProducaoMilReais,
                            anoDela,
                            medidas.QuantidadeProduzida,
                            unidade?.Nome,
                            UnidadesDaPam.Produtividade(medidas.QuantidadeProduzida, medidas.AreaColhidaHectares),
                            unidade?.DaProdutividade,
                            [.. porCategoria.Select(c => c with { Maquinas = c.Maquinas is { } m ? decimal.Round(m, 1) : null })]);
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
                        noMunicipio.MotivoSemDemanda),
                oArtTrouxeVenda ? acumulador.MaquinasVendidas : null,
                DemandaPorCategoriaECultura: DemandaDoMunicipio(codigo),
                ParqueConectado: parqueConectado.GetValueOrDefault(codigo),
                DemandaNoAnoAnterior: consulta.ComDemandaDoAnoAnterior && daAdr ? DemandaDoMunicipio(codigo, doAnoAnterior: true) : null));
        }

        var foraDoMapa = GruposForaDoMapa
            .Where(g => acumuladores.ContainsKey(g.Grupo))
            .Select(g => new IndicadoresForaDoMapa(
                g.Codigo, g.Descricao, acumuladores[g.Grupo].Cobertura(), acumuladores[g.Grupo].Vendas(),
                oArtTrouxeVenda ? acumuladores[g.Grupo].MaquinasVendidas : null))
            .ToList();

        // A CATEGORIA SAI NA ORDEM DE EXIBIÇÃO DO CATÁLOGO, a mesma do potencial por categoria: as duas
        // listas ficam lado a lado na tela, e ordens diferentes fariam o leitor comparar linhas trocadas.
        var categoriaPeloCodigo = categoriaDaLinha.Values
            .DistinctBy(c => c.Codigo, StringComparer.Ordinal)
            .ToDictionary(c => c.Codigo, c => (c.Nome, c.Ordem), StringComparer.Ordinal);

        // O TOTAL DE UNIDADES DO RECORTE É A SOMA DOS MUNICÍPIOS QUE ENTRARAM NELE, e não de todos os
        // grupos: a captura divide este número pela demanda DO RECORTE, e o que está fora do mapa não
        // tem demanda do outro lado da razão. Ele sai à parte, para o leitor saber que existe. A MESMA CONTA
        // vale para o ano anterior, que é o numerador da captura de antes (27/09/2026).
        VendasDeMaquinaDoRecorte UnidadesDoRecorte(IReadOnlyDictionary<int, Acumulador> fonte)
        {
            var doRecorte = codigosNoRecorte.Where(fonte.ContainsKey).Select(c => fonte[c]).ToList();

            return new VendasDeMaquinaDoRecorte(
                CriterioDasVendasDoArt.Criterio.ToString(),
                CriterioDasVendasDoArt.Frase,
                doRecorte.Sum(a => a.MaquinasVendidas),
                [
                    .. doRecorte
                        .SelectMany(a => a.MaquinasPorCategoria)
                        .GroupBy(c => c.Key, StringComparer.Ordinal)
                        .Select(c => new UnidadesNaCategoria(
                            c.Key,
                            categoriaPeloCodigo.TryGetValue(c.Key, out var doCatalogo) ? doCatalogo.Nome : c.Key,
                            c.Sum(u => u.Value)))
                        .OrderBy(c => categoriaPeloCodigo.TryGetValue(c.CategoriaCodigo, out var ordem) ? ordem.Ordem : short.MaxValue)
                        .ThenBy(c => c.CategoriaCodigo, StringComparer.Ordinal)
                ],
                GruposForaDoMapa.Where(g => fonte.ContainsKey(g.Grupo)).Sum(g => fonte[g.Grupo].MaquinasVendidas),
                doRecorte.Sum(a => a.MaquinasSemClassificacao),
                doRecorte.Sum(a => a.MaquinasEmLinhaSemCategoria),
                vendasSemAData,
                vendaMaisRecente,
                carregadoAte);
        }

        var maquinasVendidas = !oArtTrouxeVenda ? null : UnidadesDoRecorte(acumuladores);

        // AS UNIDADES DE UM MUNICÍPIO POR CATEGORIA — a mesma leitura do recorte, para a captura da ficha contar
        // só as categorias que têm demanda ali (27/09/2026).
        List<UnidadesNaCategoria> UnidadesDoMunicipioPorCategoria(Acumulador? acumulador) =>
        [
            .. (acumulador?.MaquinasPorCategoria ?? [])
                .Select(c => new UnidadesNaCategoria(
                    c.Key, categoriaPeloCodigo.TryGetValue(c.Key, out var doCatalogo) ? doCatalogo.Nome : c.Key, c.Value))
                .OrderBy(c => categoriaPeloCodigo.TryGetValue(c.CategoriaCodigo, out var ordem) ? ordem.Ordem : short.MaxValue)
                .ThenBy(c => c.CategoriaCodigo, StringComparer.Ordinal)
        ];

        var periodoAnterior = MontarPeriodoAnterior(
            janela, janelaAnterior, primeiraCompetenciaDoFaturamento, primeiroMesDoArt, oArtTrouxeVenda,
            codigosNoRecorte.Where(c => area.TryGetValue(c, out var daArea) && daArea.PertenceAAdr).ToHashSet(),
            mensal,
            UnidadesDoRecorte(anteriores),
            consulta.MesEmCurso);

        // O ANO ANTERIOR DE CADA MUNICÍPIO, em reais e em unidades — só quando a fonte cobre a janela inteira.
        // Este bloco é à parte da montagem de propósito: ele não mexe no potencial, só acrescenta a base da
        // variação a cada linha.
        itens = [
            .. itens.Select(i => i with
            {
                VendasNoPeriodoAnterior = periodoAnterior.VendasCobertas
                    ? (anteriores.GetValueOrDefault(i.CodigoIbge) ?? new Acumulador()).Vendas()
                    : null,
                MaquinasVendidasNoPeriodoAnterior = oArtTrouxeVenda && periodoAnterior.MaquinasCobertas
                    ? anteriores.GetValueOrDefault(i.CodigoIbge)?.MaquinasVendidas ?? 0
                    : null,
                MaquinasPorCategoria = oArtTrouxeVenda
                    ? UnidadesDoMunicipioPorCategoria(acumuladores.GetValueOrDefault(i.CodigoIbge))
                    : null
            })
        ];

        return new IndicadoresTerritoriais(
            consulta.CompetenciaInicial,
            consulta.CompetenciaFinal,
            agoraUtc,
            await contexto.ClienteCarteiras.AsNoTracking().MaxAsync(v => v.UltimaInteracaoEm, ct),
            ano,
            [.. potencial.RegrasAplicadas],
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
                    [.. categoriasDoMotor.Select(c => (c.Codigo, c.Nome))],
                    porCategoriaNoRecorte,
                    totaisDosMunicipios,
                    RelevanciaDoRecorte(codigosNoRecorte, producao, totaisDoEstado),
                    RelevanciaPorCultura(codigosNoRecorte, daRegra, culturasNoEstado)),
            deReferencia.TotaisDaRegiao,
            MontarProcedencias(agoraUtc, maquinasVendidas, deReferencia.Anos),
            MaquinasVendidas: maquinasVendidas,
            PeriodoAnterior: periodoAnterior,
            LavouraDoRecorte: await LerLavouraDoRecorteAsync(
                codigosNoRecorte.Where(c => area.TryGetValue(c, out var daArea) && daArea.PertenceAAdr).ToList(), ct),
            DemandasDosMunicipiosDaAdr: demandasDaAdr);
    }

    /// <summary>
    /// O MESMO TRECHO DO ANO ANTERIOR, montado — a cobertura das duas fontes e o mês a mês da ADR do recorte.
    ///
    /// <para><b>A série é dos municípios DA ADR do recorte</b>, porque é a soma deles que os cartões mostram: os
    /// vizinhos com cliente ficam no mapa, e não no número da área de atuação.</para>
    /// </summary>
    /// <param name="janela">A janela pedida.</param>
    /// <param name="anterior">O mesmo trecho do ano anterior.</param>
    /// <param name="primeiraDoFaturamento">Desde quando há faturamento ao alcance.</param>
    /// <param name="primeiroMesDoArt">Desde quando o ART traz venda.</param>
    /// <param name="oArtTrouxeVenda">Se há venda do ART ao alcance.</param>
    /// <param name="daAdr">Os municípios da ADR que entraram no recorte.</param>
    /// <param name="mensal">As vendas de cada município em cada mês das duas janelas.</param>
    /// <param name="unidadesAnteriores">As unidades do recorte no ano anterior, já com a quebra.</param>
    /// <param name="mesEmCurso">O último mês pedido, quando ele ainda está em curso — aí não há comparação.</param>
    private static PeriodoAnterior MontarPeriodoAnterior(
        Dominio.Comum.JanelaDeCompetencia janela,
        Dominio.Comum.JanelaDeCompetencia anterior,
        DateOnly? primeiraDoFaturamento,
        DateOnly? primeiroMesDoArt,
        bool oArtTrouxeVenda,
        IReadOnlySet<int> daAdr,
        IReadOnlyDictionary<(int Grupo, DateOnly Mes), VendasDoMes> mensal,
        VendasDeMaquinaDoRecorte unidadesAnteriores,
        DateOnly? mesEmCurso)
    {
        var cobreVendas = mesEmCurso is null && primeiraDoFaturamento is { } primeira && primeira <= anterior.Inicial;
        var cobreMaquinas = mesEmCurso is null && oArtTrouxeVenda && primeiroMesDoArt is { } primeiro && primeiro <= anterior.Inicial;

        List<VendasNoMes> Serie(Dominio.Comum.JanelaDeCompetencia j, bool comUnidades)
        {
            var meses = new List<VendasNoMes>(j.Meses);
            for (var mes = j.Inicial; mes <= j.Final; mes = mes.AddMonths(1))
            {
                var doMes = daAdr.Select(c => mensal.GetValueOrDefault((c, mes))).Where(v => v is not null).ToList();
                meses.Add(new VendasNoMes(
                    mes,
                    decimal.Round(doMes.Sum(v => v!.ValorLiquido), 2),
                    decimal.Round(doMes.Sum(v => v!.Maquina), 2),
                    decimal.Round(doMes.Sum(v => v!.PosVenda), 2),
                    comUnidades ? doMes.Sum(v => v!.MaquinasVendidas) : null));
            }

            return meses;
        }

        return new PeriodoAnterior(
            anterior.Inicial,
            anterior.Final,
            primeiraDoFaturamento,
            oArtTrouxeVenda ? primeiroMesDoArt : null,
            cobreMaquinas ? unidadesAnteriores : null,
            Serie(janela, oArtTrouxeVenda),
            cobreVendas ? Serie(anterior, cobreMaquinas) : [],
            mesEmCurso);
    }

    /// <summary>
    /// A ÁREA DE TODAS AS CULTURAS DA PAM nos municípios da ADR do recorte (issue 168) — uma leitura PRÓPRIA,
    /// que não passa pelas regras de potencial.
    ///
    /// <para><b>Cada produto no último ano em que a área DELE foi divulgada</b> (issue 152), como no resto da
    /// tela: o maior ano da tabela inteira misturaria anos em silêncio no dia em que a PAM nova entrasse
    /// incompleta. A soma é em memória — o SQLite dos testes não agrega decimal —, e o volume é de uma linha por
    /// produto e município dos anos carregados.</para>
    ///
    /// <para><b>Sigilo não vira zero:</b> o município sem área divulgada não entra na soma, e a contagem dos que
    /// entraram vai junto.</para>
    /// </summary>
    /// <param name="codigosDaAdr">Os municípios da ADR que entraram no recorte, pelo código IBGE.</param>
    /// <param name="ct">Cancelamento.</param>
    private async Task<List<AreaDoProdutoNoRecorte>> LerLavouraDoRecorteAsync(IReadOnlyList<int> codigosDaAdr, CancellationToken ct)
    {
        if (codigosDaAdr.Count == 0) return [];

        var linhas = await (
                from linha in contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
                join municipio in contexto.Municipios.AsNoTracking() on linha.MunicipioId equals municipio.Id
                where municipio.CodigoIbge != null && codigosDaAdr.Contains(municipio.CodigoIbge!.Value)
                select new
                {
                    linha.ProdutoCodigoIbge,
                    linha.ProdutoNome,
                    linha.Ano,
                    linha.AreaPlantadaHectares,
                    linha.AreaColhidaHectares
                })
            .ToListAsync(ct);

        return
        [
            .. linhas
                .GroupBy(l => l.ProdutoCodigoIbge)
                .Select(produto =>
                {
                    var comArea = produto.Where(l => l.AreaPlantadaHectares is not null).ToList();
                    if (comArea.Count == 0) return null;

                    var ano = comArea.Max(l => l.Ano);
                    var doAno = comArea.Where(l => l.Ano == ano).ToList();
                    var colhidas = doAno.Where(l => l.AreaColhidaHectares is not null).ToList();

                    return new AreaDoProdutoNoRecorte(
                        produto.Key,
                        doAno[0].ProdutoNome,
                        ano,
                        doAno.Sum(l => l.AreaPlantadaHectares!.Value),
                        colhidas.Count == 0 ? null : colhidas.Sum(l => l.AreaColhidaHectares!.Value),
                        doAno.Count);
                })
                .Where(a => a is not null)
                .Select(a => a!)
                .OrderByDescending(a => a.AreaPlantadaHectares)
                .ThenBy(a => a.ProdutoCodigoIbge)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaParaFiltro>> ListarCategoriasDeMaquinaAsync(CancellationToken ct)
    {
        // O CATÁLOGO E O DE-PARA, e não uma lista escrita aqui: a categoria que o comercial ligar a uma linha
        // de produto aparece sozinha no filtro. Os dois cruzam por Id, sem tocar a colação da linha.
        var comLinha = await contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
            .Select(l => l.CategoriaDeMaquinaId)
            .Distinct()
            .ToListAsync(ct);

        return
        [
            .. (await contexto.CategoriasDeMaquina.AsNoTracking()
                    .Where(c => comLinha.Contains(c.Id))
                    .Select(c => new { c.Codigo, c.Nome, c.Ordem })
                    .ToListAsync(ct))
                .OrderBy(c => c.Ordem)
                .ThenBy(c => c.Codigo, StringComparer.Ordinal)
                .Select(c => new CategoriaParaFiltro(c.Codigo, c.Nome, c.Ordem))
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResponsavelDeCarteira>> ListarResponsaveisDasCarteirasAsync(
        bool empresaInteira, CancellationToken ct)
    {
        // NA VISÃO DA EMPRESA, OS RESPONSÁVEIS DE TODAS AS FILIAIS (revisão de 27/09/2026): sem abrir o alcance, a
        // lista — e a validação do filtro — ficava presa à filial do cabeçalho, e o painel da empresa inteira não
        // aceitava o CEN de outra filial. A abertura vai para o diário com o motivo, como a do painel.
        using var alcance = empresaInteira
            ? contexto.AbrirAlcanceEntreEmpresas("Responsáveis das carteiras na visão consolidada da empresa (documento 32)")
            : null;

        // AS CARTEIRAS COMERCIAIS AO ALCANCE (filtro global de filial), e o dono de cada uma. O gestor vem do
        // cadastro de usuário — hoje, em produção, nenhum responsável tem gestor cadastrado, e a tela diz isso.
        var carteiras = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Natureza == NaturezaDaCarteira.Comercial)
            .Select(c => c.ResponsavelId)
            .ToListAsync(ct);

        var ids = carteiras.Distinct().ToList();
        var usuarios = await contexto.Usuarios.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.NomeExibicao, u.Natureza, u.GestorId })
            .ToListAsync(ct);

        var idsDosGestores = usuarios.Where(u => u.GestorId is not null).Select(u => u.GestorId!.Value).Distinct().ToList();
        var gestores = idsDosGestores.Count == 0
            ? new Dictionary<long, string>()
            : await contexto.Usuarios.AsNoTracking()
                .Where(u => idsDosGestores.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.NomeExibicao, ct);

        return
        [
            .. usuarios
                .Select(u => new ResponsavelDeCarteira(
                    u.Id,
                    u.NomeExibicao,
                    u.Natureza.ToString(),
                    carteiras.Count(c => c == u.Id),
                    u.GestorId is { } gestor && gestores.TryGetValue(gestor, out var nome) ? nome : null))
                .OrderBy(r => r.Nome, StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), true))
        ];
    }

    /// <summary>
    /// DE ONDE VEIO CADA NÚMERO — a procedência por indicador (issue 167).
    ///
    /// <para><b>A competência sai do dado, e não de uma constante:</b> o ano é o que está carregado
    /// nas tabelas municipais neste banco. Um texto fixo no front diria 2017 para sempre, mesmo depois
    /// do Censo de 2028 entrar.</para>
    ///
    /// <para><b>Fonte não carregada devolve nulo</b>, e a tela não mostra carimbo — em vez de carimbar
    /// uma origem que ninguém leu.</para>
    /// </summary>
    /// <param name="agoraUtc">O instante da leitura.</param>
    /// <param name="maquinas">As vendas em unidades do recorte; nulas quando o ART não trouxe nada.</param>
    /// <param name="anos">O ano carregado de cada fonte — da estrutura de referência (plano 2 do documento 54).</param>
    private static ProcedenciasDoTerritorio MontarProcedencias(
        DateTime agoraUtc, VendasDeMaquinaDoRecorte? maquinas, AnosDasFontes anos)
    {
        var anoDaLavoura = anos.Lavoura;
        var anoDoCenso = anos.Censo;
        var anoDoRebanho = anos.Rebanho;
        var temUsina = anos.TemUsina;

        return new ProcedenciasDoTerritorio(
            AreaPlantada: anoDaLavoura is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "PAM — Produção Agrícola Municipal", "5457", "Área plantada",
                    anoDaLavoura.Value.ToString(), agoraUtc,
                    "O município com produção sigilosa não entra na soma — e ausência não é zero."),

            ValorDaProducao: anoDaLavoura is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "PAM — Produção Agrícola Municipal", "5457", "Valor da produção",
                    anoDaLavoura.Value.ToString(), agoraUtc,
                    "É o que o município COLHE, em mil reais — não é venda da Tracbel nem preço de máquina. " +
                    "O café entra uma vez só, pelo Total do IBGE."),

            Tratores: anoDoCenso is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "Censo Agropecuário", "6778", "Tratores existentes",
                    anoDoCenso.Value.ToString(), agoraUtc,
                    $"O Censo é de {anoDoCenso} e o próximo sai em 2028: o parque tem essa idade. " +
                    "Valor hachurado é sigilo do IBGE, que não é zero — ele oculta o número quando poucos " +
                    "estabelecimentos o compõem. As faixas de potência não se somam ao Total: o Total é uma " +
                    "categoria ao lado delas."),

            Estabelecimentos: anoDoCenso is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "Censo Agropecuário", "6779", "Estabelecimentos agropecuários",
                    anoDoCenso.Value.ToString(), agoraUtc,
                    "Sigilo do IBGE oculta o número onde poucos estabelecimentos o compõem; ausência não é zero."),

            Rebanho: anoDoRebanho is null
                ? null
                : new ProcedenciaDoIndicador(
                    "IBGE/SIDRA", "PPM — Pesquisa da Pecuária Municipal", "3939", "Efetivo dos rebanhos — bovino",
                    anoDoRebanho.Value.ToString(), agoraUtc,
                    "A PPM é anual e anda sozinha: o ano dela não acompanha o do Censo."),

            Usinas: !temUsina
                ? null
                : new ProcedenciaDoIndicador(
                    "ANP", "Autorizações de produção de etanol", null, "Capacidade autorizada (m³/dia)",
                    null, agoraUtc,
                    "A ANP só enxerga usina de ETANOL: ausência aqui não prova ausência de usina — " +
                    "a que só faz açúcar não é autorizada por ela e não aparece."),

            // O CARIMBO DA CAPTURA. Ele existe só quando há leitura a carimbar: sem venda no ART, a
            // procedência é nula e a tela não carimba uma origem que não leu.
            MaquinasVendidas: maquinas is null
                ? null
                : new ProcedenciaDoIndicador(
                    "ART — sistema comercial de vendas", "Vendas de máquina, por chassi", null,
                    "Máquinas vendidas (unidades)",
                    maquinas.VendaMaisRecente is { } ate ? $"até {ate:dd/MM/yyyy}" : null,
                    maquinas.CarregadoAte,
                    $"{maquinas.FraseDoCriterio} O ART não traz financiamento. O que se vê é o que a última carga " +
                    "trouxe — a data da carga, ao lado, diz até quando."));
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

    /// <summary>
    /// AS MÁQUINAS CONECTADAS DE CADA MUNICÍPIO (telemetria do Operations Center, 28/09/2026) — pelo município da última
    /// posição, e pelo filtro de filial do equipamento: quem consulta conta as máquinas ao seu alcance.
    /// </summary>
    private async Task<IReadOnlyDictionary<int, ParqueConectadoNoMunicipio>> LerParqueConectadoAsync(CancellationToken ct)
    {
        var linhas = await contexto.Equipamentos.AsNoTracking()
            .Where(e => e.ExcluidoEm == null && e.MunicipioDaPosicaoId != null && e.PosicaoEm != null)
            .Join(contexto.Municipios.AsNoTracking().Where(m => m.CodigoIbge != null),
                e => e.MunicipioDaPosicaoId, m => (int?)m.Id,
                (e, m) => new { Codigo = m.CodigoIbge!.Value, e.HorimetroAtual, e.HorimetroAtualizadoEm, PosicaoEm = e.PosicaoEm!.Value })
            .ToListAsync(ct);

        return ParqueConectadoNoMunicipio.PorMunicipio(
            [.. linhas.Select(l => new MaquinaConectada(l.Codigo, l.HorimetroAtual, l.HorimetroAtualizadoEm, l.PosicaoEm))]);
    }

    /// <summary>O grupo fora do mapa de cada natureza de contraparte sem cliente no CRM.</summary>
    private static int GrupoDaNatureza(NaturezaDoParceiro natureza) => natureza switch
    {
        NaturezaDoParceiro.Fabrica => NotaParaFabrica,
        NaturezaDoParceiro.EmpresaDoGrupo => NotaParaEmpresaDoGrupo,
        NaturezaDoParceiro.OutraRevenda => NotaParaOutraRevenda,
        _ => NotaSemCadastroDeCliente
    };

    /// <summary>As vendas de um município num mês — o ponto do mini-gráfico, antes de somar a ADR.</summary>
    private sealed class VendasDoMes
    {
        public decimal ValorLiquido;
        public decimal Maquina;
        public decimal PosVenda;
        public int MaquinasVendidas;
    }

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

        /// <summary>As máquinas vendidas a clientes deste grupo, em UNIDADES — o ART (issue 69).</summary>
        public int MaquinasVendidas;

        /// <summary>Delas, as que a classificação de produto do CRM não alcança.</summary>
        public int MaquinasSemClassificacao;

        /// <summary>Delas, as de linha que existe e ainda não foi ligada a uma categoria.</summary>
        public int MaquinasEmLinhaSemCategoria;

        /// <summary>Delas, quantas em cada categoria de máquina.</summary>
        public readonly Dictionary<string, int> MaquinasPorCategoria = new(StringComparer.Ordinal);

        public readonly Dictionary<long, int> VinculosPorResponsavel = [];
        public readonly Dictionary<long, HashSet<long>> CarteirasPorResponsavel = [];

        /// <summary>Os clientes distintos com vínculo em carteira comercial — o mesmo cliente em duas carteiras conta uma vez.</summary>
        public readonly HashSet<long> ClientesComVinculo = [];

        /// <summary>Os clientes do grupo pela classe ABC: A, B, C, D e sem classe.</summary>
        private readonly int[] porClasse = new int[5];

        public void ContarClasse(ClasseDeCliente? classe) => porClasse[classe switch
        {
            ClasseDeCliente.A => 0,
            ClasseDeCliente.B => 1,
            ClasseDeCliente.C => 2,
            ClasseDeCliente.D => 3,
            _ => 4
        }]++;

        public CoberturaTerritorial Cobertura() => new(
            Clientes, Vinculos, Cobertos + ForaDaCadencia + NuncaContatados, Cobertos, ForaDaCadencia, NuncaContatados, SemCadencia,
            new ClientesPorClasse(porClasse[0], porClasse[1], porClasse[2], porClasse[3], porClasse[4]),
            ClientesComVinculo.Count);

        public VendasTerritoriais Vendas() => new(
            ClientesQueCompraram, decimal.Round(ValorLiquido, 2), decimal.Round(Maquina, 2),
            decimal.Round(Peca, 2), decimal.Round(Servico, 2), decimal.Round(Outros, 2));
    }
}
