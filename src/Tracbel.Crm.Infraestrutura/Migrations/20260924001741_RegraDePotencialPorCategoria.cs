using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class RegraDePotencialPorCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RegraDePotencial_Produto_Vigencia",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.CreateIndex(
                name: "UX_RegraDePotencial_Produto_Categoria_Vigencia",
                schema: "organizacao",
                table: "RegraDePotencial",
                columns: new[] { "ProdutoCodigoIbge", "CategoriaDeMaquinaId", "VigenteDesde" },
                unique: true,
                filter: "[RevogadoEm] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RegraDePotencial_Produto_Categoria_Vigencia",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.CreateIndex(
                name: "UX_RegraDePotencial_Produto_Vigencia",
                schema: "organizacao",
                table: "RegraDePotencial",
                columns: new[] { "ProdutoCodigoIbge", "VigenteDesde" },
                unique: true,
                filter: "[RevogadoEm] IS NULL");
        }
    }
}
