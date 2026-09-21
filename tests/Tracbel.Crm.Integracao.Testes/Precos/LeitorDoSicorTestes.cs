using System.Net;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Integracao.BancoCentral;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Precos;

/// <summary>
/// A LEITURA DO SICOR (issue 68) — o que o formato do Banco Central esconde.
///
/// <list type="bullet">
///   <item>os códigos vêm como texto com zeros à esquerda ("0001") no serviço e sem eles ("1") nas
///   tabelas auxiliares — são o mesmo programa;</item>
///   <item>o nome do produto traz aspas literais dentro do texto (<c>"\"TRATOR\""</c>);</item>
///   <item>o seguro "não informado" é −1;</item>
///   <item>o estado é o código do Banco Central (SP = 27), não o do IBGE (35);</item>
///   <item>as tabelas auxiliares vêm em Latin-1, umas entre aspas e outras não.</item>
/// </list>
///
/// <para>As amostras reproduzem as respostas reais de 21/09/2026.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDoSicorTestes
{
    private const string Resposta = """
        {"@odata.context":"https://olinda.bcb.gov.br/olinda/servico/SICOR/versao/v2/odata$metadata#InvestMunicipioProduto","value":[
          {"Municipio":"BEBEDOURO","nomeProduto":"\"TRATOR\"","MesEmissao":"02","AnoEmissao":"2025","cdPrograma":"0154","cdSubPrograma":"71","cdFonteRecurso":"0431","cdTipoSeguro":"9","cdEstado":"27","VlInvest":1000000.0,"cdProduto":"7080","cdMunicipio":"11790","Atividade":"1","cdModalidade":"14","AreaInvest":0.0},
          {"Municipio":"PALMEIRA D'OESTE","nomeProduto":"\"BOVINOS\"","MesEmissao":"06","AnoEmissao":"2026","cdPrograma":"0001","cdSubPrograma":"0002","cdFonteRecurso":"0505","cdTipoSeguro":"-1","cdEstado":"27","VlInvest":250000.0,"cdProduto":"1300","cdMunicipio":"10265","Atividade":"2","cdModalidade":"27","AreaInvest":18.19}
        ]}
        """;

    private sealed class Tratador(Func<HttpRequestMessage, byte[]> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(responder(pedido)) });
    }

    [Fact]
    public void Codigos_com_zeros_a_esquerda_viram_numeros_e_o_produto_perde_as_aspas()
    {
        var linhas = LeitorDoSicor.InterpretarInvestimentos(Resposta);

        linhas.Should().HaveCount(2);

        var trator = linhas[0];
        trator.Produto.Should().Be("TRATOR", "as aspas vêm dentro do texto e não fazem parte do nome");
        (trator.CodigoProduto, trator.CodigoPrograma, trator.CodigoFonte).Should().Be((7080, 154, 431));
        (trator.Ano, trator.Mes).Should().Be(((short)2025, (byte)2));
        (trator.CodigoMunicipioBcb, trator.Valor, trator.Area).Should().Be((11790, 1000000.00m, 0m));

        var bovinos = linhas[1];
        bovinos.CodigoSeguro.Should().Be(-1, "\"não informado\" no SICOR é −1, e não pode quebrar a leitura");
        (bovinos.CodigoPrograma, bovinos.CodigoSubprograma).Should().Be((1, 2), "\"0001\" e \"1\" são o mesmo programa");
        bovinos.Area.Should().Be(18.19m);
        bovinos.Municipio.Should().Be("PALMEIRA D'OESTE");
    }

    [Fact]
    public async Task O_ano_e_pedido_com_o_estado_do_Banco_Central_e_nao_o_do_IBGE()
    {
        var pedidos = new List<Uri>();
        var leitor = new LeitorDoSicor(new HttpClient(new Tratador(p => { pedidos.Add(p.RequestUri!); return Encoding.UTF8.GetBytes(Resposta); })));

        await leitor.LerInvestimentosAsync(LeitorDoSicor.SaoPauloNoBancoCentral, 2025, CancellationToken.None);

        var pedido = Uri.UnescapeDataString(pedidos.Single().ToString());
        pedido.Should().Contain("cdEstado eq '27'", "SP é 27 no Banco Central; 35 é o código do IBGE")
            .And.Contain("AnoEmissao eq '2025'")
            .And.StartWith(LeitorDoSicor.EnderecoDoInvestimento);
    }

    [Fact]
    public void Resposta_sem_a_lista_value_e_erro_e_nao_ano_vazio()
    {
        // UM ANO VAZIO APAGARIA O ANO INTEIRO NO BANCO (a carga espelha a fonte). Resposta sem "value" é
        // falha do serviço e tem de parar a carga.
        var ler = () => LeitorDoSicor.InterpretarInvestimentos("""{"error":{"code":"500","message":"Timeout"}}""");

        ler.Should().Throw<InvalidDataException>().WithMessage("*value*");
    }

    [Fact]
    public void Linha_sem_um_campo_esperado_para_com_erro()
    {
        var ler = () => LeitorDoSicor.InterpretarInvestimentos("""{"value":[{"Municipio":"X","AnoEmissao":"2025"}]}""");

        ler.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Tabela_de_produto_entre_aspas_e_com_codigo_com_zeros()
    {
        const string produto = "#CODIGO;DESCRICAO;DATA_INICIO;DATA_FIM\n" +
                               "\"0000\";\"Indicador de renegociação\";\"01/01/2013\";\n" +
                               "\"7080\";\"TRATOR\";\"14/05/2012\";\n" +
                               "\"2700\";\"COLHEITADEIRAS, COLHEDEIRAS E ARRANCADEIRAS\";\"14/05/2012\";\"31/12/2030\"\n";

        var itens = LeitorDoSicor.InterpretarCatalogo(produto, CatalogoDoSicor.Produto);

        itens.Should().HaveCount(3);
        itens.Single(i => i.Codigo == 7080).Should().Be(new ItemDoCatalogoDoSicor(7080, 0, "TRATOR", new DateOnly(2012, 5, 14), null));
        itens.Single(i => i.Codigo == 2700).VigenteAte.Should().Be(new DateOnly(2030, 12, 31));
    }

    [Fact]
    public void Tabela_de_subprograma_sem_aspas_traz_o_programa_na_quarta_coluna()
    {
        const string sub = "#CODIGO_SUBPROGRAMA;DESCRICAO_SUBPROGRAMA;VL_TAXA_JUROS;CODIGO_PROGRAMA\r\n" +
                           "1;Custeio (MCR 10-4);7.50;1\r\n" +
                           "71;Moderfrota;12.50;154\r\n";

        LeitorDoSicor.InterpretarCatalogo(sub, CatalogoDoSicor.Subprograma)
            .Should().BeEquivalentTo(new[]
            {
                new ItemDoCatalogoDoSicor(1, 1, "Custeio (MCR 10-4)", null, null),
                new ItemDoCatalogoDoSicor(71, 154, "Moderfrota", null, null)
            });
    }

    [Fact]
    public async Task A_tabela_auxiliar_e_lida_em_Latin1()
    {
        var bytes = Encoding.Latin1.GetBytes("#CODIGO;DESCRICAO;DATA_INICIO;DATA_FIM\n4860;MÁQUINAS E IMPLEMENTOS;14/05/2012;\n");
        var leitor = new LeitorDoSicor(new HttpClient(new Tratador(_ => bytes)));

        var itens = await leitor.LerCatalogoAsync(CatalogoDoSicor.Produto, CancellationToken.None);

        itens.Single().Descricao.Should().Be("MÁQUINAS E IMPLEMENTOS", "lido como UTF-8, o Á vira lixo");
    }

    [Fact]
    public void Tabela_vazia_para_com_erro() =>
        ((Action)(() => LeitorDoSicor.InterpretarCatalogo("#CODIGO;DESCRICAO\n", CatalogoDoSicor.Fonte)))
            .Should().Throw<InvalidDataException>();
}
