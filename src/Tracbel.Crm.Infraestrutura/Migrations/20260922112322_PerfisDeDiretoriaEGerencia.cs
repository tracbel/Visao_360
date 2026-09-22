using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class PerfisDeDiretoriaEGerencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "Perfil",
                columns: new[] { "Id", "Codigo", "Descricao", "EhPadrao", "EstaAtivo", "Nome" },
                values: new object[,]
                {
                    { 6, "GERENCIA", "Acrescenta informar a percepção do gestor por município e ver a situação das integrações e das fontes públicas.", false, true, "Gerência" },
                    { 7, "DIRETORIA", "O que a gerência tem, mais a visão de todas as filiais de uma vez.", false, true, "Diretoria" }
                });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[,]
                {
                    { 601, "PercepcaoDoGestor.Informar", 6, "Organizacao" },
                    { 602, "Integracao.Ler", 6, "EmpresaEAbaixo" },
                    { 701, "PercepcaoDoGestor.Informar", 7, "Organizacao" },
                    { 702, "Integracao.Ler", 7, "EmpresaEAbaixo" },
                    { 703, "Empresa.AlcanceEntreFiliais", 7, "Organizacao" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 601);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 602);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 701);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 702);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 703);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[] { 111, "Integracao.Ler", 1, "EmpresaEAbaixo" });
        }
    }
}
