using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class EstruturaAgropecuariaDoMunicipio : Migration
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
                name: "AreaTerritorialDoMunicipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<short>(type: "smallint", nullable: false),
                    AreaKm2 = table.Column<decimal>(type: "decimal(11,3)", precision: 11, scale: 3, nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaTerritorialDoMunicipio", x => x.Id);
                    table.CheckConstraint("CK_AreaTerritorialDoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100");
                    table.CheckConstraint("CK_AreaTerritorialDoMunicipio_Area", "[AreaKm2] IS NULL OR [AreaKm2] > 0");
                    table.ForeignKey(
                        name: "FK_AreaTerritorialDoMunicipio_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AreaTerritorialDoMunicipio_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstabelecimentosPorAreaNoMunicipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<short>(type: "smallint", nullable: false),
                    GrupoDeAreaCodigoIbge = table.Column<int>(type: "int", nullable: false),
                    GrupoDeAreaNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Estabelecimentos = table.Column<int>(type: "int", nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstabelecimentosPorAreaNoMunicipio", x => x.Id);
                    table.CheckConstraint("CK_EstabelecimentosPorAreaNoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100");
                    table.CheckConstraint("CK_EstabelecimentosPorAreaNoMunicipio_Contagem", "[Estabelecimentos] IS NULL OR [Estabelecimentos] >= 0");
                    table.CheckConstraint("CK_EstabelecimentosPorAreaNoMunicipio_Grupo", "[GrupoDeAreaCodigoIbge] > 0");
                    table.ForeignKey(
                        name: "FK_EstabelecimentosPorAreaNoMunicipio_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstabelecimentosPorAreaNoMunicipio_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FrotaDeTratoresNoMunicipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<short>(type: "smallint", nullable: false),
                    PotenciaCodigoIbge = table.Column<int>(type: "int", nullable: false),
                    PotenciaNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EstabelecimentosComTrator = table.Column<int>(type: "int", nullable: true),
                    Tratores = table.Column<int>(type: "int", nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrotaDeTratoresNoMunicipio", x => x.Id);
                    table.CheckConstraint("CK_FrotaDeTratoresNoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100");
                    table.CheckConstraint("CK_FrotaDeTratoresNoMunicipio_Contagens", "([EstabelecimentosComTrator] IS NULL OR [EstabelecimentosComTrator] >= 0) AND ([Tratores] IS NULL OR [Tratores] >= 0)");
                    table.CheckConstraint("CK_FrotaDeTratoresNoMunicipio_Potencia", "[PotenciaCodigoIbge] > 0");
                    table.ForeignKey(
                        name: "FK_FrotaDeTratoresNoMunicipio_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FrotaDeTratoresNoMunicipio_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RebanhoNoMunicipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<short>(type: "smallint", nullable: false),
                    RebanhoCodigoIbge = table.Column<int>(type: "int", nullable: false),
                    RebanhoNome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Cabecas = table.Column<int>(type: "int", nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RebanhoNoMunicipio", x => x.Id);
                    table.CheckConstraint("CK_RebanhoNoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100");
                    table.CheckConstraint("CK_RebanhoNoMunicipio_Cabecas", "[Cabecas] IS NULL OR [Cabecas] >= 0");
                    table.CheckConstraint("CK_RebanhoNoMunicipio_Rebanho", "[RebanhoCodigoIbge] > 0");
                    table.ForeignKey(
                        name: "FK_RebanhoNoMunicipio_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RebanhoNoMunicipio_Usuario_ImportadoPorId",
                        column: x => x.ImportadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsinaDeEtanol",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cnpj = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false, collation: "Latin1_General_BIN2"),
                    RazaoSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    MesDeReferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    CapacidadeDeAnidroM3Dia = table.Column<int>(type: "int", nullable: true),
                    CapacidadeDeHidratadoM3Dia = table.Column<int>(type: "int", nullable: true),
                    ImportadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ImportadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsinaDeEtanol", x => x.Id);
                    table.CheckConstraint("CK_UsinaDeEtanol_Capacidade", "([CapacidadeDeAnidroM3Dia] IS NULL OR [CapacidadeDeAnidroM3Dia] >= 0) AND ([CapacidadeDeHidratadoM3Dia] IS NULL OR [CapacidadeDeHidratadoM3Dia] >= 0)");
                    table.CheckConstraint("CK_UsinaDeEtanol_Cnpj", "LEN([Cnpj]) = 14");
                    table.ForeignKey(
                        name: "FK_UsinaDeEtanol_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsinaDeEtanol_Usuario_ImportadoPorId",
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'AreaTerritorialDoMunicipio', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'EstabelecimentosPorAreaNoMunicipio', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'FrotaDeTratoresNoMunicipio', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RebanhoNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'UsinaDeEtanol', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_AreaTerritorialDoMunicipio_ImportadoPorId",
                schema: "organizacao",
                table: "AreaTerritorialDoMunicipio",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_AreaTerritorialDoMunicipio_Municipio_Ano",
                schema: "organizacao",
                table: "AreaTerritorialDoMunicipio",
                columns: new[] { "MunicipioId", "Ano" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstabelecimentosPorAreaNoMunicipio_ImportadoPorId",
                schema: "organizacao",
                table: "EstabelecimentosPorAreaNoMunicipio",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_EstabelecimentosPorAreaNoMunicipio_Municipio_Ano_Grupo",
                schema: "organizacao",
                table: "EstabelecimentosPorAreaNoMunicipio",
                columns: new[] { "MunicipioId", "Ano", "GrupoDeAreaCodigoIbge" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrotaDeTratoresNoMunicipio_ImportadoPorId",
                schema: "organizacao",
                table: "FrotaDeTratoresNoMunicipio",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_FrotaDeTratoresNoMunicipio_Municipio_Ano_Potencia",
                schema: "organizacao",
                table: "FrotaDeTratoresNoMunicipio",
                columns: new[] { "MunicipioId", "Ano", "PotenciaCodigoIbge" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RebanhoNoMunicipio_ImportadoPorId",
                schema: "organizacao",
                table: "RebanhoNoMunicipio",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_RebanhoNoMunicipio_RebanhoCodigoIbge_Ano",
                schema: "organizacao",
                table: "RebanhoNoMunicipio",
                columns: new[] { "RebanhoCodigoIbge", "Ano" });

            migrationBuilder.CreateIndex(
                name: "UX_RebanhoNoMunicipio_Municipio_Ano_Rebanho",
                schema: "organizacao",
                table: "RebanhoNoMunicipio",
                columns: new[] { "MunicipioId", "Ano", "RebanhoCodigoIbge" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsinaDeEtanol_ImportadoPorId",
                schema: "organizacao",
                table: "UsinaDeEtanol",
                column: "ImportadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsinaDeEtanol_MunicipioId",
                schema: "organizacao",
                table: "UsinaDeEtanol",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "UX_UsinaDeEtanol_Cnpj",
                schema: "organizacao",
                table: "UsinaDeEtanol",
                column: "Cnpj",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaTerritorialDoMunicipio",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "EstabelecimentosPorAreaNoMunicipio",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "FrotaDeTratoresNoMunicipio",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "RebanhoNoMunicipio",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "UsinaDeEtanol",
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
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('AlteracaoDeCampo', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompradorPendente', 'ConjuntoPermissao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Empresa', 'Endereco', 'Equipamento', 'ExecucaoDeSincronizacao', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Interacao', 'ItemConjuntoPermissao', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDescartada', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PontoDeSincronismo', 'Processo', 'ProducaoAgricolaNoEstado', 'ProducaoAgricolaNoMunicipio', 'RegistroDeOrigem', 'RegraDePotencial', 'ResponsavelPeloMunicipio', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'VinculoDeClienteComEquipamento')");
        }
    }
}
