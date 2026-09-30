using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class RotinaDaParticaoDaAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 14, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Mensal", "PARTICAO_AUDITORIA", 1, true, null, null, new TimeOnly(2, 0, 0), null, null, "Partição da auditoria", null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // AS EXECUÇÕES DA ROTINA 14 SAEM ANTES DELA, como nas outras migrações de rotina: apontam para ela por chave
            // estrangeira. O que ela fez no banco (os meses abertos, as partições esvaziadas) não se desfaz — e não precisa:
            // um limite a mais numa função de partição não muda linha nenhuma.
            migrationBuilder.Sql("DELETE FROM [integracao].[ExecucaoDeRotina] WHERE [RotinaId] = 14;");

            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 14);
        }
    }
}
