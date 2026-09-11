using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Api.Seguranca;
using Tracbel.Crm.Api.Endpoints;
using Tracbel.Crm.Aplicacao.Catalogos;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Aplicacao.Legado;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Aplicacao.Territorio;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Tracbel.Crm.Integracao.Vortice;

// A RAIZ DE CONTEÚDO É A PASTA DO EXECUTÁVEL, E ISSO PRECISA SER DITO AQUI.
//
// `CreateBuilder` calcula o caminho do `wwwroot` A PARTIR da raiz de conteúdo, e faz isso AGORA.
// Como serviço do Windows, a pasta de trabalho do processo é `C:\Windows\System32` — então, sem
// esta linha, a raiz vira `System32` e o front é procurado em `C:\Windows\System32\wwwroot`.
//
// `UseWindowsService()` mais abaixo também ajusta a raiz, mas TARDE DEMAIS: o caminho do `wwwroot`
// já foi resolvido e não é recalculado. Medido no servidor: a página inicial devolvia o erro 422
// da API em vez do portal, porque o arquivo estático nunca era encontrado e a requisição seguia
// para os endpoints.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// RODAR COMO SERVIÇO DO WINDOWS.
//
// Sem esta linha a aplicação sobe, atende a porta, e mesmo assim MORRE. O Gerenciador de Serviços
// espera que o processo avise que terminou de iniciar; um programa de console comum nunca avisa,
// então o serviço fica em "Start Pending" e o Windows o encerra quando o prazo de partida vence.
// Medido no servidor: o portal respondeu na porta 5443 por alguns minutos e depois sumiu, com o
// serviço parado — sem erro nenhum no log da aplicação, porque a aplicação não falhou.
//
// `UseWindowsService` também aponta a raiz de conteúdo para a pasta do executável. Isso importa
// aqui: sem ela a raiz é a pasta de trabalho do serviço (`C:\Windows\System32`), e o `wwwroot`
// com o front não seria encontrado.
//
// Fora do Windows, ou rodando pelo `dotnet run`, o método não faz nada — o desenvolvimento não
// muda de comportamento.
builder.Host.UseWindowsService();

// A AUTENTICAÇÃO SÓ LIGA QUANDO HÁ COMO LIGÁ-LA.
//
// O registro do aplicativo no Entra depende do DNS definitivo (o endereço de retorno tem de estar
// cadastrado lá), e uma implantação não pode ficar refém de um cadastro que ainda não foi feito.
// Configurado, o login passa a valer; não configurado, a API segue no modo de cabeçalho — com o
// aviso alto que já existe mais abaixo.
var entraLigado = builder.Services.AdicionarEntraId(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// -------------------------------------------------------------------------------------------
// SQL Server — decisão registrada em docs/projeto/20-DECISAO-SQL-SERVER.md, que substitui a
// 19-DECISAO-POSTGRESQL.md. Em produção é um banco NOVO dentro da instância que já hospeda o
// Vórtice; em desenvolvimento é o container de infra/docker-compose.yml.
//
// Não há conversão de nomes: o nome no banco é o PascalCase do C#, exatamente como o
// docs/projeto/15-GLOSSARIO-E-NOMES.md documenta.
// -------------------------------------------------------------------------------------------
// A CADEIA DE CONEXÃO DO CRM, com mensagem útil quando ela não foi configurada.
//
// Função local de nível superior — comentário de barra, e não XML, porque instrução de nível
// superior não é membro de tipo e o compilador recusa doc-comment em cima dela.
static string LerCadeiaDeConexao(IConfiguration configuracao)
{
    var cadeia = configuracao.GetConnectionString("Crm");

    if (!string.IsNullOrWhiteSpace(cadeia)) return cadeia;

    throw new InvalidOperationException(
        "A cadeia de conexão do CRM não está configurada. Ela NÃO mora no appsettings.json — " +
        "senha em arquivo versionado fica no histórico do git para sempre. Defina a variável de " +
        "ambiente ConnectionStrings__Crm antes de subir a API. Em desenvolvimento, o valor sai " +
        "de infra/.env (copie infra/.env.exemplo) e aponta para o container de " +
        "infra/docker-compose.yml. Ver docs/projeto/23-API.md.");
}

builder.Services.AddDbContext<CrmDbContext>(opcoes =>
    opcoes.UseSqlServer(
        // VAZIO CONTA COMO AUSENTE. O `??` sozinho só pega nulo, e o appsettings versionado traz
        // a chave com string vazia de propósito — a senha não mora nele. Sem esta checagem, o
        // erro que aparece é do driver, sobre um servidor sem nome, e não sobre o que falta.
        LerCadeiaDeConexao(builder.Configuration),
        sql => sql
            // A tabela de controle de migração do EF Core nasceria em `dbo`, e a regra do
            // documento 14, seção 2, é que nada mora lá. Ela é metadado sobre a versão do
            // próprio esquema — logo, schema `metadado`. O NOME continua sendo o convencional
            // do EF: é o que qualquer ferramenta reconhece.
            .MigrationsHistoryTable("__EFMigrationsHistory", "metadado")
            // A migração cria 63 tabelas, ~200 chaves estrangeiras e ~230 índices;
            // 30 segundos não bastam numa máquina carregada.
            .CommandTimeout(180)
            // RETENTATIVA EM FALHA TRANSITÓRIA, e ela vale SÓ NA API.
            //
            // A Visão 360 consolida treze filiais e dispara mais de setenta consultas quase ao
            // mesmo tempo. Medido em 06/09/2026, com o container recém-reiniciado: o SQL Server
            // recusou parte delas com falha de conexão transitória, e a tela inteira caiu com
            // uma exceção em vez de mostrar número nenhum. Não é defeito de consulta — é a
            // natureza de conexão de rede, e o próprio EF recomenda tratar assim.
            //
            // NÃO SE LIGA ISTO NA CARGA. Lá as gravações rodam dentro de transação explícita, e
            // a estratégia de retentativa recusa `BeginTransaction` — a carga precisa de
            // `ExecutionStrategy.Execute` em volta de cada bloco, que é outro trabalho. Leitura
            // não tem transação e não tem esse problema.
            .EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

// -------------------------------------------------------------------------------------------
// O CONTEXTO DE ACESSO — quem está agindo nesta requisição.
//
// DÍVIDA NOMEADA, e é a mais importante deste passo: enquanto o Entra ID não entra (fase 0 do
// documento 13), a identidade vem de DOIS CABEÇALHOS HTTP. Isso NÃO autentica ninguém — serve
// para o front ser construído contra dado real e para a fronteira de multiempresa ser
// exercitada de verdade. As três amarras que impedem isso de virar permanente por descuido
// estão escritas em ResolvedorDeContextoProvisorio; a principal delas é a linha `PermitirPadrao`
// logo abaixo, que só é verdadeira em Desenvolvimento.
//
// O PONTO DE TROCA é uma classe só: quando a autenticação real entrar, nasce um
// ResolvedorDeContextoDoEntraId lendo as reivindicações do token, e estas linhas trocam de nome.
// Nada além delas muda — o ContextoAcesso montado já é o objeto definitivo.
// -------------------------------------------------------------------------------------------
builder.Services.Configure<OpcoesDeContextoProvisorio>(
    builder.Configuration.GetSection(OpcoesDeContextoProvisorio.Secao));

builder.Services.PostConfigure<OpcoesDeContextoProvisorio>(opcoes =>
    opcoes.PermitirPadrao = builder.Environment.IsDevelopment());

builder.Services.AddScoped<ContextoAcessoDaRequisicao>();
builder.Services.AddScoped<IProvedorContextoAcesso>(sp =>
    sp.GetRequiredService<ContextoAcessoDaRequisicao>());
builder.Services.AddScoped<ResolvedorDeContextoProvisorio>();

// OS DOIS RESOLVEDORES FICAM REGISTRADOS, e quem escolhe é o estado. O do Entra é necessário mesmo
// com o login desligado, porque a rota /auth/eu o recebe por injeção — e responde "provisório" sem
// chegar a usá-lo.
builder.Services.AddScoped<ResolvedorDeContextoDoEntraId>();
builder.Services.AddSingleton(new EstadoDaAutenticacao(entraLigado));

// O diário da via de escape da fronteira de multiempresa (documento 21, achado A-1). Sem ele
// registrado, CrmDbContext.AbrirAlcanceEntreEmpresas se RECUSA a abrir: quem ignora a
// fronteira precisa dizer que está ignorando, e isso precisa aparecer no log.
builder.Services.AddScoped<IDiarioDeAlcanceEntreEmpresas, DiarioDeAlcanceEntreEmpresasEmLog>();

builder.Services.AddSingleton<IRelogio, RelogioDoSistema>();

// -------------------------------------------------------------------------------------------
// As portas do domínio e seus adaptadores. O caso de uso conhece a interface; só esta linha
// sabe qual implementação entra (documento 22, seção 7).
// -------------------------------------------------------------------------------------------
builder.Services.AddScoped<IRepositorioClientes, RepositorioDeClientes>();
builder.Services.AddScoped<IRepositorioEquipamentos, RepositorioDeEquipamentos>();
builder.Services.AddScoped<IRepositorioCatalogos, RepositorioDeCatalogos>();
builder.Services.AddScoped<IRepositorioProcessos, RepositorioDeProcessos>();
builder.Services.AddScoped<IRepositorioVendasPerdidas, RepositorioDeVendasPerdidas>();
builder.Services.AddScoped<IRepositorioPainelDoCen, RepositorioDoPainelDoCen>();
builder.Services.AddScoped<IRepositorioFaturamento, RepositorioDeFaturamento>();
builder.Services.AddScoped<IRepositorioTarefas, RepositorioDeTarefas>();
builder.Services.AddScoped<IRepositorioInteracoes, RepositorioDeInteracoes>();
builder.Services.AddScoped<IRepositorioCarteiras, RepositorioDeCarteiras>();
builder.Services.AddScoped<IRepositorioTerritorio, RepositorioDeTerritorio>();
builder.Services.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();

// -------------------------------------------------------------------------------------------
// A PONTE DE LEITURA DO SISTEMA LEGADO.
//
// A CREDENCIAL VEM DE VARIÁVEL DE AMBIENTE (Vortice__Conexao) e não existe em arquivo nenhum do
// repositório. Sem ela, a ponte responde "não configurada" com 503 e TODO o resto da API
// continua funcionando — que é o requisito: uma dependência de VPN não pode derrubar o cadastro.
// -------------------------------------------------------------------------------------------
builder.Services.Configure<OpcoesDoVortice>(builder.Configuration.GetSection(OpcoesDoVortice.Secao));
builder.Services.AddScoped<IPonteDeLeituraDoVortice, PonteDeLeituraDoVortice>();

// Os casos de uso. Um por operação, resolvidos por injeção.
builder.Services.AddScoped<ListarClientes>();
builder.Services.AddScoped<ObterCliente>();
builder.Services.AddScoped<CriarCliente>();
builder.Services.AddScoped<AlterarCliente>();
builder.Services.AddScoped<InativarCliente>();

builder.Services.AddScoped<ListarEquipamentos>();
builder.Services.AddScoped<ObterEquipamento>();
builder.Services.AddScoped<CriarEquipamento>();
builder.Services.AddScoped<AlterarEquipamento>();
builder.Services.AddScoped<InativarEquipamento>();

builder.Services.AddScoped<ListarCatalogos>();

// A leitura do relacionamento: processo, tarefa, interação e cobertura. Todos os agregados são
// calculados no banco — a tela recebe dez linhas de funil, não 45 mil processos para somar.
builder.Services.AddScoped<ListarProcessos>();
builder.Services.AddScoped<ObterProcesso>();
builder.Services.AddScoped<ObterFunil>();
builder.Services.AddScoped<ObterPerdas>();
builder.Services.AddScoped<ObterVendasPerdidas>();
builder.Services.AddScoped<ObterPainelDoCen>();
builder.Services.AddScoped<ObterFaturamento>();
builder.Services.AddScoped<ListarTarefas>();
builder.Services.AddScoped<ObterPainelDaAgenda>();
builder.Services.AddScoped<ListarInteracoes>();
builder.Services.AddScoped<ListarCobertura>();
builder.Services.AddScoped<ObterResumoDeCobertura>();
builder.Services.AddScoped<ListarMunicipios>();
builder.Services.AddScoped<ObterCoberturaPorFilial>();
builder.Services.AddScoped<ListarTerritorioPorCarteira>();

builder.Services.AddScoped<BuscarClientesNoLegado>();
builder.Services.AddScoped<ListarParqueNoLegado>();
builder.Services.AddScoped<VerificarPonteDoLegado>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // O aviso de que a identidade é provisória sai UMA VEZ na subida, além de sair por
    // requisição quando o valor padrão é usado. É barato e é o que impede alguém encontrar esta
    // configuração num ambiente errado sem nenhum sinal.
    app.Logger.LogWarning(
        "PROVISÓRIO: a identidade desta API vem dos cabeçalhos {Usuario} e {Empresa}. " +
        "Isto NÃO autentica ninguém e vale até a autenticação pelo Entra ID entrar " +
        "(fase 0 do documento 13). Ver docs/projeto/23-API.md, seção de dívida.",
        "X-Tracbel-Usuario", "X-Tracbel-Empresa");
}

app.UseHttpsRedirection();

if (entraLigado)
{
    app.UseAuthentication();
    app.UseAuthorization();

    app.Logger.LogInformation(
        "Autenticação pelo Entra ID ATIVA. Grupo exigido: {Grupo}.",
        builder.Configuration["Entra:GrupoPermitido"] ?? "(nenhum — qualquer conta do locatário entra)");

}
else
{
    app.Logger.LogWarning(
        "Autenticação pelo Entra ID DESLIGADA: faltam Entra__TenantId, Entra__ClientId ou " +
        "Entra__ClientSecret. Qualquer um que alcance esta porta vê tudo.");
}

// AS ROTAS DE SESSÃO EXISTEM NOS DOIS MODOS. Com o login desligado, /auth/eu responde "provisório" —
// e é essa resposta que faz a tela pular o login, em vez de mostrá-lo sem ter como entrar.
app.MapearAutenticacao(entraLigado);

// A API SERVE O FRONT, quando ele estiver publicado ao lado dela em `wwwroot`.
//
// POR QUE UM SERVIÇO SÓ, E NÃO IIS NA FRENTE. Separar front e API em duas portas obrigaria a
// três coisas que não existem hoje e que só criam superfície de erro: CORS na API (que ela não
// publica de propósito), um segundo certificado, e o módulo de proxy do IIS (que não está
// instalado no servidor de aplicação). Servindo da mesma origem, `/api` resolve sozinho — é
// exatamente o que o `vite.config.ts` já faz em desenvolvimento, e a tela não muda de
// comportamento entre os dois ambientes.
//
// Em desenvolvimento não há `wwwroot`, e o bloco inteiro fica fora do caminho.
if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// ORDEM IMPORTA: o contexto de acesso precisa estar definido antes de qualquer endpoint
// resolver o CrmDbContext, porque o filtro global é pré-computado no construtor dele.
app.UseMiddleware<MeioDeCampoDeContextoDeAcesso>();

// Prova de vida que também confirma que o banco responde — é o que o script de subida checa.
app.MapGet("/saude/banco", async (CrmDbContext contexto, CancellationToken ct) =>
{
    var conecta = await contexto.Database.CanConnectAsync(ct);
    var pendentes = await contexto.Database.GetPendingMigrationsAsync(ct);
    return Results.Ok(new
    {
        Conectado = conecta,
        MigracoesPendentes = pendentes.ToArray()
    });
});

app.MapearClientes();
app.MapearEquipamentos();
app.MapearCatalogos();
app.MapearLegado();
app.MapearProcessos();
app.MapearTarefas();
app.MapearInteracoes();
app.MapearCobertura();
app.MapearCoberturaTerritorial();
app.MapearMunicipios();
app.MapearRelatorios();

// O ÚLTIMO RECURSO DEVOLVE O `index.html`, e é o que faz a navegação da tela funcionar.
//
// O front é uma aplicação de página única: `/cobertura` e `/clientes/123` existem no roteador do
// navegador, não no disco. Sem este desvio, recarregar a página numa rota interna devolveria 404 —
// o defeito clássico de SPA publicada, que só aparece quando alguém aperta F5 fora da home.
//
// Vem DEPOIS de todos os endpoints: `/api/...` que não existir continua devolvendo 404, e não uma
// página HTML disfarçada de resposta de API.
if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
{
    app.MapFallbackToFile("index.html");
}

app.Run();

/// <summary>
/// Torna a classe de entrada visível para o projeto de testes de API.
///
/// <c>WebApplicationFactory</c> precisa de um tipo público do assembly da aplicação para subir a
/// API em memória. Com instruções de nível superior, o compilador gera a classe como interna —
/// esta declaração parcial só a torna pública, sem acrescentar comportamento nenhum.
/// </summary>
public partial class Program;
