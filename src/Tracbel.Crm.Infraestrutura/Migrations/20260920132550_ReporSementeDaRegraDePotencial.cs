using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// REPÕE A REGRA DE POTENCIAL, que é SEMENTE do sistema e foi apagada como se fosse dado
    /// (issue 98).
    ///
    /// <para><b>O que aconteceu.</b> A regra nasce em <c>RegraDePotencialConfiguracao</c>, por
    /// <c>HasData</c>, e a migração de 13/09/2026 a inseriu. A sanitização de 15/09/2026
    /// (<c>sanitizar-dados-2026-09-15.sql</c>, linha 121) classificou
    /// <c>organizacao.RegraDePotencial</c> como "configuração do território" e apagou a linha, junto
    /// com a configuração herdada do Vórtice — mas esta regra não veio do Vórtice: veio da frase do
    /// gerente comercial de 13/09. Como a migração que a semeou já está aplicada, o
    /// <c>HasData</c> não a repõe: banco novo nasce com ela, banco existente fica sem.</para>
    ///
    /// <para><b>Efeito medido em 20/09/2026, no servidor:</b> 54.570 linhas de área plantada e
    /// <b>zero</b> regra — o mapa C da Visão 360 dizia "sem regra de potencial ativa" e não calculava
    /// nada.</para>
    ///
    /// <para><b>Por que SQL escrito à mão, e com <c>IF NOT EXISTS</c>.</b> Um <c>InsertData</c>
    /// quebraria onde a linha existe (banco novo, banco de ensaio recriado). O <c>IF NOT EXISTS</c>
    /// deixa a migração idempotente: repõe onde falta, não toca onde já está. Os valores são
    /// exatamente os do <c>HasData</c>, para que o modelo e o banco não divirjam.</para>
    /// </summary>
    public partial class ReporSementeDaRegraDePotencial : Migration
    {
        private const string Origem =
            "Exemplo do gerente comercial no pedido de 13/09/2026: \"\"na cultura de café, existe " +
            "potencial de 1 trator 3036N a cada 10 hectares\"\". Não confirmados: aplicabilidade, " +
            "vigência, horizonte e arredondamento.";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM organizacao.RegraDePotencial WHERE Id = 1)
                BEGIN
                    SET IDENTITY_INSERT organizacao.RegraDePotencial ON;

                    INSERT INTO organizacao.RegraDePotencial
                        (Id, ProdutoCodigoIbge, ProdutoNome, HectaresPorMaquina, ModeloDeReferencia,
                         Situacao, Origem, InformadaEm, EstaAtiva)
                    VALUES
                        (1, 40139, N'Café (em grão) Total', 10.00, N'3036N',
                         'AConfirmar', N'{Origem}', '2026-09-13', 1);

                    SET IDENTITY_INSERT organizacao.RegraDePotencial OFF;
                END
                """);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql("DELETE FROM organizacao.RegraDePotencial WHERE Id = 1;");
    }
}
