using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>
/// A ROTINA <c>PROCESSOS_VORTICE</c> NO CATÁLOGO (documento 52 §5). O que estes testes prendem: ela entra no FIM da
/// lista — a posição é o identificador semeado (<c>Id</c> = posição + 1), e inserir no meio renumeraria as rotinas que já
/// existem, com <c>ExecucaoDeRotina</c> apontando para elas —; é diária às 06:30, depois das carteiras e do parque; nasce
/// desligada; e exige a conexão do Vórtice.
/// </summary>
public sealed class RotinaDosProcessosDoVorticeTestes
{
    private static RotinaDoSistema Catalogo => RotinasDoSistema.Obter(RotinasDoSistema.ProcessosVortice)!;

    [Fact]
    public void Entra_no_fim_da_lista_e_as_que_ja_existiam_nao_mudam_de_identificador()
    {
        // Id = posição + 1, como a semente grava. As sete de antes ficam onde estavam; a do funil é a oitava (#247). A das
        // metas (METAS_GESTAO_NEGOCIOS, #248) entrou DEPOIS dela, no nono lugar, e a do preço da máquina (PRECOS_DE_MAQUINA,
        // issue 70) no décimo — nenhuma mudou de identificador.
        RotinasDoSistema.Todas.Select((r, posicao) => (Id: posicao + 1, r.Codigo)).Should().Equal(
            (1, RotinasDoSistema.FontesAnuais),
            (2, RotinasDoSistema.PrecosMensais),
            (3, RotinasDoSistema.Faturamento),
            (4, RotinasDoSistema.ArtVendas),
            (5, RotinasDoSistema.CadastroDeClientes),
            (6, RotinasDoSistema.CarteirasVortice),
            (7, RotinasDoSistema.ParqueProtheus),
            (8, RotinasDoSistema.ProcessosVortice),
            (9, RotinasDoSistema.MetasGestaoDeNegocios),
            (10, RotinasDoSistema.PrecosDeMaquina));
    }

    [Fact]
    public void E_diaria_as_06_30_nasce_desligada_e_exige_o_Vortice()
    {
        Catalogo.AgendaPadrao.Should().Be(AgendaDaRotina.DiariaAs(new TimeOnly(6, 30)));
        Catalogo.LigadaPorPadrao.Should().BeFalse("ligar é trazer dado novo para produção — decisão de quem administra, depois da simulação");
        Catalogo.ConexaoExigida.Should().Be(ConexoesDoSistema.Vortice);
        Catalogo.Conexoes.Should().Equal(ConexoesDoSistema.Vortice);
        Catalogo.Modos.Should().Equal("--somente-processos-vortice");

        var carteiras = RotinasDoSistema.Obter(RotinasDoSistema.CarteirasVortice)!.AgendaPadrao.Hora;
        Catalogo.AgendaPadrao.Hora.Should().BeAfter(carteiras!.Value, "o funil liga o processo à carteira pelo de-para que a sincronia das carteiras grava");
    }

    [Fact]
    public void Desligada_nao_roda_pela_agenda_e_ligada_roda_as_06_30_de_Sao_Paulo()
    {
        var rotina = Rotina.DoCatalogo(Catalogo, RotinasDoSistema.AgendaSemeadaDesde);
        var dia = new DateTime(2026, 10, 2, 9, 31, 0, DateTimeKind.Utc);

        rotina.EstaVencida(dia).Should().BeFalse("nasce desligada");

        rotina.Ligar(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        rotina.EstaVencida(new DateTime(2026, 10, 2, 9, 29, 0, DateTimeKind.Utc)).Should().BeFalse("06:29 em São Paulo");
        rotina.EstaVencida(dia).Should().BeTrue("06:30 em São Paulo é 09:30 em UTC");
    }

    [Fact]
    public void A_conexao_do_Vortice_diz_que_serve_ao_funil()
    {
        ConexoesDoSistema.Todas.Select((c, posicao) => (Id: posicao + 1, c.Codigo)).Should().Contain((4, ConexoesDoSistema.Vortice));
        ConexoesDoSistema.Todas.Single(c => c.Codigo == ConexoesDoSistema.Vortice).Descricao
            .Should().Contain("funil").And.Contain("vendas perdidas").And.Contain("somente leitura");
    }
}
