using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class IntegracoesConfiguraveis : Migration
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
                name: "Conexao",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1000, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    EhDoSistema = table.Column<bool>(type: "bit", nullable: false),
                    Endereco = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    Porta = table.Column<int>(type: "int", nullable: true),
                    Banco = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    Objeto = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    NomeDoCabecalho = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    SegredoProtegido = table.Column<byte[]>(type: "varbinary(4000)", maxLength: 4000, nullable: true),
                    SegredoAlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    SegredoAlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    StatusEsperado = table.Column<int>(type: "int", nullable: false),
                    MinutosEntreVerificacoes = table.Column<int>(type: "int", nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    UltimaVerificacaoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UltimaVerificacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UltimaVerificacaoResumo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conexao", x => x.Id);
                    table.CheckConstraint("CK_Conexao_MinutosEntreVerificacoes", "[MinutosEntreVerificacoes] IS NULL OR [MinutosEntreVerificacoes] BETWEEN 15 AND 1440");
                    table.CheckConstraint("CK_Conexao_StatusEsperado", "[StatusEsperado] BETWEEN 100 AND 599");
                    table.CheckConstraint("CK_Conexao_Tipo", "[Tipo] IN ('ApiRest','SqlServer','MySql','FontePublica','Monitorada')");
                    table.CheckConstraint("CK_Conexao_UltimaVerificacao", "[UltimaVerificacao] IN ('NuncaVerificada','NoAr','ComFalha')");
                    table.ForeignKey(
                        name: "FK_Conexao_Usuario_SegredoAlteradoPorId",
                        column: x => x.SegredoAlteradoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rotina",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Cadencia = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: true),
                    Dia = table.Column<int>(type: "int", nullable: true),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: true),
                    IntervaloMinutos = table.Column<int>(type: "int", nullable: true),
                    EstaLigada = table.Column<bool>(type: "bit", nullable: false),
                    AgendaVigenteDesde = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ExecucaoPedidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ExecucaoPedidaPorId = table.Column<long>(type: "bigint", nullable: true),
                    UltimaExecucaoIniciadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UltimaExecucaoTerminadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UltimoResultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    UltimaMensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rotina", x => x.Id);
                    table.CheckConstraint("CK_Rotina_Agenda", "([Mes] IS NULL OR [Mes] BETWEEN 1 AND 12) AND ([Dia] IS NULL OR [Dia] BETWEEN 1 AND 28) AND ([IntervaloMinutos] IS NULL OR [IntervaloMinutos] BETWEEN 15 AND 1440)");
                    table.CheckConstraint("CK_Rotina_Cadencia", "[Cadencia] IN ('Anual','Mensal','Diaria','Intervalo')");
                    table.CheckConstraint("CK_Rotina_UltimoResultado", "[UltimoResultado] IS NULL OR [UltimoResultado] IN ('EmAndamento','Sucesso','Falha','Ignorada')");
                    table.ForeignKey(
                        name: "FK_Rotina_Usuario_ExecucaoPedidaPorId",
                        column: x => x.ExecucaoPedidaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VerificacaoDeConexao",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConexaoId = table.Column<int>(type: "int", nullable: false),
                    VerificadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    VerificadaPorId = table.Column<long>(type: "bigint", nullable: true),
                    Ok = table.Column<bool>(type: "bit", nullable: false),
                    Resumo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    LatenciaMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacaoDeConexao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificacaoDeConexao_Conexao_ConexaoId",
                        column: x => x.ConexaoId,
                        principalSchema: "integracao",
                        principalTable: "Conexao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VerificacaoDeConexao_Usuario_VerificadaPorId",
                        column: x => x.VerificadaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExecucaoDeRotina",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RotinaId = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PedidaPorId = table.Column<long>(type: "bigint", nullable: true),
                    Maquina = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    IniciadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    TerminadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Resultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CodigoDeSaida = table.Column<int>(type: "int", nullable: true),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucaoDeRotina", x => x.Id);
                    table.CheckConstraint("CK_ExecucaoDeRotina_Motivo", "[Motivo] IN ('Agenda','Pedido','PrimeiraCarga')");
                    table.CheckConstraint("CK_ExecucaoDeRotina_Resultado", "[Resultado] IN ('EmAndamento','Sucesso','Falha','Ignorada')");
                    table.ForeignKey(
                        name: "FK_ExecucaoDeRotina_Rotina_RotinaId",
                        column: x => x.RotinaId,
                        principalSchema: "integracao",
                        principalTable: "Rotina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExecucaoDeRotina_Usuario_PedidaPorId",
                        column: x => x.PedidaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Conexao",
                columns: new[] { "Id", "Banco", "Codigo", "Descricao", "EhDoSistema", "Endereco", "EstaAtiva", "MinutosEntreVerificacoes", "Nome", "NomeDoCabecalho", "Objeto", "Porta", "SegredoAlteradoEm", "SegredoAlteradoPorId", "SegredoProtegido", "StatusEsperado", "Tipo", "UltimaVerificacao", "UltimaVerificacaoEm", "UltimaVerificacaoResumo", "Usuario" },
                values: new object[,]
                {
                    { 1, null, "PROTHEUS", "O faturamento (SD2) lido direto do ERP. Só leitura: o pedido de token é autenticação, não escrita.", true, null, true, null, "Protheus — API REST", null, null, null, null, null, null, 200, "ApiRest", "NuncaVerificada", null, null, null },
                    { 2, null, "PROTHEUS_BANCO", "O dono do chassi e o cadastro do comprador, para a integração do ART. Sessão somente leitura.", true, null, true, null, "Protheus — banco (leitura)", null, null, null, null, null, null, 200, "SqlServer", "NuncaVerificada", null, null, null },
                    { 3, null, "ART", "A view de vendas de máquina liberada para o CRM. Sessão somente leitura.", true, null, true, null, "ART — vendas de máquina", null, null, null, null, null, null, 200, "MySql", "NuncaVerificada", null, null, null },
                    { 4, null, "VORTICE", "A busca ao vivo no legado, congelado desde a fase 1 (somente referência).", true, null, true, null, "Vórtice — sistema legado", null, null, null, null, null, null, 200, "SqlServer", "NuncaVerificada", null, null, null },
                    { 5, null, "IBGE_SIDRA", "Produção agrícola, Censo Agropecuário, rebanho e área territorial.", true, "https://servicodados.ibge.gov.br/api/v3/agregados/5457/metadados", true, null, "IBGE — SIDRA", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 6, null, "IBGE_LOCALIDADES", "O catálogo oficial de municípios e o contorno de cada um.", true, "https://servicodados.ibge.gov.br/api/v1/localidades/municipios?view=nivelado", true, null, "IBGE — municípios e malhas", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 7, null, "ANP", "As usinas autorizadas, com capacidade de produção.", true, "https://www.gov.br/anp/pt-br/assuntos/producao-e-fornecimento-de-biocombustiveis/etanol/arquivos-etanol/pb-da-etanol.zip", true, null, "ANP — usinas de etanol", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 8, null, "CONAB_PRECOS", "O preço mensal das culturas em SP.", true, "https://portaldeinformacoes.conab.gov.br/downloads/arquivos/PrecosMensalUF.txt", true, null, "CONAB — preço recebido", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 9, null, "CONAB_CUSTOS", "As séries de custo de produção por cultura.", true, "https://www.gov.br/conab/pt-br/atuacao/informacoes-agropecuarias/custos-de-producao/planilhas-de-custos-de-producao/copy_of_agricolas", true, null, "CONAB — custo de produção", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 10, null, "SOCICANA", "O preço do kg de ATR da cana.", true, "https://www.socicana.com.br/calculadora-de-atr/preco-do-kg/", true, null, "Socicana — preço do ATR", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 11, null, "BCB_PTAX", "A média mensal do dólar de venda.", true, "https://api.bcb.gov.br/dados/serie/bcdata.sgs.3698/dados?formato=json", true, null, "Banco Central — dólar PTAX", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null },
                    { 12, null, "BCB_SICOR", "O crédito rural de investimento por município.", true, "https://olinda.bcb.gov.br/olinda/servico/SICOR/versao/v2/odata/InvestMunicipioProduto", true, null, "Banco Central — SICOR", null, null, null, null, null, null, 200, "FontePublica", "NuncaVerificada", null, null, null }
                });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[] { 427, "Integracao.Administrar", 4, "Organizacao" });

            migrationBuilder.InsertData(
                schema: "integracao",
                table: "Rotina",
                columns: new[] { "Id", "AgendaVigenteDesde", "Cadencia", "Codigo", "Dia", "EstaLigada", "ExecucaoPedidaEm", "ExecucaoPedidaPorId", "Hora", "IntervaloMinutos", "Mes", "Nome", "UltimaExecucaoIniciadaEm", "UltimaExecucaoTerminadaEm", "UltimaMensagem", "UltimoResultado" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), "Anual", "FONTES_ANUAIS", 1, true, null, null, new TimeOnly(3, 0, 0), null, 10, "Fontes públicas anuais", null, null, null, null },
                    { 2, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), "Mensal", "PRECOS_MENSAIS", 20, true, null, null, new TimeOnly(4, 0, 0), null, null, "Preços, custos e crédito", null, null, null, null },
                    { 3, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), "Diaria", "FATURAMENTO_PROTHEUS", null, false, null, null, new TimeOnly(5, 0, 0), null, null, "Faturamento do Protheus", null, null, null, null },
                    { 4, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Utc), "Intervalo", "ART_VENDAS", null, false, null, null, null, 60, null, "Vendas de máquina do ART", null, null, null, null }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Conexao_SegredoAlteradoPorId",
                schema: "integracao",
                table: "Conexao",
                column: "SegredoAlteradoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_Conexao_Codigo",
                schema: "integracao",
                table: "Conexao",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExecucaoDeRotina_PedidaPorId",
                schema: "integracao",
                table: "ExecucaoDeRotina",
                column: "PedidaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucaoDeRotina_RotinaId_IniciadaEm",
                schema: "integracao",
                table: "ExecucaoDeRotina",
                columns: new[] { "RotinaId", "IniciadaEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Rotina_ExecucaoPedidaPorId",
                schema: "integracao",
                table: "Rotina",
                column: "ExecucaoPedidaPorId");

            migrationBuilder.CreateIndex(
                name: "UX_Rotina_Codigo",
                schema: "integracao",
                table: "Rotina",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificacaoDeConexao_ConexaoId_VerificadaEm",
                schema: "integracao",
                table: "VerificacaoDeConexao",
                columns: new[] { "ConexaoId", "VerificadaEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_VerificacaoDeConexao_VerificadaPorId",
                schema: "integracao",
                table: "VerificacaoDeConexao",
                column: "VerificadaPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecucaoDeRotina",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "VerificacaoDeConexao",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "Rotina",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "Conexao",
                schema: "integracao");

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
                keyValue: 427);

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
        }
    }
}
