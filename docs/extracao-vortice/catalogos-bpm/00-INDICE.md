# Catálogos declarativos do Vórtice CRM — o "código-fonte" que mora em tabela

Extraído em 2026-09-02 do banco de produção `CRM`, somente leitura, via
`scripts/vortice-extracao/03-exporta-catalogos.ps1`.

**116 tabelas exportadas integralmente, 41.818 linhas.** Cada arquivo é um CSV UTF-8 com cabeçalho.

> **Segurança.** Toda coluna cujo nome contenha `senha`, `hash`, `token`, `pasw`, `passw`, `pwd` ou
> `secret` foi **omitida** da exportação. Duas tabelas tiveram colunas suprimidas:
> `GE_ObjDinamico.Senha` e `GE_Consulta.Senha`. Nenhuma senha, hash ou token saiu do banco.

---

## Por que estes CSVs são o artefato mais importante da extração

O Vórtice não é um sistema onde as regras estão no código compilado. Ele é um **motor genérico
configurado por dados**: as 62 modalidades de processo, as 980 ações, os 4.209 resultados, as 5.958
regras de ação automática e as 1.338 regras de notificação são *linhas de tabela*. O binário
(`VCRM.exe`, `CRMWeb`, o Servidor de Processos) apenas interpreta esse conteúdo.

Consequência prática para o projeto de substituição: **migrar o Vórtice não é reescrever 548 objetos
T-SQL — é reimplementar o interpretador destes 116 catálogos.** O volume de regra de negócio nas
tabelas abaixo é maior, em ordens de grandeza, do que o volume de regra nas procedures.

---

## Nomes reais descobertos no catálogo

Três das tabelas pedidas por nome não existem com esse nome. Os nomes reais são:

| Nome procurado | Nome real no banco | Linhas | Observação |
|---|---|---:|---|
| `IV_Fase` | **`IV_ProcFase`** | 273 | fases dos modelos de processo (`CodProcesso`, `Fase`, `FaseOrdem`, `Marco`) |
| `GE_Parametro` | **`GE_ParametroGlobal`** | 239 | + `IV_GlobalPar` (3.403) para parâmetros por empresa |
| tabela do Message Center | **`IV_ResMsgPapel`** | 1.338 | regra de mensagem por (resultado × papel × canal × empresa) |
| tabela dos Objetos Dinâmicos | **`GE_ObjDinamico`** | 196 | a coluna `Comando` guarda o SQL inteiro |

`IV_AcaoAutoCtrl`, `IV_ResultadoReq`, `IV_ResParam`, `IV_RetProcRegra`, `GE_CampoExig` e
`GEP_JOBNOTIFICAR` existem mas estão **vazias** — recursos do produto que a Tracbel nunca ligou.

---

## Núcleo BPM: o modelo de processo

| Arquivo | Linhas | O que é |
|---|---:|---|
| `IV_CodProcesso.csv` | 62 | os **modelos de processo** (tipos de fluxo). É o `CodProcesso` — não confundir com o número do processo |
| `IV_ProcFase.csv` | 273 | as fases de cada modelo, com `FaseOrdem` e `Marco` (ex.: `2.Pedido.Analise`) |
| `IV_ProcSt.csv` | 250 | os status possíveis por modelo |
| `IV_ProcResultado.csv` | 2.161 | **a tabela que governa tudo**: para cada par (modelo, resultado), qual Fase e qual Status o processo assume |
| `IV_ProcAcao.csv` | 740 | quais ações são válidas em cada modelo |
| `IV_ProcPersp.csv` | 67 | perspectivas (probabilidade/estágio) |
| `IV_ProcVinc.csv` | 192 | vínculo entre processos (processo pai/filho) |
| `IV_ObjFlow.csv` | 726 | desenho do fluxo (posição dos nós no editor) |
| `IV_CodPrcEmpr.csv` | 17 | quais modelos valem em quais empresas |

**Leitura:**

1. **58 dos 62 modelos estão `EmUso = 1`.** Por tipo: 26 `VENDA`, 11 `PROSPECÇÃO`, 11 `OUTRO`,
   3 `SERVIÇOS`, 1 `AFERIÇÃO` e 10 sem tipo. Há uma geração inteira de modelos "JDE" (Venda Máquina
   JDE, Entrega Máquina JDE, Venda Peças JDE, Prospecção Peças JDE…) convivendo com os modelos
   antigos equivalentes — sinal de que a migração de ERP duplicou o catálogo em vez de substituí-lo.
2. Existe um modelo chamado literalmente **`Teste Workflow`** marcado como em uso.
3. `IV_ProcResultado` é o **coração declarativo**: 2.161 linhas mapeando resultado → (fase, status).
   Um erro de uma linha aqui muda o comportamento do sistema inteiro para aquele fluxo. É a
   origem do defeito documentado em `REGRAS-DE-NEGOCIO.md` §2.2, em que um resultado de "Atividade
   Cancelada" leva o processo a `Fase = Finalizado / Status = CANCELADO`.
4. `INDFASEBASEACAO` está **100% nulo** nos 62 modelos e `INDUSABOARD` é `0` em 61 de 62 — dois
   recursos do produto (fase derivada da ação; visão kanban) que a Tracbel não usa.

---

## Ações, resultados e automação

| Arquivo | Linhas | O que é |
|---|---:|---|
| `IV_Acao.csv` | 980 | catálogo de ações/tarefas |
| `IV_Resultado.csv` | 4.209 | resultados possíveis por ação — **63 colunas**, das quais 35 são flags `CTRL*` |
| `IV_ClasseRes.csv` | 40 | classes de resultado |
| `IV_AcaoAuto.csv` | 5.958 | **regras de ação automática**: resultado X gera ação Y para o usuário Z |
| `IV_AcaoRem.csv` | 266 | remoção automática de ações |
| `IV_Distribui.csv` | 620 | distribuição de agenda entre usuários |
| `IV_Operador.csv` | 941 | operadores/atendentes |
| `IV_Motivo.csv` | 45 | motivos |
| `IV_Evento.csv` | 132 | eventos |

**Leitura:**

1. **Só 378 das 980 ações estão `EmUso = 'S'` (38,6%).** As outras 602 são catálogo morto que
   continua aparecendo em consultas e relatórios mal filtrados.
2. **Só 10 das 980 ações têm formulário associado** (`SeqFormulario` preenchido), embora existam
   176 formulários cadastrados — a maior parte dos formulários é acionada por outro caminho
   (pelo resultado, não pela ação).
3. `IV_Resultado` tem **35 colunas `CTRL*`** que funcionam como um mini-motor de regras por coluna:
   `CTRLINTERATIVO` (4.188 linhas preenchidas), `CTRLPRODUTIVO` (3.647), `CTRLMUDARATDREAG` (3.481),
   `CTRLDETALHE` (3.415), `CTRLCONCLUSAO` (2.812). Cada flag corresponde a um comportamento de tela.
   Reimplementar isso significa reimplementar 35 comportamentos condicionais, não um workflow.
4. `IV_AcaoAuto`: **5.708 das 5.958 regras estão em uso** e **5.336 são `Exigida = 'S'`** (a ação
   gerada é obrigatória). Elas cobrem 1.037 resultados distintos e 512 ações distintas.
   `TipoAgendamento` é `A` (automático) em 5.388 e `C` em 570.
5. **A explosão multiempresa é literal**: 1.429 regras são globais (empresa em branco) e as outras
   ~4.500 se repetem quase igualmente entre as 18 empresas (entre 239 e 354 regras por empresa).
   Ou seja, a mesma regra de negócio está fisicamente duplicada 17 vezes. Qualquer mudança de
   processo exige 18 edições. **Este é o maior gerador de custo de manutenção do catálogo.**
6. `UsaObjDyn` está zerado nas 5.958 regras: nenhuma ação automática usa Objeto Dinâmico como
   condição, apesar de o recurso existir.

---

## Formulários e questionários

| Arquivo | Linhas | O que é |
|---|---:|---|
| `IV_Formulario.csv` | 176 | formulários (cada um vira uma tabela física `IV_Q_<NOME>`) |
| `IV_Questao.csv` | 2.303 | perguntas, com `Nomecoluna`, `TipoDado`, `ProximaQuestao`, `Script` |
| `IV_QuestaoLista.csv` | 3.995 | listas de valores das perguntas |
| `IV_Atributo.csv` / `IV_AtribLista.csv` | 10 / 37 | atributos livres |

**Leitura:** as 2.303 questões se distribuem por **174 formulários distintos**. Cada formulário em
uso gerou uma **tabela física própria** no banco (`IV_Q_ACOMPANHAMENTO_VENDA`, `IV_Q_AFERICAO_PECAS`,
…) — são cerca de 200 tabelas do prefixo `IV_Q_`, o que explica boa parte das 767 tabelas.
`ProximaQuestao` implementa lógica de salto (formulário condicional) e `Script` guarda código —
duas coisas que precisam de equivalente explícito no sistema novo.

---

## Message Center — regras de notificação

Arquivo: **`IV_ResMsgPapel.csv` (1.338 linhas)**.

Estrutura: para cada `Resultado`, define `TipoDest` (para quem), `Papel`, `Canal`, `SeqTxtPadrao`
(qual texto padrão), `Remetente`, `NroEmpresa`, `PRIORIDADE`, `QTDEDIAS`/`QTDEHU` (prazo) e `EMUSO`.

**Leitura:**

1. **1.323 das 1.338 regras estão em uso.** Cobrem **273 resultados distintos** — ou seja, ~6,5% dos
   4.209 resultados disparam notificação.
2. **O canal é `EML` em 100% das 1.338 regras.** Não há uma única regra de SMS ou WhatsApp ativa,
   embora o produto suporte (existe o parâmetro `MESSCENTER/SMSURL` apontando para a Infobip, e as
   tabelas `IV_SMS`/`IV_SMSLog` — **paradas desde 29/07/2021**). O CRM notifica só por e-mail.
3. **Nenhuma das 1.338 regras usa `Template`** (coluna vazia em todas): o corpo da mensagem vem de
   `IV_TxtPadrao` (212 textos), não de template HTML.
4. `TipoDest`: 1.323 `USR` (usuário nominal), 13 `PAPEL`, 2 `OBDYN` (destinatário resolvido por
   Objeto Dinâmico). Notificar **pessoa nominal em vez de papel** é o padrão — o que significa que
   toda saída/troca de funcionário exige mexer nestas 1.323 linhas.
5. Mesma duplicação multiempresa das ações automáticas: 559 regras globais + ~780 replicadas por
   empresa.

Arquivos relacionados: `IV_TxtPadrao.csv` (212 textos padrão), `GE_EMAILTEMPLATE.csv` (8),
`IV_OPTEMAIL.csv` (21 contas de saída), `IV_OPTFONE.csv` (2).

---

## Objetos Dinâmicos — SQL guardado dentro do banco

Arquivo: **`GE_ObjDinamico.csv` (196 linhas)**, coluna `Comando` com o SQL completo
(o maior tem 1.788 caracteres). Coluna `Senha` **omitida**. Complemento: `GE_ObjDinAplic.csv` (124).

**Leitura:**

1. Por tipo: **168 `TESTE`** (validações booleanas que bloqueiam ou liberam um passo do processo),
   **25 `LINHA_INFORMAÇÃO`** (dados exibidos na tela) e **3 `PESQUISA_PESSOA`**.
2. O SQL usa **variáveis de bind proprietárias do Vórtice** (`:vCRMnProcessoAtivo`,
   `:vnDiferenteZero`) e **nomes de tabela com `$`** (`IV_Q$ACOMPANHAMENTO_VENDA`) que o runtime
   traduz para `_` antes de executar. **125 dos 196 comandos citam tabelas `IV_Q$`**, isto é,
   dependem diretamente do esquema físico gerado pelos formulários.
3. `UsoReqResultado = 1` em **168** objetos: eles são pré-requisito de resultado — na prática,
   **são as regras de validação do fluxo**. `UsoProcesso` em 126, `UsoWorkFlow` em 96,
   `UsoPessoa` em 45, `UsoAgenda` em 32.
4. **Três objetos apontam para uma conexão externa chamada `SISDIA`** (`PERFIL - Bloq
   Documentação(Linx)`, `OS-VIsualizar Agenda`, `OD_AFERIÇÃO_SERVIÇO`). `SISDIA` é o sistema
   legado Linx; a conexão é resolvida fora do SQL Server. São 3 pontos de integração invisíveis
   para qualquer análise de dependências do banco.
5. `TipoRetorno` está vazio nos 196 — o contrato de retorno é implícito.

Relacionados: `GE_Consulta.csv` (2, com `Senha` omitida), `GE_ConsultaVar.csv` (3),
`GE_CONSSQL.csv` (1), **`IV_ListSQL.csv` (360)** — mais SQL guardado em tabela, usado para popular
listas de valores: 278 do tipo `QUESTAO`, 54 `PROPRIED`, 28 `GLBPAR`. **Um desses SQLs ainda
consulta `LINXMAQ.cxmodelo`**, esquema do sistema Linx anterior.

---

## Parâmetros globais

| Arquivo | Linhas | O que é |
|---|---:|---|
| `GE_ParametroGlobal.csv` | 239 | parâmetros por (Sistema, Módulo, Empresa) com `Valor` e flag `Criptografado` |
| `GE_ParamLista.csv` | 405 | listas de valores de parâmetro |
| `IV_GlobalPar.csv` | 3.403 | parâmetros "gordos" do CRM: 6 campos texto + 6 numéricos + 6 datas por linha, agrupados por `SeqGlbPar` |
| `IV_GlobalParLista.csv` | 197 | listas associadas |
| `GE_SyncParam.csv` | 635 | parâmetros de sincronismo do mobile |

**`IV_GlobalPar` tem 37 grupos (`SeqGlbPar`) distintos.** Os maiores: 9401 (861 linhas), 15 (608),
9404 (301), 9400 (198), 28 (168). É uma tabela *entity-attribute-value* de propósito geral — o
`SeqGlbPar = 12` é o que o trigger de auditoria consulta para achar o assistente do vendedor.

### Parâmetros que apontam para IP, URL, caminho de rede ou SMTP

Último octeto / segmento final **mascarado**. 19 parâmetros dos 239 carregam endereço:

| Sistema | Módulo | Parâmetro | Valor (mascarado) | Situação |
|---|---|---|---|---|
| CRM | OUT | `OutParLinkBaseAgenda` | `http://192.168.109.xxx/CRMWeb/Forms/frmAndamento.aspx?p=` | **IP MORTO** (rede pré-migração de data center) |
| CRM | OUT | `OutParLinkBasePessoa` | `http://192.168.109.xxx/CRMWeb/Forms/frmPessoa.aspx?p=` | **IP MORTO** (mesma rede) |
| CRM | DOCMANAGER | `FTPWebSrv` | `10.150.14.xxx` | servidor de banco/FTP atual — IP fixo, sem nome DNS |
| CRM | MESSCENTER | `SMSURL` | `https://api-sp3.infobip.com/sms/1/text/single` | endpoint Infobip; **sem nenhuma regra SMS ativa** |
| GLOBAL | GLOBAL | `SMTPServer` | `smtp.office365.com` | — |
| GLOBAL | GLOBAL | `SMTPusr` | `noreply_agro@tracbel.com.br` | — |
| GLOBAL | GLOBAL | `eMail envio` | `noreply_agro@tracbel.com.br` | — |
| GLOBAL | GLOBAL | `SMTPpsw` | *(criptografado, não exportado em claro)* | ver observação abaixo |
| GLOBAL | QUERYVIEW | `QrpPath` | `C:\Vortice\TDev\qrp` | caminho local do servidor de aplicação |
| IMPORT | IMP | `PastaEntrada` | `D:\Client\Importador\CRM_Import` | pasta de entrada da integração |
| INTERVISION | IMP_EXT | `PastaEntrada` | `G:\Client\Global\Arquivo` | unidade `G:` — mapeamento de rede |
| VORTICO | VORTICO | `arquivocontasERP` | `C:\Vortice\BIAssist\DRE_PlanoContasBase.txt` | arquivo texto de plano de contas |
| VORTICO | VORTICO | `arquivoestruturaDRE` | `D:\Client\BIAssist` | — |

Os demais (`CatchUsrMonit`, `BLOQMANUT`, `PAR0004`, `UsaGeraFonetica`, `DiasBkp`, `SessoesERPOUT`)
estão com o valor **criptografado** por uma cifra proprietária do Vórtice.

> **Achado de segurança.** **31 dos 239 parâmetros** têm `Criptografado = 'S'`, incluindo a senha
> do SMTP. A cifra é reversível pela própria aplicação e a chave não está em lugar nenhum do banco —
> logo, mora no binário. O sistema novo deve tirar segredo de tabela de parâmetro e usar um cofre.

> **Achado operacional.** `OutParLinkBaseAgenda` e `OutParLinkBasePessoa` apontam para
> `192.168.109.209`, endereço da rede **anterior à migração de data center**. Toda notificação do
> Message Center que inclui link "abrir no CRM" está entregando **link quebrado** — e como o canal
> é 100% e-mail, isso vale para as 1.323 regras ativas.

---

## Segmentação e carteiras

| Arquivo | Linhas | O que é |
|---|---:|---|
| `IVS_Depto.csv` | 29 | departamentos (CEN) |
| `IVS_Carteira.csv` | 655 | carteiras de cliente |
| `IVS_CartCid.csv` | 673 | cidades por carteira |
| `IVS_CartDepto.csv` | 206 | carteira × departamento |
| `IVS_DeptoRes.csv` / `IVS_DeptoEmpr.csv` / `IVS_DEPTOPOT.csv` | 63 / 13 / 126 | responsáveis, empresas e potencial por departamento |
| `IVS_Segm.csv` | 8 | segmentos |
| `IV_VENDEDOR.csv` | 351 | vendedores |
| `IVS_UsrMeta.csv` | 1 | metas por usuário — **praticamente não usado** |

`INDUSAPOTZ` é nulo nos 29 departamentos. A tabela de metas tem **uma única linha**: gestão de meta
por usuário é feita fora do CRM.

---

## Plataforma: sistemas, módulos, aplicações e segurança

| Arquivo | Linhas | O que é |
|---|---:|---|
| `GE_Sistema.csv` | 11 | os "sistemas" da plataforma |
| `GE_Modulo.csv` / `GE_Mod.csv` / `GE_AppModulo.csv` | 48 / 32 / 30 | módulos |
| `GE_Aplicacao.csv` | 184 | aplicações (telas/executáveis) |
| `GE_PolSeg.csv` | 11 | **políticas de segurança** (perfis) |
| `GE_PolSegItem.csv` | 108 | itens de política |
| `GE_POLSEGPERM.csv` | 818 | permissão por (aplicação, chave, empresa, política) |
| `GE_PolSegCtrl.csv` / `GE_PolSegItLst.csv` / `GE_PolSegAces.csv` | 254 / 66 / 3 | controles e acessos |
| `GE_Permissao.csv` | 302 | permissões |
| `GE_Tab.csv` / `GE_Col.csv` / `GE_Alias.csv` | 12 / 217 / 1.413 | dicionário de dados da aplicação |
| `GE_Empresa.csv` | 18 | as 18 empresas |

**Leitura:**

1. Os 11 "sistemas" são: `GLOBAL`, `INTERVISION` (= o CRM propriamente dito, sigla `IV`),
   `SERVPROCESSO`, `DMN`, `GT`, `INTEGRADOR`, `JUNO`, `QUERYVIEW`, `QVW`, `Vortico`, `ZUMBI`.
   **Sete deles têm descrição literal "Sistema indefinido"** — a própria plataforma não sabe o que
   são; são compartimentos criados por instalação de módulo.
2. **Só 11 políticas de segurança** para 1.388 usuários e 184 aplicações. A granularidade real vem
   de `GE_POLSEGPERM` (818 linhas), que cruza aplicação × chave × empresa × política.
3. `GE_Alias` com 1.413 linhas é o dicionário que renomeia campos na interface — outro lugar onde
   customização foi feita por dado, não por código.
4. As 18 empresas são todas `TRACBEL AGRO — <cidade>`, 17 em SP (uma sem estado preenchido).
   Multiempresa aqui é **multifilial**, não multiorganização.
5. **`GE_Usuario` e `GE_Membro` não foram exportados** por conterem dados pessoais e credenciais.
   Os números (1.388 usuários, sendo 1.070 `TipoUsuario = 'U'` e 318 `'G'`) estão em
   `dominios/valores.csv`.

---

## Servidor de Processos (jobs)

| Arquivo | Linhas | O que é |
|---|---:|---|
| `GEP_JOBCAD.csv` | 71 | **catálogo de tipos de job** que o produto sabe executar |
| `GEP_JOBAGD.csv` | 25 | **os jobs efetivamente agendados**, com intervalo e última execução |
| `GEP_JOBFILA.csv` | 25 | fila corrente |
| `GEP_Fila.csv` | 81 | fila de processamento |
| `GEP_JobMonitor.csv` | 2 | monitoração |
| `GEP_Processo.csv` | 8 | processos do servidor |
| `GEP_ParEnvia.csv` / `GEP_ParRecebe.csv` | 3 / 14 | parâmetros de envio/recebimento |
| `GEP_UsrSat.csv` | 71 | usuários do satélite (mobile) |
| `GEP_IMPORT_GUI.csv` | 52 | importação por interface |

**Leitura:**

1. Os 71 tipos de job em `GEP_JOBCAD` revelam o **escopo do produto de prateleira**, não o uso da
   Tracbel: há jobs para SAP (`SAP_IMPORT_PEDIDO`, `SAP_IMPORT_EXT`), Bradesco (`SIMU_BRADESCO`),
   Conductor, Neurotech, Boa Vista, Certiface, Meliuz, PierLabs, Hyundai Leads, MKTZap, URA/discador,
   Google Agenda, Netpoint. **A Tracbel usa 25 deles.**
2. **Dos 25 jobs agendados, 8 estão mortos ou nunca rodaram:**

   | Job | Aplicação | Última execução |
   |---|---|---|
   | `APPEXEC` | `D:\VORTICE\PDI\KITCHEN_VEICULO.LNK` | **30/07/2021** |
   | `APPEXEC` | `D:\VORTICE\PDI\KITCHEN_TITULOS_ACRESC.LNK` | **29/07/2021** |
   | `APPEXEC` | `D:\VORTICE\PDI\KITCHEN_PECAS.LNK` | **29/07/2021** |
   | `APPEXEC` | `D:\VORTICE\PDI\PAN_IMP_VEICULO.LNK` | **29/07/2021** |
   | `APPEXEC` | `D:\VORTICE\PDI\PAN_TITULOS_SISDIA.LNK` | **29/07/2021** |
   | `APPEXEC` | `D:\VORTICE\PDI\INTEGRA BLOQ DOCUMENTAÇÃO.LNK` | **29/07/2021** |
   | `EMAIL_IN_CRM` | — | **29/04/2024** |
   | `COBRANCA_CRIT` | — | **21/05/2025** |
   | `PR_INT_PROPROS` | procedure | **18/09/2025** |
   | `SMS_SEND`, `MOV_PESSOA`, `SYNCSAT_PRODUTOS_GERA` | — | **nunca executaram** |

   Os seis `APPEXEC` são **transformações Pentaho Data Integration** (`.LNK` para Kitchen/Pan)
   que pararam todas no mesmo dia, 29–30/07/2021. Foi quando a integração ETL foi desligada — e é
   a mesma data em que `IV_ProcRef`, `IV_SMS` e `IV_SMSLog` param de receber linhas.
   Um deles (`PAN_TITULOS_SISDIA`) confirma que o sistema Linx/SISDIA ainda era origem de dados.
3. **Os jobs vivos** (última execução em 02/09/2026) são só 11: `CRMOUT_AGD_SEND` (e-mail, a cada
   15 min), `EMAIL_SEND` (3 min), `GT_STATUS_SEND` (30 min), `APPVTC` ×3 (importadores, 5/20/1440
   min), `SYNCSAT_PRODUTOS_GERA_AUTO` (diário) e quatro `PROCEDURE`
   (`VTC_P_GERAAPROVACAO`, `VTC_P_GERAAGUARDENTREGA`, `VTC_P_GERACONDPAGTO` a cada 5 min;
   `PR_ATU_STATUS_DEPTO` e `PR_ATU_FORMPRODTOTVS` diários).
4. Um agendamento tem `DtaInicio = 30/07/2999` — valor sentinela para "nunca mais".
5. `GEP_JobAgdExecLog` tem **1.092.904 linhas e nenhuma chave primária** (é heap). É o log de
   execução dos jobs, e cresceu 111.585 linhas desde o snapshot de junho.

---

## Relatórios (QlikView / QueryView)

| Arquivo | Linhas | O que é |
|---|---:|---|
| `GE_QVCons.csv` | 134 | consultas do QueryView, com `InstrSql`, arquivo `.QRP`, contador de uso e data do último uso |
| `GE_QVConsVar.csv` | 581 | variáveis das consultas |
| `GE_QVConsCol.csv` | 40 | colunas |
| `GE_QVPasta.csv` / `GE_QVPastaCons.csv` | 28 / 114 | pastas |
| `GE_QVRegra.csv` | 234 | regras |

**Leitura:** das 134 consultas, **68 têm arquivo `.QRP`** (layout de relatório em arquivo, fora do
banco — dependência externa) e **26 nunca foram usadas**. As mais usadas são
`FRM_001A` (6.552 execuções, último uso 06/11/2024), `FRM_003` (2.422), `TBA_101X` (2.259, usado
ontem), `TBA_101XX3` (777, usado hoje) e `TBA_017X` (428, usado hoje). O relatório `TBA_101XX3` é
justamente o "Carteira de Pedidos" com o defeito de filtro descrito em `REGRAS-DE-NEGOCIO.md` §2.8.

---

## Cobrança, documentos e saída

| Arquivo | Linhas | O que é |
|---|---:|---|
| `IV_CbrCriterio.csv` / `IV_CbrCriterioDef.csv` / `IV_CBRCRITERIOMSG.csv` | 122 / 231 / 3 | motor de cobrança |
| `DMN_DocTp.csv` | 108 | tipos de documento do Doc Manager |
| `IV_DoctoTipo.csv` / `IV_DoctoApl.csv` / `IV_DoctoAplUso.csv` | 3 / 6 / 3 | documentos |
| `OUT_Pessoa.csv` / `OUT_Contato.csv` / `OUT_PessoaLink.csv` / `OUT_PessoaRelacao.csv` | 1.918 / 147 / 122 / 9 | módulo OUT (marketing de saída) |
| `IVT_DePara.csv` | 12 | de/para de integração |
| `GE_IMPORTA_CART.csv` | 22 | importação de carteira |

O módulo `OUT_*` está **parado desde 05/12/2016**. O motor de cobrança (`IV_Cbr*`) parou em
**21/05/2025**.

---

## Arquivos exportados vazios (recurso existe, Tracbel não usa)

`IV_AcaoAutoCtrl`, `IV_ResultadoReq`, `IV_ResParam`, `IV_RetProcRegra`, `GE_CampoExig`,
`GEP_JOBNOTIFICAR`, `GEP_Notificar`, `GEP_ProcControle`, `GEP_ParRecEnvia`, `GE_TabRegra`.

Somados a estes, todos os 20 arquivos do módulo de **financiamento (`IVF_*`)**, os 6 do
**call center (`IVC_*`)**, os 4 de **material (`IVM_*`)**, os 4 de **produto/preço (`IVP_*`)** e os
7 do módulo **`JDE_*`** estão com **zero linhas** — o produto tem esses módulos, a Tracbel não os
ligou. Isso reduz o escopo real de substituição.

---

## Apêndice — os 116 arquivos exportados

| Arquivo | Linhas | Colunas | Colunas omitidas |
|---|---:|---:|---|
| `DMN_DocTp.csv` | 108 | 24 | — |
| `GE_Alias.csv` | 1.413 | 9 | — |
| `GE_Aplicacao.csv` | 184 | 9 | — |
| `GE_AppModulo.csv` | 30 | 3 | — |
| `GE_AtributoFixo.csv` | 183 | 9 | — |
| `GE_CampoExig.csv` | 0 | 7 | — |
| `GE_CampoPerm.csv` | 17 | 7 | — |
| `GE_Col.csv` | 217 | 6 | — |
| `GE_CONSSQL.csv` | 1 | 10 | — |
| `GE_Consulta.csv` | 2 | 28 | Senha |
| `GE_ConsultaVar.csv` | 3 | 10 | — |
| `GE_EMAILAGD.csv` | 2 | 13 | — |
| `GE_EMAILTEMPLATE.csv` | 8 | 6 | — |
| `GE_Empresa.csv` | 18 | 33 | — |
| `GE_IMPORTA_CART.csv` | 22 | 52 | — |
| `GE_Mod.csv` | 32 | 9 | — |
| `GE_Modulo.csv` | 48 | 8 | — |
| `GE_ObjDinamico.csv` | 196 | 28 | Senha |
| `GE_ObjDinAplic.csv` | 124 | 4 | — |
| `GE_ParametroGlobal.csv` | 239 | 8 | — |
| `GE_ParamLista.csv` | 405 | 25 | — |
| `GE_Permissao.csv` | 302 | 11 | — |
| `GE_PolSeg.csv` | 11 | 3 | — |
| `GE_PolSegAces.csv` | 3 | 7 | — |
| `GE_PolSegCtrl.csv` | 254 | 7 | — |
| `GE_PolSegItem.csv` | 108 | 7 | — |
| `GE_PolSegItLst.csv` | 66 | 6 | — |
| `GE_PolSegParams.csv` | 1 | 7 | — |
| `GE_POLSEGPERM.csv` | 818 | 5 | — |
| `GE_QVCons.csv` | 134 | 28 | — |
| `GE_QVConsCol.csv` | 40 | 8 | — |
| `GE_QVConsVar.csv` | 581 | 17 | — |
| `GE_QVPasta.csv` | 28 | 4 | — |
| `GE_QVPastaCons.csv` | 114 | 2 | — |
| `GE_QVRegra.csv` | 234 | 7 | — |
| `GE_Sequencia.csv` | 77 | 2 | — |
| `GE_Sistema.csv` | 11 | 3 | — |
| `GE_SyncParam.csv` | 635 | 10 | — |
| `GE_Tab.csv` | 12 | 2 | — |
| `GEP_Fila.csv` | 81 | 18 | — |
| `GEP_IMPORT_GUI.csv` | 52 | 15 | — |
| `GEP_JOBAGD.csv` | 25 | 18 | — |
| `GEP_JOBCAD.csv` | 71 | 11 | — |
| `GEP_JOBFILA.csv` | 25 | 16 | — |
| `GEP_JobMonitor.csv` | 2 | 10 | — |
| `GEP_JOBNOTIFICAR.csv` | 0 | 14 | — |
| `GEP_Notificar.csv` | 0 | 8 | — |
| `GEP_ParEnvia.csv` | 3 | 7 | — |
| `GEP_ParRecebe.csv` | 14 | 12 | — |
| `GEP_ParRecEnvia.csv` | 0 | 2 | — |
| `GEP_ProcControle.csv` | 0 | 5 | — |
| `GEP_Processo.csv` | 8 | 20 | — |
| `GEP_UsrSat.csv` | 71 | 16 | — |
| `IV_Acao.csv` | 980 | 37 | — |
| `IV_AcaoAuto.csv` | 5.958 | 31 | — |
| `IV_AcaoAutoCtrl.csv` | 0 | 6 | — |
| `IV_AcaoCtrl.csv` | 44 | 13 | — |
| `IV_AcaoRem.csv` | 266 | 9 | — |
| `IV_AtribLista.csv` | 37 | 2 | — |
| `IV_Atributo.csv` | 10 | 11 | — |
| `IV_Campanha.csv` | 179 | 33 | — |
| `IV_CbrCriterio.csv` | 122 | 26 | — |
| `IV_CbrCriterioDef.csv` | 231 | 16 | — |
| `IV_CBRCRITERIOMSG.csv` | 3 | 6 | — |
| `IV_ClasseRes.csv` | 40 | 5 | — |
| `IV_CodPrcEmpr.csv` | 17 | 2 | — |
| `IV_CodProcComent.csv` | 1 | 4 | — |
| `IV_CodProcesso.csv` | 62 | 35 | — |
| `IV_Distribui.csv` | 620 | 3 | — |
| `IV_DoctoApl.csv` | 6 | 15 | — |
| `IV_DoctoAplUso.csv` | 3 | 10 | — |
| `IV_DoctoTipo.csv` | 3 | 5 | — |
| `IV_Evento.csv` | 132 | 27 | — |
| `IV_Formulario.csv` | 176 | 16 | — |
| `IV_GlobalPar.csv` | 3.403 | 43 | — |
| `IV_GlobalParCtrl.csv` | 48 | 54 | — |
| `IV_GlobalParLista.csv` | 197 | 4 | — |
| `IV_ListSQL.csv` | 360 | 5 | — |
| `IV_Motivo.csv` | 45 | 4 | — |
| `IV_ObjFlow.csv` | 726 | 9 | — |
| `IV_OpBloq.csv` | 4 | 8 | — |
| `IV_Operador.csv` | 941 | 51 | — |
| `IV_OPTEMAIL.csv` | 21 | 7 | — |
| `IV_OPTFONE.csv` | 2 | 7 | — |
| `IV_Pcte.csv` | 1 | 3 | — |
| `IV_PcteAtrFx.csv` | 22 | 3 | — |
| `IV_ProcAcao.csv` | 740 | 3 | — |
| `IV_ProcFase.csv` | 273 | 5 | — |
| `IV_ProcPersp.csv` | 67 | 4 | — |
| `IV_ProcResultado.csv` | 2.161 | 5 | — |
| `IV_ProcSt.csv` | 250 | 4 | — |
| `IV_ProcVinc.csv` | 192 | 6 | — |
| `IV_Propriedade.csv` | 52 | 70 | — |
| `IV_Questao.csv` | 2.303 | 19 | — |
| `IV_QuestaoLista.csv` | 3.995 | 8 | — |
| `IV_ResMsgPapel.csv` | 1.338 | 28 | — |
| `IV_ResParam.csv` | 0 | 37 | — |
| `IV_Resultado.csv` | 4.209 | 63 | — |
| `IV_ResultadoReq.csv` | 0 | 6 | — |
| `IV_RetProcRegra.csv` | 0 | 7 | — |
| `IV_TxtPadrao.csv` | 212 | 11 | — |
| `IV_VENDEDOR.csv` | 351 | 18 | — |
| `IVS_CartCid.csv` | 673 | 4 | — |
| `IVS_CartDepto.csv` | 206 | 4 | — |
| `IVS_Carteira.csv` | 655 | 16 | — |
| `IVS_Depto.csv` | 29 | 29 | — |
| `IVS_DeptoEmpr.csv` | 13 | 5 | — |
| `IVS_DEPTOPOT.csv` | 126 | 8 | — |
| `IVS_DeptoRes.csv` | 63 | 7 | — |
| `IVS_Segm.csv` | 8 | 5 | — |
| `IVS_UsrMeta.csv` | 1 | 13 | — |
| `IVT_DePara.csv` | 12 | 8 | — |
| `OUT_Contato.csv` | 147 | 25 | — |
| `OUT_Pessoa.csv` | 1.918 | 61 | — |
| `OUT_PessoaLink.csv` | 122 | 4 | — |
| `OUT_PessoaRelacao.csv` | 9 | 7 | — |

Total: **116 arquivos**, **41.818 linhas**.
