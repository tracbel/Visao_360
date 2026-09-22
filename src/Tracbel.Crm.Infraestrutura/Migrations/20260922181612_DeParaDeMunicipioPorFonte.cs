using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class DeParaDeMunicipioPorFonte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.CreateTable(
                name: "CorrespondenciaDeMunicipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fonte = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ChaveNaFonte = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false, collation: "Latin1_General_BIN2"),
                    TextoNaFonte = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    Forma = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    CasadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CasadaPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrespondenciaDeMunicipio", x => x.Id);
                    table.CheckConstraint("CK_CorrespondenciaDeMunicipio_Forma", "[Forma] IN ('NomeUnicoNaUf','CodigoDoIbgeNaFonte','Manual')");
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDeMunicipio_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDeMunicipio_Usuario_CasadaPorId",
                        column: x => x.CasadaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenciaDeMunicipio_CasadaPorId",
                schema: "organizacao",
                table: "CorrespondenciaDeMunicipio",
                column: "CasadaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenciaDeMunicipio_MunicipioId",
                schema: "organizacao",
                table: "CorrespondenciaDeMunicipio",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "UX_CorrespondenciaDeMunicipio_Fonte_Chave",
                schema: "organizacao",
                table: "CorrespondenciaDeMunicipio",
                columns: new[] { "Fonte", "ChaveNaFonte" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrespondenciaDeMunicipio",
                schema: "organizacao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
