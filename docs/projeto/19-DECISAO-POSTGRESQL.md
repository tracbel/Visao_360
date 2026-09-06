# Decisão de banco: PostgreSQL — CRM Tracbel

> ## ⛔ SUBSTITUÍDO — o banco é SQL Server
>
> **Este documento foi substituído pelo [20-DECISAO-SQL-SERVER](20-DECISAO-SQL-SERVER.md), em
> 04/09/2026, no mesmo dia em que foi escrito.** O banco do CRM é SQL Server, criado como um banco
> novo dentro da instância que já hospeda o Vórtice. Nada abaixo descreve o estado do código.
>
> **O conteúdo continua aqui de propósito, e não é histórico morto.** A análise deste documento —
> os números de licença, a comparação de recursos, as opções de produção, a rota de reversão — foi
> o que tornou possível decidir de novo com base em fatos, e é o que torna a decisão 20 auditável
> em vez de arbitrária. O que mudou não foi um argumento técnico: foi a resposta às perguntas 1 e 2
> da **seção 9 abaixo**, que este próprio documento já apontava como as que poderiam reabrir a
> conversa — o cenário marcado em 🔴 na seção 3 aconteceu. O histórico honesto dessa ida e volta
> está na [seção 2 do documento 20](20-DECISAO-SQL-SERVER.md).
>
> Leia abaixo para entender **por que fomos ao PostgreSQL**; leia o documento 20 para saber **o que
> o banco é hoje**.

---

> Documento 19 · Versão 1.0 · 04/09/2026
> **Status: SUBSTITUÍDO pelo documento 20 em 04/09/2026.** Texto original preservado a partir daqui.
> ~~Decisão tomada, implementação já iniciada em paralelo à escrita deste documento.~~
> Reversível a baixo custo até o fim da fase 1 (seção 8) — as confirmações da seção 9 não bloqueiam a
> decisão, mas precisam de resposta antes que ela vire irreversível na prática.
> Este documento **substitui** a linha "Banco" da tabela do [documento 03, seção 10](03-ARQUITETURA.md)
> e o cabeçalho "Banco: SQL Server 2019+" do [documento 04](04-MODELO-DADOS.md). As edições pontuais nos
> documentos 03, 04, 12, 13, 14 e 18 estão listadas na seção 4 — este documento **não as aplica**.
>
> **Estado do código nesta data:** `Tracbel.Crm.Infraestrutura.csproj` ainda referencia
> `Microsoft.EntityFrameworkCore.SqlServer` e a fixture de teste de arquitetura
> (`tests/Tracbel.Crm.Arquitetura.Testes/Banco/ModeloBanco.cs`) ainda chama `UseSqlServer(...)`. A
> troca do provedor é o primeiro passo de implementação desta decisão, e ainda não foi feita no
> momento em que este documento foi escrito — registrado aqui para que ninguém leia "implementação já
> começou" como "o provedor já foi trocado".

---

## 1. A decisão em cinco linhas

1. O banco do CRM próprio da Tracbel (`tracbel_crm`) é **PostgreSQL**, não SQL Server — decisão
   tomada, implementação já em andamento.
2. O motivo central não é um capricho técnico isolado: é a mesma tese do
   [documento 12](12-DECISAO-CONTAINERS.md) levada até o fim — portabilidade real entre a Algar, a
   Azure e qualquer outro provedor — só que agora sem a exceção que o SQL Server abriu no container
   de produção.
3. PostgreSQL também resolve, com `jsonb` e índices que o SQL Server não tem de forma nativa, dois
   problemas medidos no próprio Vórtice: **175 tabelas físicas de formulário** e **seis tabelas de
   busca materializada** somando 1,3 milhão de linhas que não guardam fato — guardam um jeito de
   procurar (seção 2).
4. O preço, sem rodeio: o time e o parceiro de infraestrutura operam SQL Server hoje, não Postgres, e
   a licença que a Tracbel já paga fica sem uso nesta base nova (seção 3). Nenhum dos dois é fatal;
   nenhum é gratuito.
5. A reversão para SQL Server é barata até o fim da fase 1 e fica cara depois — porque é exatamente
   aí que o modelo passa a depender de `jsonb` e de particionamento de verdade (seção 8).

---

## 2. Por que PostgreSQL

### 2.1 Sem custo de licença por núcleo

O SQL Server é licenciado por núcleo. Preço de tabela público da Microsoft para SQL Server 2022
Standard Edition, consultado em 04/09/2026 (não é o que a Tracbel pagaria, é a referência pública de
mercado): **US$ 3.945 por pacote de 2 núcleos**, com mínimo de 4 núcleos por instância. Uma única
instância de 8 vCPUs — o mesmo dimensionamento que o [doc 12, seção 16](12-DECISAO-CONTAINERS.md) usa
para a VM de banco — exigiria 4 pacotes: **≈ US$ 15.780 só de licença perpétua** *(estimativa a partir
de preço de tabela público, câmbio e desconto corporativo não incluídos)*, sem contar a manutenção
anual (Software Assurance) nem a diferença para Enterprise Edition, que é várias vezes maior. O Vórtice
roda em **SQL Server 2019 Standard**
(`docs/extracao-vortice/00-RELATORIO-EXTRACAO.md`, cabeçalho), então esse é o patamar de referência
real, não um extremo hipotético. PostgreSQL custa **R$ 0** nessa linha, para qualquer número de
núcleos, em qualquer instância, para sempre.

### 2.2 Roda nativo em container Linux — fecha a exceção que o próprio doc 12 abriu

O [doc 12, seção 5](12-DECISAO-CONTAINERS.md) foi categórico: *"container é bom em ser descartável.
Banco é o oposto."* — e por isso o SQL Server ficou **fora** do container em produção, com uma lista de
seis motivos (Estado, Backup, Alta disponibilidade, **Licença**, Experiência do parceiro, Suporte).
PostgreSQL não elimina o motivo "Estado" — banco continua sendo o que não pode ser descartável, e a
seção 7 deste documento trata isso com o mesmo peso que o doc 12 deu ao SQL Server. Mas elimina **dois**
dos seis motivos por completo:

- **Licença** deixa de existir como variável: não há regra de mobilidade de núcleo, não há
  `Software Assurance`, não há pergunta de contrato para o item 10 da seção 19 do doc 12.
- **Windows container** deixa de ser sequer uma opção descartada: PostgreSQL roda na mesma imagem
  base Linux (`postgres:16-alpine` ou equivalente) que todo o resto da stack já decidiu usar
  (doc 12, seção 7, "Windows containers: não").

O resultado prático: a exceção "tudo em container, menos o banco" deixa de ser uma amarração
estrutural do motor e passa a ser **só** uma decisão operacional sobre maturidade de backup/HA para um
time de três pessoas — a mesma decisão, sem a variável de licença. Isso não muda a recomendação da
seção 7 (que continua sendo cautelosa com container autogerenciado em produção), mas muda **por que**
ela é cautelosa, e isso importa para a diretoria: o argumento deixa de ser "o SQL Server não pode" e
passa a ser "ainda não vale o risco para três pessoas" — uma frase reversível, não uma parede.

### 2.3 Portabilidade real — o argumento que vai à diretoria

O [doc 12, seção 3](12-DECISAO-CONTAINERS.md) definiu a frase que sustenta o programa inteiro perante a
diretoria: *"a mesma imagem que roda na máquina do desenvolvedor roda no datacenter da Algar, na Azure
ou em qualquer outro provedor, sem runbook de instalação."* Enquanto o banco ficava de fora dessa
imagem — por ser SQL Server, com licença amarrada a host e edição —, a portabilidade prometida tinha um
buraco exatamente na peça mais crítica do sistema: o dado. PostgreSQL fecha esse buraco. A escolha de
onde hospedar o banco também vira **decisão de contrato**, não de arquitetura: sai da Algar, entra na
Azure, entra em qualquer nuvem que ofereça PostgreSQL gerenciado (todas as três grandes oferecem), sem
reescrever uma linha de acesso a dado — porque o Npgsql/EF Core não muda com o provedor de
hospedagem. É o mesmo argumento do doc 12, agora sem exceção.

### 2.4 `jsonb` — os dois problemas que ele resolve com elegância

Este é o argumento técnico mais forte, porque resolve um problema **já medido**, não hipotético.

**Campos personalizados sem release** (`metadado.CampoPersonalizado`, doc 04 seção 11). A decisão já
tomada foi não copiar o motor EAV do Salesforce — campo customizado vira coluna real. Isso continua
valendo para atributo **estruturado e consultado com frequência**. Mas para o resto — parâmetro
esparso, configuração de efeito de regra (`wf.Regra.EfeitoParametros`, já `nvarchar(max)` guardando
JSON no doc 04), payload de integração recebido cru (`crm.Lead.PayloadOriginal`) — `jsonb` é o tipo
certo: binário, indexável, validável por `CHECK` com operadores JSON, sem a rigidez de uma coluna nova
por atributo nem a fragilidade de um `nvarchar(max)` que ninguém consegue indexar de verdade.

**Respostas de formulário** — o caso mais forte de todos, e já medido em detalhe no
[doc 17, seção 4](17-MODELO-UNIFICADO.md). O Vórtice materializa **uma tabela física por
formulário**: **175 tabelas `IV_Q_*`**, 2.601 colunas somadas, a maior com 104 colunas, e mais
`IV_Formulario`/`IV_Questao`/`IV_QuestaoLista`/`IV_Questionario` — **179 tabelas no total**. O
custo medido: publicar formulário é `ALTER TABLE`; variação de negócio vira tabela nova
(`IV_Q_GESTAO_CREDITO`/`_IMP`/`_AMS`, `IV_Q_COMPETIDORES_NA_NEG` a uma letra de
`IV_Q_COMPETIDORES_NA_NEGO`); 125 das 196 regras de negócio gravadas como SQL solto em
`GE_ObjDinamico` dependem diretamente dessas tabelas físicas; **32 delas nunca receberam uma resposta
sequer**; e uma pergunta transversal como "quantas vendas perdemos por preço em 2026?" exige `UNION`
manual sobre 14 tabelas com nomes de coluna diferentes (doc 17, seção 4.5).

O modelo já desenhado no doc 17 (`metadado.Formulario` → `Pergunta` → `Preenchimento` → `Resposta`,
**179 tabelas viram 4**) resolve a explosão de tabelas mesmo em SQL Server, com colunas tipadas
(`ValorTexto`, `ValorNumero`, `ValorData`, `ValorBooleano`, `CatalogoItemId`) — mas isso ainda é uma
forma de EAV com uma coluna por tipo, que cresce toda vez que um tipo de resposta novo aparecer
(lista múltipla, arquivo anexado, coordenada). `jsonb` permite guardar o valor tipado da resposta como
documento (`{"tipo": "lista_multipla", "itens": [12, 47]}`) **sem alterar `metadado.Resposta`** quando
um tipo de pergunta novo nascer, com índice GIN sobre o próprio documento
(`CREATE INDEX ... USING GIN (valor)`) para consultar por chave/valor sem esperar um `ALTER TABLE`.
Publicar formulário continua sendo `INSERT`, exatamente como o doc 17 já prometia — só que a resposta
em si também ganha a mesma elasticidade que a definição já tinha.

### 2.5 Particionamento nativo e maduro — log e auditoria

Medido: **logs consomem 42% do banco do Vórtice** (13,7 GB de 32,4 GB —
`docs/extracao-vortice/00-RELATORIO-EXTRACAO.md`, achado 11), sem nenhuma política de retenção;
`GE_LgTb` tem 11,9 milhões de linhas paradas desde 2023 porque trocaram de mecanismo de log e nunca
expurgaram o anterior. O [doc 14, seção 10](14-PADRAO-DE-BANCO.md) já **decidiu** a retenção para o
banco novo — `processo.RegraExecucao` com partição mensal e 12 meses online;
`auditoria.AlteracaoCampo` 18 meses; `auditoria.EventoAcesso` 24 meses — mas isso pressupõe uma
ferramenta de particionamento que o time consiga operar sem virar mais um passivo de manutenção
manual.

O particionamento declarativo do PostgreSQL (`PARTITION BY RANGE`, maduro desde a versão 11) faz
exatamente isso: uma partição por mês, criada com antecedência por um job simples (ou pela extensão
`pg_partman`, que automatiza criação e expurgo de partição por política), e uma consulta que filtra por
data só toca a partição certa (*partition pruning*) sem reescrever a aplicação. "Arquivar 12 meses" no
Postgres é `DETACH PARTITION` seguido de `DROP TABLE` na partição destacada — uma operação de metadado,
quase instantânea, contra um `DELETE` em massa que travaria a tabela inteira num banco sem
particionamento (exatamente o problema que fez o Vórtice nunca expurgar nada).

### 2.6 Índices parciais, por expressão e GIN — o que substitui tabela materializada

Aqui é importante ser preciso sobre o que já está resolvido e o que não está.

**Índice parcial** (`WHERE ExcluidoEm IS NULL`) o SQL Server já tem — é o índice filtrado, e o
[doc 04](04-MODELO-DADOS.md) já usa em quase toda tabela (`IX_Conta_Proprietario`,
`IX_Lead_Situacao`...). Não é um ganho exclusivo do Postgres; é continuidade.

O ganho de verdade está em **busca aproximada**, e o caso é o mais concreto do diagnóstico inteiro:
`GE_PessoaNomeFonema` (151.583 linhas, 4 colunas: `SeqPessoa`, `SeqContato`, `NomeRazao`, `NomePuro`) é
descrita no [doc 17, seção 2](17-MODELO-UNIFICADO.md) como um **índice materializado à mão** — existe
só para que quem digitar "salvadr arena" encontre "Salvador Arena". Ao lado dela, `GE_PessoaFonema`
(455.993 linhas), `GE_PessoaSimilar` (124.222) e `IV_SelecaoPessoa` (582.481): **seis tabelas e 1,3
milhão de linhas que não guardam um fato — guardam um jeito de procurar** (doc 17, seção 3).

A hipótese mais provável — e é hipótese, não fato confirmado, porque não temos o motivo documentado
pelo fornecedor — é que o SQL Server não deixou alternativa melhor: ele tem `SOUNDEX`/`DIFFERENCE`
nativos, mas são algoritmos fonéticos pensados para nomes em inglês, mal ajustados a nomes
brasileiros, o que teria empurrado o fornecedor a construir sua própria tabela de fonemas à mão. O
`doc 17, seção 9` já propõe uma correção parcial em SQL Server — uma coluna computada persistida
`NomeNormalizado` com índice —, mas isso resolve só acento/maiúscula (comparação exata após
normalização); não resolve erro de digitação, que é o problema que `GE_PessoaFonema`/`GE_PessoaSimilar`
de fato atacam.

A extensão `pg_trgm` do PostgreSQL ataca o problema certo, direto no motor: um índice GIN de trigramas
sobre `comercial.Cliente.NomeNormalizado` responde exatamente a essa pergunta —
`SELECT * FROM comercial.cliente WHERE nome_normalizado % 'salvadr arena' ORDER BY
similarity(nome_normalizado, 'salvadr arena') DESC` — sem tabela adicional, sem trigger de manutenção,
sem job de recálculo, e sem depender de fonética de idioma nenhuma (mede similaridade de string, não
som). É a peça que faltava no desenho do doc 17: a coluna computada mata o problema de acento; o índice
GIN com `pg_trgm` mata o problema de digitação — e as duas juntas aposentam as seis tabelas por
completo.

### 2.7 `citext` e collation sem acento — o problema do nome, com uma ressalva honesta

O problema medido é real: o Vórtice usa `SQL_Latin1_General_CP1_CI_AS` — *case-insensitive*, mas
**accent-sensitive** —, então uma busca por "Jose" não encontra "José" (doc 14, seção 14, nota sobre
collation). Mas aqui a honestidade exige uma ressalva: **o SQL Server também resolve isso.** O
[doc 14, seção 14](14-PADRAO-DE-BANCO.md) já recomenda, para o banco novo em SQL Server,
`Latin1_General_100_CI_AI_SC_UTF8` (ou `SQL_Latin1_General_CP1_CI_AI` como alternativa) — uma collation
*accent-insensitive* nativa, que já resolveria o defeito medido. Este não é, portanto, um argumento de
"só o Postgres consegue"; é um argumento de **ergonomia e onde a regra mora**.

`citext` (tipo de coluna, não configuração de sessão) torna a insensibilidade a maiúscula/minúscula
parte do **tipo**, não de uma collation escolhida uma vez no banco inteiro ou lembrada coluna a coluna —
o mesmo princípio que o projeto já aplica em C# com o tipo de valor `Telefone` (doc 03, seção 2.3):
quem usa o tipo não consegue errar. Para o acento, o PostgreSQL 12+ permite uma **collation ICU não
determinística** por coluna (`CREATE COLLATION ... (provider = icu, locale = 'und-u-ks-level1',
deterministic = false)`), que ignora simultaneamente caixa e acento — mais granular que escolher uma
collation nomeada de banco inteiro, mas resolvendo o mesmo problema que o doc 14 já havia endereçado
para SQL Server. **Classificação honesta:** ganho real de ergonomia e de onde a garantia mora (no tipo
da coluna, não numa configuração que se esquece), não um problema novo resolvido.

### 2.8 Extensões úteis

- **`pg_trgm`** — tratado em 2.6, é o argumento forte.
- **`postgis`**, **se a geolocalização crescer** — hoje `wf.Atividade.Latitude`/`Longitude`
  (doc 04, seção 5.5) são só `decimal(10,7)` de armazenamento, sem consulta espacial nenhuma
  (nenhum "clientes num raio de X km", nenhuma otimização de rota de visita). Não há problema medido
  hoje que o PostGIS resolva — é otimismo justificado, não evidência: se a Tracbel um dia quiser
  otimizar rota de CEN em campo ou segmentar carteira por proximidade geográfica, o PostGIS é o padrão
  de mercado maduro para isso, e o SQL Server tem um suporte espacial (`geography`/`geometry`)
  comparativamente menos investido nas versões recentes. **Classificação honesta:** opcionalidade de
  baixo custo para o futuro, não um argumento desta decisão.

### 2.9 Ecossistema de migração e observabilidade em container

O [doc 12, seção 12](12-DECISAO-CONTAINERS.md) já decidiu OpenTelemetry → Prometheus/Grafana/Loki/
Tempo. `postgres_exporter` é um dos exportadores Prometheus mais maduros e usados do mercado — plugue e
use na mesma stack já decidida. O equivalente para SQL Server no ecossistema open source é
comparativamente mais fino. Em desenvolvimento e CI, `Testcontainers.PostgreSql` sobe uma imagem
Alpine de dezenas de MB em 1–3 segundos, contra a imagem do SQL Server para Linux (mais de 1,5 GB,
15–30 segundos de partida) — o mesmo padrão de teste que o [doc 03, seção 8](03-ARQUITETURA.md) e o
[doc 12, seção 5.1](12-DECISAO-CONTAINERS.md) já previam, só que mais rápido em cada uma das centenas
de execuções que 79 semanas de programa vão acumular no CI.

---

## 3. O que perdemos

Sem enfeite — se esta seção só listasse vantagens, a seção 2 seria propaganda, não decisão.

| O que perdemos | Peso real |
|---|---|
| **O time e o parceiro de infraestrutura operam SQL Server, não Postgres** | Real. Nenhum dos planos individuais (doc 07) lista Postgres. A curva de aprendizado se soma à de Docker/Linux, já estimada em 1–2 semanas na fase 0 (doc 12, seção 6.2) — exatamente a fase mais cara para atrasar (doc 13, seção 3, "fase 0... é a pior de todas para atrasar"). Mitigação: um serviço gerenciado (seção 7) tira a maior parte da carga operacional; o dia a dia de LINQ/EF Core muda pouco entre provedores |
| **O Vórtice é SQL Server** — ler o legado direto deixa de ser trivial | Menor do que parece: a arquitetura **já proíbe** ler o legado direto. A fronteira é a camada de integração (`Tracbel.Crm.Integracao/Vortice`, doc 03, seção 1), e o acesso ao Vórtice hoje é via login `CRM_Leitura` dedicado, não *linked server* nem acesso direto do motor novo. Trocar de motor não abre nem fecha essa porta — ela já estava fechada |
| **Ferramentas que o time conhece mudam** — SSMS dá lugar a pgAdmin/DBeaver/`psql` | Real, mas raso. É uma curva de dias, não de semanas, para quem já lê SQL — e o time inteiro (3 pessoas) precisa aprender de qualquer forma a operar Linux e containers, que é uma curva bem maior (doc 12, seção 6.2) |
| **A licença de SQL Server que a Tracbel já paga não é aproveitada** | Real, e é o ponto mais delicado desta decisão — ver o destaque abaixo |
| **Contratação e suporte externo em Postgres pode ser mais difícil na região** | Incerto, marcado como tal. Talento Postgres é abundante no mercado brasileiro em geral (a base de fintechs e SaaS que cresceu na última década é majoritariamente Postgres), mas um parceiro de infraestrutura regional já calibrado para o ecossistema Microsoft (como a Algar parece ser, pelo doc 12) pode não ter Postgres como competência instalada — é exatamente a pergunta 2 da seção 9, ainda sem resposta |

> 🔴 **O argumento mais forte contra esta decisão, e o único que eu recomendaria pesar de verdade:**
> se a resposta à pergunta 1 da seção 9 vier **"sim, existe um Enterprise Agreement corporativo com a
> Microsoft que já cobre SQL Server/Azure SQL para qualquer sistema novo, sem custo marginal
> perceptível"**, então o argumento de custo de licença da seção 2.1 — hoje o mais fácil de defender
> perante a diretoria — desaparece, e o que resta é decidir se `jsonb`/particionamento/`pg_trgm`
> (seções 2.4 a 2.6, tecnicamente reais, mas contornáveis em SQL Server com mais código) valem, sozinhos,
> a curva de aprendizado da seção 3. Essa é a única combinação de fatos que eu vejo capaz de justificar
> reabrir esta decisão — e é por isso que a pergunta 1 da seção 9 é a primeira da lista, não a última.

Um segundo ponto, técnico e menos citado, também merece destaque aqui porque é uma tensão real com o
argumento de portabilidade da seção 2.3: **`jsonb` é, ele mesmo, uma feature específica de provedor.**
O EF Core abstrai `INSERT`/`UPDATE`/consulta LINQ entre provedores; **não** abstrai os operadores
`jsonb` (`@>`, `?`, `#>>`, índice GIN) usados diretamente em SQL ou via `EF.Functions`. Quanto mais o
código de aplicação depender desses operadores fora do schema `metadado`, mais essa fatia específica
deixa de ser portável — é o mesmo raciocínio de quarentena que o doc 03, seção 1, já aplica ao
vocabulário do Vórtice, e a seção 8 deste documento trata como regra explícita.

---

## 4. Onde isso muda os documentos

**Edições recomendadas, não aplicadas.** Cada uma segue o mesmo formato do
[doc 12, seção 17](12-DECISAO-CONTAINERS.md): onde, e a frase nova sugerida.

### `03-ARQUITETURA.md`

| # | Onde | Edição recomendada |
|---|---|---|
| 1 | Cabeçalho, linha "Stack" | trocar `SQL Server 2019` por `PostgreSQL 16+` |
| 2 | Seção 10, linha "Banco" | trocar `SQL Server 2019, database TRACBEL_CRM — licença já existe; EF Core é maduro` por `PostgreSQL 16+, database tracbel_crm (nomes em snake_case, ver doc 19 seção 5) — sem custo de licença por núcleo; EF Core + Npgsql; jsonb, particionamento e GIN resolvem casos que o Vórtice resolveu com tabela física — ver doc 19` |
| 3 | Seção 8, tabela de testes, linha "Aplicação" | `Testcontainers (SQL Server)` vira `Testcontainers (PostgreSQL)` |
| 4 | Seção 10, tabela | acrescentar nota de remissão: *"A escolha do motor de banco e o porquê estão decididos no documento 19."* |

### `04-MODELO-DADOS.md`

| # | Onde | Edição recomendada |
|---|---|---|
| 5 | Cabeçalho | trocar `Banco: SQL Server 2019+, database próprio TRACBEL_CRM` por `Banco: PostgreSQL 16+, database próprio tracbel_crm` |
| 6 | Seção 1.2 (tabela de tipos) | substituir pela tabela de equivalência completa da seção 6 deste documento |
| 7 | Seção 1.3 (bloco de colunas obrigatórias) | a linha `Versao rowversion NOT NULL — gerenciado pelo SQL Server` passa a `Versao — sem coluna física; concorrência otimista mapeada para a coluna de sistema xmin via UseXminAsConcurrencyToken() do Npgsql (ver doc 19, seção 6)` |
| 8 | Todo o documento | identificadores de exemplo em PascalCase (`crm.Conta`, `ChavePublica`...) são ilustrativos do *modelo conceitual*; o nome físico no banco segue a convenção snake_case do doc 19, seção 5 — recomenda-se uma nota no topo do documento remetendo lá |

### `12-DECISAO-CONTAINERS.md`

| # | Onde | Edição recomendada |
|---|---|---|
| 9 | Seção 5, título | de `Por que SQL Server fica FORA de container em produção` para `Por que o banco ainda fica fora de container autogerenciado em produção` — a razão deixa de ser licença/edição e passa a ser maturidade operacional de estado para um time de três (ver doc 19, seção 7) |
| 10 | Seção 5, tabela | remover a linha "Licença" (deixou de se aplicar); manter Estado, Backup, Alta disponibilidade, Experiência do parceiro e Suporte, agora genéricas a qualquer motor |
| 11 | Seção 9 (diagramas de topologia) | rótulos `SQL Server` viram `PostgreSQL`; a caixa "FORA DE CONTAINER" some ou vira "VM dedicada ou serviço gerenciado — ver doc 19, seção 7", já que container deixou de estar tecnicamente descartado, só não recomendado ainda |
| 12 | Seção 16 (custo) | linha `1 × VM SQL Server... licença já existente` vira `1 × VM PostgreSQL` (mesma faixa R$ 2.500–5.000, já que o custo ali já era só a VM); linha `Azure SQL Managed Instance (General Purpose)` vira `Azure Database for PostgreSQL Flexible Server (General Purpose)`, faixa revista para R$ 3.500–11.000 *(estimativa, doc 19 seção 7)* em vez de R$ 8.000–20.000 |

### `13-PROGRAMA-POR-FASES-DIRETORIA.md`

| # | Onde | Edição recomendada |
|---|---|---|
| 13 | Seção 6.2 (Licenças) | linha `SQL Server — licença já existe — doc 03, seção 10` vira `PostgreSQL — sem custo de licença (software livre) — doc 19`; a pergunta "a licença de SQL Server sobra de fato?" (antes uma resposta) vira uma pergunta em aberto só relevante se a decisão for revista (seção 9, item 3, deste documento) |
| 14 | Seção 6.1 e 6.3 (totais) | degrau 1 não muda (a estimativa já assumia licença gratuita); onde o texto cita o degrau 2 (Azure SQL MI), referenciar a faixa revista do doc 12 edição 12 |

### `14-PADRAO-DE-BANCO.md`

| # | Onde | Edição recomendada |
|---|---|---|
| 15 | Seção 3, testes `Nome_de_tabela_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore` e `Nome_de_coluna_...` | 🔴 **contradição direta** — o regex atual (`^[A-Z][A-Za-z0-9]*$`) **proíbe** underscore; a convenção decidida no doc 19, seção 5, **exige** snake_case. Os dois testes precisam ser reescritos para exigir `^[a-z][a-z0-9_]*$` (sem acento) antes de qualquer migration real ser gerada — hoje `find src -ipath "*Migration*"` não retorna nada (doc 14, seção 2.3), então a janela ainda está aberta |
| 16 | Seção 3, "Prefixo de constraint" | `PK_`, `FK_`, `IX_`, `UX_`, `CK_` viram minúsculos — `pk_`, `fk_`, `ix_`, `ux_`, `ck_` — para não reintroduzir letra maiúscula em identificador não citado, o problema exato que motiva a seção 5 deste documento |
| 17 | Seção 4, regra "Proibido `HasColumnType(...)` literal" | ganha uma exceção explícita na mesma mecânica de allowlist já usada para `TextoIlimitadoJustificado`: mapear `jsonb` no Npgsql exige `.HasColumnType("jsonb")` literal (não há atalho fluente equivalente a `IsUnicode()`); a lista de exceção passa a se chamar algo como `TipoLiteralJustificado`, cobrindo as colunas descritas no doc 19, seção 2.4 |
| 18 | Seção 9, regra 9.5 | "nível de compatibilidade e collation nascem num script de criação versionado" continua valendo; troca-se a referência de collations SQL Server (`Latin1_General_100_CI_AI_SC_UTF8`) pela decisão de collation/locale ICU do PostgreSQL (doc 19, seção 2.7) |
| 19 | Seção 14, nota sobre collation | a "recomendação, não travada" vira **decisão travada**: `citext` para comparação sem caixa + collation ICU não determinística (`und-u-ks-level1`) para busca sem acento — ver doc 19, seção 2.7 |

### `18-INTEGRACAO-PROTHEUS.md`

| # | Onde | Edição recomendada |
|---|---|---|
| 20 | Seção 4.3, recomendação | acrescentar uma frase: *"Com PostgreSQL no CRM novo, a opção (a) — linked server — deixa de ser apenas não recomendada e passa a não ter equivalente direto: não existe no PostgreSQL um recurso nativo equivalente ao MSDASQL/OLE DB linked server do SQL Server para consultar outro banco em tempo real com a mesma transparência. Isso fecha, por construção, a tentação de usar (a) como atalho — reforça, não muda, a recomendação já dada pela opção (c)."* |
| 21 | Seção 4.2, linha "(b) Réplica de leitura" | acrescentar nota: se o Protheus rodar em SQL Server (pergunta 7, seção 4.4, ainda sem resposta) e o CRM novo for PostgreSQL, uma futura réplica de leitura exigiria uma ferramenta de CDC heterogênea (ex.: Debezium lendo CDC do SQL Server e publicando no Postgres) em vez de replicação nativa homogênea — o que já era "pesado demais para um time de três" independentemente do motor do CRM (seção 4.3); a conclusão do documento não muda, fica mais explicitamente justificada |

---

## 5. Convenção de identificadores

**Decisão:** no banco, os nomes de schema, tabela e coluna são **snake_case, em português** —
`comercial.cliente`, `processo.interacao`, `integracao.ponto_de_sincronismo`, `criado_em`,
`data_inclusao`. As **palavras** continuam exatamente as do
[doc 15, seção 2 e 3](15-GLOSSARIO-E-NOMES.md) — esta decisão muda **a forma** do identificador, não o
vocabulário de negócio, que já é o requisito mais antigo e mais repetido do Ricardo no projeto ("nossas
tabelas com nomes legíveis que entendemos", doc 15, cabeçalho).

### Por que — o motivo é técnico, não estético

O PostgreSQL **dobra para minúsculo** todo identificador **não citado** (regra do padrão SQL que o
Postgres implementa ao pé da letra, diferente de outros bancos): `CREATE TABLE Cliente (...)` cria,
silenciosamente, uma tabela chamada `cliente`. Isso por si só não quebra nada — `SELECT * FROM Cliente`
também dobra para `cliente` e funciona. O problema aparece no instante em que **qualquer** ferramenta
grava o nome **entre aspas duplas** (`"Cliente"`): aspas preservam a caixa exatamente como escrita, e
criam um identificador **diferente e sensível a maiúscula**, coexistindo no mesmo schema com o `cliente`
minúsculo. A partir daí, toda consulta escrita por fora do EF Core — um DBA no `psql`, um relatório
ad hoc, uma migration escrita à mão, um `pg_dump`/`pg_restore`, uma ferramenta de BI — precisa lembrar,
para sempre, se aquele nome específico exige aspas e com qual caixa exata. Errar uma vez produz
`relation "cliente" does not exist` na melhor hipótese, ou, na pior, uma consulta que silenciosamente
acerta um objeto homônimo errado.

É exatamente o problema que o requisito de nomes legíveis do doc 15 já tentava evitar por outro ângulo
— e aqui a solução é a mesma filosofia: **escolher a convenção que nunca precisa de aspas.**
Identificador em snake_case minúsculo, sem acento, nunca colide com a regra de dobra do Postgres —
escrito com aspas ou sem aspas, citado ou não citado, dá sempre no mesmo objeto. Citar tudo sempre
funcionaria também, mas exige disciplina permanente de todo mundo que tocar o banco, para sempre; não
citar nunca, com nomes já minúsculos, funciona por construção e não depende de ninguém lembrar de
nada — a mesma razão pela qual o doc 04 prefere tipo de valor a validação lembrada (`Telefone.Criar` em
vez de "lembrar de validar o telefone toda vez").

### C# continua PascalCase — a ponte é uma convenção do EF Core, não disciplina manual

O código C# **não muda**: entidades, propriedades e DbContext continuam PascalCase
(`comercial.Cliente`, `ChavePublica`, `CriadoEm`), exatamente como o doc 03 e o doc 04 já escrevem. A
tradução para snake_case no banco é automática, via o pacote **`EFCore.NamingConventions`**
(mantido pelo mesmo autor do provedor Npgsql), habilitado uma vez no `DbContextOptionsBuilder`:

```csharp
optionsBuilder
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention();
```

A partir daí, toda migration gerada por `dotnet ef migrations add` já sai em snake_case, sem que
nenhum dev precise escrever `HasColumnName("cliente")` à mão em cada propriedade — o mesmo espírito do
doc 14, seção 3 ("previsibilidade... sem abrir o código"), só que a conversão acontece uma vez, no
bootstrap, e vale para o modelo inteiro.

### De-para — meia dúzia de exemplos, com origem

| Nome conceitual (docs 03/04, PascalCase) | Nome no banco (PostgreSQL, snake_case) | Origem |
|---|---|---|
| `comercial.Cliente` | `comercial.cliente` | doc 15, seção 3.1 |
| `processo.Interacao` | `processo.interacao` | doc 15, seção 3.2 |
| `intg.Watermark` → `integracao.PontoDeSincronismo` | `integracao.ponto_de_sincronismo` | doc 04, seção 8 → doc 15, seção 4 (já corrigia um nome em inglês) → esta decisão (só a forma) |
| `org.HierarquiaVendas` → `organizacao.HierarquiaComercial` | `organizacao.hierarquia_comercial` | doc 04, seção 2.4 → doc 15, seção 4 |
| `crm.ContaCarteira` → `comercial.ClienteCarteira` | `comercial.cliente_carteira` | doc 04, seção 3.5 → doc 15, seção 4 |
| `CriadoEm` / `AlteradoPorId` / `ChavePublica` (colunas de auditoria, todas as tabelas) | `criado_em` / `alterado_por_id` / `chave_publica` | doc 04, seção 1.3 |

Os prefixos de constraint do [doc 14, seção 3](14-PADRAO-DE-BANCO.md) seguem minúsculos pela mesma
regra — `pk_cliente`, `fk_cliente_empresa_empresa_id`, `ix_cliente_proprietario_id`,
`ux_lead_chave_publica`, `ck_conta_tipo_pessoa` — ver a edição 16 da seção 4.

---

## 6. Equivalência de tipos

Tabela de equivalência para substituir o doc 04, seção 1.2. As oito categorias pedidas, mais duas
observações que valem a pena registrar junto por serem armadilhas concretas, não só troca de nome.

| Categoria | Tipo no doc 14 (SQL Server) | Tipo no PostgreSQL | Observação |
|---|---|---|---|
| Texto limitado | `nvarchar(n)` / `varchar(n)` com `HasMaxLength` | `varchar(n)` | idêntico em espírito; Postgres usa UTF-8 por padrão, então não existe a distinção `varchar`/`nvarchar` do SQL Server — um único tipo de texto para tudo |
| Texto sem limite (só na lista de exceção, doc 14 seção 4) | `nvarchar(max)` | `text` | mesma exceção documentada (`Lead.PayloadOriginal`, `Regra.EfeitoParametros`) — candidatos naturais a `jsonb` numa iteração futura, não decidido aqui |
| Dinheiro | `decimal(18,2)` | `numeric(18,2)` | mesma semântica exata: precisão arbitrária, sem arredondamento silencioso |
| Data e hora em UTC | `datetime2(3)` | `timestamptz` (não `timestamp` sem fuso) | o Postgres normaliza internamente para UTC e converte na exibição pela sessão; usar `timestamp` sem fuso é o erro mais comum de quem migra para Postgres e precisa ser barrado por revisão de PR, igual à regra do doc 14 seção 4 |
| Data pura | `date` | `date` | idêntico |
| Booleano | `bit NOT NULL`, nunca `bit?` | `boolean NOT NULL` | Postgres tem tipo booleano nativo de verdade — `bit` no SQL Server é um número disfarçado; a regra "nunca anulável" do doc 14 seção 4 continua igual |
| Chave interna | `bigint IDENTITY(1,1)` | `bigint GENERATED BY DEFAULT AS IDENTITY` | sintaxe diferente, semântica igual: sequencial, gerado pelo banco, nunca exposto |
| Identificador público | `uniqueidentifier DEFAULT NEWID()` | `uuid DEFAULT gen_random_uuid()` | `gen_random_uuid()` é nativo desde o PostgreSQL 13 (antes exigia a extensão `pgcrypto`) |
| JSON (`metadado.CampoPersonalizado`, `metadado.Resposta`) | não existia — seria `nvarchar(max)` | `jsonb` + índice GIN | seção 2.4 |
| Enumerado (domínio fechado) | `varchar(n)` + `CHECK` | `varchar(n)` + `CHECK` — **não** usar `CREATE TYPE ... AS ENUM` nativo | manter a mesma abordagem é o que preserva a rota de reversão da seção 8; o tipo `ENUM` nativo do Postgres não tem equivalente direto no SQL Server e dificulta migration (adicionar valor exige `ALTER TYPE`, historicamente não transacional) |
| *(extra)* Controle de concorrência (`Versao`) | `rowversion`, gerenciado pelo SQL Server | sem coluna física — mapeado para a coluna de sistema `xmin`, via `.UseXminAsConcurrencyToken()` do Npgsql | é a única linha do bloco de auditoria (doc 04, seção 1.3) que muda de **mecanismo**, não só de nome de tipo — vale nota própria na migration |
| *(extra)* `char(1)` fixo com domínio (ex.: `TipoPessoa`) | `char(1)` + `CHECK` | `char(1)` + `CHECK` | idêntico — a regra do doc 14 seção 13 ("char(1) sem CHECK constraint" é proibição) continua igual |

---

## 7. Como fica a produção

Três opções, os mesmos critérios que o [doc 12, seção 8](12-DECISAO-CONTAINERS.md) já usou para
comparar hospedagem: operação por um time de três, backup e ponto de recuperação, alta disponibilidade,
custo estimado *(faixas marcadas como estimativa)* e alinhamento com o doc 12.

| Critério | (a) Container autogerenciado pela equipe | (b) Azure Database for PostgreSQL Flexible Server | (c) VM Linux dedicada |
|---|---|---|---|
| Operação por um time de 3 | Ruim — monta e mantém disciplina de backup/HA do zero, empilhada sobre a curva de Docker/Linux que já é nova (doc 12, seção 6.2) | Ótima — backup, patch de versão menor, HA e failover são responsabilidade da Microsoft | Média — reaproveita o padrão de runbook que a Algar já teria para banco em VM, mas para um motor que ela ainda não confirma que opera (seção 9, item 2) |
| Backup e ponto de recuperação | Depende inteiramente de disciplina própria (`pgBackRest`/`WAL-G` + volume persistente) — exatamente o cenário que o doc 12, seção 5, já rejeitou para bancos com estado | Backup automático + restauração *point-in-time* nativa, período de retenção configurável | Backup por ferramenta externa (`pgBackRest`/`WAL-G`) rodando na própria VM — precisa do mesmo teste de restauração exigido no portão da fase 0 (doc 06, critério 7) |
| Alta disponibilidade | Território de nicho — réplica montada e testada à mão, sem tooling pronto | HA *zone-redundant* como opção de configuração, nativa | Precisa de segunda VM com replicação em streaming configurada e testada manualmente |
| Custo estimado (mensal) 🔶 *estimativa* | ≈ igual à opção (c), mais o tempo de time gasto em operação, que não aparece na fatura mas aparece no cronograma | **R$ 3.500 – R$ 11.000** *(estimativa derivada: SQL Managed Instance General Purpose equivalente custa R$ 8.000–20.000 no doc 12 seção 16, majoritariamente por licença embutida por núcleo; sem essa licença, o mesmo dimensionamento em Postgres fica tipicamente 45–60% mais barato — sem cotação real, consultar Azure Pricing Calculator antes de orçar)* | **R$ 2.500 – R$ 5.000** *(mesma faixa que o doc 12, seção 16, já estimou para a VM do SQL Server — porque aquele número já era só a VM, com a licença tratada como custo zero adicional)* |
| Alinhamento com o doc 12 | Contradiz a lógica da seção 5 do doc 12 — *"container para o que é descartável; VM ou serviço gerenciado para o que guarda estado"* — a razão de o SQL Server ficar fora do container era sobre **estado**, não sobre motor, e essa razão não muda com Postgres | Estende a mesma lógica do doc 12, seção 5: usar o serviço gerenciado quando o time não tem maturidade operacional própria — o mesmo raciocínio que já recomendaria Azure SQL Managed Instance no degrau 2 | É literalmente o mesmo desenho de topologia que o doc 12, seção 9, já desenhou para o SQL Server, só trocando o motor dentro da mesma caixa |

### Recomendação

**Azure Database for PostgreSQL Flexible Server (General Purpose)**, condicionado à mesma pergunta que
o doc 12 já deixou em aberto na seção 19, item 5 — *existe decisão corporativa de nuvem, e ela aponta
para Azure?*. Se sim, o serviço gerenciado neutraliza por completo o risco mais concreto desta
decisão: diferente do SQL Server, onde a VM dedicada vencia porque **o parceiro de infraestrutura já
sabia operar backup daquele motor** (doc 12, seção 5, linha "Experiência do parceiro"), aqui a pergunta
"a Algar opera PostgreSQL?" (seção 9, item 2) está em aberto — enquanto ela não for respondida, o
serviço gerenciado transfere backup, patch e HA inteiramente para a Microsoft, e a resposta deixa de
importar para a operação (ainda importa para rede/latência, doc 12 seção 19, item 6).

**Se a resposta for "não, a hospedagem continua na Algar/on-premises"**, a alternativa é a **VM Linux
dedicada**, com `pgBackRest` ou `WAL-G` para backup e o mesmo teste de restauração já exigido no
portão da fase 0 — é exatamente a topologia que o doc 12, seção 9, já desenhou para o SQL Server, só
trocando o motor dentro da mesma caixa "VM dedicada, fora do container".

**Container autogerenciado pela própria equipe não é recomendado para produção agora** — pelo mesmo
motivo que valia para o SQL Server: estado não se opera bem em container sem disciplina de backup já
madura, e um time de três não tem essa maturidade ainda instalada. Duas ressalvas honestas: (1) é
**exatamente** o caminho certo para desenvolvimento e CI, via Testcontainers, mais leve e mais rápido
que a alternativa SQL Server (seção 2.9); (2) deixa de ser um "não" estrutural e volta à mesa **se e
quando** o degrau 2 do doc 12 (Kubernetes) for acionado por dois dos cinco gatilhos da seção 7 daquele
documento — nesse cenário, um operador dedicado de PostgreSQL para Kubernetes (CloudNativePG ou o
postgres-operator da Zalando, ambos padrões de mercado maduros) resolve backup, failover e *point-in-
time recovery* de forma declarativa, o que é uma categoria de ferramenta diferente de "rodar
`docker run postgres` e torcer".

---

## 8. Rota de reversão

O que preserva a opção de voltar para SQL Server, hoje, listado do mais forte para o mais frágil:

1. **EF Core como única camada de acesso a dado.** O domínio (`Tracbel.Crm.Dominio`) não conhece EF
   Core nem qualquer provedor — é testado por arquitetura (`Dominio_nao_pode_depender_de_infraestrutura`,
   doc 03, seção 8) e continua valendo sem alteração com Postgres.
2. **Nada de SQL específico de fornecedor no domínio.** Já era verdade porque o domínio não fala SQL
   algum; a única fronteira que precisa de disciplina nova é o uso direto de operadores `jsonb`
   (`@>`, `?`, `#>>`) fora do schema `metadado` — a mesma lógica de quarentena que o doc 03, seção 1,
   já aplica ao vocabulário do Vórtice. **Regra proposta:** operador `jsonb` só dentro de
   `Infraestrutura/Persistencia`, nunca em `Aplicacao`, exposto por uma interface de repositório comum
   — se o motor mudar, só essa fatia é reescrita.
3. **Migrations geradas por provedor, nunca editadas à mão.** Já é regra (doc 14, seção 9, regra 9.1).
   Trocar de provedor significa apagar a pasta `Migrations/` e gerar de novo a partir do
   `OnModelCreating` — código de configuração que muda pouco (a maior parte é `HasMaxLength`/
   `HasPrecision`/`IsRequired`, agnósticos de provedor), exceto exatamente nos pontos listados na
   seção 6 (`jsonb`, `xmin`, `timestamptz`).
4. **Teste de arquitetura que barra dependência direta de driver.** O
   [doc 14, seção 4](14-PADRAO-DE-BANCO.md) já proíbe `HasColumnType(...)` literal fora de uma
   allowlist explícita — o mesmo mecanismo, com a allowlist já proposta na edição 17 da seção 4,
   garante que "onde o banco é Postgres de verdade" fique registrado num lugar só, nunca espalhado.
   Recomenda-se um teste irmão explícito — `Nenhuma_camada_alem_de_Infraestrutura_referencia_Npgsql` —
   no mesmo padrão do teste que já protege a quarentena do vocabulário do Vórtice.

### Em que ponto a reversão deixa de ser barata

A reversão é **praticamente gratuita ao fim da fase 0** (só schema, zero dado em produção) e **ainda
barata ao fim da fase 1** — 25 tabelas (doc 04, seção 13), nenhuma delas usando `jsonb`, porque
`metadado.Resposta`/`Preenchimento` só nascem na **fase 3A** (doc 13, seção 2.6, item 18) e o
particionamento de `processo.RegraExecucao`/auditoria só compensa depois que houver volume — não no
dia 1. A partir da **fase 2** (particionamento começa a valer a pena com volume real) e sobretudo da
**fase 3A** (`jsonb` em formulário dinâmico, o caso mais forte da seção 2.4), a reversão passa a exigir
reescrever especificamente essa fatia do schema — não o banco inteiro, mas também não mais uma tarde de
trabalho. Depois da migração de dado histórico da fase 6, reversão deixa de ser uma opção prática por
qualquer motor: é o mesmo raciocínio que o doc 12, seção 15, já aplica a schema destrutivo — "restaurar
do backup imediatamente anterior... por isso a regra da seção 13", só que em escala de programa, não de
deploy.

---

## 9. O que precisa ser confirmado, e com quem

Numerada, pronta para virar pauta do comitê mensal (doc 13, seção 5.4).

1. **Existe um Enterprise Agreement ou padrão corporativo de banco de dados na Tracbel** que já cubra
   SQL Server/Azure SQL para sistemas novos sem custo marginal? *(com: TI corporativo / contratos —
   é a pergunta que mais pesa, ver o destaque da seção 3)*
2. **O parceiro de infraestrutura (Algar) opera PostgreSQL hoje**, com qual experiência e qual
   ferramenta de backup? *(com: Algar — mesma pergunta que o doc 12, seção 19, item 1, já faz para
   Linux em geral, aqui aplicada especificamente ao motor de banco)*
3. **A licença de SQL Server "que já existe" (doc 03, seção 10) é da Tracbel de fato, ou é uma licença
   embutida no contrato do Vórtice** — e portanto não transferível para um sistema novo e independente?
   *(com: setor de contratos — pergunta nova, que nenhum documento anterior respondeu com precisão)*
4. **O backup corporativo da Tracbel (a ferramenta, não só o procedimento) cobre PostgreSQL**, ou só
   está calibrado para SQL Server? *(com: Algar / TI corporativo)*
5. **O time aceita a curva de aprendizado** de Postgres somada à de Docker/Linux já estimada na fase 0
   (doc 12, seção 6.2), ou isso empurra a fase 0 de 5 para 6 semanas por um motivo a mais além do já
   citado? *(com: o time — A, B, C, doc 13, seção 5.1)*

---

## 10. Decisão registrada

- **Data:** 04/09/2026.
- **Quem decidiu:** Ricardo Coradini de Marco Moretti (arquiteto/tech lead do programa, doc 13,
  seção 5.1), com a implementação já iniciada em paralelo à redação deste documento.
- **Documentado por:** este documento, a pedido de Ricardo, registrando decisão já tomada — não uma
  proposta em aberto.
- **O que faria esta decisão ser revista:**
  1. A pergunta 1 da seção 9 voltar com "sim, existe Enterprise Agreement que já cobre SQL Server sem
     custo marginal" — o cenário destacado em 🔴 na seção 3.
  2. As perguntas 2 e 4 da seção 9 voltarem **as duas** negativas ao mesmo tempo (Algar não opera
     Postgres **e** o backup corporativo não cobre) **e** a Azure não for viável como destino
     (doc 12, seção 19, item 5) — o mesmo tipo de dupla-negativa que o doc 12, risco R9, já trata como
     gatilho de reabrir a conversa de hospedagem.
  3. O uso de `jsonb` vazar do schema `metadado` para o resto da aplicação sem a disciplina da
     seção 8, item 2 — nesse caso o problema não seria Postgres, seria a disciplina de fronteira não
     ter sido seguida, mas o efeito prático (perda de portabilidade) seria o mesmo.
