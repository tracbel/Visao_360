using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class IntegracaoDoArtLinhaDeProdutoVendaEVinculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_Origem",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_Situacao",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.AlterColumn<string>(
                name: "Situacao",
                schema: "frota",
                table: "Equipamento",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "ModeloId",
                schema: "frota",
                table: "Equipamento",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LinhaDeProdutoId",
                schema: "frota",
                table: "Equipamento",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompradorPendente",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Documento = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    TipoDePessoa = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    NomeNaOrigem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Grupo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Situacao = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Vendas = table.Column<int>(type: "int", nullable: false),
                    VendasComChassiValido = table.Column<int>(type: "int", nullable: false),
                    PrimeiraVendaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    UltimaVendaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    FiliaisDasVendas = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    NotasNoProtheus = table.Column<int>(type: "int", nullable: false),
                    NaturezaNasNotas = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    PrimeiraNotaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    UltimaNotaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    NomeDaNotaCoincide = table.Column<bool>(type: "bit", nullable: false),
                    SituacaoNoCadastroDoProtheus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TemEnderecoNoProtheus = table.Column<bool>(type: "bit", nullable: false),
                    TemMunicipioNoProtheus = table.Column<bool>(type: "bit", nullable: false),
                    TemInscricaoEstadualNoProtheus = table.Column<bool>(type: "bit", nullable: false),
                    NomeDoCadastroCoincide = table.Column<bool>(type: "bit", nullable: false),
                    DadosQueFaltam = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
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
                    table.PrimaryKey("PK_CompradorPendente", x => x.Id);
                    table.CheckConstraint("CK_CompradorPendente_Contadores", "[Vendas] >= 0 AND [VendasComChassiValido] >= 0 AND [VendasComChassiValido] <= [Vendas] AND [NotasNoProtheus] >= 0");
                    table.CheckConstraint("CK_CompradorPendente_Documento", "([TipoDePessoa] = 'Fisica' AND LEN([Documento]) = 11) OR ([TipoDePessoa] = 'Juridica' AND LEN([Documento]) = 14)");
                    table.CheckConstraint("CK_CompradorPendente_Grupo", "[Grupo] IN ('ComNotaNoProtheus','SemNotaNoProtheus')");
                    table.CheckConstraint("CK_CompradorPendente_Situacao", "[Situacao] IN ('AguardandoCadastro','Cadastrado')");
                    table.CheckConstraint("CK_CompradorPendente_SituacaoNoCadastroDoProtheus", "[SituacaoNoCadastroDoProtheus] IN ('NaoConferido','Ausente','Ativo','Bloqueado')");
                    table.CheckConstraint("CK_CompradorPendente_TipoDePessoa", "[TipoDePessoa] IN ('Fisica','Juridica')");
                    table.ForeignKey(
                        name: "FK_CompradorPendente_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompradorPendente_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LinhaDeProduto",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FamiliaId = table.Column<int>(type: "int", nullable: true),
                    Porte = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhaDeProduto", x => x.Id);
                    table.CheckConstraint("CK_LinhaDeProduto_Porte", "[Porte] IN ('NaoSeAplica','Pequeno','Medio','Grande')");
                    table.ForeignKey(
                        name: "FK_LinhaDeProduto_Familia_FamiliaId",
                        column: x => x.FamiliaId,
                        principalSchema: "frota",
                        principalTable: "Familia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendaDeMaquina",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EmpresaDoFaturamentoId = table.Column<int>(type: "int", nullable: true),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: false),
                    CompradorId = table.Column<long>(type: "bigint", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    ChaveOrigem = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    VendidaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    FaturadaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    EntregueEm = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistradaNaOrigemEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    NumeroDoPedido = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    NumeroDaNotaFiscal = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    SituacaoNaOrigem = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    GestaoNaOrigem = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    VendaDireta = table.Column<bool>(type: "bit", nullable: false),
                    RepasseDireto = table.Column<bool>(type: "bit", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: true),
                    LinhaNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProdutoNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EmpresaNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    UnidadeNaOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UnidadeDoFaturamentoNaOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    HashDaOrigem = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Transformacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImportadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    AtualizadaPelaOrigemEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
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
                    table.PrimaryKey("PK_VendaDeMaquina", x => x.Id);
                    table.CheckConstraint("CK_VendaDeMaquina_Quantidade", "[Quantidade] IS NULL OR [Quantidade] >= 0");
                    table.ForeignKey(
                        name: "FK_VendaDeMaquina_Cliente_CompradorId",
                        column: x => x.CompradorId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaDeMaquina_Empresa_EmpresaDoFaturamentoId",
                        column: x => x.EmpresaDoFaturamentoId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaDeMaquina_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaDeMaquina_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaDeMaquina_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorrespondenciaDaOrigem",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CodigoNaOrigem = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    TextoNaOrigem = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ContextoNaOrigem = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LinhaDeProdutoId = table.Column<int>(type: "int", nullable: true),
                    ModeloId = table.Column<int>(type: "int", nullable: true),
                    EmpresaCorrespondenteId = table.Column<int>(type: "int", nullable: true),
                    Situacao = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Criterio = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Ocorrencias = table.Column<int>(type: "int", nullable: false),
                    PrimeiraLeituraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UltimaLeituraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    RevisadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    RevisadaPorId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrespondenciaDaOrigem", x => x.Id);
                    table.CheckConstraint("CK_CorrespondenciaDaOrigem_Ocorrencias", "[Ocorrencias] >= 0");
                    table.CheckConstraint("CK_CorrespondenciaDaOrigem_Revisao", "([RevisadaEm] IS NULL AND [RevisadaPorId] IS NULL) OR ([RevisadaEm] IS NOT NULL AND [RevisadaPorId] IS NOT NULL)");
                    table.CheckConstraint("CK_CorrespondenciaDaOrigem_Situacao", "[Situacao] IN ('CorrespondenciaExata','PendenteDeRevisao','NaoEClassificacaoDeProduto','ConfirmadaPorRevisao','RecusadaPorRevisao')");
                    table.CheckConstraint("CK_CorrespondenciaDaOrigem_Tipo", "[Tipo] IN ('LinhaDeProduto','Produto','Unidade')");
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDaOrigem_Empresa_EmpresaCorrespondenteId",
                        column: x => x.EmpresaCorrespondenteId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDaOrigem_LinhaDeProduto_LinhaDeProdutoId",
                        column: x => x.LinhaDeProdutoId,
                        principalSchema: "frota",
                        principalTable: "LinhaDeProduto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDaOrigem_Modelo_ModeloId",
                        column: x => x.ModeloId,
                        principalSchema: "frota",
                        principalTable: "Modelo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDaOrigem_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrespondenciaDaOrigem_Usuario_RevisadaPorId",
                        column: x => x.RevisadaPorId,
                        principalSchema: "seguranca",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DivergenciaDeIntegracao",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ChaveOrigem = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: true),
                    VendaDeMaquinaId = table.Column<long>(type: "bigint", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ValorNoCrm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValorNaOrigem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValorNoProtheus = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DetectadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ConfirmadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EncerradaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
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
                    table.PrimaryKey("PK_DivergenciaDeIntegracao", x => x.Id);
                    table.CheckConstraint("CK_DivergenciaDeIntegracao_Situacao", "[Situacao] IN ('Aberta','Resolvida','DeixouDeOcorrer')");
                    table.CheckConstraint("CK_DivergenciaDeIntegracao_Tipo", "[Tipo] IN ('CompradorDiferenteDoProprietarioNoCrm','ProprietarioNoProtheusDiferenteDoComprador','CompradorAlteradoNaOrigem','ChassiAlteradoNaOrigem','RegistroAusenteNaOrigem')");
                    table.ForeignKey(
                        name: "FK_DivergenciaDeIntegracao_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DivergenciaDeIntegracao_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DivergenciaDeIntegracao_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DivergenciaDeIntegracao_VendaDeMaquina_VendaDeMaquinaId",
                        column: x => x.VendaDeMaquinaId,
                        principalSchema: "frota",
                        principalTable: "VendaDeMaquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistroDeOrigem",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Fluxo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    ChaveOrigem = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    HashDoConteudo = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ChassiNaOrigem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LinhaNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ProdutoNaOrigem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    UnidadeNaOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    VendidaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    Transformacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Decisao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Motivos = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    VendaDeMaquinaId = table.Column<long>(type: "bigint", nullable: true),
                    PrimeiraLeituraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UltimaLeituraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ConteudoAlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AusenteNaOrigemDesde = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Leituras = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroDeOrigem", x => x.Id);
                    table.CheckConstraint("CK_RegistroDeOrigem_Decisao", "[Decisao] IN ('Importado','Pendente')");
                    table.CheckConstraint("CK_RegistroDeOrigem_Leituras", "[Leituras] >= 1");
                    table.ForeignKey(
                        name: "FK_RegistroDeOrigem_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistroDeOrigem_VendaDeMaquina_VendaDeMaquinaId",
                        column: x => x.VendaDeMaquinaId,
                        principalSchema: "frota",
                        principalTable: "VendaDeMaquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VinculoDeClienteComEquipamento",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: false),
                    Natureza = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    VendaDeMaquinaId = table.Column<long>(type: "bigint", nullable: true),
                    SistemaId = table.Column<int>(type: "int", nullable: true),
                    ReferenciaEm = table.Column<DateOnly>(type: "date", nullable: true),
                    EncerradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    MotivoDoEncerramento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_VinculoDeClienteComEquipamento", x => x.Id);
                    table.CheckConstraint("CK_VinculoDeClienteComEquipamento_Encerramento", "([EncerradoEm] IS NULL AND [MotivoDoEncerramento] IS NULL) OR ([EncerradoEm] IS NOT NULL AND [MotivoDoEncerramento] IS NOT NULL)");
                    table.CheckConstraint("CK_VinculoDeClienteComEquipamento_Natureza", "[Natureza] IN ('CompradorNaVenda')");
                    table.ForeignKey(
                        name: "FK_VinculoDeClienteComEquipamento_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "comercial",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VinculoDeClienteComEquipamento_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "organizacao",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VinculoDeClienteComEquipamento_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalSchema: "frota",
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VinculoDeClienteComEquipamento_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VinculoDeClienteComEquipamento_VendaDeMaquina_VendaDeMaquinaId",
                        column: x => x.VendaDeMaquinaId,
                        principalSchema: "frota",
                        principalTable: "VendaDeMaquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte",
                sql: "[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso",
                sql: "[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento'))");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_LinhaDeProdutoId",
                schema: "frota",
                table: "Equipamento",
                column: "LinhaDeProdutoId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento",
                sql: "[ModeloId] IS NOT NULL OR [Origem] <> 'Crm'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_Origem",
                schema: "frota",
                table: "Equipamento",
                sql: "[Origem] IN ('Protheus','Crm','Art')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_Situacao",
                schema: "frota",
                table: "Equipamento",
                sql: "[Situacao] IN ('Estoque','Ativo','Vendido','Baixado','ProprietarioNaoConfirmado')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_CompradorPendente_EmpresaId_Grupo_Situacao",
                schema: "integracao",
                table: "CompradorPendente",
                columns: new[] { "EmpresaId", "Grupo", "Situacao" });

            migrationBuilder.CreateIndex(
                name: "UX_CompradorPendente_ChavePublica",
                schema: "integracao",
                table: "CompradorPendente",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CompradorPendente_Sistema_Documento",
                schema: "integracao",
                table: "CompradorPendente",
                columns: new[] { "SistemaId", "Documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenciaDaOrigem_EmpresaCorrespondenteId",
                schema: "integracao",
                table: "CorrespondenciaDaOrigem",
                column: "EmpresaCorrespondenteId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenciaDaOrigem_LinhaDeProdutoId",
                schema: "integracao",
                table: "CorrespondenciaDaOrigem",
                column: "LinhaDeProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenciaDaOrigem_ModeloId",
                schema: "integracao",
                table: "CorrespondenciaDaOrigem",
                column: "ModeloId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenciaDaOrigem_RevisadaPorId",
                schema: "integracao",
                table: "CorrespondenciaDaOrigem",
                column: "RevisadaPorId");

            migrationBuilder.CreateIndex(
                name: "UX_CorrespondenciaDaOrigem_Sistema_Tipo_Codigo",
                schema: "integracao",
                table: "CorrespondenciaDaOrigem",
                columns: new[] { "SistemaId", "Tipo", "CodigoNaOrigem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DivergenciaDeIntegracao_EmpresaId_Situacao",
                schema: "integracao",
                table: "DivergenciaDeIntegracao",
                columns: new[] { "EmpresaId", "Situacao" });

            migrationBuilder.CreateIndex(
                name: "IX_DivergenciaDeIntegracao_EquipamentoId",
                schema: "integracao",
                table: "DivergenciaDeIntegracao",
                column: "EquipamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DivergenciaDeIntegracao_VendaDeMaquinaId",
                schema: "integracao",
                table: "DivergenciaDeIntegracao",
                column: "VendaDeMaquinaId");

            migrationBuilder.CreateIndex(
                name: "UX_DivergenciaDeIntegracao_ChavePublica",
                schema: "integracao",
                table: "DivergenciaDeIntegracao",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_DivergenciaDeIntegracao_Sistema_Tipo_Chave",
                schema: "integracao",
                table: "DivergenciaDeIntegracao",
                columns: new[] { "SistemaId", "Tipo", "ChaveOrigem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinhaDeProduto_FamiliaId",
                schema: "frota",
                table: "LinhaDeProduto",
                column: "FamiliaId");

            migrationBuilder.CreateIndex(
                name: "UX_LinhaDeProduto_Codigo",
                schema: "frota",
                table: "LinhaDeProduto",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroDeOrigem_Fluxo_Decisao",
                schema: "integracao",
                table: "RegistroDeOrigem",
                columns: new[] { "Fluxo", "Decisao" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroDeOrigem_VendaDeMaquinaId",
                schema: "integracao",
                table: "RegistroDeOrigem",
                column: "VendaDeMaquinaId");

            migrationBuilder.CreateIndex(
                name: "UX_RegistroDeOrigem_Sistema_Fluxo_Chave",
                schema: "integracao",
                table: "RegistroDeOrigem",
                columns: new[] { "SistemaId", "Fluxo", "ChaveOrigem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendaDeMaquina_CompradorId",
                schema: "frota",
                table: "VendaDeMaquina",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaDeMaquina_EmpresaDoFaturamentoId",
                schema: "frota",
                table: "VendaDeMaquina",
                column: "EmpresaDoFaturamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaDeMaquina_EmpresaId",
                schema: "frota",
                table: "VendaDeMaquina",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaDeMaquina_EquipamentoId_VendidaEm",
                schema: "frota",
                table: "VendaDeMaquina",
                columns: new[] { "EquipamentoId", "VendidaEm" });

            migrationBuilder.CreateIndex(
                name: "UX_VendaDeMaquina_ChavePublica",
                schema: "frota",
                table: "VendaDeMaquina",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_VendaDeMaquina_Sistema_ChaveOrigem",
                schema: "frota",
                table: "VendaDeMaquina",
                columns: new[] { "SistemaId", "ChaveOrigem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculoDeClienteComEquipamento_ClienteId",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoDeClienteComEquipamento_EmpresaId",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoDeClienteComEquipamento_EquipamentoId_Natureza",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                columns: new[] { "EquipamentoId", "Natureza" });

            migrationBuilder.CreateIndex(
                name: "IX_VinculoDeClienteComEquipamento_SistemaId",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "UX_VinculoDeClienteComEquipamento_ChavePublica",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                column: "ChavePublica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_VinculoDeClienteComEquipamento_Venda_Cliente_Natureza",
                schema: "frota",
                table: "VinculoDeClienteComEquipamento",
                columns: new[] { "VendaDeMaquinaId", "ClienteId", "Natureza" },
                unique: true,
                filter: "[VendaDeMaquinaId] IS NOT NULL AND [EncerradoEm] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipamento_LinhaDeProduto_LinhaDeProdutoId",
                schema: "frota",
                table: "Equipamento",
                column: "LinhaDeProdutoId",
                principalSchema: "frota",
                principalTable: "LinhaDeProduto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipamento_LinhaDeProduto_LinhaDeProdutoId",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropTable(
                name: "CompradorPendente",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "CorrespondenciaDaOrigem",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "DivergenciaDeIntegracao",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "RegistroDeOrigem",
                schema: "integracao");

            migrationBuilder.DropTable(
                name: "VinculoDeClienteComEquipamento",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "LinhaDeProduto",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "VendaDeMaquina",
                schema: "frota");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso");

            migrationBuilder.DropIndex(
                name: "IX_Equipamento_LinhaDeProdutoId",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_ModeloPendente",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_Origem",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamento_Situacao",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "LinhaDeProdutoId",
                schema: "frota",
                table: "Equipamento");

            migrationBuilder.AlterColumn<string>(
                name: "Situacao",
                schema: "frota",
                table: "Equipamento",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldUnicode: false,
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<int>(
                name: "ModeloId",
                schema: "frota",
                table: "Equipamento",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte",
                sql: "[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso",
                sql: "[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_Origem",
                schema: "frota",
                table: "Equipamento",
                sql: "[Origem] IN ('Protheus','Crm')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamento_Situacao",
                schema: "frota",
                table: "Equipamento",
                sql: "[Situacao] IN ('Estoque','Ativo','Vendido','Baixado')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaPerdida', 'Vinculo')");
        }
    }
}
