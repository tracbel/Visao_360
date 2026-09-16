using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class SimplificacaoEstruturalFase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Equipe_EquipeId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Carteira_Praca_PracaId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Equipe_ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Interacao_Lead_LeadId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemDescartada_MensagemDeSaida_MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_Equipe_ProprietarioEquipeId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Equipe_ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_Tarefa_Regra_CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_TipoTarefa_Formulario_FormularioId",
                schema: "processo",
                table: "TipoTarefa");

            migrationBuilder.DropTable(
                name: "Alerta",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "CampoAuditado",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "CampoPersonalizado",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "CompartilhamentoDeRegistro",
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
                name: "Lead",
                schema: "comercial");

            migrationBuilder.DropTable(
                name: "LeituraDeHorimetro",
                schema: "frota");

            migrationBuilder.DropTable(
                name: "MensagemDeSaida",
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
                name: "Praca",
                schema: "organizacao");

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
                name: "Vinculo",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "Equipe",
                schema: "seguranca");

            migrationBuilder.DropTable(
                name: "Regra",
                schema: "processo");

            migrationBuilder.DropTable(
                name: "Fonte",
                schema: "relatorio");

            migrationBuilder.DropTable(
                name: "Pergunta",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "Preenchimento",
                schema: "metadado");

            migrationBuilder.DropTable(
                name: "Documento",
                schema: "documento");

            migrationBuilder.DropTable(
                name: "Formulario",
                schema: "metadado");

            migrationBuilder.DropIndex(
                name: "IX_TipoTarefa_FormularioId",
                schema: "processo",
                table: "TipoTarefa");

            migrationBuilder.DropIndex(
                name: "IX_Tarefa_CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropIndex(
                name: "IX_Tarefa_ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropIndex(
                name: "IX_Processo_ProprietarioEquipeId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropIndex(
                name: "IX_MensagemDescartada_MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada");

            migrationBuilder.DropIndex(
                name: "IX_Interacao_LeadId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Interacao_TemVinculo",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropIndex(
                name: "IX_Carteira_EquipeId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropIndex(
                name: "IX_Carteira_PracaId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropColumn(
                name: "FormularioId",
                schema: "processo",
                table: "TipoTarefa");

            migrationBuilder.DropColumn(
                name: "CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropColumn(
                name: "ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa");

            migrationBuilder.DropColumn(
                name: "ProprietarioEquipeId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropColumn(
                name: "MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada");

            migrationBuilder.DropColumn(
                name: "LeadId",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropColumn(
                name: "ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "EquipeId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.DropColumn(
                name: "PracaId",
                schema: "organizacao",
                table: "Carteira");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Interacao_TemVinculo",
                schema: "processo",
                table: "Interacao",
                sql: "[ProcessoId] IS NOT NULL OR [ClienteId] IS NOT NULL OR [ContatoId] IS NOT NULL");

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

            // O que o EF Core não sabe remover: partição órfã, schema vazio e o histórico de
            // migração de dbo. Ver SimplificacaoEstruturalFase1.Residuos.cs.
            RemoverResiduosDaSimplificacao(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Interacao_TemVinculo",
                schema: "processo",
                table: "Interacao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.EnsureSchema(
                name: "documento");

            migrationBuilder.EnsureSchema(
                name: "relatorio");

            migrationBuilder.AddColumn<int>(
                name: "FormularioId",
                schema: "processo",
                table: "TipoTarefa",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProprietarioEquipeId",
                schema: "processo",
                table: "Processo",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                schema: "processo",
                table: "Interacao",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EquipeId",
                schema: "organizacao",
                table: "Carteira",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PracaId",
                schema: "organizacao",
                table: "Carteira",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Alerta",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    Detalhe = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Severidade = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    VigenteAte = table.Column<DateOnly>(type: "date", nullable: true),
                    VigenteDe = table.Column<DateOnly>(type: "date", nullable: false)
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
                name: "CampoAuditado",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    RetencaoMeses = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampoAuditado", x => x.Id);
                    table.CheckConstraint("CK_CampoAuditado_Campo", "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");
                    table.CheckConstraint("CK_CampoAuditado_Entidade", "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
                    table.CheckConstraint("CK_CampoAuditado_Retencao", "[RetencaoMeses] BETWEEN 1 AND 120");
                });

            migrationBuilder.CreateTable(
                name: "CampoPersonalizado",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    CatalogoId = table.Column<int>(type: "int", nullable: true),
                    CondicaoVisibilidade = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EhObrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    EhPersonalizado = table.Column<bool>(type: "bit", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    Grupo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    NoResumo = table.Column<bool>(type: "bit", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    Rotulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TamanhoMaximo = table.Column<int>(type: "int", nullable: true),
                    TipoDeCampo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Validacao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampoPersonalizado", x => x.Id);
                    table.CheckConstraint("CK_CampoPersonalizado_Campo", "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");
                    table.CheckConstraint("CK_CampoPersonalizado_Entidade", "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
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
                name: "ConsentimentoComunicacao",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Canal = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    Concedido = table.Column<bool>(type: "bit", nullable: false),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    DecididaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    Finalidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    OrigemEvidencia = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    ReferenciaEvidencia = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RegistradoPorId = table.Column<long>(type: "bigint", nullable: true)
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
                name: "Documento",
                schema: "documento",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    CaminhoArmazenamento = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    CatalogoDoTipoDocumentoId = table.Column<int>(type: "int", nullable: false, defaultValue: 6),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    NumeroVersao = table.Column<int>(type: "int", nullable: false),
                    ResumoConteudo = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    TipoConteudo = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    TipoDocumentoId = table.Column<int>(type: "int", nullable: false),
                    ValidoAte = table.Column<DateOnly>(type: "date", nullable: true),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documento", x => x.Id);
                    table.CheckConstraint("CK_Documento_CatalogoDoTipoDocumentoId", "[CatalogoDoTipoDocumentoId] = 6");
                    table.CheckConstraint("CK_Documento_NumeroVersao", "[NumeroVersao] >= 1");
                    table.CheckConstraint("CK_Documento_Resumo", "[ResumoConteudo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9a-f]%'");
                    table.CheckConstraint("CK_Documento_Tamanho", "[TamanhoBytes] > 0");
                    table.ForeignKey(
                        name: "FK_Documento_CatalogoItem_CatalogoDoTipoDocumentoId_TipoDocumentoId",
                        columns: x => new { x.CatalogoDoTipoDocumentoId, x.TipoDocumentoId },
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumns: new[] { "CatalogoId", "Id" },
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
                name: "Equipe",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
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
                name: "EventoDeAcesso",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcorreuEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    AgenteUsuario = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Detalhe = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    NomePrincipal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RegistroId = table.Column<long>(type: "bigint", nullable: true),
                    Tipo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventoDeAcesso", x => new { x.Id, x.OcorreuEm });
                    table.CheckConstraint("CK_EventoDeAcesso_Entidade", "[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento'))");
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
                name: "Fonte",
                schema: "relatorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtualizadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EhMaterializada = table.Column<bool>(type: "bit", nullable: false),
                    EntidadeRaiz = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NomeDaVisao = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonte", x => x.Id);
                    table.CheckConstraint("CK_Fonte_EntidadeRaiz", "[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
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
                name: "Formulario",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: true),
                    VersaoPublicada = table.Column<int>(type: "int", nullable: false)
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
                name: "InteracaoParticipante",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContatoId = table.Column<long>(type: "bigint", nullable: true),
                    InteracaoId = table.Column<long>(type: "bigint", nullable: false),
                    NomeExterno = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Papel = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true)
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
                name: "ItemDeProposta",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    CatalogoDaCondicaoPagamentoId = table.Column<int>(type: "int", nullable: false, defaultValue: 7),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CondicaoPagamentoId = table.Column<int>(type: "int", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    DescontoPercentual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ModeloId = table.Column<int>(type: "int", nullable: true),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemDeProposta", x => x.Id);
                    table.CheckConstraint("CK_ItemDeProposta_CatalogoDaCondicaoPagamentoId", "[CatalogoDaCondicaoPagamentoId] = 7");
                    table.CheckConstraint("CK_ItemDeProposta_Desconto", "[DescontoPercentual] IS NULL OR [DescontoPercentual] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_ItemDeProposta_Quantidade", "[Quantidade] > 0");
                    table.CheckConstraint("CK_ItemDeProposta_ValorTotal", "[ValorTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_ItemDeProposta_CatalogoItem_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId",
                        columns: x => new { x.CatalogoDaCondicaoPagamentoId, x.CondicaoPagamentoId },
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumns: new[] { "CatalogoId", "Id" },
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
                name: "Lead",
                schema: "comercial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    CatalogoDaOrigemId = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    CatalogoDoMotivoDescarteId = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ClienteGeradoId = table.Column<long>(type: "bigint", nullable: true),
                    ContatoGeradoId = table.Column<long>(type: "bigint", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    Documento = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Interesse = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    LinhaNegocioId = table.Column<int>(type: "int", nullable: true),
                    MotivoDescarteId = table.Column<int>(type: "int", nullable: true),
                    NomeContato = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeEmpresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrigemId = table.Column<int>(type: "int", nullable: false),
                    PayloadOriginal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessoGeradoId = table.Column<long>(type: "bigint", nullable: true),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
                    QualificadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    QualificadoPorId = table.Column<long>(type: "bigint", nullable: true),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Telefone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lead", x => x.Id);
                    table.CheckConstraint("CK_Lead_CatalogoDaOrigemId", "[CatalogoDaOrigemId] = 2");
                    table.CheckConstraint("CK_Lead_CatalogoDoMotivoDescarteId", "[CatalogoDoMotivoDescarteId] = 4");
                    table.CheckConstraint("CK_Lead_PayloadOriginalJson", "[PayloadOriginal] IS NULL OR ISJSON([PayloadOriginal]) = 1");
                    table.CheckConstraint("CK_Lead_Qualificacao", "[Situacao] <> 'Qualificado' OR ([QualificadoEm] IS NOT NULL AND [QualificadoPorId] IS NOT NULL AND [ClienteGeradoId] IS NOT NULL)");
                    table.CheckConstraint("CK_Lead_Situacao", "[Situacao] IN ('Novo','EmContato','Qualificado','Descartado','Duplicado')");
                    table.ForeignKey(
                        name: "FK_Lead_CatalogoItem_CatalogoDaOrigemId_OrigemId",
                        columns: x => new { x.CatalogoDaOrigemId, x.OrigemId },
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumns: new[] { "CatalogoId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lead_CatalogoItem_CatalogoDoMotivoDescarteId_MotivoDescarteId",
                        columns: x => new { x.CatalogoDoMotivoDescarteId, x.MotivoDescarteId },
                        principalSchema: "metadado",
                        principalTable: "CatalogoItem",
                        principalColumns: new[] { "CatalogoId", "Id" },
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
                name: "LeituraDeHorimetro",
                schema: "frota",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: false),
                    Fonte = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Horas = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    LidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    RegistradoPorId = table.Column<long>(type: "bigint", nullable: true)
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
                name: "MensagemDeSaida",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EntregueEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ProximaTentativaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Situacao = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Tentativas = table.Column<short>(type: "smallint", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    UltimoErro = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
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
                name: "Meta",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    Alvo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CarteiraId = table.Column<long>(type: "bigint", nullable: true),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    LinhaDeNegocioId = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PeriodoFim = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodoInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true),
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
                name: "Permissao",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Verbo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissao", x => x.Id);
                    table.CheckConstraint("CK_Permissao_Entidade", "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
                    table.CheckConstraint("CK_Permissao_Verbo", "[Verbo] IN ('Ler','Criar','Editar','Excluir','Atribuir','Compartilhar')");
                });

            migrationBuilder.CreateTable(
                name: "Praca",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnoReferencia = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    LinhaDeNegocioId = table.Column<int>(type: "int", nullable: false),
                    MaquinasEstimadas = table.Column<int>(type: "int", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PotencialEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Uf = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: true)
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
                name: "Recepcao",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecebidaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ChaveOrigem = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Erro = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExpurgarApos = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ProcessadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recepcao", x => new { x.Id, x.RecebidaEm });
                    table.CheckConstraint("CK_Recepcao_ConteudoJson", "ISJSON([Conteudo]) = 1");
                    table.CheckConstraint("CK_Recepcao_Desfecho", "([Situacao] <> 'Falhou' OR [Erro] IS NOT NULL) AND ([Situacao] <> 'Processada' OR [ProcessadaEm] IS NOT NULL)");
                    table.CheckConstraint("CK_Recepcao_Entidade", "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
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
                name: "Regra",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    Codigo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Condicao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Efeito = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EfeitoFaseId = table.Column<int>(type: "int", nullable: true),
                    EfeitoParametros = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EfeitoTipoTarefaId = table.Column<int>(type: "int", nullable: true),
                    EhCritica = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    Evento = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ExpressaoDestinatario = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    PrazoDiasUteis = table.Column<int>(type: "int", nullable: false),
                    ResultadoId = table.Column<int>(type: "int", nullable: true),
                    TipoProcessoId = table.Column<int>(type: "int", nullable: true),
                    TipoTarefaId = table.Column<int>(type: "int", nullable: true)
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
                name: "TratadorDeEvento",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CamposFiltro = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    EhAssincrono = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false),
                    Evento = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Momento = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    TipoImplementacao = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TratadorDeEvento", x => x.Id);
                    table.CheckConstraint("CK_TratadorDeEvento_Assincrono", "[EhAssincrono] = 0 OR [Momento] = 'PosOperacao'");
                    table.CheckConstraint("CK_TratadorDeEvento_Momento", "[Momento] IN ('PreValidacao','PreOperacao','PosOperacao')");
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
                    table.CheckConstraint("CK_Vinculo_Entidade", "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
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
                name: "CompartilhamentoDeRegistro",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Entidade = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EquipeId = table.Column<long>(type: "bigint", nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Motivo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nivel = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    RegistroId = table.Column<long>(type: "bigint", nullable: false),
                    RegraOrigemId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: true),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompartilhamentoDeRegistro", x => x.Id);
                    table.CheckConstraint("CK_CompartilhamentoDeRegistro_Entidade", "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");
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
                name: "EquipeMembro",
                schema: "seguranca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EhLider = table.Column<bool>(type: "bit", nullable: false),
                    EntrouEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EquipeId = table.Column<long>(type: "bigint", nullable: false),
                    SaiuEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false)
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
                name: "FonteCampo",
                schema: "relatorio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Campo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    FonteId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    PermiteAgrupar = table.Column<bool>(type: "bit", nullable: false),
                    PermiteFiltrar = table.Column<bool>(type: "bit", nullable: false),
                    PermiteSomar = table.Column<bool>(type: "bit", nullable: false),
                    Rotulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TipoDeDado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FonteCampo", x => x.Id);
                    table.CheckConstraint("CK_FonteCampo_Campo", "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");
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
                    AlteradoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AlteradoPorId = table.Column<long>(type: "bigint", nullable: true),
                    ChavePublica = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CriadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    Definicao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    FonteId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProprietarioId = table.Column<long>(type: "bigint", nullable: false),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Visibilidade = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
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
                name: "Pergunta",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogoId = table.Column<int>(type: "int", nullable: true),
                    Codigo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EhObrigatoria = table.Column<bool>(type: "bit", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "bit", nullable: false),
                    FormularioId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<short>(type: "smallint", nullable: false),
                    PerguntaCondicaoId = table.Column<int>(type: "int", nullable: true),
                    Texto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TipoDeResposta = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ValorCondicao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
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
                name: "Preenchimento",
                schema: "metadado",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FormularioId = table.Column<int>(type: "int", nullable: false),
                    InteracaoId = table.Column<long>(type: "bigint", nullable: true),
                    PreenchidoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    PreenchidoPorId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    Situacao = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TarefaId = table.Column<long>(type: "bigint", nullable: true),
                    VersaoFormulario = table.Column<int>(type: "int", nullable: false)
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
                name: "PassagemDeFase",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntrouEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EntrouPorId = table.Column<long>(type: "bigint", nullable: false),
                    FaseId = table.Column<int>(type: "int", nullable: false),
                    HorasUteis = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    InteracaoOrigemId = table.Column<long>(type: "bigint", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: false),
                    RegraOrigemId = table.Column<int>(type: "int", nullable: true),
                    SaiuEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
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
                name: "RegraExecucao",
                schema: "processo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExecutadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CorrelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinatarioResolvidoId = table.Column<long>(type: "bigint", nullable: true),
                    DuracaoMs = table.Column<int>(type: "int", nullable: false),
                    Evento = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    InteracaoId = table.Column<long>(type: "bigint", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessoId = table.Column<long>(type: "bigint", nullable: true),
                    RegraId = table.Column<int>(type: "int", nullable: false),
                    Resultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TarefaCriadaId = table.Column<long>(type: "bigint", nullable: true),
                    TarefaId = table.Column<long>(type: "bigint", nullable: true)
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
                    CatalogoItemId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    EquipamentoId = table.Column<long>(type: "bigint", nullable: true),
                    PerguntaId = table.Column<int>(type: "int", nullable: false),
                    PreenchimentoId = table.Column<long>(type: "bigint", nullable: false),
                    ValorBooleano = table.Column<bool>(type: "bit", nullable: true),
                    ValorData = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ValorEstruturado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorNumero = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ValorTexto = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
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
                name: "IX_TipoTarefa_FormularioId",
                schema: "processo",
                table: "TipoTarefa",
                column: "FormularioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa",
                column: "CriadaPorRegraId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefa_ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa",
                column: "ResponsavelEquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ProprietarioEquipeId",
                schema: "processo",
                table: "Processo",
                column: "ProprietarioEquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagemDescartada_MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada",
                column: "MensagemDeSaidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacao_LeadId",
                schema: "processo",
                table: "Interacao",
                column: "LeadId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Interacao_TemVinculo",
                schema: "processo",
                table: "Interacao",
                sql: "[ProcessoId] IS NOT NULL OR [ClienteId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [LeadId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente",
                column: "ProprietarioEquipeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_EquipeId",
                schema: "organizacao",
                table: "Carteira",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Carteira_PracaId",
                schema: "organizacao",
                table: "Carteira",
                column: "PracaId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

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
                name: "IX_Documento_CatalogoDoTipoDocumentoId_TipoDocumentoId",
                schema: "documento",
                table: "Documento",
                columns: new[] { "CatalogoDoTipoDocumentoId", "TipoDocumentoId" });

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
                name: "UX_Documento_ChavePublica",
                schema: "documento",
                table: "Documento",
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
                name: "IX_ItemDeProposta_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                columns: new[] { "CatalogoDaCondicaoPagamentoId", "CondicaoPagamentoId" });

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
                name: "IX_Lead_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Lead",
                columns: new[] { "CatalogoDaOrigemId", "OrigemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Lead_CatalogoDoMotivoDescarteId_MotivoDescarteId",
                schema: "comercial",
                table: "Lead",
                columns: new[] { "CatalogoDoMotivoDescarteId", "MotivoDescarteId" });

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
                name: "FK_Carteira_Equipe_EquipeId",
                schema: "organizacao",
                table: "Carteira",
                column: "EquipeId",
                principalSchema: "seguranca",
                principalTable: "Equipe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Carteira_Praca_PracaId",
                schema: "organizacao",
                table: "Carteira",
                column: "PracaId",
                principalSchema: "organizacao",
                principalTable: "Praca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Equipe_ProprietarioEquipeId",
                schema: "comercial",
                table: "Cliente",
                column: "ProprietarioEquipeId",
                principalSchema: "seguranca",
                principalTable: "Equipe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interacao_Lead_LeadId",
                schema: "processo",
                table: "Interacao",
                column: "LeadId",
                principalSchema: "comercial",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemDescartada_MensagemDeSaida_MensagemDeSaidaId",
                schema: "integracao",
                table: "MensagemDescartada",
                column: "MensagemDeSaidaId",
                principalSchema: "integracao",
                principalTable: "MensagemDeSaida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Processo_Equipe_ProprietarioEquipeId",
                schema: "processo",
                table: "Processo",
                column: "ProprietarioEquipeId",
                principalSchema: "seguranca",
                principalTable: "Equipe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tarefa_Equipe_ResponsavelEquipeId",
                schema: "processo",
                table: "Tarefa",
                column: "ResponsavelEquipeId",
                principalSchema: "seguranca",
                principalTable: "Equipe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tarefa_Regra_CriadaPorRegraId",
                schema: "processo",
                table: "Tarefa",
                column: "CriadaPorRegraId",
                principalSchema: "processo",
                principalTable: "Regra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TipoTarefa_Formulario_FormularioId",
                schema: "processo",
                table: "TipoTarefa",
                column: "FormularioId",
                principalSchema: "metadado",
                principalTable: "Formulario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // POR ÚLTIMO, e não por acaso: o particionamento recria a chave primária clusterizada
            // sobre o esquema de partição, e por isso precisa que as tabelas e os índices acima já
            // existam. Ver SimplificacaoEstruturalFase1.Residuos.cs.
            RefazerResiduosDaSimplificacao(migrationBuilder);
        }
    }
}
