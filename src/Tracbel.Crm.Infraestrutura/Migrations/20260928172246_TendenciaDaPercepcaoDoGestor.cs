using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class TendenciaDaPercepcaoDoGestor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TendenciaParaTresMeses",
                schema: "organizacao",
                table: "PercepcaoDoGestor",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PercepcaoDoGestor_Tendencia",
                schema: "organizacao",
                table: "PercepcaoDoGestor",
                sql: "[TendenciaParaTresMeses] IS NULL OR [TendenciaParaTresMeses] IN ('Queda','Estavel','Alta')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PercepcaoDoGestor_Tendencia",
                schema: "organizacao",
                table: "PercepcaoDoGestor");

            migrationBuilder.DropColumn(
                name: "TendenciaParaTresMeses",
                schema: "organizacao",
                table: "PercepcaoDoGestor");
        }
    }
}
