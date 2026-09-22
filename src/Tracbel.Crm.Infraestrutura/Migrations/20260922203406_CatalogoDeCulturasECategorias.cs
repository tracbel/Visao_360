using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoDeCulturasECategorias : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CulturaId",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoriaDeMaquina",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaDeMaquina", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cultura",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Segmento = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UnidadeComercial = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    QuilosPorUnidade = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    FonteDoPreco = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ProdutoDoPreco = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    SerieDeCusto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cultura", x => x.Id);
                    table.CheckConstraint("CK_Cultura_FonteDoPreco", "([FonteDoPreco] IS NULL AND [ProdutoDoPreco] IS NULL) OR ([FonteDoPreco] IS NOT NULL AND [ProdutoDoPreco] IS NOT NULL)");
                    table.CheckConstraint("CK_Cultura_QuilosPorUnidade", "[QuilosPorUnidade] > 0");
                    table.CheckConstraint("CK_Cultura_Segmento", "[Segmento] IN ('Graos','Cana','Citros','Cafe','Fruticultura','Olericultura','Algodao','Borracha','Outros')");
                });

            migrationBuilder.CreateTable(
                name: "ProdutoDoSicorNaCategoria",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaDeMaquinaId = table.Column<int>(type: "int", nullable: false),
                    CodigoProduto = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoDoSicorNaCategoria", x => x.Id);
                    table.CheckConstraint("CK_ProdutoDoSicorNaCategoria_Produto", "[CodigoProduto] > 0");
                    table.ForeignKey(
                        name: "FK_ProdutoDoSicorNaCategoria_CategoriaDeMaquina_CategoriaDeMaquinaId",
                        column: x => x.CategoriaDeMaquinaId,
                        principalSchema: "organizacao",
                        principalTable: "CategoriaDeMaquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoDaPamNaCultura",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CulturaId = table.Column<int>(type: "int", nullable: false),
                    ProdutoCodigoIbge = table.Column<int>(type: "int", nullable: false),
                    ProdutoNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntraNaSomaDaLavoura = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoDaPamNaCultura", x => x.Id);
                    table.CheckConstraint("CK_ProdutoDaPamNaCultura_Produto", "[ProdutoCodigoIbge] > 0");
                    table.ForeignKey(
                        name: "FK_ProdutoDaPamNaCultura_Cultura_CulturaId",
                        column: x => x.CulturaId,
                        principalSchema: "organizacao",
                        principalTable: "Cultura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "CategoriaDeMaquina",
                columns: new[] { "Id", "Codigo", "EstaAtiva", "Nome", "Ordem" },
                values: new object[,]
                {
                    { 1, "TRATOR", true, "Trator", (short)1 },
                    { 2, "PLANTADEIRA", true, "Plantadeira", (short)2 },
                    { 3, "COLHEITADEIRA", true, "Colheitadeira", (short)3 },
                    { 4, "PULVERIZADOR", true, "Pulverizador", (short)4 },
                    { 5, "IMPLEMENTO", true, "Implemento", (short)5 },
                    { 6, "PRECISAO", true, "Agricultura de precisão", (short)6 }
                });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "Cultura",
                columns: new[] { "Id", "Codigo", "EstaAtiva", "FonteDoPreco", "Nome", "ProdutoDoPreco", "QuilosPorUnidade", "Segmento", "SerieDeCusto", "UnidadeComercial" },
                values: new object[,]
                {
                    { 1, "CAFE", true, "CONAB", "Café", "11195", 60m, "Cafe", "CAFÉ ARÁBICA", "saca de 60 kg" },
                    { 2, "CANA", true, "CONAB", "Cana-de-açúcar", "4238", 1000m, "Cana", "CANA DE AÇÚCAR", "tonelada" },
                    { 3, "SOJA", true, "CONAB", "Soja", "4744", 60m, "Graos", "SOJA", "saca de 60 kg" },
                    { 4, "MILHO", true, "CONAB", "Milho", "4742", 60m, "Graos", "MILHO", "saca de 60 kg" },
                    { 5, "LARANJA", true, "CONAB", "Laranja", "12290", 40.8m, "Citros", "LARANJA", "caixa de 40,8 kg" },
                    { 6, "AMENDOIM", true, "CONAB", "Amendoim", "4674", 25m, "Graos", "AMENDOIM", "saca de 25 kg" }
                });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "RegraDePotencial",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoriaDeMaquinaId", "CulturaId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "ProdutoDaPamNaCultura",
                columns: new[] { "Id", "CulturaId", "EntraNaSomaDaLavoura", "ProdutoCodigoIbge", "ProdutoNome" },
                values: new object[,]
                {
                    { 1, 1, true, 40139, "Café (em grão) Total" },
                    { 2, 1, false, 40140, "Café (em grão) Arábica" },
                    { 3, 1, false, 40141, "Café (em grão) Canephora" },
                    { 4, 2, true, 40106, "Cana-de-açúcar" },
                    { 5, 3, true, 40124, "Soja (em grão)" },
                    { 6, 4, true, 40122, "Milho (em grão)" },
                    { 7, 5, true, 40151, "Laranja" },
                    { 8, 6, true, 40101, "Amendoim (em casca)" }
                });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "ProdutoDoSicorNaCategoria",
                columns: new[] { "Id", "CategoriaDeMaquinaId", "CodigoProduto", "Descricao" },
                values: new object[,]
                {
                    { 1, 1, 7080, "TRATOR" },
                    { 2, 3, 2700, "COLHEITADEIRAS, COLHEDEIRAS E ARRANCADEIRAS" },
                    { 3, 5, 4860, "MÁQUINAS E IMPLEMENTOS" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegraDePotencial_CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "CategoriaDeMaquinaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegraDePotencial_CulturaId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "CulturaId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "UX_CategoriaDeMaquina_Codigo",
                schema: "organizacao",
                table: "CategoriaDeMaquina",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Cultura_Codigo",
                schema: "organizacao",
                table: "Cultura",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoDaPamNaCultura_CulturaId",
                schema: "organizacao",
                table: "ProdutoDaPamNaCultura",
                column: "CulturaId");

            migrationBuilder.CreateIndex(
                name: "UX_ProdutoDaPamNaCultura_Produto",
                schema: "organizacao",
                table: "ProdutoDaPamNaCultura",
                column: "ProdutoCodigoIbge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoDoSicorNaCategoria_CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "ProdutoDoSicorNaCategoria",
                column: "CategoriaDeMaquinaId");

            migrationBuilder.CreateIndex(
                name: "UX_ProdutoDoSicorNaCategoria_Produto",
                schema: "organizacao",
                table: "ProdutoDoSicorNaCategoria",
                column: "CodigoProduto",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegraDePotencial_CategoriaDeMaquina_CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "CategoriaDeMaquinaId",
                principalSchema: "organizacao",
                principalTable: "CategoriaDeMaquina",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegraDePotencial_Cultura_CulturaId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "CulturaId",
                principalSchema: "organizacao",
                principalTable: "Cultura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegraDePotencial_CategoriaDeMaquina_CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropForeignKey(
                name: "FK_RegraDePotencial_Cultura_CulturaId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropTable(
                name: "ProdutoDaPamNaCultura",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "ProdutoDoSicorNaCategoria",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "Cultura",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "CategoriaDeMaquina",
                schema: "organizacao");

            migrationBuilder.DropIndex(
                name: "IX_RegraDePotencial_CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropIndex(
                name: "IX_RegraDePotencial_CulturaId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "CategoriaDeMaquinaId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropColumn(
                name: "CulturaId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
