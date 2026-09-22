using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// OS DADOS DO ART NAS TELAS EXISTENTES, POR HTTP (documento 35, seções 10 e 11), com a aplicação
/// inteira de pé.
///
/// <para>Não há rota exclusiva do ART: a classificação e a última venda saem na lista de máquinas, o
/// histórico comercial e as divergências abertas na ficha da máquina, as máquinas compradas na ficha do
/// cliente, e o registro das execuções do serviço na administração.</para>
///
/// <para>O cenário semeado: a máquina incluída pela venda, sem dono; a máquina que já existia no CRM,
/// com dono e duas vendas (a nova e a revenda como usada); a divergência entre o comprador e o dono; e
/// duas execuções do serviço, uma com falha e outra com sucesso.</para>
///
/// <para><b>Nome, documento e chassi aqui são inventados</b> (CPF e CNPJ de exemplo, com dígito
/// verificador válido).</para>
/// </summary>
[Trait("Categoria", "IntegracaoDoArt")]
public sealed class IntegracaoDoArtTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string MaquinaNova = "1ABCD23EFGH456789";
    private const string MaquinaDoCrm = "9ZXY876WVUT543210";
    private const string MaquinaSemClassificacao = "8KLMN45PRST678901";
    private const string MaquinaDeBarretos = "1JD8R340ABC999999";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static DadosDaVendaNaOrigem Dados(DateOnly vendidaEm, string linha, string produto, string gestao, string hash) => new(
        EmpresaId: 1, EmpresaDoFaturamentoId: null, VendidaEm: vendidaEm, FaturadaEm: vendidaEm.AddDays(2), EntregueEm: null,
        RegistradaNaOrigemEm: null, NumeroDoPedido: "123", NumeroDaNotaFiscal: "4567", SituacaoNaOrigem: "P", GestaoNaOrigem: gestao,
        VendaDireta: false, RepasseDireto: false, Quantidade: 1, LinhaNaOrigem: linha, ProdutoNaOrigem: produto,
        EmpresaNaOrigem: "Agro Norte", UnidadeNaOrigem: "Ribeirão Preto", UnidadeDoFaturamentoNaOrigem: "Ribeirão Preto",
        HashDaOrigem: hash, Transformacoes: null);

    private CrmDbContext AbrirBanco(IServiceScope escopo) =>
        new(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private async Task<Guid> ChaveDoClienteAsync(string nome)
    {
        using var escopo = api.Services.CreateScope();
        await using var db = AbrirBanco(escopo);
        return await db.Clientes.Where(c => c.NomeRazao == nome).Select(c => c.ChavePublica).SingleAsync();
    }

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        await using var db = AbrirBanco(escopo);

        if (await db.Sistemas.AnyAsync(s => s.Codigo == "ART")) return;

        var sistema = Sistema.Criar("ART", "ART — vendas de máquina", "teste");
        db.Sistemas.Add(sistema);

        var medio = LinhaDeProduto.Criar("TRATOR_MEDIO", "Trator médio", null, PorteDeMaquina.Medio);
        var pulverizador = LinhaDeProduto.Criar("PULVERIZADOR", "Pulverizador", null, PorteDeMaquina.NaoSeAplica);
        db.LinhasDeProduto.AddRange(medio, pulverizador);

        var comprador = Cliente.Criar(1, "Comprador da venda", TipoDePessoa.Juridica, 100, 100,
            documento: CpfCnpj.Criar("11222333000181"), situacao: SituacaoDoCliente.Cliente);
        var dono = Cliente.Criar(1, "Dono no CRM", TipoDePessoa.Fisica, 100, 100,
            documento: CpfCnpj.Criar("52998224725"), situacao: SituacaoDoCliente.Cliente);
        db.Clientes.AddRange(comprador, dono);
        await db.SaveChangesAsync();

        var modeloId = await db.Modelos.Select(m => m.Id).FirstAsync();

        var nova = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(MaquinaNova), OrigemDoEquipamento.Art, 100, linhaDeProdutoId: medio.Id);
        var semClassificacao = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(MaquinaSemClassificacao), OrigemDoEquipamento.Art, 100);
        var doCrm = Equipamento.Criar(1, modeloId, Chassi.Criar(MaquinaDoCrm), OrigemDoEquipamento.Crm, 100, clienteId: dono.Id);
        doCrm.ClassificarSeAusente(pulverizador.Id, 100);
        var deBarretos = Equipamento.Criar(2, modeloId, Chassi.Criar(MaquinaDeBarretos), OrigemDoEquipamento.Crm, 100,
            situacao: SituacaoDoEquipamento.Estoque);
        db.Equipamentos.AddRange(nova, semClassificacao, doCrm, deBarretos);
        await db.SaveChangesAsync();

        var agora = DateTime.UtcNow;
        var vendaNova = VendaDeMaquina.Registrar(sistema.Id, "1001", nova.Id, comprador.Id,
            Dados(new DateOnly(2025, 9, 16), "TRATOR MÉDIO", "TR 6135M", "Grandes Contas", "h1"), agora, 100);
        var primeiraDoCrm = VendaDeMaquina.Registrar(sistema.Id, "1002", doCrm.Id, comprador.Id,
            Dados(new DateOnly(2024, 6, 12), "PULVERIZADOR", "PV M4030", "Varejo", "h2"), agora, 100);
        var revendaDoCrm = VendaDeMaquina.Registrar(sistema.Id, "1003", doCrm.Id, dono.Id,
            Dados(new DateOnly(2025, 9, 18), "USADOS", "Usado Outras Marcas", "Varejo", "h3"), agora, 100);
        db.VendasDeMaquina.AddRange(vendaNova, primeiraDoCrm, revendaDoCrm);
        await db.SaveChangesAsync();

        foreach (var venda in new[] { vendaNova, primeiraDoCrm, revendaDoCrm })
            db.VinculosComEquipamento.Add(VinculoDeClienteComEquipamento.RegistrarCompradorNaVenda(
                venda.EmpresaId, venda.CompradorId, venda.EquipamentoId, venda.Id, sistema.Id, venda.VendidaEm, 100));

        db.DivergenciasDeIntegracao.Add(DivergenciaDeIntegracao.Registrar(
            1, sistema.Id, TipoDeDivergencia.CompradorDiferenteDoProprietarioNoCrm, "1002", doCrm.Id, primeiraDoCrm.Id,
            "O comprador da venda não é o dono registrado no CRM.", "cliente do CRM", "comprador do ART", null, agora, 100));

        var falhou = ExecucaoDeSincronizacao.Iniciar(sistema.Id, "ART.VENDA_DE_MAQUINA", "SERVIDOR", agora.AddHours(-2));
        falhou.Falhar(3, "O ART não respondeu à leitura (erro MySQL 1045). Nada foi gravado.", agora.AddHours(-2).AddMinutes(3));
        var deuCerto = ExecucaoDeSincronizacao.Iniciar(sistema.Id, "ART.VENDA_DE_MAQUINA", "SERVIDOR", agora.AddHours(-1));
        deuCerto.Concluir(2, 4, 3, 0, 1, "4 registros lidos; 3 vendas incluídas.", agora.AddHours(-1).AddMinutes(2));
        db.ExecucoesDeSincronizacao.AddRange(falhou, deuCerto);

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Nao_ha_rota_exclusiva_do_art()
    {
        await SemearAsync();

        (await api.ClienteDeRibeirao().GetAsync("/api/v1/integracoes/art/resumo")).StatusCode.Should().Be(HttpStatusCode.NotFound,
            "os dados do ART aparecem nas telas de clientes, máquinas e administração, e não numa página própria");
    }

    [Fact]
    public async Task A_classificacao_e_o_porte_filtram_no_banco_e_a_lista_mostra_o_comprador_da_venda_sem_torna_lo_dono()
    {
        await SemearAsync();
        var http = api.ClienteDeRibeirao();

        var medios = await DadosAsync(await http.GetAsync("/api/v1/equipamentos?linhaDeProduto=TRATOR_MEDIO"));
        medios.GetProperty("total").GetInt32().Should().Be(1);

        var maquina = medios.GetProperty("itens")[0];
        maquina.GetProperty("chassi").GetString().Should().Be(MaquinaNova);
        maquina.GetProperty("classificacaoNome").GetString().Should().Be("Trator médio");
        maquina.GetProperty("porte").GetString().Should().Be("Medio");
        maquina.GetProperty("origem").GetString().Should().Be("Art");
        maquina.GetProperty("situacao").GetString().Should().Be("ProprietarioNaoConfirmado");
        maquina.GetProperty("clienteChave").ValueKind.Should().Be(JsonValueKind.Null, "o comprador de uma venda não vira dono");
        maquina.GetProperty("compradorNaUltimaVendaNome").GetString().Should().Be("Comprador da venda");
        maquina.GetProperty("naturezaDoVinculo").GetString().Should().Be("CompradorNaVenda");
        maquina.GetProperty("ultimaVendaEm").GetString().Should().Be("2025-09-16");
        maquina.GetProperty("produtoNaOrigem").GetString().Should().Be("TR 6135M");
        maquina.GetProperty("sistemaDaVenda").GetString().Should().Be("ART");

        (await DadosAsync(await http.GetAsync("/api/v1/equipamentos?porte=Medio"))).GetProperty("total").GetInt32().Should().Be(1);
        (await DadosAsync(await http.GetAsync("/api/v1/equipamentos?somenteComVenda=true"))).GetProperty("total").GetInt32().Should().Be(2);
        (await DadosAsync(await http.GetAsync("/api/v1/equipamentos?linhaDeProduto=SEM_CLASSIFICACAO"))).GetProperty("total").GetInt32().Should().Be(1);
        (await DadosAsync(await api.ClienteDeBarretos().GetAsync("/api/v1/equipamentos?somenteComVenda=true")))
            .GetProperty("total").GetInt32().Should().Be(0, "as vendas e as máquinas são de Ribeirão");

        (await http.GetAsync("/api/v1/equipamentos?linhaDeProduto=INVENTADA")).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await http.GetAsync("/api/v1/equipamentos?porte=Gigante")).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        (await DadosAsync(await http.GetAsync("/api/v1/catalogos/LINHA_DE_PRODUTO")))[0].GetProperty("itens").GetArrayLength().Should().Be(2);
        (await DadosAsync(await http.GetAsync("/api/v1/catalogos/PORTE_DE_MAQUINA")))[0].GetProperty("itens").GetArrayLength().Should().Be(4);
    }

    [Fact]
    public async Task A_maquina_ja_existente_mantem_o_dono_e_o_historico_comercial_mostra_as_duas_vendas()
    {
        await SemearAsync();
        var http = api.ClienteDeRibeirao();

        var maquina = (await DadosAsync(await http.GetAsync($"/api/v1/equipamentos?termo={MaquinaDoCrm}"))).GetProperty("itens")[0];
        maquina.GetProperty("clienteNome").GetString().Should().Be("Dono no CRM", "o dono existente não é sobrescrito");
        maquina.GetProperty("vendas").GetInt32().Should().Be(2);
        maquina.GetProperty("compradorNaUltimaVendaNome").GetString().Should().Be("Dono no CRM");

        var vendas = await DadosAsync(await http.GetAsync($"/api/v1/equipamentos/{maquina.GetProperty("chave").GetGuid()}/vendas"));
        vendas.GetArrayLength().Should().Be(2);
        vendas[0].GetProperty("chaveOrigem").GetString().Should().Be("1003", "a venda mais recente vem primeiro");
        vendas[0].GetProperty("linhaNaOrigem").GetString().Should().Be("USADOS");
        vendas[1].GetProperty("compradorNome").GetString().Should().Be("Comprador da venda");
        vendas[1].GetProperty("natureza").GetString().Should().Be("CompradorNaVenda");
        vendas[1].GetProperty("sistemaCodigo").GetString().Should().Be("ART");
        vendas[1].GetProperty("filialCodigo").GetString().Should().Be(ApiEmMemoria.FilialDeRibeirao);
        vendas[1].GetProperty("vendidaEm").GetString().Should().Be("2024-06-12");
    }

    [Fact]
    public async Task O_historico_comercial_de_maquina_de_outra_filial_responde_404()
    {
        await SemearAsync();

        var chave = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos?termo={MaquinaNova}")))
            .GetProperty("itens")[0].GetProperty("chave").GetGuid();

        (await api.ClienteDeBarretos().GetAsync($"/api/v1/equipamentos/{chave}/vendas")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_ficha_da_maquina_traz_a_divergencia_aberta_so_da_maquina_que_a_tem()
    {
        await SemearAsync();
        var http = api.ClienteDeRibeirao();

        async Task<JsonElement> FichaAsync(string chassi)
        {
            var chave = (await DadosAsync(await http.GetAsync($"/api/v1/equipamentos?termo={chassi}")))
                .GetProperty("itens")[0].GetProperty("chave").GetGuid();
            return await DadosAsync(await http.GetAsync($"/api/v1/equipamentos/{chave}"));
        }

        var doCrm = (await FichaAsync(MaquinaDoCrm)).GetProperty("divergenciasAbertas");
        doCrm.GetArrayLength().Should().Be(1);
        doCrm[0].GetProperty("tipo").GetString().Should().Be("CompradorDiferenteDoProprietarioNoCrm");
        doCrm[0].GetProperty("descricao").GetString().Should().Be("O comprador da venda não é o dono registrado no CRM.");

        (await FichaAsync(MaquinaNova)).GetProperty("divergenciasAbertas").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task A_ficha_do_cliente_lista_as_maquinas_compradas_sem_confundir_comprador_com_dono()
    {
        await SemearAsync();
        var http = api.ClienteDeRibeirao();

        var comprador = await ChaveDoClienteAsync("Comprador da venda");
        var compradas = await DadosAsync(await http.GetAsync($"/api/v1/clientes/{comprador}/maquinas-compradas"));
        compradas.GetArrayLength().Should().Be(2);
        compradas[0].GetProperty("chassi").GetString().Should().Be(MaquinaNova, "a venda mais recente vem primeiro");
        compradas[0].GetProperty("vendidaEm").GetString().Should().Be("2025-09-16");
        compradas[0].GetProperty("classificacaoNome").GetString().Should().Be("Trator médio");
        compradas[0].GetProperty("natureza").GetString().Should().Be("CompradorNaVenda");
        compradas[0].GetProperty("sistemaCodigo").GetString().Should().Be("ART");
        compradas[0].GetProperty("filialCodigo").GetString().Should().Be(ApiEmMemoria.FilialDeRibeirao);
        compradas[0].GetProperty("ehDonoAtual").GetBoolean().Should().BeFalse("a máquina do ART está sem dono confirmado");
        compradas[1].GetProperty("chassi").GetString().Should().Be(MaquinaDoCrm);
        compradas[1].GetProperty("ehDonoAtual").GetBoolean().Should().BeFalse("quem comprou em 2024 não é o dono registrado");

        var dono = await ChaveDoClienteAsync("Dono no CRM");
        var doDono = await DadosAsync(await http.GetAsync($"/api/v1/clientes/{dono}/maquinas-compradas"));
        doDono.GetArrayLength().Should().Be(1);
        doDono[0].GetProperty("ehDonoAtual").GetBoolean().Should().BeTrue();

        (await api.ClienteDeBarretos().GetAsync($"/api/v1/clientes/{comprador}/maquinas-compradas"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound, "o cliente é de Ribeirão");
        (await http.GetAsync($"/api/v1/clientes/{Guid.NewGuid()}/maquinas-compradas")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_administracao_mostra_a_ultima_execucao_o_ultimo_sucesso_e_a_falha_com_o_motivo()
    {
        await SemearAsync();

        // A SITUAÇÃO DAS INTEGRAÇÕES É DA GERÊNCIA PARA CIMA (issue 134): o perfil padrão não a vê mais.
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Gerencia);
        var http = api.ClienteDeRibeirao();

        var fluxos = await DadosAsync(await http.GetAsync("/api/v1/integracoes/sincronizacoes"));
        var art = fluxos.EnumerateArray().Single(f => f.GetProperty("fluxo").GetString() == "ART.VENDA_DE_MAQUINA");
        art.GetProperty("sistemaCodigo").GetString().Should().Be("ART");
        art.GetProperty("ultimoResultado").GetString().Should().Be("Sucesso");
        art.GetProperty("ultimoSucessoEm").ValueKind.Should().Be(JsonValueKind.String);

        var execucoes = art.GetProperty("execucoes");
        execucoes.GetArrayLength().Should().Be(2);
        execucoes[0].GetProperty("resultado").GetString().Should().Be("Sucesso");
        execucoes[0].GetProperty("tentativas").GetInt32().Should().Be(2);
        execucoes[0].GetProperty("incluidos").GetInt32().Should().Be(3);
        execucoes[1].GetProperty("resultado").GetString().Should().Be("Falha");
        execucoes[1].GetProperty("mensagem").GetString().Should().Contain("erro MySQL 1045");

        (await DadosAsync(await http.GetAsync("/api/v1/integracoes/sincronizacoes?execucoes=1")))
            .EnumerateArray().Single(f => f.GetProperty("fluxo").GetString() == "ART.VENDA_DE_MAQUINA")
            .GetProperty("execucoes").GetArrayLength().Should().Be(1);
        (await http.GetAsync("/api/v1/integracoes/sincronizacoes?execucoes=0")).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task A_classificacao_entra_no_cadastro_e_a_maquina_do_art_sem_modelo_pode_ser_editada()
    {
        await SemearAsync();
        var http = api.ClienteDeRibeirao();

        var chave = (await DadosAsync(await http.GetAsync($"/api/v1/equipamentos?termo={MaquinaSemClassificacao}")))
            .GetProperty("itens")[0].GetProperty("chave").GetGuid();

        var classificada = await CorpoAsync(await http.PutAsJsonAsync($"/api/v1/equipamentos/{chave}",
            new { linhaDeProdutoCodigo = "PULVERIZADOR" }, Json));
        classificada.GetProperty("classificacaoCodigo").GetString().Should().Be("PULVERIZADOR");
        classificada.GetProperty("modeloCodigo").ValueKind.Should().Be(JsonValueKind.Null,
            "a máquina do ART sem correspondência segura continua sem modelo — ninguém precisa inventar um");
        classificada.GetProperty("situacao").GetString().Should().Be("ProprietarioNaoConfirmado");

        (await http.PutAsJsonAsync($"/api/v1/equipamentos/{chave}", new { linhaDeProdutoCodigo = "INVENTADA" }, Json))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var semClassificacao = await CorpoAsync(await http.PutAsJsonAsync($"/api/v1/equipamentos/{chave}",
            new { linhaDeProdutoCodigo = "" }, Json));
        semClassificacao.GetProperty("classificacaoCodigo").ValueKind.Should().Be(JsonValueKind.Null, "texto vazio retira a classificação");
    }
}
