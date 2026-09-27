using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class FormularioDaVendaPerdida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormularioDeOrigem",
                schema: "processo",
                table: "VendaPerdida",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NumeroDoProcessoNaOrigem",
                schema: "processo",
                table: "VendaPerdida",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Papel",
                schema: "processo",
                table: "VendaPerdida",
                type: "varchar(12)",
                unicode: false,
                maxLength: 12,
                nullable: false,
                defaultValue: "Principal");

            migrationBuilder.AddColumn<long>(
                name: "VendaPerdidaPrincipalId",
                schema: "processo",
                table: "VendaPerdida",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendaPerdida_NumeroDoProcessoNaOrigem",
                schema: "processo",
                table: "VendaPerdida",
                column: "NumeroDoProcessoNaOrigem");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPerdida_VendaPerdidaPrincipalId",
                schema: "processo",
                table: "VendaPerdida",
                column: "VendaPerdidaPrincipalId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VendaPerdida_Formulario",
                schema: "processo",
                table: "VendaPerdida",
                sql: "[FormularioDeOrigem] IS NULL OR [FormularioDeOrigem] IN ('IV_Q_VENDA_PERDIDA','IV_Q_VENDA_PERDIDA_FY25','IV_Q_VENDA_PERDIDA_MAQIMP','IV_Q_VP_SEM_PARTICIPACAO','IV_Q_VENDA_PERDIDA_JDE','IV_Q_VENDA_PERDIDA_PROD','IV_Q_VENDA_PERDIDA_IMPLEM','IV_Q_VENDA_PERDIDA_IMPL','IV_Q_VP_TRATOR','IV_Q_VP_COLHEITADEIRA','IV_Q_VP_PLANTADEIRA','IV_Q_VP_COLHEDORA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VendaPerdida_Papel",
                schema: "processo",
                table: "VendaPerdida",
                sql: "[Papel] IN ('Principal','Complemento','Duplicata')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VendaPerdida_PapelEPrincipal",
                schema: "processo",
                table: "VendaPerdida",
                sql: "([Papel] = 'Principal' AND [VendaPerdidaPrincipalId] IS NULL) OR ([Papel] <> 'Principal' AND [VendaPerdidaPrincipalId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_VendaPerdida_VendaPerdida_VendaPerdidaPrincipalId",
                schema: "processo",
                table: "VendaPerdida",
                column: "VendaPerdidaPrincipalId",
                principalSchema: "processo",
                principalTable: "VendaPerdida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendaPerdida_VendaPerdida_VendaPerdidaPrincipalId",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropIndex(
                name: "IX_VendaPerdida_NumeroDoProcessoNaOrigem",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropIndex(
                name: "IX_VendaPerdida_VendaPerdidaPrincipalId",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VendaPerdida_Formulario",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VendaPerdida_Papel",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VendaPerdida_PapelEPrincipal",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropColumn(
                name: "FormularioDeOrigem",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropColumn(
                name: "NumeroDoProcessoNaOrigem",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropColumn(
                name: "Papel",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.DropColumn(
                name: "VendaPerdidaPrincipalId",
                schema: "processo",
                table: "VendaPerdida");
        }
    }
}
