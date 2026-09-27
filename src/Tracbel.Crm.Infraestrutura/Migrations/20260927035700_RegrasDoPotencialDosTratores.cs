using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// AS SEIS REGRAS DE POTENCIAL DOS TRATORES — a D-P01 decidida em 27/09/2026 (issue 63).
    ///
    /// <para><b>De onde vêm os números.</b> Da aba "Administrador" da planilha de mapeamento dos 203
    /// municípios, que o comercial preenche ("hectares atendidos por 1 trator" e "taxa de troca"), e do
    /// padrão do protótipo Plataforma Inteligência Agro — os dois iguais, cultura por cultura. O café era a
    /// única divergência com o CRM (10 ha no exemplo do gerente, 20 ha na planilha), e o Ricardo mandou
    /// seguir o protótipo.</para>
    ///
    /// <para><b>A regra do café de 13/09 continua gravada.</b> O passado não se reescreve: ela vale até
    /// 26/09, e a vigência nova vale de 27/09 em diante. Nenhuma linha existente é alterada.</para>
    ///
    /// <para><b>Por que SQL escrito à mão, e sem Id.</b> A tela do Administrador grava nesta tabela com
    /// identidade; um <c>InsertData</c> com Id fixo colidiria com a linha que alguém tivesse registrado
    /// por lá. Pela mesma razão a inserção pula o produto que já tem, na categoria trator, uma vigência de
    /// pé de 27/09 ou posterior: é decisão tão nova quanto esta, e ela prevalece. Rodar de novo não
    /// duplica — o <see cref="Insercao"/> é o mesmo texto que o teste de contêiner executa duas vezes.</para>
    ///
    /// <para><b>O modelo de referência é "trator".</b> A planilha e o protótipo falam em trator, sem
    /// modelo; escrever um modelo aqui seria inventá-lo.</para>
    /// </summary>
    public partial class RegrasDoPotencialDosTratores : Migration
    {
        /// <summary>O primeiro dia das seis vigências.</summary>
        public const string VigenteDesde = "2026-09-27";

        private const string Fonte =
            "É o valor da aba Administrador da planilha de mapeamento dos 203 municípios e o padrão do protótipo " +
            "Plataforma Inteligência Agro — os dois iguais.";

        /// <summary>
        /// A inserção idempotente das seis vigências. Pública para o teste de contêiner rodá-la de novo e
        /// provar que não duplica.
        /// </summary>
        public const string Insercao = $"""
            DECLARE @trator int = (SELECT Id FROM organizacao.CategoriaDeMaquina WHERE Codigo = 'TRATOR');

            INSERT INTO organizacao.RegraDePotencial
                (ProdutoCodigoIbge, ProdutoNome, CulturaId, CategoriaDeMaquinaId, HectaresPorMaquina,
                 AnosDeRenovacao, ModeloDeReferencia, Situacao, VigenteDesde, Justificativa, InformadoEm)
            SELECT r.Produto, r.Nome, c.Id, @trator, r.Hectares, r.Anos, N'trator', 'Confirmada',
                   '{VigenteDesde}', r.Justificativa, '{VigenteDesde}'
            FROM (VALUES
                (40139, N'Café (em grão) Total', 'CAFE', 20.00, 10.0,
                 N'D-P01 decidida pelo Ricardo em 27/09/2026: 1 trator a cada 20 ha, trocado a cada 10 anos. {Fonte} Vale daqui em diante no lugar do exemplo de 13/09 (1 trator 3036N a cada 10 ha, sem ciclo).'),
                (40106, N'Cana-de-açúcar', 'CANA', 170.00, 8.0,
                 N'D-P01 decidida pelo Ricardo em 27/09/2026: 1 trator a cada 170 ha, trocado a cada 8 anos. {Fonte}'),
                (40101, N'Amendoim (em casca)', 'AMENDOIM', 200.00, 8.0,
                 N'D-P01 decidida pelo Ricardo em 27/09/2026: 1 trator a cada 200 ha, trocado a cada 8 anos. {Fonte}'),
                (40124, N'Soja (em grão)', 'SOJA', 200.00, 10.0,
                 N'D-P01 decidida pelo Ricardo em 27/09/2026: 1 trator a cada 200 ha, trocado a cada 10 anos. {Fonte}'),
                (40122, N'Milho (em grão)', 'MILHO', 200.00, 10.0,
                 N'D-P01 decidida pelo Ricardo em 27/09/2026: 1 trator a cada 200 ha, trocado a cada 10 anos. {Fonte}'),
                (40151, N'Laranja', 'LARANJA', 20.00, 10.0,
                 N'D-P01 decidida pelo Ricardo em 27/09/2026: 1 trator a cada 20 ha, trocado a cada 10 anos. {Fonte}')
            ) AS r (Produto, Nome, Cultura, Hectares, Anos, Justificativa)
            JOIN organizacao.Cultura c ON c.Codigo = r.Cultura
            WHERE @trator IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1 FROM organizacao.RegraDePotencial x
                  WHERE x.ProdutoCodigoIbge = r.Produto
                    AND x.CategoriaDeMaquinaId = @trator
                    AND x.RevogadoEm IS NULL
                    AND x.VigenteDesde >= '{VigenteDesde}');
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(Insercao);

        /// <inheritdoc />
        /// <remarks>
        /// Tira só o que esta migração pôs: as vigências de 27/09 sem autor. A que tiver autor foi gravada
        /// pela tela, por alguém, e não é desta migração desfazer.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql($"""
                DELETE FROM organizacao.RegraDePotencial
                WHERE VigenteDesde = '{VigenteDesde}'
                  AND InformadoPorId IS NULL
                  AND CategoriaDeMaquinaId = (SELECT Id FROM organizacao.CategoriaDeMaquina WHERE Codigo = 'TRATOR')
                  AND ProdutoCodigoIbge IN (40139, 40106, 40101, 40124, 40122, 40151);
                """);
    }
}
