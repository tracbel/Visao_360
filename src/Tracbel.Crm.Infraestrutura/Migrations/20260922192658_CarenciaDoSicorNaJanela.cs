using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class CarenciaDoSicorNaJanela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "MesesDeCarenciaDoSicor",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                type: "smallint",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 1,
                column: "MesesDeCarenciaDoSicor",
                value: null);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParametroDoPotencial_CarenciaDoSicor",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                sql: "[MesesDeCarenciaDoSicor] IS NULL OR ([MesesDeCarenciaDoSicor] >= 0 AND [MesesDeCarenciaDoSicor] < [MesesDaJanela])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ParametroDoPotencial_CarenciaDoSicor",
                schema: "organizacao",
                table: "ParametroDoPotencial");

            migrationBuilder.DropColumn(
                name: "MesesDeCarenciaDoSicor",
                schema: "organizacao",
                table: "ParametroDoPotencial");
        }
    }
}
