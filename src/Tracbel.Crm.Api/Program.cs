using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Api.Seguranca;
using Tracbel.Crm.Api.Endpoints;
using Tracbel.Crm.Aplicacao.Catalogos;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Aplicacao.Integracoes;
using Tracbel.Crm.Aplicacao.Legado;
using Tracbel.Crm.Aplicacao.Mercado;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Aplicacao.Territorio;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Infraestrutura.Seguranca;
using Tracbel.Crm.Integracao.Conexoes;
using Tracbel.Crm.Integracao.Protheus;
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
{
    opcoes.PermitirPadrao = builder.Environment.IsDevelopment();

    // A CONCESSÃO EXPLÍCITA PELA PONTE, SÓ EM DESENVOLVIMENTO — é o que deixa validar um perfil de
    // teste com a visão da empresa num banco isolado sem abrir esse alcance a quem só escreve um
    // cabeçalho em homologação ou produção (ver OpcoesDeContextoProvisorio).
    opcoes.HonrarConcessoesExplicitas = builder.Environment.IsDevelopment();
});

builder.Services.AddScoped<ContextoAcessoDaRequisicao>();
builder.Services.AddScoped<IProvedorContextoAcesso>(sp =>
    sp.GetRequiredService<ContextoAcessoDaRequisicao>());
builder.Services.AddScoped<ResolvedorDeContextoProvisorio>();

// OS DOIS RESOLVEDORES FICAM REGISTRADOS, e quem escolhe é o estado. O do Entra é necessário mesmo
// com o login desligado, porque a rota /auth/eu o recebe por injeção — e responde "provisório" sem
// chegar a usá-lo.
builder.Services.AddScoped<ResolvedorDeContextoDoEntraId>();
builder.Services.AddSingleton(new EstadoDaAutenticacao(entraLigado));

// O GRUPO DO ENTRA É O PORTÃO (decisão de 21/09/2026): quem passa por ele e não tem cadastro ganha o
// usuário, aguardando liberação. Sem grupo configurado, qualquer conta do locatário passaria — então
// nada é criado, e vale o "sem cadastro" de antes (ver OpcoesDoPrimeiroLogin).
builder.Services.Configure<OpcoesDoPrimeiroLogin>(opcoes =>
    opcoes.CriarUsuarioAguardandoLiberacao =
        entraLigado && !string.IsNullOrWhiteSpace(builder.Configuration["Entra:GrupoPermitido"]));

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
builder.Services.AddScoped<IRepositorioDoMotorDoPotencial, RepositorioDoMotorDoPotencial>();
builder.Services.AddScoped<IRepositorioDeIndicadoresDeMercado, RepositorioDeIndicadoresDeMercado>();
builder.Services.AddScoped<IRepositorioIndicadoresTerritoriais, RepositorioDeIndicadoresTerritoriais>();
builder.Services.AddScoped<IRepositorioDePrecosDeMercado, RepositorioDePrecosDeMercado>();
builder.Services.AddScoped<IRepositorioDeCustosDeProducao, RepositorioDeCustosDeProducao>();
builder.Services.AddScoped<IRepositorioDeCreditoRural, RepositorioDeCreditoRural>();
builder.Services.AddScoped<IRepositorioDoCatalogoDoMercado, RepositorioDoCatalogoDoMercado>();
builder.Services.AddScoped<IRepositorioDeRentabilidade, RepositorioDeRentabilidade>();
builder.Services.AddScoped<IRepositorioDeParametrosDoPotencial, RepositorioDeParametrosDoPotencial>();
builder.Services.AddScoped<IRepositorioDeReferenciasDoPotencial, RepositorioDeParametrosDoPotencial>();
builder.Services.AddScoped<IRepositorioDeVigenciasDoPotencial, RepositorioDeParametrosDoPotencial>();
builder.Services.AddScoped<IRepositorioDeFontesPublicas, RepositorioDeFontesPublicas>();
builder.Services.AddScoped<IRepositorioDeOpcoesDosParametros, RepositorioDeFontesPublicas>();
builder.Services.AddScoped<IRepositorioDeCoberturaDoMotor, RepositorioDeCoberturaDoMotor>();
builder.Services.AddScoped<IRepositorioDeEscopo, RepositorioDeEscopo>();
builder.Services.AddScoped<RepositorioDeAdministracaoDeUsuarios>();
builder.Services.AddScoped<IRepositorioDeUsuariosDaAdministracao>(s => s.GetRequiredService<RepositorioDeAdministracaoDeUsuarios>());
builder.Services.AddScoped<IRepositorioDeConcessoes>(s => s.GetRequiredService<RepositorioDeAdministracaoDeUsuarios>());
builder.Services.AddScoped<IRepositorioDeReferenciasDeAcesso>(s => s.GetRequiredService<RepositorioDeAdministracaoDeUsuarios>());
builder.Services.AddScoped<IRepositorioDePerfisDaAdministracao, RepositorioDePerfisDaAdministracao>();
builder.Services.AddScoped<IRepositorioDaTrilhaDeAuditoria, RepositorioDaTrilhaDeAuditoria>();
builder.Services.AddScoped<RepositorioDeIntegracoes>();
builder.Services.AddScoped<IRepositorioDeConexoes>(s => s.GetRequiredService<RepositorioDeIntegracoes>());
builder.Services.AddScoped<IRepositorioDeRotinas>(s => s.GetRequiredService<RepositorioDeIntegracoes>());
builder.Services.AddScoped<IConsultaDoHistoricoDeIntegracoes>(s => s.GetRequiredService<RepositorioDeIntegracoes>());
builder.Services.AddScoped<IRepositorioIndicadoresExecutivos, RepositorioDeIndicadoresExecutivos>();
builder.Services.AddScoped<IRepositorioHistoricoComercial, RepositorioDeHistoricoComercial>();
builder.Services.AddScoped<IRepositorioSincronizacoes, RepositorioDeSincronizacoes>();
builder.Services.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();

// -------------------------------------------------------------------------------------------
// A PONTE DE LEITURA DO SISTEMA LEGADO.
//
// A CREDENCIAL VEM DE VARIÁVEL DE AMBIENTE (Vortice__Conexao) e não existe em arquivo nenhum do
// repositório. Sem ela, a ponte responde "não configurada" com 503 e TODO o resto da API
// continua funcionando — que é o requisito: uma dependência de VPN não pode derrubar o cadastro.
// -------------------------------------------------------------------------------------------
builder.Services.Configure<OpcoesDoVortice>(builder.Configuration.GetSection(OpcoesDoVortice.Secao));
// A CREDENCIAL DA TELA (issue 136) entra por pós-configuração, lida a cada requisição: trocar a senha em Configurações
// vale na próxima busca, sem reiniciar. Sem ela, fica a variável de ambiente, como antes.
builder.Services.AddScoped<IPostConfigureOptions<OpcoesDoVortice>, CredencialDoVorticePelaTela>();
builder.Services.AddScoped<IPonteDeLeituraDoVortice>(s => new PonteDeLeituraDoVortice(
    Options.Create(s.GetRequiredService<IOptionsSnapshot<OpcoesDoVortice>>().Value),
    s.GetRequiredService<IRelogio>(),
    s.GetRequiredService<ILogger<PonteDeLeituraDoVortice>>()));

// AS INTEGRAÇÕES CONFIGURÁVEIS (issue 136): a credencial protegida, o resolvedor (tela, depois ambiente) e o botão
// "Testar", com um cliente HTTP que não segue redirecionamento nem descomprime - o teste lê só o cabeçalho.
builder.Services.AddSingleton<IProtetorDeSegredos, ProtetorDeSegredos>();
builder.Services.AddScoped<IResolvedorDeConexoes, ResolvedorDeConexoes>();
builder.Services.AddScoped<ITestadorDeConexoes, TestadorDeConexoes>();
builder.Services.AddHttpClient(TestadorDeConexoes.NomeDoCliente)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = System.Net.DecompressionMethods.None });
builder.Services.AddHttpClient(PonteDoProtheus.NomeDoCliente);

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
builder.Services.AddScoped<ListarVendasDoEquipamento>();
builder.Services.AddScoped<ListarMaquinasCompradasPeloCliente>();

// O registro das sincronizações, para a administração (documento 35, seção 11).
builder.Services.AddScoped<ListarSincronizacoes>();

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
builder.Services.AddScoped<ObterIndicadoresTerritoriais>();
builder.Services.AddScoped<SimularMaquinas>();
builder.Services.AddScoped<ObterPrecosDeMercado>();
builder.Services.AddScoped<ObterCustosDeProducao>();
builder.Services.AddScoped<ObterCreditoRural>();
builder.Services.AddScoped<ObterRentabilidadeDasCulturas>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.ObterCatalogoDoMercado>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.CadastrarCultura>();

// Os parâmetros do potencial, com vigência (issue 71).
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.ObterParametrosDoPotencial>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.ListarHistoricoDosParametrosDoPotencial>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.InformarParametroDoPotencial>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.InformarRegraDePotencial>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.InformarPercepcaoDoGestor>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.RevogarParametroDoPotencial>();

// O painel de fontes públicas e as opções dos formulários de parâmetros (issue 77).
builder.Services.AddScoped<ObterFontesPublicas>();
builder.Services.AddScoped<ObterCoberturaDoMotor>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Potencial.ListarOpcoesDosParametros>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.ObterEscopoDeAcesso>();

// A administração de usuários e concessões (issue 113).
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.ListarUsuariosDaAdministracao>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.ObterUsuarioDaAdministracao>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.ListarPerfisParaConceder>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.AdministrarUsuario>();

// A administração de perfis próprios (issue 113, parte 2b).
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.ListarPerfisDaAdministracao>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.ListarCatalogoDePermissoes>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Seguranca.AdministrarPerfil>();

// A trilha de auditoria na tela (issue 135).
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Auditoria.ConsultarTrilhaDeAuditoria>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Auditoria.ListarEntidadesAuditadas>();

// As integrações configuráveis (issue 136).
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Integracoes.MontadorDoPainelDeIntegracoes>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Integracoes.ListarIntegracoes>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Integracoes.ListarHistoricoDeIntegracoes>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Integracoes.AdministrarConexoes>();
builder.Services.AddScoped<Tracbel.Crm.Aplicacao.Integracoes.AdministrarRotinas>();
builder.Services.AddScoped<ObterIndicadoresExecutivos>();

builder.Services.AddScoped<BuscarClientesNoLegado>();
builder.Services.AddScoped<ListarParqueNoLegado>();
builder.Services.AddScoped<VerificarPonteDoLegado>();

var app = builder.Build();

// AS MIGRAÇÕES RODAM NA SUBIDA, EM PRODUÇÃO.
//
// Esta decisão já foi a contrária, e mudou por uma razão de operação. Com o esquema aplicado por
// fora, toda atualização exigia alguém entrar no servidor e rodar o instalador — e o banco de lá é
// restaurado de backup, que é uma foto do dia em que foi tirado: migração nova fica faltando, e a
// aplicação só quebra na primeira consulta que tocar a coluna nova. Migrando na subida, atualizar
// vira copiar e reiniciar, do mesmo jeito que o user-onboarding faz neste mesmo servidor
// (scripts/deploy/publicar.ps1).
//
// SÓ EM PRODUÇÃO. Em desenvolvimento o esquema é do `dotnet ef`, e nos testes o banco é SQLite
// criado na hora — migração de SQL Server não roda lá.
//
// FALHA NA MIGRAÇÃO DERRUBA A SUBIDA, de propósito. Uma aplicação no ar com esquema pela metade
// responderia errado em silêncio; um serviço que não sobe aparece no deploy na hora.
if (app.Environment.IsProduction())
{
    await using var escopoDaMigracao = app.Services.CreateAsyncScope();
    var opcoesDaMigracao = escopoDaMigracao.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();

    // Contexto de SISTEMA: na subida não existe requisição, logo não existe contexto de acesso.
    await using var bancoDaMigracao = new CrmDbContext(opcoesDaMigracao, ProvedorDeContextoDeSistema.Instancia);

    var pendentes = (await bancoDaMigracao.Database.GetPendingMigrationsAsync()).ToList();
    if (pendentes.Count > 0)
    {
        app.Logger.LogWarning(
            "MIGRAÇÃO NA SUBIDA: aplicando {Quantidade} migração(ões): {Lista}.",
            pendentes.Count, string.Join(", ", pendentes));
        await bancoDaMigracao.Database.MigrateAsync();
        app.Logger.LogWarning("MIGRAÇÃO NA SUBIDA: concluída.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().SemPermissaoExigida("o contrato da API, só em Desenvolvimento");

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

// A PERMISSÃO QUE CADA ROTA DECLARA é conferida aqui, contra o contexto que o meio de campo acima montou
// (fase 3 do documento 41). A rota sem declaração nenhuma é barrada pelo teste de arquitetura, não aqui.
app.UseMiddleware<MeioDeCampoDePermissao>();

// Prova de vida que também confirma que o banco responde — é o que o script de subida checa.
app.MapGet("/saude/banco", async (DbContextOptions<CrmDbContext> opcoesDoBanco, CancellationToken ct) =>
{
    // O BANCO NASCE AQUI, SOB CONTEXTO DE SISTEMA, e não por injeção. O CrmDbContext injetado lê o
    // contexto de acesso no construtor — e /saude fica FORA do meio de campo de propósito, então esse
    // contexto nunca é definido e a leitura lança. Era por isso que esta rota devolvia 500 com corpo
    // vazio no servidor: a prova de vida morria antes de perguntar qualquer coisa ao banco.
    await using var contexto = new CrmDbContext(opcoesDoBanco, ProvedorDeContextoDeSistema.Instancia);

    var conecta = await contexto.Database.CanConnectAsync(ct);
    var pendentes = await contexto.Database.GetPendingMigrationsAsync(ct);
    return Results.Ok(new
    {
        Conectado = conecta,
        MigracoesPendentes = pendentes.ToArray()
    });
}).SemPermissaoExigida("prova de vida do banco: não lê dado de ninguém e é o que o script de subida confere");

app.MapearAcesso();
app.MapearClientes();
app.MapearEquipamentos();
app.MapearSincronizacoes();
app.MapearCatalogos();
app.MapearLegado();
app.MapearProcessos();
app.MapearTarefas();
app.MapearInteracoes();
app.MapearCobertura();
app.MapearCoberturaTerritorial();
app.MapearMunicipios();
app.MapearIndicadoresTerritoriais();
app.MapearMercado();
app.MapearParametrosDoPotencial();
app.MapearAdministracaoDeUsuarios();
app.MapearAdministracaoDePerfis();
app.MapearAuditoria();
app.MapearAdministracaoDeIntegracoes();
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
    app.MapFallbackToFile("index.html").SemPermissaoExigida("o arquivo da tela: não lê banco nem tem fronteira de filial");
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
