using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaLerNaDiretoriaENoAdministrador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 7,
                column: "Descricao",
                value: "O que a gerência tem, mais a visão de todas as filiais de uma vez e a trilha de auditoria.");

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[,]
                {
                    { 426, "Auditoria.Ler", 4, "Organizacao" },
                    { 705, "Auditoria.Ler", 7, "Organizacao" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 426);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 705);

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 7,
                column: "Descricao",
                value: "O que a gerência tem, mais a visão de todas as filiais de uma vez.");
        }
    }
}
