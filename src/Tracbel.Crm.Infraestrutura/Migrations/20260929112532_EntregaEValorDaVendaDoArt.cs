using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class EntregaEValorDaVendaDoArt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EntregueEm",
                schema: "integracao",
                table: "RegistroDeOrigem",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDaVenda",
                schema: "integracao",
                table: "RegistroDeOrigem",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            // A ENTREGA DAS VENDAS QUE O CRM JÁ TEM vem da própria venda, já na migração: sem isto, o faturamento do ano
            // fica sem máquina nenhuma até a primeira leitura do ART depois da publicação. O VALOR não tem de onde vir
            // aqui — ele chega com essa leitura, que acha todos os registros alterados (o valor entrou no resumo). Até lá,
            // as máquinas aparecem "sem valor", ditas. Idempotente: só preenche o que está vazio.
            migrationBuilder.Sql(
                """
                UPDATE r
                   SET r.EntregueEm = v.EntregueEm
                  FROM integracao.RegistroDeOrigem r
                  JOIN frota.VendaDeMaquina v ON v.Id = r.VendaDeMaquinaId
                 WHERE r.EntregueEm IS NULL
                   AND v.EntregueEm IS NOT NULL
                   AND v.ExcluidoEm IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntregueEm",
                schema: "integracao",
                table: "RegistroDeOrigem");

            migrationBuilder.DropColumn(
                name: "ValorDaVenda",
                schema: "integracao",
                table: "RegistroDeOrigem");
        }
    }
}
