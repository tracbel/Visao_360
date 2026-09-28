using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class PlanejamentoDaGestaoDeNegocios : Migration
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
                name: "CotaDeConsorcioVendida",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Grupo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Cota = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    AlocadaEm = table.Column<DateOnly>(type: "date", nullable: false),
                    Contemplacao = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContempladaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    ConsultorNaOrigem = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    ConsultorUsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    GestorNaOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Produto = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ValorDoBem = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ValorDaParcela = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_CotaDeConsorcioVendida", x => x.Id);
                    table.CheckConstraint("CK_CotaDeConsorcioVendida_Competencia", "DAY([Competencia]) = 1");
                    table.CheckConstraint("CK_CotaDeConsorcioVendida_Valores", "([ValorDoBem] IS NULL OR [ValorDoBem] >= 0) AND ([ValorDaParcela] IS NULL OR [ValorDaParcela] >= 0)");
                    table.ForeignKey(
                        name: "FK_CotaDeConsorcioVendida_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotaDeConsorcioVendida_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotaDeConsorcioVendida_Usuario_ConsultorUsuarioId",
                        column: x => x.ConsultorUsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForecastDaGerencia",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    IdNaOrigem = table.Column<int>(type: "int", nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    GestorNaOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LinhaNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CodigoDaLinha = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Forecast = table.Column<int>(type: "int", nullable: true),
                    BestGuess = table.Column<int>(type: "int", nullable: true),
                    HashDaOrigem = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    GeradaNaOrigemEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastDaGerencia", x => x.Id);
                    table.CheckConstraint("CK_ForecastDaGerencia_Competencia", "DAY([Competencia]) = 1");
                    table.CheckConstraint("CK_ForecastDaGerencia_IdNaOrigem", "[IdNaOrigem] > 0");
                    table.CheckConstraint("CK_ForecastDaGerencia_Quantidades", "([Forecast] IS NULL OR [Forecast] BETWEEN 0 AND 10000) AND ([BestGuess] IS NULL OR [BestGuess] BETWEEN 0 AND 10000)");
                    table.ForeignKey(
                        name: "FK_ForecastDaGerencia_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastDaGerencia_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GestorDoConsultor",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    IdNaOrigem = table.Column<int>(type: "int", nullable: false),
                    ConsultorNaOrigem = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    GestorNaOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FilialNumero = table.Column<int>(type: "int", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: true),
                    HashDaOrigem = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestorDoConsultor", x => x.Id);
                    table.CheckConstraint("CK_GestorDoConsultor_IdNaOrigem", "[IdNaOrigem] > 0");
                    table.ForeignKey(
                        name: "FK_GestorDoConsultor_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GestorDoConsultor_Usuario_ImportadoPorId",
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_CotaDeConsorcioVendida_ConsultorUsuarioId",
                schema: "organizacao",
                table: "CotaDeConsorcioVendida",
                column: "ConsultorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CotaDeConsorcioVendida_Empresa_Competencia",
                schema: "organizacao",
                table: "CotaDeConsorcioVendida",
                columns: new[] { "EmpresaId", "Competencia" },
                filter: "[ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "ConsultorNaOrigem", "ConsultorUsuarioId", "GestorNaOrigem" });

            migrationBuilder.CreateIndex(
                name: "UX_CotaDeConsorcioVendida_ChavePublica",
                schema: "organizacao",
                table: "CotaDeConsorcioVendida",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CotaDeConsorcioVendida_Sistema_Grupo_Cota",
                schema: "organizacao",
                table: "CotaDeConsorcioVendida",
                columns: new[] { "SistemaId", "Grupo", "Cota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDaGerencia_Competencia",
                schema: "organizacao",
                table: "ForecastDaGerencia",
                column: "Competencia",
                filter: "[ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "GestorNaOrigem", "CodigoDaLinha", "Forecast", "BestGuess" });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDaGerencia_ImportadoPorId",
                schema: "organizacao",
                table: "ForecastDaGerencia",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_ForecastDaGerencia_Sistema_IdNaOrigem",
                schema: "organizacao",
                table: "ForecastDaGerencia",
                columns: new[] { "SistemaId", "IdNaOrigem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GestorDoConsultor_Consultor",
                schema: "organizacao",
                table: "GestorDoConsultor",
                column: "ConsultorNaOrigem",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GestorDoConsultor_ImportadoPorId",
                schema: "organizacao",
                table: "GestorDoConsultor",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_GestorDoConsultor_Sistema_IdNaOrigem",
                schema: "organizacao",
                table: "GestorDoConsultor",
                columns: new[] { "SistemaId", "IdNaOrigem" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A TRILHA E AS CHAVES EXTERNAS DAS TRÊS SAEM ANTES, como na da percepção: a checagem da lista de entidades volta
            // sem os três nomes, e falharia com a linha deles ainda lá.
            migrationBuilder.Sql(
                "DELETE FROM [auditoria].[AlteracaoDeCampo] WHERE [Entidade] IN ('GestorDoConsultor', 'ForecastDaGerencia', 'CotaDeConsorcioVendida');");
            migrationBuilder.Sql(
                "DELETE FROM [integracao].[ChaveExterna] WHERE [Entidade] IN ('GestorDoConsultor', 'ForecastDaGerencia', 'CotaDeConsorcioVendida');");

            migrationBuilder.DropTable(
                name: "CotaDeConsorcioVendida",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "ForecastDaGerencia",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "GestorDoConsultor",
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
