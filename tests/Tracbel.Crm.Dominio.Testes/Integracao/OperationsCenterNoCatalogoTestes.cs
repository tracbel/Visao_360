using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>
/// O OPERATIONS CENTER NO CATÁLOGO DAS INTEGRAÇÕES (28/09/2026): a conexão 14 e a rotina 11, no FIM das listas — a posição
/// é o identificador semeado —, reservadas com a outra sessão no quadro. A rotina roda depois da carga do BI (06:55),
/// nasce desligada e exige a conexão.
/// </summary>
public sealed class OperationsCenterNoCatalogoTestes
{
    [Fact]
    public void A_conexao_e_a_14a_e_ultima_da_lista_e_e_um_banco_MySql()
    {
        var todas = ConexoesDoSistema.Todas;

        todas.Count.Should().Be(14);
        todas[^1].Codigo.Should().Be(ConexoesDoSistema.OperationsCenter, "conexão nova entra no fim: a posição é o identificador");
        todas[^1].Tipo.Should().Be(TipoDeConexao.MySql);
        todas[^1].Descricao.Should().Contain("somente leitura");
    }

    [Fact]
    public void A_rotina_e_a_11a_diaria_as_07_30_nasce_desligada_e_exige_o_operations_center()
    {
        var todas = RotinasDoSistema.Todas;
        var telemetria = todas[10];

        telemetria.Codigo.Should().Be(RotinasDoSistema.TelemetriaOperationsCenter);
        todas.Should().HaveCount(11, "é a última");
        telemetria.Modos.Should().Equal("--somente-operations-center");
        telemetria.LigadaPorPadrao.Should().BeFalse("trazer dado novo para produção é decisão de quem administra");
        telemetria.ConexaoExigida.Should().Be(ConexoesDoSistema.OperationsCenter);
        telemetria.Conexoes.Should().Equal(ConexoesDoSistema.OperationsCenter);
        telemetria.AgendaPadrao.Should().Be(AgendaDaRotina.DiariaAs(new TimeOnly(7, 30)));

        // DEPOIS DO PARQUE DO PROTHEUS, que traz as máquinas do dia: sem máquina no CRM, a telemetria não tem onde gravar.
        var parque = RotinasDoSistema.Obter(RotinasDoSistema.ParqueProtheus)!.AgendaPadrao.Hora!.Value;
        telemetria.AgendaPadrao.Hora.Should().BeAfter(parque);
    }
}
