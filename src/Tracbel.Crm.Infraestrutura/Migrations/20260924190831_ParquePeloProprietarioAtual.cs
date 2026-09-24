using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class ParquePeloProprietarioAtual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VinculoDeClienteComEquipamento_Natureza",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CompradorPendente_Situacao",
                schema: "integracao",
                table: "CompradorPendente");

            migrationBuilder.AddColumn<string>(
                name: "Evidencia",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CompradorPeloDonoNoProtheus",
                schema: "frota",
                table: "VendaDeMaquina",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "integracao",
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 2,
                column: "Descricao",
                value: "O cadastro de clientes, o faturamento, o parque de máquinas pelo dono atual e a conferência do ART. Sessão somente leitura.");

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 7, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Diaria", "PARQUE_PROTHEUS", null, false, null, null, new TimeOnly(5, 30, 0), null, null, "Parque de máquinas (Protheus)", null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "UX_VinculoDeClienteComEquipamento_ProprietarioAtual",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                column: "EquipamentoId",
                unique: true,
                filter: "[Natureza] = 'ProprietarioAtual' AND [EncerradoEm] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VinculoDeClienteComEquipamento_Evidencia",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                sql: "([Natureza] = 'CompradorNaVenda' AND [Evidencia] IS NULL) OR ([Natureza] = 'ProprietarioAtual' AND [VendaDeMaquinaId] IS NULL AND [Evidencia] IN ('NotaDeVenda','OrdemDeServico','CadastroAntigo','VendaNoArt'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VinculoDeClienteComEquipamento_Natureza",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                sql: "[Natureza] IN ('CompradorNaVenda','ProprietarioAtual')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento",
                sql: "[ModeloId] IS NOT NULL OR [Origem] IN ('Art','Protheus')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompradorPendente_Situacao",
                schema: "integracao",
                table: "CompradorPendente",
                sql: "[Situacao] IN ('AguardandoCadastro','Cadastrado','ResolvidoPeloDonoNoProtheus')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_VinculoDeClienteComEquipamento_ProprietarioAtual",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VinculoDeClienteComEquipamento_Evidencia",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VinculoDeClienteComEquipamento_Natureza",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CompradorPendente_Situacao",
                schema: "integracao",
                table: "CompradorPendente");

            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "Evidencia",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento");

            migrationBuilder.DropColumn(
                name: "CompradorPeloDonoNoProtheus",
                schema: "frota",
                table: "VendaDeMaquina");

            migrationBuilder.UpdateData(
                schema: "integracao",
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 2,
                column: "Descricao",
                value: "O dono do chassi e o cadastro do comprador, para a integração do ART. Sessão somente leitura.");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VinculoDeClienteComEquipamento_Natureza",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                sql: "[Natureza] IN ('CompradorNaVenda')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento",
                sql: "[ModeloId] IS NOT NULL OR [Origem] = 'Art'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompradorPendente_Situacao",
                schema: "integracao",
                table: "CompradorPendente",
                sql: "[Situacao] IN ('AguardandoCadastro','Cadastrado')");
        }
    }
}
