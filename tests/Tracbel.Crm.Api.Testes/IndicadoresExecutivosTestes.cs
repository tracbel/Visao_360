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
    // O MÊS DE SÃO PAULO, como o servidor: às 22h do último dia do mês o UTC já virou o mês.
    private static readonly DateOnly MesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(Agora);

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
    public async Task O_padrao_e_o_ano_fiscal_ate_o_ultimo_mes_fechado_e_o_cartao_so_traz_o_faturamento()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var ano = dados.GetProperty("indicadores").GetProperty("ano");

        // O ANO FISCAL ATÉ O ÚLTIMO MÊS FECHADO É O PADRÃO (27/09/2026), como nos Indicadores Geográficos: o mês
        // em curso — os R$ 1.280 semeados nele — fica no cartão do mês, e não no ano. O dezembro do ano civil
        // anterior entra quando é do ano fiscal que vai até o último mês fechado. O teste segue o relógio.
        var somado = AnoFiscal.AteOUltimoMesFechado(MesCorrente);
        var anoFiscal = AnoFiscal.Do(somado.Final);
        var dezembroEntra = somado.Contem(new DateOnly(Agora.Year - 1, 12, 1));

        ano.GetProperty("ano").GetInt32().Should().Be(anoFiscal);
        ano.GetProperty("calendario").GetString().Should().Be("Fiscal");
        ano.GetProperty("inicio").GetString().Should().Be(somado.Inicial.ToString("yyyy-MM-dd"), "o ano fiscal começa em novembro");
        ano.GetProperty("fim").GetString().Should().Be(somado.Final.ToString("yyyy-MM-dd"), "o mês em curso fica à parte");
        ano.GetProperty("total").GetDecimal().Should().Be(dezembroEntra ? 999m : 0m,
            "o mês em curso não entra no ano — ele está pela metade");
        ano.GetProperty("mesesComFaturamento").GetInt32().Should().Be(dezembroEntra ? 1 : 0);
        dados.GetProperty("indicadores").GetProperty("faturamentoDoMes").GetProperty("competencia").GetString()
            .Should().Be(MesCorrente.ToString("yyyy-MM-dd"), "o mês em curso aparece à parte, no cartão do mês");

        // A META SAIU DESTE CARTÃO (#138, 27/09/2026): a meta de VENDA, em unidades, da API Gestão de Negócios, tem rota
        // própria. A de faturamento nunca teve fonte, e os campos vinham sempre zerados.
        ano.EnumerateObject().Select(p => p.Name).Should().NotContain(n => n.Contains("meta", StringComparison.OrdinalIgnoreCase)
                                                                          || n.Contains("alvo", StringComparison.OrdinalIgnoreCase));
        Lacunas(dados).Should().NotContain("metaDeFaturamento");
        Lacunas(dados).Should().Contain(["previsao", "devolucoes"]);

        // A FRASE QUE NEGAVA O CALENDÁRIO SAIU: ele foi confirmado em 24/09 e virou o padrão em 27/09.
        Lacunas(dados).Should().NotContain(["calendarioFiscal", "calendarioCivil"]);
        dados.GetProperty("metricasSemDado").EnumerateArray()
            .Select(m => m.GetProperty("motivo").GetString()).Should().NotContain(m => m!.Contains("não foi confirmado"));
    }

    [Fact]
    public async Task O_ano_civil_continua_por_pedido_e_nao_muda_o_mes_do_cartao()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?ano={Agora.Year - 1}"));
        var indicadores = dados.GetProperty("indicadores");

        indicadores.GetProperty("ano").GetProperty("calendario").GetString().Should().Be("Civil");
        indicadores.GetProperty("ano").GetProperty("total").GetDecimal().Should().Be(999m);
        indicadores.GetProperty("faturamentoDoMes").GetProperty("total").GetDecimal().Should().Be(1280m);
        Lacunas(dados).Should().Contain("calendarioCivil").And.NotContain("metaDeFaturamento");
    }

    [Fact]
    public async Task O_ano_fiscal_pedido_comeca_em_novembro_e_para_no_ultimo_mes_fechado_se_ainda_corre()
    {
        await SemearAsync();

        // O ANO FISCAL QUE CONTÉM O DEZEMBRO SEMEADO: o de dezembro do ano passado termina em outubro deste —
        // salvo em novembro e dezembro, quando o fiscal corrente já é o seguinte. O pedido explícito é o
        // mesmo nos dois casos; o que muda é até onde ele vai: o ano que ainda corre para no último mês fechado.
        var anoFiscal = AnoFiscal.Do(new DateOnly(Agora.Year - 1, 12, 1));
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?anoFiscal={anoFiscal}"));
        var ano = dados.GetProperty("indicadores").GetProperty("ano");

        var inteiro = AnoFiscal.Inteiro(anoFiscal);
        var ultimoFechado = MesCorrente.AddMonths(-1);
        ano.GetProperty("ano").GetInt32().Should().Be(anoFiscal);
        ano.GetProperty("inicio").GetString().Should().Be($"{anoFiscal - 1}-11-01");
        ano.GetProperty("fim").GetString().Should().Be(
            (inteiro.Final <= ultimoFechado ? inteiro.Final : ultimoFechado).ToString("yyyy-MM-dd"));
        ano.GetProperty("primeiraCompetencia").GetString().Should().Be($"{Agora.Year - 1}-12-01");
    }

    [Fact]
    public async Task Ano_civil_e_ano_fiscal_juntos_sao_recusados()
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync($"{Rota}?ano={Agora.Year}&anoFiscal={Agora.Year}");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("erros")
            .EnumerateArray().Select(e => e.GetProperty("campo").GetString()).Should().Contain("anoFiscal");
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

    /// <summary>
    /// O ART da filial: entregues no ano (uma com a venda no CRM, uma aguardando cadastro, uma sem valor), no mês em curso
    /// e no mesmo trecho do ano anterior — e o que NÃO conta: a vendida e não entregue, a que sumiu da origem, a de
    /// unidade sem filial e a de Barretos.
    /// </summary>
    private async Task SemearArtAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Sistemas.AnyAsync(s => s.Codigo == ConexoesDoSistema.Art)) return;

        var art = Sistema.Criar(ConexoesDoSistema.Art, "ART — vendas de máquina", "teste");
        db.Sistemas.Add(art);
        var comprador = Cliente.Criar(1, "Comprador do ART", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.Add(comprador);
        var maquina = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar("1ART0000000000001"), OrigemDoEquipamento.Art, 100);
        db.Equipamentos.Add(maquina);
        await db.SaveChangesAsync();

        foreach (var (codigo, texto, empresa) in new[] { ("RIBEIRAO_PRETO", "Ribeirão Preto", 1), ("BARRETOS", "Barretos", 2) })
        {
            var unidade = CorrespondenciaDaOrigem.Registrar(art.Id, TipoDeCorrespondencia.Unidade, codigo, texto, null, DateTime.UtcNow);
            unidade.Avaliar(SituacaoDaCorrespondencia.CorrespondenciaExata, null, null, empresa, "nome idêntico");
            db.CorrespondenciasDaOrigem.Add(unidade);
        }

        var ultimoFechado = MesCorrente.AddMonths(-1);
        DateOnly Dia(DateOnly mes, int dia) => mes.AddDays(dia - 1);

        var venda = VendaDeMaquina.Registrar(art.Id, "7009", maquina.Id, comprador.Id,
            new DadosDaVendaNaOrigem(1, null, Dia(ultimoFechado, 2), null, Dia(ultimoFechado, 20), null, null, null, null, null,
                false, false, 1, "TRATOR MÉDIO", "TR 6110J", null, "Ribeirão Preto", null, "h7009", null),
            DateTime.UtcNow, 100);
        db.VendasDeMaquina.Add(venda);
        await db.SaveChangesAsync();

        RegistroDeOrigem Registro(string codigo, string unidade, DateOnly? entregue, decimal? valor) =>
            RegistroDeOrigem.Registrar(art.Id, MetaDeVenda.FluxoDasVendasDoArt, codigo,
                new RetratoDoRegistroDeOrigem($"h{codigo}", null, "TRATOR MÉDIO", "TR 6110J", unidade, Dia(ultimoFechado, 1), null, entregue, valor),
                DateTime.UtcNow);

        var noCrm = Registro("7009", "Ribeirão Preto", Dia(ultimoFechado, 20), 250_000m);
        noCrm.Decidir(DecisaoDaIntegracao.Importado, null, venda.Id);
        var sumiu = Registro("7007", "Ribeirão Preto", Dia(ultimoFechado, 8), 111m);
        sumiu.MarcarAusente(DateTime.UtcNow);

        db.RegistrosDeOrigem.AddRange(
            noCrm,
            Registro("7001", "Ribeirão Preto", Dia(ultimoFechado, 10), 500_000.50m),
            Registro("7002", "Ribeirão Preto", Dia(ultimoFechado, 12), null),
            Registro("7003", "Ribeirão Preto", Dia(MesCorrente, 1), 300_000m),
            Registro("7004", "Ribeirão Preto", Dia(ultimoFechado.AddMonths(-12), 5), 400_000m),
            Registro("7005", "Barretos", Dia(ultimoFechado, 3), 700_000m),
            Registro("7006", "Ribeirão Preto", null, 999m),
            sumiu,
            Registro("7008", "Unidade Desconhecida", Dia(ultimoFechado, 4), 222m));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task O_faturamento_do_ano_e_o_ART_das_maquinas_entregues_com_e_sem_comprador_no_CRM()
    {
        await SemearAsync();
        await SemearArtAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var indicadores = dados.GetProperty("indicadores");
        var somado = AnoFiscal.AteOUltimoMesFechado(MesCorrente);

        // "VENDIDA = ENTREGUE NO ART" (29/09/2026): a data de entrega preenchida, nos meses do ano — o ART inteiro, e não só
        // o que já virou venda no CRM. A vendida sem entrega, a que sumiu da origem e a de unidade sem filial não contam.
        var ano = indicadores.GetProperty("entreguesNoAno");
        ano.GetProperty("inicio").GetString().Should().Be(somado.Inicial.ToString("yyyy-MM-dd"));
        ano.GetProperty("fim").GetString().Should().Be(somado.Final.ToString("yyyy-MM-dd"), "o mês em curso fica à parte");
        ano.GetProperty("maquinas").GetInt32().Should().Be(3);
        ano.GetProperty("valor").GetDecimal().Should().Be(750_000.50m, "a máquina sem valor conta em máquinas, e não em reais");
        ano.GetProperty("semValor").GetInt32().Should().Be(1);
        ano.GetProperty("aguardandoNoCrm").GetInt32().Should().Be(2, "só uma virou venda no CRM, e as outras entram mesmo assim");

        var mes = indicadores.GetProperty("entreguesNoMesEmCurso");
        mes.GetProperty("inicio").GetString().Should().Be(MesCorrente.ToString("yyyy-MM-dd"));
        mes.GetProperty("maquinas").GetInt32().Should().Be(1);
        mes.GetProperty("valor").GetDecimal().Should().Be(300_000m);

        var anterior = indicadores.GetProperty("entreguesNoMesmoTrechoDoAnoAnterior");
        anterior.GetProperty("inicio").GetString().Should().Be(somado.Inicial.AddMonths(-12).ToString("yyyy-MM-dd"));
        anterior.GetProperty("maquinas").GetInt32().Should().Be(1);
        anterior.GetProperty("valor").GetDecimal().Should().Be(400_000m);

        // O FATURAMENTO MÊS A MÊS (29/09/2026, "pegamos do ART na coluna entregue e valor"): os doze meses que terminam no
        // mês em curso, com zero no mês sem entrega — a de doze meses antes do último fechado fica fora da série.
        var porMes = indicadores.GetProperty("entreguesPorMes").EnumerateArray().ToList();
        porMes.Should().HaveCount(12);
        porMes[0].GetProperty("inicio").GetString().Should().Be(MesCorrente.AddMonths(-11).ToString("yyyy-MM-dd"));
        porMes[^1].GetProperty("inicio").GetString().Should().Be(MesCorrente.ToString("yyyy-MM-dd"), "o último é o mês em curso");
        porMes[^1].GetProperty("maquinas").GetInt32().Should().Be(1);
        porMes[^1].GetProperty("valor").GetDecimal().Should().Be(300_000m);
        porMes[^2].GetProperty("inicio").GetString().Should().Be(MesCorrente.AddMonths(-1).ToString("yyyy-MM-dd"));
        porMes[^2].GetProperty("maquinas").GetInt32().Should().Be(3);
        porMes[^2].GetProperty("valor").GetDecimal().Should().Be(750_000.50m);
        porMes[^2].GetProperty("semValor").GetInt32().Should().Be(1);
        porMes.Take(10).Sum(m => m.GetProperty("maquinas").GetInt32()).Should().Be(0, "mês sem entrega vem com zero, e não some");

        Lacunas(dados).Should().Contain(["maquinaSemValorNoArt", "entregueAguardandoNoCrm", "notaDoProtheusNaoSomada"]);
        Lacunas(dados).Should().NotContain("vendaDeMaquinaNoArt", "a frase dizia que o ART não entra no faturamento");

        // A FILIAL É A DA UNIDADE QUE VENDEU, e cada uma vê só a sua: somadas, nada conta duas vezes.
        var deBarretos = (await DadosAsync(await api.ClienteDeBarretos().GetAsync(Rota))).GetProperty("indicadores");
        var barretos = deBarretos.GetProperty("entreguesNoAno");
        barretos.GetProperty("maquinas").GetInt32().Should().Be(1);
        barretos.GetProperty("valor").GetDecimal().Should().Be(700_000m);
        var barretosPorMes = deBarretos.GetProperty("entreguesPorMes").EnumerateArray().ToList();
        barretosPorMes[^2].GetProperty("valor").GetDecimal().Should().Be(700_000m, "a série também é da filial da unidade que vendeu");
        barretosPorMes.Sum(m => m.GetProperty("maquinas").GetInt32()).Should().Be(1);
    }

    [Fact]
    public async Task Num_ano_fiscal_fechado_a_serie_do_ART_termina_em_outubro_dele()
    {
        // O PERÍODO MUDA TODOS OS PAINÉIS COM DATA (decisão do Ricardo de 30/09/2026, #313): o "Faturamento — 12 meses" de
        // um ano fechado são os doze meses dele, de novembro a outubro, e não os doze que terminam hoje.
        await SemearAsync();
        await SemearArtAsync();
        var anoPassado = AnoFiscal.Do(MesCorrente.AddMonths(-1)) - 1;
        var doAno = AnoFiscal.Inteiro(anoPassado);

        var indicadores = (await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?anoFiscal={anoPassado}"))).GetProperty("indicadores");
        var porMes = indicadores.GetProperty("entreguesPorMes").EnumerateArray().ToList();

        porMes.Should().HaveCount(12);
        porMes[0].GetProperty("inicio").GetString().Should().Be(doAno.Inicial.ToString("yyyy-MM-dd"));
        porMes[^1].GetProperty("inicio").GetString().Should().Be(doAno.Final.ToString("yyyy-MM-dd"), "o ano fechado termina em outubro");
        // A ENTREGA DE DOZE MESES ANTES DO ÚLTIMO FECHADO é deste ano, e agora entra na série.
        porMes.Sum(m => m.GetProperty("maquinas").GetInt32()).Should().Be(1);
        porMes.Sum(m => m.GetProperty("valor").GetDecimal()).Should().Be(400_000m);

        // O MÊS EM CURSO CONTINUA MEDIDO À PARTE, qualquer que seja o ano pedido.
        indicadores.GetProperty("entreguesNoMesEmCurso").GetProperty("valor").GetDecimal().Should().Be(300_000m);
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
