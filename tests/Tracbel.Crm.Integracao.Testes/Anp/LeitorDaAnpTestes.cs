using System.IO.Compression;
using System.Net;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Integracao.Anp;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Anp;

/// <summary>
/// A LEITURA DAS USINAS DE ETANOL (ANP) — três coisas que quebram em silêncio moram aqui.
///
/// <list type="number">
///   <item><b>O mês mais recente não é o maior texto.</b> "07/2026" vem antes de "12/2021" em ordem
///   alfabética; escolher por texto pegaria o mês errado e o CRM mostraria a capacidade de cinco
///   anos atrás.</item>
///   <item><b>A razão social tem vírgula.</b> Um <c>Split(',')</c> jogaria o CNPJ para a coluna do
///   estado sem dar erro — a linha entraria errada, em silêncio.</item>
///   <item><b>O arquivo é um ZIP com mais de um CSV de capacidade</b> (a ANP separa "2018-2020" de
///   "2021-2026"): ler só o primeiro perderia metade da série.</item>
/// </list>
///
/// <para><b>Nenhuma chamada sai da máquina.</b> O ZIP é montado aqui, com o formato conferido contra
/// o arquivo real em 20/09/2026.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDaAnpTestes
{
    private const string Cabecalho =
        "Mês/Ano,Razão Social,CNPJ,Região,Estado,Município," +
        "Capacidade Produção Etanol Anidro (m³/d),Capacidade Produção Etanol Hidratado (m³/d)";

    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(responder(pedido));
    }

    /// <summary>Monta um ZIP como o da ANP: um ou mais CSVs de capacidade, com o BOM que ela grava.</summary>
    private static byte[] ZipCom(params (string Nome, string[] Linhas)[] arquivos)
    {
        using var memoria = new MemoryStream();

        using (var pacote = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (nome, linhas) in arquivos)
            {
                var entrada = pacote.CreateEntry(nome);
                using var fluxo = entrada.Open();

                // COM BOM, como o arquivo real: é o que faz "São Paulo" chegar legível.
                var conteudo = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
                    .GetBytes(string.Join("\r\n", [Cabecalho, .. linhas]));

                fluxo.Write(conteudo, 0, conteudo.Length);
            }
        }

        return memoria.ToArray();
    }

    private static LeitorDaAnp Leitor(byte[] zip, List<Uri>? pedidos = null) =>
        new(new HttpClient(new TratadorFalso(pedido =>
        {
            pedidos?.Add(pedido.RequestUri!);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(zip) };
        })));

    [Fact]
    public async Task O_mes_mais_recente_e_por_DATA_e_nao_por_ordem_alfabetica()
    {
        // "12/2021" ganharia de "07/2026" numa comparação de texto. O leitor precisa ler como data.
        var zip = ZipCom(("Etanol_Capacidade 2021-2026.csv", [
            "12/2021,USINA ANTIGA S/A,11111111000111,SUDESTE,São Paulo,BARRA BONITA,100,200",
            "07/2026,USINA DE HOJE S/A,22222222000122,SUDESTE,São Paulo,PRADOPOLIS,300,400"
        ]));

        var usinas = await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        usinas.Should().ContainSingle("só o mês mais recente entra");
        usinas[0].MesDeReferencia.Should().Be("07/2026");
        usinas[0].RazaoSocial.Should().Be("USINA DE HOJE S/A");
    }

    [Fact]
    public async Task A_virgula_da_razao_social_nao_desloca_as_colunas()
    {
        // O DEFEITO QUE ESTE TESTE IMPEDE: com Split(','), o CNPJ cairia na coluna da região e o
        // município viraria a capacidade — sem erro nenhum, só dado errado.
        var zip = ZipCom(("Etanol_Capacidade 2021-2026.csv", [
            "07/2026,\"AGRO INDUSTRIAL X, S/A\",33333333000133,SUDESTE,São Paulo,SERTAOZINHO,500,600"
        ]));

        var usina = (await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None)).Single();

        usina.RazaoSocial.Should().Be("AGRO INDUSTRIAL X, S/A");
        usina.Cnpj.Should().Be("33333333000133");
        usina.Municipio.Should().Be("SERTAOZINHO");
        usina.CapacidadeDeAnidroBruta.Should().Be("500");
        usina.CapacidadeDeHidratadoBruta.Should().Be("600");
    }

    [Fact]
    public async Task Os_dois_arquivos_de_capacidade_do_zip_sao_lidos()
    {
        // A ANP separa a série em dois CSVs. Ler só o primeiro perderia a metade recente — ou a antiga,
        // conforme a ordem em que o ZIP os guardou, que não é garantida.
        var zip = ZipCom(
            ("Etanol_Capacidade 2018-2020.csv", ["05/2019,USINA VELHA S/A,44444444000144,SUDESTE,São Paulo,ARARAS,10,20"]),
            ("Etanol_Capacidade 2021-2026.csv", ["07/2026,USINA NOVA S/A,55555555000155,SUDESTE,São Paulo,ARARAS,30,40"]),
            ("Etanol_MatériaPrima.csv", ["07/2026,IRRELEVANTE,66666666000166,SUDESTE,São Paulo,ARARAS,1,2"]));

        var usinas = await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        usinas.Should().ContainSingle("os dois foram lidos, e 07/2026 é o mês mais recente entre eles");
        usinas[0].Cnpj.Should().Be("55555555000155",
            "se só o arquivo de 2018-2020 fosse lido, a resposta seria a usina de 2019");
    }

    [Fact]
    public async Task So_a_uf_pedida_entra_e_o_acento_do_nome_do_estado_sobrevive_ao_zip()
    {
        // O nome do estado vem por extenso e COM acento. Ler o arquivo na codificação errada faria
        // "São Paulo" virar outra coisa, e o filtro devolveria zero usina sem dizer por quê.
        var zip = ZipCom(("Etanol_Capacidade 2021-2026.csv", [
            "07/2026,USINA PAULISTA S/A,77777777000177,SUDESTE,São Paulo,JAU,700,800",
            "07/2026,USINA GOIANA S/A,88888888000188,CENTRO OESTE,Goiás,GOIANESIA,900,1000"
        ]));

        var usinas = await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        usinas.Should().ContainSingle();
        usinas[0].Municipio.Should().Be("JAU");
    }

    [Fact]
    public async Task Capacidade_em_branco_chega_nula_e_zero_chega_zero()
    {
        // ZERO E AUSÊNCIA SÃO COISAS DIFERENTES, como na PAM: em 07/2026 uma das 145 usinas paulistas
        // estava com as duas capacidades zeradas — usina parada, autorização viva.
        var zip = ZipCom(("Etanol_Capacidade 2021-2026.csv", [
            "07/2026,USINA PARADA S/A,99999999000199,SUDESTE,São Paulo,ANDRADINA,0,0",
            "07/2026,USINA SEM ANIDRO S/A,10101010000110,SUDESTE,São Paulo,ARACATUBA,,1200"
        ]));

        var usinas = await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        var parada = usinas.Single(u => u.Cnpj == "99999999000199");
        parada.CapacidadeDeAnidroBruta.Should().Be("0");
        parada.CapacidadeDeHidratadoBruta.Should().Be("0");

        var semAnidro = usinas.Single(u => u.Cnpj == "10101010000110");
        semAnidro.CapacidadeDeAnidroBruta.Should().BeNull("campo em branco não é zero");
        semAnidro.CapacidadeDeHidratadoBruta.Should().Be("1200");
    }

    [Fact]
    public async Task Sem_usina_da_uf_a_resposta_e_vazia_e_nao_um_erro()
    {
        var zip = ZipCom(("Etanol_Capacidade 2021-2026.csv", [
            "07/2026,USINA GOIANA S/A,88888888000188,CENTRO OESTE,Goiás,GOIANESIA,900,1000"
        ]));

        var usinas = await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        usinas.Should().BeEmpty();
    }

    [Fact]
    public async Task Zip_sem_o_csv_de_capacidade_para_dizendo_o_que_achou()
    {
        // A ANP pode renomear o arquivo. Quando isso acontecer, a carga precisa PARAR com a lista do
        // que veio — e não gravar zero usina como se todas tivessem fechado.
        var zip = ZipCom(("Etanol_Produção.csv", ["07/2026,X,11111111000111,SUDESTE,São Paulo,JAU,1,2"]));

        var ler = async () => await Leitor(zip).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        (await ler.Should().ThrowAsync<InvalidDataException>())
            .WithMessage("*Etanol_Produção.csv*", "a mensagem precisa dizer o que o pacote tinha");
    }

    [Fact]
    public async Task O_endereco_lido_e_o_dos_dados_abertos_da_anp()
    {
        var pedidos = new List<Uri>();
        var zip = ZipCom(("Etanol_Capacidade 2021-2026.csv", [
            "07/2026,USINA S/A,11111111000111,SUDESTE,São Paulo,JAU,1,2"
        ]));

        await Leitor(zip, pedidos).LerUsinasDeEtanolAsync(LeitorDaAnp.SaoPauloNaAnp, CancellationToken.None);

        pedidos.Should().ContainSingle();
        pedidos[0].ToString().Should().Be(LeitorDaAnp.EnderecoDaBaseDeEtanol);
        pedidos[0].Host.Should().Be("www.gov.br", "é dado aberto oficial, sem CAPTCHA e sem credencial");
    }

    [Theory]
    [InlineData("a,b,c", new[] { "a", "b", "c" })]
    [InlineData("a,\"b,c\",d", new[] { "a", "b,c", "d" })]
    [InlineData("a,\"b\"\"c\",d", new[] { "a", "b\"c", "d" })]
    [InlineData("a,,c", new[] { "a", "", "c" })]
    [InlineData("a, b ,c", new[] { "a", "b", "c" })]
    public void A_separacao_de_csv_respeita_aspas_virgula_e_espaco(string linha, string[] esperado)
    {
        LeitorDaAnp.SepararCsv(linha).Should().Equal(esperado);
    }

    [Fact]
    public void O_mes_e_lido_como_data_no_formato_da_anp()
    {
        LeitorDaAnp.ComoData("07/2026").Should().Be(new DateOnly(2026, 7, 1));
        LeitorDaAnp.ComoData(" 01/2019 ").Should().Be(new DateOnly(2019, 1, 1), "o arquivo às vezes traz espaço");

        var invalido = () => LeitorDaAnp.ComoData("2026-07");
        invalido.Should().Throw<FormatException>("formato desconhecido não se adivinha");
    }
}
