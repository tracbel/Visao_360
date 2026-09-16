using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A TRILHA DE AUDITORIA AUTOMÁTICA, pela API de verdade (documento 41, fase 2).
///
/// <para>O critério de aceite da fase, escrito como teste: criar, alterar e inativar um cliente por
/// HTTP deixa linhas em <c>auditoria.AlteracaoDeCampo</c> com <c>Origem = Usuario</c>, o autor e o
/// identificador da requisição — sem que o caso de uso, o repositório ou a rota saibam que a trilha
/// existe. E o que não é para entrar não entra: campo fora da política e "mudança" que o banco não
/// reconhece como mudança.</para>
/// </summary>
[Trait("Categoria", "Auditoria")]
public sealed class AuditoriaAutomaticaNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const long UsuarioDeRibeirao = 100;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private async Task<(long Id, int EmpresaId)> ClienteAsync(Guid chave)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        var cliente = await db.Clientes.IgnoreQueryFilters().SingleAsync(c => c.ChavePublica == chave);
        return (cliente.Id, cliente.EmpresaId);
    }

    private async Task<List<AlteracaoDeCampo>> TrilhaAsync(long clienteId)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await db.AlteracoesDeCampo
            .Where(a => a.Entidade == "Cliente" && a.RegistroId == clienteId)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    private static async Task<Guid> CriarAsync(HttpClient http, string nome, string? documento = null)
    {
        var criacao = await http.PostAsJsonAsync("/api/v1/clientes", new
        {
            nomeRazao = nome,
            tipoDePessoa = "Juridica",
            documento,
            situacao = "Prospect",
            origemCodigo = "INDICACAO"
        }, Json);
        criacao.StatusCode.Should().Be(HttpStatusCode.Created, await criacao.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await criacao.Content.ReadAsStringAsync()).RootElement.GetProperty("chave").GetGuid();
    }

    [Fact]
    public async Task Criar_alterar_e_inativar_pela_API_deixam_trilha_com_origem_autor_e_requisicao()
    {
        var http = api.ClienteDeRibeirao();

        // --- criar
        var chave = await CriarAsync(http, "Fazenda da Trilha Ltda", "11.444.777/0001-61");
        var (clienteId, empresaId) = await ClienteAsync(chave);

        var inclusao = await TrilhaAsync(clienteId);
        inclusao.Should().NotBeEmpty("uma pessoa criou o cliente: o nascimento entra na trilha");
        inclusao.Should().AllSatisfy(l =>
        {
            l.Operacao.Should().Be(OperacaoAuditada.Inclusao);
            l.Origem.Should().Be(OrigemDaOperacao.Usuario);
            l.SistemaId.Should().BeNull("pessoa na tela não é sistema externo");
            l.AlteradoPorId.Should().Be(UsuarioDeRibeirao);
            l.EmpresaId.Should().Be(empresaId);
            l.ValorAnterior.Should().BeNull("antes de nascer não havia valor");
            l.CorrelacaoId.Should().NotBeNull();
        });
        inclusao.Select(l => l.CorrelacaoId).Distinct().Should().ContainSingle("uma requisição, um identificador");
        inclusao.Should().Contain(l => l.Campo == "NomeRazao" && l.ValorNovo == "Fazenda da Trilha Ltda");
        inclusao.Should().Contain(l => l.Campo == "Documento" && l.ValorNovo == "11444777000161",
            "o valor é o que o banco guarda: o documento sem máscara");
        inclusao.Should().Contain(l => l.Campo == "Situacao" && l.ValorNovo == "Prospect", "enum vira o nome, como no banco");

        // --- alterar
        var alteracao = await http.PutAsJsonAsync($"/api/v1/clientes/{chave}", new
        {
            nomeRazao = "Fazenda da Trilha Agropecuária Ltda",
            tipoDePessoa = "Juridica",
            documento = "11.444.777/0001-61",
            situacao = "Cliente"
        }, Json);
        alteracao.StatusCode.Should().Be(HttpStatusCode.OK, await alteracao.Content.ReadAsStringAsync());

        var aposAlterar = (await TrilhaAsync(clienteId)).Skip(inclusao.Count).ToList();
        aposAlterar.Should().OnlyContain(l => l.Operacao == OperacaoAuditada.Alteracao && l.Origem == OrigemDaOperacao.Usuario);
        aposAlterar.Should().ContainSingle(l => l.Campo == "NomeRazao")
            .Which.Should().BeEquivalentTo(new
            {
                ValorAnterior = "Fazenda da Trilha Ltda",
                ValorNovo = "Fazenda da Trilha Agropecuária Ltda",
                AlteradoPorId = UsuarioDeRibeirao
            });
        aposAlterar.Should().ContainSingle(l => l.Campo == "Situacao")
            .Which.ValorNovo.Should().Be("Cliente");
        aposAlterar.Should().NotContain(l => l.Campo == "Documento", "o documento não mudou");
        var requisicoesDaAlteracao = aposAlterar.Select(l => l.CorrelacaoId).Distinct().ToList();
        requisicoesDaAlteracao.Should().HaveCount(1, "uma requisição, um identificador");
        requisicoesDaAlteracao[0].Should().NotBe(inclusao[0].CorrelacaoId!.Value, "outra requisição, outro identificador");

        // --- inativar
        var inativacao = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/clientes/{chave}")
        {
            Content = JsonContent.Create(new { motivoCodigo = "DUPLICADO" }, options: Json)
        };
        (await http.SendAsync(inativacao)).StatusCode.Should().Be(HttpStatusCode.OK);

        var aposInativar = (await TrilhaAsync(clienteId)).Skip(inclusao.Count + aposAlterar.Count).ToList();
        aposInativar.Should().OnlyContain(l => l.Operacao == OperacaoAuditada.Exclusao,
            "exclusão lógica é exclusão: é o que alguém procura quando o cliente some da tela");
        aposInativar.Should().Contain(l => l.Campo == "ExcluidoEm" && l.ValorAnterior == null && l.ValorNovo != null);
        aposInativar.Should().Contain(l => l.Campo == "MotivoInativacaoId" && l.ValorNovo != null);
        aposInativar.Should().Contain(l => l.Campo == "Situacao" && l.ValorNovo == "ClienteInativo");
    }

    [Fact]
    public async Task Campo_fora_da_politica_nao_entra_na_trilha()
    {
        var http = api.ClienteDeRibeirao();
        var chave = await CriarAsync(http, "Fazenda Fora da Política Ltda");
        var (clienteId, _) = await ClienteAsync(chave);

        var alteracao = await http.PutAsJsonAsync($"/api/v1/clientes/{chave}", new
        {
            nomeRazao = "Fazenda Fora da Política S.A.",
            tipoDePessoa = "Juridica",
            situacao = "Prospect"
        }, Json);
        alteracao.StatusCode.Should().Be(HttpStatusCode.OK, await alteracao.Content.ReadAsStringAsync());

        var trilha = await TrilhaAsync(clienteId);

        // AlteradoEm, AlteradoPorId e SituacaoDesde mudam em toda alteração e não são decisão de
        // ninguém; Versao é o carimbo de concorrência. Nenhum está na política.
        trilha.Select(l => l.Campo).Should().OnlyContain(c => PoliticaDeAuditoria.Audita("Cliente", c));
        trilha.Should().NotContain(l => l.Campo == "AlteradoEm" || l.Campo == "AlteradoPorId"
                                        || l.Campo == "SituacaoDesde" || l.Campo == "Versao" || l.Campo == "CriadoEm");
    }

    [Fact]
    public async Task Mudanca_so_de_maiuscula_ou_acento_nao_entra_na_trilha_nem_derruba_a_gravacao()
    {
        // O banco compara o antes e o depois sem caixa nem acento (CK_AlteracaoDeCampo_Mudou): para ele
        // estas duas grafias são o mesmo texto, e uma linha na trilha seria recusada — levando a
        // gravação do cliente junto. A trilha pergunta "mudou?" do mesmo jeito que o banco.
        var http = api.ClienteDeRibeirao();
        var chave = await CriarAsync(http, "FAZENDA SAO JOAO LTDA");
        var (clienteId, _) = await ClienteAsync(chave);
        var antes = (await TrilhaAsync(clienteId)).Count;

        var alteracao = await http.PutAsJsonAsync($"/api/v1/clientes/{chave}", new
        {
            nomeRazao = "Fazenda São João Ltda",
            tipoDePessoa = "Juridica",
            situacao = "Prospect"
        }, Json);

        alteracao.StatusCode.Should().Be(HttpStatusCode.OK, "a grafia muda no cadastro, e a gravação não cai");
        (await TrilhaAsync(clienteId)).Skip(antes).Should().NotContain(l => l.Campo == "NomeRazao");
    }
}
