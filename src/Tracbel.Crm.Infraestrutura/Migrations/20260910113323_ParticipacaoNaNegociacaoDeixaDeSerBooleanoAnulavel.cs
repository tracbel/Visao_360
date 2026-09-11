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
            //
            // O UPDATE VAI DENTRO DE EXEC, e isso não é estilo. O SQL Server compila o lote INTEIRO
            // antes de executar a primeira linha, e no script idempotente o ALTER que cria a coluna e
            // este UPDATE caem no mesmo lote: na compilação a coluna ainda não existe, e o lote todo é
            // recusado com "Invalid column name 'Participacao'". Foi o que aconteceu na instalação do
            // servidor em 10/09/2026 — e não aconteceu aqui, porque aqui a coluna já existia. EXEC
            // adia a compilação para a hora da execução, quando o ALTER já rodou. O próprio EF faz o
            // mesmo com a restrição de verificação logo abaixo.
            migrationBuilder.Sql(@"
                EXEC(N'UPDATE processo.VendaPerdida
                       SET Participacao = CASE
                               WHEN ParticipamosDaNegociacao = 1 THEN ''Sim''
                               WHEN ParticipamosDaNegociacao = 0 THEN ''Nao''
                               ELSE ''NaoInformado''
                           END;');");

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

            // EXEC pela mesma razão da subida: a coluna recém-criada não existe na compilação do lote.
            migrationBuilder.Sql(@"
                EXEC(N'UPDATE processo.VendaPerdida
                       SET ParticipamosDaNegociacao = CASE
                               WHEN Participacao = ''Sim'' THEN 1
                               WHEN Participacao = ''Nao'' THEN 0
                               ELSE NULL
                           END;');");

            migrationBuilder.DropColumn(
                name: "Participacao",
                schema: "processo",
                table: "VendaPerdida");
        }
    }
}
