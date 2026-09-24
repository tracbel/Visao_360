using FluentAssertions;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// QUAL LOJA DA SA1 REPRESENTA O CLIENTE (carga de clientes, 24/09/2026).
///
/// <para>A SA1 tem uma linha por código + loja, e o mesmo CNPJ aparece em várias — medido: 38.744
/// lojas são 33.641 documentos. O CRM tem UM cliente por documento, e o endereço dele sai de uma
/// dessas lojas. Estes testes fixam qual, e por quê.</para>
///
/// <para><b>A propriedade que importa é a ESTABILIDADE.</b> Sem desempate determinístico, duas
/// cargas seguidas podem escolher lojas diferentes com a mesma completude e ficar trocando o
/// endereço do cliente de uma para outra — gerando trilha de auditoria a cada rodada sem que nada
/// tenha mudado na origem, e fazendo o cliente "mudar de lugar" no mapa sem motivo.</para>
/// </summary>
[Trait("Categoria", "Carga")]
public sealed class EscolhaDaLojaPrincipalTestes
{
    private static LojaDeClienteNoProtheus Loja(
        string loja,
        string codigo = "000001",
        bool bloqueada = false,
        string? logradouro = "Rua das Acácias, 100",
        string? cep = "14010000",
        string? inscricao = "123456789") =>
        new(
            Documento: "11222333000181",
            Codigo: codigo,
            Loja: loja,
            NomeRazao: "Fazenda de Exemplo Ltda",
            NomeFantasia: "Fazenda Exemplo",
            PessoaFisica: false,
            Uf: "SP",
            CodigoDoMunicipio: "43402",
            MunicipioNaOrigem: "RIBEIRAO PRETO",
            Logradouro: logradouro,
            Bairro: "Centro",
            Cep: cep,
            InscricaoEstadual: inscricao,
            Bloqueado: bloqueada);

    [Fact]
    public void A_loja_nao_bloqueada_vence_a_bloqueada_ainda_que_a_bloqueada_seja_mais_completa()
    {
        // O CADASTRO BLOQUEADO NÃO É O RETRATO ATUAL do cliente: ele foi bloqueado por algum
        // motivo, e o endereço vivo é o da loja que segue operando.
        var escolhida = CargaDeClientesDoProtheus.EscolherAPrincipal(
        [
            Loja("02", bloqueada: true),
            Loja("01", bloqueada: false, cep: null, inscricao: null)
        ]);

        escolhida.Loja.Should().Be("01");
    }

    [Fact]
    public void Entre_nao_bloqueadas_vence_a_que_tem_endereco()
    {
        // SEM LOGRADOURO NÃO NASCE ENDEREÇO, e o cliente entraria sem lugar no mapa.
        var escolhida = CargaDeClientesDoProtheus.EscolherAPrincipal(
        [
            Loja("01", logradouro: null),
            Loja("02", logradouro: "Estrada do Café, km 7")
        ]);

        escolhida.Loja.Should().Be("02");
    }

    [Fact]
    public void Com_a_mesma_completude_o_desempate_e_pelo_menor_codigo_e_depois_pela_menor_loja()
    {
        var escolhida = CargaDeClientesDoProtheus.EscolherAPrincipal(
        [
            Loja("03", codigo: "000009"),
            Loja("01", codigo: "000009"),
            Loja("02", codigo: "000002")
        ]);

        escolhida.Codigo.Should().Be("000002");
        escolhida.Loja.Should().Be("02");
    }

    /// <summary>
    /// A PROVA QUE JUSTIFICA O DESEMPATE: a ordem em que a origem devolve as lojas NÃO muda a
    /// escolha. Sem isso, a carga de amanhã pode trocar o endereço do cliente porque o Protheus
    /// devolveu as linhas noutra ordem — e ninguém conseguiria explicar a trilha.
    /// </summary>
    [Fact]
    public void A_escolha_nao_depende_da_ordem_em_que_a_origem_devolveu_as_lojas()
    {
        var lojas = new[]
        {
            Loja("03", codigo: "000007"),
            Loja("01", codigo: "000004", cep: null),
            Loja("02", codigo: "000004")
        };

        var direta = CargaDeClientesDoProtheus.EscolherAPrincipal(lojas);
        var invertida = CargaDeClientesDoProtheus.EscolherAPrincipal(lojas.Reverse());
        var embaralhada = CargaDeClientesDoProtheus.EscolherAPrincipal([lojas[1], lojas[2], lojas[0]]);

        invertida.Should().BeEquivalentTo(direta);
        embaralhada.Should().BeEquivalentTo(direta);
        direta.Codigo.Should().Be("000004");
        direta.Loja.Should().Be("02", "entre as duas do mesmo código, a que tem CEP é a mais completa");
    }

    [Fact]
    public void Uma_loja_so_e_a_escolhida_mesmo_bloqueada_e_incompleta()
    {
        // NÃO É CONTRADIÇÃO com o primeiro teste: aqui não há alternativa. Recusar o cliente porque
        // a única loja dele está bloqueada perderia um cliente que existe — e o bloqueio é do
        // cadastro na origem, não um veredito sobre o município em que ele está.
        var escolhida = CargaDeClientesDoProtheus.EscolherAPrincipal(
        [
            Loja("01", bloqueada: true, logradouro: null, cep: null, inscricao: null)
        ]);

        escolhida.Loja.Should().Be("01");
    }
}
