using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// A RETENÇÃO DA TRILHA, DECIDIDA (D-10, 20/09/2026: 18 meses), escrita no próprio catálogo do banco (issue 40).
    ///
    /// <para>A <c>MS_Description</c> de <c>auditoria.AlteracaoDeCampo</c> dizia "18 meses disponíveis, arquivamento
    /// depois" desde a migração inicial — a quantidade de partições que se pretendia manter, antes de haver decisão. A
    /// decisão veio (documento 45 §5.3) e passa a ser o que o comentário diz, com a data e o jeito de expurgar: por
    /// partição, e não por <c>DELETE</c>.</para>
    ///
    /// <para><b>Não muda o modelo nem dado nenhum</b> — só a propriedade estendida. O <c>Down</c> devolve o texto antigo.
    /// Quem abre a tabela pelo SSMS lê a política sem procurar documento.</para>
    /// </summary>
    public partial class RetencaoDaAuditoriaDecidida : Migration
    {
        private const string Decidida =
            "Particionada por mes em AlteradoEm. Retencao DECIDIDA (D-10, 20/09/2026): 18 meses. " +
            "O que passar disso sai por particao (TRUNCATE ... WITH (PARTITIONS) ou SWITCH), nunca por DELETE.";

        private const string Anterior =
            "Particionada por mes em AlteradoEm. Retencao: 18 meses disponiveis, arquivamento depois.";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(Gravar(Decidida));

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(Gravar(Anterior));

        /// <summary>
        /// Atualiza a descrição quando ela existe e cria quando não existe — a fase 1 recriou o particionamento, e a
        /// migração não supõe qual dos dois caminhos o banco percorreu.
        /// </summary>
        private static string Gravar(string texto) => $@"
IF EXISTS (SELECT 1 FROM sys.fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'auditoria', N'TABLE', N'AlteracaoDeCampo', NULL, NULL))
    EXEC sys.sp_updateextendedproperty @name = N'MS_Description', @value = N'{texto}',
        @level0type = N'SCHEMA', @level0name = N'auditoria', @level1type = N'TABLE', @level1name = N'AlteracaoDeCampo';
ELSE
    EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'{texto}',
        @level0type = N'SCHEMA', @level0name = N'auditoria', @level1type = N'TABLE', @level1name = N'AlteracaoDeCampo';";
    }
}
