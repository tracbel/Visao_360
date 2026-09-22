using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class QuantidadeDaPamSemUnidadeNoNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoEstado_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado");

            migrationBuilder.RenameColumn(
                name: "QuantidadeProduzidaToneladas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "QuantidadeProduzida");

            migrationBuilder.RenameColumn(
                name: "QuantidadeProduzidaToneladas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado",
                newName: "QuantidadeProduzida");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                sql: "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) AND ([QuantidadeProduzida] IS NULL OR [QuantidadeProduzida] >= 0) AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoEstado_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado",
                sql: "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) AND ([QuantidadeProduzida] IS NULL OR [QuantidadeProduzida] >= 0) AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoEstado_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado");

            migrationBuilder.RenameColumn(
                name: "QuantidadeProduzida",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "QuantidadeProduzidaToneladas");

            migrationBuilder.RenameColumn(
                name: "QuantidadeProduzida",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado",
                newName: "QuantidadeProduzidaToneladas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                sql: "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) AND ([QuantidadeProduzidaToneladas] IS NULL OR [QuantidadeProduzidaToneladas] >= 0) AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoEstado_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado",
                sql: "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) AND ([QuantidadeProduzidaToneladas] IS NULL OR [QuantidadeProduzidaToneladas] >= 0) AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)");
        }
    }
}
