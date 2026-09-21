using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class CreditoRuralDoSicor : Migration
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
                name: "CreditoRuralDeInvestimento",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    CodigoMunicipioBcb = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<short>(type: "smallint", nullable: false),
                    Mes = table.Column<byte>(type: "tinyint", nullable: false),
                    CodigoProduto = table.Column<int>(type: "int", nullable: false),
                    CodigoPrograma = table.Column<int>(type: "int", nullable: false),
                    CodigoSubprograma = table.Column<int>(type: "int", nullable: false),
                    CodigoFonte = table.Column<int>(type: "int", nullable: false),
                    CodigoSeguro = table.Column<int>(type: "int", nullable: false),
                    Atividade = table.Column<byte>(type: "tinyint", nullable: false),
                    CodigoModalidade = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditoRuralDeInvestimento", x => x.Id);
                    table.CheckConstraint("CK_CreditoRuralDeInvestimento_Mes", "[Mes] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_CreditoRuralDeInvestimento_Valor", "[Valor] > 0 AND [Area] >= 0");
                    table.ForeignKey(
                        name: "FK_CreditoRuralDeInvestimento_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditoRuralDeInvestimento_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemDoSicor",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Codigo = table.Column<int>(type: "int", nullable: false),
                    CodigoPai = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    VigenteDe = table.Column<DateOnly>(type: "date", nullable: true),
                    VigenteAte = table.Column<DateOnly>(type: "date", nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemDoSicor", x => x.Id);
                    table.CheckConstraint("CK_ItemDoSicor_Tipo", "[Tipo] IN ('PROGRAMA', 'SUBPROGRAMA', 'FONTE', 'PRODUTO')");
                    table.ForeignKey(
                        name: "FK_ItemDoSicor_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemConjuntoPermissao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemConjuntoPermissao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_CreditoRuralDeInvestimento_ImportadoPorId",
                schema: "organizacao",
                table: "CreditoRuralDeInvestimento",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditoRuralDeInvestimento_Municipio_Periodo",
                schema: "organizacao",
                table: "CreditoRuralDeInvestimento",
                columns: new[] { "MunicipioId", "Ano", "Mes", "CodigoProduto" })
                .Annotation("SqlServer:Include", new[] { "Valor" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditoRuralDeInvestimento_Periodo_Produto",
                schema: "organizacao",
                table: "CreditoRuralDeInvestimento",
                columns: new[] { "Ano", "Mes", "CodigoProduto" })
                .Annotation("SqlServer:Include", new[] { "Valor", "MunicipioId" });

            migrationBuilder.CreateIndex(
                name: "UX_CreditoRuralDeInvestimento_Combinacao",
                schema: "organizacao",
                table: "CreditoRuralDeInvestimento",
                columns: new[] { "CodigoMunicipioBcb", "Ano", "Mes", "CodigoProduto", "CodigoPrograma", "CodigoSubprograma", "CodigoFonte", "CodigoSeguro", "Atividade", "CodigoModalidade" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemDoSicor_ImportadoPorId",
                schema: "organizacao",
                table: "ItemDoSicor",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_ItemDoSicor_Tipo_Codigo",
                schema: "organizacao",
                table: "ItemDoSicor",
                columns: new[] { "Tipo", "CodigoPai", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditoRuralDeInvestimento",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "ItemDoSicor",
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");
        }
    }
}
