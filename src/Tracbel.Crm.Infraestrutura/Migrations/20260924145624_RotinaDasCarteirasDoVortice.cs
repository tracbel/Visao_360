using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class RotinaDasCarteirasDoVortice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "integracao",
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "A busca ao vivo no legado, congelado desde a fase 1, e a sincronia diária das carteiras MAQ_NOVOS. Sessão somente leitura.");

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 6, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Diaria", "CARTEIRAS_VORTICE", null, false, null, null, new TimeOnly(4, 30, 0), null, null, "Carteiras MAQ_NOVOS do Vórtice", null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                schema: "integracao",
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "A busca ao vivo no legado, congelado desde a fase 1 (somente referência).");
        }
    }
}
