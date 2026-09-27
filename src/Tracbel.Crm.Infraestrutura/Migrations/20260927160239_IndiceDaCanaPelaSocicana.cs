using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class IndiceDaCanaPelaSocicana : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FonteDoIndice",
                schema: "organizacao",
                table: "Cultura",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NivelDoIndice",
                schema: "organizacao",
                table: "Cultura",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProdutoDoIndice",
                schema: "organizacao",
                table: "Cultura",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "Cultura",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FonteDoIndice", "NivelDoIndice", "ProdutoDoIndice" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "Cultura",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FonteDoIndice", "NivelDoIndice", "ProdutoDoIndice" },
                values: new object[] { "SOCICANA", "MENSAL", "ATR" });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "Cultura",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FonteDoIndice", "NivelDoIndice", "ProdutoDoIndice" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "Cultura",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FonteDoIndice", "NivelDoIndice", "ProdutoDoIndice" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "Cultura",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "FonteDoIndice", "NivelDoIndice", "ProdutoDoIndice" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "Cultura",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "FonteDoIndice", "NivelDoIndice", "ProdutoDoIndice" },
                values: new object[] { null, null, null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cultura_SerieDoIndice",
                schema: "organizacao",
                table: "Cultura",
                sql: "([FonteDoIndice] IS NULL AND [ProdutoDoIndice] IS NULL AND [NivelDoIndice] IS NULL) OR ([FonteDoIndice] IS NOT NULL AND [ProdutoDoIndice] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Cultura_SerieDoIndice",
                schema: "organizacao",
                table: "Cultura");

            migrationBuilder.DropColumn(
                name: "FonteDoIndice",
                schema: "organizacao",
                table: "Cultura");

            migrationBuilder.DropColumn(
                name: "NivelDoIndice",
                schema: "organizacao",
                table: "Cultura");

            migrationBuilder.DropColumn(
                name: "ProdutoDoIndice",
                schema: "organizacao",
                table: "Cultura");
        }
    }
}
