# Carteira e município — o agrupamento territorial que existe de verdade

> **Documento 26** · Versão 1.0 · 05/09/2026
> Cobre `src/Tracbel.Crm.Dominio/Organizacao/Municipio.cs` (as duas entidades novas),
> `src/Tracbel.Crm.Infraestrutura/Migrations/20260905191622_MunicipioEAgrupamentoDeCarteira.cs`
> (a migração aditiva), `src/Tracbel.Crm.Carga/CargaDoVortice.cs` e
> `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs` (as duas cargas),
> `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs` e
> `src/Tracbel.Crm.Api/Endpoints/EndpointsDeTerritorio.cs` (as três rotas).
> Toda medição deste documento saiu de execução real contra o Vórtice em produção, **somente
> leitura**, e contra o banco do CRM depois da carga. Nenhum número é estimativa.

---

## 1. O achado: a regional não existe, a carteira e a filial existem

O programa vinha assumindo que a Tracbel Agro se organiza por **regional** — é o que a tela de
Cobertura do protótipo mostra, com sete delas: MT Norte, GO, BA Oeste e outras quatro. A premissa
estava errada, e quem a corrigiu foi o gerente comercial:

> *"Se você olhar no CRM da Vórtice temos as carteiras dos CENs, e nas carteiras temos a filial que
> ele vai atuar e quais cidades ele terá na carteira."*

Verificamos ao vivo. Ele está certo, e o dado é inequívoco:

| Objeto do Vórtice | O que guarda | Linhas |
|---|---|---:|
| **`IVS_Regional`** | a "regional" que o protótipo desenha | **0** |
| `IVS_Carteira.NroEmpresa` | a filial em que a carteira atua | preenchida em **655 de 655** |
| `IVS_CartCid` (`SeqCarteira`, `SeqCidade`) | as cidades de cada carteira | **673**, em **91 carteiras** |
| `GE_Cidade` | o catálogo de municípios | **10.214** |

**A tabela de regional existe no esquema e está vazia.** Não é que ela esteja pouco usada: ela
nunca recebeu uma linha. As sete regionais do protótipo são invenção da maquete — não há nada no
sistema atual que as sustente, nem tabela, nem coluna, nem valor de texto.

O que existe preenchido é o par **filial → carteira → municípios**. Exemplo real, a carteira 6:

```
CAJURU / SP
CASSIA DOS COQUEIROS / SP
SANTA CRUZ DA ESPERA / SP      <- truncado na origem; ver secao 4
SANTO ANTONIO DA ALE / SP      <- truncado na origem; ver secao 4
```

### 1.1 A confirmação pelo avesso: as filiais que não operam

As três filiais fora da lista oficial de treze — **Guaíra (4), Ituverava (5) e Monte Alto (10)** —
têm carteira cadastrada e **nenhuma cidade**:

| Filial | Carteiras | Com cidade |
|---|---:|---:|
| 4 — Guaíra | 32 | **0** |
| 5 — Ituverava | 39 | **0** |
| 10 — Monte Alto | 36 | **0** |

Cento e sete carteiras, zero municípios. É a mesma conclusão a que a carga do
[documento 24](24-CARGA-DE-DADOS-2026.md) chegou por outro caminho — elas não operam —, agora
confirmada por um segundo indício independente.

### 1.2 A cobertura é rala, e isso também é o achado

Das **655 carteiras** do Vórtice, só **91** declaram alguma cidade. Não corrigimos isso nem
preenchemos o que falta: o CRM novo carrega o que existe e **conta o que não existe**, e a API
devolve essa lacuna em `metricasSemDado` em vez de mostrar uma cobertura menor sem explicar.

---

## 2. O problema de modelo que o achado revelou

`comercial.Endereco.Municipio` era **texto livre obrigatório** — `nvarchar(120)`, sem catálogo,
sem chave estrangeira. Isso viola a nossa própria regra, escrita no
[documento 16, seção 3](16-HIGIENIZACAO-DE-DADOS.md): *campo com catálogo não aceita digitação
livre*.

E a violação não era teórica. A carga de 2026 **normalizou 275 municípios com espaço duplo** —
`SANTA RITA DO  PASSA` — porque o campo aceitava qualquer coisa. Se a coluna fosse chave
estrangeira, aquelas 275 linhas nem teriam chegado a existir.

Havia ainda um agravante que só apareceu quando fomos olhar a origem: **o Vórtice tem o catálogo e
o texto ao mesmo tempo**. `GE_Pessoa` guarda `SeqCidade`, que aponta para `GE_Cidade`, **e**
`Cidade`, texto livre, lado a lado. Os dois **divergem em 217 cadastros** — o texto é o que
envelheceu. Nós tínhamos importado justamente a metade ruim.

> **A regra que passa a valer, dita pelo Ricardo e adotada como dura:** *"nada de dado solto que a
> pessoa digita, isso fica muito bagunçado."* Município é seleção, daqui para frente. O texto
> sobrevive só como resíduo de migração, marcado como tal, com a conta de quanto falta e o que
> fecha a conta — seção 7.

---

## 3. O modelo novo

Duas tabelas em `organizacao`, e uma chave estrangeira em `comercial.Endereco`.

```
organizacao.Empresa ──< organizacao.Carteira ──< organizacao.CarteiraMunicipio >── organizacao.Municipio
                                                                                            ^
                                                                                            │
                                                              comercial.Endereco ───────────┘
```

### 3.1 `organizacao.Municipio` — o catálogo nacional

| Coluna | Tipo | Regra |
|---|---|---|
| `Id` | `int` | chave primária. `int` porque o Brasil tem 5.570 municípios |
| `CodigoIbge` | `int` **nulo** | a chave natural, **quando existir** — `UX_Municipio_CodigoIbge`, único e filtrado |
| `Nome` | `nvarchar(120)` | obrigatório |
| `Uf` | `char(2)` | obrigatório, **domínio fechado**: `CK_Municipio_Uf` enumera as 27 |
| `EstaAtivo` | `bit` | desligar sem apagar (fusão, emancipação) |

**Unicidade de negócio, declarada nas duas formas que fazem sentido:**

- `UX_Municipio_Uf_Nome` — único **sempre**. Sob a colação `Latin1_General_CI_AI` do banco, ele já
  recusa `CAJURU` ao lado de `Cajuru` e de `Cajurú`.
- `UX_Municipio_CodigoIbge` — único **quando o código existe**, com
  `HasFilter("[CodigoIbge] IS NOT NULL")`. O filtro é o que permite as duas coisas ao mesmo tempo:
  a chave natural presa pelo banco, e a carga de hoje podendo trazer município sem o código.

Índice de busca `IX_Municipio_Nome`, filtrado em `[EstaAtivo] = 1` — é o que a tela usa.

### 3.2 `organizacao.CarteiraMunicipio` — o território da carteira

| Coluna | Tipo | Regra |
|---|---|---|
| `Id` | `bigint` | chave primária |
| `CarteiraId` | `bigint` | chave estrangeira para `organizacao.Carteira`, `Restrict` |
| `MunicipioId` | `int` | chave estrangeira para `organizacao.Municipio`, `Restrict` |
| `VinculadoEm` / `VinculadoPorId` | `datetime2(3)` / `bigint` | quem colocou a cidade na carteira, e quando |
| `DesvinculadoEm` | `datetime2(3)` nulo | encerrar sem apagar |

`UX_CarteiraMunicipio_Carteira_Municipio_Vigente` sobre `(CarteiraId, MunicipioId)`, filtrado em
`[DesvinculadoEm] IS NULL`: **o par não se repete enquanto vigente**. O filtro existe porque
desvincular não apaga a linha — e a mesma cidade pode voltar à mesma carteira depois, o que criaria
uma segunda linha legítima.

`IX_CarteiraMunicipio_MunicipioId` cobre a consulta inversa, "que carteira atende esta cidade?".
Toda chave estrangeira fica indexada — a de carteira pelo prefixo do índice único, a de município
pelo próprio.

### 3.3 `comercial.Endereco` — a ligação, aditiva

| Mudança | O quê |
|---|---|
| **coluna nova** | `MunicipioId int NULL`, chave estrangeira `Restrict`, com `IX_Endereco_MunicipioId` filtrado em `[ExcluidoEm] IS NULL` |
| **relaxamento** | `Municipio nvarchar(120)` deixou de ser `NOT NULL` |
| **restrição nova** | `CK_Endereco_Municipio`: `([MunicipioId] IS NOT NULL AND [Municipio] IS NULL) OR ([MunicipioId] IS NULL AND [Municipio] IS NOT NULL)` |

**A migração é aditiva**: adiciona coluna, relaxa uma obrigatoriedade, cria duas tabelas. Não
renomeia nada, não remove nada, não perde uma linha.

---

## 4. As decisões de padrão, uma a uma — e por quê

Cada decisão abaixo é cobrada por um teste de `tests/Tracbel.Crm.Arquitetura.Testes/Banco/`, que
aplica o [documento 14](14-PADRAO-DE-BANCO.md).

### 4.1 Schema `organizacao`, e não um schema novo

O [documento 14, seção 2.2](14-PADRAO-DE-BANCO.md), diz que schema novo é decisão registrada, não
conveniência. Consideramos um `localidade` e **descartamos**: `organizacao` já guarda a geografia
da operação comercial — `Praca` mora lá e tem `Uf` —, e o município é a unidade atômica dessa mesma
geografia. Um schema com duas tabelas seria um schema criado para acomodar duas tabelas.

O par carteira × município não tinha dúvida: ele é atributo da carteira, e carteira é
`organizacao`.

**A conta de tabelas mudou, e mudou nos dois lugares na mesma PR**, como a regra exige: de **63
para 65**, `organizacao` de 6 para 8. Os testes
`EsquemaENomenclaturaTestes.Os_dez_schemas_do_modelo_unificado_existem_e_somam_sessenta_e_cinco_tabelas`
e `MigracaoNoContainerTestes.A_migracao_inicial_cria_os_dez_schemas_e_as_sessenta_e_cinco_tabelas`
foram renomeados junto com o número — o nome do teste é parte do contrato.

### 4.2 A coluna de empresa: **nenhuma das duas a tem**, e é decisão, não esquecimento

Esta é a decisão que o teste de multiempresa vai cobrar coerência, e ela é diferente para cada
tabela:

**`organizacao.Municipio` não tem `EmpresaId` porque município é dado nacional.** Cajuru é Cajuru
para a filial de Ribeirão Preto e para a de Marília. Copiar `EmpresaId` aqui produziria treze
cópias do mesmo Brasil — que é exatamente o antipadrão que a
[regra 11.1 do documento 14](14-PADRAO-DE-BANCO.md) proíbe pelo nome: *"proibido criar schema por
filial, banco por filial, ou tabela replicada por filial"*. E teria uma consequência funcional
ruim: o CEN de Ribeirão Preto **precisa** poder cadastrar um cliente em Uberaba, e uma tabela de
municípios filtrada por filial impediria isso.

**`organizacao.CarteiraMunicipio` não tem `EmpresaId` porque ela herda a fronteira pela carteira.**
A carteira tem a coluna e tem o filtro global; o vínculo é a ponte entre duas entidades, uma das
quais carrega a fronteira. É a mesma forma de `comercial.ClienteCarteira`, que também não tem a
coluna e pelo mesmo motivo. Uma segunda cópia de "de quem é esta linha" pode divergir da primeira,
e uma fronteira que pode divergir de si mesma não é fronteira.

**Como isso fecha com os testes.** `AuditoriaTestes.Toda_tabela_transacional_declara_a_coluna_de_multiempresa`
exige `EmpresaId` de toda entidade que herda `EntidadeBase` — e nenhuma das duas herda (4.3).
`MultiempresaTestes.Toda_entidade_com_EmpresaId_tem_filtro_global_que_usa_a_coluna` exige filtro de
quem tem a coluna — e nenhuma das duas tem. Não foi preciso abrir exceção em
`FronteiraDeEmpresaJustificada`: **a lista de exceções continua com as mesmas duas entradas de
antes**, o que é o sinal de que a decisão está coerente com o padrão, e não contornando-o.

**A fronteira continua fechada na prática.** As duas consultas de cobertura do
`RepositorioDeTerritorio` partem de `organizacao.Carteira`, que carrega o filtro global — nenhuma
delas escreve um `WHERE` de empresa à mão, que é a regra 11.3. A rota de municípios é a única
deliberadamente não filtrada, pelo motivo do parágrafo acima.

### 4.3 Nenhuma das duas herda `EntidadeBase`, e a auditoria vai para onde importa

`Municipio` é dado de referência, como `LinhaDeNegocio` e `Praca` — ninguém "cria um município" no
CRM; a lista existe antes do primeiro cliente e muda por decreto federal. O bloco de oito colunas
de auditoria responde *"quem mexeu neste registro de negócio"*, e aqui a resposta seria sempre "a
carga".

**O que precisa de trilha é o vínculo**, e ele tem: `VinculadoEm`, `VinculadoPorId`,
`DesvinculadoEm`. Tirar uma cidade de uma carteira é decisão comercial — muda a quem o cliente
daquela cidade pertence — e ela nunca apaga a linha, só a encerra. É a mesma forma de
`ClienteCarteira`, que é o precedente do projeto para tabela de vínculo.

### 4.4 A UF é domínio fechado por `CHECK`, com colação binária

```sql
CK_Municipio_Uf: [Uf] COLLATE Latin1_General_BIN2 IN ('AC','AL',...,'TO')
```

A colação binária **não é enfeite**: a do banco é `Latin1_General_CI_AI`, que ignora caixa e
acento, e sem o `COLLATE` a restrição aceitaria `'sp'` e `'PÁ'` — que são literalmente dois dos
valores que a UF de texto livre do Vórtice guarda hoje. É a contrapartida da colação insensível já
antecipada no [documento 14, seção 14](14-PADRAO-DE-BANCO.md).

### 4.5 O código do IBGE **não existe na origem**, e o risco disso está medido

`GE_Cidade` **não tem coluna de IBGE**. As colunas dela são `SeqCidade` (sequencial próprio),
`Cidade`, `Uf`, `Regiao`, `Populacao`, faixa de CEP, DDD e datas. Então:

**Como identificamos o município:** por **nome mais UF**, normalizado como o banco normaliza
(maiúsculas, sem acento, espaço colapsado — `CargaDoVortice.ChaveDeMunicipio`).

**Os três riscos disso, medidos e não estimados:**

**Risco 1 — 180 pares (nome, UF) repetidos na origem**, somando 415 linhas com UF válida.
`CATANDUVA/SP` aparece mais de uma vez com sequenciais diferentes. Do lado de cá, `UX_Municipio_Uf_Nome`
não admite isso — e é bom que não admita: duas linhas para a mesma cidade são duas verdades. **A
carga escolhe a de menor sequencial como canônica** (a consulta já vem ordenada assim, o que torna
a escolha determinística entre execuções) **e aponta todas as chaves de origem do grupo para ela**
no de-para de `integracao.ChaveExterna`. Nenhum vínculo se perde: um endereço que apontava para a
segunda linha chega no mesmo município.

**Risco 2 — o nome está truncado em 20 caracteres em 947 das 10.214 linhas**, e só **3** passam de
20. É uma marca d'água de truncamento clássica, a mesma classe já registrada no
[documento 24](24-CARGA-DE-DADOS-2026.md) para `Fantasia` e `NomeRazao`. A coluna é `varchar(50)`
hoje, mas o dado entrou por algum caminho que cortava em 20. É por isso que a carteira 6 atende
`SANTA CRUZ DA ESPERA` e não *Santa Cruz da Esperança*.

**Risco 3 — o truncamento colapsa municípios diferentes.** Três pares de UF válida se tornam
idênticos depois do corte, e a carga funde cada par numa linha só:

| Nome truncado | UF | Sequenciais na origem | O que provavelmente são |
|---|---|---|---|
| `SAO SEBASTIAO DO RIO` | MG | 8730, 8731 | São Sebastião do Rio Preto **e** São Sebastião do Rio Verde — **dois municípios reais e distintos** |
| `SAO SEBASTIAO DO BAR` | MG | 8715, 8716 | dois municípios de MG com o mesmo prefixo |
| `CAMPO ALEGRE DE MINA` | MG | 1764, 1765 | duplicata do mesmo, ou dois com o mesmo prefixo |

**O custo real hoje é zero, e isso foi verificado, não presumido:** nenhum dos seis sequenciais
acima é referenciado por vínculo de carteira (`EmCarteira = 0`) nem por cadastro de pessoa
(`EmPessoa = 0`). O risco é para o futuro, e o que o fecha é a seção 8.1.

Um quarto grupo, `SAO JOSE DO RIO PRET` com 19 linhas, tem UF `**` e é recusado antes de chegar
aqui.

### 4.6 O resto do padrão, sem novidade

Schema com palavra inteira e nada em `dbo`; tabela em PascalCase sem acento nem underscore; chave
primária `Id`; chave estrangeira `<Entidade>Id`; toda chave estrangeira indexada; prefixos `PK_`,
`FK_`, `IX_`, `UX_`, `CK_`; `datetime2(3)` em toda data; nenhum `HasColumnType` escrito à mão;
`DeleteBehavior.Restrict` em todas as três chaves estrangeiras novas — nenhuma entrou em
`CascadeJustificado`, porque nenhuma delas é coleção *owned* de agregado.

Nomes sem abreviação: `CodigoIbge`, não `CodIbge`. `Municipio`, não `Munic`.

---

## 5. A carga

Duas etapas, no mesmo padrão idempotente das cargas 24 e 25 — de-para em
`integracao.ChaveExterna`, recusa registrada em `integracao.MensagemDescartada`, marca em
`integracao.PontoDeSincronismo`. Dois fluxos novos: `VORTICE.CARGA.MUNICIPIO` e
`VORTICE.CARGA.CARTEIRA_MUNICIPIO`.

### 5.1 O catálogo de municípios — `GE_Cidade`

| | |
|---|---:|
| Linhas lidas em `GE_Cidade` | **10.214** |
| Aceitas | **9.985** |
| Recusadas — UF fora das 27 | **229** |
| Municípios distintos gravados | **9.750** |
| Chaves de origem que caíram num município já existente | **235** |

`9.985 − 235 = 9.750`: a conta fecha, e os 235 são a duplicação do catálogo antigo resolvida
(seção 4.5, risco 1). O de-para de `integracao.ChaveExterna` recebe **9.985 linhas** — uma por
chave de origem, inclusive as 235 repetidas, que apontam para o mesmo município.

**Esta é a única leitura sem filtro de toda a carga**, e é deliberada: catálogo se traz inteiro.
Trazer só as cidades que o recorte usa entregaria à tela de cadastro uma lista incompleta — que é
exatamente como o campo voltaria a ser digitado à mão. São 10.214 linhas de uma tabela de
referência, quatro ordens de grandeza abaixo do histórico.

**A reexecução prova a idempotência:** a segunda rodada leu as mesmas 10.214 linhas e gravou
`0 novos`, mantendo os 9.750.

### 5.2 O território das carteiras — `IVS_CartCid`

| | |
|---|---:|
| Linhas em `IVS_CartCid` | 673 |
| Lidas — só as das **13 filiais em operação** | **553** |
| Vínculos carteira × município gravados | **532** |
| Recusados — carteira fora da carga | **21** |
| Carteiras com pelo menos uma cidade | **73** |
| Carteiras com cidade na origem | 91 |

**As 120 linhas que a consulta nem lê são das filiais fora das treze** — 4 carteiras da filial
`891`, que não corresponde a nenhuma filial em operação do CRM. O filtro é pela filial da carteira
(`IVS_Carteira.NroEmpresa`), o mesmo recorte das cargas 24 e 25.

**As 21 recusadas não são perda de dado: é o recorte.** A carga de relacionamento
([documento 25](25-CARGA-PROCESSO-AGENDA-CARTEIRA.md)) traz as **142 carteiras** que têm cliente
com atividade em 2026; as outras não entraram, e o território delas também não. Exemplo real da
recusa, gravado: *"A carteira 71 não entrou na carga — ela não tem cliente do recorte, ou é de
filial que não opera."* Pela mesma razão, as 18 carteiras de diferença entre 91 e 73 têm cidade
cadastrada e **nenhum cliente ativo em 2026** — o cadastro territorial existe, a operação não.

Cada uma das 21 está em `integracao.MensagemDescartada`, no fluxo
`VORTICE.CARGA.CARTEIRA_MUNICIPIO`, com o motivo em português e a linha crua em JSON — conferido
no banco depois da carga.

**O município nunca é procurado por nome nesta etapa.** Ele vem do de-para que a carga de cadastro
deixou pronto: a origem aponta para a cidade por sequencial, e o de-para traduz — inclusive quando
o sequencial é uma das 235 linhas duplicadas, porque todas as chaves do grupo apontam para o mesmo
município. Vínculo cujo município não está no de-para é recusado e contado, nunca adivinhado.

**Desvincular não apaga.** O par que a origem deixou de declarar é encerrado com data
(`DesvinculadoEm`), e reaparece reaberto se a origem voltar a declará-lo. Perder o rastro de "esta
cidade já foi desta carteira" é perder a resposta de por que um cliente daquela cidade tem o
histórico que tem.

### 5.3 A ligação do endereço, e as duas tentativas

O endereço tenta duas vezes, **nesta ordem, e a ordem é a da confiança**:

| Tentativa | Como | Endereços | % |
|---|---|---:|---:|
| **1ª — o ponteiro da origem** | `GE_Pessoa.SeqCidade` → de-para → município | **19.218** | 97,85% |
| **2ª — o nome mais a UF** | texto normalizado contra `UX_Municipio_Uf_Nome` | **2** | 0,01% |
| **não casou** | ficou com o texto do legado | **421** | 2,14% |
| **total** | | **19.641** | 100% |

**19.220 de 19.641 endereços casaram — 97,86%.**

**Por que o ponteiro vem primeiro.** `GE_Pessoa` tem as duas coisas: `SeqCidade`, que aponta para
`GE_Cidade`, e `Cidade`, texto livre ao lado. O ponteiro está preenchido em **114.686 dos 119.359**
cadastros da origem (96%) e é o que o sistema de origem de fato usa; os dois **divergem em 217
cadastros**, e o texto é o que envelheceu. Buscar pelo texto primeiro seria preferir a metade
podre.

**Que o nome só resolva 2 casos é o resultado esperado, não uma decepção.** Quando o ponteiro não
resolve, quase sempre é porque o texto também é ruim — as duas colunas foram preenchidas pela mesma
tela, no mesmo momento.

---

## 6. O que não casou, contado

### 6.1 Os 229 municípios recusados na origem

Recusados por **UF fora das 27**, com o valor registrado linha a linha em
`integracao.MensagemDescartada`:

| Valor da UF | Linhas |
|---|---:|
| `**` | 210 |
| `EX` | 11 |
| `PÁ` | 5 |
| `MI` | 2 |
| *(vazio)* | 1 |
| **total** | **229** |

`PÁ` é `PA` com acento; `MI` é provavelmente `MG` digitado errado; `EX` parece "exterior". **A
linha é recusada inteira, e não só o campo** — município sem estado não é ambíguo, é impossível:
existem quinze "Bom Jesus" no Brasil, e sem a UF nenhum deles é este. Entrar com UF nula tornaria
`UX_Municipio_Uf_Nome` inútil, que é justamente a defesa que o catálogo existe para dar.

**Nenhuma dessas 229 é referenciada por vínculo de carteira** — verificado: zero. **27 cadastros de
pessoa apontam para uma delas**, e esses endereços caem nos 421 da seção seguinte.

### 6.2 Os 421 endereços que não casaram — quinze textos explicam todos

| Texto no endereço | UF | Endereços | O catálogo tem | Causa |
|---|---|---:|---|---|
| `PALMEIRA DOESTE` | SP | 148 | `PALMEIRA D'OESTE` **e** `PALMEIRA D OESTE` | apóstrofo |
| `ESTRELA DOESTE` | SP | 101 | `ESTRELA D'OESTE` **e** `ESTRELA D OESTE` | apóstrofo |
| `SANTA RITA DOESTE` | SP | 56 | com apóstrofo | apóstrofo |
| `APARECIDA DOESTE` | SP | 49 | com apóstrofo | apóstrofo |
| `SANTA CLARA DOESTE` | SP | 38 | com apóstrofo | apóstrofo |
| `GUARANI DOESTE` | SP | 15 | com apóstrofo | apóstrofo |
| `BRODOSQUI` | SP | 5 | `BRODOWSKI` | grafia antiga |
| `SANTO ANTONIO DA ALEGRIA` | SP | 1 | `SANTO ANTONIO DA ALE` | **o catálogo é que está truncado** |
| `FRANCA` | MG | 1 | Franca é de SP | UF errada |
| `SÃO CARLOS` | AM / GO / MS | 3 | São Carlos é de SP | UF errada |
| `IBITINGA` | PR | 1 | Ibitinga é de SP | UF errada |
| `JABORANDI` | MG | 1 | é de SP e de BA | UF errada |
| `SÃO JOSÉ DA BELA VIS` | DF | 2 | é de SP | UF errada + truncado |
| **total** | | **421** | | |

**Três causas, e cada uma com um remédio diferente:**

1. **O apóstrofo — 407 dos 421 (96,7%).** Uma tecla. E o mais revelador: o catálogo da origem
   guarda **as duas grafias como linhas separadas** — `ESTRELA D'OESTE` e `ESTRELA D OESTE` —
   enquanto o cadastro de pessoa escreve uma terceira, `ESTRELA DOESTE`. Três grafias da mesma
   cidade no mesmo sistema, e nenhuma delas é chave de nada. É o argumento inteiro deste documento
   em uma linha.
2. **O truncamento do catálogo — 3 casos.** `SANTO ANTONIO DA ALEGRIA` e `SÃO JOSÉ DA BELA VISTA`
   estão **certos no endereço** e cortados no catálogo. A origem do defeito é a marca d'água de 20
   caracteres da seção 4.5.
3. **A UF errada — 11 casos.** Franca em MG, Ibitinga no PR, São Carlos em três estados que não o
   têm. São erros de digitação do cadastro antigo, e é exatamente o que a chave estrangeira passa a
   impedir daqui para frente.

**Nada foi inventado e nada foi descartado.** Os 421 entraram com `MunicipioId` nulo e o texto do
legado preservado, que é o resíduo que `CK_Endereco_Municipio` admite e a seção 7 conta.

---

## 7. O resíduo de migração, e o que fecha a conta

### 7.1 Como o banco impede o texto livre de voltar

```sql
CK_Endereco_Municipio:
  ([MunicipioId] IS NOT NULL AND [Municipio] IS NULL)
  OR ([MunicipioId] IS NULL AND [Municipio] IS NOT NULL)
```

**Uma coisa OU a outra, nunca as duas nem nenhuma.** Um `INSERT` que preencha as duas é recusado
pelo banco; um que preencha só o texto está declarando, no próprio dado, que aquela linha é dívida
de migração. E a conta do resíduo é uma consulta de uma linha:

```sql
SELECT COUNT(*) FROM comercial.Endereco WHERE MunicipioId IS NULL;   -- 421
```

### 7.2 Como o código impede, um nível acima

A aplicação não tem dois parâmetros opcionais que alguém possa combinar errado — tem **um tipo**:

```csharp
MunicipioDoEndereco.Selecionado(municipioId)              // o caminho de todo endereco novo
MunicipioDoEndereco.NaoIdentificadoNaCarga(textoDoLegado) // o unico caminho para o texto
```

`Endereco.Criar` e `Endereco.Alterar` recebem esse tipo, e não uma `string`. **Não existe
sobrecarga que aceite município como texto.** O nome do segundo método é longo de propósito:
ninguém o chama por engano a partir de um formulário, e ele aparece no código como o que é.

> **Por que a chave estrangeira não é obrigatória no banco.** Torná-la `NOT NULL` exigiria descartar
> 421 endereços reais de clientes reais, ou inventar município para eles. As duas coisas são piores
> do que a coluna anulável. O que substitui a obrigatoriedade é a combinação acima: a restrição de
> verificação prende o par no banco, e o tipo do domínio prende a porta de entrada. **Registro novo
> não tem como nascer sem município do catálogo** — é o que a assinatura de `Criar` garante.

### 7.3 O caminho de saída, escrito

| Passo | O que fecha | Endereços |
|---|---|---:|
| 1 — carregar o catálogo oficial do IBGE por cima do atual | apóstrofo, grafia, nome truncado, e traz o `CodigoIbge` | **410** |
| 2 — corrigir a UF dos 11 cadastros errados, um a um, com o comercial | UF errada | **11** |
| 3 — remover a coluna `Municipio`, um deploy depois | — | **0** |

O passo 1 é o que fecha 97% da conta e resolve os três riscos da seção 4.5 de uma vez. O passo 3
segue a [regra 9.3 do documento 14](14-PADRAO-DE-BANCO.md): remover coluna acontece **um deploy
depois** de o código parar de usá-la — e por isso a coluna **não foi renomeada** agora, apesar de
um nome autodeclarado ser melhor de ler. Renomear é o que a regra proíbe; o destino desta coluna é
sair, não trocar de nome.

**Enquanto a conta não zerar, ela fica visível:** o relatório da carga imprime as três contagens a
cada execução, e a consulta acima responde em uma linha.

---

## 8. O que isto significa para as telas, e a lista para as próximas rodadas

### 8.1 A tela de Cobertura mostra hoje regionais inventadas

O protótipo agrupa a Cobertura em **sete regionais** — MT Norte, GO, BA Oeste e mais quatro. Elas
não têm origem no dado: `IVS_Regional` tem zero linhas, e não há coluna nem valor de texto no
Vórtice que corresponda a elas.

**O que a tela passa a mostrar** é o agrupamento que existe, em dois níveis, e as rotas para os dois
já estão de pé (seção 8.2):

1. **Por filial** — 13 filiais em operação, com quantas carteiras cada uma tem, quantas delas
   declaram cidade, quantos municípios distintos atende e em quais estados.
2. **Por carteira** — dentro da filial, cada carteira com o CEN responsável, a linha de negócio e a
   lista das cidades.

**A cobertura territorial, filial a filial, medida no banco depois da carga:**

| Código | Filial | Carteiras | Com cidade | Municípios |
|---|---|---:|---:|---:|
| 010101 | Ribeirão Preto | 41 | 11 | **86** |
| 010115 | Jales | 6 | 5 | 35 |
| 010116 | Votuporanga | 11 | 7 | 31 |
| 010113 | São José do Rio Preto | 12 | 8 | 30 |
| 010112 | Itápolis | 7 | 2 | 28 |
| 010114 | Catanduva | 10 | 9 | 25 |
| 010102 | Araraquara | 8 | 3 | 15 |
| 010117 | Tupã | 7 | 5 | 14 |
| 010103 | Barretos | 13 | 8 | 13 |
| 010109 | Bebedouro | 8 | 4 | 13 |
| 010111 | Franca | 6 | 4 | 13 |
| 010107 | Orlândia | 9 | 4 | 11 |
| 010118 | Marília | 4 | 3 | 10 |
| | **total** | **142** | **73** | |

**As treze filiais em operação aparecem, e todas têm alguma cobertura** — o que confirma, pelo
terceiro caminho independente, que a lista de treze é a certa.

**A lacuna é declarada, não escondida.** Das 142 carteiras, **73 têm cidade e 69 não têm** — quase
metade. A rota `/api/v1/cobertura/filiais` devolve esse número em `metricasSemDado` com o texto
medido, e a carteira sem cidade aparece na lista com o array vazio em vez de sumir da tela —
escondê-la faria a tela mostrar uma operação menor do que ela é.

**Itápolis é o caso que a tela precisa saber explicar:** 7 carteiras, só 2 com cidade, e mesmo
assim 28 municípios. O número de municípios não é proporcional ao de carteiras, e uma tela que
somasse "cobertura" sem mostrar a coluna "com cidade" faria parecer que a filial cobre mais do que
o cadastro sustenta.

### 8.2 Como o front vai preencher o campo de município

Esta é a parte que a rodada das telas consome diretamente.

**Qual endpoint alimenta a lista:** `GET /api/v1/municipios`.

**A busca é por nome, com sugestão, e por PREFIXO.** `?termo=ribeir` encontra Ribeirão Preto;
`?termo=eirão` **não encontra nada**. É uma decisão de desempenho com consequência de
comportamento, e por isso está declarada no contrato (`ConsultaDeMunicipios`) e não escondida no
repositório: busca por trecho (`LIKE '%x%'`) não usa índice e varreria as 9.750 linhas a cada tecla
digitada. Quem digita o nome de uma cidade começa pelo começo.

**Acento e caixa não importam.** Sob a colação `Latin1_General_CI_AI` do banco, `?termo=sao` já
encontra `São`. O front não precisa normalizar nada antes de mandar.

**O filtro por estado é o que torna a lista utilizável:** `?uf=SP` reduz de 9.750 para algumas
centenas. A UF é validada contra as 27 e devolve **400 com o campo nomeado** quando não é uma
delas — uma UF inválida devolvendo lista vazia pareceria "não existe cidade", que é a resposta
errada para a pergunta errada.

**O que o formulário devolve ao gravar:** o `id` do município. Não o nome.

**O que acontece quando o município não está no catálogo:** a lista vem vazia, e **não há botão de
"criar município"** — de propósito. Este catálogo é a lista oficial de municípios do Brasil, não um
campo que cresce com o que o usuário digitou; é a diferença entre ele e os catálogos de
`metadado.Catalogo`, que trazem `permiteItemNovo = true`. Município que não aparece é **falta de
carga de catálogo**, e o caminho é um chamado, não um item novo criado pela tela. Hoje as ausências
conhecidas são as 15 grafias da seção 6.2, e todas somem no passo 1 da seção 7.3.

**Sequência sugerida no formulário:** escolher a UF, depois digitar o município. É a ordem que a
rota otimiza e a que reduz a ambiguidade de nomes repetidos entre estados — há São Carlos em mais
de um estado, e foi exatamente aí que três endereços do legado erraram.

### 8.3 A lista para as próximas rodadas: onde ainda há texto onde deveria haver lista

Encontrado enquanto trabalhávamos neste documento. **Nada abaixo foi corrigido nesta rodada** — a
lista é o que guia as próximas, como o critério do
[documento 16, seção 3](16-HIGIENIZACAO-DE-DADOS.md) manda registrar.

| Coluna | O que é hoje | O que deveria ser | Gravidade |
|---|---|---|---|
| `comercial.Endereco.Uf` | `CK_Endereco_Uf` verifica a **forma** (`LIKE '[A-Z][A-Z]'`) — `'XX'` passa | domínio fechado nas 27, como `CK_Municipio_Uf` faz; ou **derivar do município e remover**, junto com a coluna de texto (seção 7.3, passo 3) | média — e some sozinha quando o resíduo zerar |
| `organizacao.Praca.Uf` | mesma verificação de forma, `'XX'` passa | mesmo remédio | baixa — a tabela está vazia (seção 9) |
| `comercial.ClienteContato.Cargo` | `nvarchar(80)` livre; a carga despeja nele o `TipoContato`/`AreaAtuacao` do legado | item de catálogo — é uma lista curta e estável | **alta**: é texto do legado entrando cru |
| `comercial.Cliente.AtividadeEconomica` | `varchar(7)` livre | catálogo CNAE — existe lista oficial, como a do IBGE | média |
| `integracao.Sistema.MeioDeAcesso` | `varchar(40)` livre (`"SQL Server, somente leitura"`) | domínio fechado — são poucos meios | baixa: sete linhas, mexidas por quem mantém o código |
| `comercial.Endereco.Bairro` | `nvarchar(120)` livre | **fica livre**: não há catálogo nacional de bairro. Exceção justificada | — |

`Logradouro`, `Numero`, `Complemento`, `Identificacao` (o nome da fazenda) e as observações de
visita continuam livres, e é o certo — são a exceção justificada que o documento 16 prevê.

---

## 9. O destino de `organizacao.Praca`

**Fica, com o escopo reduzido e registrado. Não foi removida, e o motivo está abaixo.**

### 9.1 O que foi medido

| | |
|---|---:|
| Linhas em `organizacao.Praca` | **0** |
| Carteiras com `PracaId` preenchido | **0** de 142 |
| Equivalente no Vórtice | **nenhum** |

A praça é conceito do protótipo — o indicador "Conhecimento de mercado" da Visão 360, que hoje é
número fixo no JavaScript (`mercado-pracas.json`). A tabela nasceu vazia e continua vazia.

### 9.2 O que ela perde, e o que ela mantém

A praça acumulava **dois papéis**, e o município real desmonta um deles:

| Papel | Situação |
|---|---|
| **Territorial** — "onde a carteira atua" | **Perdido.** É agora `organizacao.CarteiraMunicipio`, com 532 vínculos de dado real. `Carteira.PracaId` é um segundo jeito de dizer a mesma coisa, nulo em 142 de 142, e agora com um concorrente que tem dado |
| **Potencial de mercado** — quanto se estima vender por linha e por ano | **Mantido.** `PotencialEstimado` e `MaquinasEstimadas` por `LinhaDeNegocioId` e `AnoReferencia` não têm nada equivalente em `Municipio`, que não guarda dinheiro nem previsão |

### 9.3 A decisão, com a justificativa

**1. A tabela `organizacao.Praca` fica**, com o escopo reduzido ao papel de potencial de mercado.
Removê-la levaria junto a única modelagem que o programa tem para "quanto este mercado vale", e não
há substituto. O que muda é a leitura: ela **deixa de ser o agrupamento territorial**, e nenhuma
tela deve usá-la para dizer onde a carteira atua.

**2. `Carteira.PracaId` é declarado obsoleto** e sai num deploy futuro. É ponteiro territorial
redundante, nulo em todas as 142 carteiras, e o território agora tem tabela própria com dado real.
Não sai agora pela [regra 9.3](14-PADRAO-DE-BANCO.md): remover coluna acontece um deploy depois de
o código parar de usá-la, e removê-la nesta mesma migração violaria a regra que este documento
inteiro se apoia.

**3. `Praca.Uf` fica na lista da seção 8.3.** Se um dia a praça precisar de território, o caminho é
uma tabela `PracaMunicipio` com a mesma forma de `CarteiraMunicipio` — não uma coluna de UF que
verifica forma e não domínio.

**4. Nada disso é feito por reflexo.** A praça não foi apagada porque o dado dela nunca existiu: o
que ela representa — potencial de mercado — continua sendo uma pergunta legítima do negócio, e a
resposta simplesmente ainda não foi carregada. Uma tabela vazia com propósito claro é dívida
conhecida; uma tabela apagada é a pergunta perdida.

---

## 10. Verificação

<!-- SECAO-VERIFICACAO -->
