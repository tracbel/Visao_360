# Relatórios, Views e BI do Vórtice CRM (411 views, motor QVW/GE_QVCons, e o custo de auditoria de 44,8M linhas)

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

O Vórtice não tem "motor de relatórios": tem um executor de SQL arbitrário chamado QVW (Query View). 134 relatórios vivem em `GE_QVCons.InstrSql` (coluna TEXT única, sobrescrita a cada edição), escritos numa linguagem de macro textual (`#LS1_CMD`, `#OPT1_ListChave`, `#LTA..#LTG`, `#vksSql_Nvl`) misturada com bind vars (`:DT1`, `:vksUsr_Cod`) e um preâmbulo procedural `SELECT CASE ... INTO :LTA FROM DUAL` que faz o relatório montar o próprio SQL em runtime. Não há versionamento (GE_QVConsHst é campo de notas: 69 linhas para 134 relatórios, última em abr/2025), não há segurança de linha (125 dos 134 relatórios não têm nenhum predicado de usuário; o picklist de "Carteiras" devolve a carteira de todo mundo), e o motor aceita `UPDATE`, `INSERT`, `EXEC` e `COMMIT` — há relatórios que são DML disfarçado. O padrão de staging (`IMP_REL_TBA101_PG2`) não é idempotente: em nenhum dos 134 corpos existe um `DELETE FROM`; a tabela saiu de 208 para 1.558 linhas entre jun e ago/2026. Só 10 dos 134 relatórios foram usados nos últimos 3 meses; 26 nunca rodaram; clonagem substitui versionamento (9 gerações do mesmo "Análise da Carteira de Pedidos"). Das 411 views, 290 são auto-geradas por metadados (181 `IV_Q$` de formulários, 53 `IV$P_` e 48 `IV$PG_` decodificando EAV) e nunca coletadas quando o formulário morre; apenas 47 (11%) são usadas pelo motor de relatórios. A camada semântica (`IV$S_*`) e a de LGPD (`GE$*_LGPD`) têm defeitos graves confirmados por query: `POSSUI_EMAIL` está invertido e a view "mascarada" reexpõe todo dado sensível em colunas `z_*` em claro. Auditoria: 44,8M de 85,5M linhas do banco (52,4%) são log; 96,8% de `GE_LOG_PROCESSO` são re-carimbos "(Atualizado em ...)" do job COBRANCA_CRIT sobre 19.936 processos (617 linhas de log por processo).

## Entidades / objetos mapeados

### `GE_QVCons`

**Funcao:** Catálogo do motor de relatórios QVW (Query View). 134 linhas — cada linha é UM relatório com o SQL inteiro embutido. É o coração do modelo de relatórios do Vórtice.

**Campos-chave:** SeqCons numeric(18) PK (existe SeqCons=0!); Cod varchar(10) (AK, ex. TBA_101XX3); Nome varchar(60); Descricao varchar(200); Adv varchar(250) (aviso em texto livre exibido antes de rodar — único preenchido é SeqCons 45); InstrSql text (O SQL, sobrescrito a cada save); Qrp varchar(150) (nome do arquivo de layout externo, sem caminho); Prvw, FtTipo, FtTam, Tit, Style; Sessao varchar(20) (agrupamento no menu — NULL em 92, vazio em 42); GTipo/GTit/GRdp/GEsq/GLeg/GCol (config de gráfico — zerada em 134/134); Dono varchar(20); IndSqlChg numeric(1); ChkS1/ChkS2 numeric(18); UltAtualizacao/DtaInicioUso/DtaUltimoUso datetime; UltUsuario varchar(20); QtdeUso decimal(8)

**Relacoes:** 1:N com GE_QVConsVar (parâmetros), GE_QVConsCol (metadados de coluna), GE_QVConsHst (notas). Referenciada de forma frouxa por GE_UsuarioPerm (CodAplicacao='GE_QVCONS', ChaveAplicacao=SeqCons) — 14 de 24 chaves são órfãs. Sem FK real.

### `GE_QVConsVar`

**Funcao:** Parâmetros de cada relatório (581 linhas). Cada parâmetro pode ter sua PRÓPRIA query de picklist — e essa query não tem filtro de escopo do usuário.

**Campos-chave:** SeqCons+Var PK; Var varchar(6) (nomes padronizados: DT1 95x, DT2 94x, LS1 84x, LS2 66x, OPT1 47x, SN1 37x, LS3 27x, LT1 26x, SN2 22x, SELX1 16x, NR1 11x); Ord; Descricao varchar(40); Padrao varchar(100) (aceita literais como 'Início do ano anterior (id)'); Instrucao; InstrSql varchar(1000) (SQL do picklist); Lista varchar(250); SQLCmd varchar(250); Externa/Tudo numeric(1); TpDado varchar(2); LimLow/CorLow/LimHig/CorHig (semáforo por faixa)

**Relacoes:** N:1 GE_QVCons. Nomenclatura por convenção define o comportamento: LS=lista simples, OPT=multi-seleção, DT=data, SN=sim/não, NR=número, LT=literal/fragmento SQL, SELX=seleção salva.

### `GE_QVConsCol`

**Funcao:** Metadados de coluna do resultado (rótulo, tipo, formato, cor). Praticamente inutilizada: 40 linhas cobrindo apenas 2 relatórios (SeqCons 85 e 128) dos 134.

**Campos-chave:** SeqCons+Pos PK; ColName varchar(30); Coluna varchar(30) (rótulo); TpDado varchar(2); Cor numeric(18); Formato varchar(20); Vars varchar(50)

**Relacoes:** N:1 GE_QVCons. Quando ausente (132/134), o motor exibe o alias cru do SELECT.

### `GE_QVConsHst`

**Funcao:** Aparenta ser versionamento mas é campo de ANOTAÇÃO livre. 69 linhas para 134 relatórios, no máximo 2 por relatório (SeqHst 0 e 1), última em 2025-04-10. Não guarda o SQL anterior.

**Campos-chave:** SeqCons+SeqHst PK; Detalhe text (texto livre, ex.: '06/03/15 13:06 - Criado a toque de caixa a pedido do João Henrique'); Usr varchar(20); Dta datetime

**Relacoes:** N:1 GE_QVCons

### `GE_Consulta / GE_ConsultaVar`

**Funcao:** Motor de relatórios LEGADO (2 e 3 linhas, ambas de 18/06/2012). Guarda no schema credenciais em texto claro para conexão a banco externo por relatório.

**Campos-chave:** Seqconsulta numeric(6) PK; Cod; Tipo char(1); Instrucaosql text; Banco varchar(15); Usuario varchar(15); Senha varchar(15) — SEM CRIPTOGRAFIA; Conexao varchar(1); Arqqrp varchar(150); Checksum; DtaUltUso; QtdeUso

**Relacoes:** GE_Consulta 1:N GE_ConsultaVar. Auto-relacionamento por Seqconsultapai (hierarquia de consultas).

### `GE_CONSSQL / GE_CONSVAR / GE_CONSVARLST / GE_CONSHST`

**Funcao:** Terceira geração do motor de consultas, esboçada e abandonada (1 / 0 / 0 / 0 linhas). Tinha modelo melhor: PK própria em cada tabela, SEQRELTEMPLATE, histórico com PK real.

**Campos-chave:** SEQCONSSQL PK; SEQRELTEMPLATE; NOME; QUERY text; PREVIEW; SESSAO varchar(50); QTDEUSO; INDRELSISTEMA numeric(1)

**Relacoes:** GE_CONSSQL 1:N GE_CONSVAR 1:N GE_CONSVARLST; 1:N GE_CONSHST

### `IMP_REL_TBA101 / IMP_REL_TBA101_PG2`

**Funcao:** Tabelas de staging do relatório de Carteira de Pedidos. HEAPS sem PK. Discriminadas apenas pelo login. Fonte do defeito de não-idempotência.

**Campos-chave:** codusuario nvarchar(50) (único discriminador, com índice NC); PROCESSO_N int; CARTEIRA nvarchar(60) (contém LOGIN, não carteira); CEN nvarchar(60); COMAR decimal(8) no PG2 (precisa virar varchar(30)); DATA_MARCADO_ENTREGUE nvarchar(10) (data em texto); ULT_HISTORICO_L nvarchar(max)

**Relacoes:** Sem PK, sem FK. Consumida por UNION em GE_QVCons SeqCons 170/171 e pela view VW_REL_TBA101 (que nem filtra por usuário). Escrita por INSERT inline (sem DELETE) e pela proc VTC_Gera_REL_TBA101 (com DELETE).

### `GE_LOG_PROCESSO`

**Funcao:** Maior tabela do banco: 12.713.138 linhas. Auditoria de IV_PROCESSO desde 06/06/2023. 96,8% são re-carimbos do campo derivado 'Resumo' pelo job de cobrança.

**Campos-chave:** SEQLOGTB numeric(18) PK clusterizada; TB varchar(25) (constante 'IV_PROCESSO'); KN1/KN2 numeric(18) (chave da linha auditada); KS varchar(40); DTALOG datetime; USR varchar(20); CODAPL varchar(30) (96,9% = 'SERVER'); OBS varchar(1000) (diff inteiro em texto concatenado); NIVEL char(1) (constante '5')

**Relacoes:** Polimórfica via TB+KN1/KN2/KS — sem FK. 4 índices NC: (TB,KN1), (TB,KS), (DTALOG), (TB,KN1,KN2,KS,DTALOG) — três com coluna líder de cardinalidade 1.

### `GE_LgTb`

**Funcao:** Log genérico da geração anterior. 11.861.777 linhas CONGELADAS entre 23/12/2018 e 05/06/2023, nunca expurgadas após a migração para GE_LOG_*.

**Campos-chave:** SEQLOGTB PK; Tb varchar(25); Kn1/Kn2; Ks varchar(40); DtaLog; Usr; CodApl varchar(20); Obs varchar(250); Nivel char(1)

**Relacoes:** Mesma chave polimórfica. Top Tb: IV_PROCESSO 5.452.512, EXT_TITULO 2.296.411, EXT_NFS 871.078, IV_HISTORICO 714.093, IV_INTERACAO 536.425, IV_QUESTIONARIO 508.679. 4 índices NC.

### `IV_AgendaLog`

**Funcao:** Auditoria de IV_Agenda: 11.049.475 linhas (2018→2026). 73% escritas por CodApl='SERVER'. Único índice é a PK — trilha de uma agenda exige varredura de 11M linhas.

**Campos-chave:** SEQLOGTB PK clusterizada (ÚNICO índice); Kn1/Kn2 numeric(18); Tb varchar(12); Ks varchar(40); DtaLog; Usr; CodApl varchar(30); Obs varchar(1000)

**Relacoes:** Polimórfica. Principais OBS: 'Realizada de [N] p/ [S]' 763.294, 'Tipo de agendamento' 90.568+46.210, 'REGISTRO INSERIDO.' 64.684, re-escritas de 'DETALHE: de [Critério [n - x_DUPLICATA]'.

### `GE_Log2`

**Funcao:** Terceira geração de log, ATIVA em paralelo desde 19/12/2011 (4.394.779 linhas). Esquema melhor que as outras: identifica estação e link tipado.

**Campos-chave:** SEQLOG PK; NroEmpresa; Sistema/Modulo/Aplicacao; CodUsuario; Data; Estacao varchar(20); Nivel numeric(1); Resumo varchar(30); Detalhe varchar(1000); LinkTipo varchar(20); LinkNro numeric(18); LinkSerie varchar(250)

**Relacoes:** Índices em LinkNro, Data, CodUsuario, Aplicacao — o único log com índice por usuário e por aplicação.

### `IV_CLIENTEPROPR`

**Funcao:** EAV das propriedades do cliente (frota, cultura, equipamento, origem de receita). Base das 53 views IV$P_*, que nomeiam os slots CAMPO1..CAMPO7 conforme SEQPROPRIEDADE.

**Campos-chave:** SeqPessoa, SeqPropriedade (discriminador: 9400=cultura, 9402=trator, 9403..9411=outros), Referencia, Ativo, Notas, CodOrigem, Identificador, CAMPO1..n, NUMERO1..n, DtaInclusao/UsuInclusao/DtaAlteracao/UsuAlteracao

**Relacoes:** 1 view IV$P_<NOME> por SEQPROPRIEDADE. Única tabela do banco com trigger de auditoria própria: VTC_T_AUDITORIA (habilitada).

### `IV_GLOBALPAR / IV_GLOBALPARCTRL`

**Funcao:** EAV de listas/parâmetros globais (marcas, modelos, famílias, faixas de potência). Base das 48 views IV$PG_*, filtradas por SEQGLBPAR.

**Campos-chave:** SEQPAR, SEQGLBPAR (discriminador, ex. 9401=tratores), PARAMETRO, NROEMPRESA, CAMPO1..6, SIMNAO1..2, LITERAL1, DTAALTERACAO/USUALTERACAO

**Relacoes:** IV_GLOBALPARCTRL 1:N IV_GLOBALPAR; 1 view IV$PG_<NOME> por SEQGLBPAR

### `IV_Q_<FORMULARIO> (175 tabelas)`

**Funcao:** Uma tabela física por formulário criado no designer. 32 delas têm ZERO linhas (formulários criados e nunca usados) mas todas carregam uma view.

**Campos-chave:** SEQQUESTIONARIO + uma coluna por pergunta do formulário. Ex.: IV_Q_ACOMP_VENDA_FINANC com COMAR decimal (campo antigo) e COMAR_ varchar(30) (campo novo) coexistindo sem migração

**Relacoes:** 1:1 com view IV_Q$<FORMULARIO> (181 views para 175 tabelas — 6 views órfãs). Join com IV_QUESTIONARIO (cabeçalho), IV_FORMULARIO, GE_PESSOA, IV_HISTORICO.

### `GE_ModuloPerm / GE_Aplicacao / GE_Modulo / GE_PolSeg / GE_UsuarioPerm`

**Funcao:** Modelo de segurança. Define quem pode ver, executar e CRIAR relatórios. Módulos QVW/BUILDER (3 usuários), QVW/EXEC (348), QVW/RUN (347), QVW/VIEWER (358), QVW/SETUP (349), QUERYVIEW/VIEWER (347), VORTICO/BIASSIST (6).

**Campos-chave:** GE_ModuloPerm(Sistema, Modulo, NroEmpresa, SeqUsuario, Permissao 'S'/'N'); GE_UsuarioPerm(CodAplicacao, ChaveAplicacao, NroEmpresa, Permissao, SeqUsuario) — ACL granular por objeto; GE_PolSeg(SeqPolSeg, Politica, Descricao) 11 políticas (0=Global, 1=Vórtice, 2=Atendentes, 3=Supervisores, 4=Administrador Sistema, 5=Temporario, 6=CEN, 7=Vendas Digitais, 8=Adm de Vendas, 9=DSI, 999999=* Master)

**Relacoes:** Views GE$USUARIOPERM / GE$MODULOPERM / GE$POLSEGPERM resolvem a herança: usuário direto OU membro de grupo via GE_MEMBRO; política 0 vale para quem não tem política. ACL de GE_QVCONS: 170 linhas, 24 chaves, 14 órfãs, default = permitir.

### `GEP_JOBAGD`

**Funcao:** Agendador do Servidor de Processos (25 jobs). Contém COBRANCA_CRIT, o gerador das 12,3M linhas de re-carimbo. NÃO existe job de entrega de relatório.

**Campos-chave:** SeqJOBAgd; Tipo (CICLICA/INTERNA); NroZUMBI; DtaInicio (SeqJOBAgd 12 = COBRANCA_CRIT com DtaInicio 22/05/2039 = desligado empurrando a data); Codigo (APPVTC/APPEXEC/PROCEDURE/EMAIL_SEND/COBRANCA_CRIT/SYNCSAT_*); Descricao; Aplicacao (caminho de .EXE, .LNK do Pentaho, ou nome de procedure); Prioridade

**Relacoes:** GEP_JobAgdExecLog (981.319 linhas de execução); GEP_JOBFILA/GEP_JOBFILALOG; GEP_EMAILSENDREL e GEP_EMAILSENTREL existem com 0 linhas (entrega de relatório nunca usada).

### `X_TOTVS_BI_FATURAMENTO_POS_VENDAS / X_TOTVS_CRM_FATURAMENTO / X_T_IMP_CRM_TITULO`

**Funcao:** Camada analítica MATERIALIZADA do ERP TOTVS (407.100 / 809.821 / 462.391 linhas). É o contraexemplo BOM: carga por procedure idempotente com janela.

**Campos-chave:** ORIGEM ('FAT_PECAS','DEV_PECAS'), FILIAL, DATA_EMISSAO_NF, NRO_NF, SERIE, COD_CLIENTE, CPF_CNPJ, COD_MARCA

**Relacoes:** Carregadas por X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS (DELETE por janela de -1 mês + INSERT), X_TOTVS_ATUALIZA_BI_FATURAMENTO_MAQUINAS. Backups esquecidos em produção: X_V_IMP_CRM_IMP_NF_BKP_18_09_2023 (415.762) e X_T_IMP_CRM_TITULO_bkp_11_04 (121.381).

### `DUAL`

**Funcao:** Tabela de compatibilidade Oracle (1 coluna, 1 linha). Usada pelos relatórios para o preâmbulo procedural: SELECT CASE ... INTO :LTA FROM DUAL.

**Campos-chave:** 1 coluna

**Relacoes:** Referenciada nos corpos de GE_QVCons.InstrSql (SeqCons 103, 104, 171 e outros).

### `GE_ParametroGlobal`

**Funcao:** Parâmetros globais do sistema (236 linhas). Contém o interruptor global da auditoria.

**Campos-chave:** Sistema, Modulo, NroEmpresa, Parametro, Valor, Criptografado, Dtaalteracao, Usualteracao. Chave relevante: INTERVISION/INTERVISION/UsaLogObjDynWF = 1 desde 12/01/2015 (liga o log de objeto dinâmico/workflow, sem granularidade por tabela ou campo)

**Relacoes:** Ao lado de GE_ParamLista (333 linhas, guarda o mapeamento RD Station).

## Achados

### 1. O motor de relatórios é o módulo QVW e guarda SQL cru numa coluna TEXT — sem versionamento, sem revisão, sem teste

O sistema de relatórios é o conjunto de módulos `QVW` em `GE_Modulo`: `QVW/BUILDER` (Query Builder), `QVW/EXEC` (Query Exec), `QVW/RUN`, `QVW/VIEWER`, `QVW/SETUP`, mais um legado `QUERYVIEW/ADMIN` e `QUERYVIEW/VIEWER`. O catálogo é `GE_QVCons` (28 colunas, 134 linhas). O SQL fica em `GE_QVCons.InstrSql` (`text`, 2147483647) — uma coluna única sobrescrita a cada save. Total: 404.513 bytes de SQL de negócio sem controle de versão. As colunas de 'versionamento' são teatro: `IndSqlChg numeric(1)` (flag booleano 'o SQL mudou'), `ChkS1`/`ChkS2 numeric(18)` (checksums), `UltAtualizacao datetime`. A tabela `GE_QVConsHst` (SeqCons, SeqHst, Detalhe text, Usr, Dta) parece histórico mas NÃO guarda o SQL anterior — guarda um texto livre de anotação, no máximo 2 linhas por relatório (SeqHst 0 e 1), 69 linhas para 134 relatórios, com conteúdo tipo '06/03/15 13:06 - Criado a toque de caixa a pedido do João Henrique para atender a Colorado.'. Última anotação: 2025-04-10. Ou seja: 66 dos 134 relatórios nunca tiveram uma linha de histórico, e nenhum tem o SQL antigo recuperável.

**Evidencia:** GE_Modulo (Sistema='QVW'); GE_QVCons/GE_QVConsHst em schema/colunas.csv; queries de contagem no banco de produção (134 relatórios, 69 linhas em GE_QVConsHst, MAX(Dta)=2025-04-10)

**Licao para a Tracbel:** EVITAR: SQL de relatório dentro de linha de tabela. No CRM novo, toda definição de relatório (query, parâmetros, colunas, layout) é ARTEFATO DE CÓDIGO versionado em Git, com PR e revisão, e publicado por deploy — não editável em produção por um formulário. Se for preciso permitir edição pela UI, gravar cada save como uma nova versão imutável (report_id + version, append-only) com autor, diff e possibilidade de rollback em 1 clique, e um campo `published_version_id` apontando para a versão ativa.

### 2. A linguagem de template é substituição textual de string — não parametrização

Censo dos tokens nos 134 corpos de SQL: `#vksSql_ConcatOwner` 119, `#LS1_CMD` 104, `#vksSql_Nvl` 101, `#vksSql_Concat` 86, `#LS2_CMD` 67, `#OPT1_ListChave` 48, `#LTA` 47, `#vksSql_ConcatHifen` 38, `#LTE` 35, `#LTB` 34, `#LS3_CMD` 25, `#SELX1_ListChave`/`#SELX1_CMD` 16 cada, `#LTC` 16, `#vksSql_Sysdate` 12, `#DT1`/`#DT2` 10, `#vksSql_Substr` 4, `#vksSql_Trunc` 2. Três famílias distintas: (1) `#vksSql_*` = macros de portabilidade de banco (Nvl→ISNULL/NVL, Concat→'+'/'||', ConcatOwner→'dbo.', Sysdate→GETDATE()/SYSDATE, Substr, Trunc) — o produto foi escrito para rodar em Oracle e SQL Server com o mesmo texto; (2) `#<VAR>_CMD` = expande para um FRAGMENTO DE SQL inteiro quando o usuário preenche o parâmetro, ou para string vazia quando não — daí construções frágeis como `WHERE #LS1_CMD HIS.DTAREALIZACAO >= :DT1`, onde o fragmento precisa terminar em 'AND'; (3) `#<VAR>_ListChave` = a lista de chaves selecionadas, interpolada crua dentro de `IN ( #OPT1_ListChave )`. Só os escalares usam bind real (`:DT1`, `:DT2`, `:LS1`, `:vksUsr_Cod`). Tudo que é multivalorado ou estrutural é concatenação de string.

**Evidencia:** grep de tokens em dump de GE_QVCons.InstrSql (134 relatórios); exemplos: SeqCons 154 `AND C.SEQCARTEIRA IN ( #OPT1_ListChave )`, SeqCons 6 `WHERE #LS1_CMD HIS.DTAREALIZACAO >= :DT1`

**Licao para a Tracbel:** EVITAR string-templating de SQL. ADAPTAR: no CRM novo o filtro é dado estruturado (JSON de predicados), não texto — o backend traduz para SQL parametrizado (ou para um query builder tipado tipo Kysely/Drizzle/QueryDSL) e listas multivaloradas viram array bind (`= ANY($1)` no Postgres) ou tabela temporária, nunca `IN (` + string + `)`. COPIAR a ideia boa por trás do `#<VAR>_CMD`: predicado que desaparece quando o parâmetro está vazio — mas implementar como composição de AST, não como concatenação.

### 3. O relatório monta o próprio SQL em runtime via preâmbulo procedural com FROM DUAL

O corpo do relatório não é uma query: é um script multi-statement. O SeqCons 171 (TBA_101XX4) começa com quatro blocos do tipo `SELECT CASE WHEN :SN1_X = '1' AND :SN3_X = '1' THEN ' ' WHEN :SN1_X = '1' THEN 'AND PRC.STATUS = ''FATURADO'' OR ADM.VENDA_FINANC_DATAENT IS NOT NULL' ELSE '' END INTO :LTA FROM DUAL;` — ou seja, o relatório calcula PEDAÇOS DE SQL como strings, guarda em variáveis :LTA..:LTD, e depois o motor injeta esses pedaços de volta no corpo principal nos pontos `#LTA`..`#LTD`. Isso explica os 47 `#LTA`, 35 `#LTE`, 34 `#LTB` do censo, e usos extremos como `FROM #LTF QST` (a própria tabela do FROM vem de string) e `#LTG IV_HISTORICO HIS ON ...` (o tipo do JOIN vem de string). Existe uma tabela `DUAL` (1 coluna, 1 linha) criada só para compatibilidade Oracle. Efeito colateral: nenhuma ferramenta consegue analisar estaticamente o relatório — nem plano de execução, nem lint, nem teste, nem detecção de coluna renomeada.

**Evidencia:** Dump de GE_QVCons.InstrSql, SeqCons 171 linhas ~9030-9055; tabela DUAL em schema/tabelas.csv (1 coluna, 1 linha)

**Licao para a Tracbel:** EVITAR completamente. O caso de uso legítimo (‘se o usuário marcou X, aplique este outro filtro’) é resolvido no CRM novo com um modelo de relatório declarativo: parâmetros tipados + regras condicionais declaradas em JSON/YAML, avaliadas pelo backend em cima de uma AST de query. Se o time realmente precisar de SQL bruto, esse SQL é um arquivo .sql versionado com placeholders nomeados, jamais um script que se auto-reescreve.

### 4. O motor de relatórios executa DML e stored procedures — 'relatório' é sinônimo de 'rode este SQL'

Varredura das 134 definições encontrou escrita em produção disfarçada de relatório: SeqCons 45 (`MAN001 - Atualizacao seqhistorico (vortice)`) é um `UPDATE IV_QUESTIONARIO SET SEQHISTORICO = XB.SEQHISTORICO FROM IV_QUESTIONARIO J2 INNER JOIN (...)` em massa, sem WHERE além do join; SeqCons 103 (`EXEC_PRC`) é `exec dbo.PR_PROPRIEDADE; commit; SELECT 'SUCESSO' FROM DUAL;` (3 execuções registradas); SeqCons 104 (`EXEC_PRC01 - Inativa título excluído no SISDIA`) é `exec dbo.PR_ATUALIZA_TITULO_INATIVO @LinkStr = '#LT1', @Retorno = 'A'; COMMIT;` — com o parâmetro `#LT1` vindo de string interpolada (9 execuções); SeqCons 170/171 fazem `EXEC VTC_Gera_REL_TBA101` e `INSERT INTO IMP_REL_TBA101_PG2 (...)`. A única proteção do MAN001 é a coluna `GE_QVCons.Adv` com o texto 'Somente pessoal autorizado da Vortice pode executar essa consulta.' — um aviso em texto livre é a ÚNICA barreira entre um usuário e um UPDATE em massa. É o único `Adv` preenchido nos 134 relatórios.

**Evidencia:** grep de INSERT/UPDATE/DELETE/EXEC no dump de GE_QVCons.InstrSql: linhas 2049, 5241, 5247, 8371, 8736, 9014, 9055; GE_QVCons.Adv (1 linha preenchida, SeqCons 45)

**Licao para a Tracbel:** EVITAR: o executor de relatórios do CRM novo tem de rodar com uma conexão READ-ONLY (usuário de banco sem INSERT/UPDATE/DELETE/EXECUTE), tecnicamente incapaz de escrever — não uma política e não um aviso. Rotinas de manutenção e cargas são JOBS separados, com sua própria trilha, aprovação e janela. COPIAR o campo `Adv` como conceito (aviso antes de rodar), mas para custo/volume ('esta consulta varre 12M linhas'), nunca como controle de acesso.

### 5. Segurança de dados: 125 dos 134 relatórios não têm nenhum predicado de usuário, e o picklist de carteira oferece a carteira de todo mundo

Só 8 relatórios contêm qualquer amarra com o usuário logado, e a variável do usuário (`:vksUsr_Cod`) aparece 5 vezes — TODAS dentro dos relatórios TBA_101, e apenas para filtrar a tabela de staging (`WHERE X1.CODUSUARIO = :vksUsr_Cod`), nunca como predicado de segurança sobre dado vivo. O picklist do parâmetro 'Carteiras' do SeqCons 170 é `SELECT CRT.CARTEIRA #vksSql_ConcatHifen USR.NOMEREDUZIDO, CRT.SEQCARTEIRA FROM IVS_DEPTO DPT JOIN IVS_CARTDEPTO CDP ... WHERE DPT.SEQDEPTO IN (2,10,25) AND (EMP.NOMEREDUZIDO = :LS2 OR :TUDO2 = 1)` — não há um único predicado ligando ao usuário logado, à sua carteira ou à sua hierarquia (`IV_VENDEDOR`/`IV_VENDEDOREMPR`). Distribuição de permissão: `QVW/VIEWER` 358 usuários, `QVW/SETUP` 349, `QVW/EXEC` 348, `QVW/RUN` 347, mas `QVW/BUILDER` apenas 3 (NETO.PRADO, MATHEUS.AUGUSTO, RICARDO.MORETTI — todos política 'Administrador Sistema'). A ACL por relatório existe (`GE_UsuarioPerm` com `CodAplicacao='GE_QVCONS'`) mas está podre: 24 chaves distintas, das quais 14 apontam para SeqCons que não existem mais (ChaveAplicacao 1,2,4,15,18,20,21,22,23,24,25,26,27,28 = ÓRFÃS) e só 10 correspondem a relatório real. O SeqCons 170, com 754 execuções por dezenas de usuários diferentes, não tem NENHUMA linha de ACL — logo o default do motor é PERMITIR. Além disso o picklist oferece departamentos (2,10,25) enquanto a query principal filtra `IVP.SEQDEPTO IN (2,25)`: escolher uma carteira do depto 10 devolve zero linhas sem explicação.

**Evidencia:** GE_QVConsVar SeqCons=170 Var='OPT1' (InstrSql do picklist); grep de :vksUsr_Cod (5 ocorrências, todas em TBA_101); GE_ModuloPerm por Sistema='QVW'; GE_UsuarioPerm WHERE CodAplicacao='GE_QVCONS' com LEFT JOIN GE_QVCons

**Licao para a Tracbel:** COPIAR a separação BUILDER vs VIEWER (quem escreve query ≠ quem consome), que o Vórtice acertou. EVITAR o resto: no CRM novo a segurança de linha é do MOTOR, não do autor da query — toda consulta de relatório passa por um filtro obrigatório de escopo (empresa + departamento + carteira/hierarquia do usuário) injetado pelo backend, impossível de omitir; e o picklist de qualquer dimensão hierárquica deriva do mesmo escopo (um CEN só enxerga a própria carteira e as dos subordinados). Default de ACL = DENY, e a ACL referencia o relatório por FK real, com cascade — nada de chave órfã. Aliás: fazer o picklist e a query lerem a MESMA definição de escopo elimina de origem a armadilha 'depto 10 oferecido mas nunca casado'.

### 6. Staging não idempotente: em 134 corpos de relatório não existe um único DELETE FROM

Busca por `DELETE FROM` em todos os 134 corpos: ZERO ocorrências. O padrão do TBA_101XX4 (SeqCons 171) é `INSERT INTO IMP_REL_TBA101_PG2 (CODUSUARIO, EMPRESA, CEN, ...) SELECT DISTINCT :vksUsr_Cod, ...` sem nenhuma limpeza prévia, e depois o relatório faz UNION com essa mesma tabela filtrando só `WHERE CODUSUARIO = :vksUsr_Cod`. Resultado medido: a tabela saiu de 208 linhas (snapshot de jun/2026) para 1.558 linhas hoje. `V:AMAURY` tem 208 linhas para 13 processos distintos = 16 cópias de cada. `RENATA.ABRA` 548 linhas / 487 processos, `RENATA.ABRA2` 405/401, `MATHEUS.AUGUSTO` 397/397. Contraste revelador: a procedure equivalente `VTC_Gera_REL_TBA101` FAZ o `DELETE FROM IMP_REL_TBA101 WHERE CODUSUARIO = @CODUSUARIO;` logo na primeira linha do corpo — o time sabe o padrão; a versão inline no relatório é que o esqueceu. Estruturalmente: `IMP_REL_TBA101` (29 col, 304 linhas, 18 usuários) e `IMP_REL_TBA101_PG2` (31 col, 1.558 linhas, 4 usuários) são HEAPS — sem PK, sem clustered index — com um único índice não-clusterizado em `codusuario`. O único discriminador é o login (`codusuario nvarchar(50)`): não há run_id, não há sessão, não há timestamp. Dois usuários com o mesmo login em duas máquinas, ou duas execuções concorrentes do mesmo usuário, se sobrescrevem/misturam.

**Evidencia:** grep 'delete from' no dump completo dos 134 InstrSql = 0 hits; OBJECT_DEFINITION('VTC_Gera_REL_TBA101') linha 17; SELECT COUNT(*) em IMP_REL_TBA101/_PG2; schema/pks.csv (nenhuma das duas tem PK)

**Licao para a Tracbel:** EVITAR o conceito inteiro: relatório não escreve em tabela compartilhada. Se o CRM novo precisar materializar resultado (para paginação, export grande, ou drill-down), usar um `report_run` com id próprio (UUID), `user_id`, `params_hash`, `started_at`, `expires_at`, e as linhas em `report_run_row(run_id, ...)` com FK e ON DELETE CASCADE + TTL automático. Assim a idempotência é estrutural (cada run é um id novo), a concorrência não colide, e a limpeza é um job de expiração — não uma linha de código que alguém pode esquecer de copiar.

### 7. VW_REL_TBA101: a mesma doença numa view, com janela de data fixa no código e UNION do staging sem filtro nenhum

A view `VW_REL_TBA101` (única da família `VW_`, 5.973 bytes, bem comentada com blocos '=== BLOCO 1 – QUERY PRINCIPAL ===') tem dois defeitos que a tornam permanentemente errada. (1) Janela de data HARDCODED no corpo da view: `WHERE VND.VENDA_DATA_PEDIDO >= CONVERT(DATE,'01/01/2026',103) AND VND.VENDA_DATA_PEDIDO <= CONVERT(DATE,'31/03/2026',103)` — a view só enxerga o 1º trimestre de 2026, para sempre, até alguém editar o DDL. (2) O `UNION ALL` do bloco 2 é `FROM IMP_REL_TBA101 X1 LEFT JOIN IV_Q$ACOMP_VENDA_FINANC X2 ON X2.PROCESSO = X1.PROCESSO_N` — SEM sequer o `WHERE CODUSUARIO = ...`. Ou seja: quem consultar essa view recebe o lixo acumulado de TODOS os 18 usuários do staging, misturado com um recorte fixo de Q1/2026. Ela ainda confirma a confusão de nomenclatura do chamado: `RESP.CODUSUARIO AS CARTEIRA` (a coluna 'CARTEIRA' é login de usuário) e `HIST.VENDEDOR AS CEN` via `OUTER APPLY (SELECT TOP 1 ... ORDER BY H.DTAREALIZACAO DESC)` — o CEN é o vendedor do ÚLTIMO histórico, não o dono da carteira. Também usa literais mágicos: `H2.RESULTADO = 3231` (data do pedido) e `H3.RESULTADO = 3239` (data de aprovação) embutidos em subqueries escalares.

**Evidencia:** OBJECT_DEFINITION de VW_REL_TBA101 (lido integralmente); docs/chamado-vortice-relatorio-tba101.md itens 1.2 e observação final

**Licao para a Tracbel:** EVITAR: view com literal de data. Toda janela temporal é PARÂMETRO. EVITAR: view que faz UNION de tabela transacional com tabela de trabalho. ADAPTAR: códigos como 3231/3239 viram constantes nomeadas no domínio (`RESULTADO_PEDIDO_TIRADO`, `RESULTADO_PEDIDO_APROVADO`) referenciadas por enum/tabela de domínio com FK, para que renomear ou depreciar um resultado quebre o build em vez de zerar o relatório em silêncio. E nomear coluna pelo que ela CONTÉM: se guarda login, chama-se `cen_login`, não `CARTEIRA`.

### 8. Bug de precedência AND/OR e data cravada dentro do SQL dinâmico da procedure de carga

Na `VTC_Gera_REL_TBA101` (criada 2025-10-29, modificada 2026-03-20, 16.724 bytes), o fragmento condicional é montado assim: `IF ISNULL(@LTA,N'') <> N'' SET @whereExtra += N' AND PRC.STATUS = @pLTA OR ADM.VENDA_FINANC_DATAENT IS NOT NULL   AND ADM.DtaRealizacao > CONVERT( DATE, ''16/03/2026'', 103)'`. Dois problemas na mesma linha: (a) sem parênteses, `AND` tem precedência sobre `OR`, então a condição vira `(...AND STATUS=@pLTA) OR (DATAENT IS NOT NULL AND DtaRealizacao > '16/03/2026')` — a opção 'Listar pedidos faturados' passa a trazer tudo que tem data de entrega, independente do resto do WHERE; (b) a data `16/03/2026` está cravada em literal dentro de string dentro de SQL dinâmico. Além disso a procedure monta `@whereExtra` concatenando `@LS1_CMD` e `@LS3_CMD` CRUS (`SET @whereExtra += NCHAR(10) + @LS1_CMD`) — esses vêm das macros `#LS1_CMD`/`#LS3_CMD` do motor QVW, ou seja, texto de SQL viajando do catálogo de relatórios até um `sp_executesql`. Só `@pLS2` e `@pLTA` são bind de verdade. A lista de carteiras chega como string e é quebrada por um hack de XML: `CAST('<i>' + REPLACE(@OPT1_ListChave, ',', '</i><i>') + '</i>' AS XML).nodes('/i')`.

**Evidencia:** OBJECT_DEFINITION('VTC_Gera_REL_TBA101'), corpo salvo em scratchpad/proc_tba101.txt; corresponde ao item 1.8 de docs/chamado-vortice-relatorio-tba101.md (linha 44 da procedure)

**Licao para a Tracbel:** EVITAR SQL dinâmico por concatenação, ponto. Se o CRM novo tiver filtros compostos, montar por AST/query-builder que já emite os parênteses corretos — o bug de precedência simplesmente não é expressável. Nenhuma data de negócio (corte de FY, virada de política) mora em código: mora em tabela de configuração com vigência (`valido_de`/`valido_ate`), consultável e auditável.

### 9. Layout externo .QRP referenciado só pelo nome do arquivo — colisão silenciosa e clones quebrados

69 dos 134 relatórios têm layout externo em `GE_QVCons.Qrp varchar(150)`, gravado apenas como NOME DE ARQUIVO (ex.: `JDE_101_CarteiraPedido.qrp`), sem caminho, sem versão, sem hash. O arquivo tem de existir no disco do terminal server. Três consequências medidas: (1) COLISÃO — `JDE_101XN_CarteiraPedido-XX2.qrp` é usado simultaneamente por SeqCons 0 (TESTE123), 168 (TBA_101XX_old, 1.507 usos) e 170 (TBA_101XX3, 754 usos): três SQLs diferentes apontando para UM layout; editar o layout para um muda os outros dois sem aviso. O mesmo com `JD_AF002_Afericao_Formulario.QRP` (SeqCons 8, 9, 10), `JD_AF003_Afericao_Evolutivo.QRP` (12, 13, 14), `JD_AF005_Ranking_Satisfacao.QRP` (16, 17, 19), `JD_AF006_Ranking_Peso.QRP` (29, 30, 31), `JDE_010_CoberturaCarteira.qrp` variantes, `JDE_017X_Ciclo...qrp` (147 e 158). (2) CLONES QUEBRADOS — o botão 'clonar relatório' prefixa o nome do arquivo: `TBA_002` tem `Qrp = 'Clone_'` e `TBA_003` tem `Qrp = 'Clone_Clone_'` — nomes que não são arquivo nenhum. Esses dois relatórios têm 87 e 21 execuções registradas. (3) ACOPLAMENTO DE DEPLOY — isso é exatamente o que quebra na migração de terminal server documentada em docs/RUNBOOK-novo-terminal-server.md: copiar as pastas não basta se os .QRP e o PATH do runtime Gupta não forem junto. Detalhe adicional: a configuração de gráfico existe no schema (`GTipo`, `GTit`, `GRdp`, `GEsq`, `GLeg`, `GCol`) mas está VAZIA nos 134 relatórios — nenhum gráfico configurado, a funcionalidade está morta. E `GE_QVConsCol` (metadados de coluna: rótulo, tipo, formato, cor) tem 40 linhas cobrindo apenas 2 relatórios (85 e 128): 132 dos 134 não têm sequer rótulo de coluna definido, o motor exibe o alias cru do SELECT.

**Evidencia:** SELECT SeqCons, Cod, Qrp FROM GE_QVCons WHERE Qrp <> '' (69 linhas); SELECT ... WHERE GTipo>0 OR GCol<>'' (0 linhas); GE_QVConsCol (40 linhas, SeqCons 85 e 128)

**Licao para a Tracbel:** EVITAR artefato de apresentação fora do banco identificado por nome. No CRM novo o layout é parte da definição versionada do relatório (mesmo commit que a query) e a renderização é server-side (HTML/PDF via template no repositório), sem dependência de arquivo no disco de uma máquina específica — isso mata a classe inteira de incidentes de terminal server. Rótulo, tipo, formato e ordem de coluna são obrigatórios na definição, não opcionais: se 132 de 134 não preenchem, o campo estava errado por ser opcional.

### 10. Inventário de uso: 7,5% dos relatórios estão vivos; clonagem substituiu versionamento

Dos 134 relatórios em `GE_QVCons` (usando `QtdeUso` e `DtaUltimoUso`): 26 NUNCA foram usados (19%), 38 com último uso há mais de 3 anos (28%), 41 entre 1 e 3 anos (31%), 19 entre 3 e 12 meses (14%), e apenas 10 usados nos últimos 3 meses (7,5%). Existe um relatório com `SeqCons = 0` (`TESTE123`, 'Análise da Carteira de Pedidos FY25', dono V:AMAURY) — chave primária zero em produção. Dois com corpo VAZIO (`SeqCons 119 REL_MIDIA` e `143 CONS00143`, DATALENGTH(InstrSql)=0, IndSqlChg NULL). Artefatos de teste em produção: TESTE123 (0), CST01 'teste (AUTO)' (135), TEST_014 (145), TESTE_NETO (146), 101X_TEST (151), TESTE_444 (164), RAS 'Rass' = `SELECT * FROM GE_FIGURA` (161). Duplicação de nome (8 grupos): 'Índice de Cobertura de Carteira por Potencial' 4 cópias (SeqCons 81, 145, 146, 153, 155 — na prática 5), 'Análise da Carteira de Pedidos FY25' 3 (0, 163, 170), 'Análise da Carteira de Pedidos' 2 (71, 142), 'Ciclo de Cobertura das Negociações por Perspectiva' 2 (147, 158, +164 clone), 'Acompanhamento Venda perdida' 2 (73, 160), 'Pedido de compra' 2 (162, 167), 'Índice de Visitas & Contatos' 2 (59, 88), 'Formulario Acompanhamento de vendas produto JDE' 2 (128, 132). Contando por código, o relatório 101 tem NOVE gerações vivas simultaneamente: JDE_101(71), JDE_101x(142), TBA_101X(144), 101X_TEST(151), TBA_101XX(163), TBA_101XX_(168, 'FY25_old'), TBA_101XX3(170), TBA_101XX4(171), TESTE123(0). Concentração de propriedade: `Dono` = VTCCONS (o fornecedor) em 58 relatórios, DIEGO.MARQUES 38, MATHEUS.AUGUSTO 14, V:AMAURY 10. E `Sessao` (campo de agrupamento no menu) está NULL em 92 e vazio em 42 — os 134 relatórios aparecem numa lista única e plana.

**Evidencia:** SELECT com faixas de DtaUltimoUso/QtdeUso em GE_QVCons; GROUP BY Nome HAVING COUNT(*)>1; GROUP BY Dono; GROUP BY Sessao

**Licao para a Tracbel:** COPIAR o instrumento: `QtdeUso`, `DtaInicioUso`, `DtaUltimoUso`, `UltUsuario` no catálogo são exatamente a telemetria que permite fazer essa faxina — o CRM novo deve ter isso desde o dia 1 (e mais: tempo médio de execução, linhas devolvidas, quem exportou). EVITAR: clone como mecanismo de evolução. Com versionamento de verdade, TBA_101XX3 é a v9 de um relatório, não o nono relatório. ADAPTAR: catálogo com ciclo de vida explícito (rascunho / publicado / depreciado / arquivado), arquivamento automático de relatório sem uso por N meses (com aviso ao dono), e proibição de publicar sem `Sessao`/categoria — porque um menu plano com 134 itens garante que ninguém acha o certo e clona o errado.

### 11. As 411 views: 290 são auto-geradas por metadados e nunca coletadas quando o objeto morre

Classificação completa das 411 views por família — (1) `IV_Q$*` = 181 views (44% do total), uma por formulário/questionário: cada `IV_Q$X` faz o join de `IV_Q_X` (tabela física gerada pelo designer de formulários) com `IV_QUESTIONARIO`, `IV_FORMULARIO` e `GE_PESSOA`, expondo cabeçalho + respostas achatadas. Existem 175 tabelas `IV_Q_*` para 181 views: 6 views ÓRFÃS sem tabela base (`IV_Q$ADM_FINANCEIRO_N`, `IV_Q$ANALISE_DE_CREDITO`, `IV_Q$COMISSAO_LOCACAO`, `IV_Q$PESQ_SATISFACAO_PECA`, `IV_Q$VENDA_LOCACAO`, `IV_Q$VENDA_N`); e 32 das 175 tabelas têm ZERO linhas (formulários criados e nunca usados) mas cada uma ainda carrega sua view. (2) `IV$P_*` = 53 views: decodificam o EAV `IV_CLIENTEPROPR` — ex.: `IV$P_TRATOR` é `SELECT ... CAMPO1 AS FAMILIA, CAMPO2 AS MODELO, CAMPO3 AS FAIXA_DE_POTENCIA, CAMPO6 AS ANO, CAMPO7 AS QUANTIDADE FROM IV_CLIENTEPROPR WHERE SEQPROPRIEDADE = 9402`. (3) `IV$PG_*` = 48 views: idem sobre `IV_GLOBALPAR`+`IV_GLOBALPARCTRL` filtrando `SEQGLBPAR`, mapeando CAMPO1..6/SIMNAO1..2/LITERAL1. (4) `BI_*` 32 + `bi_*` (minúsculo) 6 = 38 views de extração para BI. (5) `IV$<entidade>` ~23 views de leitura de negócio (IV$PROCESSO, IV$HISTORICO, IV$TITULO/EMPR/ORIGEM/VENC, IV$NFSAIDA/NFITEM/NFSCMPL, IV$OrdemServico/OSItem/OSSolicitacao, IV$FICHANEG, IV$RESULTADO/RESULTADOFULL, IV$PRODUTO, IV$_PONTUACAO...). (6) `GE$*` 16 + `GEL$*` 4 = 20 (LGPD/FULL, resolução de permissão, CEP/logradouro). (7) `IV$S_*` = 8 (camada semântica). (8) `IV$A_*` = 8 (atributos/classificações). (9) `X_*` = 7 (integração/BI TOTVS). (10) `V_C5SYS*` 6 + `SYSTABLE`/`SYSCOLUMN` 2 = 8 views de COMPATIBILIDADE DE CATÁLOGO — emulam o dicionário do SQLBase/Gupta ('C5') sobre o catálogo do SQL Server, resíduo da migração de banco do produto. (11) `ZZ_*` = 6 (cópias mortas de IV$NF*/IV$OS*, prefixo ZZ = 'não use'). (12) `VBI$` 2 (plano de contas do módulo BI). (13) `VW_REL_TBA101` 1. (14) Lixo nominal: `teste_felipe`, `tba_clientes`, `tba_usuarios`, `iv_w3_conglomerado`, `IMPV_VEICULO`, `IV_EQUIPE`, `IV_EQUIPEEMPR`. Cruzamento de uso: apenas 47 das 411 views (11%) aparecem em algum dos 134 corpos de relatório; ~350 não são referenciadas por NENHUM outro objeto do banco (`sys.sql_expression_dependencies`) — incluindo as 38 views BI_*, que portanto só podem ser consumidas por ferramenta externa ou por ninguém.

**Evidencia:** schema/views.csv (411) + sys.views/OBJECT_DEFINITION (dump de 595.888 bytes); comparação IV_Q$ vs IV_Q_ (181 vs 175); grep cruzado views × GE_QVCons.InstrSql = 47 hits; sys.sql_expression_dependencies

**Licao para a Tracbel:** COPIAR a ideia central e boa: quando o usuário cria um formulário ou uma propriedade, o sistema GERA automaticamente uma projeção legível e estável para consumo analítico — isso é o que torna um CRM configurável também reportável. EVITAR: (a) gerar objeto de banco por artefato de configuração sem coletor de lixo — no CRM novo, a projeção some quando o formulário é excluído, e formulário com zero uso há N meses entra em fila de arquivamento; (b) EAV com CAMPO1..CAMPO7 — usar JSONB tipado com schema validado, ou tabela por tipo; a view decodificadora deixa de ser necessária porque a coluna já nasce com nome; (c) manter `ZZ_`/`_BKP`/`teste_felipe` em produção — objeto morto se apaga, backup se faz em backup.

### 12. A camada semântica IV$S_* é a melhor ideia do Vórtice — e tem um bug de inversão confirmado em produção

As 8 views `IV$S_*` (`IV$S_PESSOA`, `IV$S_AGENDA`, `IV$S_HISTORICO`, `IV$S_CARTEIRA`, `IV$S_CONTATO`, `IV$S_ENDERECOADIC`, `IV$S_PESSOA_CLASSE`, `IV$S_RFV`) são exatamente a camada semântica que falta na maioria dos CRMs: traduzem código de máquina em rótulo de gente. `IV$S_PESSOA` faz `CASE STATUS WHEN 'P' THEN 'PROSPECT' WHEN 'F' THEN 'FALECIDO' WHEN 'I' THEN 'INATIVO' WHEN 'S' THEN 'SUSPECT' WHEN 'A' THEN 'ATIVO' ELSE '???' END`, `CASE FISICAJURIDICA WHEN 'F' THEN 'FÍSICA' WHEN 'J' THEN 'JURÍDICA' END`, `CASE ESTADOCIVIL WHEN 'C' THEN 'CASADO' ...`, converte flags 0/1 em 'SIM'/'NÃO', e ainda decompõe a data de nascimento em `DIA_NASC`/`MES_NASC`/`ANO_NASC` (para segmentação de aniversário). MAS: a view declara a lista de colunas posicionalmente e o SELECT tem DOIS aliases `POSSUI_EMAIL` seguidos — `CASE NAOPOSSUIEMAIL WHEN 1 THEN 'SIM' ELSE 'NÃO' END AS POSSUI_EMAIL` seguido de `CASE RECEBEEMAIL WHEN 1 THEN 'SIM' ELSE 'NÃO' END AS POSSUI_EMAIL`. Como a view nomeia por posição, a coluna `POSSUI_EMAIL` recebe `NAOPOSSUIEMAIL` — INVERTIDA. Confirmado por query: pessoas com `GE_Pessoa.NAOPOSSUIEMAIL = 1` (não tem e-mail) aparecem como `POSSUI_EMAIL = 'SIM'`. Afeta 572 pessoas de 119.298. Quem segmentar 'POSSUI_EMAIL = SIM' para uma campanha pega exatamente as pessoas SEM e-mail.

**Evidencia:** OBJECT_DEFINITION('IV$S_PESSOA'); query de verificação: SELECT p.NAOPOSSUIEMAIL, v.POSSUI_EMAIL FROM GE_Pessoa p JOIN [IV$S_PESSOA] v ... WHERE p.NAOPOSSUIEMAIL=1 → todos 'SIM'; SELECT COUNT(*) = 572 de 119.298

**Licao para a Tracbel:** COPIAR a camada semântica sem hesitar: no CRM novo deve existir uma projeção de leitura por entidade em que todo código já vem traduzido, todo booleano é booleano, e datas já vêm decompostas — é o que permite que gente de negócio monte relatório sem decorar que 'F' às vezes é FÍSICA e às vezes é FALECIDO. EVITAR o modo de falha: nunca nomear coluna por POSIÇÃO (`CREATE VIEW x (a,b,c) AS SELECT ...`); usar sempre alias no SELECT, e ter um teste automatizado que compare o mapeamento de cada CASE com a tabela de domínio. E notar o cheiro de fundo: `STATUS` e `FISICAJURIDICA` compartilham a letra 'F' com significados opostos — domínios distintos devem ter codificações distintas, ou melhor, ser enums separados.

### 13. A camada LGPD (GE$*_LGPD / GE$*_FULL) mascara e reexpõe o dado em claro na MESMA view

O produto tem pares de views para LGPD: `GE$PESSOA_FULL`/`GE$PESSOA_LGPD`, `GE$PESSOAEND_FULL`/`_LGPD`, `GE$PESSOAFONE_FULL`/`_LGPD`, `GE$EMAIL_FULL`/`_LGPD`, `GE$CONTATO_FULL`/`_LGPD`. A versão _LGPD aplica `dbo.fva_StrMaskLGPD(col, manter_inicio, manter_fim)` em NOMERAZAO(7,8), FANTASIA(5,5), PALAVRACHAVE(6,6), LOGRADOURO(8,6), NROLOGRADOURO(0,2), EMAIL(4,5), SKYPE(3,3); `FONENRO1/2/3 % 10000` e `NROCGCCPF % 10000` (mantém só os 4 últimos dígitos); INSCRICAORG por SUBSTRING+REPLICATE('*'); e a data de nascimento vira `fva_DateTimeFromParts(1804, MONTH(DTANASCFUND), DAY(DTANASCFUND), ...)` — preserva dia/mês (para aniversário) e apaga o ano. Boa engenharia. MAS a mesma view termina com `, NOMERAZAO AS z_NOMERAZAO, FANTASIA AS z_FANTASIA, PALAVRACHAVE AS z_PALAVRACHAVE, LOGRADOURO AS z_LOGRADOURO, NROLOGRADOURO AS z_NROLOGRADOURO, EMAIL AS z_EMAIL, SKYPE AS z_SKYPE, NROCGCCPF as z_NROCGCCPF, FONEDDD1/2/3 as z_..., FONENRO1/2/3 as z_..., DTANASCFUND as z_DTANASCFUND FROM GE_PESSOA` — TODO campo mascarado é reexposto em claro sob prefixo `z_`. Um `SELECT *` na view 'protegida' devolve o dado íntegro. Confirmado por query: SeqPessoa 91662 devolve `NOMERAZAO = 'RENATA ********* ********* ** **LVEIRA O'` e `z_NOMERAZAO = 'RENATA CRISTIANE GONCALVES DA SILVEIRA O'` na mesma linha. Segundo furo: o mascaramento é condicionado a `CASE FISICAJURIDICA WHEN 'F'` — 1.663 pessoas têm FISICAJURIDICA em branco, 32 têm '0' e 1 tem 'O' (1.696 no total, além de 116 com CPF de até 9 dígitos não marcadas como 'F'); nenhuma delas é mascarada. Confirmado: SeqPessoa 91663 'APPARECIDA COELHO MOROTTI' sai com nome e CPF (130175570001) idênticos nas colunas mascarada e crua.

**Evidencia:** OBJECT_DEFINITION('GE$PESSOA_LGPD'); query SELECT NOMERAZAO, z_NOMERAZAO, NROCGCCPF, z_NROCGCCPF FROM [GE$PESSOA_LGPD]; SELECT FISICAJURIDICA, COUNT(*) FROM GE_Pessoa → J 75.927 / F 41.675 / '' 1.663 / '0' 32 / 'O' 1

**Licao para a Tracbel:** COPIAR a estratégia de mascaramento preservando utilidade (dia/mês do aniversário sem o ano; últimos 4 dígitos do documento) — é boa e rara. EVITAR os dois furos: (a) o dado sensível NÃO pode existir na mesma projeção que a versão mascarada, em nenhuma coluna, com nenhum prefixo — no CRM novo isso vira Row/Column-Level Security no banco (Postgres RLS + políticas por role) ou duas projeções fisicamente distintas com GRANT separado, e a versão em claro exige role explícita + registro de acesso; (b) NUNCA condicionar proteção a um char de texto livre — pessoa física/jurídica é enum NOT NULL com CHECK, e a regra de máscara deve ser fail-closed: se o tipo é desconhecido, MASCARA. Aqui a regra é fail-open, e 1.696 pessoas passam.

### 14. As views BI_* são extração hand-made com literais mágicos e sem parâmetro — e ninguém no banco as referencia

38 views (32 `BI_*` + 6 `bi_*` minúsculas, duplicando conceitos: BI_OPORT_FUNIL vs bi_oportunidade vs bi_Oportunidades vs bi_oportunidade_produto). Convenção de coluna por prefixo de assunto para consumo em ferramenta de BI: `VDAE_` (103 ocorrências), `VDA_` (85), `ONE_` (66), `CART_` (49), `VPERD_` (42), `FUNIL_` (15), `AGDCEN_`, `VISITA_`, `FROTA_`, `APROVACAO_`. Exemplo real, `BI_OPORT_FUNIL`: cinco `UNION` de blocos quase idênticos, cada um produzindo `FUNIL_TIPO` fixo ('Monitoramento','Oportunidade','Pedido','Pedido','Venda') e `FUNIL_VALOR = 1`, todos com os mesmos literais cravados: `PDD.CODPROCESSO IN (7)`, `PRC.FASEORDEM > 35`, `ISNULL(PRC.DTAREALIZACAO, PDD.DTAULTRESULTADO) >= convert(datetime,'01/01/2010',103)`, `PRC.STATUS != 'VENDA PERDIDA' AND PRC.STATUS != 'DESISTIU DA COMPRA'`, `PSM.STATUS = 'PEDIDO REALIZADO'`. Ou seja: o funil inteiro depende de comparar STATUS por STRING contra literais em português — renomear um status na tela zera o funil sem erro. Versionamento por cópia também aqui: `BI_OPORT_FUNIL`, `BI_OPORT_FUNIL2`, `BI_OPORT_FUNIL2_BKP` convivem; idem `BI_VENDA_MAQ`, `BI_VENDA_MAQ_NEW`, `BI_VENDA_MAQ_ENC`. Nenhuma das 38 é referenciada por outro objeto do banco nem por qualquer relatório QVW — o consumo é 100% externo (ferramenta de BI apontando direto no banco de produção). O contraste bom está do lado TOTVS: `X_TOTVS_BI_FATURAMENTO_POS_VENDAS` (407.100 linhas) é tabela MATERIALIZADA, atualizada pela proc `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS`, que é IDEMPOTENTE por janela: calcula `@V_filtro = DATEADD(month,-1,MAX(DATA_EMISSAO_NF))`, faz `DELETE ... WHERE DATA_EMISSAO_NF >= @V_filtro AND ORIGEM IN ('FAT_PECAS','DEV_PECAS')` e só então reinsere. Junto: `X_TOTVS_CRM_FATURAMENTO` 809.821, `X_T_IMP_CRM_TITULO` 462.391, `X_V_IMP_CRM_IMP_NF` 815.854 — mais os backups esquecidos em produção `X_V_IMP_CRM_IMP_NF_BKP_18_09_2023` (415.762 linhas) e `X_T_IMP_CRM_TITULO_bkp_11_04` (121.381).

**Evidencia:** OBJECT_DEFINITION de BI_OPORT_FUNIL e X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS; censo de prefixos de alias nas views BI_*; schema/tabelas.csv (X_*); sys.sql_expression_dependencies

**Licao para a Tracbel:** COPIAR o padrão TOTVS (proc idempotente por janela → tabela materializada), não o padrão BI_* (view crua sobre OLTP). No CRM novo: camada analítica separada, com tabelas de fato/dimensão materializadas por job idempotente, chave de negócio estável e coluna de `carga_id`/`atualizado_em`; a ferramenta de BI NUNCA aponta para a base transacional. EVITAR: comparar status/fase por string literal — dimensão com chave surrogate e código estável, para que renomear o rótulo não quebre o funil. E `_BKP`/`_bkp_11_04`/`_NEW` não são estratégia de versionamento: são 537 mil linhas de lixo em produção.

### 15. 52,4% de todas as linhas do banco são log de auditoria — 44,8M de 85,5M

Somando todas as tabelas de log do catálogo: 44.804.915 linhas de 85.455.292 no banco inteiro (52,4%). Ranking: `GE_LOG_PROCESSO` 12.678.640 (12.713.138 na medição ao vivo — ainda crescendo), `GE_LgTb` 11.861.777, `IV_AgendaLog` 11.049.475, `GE_Log2` 4.394.779, `GE_LOG_HISTORICO` 1.015.045, `GEP_JobAgdExecLog` 981.319, `GE_LOG_PESSOA` 943.712, `GE_LOG_TRANS` 782.089, `GE_LOG_EXT` 622.075, `IV_CbrCobrancaTitLog` 304.684, `GE_LOG_CONFIG` 108.492, `IV_SMSLog` 27.430, `GE_LOG_CONTATO` 19.785. Para comparação, as entidades de negócio: `IV_PROCESSO` 1.190.252, `IV_Historico` 2.483.882, `IV_Agenda` 965.850, `GE_Pessoa` 119.298. Largura média de linha medida por amostragem de 100k: GE_LOG_PROCESSO 237 bytes, IV_AgendaLog 224, GE_LgTb 160 — logo só essas três representam ~7,4 GB de dados úteis, antes dos índices. O modelo de log é genérico e idêntico em todas: `(SEQLOGTB, TB varchar(25), KN1 numeric(18), KN2 numeric(18), KS varchar(40), DTALOG datetime, USR varchar(20), CODAPL varchar(30), OBS varchar(1000), NIVEL char(1))` — `TB`+`KN1/KN2/KS` é a chave polimórfica da linha auditada, e TODO o diff vira TEXTO CONCATENADO em `OBS`, ex.: 'Resumo de [(Atualizado em 12/06/2024 12:32:02) Referente] p/ [], Realizado de [0] p/ [1], Data do último status de [Cobrança Finalizada] p/ [], Descrição do status de [Cobrança Finalizada] p/ []'. Não há coluna de campo, nem valor_antigo, nem valor_novo, nem tipo. Responder 'quem mudou o Status do processo X e quando' exige `LIKE '%STATUS%'` sobre um varchar(1000) em 12,7M linhas. `NIVEL` é constante 5 em 100% das linhas de GE_LOG_PROCESSO — não classifica nada. A ativação é um interruptor global: `GE_ParametroGlobal` INTERVISION/`UsaLogObjDynWF = 1` desde 12/01/2015, sem granularidade por tabela ou por campo.

**Evidencia:** schema/tabelas.csv agregado por awk (44.804.915 / 85.455.292); COUNT_BIG ao vivo; AVG(DATALENGTH(...)) sobre TOP 100000; SELECT NIVEL, COUNT_BIG(*) FROM GE_LOG_PROCESSO → 5: 12.713.138; GE_ParametroGlobal 'UsaLogObjDynWF'

**Licao para a Tracbel:** EVITAR: log como texto concatenado numa coluna. No CRM novo a auditoria é ESTRUTURADA — `audit_event(id, entidade, entidade_id, acao, ator_id, origem, ocorrido_em)` + `audit_field_change(event_id, campo, valor_antes, valor_depois)` — consultável por índice, não por LIKE. EVITAR: auditoria como interruptor global. A política é POR CAMPO, declarada no modelo de domínio (`@Audited`), com nível (crítico/informativo) que realmente diferencie. E o dado de auditoria não mora na base transacional: vai para armazenamento append-only particionado por mês, com retenção definida por política legal (ex.: 5 anos) e expurgo/arquivamento automático — não para crescer até ser metade do banco.

### 16. 96,8% de GE_LOG_PROCESSO são re-carimbos de um campo denormalizado por um job de cobrança

Composição de `GE_LOG_PROCESSO` (12.713.138 linhas): 100% com `TB = 'IV_PROCESSO'` (um único valor), 96,9% com `CODAPL = 'SERVER'` (12.337.385). Por padrão de OBS: 12.303.969 linhas (96,8%) contêm '(Atualizado em' — são re-escritas do campo denormalizado `Resumo` do processo, cujo texto é do tipo '(Atualizado em dd/mm/aaaa hh:mm:ss) Referente a N títulos no total de R$ X'; contra apenas 17.162 de mudança de STATUS, 16.527 de FASE, 336 de PERSPECTIVA e 375.144 de outros. Essas 12,3M linhas se concentram em apenas 19.936 processos distintos = 617 linhas de auditoria por processo, num universo de 1.190.252 processos (1,7% dos processos geram 96,8% do volume). O gerador é o job `COBRANCA_CRIT — REALIZA O PROCESSAMENTO DOS CRITÉRIOS DE COBRANÇA` (GEP_JOBAGD SeqJOBAgd 12, tipo CICLICA, prioridade 5): a cada ciclo ele recalcula o resumo de cobrança, o timestamp dentro do texto muda, o auditor genérico enxerga 'campo mudou' e grava. Distribuição temporal: 2023 2.601.787 / 2024 6.688.755 / 2025 3.346.795 / 2026 75.801 — a queda de 2026 coincide com `DtaInicio` do job empurrada para 22/05/2039, ou seja, ele foi desligado empurrando a data. Agravante de armazenamento: a tabela tem 4 índices não-clusterizados além da PK — `GE_LG_PRC_IE1_3 (TB, KN1)`, `GE_LG_PRCIE2_3 (TB, KS)`, `GE_LG_PRCIE4_3 (DTALOG)` e `GE_LG_PRCIE3_3 (TB, KN1, KN2, KS, DTALOG)` — e como TB tem cardinalidade 1, três desses índices têm coluna líder constante: pagam espaço e custo de escrita em 12,7M linhas sem oferecer seletividade. `IV_AgendaLog` (11,0M linhas) tem o problema inverso: SÓ a PK clusterizada em SEQLOGTB, nenhum índice em (Tb, Kn1) — consultar a trilha de UMA agenda é varredura completa de 11M linhas. Dela, 8.098.962 linhas (73%) também são `CodApl='SERVER'`, com picos em 2023 (2,23M), 2024 (3,45M) e 2025 (1,96M), dominadas por 'Realizada de [N] p/ [S]' (763.294), 'REGISTRO INSERIDO.' (64.684) e re-escritas de 'DETALHE: de [Critério [26 - 1_DUPLICATA]...' — de novo o motor de cobrança.

**Evidencia:** GROUP BY TB/CODAPL/NIVEL/YEAR(DTALOG) em GE_LOG_PROCESSO; CASE por padrão de OBS (12.303.969 restamps); COUNT(DISTINCT KN1)=19.936; GEP_JOBAGD SeqJOBAgd 12 (COBRANCA_CRIT, DtaInicio 22/05/2039); sys.indexes/sys.index_columns

**Licao para a Tracbel:** A lição central: NÃO AUDITAR CAMPO DERIVADO. No CRM novo, um resumo/rollup como 'N títulos no total de R$ X' é campo CALCULADO (view, coluna gerada ou cache com `calculado_em` separado do dado), fora do escopo de auditoria por definição — e nunca carrega um timestamp no próprio texto, porque isso transforma toda releitura em 'mudança'. Complementar: (a) job automatizado NÃO escreve com identidade de usuário genérica — usa ator de sistema com trilha própria e mais barata; (b) o auditor compara valor semântico, não string, e descarta diffs vazios; (c) toda tabela com mais de ~1M linhas passa por revisão de índice — coluna líder com cardinalidade 1 é índice inútil, e tabela de trilha sem índice pela chave da entidade auditada é trilha que ninguém consegue consultar.

### 17. GE_LgTb: 11,9M linhas congeladas desde jun/2023 que ninguém expurgou depois da migração

`GE_LgTb` é o log genérico ANTIGO: 11.861.777 linhas, período 23/12/2018 17:30 a 05/06/2023 16:30 — congelado. Em 06/06/2023 o produto migrou para tabelas de log por entidade: `GE_LOG_PROCESSO` (primeira linha 06/06/2023 07:59), `GE_LOG_TRANS` (06/06/2023 08:01), `GE_LOG_EXT` (06/06/2023 08:25); `GE_LOG_HISTORICO`, `GE_LOG_PESSOA` e `GE_LOG_CONFIG` retêm dado desde 2018 (foram particionadas a partir da GE_LgTb). Composição do que ficou parado: `IV_PROCESSO` 5.452.512, `EXT_TITULO` 2.296.411, `EXT_NFS` 871.078, `IV_HISTORICO` 714.093, `IV_INTERACAO` 536.425, `IV_QUESTIONARIO` 508.679, `GEP_EMAILSENT` 398.913, `GE_PESSOA` 384.534. Ela carrega 4 índices não-clusterizados (`Tb,Kn1`, `Tb,Ks`, `DtaLog`, `Tb,DtaLog`) que continuam ocupando espaço em backup e restore para dados que ninguém consulta. Estimativa: ~1,9 GB de dados + índices. Ao lado, `GE_Log2` (4,39M linhas, 19/12/2011 a hoje) é AINDA OUTRO formato de log, com esquema diferente (`NroEmpresa, Sistema, Modulo, Aplicacao, CodUsuario, Data, Estacao, Nivel, Resumo, Detalhe, LinkTipo, LinkNro, LinkSerie`) — mais rico (tem estação e link tipado) e ainda ativo em paralelo. Ou seja, o CRM tem TRÊS gerações de auditoria convivendo: GE_Log2 (2011→), GE_LgTb (2018–2023) e GE_LOG_* (2023→), sem migração nem expurgo.

**Evidencia:** MIN/MAX(DtaLog) por tabela de log; GROUP BY Tb em GE_LgTb; sys.indexes; schema/colunas.csv para GE_Log2

**Licao para a Tracbel:** EVITAR: mudar o esquema de auditoria sem plano de migração e sem data de expurgo do antigo. No CRM novo, decidir a política de retenção ANTES de ligar a auditoria (quanto tempo, onde, quem pode ler) e implementar particionamento por período desde o início, para que 'apagar 2018' seja um DROP PARTITION e não um DELETE de 5M linhas. Se um dia o formato mudar: migração + corte + descomissionamento no mesmo projeto, nunca deixar a geração anterior viva 'por segurança'.

### 18. Resíduos do motor legado: senha em texto claro no schema, DUAL, e views de compatibilidade com o banco antigo

Ao lado do QVW convivem dois motores mortos. (1) `GE_Consulta` (29 colunas, 2 linhas: 'teste' com 30 usos e 'PSQ01_Evolucao da Pesquisa de Qualidade' com 48, ambas incluídas em 18/06/2012) + `GE_ConsultaVar` (10 col, 3 linhas). O esquema do motor antigo é mais alarmante que o novo: ele guardava, POR RELATÓRIO, credenciais para conectar em outro banco — `Banco varchar(15)`, `Usuario varchar(15)`, `Senha varchar(15)`, `Conexao varchar(1)`. Senha de banco, em coluna varchar, sem criptografia, no catálogo de relatórios. Nas 2 linhas vivas os campos estão vazios, mas o campo existe e o motor sabia lê-lo. (2) `GE_CONSSQL` (1 linha, 'ADV_001') + `GE_CONSVAR` (0) + `GE_CONSVARLST` (0) + `GE_CONSHST` (0) — uma terceira geração, esboçada e abandonada; note que essa terceira tinha `SEQCONSHST` como PK real e `SEQRELTEMPLATE` (referência a template), sinal de que a Vórtice já tentou consertar o modelo e parou. (3) Resíduos de portabilidade: a tabela `DUAL` (1 coluna, 1 linha) para compatibilidade Oracle; e 8 views de compatibilidade de dicionário — `V_C5SYSTABLE`, `V_C5SYSCOLUMN`, `V_C5SYSINDEX`, `V_C5SYSPK`, `V_C5SYSCONSTRAINT`, `V_C5SYSREFERENCE`, `SYSTABLE`, `SYSCOLUMN` — que emulam o catálogo do SQLBase/Gupta ('C5') sobre `sys.*` do SQL Server. O cliente desktop consulta essas views achando que está num banco Gupta. (4) A distribuição agendada de relatório existe no schema mas nunca foi usada: `GEP_EMAILSENTREL` e `GEP_EMAILSENDREL` têm 0 linhas; o agendador `GEP_JOBAGD` (25 jobs) só dispara .EXE, atalhos .LNK do Pentaho (`D:\VORTICE\PDI\KITCHEN_PECAS.LNK`) e stored procedures — nenhum job entrega relatório a ninguém.

**Evidencia:** GE_Consulta (2 linhas) e colunas Banco/Usuario/Senha em schema/colunas.csv; GE_CONSSQL/GE_CONSVAR/GE_CONSVARLST/GE_CONSHST; tabela DUAL; views V_C5SYS*/SYSTABLE/SYSCOLUMN; GEP_JOBAGD (25 linhas); GEP_EMAILSENTREL/GEP_EMAILSENDREL = 0 linhas

**Licao para a Tracbel:** EVITAR: credencial em coluna de aplicação, sob qualquer pretexto — no CRM novo, conexão externa é um `datasource` nomeado, com segredo em cofre (Key Vault / Secrets Manager) e referência por id; a aplicação nunca vê a senha. EVITAR: deixar geração anterior de um subsistema viva 'por via das dúvidas' — três motores de consulta coexistindo é o cheiro de que ninguém tem autoridade para apagar. COPIAR o que faltou: entrega agendada de relatório (e-mail/Teams/link com expiração) é requisito de dia 1 no CRM novo — sem ela, o usuário exporta CSV e a governança do dado acaba ali.

### 19. O que o CRM novo precisa oferecer no lugar do QVW (síntese acionável)

O modelo do Vórtice falha por cinco causas estruturais, todas mensuradas acima: (a) SQL cru como unidade de configuração, editável em produção, sem versão — 134 relatórios, 0 versões recuperáveis; (b) composição por concatenação de string em vez de parametrização — 48 `#OPT1_ListChave` interpolados, bug de precedência AND/OR na proc de carga; (c) ausência de escopo de dados no motor — 125/134 sem predicado de usuário, picklist devolvendo carteira alheia, 347 usuários com EXEC e ACL default-allow com 14 chaves órfãs; (d) materialização por staging compartilhada sem idempotência nem chave de execução — 0 `DELETE FROM` em 134 corpos, PG2 de 208 → 1.558 linhas; (e) sem ciclo de vida — clone como versionamento (9 gerações do 101), 26 relatórios nunca usados, 290 views auto-geradas nunca coletadas, artefatos 'teste_felipe'/'TESTE123'/'Clone_Clone_' em produção. O substituto tem sete peças: (1) MODELO SEMÂNTICO versionado em código (entidades, métricas, dimensões, com códigos já traduzidos como as `IV$S_*` fazem) sobre o qual o usuário monta consulta arrastando campo — o usuário nunca escreve SQL; (2) MOTOR que compila esse modelo para SQL parametrizado e injeta OBRIGATORIAMENTE o predicado de escopo (empresa + departamento + carteira/hierarquia), com conexão read-only; (3) CATÁLOGO com ciclo de vida (rascunho/publicado/depreciado/arquivado), dono, categoria obrigatória, e telemetria de uso — copiando `QtdeUso`/`DtaUltimoUso`/`UltUsuario`, que é a única coisa do GE_QVCons que permitiu esta análise; (4) VERSIONAMENTO real: cada publicação é imutável, com diff, autor e rollback; (5) EXECUÇÃO isolada: `report_run` com id próprio, params, TTL e limite de linhas/tempo — nada de tabela de staging por login; (6) ENTREGA: agendamento com envio por e-mail/link expirável e export auditado (quem exportou o quê, quando) — a peça que o Vórtice tem só no schema vazio; (7) CAMADA ANALÍTICA separada, materializada por job idempotente por janela (copiando exatamente o padrão da `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS`), para que o BI nunca aponte no OLTP.

**Evidencia:** Síntese dos achados acima; todos os números citados foram medidos em produção (base CRM, conta CRM_Leitura) ou no catálogo em c:\projetos\vortice-crm-agent\schema\

**Licao para a Tracbel:** Roadmap direto do que copiar (telemetria de uso, camada semântica IV$S_*, mascaramento LGPD que preserva utilidade, separação BUILDER/VIEWER, proc de carga idempotente por janela), do que evitar (SQL em coluna, macro textual, staging por login, auditoria de campo derivado, clone como versão, view com data cravada) e do que adaptar (EAV → JSONB tipado; view auto-gerada → projeção com coleta de lixo; ACL por relatório → escopo no motor com default deny).

## Lacunas declaradas

- Não consegui medir o TAMANHO EM DISCO das tabelas: a conta CRM_Leitura não tem VIEW DATABASE STATE (sys.dm_db_partition_stats negado). As estimativas de GB citadas são derivadas de linhas × largura média medida por amostragem de 100k linhas (GE_LOG_PROCESSO 237 B, IV_AgendaLog 224 B, GE_LgTb 160 B), SEM contar índices — o valor real é maior.
- Não consegui confirmar por CÓDIGO que o default da ACL de relatórios é 'permitir'. A conclusão é inferência forte a partir da evidência: SeqCons 170 tem 754 execuções por vários usuários distintos e NENHUMA linha em GE_UsuarioPerm; e 124 dos 134 relatórios não têm entrada de ACL. Confirmar isso exige ler o binário do cliente QVW ou perguntar à Vórtice.
- Não sei QUAL ferramenta externa consome as 38 views BI_*/bi_*. Nenhuma delas é referenciada por outro objeto do banco nem por relatório QVW. Os prefixos de coluna (ONE_, VDA_, VDAE_, CART_, VPERD_) sugerem convenção de um produto específico, mas não consegui identificá-lo. Verificar em sys.dm_exec_query_stats / Extended Events / logins ativos exigiria permissão de DBA.
- Não consegui inspecionar os arquivos .QRP (69 relatórios dependem deles). Eles estão no disco do terminal server, não no banco — não pude verificar quais existem, quais estão órfãos, nem confirmar que 'Clone_' e 'Clone_Clone_' não são arquivos válidos (é a inferência mais provável, mas não verificada em disco).
- Não medi TEMPO DE EXECUÇÃO nem volume de linhas devolvido por relatório. GE_QVCons só guarda contagem de uso (QtdeUso), não custo. Sem isso não dá para dizer quais relatórios são caros para o servidor — informação que o CRM novo deveria capturar.
- Não investiguei GE_ObjDinamico / GE_ObjDinAplic (122 linhas) a fundo — é o modelo de 'objeto dinâmico' que provavelmente controla QUAIS tabelas são auditadas e para qual GE_LOG_* cada uma roteia. Confirmar isso fecharia a explicação de por que GE_LOG_HISTORICO/PESSOA/CONFIG retêm dado desde 2018 enquanto PROCESSO/TRANS/EXT começam em 06/06/2023.
- Não li o corpo completo das 85 stored procedures nem das 51 funções escalares. Li integralmente só VTC_Gera_REL_TBA101 e o início de X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS. Podem existir outras cargas de BI ou de relatório com defeitos semelhantes (candidatas: BI_PROCESSO_333, BI_RELATORIO_PROSPECCAO, X_P_REL_001, X_BI_PROSPECCAO_MAQUINAS_*).
- Não confirmei se as 6 views IV_Q$ órfãs (ADM_FINANCEIRO_N, ANALISE_DE_CREDITO, COMISSAO_LOCACAO, PESQ_SATISFACAO_PECA, VENDA_LOCACAO, VENDA_N) estão de fato quebradas em runtime — a comparação foi feita contra o snapshot schema/tabelas.csv (jun/2026), não contra sys.tables ao vivo; as tabelas base podem ter sido criadas depois.
