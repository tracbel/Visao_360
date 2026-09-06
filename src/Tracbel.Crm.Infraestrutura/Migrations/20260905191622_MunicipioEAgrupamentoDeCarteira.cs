using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class MunicipioEAgrupamentoDeCarteira : Migration
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
                name: "Municipio",
                schema: "comercial",
                table: "Endereco",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AddColumn<int>(
                name: "MunicipioId",
                schema: "comercial",
                table: "Endereco",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Municipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoIbge = table.Column<int>(type: "int", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Uf = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipio", x => x.Id);
                    table.CheckConstraint("CK_Municipio_CodigoIbge", "[CodigoIbge] IS NULL OR [CodigoIbge] BETWEEN 1000000 AND 5999999");
                    table.CheckConstraint("CK_Municipio_Uf", "[Uf] COLLATE Latin1_General_BIN2 IN ('AC','AL','AP','AM','BA','CE','DF','ES','GO','MA','MT','MS','MG','PA','PB','PR','PE','PI','RJ','RN','RS','RO','RR','SC','SP','SE','TO')");
                });

            migrationBuilder.CreateTable(
                name: "CarteiraMunicipio",
                schema: "organizacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarteiraId = table.Column<long>(type: "bigint", nullable: false),
                    MunicipioId = table.Column<int>(type: "int", nullable: false),
                    VinculadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    VinculadoPorId = table.Column<long>(type: "bigint", nullable: false),
                    DesvinculadoEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarteiraMunicipio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarteiraMunicipio_Carteira_CarteiraId",
                        column: x => x.CarteiraId,
                        principalSchema: "organizacao",
                        principalTable: "Carteira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarteiraMunicipio_Municipio_MunicipioId",
                        column: x => x.MunicipioId,
                        principalSchema: "organizacao",
                        principalTable: "Municipio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte",
                sql: "[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso",
                sql: "[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo'))");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_MunicipioId",
                schema: "comercial",
                table: "Endereco",
                column: "MunicipioId",
                filter: "[ExcluidoEm] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Endereco_Municipio",
                schema: "comercial",
                table: "Endereco",
                sql: "([MunicipioId] IS NOT NULL AND [Municipio] IS NULL) OR ([MunicipioId] IS NULL AND [Municipio] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.CreateIndex(
                name: "IX_CarteiraMunicipio_MunicipioId",
                schema: "organizacao",
                table: "CarteiraMunicipio",
                column: "MunicipioId",
                filter: "[DesvinculadoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_CarteiraMunicipio_Carteira_Municipio_Vigente",
                schema: "organizacao",
                table: "CarteiraMunicipio",
                columns: new[] { "CarteiraId", "MunicipioId" },
                unique: true,
                filter: "[DesvinculadoEm] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Municipio_Nome",
                schema: "organizacao",
                table: "Municipio",
                column: "Nome",
                filter: "[EstaAtivo] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_Municipio_CodigoIbge",
                schema: "organizacao",
                table: "Municipio",
                column: "CodigoIbge",
                unique: true,
                filter: "[CodigoIbge] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Municipio_Uf_Nome",
                schema: "organizacao",
                table: "Municipio",
                columns: new[] { "Uf", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_Municipio_MunicipioId",
                schema: "comercial",
                table: "Endereco",
                column: "MunicipioId",
                principalSchema: "organizacao",
                principalTable: "Municipio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_Municipio_MunicipioId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropTable(
                name: "CarteiraMunicipio",
                schema: "organizacao");

            migrationBuilder.DropTable(
                name: "Municipio",
                schema: "organizacao");

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
                name: "IX_Endereco_MunicipioId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Endereco_Municipio",
                schema: "comercial",
                table: "Endereco");

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
                name: "MunicipioId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.AlterColumn<string>(
                name: "Municipio",
                schema: "comercial",
                table: "Endereco",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte",
                sql: "[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso",
                sql: "[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");
        }
    }
}
