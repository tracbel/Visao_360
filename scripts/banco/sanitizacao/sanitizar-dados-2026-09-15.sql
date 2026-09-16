/* =================================================================================================
   SANITIZACAO DOS DADOS DO CRM - 15/09/2026 (documento 38)

   O QUE FAZ
     Remove os dados operacionais e tecnicos antigos e PRESERVA a estrutura do banco: nenhuma tabela,
     coluna, indice, restricao, relacionamento ou migracao e criado, alterado ou removido.

     Preserva:  migracoes, catalogos (metadado, frota), sistemas de integracao, estrutura de
                permissoes, configuracoes vazias e as 18 filiais.
     Mantem:    so os usuarios administradores (lista abaixo, por ambiente).
     Sanitiza:  organizacao.Municipio - fica so quem tem codigo do IBGE.
     Apaga:     clientes, contatos, enderecos, carteiras, processos, agenda (tarefas), linha do tempo
                (interacoes), faturamento, vendas perdidas, maquinas, vendas e dados do ART, territorio,
                configuracao herdada do Vortice (tipos de processo, fases, tipos de tarefa, resultados,
                motivos de perda, linhas de negocio, regra de potencial), de-para e filas do legado,
                pontos e historico de sincronizacao e a trilha de auditoria dos registros removidos.

   COMO FUNCIONA
     1. Guardas antes de tocar em qualquer dado: o banco precisa se chamar TracbelCrm (o arquivo
        restaurado TracbelCrmArquivo20260915 e as copias de ensaio sao recusados), toda tabela do banco
        precisa estar classificada abaixo e todo administrador precisa existir ativo.
     2. Conta todas as tabelas (COUNT_BIG exato).
     3. BEGIN TRANSACTION.
     4. Quebra o unico ciclo real sem desligar restricao: processo.Interacao.TarefaId = NULL (a coluna
        aceita nulo e nenhuma CHECK a cita). O gestor dos administradores so e esvaziado se apontar para
        quem sai.
     5. Apaga na ordem natural das chaves estrangeiras (filha antes da mae). Nenhuma restricao e
        desabilitada. As auto-referencias de frota.Equipamento e comercial.Cliente saem no mesmo
        DELETE que esvazia a tabela.
     6. Valida DENTRO da transacao: tabelas a apagar com 0 linhas, tabelas preservadas com a mesma
        contagem, usuarios = administradores, municipios = os que tinham codigo IBGE, nenhuma chave
        estrangeira ou CHECK desligada ou nao confiavel a mais que antes, e DBCC CHECKCONSTRAINTS sem
        nenhuma violacao.
     7. @Confirmar = 0 (padrao): ROLLBACK - e uma SIMULACAO, o banco nao muda.
        @Confirmar = 1: COMMIT. Qualquer erro em qualquer etapa: ROLLBACK, nada fica pela metade.
     8. So depois do COMMIT, reinicia o IDENTITY das tabelas que ficaram vazias e ja tinham recebido
        linhas (o proximo registro volta a ser 1). Tabelas preservadas e parcialmente mantidas
        (Usuario, Municipio) nao sao tocadas.

   COMO RODAR
     - scripts/banco/sanitizacao/executar-sanitizacao.ps1 -Onde Local|Servidor [-Confirmar]
     - DBeaver: abrir este arquivo, ajustar @Confirmar, selecionar TUDO e executar como UM lote
       (Ctrl+Enter com o texto selecionado). Nao use "executar script" (Alt+X): ele separa os comandos
       e as variaveis se perdem.

   RESTAURACAO
     Backups de 15/09/2026, anteriores a esta limpeza (documento 38):
       servidor: ...\MSSQL\Backup\TracbelCrm-arquivo-antes-da-limpeza-20260915-095042.bak
       estacao:  dados-locais\bancos\ (copia do servidor e do banco local)

   ASCII puro de proposito: roda igual no DBeaver, no sqlcmd e no PowerShell 5.1 do servidor.
   ================================================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;

-- 0 = SIMULACAO (executa, valida e desfaz). 1 = GRAVA.
DECLARE @Confirmar bit = 0;

-- -----------------------------------------------------------------------------------------------
-- ADMINISTRADORES QUE FICAM - decisao de 15/09/2026: as contas que entram no app hoje.
--   servidor AGRO-SISTEMAS-W: as duas contas pessoais ja ligadas ao Entra ID.
--   banco local de desenvolvimento: o usuario ficticio que a configuracao local usa para entrar.
-- -----------------------------------------------------------------------------------------------
DECLARE @Administradores TABLE (NomePrincipal nvarchar(256) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY);
IF @@SERVERNAME = N'AGRO-SISTEMAS-W'
    INSERT @Administradores (NomePrincipal) VALUES (N'ricardo.moretti@tracbel.com.br'), (N'hugo.rocha@tracbel.com.br');
ELSE
    INSERT @Administradores (NomePrincipal) VALUES (N'cen.ribeiraopreto@tracbel.com.br');

-- -----------------------------------------------------------------------------------------------
-- CLASSIFICACAO DE TODAS AS TABELAS
--   A estrutural | B cadastro necessario | C operacional | D derivado/tecnico
-- -----------------------------------------------------------------------------------------------
DECLARE @Classificacao TABLE (
    Tabela    nvarchar(300) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    Categoria char(1)       NOT NULL,
    Acao      varchar(30)   NOT NULL,
    Motivo    nvarchar(300) COLLATE DATABASE_DEFAULT NOT NULL);

INSERT @Classificacao (Tabela, Categoria, Acao, Motivo) VALUES
 -- A - estruturais
 (N'dbo.__EFMigrationsHistory',               'A', 'PRESERVAR', N'Historico de migracoes do EF Core (vazio neste schema)'),
 (N'metadado.__EFMigrationsHistory',          'A', 'PRESERVAR', N'Historico de migracoes do EF Core'),
 (N'metadado.Catalogo',                       'A', 'PRESERVAR', N'Catalogos de dominio do seed, usados por formularios e validacao da API'),
 (N'metadado.CatalogoItem',                   'A', 'PRESERVAR', N'Itens dos catalogos de dominio'),
 (N'frota.Marca',                             'A', 'PRESERVAR', N'Catalogo de maquinas (seed)'),
 (N'frota.Familia',                           'A', 'PRESERVAR', N'Catalogo de maquinas (seed)'),
 (N'frota.Modelo',                            'A', 'PRESERVAR', N'Catalogo de maquinas (seed)'),
 (N'frota.LinhaDeProduto',                    'A', 'PRESERVAR', N'Classificacao de produto (migracao)'),
 (N'integracao.Sistema',                      'A', 'PRESERVAR', N'Catalogo dos sistemas de origem'),
 (N'seguranca.Permissao',                     'A', 'PRESERVAR', N'Estrutura de permissoes (vazia)'),
 (N'seguranca.ConjuntoDePermissao',           'A', 'PRESERVAR', N'Estrutura de perfis (vazia)'),
 (N'seguranca.ConjuntoDePermissaoItem',       'A', 'PRESERVAR', N'Estrutura de perfis (vazia)'),
 (N'seguranca.UsuarioConjuntoDePermissao',    'A', 'PRESERVAR', N'Concessoes de perfil (vazia)'),
 (N'auditoria.CampoAuditado',                 'A', 'PRESERVAR', N'Configuracao de auditoria (vazia)'),
 (N'metadado.CampoPersonalizado',             'A', 'PRESERVAR', N'Metadado de configuracao (vazio)'),
 (N'metadado.TratadorDeEvento',               'A', 'PRESERVAR', N'Metadado de configuracao (vazio)'),
 (N'metadado.Formulario',                     'A', 'PRESERVAR', N'Metadado de configuracao (vazio)'),
 (N'metadado.Pergunta',                       'A', 'PRESERVAR', N'Metadado de configuracao (vazio)'),
 (N'processo.Regra',                          'A', 'PRESERVAR', N'Configuracao do motor de regras (vazia)'),
 (N'relatorio.Fonte',                         'A', 'PRESERVAR', N'Configuracao de relatorios (vazia)'),
 (N'relatorio.FonteCampo',                    'A', 'PRESERVAR', N'Configuracao de relatorios (vazia)'),
 (N'relatorio.Relatorio',                     'A', 'PRESERVAR', N'Configuracao de relatorios (vazia)'),
 -- B - cadastros necessarios
 (N'organizacao.Empresa',                     'B', 'PRESERVAR', N'Filiais: fronteira de acesso do sistema'),
 (N'seguranca.Usuario',                       'B', 'MANTER_ADMINISTRADORES', N'Ficam so os administradores de desenvolvimento'),
 (N'organizacao.Municipio',                   'B', 'SANITIZAR_IBGE', N'Ficam so os municipios com codigo do IBGE'),
 (N'processo.TipoProcesso',                   'B', 'APAGAR', N'Configuracao herdada do Vortice (decisao de 15/09/2026)'),
 (N'processo.Fase',                           'B', 'APAGAR', N'Configuracao herdada do Vortice (decisao de 15/09/2026)'),
 (N'processo.TipoTarefa',                     'B', 'APAGAR', N'Configuracao herdada do Vortice (decisao de 15/09/2026)'),
 (N'processo.Resultado',                      'B', 'APAGAR', N'Configuracao herdada do Vortice (decisao de 15/09/2026)'),
 (N'processo.MotivoDePerda',                  'B', 'APAGAR', N'Configuracao herdada do Vortice (decisao de 15/09/2026)'),
 (N'organizacao.LinhaDeNegocio',              'B', 'APAGAR', N'Configuracao herdada do Vortice (decisao de 15/09/2026)'),
 (N'organizacao.RegraDePotencial',            'B', 'APAGAR', N'Configuracao do territorio (decisao de 15/09/2026)'),
 -- C - operacionais
 (N'comercial.Cliente',                       'C', 'APAGAR', N'Clientes'),
 (N'comercial.Contato',                       'C', 'APAGAR', N'Contatos'),
 (N'comercial.ClienteContato',                'C', 'APAGAR', N'Relacionamento cliente x contato'),
 (N'comercial.Endereco',                      'C', 'APAGAR', N'Enderecos dos clientes'),
 (N'comercial.ClienteCarteira',               'C', 'APAGAR', N'Vinculo cliente x carteira'),
 (N'comercial.CanalContato',                  'C', 'APAGAR', N'Canais de contato'),
 (N'comercial.ConsentimentoComunicacao',      'C', 'APAGAR', N'Consentimentos'),
 (N'comercial.Lead',                          'C', 'APAGAR', N'Leads'),
 (N'comercial.Alerta',                        'C', 'APAGAR', N'Alertas'),
 (N'comercial.FaturamentoDoCliente',          'C', 'APAGAR', N'Faturamento importado do Protheus'),
 (N'comercial.FaturamentoSemCliente',         'C', 'APAGAR', N'Faturamento importado sem cliente'),
 (N'organizacao.Carteira',                    'C', 'APAGAR', N'Carteiras do legado'),
 (N'organizacao.CarteiraMunicipio',           'C', 'APAGAR', N'Vinculo carteira x municipio'),
 (N'organizacao.Meta',                        'C', 'APAGAR', N'Metas'),
 (N'organizacao.Praca',                       'C', 'APAGAR', N'Pracas'),
 (N'organizacao.HierarquiaComercial',         'C', 'APAGAR', N'Hierarquia comercial'),
 (N'organizacao.MunicipioDaAreaDeAtuacao',    'C', 'APAGAR', N'Area de atuacao importada da planilha'),
 (N'organizacao.ResponsavelPeloMunicipio',    'C', 'APAGAR', N'Responsaveis por municipio importados da planilha'),
 (N'processo.Processo',                       'C', 'APAGAR', N'Oportunidades e funil'),
 (N'processo.Tarefa',                         'C', 'APAGAR', N'Agenda'),
 (N'processo.Interacao',                      'C', 'APAGAR', N'Linha do tempo'),
 (N'processo.InteracaoParticipante',          'C', 'APAGAR', N'Participantes de interacao'),
 (N'processo.PassagemDeFase',                 'C', 'APAGAR', N'Historico de fases'),
 (N'processo.ItemDeProposta',                 'C', 'APAGAR', N'Itens de proposta'),
 (N'processo.VendaPerdida',                   'C', 'APAGAR', N'Vendas perdidas'),
 (N'frota.Equipamento',                       'C', 'APAGAR', N'Maquinas dos clientes'),
 (N'frota.VendaDeMaquina',                    'C', 'APAGAR', N'Vendas de maquina do ART'),
 (N'frota.VinculoDeClienteComEquipamento',    'C', 'APAGAR', N'Comprador na venda (ART)'),
 (N'frota.LeituraDeHorimetro',                'C', 'APAGAR', N'Leituras de horimetro'),
 (N'documento.Documento',                     'C', 'APAGAR', N'Documentos (sem arquivo fisico)'),
 (N'documento.Vinculo',                       'C', 'APAGAR', N'Vinculos de documento'),
 (N'seguranca.Equipe',                        'C', 'APAGAR', N'Equipes'),
 (N'seguranca.EquipeMembro',                  'C', 'APAGAR', N'Membros de equipe'),
 (N'seguranca.CompartilhamentoDeRegistro',    'C', 'APAGAR', N'Compartilhamentos de registro'),
 (N'metadado.Preenchimento',                  'C', 'APAGAR', N'Formularios preenchidos'),
 (N'metadado.Resposta',                       'C', 'APAGAR', N'Respostas de formulario'),
 (N'integracao.RegistroDeOrigem',             'C', 'APAGAR', N'Registros lidos do ART'),
 (N'integracao.CorrespondenciaDaOrigem',      'C', 'APAGAR', N'De-para do ART'),
 (N'integracao.CompradorPendente',            'C', 'APAGAR', N'Fila de compradores do ART'),
 (N'integracao.DivergenciaDeIntegracao',      'C', 'APAGAR', N'Divergencias do ART'),
 -- D - derivados e tecnicos
 (N'organizacao.AreaPlantadaNoMunicipio',     'D', 'APAGAR', N'Estatistica do IBGE carregada; recarregavel'),
 (N'auditoria.AlteracaoDeCampo',              'D', 'APAGAR', N'Trilha de alteracoes de registros removidos'),
 (N'auditoria.EventoDeAcesso',                'D', 'APAGAR', N'Eventos de acesso'),
 (N'integracao.ChaveExterna',                 'D', 'APAGAR', N'De-para de chaves do legado'),
 (N'integracao.MensagemDescartada',           'D', 'APAGAR', N'Fila de descarte das cargas'),
 (N'integracao.PontoDeSincronismo',           'D', 'APAGAR', N'Posicao das cargas antigas; manter faria uma carga recomecar do meio'),
 (N'integracao.ExecucaoDeSincronizacao',      'D', 'APAGAR', N'Historico do servico do ART (desabilitado)'),
 (N'integracao.Recepcao',                     'D', 'APAGAR', N'Area de recepcao'),
 (N'integracao.MensagemDeSaida',              'D', 'APAGAR', N'Fila de saida'),
 (N'processo.RegraExecucao',                  'D', 'APAGAR', N'Log de execucao de regras');

DECLARE @Antes      TABLE (Tabela nvarchar(300) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY, Linhas bigint NOT NULL);
DECLARE @Depois     TABLE (Tabela nvarchar(300) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY, Linhas bigint NOT NULL);
DECLARE @Violacoes  TABLE (Tabela nvarchar(400), Restricao nvarchar(400), Onde nvarchar(4000));
DECLARE @Problemas  TABLE (Problema nvarchar(1000) NOT NULL);
DECLARE @sqlContagem nvarchar(max);
DECLARE @sqlReseed   nvarchar(max);
DECLARE @Etapa       nvarchar(200) = N'guardas';
DECLARE @MunicipiosComIbge bigint;
DECLARE @MunicipiosSemIbge bigint;
DECLARE @FksRuinsAntes int;
DECLARE @ChecksRuinsAntes int;
DECLARE @Inicio datetime2(0) = SYSDATETIME();

BEGIN TRY
    -- 1. GUARDAS ---------------------------------------------------------------------------------
    IF DB_NAME() <> N'TracbelCrm'
        THROW 50001, N'Este script so roda no banco TracbelCrm. O arquivo restaurado e as copias de ensaio sao recusados. Nada foi alterado.', 1;

    IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
               WHERE NOT EXISTS (SELECT 1 FROM @Classificacao c WHERE c.Tabela = (s.name + N'.' + t.name) COLLATE DATABASE_DEFAULT))
        THROW 50002, N'Existe tabela no banco que este script nao classifica. Nada foi alterado.', 1;

    IF EXISTS (SELECT 1 FROM @Classificacao WHERE OBJECT_ID(Tabela) IS NULL)
        THROW 50003, N'O script classifica uma tabela que nao existe neste banco. Nada foi alterado.', 1;

    IF EXISTS (SELECT 1 FROM @Administradores a
               WHERE NOT EXISTS (SELECT 1 FROM seguranca.Usuario u
                                 WHERE u.NomePrincipal = a.NomePrincipal AND u.EstaAtivo = 1 AND u.ExcluidoEm IS NULL))
        THROW 50004, N'Um administrador da lista nao existe ativo em seguranca.Usuario. Nada foi alterado.', 1;

    -- 2. CONTAGEM ANTES --------------------------------------------------------------------------
    SET @Etapa = N'contagem antes';
    SELECT @sqlContagem = STRING_AGG(CONVERT(nvarchar(max),
               N'SELECT N''' + s.name + N'.' + t.name + N''', COUNT_BIG(*) FROM ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)),
           N' UNION ALL ')
    FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id;

    INSERT @Antes (Tabela, Linhas) EXEC sys.sp_executesql @sqlContagem;

    SELECT @MunicipiosComIbge = COUNT_BIG(*) FROM organizacao.Municipio WHERE CodigoIbge IS NOT NULL;
    SELECT @MunicipiosSemIbge = COUNT_BIG(*) FROM organizacao.Municipio WHERE CodigoIbge IS NULL;
    SELECT @FksRuinsAntes = COUNT(*) FROM sys.foreign_keys WHERE is_disabled = 1 OR is_not_trusted = 1;
    SELECT @ChecksRuinsAntes = COUNT(*) FROM sys.check_constraints WHERE is_disabled = 1 OR is_not_trusted = 1;

    -- 3. TRANSACAO -------------------------------------------------------------------------------
    BEGIN TRANSACTION;

    -- 4. O CICLO TAREFA x INTERACAO e o gestor dos administradores ------------------------------
    SET @Etapa = N'processo.Interacao.TarefaId = NULL';
    UPDATE processo.Interacao SET TarefaId = NULL WHERE TarefaId IS NOT NULL;

    SET @Etapa = N'gestor dos administradores';
    UPDATE u SET GestorId = NULL
    FROM seguranca.Usuario u
    JOIN @Administradores a ON a.NomePrincipal = u.NomePrincipal
    WHERE u.GestorId IS NOT NULL
      AND u.GestorId NOT IN (SELECT x.Id FROM seguranca.Usuario x JOIN @Administradores y ON y.NomePrincipal = x.NomePrincipal);

    -- 5. EXCLUSAO NA ORDEM NATURAL DAS CHAVES ESTRANGEIRAS --------------------------------------
    SET @Etapa = N'auditoria.AlteracaoDeCampo';            DELETE FROM auditoria.AlteracaoDeCampo;
    SET @Etapa = N'auditoria.EventoDeAcesso';              DELETE FROM auditoria.EventoDeAcesso;
    SET @Etapa = N'comercial.Alerta';                      DELETE FROM comercial.Alerta;
    SET @Etapa = N'comercial.CanalContato';                DELETE FROM comercial.CanalContato;
    SET @Etapa = N'comercial.ClienteCarteira';             DELETE FROM comercial.ClienteCarteira;
    SET @Etapa = N'comercial.ClienteContato';              DELETE FROM comercial.ClienteContato;
    SET @Etapa = N'comercial.ConsentimentoComunicacao';    DELETE FROM comercial.ConsentimentoComunicacao;
    SET @Etapa = N'comercial.FaturamentoDoCliente';        DELETE FROM comercial.FaturamentoDoCliente;
    SET @Etapa = N'comercial.FaturamentoSemCliente';       DELETE FROM comercial.FaturamentoSemCliente;
    SET @Etapa = N'documento.Vinculo';                     DELETE FROM documento.Vinculo;
    SET @Etapa = N'frota.LeituraDeHorimetro';              DELETE FROM frota.LeituraDeHorimetro;
    SET @Etapa = N'frota.VinculoDeClienteComEquipamento';  DELETE FROM frota.VinculoDeClienteComEquipamento;
    SET @Etapa = N'integracao.ChaveExterna';               DELETE FROM integracao.ChaveExterna;
    SET @Etapa = N'integracao.CompradorPendente';          DELETE FROM integracao.CompradorPendente;
    SET @Etapa = N'integracao.CorrespondenciaDaOrigem';    DELETE FROM integracao.CorrespondenciaDaOrigem;
    SET @Etapa = N'integracao.DivergenciaDeIntegracao';    DELETE FROM integracao.DivergenciaDeIntegracao;
    SET @Etapa = N'integracao.ExecucaoDeSincronizacao';    DELETE FROM integracao.ExecucaoDeSincronizacao;
    SET @Etapa = N'integracao.MensagemDescartada';         DELETE FROM integracao.MensagemDescartada;
    SET @Etapa = N'integracao.PontoDeSincronismo';         DELETE FROM integracao.PontoDeSincronismo;
    SET @Etapa = N'integracao.Recepcao';                   DELETE FROM integracao.Recepcao;
    SET @Etapa = N'integracao.RegistroDeOrigem';           DELETE FROM integracao.RegistroDeOrigem;
    SET @Etapa = N'metadado.Resposta';                     DELETE FROM metadado.Resposta;
    SET @Etapa = N'organizacao.AreaPlantadaNoMunicipio';   DELETE FROM organizacao.AreaPlantadaNoMunicipio;
    SET @Etapa = N'organizacao.CarteiraMunicipio';         DELETE FROM organizacao.CarteiraMunicipio;
    SET @Etapa = N'organizacao.HierarquiaComercial';       DELETE FROM organizacao.HierarquiaComercial;
    SET @Etapa = N'organizacao.Meta';                      DELETE FROM organizacao.Meta;
    SET @Etapa = N'organizacao.MunicipioDaAreaDeAtuacao';  DELETE FROM organizacao.MunicipioDaAreaDeAtuacao;
    SET @Etapa = N'organizacao.RegraDePotencial';          DELETE FROM organizacao.RegraDePotencial;
    SET @Etapa = N'organizacao.ResponsavelPeloMunicipio';  DELETE FROM organizacao.ResponsavelPeloMunicipio;
    SET @Etapa = N'processo.InteracaoParticipante';        DELETE FROM processo.InteracaoParticipante;
    SET @Etapa = N'processo.ItemDeProposta';               DELETE FROM processo.ItemDeProposta;
    SET @Etapa = N'processo.PassagemDeFase';               DELETE FROM processo.PassagemDeFase;
    SET @Etapa = N'processo.RegraExecucao';                DELETE FROM processo.RegraExecucao;
    SET @Etapa = N'processo.VendaPerdida';                 DELETE FROM processo.VendaPerdida;
    SET @Etapa = N'seguranca.CompartilhamentoDeRegistro';  DELETE FROM seguranca.CompartilhamentoDeRegistro;
    SET @Etapa = N'seguranca.EquipeMembro';                DELETE FROM seguranca.EquipeMembro;
    SET @Etapa = N'documento.Documento';                   DELETE FROM documento.Documento;
    SET @Etapa = N'frota.VendaDeMaquina';                  DELETE FROM frota.VendaDeMaquina;
    SET @Etapa = N'integracao.MensagemDeSaida';            DELETE FROM integracao.MensagemDeSaida;
    SET @Etapa = N'metadado.Preenchimento';                DELETE FROM metadado.Preenchimento;
    SET @Etapa = N'frota.Equipamento';                     DELETE FROM frota.Equipamento;
    SET @Etapa = N'processo.Tarefa';                       DELETE FROM processo.Tarefa;
    SET @Etapa = N'comercial.Endereco';                    DELETE FROM comercial.Endereco;
    SET @Etapa = N'processo.Interacao';                    DELETE FROM processo.Interacao;
    SET @Etapa = N'comercial.Lead';                        DELETE FROM comercial.Lead;
    SET @Etapa = N'organizacao.Municipio (sem codigo IBGE)'; DELETE FROM organizacao.Municipio WHERE CodigoIbge IS NULL;
    SET @Etapa = N'processo.Resultado';                    DELETE FROM processo.Resultado;
    SET @Etapa = N'processo.Processo';                     DELETE FROM processo.Processo;
    SET @Etapa = N'processo.TipoTarefa';                   DELETE FROM processo.TipoTarefa;
    SET @Etapa = N'comercial.Cliente';                     DELETE FROM comercial.Cliente;
    SET @Etapa = N'comercial.Contato';                     DELETE FROM comercial.Contato;
    SET @Etapa = N'organizacao.Carteira';                  DELETE FROM organizacao.Carteira;
    SET @Etapa = N'processo.Fase';                         DELETE FROM processo.Fase;
    SET @Etapa = N'processo.MotivoDePerda';                DELETE FROM processo.MotivoDePerda;
    SET @Etapa = N'organizacao.Praca';                     DELETE FROM organizacao.Praca;
    SET @Etapa = N'processo.TipoProcesso';                 DELETE FROM processo.TipoProcesso;
    SET @Etapa = N'seguranca.Equipe';                      DELETE FROM seguranca.Equipe;
    SET @Etapa = N'organizacao.LinhaDeNegocio';            DELETE FROM organizacao.LinhaDeNegocio;
    SET @Etapa = N'seguranca.Usuario (exceto administradores)';
    DELETE u FROM seguranca.Usuario u
    WHERE NOT EXISTS (SELECT 1 FROM @Administradores a WHERE a.NomePrincipal = u.NomePrincipal);

    -- 6. VALIDACAO DENTRO DA TRANSACAO ----------------------------------------------------------
    SET @Etapa = N'validacao';
    INSERT @Depois (Tabela, Linhas) EXEC sys.sp_executesql @sqlContagem;

    INSERT @Problemas (Problema)
    SELECT N'Tabela ' + c.Tabela + N' deveria ficar vazia e ficou com ' + CONVERT(nvarchar(20), d.Linhas) + N' linha(s).'
    FROM @Classificacao c JOIN @Depois d ON d.Tabela = c.Tabela
    WHERE c.Acao = 'APAGAR' AND d.Linhas <> 0;

    INSERT @Problemas (Problema)
    SELECT N'Tabela preservada ' + c.Tabela + N' mudou de ' + CONVERT(nvarchar(20), a.Linhas) + N' para ' + CONVERT(nvarchar(20), d.Linhas) + N' linha(s).'
    FROM @Classificacao c JOIN @Antes a ON a.Tabela = c.Tabela JOIN @Depois d ON d.Tabela = c.Tabela
    WHERE c.Acao = 'PRESERVAR' AND a.Linhas <> d.Linhas;

    IF (SELECT COUNT(*) FROM seguranca.Usuario) <> (SELECT COUNT(*) FROM @Administradores)
       OR EXISTS (SELECT 1 FROM @Administradores a WHERE NOT EXISTS (SELECT 1 FROM seguranca.Usuario u WHERE u.NomePrincipal = a.NomePrincipal AND u.EstaAtivo = 1))
        INSERT @Problemas (Problema) VALUES (N'seguranca.Usuario nao ficou exatamente com os administradores ativos.');

    IF EXISTS (SELECT 1 FROM organizacao.Municipio WHERE CodigoIbge IS NULL)
       OR (SELECT COUNT_BIG(*) FROM organizacao.Municipio) <> @MunicipiosComIbge
        INSERT @Problemas (Problema) VALUES (N'organizacao.Municipio nao ficou exatamente com os municipios que tinham codigo do IBGE.');

    IF (SELECT COUNT(*) FROM sys.foreign_keys WHERE is_disabled = 1 OR is_not_trusted = 1) > @FksRuinsAntes
        INSERT @Problemas (Problema) VALUES (N'Ha chave estrangeira desabilitada ou nao confiavel a mais que antes.');

    IF (SELECT COUNT(*) FROM sys.check_constraints WHERE is_disabled = 1 OR is_not_trusted = 1) > @ChecksRuinsAntes
        INSERT @Problemas (Problema) VALUES (N'Ha restricao CHECK desabilitada ou nao confiavel a mais que antes.');

    INSERT @Violacoes (Tabela, Restricao, Onde) EXEC (N'DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS, NO_INFOMSGS');
    INSERT @Problemas (Problema)
    SELECT N'Violacao da restricao ' + ISNULL(Restricao, N'?') + N' em ' + ISNULL(Tabela, N'?') + N': ' + ISNULL(Onde, N'')
    FROM @Violacoes;

    IF EXISTS (SELECT 1 FROM @Problemas)
        THROW 50010, N'A validacao encontrou problemas. Nada foi gravado.', 1;

    -- 7. COMMIT OU SIMULACAO ---------------------------------------------------------------------
    IF @Confirmar = 1
        COMMIT TRANSACTION;
    ELSE
        ROLLBACK TRANSACTION;

    -- 8. IDENTITY - so depois do COMMIT e so nas tabelas esvaziadas que ja tinham recebido linhas --
    IF @Confirmar = 1
    BEGIN
        SET @Etapa = N'reinicio dos IDENTITY';
        SELECT @sqlReseed = STRING_AGG(CONVERT(nvarchar(max),
                   N'DBCC CHECKIDENT (N''' + s.name + N'.' + t.name + N''', RESEED, 0) WITH NO_INFOMSGS;'),
               N' ')
        FROM sys.identity_columns ic
        JOIN sys.tables t ON t.object_id = ic.object_id
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        JOIN @Classificacao c ON c.Tabela = (s.name + N'.' + t.name) COLLATE DATABASE_DEFAULT
        WHERE c.Acao = 'APAGAR' AND ic.last_value IS NOT NULL;

        IF @sqlReseed IS NOT NULL EXEC sys.sp_executesql @sqlReseed;
    END

    -- RELATORIO ----------------------------------------------------------------------------------
    SELECT CASE WHEN @Confirmar = 1 THEN N'GRAVADO' ELSE N'SIMULACAO - DESFEITO' END AS Resultado,
           @@SERVERNAME AS Servidor,
           DB_NAME() AS Banco,
           (SELECT SUM(Linhas) FROM @Antes) AS LinhasAntes,
           (SELECT SUM(Linhas) FROM @Depois) AS LinhasDepois,
           @MunicipiosComIbge AS MunicipiosComIbgeMantidos,
           @MunicipiosSemIbge AS MunicipiosSemIbgeRemovidos,
           (SELECT STRING_AGG(NomePrincipal, N', ') FROM @Administradores) AS AdministradoresMantidos,
           (SELECT COUNT(*) FROM @Violacoes) AS ViolacoesDeRestricao,
           DATEDIFF(SECOND, @Inicio, SYSDATETIME()) AS Segundos;

    SELECT c.Categoria, c.Tabela, c.Acao, a.Linhas AS Antes, d.Linhas AS Depois, c.Motivo
    FROM @Classificacao c
    LEFT JOIN @Antes a ON a.Tabela = c.Tabela
    LEFT JOIN @Depois d ON d.Tabela = c.Tabela
    ORDER BY c.Categoria, c.Tabela;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    SELECT N'FALHOU - NADA FOI GRAVADO' AS Resultado, @Etapa AS Etapa, ERROR_NUMBER() AS Erro, ERROR_MESSAGE() AS Mensagem;
    SELECT Problema FROM @Problemas;
    SELECT Tabela, Restricao, Onde FROM @Violacoes;

    THROW;
END CATCH;
