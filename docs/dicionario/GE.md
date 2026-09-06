# GE - Geral / plataforma (pessoas, usuarios, seguranca, logs, configuracao)

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**160 tabelas · 2.263 colunas · 34.500.052 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`GE_Alias`](#ge_alias) | isolada | 9 | 1.413 |
| [`GE_Aplicacao`](#ge_aplicacao) | nucleo | 9 | 184 |
| [`GE_AppModulo`](#ge_appmodulo) | nucleo | 3 | 30 |
| [`GE_AtributoFixo`](#ge_atributofixo) | catalogo | 9 | 183 |
| [`GE_Bairro`](#ge_bairro) | catalogo | 3 | 619 |
| [`GE_CampoExig`](#ge_campoexig) | vazia | 7 | 0 |
| [`GE_CampoMemo`](#ge_campomemo) | nucleo | 4 | 3.129 |
| [`GE_CampoPerm`](#ge_campoperm) | nucleo | 7 | 17 |
| [`GE_CFinConj`](#ge_cfinconj) | vazia | 22 | 0 |
| [`GE_CFinDocto`](#ge_cfindocto) | vazia | 7 | 0 |
| [`GE_CFinFis`](#ge_cfinfis) | vazia | 51 | 0 |
| [`GE_CFinRPes`](#ge_cfinrpes) | vazia | 12 | 0 |
| [`GE_Cidade`](#ge_cidade) | nucleo | 19 | 10.214 |
| [`GE_CIdadePref`](#ge_cidadepref) | vazia | 3 | 0 |
| [`GE_Cidade_BKPJUN`](#ge_cidade_bkpjun) | lixo/backup | 19 | 9.966 |
| [`GE_Cidade_CRM`](#ge_cidade_crm) | nucleo | 18 | 729 |
| [`GE_Col`](#ge_col) | catalogo | 6 | 217 |
| [`GE_ColPosition`](#ge_colposition) | vazia | 5 | 0 |
| [`GE_ColRegra`](#ge_colregra) | vazia | 8 | 0 |
| [`GE_CONSHST`](#ge_conshst) | vazia | 5 | 0 |
| [`GE_CONSSQL`](#ge_conssql) | catalogo | 10 | 1 |
| [`GE_Consulta`](#ge_consulta) | nucleo | 29 | 2 |
| [`GE_ConsultaVar`](#ge_consultavar) | nucleo | 10 | 3 |
| [`GE_CONSVAR`](#ge_consvar) | vazia | 10 | 0 |
| [`GE_CONSVARLST`](#ge_consvarlst) | vazia | 3 | 0 |
| [`GE_Contato`](#ge_contato) | isolada | 41 | 41.164 |
| [`GE_CONTATOAPP`](#ge_contatoapp) | vazia | 12 | 0 |
| [`GE_ContatoEmail`](#ge_contatoemail) | vazia | 5 | 0 |
| [`GE_ContatoPapel`](#ge_contatopapel) | isolada | 5 | 43.489 |
| [`GE_ContatoPapel_BKPJUN`](#ge_contatopapel_bkpjun) | lixo/backup | 5 | 35.413 |
| [`GE_ContatoPapel_ITA`](#ge_contatopapel_ita) | lixo/backup | 5 | 5.032 |
| [`GE_Contato_BKPJUN`](#ge_contato_bkpjun) | lixo/backup | 41 | 28.051 |
| [`GE_Contato_ITA`](#ge_contato_ita) | lixo/backup | 41 | 10.866 |
| [`GE_CTRL2`](#ge_ctrl2) | isolada | 6 | 276 |
| [`GE_CtrlE`](#ge_ctrle) | isolada | 9 | 16 |
| [`GE_CtrlM`](#ge_ctrlm) | isolada | 11 | 31 |
| [`GE_CtrlME`](#ge_ctrlme) | isolada | 6 | 36 |
| [`GE_CTRLMU`](#ge_ctrlmu) | vazia | 6 | 0 |
| [`GE_CTRLUSR`](#ge_ctrlusr) | isolada | 6 | 33 |
| [`GE_DiaNaoUtil`](#ge_dianaoutil) | vazia | 5 | 0 |
| [`GE_Email`](#ge_email) | isolada | 16 | 38.765 |
| [`GE_EMAILAGD`](#ge_emailagd) | nucleo | 13 | 2 |
| [`GE_EMAILINVALIDO`](#ge_emailinvalido) | vazia | 3 | 0 |
| [`GE_EmailSpam`](#ge_emailspam) | vazia | 4 | 0 |
| [`GE_EMAILTEMPLATE`](#ge_emailtemplate) | nucleo | 6 | 8 |
| [`GE_Empresa`](#ge_empresa) | catalogo | 33 | 18 |
| [`GE_FCadConj`](#ge_fcadconj) | vazia | 43 | 0 |
| [`GE_FCadF`](#ge_fcadf) | vazia | 97 | 0 |
| [`GE_FCadInstCred`](#ge_fcadinstcred) | vazia | 13 | 0 |
| [`GE_FcadRBanc`](#ge_fcadrbanc) | vazia | 18 | 0 |
| [`GE_FcadRCom`](#ge_fcadrcom) | vazia | 20 | 0 |
| [`GE_FCadRPes`](#ge_fcadrpes) | vazia | 33 | 0 |
| [`GE_FERIADO`](#ge_feriado) | vazia | 7 | 0 |
| [`GE_Figura`](#ge_figura) | isolada | 8 | 2.078 |
| [`GE_FONEINVALIDO`](#ge_foneinvalido) | vazia | 3 | 0 |
| [`GE_Grafico`](#ge_grafico) | vazia | 26 | 0 |
| [`GE_Help`](#ge_help) | vazia | 3 | 0 |
| [`GE_IMPORTA_CART`](#ge_importa_cart) | isolada | 52 | 22 |
| [`GE_IntCtrl`](#ge_intctrl) | vazia | 14 | 0 |
| [`GE_LgTb`](#ge_lgtb) | isolada | 10 | 11.861.777 |
| [`GE_Log2`](#ge_log2) | isolada | 14 | 4.394.779 |
| [`GE_LogAtividade`](#ge_logatividade) | vazia | 10 | 0 |
| [`GE_LOG_CARTCRED`](#ge_log_cartcred) | vazia | 10 | 0 |
| [`GE_LOG_CONFIG`](#ge_log_config) | isolada | 10 | 108.492 |
| [`GE_LOG_CONTATO`](#ge_log_contato) | isolada | 10 | 19.785 |
| [`GE_LOG_EXT`](#ge_log_ext) | isolada | 10 | 622.075 |
| [`GE_LOG_HISTORICO`](#ge_log_historico) | isolada | 10 | 1.015.045 |
| [`GE_LOG_PESSOA`](#ge_log_pessoa) | isolada | 10 | 943.712 |
| [`GE_LOG_PROCESSO`](#ge_log_processo) | isolada | 10 | 12.678.640 |
| [`GE_LOG_TRANS`](#ge_log_trans) | isolada | 10 | 782.089 |
| [`GE_Membro`](#ge_membro) | nucleo | 3 | 1.584 |
| [`GE_Membro_BKP20250520`](#ge_membro_bkp20250520) | lixo/backup | 3 | 1.915 |
| [`GE_MobileConfig`](#ge_mobileconfig) | vazia | 14 | 0 |
| [`GE_MOBILEPROP`](#ge_mobileprop) | vazia | 2 | 0 |
| [`GE_Mod`](#ge_mod) | isolada | 9 | 32 |
| [`GE_Modulo`](#ge_modulo) | catalogo | 8 | 48 |
| [`GE_ModuloPerm`](#ge_moduloperm) | nucleo | 6 | 25.435 |
| [`GE_ObjDinamico`](#ge_objdinamico) | isolada | 29 | 193 |
| [`GE_ObjDinAplic`](#ge_objdinaplic) | isolada | 4 | 122 |
| [`GE_ParametroGlobal`](#ge_parametroglobal) | isolada | 8 | 236 |
| [`GE_ParamLista`](#ge_paramlista) | catalogo | 25 | 333 |
| [`GE_Permissao`](#ge_permissao) | nucleo | 11 | 307 |
| [`GE_PesDelVinc`](#ge_pesdelvinc) | vazia | 7 | 0 |
| [`GE_Pessoa`](#ge_pessoa) | nucleo | 76 | 118.463 |
| [`GE_PessoaAlerta`](#ge_pessoaalerta) | isolada | 8 | 237 |
| [`GE_PessoaAlt`](#ge_pessoaalt) | vazia | 68 | 0 |
| [`ge_pessoaativa`](#ge_pessoaativa) | isolada | 1 | 1.175 |
| [`GE_PESSOAATIVAUSR`](#ge_pessoaativausr) | vazia | 5 | 0 |
| [`GE_PessoaClasse`](#ge_pessoaclasse) | isolada | 7 | 1.878 |
| [`GE_PessoaDel`](#ge_pessoadel) | vazia | 76 | 0 |
| [`GE_PessoaDestino`](#ge_pessoadestino) | vazia | 5 | 0 |
| [`GE_PessoaEmail`](#ge_pessoaemail) | vazia | 8 | 0 |
| [`GE_PessoaEnd`](#ge_pessoaend) | nucleo | 26 | 6.480 |
| [`GE_PessoaEndAlt`](#ge_pessoaendalt) | vazia | 21 | 0 |
| [`GE_PessoaEndOutroBc`](#ge_pessoaendoutrobc) | nucleo | 3 | 30.943 |
| [`GE_PessoaEnd_BKPJUN`](#ge_pessoaend_bkpjun) | lixo/backup | 26 | 3.739 |
| [`GE_PessoaEnd_ITA`](#ge_pessoaend_ita) | lixo/backup | 26 | 2.779 |
| [`GE_PessoaFis`](#ge_pessoafis) | isolada | 32 | 5.136 |
| [`GE_PessoaFone`](#ge_pessoafone) | isolada | 19 | 112.659 |
| [`GE_PessoaFonema`](#ge_pessoafonema) | isolada | 3 | 455.993 |
| [`GE_PessoaFone_BKPJUN`](#ge_pessoafone_bkpjun) | lixo/backup | 19 | 69.894 |
| [`GE_PessoaFone_ITA`](#ge_pessoafone_ita) | lixo/backup | 19 | 42.594 |
| [`GE_PessoaJur`](#ge_pessoajur) | vazia | 15 | 0 |
| [`GE_PessoaLink`](#ge_pessoalink) | isolada | 5 | 32.831 |
| [`GE_PessoaLinkbkp`](#ge_pessoalinkbkp) | isolada | 5 | 58.741 |
| [`GE_PessoaLink_bkpago22`](#ge_pessoalink_bkpago22) | lixo/backup | 5 | 92.068 |
| [`GE_PessoaLink_BKPJUN`](#ge_pessoalink_bkpjun) | lixo/backup | 5 | 90.857 |
| [`GE_PessoaLink_ITA`](#ge_pessoalink_ita) | lixo/backup | 5 | 24.864 |
| [`GE_PessoaMural`](#ge_pessoamural) | isolada | 14 | 145.313 |
| [`GE_PessoaNomeFonema`](#ge_pessoanomefonema) | isolada | 4 | 151.583 |
| [`GE_PessoaNota`](#ge_pessoanota) | nucleo | 2 | 1.232 |
| [`GE_PessoaPasw`](#ge_pessoapasw) | vazia | 9 | 0 |
| [`GE_PESSOAREDESOCIAL`](#ge_pessoaredesocial) | vazia | 10 | 0 |
| [`GE_PessoaRelacao`](#ge_pessoarelacao) | isolada | 9 | 5.595 |
| [`GE_PessoaRelacao_BKPJUN`](#ge_pessoarelacao_bkpjun) | lixo/backup | 9 | 5.543 |
| [`GE_PessoaRelacao_ITA`](#ge_pessoarelacao_ita) | lixo/backup | 9 | 3.092 |
| [`GE_PessoaSimilar`](#ge_pessoasimilar) | nucleo | 14 | 124.222 |
| [`GE_PessoaUnidade`](#ge_pessoaunidade) | vazia | 3 | 0 |
| [`GE_PessoaVersao`](#ge_pessoaversao) | isolada | 19 | 12.019 |
| [`GE_Pessoa_BKP28052025`](#ge_pessoa_bkp28052025) | lixo/backup | 76 | 0 |
| [`GE_Pessoa_BKPJUN`](#ge_pessoa_bkpjun) | lixo/backup | 76 | 88.142 |
| [`GE_Pessoa_ITA`](#ge_pessoa_ita) | lixo/backup | 76 | 28.377 |
| [`GE_PolSeg`](#ge_polseg) | catalogo | 3 | 11 |
| [`GE_PolSegAces`](#ge_polsegaces) | nucleo | 7 | 3 |
| [`GE_PolSegCtrl`](#ge_polsegctrl) | nucleo | 7 | 254 |
| [`GE_PolSegItem`](#ge_polsegitem) | catalogo | 7 | 108 |
| [`GE_PolSegItLst`](#ge_polsegitlst) | nucleo | 6 | 66 |
| [`GE_PolSegParams`](#ge_polsegparams) | nucleo | 7 | 1 |
| [`GE_POLSEGPERM`](#ge_polsegperm) | nucleo | 5 | 805 |
| [`GE_PROCESSOWEB`](#ge_processoweb) | vazia | 5 | 0 |
| [`GE_QVCons`](#ge_qvcons) | catalogo | 28 | 134 |
| [`GE_QVConsCol`](#ge_qvconscol) | nucleo | 8 | 40 |
| [`GE_QVConsHst`](#ge_qvconshst) | nucleo | 5 | 69 |
| [`GE_QVConsVar`](#ge_qvconsvar) | nucleo | 17 | 581 |
| [`GE_QVPasta`](#ge_qvpasta) | nucleo | 4 | 28 |
| [`GE_QVPastaCons`](#ge_qvpastacons) | nucleo | 2 | 113 |
| [`GE_QVRegra`](#ge_qvregra) | isolada | 7 | 234 |
| [`GE_Regiao`](#ge_regiao) | vazia | 6 | 0 |
| [`GE_RelPasta`](#ge_relpasta) | vazia | 3 | 0 |
| [`GE_RelPastaCons`](#ge_relpastacons) | vazia | 2 | 0 |
| [`GE_RELTEMPLATE`](#ge_reltemplate) | vazia | 6 | 0 |
| [`GE_Rota`](#ge_rota) | vazia | 6 | 0 |
| [`GE_Sequencia`](#ge_sequencia) | isolada | 2 | 77 |
| [`GE_Sistema`](#ge_sistema) | nucleo | 3 | 11 |
| [`GE_SyncParam`](#ge_syncparam) | isolada | 10 | 635 |
| [`GE_Tab`](#ge_tab) | catalogo | 2 | 12 |
| [`GE_TabRegra`](#ge_tabregra) | vazia | 5 | 0 |
| [`GE_TempLong`](#ge_templong) | isolada | 3 | 6 |
| [`GE_TipoLogradouro`](#ge_tipologradouro) | isolada | 3 | 124 |
| [`GE_URACENARIO`](#ge_uracenario) | vazia | 15 | 0 |
| [`GE_UsrAcSp`](#ge_usracsp) | vazia | 7 | 0 |
| [`GE_UsrCtrl`](#ge_usrctrl) | vazia | 7 | 0 |
| [`GE_UsrParam`](#ge_usrparam) | nucleo | 7 | 16.827 |
| [`GE_Usuario`](#ge_usuario) | catalogo | 35 | 1.379 |
| [`GE_USUARIOCMPL`](#ge_usuariocmpl) | vazia | 5 | 0 |
| [`GE_UsuarioLink`](#ge_usuariolink) | vazia | 5 | 0 |
| [`GE_UsuarioPerm`](#ge_usuarioperm) | isolada | 5 | 38.601 |
| [`GE_UsuarioPerm_BKPJUN`](#ge_usuarioperm_bkpjun) | lixo/backup | 5 | 31.120 |
| [`GE_UsuarioSenhaMem`](#ge_usuariosenhamem) | isolada | 3 | 2.456 |
| [`GE_Usuario_BKP20250520`](#ge_usuario_bkp20250520) | lixo/backup | 35 | 1.302 |

---

### GE_Alias

`classe: isolada` · `9 colunas` · `1.413 linhas (snapshot 03/06/2026)` · `PK: SeqAlias`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAlias` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(30)` | **nao** | - |  |
| 3 | `SEQ` | `numeric(18,0)` | **nao** | - |  |
| 4 | `Origem` | `varchar(30)` | sim | - | sistema de origem do dado |
| 5 | `LinkStr` | `varchar(80)` | sim | - |  |
| 6 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `ChaveCMPL` | `varchar(250)` | sim | - |  |

---

### GE_Aplicacao

`classe: nucleo` · `9 colunas` · `184 linhas (snapshot 03/06/2026)` · `PK: SeqAplicacao`

**Funcao:** Catálogo de licenciamento e inventário de telas do produto: 11 sistemas, 48 módulos (16 com código de licença CRM_M0xx/QVW_M00x/GLB_M000) e 184 aplicações/telas registradas. É a fonte mais confiável do 'que o Vórtice vende'. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAplicacao` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodAplicacao` | `varchar(20)` | **nao** | - |  |
| 3 | `Sistema` | `varchar(20)` | **nao** | - | FK -> `GE_Modulo.Sistema` |
| 4 | `Modulo` | `varchar(20)` | **nao** | - | FK -> `GE_Modulo.Modulo` |
| 5 | `Descricao` | `varchar(50)` | **nao** | - | descricao do registro |
| 6 | `DTALIMITEUSO` | `datetime` | sim | - |  |
| 7 | `TipoAcesso` | `char(1)` | sim | - |  |
| 8 | `RegistrarLog` | `char(1)` | sim | - |  |
| 9 | `Especial1` | `varchar(20)` | sim | - |  |

**Referenciada por:** `GE_Help.SeqAplicacao`

---

### GE_AppModulo

`classe: nucleo` · `3 colunas` · `30 linhas (snapshot 03/06/2026)` · `PK: Modulo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de metadado de aplicacao/tela, no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Modulo` | `varchar(10)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `DescRed` | `varchar(20)` | sim | - |  |

**Referenciada por:** `GE_PolSegItem.Modulo`

---

### GE_AtributoFixo

`classe: catalogo` · `9 colunas` · `183 linhas (snapshot 03/06/2026)` · `PK: Atributo, Lista`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de propriedade/atributo customizado, no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Atributo` | `varchar(12)` | **nao** | - | **PK** |
| 2 | `Lista` | `varchar(30)` | **nao** | - | **PK** |
| 3 | `UsaFisica` | `char(1)` | **nao** | - |  |
| 4 | `UsaJuridica` | `char(1)` | **nao** | - |  |
| 5 | `LinkExterno` | `varchar(40)` | sim | - |  |
| 6 | `NroAdic` | `numeric(18,0)` | sim | - |  |
| 7 | `DadoAdic` | `varchar(250)` | sim | - |  |
| 8 | `SeqAtrib` | `decimal(8,0)` | sim | - |  |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

**Referenciada por:** `IV_PcteAtrFx.Atributo`, `IV_PcteAtrFx.Lista`

---

### GE_Bairro

`classe: catalogo` · `3 colunas` · `619 linhas (snapshot 03/06/2026)` · `PK: SeqCidade, SeqBairro`

**Funcao:** Geografia. GE_Cidade (10.214) e GE_Bairro são usados; GE_Regiao e GE_Rota têm 0 linhas apesar de GE_Pessoa.SeqRegiao/SeqRota terem FK. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCidade` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_Cidade.SeqCidade` |
| 2 | `SeqBairro` | `decimal(5,0)` | **nao** | - | **PK** |
| 3 | `Bairro` | `varchar(35)` | sim | - |  |

**Referenciada por:** `GE_Pessoa.SeqBairro`, `GE_Pessoa.SeqCidade`, `GE_PessoaEnd.SeqBairro`, `GE_PessoaEnd.SeqCidade`

---

### GE_CampoExig

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Form, Campo, Tipo, SeqPolSeg`

**Funcao:** Motor de POLÍTICA DE SEGURANÇA: 11 políticas nomeadas × catálogo de 108 itens tipados × 254 valores efetivos, mais janela de horário e obrigatoriedade de campo. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Form` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `Campo` | `varchar(40)` | **nao** | - | **PK** |
| 3 | `Tipo` | `varchar(30)` | **nao** | - | **PK** |
| 4 | `SeqPolSeg` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_PolSeg.SeqPolSeg` |
| 5 | `Exige` | `numeric(1,0)` | sim | - |  |
| 6 | `Dtaalteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `Usualteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_CampoMemo

`classe: nucleo` · `4 colunas` · `3.129 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, CodAplicacao, Campo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de campanha de marketing, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com usuario (`SeqUsuario`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `CodAplicacao` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `Campo` | `varchar(30)` | **nao** | - | **PK** |
| 4 | `Valor` | `varchar(100)` | sim | - |  |

---

### GE_CampoPerm

`classe: nucleo` · `7 colunas` · `17 linhas (snapshot 03/06/2026)` · `PK: CodAplicacao, NroEmpresa, Campo, SeqUsuario`

**Funcao:** Permissão por CAMPO/widget da tela. 17 linhas, amarradas a nomes de controle Gupta (pbtCientes, dfnLatitude, dfnLongitude). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodAplicacao` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `Campo` | `varchar(30)` | **nao** | - | **PK** |
| 4 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 5 | `Permissao` | `char(1)` | sim | - |  |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### GE_CFinConj

`classe: vazia` · `22 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCFinFis` | `numeric(18,0)` | **nao** | - |  |
| 2 | `SeqPessoaConj` | `numeric(10,0)` | **nao** | - |  |
| 3 | `OutraRendaVlr` | `numeric(14,2)` | sim | - |  |
| 4 | `OutraRendaOrigem` | `varchar(50)` | sim | - |  |
| 5 | `OcpEmpresa` | `varchar(40)` | sim | - |  |
| 6 | `OcpCNPJ` | `numeric(13,0)` | sim | - |  |
| 7 | `OcpCNPJDig` | `numeric(2,0)` | sim | - |  |
| 8 | `OcpEndereco` | `varchar(40)` | sim | - |  |
| 9 | `OcpEnderecoNro` | `varchar(10)` | sim | - |  |
| 10 | `OcpEnderecoCmpl` | `varchar(30)` | sim | - |  |
| 11 | `OcpFoneDDD` | `varchar(2)` | sim | - |  |
| 12 | `OcpFone` | `numeric(12,0)` | sim | - |  |
| 13 | `OcpFoneCmpl` | `varchar(12)` | sim | - |  |
| 14 | `OcpSeqCidade` | `numeric(18,0)` | sim | - |  |
| 15 | `OcpEncarregado` | `varchar(20)` | sim | - |  |
| 16 | `OcpCidade` | `varchar(30)` | sim | - |  |
| 17 | `OcpFuncao` | `varchar(30)` | sim | - |  |
| 18 | `OcpBairro` | `varchar(20)` | sim | - |  |
| 19 | `OcpRenda` | `numeric(14,2)` | sim | - |  |
| 20 | `OcpCEP` | `numeric(8,0)` | sim | - |  |
| 21 | `OcpDtaAdmis` | `datetime` | sim | - |  |
| 22 | `OcpUF` | `varchar(2)` | sim | - |  |

---

### GE_CFinDocto

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de documento, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCFinFis` | `numeric(18,0)` | **nao** | - |  |
| 2 | `DocReqSeqPar` | `numeric(18,0)` | **nao** | - |  |
| 3 | `SeqDocto` | `numeric(18,0)` | sim | - | documento (`DMN_Doc.SeqDocto`) |
| 4 | `SeqDocTp` | `numeric(4,0)` | **nao** | - |  |
| 5 | `CondicaoDocSeqPar` | `numeric(18,0)` | sim | - |  |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_CFinFis

`classe: vazia` · `51 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCFinFis` | `numeric(18,0)` | **nao** | - |  |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Situacao` | `char(1)` | sim | - |  |
| 4 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 5 | `DtaAtualizacao` | `datetime` | sim | - |  |
| 6 | `DtaRenovacao` | `datetime` | sim | - |  |
| 7 | `VlrLimCred` | `numeric(14,2)` | sim | - |  |
| 8 | `CnfPessoa` | `varchar(150)` | sim | - |  |
| 9 | `OcpEmpresa` | `varchar(40)` | sim | - |  |
| 10 | `OcpCNPJ` | `numeric(13,0)` | sim | - |  |
| 11 | `OcpCNPJDig` | `numeric(2,0)` | sim | - |  |
| 12 | `OcpFoneDDD` | `varchar(2)` | sim | - |  |
| 13 | `OcpFone` | `numeric(12,0)` | sim | - |  |
| 14 | `OcpFoneCmpl` | `varchar(12)` | sim | - |  |
| 15 | `OcpEndereco` | `varchar(40)` | sim | - |  |
| 16 | `OcpEnderecoNro` | `varchar(10)` | sim | - |  |
| 17 | `OcpEnderecoCmpl` | `varchar(30)` | sim | - |  |
| 18 | `OcpSeqCidade` | `numeric(18,0)` | sim | - |  |
| 19 | `OcpEncarregado` | `varchar(20)` | sim | - |  |
| 20 | `OcpCidade` | `varchar(30)` | sim | - |  |
| 21 | `OcpFuncao` | `varchar(30)` | sim | - |  |
| 22 | `OcpBairro` | `varchar(20)` | sim | - |  |
| 23 | `OcpRenda` | `numeric(14,2)` | sim | - |  |
| 24 | `OcpCEP` | `numeric(8,0)` | sim | - |  |
| 25 | `OcpDtaAdmis` | `datetime` | sim | - |  |
| 26 | `OcpUF` | `varchar(2)` | sim | - |  |
| 27 | `OcpAnterior` | `varchar(40)` | sim | - |  |
| 28 | `OcpAFoneDDD` | `varchar(2)` | sim | - |  |
| 29 | `OcpAFone` | `numeric(12,0)` | sim | - |  |
| 30 | `OcpADtaAdmis` | `datetime` | sim | - |  |
| 31 | `OcpADtaDemis` | `datetime` | sim | - |  |
| 32 | `OutraRendaVlr` | `numeric(14,2)` | sim | - |  |
| 33 | `OutraRendaOrigem` | `varchar(50)` | sim | - |  |
| 34 | `FinancTeve` | `numeric(1,0)` | sim | - |  |
| 35 | `FinancVlrParcela` | `numeric(14,2)` | sim | - |  |
| 36 | `FinancBanco` | `varchar(25)` | sim | - |  |
| 37 | `FinancQuitado` | `numeric(1,0)` | sim | - |  |
| 38 | `FinancBem` | `varchar(20)` | sim | - |  |
| 39 | `TipDocExigido` | `char(3)` | sim | - |  |
| 40 | `OutraInf` | `varchar(250)` | sim | - |  |
| 41 | `CnfOcpTempo` | `numeric(4,1)` | sim | - |  |
| 42 | `CnfOcpRenda` | `numeric(14,2)` | sim | - |  |
| 43 | `CnfOcpSetor` | `varchar(20)` | sim | - |  |
| 44 | `CnfOcpResponsavel` | `varchar(20)` | sim | - |  |
| 45 | `CnfDta` | `datetime` | sim | - |  |
| 46 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 47 | `CnfObs` | `varchar(250)` | sim | - |  |
| 48 | `AprUsuario` | `varchar(20)` | sim | - |  |
| 49 | `AprDta` | `datetime` | sim | - |  |
| 50 | `AprObs` | `varchar(250)` | sim | - |  |
| 51 | `OcpFisicaJuridica` | `char(1)` | sim | - |  |

---

### GE_CFinRPes

`classe: vazia` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRPes` | `numeric(18,0)` | **nao** | - |  |
| 2 | `SeqCFinFis` | `numeric(18,0)` | **nao** | - |  |
| 3 | `NroRef` | `numeric(2,0)` | sim | - |  |
| 4 | `TipoRelacSeqPar` | `numeric(18,0)` | sim | - |  |
| 5 | `NOME` | `varchar(40)` | sim | - |  |
| 6 | `Fone1DDD` | `varchar(2)` | sim | - |  |
| 7 | `Fone1` | `numeric(12,0)` | sim | - |  |
| 8 | `Fone1Cmpl` | `varchar(12)` | sim | - |  |
| 9 | `Fone2DDD` | `varchar(2)` | sim | - |  |
| 10 | `Fone2` | `numeric(12,0)` | sim | - |  |
| 11 | `Fone2Cmpl` | `varchar(12)` | sim | - |  |
| 12 | `OBS` | `varchar(100)` | sim | - | texto livre |

---

### GE_Cidade

`classe: nucleo` · `19 colunas` · `10.214 linhas (snapshot 03/06/2026)` · `PK: SeqCidade`

**Funcao:** Geografia. GE_Cidade (10.214) e GE_Bairro são usados; GE_Regiao e GE_Rota têm 0 linhas apesar de GE_Pessoa.SeqRegiao/SeqRota terem FK. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCidade` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Cidade` | `varchar(50)` | **nao** | - |  |
| 3 | `Uf` | `varchar(2)` | **nao** | - |  |
| 4 | `Regiao` | `varchar(12)` | sim | - |  |
| 5 | `Populacao` | `decimal(8,0)` | sim | - |  |
| 6 | `DtaAniversario` | `datetime` | sim | - |  |
| 7 | `CEPInicial` | `varchar(12)` | sim | - |  |
| 8 | `CEPFinal` | `varchar(12)` | sim | - |  |
| 9 | `Ddd` | `varchar(5)` | sim | - |  |
| 10 | `Pais` | `varchar(25)` | sim | - |  |
| 11 | `DtaFeriado1` | `datetime` | sim | - |  |
| 12 | `DescFeriado1` | `varchar(20)` | sim | - |  |
| 13 | `DtaFeriado2` | `datetime` | sim | - |  |
| 14 | `DescFeriado2` | `varchar(20)` | sim | - |  |
| 15 | `ExgBairro` | `char(1)` | **nao** | - |  |
| 16 | `ExgLogradouro` | `char(1)` | **nao** | - |  |
| 17 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 18 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 19 | `INDDESCARTADDD` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GE_Bairro.SeqCidade`, `GE_Pessoa.SeqCidade`, `GE_PessoaEnd.SeqCidade`, `IVS_CartCid.SeqCidade`

---

### GE_CIdadePref

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqCidade, NroEmpresa`

**Funcao:** (c) GE_CIdadePref.SeqCidade tem FK para GE_Cidade_CRM (729 linhas, tabela legada) e não para GE_Cidade (10.214). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCidade` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_Cidade_CRM.SeqCidade` |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `Ordem` | `decimal(2,0)` | sim | - |  |

---

### GE_Cidade_BKPJUN

`classe: lixo/backup` · `19 colunas` · `9.966 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de geografia/enderecamento, no modulo `GE` (geral/plataforma).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (19 colunas) para manter o documento legivel._

---

### GE_Cidade_CRM

`classe: nucleo` · `18 colunas` · `729 linhas (snapshot 03/06/2026)` · `PK: SeqCidade`

**Funcao:** (c) GE_CIdadePref.SeqCidade tem FK para GE_Cidade_CRM (729 linhas, tabela legada) e não para GE_Cidade (10.214). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCidade` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Cidade` | `varchar(50)` | **nao** | - |  |
| 3 | `Uf` | `varchar(2)` | **nao** | - |  |
| 4 | `Regiao` | `varchar(12)` | sim | - |  |
| 5 | `Populacao` | `decimal(8,0)` | sim | - |  |
| 6 | `DtaAniversario` | `datetime` | sim | - |  |
| 7 | `CEPInicial` | `varchar(12)` | sim | - |  |
| 8 | `CEPFinal` | `varchar(12)` | sim | - |  |
| 9 | `Ddd` | `varchar(5)` | sim | - |  |
| 10 | `Pais` | `varchar(25)` | sim | - |  |
| 11 | `DtaFeriado1` | `datetime` | sim | - |  |
| 12 | `DescFeriado1` | `varchar(20)` | sim | - |  |
| 13 | `DtaFeriado2` | `datetime` | sim | - |  |
| 14 | `DescFeriado2` | `varchar(20)` | sim | - |  |
| 15 | `ExgBairro` | `char(1)` | **nao** | - |  |
| 16 | `ExgLogradouro` | `char(1)` | **nao** | - |  |
| 17 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 18 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

**Referenciada por:** `GE_CIdadePref.SeqCidade`

---

### GE_Col

`classe: catalogo` · `6 colunas` · `217 linhas (snapshot 03/06/2026)` · `PK: Tabela, Coluna`

**Funcao:** E a permissão por CAMPO (GE_CampoPerm/GE_CampoExig/GE_ColRegra) é um diferencial real que quase nenhum CRM de mercado tem: COPIAR. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tabela` | `varchar(30)` | **nao** | - | **PK**; FK -> `GE_Tab.Tabela` |
| 2 | `Coluna` | `varchar(30)` | **nao** | - | **PK** |
| 3 | `TipoDado` | `varchar(15)` | sim | - |  |
| 4 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 5 | `Instrucoes` | `varchar(250)` | sim | - |  |
| 6 | `IndExigido` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GE_ColRegra.Coluna`, `GE_ColRegra.Tabela`

---

### GE_ColPosition

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Objeto, SeqUsuario, Coluna`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Objeto` | `varchar(60)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `Coluna` | `varchar(40)` | **nao** | - | **PK** |
| 4 | `Posicao` | `decimal(4,0)` | sim | - |  |
| 5 | `Tamanho` | `decimal(6,4)` | sim | - |  |

---

### GE_ColRegra

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqColRegra`

**Funcao:** E a permissão por CAMPO (GE_CampoPerm/GE_CampoExig/GE_ColRegra) é um diferencial real que quase nenhum CRM de mercado tem: COPIAR. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqColRegra` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(30)` | **nao** | - | FK -> `GE_Col.Tabela` |
| 3 | `Coluna` | `varchar(30)` | **nao** | - | FK -> `GE_Col.Coluna` |
| 4 | `SeqPolSeg` | `numeric(6,0)` | **nao** | - |  |
| 5 | `Condicao` | `varchar(30)` | **nao** | - |  |
| 6 | `Regra` | `numeric(18,0)` | sim | - |  |
| 7 | `SeqPerfil` | `numeric(6,0)` | **nao** | - |  |
| 8 | `CKS` | `numeric(18,0)` | sim | - |  |

---

### GE_CONSHST

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCONSHST`

**Funcao:** Terceira geração do motor de consultas, esboçada e abandonada (1 / 0 / 0 / 0 linhas). Tinha modelo melhor: PK própria em cada tabela, SEQRELTEMPLATE, histórico com PK real. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONSHST` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - | FK -> `GE_CONSSQL.SEQCONSSQL` |
| 3 | `DETALHE` | `text(2147483647)` | **nao** | - |  |
| 4 | `CODUSUARIO` | `varchar(20)` | **nao** | - | login do usuario (varchar) |
| 5 | `DTAREALIZACAO` | `datetime` | **nao** | - | data de realizacao |

---

### GE_CONSSQL

`classe: catalogo` · `10 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQCONSSQL`

**Funcao:** Terceira geração do motor de consultas, esboçada e abandonada (1 / 0 / 0 / 0 linhas). Tinha modelo melhor: PK própria em cada tabela, SEQRELTEMPLATE, histórico com PK real. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQRELTEMPLATE` | `numeric(18,0)` | sim | - | FK -> `GE_RELTEMPLATE.SEQRELTEMPLATE` |
| 3 | `NOME` | `varchar(50)` | **nao** | - |  |
| 4 | `DESCRICAO` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `ADVERTENCIA` | `varchar(250)` | sim | - |  |
| 6 | `QUERY` | `text(2147483647)` | **nao** | - |  |
| 7 | `PREVIEW` | `numeric(1,0)` | sim | - |  |
| 8 | `SESSAO` | `varchar(50)` | sim | - |  |
| 9 | `QTDEUSO` | `numeric(18,0)` | sim | - |  |
| 10 | `INDRELSISTEMA` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GEP_EMAILSENDREL.SEQCONSSQL`, `GE_CONSHST.SEQCONSSQL`, `GE_CONSVAR.SEQCONSSQL`, `GE_EMAILAGD.SEQCONSSQL`, `GE_RelPastaCons.SEQCONSSQL`

---

### GE_Consulta

`classe: nucleo` · `29 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: Seqconsulta`

**Funcao:** Motor de relatórios LEGADO (2 e 3 linhas, ambas de 18/06/2012). Guarda no schema credenciais em texto claro para conexão a banco externo por relatório. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seqconsulta` | `numeric(6,0)` | **nao** | - | **PK** |
| 2 | `Cod` | `varchar(10)` | sim | - |  |
| 3 | `Seqconsultapai` | `numeric(6,0)` | sim | - |  |
| 4 | `Tipo` | `varchar(1)` | **nao** | - |  |
| 5 | `Consulta` | `varchar(50)` | **nao** | - |  |
| 6 | `Descricao` | `varchar(200)` | **nao** | - | descricao do registro |
| 7 | `Dtinclusao` | `datetime` | **nao** | - |  |
| 8 | `Instrucaosql` | `text(2147483647)` | sim | - |  |
| 9 | `Advertencia` | `varchar(250)` | sim | - |  |
| 10 | `Banco` | `varchar(15)` | sim | - |  |
| 11 | `Usuario` | `varchar(15)` | sim | - |  |
| 12 | `Senha` | `varchar(15)` | sim | - |  |
| 13 | `Conexao` | `varchar(1)` | sim | - |  |
| 14 | `Colunadado` | `varchar(100)` | sim | - |  |
| 15 | `Tipografico` | `numeric(6,0)` | sim | - |  |
| 16 | `Titulografico` | `varchar(80)` | sim | - |  |
| 17 | `Titulorodape` | `varchar(40)` | sim | - |  |
| 18 | `Tituloesquerdo` | `varchar(40)` | sim | - |  |
| 19 | `Legeixox` | `varchar(30)` | sim | - |  |
| 20 | `Colunatexto` | `varchar(30)` | sim | - |  |
| 21 | `Colunalegenda` | `varchar(30)` | sim | - |  |
| 22 | `Titulorel` | `varchar(150)` | sim | - |  |
| 23 | `Arqqrp` | `varchar(150)` | sim | - |  |
| 24 | `Qrpdireto` | `varchar(1)` | sim | - |  |
| 25 | `Checksum` | `numeric(18,0)` | sim | - |  |
| 26 | `DtaInicioUso` | `datetime` | sim | - |  |
| 27 | `DtaUltUso` | `datetime` | sim | - |  |
| 28 | `UltUsuario` | `varchar(20)` | sim | - |  |
| 29 | `QtdeUso` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `GE_ConsultaVar.Seqconsulta`

---

### GE_ConsultaVar

`classe: nucleo` · `10 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: Seqconsulta, Variavel`

**Funcao:** Motor de relatórios LEGADO (2 e 3 linhas, ambas de 18/06/2012). Guarda no schema credenciais em texto claro para conexão a banco externo por relatório. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seqconsulta` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `GE_Consulta.Seqconsulta` |
| 2 | `Variavel` | `varchar(3)` | **nao** | - | **PK** |
| 3 | `Descricao` | `varchar(40)` | **nao** | - | descricao do registro |
| 4 | `Instrucao` | `varchar(1000)` | sim | - |  |
| 5 | `Padrao` | `varchar(100)` | sim | - |  |
| 6 | `Minimo` | `numeric(15,5)` | sim | - |  |
| 7 | `Maximo` | `numeric(15,5)` | sim | - |  |
| 8 | `Geralista` | `varchar(250)` | sim | - |  |
| 9 | `Tipoin` | `varchar(1)` | sim | - |  |
| 10 | `Retorno` | `varchar(1)` | sim | - |  |

---

### GE_CONSVAR

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCONSVAR`

**Funcao:** Terceira geração do motor de consultas, esboçada e abandonada (1 / 0 / 0 / 0 linhas). Tinha modelo melhor: PK própria em cada tabela, SEQRELTEMPLATE, histórico com PK real. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONSVAR` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - | FK -> `GE_CONSSQL.SEQCONSSQL` |
| 3 | `VARIAVEL` | `varchar(20)` | **nao** | - |  |
| 4 | `ORDEM` | `numeric(18,0)` | **nao** | - |  |
| 5 | `DESCRICAO` | `varchar(250)` | sim | - | descricao do registro |
| 6 | `PADRAO` | `varchar(100)` | sim | - |  |
| 7 | `INSTRUCAO` | `varchar(250)` | sim | - |  |
| 8 | `QUERY` | `text(2147483647)` | sim | - |  |
| 9 | `TIPODADO` | `varchar(2)` | **nao** | - |  |
| 10 | `TUDO` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `GE_CONSVARLST.SEQCONSVAR`

---

### GE_CONSVARLST

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQCONSVARLST`

**Funcao:** Terceira geração do motor de consultas, esboçada e abandonada (1 / 0 / 0 / 0 linhas). Tinha modelo melhor: PK própria em cada tabela, SEQRELTEMPLATE, histórico com PK real. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQCONSVARLST` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQCONSVAR` | `numeric(18,0)` | **nao** | - | FK -> `GE_CONSVAR.SEQCONSVAR` |
| 3 | `VALOR` | `varchar(100)` | sim | - |  |

---

### GE_Contato

`classe: isolada` · `41 colunas` · `41.164 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqContato`

**Funcao:** Contatos (pessoas de contato) DENTRO de um cliente. Entidade FRACA: 41.276 linhas, 41 colunas, PK composta (SeqPessoa, SeqContato) — o contato não existe fora do cliente. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqContato` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 4 | `TipoContato` | `varchar(30)` | sim | - |  |
| 5 | `AreaAtuacao` | `varchar(20)` | sim | - |  |
| 6 | `Rg` | `varchar(20)` | sim | - |  |
| 7 | `Cpf` | `numeric(18,0)` | sim | - |  |
| 8 | `DigCPF` | `decimal(2,0)` | sim | - |  |
| 9 | `Saudacao` | `varchar(20)` | sim | - |  |
| 10 | `Contato` | `varchar(40)` | sim | - |  |
| 11 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 12 | `FoneNro1` | `decimal(12,0)` | sim | - |  |
| 13 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 14 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 15 | `FoneNro2` | `decimal(12,0)` | sim | - |  |
| 16 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 17 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 18 | `FaxNro` | `decimal(12,0)` | sim | - |  |
| 19 | `Sexo` | `char(1)` | sim | - |  |
| 20 | `EstadoCivil` | `char(1)` | sim | - |  |
| 21 | `DtaNascimento` | `datetime` | sim | - |  |
| 22 | `NivelDecisao` | `char(1)` | sim | - |  |
| 23 | `Posicionamento` | `char(1)` | sim | - |  |
| 24 | `ObsPessoal` | `varchar(250)` | sim | - |  |
| 25 | `Atributo1` | `varchar(20)` | sim | - |  |
| 26 | `Atributo2` | `varchar(20)` | sim | - |  |
| 27 | `Atributo3` | `varchar(20)` | sim | - |  |
| 28 | `AtribDta` | `datetime` | sim | - |  |
| 29 | `AtribNum` | `numeric(18,0)` | sim | - |  |
| 30 | `EMail` | `varchar(50)` | sim | - |  |
| 31 | `LinkWeb` | `numeric(1,0)` | sim | - |  |
| 32 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 33 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 34 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 35 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 36 | `Skype` | `varchar(70)` | sim | - |  |
| 37 | `INDWHATSAPPF1` | `numeric(1,0)` | sim | - |  |
| 38 | `INDWHATSAPPF2` | `numeric(1,0)` | sim | - |  |
| 39 | `INDWHATSAPPFX` | `numeric(1,0)` | sim | - |  |
| 40 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 41 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |

---

### GE_CONTATOAPP

`classe: vazia` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPESSOA, SEQCONTATO`

**Funcao:** Existem ainda GE_CONTATOAPP (0 linhas, login do contato no app com SENHA varchar(250) e FCMTOKEN) e GE_ContatoEmail (0 linhas). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SEQCONTATO` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `NOME` | `varchar(50)` | **nao** | - |  |
| 4 | `USUARIO` | `varchar(100)` | **nao** | - |  |
| 5 | `SENHA` | `varchar(250)` | sim | - |  |
| 6 | `INDATIVO` | `numeric(1,0)` | sim | - |  |
| 7 | `DTALOGINANT` | `datetime` | sim | - |  |
| 8 | `DTALOGIN` | `datetime` | sim | - |  |
| 9 | `FCMTOKEN` | `varchar(250)` | sim | - |  |
| 10 | `ENDPOINT` | `varchar(250)` | sim | - |  |
| 11 | `AUTH` | `varchar(250)` | sim | - |  |
| 12 | `P256DH` | `varchar(250)` | sim | - |  |

---

### GE_ContatoEmail

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqEmail` | `numeric(10,0)` | **nao** | - |  |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `SeqContato` | `numeric(4,0)` | **nao** | - |  |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_ContatoPapel

`classe: isolada` · `5 colunas` · `43.489 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqContato, Papel`

**Funcao:** Manter o conceito de N papéis por vínculo (GE_ContatoPapel acertou) e o `nivel_decisao` (85% de preenchimento prova que o vendedor usa). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqContato` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Papel` | `varchar(20)` | **nao** | - | **PK** |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_ContatoPapel_BKPJUN

`classe: lixo/backup` · `5 colunas` · `35.413 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de plano de contas, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### GE_ContatoPapel_ITA

`classe: lixo/backup` · `5 colunas` · `5.032 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de plano de contas, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### GE_Contato_BKPJUN

`classe: lixo/backup` · `41 colunas` · `28.051 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de plano de contas, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (41 colunas) para manter o documento legivel._

---

### GE_Contato_ITA

`classe: lixo/backup` · `41 colunas` · `10.866 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de plano de contas, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (41 colunas) para manter o documento legivel._

---

### GE_CTRL2

`classe: isolada` · `6 colunas` · `276 linhas (snapshot 03/06/2026)` · `PK: K01`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `K01` | `varchar(40)` | **nao** | - | **PK** |
| 2 | `DA` | `varchar(40)` | sim | - |  |
| 3 | `DD` | `varchar(40)` | sim | - |  |
| 4 | `DA1` | `varchar(40)` | sim | - |  |
| 5 | `N1` | `varchar(20)` | sim | - |  |
| 6 | `CHKSUM` | `varchar(50)` | sim | - |  |

---

### GE_CtrlE

`classe: isolada` · `9 colunas` · `16 linhas (snapshot 03/06/2026)` · `PK: K01`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `K01` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `Sfan` | `varchar(100)` | sim | - |  |
| 3 | `Srso` | `varchar(150)` | sim | - |  |
| 4 | `Snrd` | `varchar(40)` | sim | - |  |
| 5 | `Nrcg` | `varchar(40)` | sim | - |  |
| 6 | `Dgcg` | `varchar(12)` | sim | - |  |
| 7 | `CHKSUM` | `varchar(50)` | sim | - |  |
| 8 | `Dglb` | `varchar(40)` | sim | - |  |
| 9 | `Datu` | `datetime` | sim | - |  |

---

### GE_CtrlM

`classe: isolada` · `11 colunas` · `31 linhas (snapshot 03/06/2026)` · `PK: K01`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `K01` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `Dexp` | `varchar(20)` | sim | - |  |
| 3 | `Nusr` | `varchar(20)` | sim | - |  |
| 4 | `Ctemp` | `varchar(20)` | sim | - |  |
| 5 | `Nivel` | `varchar(20)` | sim | - |  |
| 6 | `Datu` | `datetime` | sim | - |  |
| 7 | `Dglb` | `varchar(40)` | sim | - |  |
| 8 | `CHKSUM` | `varchar(50)` | sim | - |  |
| 9 | `KDESC` | `varchar(250)` | sim | - |  |
| 10 | `DEXPTMP` | `varchar(20)` | sim | - |  |
| 11 | `DEXPQTD` | `varchar(20)` | sim | - |  |

---

### GE_CtrlME

`classe: isolada` · `6 colunas` · `36 linhas (snapshot 03/06/2026)` · `PK: K01`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `K01` | `varchar(40)` | **nao** | - | **PK** |
| 2 | `Dexp` | `varchar(20)` | sim | - |  |
| 3 | `Nusr` | `varchar(20)` | sim | - |  |
| 4 | `Datu` | `datetime` | sim | - |  |
| 5 | `Dglb` | `varchar(40)` | sim | - |  |
| 6 | `CHKSUM` | `varchar(50)` | sim | - |  |

---

### GE_CTRLMU

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `K01` | `varchar(30)` | **nao** | - |  |
| 3 | `DATU` | `datetime` | sim | - |  |
| 4 | `INDATIVO` | `numeric(1,0)` | sim | - |  |
| 5 | `INDTEMPORARIO` | `numeric(1,0)` | sim | - |  |
| 6 | `CHKSUM` | `varchar(50)` | sim | - |  |

---

### GE_CTRLUSR

`classe: isolada` · `6 colunas` · `33 linhas (snapshot 03/06/2026)` · `PK: K01`

**Funcao:** ControlSet de LICENCIAMENTO do fornecedor (módulos habilitados, quantidade de usuários, data de expiração). Conteúdo ofuscado em hexadecimal, ilegível por consulta. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `K01` | `varchar(40)` | **nao** | - | **PK** |
| 2 | `SPW` | `varchar(30)` | sim | - |  |
| 3 | `SNVL` | `varchar(20)` | sim | - |  |
| 4 | `DGER` | `varchar(40)` | sim | - |  |
| 5 | `CHKSUM` | `varchar(50)` | sim | - |  |
| 6 | `DATU` | `datetime` | sim | - |  |

---

### GE_DiaNaoUtil

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Data`

**Funcao:** Calendário útil: GE_FERIADO, GE_DiaNaoUtil, GEL_* e a política ATD_AG0_73_HPT 'agendar fora do período padrão de trabalho'; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Data` | `datetime` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 3 | `Motivo` | `varchar(100)` | sim | - |  |
| 4 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_Email

`classe: isolada` · `16 colunas` · `38.765 linhas (snapshot 03/06/2026)` · `PK: SeqEmail`

**Funcao:** TRÊS modelos de e-mail concorrentes. GE_Email (39.363 linhas) é o real e tem o melhor modelo de finalidade por canal. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqEmail` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `eMail` | `varchar(70)` | **nao** | - |  |
| 3 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `SeqUsuario` | `numeric(18,0)` | sim | - | usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 5 | `IndPreferencial` | `numeric(1,0)` | sim | - |  |
| 6 | `IndUsoMkt` | `numeric(1,0)` | sim | - |  |
| 7 | `IndUsoProfissional` | `numeric(1,0)` | sim | - |  |
| 8 | `IndUsoPessoal` | `numeric(1,0)` | sim | - |  |
| 9 | `IndUsoFiscal` | `numeric(1,0)` | sim | - |  |
| 10 | `IndEmUso` | `numeric(1,0)` | sim | - |  |
| 11 | `Senha` | `varchar(50)` | sim | - |  |
| 12 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 13 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 14 | `Obs` | `varchar(50)` | sim | - | texto livre |
| 15 | `MOTIVO` | `varchar(30)` | sim | - |  |
| 16 | `DTAEMAILATIVO` | `datetime` | sim | - |  |

---

### GE_EMAILAGD

`classe: nucleo` · `13 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILAGD`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de agenda (tarefa do usuario), no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEMAILAGD` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SEQEMAILTEMPLATE` | `numeric(18,0)` | **nao** | - | FK -> `GE_EMAILTEMPLATE.SEQEMAILTEMPLATE` |
| 3 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - | FK -> `GE_CONSSQL.SEQCONSSQL` |
| 4 | `RELANEXO` | `numeric(1,0)` | sim | - |  |
| 5 | `NOME` | `varchar(50)` | **nao** | - |  |
| 6 | `DESCRICAO` | `varchar(250)` | sim | - | descricao do registro |
| 7 | `PARA` | `varchar(250)` | sim | - |  |
| 8 | `CC` | `varchar(250)` | sim | - |  |
| 9 | `CCO` | `varchar(250)` | sim | - |  |
| 10 | `INTERVALOEXEC` | `numeric(18,0)` | sim | - |  |
| 11 | `PLANOEXEC` | `varchar(250)` | sim | - |  |
| 12 | `DTAINICIO` | `datetime` | sim | - |  |
| 13 | `FILTROVALOR` | `varchar(250)` | sim | - |  |

---

### GE_EMAILINVALIDO

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: EMAIL`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `EMAIL` | `varchar(200)` | **nao** | - | **PK** |
| 2 | `ORIGEM` | `varchar(20)` | sim | - | sistema de origem do dado |
| 3 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |

---

### GE_EmailSpam

`classe: vazia` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqEmailSpam`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqEmailSpam` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `eMail` | `varchar(70)` | sim | - |  |
| 3 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_EMAILTEMPLATE

`classe: nucleo` · `6 colunas` · `8 linhas (snapshot 03/06/2026)` · `PK: SEQEMAILTEMPLATE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de e-mail, no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQEMAILTEMPLATE` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TIPO` | `varchar(20)` | **nao** | - |  |
| 3 | `NOME` | `varchar(50)` | **nao** | - |  |
| 4 | `DESCRICAO` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `ASSUNTO` | `varchar(250)` | sim | - |  |
| 6 | `MENSAGEM` | `text(2147483647)` | **nao** | - |  |

**Referenciada por:** `GE_EMAILAGD.SEQEMAILTEMPLATE`

---

### GE_Empresa

`classe: catalogo` · `33 colunas` · `18 linhas (snapshot 03/06/2026)` · `PK: NroEmpresa`

**Funcao:** As 18 filiais (17 Tracbel Agro + Colorado Equipamentos). Chave de multiempresa presente em quase toda tabela de permissão. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 2 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 3 | `Fantasia` | `varchar(30)` | **nao** | - |  |
| 4 | `RazaoSocial` | `varchar(60)` | sim | - |  |
| 5 | `NomeReduzido` | `varchar(12)` | sim | - |  |
| 6 | `Endereco` | `varchar(40)` | sim | - |  |
| 7 | `EnderecoNRO` | `varchar(10)` | sim | - |  |
| 8 | `Bairro` | `varchar(15)` | sim | - |  |
| 9 | `Cep` | `varchar(12)` | sim | - |  |
| 10 | `Cidade` | `varchar(25)` | sim | - |  |
| 11 | `Estado` | `varchar(2)` | sim | - |  |
| 12 | `InscrEstadual` | `varchar(20)` | sim | - |  |
| 13 | `InscrMunicipal` | `varchar(20)` | sim | - |  |
| 14 | `OutraInscricao` | `varchar(20)` | sim | - |  |
| 15 | `Pais` | `varchar(25)` | sim | - |  |
| 16 | `NroCGC` | `decimal(15,0)` | sim | - |  |
| 17 | `DigCGC` | `decimal(2,0)` | sim | - |  |
| 18 | `FoneNro` | `decimal(8,0)` | sim | - |  |
| 19 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 20 | `FaxNro` | `decimal(8,0)` | sim | - |  |
| 21 | `ArqLogo` | `varchar(100)` | sim | - |  |
| 22 | `Matriz` | `decimal(3,0)` | sim | - |  |
| 23 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 24 | `email` | `varchar(50)` | sim | - |  |
| 25 | `homepage` | `varchar(50)` | sim | - |  |
| 26 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 27 | `EmpSeguranca` | `decimal(3,0)` | sim | - |  |
| 28 | `Ddd` | `varchar(5)` | sim | - |  |
| 29 | `Fone` | `varchar(15)` | sim | - |  |
| 30 | `Fax` | `varchar(15)` | sim | - |  |
| 31 | `Sigla` | `varchar(4)` | sim | - |  |
| 32 | `FusoHorario` | `decimal(2,0)` | sim | - |  |
| 33 | `Regional` | `varchar(15)` | sim | - |  |

**Referenciada por:** `GE_ModuloPerm.NroEmpresa`, `GE_Permissao.NroEmpresa`, `IV_CodPrcEmpr.NroEmpresa`

---

### GE_FCadConj

`classe: vazia` · `43 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqFCadF`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFCadF` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_FCadF.SeqFCadF` |
| 2 | `Nome` | `varchar(50)` | sim | - |  |
| 3 | `NroCGCCPF` | `decimal(13,0)` | sim | - |  |
| 4 | `DigCGCCPF` | `decimal(2,0)` | sim | - |  |
| 5 | `RG` | `varchar(25)` | sim | - |  |
| 6 | `DtaNascimento` | `datetime` | sim | - |  |
| 7 | `OcpTipo` | `varchar(3)` | sim | - |  |
| 8 | `OcpEmpresa` | `varchar(40)` | sim | - |  |
| 9 | `OcpCNPJ` | `decimal(13,0)` | sim | - |  |
| 10 | `OcpCNPJDig` | `decimal(2,0)` | sim | - |  |
| 11 | `OcpEndereco` | `varchar(40)` | sim | - |  |
| 12 | `OcpSeqCidade` | `numeric(18,0)` | sim | - |  |
| 13 | `OcpCidade` | `varchar(30)` | sim | - |  |
| 14 | `OcpBairro` | `varchar(20)` | sim | - |  |
| 15 | `OcpUF` | `varchar(2)` | sim | - |  |
| 16 | `OcpCEP` | `decimal(8,0)` | sim | - |  |
| 17 | `OcpFone` | `decimal(12,0)` | sim | - |  |
| 18 | `OcpEncarregado` | `varchar(20)` | sim | - |  |
| 19 | `OcpFuncao` | `varchar(30)` | sim | - |  |
| 20 | `OcpRenda` | `decimal(15,2)` | sim | - |  |
| 21 | `OcpDtaAdmis` | `datetime` | sim | - |  |
| 22 | `OcpAnterior` | `varchar(40)` | sim | - |  |
| 23 | `OcpAFone` | `decimal(11,0)` | sim | - |  |
| 24 | `OcpADtaAdmis` | `datetime` | sim | - |  |
| 25 | `OcpADtaDemis` | `datetime` | sim | - |  |
| 26 | `Ocp2Origem` | `varchar(40)` | sim | - |  |
| 27 | `Ocp2Endereco` | `varchar(40)` | sim | - |  |
| 28 | `Ocp2SeqCidade` | `numeric(18,0)` | sim | - |  |
| 29 | `Ocp2Cidade` | `varchar(30)` | sim | - |  |
| 30 | `Ocp2Bairro` | `varchar(20)` | sim | - |  |
| 31 | `Ocp2UF` | `varchar(2)` | sim | - |  |
| 32 | `Ocp2CEP` | `decimal(8,0)` | sim | - |  |
| 33 | `Ocp2Fone` | `decimal(12,0)` | sim | - |  |
| 34 | `Ocp2Encarregado` | `varchar(20)` | sim | - |  |
| 35 | `Ocp2Funcao` | `varchar(30)` | sim | - |  |
| 36 | `Ocp2Renda` | `decimal(15,2)` | sim | - |  |
| 37 | `Ocp2DtaAdmis` | `datetime` | sim | - |  |
| 38 | `OcpContador` | `varchar(40)` | sim | - |  |
| 39 | `OcpCtdFone` | `decimal(11,0)` | sim | - |  |
| 40 | `CnfOcpSetor` | `varchar(20)` | sim | - |  |
| 41 | `CnfDta` | `datetime` | sim | - |  |
| 42 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 43 | `CnfObs` | `varchar(250)` | sim | - |  |

---

### GE_FCadF

`classe: vazia` · `97 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqFCadF`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFCadF` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `ObjFinanc` | `numeric(1,0)` | sim | - |  |
| 4 | `ObjConsor` | `numeric(1,0)` | sim | - |  |
| 5 | `ObjCredProp` | `numeric(1,0)` | sim | - |  |
| 6 | `Avalista` | `numeric(1,0)` | sim | - |  |
| 7 | `Situacao` | `char(1)` | sim | - |  |
| 8 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 9 | `DtaAtualizacao` | `datetime` | sim | - |  |
| 10 | `DtaRenovacao` | `datetime` | sim | - |  |
| 11 | `VlrLimCred` | `decimal(15,2)` | sim | - |  |
| 12 | `CnfPessoa` | `varchar(150)` | sim | - |  |
| 13 | `OcpTipo` | `varchar(3)` | sim | - |  |
| 14 | `OcpEmpresa` | `varchar(40)` | sim | - |  |
| 15 | `OcpCNPJ` | `decimal(13,0)` | sim | - |  |
| 16 | `OcpCNPJDig` | `decimal(2,0)` | sim | - |  |
| 17 | `OcpEndereco` | `varchar(40)` | sim | - |  |
| 18 | `OcpSeqCidade` | `numeric(18,0)` | sim | - |  |
| 19 | `OcpCidade` | `varchar(30)` | sim | - |  |
| 20 | `OcpBairro` | `varchar(20)` | sim | - |  |
| 21 | `OcpCEP` | `decimal(8,0)` | sim | - |  |
| 22 | `OcpUF` | `varchar(2)` | sim | - |  |
| 23 | `OcpFone` | `decimal(12,0)` | sim | - |  |
| 24 | `OcpEncarregado` | `varchar(20)` | sim | - |  |
| 25 | `OcpFuncao` | `varchar(30)` | sim | - |  |
| 26 | `OcpRenda` | `decimal(15,2)` | sim | - |  |
| 27 | `OcpDtaAdmis` | `datetime` | sim | - |  |
| 28 | `OcpAnterior` | `varchar(40)` | sim | - |  |
| 29 | `OcpAFone` | `decimal(11,0)` | sim | - |  |
| 30 | `OcpADtaAdmis` | `datetime` | sim | - |  |
| 31 | `OcpADtaDemis` | `datetime` | sim | - |  |
| 32 | `Ocp2Origem` | `varchar(40)` | sim | - |  |
| 33 | `Ocp2Endereco` | `varchar(40)` | sim | - |  |
| 34 | `Ocp2SeqCidade` | `numeric(18,0)` | sim | - |  |
| 35 | `Ocp2Cidade` | `varchar(30)` | sim | - |  |
| 36 | `Ocp2Bairro` | `varchar(20)` | sim | - |  |
| 37 | `Ocp2UF` | `varchar(2)` | sim | - |  |
| 38 | `Ocp2CEP` | `decimal(8,0)` | sim | - |  |
| 39 | `Ocp2Fone` | `decimal(12,0)` | sim | - |  |
| 40 | `Ocp2Encarregado` | `varchar(20)` | sim | - |  |
| 41 | `Ocp2Funcao` | `varchar(30)` | sim | - |  |
| 42 | `Ocp2Renda` | `decimal(15,2)` | sim | - |  |
| 43 | `Ocp2DtaAdmis` | `datetime` | sim | - |  |
| 44 | `OcpContador` | `varchar(40)` | sim | - |  |
| 45 | `OcpCtdFone` | `decimal(11,0)` | sim | - |  |
| 46 | `MorTipo` | `varchar(3)` | sim | - |  |
| 47 | `MorTempoRes` | `decimal(4,1)` | sim | - |  |
| 48 | `MorTempoResAnt` | `decimal(4,1)` | sim | - |  |
| 49 | `MorTempoResCid` | `decimal(4,1)` | sim | - |  |
| 50 | `MorImob` | `varchar(30)` | sim | - |  |
| 51 | `MorImobFone` | `decimal(11,0)` | sim | - |  |
| 52 | `MorImobCtto` | `varchar(20)` | sim | - |  |
| 53 | `MorImobAnt` | `varchar(30)` | sim | - |  |
| 54 | `MorImobAFone` | `decimal(11,0)` | sim | - |  |
| 55 | `MorImobACtto` | `varchar(20)` | sim | - |  |
| 56 | `PI1Tipo` | `varchar(20)` | sim | - |  |
| 57 | `PI1Matric` | `varchar(15)` | sim | - |  |
| 58 | `PI1Vlr` | `decimal(15,2)` | sim | - |  |
| 59 | `PI1Local` | `varchar(80)` | sim | - |  |
| 60 | `PI2Tipo` | `varchar(20)` | sim | - |  |
| 61 | `PI2Matric` | `varchar(15)` | sim | - |  |
| 62 | `PI2Vlr` | `decimal(15,2)` | sim | - |  |
| 63 | `PI2Local` | `varchar(80)` | sim | - |  |
| 64 | `PA1Tipo` | `varchar(20)` | sim | - |  |
| 65 | `PA1Marca` | `varchar(20)` | sim | - |  |
| 66 | `PA1Modelo` | `varchar(20)` | sim | - |  |
| 67 | `PA1Ano` | `decimal(4,0)` | sim | - |  |
| 68 | `PA1Vlr` | `decimal(15,2)` | sim | - |  |
| 69 | `PA1Placa` | `varchar(7)` | sim | - |  |
| 70 | `PA1Alienado` | `numeric(1,0)` | sim | - |  |
| 71 | `PA1Qtde` | `decimal(4,0)` | sim | - |  |
| 72 | `PA1Obs` | `varchar(100)` | sim | - |  |
| 73 | `PA2Tipo` | `varchar(20)` | sim | - |  |
| 74 | `Pa2Marca` | `varchar(20)` | sim | - |  |
| 75 | `PA2Modelo` | `varchar(20)` | sim | - |  |
| 76 | `PA2Ano` | `decimal(4,0)` | sim | - |  |
| 77 | `PA2Vlr` | `decimal(15,2)` | sim | - |  |
| 78 | `PA2Placa` | `varchar(7)` | sim | - |  |
| 79 | `PA2Alienado` | `numeric(1,0)` | sim | - |  |
| 80 | `PA2Qtde` | `decimal(4,0)` | sim | - |  |
| 81 | `PA2Obs` | `varchar(100)` | sim | - |  |
| 82 | `FinancTeve` | `numeric(1,0)` | sim | - |  |
| 83 | `FinancBanco` | `varchar(25)` | sim | - |  |
| 84 | `FinancQuitado` | `numeric(1,0)` | sim | - |  |
| 85 | `FinancBem` | `varchar(20)` | sim | - |  |
| 86 | `OutraInf` | `varchar(250)` | sim | - |  |
| 87 | `CnfOcpTempo` | `decimal(4,1)` | sim | - |  |
| 88 | `CnfOcpRenda` | `decimal(15,2)` | sim | - |  |
| 89 | `CnfOcpSetor` | `varchar(20)` | sim | - |  |
| 90 | `CnfOcpResponsavel` | `varchar(20)` | sim | - |  |
| 91 | `CnfDta` | `datetime` | sim | - |  |
| 92 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 93 | `CnfObs` | `varchar(250)` | sim | - |  |
| 94 | `AprUsuario` | `varchar(20)` | sim | - |  |
| 95 | `AprDta` | `datetime` | sim | - |  |
| 96 | `AprObs` | `varchar(250)` | sim | - |  |
| 97 | `FinancVlrParcela` | `decimal(15,2)` | sim | - |  |

**Referenciada por:** `GE_FCadConj.SeqFCadF`, `GE_FCadRPes.SeqFCadF`

---

### GE_FCadInstCred

`classe: vazia` · `13 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqFCadIC`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFCadIC` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `TipoFcad` | `char(1)` | **nao** | - |  |
| 4 | `SeqFcad` | `numeric(18,0)` | **nao** | - |  |
| 5 | `InstCred` | `varchar(12)` | sim | - |  |
| 6 | `PendCheque` | `char(1)` | sim | - |  |
| 7 | `Protesto` | `char(1)` | sim | - |  |
| 8 | `Acao` | `char(1)` | sim | - | acao (`IV_Acao.Acao`) |
| 9 | `DtaUltRegistro` | `datetime` | sim | - |  |
| 10 | `Pontual` | `char(1)` | sim | - |  |
| 11 | `CnfDta` | `datetime` | sim | - |  |
| 12 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 13 | `CnfObs` | `varchar(250)` | sim | - |  |

---

### GE_FcadRBanc

`classe: vazia` · `18 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRBanc`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRBanc` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `TipoFcad` | `char(1)` | **nao** | - |  |
| 4 | `SeqFcad` | `numeric(18,0)` | **nao** | - |  |
| 5 | `NroRef` | `decimal(1,0)` | **nao** | - |  |
| 6 | `SeqRef` | `numeric(18,0)` | sim | - |  |
| 7 | `Banco` | `varchar(30)` | sim | - |  |
| 8 | `Agencia` | `varchar(20)` | sim | - |  |
| 9 | `CtaCorr` | `varchar(20)` | sim | - |  |
| 10 | `DtaAberturaCta` | `datetime` | sim | - |  |
| 11 | `Fone` | `decimal(11,0)` | sim | - |  |
| 12 | `Contato` | `varchar(30)` | sim | - |  |
| 13 | `CartCred1` | `varchar(10)` | sim | - |  |
| 14 | `CartCred2` | `varchar(10)` | sim | - |  |
| 15 | `SeqPessoaRef` | `numeric(10,0)` | sim | - |  |
| 16 | `CnfDta` | `datetime` | sim | - |  |
| 17 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 18 | `CnfObs` | `varchar(250)` | sim | - |  |

---

### GE_FcadRCom

`classe: vazia` · `20 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRCom`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRCom` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `TipoFcad` | `char(1)` | **nao** | - |  |
| 4 | `TipoRef` | `char(1)` | sim | - |  |
| 5 | `SeqFcad` | `numeric(18,0)` | **nao** | - |  |
| 6 | `NroRef` | `decimal(1,0)` | **nao** | - |  |
| 7 | `Empresa` | `varchar(40)` | sim | - |  |
| 8 | `Fone` | `decimal(11,0)` | sim | - |  |
| 9 | `Contato` | `varchar(30)` | sim | - |  |
| 10 | `SeqPessoaRef` | `numeric(10,0)` | sim | - |  |
| 11 | `CnfDta` | `datetime` | sim | - |  |
| 12 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 13 | `CnfContato` | `varchar(20)` | sim | - |  |
| 14 | `CnfTempoCliente` | `decimal(4,1)` | sim | - |  |
| 15 | `CnfPontual` | `char(1)` | sim | - |  |
| 16 | `CnfTempoAtraso` | `decimal(4,0)` | sim | - |  |
| 17 | `CnfDtaUltCompra` | `datetime` | sim | - |  |
| 18 | `CnfVlrUltCompra` | `decimal(15,2)` | sim | - |  |
| 19 | `CnfVlrMedio` | `decimal(15,2)` | sim | - |  |
| 20 | `CnfObs` | `varchar(250)` | sim | - |  |

---

### GE_FCadRPes

`classe: vazia` · `33 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRPes`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRPes` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqFCadF` | `numeric(18,0)` | **nao** | - | FK -> `GE_FCadF.SeqFCadF` |
| 3 | `NroRef` | `decimal(1,0)` | sim | - |  |
| 4 | `Nome` | `varchar(40)` | sim | - |  |
| 5 | `GrauRelacionam` | `varchar(20)` | sim | - |  |
| 6 | `NroCGCCPF` | `decimal(13,0)` | sim | - |  |
| 7 | `DigCGCCPF` | `decimal(2,0)` | sim | - |  |
| 8 | `PercPart` | `decimal(6,2)` | sim | - |  |
| 9 | `DtaEntradaSocio` | `datetime` | sim | - |  |
| 10 | `Endereco` | `varchar(40)` | sim | - |  |
| 11 | `SeqCidade` | `numeric(18,0)` | sim | - |  |
| 12 | `Cidade` | `varchar(30)` | sim | - |  |
| 13 | `Bairro` | `varchar(20)` | sim | - |  |
| 14 | `UF` | `varchar(2)` | sim | - |  |
| 15 | `Fone1` | `decimal(12,0)` | sim | - |  |
| 16 | `Fone2` | `decimal(12,0)` | sim | - |  |
| 17 | `Fone3` | `decimal(12,0)` | sim | - |  |
| 18 | `Obs` | `varchar(100)` | sim | - | texto livre |
| 19 | `SeqPessoaRef` | `numeric(10,0)` | sim | - |  |
| 20 | `CnfDta` | `datetime` | sim | - |  |
| 21 | `CnfUsuario` | `varchar(20)` | sim | - |  |
| 22 | `CnfContato` | `varchar(20)` | sim | - |  |
| 23 | `CnfGrauRelac` | `varchar(20)` | sim | - |  |
| 24 | `CnfTempoRes` | `decimal(4,1)` | sim | - |  |
| 25 | `CnfObs` | `varchar(120)` | sim | - |  |
| 26 | `IndAssinaEmpr` | `numeric(1,0)` | sim | - |  |
| 27 | `EstadoCivil` | `char(1)` | sim | - |  |
| 28 | `CEP` | `decimal(8,0)` | sim | - |  |
| 29 | `Renda` | `decimal(15,2)` | sim | - |  |
| 30 | `ConjNome` | `varchar(50)` | sim | - |  |
| 31 | `ConjNroCPF` | `decimal(13,0)` | sim | - |  |
| 32 | `ConjDigCPF` | `decimal(2,0)` | sim | - |  |
| 33 | `ConjDtaNascimento` | `datetime` | sim | - |  |

---

### GE_FERIADO

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQFERIADO`

**Funcao:** Calendário útil: GE_FERIADO, GE_DiaNaoUtil, GEL_* e a política ATD_AG0_73_HPT 'agendar fora do período padrão de trabalho'; (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQFERIADO` | `numeric(4,0)` | **nao** | - | **PK** |
| 2 | `DATA` | `datetime` | **nao** | - |  |
| 3 | `INDFIXO` | `numeric(1,0)` | sim | - |  |
| 4 | `DESCRICAO` | `varchar(40)` | **nao** | - | descricao do registro |
| 5 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 6 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `TIPO` | `char(1)` | sim | - |  |

---

### GE_Figura

`classe: isolada` · `8 colunas` · `2.078 linhas (snapshot 03/06/2026)` · `PK: Tipo, Codigo, NroEmpresa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `GE` (geral/plataforma). As colunas confirmam escopo multiempresa (`NroEmpresa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tipo` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `Codigo` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 4 | `Figura` | `text(2147483647)` | sim | - |  |
| 5 | `Arquivo` | `varchar(100)` | sim | - |  |
| 6 | `TipoArquivo` | `decimal(4,0)` | sim | - |  |
| 7 | `TamanhoArquivo` | `numeric(18,0)` | sim | - |  |
| 8 | `FIGURABIN` | `image(2147483647)` | sim | - |  |

---

### GE_FONEINVALIDO

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: TELEFONE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `TELEFONE` | `numeric(11,0)` | **nao** | - | **PK** |
| 2 | `ORIGEM` | `varchar(20)` | sim | - | sistema de origem do dado |
| 3 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |

---

### GE_Grafico

`classe: vazia` · `26 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de recurso de apresentacao/relatorio, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqGraf` | `numeric(18,0)` | **nao** | - |  |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `Kn1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `Kn2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `Ks1` | `varchar(20)` | **nao** | - |  |
| 6 | `Titulo` | `varchar(40)` | sim | - |  |
| 7 | `TituloEsquerdo` | `varchar(40)` | sim | - |  |
| 8 | `TituloRodape` | `varchar(40)` | sim | - |  |
| 9 | `Tipo` | `numeric(2,0)` | sim | - |  |
| 10 | `Estilo` | `numeric(2,0)` | sim | - |  |
| 11 | `True3D` | `numeric(1,0)` | sim | - |  |
| 12 | `LabelAtivo` | `numeric(1,0)` | sim | - |  |
| 13 | `LabelPerc` | `numeric(1,0)` | sim | - |  |
| 14 | `LabelComLegenda` | `numeric(1,0)` | sim | - |  |
| 15 | `QtdeMaxItens` | `numeric(2,0)` | sim | - |  |
| 16 | `Grid` | `numeric(1,0)` | sim | - |  |
| 17 | `ColLegenda` | `varchar(30)` | sim | - |  |
| 18 | `ColDado` | `varchar(100)` | sim | - |  |
| 19 | `ColCor` | `varchar(40)` | sim | - |  |
| 20 | `LegendaPos` | `numeric(2,0)` | sim | - |  |
| 21 | `YEscalaEstilo` | `numeric(2,0)` | sim | - |  |
| 22 | `YEscalaMax` | `numeric(18,0)` | sim | - |  |
| 23 | `YEscalaQtde` | `numeric(2,0)` | sim | - |  |
| 24 | `XEscalaEstilo` | `numeric(2,0)` | sim | - |  |
| 25 | `XEscalaMax` | `numeric(18,0)` | sim | - |  |
| 26 | `XEscalaQtde` | `numeric(2,0)` | sim | - |  |

---

### GE_Help

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqAplicacao`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de recurso de apresentacao/relatorio, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAplicacao` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Aplicacao.SeqAplicacao` |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `Help` | `text(2147483647)` | sim | - |  |

---

### GE_IMPORTA_CART

`classe: isolada` · `52 colunas` · `22 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de carteira de clientes, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SEQPESSOAPRC` | `numeric(10,0)` | sim | - |  |
| 3 | `Status` | `char(1)` | sim | - | status - validar dominio real por tabela |
| 4 | `NomeRazao` | `varchar(70)` | sim | - |  |
| 5 | `Fantasia` | `varchar(50)` | sim | - |  |
| 6 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 7 | `DtaNascFund` | `datetime` | sim | - |  |
| 8 | `Cidade` | `varchar(50)` | sim | - |  |
| 9 | `Uf` | `varchar(2)` | sim | - |  |
| 10 | `Pais` | `varchar(25)` | sim | - |  |
| 11 | `Bairro` | `varchar(50)` | sim | - |  |
| 12 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 13 | `Logradouro` | `varchar(80)` | sim | - |  |
| 14 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 15 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 16 | `Cep` | `varchar(12)` | sim | - |  |
| 17 | `NroCGCCPF` | `decimal(13,0)` | sim | - |  |
| 18 | `DigCGCCPF` | `decimal(2,0)` | sim | - |  |
| 19 | `Atividade` | `varchar(30)` | sim | - |  |
| 20 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 21 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 22 | `FoneNro1` | `decimal(12,0)` | sim | - |  |
| 23 | `Tipo1` | `numeric(1,0)` | sim | - |  |
| 24 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 25 | `FoneNro2` | `decimal(12,0)` | sim | - |  |
| 26 | `Tipo2` | `numeric(1,0)` | sim | - |  |
| 27 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 28 | `FoneNro3` | `decimal(12,0)` | sim | - |  |
| 29 | `Tipo3` | `numeric(1,0)` | sim | - |  |
| 30 | `FoneDDD4` | `varchar(5)` | sim | - |  |
| 31 | `FoneNro4` | `decimal(12,0)` | sim | - |  |
| 32 | `Tipo4` | `numeric(1,0)` | sim | - |  |
| 33 | `FoneDDD5` | `varchar(5)` | sim | - |  |
| 34 | `FoneNro5` | `decimal(12,0)` | sim | - |  |
| 35 | `Tipo5` | `numeric(1,0)` | sim | - |  |
| 36 | `FoneDDD6` | `varchar(5)` | sim | - |  |
| 37 | `FoneNro6` | `decimal(12,0)` | sim | - |  |
| 38 | `Tipo6` | `numeric(1,0)` | sim | - |  |
| 39 | `FoneDDD7` | `varchar(5)` | sim | - |  |
| 40 | `FoneNro7` | `decimal(12,0)` | sim | - |  |
| 41 | `Tipo7` | `numeric(1,0)` | sim | - |  |
| 42 | `FoneDDD8` | `varchar(5)` | sim | - |  |
| 43 | `FoneNro8` | `decimal(12,0)` | sim | - |  |
| 44 | `Tipo8` | `numeric(1,0)` | sim | - |  |
| 45 | `Email1` | `varchar(70)` | sim | - |  |
| 46 | `Email2` | `varchar(70)` | sim | - |  |
| 47 | `Email3` | `varchar(70)` | sim | - |  |
| 48 | `CNAE` | `varchar(15)` | sim | - |  |
| 49 | `DESCCNAE` | `varchar(180)` | sim | - |  |
| 50 | `NJUR` | `varchar(15)` | sim | - |  |
| 51 | `DESCNJUR` | `varchar(80)` | sim | - |  |
| 52 | `ORIGEMRECEITA` | `varchar(80)` | sim | - |  |

---

### GE_IntCtrl

`classe: vazia` · `14 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Tipo, NroEmpresa, StrK, NroK`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de controle/condicoes de uso, no modulo `GE` (geral/plataforma). As colunas confirmam escopo multiempresa (`NroEmpresa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tipo` | `varchar(10)` | **nao** | - | **PK** |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `StrK` | `varchar(40)` | **nao** | - | **PK** |
| 4 | `NroK` | `numeric(18,0)` | **nao** | - | **PK** |
| 5 | `Ind1` | `numeric(1,0)` | sim | - |  |
| 6 | `Ind2` | `numeric(1,0)` | sim | - |  |
| 7 | `Str1` | `varchar(100)` | sim | - |  |
| 8 | `Nro1` | `numeric(18,0)` | sim | - |  |
| 9 | `Dta1` | `datetime` | sim | - |  |
| 10 | `Obs` | `varchar(150)` | sim | - | texto livre |
| 11 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 12 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 13 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 14 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_LgTb

`classe: isolada` · `10 colunas` · `11.861.777 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Log genérico da geração anterior. 11.861.777 linhas CONGELADAS entre 23/12/2018 e 05/06/2023, nunca expurgadas após a migração para GE_LOG_*. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tb` | `varchar(25)` | **nao** | - |  |
| 2 | `Kn1` | `numeric(18,0)` | **nao** | - |  |
| 3 | `Kn2` | `numeric(18,0)` | **nao** | - |  |
| 4 | `Ks` | `varchar(40)` | **nao** | - |  |
| 5 | `DtaLog` | `datetime` | **nao** | - |  |
| 6 | `Usr` | `varchar(20)` | sim | - |  |
| 7 | `CodApl` | `varchar(20)` | sim | - |  |
| 8 | `Obs` | `varchar(250)` | sim | - | texto livre |
| 9 | `Nivel` | `char(1)` | sim | - |  |
| 10 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |

---

### GE_Log2

`classe: isolada` · `14 colunas` · `4.394.779 linhas (snapshot 03/06/2026)` · `PK: SEQLOG`

**Funcao:** Terceira geração de log, ATIVA em paralelo desde 19/12/2011 (4.394.779 linhas). Esquema melhor que as outras: identifica estação e link tipado. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 2 | `Sistema` | `varchar(20)` | sim | - |  |
| 3 | `Modulo` | `varchar(20)` | sim | - |  |
| 4 | `Aplicacao` | `varchar(30)` | sim | - |  |
| 5 | `CodUsuario` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 6 | `Data` | `datetime` | sim | - |  |
| 7 | `Estacao` | `varchar(20)` | sim | - |  |
| 8 | `Nivel` | `numeric(1,0)` | sim | - |  |
| 9 | `Resumo` | `varchar(30)` | sim | - |  |
| 10 | `Detalhe` | `varchar(1000)` | sim | - |  |
| 11 | `LinkTipo` | `varchar(20)` | sim | - |  |
| 12 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 13 | `LinkSerie` | `varchar(250)` | sim | - | chave de vinculo com documento do ERP |
| 14 | `SEQLOG` | `numeric(18,0)` | **nao** | - | **PK** |

---

### GE_LogAtividade

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqLogAtividade`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de atividade, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqLogAtividade` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `varchar(20)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | sim | - |  |
| 4 | `KN2` | `numeric(18,0)` | sim | - |  |
| 5 | `KS` | `varchar(40)` | sim | - |  |
| 6 | `DtaLog` | `datetime` | **nao** | - |  |
| 7 | `Host` | `varchar(30)` | sim | - |  |
| 8 | `CodUsuario` | `varchar(20)` | **nao** | - | login do usuario (varchar) |
| 9 | `Resumo` | `varchar(30)` | sim | - |  |
| 10 | `Detalhe` | `varchar(250)` | sim | - |  |

---

### GE_LOG_CARTCRED

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** _(inferido)_ Pelo nome, e uma trilha/log de alteracoes relacionada a carteira de clientes, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_CONFIG

`classe: isolada` · `10 colunas` · `108.492 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_CONTATO

`classe: isolada` · `10 colunas` · `19.785 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_EXT

`classe: isolada` · `10 colunas` · `622.075 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_HISTORICO

`classe: isolada` · `10 colunas` · `1.015.045 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_PESSOA

`classe: isolada` · `10 colunas` · `943.712 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_PROCESSO

`classe: isolada` · `10 colunas` · `12.678.640 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_LOG_TRANS

`classe: isolada` · `10 colunas` · `782.089 linhas (snapshot 03/06/2026)` · `PK: SEQLOGTB`

**Funcao:** Trilha de auditoria de DADOS, escrita pela aplicação (não por trigger). ~32 milhões de linhas e ~10 GB. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQLOGTB` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TB` | `varchar(25)` | **nao** | - |  |
| 3 | `KN1` | `numeric(18,0)` | **nao** | - |  |
| 4 | `KN2` | `numeric(18,0)` | **nao** | - |  |
| 5 | `KS` | `varchar(40)` | **nao** | - |  |
| 6 | `DTALOG` | `datetime` | **nao** | - |  |
| 7 | `USR` | `varchar(20)` | sim | - |  |
| 8 | `CODAPL` | `varchar(30)` | sim | - |  |
| 9 | `OBS` | `varchar(1000)` | sim | - | texto livre |
| 10 | `NIVEL` | `char(1)` | sim | - |  |

---

### GE_Membro

`classe: nucleo` · `3 colunas` · `1.584 linhas (snapshot 03/06/2026)` · `PK: Grupo, Usuario`

**Funcao:** Vínculo usuário↔grupo. Plano (sem grupos aninhados). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Grupo` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Usuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario` |
| 3 | `INDATIVO` | `numeric(1,0)` | sim | - |  |

---

### GE_Membro_BKP20250520

`classe: lixo/backup` · `3 colunas` · `1.915 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (3 colunas) para manter o documento legivel._

---

### GE_MobileConfig

`classe: vazia` · `14 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqMobileConfig`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de parametrizacao, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqMobileConfig` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `Nome` | `varchar(100)` | **nao** | - |  |
| 3 | `Acao` | `numeric(6,0)` | sim | - | acao (`IV_Acao.Acao`) |
| 4 | `Resultado` | `numeric(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 5 | `SeqPropriedade` | `numeric(4,0)` | sim | - | FK -> `IV_Propriedade.SeqPropriedade` |
| 6 | `SeqDocTp` | `numeric(4,0)` | sim | - |  |
| 7 | `CompresFoto` | `numeric(2,0)` | sim | - |  |
| 8 | `IntervaloSync` | `numeric(8,0)` | sim | - |  |
| 9 | `ExigeFoto` | `numeric(1,0)` | sim | - |  |
| 10 | `ExigeContato` | `numeric(1,0)` | sim | - |  |
| 11 | `ExigeContatoFone` | `numeric(1,0)` | sim | - |  |
| 12 | `ExigeContatoEmail` | `numeric(1,0)` | sim | - |  |
| 13 | `ExigeDtaNasc` | `numeric(1,0)` | sim | - |  |
| 14 | `GRLimiteRario` | `numeric(8,0)` | sim | - |  |

**Referenciada por:** `GEP_UsrSat.SeqMobileConfig`, `GE_MOBILEPROP.SEQMOBILECONFIG`

---

### GE_MOBILEPROP

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQMOBILECONFIG, SEQPROPRIEDADE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de aplicativo mobile, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQMOBILECONFIG` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_MobileConfig.SeqMobileConfig` |
| 2 | `SEQPROPRIEDADE` | `numeric(4,0)` | **nao** | - | **PK**; FK -> `IV_Propriedade.SeqPropriedade` |

---

### GE_Mod

`classe: isolada` · `9 colunas` · `32 linhas (snapshot 03/06/2026)` · `PK: Modulo`

**Funcao:** Três módulos com versão VAZIA em GE_Modulo (isto é, registrados mas sem release aplicada no ambiente da Tracbel), todos com TipoAcesso='P': CRM_M100 'Vórtico CRM BPM 2025', CRM_M110 'Vórtico Painel de Controle 2025', CRM_M020 'Vórtico Web CRM', além de CRM_M021 'Vortico OUT CRM' e CRM_M007 'Vórtico Config RD Station'. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Modulo` | `varchar(10)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `Tpac` | `char(1)` | sim | - |  |
| 4 | `Vers` | `varchar(10)` | sim | - |  |
| 5 | `Datu` | `datetime` | sim | - |  |
| 6 | `CHKSUM` | `varchar(50)` | sim | - |  |
| 7 | `SISTEMAGE` | `varchar(20)` | sim | - |  |
| 8 | `MODULOGE` | `varchar(20)` | sim | - |  |
| 9 | `TIPO` | `char(1)` | sim | - |  |

---

### GE_Modulo

`classe: catalogo` · `8 colunas` · `48 linhas (snapshot 03/06/2026)` · `PK: Sistema, Modulo`

**Funcao:** Catálogo de licenciamento e inventário de telas do produto: 11 sistemas, 48 módulos (16 com código de licença CRM_M0xx/QVW_M00x/GLB_M000) e 184 aplicações/telas registradas. É a fonte mais confiável do 'que o Vórtice vende'. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Sistema` | `varchar(20)` | **nao** | - | **PK**; FK -> `GE_Sistema.Sistema` |
| 2 | `Modulo` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 4 | `SiglaModulo` | `varchar(10)` | sim | - |  |
| 5 | `TipoAcesso` | `char(1)` | sim | - |  |
| 6 | `Versao` | `varchar(20)` | sim | - |  |
| 7 | `CHKSUM` | `varchar(50)` | sim | - |  |
| 8 | `TIPO` | `char(1)` | sim | - |  |

**Referenciada por:** `GE_Aplicacao.Modulo`, `GE_Aplicacao.Sistema`, `GE_ModuloPerm.Modulo`, `GE_ModuloPerm.Sistema`

---

### GE_ModuloPerm

`classe: nucleo` · `6 colunas` · `25.435 linhas (snapshot 03/06/2026)` · `PK: Sistema, Modulo, NroEmpresa, SeqUsuario`

**Funcao:** Acesso a MÓDULO por usuário e empresa. 25.202 linhas, 409 usuários, 42 combinações Sistema/Módulo, 18 empresas. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Sistema` | `varchar(20)` | **nao** | - | **PK**; FK -> `GE_Modulo.Sistema` |
| 2 | `Modulo` | `varchar(20)` | **nao** | - | **PK**; FK -> `GE_Modulo.Modulo` |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `GE_Empresa.NroEmpresa`; multiempresa - filial/empresa |
| 4 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 5 | `Permissao` | `char(1)` | sim | - |  |
| 6 | `CHKSUM` | `varchar(50)` | sim | - |  |

---

### GE_ObjDinamico

`classe: isolada` · `29 colunas` · `193 linhas (snapshot 03/06/2026)` · `PK: SeqObjDyn`

**Funcao:** Único mecanismo de extensão/low-code do produto: 196 SQLs armazenados no banco que atuam como pré-requisito de resultado, desvio de workflow, seleção de destinatário, validação de cadastro, painel de informação na tela da pessoa e roteamento de e-mail. Pode até apontar para um banco externo. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqObjDyn` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tipo` | `varchar(20)` | sim | - |  |
| 3 | `TipoRetorno` | `varchar(20)` | sim | - |  |
| 4 | `Descricao` | `varchar(50)` | sim | - | descricao do registro |
| 5 | `MensInicial` | `varchar(100)` | sim | - |  |
| 6 | `MensFalso` | `varchar(100)` | sim | - |  |
| 7 | `MensVerdadeiro` | `varchar(100)` | sim | - |  |
| 8 | `Uso` | `varchar(200)` | sim | - |  |
| 9 | `Conexao` | `varchar(20)` | sim | - |  |
| 10 | `BancoDados` | `varchar(20)` | sim | - |  |
| 11 | `Usuario` | `varchar(20)` | sim | - |  |
| 12 | `Senha` | `varchar(40)` | sim | - |  |
| 13 | `DBVersion` | `numeric(18,0)` | sim | - |  |
| 14 | `Comando` | `text(2147483647)` | sim | - |  |
| 15 | `UsoJuncaoPessoa` | `numeric(1,0)` | sim | - |  |
| 16 | `UsoPessoa` | `numeric(1,0)` | sim | - |  |
| 17 | `UsoProcesso` | `numeric(1,0)` | sim | - |  |
| 18 | `UsoAgenda` | `numeric(1,0)` | sim | - |  |
| 19 | `UsoHistorico` | `numeric(1,0)` | sim | - |  |
| 20 | `UsoAcao` | `numeric(1,0)` | sim | - |  |
| 21 | `UsoResultado` | `numeric(1,0)` | sim | - |  |
| 22 | `UsoReqResultado` | `numeric(1,0)` | sim | - |  |
| 23 | `UsoResultadoCmpl` | `numeric(1,0)` | sim | - |  |
| 24 | `UsoWorkFlow` | `numeric(1,0)` | sim | - |  |
| 25 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 26 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 27 | `USOMSGCENTER` | `numeric(1,0)` | sim | - |  |
| 28 | `ChkSum` | `numeric(18,0)` | sim | - |  |
| 29 | `USOMSGCTRSMS` | `numeric(1,0)` | sim | - |  |

---

### GE_ObjDinAplic

`classe: isolada` · `4 colunas` · `122 linhas (snapshot 03/06/2026)` · `PK: SeqObjDyn, Aplicativo, Chave`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqObjDyn` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Aplicativo` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `Chave` | `numeric(18,0)` | **nao** | - | **PK** |
| 4 | `Vf` | `numeric(1,0)` | sim | - |  |

---

### GE_ParametroGlobal

`classe: isolada` · `8 colunas` · `236 linhas (snapshot 03/06/2026)` · `PK: Sistema, Modulo, NroEmpresa, Parametro`

**Funcao:** Parâmetros por usuário (16.827 linhas), globais por sistema/módulo/empresa (236) e listas (333). Contêm preferência de UI, mas TAMBÉM autorização (escopo de empresa em relatório) e segredo (token de API em texto claro). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Sistema` | `varchar(12)` | **nao** | - | **PK** |
| 2 | `Modulo` | `varchar(12)` | **nao** | - | **PK** |
| 3 | `NroEmpresa` | `decimal(3,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 4 | `Parametro` | `varchar(30)` | **nao** | - | **PK** |
| 5 | `Valor` | `varchar(1000)` | sim | - |  |
| 6 | `Criptografado` | `varchar(1)` | sim | - |  |
| 7 | `Dtaalteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `Usualteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_ParamLista

`classe: catalogo` · `25 colunas` · `333 linhas (snapshot 03/06/2026)` · `PK: SeqParamLista`

**Funcao:** Catálogo genérico único ('uma tabela para todos os dropdowns') com slots ListaStr/ListaNro/Str1..Str5/Nro1..Nro6/Ind1..Ind4/DTA1..DTA2/STRL1 discriminados pela coluna Parametro. Contém CRMPES_TIPOFONE(3), CRMPES_ATIVIDADEPF(11), CRMPES_ATIVIDADEPJ(11), CRMPES_FAIXAFATURA(12), CRMFI_SITUACAO(12), CRM_DESTINOTBL(12), RDSTATION_MAP_FIELD(148), RDSTATION_MAP_EVENT(11), RDSTATION_MAP_CAB(10). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqParamLista` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Parametro` | `varchar(20)` | **nao** | - |  |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | multiempresa - filial/empresa |
| 4 | `ListaStr` | `varchar(250)` | sim | - |  |
| 5 | `ListaNro` | `numeric(18,0)` | sim | - |  |
| 6 | `Str1` | `varchar(250)` | sim | - |  |
| 7 | `Nro1` | `numeric(18,0)` | sim | - |  |
| 8 | `Nro2` | `numeric(18,0)` | sim | - |  |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 11 | `STR2` | `varchar(250)` | sim | - |  |
| 12 | `STR3` | `varchar(250)` | sim | - |  |
| 13 | `STR4` | `varchar(250)` | sim | - |  |
| 14 | `NRO3` | `numeric(18,0)` | sim | - |  |
| 15 | `NRO4` | `numeric(18,0)` | sim | - |  |
| 16 | `Ind1` | `numeric(1,0)` | sim | - |  |
| 17 | `Ind2` | `numeric(1,0)` | sim | - |  |
| 18 | `DTA1` | `datetime` | sim | - |  |
| 19 | `DTA2` | `datetime` | sim | - |  |
| 20 | `STR5` | `varchar(250)` | sim | - |  |
| 21 | `IND3` | `numeric(1,0)` | sim | - |  |
| 22 | `NRO5` | `numeric(18,0)` | sim | - |  |
| 23 | `NRO6` | `numeric(18,0)` | sim | - |  |
| 24 | `IND4` | `numeric(1,0)` | sim | - |  |
| 25 | `STRL1` | `varchar(4000)` | sim | - |  |

**Referenciada por:** `GEP_EMailSend.SEQCONTAEMAIL`, `IV_ResMsgPapel.SEQCONTAENVIO`, `IV_WHATSAPP.CONTAWAPSEQPAR`

---

### GE_Permissao

`classe: nucleo` · `11 colunas` · `307 linhas (snapshot 03/06/2026)` · `PK: SeqAplicacao, SeqUsuario, NroEmpresa`

**Funcao:** Camada de permissão por TELA com verbos CRUD — desenhada por completo e praticamente não usada (302 linhas, 55 usuários, 49 de 184 aplicações). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqAplicacao` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; FK -> `GE_Empresa.NroEmpresa`; multiempresa - filial/empresa |
| 4 | `Executar` | `char(1)` | sim | - |  |
| 5 | `Incluir` | `char(1)` | sim | - |  |
| 6 | `Alterar` | `char(1)` | sim | - |  |
| 7 | `Excluir` | `char(1)` | sim | - |  |
| 8 | `RegistrarLog` | `char(1)` | sim | - |  |
| 9 | `Verimpressao` | `char(1)` | sim | - |  |
| 10 | `Exportar` | `char(1)` | sim | - |  |
| 11 | `Imprimir` | `char(1)` | sim | - |  |

---

### GE_PesDelVinc

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPesDelVinc`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPesDelVinc` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_PessoaDel.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Tabela` | `varchar(30)` | **nao** | - |  |
| 4 | `KN1` | `numeric(18,0)` | sim | - |  |
| 5 | `KN2` | `numeric(18,0)` | sim | - |  |
| 6 | `KS1` | `varchar(40)` | sim | - |  |
| 7 | `KS2` | `varchar(40)` | sim | - |  |

---

### GE_Pessoa

`classe: nucleo` · `76 colunas` · `118.463 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** Cadastro mestre único de pessoas: cliente, prospect, suspect, falecido, fornecedor, funcionário — tudo na mesma tabela, diferenciado por Status char(1) e Grupo varchar(30). 76 colunas, 118.463 linhas, viva (última inclusão/alteração 30/08/2026). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `SeqCidade` | `decimal(6,0)` | sim | - | FK -> `GE_Bairro.SeqCidade`; FK -> `GE_Cidade.SeqCidade` |
| 3 | `SeqBairro` | `decimal(5,0)` | sim | - | FK -> `GE_Bairro.SeqBairro` |
| 4 | `Versao` | `decimal(2,0)` | sim | - |  |
| 5 | `Status` | `char(1)` | **nao** | - | status - validar dominio real por tabela |
| 6 | `DtaAtivacao` | `datetime` | sim | - |  |
| 7 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 8 | `Fantasia` | `varchar(50)` | sim | - |  |
| 9 | `PalavraChave` | `varchar(50)` | sim | - |  |
| 10 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 11 | `Sexo` | `char(1)` | sim | - |  |
| 12 | `Cidade` | `varchar(50)` | sim | - |  |
| 13 | `Uf` | `varchar(2)` | sim | - |  |
| 14 | `Pais` | `varchar(25)` | sim | - |  |
| 15 | `Bairro` | `varchar(50)` | sim | - |  |
| 16 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 17 | `Logradouro` | `varchar(80)` | sim | - |  |
| 18 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 19 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 20 | `Cep` | `varchar(12)` | sim | - |  |
| 21 | `CxPostal` | `varchar(7)` | sim | - |  |
| 22 | `SeqPessoaEndCobr` | `decimal(3,0)` | sim | - |  |
| 23 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 24 | `FoneNro1` | `decimal(12,0)` | sim | - |  |
| 25 | `FoneCmpl1` | `varchar(20)` | sim | - |  |
| 26 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 27 | `FoneNro2` | `decimal(12,0)` | sim | - |  |
| 28 | `FoneCmpl2` | `varchar(20)` | sim | - |  |
| 29 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 30 | `FoneNro3` | `decimal(12,0)` | sim | - |  |
| 31 | `FoneCmpl3` | `varchar(20)` | sim | - |  |
| 32 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 33 | `FaxNro` | `decimal(12,0)` | sim | - |  |
| 34 | `NroCGCCPF` | `decimal(13,0)` | sim | - |  |
| 35 | `DigCGCCPF` | `decimal(2,0)` | sim | - |  |
| 36 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 37 | `UFEmissor` | `varchar(2)` | sim | - |  |
| 38 | `OrgaoEmissor` | `varchar(10)` | sim | - |  |
| 39 | `InscMunic` | `varchar(15)` | sim | - |  |
| 40 | `InscProdutor` | `varchar(20)` | sim | - |  |
| 41 | `CNAE` | `varchar(15)` | sim | - |  |
| 42 | `DtaNascFund` | `datetime` | sim | - |  |
| 43 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 44 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 45 | `Email` | `varchar(70)` | sim | - |  |
| 46 | `HomePage` | `varchar(80)` | sim | - |  |
| 47 | `EstadoCivil` | `varchar(20)` | sim | - |  |
| 48 | `Atividade` | `varchar(30)` | sim | - |  |
| 49 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 50 | `GrauInstrucao` | `varchar(30)` | sim | - |  |
| 51 | `Grupo` | `varchar(30)` | sim | - |  |
| 52 | `Porte` | `varchar(30)` | sim | - |  |
| 53 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 54 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 55 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 56 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 57 | `DtaInativacao` | `datetime` | sim | - | data de inativacao |
| 58 | `UsuInativacao` | `varchar(20)` | sim | - |  |
| 59 | `ObsInativacao` | `varchar(50)` | sim | - |  |
| 60 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 61 | `Telefonema` | `numeric(1,0)` | sim | - |  |
| 62 | `Correspondencia` | `numeric(1,0)` | sim | - |  |
| 63 | `RecebeEmail` | `numeric(1,0)` | sim | - |  |
| 64 | `NaoPossuiEmail` | `numeric(1,0)` | sim | - |  |
| 65 | `ProblemaCredito` | `numeric(1,0)` | sim | - |  |
| 66 | `IndContribICMS` | `char(1)` | sim | - |  |
| 67 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 68 | `Latitude` | `decimal(14,11)` | sim | - | geolocalizacao |
| 69 | `Longitude` | `decimal(14,11)` | sim | - | geolocalizacao |
| 70 | `SeqRegiao` | `decimal(6,0)` | sim | - | FK -> `GE_Regiao.SeqRegiao` |
| 71 | `SeqRota` | `decimal(6,0)` | sim | - | FK -> `GE_Rota.SeqRota` |
| 72 | `RecebeSMS` | `numeric(1,0)` | sim | - |  |
| 73 | `SEQPESSOAPRC` | `numeric(10,0)` | **nao** | - |  |
| 74 | `Skype` | `varchar(70)` | sim | - |  |
| 75 | `SMSCODIGO` | `varchar(8)` | sim | - |  |
| 76 | `SMSCODIGODTA` | `datetime` | sim | - |  |

**Referenciada por:** `DMN_DocPes.SeqPessoa`, `EXT_EMAIL.SEQPESSOA`, `EXT_NFS.SeqPessoa`, `EXT_OS.SeqPessoa`, `EXT_Pedido.SeqPessoa`, `EXT_Pessoa.SeqPessoa`, `EXT_Titulo.SeqPessoa`, `EXT_Veic.SeqPessoa`, `EXT_VeicAgd.SeqPessoa`, `EXT_VeicProp.SeqPessoa`, `GE_FCadF.SeqPessoa`, `GE_FCadInstCred.SeqPessoa`, `GE_FcadRBanc.SeqPessoa`, `GE_FcadRCom.SeqPessoa`, `GE_PESSOAREDESOCIAL.SEQPESSOA`, `GE_PessoaEmail.SeqPessoa`, `GE_PessoaEndOutroBc.SeqPessoa`, `GE_PessoaJur.SeqPessoa`, `GE_PessoaNota.SeqPessoa`, `GE_PessoaPasw.SeqPessoa`, `GE_PessoaSimilar.SeqPessoa1`, `IVF_Financeira.SeqPessoa`, `IVM_MatPessoa.SeqPessoa`, `IVS_Pes.SeqPessoa`, `IV_AgdRec.SeqPessoa`, `IV_Agenda.SeqPessoa`, `IV_CampPesMsg.SEQPESSOA`, `IV_CampVoucher.SEQPESSOA`, `IV_ClientePropr.SeqPessoa`, `IV_Historico.SeqPessoa`, `IV_OS.SEQPESSOA`, `IV_PUSH.SEQPESSOA`, `IV_Pessoa.SeqPessoa`, `IV_ProjPessoa.SeqPessoa`, `IV_SelecaoPessoa.SeqPessoa`, `IV_WHATSAPP.SEQPESSOA`

---

### GE_PessoaAlerta

`classe: isolada` · `8 colunas` · `237 linhas (snapshot 03/06/2026)` · `PK: SeqPessoaAlerta`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoaAlerta` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Resumo` | `varchar(20)` | **nao** | - |  |
| 4 | `Advertencia` | `varchar(250)` | sim | - |  |
| 5 | `DtaVigor` | `datetime` | sim | - |  |
| 6 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 7 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 8 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

---

### GE_PessoaAlt

`classe: vazia` · `68 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoaAlt`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoaAlt` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `SeqCidade` | `numeric(6,0)` | sim | - |  |
| 4 | `SeqBairro` | `numeric(5,0)` | sim | - |  |
| 5 | `Versao` | `numeric(2,0)` | sim | - |  |
| 6 | `Status` | `char(1)` | **nao** | - | status - validar dominio real por tabela |
| 7 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 8 | `Fantasia` | `varchar(50)` | sim | - |  |
| 9 | `PalavraChave` | `varchar(50)` | sim | - |  |
| 10 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 11 | `SEXO` | `char(1)` | sim | - |  |
| 12 | `Cidade` | `varchar(50)` | sim | - |  |
| 13 | `UF` | `varchar(2)` | sim | - |  |
| 14 | `PAIS` | `varchar(25)` | sim | - |  |
| 15 | `Bairro` | `varchar(50)` | sim | - |  |
| 16 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 17 | `Logradouro` | `varchar(80)` | sim | - |  |
| 18 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 19 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 20 | `CEP` | `varchar(12)` | sim | - |  |
| 21 | `CxPostal` | `varchar(7)` | sim | - |  |
| 22 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 23 | `Latitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 24 | `Longitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 25 | `SeqPessoaEndCobr` | `numeric(3,0)` | sim | - |  |
| 26 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 27 | `FoneNro1` | `numeric(12,0)` | sim | - |  |
| 28 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 29 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 30 | `FoneNro2` | `numeric(12,0)` | sim | - |  |
| 31 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 32 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 33 | `FoneNro3` | `numeric(12,0)` | sim | - |  |
| 34 | `FoneCmpl3` | `varchar(12)` | sim | - |  |
| 35 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 36 | `FaxNro` | `numeric(12,0)` | sim | - |  |
| 37 | `NroCGCCPF` | `numeric(13,0)` | sim | - |  |
| 38 | `DigCGCCPF` | `numeric(2,0)` | sim | - |  |
| 39 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 40 | `UFEmissor` | `varchar(2)` | sim | - |  |
| 41 | `OrgaoEmissor` | `varchar(10)` | sim | - |  |
| 42 | `InscMunic` | `varchar(15)` | sim | - |  |
| 43 | `InscProdutor` | `varchar(20)` | sim | - |  |
| 44 | `CNAE` | `varchar(15)` | sim | - |  |
| 45 | `DtaNascFund` | `datetime` | sim | - |  |
| 46 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 47 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 48 | `Email` | `varchar(70)` | sim | - |  |
| 49 | `HomePage` | `varchar(80)` | sim | - |  |
| 50 | `EstadoCivil` | `varchar(20)` | sim | - |  |
| 51 | `Atividade` | `varchar(30)` | sim | - |  |
| 52 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 53 | `GrauInstrucao` | `varchar(30)` | sim | - |  |
| 54 | `Grupo` | `varchar(30)` | sim | - |  |
| 55 | `Porte` | `varchar(30)` | sim | - |  |
| 56 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 57 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 58 | `DtaInativacao` | `datetime` | sim | - | data de inativacao |
| 59 | `UsuInativacao` | `varchar(20)` | sim | - |  |
| 60 | `ObsInativacao` | `varchar(50)` | sim | - |  |
| 61 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 62 | `Telefonema` | `numeric(1,0)` | sim | - |  |
| 63 | `Correspondencia` | `numeric(1,0)` | sim | - |  |
| 64 | `RecebeEmail` | `numeric(1,0)` | sim | - |  |
| 65 | `RecebeSMS` | `numeric(1,0)` | sim | - |  |
| 66 | `NaoPossuiEmail` | `numeric(1,0)` | sim | - |  |
| 67 | `ProblemaCredito` | `numeric(1,0)` | sim | - |  |
| 68 | `IndContribICMS` | `char(1)` | sim | - |  |

---

### ge_pessoaativa

`classe: isolada` · `1 colunas` · `1.175 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `seqpessoa` | `numeric(18,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |

---

### GE_PESSOAATIVAUSR

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQUSUARIO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `CODUSUARIO` | `varchar(20)` | sim | - | login do usuario (varchar) |
| 3 | `SEQPESSOA` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `PESSOALINK` | `varchar(250)` | sim | - |  |
| 5 | `DTAACESSO` | `datetime` | sim | - |  |

---

### GE_PessoaClasse

`classe: isolada` · `7 colunas` · `1.878 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, Classe`

**Funcao:** - Não apurei o conteúdo dos catálogos GE_PessoaClasse além da distribuição (FILIAL 873, FUNCIONÁRIOS 612, PARCEIROS 236, FALECIDO 71, REDE CONCESSIONÁRIOS 69, SAM 46) — as colunas ORIGEM e IDENTORIGEM estão vazias na amostra, então não sei que sistema aplica essas classes. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Classe` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `Dtaalteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 4 | `Usualteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 5 | `OBS` | `varchar(250)` | sim | - | texto livre |
| 6 | `ORIGEM` | `varchar(30)` | sim | - | sistema de origem do dado |
| 7 | `IDENTORIGEM` | `varchar(50)` | sim | - |  |

---

### GE_PessoaDel

`classe: vazia` · `76 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqPessoaPRC` | `numeric(10,0)` | sim | - |  |
| 3 | `SeqCidade` | `numeric(6,0)` | sim | - |  |
| 4 | `SeqBairro` | `numeric(10,0)` | sim | - |  |
| 5 | `Versao` | `numeric(2,0)` | sim | - |  |
| 6 | `Status` | `char(1)` | **nao** | - | status - validar dominio real por tabela |
| 7 | `DtaAtivacao` | `datetime` | sim | - |  |
| 8 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 9 | `Fantasia` | `varchar(50)` | sim | - |  |
| 10 | `PalavraChave` | `varchar(50)` | sim | - |  |
| 11 | `FisicaJuridica` | `char(1)` | sim | - |  |
| 12 | `SEXO` | `char(1)` | sim | - |  |
| 13 | `Cidade` | `varchar(50)` | sim | - |  |
| 14 | `UF` | `varchar(2)` | sim | - |  |
| 15 | `PAIS` | `varchar(25)` | sim | - |  |
| 16 | `Bairro` | `varchar(50)` | sim | - |  |
| 17 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 18 | `Logradouro` | `varchar(80)` | sim | - |  |
| 19 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 20 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 21 | `CEP` | `varchar(12)` | sim | - |  |
| 22 | `CxPostal` | `varchar(7)` | sim | - |  |
| 23 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 24 | `Latitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 25 | `Longitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 26 | `SeqPessoaEndCobr` | `numeric(3,0)` | sim | - |  |
| 27 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 28 | `FoneNro1` | `numeric(12,0)` | sim | - |  |
| 29 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 30 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 31 | `FoneNro2` | `numeric(12,0)` | sim | - |  |
| 32 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 33 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 34 | `FoneNro3` | `numeric(12,0)` | sim | - |  |
| 35 | `FoneCmpl3` | `varchar(12)` | sim | - |  |
| 36 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 37 | `FaxNro` | `numeric(12,0)` | sim | - |  |
| 38 | `NroCGCCPF` | `numeric(13,0)` | sim | - |  |
| 39 | `DigCGCCPF` | `numeric(2,0)` | sim | - |  |
| 40 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 41 | `UFEmissor` | `varchar(2)` | sim | - |  |
| 42 | `OrgaoEmissor` | `varchar(10)` | sim | - |  |
| 43 | `InscMunic` | `varchar(15)` | sim | - |  |
| 44 | `InscProdutor` | `varchar(20)` | sim | - |  |
| 45 | `CNAE` | `varchar(15)` | sim | - |  |
| 46 | `DtaNascFund` | `datetime` | sim | - |  |
| 47 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 48 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 49 | `Email` | `varchar(70)` | sim | - |  |
| 50 | `Skype` | `varchar(70)` | sim | - |  |
| 51 | `HomePage` | `varchar(80)` | sim | - |  |
| 52 | `EstadoCivil` | `varchar(20)` | sim | - |  |
| 53 | `Atividade` | `varchar(30)` | sim | - |  |
| 54 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 55 | `GrauInstrucao` | `varchar(30)` | sim | - |  |
| 56 | `Grupo` | `varchar(30)` | sim | - |  |
| 57 | `Porte` | `varchar(30)` | sim | - |  |
| 58 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 59 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 60 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 61 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 62 | `DtaInativacao` | `datetime` | sim | - | data de inativacao |
| 63 | `UsuInativacao` | `varchar(20)` | sim | - |  |
| 64 | `ObsInativacao` | `varchar(50)` | sim | - |  |
| 65 | `CODVENDEDOR` | `varchar(20)` | sim | - | codigo/login do vendedor (varchar) |
| 66 | `Telefonema` | `numeric(1,0)` | sim | - |  |
| 67 | `Correspondencia` | `numeric(1,0)` | sim | - |  |
| 68 | `RecebeEmail` | `numeric(1,0)` | sim | - |  |
| 69 | `RecebeSMS` | `numeric(1,0)` | sim | - |  |
| 70 | `NaoPossuiEmail` | `numeric(1,0)` | sim | - |  |
| 71 | `ProblemaCredito` | `numeric(1,0)` | sim | - |  |
| 72 | `IndContribICMS` | `char(1)` | sim | - |  |
| 73 | `SeqRegiao` | `numeric(6,0)` | sim | - |  |
| 74 | `SeqRota` | `numeric(6,0)` | sim | - |  |
| 75 | `DtaDel` | `datetime` | **nao** | - |  |
| 76 | `UsrDel` | `varchar(20)` | **nao** | - |  |

**Referenciada por:** `GE_PesDelVinc.SeqPessoa`

---

### GE_PessoaDestino

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, Destino`

**Funcao:** GE_PessoaDestino e GE_PESSOAREDESOCIAL têm 0. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Destino` | `varchar(20)` | **nao** | - | **PK**; FK -> `GEP_ParEnvia.Destino` |
| 3 | `IndEnvia` | `numeric(1,0)` | sim | - |  |
| 4 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

---

### GE_PessoaEmail

`classe: vazia` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqContato`

**Funcao:** TRÊS modelos de e-mail concorrentes. GE_Email (39.363 linhas) é o real e tem o melhor modelo de finalidade por canal. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqContato` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `EMail` | `varchar(50)` | sim | - |  |
| 4 | `Preferencial` | `numeric(1,0)` | sim | - |  |
| 5 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 7 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 8 | `Obs` | `varchar(50)` | sim | - | texto livre |

---

### GE_PessoaEnd

`classe: nucleo` · `26 colunas` · `6.480 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqPessoaEnd`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqPessoaEnd` | `decimal(3,0)` | **nao** | - | **PK** |
| 3 | `SeqCidade` | `decimal(6,0)` | sim | - | FK -> `GE_Bairro.SeqCidade`; FK -> `GE_Cidade.SeqCidade` |
| 4 | `TipoEndereco` | `char(1)` | **nao** | - |  |
| 5 | `Cidade` | `varchar(50)` | sim | - |  |
| 6 | `Uf` | `varchar(2)` | sim | - |  |
| 7 | `SeqBairro` | `decimal(5,0)` | sim | - | FK -> `GE_Bairro.SeqBairro` |
| 8 | `Bairro` | `varchar(50)` | sim | - |  |
| 9 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 10 | `Logradouro` | `varchar(80)` | sim | - |  |
| 11 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 12 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 13 | `CxPostal` | `varchar(7)` | sim | - |  |
| 14 | `SeqPessoaEndCobr` | `decimal(3,0)` | sim | - |  |
| 15 | `Cep` | `varchar(12)` | sim | - |  |
| 16 | `Pais` | `varchar(25)` | sim | - |  |
| 17 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 18 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 19 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 20 | `Latitude` | `decimal(14,11)` | sim | - | geolocalizacao |
| 21 | `Longitude` | `decimal(14,11)` | sim | - | geolocalizacao |
| 22 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 23 | `SeqRegiao` | `decimal(6,0)` | sim | - |  |
| 24 | `SeqRota` | `decimal(6,0)` | sim | - |  |
| 25 | `CHAVEADICIONAL` | `varchar(30)` | sim | - |  |
| 26 | `INSCPRODUTOR` | `varchar(20)` | sim | - |  |

---

### GE_PessoaEndAlt

`classe: vazia` · `21 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoaEndAlt`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoaEndAlt` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `SeqPessoaEnd` | `numeric(3,0)` | sim | - |  |
| 4 | `SeqCidade` | `numeric(6,0)` | sim | - |  |
| 5 | `SeqBairro` | `numeric(5,0)` | sim | - |  |
| 6 | `Cidade` | `varchar(50)` | sim | - |  |
| 7 | `Bairro` | `varchar(50)` | sim | - |  |
| 8 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 9 | `Logradouro` | `varchar(80)` | sim | - |  |
| 10 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 11 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 12 | `CxPostal` | `varchar(7)` | sim | - |  |
| 13 | `RefEndereco` | `varchar(150)` | sim | - |  |
| 14 | `Latitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 15 | `Longitude` | `numeric(14,11)` | sim | - | geolocalizacao |
| 16 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 17 | `SeqPessoaEndCobr` | `numeric(3,0)` | sim | - |  |
| 18 | `CEP` | `varchar(12)` | sim | - |  |
| 19 | `PAIS` | `varchar(25)` | sim | - |  |
| 20 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 21 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |

---

### GE_PessoaEndOutroBc

`classe: nucleo` · `3 colunas` · `30.943 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqPessoaEnd`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqPessoaEnd` | `decimal(3,0)` | **nao** | - | **PK** |
| 3 | `SeqEndOutroBc` | `numeric(18,0)` | sim | - |  |

---

### GE_PessoaEnd_BKPJUN

`classe: lixo/backup` · `26 colunas` · `3.739 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (26 colunas) para manter o documento legivel._

---

### GE_PessoaEnd_ITA

`classe: lixo/backup` · `26 colunas` · `2.779 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (26 colunas) para manter o documento legivel._

---

### GE_PessoaFis

`classe: isolada` · `32 colunas` · `5.136 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `NomePai` | `varchar(50)` | sim | - |  |
| 3 | `NomeMae` | `varchar(50)` | sim | - |  |
| 4 | `NatCidade` | `varchar(50)` | sim | - |  |
| 5 | `NatUF` | `varchar(2)` | sim | - |  |
| 6 | `NatSeqCidade` | `numeric(6,0)` | sim | - |  |
| 7 | `Nacionalidade` | `varchar(25)` | sim | - |  |
| 8 | `DtaEmissaoRG` | `datetime` | sim | - |  |
| 9 | `NatOcupacaoSeqPar` | `numeric(18,0)` | sim | - |  |
| 10 | `AtividadeSeqPar` | `numeric(18,0)` | sim | - |  |
| 11 | `NacionalSeqPar` | `numeric(18,0)` | sim | - |  |
| 12 | `EstadoCivilSeqPar` | `numeric(18,0)` | sim | - |  |
| 13 | `GrauInstrSeqPar` | `numeric(18,0)` | sim | - |  |
| 14 | `Sexo` | `char(1)` | sim | - |  |
| 15 | `DtaNascimento` | `datetime` | sim | - |  |
| 16 | `RGDtaEmissao` | `datetime` | sim | - |  |
| 17 | `QtdDependente` | `decimal(2,0)` | sim | - |  |
| 18 | `CNHNro` | `numeric(18,0)` | sim | - |  |
| 19 | `CNHDtaValidade` | `datetime` | sim | - |  |
| 20 | `RGOrgaoEmissor` | `varchar(12)` | sim | - |  |
| 21 | `RGUFEmissao` | `varchar(2)` | sim | - |  |
| 22 | `CartTrabNro` | `numeric(18,0)` | sim | - |  |
| 23 | `CartTrabSerie` | `varchar(10)` | sim | - |  |
| 24 | `CartTrabUF` | `char(2)` | sim | - |  |
| 25 | `IdentProfSeqPar` | `numeric(18,0)` | sim | - |  |
| 26 | `IdentProfNro` | `varchar(20)` | sim | - |  |
| 27 | `CNHDtaEmis` | `datetime` | sim | - |  |
| 28 | `CartTrabDtaEmis` | `datetime` | sim | - |  |
| 29 | `IdentProfDtaEmis` | `datetime` | sim | - |  |
| 30 | `CRNM` | `varchar(20)` | sim | - |  |
| 31 | `CRNMDTAEMISSAO` | `datetime` | sim | - |  |
| 32 | `CRNMDTAVALIDADE` | `datetime` | sim | - |  |

---

### GE_PessoaFone

`classe: isolada` · `19 colunas` · `112.659 linhas (snapshot 03/06/2026)` · `PK: SeqPesFone`

**Funcao:** Telefones (113.567 linhas). Modelo tabular correto, mas convive com as 3 colunas FoneNro1/2/3 desnormalizadas dentro de GE_Pessoa. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPesFone` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TipoFoneSeqPar` | `numeric(18,0)` | **nao** | - |  |
| 3 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `DDD` | `varchar(5)` | sim | - |  |
| 5 | `Numero` | `numeric(12,0)` | **nao** | - |  |
| 6 | `Complemento` | `varchar(20)` | sim | - |  |
| 7 | `Obs` | `varchar(30)` | sim | - | texto livre |
| 8 | `IndFonePref` | `numeric(1,0)` | sim | - |  |
| 9 | `NroFonePessoa` | `numeric(1,0)` | sim | - |  |
| 10 | `DTAULTSUCESSO` | `datetime` | sim | - |  |
| 11 | `DTAULTINSUCESSO` | `datetime` | sim | - |  |
| 12 | `INDEMUSO` | `numeric(1,0)` | sim | - |  |
| 13 | `MOTIVOEMUSO` | `varchar(30)` | sim | - |  |
| 14 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 15 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 16 | `INDUSOMKT` | `numeric(1,0)` | sim | - |  |
| 17 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 18 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 19 | `INDWHATSAPP` | `numeric(1,0)` | sim | - |  |

---

### GE_PessoaFonema

`classe: isolada` · `3 colunas` · `455.993 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqContato, Particula`

**Funcao:** A detecção usa busca fonética própria em GE_PessoaFonema (455.993 partículas) e GE_PessoaNomeFonema (151.583). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqContato` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Particula` | `varchar(30)` | **nao** | - | **PK** |

---

### GE_PessoaFone_BKPJUN

`classe: lixo/backup` · `19 colunas` · `69.894 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (19 colunas) para manter o documento legivel._

---

### GE_PessoaFone_ITA

`classe: lixo/backup` · `19 colunas` · `42.594 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (19 colunas) para manter o documento legivel._

---

### GE_PessoaJur

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `QtdFuncionario` | `decimal(4,0)` | sim | - |  |
| 3 | `QtdFilial` | `decimal(4,0)` | sim | - |  |
| 4 | `VlrFatMedioMes` | `decimal(15,2)` | sim | - |  |
| 5 | `VlrCapitalSoc` | `decimal(15,2)` | sim | - |  |
| 6 | `Clg1CNPJ` | `numeric(18,0)` | sim | - |  |
| 7 | `Clg1RazaoSoc` | `varchar(40)` | sim | - |  |
| 8 | `Clg1PercPart` | `decimal(6,2)` | sim | - |  |
| 9 | `Clg1Obs` | `varchar(100)` | sim | - |  |
| 10 | `Clg1SeqPessoa` | `numeric(10,0)` | sim | - |  |
| 11 | `Clg2CNPJ` | `numeric(18,0)` | sim | - |  |
| 12 | `Clg2RazaoSoc` | `varchar(40)` | sim | - |  |
| 13 | `Clg2PercPart` | `decimal(6,2)` | sim | - |  |
| 14 | `Clg2Obs` | `varchar(100)` | sim | - |  |
| 15 | `Clg2SeqPessoa` | `numeric(10,0)` | sim | - |  |

---

### GE_PessoaLink

`classe: isolada` · `5 colunas` · `32.831 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** Camada de identidade e reconciliação: 32.831 vínculos externos de pessoa, 602.150 de processo e 645.850 de histórico com sistemas de origem; 124.222 pares de pessoas candidatas a duplicidade. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `DtaGeracao` | `datetime` | sim | - |  |
| 5 | `SEQPESSOALINK` | `numeric(18,0)` | **nao** | - |  |

---

### GE_PessoaLinkbkp

`classe: isolada` · `5 colunas` · `58.741 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 2 | `Origem` | `varchar(20)` | **nao** | - | sistema de origem do dado |
| 3 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `DtaGeracao` | `datetime` | sim | - |  |
| 5 | `SEQPESSOALINK` | `numeric(18,0)` | sim | - |  |

---

### GE_PessoaLink_bkpago22

`classe: lixo/backup` · `5 colunas` · `92.068 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### GE_PessoaLink_BKPJUN

`classe: lixo/backup` · `5 colunas` · `90.857 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### GE_PessoaLink_ITA

`classe: lixo/backup` · `5 colunas` · `24.864 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### GE_PessoaMural

`classe: isolada` · `14 colunas` · `145.313 linhas (snapshot 03/06/2026)` · `PK: SeqPessoaMural`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com processo (`Processo`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoaMural` | `numeric(10,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `Resumo` | `varchar(30)` | **nao** | - |  |
| 4 | `Nivel` | `decimal(1,0)` | sim | - |  |
| 5 | `Detalhe` | `varchar(250)` | sim | - |  |
| 6 | `IndRemovivel` | `numeric(1,0)` | sim | - |  |
| 7 | `DtaExpira` | `datetime` | sim | - |  |
| 8 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 9 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 10 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 11 | `Processo` | `numeric(18,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 12 | `LinkOrigem` | `varchar(20)` | sim | - |  |
| 13 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 14 | `LinkStr` | `varchar(30)` | sim | - |  |

---

### GE_PessoaNomeFonema

`classe: isolada` · `4 colunas` · `151.583 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqContato`

**Funcao:** Busca fonética em GE_PessoaFonema (455.993) e GE_PessoaNomeFonema (151.583) (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqContato` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `NomeRazao` | `varchar(120)` | sim | - |  |
| 4 | `NomePuro` | `varchar(100)` | sim | - |  |

---

### GE_PessoaNota

`classe: nucleo` · `2 colunas` · `1.232 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** GE_PessoaNota, IV_ProcComent e IV_CodProcComent (comentário por modelo de processo) existem mas estão zerados. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Anotacao` | `text(2147483647)` | sim | - |  |

---

### GE_PessoaPasw

`classe: vazia` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `eMail` | `varchar(50)` | sim | - |  |
| 3 | `Senha` | `varchar(30)` | sim | - |  |
| 4 | `PerguntaChave` | `varchar(250)` | sim | - |  |
| 5 | `Resposta` | `varchar(30)` | sim | - |  |
| 6 | `TrocarSenha` | `char(1)` | sim | - |  |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `OrigemCadastro` | `varchar(20)` | sim | - |  |

---

### GE_PESSOAREDESOCIAL

`classe: vazia` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQPESREDESOCIAL`

**Funcao:** GE_PessoaDestino e GE_PESSOAREDESOCIAL têm 0. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQPESREDESOCIAL` | `numeric(12,0)` | **nao** | - | **PK** |
| 2 | `SEQPESSOA` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `REDESOCIAL` | `varchar(50)` | sim | - |  |
| 4 | `IDENTIFICADOR` | `varchar(250)` | sim | - |  |
| 5 | `URL` | `varchar(250)` | sim | - |  |
| 6 | `INDEMUSO` | `numeric(1,0)` | sim | - |  |
| 7 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 8 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 9 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_PessoaRelacao

`classe: isolada` · `9 colunas` · `5.595 linhas (snapshot 03/06/2026)` · `PK: TipoRelacionamento, SeqPrincipal, SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, e um relacionamento relacionada a pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `TipoRelacionamento` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `SeqPrincipal` | `numeric(10,0)` | **nao** | - | **PK** |
| 3 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `Obs` | `varchar(80)` | sim | - | texto livre |
| 5 | `LinkOrigem` | `varchar(20)` | sim | - |  |
| 6 | `Link` | `varchar(50)` | sim | - |  |
| 7 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### GE_PessoaRelacao_BKPJUN

`classe: lixo/backup` · `9 colunas` · `5.543 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, e um relacionamento relacionada a pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (9 colunas) para manter o documento legivel._

---

### GE_PessoaRelacao_ITA

`classe: lixo/backup` · `9 colunas` · `3.092 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, e um relacionamento relacionada a pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (9 colunas) para manter o documento legivel._

---

### GE_PessoaSimilar

`classe: nucleo` · `14 colunas` · `124.222 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa1, SeqPessoa2`

**Funcao:** Motor de detecção de duplicatas + fila de curadoria. 124.222 pares candidatos, 124.213 marcados Duplicado=1, SeqPessoaFica sugerido em 123.693 — mas apenas 9 revisados e a última revisão foi em 27/02/2018. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa1` | `numeric(10,0)` | **nao** | - | **PK**; FK -> `GE_Pessoa.SeqPessoa` |
| 2 | `SeqPessoa2` | `numeric(10,0)` | **nao** | - | **PK** |
| 3 | `DtaGeracao` | `datetime` | sim | - |  |
| 4 | `Probabilidade` | `decimal(3,0)` | sim | - |  |
| 5 | `ObsGeracao` | `varchar(150)` | sim | - |  |
| 6 | `Revisado` | `numeric(1,0)` | sim | - |  |
| 7 | `DtaRevisao` | `datetime` | sim | - |  |
| 8 | `Duplicado` | `numeric(1,0)` | sim | - |  |
| 9 | `Parecer` | `varchar(30)` | sim | - |  |
| 10 | `UsuParecer` | `varchar(20)` | sim | - |  |
| 11 | `UsuEmTrabalho` | `varchar(20)` | sim | - |  |
| 12 | `DtaEmTrabalho` | `datetime` | sim | - |  |
| 13 | `SeqPessoaFica` | `numeric(10,0)` | sim | - |  |
| 14 | `ObsFica` | `varchar(150)` | sim | - |  |

---

### GE_PessoaUnidade

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqUnidade` | `numeric(4,0)` | **nao** | - |  |
| 3 | `Nivel` | `numeric(1,0)` | sim | - |  |

---

### GE_PessoaVersao

`classe: isolada` · `19 colunas` · `12.019 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, Versao`

**Funcao:** GE_PessoaVersao (12.021 linhas / 6.473 pessoas) guarda o histórico das alterações CADASTRAIS sensíveis: NomeRazao, Cidade, Uf, Bairro, EnderecoCompleto, Cep, Pais, NroCGCCPF+DigCGCCPF, InscricaoRG, InscMunic, InscProdutor, CNAE, mais DtaAtualizacao, UsuAlterou, TipoAlteracao char(1) e Justificativa varchar(150). (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Versao` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 4 | `Cidade` | `varchar(50)` | sim | - |  |
| 5 | `Uf` | `varchar(2)` | sim | - |  |
| 6 | `Bairro` | `varchar(50)` | sim | - |  |
| 7 | `EnderecoCompleto` | `varchar(120)` | sim | - |  |
| 8 | `Cep` | `varchar(12)` | sim | - |  |
| 9 | `Pais` | `varchar(25)` | sim | - |  |
| 10 | `NroCGCCPF` | `decimal(13,0)` | sim | - |  |
| 11 | `DigCGCCPF` | `decimal(2,0)` | sim | - |  |
| 12 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 13 | `InscMunic` | `varchar(15)` | sim | - |  |
| 14 | `InscProdutor` | `varchar(20)` | sim | - |  |
| 15 | `CNAE` | `varchar(15)` | sim | - |  |
| 16 | `DtaAtualizacao` | `datetime` | sim | - |  |
| 17 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 18 | `TipoAlteracao` | `char(1)` | sim | - |  |
| 19 | `Justificativa` | `varchar(150)` | sim | - |  |

---

### GE_Pessoa_BKP28052025

`classe: lixo/backup` · `76 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (76 colunas) para manter o documento legivel._

---

### GE_Pessoa_BKPJUN

`classe: lixo/backup` · `76 colunas` · `88.142 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (76 colunas) para manter o documento legivel._

---

### GE_Pessoa_ITA

`classe: lixo/backup` · `76 colunas` · `28.377 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (76 colunas) para manter o documento legivel._

---

### GE_PolSeg

`classe: catalogo` · `3 colunas` · `11 linhas (snapshot 03/06/2026)` · `PK: SeqPolSeg`

**Funcao:** Catálogo de 108 itens de política de segurança tipados por módulo (senha, LGPD, agenda, histórico, processo, silos, TAGs, relatórios, mobile) e o mascaramento LGPD implementado como par de views FULL/LGPD. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPolSeg` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Politica` | `varchar(30)` | sim | - |  |
| 3 | `Descricao` | `varchar(250)` | sim | - | descricao do registro |

**Referenciada por:** `GE_CampoExig.SeqPolSeg`, `GE_POLSEGPERM.SEQPOLSEG`, `GE_PolSegAces.SeqPolSeg`, `GE_PolSegCtrl.SeqPolSeg`, `GE_PolSegParams.SeqPolSeg`, `GE_Usuario.SeqPolSeg`

---

### GE_PolSegAces

`classe: nucleo` · `7 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SeqPolSeg, Modulo, DiaSem`

**Funcao:** Motor de POLÍTICA DE SEGURANÇA: 11 políticas nomeadas × catálogo de 108 itens tipados × 254 valores efetivos, mais janela de horário e obrigatoriedade de campo. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPolSeg` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_PolSeg.SeqPolSeg` |
| 2 | `Modulo` | `varchar(10)` | **nao** | - | **PK** |
| 3 | `DiaSem` | `numeric(1,0)` | **nao** | - | **PK** |
| 4 | `HINI1` | `decimal(4,0)` | sim | - |  |
| 5 | `HFIM1` | `decimal(4,0)` | sim | - |  |
| 6 | `HINI2` | `decimal(4,0)` | sim | - |  |
| 7 | `HFIM2` | `decimal(4,0)` | sim | - |  |

---

### GE_PolSegCtrl

`classe: nucleo` · `7 colunas` · `254 linhas (snapshot 03/06/2026)` · `PK: Seq`

**Funcao:** Motor de POLÍTICA DE SEGURANÇA: 11 políticas nomeadas × catálogo de 108 itens tipados × 254 valores efetivos, mais janela de horário e obrigatoriedade de campo. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Modulo` | `varchar(10)` | **nao** | - |  |
| 3 | `Item` | `varchar(20)` | **nao** | - |  |
| 4 | `SeqPolSeg` | `decimal(6,0)` | **nao** | - | FK -> `GE_PolSeg.SeqPolSeg` |
| 5 | `Str` | `varchar(30)` | sim | - |  |
| 6 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_PolSegItem

`classe: catalogo` · `7 colunas` · `108 linhas (snapshot 03/06/2026)` · `PK: Modulo, Item`

**Funcao:** Catálogo de 108 itens de política de segurança tipados por módulo (senha, LGPD, agenda, histórico, processo, silos, TAGs, relatórios, mobile) e o mascaramento LGPD implementado como par de views FULL/LGPD. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Modulo` | `varchar(10)` | **nao** | - | **PK**; FK -> `GE_AppModulo.Modulo` |
| 2 | `Item` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `Descricao` | `varchar(100)` | sim | - | descricao do registro |
| 4 | `DescRed` | `varchar(30)` | sim | - |  |
| 5 | `Tipo` | `char(1)` | sim | - |  |
| 6 | `SubTipo` | `char(1)` | sim | - |  |
| 7 | `Ordem` | `varchar(10)` | sim | - |  |

**Referenciada por:** `GE_PolSegItLst.Item`, `GE_PolSegItLst.Modulo`

---

### GE_PolSegItLst

`classe: nucleo` · `6 colunas` · `66 linhas (snapshot 03/06/2026)` · `PK: Modulo, Item, Lista`

**Funcao:** Motor de POLÍTICA DE SEGURANÇA: 11 políticas nomeadas × catálogo de 108 itens tipados × 254 valores efetivos, mais janela de horário e obrigatoriedade de campo. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Modulo` | `varchar(10)` | **nao** | - | **PK**; FK -> `GE_PolSegItem.Modulo` |
| 2 | `Item` | `varchar(20)` | **nao** | - | **PK**; FK -> `GE_PolSegItem.Item` |
| 3 | `Lista` | `varchar(40)` | **nao** | - | **PK** |
| 4 | `Nro` | `numeric(18,0)` | sim | - |  |
| 5 | `Str` | `varchar(20)` | sim | - |  |
| 6 | `CTRLATUALIZACAO` | `numeric(1,0)` | sim | - |  |

---

### GE_PolSegParams

`classe: nucleo` · `7 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SeqPolSeg, Param1, Param2, Param3`

**Funcao:** Motor de POLÍTICA DE SEGURANÇA: 11 políticas nomeadas × catálogo de 108 itens tipados × 254 valores efetivos, mais janela de horário e obrigatoriedade de campo. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPolSeg` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_PolSeg.SeqPolSeg` |
| 2 | `Param1` | `varchar(30)` | **nao** | - | **PK** |
| 3 | `Param2` | `varchar(50)` | **nao** | - | **PK** |
| 4 | `Param3` | `varchar(50)` | **nao** | - | **PK** |
| 5 | `Str` | `varchar(250)` | sim | - |  |
| 6 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 7 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |

---

### GE_POLSEGPERM

`classe: nucleo` · `5 colunas` · `805 linhas (snapshot 03/06/2026)` · `PK: CODAPLICACAO, CHAVEAPLICACAO, NROEMPRESA, SEQPOLSEG`

**Funcao:** Mesma ideia da ACL de registro, mas concedida à POLÍTICA em vez do usuário — 805 linhas fazem o trabalho que na versão por usuário exige 38.680. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CODAPLICACAO` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `CHAVEAPLICACAO` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `NROEMPRESA` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 4 | `SEQPOLSEG` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_PolSeg.SeqPolSeg` |
| 5 | `PERMISSAO` | `decimal(4,0)` | sim | - |  |

---

### GE_PROCESSOWEB

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: PROCESSO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de processo/oportunidade do BPM, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com processo (`Processo`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ProcDado.Processo`; numero do processo (`IV_Processo.Processo`) |
| 2 | `DEPTOPESSOAPROC` | `varchar(20)` | sim | - |  |
| 3 | `SEQUSUARIOAPPRESP` | `numeric(18,0)` | **nao** | - |  |
| 4 | `DTASOLICUSUEXT` | `datetime` | sim | - |  |
| 5 | `INDAGUARDUSUEXT` | `numeric(1,0)` | sim | - |  |

---

### GE_QVCons

`classe: catalogo` · `28 colunas` · `134 linhas (snapshot 03/06/2026)` · `PK: SeqCons`

**Funcao:** Camada de relatórios: 134 consultas SQL editáveis em produção com layout .qrp, gráfico e telemetria de uso; 411 views no banco, das quais 32 são BI_* de funil/carteira/cobertura; e a Estrutura Gerencial de Análise (EGA) para DRE gerencial. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCons` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Cod` | `varchar(10)` | sim | - |  |
| 3 | `Nome` | `varchar(60)` | sim | - |  |
| 4 | `Descricao` | `varchar(200)` | sim | - | descricao do registro |
| 5 | `Adv` | `varchar(250)` | sim | - |  |
| 6 | `InstrSql` | `text(2147483647)` | sim | - |  |
| 7 | `Qrp` | `varchar(150)` | sim | - |  |
| 8 | `Prvw` | `numeric(1,0)` | sim | - |  |
| 9 | `FtTipo` | `varchar(30)` | sim | - |  |
| 10 | `FtTam` | `decimal(2,0)` | sim | - |  |
| 11 | `Tit` | `varchar(150)` | sim | - |  |
| 12 | `Sessao` | `varchar(20)` | sim | - |  |
| 13 | `GTipo` | `decimal(8,0)` | sim | - |  |
| 14 | `GTit` | `varchar(100)` | sim | - |  |
| 15 | `GRdp` | `varchar(50)` | sim | - |  |
| 16 | `GEsq` | `varchar(50)` | sim | - |  |
| 17 | `GLeg` | `varchar(30)` | sim | - |  |
| 18 | `GCol` | `varchar(100)` | sim | - |  |
| 19 | `Dono` | `varchar(20)` | sim | - |  |
| 20 | `IndSqlChg` | `numeric(1,0)` | sim | - |  |
| 21 | `Style` | `numeric(1,0)` | sim | - |  |
| 22 | `ChkS1` | `numeric(18,0)` | sim | - |  |
| 23 | `ChkS2` | `numeric(18,0)` | sim | - |  |
| 24 | `UltAtualizacao` | `datetime` | sim | - |  |
| 25 | `DtaInicioUso` | `datetime` | sim | - |  |
| 26 | `DtaUltimoUso` | `datetime` | sim | - |  |
| 27 | `UltUsuario` | `varchar(20)` | sim | - |  |
| 28 | `QtdeUso` | `decimal(8,0)` | sim | - |  |

**Referenciada por:** `GE_QVConsCol.SeqCons`, `GE_QVConsHst.SeqCons`, `GE_QVConsVar.SeqCons`, `GE_QVPastaCons.SeqCons`

---

### GE_QVConsCol

`classe: nucleo` · `8 colunas` · `40 linhas (snapshot 03/06/2026)` · `PK: SeqCons, Pos`

**Funcao:** Metadados de coluna do resultado (rótulo, tipo, formato, cor). Praticamente inutilizada: 40 linhas cobrindo apenas 2 relatórios (SeqCons 85 e 128) dos 134. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCons` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_QVCons.SeqCons` |
| 2 | `Pos` | `decimal(2,0)` | **nao** | - | **PK** |
| 3 | `ColName` | `varchar(30)` | sim | - |  |
| 4 | `Coluna` | `varchar(30)` | sim | - |  |
| 5 | `TpDado` | `varchar(2)` | sim | - |  |
| 6 | `Cor` | `numeric(18,0)` | sim | - |  |
| 7 | `Formato` | `varchar(20)` | sim | - |  |
| 8 | `Vars` | `varchar(50)` | sim | - |  |

---

### GE_QVConsHst

`classe: nucleo` · `5 colunas` · `69 linhas (snapshot 03/06/2026)` · `PK: SeqCons, SeqHst`

**Funcao:** Aparenta ser versionamento mas é campo de ANOTAÇÃO livre. 69 linhas para 134 relatórios, no máximo 2 por relatório (SeqHst 0 e 1), última em 2025-04-10. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCons` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_QVCons.SeqCons` |
| 2 | `SeqHst` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Detalhe` | `text(2147483647)` | sim | - |  |
| 4 | `Usr` | `varchar(20)` | sim | - |  |
| 5 | `Dta` | `datetime` | sim | - |  |

---

### GE_QVConsVar

`classe: nucleo` · `17 colunas` · `581 linhas (snapshot 03/06/2026)` · `PK: SeqCons, Var`

**Funcao:** Parâmetros de cada relatório (581 linhas). Cada parâmetro pode ter sua PRÓPRIA query de picklist — e essa query não tem filtro de escopo do usuário. (fonte: `09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqCons` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_QVCons.SeqCons` |
| 2 | `Var` | `varchar(6)` | **nao** | - | **PK** |
| 3 | `Ord` | `decimal(2,0)` | **nao** | - |  |
| 4 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 5 | `Padrao` | `varchar(100)` | sim | - |  |
| 6 | `DepAnt` | `numeric(1,0)` | sim | - |  |
| 7 | `Instrucao` | `varchar(250)` | sim | - |  |
| 8 | `InstrSql` | `varchar(1000)` | sim | - |  |
| 9 | `Lista` | `varchar(250)` | sim | - |  |
| 10 | `SQLCmd` | `varchar(250)` | sim | - |  |
| 11 | `Externa` | `numeric(1,0)` | sim | - |  |
| 12 | `TpDado` | `varchar(2)` | sim | - |  |
| 13 | `Tudo` | `numeric(1,0)` | sim | - |  |
| 14 | `LimLow` | `decimal(15,2)` | sim | - |  |
| 15 | `CorLow` | `numeric(18,0)` | sim | - |  |
| 16 | `LimHig` | `decimal(15,2)` | sim | - |  |
| 17 | `CorHig` | `numeric(18,0)` | sim | - |  |

---

### GE_QVPasta

`classe: nucleo` · `4 colunas` · `28 linhas (snapshot 03/06/2026)` · `PK: SeqPasta`

**Funcao:** Camada de relatórios: 134 consultas SQL editáveis em produção com layout .qrp, gráfico e telemetria de uso; 411 views no banco, das quais 32 são BI_* de funil/carteira/cobertura; e a Estrutura Gerencial de Análise (EGA) para DRE gerencial. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPasta` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(50)` | **nao** | - | descricao do registro |
| 3 | `CodPasta` | `varchar(8)` | **nao** | - |  |
| 4 | `SeqPastaPai` | `decimal(6,0)` | sim | - |  |

**Referenciada por:** `GE_QVPastaCons.SeqPasta`

---

### GE_QVPastaCons

`classe: nucleo` · `2 colunas` · `113 linhas (snapshot 03/06/2026)` · `PK: SeqPasta, SeqCons`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de relatorio/consulta, no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPasta` | `decimal(6,0)` | **nao** | - | **PK**; FK -> `GE_QVPasta.SeqPasta` |
| 2 | `SeqCons` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_QVCons.SeqCons` |

---

### GE_QVRegra

`classe: isolada` · `7 colunas` · `234 linhas (snapshot 03/06/2026)` · `PK: SeqRegra`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRegra` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqCons` | `numeric(18,0)` | **nao** | - |  |
| 3 | `Regra` | `varchar(20)` | sim | - |  |
| 4 | `Ordem` | `varchar(4)` | sim | - |  |
| 5 | `Condicao` | `varchar(250)` | sim | - |  |
| 6 | `Acao1` | `varchar(250)` | sim | - |  |
| 7 | `Acao2` | `varchar(250)` | sim | - |  |

---

### GE_Regiao

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRegiao`

**Funcao:** Geografia. GE_Cidade (10.214) e GE_Bairro são usados; GE_Regiao e GE_Rota têm 0 linhas apesar de GE_Pessoa.SeqRegiao/SeqRota terem FK. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRegiao` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Regiao` | `varchar(15)` | **nao** | - |  |
| 3 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 4 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 6 | `LinkStr` | `varchar(30)` | sim | - |  |

**Referenciada por:** `GE_Pessoa.SeqRegiao`

---

### GE_RelPasta

`classe: vazia` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPasta`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de recurso de apresentacao/relatorio, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPasta` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Nome` | `varchar(50)` | sim | - |  |
| 3 | `SeqPastaPai` | `numeric(18,0)` | sim | - |  |

**Referenciada por:** `GE_RelPastaCons.SeqPasta`

---

### GE_RelPastaCons

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPasta, SEQCONSSQL`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de relatorio/consulta, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPasta` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_RelPasta.SeqPasta` |
| 2 | `SEQCONSSQL` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_CONSSQL.SEQCONSSQL` |

---

### GE_RELTEMPLATE

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQRELTEMPLATE`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de recurso de apresentacao/relatorio, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQRELTEMPLATE` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `TIPO` | `varchar(20)` | **nao** | - |  |
| 3 | `NOME` | `varchar(50)` | **nao** | - |  |
| 4 | `DESCRICAO` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `ARQUIVONOME` | `varchar(100)` | sim | - |  |
| 6 | `TEMPLATE` | `text(2147483647)` | **nao** | - |  |

**Referenciada por:** `GE_CONSSQL.SEQRELTEMPLATE`

---

### GE_Rota

`classe: vazia` · `6 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqRota`

**Funcao:** Geografia. GE_Cidade (10.214) e GE_Bairro são usados; GE_Regiao e GE_Rota têm 0 linhas apesar de GE_Pessoa.SeqRegiao/SeqRota terem FK. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqRota` | `decimal(6,0)` | **nao** | - | **PK** |
| 2 | `Rota` | `varchar(15)` | **nao** | - |  |
| 3 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 4 | `DtaAlteracao` | `datetime` | **nao** | - | auditoria de alteracao (data) |
| 5 | `UsuAlteracao` | `varchar(20)` | **nao** | - | auditoria de alteracao (usuario) |
| 6 | `LinkStr` | `varchar(30)` | sim | - |  |

**Referenciada por:** `GE_Pessoa.SeqRota`

---

### GE_Sequencia

`classe: isolada` · `2 colunas` · `77 linhas (snapshot 03/06/2026)` · `PK: NomeTabela`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NomeTabela` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `Sequencia` | `numeric(18,0)` | **nao** | - |  |

---

### GE_Sistema

`classe: nucleo` · `3 colunas` · `11 linhas (snapshot 03/06/2026)` · `PK: Sistema`

**Funcao:** Catálogo de licenciamento e inventário de telas do produto: 11 sistemas, 48 módulos (16 com código de licença CRM_M0xx/QVW_M00x/GLB_M000) e 184 aplicações/telas registradas. É a fonte mais confiável do 'que o Vórtice vende'. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Sistema` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(40)` | sim | - | descricao do registro |
| 3 | `SiglaSistema` | `varchar(5)` | **nao** | - |  |

**Referenciada por:** `GE_Modulo.Sistema`

---

### GE_SyncParam

`classe: isolada` · `10 colunas` · `635 linhas (snapshot 03/06/2026)` · `PK: Origem, Tabela, ChaveN1, ChaveN2, ChaveStr`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de job/sincronizacao, no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Origem` | `varchar(30)` | **nao** | - | **PK**; sistema de origem do dado |
| 2 | `Tabela` | `varchar(30)` | **nao** | - | **PK** |
| 3 | `ChaveN1` | `numeric(18,0)` | **nao** | - | **PK** |
| 4 | `ChaveN2` | `numeric(18,0)` | **nao** | - | **PK** |
| 5 | `ChaveStr` | `varchar(30)` | **nao** | - | **PK** |
| 6 | `ChaveLocalN1` | `numeric(18,0)` | sim | - |  |
| 7 | `ChaveLocalN2` | `numeric(18,0)` | sim | - |  |
| 8 | `ChaveLocalStr` | `varchar(30)` | sim | - |  |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `IndMudanca` | `numeric(1,0)` | sim | - |  |

---

### GE_Tab

`classe: catalogo` · `2 colunas` · `12 linhas (snapshot 03/06/2026)` · `PK: Tabela`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tabela` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(200)` | sim | - | descricao do registro |

**Referenciada por:** `GE_Col.Tabela`, `GE_TabRegra.Tabela`

---

### GE_TabRegra

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqTabRegra`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqTabRegra` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(30)` | **nao** | - | FK -> `GE_Tab.Tabela` |
| 3 | `SeqPolSeg` | `numeric(6,0)` | **nao** | - |  |
| 4 | `Condicao` | `varchar(30)` | **nao** | - |  |
| 5 | `Regra` | `numeric(18,0)` | sim | - |  |

---

### GE_TempLong

`classe: isolada` · `3 colunas` · `6 linhas (snapshot 03/06/2026)` · `PK: kn, ks`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `kn` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `ks` | `varchar(20)` | **nao** | - | **PK** |
| 3 | `Dado` | `text(2147483647)` | sim | - |  |

---

### GE_TipoLogradouro

`classe: isolada` · `3 colunas` · `124 linhas (snapshot 03/06/2026)` · `PK: TipoLogradouro`

**Funcao:** _(inferido)_ Pelo nome, e um catalogo de tipos, no modulo `GE` (geral/plataforma).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `TipoLogradouro` | `varchar(15)` | **nao** | - | **PK** |
| 2 | `Descricao` | `varchar(20)` | sim | - | descricao do registro |
| 3 | `Substituicoes` | `varchar(200)` | sim | - |  |

---

### GE_URACENARIO

`classe: vazia` · `15 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQURACENARIO`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de telefonia, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQURACENARIO` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `IDCENARIO` | `varchar(50)` | sim | - |  |
| 3 | `NOME` | `varchar(100)` | sim | - |  |
| 4 | `DESCRICAO` | `varchar(250)` | sim | - | descricao do registro |
| 5 | `REMETENTE` | `numeric(12,0)` | sim | - |  |
| 6 | `MINPERIODO` | `numeric(4,0)` | sim | - |  |
| 7 | `MAXPERIODO` | `numeric(4,0)` | sim | - |  |
| 8 | `MAXCOUNT` | `numeric(1,0)` | sim | - |  |
| 9 | `NOMEVIEW` | `varchar(30)` | sim | - |  |
| 10 | `PARAMETROS` | `varchar(250)` | sim | - |  |
| 11 | `SCRIPT` | `text(2147483647)` | sim | - |  |
| 12 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 13 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 14 | `NOTIFYURL` | `varchar(100)` | sim | - |  |
| 15 | `NOTIFYCONTENTTYPE` | `varchar(20)` | sim | - |  |

**Referenciada por:** `IV_URADISPARO.SEQURACENARIO`

---

### GE_UsrAcSp

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Tipo, SeqUsuario, KS1`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tipo` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 3 | `KS1` | `varchar(20)` | **nao** | - | **PK** |
| 4 | `Str1` | `varchar(30)` | sim | - |  |
| 5 | `DtaAtribuido` | `datetime` | sim | - |  |
| 6 | `DtaLimite` | `datetime` | sim | - |  |
| 7 | `CHKSUM` | `varchar(50)` | sim | - |  |

---

### GE_UsrCtrl

`classe: vazia` · `7 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: Seq`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de controle/condicoes de uso relacionada a usuario do sistema, no modulo `GE` (geral/plataforma). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Seq` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Uc` | `numeric(18,0)` | sim | - |  |
| 3 | `Sm` | `varchar(10)` | sim | - |  |
| 4 | `Mq` | `varchar(50)` | sim | - |  |
| 5 | `Cx` | `varchar(40)` | sim | - |  |
| 6 | `Dc` | `datetime` | sim | - |  |
| 7 | `Up` | `datetime` | sim | - |  |

---

### GE_UsrParam

`classe: nucleo` · `7 colunas` · `16.827 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, NroEmpresa, Parametro`

**Funcao:** Parâmetros por usuário (16.827 linhas), globais por sistema/módulo/empresa (236) e listas (333). Contêm preferência de UI, mas TAMBÉM autorização (escopo de empresa em relatório) e segredo (token de API em texto claro). (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 3 | `Parametro` | `varchar(30)` | **nao** | - | **PK** |
| 4 | `Valor` | `varchar(1000)` | sim | - |  |
| 5 | `Criptografado` | `char(1)` | sim | - |  |
| 6 | `Dtaalteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 7 | `Usualteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### GE_Usuario

`classe: catalogo` · `35 colunas` · `1.379 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario`

**Funcao:** Identidade única para USUÁRIO e GRUPO (TipoUsuario 'U'=1.068 / 'G'=318). Guarda credencial, política de segurança, nível/status e os dois últimos logins. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `CodUsuario` | `varchar(20)` | **nao** | - | login do usuario (varchar) |
| 3 | `Nome` | `varchar(40)` | sim | - |  |
| 4 | `NomeReduzido` | `varchar(20)` | sim | - |  |
| 5 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 6 | `Senha` | `varchar(30)` | sim | - |  |
| 7 | `LoginId` | `varchar(20)` | sim | - |  |
| 8 | `TipoUsuario` | `char(1)` | **nao** | - |  |
| 9 | `RegistrarLog` | `char(1)` | **nao** | - |  |
| 10 | `DTALIMITEUSO` | `datetime` | sim | - |  |
| 11 | `Nivel` | `decimal(1,0)` | sim | - |  |
| 12 | `Assinatura` | `varchar(30)` | sim | - |  |
| 13 | `CodUsuarioExt` | `varchar(40)` | sim | - |  |
| 14 | `NroUsuarioExt` | `numeric(18,0)` | sim | - |  |
| 15 | `RecebeCiencia` | `numeric(1,0)` | sim | - |  |
| 16 | `Senha3` | `varchar(30)` | sim | - |  |
| 17 | `Loginexpirando` | `numeric(1,0)` | sim | - |  |
| 18 | `Qtdeloginrestante` | `numeric(18,0)` | sim | - |  |
| 19 | `Ulttrocasenha` | `datetime` | sim | - |  |
| 20 | `Identificacao` | `varchar(40)` | sim | - |  |
| 21 | `SeqPolSeg` | `decimal(6,0)` | sim | - | FK -> `GE_PolSeg.SeqPolSeg` |
| 22 | `CelularTrab` | `varchar(20)` | sim | - |  |
| 23 | `INDSILO` | `numeric(1,0)` | sim | - |  |
| 24 | `ChkSum` | `varchar(50)` | sim | - |  |
| 25 | `TIPOSILO` | `varchar(6)` | sim | - |  |
| 26 | `SEQCONTATO` | `numeric(4,0)` | sim | - |  |
| 27 | `INDUSREXT` | `numeric(1,0)` | sim | - |  |
| 28 | `LOGIN` | `varchar(100)` | sim | - |  |
| 29 | `EMAILTRAB` | `varchar(100)` | sim | - |  |
| 30 | `DTAINCLUSAO` | `datetime` | sim | - | auditoria de inclusao (data) |
| 31 | `USUINCLUSAO` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 32 | `DTAALTERACAO` | `datetime` | sim | - | auditoria de alteracao (data) |
| 33 | `USUALTERACAO` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 34 | `DTALOGIN` | `datetime` | sim | - |  |
| 35 | `DTALOGINANT` | `datetime` | sim | - |  |

**Referenciada por:** `GE_CampoMemo.SeqUsuario`, `GE_CampoPerm.SeqUsuario`, `GE_Membro.Usuario`, `GE_ModuloPerm.SeqUsuario`, `GE_PESSOAATIVAUSR.SEQUSUARIO`, `GE_Permissao.SeqUsuario`, `GE_UsrParam.SeqUsuario`, `GE_UsuarioLink.SeqUsuario`, `Gep_UsrPabx.SeqUsuario`, `IVC_ATENDENTE.SEQUSUARIO`, `IVC_EQUIPEUSR.SEQUSUARIO`, `IVS_Depto.SeqUsrDirDepto`, `IVS_DeptoEmpr.SeqUsrDirDepto`, `IVS_DeptoEmpr.SeqUsrGerDepto`, `IVS_UsrMeta.SeqUsuario`, `IV_AcaoMon.SeqUsuario`, `IV_AgdRec.SeqUsuario`, `IV_AtdBloq.SeqUsuario`, `IV_CbrCriterio.SeqUsrDestino`, `IV_CobrCrit.SeqUsuario`, `IV_ConhecLeitura.SeqUsuario`, `IV_Distribui.SeqUsuario`, `IV_ProjEquipe.SeqUsuario`, `IV_USRPUSH.SEQUSUARIO`, `IV_USRSTATUS.SEQUSUARIO`

---

### GE_USUARIOCMPL

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQUSUARIO`

**Funcao:** _(inferido)_ Pelo nome, e um complemento do registro pai relacionada a usuario do sistema, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com usuario (`SeqUsuario`). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `G_TOKEN` | `varchar(500)` | sim | - |  |
| 3 | `G_ULTSINC` | `datetime` | sim | - |  |
| 4 | `G_STATUSSINC` | `numeric(1,0)` | sim | - |  |
| 5 | `G_IDCALENDARIO` | `varchar(250)` | sim | - |  |

---

### GE_UsuarioLink

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, SeqUsrLink`

**Funcao:** GE_UsuarioLink (vínculo com sistema externo) tem 0 linhas; (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `GE_Usuario.SeqUsuario`; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `SeqUsrLink` | `decimal(6,0)` | **nao** | - | **PK** |
| 3 | `NroUsuarioExt` | `numeric(18,0)` | sim | - |  |
| 4 | `CodUsuarioExt` | `varchar(40)` | sim | - |  |
| 5 | `ORIGEM` | `varchar(20)` | sim | - | sistema de origem do dado |

---

### GE_UsuarioPerm

`classe: isolada` · `5 colunas` · `38.601 linhas (snapshot 03/06/2026)` · `PK: CodAplicacao, ChaveAplicacao, SeqUsuario, NroEmpresa`

**Funcao:** ACL POLIMÓRFICA por registro de catálogo, concedida a usuário OU grupo. 38.680 linhas; 34.124 para 273 grupos e 4.295 para 131 usuários. (fonte: `07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `CodAplicacao` | `varchar(20)` | **nao** | - | **PK** |
| 2 | `ChaveAplicacao` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 4 | `NroEmpresa` | `numeric(6,0)` | **nao** | - | **PK**; multiempresa - filial/empresa |
| 5 | `Permissao` | `char(1)` | sim | - |  |

---

### GE_UsuarioPerm_BKPJUN

`classe: lixo/backup` · `5 colunas` · `31.120 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com usuario (`SeqUsuario`), escopo multiempresa (`NroEmpresa`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### GE_UsuarioSenhaMem

`classe: isolada` · `3 colunas` · `2.456 linhas (snapshot 03/06/2026)` · `PK: SeqUsuario, DtaTroca`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com usuario (`SeqUsuario`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** | - | **PK**; usuario do sistema (`GE_Usuario.SeqUsuario`) |
| 2 | `DtaTroca` | `datetime` | **nao** | - | **PK** |
| 3 | `Senha` | `varchar(30)` | sim | - |  |

---

### GE_Usuario_BKP20250520

`classe: lixo/backup` · `35 colunas` · `1.302 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de usuario do sistema, no modulo `GE` (geral/plataforma). As colunas confirmam vinculo com pessoa (`SeqPessoa`), vinculo com usuario (`SeqUsuario`).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (35 colunas) para manter o documento legivel._

---
