using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class ChaveCompostaDeCatalogoEDominioDeEntidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogoItem_CatalogoItem_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_CatalogoItem_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_CatalogoItem_OrigemId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_ClienteContato_CatalogoItem_PapelId",
                schema: "comercial",
                table: "ClienteContato");

            migrationBuilder.DropForeignKey(
                name: "FK_Documento_CatalogoItem_TipoDocumentoId",
                schema: "documento",
                table: "Documento");

            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_CatalogoItem_CulturaId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemDeProposta_CatalogoItem_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta");

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

            migrationBuilder.DropIndex(
                name: "IX_Processo_ConcorrenteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropIndex(
                name: "IX_Lead_MotivoDescarteId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Lead_OrigemId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_ItemDeProposta_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta");

            migrationBuilder.DropIndex(
                name: "IX_Endereco_CulturaId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropIndex(
                name: "IX_Documento_TipoDocumentoId",
                schema: "documento",
                table: "Documento");

            migrationBuilder.DropIndex(
                name: "IX_ClienteContato_PapelId",
                schema: "comercial",
                table: "ClienteContato");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_OrigemId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_CatalogoItem_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem");

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDoConcorrenteId",
                schema: "processo",
                table: "Processo",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDaOrigemId",
                schema: "comercial",
                table: "Lead",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDoMotivoDescarteId",
                schema: "comercial",
                table: "Lead",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDaCondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                type: "int",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDaCulturaId",
                schema: "comercial",
                table: "Endereco",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDoTipoDocumentoId",
                schema: "documento",
                table: "Documento",
                type: "int",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDoPapelId",
                schema: "comercial",
                table: "ClienteContato",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDaOrigemId",
                schema: "comercial",
                table: "Cliente",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "CatalogoDoMotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.InsertData(
                schema: "metadado",
                table: "Catalogo",
                columns: new[] { "Id", "Codigo", "Descricao", "EstaAtivo", "Nome", "PermiteItemNovo" },
                values: new object[,]
                {
                    { 1, "PAPEL_CONTATO", "Papel do contato na decisão de compra do cliente.", true, "Papel do contato", true },
                    { 2, "ORIGEM_LEAD", "Canal pelo qual o lead ou o cliente chegou à Tracbel.", true, "Origem do lead", true },
                    { 3, "MOTIVO_INATIVACAO", "Por que um cliente deixou de ser cliente ativo.", true, "Motivo de inativação", true },
                    { 4, "MOTIVO_DESCARTE", "Por que um lead foi descartado sem virar cliente.", true, "Motivo de descarte", true },
                    { 5, "CULTURA", "Cultura agrícola da propriedade do cliente.", true, "Cultura agrícola", true },
                    { 6, "TIPO_DOCUMENTO", "Tipo do arquivo anexado a cliente, processo ou equipamento.", true, "Tipo de documento", true },
                    { 7, "CONDICAO_PAGAMENTO", "Forma de pagamento ou financiamento do item de proposta.", true, "Condição de pagamento", true },
                    { 8, "CONCORRENTE", "Fabricante concorrente que disputou ou levou o negócio.", true, "Concorrente", true }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Processo_CatalogoDoConcorrenteId_ConcorrenteId",
                schema: "processo",
                table: "Processo",
                columns: new[] { "CatalogoDoConcorrenteId", "ConcorrenteId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Processo_CatalogoDoConcorrenteId",
                schema: "processo",
                table: "Processo",
                sql: "[CatalogoDoConcorrenteId] = 8");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lead_CatalogoDaOrigemId",
                schema: "comercial",
                table: "Lead",
                sql: "[CatalogoDaOrigemId] = 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lead_CatalogoDoMotivoDescarteId",
                schema: "comercial",
                table: "Lead",
                sql: "[CatalogoDoMotivoDescarteId] = 4");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDeProposta_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                columns: new[] { "CatalogoDaCondicaoPagamentoId", "CondicaoPagamentoId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItemDeProposta_CatalogoDaCondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                sql: "[CatalogoDaCondicaoPagamentoId] = 7");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FonteCampo_Campo",
                schema: "relatorio",
                table: "FonteCampo",
                sql: "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");

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

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_CatalogoDaCulturaId_CulturaId",
                schema: "comercial",
                table: "Endereco",
                columns: new[] { "CatalogoDaCulturaId", "CulturaId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Endereco_CatalogoDaCulturaId",
                schema: "comercial",
                table: "Endereco",
                sql: "[CatalogoDaCulturaId] = 5");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_CatalogoDoTipoDocumentoId_TipoDocumentoId",
                schema: "documento",
                table: "Documento",
                columns: new[] { "CatalogoDoTipoDocumentoId", "TipoDocumentoId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documento_CatalogoDoTipoDocumentoId",
                schema: "documento",
                table: "Documento",
                sql: "[CatalogoDoTipoDocumentoId] = 6");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteContato_CatalogoDoPapelId_PapelId",
                schema: "comercial",
                table: "ClienteContato",
                columns: new[] { "CatalogoDoPapelId", "PapelId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClienteContato_CatalogoDoPapelId",
                schema: "comercial",
                table: "ClienteContato",
                sql: "[CatalogoDoPapelId] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Cliente",
                columns: new[] { "CatalogoDaOrigemId", "OrigemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_CatalogoDoMotivoInativacaoId_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                columns: new[] { "CatalogoDoMotivoInativacaoId", "MotivoInativacaoId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cliente_CatalogoDaOrigemId",
                schema: "comercial",
                table: "Cliente",
                sql: "[CatalogoDaOrigemId] = 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cliente_CatalogoDoMotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                sql: "[CatalogoDoMotivoInativacaoId] = 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_CatalogoId_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem",
                columns: new[] { "CatalogoId", "ItemPaiId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Campo",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Campo",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Campo",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogoItem_CatalogoItem_CatalogoId_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem",
                columns: new[] { "CatalogoId", "ItemPaiId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_CatalogoItem_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Cliente",
                columns: new[] { "CatalogoDaOrigemId", "OrigemId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_CatalogoItem_CatalogoDoMotivoInativacaoId_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                columns: new[] { "CatalogoDoMotivoInativacaoId", "MotivoInativacaoId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClienteContato_CatalogoItem_CatalogoDoPapelId_PapelId",
                schema: "comercial",
                table: "ClienteContato",
                columns: new[] { "CatalogoDoPapelId", "PapelId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documento_CatalogoItem_CatalogoDoTipoDocumentoId_TipoDocumentoId",
                schema: "documento",
                table: "Documento",
                columns: new[] { "CatalogoDoTipoDocumentoId", "TipoDocumentoId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_CatalogoItem_CatalogoDaCulturaId_CulturaId",
                schema: "comercial",
                table: "Endereco",
                columns: new[] { "CatalogoDaCulturaId", "CulturaId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemDeProposta_CatalogoItem_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                columns: new[] { "CatalogoDaCondicaoPagamentoId", "CondicaoPagamentoId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_CatalogoItem_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Lead",
                columns: new[] { "CatalogoDaOrigemId", "OrigemId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_CatalogoItem_CatalogoDoMotivoDescarteId_MotivoDescarteId",
                schema: "comercial",
                table: "Lead",
                columns: new[] { "CatalogoDoMotivoDescarteId", "MotivoDescarteId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Processo_CatalogoItem_CatalogoDoConcorrenteId_ConcorrenteId",
                schema: "processo",
                table: "Processo",
                columns: new[] { "CatalogoDoConcorrenteId", "ConcorrenteId" },
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumns: new[] { "CatalogoId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogoItem_CatalogoItem_CatalogoId_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_CatalogoItem_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_CatalogoItem_CatalogoDoMotivoInativacaoId_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_ClienteContato_CatalogoItem_CatalogoDoPapelId_PapelId",
                schema: "comercial",
                table: "ClienteContato");

            migrationBuilder.DropForeignKey(
                name: "FK_Documento_CatalogoItem_CatalogoDoTipoDocumentoId_TipoDocumentoId",
                schema: "documento",
                table: "Documento");

            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_CatalogoItem_CatalogoDaCulturaId_CulturaId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemDeProposta_CatalogoItem_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_CatalogoItem_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Lead_CatalogoItem_CatalogoDoMotivoDescarteId_MotivoDescarteId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropForeignKey(
                name: "FK_Processo_CatalogoItem_CatalogoDoConcorrenteId_ConcorrenteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao");

            migrationBuilder.DropIndex(
                name: "IX_Processo_CatalogoDoConcorrenteId_ConcorrenteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Processo_CatalogoDoConcorrenteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao");

            migrationBuilder.DropIndex(
                name: "IX_Lead_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Lead_CatalogoDoMotivoDescarteId_MotivoDescarteId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lead_CatalogoDaOrigemId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lead_CatalogoDoMotivoDescarteId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_ItemDeProposta_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItemDeProposta_CatalogoDaCondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FonteCampo_Campo",
                schema: "relatorio",
                table: "FonteCampo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso");

            migrationBuilder.DropIndex(
                name: "IX_Endereco_CatalogoDaCulturaId_CulturaId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Endereco_CatalogoDaCulturaId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropIndex(
                name: "IX_Documento_CatalogoDoTipoDocumentoId_TipoDocumentoId",
                schema: "documento",
                table: "Documento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documento_CatalogoDoTipoDocumentoId",
                schema: "documento",
                table: "Documento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro");

            migrationBuilder.DropIndex(
                name: "IX_ClienteContato_CatalogoDoPapelId_PapelId",
                schema: "comercial",
                table: "ClienteContato");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClienteContato_CatalogoDoPapelId",
                schema: "comercial",
                table: "ClienteContato");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_CatalogoDaOrigemId_OrigemId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_CatalogoDoMotivoInativacaoId_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cliente_CatalogoDaOrigemId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cliente_CatalogoDoMotivoInativacaoId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna");

            migrationBuilder.DropIndex(
                name: "IX_CatalogoItem_CatalogoId_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoPersonalizado_Campo",
                schema: "metadado",
                table: "CampoPersonalizado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoAuditado_Campo",
                schema: "auditoria",
                table: "CampoAuditado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Campo",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo");

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "metadado",
                table: "Catalogo",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "CatalogoDoConcorrenteId",
                schema: "processo",
                table: "Processo");

            migrationBuilder.DropColumn(
                name: "CatalogoDaOrigemId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "CatalogoDoMotivoDescarteId",
                schema: "comercial",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "CatalogoDaCondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta");

            migrationBuilder.DropColumn(
                name: "CatalogoDaCulturaId",
                schema: "comercial",
                table: "Endereco");

            migrationBuilder.DropColumn(
                name: "CatalogoDoTipoDocumentoId",
                schema: "documento",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "CatalogoDoPapelId",
                schema: "comercial",
                table: "ClienteContato");

            migrationBuilder.DropColumn(
                name: "CatalogoDaOrigemId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "CatalogoDoMotivoInativacaoId",
                schema: "comercial",
                table: "Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Processo_ConcorrenteId",
                schema: "processo",
                table: "Processo",
                column: "ConcorrenteId");

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
                name: "IX_ItemDeProposta_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                column: "CondicaoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_CulturaId",
                schema: "comercial",
                table: "Endereco",
                column: "CulturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_TipoDocumentoId",
                schema: "documento",
                table: "Documento",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteContato_PapelId",
                schema: "comercial",
                table: "ClienteContato",
                column: "PapelId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                column: "MotivoInativacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_OrigemId",
                schema: "comercial",
                table: "Cliente",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem",
                column: "ItemPaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogoItem_CatalogoItem_ItemPaiId",
                schema: "metadado",
                table: "CatalogoItem",
                column: "ItemPaiId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_CatalogoItem_MotivoInativacaoId",
                schema: "comercial",
                table: "Cliente",
                column: "MotivoInativacaoId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_CatalogoItem_OrigemId",
                schema: "comercial",
                table: "Cliente",
                column: "OrigemId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClienteContato_CatalogoItem_PapelId",
                schema: "comercial",
                table: "ClienteContato",
                column: "PapelId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documento_CatalogoItem_TipoDocumentoId",
                schema: "documento",
                table: "Documento",
                column: "TipoDocumentoId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_CatalogoItem_CulturaId",
                schema: "comercial",
                table: "Endereco",
                column: "CulturaId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemDeProposta_CatalogoItem_CondicaoPagamentoId",
                schema: "processo",
                table: "ItemDeProposta",
                column: "CondicaoPagamentoId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_CatalogoItem_MotivoDescarteId",
                schema: "comercial",
                table: "Lead",
                column: "MotivoDescarteId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_CatalogoItem_OrigemId",
                schema: "comercial",
                table: "Lead",
                column: "OrigemId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Processo_CatalogoItem_ConcorrenteId",
                schema: "processo",
                table: "Processo",
                column: "ConcorrenteId",
                principalSchema: "metadado",
                principalTable: "CatalogoItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
