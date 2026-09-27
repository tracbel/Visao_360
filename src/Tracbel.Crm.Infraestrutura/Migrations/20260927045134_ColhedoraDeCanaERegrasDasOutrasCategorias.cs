using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// AS OUTRAS CATEGORIAS DA D-P01 E A REFERÊNCIA DE CUSTO DA D-P07 — as duas aprovadas pelo Ricardo em
    /// 27/09/2026 (issue 63).
    ///
    /// <para><b>A colhedora de cana vira categoria</b> (a sétima, no fim da lista da semente), e a linha
    /// <c>COLHEDORA_DE_CANA</c> do ART passa a apontar para ela. Colher cana é outra máquina que colher grão:
    /// ligá-la à colheitadeira misturava dois mercados, e deixá-la sem categoria tirava da captura a máquina
    /// mais vendida na região da cana.</para>
    ///
    /// <para><b>Oito regras novas, de 27/09/2026</b>, com os valores do padrão do protótipo Plataforma
    /// Inteligência Agro, aprovado como está: colheitadeira de soja e milho (1.500 ha, troca em 10 anos),
    /// plantadeira de soja e milho (800 ha, 10 anos), pulverizador de cana (1.500 ha, 8 anos) e de soja e
    /// milho (1.000 ha, 8 anos) e colhedora de cana (700 ha, 8 anos). As regras de trator (#239) não mudam.
    /// Mesmo desenho daquela migração: SQL escrito à mão, sem Id fixo, idempotente, e pulando o par produto ×
    /// categoria que já tenha vigência de pé de 27/09 em diante.</para>
    ///
    /// <para><b>A D-P07 é a referência da planilha do comercial</b>: Franca para o café e Piracicaba para a
    /// cana, na camada do custo TOTAL — o custo total da CONAB nesses dois locais bate ao centavo com a
    /// planilha. Só grava onde ninguém decidiu antes (a tela do Administrador também grava ali), e as outras
    /// quatro culturas continuam sem referência: a CONAB não publica custo delas em São Paulo.</para>
    /// </summary>
    public partial class ColhedoraDeCanaERegrasDasOutrasCategorias : Migration
    {
        /// <summary>O primeiro dia das oito vigências.</summary>
        public const string VigenteDesde = "2026-09-27";

        private const string Fonte =
            "É o padrão do protótipo Plataforma Inteligência Agro, aprovado como está — a planilha da Tracbel só tem o trator.";

        /// <summary>
        /// A inserção idempotente das oito vigências. Pública para o teste de contêiner rodá-la de novo e
        /// provar que não duplica.
        /// </summary>
        public const string InsercaoDasRegras = $"""
            INSERT INTO organizacao.RegraDePotencial
                (ProdutoCodigoIbge, ProdutoNome, CulturaId, CategoriaDeMaquinaId, HectaresPorMaquina,
                 AnosDeRenovacao, ModeloDeReferencia, Situacao, VigenteDesde, Justificativa, InformadoEm)
            SELECT r.Produto, r.Nome, c.Id, cat.Id, r.Hectares, r.Anos, r.Modelo, 'Confirmada',
                   '{VigenteDesde}', r.Justificativa, '{VigenteDesde}'
            FROM (VALUES
                (40124, N'Soja (em grão)', 'SOJA', 'COLHEITADEIRA', 1500.00, 10.0, N'colheitadeira',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para a colheitadeira: 1 a cada 1.500 ha, com troca a cada 10 anos. {Fonte}'),
                (40122, N'Milho (em grão)', 'MILHO', 'COLHEITADEIRA', 1500.00, 10.0, N'colheitadeira',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para a colheitadeira: 1 a cada 1.500 ha, com troca a cada 10 anos. {Fonte}'),
                (40124, N'Soja (em grão)', 'SOJA', 'PLANTADEIRA', 800.00, 10.0, N'plantadeira',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para a plantadeira: 1 a cada 800 ha, com troca a cada 10 anos. {Fonte}'),
                (40122, N'Milho (em grão)', 'MILHO', 'PLANTADEIRA', 800.00, 10.0, N'plantadeira',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para a plantadeira: 1 a cada 800 ha, com troca a cada 10 anos. {Fonte}'),
                (40106, N'Cana-de-açúcar', 'CANA', 'PULVERIZADOR', 1500.00, 8.0, N'pulverizador',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para o pulverizador: 1 a cada 1.500 ha de cana, com troca a cada 8 anos. {Fonte}'),
                (40124, N'Soja (em grão)', 'SOJA', 'PULVERIZADOR', 1000.00, 8.0, N'pulverizador',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para o pulverizador: 1 a cada 1.000 ha de soja, com troca a cada 8 anos. {Fonte}'),
                (40122, N'Milho (em grão)', 'MILHO', 'PULVERIZADOR', 1000.00, 8.0, N'pulverizador',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para o pulverizador: 1 a cada 1.000 ha de milho, com troca a cada 8 anos. {Fonte}'),
                (40106, N'Cana-de-açúcar', 'CANA', 'COLHEDORA_DE_CANA', 700.00, 8.0, N'colhedora de cana',
                 N'D-P01 aprovada pelo Ricardo em 27/09/2026 para a colhedora de cana: 1 a cada 700 ha, com troca a cada 8 anos. {Fonte}')
            ) AS r (Produto, Nome, Cultura, Categoria, Hectares, Anos, Modelo, Justificativa)
            JOIN organizacao.Cultura c ON c.Codigo = r.Cultura
            JOIN organizacao.CategoriaDeMaquina cat ON cat.Codigo = r.Categoria
            WHERE NOT EXISTS (
                SELECT 1 FROM organizacao.RegraDePotencial x
                WHERE x.ProdutoCodigoIbge = r.Produto
                  AND x.CategoriaDeMaquinaId = cat.Id
                  AND x.RevogadoEm IS NULL
                  AND x.VigenteDesde >= '{VigenteDesde}');
            """;

        /// <summary>
        /// A referência de custo da D-P07, só onde ninguém decidiu antes. Pública pelo mesmo motivo.
        /// </summary>
        public const string ReferenciaDoCusto = """
            UPDATE organizacao.Cultura
               SET LocalDeReferenciaDoCusto = N'Franca', CamadaDeCustoDaMargem = 'Total'
             WHERE Codigo = 'CAFE' AND LocalDeReferenciaDoCusto IS NULL AND CamadaDeCustoDaMargem IS NULL;

            UPDATE organizacao.Cultura
               SET LocalDeReferenciaDoCusto = N'Piracicaba', CamadaDeCustoDaMargem = 'Total'
             WHERE Codigo = 'CANA' AND LocalDeReferenciaDoCusto IS NULL AND CamadaDeCustoDaMargem IS NULL;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "CategoriaDeMaquina",
                columns: new[] { "Id", "Codigo", "EstaAtiva", "Nome", "Ordem" },
                values: new object[] { 7, "COLHEDORA_DE_CANA", true, "Colhedora de cana", (short)7 });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "LinhaDeProdutoNaCategoria",
                columns: new[] { "Id", "CategoriaDeMaquinaId", "CodigoDaLinha", "Descricao" },
                values: new object[] { 9, 7, "COLHEDORA_DE_CANA", "Colhedora de cana" });

            // AS REGRAS DEPOIS DA CATEGORIA: a da colhedora de cana aponta para ela.
            migrationBuilder.Sql(InsercaoDasRegras);
            migrationBuilder.Sql(ReferenciaDoCusto);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Desfaz na ordem inversa, e só o que esta migração pôs: as vigências de 27/09 sem autor nas quatro
        /// categorias, e a referência de custo onde ela ainda é a que esta migração gravou.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                UPDATE organizacao.Cultura SET LocalDeReferenciaDoCusto = NULL, CamadaDeCustoDaMargem = NULL
                 WHERE (Codigo = 'CAFE' AND LocalDeReferenciaDoCusto = N'Franca' AND CamadaDeCustoDaMargem = 'Total')
                    OR (Codigo = 'CANA' AND LocalDeReferenciaDoCusto = N'Piracicaba' AND CamadaDeCustoDaMargem = 'Total');

                DELETE r FROM organizacao.RegraDePotencial r
                  JOIN organizacao.CategoriaDeMaquina cat ON cat.Id = r.CategoriaDeMaquinaId
                 WHERE r.VigenteDesde = '{VigenteDesde}'
                   AND r.InformadoPorId IS NULL
                   AND cat.Codigo IN ('COLHEITADEIRA', 'PLANTADEIRA', 'PULVERIZADOR', 'COLHEDORA_DE_CANA');
                """);

            migrationBuilder.DeleteData(
                schema: "organizacao",
                table: "LinhaDeProdutoNaCategoria",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "organizacao",
                table: "CategoriaDeMaquina",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
