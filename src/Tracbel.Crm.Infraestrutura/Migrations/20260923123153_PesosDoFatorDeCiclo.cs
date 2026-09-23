using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class PesosDoFatorDeCiclo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                columns: new[] { "Id", "FatorMaximo", "FatorMinimo", "InformadoEm", "InformadoPorId", "Justificativa", "LimiteDaPercepcao", "LimiteDeAquecimento", "LimiteDeRetracao", "LimiteDeSuperaquecimento", "MesesDaJanela", "MesesDeCarenciaDoSicor", "MinimoDeLinhasNoCredito", "MotivoDaRevogacao", "NomeDaFaixaIntermediaria", "PesoDoIndicadorComercial", "PesoDoIndicadorDeCredito", "PesoDoIndicadorDePreco", "PesoDosContratosNoCredito", "RevogadoEm", "RevogadoPorId", "VigenteDesde" },
                values: new object[] { 2, 1.50m, 0.40m, new DateTime(2026, 9, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "D-P05 decidida em 23/09/2026 (documento 48, §5.1): pesos MEDIDOS NO PROTÓTIPO, a confirmar — 0,40 no preço e na rentabilidade, 0,50 no crédito, fator entre 0,40 e 1,50. A percepção do gestor pesa 1,00, e não os 0,40 do protótipo: lá a escala ia de −2 a +2 e dava ±40% de efeito; a D-P04 trocou para ±5 pontos percentuais, e o peso 1,00 é o que torna esse rótulo literal. O termo de troca fica fora até a issue 70 trazer o preço de máquina.", 5.00m, 1.20m, 1.00m, 1.40m, (short)12, null, null, null, null, 1.00m, 0.50m, 0.40m, 0.70m, null, null, new DateOnly(2026, 9, 23) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
