# Padrão de banco de dados — CRM Tracbel

> Documento normativo. Formaliza e substitui as convenções do
> [04-MODELO-DADOS](04-MODELO-DADOS.md), seção 1; o vocabulário e os nomes de negócio ficam no
> [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md) — este documento **referencia** aquele, não o
> duplica. A eliminação de tabelas duplicadas é tratada em
> [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md) *(em elaboração por outra frente — não antecipado
> aqui)*; o detalhamento de higienização de dado é tratado em
> [16-HIGIENIZACAO-DE-DADOS](16-HIGIENIZACAO-DE-DADOS.md) *(idem)*.
>
> **Toda regra abaixo segue o mesmo formato: regra → por quê, com o achado do Vórtice e o
> número → como se verifica.** Regra sem teste automatizado é **recomendação**, não padrão, e
> está marcada como tal — ver seção 14. Os testes que aplicam este documento vivem em
> [`tests/Tracbel.Crm.Arquitetura.Testes/Banco/`](../../tests/Tracbel.Crm.Arquitetura.Testes/Banco/)
> e citam a seção exata que verificam; os números de seção deste documento são, portanto,
> **contrato com o código de teste** — não renumere uma seção sem atualizar as strings de
> `Should().BeEmpty(...)` nos arquivos daquela pasta, e vice-versa.

---

## 1. O problema medido — por que este documento existe

A extração ao vivo do Vórtice em 02/09/2026
([relatório completo](../extracao-vortice/00-RELATORIO-EXTRACAO.md)) mediu, num banco de 767
tabelas e 86,3 milhões de linhas, sete problemas que este documento existe para impedir que se
repitam. Cada um aponta para a seção que o resolve.

| # | Problema medido | Evidência | Resolvido em |
|---|---|---|---|
| 1 | **Zero `CHECK CONSTRAINT`** em 767 tabelas | 00-RELATORIO-EXTRACAO, "Números-chave": `CHECK constraints: 0`; achado 7 | seções 4, 6, 14 |
| 2 | Domínio de status duplicado por digitação livre: `FINALIZADO` (18.416 linhas) convive com `FINALIZADA` (11.563) | 00-RELATORIO-EXTRACAO, achado 7 (`IV_Processo.Status`) | seções 4, 6 |
| 3 | **437.694 processos com status em branco** | 00-RELATORIO-EXTRACAO, achado 7 | seções 5, 6 |
| 4 | **90 tabelas em 34 grupos** de conjunto de colunas idêntico (a maioria, cópias `*_BKP*`/`*_ITA`) | [`tabelas-duplicadas.txt`](../extracao-vortice/tabelas-duplicadas.txt), linha 4 | seção 13 (proibição); unificação em [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md) |
| 5 | **Logs ocupam 42% do banco** (~13,7 GB de 32,4 GB), sem política de retenção | 00-RELATORIO-EXTRACAO, achado 11 | seção 10 |
| 6 | `AUTO_SHRINK` **ligado** em produção e homologação, com crescimento de arquivo em 10% (percentual, não fixo) | 00-RELATORIO-EXTRACAO, achado 9 | seção 10 |
| 7 | **5 tabelas grandes sem chave primária**, todas heap (a maior com 1.092.904 linhas) | 00-RELATORIO-EXTRACAO, achado 10 | seções 3, 6 |

O item 4 merece uma palavra a mais porque conecta com o item 1: dos 34 grupos de tabelas com
colunas idênticas, a maioria **é literalmente a mesma tabela copiada como backup manual** —
`IV_Processo` / `IV_Processo_bkp20250717`, `IV_Agenda` / `IV_Agenda_bkp20250717`, `GE_Pessoa` /
`GE_Pessoa_BKPJUN` / `GE_Pessoa_BKP28052025`, `EXT_Titulo` com **quatro** variantes de backup. Não
é falta de disciplina de uma pessoa — é a ausência de qualquer mecanismo que torne esse padrão
difícil. A seção 13 fecha exatamente essa porta.

**Como usar este documento:** cada seção a seguir declara a regra, o porquê (com o achado do
Vórtice sempre que existir), e como ela é verificada — automaticamente por `dotnet test`, por
revisão humana de PR, ou por script de provisionamento. A seção 15 é o índice reverso: dado um
teste que falhou, qual regra e qual seção ele protege.

---

## 2. Schemas — organização por domínio

**Regra 2.1 — [Testado]** Toda tabela vive num schema de uma **lista fechada**; nenhuma tabela
vive em `dbo`.

> **Por quê:** `[V]` o Vórtice inteiro — 767 tabelas — mora em `dbo`, sem nenhum agrupamento lógico
> no próprio SQL Server. É o medo declarado do projeto ("imagina 700 tabelas" sem organização
> nenhuma), e a única defesa estrutural é o schema ser obrigatório desde a primeira tabela.
>
> **Como se verifica:** `EsquemaENomenclaturaTestes.Toda_entidade_esta_num_schema_da_lista_fechada_e_nenhuma_esta_em_dbo`.

**Regra 2.2 — [Testado]** O schema padrão do `CrmDbContext` (`HasDefaultSchema(...)`) também
precisa estar na lista fechada.

> **Por quê:** a entidade que não declarar schema explicitamente cairia em `dbo` silenciosamente —
> a regra 2.1 fecha a porta da frente, esta fecha a dos fundos.
>
> **Como se verifica:** `EsquemaENomenclaturaTestes.O_schema_padrao_do_contexto_esta_na_lista_fechada`.

### 2.1 A lista fechada, hoje

Esta é a lista tal como está gravada em `EsquemaENomenclaturaTestes.SchemasPermitidos` e usada
pelo `CrmDbContext` e pelas classes de `Persistencia/Configuracoes/` — é o que os testes acima
verificam **agora**:

| Schema | O que guarda | Tabelas |
|---|---|---|
| `organizacao` | empresa, linha de negócio, carteira, município e carteira × município (doc 26), área de atuação, responsável pelo município, produção agrícola no município e no estado e regra de potencial (doc 32; issue 64); a estrutura agropecuária — frota de tratores, estabelecimentos por área, rebanho, área territorial e usinas de etanol (issue 65); os preços de mercado — cotação mensal por produto e dólar PTAX (issue 66); o custo de produção por cultura, local e safra (issue 67); o crédito rural de investimento do SICOR e as tabelas auxiliares dele (issue 68); os parâmetros gerais do potencial e a percepção do gestor por município, com vigência (issue 71); o de-para entre a chave de cada fonte e o município do catálogo (issue 154); o total publicado pelo IBGE para o estado nas quatro pesquisas da estrutura agropecuária (issue 155) | 24 |
| `seguranca` | usuário, permissão e a ligação entre os dois | 4 |
| `comercial` | cliente, contato, canal, endereço, carteira, faturamento do cliente e faturamento sem cliente (doc 31) | 8 — schema **padrão** do contexto |
| `processo` | processo, fase, tarefa, interação, tipo, resultado, motivo e venda perdida | 9 |
| `frota` | equipamento do cliente, marca, modelo, família, linha de produto, venda de máquina e vínculo de cliente com máquina (doc 35, seção 10) | 7 |
| `auditoria` | quem alterou o quê — particionada por mês | 1 |
| `integracao` | fronteira com o ERP, o Vórtice e o ART: correspondência da origem, registro de origem, comprador pendente e divergência (doc 35, seção 10); execução de sincronização, uma linha por ciclo do serviço do Windows (doc 35, seção 11); conexão, verificação, rotina e execução da rotina — as integrações configuráveis pela tela (issue 136) | 13 |
| `metadado` | catálogo e item de catálogo — extensão sem release; ver seção 12 | 2 |

**Total: 68 tabelas em 8 schemas.** A conta começou em 63 no
[17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 8.12, subiu para 80 em 10 schemas com as
dezessete decisões registradas nos documentos 26, 31, 32 e 35, e a **fase 1** do
[41-PLANO-EXECUTIVO](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md) a trouxe para 49 em 8, removendo as 31
tabelas que nunca receberam uma linha e esvaziando por completo os schemas `documento` e `relatorio`
— que por isso saíram da lista. A quinquagésima é `organizacao.ProducaoAgricolaNoEstado` (issue 64):
o IBGE publica o total da UF, e ele não é a soma dos municípios. As cinco seguintes são a **estrutura
agropecuária** (issue 65) — frota de tratores, estabelecimentos por área, rebanho, área territorial e
usinas de etanol: a PAM diz quanto se *planta*, e nenhuma delas dizia o que já existe para mecanizar
isso. São cinco e não uma porque são cinco granularidades, de quatro pesquisas e duas agências, e
juntá-las faria a coluna do ano significar coisas diferentes na mesma linha. As duas últimas são os **preços de mercado** (issue 66): o preço de cada produto agrícola por mês, em reais e na unidade da fonte (CONAB e Socicana), e o dólar PTAX do mês. São duas e não uma porque o dólar é um só para todos os produtos; o preço em dólar é calculado na leitura, e gravá-lo seria uma segunda verdade para o mesmo número. A quinquagésima oitava é o **custo de produção** da CONAB (issue 67): uma linha por aba da série histórica — cultura, local de referência e safra —, com as cinco camadas da metodologia por hectare e por unidade. As duas últimas são o **crédito rural de investimento** do SICOR (issue 68) — uma linha por município, mês e combinação de produto, programa, fonte e modalidade, como o Banco Central publica — e as **tabelas auxiliares** dele (programa, subprograma, fonte e produto), numa tabela só porque as quatro têm a mesma forma. As duas últimas são os **parâmetros do potencial com vigência** (issue 71): os parâmetros gerais do modelo (janela dos índices, composição do crédito, faixas de mercado, pesos e limites do fator, limite da percepção) e a percepção do gestor por município. A regra por cultura já existia (`RegraDePotencial`) e ganhou as mesmas colunas de vigência; as outras duas são tabelas próprias porque são outras granularidades — o conjunto geral se lê inteiro numa data, e a percepção é por município, informada pelo gestor e não pelo administrador. As quatro últimas são as **integrações configuráveis** (issue 136): a **conexão** (o sistema, o endereço e a credencial — a senha protegida pela proteção do Windows da máquina, nunca em texto), a **verificação** (o histórico do botão "Testar" e do monitoramento), a **rotina** (a agenda de cada carga do servidor, que substituiu as tarefas agendadas do Windows) e a **execução da rotina** (o histórico do orquestrador). São quatro e não uma porque são dois cadastros — o que se conecta e o que roda — e os dois históricos deles, com ciclos de vida diferentes: a conexão e a rotina mudam quando alguém edita; a verificação e a execução só crescem. A última é o **de-para de município por fonte** (issue 154): nenhuma das fontes de mercado publica o código do IBGE — o SICOR traz o código do Banco Central, a ANP e os custos da CONAB trazem só o nome —, e até aqui cada carga recasava o nome a cada rodada, de modo que uma grafia nova na origem fazia o município sumir do número sem ninguém ver. A tabela guarda, uma vez, para onde a chave de cada fonte aponta, com a forma do casamento e o autor; da segunda rodada em diante o par responde e nenhum nome é comparado de novo. É uma tabela só, e não uma coluna em cada fonte, porque a chave da fonte não cabe no município (um município tem várias) nem na medida (a mesma chave vale para todos os anos daquela fonte). A última é o **total publicado do estado** (issue 155): o IBGE divulga a linha da UF com o que o sigilo esconde nos municípios, e somar os 645 devolve um número menor que o oficial — a lavoura já comparava com a linha publicada (`ProducaoAgricolaNoEstado`) e o parque, as propriedades e o rebanho somavam, dois métodos na mesma tela. É **uma** tabela para as quatro pesquisas, e não quatro como no nível municipal, porque a chave guardada é a do próprio SIDRA (tabela, variável, período e categoria) e o estado é uma localidade só: quatro tabelas repetiriam a mesma estrutura para guardar algumas dezenas de linhas no total.

A conta é verificada por
`EsquemaENomenclaturaTestes.Os_oito_schemas_do_modelo_unificado_existem_e_somam_cinquenta_e_cinco_tabelas`
contra o modelo, e por
`MigracaoNoContainerTestes.A_cadeia_de_migracoes_cria_os_oito_schemas_e_as_cinquenta_e_cinco_tabelas`
contra o banco de verdade. **Os dois testes e esta tabela mudam na MESMA PR** — é o portão da seção
10.2.

### 2.2 Como criar um schema novo

Um schema não nasce por conveniência de quem escreve a migration do dia — ele é uma decisão
registrada, na ordem:

1. Proponha o nome no PR e diga qual domínio de negócio ele agrupa.
2. Acrescente o nome a `SchemasPermitidos` em `EsquemaENomenclaturaTestes.cs` **e** à tabela da
   seção 2.1 deste documento, **na mesma PR** — os dois lugares mudam juntos, ou o teste falha
   (se você esqueceu o teste) ou o documento fica errado (se você esqueceu o documento).
3. Se o schema introduzir um termo de negócio novo, registre-o primeiro no
   [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 2 — o glossário é o vocabulário; este
   documento só obriga o uso dele.
4. Só então crie a primeira entidade nesse schema.

### 2.3 A troca de sigla por palavra inteira — aplicada

O [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 2, decidiu que as siglas de schema do
desenho original (`org`, `seg`, `crm`, `wf`, `equip`, `aud`, `intg`, `meta`, `rel`) seriam palavras
inteiras — **"o pessoal do comercial usa a palavra inteira, não a sigla"**.

**Isso está feito.** Código, teste e a tabela da seção 2.1 usam as palavras inteiras, e a migration
inicial as criou assim. A regra que fica para o futuro é a de sempre: um schema novo muda as três
coisas juntas, na mesma PR — `ToTable(...)`/`HasDefaultSchema(...)`, `SchemasPermitidos` em
`EsquemaENomenclaturaTestes.cs`, e a tabela da seção 2.1 deste documento.

---

## 3. Nomenclatura de tabela e coluna

O vocabulário — **qual** palavra usar para "cliente", "processo", "frota" — está inteiramente no
[15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md); não é repetido aqui. Esta seção define **a forma**
que qualquer nome escolhido precisa ter, e é o que os testes verificam mecanicamente — o glossário
não pode ser verificado por um teste de arquitetura porque é uma lista de palavras de negócio, não
uma forma sintática.

| Regra | Por quê — achado do Vórtice | Status | Como se verifica |
|---|---|---|---|
| Toda tabela tem chave primária | `[V]` 5 tabelas grandes são heap, sem PK nem índice clusterizado (achado 10, a maior com 1.092.904 linhas) | **Testado** | `EsquemaENomenclaturaTestes.Toda_tabela_tem_chave_primaria` |
| A chave primária sempre se chama `Id` — a ÚNICA exceção são as 4 tabelas particionadas por data, onde o SQL Server exige a coluna de particionamento na chave (e mesmo lá a chave começa por `Id`) | previsibilidade — qualquer dev novo sabe o nome da PK sem abrir a tabela | **Testado** | `EsquemaENomenclaturaTestes.A_chave_primaria_e_a_coluna_Id_salvo_nas_tabelas_particionadas_por_data` |
| Nome de tabela é PascalCase, só ASCII, sem acento/`ç`, sem `_` | `[V]` os prefixos `IV_`, `GE_`, `EXT_`, `IVS_` do Vórtice pararam de significar algo e hoje só confundem; o regex `^[A-Z][A-Za-z0-9]*$` torna esse estilo estruturalmente impossível | **Testado** | `EsquemaENomenclaturaTestes.Nome_de_tabela_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore` |
| Nome de coluna é PascalCase, só ASCII, sem acento/`ç`, sem `_` | mesmo motivo; evita também problema de collation e de ferramenta com acento em identificador (doc 15, seção 1) | **Testado** | `EsquemaENomenclaturaTestes.Nome_de_coluna_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore` |
| Proibidos os tokens `BKP`, `OLD`, `TESTE`, `TMP`, `MIG` em nome de tabela ou de coluna | `[V]` 27 tabelas com "bkp" no nome no levantamento bruto, incluindo `GE_Pessoa_BKPJUN` (dado de produção, referenciada por procedure) e `GE_Pessoa_BKP28052025` — dois backups da mesma tabela, sete meses depois, sem processo nem limpeza; ver também seção 13 | **Testado** | `EsquemaENomenclaturaTestes.Nenhuma_tabela_ou_coluna_usa_nome_de_backup_teste_ou_migracao` |
| Prefixo de constraint: `PK_`, `FK_`, `IX_` (índice comum), `UX_` (índice único), `CK_` (check) | previsibilidade ao ler o catálogo do banco direto no SSMS, sem abrir o código | **Testado** | `EsquemaENomenclaturaTestes.Indices_e_chaves_seguem_o_prefixo_padrao` |
| Vocabulário é a palavra inteira do negócio, em português, sem sigla — três exceções (`Id`/`Lead`; siglas que a Tracbel fala como CEN/CNPJ/CPF/NF/OS/ERP/CRM; sem acento no identificador) | `[V]` o vocabulário do Vórtice (`SeqPessoa`, `CodProcesso`, `DtaRealizacao`, `IndEstorno`) só mora na cabeça de duas pessoas | **Recomendação** — o regex de PascalCase barra sigla com underscore, mas não impede uma sigla nova de 3 letras em PascalCase; a escolha da palavra certa é revisão de PR contra o [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md) | revisão de PR |

**A nuance do prefixo de índice, que engana quem não leu o EF Core de perto:** o EF Core já nomeia
`PK_...`, `FK_...` e `IX_...` sozinho, por convenção — você não escreve isso. **Só o índice
ÚNICO** precisa de `HasDatabaseName("UX_...")` explícito, porque a convenção padrão do EF Core
para índice único também é `IX_...`, não `UX_...`. Exemplo real do código
(`LeadConfiguracao.cs`):

```csharp
b.HasIndex(l => l.ChavePublica).IsUnique().HasDatabaseName("UX_Lead_ChavePublica");
```

Sem o `HasDatabaseName`, o índice sairia como `IX_Lead_ChavePublica` — único, mas com o prefixo
errado — e o teste `Indices_e_chaves_seguem_o_prefixo_padrao` falharia.

---

## 4. Tipos de coluna — permitidos e proibidos

`[V]` os tipos errados do Vórtice não foram um mau dia: são um padrão sistemático — `varchar` em
nome de gente (corrompe acento), `decimal(12)` para telefone (estoura com DDI), `numeric(18,0)`
como resposta padrão para "não sei que precisão usar" (arredonda dinheiro em silêncio), `datetime`
em vez de `datetime2`, `char(1)` anulável para booleano. A defesa não é lembrar de não usar cada
tipo errado — é **proibir a via que os introduziria**: `HasColumnType("...")` escrito à mão. Sem
essa via, `HasMaxLength`/`IsUnicode`/`HasPrecision` (agnósticas de provedor) são as únicas
ferramentas, e nenhuma delas sabe produzir `text`, `ntext`, `image`, `money`, `float` ou `datetime`
simples.

| Regra | Por quê — achado do Vórtice | Status | Como se verifica |
|---|---|---|---|
| Proibido `HasColumnType("...")` literal, salvo com um tipo da lista curta de exceções justificadas (hoje só `nvarchar(max)`, para as 8 colunas de JSON) | é a via de entrada de todos os tipos abaixo — proibir a via é mais barato que vigiar cada tipo | **Testado** | `TiposDeColunaTestes.HasColumnType_literal_so_aparece_com_tipo_da_lista_justificada` (varre o texto-fonte de `Persistencia/Configuracoes/`, não o modelo — ver comentário no teste), reforçado por `Nenhuma_coluna_do_modelo_resolve_para_um_tipo_proibido`, que verifica o tipo RESOLVIDO |
| Toda coluna de texto declara `HasMaxLength`, salvo entrada na lista `TextoIlimitadoJustificado` | `nvarchar(max)` sem tamanho é "campo que aceita qualquer coisa"; hoje a lista tem exatamente 2 entradas: `Lead.PayloadOriginal` (JSON cru do RD Station, tamanho não previsível por natureza) e `Regra.EfeitoParametros` (JSON de parâmetros, forma varia por efeito) | **Testado** | `TiposDeColunaTestes.Texto_sem_tamanho_maximo_so_e_permitido_na_lista_de_justificativas` |
| Todo `decimal` declara precisão **e** escala explícitas (`HasPrecision(p, s)`) | `[V]` `numeric(18,0)` é a escala "padrão" do Vórtice para tudo — não existe escala implícita aceitável aqui; dinheiro é `decimal(18,2)` | **Testado** | `TiposDeColunaTestes.Todo_decimal_declara_precisao_e_escala_explicitas` |
| Propriedade booleana (`bool`) nunca é anulável (`bool?`) | `[V]` `IV_Agenda.Realizada` é `char(1)` e aceita `NULL` — um "sim/não" que na prática tem três estados, e o terceiro nunca foi uma decisão de negócio | **Testado** | `TiposDeColunaTestes.Nenhuma_propriedade_booleana_e_anulavel` |
| Toda coluna de data/hora declara `HasPrecision(3)` — vira `datetime2(3)`, nunca `datetime` | `[V]` `datetime` tem precisão irregular de ~3ms e faixa curta | **Testado** | `TiposDeColunaTestes.Toda_data_hora_declara_precisao_de_milissegundos` |
| Coluna `char(1)` de tamanho fixo sempre tem `CHECK` enumerando os valores válidos | é exatamente como `TipoPessoa IN ('F','J')` deveria **sempre** ter sido mapeado — sem o `CHECK`, `char(1)` aceita qualquer caractere; ver também seção 14 | **Testado** | `TiposDeColunaTestes.Coluna_de_caractere_fixo_unico_tem_check_constraint` — vacuamente verdadeiro hoje (nenhuma coluna `char(1)` mapeada ainda); guarda o dia em que uma aparecer |
| Coluna que representa um `enum` do C# tem `CHECK` enumerando os valores válidos | `[V]` `IV_Processo.Status` tem mais de 20 valores de texto livre com duplicatas semânticas — `FINALIZADO` (18.416) convive com `FINALIZADA` (11.563) — porque o Vórtice tem zero `CHECK` em 767 tabelas; ver também seção 6 | **Testado** | `IntegridadeReferencialTestes.Coluna_baseada_em_enum_tem_check_constraint_de_dominio` |
| `varchar` só para código ASCII; `nvarchar` para texto de gente (nome, endereço, observação) | `[V]` o Vórtice usa `varchar` em `Detalhe` e corrompe acentuação | **Recomendação** — a escolha entre `IsUnicode(true)`/`IsUnicode(false)` não é verificada por teste, só a *presença* do `HasMaxLength` é | revisão de PR |
| Telefone é sempre texto (`varchar`), nunca numérico | `[V]` `GE_Pessoa.FoneNro1` é `decimal(12)`: telefone com DDI ou zero à esquerda estoura o tipo e a API devolve erro genérico | **Recomendação**, reforçada estruturalmente pelo tipo de valor `Telefone` (`Dominio.Comum.Telefone`, um `record struct` sobre `string`) — quem usa o tipo de valor não consegue errar isso | revisão de PR; o construtor de `Telefone.Criar` já recusa entrada inválida |

---

## 5. Colunas obrigatórias e proibidas

### 5.1 Obrigatórias — o bloco de auditoria

Toda entidade **transacional** (que herda `EntidadeBase`) carrega oito colunas fixas. O C# já
garante isso por herança; o papel do teste é vigiar que ninguém tire uma delas do mapeamento com
`Ignore(...)`.

| Coluna | Tipo | Regra | Status |
|---|---|---|---|
| `Id` | `bigint` | PK interna, nunca exposta (ver seção 3) | **Testado** |
| `ChavePublica` | `uniqueidentifier` | o identificador que a API expõe — ninguém enumera registro contando `Id` | **Testado** |
| `CriadoEm` | `datetime2(3)` | quando o registro nasceu (UTC) | **Testado** |
| `CriadoPorId` | `bigint` | quem criou — **sempre ID, nunca texto** (ver abaixo) | **Testado** |
| `AlteradoEm` | `datetime2(3)` (nulo) | última alteração | **Testado** |
| `AlteradoPorId` | `bigint` (nulo) | quem alterou por último — sempre ID | **Testado** |
| `ExcluidoEm` | `datetime2(3)` (nulo) | soft delete: `NULL` = ativo | **Testado** |
| `Versao` | `rowversion` | concorrência otimista, gerenciada pelo banco — incrementada a cada gravação, inclusive a que não passa pela aplicação | **Testado** |

> **Por quê o bloco inteiro:** `[V]` desativar um usuário no Vórtice destrói as permissões porque
> não existe flag de ativo/inativo (`GE_Usuario` não tem coluna de inativação, confirmado no
> schema) — a coluna simplesmente nunca existiu, porque nada obrigava sua existência.
>
> **Como se verifica:** `AuditoriaTestes.Entidade_transacional_mantem_todas_as_colunas_de_auditoria`.

**Regra — [Testado]** `CriadoPorId` e `AlteradoPorId` são sempre `long` (`bigint`) — id de
usuário — nunca texto.

> **Por quê:** `[V]` `IV_Agenda.Vendedor` guarda um **login** (`varchar`), não um ID. Trocar o
> login de alguém, ou o próprio nome de usuário, quebra silenciosamente todo histórico de autoria.
>
> **Como se verifica:** `AuditoriaTestes.Colunas_de_auditoria_de_quem_fez_nunca_sao_texto_livre`.

### 5.2 Obrigatórias — multiempresa

**Regra — [Testado]** Toda tabela transacional tem `EmpresaId int NOT NULL`.

> **Por quê:** `[V]` no Vórtice a multiempresa é nominal — `NroEmpresa` está em toda tabela de
> permissão, mas 340 dos 409 usuários ativos têm acesso a 17 das 18 empresas, e a coluna
> `NROEMPRESA` da tabela de permissão tem **um único valor distinto** (doc 05, seção 1). Uma
> coluna que existe mas não é usada como fronteira de fato não é fronteira nenhuma. Detalhe
> completo na seção 11.
>
> **Como se verifica:** `AuditoriaTestes.Toda_tabela_transacional_declara_a_coluna_de_multiempresa`.
> Era recomendação sem teste; passou a ser testada. O teste varre toda entidade que herda
> `EntidadeBase` e exige `EmpresaId` — coluna que existe e não é usada como fronteira não é
> fronteira, mas coluna que nem existe não tem como ser nenhuma das duas coisas.

### 5.3 Proibidas

| Proibição | Por quê — achado do Vórtice | Status |
|---|---|---|
| Autoria por login/nome de usuário em vez de ID | ver `CriadoPorId`/`AlteradoPorId` acima — `[V]` `IV_Agenda.Vendedor` | **Testado** (dentro do escopo das colunas de `EntidadeBase`) |
| Colunas numeradas (`Telefone1`, `Telefone2`, `Telefone3`, `Email1`, `Email2`...) em vez de tabela filha | `[V]` o Vórtice tem **três** modelos concorrentes de e-mail (`GE_Pessoa.Email`, `GE_Email`, `GE_PessoaEmail`) e **três** de telefone (`FoneDDD1..3`+`FoneNro1..3`, `GE_PessoaFone`, e mais) dentro do mesmo banco — ninguém sabe qual é a verdade (doc 04, seção 3.3). O modelo novo usa uma tabela polimórfica única (`crm.CanalContato`) | **Recomendação** — não há teste de arquitetura que detecte "coluna numerada"; é revisão de PR |
| Status/situação como texto livre sem `CHECK` de domínio | ver seção 4/6 — `[V]` `FINALIZADO`/`FINALIZADA` | **Testado** (via regra de `enum` → `CHECK`, seção 6) |

---

## 6. Integridade referencial obrigatória

Esta é a seção que ataca o achado mais caro do diagnóstico original: `IV_Agenda`, `IV_Historico` e
`IV_ProcDado` não têm FK para `IV_Processo`, e isso produziu **345.535 linhas órfãs** (doc 04,
seção 1.4). "Foreign keys: 672" existiam no Vórtice — não adiantou, porque a FK que falta é
exatamente a que dói.

| Regra | Por quê — achado do Vórtice | Status | Como se verifica |
|---|---|---|---|
| Toda coluna cujo nome bate **exatamente** com `<Entidade>Id` de uma entidade já mapeada tem `ForeignKey` declarada | `[V]` a lacuna de FK gerou 345.535 linhas órfãs; a mesma classe de defeito foi pega **nesta própria mudança**, em miniatura, em `UsuarioConjuntoPermissao.ConjuntoPermissaoId` e `ExecucaoRegra.RegraId`, que apontavam para tabelas do próprio modelo sem FK | **Testado** — heurística por nome; ver limite abaixo | `IntegridadeReferencialTestes.Toda_referencia_a_entidade_mapeada_tem_foreign_key_declarada` |
| `DeleteBehavior.Cascade` só é aceito quando listado em `CascadeJustificado`, com o nome da constraint | apagar em cascata silenciosamente é como o Vórtice perde rastro sem ninguém decidir isso de propósito; o padrão do projeto é `Restrict` | **Testado** | `IntegridadeReferencialTestes.Exclusao_em_cascata_so_e_usada_na_lista_de_excecoes_documentadas` |
| Coluna que representa um `enum` do C# tem `CHECK` de domínio fechado no banco, não só no compilador | `[V]` zero `CHECK constraints` em 767 tabelas é a causa raiz de `FINALIZADO`/`FINALIZADA` conviverem e de 437.694 processos com status em branco | **Testado** (mesma regra listada na seção 4) | `IntegridadeReferencialTestes.Coluna_baseada_em_enum_tem_check_constraint_de_dominio` |
| Nenhuma coluna de texto livre onde existe (ou deveria existir) um catálogo | `[V]` a coluna `Origem` de conta/lead virou texto livre no Vórtice, com duplicatas e valores de teste em produção (doc 04, seção 1.4, item 2) | **Recomendação**, com duas exceções agora testadas (as duas linhas abaixo) — no caso geral não é mecanicamente detectável a partir do modelo do EF (o EF não sabe distinguir "texto livre por decisão" de "texto livre por esquecimento") | revisão de PR |
| Toda coluna que aponta para `metadado.CatalogoItem` usa **chave estrangeira composta** `(CatalogoDo<Papel>Id, <Papel>Id)` contra `AK_CatalogoItem_CatalogoId`, com a coluna de catálogo constante — valor padrão **e** `CHECK` | doc 17, seção 8.11. Sem ela, trocar três catálogos próprios por um catálogo genérico trocou três domínios fechados por um domínio aberto: `ClienteContato.PapelId` aceitava um item de `CULTURA` (doc 21, achado I-1) | **Testado** | `CatalogoDeSistemaTestes` (três testes) |
| Toda coluna chamada `Entidade`/`EntidadeRaiz` tem `CHECK` com a lista **fechada e gerada do próprio modelo** das entidades mapeadas, comparada com `COLLATE Latin1_General_BIN2`; toda coluna chamada `Campo` tem `CHECK` de identificador PascalCase ASCII | é um ponteiro polimórfico: em cinco das dez tabelas `RegistroId` não tem — nem pode ter — chave estrangeira, e o domínio da coluna é a única defesa que sobra. `[V]` é a classe dos 5.302 documentos órfãos e do compartilhamento perdido (doc 21, achado I-3) | **Testado** | `DominioDeEntidadeTestes` (dois testes) |

> **Sobre "não é mecanicamente detectável":** para `Entidade` **é**, e desde 04/09/2026 está
> detectado. O conjunto de valores válidos daquela coluna é exatamente o conjunto de entidades
> mapeadas no `CrmDbContext`, conhecido em tempo de compilação — o `CHECK` é **gerado** dele
> (`CrmDbContext.FecharDominioDoPonteiroPolimorfico`) e o teste compara os dois. A frase acima
> continua valendo para o caso geral: o EF não sabe dizer se uma coluna de texto qualquer
> *deveria* ter catálogo.

**O limite honesto da heurística de FK por nome:** ela só reconhece uma referência quando a
propriedade se chama **exatamente** `<NomeDaEntidade>Id` (ex.: `RegraId` → entidade `Regra`).
Referência por **papel** — `ProprietarioId` apontando para `Usuario`, como no doc 04 — não é pega
por nome, porque o nome não bate com `UsuarioId`. Essa classe de caso continua dependendo da
**revisão de PR** (ver seção 15) — é o próprio comentário do teste que registra esse limite, para
que ninguém confie cegamente na heurística além do que ela cobre. Mesmo limitada, ela cresce junto
com o modelo: cada entidade nova aumenta a chance de pegar uma referência esquecida.

O exemplo hoje registrado em `CascadeJustificado` é `ConjuntoPermissaoItem` →
`FK_ConjuntoPermissaoItem_ConjuntoPermissao_ConjuntoPermissaoId`: não é uma entidade independente,
é a coleção *owned* do agregado `ConjuntoPermissao` — apagar o conjunto apaga os itens dele, do
mesmo jeito que apagar um Pedido apagaria os ItensPedido. O próximo caso já é esperado quando o
módulo `documento` nascer: `doc.DocumentoVinculo` → `doc.Documento` (doc 04, seção 10).

---

## 7. Índices

| Regra | Por quê — achado do Vórtice | Status | Como se verifica |
|---|---|---|---|
| Toda foreign key tem as colunas cobertas por um índice — próprio, como prefixo de um composto, ou pela própria chave primária | `[V]` diagnóstico original: "Foreign keys: 672 — nenhuma no caminho quente do BPM". Ter a FK declarada não bastou: sem índice, toda checagem de integridade e todo `JOIN` varrem a tabela inteira | **Testado** | `IntegridadeReferencialTestes.Toda_foreign_key_declarada_tem_indice_pelo_prefixo_das_colunas` |
| Índice de busca sobre tabela com soft delete usa filtro `WHERE ExcluidoEm IS NULL` | evita que o registro excluído logicamente continue custando espaço de índice e aparecendo em varredura de intervalo; já é o padrão em `LeadConfiguracao` (`HasFilter("ExcluidoEm IS NULL")`) | **Recomendação** — nenhum teste verifica a presença do filtro | revisão de PR |

O prefixo do nome do índice (`IX_`/`UX_`) já é regra da **seção 3**, testada por
`Indices_e_chaves_seguem_o_prefixo_padrao` — não repetida aqui para não ter duas seções afirmando
a mesma coisa com testes diferentes.

---

## 8. Dados de referência e seed

**Não há teste automatizado para esta seção** — é recomendação de processo.

- **Regra 8.1.** Todo catálogo fechado (`Permissao`, `TipoProcesso`, `Estagio`, `TipoTarefa`,
  `Desfecho`, `LinhaNegocio`...) nasce de **seed versionado no Git**, nunca de `INSERT` manual
  direto em homologação ou produção. O mecanismo concreto (EF Core `HasData` na migration, ou um
  seeder de aplicação rodado pelo job `crm-migracao` — ver seção 9) fica em aberto para quando o
  primeiro catálogo real for implementado; a regra que já vale hoje é **onde ele não pode nascer**.
- **Regra 8.2.** O seed é **idempotente**: rodar duas vezes não duplica linha nem lança exceção.
  É o mesmo script para dev, homologação e produção — o que muda é o ambiente, não o dado semente.
- **Regra 8.3.** Dado de referência (`Permissao.Codigo`, `TipoProcesso.Codigo`...) é criado com
  código estável, nunca com o `Id` interno fixado à mão — o `Id` é gerado pelo banco; o que o seed
  fixa é o `Codigo` (`UQ_...` já garante unicidade — ver seção 6).
- **Regra 8.4 — a única exceção à 8.3, fechada e nomeada.** Os **oito catálogos de sistema** de
  `metadado.Catalogo` — `PAPEL_CONTATO`, `ORIGEM_LEAD`, `MOTIVO_INATIVACAO`, `MOTIVO_DESCARTE`,
  `CULTURA`, `TIPO_DOCUMENTO`, `CONDICAO_PAGAMENTO`, `CONCORRENTE` — nascem com `Id` **fixo**,
  declarado em `Dominio.Metadado.CatalogosDeSistema` e semeado pela migração. **Por quê:** a
  seção 8.11 do doc 17 exige que `Contato.PapelId` só aceite item de `PAPEL_CONTATO`
  *verificado pelo banco*, e o mecanismo disso é uma chave estrangeira composta cuja coluna de
  catálogo é uma **constante** — e constante, no SQL Server, é um literal dentro de um `CHECK`.
  Isso levanta o `Id` desses oito de dado para **esquema**. Nada mais no banco pode fixar `Id`
  à mão, e um número usado aqui nunca é reaproveitado (catálogo aposentado vira `EstaAtivo = 0`).
  **Testado** por `CatalogoDeSistemaTestes.Os_catalogos_de_sistema_nascem_semeados_com_o_identificador_que_o_esquema_usa`,
  que compara o seed do modelo com a lista do código.

> **Por quê isto importa desde já:** `[V]` `IV_SegPerfil` (a tabela que resolveria perfis de
> permissão no Vórtice) está **vazia** — o catálogo nunca foi populado de forma disciplinada, e o
> resultado foi 370 combinações distintas de 51 flags para 939 usuários (doc 05, seção 1). Um
> catálogo de permissão que nasce por seed versionado, revisado em PR, não tem como chegar
> silenciosamente vazio.

---

## 9. Migrations do EF Core como única via de alteração de schema

**Não há teste automatizado que verifique isto hoje** — é recomendação de processo, reforçada
pelo pipeline descrito no [12-DECISAO-CONTAINERS](12-DECISAO-CONTAINERS.md).

- **Regra 9.1.** Toda alteração de schema nasce de `dotnet ef migrations add <Nome>`, com o código
  gerado revisado em PR como qualquer outro código — nunca editado à mão depois de gerado, salvo
  para corrigir o nome da migration.
- **Regra 9.2.** Proibido alterar schema por SSMS Table Designer, por script solto rodado à mão em
  homologação ou produção, ou por qualquer caminho que não deixe rastro no histórico de migrations
  do Git.
- **Regra 9.3.** Migration é **aditiva** por padrão: adiciona coluna, não renomeia, não remove.
  Remover uma coluna acontece **um deploy depois** de o código parar de usá-la (doc 12, seção 13,
  regras 1 e 2) — é o que torna o rollback de código (voltar uma tag de imagem) compatível com o
  schema que ficou para trás.
- **Regra 9.4.** Em homologação e produção, a migration roda como o job próprio `crm-migracao`
  (doc 12, seção 4.1), **antes** de subir a nova versão da API — nunca por um desenvolvedor
  rodando `dotnet ef database update` contra um ambiente compartilhado. Em desenvolvimento local,
  o script `scripts/banco/subir-banco.ps1` (seção "Entregável 2" — ver
  [`docs/banco/README.md`](../../docs/banco/README.md)) pode aplicá-la diretamente, porque o banco
  local é descartável.
- **Regra 9.5.** Nível de compatibilidade e collation do banco nascem **num script de criação
  versionado**, nunca de uma configuração feita à mão no servidor — é a lição direta da
  investigação de 02/09/2026: o banco do Vórtice está em *compatibility level* 100 (SQL 2008)
  **rodando sobre SQL Server 2019**, e ninguém sabe documentar quando ou por que isso foi fixado
  assim (doc 12, seção 5.1).

> **Por quê a disciplina toda:** `[V]` a integração do Vórtice não usa transação — o
> `BEGIN TRANSACTION` está literalmente comentado nas procedures (doc 03, seção 5.3) — e o deploy
> é cópia de pasta manual (doc 12, seção 4). Schema sem histórico versionado é a mesma classe de
> risco: mudança que ninguém consegue reconstruir depois.

---

## 10. Retenção, crescimento e arquivamento — decididos desde o início

**Não há teste automatizado para a configuração FÍSICA desta seção** — nenhum arquivo de `Banco/`
inspeciona `AUTO_SHRINK` nem crescimento de arquivo (o modelo do EF Core não representa `AUTO_SHRINK` nem crescimento de
arquivo). É recomendação, aplicada por script de provisionamento (ver seção "Entregável 2").

| Regra | Por quê — achado do Vórtice | Onde aplicar |
|---|---|---|
| Toda tabela de log/histórico **declara sua política de retenção na migration que a cria** — nunca "depois" | `[V]` logs somam **42% do banco** (~13,7 GB de 32,4 GB — achado 11); `GE_LgTb` tem 11,9 milhões de linhas **congeladas desde jun/2023**, nunca expurgadas; existe um job `LOG_COMPACTA` cadastrado e **não agendado** | migration + job de expurgo, no mesmo PR |
| `AUTO_SHRINK` é sempre **OFF** | `[V]` `is_auto_shrink_on = True` em produção e homologação — num banco de 36 GB com log em `FULL recovery`, o auto-shrink causa fragmentação contínua de índice e picos de I/O imprevisíveis (achado 9) | script de criação do banco (dev: `docker-compose.yml`/init script; homolog/prod: runbook da infra) |
| Crescimento de arquivo de dados em **MB fixo**, nunca percentual | `[V]` o arquivo de dados do Vórtice cresce de 10 em 10% — antipadrão reconhecido em arquivo desse porte (achado 9) | idem |
| `wf.RegraExecucao`: partição mensal, **12 meses online**, arquivamento depois | já registrado no doc 04 (linha 992) e no doc 05, seção 10, tabela de auditoria | migration da tabela |
| `aud.AlteracaoCampo`: 18 meses online, arquivo depois. `aud.EventoAcesso`: 24 meses. Mudança de permissão: **permanente** | doc 05, seção 10 — corrige o Vórtice, que audita tudo sem seletividade e sem expurgo (44,8M de 85,5M linhas do banco são log) | migration de cada tabela de auditoria |

---

## 11. Multiempresa sem cópia por filial

**Não há teste automatizado para esta seção hoje** (ver a lacuna já registrada na seção 5.2).

- **Regra 11.1.** `EmpresaId` é a **única** fronteira de multiempresa. Proibido criar schema por
  filial, banco por filial, ou tabela replicada por filial (`ClienteMAQ`, `ClienteSJRP`...).
- **Regra 11.2.** Hierarquia entre empresas (matriz/filial) usa caminho materializado
  (`Empresa.Caminho`, ex.: `/1/4/9/`), mantido na própria transação de gravação — nunca CTE
  recursiva em consulta quente. Já desenhado no doc 04, seção 2.1.
- **Regra 11.3.** O escopo de empresa é aplicado **num ponto só**: o filtro global do EF Core
  (`HasQueryFilter`, doc 03, seção 6) — nunca replicado como condição solta em repositório,
  relatório ou job. Hoje só `Lead` tem o filtro implementado (`CrmDbContext.OnModelCreating`); é o
  modelo a repetir, não a reinventar, a cada entidade nova.

> **Por quê:** `[V]` no Vórtice a multiempresa é **nominal**: 18 filiais, `NroEmpresa` em toda
> tabela de permissão, mas **340 dos 409 usuários ativos têm acesso a 17 empresas** (doc 05, seção
> 1) — e a coluna de multiempresa na tabela de permissão tem **um único valor distinto** em toda a
> tabela. Isolamento que ninguém usa é isolamento que não existe. Como as 18 empresas do Vórtice
> são todas filiais da mesma Tracbel Agro (17 em SP), multiempresa aqui é **multifilial**, não
> multi-tenant — não há razão de negócio para duplicar schema ou tabela por filial (ver também o
> achado de duplicação por filial nas regras de workflow, achado 6 do relatório de extração:
> 5.958 regras para 1.262 pares distintos, replicadas por empresa).

---

## 12. Extensibilidade pelo schema `metadado`

**Não há teste automatizado para esta seção hoje.**

- **Regra 12.1.** Campo personalizado vira **coluna real**, criada por migration gerada. É uma
  decisão consciente de **não copiar** o motor EAV do Salesforce — coluna real não paga o preço de
  performance do EAV nem herda teto de campos.
- **Regra 12.2.** `metadado.CampoEntidade` (doc 04, seção 11) é **catálogo para a UI montar a
  tela** — rótulo, tipo de dado exibido, obrigatoriedade, agrupamento, ordem. Ele **descreve** a
  coluna real; não é a fonte do dado, e nunca substitui o `NOT NULL`/`CHECK` que a coluna real já
  declara (seção 4).
- **Regra 12.3.** Comportamento novo (validar, integrar, notificar) é **uma classe** que implementa
  `IManipuladorEvento<T>` ou `IEfeitoRegra` (doc 03, seções 5.1 e 3.1) mais **uma linha** em
  `metadado.ManipuladorEvento` — nunca uma alteração num arquivo existente. É o ponto de extensão
  mais usado do sistema, por desenho.

> **Por quê:** `[DYN]` o ponto de extensão do Dataverse é uma **linha de tabela**, não uma
> alteração de código — `SdkMessageProcessingStep` liga/desliga/reordena lógica sem deploy. `[V]`
> o Vórtice não tem esse ponto de extensão: acrescentar campo ou regra nova mexe em tela e em
> banco ao mesmo tempo, sem catálogo que separe "o que existe" de "como a tela mostra".

---

## 13. Proibições explícitas

| Proibição | Por quê — achado do Vórtice | Status | Como se verifica |
|---|---|---|---|
| Tabela de backup dentro do banco de produção (nome com `BKP`, `OLD`, `TMP`, data no nome) | `[V]` 27 tabelas com "bkp" no nome; `GE_Pessoa_BKPJUN` é dado de produção **referenciado por procedure**; `IV_Processo_bkp20250717` e `IV_Agenda_bkp20250717` são cópias completas de tabelas vivas; `tabelas-duplicadas.txt` lista **90 tabelas em 34 grupos** de estrutura idêntica, a maioria backups manuais | **Testado** | `EsquemaENomenclaturaTestes.Nenhuma_tabela_ou_coluna_usa_nome_de_backup_teste_ou_migracao` (mesma regra da seção 3) |
| SQL de regra de negócio guardado em coluna de dado | `[V]` `GE_ObjDinamico` guarda **196 SQLs** no banco — o maior com 1.788 caracteres —, **168 do tipo `TESTE`**, que são literalmente as regras de validação que liberam ou bloqueiam um passo do processo; usam variável de bind proprietária e nome de tabela traduzido em runtime (achado 12) | **Recomendação** — o modelo do EF Core não inspeciona *conteúdo* de coluna, só schema; regra de negócio vive em código (`IEfeitoRegra`/`Regra.Condicao` como expressão avaliada pela aplicação, doc 03 seção 3.1), nunca em SQL dinâmico gravado como dado | revisão de PR |
| Tabela sem chave primária | `[V]` achado 10 — 5 tabelas heap grandes, a maior com 1.092.904 linhas | **Testado** (mesma regra da seção 3) | `EsquemaENomenclaturaTestes.Toda_tabela_tem_chave_primaria` |
| `char(1)` sem `CHECK constraint` | `TipoPessoa IN ('F','J')` é exatamente o caso que NUNCA pode voltar a ser mapeado sem `CHECK` — sem ele, `char(1)` aceita qualquer caractere (ver seções 4 e 14) | **Testado** | `TiposDeColunaTestes.Coluna_de_caractere_fixo_unico_tem_check_constraint` |
| View sobre view | `[V]` `X_V_BI_DESPESAS_VENDA_MAQUINAS` tem 719 linhas de SQL — um relatório inteiro compilado como view (25 views do Vórtice têm 80 ou mais linhas de SQL); a cadeia de views sobre `EXT_NFS`/`IMP_NFS` (achado 1) escondeu por 17 meses que a integração de faturamento havia parado, porque ninguém enxergava a origem através das camadas | **Recomendação** — não representável no modelo do EF Core (views não fazem parte dele nesta fase); `rel.FonteRelatorio` (doc 04, seção 12) aponta para **uma** view que já aplica o filtro de segurança, nunca para uma cadeia | revisão de PR |
| Trigger | `[V]` o único trigger do Vórtice (`VTC_T_AUDITORIA`) tem três defeitos estruturais: assume uma única linha por operação (`UPDATE` multilinha só audita a última lida, em silêncio); declara `AFTER INSERT, UPDATE, DELETE` mas **não trata `DELETE`**; tem cinco IDs de propriedade **hardcoded** no corpo (achado 5) | **Recomendação** — trigger não aparece no modelo do EF Core, então não é mecanicamente detectável a partir dele; lógica de efeito colateral vai para `SaveChangesAsync`/outbox (doc 03, seção 5.3), nunca para o banco | revisão de PR |

---

## 14. Qualidade de dado que o banco garante por estrutura

Regra sem teste é recomendação, não padrão. As garantias desta seção são exatamente as que o
banco impõe **por estrutura** — `CHECK`, `FK`, unicidade, `NOT NULL`, collation de comparação —
não por revisão humana nem por confiança no código da aplicação. O detalhamento de qualidade de
dado (deduplicação, sanitização, métricas, migração do dado legado) fica inteiramente em
[16-HIGIENIZACAO-DE-DADOS](16-HIGIENIZACAO-DE-DADOS.md) *(em elaboração por outra frente)* — esta
seção só lista o que é **garantido pela forma da coluna**, não pelo valor que chega nela.

| Garantia estrutural | Mecanismo | Status |
|---|---|---|
| Domínio fechado (enum, `char(1)`) | `CHECK constraint` | **Testado** — `TiposDeColunaTestes.Coluna_de_caractere_fixo_unico_tem_check_constraint`, `IntegridadeReferencialTestes.Coluna_baseada_em_enum_tem_check_constraint_de_dominio` |
| Integridade referencial (a linha filha só existe se a pai existir) | `FOREIGN KEY` declarada | **Testado** — `IntegridadeReferencialTestes.Toda_referencia_a_entidade_mapeada_tem_foreign_key_declarada` |
| Unicidade de negócio (ex.: `ChavePublica`, `Codigo` de catálogo) | índice único (`UX_...`) | **Recomendação** — o *prefixo* `UX_` é testado (seção 3); a decisão de **quais** colunas de negócio precisam de unicidade (ex.: `CpfCnpj` por empresa, como o doc 04 já modela para `crm.Conta`) é revisão de PR, caso a caso |
| Obrigatoriedade (`NOT NULL`) como regra do banco, não só validação da aplicação | `IsRequired()`/tipo não anulável no C# | **Recomendação** — não há teste que audite "toda coluna que deveria ser obrigatória é `IsRequired()`"; é revisão de PR |
| Comparação de texto sem distinguir acento (`"Jose"` encontra `"José"`) | colação **accent-insensitive** (`_AI`) — `Latin1_General_CI_AI` no banco inteiro | **Testado contra o banco de verdade** — `MigracaoNoContainerTestes.A_restricao_de_dominio_recusa_o_rotulo_que_so_difere_por_acento`; ver nota abaixo |
| A colação certa mesmo quando o banco NASCE com a errada — o caso de produção, dentro da instância do Vórtice | `ALTER DATABASE ... COLLATE` idempotente na migração inicial, antes do primeiro `CreateTable` | **Testado contra o banco de verdade** — `ColacaoNoContainerTestes` (dois testes), que criam o banco com `SQL_Latin1_General_CP1_CI_AS` de propósito |
| A coluna de papel só aceita item **daquele** catálogo | chave estrangeira **composta** contra `AK_CatalogoItem_CatalogoId`, com a coluna de catálogo constante | **Testado** — `CatalogoDeSistemaTestes` |
| O ponteiro polimórfico só nomeia entidade que existe | `CHECK` com lista fechada gerada do modelo, em `COLLATE Latin1_General_BIN2` | **Testado** — `DominioDeEntidadeTestes` |
| Unicidade de rótulo sem distinguir caixa nem acento (`Soja` e `SOJA` são o mesmo item) | a mesma colação, sob o índice único `UX_CatalogoItem_Catalogo_Descricao` | **Testado contra o banco de verdade** — `MigracaoNoContainerTestes.A_restricao_de_dominio_recusa_o_valor_invalido_no_banco` |

> **Nota sobre colação — decidida.** O Vórtice usa `SQL_Latin1_General_CP1_CI_AS` —
> *case-insensitive*, mas **accent-sensitive** (confirmado na investigação de 02/09/2026, doc 12,
> seção 5.1). Isso significa que uma busca por "Jose" não encontra "José" — um defeito de
> usabilidade discreto, mas real, numa base de nomes próprios brasileiros.
>
> **A colação do banco novo é `Latin1_General_CI_AI`**, decidida e aplicada — ver
> [20-DECISAO-SQL-SERVER, seção 4.3 e 4.4](20-DECISAO-SQL-SERVER.md). Ela vale para o banco
> inteiro e nasce em três lugares versionados (regra 9.5): o `MSSQL_COLLATION` do container de
> desenvolvimento, o `CREATE DATABASE ... COLLATE` de `scripts/banco/subir-banco.ps1`, e um
> `ALTER DATABASE ... COLLATE` idempotente na própria migração inicial. O terceiro é o que importa
> em produção, onde o banco nasce dentro da instância do Vórtice e herdaria a colação dele.
>
> **A contrapartida, dita em voz alta:** com colação insensível a caixa no banco inteiro, um
> `CHECK` como `[Situacao] IN ('Aberto','Suspenso')` passa a aceitar `'ABERTO'` escrito à mão por
> fora da aplicação. Onde a distinção de caixa é a própria regra — a UF em duas maiúsculas
> (`CK_Endereco_Uf`), o resumo do documento em hexadecimal minúsculo (`CK_Documento_Resumo`) —, a
> restrição usa `COLLATE Latin1_General_BIN2` explicitamente e continua distinguindo.

---

## 15. Como cada regra é verificada

Verificação, neste projeto, acontece de três formas — nenhuma delas é "documentação apenas":

1. **Teste automatizado** (`dotnet test`, roda no CI) — a forma forte. É o que existe para toda
   regra marcada **[Testado]** nas seções 2 a 14.
2. **Revisão de PR** — a forma que cobre o que o modelo do EF Core não consegue enxergar (conteúdo
   de coluna, configuração física do banco, escolha de collation, decisão de qual campo precisa de
   unicidade). É o que existe para toda regra marcada **[Recomendação]**.
3. **Script de provisionamento** — para configuração física que não é schema (ex.: `AUTO_SHRINK`,
   crescimento de arquivo) — ver seção 10 e "Entregável 2" abaixo.

### 15.1 Índice reverso — por arquivo de teste

Um teste falhou no CI. Esta tabela responde "qual regra, e onde está documentada":

**`ModeloBanco.cs`** — não é um arquivo de teste; é o *fixture* compartilhado. Constrói o modelo
do `CrmDbContext` com o provedor SQL Server (nunca abre conexão real), usando o modelo de
*design time* (`IDesignTimeModel`) — o único que expõe `CheckConstraints`. Todos os arquivos
abaixo dependem dele, **menos** `MigracaoNoContainerTestes.cs` e `ColacaoNoContainerTestes.cs`,
que falam com um banco de verdade.

**`EsquemaENomenclaturaTestes.cs`** (seções 2, 3 e 13):

| Teste | Regra | Seção |
|---|---|---|
| `Toda_entidade_esta_num_schema_da_lista_fechada_e_nenhuma_esta_em_dbo` | schema fechado, nada em `dbo` | 2 |
| `Os_dez_schemas_do_modelo_unificado_existem_e_somam_setenta_e_duas_tabelas` | o portão de 72 tabelas em 10 schemas (doc 17, seção 10.2) | 2.1 |
| `O_schema_padrao_do_contexto_esta_na_lista_fechada` | `HasDefaultSchema` também na lista | 2 |
| `Toda_tabela_tem_chave_primaria` | PK obrigatória | 3, 13 |
| `A_chave_primaria_e_a_coluna_Id_salvo_nas_tabelas_particionadas_por_data` | PK sempre `Id`; a exceção são as 4 particionadas, onde o SQL Server exige a coluna de particionamento na chave | 3, 10 |
| `Nome_de_tabela_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore` | forma do nome de tabela | 3 |
| `Nome_de_coluna_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore` | forma do nome de coluna | 3 |
| `Nome_de_coluna_nao_usa_abreviacao_proibida` | a palavra inteira, nunca `Qtd`/`Cod`/`Dta`/`Vlr` (doc 15, seção 5) | 3 |
| `Nenhuma_tabela_ou_coluna_usa_nome_de_backup_teste_ou_migracao` | tokens proibidos (`BKP`/`OLD`/`TESTE`/`TMP`/`MIG`) | 3, 13 |
| `Indices_e_chaves_seguem_o_prefixo_padrao` | prefixo `PK_`/`AK_`/`FK_`/`IX_`/`UX_`/`CK_` | 3 |
| `Todo_nome_de_objeto_cabe_no_limite_de_identificador_do_SqlServer` | 128 caracteres, e sem colisão dentro do schema | 3 |

**`TiposDeColunaTestes.cs`** (seção 4, tocando 6 e 14):

| Teste | Regra | Seção |
|---|---|---|
| `HasColumnType_literal_so_aparece_com_tipo_da_lista_justificada` | proibido `HasColumnType(...)` fora da lista curta (hoje: `nvarchar(max)` para JSON) | 4 |
| `Nenhuma_coluna_do_modelo_resolve_para_um_tipo_proibido` | nenhuma coluna resolve para `text`/`ntext`/`image`/`money`/`float`/`real`/`datetime`/`sql_variant` — verificado contra o TIPO RESOLVIDO, não contra a configuração | 4 |
| `Texto_sem_tamanho_maximo_so_e_permitido_na_lista_de_justificativas` | `HasMaxLength` obrigatório; cada exceção NOMEADA em `TextoIlimitadoJustificado` | 4 |
| `Toda_coluna_de_json_tem_check_de_json_valido` | toda coluna daquela lista tem `CHECK (ISJSON([Coluna]) = 1)` — é o que substitui a validação que o tipo `jsonb` fazia sozinho | 4 |
| `Todo_decimal_declara_precisao_e_escala_explicitas` | `HasPrecision(p, s)` obrigatório | 4 |
| `Toda_coluna_de_dinheiro_e_decimal_18_2` | dinheiro é sempre `decimal(18,2)` — enxerga através do tipo de valor `Dinheiro` | 4 |
| `Nenhuma_propriedade_booleana_e_anulavel` | `bool`, nunca `bool?` | 4 |
| `Toda_coluna_de_data_e_hora_e_datetime2` | nunca `datetime` nem `smalldatetime` | 4 |
| `Toda_data_hora_declara_precisao_de_milissegundos` | `datetime2(3)` sempre | 4 |
| `Coluna_de_caractere_fixo_unico_tem_check_constraint` | `char(1)` exige `CHECK` | 4, 14 |

**`IntegridadeReferencialTestes.cs`** (seção 6, tocando 4 e 7):

| Teste | Regra | Seção |
|---|---|---|
| `Toda_foreign_key_declarada_tem_indice_pelo_prefixo_das_colunas` | FK sempre indexada | 7 |
| `Toda_referencia_a_entidade_mapeada_tem_foreign_key_declarada` | FK sempre declarada (heurística por nome) | 6 |
| `Exclusao_em_cascata_so_e_usada_na_lista_de_excecoes_documentadas` | `Cascade` só na lista de exceções | 6 |
| `Coluna_baseada_em_enum_tem_check_constraint_de_dominio` | `enum` → `CHECK` | 4, 6 |
| `Toda_restricao_de_verificacao_usa_sintaxe_de_SqlServer` | nome de coluna entre colchetes na restrição de verificação — compara com a lista real de colunas da tabela | 6 |
| `Todo_indice_filtrado_filtra_por_coluna_que_existe_na_tabela` | o filtro do índice só cita coluna que existe, e só entre colchetes | 7 |

**`AuditoriaTestes.cs`** (seção 5):

| Teste | Regra | Seção |
|---|---|---|
| `Entidade_transacional_mantem_todas_as_colunas_de_auditoria` | as 8 colunas de `EntidadeBase` presentes | 5.1 |
| `Toda_entidade_transacional_tem_controle_de_concorrencia_otimista` | `rowversion` gerado pelo banco em toda entidade transacional | 5.1 |
| `Colunas_de_auditoria_de_quem_fez_nunca_sao_texto_livre` | `CriadoPorId`/`AlteradoPorId` sempre `bigint` | 5.1 |
| `Toda_tabela_transacional_declara_a_coluna_de_multiempresa` | `EmpresaId` em toda tabela transacional | 5.2 |

**`MigracaoNoContainerTestes.cs`** — falam com um banco de VERDADE (o outro arquivo assim é
`ColacaoNoContainerTestes.cs`, mais abaixo). Criam
um banco próprio (`TracbelCrmMigracaoTeste`), aplicam a migração e conferem o catálogo do SQL
Server. Sem container respondendo, são **ignorados com a razão escrita** — nunca falham por falta
de infraestrutura, nunca passam em silêncio. Categoria `BancoReal`, para o CI poder filtrar
(`dotnet test --filter "Categoria!=BancoReal"`).

| Teste | Regra | Seção |
|---|---|---|
| `A_migracao_inicial_cria_os_dez_schemas_e_as_sessenta_e_tres_tabelas` | o portão de 63/10, no banco de verdade | 2.1 |
| `Nenhuma_tabela_do_modelo_nasce_no_schema_dbo` | nada em `dbo` — inclusive a tabela de controle do EF | 2 |
| `As_quatro_tabelas_de_log_e_auditoria_estao_particionadas_por_data` | partição mensal E índice alinhado nas 4 tabelas de log | 10 |
| `Toda_coluna_de_data_e_hora_do_banco_e_datetime2_de_milissegundos` | nenhuma coluna `datetime`/`smalldatetime` no banco criado | 4 |
| `A_restricao_de_dominio_recusa_o_valor_invalido_no_banco` | o banco recusa 'SOJA' depois de 'Soja' — unicidade de rótulo sem distinguir **caixa** | 14 |
| `A_restricao_de_dominio_recusa_o_rotulo_que_so_difere_por_acento` | o banco recusa 'Sója' depois de 'Soja' — unicidade sem distinguir **acento**, que é a propriedade que a colação `_AI` comprou e a do Vórtice não dá | 14 |
| `A_concorrencia_otimista_recusa_a_segunda_gravacao_em_cima_da_primeira` | `rowversion` funcionando: a segunda gravação recebe erro, não sobrescreve | 5.1 |

**`CatalogoDeSistemaTestes.cs`** (seções 6 e 8) — o achado I-1 do
[documento 21](21-AUDITORIA-ESTRUTURA-E-NOMES.md):

| Teste | Regra | Seção |
|---|---|---|
| `Toda_coluna_que_aponta_para_item_de_catalogo_usa_chave_estrangeira_composta` | chave composta contra `AK_CatalogoItem_CatalogoId`; exceção genérica só com entrada em `ReferenciaGenericaJustificada` | 6 |
| `A_coluna_de_catalogo_da_chave_composta_e_uma_constante_presa_pelo_banco` | a coluna de catálogo tem valor padrão **e** `CHECK` com o mesmo número de `CatalogosDeSistema` | 6 |
| `Os_catalogos_de_sistema_nascem_semeados_com_o_identificador_que_o_esquema_usa` | o seed do modelo bate com a lista do código — a exceção nomeada da regra 8.4 | 8 |

**`DominioDeEntidadeTestes.cs`** (seções 6 e 14) — o achado I-3:

| Teste | Regra | Seção |
|---|---|---|
| `Toda_coluna_que_nomeia_entidade_tem_o_dominio_fechado_pela_lista_do_modelo` | as 10 colunas `Entidade`/`EntidadeRaiz` têm `CHECK` com a lista do modelo, em `COLLATE Latin1_General_BIN2`; lista divergente do modelo falha | 6, 14 |
| `Toda_coluna_que_nomeia_campo_exige_identificador_do_padrao_de_nomenclatura` | as 4 colunas `Campo` só aceitam identificador PascalCase ASCII | 3, 6 |

**`ColacaoNoContainerTestes.cs`** (seções 9.5 e 14) — o achado I-2. Também categoria `BancoReal`. Cria o banco **com a colação do Vórtice** de propósito, porque é o único cenário em que o `ALTER DATABASE ... COLLATE` da migração faz alguma coisa — no contêiner, cuja instância já nasce `Latin1_General_CI_AI`, o comando nunca chegava a executar:

| Teste | Regra | Seção |
|---|---|---|
| `A_migracao_corrige_a_colacao_do_banco_que_nasceu_com_a_colacao_do_Vortice` | a colação do banco depois da migração é `Latin1_General_CI_AI`, e uma coluna sem colação declarada herdou a corrigida — o que prova também a **ordem** do comando | 9.5, 14 |
| `No_banco_nascido_errado_e_corrigido_a_unicidade_ja_ignora_acento` | a prova de comportamento: `UX_Catalogo_Codigo` recusa `'SÉGMENTO'` ao lado de `'SEGMENTO'` | 14 |

**Total: 40 métodos de teste contra o modelo + 9 contra o banco de verdade, em 8 arquivos, 0
falhas** (conferido em 04/09/2026, depois das correções dos três achados "impede" do
[documento 21](21-AUDITORIA-ESTRUTURA-E-NOMES.md)).

---

## 16. Checklist de revisão de PR que toca no banco

Uma página. Se a resposta for "não" em qualquer item **testado**, o CI já bloqueia o merge — esta
lista é para pegar o que o CI não vê.

**Schema e nome**
- [ ] O schema usado está na lista fechada da seção 2.1? Se é novo, a PR atualiza o teste **e**
      este documento juntos (seção 2.2)?
- [ ] O nome de tabela/coluna é a palavra do [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), não
      uma sigla nova?

**Tipo e coluna**
- [ ] Nenhum `HasColumnType(...)` foi escrito à mão?
- [ ] Toda coluna nova de texto/decimal/data segue a seção 4 (`HasMaxLength`, `HasPrecision`,
      `HasPrecision(3)`)?
- [ ] Telefone é `varchar` (ou o tipo de valor `Telefone`), nunca numérico?
- [ ] `nvarchar` para texto de gente, `varchar` só para código ASCII?

**Integridade**
- [ ] Toda coluna `<Entidade>Id` nova tem `HasForeignKey` declarada — inclusive quando o nome não
      bate exatamente (referência por papel, ver limite da heurística na seção 6)?
- [ ] Se há `DeleteBehavior.Cascade`, ele está na lista `CascadeJustificado` **com o motivo**?
- [ ] Todo `enum` novo gravado como coluna tem `CHECK` enumerando os valores?
- [ ] Toda FK nova tem índice cobrindo a coluna?
- [ ] Coluna nova que aponta para `metadado.CatalogoItem` usa `LigarAoCatalogoDeSistema` (chave composta + catálogo constante), e o catálogo está em `CatalogosDeSistema`?
- [ ] Entidade nova regenerou a lista fechada das dez colunas `Entidade`/`EntidadeRaiz` numa migração? (O teste avisa; a migração é quem resolve.)

**Auditoria e multiempresa**
- [ ] A entidade nova herda `EntidadeBase` (transacional) ou tem justificativa explícita para não
      herdar?
- [ ] Tem `EmpresaId NOT NULL`, a menos que seja catálogo global (ex.: `Permissao`)?

**Proibições (seção 13)**
- [ ] Nenhuma tabela/coluna nova contém `BKP`, `OLD`, `TESTE`, `TMP` ou `MIG` no nome?
- [ ] Nenhuma regra de negócio está sendo gravada como SQL dentro de uma coluna de dado?
- [ ] Nenhuma view nova consulta outra view (aponta para tabela ou para a `FonteRelatorio`
      curada)?
- [ ] Nenhum trigger está sendo criado?

**Migration**
- [ ] A migration é aditiva (não renomeia, não remove coluna em uso)?
- [ ] Se remove coluna, o código já parou de usá-la num deploy anterior (seção 9.3)?
- [ ] Tabela de log/histórico nova já nasce com política de retenção decidida (seção 10)?

**Documentação**
- [ ] Se a PR muda uma regra desta lista, ela atualiza o teste correspondente em
      `tests/Tracbel.Crm.Arquitetura.Testes/Banco/` **e** este documento, na mesma PR — nunca só
      um dos dois?
