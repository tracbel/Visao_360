using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class ModeloPendenteSoNaOrigemArt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento",
                sql: "[ModeloId] IS NOT NULL OR [Origem] = 'Art'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento",
                sql: "[ModeloId] IS NOT NULL OR [Origem] <> 'Crm'");
        }
    }
}
