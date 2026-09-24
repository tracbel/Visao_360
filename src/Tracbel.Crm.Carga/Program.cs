using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Carga.Sincronizacao;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Integracao.Anp;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.BancoCentral;
using Tracbel.Crm.Integracao.Conab;
using Tracbel.Crm.Integracao.Ibge;
using Tracbel.Crm.Integracao.Protheus;
using Tracbel.Crm.Integracao.Socicana;
using Tracbel.Crm.Integracao.Vortice;

// --servico-art — O SERVIÇO DO WINDOWS TracbelCrmSincronizacaoArt (documento 35, seção 11).
//
// Vem antes de tudo, inclusive da codificação do console: como serviço não há console, e o ciclo, a
// configuração e o log são outros (Sincronizacao/ServicoDeSincronizacaoDoArt.cs).
if (args.Contains("--servico-art", StringComparer.Ordinal))
    return await HospedagemDaSincronizacao.RodarAsync(args);

Console.OutputEncoding = Encoding.UTF8;

// --conceder-administrador-inicial — O PRIMEIRO ADMINISTRADOR (AdministradorInicial.cs). Não é carga
// nem lê fonte nenhuma: é o passo de instalação que dá a alguém o perfil que a tela de administração
// exige. Sai antes de tudo o que é da carga, inclusive da trava do Vórtice.
if (args.Contains(AdministradorInicial.Opcao, StringComparer.Ordinal))
    return await AdministradorInicial.RodarAsync(args);

// --orquestrar — O ORQUESTRADOR (issue 136, Orquestrador.cs): a tarefa do Windows que, a cada cinco minutos, roda as
// rotinas vencidas pela agenda do banco e testa as APIs monitoradas. Não é carga: roda as cargas como processos filhos.
if (args.Contains(Orquestrador.Opcao, StringComparer.Ordinal))
    return await Orquestrador.RodarAsync(args);

// =================================================================================================
// A CARGA DE DADOS DO SISTEMA LEGADO — projeto de console, e não endpoint da API.
//
// POR QUE CONSOLE. Três razões, e nenhuma é gosto:
//
//   1. A leitura do recorte varre milhões de linhas de agenda e histórico e leva minutos. Isso não
//      cabe numa requisição HTTP: o tempo limite do servidor web, do balanceador e do navegador
//      são todos menores, e a carga morreria no meio sem ninguém saber onde parou.
//
//   2. A carga precisa de alcance de organização — ela grava em treze filiais. Pôr esse poder atrás
//      de uma rota HTTP significaria que existe uma URL capaz de atravessar a fronteira de
//      multiempresa. Não deve existir. Aqui o poder mora num processo com escopo, horário e
//      operador conhecidos.
//
//   3. A carga é o ENSAIO da migração, que vai rodar por agendamento e não por clique. Escrevê-la
//      já no formato em que ela vai viver evita reescrevê-la depois.
//
// A API não perde nada com isso: quem quiser disparar a carga por lá, um dia, chama este mesmo
// executável — o contrário (extrair um processo de dentro de um endpoint) é que não sai de graça.
// =================================================================================================

var ano = LerInteiro(args, "--ano") ?? 2026;
var limite = LerInteiro(args, "--limite");
var recomecar = args.Contains("--recomecar", StringComparer.Ordinal);
var somenteMedir = args.Contains("--somente-medir", StringComparer.Ordinal);

// A CARGA TEM DUAS ETAPAS E ELAS SAO SEPARAVEIS.
//
//   cadastro       - cliente, endereco, contato e parque de maquinas (documento 24)
//   relacionamento - usuario, carteira, processo, tarefa e interacao (documento 25)
//
// A segunda depende do de-para da primeira e nao roda sozinha num banco vazio; separa-las por
// bandeira existe para o ensaio, para reexecutar so a metade que mudou, e para o dia em que a
// migracao completa rodar as duas em janelas diferentes.
var somenteCadastro = args.Contains("--somente-cadastro", StringComparer.Ordinal);
var somenteRelacionamento = args.Contains("--somente-relacionamento", StringComparer.Ordinal);

// --somente-faturamento — A ETAPA QUE NAO PRECISA DO VORTICE.
//
// O faturamento vem da SD2 do Protheus e o cliente vem do nosso proprio cadastro: o sistema de
// origem nao entra em lugar nenhum dessa etapa. Reler o Vortice inteiro para atualizar o
// faturamento custa horas e nao muda nada do que o Vortice traz — e o faturamento e justamente o
// dado que muda TODO DIA, porque ha nota emitida hoje.
//
// E o que permite atualizar o numero da diretoria sem uma janela de migracao.
var somenteFaturamento = args.Contains("--somente-faturamento", StringComparer.Ordinal);

// --somente-territorio — O MUNICIPIO OFICIAL, A ADR, OS RESPONSAVEIS E A AREA PLANTADA (documento 32).
//
// Tambem nao le o Vortice nem o Protheus: le as duas planilhas do comercial e as APIs publicas do
// IBGE. As planilhas trazem nome de funcionario e ficam FORA do repositorio; o caminho vem na linha
// de comando:
//
//   --somente-territorio --area-de-atuacao "<pasta>\Area de Atuacao.xlsx"
//                        --cen-e-gestor    "<pasta>\CEN e Gestor por Municipio.xlsx"
var somenteTerritorio = args.Contains("--somente-territorio", StringComparer.Ordinal);

// --somente-pam — SO A PRODUCAO AGRICOLA MUNICIPAL DO IBGE (issue 64).
//
// E a ETAPA 4 do territorio, sozinha: as quatro medidas da tabela SIDRA 5457 (area plantada,
// colhida, quantidade e valor), por municipio de SP e no total do estado, nos ultimos tres anos
// publicados.
//
// POR QUE SEPARADO DO --somente-territorio: a PAM muda uma vez por ano e nao precisa de planilha
// nenhuma. E a unica etapa do territorio que o SERVIDOR consegue rodar sozinho (regra R-5 do doc 46),
// porque as outras tres dependem das duas planilhas do comercial, que trazem nome de funcionario e
// nao devem viajar ate la so para atualizar o IBGE.
//
// Ele conta com o catalogo de municipios ja reconhecido, que o --somente-territorio faz e nao muda
// de ano para ano. Uma TRAVA no proprio banco impede que a rotina agendada e uma carga manual
// escrevam ao mesmo tempo.
var somentePam = args.Contains("--somente-pam", StringComparer.Ordinal);

// --somente-estrutura — O QUE JA EXISTE NO TERRITORIO PARA MECANIZAR (issue 65).
//
// Cinco fontes publicas, de quatro pesquisas e duas agencias: tratores por potencia e
// estabelecimentos por tamanho (Censo Agropecuario), rebanho bovino (Pesquisa da Pecuaria Municipal),
// area territorial (Censo Demografico) e as usinas de etanol autorizadas (dados abertos da ANP).
//
// A PAM diz quanto se PLANTA; esta carga diz o que ha instalado. Nenhuma das cinco usa planilha, e
// todas podem rodar no servidor.
var somenteEstrutura = args.Contains("--somente-estrutura", StringComparer.Ordinal);

// --somente-precos — A BASE DE PRECOS QUE SO CRESCE (issue 66).
//
// Tres fontes abertas: o preco recebido pelo produtor em SP (CONAB), o preco do kg de ATR da cana
// (Socicana) e o dolar PTAX mensal (Banco Central). Nenhuma usa planilha, nenhuma precisa do catalogo
// de municipios, e todas rodam no servidor. E a rotina MENSAL — as outras cargas publicas sao anuais.
var somentePrecos = args.Contains("--somente-precos", StringComparer.Ordinal);

// --somente-custos — O CUSTO DE PRODUCAO DAS CULTURAS EM SP (issue 67).
//
// As series historicas da CONAB, em .xls, uma por cultura: cafe arabica, cana, soja, milho, amendoim
// e laranja. So as abas de locais de Sao Paulo. Roda na rotina mensal junto com os precos: a CONAB
// revisa as planilhas ao longo do ano.
var somenteCustos = args.Contains("--somente-custos", StringComparer.Ordinal);

// --somente-credito — O CREDITO RURAL DE INVESTIMENTO DO SICOR, POR MUNICIPIO E MES (issue 68).
//
// Dados abertos do Banco Central: Sao Paulo inteiro, todos os produtos, desde 2013, mais as tabelas
// auxiliares (programa, subprograma, fonte e produto). Relê sempre os dois ultimos anos, onde o Banco
// Central acrescenta contrato registrado com atraso. Roda na rotina mensal.
var somenteCredito = args.Contains("--somente-credito", StringComparer.Ordinal);

// --somente-art [--simular] — AS VENDAS DE MÁQUINA DO ART (documento 35, seção 10).
//
// Lê a view do ART (MySQL, sessão somente leitura) e confere dono e cadastro no banco do Protheus
// (só SELECT). Não lê o Vórtice. Com --simular, a carga inteira roda numa transação DESFEITA no fim:
// os números saem, e o banco não muda.
var somenteArt = args.Contains("--somente-art", StringComparer.Ordinal);

// --somente-clientes-protheus — O CADASTRO DE CLIENTES, DA SA1 (decisão de 24/09/2026).
//
// Le a SA1010 do Protheus (so SELECT) e cria cliente e endereco principal. NAO le o Vortice: a
// carga de cadastro do legado foi aposentada, e a SA1 e a fonte decidida.
//
// Ela vem ANTES das vendas na ordem de carga, e isso nao e preferencia: sem cliente, o ART nao
// consegue casar comprador nenhum e as vendas ficam todas pendentes.
var somenteClientesDoProtheus = args.Contains("--somente-clientes-protheus", StringComparer.Ordinal);

// --somente-carteiras-vortice [--simular] — AS CARTEIRAS MAQ_NOVOS DO VÓRTICE (decisão de 24/09/2026).
//
// Le do Vortice (so SELECT, NOLOCK) as carteiras do departamento MAQ-NOVOS e os clientes delas, casa cada cliente
// com o cadastro do CRM pelo CPF/CNPJ e SINCRONIZA: vinculo novo entra, vinculo que sumiu e encerrado. E a rotina
// diaria CARTEIRAS_VORTICE do orquestrador. Com --simular, calcula o plano so com leitura e nao abre transacao.
//
// Vem DEPOIS do --somente-clientes-protheus na ordem de carga: o cliente que ainda nao entrou pela SA1 fica
// pendente, e entra na rodada seguinte.
var somenteCarteirasDoVortice = args.Contains("--somente-carteiras-vortice", StringComparer.Ordinal);

var simular = args.Contains("--simular", StringComparer.Ordinal);

// =================================================================================================
// O VÓRTICE ESTÁ CONGELADO — LEGADO / SOMENTE REFERÊNCIA (decisão D-12, documento 41).
//
// A partir da fase 1 da reestruturação, a leitura do sistema de origem NÃO volta ao fluxo
// operacional. O código não foi removido de propósito: ele é a documentação executável de como o
// dado do Vórtice foi interpretado, e a fase 8 é que decide o destino dele. Mas ele deixa de ser
// uma opção de rotina, e quem quiser rodá-lo precisa dizer isso em voz alta na linha de comando.
//
// O QUE CONTINUA LIVRE, porque nada disso lê o Vórtice:
//   --somente-faturamento   o faturamento do Protheus, que muda todo dia e é o número da diretoria;
//   --somente-territorio    as planilhas do comercial e o IBGE;
//   --somente-pam           só a produção agrícola do IBGE — a rotina anual do servidor;
//   --somente-estrutura     o Censo, o rebanho, a área territorial e as usinas da ANP;
//   --somente-precos        os preços da CONAB e da Socicana e o dólar PTAX — a rotina mensal;
//   --somente-custos        o custo de produção das culturas em SP, das séries da CONAB;
//   --somente-credito       o crédito rural de investimento do SICOR, por município e mês;
//   --somente-art           as vendas de máquina do ART;
//   --somente-medir         só conta linhas, não grava nada.
//
// A ÚNICA LEITURA DO VÓRTICE LIBERADA (decisão de 24/09/2026):
//   --somente-carteiras-vortice   as carteiras MAQ_NOVOS — o Vórtice continua sendo onde o comercial edita a
//                                 carteira de cada vendedor, e ela não mora em nenhum outro lugar.
//
// O QUE PEDE A DECLARAÇÃO: a carga completa, --somente-cadastro e --somente-relacionamento.
const string DeclaracaoDeUsoDoLegado = "--legado-somente-referencia-eu-sei-o-que-estou-fazendo";

var leOVortice = !somenteFaturamento && !somenteTerritorio && !somentePam && !somenteEstrutura && !somentePrecos && !somenteCustos && !somenteCredito
                 && !somenteArt && !somenteClientesDoProtheus && !somenteCarteirasDoVortice && !somenteMedir;

if (leOVortice && !args.Contains(DeclaracaoDeUsoDoLegado, StringComparer.Ordinal))
{
    Console.Error.WriteLine(
        "A carga do Vórtice está CONGELADA: LEGADO / SOMENTE REFERÊNCIA (decisão D-12, " +
        "documento 41). Ela não é fonte de dado novo, e rodá-la por engano recolocaria o " +
        "sistema de origem no fluxo operacional. Nada foi lido e nada foi gravado.");
    Console.Error.WriteLine();
    Console.Error.WriteLine(
        "  O que continua valendo sem declaração nenhuma: --somente-faturamento (Protheus), " +
        "--somente-territorio (planilhas e IBGE), --somente-pam (só o IBGE), --somente-estrutura, --somente-precos, --somente-custos e --somente-credito (fontes públicas), --somente-art e " +
        "--somente-medir.");
    Console.Error.WriteLine();
    Console.Error.WriteLine(
        $"  Se a leitura do legado for MESMO o que se quer, acrescente {DeclaracaoDeUsoDoLegado} " +
        "— e registre por quê.");
    return 2;
}

var configuracao = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var conexaoDoCrm = configuracao.GetConnectionString("Crm");

if (string.IsNullOrWhiteSpace(conexaoDoCrm))
{
    Console.Error.WriteLine(
        "Não achei a cadeia de conexão do CRM. Defina ConnectionStrings__Crm ou rode a partir " +
        "de uma pasta com o appsettings.json ao lado.");
    return 2;
}

// A CREDENCIAL DA TELA (issue 136) vai por cima das variáveis de ambiente, com os mesmos nomes: nenhum caminho da carga
// muda, e sem credencial na tela tudo segue como antes. O aviso diz qual conexão não abriu — nunca o valor.
var (credenciaisDaTela, avisosDasCredenciais) = await CredenciaisDaTela.LerAsync(conexaoDoCrm, configuracao, CancellationToken.None);
foreach (var aviso in avisosDasCredenciais) Console.Error.WriteLine("Credencial da tela não usada — " + aviso);
if (credenciaisDaTela.Count > 0) configuracao = CredenciaisDaTela.Sobrepor(configuracao, credenciaisDaTela);

var conexaoDoLegado = configuracao["Vortice:Conexao"];

// A EXIGÊNCIA CAI NO MODO SÓ-FATURAMENTO, e só nele: essa etapa não abre conexão com o legado.
// A cadeia continua sendo passada adiante como veio (possivelmente vazia) — se algum caminho
// tentar usá-la neste modo, a falha é imediata e ruidosa, que é o comportamento desejado.
if (string.IsNullOrWhiteSpace(conexaoDoLegado) && !somenteFaturamento && !somenteTerritorio && !somentePam && !somenteEstrutura
    && !somentePrecos && !somenteCustos && !somenteCredito && !somenteArt && !somenteClientesDoProtheus && !somenteCarteirasDoVortice)
{
    Console.Error.WriteLine(
        "A leitura do sistema legado exige a variável de ambiente Vortice__Conexao, que NUNCA " +
        "mora em arquivo do repositório. Sem ela a carga não roda — e nada foi gravado. " +
        "Para atualizar só o faturamento, que não usa o legado, use --somente-faturamento.");
    return 2;
}

using var fabricaDeLog = LoggerFactory.Create(construtor => construtor
    .SetMinimumLevel(LogLevel.Warning)
    .AddSimpleConsole(opcoes => opcoes.SingleLine = true));

var opcoesDoBanco = new DbContextOptionsBuilder<CrmDbContext>()
    .UseSqlServer(conexaoDoCrm, sql => sql.CommandTimeout(180))
    .Options;

var diario = new DiarioDeAlcanceEntreEmpresasEmLog(
    fabricaDeLog.CreateLogger<DiarioDeAlcanceEntreEmpresasEmLog>());

// -------------------------------------------------------------------------------------------------
// O de-para de filiais e quem responde pelos registros criados.
// -------------------------------------------------------------------------------------------------

var (deParaDeFiliais, usuarioId, empresaDeCasaId, erro) =
    await PreparacaoDaCarga.PrepararAsync(opcoesDoBanco, diario);

if (erro is not null)
{
    Console.Error.WriteLine(erro);
    return 2;
}

var contexto = new ContextoDeCargaDeSistema(
    usuarioId, empresaDeCasaId, deParaDeFiliais.Values.ToHashSet());

CrmDbContext AbrirContexto() => new(opcoesDoBanco, contexto, diario);

var leitor = new LeitorDeCargaDoVortice(
    Options.Create(new OpcoesDoVortice { Conexao = conexaoDoLegado, TempoLimiteSegundos = 15 }),
    fabricaDeLog.CreateLogger<LeitorDeCargaDoVortice>());

// O IBGE RESPONDE EM GZIP mesmo sem o pedido; sem a descompressão o JSON chega ilegível. O cliente é
// um só para as duas cargas que o usam: a do território e a conferência das grafias cortadas que a
// carga do cadastro faz ao fim (documento 32, seção 4.6).
using var clienteDoIbge = new HttpClient(
    new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
{
    Timeout = TimeSpan.FromMinutes(5)
};

var ibge = new LeitorDoIbge(clienteDoIbge);

// A ANP TEM CLIENTE PRÓPRIO, SEM DESCOMPRESSÃO — e isso não é detalhe de estilo.
//
// O que ela entrega é um ZIP: um arquivo JÁ COMPRIMIDO, que não ganha nada com compressão de
// transporte. E pedi-la custou caro: com `DecompressionMethods.All`, que inclui brotli, o download
// falhava NO SERVIDOR com "The SSL connection could not be established — Received an unexpected EOF
// or 0 bytes from the transport stream". Na estação funcionava, e por isso a issue 65 passou nos
// testes e quebrou só lá.
//
// MEDIDO em 20/09/2026, de dentro do servidor, no mesmo endereço: HttpClient simples baixou os
// 946 KB; com GZip+Deflate, também; com `All`, caiu. O `www.gov.br` fecha a conexão diante do
// `Accept-Encoding` com brotli.
using var clienteDaAnp = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

// -------------------------------------------------------------------------------------------------
// Atalho — só o território. Sai antes da exigência do Protheus, que esta etapa não usa.
// -------------------------------------------------------------------------------------------------

if (somenteTerritorio || somentePam || somenteEstrutura || somentePrecos || somenteCustos || somenteCredito)
{
    var caminhoDaAreaDeAtuacao = LerTexto(args, "--area-de-atuacao");
    var caminhoDoCenEGestor = LerTexto(args, "--cen-e-gestor");

    if (somenteTerritorio && (caminhoDaAreaDeAtuacao is null || caminhoDoCenEGestor is null))
    {
        Console.Error.WriteLine(
            "A carga do território precisa das duas planilhas: --area-de-atuacao \"<arquivo>\" e " +
            "--cen-e-gestor \"<arquivo>\". Nada foi gravado.");
        Console.Error.WriteLine(
            "  Para atualizar SÓ a produção agrícola do IBGE, que não usa planilha nenhuma, " +
            "use --somente-pam; para o Censo, o rebanho, a área e as usinas, --somente-estrutura.");
        return 2;
    }

    var cargaDoTerritorio = new CargaDeTerritorio(
        AbrirContexto, ibge, usuarioId, Console.WriteLine);

    var cargaDaEstrutura = new CargaDaEstruturaAgropecuaria(
        AbrirContexto,
        new LeitorDaEstruturaAgropecuaria(clienteDoIbge),
        new LeitorDaAnp(clienteDaAnp),
        usuarioId,
        Console.WriteLine);

    // OS PRECOS USAM O CLIENTE SEM DESCOMPRESSAO, o mesmo da ANP: a CONAB tambem fica atras de um
    // dominio do governo, e o defeito do brotli no `www.gov.br` (ver acima) nao precisa ser
    // redescoberto no servidor.
    var cargaDePrecos = new CargaDePrecos(
        AbrirContexto,
        new LeitorDaConab(clienteDaAnp),
        new LeitorDaSocicana(clienteDaAnp),
        new LeitorDoPtax(clienteDaAnp),
        usuarioId,
        Console.WriteLine);

    var oQueRodou = somenteTerritorio ? "A CARGA DO TERRITÓRIO"
        : somentePam ? "A CARGA DA PRODUÇÃO AGRÍCOLA"
        : somentePrecos ? "A CARGA DOS PREÇOS DE MERCADO"
        : somenteCustos ? "A CARGA DOS CUSTOS DE PRODUÇÃO"
        : somenteCredito ? "A CARGA DO CRÉDITO RURAL"
        : "A CARGA DA ESTRUTURA AGROPECUÁRIA";

    try
    {
        var contagens = somenteTerritorio
            ? await cargaDoTerritorio.ExecutarAsync(
                caminhoDaAreaDeAtuacao!, caminhoDoCenEGestor!, CancellationToken.None)
            : somentePam
                ? await cargaDoTerritorio.ExecutarSoAProducaoAgricolaAsync(CancellationToken.None)
                : somentePrecos
                    ? await cargaDePrecos.ExecutarAsync(CancellationToken.None)
                    : somenteCustos
                        ? await new CargaDeCustosDeProducao(AbrirContexto, new LeitorDeCustosDaConab(clienteDaAnp), usuarioId, Console.WriteLine).ExecutarAsync(CancellationToken.None)
                        : somenteCredito
                            ? await new CargaDoCreditoRural(AbrirContexto, new LeitorDoSicor(clienteDaAnp), usuarioId, Console.WriteLine).ExecutarAsync(CancellationToken.None)
                            : await cargaDaEstrutura.ExecutarAsync(CancellationToken.None);

        foreach (var etapa in contagens.GroupBy(c => c.Etapa))
        {
            Console.WriteLine();
            Console.WriteLine($"- {etapa.Key} -");
            foreach (var (_, rotulo, valor) in etapa)
                Console.WriteLine($"  {valor,9}  {rotulo}");
        }

        return 0;
    }
    catch (Exception falha) when (falha is FileNotFoundException or InvalidDataException
                                      or HttpRequestException or TaskCanceledException
                                      or DbUpdateException or RegraDeNegocioViolada
                                      or InvalidOperationException)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"{oQueRodou} PAROU, e a etapa em curso foi desfeita: " + falha.Message);

        // A CAUSA DE VERDADE mora na exceção mais interna: "erro ao salvar as alterações" não diz
        // qual restrição recusou nem por quê.
        if (falha.GetBaseException() is { } causa && !ReferenceEquals(causa, falha))
            Console.Error.WriteLine("  causa: " + causa.Message);
        return 3;
    }
}

// -------------------------------------------------------------------------------------------------
// Atalho — só o ART. Sai antes da exigência da API REST do Protheus, que esta etapa não usa.
// -------------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------------
// Atalho — só o cadastro de clientes, da SA1 do Protheus (decisão de 24/09/2026).
// -------------------------------------------------------------------------------------------------

if (somenteClientesDoProtheus)
{
    var opcoesDoBancoParaClientes = new OpcoesDoBancoDoProtheus();
    configuracao.GetSection(OpcoesDoBancoDoProtheus.Secao).Bind(opcoesDoBancoParaClientes);

    if (!opcoesDoBancoParaClientes.EstaConfigurada)
    {
        Console.Error.WriteLine(
            "A carga de clientes exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha. Nada foi gravado.");
        return 2;
    }

    Console.WriteLine(simular
        ? "Carga de clientes (SA1 do Protheus) — SIMULAÇÃO: tudo roda numa transação desfeita no fim."
        : "Carga de clientes (SA1 do Protheus) — gravando.");
    Console.WriteLine();

    var cargaDeClientes = new CargaDeClientesDoProtheus(
        AbrirContexto, new LeitorDeClientesDoProtheus(opcoesDoBancoParaClientes), usuarioId, Console.WriteLine);

    var resultadoDosClientes = await cargaDeClientes.ExecutarAsync(simular, CancellationToken.None);
    if (!resultadoDosClientes.EhSucesso)
    {
        Console.Error.WriteLine("A CARGA DE CLIENTES PAROU: " + resultadoDosClientes.Erro);
        return 3;
    }

    foreach (var etapa in resultadoDosClientes.Valor.Contagens.GroupBy(c => c.Etapa))
    {
        Console.WriteLine();
        Console.WriteLine($"- {etapa.Key} -");
        foreach (var (_, rotulo, valor) in etapa)
            Console.WriteLine($"  {valor,7:N0}  {rotulo}");
    }

    Console.WriteLine();
    return 0;
}

// -------------------------------------------------------------------------------------------------
// Atalho — só as carteiras MAQ_NOVOS do Vórtice (decisão de 24/09/2026).
// -------------------------------------------------------------------------------------------------

if (somenteCarteirasDoVortice)
{
    if (string.IsNullOrWhiteSpace(conexaoDoLegado))
    {
        Console.Error.WriteLine(
            "A sincronia das carteiras exige a credencial do Vórtice: grave-a e teste-a em Configurações › Integrações, " +
            "ou defina Vortice__Conexao no servidor. Nada foi lido e nada foi gravado.");
        return 2;
    }

    // O BANCO DO PROTHEUS É OPCIONAL: com ele, o vínculo cujo cliente não está no CRM diz POR QUÊ (fora da área de
    // atuação, fora da SA1, ainda não carregado); sem ele, fica com o motivo genérico.
    var opcoesDoBancoParaCarteiras = new OpcoesDoBancoDoProtheus();
    configuracao.GetSection(OpcoesDoBancoDoProtheus.Secao).Bind(opcoesDoBancoParaCarteiras);
    var leitorDaSa1 = opcoesDoBancoParaCarteiras.EstaConfigurada ? new LeitorDeClientesDoProtheus(opcoesDoBancoParaCarteiras) : null;

    // A SIMULAÇÃO LÊ O CRM COM INTENÇÃO DE LEITURA DECLARADA: ela não grava nada por construção (o plano é calculado
    // só com consultas), e a cadeia diz isso ao servidor também.
    var opcoesDaSincronia = simular
        ? new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(
                new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(conexaoDoCrm)
                {
                    ApplicationIntent = Microsoft.Data.SqlClient.ApplicationIntent.ReadOnly
                }.ConnectionString,
                sql => sql.CommandTimeout(180))
            .Options
        : opcoesDoBanco;

    CrmDbContext AbrirContextoDaSincronia() => new(opcoesDaSincronia, contexto, diario);

    Console.WriteLine(simular
        ? "Carteiras MAQ_NOVOS do Vórtice — SIMULAÇÃO: o plano é calculado só com leitura; nenhuma transação é aberta."
        : "Carteiras MAQ_NOVOS do Vórtice — sincronizando.");
    if (leitorDaSa1 is null)
        Console.WriteLine("  (sem o banco do Protheus configurado: o motivo de quem não casa fica genérico)");
    Console.WriteLine();

    var leitorDasCarteiras = new LeitorDeCarteirasDoVortice(new OpcoesDoVortice { Conexao = conexaoDoLegado });
    var cargaDeCarteiras = new CargaDeCarteirasDoVortice(
        AbrirContextoDaSincronia, leitorDasCarteiras.LerAsync, leitorDaSa1 is null ? null : leitorDaSa1.LerAsync,
        deParaDeFiliais, usuarioId, () => DateTime.UtcNow, Console.WriteLine);

    try
    {
        // A MESMA TRAVA DAS OUTRAS CARGAS, e só quando grava: a rotina do orquestrador e uma rodada manual não
        // sincronizam ao mesmo tempo. A simulação não a toma — ela não escreve, e não deve impedir quem escreve.
        await using var travaDasCarteiras = simular
            ? null
            : await TravaDeFluxo.TomarAsync(AbrirContexto(), CargaDeCarteirasDoVortice.Fluxo, CancellationToken.None);

        var resultadoDasCarteiras = await cargaDeCarteiras.ExecutarAsync(simular, CancellationToken.None);
        if (!resultadoDasCarteiras.EhSucesso)
        {
            Console.Error.WriteLine("A SINCRONIA DAS CARTEIRAS PAROU: " + resultadoDasCarteiras.Erro);
            return 3;
        }

        foreach (var etapa in resultadoDasCarteiras.Valor.Contagens.GroupBy(c => c.Etapa))
        {
            Console.WriteLine();
            Console.WriteLine($"- {etapa.Key} -");
            foreach (var (_, rotulo, valor) in etapa)
                Console.WriteLine($"  {valor,7:N0}  {rotulo}");
        }

        if (resultadoDasCarteiras.Valor.Observacoes.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("- Observações -");
            foreach (var observacao in resultadoDasCarteiras.Valor.Observacoes) Console.WriteLine("  " + observacao);
        }

        Console.WriteLine();
        return 0;
    }
    catch (Exception falha) when (falha is DbUpdateException or RegraDeNegocioViolada or InvalidOperationException)
    {
        Console.Error.WriteLine("A SINCRONIA DAS CARTEIRAS PAROU, e a transação foi desfeita: " + falha.Message);
        if (falha.GetBaseException() is { } causa && !ReferenceEquals(causa, falha))
            Console.Error.WriteLine("  causa: " + causa.Message);
        return 3;
    }
}

if (somenteArt)
{
    var opcoesDoArt = new OpcoesDoArt();
    configuracao.GetSection(OpcoesDoArt.Secao).Bind(opcoesDoArt);

    var opcoesDoBancoDoProtheus = new OpcoesDoBancoDoProtheus();
    configuracao.GetSection(OpcoesDoBancoDoProtheus.Secao).Bind(opcoesDoBancoDoProtheus);

    if (!opcoesDoArt.EstaConfigurada)
    {
        Console.Error.WriteLine(
            "A carga do ART exige Art__Servidor, Art__Banco, Art__Usuario, Art__Senha e Art__Visao — use " +
            "scripts/integracao/rodar-carga-do-art.ps1, que as carrega sem imprimir. Nada foi gravado.");
        return 2;
    }

    Console.WriteLine(simular
        ? "Carga do ART — SIMULAÇÃO: tudo roda numa transação desfeita no fim."
        : "Carga do ART — gravando.");
    Console.WriteLine();

    // A MESMA TRAVA E O MESMO REGISTRO DO SERVIÇO (documento 35, seção 11): se o serviço estiver no meio
    // de um ciclo, a carga manual é recusada em vez de gravar em paralelo, e a carga real fica registrada
    // em integracao.ExecucaoDeSincronizacao. Uma tentativa só — há alguém olhando o terminal.
    var executorDoArt = new ExecutorDaSincronizacaoDoArt(
        conexaoDoCrm, AbrirContexto, opcoesDoArt, opcoesDoBancoDoProtheus, usuarioId,
        tentativas: 1, esperaBase: TimeSpan.Zero, Console.WriteLine,
        fabricaDeLog.CreateLogger<ExecutorDaSincronizacaoDoArt>());

    try
    {
        var desfecho = await executorDoArt.ExecutarAsync(simular, CancellationToken.None);
        if (desfecho.Relatorio is not { } relatorio)
        {
            Console.Error.WriteLine("A CARGA DO ART PAROU: " + desfecho.Mensagem);
            return 3;
        }

        foreach (var etapa in relatorio.Contagens.GroupBy(c => c.Etapa))
        {
            Console.WriteLine();
            Console.WriteLine($"- {etapa.Key} -");
            foreach (var (_, rotulo, valor) in etapa)
                Console.WriteLine($"  {valor,7}  {rotulo}");
        }

        Console.WriteLine();
        Console.WriteLine("- Unidades do ART → filiais do CRM -");
        foreach (var (unidade, filial, situacao, ocorrencias, criterio) in relatorio.Unidades)
            Console.WriteLine($"  {unidade,-24} {ocorrencias,5} vendas → {filial,-18} {situacao,-22} {criterio}");

        if (relatorio.Observacoes.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("- Observações -");
            foreach (var observacao in relatorio.Observacoes) Console.WriteLine("  " + observacao);
        }

        return 0;
    }
    catch (Exception falha) when (falha is DbUpdateException or RegraDeNegocioViolada or InvalidOperationException)
    {
        Console.Error.WriteLine("A CARGA DO ART PAROU, e a transação foi desfeita: " + falha.Message);
        if (falha.GetBaseException() is { } causa && !ReferenceEquals(causa, falha))
            Console.Error.WriteLine("  causa: " + causa.Message);
        return 3;
    }
}

// A PONTE DO PROTHEUS. Ela lê o faturamento da ORIGEM, e não da cópia no Vórtice que parou em
// 11/04/2025. As credenciais vêm de Protheus__Base, Protheus__Usuario e Protheus__Senha — nunca
// de arquivo versionado. Na estação, os valores ficam no .env da raiz (TOTVS_API_*, ver
// .env.exemplo); o publicar.ps1 ainda não os repassa ao servidor (issue [018a]).
var opcoesDoProtheus = new OpcoesDoProtheus
{
    Base = configuracao["Protheus:Base"],
    Usuario = configuracao["Protheus:Usuario"],
    Senha = configuracao["Protheus:Senha"]
};

// O CADASTRO NÃO LÊ O PROTHEUS. Exigir a credencial dele para rodar só o cadastro impediria justamente
// a recarga que confere os endereços (documento 32, seção 4.6) numa estação sem o Protheus configurado.
if (!opcoesDoProtheus.EstaConfigurada && !somenteCadastro)
{
    Console.Error.WriteLine(
        "A carga do faturamento exige as variáveis Protheus__Base, Protheus__Usuario e " +
        "Protheus__Senha. Sem elas o faturamento não entra — e sem faturamento a curva ABC não " +
        "tem base, então a classe A/B/C/D do cliente não é apurada. " +
        "Ver docs/projeto/28-PROTHEUS-ACESSO-E-TABELAS.md.");
    return 2;
}

var colecao = new ServiceCollection();
colecao.AddHttpClient(PonteDoProtheus.NomeDoCliente);
var provedorDeHttp = colecao.BuildServiceProvider();

var faturamentoDoProtheus = new LeitorDeFaturamentoDoProtheus(
    new PonteDoProtheus(
        provedorDeHttp.GetRequiredService<IHttpClientFactory>(),
        Options.Create(opcoesDoProtheus)));

// -------------------------------------------------------------------------------------------------
// Atalho — só o faturamento. Sai antes de medir o recorte, que é uma consulta ao legado.
// -------------------------------------------------------------------------------------------------

if (somenteFaturamento)
{
    Console.WriteLine("Atualizando SOMENTE o faturamento (Protheus). O sistema legado não é lido.");
    Console.WriteLine();

    var soFaturamento = new CargaDeProcessoDoVortice(
        AbrirContexto, leitor, faturamentoDoProtheus, deParaDeFiliais, usuarioId, Console.WriteLine);

    var resultadoDoFaturamento = await soFaturamento.ExecutarSomenteFaturamentoAsync(
        CancellationToken.None);

    if (!resultadoDoFaturamento.EhSucesso)
    {
        Console.Error.WriteLine(resultadoDoFaturamento.Erro);
        return 3;
    }

    Console.WriteLine();
    Console.WriteLine("Decisões da carga:");
    foreach (var (decisao, quantas) in resultadoDoFaturamento.Valor.Decisoes.OrderBy(p => p.Key))
        Console.WriteLine($"  {quantas,8}  {decisao}");

    return 0;
}

var recorte = new RecorteDaCarga(ano, deParaDeFiliais.Keys.Order().ToList(), limite);

Console.WriteLine($"Carga do sistema legado — recorte: atividade em {ano}, " +
                  $"{deParaDeFiliais.Count} filiais em operação" +
                  (limite is null ? "." : $", teto de {limite} clientes."));
Console.WriteLine();

// -------------------------------------------------------------------------------------------------
// Passo 1 — MEDIR ANTES DE CARREGAR.
// -------------------------------------------------------------------------------------------------

Console.WriteLine("Medindo o recorte na origem…");
var contagem = await leitor.ContarPorFilialAsync(recorte, CancellationToken.None);

if (!contagem.EhSucesso)
{
    Console.Error.WriteLine(contagem.Erro);
    return 3;
}

Console.WriteLine($"{"filial",-8} {"código CRM",-12} {"pessoas",8}");
foreach (var (codigo, pessoas) in contagem.Valor.OrderByDescending(p => p.Value))
    Console.WriteLine($"{codigo,-8} {CodigoDoCrm(codigo),-12} {pessoas,8}");

Console.WriteLine($"{"TOTAL",-8} {string.Empty,-12} {contagem.Valor.Values.Sum(),8}");
Console.WriteLine();

if (somenteMedir) return 0;

// -------------------------------------------------------------------------------------------------
// Passo 2 — a carga.
// -------------------------------------------------------------------------------------------------

if (recomecar)
{
    Console.WriteLine("Limpando o cadastro antes de recomeçar…");
    await using var limpeza = AbrirContexto();

    // A ORDEM É A DAS CHAVES ESTRANGEIRAS, e nenhuma delas tem exclusão em cascata — de
    // propósito: apagar em cascata é como se perde histórico sem ninguém decidir perdê-lo.
    // TAREFA E INTERACAO APONTAM UMA PARA A OUTRA - e o duplo ponteiro que o sistema de origem
    // acertou e que nos copiamos. Um ciclo de chave estrangeira nao se apaga por ordenacao: os
    // ponteiros sao soltos primeiro, e so entao as duas tabelas saem.
    //
    // O USUARIO SO SAI SE VEIO DA CARGA. Os usuarios de desenvolvimento do seed continuam, e um
    // deles e quem esta rodando este comando - apaga-lo tiraria o chao da propria execucao.
    foreach (var comando in new[]
             {
                 "UPDATE processo.Tarefa SET InteracaoConclusaoId = NULL, InteracaoOrigemId = NULL",
                 // processo.InteracaoParticipante, processo.PassagemDeFase e processo.ItemDeProposta
                 // saíram do banco na fase 1 (documento 41): nunca tiveram linha e não havia carga
                 // que as preenchesse, então não há o que limpar antes de recomeçar.
                 "DELETE FROM processo.Interacao",
                 "DELETE FROM processo.Tarefa",
                 "DELETE FROM processo.Processo",
                 "DELETE FROM comercial.ClienteCarteira",
                 "DELETE FROM organizacao.CarteiraMunicipio",
                 "DELETE FROM organizacao.Carteira",
                 "DELETE FROM processo.Resultado",
                 "DELETE FROM processo.Fase",
                 "DELETE FROM processo.TipoTarefa",
                 "DELETE FROM processo.TipoProcesso",
                 "DELETE FROM processo.MotivoDePerda",
                 "DELETE FROM organizacao.LinhaDeNegocio",
                 "DELETE FROM comercial.ClienteContato",
                 "DELETE FROM frota.Equipamento",
                 "DELETE FROM comercial.Endereco",
                 "DELETE FROM organizacao.Municipio",
                 "DELETE FROM comercial.Contato",
                 "DELETE FROM comercial.Cliente",
                 "DELETE FROM seguranca.Usuario WHERE Id IN " +
                 "(SELECT RegistroId FROM integracao.ChaveExterna WHERE Entidade = 'Usuario')",
                 "DELETE FROM integracao.ChaveExterna",
                 "DELETE FROM integracao.MensagemDescartada",
                 "DELETE FROM integracao.PontoDeSincronismo"
             })
        await limpeza.Database.ExecuteSqlRawAsync(comando);

    Console.WriteLine("  cadastro vazio. O que entrar agora veio todo do sistema legado.");
    Console.WriteLine();
}

var cronometro = Stopwatch.StartNew();

ResumoDaCarga? resumoDoCadastro = null;

if (!somenteRelacionamento)
{
    var carga = new CargaDoVortice(
        AbrirContexto, leitor, deParaDeFiliais, usuarioId, Console.WriteLine,
        new ConsolidacaoDeGrafiasCortadas(AbrirContexto, ibge.LerMalhaMunicipalAsync, usuarioId));

    var resultado = await carga.ExecutarAsync(recorte, CancellationToken.None);

    if (!resultado.EhSucesso)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("A CARGA PAROU: " + resultado.Erro);
        return 3;
    }

    resumoDoCadastro = resultado.Valor;
}

ResumoDoRelacionamento? resumoDoRelacionamento = null;

if (!somenteCadastro)
{
    Console.WriteLine();
    Console.WriteLine("Carga do relacionamento - usuario, carteira, processo, tarefa e interacao.");

    var cargaDeRelacionamento = new CargaDeProcessoDoVortice(
        AbrirContexto, leitor, faturamentoDoProtheus, deParaDeFiliais, usuarioId,
        Console.WriteLine);

    var resultadoDoRelacionamento =
        await cargaDeRelacionamento.ExecutarAsync(recorte, CancellationToken.None);

    if (!resultadoDoRelacionamento.EhSucesso)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("A CARGA DE RELACIONAMENTO PAROU: " + resultadoDoRelacionamento.Erro);
        return 3;
    }

    resumoDoRelacionamento = resultadoDoRelacionamento.Valor;
}

cronometro.Stop();

// -------------------------------------------------------------------------------------------------
// Passo 3 — o que aconteceu, em número.
// -------------------------------------------------------------------------------------------------

Console.WriteLine();
Console.WriteLine($"Carga concluida em {cronometro.Elapsed:hh\\:mm\\:ss}.");
Console.WriteLine();
Console.WriteLine($"{"entidade",-24} {"lidas",9} {"gravadas",9}");

if (resumoDoCadastro is { } cadastro)
{
    Console.WriteLine($"{"Municipio",-24} {cadastro.MunicipiosLidos,9} {cadastro.MunicipiosGravados,9}");
    Console.WriteLine($"{"Cliente",-24} {cadastro.ClientesLidos,9} {cadastro.ClientesGravados,9}");
    Console.WriteLine($"{"Endereco",-24} {"-",9} {cadastro.EnderecosGravados,9}");
    Console.WriteLine($"{"Contato",-24} {cadastro.ContatosLidos,9} {cadastro.ContatosGravados,9}");
    Console.WriteLine($"{"Equipamento",-24} {cadastro.EquipamentosLidos,9} {cadastro.EquipamentosGravados,9}");
}

if (resumoDoRelacionamento is { } rel)
{
    Console.WriteLine($"{"Usuario",-24} {rel.UsuariosLidos,9} {rel.UsuariosGravados,9}");
    Console.WriteLine($"{"LinhaDeNegocio",-24} {"-",9} {rel.LinhasDeNegocioGravadas,9}");
    Console.WriteLine($"{"Carteira",-24} {rel.CarteirasLidas,9} {rel.CarteirasGravadas,9}");
    Console.WriteLine($"{"CarteiraMunicipio",-24} {rel.MunicipiosDeCarteiraLidos,9} " +
                      $"{rel.MunicipiosDeCarteiraGravados,9}");
    Console.WriteLine($"{"ClienteCarteira",-24} {rel.VinculosLidos,9} {rel.VinculosGravados,9}");
    Console.WriteLine($"{"TipoProcesso",-24} {"-",9} {rel.TiposDeProcessoGravados,9}");
    Console.WriteLine($"{"Fase",-24} {"-",9} {rel.FasesGravadas,9}");
    Console.WriteLine($"{"TipoTarefa",-24} {"-",9} {rel.TiposDeTarefaGravados,9}");
    Console.WriteLine($"{"Resultado",-24} {"-",9} {rel.ResultadosGravados,9}");
    Console.WriteLine($"{"Processo",-24} {rel.ProcessosLidos,9} {rel.ProcessosGravados,9}");
    Console.WriteLine($"{"Tarefa",-24} {rel.TarefasLidas,9} {rel.TarefasGravadas,9}");
    Console.WriteLine($"{"Interacao",-24} {rel.InteracoesLidas,9} {rel.InteracoesGravadas,9}");
}

// -------------------------------------------------------------------------------------------------
// O MUNICIPIO, EM NUMERO — o que o documento 26 publica.
//
// Esta conta e o que diz quando a coluna de texto do endereco pode ser removida: enquanto
// "sem municipio" for maior que zero, ela e divida de migracao viva.
// -------------------------------------------------------------------------------------------------
if (resumoDoCadastro is { } comMunicipio)
{
    Console.WriteLine();
    Console.WriteLine("- Municipio -");
    Console.WriteLine($"  {comMunicipio.MunicipiosGravados,9}  municipios distintos no catalogo do CRM");
    Console.WriteLine($"  {comMunicipio.MunicipiosDuplicadosNaOrigem,9}  chave(s) de origem que caem num " +
                      "municipio ja existente (a duplicacao do catalogo antigo)");
    Console.WriteLine($"  {comMunicipio.EnderecosComMunicipioPeloPonteiro,9}  endereco(s) ligados pelo " +
                      "ponteiro de cidade da origem");
    Console.WriteLine($"  {comMunicipio.EnderecosComMunicipioPeloNome,9}  endereco(s) ligados por nome mais UF");
    Console.WriteLine($"  {comMunicipio.EnderecosSemMunicipio,9}  endereco(s) SEM municipio do catalogo — " +
                      "ficaram com o texto do legado");
    Console.WriteLine($"  {comMunicipio.MunicipiosReconhecidosPelaChave,9}  municipio(s) renomeados pelo IBGE e " +
                      "reencontrados pela chave de origem (nao recriados)");
    Console.WriteLine($"  {comMunicipio.CorrecoesDeGrafiaMantidas,9}  endereco(s) com a correcao de grafia cortada mantida");
    Console.WriteLine($"  {comMunicipio.GrafiasCortadasReapontadas,9}  endereco(s) reapontados da grafia cortada " +
                      "para o municipio oficial nesta rodada");
    Console.WriteLine($"  {comMunicipio.GrafiasCortadasPendentes,9}  endereco(s) em grafia cortada pendentes de conferencia" +
                      (comMunicipio.ContornoOficialDisponivel ? string.Empty : " (contorno oficial do IBGE indisponivel)"));
}

if (resumoDoRelacionamento is { } comTerritorio)
{
    Console.WriteLine();
    Console.WriteLine("- Territorio da carteira -");
    Console.WriteLine($"  {comTerritorio.MunicipiosDeCarteiraGravados,9}  vinculo(s) carteira x municipio");
    Console.WriteLine($"  {comTerritorio.CarteirasComMunicipio,9}  carteira(s) com pelo menos uma cidade");
}

foreach (var (rotulo, saneamento, recusadas, porMotivo, exemplos) in Relatorios(
             resumoDoCadastro, resumoDoRelacionamento))
{
    Console.WriteLine();
    Console.WriteLine($"- {rotulo} -");
    Console.WriteLine("O que foi normalizado e o que foi recusado, por campo:");
    Console.WriteLine($"{"campo",-24} {"corrigidos",11} {"recusados",10}  exemplo");

    foreach (var (campo, contagemDoCampo) in saneamento.OrderByDescending(p => p.Value.Corrigidos))
        Console.WriteLine(
            $"{campo,-24} {contagemDoCampo.Corrigidos,11} {contagemDoCampo.Recusados,10}  " +
            (contagemDoCampo.ExemploDeCorrecao ?? contagemDoCampo.ExemploDeRecusa ?? string.Empty));

    Console.WriteLine();
    Console.WriteLine($"Recusas na fila de descarte: {recusadas}");

    foreach (var (regra, quantidade) in porMotivo.OrderByDescending(p => p.Value))
    {
        Console.WriteLine($"  {quantidade,7}  {regra}");
        Console.WriteLine($"           exemplo: {exemplos[regra]}");
    }
}

if (resumoDoRelacionamento is { } comDecisoes)
{
    Console.WriteLine();
    Console.WriteLine("As decisoes que a carga teve de tomar, e quantas linhas cada uma alcancou:");

    foreach (var (decisao, quantidade) in comDecisoes.Decisoes.OrderByDescending(p => p.Value))
        Console.WriteLine($"  {quantidade,9}  {decisao}");

    Console.WriteLine();
    Console.WriteLine($"  {comDecisoes.TarefasLigadasAInteracao,9}  " +
                      "Tarefas ligadas a interacao que as concluiu (o duplo ponteiro)");
    Console.WriteLine($"  {comDecisoes.VinculosComUltimoContato,9}  " +
                      "Vinculos de carteira com data de ultimo contato calculada das interacoes");
}

return 0;

// =================================================================================================
// Apoio
// =================================================================================================

static int? LerInteiro(string[] argumentos, string nome)
{
    var indice = Array.IndexOf(argumentos, nome);

    return indice >= 0 && indice + 1 < argumentos.Length
           && int.TryParse(argumentos[indice + 1], NumberStyles.Integer, CultureInfo.InvariantCulture,
               out var valor)
        ? valor
        : null;
}

static string? LerTexto(string[] argumentos, string nome)
{
    var indice = Array.IndexOf(argumentos, nome);

    return indice >= 0 && indice + 1 < argumentos.Length && !string.IsNullOrWhiteSpace(argumentos[indice + 1])
        ? argumentos[indice + 1]
        : null;
}

static string CodigoDoCrm(int codigoNoLegado) =>
    "0101" + codigoNoLegado.ToString("00", CultureInfo.InvariantCulture);

// O de-para de filiais e quem responde pelos registros: PreparacaoDaCarga.cs, que o serviço também usa.

// Junta os dois relatorios de saneamento num laco so, para o mesmo formato valer para as duas
// etapas - e para acrescentar uma terceira, um dia, nao exigir copiar o bloco de impressao.
static IEnumerable<(string Rotulo,
                    IReadOnlyDictionary<string, ContagemDeSaneamento> Saneamento,
                    int Recusadas,
                    IReadOnlyDictionary<string, int> PorMotivo,
                    IReadOnlyDictionary<string, string> Exemplos)>
    Relatorios(ResumoDaCarga? cadastro, ResumoDoRelacionamento? relacionamento)
{
    if (cadastro is not null)
        yield return ("Cadastro", cadastro.Saneamento, cadastro.Recusadas,
            cadastro.RecusasPorMotivo, cadastro.ExemplosDeRecusa);

    if (relacionamento is not null)
        yield return ("Relacionamento", relacionamento.Saneamento, relacionamento.Recusadas,
            relacionamento.RecusasPorMotivo, relacionamento.ExemplosDeRecusa);
}
