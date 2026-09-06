# Usuários, permissões e multiempresa no Vórtice CRM (prefixos GE_ e IVC_) — modelo de segurança, escopo de registro, senha, criação de usuário e auditoria

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

O Vórtice tem 8 camadas de permissão empilhadas (módulo, aplicação/tela, registro de catálogo por usuário, registro de catálogo por política, campo/widget, comportamento por política, horário de acesso, obrigatoriedade de campo) — mas nenhuma delas é escopo de REGISTRO transacional. Não existe RLS no SQL Server (sys.security_policies = 0) e há apenas 1 trigger no banco inteiro: toda a segurança e toda a auditoria são feitas no cliente Gupta/BPM, então acesso direto ao banco ignora tudo. Identidade e grupo são a MESMA tabela (GE_Usuario.TipoUsuario U=1.068 / G=318), com grupos planos (sem aninhamento) em GE_Membro. Não há papel/perfil: IV_Operador tem 939 linhas com 51 flags char(1) e 370 combinações distintas — cada usuário é um floco de neve; a tabela que resolveria isso (IV_SegPerfil) está vazia, assim como todo o bloco IVC_ (EQUIPE, EQUIPEUSR, ATENDENTE = 0 linhas). A senha é uma string de 30 bytes não-imprimíveis SEM SALT: 663 usuários com senha produzem só 467 valores distintos, e 80 usuários compartilham exatamente o mesmo valor armazenado. Política de senha só existe para 1 das 11 políticas (CEN: força "Fraco", mín. 12 caracteres, 0 memorizadas) e nenhuma tem expiração. O escopo de visibilidade é resolvido em código a partir de 4 rotas paralelas e frágeis (IV_VENDEDOR.SeqUsuarioLider, IVS_Carteira.SeqUsrResp/SeqUrSuperv, IVS_DeptoEmpr.SeqUsrGerDepto, IVS_Pes) — e a rota de supervisor está morta (SeqUrSuperv NULL em 655/655 carteiras). Multiempresa é nominal: 18 filiais, toda tabela de permissão tem NroEmpresa, mas 340 dos 409 usuários ativos têm acesso a 17 empresas. A auditoria custa ~10 GB / 32 milhões de linhas sem retenção, registra bem mudanças de permissão, e não registra login (114 eventos desde 2017).

## Entidades / objetos mapeados

### `GE_Usuario`

**Funcao:** Identidade única para USUÁRIO e GRUPO (TipoUsuario 'U'=1.068 / 'G'=318). Guarda credencial, política de segurança, nível/status e os dois últimos logins. 35 colunas.

**Campos-chave:** SeqUsuario numeric(18) PK · CodUsuario varchar(20) · Nome · NomeReduzido · SeqPessoa numeric(10) (só 260 preenchidos) · Senha varchar(30) (sem salt) · Senha3 varchar(30) · ChkSum varchar(50) · LoginId varchar(20) (311 preenchidos) · LOGIN varchar(100) (0 preenchidos) · TipoUsuario char(1) U/G · RegistrarLog char(1) ('N' em 100%) · DTALIMITEUSO (NULL em 100%) · Nivel decimal(1) (0=desativado, 3=normal, 4/5, 7, 8=master) · SeqPolSeg decimal(6) (710 NULL) · Ulttrocasenha · INDSILO (0 em 100%) · DTALOGIN/DTALOGINANT (só 2 últimos) · DTAINCLUSAO/USUINCLUSAO/DTAALTERACAO/USUALTERACAO

**Relacoes:** FK SeqPolSeg → GE_PolSeg. Referenciada por GE_Permissao, GE_ModuloPerm, GE_CampoPerm, GE_UsrParam, GE_Membro (Usuario e Grupo), GE_UsuarioLink, GE_PESSOAATIVAUSR, IVC_ATENDENTE, IVC_EQUIPEUSR, IV_USRSTATUS, IV_USRPUSH, IVS_UsrMeta, IVS_Depto.SeqUsrDirDepto, IVS_DeptoEmpr.SeqUsrGerDepto/SeqUsrDirDepto, IV_ProjEquipe, IV_AcaoMon, IV_Distribui, IV_CobrCrit, IV_AtdBloq, IV_ConhecLeitura, IV_AgdRec. NÃO há FK de GE_UsuarioPerm.SeqUsuario (261 órfãos).

### `GE_Membro`

**Funcao:** Vínculo usuário↔grupo. Plano (sem grupos aninhados). 1.584 linhas / 462 usuários / 318 grupos.

**Campos-chave:** Grupo numeric(18) (→ GE_Usuario com TipoUsuario='G') · Usuario numeric(18) (→ GE_Usuario com TipoUsuario='U') · INDATIVO numeric(1) (0/NULL em 100% — flag nunca usado)

**Relacoes:** FK Usuario → GE_Usuario. Grupo NÃO tem FK. É o que a view GE$USUARIOPERM usa para expandir 38.680 linhas de ACL em 4.382.561.

### `GE_PolSeg / GE_PolSegItem / GE_PolSegItLst / GE_PolSegCtrl / GE_PolSegAces / GE_PolSegParams / GE_CampoExig`

**Funcao:** Motor de POLÍTICA DE SEGURANÇA: 11 políticas nomeadas × catálogo de 108 itens tipados × 254 valores efetivos, mais janela de horário e obrigatoriedade de campo.

**Campos-chave:** GE_PolSeg(SeqPolSeg decimal(6) PK, Politica varchar(30), Descricao varchar(250)) · GE_PolSegItem(Modulo varchar(10), Item varchar(20) = a chave real, Ordem varchar(10) = código de exibição, Tipo char(1) B/N/C/T, SubTipo char(1) L=lista, Descricao) · GE_PolSegItLst(Modulo, Item, Lista varchar(40) = rótulo, Nro, Str) · GE_PolSegCtrl(Seq, Modulo, Item, SeqPolSeg, Str = o valor, DtaAlteracao, UsuAlteracao) · GE_PolSegAces(SeqPolSeg, Modulo, DiaSem, HINI1/HFIM1/HINI2/HFIM2 decimal(4)) · GE_PolSegParams(SeqPolSeg, Param1/2/3, Str) · GE_CampoExig(Form, Campo, Tipo, SeqPolSeg, Exige) 0 linhas

**Relacoes:** Todas FK SeqPolSeg → GE_PolSeg. GE_Usuario.SeqPolSeg aponta a política do usuário. Módulos usados: CRM_M001 (desktop, 66 itens), CRM_M051 (mobile/Juno), CRM_M003 (formulários), DMN_M001 (documentos), GLB_M000 (global/senha/LGPD) e um GLB_M001 duplicado e conflitante.

### `GE_ModuloPerm`

**Funcao:** Acesso a MÓDULO por usuário e empresa. 25.202 linhas, 409 usuários, 42 combinações Sistema|Módulo, 18 empresas. Valor sempre 'S' (ausência = negado). NÃO herda de grupo.

**Campos-chave:** Sistema varchar(20) · Modulo varchar(20) · NroEmpresa numeric(6) · SeqUsuario numeric(18) · Permissao char(1) · CHKSUM varchar(50)

**Relacoes:** FK (Sistema,Modulo) → GE_Modulo; FK NroEmpresa → GE_Empresa; FK SeqUsuario → GE_Usuario. View de resolução GE$MODULOPERM (1:1, sem expansão de grupo). Média de ~62 linhas por usuário.

### `GE_UsuarioPerm`

**Funcao:** ACL POLIMÓRFICA por registro de catálogo, concedida a usuário OU grupo. 38.680 linhas; 34.124 para 273 grupos e 4.295 para 131 usuários. Sem colunas de auditoria e sem FK na chave do recurso.

**Campos-chave:** CodAplicacao varchar(20) = tipo do recurso (IV_RESULTADO, IV_ACAO, IV_ACAORES, IV_FORMULARIO, IV_SELECAO, IV_CAMPANHA, IV_VENDEDOR, IVS_DEPTO, IV_CODPROCESSO, GE_QVCONS, GE_PESSOAALERTA, IV_TXTPADRAO, IV_PROPRIEDADE, DMN_ACESSO, IV_FORQS_<n>) · ChaveAplicacao numeric(18) = id do recurso · SeqUsuario numeric(18) = sujeito · NroEmpresa numeric(6) (0 = todas, em 37.790 linhas) · Permissao char(1) = 0 Bloqueado / 1 Saber que existe / 2 Ler os dados / 3 Inserir / 4 Alterar / 5 Excluir / 6 TOTAL, ou 'S'/'N'

**Relacoes:** Nenhuma FK. View de resolução GE$USUARIOPERM expande grupos → 4.382.561 linhas. Órfãos: 261 sujeitos inexistentes; chaves inexistentes: 94 (IV_VENDEDOR), 60 (IV_RESULTADO), 12 (IV_CAMPANHA), 11 (IV_ACAO).

### `GE_POLSEGPERM`

**Funcao:** Mesma ideia da ACL de registro, mas concedida à POLÍTICA em vez do usuário — 805 linhas fazem o trabalho que na versão por usuário exige 38.680.

**Campos-chave:** CODAPLICACAO varchar(30) (DMN_DOCTP 469, ATFX_FORMACTTO 84, IV_GLOBALPAR 81, ATFX_TIPORELAC 65, ATFX_PAPELCTT 33, PRLST_* , ATFX_PESSTATUS, ATFX_GRUPO) · CHAVEAPLICACAO numeric(18) · NROEMPRESA numeric(6) (0 em todas) · SEQPOLSEG decimal(6) · PERMISSAO decimal(4) bitmask: 2, 4=Apenas leitura/Visualizar, 8=Inserir, 16=Alterar, 32=Inserir/Alterar/Excluir

**Relacoes:** FK SEQPOLSEG → GE_PolSeg. View de resolução GE$POLSEGPERM (805 → 143.645 linhas por usuário).

### `GE_Permissao / GE_Aplicacao / GE_Modulo / GE_Sistema`

**Funcao:** Camada de permissão por TELA com verbos CRUD — desenhada por completo e praticamente não usada (302 linhas, 55 usuários, 49 de 184 aplicações).

**Campos-chave:** GE_Permissao(SeqAplicacao, SeqUsuario, NroEmpresa, Executar, Incluir, Alterar, Excluir, RegistrarLog, Verimpressao, Exportar, Imprimir — todos char(1)) · GE_Aplicacao(SeqAplicacao PK, CodAplicacao varchar(20) ex. PES1PES00/IVS1PRC00/IVS7HST01, Sistema, Modulo, Descricao, TipoAcesso, RegistrarLog='N' em 184/184) · GE_Modulo(Sistema, Modulo, Descricao, SiglaModulo, TipoAcesso C/P/L, TIPO) 48 linhas · GE_Sistema(Sistema, Descricao, SiglaSistema) 11 linhas

**Relacoes:** GE_Aplicacao FK (Sistema,Modulo) → GE_Modulo; GE_Modulo FK Sistema → GE_Sistema; GE_Permissao FK SeqUsuario → GE_Usuario e NroEmpresa → GE_Empresa. Sistemas: INTERVISION (Vórtico CRM), GLOBAL, Vortico, QVW/QUERYVIEW, SERVPROCESSO, DMN, JUNO, GT, ZUMBI, INTEGRADOR — com duplicação de caixa 'Vortico'/'VORTICO'.

### `GE_CampoPerm`

**Funcao:** Permissão por CAMPO/widget da tela. 17 linhas, amarradas a nomes de controle Gupta (pbtCientes, dfnLatitude, dfnLongitude).

**Campos-chave:** CodAplicacao varchar(20) · NroEmpresa numeric(6) · Campo varchar(30) = nome do controle · SeqUsuario numeric(18) (14 das 17 linhas são grupos) · Permissao char(1) ('1' e '9') · UsuAlteracao · DtaAlteracao

**Relacoes:** FK SeqUsuario → GE_Usuario. Única tabela de permissão (junto com GE_PolSegCtrl) que tem quem/quando.

### `GE_Empresa`

**Funcao:** As 18 filiais (17 Tracbel Agro + Colorado Equipamentos). Chave de multiempresa presente em quase toda tabela de permissão.

**Campos-chave:** NroEmpresa numeric(6) PK · EmUso numeric(1) (0 nas empresas 6 e 891) · Fantasia · NomeReduzido · Sigla varchar(4) (CRPO, CARQ, CBAR, …) · Cidade/Estado · Matriz decimal(3) · EmpSeguranca decimal(3) (=1 em 17 de 18) · FusoHorario decimal(2) · Regional varchar(15)

**Relacoes:** Referenciada por GE_ModuloPerm, GE_Permissao, GE_UsuarioPerm, GE_POLSEGPERM, GE_CampoPerm, GE_UsrParam, GE_ParametroGlobal, IVS_Carteira, IVS_DeptoEmpr, IV_VENDEDOREMPR, IV_CodPrcEmpr, IV_Agenda, IV_Historico, IV_ProcDado.

### `IV_VENDEDOR / IV_VENDEDOREMPR (= views IV_EQUIPE / IV_EQUIPEEMPR)`

**Funcao:** Cadastro de vendedor/CEN e a HIERARQUIA comercial que define o escopo gerencial (carteira de pedidos, roteamento de aprovação). 349 e 929 linhas.

**Campos-chave:** IV_VENDEDOR: SEQVENDEDOR numeric(18) PK · CODVENDEDOR varchar(20) (é o que IV_Agenda.Vendedor guarda!) · VENDEDOR varchar(40) · SeqUsuario numeric(18) (o usuário do vendedor) · SeqUsuarioLider numeric(18) (o usuário do líder — o eixo da hierarquia) · EMUSO numeric(1) · NROEMPRPADRAO · EQUIPE varchar(40) · SeqUnidade. IV_VENDEDOREMPR: SEQVENDEDOR × NroEmpresa

**Relacoes:** IVS_Carteira.SeqVendedor → IV_VENDEDOR (o campo 'CEN' da tela IVS1DPT05). GE_UsuarioPerm com CodAplicacao='IV_VENDEDOR' controla quem aparece na lista (469 linhas, 94 chaves órfãs). Sem FK declarada para GE_Usuario nas colunas SeqUsuario/SeqUsuarioLider.

### `IVS_Carteira / IVS_CartCid / IVS_CartDepto / IVS_Pes / IVS_Depto / IVS_DeptoEmpr / IVS_Segm / IVS_Regional`

**Funcao:** Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente→carteira/departamento (139.036 linhas / 99.810 pessoas), departamentos (29, 13 em uso), departamento por empresa (13), segmentos (8), regionais (0).

**Campos-chave:** IVS_Carteira(SeqCarteira decimal(4), NroEmpresa, Carteira varchar(15) ex. MAQ_13SJRP_01, SeqUsrResp = responsável, SeqUrSuperv = supervisor (NULL em 655/655), SeqVendedor = CEN, SeqCanal, SeqRegional) · IVS_Pes(SeqPessoa+SeqDepto+SeqCarteira PK composta, Ciclo, Status, Situacao, Potencial, Score, Classe) · IVS_Depto(SeqDepto, Depto, Descricao, SeqUsrDirDepto = diretor, SeqSegm, CtrlPorEmpresa, MultCarteira, CodProcesso) · IVS_DeptoEmpr(SeqDepto, NroEmpresa, SeqUsrGerDepto = gerente, SeqUsrDirDepto, SEQUSUARIO)

**Relacoes:** IVS_CartCid/CartDepto FK → IVS_Carteira; IVS_Pes FK SeqDepto → IVS_Depto; IVS_Depto FK SeqSegm → IVS_Segm e CodProcesso → IV_CodProcesso; IVS_Depto/IVS_DeptoEmpr FK SeqUsrDirDepto/SeqUsrGerDepto → GE_Usuario. São as 4 rotas de escopo referenciadas pelos itens de política BB_A001/A003/A005/A007 do módulo CRM_M051.

### `IV_Operador / IV_SegPerfil / IV_Atendente`

**Funcao:** Permissões operacionais por usuário (939 linhas × 51 colunas de flags, 370 combinações distintas) e o modelo de PERFIL que resolveria isso — nunca ativado (0 linhas em ambas).

**Campos-chave:** IV_Operador: SeqUsuario PK · Funcao char(1) · Supervisor varchar(20) · Gerente varchar(20) · EVendedor · IncHistorico/AltHistorico/ExcHistorico · IncHistRetr + QtdDiasRetr · IncAgenda/AltAgenda/ExcAgenda/VerAgenda/ConcAgenda/ReativarAgenda · AltOperAgenda/AltOperAgdReag/ConcAgeVendor · AnalisarHistorico/AbreAnalise · Altera{grupo,CodImport,Regiao,Vendedor,ClienteAtivo,Status} · HoraInicial/HoraFinal + DispDomingo..DispSabado · Departamento · eMail · EMailAssinatura · EnderecoSMTP. IV_SegPerfil: SeqPerfil + as mesmas 24 flags (0 linhas). IV_Atendente: SeqUsuario→SeqPerfil (0 linhas)

**Relacoes:** IV_Operador.SeqUsuario ↔ GE_Usuario (7 linhas órfãs). Supervisor/Gerente são varchar(20), não FK.

### `IVC_EQUIPE / IVC_EQUIPEUSR / IVC_ATENDENTE / IVC_RAMAL / IVC_ATENDENTELOG / IVC_CHAMADALOG`

**Funcao:** Modelo de EQUIPE e de call center — schema completo, ZERO linhas em todas. Nunca implantado na Tracbel.

**Campos-chave:** IVC_EQUIPE(SEQEQUIPE numeric(6), EQUIPE varchar(20), DESCRICAO varchar(150)) · IVC_EQUIPEUSR(SEQUSUARIO, SEQEQUIPE, PAPEL varchar(3) — o único lugar do schema com noção de PAPEL dentro de equipe) · IVC_ATENDENTE(SEQUSUARIO, STATUS, STATUSCTI, SEQAGENDA, NRORAMAL, DTALOGIN, IDCHAMADACTI, SENTIDO, PROCESSO, SILOORIGEM)

**Relacoes:** FK IVC_EQUIPEUSR.SEQEQUIPE → IVC_EQUIPE e SEQUSUARIO → GE_Usuario; FK IVC_ATENDENTE.SEQUSUARIO → GE_Usuario. O que existe de fato como 'equipe' é a view IV_EQUIPE sobre IV_VENDEDOR.

### `GE_UsrParam / GE_ParametroGlobal / GE_ParamLista`

**Funcao:** Parâmetros por usuário (16.827 linhas), globais por sistema/módulo/empresa (236) e listas (333). Contêm preferência de UI, mas TAMBÉM autorização (escopo de empresa em relatório) e segredo (token de API em texto claro).

**Campos-chave:** GE_UsrParam(SeqUsuario, NroEmpresa, Parametro varchar(30), Valor varchar(1000), Criptografado char(1), Dtaalteracao, Usualteracao) — destaques: PERMRELGERNROEMPRESAIN (284 usuários, CSV de empresas), OPERADOR-EMPULTLG (299, criptografado), SEGURANCA-EMPULTLG (11), AGEVER* / HSTCOLUNA* (layout). GE_ParametroGlobal(Sistema, Modulo, NroEmpresa, Parametro, Valor, Criptografado) — CrmImpController_token (texto claro, Criptografado='N'), SMTP_PorUsuario (criptografado), PolSegVersao='1.0.30'. GE_ParamLista(SeqParamLista, Parametro, NroEmpresa, ListaStr, Str1..Str5, Nro1..Nro6, Ind1..Ind4, STRL1 varchar(4000))

**Relacoes:** GE_UsrParam FK SeqUsuario → GE_Usuario. GE_ParamLista guarda mapeamentos de integração (RDSTATION_MAP_FIELD 148, JUPTER_USR 65).

### `GE_LgTb / GE_LOG_PROCESSO / GE_LOG_HISTORICO / GE_LOG_PESSOA / GE_LOG_TRANS / GE_LOG_EXT / GE_LOG_CONFIG / GE_LOG_CONTATO / IV_AgendaLog`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. GE_LgTb congelada em 05/06/2023, substituída pelas GE_LOG_* particionadas por domínio a partir de 06/06/2023.

**Campos-chave:** Estrutura comum (10 colunas): SEQLOGTB numeric(18) · Tb varchar(25) = tabela auditada · Kn1/Kn2 numeric(18) = chaves numéricas · Ks varchar(40) = chave string · DtaLog datetime · Usr varchar(20) = quem · CodApl varchar(20/30) = de qual tela · Obs varchar(250 em GE_LgTb / 1000 nas GE_LOG_*) · Nivel char(1). IV_AgendaLog tem as mesmas 9 colunas (sem Nivel).

**Relacoes:** Sem FK. Volumes: GE_LOG_PROCESSO 12,7M/3,3 GB · IV_AgendaLog 11,1M/2,77 GB · GE_LgTb 11,9M/2,5 GB · GE_LOG_HISTORICO 1,05M · GE_LOG_PESSOA 961k · GE_LOG_TRANS 803k · GE_LOG_EXT 623k · GE_LOG_CONFIG 110k · GE_LOG_CONTATO 20k. GE_LogAtividade e GE_LOG_CARTCRED = 0.

### `GE_Log2`

**Funcao:** Log de EVENTOS de aplicação (4,58M linhas / 766 MB) — é onde ficam as mudanças de permissão, as exclusões e o (quase inexistente) login.

**Campos-chave:** SEQLOG numeric(18) · NroEmpresa · Sistema · Modulo · Aplicacao varchar(30) · CodUsuario varchar(20) = ator · Data datetime · Estacao varchar(20) (vazio nas amostras) · Nivel numeric(1) · Resumo varchar(30) = tipo do evento · Detalhe varchar(1000) = texto legível · LinkTipo varchar(20) + LinkNro numeric(18) = recurso afetado · LinkSerie varchar(250)

**Relacoes:** Eventos de segurança: ALTERACÃO PERMISSÕES 34.834 (2011→2026) · ALT. PERMISSÃO POLÍTICAS 1.902 · EXCLUSÃO EM GE_USUARIOPERM 841 · EXCLUSÃO EM GE_MODULOPERM 835 · PARÂMETRO GLOBAL 10.415 · LOGIN apenas 114. Ruído dominante: SINCRONISMO JUPITER 3,74M (81% da tabela).

### `GE_CtrlM / GE_CtrlE / GE_CtrlME / GE_CTRLUSR / GE_CTRL2 / GE_CTRLMU`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta.

**Campos-chave:** GE_CtrlM(K01 varchar(30) PK, Dexp = expiração, Nusr = nº usuários, Ctemp, Nivel, Datu, Dglb, CHKSUM varchar(50), KDESC, DEXPTMP, DEXPQTD) 31 linhas · GE_CtrlE(K01, Sfan, Srso, Nrcg, Dgcg, CHKSUM) 16 · GE_CtrlME 36 · GE_CTRLUSR(K01, SPW, SNVL, DGER, CHKSUM) 33 · GE_CTRL2 276

**Relacoes:** Sem FK. Ligado ao item de política GLB_EXP_10 'Exibir aviso que ControlSet irá expirar'. Última atualização (Datu) em 29/04/2026.

## Achados

### 1. Não existe permissão por REGISTRO transacional — nem no banco, nem declarativa

sys.security_policies = 0 (nenhum Row-Level Security nativo do SQL Server) e sys.triggers retorna UMA única trigger no banco inteiro (VTC_T_AUDITORIA sobre IV_ClientePropr). Não há coluna de dono relacional nas tabelas transacionais principais: IV_Processo (23 colunas) NÃO tem SeqUsuario, NroEmpresa nem SeqVendedor — o responsável é IV_Processo.UsuResponsavel varchar (string de login, sem FK). O escopo do processo vem indiretamente de IV_ProcDado (SeqDepto, Vendedor varchar, NroEmpresa) e de IVS_Pes. IV_Agenda tem apenas SeqUsuario, NroEmpresa e Vendedor varchar. IV_Historico tem SeqUsuario, CodUsuario, NroEmpresa e Vendedor varchar. Conclusão: todo filtro de visibilidade é montado em código no cliente Gupta e no servidor BPM.

**Evidencia:** Query: SELECT COUNT(*) FROM sys.security_policies → 0; SELECT OBJECT_NAME(parent_id), name FROM sys.triggers → 1 linha (IV_ClientePropr / VTC_T_AUDITORIA); sys.columns de IV_Processo, IV_Agenda, IV_Historico, IV_ProcDado

**Licao para a Tracbel:** EVITAR. No CRM novo, escopo de registro tem que ser declarativo e aplicado no servidor (RLS do Postgres/SQL Server ou um único filtro central no ORM/repositório), nunca replicado em cada tela. E toda tabela transacional precisa de owner_user_id + team_id/branch_id como FK real, não string de login.

### 2. Usuário e grupo são a mesma tabela; grupos são planos e não aninhados

GE_Usuario.TipoUsuario: 'U' = 1.068 usuários, 'G' = 318 grupos (mesma PK SeqUsuario, mesmo espaço de código). GE_Membro(Grupo numeric, Usuario numeric, INDATIVO) tem 1.584 vínculos, e 100% dos membros são TipoUsuario='U' — ou seja, NÃO há grupo dentro de grupo. 462 usuários pertencem a pelo menos um grupo; a distribuição vai de 1 grupo (114 usuários) até 21 grupos (1 usuário). O maior grupo é SeqUsuario=1 'todos' com 462 membros — um pseudo-usuário que também é usado como sujeito default em GE_Permissao (CodUsuario='todos'). INDATIVO é 0/NULL em 100% das linhas (flag existe e nunca foi usado).

**Evidencia:** SELECT TipoUsuario, COUNT(*) FROM GE_Usuario GROUP BY TipoUsuario; JOIN GE_Membro×GE_Usuario por Usuario agrupando TipoUsuario → só 'U' (1.576); SUM(CASE WHEN INDATIVO=1) = 0 em todos os grupos

**Licao para a Tracbel:** COPIAR o princípio (sujeito polimórfico: uma permissão pode ser concedida a usuário OU a grupo, resolvida na mesma consulta), mas ADAPTAR: usar tabelas separadas (users, groups, group_members) com FK explícita, permitir grupo dentro de grupo (hierarquia de grupos) e ter um flag de ativo que realmente seja escrito.

### 3. Não existe papel/perfil: 939 usuários, 370 combinações distintas de permissão em IV_Operador

IV_Operador (51 colunas, 939 linhas, 1 por usuário) carrega flags char(1) individuais: EVendedor, IncHistorico, IncHistRetr, QtdDiasRetr, IncHistAgConcl, AltHistorico, ExcHistorico, IncAgenda, AltAgenda, ExcAgenda, VerAgenda, ConcAgenda, AltOperAgenda, AltOperAgdReag, ConcAgeVendor, ReativarAgenda, AnalisarHistorico, AbreAnalise, Alteragrupo, AlteraCodImport, AlteraRegiao, AlteraVendedor, AlteraClienteAtivo, AlteraStatus — além de disponibilidade (DispDomingo..DispSabado, HoraInicial/HoraFinal), e-mail e SMTP do operador. Contando só 17 dessas flags já dão 370 combinações distintas para 939 usuários. A tabela que existiria para resolver isso — IV_SegPerfil (SeqPerfil + exatamente as mesmas 24 flags) — tem 0 linhas, e IV_Atendente (SeqUsuario→SeqPerfil) também tem 0 linhas. Ou seja: o modelo de perfil foi desenhado, implementado no schema e nunca ativado.

**Evidencia:** SELECT COUNT(*), COUNT(DISTINCT SeqUsuario), COUNT(DISTINCT CONCAT(17 flags)) FROM IV_Operador → 939 / 939 / 370; schema/tabelas.csv: IV_SegPerfil 27 colunas 0 linhas, IV_Atendente 2 colunas 0 linhas

**Licao para a Tracbel:** EVITAR flags por usuário. No CRM novo: permissão SEMPRE via papel (role), usuário recebe papéis, e permissão direta ao usuário é exceção auditada e com data de expiração. Se um dia existirem 370 perfis distintos para 939 pessoas, o modelo falhou.

### 4. Todo o bloco IVC_ (equipes e atendimento) está vazio — e IV_EQUIPE é apenas uma VIEW sobre IV_VENDEDOR

IVC_EQUIPE (SEQEQUIPE, EQUIPE, DESCRICAO) = 0 linhas; IVC_EQUIPEUSR (SEQUSUARIO, SEQEQUIPE, PAPEL varchar(3)) = 0 linhas; IVC_ATENDENTE (18 colunas de status de call center) = 0 linhas; IVC_ATENDENTELOG, IVC_CHAMADALOG, IVC_RAMAL = 0 linhas. O conceito de 'equipe' que realmente existe é a view IV_EQUIPE, cujas colunas (SEQEQUIPE, CODEQUIPE, EQUIPE, CODEQUIPEFORA, USUALTEROU, CODEQUIPEFORAX, SEQUSUARIO, SEQUSUARIOLIDER) mapeiam 1:1 as colunas de IV_VENDEDOR (SEQVENDEDOR, CODVENDEDOR, VENDEDOR, CODVENDEDORFORA, UsuAlterou, CODVENDEDORFORAX, SeqUsuario, SeqUsuarioLider) e devolve exatamente as mesmas 349 linhas. IV_EQUIPEEMPR = IV_VENDEDOREMPR (929 linhas em ambas).

**Evidencia:** schema/tabelas.csv (IVC_* com linhas=0); SELECT COUNT(*) FROM IV_VENDEDOR=349 e FROM IV_EQUIPE=349; FROM IV_VENDEDOREMPR=929 e FROM IV_EQUIPEEMPR=929; SELECT TOP 3 de cada view

**Licao para a Tracbel:** EVITAR o apelido. Se equipe e vendedor são a mesma coisa, chame de uma coisa só. No CRM novo, modelar Team como entidade de primeira classe (team, team_member com papel: membro/líder/gestor, vigência) e nunca criar sinônimos de view para renomear conceito — isso mata a rastreabilidade de quem depende de quê.

### 5. Senha armazenada sem salt: 80 usuários compartilham exatamente o mesmo valor gravado

GE_Usuario.Senha varchar(30): dos 1.068 usuários 'U', 663 têm valor; desses, apenas 467 são DISTINTOS. O maior grupo de colisão tem 80 usuários com o mesmo valor gravado, depois 13, 12, 7 e 7. Colisão nesse volume só é possível se a transformação for determinística e sem salt por usuário (mesma senha em claro → mesmo valor armazenado). O comprimento é sempre exatamente 30 (1 usuário com 0). O conteúdo NÃO é hexadecimal, NÃO é base64 e contém bytes fora de ASCII imprimível (662 de 662 falham nos três testes) — é payload binário guardado em varchar, portanto dependente de collation/codepage. Há ainda Senha3 varchar(30) (243 usuários, 141 distintos — também colide) e ChkSum varchar(50) (994 usuários). Histórico de senhas em GE_UsuarioSenhaMem (SeqUsuario, DtaTroca, Senha): 2.490 linhas para 738 usuários, de 25/09/2013 a 28/08/2026 — ou seja, senhas antigas também ficam guardadas no mesmo formato reversível/colidível.

**Evidencia:** SELECT COUNT(Senha)=663, COUNT(DISTINCT Senha)=467, MIN/MAX(LEN)=0/30 FROM GE_Usuario WHERE TipoUsuario='U'; GROUP BY Senha ORDER BY COUNT(*) DESC → 80, 13, 12, 7, 7; testes LIKE '%[^0-9A-Fa-f]%', '%[^0-9A-Za-z+/=]%', '%[^ -~]%' → 662/662 em todos

**Licao para a Tracbel:** EVITAR sem exceção. CRM novo: Argon2id (ou bcrypt cost≥12) com salt por usuário, e nunca guardar histórico de senha em formato recuperável (guardar só hashes com salt distinto). Além disso, o bloco de 80 contas com o mesmo hash é um risco vivo hoje — se uma senha vazar, 80 contas caem juntas; vale forçar troca dessas contas antes da migração.

### 6. Política de senha configurada em apenas 1 das 11 políticas, com valor 'Fraco' e sem expiração

O catálogo GE_PolSegItem define no módulo GLB_M000 os itens: GLB_SH0_10 (DD.A01 'Exigir troca de senha a cada X dias'), GLB_SH0_20 (DD.A02 'Nível de segurança da senha'), GLB_SH0_25 (DD.A03 'Quantidade mínima de caracteres'), GLB_SH0_27 (DD.A04 'Quantidade de senhas a serem memorizadas'). Em GE_PolSegCtrl (254 linhas, os valores efetivos) esses itens só aparecem para SeqPolSeg=6 (CEN): GLB_SH0_20='1' (que em GE_PolSegItLst = 'Fraco'; as opções seriam 1 Fraco, 2 Médio, 3 Difícil, 4 Forte), GLB_SH0_25='12', GLB_SH0_27='0' (não memoriza). GLB_SH0_10 (expiração) NÃO está configurado em política nenhuma → senha nunca expira. Coerente com os dados: 332 usuários com Ulttrocasenha NULL, DTALIMITEUSO NULL em 100% dos 1.068 usuários e Loginexpirando=0 em 100%. Pior: existe um segundo módulo GLB_M001 com os MESMOS códigos de item e valores CONFLITANTES para a política 6 (GLB_SH0_20='F' e GLB_SH0_25='4' contra '1' e '12' em GLB_M000), sem que se saiba qual prevalece.

**Evidencia:** Export de GE_PolSegCtrl×GE_PolSeg×GE_PolSegItem (255 linhas) — só SeqPolSeg=6 tem GLB_SH0_2x; GE_PolSegItLst para GLB_SH0_20 (1 Fraco/2 Médio/3 Difícil/4 Forte) e GLB_SH0_25 (6/8/10/12); SELECT sobre GE_Usuario: sem_data_limite=1068, login_expirando=0, nunca_trocou=332

**Licao para a Tracbel:** COPIAR a ideia de política de segurança nomeada e reutilizável, mas ADAPTAR: política de senha é GLOBAL do tenant (não opcional por perfil), com defaults seguros aplicados quando não configurada — no Vórtice, política não configurada = sem restrição. E jamais permitir dois namespaces (GLB_M000/GLB_M001) guardando o mesmo item com valores diferentes.

### 7. Oito camadas de permissão distintas, cada uma com sua própria tabela e sua própria gramática

(1) MÓDULO: GE_ModuloPerm(Sistema, Modulo, NroEmpresa, SeqUsuario, Permissao char(1)) — 25.202 linhas, 42 combinações Sistema|Módulo, valor sempre 'S' (ausência = negado). (2) APLICAÇÃO/TELA: GE_Permissao(SeqAplicacao, SeqUsuario, NroEmpresa, Executar, Incluir, Alterar, Excluir, RegistrarLog, Verimpressao, Exportar, Imprimir) sobre GE_Aplicacao (184 telas). (3) REGISTRO DE CATÁLOGO POR USUÁRIO/GRUPO: GE_UsuarioPerm(CodAplicacao, ChaveAplicacao, SeqUsuario, NroEmpresa, Permissao char(1)) — 38.680 linhas, ACL polimórfica. (4) REGISTRO DE CATÁLOGO POR POLÍTICA: GE_POLSEGPERM(CODAPLICACAO, CHAVEAPLICACAO, NROEMPRESA, SEQPOLSEG, PERMISSAO decimal(4)) — 805 linhas, bitmask 2/4/8/16/32. (5) CAMPO/WIDGET: GE_CampoPerm(CodAplicacao, NroEmpresa, Campo, SeqUsuario, Permissao). (6) COMPORTAMENTO: GE_PolSegCtrl sobre o catálogo GE_PolSegItem (108 itens tipados B=booleano, N=numérico, C=escolha, T=título; SubTipo L = valores vindos de GE_PolSegItLst). (7) JANELA DE HORÁRIO: GE_PolSegAces(SeqPolSeg, Modulo, DiaSem, HINI1, HFIM1, HINI2, HFIM2) — 3 linhas. (8) OBRIGATORIEDADE DE CAMPO POR POLÍTICA: GE_CampoExig(Form, Campo, Tipo, SeqPolSeg, Exige) — 0 linhas. Mais GE_PolSegParams (SeqPolSeg + Param1/2/3 + Str) com 1 linha.

**Evidencia:** schema/colunas.csv para cada tabela; contagens ao vivo: GE_ModuloPerm 25.202, GE_UsuarioPerm 38.680, GE_POLSEGPERM 805, GE_CampoPerm 17, GE_PolSegCtrl 254, GE_PolSegItem 108, GE_PolSegItLst 66, GE_PolSegAces 3, GE_CampoExig 0

**Licao para a Tracbel:** EVITAR a proliferação. No CRM novo, UM modelo único: (subject, action, resource_type, resource_id|null, effect, scope) — permissão de módulo, de tela, de registro e de campo são o mesmo par (ação, recurso) em granularidades diferentes, não oito tabelas com oito gramáticas. COPIAR, isso sim: a ideia de catálogo tipado de item de política (GE_PolSegItem com Tipo B/N/C + lista de valores) — é um bom modelo para 'settings de política' versionáveis e renderizáveis por metadados.

### 8. A camada de CRUD por tela (GE_Permissao) é letra morta: 302 linhas, 55 usuários, quase tudo no pseudo-usuário 'todos'

GE_Permissao existe com o modelo mais completo do sistema (Executar/Incluir/Alterar/Excluir/RegistrarLog/Verimpressao/Exportar/Imprimir por aplicação × usuário × empresa), mas tem só 302 linhas cobrindo 55 usuários, 49 aplicações e 17 empresas — para 184 aplicações cadastradas em GE_Aplicacao. A amostragem mostra que a maioria das linhas é do SeqUsuario=1, CodUsuario='todos' (o pseudo-usuário/grupo default), com exemplos como PES1PES00 'Cadastro de pessoas' S/S, IVS2HST01 'Análise Históricos' Executar=S/Incluir=N. Ou seja: o controle real de tela é feito por GE_ModuloPerm (acesso ao módulo inteiro), não por aplicação.

**Evidencia:** SELECT COUNT(*)=302, COUNT(DISTINCT SeqUsuario)=55, COUNT(DISTINCT SeqAplicacao)=49 FROM GE_Permissao; JOIN com GE_Aplicacao e GE_Usuario mostrando CodUsuario='todos'; GE_Aplicacao tem 184 linhas

**Licao para a Tracbel:** Lição de produto, não de schema: uma camada de permissão que ninguém preenche é dívida. No CRM novo, se a granularidade 'tela' não for realmente usada, não crie a tabela — e se criar, gere as linhas automaticamente a partir do papel, nunca esperando que um admin preencha 184×N linhas à mão.

### 9. GE_UsuarioPerm é uma ACL polimórfica sem integridade referencial — 261 sujeitos órfãos e 177 chaves órfãs

GE_UsuarioPerm identifica o recurso por par (CodAplicacao varchar(20), ChaveAplicacao numeric) — polimórfico, portanto sem FK possível na chave. Tipos de recurso realmente usados: IV_RESULTADO (25.056 linhas, 4.180 chaves, 361 sujeitos), IV_ACAORES (5.993), IV_ACAO (1.461), GE_PESSOAALERTA (923), IV_FORMULARIO (805), IV_SELECAO (569), IV_VENDEDOR (469), IV_CAMPANHA (304), IV_TXTPADRAO (213), GE_QVCONS (170), IVS_DEPTO (129), IV_CODPROCESSO (103), IV_PROPRIEDADE, IV_MOTIVO, IV_BASECONHEC, DMN_ACESSO, e ~170 tipos IV_FORQS_<n> (permissão por PERGUNTA de formulário). Consequência da falta de FK: 261 linhas apontam para SeqUsuario que não existe mais em GE_Usuario; e as chaves de recurso apodrecem — 94 chaves de IV_VENDEDOR, 60 de IV_RESULTADO, 12 de IV_CAMPANHA e 11 de IV_ACAO não têm registro correspondente. A tabela também não tem NENHUMA coluna de auditoria (só 5 colunas: CodAplicacao, ChaveAplicacao, SeqUsuario, NroEmpresa, Permissao) — não se sabe quem concedeu nem quando, a não ser pelo log de aplicação.

**Evidencia:** GROUP BY CodAplicacao, Permissao sobre GE_UsuarioPerm; NOT EXISTS contra GE_Usuario → 261; NOT EXISTS contra IV_VENDEDOR/IV_Resultado/IV_Campanha/IV_Acao → 94/60/12/11; schema/colunas.csv GE_UsuarioPerm = 5 colunas

**Licao para a Tracbel:** COPIAR o conceito (ACL por registro de catálogo é exatamente o que o BPM precisa), ADAPTAR a implementação: usar tabela por tipo de recurso OU tabela polimórfica com resource_type enumerado + job de integridade + ON DELETE CASCADE lógico. E toda linha de ACL carrega granted_by, granted_at, expires_at — permissão sem quem/quando/até-quando é impossível de auditar.

### 10. Escala de permissão decodificada: 0=Bloqueado, 2=Ler os dados, 3=Inserir, 4=Alterar, 5=Excluir, 6=TOTAL; e 'S'/'N' para recursos booleanos

GE_UsuarioPerm.Permissao é char(1) mas mistura duas gramáticas. Para recursos com nível (IV_RESULTADO, IV_ACAORES, IV_FORMULARIO, IV_SELECAO, IV_PROPRIEDADE, IV_ATRIBESPECIAL) usa a escala ordinal 0..6; para recursos booleanos (IV_ACAO, IV_CAMPANHA, IV_VENDEDOR, IVS_DEPTO, GE_QVCONS, IV_MOTIVO, IV_CODPROCESSO, IV_TXTPADRAO, IV_FORQS_*) usa 'S'/'N'. Os rótulos vieram do próprio log da aplicação (GE_Log2, Resumo='ALTERACÃO PERMISSÕES', campo Detalhe): 'Bloqueado', 'Saber que existe', 'Ler os dados', 'Inserir', 'Alterar', 'Excluir', 'TOTAL' para os de nível; 'Permitido'/'Bloqueado' para os booleanos. Validado ponta a ponta num caso real: em 26/08/2026 11:10:30 foi inserida permissão 'Ler os dados' para EDUARDA.MARIOTO no IV_RESULTADO 3466 e às 11:10:37 alterada para 'Bloqueado'; o valor atual em GE_UsuarioPerm para essa linha é Permissao='0'. Já GE_POLSEGPERM usa outra escala, em bitmask decimal: 2, 4 (Apenas leitura/Visualizar), 8 (Inserir), 16 (Alterar/Inserir-Alterar), 32 (Total/Inserir-Alterar-Excluir) — os rótulos vêm de GE_PolSegItLst.

**Evidencia:** Parse de GE_Log2.Detalhe entre 'perm: ' e ', em:' agrupado por LinkTipo; SELECT em GE_UsuarioPerm para EDUARDA.MARIOTO × IV_RESULTADO 3466 → Permissao='0'; GE_PolSegItLst para ATD_SGM_20 (4/8/16/32) e ATD_PRC_40_LNK (4 Visualizar/8 Inserir/16 Alterar/32 Excluir)

**Licao para a Tracbel:** EVITAR char(1) polissêmico e escala ordinal implícita. No CRM novo: enum tipado por tipo de recurso, ou melhor, conjunto explícito de ações (read, create, update, delete, export) — escala ordinal impede permissões como 'pode alterar mas não pode inserir', que aqui é impossível de expressar.

### 11. As views de resolução explodem 38 mil linhas de ACL em 4,4 milhões de linhas efetivas

Existem três views que resolvem a permissão efetiva expandindo a participação em grupos: GE$USUARIOPERM (colunas CODAPLICACAO, CHAVEAPLICACAO, NROEMPRESA, PERMISSAO, SEQUSUARIO), GE$MODULOPERM (SISTEMA, MODULO, NROEMPRESA, SEQUSUARIO) e GE$POLSEGPERM (CODAPLICACAO, CHAVEAPLICACAO, NROEMPRESA, SEQPOLSEG, SEQUSUARIO). Contagens medidas: GE_UsuarioPerm 38.680 linhas base → GE$USUARIOPERM 4.382.561 linhas resolvidas (fan-out de 113×); GE_POLSEGPERM 805 → GE$POLSEGPERM 143.645 (178×); GE_ModuloPerm 25.202 → GE$MODULOPERM 25.202 (1:1). O fan-out vem do fato de a ACL ser concedida majoritariamente a GRUPOS: 34.124 linhas de GE_UsuarioPerm têm sujeito TipoUsuario='G' (273 grupos) contra 4.295 linhas para 131 usuários diretos. Já GE_ModuloPerm tem 100% dos sujeitos como TipoUsuario='U' (409 usuários) — ou seja, permissão de módulo não herda de grupo.

**Evidencia:** SELECT COUNT(*) de cada tabela base e de cada view GE$…; JOIN GE_UsuarioPerm×GE_Usuario e GE_ModuloPerm×GE_Usuario agrupando por TipoUsuario

**Licao para a Tracbel:** EVITAR resolver permissão por view materializada implícita a cada consulta. COPIAR a ideia de ter uma 'effective permissions' consultável, mas ADAPTAR: cache de permissão efetiva por usuário calculado no login (ou tabela materializada com invalidação por evento), não view que reexpande 4,4 milhões de linhas a cada checagem. E a inconsistência (ACL herda de grupo, módulo não) é bug de modelo — no CRM novo, TUDO herda por papel/grupo, sem exceção.

### 12. Permissão por AÇÃO e por RESULTADO: o coração da segurança do BPM — e cobre 99,5% do catálogo

O controle mais fino e mais usado do Vórtice é 'que ações eu posso executar e que resultados eu posso registrar'. IV_Resultado tem 4.201 registros e 4.180 deles (99,5%) têm pelo menos uma linha de ACL em GE_UsuarioPerm, distribuída entre 361 sujeitos (25.056 linhas). IV_Acao tem 979 registros, 963 com ACL para 163 sujeitos. IV_Formulario 176 registros, 175 com ACL. IV_Selecao 543, 437 com ACL. IV_CodProcesso 62, 57 com ACL. IVS_Depto 29, 28 com ACL. Ou seja: cada resultado novo criado num fluxo BPM precisa ser liberado individualmente para grupos/usuários, senão ninguém consegue registrá-lo. Isso conecta diretamente com a regra já documentada no repositório: 'Ausência de resultado ali é permissão, não ausência de código'.

**Evidencia:** GROUP BY CodAplicacao sobre GE_UsuarioPerm contando DISTINCT ChaveAplicacao e DISTINCT SeqUsuario; contagens dos catálogos em schema/tabelas.csv (IV_Resultado 4.201, IV_Acao 979, IV_Formulario 176, IV_Selecao 543, IV_CodProcesso 62); docs/REGRAS-DE-NEGOCIO.md linha 288

**Licao para a Tracbel:** COPIAR o conceito — permissão por transição de fluxo (quem pode executar a ação X e registrar o resultado Y) é exatamente certo para um CRM com BPM. ADAPTAR a granularidade: conceder por PAPEL × TIPO DE PROCESSO × FASE, com herança e default (ex.: 'papel Vendedor tem todos os resultados da fase Negociação'), em vez de 4.180 concessões individuais. Deixar a concessão individual só como exceção.

### 13. Permissão por CAMPO existe, mas amarrada ao nome físico do widget Gupta — e tem 17 linhas

GE_CampoPerm(CodAplicacao, NroEmpresa, Campo, SeqUsuario, Permissao, UsuAlteracao, DtaAlteracao) tem 17 linhas no total. 15 delas são sobre o campo 'pbtCientes' na aplicação IVS7HST01, empresa 1, todas com Permissao='1' e todas criadas no mesmo instante (12/01/2012 17:42:04) por VTCCONS — concedidas a 14 grupos (alm_peças_rpo, vend_peças_orl, ger_ven_maq, admin crm, qualidade, etc.) e 1 usuário. As outras 2 são sobre 'dfnLatitude' e 'dfnLongitude' na aplicação PES1PES00, empresa 0, Permissao='9', concedidas ao grupo consultores_digitais por NETO.PRADO em 02/07/2024. Os nomes 'pbtCientes' (pushbutton) e 'dfnLatitude'/'dfnLongitude' (data field numeric) são nomes de controle do form Gupta/Centura — a permissão está acoplada à implementação da tela, não ao dado. Complementarmente, GE_CampoExig (Form, Campo, Tipo, SeqPolSeg, Exige) permitiria tornar campo obrigatório por política, mas tem 0 linhas. Existe ainda permissão por PERGUNTA de formulário via GE_UsuarioPerm com CodAplicacao='IV_FORQS_<n>' (~170 tipos distintos, S/N).

**Evidencia:** SELECT completo de GE_CampoPerm (17 linhas) com JOIN em GE_Usuario mostrando TipoUsuario='G' em 14 delas; schema/tabelas.csv GE_CampoExig 0 linhas; GROUP BY CodAplicacao LIKE 'IV_FORQS_%' em GE_UsuarioPerm

**Licao para a Tracbel:** EVITAR amarrar permissão a nome de controle de UI (pbtCientes) — a tela muda e a permissão vira lixo silencioso. COPIAR a intenção: permissão por campo é necessária (LGPD: CPF, faturamento, telefone). ADAPTAR: declarar no MODELO (entity.field) e derivar a UI, com três estados por campo (oculto / somente leitura / editável). E a permissão por pergunta de formulário (IV_FORQS_*) é boa ideia mal implementada — no novo, campo de formulário é um recurso como qualquer outro.

### 14. O escopo de visibilidade é resolvido por 4 rotas paralelas — e uma delas está morta

As rotas de escopo que o Vórtice reconhece estão explicitadas como itens de política no módulo CRM_M051: PES_RESP/BB_A001 'Envia as pessoas das carteiras da qual sou o responsável' (→ IVS_Carteira.SeqUsrResp), PES_SUPER/BB_A003 'das quais sou supervisor' (→ IVS_Carteira.SeqUrSuperv), PES_GERENTE/BB_A005 'das quais sou gerente' (→ IVS_DeptoEmpr.SeqUsrGerDepto), PES_RESPDEPTO/BB_A007 'do departamento da qual sou responsável' (→ IVS_Depto.SeqUsrDirDepto); e os equivalentes para agenda AGD_RESP/AGD_SUPER/AGD_GERENTE/AGD_PESSOAS (DD_A001/A003/A005/B005). A quinta rota, para pedidos/aprovações, é a hierarquia de vendas IV_VENDEDOR.SeqUsuarioLider. Medições: IVS_Carteira tem 655 carteiras em 18 empresas, 143 usuários responsáveis distintos, 100 CENs (SeqVendedor) — e SeqUrSuperv é NULL/0 em 655 de 655 carteiras, ou seja, a rota 'supervisor' nunca foi populada. Além disso o responsável é altamente concentrado: SeqUsrResp=862 responde por 371 das 655 carteiras (57%). IVS_Pes (139.036 linhas, 99.810 pessoas, 168 carteiras, 13 departamentos em uso de 29 cadastrados) é o que liga pessoa→carteira/departamento, com um cliente aparecendo em várias carteiras (uma por linha de negócio).

**Evidencia:** GE_PolSegCtrl+GE_PolSegItem módulo CRM_M051 (itens BB_A001..BB_A007, DD_A001..DD_B005); SELECT COUNT(DISTINCT SeqUrSuperv)=0 e SUM(CASE WHEN SeqUrSuperv IS NULL OR =0)=655 FROM IVS_Carteira; TOP SeqUsrResp → 862 com 371; SELECT COUNT(*) 139.036 / COUNT(DISTINCT SeqPessoa) 99.810 FROM IVS_Pes; lista completa de IVS_Depto

**Licao para a Tracbel:** COPIAR o vocabulário (responsável / supervisor / gerente / diretor de departamento é exatamente a hierarquia comercial da Tracbel) mas ADAPTAR radicalmente: UMA tabela de atribuição (assignment: subject, scope_type, scope_id, role, vigência) em vez de 4 colunas de ponteiro espalhadas em 3 tabelas. E validar no cadastro: se a rota 'supervisor' existe na tela mas nunca é preenchida, ela precisa sumir ou virar obrigatória.

### 15. Essas rotas de escopo só valem para o mobile — no desktop não há equivalente configurável

Todos os itens PES_* e AGD_* estão no módulo CRM_M051, cujos títulos são explicitamente sobre sincronismo: 'Relacionados ao envio de pessoas', 'Relacionados ao envio das agendas', 'Relacionados ao envio de históricos' (HST_PROD/HH_A001 'Produtivos ocorridos a até quantos dias', HST_IMPROD, HST_MINIMO), 'Relacionados a cadastros fora da carteira' (JUNO_CGO1/JUNO_CGO2). São as regras de CARGA do Vórtico Mobile/Juno, não regras de visibilidade da aplicação desktop. No módulo CRM_M001 (o desktop, com 66 itens) não existe nenhum item equivalente a 'ver só a minha carteira' — o que existe são controles de comportamento (ATD_CT0_10 'Alterar dados do contato' com opções C=De sua carteira / T=Todos / N=Não permitido; ATD_CT0_20 e ATD_CT0_22 com escala 0=Totalmente restrito, 100=Ver de todos, 200=Ver da sua carteira, 300=Ver e alterar da sua carteira, 350=Ver todos e alterar da sua carteira, 999=Total acesso). E na configuração real da Tracbel esses três itens estão com valor 'T'/'Todos' nas políticas 0, 2, 5, 6, 7 e 9.

**Evidencia:** GE_PolSegItem agrupado por Modulo: CRM_M001 (66 itens), CRM_M051 (mobile), CRM_M003, DMN_M001, GLB_M000; GE_PolSegItLst para ATD_CT0_10/20/22; export polsegctrl.csv mostrando ATD_CT0_10='T' nas políticas 0,2,5,6,7,9

**Licao para a Tracbel:** ACHADO CRÍTICO PARA O PROJETO NOVO: no Vórtice, o escopo de leitura no desktop é praticamente aberto (todo mundo vê tudo, o que restringe é o que pode ALTERAR). Se a Tracbel quer de fato compartimentar por filial/carteira, isso é requisito NOVO, não é 'copiar o que já existe'. COPIAR, sim, a escala de ATD_CT0_20 (restrito / ver todos / ver da carteira / ver e alterar da carteira / ver todos e alterar da carteira / total) — é uma taxonomia de visibilidade boa e completa; ela só nunca foi aplicada de forma restritiva.

### 16. Hierarquia comercial é um ponteiro de um nível só, sem closure table e sem reatribuição

IV_VENDEDOR (349 linhas) tem SeqUsuario (o usuário do vendedor) e SeqUsuarioLider (o usuário do líder). Não há tabela de caminho/closure, não há vigência, não há histórico. Consequências já documentadas e confirmadas: (a) a carteira de pedidos de um gerente é resolvida por 'os CENs cujo SeqUsuarioLider = meu SeqUsuario', então criar usuário novo sem mover SeqUsuario do próprio registro E o SeqUsuarioLider de todos os subordinados deixa o gerente sem enxergar nada (caso Renata Abra, 13/08/2026: usuário novo 1467, registro de gerente IV_VENDEDOR 331 e 6 CENs continuaram no usuário antigo 919); (b) a agenda de aprovação é atribuída ao líder do CEN NO MOMENTO em que é gerada e gravada em IV_Agenda.SeqUsuario — trocar o líder depois não reatribui nada já gerado (caso 12/08/2026, ações 870/871). Complicador de join: IV_Agenda.Vendedor guarda o CODVENDEDOR varchar (ex. 'JHENIFER.SANTOS'), não o SEQVENDEDOR. E o vínculo com empresa é uma tabela à parte, IV_VENDEDOREMPR (929 linhas, SEQVENDEDOR × NroEmpresa).

**Evidencia:** schema/colunas.csv IV_VENDEDOR (18 colunas); docs/REGRAS-DE-NEGOCIO.md seções 2.4/2.5/2.6; memória vortice-troca-usuario-hierarquia e vortice-aprovacao-roteamento; SELECT COUNT(*) IV_VENDEDOR=349, IV_VENDEDOREMPR=929

**Licao para a Tracbel:** EVITAR ponteiro de líder sem histórico. COPIAR: a hierarquia comercial é de fato o eixo de escopo gerencial. ADAPTAR: (1) closure table ou ltree para resolver 'todos os meus subordinados' em uma consulta; (2) vigência (valid_from/valid_to) no vínculo líder-subordinado; (3) ao trocar responsável, o sistema PERGUNTA e REATRIBUI as tarefas abertas — nunca deixar tarefa órfã apontando para o líder antigo; (4) FK numérica sempre, nunca guardar código de vendedor como string.

### 17. Multiempresa: 18 filiais, permissão replicada em todas — isolamento é nominal

GE_Empresa tem 18 linhas (17 Tracbel Agro por cidade + COLORADO EQUIPAMENTOS 891; a 6 'COLORADO DESATIVADO' e a 891 estão com EmUso=0). Praticamente TODA tabela de permissão é chaveada por NroEmpresa: GE_ModuloPerm, GE_Permissao, GE_UsuarioPerm, GE_POLSEGPERM, GE_CampoPerm, GE_UsrParam, GE_ParametroGlobal. GE_Empresa.EmpSeguranca = 1 em 17 das 18 (a 6 aponta para 6), sugerindo que a segurança seria lida da empresa 1. Mas na prática as linhas são replicadas: GE_ModuloPerm tem 3.264 linhas na empresa 1, 2.388 na 2, 2.375 na 3… até 860 na 18. E o efeito real do 'isolamento' é: dos 409 usuários com permissão de módulo, 340 têm acesso a 17 empresas, 20 a 16 empresas, 7 a 18, e apenas 37 estão restritos a 1 empresa. GE_UsuarioPerm, por sua vez, usa NroEmpresa=0 ('todas') em 37.790 das 38.680 linhas — mas 890 linhas são específicas de empresa, criando um modelo híbrido inconsistente.

**Evidencia:** SELECT completo de GE_Empresa (18 linhas, EmpSeguranca); GROUP BY NroEmpresa em GE_ModuloPerm (18 empresas, 3.264→860); distribuição empresas_com_acesso por usuário: 1→37, 16→20, 17→340, 18→7; GROUP BY NroEmpresa em GE_UsuarioPerm → 0:37.790

**Licao para a Tracbel:** EVITAR replicar a linha de permissão por empresa (cartesiano: usuário × módulo × 18 filiais = 25 mil linhas para 409 pessoas, e a manutenção é impossível). ADAPTAR: permissão é do PAPEL; o escopo de empresa/filial é um ATRIBUTO da atribuição do papel (role_assignment com branch_scope: 'todas' | lista | 'a minha'). Assim uma mudança de papel vale para todas as filiais de uma vez, e restringir a uma filial é uma linha, não 18.

### 18. O contexto de empresa é de SESSÃO (uma filial por vez), e o escopo de empresa para relatórios é um CSV dentro de um varchar

O usuário entra em UMA empresa por sessão: IV_USRSTATUS (186 linhas) guarda por usuário SEQPESSOAATIVA, LINKERPPESSOAATIVA, NROEMPRESACNX (a empresa da conexão), RAZAOSOCIALATIVA e DTAATUALIZACAO; e GE_UsrParam guarda 'OPERADOR-EMPULTLG' (299 usuários, Criptografado='S') = última empresa logada, além de 'SEGURANCA-EMPULTLG' (11 usuários). Já o escopo multiempresa para relatórios gerenciais NÃO usa nenhuma tabela de permissão: está em GE_UsrParam com Parametro='PERMRELGERNROEMPRESAIN' (284 usuários), Valor = lista separada por vírgula dentro de um varchar(1000), ex.: '2,3,9,12,4,11,5,10,7,1', '2,3,9,14,11,4,12,5,15,10,18,7,1,13,17,16', '18,17', '11', '891'.

**Evidencia:** SELECT TOP 10 FROM IV_USRSTATUS; GROUP BY Parametro em GE_UsrParam filtrando %PERM%/%EMPR%/%SEG% → PERMRELGERNROEMPRESAIN 284 usuários maxlen 46, SEGURANCA-EMPULTLG 11; amostra dos valores CSV

**Licao para a Tracbel:** EVITAR lista CSV em varchar como mecanismo de autorização — não tem FK, não indexa, não valida, e cria um caminho de permissão paralelo invisível para quem audita as tabelas de permissão. COPIAR: a ideia de contexto de filial ativo na sessão é boa para UX. ADAPTAR: guardar o escopo como linhas (user_branch_scope) e o contexto ativo apenas na sessão/token, nunca persistindo permissão como texto.

### 19. Desativar usuário DESTRÓI as permissões — não há flag de ativo/inativo

GE_Usuario não tem coluna de status; o que existe é Nivel decimal(1) e DTALIMITEUSO datetime (NULL em 100% dos 1.068 usuários, logo nunca usada). O perfil por Nivel, medido: Nivel 0 = 634 usuários com MÉDIA DE 0 MÓDULOS em GE_ModuloPerm (só 47 já logaram alguma vez) — são os desativados; Nivel 3 = 409 usuários, média 56,7 módulos, 251 já logaram; Nivel 4 = 5 usuários (todos SeqPolSeg=7 Vendas Digitais); Nivel 5 = 5 usuários (SeqPolSeg=3 Supervisores); Nivel 7 = 1; Nivel 8 = 13 (média 84,8 módulos) — os masters: TI da Tracbel (RICARDO.MORETTI, NETO.PRADO, CAETANO.ESTEVES, CAIO.CAMPOS, GUSTAVO.AGUIAR, MATHEUS.AUGUSTO), MASTER, e 6 contas do fornecedor com prefixo 'V:' (V:AMAURY, V:DANIEL, V:DAVI, V:GUI.BATISTA, V:MATHEUS.PACIFICO, V:NICOLAS), todas ativas (a V:NICOLAS logou em 28/08/2026). A destruição é confirmada pelo próprio log: GE_Log2 tem 835 eventos 'EXCLUSÃO EM GE_MODULOPERM' e 841 'EXCLUSÃO EM GE_USUARIOPERM'.

**Evidencia:** GROUP BY Nivel com OUTER APPLY contando GE_ModuloPerm → Nivel 0 média 0,0 módulos; SELECT dos usuários Nivel 4/5/7/8 com CodUsuario e DTALOGIN; GE_Log2 GROUP BY Resumo → 'EXCLUSÃO EM GE_MODULOPERM' 835, 'EXCLUSÃO EM GE_USUARIOPERM' 841; DTALIMITEUSO NULL em 1.068/1.068

**Licao para a Tracbel:** EVITAR com força. Desativação tem que ser reversível: status ('ativo'/'suspenso'/'desligado') + deactivated_at, mantendo as atribuições intactas. Destruir a ACL na desativação (a) impede reativação sem refazer tudo, (b) apaga a evidência de o que a pessoa podia fazer quando cometeu um ato que está sendo investigado. Também: contas de FORNECEDOR com nível master precisam ser nomeadas, ter validade e MFA — no Vórtice as 6 contas 'V:' são indistinguíveis de admins internos.

### 20. 70% das contas nunca logaram e 87% estão dormentes há mais de 90 dias

De 1.068 usuários (TipoUsuario='U'): 750 nunca logaram (DTALOGIN NULL) e apenas 140 logaram nos últimos 90 dias. O campo DTALOGIN só começa em 19/02/2025 (máximo 29/08/2026), então parte dos 750 é limitação de instrumentação — mas GE_Usuario só guarda DTALOGIN e DTALOGINANT, isto é, os DOIS últimos acessos, e nada mais. Cobertura de credencial: 383 usuários não têm Senha nem Senha3 (só ChkSum), 22 não têm nada, 368 têm só Senha, 243 têm Senha e Senha3. LoginId está preenchido em apenas 311 de 1.068; a coluna LOGIN varchar(100) está 100% NULL (nenhuma integração de diretório/SSO); EMAILTRAB 100% NULL; SeqPessoa (vínculo usuário↔pessoa) preenchido em só 260; GE_UsuarioLink (vínculo com sistema externo) tem 0 linhas; INDUSREXT=1 em 0 usuários.

**Evidencia:** SELECT com SUMs sobre GE_Usuario WHERE TipoUsuario='U': nunca_logou=750, logou_90d=140, nunca_trocou=332; MIN(DTALOGIN)=19/02/2025; cruzamento Senha/Senha3/ChkSum (383/368/243/52/22); COUNT(LoginId)=311, COUNT(LOGIN)=0, COUNT(EMAILTRAB)=0, COUNT(SeqPessoa)=260

**Licao para a Tracbel:** COPIAR nada. ADAPTAR: (1) identidade federada desde o dia 1 (Entra ID/SSO da Tracbel) — o Vórtice tem a coluna LOGIN e nunca usou; (2) desativação automática por inatividade (ex.: 90 dias sem login → suspende, 180 → desliga); (3) e-mail obrigatório e único no usuário; (4) tabela de sessões/login com histórico, não duas colunas sobrescritas.

### 21. Auditoria: ~10 GB e 32 milhões de linhas, sem retenção, com a tabela antiga congelada desde 2023

Volumes medidos (linhas / MB em disco): GE_LOG_PROCESSO 12.713.138 / 3.300 MB; IV_AgendaLog 11.140.833 / 2.767 MB; GE_LgTb 11.861.777 / 2.503 MB; GE_Log2 4.583.079 / 766 MB; GE_LOG_HISTORICO 1.046.906 / 182 MB; GE_LOG_PESSOA 961.039 / 206 MB; GE_LOG_TRANS 802.787 / 167 MB; GE_LOG_EXT 622.616 / 126 MB; GE_LOG_CONFIG 110.182 / 22 MB; GE_LOG_CONTATO 19.921 / 5 MB. Total ~10 GB / ~32 milhões de linhas. GE_LgTb (o log unificado antigo, colunas Tb, Kn1, Kn2, Ks, DtaLog, Usr, CodApl, Obs varchar(250), Nivel, SEQLOGTB) está CONGELADO: sua última linha é de 05/06/2023 16:30:38, e GE_LOG_PROCESSO começa em 06/06/2023 07:59:32 — em jun/2023 o Vórtice particionou o log por domínio (PROCESSO, HISTORICO, PESSOA, TRANS, EXT, CONFIG, CONTATO, CARTCRED), com a mesma estrutura de 10 colunas e Obs ampliado para varchar(1000). Os 2,5 GB da tabela velha nunca foram expurgados. As tabelas GE_LogAtividade, GE_LOG_CARTCRED, IVC_ATENDENTELOG e IVC_CHAMADALOG estão zeradas.

**Evidencia:** Query em sys.tables×sys.partitions×sys.allocation_units somando total_pages; MIN/MAX(DtaLog) em GE_LgTb (23/12/2018 → 05/06/2023) e MIN/MAX(DTALOG) em GE_LOG_PROCESSO (06/06/2023 → 30/08/2026); schema/colunas.csv das 8 tabelas GE_LOG_*

**Licao para a Tracbel:** COPIAR o particionamento por domínio (facilita expurgo seletivo e evita hot table única). ADAPTAR: definir retenção POR TIPO desde o dia 1 (ex.: log de permissão 5 anos, log de sincronismo 90 dias, log de agenda 2 anos), com partição por data e arquivamento automático para storage frio. EVITAR: guardar 2,5 GB de log morto de 2018-2023 no banco quente por 3 anos.

### 22. Mudança de permissão É auditada — mas login NÃO é (114 eventos em 9 anos)

GE_Log2 (NroEmpresa, Sistema, Modulo, Aplicacao, CodUsuario, Data, Estacao, Nivel, Resumo, Detalhe varchar(1000), LinkTipo, LinkNro, LinkSerie, SEQLOG) registra eventos de aplicação com Detalhe legível. Eventos de segurança: 'ALTERACÃO PERMISSÕES' 34.834 linhas (19/12/2011 → 28/08/2026), 'ALT. PERMISSÃO POLÍTICAS' 1.902, 'EXCLUSÃO EM GE_USUARIOPERM' 841 (desde 13/03/2025), 'EXCLUSÃO EM GE_MODULOPERM' 835, 'PARÂMETRO GLOBAL' 10.415. O Detalhe é excelente: 'Alterado permissão para: EDUARDA.MARIOTO, perm: Bloqueado, em: Recebimento Realizado' com LinkTipo='IV_RESULTADO' e LinkNro=3466. Já 'LOGIN' tem apenas 114 linhas entre 01/08/2017 e 28/08/2026 — não há trilha de autenticação. E 80% do log é ruído de integração: 'SINCRONISMO JUPITER' sozinho tem 3.739.599 linhas (3.684.662 no Nivel 5 + 54.937 no Nivel 3) dos 4,58 milhões totais. O campo Estacao vem VAZIO nas amostras — sem IP nem estação de trabalho. Por fim, GE_Usuario.RegistrarLog='N' em 100% dos 1.386 registros e GE_Aplicacao.RegistrarLog='N' em 100% das 184 aplicações: o interruptor de auditoria fina existe e está desligado em todo lugar.

**Evidencia:** GROUP BY Resumo,Nivel em GE_Log2 (top 25); filtro LIKE '%LOGIN%'/'%SENHA%'/'%ACESSO%'/'%AUTENT%' → só 'LOGIN' 114 e 'EXCLUSÃO EM GE_USUARIOPERM' 841; SELECT TOP 6 de 'ALTERACÃO PERMISSÕES' com Detalhe completo e Estacao vazia; GROUP BY RegistrarLog em GE_Usuario e GE_Aplicacao → 'N' em 1.386 e 184

**Licao para a Tracbel:** COPIAR: o formato do evento de permissão (ator, alvo, permissão em texto legível, tipo+id do recurso, timestamp) é bom e deve ser o padrão. ADAPTAR/CORRIGIR três buracos: (1) log de autenticação obrigatório (sucesso, falha, MFA, origem/IP, user-agent) — hoje inexiste; (2) separar log de AUDITORIA de log de INTEGRAÇÃO (não deixar 3,7 milhões de linhas de sincronismo afogarem 34 mil eventos de segurança); (3) registrar IP/estação sempre.

### 23. A auditoria é 100% da aplicação — acesso direto ao banco não deixa rastro

Só existe 1 trigger no banco inteiro (VTC_T_AUDITORIA em IV_ClientePropr). Logo, GE_LgTb, GE_LOG_*, IV_AgendaLog e GE_Log2 são escritos pelo cliente Gupta e pelo servidor BPM (VRTCSERVER), não pelo SGBD. Combinado com o fato de que também não há RLS (sys.security_policies=0) nem FK cobrindo as ACLs, a consequência é direta: qualquer conta com acesso ao SQL Server contorna simultaneamente TODAS as 8 camadas de permissão e TODA a trilha de auditoria. As próprias tabelas de ACL não têm colunas de quem/quando: GE_UsuarioPerm tem 5 colunas (nenhuma de auditoria), GE_ModuloPerm tem 6 (a sexta é CHKSUM), GE_POLSEGPERM tem 5 — só GE_CampoPerm e GE_PolSegCtrl carregam UsuAlteracao/DtaAlteracao.

**Evidencia:** SELECT de sys.triggers → 1 linha; sys.security_policies → 0; schema/colunas.csv de GE_UsuarioPerm (5 col), GE_ModuloPerm (6 col), GE_POLSEGPERM (5 col), GE_CampoPerm (7 col, com UsuAlteracao/DtaAlteracao)

**Licao para a Tracbel:** EVITAR depender só da aplicação. No CRM novo: (1) RLS no banco como rede de segurança (o filtro de tenant/escopo é do SGBD, não do ORM); (2) auditoria de tabelas críticas por trigger ou CDC, independente da aplicação; (3) toda tabela de permissão com created_by/created_at/updated_by/updated_at; (4) nenhuma conta de aplicação com DDL, e acesso humano ao banco só via conta nominal e read-only, como já é a prática no repositório do agente.

### 24. Token da API guardado em texto claro numa tabela de parâmetros

GE_ParametroGlobal (Sistema, Modulo, NroEmpresa, Parametro, Valor varchar(1000), Criptografado, Dtaalteracao, Usualteracao) guarda em Sistema='CRM', Modulo='WS' o parâmetro 'CrmImpController_token' com o valor legível '41IldlYrotsiMKqQJUi21Q' e Criptografado='N', alterado por VTCCONS (sem data). No mesmo cadastro, o parâmetro 'SMTP_PorUsuario' está com Criptografado='S' e valor ilegível — prova de que a coluna Criptografado funciona e simplesmente não foi usada para o token. Há também 'PolSegVersao'='1.0.30' (versão do modelo de política, alterada por DIEGO.MARQUES em 06/06/2023). A documentação da API (docs/API-wsVorticeCrmApi.md) confirma múltiplos fluxos de autenticação heterogêneos: POST /api/crm/login (valida login do CRM), POST /api/cartao/auth (gera token para os demais endpoints, com WHERE pelo token), POST /api/acordo/auth (2FA por CPF + código com telefone truncado) e POST /api/portal/signin|signup|password|senharecupera.

**Evidencia:** SELECT em GE_ParametroGlobal filtrando %TOKEN%/%SEG%/%SENHA%/%LOG%; docs/API-wsVorticeCrmApi.md linhas 7, 17, 72-73, 77, 81, 88, 101

**Licao para a Tracbel:** EVITAR segredo em tabela de parâmetro. CRM novo: segredos em cofre (Key Vault/secret manager), nunca no banco de negócio; se tiver que ficar no banco, criptografia com chave externa e rotação. E ADAPTAR a autenticação da API: um único mecanismo (OAuth2/OIDC com escopos), não quatro fluxos diferentes por módulo.

### 25. Criar um usuário toca ~8 tabelas e ~110 linhas, sem transação garantida e sem API

Não existe endpoint na wsVorticeCrmApi para criar usuário interno (a API cria pessoa/cliente e usuário de PORTAL). A criação é manual na tela Configuração → Usuários do sistema (GESTOS000) e materializa, com os números medidos nesta base: GE_Usuario (1 linha, incluindo SeqPolSeg e o trio Senha/Senha3/ChkSum gerado pelo botão 'Inicializa senha'); GE_ModuloPerm (média de ~62 linhas por usuário — 25.202 linhas para 409 usuários, replicadas por empresa); GE_Membro (1 a 21 linhas, mediana ~3); GE_UsuarioPerm (ACL de resultados/ações — só herdada se o usuário entrar nos grupos certos); IV_Operador (1 linha com 51 flags); IV_VENDEDOR + IV_VENDEDOREMPR (se for vendedor/CEN/gerente, com SeqUsuarioLider); GE_UsrParam (~40 linhas de preferências, incluindo PERMRELGERNROEMPRESAIN e OPERADOR-EMPULTLG); IV_USRSTATUS (contexto de sessão). Existem 7 linhas de IV_Operador cujo SeqUsuario não existe mais em GE_Usuario, evidência de que a criação/exclusão não é atômica.

**Evidencia:** docs/pedido-vortice-api-usuario.md e memória vortice-criar-usuario; contagens: GE_ModuloPerm 25.202/409 usuários; distribuição de grupos por usuário (1→114 … 21→1); IV_Operador 939 linhas; NOT EXISTS IV_Operador×GE_Usuario → 7 órfãos; GE_UsrParam 16.827 linhas/~294 usuários

**Licao para a Tracbel:** COPIAR o checklist (é uma boa lista do que um usuário de CRM precisa ter). ADAPTAR para que provisionamento seja UM comando: criar usuário = criar identidade + atribuir papel(is) + definir escopo (filial, carteira, líder) — e as ~110 linhas devem ser DERIVADAS do papel, não digitadas. Provisionamento e desprovisionamento por API desde o dia 1, para plugar no onboarding (Fluig) e no offboarding do RH.

### 26. O bitmask por política sobre catálogos: DMN_DOCTP (tipos de documento) é o caso mais usado

GE_POLSEGPERM concede permissão a um REGISTRO de catálogo por POLÍTICA (não por usuário), com PERMISSAO em bitmask decimal. Distribuição por tipo de recurso: DMN_DOCTP (tipo de documento do Doc Manager) 469 linhas / 102 chaves / 10 políticas; ATFX_FORMACTTO (forma de contato) 84 / 17 / 5; IV_GLOBALPAR (parâmetro global!) 81 / 48 / 6; ATFX_TIPORELAC 65 / 13 / 5; ATFX_PAPELCTT 33 / 11 / 4; PRLST_CRMPES_FAIXAFATURA 27 / 9 / 5; ATFX_PESSTATUS 19 / 5 / 7; PRLST_CRMPES_TIPOFONE 15 / 3 / 5; ATFX_GRUPO 7 / 1 / 7; PRLST_CRMPES_FAIXARENDA 5 / 1 / 5. Exemplo de leitura: a política 4 (Administrador Sistema) tem 94 tipos de documento com PERMISSAO=32 (total) e 6 com 8 (inserir); a política 6 (CEN) tem 24 com 8 e 4 com 4 (só leitura); a política 2 (Atendentes) tem 85 com 8. Notável: IV_GLOBALPAR significa permissão de ALTERAR PARÂMETRO DO SISTEMA por política, chave a chave (48 parâmetros controlados).

**Evidencia:** GROUP BY CODAPLICACAO em GE_POLSEGPERM; GROUP BY SEQPOLSEG,PERMISSAO para DMN_DOCTP e IV_GLOBALPAR; GE_PolSegItLst decodificando 4=Apenas leitura, 8=Inserir, 16=Inserir/Alterar, 32=Inserir/Alterar/Excluir

**Licao para a Tracbel:** COPIAR: conceder ACL de catálogo ao PAPEL (aqui, à política) em vez de ao usuário é o caminho certo — repare que essa camada tem 805 linhas contra 38.680 da equivalente por usuário. O erro do Vórtice foi manter as DUAS. No CRM novo: só a versão por papel, com override individual raro e datado. E permissão sobre parâmetro de sistema (IV_GLOBALPAR) é ideia boa: configuração sensível deve ter dono.

### 27. Controles de comportamento por política que valem a pena copiar (e os que revelam ausências)

O catálogo GE_PolSegItem tem 108 itens em 5 módulos (CRM_M001 desktop 66, CRM_M051 mobile, CRM_M003 formulários, DMN_M001 documentos, GLB_M000 global) e cobre coisas que a maioria dos CRMs não parametriza: EE.AGDANT 'Permite agendamento retroativo até quantos minutos úteis (>10.000=Ilimitado)' — configurado em 0 para CEN e 99.999 para Administrador; EE.AGDAFU 'Permite andamento de agendas futuras até n dias'; EH.HS10 'Qtde de dias para registro de andamento retroativo' — 1 para Atendentes, 2 para CEN, 5 para DSI, 999 para Administrador; EH.HS90 'Registrar históricos apenas com a data atual (bloqueará campo de data) [SalesFunnel]'; EE.HPTAGD 'agendar fora do período padrão de trabalho'; PR.Q60/Q61 'Permite ao gestor do modelo alterar manualmente a fase / o status de um processo' (0 para CEN); PR.Q40 'trocar a empresa de um processo durante andamento'; PR.P10 'Permite excluir tudo relacionado ao processo'; RE_90 'exportar relatórios (CSV)' e REG_10 'executar relatórios gerenciais' — ambos 0 para CEN e 1 para Atendentes/Admin; TT_01 'Desabilitar a chamada de navegadores de internet'; GLB_M000/HH.A01 'Utiliza o controle de visualização de dados (LGPD)' — valor 0 em TODAS as 11 políticas, ou seja, o controle LGPD está desligado. Silos (fila compartilhada de atividades, itens FF.AA01/AA15/AA20 e GLB FF.A01) existem no modelo mas GE_Usuario.INDSILO=0 em 100% dos 1.386 registros — nunca usados na Tracbel.

**Evidencia:** Listagem completa de GE_PolSegItem (108 itens com Modulo/Item/Ordem/Tipo/SubTipo/Descricao); export polsegctrl.csv com os valores por política; GROUP BY INDSILO,TIPOSILO em GE_Usuario → 0 em 1.068 'U' e 318 'G'

**Licao para a Tracbel:** COPIAR direto para o backlog do CRM novo: janela de retroatividade de lançamento por papel, bloqueio de edição de data de histórico, permissão de exportar/executar relatório gerencial (é controle de vazamento de dados!), e permissão de alterar fase/status manualmente (é controle de fraude de funil). EVITAR: deixar o controle de LGPD como flag desligado — se o CRM guarda CPF e faturamento de cliente, mascaramento de campo tem que ser padrão, não opcional.

### 28. As 11 políticas de segurança e como estão distribuídas — inclusive 710 registros sem política

GE_PolSeg (SeqPolSeg, Politica varchar(30), Descricao varchar(250)) tem 11 linhas: 0 Global, 1 Vórtice (equipe do fornecedor), 2 Atendentes, 3 Supervisores, 4 Administrador Sistema, 5 Temporario, 6 CEN, 7 Vendas Digitas [sic], 8 Adm de Vendas, 9 DSI, 999999 * Master. Distribuição em GE_Usuario.SeqPolSeg: 501 usuários na política 2 (Atendentes), 71 na 3, 64 na 6 (CEN), 12 na 7, 11 na 1, 8 na 4, 7 na 8, 2 na 9 — e 710 registros com SeqPolSeg NULL (dos quais 318 são grupos, restando ~392 usuários sem política atribuída, que caem no default/Global). Densidade de configuração por política em GE_PolSegCtrl: CEN 65 itens, Administrador Sistema 38, Atendentes 32, DSI 32, Vendas Digitais 31, Temporario 17, Global 13, Supervisores 9, Adm de Vendas 9, Vórtice 7, * Master 1. Ou seja: a política mais restritiva (CEN) é a mais detalhada, e 'Supervisores' — que teoricamente teria escopo ampliado — tem só 9 itens configurados.

**Evidencia:** SELECT * FROM GE_PolSeg (11 linhas); GROUP BY SeqPolSeg em GE_Usuario com JOIN em GE_PolSeg; GROUP BY SeqPolSeg em GE_PolSegCtrl

**Licao para a Tracbel:** COPIAR: política de segurança nomeada, com herança de um Global, é um bom modelo mental (é o 'Profile' do Salesforce). ADAPTAR: (1) política obrigatória — 392 usuários sem política é buraco; (2) mostrar na tela o valor EFETIVO (herdado do Global vs sobrescrito), porque hoje 'Supervisores' com 9 itens parece restritivo e na verdade herda tudo; (3) versionar a política (existe GE_ParametroGlobal PolSegVersao='1.0.30', mas não há histórico de qual usuário tinha qual política em que data).

### 29. Janela de horário de acesso: existe, funciona por dia da semana e módulo, e tem 3 linhas

GE_PolSegAces(SeqPolSeg, Modulo, DiaSem numeric(1), HINI1, HFIM1, HINI2, HFIM2 — todos decimal(4)) permite restringir acesso por política × módulo × dia da semana com DOIS intervalos por dia (típico de horário comercial com intervalo de almoço). A tabela tem apenas 3 linhas — o recurso existe e praticamente não é usado. Horários são guardados como decimal(4) (formato HHMM), não como time.

**Evidencia:** schema/colunas.csv GE_PolSegAces (7 colunas); schema/tabelas.csv GE_PolSegAces = 3 linhas; FK__GE_PolSeg__SeqPo__1FE46232 → GE_PolSeg

**Licao para a Tracbel:** COPIAR o conceito (restringir horário de acesso por papel é controle real, principalmente para usuário terceirizado/temporário — a política 5 'Temporario' existe justamente para isso). ADAPTAR: usar tipo time/timestamptz com fuso (GE_Empresa até tem FusoHorario decimal(2), sem uso aparente), permitir exceções por data (feriado — existe GE_FERIADO e GE_DiaNaoUtil, ambas com 0 linhas) e logar a tentativa fora de janela.

## Lacunas declaradas

- Não consegui ler o CÓDIGO das 411 views nem das ~419 procedures/functions: a conta CRM_Leitura não tem VIEW DEFINITION (sys.sql_modules.definition vem NULL). Consequência prática: a lógica EXATA de resolução de escopo (o WHERE que o cliente monta para decidir quais pessoas/processos/agendas cada usuário vê) foi INFERIDA a partir dos itens de política, das colunas e do comportamento documentado — não foi lida. Também não pude ler a definição das views GE$USUARIOPERM / GE$MODULOPERM / GE$POLSEGPERM, só medir o resultado. O script docs/criar-usuario-visualizacao-sql.sql já existe no repositório para pedir essa permissão (GRANT VIEW DEFINITION) — vale executá-lo antes da próxima rodada de engenharia reversa.
- Algoritmo da senha desconhecido. Sei que é determinístico, sem salt, produz sempre 30 caracteres, contém bytes fora do ASCII imprimível (não é hex nem base64) e que existem três colunas relacionadas (Senha, Senha3, ChkSum). Não sei se é hash proprietário ou cifra reversível, nem qual o papel de Senha3 vs Senha (243 usuários têm as duas), nem o que ChkSum protege (994 usuários). Isso importa para a MIGRAÇÃO: se for cifra reversível, as senhas atuais são recuperáveis por quem tem o banco; se for hash, ninguém migra senha e todos trocam.
- Semântica exata da chave de GE_UsuarioPerm quando CodAplicacao='IV_ACAORES' (5.993 linhas, 825 chaves distintas, faixa 1 a 940078): 802 das 825 chaves batem com IV_Resultado.Resultado e 821 batem com IV_Acao.Acao — os dois domínios numéricos se sobrepõem, então não consegui determinar se a chave é a ação, o resultado ou um par codificado. Precisaria da definição da tela/procedure.
- Não determinei se GE_Permissao (CRUD por tela) ainda é CONSULTADA em runtime ou se é código legado. As 302 linhas e a concentração no pseudo-usuário 'todos' sugerem legado, mas sem o código da aplicação não dá para afirmar.
- Não sei qual módulo prevalece quando GLB_M000 e GLB_M001 guardam o MESMO item com valores diferentes (caso concreto: política 6/CEN tem GLB_SH0_20='1' e GLB_SH0_25='12' em GLB_M000, contra 'F' e '4' em GLB_M001). Isso muda a resposta sobre qual é a política de senha efetiva hoje.
- A semântica dos Niveis 1, 4, 5 e 7 de GE_Usuario foi inferida por correlação (nível 4 ↔ política 7 Vendas Digitais; nível 5 ↔ política 3 Supervisores; nível 8 ↔ política 4 Administrador e 1 Vórtice). Não há tabela de domínio para Nivel no banco — pode haver regra codificada na aplicação que eu não vi.
- GE_CtrlM/GE_CtrlE/GE_CTRLUSR (ControlSet de licença) estão ofuscados em hexadecimal; não apurei o LIMITE de usuários licenciados nem a data real de expiração, que são informações relevantes para o business case de substituição.
- Não verifiquei se a wsVorticeCrmApi aplica alguma das 8 camadas de permissão nas chamadas REST — a documentação em docs/API-wsVorticeCrmApi.md descreve os fluxos de autenticação (/api/crm/login, /api/cartao/auth com token, /api/acordo/auth com 2FA, /api/portal/*) mas não a autorização por recurso. Se a API ignora GE_UsuarioPerm/GE_ModuloPerm, ela é um bypass de todo o modelo.
- Não medi o custo de RUNTIME das views de resolução (tempo de execução / plano de GE$USUARIOPERM com 4,38M linhas), apenas o volume. Faltou VIEW DATABASE STATE para olhar planos e esperas.
- Não apurei a política de backup/retenção nem se existe expurgo agendado para as tabelas de log — concluí 'sem retenção' a partir da evidência de que GE_LgTb mantém 2,5 GB de dados de 2018-2023 três anos depois de ter sido substituída.
