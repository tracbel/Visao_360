using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Saneamento;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Carga;

/// <summary>
/// AS TRADUÇÕES DE DOMÍNIO ABERTO PARA DOMÍNIO FECHADO, exercitadas com os valores REAIS medidos
/// no sistema de origem.
///
/// <para>São as decisões mais discutíveis da carga do documento 25, e é por isso que cada uma
/// tem um teste com o valor que existe de verdade lá — <c>FINALIZADO</c> ao lado de
/// <c>FINALIZADA</c>, <c>64</c> onde deveria haver uma classe de cliente, a letra <c>M</c> de
/// natureza que ninguém documentou. Uma decisão discutível sem teste é uma opinião; com teste,
/// é um contrato que alguém pode discordar e mudar sabendo o que quebra.</para>
///
/// <para>Nada aqui depende de VPN nem de banco: a tradução é função pura, e é justamente por
/// isso que ela pode ser exercitada valor a valor.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class SaneamentoDeProcessoTestes
{
    // =============================================================================================
    // Situação do processo
    // =============================================================================================

    /// <summary>Os status que declaram desfecho comercial são traduzidos sem assumir nada.</summary>
    /// <param name="statusNaOrigem">O texto como a origem o guarda.</param>
    /// <param name="esperada">A situação no domínio fechado.</param>
    [Theory]
    [InlineData("FATURADO", SituacaoDoProcesso.Ganho)]
    [InlineData("VENDA REALIZADA", SituacaoDoProcesso.Ganho)]
    [InlineData("VENDA PERDIDA", SituacaoDoProcesso.Perdido)]
    [InlineData("DESISTIU DA COMPRA", SituacaoDoProcesso.Perdido)]
    [InlineData("PEDIDO NÃO APROVADO", SituacaoDoProcesso.Perdido)]
    [InlineData("CANCELADO", SituacaoDoProcesso.Cancelado)]
    [InlineData("PARADO", SituacaoDoProcesso.Suspenso)]
    public void Traduz_o_status_que_declara_desfecho_comercial(
        string statusNaOrigem, SituacaoDoProcesso esperada)
    {
        var correcoes = new List<CorrecaoAplicada>();

        var (situacao, veioDaOrigem) = SaneamentoDeProcesso.Situacao(statusNaOrigem, correcoes);

        situacao.Should().Be(esperada);
        veioDaOrigem.Should().BeTrue("o status declara o desfecho e nada foi assumido");
        correcoes.Should().BeEmpty("não houve tradução assumida a registrar");
    }

    /// <summary>
    /// <c>CANCELADO</c> e <c>CANCELADA</c> convivem na mesma coluna, e as duas grafias caem no
    /// mesmo lugar — 41.410 linhas numa, 2.412 na outra, medidas no banco de origem.
    /// </summary>
    [Fact]
    public void As_duas_grafias_de_cancelamento_caem_no_mesmo_lugar()
    {
        var correcoes = new List<CorrecaoAplicada>();

        SaneamentoDeProcesso.Situacao("CANCELADO", correcoes).Situacao
            .Should().Be(SituacaoDoProcesso.Cancelado);

        SaneamentoDeProcesso.Situacao("CANCELADA", correcoes).Situacao
            .Should().Be(SituacaoDoProcesso.Cancelado);
    }

    /// <summary>
    /// A DECISÃO MAIS DELICADA DA CARGA, escrita como teste.
    ///
    /// <para>Um processo que a origem encerrou sem declarar desfecho comercial —
    /// <c>FINALIZADO</c>, <c>FINALIZADA</c>, <c>CONCLUIDO</c>, ou o status em branco de 480.428
    /// linhas — <b>continua aberto</b> no CRM novo. Empurrá-lo para Ganho inventaria uma venda;
    /// para Perdido, uma perda; para Cancelado, repetiria o defeito do legado, onde o
    /// cancelamento virou a lixeira que engoliu 5.991 negociações de 2026 sem motivo nenhum.</para>
    ///
    /// <para>A tradução assumida fica REGISTRADA, com o valor da origem — é o princípio 1.4 do
    /// documento 16: nada é traduzido em silêncio.</para>
    /// </summary>
    /// <param name="statusNaOrigem">O texto como a origem o guarda.</param>
    [Theory]
    [InlineData("FINALIZADO")]
    [InlineData("FINALIZADA")]
    [InlineData("CONCLUIDO")]
    [InlineData("EM ABERTO")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("* Não informado *")]
    public void O_status_que_nao_declara_desfecho_comercial_mantem_o_processo_aberto(string statusNaOrigem)
    {
        var correcoes = new List<CorrecaoAplicada>();

        var (situacao, veioDaOrigem) = SaneamentoDeProcesso.Situacao(statusNaOrigem, correcoes);

        situacao.Should().Be(SituacaoDoProcesso.Aberto);
        veioDaOrigem.Should().BeFalse("a situação foi assumida, não lida");

        correcoes.Should().ContainSingle()
            .Which.Campo.Should().Be("situacaoDoProcesso",
                "toda tradução assumida fica registrada com o valor que a origem tinha");
    }

    // =============================================================================================
    // Situação da tarefa
    // =============================================================================================

    /// <summary>
    /// Concluir exige as TRÊS coisas que a restrição de verificação do banco cobra: data, quem
    /// concluiu e desfecho.
    /// </summary>
    [Fact]
    public void Conclui_a_tarefa_so_quando_a_origem_traz_data_desfecho_e_quem_concluiu()
    {
        SaneamentoDeProcesso.SituacaoDaTarefaDe("S", true, true, true)
            .Should().Be(SituacaoDaTarefa.Concluida);
    }

    /// <summary>
    /// Faltando qualquer uma das três, a tarefa fica PENDENTE — visível na agenda, em vez de
    /// fechada sem ninguém conseguir dizer com que desfecho.
    /// </summary>
    /// <param name="realizada">A letra de "realizada" da origem.</param>
    /// <param name="temData">Se a origem traz a data.</param>
    /// <param name="temDesfecho">Se a origem traz o desfecho.</param>
    /// <param name="temQuem">Se a origem traz quem concluiu.</param>
    [Theory]
    [InlineData("S", false, true, true)]
    [InlineData("S", true, false, true)]
    [InlineData("S", true, true, false)]
    [InlineData("N", true, true, true)]
    [InlineData(null, true, true, true)]
    public void Tarefa_sem_as_tres_coisas_da_conclusao_fica_pendente(
        string? realizada, bool temData, bool temDesfecho, bool temQuem)
    {
        SaneamentoDeProcesso.SituacaoDaTarefaDe(realizada, temData, temDesfecho, temQuem)
            .Should().Be(SituacaoDaTarefa.Pendente);
    }

    // =============================================================================================
    // Classe do desfecho
    // =============================================================================================

    /// <summary>
    /// A classe do desfecho sai do que ele FAZ com o processo, e a ordem importa: cancelar vence
    /// perder, perder vence avançar.
    /// </summary>
    /// <param name="statusDeDestino">O status que o mapeamento manda gravar.</param>
    /// <param name="declaraFaseSeguinte">Se o mapeamento aponta fase de destino.</param>
    /// <param name="esperada">A classe esperada.</param>
    [Theory]
    [InlineData("CANCELADO", true, ClasseDeResultado.Cancelamento)]
    [InlineData("VENDA PERDIDA", true, ClasseDeResultado.Perda)]
    [InlineData("FATURADO", false, ClasseDeResultado.Avanco)]
    [InlineData("", true, ClasseDeResultado.Avanco)]
    public void A_classe_do_desfecho_sai_do_que_ele_faz_com_o_processo(
        string statusDeDestino, bool declaraFaseSeguinte, ClasseDeResultado esperada)
    {
        var (classe, veioDoMapeamento) =
            SaneamentoDeProcesso.ClasseDoResultado(statusDeDestino, declaraFaseSeguinte);

        classe.Should().Be(esperada);
        veioDoMapeamento.Should().BeTrue();
    }

    /// <summary>
    /// [V] 84% dos desfechos mapeados do fluxo de vendas não declaram nem fase nem status — e é
    /// por isso que o desfecho mais lançado do CRM inteiro é "Contato Em Andamento", que não move
    /// nada. Eles entram como MANUTENÇÃO, e a carga conta quantos foram.
    /// </summary>
    [Fact]
    public void O_desfecho_que_nao_declara_nada_entra_como_manutencao_e_a_suposicao_fica_marcada()
    {
        var (classe, veioDoMapeamento) =
            SaneamentoDeProcesso.ClasseDoResultado(statusDeDestino: null, declaraFaseSeguinte: false);

        classe.Should().Be(ClasseDeResultado.Manutencao);
        veioDoMapeamento.Should().BeFalse("nada no mapeamento da origem sustenta essa classe");
    }

    // =============================================================================================
    // Natureza da interação
    // =============================================================================================

    /// <summary>
    /// A natureza separa três coisas que a linha do tempo do legado mostra misturadas — e o
    /// terceiro caso, o registro que o servidor escreveu sozinho, é um quarto do volume do ano.
    /// </summary>
    [Fact]
    public void O_que_o_servidor_da_origem_escreveu_entra_como_Sistema_e_nao_como_relacionamento()
    {
        var correcoes = new List<CorrecaoAplicada>();
        var autores = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "VRTCSERVER" };

        SaneamentoDeProcesso.Natureza("A", "VRTCSERVER", autores, correcoes)
            .Should().Be(NaturezaDaInteracao.Sistema);

        SaneamentoDeProcesso.Natureza("A", "GABRIELA.MIELE", autores, correcoes)
            .Should().Be(NaturezaDaInteracao.Ativa);

        SaneamentoDeProcesso.Natureza("R", "GABRIELA.MIELE", autores, correcoes)
            .Should().Be(NaturezaDaInteracao.Receptiva);

        correcoes.Should().BeEmpty("nenhuma dessas três foi assumida");
    }

    /// <summary>
    /// A letra <c>M</c> existe no dado (39 linhas em 2026) e não tem significado documentado. Ela
    /// entra como Ativa — o caso de 99% do histórico — e a suposição fica escrita.
    /// </summary>
    [Fact]
    public void A_letra_de_natureza_sem_significado_documentado_entra_como_Ativa_com_a_suposicao_registrada()
    {
        var correcoes = new List<CorrecaoAplicada>();

        SaneamentoDeProcesso.Natureza("M", "GABRIELA.MIELE", new HashSet<string>(), correcoes)
            .Should().Be(NaturezaDaInteracao.Ativa);

        correcoes.Should().ContainSingle().Which.Campo.Should().Be("natureza");
    }

    // =============================================================================================
    // Classe do cliente na carteira
    // =============================================================================================

    /// <summary>As quatro letras que são classe de verdade passam sem suposição.</summary>
    /// <param name="potencial">O texto de potencial da origem.</param>
    /// <param name="esperada">A classe esperada.</param>
    [Theory]
    [InlineData("A", ClasseDeCliente.A)]
    [InlineData("b", ClasseDeCliente.B)]
    [InlineData("C", ClasseDeCliente.C)]
    [InlineData("D", ClasseDeCliente.D)]
    public void As_quatro_classes_reais_passam_sem_suposicao(string potencial, ClasseDeCliente esperada)
    {
        var correcoes = new List<CorrecaoAplicada>();

        var (classe, veioDaOrigem) = SaneamentoDeProcesso.Classe(potencial, correcoes);

        classe.Should().Be(esperada);
        veioDaOrigem.Should().BeTrue();
        correcoes.Should().BeEmpty();
    }

    /// <summary>
    /// [V] 4.553 linhas da tabela de posse trazem <c>64</c>, <c>43</c>, <c>22</c> ou <c>85</c>
    /// onde deveria haver uma classe — resquício de outro domínio gravado no mesmo campo de texto
    /// livre. Segmentação por potencial feita hoje no legado devolve resultado errado sem avisar.
    /// </summary>
    /// <param name="potencial">O valor real medido na origem.</param>
    [Theory]
    [InlineData("64")]
    [InlineData("43")]
    [InlineData("22")]
    [InlineData("85")]
    [InlineData("Z")]
    [InlineData("")]
    public void O_potencial_que_nao_e_classe_entra_como_C_com_a_suposicao_registrada(string potencial)
    {
        var correcoes = new List<CorrecaoAplicada>();

        var (classe, veioDaOrigem) = SaneamentoDeProcesso.Classe(potencial, correcoes);

        classe.Should().Be(ClasseDeCliente.C);
        veioDaOrigem.Should().BeFalse();
        correcoes.Should().ContainSingle().Which.Campo.Should().Be("classeNaCarteira");
    }

    // =============================================================================================
    // Código estável de catálogo
    // =============================================================================================

    /// <summary>
    /// O código do catálogo é o contrato público (documento 16, seção 3) e não pode carregar
    /// acento, espaço nem pontuação — nem mudar quando alguém corrigir a grafia do nome na tela.
    /// </summary>
    /// <param name="texto">O nome como a origem o escreve.</param>
    /// <param name="esperado">O código estável.</param>
    [Theory]
    [InlineData("Venda Equipamento Tracbel Agro", "VENDA_EQUIPAMENTO_TRACBEL_AGRO")]
    [InlineData("Aferição Qualidade JDE", "AFERICAO_QUALIDADE_JDE")]
    [InlineData("* Não iniciado *", "NAO_INICIADO")]
    [InlineData("Venda Peças/Pneus", "VENDA_PECAS_PNEUS")]
    [InlineData("   ", "")]
    public void O_codigo_de_catalogo_nasce_sem_acento_sem_espaco_e_sem_pontuacao(
        string texto, string esperado) =>
        SaneamentoDeProcesso.Codificar(texto, 60).Should().Be(esperado);

    /// <summary>O código respeita o tamanho da coluna e nunca termina em separador.</summary>
    [Fact]
    public void O_codigo_respeita_o_tamanho_da_coluna_e_nao_termina_em_separador()
    {
        var codigo = SaneamentoDeProcesso.Codificar("Venda Equipamento Tracbel Agro", 18);

        codigo.Length.Should().BeLessThanOrEqualTo(18);
        codigo.Should().NotEndWith("_");
    }
}
