using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class PecasDoProtheus : Migration
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
                name: "FaturamentoDePecasNoMes",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    Setor = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Grupo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Linha = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    VendedorCodigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    VendedorNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Quantidade = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ValorLiquido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDeDevolucoes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDeDesconto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDeTabela = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Itens = table.Column<int>(type: "int", nullable: false),
                    ApuradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
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
                    table.PrimaryKey("PK_FaturamentoDePecasNoMes", x => x.Id);
                    table.CheckConstraint("CK_FaturamentoDePecasNoMes_Competencia", "DAY([Competencia]) = 1");
                    table.CheckConstraint("CK_FaturamentoDePecasNoMes_Itens", "[Itens] > 0");
                    table.ForeignKey(
                        name: "FK_FaturamentoDePecasNoMes_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturamentoDePecasNoMes_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturamentoDePecasNoMes_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrcamentoDePecas",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    ChaveNaOrigem = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Numero = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    Situacao = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Prazo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Reserva = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TipoDeAtendimento = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TipoDeOrcamento = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    OrcadoEm = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidoAte = table.Column<DateOnly>(type: "date", nullable: true),
                    AlteradoNaOrigemEm = table.Column<DateOnly>(type: "date", nullable: true),
                    VendedorCodigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    VendedorNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDeDesconto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Itens = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_OrcamentoDePecas", x => x.Id);
                    table.CheckConstraint("CK_OrcamentoDePecas_Itens", "[Itens] > 0");
                    table.ForeignKey(
                        name: "FK_OrcamentoDePecas_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrcamentoDePecas_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrcamentoDePecas_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CoberturaDoEstoque', 'CompradorPendente', 'Conexao', 'ConferenciaDaGestaoDeNegocios', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EquipamentoEmEstoque', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDePecasNoMes', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FinanciamentoDaVenda', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'OrcamentoDePecas', 'OrdemDeServico', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'UtilizacaoDasTerrasNoMunicipio', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CoberturaDoEstoque', 'CompradorPendente', 'Conexao', 'ConferenciaDaGestaoDeNegocios', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EquipamentoEmEstoque', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDePecasNoMes', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FinanciamentoDaVenda', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'OrcamentoDePecas', 'OrdemDeServico', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'UtilizacaoDasTerrasNoMunicipio', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentoDePecasNoMes_Cliente",
                schema: "comercial",
                table: "FaturamentoDePecasNoMes",
                columns: new[] { "ClienteId", "Competencia" },
                filter: "[ClienteId] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "Setor", "Grupo", "ValorLiquido" });

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentoDePecasNoMes_Empresa",
                schema: "comercial",
                table: "FaturamentoDePecasNoMes",
                columns: new[] { "EmpresaId", "Competencia" })
                .Annotation("SqlServer:Include", new[] { "Setor", "Grupo", "ValorLiquido" });

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentoDePecasNoMes_Sistema_Competencia",
                schema: "comercial",
                table: "FaturamentoDePecasNoMes",
                columns: new[] { "SistemaId", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "UX_FaturamentoDePecasNoMes_ChavePublica",
                schema: "comercial",
                table: "FaturamentoDePecasNoMes",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoDePecas_Cliente",
                schema: "comercial",
                table: "OrcamentoDePecas",
                column: "ClienteId",
                filter: "[ClienteId] IS NOT NULL AND [ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "Situacao", "OrcadoEm", "ValidoAte", "ValorTotal" });

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoDePecas_Empresa_Situacao",
                schema: "comercial",
                table: "OrcamentoDePecas",
                columns: new[] { "EmpresaId", "Situacao" },
                filter: "[ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "OrcadoEm", "ValorTotal" });

            migrationBuilder.CreateIndex(
                name: "UX_OrcamentoDePecas_ChavePublica",
                schema: "comercial",
                table: "OrcamentoDePecas",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OrcamentoDePecas_Sistema_Chave",
                schema: "comercial",
                table: "OrcamentoDePecas",
                columns: new[] { "SistemaId", "ChaveNaOrigem" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // O QUE APONTA PARA AS DUAS TABELAS SAI ANTES, como na das ordens de serviço: a checagem da lista de entidades volta
            // sem os dois nomes.
            migrationBuilder.Sql("DELETE FROM [auditoria].[AlteracaoDeCampo] WHERE [Entidade] IN ('OrcamentoDePecas', 'FaturamentoDePecasNoMes');");
            migrationBuilder.Sql("DELETE FROM [integracao].[ChaveExterna] WHERE [Entidade] IN ('OrcamentoDePecas', 'FaturamentoDePecasNoMes');");

            migrationBuilder.DropTable(
                name: "FaturamentoDePecasNoMes",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "OrcamentoDePecas",
                schema: "comercial");

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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CoberturaDoEstoque', 'CompradorPendente', 'Conexao', 'ConferenciaDaGestaoDeNegocios', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EquipamentoEmEstoque', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FinanciamentoDaVenda', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'OrdemDeServico', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'UtilizacaoDasTerrasNoMunicipio', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'ClassificacaoDeResultadoDoVortice', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CoberturaDoEstoque', 'CompradorPendente', 'Conexao', 'ConferenciaDaGestaoDeNegocios', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotaDeConsorcioVendida', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EquipamentoEmEstoque', 'EstabelecimentosPorAreaNoMunicipio', 'EstagioDoProcesso', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FinanciamentoDaVenda', 'ForecastDaGerencia', 'FrotaDeTratoresNoMunicipio', 'GestorDoConsultor', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'OrdemDeServico', 'ParametroDoPlanejamento', 'ParametroDoPotencial', 'PercepcaoDaCultura', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'PrecoDeMaquinaNoMes', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'ShareAlvoDaCategoria', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'UtilizacaoDasTerrasNoMunicipio', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");
        }
    }
}
