# 43 — Municípios pendentes de validação comercial (CEN por município)

> **Versão 1.0 · 15/09/2026 · decisão D-4.** Relatório pedido na Fase 2: os municípios em que as duas
> fontes discordam sobre quem é o CEN.
>
> **Nenhuma fonte foi assumida como verdade. Nada foi resolvido automaticamente.** Os casos estão
> marcados como `PENDENTE DE VALIDAÇÃO COMERCIAL`.
>
> Medido no banco de arquivo `TracbelCrmArquivo20260915` (contêiner `tracbel-crm-ensaio`), somente
> leitura — os dados foram removidos do banco operacional na sanitização (documento 38).

## 1. Resumo

| Situação | Municípios | Clientes nesses municípios | Vínculos de carteira | Marcação |
|---|---:|---:|---:|---|
| **Nomes diferentes** | **82** | 6.988 | 213 | `PENDENTE DE VALIDAÇÃO COMERCIAL` |
| **Provável mesma pessoa** (grafia diferente) | **46** | 3.721 | 97 | `PENDENTE DE VALIDAÇÃO COMERCIAL` |
| Mesmo nome ou mesmo usuário | 75 | 7.313 | 186 | segue sem pendência |
| **Total com CEN declarado** | **203** | 18.022 | 496 | |

Os 203 municípios com CEN declarado **estão todos na ADR**. A área de atuação tem 238 linhas: 203 na
ADR (todas com filial responsável) e 35 sem marcação, que também não têm CEN declarado.

## 2. As duas fontes

| | Fonte A | Fonte B |
|---|---|---|
| Arquivo | `Area de Atuação.xlsx`, coluna `CEN` | `CEN e Gestor por Municipio.xlsx`, colunas `Vendedor_Territorio` e `Gerente_Territorio` |
| Municípios com CEN | 203 | 203 |
| CEN identificado como usuário do CRM | 73 | 163 |
| CEN não identificado | 111 | 40 |
| Vaga declarada como "a contratar" | 19 | 0 |
| Declara gestor? | não | sim — 203 municípios (122 identificados, 81 não) |

O gestor tem **uma fonte só**: não há divergência a resolver, mas também não há segunda fonte para
confirmar.

## 3. Como a divergência foi medida

Foi usada **a mesma regra do domínio** (`ResponsavelPeloMunicipio.CompararCen`, em
`AreaDeAtuacao.cs:369-385`), nesta ordem:

1. mesmo usuário do CRM identificado nas duas fontes → **mesmo**;
2. mesma chave de nome (sem o domínio do e-mail, sem acento, com ponto, sublinhado e hífen virando
   espaço) → **mesmo**;
3. uma fonte diz "vaga a contratar" e a outra diz um nome → **nomes diferentes**;
4. mesmo primeiro nome, ou um nome contido no outro → **provável mesma pessoa**;
5. o resto → **nomes diferentes**.

Comparação por igualdade literal daria 131 divergências; com a regra do domínio, são **82 nomes
diferentes** e **46 prováveis mesma pessoa** — os 82 são exatamente os que o documento 32 já
registrava.

## 4. O relatório

**O arquivo completo, com os nomes das duas fontes, está em:**

```text
dados-locais/territorio/municipios-cen-por-fonte-20260915.csv
```

Fora do Git, porque traz nome de pessoa. Colunas: `CodigoIbge`, `Municipio`, `Uf`, `NomeFonteA`,
`SituacaoFonteA`, `NomeFonteB`, `SituacaoFonteB`, `Comparacao`.

### 4.1 Perfil dos 128 pendentes

| Situação na fonte A | Municípios | | Situação na fonte B | Municípios |
|---|---:|---|---|---:|
| Não identificado | 92 | | Não identificado | 21 |
| Usuário identificado | 17 | | Usuário identificado | 107 |
| Vaga a contratar | 19 | | — | — |

**Leitura:** na maioria dos casos a fonte A traz um nome que a carga não conseguiu casar com usuário do
CRM, e a fonte B traz um usuário identificado. Isso **não** faz da fonte B a verdade: ela pode estar
desatualizada — nenhuma das duas declara desde quando vale.

### 4.2 Os dez municípios divergentes com mais clientes

| Código IBGE | Município | UF | Clientes |
|---|---|:-:|---:|
| 3543402 | Ribeirão Preto | SP | 466 |
| 3555000 | Tupã | SP | 328 |
| 3503208 | Araraquara | SP | 315 |
| 3505906 | Batatais | SP | 293 |
| 3521903 | Itajobi | SP | 286 |
| 3525706 | José Bonifácio | SP | 227 |
| 3509403 | Cajuru | SP | 191 |
| 3521309 | Ipuã | SP | 175 |
| 3557105 | Votuporanga | SP | 166 |
| 3536307 | Patrocínio Paulista | SP | 161 |

Todos os municípios com divergência são de **São Paulo**.

## 5. Impacto no CRM

| Onde | Impacto hoje |
|---|---|
| **Indicadores Geográficos** | **desde 21/09/2026 (issue 107) não mostra nem compara as planilhas**: quem atende o município é o responsável da carteira do CRM. As afirmações continuam gravadas para a conciliação (documento 32, §4.3) |
| **Cobertura por Filial e Carteira** | **não usa** estas planilhas: usa `CarteiraMunicipio` (o que veio do Vórtice). Não quebra |
| **Atribuição de trabalho** | 6.988 clientes estão em municípios cujo CEN não é confiável em nenhuma das duas fontes |
| **Modelo alvo** | a fonte canônica de "quem atende o município" passa a ser `CarteiraMunicipio` → `Carteira.ResponsavelId` (documento 40, seção 6.4). A conciliação precisa acontecer antes de `ResponsavelPeloMunicipio` sair |

**Enquanto não houver decisão comercial, nada muda:** a tabela de afirmações continua, a tela continua
mostrando as duas fontes, e a fase 9 do plano executivo fica bloqueada nesta parte.

## 6. O que o comercial precisa decidir

1. **Qual fonte vale, município a município** — ou um terceiro nome, quando nenhuma das duas estiver
   certa.
2. **As 19 vagas "a contratar"** da fonte A: quem responde por esses municípios hoje?
3. **Os 92 nomes que a carga não identificou**: são pessoas que não têm conta no CRM, saíram da
   empresa, ou é grafia diferente?
4. **O gestor** (fonte única): confirma os 203, inclusive os 81 não identificados?
5. **A vigência**: a partir de quando a atribuição confirmada vale? Nenhuma planilha declara data.

**Formato de devolução sugerido:** o mesmo CSV, com duas colunas a mais — `CenConfirmado` e
`ObservacaoDoComercial` — para que a conciliação da fase 9 seja uma leitura, e não uma interpretação.

## 7. Limites deste relatório

- Os dados são os **anteriores à sanitização**, preservados no banco de arquivo; o banco operacional
  está vazio.
- Os nomes ficam fora do repositório (dado pessoal); o que está aqui são contagens e municípios.
- A comparação é entre as duas planilhas recebidas em 13/09/2026. Se o comercial tiver uma terceira
  fonte mais recente, ela não está considerada.
- Municípios fora da ADR (35 linhas da área de atuação) não têm CEN declarado em nenhuma fonte.
