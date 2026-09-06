using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class ModeloInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Escrita a mao (ver ModeloInicial.ColacaoDoBanco.cs). Precisa vir ANTES de qualquer
            // tabela: colacao de banco nao se troca com objeto em uso.
            DefinirColacaoDoBanco(migrationBuilder);

            migrationBuilder.EnsureSchema(
                name: "comercial");

            migrationBuilder.EnsureSchema(
                name: "auditoria");

            migrationBuilder.EnsureSchema(
                name: "metadado");

            migrationBuilder.EnsureSchema(
                name: "organizacao");

            migrationBuilder.EnsureSchema(
                name: "integracao");

            migrationBuilder.EnsureSchema(
                name: "seguranca");

            migrationBuilder.EnsureSchema(
                name: "documento");

            migrationBuilder.EnsureSchema(
                name: "frota");

            migrationBuilder.EnsureSchema(
                name: "processo");

            migrationBuilder.EnsureSchema(
                name: "relatorio");

            migrationBuilder.CreateTable(
                name: "CampoAuditado",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    RetencaoMeses = table.Column<short>(type: "smallint", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampoAuditado", x => x.Id);
                    table.CheckConstraint("CK_CampoAuditado_Retencao", "[RetencaoMeses] BETWEEN 1 AND 120");
                });

            migrationBuilder.CreateTable(
                name: "Catalogo",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PermiteItemNovo = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConjuntoDePermissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConjuntoDePermissao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresa",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Cnpj = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    EmpresaPaiId = table.Column<int>(type: "int", nullable: true),
                    Caminho = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false),
                    Nivel = table.Column<short>(type: "smallint", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                    table.CheckConstraint("CK_Empresa_Nivel", "[Nivel] >= 0");
                    table.ForeignKey(
                        name: "FK_Empresa_Empresa_EmpresaPaiId",
                        column: x => x.EmpresaPaiId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LinhaDeNegocio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DiasCicloClasseA = table.Column<short>(type: "smallint", nullable: true),
                    DiasCicloClasseB = table.Column<short>(type: "smallint", nullable: true),
                    DiasCicloClasseC = table.Column<short>(type: "smallint", nullable: true),
                    DiasCicloClasseD = table.Column<short>(type: "smallint", nullable: true),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhaDeNegocio", x => x.Id);
                    table.CheckConstraint("CK_LinhaDeNegocio_Ciclo", "COALESCE([DiasCicloClasseA], 1) > 0 AND COALESCE([DiasCicloClasseB], 1) > 0 AND COALESCE([DiasCicloClasseC], 1) > 0 AND COALESCE([DiasCicloClasseD], 1) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Marca",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EhRepresentada = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotivoDePerda",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Categoria = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ExigeConcorrente = table.Column<bool>(type: "bit", nullable: false),
                    ExigeObservacao = table.Column<bool>(type: "bit", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivoDePerda", x => x.Id);
                    table.CheckConstraint("CK_MotivoDePerda_Categoria", "[Categoria] IN ('Preco','Prazo','Produto','Financiamento','Desistencia','Concorrencia','Outro')");
                });

            migrationBuilder.CreateTable(
                name: "Permissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Verbo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissao", x => x.Id);
                    table.CheckConstraint("CK_Permissao_Verbo", "[Verbo] IN ('Ler','Criar','Editar','Excluir','Atribuir','Compartilhar')");
                });

            migrationBuilder.CreateTable(
                name: "Sistema",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MeioDeAcesso = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ResponsavelTecnico = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TratadorDeEvento",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Evento = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    TipoImplementacao = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Momento = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    EhAssincrono = table.Column<bool>(type: "bit", nullable: false),
                    CamposFiltro = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TratadorDeEvento", x => x.Id);
                    table.CheckConstraint("CK_TratadorDeEvento_Assincrono", "[EhAssincrono] = 0 OR [Momento] = 'PosOperacao'");
                    table.CheckConstraint("CK_TratadorDeEvento_Momento", "[Momento] IN ('PreValidacao','PreOperacao','PosOperacao')");
                });

            migrationBuilder.CreateTable(
                name: "CampoPersonalizado",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Rotulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TipoDeCampo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    EhPersonalizado = table.Column<bool>(type: "bit", nullable: false),
                    EhObrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    TamanhoMaximo = table.Column<int>(type: "int", nullable: true),
                    CatalogoId = table.Column<int>(type: "int", nullable: true),
                    Grupo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    CondicaoVisibilidade = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Validacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoResumo = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampoPersonalizado", x => x.Id);
                    table.CheckConstraint("CK_CampoPersonalizado_Lista", "[TipoDeCampo] <> 'Lista' OR [CatalogoId] IS NOT NULL");
                    table.CheckConstraint("CK_CampoPersonalizado_Tipo", "[TipoDeCampo] IN ('Texto','Numero','Data','Booleano','Lista','Referencia')");
                    table.CheckConstraint("CK_CampoPersonalizado_ValidacaoJson", "[Validacao] IS NULL OR ISJSON([Validacao]) = 1");
                    table.ForeignKey(
                        name: "FK_CampoPersonalizado_Catalogo_CatalogoId",
                        column: x => x.CatalogoId,
                        principalSchema: "metadado",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoItem",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogoId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_CI_AI"),
                    ItemPaiId = table.Column<int>(type: "int", nullable: true),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    ExigeObservacao = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoUsoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoItem", x => x.Id);
                    table.UniqueConstraint("AK_CatalogoItem_CatalogoId", x => new { x.CatalogoId, x.Id });
                    table.CheckConstraint("CK_CatalogoItem_Ordem", "[Ordem] >= 0");
                    table.CheckConstraint("CK_CatalogoItem_Pai", "[ItemPaiId] IS NULL OR [ItemPaiId] <> [Id]");
                    table.ForeignKey(
                        name: "FK_CatalogoItem_CatalogoItem_ItemPaiId",
                        column: x => x.ItemPaiId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogoItem_Catalogo_CatalogoId",
                        column: x => x.CatalogoId,
                        principalSchema: "metadado",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConjuntoDePermissaoItem",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConjuntoPermissaoId = table.Column<int>(type: "int", nullable: false),
                    CodigoPermissao = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
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
                name: "Equipe",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Equipe", x => x.Id);
                    table.CheckConstraint("CK_Equipe_Tipo", "[Tipo] IN ('Proprietaria','Acesso')");
                    table.ForeignKey(
                        name: "FK_Equipe_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentidadeExterna = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomePrincipal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeExibicao = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    GestorId = table.Column<long>(type: "bigint", nullable: true),
                    Papel = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    DesativadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UltimoLoginEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
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
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.CheckConstraint("CK_Usuario_Desativacao", "[EstaAtivo] = 1 OR [DesativadoEm] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Usuario_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuario_Usuario_GestorId",
                        column: x => x.GestorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Praca",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Uf = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: true),
                    LinhaDeNegocioId = table.Column<int>(type: "int", nullable: false),
                    AnoReferencia = table.Column<short>(type: "smallint", nullable: false),
                    PotencialEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaquinasEstimadas = table.Column<int>(type: "int", nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Praca", x => x.Id);
                    table.CheckConstraint("CK_Praca_AnoReferencia", "[AnoReferencia] BETWEEN 2000 AND 2100");
                    table.CheckConstraint("CK_Praca_Uf", "[Uf] IS NULL OR [Uf] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z]'");
                    table.ForeignKey(
                        name: "FK_Praca_LinhaDeNegocio_LinhaDeNegocioId",
                        column: x => x.LinhaDeNegocioId,
                        principalSchema: "organizacao",
                        principalTable: "LinhaDeNegocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TipoProcesso",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LinhaDeNegocioId = table.Column<int>(type: "int", nullable: true),
                    Versao = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoProcesso", x => x.Id);
                    table.CheckConstraint("CK_TipoProcesso_Versao", "[Versao] >= 1");
                    table.ForeignKey(
                        name: "FK_TipoProcesso_LinhaDeNegocio_LinhaDeNegocioId",
                        column: x => x.LinhaDeNegocioId,
                        principalSchema: "organizacao",
                        principalTable: "LinhaDeNegocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Familia",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarcaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Familia_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalSchema: "frota",
                        principalTable: "Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChaveExterna",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    RegistroId = table.Column<long>(type: "bigint", nullable: false),
                    ChaveOrigem = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    SincronizadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChaveExterna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChaveExterna_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fonte",
                schema: "relatorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    NomeDaVisao = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    EntidadeRaiz = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: true),
                    EhMaterializada = table.Column<bool>(type: "bit", nullable: false),
                    AtualizadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonte", x => x.Id);
                    table.CheckConstraint("CK_Fonte_Materializada", "[EhMaterializada] = 0 OR [AtualizadaEm] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Fonte_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MensagemDeSaida",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Situacao = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Tentativas = table.Column<short>(type: "smallint", nullable: false),
                    ProximaTentativaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UltimoErro = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EntregueEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagemDeSaida", x => x.Id);
                    table.CheckConstraint("CK_MensagemDeSaida_ConteudoJson", "ISJSON([Conteudo]) = 1");
                    table.CheckConstraint("CK_MensagemDeSaida_Entrega", "[Situacao] <> 'Entregue' OR [EntregueEm] IS NOT NULL");
                    table.CheckConstraint("CK_MensagemDeSaida_Situacao", "[Situacao] IN ('Pendente','Entregue','Falhou','DescartadaAposLimite')");
                    table.CheckConstraint("CK_MensagemDeSaida_Tentativas", "[Tentativas] >= 0");
                    table.ForeignKey(
                        name: "FK_MensagemDeSaida_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PontoDeSincronismo",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Fluxo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    UltimoValor = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    RegistrosLidos = table.Column<int>(type: "int", nullable: false),
                    RegistrosGravados = table.Column<int>(type: "int", nullable: false),
                    RegistrosErro = table.Column<int>(type: "int", nullable: false),
                    MinutosParaAlarme = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontoDeSincronismo", x => x.Id);
                    table.CheckConstraint("CK_PontoDeSincronismo_Alarme", "[MinutosParaAlarme] > 0");
                    table.CheckConstraint("CK_PontoDeSincronismo_Contadores", "[RegistrosLidos] >= 0 AND [RegistrosGravados] >= 0 AND [RegistrosErro] >= 0");
                    table.ForeignKey(
                        name: "FK_PontoDeSincronismo_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recepcao",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecebidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ChaveOrigem = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Erro = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProcessadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ExpurgarApos = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recepcao", x => new { x.Id, x.RecebidaEm });
                    table.CheckConstraint("CK_Recepcao_ConteudoJson", "ISJSON([Conteudo]) = 1");
                    table.CheckConstraint("CK_Recepcao_Desfecho", "([Situacao] <> 'Falhou' OR [Erro] IS NOT NULL) AND ([Situacao] <> 'Processada' OR [ProcessadaEm] IS NOT NULL)");
                    table.CheckConstraint("CK_Recepcao_Expurgo", "[ExpurgarApos] > [RecebidaEm]");
                    table.CheckConstraint("CK_Recepcao_Situacao", "[Situacao] IN ('Recebida','Processada','Falhou','Ignorada')");
                    table.ForeignKey(
                        name: "FK_Recepcao_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documento",
                schema: "documento",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    TipoDocumentoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    TipoConteudo = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    ResumoConteudo = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    CaminhoArmazenamento = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    NumeroVersao = table.Column<int>(type: "int", nullable: false),
                    ValidoAte = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_Documento", x => x.Id);
                    table.CheckConstraint("CK_Documento_NumeroVersao", "[NumeroVersao] >= 1");
                    table.CheckConstraint("CK_Documento_Resumo", "[ResumoConteudo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9a-f]%'");
                    table.CheckConstraint("CK_Documento_Tamanho", "[TamanhoBytes] > 0");
                    table.ForeignKey(
                        name: "FK_Documento_CatalogoItem_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documento_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlteracaoDeCampo",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    RegistroId = table.Column<long>(type: "bigint", nullable: false),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ValorNovo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlteracaoDeCampo", x => new { x.Id, x.AlteradoEm });
                    table.CheckConstraint("CK_AlteracaoDeCampo_Mudou", "([ValorAnterior] IS NOT NULL OR [ValorNovo] IS NOT NULL) AND ([ValorAnterior] IS NULL OR [ValorNovo] IS NULL OR [ValorAnterior] <> [ValorNovo])");
                    table.ForeignKey(
                        name: "FK_AlteracaoDeCampo_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlteracaoDeCampo_Usuario_AlteradoPorId",
                        column: x => x.AlteradoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    NomeRazao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_CI_AI"),
                    NomeFantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoDePessoa = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Documento = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    InscricaoEstadual = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    AtividadeEconomica = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SituacaoDesde = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    MotivoInativacaoId = table.Column<int>(type: "int", nullable: true),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
                    ProprietarioEquipeId = table.Column<long>(type: "bigint", nullable: true),
                    ClienteMatrizId = table.Column<long>(type: "bigint", nullable: true),
                    OrigemId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                    table.CheckConstraint("CK_Cliente_Documento", "[Documento] IS NULL OR ([TipoDePessoa] = 'Fisica' AND LEN([Documento]) = 11) OR ([TipoDePessoa] = 'Juridica' AND LEN([Documento]) = 14)");
                    table.CheckConstraint("CK_Cliente_Situacao", "[Situacao] IN ('Suspect','Prospect','Cliente','ClienteInativo','Encerrado')");
                    table.CheckConstraint("CK_Cliente_TipoDePessoa", "[TipoDePessoa] IN ('Fisica','Juridica')");
                    table.ForeignKey(
                        name: "FK_Cliente_CatalogoItem_MotivoInativacaoId",
                        column: x => x.MotivoInativacaoId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_CatalogoItem_OrigemId",
                        column: x => x.OrigemId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Cliente_ClienteMatrizId",
                        column: x => x.ClienteMatrizId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Equipe_ProprietarioEquipeId",
                        column: x => x.ProprietarioEquipeId,
                        principalSchema: "seguranca",
                        principalTable: "Equipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cliente_Usuario_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompartilhamentoDeRegistro",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    RegistroId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    EquipeId = table.Column<long>(type: "bigint", nullable: true),
                    Nivel = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Motivo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    RegraOrigemId = table.Column<int>(type: "int", nullable: true),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
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
                    table.PrimaryKey("PK_CompartilhamentoDeRegistro", x => x.Id);
                    table.CheckConstraint("CK_CompartilhamentoDeRegistro_Motivo", "[Motivo] IN ('Manual','Regra','Equipe','Hierarquia','Delegacao')");
                    table.CheckConstraint("CK_CompartilhamentoDeRegistro_Nivel", "[Nivel] IN ('Leitura','Edicao')");
                    table.CheckConstraint("CK_CompartilhamentoDeRegistro_UmSujeito", "([UsuarioId] IS NOT NULL AND [EquipeId] IS NULL) OR ([UsuarioId] IS NULL AND [EquipeId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CompartilhamentoDeRegistro_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompartilhamentoDeRegistro_Equipe_EquipeId",
                        column: x => x.EquipeId,
                        principalSchema: "seguranca",
                        principalTable: "Equipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompartilhamentoDeRegistro_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contato",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Sobrenome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Documento = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Contato", x => x.Id);
                    table.CheckConstraint("CK_Contato_Documento", "[Documento] IS NULL OR LEN([Documento]) = 11");
                    table.ForeignKey(
                        name: "FK_Contato_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contato_Usuario_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipeMembro",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipeId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    EhLider = table.Column<bool>(type: "bit", nullable: false),
                    EntrouEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    SaiuEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipeMembro", x => x.Id);
                    table.CheckConstraint("CK_EquipeMembro_Periodo", "[SaiuEm] IS NULL OR [SaiuEm] >= [EntrouEm]");
                    table.ForeignKey(
                        name: "FK_EquipeMembro_Equipe_EquipeId",
                        column: x => x.EquipeId,
                        principalSchema: "seguranca",
                        principalTable: "Equipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipeMembro_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventoDeAcesso",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcorreuEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    NomePrincipal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tipo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    RegistroId = table.Column<long>(type: "bigint", nullable: true),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    AgenteUsuario = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Detalhe = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventoDeAcesso", x => new { x.Id, x.OcorreuEm });
                    table.CheckConstraint("CK_EventoDeAcesso_Registro", "([Entidade] IS NULL AND [RegistroId] IS NULL) OR ([Entidade] IS NOT NULL AND [RegistroId] IS NOT NULL)");
                    table.CheckConstraint("CK_EventoDeAcesso_Tipo", "[Tipo] IN ('Login','LoginFalhou','Logout','AcessoNegado','ExportacaoDados','LeituraDadoSensivel')");
                    table.ForeignKey(
                        name: "FK_EventoDeAcesso_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HierarquiaComercial",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AncestralId = table.Column<long>(type: "bigint", nullable: false),
                    DescendenteId = table.Column<long>(type: "bigint", nullable: false),
                    Profundidade = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HierarquiaComercial", x => x.Id);
                    table.CheckConstraint("CK_HierarquiaComercial_Profundidade", "[Profundidade] >= 0");
                    table.ForeignKey(
                        name: "FK_HierarquiaComercial_Usuario_AncestralId",
                        column: x => x.AncestralId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HierarquiaComercial_Usuario_DescendenteId",
                        column: x => x.DescendenteId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioConjuntoDePermissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    ConjuntoPermissaoId = table.Column<int>(type: "int", nullable: false),
                    ConcedidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ConcedidoPorId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Carteira",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    LinhaDeNegocioId = table.Column<int>(type: "int", nullable: false),
                    PracaId = table.Column<int>(type: "int", nullable: true),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ResponsavelId = table.Column<long>(type: "bigint", nullable: false),
                    SupervisorId = table.Column<long>(type: "bigint", nullable: true),
                    EquipeId = table.Column<long>(type: "bigint", nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Carteira", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carteira_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carteira_Equipe_EquipeId",
                        column: x => x.EquipeId,
                        principalSchema: "seguranca",
                        principalTable: "Equipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carteira_LinhaDeNegocio_LinhaDeNegocioId",
                        column: x => x.LinhaDeNegocioId,
                        principalSchema: "organizacao",
                        principalTable: "LinhaDeNegocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carteira_Praca_PracaId",
                        column: x => x.PracaId,
                        principalSchema: "organizacao",
                        principalTable: "Praca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carteira_Usuario_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carteira_Usuario_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fase",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    Marco = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ProbabilidadePercentual = table.Column<short>(type: "smallint", nullable: true),
                    ExigeCamposObrigatorios = table.Column<bool>(type: "bit", nullable: false),
                    EhFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fase", x => x.Id);
                    table.CheckConstraint("CK_Fase_Probabilidade", "[ProbabilidadePercentual] IS NULL OR [ProbabilidadePercentual] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_Fase_TipoProcesso_TipoProcessoId",
                        column: x => x.TipoProcessoId,
                        principalSchema: "processo",
                        principalTable: "TipoProcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Formulario",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: true),
                    VersaoPublicada = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formulario", x => x.Id);
                    table.CheckConstraint("CK_Formulario_Versao", "[VersaoPublicada] >= 1");
                    table.ForeignKey(
                        name: "FK_Formulario_TipoProcesso_TipoProcessoId",
                        column: x => x.TipoProcessoId,
                        principalSchema: "processo",
                        principalTable: "TipoProcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Modelo",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamiliaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PotenciaCv = table.Column<short>(type: "smallint", nullable: true),
                    IntervaloManutencaoHoras = table.Column<int>(type: "int", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modelo", x => x.Id);
                    table.CheckConstraint("CK_Modelo_IntervaloManutencao", "[IntervaloManutencaoHoras] IS NULL OR [IntervaloManutencaoHoras] > 0");
                    table.CheckConstraint("CK_Modelo_Potencia", "[PotenciaCv] IS NULL OR [PotenciaCv] > 0");
                    table.ForeignKey(
                        name: "FK_Modelo_Familia_FamiliaId",
                        column: x => x.FamiliaId,
                        principalSchema: "frota",
                        principalTable: "Familia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FonteCampo",
                schema: "relatorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FonteId = table.Column<int>(type: "int", nullable: false),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Rotulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TipoDeDado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PermiteAgrupar = table.Column<bool>(type: "bit", nullable: false),
                    PermiteFiltrar = table.Column<bool>(type: "bit", nullable: false),
                    PermiteSomar = table.Column<bool>(type: "bit", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FonteCampo", x => x.Id);
                    table.CheckConstraint("CK_FonteCampo_TipoDeDado", "[TipoDeDado] IN ('Texto','Numero','Data','Booleano','Moeda','Percentual')");
                    table.ForeignKey(
                        name: "FK_FonteCampo_Fonte_FonteId",
                        column: x => x.FonteId,
                        principalSchema: "relatorio",
                        principalTable: "Fonte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Relatorio",
                schema: "relatorio",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FonteId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Definicao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Visibilidade = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Relatorio", x => x.Id);
                    table.CheckConstraint("CK_Relatorio_DefinicaoJson", "ISJSON([Definicao]) = 1");
                    table.CheckConstraint("CK_Relatorio_Visibilidade", "[Visibilidade] IN ('Privado','Equipe','Empresa')");
                    table.ForeignKey(
                        name: "FK_Relatorio_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relatorio_Fonte_FonteId",
                        column: x => x.FonteId,
                        principalSchema: "relatorio",
                        principalTable: "Fonte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relatorio_Usuario_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MensagemDescartada",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MensagemDeSaidaId = table.Column<long>(type: "bigint", nullable: true),
                    Fluxo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Erro = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Tentativas = table.Column<short>(type: "smallint", nullable: false),
                    DescartadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    TratadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    TratadaPorId = table.Column<long>(type: "bigint", nullable: true),
                    Tratativa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagemDescartada", x => x.Id);
                    table.CheckConstraint("CK_MensagemDescartada_ConteudoJson", "ISJSON([Conteudo]) = 1");
                    table.CheckConstraint("CK_MensagemDescartada_Tentativas", "[Tentativas] >= 0");
                    table.CheckConstraint("CK_MensagemDescartada_Tratativa", "[TratadaEm] IS NULL OR ([TratadaPorId] IS NOT NULL AND [Tratativa] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_MensagemDescartada_MensagemDeSaida_MensagemDeSaidaId",
                        column: x => x.MensagemDeSaidaId,
                        principalSchema: "integracao",
                        principalTable: "MensagemDeSaida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MensagemDescartada_Usuario_TratadaPorId",
                        column: x => x.TratadaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vinculo",
                schema: "documento",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<long>(type: "bigint", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    RegistroId = table.Column<long>(type: "bigint", nullable: false),
                    VinculadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    VinculadoPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vinculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vinculo_Documento_DocumentoId",
                        column: x => x.DocumentoId,
                        principalSchema: "documento",
                        principalTable: "Documento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vinculo_Usuario_VinculadoPorId",
                        column: x => x.VinculadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alerta",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    Severidade = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detalhe = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    VigenteDe = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteAte = table.Column<DateOnly>(type: "date", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Alerta", x => x.Id);
                    table.CheckConstraint("CK_Alerta_Severidade", "[Severidade] IN ('Informativo','Atencao','Critico')");
                    table.CheckConstraint("CK_Alerta_Vigencia", "[VigenteAte] IS NULL OR [VigenteAte] >= [VigenteDe]");
                    table.ForeignKey(
                        name: "FK_Alerta_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Endereco",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Identificacao = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Logradouro = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Numero = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Complemento = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Municipio = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Uf = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Cep = table.Column<string>(type: "char(8)", unicode: false, fixedLength: true, maxLength: 8, nullable: true),
                    Hectares = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    CulturaId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    EhPrincipal = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Endereco", x => x.Id);
                    table.CheckConstraint("CK_Endereco_Coordenada", "([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)");
                    table.CheckConstraint("CK_Endereco_Hectares", "[Hectares] IS NULL OR [Hectares] > 0");
                    table.CheckConstraint("CK_Endereco_Tipo", "[Tipo] IN ('Fiscal','Entrega','Cobranca','Fazenda')");
                    table.CheckConstraint("CK_Endereco_Uf", "[Uf] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z]'");
                    table.ForeignKey(
                        name: "FK_Endereco_CatalogoItem_CulturaId",
                        column: x => x.CulturaId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Endereco_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Endereco_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CanalContato",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValorNormalizado = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Rotulo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    EhPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    EhValido = table.Column<bool>(type: "bit", nullable: false),
                    ValidadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanalContato", x => x.Id);
                    table.CheckConstraint("CK_CanalContato_Tipo", "[Tipo] IN ('Email','Telefone','Celular','WhatsApp')");
                    table.CheckConstraint("CK_CanalContato_UmDono", "([ClienteId] IS NOT NULL AND [ContatoId] IS NULL) OR ([ClienteId] IS NULL AND [ContatoId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CanalContato_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CanalContato_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CanalContato_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClienteContato",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    ContatoId = table.Column<long>(type: "bigint", nullable: false),
                    PapelId = table.Column<int>(type: "int", nullable: false),
                    EhPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    IniciouEm = table.Column<DateOnly>(type: "date", nullable: true),
                    EncerrouEm = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteContato", x => x.Id);
                    table.CheckConstraint("CK_ClienteContato_Periodo", "[EncerrouEm] IS NULL OR [IniciouEm] IS NULL OR [EncerrouEm] >= [IniciouEm]");
                    table.ForeignKey(
                        name: "FK_ClienteContato_CatalogoItem_PapelId",
                        column: x => x.PapelId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClienteContato_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClienteContato_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsentimentoComunicacao",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    Canal = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Finalidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Concedido = table.Column<bool>(type: "bit", nullable: false),
                    DecididaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    OrigemEvidencia = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    ReferenciaEvidencia = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    RegistradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimentoComunicacao", x => x.Id);
                    table.CheckConstraint("CK_ConsentimentoComunicacao_Canal", "[Canal] IN ('Email','Sms','WhatsApp','Telefone','Correspondencia')");
                    table.CheckConstraint("CK_ConsentimentoComunicacao_Finalidade", "[Finalidade] IN ('Marketing','Transacional','Pesquisa','Cobranca')");
                    table.CheckConstraint("CK_ConsentimentoComunicacao_UmTitular", "([ClienteId] IS NOT NULL AND [ContatoId] IS NULL) OR ([ClienteId] IS NULL AND [ContatoId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ConsentimentoComunicacao_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentimentoComunicacao_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentimentoComunicacao_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClienteCarteira",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    CarteiraId = table.Column<long>(type: "bigint", nullable: false),
                    Classe = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false),
                    PotencialAnual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiasCicloContato = table.Column<short>(type: "smallint", nullable: true),
                    UltimaInteracaoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    VinculadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    VinculadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    DesvinculadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteCarteira", x => x.Id);
                    table.CheckConstraint("CK_ClienteCarteira_Ciclo", "[DiasCicloContato] IS NULL OR [DiasCicloContato] > 0");
                    table.CheckConstraint("CK_ClienteCarteira_Classe", "[Classe] IN ('A','B','C','D')");
                    table.ForeignKey(
                        name: "FK_ClienteCarteira_Carteira_CarteiraId",
                        column: x => x.CarteiraId,
                        principalSchema: "organizacao",
                        principalTable: "Carteira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClienteCarteira_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Meta",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    LinhaDeNegocioId = table.Column<int>(type: "int", nullable: true),
                    CarteiraId = table.Column<long>(type: "bigint", nullable: true),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PeriodoInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodoFim = table.Column<DateOnly>(type: "date", nullable: false),
                    Alvo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Meta", x => x.Id);
                    table.CheckConstraint("CK_Meta_Alvo", "[Alvo] >= 0");
                    table.CheckConstraint("CK_Meta_Periodo", "[PeriodoFim] >= [PeriodoInicio]");
                    table.CheckConstraint("CK_Meta_Tipo", "[Tipo] IN ('Faturamento','Cobertura','Frequencia','Volume')");
                    table.ForeignKey(
                        name: "FK_Meta_Carteira_CarteiraId",
                        column: x => x.CarteiraId,
                        principalSchema: "organizacao",
                        principalTable: "Carteira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Meta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Meta_LinhaDeNegocio_LinhaDeNegocioId",
                        column: x => x.LinhaDeNegocioId,
                        principalSchema: "organizacao",
                        principalTable: "LinhaDeNegocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Meta_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Processo",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    CarteiraId = table.Column<long>(type: "bigint", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FaseId = table.Column<int>(type: "int", nullable: false),
                    FaseDesde = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SituacaoDesde = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    MotivoDePerdaId = table.Column<int>(type: "int", nullable: true),
                    ObservacaoDaPerda = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConcorrenteId = table.Column<int>(type: "int", nullable: true),
                    ValorEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ValorFinal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Quantidade = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: true),
                    PrevisaoConclusao = table.Column<DateOnly>(type: "date", nullable: true),
                    PrevisaoConclusaoOriginal = table.Column<DateOnly>(type: "date", nullable: true),
                    ConcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
                    ProprietarioEquipeId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Processo", x => x.Id);
                    table.CheckConstraint("CK_Processo_Encerramento", "[Situacao] IN ('Aberto','Suspenso') OR [ConcluidoEm] IS NOT NULL");
                    table.CheckConstraint("CK_Processo_MotivoDePerda", "[Situacao] <> 'Perdido' OR [MotivoDePerdaId] IS NOT NULL");
                    table.CheckConstraint("CK_Processo_Situacao", "[Situacao] IN ('Aberto','Suspenso','Ganho','Perdido','Cancelado')");
                    table.CheckConstraint("CK_Processo_Valores", "([ValorEstimado] IS NULL OR [ValorEstimado] >= 0) AND ([ValorFinal] IS NULL OR [ValorFinal] >= 0)");
                    table.ForeignKey(
                        name: "FK_Processo_Carteira_CarteiraId",
                        column: x => x.CarteiraId,
                        principalSchema: "organizacao",
                        principalTable: "Carteira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_CatalogoItem_ConcorrenteId",
                        column: x => x.ConcorrenteId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_Equipe_ProprietarioEquipeId",
                        column: x => x.ProprietarioEquipeId,
                        principalSchema: "seguranca",
                        principalTable: "Equipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_Fase_FaseId",
                        column: x => x.FaseId,
                        principalSchema: "processo",
                        principalTable: "Fase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_MotivoDePerda_MotivoDePerdaId",
                        column: x => x.MotivoDePerdaId,
                        principalSchema: "processo",
                        principalTable: "MotivoDePerda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_TipoProcesso_TipoProcessoId",
                        column: x => x.TipoProcessoId,
                        principalSchema: "processo",
                        principalTable: "TipoProcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Processo_Usuario_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pergunta",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormularioId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TipoDeResposta = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CatalogoId = table.Column<int>(type: "int", nullable: true),
                    EhObrigatoria = table.Column<bool>(type: "bit", nullable: false),
                    PerguntaCondicaoId = table.Column<int>(type: "int", nullable: true),
                    ValorCondicao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pergunta", x => x.Id);
                    table.CheckConstraint("CK_Pergunta_Catalogo", "[TipoDeResposta] NOT IN ('Catalogo','CatalogoMultiplo') OR [CatalogoId] IS NOT NULL");
                    table.CheckConstraint("CK_Pergunta_Condicao", "[PerguntaCondicaoId] IS NULL OR [PerguntaCondicaoId] <> [Id]");
                    table.CheckConstraint("CK_Pergunta_TipoDeResposta", "[TipoDeResposta] IN ('Texto','Numero','Data','Booleano','Catalogo','Cliente','Equipamento','CatalogoMultiplo')");
                    table.ForeignKey(
                        name: "FK_Pergunta_Catalogo_CatalogoId",
                        column: x => x.CatalogoId,
                        principalSchema: "metadado",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pergunta_Formulario_FormularioId",
                        column: x => x.FormularioId,
                        principalSchema: "metadado",
                        principalTable: "Formulario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pergunta_Pergunta_PerguntaCondicaoId",
                        column: x => x.PerguntaCondicaoId,
                        principalSchema: "metadado",
                        principalTable: "Pergunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TipoTarefa",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: true),
                    Categoria = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PrazoDiasUteis = table.Column<short>(type: "smallint", nullable: false),
                    EhAprovacao = table.Column<bool>(type: "bit", nullable: false),
                    ExigeGeorreferencia = table.Column<bool>(type: "bit", nullable: false),
                    ContaParaCobertura = table.Column<bool>(type: "bit", nullable: false),
                    FormularioId = table.Column<int>(type: "int", nullable: true),
                    Cor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoUsoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoTarefa", x => x.Id);
                    table.CheckConstraint("CK_TipoTarefa_Categoria", "[Categoria] IN ('Visita','Ligacao','WhatsApp','Email','Remota','Interna')");
                    table.CheckConstraint("CK_TipoTarefa_Cor", "[Cor] IS NULL OR ([Cor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND LEN([Cor]) = 7)");
                    table.CheckConstraint("CK_TipoTarefa_Prazo", "[PrazoDiasUteis] >= 0");
                    table.ForeignKey(
                        name: "FK_TipoTarefa_Formulario_FormularioId",
                        column: x => x.FormularioId,
                        principalSchema: "metadado",
                        principalTable: "Formulario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TipoTarefa_TipoProcesso_TipoProcessoId",
                        column: x => x.TipoProcessoId,
                        principalSchema: "processo",
                        principalTable: "TipoProcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipamento",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    ModeloId = table.Column<int>(type: "int", nullable: false),
                    Chassi = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NumeroSerie = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Placa = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    AnoFabricacao = table.Column<short>(type: "smallint", nullable: true),
                    AnoModelo = table.Column<short>(type: "smallint", nullable: true),
                    HorimetroAtual = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    HorimetroAtualizadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    LocalizacaoDescrita = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EnderecoId = table.Column<long>(type: "bigint", nullable: true),
                    EquipamentoPaiId = table.Column<long>(type: "bigint", nullable: true),
                    EquipamentoSubstitutoId = table.Column<long>(type: "bigint", nullable: true),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Origem = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    VendidoEm = table.Column<DateOnly>(type: "date", nullable: true),
                    GarantiaAte = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_Equipamento", x => x.Id);
                    table.CheckConstraint("CK_Equipamento_Ano", "([AnoFabricacao] IS NULL OR [AnoFabricacao] BETWEEN 1900 AND 2100) AND ([AnoModelo] IS NULL OR [AnoModelo] BETWEEN 1900 AND 2100)");
                    table.CheckConstraint("CK_Equipamento_Hierarquia", "([EquipamentoPaiId] IS NULL OR [EquipamentoPaiId] <> [Id]) AND ([EquipamentoSubstitutoId] IS NULL OR [EquipamentoSubstitutoId] <> [Id])");
                    table.CheckConstraint("CK_Equipamento_Horimetro", "[HorimetroAtual] IS NULL OR [HorimetroAtual] >= 0");
                    table.CheckConstraint("CK_Equipamento_Origem", "[Origem] IN ('Protheus','Crm')");
                    table.CheckConstraint("CK_Equipamento_Situacao", "[Situacao] IN ('Estoque','Ativo','Vendido','Baixado')");
                    table.ForeignKey(
                        name: "FK_Equipamento_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipamento_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipamento_Endereco_EnderecoId",
                        column: x => x.EnderecoId,
                        principalSchema: "comercial",
                        principalTable: "Endereco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipamento_Equipamento_EquipamentoPaiId",
                        column: x => x.EquipamentoPaiId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipamento_Equipamento_EquipamentoSubstitutoId",
                        column: x => x.EquipamentoSubstitutoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipamento_Modelo_ModeloId",
                        column: x => x.ModeloId,
                        principalSchema: "frota",
                        principalTable: "Modelo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lead",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    NomeContato = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeEmpresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Documento = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    Interesse = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    OrigemId = table.Column<int>(type: "int", nullable: false),
                    LinhaNegocioId = table.Column<int>(type: "int", nullable: true),
                    PayloadOriginal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
                    QualificadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    QualificadoPorId = table.Column<long>(type: "bigint", nullable: true),
                    ClienteGeradoId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoGeradoId = table.Column<long>(type: "bigint", nullable: true),
                    ProcessoGeradoId = table.Column<long>(type: "bigint", nullable: true),
                    MotivoDescarteId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Lead", x => x.Id);
                    table.CheckConstraint("CK_Lead_PayloadOriginalJson", "[PayloadOriginal] IS NULL OR ISJSON([PayloadOriginal]) = 1");
                    table.CheckConstraint("CK_Lead_Qualificacao", "[Situacao] <> 'Qualificado' OR ([QualificadoEm] IS NOT NULL AND [QualificadoPorId] IS NOT NULL AND [ClienteGeradoId] IS NOT NULL)");
                    table.CheckConstraint("CK_Lead_Situacao", "[Situacao] IN ('Novo','EmContato','Qualificado','Descartado','Duplicado')");
                    table.ForeignKey(
                        name: "FK_Lead_CatalogoItem_MotivoDescarteId",
                        column: x => x.MotivoDescarteId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_CatalogoItem_OrigemId",
                        column: x => x.OrigemId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Cliente_ClienteGeradoId",
                        column: x => x.ClienteGeradoId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Contato_ContatoGeradoId",
                        column: x => x.ContatoGeradoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_LinhaDeNegocio_LinhaNegocioId",
                        column: x => x.LinhaNegocioId,
                        principalSchema: "organizacao",
                        principalTable: "LinhaDeNegocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Processo_ProcessoGeradoId",
                        column: x => x.ProcessoGeradoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Usuario_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_Usuario_QualificadoPorId",
                        column: x => x.QualificadoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resultado",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoTarefaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Classe = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    FaseDestinoId = table.Column<int>(type: "int", nullable: true),
                    ExigeJustificativa = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoUsoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resultado", x => x.Id);
                    table.CheckConstraint("CK_Resultado_Classe", "[Classe] IN ('Avanco','Manutencao','Perda','Cancelamento')");
                    table.ForeignKey(
                        name: "FK_Resultado_Fase_FaseDestinoId",
                        column: x => x.FaseDestinoId,
                        principalSchema: "processo",
                        principalTable: "Fase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resultado_TipoTarefa_TipoTarefaId",
                        column: x => x.TipoTarefaId,
                        principalSchema: "processo",
                        principalTable: "TipoTarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemDeProposta",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    ModeloId = table.Column<int>(type: "int", nullable: true),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DescontoPercentual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CondicaoPagamentoId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ItemDeProposta", x => x.Id);
                    table.CheckConstraint("CK_ItemDeProposta_Desconto", "[DescontoPercentual] IS NULL OR [DescontoPercentual] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_ItemDeProposta_Quantidade", "[Quantidade] > 0");
                    table.CheckConstraint("CK_ItemDeProposta_ValorTotal", "[ValorTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_ItemDeProposta_CatalogoItem_CondicaoPagamentoId",
                        column: x => x.CondicaoPagamentoId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemDeProposta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemDeProposta_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemDeProposta_Modelo_ModeloId",
                        column: x => x.ModeloId,
                        principalSchema: "frota",
                        principalTable: "Modelo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemDeProposta_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeituraDeHorimetro",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: false),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    Horas = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Fonte = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    RegistradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeituraDeHorimetro", x => x.Id);
                    table.CheckConstraint("CK_LeituraDeHorimetro_Horas", "[Horas] >= 0");
                    table.ForeignKey(
                        name: "FK_LeituraDeHorimetro_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeituraDeHorimetro_Usuario_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regra",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Evento = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: true),
                    TipoTarefaId = table.Column<int>(type: "int", nullable: true),
                    ResultadoId = table.Column<int>(type: "int", nullable: true),
                    Condicao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Efeito = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EfeitoTipoTarefaId = table.Column<int>(type: "int", nullable: true),
                    EfeitoFaseId = table.Column<int>(type: "int", nullable: true),
                    EfeitoParametros = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrazoDiasUteis = table.Column<int>(type: "int", nullable: false),
                    ExpressaoDestinatario = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    EhCritica = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regra", x => x.Id);
                    table.CheckConstraint("CK_Regra_Efeito", "[Efeito] IN ('CriarTarefa','MoverFase','EncerrarProcesso','Notificar','ChamarWebhook','AtribuirCarteira')");
                    table.CheckConstraint("CK_Regra_EfeitoParametrosJson", "[EfeitoParametros] IS NULL OR ISJSON([EfeitoParametros]) = 1");
                    table.ForeignKey(
                        name: "FK_Regra_Fase_EfeitoFaseId",
                        column: x => x.EfeitoFaseId,
                        principalSchema: "processo",
                        principalTable: "Fase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Regra_Resultado_ResultadoId",
                        column: x => x.ResultadoId,
                        principalSchema: "processo",
                        principalTable: "Resultado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Regra_TipoProcesso_TipoProcessoId",
                        column: x => x.TipoProcessoId,
                        principalSchema: "processo",
                        principalTable: "TipoProcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Regra_TipoTarefa_EfeitoTipoTarefaId",
                        column: x => x.EfeitoTipoTarefaId,
                        principalSchema: "processo",
                        principalTable: "TipoTarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Regra_TipoTarefa_TipoTarefaId",
                        column: x => x.TipoTarefaId,
                        principalSchema: "processo",
                        principalTable: "TipoTarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interacao",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    TipoTarefaId = table.Column<int>(type: "int", nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    LeadId = table.Column<long>(type: "bigint", nullable: true),
                    TarefaId = table.Column<long>(type: "bigint", nullable: true),
                    Assunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detalhe = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResultadoId = table.Column<int>(type: "int", nullable: true),
                    ResultadoComplemento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Natureza = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    OcorridaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    RegistradoPorId = table.Column<long>(type: "bigint", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interacao", x => x.Id);
                    table.CheckConstraint("CK_Interacao_Coordenada", "([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)");
                    table.CheckConstraint("CK_Interacao_Duracao", "[DuracaoMinutos] IS NULL OR [DuracaoMinutos] >= 0");
                    table.CheckConstraint("CK_Interacao_Natureza", "[Natureza] IN ('Ativa','Receptiva','Sistema')");
                    table.CheckConstraint("CK_Interacao_TemVinculo", "[ProcessoId] IS NOT NULL OR [ClienteId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [LeadId] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Interacao_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_Lead_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "comercial",
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_Resultado_ResultadoId",
                        column: x => x.ResultadoId,
                        principalSchema: "processo",
                        principalTable: "Resultado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_TipoTarefa_TipoTarefaId",
                        column: x => x.TipoTarefaId,
                        principalSchema: "processo",
                        principalTable: "TipoTarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interacao_Usuario_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteracaoParticipante",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteracaoId = table.Column<long>(type: "bigint", nullable: false),
                    Papel = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    NomeExterno = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteracaoParticipante", x => x.Id);
                    table.CheckConstraint("CK_InteracaoParticipante_Identificado", "[UsuarioId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [NomeExterno] IS NOT NULL");
                    table.CheckConstraint("CK_InteracaoParticipante_Papel", "[Papel] IN ('Autor','Destinatario','Copia','Participante')");
                    table.ForeignKey(
                        name: "FK_InteracaoParticipante_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteracaoParticipante_Interacao_InteracaoId",
                        column: x => x.InteracaoId,
                        principalSchema: "processo",
                        principalTable: "Interacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteracaoParticipante_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PassagemDeFase",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: false),
                    FaseId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    EntrouEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    SaiuEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    HorasUteis = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    InteracaoOrigemId = table.Column<long>(type: "bigint", nullable: true),
                    RegraOrigemId = table.Column<int>(type: "int", nullable: true),
                    EntrouPorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassagemDeFase", x => x.Id);
                    table.CheckConstraint("CK_PassagemDeFase_Ordem", "[Ordem] >= 1");
                    table.CheckConstraint("CK_PassagemDeFase_Periodo", "[SaiuEm] IS NULL OR [SaiuEm] >= [EntrouEm]");
                    table.ForeignKey(
                        name: "FK_PassagemDeFase_Fase_FaseId",
                        column: x => x.FaseId,
                        principalSchema: "processo",
                        principalTable: "Fase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PassagemDeFase_Interacao_InteracaoOrigemId",
                        column: x => x.InteracaoOrigemId,
                        principalSchema: "processo",
                        principalTable: "Interacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PassagemDeFase_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PassagemDeFase_Regra_RegraOrigemId",
                        column: x => x.RegraOrigemId,
                        principalSchema: "processo",
                        principalTable: "Regra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PassagemDeFase_Usuario_EntrouPorId",
                        column: x => x.EntrouPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tarefa",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    TipoTarefaId = table.Column<int>(type: "int", nullable: false),
                    Assunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detalhe = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponsavelId = table.Column<long>(type: "bigint", nullable: false),
                    ResponsavelEquipeId = table.Column<long>(type: "bigint", nullable: true),
                    OrigemAtribuicao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    AgendadaPara = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    PrazoLimite = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Prioridade = table.Column<short>(type: "smallint", nullable: false),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ConcluidaPorId = table.Column<long>(type: "bigint", nullable: true),
                    ResultadoId = table.Column<int>(type: "int", nullable: true),
                    InteracaoConclusaoId = table.Column<long>(type: "bigint", nullable: true),
                    CriadaPorRegraId = table.Column<int>(type: "int", nullable: true),
                    InteracaoOrigemId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Tarefa", x => x.Id);
                    table.CheckConstraint("CK_Tarefa_Conclusao", "[Situacao] <> 'Concluida' OR ([ConcluidaEm] IS NOT NULL AND [ConcluidaPorId] IS NOT NULL AND [ResultadoId] IS NOT NULL)");
                    table.CheckConstraint("CK_Tarefa_OrigemAtribuicao", "[OrigemAtribuicao] IN ('Regra','Manual','Hierarquia','Carteira','Rodizio')");
                    table.CheckConstraint("CK_Tarefa_Prioridade", "[Prioridade] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Tarefa_Situacao", "[Situacao] IN ('Pendente','EmAndamento','Concluida','Cancelada','Reatribuida')");
                    table.ForeignKey(
                        name: "FK_Tarefa_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Contato_ContatoId",
                        column: x => x.ContatoId,
                        principalSchema: "comercial",
                        principalTable: "Contato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Equipe_ResponsavelEquipeId",
                        column: x => x.ResponsavelEquipeId,
                        principalSchema: "seguranca",
                        principalTable: "Equipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Interacao_InteracaoConclusaoId",
                        column: x => x.InteracaoConclusaoId,
                        principalSchema: "processo",
                        principalTable: "Interacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Interacao_InteracaoOrigemId",
                        column: x => x.InteracaoOrigemId,
                        principalSchema: "processo",
                        principalTable: "Interacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Regra_CriadaPorRegraId",
                        column: x => x.CriadaPorRegraId,
                        principalSchema: "processo",
                        principalTable: "Regra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Resultado_ResultadoId",
                        column: x => x.ResultadoId,
                        principalSchema: "processo",
                        principalTable: "Resultado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_TipoTarefa_TipoTarefaId",
                        column: x => x.TipoTarefaId,
                        principalSchema: "processo",
                        principalTable: "TipoTarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Usuario_ConcluidaPorId",
                        column: x => x.ConcluidaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarefa_Usuario_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preenchimento",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FormularioId = table.Column<int>(type: "int", nullable: false),
                    VersaoFormulario = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    TarefaId = table.Column<long>(type: "bigint", nullable: true),
                    InteracaoId = table.Column<long>(type: "bigint", nullable: true),
                    PreenchidoPorId = table.Column<long>(type: "bigint", nullable: false),
                    PreenchidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preenchimento", x => x.Id);
                    table.CheckConstraint("CK_Preenchimento_Situacao", "[Situacao] IN ('EmAndamento','Concluido','Cancelado')");
                    table.CheckConstraint("CK_Preenchimento_TemVinculo", "[ClienteId] IS NOT NULL OR [ProcessoId] IS NOT NULL OR [TarefaId] IS NOT NULL OR [InteracaoId] IS NOT NULL");
                    table.CheckConstraint("CK_Preenchimento_Versao", "[VersaoFormulario] >= 1");
                    table.ForeignKey(
                        name: "FK_Preenchimento_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preenchimento_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preenchimento_Formulario_FormularioId",
                        column: x => x.FormularioId,
                        principalSchema: "metadado",
                        principalTable: "Formulario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preenchimento_Interacao_InteracaoId",
                        column: x => x.InteracaoId,
                        principalSchema: "processo",
                        principalTable: "Interacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preenchimento_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preenchimento_Tarefa_TarefaId",
                        column: x => x.TarefaId,
                        principalSchema: "processo",
                        principalTable: "Tarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preenchimento_Usuario_PreenchidoPorId",
                        column: x => x.PreenchidoPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegraExecucao",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExecutadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    RegraId = table.Column<int>(type: "int", nullable: false),
                    CorrelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Evento = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    TarefaId = table.Column<long>(type: "bigint", nullable: true),
                    InteracaoId = table.Column<long>(type: "bigint", nullable: true),
                    Resultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TarefaCriadaId = table.Column<long>(type: "bigint", nullable: true),
                    DestinatarioResolvidoId = table.Column<long>(type: "bigint", nullable: true),
                    DuracaoMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegraExecucao", x => new { x.Id, x.ExecutadoEm });
                    table.CheckConstraint("CK_RegraExecucao_Resultado", "[Resultado] IN ('Disparou','CondicaoFalsa','RegraInativa','SemDestinatario','Erro','Suprimida')");
                    table.ForeignKey(
                        name: "FK_RegraExecucao_Interacao_InteracaoId",
                        column: x => x.InteracaoId,
                        principalSchema: "processo",
                        principalTable: "Interacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegraExecucao_Processo_ProcessoId",
                        column: x => x.ProcessoId,
                        principalSchema: "processo",
                        principalTable: "Processo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegraExecucao_Regra_RegraId",
                        column: x => x.RegraId,
                        principalSchema: "processo",
                        principalTable: "Regra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegraExecucao_Tarefa_TarefaCriadaId",
                        column: x => x.TarefaCriadaId,
                        principalSchema: "processo",
                        principalTable: "Tarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegraExecucao_Tarefa_TarefaId",
                        column: x => x.TarefaId,
                        principalSchema: "processo",
                        principalTable: "Tarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegraExecucao_Usuario_DestinatarioResolvidoId",
                        column: x => x.DestinatarioResolvidoId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resposta",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreenchimentoId = table.Column<long>(type: "bigint", nullable: false),
                    PerguntaId = table.Column<int>(type: "int", nullable: false),
                    ValorTexto = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ValorNumero = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ValorData = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ValorBooleano = table.Column<bool>(type: "bit", nullable: true),
                    CatalogoItemId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: true),
                    ValorEstruturado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resposta", x => x.Id);
                    table.CheckConstraint("CK_Resposta_UmValor", "(CASE WHEN [ValorTexto]        IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorNumero]       IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorData]         IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorBooleano]     IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [CatalogoItemId]   IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ClienteId]         IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [EquipamentoId]     IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorEstruturado]  IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.CheckConstraint("CK_Resposta_ValorEstruturadoJson", "[ValorEstruturado] IS NULL OR ISJSON([ValorEstruturado]) = 1");
                    table.ForeignKey(
                        name: "FK_Resposta_CatalogoItem_CatalogoItemId",
                        column: x => x.CatalogoItemId,
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resposta_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resposta_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resposta_Pergunta_PerguntaId",
                        column: x => x.PerguntaId,
                        principalSchema: "metadado",
                        principalTable: "Pergunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resposta_Preenchimento_PreenchimentoId",
                        column: x => x.PreenchimentoId,
                        principalSchema: "metadado",
                        principalTable: "Preenchimento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_ClienteId",
                schema: "comercial",
                table: "Alerta",
                column: "ClienteId",
                filter: "[EstaAtivo] = 1 AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_EmpresaId",
                schema: "comercial",
                table: "Alerta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "UX_Alerta_ChavePublica",
                schema: "comercial",
                table: "Alerta",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlteracaoDeCampo_AlteradoPorId",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                column: "AlteradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AlteracaoDeCampo_CorrelacaoId",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                column: "CorrelacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlteracaoDeCampo_EmpresaId",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AlteracaoDeCampo_Entidade_RegistroId_AlteradoEm",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                columns: new[] { "Entidade", "RegistroId", "AlteradoEm" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "UX_CampoAuditado_Entidade_Campo",
                schema: "auditoria",
                table: "CampoAuditado",
                columns: new[] { "Entidade", "Campo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampoPersonalizado_CatalogoId",
                schema: "metadado",
                table: "CampoPersonalizado",
                column: "CatalogoId");

            migrationBuilder.CreateIndex(
                name: "UX_CampoPersonalizado_Entidade_Campo",
                schema: "metadado",
                table: "CampoPersonalizado",
                columns: new[] { "Entidade", "Campo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CanalContato_ClienteId",
                schema: "comercial",
                table: "CanalContato",
                column: "ClienteId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CanalContato_ContatoId",
                schema: "comercial",
                table: "CanalContato",
                column: "ContatoId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CanalContato_EmpresaId",
                schema: "comercial",
                table: "CanalContato",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CanalContato_ValorNormalizado",
                schema: "comercial",
                table: "CanalContato",
                column: "ValorNormalizado",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_EmpresaId_LinhaDeNegocioId",
                schema: "organizacao",
                table: "Carteira",
                columns: new[] { "EmpresaId", "LinhaDeNegocioId" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_EquipeId",
                schema: "organizacao",
                table: "Carteira",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_LinhaDeNegocioId",
                schema: "organizacao",
                table: "Carteira",
                column: "LinhaDeNegocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_PracaId",
                schema: "organizacao",
                table: "Carteira",
                column: "PracaId");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_ResponsavelId",
                schema: "organizacao",
                table: "Carteira",
                column: "ResponsavelId",
                filter: "[EstaAtiva] = 1 AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_SupervisorId",
                schema: "organizacao",
                table: "Carteira",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "UX_Carteira_ChavePublica",
                schema: "organizacao",
                table: "Carteira",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Carteira_Codigo",
                schema: "organizacao",
                table: "Carteira",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Catalogo_Codigo",
                schema: "metadado",
                table: "Catalogo",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_CatalogoId_Ordem",
                schema: "metadado",
                table: "CatalogoItem",
                columns: new[] { "CatalogoId", "Ordem" },
                filter: "[EstaAtivo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem",
                column: "ItemPaiId");

            migrationBuilder.CreateIndex(
                name: "UX_CatalogoItem_Catalogo_Codigo",
                schema: "metadado",
                table: "CatalogoItem",
                columns: new[] { "CatalogoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CatalogoItem_Catalogo_Descricao",
                schema: "metadado",
                table: "CatalogoItem",
                columns: new[] { "CatalogoId", "Descricao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChaveExterna_Entidade_RegistroId",
                schema: "integracao",
                table: "ChaveExterna",
                columns: new[] { "Entidade", "RegistroId" });

            migrationBuilder.CreateIndex(
                name: "UX_ChaveExterna_Sistema_Entidade_Chave",
                schema: "integracao",
                table: "ChaveExterna",
                columns: new[] { "SistemaId", "Entidade", "ChaveOrigem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ClienteMatrizId",
                schema: "comercial",
                table: "Cliente",
                column: "ClienteMatrizId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_EmpresaId_Situacao",
                schema: "comercial",
                table: "Cliente",
                columns: new[] { "EmpresaId", "Situacao" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                column: "MotivoInativacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_NomeRazao",
                schema: "comercial",
                table: "Cliente",
                column: "NomeRazao",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_OrigemId",
                schema: "comercial",
                table: "Cliente",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente",
                column: "ProprietarioEquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ProprietarioId",
                schema: "comercial",
                table: "Cliente",
                column: "ProprietarioId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Cliente_ChavePublica",
                schema: "comercial",
                table: "Cliente",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Cliente_Empresa_Documento",
                schema: "comercial",
                table: "Cliente",
                columns: new[] { "EmpresaId", "Documento" },
                unique: true,
                filter: "[Documento] IS NOT NULL AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteCarteira_CarteiraId_Classe_UltimaInteracaoEm",
                schema: "comercial",
                table: "ClienteCarteira",
                columns: new[] { "CarteiraId", "Classe", "UltimaInteracaoEm" },
                filter: "[DesvinculadoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_ClienteCarteira_Cliente_Carteira_Vigente",
                schema: "comercial",
                table: "ClienteCarteira",
                columns: new[] { "ClienteId", "CarteiraId" },
                unique: true,
                filter: "[DesvinculadoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteContato_ContatoId",
                schema: "comercial",
                table: "ClienteContato",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteContato_PapelId",
                schema: "comercial",
                table: "ClienteContato",
                column: "PapelId");

            migrationBuilder.CreateIndex(
                name: "UX_ClienteContato_Cliente_Contato_Papel",
                schema: "comercial",
                table: "ClienteContato",
                columns: new[] { "ClienteId", "ContatoId", "PapelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ClienteContato_Principal",
                schema: "comercial",
                table: "ClienteContato",
                column: "ClienteId",
                unique: true,
                filter: "[EhPrincipal] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CompartilhamentoDeRegistro_EmpresaId",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CompartilhamentoDeRegistro_Entidade_RegistroId",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                columns: new[] { "Entidade", "RegistroId" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompartilhamentoDeRegistro_EquipeId_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                columns: new[] { "EquipeId", "Entidade" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompartilhamentoDeRegistro_UsuarioId_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                columns: new[] { "UsuarioId", "Entidade" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_CompartilhamentoDeRegistro_ChavePublica",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                column: "ChavePublica",
                unique: true);

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
                name: "IX_ConsentimentoComunicacao_ClienteId_Canal_Finalidade_DecididaEm",
                schema: "comercial",
                table: "ConsentimentoComunicacao",
                columns: new[] { "ClienteId", "Canal", "Finalidade", "DecididaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimentoComunicacao_ContatoId_Canal_Finalidade_DecididaEm",
                schema: "comercial",
                table: "ConsentimentoComunicacao",
                columns: new[] { "ContatoId", "Canal", "Finalidade", "DecididaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimentoComunicacao_EmpresaId",
                schema: "comercial",
                table: "ConsentimentoComunicacao",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contato_EmpresaId",
                schema: "comercial",
                table: "Contato",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contato_ProprietarioId",
                schema: "comercial",
                table: "Contato",
                column: "ProprietarioId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Contato_ChavePublica",
                schema: "comercial",
                table: "Contato",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Contato_Empresa_Documento",
                schema: "comercial",
                table: "Contato",
                columns: new[] { "EmpresaId", "Documento" },
                unique: true,
                filter: "[Documento] IS NOT NULL AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_EmpresaId",
                schema: "documento",
                table: "Documento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_ResumoConteudo",
                schema: "documento",
                table: "Documento",
                column: "ResumoConteudo",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_TipoDocumentoId",
                schema: "documento",
                table: "Documento",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "UX_Documento_ChavePublica",
                schema: "documento",
                table: "Documento",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_Caminho",
                schema: "organizacao",
                table: "Empresa",
                column: "Caminho",
                filter: "[EstaAtiva] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_EmpresaPaiId",
                schema: "organizacao",
                table: "Empresa",
                column: "EmpresaPaiId");

            migrationBuilder.CreateIndex(
                name: "UX_Empresa_ChavePublica",
                schema: "organizacao",
                table: "Empresa",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Empresa_Codigo",
                schema: "organizacao",
                table: "Empresa",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_CulturaId",
                schema: "comercial",
                table: "Endereco",
                column: "CulturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_EmpresaId",
                schema: "comercial",
                table: "Endereco",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_Uf_Municipio",
                schema: "comercial",
                table: "Endereco",
                columns: new[] { "Uf", "Municipio" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Endereco_ChavePublica",
                schema: "comercial",
                table: "Endereco",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Endereco_Principal",
                schema: "comercial",
                table: "Endereco",
                column: "ClienteId",
                unique: true,
                filter: "[EhPrincipal] = 1 AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_ClienteId_Origem",
                schema: "frota",
                table: "Equipamento",
                columns: new[] { "ClienteId", "Origem" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_EmpresaId",
                schema: "frota",
                table: "Equipamento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_EnderecoId",
                schema: "frota",
                table: "Equipamento",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_EquipamentoPaiId",
                schema: "frota",
                table: "Equipamento",
                column: "EquipamentoPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_EquipamentoSubstitutoId",
                schema: "frota",
                table: "Equipamento",
                column: "EquipamentoSubstitutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_ModeloId",
                schema: "frota",
                table: "Equipamento",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "UX_Equipamento_Chassi",
                schema: "frota",
                table: "Equipamento",
                column: "Chassi",
                unique: true,
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Equipamento_ChavePublica",
                schema: "frota",
                table: "Equipamento",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Equipe_ChavePublica",
                schema: "seguranca",
                table: "Equipe",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Equipe_Empresa_Nome",
                schema: "seguranca",
                table: "Equipe",
                columns: new[] { "EmpresaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipeMembro_UsuarioId",
                schema: "seguranca",
                table: "EquipeMembro",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_EquipeMembro_Equipe_Usuario_Vigente",
                schema: "seguranca",
                table: "EquipeMembro",
                columns: new[] { "EquipeId", "UsuarioId" },
                unique: true,
                filter: "[SaiuEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventoDeAcesso_Entidade_RegistroId_OcorreuEm",
                schema: "auditoria",
                table: "EventoDeAcesso",
                columns: new[] { "Entidade", "RegistroId", "OcorreuEm" },
                descending: new[] { false, false, true },
                filter: "[Entidade] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventoDeAcesso_Tipo_OcorreuEm",
                schema: "auditoria",
                table: "EventoDeAcesso",
                columns: new[] { "Tipo", "OcorreuEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_EventoDeAcesso_UsuarioId_OcorreuEm",
                schema: "auditoria",
                table: "EventoDeAcesso",
                columns: new[] { "UsuarioId", "OcorreuEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_Familia_Marca_Codigo",
                schema: "frota",
                table: "Familia",
                columns: new[] { "MarcaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fase_TipoProcessoId_Ordem",
                schema: "processo",
                table: "Fase",
                columns: new[] { "TipoProcessoId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "UX_Fase_Tipo_Codigo",
                schema: "processo",
                table: "Fase",
                columns: new[] { "TipoProcessoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fonte_SistemaId",
                schema: "relatorio",
                table: "Fonte",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "UX_Fonte_Codigo",
                schema: "relatorio",
                table: "Fonte",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_FonteCampo_Fonte_Campo",
                schema: "relatorio",
                table: "FonteCampo",
                columns: new[] { "FonteId", "Campo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Formulario_TipoProcessoId",
                schema: "metadado",
                table: "Formulario",
                column: "TipoProcessoId");

            migrationBuilder.CreateIndex(
                name: "UX_Formulario_Codigo",
                schema: "metadado",
                table: "Formulario",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HierarquiaComercial_DescendenteId_Profundidade",
                schema: "organizacao",
                table: "HierarquiaComercial",
                columns: new[] { "DescendenteId", "Profundidade" });

            migrationBuilder.CreateIndex(
                name: "UX_HierarquiaComercial_Ancestral_Descendente",
                schema: "organizacao",
                table: "HierarquiaComercial",
                columns: new[] { "AncestralId", "DescendenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_ClienteId_OcorridaEm",
                schema: "processo",
                table: "Interacao",
                columns: new[] { "ClienteId", "OcorridaEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_ContatoId",
                schema: "processo",
                table: "Interacao",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_EmpresaId",
                schema: "processo",
                table: "Interacao",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_LeadId",
                schema: "processo",
                table: "Interacao",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_ProcessoId_OcorridaEm",
                schema: "processo",
                table: "Interacao",
                columns: new[] { "ProcessoId", "OcorridaEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_RegistradoPorId",
                schema: "processo",
                table: "Interacao",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_ResultadoId",
                schema: "processo",
                table: "Interacao",
                column: "ResultadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_TarefaId",
                schema: "processo",
                table: "Interacao",
                column: "TarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_TipoTarefaId",
                schema: "processo",
                table: "Interacao",
                column: "TipoTarefaId");

            migrationBuilder.CreateIndex(
                name: "UX_Interacao_ChavePublica",
                schema: "processo",
                table: "Interacao",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteracaoParticipante_ContatoId",
                schema: "processo",
                table: "InteracaoParticipante",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_InteracaoParticipante_InteracaoId",
                schema: "processo",
                table: "InteracaoParticipante",
                column: "InteracaoId");

            migrationBuilder.CreateIndex(
                name: "IX_InteracaoParticipante_UsuarioId",
                schema: "processo",
                table: "InteracaoParticipante",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDeProposta_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                column: "CondicaoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDeProposta_EmpresaId",
                schema: "processo",
                table: "ItemDeProposta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDeProposta_EquipamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                column: "EquipamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDeProposta_ModeloId",
                schema: "processo",
                table: "ItemDeProposta",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "UX_ItemDeProposta_ChavePublica",
                schema: "processo",
                table: "ItemDeProposta",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ItemDeProposta_Processo_Ordem",
                schema: "processo",
                table: "ItemDeProposta",
                columns: new[] { "ProcessoId", "Ordem" },
                unique: true,
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_ClienteGeradoId",
                schema: "comercial",
                table: "Lead",
                column: "ClienteGeradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_ContatoGeradoId",
                schema: "comercial",
                table: "Lead",
                column: "ContatoGeradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_Documento",
                schema: "comercial",
                table: "Lead",
                column: "Documento",
                filter: "[Documento] IS NOT NULL AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_Email",
                schema: "comercial",
                table: "Lead",
                column: "Email",
                filter: "[Email] IS NOT NULL AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_EmpresaId_Situacao_CriadoEm",
                schema: "comercial",
                table: "Lead",
                columns: new[] { "EmpresaId", "Situacao", "CriadoEm" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_LinhaNegocioId",
                schema: "comercial",
                table: "Lead",
                column: "LinhaNegocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_MotivoDescarteId",
                schema: "comercial",
                table: "Lead",
                column: "MotivoDescarteId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_OrigemId",
                schema: "comercial",
                table: "Lead",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_ProcessoGeradoId",
                schema: "comercial",
                table: "Lead",
                column: "ProcessoGeradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_ProprietarioId_Situacao",
                schema: "comercial",
                table: "Lead",
                columns: new[] { "ProprietarioId", "Situacao" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_QualificadoPorId",
                schema: "comercial",
                table: "Lead",
                column: "QualificadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_Lead_ChavePublica",
                schema: "comercial",
                table: "Lead",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeituraDeHorimetro_RegistradoPorId",
                schema: "frota",
                table: "LeituraDeHorimetro",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_LeituraDeHorimetro_Equipamento_Instante",
                schema: "frota",
                table: "LeituraDeHorimetro",
                columns: new[] { "EquipamentoId", "LidaEm" },
                unique: true,
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UX_LinhaDeNegocio_Codigo",
                schema: "organizacao",
                table: "LinhaDeNegocio",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Marca_Codigo",
                schema: "frota",
                table: "Marca",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDeSaida_CorrelacaoId",
                schema: "integracao",
                table: "MensagemDeSaida",
                column: "CorrelacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDeSaida_ProximaTentativaEm",
                schema: "integracao",
                table: "MensagemDeSaida",
                column: "ProximaTentativaEm",
                filter: "[Situacao] = 'Pendente'");

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDeSaida_SistemaId",
                schema: "integracao",
                table: "MensagemDeSaida",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDescartada_Fluxo_DescartadaEm",
                schema: "integracao",
                table: "MensagemDescartada",
                columns: new[] { "Fluxo", "DescartadaEm" },
                filter: "[TratadaEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDescartada_MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada",
                column: "MensagemDeSaidaId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDescartada_TratadaPorId",
                schema: "integracao",
                table: "MensagemDescartada",
                column: "TratadaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Meta_CarteiraId",
                schema: "organizacao",
                table: "Meta",
                column: "CarteiraId");

            migrationBuilder.CreateIndex(
                name: "IX_Meta_EmpresaId_Tipo_PeriodoInicio",
                schema: "organizacao",
                table: "Meta",
                columns: new[] { "EmpresaId", "Tipo", "PeriodoInicio" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Meta_LinhaDeNegocioId",
                schema: "organizacao",
                table: "Meta",
                column: "LinhaDeNegocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Meta_UsuarioId",
                schema: "organizacao",
                table: "Meta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_Meta_ChavePublica",
                schema: "organizacao",
                table: "Meta",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modelo_FamiliaId",
                schema: "frota",
                table: "Modelo",
                column: "FamiliaId");

            migrationBuilder.CreateIndex(
                name: "UX_Modelo_Codigo",
                schema: "frota",
                table: "Modelo",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MotivoDePerda_Codigo",
                schema: "processo",
                table: "MotivoDePerda",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PassagemDeFase_EntrouPorId",
                schema: "processo",
                table: "PassagemDeFase",
                column: "EntrouPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PassagemDeFase_FaseId",
                schema: "processo",
                table: "PassagemDeFase",
                column: "FaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PassagemDeFase_InteracaoOrigemId",
                schema: "processo",
                table: "PassagemDeFase",
                column: "InteracaoOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_PassagemDeFase_ProcessoId",
                schema: "processo",
                table: "PassagemDeFase",
                column: "ProcessoId",
                filter: "[SaiuEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PassagemDeFase_RegraOrigemId",
                schema: "processo",
                table: "PassagemDeFase",
                column: "RegraOrigemId");

            migrationBuilder.CreateIndex(
                name: "UX_PassagemDeFase_Processo_Ordem",
                schema: "processo",
                table: "PassagemDeFase",
                columns: new[] { "ProcessoId", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pergunta_CatalogoId",
                schema: "metadado",
                table: "Pergunta",
                column: "CatalogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pergunta_FormularioId_Ordem",
                schema: "metadado",
                table: "Pergunta",
                columns: new[] { "FormularioId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_Pergunta_PerguntaCondicaoId",
                schema: "metadado",
                table: "Pergunta",
                column: "PerguntaCondicaoId");

            migrationBuilder.CreateIndex(
                name: "UX_Pergunta_Formulario_Codigo",
                schema: "metadado",
                table: "Pergunta",
                columns: new[] { "FormularioId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Permissao_Codigo",
                schema: "seguranca",
                table: "Permissao",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PontoDeSincronismo_SistemaId",
                schema: "integracao",
                table: "PontoDeSincronismo",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "UX_PontoDeSincronismo_Fluxo",
                schema: "integracao",
                table: "PontoDeSincronismo",
                column: "Fluxo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Praca_LinhaDeNegocioId",
                schema: "organizacao",
                table: "Praca",
                column: "LinhaDeNegocioId");

            migrationBuilder.CreateIndex(
                name: "UX_Praca_Codigo_Linha_Ano",
                schema: "organizacao",
                table: "Praca",
                columns: new[] { "Codigo", "LinhaDeNegocioId", "AnoReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_ClienteId",
                schema: "metadado",
                table: "Preenchimento",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_EmpresaId",
                schema: "metadado",
                table: "Preenchimento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_FormularioId_PreenchidoEm",
                schema: "metadado",
                table: "Preenchimento",
                columns: new[] { "FormularioId", "PreenchidoEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_InteracaoId",
                schema: "metadado",
                table: "Preenchimento",
                column: "InteracaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_PreenchidoPorId",
                schema: "metadado",
                table: "Preenchimento",
                column: "PreenchidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_ProcessoId",
                schema: "metadado",
                table: "Preenchimento",
                column: "ProcessoId");

            migrationBuilder.CreateIndex(
                name: "IX_Preenchimento_TarefaId",
                schema: "metadado",
                table: "Preenchimento",
                column: "TarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_CarteiraId",
                schema: "processo",
                table: "Processo",
                column: "CarteiraId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ClienteId",
                schema: "processo",
                table: "Processo",
                column: "ClienteId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ConcorrenteId",
                schema: "processo",
                table: "Processo",
                column: "ConcorrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ContatoId",
                schema: "processo",
                table: "Processo",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_EmpresaId_FaseId",
                schema: "processo",
                table: "Processo",
                columns: new[] { "EmpresaId", "FaseId" },
                filter: "[Situacao] = 'Aberto' AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_FaseId",
                schema: "processo",
                table: "Processo",
                column: "FaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_MotivoDePerdaId",
                schema: "processo",
                table: "Processo",
                column: "MotivoDePerdaId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ProprietarioEquipeId",
                schema: "processo",
                table: "Processo",
                column: "ProprietarioEquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ProprietarioId_Situacao",
                schema: "processo",
                table: "Processo",
                columns: new[] { "ProprietarioId", "Situacao" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_TipoProcessoId",
                schema: "processo",
                table: "Processo",
                column: "TipoProcessoId");

            migrationBuilder.CreateIndex(
                name: "UX_Processo_ChavePublica",
                schema: "processo",
                table: "Processo",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Processo_Empresa_Numero",
                schema: "processo",
                table: "Processo",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recepcao_Entidade_RecebidaEm",
                schema: "integracao",
                table: "Recepcao",
                columns: new[] { "Entidade", "RecebidaEm" },
                filter: "[Situacao] = 'Recebida'");

            migrationBuilder.CreateIndex(
                name: "IX_Recepcao_ExpurgarApos",
                schema: "integracao",
                table: "Recepcao",
                column: "ExpurgarApos");

            migrationBuilder.CreateIndex(
                name: "IX_Recepcao_SistemaId_ChaveOrigem",
                schema: "integracao",
                table: "Recepcao",
                columns: new[] { "SistemaId", "ChaveOrigem" });

            migrationBuilder.CreateIndex(
                name: "IX_Regra_EfeitoFaseId",
                schema: "processo",
                table: "Regra",
                column: "EfeitoFaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Regra_EfeitoTipoTarefaId",
                schema: "processo",
                table: "Regra",
                column: "EfeitoTipoTarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_Regra_Evento_ResultadoId_Ordem",
                schema: "processo",
                table: "Regra",
                columns: new[] { "Evento", "ResultadoId", "Ordem" },
                filter: "[EstaAtiva] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Regra_ResultadoId",
                schema: "processo",
                table: "Regra",
                column: "ResultadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Regra_TipoProcessoId",
                schema: "processo",
                table: "Regra",
                column: "TipoProcessoId");

            migrationBuilder.CreateIndex(
                name: "IX_Regra_TipoTarefaId",
                schema: "processo",
                table: "Regra",
                column: "TipoTarefaId");

            migrationBuilder.CreateIndex(
                name: "UX_Regra_Codigo",
                schema: "processo",
                table: "Regra",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_CorrelacaoId",
                schema: "processo",
                table: "RegraExecucao",
                column: "CorrelacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_DestinatarioResolvidoId",
                schema: "processo",
                table: "RegraExecucao",
                column: "DestinatarioResolvidoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_InteracaoId",
                schema: "processo",
                table: "RegraExecucao",
                column: "InteracaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_ProcessoId_ExecutadoEm",
                schema: "processo",
                table: "RegraExecucao",
                columns: new[] { "ProcessoId", "ExecutadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_RegraId_ExecutadoEm_Resultado",
                schema: "processo",
                table: "RegraExecucao",
                columns: new[] { "RegraId", "ExecutadoEm", "Resultado" });

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_TarefaCriadaId",
                schema: "processo",
                table: "RegraExecucao",
                column: "TarefaCriadaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegraExecucao_TarefaId",
                schema: "processo",
                table: "RegraExecucao",
                column: "TarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorio_EmpresaId_Visibilidade",
                schema: "relatorio",
                table: "Relatorio",
                columns: new[] { "EmpresaId", "Visibilidade" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorio_FonteId",
                schema: "relatorio",
                table: "Relatorio",
                column: "FonteId");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorio_ProprietarioId_Nome",
                schema: "relatorio",
                table: "Relatorio",
                columns: new[] { "ProprietarioId", "Nome" },
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Relatorio_ChavePublica",
                schema: "relatorio",
                table: "Relatorio",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_CatalogoItemId",
                schema: "metadado",
                table: "Resposta",
                column: "CatalogoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_ClienteId",
                schema: "metadado",
                table: "Resposta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_EquipamentoId",
                schema: "metadado",
                table: "Resposta",
                column: "EquipamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_PerguntaId_CatalogoItemId",
                schema: "metadado",
                table: "Resposta",
                columns: new[] { "PerguntaId", "CatalogoItemId" },
                filter: "[CatalogoItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_PerguntaId_ValorNumero",
                schema: "metadado",
                table: "Resposta",
                columns: new[] { "PerguntaId", "ValorNumero" },
                filter: "[ValorNumero] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_PerguntaId_ValorTexto",
                schema: "metadado",
                table: "Resposta",
                columns: new[] { "PerguntaId", "ValorTexto" },
                filter: "[ValorTexto] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resposta_PreenchimentoId",
                schema: "metadado",
                table: "Resposta",
                column: "PreenchimentoId");

            migrationBuilder.CreateIndex(
                name: "UX_Resposta_Preenchimento_Pergunta",
                schema: "metadado",
                table: "Resposta",
                columns: new[] { "PreenchimentoId", "PerguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resultado_FaseDestinoId",
                schema: "processo",
                table: "Resultado",
                column: "FaseDestinoId");

            migrationBuilder.CreateIndex(
                name: "UX_Resultado_TipoTarefa_Codigo",
                schema: "processo",
                table: "Resultado",
                columns: new[] { "TipoTarefaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Sistema_Codigo",
                schema: "integracao",
                table: "Sistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ClienteId",
                schema: "processo",
                table: "Tarefa",
                column: "ClienteId",
                filter: "[Situacao] <> 'Concluida' AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ConcluidaPorId",
                schema: "processo",
                table: "Tarefa",
                column: "ConcluidaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ContatoId",
                schema: "processo",
                table: "Tarefa",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa",
                column: "CriadaPorRegraId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_EmpresaId",
                schema: "processo",
                table: "Tarefa",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_InteracaoConclusaoId",
                schema: "processo",
                table: "Tarefa",
                column: "InteracaoConclusaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_InteracaoOrigemId",
                schema: "processo",
                table: "Tarefa",
                column: "InteracaoOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ProcessoId",
                schema: "processo",
                table: "Tarefa",
                column: "ProcessoId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa",
                column: "ResponsavelEquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ResponsavelId_AgendadaPara",
                schema: "processo",
                table: "Tarefa",
                columns: new[] { "ResponsavelId", "AgendadaPara" },
                filter: "[Situacao] IN ('Pendente','EmAndamento') AND [ExcluidoEm] IS NULL")
                .Annotation("SqlServer:Include", new[] { "Assunto", "Prioridade", "ProcessoId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ResultadoId",
                schema: "processo",
                table: "Tarefa",
                column: "ResultadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_TipoTarefaId",
                schema: "processo",
                table: "Tarefa",
                column: "TipoTarefaId");

            migrationBuilder.CreateIndex(
                name: "UX_Tarefa_ChavePublica",
                schema: "processo",
                table: "Tarefa",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoProcesso_LinhaDeNegocioId",
                schema: "processo",
                table: "TipoProcesso",
                column: "LinhaDeNegocioId");

            migrationBuilder.CreateIndex(
                name: "UX_TipoProcesso_Codigo_Versao",
                schema: "processo",
                table: "TipoProcesso",
                columns: new[] { "Codigo", "Versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoTarefa_FormularioId",
                schema: "processo",
                table: "TipoTarefa",
                column: "FormularioId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoTarefa_TipoProcessoId",
                schema: "processo",
                table: "TipoTarefa",
                column: "TipoProcessoId");

            migrationBuilder.CreateIndex(
                name: "UX_TipoTarefa_Codigo",
                schema: "processo",
                table: "TipoTarefa",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TratadorDeEvento_Evento_Momento_Ordem",
                schema: "metadado",
                table: "TratadorDeEvento",
                columns: new[] { "Evento", "Momento", "Ordem" },
                filter: "[EstaAtivo] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_TratadorDeEvento_Evento_Tipo_Momento",
                schema: "metadado",
                table: "TratadorDeEvento",
                columns: new[] { "Evento", "TipoImplementacao", "Momento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_EmpresaId",
                schema: "seguranca",
                table: "Usuario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_GestorId",
                schema: "seguranca",
                table: "Usuario",
                column: "GestorId",
                filter: "[EstaAtivo] = 1 AND [ExcluidoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Usuario_ChavePublica",
                schema: "seguranca",
                table: "Usuario",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Usuario_IdentidadeExterna",
                schema: "seguranca",
                table: "Usuario",
                column: "IdentidadeExterna",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Usuario_NomePrincipal",
                schema: "seguranca",
                table: "Usuario",
                column: "NomePrincipal",
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

            migrationBuilder.CreateIndex(
                name: "IX_Vinculo_Entidade_RegistroId",
                schema: "documento",
                table: "Vinculo",
                columns: new[] { "Entidade", "RegistroId" });

            migrationBuilder.CreateIndex(
                name: "IX_Vinculo_VinculadoPorId",
                schema: "documento",
                table: "Vinculo",
                column: "VinculadoPorId");

            migrationBuilder.CreateIndex(
                name: "UX_Vinculo_Documento_Entidade_Registro",
                schema: "documento",
                table: "Vinculo",
                columns: new[] { "DocumentoId", "Entidade", "RegistroId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Interacao_Tarefa_TarefaId",
                schema: "processo",
                table: "Interacao",
                column: "TarefaId",
                principalSchema: "processo",
                principalTable: "Tarefa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Escrita a mao (ver ModeloInicial.Particionamento.cs). Vem por ultimo: converte as
            // quatro tabelas de log e area de pouso em particionadas por mes, ja com os indices
            // alinhados. Se voce regenerar esta migration, esta linha precisa voltar.
            ParticionarTabelasDeLogEAuditoria(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Cliente_ClienteId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Cliente_ClienteGeradoId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Cliente_ClienteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Cliente_ClienteId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Empresa_EmpresaId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Contato_Empresa_EmpresaId",
                schema: "comercial",
                table: "Contato");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipe_Empresa_EmpresaId",
                schema: "seguranca",
                table: "Equipe");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Empresa_EmpresaId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Empresa_EmpresaId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Empresa_EmpresaId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Empresa_EmpresaId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Empresa_EmpresaId",
                schema: "seguranca",
                table: "Usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Usuario_ResponsavelId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Usuario_SupervisorId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Contato_Usuario_ProprietarioId",
                schema: "comercial",
                table: "Contato");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Usuario_RegistradoPorId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Usuario_ProprietarioId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Usuario_QualificadoPorId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Usuario_ProprietarioId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Usuario_ConcluidaPorId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Usuario_ResponsavelId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogoItem_Catalogo_CatalogoId",
                schema: "metadado",
                table: "CatalogoItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Contato_ContatoId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Contato_ContatoGeradoId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Contato_ContatoId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Contato_ContatoId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Equipe_EquipeId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Equipe_ProprietarioEquipeId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Equipe_ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_LinhaDeNegocio_LinhaDeNegocioId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_LinhaDeNegocio_LinhaNegocioId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Praca_LinhaDeNegocio_LinhaDeNegocioId",
                schema: "organizacao",
                table: "Praca");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoProcesso_LinhaDeNegocio_LinhaDeNegocioId",
                schema: "processo",
                table: "TipoProcesso");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Praca_PracaId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_CatalogoItem_MotivoDescarteId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_CatalogoItem_OrigemId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_CatalogoItem_ConcorrenteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Carteira_CarteiraId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Fase_TipoProcesso_TipoProcessoId",
                schema: "processo",
                table: "Fase");

            migrationBuilder.DropForeignKey(
                name: "FK_Formulario_TipoProcesso_TipoProcessoId",
                schema: "metadado",
                table: "Formulario");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_TipoProcesso_TipoProcessoId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Regra_TipoProcesso_TipoProcessoId",
                schema: "processo",
                table: "Regra");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoTarefa_TipoProcesso_TipoProcessoId",
                schema: "processo",
                table: "TipoTarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Lead_LeadId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Processo_ProcessoId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Processo_ProcessoId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Resultado_ResultadoId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Regra_Resultado_ResultadoId",
                schema: "processo",
                table: "Regra");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Resultado_ResultadoId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Tarefa_TarefaId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropTable(
                name: "Alerta",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "AlteracaoDeCampo",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "CampoAuditado",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "CampoPersonalizado",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "CanalContato",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "ChaveExterna",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "ClienteCarteira",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "ClienteContato",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "CompartilhamentoDeRegistro",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "ConjuntoDePermissaoItem",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "ConsentimentoComunicacao",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "EquipeMembro",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "EventoDeAcesso",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "FonteCampo",
                schema: "relatorio");

            migrationBuilder.DropTable(
                name: "HierarquiaComercial",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "InteracaoParticipante",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "ItemDeProposta",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "LeituraDeHorimetro",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "MensagemDescartada",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "Meta",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "PassagemDeFase",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Permissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "PontoDeSincronismo",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "Recepcao",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "RegraExecucao",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Relatorio",
                schema: "relatorio");

            migrationBuilder.DropTable(
                name: "Resposta",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "TratadorDeEvento",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "UsuarioConjuntoDePermissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "Vinculo",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "MensagemDeSaida",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "Fonte",
                schema: "relatorio");

            migrationBuilder.DropTable(
                name: "Equipamento",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "Pergunta",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "Preenchimento",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "ConjuntoDePermissao",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "Documento",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "Sistema",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "Endereco",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "Modelo",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "Familia",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "Marca",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "Cliente",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "Empresa",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "Usuario",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "Catalogo",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "Contato",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "Equipe",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "LinhaDeNegocio",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "Praca",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "CatalogoItem",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "Carteira",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "TipoProcesso",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Lead",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "Processo",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "MotivoDePerda",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Resultado",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Tarefa",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Interacao",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Regra",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Fase",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "TipoTarefa",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Formulario",
                schema: "metadado");

            // Escrita a mao (ver ModeloInicial.Particionamento.cs). DROP TABLE nao leva junto a
            // funcao nem o esquema de particao: sem esta limpeza, reaplicar a migration
            // falharia em "ja existe". Se voce regenerar esta migration, esta linha precisa
            // voltar.
            DesfazerParticionamento(migrationBuilder);
        }
    }
}
