# Higienização e saneamento de dados — CRM Tracbel

> **Documento 16** · Versão 1.0 · 03/09/2026
> Requisito explícito e repetido do Ricardo: **dado sujo não entra**. Este documento é
> normativo. Toda regra segue o mesmo formato: **regra → o defeito do Vórtice que ela evita,
> com número → onde se aplica (entrada, integração, migração) → como se verifica**.
> `[V]` = defeito medido no Vórtice, sempre com achado e número — nunca "o Vórtice é ruim
> nisso" sem prova.
>
> Nomes de schema/tabela seguem [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md):
> `comercial.Cliente`, `organizacao.*`, `processo.*`, `frota.*`, `integracao.*`,
> `seguranca.*`, `metadado.*`. Onde o [04-MODELO-DADOS](04-MODELO-DADOS.md) ainda usa o nome
> antigo (`crm.Conta`, `wf.Processo`), a leitura correta é a errata do documento 15.
>
> Código correspondente: tipos de valor em `src/Tracbel.Crm.Dominio/Comum/` (citados campo a
> campo na seção 2) e pipeline de saneamento em `src/Tracbel.Crm.Integracao/Saneamento/`
> (seção 8). Medição real: `scripts/dados/validar-seed.mjs` (protótipo, seção 7) e
> [qualidade-dados-legado](../extracao-vortice/qualidade-dados-legado.md) (Vórtice ao vivo,
> seção 8).
>
> ⚠️ **Fronteira com outras frentes.** Este documento não declara `CREATE TABLE` nem
> `CHECK CONSTRAINT` — isso é `14-PADRAO-DE-BANCO.md`, de outra frente, e os testes de
> `tests/Tracbel.Crm.Arquitetura.Testes/Banco/`, que também não pertencem a esta frente. A
> seção 9 lista, em tabela, exatamente o que este documento pede para o doc 14 incorporar.
> Nenhum arquivo dessas duas áreas foi tocado para produzir este documento.

---

## 1. Princípios

Quatro princípios. Toda regra da seção 2 em diante é a aplicação de um deles — quando uma
regra parecer arbitrária, volte aqui.

### 1.1 Normalizar na entrada, não na leitura

Um dado só é validado uma vez: quando entra. Nenhuma tela, relatório ou integração deveria
precisar "limpar" o dado de novo antes de usá-lo — se precisa, é porque algo entrou sujo.

`[V]` O Vórtice faz o oposto sistematicamente: **zero `CHECK CONSTRAINT` em 767 tabelas**
(documento 00-RELATORIO-EXTRACAO, seção 1) empurra toda validação para o momento da leitura
— cada relatório, cada tela, reinventa sua própria lista de sinônimos para `IV_Processo.Status`
(mais de 20 valores livres, `FINALIZADO` convivendo com `FINALIZADA`) porque nunca houve um
portão na entrada. O custo não é uma vez: é **toda consulta, para sempre**.

### 1.2 Um só ponto de normalização por tipo

Um tipo de dado — telefone, e-mail, documento — tem **um** lugar que sabe validá-lo e
normalizá-lo: o tipo de valor em `Tracbel.Crm.Dominio.Comum`. Nenhuma outra camada
reimplementa a regra. A entidade de domínio (`Lead`, e as que vierem) recebe o tipo já
validado; a tela usa o mesmo tipo para formatar; a migração usa o mesmo tipo para aceitar
ou rejeitar a linha do Vórtice.

`[V]` O Vórtice tem **três modelos concorrentes de e-mail** (`GE_Pessoa.Email`, `GE_Email`,
`GE_PessoaEmail`) e **três de telefone** (`FoneDDD1..3`+`FoneNro1..3`, `GE_PessoaFone`, e
mais um em `GE_Contato`) dentro do mesmo banco — documento 01, achado 9.5. Nenhum deles
normaliza, e cada um pode dizer uma coisa diferente sobre a mesma pessoa. Não é falta de
validação: é validação espalhada, o que é pior, porque cada cópia diverge com o tempo.

### 1.3 Rejeitar é melhor que corrigir em silêncio

Quando um dado não serve, o sistema tem duas opções: adivinhar um valor "razoável" e seguir
em frente, ou recusar a linha e dizer por quê. Aqui, sempre a segunda. Um valor adivinhado
em silêncio parece mais gentil no curto prazo e é sempre mais caro depois — porque ninguém
sabe que ele foi adivinhado.

`[V]` `IV_Historico.ResultadoCmpl` é `varchar(150)` mas a aplicação grava truncado em 20
caracteres, sem erro e sem aviso (documento 01, achado 9.2). Medido ao vivo nesta rodada
([qualidade-dados-legado](../extracao-vortice/qualidade-dados-legado.md), seção 4): **44.962
linhas de `IV_Historico`** e **4.566 de `IV_Agenda`** têm exatamente 20 caracteres — a marca
d'água do corte. Ninguém decidiu truncar; o dado só foi encolhendo, um `INSERT` de cada vez,
e nenhum log registra que aquilo aconteceu. É por isso que `TextoNormalizado.TentarCriar`
(seção 2.1) **rejeita** texto acima do tamanho máximo — não corta.

Todo tipo de valor deste projeto segue o mesmo padrão de duas portas, já estabelecido em
`CpfCnpj`, `Email` e `Telefone` antes deste documento:

```csharp
public static T Criar(string entrada)                       // lança RegraDeNegocioViolada
public static bool TentarCriar(string? entrada, out T valor) // não lança — para lote
```

`Criar` é para o formulário interativo, onde faz sentido travar o usuário até ele corrigir.
`TentarCriar` é para importação em massa e migração, onde a linha ruim precisa ser
**registrada e o lote precisa continuar** — nunca abortar tudo por uma linha.

`[V]` O sincronismo do Vórtico Mobile aborta a carga **inteira** numa única linha órfã:
5.302 vínculos quebrados (6,9% dos vínculos de documento) travam o app de todo mundo cujo
escopo os inclua (documento 01, achado 9.11). `TentarCriar` existe para que isso nunca se
repita: a linha ruim vira uma `CampoRejeitado` (seção 8) e a próxima linha do lote é
processada normalmente.

### 1.4 Toda correção automática fica registrada

Quando o saneamento MUDA um valor — remove máscara, completa o nono dígito, baixa a caixa
de um e-mail — essa mudança é um fato, e fatos se registram. Ninguém deveria precisar
adivinhar, três meses depois, se um telefone "sempre foi assim" ou se o sistema o alterou.

`[V]` O Vórtice tem o problema oposto e, mesmo assim, não resolve este: **44,8 milhões de
85,5 milhões de linhas do banco (52,4%) são log** (documento 01, seção 10), majoritariamente
ruído — 96,8% de `GE_LOG_PROCESSO` são re-carimbos idênticos de um job de cobrança. Muito
log de coisa que não importa, e nenhum registro da única coisa que importaria: quando e por
que um valor foi alterado por uma rotina automática. Volume de log não é a mesma coisa que
auditoria útil.

A seção 8 define o formato do registro (`RegistroSaneamento`): fonte, entidade, quando, o
que foi normalizado e o que foi rejeitado — pequeno, específico, e feito para ser lido, não
para acumular.

---

## 2. Catálogo de saneamento por tipo de dado

Cada bloco: o que é **aceito**, o que é **normalizado**, o que é **rejeitado**, o defeito do
Vórtice evitado, e o tipo de valor correspondente em `Tracbel.Crm.Dominio.Comum` (todos
`readonly record struct`, com `Criar`/`TentarCriar`, comparação por valor by design da
`record`, e mensagem de erro que diz o que corrigir — o mesmo molde de `CpfCnpj`).

### 2.1 Nome e razão social

| | |
|---|---|
| **Aceita** | Texto humano: letras (inclusive acentuadas), espaço, apóstrofo, hífen, ponto de abreviação. |
| **Normaliza** | Espaço nas pontas removido; espaço duplo colapsado em um; caractere invisível (largura zero, NBSP, BOM, marca de direção) removido. **Caixa não é forçada** — "McDonald", "O'Brien" e "de Souza" têm exceção legítima de capitalização que um `ToTitleCase()` erraria mais do que acertaria. |
| **Rejeita** | Vazio depois de normalizado; maior que o tamanho da coluna (`nvarchar(200)` para razão social, `nvarchar(120)` para contato — documento 04); só dígito, sem nenhuma letra (não é nome). |

`[V]` Medido ao vivo: **332 linhas de `GE_Pessoa.NomeRazao`** e **349.315 linhas de
`IV_Historico.Detalhe`** (14,3% de toda a tabela) têm espaço duplo — o maior achado absoluto
da medição contra o legado (seção 8). Nenhuma normalização de texto existe em nenhum ponto
da esteira de gravação do Vórtice.

**Tipo de valor:** `TextoNormalizado` (`src/Tracbel.Crm.Dominio/Comum/TextoNormalizado.cs`).

### 2.2 CPF e CNPJ

| | |
|---|---|
| **Aceita** | Com ou sem máscara (`529.982.247-25` ou `52998224725`). |
| **Normaliza** | Extrai só os dígitos; determina CPF (11) ou CNPJ (14) pelo tamanho. |
| **Rejeita** | Dígito verificador que não confere; sequência repetida (`00000000000`, `11111111111`...); tamanho diferente de 11 ou 14. |

`[V]` **116 CPFs repetidos entre clientes distintos** por falta de validação na entrada
(documento 01, achado 9.4). Mas a medição ao vivo desta rodada encontrou algo pior e **não
documentado até agora**: `GE_Pessoa.NroCGCCPF` é `decimal(13,0)` — um tipo numérico não
preserva zero à esquerda. Resultado medido
([qualidade-dados-legado](../extracao-vortice/qualidade-dados-legado.md), seção 1):

> 🔴 **51.523 de 75.933 pessoas jurídicas (67,9%) têm o CNPJ armazenado sem os zeros à
> esquerda que a raiz+filial de 12 dígitos exige.** Mais 6.611 de 41.719 pessoas físicas
> (15,8%) no mesmo problema com o CPF. Não existe, em lugar nenhum do banco, a contagem de
> quantos zeros faltam para reconstituir o documento original.

Esta é a prova mais forte deste documento de que "não fazer o que o Vórtice fez" precisa
significar **nunca gravar documento em coluna numérica** — não só telefone (o exemplo já
citado no documento 04, seção 1.2). Ver seção 9.

**Tipo de valor:** `CpfCnpj` (já existia; testes estendidos, sem alterar a regra).

### 2.3 Inscrição estadual

| | |
|---|---|
| **Aceita** | Dígitos com máscara variável por UF (de 8 a 14 posições, conforme a unidade), ou o literal `ISENTO`. Sempre acompanhada da UF de emissão — IE sem UF não identifica nada. |
| **Normaliza** | Extrai dígitos quando não é `ISENTO`; preserva a UF associada. |
| **Rejeita** | Tamanho fora da faixa conhecida para a UF informada; IE numérica sem a UF de emissão. |

**Decisão consciente de escopo:** o dígito verificador de inscrição estadual tem **27
algoritmos diferentes**, um por unidade federativa, sem padrão nacional único. Implementar
os 27 nesta fase é desproporcional ao valor: nenhum achado do Vórtice (`docs/extracao-vortice/dominios/`)
mede IE como fonte de defeito, porque a coluna nunca fez parte da amostra de domínios
analisada. A regra acima valida **formato e presença da UF** — não o dígito verificador.
Isto é dívida técnica **declarada**, não uma lacuna silenciosa: se um caso de uso futuro
precisar do dígito verificador completo, a extensão é aditiva (mais uma tabela de algoritmo
por UF), sem quebrar quem já usa o formato validado hoje.

**Tipo de valor:** nenhum dedicado — validação de formato vive como função utilitária no
pipeline de saneamento (seção 8), não como `record struct` próprio. Não há uso do protótipo
que justifique um tipo forte agora (regra de escopo da seção "O que decidimos não fazer",
ao final desta seção).

### 2.4 Telefone

| | |
|---|---|
| **Aceita** | Com ou sem máscara, com ou sem DDI `+55`, celular com ou sem o nono dígito. |
| **Normaliza** | Só dígitos; remove DDI `55` nacional; completa o nono dígito quando reconhece um celular anterior à migração de 2016 (número local de 8 dígitos começando em 6-9). |
| **Rejeita** | DDI estrangeiro explícito (nunca adivinhado — ver abaixo); tamanho fora de 10/11 dígitos; DDD fora de 11-99; celular sem "9" na posição certa depois de tentar completar. |

`[V]` `GE_Pessoa.FoneNro1` é `decimal(12,0)` — telefone com DDI ou zero à esquerda estoura o
tipo e a API do Vórtice devolve `{"Message":"An error has occurred."}` sem dizer o motivo
(documento 04, seção 1.2). Medido ao vivo: **168 linhas** de `FoneNro1` fora da faixa
plausível de 8-9 dígitos locais.

Duas extensões feitas nesta rodada sobre o tipo `Telefone` já existente:

1. **Celular antigo sem o nono dígito** — um número local de 8 dígitos começando em 6-9 é
   celular (fixo brasileiro só começa em 2-5); o tipo completa o "9" automaticamente e
   registra a correção (seção 8). Sem isto, o mesmo assinante virava dois cadastros
   diferentes conforme a máscara com que o número chegasse.
2. **Número internacional rejeitado, não adivinhado** — sem DDI explícito reconhecido como
   `+55`, um número estrangeiro pode coincidir em quantidade de dígitos com um celular
   brasileiro válido (`+1 397 123 4567` → `13971234567`, 11 dígitos, DDD "13" e nono dígito
   "9" por pura coincidência) e ser aceito silenciosamente como nacional. A extensão rejeita
   qualquer `+` seguido de DDI que não seja `55`, antes mesmo de contar dígitos.

**Tipo de valor:** `Telefone` (estendido — `src/Tracbel.Crm.Dominio/Comum/Telefone.cs`).

### 2.5 E-mail

| | |
|---|---|
| **Aceita** | `local@dominio.tld`, em qualquer caixa. |
| **Normaliza** | Minúsculo; espaço nas pontas removido. |
| **Rejeita** | Sem `@`; domínio sem ponto; espaço interno; mais de 200 caracteres. |

`[V]` Três modelos concorrentes de e-mail no mesmo banco (seção 1.2). Medido ao vivo: **528
linhas de `GE_Pessoa.Email`** com formato inválido (sem `@`/domínio, ou com espaço) — um
e-mail assim não é só "mal formatado", é **funcionalmente morto**: nunca entrega mensagem
nenhuma.

**Tipo de valor:** `Email` (já existia; ganhou teste explícito de e-mail inteiro em caixa
alta — a normalização já cobria o caso, faltava só o teste que prova).

### 2.6 CEP e endereço

| | |
|---|---|
| **Aceita** (CEP) | Com ou sem hífen (`78455-000` ou `78455000`). |
| **Normaliza** (CEP) | 8 dígitos, sem hífen internamente; formata só na exibição. |
| **Rejeita** (CEP) | Tamanho diferente de 8; sequência de um único dígito repetido (`00000000` — erro de preenchimento, nenhuma faixa dos Correios usa isso). |
| **Endereço** | Logradouro, bairro e complemento são `TextoNormalizado`. **Número é texto, não numérico** — `S/N` é um número de endereço válido (visto no próprio protótipo: `prototipo/dados-seed/cliente-84391.json`, `endereco_principal.numero = "S/N"`). CEP presente com logradouro vazio é rejeitado como inconsistente. |

`[V]` Endereço é o campo mais livre do cadastro do Vórtice e, por isso, o mais sujeito a
erro nunca detectado — `GE_PessoaEnd` não entrou na amostra de 40 colunas de domínio
(`docs/extracao-vortice/dominios/00-INDICE.md`) porque não é coluna de código, é texto
solto sem nenhuma validação de formato.

**Tipo de valor:** `Cep` (novo — `src/Tracbel.Crm.Dominio/Comum/Cep.cs`) + `TextoNormalizado`
para os componentes textuais. Endereço completo não ganha um tipo próprio: é composição de
tipos que já existem, não uma entidade nova.

### 2.7 Cidade e UF

| | |
|---|---|
| **Aceita** (UF) | 2 letras, qualquer caixa. |
| **Normaliza** (UF) | Maiúsculo. |
| **Rejeita** (UF) | Fora das 27 unidades federativas (26 estados + DF). |
| **Cidade** | **Não é texto livre no banco novo** — é FK para um catálogo de município (base IBGE), com o texto livre existindo só como entrada temporária, antes do casamento contra o catálogo (por nome + UF, com tolerância a acento/caixa). Depois de casada, a cidade é sempre um ID; texto livre nunca é gravado como cidade definitiva. |

`[V]` O próprio protótipo ilustra por que isto importa: a mesma cidade aparece como
`"cidade": "Sorriso"` + `"uf": "MT"` em campos separados num arquivo
(`prototipo/dados-seed/carteira-cen.json`) e embutida na mesma string em outro —
`"cidade": "Lucas do Rio Verde/MT"` (`prototipo/dados-seed/oportunidade-1517613.json`).
Sem uma FK de catálogo por trás, cada tela que precisar "a cidade" reinventa o parse dessa
string — exatamente o problema que fez `IV_Processo.Fase` divergir entre `Afericao` e
`Aferição` (achado real, ver seção 3): dois formatos de representar a mesma informação,
nenhum deles obrigatório.

**Tipo de valor:** nenhum dedicado para UF isoladamente — 2 caracteres fixos não justificam
um `record struct`; a validação de UF vive no pipeline de saneamento e, no banco, é FK ou
`CHECK IN (...)` (ver a distinção da seção 9 entre lista nacional imutável e catálogo de
negócio). Cidade é sempre FK — nunca chega a ser um "tipo de valor" de texto.

### 2.8 Data e hora, em UTC, com faixa plausível

| | |
|---|---|
| **Aceita** | Qualquer formato interpretável como instante no tempo. |
| **Normaliza** | Converte para UTC. `Local` converte de verdade; `Unspecified` é tratado como **já sendo** UTC — quem tem horário local converte ANTES de chamar o tipo, que não adivinha fuso. |
| **Rejeita** | Ano fora de `[1900, ano atual + 30]` — o piso comum a todo o sistema. Campos com regra mais estreita (ex.: vencimento de título financeiro, que não existiria antes da empresa) aplicam faixa adicional no catálogo específico daquele campo, documentada junto da tabela que o usa. |

`[V]` Sem `CHECK` de faixa em nenhuma coluna de data, o Vórtice tem vencimento gravado em
**08/05/5024**, e a maior data já encontrada no banco inteiro é **20/08/5173**
(documento 01, achado 3.8; documento 10). Medido ao vivo nesta rodada — a extensão do
achado, tabela por tabela:

| Tabela.Coluna | Fora de `[1990, ano+30]` |
|---|---:|
| `IV_Processo.DtaPrevConclusao` | 0 |
| `IV_Agenda.DtaAgenda` | 7 |
| `IV_Historico.DtaRealizacao` | 1 |
| `EXT_Titulo.DtaVencto` | 6 |

A causa raiz já foi diagnosticada pela pesquisa 02: o Protheus usa a sentinela
`1900-01-01` para "sem data", tratada em só 2 das 5 colunas de data de título relevantes; o
que escapa dessa coalescência incompleta é o que aparece na tabela acima. **Duas
preocupações distintas, nunca confundidas:**

1. **Sentinela de origem** (ex.: `1900-01-01` do Protheus) é convertida em `NULL`
   explicitamente no adaptador de integração (`Tracbel.Crm.Integracao`), **antes** de o
   valor alcançar `DataHoraUtc`. O tipo de valor não reconhece sentinela — ele só sabe
   validar faixa.
2. **Faixa implausível** (`5173`, `2223`, `5024`) é o que `DataHoraUtc.TentarCriar` rejeita.

**Tipo de valor:** `DataHoraUtc` (novo — `src/Tracbel.Crm.Dominio/Comum/DataHoraUtc.cs`).

### 2.9 Dinheiro

| | |
|---|---|
| **Aceita** | `decimal`, não negativo. |
| **Normaliza** | Nada — dinheiro não tem "correção automática" segura. Ou já vem com no máximo duas casas, ou é rejeitado. |
| **Rejeita** | Mais de duas casas decimais; valor negativo (desconto e estorno são modelados como grandeza sempre positiva + uma operação separada, nunca como sinal negativo do mesmo campo — isto evita que "negativo" precise de dois significados diferentes na mesma coluna). |

`[V]` O documento 04 (seção 1.2) já proíbe `float`/`money` no **banco** pelo arredondamento
silencioso; este tipo estende o mesmo cuidado para a **entrada**. Medido ao vivo, o contraste
é instrutivo: `EXT_Titulo.VlrOriginal` é `numeric(14,2)` — o tipo certo — e o resultado é
**zero linhas negativas** e zero problema de escala. Comparado com `NroCGCCPF decimal(13,0)`
(seção 2.2) e `FoneNro1 decimal(12,0)` (seção 2.4) na mesma tabela `GE_Pessoa`, fica claro:
**o defeito não é "o Vórtice não valida nada"; é "o Vórtice não valida nada onde não
escolheu por acidente o tipo certo"**. A medição da qualidade dos 45 JSONs do protótipo
(seção 7) também encontrou um valor monetário como prosa livre —
`"faturamento_declarado": "R$ 80 a 120 milhões / ano"` — exatamente o tipo de dado que este
tipo de valor existe para nunca deixar entrar como número de negócio.

**Tipo de valor:** `Dinheiro` (novo — `src/Tracbel.Crm.Dominio/Comum/Dinheiro.cs`).

### 2.10 Documento fiscal (nota fiscal, pedido do ERP)

| | |
|---|---|
| **Aceita** | Número de nota como exibido pelo ERP (ex.: `NF-e 001.847.559`) ou código de pedido (ex.: `PV-2023-04891`). |
| **Normaliza** | `TextoNormalizado` sobre a representação exibida. |
| **Rejeita** | Formato reconhecidamente truncado ou incompleto para o padrão do documento informado. |

O defeito real aqui não é de **formato** do número — é de **vínculo ausente**. `[V]`
"CRM e ERP são duas ilhas": `EXT_*` (ERP) e `IV_*` (CRM/BPM) se ligam **apenas por
`SeqPessoa`**; não há `IdNFS`, `IdOS` nem `idTitulo` em `IV_Historico` ou `IV_ProcDado`, e
**99,1% das notas fiscais não têm `IdVeic`** (documento 01, seção 4). Não adianta o número
da nota estar bem formatado se nada no banco consegue responder "esta oportunidade virou
qual nota fiscal?" por consulta — a ponte real hoje é um formulário digitado à mão
(`IV_Q_ACOMP_VENDA_FINANC`).

**Decisão de fronteira com o ERP** (Protheus/TOTVS — confirmado como o ERP de integração
desta fase): o saneamento de documento fiscal na origem `Tracbel.Crm.Integracao` tem **dono
claro**. O que vem do Protheus é saneado uma única vez, na entrada, pela camada de
integração; o CRM lê o resultado já saneado e **não regrava por cima do que pertence ao
ERP** — o Protheus continua sendo o sistema de registro para o dado fiscal, o CRM só o
referencia. O desenho da integração em si (watermark, fila, retentativa —
`integracao.ChaveExterna`, `integracao.PontoDeSincronismo`, documento 04 seção 8) é de outra
frente; o que este documento fixa é o princípio de propriedade do dado, não o mecanismo.

**Tipo de valor:** `TextoNormalizado` para o número exibido; o vínculo estrutural
(FK de `processo.Processo` para o título/nota) é modelagem de dados, não tipo de valor.

### 2.11 Placa e chassi

| | |
|---|---|
| **Aceita** (chassi) | 17 caracteres (padrão VIN), com ou sem espaço/caixa mista. |
| **Normaliza** (chassi) | Maiúsculo, sem espaço. |
| **Rejeita** (chassi) | Tamanho diferente de 17; contém `I`, `O` ou `Q` (o padrão VIN os proíbe para não confundir com `1`/`0` na leitura manual da plaqueta). |
| **Placa** | Mesma disciplina do chassi (maiúsculo, valida os dois formatos oficiais — antigo `LLLNNNN` e Mercosul `LLLNLNN`, rejeita o resto). Não ganhou tipo de valor dedicado nesta rodada porque **o protótipo não usa placa em nenhum dos 45 JSONs** — quando um caso de uso a exigir, o padrão a seguir é o de `Chassi`. |

`[V]` Medido ao vivo em `EXT_Veic`: **3.262 de 8.020 chassis (40,7%) não têm 17 caracteres**
— quatro em cada dez. Sem o tipo `Chassi`, essa coluna nunca serviu como chave confiável de
deduplicação de equipamento (seção 4). Em contraste, **placa está quase sempre bem formada**
(2 violações em toda a tabela) — outro caso, como o dinheiro da seção 2.9, de "onde o
Vórtice acertou o formato, o dado reflete isso".

**Tipo de valor:** `Chassi` (novo — `src/Tracbel.Crm.Dominio/Comum/Chassi.cs`).

### 2.12 Texto livre (observação, descrição)

| | |
|---|---|
| **Aceita** | Qualquer texto humano — mas só nos poucos campos que são texto livre **de propósito** (ver a regra da seção 3: catálogo é seleção, texto livre é exceção justificada). |
| **Normaliza** | `TextoNormalizado`: espaço nas pontas, espaço duplo, caractere invisível. |
| **Rejeita** | Vazio depois de normalizado, quando o campo é obrigatório; **maior que o tamanho máximo da coluna — rejeitado, nunca truncado** (ver princípio 1.3). |

`[V]` O par de números mais contundente desta rodada, ambos na mesma tabela: **349.315
linhas de `IV_Historico.Detalhe` com espaço duplo** (14,3% da tabela) e **44.962 linhas de
`IV_Historico.ResultadoCmpl` truncadas em exatamente 20 caracteres**. A mesma tabela mostra
os dois lados do princípio 1.3: um defeito que **corrige em silêncio** (o truncamento) e um
que **nem tenta** normalizar (o espaço duplo). Nenhum dos dois teria sobrevivido a um `TextoNormalizado.TentarCriar` na entrada.

**Tipo de valor:** `TextoNormalizado` (novo — `src/Tracbel.Crm.Dominio/Comum/TextoNormalizado.cs`).

### 2.13 Coordenada geográfica

| | |
|---|---|
| **Aceita** | Par latitude/longitude numérico. |
| **Normaliza** | Nada — coordenada errada não tem correção sensata; se está fora de faixa, é bruta e deve ser rejeitada, não adivinhada. |
| **Rejeita** | Fora da faixa matemática (`lat` fora de -90..90, `lng` fora de -180..180) **ou** fora da caixa de atuação da Tracbel (hoje, só Brasil: `lat` -34..6, `lng` -75..-32 — constante única, revista de propósito se a área de atuação crescer). |

`[V]` Geolocalização do atendimento em campo (`IV_Historico.Latitude`/`Longitude`) é um dos
**acertos** do Vórtice a preservar (documento 01, seção 12) — mas sem validação de faixa.
Medido ao vivo: **12 linhas** com coordenada em (0,0) ou fora do Brasil, contra 2.436.127
linhas na tabela — baixo em proporção, mas zero seria o resultado de ter o tipo desde o
início.

**Tipo de valor:** `Coordenada` (novo — `src/Tracbel.Crm.Dominio/Comum/Coordenada.cs`).

### O que decidimos não fazer, por enquanto

Mesmo espírito da seção 13 do documento 05: nomear o que foi descartado, e por quê, evita
que a pergunta volte sem contexto.

| Descartado | Por quê |
|---|---|
| Dígito verificador de inscrição estadual (27 algoritmos por UF) | Nenhum achado do Vórtice mede IE como fonte de defeito; custo de implementar 27 algoritmos não se paga nesta fase (seção 2.3) |
| Telefone internacional (fora do Brasil) validado de verdade | O tipo `Telefone` é declaradamente brasileiro; suportar outro país exige um plano de numeração próprio por país — hoje, é rejeitado, não mal-aceito (seção 2.4) |
| Tipo de valor dedicado para Placa | Zero uso no protótipo hoje; o padrão de `Chassi` já está pronto para quando precisar (seção 2.11) |
| Tipo de valor dedicado para UF/Cidade | 2 caracteres fixos (UF) ou FK pura (Cidade) não justificam um `record struct`; a validação vive no pipeline e no banco (seção 2.7) |
| Correção automática de dinheiro com casas extras (arredondar) | Arredondar em silêncio é o mesmo erro do Vórtice (achado 3.8) — rejeita, não adivinha (seção 2.9) |

---

## 3. Domínios e catálogos

**Regra geral: nenhuma coluna de domínio aceita texto livre.** Toda coluna que representa
"um de um conjunto conhecido de valores" — situação, motivo, categoria, tipo — é FK para uma
tabela de referência (`metadado.*` para extensão sem release, ou uma tabela de catálogo
própria do módulo, ex. `processo.MotivoDePerda`). Nunca uma coluna `varchar` onde a
aplicação promete, sem garantia nenhuma do banco, gravar só os valores "certos".

`[V]` O banco `CRM` não tem uma única `CHECK CONSTRAINT` (documento 00-RELATORIO-EXTRACAO,
seção 1) nem tabela de domínio para a maioria das 40 colunas de código medidas
(`docs/extracao-vortice/dominios/00-INDICE.md`). O resultado, com números reais:

- `IV_Processo.Status`: mais de 20 valores de texto livre com duplicata semântica —
  `FINALIZADO` (18.416) ao lado de `FINALIZADA` (11.563), `CANCELADO` (41.390) ao lado de
  `CANCELADA` (2.412), e **437.694 linhas em branco** na extração original.
- `GE_Pessoa.Sexo` guarda `J` de pessoa jurídica — a mesma coluna carrega dois conceitos.
- `EXT_Titulo.Status` mistura código de uma letra (`B`, `A`) com texto por extenso (`Pago`,
  `Compensado`, `A Depositar`) — duas integrações diferentes gravando na mesma coluna em
  épocas diferentes. Medido ao vivo: **100% das 577.925 linhas de `EXT_Titulo`** caem num
  desses dois vocabulários — não há uma terceira opção "correta" para migrar.

### 3.1 Campo com catálogo não aceita digitação livre

Esta é a regra mais repetida deste documento porque é a mais violada no legado, e porque
foi reafirmada explicitamente pelo Ricardo: **quando um formulário do CEN dá seguimento a um
processo, a maioria dos campos puxa de tabela — não é texto digitado.**

**O que é sempre seleção, nunca texto**, porque todos têm catálogo: motivo de perda, tipo de
tarefa, resultado, categoria de interação, marca e modelo de equipamento, cultura agrícola,
praça/regional, concorrente, condição de pagamento, linha de negócio, classe do cliente
(A/B/C/D), situação (do cliente, do processo, do título). Cada um destes é uma FK na tabela;
na tela, é um componente de seleção (`<select>`, autocomplete restrito ao catálogo,
picklist) — nunca um `<input type="text">` solto.

**O que continua sendo texto livre — a exceção, e ela precisa de justificativa**:
observação e descrição do que foi conversado com o cliente. São os únicos campos onde o
conteúdo é, por natureza, imprevisível. Mesmo esses passam pela normalização da seção 2.12
(`TextoNormalizado`) — "ser texto livre" não é o mesmo que "ser texto sem regra nenhuma".

**A evidência do porquê**, com o número mais direto deste documento inteiro: sem essa regra,
`IV_Processo.Status` — um campo que é, por natureza, um catálogo fechado de estados possíveis
de um processo — virou texto livre, e o resultado medido é `FINALIZADO` convivendo com
`FINALIZADA`, `CANCELADO` com `CANCELADA`, e (medido ao vivo nesta rodada,
[qualidade-dados-legado](../extracao-vortice/qualidade-dados-legado.md), seção 2) **480.397
processos com status em branco** — mais até que os 437.694 da extração original, porque o
sistema continua vivo e continua gerando a mesma sujeira todo dia. A medição ao vivo achou
ainda uma quarta grafia não citada antes: a coluna devolveu **`Finalizado` em caixa mista**,
não `FINALIZADO` — só não apareceu como valor "diferente" na consulta porque a collation do
banco é *case-insensitive*. São no mínimo **quatro grafias para dois conceitos**, e nenhuma
delas seria possível se `Status` fosse FK para `processo.SituacaoProcesso` desde o primeiro
dia.

**A regra vale nos três momentos, sempre juntos — nenhum sozinho basta:**

1. **Na tela**: o campo é um componente de seleção. Não existe caminho de UI para digitar um
   valor que não esteja no catálogo — a mesma disciplina de "não existe caminho de código
   para construir um `Telefone` inválido" (documento 03, seção 2.3), aplicada à interface.
2. **Na API**: o endpoint que recebe o valor **rejeita** (400/422) qualquer código que não
   exista no catálogo correspondente — mesmo que a tela tenha sido contornada (chamada
   direta, integração, script). A tela sozinha nunca é a defesa; documento 05, seção 8,
   já estabelece isto para segurança, e aqui é a mesma lógica para qualidade de dado.
3. **No banco**: FK para a tabela de referência — **nunca `CHECK IN (lista fixa)`** para
   catálogo de negócio (contraste com UF na seção 9, que é lista nacional imutável e por
   isso pode ser `CHECK`). A razão de ser FK e não `CHECK`: o catálogo **precisa poder
   crescer sem migration** — um motivo de perda novo, uma linha de negócio nova, não podem
   depender de alterar a definição da tabela.

**Como um valor novo entra no catálogo:**

1. Alguém do negócio propõe o valor novo (o mesmo fluxo do glossário, documento 15, seção
   6 — "alguém do negócio usa a palavra numa reunião; ela é a candidata").
2. O dono do catálogo daquele domínio aprova — para catálogo comercial (motivo de perda,
   categoria de interação, classe de cliente), é o Gestor Regional ou Admin Comercial
   (`seguranca.ConjuntoDePermissao` = `ADM_VENDAS`, documento 05, seção 4); para catálogo de
   frota (marca, modelo), é quem administra o cadastro de produto.
3. A linha nova entra com `CriadoEm`/`CriadoPorId` (o padrão de toda tabela, documento 04,
   seção 1.3) — a data de vigência **é** a data de criação; não existe "valor válido a
   partir de uma data futura" nesta fase.
4. Só depois de existir no catálogo o valor pode ser referenciado por um registro
   transacional.

**Por que um valor usado no histórico nunca se apaga:** apagar uma linha de catálogo que já
foi referenciada quebraria toda FK apontando para ela — exatamente o modelo de soft delete
que `EntidadeBase.Excluir` já aplica a toda entidade transacional (documento 04, seção 1.3).
Um valor de catálogo que deixou de fazer sentido **se aposenta**, não se apaga: ganha
`EstaAtivo = 0` (ou, para catálogos com regra de vigência mais rica, `AposentadoEm` +
`AposentadoPorId`). A tela para de oferecer o valor aposentado em telas de criação; registros
antigos que já o usam continuam lendo e exibindo normalmente. `[DYN]` é o mesmo raciocínio
do Dataverse para opção de picklist obsoleta: desativa-se, nunca se remove.

### 3.2 O que os domínios reais do Vórtice ensinam a não fazer

Cinco padrões, medidos e catalogados em `docs/extracao-vortice/dominios/00-INDICE.md`
(379 colunas, 1.115 valores distintos) — cada um é o argumento vivo para a regra 3.1:

1. **Ausência total de constraint** produz vocabulário duplicado em produção — é o próprio
   `IV_Processo.Status`/`Fase` já citado.
2. **Nulo, vazio e zero significam coisas diferentes, sem padronização** —
   `GE_Pessoa.IndContribICMS` mistura `N`, nulo e `0` para a mesma ideia. Regra deste
   documento: **o saneamento decide, uma vez, por coluna, qual coalescência aplicar** — nunca
   à mão, coluna a coluna, em cada tela.
3. **Flags mortas em massa**: 112 das 379 colunas de domínio (29,6%) têm **um único valor**
   em toda a tabela — `IV_Acao.ExigeProduto` é `N` em 100% das 980 ações. São recursos que a
   Tracbel nunca ligou. Regra: **não migrar flag morta como coluna** — se 100% dos dados têm
   o mesmo valor há anos, ela não carrega informação, carrega peso.
4. **Código de uma letra sem legenda** — `IV_Operador` tem colunas de permissão no alfabeto
   `G`/`E`/`D`/`N` sem documentação do que cada letra significa. Regra: **todo domínio
   migrado tem a legenda escrita no catálogo de destino**, nunca só o código.
5. **A mesma coluna carrega dois conceitos** — `GE_Pessoa.Sexo` guardando `J` de pessoa
   jurídica (já citado acima). Regra: **uma coluna, um conceito**; se dois conceitos
   convivem hoje, a migração os separa em duas colunas — não os carrega juntos para o banco
   novo.

---

## 4. Deduplicação

**Chave natural por entidade:**

| Entidade | Chave natural | Tipo de valor |
|---|---|---|
| `comercial.Cliente` | CNPJ ou CPF | `CpfCnpj` |
| `frota.Equipamento` | Chassi | `Chassi` |
| `comercial.Contato` | Nenhuma chave natural forte sozinha — nome + telefone/e-mail em conjunto, com confirmação humana antes de mesclar (ver abaixo) |

**Com duplicata já existente** (o cenário mais caro, porque os dois registros já
acumularam histórico próprio):

1. **Nunca apagar um dos dois.** Um dos registros vira o **sobrevivente** — critério:
   maior número de campos preenchidos, e em empate, o mais antigo (`CriadoEm` menor).
2. O registro perdedor recebe `ExcluidoEm` (soft delete padrão, documento 04 seção 1.3) e
   uma referência explícita para o sobrevivente (`MescladoEmId`), preservando a
   rastreabilidade — ninguém pode perguntar "cadê o cliente X" e receber "não existe".
3. Todo filho do perdedor (processo, interação, equipamento, título) é **reatribuído** ao
   sobrevivente antes do soft delete — a mesma disciplina de reatribuição que a hierarquia
   comercial já exige (documento 05, seção 5: "o sistema reatribui", nunca deixa órfão).
4. A fusão é uma ação **auditada** (`auditoria.EventoDeAcesso`, tipo `FusaoDeRegistro`), com
   quem decidiu e quando — fusão automática sem revisão humana não é permitida nesta fase:
   o `CpfCnpj`/`Chassi` batendo é forte evidência, não certeza absoluta de que são o mesmo
   cliente/equipamento.

`[V]` O Vórtice tem uma **fila de deduplicação com 124 mil pares abandonados desde 2018**
(documento 01, achado 9.8) — o mecanismo de detectar existe (ou existiu), o de **decidir e
executar** nunca funcionou. A regra 3 acima (reatribuir antes de soft-deletar) é
precisamente o que uma fila que só aponta pares candidatos, sem forçar a reatribuição, nunca
resolve sozinha.

**Para impedir duplicata nova**, duas camadas, nunca uma sozinha (mesmo princípio de
defesa em profundidade do documento 05):

1. **Na aplicação, primeiro**: antes de criar um cliente/equipamento, buscar por
   `CpfCnpj`/`Chassi` já normalizado. Se existir, a tela oferece "usar o existente" em vez
   de criar — mais barato prevenir do que fundir depois.
2. **No banco, como rede de segurança**: índice único filtrado sobre a chave natural, por
   empresa e ignorando excluídos — já modelado no documento 04
   (`UX_Conta_CpfCnpj ON crm.Conta (EmpresaId, CpfCnpj) WHERE ... AND ExcluidoEm IS NULL`,
   renomeado por `comercial.Cliente` conforme o glossário). Se a camada 1 falhar (condição
   de corrida, bug, importação em lote fora do fluxo normal), o `INSERT` duplicado
   **quebra no banco**, não silenciosamente na tela seguinte.

A medição desta rodada confirma o valor da chave natural na prática, em dois lugares
diferentes:

- **Nos 45 JSONs do protótipo** (seção 7): o mesmo CNPJ (`11.234.567/0001-89`) identifica o
  cliente 84391 em `cliente-84391.json` **e** o cliente 84430 em `clientes-extra.json` — uma
  duplicata real de chave natural, dentro do próprio material de referência da interface.
- **No Vórtice ao vivo**: a fila de 124 mil pares abandonados citada acima é o que acontece
  quando a chave natural nunca é reforçada no ponto de entrada.

Duplicação **de tabela inteira** (backups esquecidos, cópias `_ITA`/`_BKPJUN`) é um problema
relacionado mas distinto — documentado em `docs/extracao-vortice/tabelas-duplicadas.txt`
(34 grupos, 90 tabelas com o mesmo conjunto de colunas) e não repetido aqui: aquele arquivo
já cataloga o achado; esta seção trata de duplicata de **linha**, dentro de uma única tabela
viva.

---

## 5. Sanitização de segurança

**Texto que vira HTML**: a defesa é no **render**, não na entrada. Todo texto livre
(observação, nome) é *HTML-encoded* no momento de exibir na tela — nunca "limpo" de tags na
entrada, porque isso destrói dado legítimo sem necessidade (um cliente que se chama
literalmente "<Fazenda>" não devia perder os sinais de menor/maior). Exceção: se um campo
algum dia virar rich text de verdade (não existe hoje no protótipo), aplica-se lista de
permissão de tags na gravação, não bloqueio de caractere.

**Texto que vira parâmetro de consulta**: já é regra do documento 05, seção 11 —
`FromSqlRaw` proibido, EF Core sempre parametrizado, barrado por teste de arquitetura no CI.
Este documento reforça o mesmo princípio para qualquer filtro dinâmico que chegue de fora
do código (relatório com filtro do usuário, exportação, busca): o valor do usuário nunca
vira texto concatenado em SQL, nem em `LIKE`, nem em `ORDER BY` dinâmico.

**Upload de arquivo**: o documento 05 já exige magic bytes (não extensão), limite de
tamanho, antivírus e storage fora da webroot. Deste ângulo de qualidade de dado, mais uma
camada: o **nome do arquivo** enviado pelo usuário passa por `TextoNormalizado` antes de
gravar qualquer metadado — um nome de arquivo com caractere de controle ou sequência
`../` é vetor de path traversal, não só de exibição estranha.

**O que nunca vai para log**, em nenhum nível de severidade:

- CPF/CNPJ completo — só os 4 últimos dígitos, se for preciso identificar no log.
- E-mail completo em log de erro — domínio pode aparecer, endereço completo não.
- Qualquer token de integração (Entra ID client credentials, chave do Protheus).
- Qualquer campo marcado `[CampoSensivel]` (documento 05, seção 7 — margem, custo, comissão)
  e qualquer dado pessoal fora do escopo do log técnico (endereço completo, telefone
  completo).

`[V]` O Vórtice guarda **31 dos 239 parâmetros globais "criptografados" com cifra
proprietária reversível, cuja chave mora só no executável** — incluindo a senha SMTP em
texto reversível (documento 00-RELATORIO-EXTRACAO, achado 14) — e um **token de API em
texto claro** numa tabela de parâmetros (documento 01, achado 5.14). A regra acima existe
para que o CRM novo não repita o mesmo erro num lugar diferente: um log de aplicação
"esquecido" com dado sensível é tão grave quanto uma coluna de banco sem proteção.

---

## 6. LGPD aplicada ao dado

**Minimização**: só o campo que o caso de uso exige entra no formulário e no pipeline de
saneamento — documento 05, seção 9, já fixa isto no nível de tabela ("não copiamos as 767
tabelas do Vórtice"). No nível de dado, a mesma disciplina vale para o `PayloadOriginal` do
`Lead` (`src/Tracbel.Crm.Dominio/Crm/Lead.cs`): o JSON bruto é guardado **inteiro**, para
poder reprocessar um lead quando o mapeamento de campo for corrigido — mas só é exposto a
quem tem permissão de reprocessamento, nunca na tela comum de qualificação.

**Anonimização de verdade** (a correção mais importante desta seção): `[V]` as views
`GE$PESSOA_LGPD`, `GE$CONTATO_LGPD`, `GE$EMAIL_LGPD`, `GE$PESSOAFONE_LGPD` mascaram a coluna
de PII **e republicam o valor cru na mesma view**, em colunas `z_NOMERAZAO`, `z_NROCGCCPF`,
`z_EMAIL`, `z_FONENRO1` (documento 01, achado 6.1). A "anonimização" inteira é contornável
com um único `SELECT z_EMAIL FROM GE$PESSOA_LGPD` — não é anonimização, é mascaramento de
exibição com a fuga de propósito ao lado.

A correção não é "mascarar melhor" — é estrutural: quando um titular exerce o direito de
eliminação (documento 05, seção 9), o dado pessoal é **sobrescrito na própria coluna
original**. Não existe `z_*`, não existe segunda cópia em nenhuma view, nenhuma tabela de
staging, nenhum backup lógico acessível pela mesma credencial de aplicação. O registro
transacional permanece (obrigação fiscal/contábil), mas as colunas de identificação pessoal
— nome, documento, contato — viram um marcador de "titular anonimizado" mais um id
técnico sem significado, com a mesma técnica de qualquer sistema que precise manter
integridade referencial sem manter identidade: um hash unidirecional com sal por titular,
nunca reversível, usado só quando é preciso confirmar que duas linhas anonimizadas eram do
mesmo titular (join estatístico) — nunca para recuperar quem era.

**Pseudonimização em homologação**: dado de produção usado em ambiente de teste precisa ter
todo campo de PII substituído por dado **sintético mas estatisticamente equivalente** —
mesma distribuição de nome, cidade, faixa de faturamento — nunca o dado real, mesmo que o
ambiente de homologação seja "interno". Técnica recomendada: geração determinística por
semente = `Id` interno do registro (o mesmo `Id` sempre produz o mesmo nome sintético),
preservando consistência referencial entre tabelas relacionadas sem nunca expor o dado real
— nome e telefone sintéticos do "cliente 84391" continuam sendo os mesmos toda vez que o
ambiente de homologação for recriado, o que view de relatório nenhuma consegue distinguir
de dado real para fins de teste funcional.

`[V]` O Vórtice não tem essa disciplina: **`CRM_HOMO` está atrasado 51 tabelas e 362
colunas em relação à produção** e mudanças de schema acontecem direto em produção
(documento 00-RELATORIO-EXTRACAO, achado 8) — não há evidência de que o ambiente de
homologação sequer tenha dado pseudonimizado; o mais provável é ser cópia direta de
produção, o que é, por si, um risco de LGPD que este documento fixa a intenção explícita de
não repetir.

**Retenção e expurgo**: o documento 05, seção 10, já define a retenção de auditoria (18-24
meses online, depois arquivo). Do ângulo do **dado pessoal em si**: todo campo de PII tem,
desde a criação da tabela que o guarda, uma regra de retenção escrita e um job de expurgo
implementado **junto** — nunca "depois", que é como o Vórtice chegou a `GE_LgTb`, **11,9
milhões de linhas congeladas desde junho de 2023, nunca expurgadas** (documento 01, achado
10.3), com um job de compactação (`LOG_COMPACTA`) que existe no catálogo mas **não está
agendado**. Ter o job no repositório sem agendá-lo é o mesmo que não ter o job.

---

## 7. Qualidade contínua

Cinco métricas, medidas semanalmente, com meta e ação de quando cai. A coluna "hoje" usa a
medição ao vivo desta rodada
([qualidade-dados-legado](../extracao-vortice/qualidade-dados-legado.md)) como baseline —
não porque o Vórtice seja o alvo, mas porque é o único dado real disponível para calibrar
"o que é razoável esperar" antes do banco novo ter volume próprio.

| Métrica | Definição | Meta | Hoje (Vórtice, referência) | O que fazer quando cai |
|---|---|---|---|---|
| **Completude** | % de campos obrigatórios preenchidos | ≥ 98% | `IV_Processo.Status` em branco: 480.397/1.174.932 ≈ 59% preenchido | Abre item para o dono do processo de entrada — completude baixa é sintoma de formulário mal desenhado ou integração incompleta, não se corrige só no dado |
| **Validade** | % de valores que passam no tipo de valor correspondente (`TentarCriar` = true) | 100% | `GE_Pessoa.Email` inválido: 528/119.348 ≈ 99,6% válido | Se cair abaixo de 100% **depois** do saneamento estar em produção, é bug — algum ponto de entrada está pulando o pipeline, não é "dado ruim", é "código com furo" |
| **Unicidade** | % de registros sem duplicata pela chave natural (seção 4) | 100% | Vórtice: 124 mil pares na fila de dedup nunca resolvida | Investigar o ponto de entrada que permitiu a duplicata (camada 1 da seção 4 falhou); rodar rotina de fusão assistida, nunca fusão automática silenciosa |
| **Atualidade** | Dias desde o último contato confirmado, por classe do cliente | A: 30d · B: 60d · C: 90d · D: 120d (as mesmas faixas já usadas no protótipo, `prototipo/dados-seed/meta-frequencia.json`) | — (métrica nova, sem equivalente direto no Vórtice: `IVS_Pes`/RFV está 100% morto, documento 01 achado 9.7) | Dispara tarefa de recontato automaticamente no dia seguinte ao vencimento da meta — nunca depende de alguém lembrar |
| **Consistência** | % de registros sem contradição entre colunas relacionadas (ex.: "realizada" e "status de conclusão" concordando) | ≥ 99,5% | `IV_Agenda.Realizada='S'` × `Status<>'C'`: 132.878/931.989 → **85,7% consistente hoje** | Toda vez que duas colunas podem discordar, a pergunta certa é "por que existem duas colunas para a mesma resposta" — a correção estrutural é ter uma fonte de verdade, não reconciliar as duas eternamente |

O relatório semanal soma as cinco métricas por entidade (`comercial.Cliente`,
`processo.Processo`, `frota.Equipamento`...) e é a mesma fonte que alimenta o painel de
qualidade citado no documento 06 (portão de qualidade da fase) — nenhuma métrica nova de
processo aqui, só a extensão da disciplina de teste para o dado em produção.

**Medição do protótipo, nesta rodada** (`node scripts/dados/validar-seed.mjs`, relatório
completo em [qualidade-dados-seed](../../docs/prototipo/qualidade-dados-seed.md)):

| Regra | Achados em 45 arquivos |
|---|---:|
| Documento (CNPJ/CPF) | 55 |
| Dinheiro fora do catálogo | 1 |
| Duplicata de chave natural | 1 par (CNPJ `11.234.567/0001-89` em dois clientes) |

O achado dominante — **as 55 ocorrências de CNPJ no material de protótipo falham no dígito
verificador, 100% delas** — não é sinal de bug no validador: são CNPJs inventados à mão para
o protótipo visual, plausíveis ao olho (14 dígitos, máscara correta) mas nunca calculados. É
o argumento mais direto deste documento para a seção 8: **nem o próprio material de
referência do time entra como carga real sem passar pelo saneamento.**

---

## 8. Migração do Vórtice

**Princípio único, sem exceção:** nada entra no banco novo — nem dado do Vórtice, nem dado
do protótipo, nem dado de teste — sem passar pelo mesmo pipeline de saneamento do dado real.
A prova de que isto não é teórico está na própria seção 7: **o material do protótipo
reprovou na mesma regra que reprovaria uma linha do Vórtice.**

### 8.1 O pipeline (`Tracbel.Crm.Integracao/Saneamento/`)

```csharp
namespace Tracbel.Crm.Integracao.Saneamento;

public interface ISanitizador<in TBruto, TLimpo>
{
    ResultadoSaneamento<TLimpo> Sanear(TBruto bruto);
}
```

- **`CorrecaoAplicada(Campo, ValorOriginal, ValorNormalizado, Motivo)`** — o registro do
  princípio 1.4: toda correção automática, nomeada.
- **`CampoRejeitado(Campo, ValorOriginal, Motivo)`** — o registro do princípio 1.3: toda
  rejeição, com o porquê.
- **`ResultadoSaneamento<T>`** — reaproveita o `Resultado<T>` que já existe no domínio
  (`Tracbel.Crm.Dominio.Comum.Resultado<T>`, usado em toda a camada de aplicação) para
  sucesso/valor, e acrescenta as duas listas acima. Não duplicamos o conceito de
  sucesso/falha — só o estendemos com a trilha que a migração exige.
- **`RegistroSaneamento`** — o que persiste como trilha de uma linha de importação: fonte,
  entidade, quando (via `IRelogio`, a mesma abstração de tempo testável já usada no domínio),
  aceito ou não, e as duas listas.

Exemplo de referência implementado nesta rodada — `SanitizadorLeadExterno`, saneando um
lead recebido de fora (site, RD Station, WhatsApp) campo a campo, cada um pelo tipo de valor
correspondente, antes de alcançar `Lead.Criar`:

```csharp
public sealed class SanitizadorLeadExterno : ISanitizador<LeadExternoBruto, LeadExternoSaneado>
{
    public ResultadoSaneamento<LeadExternoSaneado> Sanear(LeadExternoBruto bruto)
    {
        // Nome ausente ou vazio depois de normalizado -> rejeita o registro inteiro.
        // Cada outro campo (e-mail, telefone, documento) é saneado por si: um e-mail
        // inválido vira CampoRejeitado, mas NÃO derruba o lead inteiro — a composição
        // entre campos ("precisa de e-mail OU telefone") é regra de Lead.Criar, não deste
        // pipeline. Ver Tracbel.Crm.Integracao/Saneamento/SanitizadorLeadExterno.cs.
    }
}
```

Este é o padrão para todo adaptador de origem externa futuro (Vórtice, Protheus, planilha):
um `TBruto` só com string solta, um `TLimpo` só com tipo de valor, e a composição de regra
de negócio deixada para a entidade de domínio que vai consumir o resultado.

### 8.2 Regra de saneamento por entidade migrada

| Entidade do Vórtice | Entidade do CRM novo | Regra de saneamento específica | Recusa migrar quando |
|---|---|---|---|
| `GE_Pessoa` | `comercial.Cliente` | `CpfCnpj` a partir de `NroCGCCPF` (`decimal`) + `DigCGCCPF`, reconstituído com zero à esquerda restaurado por consulta ao cadastro de raiz de CNPJ conhecida — **não** por conversão direta de número para texto (perderia o zero de novo) | `FisicaJuridica NOT IN ('F','J')` (33 linhas medidas) sem revisão manual; `NroCGCCPF` que não permite reconstituir um CPF/CNPJ com dígito verificador válido mesmo após tentativa de restauração de zero |
| `IV_Processo` | `processo.Processo` | `Status`/`Fase` mapeados por tabela de sinônimo explícita (`FINALIZADO`+`FINALIZADA`+`Finalizado` → um único `Id` de `processo.SituacaoProcesso`) — nunca migrados como texto | `Status` em branco **e** nenhuma tarefa/interação filha viva (processo verdadeiramente órfão, documento 01: 345.535 linhas de `IV_ProcDado` sem processo) |
| `IV_Agenda` | `processo.Tarefa` | `Vendedor` resolvido para `UsuarioId` via tabela de login→usuário (487.038 linhas hoje guardam login, não ID) — nunca migrado como texto | `Vendedor` que não resolve para nenhum usuário conhecido, ativo ou desligado |
| `IV_Historico` | `processo.Interacao` | `ResultadoCmpl`/`Detalhe` saneados por `TextoNormalizado`; os 44.962 registros truncados em 20 caracteres migram com o texto que sobrou (não há como recuperar o que foi cortado) e uma flag `TextoTruncadoNaOrigem = true`, nunca finge que o texto sempre foi curto | Nunca recusa por conteúdo do texto — histórico é fato ocorrido; entra sempre, saneado |
| `EXT_Veic` (morta) | `frota.Equipamento` | `Chassi` validado pelo tipo `Chassi` (3.262 de 8.020 hoje fora do padrão de 17); fonte preferida é o cadastro **vivo** `IV_ClientePropr` (246.684 linhas), não o `EXT_Veic` morto desde 05/2024 | `Chassi` que não normaliza para 17 caracteres válidos, sem correspondência manual possível |
| `EXT_Titulo` | (referência via `integracao.ChaveExterna` — título continua no domínio do ERP, seção 2.10) | `Status` mapeado pela tabela de sinônimo código↔texto (`B`=`Pago` presumido, a confirmar com o financeiro antes de migrar — **não adivinhado** nesta fase); `DtaVencto`/`DtaEmissao` por `DataHoraUtc`, sentinela `1900-01-01` coalescida para `NULL` antes da validação de faixa | `DtaVencto` fora de `[1990, ano+30]` sem correspondência de emissão plausível (6 títulos medidos) |

### 8.3 O que se recusa a migrar, sem exceção

- Qualquer linha que falhe o saneamento de campo obrigatório **e** não tenha correspondência
  manual possível (não "não tentamos resolver" — "tentamos e não deu").
- Colunas de senha, hash e token (`GE_Usuario.Senha`/`Senha3`, `GE_UsuarioSenhaMem`,
  `GE_PessoaPasw`, tokens em `GE_CONTATOAPP.FCMTOKEN` e afins) — **por decisão, não por
  limitação técnica**. O CRM novo não guarda senha (documento 05, seção 3); migrar essas
  colunas seria copiar um passivo de segurança para o sistema que existe para não ter esse
  passivo.
- Módulo inteiro sem uso: financiamento (`IVF_*`, 20 tabelas vazias), call center (`IVC_*`,
  6 tabelas vazias), material (`IVM_*`), produto/preço (`IVP_*`) — 100% vazios, nada a
  saneiar nem migrar.
- Log (`GE_LOG_*`, `GE_LgTb`, `IV_AgendaLog`) — 52,4% do banco, não é dado de negócio.

**Rejeitar não é ignorar.** Toda linha recusada vira uma linha em
`integracao.MensagemDescartada` (documento 04, seção 8) com o motivo exato da recusa — a
mesma tabela que já existe para mensagem de integração falhada. Migração e integração
contínua compartilham o mesmo conceito de "isto não entrou, e aqui está por quê": `[V]` a
fila de e-mail do Vórtice tem **22.512 falhas silenciosas, sem retry, e sem ninguém
sabendo** (documento 04, seção 8) — o oposto exato do que `integracao.MensagemDescartada`
existe para garantir.

---

## 9. O que o banco garante por estrutura

Para o [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md) incorporar. Este documento não declara
`CREATE TABLE`; a tabela abaixo é o contrato que qualquer `CREATE TABLE` precisa cumprir
para as regras das seções 2 e 3 valerem também como última linha de defesa, não só como
disciplina de aplicação.

| Dado | Tipo de coluna exigido | Nunca use | Constraint | Por que (com número) |
|---|---|---|---|---|
| CPF/CNPJ | `varchar(14)` só dígitos, `NOT NULL` quando obrigatório | `decimal`/`numeric` | `CHECK (LEN(...) = 11 OR LEN(...) = 14)` + índice único filtrado por empresa | `decimal(13,0)` faz **67,9% dos CNPJs e 15,8% dos CPFs** do Vórtice perderem zero à esquerda (seção 2.2) |
| Telefone | `varchar(20)` | `decimal`/`numeric` | — (formato validado na aplicação; banco só limita tamanho) | `decimal(12,0)` estoura com DDI ou zero à esquerda (documento 04, seção 1.2); 168 linhas medidas fora de faixa |
| E-mail | `nvarchar(200)` | `varchar` (corrompe acento) | — | Normalização (minúsculo) é disciplina de aplicação; banco não força caixa |
| CEP | `char(8)` fixo, sem hífen | `varchar` de tamanho variável | `CHECK (LEN(...) = 8)` | Formato fixo conhecido; variável só convida a gravar formatos mistos |
| UF | `char(2)` | — | `CHECK (UF IN ('AC','AL',...))` **— única exceção à regra "catálogo é FK, não CHECK"** (seção 3.1), porque as 27 UFs são uma lista nacional que não muda por decisão de negócio da Tracbel | Lista verdadeiramente imutável; distinta de motivo de perda, classe de cliente etc., que são catálogo de negócio e **precisam** ser FK |
| Chassi | `varchar(17)` fixo | `varchar` maior que 17 | `CHECK (LEN(...) = 17)` + índice único filtrado | 40,7% dos chassis do Vórtice não têm 17 caracteres (seção 2.11) — sem o `CHECK`, o banco novo repete o padrão |
| Data/hora | `datetime2(3)`, sempre gravada em UTC | `datetime` (precisão/faixa piores) | `CHECK (ano >= 1900)` no banco; o teto móvel (`ano atual + 30`) **não** vira `CHECK` — SQL Server não aceita função não determinística (`GETDATE()`) direto numa constraint; o teto móvel fica só no tipo de valor da aplicação (`DataHoraUtc`) | Vencimento gravado em 5024, 2223 (seção 2.8) |
| Dinheiro | `decimal(18,2)` (ou a escala que o negócio pedir, sempre 2 casas) | `float`, `money` | `CHECK (valor >= 0)` onde a coluna representa grandeza não negativa | Arredondamento silencioso do `float`; `EXT_Titulo.VlrOriginal numeric(14,2)` é o contraexemplo de que o tipo certo já resolve sozinho (seção 2.9) |
| Domínio/catálogo de negócio | FK `int`/`bigint` para tabela de referência | `varchar` com valor "esperado" só por convenção da aplicação | FK `NOT NULL` (quando obrigatório) + coluna `EstaAtivo`/situação na tabela de referência, nunca `DELETE` de linha referenciada | `IV_Processo.Status`: 4+ grafias medidas para 2 conceitos, 480.397 em branco (seção 3) |
| Texto livre | `nvarchar(N)` do tamanho real do negócio, generoso | Coluna menor que a maior gravação real observada | — (tamanho validado na aplicação antes do `INSERT`; rejeita, não deixa o banco truncar) | `IV_Historico.ResultadoCmpl varchar(150)` truncado em 20 pela aplicação — o banco permitia 150; quem cortou foi o código (seção 2.12) |
| Coordenada | `decimal(9,6)` (latitude) / `decimal(9,6)` (longitude) — 6 casas já dá ~11 cm de precisão, suficiente para atendimento em campo | `float` (impreciso para comparação de igualdade) | `CHECK` de faixa matemática (`lat BETWEEN -90 AND 90`); faixa de negócio (só Brasil) fica na aplicação, pelo mesmo motivo do teto móvel de data — não é constraint universal, é regra de área de atuação que pode mudar | 12 coordenadas implausíveis medidas em `IV_Historico` (seção 2.13) |

**Toda tabela de catálogo**, sem exceção, tem a coluna de aposentadoria (`EstaAtivo bit` ou
equivalente) e a FK que aponta para ela é declarada **sem** `ON DELETE CASCADE` — apagar uma
linha de catálogo referenciada deve ser impossível por construção, não só por convenção
(seção 3.1: "um valor usado no histórico nunca se apaga").
