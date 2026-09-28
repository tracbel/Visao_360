using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class TelemetriaDoOperationsCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MunicipioDaPosicaoId",
                schema: "frota",
                table: "Equipamento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PosicaoEm",
                schema: "frota",
                table: "Equipamento",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PosicaoLatitude",
                schema: "frota",
                table: "Equipamento",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PosicaoLongitude",
                schema: "frota",
                table: "Equipamento",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Conexao",
                columns: new[] { "Id", "Banco", "Codigo", "Descricao", "EhDoSistema", "Endereco", "EstaAtiva", "MinutosEntreVerificacoes", "Nome", "NomeDoCabecalho", "Objeto", "Porta", "SegredoAlteradoEm", "SegredoAlteradoPorId", "SegredoProtegido", "StatusEsperado", "Tipo", "UltimaVerificacao", "UltimaVerificacaoEm", "UltimaVerificacaoResumo", "Usuario" },
                values: new object[] { 14, null, "OPERATIONS_CENTER", "O horímetro e a última posição das máquinas John Deere conectadas, do banco do BI que espelha o Operations Center. Sessão somente leitura.", true, null, true, null, "Operations Center — telemetria John Deere", null, null, null, null, null, null, 200, "MySql", "NuncaVerificada", null, null, null });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 11, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Diaria", "TELEMETRIA_OPERATIONS_CENTER", null, false, null, null, new TimeOnly(7, 30, 0), null, null, "Telemetria das máquinas (Operations Center)", null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_MunicipioDaPosicaoId",
                schema: "frota",
                table: "Equipamento",
                column: "MunicipioDaPosicaoId",
                filter: "[MunicipioDaPosicaoId] IS NOT NULL AND [ExcluidoEm] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_Posicao",
                schema: "frota",
                table: "Equipamento",
                sql: "([PosicaoLatitude] IS NULL AND [PosicaoLongitude] IS NULL AND [PosicaoEm] IS NULL AND [MunicipioDaPosicaoId] IS NULL) OR ([PosicaoLatitude] BETWEEN -90 AND 90 AND [PosicaoLongitude] BETWEEN -180 AND 180 AND [PosicaoEm] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipamento_Municipio_MunicipioDaPosicaoId",
                schema: "frota",
                table: "Equipamento",
                column: "MunicipioDaPosicaoId",
                principalSchema: "organizacao",
                principalTable: "Municipio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // O QUE APONTA PARA A ROTINA 11 E PARA A CONEXÃO 14 SAI ANTES DELAS, como na do preço da máquina: a execução e o
            // teste têm chave estrangeira, e o DeleteData falharia com a rotina já rodada ou a conexão já testada.
            migrationBuilder.Sql("DELETE FROM [integracao].[ExecucaoDeRotina] WHERE [RotinaId] = 11;");
            migrationBuilder.Sql("DELETE FROM [integracao].[VerificacaoDeConexao] WHERE [ConexaoId] = 14;");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipamento_Municipio_MunicipioDaPosicaoId",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropIndex(
                name: "IX_Equipamento_MunicipioDaPosicaoId",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_Posicao",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "MunicipioDaPosicaoId",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropColumn(
                name: "PosicaoEm",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropColumn(
                name: "PosicaoLatitude",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropColumn(
                name: "PosicaoLongitude",
                schema: "frota",
                table: "Equipamento");
        }
    }
}
