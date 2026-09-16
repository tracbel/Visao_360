using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// OS CINCO CARTÕES DA VISÃO 360 POR HTTP (documento 36), com a aplicação inteira de pé.
///
/// <para>O cenário semeado cobre exatamente o que o pedido exige que os cartões não confundam: nota
/// com cliente e sem cliente por natureza, com as parcelas fechando o total; ano civil pedido contra
/// competência do cartão; cliente único contra vínculo, com um cliente de Barretos na carteira de
/// Ribeirão; cobertura pela cadência, com vínculo coberto, fora da cadência, nunca contatado e em
/// linha sem cadência.</para>
///
/// <para>A meta de faturamento saiu na Fase 1 (documento 41): a tabela nunca teve linha e não havia
/// tela que a preenchesse, então o cartão do ano declara a lacuna em vez de mostrar alvo.</para>
/// </summary>
[Trait("Categoria", "PainelExecutivo")]
public sealed class IndicadoresExecutivosTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/relatorios/indicadores-executivos";

    private static readonly DateTime Agora = DateTime.UtcNow;
    private static readonly DateOnly MesCorrente = new(Agora.Year, Agora.Month, 1);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static IEnumerable<string?> Lacunas(JsonElement dados) =>
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString());

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Carteiras.AnyAsync()) return;

        var comCadencia = LinhaDeNegocio.Criar("MAQ_EXEC", "Máquinas");
        comCadencia.DeclararCadencia(180, 180, 180, 360);
        var semCadencia = LinhaDeNegocio.Criar("PECAS_EXEC", "Peças");
        db.LinhasDeNegocio.AddRange(comCadencia, semCadencia);
        await db.SaveChangesAsync();

        var maquinasRibeirao = Carteira.Criar(1, comCadencia.Id, "MAQ_EXEC_RP", "Máquinas RP", 100, 100);
        var pecasRibeirao = Carteira.Criar(1, semCadencia.Id, "PECAS_EXEC_RP", "Peças RP", 100, 100);
        var depositoRibeirao = Carteira.Criar(1, comCadencia.Id, "ADM_EXEC_RP", "Depósito RP", 100, 100,
            natureza: NaturezaDaCarteira.Administrativa);
        var maquinasBarretos = Carteira.Criar(2, comCadencia.Id, "MAQ_EXEC_BA", "Máquinas Barretos", 200, 200);
        db.Carteiras.AddRange(maquinasRibeirao, pecasRibeirao, depositoRibeirao, maquinasBarretos);

        // Documento fictício de teste, com dígitos verificadores válidos.
        var coberto = Cliente.Criar(1, "Coberto em Ribeirão", TipoDePessoa.Juridica, 100, 100,
            documento: CpfCnpj.Criar("11222333000181"), situacao: SituacaoDoCliente.Cliente);
        var nunca = Cliente.Criar(1, "Nunca contatado", TipoDePessoa.Juridica, 100, 100);
        var deBarretos = Cliente.Criar(2, "Cadastrado em Barretos", TipoDePessoa.Juridica, 200, 200,
            situacao: SituacaoDoCliente.Suspect);
        var semVinculo = Cliente.Criar(1, "Sem vínculo", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.AddRange(coberto, nunca, deBarretos, semVinculo);
        await db.SaveChangesAsync();

        coberto.ApurarClasse(ClasseDeCliente.A, 1000m, Agora, 100);

        var cobertoNasMaquinas = ClienteCarteira.Criar(coberto.Id, maquinasRibeirao.Id, ClasseDeCliente.C, 100);
        cobertoNasMaquinas.RegistrarInteracao(Agora.AddDays(-10));
        // Classe A (180 dias) no cadastro de Barretos, mas o cadastro não está ao alcance de Ribeirão:
        // o vínculo conta como D (360 dias), e 400 dias continuam fora da cadência.
        var deBarretosEmRibeirao = ClienteCarteira.Criar(deBarretos.Id, maquinasRibeirao.Id, ClasseDeCliente.C, 100);
        deBarretosEmRibeirao.RegistrarInteracao(Agora.AddDays(-400));
        var deBarretosEmBarretos = ClienteCarteira.Criar(deBarretos.Id, maquinasBarretos.Id, ClasseDeCliente.C, 200);
        deBarretosEmBarretos.RegistrarInteracao(Agora.AddDays(-5));

        db.ClienteCarteiras.AddRange(
            cobertoNasMaquinas,
            ClienteCarteira.Criar(coberto.Id, pecasRibeirao.Id, ClasseDeCliente.C, 100),
            ClienteCarteira.Criar(nunca.Id, maquinasRibeirao.Id, ClasseDeCliente.C, 100),
            ClienteCarteira.Criar(nunca.Id, depositoRibeirao.Id, ClasseDeCliente.C, 100),
            deBarretosEmRibeirao,
            deBarretosEmBarretos);

        // Ribeirão fatura no mês corrente com cliente e sem cliente (três naturezas), e em dezembro do
        // ano anterior. Barretos não fatura: é a filial que prova "sem dado, e não zero".
        db.FaturamentoDosClientes.AddRange(
            FaturamentoDoCliente.Criar(1, coberto.Id, MesCorrente, 1000m, 2, 5, new QuebraDoFaturamento(600m, 300m, 100m, 0m)),
            FaturamentoDoCliente.Criar(1, coberto.Id, new DateOnly(Agora.Year - 1, 12, 1), 999m, 1, 1, new QuebraDoFaturamento(999m, 0m, 0m, 0m)));
        db.FaturamentoSemClientes.AddRange(
            FaturamentoSemCliente.Criar(1, MesCorrente, "11111111000111", "Contraparte de teste", NaturezaDoParceiro.ClienteNaoCadastrado, 200m, 1, 1, new QuebraDoFaturamento(200m, 0m, 0m, 0m)),
            FaturamentoSemCliente.Criar(1, MesCorrente, "22222222000122", "Sem documento de teste", NaturezaDoParceiro.SemDocumento, 30m, 1, 1, new QuebraDoFaturamento(0m, 30m, 0m, 0m)),
            FaturamentoSemCliente.Criar(1, MesCorrente, "33333333000133", "Fábrica de teste", NaturezaDoParceiro.Fabrica, 50m, 1, 1, new QuebraDoFaturamento(50m, 0m, 0m, 0m)));

        // A META SAIU NA FASE 1 (documento 41): `organizacao.Meta` nunca teve linha e não havia tela
        // que a preenchesse. O cartão do ano segue sem alvo, e é isso que os testes conferem abaixo.

        var motivo = MotivoDePerda.Criar("PRECO_EXEC", "Preço", CategoriaDeMotivoDePerda.Preco);
        db.MotivosDePerda.Add(motivo);
        db.TiposDeTarefa.Add(TipoTarefa.Criar("VISITAR_EXEC", "Visitar cliente", CategoriaDeInteracao.Visita));
        await db.SaveChangesAsync();

        db.VendasPerdidas.AddRange(
            VendaPerdida.Criar(1, Agora.AddDays(-30), motivo.Id, ocorridaEm: DateOnly.FromDateTime(Agora.AddDays(-40)),
                modeloDoConcorrente: "Modelo de teste", quantidade: 2, precoDoConcorrente: 100m, precoOfertado: 110m),
            VendaPerdida.Criar(1, Agora.AddDays(-5), motivo.Id),
            VendaPerdida.Criar(2, Agora.AddDays(-3), motivo.Id));

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task O_faturamento_do_mes_separa_a_nota_sem_cliente_e_as_parcelas_fecham_o_total()
    {
        await SemearAsync();
        var mes = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota))).GetProperty("indicadores").GetProperty("faturamentoDoMes");

        mes.GetProperty("competencia").GetString().Should().Be(MesCorrente.ToString("yyyy-MM-dd"));
        mes.GetProperty("comCliente").GetDecimal().Should().Be(1000m, "dezembro do ano anterior não é o mês do cartão");
        mes.GetProperty("contraparteSemCadastro").GetDecimal().Should().Be(230m, "cliente não cadastrado mais nota sem documento");
        mes.GetProperty("repasseDeFabrica").GetDecimal().Should().Be(50m);
        mes.GetProperty("semCliente").GetDecimal().Should().Be(280m);
        mes.GetProperty("total").GetDecimal().Should().Be(1280m);
        (mes.GetProperty("maquina").GetDecimal() + mes.GetProperty("peca").GetDecimal()
         + mes.GetProperty("servico").GetDecimal() + mes.GetProperty("outros").GetDecimal())
            .Should().Be(1280m, "a quebra por grupo de item é outro corte do mesmo total");
        mes.GetProperty("notas").GetInt32().Should().Be(5);
        mes.GetProperty("carregadoEm").ValueKind.Should().Be(JsonValueKind.String);
    }

    [Fact]
    public async Task Filial_sem_faturamento_declara_a_lacuna_em_vez_de_mostrar_zero()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeBarretos().GetAsync(Rota));

        dados.GetProperty("indicadores").GetProperty("faturamentoDoMes").ValueKind.Should().Be(JsonValueKind.Null);
        Lacunas(dados).Should().Contain("faturamentoDoMes");
    }

    [Fact]
    public async Task O_ano_e_o_civil_pedido_e_o_cartao_fica_sem_meta()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var ano = dados.GetProperty("indicadores").GetProperty("ano");

        ano.GetProperty("ano").GetInt32().Should().Be(Agora.Year);
        ano.GetProperty("total").GetDecimal().Should().Be(1280m);
        ano.GetProperty("mesesComFaturamento").GetInt32().Should().Be(1);

        // A META SAIU NA FASE 1 (documento 41). Sem fonte, o cartão mostra o realizado e declara a
        // lacuna: "sem meta é sem meta, e não meta zero" — que é o que a tela já dizia, porque a
        // tabela nunca teve uma linha.
        ano.GetProperty("metasDaFilial").GetInt32().Should().Be(0);
        ano.GetProperty("alvoDaFilial").ValueKind.Should().Be(JsonValueKind.Null);
        ano.GetProperty("metasQueCruzamOAno").GetInt32().Should().Be(0);
        Lacunas(dados).Should().Contain(["metaDeFaturamento", "calendarioFiscal", "previsao", "devolucoes"]);
    }

    [Fact]
    public async Task O_ano_anterior_segue_o_pedido_e_nao_muda_o_mes_do_cartao()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?ano={Agora.Year - 1}"));
        var indicadores = dados.GetProperty("indicadores");

        indicadores.GetProperty("ano").GetProperty("total").GetDecimal().Should().Be(999m);
        indicadores.GetProperty("ano").GetProperty("alvoDaFilial").ValueKind.Should().Be(JsonValueKind.Null, "sem meta é sem meta, e não meta zero");
        indicadores.GetProperty("faturamentoDoMes").GetProperty("total").GetDecimal().Should().Be(1280m);
        Lacunas(dados).Should().Contain("metaDeFaturamento");
    }

    [Fact]
    public async Task Os_clientes_unicos_somados_entre_filiais_nao_contam_ninguem_duas_vezes()
    {
        await SemearAsync();
        var ribeirao = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota))).GetProperty("indicadores").GetProperty("carteira");
        var barretos = (await DadosAsync(await api.ClienteDeBarretos().GetAsync(Rota))).GetProperty("indicadores").GetProperty("carteira");

        ribeirao.GetProperty("clientesCadastradosComVinculo").GetInt32().Should().Be(2, "o cliente sem vínculo não está em carteira");
        barretos.GetProperty("clientesCadastradosComVinculo").GetInt32().Should().Be(1);
        (ribeirao.GetProperty("clientesCadastradosComVinculo").GetInt32() + barretos.GetProperty("clientesCadastradosComVinculo").GetInt32())
            .Should().Be(3, "três clientes distintos têm vínculo");

        ribeirao.GetProperty("clientesNasCarteirasDaFilial").GetInt32().Should().Be(3, "o cliente de Barretos está na carteira de Ribeirão");
        (ribeirao.GetProperty("clientesNasCarteirasDaFilial").GetInt32() + barretos.GetProperty("clientesNasCarteirasDaFilial").GetInt32())
            .Should().Be(4, "é por isso que esta contagem não se soma entre filiais");

        ribeirao.GetProperty("vinculos").GetInt32().Should().Be(5);
        ribeirao.GetProperty("vinculosComerciais").GetInt32().Should().Be(4, "o depósito administrativo não é carteira comercial");
        ribeirao.GetProperty("clientes").GetInt32().Should().Be(1);
        ribeirao.GetProperty("prospects").GetInt32().Should().Be(1);
        ribeirao.GetProperty("semDocumento").GetInt32().Should().Be(1);
        barretos.GetProperty("suspects").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task A_cobertura_usa_a_cadencia_declarada_da_linha_e_diz_que_nao_mede_visita()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var cobertura = dados.GetProperty("indicadores").GetProperty("cobertura");

        cobertura.GetProperty("vinculosComerciais").GetInt32().Should().Be(4);
        cobertura.GetProperty("elegiveis").GetInt32().Should().Be(3, "o vínculo em linha sem cadência sai do denominador");
        cobertura.GetProperty("cobertos").GetInt32().Should().Be(1);
        cobertura.GetProperty("foraDaCadencia").GetInt32().Should().Be(1);
        cobertura.GetProperty("nuncaContatados").GetInt32().Should().Be(1);
        cobertura.GetProperty("semCadencia").GetInt32().Should().Be(1);
        cobertura.GetProperty("pendentes").GetInt32().Should().Be(2);
        cobertura.GetProperty("tiposMarcadosComoVisita").GetInt32().Should().Be(0);
        Lacunas(dados).Should().Contain(["visita", "classeDeClienteDeOutraFilial"]);
    }

    [Fact]
    public async Task As_vendas_perdidas_ficam_na_filial_e_nao_ha_percentual_de_mercado()
    {
        await SemearAsync();
        var ribeirao = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var mercado = ribeirao.GetProperty("indicadores").GetProperty("mercado");

        mercado.GetProperty("vendasPerdidasRegistradas").GetInt32().Should().Be(2, "a de Barretos não é desta filial");
        mercado.GetProperty("comModeloDoConcorrente").GetInt32().Should().Be(1);
        mercado.GetProperty("comOsDoisPrecos").GetInt32().Should().Be(1);
        mercado.GetProperty("unidades").GetInt32().Should().Be(3);
        mercado.GetProperty("primeiraEm").GetString().Should().Be(DateOnly.FromDateTime(Agora.AddDays(-40)).ToString("yyyy-MM-dd"));
        mercado.GetProperty("ultimaEm").GetString().Should().Be(DateOnly.FromDateTime(Agora.AddDays(-5)).ToString("yyyy-MM-dd"),
            "sem data da perda, vale a data do registro");
        mercado.EnumerateObject().Select(p => p.Name).Should().NotContain(n => n.Contains("participacao", StringComparison.OrdinalIgnoreCase));
        Lacunas(ribeirao).Should().Contain("participacaoDeMercado");

        var barretos = await DadosAsync(await api.ClienteDeBarretos().GetAsync(Rota));
        barretos.GetProperty("indicadores").GetProperty("mercado").GetProperty("vendasPerdidasRegistradas").GetInt32().Should().Be(1);
    }

    [Theory]
    [InlineData(2019)]
    [InlineData(9999)]
    public async Task Ano_fora_do_intervalo_e_recusado_com_o_campo_nomeado(int ano)
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync($"{Rota}?ano={ano}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("erros")
            .EnumerateArray().Select(e => e.GetProperty("campo").GetString()).Should().Contain("ano");
    }
}
