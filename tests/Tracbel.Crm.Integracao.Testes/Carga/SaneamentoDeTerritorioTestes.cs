using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Integracao.Carga;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Carga;

/// <summary>
/// AS DECISÕES DA CARGA DO TERRITÓRIO (documento 32), exercitadas com os casos que existem de
/// verdade no catálogo e nas planilhas — o nome cortado em 20 caracteres, as duas grafias do
/// apóstrofo, a vaga "a contratar", a célula com duas pessoas.
///
/// <para><b>Os nomes de pessoa aqui são inventados.</b> As planilhas reais têm nome de funcionário
/// e não entram no repositório; o que se testa é a regra, e a regra não depende de quem é.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class SaneamentoDeTerritorioTestes
{
    // =============================================================================================
    // Chaves de comparação
    // =============================================================================================

    [Theory]
    [InlineData("Arco-Íris", "ARCO IRIS")]
    [InlineData("  São   José do Rio Preto ", "SAO JOSE DO RIO PRETO")]
    [InlineData("Estrela d'Oeste", "ESTRELA D'OESTE")]
    [InlineData("Estrela d’Oeste", "ESTRELA D'OESTE")]
    public void A_chave_exata_ignora_acento_caixa_e_hifen_e_preserva_o_apostrofo(string nome, string esperada) =>
        SaneamentoDeTerritorio.ChaveExata(nome).Should().Be(esperada);

    [Fact]
    public void Sem_o_apostrofo_as_duas_grafias_do_legado_viram_a_mesma_chave() =>
        SaneamentoDeTerritorio.ChaveSemApostrofo("ESTRELA D'OESTE")
            .Should().Be(SaneamentoDeTerritorio.ChaveSemApostrofo("ESTRELA D OESTE"));

    [Fact]
    public void A_chave_da_colacao_so_ignora_caixa_e_acento_como_o_banco()
    {
        SaneamentoDeTerritorio.ChaveDaColacao("Arco-Íris").Should().Be(SaneamentoDeTerritorio.ChaveDaColacao("ARCO-IRIS"));
        SaneamentoDeTerritorio.ChaveDaColacao("Arco-Íris").Should().NotBe(SaneamentoDeTerritorio.ChaveDaColacao("ARCO IRIS"),
            "a colação Latin1_General_CI_AI não iguala hífen e espaço — e é contra ela que o índice único compara");
    }

    // =============================================================================================
    // Reconhecimento no IBGE
    // =============================================================================================

    private static readonly MunicipioDoIbge RibeiraoPreto = new(3543402, "Ribeirão Preto", "SP");
    private static readonly MunicipioDoIbge EstrelaDOeste = new(3515202, "Estrela d'Oeste", "SP");
    private static readonly MunicipioDoIbge SantaCruzDaEsperanca = new(3546256, "Santa Cruz da Esperança", "SP");
    private static readonly MunicipioDoIbge SaoSebastiaoDoRioPreto = new(3164100, "São Sebastião do Rio Preto", "MG");
    private static readonly MunicipioDoIbge SaoSebastiaoDoRioVerde = new(3164209, "São Sebastião do Rio Verde", "MG");
    private static readonly MunicipioDoIbge Cajuru = new(3509601, "Cajuru", "SP");
    private static readonly MunicipioDoIbge Altinopolis = new(3501004, "Altinópolis", "SP");

    private static readonly MunicipioDoIbge[] Oficiais =
        [RibeiraoPreto, EstrelaDOeste, SantaCruzDaEsperanca, SaoSebastiaoDoRioPreto, SaoSebastiaoDoRioVerde, Cajuru, Altinopolis];

    private static readonly MunicipioDoCatalogo[] Catalogo =
    [
        new(1, "RIBEIRAO PRETO", "SP", null),
        new(2, "ESTRELA D'OESTE", "SP", null),
        new(3, "ESTRELA D OESTE", "SP", null),
        new(4, "SANTA CRUZ DA ESPERA", "SP", null),
        new(5, "A CADASTRAR", "SP", null),
        new(6, "SAO SEBASTIAO DO RIO", "MG", null),
        new(7, "Cajuru", "SP", 3509601)
    ];

    [Fact]
    public void Nome_igual_casa_na_primeira_tentativa()
    {
        var resultado = SaneamentoDeTerritorio.Reconhecer(Catalogo, Oficiais);

        resultado.Reconhecidos.Should().ContainSingle(r => r.MunicipioId == 1)
            .Which.Should().Be(new ReconhecimentoNoIbge(1, "RIBEIRAO PRETO", RibeiraoPreto, FormaDeReconhecimento.NomeIgual));
    }

    [Fact]
    public void A_segunda_grafia_do_apostrofo_nao_e_fundida_e_fica_para_revisao()
    {
        var resultado = SaneamentoDeTerritorio.Reconhecer(Catalogo, Oficiais);

        resultado.Reconhecidos.Should().ContainSingle(r => r.MunicipioId == 2)
            .Which.Forma.Should().Be(FormaDeReconhecimento.NomeIgual);

        resultado.Variantes.Should().ContainSingle()
            .Which.Should().Be(new VarianteDeGrafia(3, "ESTRELA D OESTE", EstrelaDOeste, FormaDeReconhecimento.GrafiaDoApostrofo),
                "o código já pertence à linha 2; juntar as duas mudaria o município de endereços sem ninguém decidir");
    }

    [Fact]
    public void A_grafia_cortada_de_um_municipio_ja_reconhecido_e_variante_de_nome_cortado()
    {
        MunicipioDoCatalogo[] catalogo =
        [
            new(20, "SANTA CRUZ DA ESPERANCA", "SP", null),
            new(21, "SANTA CRUZ DA ESPERA", "SP", null)
        ];

        var resultado = SaneamentoDeTerritorio.Reconhecer(catalogo, Oficiais);

        resultado.Reconhecidos.Should().ContainSingle(r => r.MunicipioId == 20);
        resultado.Variantes.Should().ContainSingle().Which.Should().Be(
            new VarianteDeGrafia(21, "SANTA CRUZ DA ESPERA", SantaCruzDaEsperanca, FormaDeReconhecimento.NomeTruncadoNaOrigem),
            "é a forma que a correção autorizada consolida — e só ela (documento 32, seção 4.6)");
    }

    [Fact]
    public void Nome_cortado_em_vinte_caracteres_casa_quando_o_prefixo_e_de_um_so_municipio() =>
        SaneamentoDeTerritorio.Reconhecer(Catalogo, Oficiais).Reconhecidos
            .Should().ContainSingle(r => r.MunicipioId == 4)
            .Which.Should().Be(new ReconhecimentoNoIbge(4, "SANTA CRUZ DA ESPERA", SantaCruzDaEsperanca, FormaDeReconhecimento.NomeTruncadoNaOrigem));

    [Fact]
    public void Prefixo_de_dois_municipios_nao_escolhe_nenhum()
    {
        var resultado = SaneamentoDeTerritorio.Reconhecer(Catalogo, Oficiais);

        resultado.SemCorrespondencia.Select(m => m.Id).Should().BeEquivalentTo([5, 6],
            "\"SAO SEBASTIAO DO RIO\" é Rio Preto e é Rio Verde — o documento 26 mediu esse caso em MG");
    }

    [Fact]
    public void Linha_ja_reconhecida_nao_e_tocada_e_o_oficial_sem_linha_e_apontado()
    {
        var resultado = SaneamentoDeTerritorio.Reconhecer(Catalogo, Oficiais);

        resultado.Reconhecidos.Should().NotContain(r => r.MunicipioId == 7);
        resultado.AusentesNoCatalogo.Should().BeEquivalentTo([SaoSebastiaoDoRioPreto, SaoSebastiaoDoRioVerde, Altinopolis]);
    }

    [Fact]
    public void Rodar_de_novo_sobre_o_catalogo_reconhecido_nao_reconhece_nada()
    {
        var primeira = SaneamentoDeTerritorio.Reconhecer(Catalogo, Oficiais);
        var codigos = primeira.Reconhecidos.ToDictionary(r => r.MunicipioId, r => r.Oficial);

        var depois = Catalogo
            .Select(m => codigos.TryGetValue(m.Id, out var oficial) ? m with { Nome = oficial.Nome, CodigoIbge = oficial.Codigo } : m)
            .ToList();

        var segunda = SaneamentoDeTerritorio.Reconhecer(depois, Oficiais);

        segunda.Reconhecidos.Should().BeEmpty();
        segunda.Variantes.Should().ContainSingle(v => v.MunicipioId == 3);
    }

    [Fact]
    public void Entre_duas_linhas_da_mesma_chave_vence_a_que_o_banco_ja_escreve_igual_ao_oficial()
    {
        MunicipioDoIbge arcoIris = new(3503356, "Arco-Íris", "SP");
        MunicipioDoCatalogo[] catalogo = [new(10, "ARCO IRIS", "SP", null), new(11, "ARCO-IRIS", "SP", null)];

        var resultado = SaneamentoDeTerritorio.Reconhecer(catalogo, [arcoIris]);

        resultado.Reconhecidos.Should().ContainSingle().Which.MunicipioId.Should().Be(11,
            "renomear a linha 10 para \"Arco-Íris\" colidiria com a linha 11 no índice único de nome");
        resultado.Variantes.Should().ContainSingle().Which.MunicipioId.Should().Be(10);
    }

    // =============================================================================================
    // Responsáveis
    // =============================================================================================

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<long>> Usuarios = SaneamentoDeTerritorio.IndexarUsuarios(
    [
        (1L, "fulano.detal@tracbel.com.br", "FULANO DE TAL", "Fulano de Tal"),
        (2L, "beltrano.silva@sem-email.vortice.invalid", "BELTRANO SILVA", "Beltrano da Silva"),
        (3L, "beltrano.souza@tracbel.com.br", "BELTRANO SOUZA", "Beltrano Souza"),
        (4L, "ciclano@tracbel.com.br", "CICLANO", "Ciclano"),
        (5L, "ciclano.dois@tracbel.com.br", "CICLANO", "Ciclano Dois")
    ]);

    [Theory]
    [InlineData("A Contratar 3")]
    [InlineData("CONTRATAR 4")]
    [InlineData("a contratar")]
    public void Vaga_em_aberto_nao_e_pessoa(string celula) =>
        SaneamentoDeTerritorio.IdentificarResponsavel(celula, Usuarios)
            .Should().Be((SituacaoDoResponsavel.VagaAContratar, (long?)null));

    [Theory]
    [InlineData("FULANO.DETAL", 1L)]
    [InlineData("Beltrano Silva", 2L)]
    public void Login_e_nome_de_exibicao_identificam_quando_casam_com_um_so_usuario(string celula, long usuarioId) =>
        SaneamentoDeTerritorio.IdentificarResponsavel(celula, Usuarios)
            .Should().Be((SituacaoDoResponsavel.UsuarioIdentificado, (long?)usuarioId));

    [Theory]
    [InlineData("BELTRANO", "primeiro nome não é igualdade: casaria com dois usuários por semelhança")]
    [InlineData("CICLANO", "o nome de exibição é de duas pessoas")]
    [InlineData("FULANO.DETAL // BELTRANO.SILVA", "duas pessoas na mesma célula")]
    [InlineData("NINGUEM.CONHECIDO", "não há usuário com esse nome")]
    public void Sem_igualdade_com_um_so_usuario_fica_nao_identificado(string celula, string porque) =>
        SaneamentoDeTerritorio.IdentificarResponsavel(celula, Usuarios)
            .Should().Be((SituacaoDoResponsavel.NaoIdentificado, (long?)null), porque);

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" - ", true)]
    [InlineData("Adolfo", false)]
    public void O_traco_das_planilhas_e_celula_vazia(string? celula, bool vazia) =>
        SaneamentoDeTerritorio.CelulaVazia(celula).Should().Be(vazia);

    // =============================================================================================
    // As medidas do SIDRA (área plantada, colhida, quantidade e valor)
    // =============================================================================================

    [Theory]
    [InlineData("-", 0)]
    [InlineData("37104", 37104)]
    [InlineData("12.5", 12.5)]
    [InlineData("1200000", 1200000)]
    public void Zero_do_ibge_vira_zero_e_numero_vira_numero(string valor, decimal esperado) =>
        SaneamentoDeTerritorio.MedidaDoSidra(valor).Should().Be(esperado);

    [Theory]
    [InlineData("...")]
    [InlineData("..")]
    [InlineData("X")]
    public void Dado_nao_disponivel_ou_sigiloso_vira_nulo_e_nunca_zero(string valor) =>
        SaneamentoDeTerritorio.MedidaDoSidra(valor).Should().BeNull();

    [Fact]
    public void Variavel_que_nao_veio_na_resposta_tambem_e_nula()
    {
        // NULO NA ENTRADA É OUTRA COISA: não é o "..." do IBGE (existe e não foi divulgado), é o
        // SIDRA não ter trazido a variável naquele lote. As duas viram nulo, e nenhuma vira zero.
        SaneamentoDeTerritorio.MedidaDoSidra(null).Should().BeNull();
    }

    [Fact]
    public void Simbolo_desconhecido_para_a_carga_em_vez_de_ser_adivinhado()
    {
        var ler = () => SaneamentoDeTerritorio.MedidaDoSidra("n/d");
        ler.Should().Throw<FormatException>();
    }
}
