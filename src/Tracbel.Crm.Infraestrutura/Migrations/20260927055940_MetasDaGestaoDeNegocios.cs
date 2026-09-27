using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class MetasDaGestaoDeNegocios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Conexao_Tipo",
                schema: "integracao",
                table: "Conexao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.AddColumn<string>(
                name: "VendedorNaOrigem",
                schema: "frota",
                table: "VendaDeMaquina",
                type: "varchar(80)",
                unicode: false,
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetaDeVenda",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    IdNaOrigem = table.Column<int>(type: "int", nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    LinhaNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CodigoDaLinha = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    LinhaDeProdutoId = table.Column<int>(type: "int", nullable: true),
                    ConsultorNaOrigem = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    ConsultorUsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    VendaDireta = table.Column<bool>(type: "bit", nullable: false),
                    Origem = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Margem = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    HashDaOrigem = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ImportadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    AtualizadaPelaOrigemEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    GeradaNaOrigemEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    IdadeNaOrigemSegundos = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_MetaDeVenda", x => x.Id);
                    table.CheckConstraint("CK_MetaDeVenda_Competencia", "DAY([Competencia]) = 1");
                    table.CheckConstraint("CK_MetaDeVenda_IdNaOrigem", "[IdNaOrigem] > 0");
                    table.CheckConstraint("CK_MetaDeVenda_Origem", "[Origem] IN ('Campanha','Consorcio')");
                    table.CheckConstraint("CK_MetaDeVenda_Quantidade", "[Quantidade] >= 0");
                    table.ForeignKey(
                        name: "FK_MetaDeVenda_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaDeVenda_LinhaDeProduto_LinhaDeProdutoId",
                        column: x => x.LinhaDeProdutoId,
                        principalSchema: "frota",
                        principalTable: "LinhaDeProduto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaDeVenda_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaDeVenda_Usuario_ConsultorUsuarioId",
                        column: x => x.ConsultorUsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Conexao",
                columns: new[] { "Id", "Banco", "Codigo", "Descricao", "EhDoSistema", "Endereco", "EstaAtiva", "MinutosEntreVerificacoes", "Nome", "NomeDoCabecalho", "Objeto", "Porta", "SegredoAlteradoEm", "SegredoAlteradoPorId", "SegredoProtegido", "StatusEsperado", "Tipo", "UltimaVerificacao", "UltimaVerificacaoEm", "UltimaVerificacaoResumo", "Usuario" },
                values: new object[] { 13, null, "GESTAO_NEGOCIOS", "O cadastro de metas de venda (unidades por consultor, linha, mês e filial) da API da Inteligência de Mercado. Só leitura, com a chave no cabeçalho Bearer.", true, null, true, null, "Gestão de Negócios — API", null, null, null, null, null, null, 200, "ApiComChave", "NuncaVerificada", null, null, null });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[,]
                {
                    { 118, "Meta.Ler", 1, "Proprios" },
                    { 428, "Meta.Ler", 4, "Organizacao" },
                    { 604, "Meta.Ler", 6, "EmpresaEAbaixo" },
                    { 706, "Meta.Ler", 7, "EmpresaEAbaixo" }
                });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "AnoInicialDoHistorico", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[] { 8, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), null, "Diaria", "METAS_GESTAO_NEGOCIOS", null, false, null, null, new TimeOnly(6, 0, 0), null, null, "Metas de venda (Gestão de Negócios)", null, null, null, null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Conexao_Tipo",
                schema: "integracao",
                table: "Conexao",
                sql: "[Tipo] IN ('ApiRest','SqlServer','MySql','FontePublica','Monitorada','ApiComChave')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'CategoriaDeMaquina', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'Conexao', 'Contato', 'CorrespondenciaDaOrigem', 'CorrespondenciaDeMunicipio', 'CotacaoDeProduto', 'CotacaoDoDolar', 'CreditoRuralDeInvestimento', 'Cultura', 'CulturaNoGrupoDeCompartilhamento', 'CustoDeProducao', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeRotina', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'GrupoDeCompartilhamento', 'Interacao', 'ItemDoSicor', 'LinhaDeNegocio', 'LinhaDeProduto', 'LinhaDeProdutoNaCategoria', 'Marca', 'MedidaDoIbgeNoEstado', 'MensagemDescartada', 'MetaDeVenda', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'ParametroDoPotencial', 'PercepcaoDoGestor', 'Perfil', 'PerfilPermissao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'ProducaoDeMilhoPorSafraNoMunicipio', 'ProdutoDaPamNaCultura', 'ProdutoDoSicorNaCategoria', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Rotina', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioPerfil', 'VendaDeMaquina', 'VendaPerdida', 'VerificacaoDeConexao', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_MetaDeVenda_Consultor_Competencia",
                schema: "organizacao",
                table: "MetaDeVenda",
                columns: new[] { "ConsultorUsuarioId", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_MetaDeVenda_Empresa_Competencia",
                schema: "organizacao",
                table: "MetaDeVenda",
                columns: new[] { "EmpresaId", "Competencia" },
                filter: "[ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "CodigoDaLinha", "ConsultorNaOrigem", "ConsultorUsuarioId", "VendaDireta", "Origem", "Quantidade" });

            migrationBuilder.CreateIndex(
                name: "IX_MetaDeVenda_LinhaDeProdutoId",
                schema: "organizacao",
                table: "MetaDeVenda",
                column: "LinhaDeProdutoId");

            migrationBuilder.CreateIndex(
                name: "UX_MetaDeVenda_ChavePublica",
                schema: "organizacao",
                table: "MetaDeVenda",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MetaDeVenda_Sistema_IdNaOrigem",
                schema: "organizacao",
                table: "MetaDeVenda",
                columns: new[] { "SistemaId", "IdNaOrigem" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaDeVenda",
                schema: "organizacao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Conexao_Tipo",
                schema: "integracao",
                table: "Conexao");

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
                table: "Conexao",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 428);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 604);

            migrationBuilder.DeleteData(
                schema: "seguranca",
                table: "PerfilPermissao",
                keyColumn: "Id",
                keyValue: 706);

            migrationBuilder.DeleteData(
                schema: "integracao",
                table: "Rotina",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "VendedorNaOrigem",
                schema: "frota",
                table: "VendaDeMaquina");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Conexao_Tipo",
                schema: "integracao",
                table: "Conexao",
                sql: "[Tipo] IN ('ApiRest','SqlServer','MySql','FontePublica','Monitorada')");

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
