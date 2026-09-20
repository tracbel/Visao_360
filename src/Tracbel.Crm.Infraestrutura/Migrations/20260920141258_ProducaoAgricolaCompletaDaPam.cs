using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// A PAM INTEIRA (issue 64): a tabela de área plantada passa a guardar as quatro medidas que o
    /// IBGE publica, e ganha ao lado a linha do estado.
    ///
    /// <para><b>A tabela é RENOMEADA, não recriada.</b> O <c>dotnet ef</c> gerou um DROP seguido de
    /// CREATE, que apagaria os 54.570 registros de área plantada já carregados aqui e no servidor e
    /// deixaria o mapa de potencial em branco até a próxima carga. Renomear e acrescentar coluna
    /// preserva a área plantada; as três medidas novas ficam nulas — que é exatamente o que "o IBGE
    /// ainda não foi lido para esta medida" significa — até a carga rodar.</para>
    ///
    /// <para><b>Por que o total do estado é tabela própria</b> e não uma linha com município nulo: o
    /// total da UF não é a soma dos municípios (o valor municipal sigiloso entra nele sem aparecer
    /// embaixo), e uma coluna anulável na chave transformaria o índice único em algo que o SQL Server
    /// não consegue defender.</para>
    /// </summary>
    public partial class ProducaoAgricolaCompletaDaPam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------------------------------
            // 1. A tabela do município muda de nome e ganha as três medidas que faltavam.
            // ---------------------------------------------------------------------------------
            migrationBuilder.RenameTable(
                name: "AreaPlantadaNoMunicipio",
                schema: "organizacao",
                newName: "ProducaoAgricolaNoMunicipio",
                newSchema: "organizacao");

            migrationBuilder.RenameIndex(
                name: "IX_AreaPlantadaNoMunicipio_ImportadoPorId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "IX_ProducaoAgricolaNoMunicipio_ImportadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_AreaPlantadaNoMunicipio_ProdutoCodigoIbge_Ano",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "IX_ProducaoAgricolaNoMunicipio_ProdutoCodigoIbge_Ano");

            migrationBuilder.RenameIndex(
                name: "UX_AreaPlantadaNoMunicipio_Municipio_Ano_Produto",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "UX_ProducaoAgricolaNoMunicipio_Municipio_Ano_Produto");

            // O sp_rename da tabela não renomeia chave, FK nem CHECK: elas continuam com o nome
            // antigo até serem trocadas aqui, uma a uma.
            migrationBuilder.DropForeignKey(
                name: "FK_AreaPlantadaNoMunicipio_Municipio_MunicipioId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropForeignKey(
                name: "FK_AreaPlantadaNoMunicipio_Usuario_ImportadoPorId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AreaPlantadaNoMunicipio",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AreaPlantadaNoMunicipio_Ano",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AreaPlantadaNoMunicipio_Area",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AreaPlantadaNoMunicipio_Produto",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.AddColumn<decimal>(
                name: "AreaColhidaHectares",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                type: "decimal(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeProduzidaToneladas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDaProducaoMilReais",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProducaoAgricolaNoMunicipio",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProducaoAgricolaNoMunicipio_Municipio_MunicipioId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                column: "MunicipioId",
                principalSchema: "organizacao",
                principalTable: "Municipio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProducaoAgricolaNoMunicipio_Usuario_ImportadoPorId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                column: "ImportadoPorId",
                principalSchema: "seguranca",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Ano",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                sql: "[Ano] BETWEEN 1974 AND 2100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                sql: "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) AND ([QuantidadeProduzidaToneladas] IS NULL OR [QuantidadeProduzidaToneladas] >= 0) AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Produto",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                sql: "[ProdutoCodigoIbge] > 0");

            // ---------------------------------------------------------------------------------
            // 2. O total do estado, que o IBGE publica e não é a soma dos municípios.
            // ---------------------------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "ProducaoAgricolaNoEstado",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstadoCodigoIbge = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<short>(type: "smallint", nullable: false),
                    ProdutoCodigoIbge = table.Column<int>(type: "int", nullable: false),
                    ProdutoNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AreaPlantadaHectares = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    AreaColhidaHectares = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: true),
                    QuantidadeProduzidaToneladas = table.Column<decimal>(type: "decimal(20,2)", precision: 20, scale: 2, nullable: true),
                    ValorDaProducaoMilReais = table.Column<decimal>(type: "decimal(20,2)", precision: 20, scale: 2, nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducaoAgricolaNoEstado", x => x.Id);
                    table.CheckConstraint("CK_ProducaoAgricolaNoEstado_Ano", "[Ano] BETWEEN 1974 AND 2100");
                    table.CheckConstraint("CK_ProducaoAgricolaNoEstado_Estado", "[EstadoCodigoIbge] BETWEEN 11 AND 53");
                    table.CheckConstraint("CK_ProducaoAgricolaNoEstado_Medidas", "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) AND ([QuantidadeProduzidaToneladas] IS NULL OR [QuantidadeProduzidaToneladas] >= 0) AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)");
                    table.CheckConstraint("CK_ProducaoAgricolaNoEstado_Produto", "[ProdutoCodigoIbge] > 0");
                    table.ForeignKey(
                        name: "FK_ProducaoAgricolaNoEstado_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProducaoAgricolaNoEstado_ImportadoPorId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_ProducaoAgricolaNoEstado_Estado_Ano_Produto",
                schema: "organizacao",
                table: "ProducaoAgricolaNoEstado",
                columns: new[] { "EstadoCodigoIbge", "Ano", "ProdutoCodigoIbge" },
                unique: true);

            // ---------------------------------------------------------------------------------
            // 3. As duas listas de entidade auditável trocam o nome antigo pelos dois novos.
            // ---------------------------------------------------------------------------------
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A volta apaga o total do estado (que só a PAM alimenta) e devolve a tabela do município
            // ao nome antigo, com a área plantada intacta. As três medidas novas se perdem — elas
            // vieram do IBGE e voltam na próxima carga.
            migrationBuilder.DropTable(
                name: "ProducaoAgricolaNoEstado",
                schema: "organizacao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropForeignKey(
                name: "FK_ProducaoAgricolaNoMunicipio_Municipio_MunicipioId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropForeignKey(
                name: "FK_ProducaoAgricolaNoMunicipio_Usuario_ImportadoPorId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProducaoAgricolaNoMunicipio",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Ano",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Medidas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProducaoAgricolaNoMunicipio_Produto",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropColumn(
                name: "AreaColhidaHectares",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropColumn(
                name: "QuantidadeProduzidaToneladas",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.DropColumn(
                name: "ValorDaProducaoMilReais",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio");

            migrationBuilder.RenameIndex(
                name: "IX_ProducaoAgricolaNoMunicipio_ImportadoPorId",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "IX_AreaPlantadaNoMunicipio_ImportadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_ProducaoAgricolaNoMunicipio_ProdutoCodigoIbge_Ano",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "IX_AreaPlantadaNoMunicipio_ProdutoCodigoIbge_Ano");

            migrationBuilder.RenameIndex(
                name: "UX_ProducaoAgricolaNoMunicipio_Municipio_Ano_Produto",
                schema: "organizacao",
                table: "ProducaoAgricolaNoMunicipio",
                newName: "UX_AreaPlantadaNoMunicipio_Municipio_Ano_Produto");

            migrationBuilder.RenameTable(
                name: "ProducaoAgricolaNoMunicipio",
                schema: "organizacao",
                newName: "AreaPlantadaNoMunicipio",
                newSchema: "organizacao");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AreaPlantadaNoMunicipio",
                schema: "organizacao",
                table: "AreaPlantadaNoMunicipio",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AreaPlantadaNoMunicipio_Municipio_MunicipioId",
                schema: "organizacao",
                table: "AreaPlantadaNoMunicipio",
                column: "MunicipioId",
                principalSchema: "organizacao",
                principalTable: "Municipio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AreaPlantadaNoMunicipio_Usuario_ImportadoPorId",
                schema: "organizacao",
                table: "AreaPlantadaNoMunicipio",
                column: "ImportadoPorId",
                principalSchema: "seguranca",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AreaPlantadaNoMunicipio_Ano",
                schema: "organizacao",
                table: "AreaPlantadaNoMunicipio",
                sql: "[Ano] BETWEEN 1974 AND 2100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AreaPlantadaNoMunicipio_Area",
                schema: "organizacao",
                table: "AreaPlantadaNoMunicipio",
                sql: "[AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AreaPlantadaNoMunicipio_Produto",
                schema: "organizacao",
                table: "AreaPlantadaNoMunicipio",
                sql: "[ProdutoCodigoIbge] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");
        }
    }
}
