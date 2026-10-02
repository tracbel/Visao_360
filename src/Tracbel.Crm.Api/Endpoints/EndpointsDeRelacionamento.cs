using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// As rotas de LEITURA do relacionamento — processo, tarefa, interação e cobertura.
///
/// <para>Todas são somente leitura nesta etapa, e é uma decisão, não uma etapa faltando: a
/// escrita de processo e de tarefa carrega o motor de regras (mudar de fase dispara automação),
/// e ele entra junto com a tela que o exercita. O que existe aqui é o que as telas precisam para
/// mostrar número real em vez de exemplo.</para>
///
/// <para>Cada endpoint faz três coisas e nada mais: recebe, delega ao caso de uso e traduz o
/// <c>Resultado</c> em código HTTP. Nenhuma decisão de negócio mora neste arquivo.</para>
/// </summary>
public static class EndpointsDeRelacionamento
{
    /// <summary>Registra as rotas de processo.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearProcessos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/processos")
            .WithTags("Processos (banco do CRM)");

        grupo.MapGet("/", async (
                ListarProcessos caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                string? termo = null,
                string? situacao = null,
                Guid? clienteChave = null,
                string? faseCodigo = null,
                string? tipoProcessoCodigo = null,
                string? ordenarPor = null,
                bool descendente = false,
                bool incluirEncerrados = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, termo, situacao, clienteChave, faseCodigo, tipoProcessoCodigo,
                ordenarPor, descendente, incluirEncerrados, ct)).Responder())
            .WithName("ListarProcessos")
            .ExigePermissao(Permissoes.ProcessoLer)
            .WithSummary("Lista processos e oportunidades, com fase, valor e tempo de fase.");

        grupo.MapGet("/{chave:guid}", async (Guid chave, ObterProcesso caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithName("ObterProcesso")
            .ExigePermissao(Permissoes.ProcessoLer)
            .WithSummary("Traz a ficha de um processo pela chave pública.");

        return app;
    }

    /// <summary>Registra as rotas de tarefa.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearTarefas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/tarefas")
            .WithTags("Tarefas (banco do CRM)");

        grupo.MapGet("/", async (
                ListarTarefas caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                bool minhas = false,
                string? situacao = null,
                Guid? clienteChave = null,
                DateOnly? de = null,
                DateOnly? ate = null,
                bool somenteAtrasadas = false,
                string? ordenarPor = null,
                bool descendente = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, minhas, situacao, clienteChave, de, ate, somenteAtrasadas,
                ordenarPor, descendente, ct)).Responder())
            .WithName("ListarTarefas")
            .ExigePermissao(Permissoes.TarefaLer)
            .WithSummary("Lista tarefas da agenda, com prazo e atraso calculados.");

        return app;
    }

    /// <summary>Registra as rotas de interação.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearInteracoes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/interacoes")
            .WithTags("Interações (banco do CRM)");

        grupo.MapGet("/", async (
                ListarInteracoes caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                Guid? clienteChave = null,
                string? natureza = null,
                DateOnly? de = null,
                DateOnly? ate = null) =>
            (await caso.ExecutarAsync(pagina, tamanho, clienteChave, natureza, de, ate, ct)).Responder())
            .WithName("ListarInteracoes")
            .ExigePermissao(Permissoes.InteracaoLer)
            .WithSummary("Lista a linha do tempo de contatos, filtrada por cliente.");

        return app;
    }

    /// <summary>Registra as rotas de cobertura de carteira.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearCobertura(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/cobertura")
            .WithTags("Cobertura de carteira (banco do CRM)");

        grupo.MapGet("/", async (
                ListarCobertura caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                string? classe = null,
                int? diasSemContato = null,
                bool somenteSemContato = false,
                string? ordenarPor = null,
                bool descendente = false,
                string? termo = null) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, classe, diasSemContato, somenteSemContato,
                ordenarPor, descendente, ct, termo)).Responder())
            .WithName("ListarCobertura")
            .ExigePermissao(Permissoes.CoberturaLer)
            .WithSummary(
                "A carteira cliente a cliente, com a data do último contato, o atraso de ciclo e a prioridade pela curva " +
                "ABC do cliente. `termo` filtra por um trecho do cliente, da carteira ou do responsável.");

        // AS CARTEIRAS DE UM CLIENTE moram na ficha dele, e a rota fica sob /clientes — o mesmo desenho de
        // /clientes/{chave}/maquinas-compradas. O caso de uso é de carteira, e a permissão é a da cobertura.
        app.MapGet("/api/v1/clientes/{chave:guid}/carteiras", async (
                Guid chave, ListarCarteirasDoCliente caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithTags("Clientes (banco do CRM)")
            .WithName("ListarCarteirasDoCliente")
            .ExigePermissao(Permissoes.CoberturaLer)
            .WithSummary(
                "As carteiras em que o cliente está: natureza, CEN responsável, filial, classe, cadência da linha de " +
                "negócio e a data do vínculo — nas carteiras ao alcance de quem consulta.");

        return app;
    }

    /// <summary>
    /// Registra as rotas de relatório — os agregados que as telas consomem.
    ///
    /// <para><b>Tudo aqui é calculado no banco.</b> A tela recebe dez linhas de funil, não 45 mil
    /// processos para somar no navegador. E quando o dado não sustenta a métrica, a resposta traz
    /// <c>metricasSemDado</c> com o motivo medido, em vez de um número construído sobre nada.</para>
    /// </summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearRelatorios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/relatorios")
            .WithTags("Relatórios (agregados calculados no banco)");

        grupo.MapGet("/funil", async (ObterFunil caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterFunil")
            .ExigePermissao(Permissoes.RelatorioLer)
            .WithSummary("O funil por fase: quantos processos e quanto valor, com a cobertura do valor.");

        grupo.MapGet("/perdas", async (ObterPerdas caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterPerdas")
            .ExigePermissao(Permissoes.RelatorioLer)
            .WithSummary("As perdas por motivo.");

        grupo.MapGet("/vendas-perdidas", async (
                ObterVendasPerdidas caso, CancellationToken ct, DateOnly? de = null, DateOnly? ate = null,
                string? formulario = null, Guid? responsavel = null) =>
                (await caso.ExecutarAsync(de, ate, formulario, responsavel, ct)).Responder())
            .WithName("ObterVendasPerdidas")
            .ExigePermissao(Permissoes.RelatorioLer)
            .WithSummary(
                "As vendas perdidas registradas no formulário: por motivo, para qual concorrente " +
                "e a que distância de preço.")
            .WithDescription(
                "Só a resposta principal conta. Período pela data em que o formulário foi preenchido (`de` e `ate`, AAAA-MM-DD; " +
                "padrão: o ano fiscal até o último mês fechado); `formulario` filtra pelo formulário do Vórtice; `responsavel` " +
                "(chave pública) pelo responsável do processo no funil. Os processos perdidos vêm do funil do Vórtice.");

        // O FUNIL POR ESTÁGIO (documento 52, 27/09/2026): Lead → Faturamento, dos processos 31/41/50 do Vórtice. A mesma
        // permissão do funil por fase, que a tela do Funil já exige; a filial é o filtro global, como em toda rota daqui.
        grupo.MapGet("/funil-por-estagio", async (
                ObterFunilPorEstagio caso, CancellationToken ct, DateOnly? de = null, DateOnly? ate = null,
                string? @base = null, Guid? carteira = null, Guid? responsavel = null) =>
                (await caso.ExecutarAsync(de, ate, @base, carteira, responsavel, ct)).Responder())
            .WithName("ObterFunilPorEstagio")
            .ExigePermissao(Permissoes.RelatorioLer)
            .WithSummary("O funil por estágio — Lead, Qualificado, Cobertura, Negociação, Pedido e Faturamento — do Vórtice.")
            .WithDescription(
                "`base=abertura` (padrão) é a coorte: os processos abertos no período e até onde chegaram; `base=etapa` é o " +
                "fluxo: as etapas alcançadas no período. Período em `de` e `ate` (AAAA-MM-DD; padrão: o ano fiscal até o " +
                "último mês fechado). `carteira` e `responsavel` pela chave pública. Por estágio: processos, % sobre o " +
                "anterior e sobre o Lead, o subfunil digital e os desfechos; mais os processos parados em Negociação ou " +
                "Pedido e a última execução da rotina PROCESSOS_VORTICE.");

        grupo.MapGet("/faturamento", async (int? anoFiscal, ObterFaturamento caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(anoFiscal, ct)).Responder())
            .WithName("ObterFaturamento")
            .ExigePermissao(Permissoes.FaturamentoLer)
            .WithSummary(
                "O faturamento lido da SD2 do Protheus: série de doze meses e os cinco maiores clientes, com a " +
                "competência mais recente sempre junto do número. Com anoFiscal, a série termina no fim do ano (ou no " +
                "último mês carregado) e o ranking é o do ano; sem ele, os doze meses até o último carregado e o acumulado.");

        // O FATURAMENTO DE UM CLIENTE mora na ficha dele, e a rota fica sob /clientes, como as máquinas compradas.
        app.MapGet("/api/v1/clientes/{chave:guid}/faturamento", async (
                Guid chave, ObterFaturamentoDoCliente caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithTags("Clientes (banco do CRM)")
            .WithName("ObterFaturamentoDoCliente")
            .ExigePermissao(Permissoes.FaturamentoLer)
            .WithSummary(
                "O faturamento do cliente lido da SD2 do Protheus: os doze meses até a competência mais recente da " +
                "carga, a quebra máquina, peça, serviço e outros, a filial que emitiu a nota e a data da carga.");

        // AS PEÇAS DO CLIENTE (02/10/2026): o faturamento de peças por mês e os orçamentos, da rotina 15 POS_VENDA_PROTHEUS. A
        // mesma permissão do faturamento do cliente, que é o mesmo assunto em reais.
        app.MapGet("/api/v1/clientes/{chave:guid}/pecas", async (
                Guid chave, ObterPecasDoCliente caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithTags("Clientes (banco do CRM)")
            .WithName("ObterPecasDoCliente")
            .ExigePermissao(Permissoes.FaturamentoLer)
            .WithSummary(
                "As peças do cliente, do Protheus: os doze meses com balcão × oficina e o grupo comercial (na régua do painel " +
                "\"Faturamento Peças\" do BI), a série, o vendedor principal e os orçamentos em aberto. Sem custo nem margem.");

        grupo.MapGet("/indicadores-executivos", async (
                ObterIndicadoresExecutivos caso, CancellationToken ct, int? ano = null, int? anoFiscal = null) =>
                (await caso.ExecutarAsync(ano, anoFiscal, ct)).Responder())
            .WithName("ObterIndicadoresExecutivos")
            .ExigePermissao(Permissoes.FaturamentoLer)
            .WithSummary("Os cinco indicadores da Visão 360 para a filial do cabeçalho.")
            .WithDescription(
                "Faturamento da competência mais recente com a nota sem cliente separada por natureza; realizado do " +
                "ANO FISCAL `anoFiscal` (novembro a outubro, com o nome do ano em que termina; padrão: o corrente, até " +
                "hoje) — ou do ano civil `ano`, quando pedido — ao lado da meta de faturamento da filial; clientes " +
                "únicos pela filial de cadastro e vínculos pela filial da carteira; cobertura pela cadência declarada; " +
                "vendas perdidas registradas. Todo número se soma entre filiais, exceto clientesNasCarteirasDaFilial.");

        grupo.MapGet("/cen", async (
                ObterPainelDoCen caso, CancellationToken ct, Guid? responsavel = null, Guid? carteira = null) =>
                (await caso.ExecutarAsync(responsavel, ct, carteira)).Responder())
            .WithName("ObterPainelDoCen")
            .ExigePermissao(Permissoes.RelatorioLer)
            .WithSummary(
                "O painel de um CEN: cobertura por classe A/B/C/D contra a cadência declarada, " +
                "processos ganhos e perdidos, e o faturamento da carteira. Sem `responsavel`, " +
                "devolve o consolidado; `carteira` (chave pública) recorta numa carteira só.");

        grupo.MapGet("/agenda", async (ObterPainelDaAgenda caso, CancellationToken ct, bool minhas = false) =>
                (await caso.ExecutarAsync(minhas, ct)).Responder())
            .WithName("ObterPainelDaAgenda")
            .ExigePermissao(Permissoes.TarefaLer)
            .WithSummary("O painel do CEN: pendentes, atrasadas, hoje, próximos sete dias.");

        grupo.MapGet("/cobertura", async (ObterResumoDeCobertura caso, CancellationToken ct, string? classe = null) =>
                (await caso.ExecutarAsync(ct, classe)).Responder())
            .WithName("ObterResumoDeCobertura")
            .ExigePermissao(Permissoes.CoberturaLer)
            .WithSummary(
                "A cobertura por carteira: clientes, contatados em 30 e 90 dias, nunca contatados. `classe` (A, B, C ou D) " +
                "conta só os clientes daquela classe da curva ABC; sem classe apurada conta como D, como no painel do CEN.");

        // A META DE VENDA × O REALIZADO (#138, 27/09/2026). A profundidade de Meta.Ler decide o alcance (D-M5): Próprios vê
        // a própria meta; a filial inteira a partir de EmpresaEAbaixo. Uma filial por chamada, como as outras da Visão 360.
        // O FORECAST DA GERÊNCIA (28/09/2026): a previsão de cada gestor da API Gestão de Negócios, com o PO e o realizado do time.
        // É da gerência: Meta.Ler a partir da filial inteira.
        grupo.MapGet("/forecast", async (ObterForecastDaGerencia caso, CancellationToken ct, string? competencia = null) =>
                (await caso.ExecutarAsync(competencia, ct)).Responder())
            .WithName("ObterForecastDaGerencia")
            .ExigePermissao(Permissoes.MetaLer)
            .WithSummary("O forecast e o best guess de cada gestor (API Gestão de Negócios), com o PO e o realizado do time, num mês.")
            .WithDescription(
                "Por gestor e linha: o PO (a meta dos consultores do time, pelo de-para de consultores da GN), o Forecast e o Best Guess " +
                "do gestor e as máquinas vendidas pelo time (ART, pelo vendedor). Mês padrão: o corrente, quando tem forecast; senão, o " +
                "mais recente com forecast. Pede Meta.Ler a partir da filial inteira.");

        grupo.MapGet("/metas", async (
                ObterMetaERealizado caso, CancellationToken ct, string? competenciaInicial = null, string? competenciaFinal = null) =>
                (await caso.ExecutarAsync(competenciaInicial, competenciaFinal, ct)).Responder())
            .WithName("ObterMetaERealizado")
            .ExigePermissao(Permissoes.MetaLer)
            .WithSummary("A meta de venda (API Gestão de Negócios) × as máquinas vendidas (ART), da filial do cabeçalho.")
            .WithDescription(
                "Meta em unidades por mês, linha e consultor, contra as vendas de máquina que o CRM tem (frota.VendaDeMaquina, pela " +
                "data da ENTREGA, a régua da GN desde 28/09/2026 — a vendida e não entregue vem à parte); as vendas do ART que aguardam cadastro ou chassi vêm em número, à parte. Consórcio à parte, em cotas. " +
                "Período padrão: o ano fiscal (novembro a outubro) até o último mês fechado, com o mês em curso à parte e o realizado " +
                "do mesmo trecho do ano fiscal anterior. `competenciaInicial` e `competenciaFinal` (AAAA-MM) pedem outro período. " +
                "No alcance Próprios, só a meta e as vendas da própria pessoa.");

        // O ESTOQUE E A COBERTURA (28/09/2026): a disponibilidade de máquina para a venda, pela filial do cabeçalho — ou por
        // todas, em "Todas as filiais" —, e a cobertura em meses, da organização. Sem custo e sem cliente.
        grupo.MapGet("/estoque", async (ObterEstoqueECobertura caso, CancellationToken ct) => (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterEstoqueECobertura")
            .ExigePermissao(Permissoes.RelatorioLer)
            .WithSummary("O estoque de máquinas e os pedidos à fábrica (TOTVS, pela API Gestão de Negócios), e a cobertura em meses.")
            .WithDescription(
                "Máquina a máquina, da filial do cabeçalho: situação, reserva, pagamento, dias no pátio (de hoje, pela data de entrada) e " +
                "chegada prevista do pedido; os totais e os grupos. A cobertura — o estoque dividido pelas vendas, em quantidade — é da " +
                "organização, por mês e por grupo. O custo e o cliente do atendimento não são lidos.");

        return app;
    }
}
