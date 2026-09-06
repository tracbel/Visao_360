# Núcleo CRM/BPM do Vórtice (prefixo IV_, 377 tabelas) — motor de workflow completo

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

O motor BPM do Vórtice é um autômato de 5 peças: Processo (estado), Agenda (tarefa aberta), Histórico (fato imutável), Ação (tipo de tarefa) e Resultado (desfecho). Ao dar andamento, o Resultado escolhido dispara DOIS mecanismos independentes e desacoplados: IV_ProcResultado(CodProcesso, Resultado) muda Fase/Status do processo, e IV_AcaoAuto(Resultado) cria as próximas agendas. Nenhum dos dois conhece o outro — daí a classe inteira de defeitos "processo mudou de fase e ficou sem tarefa" (155 de 239 processos em Entrega travados assim, doc 2.3). Confirmei empiricamente a semântica real de IV_ProcResultado, não documentada: a fase aplicada é COALESCE(FaseSeguinte, Fase) e Fase é a fase de origem/pré-condição; 1.456 das 2.160 linhas (67%) têm Fase, Status e FaseSeguinte todos vazios, ou seja, só declaram "resultado permitido neste fluxo" sem transição. A regra de automação NÃO tem linguagem de condição: IV_AcaoAutoCtrl (a tabela de condições) está VAZIA e UsaObjDyn=0 em 100% das linhas preenchidas — o único discriminante real é (Resultado, NroEmpresa). Por isso o roteamento é feito por explosão cartesiana: 5.958 regras para apenas 1.262 pares (Resultado, Ação) distintos, uma cópia por filial com SeqUsuario hardcoded. Formulários geram DDL: cada IV_Formulario vira uma tabela física IV_Q_<Descricao> (175 tabelas, 2.601 colunas, 87,5k linhas) e 53 views IV$P_ por propriedade. O catálogo apodrece: 980 ações (só 169 usadas em 2026), 4.208 resultados (501 em 2026), 131 das 377 tabelas IV_ vazias (35%). Integridade referencial no caminho quente é inexistente — não há FK de IV_Agenda/IV_Historico/IV_ProcDado para IV_Processo, e há 345.535 linhas de IV_ProcDado sem IV_Processo correspondente.

## Entidades / objetos mapeados

### `IV_Processo`

**Funcao:** O CASO/oportunidade — guarda apenas o ESTADO. 22 colunas, 1.174.932 linhas (1.190.252 live).

**Campos-chave:** PK Processo numeric(18). Estado: Fase varchar(20)+FaseOrdem, Status varchar(20)+StatusDesc, Perspectiva decimal(3), Realizado+DtaRealizacao, DtaFase, DtaStatus, DtaPrevConclusao+DtaPrevConcOrig, Valor decimal(15,2), Qtde, Prioridade, UsuResponsavel

**Relacoes:** NÃO tem CodProcesso nem SeqPessoa — ambos ficam em IV_ProcDado, sem FK. Referenciada por IV_Agenda.Processo, IV_Historico.Processo, IV_Questionario.Processo, IVM_ProcMat (todas sem FK declarada exceto IVM_ProcMat)

### `IV_ProcDado`

**Funcao:** O CONTEXTO do processo (cliente, tipo de fluxo, vendedor, origem, linhagem). 24 col, 1.516.214 linhas (1.532.133 live).

**Campos-chave:** PK Processo. CodProcesso (tipo de fluxo), SeqPessoa, NroEmpresa, Vendedor, ProcessoPai, ProcessoDNA (raiz da linhagem), Motivo, Campanha, Origem, HistoricoOrigem, UltResultado/UltHistorico/DtaUltResultado, HUDecorrido, SeqDepto

**Relacoes:** FK apenas para IVS_Depto.SeqDepto. Sem FK para IV_Processo → 345.535 linhas órfãs. Alvo de FK de IV_ProcRef, IV_ProcAtiv, IV_PROCTAG, IV_PROCPESLINK, GE_PROCESSOWEB

### `IV_CodProcesso`

**Funcao:** Catálogo dos TIPOS DE FLUXO (BPM). 35 col, 62 linhas, 58 EmUso=1.

**Campos-chave:** PK CodProcesso decimal(4). Feature flags: UsaPerspectiva, UsaStatus, UsaStatusDes, UsaValor, UsaQtde, UsaProduto, UsaResumo, UsaFichCad, CriaFichaNeg, INDUSABOARD, INDFASEBASEACAO. Atalhos: AcaoProsp, AcaoAcomp, AcaoVenda, FaseEnvioERP

**Relacoes:** Pai de IV_ProcFase, IV_ProcSt, IV_ProcPersp, IV_ProcResultado, IV_ProcAcao, IV_ProcVinc, IV_CodPrcEmpr, IV_CodProcComent. Maiores: 3 Nota Fiscal (587.867 proc, sem fase), 50 Venda Equipamento Tracbel Agro (40.718), 41 Venda Maq/Impl/AMS (23.844)

### `IV_ProcResultado`

**Funcao:** A TABELA DE TRANSIÇÃO da máquina de estados. 5 col, 2.153 linhas (2.160 live) — 1.456 (67%) totalmente vazias.

**Campos-chave:** PK (CodProcesso, Resultado). Fase varchar(20) = fase de ORIGEM; Status varchar(20) = status APLICADO; FaseSeguinte varchar(20) = fase APLICADA. Fase efetiva = COALESCE(FaseSeguinte, Fase)

**Relacoes:** FK para IV_CodProcesso e IV_Resultado. Não tem FK para IV_ProcFase/IV_ProcSt — os nomes de fase/status são strings livres

### `IV_AcaoAuto`

**Funcao:** A REGRA DE AUTOMAÇÃO (gerar próxima tarefa). 31 col, 5.942 linhas (5.958 live), 5.708 ativas.

**Campos-chave:** PK SeqAcaoAuto. Gatilho: Resultado (+ResultadoCmpl). Efeito: Acao. Destino: SeqUsuario (positivo=fixo, 0=quem lançou, negativo=pseudo-papel). Filtro: NroEmpresa. Controles: Exigida S/N, SemConfirmacao S/N, Mesmoprocesso S/N, QtdeDias, QtdeHU, SeqOrder, EMUSO, QuebraDNA, EXIGESCOLHAATDGRPO. MORTAS: Operacao, GerarPara, FormaGeracao, UsaObjDyn, Pcte

**Relacoes:** FK para IV_Resultado e IV_Acao. IV_AcaoAutoCtrl (tabela de CONDIÇÕES) existe e está VAZIA — não há linguagem de condição

### `IV_Acao`

**Funcao:** Catálogo dos TIPOS DE TAREFA. 37 col, 979 linhas (980 live), 378 EmUso='S'.

**Campos-chave:** PK Acao decimal(6). EmUso S/N, Classe char(1) (O/X/T/P/W/R/?), Avulsa, PermiteExclusao, ExigeDetalhe, ExigeFormulario+SeqFormulario, QtdeMaxPessoa (limite VITALÍCIO), QtdeMaxProcesso, QtdeMaxPessoaEmp, PrazoRealizacao, TempoMedio, Instrucao. 10 flags 100% mortas

**Relacoes:** FK para IV_Formulario. Referenciada por IV_Agenda.Acao, IV_AcaoAuto, IV_AcaoCtrl, IV_AcaoRem, IV_AcaoMon, IV_AcaoAtendente, IV_ProcAcao, IV_Distribui. 744 já usadas em IV_Agenda, só 169 em 2026

### `IV_Resultado`

**Funcao:** Catálogo dos DESFECHOS. 63 col, 4.201 linhas (4.208 live), 3.665 EMUSO=1.

**Campos-chave:** PK Resultado decimal(6). Acao (a qual ação pertence), Descricao, Ordem, ADVERTENCIA, REAGENDARPARA, Seqformulario, VidaUtil, HORAPRORR*/HORAREAGENDA, + 35 colunas CTRL* com domínio 0/1/9 (9=exige, 1=habilita). Mais usadas: CTRLINTERATIVO 4.187, CTRLPRODUTIVO 3.646, CTRLMUDARATDREAG 3.481, CTRLDETALHE=9 3.414, CTRLCONCLUSAO=9 2.758

**Relacoes:** Pai de IV_ResultadoCmpl, IV_ResultadoInstr, IV_ResVinc, IV_ResClasse, IV_ResMsgPapel, IV_ResParam, IV_ResEvtOut, IV_ResultadoWeb. Gatilho de IV_AcaoAuto e IV_AcaoRem. Alvo de IV_Historico.Resultado. 2.854 já usados, só 501 em 2026

### `IV_Agenda`

**Funcao:** A TAREFA aberta/atribuída. 40 col, 931.989 linhas (930.126 realizadas / 35.724 pendentes).

**Campos-chave:** PK SeqAgenda. SeqUsuario (dono), SeqPessoa, Processo, CodProcesso, Acao, DtaAgenda/Final/Original/LimiteExecucao/Aviso, Realizada S/N, Status char(1) (C=827k concluída, P=12,8k, vazio=116k), Classe char(1) (O/X/T/P/C/W/E/R/M), TipoAgendamento (A=817k automático/M=141k manual), HistoricoOrigem, UsuGerouAcao, UltResultado, UltHistorico, Vendedor (CODVENDEDOR varchar!), HUDecorrido

**Relacoes:** FK só para IV_Acao e GE_Pessoa. Sem FK para IV_Processo, GE_Usuario nem IV_CodProcesso. Logada em IV_AgendaLog (11M linhas)

### `IV_Historico`

**Funcao:** O FATO imutável — cada andamento dado. 29 col, 2.436.127 linhas.

**Campos-chave:** PK SeqHistorico. SeqPessoa NOT NULL, Resultado decimal(6) NOT NULL, ResultadoCmpl varchar(150) (truncado em 20 na gravação normal; 917 exceções), AcaoGeradora, AgendaOrigem, Processo, CodProcesso, Detalhe varchar(4000), Natureza (A=1.428.832 ativo / R=1.054.975 receptivo / M=75), Duracao, Valor, Qtde, Latitude/Longitude, TemCiencia, Departamento, Vendedor

**Relacoes:** FK para IV_Resultado e GE_Pessoa. Extensões 1:1/1:N: IV_Interacao (720.525), IV_HistLink (645.850), IV_HistoricoNota (94.315), IV_HistInfo (17.250), IV_Ciencia (173.477)

### `IV_Formulario + IV_Questao + IV_Q_*`

**Funcao:** Motor de formulários que GERA DDL: 176 formulários, 2.300 questões, e 175 TABELAS FÍSICAS IV_Q_<Descricao> (2.601 colunas, 87.532 linhas, 32 vazias).

**Campos-chave:** IV_Formulario: SeqFormulario, Descricao varchar(20) (=sufixo da tabela física!), Layout F/S, Restricao, PrimeiraQuestao, QuestaoGuia, INDUSAASSINATURA. IV_Questao: (SeqFormulario,Questao), Nomecoluna varchar(30) (=nome da coluna física), TipoDado C/M/V/S/N/L/D/X/R/H, ProximaQuestao (ramificação), Peso. IV_Q_*: PK SEQQUESTIONARIO + 1 coluna por questão

**Relacoes:** IV_Questionario (87.929) é o cabeçalho da resposta e liga a SeqPessoa, SeqFormulario, SeqHistorico, SeqAgenda, Processo, SEQPROJETO, Resultado. Validado: form 110 'OS_GARANTIA' → IV_Q_OS_GARANTIA, 4 colunas, 7.786 linhas = 7.786 respostas

### `IV_Propriedade + IV_ClientePropr`

**Funcao:** Custom objects do cliente por SLOT FIXO. 52 definições, 246.684 instâncias. Gera 53 views IV$P_*.

**Campos-chave:** IV_Propriedade (70 col) guarda RÓTULOS em Campo1..8, Numero1..6, Data1..6, SimNao1..6, Literal1..10 + flag *Sql por slot; metadados Nivel, UmPorPessoa, ReferenciaSQL, TipoRefVinculado. IV_ClientePropr (51 col) guarda VALORES nos mesmos slots + SeqPropPessoa PK, SeqPessoa, SeqPropriedade, Referencia, Identificador, Ativo

**Relacoes:** Listas em IV_PropriLista (7.375) e IV_ListSQL (Tipo='PROPRIED', 54). Alvo de FK de EXT_VeicFam, GE_MobileConfig, GE_MOBILEPROP. Maiores: NO-MÁQUINA/IMPLEMENT 108.186, Origem da Receita 40.215, Trator 25.393

### `IV_VENDEDOR + IV_VENDEDOREMPR`

**Funcao:** O CEN/vendedor comercial e a HIERARQUIA de vendas — separado do usuário do sistema. 344 + 917 linhas.

**Campos-chave:** IV_VENDEDOR: PK SEQVENDEDOR, CODVENDEDOR varchar(20) (usado como FK em IV_Agenda.Vendedor!), SeqUsuario (opcional), SeqUsuarioLider (a hierarquia), EMUSO, SeqUnidade, EQUIPE, NROEMPRPADRAO, COMISSAO1/2. IV_VENDEDOREMPR: (SEQVENDEDOR, NroEmpresa) — habilita o CEN na filial

**Relacoes:** IVS_Carteira.SeqVendedor → IV_VENDEDOR (o campo 'CEN' da tela de carteira). Sem FK para GE_Usuario. Hierarquia paralela e desconectada de IV_Operador.Supervisor/Gerente (varchar)

### `IV_Evento`

**Funcao:** Motor de automação por EVENTO EXTERNO (ERP/webhook). 27 col, 132 linhas.

**Campos-chave:** PK SeqEvento. Origem varchar(20) (RD, PROTHEUS, SISDIA, AUDIT), Evento varchar(200) (WEBHOOK.CONVERTED, NFS/MAQ-NOVOS/N, OS, PECAFAT...), ResultAtivo/ResultRec/ResultRecPE/RESULTRECHSTE/RESULTRECHSTNE (qual RESULTADO injetar), GeraAcao, ProcessoUnico, AtuHistProcesso, ExcluiProcesso, ProcedureAdicional varchar(200) (nome de stored procedure!)

**Relacoes:** Alimenta indiretamente IV_AcaoAuto ao lançar um Resultado. IV_EventoAcao está vazia

### `IV_AcaoRem + IV_AcaoCtrl + IV_Distribui`

**Funcao:** Os outros três primitivos de automação: remoção de tarefa (266), escalação por e-mail (44), round-robin (609).

**Campos-chave:** IV_AcaoRem: Resultado, Acao (a remover), NaoTrabalhada=1, Interativo, INDREMPES. IV_AcaoCtrl: Acao, SeqCtrl, Base='A', SeqUsrBase, Minuto (60/480/960/1440), Destinatario, Atitude='EMAIL', IntervaloExec (30/1440), Papel, SeqTxtPadrao, Evento. IV_Distribui: SeqUsuario, Acao, UltimoUsuario

**Relacoes:** IV_AcaoRem FK para IV_Acao e IV_Resultado. IV_AcaoCtrl FK para IV_Acao e IV_TxtPadrao. Ex.: resultado 3449 'Venda Perdida' remove as ações 804, 863, 822, 821, 819

### `IV_ProcFaseMonit / IV_ProcStatMonit / IV_ProcPerspMonit`

**Funcao:** Linha do tempo materializada do processo — insumo pronto de lead time. 655.144 / 850.022 / 126.145 linhas.

**Campos-chave:** FaseMonit: (Processo, SeqFase) + FASE, FaseOrdem, DtaInicio, DtaFim, UltHistorico, HU (horas úteis), HN (horas naturais). StatMonit/PerspMonit: (Processo, DtaMonit) + Status/Perspectiva, HU, HN, UsuAlteracao

**Relacoes:** Sem FK para IV_Processo. ⚠️ HU vem NEGATIVO em produção (-0,60 / -0,50 / -0,40 no processo 1619541) e o mesmo processo revisita fases fora de ordem (75→26→41→75)

### `IV_Selecao + IV_SelecaoCriterio + IV_SelecaoPessoa`

**Funcao:** Motor de segmentação com DSL própria de álgebra de conjuntos. 543 / 2.865 / 582.481 linhas.

**Campos-chave:** IV_Selecao: SeqSelecao, Selecao, Dinamica (recalcula vs snapshot), QtdeClientes, Construtor. IV_SelecaoCriterio: SeqCriterio, Operacao '+'/'-', Ordem, SeqSelecaoFoco (aninhamento), InstrSQL text com DSL do tipo [Tab.Agenda].ACAO = 'X' [E] [Tab.Agenda].REALIZADA = 'N'. IV_SelecaoPessoa: SeqSelecao, SeqPessoa, Usado

**Relacoes:** FK IV_SelecaoCriterio→IV_Selecao, IV_SelecaoPessoa→IV_Selecao e GE_Pessoa. Ligada a IV_Campanha (179 linhas; as tabelas de execução IV_CampPessoa/CampPesMsg/CampVoucher estão VAZIAS)

### `IV_AgendaLog`

**Funcao:** Trilha de auditoria da agenda — a maior tabela do módulo. 9 col, 11.049.475 linhas (46% de todas as linhas IV_).

**Campos-chave:** PK SEQLOGTB. Kn1 (SeqAgenda), Tb varchar(12) ('IV_AGENDA' em 100%), Kn2 (=0, morto), Ks (='X', morto), DtaLog, Usr, CodApl varchar(30) (a TELA de origem: IVS7AGE02_AndamentoAgendaTab, MOBILELITE, IVS1AGE05_AgendaTransfere, frmPrincipal, 'Servidor de processos'), Obs varchar(1000) em TEXTO LIVRE ('Realizada de [N] p/ [S]')

**Relacoes:** Sem FK. Não é diff estruturado — não permite consultar mudanças por campo sem parsear string

## Achados

### 1. O modelo canônico das 5 entidades — o que COPIAR do Vórtice

O núcleo é enxuto e correto conceitualmente:
- IV_Processo (PK Processo numeric(18), 22 col, 1.174.932 linhas) = o CASO/oportunidade. Colunas de estado: Fase varchar(20)+FaseOrdem decimal(2), Status varchar(20)+StatusDesc varchar(150), Perspectiva decimal(3), Realizado decimal(1)+DtaRealizacao, DtaFase, DtaStatus, DtaPrevConclusao+DtaPrevConcOrig (baseline vs atual!), Valor decimal(15,2), Qtde, Prioridade, UsuResponsavel.
- IV_Agenda (PK SeqAgenda, 40 col, 931.989) = a TAREFA pendente de um usuário. SeqUsuario (dono), SeqPessoa, Processo, CodProcesso, Acao, DtaAgenda/DtaAgendaFinal/DtaLimiteExecucao/DtaAviso/DtaAgendaOriginal, Realizada char(1) S/N (930.126 S / 35.724 N), UltResultado, UltHistorico, HistoricoOrigem (qual histórico a criou), UsuGerouAcao, DtaGeracao/DtaGeracaoOrig, HUDecorrido decimal(8,2) = horas úteis decorridas.
- IV_Historico (PK SeqHistorico, 29 col, 2.436.127) = o FATO imutável. Resultado decimal(6) NOT NULL, ResultadoCmpl varchar(150), AcaoGeradora, AgendaOrigem, Detalhe varchar(4000), Natureza char(1) (A=ativo 1.428.832 / R=receptivo 1.054.975 / M=75), Duracao, Latitude/Longitude numeric(14,11) (geo do atendimento em campo), TemCiencia.
- IV_Acao (PK Acao decimal(6), 37 col, 980) = o TIPO de tarefa.
- IV_Resultado (PK Resultado decimal(6), 63 col, 4.208) = o DESFECHO. Resultado pertence a uma Ação (coluna Acao; 3.992 têm, 216 são órfãos/genéricos).
O encadeamento é bidirecional e auditável: Agenda.HistoricoOrigem -> Historico.SeqHistorico e Historico.AgendaOrigem -> Agenda.SeqAgenda. Medido: 684.110 de 931.989 agendas nasceram de um histórico (73%), 281.740 foram criadas à mão; 1.423.965 de 2.436.127 históricos vieram de uma agenda (58%), 1.059.917 foram lançamentos avulsos.

**Evidencia:** schema/colunas.csv; query live: SELECT COUNT(*) ... IV_Agenda WHERE HistoricoOrigem IS NOT NULL / IV_Historico WHERE AgendaOrigem IS NOT NULL; GROUP BY Natureza

**Licao para a Tracbel:** COPIAR integralmente essa separação de 5 entidades — é o acerto central do Vórtice e vale mais que qualquer feature. Traduzindo: Case (agregado com estado), Task (unidade de trabalho atribuível), Event/Activity (log append-only), TaskType (catálogo) e Outcome (catálogo de desfechos ligado ao TaskType). Manter o duplo ponteiro (task.created_from_event_id e event.completed_task_id) — é ele que permite responder 'quem gerou essa tarefa e por quê' em uma query, o que resolveu os casos A e B do doc de regras. Copiar também DtaPrevConclusao vs DtaPrevConcOrig (previsão atual vs baseline original), Natureza ativo/receptivo e geolocalização do atendimento.

### 2. Como uma regra de automação é declarada — IV_AcaoAuto (31 colunas, 5.958 linhas)

Uma regra é uma linha em IV_AcaoAuto e lê-se: "quando o Resultado R for lançado, crie uma agenda da Ação A para o usuário U". Colunas que realmente governam:
- SeqAcaoAuto numeric(18) PK
- Resultado decimal(6) NOT NULL — o GATILHO (único). FK -> IV_Resultado
- ResultadoCmpl varchar(150) — refina o gatilho pelo complemento do resultado
- Acao decimal(6) — o que criar. FK -> IV_Acao
- SeqUsuario numeric(18) — PARA QUEM (ver achado seguinte)
- NroEmpresa numeric(6) — filial em que a regra vale (4.529 preenchidas, 1.429 nulas = vale para todas)
- Exigida char(1) NOT NULL: S=5.336 / N=622 — se S, a agenda é criada obrigatoriamente
- SemConfirmacao char(1): N=4.885 / S=1.073 — S cria sem perguntar ao usuário; N abre diálogo
- Mesmoprocesso char(1): S=5.693 / N=265 — a nova agenda fica no mesmo processo ou abre processo novo
- QtdeDias numeric(4) NOT NULL — offset da data da agenda; 5.886 de 5.958 são ZERO (o prazo real vem da Ação, não da regra)
- QtdeHU decimal(8,2) — offset em horas úteis
- Prioridade decimal(1), Departamento varchar(20), TipoAgendamento char(1) (A=5.388 automático / C=570), AtivoReceptivo char(1) (A em 100%)
- SeqOrder decimal(4) — ordem de execução das regras do mesmo resultado
- EMUSO numeric(1): 1=5.708 / 0=234 / null=16 — 0 é regra DESLIGADA (é o caso das 17 regras da ação 846 'Solicitar Entrega Física FY25')
- QuebraDNA numeric(1): 1 em apenas 41 regras — quebra a linhagem do processo
- EXIGESCOLHAATDGRPO numeric(1): 1=2.200 — exige escolher atendente dentro do grupo
- INDGERAPARAPESPRC numeric(1): 1 em apenas 5 regras
COLUNAS MORTAS (não usadas em nenhuma linha): Operacao varchar(2) (5.940 null), GerarPara char(1) (5.949 null), FormaGeracao char(1) (5.940 null), UsaObjDyn (0 ou null em 100%), Pcte (null em 100%), PROPAGAFORMTRAB (0 ou null), SOLICDATAAGD (0 ou null), HORAAGENDA, CHKSUM.

**Evidencia:** Query live: GROUP BY de cada coluna em IV_AcaoAuto WITH(NOLOCK); schema/colunas.csv

**Licao para a Tracbel:** COPIAR a ideia (regra declarativa gatilho->efeito, versionada em tabela, com flag de desligamento). EVITAR o formato: 31 colunas das quais 9 estão 100% mortas, e o gatilho é uma coluna única (Resultado). ADAPTAR para: rule { id, trigger: {outcome_id, outcome_detail?}, condition: <expressão>, effect: {create_task | close_task | set_state | notify}, assignee: <expressão de papel>, due: <offset em dias úteis>, enabled, priority, order }. A condição precisa ser uma EXPRESSÃO avaliada (CEL/JSONLogic), não uma coluna NroEmpresa.

### 3. 🔴 A regra de automação NÃO tem linguagem de condição — e é isso que causa a explosão de 5.958 regras

Existem exatamente dois lugares onde uma condição poderia morar, e ambos estão inertes:
1. IV_AcaoAutoCtrl (SeqAcaoAutoCtrl, Tipo varchar(10), CtrlNum, CtrlNum2, CtrlStr varchar(40), SeqAcaoAuto) — a tabela de condições da regra. Contagem live: 0 LINHAS.
2. IV_AcaoAuto.UsaObjDyn — objeto dinâmico como condição: 2.747 linhas com 0, 3.211 com NULL. ZERO linhas com 1.
Portanto o único discriminante efetivo é o par (Resultado, NroEmpresa). Consequência medida: 5.958 regras cobrem apenas 1.262 pares (Resultado, Ação) distintos e 1.037 resultados-gatilho — média de 5,75 regras por resultado. O padrão é literalmente uma cópia por filial:
- resultado 3440 'Faturamento Realizado' -> 38 regras, sendo 16 idênticas para a ação 814 (uma por NroEmpresa 1..18, com SeqUsuario diferente) e 16 para a ação 846 (todas com EMUSO=0, desligadas);
- resultado 3668 'Alterado Forma de Pagamento' -> 16 regras iguais para a ação 769, SeqUsuario 951..966, NroEmpresa 1..18;
- pior caso: resultado 3329 com 96 regras / 6 ações / 16 empresas; resultado 2984 com 80 regras para UMA ação em 16 empresas.
Manutenção: trocar o aprovador de uma filial exige achar e editar N linhas espalhadas; esquecer uma deixa a agenda indo para quem saiu da empresa (é exatamente o defeito 2.5 do doc — 'aprovação vai para o líder do CEN no momento da geração e não se reatribui').

**Evidencia:** Query live: SELECT COUNT(*) FROM IV_AcaoAutoCtrl = 0; GROUP BY UsaObjDyn; SELECT Resultado, COUNT(*), COUNT(DISTINCT Acao), COUNT(DISTINCT NroEmpresa) FROM IV_AcaoAuto GROUP BY Resultado ORDER BY 2 DESC; SELECT COUNT(DISTINCT Resultado)=1037, COUNT(*)=5958, COUNT(DISTINCT Resultado+'|'+Acao)=1262

**Licao para a Tracbel:** EVITAR a todo custo. No CRM próprio, a regra deve ter (a) condição como expressão avaliada em runtime contra o contexto {processo, pessoa, filial, produto, valor, carteira, usuário}, e (b) destinatário como EXPRESSÃO DE PAPEL resolvida no momento da geração (ex.: 'lider_da_carteira(processo.carteira)', 'responsavel_por(processo.filial, papel=ADM_FINANCEIRO)'), nunca um ID de usuário gravado. Isso colapsa as 5.958 linhas do Vórtice em ~1.262 regras e elimina de uma vez os defeitos 2.1, 2.5 e 2.6 do documento de regras. Regra prática: se a mesma regra precisa ser duplicada por filial, o modelo está errado.

### 4. Para QUEM a tarefa é gerada: IV_AcaoAuto.SeqUsuario com códigos mágicos negativos não documentados

Três regimes coexistem na mesma coluna numérica:
- SeqUsuario = número positivo (5.491 regras): usuário FIXO hardcoded. Ex.: regra 9473, resultado 3784 'Chassi Aprovado' -> ação 769 sempre para o usuário 945 (150 ocorrências desde 10/04/2026).
- SeqUsuario = 0 (119 regras, 47 ações): vai para QUEM LANÇOU o andamento. É o caso das regras 9067/9068 (resultados 3236 'Desistiu da Compra' / 3237 'Venda Cancelada' -> ação 767).
- SeqUsuario < 0 (141 regras): PSEUDO-PAPEL resolvido em runtime. Não existe GE_Usuario com Seq <= 0 (query confirmou 0 linhas). Códigos encontrados: -2 (2 regras, ação 528 AUTORIZAR VENDA), -3 (7, ação 871 Aprovar Demonstração FY26), -4 (3), -5 (61 regras, 18 ações), -6 (2), -7 (3), -12 (10), -13 (1), -14 (4), -15 (48 regras, 16 ações).
VALIDAÇÃO EMPÍRICA que fiz: para a ação 767 (regras com SeqUsuario -15/-12/0), das 200 agendas geradas desde 01/06/2026, 137 foram para o próprio vendedor do processo (IV_Agenda.SeqUsuario = IV_VENDEDOR.SeqUsuario do CODVENDEDOR da agenda) e nenhuma para o líder. Para a ação 900 (regra 9535, SeqUsuario = -5), as agendas de ago/2026 foram todas para o vendedor/CEN do processo (EDUARDO.PRADO, RAFAEL.LADEIA, BEATRIZ.MARINHO, JULIA.COSTA), com UsuGerouAcao sendo quem lançou o faturamento (RENATA.MARTINS, LAYNE.SOUZA).
O mesmo esquema aparece em IV_AcaoCtrl.Destinatario (valor -7 em 2 linhas) e em IV_ResMsgPapel.Papel (valor '-5' em 52 linhas).

**Evidencia:** Query live: SELECT SeqUsuario, COUNT(*) FROM IV_AcaoAuto WHERE SeqUsuario<=0 GROUP BY SeqUsuario; SELECT ... FROM GE_Usuario WHERE SeqUsuario<=0 (0 linhas); join IV_Agenda x IV_VENDEDOR por CODVENDEDOR para acoes 767/900

**Licao para a Tracbel:** EVITAR números mágicos negativos em coluna de FK — é a pior parte do desenho: intransparente, indocumentado, impossível de auditar e não valida nada. ADAPTAR para um campo tipado: assignee = { kind: 'user'|'role_expr'|'actor_who_completed'|'round_robin_pool', value: ... }. E registrar SEMPRE no evento gerado quem foi resolvido e por qual expressão (assignment_reason), para que 'apareceu agenda que não é minha' seja respondível em 1 query sem engenharia reversa.

### 5. 🔴 Semântica REAL de IV_ProcResultado — descoberta e validada por query, contradiz a leitura ingênua

IV_ProcResultado tem só 5 colunas: (CodProcesso decimal(4), Resultado decimal(6)) PK, Fase varchar(20), Status varchar(20), FaseSeguinte varchar(20). 2.160 linhas.
A semântica verdadeira, que confirmei medindo o estado real dos processos:
- STATUS aplicado ao processo = coluna Status.
- FASE aplicada ao processo = COALESCE(FaseSeguinte, Fase). A coluna Fase é a fase de ORIGEM/pré-condição, não o destino.
Provas (fluxo 50, últimos históricos desde 01/06/2026, estado atual de IV_Processo):
- Resultado 3440 'Faturamento Realizado': parametrizado Fase=Faturamento, Status=FATURADO, FaseSeguinte=Recebimento -> 398 processos ficaram em Fase=Recebimento/Status=FATURADO. Zero em 'Faturamento'.
- Resultado 3466 'Recebimento Realizado': Fase=Recebimento, Status=FATURADO, FaseSeguinte=Entrega -> 162 processos em Fase=Entrega/FATURADO.
- Resultado 3236 'Desistiu da Compra': Fase=Negociação, Status=DESISTIU DA COMPRA, FaseSeguinte=Monitoramento -> 313 em Fase=Monitoramento.
- Resultado 3438 'Atividade Cancelada' (ação 807): Fase=Finalizado, Status=CANCELADO, FaseSeguinte=VAZIO -> 12 processos em Finalizado/CANCELADO (o defeito 2.2 do doc).
- Resultado 3784 'Chassi Aprovado': linha existe mas Fase, Status e FaseSeguinte TODOS vazios -> os 153 processos ficaram cada um onde estavam (103 Recebimento/FATURADO, 42 Faturamento/PEDIDO REALIZADO, etc.). Confirma que linha vazia = 'resultado permitido, sem transição'.
A MEDIDA MAIS IMPORTANTE: das 2.160 linhas, 1.456 (67,4%) têm Fase, Status e FaseSeguinte TODOS vazios. No fluxo 50: 280 linhas, 234 sem FaseSeguinte, 183 sem Fase, 160 sem Status. Ou seja, a tabela acumula duas responsabilidades incompatíveis — 'este resultado é válido neste fluxo' (autorização) e 'este resultado muda o estado assim' (transição) — e a maioria só serve para a primeira.

**Evidencia:** Query live: CTE com ROW_NUMBER por Processo sobre IV_Historico (CodProcesso=50, DtaRealizacao>='2026-06-01') juntada a IV_Processo, agrupada por Resultado/Fase/Status; SELECT SUM(CASE WHEN Fase='' AND Status='' AND FaseSeguinte='' ...) FROM IV_ProcResultado

**Licao para a Tracbel:** EVITAR: (1) usar strings livres varchar(20) como identidade de estado — Fase e Status são texto, com variantes reais na base ('CREDITO NAO APROVADO' sem acento vs 'PEDIDO NÃO APROVADO' com), o que quebra filtros de relatório (é a raiz do defeito 2.8); (2) sobrecarregar uma tabela com autorização e transição; (3) semântica implícita de COALESCE que ninguém documentou.
ADAPTAR para uma máquina de estados explícita: transition { flow_id, from_state_id (obrigatório e validado), outcome_id, to_state_id (obrigatório), guard?, side_effects[] }, com state_id sendo FK para uma tabela de estados. Estado terminal deve ser um ATRIBUTO do estado (is_terminal, is_won, is_lost), nunca uma lista de strings hardcoded no SQL do relatório. E o motor deve REJEITAR uma transição cuja from_state não bate, em vez de aplicar silenciosamente.

### 6. Parametrização de fase, status, marco e perspectiva — 4 tabelas planas por tipo de fluxo

IV_CodProcesso (PK CodProcesso decimal(4), 35 col, 62 linhas, 58 EmUso=1) é o TIPO DE FLUXO. Colunas de capacidade (feature flags do fluxo): UsaPerspectiva, UsaPercConclusao, UsaValor, UsaMaterial, UsaStatus, UsaStatusDes, UsaResumo, UsaFichCad, UsaQtde, UsaProduto, CriaFichaNeg, INDUSABOARD (kanban), INDFASEBASEACAO. Rótulos customizáveis: DescValor, DescPersp, DescQtde. Atalhos: AcaoProsp, AcaoAcomp, AcaoVenda (ação padrão de cada etapa), FaseEnvioERP (a partir de qual fase manda para o ERP), ProdFamilia varchar(250), CorLinha, QTDPRODUTO/QTDPRODUTOUND.
As 4 tabelas de parametrização, todas chaveadas por CodProcesso:
- IV_ProcFase (CodProcesso, Fase) + FaseOrdem decimal(2), Padrao numeric(1), Marco varchar(20). 273 linhas totais. Fluxo 50: 21 fases (ordem 0 Qualificação, 1 Monitoramento, 5 Apresentação [Padrao=1], 10 Negociação, 13 Demonstração, 14 Visita à Fabrica, 15 Cliente Referência, 16 Proposta Apresentada, 17 Pedido de Venda, 19 Montagem, 26 Análise, 27 Aprovação, 41 Formalização, 42 Autorização, 46 Registrando Cédula, 75 Faturamento, 81 Recebimento, 85 Preparação, 90 Entrega, 95 Pós-Entrega, 99 Finalizado). MARCO é o agrupamento macro para SLA/funil: '1. Negoc - Pedido' (fases 5-17), '2. Ped - Montagem' (19), '3. Montagem - Aprov' (26-27), '4. Aprov - AF' (41-46), '5.AF - Finalizado' (75-99).
- IV_ProcSt (CodProcesso, Status) + StatusOrdem, Padrao. 250 linhas. Fluxo 50: 15 status (5 EM ABERTO [Padrao=1], 8 EM ANDAMENTO, 20 PENDENTE, 45 FAT AUTORIZADO, 48 FATURADO, 50 PEDIDO REALIZADO, 60 VENDA REALIZADA, 65 CREDITO APROVADO, 70 VENDA PERDIDA, 75 DESISTIU DA COMPRA, 80 DEVOLVIDO, 90 CANCELADO, 93 CONCLUÍDO, 95 PEDIDO NÃO APROVADO, 99 CREDITO NAO APROVADO).
- IV_ProcPersp (CodProcesso, PerspOrdem) + Perspectiva varchar(20), QtdDiaPro decimal(6,2). 67 linhas. Fluxos 50 e 41: 10=Frio (60 dias), 50=Morno (30), 90=Quente (15). QtdDiaPro = prazo esperado de conversão por temperatura.
- IV_ProcAcao (CodProcesso, Acao) + QtdeLimite decimal(2). 738 linhas — quais ações são permitidas no fluxo. ⚠️ QtdeLimite é NULL nas 740 linhas: o limite por fluxo está declarado mas NUNCA foi usado (bate com o 'já descartado QtdeLimite' do doc 2.3).
- IV_ProcVinc (CodProcesso, SeqVinc) + Vinculo varchar(5), Exigido, NroVinc, CodVinc. 190 linhas, TODAS com Vinculo='FORM' — só formulários. Fluxo 50: 21 formulários anexáveis (NroVinc 117,134,141..152,156..161,168,169), todos Exigido=0.
Fase e Status são strings livres denormalizadas em IV_Processo.Fase/Status — não há FK.
Distribuição real de parametrização (contagem live): só 6 fluxos têm mais de 15 fases (50, 41, 7, 6, 52, 14). 22 dos 62 tipos têm ZERO fases e ZERO status parametrizados, incluindo o CodProcesso 3 'Nota Fiscal' que sozinho tem 587.867 processos — ou seja, metade da base de 'processos' não é oportunidade, é registro de NF sem fluxo nenhum.

**Evidencia:** Query live: SELECT cp.*, subselects de contagem em IV_ProcFase/ProcSt/ProcPersp/ProcResultado/ProcAcao/ProcVinc/ProcDado FROM IV_CodProcesso ORDER BY qProc DESC; SELECT FaseOrdem,Fase,Padrao,Marco FROM IV_ProcFase WHERE CodProcesso=50

**Licao para a Tracbel:** COPIAR: (a) o conceito de MARCO agrupando fases — é o que permite SLA e funil sem reescrever relatório a cada fase nova; (b) a Perspectiva com prazo esperado (QtdDiaPro) por temperatura; (c) fase e status como DIMENSÕES ORTOGONAIS (onde está x como está) — muitos CRMs colapsam isso num campo só e perdem expressividade; (d) feature flags por tipo de fluxo (UsaValor, UsaProduto, UsaPerspectiva) permitindo um mesmo motor servir venda, garantia e cobrança.
EVITAR: fase/status como varchar livre denormalizado; QtdeLimite morto; misturar 'Nota Fiscal' (587k registros sem fluxo) na mesma tabela das oportunidades reais — isso destrói qualquer métrica de pipeline e obriga todo relatório a filtrar por tipo.

### 7. IV_Processo NÃO tem CodProcesso nem SeqPessoa — o split Processo/ProcDado e 345.535 órfãos

O agregado 'processo' está partido em duas tabelas sem FK entre elas:
- IV_Processo (22 col): só o ESTADO (Fase, Status, Perspectiva, Valor, Qtde, datas, UsuResponsavel). NÃO tem CodProcesso (o tipo de fluxo) e NÃO tem SeqPessoa (o cliente!).
- IV_ProcDado (24 col, 1.516.214 linhas): o CONTEXTO. CodProcesso, SeqPessoa, NroEmpresa, Vendedor, PessoaDepto, SeqDepto, SeqProjeto, HistoricoOrigem, FormaPrimCont, AtivoReceptivo, Motivo varchar(40), Campanha varchar(60), Origem, UltResultado, UltHistorico, DtaUltResultado, DtaPrimResultado, HUDecorrido, ResultadoCmpl, DtaGeracao, UsuGeracao, e o par ProcessoPai / ProcessoDNA.
Para saber de qual CLIENTE é um processo, ou de qual FLUXO ele é, é obrigatório juntar as duas — e não existe FK. Medição live:
- IV_Processo: 1.190.252 linhas
- IV_ProcDado: 1.532.133 linhas
- IV_ProcDado SEM IV_Processo correspondente: 345.535 (22,6%!)
- IV_Processo SEM IV_ProcDado: 3.654
ÁRVORE/LINHAGEM: ProcessoPai (1.532.015 preenchidos) = processo que originou este; ProcessoDNA (1.532.086 preenchidos) = raiz da linhagem, sendo 1.283.533 iguais ao próprio Processo (são raízes) e ~248 mil herdados de um ancestral. IV_AcaoAuto.QuebraDNA=1 (41 regras) é o que inicia uma linhagem nova. É um modelo de 'processo que se desdobra em subprocessos mantendo rastro da origem comercial' — conceitualmente muito bom.
O tipo de fluxo (CodProcesso) fica REPLICADO em IV_ProcDado, IV_Agenda e IV_Historico, sem fonte única — é exatamente por isso que a tela IVS1AGE00_AgendaDetalhe consegue 'trocar o tipo de 50 para 41' reescrevendo o CodProcesso de todo o histórico (defeito 1.4 do doc).

**Evidencia:** Query live: SELECT (SELECT COUNT(*) FROM IV_Processo)=1190252, (SELECT COUNT(*) FROM IV_ProcDado)=1532133, órfãos via NOT EXISTS = 345535 e 3654; SELECT SUM(CASE ProcessoDNA=Processo...) FROM IV_ProcDado; schema/fks.csv não contém FK IV_ProcDado.Processo -> IV_Processo

**Licao para a Tracbel:** EVITAR o split. No CRM próprio, UMA tabela process/opportunity com flow_type_id NOT NULL FK, person_id NOT NULL FK e state_id NOT NULL FK. O tipo de fluxo é IMUTÁVEL após a criação (ou muda por migração explícita, versionada, nunca por UPDATE de tela).
COPIAR: o par parent_process_id + root_process_id (o 'DNA') — dá rastreabilidade de campanha/origem através de desdobramentos e é barato. COPIAR também a materialização de UltResultado/UltHistorico/DtaUltResultado/HUDecorrido no agregado (é cache de leitura legítimo), mas mantê-lo em colunas do próprio processo, não em tabela satélite.
E FKs de verdade com ON DELETE RESTRICT: 345 mil linhas órfãs em produção é o custo de não ter.

### 8. Dados dinâmicos do processo: 4 mecanismos diferentes, nenhum deles um campo JSON

O Vórtice guarda o que não cabe no modelo fixo de quatro maneiras distintas:
1. IV_ProcLink (7 col, 602.150 linhas) — link POLIMÓRFICO para documentos do ERP: LinkDocto varchar(20) + LinkNro numeric(18) + LinkSerie varchar(250) + LinkNroEmpresa. Domínio real medido: NFS EXT 188.737, NFS 188.453, NFS_SPRESS 135.136, OS 69.074, OS_SPRESS 19.942, PDVEIC 502, LNK_GRUPO 134, EVENTO 96, AUDIT 51, LNK_USOGEN 13, VTC_EMAIL 11, IDNFSEXTERNO 1. Existe o gêmeo IV_HistLink (5 col, 645.850) para o histórico e IV_AgdLink (11 col, 4.534) para a agenda.
2. IV_ProcRef (5 col, 61.975 linhas) — referência tipada: Tipo varchar(15) + Referencia varchar(40). Domínio real: 100% 'Chassi'. É o vínculo processo<->número de série do equipamento.
3. IV_ProcProduto (18 col, 62.578) — itens do processo: SeqProduto, CodProduto, Valor, Qtde, DESCONTO, TIPOPGTO, STATUS + ATRIBUTO01..ATRIBUTO08 varchar(40) (8 slots genéricos sem rótulo!).
4. IV_ProcDocto (5 col, 73.646) — vínculo com o Doc Manager (DMN_Doc). ⚠️ SEM NENHUMA FK no SQL Server: 5.302 vínculos órfãos (6,9%) em 2.635 processos, e é isso que aborta o sincronismo do Vórtico Mobile (defeito 2.13).
Não existe nenhuma coluna JSON/XML no núcleo IV_. Tudo é coluna física ou slot numerado.

**Evidencia:** Query live: GROUP BY LinkDocto FROM IV_ProcLink; GROUP BY Tipo FROM IV_ProcRef; schema/colunas.csv para IV_ProcProduto/IV_ProcDocto; docs/REGRAS-DE-NEGOCIO.md 2.13

**Licao para a Tracbel:** ADAPTAR: um único mecanismo de vínculo externo tipado — external_ref { process_id, ref_type (enum: erp_invoice|erp_service_order|chassis|document|email), ref_key (jsonb com a chave composta), company_id } — com índice GIN e FK real onde o alvo é interno. Isso substitui IV_ProcLink + IV_ProcRef + IV_ProcDocto + IV_HistLink + IV_AgdLink de uma vez.
EVITAR ATRIBUTO01..08 sem rótulo em IV_ProcProduto — é dado que ninguém consegue interpretar 3 anos depois. Para itens, usar colunas nomeadas + um jsonb attrs validado por JSON Schema do tipo de produto.
E FK obrigatória em vínculo de documento: o custo de não ter é o app mobile inteiro parado.

### 9. 🔴 Formulários geram DDL: 175 tabelas físicas IV_Q_* e 53 views IV$P_* criadas em runtime

O motor de formulários é composto por:
- IV_Formulario (16 col, 176 linhas, 171 EmUso='S'): SeqFormulario, Descricao varchar(20), Objetivo varchar(40), Script varchar(500), PrimeiraQuestao, QuestaoGuia, Layout char(1) (F=146 formulário / S=30 script), Restricao char(1) (N=124 / R=39), IndUsoPessoa (126 sim), INDUSOPROJETO, INDUSOPROPRIEDADE, INDUMPORPESSOA, INDUSAASSINATURA (só 4), UsaObs.
- IV_Questao (19 col, 2.303 linhas): (SeqFormulario, Questao) PK, Descricao varchar(60), Nomecoluna varchar(30), Grupo, TipoDado char(1), Tamanho, NumMinimo/NumMaximo, QTDEDECIMAL, Peso, ProximaQuestao (ramificação condicional!), Script varchar(500), Prefixoresposta, ExigeResposta, EmUso.
  Domínio de TipoDado medido: C=653 (caractere), M=449 (múltipla escolha), V=255, S=251, N=223 (numérico), L=217 (lista), D=211 (data), X=31, R=11, H=2.
- IV_QuestaoLista (8 col, 3.995): as opções de cada questão, com ProximaQuestao própria por opção (roteamento por resposta) e Peso (scoring).
- IV_Questionario (18 col, 87.929 linhas): o CABEÇALHO da resposta — SeqQuestionario, SeqPessoa, SeqFormulario, SeqHistorico, SeqAgenda, Processo, SubProcesso, SEQPROJETO, Resultado, DtaRealizacao, Obs varchar(4000), Link*.
O ACHADO: as RESPOSTAS não ficam em tabela genérica. Cada formulário tem uma TABELA FÍSICA PRÓPRIA chamada IV_Q_<IV_Formulario.Descricao>, com uma coluna por questão nomeada exatamente como IV_Questao.Nomecoluna e PK SEQQUESTIONARIO. Validei par a par:
- Formulário 110 Descricao='OS_GARANTIA', 4 questões (NUMERO_OS C(30), DATA_ABERTURA_OS D, CHASSI C(30), TIPO_GARANTIA M(40)) -> tabela IV_Q_OS_GARANTIA com exatamente SEQQUESTIONARIO + essas 4 colunas nos tipos varchar(30)/datetime/varchar(30)/varchar(40). 7.786 linhas na tabela = 7.786 respostas em IV_Questionario. Casamento exato.
- Formulário 27 'COMISSAO' (32 questões) -> IV_Q_COMISSAO, 36 colunas, 7.165 linhas = 7.165 respostas.
ESCALA DO ESTRAGO: 175 tabelas IV_Q_* (46% de todas as 377 tabelas IV_), 2.601 colunas, 87.532 linhas totais — média de 500 linhas por tabela. 32 delas estão VAZIAS. A maior tem 7.786 linhas. Vinte e uma têm 1 linha ou menos. Extremos: IV_Q_ACOMPANH_VENDA_JDE com 104 COLUNAS para 6.343 linhas; IV_Q_ACOMP_VENDA_DIRETAJD com 94 colunas para 270 linhas.
O mesmo padrão contamina as views: das 411 views do banco, 324 começam com 'IV$', sendo 53 do tipo IV$P_<NOME_DA_PROPRIEDADE> (uma view por linha de IV_Propriedade!), 48 do tipo IV$PG_<lista> e 8 IV$A_<atributo>.
Existe ainda IV_RESULTADO_3110 (74 colunas, 2.826 linhas) — uma tabela de formulário disfarçada de tabela de resultado.

**Evidencia:** schema/tabelas.csv (grep '^"IV_Q_' = 175 tabelas, soma linhas 87.532, soma colunas 2.601); schema/colunas.csv para IV_Q_OS_GARANTIA e IV_Q_VENDA; query live SELECT Questao,Nomecoluna,TipoDado,Tamanho FROM IV_Questao WHERE SeqFormulario=110; contagem de respostas por formulário via subselect em IV_Questionario; schema/views.csv

**Licao para a Tracbel:** EVITAR ABSOLUTAMENTE. DDL em runtime significa: migração de banco impossível de versionar, ORM inútil, nenhuma constraint real (todas as colunas são NULL), impossível consultar 'todas as respostas de X' sem SQL dinâmico, e 175 tabelas para 87 mil linhas.
ADAPTAR: form_definition (jsonb com o schema das questões, versionado — form_version_id), form_response { id, form_version_id, process_id, event_id, person_id, answers jsonb } com validação por JSON Schema na aplicação e índices GIN/expressão só nos campos realmente consultados. Se algum formulário virar relatório pesado, materializar UMA view por demanda — decisão de performance, não default de arquitetura.
COPIAR do Vórtice: (a) ProximaQuestao em IV_Questao e em IV_QuestaoLista — ramificação condicional por resposta, que é um requisito real da operação (formulários de Venda Perdida por tipo de equipamento); (b) Peso por opção (scoring/qualificação de lead); (c) o vínculo do questionário simultaneamente a pessoa, processo, agenda E histórico.

### 10. Objetos customizados: o padrão 'slot fixo + tabela de rótulos', repetido 3 vezes

O Vórtice tem um idioma próprio para campos customizáveis, aplicado a três domínios diferentes:
1. PROPRIEDADES (o custom object do cliente). IV_Propriedade (70 colunas, 52 linhas) é a DEFINIÇÃO: guarda os RÓTULOS nas colunas Campo1..Campo8, Numero1..Numero6, Data1..Data6, SimNao1..SimNao6, Literal1..Literal10 — mais um flag *Sql por slot (Campo1Sql, Literal1Sql...) dizendo se a lista de valores vem de SQL. Metadados: Propriedade varchar(20), Nivel, UmPorPessoa, MostraRef, Usacomplemento, TipoRefVinculado, ReferenciaSQL. IV_ClientePropr (51 colunas, 246.684 linhas) é a INSTÂNCIA, com exatamente os mesmos slots tipados (Campo1..8 varchar(40), Numero1..6 decimal(15,2), Data1..6 datetime, SimNao1..6 numeric(1), Literal1..10 varchar(40)) + SeqPropPessoa PK, SeqPessoa, SeqPropriedade, Referencia, Identificador, Ativo. IV_PropriLista (7.375) traz as listas de valores.
   Uso real: 108.186 instâncias de 'NO-MÁQUINA/IMPLEMENT', 40.215 'Origem da Receita', 25.393 'Trator', 19.709 'NO-Origem da Receit+', 9.242 'Equipamento(Sisdia)', 7.118 'CNAE', 7.117 'NJUR'. Note a proliferação de duplicatas versionadas à mão: 'Trator' (25.393) convive com 'NO-Trator+' (6.452) e 'NO-TRATOR(SISDIA)' (4.445); 'Implemento' com 'NO-Implemento+' e 'NO-IMPLEMENTOS'. 12 das 52 propriedades têm ZERO instâncias.
2. PARÂMETROS GLOBAIS. IV_GlobalPar (43 colunas, 3.848 linhas) usa os MESMOS slots (Campo1..6, Numero1..6, Data1..6, Literal1..10 varchar(100), SimNao1..10), com IV_GlobalParCtrl (54 col, 48 linhas) como tabela de rótulos e IV_GlobalParLista (197) como listas.
3. ATRIBUTOS. IV_Atributo (11 col, 10 linhas) + IV_AtribLista (37) + IV_ClienteAtrib (6 col, 2.018).
As listas de valores desses slots vêm de IV_ListSQL (5 col, 360 linhas: Tipo varchar(10), SeqMain, SeqSub, Sql text) — Tipo = QUESTAO (278), PROPRIED (54), GLBPAR (28). Ou seja, o combo de uma questão/propriedade é um SELECT armazenado em text.

**Evidencia:** schema/colunas.csv para IV_Propriedade / IV_ClientePropr / IV_GlobalPar; query live SELECT p.*, subselect COUNT em IV_ClientePropr FROM IV_Propriedade ORDER BY instancias DESC; GROUP BY Tipo FROM IV_ListSQL

**Licao para a Tracbel:** EVITAR o slot fixo: teto rígido (8 textos, 6 números, 6 datas, 10 literais), colunas sem tipo semântico, e obriga uma view IV$P_* por objeto só para dar nome aos campos (53 views geradas). E EVITAR o SQL guardado em text (IV_ListSQL) como fonte de combo — é acoplamento ao schema e superfície de injeção.
ADAPTAR: custom_object_type (definição jsonb versionada) + custom_object_instance (person_id, type_id, attrs jsonb) com validação por JSON Schema; listas de valores como tabela de referência ou enum, nunca SQL livre.
COPIAR: a ideia de UmPorPessoa (cardinalidade declarada) e de Identificador/Referencia (chave natural da instância) — são bons. E COPIAR a lição operacional: sem governança, o catálogo duplica ('Trator', 'NO-Trator+', 'NO-TRATOR(SISDIA)'). Prever versionamento e deprecação no próprio modelo.

### 11. O catálogo de Ação: 37 colunas, das quais 10 estão 100% mortas

IV_Acao (PK Acao decimal(6), 980 linhas). Flags que REALMENTE variam na base:
- EmUso char(1): S=378 / N=602 — 61% do catálogo está desativado
- Classe char(1): O=672, X=226, T=42, P=31, W=5, R=2, '?'=2 (significado dos códigos não documentado; exemplos: T = ações de aferição/insatisfação/cobrança; W = 'Abrir O.S Licença/Atualização', 'Ajustar Carteira Pneus'; R = 'Aprovar Pedido D.S.I.', 'Executar Solução Remota C.S.C')
- Avulsa char(1): N=829 / S=151 — S permite lançar a ação fora de um fluxo
- PermiteExclusao char(1): N=762 / S=218
- ExigeDetalhe char(1): N=701 / S=279
- ExigeFormulario char(1): N=971 / S=9; SeqFormulario preenchido em apenas 10 de 980 ações
- QtdeMaxPessoa decimal(2): NULL=872, 0=66, 1=41, 2=1 → o limite VITALÍCIO por pessoa. É o valor 1 da ação 897 'Validar e Qualificar LEAD' que faz reconversão de lead nunca virar tarefa (defeito 2.7, medido: 189 de 193 casos bloqueados, só 7 com tarefa anterior ainda aberta)
- QtdeMaxProcesso numeric(2): NULL=758, 1=147, 0=66, 2=9
- QtdeMaxPessoaEmp decimal(2): NULL=909, 0=66, 1=5
Flags MORTAS (valor único em todas as 980 linhas): ExigeResposta='N', ExigeProduto='N', ExigeVendedor='N' (1 exceção), ExigeMotivo='N', ExigeDepto='N', ExigePrazo=0, ExigeDtaLimite=0, TarefaCompromisso='T', PermiteReagendar (789 null / 191 zero), Pcte (null em 100% — a partição por módulo/licença nunca foi usada na Tracbel).
Outras: PrazoRealizacao, PrazoMaxInicio, PrazoMaxReag, MinutoAntesPerm, TempoMedio, DutUltAgenda, Instrucao varchar(250), Sigla.
USO REAL vs CATÁLOGO: 980 ações cadastradas, 744 já apareceram em IV_Agenda alguma vez, e apenas 169 foram usadas em 2026.

**Evidencia:** Query live: 18 GROUP BY encadeados por UNION ALL sobre IV_Acao; SELECT COUNT(DISTINCT Acao) FROM IV_Agenda (=744) e WHERE DtaAgenda>='2026-01-01' (=169); doc REGRAS-DE-NEGOCIO 2.7

**Licao para a Tracbel:** COPIAR o conceito de LIMITE DE CARDINALIDADE declarativo (max por pessoa / por processo / por pessoa-empresa) — é uma boa ideia de deduplicação. Mas ADAPTAR: precisa de JANELA. 'QtdeMaxPessoa=1' vitalício é uma decisão de produto disfarçada de config e produziu o pior defeito de funil da operação. O certo é max_open_per_person (tarefas abertas) e/ou max_per_person_per_window (1 a cada 90 dias), explicitamente nomeados.
EVITAR: 10 flags que nunca variam (ruído puro que confunde quem parametriza) e um catálogo com 61% de itens desativados sem arquivamento. ADAPTAR: task_type com status enum (draft|active|deprecated|archived), deprecated_at, e replaced_by_id — e uma tela que só mostre ativos por padrão.

### 12. O catálogo de Resultado: 63 colunas, 35 delas prefixadas CTRL*, com domínio ternário 0/1/9 não documentado

IV_Resultado (PK Resultado decimal(6), 4.208 linhas, 3.665 com EMUSO=1). Estrutura: Acao (a qual ação pertence — 3.992 têm, 216 são genéricos), Descricao varchar(40), Ordem, ADVERTENCIA varchar(200), CMPLTTXTPADRAO varchar(250), Seqformulario, REAGENDARPARA numeric(6) (auto-reagenda para outro resultado), VidaUtil, HORAPRORRLIMITE / HORAPRORRAGDA / HORAREAGENDA / MINUTILREAGENDA (prorrogações em horas), QtdeMaxPorAcao, CanalPadrao, ASSUNTOCMPL, ASSUNTOEMAIL, PubWeb.
As 35 colunas CTRL* dizem, POR RESULTADO, quais campos a tela exige/exibe ao dar o andamento. O domínio NÃO é booleano: é 0 / 1 / 9. Uso medido (só valores não-zero):
- CTRLINTERATIVO=1 em 4.187 (quase todos) — resultado utilizável na tela
- CTRLPRODUTIVO=1 em 3.646 — conta como contato produtivo nas métricas
- CTRLMUDARATDREAG=1 em 3.481 — permite trocar o atendente ao reagendar
- CTRLDETALHE=9 em 3.414 — 9 = EXIGE detalhe (1 = apenas habilita)
- CTRLCONCLUSAO=9 em 2.758 e =1 em 53 — conclui/exige conclusão da agenda
- CTRLREAGENDA=1 em 1.328
- CTRLCOMPLEMENTO=9 em 772; CTRLVENDEDOR=9 em 301; CTRLQTDE=1 em 236; CTRLCAMPANHA=9 em 147; CTRLCONTATOPJ=9 em 101; CTRLFORMACONTATO=1 em 101; CTRLPRODUTO=9 em 83; CTRLMOTIVO=9 em 76; CTRLCONTATOPF=9 em 49; CTRLPERSPECTIVAPROC=1 em 31; CTRLCLIENTEATIVO=9 em 18; CTRLFORMULARIO=1 em 13; CTRLVALOR=9 em 10; CTRLDETALHEPROC=1 em 8; CTRLRESPPROC=1 em 3; CTRLDURACAO, CTRLSTATUSPROC, CTRLVALORPROC, CTRLDTAENCERRAPROC, CTRLDEPARTAMENTO, CTRLCONFIRMAREAG: menos de 5 cada. NUNCA usadas: CTRLREAGENDASILO, CTRLPROJETO, CTRLIMAGEM, CTRLCTIDISPONIVEL, CTRLCMPLSQL, CTRLAGENDACONF, CTRLRESUMOPROC, CTRLDESCRSTATUSPROC.
Satélites do resultado:
- IV_ResultadoCmpl (3.110) — complementos válidos. ⚠️ não é validado na escrita: o conector do RD Station grava 'TALLOS Chat' direto em IV_Historico.ResultadoCmpl sem passar por aqui (defeito 2.10).
- IV_ResVinc (965 linhas) — ARTEFATOS EXIGIDOS por (Resultado, CodProcesso). Vinculo varchar(5) com domínio real: RES=341 (outro resultado como pré-requisito), FORM=294 (formulário), OBDY=172 (objeto dinâmico), DMNG=158 (documento no Doc Manager). Tem Exigido numeric(1) e GrupoVinc varchar(10) que agrupa ALTERNATIVAS (ex.: resultado 3242 'Venda Perdida' exige um dos formulários 146..150, todos GrupoVinc='01').
- IV_ResClasse (236) + IV_ClasseRes (40, com Cor) — classificação/cor do resultado no funil.
- IV_ResultadoInstr (98) — instrução em text para o usuário.
- IV_ResMsgPapel (28 col, 1.338 linhas, 1.323 EMUSO=1) — MOTOR DE NOTIFICAÇÃO: por resultado, dispara mensagem. Canal='EML' em 100%; TipoDest: USR=1.323, PAPEL=13, OBDYN=2. Tem SeqTxtPadrao, Template varchar(250), Remetente, QTDEDIAS/QTDEHU (atraso do envio), INDENVIAQLQERHORA, PRIORIDADE, SEQCONTAENVIO. O campo Papel varchar(25) volta a usar os pseudo-códigos ('-5' em 52 linhas).
USO REAL: 4.208 resultados cadastrados, 2.854 já usados em IV_Historico, apenas 501 usados em 2026.

**Evidencia:** Query live: 37 GROUP BY por UNION ALL sobre IV_Resultado filtrando valores <>0; SELECT Vinculo,COUNT(*) FROM IV_ResVinc; SELECT Canal,TipoDest,COUNT(*) FROM IV_ResMsgPapel; SELECT COUNT(DISTINCT Resultado) FROM IV_Historico total=2854 e 2026=501

**Licao para a Tracbel:** COPIAR: (a) IV_ResVinc é a melhor ideia do catálogo — 'para concluir com este desfecho você precisa ANEXAR X' com grupos de alternativas. Implementar como outcome_requirement { outcome_id, flow_id, kind: form|document|reference|prior_outcome, target_id, required, alternative_group }; (b) a instrução por resultado; (c) a cor/classe para o funil.
EVITAR: 35 colunas CTRL* com domínio ternário 0/1/9 não documentado — é configuração de UI vazando para o schema do domínio, e 8 delas nunca foram usadas. ADAPTAR: um único jsonb ui_requirements { detail: 'required', reschedule: 'allowed', ... } com enum nomeado (hidden|optional|required), validado por schema. E validar ResultadoCmpl na escrita — o defeito 'TALLOS Chat' existe porque a API grava sem checar o catálogo.

### 13. Três motores de automação coexistem, e só um é conhecido

Além de IV_AcaoAuto (geração), o núcleo tem mais três mecanismos independentes:
1. IV_AcaoRem (9 col, 266 linhas) — REMOÇÃO. 'Quando o Resultado R for lançado, apague as agendas pendentes da Ação A'. Colunas: Resultado, Acao, NaoTrabalhada numeric(1) (=1 em todas: só remove as não trabalhadas), Interativo, INDREMPES. Exemplos reais: resultado 3449 'Venda Perdida' remove as ações 804, 863, 822, 821, 819 (todas as pendências do fluxo FY25); resultado 940129 'Reclamação Procede' remove a 940029 'ACOMPANHAR INSATISFAÇÃO SERVIÇOS'. É o mecanismo de limpeza — e a ausência dele para o caso 'agenda duplicada' é o que empurra o usuário a usar 'Atividade Cancelada' e cancelar o processo inteiro (defeito 2.2).
2. IV_AcaoCtrl (13 col, 44 linhas) — ESCALAÇÃO/ALERTA por ação. Colunas: Acao, SeqCtrl, Base char(1)='A' em todas, SeqUsrBase, Minuto decimal(8) (prazo: 60, 480=8h, 960=16h, 1440=24h), Destinatario numeric(18), Atitude varchar(10)='EMAIL' em todas as 44, IntervaloExec decimal(4) (repetição: 30 ou 1440 min), DtaProxExec, TipoDest, Papel, SeqTxtPadrao, Evento. Ex.: ação 382 tem 18 linhas mapeando SeqUsrBase -> Destinatario (escalação nominal, uma linha por par). Ação 825 escala em 60 minutos.
3. IV_Evento (27 col, 132 linhas) — AUTOMAÇÃO POR EVENTO EXTERNO, o motor menos visível. Origem varchar(20) identifica o sistema: RD (RD Station), PROTHEUS/Protheus, SISDIA, AUDIT. Evento varchar(200) é o nome do gatilho ('WEBHOOK.CONVERTED', 'WEBHOOK.MARKED_OPPORTUNITY', 'NFS/MAQ-NOVOS/N', 'NFS/MAQ-PEÇAS/N', 'OS', 'PECAFAT', 'VEICFATTRTN'...). Efeitos declarados: ResultAtivo numeric(6) e ResultRec/ResultRecPE/RESULTRECHSTE/RESULTRECHSTNE (qual RESULTADO lançar, o que reentra no motor de IV_AcaoAuto!), GeraAcao, GeraAndSempre, GeraRecSempre, ProcessoUnico, AtuHistProcesso, ExcluiProcesso, DetalheAuto, e ProcedureAdicional varchar(200) — o NOME DE UMA STORED PROCEDURE a executar. Ex.: WEBHOOK.CONVERTED -> ResultRec 1278, ProcessoUnico=1.
Há também IV_Distribui (3 col, 609 linhas: SeqUsuario, Acao, UltimoUsuario) = round-robin de distribuição de tarefas por ação.
E IV_ObjFlow (9 col, 726 linhas) — o DESENHISTA visual: CodModelo, TipoObj ('ACAO' 391, 'RES' 246, e variantes 'RES:<complemento>' / 'CMPL:<complemento>'), ChaveObj, PosLeft/PosTop, IndCor, Dados varchar(1000). ⚠️ Guarda APENAS coordenadas de tela: o diagrama é um desenho DAS regras, não a fonte da verdade. E existe para só 18 CodModelo, contra 62 tipos de fluxo — 44 fluxos não têm diagrama nenhum.

**Evidencia:** Query live: SELECT TOP 12 ... FROM IV_AcaoRem JOIN IV_Resultado/IV_Acao; SELECT * FROM IV_AcaoCtrl (44 linhas); SELECT TOP 25 ... FROM IV_Evento ORDER BY DtaUltUso DESC; SELECT TipoObj,COUNT(*),COUNT(DISTINCT CodModelo) FROM IV_ObjFlow

**Licao para a Tracbel:** COPIAR os TRÊS primitivos como cidadãos de primeira classe da mesma linguagem de regra: CREATE_TASK, CLOSE_TASK (o AcaoRem — essencial e hoje subusado com só 266 regras), ESCALATE/NOTIFY (o AcaoCtrl, hoje limitado a e-mail). Um efeito CLOSE_TASK bem parametrizado teria evitado o defeito mais caro da operação.
COPIAR também IV_Evento: gatilho externo (webhook/ERP) que injeta um OUTCOME no mesmo motor é o desenho certo — o sistema externo não cria tarefa, ele produz um evento de domínio.
EVITAR: ProcedureAdicional (nome de stored procedure em varchar) — é código escondido no dado, invisível para o time (e com o login read-only sys.sql_modules.definition vem NULL, então nem dá para ler). Substituir por handler nomeado registrado na aplicação.
EVITAR: diagrama que só guarda coordenadas. No CRM próprio o diagrama deve ser GERADO da definição das transições, não desenhado em paralelo — senão diverge, como divergiu (18 modelos para 62 fluxos).

### 14. Observabilidade: 4 tabelas de monitoração e um log de 11 milhões de linhas — o que funciona e o bug do HU negativo

O Vórtice materializa a linha do tempo do processo em três tabelas append-only:
- IV_ProcFaseMonit (9 col, 655.144 linhas): (Processo, SeqFase) + FASE, FaseOrdem, DtaInicio, DtaFim, UltHistorico, HU decimal/numeric(6,2), HN numeric(6,2). Uma linha por VISITA a uma fase — com SeqFase sequencial, então revisitas geram linhas novas.
- IV_ProcStatMonit (6 col, 850.022): (Processo, DtaMonit) + Status, HU, HN, UsuAlteracao.
- IV_ProcPerspMonit (6 col, 126.145): (Processo, DtaMonit) + Perspectiva, HU, HN.
HU = horas ÚTEIS na fase/status, HN = horas naturais. É exatamente o insumo de lead time / cycle time por etapa, pronto para o funil.
DOIS PROBLEMAS medidos em amostra real (processo 1619541):
(a) HU vem NEGATIVO: fase Faturamento HU=-0,60 / HN=0,97; Análise HU=-0,50 / HN=0,10; Formalização HU=-0,40 / HN=0,10. O cálculo de horas úteis quebra quando o intervalo cai fora do expediente. Qualquer média de lead time por fase está contaminada.
(b) NÃO HÁ ORDEM IMPOSTA: o mesmo processo percorreu Monitoramento(1) -> Pedido de Venda(17) -> Faturamento(75) -> Análise(26) -> Formalização(41) -> Faturamento(75). O motor aceita ir para trás e pular etapas sem restrição, porque a transição não valida a fase de origem.
AUDITORIA: IV_AgendaLog (9 col, 11.049.475 linhas — a maior tabela do módulo, 46% de todas as linhas IV_). Estrutura genérica: SEQLOGTB PK, Kn1 (a chave do registro), Tb varchar(12) (='IV_AGENDA' em 100% dos casos medidos em ago/2026), Kn2 (=0), Ks (='X'), DtaLog, Usr, CodApl varchar(30), Obs varchar(1000). O CodApl identifica a TELA que fez a alteração — muito útil: IVS7AGE02_AndamentoAgendaTab 9.768, MOBILELITE 5.570, IVS1AGE05_AgendaTransfere 5.001, frmPrincipal 3.222, IVS1AGE07_Reagenda 1.773, IVS1AGE04 1.284, 'Servidor de processos' 208, IVS1AGE00_AgendaDetalhe 204. O Obs é TEXTO LIVRE do tipo 'Realizada de [N] p/ [S]', 'DTAAGENDA: de [29/08/2026 13:55:44] para [28/10/2026 11:41:28]...' — não é diff estruturado, então não dá para consultar 'todas as mudanças no campo X' sem parsear string.
Outros: IV_Ciencia (5 col, 173.477) = confirmação de leitura de um histórico por outro usuário (SeqHistorico, SeqCiencia, DtaLeitura, CodUsuario, Obs) — bom mecanismo de 'dar ciência'. IV_HistoricoNota (text, 94.315), IV_HistInfo (17.250), IV_AgendaCtrl (963), IV_USRSTATUS (186, presença/pessoa ativa do usuário).

**Evidencia:** Query live: SELECT * FROM IV_ProcFaseMonit WHERE Processo=1619541 ORDER BY SeqFase; SELECT Tb,CodApl,COUNT(*) FROM IV_AgendaLog WHERE DtaLog>='2026-08-01' GROUP BY; SELECT TOP 6 ... FROM IV_AgendaLog ORDER BY SEQLOGTB DESC; schema/tabelas.csv

**Licao para a Tracbel:** COPIAR: a materialização do tempo por fase E por status separadamente, com horas úteis e naturais lado a lado — é a base de todo indicador de funil e a maioria dos CRMs não tem. COPIAR também o registro da APLICAÇÃO/TELA de origem no log (CodApl) e o mecanismo de 'ciência'.
EVITAR: calcular horas úteis sem calendário de trabalho validado (HU negativo em produção); log de auditoria como texto livre. ADAPTAR: audit_log { entity, entity_id, field, old_value, new_value, actor, source_app, at } — colunas separadas, consultável, e derivável de um event store se o modelo for event-sourced. Um único log de 11M linhas com Obs em varchar(1000) é write-only.
EVITAR também: aceitar transição sem validar from_state. Se o desenho da máquina de estados exigir a fase de origem (como IV_ProcResultado.Fase já sugere mas não aplica), o pulo Faturamento->Análise->Formalização->Faturamento simplesmente não acontece.

### 15. Atores: IV_Operador, IV_VENDEDOR e a hierarquia comercial paralela ao cadastro de usuário

Há DOIS cadastros de gente, com propósitos diferentes e sem unificação:
1. IV_Operador (51 colunas, 932 linhas) — o perfil operacional do usuário no CRM, chaveado por SeqUsuario (FK para GE_Usuario). Contém: SeqUnidade, Funcao char(1), Supervisor varchar(20), Gerente varchar(20), Status, Departamento, CodUsuario, eMail, EMailAssinatura, EnderecoSMTP, FormaEnvioEmail; disponibilidade (HoraInicial/HoraFinal + DispDomingo..DispSabado); e 22 FLAGS DE PERMISSÃO em char(1) chapadas na mesma tabela: IncHistorico, IncHistRetr, QtdDiasRetr, IncHistAgConcl, AltHistorico, ExcHistorico, IncAgenda, AltAgenda, ExcAgenda, VerAgenda, ConcAgenda, AltOperAgenda, AltOperAgdReag, ConcAgeVendor, ReativarAgenda, AnalisarHistorico, AbreAnalise, Alteragrupo, AlteraCodImport, AlteraRegiao, AlteraVendedor, AlteraClienteAtivo, AlteraStatus. Mais PesAtendSeq/PesAtendEnv/PesAtendHora (pessoa em atendimento agora).
   IV_Atendente (2 col: SeqUsuario, SeqPerfil) e IV_SegPerfil (27 col) estão AMBAS VAZIAS — o modelo de perfil/papel foi previsto e nunca implantado. A segurança fica nas 22 flags por usuário.
2. IV_VENDEDOR (18 col, 344 linhas) — o CEN/vendedor comercial, que NÃO é necessariamente um usuário. SEQVENDEDOR PK, CODVENDEDOR varchar(20) (ex. 'JULIA.COSTA'), VENDEDOR varchar(40), SeqUsuario (opcional!), SeqUsuarioLider (a HIERARQUIA de vendas), SeqUnidade, EMUSO, NROEMPRPADRAO, EQUIPE varchar(40), SALARIOFIXO, COMISSAO1/COMISSAO2, CODVENDEDORFORA/CODVENDEDORFORAX (código no ERP).
   IV_VENDEDOREMPR (4 col, 917 linhas: SEQVENDEDOR, NroEmpresa) — o vendedor só aparece na lista de CEN de uma filial se tiver linha aqui.
⚠️ ARMADILHA CONFIRMADA: IV_Agenda.Vendedor guarda o CODVENDEDOR (varchar 'JULIA.COSTA'), não o SEQVENDEDOR numérico. E IV_Historico.Vendedor / IV_ProcDado.Vendedor também. Toda a hierarquia de visibilidade de relatório (quem vê a carteira de quem) depende de IV_VENDEDOR.SeqUsuarioLider, que é uma segunda árvore, desconectada de IV_Operador.Supervisor/Gerente (que são varchar).

**Evidencia:** schema/colunas.csv; query live SELECT COUNT(*) FROM IV_Atendente=0, IV_SegPerfil=0; docs/REGRAS-DE-NEGOCIO.md 2.4/2.5/2.6

**Licao para a Tracbel:** COPIAR o insight central: o VENDEDOR COMERCIAL não é o mesmo objeto que o USUÁRIO DO SISTEMA. Um CEN pode existir sem login (pessoa terceirizada, código herdado do ERP) e um usuário pode não ser vendedor. Separar sales_rep de app_user, ligados por FK opcional, é correto.
EVITAR: (a) duas hierarquias paralelas (IV_Operador.Supervisor/Gerente em varchar vs IV_VENDEDOR.SeqUsuarioLider numérico) — uma só, tipada, com validação de ciclo; (b) 22 flags booleanas de permissão coladas no cadastro do usuário, com a tabela de perfil (IV_SegPerfil) existindo vazia — implementar RBAC de verdade desde o dia 1: role, permission, role_permission, user_role; (c) chave estrangeira em varchar de nome de login ('JULIA.COSTA') em 3 tabelas de alto volume — sempre id numérico/uuid.
E modelar a hierarquia como fato TEMPORAL (rep_manager { rep_id, manager_id, valid_from, valid_to }): isso resolve de vez o defeito 2.5 (aprovação foi para o líder antigo) e o 2.6 (troca de usuário quebra o relatório do gerente), porque a atribuição passa a ser resolvida contra a hierarquia vigente e é reprocessável.

### 16. Segmentação por DSL própria: IV_Selecao e o construtor de critérios (o que copiar)

IV_Selecao (12 col, 543 linhas): SeqSelecao PK, Selecao varchar(50), Descricao varchar(200), EmUso, Dinamica numeric(1) (recalcula vs snapshot), Tipo char(1), DtaGeracao, UsuGeracao, QtdeClientes decimal(7) (contagem materializada), Construtor varchar(20), Usuario.
IV_SelecaoCriterio (7 col, 2.865 linhas): SeqSelecao, SeqCriterio, Operacao char(1), Ordem, SeqSelecaoFoco (seleção aninhada!), DescCriterio varchar(60), InstrSQL text.
Inspecionei o conteúdo de InstrSQL e NÃO é SQL cru — é uma DSL própria com aliases de tabela nomeados:
  [Tab.Agenda].ACAO = 'Monitorar Cliente (FY25)' [E] [Tab.Agenda].REALIZADA = 'N'
  [Tab.Carteira].CARTEIRA = 'MAQ_01RIB_05'
  [Tab.Pessoa].SEQPESSOA [Na Lista] (121374, 121293, ...)
Operacao: '+' inclui o conjunto, '-' exclui — ou seja, a seleção é uma ÁLGEBRA DE CONJUNTOS ordenada (Ordem), com SeqSelecaoFoco permitindo referenciar outra seleção como base.
IV_SelecaoPessoa (3 col, 582.481 linhas): o resultado materializado (SeqSelecao, SeqPessoa, Usado numeric(1) — marca quem já foi trabalhado).
IV_SelecaoColList (35) define as colunas exibidas.
Ligado a campanha: IV_Campanha (33 col, 179 linhas) com DtaInicio/DtaFim, Resultado (o resultado que a campanha lança), SEQFORMULARIO, SEQTXTPADRAO, SEQEMAILTEMPLATE, prioridades por canal (PRIORIDADEEMAIL/SMS/CORREIO/ALLIN/Mail2Easy), janelas de horário (EMAILHORAINI/FIM, SMSHORAINI/FIM), INDINTERATIVO. As tabelas de execução da campanha (IV_CampPessoa, IV_CampPesMsg, IV_CampVoucher, IV_CAMPSELECAO) estão TODAS VAZIAS — o módulo de campanha nunca foi operacionalizado na Tracbel, só o cadastro.
IV_Motivo (4 col, 43 linhas) = origem/motivo do lead, catálogo saudável e em uso (Instagram Anúncio, RD Tallos, Google anúncio, Landing Page, John Deere, MF Rural...). ⚠️ Erro de digitação em produção: SeqMotivo 44 tem Motivo='Call Now'/Descricao='MF Rural' e o 45 tem Motivo='MF Rural'/Descricao='Call Now' — invertidos.

**Evidencia:** Query live: SELECT TOP 3 SeqSelecao,SeqCriterio,Operacao,DescCriterio,LEFT(CAST(InstrSQL AS varchar(400)),300) FROM IV_SelecaoCriterio; SELECT * FROM IV_Motivo ORDER BY SeqMotivo; schema/tabelas.csv (IV_CampPessoa=0)

**Licao para a Tracbel:** COPIAR o desenho da seleção — é a melhor peça do módulo de marketing do Vórtice: critérios como lista ORDENADA de operações de conjunto (+/-), com aliases de tabela legíveis pelo usuário de negócio, seleção aninhada, flag dinâmica vs snapshot e contagem materializada. Implementar como segment { id, name, is_dynamic } + segment_criterion { segment_id, ord, op: include|exclude, expr jsonb, base_segment_id? } compilado para SQL parametrizado no servidor — nunca concatenando string.
COPIAR também segment_member.used (marcar quem já foi trabalhado) — evita retrabalho em campanha.
EVITAR: guardar a expressão como text livre (mesmo sendo DSL, não há schema nem validação); e cadastrar um módulo de campanha inteiro (33 colunas + 5 tabelas) que nunca é usado.

### 17. Inventário completo das 377 tabelas IV_ agrupadas por sub-função (com linhas)

Total: 377 tabelas, 23.793.450 linhas, 131 VAZIAS (34,7%). Snapshot dos CSVs = jun/2026; onde medi ao vivo o número está em parênteses.

A) MOTOR DE PROCESSO — runtime (17 tabelas, 5,12M linhas)
IV_ProcDado 1.516.214 (1.532.133) · IV_Processo 1.174.932 (1.190.252) · IV_ProcStatMonit 850.022 · IV_ProcFaseMonit 655.144 · IV_ProcLink 602.150 · IV_ProcPerspMonit 126.145 · IV_ProcDocto 73.646 (76.603) · IV_ProcProduto 62.578 · IV_ProcRef 61.975 · IV_Processo_bkp20250717 1 · VAZIAS: IV_ProcComent, IV_ProcRelacao, IV_ProcTpRel, IV_ProcAtiv, IV_ProcProjeto, IV_PROCTAG, IV_PROCPESLINK

B) PARAMETRIZAÇÃO DO FLUXO — design-time (12 tabelas, 3.766 linhas)
IV_ProcResultado 2.153 (2.160) · IV_ProcAcao 738 · IV_ObjFlow 726 · IV_ProcFase 273 · IV_ProcSt 250 · IV_ProcVinc 190 · IV_ProcPersp 67 · IV_CodProcesso 62 · IV_CodPrcEmpr 17 · IV_CodProcComent 1 · VAZIAS: IV_ResultadoReq, IV_RetProcRegra

C) AGENDA / TAREFA (13 tabelas, 11,99M linhas)
IV_AgendaLog 11.049.475 · IV_Agenda 931.989 · IV_AgendaCtrl 963 · IV_AgdLink 4.534 · IV_Agenda_bkp20250717 1 · VAZIAS: IV_AgdUsr, IV_AgdRec, IV_Recurso, IV_RecUso, IV_AGENDACMPL, IV_AGENDAITEM, IV_AGDPLANTRG, IV_AGDPLANACAO

D) HISTÓRICO / INTERAÇÃO (7 tabelas, 3,89M linhas)
IV_Historico 2.436.127 · IV_Interacao 720.525 · IV_HistLink 645.850 · IV_Ciencia 173.477 · IV_HistoricoNota 94.315 · IV_HistInfo 17.250 · VAZIA: IV_HISTORICOTAG

E) CATÁLOGO DE AÇÃO + AUTOMAÇÃO (13 tabelas, 7.863 linhas)
IV_AcaoAuto 5.942 (5.958) · IV_Acao 979 (980) · IV_Distribui 609 · IV_AcaoRem 266 · IV_AcaoCtrl 44 · IV_PcteAtrFx 22 · IV_Pcte 1 · VAZIAS: IV_AcaoAutoCtrl, IV_AcaoAtendente, IV_AcaoMon, IV_AcaoCmpl, IV_ACAOANEXA, IV_ACAOURACENARIO

F) CATÁLOGO DE RESULTADO (13 tabelas, 12.771 linhas)
IV_Resultado 4.201 (4.208) · IV_ResultadoCmpl 3.110 · IV_RESULTADO_3110 2.826 · IV_ResMsgPapel 1.341 (1.338) · IV_ResVinc 959 (965) · IV_ResClasse 236 · IV_ResultadoInstr 98 · IV_ClasseRes 40 · VAZIAS: IV_ResParam, IV_ResEvtOut, IV_ResultadoWeb, IV_RESJOB, IV_PAREVTEXTRES

G) FORMULÁRIOS / QUESTIONÁRIOS (4 + 175 tabelas geradas, 179.396 linhas)
IV_Questionario 85.393 (87.929) · IV_QuestaoLista 3.995 · IV_Questao 2.300 (2.303) · IV_Formulario 176 · + 175 tabelas IV_Q_* somando 87.532 linhas e 2.601 colunas (32 vazias). Maiores: IV_Q_OS_GARANTIA 7.786, IV_Q_COMISSAO 7.165, IV_Q_ACOMPANH_VENDA_JDE 6.343 (104 col!), IV_Q_PECAS_AFERICAO 5.174, IV_Q_GAR_DATA_SERVICO 4.888

H) OBJETOS DINÂMICOS / PROPRIEDADES (12 tabelas, 428.775 linhas)
IV_ClientePropr 246.684 · IV_ClientePropr_ITA 157.900 (backup) · IV_PropriLista_ITA 7.520 · IV_PropriLista 7.375 · IV_ClienteAtrib 2.018 · IV_ClientePropr_BKPJUN 1.000 · IV_Propriedade 52 · IV_AtribLista 37 · IV_Propriedade_ITA 32 · IV_Propriedade_BKPJUN 21 · IV_Atributo 10 · VAZIAS: IV_PropriListaLk, IV_ClientePropCmpl

I) ATORES / SEGURANÇA (10 tabelas, 2.383 linhas)
IV_Operador 932 · IV_VENDEDOREMPR 917 · IV_VENDEDOR 344 · IV_USRSTATUS 186 · IV_OpBloq 4 · VAZIAS: IV_Atendente, IV_SegPerfil, IV_AtdBloq, IV_Unidade, IV_Departamento

J) SEGMENTAÇÃO / CAMPANHA / TEXTOS (14 tabelas, 586.998 linhas)
IV_SelecaoPessoa 582.481 · IV_SelecaoCriterio 2.865 · IV_Selecao 543 · IV_TxtPadrao 212 · IV_Campanha 179 · IV_Motivo 43 · IV_SelecaoColList 35 · IV_TxtPadraoUso 5 · VAZIAS: IV_CampPessoa, IV_CampPesMsg, IV_CampVoucher, IV_CAMPSELECAO, IV_CustoMidia, IV_TxtPadConta

K) EVENTOS / INTEGRAÇÃO / PARÂMETROS (12 tabelas, 478.420 linhas)
IV_STATUS_DEPTO 473.844 · IV_GlobalPar 3.848 · IV_ListSQL 360 · IV_GlobalParLista 197 · IV_Evento 132 · IV_GlobalParCtrl 48 · IV_OcrmAgd 18 · IV_OcrmDest 17 · VAZIAS: IV_EventoAcao, IV_LEADFACEBOOK, IV_LEADFACEITEM, IV_TIPOCONTEUDO(1)

L) CANAIS / MENSAGERIA (13 tabelas, 54.203 linhas)
IV_SMSLog 27.430 · IV_SMS 26.749 · IV_OPTEMAIL 21 · IV_OPTFONE 2 · VAZIAS: IV_eMail, IV_WHATSAPP, IV_PUSH, IV_USRPUSH, IV_EMAILESTATISTICA, IV_FoneCtrl, IV_FoneCtrl2, IV_FoneCtrlHst, IV_URADISPARO, IV_URARELATORIO

M) COBRANÇA — submódulo Cbr (18 tabelas, 847.438 linhas)
IV_CbrCobrancaTitLog 304.684 · IV_CBRTITULOHST 173.530 · IV_CbrCobrancaTit 155.204 · IV_CBRCRITMON 128.167 · IV_CbrCobranca 64.449 · IV_CbrCobrancaLote 21.048 · IV_CbrCriterioDef 231 · IV_CbrCriterio 122 · IV_CBRCRITERIOMSG 3 · VAZIAS (geração antiga duplicada!): IV_CobrCrit, IV_CobrCritAgd, IV_CobrCritDef, IV_CobrCritMon, IV_CobrTit, IV_CbrTitulo, IV_CbrCobrancaMon, IV_CBRCOBRANCAHST, IV_CFinDocAceito

N) MÓDULOS MORTOS / NUNCA IMPLANTADOS (~40 tabelas, ~100 linhas no total)
Projetos: IV_Projeto, IV_ProjPessoa, IV_ProjEquipe, IV_ProjDocto, IV_ProjColec, IV_TEMPLATEPROJ (todas 0)
Atividades/planos: IV_Ativ, IV_AtivProc, IV_AtivAgenda, IV_PlanoAtiv, IV_ATIVGRUPO, IV_ATIVMODELO, IV_ATIVPADRAO (todas 0)
Cartão/crédito (12 tabelas): IV_CartCred, IV_CARTPESSOA, IV_CartProposta, IV_CartTitular, IV_CartContratante, IV_CartProduto, IV_CartMarca, IV_CartLoteImp, IV_CartLoteCartao, IV_CARTPESORIGEM, IV_BonusCC (todas 0)
Base de conhecimento: IV_Conhecimento 9, IV_ConhecFonema 19, IV_ConhecLeitura 49, IV_BaseInformacao 0
Documentos/aplicações: IV_DoctoApl 6, IV_DoctoTipo 3, IV_DoctoAplUso 3
Outras zeradas: IV_FichaNegVeic (79 col!), IV_OS (22 col), IV_Pessoa, IV_PessoaStat, IV_TC_PESSOA, IV_ObjVenda, IV_ESTRPRODUTO, IV_SELPROMOPRD, IV_TpPgto, IV_TAGCAD, IV_Produto(1)

**Evidencia:** schema/tabelas.csv filtrado por prefixo IV_ (377 linhas, soma 23.793.450, 131 com 0 linhas); contagens live via connect.ps1 onde indicado entre parênteses

**Licao para a Tracbel:** O domínio REAL da Tracbel cabe em cerca de 45 tabelas, não 377. Distribuição: 4 tabelas concentram 16,5M das 23,8M linhas (AgendaLog, Historico, ProcDado, Processo). 175 tabelas (46%) são geração automática de formulário. 131 (35%) estão vazias — módulos comprados e nunca ligados (cartão de crédito, projetos, WhatsApp, push, URA, base de conhecimento) ou gerações duplicadas do mesmo módulo (IV_Cobr* morto convivendo com IV_Cbr* vivo).
Para o CRM próprio: (1) partir de um núcleo de ~20 tabelas (process, task, event, task_type, outcome, flow_type, state, transition, rule, form_definition, form_response, person, sales_rep, custom_object_*, segment_*, audit) e crescer por demanda comprovada; (2) NUNCA versionar um módulo criando uma segunda geração de tabelas ao lado da primeira — migrar; (3) tabela vazia por mais de 2 releases é dívida: remover; (4) planejar particionamento/retenção para o audit log desde o início (11M linhas em uma tabela sem estrutura de diff é o maior ativo desperdiçado da base).

## Lacunas declaradas

- Significado dos códigos negativos de SeqUsuario em IV_AcaoAuto: confirmei empiricamente apenas -5 e -15 (resolvem para o VENDEDOR/CEN do processo — validado nas ações 900 e 767). Não consegui determinar -2, -3, -4, -6, -7, -12, -13, -14. Suspeita forte de que -3 seja 'líder do CEN' (usado só na ação 871 Aprovar Demonstração), mas não havia agendas recentes dessa ação para medir. Resolver isso exige acesso ao código do VRTCSERVER ou pergunta direta à Vórtice.
- Significado das letras de IV_Acao.Classe (O=672, X=226, T=42, P=31, W=5, R=2, '?'=2) e de IV_Agenda.Classe (O=640k, X=154k, T=136k, P=33k, C=2,5k, W=154, E=41, R=7, M=1). Levantei exemplos concretos por classe mas não há tabela de domínio no banco nem correlação óbvia; 'X' pode ser 'excluída/inativa' e 'T' agrupa aferições/insatisfação/cobrança, mas é inferência.
- Semântica exata do valor 9 vs 1 nas 35 colunas CTRL* de IV_Resultado. A leitura mais provável (0=não usa, 1=habilita/opcional, 9=exige/obrigatório) é consistente com os dados e com os casos conhecidos, mas não está documentada em lugar nenhum e não consegui um teste que a isolasse — CTRLCONCLUSAO=9 aparece em 2.758 de 4.208 resultados, o que é alto demais para significar 'conclui o processo'.
- Código do motor de workflow. sys.sql_modules.definition volta NULL com o login CRM_Leitura (falta a permissão VIEW DEFINITION), então não pude ler as 6 procedures/1 função do banco nem o conteúdo de IV_Evento.ProcedureAdicional. A ordem exata de avaliação entre IV_ProcResultado, IV_AcaoAuto, IV_AcaoRem e IV_ResVinc dentro de uma transação de andamento é inferida do efeito, não lida do código.
- Não apurei se IV_ProcResultado.Fase é VALIDADA como pré-condição (o motor recusa a transição se o processo não está naquela fase) ou apenas documental. A amostra do processo 1619541 (Faturamento→Análise→Formalização→Faturamento) sugere que NÃO valida, mas não testei o caso de um resultado com Fase preenchida aplicado a processo em fase divergente.
- Regra de truncamento de IV_Historico.ResultadoCmpl: o doc afirma corte em 20 caracteres na gravação, mas medi 917 linhas com LEN > 20. Não determinei qual caminho de escrita (tela desktop, mobile, API, conector RD) trunca e qual não.
- Domínio e uso de IV_ObjFlow.Dados varchar(1000) e IndCor — só verifiquei que TipoObj é ACAO/RES/'RES:<cmpl>'/'CMPL:<cmpl>' e que guarda PosLeft/PosTop. Não inspecionei o conteúdo de Dados para saber se há alguma regra codificada ali (venho assumindo que é só estilo/rótulo do desenho).
- Não medi quantos dos 5.958 IV_AcaoAuto.SeqUsuario positivos apontam para usuários já INATIVOS em GE_Usuario — é a métrica que quantificaria o defeito 2.5/2.6 (agenda indo para quem saiu). Ficou de fora por tempo.
- IV_STATUS_DEPTO (473.844 linhas) e IV_Distribui: classifiquei pela estrutura mas não confirmei o papel operacional exato de nenhuma das duas com dado real.
- Submódulo de cobrança IV_Cbr* (847 mil linhas, 18 tabelas) foi apenas inventariado — não mapeei o motor de critérios (IV_CbrCriterio / IV_CbrCriterioDef / IV_CBRCRITMON) nem como ele se conecta ao motor de agenda principal.
