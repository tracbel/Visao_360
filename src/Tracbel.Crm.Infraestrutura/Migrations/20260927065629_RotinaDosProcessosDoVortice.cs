using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class RotinaDosProcessosDoVortice : Migration
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
                value: "A busca ao vivo no legado, congelado desde a fase 1, a sincronia diária das carteiras MAQ_NOVOS e o funil e as vendas perdidas (histórico desde 2012). Sessão somente leitura.");

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 8, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Diaria", "PROCESSOS_VORTICE", null, false, null, null, new TimeOnly(6, 30, 0), null, null, "Funil e vendas perdidas do Vórtice", null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.UpdateData(
                schema: "integracao",
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "A busca ao vivo no legado, congelado desde a fase 1, e a sincronia diária das carteiras MAQ_NOVOS. Sessão somente leitura.");
        }
    }
}
