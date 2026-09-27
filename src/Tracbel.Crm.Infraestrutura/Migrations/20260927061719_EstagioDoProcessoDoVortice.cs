using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class EstagioDoProcessoDoVortice : Migration
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
                name: "ClassificacaoDeResultadoDoVortice",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoNaOrigem = table.Column<int>(type: "int", nullable: false),
                    Estagio = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: true),
                    ContaComoContato = table.Column<bool>(type: "bit", nullable: false),
                    Fonte = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificacaoDeResultadoDoVortice", x => x.Id);
                    table.CheckConstraint("CK_ClassificacaoDeResultadoDoVortice_Codigo", "[CodigoNaOrigem] > 0");
                    table.CheckConstraint("CK_ClassificacaoDeResultadoDoVortice_Estagio", "[Estagio] IS NULL OR [Estagio] IN ('Lead','Qualificado','Cobertura','Negociacao','Pedido','Faturamento')");
                    table.CheckConstraint("CK_ClassificacaoDeResultadoDoVortice_Utilidade", "[Estagio] IS NOT NULL OR [ContaComoContato] = 1");
                });

            migrationBuilder.CreateTable(
                name: "EstagioDoProcesso",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    NumeroDoProcessoNaOrigem = table.Column<long>(type: "bigint", nullable: false),
                    TipoDeProcessoNaOrigem = table.Column<short>(type: "smallint", nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    CarteiraId = table.Column<long>(type: "bigint", nullable: true),
                    ResponsavelId = table.Column<long>(type: "bigint", nullable: true),
                    AbertoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    AberturaDeduzida = table.Column<bool>(type: "bit", nullable: false),
                    Desfecho = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DesfechoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Estagio = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    AlcancadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UltimaAcaoDaEtapaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    HerdadoDoProcessoDna = table.Column<bool>(type: "bit", nullable: false),
                    NumeroDoProcessoDnaNaOrigem = table.Column<long>(type: "bigint", nullable: true),
                    ResultadoQueAbriu = table.Column<int>(type: "int", nullable: false),
                    PelaEntradaDigital = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstagioDoProcesso", x => x.Id);
                    table.CheckConstraint("CK_EstagioDoProcesso_Desfecho", "[Desfecho] IN ('Aberto','Suspenso','Ganho','Perdido','Cancelado')");
                    table.CheckConstraint("CK_EstagioDoProcesso_Estagio", "[Estagio] IN ('Lead','Qualificado','Cobertura','Negociacao','Pedido','Faturamento')");
                    table.CheckConstraint("CK_EstagioDoProcesso_Heranca", "([HerdadoDoProcessoDna] = 1 AND [NumeroDoProcessoDnaNaOrigem] IS NOT NULL) OR ([HerdadoDoProcessoDna] = 0 AND [NumeroDoProcessoDnaNaOrigem] IS NULL)");
                    table.CheckConstraint("CK_EstagioDoProcesso_Tipo", "[TipoDeProcessoNaOrigem] IN (31, 41, 50)");
                    table.ForeignKey(
                        name: "FK_EstagioDoProcesso_Carteira_CarteiraId",
                        column: x => x.CarteiraId,
                        principalSchema: "organizacao",
                        principalTable: "Carteira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstagioDoProcesso_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstagioDoProcesso_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstagioDoProcesso_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstagioDoProcesso_Usuario_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "ClassificacaoDeResultadoDoVortice",
                columns: new[] { "Id", "CodigoNaOrigem", "ContaComoContato", "Estagio", "Fonte" },
                values: new object[,]
                {
                    { 1, 1278, false, "Lead", "extrator do funil: Lead (l. 257)" },
                    { 2, 3803, false, "Qualificado", "extrator do funil: Qualificado (l. 384)" },
                    { 3, 250, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 4, 252, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 5, 254, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 6, 255, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 7, 260, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 8, 263, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 9, 265, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 10, 267, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 11, 299, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 12, 300, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 13, 304, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 14, 305, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 15, 306, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 16, 307, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 17, 570, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 18, 1286, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 19, 3225, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 20, 3227, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 21, 3639, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 22, 3640, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 23, 2605, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 24, 1929, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 25, 2547, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 26, 2553, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 27, 2554, true, "Cobertura", "extrator do funil: Cobertura (l. 520–525); BI_CARTEIRA_VN (l. 86–87)" },
                    { 28, 2563, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 29, 2564, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 30, 2565, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 31, 2566, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 32, 2568, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 33, 3234, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 34, 3572, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 35, 3573, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 36, 2612, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 37, 3223, true, "Negociacao", "extrator do funil: Negociação (l. 666–669); BI_CARTEIRA_VN (l. 86–87)" },
                    { 38, 2607, false, "Negociacao", "extrator do funil: Negociação (l. 666–669); também Cobertura (decisão de 27/09/2026)" },
                    { 39, 2609, false, "Negociacao", "extrator do funil: Negociação (l. 666–669); também Cobertura (decisão de 27/09/2026)" },
                    { 40, 2610, false, "Negociacao", "extrator do funil: Negociação (l. 666–669); também Cobertura (decisão de 27/09/2026)" },
                    { 41, 2548, true, "Pedido", "extrator do funil: Pedido (l. 803); BI_CARTEIRA_VN (l. 86–87)" },
                    { 42, 3231, true, "Pedido", "extrator do funil: Pedido (l. 803); BI_CARTEIRA_VN (l. 86–87)" },
                    { 43, 3232, true, "Pedido", "extrator do funil: Pedido (l. 803); BI_CARTEIRA_VN (l. 86–87)" },
                    { 44, 3239, false, "Pedido", "extrator do funil: Pedido (l. 803)" },
                    { 45, 3663, true, "Pedido", "extrator do funil: Pedido (l. 803); BI_CARTEIRA_VN (l. 86–87)" },
                    { 46, 2529, false, "Faturamento", "extrator do funil: Faturamento (l. 936)" },
                    { 47, 2530, false, "Faturamento", "extrator do funil: Faturamento (l. 936)" },
                    { 48, 3440, false, "Faturamento", "extrator do funil: Faturamento (l. 936)" },
                    { 49, 3494, false, "Faturamento", "extrator do funil: Faturamento (l. 936)" },
                    { 50, 257, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 51, 258, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 52, 302, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 53, 308, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 54, 3226, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 55, 3235, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 56, 3236, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 57, 2622, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 58, 1175, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 59, 2555, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 60, 2549, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 61, 2550, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 62, 3575, true, null, "BI_CARTEIRA_VN (l. 86–87)" },
                    { 63, 3576, true, null, "BI_CARTEIRA_VN (l. 86–87)" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "UX_ClassificacaoDeResultadoDoVortice_Codigo",
                schema: "integracao",
                table: "ClassificacaoDeResultadoDoVortice",
                column: "CodigoNaOrigem",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstagioDoProcesso_CarteiraId",
                schema: "processo",
                table: "EstagioDoProcesso",
                column: "CarteiraId");

            migrationBuilder.CreateIndex(
                name: "IX_EstagioDoProcesso_ClienteId",
                schema: "processo",
                table: "EstagioDoProcesso",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EstagioDoProcesso_EmpresaId_Estagio_AbertoEm",
                schema: "processo",
                table: "EstagioDoProcesso",
                columns: new[] { "EmpresaId", "Estagio", "AbertoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_EstagioDoProcesso_EmpresaId_Estagio_AlcancadoEm",
                schema: "processo",
                table: "EstagioDoProcesso",
                columns: new[] { "EmpresaId", "Estagio", "AlcancadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_EstagioDoProcesso_ProcessoId",
                schema: "processo",
                table: "EstagioDoProcesso",
                column: "ProcessoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstagioDoProcesso_ResponsavelId",
                schema: "processo",
                table: "EstagioDoProcesso",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "UX_EstagioDoProcesso_Numero_Estagio",
                schema: "processo",
                table: "EstagioDoProcesso",
                columns: new[] { "NumeroDoProcessoNaOrigem", "Estagio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassificacaoDeResultadoDoVortice",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "EstagioDoProcesso",
                schema: "processo");

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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
