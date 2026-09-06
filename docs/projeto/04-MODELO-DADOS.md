# Modelo de dados — CRM Tracbel

> **Documento 04 de 07** · Versão 1.0 · 30/08/2026
> Banco: **SQL Server 2019+**, database próprio `TRACBEL_CRM`. Acesso via **EF Core 8**.
> Cada decisão abaixo cita a lição que a originou: `[V]` = defeito medido no Vórtice,
> `[SF]` = Salesforce, `[DYN]` = Dynamics/Dataverse.

---

## 1. Convenções — leia antes de tudo

Estas regras valem para **todas** as tabelas. Um dev novo que entender esta seção consegue ler
qualquer parte do modelo.

### 1.1 Nomenclatura

| Regra | Exemplo | Por quê |
|---|---|---|
| Tabelas no **singular**, PascalCase, em português | `Conta`, `TipoTarefa` | o time lê em português; singular porque a linha é *uma* conta |
| Nada de prefixo de módulo no nome | `Conta`, não `CRM_Conta` | `[V]` os prefixos `IV_`/`GE_`/`EXT_` do Vórtice deixaram de significar algo; hoje só confundem |
| Schema separa o módulo | `vendas.Oportunidade`, `seg.Usuario` | o agrupamento é do SQL Server, não do nome |
| FK: nome da entidade + `Id` | `ContaId`, `ProprietarioId` | `[V]` no Vórtice, `IV_Agenda.Vendedor` guarda um *login*, não um ID — nunca mais |
| Booleano: prefixo verbal | `EstaAtivo`, `PermiteEdicao` | `[V]` `numeric(1)` com 0/1/9 e domínio ternário não documentado |
| Data/hora: sufixo `Em`; data pura: sufixo `Data` | `CriadoEm`, `PrevisaoData` | evita ambiguidade de fuso |

### 1.2 Tipos — as correções obrigatórias

| Use | Nunca use | Motivo |
|---|---|---|
| `datetime2(3)` | `datetime` | precisão e faixa corretas |
| `decimal(18,2)` para dinheiro | `float`, `money` | `[V]` arredondamento silencioso |
| `varchar` só para código ASCII; `nvarchar` para texto de gente | `varchar` em nome/observação | `[V]` acentuação corrompida em `Detalhe` |
| `varchar(20)` para telefone | `decimal(12)` | `[V]` **`GE_Pessoa.FoneNro1` é `decimal(12)`** — telefone com DDI ou zero à esquerda estoura e a API devolve erro genérico |
| `bit` | `char(1)` com 'S'/'N' | `[V]` `IV_Agenda.Realizada` é `char(1)` e aceita `NULL`, criando um terceiro estado acidental |
| Coluna com o tamanho que o negócio pede | tamanho menor que a gravação | `[V]` `IV_Historico.ResultadoCmpl` é `varchar(150)` mas **grava truncado em 20** |

### 1.3 Colunas obrigatórias em toda tabela transacional

```sql
-- Bloco padrão. Copie em toda tabela de negócio.
Id              bigint         IDENTITY(1,1) NOT NULL,  -- PK interna, clustered, nunca exposta
ChavePublica    uniqueidentifier NOT NULL
                CONSTRAINT DF_<Tabela>_ChavePublica DEFAULT NEWID(),  -- o ID que a API expõe
EmpresaId       int            NOT NULL,  -- multiempresa: SEMPRE presente (ver 2.1)
CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_<Tabela>_CriadoEm DEFAULT SYSUTCDATETIME(),
CriadoPorId     bigint         NOT NULL,
AlteradoEm      datetime2(3)   NULL,
AlteradoPorId   bigint         NULL,
ExcluidoEm      datetime2(3)   NULL,  -- soft delete: NULL = ativo
Versao          rowversion     NOT NULL  -- concorrência otimista, gerenciado pelo SQL Server
```

**Por que `Id` interno + `ChavePublica` externa:** o `bigint` sequencial dá índice clustered eficiente;
o GUID é o que aparece na URL e na API, para que ninguém consiga enumerar registros nem inferir volume
de negócio pelo ID. `[SF]` faz o mesmo com IDs opacos.

**Por que soft delete:** `[V]` desativar um usuário no Vórtice **destrói as permissões** porque não há
flag de ativo/inativo. Aqui nada é apagado fisicamente no caminho transacional.

### 1.4 Regras de integridade que não se negociam

1. **Toda FK é declarada no banco.** `[V]` não existe FK de `IV_Agenda`, `IV_Historico` nem `IV_ProcDado`
   para `IV_Processo` — resultado: **345.535 linhas órfãs** e o sincronismo do mobile abortando em
   documento inexistente. Aqui, o banco recusa.
2. **Nenhuma coluna de texto livre onde existe catálogo.** `[V]` a coluna `Origem` virou texto livre,
   com duplicatas e valores de teste em produção.
3. **Todo `CHECK` de domínio é declarado**, não confiado à aplicação.
4. **Índice filtrado para soft delete:** `WHERE ExcluidoEm IS NULL` em todo índice de busca.

---

## 2. Módulo `org` — empresa, usuário e hierarquia

### 2.1 `org.Empresa` — a unidade organizacional

`[DYN]` É o equivalente da *Business Unit*: **fronteira de segurança, não organograma**. É a coluna
`EmpresaId` gravada em cada linha que torna a checagem de acesso barata — sem join.

```sql
CREATE TABLE org.Empresa (
    Id              int            IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Empresa_Chave DEFAULT NEWID(),
    Codigo          varchar(20)    NOT NULL,  -- código da filial usado no TOTVS
    Nome            nvarchar(120)  NOT NULL,
    Cnpj            varchar(14)    NULL,
    -- Hierarquia de unidades. NULL = raiz (a holding).
    EmpresaPaiId    int            NULL,
    -- Caminho materializado: '/1/4/9/'. Permite "esta empresa e todas abaixo" com um LIKE,
    -- sem CTE recursiva. Mantido por trigger (ver 9.2).
    Caminho         varchar(400)   NOT NULL CONSTRAINT DF_Empresa_Caminho DEFAULT '/',
    Nivel           smallint       NOT NULL CONSTRAINT DF_Empresa_Nivel DEFAULT 0,
    EstaAtiva       bit            NOT NULL CONSTRAINT DF_Empresa_Ativa DEFAULT 1,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Empresa_CriadoEm DEFAULT SYSUTCDATETIME(),
    AlteradoEm      datetime2(3)   NULL,
    CONSTRAINT PK_Empresa PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Empresa_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Empresa_Pai FOREIGN KEY (EmpresaPaiId) REFERENCES org.Empresa(Id)
);
CREATE INDEX IX_Empresa_Caminho ON org.Empresa (Caminho) WHERE EstaAtiva = 1;
```

> `[V]` No Vórtice a multiempresa é **nominal**: 18 filiais, `NroEmpresa` em toda tabela de permissão,
> mas **340 dos 409 usuários ativos têm acesso a 17 empresas**. Isolamento que ninguém usa é isolamento
> que não existe. Aqui a empresa é a raiz do escopo de acesso, e o padrão é ver **só a sua**.

### 2.2 `seg.Usuario` — espelho do Entra ID

O CRM **não guarda senha**. `[V]` No Vórtice a senha tem 30 bytes sem salt, e **80 usuários compartilham
exatamente o mesmo valor armazenado**. Aqui a autenticação é delegada ao Microsoft Entra ID, que a
Tracbel já opera com MFA.

```sql
CREATE TABLE seg.Usuario (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Usuario_Chave DEFAULT NEWID(),
    -- Identidade externa: o object id do Entra ID. É a chave real de login.
    EntraObjectId   uniqueidentifier NOT NULL,
    Upn             nvarchar(200)  NOT NULL,  -- ricardo.moretti@tracbel.com.br
    NomeCompleto    nvarchar(200)  NOT NULL,
    NomeExibicao    nvarchar(80)   NOT NULL,
    Email           nvarchar(200)  NOT NULL,
    -- Empresa "de casa" do usuário: define o escopo padrão dele.
    EmpresaId       int            NOT NULL,
    -- Gestor direto. Sustenta a segurança por hierarquia (ver documento 05).
    GestorId        bigint         NULL,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_Usuario_Ativo DEFAULT 1,
    DesativadoEm    datetime2(3)   NULL,
    UltimoLoginEm   datetime2(3)   NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Usuario_CriadoEm DEFAULT SYSUTCDATETIME(),
    AlteradoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Usuario_Entra UNIQUE (EntraObjectId),
    CONSTRAINT UQ_Usuario_Upn UNIQUE (Upn),
    CONSTRAINT FK_Usuario_Empresa FOREIGN KEY (EmpresaId) REFERENCES org.Empresa(Id),
    CONSTRAINT FK_Usuario_Gestor FOREIGN KEY (GestorId) REFERENCES seg.Usuario(Id)
);
CREATE INDEX IX_Usuario_Gestor ON seg.Usuario (GestorId) WHERE EstaAtivo = 1;
```

> **Desativar nunca apaga.** `EstaAtivo = 0` preserva o histórico e as permissões — corrigindo
> diretamente o defeito do Vórtice.

### 2.3 `seg.Equipe` e `seg.EquipeMembro`

`[DYN]` *Owner Team*: uma equipe pode **ser dona** de registros. É a forma barata de dar acesso a um
grupo sem materializar uma linha de compartilhamento por usuário.

```sql
CREATE TABLE seg.Equipe (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Equipe_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    Nome            nvarchar(120)  NOT NULL,
    Descricao       nvarchar(400)  NULL,
    -- 'Proprietaria' pode ser dona de registros; 'Acesso' só concede acesso.
    Tipo            varchar(20)    NOT NULL,
    EstaAtiva       bit            NOT NULL CONSTRAINT DF_Equipe_Ativa DEFAULT 1,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Equipe_CriadoEm DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Equipe PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Equipe_Nome UNIQUE (EmpresaId, Nome),
    CONSTRAINT FK_Equipe_Empresa FOREIGN KEY (EmpresaId) REFERENCES org.Empresa(Id),
    CONSTRAINT CK_Equipe_Tipo CHECK (Tipo IN ('Proprietaria','Acesso'))
);

CREATE TABLE seg.EquipeMembro (
    EquipeId        bigint         NOT NULL,
    UsuarioId       bigint         NOT NULL,
    EhLider         bit            NOT NULL CONSTRAINT DF_EquipeMembro_Lider DEFAULT 0,
    AdicionadoEm    datetime2(3)   NOT NULL CONSTRAINT DF_EquipeMembro_Em DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_EquipeMembro PRIMARY KEY CLUSTERED (EquipeId, UsuarioId),
    CONSTRAINT FK_EquipeMembro_Equipe FOREIGN KEY (EquipeId) REFERENCES seg.Equipe(Id),
    CONSTRAINT FK_EquipeMembro_Usuario FOREIGN KEY (UsuarioId) REFERENCES seg.Usuario(Id)
);
```

### 2.4 `org.HierarquiaVendas` — closure table

`[V]` A hierarquia comercial do Vórtice é **um ponteiro de um nível só** (`IV_VENDEDOR.SeqUsuarioLider`),
sem closure table e sem reatribuição. É por isso que trocar o usuário de um gerente **quebra o relatório
de carteira**: o novo usuário "não lidera ninguém".

Uma *closure table* materializa todos os pares ancestral→descendente. Responder "quem está abaixo de
mim, em qualquer profundidade" vira um `SELECT` simples.

```sql
CREATE TABLE org.HierarquiaVendas (
    AncestralId     bigint         NOT NULL,  -- o gestor (em qualquer nível acima)
    DescendenteId   bigint         NOT NULL,  -- o subordinado
    Profundidade    smallint       NOT NULL,  -- 0 = ele mesmo; 1 = direto; 2 = neto...
    CONSTRAINT PK_HierarquiaVendas PRIMARY KEY CLUSTERED (AncestralId, DescendenteId),
    CONSTRAINT FK_HierVendas_Ancestral   FOREIGN KEY (AncestralId)   REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_HierVendas_Descendente FOREIGN KEY (DescendenteId) REFERENCES seg.Usuario(Id)
);
CREATE INDEX IX_HierVendas_Descendente ON org.HierarquiaVendas (DescendenteId, Profundidade);
```

> **Regra operacional:** a closure é reconstruída pelo serviço sempre que `Usuario.GestorId` muda, dentro
> da mesma transação. Trocar o gestor **reatribui** — o oposto do Vórtice, onde nada se reatribui sozinho.

### 2.5 `org.Carteira` — o ativo conceitual da Tracbel

`[V]` A carteirização multi-linha-de-negócio é **o melhor ativo do modelo antigo** e nem Salesforce nem
Dynamics fazem isso nativamente. Um cliente pertence a **várias carteiras ao mesmo tempo**, uma por linha
de negócio (MAQ, PEÇ, DSI, PNEUS, PUK).

```sql
CREATE TABLE org.LinhaNegocio (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(20)    NOT NULL,  -- 'MAQ', 'PEC', 'DSI', 'PNEUS', 'PUK'
    Nome            nvarchar(80)   NOT NULL,
    EstaAtiva       bit            NOT NULL CONSTRAINT DF_LinhaNegocio_Ativa DEFAULT 1,
    CONSTRAINT PK_LinhaNegocio PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_LinhaNegocio_Codigo UNIQUE (Codigo)
);

CREATE TABLE org.Carteira (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Carteira_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    LinhaNegocioId  int            NOT NULL,
    Codigo          varchar(40)    NOT NULL,  -- 'MAQ_13SJRP_01'
    Nome            nvarchar(120)  NOT NULL,
    -- O CEN (consultor) responsável. No Vórtice isto apontava para IV_VENDEDOR, um cadastro
    -- paralelo ao de usuário -- fonte constante de confusão. Aqui é o usuário, e ponto.
    ResponsavelId   bigint         NOT NULL,
    -- Equipe dona da carteira, quando a carteira é de time e não de pessoa.
    EquipeId        bigint         NULL,
    EstaAtiva       bit            NOT NULL CONSTRAINT DF_Carteira_Ativa DEFAULT 1,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Carteira_CriadoEm DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Carteira PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Carteira_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Carteira_Empresa     FOREIGN KEY (EmpresaId)      REFERENCES org.Empresa(Id),
    CONSTRAINT FK_Carteira_Linha       FOREIGN KEY (LinhaNegocioId) REFERENCES org.LinhaNegocio(Id),
    CONSTRAINT FK_Carteira_Responsavel FOREIGN KEY (ResponsavelId)  REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_Carteira_Equipe      FOREIGN KEY (EquipeId)       REFERENCES seg.Equipe(Id)
);
CREATE INDEX IX_Carteira_Responsavel ON org.Carteira (ResponsavelId) WHERE EstaAtiva = 1;
```

---

## 3. Módulo `crm` — pessoas, contas e contatos

### 3.1 `crm.Conta` — o cadastro mestre

`[SF]` Separamos deliberadamente **Lead** (pré-relacionamento, tabela flat) de **Conta** (pós-relacionamento,
normalizada). `[V]` O Vórtice mistura cliente, prospect, suspect e **falecido** num único `char(1)` —
o domínio real medido é `P`=76.048, `A`=39.624, `S`=1.416, `F`=54, `O`=25, `I`=5, **mais 2.126 linhas
com espaço em branco** que a própria view do fornecedor devolve como `'???'`.

```sql
CREATE TABLE crm.Conta (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Conta_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,

    -- ---- Identificação ----
    RazaoSocial     nvarchar(200)  NOT NULL,
    NomeFantasia    nvarchar(200)  NULL,
    -- 'F' = pessoa física, 'J' = jurídica. CHECK, não convenção.
    TipoPessoa      char(1)        NOT NULL,
    -- Só dígitos, sem máscara. 11 (CPF) ou 14 (CNPJ). Validado na aplicação E aqui.
    CpfCnpj         varchar(14)    NULL,
    InscricaoEstadual varchar(20)  NULL,
    Cnae            varchar(7)     NULL,

    -- ---- Ciclo de vida ----
    -- Substitui o char(1) ambíguo do Vórtice por um domínio fechado e explícito.
    Situacao        varchar(20)    NOT NULL CONSTRAINT DF_Conta_Situacao DEFAULT 'Prospect',
    SituacaoDesde   datetime2(3)   NOT NULL CONSTRAINT DF_Conta_SitDesde DEFAULT SYSUTCDATETIME(),
    MotivoInativacaoId int         NULL,

    -- ---- Propriedade (âncora da segurança por registro -- ver documento 05) ----
    ProprietarioId  bigint         NOT NULL,
    ProprietarioEquipeId bigint    NULL,

    -- ---- Grupo econômico ----
    -- Auto-relacionamento: a matriz do grupo. NULL = não pertence a grupo.
    ContaMatrizId   bigint         NULL,

    -- ---- Origem ----
    -- FK para catálogo. [V] no Vórtice 'Origem' virou texto livre, com duplicatas e lixo de teste.
    OrigemId        int            NULL,

    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Conta_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    AlteradoEm      datetime2(3)   NULL,
    AlteradoPorId   bigint         NULL,
    ExcluidoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,

    CONSTRAINT PK_Conta PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Conta_Empresa      FOREIGN KEY (EmpresaId)       REFERENCES org.Empresa(Id),
    CONSTRAINT FK_Conta_Proprietario FOREIGN KEY (ProprietarioId)  REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_Conta_Equipe       FOREIGN KEY (ProprietarioEquipeId) REFERENCES seg.Equipe(Id),
    CONSTRAINT FK_Conta_Matriz       FOREIGN KEY (ContaMatrizId)   REFERENCES crm.Conta(Id),
    CONSTRAINT CK_Conta_TipoPessoa   CHECK (TipoPessoa IN ('F','J')),
    CONSTRAINT CK_Conta_Situacao     CHECK (Situacao IN
        ('Suspect','Prospect','Cliente','ClienteInativo','Encerrada')),
    -- O documento tem o tamanho certo para o tipo de pessoa.
    CONSTRAINT CK_Conta_CpfCnpj      CHECK (
        CpfCnpj IS NULL
        OR (TipoPessoa = 'F' AND LEN(CpfCnpj) = 11)
        OR (TipoPessoa = 'J' AND LEN(CpfCnpj) = 14))
);

-- Unicidade de documento POR EMPRESA, ignorando excluídos.
-- [V] Medido no Vórtice: 116 CPFs repetidos entre clientes distintos.
CREATE UNIQUE INDEX UX_Conta_CpfCnpj
    ON crm.Conta (EmpresaId, CpfCnpj)
    WHERE CpfCnpj IS NOT NULL AND ExcluidoEm IS NULL;

CREATE INDEX IX_Conta_Proprietario ON crm.Conta (ProprietarioId) WHERE ExcluidoEm IS NULL;
CREATE INDEX IX_Conta_Situacao     ON crm.Conta (EmpresaId, Situacao) WHERE ExcluidoEm IS NULL;
CREATE INDEX IX_Conta_RazaoSocial  ON crm.Conta (RazaoSocial) WHERE ExcluidoEm IS NULL;
```

### 3.2 `crm.Contato` — entidade com identidade própria

`[V]` No Vórtice, `GE_Contato` é **entidade fraca**: PK composta `(SeqPessoa, SeqContato)`, **sem FK e
sem identidade própria** — resultado medido: **625 contatos órfãos**. E um contato que troca de empresa
precisa ser recadastrado.

`[SF]` Aqui o contato existe por si, e o vínculo com a conta é **N:N com papel** (`AccountContactRelation`).

```sql
CREATE TABLE crm.Contato (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Contato_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    Nome            nvarchar(120)  NOT NULL,
    Sobrenome       nvarchar(120)  NULL,
    Cpf             varchar(11)    NULL,
    Cargo           nvarchar(80)   NULL,
    DataNascimento  date           NULL,
    ProprietarioId  bigint         NOT NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Contato_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    AlteradoEm      datetime2(3)   NULL,
    AlteradoPorId   bigint         NULL,
    ExcluidoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,
    CONSTRAINT PK_Contato PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Contato_Empresa      FOREIGN KEY (EmpresaId)      REFERENCES org.Empresa(Id),
    CONSTRAINT FK_Contato_Proprietario FOREIGN KEY (ProprietarioId) REFERENCES seg.Usuario(Id)
);

CREATE TABLE crm.ContaContato (
    ContaId         bigint         NOT NULL,
    ContatoId       bigint         NOT NULL,
    PapelId         int            NOT NULL,   -- 'Comprador', 'Operador', 'Financeiro', 'Decisor'
    EhPrincipal     bit            NOT NULL CONSTRAINT DF_ContaContato_Principal DEFAULT 0,
    IniciouEm       date           NULL,
    EncerrouEm      date           NULL,       -- NULL = vínculo vigente
    CONSTRAINT PK_ContaContato PRIMARY KEY CLUSTERED (ContaId, ContatoId, PapelId),
    CONSTRAINT FK_ContaContato_Conta   FOREIGN KEY (ContaId)   REFERENCES crm.Conta(Id),
    CONSTRAINT FK_ContaContato_Contato FOREIGN KEY (ContatoId) REFERENCES crm.Contato(Id),
    CONSTRAINT FK_ContaContato_Papel   FOREIGN KEY (PapelId)   REFERENCES crm.PapelContato(Id)
);
-- No máximo um contato principal por conta.
CREATE UNIQUE INDEX UX_ContaContato_Principal
    ON crm.ContaContato (ContaId) WHERE EhPrincipal = 1;
```

### 3.3 Canais de contato — um modelo, não três

`[V]` O Vórtice tem **três modelos concorrentes de e-mail e três de telefone** dentro do mesmo banco:
`GE_Pessoa.Email`, `GE_Email`, `GE_PessoaEmail`; e `FoneDDD1..3`+`FoneNro1..3`, `GE_PessoaFone`, e mais.
Ninguém sabe qual é a verdade.

Aqui existe **um único** modelo, polimórfico entre Conta e Contato.

```sql
CREATE TABLE crm.CanalContato (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    EmpresaId       int            NOT NULL,
    -- Polimorfismo controlado: exatamente UM dos dois é preenchido (ver CHECK).
    ContaId         bigint         NULL,
    ContatoId       bigint         NULL,
    Tipo            varchar(20)    NOT NULL,  -- 'Email','Telefone','Celular','WhatsApp'
    -- Telefone é varchar. NUNCA decimal. [V] GE_Pessoa.FoneNro1 é decimal(12) e estoura com DDI.
    Valor           varchar(200)   NOT NULL,
    -- Só dígitos, para busca e deduplicação. Preenchido pela aplicação na gravação.
    ValorNormalizado varchar(200)  NOT NULL,
    Rotulo          nvarchar(40)   NULL,      -- 'Comercial', 'Fazenda', 'Pessoal'
    EhPrincipal     bit            NOT NULL CONSTRAINT DF_Canal_Principal DEFAULT 0,
    EhValido        bit            NOT NULL CONSTRAINT DF_Canal_Valido DEFAULT 1,
    ValidadoEm      datetime2(3)   NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Canal_CriadoEm DEFAULT SYSUTCDATETIME(),
    ExcluidoEm      datetime2(3)   NULL,
    CONSTRAINT PK_CanalContato PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Canal_Conta   FOREIGN KEY (ContaId)   REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Canal_Contato FOREIGN KEY (ContatoId) REFERENCES crm.Contato(Id),
    CONSTRAINT CK_Canal_Tipo CHECK (Tipo IN ('Email','Telefone','Celular','WhatsApp')),
    -- Exatamente um dono. Isto é o que impede o registro órfão.
    CONSTRAINT CK_Canal_UmDono CHECK (
        (ContaId IS NOT NULL AND ContatoId IS NULL)
     OR (ContaId IS NULL AND ContatoId IS NOT NULL))
);
CREATE INDEX IX_Canal_Normalizado ON crm.CanalContato (ValorNormalizado) WHERE ExcluidoEm IS NULL;
```

### 3.4 `crm.ConsentimentoComunicacao` — LGPD com prova

`[V]` **Dois defeitos graves de LGPD medidos:** (a) as views de anonimização republicam o dado cru na
**mesma view**, em colunas `z_NOMERAZAO`, `z_NROCGCCPF`, `z_EMAIL` — a anonimização é contornável com um
`SELECT`; (b) o opt-in **não tem data, não tem origem e não tem prova** — apenas um flag, o que não
sustenta uma solicitação de titular.

```sql
CREATE TABLE crm.ConsentimentoComunicacao (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    EmpresaId       int            NOT NULL,
    ContaId         bigint         NULL,
    ContatoId       bigint         NULL,
    Canal           varchar(20)    NOT NULL,  -- 'Email','Sms','WhatsApp','Telefone','Correspondencia'
    Finalidade      varchar(40)    NOT NULL,  -- 'Marketing','Transacional','Pesquisa','Cobranca'
    -- A decisão do titular.
    Concedido       bit            NOT NULL,
    -- A PROVA. Sem estes três campos não há consentimento defensável.
    DecididoEm      datetime2(3)   NOT NULL,
    OrigemEvidencia varchar(60)    NOT NULL,  -- 'FormularioSite','RDStation','Telefone','Contrato'
    EvidenciaRef    nvarchar(400)  NULL,      -- URL, id da submissão, caminho do documento assinado
    EnderecoIp      varchar(45)    NULL,      -- IPv4 ou IPv6
    -- Revogação: nunca se apaga o consentimento, cria-se um novo registro negando.
    RegistradoPorId bigint         NULL,      -- NULL = ação do próprio titular
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Consent_CriadoEm DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Consentimento PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Consent_Conta   FOREIGN KEY (ContaId)   REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Consent_Contato FOREIGN KEY (ContatoId) REFERENCES crm.Contato(Id),
    CONSTRAINT CK_Consent_UmDono CHECK (
        (ContaId IS NOT NULL AND ContatoId IS NULL)
     OR (ContaId IS NULL AND ContatoId IS NOT NULL))
);
-- A consulta quente: "posso mandar e-mail de marketing para este contato?"
CREATE INDEX IX_Consent_Consulta
    ON crm.ConsentimentoComunicacao (ContatoId, Canal, Finalidade, DecididoEm DESC);
```

> **Regra:** a tabela é **append-only**. Revogar é inserir uma linha com `Concedido = 0`. A resposta
> vigente é sempre a linha mais recente para o trio (titular, canal, finalidade). Isso dá a trilha
> completa que a LGPD exige.

### 3.5 `crm.ContaCarteira` — a carteirização

```sql
CREATE TABLE crm.ContaCarteira (
    ContaId         bigint         NOT NULL,
    CarteiraId      bigint         NOT NULL,
    -- Potencial e cadência: [V] o único mecanismo de segmentação que REALMENTE funciona no
    -- Vórtice (IVS_DEPTOPOT, 109.118 linhas preenchidas). O RFV de IVS_Pes está 100% morto.
    PotencialAnual  decimal(18,2)  NULL,
    DiasCicloContato smallint      NULL,   -- cadência esperada de contato
    UltimoContatoEm  datetime2(3)  NULL,
    VinculadoEm     datetime2(3)   NOT NULL CONSTRAINT DF_ContaCart_Em DEFAULT SYSUTCDATETIME(),
    VinculadoPorId  bigint         NOT NULL,
    CONSTRAINT PK_ContaCarteira PRIMARY KEY CLUSTERED (ContaId, CarteiraId),
    CONSTRAINT FK_ContaCart_Conta    FOREIGN KEY (ContaId)    REFERENCES crm.Conta(Id),
    CONSTRAINT FK_ContaCart_Carteira FOREIGN KEY (CarteiraId) REFERENCES org.Carteira(Id)
);
CREATE INDEX IX_ContaCarteira_Carteira ON crm.ContaCarteira (CarteiraId);
```

---

## 4. Módulo `crm` — Lead (fase 1)

`[SF]` Lead é uma tabela **flat e autossuficiente**: nome, empresa e contato no mesmo registro, porque
antes da qualificação não se sabe se há conta, contato ou oportunidade.

`[DYN]` Na qualificação, **o lead sobrevive** (`statecode` muda, não some) e os registros criados apontam
de volta por `originatingleadid`. Adotamos o modelo do Dynamics: é reversível na leitura e rastreável.

```sql
CREATE TABLE crm.Lead (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Lead_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,

    -- ---- Dados crus, como chegaram ----
    NomeContato     nvarchar(200)  NOT NULL,
    NomeEmpresa     nvarchar(200)  NULL,
    Email           nvarchar(200)  NULL,
    Telefone        varchar(20)    NULL,
    CpfCnpj         varchar(14)    NULL,
    Cidade          nvarchar(120)  NULL,
    Uf              char(2)        NULL,
    Cargo           nvarchar(80)   NULL,
    Interesse       nvarchar(400)  NULL,   -- o que o lead disse que quer
    LinhaNegocioId  int            NULL,

    -- ---- Origem ----
    OrigemId        int            NOT NULL,
    CampanhaId      bigint         NULL,
    -- Identificador da conversão no RD Station. [V] o defeito 2.9 nasce de o campo só ser
    -- preenchido quando o identificador está mapeado numa TELA. Aqui guardamos o cru SEMPRE.
    IdentificadorConversao nvarchar(200) NULL,
    PayloadOriginal nvarchar(max)  NULL,   -- JSON recebido, íntegro, para reprocessar

    -- ---- Qualificação ----
    Situacao        varchar(20)    NOT NULL CONSTRAINT DF_Lead_Situacao DEFAULT 'Novo',
    Pontuacao       smallint       NULL,   -- score de qualificação
    QualificadoEm   datetime2(3)   NULL,
    QualificadoPorId bigint        NULL,
    MotivoDescarteId int           NULL,
    -- Para onde o lead virou. Preenchidos na qualificação; o lead NÃO é apagado. [DYN]
    ContaGeradaId   bigint         NULL,
    ContatoGeradoId bigint         NULL,
    ProcessoGeradoId bigint        NULL,

    ProprietarioId  bigint         NOT NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Lead_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    AlteradoEm      datetime2(3)   NULL,
    AlteradoPorId   bigint         NULL,
    ExcluidoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,

    CONSTRAINT PK_Lead PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Lead_Empresa      FOREIGN KEY (EmpresaId)       REFERENCES org.Empresa(Id),
    CONSTRAINT FK_Lead_Origem       FOREIGN KEY (OrigemId)        REFERENCES crm.OrigemLead(Id),
    CONSTRAINT FK_Lead_Proprietario FOREIGN KEY (ProprietarioId)  REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_Lead_ContaGerada  FOREIGN KEY (ContaGeradaId)   REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Lead_ContatoGerado FOREIGN KEY (ContatoGeradoId) REFERENCES crm.Contato(Id),
    CONSTRAINT CK_Lead_Situacao CHECK (Situacao IN
        ('Novo','EmContato','Qualificado','Descartado','Duplicado')),
    -- Se qualificou, tem que ter data, quem e o que gerou. Regra no banco, não só no código.
    CONSTRAINT CK_Lead_Qualificacao CHECK (
        Situacao <> 'Qualificado'
        OR (QualificadoEm IS NOT NULL AND QualificadoPorId IS NOT NULL AND ContaGeradaId IS NOT NULL))
);
CREATE INDEX IX_Lead_Situacao     ON crm.Lead (EmpresaId, Situacao, CriadoEm DESC) WHERE ExcluidoEm IS NULL;
CREATE INDEX IX_Lead_Proprietario ON crm.Lead (ProprietarioId, Situacao) WHERE ExcluidoEm IS NULL;
CREATE INDEX IX_Lead_Email        ON crm.Lead (Email) WHERE Email IS NOT NULL AND ExcluidoEm IS NULL;
CREATE INDEX IX_Lead_CpfCnpj      ON crm.Lead (CpfCnpj) WHERE CpfCnpj IS NOT NULL AND ExcluidoEm IS NULL;
```

> `[V]` **A ação 897 tem limite vitalício de 1 por pessoa** — reconversão de lead conhecido nunca vira
> tarefa nova. Aqui não existe limite vitalício: a política de retrabalho de lead conhecido é uma
> **regra com condição** (ver seção 5.4), com janela temporal configurável.

---

## 5. Módulo `wf` — processo e motor de workflow

Este é o coração. Cinco entidades copiadas do Vórtice (o acerto dele), corrigidas em três pontos:
condição na regra, log de execução obrigatório e caminho percorrido.

### 5.1 Catálogos

```sql
-- O tipo de fluxo. [V] equivale a IV_CodProcesso ("CodProcesso" -- nome péssimo, porque
-- parecia ser o número do processo e não é; aqui o nome diz o que é).
CREATE TABLE wf.TipoProcesso (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(40)    NOT NULL,   -- 'VENDA_EQUIPAMENTO', 'DEMONSTRACAO'
    Nome            nvarchar(120)  NOT NULL,
    LinhaNegocioId  int            NULL,
    -- Versão da definição do fluxo. Trocar o fluxo cria versão nova; processos em andamento
    -- continuam na versão em que nasceram. [V] no Vórtice, trocar o tipo REESCREVE o histórico.
    Versao          int            NOT NULL CONSTRAINT DF_TipoProcesso_Versao DEFAULT 1,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_TipoProcesso_Ativo DEFAULT 1,
    CONSTRAINT PK_TipoProcesso PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_TipoProcesso UNIQUE (Codigo, Versao)
);

-- O estágio do fluxo (a barra que o usuário vê no topo). [DYN] Business Process Flow.
CREATE TABLE wf.Estagio (
    Id              int            IDENTITY(1,1) NOT NULL,
    TipoProcessoId  int            NOT NULL,
    Codigo          varchar(40)    NOT NULL,   -- 'QUALIFICACAO','NEGOCIACAO','APROVACAO'
    Nome            nvarchar(80)   NOT NULL,
    Ordem           smallint       NOT NULL,
    -- Marco do funil, para relatório. [V] equivale à coluna Marco de IV_ProcFase.
    Marco           nvarchar(60)   NULL,
    -- Se 1, o processo não avança enquanto os campos obrigatórios do estágio não estiverem
    -- preenchidos. [DYN] "stage-gating".
    ExigeCamposObrigatorios bit    NOT NULL CONSTRAINT DF_Estagio_Gate DEFAULT 1,
    EhFinal         bit            NOT NULL CONSTRAINT DF_Estagio_Final DEFAULT 0,
    CONSTRAINT PK_Estagio PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Estagio UNIQUE (TipoProcessoId, Codigo),
    CONSTRAINT FK_Estagio_Tipo FOREIGN KEY (TipoProcessoId) REFERENCES wf.TipoProcesso(Id)
);

-- O tipo de tarefa. [V] equivale a IV_Acao (980 linhas, só 169 usadas em 2026).
CREATE TABLE wf.TipoTarefa (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(40)    NOT NULL,
    Nome            nvarchar(120)  NOT NULL,
    TipoProcessoId  int            NULL,       -- NULL = vale para qualquer fluxo
    PrazoDiasUteis  smallint       NOT NULL CONSTRAINT DF_TipoTarefa_Prazo DEFAULT 1,
    EhAprovacao     bit            NOT NULL CONSTRAINT DF_TipoTarefa_Aprov DEFAULT 0,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_TipoTarefa_Ativo DEFAULT 1,
    -- Data em que o catálogo foi usado pela última vez. Alimenta a rotina de higienização.
    -- [V] 980 ações e 4.208 resultados apodrecendo sem ninguém saber quais estão vivos.
    UltimoUsoEm     datetime2(3)   NULL,
    CONSTRAINT PK_TipoTarefa PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_TipoTarefa UNIQUE (Codigo),
    CONSTRAINT FK_TipoTarefa_Tipo FOREIGN KEY (TipoProcessoId) REFERENCES wf.TipoProcesso(Id)
);

-- O desfecho. [V] equivale a IV_Resultado (63 colunas, 35 prefixadas CTRL*, domínio 0/1/9
-- não documentado). Aqui são 8 colunas e todas têm significado.
CREATE TABLE wf.Desfecho (
    Id              int            IDENTITY(1,1) NOT NULL,
    TipoTarefaId    int            NOT NULL,
    Codigo          varchar(40)    NOT NULL,
    Nome            nvarchar(120)  NOT NULL,
    -- Classificação para o funil: 'Avanco','Manutencao','Perda','Cancelamento'.
    Classe          varchar(20)    NOT NULL,
    ExigeJustificativa bit         NOT NULL CONSTRAINT DF_Desfecho_Justif DEFAULT 0,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_Desfecho_Ativo DEFAULT 1,
    UltimoUsoEm     datetime2(3)   NULL,
    CONSTRAINT PK_Desfecho PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Desfecho UNIQUE (TipoTarefaId, Codigo),
    CONSTRAINT FK_Desfecho_TipoTarefa FOREIGN KEY (TipoTarefaId) REFERENCES wf.TipoTarefa(Id),
    CONSTRAINT CK_Desfecho_Classe CHECK (Classe IN ('Avanco','Manutencao','Perda','Cancelamento'))
);
```

### 5.2 `wf.Processo` — o caso

```sql
CREATE TABLE wf.Processo (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Processo_Chave DEFAULT NEWID(),
    -- Número legível pelo humano, sequencial por empresa. É o que o usuário fala ao telefone.
    Numero          bigint         NOT NULL,
    EmpresaId       int            NOT NULL,
    TipoProcessoId  int            NOT NULL,

    -- [V] IV_Processo NÃO tem SeqPessoa -- o vínculo com a pessoa é indireto e gerou 345.535
    -- órfãos. Aqui a conta é obrigatória e tem FK.
    ContaId         bigint         NOT NULL,
    ContatoId       bigint         NULL,
    CarteiraId      bigint         NULL,

    Titulo          nvarchar(200)  NOT NULL,
    Descricao       nvarchar(max)  NULL,

    -- ---- Estado ----
    EstagioId       int            NOT NULL,
    EstagioDesde    datetime2(3)   NOT NULL CONSTRAINT DF_Processo_EstDesde DEFAULT SYSUTCDATETIME(),
    Situacao        varchar(20)    NOT NULL CONSTRAINT DF_Processo_Situacao DEFAULT 'Aberto',
    SituacaoDesde   datetime2(3)   NOT NULL CONSTRAINT DF_Processo_SitDesde DEFAULT SYSUTCDATETIME(),
    MotivoEncerramentoId int       NULL,

    -- ---- Valor ----
    ValorEstimado   decimal(18,2)  NULL,
    ValorFinal      decimal(18,2)  NULL,
    Quantidade      decimal(12,3)  NULL,

    -- ---- Prazo: atual E baseline ----
    -- [V] o Vórtice acerta aqui (DtaPrevConclusao vs DtaPrevConcOrig) e copiamos: sem a
    -- baseline não dá para medir derrapagem.
    PrevisaoConclusao      date    NULL,
    PrevisaoConclusaoOriginal date NULL,
    ConcluidoEm     datetime2(3)   NULL,

    ProprietarioId  bigint         NOT NULL,
    ProprietarioEquipeId bigint    NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Processo_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    AlteradoEm      datetime2(3)   NULL,
    AlteradoPorId   bigint         NULL,
    ExcluidoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,

    CONSTRAINT PK_Processo PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Processo_Numero UNIQUE (EmpresaId, Numero),
    CONSTRAINT FK_Processo_Empresa FOREIGN KEY (EmpresaId)      REFERENCES org.Empresa(Id),
    CONSTRAINT FK_Processo_Tipo    FOREIGN KEY (TipoProcessoId) REFERENCES wf.TipoProcesso(Id),
    CONSTRAINT FK_Processo_Conta   FOREIGN KEY (ContaId)        REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Processo_Contato FOREIGN KEY (ContatoId)      REFERENCES crm.Contato(Id),
    CONSTRAINT FK_Processo_Estagio FOREIGN KEY (EstagioId)      REFERENCES wf.Estagio(Id),
    CONSTRAINT FK_Processo_Carteira FOREIGN KEY (CarteiraId)    REFERENCES org.Carteira(Id),
    CONSTRAINT FK_Processo_Proprietario FOREIGN KEY (ProprietarioId) REFERENCES seg.Usuario(Id),
    CONSTRAINT CK_Processo_Situacao CHECK (Situacao IN
        ('Aberto','Suspenso','Ganho','Perdido','Cancelado')),
    -- Encerrou? Então tem data e motivo. [V] "Atividade Cancelada" cancelava o processo sem
    -- registrar motivo nem permitir desfazer.
    CONSTRAINT CK_Processo_Encerramento CHECK (
        Situacao IN ('Aberto','Suspenso')
        OR (ConcluidoEm IS NOT NULL AND MotivoEncerramentoId IS NOT NULL))
);
CREATE INDEX IX_Processo_Conta   ON wf.Processo (ContaId) WHERE ExcluidoEm IS NULL;
CREATE INDEX IX_Processo_Aberto  ON wf.Processo (EmpresaId, EstagioId)
    WHERE Situacao = 'Aberto' AND ExcluidoEm IS NULL;
CREATE INDEX IX_Processo_Proprietario ON wf.Processo (ProprietarioId, Situacao) WHERE ExcluidoEm IS NULL;
```

### 5.3 `wf.ProcessoEstagioTrilha` — o caminho percorrido

`[DYN]` A ideia mais valiosa do Business Process Flow: a barra de estágios tem **tabela de instância
própria** que grava o caminho percorrido (`traversedpath`), não só o ponto atual. Isso torna a jornada
consultável, reportável e auditável.

`[V]` No Vórtice, `Fase` e `Status` são duas colunas soltas em `IV_Processo`. Quando o resultado 3438
("Atividade Cancelada") gravou `Fase=Finalizado, Status=CANCELADO`, **não havia como saber de onde o
processo veio nem como voltar** — e não existe desfazer.

```sql
CREATE TABLE wf.ProcessoEstagioTrilha (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ProcessoId      bigint         NOT NULL,
    EstagioId       int            NOT NULL,
    -- Sequência de entrada. Reentrar num estágio gera nova linha com ordem maior:
    -- o caminho de ida e volta fica registrado.
    Ordem           int            NOT NULL,
    EntrouEm        datetime2(3)   NOT NULL,
    SaiuEm          datetime2(3)   NULL,      -- NULL = é o estágio atual
    -- Duração em horas úteis, calculada na saída. Alimenta o SLA por estágio.
    HorasUteis      decimal(10,2)  NULL,
    -- O que provocou a entrada neste estágio: qual atividade, qual desfecho, qual regra.
    -- É esta rastreabilidade que faltou no Vórtice.
    AtividadeOrigemId bigint       NULL,
    RegraOrigemId   int            NULL,
    EntrouPorId     bigint         NOT NULL,
    CONSTRAINT PK_ProcEstagioTrilha PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ProcEstagioTrilha UNIQUE (ProcessoId, Ordem),
    CONSTRAINT FK_PET_Processo FOREIGN KEY (ProcessoId) REFERENCES wf.Processo(Id),
    CONSTRAINT FK_PET_Estagio  FOREIGN KEY (EstagioId)  REFERENCES wf.Estagio(Id)
);
CREATE INDEX IX_PET_Atual ON wf.ProcessoEstagioTrilha (ProcessoId) WHERE SaiuEm IS NULL;
```

### 5.4 `wf.Tarefa` — a agenda do usuário

```sql
CREATE TABLE wf.Tarefa (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Tarefa_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    ProcessoId      bigint         NULL,      -- NULL = tarefa avulsa, sem processo
    ContaId         bigint         NULL,
    ContatoId       bigint         NULL,
    TipoTarefaId    int            NOT NULL,

    Assunto         nvarchar(200)  NOT NULL,
    Detalhe         nvarchar(max)  NULL,

    -- ---- Atribuição ----
    -- [V] IV_Agenda.Vendedor guarda o LOGIN (varchar), não o ID. Aqui é FK, sempre.
    ResponsavelId   bigint         NOT NULL,
    ResponsavelEquipeId bigint     NULL,
    -- Como a atribuição foi decidida: 'Regra','Manual','Hierarquia','Carteira','RoundRobin'.
    -- Responde "por que essa tarefa é minha?" sem investigação. [V] caso B do suporte.
    OrigemAtribuicao varchar(20)   NOT NULL,

    -- ---- Prazo ----
    AgendadaPara    datetime2(3)   NOT NULL,
    PrazoLimite     datetime2(3)   NULL,
    Prioridade      tinyint        NOT NULL CONSTRAINT DF_Tarefa_Prioridade DEFAULT 3,  -- 1=alta

    -- ---- Execução ----
    Situacao        varchar(20)    NOT NULL CONSTRAINT DF_Tarefa_Situacao DEFAULT 'Pendente',
    ConcluidaEm     datetime2(3)   NULL,
    ConcluidaPorId  bigint         NULL,
    DesfechoId      int            NULL,
    -- A atividade que registrou a conclusão. Junto com Atividade.TarefaId forma o duplo
    -- ponteiro que o Vórtice acertou (Agenda.HistoricoOrigem / Historico.AgendaOrigem).
    AtividadeConclusaoId bigint    NULL,

    -- ---- Rastreabilidade da criação ----
    CriadaPorRegraId int           NULL,      -- qual regra gerou; NULL = criada à mão
    AtividadeOrigemId bigint       NULL,      -- qual andamento disparou a regra

    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Tarefa_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    AlteradoEm      datetime2(3)   NULL,
    ExcluidoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,

    CONSTRAINT PK_Tarefa PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Tarefa_Processo    FOREIGN KEY (ProcessoId)     REFERENCES wf.Processo(Id),
    CONSTRAINT FK_Tarefa_Conta       FOREIGN KEY (ContaId)        REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Tarefa_TipoTarefa  FOREIGN KEY (TipoTarefaId)   REFERENCES wf.TipoTarefa(Id),
    CONSTRAINT FK_Tarefa_Responsavel FOREIGN KEY (ResponsavelId)  REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_Tarefa_Desfecho    FOREIGN KEY (DesfechoId)     REFERENCES wf.Desfecho(Id),
    CONSTRAINT CK_Tarefa_Situacao CHECK (Situacao IN
        ('Pendente','EmAndamento','Concluida','Cancelada','Reatribuida')),
    CONSTRAINT CK_Tarefa_OrigemAtrib CHECK (OrigemAtribuicao IN
        ('Regra','Manual','Hierarquia','Carteira','RoundRobin')),
    -- Concluiu? Tem data, quem e desfecho. Sem exceção.
    CONSTRAINT CK_Tarefa_Conclusao CHECK (
        Situacao <> 'Concluida'
        OR (ConcluidaEm IS NOT NULL AND ConcluidaPorId IS NOT NULL AND DesfechoId IS NOT NULL))
);
-- O índice que sustenta a tela principal do usuário ("minhas tarefas pendentes").
CREATE INDEX IX_Tarefa_MinhaAgenda
    ON wf.Tarefa (ResponsavelId, AgendadaPara)
    INCLUDE (Assunto, Prioridade, ProcessoId, ContaId)
    WHERE Situacao IN ('Pendente','EmAndamento') AND ExcluidoEm IS NULL;
CREATE INDEX IX_Tarefa_Processo ON wf.Tarefa (ProcessoId) WHERE ExcluidoEm IS NULL;
```

### 5.5 `wf.Atividade` — o fato imutável

`[DYN]` `ActivityPointer` é **herança de tabela real**: a tabela base guarda toda atividade; a específica
guarda só o extra. Adotamos a versão simplificada — uma tabela base com discriminador — porque com 3
devs a herança física de tabela custa mais do que entrega.

```sql
CREATE TABLE wf.Atividade (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Atividade_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    -- Discriminador: 'Andamento','Ligacao','Email','Visita','Reuniao','Nota','Sistema'.
    Tipo            varchar(20)    NOT NULL,

    ProcessoId      bigint         NULL,
    ContaId         bigint         NULL,
    ContatoId       bigint         NULL,
    LeadId          bigint         NULL,
    -- Duplo ponteiro com Tarefa (ver 5.4).
    TarefaId        bigint         NULL,

    Assunto         nvarchar(200)  NOT NULL,
    Detalhe         nvarchar(max)  NULL,
    DesfechoId      int            NULL,
    -- Complemento livre do desfecho. varchar(200) E grava 200 -- [V] no Vórtice a coluna
    -- aceita 150 e a aplicação trunca em 20, silenciosamente.
    DesfechoComplemento nvarchar(200) NULL,

    -- 'Ativo' = nós procuramos o cliente; 'Receptivo' = o cliente nos procurou. [V] acerto do
    -- Vórtice, medido: 1.428.832 ativos contra 1.054.975 receptivos.
    Natureza        varchar(12)    NOT NULL CONSTRAINT DF_Atividade_Natureza DEFAULT 'Ativo',

    OcorreuEm       datetime2(3)   NOT NULL,
    DuracaoMinutos  int            NULL,
    -- Geo do atendimento em campo. [V] acerto do Vórtice, usado de verdade na operação agrícola.
    Latitude        decimal(10,7)  NULL,
    Longitude       decimal(10,7)  NULL,

    RegistradoPorId bigint         NOT NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Atividade_CriadoEm DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Atividade PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Atividade_Processo FOREIGN KEY (ProcessoId) REFERENCES wf.Processo(Id),
    CONSTRAINT FK_Atividade_Conta    FOREIGN KEY (ContaId)    REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Atividade_Contato  FOREIGN KEY (ContatoId)  REFERENCES crm.Contato(Id),
    CONSTRAINT FK_Atividade_Lead     FOREIGN KEY (LeadId)     REFERENCES crm.Lead(Id),
    CONSTRAINT FK_Atividade_Tarefa   FOREIGN KEY (TarefaId)   REFERENCES wf.Tarefa(Id),
    CONSTRAINT FK_Atividade_Desfecho FOREIGN KEY (DesfechoId) REFERENCES wf.Desfecho(Id),
    CONSTRAINT FK_Atividade_Usuario  FOREIGN KEY (RegistradoPorId) REFERENCES seg.Usuario(Id),
    CONSTRAINT CK_Atividade_Natureza CHECK (Natureza IN ('Ativo','Receptivo','Sistema')),
    -- Toda atividade se liga a ALGUMA coisa. Nunca solta.
    CONSTRAINT CK_Atividade_TemVinculo CHECK (
        ProcessoId IS NOT NULL OR ContaId IS NOT NULL
        OR ContatoId IS NOT NULL OR LeadId IS NOT NULL)
);
CREATE INDEX IX_Atividade_Processo ON wf.Atividade (ProcessoId, OcorreuEm DESC);
CREATE INDEX IX_Atividade_Conta    ON wf.Atividade (ContaId, OcorreuEm DESC);
```

> **Imutabilidade:** `wf.Atividade` **não tem `AlteradoEm` nem `ExcluidoEm`** — é *append-only* por
> desenho. Corrigir um lançamento é registrar uma atividade de estorno. A permissão de `UPDATE` e
> `DELETE` é negada no banco para a role da aplicação.

### 5.6 `wf.AtividadeParticipante`

`[DYN]` `ActivityParty` com tipos de participação. Resolve N:N tipado e polimórfico de uma vez — quem
foi o remetente, quem foi destinatário, quem estava na visita.

```sql
CREATE TABLE wf.AtividadeParticipante (
    AtividadeId     bigint         NOT NULL,
    -- Papel: 'Autor','Destinatario','Copia','Participante','Cliente'.
    Papel           varchar(20)    NOT NULL,
    UsuarioId       bigint         NULL,
    ContatoId       bigint         NULL,
    -- Para participante externo que não está no cadastro.
    NomeExterno     nvarchar(200)  NULL,
    CONSTRAINT PK_AtivParticipante PRIMARY KEY CLUSTERED (AtividadeId, Papel, UsuarioId, ContatoId),
    CONSTRAINT FK_AP_Atividade FOREIGN KEY (AtividadeId) REFERENCES wf.Atividade(Id),
    CONSTRAINT FK_AP_Usuario   FOREIGN KEY (UsuarioId)   REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_AP_Contato   FOREIGN KEY (ContatoId)   REFERENCES crm.Contato(Id)
);
```

### 5.7 `wf.Regra` — a automação **com condição**

Aqui está a correção mais importante do projeto.

`[V]` `IV_AcaoAuto` tem 31 colunas, das quais **9 estão 100% mortas**, e **não tem linguagem de
condição**: `IV_AcaoAutoCtrl` está **vazia** e `UsaObjDyn = 0` em 100% das linhas. Sem condição, a única
saída é replicar a regra por filial: **5.958 regras para 1.262 pares (Resultado, Ação) distintos**.

Com uma expressão de condição, essas 5.958 regras cabem em pouco mais de mil.

```sql
CREATE TABLE wf.Regra (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(60)    NOT NULL,
    Nome            nvarchar(200)  NOT NULL,
    Descricao       nvarchar(1000) NULL,   -- em português, para quem for manter

    -- ---- GATILHO ----
    -- Evento que dispara: 'TarefaConcluida','ProcessoCriado','EstagioAlterado','LeadRecebido'...
    Evento          varchar(40)    NOT NULL,
    TipoProcessoId  int            NULL,   -- NULL = qualquer fluxo
    TipoTarefaId    int            NULL,
    DesfechoId      int            NULL,

    -- ---- CONDIÇÃO (o que faltava no Vórtice) ----
    -- Expressão booleana avaliada em runtime contra o contexto do evento.
    -- Ex.: "processo.ValorEstimado > 500000 && conta.Situacao == 'Cliente'"
    -- NULL = sem condição (sempre verdadeira). Validada na gravação: expressão inválida
    -- não entra na tabela.
    Condicao        nvarchar(2000) NULL,

    -- ---- EFEITO ----
    -- 'CriarTarefa','MoverEstagio','EncerrarProcesso','Notificar','ChamarWebhook'.
    Efeito          varchar(40)    NOT NULL,
    EfeitoTipoTarefaId int         NULL,   -- se CriarTarefa
    EfeitoEstagioId int            NULL,   -- se MoverEstagio
    EfeitoParametros nvarchar(max) NULL,   -- JSON com o resto

    -- ---- DESTINATÁRIO (expressão, não ID fixo) ----
    -- [V] IV_AcaoAuto.SeqUsuario guarda um ID FIXO -- por isso a regra 9473 manda 150 tarefas
    -- para o mesmo usuário desde abril, e por isso a aprovação vai para o líder ERRADO quando
    -- o líder muda. Aqui é uma expressão RESOLVIDA NA HORA.
    -- Ex.: 'processo.Proprietario', 'carteira.Responsavel', 'gestorDe(processo.Proprietario)',
    --      'equipe:APROVACAO_MAQ', 'quemExecutou'
    ExpressaoDestinatario nvarchar(400) NOT NULL,

    Ordem           smallint       NOT NULL CONSTRAINT DF_Regra_Ordem DEFAULT 100,
    -- Desligar sem apagar. [V] EMUSO existe no Vórtice e é um acerto -- copiado.
    EstaAtiva       bit            NOT NULL CONSTRAINT DF_Regra_Ativa DEFAULT 1,
    -- Se 1, falha na regra aborta a transação. Se 0, registra e segue.
    EhCritica       bit            NOT NULL CONSTRAINT DF_Regra_Critica DEFAULT 0,

    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Regra_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    AlteradoEm      datetime2(3)   NULL,
    AlteradoPorId   bigint         NULL,

    CONSTRAINT PK_Regra PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Regra_Codigo UNIQUE (Codigo),
    CONSTRAINT FK_Regra_TipoProcesso FOREIGN KEY (TipoProcessoId) REFERENCES wf.TipoProcesso(Id),
    CONSTRAINT FK_Regra_TipoTarefa   FOREIGN KEY (TipoTarefaId)   REFERENCES wf.TipoTarefa(Id),
    CONSTRAINT FK_Regra_Desfecho     FOREIGN KEY (DesfechoId)     REFERENCES wf.Desfecho(Id),
    CONSTRAINT CK_Regra_Efeito CHECK (Efeito IN
        ('CriarTarefa','MoverEstagio','EncerrarProcesso','Notificar','ChamarWebhook','AtribuirCarteira'))
);
CREATE INDEX IX_Regra_Gatilho ON wf.Regra (Evento, DesfechoId, Ordem) WHERE EstaAtiva = 1;
```

### 5.8 `wf.RegraExecucao` — o log que torna o silêncio impossível

**Esta tabela é a razão de o defeito 899/900 não poder acontecer de novo.**

`[V]` No Vórtice, a taxa de geração da ação 900 caiu de **68% em março para 3% em agosto de 2026** e
**ninguém percebeu**, porque uma regra que não dispara não deixa rastro. Aqui, toda avaliação é gravada —
inclusive (e principalmente) as que **não** dispararam.

```sql
CREATE TABLE wf.RegraExecucao (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    RegraId         int            NOT NULL,
    -- Correlação: agrupa todas as regras avaliadas no mesmo evento.
    CorrelacaoId    uniqueidentifier NOT NULL,
    Evento          varchar(40)    NOT NULL,
    ProcessoId      bigint         NULL,
    TarefaId        bigint         NULL,
    AtividadeId     bigint         NULL,

    -- 'Disparou','CondicaoFalsa','RegraInativa','SemDestinatario','Erro','Suprimida'
    Resultado       varchar(20)    NOT NULL,
    -- Em português, legível: "Condição falsa: ValorEstimado (120000) não é > 500000".
    -- É isto que o suporte lê. Sem isto, voltamos ao Vórtice.
    Motivo          nvarchar(1000) NULL,
    -- O que a regra produziu, quando disparou.
    TarefaCriadaId  bigint         NULL,
    EstagioDestinoId int           NULL,
    DestinatarioResolvidoId bigint NULL,

    DuracaoMs       int            NOT NULL,
    ExecutadoEm     datetime2(3)   NOT NULL CONSTRAINT DF_RegraExec_Em DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_RegraExecucao PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_RegraExec_Regra FOREIGN KEY (RegraId) REFERENCES wf.Regra(Id),
    CONSTRAINT CK_RegraExec_Resultado CHECK (Resultado IN
        ('Disparou','CondicaoFalsa','RegraInativa','SemDestinatario','Erro','Suprimida'))
);
CREATE INDEX IX_RegraExec_Correlacao ON wf.RegraExecucao (CorrelacaoId);
CREATE INDEX IX_RegraExec_Monitor    ON wf.RegraExecucao (RegraId, ExecutadoEm DESC, Resultado);
CREATE INDEX IX_RegraExec_Processo   ON wf.RegraExecucao (ProcessoId, ExecutadoEm DESC);
```

> **Retenção:** partição mensal, 12 meses online, arquivamento depois. `[V]` o Vórtice tem 44,8M de
> 85,5M linhas em log **sem nenhuma política de retenção** — 52,4% do banco.

> **O alarme que fecha o ciclo:** um job diário compara a taxa de disparo de cada regra com a média
> das 4 semanas anteriores. Queda maior que 30% abre incidente automaticamente. É exatamente o
> monitoramento que teria pego a ação 900 em março, e não em agosto.

---

## 6. Módulo `seg` — permissões (detalhe no documento 05)

```sql
-- Permissão nomeada. Granularidade: entidade + verbo.
CREATE TABLE seg.Permissao (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(80)    NOT NULL,   -- 'Processo.Ler', 'Processo.Editar', 'Lead.Qualificar'
    Entidade        varchar(40)    NOT NULL,
    Verbo           varchar(20)    NOT NULL,   -- 'Ler','Criar','Editar','Excluir','Atribuir','Compartilhar'
    Descricao       nvarchar(200)  NOT NULL,
    CONSTRAINT PK_Permissao PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Permissao UNIQUE (Codigo)
);

-- Conjunto de permissões. [SF] Permission Set: aditivo e componível.
CREATE TABLE seg.ConjuntoPermissao (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(60)    NOT NULL,   -- 'VENDEDOR','GERENTE_VENDAS','ADM_VENDAS'
    Nome            nvarchar(120)  NOT NULL,
    Descricao       nvarchar(400)  NULL,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_ConjPerm_Ativo DEFAULT 1,
    CONSTRAINT PK_ConjuntoPermissao PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ConjuntoPermissao UNIQUE (Codigo)
);

CREATE TABLE seg.ConjuntoPermissaoItem (
    ConjuntoPermissaoId int        NOT NULL,
    PermissaoId     int            NOT NULL,
    -- A PROFUNDIDADE. [DYN] a matriz privilégio x access level é o coração do modelo:
    -- 0=Nenhum, 1=Proprios, 2=Equipe, 3=Empresa, 4=EmpresaEAbaixo, 5=Organizacao
    Profundidade    tinyint        NOT NULL,
    CONSTRAINT PK_ConjPermItem PRIMARY KEY CLUSTERED (ConjuntoPermissaoId, PermissaoId),
    CONSTRAINT FK_CPI_Conjunto  FOREIGN KEY (ConjuntoPermissaoId) REFERENCES seg.ConjuntoPermissao(Id),
    CONSTRAINT FK_CPI_Permissao FOREIGN KEY (PermissaoId)         REFERENCES seg.Permissao(Id),
    CONSTRAINT CK_CPI_Profundidade CHECK (Profundidade BETWEEN 0 AND 5)
);

CREATE TABLE seg.UsuarioConjuntoPermissao (
    UsuarioId       bigint         NOT NULL,
    ConjuntoPermissaoId int        NOT NULL,
    ConcedidoEm     datetime2(3)   NOT NULL CONSTRAINT DF_UCP_Em DEFAULT SYSUTCDATETIME(),
    ConcedidoPorId  bigint         NOT NULL,
    ExpiraEm        datetime2(3)   NULL,      -- acesso temporário (cobertura de férias)
    CONSTRAINT PK_UsuarioConjPerm PRIMARY KEY CLUSTERED (UsuarioId, ConjuntoPermissaoId),
    CONSTRAINT FK_UCP_Usuario  FOREIGN KEY (UsuarioId)           REFERENCES seg.Usuario(Id),
    CONSTRAINT FK_UCP_Conjunto FOREIGN KEY (ConjuntoPermissaoId) REFERENCES seg.ConjuntoPermissao(Id)
);

-- Compartilhamento explícito de um registro. [SF] tabela de share com RowCause tipado.
CREATE TABLE seg.CompartilhamentoRegistro (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    Entidade        varchar(40)    NOT NULL,   -- 'Processo','Conta','Lead'
    RegistroId      bigint         NOT NULL,
    UsuarioId       bigint         NULL,
    EquipeId        bigint         NULL,
    -- 'Leitura' ou 'Edicao'.
    Nivel           varchar(10)    NOT NULL,
    -- POR QUE este usuário vê este registro. [DYN] a Microsoft precisou criar uma API só para
    -- responder isso; aqui está gravado desde o começo.
    Motivo          varchar(20)    NOT NULL,   -- 'Manual','Regra','Equipe','Hierarquia','Delegacao'
    RegraOrigemId   int            NULL,
    ConcedidoEm     datetime2(3)   NOT NULL CONSTRAINT DF_CompReg_Em DEFAULT SYSUTCDATETIME(),
    ConcedidoPorId  bigint         NOT NULL,
    ExpiraEm        datetime2(3)   NULL,
    CONSTRAINT PK_CompartilhamentoRegistro PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_CompReg_Nivel CHECK (Nivel IN ('Leitura','Edicao')),
    CONSTRAINT CK_CompReg_UmSujeito CHECK (
        (UsuarioId IS NOT NULL AND EquipeId IS NULL)
     OR (UsuarioId IS NULL AND EquipeId IS NOT NULL))
);
CREATE UNIQUE INDEX UX_CompReg
    ON seg.CompartilhamentoRegistro (Entidade, RegistroId, UsuarioId, EquipeId, Motivo);
CREATE INDEX IX_CompReg_Usuario ON seg.CompartilhamentoRegistro (UsuarioId, Entidade);
```

---

## 7. Módulo `aud` — auditoria escopada

`[SF]` Field History Tracking: **20 campos por objeto, 18 meses**. Auditoria é **escopada**, não total.
`[V]` O Vórtice audita tudo e o resultado é 44,8M linhas, das quais **96,8% de `GE_LOG_PROCESSO` são
re-carimbos** de um job de cobrança — 617 linhas de log por processo.

```sql
-- Quais campos são auditados. Sem linha aqui, não há auditoria: opt-in explícito.
CREATE TABLE aud.CampoAuditado (
    Id              int            IDENTITY(1,1) NOT NULL,
    Entidade        varchar(40)    NOT NULL,
    Campo           varchar(60)    NOT NULL,
    RetencaoMeses   smallint       NOT NULL CONSTRAINT DF_CampoAud_Ret DEFAULT 18,
    CONSTRAINT PK_CampoAuditado PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_CampoAuditado UNIQUE (Entidade, Campo)
);

CREATE TABLE aud.AlteracaoCampo (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    Entidade        varchar(40)    NOT NULL,
    RegistroId      bigint         NOT NULL,
    Campo           varchar(60)    NOT NULL,
    ValorAnterior   nvarchar(400)  NULL,
    ValorNovo       nvarchar(400)  NULL,
    AlteradoEm      datetime2(3)   NOT NULL CONSTRAINT DF_AlterCampo_Em DEFAULT SYSUTCDATETIME(),
    AlteradoPorId   bigint         NOT NULL,
    -- Correlaciona com a requisição HTTP que causou a mudança.
    CorrelacaoId    uniqueidentifier NULL,
    CONSTRAINT PK_AlteracaoCampo PRIMARY KEY CLUSTERED (Id)
);
CREATE INDEX IX_AlterCampo_Registro ON aud.AlteracaoCampo (Entidade, RegistroId, AlteradoEm DESC);

-- Auditoria de ACESSO. [V] o Vórtice tem 114 eventos de login em 9 anos -- ou seja, nenhum.
CREATE TABLE aud.EventoAcesso (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    UsuarioId       bigint         NULL,       -- NULL quando o login falhou e não identificou
    Upn             nvarchar(200)  NULL,
    Tipo            varchar(30)    NOT NULL,   -- 'Login','LoginFalhou','Logout','AcessoNegado',
                                               -- 'ExportacaoDados','LeituraDadoSensivel'
    EnderecoIp      varchar(45)    NULL,
    UserAgent       nvarchar(400)  NULL,
    Detalhe         nvarchar(1000) NULL,
    OcorreuEm       datetime2(3)   NOT NULL CONSTRAINT DF_EvAcesso_Em DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_EventoAcesso PRIMARY KEY CLUSTERED (Id)
);
CREATE INDEX IX_EvAcesso_Usuario ON aud.EventoAcesso (UsuarioId, OcorreuEm DESC);
CREATE INDEX IX_EvAcesso_Tipo    ON aud.EventoAcesso (Tipo, OcorreuEm DESC);
```

---

## 8. Módulo `intg` — integração (anti-corruption layer)

`[V]` A integração do Vórtice não tem transação, não tem watermark confiável, não tem fila de erro e
registra falha crítica como `Warning`. Resultado: **783.242 títulos parados há 15 meses sem ninguém saber**.

```sql
-- Correlação de identidade com sistemas externos. [SF] External ID: o mecanismo de upsert
-- idempotente. [DYN] Alternate Key.
CREATE TABLE intg.ChaveExterna (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    Entidade        varchar(40)    NOT NULL,   -- 'Conta','Contato','Processo'
    RegistroId      bigint         NOT NULL,
    SistemaOrigem   varchar(20)    NOT NULL,   -- 'TOTVS','JDE','VORTICE','RDSTATION'
    ChaveOrigem     varchar(200)   NOT NULL,
    SincronizadoEm  datetime2(3)   NOT NULL CONSTRAINT DF_ChaveExt_Em DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ChaveExterna PRIMARY KEY CLUSTERED (Id),
    -- A chave externa é ÚNICA por sistema. É isto que torna o upsert idempotente e impede
    -- a duplicação que o Vórtice sofre.
    CONSTRAINT UQ_ChaveExterna UNIQUE (SistemaOrigem, Entidade, ChaveOrigem)
);
CREATE INDEX IX_ChaveExterna_Registro ON intg.ChaveExterna (Entidade, RegistroId);

-- Marca d'água por fluxo de integração: até onde já lemos.
CREATE TABLE intg.Watermark (
    Fluxo           varchar(60)    NOT NULL,   -- 'TOTVS.Titulo','VORTICE.Pessoa'
    UltimoValor     varchar(100)   NOT NULL,   -- timestamp ou ID, conforme o fluxo
    ProcessadoEm    datetime2(3)   NOT NULL,
    RegistrosLidos  int            NOT NULL,
    RegistrosGravados int          NOT NULL,
    RegistrosErro   int            NOT NULL,
    CONSTRAINT PK_Watermark PRIMARY KEY CLUSTERED (Fluxo)
);

-- Outbox: mensagens que ESTE sistema precisa entregar a outro. Escrita na MESMA transação
-- do dado, garantindo que nunca há dado sem evento nem evento sem dado.
CREATE TABLE intg.MensagemSaida (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    Tipo            varchar(60)    NOT NULL,
    Destino         varchar(40)    NOT NULL,
    Payload         nvarchar(max)  NOT NULL,
    CorrelacaoId    uniqueidentifier NOT NULL,
    Situacao        varchar(20)    NOT NULL CONSTRAINT DF_MsgSaida_Sit DEFAULT 'Pendente',
    Tentativas      smallint       NOT NULL CONSTRAINT DF_MsgSaida_Tent DEFAULT 0,
    ProximaTentativaEm datetime2(3) NULL,
    UltimoErro      nvarchar(2000) NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_MsgSaida_Em DEFAULT SYSUTCDATETIME(),
    EntregueEm      datetime2(3)   NULL,
    CONSTRAINT PK_MensagemSaida PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_MsgSaida_Situacao CHECK (Situacao IN
        ('Pendente','Entregue','Falhou','DescartadaAposLimite'))
);
CREATE INDEX IX_MsgSaida_Fila ON intg.MensagemSaida (ProximaTentativaEm)
    WHERE Situacao = 'Pendente';

-- Dead-letter: o que falhou definitivamente. [V] a fila de e-mail do Vórtice tem 22.512
-- falhas silenciosas e SEM RETRY. Aqui nada morre em silêncio.
CREATE TABLE intg.MensagemDescartada (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    MensagemSaidaId bigint         NULL,
    Fluxo           varchar(60)    NOT NULL,
    Payload         nvarchar(max)  NOT NULL,
    Erro            nvarchar(4000) NOT NULL,
    Tentativas      smallint       NOT NULL,
    DescartadaEm    datetime2(3)   NOT NULL CONSTRAINT DF_MsgDesc_Em DEFAULT SYSUTCDATETIME(),
    -- Quem tratou e como. Uma DLQ que ninguém olha é igual a não ter DLQ.
    TratadaEm       datetime2(3)   NULL,
    TratadaPorId    bigint         NULL,
    Tratativa       nvarchar(1000) NULL,
    CONSTRAINT PK_MensagemDescartada PRIMARY KEY CLUSTERED (Id)
);
```

---

## 9. Módulo `equip` — equipamentos do cliente

`[SF]` `Asset` · `[DYN]` `msdyn_customerasset` com **duas hierarquias** (ParentAsset e MasterAsset).
Para uma revenda de máquinas agrícolas, esta é a entidade mais valiosa do pós-venda — e é onde estão os
3,57 milhões de linhas de plano de manutenção do Vórtice.

```sql
CREATE TABLE equip.Marca (
    Id int IDENTITY(1,1) NOT NULL, Nome nvarchar(80) NOT NULL,
    CONSTRAINT PK_Marca PRIMARY KEY CLUSTERED (Id), CONSTRAINT UQ_Marca UNIQUE (Nome));

CREATE TABLE equip.Familia (
    Id int IDENTITY(1,1) NOT NULL, MarcaId int NOT NULL, Nome nvarchar(80) NOT NULL,
    CONSTRAINT PK_Familia PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Familia_Marca FOREIGN KEY (MarcaId) REFERENCES equip.Marca(Id));

CREATE TABLE equip.Modelo (
    Id int IDENTITY(1,1) NOT NULL, FamiliaId int NOT NULL,
    Codigo varchar(40) NOT NULL, Nome nvarchar(120) NOT NULL,
    CONSTRAINT PK_Modelo PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Modelo UNIQUE (Codigo),
    CONSTRAINT FK_Modelo_Familia FOREIGN KEY (FamiliaId) REFERENCES equip.Familia(Id));

CREATE TABLE equip.Equipamento (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Equip_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    -- Dono atual. NULL = em estoque, ainda não vendido.
    ContaId         bigint         NULL,
    ModeloId        int            NOT NULL,
    -- Chassi é a identidade de verdade da máquina agrícola.
    Chassi          varchar(40)    NOT NULL,
    NumeroSerie     varchar(40)    NULL,
    Placa           varchar(10)    NULL,
    AnoFabricacao   smallint       NULL,
    AnoModelo       smallint       NULL,
    HorimetroAtual  decimal(12,2)  NULL,
    HorimetroAtualizadoEm datetime2(3) NULL,
    -- [DYN] duas hierarquias distintas, e ambas fazem sentido aqui:
    -- 'Pai' = implemento acoplado a um trator; 'Mestre' = a máquina que substituiu esta.
    EquipamentoPaiId bigint        NULL,
    EquipamentoMestreId bigint     NULL,
    Situacao        varchar(20)    NOT NULL CONSTRAINT DF_Equip_Situacao DEFAULT 'Ativo',
    VendidoEm       date           NULL,
    GarantiaAte     date           NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Equip_CriadoEm DEFAULT SYSUTCDATETIME(),
    ExcluidoEm      datetime2(3)   NULL,
    Versao          rowversion     NOT NULL,
    CONSTRAINT PK_Equipamento PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Equip_Conta  FOREIGN KEY (ContaId)  REFERENCES crm.Conta(Id),
    CONSTRAINT FK_Equip_Modelo FOREIGN KEY (ModeloId) REFERENCES equip.Modelo(Id),
    CONSTRAINT FK_Equip_Pai    FOREIGN KEY (EquipamentoPaiId)    REFERENCES equip.Equipamento(Id),
    CONSTRAINT FK_Equip_Mestre FOREIGN KEY (EquipamentoMestreId) REFERENCES equip.Equipamento(Id),
    CONSTRAINT CK_Equip_Situacao CHECK (Situacao IN ('Estoque','Ativo','Vendido','Baixado'))
);
CREATE UNIQUE INDEX UX_Equipamento_Chassi ON equip.Equipamento (Chassi) WHERE ExcluidoEm IS NULL;
CREATE INDEX IX_Equipamento_Conta ON equip.Equipamento (ContaId) WHERE ExcluidoEm IS NULL;
```

---

## 10. Módulo `doc` — documentos

`[V]` O sincronismo do mobile morre em **5.302 vínculos órfãos (6,9%)** porque `IV_ProcDocto` aponta
para `DMN_Doc` **sem FK** — o SQL Server serve o órfão, o SQLite do aparelho recusa e aborta a carga.

```sql
CREATE TABLE doc.Documento (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Doc_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    TipoDocumentoId int            NOT NULL,
    Nome            nvarchar(260)  NOT NULL,
    ContentType     varchar(120)   NOT NULL,
    TamanhoBytes    bigint         NOT NULL,
    -- SHA-256 do conteúdo: deduplicação e verificação de integridade.
    HashConteudo    char(64)       NOT NULL,
    -- Caminho no storage (Azure Blob ou file share). NUNCA o binário no banco.
    CaminhoStorage  nvarchar(600)  NOT NULL,
    ValidoAte       date           NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Doc_CriadoEm DEFAULT SYSUTCDATETIME(),
    CriadoPorId     bigint         NOT NULL,
    ExcluidoEm      datetime2(3)   NULL,
    CONSTRAINT PK_Documento PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Doc_Tipo FOREIGN KEY (TipoDocumentoId) REFERENCES doc.TipoDocumento(Id)
);

-- Vínculo polimórfico. A FK para Documento é OBRIGATÓRIA e declarada -- é exatamente
-- o que falta no Vórtice e o que trava o mobile.
CREATE TABLE doc.DocumentoVinculo (
    DocumentoId     bigint         NOT NULL,
    Entidade        varchar(40)    NOT NULL,   -- 'Processo','Conta','Equipamento'
    RegistroId      bigint         NOT NULL,
    VinculadoEm     datetime2(3)   NOT NULL CONSTRAINT DF_DocVinc_Em DEFAULT SYSUTCDATETIME(),
    VinculadoPorId  bigint         NOT NULL,
    CONSTRAINT PK_DocumentoVinculo PRIMARY KEY CLUSTERED (DocumentoId, Entidade, RegistroId),
    -- ON DELETE CASCADE: excluir o documento REMOVE os vínculos. Sem órfão possível.
    CONSTRAINT FK_DocVinc_Documento FOREIGN KEY (DocumentoId)
        REFERENCES doc.Documento(Id) ON DELETE CASCADE
);
CREATE INDEX IX_DocVinculo_Registro ON doc.DocumentoVinculo (Entidade, RegistroId);
```

---

## 11. Módulo `meta` — extensibilidade sem release

`[SF]` Custom Metadata Types. `[DYN]` `SdkMessageProcessingStep`. A ideia: **configuração é dado**.

Decisão consciente: **não** copiamos o motor EAV do Salesforce. Campo customizado aqui vira **coluna
real** via migration gerada — mantemos o catálogo (para a UI saber montar a tela) sem pagar o preço do
EAV nem herdar teto de campos.

```sql
-- Catálogo de campos, inclusive os nativos. A UI monta a tela a partir daqui.
-- [SF] a UI API devolve DADO + METADADO juntos, para o cliente nunca hardcodar campo.
CREATE TABLE meta.CampoEntidade (
    Id              int            IDENTITY(1,1) NOT NULL,
    Entidade        varchar(40)    NOT NULL,
    Campo           varchar(60)    NOT NULL,   -- nome da coluna real
    Rotulo          nvarchar(80)   NOT NULL,   -- o que o usuário lê
    TipoDado        varchar(20)    NOT NULL,   -- 'Texto','Numero','Data','Booleano','Lista','Referencia'
    EhCustomizado   bit            NOT NULL CONSTRAINT DF_CampoEnt_Custom DEFAULT 0,
    EhObrigatorio   bit            NOT NULL CONSTRAINT DF_CampoEnt_Obrig DEFAULT 0,
    Tamanho         int            NULL,
    ListaValoresId  int            NULL,
    -- Ordem e agrupamento na tela.
    Grupo           nvarchar(60)   NULL,
    Ordem           smallint       NOT NULL CONSTRAINT DF_CampoEnt_Ordem DEFAULT 100,
    -- Visibilidade condicional. [SF] Dynamic Forms substitui N layouts por M record types.
    CondicaoVisibilidade nvarchar(1000) NULL,
    -- Aparece no cabeçalho do registro. [SF] Compact Layout: 7 campos, a melhor razão
    -- esforço/benefício de UX de todo o Salesforce.
    NoResumo        bit            NOT NULL CONSTRAINT DF_CampoEnt_Resumo DEFAULT 0,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_CampoEnt_Ativo DEFAULT 1,
    CONSTRAINT PK_CampoEntidade PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_CampoEntidade UNIQUE (Entidade, Campo)
);

-- Registro de handlers. [DYN] o ponto de extensão é uma LINHA DE TABELA, não de código.
-- Ligar/desligar/reordenar lógica sem deploy.
CREATE TABLE meta.ManipuladorEvento (
    Id              int            IDENTITY(1,1) NOT NULL,
    Evento          varchar(60)    NOT NULL,   -- 'Processo.AntesDeSalvar','Lead.AposQualificar'
    -- Nome do tipo .NET que implementa IManipuladorEvento. Resolvido por DI no boot.
    TipoDotNet      varchar(200)   NOT NULL,
    Estagio         varchar(20)    NOT NULL,   -- 'PreValidacao','PreOperacao','PosOperacao'
    Ordem           smallint       NOT NULL CONSTRAINT DF_ManipEv_Ordem DEFAULT 100,
    EhAssincrono    bit            NOT NULL CONSTRAINT DF_ManipEv_Async DEFAULT 0,
    EstaAtivo       bit            NOT NULL CONSTRAINT DF_ManipEv_Ativo DEFAULT 1,
    -- Só dispara se estes campos mudarem. [DYN] filtering attributes -- "a otimização que
    -- decide a performance de todo o sistema".
    CamposFiltro    varchar(400)   NULL,       -- CSV de nomes de coluna
    CONSTRAINT PK_ManipuladorEvento PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ManipuladorEvento UNIQUE (Evento, TipoDotNet, Estagio),
    CONSTRAINT CK_ManipEv_Estagio CHECK (Estagio IN ('PreValidacao','PreOperacao','PosOperacao')),
    -- [DYN] assíncrono SÓ existe no PostOperation.
    CONSTRAINT CK_ManipEv_Async CHECK (EhAssincrono = 0 OR Estagio = 'PosOperacao')
);
```

---

## 12. Módulo `rel` — relatórios com contrato curado

`[V]` O QVW guarda **SQL cru** numa coluna `TEXT`, aceita `UPDATE`/`EXEC`, e **125 dos 134 relatórios não
têm predicado de usuário**. `[SF]` Report Type: o admin cura o conjunto de dados; o usuário monta o
relatório sem enxergar o schema — **e sem escrever SQL**.

```sql
-- O contrato curado. Define O QUE pode ser consultado. Criado por dev, não por usuário.
CREATE TABLE rel.FonteRelatorio (
    Id              int            IDENTITY(1,1) NOT NULL,
    Codigo          varchar(60)    NOT NULL,   -- 'PIPELINE_VENDAS','LEADS_POR_ORIGEM'
    Nome            nvarchar(120)  NOT NULL,
    Descricao       nvarchar(400)  NULL,
    -- Nome da VIEW que serve esta fonte. A view JÁ aplica o filtro de segurança por linha
    -- (ver documento 05) -- o usuário nunca consegue escapar do próprio escopo.
    ViewNome        varchar(120)   NOT NULL,
    EntidadeRaiz    varchar(40)    NOT NULL,
    EstaAtiva       bit            NOT NULL CONSTRAINT DF_FonteRel_Ativa DEFAULT 1,
    CONSTRAINT PK_FonteRelatorio PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_FonteRelatorio UNIQUE (Codigo)
);

CREATE TABLE rel.FonteCampo (
    Id              int            IDENTITY(1,1) NOT NULL,
    FonteRelatorioId int           NOT NULL,
    Campo           varchar(60)    NOT NULL,
    Rotulo          nvarchar(80)   NOT NULL,
    TipoDado        varchar(20)    NOT NULL,
    PermiteAgrupar  bit            NOT NULL CONSTRAINT DF_FonteCampo_Agrup DEFAULT 1,
    PermiteFiltrar  bit            NOT NULL CONSTRAINT DF_FonteCampo_Filtr DEFAULT 1,
    PermiteSomar    bit            NOT NULL CONSTRAINT DF_FonteCampo_Somar DEFAULT 0,
    CONSTRAINT PK_FonteCampo PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_FonteCampo UNIQUE (FonteRelatorioId, Campo),
    CONSTRAINT FK_FonteCampo_Fonte FOREIGN KEY (FonteRelatorioId) REFERENCES rel.FonteRelatorio(Id)
);

-- O relatório do usuário: uma DEFINIÇÃO EM JSON, nunca SQL.
CREATE TABLE rel.Relatorio (
    Id              bigint         IDENTITY(1,1) NOT NULL,
    ChavePublica    uniqueidentifier NOT NULL CONSTRAINT DF_Relatorio_Chave DEFAULT NEWID(),
    EmpresaId       int            NOT NULL,
    FonteRelatorioId int           NOT NULL,
    Nome            nvarchar(120)  NOT NULL,
    -- {campos:[], filtros:[], agrupamentos:[], ordenacao:[], grafico:{}}
    -- O motor TRADUZ isto para SQL parametrizado. O usuário nunca escreve SQL.
    Definicao       nvarchar(max)  NOT NULL,
    -- 'Privado','Equipe','Empresa'
    Visibilidade    varchar(20)    NOT NULL CONSTRAINT DF_Relatorio_Vis DEFAULT 'Privado',
    ProprietarioId  bigint         NOT NULL,
    CriadoEm        datetime2(3)   NOT NULL CONSTRAINT DF_Relatorio_Em DEFAULT SYSUTCDATETIME(),
    AlteradoEm      datetime2(3)   NULL,
    ExcluidoEm      datetime2(3)   NULL,
    CONSTRAINT PK_Relatorio PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Relatorio_Fonte FOREIGN KEY (FonteRelatorioId) REFERENCES rel.FonteRelatorio(Id),
    CONSTRAINT CK_Relatorio_Vis CHECK (Visibilidade IN ('Privado','Equipe','Empresa'))
);
```

---

## 13. Resumo — tabelas por fase

| Schema | Tabela | Fase | Origem da decisão |
|---|---|---|---|
| `org` | Empresa, LinhaNegocio, Carteira, HierarquiaVendas | **1** | `[DYN]` BU · `[V]` carteira |
| `seg` | Usuario, Equipe, EquipeMembro | **1** | Entra ID · `[DYN]` Owner Team |
| `seg` | Permissao, ConjuntoPermissao, ConjuntoPermissaoItem, UsuarioConjuntoPermissao | **1** | `[SF]` Permission Set + `[DYN]` depth |
| `seg` | CompartilhamentoRegistro | **1** | `[SF]` share table |
| `crm` | Conta, Contato, ContaContato, CanalContato | **1** | `[SF]` Account/Contact |
| `crm` | ConsentimentoComunicacao | **1** | LGPD — `[V]` opt-in sem prova |
| `crm` | ContaCarteira | **1** | `[V]` o melhor ativo do legado |
| `crm` | Lead, OrigemLead, PapelContato | **1** | `[SF]` flat + `[DYN]` sobrevive |
| `wf` | TipoProcesso, Estagio, TipoTarefa, Desfecho | **1** | `[V]` 5 entidades |
| `wf` | Processo, ProcessoEstagioTrilha | **1** | `[DYN]` BPF traversedpath |
| `wf` | Tarefa, Atividade, AtividadeParticipante | **1** | `[V]` duplo ponteiro · `[DYN]` ActivityParty |
| `wf` | Regra, RegraExecucao | **1** | **a correção central** |
| `aud` | CampoAuditado, AlteracaoCampo, EventoAcesso | **1** | `[SF]` escopada |
| `meta` | CampoEntidade, ManipuladorEvento | **1** | `[SF]` __mdt · `[DYN]` step |
| `intg` | ChaveExterna, Watermark, MensagemSaida, MensagemDescartada | **2** | ACL com DLQ |
| `equip` | Marca, Familia, Modelo, Equipamento | **3** | `[SF]` Asset · `[DYN]` customerasset |
| `doc` | TipoDocumento, Documento, DocumentoVinculo | **3** | `[V]` 5.302 órfãos |
| `rel` | FonteRelatorio, FonteCampo, Relatorio | **4** | `[SF]` Report Type |

**Fase 1: 25 tabelas.** É o que sustenta Leads e Prospecção de ponta a ponta, com segurança e auditoria
reais desde o primeiro dia.

Para comparação: o Vórtice tem **767 tabelas, das quais 131 vazias**.
