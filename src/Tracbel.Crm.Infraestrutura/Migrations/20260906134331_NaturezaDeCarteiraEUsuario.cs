using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class NaturezaDeCarteiraEUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Natureza",
                schema: "seguranca",
                table: "Usuario",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                // O PADRÃO É "Pessoa", E NÃO STRING VAZIA. A coluna nasce em base com dado
                // dentro, e vazio não passa na restrição — nem significa coisa nenhuma. As
                // contas que não são gente são reclassificadas pela carga logo em seguida.
                defaultValue: "Pessoa");

            migrationBuilder.AddColumn<string>(
                name: "Natureza",
                schema: "organizacao",
                table: "Carteira",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                // Mesma razão: "Comercial" é a presunção, e a carga corrige o que não for.
                defaultValue: "Comercial");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Usuario_Natureza",
                schema: "seguranca",
                table: "Usuario",
                sql: "[Natureza] IN ('Pessoa','Departamento','Sistema','Fornecedor','Teste')");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_EmpresaId_Natureza",
                schema: "organizacao",
                table: "Carteira",
                columns: new[] { "EmpresaId", "Natureza" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Carteira_Natureza",
                schema: "organizacao",
                table: "Carteira",
                sql: "[Natureza] IN ('Comercial','Administrativa','Teste')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Usuario_Natureza",
                schema: "seguranca",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Carteira_EmpresaId_Natureza",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Carteira_Natureza",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropColumn(
                name: "Natureza",
                schema: "seguranca",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Natureza",
                schema: "organizacao",
                table: "Carteira");
        }
    }
}
