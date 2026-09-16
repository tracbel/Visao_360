using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacaoComoServicoECatalogoDeClassificacao : Migration
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

            migrationBuilder.CreateTable(
                name: "ExecucaoDeSincronizacao",
                schema: "integracao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Fluxo = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Maquina = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    IniciadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    TerminadaEm = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Resultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Tentativas = table.Column<int>(type: "int", nullable: false),
                    RegistrosLidos = table.Column<int>(type: "int", nullable: false),
                    Incluidos = table.Column<int>(type: "int", nullable: false),
                    Atualizados = table.Column<int>(type: "int", nullable: false),
                    Pendentes = table.Column<int>(type: "int", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucaoDeSincronizacao", x => x.Id);
                    table.CheckConstraint("CK_ExecucaoDeSincronizacao_Contadores", "[Tentativas] >= 0 AND [RegistrosLidos] >= 0 AND [Incluidos] >= 0 AND [Atualizados] >= 0 AND [Pendentes] >= 0");
                    table.CheckConstraint("CK_ExecucaoDeSincronizacao_Resultado", "[Resultado] IN ('EmAndamento','Sucesso','Falha','Ignorada')");
                    table.ForeignKey(
                        name: "FK_ExecucaoDeSincronizacao_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "integracao",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vinculo_Entidade",
                schema: "documento",
                table: "Vinculo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Recepcao_Entidade",
                schema: "integracao",
                table: "Recepcao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Permissao_Entidade",
                schema: "seguranca",
                table: "Permissao",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fonte_EntidadeRaiz",
                schema: "relatorio",
                table: "Fonte",
                sql: "[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EventoDeAcesso_Entidade",
                schema: "auditoria",
                table: "EventoDeAcesso",
                sql: "[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompartilhamentoDeRegistro_Entidade",
                schema: "seguranca",
                table: "CompartilhamentoDeRegistro",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChaveExterna_Entidade",
                schema: "integracao",
                table: "ChaveExterna",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoPersonalizado_Entidade",
                schema: "metadado",
                table: "CampoPersonalizado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CampoAuditado_Entidade",
                schema: "auditoria",
                table: "CampoAuditado",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlteracaoDeCampo_Entidade",
                schema: "auditoria",
                table: "AlteracaoDeCampo",
                sql: "[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'AreaPlantadaNoMunicipio', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'CarteiraMunicipio', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'CompradorPendente', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'CorrespondenciaDaOrigem', 'DivergenciaDeIntegracao', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoDeSincronizacao', 'ExecucaoRegra', 'Familia', 'Fase', 'FaturamentoDoCliente', 'FaturamentoSemCliente', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'LinhaDeProduto', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'Municipio', 'MunicipioDaAreaDeAtuacao', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'RegistroDeOrigem', 'Regra', 'RegraDePotencial', 'Relatorio', 'ResponsavelPeloMunicipio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'VendaDeMaquina', 'VendaPerdida', 'Vinculo', 'VinculoDeClienteComEquipamento')");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucaoDeSincronizacao_SistemaId_Fluxo_IniciadaEm",
                schema: "integracao",
                table: "ExecucaoDeSincronizacao",
                columns: new[] { "SistemaId", "Fluxo", "IniciadaEm" },
                descending: new[] { false, false, true });

            // O CATÁLOGO DE CLASSIFICAÇÃO VAI NA MIGRAÇÃO (documento 35, seção 11). O banco central do servidor
            // não recebe o seed de desenvolvimento, e sem as dez classificações a carga do ART recusa rodar. É
            // o mesmo MERGE de scripts/banco/seed/01-dados-de-referencia.sql: repetível, não troca família já
            // apontada (pode ter sido corrigida por uma pessoa) e, num banco sem família, deixa a família nula.
            // Dentro de EXEC porque o lote é compilado inteiro antes de rodar.
            migrationBuilder.Sql(@"EXEC(N'
MERGE frota.LinhaDeProduto AS destino
USING (
    SELECT v.Codigo, v.Nome, v.Porte,
           (SELECT TOP 1 fa.Id FROM frota.Familia fa JOIN frota.Marca ma ON ma.Id = fa.MarcaId
            WHERE ma.Codigo = v.Marca AND fa.Codigo = v.Familia) AS FamiliaId
    FROM (VALUES
        (''TRATOR_PEQUENO'',           N''Trator pequeno'',              ''Pequeno'',     ''JOHN_DEERE'', ''TRATOR''),
        (''TRATOR_MEDIO'',             N''Trator médio'',                ''Medio'',       ''JOHN_DEERE'', ''TRATOR''),
        (''TRATOR_GRANDE'',            N''Trator grande'',               ''Grande'',      ''JOHN_DEERE'', ''TRATOR''),
        (''COLHEDORA_DE_CANA'',        N''Colhedora de cana'',           ''NaoSeAplica'', ''JOHN_DEERE'', ''COLHEDORA_DE_CANA''),
        (''COLHEITADEIRA'',            N''Colheitadeira de grãos'',      ''NaoSeAplica'', NULL,           NULL),
        (''PLANTADEIRA'',              N''Plantadeira'',                 ''NaoSeAplica'', NULL,           NULL),
        (''PULVERIZADOR'',             N''Pulverizador'',                ''NaoSeAplica'', ''JOHN_DEERE'', ''PULVERIZADOR''),
        (''PLATAFORMA_DE_CORTE'',      N''Plataforma de corte'',         ''NaoSeAplica'', NULL,           NULL),
        (''IMPLEMENTO_JOHN_DEERE'',    N''Implemento John Deere'',       ''NaoSeAplica'', ''JOHN_DEERE'', ''IMPLEMENTO''),
        (''IMPLEMENTO_OUTRAS_MARCAS'', N''Implemento de outras marcas'', ''NaoSeAplica'', ''OUTRAS'',     ''IMPLEMENTO'')
    ) AS v (Codigo, Nome, Porte, Marca, Familia)
) AS origem
ON destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Nome <> origem.Nome OR destino.Porte <> origem.Porte
                  OR (destino.FamiliaId IS NULL AND origem.FamiliaId IS NOT NULL))
    THEN UPDATE SET Nome = origem.Nome, Porte = origem.Porte,
                    FamiliaId = COALESCE(destino.FamiliaId, origem.FamiliaId)
WHEN NOT MATCHED BY TARGET
    THEN INSERT (Codigo, Nome, FamiliaId, Porte, EstaAtiva)
         VALUES (origem.Codigo, origem.Nome, origem.FamiliaId, origem.Porte, 1);
')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecucaoDeSincronizacao",
                schema: "integracao");

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
        }
    }
}
