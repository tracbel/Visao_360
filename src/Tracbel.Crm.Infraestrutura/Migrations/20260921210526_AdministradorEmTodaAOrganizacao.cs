using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AdministradorEmTodaAOrganizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "Todas as permissões, em todas as filiais: tudo o que o padrão dá, mais excluir, a visão de todas as filiais, a administração de perfis e usuários e os parâmetros do potencial.");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 401,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 402,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 403,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 404,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 405,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 406,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 407,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 408,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 409,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 410,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 411,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 412,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 413,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 414,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 415,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 416,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 417,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 418,
                column: "Profundidade",
                value: "Organizacao");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 422,
                column: "Profundidade",
                value: "Organizacao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "Tudo o que o padrão dá, mais excluir, a visão entre filiais, a administração de perfis e usuários e os parâmetros do potencial.");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 401,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 402,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 403,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 404,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 405,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 406,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 407,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 408,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 409,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 410,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 411,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 412,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 413,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 414,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 415,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 416,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 417,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 418,
                column: "Profundidade",
                value: "EmpresaEAbaixo");

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 422,
                column: "Profundidade",
                value: "EmpresaEAbaixo");
        }
    }
}
