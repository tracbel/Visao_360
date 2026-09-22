using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class RevogacaoDeConcessaoEVerUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_UsuarioPerfil_Usuario_Perfil_Empresa",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.AddColumn<string>(
                name: "MotivoDaRevogacao",
                schema: "seguranca",
                table: "UsuarioPerfil",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevogadaEm",
                schema: "seguranca",
                table: "UsuarioPerfil",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RevogadaPorId",
                schema: "seguranca",
                table: "UsuarioPerfil",
                type: "bigint",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[,]
                {
                    { 425, "Usuario.Ler", 4, "Organizacao" },
                    { 603, "Usuario.Ler", 6, "EmpresaEAbaixo" },
                    { 704, "Usuario.Ler", 7, "Organizacao" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPerfil_RevogadaPorId",
                schema: "seguranca",
                table: "UsuarioPerfil",
                column: "RevogadaPorId");

            migrationBuilder.CreateIndex(
                name: "UX_UsuarioPerfil_Usuario_Perfil_Empresa",
                schema: "seguranca",
                table: "UsuarioPerfil",
                columns: new[] { "UsuarioId", "PerfilId", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL AND [RevogadaEm] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioPerfil_Usuario_RevogadaPorId",
                schema: "seguranca",
                table: "UsuarioPerfil",
                column: "RevogadaPorId",
                principalSchema: "seguranca",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioPerfil_Usuario_RevogadaPorId",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioPerfil_RevogadaPorId",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.DropIndex(
                name: "UX_UsuarioPerfil_Usuario_Perfil_Empresa",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 425);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 603);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 704);

            migrationBuilder.DropColumn(
                name: "MotivoDaRevogacao",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.DropColumn(
                name: "RevogadaEm",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.DropColumn(
                name: "RevogadaPorId",
                schema: "seguranca",
                table: "UsuarioPerfil");

            migrationBuilder.CreateIndex(
                name: "UX_UsuarioPerfil_Usuario_Perfil_Empresa",
                schema: "seguranca",
                table: "UsuarioPerfil",
                columns: new[] { "UsuarioId", "PerfilId", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL");
        }
    }
}
