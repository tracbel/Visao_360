using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// A ROTINA DAS METAS E DO PLANEJAMENTO DA GN PASSA A LER DE HORA EM HORA (01/10/2026).
    ///
    /// <para><b>Por quê.</b> O gestor digita o forecast do mês novo no próprio dia 1º, e o best guess ao longo do mês. Em
    /// 01/10/2026 a leitura das 06:00 trouxe o forecast de 2 gestores; às 11:39 a API Gestão de Negócios já tinha o de 7 (37
    /// linhas), e a tela do Forecast da Gerência passou a manhã quase vazia.</para>
    ///
    /// <para><b>Por que SQL escrito à mão, e com condição.</b> O <c>UpdateData</c> que o EF gera troca a agenda de qualquer
    /// jeito; mas a agenda se muda pela tela (Configurações › Integrações), e quem a mudou decidiu. Então a troca só acontece
    /// se a rotina ainda estiver na agenda semeada — diária às 06:00. Ligada ou desligada fica como está, e a
    /// <c>AgendaVigenteDesde</c> não muda: ligada, a primeira leitura nova sai na volta seguinte do orquestrador.</para>
    /// </summary>
    public partial class RotinaDasMetasDeHoraEmHora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE integracao.Rotina
                SET Cadencia = 'Intervalo', Hora = NULL, IntervaloMinutos = 60
                WHERE Codigo = 'METAS_GESTAO_NEGOCIOS' AND Cadencia = 'Diaria' AND Hora = '06:00:00' AND IntervaloMinutos IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE integracao.Rotina
                SET Cadencia = 'Diaria', Hora = '06:00:00', IntervaloMinutos = NULL
                WHERE Codigo = 'METAS_GESTAO_NEGOCIOS' AND Cadencia = 'Intervalo' AND IntervaloMinutos = 60;
                """);
        }
    }
}
