# Decisão de banco: SQL Server — CRM Tracbel

> Documento 20 · Versão 1.0 · 04/09/2026
> **Status: decisão tomada e JÁ IMPLEMENTADA.** O código deste repositório roda sobre SQL Server
> nesta data: provedor trocado, nomes em PascalCase, `rowversion`, colação `Latin1_General_CI_AI`,
> JSON em `nvarchar(max)` com `CHECK (ISJSON(...) = 1)`, particionamento por função e esquema de
> partição, migração inicial regerada do zero e aplicada num contêiner de verdade.
>
> Este documento **substitui** o [19-DECISAO-POSTGRESQL](19-DECISAO-POSTGRESQL.md), que fica no
> repositório com o conteúdo intacto: a análise que levou ao PostgreSQL continua válida como
> análise, e é justamente o que torna esta segunda decisão auditável em vez de arbitrária.

---

## 1. A decisão em cinco linhas

1. O banco do CRM próprio da Tracbel é **SQL Server**, criado como um **banco novo dentro da
   instância que já hospeda o Vórtice** (`10.150.14.65:1433`, onde já vivem `CRM` com 35 GB e
   `CRM_HOMO` com 26 GB).
2. O motivo é **operacional, não técnico**: o time e o parceiro de infraestrutura operam SQL
   Server, a licença já existe, e a migração do Vórtice deixa de ser integração entre servidores
   para virar **consulta entre bancos na mesma instância**.
3. **Desempenho não decidiu nada.** No volume medido — banco final na ordem de 18 GB, 150 a 250 mil
   linhas por ano no núcleo, 128 a 180 usuários ativos por mês — os dois motores empatam, e dizer
   outra coisa seria inventar um critério para justificar uma decisão que já tinha outro motivo
   (seção 3).
4. A convivência na instância do Vórtice vem com **condições**, e elas não são negociáveis: arquivos
   de dado e log próprios, `AUTO_SHRINK` desligado, nível de compatibilidade atual, destino de
   backup próprio, janela de manutenção própria (seção 6).
5. A decisão é **reversível**: um banco SQL Server muda de instância — ou de hospedagem inteira —
   com `BACKUP` e `RESTORE`. O que a torna irreversível não é o motor, é o volume de dado
   histórico migrado (seção 10).

---

## 2. O histórico honesto — SQL Server, PostgreSQL, SQL Server

Este projeto mudou de motor duas vezes em três dias. Registrar isso sem enfeite é o que impede a
próxima pessoa de refazer o mesmo caminho por não saber que ele já foi percorrido.

| Quando | Decisão | Motivo declarado | O que ficou dela |
|---|---|---|---|
| Até 03/09/2026 | **SQL Server** | É o que a Tracbel opera, e o que o [doc 03, seção 10](03-ARQUITETURA.md) e o [doc 04](04-MODELO-DADOS.md) assumiam desde o início | O [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md) e o [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md) foram escritos aqui — em PascalCase, com `datetime2(3)` e `rowversion`. **Nunca deixaram de valer** |
| 03–04/09/2026 | **PostgreSQL** ([doc 19](19-DECISAO-POSTGRESQL.md)) | Custo de licença por núcleo, container Linux nativo, portabilidade, `jsonb`, particionamento, índice parcial | Uma implementação completa e verde: 63 tabelas, 166 chaves estrangeiras, 4 tabelas particionadas, 189 testes. E **sete regras que eram recomendação viraram teste** — esse ganho é permanente, seção 8 |
| 04/09/2026 | **SQL Server** (este documento) | Operação: time, parceiro, licença existente, e a migração do Vórtice virando consulta entre bancos | O que está no repositório hoje |

**O que mudou entre a decisão 19 e esta.** Não foi um argumento técnico novo — foi o peso relativo
de argumentos que a própria seção 3 do doc 19 já listava como custo, e a resposta a duas das cinco
perguntas que aquele documento deixou em aberto na seção 9:

- **Pergunta 1** (*existe padrão corporativo/licença que já cubra SQL Server sem custo marginal?*)
  — a licença existe e está paga. Era, por escrito, "o único argumento que eu recomendaria pesar de
  verdade" contra a decisão 19: o próprio doc 19 marcou esse cenário em 🔴 na seção 3 e o listou
  como o primeiro motivo de revisão na seção 10. Ele apareceu.
- **Pergunta 2** (*o parceiro de infraestrutura opera PostgreSQL?*) — a resposta prática é que ele
  opera SQL Server, e a instância que hospedaria o banco novo já está sob a operação dele.

O terceiro fato não era pergunta em documento nenhum, e é o que mais muda o trabalho concreto do
programa: **a migração do dado do Vórtice deixa de ser integração e vira consulta entre bancos na
mesma instância** (seção 8).

**O que NÃO mudou:** nenhuma regra do [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md) foi afrouxada
nesta volta. As sete regras que a rodada PostgreSQL transformou de recomendação em teste continuam
testadas, e duas ficaram mais fortes (seção 8).

---

## 3. Os números — por que desempenho não decidiu

Um bom motivo para desconfiar de uma troca de motor é quando ela vem embrulhada em promessa de
desempenho. Esta não vem. Os números do próprio programa dizem por quê:

| Grandeza | Valor | Fonte |
|---|---|---|
| Tamanho estimado do banco novo, regime | **~18 GB** | projeção do programa a partir do dado útil do Vórtice, depois da higienização do [doc 16](16-HIGIENIZACAO-DE-DADOS.md) |
| Linhas por ano no núcleo (processo, tarefa, interação) | **150 a 250 mil** | volumetria da extração de 02/09/2026 |
| Usuários ativos por mês | **128 a 180** | acesso medido no Vórtice |
| Banco do Vórtice hoje, para comparação | `CRM` **35 GB** · `CRM_HOMO` **26 GB** | extração de 02/09/2026 |

Nessa faixa, **os dois motores cabem folgadamente na memória de um servidor comum e nenhum dos dois
chega perto do próprio limite.** Um índice bem escolhido vale mais do que a escolha de motor por
uma ordem de grandeza; um índice esquecido custa mais do que qualquer diferença entre eles. Foi por
isso que a decisão foi tomada pelo critério que realmente diferencia os dois neste contexto: **quem
opera o banco às três da manhã.**

Dito o que é honesto dizer: o Vórtice tem 86,3 milhões de linhas em 767 tabelas — mas **42% disso é
log sem política de retenção** e 19,4 milhões de linhas são staging permanente. O dado de negócio
que sobrevive à higienização é a fração pequena, e é ela que dimensiona o banco novo.

---

## 4. O que muda no modelo, ponto a ponto

### 4.1 Nomes voltam a PascalCase

`comercial.Cliente`, `processo.Interacao`, `integracao.PontoDeSincronismo` — exatamente o que o
[15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md) sempre documentou. O `snake_case` foi imposição do
PostgreSQL, que rebaixa identificador não citado para minúsculo e transformaria `ChavePublica` em
`chavepublica`. Sem essa imposição, o nome no banco volta a ser **letra por letra igual ao nome em
C#** — não há mais uma tradução no meio, e o `EFCore.NamingConventions` saiu do projeto.

Os **schemas continuam com a palavra inteira** (`comercial`, `processo`, `organizacao`, `seguranca`,
`frota`, `documento`, `auditoria`, `integracao`, `metadado`, `relatorio`), e **nenhuma tabela vive
em `dbo`** — que é literalmente onde as 767 tabelas do Vórtice se amontoam. Isso é testado, contra o
modelo e contra o banco de verdade.

Prefixos de objeto voltam a maiúsculo: `PK_`, `AK_`, `FK_`, `IX_`, `UX_`, `CK_`.

### 4.2 Concorrência otimista: `rowversion`

A coluna `EntidadeBase.Versao` volta a ser um `rowversion` que o **banco** incrementa a cada
gravação da linha — inclusive na gravação que não passa pela aplicação. Na passagem pelo PostgreSQL
o papel era da coluna de sistema `xmin`. A promessa nunca mudou, e agora é **provada por um teste
que roda contra o banco de verdade**: duas sessões leem a mesma linha, a primeira salva, e a segunda
recebe `DbUpdateConcurrencyException` em vez de sobrescrever em silêncio
(`MigracaoNoContainerTestes.A_concorrencia_otimista_recusa_a_segunda_gravacao_em_cima_da_primeira`).

O comentário de `EntidadeBase.Versao`, que a rodada anterior deixou desatualizado e registrou como
pendência, foi corrigido na mesma mudança.

### 4.3 Busca sem acento: colação, não coluna derivada

**A coluna computada saiu.** No PostgreSQL, buscar "Jose" e encontrar "José" exigia uma coluna
gerada e persistida (`Cliente.NomeNormalizado`), calculada com `translate()`, mais um índice sobre
ela — porque a colação não determinística do PostgreSQL, que resolveria a comparação, **não aceita
`LIKE`**, e busca por prefixo é o caso de uso principal.

No SQL Server a colação `Latin1_General_CI_AI` resolve os dois de uma vez: **ignora caixa e acento
em igualdade, em `LIKE` e em unicidade de índice.** Então:

- `Cliente.NomeNormalizado` **deixou de existir** — coluna, índice e expressão (o modelo ficou com
  uma coluna a menos que a versão PostgreSQL; hoje são 754, depois das nove colunas de catálogo
  constante da migração de 04/09/2026 — ver documento 21, achado I-1);
- `Cliente.NomeRazao` ganhou a colação e o índice filtrado que era da coluna derivada;
- a garantia de unicidade sem caixa nem acento — a que impede `FINALIZADO` e `FINALIZADA` de
  conviverem no mesmo catálogo — passou a ser a colação de `metadado.CatalogoItem.Descricao` sob o
  índice único `UX_CatalogoItem_Catalogo_Descricao`. **O comportamento foi preservado e continua
  provado**: `A_restricao_de_dominio_recusa_o_valor_invalido_no_banco` grava `'Soja'` e depois
  `'SOJA'` no banco de verdade, e o segundo é recusado com violação de unicidade. Desde 04/09/2026
  há também o caso de **acento**, que é o que esta colação comprou e a do Vórtice não dá:
  `A_restricao_de_dominio_recusa_o_rotulo_que_so_difere_por_acento` grava `'Soja'` e depois
  `'Sója'`, e o segundo também é recusado (documento 21, achado I-2 — até então só a caixa era
  exercitada, e a colação do Vórtice recusaria a caixa igual).

Isto é uma **simplificação real**, não uma troca lateral: sai uma coluna, sai um índice, sai uma
expressão de banco, e a garantia fica mais forte, porque agora vale para busca por prefixo também.

> ⚠️ **A contrapartida, dita em voz alta.** A colação vale para o **banco inteiro** (seção 4.4).
> Isso significa que uma restrição de verificação como `[Situacao] IN ('Aberto','Suspenso')` passa a
> aceitar `'ABERTO'` escrito à mão por fora da aplicação. É um afrouxamento em relação ao
> PostgreSQL, onde a colação padrão do banco era determinística. Aceitamos porque: (a) o valor é
> sempre escrito pelo EF Core a partir do `enum` do C#, com a caixa exata; (b) o defeito medido no
> Vórtice é o **oposto** — duplicata semântica por variação de escrita —, e a colação insensível o
> ataca, não o cria; (c) onde a distinção de caixa é a própria regra (a UF em duas maiúsculas, o
> resumo do documento em hexadecimal minúsculo), a restrição usa `COLLATE Latin1_General_BIN2`
> explicitamente, e continua distinguindo.

### 4.4 Colação do banco, declarada na criação

`Latin1_General_CI_AI`, e ela nasce em **três lugares versionados**, nunca de configuração à mão no
servidor — [documento 14, regra 9.5](14-PADRAO-DE-BANCO.md), que é a lição direta do Vórtice estar
em nível de compatibilidade 100 sem ninguém saber por quê:

1. o contêiner de desenvolvimento (`MSSQL_COLLATION` em `infra/docker-compose.yml`), que fixa a
   colação do **servidor** na criação da instância;
2. o `CREATE DATABASE ... COLLATE` de `scripts/banco/subir-banco.ps1`;
3. a própria migração inicial (`ModeloInicial.ColacaoDoBanco.cs`), que emite um `ALTER DATABASE ...
   COLLATE` idempotente — **e é este que importa em produção**, porque a colação padrão da instância
   do Vórtice é `SQL_Latin1_General_CP1_CI_AS`, *accent-SENSITIVE*. Sem ele, um `CREATE DATABASE`
   sem `COLLATE` herdaria a colação do Vórtice, e "Jose" deixaria de encontrar "José" exatamente
   como acontece lá hoje.

**E este terceiro é exercitado por teste desde 04/09/2026.** Até a auditoria do documento 21
(achado I-2) ele não era: no contêiner, o item 1 desta lista fixa a colação do servidor e, por
consequência, a do `model`; como todo banco novo nasce do `model`, a condição do `IF` que guarda o
`ALTER` era sempre falsa e o comando nunca rodava — o bloco podia estar quebrado com todos os
testes verdes. `ColacaoNoContainerTestes` cria o banco **com a colação do Vórtice**
(`CREATE DATABASE ... COLLATE SQL_Latin1_General_CP1_CI_AS`), aplica a migração e exige que
`DATABASEPROPERTYEX(DB_NAME(),'Collation')` volte `Latin1_General_CI_AI`, que uma coluna sem
colação declarada (`metadado.Catalogo.Nome`) tenha herdado a colação já corrigida — o que também
prova a **ordem**, o `ALTER` antes do primeiro `CreateTable` — e que a unicidade de
`UX_Catalogo_Codigo` recuse `'SÉGMENTO'` ao lado de `'SEGMENTO'` nesse banco.

### 4.5 Tipos

| No PostgreSQL | No SQL Server |
|---|---|
| `timestamptz(3)` | `datetime2(3)` |
| `jsonb` | `nvarchar(max)` + `CHECK (ISJSON(coluna) = 1)` |
| coluna de sistema `xmin` | coluna `Versao` do tipo `rowversion` |
| `gen_random_uuid()` | `NEWID()` |
| `numeric(18,2)` | `decimal(18,2)` |
| `character varying(n)` / `text` | `nvarchar(n)` / `varchar(n)` |
| coluna gerada com `translate()` | — (some; ver 4.3) |
| índice parcial (`WHERE ...`) | índice filtrado (`WHERE ...`) |

Sobre `NEWID()` e não `NEWSEQUENTIALID()`: o sequencial fragmenta menos o índice, mas é **adivinhável
por construção** (deriva do relógio e do endereço MAC). `ChavePublica` é justamente o identificador
que a API expõe para que ninguém enumere registro contando `Id` — um valor adivinhável anularia o
propósito da coluna. Fragmentação é problema de manutenção; identificador previsível é problema de
segurança.

Sobre `datetime2(3)` e fuso: o SQL Server não tem tipo com fuso embutido — `datetimeoffset` carrega
o deslocamento, não o fuso, e não é o que o padrão pede. **A garantia de que todo instante é UTC vem
do tipo de valor `DataHoraUtc` no domínio**, e a garantia de faixa e precisão da coluna vem do teste
`Toda_coluna_de_data_e_hora_e_datetime2`, que verifica o tipo RESOLVIDO pelo provedor — mais o teste
irmão que roda contra o banco de verdade. É honestamente uma garantia mais fraca do que o
`timestamptz` do PostgreSQL dava: lá o banco recusava um valor sem fuso; aqui quem garante é o
domínio. Registrado como o que é.

---

## 5. JSON — onde ele está e o que observar

Oito colunas guardam documento JSON. No PostgreSQL eram `jsonb`, um tipo que **valida a entrada por
si só**. No SQL Server são `nvarchar(max)` com `CHECK (ISJSON(coluna) = 1)` — o banco continua
recusando documento malformado, só que por restrição declarada em vez de por tipo.

| Coluna | O que guarda | Consulta por dentro? |
|---|---|---|
| `metadado.CampoPersonalizado.Validacao` | regras de validação do campo | não |
| `integracao.Recepcao.Conteudo` | a linha crua recebida da integração | não |
| `integracao.MensagemDeSaida.Conteudo` | a mensagem da fila de saída | não |
| `integracao.MensagemDescartada.Conteudo` | a mensagem que a fila desistiu de entregar | não |
| `relatorio.Relatorio.Definicao` | colunas, filtros e ordenação do relatório salvo | não |
| `comercial.Lead.PayloadOriginal` | o payload cru do RD Station | não |
| `processo.Regra.EfeitoParametros` | parâmetros do efeito, variam por efeito | não |
| `metadado.Resposta.ValorEstruturado` | resposta que não cabe em coluna escalar | **pode vir a exigir** |

> 📌 **Observação registrada.** Sete das oito guardam o documento inteiro e são lidas inteiras — a
> perda do `jsonb` não custa nada nelas. **Só `metadado.Resposta.ValorEstruturado` pode vir a exigir
> consulta por dentro** (escolha múltipla, matriz), e é para ela que o próximo passo seria `OPENJSON`
> ou um índice sobre coluna computada de `JSON_VALUE` — mecanismos que o SQL Server tem, com sintaxe
> diferente e sem índice invertido equivalente ao GIN. Não é trabalho para agora: a tabela só nasce
> na fase 3A. Está escrito aqui para que, quando a consulta transversal ("quantas vendas perdemos
> por preço em 2026?") chegar, ninguém descubra o assunto no meio da implementação.

A lista das oito não é uma nota de rodapé: ela é **código**. Cada uma está nomeada em
`TiposDeColunaTestes.TextoIlimitadoJustificado`, e um teste novo
(`Toda_coluna_de_json_tem_check_de_json_valido`) exige que cada uma tenha o seu `CHECK (ISJSON...)`.
Uma coluna de texto sem tamanho que não esteja nessa lista faz o `dotnet test` falhar.

---

## 6. Convivência na instância do Vórtice — as condições

O banco novo divide a instância `10.150.14.65:1433` com `CRM` (35 GB) e `CRM_HOMO` (26 GB), num
SQL Server **2019 Standard** (15.0.2135.5) sobre Windows Server 2019. Estas são as condições
aprovadas, e cada uma existe por um achado medido — não por precaução genérica.

| # | Condição | Por quê |
|---|---|---|
| 1 | **Arquivos de dado e log próprios**, em caminho próprio, nunca compartilhando arquivo com `CRM` ou `CRM_HOMO` | É o que permite mover o banco de instância com `BACKUP`/`RESTORE` sem tocar no Vórtice — a rota de reversão da seção 10 —, e o que impede o crescimento de um de espremer o outro |
| 2 | **`AUTO_SHRINK` desligado no nosso banco** | `[V]` achado 9 da extração: `is_auto_shrink_on = True` em `CRM` **e** em `CRM_HOMO`. Num banco de dezenas de GB com log em `FULL`, o auto-shrink causa fragmentação contínua de índice e picos de I/O imprevisíveis. Aplicado por `subir-banco.ps1` em dev e pelo runbook em produção ([doc 14, seção 10](14-PADRAO-DE-BANCO.md)) |
| 3 | **Crescimento de arquivo em MB fixo**, nunca percentual | `[V]` o arquivo de dados do Vórtice cresce de 10 em 10 por cento — antipadrão reconhecido em arquivo desse porte |
| 4 | **Nível de compatibilidade ATUAL da instância**, nunca o 100 do Vórtice | `[V]` o banco do Vórtice está em *compatibility level* 100 (SQL Server 2008) rodando sobre SQL Server 2019, e ninguém sabe documentar quando ou por que isso foi fixado. Herdar esse nível significaria abrir mão de 11 anos de otimizador — e herdá-lo **por acidente** é o risco real, porque nível de compatibilidade é herdado do `model` na criação |
| 5 | **Destino de backup próprio** — e **NUNCA** o share `\\10.150.6.230\bkpbd` | 🔴 Achado de segurança 1 do [inventário de binários](../extracao-vortice/binarios/INVENTARIO-BINARIOS.md), seção 8.2: aquele share contém o dump completo do banco de produção (29,9 GB) com ACL NTFS `Todos = ReadAndExecute` na raiz. Qualquer conta da rede copia a base inteira: clientes, propostas, CPFs, faturamento. Gravar nosso backup lá seria publicar o CRM novo no mesmo lugar |
| 6 | **Janela de manutenção própria** | Reindexação, atualização de estatística e expurgo de partição não podem cair na janela do Vórtice — os dois competem pela mesma CPU, memória e I/O (seção 7) |
| 7 | **Login próprio da aplicação**, com permissão só no banco novo | `[V]` no Vórtice **todos os usuários compartilham o mesmo login SQL `crm`**, com a senha em texto claro em três arquivos de configuração legíveis por qualquer usuário do terminal server — o que elimina qualquer rastreabilidade por usuário no banco. Não repetir isso é barato |

> 🔒 **Nada foi criado na instância de produção nesta rodada.** Todo o trabalho aconteceu no
> contêiner local. A criação do banco em produção é trabalho da infraestrutura, com o runbook
> derivado desta seção.

---

## 7. A limitação da edição Standard, sem rodeio

**A edição Standard não isola processador nem memória entre bancos da mesma instância.** O
Resource Governor — o recurso que permitiria dizer "o CRM novo não pode passar de X% de CPU" — é
**exclusivo da Enterprise**. Na Standard:

- os dois bancos disputam o **mesmo pool de memória** (o *buffer pool* é da instância, não do
  banco) e o mesmo conjunto de CPUs;
- uma consulta pesada no Vórtice — e ele tem 25 views com 80 ou mais linhas de SQL, uma delas com
  719 — pode degradar o CRM novo, e vice-versa;
- o limite de memória da edição Standard (128 GB de *buffer pool*) vale para a **instância inteira**,
  somando os dois.

O que **temos** na Standard, e é o que sustenta a decisão: particionamento de tabela (desde o
SQL Server 2016 SP1 — seção 9), `BACKUP`/`RESTORE` por banco, compressão de backup, e configuração
por banco de recovery, auto-shrink, crescimento de arquivo e nível de compatibilidade — que é
exatamente a lista da seção 6.

**Mitigações reais, na ordem em que se aplicam:**

1. A condição 6 da seção 6 (janela de manutenção própria) tira o pior caso, que é a reindexação dos
   dois ao mesmo tempo.
2. O volume da seção 3 dá folga: um banco de ~18 GB cabe inteiro em memória num servidor de porte
   comum, e o que ele tira do *buffer pool* é pequeno diante do que o Vórtice já ocupa.
3. Se a disputa virar problema medido — e a única forma de saber é **medir**, não supor —, a saída
   é **instância separada**, que é `BACKUP`/`RESTORE` (seção 10), não reescrita.

> 📌 **O que precisa ser confirmado com a infraestrutura:** se a instância tem folga de memória e
> CPU para mais um banco ativo, e se existe hoje qualquer monitoração de espera (`sys.dm_os_wait_stats`)
> que permita comparar antes e depois. Sem essa medida de linha de base, "o CRM novo deixou o Vórtice
> lento" vira uma discussão sem árbitro.

---

## 8. O que ganhamos — e o que a passagem pelo PostgreSQL deixou

### 8.1 A migração do Vórtice vira consulta entre bancos

É o ganho concreto que nenhum documento anterior tinha na mesa. Com o CRM novo na **mesma
instância** do Vórtice, ler o legado passa a ser `SELECT ... FROM CRM.dbo.IV_Processo` — mesma
transação, mesmo motor, mesmo plano de execução. Some do caminho:

- o *linked server* ou a exportação intermediária que uma migração entre servidores exigiria;
- a janela de cópia de dezenas de GB pela rede;
- a classe inteira de defeito de conversão de tipo e de codificação de caractere na travessia.

A carga histórica do [doc 16](16-HIGIENIZACAO-DE-DADOS.md) deixa de ser um projeto de ETL entre
plataformas e vira um conjunto de instruções `INSERT ... SELECT` com transformação — revisáveis,
repetíveis e, principalmente, **transacionais**. `[V]` A integração do Vórtice não tem transação: o
`BEGIN TRANSACTION` está literalmente comentado nas procedures, e o resultado medido são 783.242
títulos (R$ 5,18 bi) presos em staging desde maio de 2025.

> ⚠️ **A regra que continua valendo, e fica mais importante, não menos.** A arquitetura proíbe a
> aplicação de ler o Vórtice direto: a fronteira é `Tracbel.Crm.Integracao/Vortice`
> ([doc 03, seção 1](03-ARQUITETURA.md)). Estar na mesma instância torna a violação dessa regra
> *fácil* pela primeira vez — antes ela era impossível por acidente. A facilidade é para a **carga
> histórica**, que é um evento com começo e fim; não é permissão para a aplicação consultar `CRM.dbo`
> em tempo de execução. Vale a pena um teste de arquitetura que barre isso quando a camada de
> integração for escrita.

### 8.2 O que a passagem pelo PostgreSQL deixou de bom, e ficou

A rodada anterior não foi desperdício. Ela endureceu o padrão, e **nada disso foi devolvido**:

**As sete regras que viraram teste continuam testadas:**

| Regra ([doc 14](14-PADRAO-DE-BANCO.md)) | Teste |
|---|---|
| Multiempresa obrigatória (`EmpresaId` em toda tabela transacional) — era seção 5.2, "[Recomendação, sem teste hoje]" | `AuditoriaTestes.Toda_tabela_transacional_declara_a_coluna_de_multiempresa` |
| Abreviação proibida em nome de coluna (doc 15, seção 5) | `EsquemaENomenclaturaTestes.Nome_de_coluna_nao_usa_abreviacao_proibida` |
| Limite de identificador do motor | `EsquemaENomenclaturaTestes.Todo_nome_de_objeto_cabe_no_limite_de_identificador_do_SqlServer` |
| Sintaxe da restrição de verificação | `IntegridadeReferencialTestes.Toda_restricao_de_verificacao_usa_sintaxe_de_SqlServer` |
| Coluna do índice filtrado existe na tabela | `IntegridadeReferencialTestes.Todo_indice_filtrado_filtra_por_coluna_que_existe_na_tabela` |
| Dinheiro é sempre `decimal(18,2)` | `TiposDeColunaTestes.Toda_coluna_de_dinheiro_e_decimal_18_2` |
| O portão de 63 tabelas em 10 schemas (doc 17, seção 10.2) | `EsquemaENomenclaturaTestes.Os_dez_schemas_do_modelo_unificado_existem_e_somam_sessenta_e_tres_tabelas` |

**E duas ficaram mais fortes na tradução de volta:**

1. **Texto sem tamanho.** No PostgreSQL, uma coluna escapava do `HasMaxLength` obrigatório só por
   ser `jsonb` — exclusão genérica por tipo. Aqui, cada uma das oito colunas de JSON está **nomeada
   uma a uma** em `TextoIlimitadoJustificado`, e ainda precisa do seu `CHECK (ISJSON(...))` para
   passar no teste novo da seção 5. Uma coluna nova sem tamanho não passa, tenha o tipo que tiver.
2. **Sintaxe da restrição de verificação.** A versão PostgreSQL procurava colchete e proibia. Esta
   versão compara com a **lista real de colunas da tabela**: se um identificador que é coluna
   aparece fora de colchete, falha. Ela detecta o erro em vez de detectar um caractere.

---

## 9. Particionamento na Standard — confirmado

As mesmas quatro tabelas, com a mesma retenção declarada:

| Tabela | Coluna de partição | Retenção |
|---|---|---|
| `auditoria.AlteracaoDeCampo` | `AlteradoEm` | 18 meses disponíveis, arquivamento depois |
| `auditoria.EventoDeAcesso` | `OcorreuEm` | 24 meses (exigência de LGPD) |
| `processo.RegraExecucao` | `ExecutadoEm` | 12 meses disponíveis, arquivamento depois |
| `integracao.Recepcao` | `RecebidaEm` | efêmera — expurgo pela coluna `ExpurgarApos` |

**Particionamento de tabela deixou de ser exclusividade da Enterprise no SQL Server 2016 SP1.** A
edição Standard da instância de produção suporta função de partição, esquema de partição, índice
alinhado, `SWITCH` e `TRUNCATE TABLE ... WITH (PARTITIONS ...)` — que é o conjunto exato de que o
expurgo precisa. Nada em `ModeloInicial.Particionamento.cs` depende de Enterprise.

**Como está implementado, e por que é SQL bruto:** o EF Core não tem API de particionamento. O
`CreateTable` gerado cria as quatro tabelas no filegroup padrão; o bloco escrito à mão cria a função
e o esquema de partição de cada uma, e **recria a chave primária (que é o índice clusterizado) e
todos os índices não clusterizados sobre o esquema** — que é como o SQL Server particiona uma
tabela. Recriar os índices não é zelo: **índice desalinhado impede `SWITCH` e `TRUNCATE` por
partição**, que são exatamente o motivo de particionar. Há teste para isso, contra o banco de
verdade.

**Uma diferença a favor do SQL Server, honestamente:** no PostgreSQL era preciso criar uma partição
`DEFAULT` de escape, sem a qual uma linha com data fora da faixa criada seria **recusada** pelo
banco. A função de partição do SQL Server cobre o domínio inteiro por construção — sempre há uma
partição à esquerda do primeiro limite e outra à direita do último. Se o job mensal que cria os
limites seguintes falhar, **nada é recusado**; a última partição só fica maior. Um modo de falha a
menos.

---

## 10. Reversão

**Reverter para outro motor é caro; reverter para outra instância ou outra hospedagem é barato.**
São coisas diferentes, e vale separar:

| Cenário | Custo |
|---|---|
| Sair da instância do Vórtice para uma instância própria | **Baixo, e continua baixo.** `BACKUP DATABASE` + `RESTORE DATABASE`. É por isso que a condição 1 da seção 6 (arquivos próprios) existe |
| Ir para Azure SQL Managed Instance / VM dedicada | **Baixo.** O mesmo `BACKUP`/`RESTORE`, mais a conversa de rede e latência que o [doc 12, seção 19](12-DECISAO-CONTAINERS.md) já tem em aberto |
| Voltar para PostgreSQL | **Barato hoje, caro depois.** Hoje é apagar `Migrations/`, trocar o provedor e gerar de novo — o que esta rodada acabou de provar, nas duas direções, em um dia. Depois da carga histórica da fase 6, deixa de ser opção prática por qualquer motor |

O que preserva a opção, e continua valendo palavra por palavra do
[doc 19, seção 8](19-DECISAO-POSTGRESQL.md): EF Core como única camada de acesso a dado; domínio que
não conhece provedor nenhum (testado por arquitetura); migrations geradas, nunca editadas à mão; e
`HasColumnType(...)` literal proibido fora de uma lista curta e revisada em PR — que é o registro,
num lugar só, de onde o banco é SQL Server de verdade.

### O que faria esta decisão ser revista

1. **A instância do Vórtice não aguentar mais um banco ativo** — medido, não suposto (seção 7). A
   resposta natural seria instância separada, não motor diferente.
2. **A licença de SQL Server se revelar embutida no contrato do Vórtice** e não transferível para um
   sistema novo — é a pergunta 3 da [seção 9 do doc 19](19-DECISAO-POSTGRESQL.md), que continua sem
   resposta formal. Se a licença não existir de fato, o argumento de custo da decisão 19 volta
   inteiro.
3. **A ausência de índice invertido para JSON virar gargalo real** em
   `metadado.Resposta.ValorEstruturado` depois da fase 3A (seção 5) — e mesmo aí, a resposta
   provável é um índice sobre coluna computada de `JSON_VALUE`, não trocar de motor.
4. **A decisão corporativa de nuvem apontar para um provedor onde o custo de licença por núcleo
   volte a pesar** — o cenário que o doc 19 descreveu, revertido.

---

## 11. Onde isso muda os documentos

Aplicado nesta rodada:

| Documento | O que mudou |
|---|---|
| [19-DECISAO-POSTGRESQL](19-DECISAO-POSTGRESQL.md) | Marcado como **substituído**, com link para este. Conteúdo **intacto** — a análise continua valendo como análise |
| [README do programa](README.md) | Índice, tabela de stack e decisões travadas |
| [docs/banco/README](../banco/README.md) | Passo a passo de subir o banco, agora SQL Server |
| [12-DECISAO-CONTAINERS](12-DECISAO-CONTAINERS.md), [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md) | Só as menções que ficaram **factualmente erradas** — não uma reescrita |
| `docs/banco/DICIONARIO.md`, `ERD.md`, `catalogo.csv`, `esquema.json` | Regerados do modelo, com os nomes novos |

---

## 12. Decisão registrada

- **Data:** 04/09/2026.
- **Quem decidiu:** Ricardo Coradini de Marco Moretti (arquiteto/tech lead do programa,
  [doc 13, seção 5.1](13-PROGRAMA-POR-FASES-DIRETORIA.md)).
- **Critério declarado:** operacional — time, parceiro de infraestrutura, licença existente e a
  migração do Vórtice virando consulta entre bancos. **Não** desempenho (seção 3).
- **Estado da implementação nesta data:** completa e verificada. `dotnet build` sem aviso,
  191 testes passando (189 anteriores mais dois novos: o `CHECK (ISJSON)` de toda coluna de JSON e a
  concorrência otimista contra o banco de verdade), contêiner saudável, migração aplicando e
  revertendo, 63 tabelas em 10 schemas conferidas no catálogo do banco.
- **O que fica em aberto, e com quem:** as confirmações da seção 6 e da seção 7, com a
  infraestrutura, antes de criar o banco em produção. Nenhuma delas bloqueia o desenvolvimento, que
  roda inteiro no contêiner local.
