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
/// A FICHA 360 DO CLIENTE COM O DADO CRUZADO (27/09/2026), por HTTP, com a aplicação inteira de pé: a frota pelo dono
/// atual e pelas compras no ART, o faturamento do cliente e as carteiras dele.
///
/// <para>O cenário semeado cobre o que a ficha não pode confundir: o dono atual pela sincronia do parque (sem dono
/// confirmado) contra o dono confirmado; o comprador do ART que já não é o dono; a compra registrada pelo dono do
/// Protheus no lugar do comprador; o vínculo encerrado; a máquina do cliente cadastrada em outra filial (a fronteira não
/// abre); as notas de outra filial; a carteira de outra filial; e o cliente sem nada.</para>
///
/// <para><b>Nome, documento e chassi aqui são inventados.</b></para>
/// </summary>
[Trait("Categoria", "FichaDoCliente")]
public sealed class FichaDoClienteNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string PelaNota = "1FICHA360NOTA0001";
    private const string ConfirmadaNoArt = "1FICHA360CONF0002";
    private const string RevendidaAOutro = "1FICHA360REVE0003";
    private const string DeOutraFilial = "1FICHA360BARR0004";
    private const string SoDoOutro = "1FICHA360OUTR0005";
    private const string CompradaPeloDono = "1FICHA360SUBS0006";
    private const string DonoEncerrado = "1FICHA360ENCE0007";

    private static readonly DateTime Agora = DateTime.UtcNow;
    private static readonly DateOnly MesCorrente = new(Agora.Year, Agora.Month, 1);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static IEnumerable<string?> Lacunas(JsonElement dados) =>
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString());

    private static DadosDaVendaNaOrigem Venda(int empresaId, DateOnly vendidaEm, string hash) => new(
        EmpresaId: empresaId, EmpresaDoFaturamentoId: null, VendidaEm: vendidaEm, FaturadaEm: vendidaEm, EntregueEm: null,
        RegistradaNaOrigemEm: null, NumeroDoPedido: null, NumeroDaNotaFiscal: null, SituacaoNaOrigem: null, GestaoNaOrigem: null,
        VendaDireta: false, RepasseDireto: false, Quantidade: 1, LinhaNaOrigem: "TRATOR", ProdutoNaOrigem: "TR 6135M",
        EmpresaNaOrigem: null, UnidadeNaOrigem: null, UnidadeDoFaturamentoNaOrigem: null, HashDaOrigem: hash, Transformacoes: null);

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

        var art = Sistema.Criar("ART", "ART — vendas de máquina", "teste");
        var protheus = Sistema.Criar("PROTHEUS", "Protheus (TOTVS) — ERP", "teste");
        db.Sistemas.AddRange(art, protheus);

        var dono = Cliente.Criar(1, "Dono da frota", TipoDePessoa.Juridica, 100, 100,
            documento: CpfCnpj.Criar("11222333000181"), situacao: SituacaoDoCliente.Cliente);
        var outro = Cliente.Criar(1, "Outro dono", TipoDePessoa.Fisica, 100, 100,
            documento: CpfCnpj.Criar("52998224725"), situacao: SituacaoDoCliente.Cliente);
        var semNada = Cliente.Criar(1, "Cliente sem nada", TipoDePessoa.Juridica, 100, 100);
        var semClasse = Cliente.Criar(1, "Cliente sem classe", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.AddRange(dono, outro, semNada, semClasse);
        await db.SaveChangesAsync();

        dono.ApurarClasse(ClasseDeCliente.A, 1500m, Agora, 100);
        outro.ApurarClasse(ClasseDeCliente.C, 10m, Agora, 100);
        semNada.ApurarClasse(ClasseDeCliente.D, 0m, Agora, 100);

        // ---- as máquinas ----
        var modeloId = await db.Modelos.Select(m => m.Id).FirstAsync();
        var pelaNota = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(PelaNota), OrigemDoEquipamento.Protheus, 100);
        var confirmada = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(ConfirmadaNoArt), OrigemDoEquipamento.Art, 100);
        var revendida = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(RevendidaAOutro), OrigemDoEquipamento.Art, 100);
        var deOutraFilial = Equipamento.RegistrarPelaIntegracao(2, Chassi.Criar(DeOutraFilial), OrigemDoEquipamento.Protheus, 200);
        var soDoOutro = Equipamento.Criar(1, modeloId, Chassi.Criar(SoDoOutro), OrigemDoEquipamento.Crm, 100, clienteId: outro.Id);
        var compradaPeloDono = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(CompradaPeloDono), OrigemDoEquipamento.Art, 100);
        var encerrada = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(DonoEncerrado), OrigemDoEquipamento.Protheus, 100);
        db.Equipamentos.AddRange(pelaNota, confirmada, revendida, deOutraFilial, soDoOutro, compradaPeloDono, encerrada);
        await db.SaveChangesAsync();

        confirmada.ConfirmarProprietario(dono.Id, 100);

        var vendaConfirmada = VendaDeMaquina.Registrar(art.Id, "F1", confirmada.Id, dono.Id, Venda(1, new DateOnly(2024, 6, 12), "f1"), Agora, 100);
        var vendaRevendida = VendaDeMaquina.Registrar(art.Id, "F2", revendida.Id, dono.Id, Venda(1, new DateOnly(2023, 3, 20), "f2"), Agora, 100);
        var vendaSubstituida = VendaDeMaquina.Registrar(art.Id, "F3", compradaPeloDono.Id, dono.Id, Venda(1, new DateOnly(2025, 2, 1), "f3"), Agora, 100,
            compradorPeloDonoNoProtheus: true);
        db.VendasDeMaquina.AddRange(vendaConfirmada, vendaRevendida, vendaSubstituida);
        await db.SaveChangesAsync();

        foreach (var venda in new[] { vendaConfirmada, vendaRevendida, vendaSubstituida })
            db.VinculosComEquipamento.Add(VinculoDeClienteComEquipamento.RegistrarCompradorNaVenda(
                venda.EmpresaId, venda.CompradorId, venda.EquipamentoId, venda.Id, art.Id, venda.VendidaEm, 100));

        var vinculoEncerrado = VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
            1, dono.Id, encerrada.Id, protheus.Id, new DateOnly(2022, 1, 1), EvidenciaDoProprietario.CadastroAntigo, 100);
        vinculoEncerrado.Encerrar("O dono atual mudou.", Agora, 100);

        db.VinculosComEquipamento.AddRange(
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                1, dono.Id, pelaNota.Id, protheus.Id, new DateOnly(2025, 3, 10), EvidenciaDoProprietario.NotaDeVenda, 100),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                1, dono.Id, confirmada.Id, art.Id, new DateOnly(2024, 6, 12), EvidenciaDoProprietario.VendaNoArt, 100),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                1, outro.Id, revendida.Id, protheus.Id, new DateOnly(2025, 8, 1), EvidenciaDoProprietario.OrdemDeServico, 100),
            // O DONO É DE RIBEIRÃO, a máquina está cadastrada em Barretos — o vínculo fica na filial do cliente.
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                1, dono.Id, deOutraFilial.Id, protheus.Id, new DateOnly(2025, 5, 5), EvidenciaDoProprietario.OrdemDeServico, 100),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                1, outro.Id, soDoOutro.Id, protheus.Id, new DateOnly(2024, 1, 1), EvidenciaDoProprietario.CadastroAntigo, 100),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                1, dono.Id, compradaPeloDono.Id, protheus.Id, new DateOnly(2025, 2, 1), EvidenciaDoProprietario.NotaDeVenda, 100),
            vinculoEncerrado);

        // ---- o faturamento ----
        // A âncora é a competência mais recente AO ALCANCE, de qualquer cliente: a do "Outro dono", no mês corrente.
        db.FaturamentoDosClientes.AddRange(
            FaturamentoDoCliente.Criar(1, outro.Id, MesCorrente, 10m, 1, 1, new QuebraDoFaturamento(0m, 10m, 0m, 0m)),
            FaturamentoDoCliente.Criar(1, dono.Id, MesCorrente.AddMonths(-1), 1000m, 2, 4, new QuebraDoFaturamento(600m, 300m, 100m, 0m)),
            FaturamentoDoCliente.Criar(1, dono.Id, MesCorrente.AddMonths(-11), 500m, 1, 2, new QuebraDoFaturamento(0m, 500m, 0m, 0m)),
            FaturamentoDoCliente.Criar(1, dono.Id, MesCorrente.AddMonths(-14), 2000m, 1, 1, new QuebraDoFaturamento(2000m, 0m, 0m, 0m)),
            // A NOTA EMITIDA EM BARRETOS não aparece para quem está em Ribeirão: a fronteira é a filial da nota.
            FaturamentoDoCliente.Criar(2, dono.Id, MesCorrente.AddMonths(-2), 777m, 1, 1, new QuebraDoFaturamento(777m, 0m, 0m, 0m)));

        // ---- as carteiras ----
        var maquinas = LinhaDeNegocio.Criar("MAQ_FICHA", "Máquinas");
        maquinas.DeclararCadencia(180, 180, 180, 360);
        db.LinhasDeNegocio.Add(maquinas);
        await db.SaveChangesAsync();

        var comercial = Carteira.Criar(1, maquinas.Id, "MAQ_FICHA_RP", "Máquinas RP", 100, 100);
        var deposito = Carteira.Criar(1, maquinas.Id, "ADM_FICHA_RP", "Depósito RP", 100, 100, natureza: NaturezaDaCarteira.Administrativa);
        var deBarretos = Carteira.Criar(2, maquinas.Id, "MAQ_FICHA_BA", "Máquinas Barretos", 200, 200);
        db.Carteiras.AddRange(comercial, deposito, deBarretos);
        await db.SaveChangesAsync();

        db.ClienteCarteiras.AddRange(
            ClienteCarteira.Criar(dono.Id, deposito.Id, ClasseDeCliente.C, 100, vinculadoEmUtc: new DateTime(2020, 5, 5, 0, 0, 0, DateTimeKind.Utc)),
            ClienteCarteira.Criar(dono.Id, comercial.Id, ClasseDeCliente.C, 100, vinculadoEmUtc: new DateTime(2025, 1, 6, 19, 49, 52, DateTimeKind.Utc)),
            ClienteCarteira.Criar(dono.Id, deBarretos.Id, ClasseDeCliente.C, 200),
            ClienteCarteira.Criar(semClasse.Id, comercial.Id, ClasseDeCliente.C, 100));

        await db.SaveChangesAsync();
    }

    // =============================================================================================
    // A frota pelo dono atual
    // =============================================================================================

    [Fact]
    public async Task O_filtro_por_cliente_traz_o_dono_atual_e_as_compras_no_art_com_a_relacao_e_a_evidencia()
    {
        await SemearAsync();
        var dono = await ChaveDoClienteAsync("Dono da frota");

        var frota = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos?clienteChave={dono}"));
        var porChassi = frota.GetProperty("itens").EnumerateArray().ToDictionary(m => m.GetProperty("chassi").GetString()!);

        porChassi.Keys.Should().BeEquivalentTo([PelaNota, ConfirmadaNoArt, RevendidaAOutro, CompradaPeloDono],
            "o dono atual pelo vínculo, o dono confirmado e o comprador no ART entram; a máquina de outra filial, a " +
            "do outro dono e o vínculo encerrado não");
        frota.GetProperty("total").GetInt32().Should().Be(4);

        // O DONO ATUAL SEM DONO CONFIRMADO — é o caso das 19 mil máquinas do Protheus.
        var pelaNota = porChassi[PelaNota];
        pelaNota.GetProperty("clienteChave").ValueKind.Should().Be(JsonValueKind.Null, "o dono confirmado continua vazio");
        pelaNota.GetProperty("donoAtualChave").GetGuid().Should().Be(dono);
        pelaNota.GetProperty("evidenciaDoDonoAtual").GetString().Should().Be("NotaDeVenda");
        pelaNota.GetProperty("evidenciaDoDonoAtualEm").GetString().Should().Be("2025-03-10");
        var relacao = pelaNota.GetProperty("relacaoComOCliente");
        relacao.GetProperty("ehDonoAtual").GetBoolean().Should().BeTrue();
        relacao.GetProperty("ehDonoConfirmado").GetBoolean().Should().BeFalse();
        relacao.GetProperty("compradaEm").ValueKind.Should().Be(JsonValueKind.Null);

        var confirmada = porChassi[ConfirmadaNoArt].GetProperty("relacaoComOCliente");
        confirmada.GetProperty("ehDonoAtual").GetBoolean().Should().BeTrue();
        confirmada.GetProperty("ehDonoConfirmado").GetBoolean().Should().BeTrue();
        confirmada.GetProperty("compradaEm").GetString().Should().Be("2024-06-12");

        // O COMPRADOR DO ART FICA COMO HISTÓRICO: a máquina aparece, com o dono atual que é outro.
        var revendida = porChassi[RevendidaAOutro];
        revendida.GetProperty("relacaoComOCliente").GetProperty("ehDonoAtual").GetBoolean().Should().BeFalse();
        revendida.GetProperty("relacaoComOCliente").GetProperty("compradaEm").GetString().Should().Be("2023-03-20");
        revendida.GetProperty("donoAtualNome").GetString().Should().Be("Outro dono");
        revendida.GetProperty("evidenciaDoDonoAtual").GetString().Should().Be("OrdemDeServico");

        porChassi[CompradaPeloDono].GetProperty("relacaoComOCliente").GetProperty("compradaPeloDonoNoProtheus")
            .GetBoolean().Should().BeTrue("a venda entrou com o dono do Protheus no lugar do comprador do ART");
        porChassi[RevendidaAOutro].GetProperty("relacaoComOCliente").GetProperty("compradaPeloDonoNoProtheus")
            .GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task A_lista_sem_filtro_mostra_o_dono_atual_e_nao_traz_a_relacao()
    {
        await SemearAsync();

        var maquina = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos?termo={PelaNota}")))
            .GetProperty("itens")[0];

        maquina.GetProperty("donoAtualNome").GetString().Should().Be("Dono da frota");
        maquina.GetProperty("evidenciaDoDonoAtual").GetString().Should().Be("NotaDeVenda");
        maquina.GetProperty("relacaoComOCliente").ValueKind.Should().Be(JsonValueKind.Null, "sem clienteChave não há cliente do filtro");

        var ficha = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos/{maquina.GetProperty("chave").GetGuid()}"));
        ficha.GetProperty("donoAtualNome").GetString().Should().Be("Dono da frota");
        ficha.GetProperty("evidenciaDoDonoAtual").GetString().Should().Be("NotaDeVenda");
    }

    [Fact]
    public async Task A_maquina_do_cliente_cadastrada_em_outra_filial_nao_abre_a_fronteira()
    {
        await SemearAsync();
        var dono = await ChaveDoClienteAsync("Dono da frota");

        (await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos?termo={DeOutraFilial}")))
            .GetProperty("total").GetInt32().Should().Be(0, "a máquina é de Barretos");

        // Em Barretos, o cliente de Ribeirão não resolve: a consulta é recusada, e não devolve a máquina de lá.
        (await api.ClienteDeBarretos().GetAsync($"/api/v1/equipamentos?clienteChave={dono}"))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Em_todas_as_filiais_o_administrador_ve_tambem_a_maquina_cadastrada_em_outra_filial()
    {
        await using var app = new ApiEmMemoria();
        await app.InitializeAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);

        var ficha = new FichaDoClienteNaApiTestes(app);
        await ficha.SemearAsync();
        var dono = await ficha.ChaveDoClienteAsync("Dono da frota");

        var todas = app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ContextoAcesso.CodigoDeTodasAsFiliais);
        var frota = await DadosAsync(await todas.GetAsync($"/api/v1/equipamentos?clienteChave={dono}"));

        frota.GetProperty("itens").EnumerateArray().Select(m => m.GetProperty("chassi").GetString())
            .Should().Contain(DeOutraFilial, "quem alcança as duas filiais vê a máquina das duas — a regra é a de sempre");
    }

    [Fact]
    public async Task As_maquinas_compradas_dizem_dono_atual_pelo_vinculo_e_nao_so_pelo_cadastro_da_maquina()
    {
        await SemearAsync();
        var dono = await ChaveDoClienteAsync("Dono da frota");

        var compradas = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{dono}/maquinas-compradas")))
            .EnumerateArray().ToDictionary(m => m.GetProperty("chassi").GetString()!, m => m.GetProperty("ehDonoAtual").GetBoolean());

        compradas[ConfirmadaNoArt].Should().BeTrue();
        compradas[CompradaPeloDono].Should().BeTrue("o vínculo de dono atual é dele, mesmo sem o dono confirmado");
        compradas[RevendidaAOutro].Should().BeFalse("a máquina foi revendida: o dono atual é outro");
    }

    // =============================================================================================
    // O faturamento do cliente
    // =============================================================================================

    [Fact]
    public async Task O_faturamento_do_cliente_traz_os_doze_meses_a_quebra_a_filial_e_a_data_da_carga()
    {
        await SemearAsync();
        var dono = await ChaveDoClienteAsync("Dono da frota");

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{dono}/faturamento"));

        dados.GetProperty("competenciaMaisRecente").GetString().Should().Be(MesCorrente.ToString("yyyy-MM-dd"),
            "a âncora é a carga, e não a última compra do cliente");
        dados.GetProperty("carregadoEm").ValueKind.Should().Be(JsonValueKind.String);
        dados.GetProperty("ultimoMesEstaIncompleto").GetBoolean().Should().BeTrue("a carga gravou o mês corrente antes de ele acabar");
        dados.GetProperty("ultimaCompraEm").GetString().Should().Be(MesCorrente.AddMonths(-1).ToString("yyyy-MM-dd"));

        var doze = dados.GetProperty("dozeMeses");
        doze.GetProperty("valorLiquido").GetDecimal().Should().Be(1500m, "a nota de Barretos e a de 14 meses atrás ficam fora");
        doze.GetProperty("maquina").GetDecimal().Should().Be(600m);
        (doze.GetProperty("maquina").GetDecimal() + doze.GetProperty("peca").GetDecimal()
         + doze.GetProperty("servico").GetDecimal() + doze.GetProperty("outros").GetDecimal())
            .Should().Be(1500m, "as quatro parcelas fecham o total");
        doze.GetProperty("de").GetString().Should().Be(MesCorrente.AddMonths(-11).ToString("yyyy-MM-dd"));

        dados.GetProperty("janelaCarregada").GetProperty("valorLiquido").GetDecimal().Should().Be(3500m);
        dados.GetProperty("janelaCarregada").GetProperty("maquina").GetDecimal().Should().Be(2600m);

        var serie = dados.GetProperty("serie").EnumerateArray().ToList();
        serie.Should().HaveCount(12);
        serie[^1].GetProperty("valorLiquido").GetDecimal().Should().Be(0m, "o mês sem nota entra com zero");
        serie[^2].GetProperty("valorLiquido").GetDecimal().Should().Be(1000m);
        serie[0].GetProperty("valorLiquido").GetDecimal().Should().Be(500m);

        var filiais = dados.GetProperty("porFilial").EnumerateArray().ToList();
        filiais.Should().ContainSingle("a filial de Barretos está fora do alcance");
        filiais[0].GetProperty("filialCodigo").GetString().Should().Be(ApiEmMemoria.FilialDeRibeirao);
        filiais[0].GetProperty("dozeMeses").GetDecimal().Should().Be(1500m);
        filiais[0].GetProperty("naJanela").GetDecimal().Should().Be(3500m);

        Lacunas(dados).Should().Contain("mesIncompleto").And.NotContain("faturamentoDoCliente");
    }

    [Fact]
    public async Task O_cliente_sem_nota_recebe_o_motivo_e_nao_um_zero_calado()
    {
        await SemearAsync();
        var semNada = await ChaveDoClienteAsync("Cliente sem nada");

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{semNada}/faturamento"));

        dados.GetProperty("dozeMeses").GetProperty("valorLiquido").GetDecimal().Should().Be(0m);
        dados.GetProperty("ultimaCompraEm").ValueKind.Should().Be(JsonValueKind.Null);
        dados.GetProperty("porFilial").GetArrayLength().Should().Be(0);
        Lacunas(dados).Should().Contain("faturamentoDoCliente");
    }

    [Fact]
    public async Task O_faturamento_e_as_carteiras_do_cliente_de_outra_filial_respondem_404()
    {
        await SemearAsync();
        var dono = await ChaveDoClienteAsync("Dono da frota");

        (await api.ClienteDeBarretos().GetAsync($"/api/v1/clientes/{dono}/faturamento")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await api.ClienteDeBarretos().GetAsync($"/api/v1/clientes/{dono}/carteiras")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{Guid.NewGuid()}/faturamento")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{Guid.NewGuid()}/carteiras")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // =============================================================================================
    // As carteiras do cliente
    // =============================================================================================

    [Fact]
    public async Task As_carteiras_do_cliente_trazem_o_cen_a_filial_a_classe_do_cadastro_e_a_cadencia_dela()
    {
        await SemearAsync();
        var dono = await ChaveDoClienteAsync("Dono da frota");

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{dono}/carteiras"));

        dados.GetProperty("classe").GetString().Should().Be("A", "a classe é a do cadastro, e não a C do vínculo");
        var carteiras = dados.GetProperty("carteiras").EnumerateArray().ToList();
        carteiras.Should().HaveCount(2, "a carteira de Barretos está fora do alcance");

        var comercial = carteiras[0];
        comercial.GetProperty("carteiraCodigo").GetString().Should().Be("MAQ_FICHA_RP", "a comercial vem primeiro");
        comercial.GetProperty("naturezaDaCarteira").GetString().Should().Be("Comercial");
        comercial.GetProperty("responsavelNome").GetString().Should().Be("cen.ribeiraopreto");
        comercial.GetProperty("naturezaDoResponsavel").GetString().Should().Be("Pessoa");
        comercial.GetProperty("filialCodigo").GetString().Should().Be(ApiEmMemoria.FilialDeRibeirao);
        comercial.GetProperty("linhaDeNegocioNome").GetString().Should().Be("Máquinas");
        comercial.GetProperty("diasDeCadencia").GetInt32().Should().Be(180, "a cadência da classe A na linha");
        comercial.GetProperty("vinculadoEm").GetDateTime().Should().Be(new DateTime(2025, 1, 6, 19, 49, 52));
        comercial.GetProperty("ultimaInteracaoEm").ValueKind.Should().Be(JsonValueKind.Null);
        comercial.GetProperty("estaForaDaCadencia").ValueKind.Should().Be(JsonValueKind.Null, "sem registro não é atraso");

        carteiras[1].GetProperty("naturezaDaCarteira").GetString().Should().Be("Administrativa");
        Lacunas(dados).Should().Contain("ultimoContato");
    }

    [Fact]
    public async Task Sem_classe_apurada_a_cadencia_e_a_da_classe_D_e_o_motivo_vai_junto()
    {
        await SemearAsync();
        var semClasse = await ChaveDoClienteAsync("Cliente sem classe");

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{semClasse}/carteiras"));

        dados.GetProperty("classe").ValueKind.Should().Be(JsonValueKind.Null);
        dados.GetProperty("carteiras")[0].GetProperty("diasDeCadencia").GetInt32().Should().Be(360);
        Lacunas(dados).Should().Contain("classe");
    }

    [Fact]
    public async Task O_cliente_fora_de_carteira_recebe_o_motivo()
    {
        await SemearAsync();
        var semNada = await ChaveDoClienteAsync("Cliente sem nada");

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{semNada}/carteiras"));

        dados.GetProperty("carteiras").GetArrayLength().Should().Be(0);
        Lacunas(dados).Should().Contain("carteiras").And.NotContain("ultimoContato");
    }
}
