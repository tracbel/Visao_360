using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class JustificativaSemSimboloDeSecao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 2,
                column: "Justificativa",
                value: "D-P05 decidida em 23/09/2026 (documento 48, seção 5.1): pesos medidos no protótipo, a confirmar — 0,40 no preço e na rentabilidade, 0,50 no crédito, fator entre 0,40 e 1,50. A percepção pesa 1,00, e não os 0,40 do protótipo: a D-P04 trocou a escala de −2 a +2 para ±5 pontos percentuais, e 1,00 torna esse rótulo literal. O termo de troca fica fora até a issue 70.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 2,
                column: "Justificativa",
                value: "D-P05 decidida em 23/09/2026 (documento 48, §5.1): pesos medidos no protótipo, a confirmar — 0,40 no preço e na rentabilidade, 0,50 no crédito, fator entre 0,40 e 1,50. A percepção pesa 1,00, e não os 0,40 do protótipo: a D-P04 trocou a escala de −2 a +2 para ±5 pontos percentuais, e 1,00 torna esse rótulo literal. O termo de troca fica fora até a issue 70.");
        }
    }
}
