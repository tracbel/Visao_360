using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O CATÁLOGO DE CULTURAS E CATEGORIAS DE MÁQUINA (issue 165) — o que liga o vocabulário das cinco
/// fontes e faz cultura nova entrar pela tela.
///
/// <para>Os códigos e os vínculos são os do anexo 49C, medidos nas fontes em 22/09/2026.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class CatalogoDeCulturasTestes
{
    private static Cultura Cafe() =>
        Cultura.Registrar("cafe", "Café", SegmentoDaCultura.Cafe, "saca de 60 kg", 60m, "CONAB", "11195", "CAFÉ ARÁBICA");

    [Fact]
    public void A_cultura_nasce_ativa_e_com_o_codigo_em_caixa_alta()
    {
        // O CÓDIGO É A IDENTIDADE, e quem digita não precisa acertar a caixa: "cafe", "Cafe" e "CAFE" são
        // a mesma cultura, e duas linhas com grafias diferentes seriam duas culturas no mesmo mapa.
        var cafe = Cafe();

        cafe.Codigo.Should().Be("CAFE");
        cafe.EstaAtiva.Should().BeTrue();
        cafe.QuilosPorUnidade.Should().Be(60m, "é o fator que transforma o R$/kg da CONAB no preço da saca");
    }

    [Fact]
    public void A_fonte_do_preco_e_o_produto_dela_andam_juntos()
    {
        // Uma fonte sem produto não acha nada, e um produto sem fonte não diz onde procurar. Os dois
        // vazios são legítimos: significam "esta cultura não tem preço publicado".
        var soFonte = () => Cultura.Registrar("X", "X", SegmentoDaCultura.Graos, "saca", 60m, "CONAB", null);
        var soProduto = () => Cultura.Registrar("Y", "Y", SegmentoDaCultura.Graos, "saca", 60m, null, "4744");

        soFonte.Should().Throw<RegraDeNegocioViolada>();
        soProduto.Should().Throw<RegraDeNegocioViolada>();

        Cultura.Registrar("Z", "Z", SegmentoDaCultura.Graos, "saca de 60 kg", 60m)
            .FonteDoPreco.Should().BeNull("cultura sem preço publicado é caso legítimo, não erro");
    }

    [Fact]
    public void Unidade_comercial_sem_peso_e_recusada()
    {
        // Zero dividiria o preço por nada, e negativo inverteria o sinal do que o produtor recebe.
        var semPeso = () => Cultura.Registrar("X", "X", SegmentoDaCultura.Graos, "saca", 0m);

        semPeso.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Redefinir_diz_se_mudou_e_desligar_nao_apaga()
    {
        var cafe = Cafe();

        cafe.Redefinir("Café", SegmentoDaCultura.Cafe, "saca de 60 kg", 60m, "CONAB", "11195", "CAFÉ ARÁBICA")
            .Should().BeFalse("nada mudou");

        cafe.Redefinir("Café arábica", SegmentoDaCultura.Cafe, "saca de 60 kg", 60m, "CONAB", "11195", "CAFÉ ARÁBICA")
            .Should().BeTrue();
        cafe.Nome.Should().Be("Café arábica");

        // DESLIGAR NÃO APAGA: o histórico de preço, custo e potencial continua explicando o passado.
        cafe.DefinirAtiva(false).Should().BeTrue();
        cafe.EstaAtiva.Should().BeFalse();
        cafe.DefinirAtiva(false).Should().BeFalse("já estava desligada");
    }

    [Fact]
    public void O_vinculo_com_a_pam_guarda_quem_entra_na_soma_da_lavoura()
    {
        // O "Total" do café já contém Arábica e Canephora: somar os três dobraria a área do café. A marca
        // vive no vínculo, e não numa lista de exceções escrita no código.
        var total = ProdutoDaPamNaCultura.Ligar(1, 40139, "Café (em grão) Total", entraNaSomaDaLavoura: true);
        var arabica = ProdutoDaPamNaCultura.Ligar(1, 40140, "Café (em grão) Arábica", entraNaSomaDaLavoura: false);

        total.EntraNaSomaDaLavoura.Should().BeTrue();
        arabica.EntraNaSomaDaLavoura.Should().BeFalse();
    }

    [Fact]
    public void Vinculo_sem_codigo_ou_sem_rotulo_e_recusado()
    {
        var semCodigo = () => ProdutoDaPamNaCultura.Ligar(1, 0, "Café", true);
        var semRotulo = () => ProdutoDaPamNaCultura.Ligar(1, 40139, "  ", true);
        var sicorSemCodigo = () => ProdutoDoSicorNaCategoria.Ligar(1, 0, "TRATOR");

        semCodigo.Should().Throw<RegraDeNegocioViolada>();
        semRotulo.Should().Throw<RegraDeNegocioViolada>();
        sicorSemCodigo.Should().Throw<RegraDeNegocioViolada>();
    }

    // =============================================================================================
    // A semente — o anexo 49C
    // =============================================================================================

    [Fact]
    public void A_semente_traz_as_seis_culturas_do_pedido_com_o_vinculo_de_cada_fonte()
    {
        var codigos = CatalogoSemeado.Culturas.Select(c => c.Codigo);

        codigos.Should().BeEquivalentTo(["CAFE", "CANA", "SOJA", "MILHO", "LARANJA", "AMENDOIM"]);

        var cafe = CatalogoSemeado.Culturas.Single(c => c.Codigo == "CAFE");
        cafe.ProdutoDoPreco.Should().Be("11195", "é o id_produto do café arábica tipo 6 na CONAB (anexo 49C)");
        cafe.SerieDeCusto.Should().Be("CAFÉ ARÁBICA");
        cafe.QuilosPorUnidade.Should().Be(60m);

        CatalogoSemeado.Culturas.Single(c => c.Codigo == "LARANJA").QuilosPorUnidade.Should().Be(40.8m,
            "a laranja é negociada em caixa de 40,8 kg");
    }

    [Fact]
    public void So_o_cafe_total_entra_na_soma_da_lavoura()
    {
        // É a regra que impede contar o café duas vezes — e ela agora vem do catálogo, não de uma lista
        // de códigos escrita dentro da consulta.
        var cafe = CatalogoSemeado.Culturas.Single(c => c.Codigo == "CAFE");

        cafe.Produtos.Should().HaveCount(3);
        cafe.Produtos.Where(p => p.NaSoma).Select(p => p.Codigo).Should().Equal([40139]);
    }

    [Fact]
    public void Todo_produto_da_pam_pertence_a_uma_cultura_so()
    {
        // O índice único do banco é sobre o código do produto: o mesmo produto em duas culturas somaria a
        // mesma terra duas vezes. A semente não pode nascer violando isso.
        var todos = CatalogoSemeado.Culturas.SelectMany(c => c.Produtos).Select(p => p.Codigo).ToList();

        todos.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Tres_categorias_nascem_sem_produto_do_sicor_e_isso_nao_e_erro()
    {
        // O investimento do Banco Central não separa plantadeira, pulverizador nem agricultura de precisão
        // (anexo 49C). A categoria existe do mesmo jeito: só o crédito é que não a enxerga.
        var semSicor = CatalogoSemeado.Categorias.Where(c => c.ProdutosDoSicor.Count == 0).Select(c => c.Codigo);

        semSicor.Should().BeEquivalentTo(["PLANTADEIRA", "PULVERIZADOR", "PRECISAO"]);
    }

    [Fact]
    public void Os_produtos_de_maquina_do_sicor_da_semente_sao_os_mesmos_que_o_credito_usa()
    {
        // A issue 157 tirou a lista [7080, 4860, 2700] da infraestrutura e a deixou no domínio, "até a 165
        // pô-la no catálogo". Este teste prende as duas ao mesmo conjunto: divergir seria contar o crédito
        // de máquina de um jeito no painel e de outro no catálogo.
        var doCatalogo = CatalogoSemeado.Categorias.SelectMany(c => c.ProdutosDoSicor).Select(p => p.Codigo);

        doCatalogo.Should().BeEquivalentTo(ParametroDoPotencial.ProdutosDeMaquinaNoSicor);
    }
}
