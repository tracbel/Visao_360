using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class EstoqueDaGestaoDeNegocios : Migration
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
                name: "CoberturaDoEstoque",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Recorte = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Chave = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: true),
                    MesesDeEstoque = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Vendas = table.Column<int>(type: "int", nullable: false),
                    HashDaOrigem = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    GeradaNaOrigemEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoberturaDoEstoque", x => x.Id);
                    table.CheckConstraint("CK_CoberturaDoEstoque_Competencia", "([Recorte] = 'MES' AND [Competencia] IS NOT NULL AND DAY([Competencia]) = 1) OR ([Recorte] = 'GRUPO' AND [Competencia] IS NULL)");
                    table.CheckConstraint("CK_CoberturaDoEstoque_Recorte", "[Recorte] IN ('MES', 'GRUPO')");
                    table.CheckConstraint("CK_CoberturaDoEstoque_Valores", "[MesesDeEstoque] >= 0 AND [Vendas] >= 0");
                    table.ForeignKey(
                        name: "FK_CoberturaDoEstoque_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoberturaDoEstoque_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipamentoEmEstoque",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Chaint = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Chassi = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Pedido = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Comar = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Situacao = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Grupo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Configuracao = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EhUsado = table.Column<bool>(type: "bit", nullable: false),
                    AnoModelo = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: true),
                    EntradaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    ChegadaPrevistaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    FaturamentoPrevistoEm = table.Column<DateOnly>(type: "date", nullable: true),
                    Pago = table.Column<bool>(type: "bit", nullable: false),
                    Reservado = table.Column<bool>(type: "bit", nullable: false),
                    SituacaoNaFabrica = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    HashDaOrigem = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipamentoEmEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipamentoEmEstoque_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipamentoEmEstoque_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 12, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Intervalo", "ESTOQUE_GESTAO_NEGOCIOS", null, false, null, null, null, 60, null, "Estoque e cobertura (Gestão de Negócios)", null, null, null, null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CoberturaDoEstoque', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EquipamentoEmEstoque', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CoberturaDoEstoque', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EquipamentoEmEstoque', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_CoberturaDoEstoque_ImportadoPorId",
                schema: "frota",
                table: "CoberturaDoEstoque",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_CoberturaDoEstoque_Sistema_Recorte_Chave",
                schema: "frota",
                table: "CoberturaDoEstoque",
                columns: new[] { "SistemaId", "Recorte", "Chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipamentoEmEstoque_Empresa",
                schema: "frota",
                table: "EquipamentoEmEstoque",
                column: "EmpresaId",
                filter: "[ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "Situacao", "Grupo", "Reservado", "EntradaEm", "ChegadaPrevistaEm" });

            migrationBuilder.CreateIndex(
                name: "UX_EquipamentoEmEstoque_ChavePublica",
                schema: "frota",
                table: "EquipamentoEmEstoque",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_EquipamentoEmEstoque_Sistema_Chaint",
                schema: "frota",
                table: "EquipamentoEmEstoque",
                columns: new[] { "SistemaId", "Chaint" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // O QUE APONTA PARA A ROTINA 12 E PARA AS DUAS TABELAS SAI ANTES, como na da telemetria e na do planejamento: a
            // execução tem chave estrangeira para a rotina, e a checagem da lista de entidades volta sem os dois nomes.
            migrationBuilder.Sql("DELETE FROM [integracao].[ExecucaoDeRotina] WHERE [RotinaId] = 12;");
            migrationBuilder.Sql(
                "DELETE FROM [auditoria].[AlteracaoDeCampo] WHERE [Entidade] IN ('EquipamentoEmEstoque', 'CoberturaDoEstoque');");
            migrationBuilder.Sql(
                "DELETE FROM [integracao].[ChaveExterna] WHERE [Entidade] IN ('EquipamentoEmEstoque', 'CoberturaDoEstoque');");

            migrationBuilder.DropTable(
                name: "CoberturaDoEstoque",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "EquipamentoEmEstoque",
                schema: "frota");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
