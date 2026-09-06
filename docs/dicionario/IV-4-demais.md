# IV-4 - Demais tabelas do nucleo CRM

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**89 tabelas · 1.151 colunas · 1.493.791 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IV_Atendente`](#iv_atendente) | vazia | 2 | 0 |
| [`IV_Ativ`](#iv_ativ) | vazia | 17 | 0 |
| [`IV_ATIVGRUPO`](#iv_ativgrupo) | vazia | 4 | 0 |
| [`IV_ATIVMODELO`](#iv_ativmodelo) | vazia | 3 | 0 |
| [`IV_ATIVPADRAO`](#iv_ativpadrao) | vazia | 8 | 0 |
| [`IV_BaseInformacao`](#iv_baseinformacao) | vazia | 8 | 0 |
| [`IV_BonusCC`](#iv_bonuscc) | vazia | 8 | 0 |
| [`IV_Campanha`](#iv_campanha) | isolada | 33 | 179 |
| [`IV_CampPesMsg`](#iv_camppesmsg) | vazia | 22 | 0 |
| [`IV_CampPessoa`](#iv_camppessoa) | vazia | 5 | 0 |
| [`IV_CAMPSELECAO`](#iv_campselecao) | vazia | 11 | 0 |
| [`IV_CampVoucher`](#iv_campvoucher) | vazia | 9 | 0 |
| [`IV_CartContratante`](#iv_cartcontratante) | vazia | 14 | 0 |
| [`IV_CartCred`](#iv_cartcred) | vazia | 36 | 0 |
| [`IV_CartLoteCartao`](#iv_cartlotecartao) | vazia | 5 | 0 |
| [`IV_CartLoteImp`](#iv_cartloteimp) | vazia | 7 | 0 |
| [`IV_CartMarca`](#iv_cartmarca) | vazia | 5 | 0 |
| [`IV_CARTPESORIGEM`](#iv_cartpesorigem) | vazia | 4 | 0 |
| [`IV_CARTPESSOA`](#iv_cartpessoa) | vazia | 53 | 0 |
| [`IV_CartProduto`](#iv_cartproduto) | vazia | 9 | 0 |
| [`IV_CartProposta`](#iv_cartproposta) | vazia | 30 | 0 |
| [`IV_CartTitular`](#iv_carttitular) | vazia | 12 | 0 |
| [`IV_CbrCobranca`](#iv_cbrcobranca) | nucleo | 8 | 64.449 |
| [`IV_CBRCOBRANCAHST`](#iv_cbrcobrancahst) | vazia | 5 | 0 |
| [`IV_CbrCobrancaLote`](#iv_cbrcobrancalote) | nucleo | 4 | 21.048 |
| [`IV_CbrCobrancaMon`](#iv_cbrcobrancamon) | vazia | 11 | 0 |
| [`IV_CbrCobrancaTit`](#iv_cbrcobrancatit) | nucleo | 10 | 155.204 |
| [`IV_CbrCobrancaTitLog`](#iv_cbrcobrancatitlog) | nucleo | 5 | 304.684 |
| [`IV_CbrCriterio`](#iv_cbrcriterio) | catalogo | 26 | 122 |
| [`IV_CbrCriterioDef`](#iv_cbrcriteriodef) | nucleo | 16 | 231 |
| [`IV_CBRCRITERIOMSG`](#iv_cbrcriteriomsg) | isolada | 6 | 3 |
| [`IV_CBRCRITMON`](#iv_cbrcritmon) | nucleo | 17 | 128.167 |
| [`IV_CbrTitulo`](#iv_cbrtitulo) | vazia | 5 | 0 |
| [`IV_CBRTITULOHST`](#iv_cbrtitulohst) | isolada | 5 | 173.530 |
| [`IV_CFinDocAceito`](#iv_cfindocaceito) | vazia | 4 | 0 |
| [`IV_CobrCrit`](#iv_cobrcrit) | vazia | 11 | 0 |
| [`IV_CobrCritAgd`](#iv_cobrcritagd) | vazia | 5 | 0 |
| [`IV_CobrCritDef`](#iv_cobrcritdef) | vazia | 10 | 0 |
| [`IV_CobrCritMon`](#iv_cobrcritmon) | vazia | 17 | 0 |
| [`IV_CobrTit`](#iv_cobrtit) | vazia | 11 | 0 |
| [`IV_ConhecFonema`](#iv_conhecfonema) | nucleo | 2 | 19 |
| [`IV_Conhecimento`](#iv_conhecimento) | catalogo | 19 | 9 |
| [`IV_ConhecLeitura`](#iv_conhecleitura) | nucleo | 5 | 49 |
| [`IV_CustoMidia`](#iv_customidia) | vazia | 9 | 0 |
| [`IV_eMail`](#iv_email) | vazia | 15 | 0 |
| [`IV_EMAILESTATISTICA`](#iv_emailestatistica) | vazia | 20 | 0 |
| [`IV_ESTRPRODUTO`](#iv_estrproduto) | vazia | 6 | 0 |
| [`IV_FichaNegVeic`](#iv_fichanegveic) | vazia | 79 | 0 |
| [`IV_FoneCtrl`](#iv_fonectrl) | vazia | 6 | 0 |
| [`IV_FoneCtrl2`](#iv_fonectrl2) | vazia | 6 | 0 |
| [`IV_FoneCtrlHst`](#iv_fonectrlhst) | vazia | 7 | 0 |
| [`IV_LEADFACEBOOK`](#iv_leadfacebook) | vazia | 21 | 0 |
| [`IV_LEADFACEITEM`](#iv_leadfaceitem) | vazia | 6 | 0 |
| [`IV_ObjFlow`](#iv_objflow) | isolada | 9 | 726 |
| [`IV_ObjVenda`](#iv_objvenda) | vazia | 13 | 0 |
| [`IV_OcrmAgd`](#iv_ocrmagd) | isolada | 8 | 18 |
| [`IV_OcrmDest`](#iv_ocrmdest) | isolada | 12 | 17 |
| [`IV_OpBloq`](#iv_opbloq) | nucleo | 8 | 4 |
| [`IV_Operador`](#iv_operador) | catalogo | 51 | 932 |
| [`IV_OPTEMAIL`](#iv_optemail) | nucleo | 7 | 21 |
| [`IV_OPTFONE`](#iv_optfone) | nucleo | 7 | 2 |
| [`IV_OS`](#iv_os) | vazia | 22 | 0 |
| [`IV_Pessoa`](#iv_pessoa) | vazia | 6 | 0 |
| [`IV_PessoaStat`](#iv_pessoastat) | vazia | 3 | 0 |
| [`IV_PlanoAtiv`](#iv_planoativ) | vazia | 10 | 0 |
| [`IV_Produto`](#iv_produto) | nucleo | 24 | 1 |
| [`IV_ProjColec`](#iv_projcolec) | vazia | 4 | 0 |
| [`IV_ProjDocto`](#iv_projdocto) | vazia | 4 | 0 |
| [`IV_ProjEquipe`](#iv_projequipe) | vazia | 4 | 0 |
| [`IV_Projeto`](#iv_projeto) | vazia | 17 | 0 |
| [`IV_ProjPessoa`](#iv_projpessoa) | vazia | 4 | 0 |
| [`IV_PUSH`](#iv_push) | vazia | 15 | 0 |
| [`IV_RESULTADO_3110`](#iv_resultado_3110) | lixo/backup | 74 | 2.826 |
| [`IV_Selecao`](#iv_selecao) | catalogo | 12 | 543 |
| [`IV_SelecaoColList`](#iv_selecaocollist) | isolada | 3 | 35 |
| [`IV_SelecaoCriterio`](#iv_selecaocriterio) | nucleo | 7 | 2.865 |
| [`IV_SelecaoPessoa`](#iv_selecaopessoa) | nucleo | 3 | 582.481 |
| [`IV_SELPROMOPRD`](#iv_selpromoprd) | vazia | 5 | 0 |
| [`IV_SMS`](#iv_sms) | isolada | 19 | 26.749 |
| [`IV_SMSLog`](#iv_smslog) | isolada | 5 | 27.430 |
| [`IV_TC_PESSOA`](#iv_tc_pessoa) | vazia | 13 | 0 |
| [`IV_TEMPLATEPROJ`](#iv_templateproj) | vazia | 7 | 0 |
| [`IV_URADISPARO`](#iv_uradisparo) | vazia | 12 | 0 |
| [`IV_URARELATORIO`](#iv_urarelatorio) | vazia | 16 | 0 |
| [`IV_USRPUSH`](#iv_usrpush) | vazia | 15 | 0 |
| [`IV_USRSTATUS`](#iv_usrstatus) | nucleo | 7 | 186 |
| [`IV_VENDEDOR`](#iv_vendedor) | isolada | 18 | 344 |
| [`IV_VENDEDOREMPR`](#iv_vendedorempr) | isolada | 4 | 917 |
| [`IV_WHATSAPP`](#iv_whatsapp) | vazia | 21 | 0 |

---

### IV_Atendente

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario`

**Funcao:** Permissões operacionais por usuário (939 linhas × 51 colunas de flags, 370 combinações distintas) e o modelo de PERFIL que resolveria isso — nunca ativado (0 linhas em ambas). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `SeqPerfil` | `decimal(4,0)` | **nao** | - | FK -> `IV_SegPerfil.SeqPerfil` |

---

### IV_Ativ

`classe: vazia` · `17 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoAtiv`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de atividade, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoAtiv` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_PlanoAtiv.SeqPlanoAtiv` |
| 2 | `Procedimento` | `text(2147483647)` | sim | - |  |
| 3 | `Recurso` | `varchar(20)` | sim | - |  |
| 4 | `TempoEstimado` | `decimal(6,0)` | sim | - |  |
| 5 | `TempoReal` | `decimal(6,0)` | sim | - |  |
| 6 | `TempoRealTot` | `decimal(6,0)` | sim | - |  |
| 7 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 8 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 9 | `DtaPlanejada` | `datetime` | sim | - |  |
| 10 | `DtaConfirmada` | `datetime` | sim | - |  |
| 11 | `DtaUltAndamento` | `datetime` | sim | - |  |
| 12 | `Realizada` | `numeric(1,0)` | sim | - |  |
| 13 | `StatInc` | `varchar(8)` | sim | - |  |
| 14 | `RecursoCliente` | `varchar(20)` | sim | - |  |
| 15 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 16 | `TipoFaturamento` | `char(1)` | sim | - |  |
| 17 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |

**Referenciada por:** `IV_AtivAgenda.SeqPlanoAtiv`, `IV_AtivProc.SeqPlanoAtiv`

---

### IV_ATIVGRUPO

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQATIVGRUPO`

**Funcao:** _(inferido)_ Pelo nome, e um agrupamento relacionada a atividade, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQATIVGRUPO` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `GRUPO` | `varchar(100)` | **nao** | - |  |
| 3 | `SEQATIVMODELO` | `numeric(6,0)` | **nao** | - | FK -> `IV_ATIVMODELO.SEQATIVMODELO` |
| 4 | `ORDEM` | `numeric(4,0)` | **nao** | - |  |

**Referenciada por:** `IV_ATIVPADRAO.SEQATIVGRUPO`

---

### IV_ATIVMODELO

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQATIVMODELO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de atividade, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQATIVMODELO` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `MODELOPROJETO` | `varchar(50)` | **nao** | - |  |
| 3 | `ORDEM` | `numeric(4,0)` | **nao** | - |  |

**Referenciada por:** `IV_ATIVGRUPO.SEQATIVMODELO`

---

### IV_ATIVPADRAO

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQATIVPADRAO`

**Funcao:** _(inferido)_ Pelo nome, e um texto/modelo padrao relacionada a atividade, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQATIVPADRAO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQATIVGRUPO` | `numeric(6,0)` | **nao** | - | FK -> `IV_ATIVGRUPO.SEQATIVGRUPO` |
| 3 | `DESCRICAO` | `varchar(250)` | **nao** | - | descricao do registro |
| 4 | `TEMPOESTIMADO` | `numeric(6,0)` | sim | - |  |
| 5 | `RECURSO` | `varchar(20)` | sim | - |  |
| 6 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 8 | `ORDEM` | `numeric(4,0)` | **nao** | - |  |

---

### IV_BaseInformacao

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqConh`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de acao (tipo de tarefa), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqConh` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Conhecimento.SeqConh` |
| 2 | `FonteTipo` | `varchar(20)` | sim | - |  |
| 3 | `FonteTamanho` | `decimal(2,0)` | sim | - |  |
| 4 | `FonteCor` | `numeric(18,0)` | sim | - |  |
| 5 | `FundoCor` | `numeric(18,0)` | sim | - |  |
| 6 | `Conteudo` | `text(2147483647)` | sim | - |  |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_BonusCC

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqBonusCC`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de bonus/comissao, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqBonusCC` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `DtaMovto` | `datetime` | sim | - |  |
| 4 | `Descricao` | `varchar(150)` | sim | - | descricao do registro |
| 5 | `VlrMovto` | `numeric(14,2)` | sim | - |  |
| 6 | `LinkDocto` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 7 | `LinkTipo` | `varchar(12)` | sim | - |  |
| 8 | `TipoLcto` | `char(1)` | sim | - |  |

---

### IV_Campanha

`classe: isolada` · `33 colunas` · `179 linhas (snapshot 03/06/2026)` · `PK: SeqCampanha`

**Funcao:** Módulo de campanha multicanal com priorização de canal (e-mail, SMS, correio, AllIn, Mail2Easy), janela de horário por canal, voucher e custo de mídia — 179 campanhas cadastradas mas IV_CampPessoa vazia (módulo morto na prática). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCampanha` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Campanha` | `varchar(60)` | sim | - |  |
| 3 | `Descricao` | `varchar(200)` | sim | - | descricao do registro |
| 4 | `Detalhe` | `varchar(500)` | sim | - |  |
| 5 | `Grupo` | `varchar(20)` | sim | - |  |
| 6 | `DtaInicio` | `datetime` | sim | - |  |
| 7 | `DtaFim` | `datetime` | sim | - |  |
| 8 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 9 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 10 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 11 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 12 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 13 | `ProvedorSmsPL` | `numeric(18,0)` | sim | - |  |
| 14 | `Resultado` | `numeric(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 15 | `PrioridadeMail2Easy` | `char(1)` | sim | - |  |
| 16 | `DetHistorico` | `varchar(250)` | sim | - |  |
| 17 | `PRIORIDADEALLIN` | `char(1)` | sim | - |  |
| 18 | `PRIORIDADEEMAIL` | `char(1)` | sim | - |  |
| 19 | `PRIORIDADESMS` | `char(1)` | sim | - |  |
| 20 | `PRIORIDADECORREIO` | `char(1)` | sim | - |  |
| 21 | `SEQTXTPADRAO` | `numeric(18,0)` | sim | - |  |
| 22 | `INDINTERATIVO` | `numeric(1,0)` | sim | - |  |
| 23 | `CODCAMPANHA` | `varchar(20)` | sim | - |  |
| 24 | `SEQCONTEUDO` | `numeric(6,0)` | sim | - |  |
| 25 | `SEQFORMULARIO` | `numeric(18,0)` | sim | - | formulario (`IV_Formulario.SeqFormulario`) |
| 26 | `SEQEMAILTEMPLATE` | `numeric(18,0)` | sim | - |  |
| 27 | `PROVEDOREMAILPL` | `numeric(18,0)` | sim | - |  |
| 28 | `EMAILHORAINI` | `varchar(5)` | sim | - |  |
| 29 | `EMAILHORAFIM` | `varchar(5)` | sim | - |  |
| 30 | `SMSHORAINI` | `varchar(5)` | sim | - |  |
| 31 | `SMSHORAFIM` | `varchar(5)` | sim | - |  |
| 32 | `QTDMSG` | `numeric(18,0)` | sim | - |  |
| 33 | `IDULTUPLOAD` | `varchar(50)` | sim | - |  |

---

### IV_CampPesMsg

`classe: vazia` · `22 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCampPessoa, Canal`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de campanha de marketing, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCampPessoa` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_CampPessoa.SeqCampPessoa` |
| 2 | `Canal` | `varchar(12)` | **nao** | - | **PK** |
| 3 | `CanalDestino` | `varchar(100)` | sim | - |  |
| 4 | `Mensagem` | `text(2147483647)` | sim | - |  |
| 5 | `DtaEnviar` | `datetime` | sim | - |  |
| 6 | `DtaEnvio` | `datetime` | sim | - |  |
| 7 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 8 | `StatusMsg` | `varchar(250)` | sim | - |  |
| 9 | `DtaLeitura` | `datetime` | sim | - |  |
| 10 | `DtaClique` | `datetime` | sim | - |  |
| 11 | `IndSpam` | `numeric(1,0)` | sim | - |  |
| 12 | `IndDescadastro` | `numeric(1,0)` | sim | - |  |
| 13 | `IndEntregue` | `numeric(1,0)` | sim | - |  |
| 14 | `IndEnviado` | `numeric(1,0)` | sim | - |  |
| 15 | `IndLido` | `numeric(1,0)` | sim | - |  |
| 16 | `IndClique` | `numeric(1,0)` | sim | - |  |
| 17 | `IndErro` | `numeric(1,0)` | sim | - |  |
| 18 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 19 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 20 | `SEQCAMPANHA` | `numeric(6,0)` | **nao** | - |  |
| 21 | `SEQCONTATO` | `numeric(6,0)` | **nao** | - |  |
| 22 | `DTAGERACAO` | `datetime` | sim | - |  |

---

### IV_CampPessoa

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCampPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCampPessoa` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCampanha` | `numeric(6,0)` | **nao** | - |  |
| 3 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `SeqContato` | `numeric(6,0)` | sim | - |  |
| 5 | `DtaGeracao` | `datetime` | sim | - |  |

**Referenciada por:** `IV_CampPesMsg.SeqCampPessoa`, `IV_CampVoucher.SeqCampPessoa`

---

### IV_CAMPSELECAO

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCAMPANHA, SEQSELECAO`

**Funcao:** **Relacoes:** IV_CAMPSELECAO liga campanha↔IV_Selecao; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCAMPANHA` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `SEQSELECAO` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `USAPESSOA` | `numeric(1,0)` | sim | - |  |
| 4 | `USACONTATO` | `numeric(1,0)` | sim | - |  |
| 5 | `USAPESSOAGE` | `numeric(1,0)` | sim | - |  |
| 6 | `USAPESSOAPES` | `numeric(1,0)` | sim | - |  |
| 7 | `PAPEIS` | `varchar(1000)` | sim | - |  |
| 8 | `STATUS` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 9 | `DTAGERACAO` | `datetime` | sim | - |  |
| 10 | `DTAULTSTATUS` | `datetime` | sim | - |  |
| 11 | `USATODOSCANAIS` | `numeric(1,0)` | sim | - |  |

---

### IV_CampVoucher

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCampPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de campanha de marketing, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCampPessoa` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_CampPessoa.SeqCampPessoa` |
| 2 | `DtaUso` | `datetime` | sim | - |  |
| 3 | `NroVoucher` | `varchar(50)` | sim | - |  |
| 4 | `IndUso` | `numeric(1,0)` | sim | - |  |
| 5 | `ChaveBusca` | `varchar(30)` | sim | - |  |
| 6 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 7 | `SEQCAMPANHA` | `numeric(6,0)` | **nao** | - |  |
| 8 | `SEQCONTATO` | `numeric(6,0)` | **nao** | - |  |
| 9 | `DTAGERACAO` | `datetime` | sim | - |  |

---

### IV_CartContratante

`classe: vazia` · `14 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqContratante`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqContratante` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `Contratante` | `varchar(40)` | **nao** | - |  |
| 3 | `DescContratante` | `varchar(100)` | **nao** | - |  |
| 4 | `SeqCartProd` | `numeric(6,0)` | **nao** | - | FK -> `IV_CartProduto.SeqCartProd` |
| 5 | `SeqPessoaGrupo` | `numeric(10,0)` | **nao** | - |  |
| 6 | `SeqPessoaCtr` | `numeric(10,0)` | **nao** | - |  |
| 7 | `NroConta` | `numeric(18,0)` | sim | - |  |
| 8 | `IDOrigemCml` | `numeric(18,0)` | sim | - |  |
| 9 | `NroEstabelecimento` | `numeric(18,0)` | sim | - |  |
| 10 | `IndUsoEmProposta` | `numeric(1,0)` | sim | - |  |
| 11 | `IndProdutoPadrao` | `numeric(1,0)` | sim | - |  |
| 12 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 13 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 14 | `ChkSum` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `IV_CartLoteImp.SeqContratante`, `IV_CartProposta.SeqContratante`, `IV_CartTitular.SeqContratante`

---

### IV_CartCred

`classe: vazia` · `36 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCartCred`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCartCred` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCartTitular` | `numeric(18,0)` | **nao** | - | FK -> `IV_CartTitular.SeqCartTitular` |
| 3 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `NroCartao` | `varchar(50)` | sim | - |  |
| 5 | `NroCartaoX` | `varchar(24)` | sim | - |  |
| 6 | `NroCartao6` | `varchar(6)` | sim | - |  |
| 7 | `Validade` | `numeric(4,0)` | sim | - |  |
| 8 | `Situacao` | `char(1)` | sim | - |  |
| 9 | `SeqCFinFis` | `numeric(18,0)` | sim | - |  |
| 10 | `Tipo` | `char(1)` | sim | - |  |
| 11 | `NomeImpresso` | `varchar(30)` | sim | - |  |
| 12 | `TipoRelacSeqPar` | `numeric(18,0)` | sim | - |  |
| 13 | `Nome` | `varchar(50)` | sim | - |  |
| 14 | `Sexo` | `char(1)` | sim | - |  |
| 15 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 16 | `FoneNro1` | `numeric(12,0)` | sim | - |  |
| 17 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 18 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 19 | `FoneNro2` | `numeric(12,0)` | sim | - |  |
| 20 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 21 | `NroCPF` | `numeric(13,0)` | sim | - |  |
| 22 | `DigCPF` | `numeric(2,0)` | sim | - |  |
| 23 | `RGNro` | `varchar(15)` | sim | - |  |
| 24 | `RGOrgaoEmissor` | `varchar(12)` | sim | - |  |
| 25 | `RGUFEmissao` | `char(2)` | sim | - |  |
| 26 | `RGDtaEmissao` | `datetime` | sim | - |  |
| 27 | `DtaNascimento` | `datetime` | sim | - |  |
| 28 | `DtaImpressao` | `datetime` | sim | - |  |
| 29 | `DtaUltImpressao` | `datetime` | sim | - |  |
| 30 | `NacionalSeqPar` | `numeric(18,0)` | sim | - |  |
| 31 | `EstadoCivilSeqPar` | `numeric(18,0)` | sim | - |  |
| 32 | `ChkSum` | `numeric(18,0)` | sim | - |  |
| 33 | `CVV` | `varchar(3)` | sim | - |  |
| 34 | `Trilha2Cmpl` | `varchar(16)` | sim | - |  |
| 35 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 36 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |

**Referenciada por:** `IV_CartLoteCartao.SeqCartCred`

---

### IV_CartLoteCartao

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCartLote, SeqCartCred`

**Funcao:** _(inferido)_ Pelo nome, e um processamento em lote relacionada a carteira de clientes, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCartLote` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_CartLoteImp.SeqCartLote` |
| 2 | `SeqCartCred` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_CartCred.SeqCartCred` |
| 3 | `IndImpressaoOk` | `numeric(1,0)` | sim | - |  |
| 4 | `Via` | `numeric(2,0)` | sim | - |  |
| 5 | `ChkSum` | `numeric(18,0)` | sim | - |  |

---

### IV_CartLoteImp

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCartLote`

**Funcao:** _(inferido)_ Pelo nome, e um processamento em lote relacionada a carteira de clientes, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCartLote` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqContratante` | `numeric(10,0)` | **nao** | - | FK -> `IV_CartContratante.SeqContratante` |
| 3 | `DescLote` | `varchar(30)` | sim | - |  |
| 4 | `TipoLote` | `char(1)` | sim | - |  |
| 5 | `Situacao` | `char(1)` | sim | - |  |
| 6 | `DtaImpressao` | `datetime` | sim | - |  |
| 7 | `UsuImpressao` | `varchar(20)` | sim | - |  |

**Referenciada por:** `IV_CartLoteCartao.SeqCartLote`

---

### IV_CartMarca

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCartMarca`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCartMarca` | `numeric(4,0)` | **nao** | - | **PK** |
| 2 | `Marca` | `varchar(30)` | sim | - |  |
| 3 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 4 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 5 | `ChkSum` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `IV_CartProduto.SeqCartMarca`

---

### IV_CARTPESORIGEM

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCARTPESORIGEM`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCARTPESORIGEM` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQPROPOSTA` | `numeric(18,0)` | **nao** | - | FK -> `IV_CartProposta.SeqProposta` |
| 3 | `ORIGEM` | `varchar(30)` | sim | - | sistema de origem do dado |
| 4 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |

---

### IV_CARTPESSOA

`classe: vazia` · `53 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPROPOSTA`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPROPOSTA` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_CartProposta.SeqProposta` |
| 2 | `NOMEMAE` | `varchar(50)` | sim | - |  |
| 3 | `NOMEPAI` | `varchar(50)` | sim | - |  |
| 4 | `RGINSCRICAO` | `varchar(20)` | sim | - |  |
| 5 | `RGUFEMISSOR` | `varchar(2)` | sim | - |  |
| 6 | `RGORGAOEMISSOR` | `varchar(10)` | sim | - |  |
| 7 | `RGDTAEMISSAO` | `datetime` | sim | - |  |
| 8 | `CNHNRO` | `numeric(18,0)` | sim | - |  |
| 9 | `CNHDTAVALIDADE` | `datetime` | sim | - |  |
| 10 | `CNHDTAEMISSAO` | `datetime` | sim | - |  |
| 11 | `CARTTRABNRO` | `numeric(18,0)` | sim | - |  |
| 12 | `CARTTRABSERIE` | `varchar(10)` | sim | - |  |
| 13 | `CARTTRABUF` | `varchar(2)` | sim | - |  |
| 14 | `CARTTRABDTAEMISSAO` | `datetime` | sim | - |  |
| 15 | `IDENTPROFNRO` | `varchar(20)` | sim | - |  |
| 16 | `IDENTPROFSEQPAR` | `numeric(18,0)` | sim | - |  |
| 17 | `IDENTPROFDTAEMISSAO` | `datetime` | sim | - |  |
| 18 | `CEP` | `varchar(12)` | sim | - |  |
| 19 | `UF` | `varchar(2)` | sim | - |  |
| 20 | `SEQCIDADE` | `numeric(18,0)` | sim | - |  |
| 21 | `BAIRRO` | `varchar(50)` | sim | - |  |
| 22 | `LOGRADOURO` | `varchar(80)` | sim | - |  |
| 23 | `LOGRADOURONRO` | `varchar(10)` | sim | - |  |
| 24 | `LOGRADOUROCMPL` | `varchar(30)` | sim | - |  |
| 25 | `FONECELULAR` | `numeric(18,0)` | sim | - |  |
| 26 | `FONERESIDENCIAL` | `numeric(18,0)` | sim | - |  |
| 27 | `NACIONALSEQPAR` | `numeric(18,0)` | sim | - |  |
| 28 | `NATUF` | `varchar(2)` | sim | - |  |
| 29 | `NATSEQCIDADE` | `numeric(18,0)` | sim | - |  |
| 30 | `OCPRENDA` | `numeric(15,2)` | sim | - |  |
| 31 | `OUTRARENDAVLR` | `numeric(15,2)` | sim | - |  |
| 32 | `OCPFISICAJURIDICA` | `varchar(1)` | sim | - |  |
| 33 | `NATOCUPACAOSEQPAR` | `numeric(18,0)` | sim | - |  |
| 34 | `ATIVIDADESEQPAR` | `numeric(18,0)` | sim | - |  |
| 35 | `ESTADOCIVILSEQPAR` | `numeric(18,0)` | sim | - |  |
| 36 | `GRAUINSTRSEQPAR` | `numeric(18,0)` | sim | - |  |
| 37 | `DTANASCIMENTO` | `datetime` | sim | - |  |
| 38 | `SEXO` | `varchar(1)` | sim | - |  |
| 39 | `EMAIL` | `varchar(70)` | sim | - |  |
| 40 | `QTDDEPENDENTE` | `numeric(2,0)` | sim | - |  |
| 41 | `CPF` | `numeric(11,0)` | sim | - |  |
| 42 | `NOME` | `varchar(60)` | sim | - |  |
| 43 | `FOTO` | `text(2147483647)` | sim | - |  |
| 44 | `INDPROCESSADO` | `numeric(1,0)` | sim | - |  |
| 45 | `TIPODOCTOEXIGIDO` | `varchar(3)` | sim | - |  |
| 46 | `ENTIDADEPROPOSTA` | `varchar(20)` | sim | - |  |
| 47 | `FONERECADO` | `numeric(18,0)` | sim | - |  |
| 48 | `CRNM` | `varchar(20)` | sim | - |  |
| 49 | `CRNMDTAEMISSAO` | `datetime` | sim | - |  |
| 50 | `CRNMDTAVALIDADE` | `datetime` | sim | - |  |
| 51 | `ABORDAGEMTIPO` | `varchar(1)` | sim | - |  |
| 52 | `ABORDAGEMLOCAL` | `varchar(50)` | sim | - |  |
| 53 | `CERTIFACEAPPKEY` | `varchar(1000)` | sim | - |  |

---

### IV_CartProduto

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCartProd`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCartProd` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `SeqCartMarca` | `numeric(4,0)` | **nao** | - | FK -> `IV_CartMarca.SeqCartMarca` |
| 3 | `DescProduto` | `varchar(40)` | sim | - |  |
| 4 | `IndAltDadoAprovacao` | `numeric(1,0)` | sim | - |  |
| 5 | `QtdeMaxCartAdicional` | `numeric(2,0)` | sim | - |  |
| 6 | `ModeloImpressao` | `varchar(10)` | sim | - |  |
| 7 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 8 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 9 | `ChkSum` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `IV_CartContratante.SeqCartProd`

---

### IV_CartProposta

`classe: vazia` · `30 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProposta`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`), escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProposta` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCFinFis` | `numeric(18,0)` | **nao** | - |  |
| 3 | `SeqContratante` | `numeric(10,0)` | **nao** | - | FK -> `IV_CartContratante.SeqContratante` |
| 4 | `PercLojIndicSeqPar` | `numeric(18,0)` | sim | - |  |
| 5 | `PercLojObs` | `varchar(250)` | sim | - |  |
| 6 | `SituacaoSeqPar` | `numeric(18,0)` | sim | - |  |
| 7 | `OrigemSeqPar` | `numeric(18,0)` | sim | - |  |
| 8 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 9 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 10 | `TipoEndCorresp` | `char(1)` | sim | - |  |
| 11 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 12 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 13 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 14 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 15 | `UsuPromotor` | `varchar(20)` | sim | - |  |
| 16 | `IndRecusaCliente` | `numeric(1,0)` | sim | - |  |
| 17 | `MotRecusaCliSeqPar` | `numeric(18,0)` | sim | - |  |
| 18 | `DtaEnvioFin` | `datetime` | sim | - |  |
| 19 | `DtaRespostaFin` | `datetime` | sim | - |  |
| 20 | `MotivoRecusa` | `varchar(15)` | sim | - |  |
| 21 | `IdProposta` | `varchar(20)` | sim | - |  |
| 22 | `RetObs` | `varchar(250)` | sim | - |  |
| 23 | `RetStatus` | `varchar(50)` | sim | - |  |
| 24 | `OBS` | `varchar(250)` | sim | - | texto livre |
| 25 | `RETPROPOBS` | `varchar(250)` | sim | - |  |
| 26 | `RETPROPSTATUS` | `varchar(50)` | sim | - |  |
| 27 | `DTARESPOSTAPROP` | `datetime` | sim | - |  |
| 28 | `INDRETPROCESSADO` | `numeric(1,0)` | sim | - |  |
| 29 | `INDRETPROPPROCESSADO` | `numeric(1,0)` | sim | - |  |
| 30 | `NEUROTECH_LOG_ID` | `numeric(11,0)` | sim | - |  |

**Referenciada por:** `IV_CARTPESORIGEM.SEQPROPOSTA`, `IV_CARTPESSOA.SEQPROPOSTA`, `IV_CartTitular.SeqProposta`

---

### IV_CartTitular

`classe: vazia` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCartTitular`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCartTitular` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqContratante` | `numeric(10,0)` | **nao** | - | FK -> `IV_CartContratante.SeqContratante` |
| 3 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `SeqProposta` | `numeric(18,0)` | sim | - | FK -> `IV_CartProposta.SeqProposta` |
| 5 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 6 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 7 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 8 | `NroConta` | `numeric(18,0)` | sim | - |  |
| 9 | `DiaVencto` | `numeric(2,0)` | sim | - |  |
| 10 | `VlrLimiteAprovado` | `numeric(14,2)` | sim | - |  |
| 11 | `Validade` | `numeric(4,0)` | sim | - |  |
| 12 | `OBS` | `varchar(250)` | sim | - | texto livre |

**Referenciada por:** `IV_CartCred.SeqCartTitular`

---

### IV_CbrCobranca

`classe: nucleo` · `8 colunas` · `64.449 linhas (snapshot 03/06/2026)` · `PK: SeqCbrCobranca`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de cobranca, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com processo (`Processo`), vinculo com agenda (`SeqAgenda`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCbrCobranca` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `SeqAgenda` | `numeric(18,0)` | sim | - | FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 4 | `SeqCbrCriterio` | `numeric(6,0)` | sim | - | FK -> `IV_CbrCriterio.SeqCbrCriterio` |
| 5 | `Lote` | `numeric(18,0)` | **nao** | - | FK -> `IV_CbrCobrancaLote.Lote` |
| 6 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 7 | `Dta1Cob` | `datetime` | sim | - |  |
| 8 | `Dta2Cob` | `datetime` | sim | - |  |

**Referenciada por:** `IV_CbrCobrancaMon.SeqCbrCobranca`, `IV_CbrCobrancaTit.SeqCbrCobranca`

---

### IV_CBRCOBRANCAHST

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCBRCOBRHST`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCBRCOBRHST` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCBRCOBRANCA` | `numeric(18,0)` | **nao** | - |  |
| 3 | `SEQCBRCRITMSG` | `numeric(8,0)` | sim | - |  |
| 4 | `DATA` | `datetime` | sim | - |  |
| 5 | `OBS` | `varchar(150)` | sim | - | texto livre |

---

### IV_CbrCobrancaLote

`classe: nucleo` · `4 colunas` · `21.048 linhas (snapshot 03/06/2026)` · `PK: Lote`

**Funcao:** _(inferido)_ Pelo nome, e um processamento em lote relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Lote` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `DtaGeracao` | `datetime` | sim | - |  |
| 3 | `SeqCbrCriterio` | `numeric(6,0)` | **nao** | - | FK -> `IV_CbrCriterio.SeqCbrCriterio` |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_CbrCobranca.Lote`

---

### IV_CbrCobrancaMon

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCbrCobrancaMonit`

**Funcao:** _(inferido)_ Pelo nome, e um monitoramento relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCbrCobrancaMonit` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCbrCobranca` | `numeric(18,0)` | **nao** | - | FK -> `IV_CbrCobranca.SeqCbrCobranca` |
| 3 | `DtaAnalise` | `datetime` | **nao** | - |  |
| 4 | `VlrAnterior` | `numeric(14,2)` | sim | - |  |
| 5 | `VlrNovo` | `numeric(14,2)` | sim | - |  |
| 6 | `VlrBaixado` | `numeric(14,2)` | sim | - |  |
| 7 | `VlrAlterado` | `numeric(14,2)` | sim | - |  |
| 8 | `QtdAnterior` | `numeric(4,0)` | sim | - |  |
| 9 | `QtdTitNovo` | `numeric(4,0)` | sim | - |  |
| 10 | `QtdTitBaixado` | `numeric(4,0)` | sim | - |  |
| 11 | `QtdTitAlterado` | `numeric(4,0)` | sim | - |  |

---

### IV_CbrCobrancaTit

`classe: nucleo` · `10 colunas` · `155.204 linhas (snapshot 03/06/2026)` · `PK: SeqCbrCobranca, idTitulo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCbrCobranca` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_CbrCobranca.SeqCbrCobranca` |
| 2 | `idTitulo` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `MotivoSaida` | `char(1)` | sim | - |  |
| 4 | `IndQuitado` | `numeric(1,0)` | sim | - |  |
| 5 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 6 | `DtaSaida` | `datetime` | sim | - |  |
| 7 | `VlrOriginal` | `numeric(14,2)` | sim | - |  |
| 8 | `VlrPago` | `numeric(14,2)` | sim | - |  |
| 9 | `VlrAberto` | `numeric(14,2)` | sim | - |  |
| 10 | `DtaVencto` | `datetime` | sim | - |  |

**Referenciada por:** `IV_CbrCobrancaTitLog.SeqCbrCobranca`, `IV_CbrCobrancaTitLog.idTitulo`

---

### IV_CbrCobrancaTitLog

`classe: nucleo` · `5 colunas` · `304.684 linhas (snapshot 03/06/2026)` · `PK: SeqCbrCobLog`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCbrCobLog` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCbrCobranca` | `numeric(18,0)` | sim | - | FK -> `IV_CbrCobrancaTit.SeqCbrCobranca` |
| 3 | `idTitulo` | `numeric(18,0)` | sim | - | FK -> `IV_CbrCobrancaTit.idTitulo` |
| 4 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 5 | `DtaLog` | `datetime` | sim | - |  |

---

### IV_CbrCriterio

`classe: catalogo` · `26 colunas` · `122 linhas (snapshot 03/06/2026)` · `PK: SeqCbrCriterio`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCbrCriterio` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `Criterio` | `varchar(30)` | sim | - |  |
| 3 | `NroEmprAgenda` | `numeric(6,0)` | sim | - |  |
| 4 | `VlrPrior0` | `numeric(14,2)` | sim | - |  |
| 5 | `VlrPrior1` | `numeric(14,2)` | sim | - |  |
| 6 | `SeqTxt1CobeMail` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 7 | `Dias1Cob` | `numeric(2,0)` | sim | - |  |
| 8 | `SeqTxt1CobSMS` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 9 | `SeqTxt2CobeMail` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 10 | `Dias2Cob` | `numeric(2,0)` | sim | - |  |
| 11 | `SeqTxt2CobSMS` | `numeric(18,0)` | sim | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 12 | `DiasGerarAgenda` | `numeric(2,0)` | sim | - |  |
| 13 | `IndExecAutomatica` | `numeric(1,0)` | sim | - |  |
| 14 | `IndAceitaNovoTitulo` | `numeric(1,0)` | sim | - |  |
| 15 | `Acao` | `numeric(6,0)` | sim | - | acao (`IV_Acao.Acao`) |
| 16 | `ResultadoBaixa` | `numeric(6,0)` | sim | - |  |
| 17 | `PapelDestino` | `varchar(250)` | sim | - |  |
| 18 | `SeqUsrDestino` | `numeric(18,0)` | sim | - | FK -> `GE_Usuario.SeqUsuario` |
| 19 | `RELACDESTINO` | `varchar(150)` | sim | - |  |
| 20 | `INDAVISOINDIVIDUALSM` | `numeric(1,0)` | sim | - |  |
| 21 | `resultadosaida` | `numeric(6,0)` | sim | - |  |
| 22 | `VLRMINIMO` | `numeric(14,2)` | sim | - |  |
| 23 | `INDAGDAVULSA` | `numeric(1,0)` | sim | - |  |
| 24 | `INDAVISOINDIVIDSMS` | `numeric(1,0)` | sim | - |  |
| 25 | `INDGERAAVISODIAUTL` | `numeric(1,0)` | sim | - |  |
| 26 | `EMAILREMETENTE` | `varchar(100)` | sim | - |  |

**Referenciada por:** `IV_CBRCRITMON.SEQCBRCRITERIO`, `IV_CbrCobranca.SeqCbrCriterio`, `IV_CbrCobrancaLote.SeqCbrCriterio`, `IV_CbrCriterioDef.SeqCbrCriterio`

---

### IV_CbrCriterioDef

`classe: nucleo` · `16 colunas` · `231 linhas (snapshot 03/06/2026)` · `PK: SeqCbrCritDef`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCbrCritDef` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCbrCriterio` | `numeric(6,0)` | **nao** | - | FK -> `IV_CbrCriterio.SeqCbrCriterio` |
| 3 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 4 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 5 | `Departamento` | `varchar(30)` | sim | - | departamento |
| 6 | `Especie` | `varchar(20)` | sim | - |  |
| 7 | `TipoCobranca` | `varchar(20)` | sim | - |  |
| 8 | `IndCobrJuridica` | `numeric(1,0)` | sim | - |  |
| 9 | `LocalCobranca` | `varchar(25)` | sim | - |  |
| 10 | `VlrAberto` | `numeric(14,2)` | sim | - |  |
| 11 | `DiasAbertoDe` | `numeric(2,0)` | sim | - |  |
| 12 | `DiasAbertoAte` | `numeric(2,0)` | sim | - |  |
| 13 | `AtividadePessoa` | `varchar(30)` | sim | - |  |
| 14 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 15 | `diasvencidode` | `numeric(4,0)` | sim | - |  |
| 16 | `diasvencidoate` | `numeric(4,0)` | sim | - |  |

---

### IV_CBRCRITERIOMSG

`classe: isolada` · `6 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SEQCBRCRITMSG`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCBRCRITMSG` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `SEQCBRCRITERIO` | `numeric(6,0)` | sim | - |  |
| 3 | `DIAS1COB` | `numeric(2,0)` | sim | - |  |
| 4 | `INDENVIOALTERNATIVO` | `numeric(1,0)` | sim | - |  |
| 5 | `SEQTXTSMS` | `numeric(18,0)` | sim | - |  |
| 6 | `SEQTXTEMAIL` | `numeric(18,0)` | sim | - |  |

---

### IV_CBRCRITMON

`classe: nucleo` · `17 colunas` · `128.167 linhas (snapshot 03/06/2026)` · `PK: SEQCBRCRITERIOMON`

**Funcao:** _(inferido)_ Pelo nome, e um monitoramento relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCBRCRITERIOMON` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCBRCRITERIO` | `numeric(6,0)` | **nao** | - | FK -> `IV_CbrCriterio.SeqCbrCriterio` |
| 3 | `DTAANALISE` | `datetime` | **nao** | - |  |
| 4 | `DTAULTANALISE` | `datetime` | sim | - |  |
| 5 | `VLRNOVO` | `numeric(14,2)` | sim | - |  |
| 6 | `VLRBAIXADO` | `numeric(14,2)` | sim | - |  |
| 7 | `VLRALTERADO` | `numeric(14,2)` | sim | - |  |
| 8 | `VLRMIGRADO` | `numeric(14,2)` | sim | - |  |
| 9 | `QTDTITNOVO` | `numeric(6,0)` | sim | - |  |
| 10 | `QTDTITBAIXADO` | `numeric(6,0)` | sim | - |  |
| 11 | `QTDTITMIGRADO` | `numeric(6,0)` | sim | - |  |
| 12 | `QTDPESNOVO` | `numeric(6,0)` | sim | - |  |
| 13 | `QTDPESBAIXADO` | `numeric(6,0)` | sim | - |  |
| 14 | `QTDPESALTERADO` | `numeric(6,0)` | sim | - |  |
| 15 | `QTDPESMIGRADO` | `numeric(6,0)` | sim | - |  |
| 16 | `USUANALISE` | `varchar(20)` | sim | - |  |
| 17 | `QTDTITALTERADO` | `numeric(6,0)` | sim | - |  |

---

### IV_CbrTitulo

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: idTitulo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `idTitulo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IndNaoCobravel` | `numeric(1,0)` | sim | - |  |
| 3 | `Motivo` | `varchar(100)` | sim | - |  |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 5 | `DtaUltAlteracao` | `datetime` | sim | - |  |

---

### IV_CBRTITULOHST

`classe: isolada` · `5 colunas` · `173.530 linhas (snapshot 03/06/2026)` · `PK: SEQCBRTITHST`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a titulo financeiro, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCBRTITHST` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IDTITULO` | `numeric(18,0)` | sim | - |  |
| 3 | `DATA` | `date` | sim | - |  |
| 4 | `TIPOHST` | `varchar(12)` | sim | - |  |
| 5 | `OBS` | `varchar(150)` | sim | - | texto livre |

---

### IV_CFinDocAceito

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: DocReqSeqPar, SeqDocTp`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `DocReqSeqPar` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqDocTp` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_CobrCrit

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Seq`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`), escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `Acao` | `decimal(6,0)` | sim | - | FK -> `IV_Acao.Acao`; acao (`IV_Acao.Acao`) |
| 4 | `Resultado` | `decimal(6,0)` | sim | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 5 | `Descricao` | `varchar(30)` | sim | - | descricao do registro |
| 6 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 7 | `VlrPr0` | `decimal(15,2)` | sim | - |  |
| 8 | `VlrPr1` | `decimal(15,2)` | sim | - |  |
| 9 | `Conexao` | `varchar(18)` | sim | - |  |
| 10 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 11 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_CobrCritAgd.Seq`, `IV_CobrCritDef.Seq`, `IV_CobrCritMon.Seq`

---

### IV_CobrCritAgd

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAgenda`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel relacionada a agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 2 | `Seq` | `decimal(4,0)` | sim | - | FK -> `IV_CobrCrit.Seq` |
| 3 | `SeqCobCrit` | `numeric(18,0)` | sim | - | FK -> `IV_CobrCritMon.SeqCobCrit` |
| 4 | `VlrAberto` | `decimal(15,2)` | sim | - |  |
| 5 | `QtdTitulo` | `decimal(4,0)` | sim | - |  |

---

### IV_CobrCritDef

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqParam, Seq`

**Funcao:** _(inferido)_ Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqParam` | `decimal(4,0)` | **nao** | - | **PK** |
| 2 | `Seq` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_CobrCrit.Seq` |
| 3 | `NroEmpresaOrigem` | `numeric(6,0)` | sim | - |  |
| 4 | `Origem` | `varchar(30)` | sim | - | sistema de origem do dado |
| 5 | `Especie` | `varchar(30)` | sim | - |  |
| 6 | `TipoCobr` | `varchar(30)` | sim | - |  |
| 7 | `NroDias` | `decimal(3,0)` | sim | - |  |
| 8 | `NroDiasAte` | `decimal(4,0)` | sim | - |  |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_CobrCritMon

`classe: vazia` · `17 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCobCrit`

**Funcao:** _(inferido)_ Pelo nome, e um monitoramento relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCobCrit` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Seq` | `decimal(4,0)` | **nao** | - | FK -> `IV_CobrCrit.Seq` |
| 3 | `DtaAnalise` | `datetime` | **nao** | - |  |
| 4 | `DtaUltAnalise` | `datetime` | sim | - |  |
| 5 | `VlrNovo` | `decimal(15,2)` | sim | - |  |
| 6 | `VlrBaixado` | `decimal(15,2)` | sim | - |  |
| 7 | `VlrAlterado` | `decimal(15,2)` | sim | - |  |
| 8 | `VlrMigrado` | `decimal(15,2)` | sim | - |  |
| 9 | `QtdTitNovo` | `decimal(6,0)` | sim | - |  |
| 10 | `QtdTitBaixado` | `decimal(6,0)` | sim | - |  |
| 11 | `QtdTitAlterado` | `decimal(6,0)` | sim | - |  |
| 12 | `QtdTitMIgrado` | `decimal(6,0)` | sim | - |  |
| 13 | `QtdPesNovo` | `decimal(6,0)` | sim | - |  |
| 14 | `QtdPesBaixado` | `decimal(6,0)` | sim | - |  |
| 15 | `QtdPesAlterado` | `decimal(6,0)` | sim | - |  |
| 16 | `QtdPesMIgrado` | `decimal(6,0)` | sim | - |  |
| 17 | `UsuAnalise` | `varchar(20)` | sim | - |  |

**Referenciada por:** `IV_CobrCritAgd.SeqCobCrit`

---

### IV_CobrTit

`classe: vazia` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: IdTitulo, Seq`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de titulo financeiro, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`), escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `IdTitulo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Seq` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 4 | `DtaVencto` | `datetime` | sim | - |  |
| 5 | `VlrAberto` | `decimal(15,2)` | sim | - |  |
| 6 | `Quitado` | `numeric(1,0)` | sim | - |  |
| 7 | `NroEmpresa` | `decimal(6,0)` | sim | - | multiempresa - filial/empresa |
| 8 | `Titulo` | `varchar(40)` | sim | - |  |
| 9 | `DtaAnalise` | `datetime` | **nao** | - |  |
| 10 | `Obs` | `varchar(100)` | sim | - | texto livre |
| 11 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |

---

### IV_ConhecFonema

`classe: nucleo` · `2 colunas` · `19 linhas (snapshot 03/06/2026)` · `PK: SeqConh, Particula`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqConh` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Conhecimento.SeqConh` |
| 2 | `Particula` | `varchar(20)` | **nao** | - | **PK** |

---

### IV_Conhecimento

`classe: catalogo` · `19 colunas` · `9 linhas (snapshot 03/06/2026)` · `PK: SeqConh`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de base de conhecimento, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqConh` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `SeqConhPai` | `decimal(6,0)` | sim | - |  |
| 3 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 4 | `Conteudo` | `varchar(200)` | sim | - |  |
| 5 | `Tipo` | `char(1)` | sim | - |  |
| 6 | `Atalho` | `varchar(250)` | sim | - |  |
| 7 | `QtdeAcesso` | `numeric(18,0)` | sim | - |  |
| 8 | `DtaUltAcesso` | `datetime` | sim | - |  |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 11 | `TipoArquivo` | `char(1)` | sim | - |  |
| 12 | `AtalhoPcte` | `varchar(250)` | sim | - |  |
| 13 | `PermiteAnexar` | `char(1)` | sim | - |  |
| 14 | `PalavraChave` | `varchar(250)` | sim | - |  |
| 15 | `Versao` | `varchar(10)` | sim | - |  |
| 16 | `Dtainclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 17 | `Usuinclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 18 | `Resultado` | `decimal(6,0)` | sim | - | FK -> `IV_Resultado.Resultado`; resultado (`IV_Resultado.Resultado`) |
| 19 | `ResultadoCmpl` | `varchar(150)` | sim | - | resultado complementar (texto livre; ver defeito 2.10) |

**Referenciada por:** `IV_BaseInformacao.SeqConh`, `IV_ConhecFonema.SeqConh`, `IV_ConhecLeitura.SeqConh`

---

### IV_ConhecLeitura

`classe: nucleo` · `5 colunas` · `49 linhas (snapshot 03/06/2026)` · `PK: SeqConh, SeqUsuario`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqConh` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `IV_Conhecimento.SeqConh` |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `Lido` | `char(1)` | sim | - |  |
| 4 | `DtaUltAcesso` | `datetime` | sim | - |  |
| 5 | `QtdeAcesso` | `numeric(18,0)` | sim | - |  |

---

### IV_CustoMidia

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCustoCamp`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de midia/custo de midia, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCustoCamp` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 3 | `Campanha` | `varchar(20)` | sim | - |  |
| 4 | `DtaBase` | `datetime` | sim | - |  |
| 5 | `MesBase` | `datetime` | sim | - |  |
| 6 | `Obs` | `varchar(200)` | sim | - | texto livre |
| 7 | `Valor` | `decimal(15,2)` | sim | - |  |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_eMail

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqEmail`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com historico (`SeqHistorico`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqEmail` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqHistorico` | `numeric(18,0)` | sim | - | FK -> `IV_Historico.SeqHistorico`; historico (`IV_Historico.SeqHistorico`) |
| 3 | `SeqPessoa` | `numeric(8,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `De` | `varchar(100)` | sim | - |  |
| 5 | `Para` | `varchar(250)` | sim | - |  |
| 6 | `Cc` | `varchar(250)` | sim | - |  |
| 7 | `Cco` | `varchar(250)` | sim | - |  |
| 8 | `Assunto` | `varchar(150)` | sim | - |  |
| 9 | `Detalhe` | `varchar(1000)` | sim | - |  |
| 10 | `Arquivo` | `varchar(250)` | sim | - |  |
| 11 | `Enviado` | `numeric(1,0)` | sim | - |  |
| 12 | `FormaEnvio` | `varchar(10)` | sim | - |  |
| 13 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 14 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 15 | `DtaEnvio` | `datetime` | sim | - |  |

---

### IV_EMAILESTATISTICA

`classe: vazia` · `20 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILESTATISTICA`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEMAILESTATISTICA` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCONTAEMAIL` | `numeric(18,0)` | sim | - |  |
| 3 | `CONTEXTO` | `varchar(250)` | sim | - |  |
| 4 | `SUBCONTEXTO` | `varchar(250)` | sim | - |  |
| 5 | `LINKSTR` | `varchar(250)` | sim | - |  |
| 6 | `DE` | `varchar(150)` | sim | - |  |
| 7 | `PARA` | `varchar(250)` | sim | - |  |
| 8 | `ASSUNTO` | `varchar(250)` | sim | - |  |
| 9 | `DTAENVIO` | `datetime` | sim | - |  |
| 10 | `DTALEITURA` | `datetime` | sim | - |  |
| 11 | `DTACLIQUE` | `datetime` | sim | - |  |
| 12 | `INDSPAM` | `numeric(1,0)` | sim | - |  |
| 13 | `INDDESCADASTRO` | `numeric(1,0)` | sim | - |  |
| 14 | `INDENTREGUE` | `numeric(1,0)` | sim | - |  |
| 15 | `INDENVIADO` | `numeric(1,0)` | sim | - |  |
| 16 | `INDLIDO` | `numeric(1,0)` | sim | - |  |
| 17 | `INDCLIQUE` | `numeric(1,0)` | sim | - |  |
| 18 | `INDERRO` | `numeric(1,0)` | sim | - |  |
| 19 | `DTAGERACAO` | `datetime` | sim | - |  |
| 20 | `DTAULTATUALIZACAO` | `datetime` | sim | - |  |

---

### IV_ESTRPRODUTO

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQESTRPROD`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de produto, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQESTRPROD` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `NIVEL` | `numeric(1,0)` | **nao** | - |  |
| 3 | `DESCRICAO` | `varchar(40)` | **nao** | - | descricao do registro |
| 4 | `SEQESTRPRODPAI` | `numeric(10,0)` | sim | - | FK -> `IV_ESTRPRODUTO.SEQESTRPROD` |
| 5 | `TIPOESTRUTURA` | `char(1)` | **nao** | - |  |
| 6 | `DESCCOMPLETA` | `varchar(255)` | sim | - |  |

**Referenciada por:** `IV_ESTRPRODUTO.SEQESTRPRODPAI`, `IV_Produto.SEQESTRPROD`

---

### IV_FichaNegVeic

`classe: vazia` · `79 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Processo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de veiculo/equipamento, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Processo` | `numeric(18,0)` | **nao** | - | **PK**; numero do processo (`IV_Processo.Processo`) |
| 2 | `Filial` | `decimal(6,0)` | sim | - |  |
| 3 | `PedVenda` | `decimal(10,0)` | sim | - |  |
| 4 | `DtaFaturamento` | `datetime` | sim | - |  |
| 5 | `SeqPlanoMelhor` | `numeric(18,0)` | sim | - |  |
| 6 | `Avaliacao` | `decimal(10,0)` | sim | - |  |
| 7 | `TipoVeiculo` | `varchar(5)` | sim | - |  |
| 8 | `TipoPgto` | `varchar(15)` | sim | - |  |
| 9 | `NovoUsado` | `char(1)` | sim | - |  |
| 10 | `ApresentDinamica` | `varchar(2)` | sim | - |  |
| 11 | `ApresentEstatica` | `varchar(2)` | sim | - |  |
| 12 | `MarcaInteresse` | `varchar(30)` | sim | - |  |
| 13 | `FamiliaInteresse` | `varchar(30)` | sim | - |  |
| 14 | `ModeloInteresse` | `varchar(40)` | sim | - |  |
| 15 | `IntAnoFabr` | `decimal(4,0)` | sim | - |  |
| 16 | `IntAnoMod` | `decimal(4,0)` | sim | - |  |
| 17 | `MarcaEscolhida` | `varchar(30)` | sim | - |  |
| 18 | `ModeloEscolhido` | `varchar(40)` | sim | - |  |
| 19 | `CodModEscolhido` | `varchar(30)` | sim | - |  |
| 20 | `VlrInteresseDe` | `decimal(15,2)` | sim | - |  |
| 21 | `VlrInteresseAte` | `decimal(15,2)` | sim | - |  |
| 22 | `Cor1` | `varchar(30)` | sim | - |  |
| 23 | `Cor2` | `varchar(30)` | sim | - |  |
| 24 | `CorInterna` | `varchar(30)` | sim | - |  |
| 25 | `NroPortas` | `decimal(1,0)` | sim | - |  |
| 26 | `VlrTabela` | `decimal(15,2)` | sim | - |  |
| 27 | `VlrProposto` | `decimal(15,2)` | sim | - |  |
| 28 | `VlrEntrada` | `decimal(15,2)` | sim | - |  |
| 29 | `VlrPrestacao` | `decimal(15,2)` | sim | - |  |
| 30 | `VlrSeguro` | `decimal(15,2)` | sim | - |  |
| 31 | `VlrGarExtendida` | `decimal(15,2)` | sim | - |  |
| 32 | `VlrTC` | `decimal(15,2)` | sim | - |  |
| 33 | `QtdePrestacao` | `decimal(4,0)` | sim | - |  |
| 34 | `UsadoMarca` | `varchar(30)` | sim | - |  |
| 35 | `UsadoModelo` | `varchar(30)` | sim | - |  |
| 36 | `UsadoCor` | `varchar(30)` | sim | - |  |
| 37 | `UsadoAnoFabr` | `decimal(4,0)` | sim | - |  |
| 38 | `UsadoAnoMod` | `decimal(4,0)` | sim | - |  |
| 39 | `UsadoNroPortas` | `decimal(1,0)` | sim | - |  |
| 40 | `UsadoVlrPre` | `decimal(15,2)` | sim | - |  |
| 41 | `UsadoVlrAvalCct` | `decimal(15,2)` | sim | - |  |
| 42 | `UsadoObs` | `varchar(200)` | sim | - |  |
| 43 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 44 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 45 | `VlrCusto` | `decimal(15,2)` | sim | - |  |
| 46 | `VlrMargem` | `decimal(15,2)` | sim | - |  |
| 47 | `Financeira` | `varchar(20)` | sim | - |  |
| 48 | `SeqFinanc` | `decimal(6,0)` | sim | - | FK -> `IVF_Financeira.SeqFinanc` |
| 49 | `Placa` | `varchar(7)` | sim | - |  |
| 50 | `Chassi` | `varchar(50)` | sim | - |  |
| 51 | `Negociacao` | `varchar(250)` | sim | - |  |
| 52 | `Opcionais` | `varchar(250)` | sim | - |  |
| 53 | `ObsNegociacao` | `varchar(250)` | sim | - |  |
| 54 | `UsadoChassi` | `varchar(50)` | sim | - |  |
| 55 | `UsadoPlaca` | `varchar(7)` | sim | - |  |
| 56 | `IndVeicUsadoTroca` | `numeric(1,0)` | sim | - |  |
| 57 | `UsadoCombustivel` | `char(1)` | sim | - |  |
| 58 | `UsadoVlrTroca` | `decimal(15,2)` | sim | - |  |
| 59 | `Combustivel` | `char(1)` | sim | - |  |
| 60 | `UsadoRenavam` | `varchar(16)` | sim | - |  |
| 61 | `UsadoNroMotor` | `varchar(30)` | sim | - |  |
| 62 | `USADOKM` | `numeric(8,0)` | sim | - |  |
| 63 | `TIPOVENDA` | `varchar(15)` | sim | - |  |
| 64 | `USADOMARCA2` | `varchar(30)` | sim | - |  |
| 65 | `USADOMODELO2` | `varchar(30)` | sim | - |  |
| 66 | `USADORENAVAM2` | `varchar(16)` | sim | - |  |
| 67 | `USADOPLACA2` | `varchar(7)` | sim | - |  |
| 68 | `USADOKM2` | `numeric(8,0)` | sim | - |  |
| 69 | `USADOANOFABR2` | `numeric(4,0)` | sim | - |  |
| 70 | `USADOANOMOD2` | `numeric(4,0)` | sim | - |  |
| 71 | `USADOCOR2` | `varchar(30)` | sim | - |  |
| 72 | `USADONROPORTAS2` | `numeric(1,0)` | sim | - |  |
| 73 | `USADOVLRPRE2` | `numeric(14,2)` | sim | - |  |
| 74 | `USADOVLRAVALCCT2` | `numeric(14,2)` | sim | - |  |
| 75 | `USADOVLRTROCA2` | `numeric(14,2)` | sim | - |  |
| 76 | `USADOOBS2` | `varchar(200)` | sim | - |  |
| 77 | `USADOCOMBUSTIVEL2` | `char(1)` | sim | - |  |
| 78 | `USADOCHASSI2` | `varchar(50)` | sim | - |  |
| 79 | `USADONROMOTOR2` | `varchar(30)` | sim | - |  |

**Referenciada por:** `IVF_Agregado.Processo`, `IVF_PlanoIndic.Processo`, `IVF_Proposta.ProcessoFN`

---

### IV_FoneCtrl

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: FoneNro, FoneDDD`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de controle/condicoes de uso relacionada a telefonia, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `FoneNro` | `decimal(12,0)` | **nao** | - | **PK** |
| 2 | `FoneDDD` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `Bloqueado` | `numeric(1,0)` | sim | - |  |
| 4 | `Status` | `varchar(18)` | sim | - | status - validar dominio real por tabela |
| 5 | `DtaBloqueio` | `datetime` | sim | - |  |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

**Referenciada por:** `IV_FoneCtrlHst.FoneDDD`, `IV_FoneCtrlHst.FoneNro`

---

### IV_FoneCtrl2

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `FoneNro` | `decimal(12,0)` | **nao** | - |  |
| 2 | `FoneDDD` | `decimal(2,0)` | **nao** | - |  |
| 3 | `Bloqueado` | `numeric(1,0)` | sim | - |  |
| 4 | `Status` | `varchar(18)` | sim | - | status - validar dominio real por tabela |
| 5 | `DtaBloqueio` | `datetime` | sim | - |  |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_FoneCtrlHst

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: FoneNro, FoneDDD, SeqHst`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a telefonia, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `FoneNro` | `decimal(12,0)` | **nao** | - | **PK**; FK -> `IV_FoneCtrl.FoneNro` |
| 2 | `FoneDDD` | `decimal(2,0)` | **nao** | - | **PK**; FK -> `IV_FoneCtrl.FoneDDD` |
| 3 | `SeqHst` | `decimal(4,0)` | **nao** | - | **PK** |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `Operacao` | `varchar(20)` | sim | - |  |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 7 | `Obs` | `varchar(150)` | sim | - | texto livre |

---

### IV_LEADFACEBOOK

`classe: vazia` · `21 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQLEADFACEBOOK`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de lead de marketing, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLEADFACEBOOK` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CAMPANHA` | `varchar(100)` | sim | - |  |
| 3 | `ANUNCIO` | `varchar(100)` | sim | - |  |
| 4 | `DTALEAD` | `datetime` | sim | - |  |
| 5 | `PLATAFORMA` | `varchar(100)` | sim | - |  |
| 6 | `NOME` | `varchar(100)` | sim | - |  |
| 7 | `EMAIL` | `varchar(100)` | sim | - |  |
| 8 | `CPF` | `varchar(20)` | sim | - |  |
| 9 | `FONE` | `varchar(20)` | sim | - |  |
| 10 | `ENDERECO` | `varchar(150)` | sim | - |  |
| 11 | `CIDADE` | `varchar(100)` | sim | - |  |
| 12 | `ESTADO` | `varchar(2)` | sim | - |  |
| 13 | `CEP` | `varchar(10)` | sim | - |  |
| 14 | `DTANASCIMENTO` | `datetime` | sim | - |  |
| 15 | `SEXO` | `varchar(5)` | sim | - |  |
| 16 | `ESTADOCIVIL` | `varchar(30)` | sim | - |  |
| 17 | `PROFISSAO` | `varchar(100)` | sim | - |  |
| 18 | `FONECOMERCIAL` | `varchar(30)` | sim | - |  |
| 19 | `EMAILCOMERCIAL` | `varchar(100)` | sim | - |  |
| 20 | `NOMEEMPRESA` | `varchar(100)` | sim | - |  |
| 21 | `LEADID` | `varchar(250)` | sim | - |  |

**Referenciada por:** `IV_LEADFACEITEM.SEQLEADFACEBOOK`

---

### IV_LEADFACEITEM

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQLEADFACEBOOK, SEQLEADITEM`

**Funcao:** _(inferido)_ Pelo nome, e a filha 1:N de itens relacionada a lead de marketing, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLEADFACEBOOK` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_LEADFACEBOOK.SEQLEADFACEBOOK` |
| 2 | `SEQLEADITEM` | `numeric(2,0)` | **nao** | - | **PK** |
| 3 | `PERGUNTA` | `varchar(250)` | sim | - |  |
| 4 | `TIPODADO` | `varchar(1)` | sim | - |  |
| 5 | `RESPOSTATEXTO` | `varchar(250)` | sim | - |  |
| 6 | `RESPOSTADATA` | `datetime` | sim | - |  |

---

### IV_ObjFlow

`classe: isolada` · `9 colunas` · `726 linhas (snapshot 03/06/2026)` · `PK: CodModelo, ModoSimples, TipoObj, ChaveObj`

**Funcao:** O desenho é persistido em IV_ObjFlow (726 objetos: CodModelo, ModoSimples, TipoObj, ChaveObj, PosLeft, PosTop, IndCor, Dados, IndMudanca) — ou seja, as coordenadas do diagrama ficam no banco junto com a definição do fluxo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodModelo` | `decimal(8,0)` | **nao** | - | **PK** |
| 2 | `ModoSimples` | `numeric(1,0)` | **nao** | - | **PK** |
| 3 | `TipoObj` | `varchar(40)` | **nao** | - | **PK** |
| 4 | `ChaveObj` | `numeric(18,0)` | **nao** | - | **PK** |
| 5 | `PosLeft` | `numeric(18,0)` | sim | - |  |
| 6 | `PosTop` | `numeric(18,0)` | sim | - |  |
| 7 | `IndCor` | `decimal(10,0)` | sim | - |  |
| 8 | `Dados` | `varchar(1000)` | sim | - |  |
| 9 | `IndMudanca` | `numeric(1,0)` | sim | - |  |

---

### IV_ObjVenda

`classe: vazia` · `13 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqObjVenda`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqObjVenda` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 3 | `Seguimento` | `varchar(20)` | sim | - |  |
| 4 | `Familia` | `varchar(20)` | sim | - |  |
| 5 | `SeqUsuario` | `decimal(8,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 6 | `QtdeContato` | `decimal(4,0)` | sim | - |  |
| 7 | `QtdeUnidVendida` | `decimal(4,0)` | sim | - |  |
| 8 | `VlrVenda` | `decimal(15,2)` | sim | - |  |
| 9 | `QtdeTestDrive` | `decimal(4,0)` | sim | - |  |
| 10 | `DtaReferencia` | `datetime` | sim | - |  |
| 11 | `Obs` | `varchar(200)` | sim | - | texto livre |
| 12 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 13 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_OcrmAgd

`classe: isolada` · `8 colunas` · `18 linhas (snapshot 03/06/2026)` · `PK: SeqOcrmAgd`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqOcrmAgd` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqOcrmDest` | `numeric(18,0)` | **nao** | - |  |
| 3 | `SeqAgenda` | `numeric(18,0)` | **nao** | - | agenda (`IV_Agenda.SeqAgenda`) |
| 4 | `DtaEnvio` | `datetime` | sim | - |  |
| 5 | `IndEnviado` | `numeric(1,0)` | sim | - |  |
| 6 | `NroEnvio` | `numeric(1,0)` | sim | - |  |
| 7 | `eMail` | `varchar(50)` | sim | - |  |
| 8 | `STATUSMSG` | `varchar(250)` | sim | - |  |

---

### IV_OcrmDest

`classe: isolada` · `12 colunas` · `17 linhas (snapshot 03/06/2026)` · `PK: SeqOcrmDest`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de status, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqOcrmDest` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `Acao` | `numeric(10,0)` | sim | - | acao (`IV_Acao.Acao`) |
| 4 | `HUEnvio1` | `numeric(4,1)` | sim | - |  |
| 5 | `HUEnvio2` | `numeric(4,1)` | sim | - |  |
| 6 | `HUEnvio3` | `numeric(4,1)` | sim | - |  |
| 7 | `HUEnvio4` | `numeric(4,1)` | sim | - |  |
| 8 | `IndEnvPessoa` | `numeric(1,0)` | sim | - |  |
| 9 | `IndEnvContato` | `numeric(1,0)` | sim | - |  |
| 10 | `IndEnvHistProc` | `numeric(1,0)` | sim | - |  |
| 11 | `IndEnvHistDNA` | `numeric(1,0)` | sim | - |  |
| 12 | `IndEnvProcesso` | `numeric(1,0)` | sim | - |  |

---

### IV_OpBloq

`classe: nucleo` · `8 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, DtaInicial`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de bloqueio, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Operador.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `DtaInicial` | `datetime` | **nao** | - | **PK** |
| 3 | `DtaFinal` | `datetime` | sim | - |  |
| 4 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 5 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 6 | `SeqUsrDesvio` | `numeric(18,0)` | sim | - |  |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_Operador

`classe: catalogo` · `51 colunas` · `932 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario`

**Funcao:** Permissões operacionais por usuário (939 linhas × 51 colunas de flags, 370 combinações distintas) e o modelo de PERFIL que resolveria isso — nunca ativado (0 linhas em ambas). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `SeqUnidade` | `numeric(4,0)` | sim | - |  |
| 3 | `Funcao` | `char(1)` | sim | - |  |
| 4 | `Supervisor` | `varchar(20)` | sim | - |  |
| 5 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 6 | `JustifDispon` | `varchar(100)` | sim | - |  |
| 7 | `Gerente` | `varchar(20)` | sim | - |  |
| 8 | `HoraInicial` | `datetime` | sim | - |  |
| 9 | `HoraFinal` | `datetime` | sim | - |  |
| 10 | `DispDomingo` | `numeric(1,0)` | sim | - |  |
| 11 | `DispSegunda` | `numeric(1,0)` | sim | - |  |
| 12 | `DispTerca` | `numeric(1,0)` | sim | - |  |
| 13 | `DispQuarta` | `numeric(1,0)` | sim | - |  |
| 14 | `DispQuinta` | `numeric(1,0)` | sim | - |  |
| 15 | `DispSexta` | `numeric(1,0)` | sim | - |  |
| 16 | `DispSabado` | `numeric(1,0)` | sim | - |  |
| 17 | `EVendedor` | `char(1)` | sim | - |  |
| 18 | `IncHistorico` | `char(1)` | sim | - |  |
| 19 | `IncHistRetr` | `char(1)` | sim | - |  |
| 20 | `QtdDiasRetr` | `decimal(4,0)` | sim | - |  |
| 21 | `IncHistAgConcl` | `char(1)` | sim | - |  |
| 22 | `AltHistorico` | `char(1)` | sim | - |  |
| 23 | `ExcHistorico` | `char(1)` | sim | - |  |
| 24 | `IncAgenda` | `char(1)` | sim | - |  |
| 25 | `AltAgenda` | `char(1)` | sim | - |  |
| 26 | `ExcAgenda` | `char(1)` | sim | - |  |
| 27 | `VerAgenda` | `char(1)` | sim | - |  |
| 28 | `ConcAgenda` | `char(1)` | sim | - |  |
| 29 | `AltOperAgenda` | `char(1)` | sim | - |  |
| 30 | `AltOperAgdReag` | `char(1)` | sim | - |  |
| 31 | `ConcAgeVendor` | `char(1)` | sim | - |  |
| 32 | `ReativarAgenda` | `char(1)` | sim | - |  |
| 33 | `AnalisarHistorico` | `char(1)` | sim | - |  |
| 34 | `AbreAnalise` | `char(1)` | sim | - |  |
| 35 | `Alteragrupo` | `char(1)` | sim | - |  |
| 36 | `AlteraCodImport` | `char(1)` | sim | - |  |
| 37 | `AlteraRegiao` | `char(1)` | sim | - |  |
| 38 | `AlteraVendedor` | `char(1)` | sim | - |  |
| 39 | `AlteraClienteAtivo` | `char(1)` | sim | - |  |
| 40 | `AlteraStatus` | `char(1)` | sim | - |  |
| 41 | `DupClicAgenda` | `varchar(3)` | sim | - |  |
| 42 | `FormaEnvioEmail` | `varchar(3)` | sim | - |  |
| 43 | `EnderecoSMTP` | `varchar(30)` | sim | - |  |
| 44 | `Departamento` | `varchar(20)` | sim | - | departamento |
| 45 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 46 | `eMail` | `varchar(60)` | sim | - |  |
| 47 | `EMailAssinatura` | `varchar(250)` | sim | - |  |
| 48 | `QtdHistAdicAgenda` | `decimal(1,0)` | sim | - |  |
| 49 | `PesAtendSeq` | `numeric(18,0)` | sim | - |  |
| 50 | `PesAtendEnv` | `varchar(20)` | sim | - |  |
| 51 | `PesAtendHora` | `datetime` | sim | - |  |

**Referenciada por:** `IV_AcaoAtendente.SeqUsuario`, `IV_OpBloq.SeqUsuario`

---

### IV_OPTEMAIL

`classe: nucleo` · `7 colunas` · `21 linhas (snapshot 03/06/2026)` · `PK: SEQCONTEUDO, EMAIL`

**Funcao:** Módulo de campanha multicanal com priorização de canal (e-mail, SMS, correio, AllIn, Mail2Easy), janela de horário por canal, voucher e custo de mídia — 179 campanhas cadastradas mas IV_CampPessoa vazia (módulo morto na prática). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONTEUDO` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `IV_TIPOCONTEUDO.SEQCONTEUDO` |
| 2 | `EMAIL` | `varchar(70)` | **nao** | - | **PK** |
| 3 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 4 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `MOTIVO` | `varchar(200)` | sim | - |  |
| 6 | `OBS` | `varchar(100)` | sim | - | texto livre |
| 7 | `OUTIN` | `char(1)` | sim | - |  |

---

### IV_OPTFONE

`classe: nucleo` · `7 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQCONTEUDO, FONENUMERO`

**Funcao:** Módulo de campanha multicanal com priorização de canal (e-mail, SMS, correio, AllIn, Mail2Easy), janela de horário por canal, voucher e custo de mídia — 179 campanhas cadastradas mas IV_CampPessoa vazia (módulo morto na prática). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONTEUDO` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `IV_TIPOCONTEUDO.SEQCONTEUDO` |
| 2 | `FONENUMERO` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 4 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `MOTIVO` | `varchar(200)` | sim | - |  |
| 6 | `OBS` | `varchar(100)` | sim | - | texto livre |
| 7 | `OUTIN` | `char(1)` | sim | - |  |

---

### IV_OS

`classe: vazia` · `22 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQOS`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de ordem de servico, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQOS` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `VERSAO` | `numeric(2,0)` | **nao** | - |  |
| 3 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `SEQPROJETO` | `numeric(18,0)` | sim | - | FK -> `IV_Projeto.SeqProjeto`; projeto (`IV_Projeto`) |
| 5 | `TIPOOS` | `varchar(20)` | **nao** | - |  |
| 6 | `DTAFINALIZACAO` | `datetime` | sim | - |  |
| 7 | `DTAFINALPREV` | `datetime` | sim | - |  |
| 8 | `OBJETIVO` | `varchar(250)` | sim | - |  |
| 9 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 10 | `USUINCLUSAO` | `varchar(20)` | **nao** | - | auditoria de inclusao (usuario) |
| 11 | `DTAAPROVACAO` | `datetime` | sim | - |  |
| 12 | `USUAPROVOU` | `varchar(20)` | sim | - |  |
| 13 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 14 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 15 | `STATUS` | `varchar(20)` | sim | - | status - validar dominio real por tabela |
| 16 | `STATUSDESC` | `varchar(250)` | sim | - |  |
| 17 | `TIPOVALOR` | `char(1)` | **nao** | - |  |
| 18 | `VLRORCADO` | `numeric(12,2)` | sim | - |  |
| 19 | `VLRDESCONTO` | `numeric(12,2)` | sim | - |  |
| 20 | `TIPOFATURAMENTO` | `char(2)` | **nao** | - |  |
| 21 | `DTAPREVFATURAR` | `datetime` | sim | - |  |
| 22 | `OBS` | `varchar(250)` | sim | - | texto livre |

---

### IV_Pessoa

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `LinkWeb` | `numeric(1,0)` | sim | - |  |
| 3 | `NomeReduzido` | `varchar(10)` | sim | - |  |
| 4 | `DirEspecial` | `varchar(20)` | sim | - |  |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_PessoaStat

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `DtaUltCalcHst` | `datetime` | sim | - |  |
| 3 | `DtaHstBase` | `datetime` | sim | - |  |

---

### IV_PlanoAtiv

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPlanoAtiv`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de financiamento, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPlanoAtiv` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `char(1)` | sim | - |  |
| 3 | `Nro` | `decimal(10,0)` | sim | - |  |
| 4 | `Cod` | `varchar(20)` | sim | - |  |
| 5 | `Descricao` | `varchar(80)` | sim | - | descricao do registro |
| 6 | `Nivel` | `decimal(1,0)` | sim | - |  |
| 7 | `NroOrdem` | `decimal(6,0)` | sim | - |  |
| 8 | `SeqGrupo` | `numeric(18,0)` | sim | - |  |
| 9 | `Analitica` | `decimal(1,0)` | sim | - |  |
| 10 | `DtaNula` | `datetime` | sim | - |  |

**Referenciada por:** `IV_Ativ.SeqPlanoAtiv`

---

### IV_Produto

`classe: nucleo` · `24 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SeqProduto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de produto, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProduto` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodProduto` | `varchar(50)` | sim | - |  |
| 3 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 4 | `CodProdutoFora` | `numeric(18,0)` | sim | - |  |
| 5 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 6 | `Especificacao` | `varchar(250)` | sim | - |  |
| 7 | `Preco1` | `decimal(15,2)` | sim | - |  |
| 8 | `Preco2` | `decimal(15,2)` | sim | - |  |
| 9 | `Familia` | `varchar(60)` | sim | - |  |
| 10 | `Atributo1` | `varchar(40)` | sim | - |  |
| 11 | `Atributo2` | `varchar(40)` | sim | - |  |
| 12 | `Atributo3` | `varchar(40)` | sim | - |  |
| 13 | `Atributo4` | `varchar(40)` | sim | - |  |
| 14 | `Atributo5` | `varchar(40)` | sim | - |  |
| 15 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 16 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 17 | `Fator1` | `decimal(8,4)` | sim | - |  |
| 18 | `Fator2` | `decimal(8,4)` | sim | - |  |
| 19 | `Fator3` | `decimal(8,4)` | sim | - |  |
| 20 | `EmUso` | `char(1)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 21 | `URLDetalhes` | `varchar(100)` | sim | - |  |
| 22 | `CHAVEPRODUTOERP` | `varchar(80)` | sim | - |  |
| 23 | `MARCA` | `varchar(30)` | sim | - |  |
| 24 | `SEQESTRPROD` | `numeric(10,0)` | sim | - | FK -> `IV_ESTRPRODUTO.SEQESTRPROD` |

---

### IV_ProjColec

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProjColec`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de projeto, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProjColec` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(20)` | sim | - | descricao do registro |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

**Referenciada por:** `IV_Projeto.SeqProjColec`

---

### IV_ProjDocto

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProjeto, SeqDocto`

**Funcao:** vínculos em IV_ProcDocto (76.603) e IV_ProjDocto. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProjeto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Projeto.SeqProjeto`; projeto (`IV_Projeto`) |
| 2 | `SeqDocto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `DMN_Doc.SeqDocto`; documento (`DMN_Doc.SeqDocto`) |
| 3 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 4 | `UsuIncluiu` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |

---

### IV_ProjEquipe

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProjeto, SeqUsuario`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de projeto, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProjeto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Projeto.SeqProjeto`; projeto (`IV_Projeto`) |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_Projeto

`classe: vazia` · `17 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqProjeto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de projeto, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqProjeto` | `numeric(18,0)` | **nao** | - | **PK**; projeto (`IV_Projeto`) |
| 2 | `SeqProjColec` | `numeric(18,0)` | sim | - | FK -> `IV_ProjColec.SeqProjColec` |
| 3 | `Projeto` | `varchar(40)` | sim | - |  |
| 4 | `Descricao` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `Tipo` | `varchar(20)` | sim | - |  |
| 6 | `Responsavel` | `varchar(20)` | sim | - |  |
| 7 | `DtaIniPrev` | `datetime` | sim | - |  |
| 8 | `DtaFinPrev` | `datetime` | sim | - |  |
| 9 | `DtaInicial` | `datetime` | sim | - |  |
| 10 | `DtaFinal` | `datetime` | sim | - |  |
| 11 | `DtaInsProcDe` | `datetime` | sim | - |  |
| 12 | `DtaInsProcA` | `datetime` | sim | - |  |
| 13 | `QtdHoraEst` | `decimal(6,0)` | sim | - |  |
| 14 | `VlrVenda` | `decimal(15,2)` | sim | - |  |
| 15 | `CustoEst` | `decimal(15,2)` | sim | - |  |
| 16 | `PubWeb` | `numeric(1,0)` | sim | - |  |
| 17 | `Status` | `varchar(20)` | sim | - | status - validar dominio real por tabela |

**Referenciada por:** `DMN_DocProj.SeqProjeto`, `IV_OS.SEQPROJETO`, `IV_ProjDocto.SeqProjeto`, `IV_ProjEquipe.SeqProjeto`, `IV_ProjPessoa.SeqProjeto`, `IV_Questionario.SEQPROJETO`

---

### IV_ProjPessoa

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqProjeto`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqProjeto` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Projeto.SeqProjeto`; projeto (`IV_Projeto`) |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_PUSH

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPUSH`

**Funcao:** Push: IV_PUSH (15 colunas, APLICATIVO/ASSUNTO/MENSAGEM/CONTEXTO/SUBCONTEXTO/STATUS) e IV_USRPUSH — ZERO linhas; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPUSH` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `APLICATIVO` | `varchar(50)` | **nao** | - |  |
| 3 | `ASSUNTO` | `varchar(150)` | sim | - |  |
| 4 | `MENSAGEM` | `varchar(1000)` | **nao** | - |  |
| 5 | `SEQPESSOA` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 6 | `PROCESSO` | `numeric(18,0)` | sim | - | FK -> `IV_ProcDado.Processo`; numero do processo (`IV_Processo.Processo`) |
| 7 | `SEQHISTORICO` | `numeric(18,0)` | sim | - | FK -> `IV_Historico.SeqHistorico`; historico (`IV_Historico.SeqHistorico`) |
| 8 | `SEQCONTATO` | `numeric(4,0)` | sim | - |  |
| 9 | `DTAGERACAO` | `datetime` | sim | - |  |
| 10 | `DTAENVIO` | `datetime` | sim | - |  |
| 11 | `CONTEXTO` | `varchar(250)` | sim | - |  |
| 12 | `SUBCONTEXTO` | `varchar(250)` | sim | - |  |
| 13 | `DTAENVIAR` | `datetime` | sim | - |  |
| 14 | `STATUS` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 15 | `STATUSMSG` | `varchar(250)` | sim | - |  |

---

### IV_RESULTADO_3110

`classe: lixo/backup` · `74 colunas` · `2.826 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Existe ainda IV_RESULTADO_3110 (74 colunas, 2.826 linhas) — uma tabela de formulário disfarçada de tabela de resultado. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

> **Nao usar em producao.** Motivo da classificacao: copia pontual de IV_Resultado (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (74 colunas) para manter o documento legivel._

---

### IV_Selecao

`classe: catalogo` · `12 colunas` · `543 linhas (snapshot 03/06/2026)` · `PK: SeqSelecao`

**Funcao:** Módulo de campanha multicanal com priorização de canal (e-mail, SMS, correio, AllIn, Mail2Easy), janela de horário por canal, voucher e custo de mídia — 179 campanhas cadastradas mas IV_CampPessoa vazia (módulo morto na prática). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqSelecao` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Selecao` | `varchar(50)` | **nao** | - |  |
| 3 | `Descricao` | `varchar(200)` | sim | - | descricao do registro |
| 4 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 5 | `Dinamica` | `numeric(1,0)` | sim | - |  |
| 6 | `Tipo` | `char(1)` | sim | - |  |
| 7 | `DtaGeracao` | `datetime` | sim | - |  |
| 8 | `QtdeClientes` | `decimal(7,0)` | sim | - |  |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `Construtor` | `varchar(20)` | sim | - |  |
| 11 | `Usuario` | `varchar(20)` | sim | - |  |
| 12 | `UsuGeracao` | `varchar(20)` | sim | - |  |

**Referenciada por:** `IV_SelecaoCriterio.SeqSelecao`, `IV_SelecaoPessoa.SeqSelecao`

---

### IV_SelecaoColList

`classe: isolada` · `3 colunas` · `35 linhas (snapshot 03/06/2026)` · `PK: TbNome, ColNome`

**Funcao:** IV_SelecaoColList (35) define as colunas exibidas. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `TbNome` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `ColNome` | `varchar(30)` | **nao** | - | **PK** |
| 3 | `InstrSql` | `varchar(250)` | sim | - |  |

---

### IV_SelecaoCriterio

`classe: nucleo` · `7 colunas` · `2.865 linhas (snapshot 03/06/2026)` · `PK: SeqSelecao, SeqCriterio`

**Funcao:** Motor de segmentação com DSL própria de álgebra de conjuntos. 543 / 2.865 / 582.481 linhas. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqSelecao` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Selecao.SeqSelecao` |
| 2 | `SeqCriterio` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Operacao` | `char(1)` | sim | - |  |
| 4 | `Ordem` | `decimal(4,0)` | sim | - |  |
| 5 | `SeqSelecaoFoco` | `numeric(18,0)` | sim | - |  |
| 6 | `DescCriterio` | `varchar(60)` | sim | - |  |
| 7 | `InstrSQL` | `text(2147483647)` | sim | - |  |

---

### IV_SelecaoPessoa

`classe: nucleo` · `3 colunas` · `582.481 linhas (snapshot 03/06/2026)` · `PK: SeqSelecao, SeqPessoa`

**Funcao:** Motor de segmentação com DSL própria de álgebra de conjuntos. 543 / 2.865 / 582.481 linhas. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqSelecao` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Selecao.SeqSelecao` |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Usado` | `numeric(1,0)` | sim | - |  |

---

### IV_SELPROMOPRD

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQSELPROMO, IDPRODUTO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de promocao, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQSELPROMO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IDPRODUTO` | `numeric(10,0)` | **nao** | - | **PK** |
| 3 | `PERDESCONTO` | `numeric(4,2)` | sim | - |  |
| 4 | `VLRUNITARIO` | `numeric(14,2)` | sim | - |  |
| 5 | `QTDLIMITE` | `numeric(10,2)` | sim | - |  |

---

### IV_SMS

`classe: isolada` · `19 colunas` · `26.749 linhas (snapshot 03/06/2026)` · `PK: SeqSMS`

**Funcao:** usados pelo Message Center (SeqTxtPadrao em IV_ResMsgPapel), por SMS (IV_SMS.SeqTxtPadrao), por WhatsApp e pelo escalonamento (IV_AcaoCtrl.SeqTxtPadrao). (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqSMS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Origem` | `char(3)` | sim | - | sistema de origem do dado |
| 3 | `Chave` | `numeric(18,0)` | sim | - |  |
| 4 | `Destino` | `numeric(16,0)` | sim | - |  |
| 5 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 6 | `SeqUsuario` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 7 | `SeqTxtPadrao` | `numeric(18,0)` | sim | - |  |
| 8 | `Mensagem` | `varchar(1000)` | sim | - |  |
| 9 | `DtaGeracao` | `datetime` | sim | - |  |
| 10 | `DtaEnvio` | `datetime` | sim | - |  |
| 11 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 12 | `IndRetProcessado` | `numeric(1,0)` | sim | - |  |
| 13 | `ContaSeqPar` | `numeric(18,0)` | sim | - |  |
| 14 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 15 | `Resultado` | `numeric(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 16 | `SeqContaSMS` | `numeric(18,0)` | sim | - |  |
| 17 | `CONTEXTO` | `varchar(250)` | sim | - |  |
| 18 | `SUBCONTEXTO` | `varchar(250)` | sim | - |  |
| 19 | `INDENVIAQLQERHORA` | `numeric(1,0)` | sim | - |  |

---

### IV_SMSLog

`classe: isolada` · `5 colunas` · `27.430 linhas (snapshot 03/06/2026)` · `PK: SeqSMS, SeqLog`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a mensageria (SMS/WhatsApp/push), no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqSMS` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqLog` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `DtaLog` | `datetime` | sim | - |  |
| 4 | `Status` | `char(6)` | sim | - | status - validar dominio real por tabela |
| 5 | `OBS` | `varchar(150)` | sim | - | texto livre |

---

### IV_TC_PESSOA

`classe: vazia` · `13 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQQUESTIONARIO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Questionario.SeqQuestionario`; questionario (`IV_Questionario.SeqQuestionario`) |
| 2 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim | - |  |
| 3 | `TIPO_DE_EQUIPAMENTO` | `varchar(30)` | sim | - |  |
| 4 | `MARCA` | `varchar(30)` | sim | - |  |
| 5 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim | - |  |
| 6 | `ANO_MODELO_EQUIPAMEN` | `varchar(20)` | sim | - |  |
| 7 | `N__SERIE` | `varchar(250)` | sim | - |  |
| 8 | `HORIMETRO` | `varchar(250)` | sim | - |  |
| 9 | `DATA_INICIO_LOCACAO` | `datetime` | sim | - |  |
| 10 | `DATA_TERMINO_LOCACAO` | `datetime` | sim | - |  |
| 11 | `VALOR_MENSAL_DO_CONT` | `decimal(14,2)` | sim | - |  |
| 12 | `VALOR_TOTAL_DO_CONTR` | `decimal(14,2)` | sim | - |  |
| 13 | `DATA_RENOVACAO` | `datetime` | sim | - |  |

---

### IV_TEMPLATEPROJ

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: ARQUIVOTEMPLATE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de projeto, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `ARQUIVOTEMPLATE` | `varchar(250)` | **nao** | - | **PK** |
| 2 | `MENSSAGEM` | `varchar(250)` | sim | - |  |
| 3 | `SEQPROJETO` | `numeric(18,0)` | **nao** | - | projeto (`IV_Projeto`) |
| 4 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 5 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 6 | `USUALTEROU` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 7 | `ORIGEM` | `varchar(20)` | sim | - | sistema de origem do dado |

---

### IV_URADISPARO

`classe: vazia` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQURADISPARO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com agenda (`SeqAgenda`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQURADISPARO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQAGENDA` | `numeric(18,0)` | sim | - | FK -> `IV_Agenda.SeqAgenda`; agenda (`IV_Agenda.SeqAgenda`) |
| 3 | `SEQURACENARIO` | `numeric(18,0)` | **nao** | - | FK -> `GE_URACENARIO.SEQURACENARIO` |
| 4 | `DESTINO` | `numeric(12,0)` | sim | - |  |
| 5 | `DTAGERACAO` | `datetime` | sim | - |  |
| 6 | `BULKID` | `varchar(50)` | sim | - |  |
| 7 | `MESSAGEID` | `varchar(50)` | sim | - |  |
| 8 | `DTAENVIO` | `datetime` | sim | - |  |
| 9 | `STATUS` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 10 | `PRIORIDADE` | `numeric(2,0)` | sim | - |  |
| 11 | `DTAENVIAR` | `datetime` | sim | - |  |
| 12 | `STATUSDESC` | `varchar(250)` | sim | - |  |

**Referenciada por:** `IV_URARELATORIO.SEQURADISPARO`

---

### IV_URARELATORIO

`classe: vazia` · `16 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQURARELATORIO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQURARELATORIO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQURADISPARO` | `numeric(18,0)` | **nao** | - | FK -> `IV_URADISPARO.SEQURADISPARO` |
| 3 | `SENDAT` | `datetime` | sim | - |  |
| 4 | `DONEAT` | `datetime` | sim | - |  |
| 5 | `DURACAO` | `numeric(6,0)` | sim | - |  |
| 6 | `DTMFCODES` | `varchar(50)` | sim | - |  |
| 7 | `URLAUDIOFILE` | `varchar(250)` | sim | - |  |
| 8 | `PRICEPERMESSAGE` | `numeric(14,2)` | sim | - |  |
| 9 | `STATUS_GROUPNAME` | `varchar(40)` | sim | - |  |
| 10 | `STATUS_NAME` | `varchar(50)` | sim | - |  |
| 11 | `STATUS_DESCRIPTION` | `varchar(250)` | sim | - |  |
| 12 | `ERROR_GROUPNAME` | `varchar(40)` | sim | - |  |
| 13 | `ERROR_NAME` | `varchar(50)` | sim | - |  |
| 14 | `ERROR_DESCRIPTION` | `varchar(250)` | sim | - |  |
| 15 | `DESTINO` | `numeric(14,0)` | sim | - |  |
| 16 | `REMETENTE` | `numeric(14,0)` | sim | - |  |

---

### IV_USRPUSH

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPUSH`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com processo (`Processo`), vinculo com historico (`SeqHistorico`), vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPUSH` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `APLICATIVO` | `varchar(50)` | **nao** | - |  |
| 4 | `ASSUNTO` | `varchar(150)` | sim | - |  |
| 5 | `MENSAGEM` | `varchar(1000)` | **nao** | - |  |
| 6 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 7 | `SEQHISTORICO` | `numeric(18,0)` | sim | - | historico (`IV_Historico.SeqHistorico`) |
| 8 | `DTAGERACAO` | `datetime` | sim | - |  |
| 9 | `DTAENVIO` | `datetime` | sim | - |  |
| 10 | `CONTEXTO` | `varchar(250)` | sim | - |  |
| 11 | `SUBCONTEXTO` | `varchar(250)` | sim | - |  |
| 12 | `DTAENVIAR` | `datetime` | sim | - |  |
| 13 | `STATUS` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 14 | `STATUSMSG` | `varchar(250)` | sim | - |  |
| 15 | `ORIGEM` | `varchar(30)` | sim | - | sistema de origem do dado |

---

### IV_USRSTATUS

`classe: nucleo` · `7 colunas` · `186 linhas (snapshot 03/06/2026)` · `PK: SEQUSUARIO`

**Funcao:** O usuário entra em UMA empresa por sessão: IV_USRSTATUS (186 linhas) guarda por usuário SEQPESSOAATIVA, LINKERPPESSOAATIVA, NROEMPRESACNX (a empresa da conexão), RAZAOSOCIALATIVA e DTAATUALIZACAO; (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `DTAATUALIZACAO` | `datetime` | sim | - |  |
| 3 | `SEQPESSOAATIVA` | `numeric(10,0)` | sim | - |  |
| 4 | `LINKERPPESSOAATIVA` | `varchar(40)` | sim | - |  |
| 5 | `NROEMPRESACNX` | `numeric(6,0)` | sim | - |  |
| 6 | `RAZAOSOCIALATIVA` | `varchar(80)` | sim | - |  |
| 7 | `OBS` | `varchar(150)` | sim | - | texto livre |

---

### IV_VENDEDOR

`classe: isolada` · `18 colunas` · `344 linhas (snapshot 03/06/2026)` · `PK: SEQVENDEDOR`

**Funcao:** Cadastro de vendedor/CEN e a HIERARQUIA comercial que define o escopo gerencial (carteira de pedidos, roteamento de aprovação). 349 e 929 linhas. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQVENDEDOR` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqUnidade` | `numeric(4,0)` | sim | - |  |
| 3 | `CODVENDEDOR` | `varchar(20)` | **nao** | - | codigo/login do vendedor (varchar) |
| 4 | `VENDEDOR` | `varchar(40)` | sim | - | guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR |
| 5 | `CODVENDEDORFORA` | `numeric(18,0)` | sim | - |  |
| 6 | `CODVENDEDORFORAX` | `varchar(20)` | sim | - |  |
| 7 | `SeqUsuario` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 8 | `SeqUsuarioLider` | `numeric(18,0)` | sim | - |  |
| 9 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 10 | `NroEmpresaBKP` | `numeric(6,0)` | sim | - |  |
| 11 | `EMUSO` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 12 | `NROEMPRPADRAO` | `numeric(6,0)` | sim | - |  |
| 13 | `SALARIOFIXO` | `numeric(14,2)` | sim | - |  |
| 14 | `COMISSAO1` | `numeric(8,4)` | sim | - |  |
| 15 | `COMISSAO2` | `numeric(8,4)` | sim | - |  |
| 16 | `OBS` | `varchar(200)` | sim | - | texto livre |
| 17 | `EQUIPE` | `varchar(40)` | sim | - |  |
| 18 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### IV_VENDEDOREMPR

`classe: isolada` · `4 colunas` · `917 linhas (snapshot 03/06/2026)` · `PK: SEQVENDEDOR, NroEmpresa`

**Funcao:** Cadastro de vendedor/CEN e a HIERARQUIA comercial que define o escopo gerencial (carteira de pedidos, roteamento de aprovação). 349 e 929 linhas. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQVENDEDOR` | `numeric(18,0)` | **nao** | - | **PK**; vendedor (`IV_VENDEDOR.SEQVENDEDOR`) |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_WHATSAPP

`classe: vazia` · `21 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQWHATSAPP`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de mensageria (SMS/WhatsApp/push), no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com processo (`Processo`), vinculo com historico (`SeqHistorico`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQWHATSAPP` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `ORIGEM` | `char(3)` | sim | - | sistema de origem do dado |
| 3 | `CHAVE` | `numeric(18,0)` | sim | - |  |
| 4 | `DESTINO` | `varchar(50)` | sim | - |  |
| 5 | `PROCESSO` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 6 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 7 | `SEQTXTPADRAO` | `numeric(18,0)` | **nao** | - | FK -> `IV_TxtPadrao.SeqTxtPadrao` |
| 8 | `MENSAGEM` | `varchar(2000)` | sim | - |  |
| 9 | `NROEMPRESA` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 10 | `SEQHISTORICO` | `numeric(6,0)` | sim | - | historico (`IV_Historico.SeqHistorico`) |
| 11 | `CONTAWAPSEQPAR` | `numeric(18,0)` | sim | - | FK -> `GE_ParamLista.SeqParamLista` |
| 12 | `INDENVIAQLQERHORA` | `numeric(1,0)` | sim | - |  |
| 13 | `DTAENVIO` | `datetime` | sim | - |  |
| 14 | `INDENVIADO` | `numeric(1,0)` | sim | - |  |
| 15 | `INDENVIOOK` | `numeric(1,0)` | sim | - |  |
| 16 | `CODSTATUSENVIO` | `varchar(200)` | sim | - |  |
| 17 | `DESCRSTATUSENVIO` | `varchar(300)` | sim | - |  |
| 18 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 19 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 20 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 21 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---
