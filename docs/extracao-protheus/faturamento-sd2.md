# Faturamento no Protheus — tabela SD2

> Extraido de `SD2 — Itens de Nota Fiscal de Saida` em 06/09/2026 16:02,
> somente leitura, pela API de producao. Recorte: emissao a partir de **20260101**.
> **54.158 itens**, **R$ 263.369.076,97**.

## A premissa que este numero derruba

O projeto operou meses com "o faturamento parou em 11/04/2025". A frase vinha de
`X_TOTVS_CRM_FATURAMENTO`, a tabela que o **Vortice recebe** do Protheus — e essa de fato para
naquela data. Na **origem**, nao para: ha nota emitida ate hoje.

**O que morreu foi a integracao Protheus -> Vortice, nao o faturamento.** Toda tela que hoje diz
"depende do Protheus, que esta parado" pode passar a mostrar numero atual, lendo daqui.

## Como maquina se separa de peca

`D2_GRUPO` viaja na propria linha da nota — nao precisa juntar com a SB1. O catalogo e a
`SBM`: `VEIC` e maquina, a faixa `1001..10xx` e peca por linha de produto, `SRV` e
servico, `MO_O` e mao de obra de oficina.
## Por mes

| Mes | Itens | Valor | % |
|---|---:|---:|---:|
| 2026-01 | 5.086 | R$ 27.610.623,80 | 10,5% |
| 2026-02 | 6.096 | R$ 26.693.011,21 | 10,1% |
| 2026-03 | 6.648 | R$ 43.458.471,99 | 16,5% |
| 2026-04 | 6.564 | R$ 44.207.000,03 | 16,8% |
| 2026-05 | 7.229 | R$ 43.671.403,72 | 16,6% |
| 2026-06 | 7.268 | R$ 23.552.423,53 | 8,9% |
| 2026-07 | 7.174 | R$ 25.896.829,82 | 9,8% |
| 2026-08 | 6.728 | R$ 25.968.130,23 | 9,9% |
| 2026-09 | 1.365 | R$ 2.311.182,64 | 0,9% |

## Por grupo de produto

| Grupo | Itens | Valor | % |
|---|---:|---:|---:|
| MAQUINAS | 690 | R$ 179.612.323,97 | 68,2% |
| PEÇAS COLHEDORAS DE CANA | 16.066 | R$ 20.949.863,64 | 8,0% |
| PEÇAS TRATORES | 24.427 | R$ 18.591.004,49 | 7,1% |
| COMISSOES VENDA DIRETA | 52 | R$ 18.317.551,15 | 7,0% |
| SERVICOS | 2.438 | R$ 3.584.725,18 | 1,4% |
| INCENTIVOS SOBRE VENDAS | 28 | R$ 2.839.925,60 | 1,1% |
| PEÇAS COLHEITADEIRA DE GRAOS | 3.740 | R$ 2.832.066,44 | 1,1% |
| BONUS PERFORMANCE | 2 | R$ 2.360.501,01 | 0,9% |
| PEÇAS LUBRIFICANTES | 990 | R$ 2.291.972,55 | 0,9% |
| OUTROS | 58 | R$ 1.932.179,04 | 0,7% |
| PEÇAS PULVERIZADOR | 1.290 | R$ 1.610.242,60 | 0,6% |
| ATIVACOES / LICENSAS | 118 | R$ 1.002.214,96 | 0,4% |
| REEMBOLSOS JOHN DEERE | 18 | R$ 923.625,21 | 0,4% |
| ATIV MAQUINAS E EQUIPAMENTOS | 6 | R$ 594.512,00 | 0,2% |
| PEÇAS COOL GARD | 234 | R$ 532.895,21 | 0,2% |
| TREINAMENTO | 80 | R$ 461.084,89 | 0,2% |
| PEÇAS METISA | 91 | R$ 452.060,64 | 0,2% |
| LOCACAO | 10 | R$ 417.301,34 | 0,2% |
| LIMPEZA DPM | 146 | R$ 361.622,59 | 0,1% |
| COMISSAO CONSORCIOS | 8 | R$ 351.424,35 | 0,1% |
| PNEUS AGRICOLAS OUTROS | 29 | R$ 345.840,00 | 0,1% |
| PEÇAS PLATAFORMAS | 756 | R$ 342.333,40 | 0,1% |
| COMISSAO SEGUROS | 44 | R$ 321.546,70 | 0,1% |
| PEÇAS AMS \ AUTEQ | 185 | R$ 300.017,50 | 0,1% |
| PEÇAS JD PRECISION UPGRADE | 13 | R$ 259.704,19 | 0,1% |

## Por filial

| Filial | Itens | Valor | % |
|---|---:|---:|---:|
| 010101 | 54.158 | R$ 263.369.076,97 | 100,0% |
