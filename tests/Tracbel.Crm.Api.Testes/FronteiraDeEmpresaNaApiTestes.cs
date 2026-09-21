using Tracbel.Crm.Dominio.Seguranca;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A FRONTEIRA DE EMPRESA PROVADA NA API — a mesma requisição, dois contextos, dois resultados.
///
/// <para>É a prova que o requisito pede com todas as letras: uma requisição no contexto de uma
/// filial não enxerga dado de outra. Aqui ela acontece por HTTP, com a aplicação inteira de pé,
/// e não numa consulta montada à mão.</para>
///
/// <para>O último teste é a PROVA NEGATIVA: desliga o filtro e confirma que, sem ele, a mesma
/// consulta entrega o registro da outra filial. Sem isso, os testes acima seriam igualmente
/// compatíveis com "o banco está vazio" — e um teste que passa pelo motivo errado dá segurança
/// falsa.</para>
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class FronteiraDeEmpresaNaApiTestes(ApiEmMemoria api, ITestOutputHelper saida)
    : IClassFixture<ApiEmMemoria>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();

    /// <summary>Cria um cliente na filial de Ribeirão Preto e devolve a chave pública dele.</summary>
    private async Task<Guid> CriarEmRibeiraoAsync(string nome)
    {
        var resposta = await api.ClienteDeRibeirao().PostAsJsonAsync("/api/v1/clientes", new
        {
            nomeRazao = nome,
            tipoDePessoa = "Juridica"
        }, Json);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await CorpoAsync(resposta)).GetProperty("chave").GetGuid();
    }

    [Fact]
    public async Task A_MESMA_requisicao_GET_devolve_200_numa_filial_e_404_na_outra()
    {
        var chave = await CriarEmRibeiraoAsync("Fazenda Exclusiva de Ribeirão Ltda");
        var rota = $"/api/v1/clientes/{chave}";

        var emRibeirao = await api.ClienteDeRibeirao().GetAsync(rota);
        var emBarretos = await api.ClienteDeBarretos().GetAsync(rota);

        saida.WriteLine($"GET {rota}");
        saida.WriteLine($"  X-Tracbel-Empresa: {ApiEmMemoria.FilialDeRibeirao} → {(int)emRibeirao.StatusCode}");
        saida.WriteLine($"  X-Tracbel-Empresa: {ApiEmMemoria.FilialDeBarretos} → {(int)emBarretos.StatusCode}");

        emRibeirao.StatusCode.Should().Be(HttpStatusCode.OK);

        // 404, e não 403: distinguir "não existe" de "existe mas não é seu" contaria a quem não
        // pode ver que o registro existe.
        emBarretos.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_listagem_da_outra_filial_nao_traz_o_registro_nem_no_total()
    {
        await CriarEmRibeiraoAsync("Fazenda Invisível para Barretos Ltda");

        var resposta = await api.ClienteDeBarretos()
            .GetAsync("/api/v1/clientes?termo=Invisível&incluirInativos=true");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        var dados = (await CorpoAsync(resposta)).GetProperty("dados");
        dados.GetProperty("itens").GetArrayLength().Should().Be(0);

        // O TOTAL TAMBÉM É ZERO: um filtro aplicado só na página, e não na contagem, devolveria
        // uma lista vazia com "total: 1" — e a tela diria que existe um registro que ela não pode
        // mostrar. Isso já é vazamento.
        dados.GetProperty("total").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task A_escrita_tambem_bate_na_fronteira_e_nao_so_a_leitura()
    {
        // Se a fronteira valesse só para a leitura, bastaria um PUT com a chave certa para
        // alterar o cadastro de outra filial sem nunca conseguir lê-lo.
        var chave = await CriarEmRibeiraoAsync("Fazenda que Barretos não altera Ltda");

        // BARRETOS RECEBE O PERFIL DE EXCLUSÃO para que o teste prove a FRONTEIRA (404), e não a falta de
        // permissão (403), que a fase 3 passou a conferir antes: sem o perfil, o DELETE pararia no 403.
        await api.ConcederPerfilAsync(200, PerfisDeSistema.ExclusaoDeCadastro);
        var http = api.ClienteDeBarretos();

        var alteracao = await http.PutAsJsonAsync($"/api/v1/clientes/{chave}", new
        {
            nomeRazao = "Sequestro de cadastro",
            tipoDePessoa = "Juridica"
        }, Json);

        alteracao.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var inativacao = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/clientes/{chave}")
        {
            Content = JsonContent.Create(new { motivoCodigo = "DUPLICADO" }, options: Json)
        };

        (await http.SendAsync(inativacao)).StatusCode.Should().Be(HttpStatusCode.NotFound);

        saida.WriteLine("PUT e DELETE no contexto de Barretos: 404 nos dois");
    }

    [Fact]
    public async Task O_equipamento_da_outra_filial_tambem_nao_aparece()
    {
        var cliente = await CriarEmRibeiraoAsync("Fazenda com Máquina Ltda");

        var criacao = await api.ClienteDeRibeirao().PostAsJsonAsync("/api/v1/equipamentos", new
        {
            chassi = "1JD8R340XYZ987654",
            modeloCodigo = ApiEmMemoria.ModeloSemeado,
            clienteChave = cliente.ToString(),
            situacao = "Ativo"
        }, Json);

        criacao.StatusCode.Should().Be(
            HttpStatusCode.Created, await criacao.Content.ReadAsStringAsync());

        // A busca por chassi é por VALOR INTEIRO — a razão está no repositório, e a limitação de
        // busca por pedaço está registrada como dívida no documento 23.
        const string Rota = "/api/v1/equipamentos?termo=1JD8R340XYZ987654";

        var deRibeirao = await api.ClienteDeRibeirao().GetAsync(Rota);
        var deBarretos = await api.ClienteDeBarretos().GetAsync(Rota);

        saida.WriteLine($"GET {Rota}");
        saida.WriteLine($"  filial {ApiEmMemoria.FilialDeRibeirao} → {(int)deRibeirao.StatusCode}");
        saida.WriteLine($"  filial {ApiEmMemoria.FilialDeBarretos} → {(int)deBarretos.StatusCode}");

        (await CorpoAsync(deRibeirao)).GetProperty("dados").GetProperty("total").GetInt32()
            .Should().Be(1);

        (await CorpoAsync(deBarretos)).GetProperty("dados").GetProperty("total").GetInt32()
            .Should().Be(0, "a fronteira vale para toda entidade que tem EmpresaId");
    }

    /// <summary>
    /// A PROVA NEGATIVA: desliga o filtro global e confirma que o registro APARECE.
    ///
    /// <para>Os testes acima afirmam que Barretos não vê o cliente de Ribeirão Preto. Sozinhos,
    /// são compatíveis com uma explicação bem mais chata — a de que o registro não foi gravado,
    /// ou de que a consulta nunca devolve nada. Aqui a MESMA consulta roda com
    /// <c>IgnoreQueryFilters</c>, no MESMO contexto de Barretos, e o registro aparece.</para>
    ///
    /// <para>Isso prende os dois lados: o dado está lá, a consulta funciona, e o que separa um
    /// resultado do outro é exclusivamente a fronteira. Se esta asserção falhar, os outros testes
    /// deste arquivo estão passando pelo motivo errado.</para>
    /// </summary>
    [Fact]
    public async Task Prova_negativa_sem_o_filtro_global_o_registro_da_outra_filial_aparece()
    {
        var chave = await CriarEmRibeiraoAsync("Fazenda da Prova Negativa Ltda");

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();

        // Um contexto montado com a identidade de Barretos, igual ao que a requisição HTTP monta.
        await using var comoBarretos = new CrmDbContext(opcoes, ContextoDeBarretos());

        var comFronteira = await comoBarretos.Clientes
            .Where(c => c.ChavePublica == chave).Select(c => c.NomeRazao).ToListAsync();

        var semFronteira = await comoBarretos.Clientes
            .IgnoreQueryFilters()
            .Where(c => c.ChavePublica == chave).Select(c => c.NomeRazao).ToListAsync();

        saida.WriteLine("--- SQL da consulta COM a fronteira ---");
        saida.WriteLine(comoBarretos.Clientes.Where(c => c.ChavePublica == chave).ToQueryString());
        saida.WriteLine(string.Empty);
        saida.WriteLine($"COM a fronteira (contexto de Barretos): {comFronteira.Count} linha(s)");
        saida.WriteLine($"SEM a fronteira, a MESMA consulta:      {semFronteira.Count} linha(s) " +
                        $"→ {string.Join(", ", semFronteira)}");

        comFronteira.Should().BeEmpty("é a fronteira que barra, e a API devolve 404 por causa dela");

        semFronteira.Should().ContainSingle(
            "o dado ESTÁ no banco e a consulta funciona — se esta asserção falhar, os 404 dos " +
            "outros testes vêm de banco vazio, não de segurança");
    }

    private static Dominio.Portas.IProvedorContextoAcesso ContextoDeBarretos()
    {
        var portador = new ContextoAcessoDaRequisicao();

        portador.Definir(new Dominio.Seguranca.ContextoAcesso(
            usuarioId: 200,
            nomeExibicao: "cen.barretos",
            empresaId: 2,
            empresasVisiveis: new HashSet<int> { 2 },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Dominio.Seguranca.Profundidade>()));

        return portador;
    }
}
