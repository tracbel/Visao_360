using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Comum;

/// <summary>
/// A CONFERÊNCIA DE VERSÃO — a camada de concorrência que protege a janela que o
/// <c>rowversion</c> não alcança.
///
/// <para>O <c>rowversion</c> do SQL Server protege a corrida entre duas requisições simultâneas.
/// Numa API sem estado, porém, a janela que dói é outra e é muito maior: o usuário abre a tela às
/// 9h, o colega salva às 9h05, e o primeiro salva às 9h10 por cima — sem que nenhuma corrida
/// tenha acontecido, porque as duas gravações foram sequenciais.</para>
///
/// <para>Só o cliente sabe qual versão ele leu. Por isso ele a devolve no <c>PUT</c>, e a
/// comparação acontece na entidade. Estes testes ficam no domínio, sem banco, porque a regra é do
/// domínio — e porque o provedor de teste da camada de aplicação (SQLite) não tem
/// <c>rowversion</c> e não conseguiria exercitá-la.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public class ConcorrenciaOtimistaTestes
{
    private static Cliente ClienteCom(byte[]? versao)
    {
        var cliente = Cliente.Criar(
            empresaId: 1,
            nomeRazao: "Fazenda da Concorrência Ltda",
            tipoDePessoa: TipoDePessoa.Juridica,
            proprietarioId: 10,
            criadoPorId: 10);

        // A propriedade tem `protected set` — quem a preenche é o banco. Aqui ela é posta por
        // reflexão porque o teste precisa simular o carimbo que o SQL Server geraria, e afrouxar
        // o encapsulamento da entidade só para testar seria trocar a garantia pela conveniência.
        typeof(EntidadeBase)
            .GetProperty(nameof(EntidadeBase.Versao))!
            .SetValue(cliente, versao);

        return cliente;
    }

    [Fact]
    public void A_versao_igual_a_do_registro_passa()
    {
        var cliente = ClienteCom([1, 2, 3, 4, 5, 6, 7, 8]);

        cliente.VersaoConfere([1, 2, 3, 4, 5, 6, 7, 8]).Should().BeTrue();
    }

    [Fact]
    public void A_versao_DIFERENTE_e_recusada_e_e_isso_que_impede_a_alteracao_cega()
    {
        // O carimbo mudou porque alguém gravou no meio do caminho. Sem esta recusa, a segunda
        // gravação sobrescreveria em silêncio o trabalho da primeira — que é exatamente o
        // desfecho que o documento 14, seção 5.1, existe para impedir.
        var cliente = ClienteCom([0, 0, 0, 0, 0, 0, 0, 9]);

        cliente.VersaoConfere([0, 0, 0, 0, 0, 0, 0, 8]).Should().BeFalse();
    }

    [Fact]
    public void Versao_de_tamanho_diferente_tambem_e_recusada()
    {
        var cliente = ClienteCom([1, 2, 3, 4, 5, 6, 7, 8]);

        cliente.VersaoConfere([1, 2, 3]).Should().BeFalse();
    }

    [Fact]
    public void Quem_NAO_manda_versao_dispensa_a_conferencia_e_assume_o_risco()
    {
        // A concessão é declarada, não acidental: um cliente de integração que não guarda versão
        // precisa continuar conseguindo gravar. O que ele perde é a proteção, e ele a perde
        // porque escolheu não mandar a versão.
        var cliente = ClienteCom([1, 2, 3, 4, 5, 6, 7, 8]);

        cliente.VersaoConfere(null).Should().BeTrue();
    }

    [Fact]
    public void Registro_sem_carimbo_no_banco_tambem_dispensa_a_conferencia()
    {
        // É o caso do provedor de teste (SQLite), que não tem rowversion: a propriedade sai do
        // modelo e chega nula. Comparar contra nulo recusaria toda alteração e quebraria os
        // testes de comportamento sem que houvesse concorrência nenhuma acontecendo.
        var cliente = ClienteCom(null);

        cliente.VersaoConfere([1, 2, 3, 4, 5, 6, 7, 8]).Should().BeTrue();
    }
}
