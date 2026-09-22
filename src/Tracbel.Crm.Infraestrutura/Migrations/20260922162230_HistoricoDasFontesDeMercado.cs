using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class HistoricoDasFontesDeMercado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EncerradaEm",
                schema: "organizacao",
                table: "UsinaDeEtanol",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncerradaEm",
                schema: "organizacao",
                table: "UsinaDeEtanol");
        }
    }
}
