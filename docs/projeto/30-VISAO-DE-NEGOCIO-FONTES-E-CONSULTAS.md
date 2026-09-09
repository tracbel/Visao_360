# A visão de negócio — de onde vem cada número, e o que ainda não existe

> Documento 30 · Versão 1.0 · 09/09/2026
> Responde ao pedido: faturamento por cliente, equipamento, região e filial; potencial do cliente
> (hectares, crédito, cultura, tamanho da fazenda) e cadastro — **sem depender do texto livre do
> CRM legado**.
> Convenção: `[medido]` = valor obtido ao vivo contra a produção em 09/09/2026.
> Todo acesso foi **somente leitura**.

---

## O que mudou: acesso direto ao banco do Protheus

Até ontem o faturamento vinha pela API REST, uma filial por vez, **1.340 requisições** contra o
ERP de produção. Com `TOTVS_DB_SERVER` a mesma pergunta virou **uma consulta** — as dezesseis
filiais de uma vez, com `GROUP BY` feito pelo banco.

Isso não é só velocidade. A API escondia a filial atrás de um cabeçalho que mudava sozinho; o SQL
mostra `D2_FILIAL` como coluna, e o erro de ler uma filial achando que eram todas **deixa de ser
possível**.

| Fonte | Endereço | Estado |
|---|---|---|
| Protheus SQL | `10.100.6.243:1433`, base `TMPRD` | ✅ **funciona** |
| Protheus REST | `10.100.10.98:5891` | ✅ funciona (agora redundante) |
| **ART (MySQL)** | `aftracbel.tracbel.com.br:3306` → `10.235.0.58` | ❌ **sem rota** — nem ping |
| Vórtice (SQL) | `COLWCRM` / `10.150.14.65:1433` | ✅ funciona |
| CRM novo | container local / servidor de aplicação | ✅ funciona |

---

## 1. O faturamento, corrigido e completo

`[medido]` **set/2023 a set/2026, dezesseis filiais, só CFOP de venda:**

| Classe | Valor | Fatia |
|---|---:|---:|
| Máquina | R$ 1.395,8 mi | 55% |
| Peça | R$ 821,3 mi | 33% |
| **Repasse da fábrica** | R$ 145,4 mi | 6% |
| Serviço | R$ 120,5 mi | 5% |
| Não classificado | R$ 35,2 mi | 1% |
| **Total** | **R$ 2.518,2 mi** | 8.888 clientes · 946.445 itens |

**O "repasse" é uma classe nova, e ela conserta um erro anterior.** Comissão de venda direta
(`COM`), incentivo sobre vendas (`ICV`), bônus de performance (`P4P`) e comissões de peças,
consórcio e seguro (`CPP`, `COC`, `COS`) **são receita paga pela John Deere à concessionária** —
não são venda a cliente. Ficavam misturadas em "serviço" e inflavam o pós-venda.

É por isso que **a John Deere aparecia como maior cliente**: `[medido]` três CNPJs dela somam
R$ 159,4 mi no período, e nenhum real disso é venda para um produtor.

### 1.1 Por filial — e o que isso revela

| Filial | Total | Máquina | Peça | Clientes |
|---|---:|---:|---:|---:|
| Ribeirão Preto | R$ 966,0 mi | R$ 683,9 mi | R$ 158,5 mi | 2.621 |
| Bebedouro | R$ 376,7 mi | R$ 303,2 mi | R$ 37,3 mi | 1.284 |
| São José do Rio Preto | R$ 180,4 mi | R$ 59,2 mi | R$ 98,0 mi | 1.396 |
| **Orlândia** | R$ 179,1 mi | **R$ 4,8 mi** | **R$ 151,5 mi** | 1.049 |
| Catanduva | R$ 155,6 mi | R$ 57,7 mi | R$ 86,3 mi | 993 |
| Franca | R$ 115,0 mi | R$ 102,2 mi | R$ 7,9 mi | 767 |
| Barretos | R$ 110,8 mi | R$ 28,8 mi | R$ 65,2 mi | 895 |
| Votuporanga | R$ 85,7 mi | R$ 31,0 mi | R$ 44,0 mi | 725 |
| Jales | R$ 83,0 mi | R$ 30,7 mi | R$ 37,7 mi | 832 |
| **Araraquara** | R$ 65,6 mi | **R$ 3,3 mi** | R$ 49,3 mi | 745 |
| Marília | R$ 61,6 mi | R$ 46,7 mi | R$ 6,7 mi | 505 |
| Tupã | R$ 61,5 mi | R$ 35,5 mi | R$ 18,5 mi | 519 |
| Guaíra | R$ 29,6 mi | R$ 5,6 mi | R$ 18,1 mi | 511 |
| **Ituverava** | R$ 23,3 mi | **R$ 0,6 mi** | R$ 22,6 mi | 307 |
| **Itápolis** | R$ 13,4 mi | **R$ 1,6 mi** | R$ 11,5 mi | 571 |
| **Monte Alto** | R$ 10,9 mi | **R$ 1,0 mi** | R$ 8,4 mi | 531 |

**Cinco filiais praticamente não vendem máquina.** Orlândia é a **quarta maior em faturamento** e
vende R$ 4,8 mi de máquina contra R$ 151,5 mi de peça — é um centro de peças, não uma revenda.
Araraquara, Ituverava, Itápolis e Monte Alto seguem o mesmo padrão.

Isso muda como se lê o desempenho delas: cobrar meta de máquina de Monte Alto, que vendeu R$ 1,0
mi em três anos, é cobrar o que a praça não faz. **A diretoria precisa ver isso separado, e hoje
nenhum relatório separa.**

---

## 2. O cadastro do cliente — o que o Protheus tem de verdade

`[medido]` sobre **38.689 clientes** na `SA1010`:

| Campo | Título | Preenchido | Serve? |
|---|---|---:|---|
| `A1_NOME` / `A1_NREDUZ` | Nome e fantasia | 38.689 | ✅ |
| `A1_CGC` | CNPJ/CPF | 38.682 | ✅ |
| `A1_MUN` / `A1_EST` | Cidade e UF | 38.682 | ✅ **é a "região"** |
| `A1_END` / `A1_BAIRRO` / `A1_CEP` | Endereço | — | ✅ |
| `A1_INSCR` / `A1_INSCRM` | Inscrição estadual e municipal | — | ✅ |
| `A1_PESSOA` | Física ou jurídica | — | ✅ |
| **`A1_LC`** | **Limite de crédito** | **10.760** | ✅ **R$ 123,7 mi concedidos** |
| `A1_RISCO` | Risco | 38.675 | ✅ |
| `A1_VENCLC` | Vencimento do limite | — | ✅ |
| `A1_ULTCOM` | Última compra | 10.587 | ✅ |
| `A1_ATIVIDA` | Atividade | **0** | ❌ vazio |
| `A1_SATIV1..8` | Segmentos | **1** | ❌ vazio |
| `A1_VEND` | **Vendedor** | **0** | ❌ vazio |
| `A1_GRPVEN` | **Grupo econômico** | **0** | ❌ vazio |

**Correção do que eu havia recomendado no documento 29.** Eu sugeri usar `A1_VEND` para comparar
o vendedor do ERP com o CEN do CRM, e `A1_GRPVEN` para resolver grupo econômico. **Os dois estão
vazios em 100% dos registros.** Nenhuma das duas ideias se sustenta — o ERP não sabe de quem é o
cliente, e o agrupamento econômico terá de sair da raiz do CNPJ.

**O crédito é o achado bom, e ele responde direto ao "esse cara tem crédito".** Não é uma pergunta
de opinião: `A1_LC` diz quanto, `A1_VENCLC` diz até quando e `A1_RISCO` diz em que classe.
Cruzado com faturamento, separa três situações que hoje ninguém enxerga junto: quem tem crédito e
não usa, quem usa tudo, e quem compra sem limite nenhum.

---

## 3. A frota — por que o Protheus não serve, e o ART sim

`[medido]` sobre **38.369 chassis** na `VV1010`:

| Campo | Preenchido |
|---|---:|
| `VV1_CHASSI` | 38.369 |
| `VV1_MODVEI` (modelo) | 38.239 |
| **`VV1_CLIULV` (dono)** | **3.550 — 9%** |
| `VV1_FABANO` (ano) | **0** |
| `VV1_KMS` (horímetro) | **0** |
| `VV1_SEGMOD` (segmento) | **1** |

**Nove por cento da frota tem dono, e não há ano nem horímetro.** Não dá para responder "quais
máquinas este cliente tem" nem "qual está no ponto de troca" com esse dado.

**A planilha que você mandou é o modelo do ART**, e ele tem exatamente o que falta: horímetro
atual, data do registro, **origem da atualização** (Operation Center, Simova ou manual), família,
série, ano de fabricação, data de entrega técnica e cliente atual com filial.

`ART_DB_SERVER` = `aftracbel.tracbel.com.br` (MySQL, base `aftba`, view `bi_art_veiculos`), que
resolve para `10.235.0.58`. **A VPN roteia a faixa, mas o host não responde nem a ping** — porta
3306, 3307 e 33060 todas fechadas. **É pendência de rede, não de credencial.**

---

## 4. As carteiras — o modelo de quatro colunas não existe

Sua planilha prevê **Carteira Máquinas, Peças, Serviços e PUK**. `[medido]` no que veio do
Vórtice:

| Linha de negócio | Clientes | Vínculos |
|---|---:|---:|
| Soluções Integradas (PUK) | 12.346 | 18.411 |
| Venda de Máquinas e Implemento | 16.401 | 16.409 |
| Venda de Pneus | 8.032 | 8.032 |
| Venda de AMS | 3.270 | 3.270 |
| Venda de Peças | 2.112 | 2.112 |
| Vendas Digitais | 526 | 526 |
| Venda de Peças Externo | 211 | 211 |
| **Venda de Serviços** | **3** | **3** |
| Prospecção de Máquinas | 4 | 4 |
| Prospecção Peças | 4 | 4 |

**"Venda de Serviços" tem três clientes.** A coluna existe no sistema e está vazia na prática.
Peças soma 2.323 num universo de 49 mil vínculos — enquanto o faturamento mostra **R$ 821 milhões
de peça**. Ou seja: **a peça vende muito e quase não tem carteira.**

Isso é uma decisão de negócio, não um defeito de dados: a peça é vendida no balcão e por telefone,
sem carteira nominal. Mas significa que a coluna "Carteira Peças" da planilha vai vir vazia para a
quase totalidade dos clientes, e é melhor saber disso antes de desenhar a tela.

---

## 5. O que ainda não existe em lugar nenhum

Você pediu **hectares, cultura, tamanho da fazenda e atividade**. Levantei as três fontes:

| Onde procurei | Resultado |
|---|---|
| Protheus `SA1010` | `A1_ATIVIDA` e `A1_SATIV1..8` vazios |
| Protheus — campos de área/hectare | não existem |
| ART | **é onde deve estar** — sem rota |

A planilha mostra `Extensão Área`, `Atividade Principal`, `Área Agriculturável`, `Cultivo
Principal`, `Área Cultivo Principal`, `Cultivo Secundário` e `Área Cultivo Secundário`. Nenhum
desses campos existe no Protheus. **Ou vem do ART, ou vem do cadastro do Vórtice, ou não existe.**

---

## 6. As consultas

Todas somente leitura, com `WITH (NOLOCK)` para não travar o ERP, e `D_E_L_E_T_ = ''` — sem esse
filtro entram os registros excluídos logicamente, que o Protheus mantém na tabela.

### 6.1 Faturamento por classe (o número da diretoria)

```sql
SELECT
  CAST(SUM(d.D2_TOTAL)/1000000 AS decimal(14,1))                            AS TotalMi,
  CAST(SUM(CASE WHEN m.Classe='Maquina' THEN d.D2_TOTAL ELSE 0 END)/1000000 AS decimal(14,1)) AS MaquinaMi,
  CAST(SUM(CASE WHEN m.Classe='Peca'    THEN d.D2_TOTAL ELSE 0 END)/1000000 AS decimal(14,1)) AS PecaMi,
  CAST(SUM(CASE WHEN m.Classe='Servico' THEN d.D2_TOTAL ELSE 0 END)/1000000 AS decimal(14,1)) AS ServicoMi,
  CAST(SUM(CASE WHEN m.Classe='Repasse' THEN d.D2_TOTAL ELSE 0 END)/1000000 AS decimal(14,1)) AS RepasseMi,
  COUNT(DISTINCT d.D2_CLIENTE + d.D2_LOJA)                                  AS Clientes
FROM SD2010 d WITH (NOLOCK)
CROSS APPLY (SELECT Classe = CASE
    WHEN LTRIM(RTRIM(d.D2_GRUPO)) = 'VEIC'                                 THEN 'Maquina'
    WHEN LTRIM(RTRIM(d.D2_GRUPO)) LIKE '1[0-9][0-9][0-9]'                  THEN 'Peca'
    WHEN LTRIM(RTRIM(d.D2_GRUPO)) IN ('SRV','MO_O','MO_T','SAD')           THEN 'Servico'
    WHEN LTRIM(RTRIM(d.D2_GRUPO)) IN ('COM','ICV','P4P','CPP','COC','COS') THEN 'Repasse'
    ELSE 'Outros' END) m
WHERE d.D_E_L_E_T_ = ''
  AND d.D2_EMISSAO >= '20230901'
  AND d.D2_TIPO = 'N'
  AND LTRIM(RTRIM(d.D2_CF)) IN ('5102','6102','5108','6108','5117','6117',
                                '5403','6403','5405','6405','5656','6656',
                                '5659','6659','5933','6933');
```

**Os três filtros do `WHERE` são a diferença entre R$ 2,5 bi e R$ 5,3 bi.** `D2_TIPO='N'` tira
devolução e beneficiamento; a lista de CFOP tira transferência entre filiais, remessa de
demonstração, retorno de conserto e baixa de estoque. A lista é **de inclusão**: CFOP novo que
ninguém classificou fica de fora e aparece como diferença, em vez de entrar como venda em silêncio.

### 6.2 Por filial

A mesma consulta com `GROUP BY d.D2_FILIAL`. `D2_FILIAL` traz o código de seis dígitos
(`010101`…`010118`), que é o mesmo de `organizacao.Empresa` no CRM — **não precisa de de-para**.

### 6.3 Cliente com faturamento, crédito e risco

```sql
WITH Fat AS (
  SELECT d.D2_CLIENTE AS Cod, d.D2_LOJA AS Loja,
         SUM(d.D2_TOTAL) AS Valor,
         SUM(CASE WHEN LTRIM(RTRIM(d.D2_GRUPO))='VEIC' THEN d.D2_TOTAL ELSE 0 END) AS Maquina,
         MAX(d.D2_EMISSAO) AS UltimaNota
  FROM SD2010 d WITH (NOLOCK)
  WHERE d.D_E_L_E_T_='' AND d.D2_EMISSAO >= '20230901' AND d.D2_TIPO='N'
    AND LTRIM(RTRIM(d.D2_CF)) IN ('5102','6102','5108','6108','5117','6117',
                                  '5403','6403','5405','6405','5656','6656',
                                  '5659','6659','5933','6933')
  GROUP BY d.D2_CLIENTE, d.D2_LOJA
)
SELECT a.A1_NOME, a.A1_MUN, a.A1_EST, a.A1_CGC,
       f.Valor, f.Maquina, f.UltimaNota,
       a.A1_LC AS LimiteCredito, a.A1_RISCO, a.A1_VENCLC
FROM Fat f
JOIN SA1010 a WITH (NOLOCK)
  ON a.A1_COD = f.Cod AND a.A1_LOJA = f.Loja AND a.D_E_L_E_T_ = ''
ORDER BY f.Valor DESC;
```

**A chave é código + loja, nunca só o código.** O mesmo código com lojas diferentes tem **CNPJs
diferentes** — matriz e filiais do mesmo grupo. Casar só por código atribui a nota da filial ao
CNPJ da matriz.

### 6.4 Separar o repasse da fábrica

```sql
-- Quanto do "faturamento" é dinheiro que a John Deere paga, e não venda a cliente.
SELECT LTRIM(RTRIM(d.D2_GRUPO)) AS Grupo,
       LTRIM(RTRIM(b.BM_DESC))  AS Descricao,
       CAST(SUM(d.D2_TOTAL)/1000000 AS decimal(14,1)) AS Mi
FROM SD2010 d WITH (NOLOCK)
LEFT JOIN SBM010 b WITH (NOLOCK)
  ON LTRIM(RTRIM(b.BM_GRUPO)) = LTRIM(RTRIM(d.D2_GRUPO)) AND b.D_E_L_E_T_ = ''
WHERE d.D_E_L_E_T_='' AND d.D2_EMISSAO >= '20230901' AND d.D2_TIPO='N'
  AND LTRIM(RTRIM(d.D2_GRUPO)) IN ('COM','ICV','P4P','CPP','COC','COS')
GROUP BY LTRIM(RTRIM(d.D2_GRUPO)), LTRIM(RTRIM(b.BM_DESC))
ORDER BY SUM(d.D2_TOTAL) DESC;
```

---

## 7. O que eu recomendo mostrar para a diretoria

Em ordem de impacto, e **tudo isto já é mensurável hoje**, sem o ART:

1. **Faturamento por filial separando máquina de peça.** É o que mostra que Orlândia é um centro
   de peças e Monte Alto não vende máquina. Nenhum relatório atual separa.
2. **Receita de cliente × repasse da fábrica.** R$ 145,4 mi que hoje entram como se fossem venda.
3. **Crédito concedido × crédito usado.** R$ 123,7 mi de limite em 10.760 clientes, cruzado com o
   que cada um comprou.
4. **Quem compra e o CRM não conhece.** Já medido: R$ 798,6 mi de contrapartes sem cadastro no
   CRM, das quais R$ 573,4 mi são clientes que deveriam existir e não existem.
5. **Quem comprou máquina e não voltou para peça.** Agora com o denominador certo, sem o repasse
   contaminando o pós-venda.

O que **não** recomendo prometer antes do ART: hectares, cultura, tamanho da fazenda e frota por
cliente com horímetro. São exatamente os itens da sua planilha que não existem em nenhuma fonte
que eu alcanço hoje.

---

## 8. Pendências

1. **ART sem rota** (`10.235.0.58:3306`). Bloqueia frota com horímetro e todo o dado agronômico.
   É pedido de liberação de rede — a credencial já existe.
2. **CFOP 5949** — R$ 38,1 mi em "outra saída não especificada", fora do total por decisão
   conservadora. Precisa da resposta do fiscal.
3. **Carteira de Peças e de Serviços vazias** — decisão de negócio, não defeito. Define se as
   colunas entram na tela.
4. **Grupo econômico** — `A1_GRPVEN` vazio; terá de sair da raiz do CNPJ.
5. **A API ainda não autentica ninguém.** Ver documento 13 e a seção de Entra ID.
