using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// A CLASSIFICAÇÃO DO FUNIL E O ÚLTIMO CONTATO DAS CARTEIRAS LEEM A MESMA LISTA (decisão de 27/09/2026, documento 52
/// §2.2). A <c>CARTEIRAS_VORTICE</c> calcula o último contato com a constante
/// <see cref="LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato"/> (PR #244); a tabela
/// <c>integracao.ClassificacaoDeResultadoDoVortice</c> nasce com <c>ContaComoContato</c> semeado. Se um código entrar
/// numa e não na outra, a carteira e o funil passam a dizer coisas diferentes sobre o mesmo cliente — este teste recusa
/// antes.
/// </summary>
public sealed class ClassificacaoEUltimoContatoTestes
{
    [Fact]
    public void A_semente_da_classificacao_e_a_constante_do_ultimo_contato_sao_iguais_codigo_a_codigo()
    {
        var daSemente = ClassificacaoDeResultadoDoVortice.Semente.Where(i => i.ContaComoContato).Select(i => i.Codigo).ToList();

        daSemente.Should().BeEquivalentTo(LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato,
            "a tabela de classificação e a constante do último contato são a mesma lista da BI_CARTEIRA_VN");
        ClassificacaoDeResultadoDoVortice.ResultadosQueContamComoContato.Should().Equal(
            LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato, "na mesma ordem da view, as duas");
        daSemente.Should().HaveCount(53);
    }
}
