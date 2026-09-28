using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// A D-P07 PARA AS OUTRAS QUATRO CULTURAS — decidida pelo Ricardo em 28/09/2026: a cidade da CONAB mais perto da
    /// ADR com a safra mais recente, no custo TOTAL, como o café e a cana.
    ///
    /// <para>Amendoim em <b>Jaboticabal</b> (safras 2014–2024), laranja em <b>Itápolis</b> (2022–2025), milho em
    /// <b>Assis</b> (a série "MILHO 2ª SAFRA", 2019–2023) e soja em <b>Assis</b> (2019–2021, a última que a CONAB publicou
    /// em São Paulo). A migração de 27/09 dizia que a CONAB não publicava custo delas no estado: publica, só não nos
    /// locais da planilha do comercial.</para>
    ///
    /// <para><b>Só onde ninguém decidiu antes</b> — a tela do Administrador também grava ali —, e sem mudança no modelo:
    /// é SQL escrito à mão, idempotente, e o Down só desfaz o que ainda é o que esta migração gravou.</para>
    /// </summary>
    public partial class ReferenciaDoCustoDasOutrasCulturas : Migration
    {
        /// <summary>
        /// A referência de custo das quatro culturas. Pública para o teste de contêiner rodá-la de novo e provar que não
        /// sobrescreve.
        /// </summary>
        public const string ReferenciaDoCusto = """
            UPDATE organizacao.Cultura
               SET LocalDeReferenciaDoCusto = N'Jaboticabal', CamadaDeCustoDaMargem = 'Total'
             WHERE Codigo = 'AMENDOIM' AND LocalDeReferenciaDoCusto IS NULL AND CamadaDeCustoDaMargem IS NULL;

            UPDATE organizacao.Cultura
               SET LocalDeReferenciaDoCusto = N'Itápolis', CamadaDeCustoDaMargem = 'Total'
             WHERE Codigo = 'LARANJA' AND LocalDeReferenciaDoCusto IS NULL AND CamadaDeCustoDaMargem IS NULL;

            UPDATE organizacao.Cultura
               SET LocalDeReferenciaDoCusto = N'Assis', CamadaDeCustoDaMargem = 'Total'
             WHERE Codigo IN ('MILHO', 'SOJA') AND LocalDeReferenciaDoCusto IS NULL AND CamadaDeCustoDaMargem IS NULL;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReferenciaDoCusto);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE organizacao.Cultura SET LocalDeReferenciaDoCusto = NULL, CamadaDeCustoDaMargem = NULL
                 WHERE CamadaDeCustoDaMargem = 'Total'
                   AND ((Codigo = 'AMENDOIM' AND LocalDeReferenciaDoCusto = N'Jaboticabal')
                     OR (Codigo = 'LARANJA' AND LocalDeReferenciaDoCusto = N'Itápolis')
                     OR (Codigo IN ('MILHO', 'SOJA') AND LocalDeReferenciaDoCusto = N'Assis'));
                """);
        }
    }
}
