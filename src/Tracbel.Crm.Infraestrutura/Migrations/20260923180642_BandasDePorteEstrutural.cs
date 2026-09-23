using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class BandasDePorteEstrutural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PorteGrandeAPartirDe",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                type: "decimal(12,1)",
                precision: 12,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorteMedioAPartirDe",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                type: "decimal(12,1)",
                precision: 12,
                scale: 1,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PorteGrandeAPartirDe", "PorteMedioAPartirDe" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PorteGrandeAPartirDe", "PorteMedioAPartirDe" },
                values: new object[] { null, null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParametroDoPotencial_BandasDePorte",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                sql: "([PorteMedioAPartirDe] IS NULL AND [PorteGrandeAPartirDe] IS NULL) OR ([PorteMedioAPartirDe] IS NOT NULL AND [PorteGrandeAPartirDe] IS NOT NULL AND [PorteMedioAPartirDe] > 0 AND [PorteMedioAPartirDe] < [PorteGrandeAPartirDe])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ParametroDoPotencial_BandasDePorte",
                schema: "organizacao",
                table: "ParametroDoPotencial");

            migrationBuilder.DropColumn(
                name: "PorteGrandeAPartirDe",
                schema: "organizacao",
                table: "ParametroDoPotencial");

            migrationBuilder.DropColumn(
                name: "PorteMedioAPartirDe",
                schema: "organizacao",
                table: "ParametroDoPotencial");
        }
    }
}
