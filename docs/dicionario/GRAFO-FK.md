# Grafo de chaves estrangeiras - o nucleo do modelo

> Snapshot de 03/06/2026. Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao.**

Nos diagramas `erDiagram`, as arestas sao **apenas FKs realmente declaradas** em `schema/fks.csv`. Boa parte das ligacoes do Vortice e feita **por convencao de nome, sem FK** - notavelmente `IV_Agenda.Processo`, `IV_Historico.Processo` e `IV_ProcDado.Processo`, que **nao** tem FK para `IV_Processo`. Essas ligacoes aparecem como linha tracejada nos diagramas `graph` que acompanham cada bloco.

[Voltar ao indice](00-INDICE.md)

---

## 1. Nucleo CRM/BPM: pessoa -> processo -> agenda -> historico -> questionario

### 1a. FKs declaradas no nucleo

Cada linha e uma FK real, no formato `destino ||--o{ origem : "coluna_origem"`.

```mermaid
erDiagram
    GE_Pessoa ||--o{ IVS_Pes : "SeqPessoa"
    IV_Formulario ||--o{ IV_Acao : "SeqFormulario"
    IV_Acao ||--o{ IV_AcaoAuto : "Acao"
    IV_Resultado ||--o{ IV_AcaoAuto : "Resultado"
    IV_Acao ||--o{ IV_Agenda : "Acao"
    GE_Pessoa ||--o{ IV_Agenda : "SeqPessoa"
    IV_Resultado ||--o{ IV_Historico : "Resultado"
    GE_Pessoa ||--o{ IV_Historico : "SeqPessoa"
    IV_Historico ||--o{ IV_Interacao : "SeqHistorico"
    IV_CodProcesso ||--o{ IV_ProcFase : "CodProcesso"
    IV_CodProcesso ||--o{ IV_ProcResultado : "CodProcesso"
    IV_Resultado ||--o{ IV_ProcResultado : "Resultado"
    IV_Formulario ||--o{ IV_Questao : "SeqFormulario"
    IV_Questao ||--o{ IV_QuestaoLista : "Questao"
    IV_Formulario ||--o{ IV_Questionario : "SeqFormulario"
    IV_Resultado ||--o{ IV_ResultadoCmpl : "Resultado"
```

**Sem FK declarada dentro deste recorte** (ligam-se por convencao de nome, nao por integridade referencial): `IV_Processo`, `IV_ProcDado`, `IV_Ciencia`, `IV_HistLink`, `IV_ProcDocto`, `DMN_Doc`, `IVS_Carteira`, `IV_VENDEDOR`, `GE_Usuario`

_16 nos com FK, 16 arestas._

### 1b. O fluxo logico (inclui as ligacoes SEM FK)

Linha cheia = FK declarada. Linha tracejada = ligacao por convencao de nome, **sem** integridade referencial no banco (documentado em `04-nucleo-crm-bpm...md` e em `REGRAS-DE-NEGOCIO.md` 1.2 e 1.3).

```mermaid
graph LR
    GE_Pessoa["GE_Pessoa<br/>pessoa - PK SeqPessoa"]
    IV_Processo["IV_Processo<br/>oportunidade - PK Processo"]
    IV_ProcDado["IV_ProcDado<br/>dados do processo"]
    IV_Agenda["IV_Agenda<br/>tarefa - PK SeqAgenda"]
    IV_Historico["IV_Historico<br/>andamento - PK SeqHistorico"]
    IV_Interacao["IV_Interacao<br/>janelas da interacao"]
    IV_Questionario["IV_Questionario<br/>resposta de formulario"]
    IV_Formulario["IV_Formulario<br/>definicao do formulario"]
    IV_Questao["IV_Questao<br/>questoes"]
    IV_Q["IV_Q_*<br/>175 tabelas fisicas geradas"]
    IV_Resultado["IV_Resultado<br/>desfecho escolhido"]
    IV_Acao["IV_Acao<br/>tipo de tarefa"]
    IV_ProcResultado["IV_ProcResultado<br/>Resultado -> Fase/Status"]
    IV_AcaoAuto["IV_AcaoAuto<br/>Resultado -> proxima Acao"]
    IV_ProcFase["IV_ProcFase<br/>fases do fluxo"]
    IV_CodProcesso["IV_CodProcesso<br/>tipo de fluxo 41/50"]
    GE_Usuario["GE_Usuario<br/>usuario"]
    IV_VENDEDOR["IV_VENDEDOR<br/>CEN + hierarquia"]
    IVS_Pes["IVS_Pes<br/>carteirizacao/RFV"]
    IVS_Carteira["IVS_Carteira<br/>carteira"]

    GE_Pessoa --> IV_Historico
    GE_Pessoa --> IV_Agenda
    GE_Pessoa --> IV_Questionario
    GE_Pessoa --> IVS_Pes
    IVS_Pes --> IVS_Carteira
    IV_Processo -.->|sem FK| IV_ProcDado
    IV_Processo -.->|sem FK| IV_Agenda
    IV_Processo -.->|sem FK| IV_Historico
    IV_Agenda -->|HistoricoOrigem| IV_Historico
    IV_Historico -->|AgendaOrigem| IV_Agenda
    IV_Historico --> IV_Interacao
    IV_Historico --> IV_Questionario
    IV_Resultado --> IV_Historico
    IV_Acao --> IV_Agenda
    IV_Resultado --> IV_ProcResultado
    IV_ProcResultado -.->|muda Fase/Status| IV_Processo
    IV_Resultado --> IV_AcaoAuto
    IV_AcaoAuto -.->|gera| IV_Agenda
    IV_AcaoAuto --> IV_Acao
    IV_CodProcesso --> IV_ProcFase
    IV_CodProcesso --> IV_ProcResultado
    IV_Formulario --> IV_Questao
    IV_Formulario --> IV_Questionario
    IV_Questao -.->|gera DDL| IV_Q
    IV_Questionario --> IV_Q
    GE_Usuario -.-> IV_Agenda
    IV_VENDEDOR -.->|SeqUsuarioLider| GE_Usuario
```

> `IV_Agenda.CodProcesso` e `IV_Historico.CodProcesso` guardam o **tipo de fluxo** (41 ou 50), nao o numero do processo - que fica em `Processo`. `IV_Processo` **nao tem** coluna `CodProcesso` (`REGRAS-DE-NEGOCIO.md` 1.3).

---

## 2. ERP: equipamentos, notas fiscais, ordens de servico e titulos

### 2a. FKs declaradas no bloco EXT_/IMP_

Dados espelhados do TOTVS/JDE. `SeqPessoa` liga tudo de volta a `GE_Pessoa`.

```mermaid
erDiagram
    EXT_NFSOper ||--o{ EXT_NFS : "IdNFSOper"
    EXT_Veic ||--o{ EXT_NFS : "IdVeic"
    EXT_Vendedor ||--o{ EXT_NFS : "IdVendedor"
    GE_Pessoa ||--o{ EXT_NFS : "SeqPessoa"
    EXT_NFS ||--o{ EXT_NFSItem : "IdNFS"
    EXT_Produto ||--o{ EXT_NFSItem : "IdProduto"
    EXT_Vendedor ||--o{ EXT_NFSItem : "IdVendedor"
    EXT_Veic ||--o{ EXT_OS : "IdVeic"
    GE_Pessoa ||--o{ EXT_OS : "SeqPessoa"
    EXT_Pessoa ||--o{ EXT_Pedido : "IdPessoa"
    GE_Pessoa ||--o{ EXT_Pedido : "SeqPessoa"
    GE_Pessoa ||--o{ EXT_Pessoa : "SeqPessoa"
    EXT_Pessoa ||--o{ EXT_Titulo : "IdPessoa"
    GE_Pessoa ||--o{ EXT_Titulo : "SeqPessoa"
    EXT_Titulo ||--o{ EXT_TituloMov : "idTitulo"
    EXT_VeicMarca ||--o{ EXT_Veic : "IdVeicMarca"
    EXT_VeicModelo ||--o{ EXT_Veic : "IdVeicModelo"
    GE_Pessoa ||--o{ EXT_Veic : "SeqPessoa"
    EXT_VeicPlanoMan ||--o{ EXT_Veic : "SeqPlanoMAN"
    EXT_VeicMarca ||--o{ EXT_VeicFam : "IdVeicMarca"
    EXT_VeicPlanoMan ||--o{ EXT_VeicMarca : "SeqPlanoManMarca"
    EXT_VeicTipoMan ||--o{ EXT_VeicMarca : "SeqTipoManPadrao"
    EXT_VeicModelo ||--o{ EXT_VeicModPlano : "IdVeicModelo"
    EXT_VeicPlanoMan ||--o{ EXT_VeicModPlano : "SeqPlanoMAN"
    EXT_VeicFam ||--o{ EXT_VeicModelo : "IdVeicFamilia"
    EXT_VeicTipoMan ||--o{ EXT_VeicPlanoMan : "SeqTipoMan"
```

**Sem FK declarada dentro deste recorte** (ligam-se por convencao de nome, nao por integridade referencial): `IMP_OS`, `IMP_Titulo`

_18 nos com FK, 26 arestas._

### 2b. As quatro arvores do ERP

```mermaid
graph LR
    GE_Pessoa["GE_Pessoa"]
    subgraph Equipamento
      EXT_VeicMarca --> EXT_VeicFam
      EXT_VeicFam --> EXT_VeicModelo
      EXT_VeicModelo --> EXT_Veic
      EXT_VeicTipoMan --> EXT_VeicPlanoMan
      EXT_VeicPlanoMan --> EXT_Veic
    end
    subgraph Faturamento
      EXT_NFSOper --> EXT_NFS
      EXT_NFS --> EXT_NFSItem
    end
    subgraph Servico
      EXT_OS --> EXT_OSItem
      IMP_OS -.->|staging| EXT_OS
    end
    subgraph Financeiro
      EXT_Titulo --> EXT_TituloMov
      IMP_Titulo -.->|staging| EXT_Titulo
      X_T_IMP_CRM_TITULO -.->|TOTVS| IMP_Titulo
    end
    GE_Pessoa --> EXT_Veic
    GE_Pessoa --> EXT_NFS
    GE_Pessoa --> EXT_OS
    GE_Pessoa --> EXT_Titulo
    EXT_Veic --> EXT_NFS
    EXT_Veic --> EXT_OS
```

> Freshness: `EXT_NFS` e `X_TOTVS_CRM_FATURAMENTO` param em **11/04/2025** (`SCHEMA_MAP.md`). Confirmar `MAX(<coluna_data>)` antes de afirmar tendencia recente.

---

## 3. Seguranca, usuarios e multiempresa

### 3a. FKs declaradas no bloco GE_ de seguranca

Usuarios, politicas de seguranca, permissoes e o cadastro de aplicacoes/telas.

```mermaid
erDiagram
    GE_Modulo ||--o{ GE_Aplicacao : "Modulo"
    GE_Sistema ||--o{ GE_Modulo : "Sistema"
    GE_Empresa ||--o{ GE_Permissao : "NroEmpresa"
    GE_Usuario ||--o{ GE_Permissao : "SeqUsuario"
    GE_PolSeg ||--o{ GE_Usuario : "SeqPolSeg"
    IVC_EQUIPE ||--o{ IVC_EQUIPEUSR : "SEQEQUIPE"
    GE_Usuario ||--o{ IVC_EQUIPEUSR : "SEQUSUARIO"
```

**Sem FK declarada dentro deste recorte** (ligam-se por convencao de nome, nao por integridade referencial): `GE_PolSegItem`, `IV_Operador`, `IV_Atendente`

_9 nos com FK, 7 arestas._

### 3b. As camadas de autorizacao

```mermaid
graph TD
    GE_Sistema["GE_Sistema<br/>sistema"] --> GE_Modulo["GE_Modulo<br/>modulo"]
    GE_Modulo --> GE_Aplicacao["GE_Aplicacao<br/>tela/aplicacao"]
    GE_Aplicacao --> GE_Permissao["GE_Permissao<br/>permissao por objeto"]
    GE_PolSeg["GE_PolSeg<br/>politica de seguranca"] --> GE_Permissao
    GE_PolSeg --> GE_Usuario["GE_Usuario<br/>usuario do CRM"]
    GE_Usuario --> IV_Operador["IV_Operador<br/>operador"]
    GE_Usuario --> IV_Atendente["IV_Atendente<br/>atendente"]
    GE_Usuario --> IV_VENDEDOR["IV_VENDEDOR<br/>vendedor/CEN"]
    GE_Empresa["GE_Empresa<br/>empresa/filial"] -.->|NroEmpresa| GE_Usuario
    GE_Empresa -.->|NroEmpresa| GE_PolSeg
    IV_VENDEDOR -->|SeqUsuarioLider| GE_Usuario
```

> Criar um usuario toca ~5-7 tabelas (`GE_Usuario`, `IVC_EQUIPEUSR`, `IV_Operador`, `IV_Atendente`/`IVC_ATENDENTE`, `IV_VENDEDOR`, `GE_PolSeg*`) e **nao tem API** (`REGRAS-DE-NEGOCIO.md` 2.11).

---

_Os diagramas `erDiagram` sao gerados a partir de `schema/fks.csv`. As arestas tracejadas dos diagramas `graph` vem de regras documentadas em `SCHEMA_MAP.md` e `REGRAS-DE-NEGOCIO.md` e **nao existem como constraint no banco**._
