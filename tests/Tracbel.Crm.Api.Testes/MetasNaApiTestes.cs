using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A META DE VENDA × O REALIZADO POR HTTP (#138), com a aplicação inteira de pé.
///
/// <para>O cenário semeado, em nov/2025 a ago/2026 — um período explícito, para o teste não depender do dia em que roda:
/// em Ribeirão, a meta do CEN (que tem conta), a de um consultor SEM conta, a de consórcio (à parte), uma fora do período
/// e uma excluída; em Barretos, a meta do CEN de lá. As vendas: a do CEN, a do consultor sem conta, uma sem vendedor numa
/// linha sem meta, uma do ano anterior e a de Barretos. E duas vendas do ART pendentes: uma de Ribeirão, uma de unidade
/// sem filial.</para>
///
/// <para>O que se prende: o Padrão vê SÓ a própria meta (D-M5); a Gerência vê a filial, com a lacuna em número; o
/// consórcio fica à parte; o mesmo trecho do ano anterior traz só o realizado; o período sem parâmetro é o ano fiscal até
/// o último mês fechado, com o mês em curso à parte; e a recusa por parâmetro e por permissão.</para>
/// </summary>
[Trait("Categoria", "Metas")]
public sealed class MetasNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/relatorios/metas";
    private const string Periodo = "?competenciaInicial=2025-11&competenciaFinal=2026-08";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static IEnumerable<string?> Lacunas(JsonElement dados) =>
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString());

    private static DadosDaMetaNaOrigem Meta(int empresa, int ano, int mes, string linha, string consultor, long? conta, int quantidade,
        OrigemDaMeta origem = OrigemDaMeta.Campanha, int? linhaDeProduto = null) => new(
        empresa, new DateOnly(ano, mes, 1), linha, CodigoEstavel.De(linha, 60), linhaDeProduto, consultor, conta, false, origem, quantidade,
        450_000m, null, $"h-{empresa}-{ano}-{mes}-{linha}-{consultor}");

    private static DadosDaVendaNaOrigem Venda(int empresa, DateOnly vendidaEm, string linha, string? vendedor, string hash) => new(
        empresa, null, vendidaEm, vendidaEm, null, null, "P", "NF", "P", "Varejo", false, false, 1, linha, "TR 6110J", "Agro Norte",
        "Ribeirão Preto", null, hash, null, vendedor);

    internal static async Task SemearAsync(ApiEmMemoria app)
    {
        using var escopo = app.Services.CreateScope();
        await using var db = new CrmDbContext(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);
        if (await db.MetasDeVenda.AnyAsync()) return;

        var gn = Sistema.Criar(ConexoesDoSistema.GestaoDeNegocios, "Gestão de Negócios — API", "teste");
        var art = Sistema.Criar(ConexoesDoSistema.Art, "ART — vendas de máquina", "teste");
        db.Sistemas.AddRange(gn, art);
        var medio = LinhaDeProduto.Criar("TRATOR_MEDIO_METAS", "Trator médio", null, PorteDeMaquina.Medio);
        db.LinhasDeProduto.Add(medio);
        await db.SaveChangesAsync();

        var leitura = new LeituraDaMeta(new DateTime(2026, 9, 27, 9, 0, 0, DateTimeKind.Utc), null, null);
        var metas = new[]
        {
            MetaDeVenda.Registrar(gn.Id, 1, Meta(1, 2025, 11, "TRATOR MÉDIO", "CEN.RIBEIRAOPRETO", 100, 3, linhaDeProduto: medio.Id), leitura, 100),
            MetaDeVenda.Registrar(gn.Id, 2, Meta(1, 2025, 12, "TRATOR MÉDIO", "OUTRO.SEM.CONTA", null, 2, linhaDeProduto: medio.Id), leitura, 100),
            MetaDeVenda.Registrar(gn.Id, 3, Meta(1, 2026, 1, "CONSÓRCIO", "CEN.RIBEIRAOPRETO", 100, 5, OrigemDaMeta.Consorcio), leitura, 100),
            MetaDeVenda.Registrar(gn.Id, 4, Meta(2, 2025, 11, "TRATOR MÉDIO", "CEN.BARRETOS", 200, 4, linhaDeProduto: medio.Id), leitura, 100),
            MetaDeVenda.Registrar(gn.Id, 5, Meta(1, 2026, 10, "TRATOR MÉDIO", "CEN.RIBEIRAOPRETO", 100, 9, linhaDeProduto: medio.Id), leitura, 100),
            MetaDeVenda.Registrar(gn.Id, 6, Meta(1, 2025, 11, "TRATOR MÉDIO", "CEN.RIBEIRAOPRETO", 100, 100, linhaDeProduto: medio.Id), leitura, 100)
        };
        metas[5].Excluir(100);
        db.MetasDeVenda.AddRange(metas);

        var comprador = Cliente.Criar(1, "Comprador das metas", TipoDePessoa.Juridica, 100, 100,
            documento: CpfCnpj.Criar("11444777000161"), situacao: SituacaoDoCliente.Cliente);
        db.Clientes.Add(comprador);
        await db.SaveChangesAsync();

        var vendas = new (int Empresa, DateOnly Data, string Linha, string? Vendedor, string Chassi)[]
        {
            (1, new DateOnly(2025, 11, 10), "TRATOR MÉDIO", "cen.ribeiraopreto", "1MET4S00000000001"),
            (1, new DateOnly(2025, 12, 5), "TRATOR MÉDIO", "Outro Sem-Conta", "1MET4S00000000002"),
            (1, new DateOnly(2026, 2, 1), "COLHEDORA CANA CH 750", null, "1MET4S00000000003"),
            (1, new DateOnly(2024, 11, 20), "TRATOR MÉDIO", "cen.ribeiraopreto", "1MET4S00000000004"),
            (2, new DateOnly(2025, 11, 15), "TRATOR MÉDIO", "cen.barretos", "1MET4S00000000005")
        };
        foreach (var (empresa, data, linha, vendedor, chassi) in vendas)
        {
            var maquina = Equipamento.RegistrarPelaIntegracao(empresa, Chassi.Criar(chassi), OrigemDoEquipamento.Art, 100);
            db.Equipamentos.Add(maquina);
            await db.SaveChangesAsync();
            db.VendasDeMaquina.Add(VendaDeMaquina.Registrar(art.Id, chassi, maquina.Id, comprador.Id, Venda(empresa, data, linha, vendedor, chassi),
                DateTime.UtcNow, 100));
        }

        // AS VENDAS DO ART QUE O CRM AINDA NÃO TEM: uma de Ribeirão, uma de unidade sem filial.
        var unidade = CorrespondenciaDaOrigem.Registrar(art.Id, TipoDeCorrespondencia.Unidade, "RIBEIRAO_PRETO", "Ribeirão Preto", null, DateTime.UtcNow);
        unidade.Avaliar(SituacaoDaCorrespondencia.CorrespondenciaExata, null, null, 1, "nome idêntico");
        db.CorrespondenciasDaOrigem.Add(unidade);
        db.RegistrosDeOrigem.AddRange(
            RegistroDeOrigem.Registrar(art.Id, MetaDeVenda.FluxoDasVendasDoArt, "9001",
                new RetratoDoRegistroDeOrigem("p1", null, "TRATOR MÉDIO", "TR", "Ribeirão Preto", new DateOnly(2026, 3, 1), null), DateTime.UtcNow),
            RegistroDeOrigem.Registrar(art.Id, MetaDeVenda.FluxoDasVendasDoArt, "9002",
                new RetratoDoRegistroDeOrigem("p2", null, "TRATOR MÉDIO", "TR", "Unidade Desconhecida", new DateOnly(2026, 3, 1), null), DateTime.UtcNow));

        db.PontosDeSincronismo.Add(PontoDeSincronismo.Criar(gn.Id, MetaDeVenda.FluxoDaCarga, "2026-09-27T04:30:00.0000000Z"));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task O_padrao_ve_so_a_propria_meta_e_as_proprias_vendas()
    {
        await SemearAsync(api);
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota + Periodo));

        dados.GetProperty("alcance").GetString().Should().Be("Proprios");
        var totais = dados.GetProperty("totais");
        totais.GetProperty("metaMaquinas").GetInt32().Should().Be(3, "só a meta dele — a excluída e a de fora do período não entram");
        totais.GetProperty("realizadoMaquinas").GetInt32().Should().Be(1, "só a venda em que ele é o vendedor");
        totais.GetProperty("metaConsorcio").GetInt32().Should().Be(5, "o consórcio fica à parte, em cotas");
        totais.GetProperty("pendentesNoArt").ValueKind.Should().Be(JsonValueKind.Null, "a pendente ainda não tem vendedor");

        dados.GetProperty("porConsultor").EnumerateArray().Select(c => c.GetProperty("consultor").GetString())
            .Should().Equal("CEN.RIBEIRAOPRETO");
        dados.GetProperty("mesmoTrechoDoFyAnterior").GetProperty("realizadoMaquinas").GetInt32().Should().Be(1);
        Lacunas(dados).Should().Contain(["pendentesNoArt", "vendasPelaFilial", "consorcio", "metaDoAnoAnterior", "previsao"]);
        Lacunas(dados).Should().NotContain("loginSemCasamento", "a meta dele está casada com a conta");
        Lacunas(dados).Should().NotContain("consultoresSemConta", "no alcance Próprios não há consultor alheio");
    }

    [Fact]
    public async Task A_gerencia_ve_a_filial_inteira_com_a_lacuna_em_numero()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await SemearAsync(app);
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota + Periodo));

        dados.GetProperty("alcance").GetString().Should().Be("Filial");
        var totais = dados.GetProperty("totais");
        totais.GetProperty("metaMaquinas").GetInt32().Should().Be(5, "3 do CEN + 2 do consultor sem conta; a de Barretos é de lá");
        totais.GetProperty("realizadoMaquinas").GetInt32().Should().Be(3, "as três vendas de Ribeirão no período, com e sem vendedor");
        totais.GetProperty("pendentesNoArt").GetInt32().Should().Be(1, "a pendente de unidade sem filial não é de filial nenhuma");
        totais.GetProperty("metaConsorcio").GetInt32().Should().Be(5);

        var linhas = dados.GetProperty("porLinha").EnumerateArray().ToDictionary(l => l.GetProperty("codigo").GetString()!);
        linhas["TRATOR_MEDIO"].GetProperty("meta").GetInt32().Should().Be(5);
        linhas["TRATOR_MEDIO"].GetProperty("realizado").GetInt32().Should().Be(2);
        linhas["COLHEDORA_CANA_CH_750"].GetProperty("meta").GetInt32().Should().Be(0, "a linha só tem venda — meta zero, e aparece");
        linhas.Should().NotContainKey("CONSORCIO", "o consórcio não entra nas linhas de máquinas");

        var consultores = dados.GetProperty("porConsultor").EnumerateArray().ToDictionary(c => c.GetProperty("consultor").GetString()!);
        consultores["OUTRO.SEM.CONTA"].GetProperty("temConta").GetBoolean().Should().BeFalse();
        consultores["OUTRO.SEM.CONTA"].GetProperty("realizado").GetInt32().Should().Be(1, "o vendedor do ART (\"Outro Sem-Conta\") casa com o consultor pela chave da pessoa");
        consultores["CEN.RIBEIRAOPRETO"].GetProperty("temConta").GetBoolean().Should().BeTrue();

        var meses = dados.GetProperty("porMes").EnumerateArray().ToList();
        meses.Should().HaveCount(10, "nov/2025 a ago/2026");
        meses[0].GetProperty("metaMaquinas").GetInt32().Should().Be(3);
        meses[0].GetProperty("realizadoMaquinas").GetInt32().Should().Be(1);

        Lacunas(dados).Should().Contain(["pendentesNoArt", "pendentesSemFilial", "consultoresSemConta", "vendasSemVendedor"]);
        dados.GetProperty("origem").GetProperty("geradaNaOrigemEm").GetString().Should().StartWith("2026-09-27T04:30:00");
        dados.GetProperty("origem").GetProperty("rota").GetString().Should().Be("/api/v1/cadastros/metas");
    }

    [Fact]
    public async Task Sem_parametro_o_periodo_e_o_ano_fiscal_ate_o_ultimo_mes_fechado_com_o_mes_em_curso_a_parte()
    {
        await SemearAsync(api);
        // O MÊS CORRENTE É O DE SÃO PAULO (revisão do PR #248), e não o do UTC.
        var mesCorrente = Tracbel.Crm.Aplicacao.Relacionamento.ObterMetaERealizado.MesCorrenteEmSaoPaulo(DateTime.UtcNow);
        var ultimoFechado = mesCorrente.AddMonths(-1);
        var inicioDoAno = new DateOnly(ultimoFechado.Month >= 11 ? ultimoFechado.Year : ultimoFechado.Year - 1, 11, 1);

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var periodo = dados.GetProperty("periodo");

        periodo.GetProperty("ehOPadrao").GetBoolean().Should().BeTrue();
        periodo.GetProperty("inicial").GetString().Should().Be(inicioDoAno.ToString("yyyy-MM-dd"));
        periodo.GetProperty("final").GetString().Should().Be(ultimoFechado.ToString("yyyy-MM-dd"), "o mês em curso fica fora da comparação");
        periodo.GetProperty("inicialDoAnterior").GetString().Should().Be(inicioDoAno.AddMonths(-12).ToString("yyyy-MM-dd"));
        dados.GetProperty("mesEmCurso").GetProperty("competencia").GetString().Should().Be(mesCorrente.ToString("yyyy-MM-dd"));
        Lacunas(dados).Should().Contain("mesEmCurso");
    }

    [Fact]
    public async Task No_alcance_proprios_o_login_que_nao_casa_diz_isso_e_nao_sem_meta()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await SemearAsync(app);

        // A CONTA DO CEN NÃO CASA COM NADA: a carga não achou a conta dele na GN, e o ART escreve outro nome.
        using (var escopo = app.Services.CreateScope())
        await using (var db = new CrmDbContext(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia))
        {
            await db.MetasDeVenda.Where(m => m.ConsultorUsuarioId == 100).ExecuteUpdateAsync(s => s.SetProperty(m => m.ConsultorUsuarioId, (long?)null));
            await db.VendasDeMaquina.Where(v => v.VendedorNaOrigem == "cen.ribeiraopreto")
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.VendedorNaOrigem, "outra.pessoa"));
        }

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota + Periodo));

        dados.GetProperty("totais").GetProperty("metaMaquinas").GetInt32().Should().Be(0);
        Lacunas(dados).Should().Contain("loginSemCasamento").And.NotContain("semMetaNoPeriodo",
            "o zero é falta de casamento do login, e não falta de meta");
    }

    [Theory]
    [InlineData("?competenciaInicial=2025-11", "competenciaFinal")]
    [InlineData("?competenciaInicial=2025-13&competenciaFinal=2026-08", "competenciaInicial")]
    [InlineData("?competenciaInicial=2026-08&competenciaFinal=2025-11", "competenciaFinal")]
    [InlineData("?competenciaInicial=2019-11&competenciaFinal=2020-08", "competenciaInicial")]
    [InlineData("?competenciaInicial=2022-01&competenciaFinal=2026-08", "competenciaFinal")]
    public async Task Periodo_que_nao_vale_e_recusado_com_o_campo_nomeado(string consulta, string campo)
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync(Rota + consulta);

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("erros")
            .EnumerateArray().Select(e => e.GetProperty("campo").GetString()).Should().Contain(campo);
    }

    [Fact]
    public async Task Sem_Meta_Ler_a_rota_recusa_com_403()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();

        // O ADMINISTRADOR TIROU Meta.Ler DO PERFIL PADRÃO: a linha 118 sai, e quem só tem o padrão não vê meta nenhuma.
        using (var escopo = app.Services.CreateScope())
        await using (var db = new CrmDbContext(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia))
        {
            await db.PerfisPermissoes.Where(p => p.Id == 118).ExecuteDeleteAsync();
        }

        (await app.ClienteDeRibeirao().GetAsync(Rota)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
