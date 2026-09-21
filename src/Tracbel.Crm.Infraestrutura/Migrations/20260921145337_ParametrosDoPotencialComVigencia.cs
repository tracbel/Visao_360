using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// ISSUE 71 — os parâmetros do potencial passam a ter vigência, autor e justificativa.
    ///
    /// <para><b>A regra do café é preservada inteira.</b> <c>Origem</c> e <c>InformadaEm</c> são RENOMEADAS
    /// (<c>sp_rename</c>), não recriadas: o texto do gerente comercial vira a justificativa e 13/09/2026 vira o
    /// início da vigência.</para>
    ///
    /// <para><b>A única operação destrutiva é <c>DROP COLUMN EstaAtiva</c>, e ela não perde dado:</b> a tabela
    /// tem uma linha, ativa, e a revogação com data, autor e motivo substitui o liga-desliga. O manifesto do
    /// pacote a marca como destrutiva mesmo assim — é o portão fazendo o trabalho dele — e a publicação
    /// automática para e pede autorização.</para>
    /// </summary>
    public partial class ParametrosDoPotencialComVigencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegraDePotencial_ProdutoCodigoIbge",
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
                name: "EstaAtiva",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.RenameColumn(
                name: "Origem",
                schema: "organizacao",
                table: "RegraDePotencial",
                newName: "Justificativa");

            migrationBuilder.RenameColumn(
                name: "InformadaEm",
                schema: "organizacao",
                table: "RegraDePotencial",
                newName: "VigenteDesde");

            migrationBuilder.AddColumn<decimal>(
                name: "AnosDeRenovacao",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InformadoEm",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "datetime2(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "InformadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDaRevogacao",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevogadoEm",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RevogadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParametroDoPotencial",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MesesDaJanela = table.Column<short>(type: "smallint", nullable: false),
                    PesoDosContratosNoCredito = table.Column<decimal>(type: "decimal(4,3)", precision: 4, scale: 3, nullable: false),
                    LimiteDeRetracao = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    LimiteDeAquecimento = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    LimiteDeSuperaquecimento = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    NomeDaFaixaIntermediaria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LimiteDaPercepcao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDoIndicadorDePreco = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: true),
                    PesoDoIndicadorDeCredito = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: true),
                    PesoDoIndicadorComercial = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: true),
                    FatorMinimo = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: true),
                    FatorMaximo = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    Justificativa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    InformadoPorId = table.Column<long>(type: "bigint", nullable: true),
                    InformadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    RevogadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    RevogadoPorId = table.Column<long>(type: "bigint", nullable: true),
                    MotivoDaRevogacao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametroDoPotencial", x => x.Id);
                    table.CheckConstraint("CK_ParametroDoPotencial_Faixas", "[LimiteDeRetracao] > 0 AND [LimiteDeRetracao] < [LimiteDeAquecimento] AND [LimiteDeAquecimento] < [LimiteDeSuperaquecimento]");
                    table.CheckConstraint("CK_ParametroDoPotencial_Fator", "([FatorMinimo] IS NULL AND [FatorMaximo] IS NULL) OR ([FatorMinimo] > 0 AND [FatorMinimo] < 1 AND [FatorMaximo] > 1)");
                    table.CheckConstraint("CK_ParametroDoPotencial_Janela", "[MesesDaJanela] BETWEEN 1 AND 60");
                    table.CheckConstraint("CK_ParametroDoPotencial_Percepcao", "[LimiteDaPercepcao] > 0 AND [LimiteDaPercepcao] <= 50");
                    table.CheckConstraint("CK_ParametroDoPotencial_PesoDosContratos", "[PesoDosContratosNoCredito] BETWEEN 0 AND 1");
                    table.CheckConstraint("CK_ParametroDoPotencial_Revogacao", "([RevogadoEm] IS NULL AND [RevogadoPorId] IS NULL AND [MotivoDaRevogacao] IS NULL) OR ([RevogadoEm] IS NOT NULL AND [RevogadoPorId] IS NOT NULL AND [MotivoDaRevogacao] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ParametroDoPotencial_Usuario_InformadoPorId",
                        column: x => x.InformadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParametroDoPotencial_Usuario_RevogadoPorId",
                        column: x => x.RevogadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PercepcaoDoGestor",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    Percentual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    Justificativa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    InformadoPorId = table.Column<long>(type: "bigint", nullable: true),
                    InformadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    RevogadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    RevogadoPorId = table.Column<long>(type: "bigint", nullable: true),
                    MotivoDaRevogacao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PercepcaoDoGestor", x => x.Id);
                    table.CheckConstraint("CK_PercepcaoDoGestor_Percentual", "[Percentual] BETWEEN -50 AND 50");
                    table.CheckConstraint("CK_PercepcaoDoGestor_Revogacao", "([RevogadoEm] IS NULL AND [RevogadoPorId] IS NULL AND [MotivoDaRevogacao] IS NULL) OR ([RevogadoEm] IS NOT NULL AND [RevogadoPorId] IS NOT NULL AND [MotivoDaRevogacao] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PercepcaoDoGestor_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercepcaoDoGestor_Usuario_InformadoPorId",
                        column: x => x.InformadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercepcaoDoGestor_Usuario_RevogadoPorId",
                        column: x => x.RevogadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "ParametroDoPotencial",
                columns: new[] { "Id", "FatorMaximo", "FatorMinimo", "InformadoEm", "InformadoPorId", "Justificativa", "LimiteDaPercepcao", "LimiteDeAquecimento", "LimiteDeRetracao", "LimiteDeSuperaquecimento", "MesesDaJanela", "MotivoDaRevogacao", "NomeDaFaixaIntermediaria", "PesoDoIndicadorComercial", "PesoDoIndicadorDeCredito", "PesoDoIndicadorDePreco", "PesoDosContratosNoCredito", "RevogadoEm", "RevogadoPorId", "VigenteDesde" },
                values: new object[] { 1, null, null, new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, "Texto-base do Ricardo de 21/09/2026 (issue 63): os últimos 12 meses contra os 12 anteriores; crédito com 70% de contratos e 30% de valor; < 1 retraído, > 1,2 aquecido, > 1,4 super aquecido; percepção do gestor de −5% a +5% por município. Em aberto: pesos dos indicadores, limites do fator e o nome da faixa entre 1,0 e 1,2.", 5.00m, 1.20m, 1.00m, 1.40m, (short)12, null, null, null, null, null, 0.70m, null, null, new DateOnly(2026, 9, 21) });

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "Tudo o que o padrão dá, mais excluir, a visão entre filiais, a administração de perfis e usuários e os parâmetros do potencial.");

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "Perfil",
                columns: new[] { "Id", "Codigo", "Descricao", "EhPadrao", "EstaAtivo", "Nome" },
                values: new object[] { 5, "GESTOR_COMERCIAL", "Acrescenta informar a percepção do gestor sobre cada município, dentro do limite dos parâmetros gerais (issue 71, D-P04). Concedido a quem responde pelo território.", false, true, "Gestor comercial" });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[,]
                {
                    { 117, "ParametroDoPotencial.Ler", 1, "EmpresaEAbaixo" },
                    { 422, "ParametroDoPotencial.Ler", 4, "EmpresaEAbaixo" },
                    { 423, "ParametroDoPotencial.Administrar", 4, "Organizacao" },
                    { 424, "PercepcaoDoGestor.Informar", 4, "Organizacao" }
                });

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "RegraDePotencial",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AnosDeRenovacao", "InformadoEm", "InformadoPorId", "MotivoDaRevogacao", "RevogadoEm", "RevogadoPorId" },
                values: new object[] { null, new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[] { 501, "PercepcaoDoGestor.Informar", 5, "Organizacao" });

            migrationBuilder.CreateIndex(
                name: "IX_RegraDePotencial_InformadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "InformadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegraDePotencial_RevogadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "RevogadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_RegraDePotencial_Produto_Vigencia",
                schema: "organizacao",
                table: "RegraDePotencial",
                columns: new[] { "ProdutoCodigoIbge", "VigenteDesde" },
                unique: true,
                filter: "[RevogadoEm] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegraDePotencial_AnosDeRenovacao",
                schema: "organizacao",
                table: "RegraDePotencial",
                sql: "[AnosDeRenovacao] IS NULL OR [AnosDeRenovacao] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegraDePotencial_Revogacao",
                schema: "organizacao",
                table: "RegraDePotencial",
                sql: "([RevogadoEm] IS NULL AND [RevogadoPorId] IS NULL AND [MotivoDaRevogacao] IS NULL) OR ([RevogadoEm] IS NOT NULL AND [RevogadoPorId] IS NOT NULL AND [MotivoDaRevogacao] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_ParametroDoPotencial_InformadoPorId",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                column: "InformadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametroDoPotencial_RevogadoPorId",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                column: "RevogadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_ParametroDoPotencial_Vigencia",
                schema: "organizacao",
                table: "ParametroDoPotencial",
                column: "VigenteDesde",
                unique: true,
                filter: "[RevogadoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PercepcaoDoGestor_InformadoPorId",
                schema: "organizacao",
                table: "PercepcaoDoGestor",
                column: "InformadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PercepcaoDoGestor_RevogadoPorId",
                schema: "organizacao",
                table: "PercepcaoDoGestor",
                column: "RevogadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_PercepcaoDoGestor_Municipio_Vigencia",
                schema: "organizacao",
                table: "PercepcaoDoGestor",
                columns: new[] { "MunicipioId", "VigenteDesde" },
                unique: true,
                filter: "[RevogadoEm] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_RegraDePotencial_Usuario_InformadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "InformadoPorId",
                principalSchema: "seguranca",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegraDePotencial_Usuario_RevogadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "RevogadoPorId",
                principalSchema: "seguranca",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegraDePotencial_Usuario_InformadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropForeignKey(
                name: "FK_RegraDePotencial_Usuario_RevogadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropTable(
                name: "ParametroDoPotencial",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "PercepcaoDoGestor",
                schema: "organizacao");

            migrationBuilder.DropIndex(
                name: "IX_RegraDePotencial_InformadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropIndex(
                name: "IX_RegraDePotencial_RevogadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropIndex(
                name: "UX_RegraDePotencial_Produto_Vigencia",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegraDePotencial_AnosDeRenovacao",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegraDePotencial_Revogacao",
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

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 422);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 423);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 424);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "AnosDeRenovacao",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropColumn(
                name: "InformadoEm",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropColumn(
                name: "InformadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropColumn(
                name: "MotivoDaRevogacao",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropColumn(
                name: "RevogadoEm",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.DropColumn(
                name: "RevogadoPorId",
                schema: "organizacao",
                table: "RegraDePotencial");

            migrationBuilder.RenameColumn(
                name: "VigenteDesde",
                schema: "organizacao",
                table: "RegraDePotencial",
                newName: "InformadaEm");

            migrationBuilder.RenameColumn(
                name: "Justificativa",
                schema: "organizacao",
                table: "RegraDePotencial",
                newName: "Origem");

            migrationBuilder.AddColumn<bool>(
                name: "EstaAtiva",
                schema: "organizacao",
                table: "RegraDePotencial",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "seguranca",
                table: "Perfil",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descricao",
                value: "Tudo o que o padrão dá, mais excluir, a visão entre filiais e a administração de perfis e usuários.");

            migrationBuilder.UpdateData(
                schema: "organizacao",
                table: "RegraDePotencial",
                keyColumn: "Id",
                keyValue: 1,
                column: "EstaAtiva",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegraDePotencial_ProdutoCodigoIbge",
                schema: "organizacao",
                table: "RegraDePotencial",
                column: "ProdutoCodigoIbge",
                filter: "[EstaAtiva] = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Contato', 'CorrespondenciaDaOrigem', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");
        }
    }
}
