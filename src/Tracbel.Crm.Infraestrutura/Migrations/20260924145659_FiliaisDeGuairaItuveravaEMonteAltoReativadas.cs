using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// GUAÍRA (010104), ITUVERAVA (010105) E MONTE ALTO (010110) VOLTAM A SER FILIAIS ATIVAS — decisão do Ricardo em
    /// 24/09/2026.
    ///
    /// <para><b>Por que elas estavam inativas.</b> O seed de 04/09/2026 ativou só as 13 filiais "em operação" da lista
    /// daquele dia. Medido em 24/09: as três FATURAM no Protheus (as três estão em <c>SYS_COMPANY</c> e em
    /// <c>X_V_UTIL_FILIAIS</c>), a carga da SA1 já pôs 2.168 clientes sob elas pela área de atuação, e o Vórtice tem
    /// nelas as carteiras do pool da Inteligência de Mercado (<c>TBA_*</c>, filial 05) e as contas-chave (filial 10).
    /// Inativas, elas ficavam fora do seletor de filial, do consolidado da Visão 360 e do de-para das cargas.</para>
    ///
    /// <para><b>A filial 06 NÃO é criada:</b> não existe 010106 no Protheus, e o <c>NroEmpresa</c> 6 do Vórtice é a
    /// "Colorado desativado", que o CRM já tem como <c>CFRA_COLORADO_6</c>, inativa. Filial não se inventa.</para>
    ///
    /// <para><b>Repetível e só com o que existe:</b> reativa as que estão inativas e não mexe em mais nada. Num banco
    /// sem essas filiais (o de teste, que nasce vazio) ela não faz nada — quem as cria é o seed, que passou a trazê-las
    /// ativas. O banco central do servidor não recebe o seed; é por isso que a mudança vai numa migração, que roda na
    /// subida da API.</para>
    /// </summary>
    public partial class FiliaisDeGuairaItuveravaEMonteAltoReativadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE organizacao.Empresa
SET EstaAtiva = 1, AlteradoEm = SYSUTCDATETIME()
WHERE Codigo IN ('010104', '010105', '010110') AND EstaAtiva = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE organizacao.Empresa
SET EstaAtiva = 0, AlteradoEm = SYSUTCDATETIME()
WHERE Codigo IN ('010104', '010105', '010110') AND EstaAtiva = 1;");
        }
    }
}
