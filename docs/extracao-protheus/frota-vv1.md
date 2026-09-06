# Frota no Protheus — tabela VV1

> Extraido de `VV1 — Cadastro de Veiculos` em 06/09/2026 15:59, somente
> leitura, pela API de producao (`genericQuery`). **38.358 veiculos.**

## O que a VV1 e

E o cadastro de veiculo do modulo de concessionaria (SIGAVEI). Uma linha por chassi — nao por
modelo e nao por venda. Quem quiser venda usa a VV0/VVA (Saidas de Veiculos); quem quiser
cadastro de modelo usa a VV2.

**Nem toda linha da VV1 e maquina em poder do cliente.** A tabela guarda o chassi desde a
entrada, entao ela mistura estoque, vendido e devolvido. A separacao sai de `VV1_SITVEI` e
`VV1_STATUS` — que estao **vazios na amostra medida**, e por isso este relatorio nao os usa.
Confirmar com o negocio antes de tratar qualquer recorte como "frota em campo".
## Por filial

| Valor | Veiculos | % |
|---|---:|---:|
| 0101 | 38.358 | 100,0% |

## Por marca (VV1_CODMAR, nome da VVX)

| Valor | Veiculos | % |
|---|---:|---:|
| JD -  | 35.046 | 92,8% |
| TAT | 368 | 1,0% |
| MAC | 359 | 1,0% |
| MAR | 209 | 0,6% |
| AGR | 176 | 0,5% |
| BAL | 137 | 0,4% |
| (vazio) | 129 | 0,3% |
| SIL | 119 | 0,3% |
| KOM | 116 | 0,3% |
| DMB | 109 | 0,3% |
| ORI | 107 | 0,3% |
| KAM | 100 | 0,3% |
| BMD | 89 | 0,2% |
| BAC | 84 | 0,2% |
| VAL | 84 | 0,2% |
| NB | 79 | 0,2% |
| SAN | 78 | 0,2% |
| VIC | 78 | 0,2% |
| MSF | 57 | 0,2% |
| SJ | 46 | 0,1% |
| JOS | 44 | 0,1% |
| CRE | 42 | 0,1% |
| FAL | 37 | 0,1% |
| OXB | 33 | 0,1% |
| JZ | 31 | 0,1% |

## Por tipo de veiculo (VV1_TIPVEI, nome da VV8)

| Valor | Veiculos | % |
|---|---:|---:|
| 1 | 38.016 | 99,1% |
| (vazio) | 342 | 0,9% |

## Modelos mais frequentes (VV1_MODVEI)

| Valor | Veiculos | % |
|---|---:|---:|
| A CLASSIFICAR | 2.033 | 9,0% |
| TR 7230J | 1.744 | 7,7% |
| CH 570 - C/ ESTEIRA | 1.462 | 6,4% |
| TR 5078E | 1.428 | 6,3% |
| TR 5090E | 1.375 | 6,1% |
| TR 5080E | 1.348 | 5,9% |
| TR 6190J | 1.061 | 4,7% |
| TR 6180J | 952 | 4,2% |
| TR 6100J | 919 | 4,0% |
| CH 3520 ESTEIRA/15 | 907 | 4,0% |
| TR 5080EN | 882 | 3,9% |
| TR 7225J | 695 | 3,1% |
| CH 3520 | 645 | 2,8% |
| TR 7200J | 559 | 2,5% |
| TR 6115J | 528 | 2,3% |
| 2450HS | 497 | 2,2% |
| TR 5075E | 489 | 2,2% |
| TR 7M 230 | 476 | 2,1% |
| 2471HS | 460 | 2,0% |
| 2440HS | 456 | 2,0% |
| TR 5403 | 450 | 2,0% |
| TR 5085E | 414 | 1,8% |
| 2580HS | 406 | 1,8% |
| 2250HS | 399 | 1,8% |
| TR 6125J | 387 | 1,7% |
| TR 5070E | 367 | 1,6% |
| 2330HS | 364 | 1,6% |
| TR 5065E | 357 | 1,6% |
| 2001HS | 345 | 1,5% |
| 1040HS | 302 | 1,3% |
