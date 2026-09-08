using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Integracao.Protheus;
using Tracbel.Crm.Integracao.Vortice;

Console.OutputEncoding = Encoding.UTF8;

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

var conexaoDoLegado = configuracao["Vortice:Conexao"];

// A EXIGÊNCIA CAI NO MODO SÓ-FATURAMENTO, e só nele: essa etapa não abre conexão com o legado.
// A cadeia continua sendo passada adiante como veio (possivelmente vazia) — se algum caminho
// tentar usá-la neste modo, a falha é imediata e ruidosa, que é o comportamento desejado.
if (string.IsNullOrWhiteSpace(conexaoDoLegado) && !somenteFaturamento)
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
    await PrepararAsync(opcoesDoBanco, diario);

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

// A PONTE DO PROTHEUS. Ela lê o faturamento da ORIGEM, e não da cópia no Vórtice que parou em
// 11/04/2025. As credenciais vêm de Protheus__Base, Protheus__Usuario e Protheus__Senha — nunca
// de arquivo versionado, porque a senha viaja na query string do endpoint de token.
var opcoesDoProtheus = new OpcoesDoProtheus
{
    Base = configuracao["Protheus:Base"],
    Usuario = configuracao["Protheus:Usuario"],
    Senha = configuracao["Protheus:Senha"]
};

if (!opcoesDoProtheus.EstaConfigurada)
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
                 "DELETE FROM processo.InteracaoParticipante",
                 "DELETE FROM processo.PassagemDeFase",
                 "DELETE FROM processo.ItemDeProposta",
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
        AbrirContexto, leitor, deParaDeFiliais, usuarioId, Console.WriteLine);

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

static string CodigoDoCrm(int codigoNoLegado) =>
    "0101" + codigoNoLegado.ToString("00", CultureInfo.InvariantCulture);

// Monta o DE-PARA DE FILIAIS a partir do banco, nao de um arquivo.
//
// O codigo oficial da filial e 0101NN, e NN e o numero da filial no sistema de origem — a
// correspondencia esta registrada em dados-referencia/empresa.json e ja foi semeada em
// organizacao.Empresa. Ler daqui garante que a carga e a API concordem sobre quais filiais
// existem: se uma filial for desativada amanha, a carga para de trazer cliente para ela sem
// ninguem precisar lembrar de mexer aqui.
static async Task<(Dictionary<int, int> DePara, long UsuarioId, int EmpresaId, string? Erro)>
    PrepararAsync(DbContextOptions<CrmDbContext> opcoes, DiarioDeAlcanceEntreEmpresasEmLog diario)
{
    var provisorio = new ContextoDeCargaDeSistema(0, 0, new HashSet<int>());
    await using var contexto = new CrmDbContext(opcoes, provisorio, diario);

    var empresas = await contexto.Empresas.AsNoTracking()
        .Where(e => e.EstaAtiva)
        .Select(e => new { e.Id, e.Codigo })
        .ToListAsync();

    var dePara = new Dictionary<int, int>();

    foreach (var empresa in empresas)
    {
        if (empresa.Codigo.Length != 6
            || !empresa.Codigo.StartsWith("0101", StringComparison.Ordinal)
            || !int.TryParse(empresa.Codigo.AsSpan(4), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var numero))
            continue;

        dePara[numero] = empresa.Id;
    }

    if (dePara.Count == 0)
        return ([], 0, 0,
            "Nenhuma filial ativa no padrão 0101NN foi encontrada em organizacao.Empresa. Rode o " +
            "seed de dados de referência antes da carga: ./scripts/banco/rodar-seed.ps1");

    var usuario = await contexto.Usuarios.AsNoTracking()
        .Where(u => u.EstaAtivo)
        .OrderBy(u => u.Id)
        .FirstOrDefaultAsync();

    return usuario is null
        ? ([], 0, 0,
            "Nenhum usuário ativo em seguranca.Usuario. Todo registro criado precisa de um " +
            "responsável, e a coluna é chave estrangeira: rode o seed de usuários de " +
            "desenvolvimento antes da carga.")
        : (dePara, usuario.Id, usuario.EmpresaId, null);
}

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
