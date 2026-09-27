using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class ParametrosDoPlanejamento : Migration
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
                name: "ParametroDoPlanejamento",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SazonalidadeJaneiro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeFevereiro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeMarco = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeAbril = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeMaio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeJunho = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeJulho = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeAgosto = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeSetembro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeOutubro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeNovembro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SazonalidadeDezembro = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDoPotencial = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDaCobertura = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDoCredito = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDaRentabilidade = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDosClientes = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDaRealizacao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PesoDaPenetracao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ParametroDoPlanejamento", x => x.Id);
                    table.CheckConstraint("CK_ParametroDoPlanejamento_Faixa", "[SazonalidadeJaneiro] + 0 BETWEEN 0 AND 100 AND [SazonalidadeFevereiro] + 0 BETWEEN 0 AND 100 AND [SazonalidadeMarco] + 0 BETWEEN 0 AND 100 AND [SazonalidadeAbril] + 0 BETWEEN 0 AND 100 AND [SazonalidadeMaio] + 0 BETWEEN 0 AND 100 AND [SazonalidadeJunho] + 0 BETWEEN 0 AND 100 AND [SazonalidadeJulho] + 0 BETWEEN 0 AND 100 AND [SazonalidadeAgosto] + 0 BETWEEN 0 AND 100 AND [SazonalidadeSetembro] + 0 BETWEEN 0 AND 100 AND [SazonalidadeOutubro] + 0 BETWEEN 0 AND 100 AND [SazonalidadeNovembro] + 0 BETWEEN 0 AND 100 AND [SazonalidadeDezembro] + 0 BETWEEN 0 AND 100 AND [PesoDoPotencial] + 0 BETWEEN 0 AND 100 AND [PesoDaCobertura] + 0 BETWEEN 0 AND 100 AND [PesoDoCredito] + 0 BETWEEN 0 AND 100 AND [PesoDaRentabilidade] + 0 BETWEEN 0 AND 100 AND [PesoDosClientes] + 0 BETWEEN 0 AND 100 AND [PesoDaRealizacao] + 0 BETWEEN 0 AND 100 AND [PesoDaPenetracao] + 0 BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_ParametroDoPlanejamento_Pesos", "([PesoDoPotencial] + [PesoDaCobertura] + [PesoDoCredito] + [PesoDaRentabilidade] + [PesoDosClientes] + [PesoDaRealizacao] + [PesoDaPenetracao]) > 0");
                    table.CheckConstraint("CK_ParametroDoPlanejamento_Revogacao", "([RevogadoEm] IS NULL AND [RevogadoPorId] IS NULL AND [MotivoDaRevogacao] IS NULL) OR ([RevogadoEm] IS NOT NULL AND [RevogadoPorId] IS NOT NULL AND [MotivoDaRevogacao] IS NOT NULL)");
                    table.CheckConstraint("CK_ParametroDoPlanejamento_Sazonalidade", "([SazonalidadeJaneiro] + [SazonalidadeFevereiro] + [SazonalidadeMarco] + [SazonalidadeAbril] + [SazonalidadeMaio] + [SazonalidadeJunho] + [SazonalidadeJulho] + [SazonalidadeAgosto] + [SazonalidadeSetembro] + [SazonalidadeOutubro] + [SazonalidadeNovembro] + [SazonalidadeDezembro]) BETWEEN 99.95 AND 100.05");
                    table.ForeignKey(
                        name: "FK_ParametroDoPlanejamento_Usuario_InformadoPorId",
                        column: x => x.InformadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParametroDoPlanejamento_Usuario_RevogadoPorId",
                        column: x => x.RevogadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShareAlvoDaCategoria",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaDeMaquinaId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ShareAlvoDaCategoria", x => x.Id);
                    table.CheckConstraint("CK_ShareAlvoDaCategoria_Percentual", "[Percentual] + 0 > 0 AND [Percentual] + 0 <= 100");
                    table.CheckConstraint("CK_ShareAlvoDaCategoria_Revogacao", "([RevogadoEm] IS NULL AND [RevogadoPorId] IS NULL AND [MotivoDaRevogacao] IS NULL) OR ([RevogadoEm] IS NOT NULL AND [RevogadoPorId] IS NOT NULL AND [MotivoDaRevogacao] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ShareAlvoDaCategoria_CategoriaDeMaquina_CategoriaDeMaquinaId",
                        column: x => x.CategoriaDeMaquinaId,
                        principalSchema: "organizacao",
                        principalTable: "CategoriaDeMaquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShareAlvoDaCategoria_Usuario_InformadoPorId",
                        column: x => x.InformadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShareAlvoDaCategoria_Usuario_RevogadoPorId",
                        column: x => x.RevogadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "ParametroDoPlanejamento",
                columns: new[] { "Id", "InformadoEm", "InformadoPorId", "Justificativa", "MotivoDaRevogacao", "PesoDaCobertura", "PesoDaPenetracao", "PesoDaRealizacao", "PesoDaRentabilidade", "PesoDoCredito", "PesoDoPotencial", "PesoDosClientes", "RevogadoEm", "RevogadoPorId", "SazonalidadeAbril", "SazonalidadeAgosto", "SazonalidadeDezembro", "SazonalidadeFevereiro", "SazonalidadeJaneiro", "SazonalidadeJulho", "SazonalidadeJunho", "SazonalidadeMaio", "SazonalidadeMarco", "SazonalidadeNovembro", "SazonalidadeOutubro", "SazonalidadeSetembro", "VigenteDesde" },
                values: new object[] { 1, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Valores do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar: a sazonalidade mensal dele (6,52% em janeiro a 10,51% em outubro) e os pesos do IOC — 25 potencial, 20 cobertura, 15 crédito, 15 rentabilidade, 10 clientes, 5 realização e 10 penetração. Decisão do Ricardo de 27/09/2026 (issue 256).", null, 20m, 10m, 5m, 15m, 15m, 25m, 10m, null, null, 8.59m, 8.75m, 8.30m, 6.89m, 6.52m, 7.84m, 8.58m, 8.87m, 8.56m, 7.44m, 10.51m, 9.15m, new DateOnly(2026, 9, 27) });

            migrationBuilder.InsertData(
                schema: "organizacao",
                table: "ShareAlvoDaCategoria",
                columns: new[] { "Id", "CategoriaDeMaquinaId", "InformadoEm", "InformadoPorId", "Justificativa", "MotivoDaRevogacao", "Percentual", "RevogadoEm", "RevogadoPorId", "VigenteDesde" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Share-alvo padrão do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar. Trazido por decisão do Ricardo de 27/09/2026 (issue 256).", null, 31m, null, null, new DateOnly(2026, 9, 27) },
                    { 2, 2, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Share-alvo padrão do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar. Trazido por decisão do Ricardo de 27/09/2026 (issue 256).", null, 31m, null, null, new DateOnly(2026, 9, 27) },
                    { 3, 3, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Share-alvo padrão do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar. Trazido por decisão do Ricardo de 27/09/2026 (issue 256).", null, 31m, null, null, new DateOnly(2026, 9, 27) },
                    { 4, 4, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Share-alvo padrão do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar. Trazido por decisão do Ricardo de 27/09/2026 (issue 256).", null, 31m, null, null, new DateOnly(2026, 9, 27) },
                    { 5, 7, new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, "Share-alvo padrão do protótipo da pasta 360 (Plataforma Inteligência Agro), a confirmar. Trazido por decisão do Ricardo de 27/09/2026 (issue 256).", null, 31m, null, null, new DateOnly(2026, 9, 27) }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_ParametroDoPlanejamento_InformadoPorId",
                schema: "organizacao",
                table: "ParametroDoPlanejamento",
                column: "InformadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametroDoPlanejamento_RevogadoPorId",
                schema: "organizacao",
                table: "ParametroDoPlanejamento",
                column: "RevogadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_ParametroDoPlanejamento_Vigencia",
                schema: "organizacao",
                table: "ParametroDoPlanejamento",
                column: "VigenteDesde",
                unique: true,
                filter: "[RevogadoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShareAlvoDaCategoria_InformadoPorId",
                schema: "organizacao",
                table: "ShareAlvoDaCategoria",
                column: "InformadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareAlvoDaCategoria_RevogadoPorId",
                schema: "organizacao",
                table: "ShareAlvoDaCategoria",
                column: "RevogadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_ShareAlvoDaCategoria_Categoria_Vigencia",
                schema: "organizacao",
                table: "ShareAlvoDaCategoria",
                columns: new[] { "CategoriaDeMaquinaId", "VigenteDesde" },
                unique: true,
                filter: "[RevogadoEm] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParametroDoPlanejamento",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "ShareAlvoDaCategoria",
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
