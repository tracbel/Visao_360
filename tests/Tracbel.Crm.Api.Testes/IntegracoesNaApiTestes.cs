using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Conexoes;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// AS INTEGRAÇÕES CONFIGURÁVEIS PELA API DE VERDADE (issue 136). O que se prende: a senha entra e não sai — nem na
/// resposta, nem no banco em texto, nem na trilha; a Gerência vê e não mexe; a agenda recusa o que não existe e a
/// rotina que exige credencial não liga sem ela; o botão "Testar" roda no servidor e guarda quem testou.
/// </summary>
public sealed class IntegracoesNaApiTestes
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private const string Base = "/api/v1/admin/integracoes";
    private const string Senha = "segredo-que-nao-volta-123";

    /// <summary>Responde a qualquer GET com o status escolhido, e conta os pedidos.</summary>
    private sealed class RespostaSimulada(HttpStatusCode status) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Pedidos { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Pedidos.Add(request);
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }

    private static async Task<JsonElement> ComStatusAsync(HttpResponseMessage resposta, HttpStatusCode esperado)
    {
        var texto = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(esperado, texto);
        return JsonDocument.Parse(texto).RootElement.Clone();
    }

    private static CrmDbContext Sistema(ApiEmMemoria app) =>
        new(app.Services.CreateScope().ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private static async Task<ApiEmMemoria> ComAdministradorAsync(RespostaSimulada? internet = null)
    {
        var app = new ApiEmMemoria
        {
            AjustarServicos = internet is null ? null : s => s.AddHttpClient(TestadorDeConexoes.NomeDoCliente).ConfigurePrimaryHttpMessageHandler(() => internet)
        };
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        return app;
    }

    [Fact]
    public async Task A_gerencia_ve_o_painel_e_nao_mexe_e_o_usuario_comum_nem_ve()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        (await app.ClienteDeRibeirao().GetAsync(Base)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        var painel = await ComStatusAsync(await app.ClienteDeRibeirao().GetAsync(Base), HttpStatusCode.OK);
        painel.GetProperty("dados").GetProperty("podeAdministrar").GetBoolean().Should().BeFalse();
        painel.GetProperty("dados").GetProperty("conexoes").GetArrayLength().Should().Be(ConexoesDoSistema.Todas.Count);
        painel.GetProperty("dados").GetProperty("rotinas").EnumerateArray().Select(r => r.GetProperty("codigo").GetString())
            .Should().Equal(RotinasDoSistema.Todas.Select(r => r.Codigo));

        (await app.ClienteDeRibeirao().PostAsync($"{Base}/conexoes/IBGE_SIDRA/teste", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await app.ClienteDeRibeirao().PostAsync($"{Base}/rotinas/{RotinasDoSistema.PrecosMensais}/execucao", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_senha_entra_protegida_e_nao_volta_nem_na_resposta_nem_no_banco_nem_na_trilha()
    {
        await using var app = await ComAdministradorAsync();
        var admin = app.ClienteDeRibeirao();

        await ComStatusAsync(await admin.PutAsJsonAsync($"{Base}/conexoes/{ConexoesDoSistema.Protheus}",
            new { endereco = "http://erp.exemplo.invalid:5891/rest", usuario = "leitura" }, Json), HttpStatusCode.OK);
        var resposta = await admin.PutAsJsonAsync($"{Base}/conexoes/{ConexoesDoSistema.Protheus}/segredo", new { segredo = Senha }, Json);
        var texto = await resposta.Content.ReadAsStringAsync();

        resposta.StatusCode.Should().Be(HttpStatusCode.OK, texto);
        texto.Should().NotContain(Senha, "a senha não volta em resposta nenhuma");
        var conexao = JsonDocument.Parse(texto).RootElement;
        conexao.GetProperty("origemDaCredencial").GetString().Should().Be("Tela");
        conexao.GetProperty("temSegredoNaTela").GetBoolean().Should().BeTrue();
        conexao.GetProperty("segredoAlteradoPor").GetString().Should().Be(ApiEmMemoria.UsuarioDeRibeirao);

        (await admin.GetStringAsync(Base)).Should().NotContain(Senha);

        await using var db = Sistema(app);
        var gravada = await db.Conexoes.AsNoTracking().SingleAsync(c => c.Codigo == ConexoesDoSistema.Protheus);
        Encoding.UTF8.GetString(gravada.SegredoProtegido!).Should().NotContain(Senha, "no banco ela está protegida, não em texto");
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(Dominio.Integracao.Conexao)).ToListAsync();
        trilha.Should().Contain(a => a.Campo == nameof(Dominio.Integracao.Conexao.SegredoAlteradoEm) && a.AlteradoPorId == 100, "a troca da credencial fica na trilha");
        trilha.Should().NotContain(a => (a.ValorNovo ?? string.Empty).Contains(Senha) || (a.ValorAnterior ?? string.Empty).Contains(Senha));

        // RETIRAR VOLTA AO AMBIENTE — que, no teste, não tem nada.
        var retirada = await ComStatusAsync(await admin.PostAsync($"{Base}/conexoes/{ConexoesDoSistema.Protheus}/segredo/remocao", null), HttpStatusCode.OK);
        retirada.GetProperty("origemDaCredencial").GetString().Should().Be("Nenhuma");
    }

    [Theory]
    [InlineData(ConexoesDoSistema.Vortice, "endereco", """{"endereco":"servidor;Password=x","banco":"Vortice","usuario":"u"}""")]
    [InlineData(ConexoesDoSistema.Art, "objeto", """{"endereco":"art.exemplo.invalid","banco":"vendas","objeto":"vw; DROP TABLE x","usuario":"u"}""")]
    [InlineData("IBGE_SIDRA", "endereco", """{"endereco":"https://outro.exemplo.invalid"}""")]
    [InlineData(ConexoesDoSistema.Protheus, "usuario", """{"endereco":"http://erp.exemplo.invalid/rest"}""")]
    public async Task A_configuracao_invalida_volta_no_campo(string codigo, string campo, string corpo)
    {
        await using var app = await ComAdministradorAsync();

        var resposta = await app.ClienteDeRibeirao().PutAsync($"{Base}/conexoes/{codigo}", new StringContent(corpo, Encoding.UTF8, "application/json"));

        var erro = await ComStatusAsync(resposta, HttpStatusCode.UnprocessableEntity);
        erro.GetProperty("erros").EnumerateArray().Select(e => e.GetProperty("campo").GetString()).Should().Contain(campo);
    }

    [Fact]
    public async Task A_rotina_que_exige_credencial_nao_liga_sem_ela_e_rodar_agora_entra_na_fila()
    {
        await using var app = await ComAdministradorAsync();
        var admin = app.ClienteDeRibeirao();
        var rotina = $"{Base}/rotinas/{RotinasDoSistema.Faturamento}";

        var semCredencial = await admin.PutAsJsonAsync(rotina, new { cadencia = "Diaria", hora = "05:00", ligada = true }, Json);
        (await ComStatusAsync(semCredencial, HttpStatusCode.Conflict)).GetProperty("title").GetString().Should().Contain("credencial");

        var diaQueNaoExiste = await admin.PutAsJsonAsync(rotina, new { cadencia = "Mensal", dia = 31, hora = "05:00" }, Json);
        (await ComStatusAsync(diaQueNaoExiste, HttpStatusCode.UnprocessableEntity)).GetProperty("erros")[0].GetProperty("campo").GetString().Should().Be("dia");

        // O FATURAMENTO LÊ O BANCO DO PROTHEUS (24/09/2026): a credencial que ele exige é a da conexão de banco.
        await ComStatusAsync(await admin.PutAsJsonAsync($"{Base}/conexoes/{ConexoesDoSistema.ProtheusBanco}",
            new { endereco = "erp.exemplo.invalid,1433", banco = "TMPRD", usuario = "leitura" }, Json), HttpStatusCode.OK);
        await ComStatusAsync(await admin.PutAsJsonAsync($"{Base}/conexoes/{ConexoesDoSistema.ProtheusBanco}/segredo", new { segredo = Senha }, Json), HttpStatusCode.OK);

        var ligada = await ComStatusAsync(await admin.PutAsJsonAsync(rotina, new { cadencia = "Diaria", hora = "06:30", ligada = true }, Json), HttpStatusCode.OK);
        ligada.GetProperty("estaLigada").GetBoolean().Should().BeTrue();
        ligada.GetProperty("agenda").GetString().Should().Be("todo dia às 06:30");
        ligada.GetProperty("pendencia").ValueKind.Should().Be(JsonValueKind.Null);

        var naFila = await ComStatusAsync(await admin.PostAsync($"{rotina}/execucao", null), HttpStatusCode.OK);
        naFila.GetProperty("naFila").GetBoolean().Should().BeTrue();
        naFila.GetProperty("execucaoPedidaPor").GetString().Should().Be(ApiEmMemoria.UsuarioDeRibeirao);

        await using var db = Sistema(app);
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(Rotina)).Select(a => a.Campo).ToListAsync();
        trilha.Should().Contain(["EstaLigada", "Hora"], "quem mudou a agenda e ligou a rotina fica registrado");
    }

    [Fact]
    public async Task A_api_monitorada_nasce_pela_tela_e_o_testar_guarda_quem_testou()
    {
        var internet = new RespostaSimulada(HttpStatusCode.NoContent);
        await using var app = await ComAdministradorAsync(internet);
        var admin = app.ClienteDeRibeirao();

        var criada = await ComStatusAsync(await admin.PostAsJsonAsync($"{Base}/conexoes", new
        {
            codigo = "clima_tempo", nome = "Clima", endereco = "https://api.exemplo.invalid/saude",
            nomeDoCabecalho = "X-Api-Key", statusEsperado = 200, minutosEntreVerificacoes = 30
        }, Json), HttpStatusCode.Created);
        criada.GetProperty("codigo").GetString().Should().Be("CLIMA_TEMPO");
        await admin.PutAsJsonAsync($"{Base}/conexoes/CLIMA_TEMPO/segredo", new { segredo = Senha }, Json);

        var teste = await ComStatusAsync(await admin.PostAsync($"{Base}/conexoes/CLIMA_TEMPO/teste", null), HttpStatusCode.OK);
        teste.GetProperty("ok").GetBoolean().Should().BeFalse("respondeu 204 e o esperado era 200");
        teste.GetProperty("resumo").GetString().Should().Contain("204").And.NotContain(Senha);
        internet.Pedidos.Single().Headers.GetValues("X-Api-Key").Single().Should().Be(Senha, "o segredo vai no cabeçalho cadastrado, e só nele");

        var historico = await ComStatusAsync(await admin.GetAsync($"{Base}/conexoes/CLIMA_TEMPO/verificacoes"), HttpStatusCode.OK);
        var verificacao = historico.GetProperty("dados").EnumerateArray().Single();
        verificacao.GetProperty("verificadaPor").GetString().Should().Be(ApiEmMemoria.UsuarioDeRibeirao);
        verificacao.GetProperty("ok").GetBoolean().Should().BeFalse();

        (await admin.PostAsJsonAsync($"{Base}/conexoes", new { codigo = "CLIMA_TEMPO", nome = "De novo", endereco = "https://api.exemplo.invalid" }, Json))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, "o código já existe");
        (await admin.PostAsync($"{Base}/conexoes/{ConexoesDoSistema.Protheus}/desativacao", null))
            .StatusCode.Should().Be(HttpStatusCode.Conflict, "as do sistema não saem do monitoramento: desliga-se a rotina");
    }

    [Fact]
    public async Task A_fonte_publica_que_redireciona_esta_no_ar_e_a_conexao_sem_credencial_nem_e_consultada()
    {
        var internet = new RespostaSimulada(HttpStatusCode.Found);
        await using var app = await ComAdministradorAsync(internet);
        var admin = app.ClienteDeRibeirao();

        var ibge = await ComStatusAsync(await admin.PostAsync($"{Base}/conexoes/IBGE_SIDRA/teste", null), HttpStatusCode.OK);
        ibge.GetProperty("ok").GetBoolean().Should().BeTrue();
        internet.Pedidos.Single().RequestUri!.ToString().Should().Be(ConexoesDoSistema.Todas.Single(c => c.Codigo == "IBGE_SIDRA").Endereco);

        var art = await ComStatusAsync(await admin.PostAsync($"{Base}/conexoes/{ConexoesDoSistema.Art}/teste", null), HttpStatusCode.OK);
        art.GetProperty("ok").GetBoolean().Should().BeFalse();
        art.GetProperty("resumo").GetString().Should().Contain("Sem credencial");
        internet.Pedidos.Should().HaveCount(1, "sem credencial nada é consultado");
    }
}
