# Módulos, telas, permissões e licenças do Vórtice

> **Documento 11 de 11** · Versão 1.0 · 31/08/2026
> Levantado por consulta read-only ao banco de produção em 31/08/2026, complementando as
> capturas de tela. Substitui as estimativas que os documentos 00 e 05 usavam.

---

## 1. A anatomia do produto

O Vórtico não é um aplicativo — são **quatro executáveis desktop** com licença própria, mais
módulos de apoio, tudo controlado por um catálogo de licenciamento no próprio banco.

| Módulo | Sigla | Versão instalada | O que é |
|---|---|---|---|
| **Atendente** | `CRM_M001` | 4.04.01**r15** | a tela do CEN — agenda, andamento, cadastro |
| **Administrador** | `CRM_M002` | 4.04.01**r16** | administração de dados |
| **Supervisor** | `CRM_M003` | 4.04.01**r17** | o menu Configuração das capturas |
| **Configurador** | `CRM_M004` | 4.04.01**r18** | parametrização profunda |
| Modeler | `CRM_M006` | 4.04.01r03 | desenho de fluxo |
| Cobrança | `CRM_M010` | 4.04.01r02 | régua de cobrança |
| Segurança global | `GLB_M000` | 4.04.01r05 | políticas |
| Query Builder | `QVW_M001` | 4.03.01r01 | criação de relatório |
| Query Viewer | `QVW_M003` | 4.04.01r01 | execução de relatório |
| JUNO Mobile | `CRM_M051` | 4.04.01r01 | app antigo |
| Mobile Lite | `CRM_M052` | 4.04.01r01 | app atual |

### 🔴 1.1 Cinco gerações de software convivendo

As versões instaladas, ordenadas:

| Versão | Módulos |
|---|---|
| **3.7.00** | `TESTESMS` |
| **3.90.00** | `IMPORTADOREXT` (integração com ERP) |
| **4.01.01r43** | `GLOBAL/OPERADOR` |
| **4.03.01r01** | `QUERYVIEW`, `QVW/BUILDER` |
| **4.04.01r01–r18** | o núcleo |

**Cinco gerações, com quase quatro versões maiores de distância entre a mais velha e a mais nova.**

Isso não é curiosidade: é a causa do erro que documentamos. O job `RUNGEPIMPORT` falha **a cada 20
minutos há meses** com `Versão incompatível [4.04.01r05] x [4.04.01r01]`. Não é um defeito isolado —
é o sintoma previsível de uma instalação que nunca foi atualizada por inteiro.

> **Para o CRM novo:** versão única, deploy atômico, e a versão declarada num lugar só
> (`Directory.Build.props`). Componente que sobe sozinho é componente que diverge.

### 🟡 1.2 Cinco módulos licenciados e nunca instalados

Estes estão registrados com `TipoAcesso = P` (licença paga) e **a coluna de versão em branco**:

| Módulo | Sigla |
|---|---|
| Vórtico Config RD Station | `CRM_M007` |
| **Vórtico Web CRM** | `CRM_M020` |
| Vortico OUT CRM | `CRM_M021` |
| **Vórtico CRM BPM 2025** | `CRM_M100` |
| **Vórtico Painel de Controle 2025** | `CRM_M110` |

Os três em negrito são a geração web que o fornecedor vem oferecendo. Estão no catálogo e **não têm
versão instalada**.

> **Ação, independente do projeto:** conferir o contrato. Se algum desses está sendo cobrado sem
> estar implantado, é dinheiro saindo por nada. Não consigo responder isso pelo banco — só o
> contrato diz.

---

## 2. As 184 telas

`GE_Aplicacao` registra **184 telas**, distribuídas assim:

| Sistema / Módulo | Telas | Quem usa |
|---|---:|---|
| INTERVISION / **CONFIG** | **44** | Configurador |
| INTERVISION / **ADMIN** | **34** | Administrador |
| INTERVISION / **OPERADOR** | **32** | **o CEN — o usuário comum** |
| INTERVISION / **SUPERVISOR** | **30** | Supervisor |
| GLOBAL / SEGURANCA | 14 | segurança |
| INTERVISION / COBRANCA | 8 | cobrança |
| Demais (11 módulos) | 22 | integração, migração, BI, jobs |

**Duas leituras:**

**O usuário comum tem 32 telas.** É muito para quem precisa executar meia dúzia de tarefas por dia —
e explica as 10 abas fixas que vimos na captura.

**A configuração tem 108 telas** (CONFIG 44 + ADMIN 34 + SUPERVISOR 30) contra 32 de operação.
**Mais de três telas de configuração para cada tela de trabalho.** É o retrato de um produto que
virou plataforma de parametrização.

> **Para o CRM novo:** o documento 08 propõe **5 destinos** para o usuário comum e uma área de
> administração que só aparece com permissão. A meta é ficar abaixo de 15 telas de operação na
> fase 1 inteira.

---

## 3. O modelo de permissão, medido

### 3.1 As 11 políticas

| # | Política | Usuários | Nunca logaram | Ativos em 90 dias |
|---:|---|---:|---:|---:|
| 2 | Atendentes | **501** | **352 (70%)** | 77 |
| 3 | Supervisores | 71 | 45 (63%) | 12 |
| 6 | CEN | 64 | 1 | **25** |
| 7 | Vendas Digitas *(sic)* | 12 | 1 | 6 |
| 1 | Vórtice *(o fornecedor)* | 11 | 4 | 4 |
| 4 | Administrador Sistema | 8 | 1 | 5 |
| 8 | Adm de Vendas | 7 | 0 | 6 |
| 9 | DSI | 2 | 1 | 1 |
| 0 / 5 / 999999 | Global · Temporario · * Master | **0** | — | — |

### 🔴 3.2 Metade dos usuários não tem política nenhuma

| | |
|---|---:|
| Usuários com política | **676** |
| **Usuários sem política** | **710 — 51%** |
| Total cadastrado | **1.386** |
| **Usuários realmente ativos em 90 dias** | **≈ 140 (10%)** |

**710 usuários não estão em nenhuma política de segurança.** Desses, só 4 logaram nos últimos 90
dias — o que sugere que são contas antigas nunca removidas, e não um buraco ativo. Mas o efeito é o
mesmo: **não é possível responder "o que este usuário pode fazer" para metade do cadastro.**

E a política "Atendentes", a maior de todas com 501 usuários, tem **352 que nunca logaram**.

> **Número corrigido:** os documentos 00 e 02 usam **130 usuários** para calcular o custo do
> Salesforce. A medição confirma a ordem de grandeza: **≈140 usuários ativos em 90 dias**. O cálculo
> do business case continua válido — se algo, era conservador.

### 🔴 3.3 A permissão por tela cobre 5% das telas

`GE_POLSEGPERM` — a tabela que diz qual política acessa qual tela — tem **805 linhas** no total:

| Política | Linhas | Telas distintas | Empresas distintas |
|---|---:|---:|---:|
| 4 Administrador Sistema | 164 | 10 | **1** |
| 2 Atendentes | 160 | 10 | **1** |
| 3 Supervisores | 156 | 10 | **1** |
| 0 Global | 121 | 8 | **1** |
| 1 Vórtice | 78 | 8 | **1** |
| 8 Adm de Vendas | 39 | 3 | **1** |
| 7 Vendas Digitais | 38 | 3 | **1** |
| 6 CEN | 32 | 4 | **1** |
| 9 DSI | 15 | 2 | **1** |
| 5 Temporario | 2 | 1 | **1** |

**Nenhuma política controla mais de 10 telas — de 184 registradas.** As outras 174 não têm controle
de acesso por política: quem abre o módulo, vê.

E a coluna `NROEMPRESA` existe em todas as linhas com **exatamente um valor distinto**. A permissão
multiempresa está modelada e **nunca foi usada** — mais uma confirmação de que o isolamento entre as
18 filiais é nominal.

### 🟡 3.4 Os 108 itens de política, e onde eles não existem

| Módulo | Itens configuráveis |
|---|---:|
| `CRM_M001` (Atendente, desktop) | **68** |
| `CRM_M051` (JUNO / mobile) | **23** |
| `GLB_M000` (global: senha, LGPD) | 13 |
| `DMN_M001` (documentos) | 2 |
| `CRM_M003` (Supervisor) | 2 |

**Sem nenhum item:** `CRM_M002` (Administrador), `CRM_M004` (Configurador) e — o mais grave —
**`CRM_M052` (Mobile Lite)**, que é o app em uso hoje.

Ou seja: as 23 regras de sincronismo mobile pertencem ao **JUNO**, o app que parou em março de 2025.
O **Mobile Lite não tem política de segurança própria** — só o parâmetro `SyncAgdFutura`.

### 🔴 3.5 A prova definitiva: não existe coluna de inativação

Colunas de `GE_Usuario`:

```
SeqUsuario, CodUsuario, Nome, NomeReduzido, SeqPessoa, Senha, LoginId, TipoUsuario,
RegistrarLog, DTALIMITEUSO, Nivel, Assinatura, CodUsuarioExt, NroUsuarioExt,
RecebeCiencia, Senha3, Loginexpirando, Qtdeloginrestante, Ulttrocasenha, Identificacao,
SeqPolSeg, CelularTrab, INDSILO, ChkSum, TIPOSILO, SEQCONTATO, INDUSREXT, LOGIN,
EMAILTRAB, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, DTALOGIN, DTALOGINANT
```

**Não há `Ativo`, `Inativo`, `DtaInativacao` nem `Situacao`.** Só `DTALIMITEUSO` — uma data limite,
que não é a mesma coisa.

Isso confirma, direto no schema, o que o suporte já sabia na prática: **desativar um usuário no
Vórtice significa desmontar as permissões dele.** Não há como preservar o histórico e cortar o
acesso ao mesmo tempo.

E há **três senhas** no cadastro: `Senha`, `Senha3`, e `ChkSum`. Somado ao que já medimos —
663 senhas produzindo 467 valores distintos, 80 usuários com o mesmo valor — o modelo de
credencial é o ponto mais frágil do sistema.

---

## 4. O que isso muda nos nossos documentos

| Documento | Ajuste |
|---|---|
| **00 Briefing** | trocar "130 usuários" por **"≈140 ativos, de 1.386 cadastrados"**. O contraste é mais forte e é medido |
| **01 Diagnóstico** | acrescentar: 710 usuários sem política · permissão por tela cobre 10 de 184 · cinco gerações de software convivendo · `GE_Usuario` sem coluna de inativação |
| **05 Segurança** | a seção 2 dizia "oito camadas". Agora sabemos que a camada de tela **cobre 5% das telas** e a multiempresa tem **um valor distinto**. A conclusão fica mais dura: não são oito camadas fracas, são **duas que funcionam parcialmente e seis decorativas** |
| **06 Plano** | a fase 0 precisa de uma tarefa nova: **conferir o contrato de licenças** (5 módulos pagos sem versão instalada) |

---

## 5. O que o CRM novo faz diferente — ponto a ponto

| Achado no Vórtice | No CRM Tracbel |
|---|---|
| 4 executáveis, 5 gerações de versão | **1 aplicação**, versão declarada num arquivo só |
| 184 telas, 108 delas de configuração | **5 destinos** + administração por permissão |
| 51% dos usuários sem política | **impossível**: sem conjunto de permissão, o usuário não acessa nada |
| Permissão de tela em 10 de 184 | permissão por **entidade + verbo**, aplicada em todas |
| `NROEMPRESA` com 1 valor distinto | `EmpresaId` é raiz do escopo, com padrão restritivo |
| Sem coluna de inativação | `EstaAtivo` + `DesativadoEm`; nada é apagado |
| 3 colunas de senha, sem salt | **zero colunas de senha** — autenticação no Entra ID |
| Mobile Lite sem política própria | a permissão é a mesma para web e mobile — uma fonte de verdade |
| 41 fluxos marcados ativos e mortos | `UltimoUsoEm` + job mensal de higienização |

---

## 6. As consultas

```sql
-- Licenças e deriva de versão
SELECT Sistema, Modulo, SiglaModulo, Descricao, Versao, TipoAcesso
FROM GE_Modulo WITH (NOLOCK)
WHERE SiglaModulo IS NOT NULL AND SiglaModulo <> ''
ORDER BY Sistema, SiglaModulo;

-- Telas por módulo
SELECT Sistema, Modulo, COUNT(*) AS Telas
FROM GE_Aplicacao WITH (NOLOCK) GROUP BY Sistema, Modulo ORDER BY Telas DESC;

-- Usuários por política, com atividade real
SELECT p.SeqPolSeg, p.Politica, COUNT(u.SeqUsuario) AS Usuarios,
       SUM(CASE WHEN u.DTALOGIN IS NULL THEN 1 ELSE 0 END) AS NuncaLogaram,
       SUM(CASE WHEN u.DTALOGIN >= '2026-06-01' THEN 1 ELSE 0 END) AS Ativos90d
FROM GE_PolSeg p WITH (NOLOCK)
LEFT JOIN GE_Usuario u WITH (NOLOCK) ON u.SeqPolSeg = p.SeqPolSeg
GROUP BY p.SeqPolSeg, p.Politica ORDER BY Usuarios DESC;

-- Cobertura da permissão por tela
SELECT SEQPOLSEG, COUNT(*) AS Linhas, COUNT(DISTINCT CODAPLICACAO) AS Telas,
       COUNT(DISTINCT NROEMPRESA) AS Empresas
FROM GE_POLSEGPERM WITH (NOLOCK) GROUP BY SEQPOLSEG ORDER BY Linhas DESC;

-- Fluxos: marcados ativos x realmente usados
SELECT CASE WHEN cp.EmUso = 1 THEN 'ATIVO' ELSE 'inativo' END AS Marcacao,
       CASE WHEN u.Proc2026 > 0 THEN 'com uso 2026' ELSE 'sem uso' END AS Uso,
       COUNT(*) AS Fluxos
FROM IV_CodProcesso cp WITH (NOLOCK)
LEFT JOIN (SELECT CodProcesso, COUNT(DISTINCT Processo) AS Proc2026
           FROM IV_Agenda WITH (NOLOCK) WHERE DtaAgenda >= '2026-01-01'
           GROUP BY CodProcesso) u ON u.CodProcesso = cp.CodProcesso
GROUP BY CASE WHEN cp.EmUso = 1 THEN 'ATIVO' ELSE 'inativo' END,
         CASE WHEN u.Proc2026 > 0 THEN 'com uso 2026' ELSE 'sem uso' END;
```

---

## 7. O que ainda não apurei

| Tema | Por que não fechei |
|---|---|
| Conteúdo dos 68 itens de `CRM_M001` | vale um levantamento próprio — é o que define o comportamento do CEN |
| `GE_UsuarioPerm` (ACL polimórfica) | a pesquisa registrou 261 sujeitos órfãos; não reconferi |
| Escala numérica de permissão (0/2/3/4/5/6) | documentada na pesquisa, não revalidada |
| Custo real das licenças | não está no banco — só no contrato |
| Telas de `CRM_M004` (Configurador) | 44 telas sem nenhum item de política; entender o que controla o acesso a elas |
