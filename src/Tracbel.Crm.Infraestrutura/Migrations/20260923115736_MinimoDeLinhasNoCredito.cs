using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class MinimoDeLinhasNoCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimoDeLinhasNoCredito",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 1,
                column: "MinimoDeLinhasNoCredito",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimoDeLinhasNoCredito",
                schema: "organizacao",
                table: "ParametroDoPotencial");
        }
    }
}
