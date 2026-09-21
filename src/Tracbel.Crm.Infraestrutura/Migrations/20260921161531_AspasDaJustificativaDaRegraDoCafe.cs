using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// ISSUE 77 — AS ASPAS DOBRADAS DA REGRA DO CAFÉ. A <c>ReporSementeDaRegraDePotencial</c> (20/09/2026) gravou a
    /// origem com <c>\"\"</c> dentro de um literal SQL, onde aspas duplas não se escapam: a linha reposta ficou com
    /// <c>""na cultura de café…""</c>, e a tela do administrador mostrou assim. A semente do modelo sempre teve aspas
    /// simples; só as linhas repostas por aquela migração estão erradas.
    ///
    /// <para><b>Só UPDATE, só na linha 1, e só se ela tiver as aspas dobradas</b> — rodar de novo não muda nada, e
    /// nenhuma coluna ou tabela é apagada.</para>
    /// </summary>
    public partial class AspasDaJustificativaDaRegraDoCafe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE organizacao.RegraDePotencial
                   SET Justificativa = REPLACE(Justificativa, N'""', N'"')
                 WHERE Id = 1 AND Justificativa LIKE N'%""%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nada a desfazer: voltar as aspas dobradas seria reintroduzir o defeito.
        }
    }
}
