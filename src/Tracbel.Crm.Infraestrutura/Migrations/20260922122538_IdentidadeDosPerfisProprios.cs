using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// OS PERFIS PRÓPRIOS NUNCA COLIDEM COM A SEMENTE (issue 113, parte 2b).
    ///
    /// <para>A semente usa identificadores fixos: perfis 1 a 7, e as permissões na faixa 100 × perfil + posição (a
    /// Diretoria vai até 704). Sem isto, o primeiro perfil criado pela tela nasceria com o identificador 8, e a
    /// primeira permissão dele com 705 — os mesmos que o sistema usaria para o próximo perfil semeado ou para a
    /// próxima permissão da Diretoria. A publicação que os semeasse quebraria na chave primária.</para>
    ///
    /// <para>Daqui em diante, o que a tela cria nasce a partir de 1000 (perfil) e de 100.000 (permissão). Só
    /// empurra para a frente, e só se ainda não passou do limite: rodar de novo não faz nada.</para>
    /// </summary>
    public partial class IdentidadeDosPerfisProprios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsSqlServer()) return;

            migrationBuilder.Sql(
                "IF IDENT_CURRENT('seguranca.Perfil') < 999 DBCC CHECKIDENT ('seguranca.Perfil', RESEED, 999);");
            migrationBuilder.Sql(
                "IF IDENT_CURRENT('seguranca.PerfilPermissao') < 99999 DBCC CHECKIDENT ('seguranca.PerfilPermissao', RESEED, 99999);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NÃO VOLTA: trazer a identidade de volta faria o próximo perfil criado colidir com um que já existe.
        }
    }
}