using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class ParticipacaoNaNegociacaoDeixaDeSerBooleanoAnulavel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A COLUNA NOVA ENTRA ANTES, O DADO É TRADUZIDO, E SÓ ENTÃO A ANTIGA SAI.
            //
            // A migração gerada pelo EF apagava a coluna primeiro e criava a nova depois — o que
            // teria descartado a resposta de todas as vendas perdidas já carregadas. O aviso
            // "may result in the loss of data" era literal.
            migrationBuilder.AddColumn<string>(
                name: "Participacao",
                schema: "processo",
                table: "VendaPerdida",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "NaoInformado");

            // NULO VIRA "NaoInformado", E NÃO "Nao". Era exatamente essa confusão que motivou a
            // mudança: quem não respondeu não afirmou que ficamos de fora.
            migrationBuilder.Sql(@"
                UPDATE processo.VendaPerdida
                SET Participacao = CASE
                        WHEN ParticipamosDaNegociacao = 1 THEN 'Sim'
                        WHEN ParticipamosDaNegociacao = 0 THEN 'Nao'
                        ELSE 'NaoInformado'
                    END;");

            migrationBuilder.DropColumn(
                name: "ParticipamosDaNegociacao",
                schema: "processo",
                table: "VendaPerdida");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VendaPerdida_Participacao",
                schema: "processo",
                table: "VendaPerdida",
                sql: "[Participacao] IN ('NaoInformado','Sim','Nao')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VendaPerdida_Participacao",
                schema: "processo",
                table: "VendaPerdida");

            // A VOLTA TAMBÉM PRESERVA, e na ordem certa: a coluna antiga volta a existir, o dado é
            // recomposto, e só então o texto sai. "NaoInformado" volta a ser NULL, que é o que o
            // modelo antigo usava para a terceira resposta.
            migrationBuilder.AddColumn<bool>(
                name: "ParticipamosDaNegociacao",
                schema: "processo",
                table: "VendaPerdida",
                type: "bit",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE processo.VendaPerdida
                SET ParticipamosDaNegociacao = CASE
                        WHEN Participacao = 'Sim' THEN 1
                        WHEN Participacao = 'Nao' THEN 0
                        ELSE NULL
                    END;");

            migrationBuilder.DropColumn(
                name: "Participacao",
                schema: "processo",
                table: "VendaPerdida");
        }
    }
}
