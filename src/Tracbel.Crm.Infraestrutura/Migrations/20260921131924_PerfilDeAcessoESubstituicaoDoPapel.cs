using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class PerfilDeAcessoESubstituicaoDoPapel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConjuntoDePermissaoItem",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "UsuarioConjuntoDePermissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "ConjuntoDePermissao",
                schema: "seguranca");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "Papel",
                schema: "seguranca",
                table: "Usuario");

            migrationBuilder.CreateTable(
                name: "Perfil",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    EhPadrao = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfil", x => x.Id);
                    table.CheckConstraint("CK_Perfil_PadraoAtivo", "[EhPadrao] = 0 OR [EstaAtivo] = 1");
                });

            migrationBuilder.CreateTable(
                name: "PerfilPermissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    CodigoPermissao = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Profundidade = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilPermissao", x => x.Id);
                    table.CheckConstraint("CK_PerfilPermissao_Profundidade", "[Profundidade] IN ('Proprios','Equipe','Empresa','EmpresaEAbaixo','Organizacao')");
                    table.ForeignKey(
                        name: "FK_PerfilPermissao_Perfil_PerfilId",
                        column: x => x.PerfilId,
                        principalSchema: "seguranca",
                        principalTable: "Perfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPerfil",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    Justificativa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ConcedidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ConcedidoPorId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPerfil", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Perfil_PerfilId",
                        column: x => x.PerfilId,
                        principalSchema: "seguranca",
                        principalTable: "Perfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Usuario_ConcedidoPorId",
                        column: x => x.ConcedidoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "Perfil",
                columns: new[] { "Id", "Codigo", "Descricao", "EhPadrao", "EstaAtivo", "Nome" },
                values: new object[,]
                {
                    { 1, "PADRAO", "O que todo usuário recebe: ler as telas e cadastrar e alterar cliente e equipamento. Sem excluir e sem visão entre filiais.", true, true, "Padrão" },
                    { 2, "EXCLUSAO_DE_CADASTRO", "Acrescenta excluir cliente e dar baixa em equipamento.", false, true, "Exclusão de cadastro" },
                    { 3, "VISAO_ENTRE_FILIAIS", "Permite abrir, com motivo registrado, o alcance entre filiais (visão da empresa).", false, true, "Visão entre filiais" },
                    { 4, "ADMINISTRADOR", "Tudo o que o padrão dá, mais excluir, a visão entre filiais e a administração de perfis e usuários.", false, true, "Administrador" }
                });

            migrationBuilder.InsertData(
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "Id", "CodigoPermissao", "PerfilId", "Profundidade" },
                values: new object[,]
                {
                    { 101, "Cliente.Ler", 1, "EmpresaEAbaixo" },
                    { 102, "Equipamento.Ler", 1, "EmpresaEAbaixo" },
                    { 103, "Catalogo.Ler", 1, "EmpresaEAbaixo" },
                    { 104, "Processo.Ler", 1, "EmpresaEAbaixo" },
                    { 105, "Tarefa.Ler", 1, "EmpresaEAbaixo" },
                    { 106, "Interacao.Ler", 1, "EmpresaEAbaixo" },
                    { 107, "Cobertura.Ler", 1, "EmpresaEAbaixo" },
                    { 108, "Relatorio.Ler", 1, "EmpresaEAbaixo" },
                    { 109, "Faturamento.Ler", 1, "EmpresaEAbaixo" },
                    { 110, "Territorio.Ler", 1, "EmpresaEAbaixo" },
                    { 111, "Integracao.Ler", 1, "EmpresaEAbaixo" },
                    { 112, "Legado.Ler", 1, "EmpresaEAbaixo" },
                    { 113, "Cliente.Criar", 1, "EmpresaEAbaixo" },
                    { 114, "Cliente.Editar", 1, "EmpresaEAbaixo" },
                    { 115, "Equipamento.Criar", 1, "EmpresaEAbaixo" },
                    { 116, "Equipamento.Editar", 1, "EmpresaEAbaixo" },
                    { 201, "Cliente.Excluir", 2, "EmpresaEAbaixo" },
                    { 202, "Equipamento.Excluir", 2, "EmpresaEAbaixo" },
                    { 301, "Empresa.AlcanceEntreFiliais", 3, "Organizacao" },
                    { 401, "Cliente.Ler", 4, "EmpresaEAbaixo" },
                    { 402, "Cliente.Criar", 4, "EmpresaEAbaixo" },
                    { 403, "Cliente.Editar", 4, "EmpresaEAbaixo" },
                    { 404, "Cliente.Excluir", 4, "EmpresaEAbaixo" },
                    { 405, "Equipamento.Ler", 4, "EmpresaEAbaixo" },
                    { 406, "Equipamento.Criar", 4, "EmpresaEAbaixo" },
                    { 407, "Equipamento.Editar", 4, "EmpresaEAbaixo" },
                    { 408, "Equipamento.Excluir", 4, "EmpresaEAbaixo" },
                    { 409, "Catalogo.Ler", 4, "EmpresaEAbaixo" },
                    { 410, "Processo.Ler", 4, "EmpresaEAbaixo" },
                    { 411, "Tarefa.Ler", 4, "EmpresaEAbaixo" },
                    { 412, "Interacao.Ler", 4, "EmpresaEAbaixo" },
                    { 413, "Cobertura.Ler", 4, "EmpresaEAbaixo" },
                    { 414, "Relatorio.Ler", 4, "EmpresaEAbaixo" },
                    { 415, "Faturamento.Ler", 4, "EmpresaEAbaixo" },
                    { 416, "Territorio.Ler", 4, "EmpresaEAbaixo" },
                    { 417, "Integracao.Ler", 4, "EmpresaEAbaixo" },
                    { 418, "Legado.Ler", 4, "EmpresaEAbaixo" },
                    { 419, "Empresa.AlcanceEntreFiliais", 4, "Organizacao" },
                    { 420, "Perfil.Administrar", 4, "Organizacao" },
                    { 421, "Usuario.Administrar", 4, "Organizacao" }
                });

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

            migrationBuilder.CreateIndex(
                name: "UX_Perfil_Codigo",
                schema: "seguranca",
                table: "Perfil",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Perfil_Padrao",
                schema: "seguranca",
                table: "Perfil",
                column: "EhPadrao",
                unique: true,
                filter: "[EhPadrao] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_PerfilPermissao_Perfil_Codigo",
                schema: "seguranca",
                table: "PerfilPermissao",
                columns: new[] { "PerfilId", "CodigoPermissao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPerfil_ConcedidoPorId",
                schema: "seguranca",
                table: "UsuarioPerfil",
                column: "ConcedidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPerfil_EmpresaId",
                schema: "seguranca",
                table: "UsuarioPerfil",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPerfil_PerfilId",
                schema: "seguranca",
                table: "UsuarioPerfil",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "UX_UsuarioPerfil_Usuario_Perfil_Empresa",
                schema: "seguranca",
                table: "UsuarioPerfil",
                columns: new[] { "UsuarioId", "PerfilId", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfilPermissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "UsuarioPerfil",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "Perfil",
                schema: "seguranca");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.AddColumn<string>(
                name: "Papel",
                schema: "seguranca",
                table: "Usuario",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConjuntoDePermissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConjuntoDePermissao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConjuntoDePermissaoItem",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoPermissao = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    ConjuntoPermissaoId = table.Column<int>(type: "int", nullable: false),
                    Profundidade = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConjuntoDePermissaoItem", x => x.Id);
                    table.CheckConstraint("CK_ConjuntoDePermissaoItem_Profundidade", "[Profundidade] IN ('Proprios','Equipe','Empresa','EmpresaEAbaixo','Organizacao')");
                    table.ForeignKey(
                        name: "FK_ConjuntoDePermissaoItem_ConjuntoDePermissao_ConjuntoPermissaoId",
                        column: x => x.ConjuntoPermissaoId,
                        principalSchema: "seguranca",
                        principalTable: "ConjuntoDePermissao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioConjuntoDePermissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConcedidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ConcedidoPorId = table.Column<long>(type: "bigint", nullable: false),
                    ConjuntoPermissaoId = table.Column<int>(type: "int", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioConjuntoDePermissao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioConjuntoDePermissao_ConjuntoDePermissao_ConjuntoPermissaoId",
                        column: x => x.ConjuntoPermissaoId,
                        principalSchema: "seguranca",
                        principalTable: "ConjuntoDePermissao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioConjuntoDePermissao_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
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
                name: "UX_ConjuntoDePermissao_Codigo",
                schema: "seguranca",
                table: "ConjuntoDePermissao",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ConjuntoDePermissaoItem_Conjunto_Codigo",
                schema: "seguranca",
                table: "ConjuntoDePermissaoItem",
                columns: new[] { "ConjuntoPermissaoId", "CodigoPermissao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioConjuntoDePermissao_ConjuntoPermissaoId",
                schema: "seguranca",
                table: "UsuarioConjuntoDePermissao",
                column: "ConjuntoPermissaoId");

            migrationBuilder.CreateIndex(
                name: "UX_UsuarioConjuntoDePermissao_Usuario_Conjunto",
                schema: "seguranca",
                table: "UsuarioConjuntoDePermissao",
                columns: new[] { "UsuarioId", "ConjuntoPermissaoId" },
                unique: true);
        }
    }
}
