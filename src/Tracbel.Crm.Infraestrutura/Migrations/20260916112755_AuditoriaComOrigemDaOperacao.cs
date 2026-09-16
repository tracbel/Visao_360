using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// A TRILHA GANHA ORIGEM, SISTEMA E OPERAÇÃO (documento 41, fase 2).
    ///
    /// <para><b>Escrita à mão em dois pontos</b>, e nenhum é gosto:</para>
    /// <list type="number">
    ///   <item><b>As colunas obrigatórias nascem anuláveis, recebem valor e só então ficam
    ///   obrigatórias.</b> O gerador faria <c>NOT NULL DEFAULT ''</c>, e o texto vazio viola a
    ///   restrição de domínio assim que houver uma linha na trilha — o banco do servidor tem as da
    ///   carga do ART. As linhas que já existem vieram TODAS das cargas, que gravavam a trilha à mão
    ///   e só registravam alteração do que já existia: por isso <c>Integracao</c> e
    ///   <c>Alteracao</c>. O <c>SistemaId</c> delas fica nulo — adivinhar o sistema pelo nome da
    ///   entidade seria inventar dado na trilha que existe para não ter dado inventado.</item>
    ///   <item><b>O índice de <c>SistemaId</c> nasce sobre o esquema de partição.</b> A tabela é
    ///   particionada por mês; um índice criado no filegroup padrão fica desalinhado e impede
    ///   <c>SWITCH</c> e <c>TRUNCATE</c> por partição, que são o motivo de particionar
    ///   (<c>ModeloInicial.Particionamento.cs</c>).</item>
    /// </list>
    ///
    /// <para><b>Retenção:</b> continua a declarada na migração inicial (18 meses disponíveis). O prazo
    /// definitivo é a decisão D-10, pendente com o jurídico.</para>
    /// </summary>
    public partial class AuditoriaComOrigemDaOperacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Operacao",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origem",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                type: "varchar(12)",
                unicode: false,
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SistemaId",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                type: "int",
                nullable: true);

            // EXEC, e não UPDATE direto: as colunas acabaram de ser criadas neste mesmo lote, e o SQL
            // Server resolve os nomes do lote antes de executar.
            migrationBuilder.Sql(
                "EXEC(N'UPDATE [auditoria].[AlteracaoDeCampo] SET [Origem] = ''Integracao'', [Operacao] = ''Alteracao'' " +
                "WHERE [Origem] IS NULL OR [Operacao] IS NULL');");

            migrationBuilder.AlterColumn<string>(
                name: "Operacao",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Origem",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                type: "varchar(12)",
                unicode: false,
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldUnicode: false,
                oldMaxLength: 12,
                oldNullable: true);

            // O índice alinhado à partição. Fora do SQL Server não há partição, e o índice é o comum.
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = N'PS_Mensal_AlteracaoDeCampo')
    EXEC(N'CREATE NONCLUSTERED INDEX [IX_AlteracaoDeCampo_SistemaId] ON [auditoria].[AlteracaoDeCampo] ([SistemaId] ASC) ON [PS_Mensal_AlteracaoDeCampo]([AlteradoEm])');
ELSE
    EXEC(N'CREATE NONCLUSTERED INDEX [IX_AlteracaoDeCampo_SistemaId] ON [auditoria].[AlteracaoDeCampo] ([SistemaId] ASC)');");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Operacao",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Operacao] IN ('Inclusao','Alteracao','Exclusao')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Origem",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Origem] IN ('Usuario','Integracao','Importacao','Sistema','Job')");

            migrationBuilder.AddForeignKey(
                name: "FK_AlteracaoDeCampo_Sistema_SistemaId",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                column: "SistemaId",
                principalSchema: "integracao",
                principalTable: "Sistema",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlteracaoDeCampo_Sistema_SistemaId",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropIndex(
                name: "IX_AlteracaoDeCampo_SistemaId",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Operacao",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Origem",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "Operacao",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "Origem",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "SistemaId",
                schema: "auditoria",
                table: "AlteracaoDeCampo");
        }
    }
}
