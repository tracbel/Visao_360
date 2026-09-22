using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// A USINA QUE SAI DA LISTA DA ANP FICA ENCERRADA, NÃO APAGADA (issue 153) — e as duas armadilhas que a regra evita:
/// a leitura vazia e a linha recusada.
/// </summary>
public sealed class UsinaDeEtanolTestes
{
    private static readonly DateTime Agora = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    private static UsinaDeEtanol Usina(string cnpj) =>
        UsinaDeEtanol.Registrar(cnpj, "USINA " + cnpj[..4], 10, new DateOnly(2026, 7, 1), 700, 1_300, 1, Agora.AddMonths(-1));

    [Fact]
    public void A_que_nao_veio_na_lista_fica_encerrada_e_a_que_veio_continua_vigente()
    {
        var fica = Usina("11111111000111");
        var sai = Usina("22222222000122");

        var encerradas = UsinaDeEtanol.EncerrarAsQueSairam([fica, sai], new HashSet<string> { fica.Cnpj }, 7, Agora);

        encerradas.Should().ContainSingle().Which.Should().BeSameAs(sai);
        sai.EncerradaEm.Should().Be(Agora);
        sai.EstaVigente.Should().BeFalse();
        fica.EstaVigente.Should().BeTrue();
    }

    [Fact]
    public void Lista_vazia_nao_encerra_nada()
    {
        // A ANP não fica sem nenhuma usina em SP de um mês para o outro: é arquivo que não veio inteiro.
        var usina = Usina("11111111000111");

        UsinaDeEtanol.EncerrarAsQueSairam([usina], new HashSet<string>(), 7, Agora).Should().BeEmpty();
        usina.EstaVigente.Should().BeTrue();
    }

    [Fact]
    public void Encerrar_de_novo_nao_muda_a_data()
    {
        var usina = Usina("11111111000111");
        usina.Encerrar(7, Agora).Should().BeTrue();

        UsinaDeEtanol.EncerrarAsQueSairam([usina], new HashSet<string> { "99999999000199" }, 7, Agora.AddMonths(1))
            .Should().BeEmpty("já estava encerrada");
        usina.EncerradaEm.Should().Be(Agora);
    }

    [Fact]
    public void A_que_volta_a_lista_reabre_e_conta_como_mudanca()
    {
        var usina = Usina("11111111000111");
        usina.Encerrar(7, Agora);

        usina.Reapurar(usina.RazaoSocial, usina.MunicipioId, usina.MesDeReferencia, 700, 1_300, 7, Agora.AddMonths(1))
            .Should().BeTrue("reabrir é mudança, mesmo com a capacidade igual");
        usina.EstaVigente.Should().BeTrue();
    }
}
