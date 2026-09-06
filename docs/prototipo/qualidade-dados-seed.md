# Qualidade dos dados da semente do protótipo

> Gerado por `scripts/dados/validar-seed.mjs` em 2026-09-03, contra o catálogo de saneamento de `docs/projeto/16-HIGIENIZACAO-DE-DADOS.md`. Este documento é gerado — não editar à mão. **O script só relata; nada nos 45 JSONs foi alterado.**

Arquivos analisados: **45**. Achados: **56**.

> **Leitura do achado dominante.** as 55 ocorrências de CNPJ encontradas nos arquivos de cliente (`cliente-84391.json`, `clientes-extra.json`, `clientes-outras.json`, `oportunidade-1517613.json`) falham no dígito verificador — 100% delas nesta rodada. Isto não é sinal de bug no validador: são CNPJs inventados à mão para o protótipo visual, plausíveis para o olho (14 dígitos, máscara correta) mas nunca calculados. **É exatamente o argumento da frente de saneamento**: se este arquivo fosse promovido a carga real sem passar pelo tipo `CpfCnpj`, o banco novo nasceria com o mesmo defeito medido no Vórtice (116 CPFs repetidos por falta de validação na entrada — documento 01, achado 9.4). O documento 16, seção 8, trata isto como regra de migração: nenhum dado de protótipo vira massa de teste sem passar pelo mesmo pipeline de saneamento do dado real.

## Por arquivo

| Arquivo | Achados |
|---|---:|
| `agenda.json` | 0 |
| `alertas.json` | 0 |
| `carteira-cen.json` | 0 |
| `catalogo-modelos.json` | 0 |
| `cens.json` | 0 |
| `cidades-latlng.json` | 0 |
| `cliente-84391.json` | 2 |
| `clientes-extra.json` | 24 |
| `clientes-outras.json` | 29 |
| `cobertura.json` | 0 |
| `config-categorias-interacao-cfg.json` | 0 |
| `config-categorias-interacao.json` | 0 |
| `config-integracoes.json` | 0 |
| `config-metas.json` | 0 |
| `config-motivos-perda.json` | 0 |
| `config-roles.json` | 0 |
| `config-usuarios.json` | 0 |
| `constantes-escalares.json` | 0 |
| `equipamento-1RW7250PVMR123456.json` | 0 |
| `estado-carteira.json` | 0 |
| `estado-clientes.json` | 0 |
| `estado-config.json` | 0 |
| `estado-nova-oportunidade.json` | 0 |
| `estado-performance.json` | 0 |
| `estado-pipeline.json` | 0 |
| `faturamento-12m.json` | 0 |
| `funil.json` | 0 |
| `fytd-meses.json` | 0 |
| `linha-icon.json` | 0 |
| `manifesto.json` | 0 |
| `mercado-pracas.json` | 0 |
| `meta-frequencia.json` | 0 |
| `mix-linhas.json` | 0 |
| `oportunidade-1517613.json` | 1 |
| `perfis-360.json` | 0 |
| `performance-cens.json` | 0 |
| `performance-series.json` | 0 |
| `pipeline-fases.json` | 0 |
| `pipeline.json` | 0 |
| `pos-vendas-cliente-84391.json` | 0 |
| `routes.json` | 0 |
| `status-cobertura-labels.json` | 0 |
| `tipo-meta.json` | 0 |
| `top-clientes.json` | 0 |
| `vendas-perdidas-motivos.json` | 0 |

## Por regra

| Regra | Achados | O que verifica |
|---|---:|---|
| `documento` | 55 | CNPJ/CPF com dígito verificador inválido ou tamanho errado |
| `dinheiro` | 1 | Valor monetário com mais de duas casas decimais, ou fora do formato R$ |

## Duplicata de chave natural — CNPJ/CPF em mais de um registro

Chave natural do cliente é o CNPJ/CPF (documento 16, seção 4). O mesmo documento sob ids ou arquivos diferentes é uma duplicata real, ou uma inconsistência entre a semente e o protótipo que a originou — os dois casos abaixo são exatamente isso, e cada um é achado real medido nesta rodada, não hipotético.

- **11234567000189**
  - `cliente-84391.json` em `$.cnpj` = `'11.234.567/0001-89'`
  - `clientes-extra.json` em `$.84430.cnpj` = `'11.234.567/0001-89'`

## Chassi repetido com modelo divergente

Chassi repetir entre `cliente-*.json`, `equipamento-*.json` e `pos-vendas-*.json` é **esperado por desenho** (as três fichas descrevem o mesmo bem). Só vira achado quando o "modelo" associado diverge entre as ocorrências.

Nenhuma inconsistência encontrada.

## Todos os achados, por arquivo

### `cliente-84391.json` (2)

| Caminho | Regra | Valor | Motivo |
|---|---|---|---|
| `$.cnpj` | `documento` | `11.234.567/0001-89` | CNPJ com dígito verificador inválido |
| `$.segmentacao.faturamento_declarado` | `dinheiro` | `R$ 80 a 120 milhões / ano` | valor monetário em texto fora do formato "R$ 0.000,00": 'R$ 80 a 120 milhões / ano' |

### `clientes-extra.json` (24)

| Caminho | Regra | Valor | Motivo |
|---|---|---|---|
| `$.84391.cnpj` | `documento` | `18.245.339/0001-42` | CNPJ com dígito verificador inválido |
| `$.84402.cnpj` | `documento` | `14.567.821/0001-08` | CNPJ com dígito verificador inválido |
| `$.84418.cnpj` | `documento` | `22.851.774/0001-91` | CNPJ com dígito verificador inválido |
| `$.84425.cnpj` | `documento` | `09.472.318/0001-55` | CNPJ com dígito verificador inválido |
| `$.84430.cnpj` | `documento` | `11.234.567/0001-89` | CNPJ com dígito verificador inválido |
| `$.84437.cnpj` | `documento` | `27.881.443/0001-17` | CNPJ com dígito verificador inválido |
| `$.84445.cnpj` | `documento` | `15.442.998/0001-33` | CNPJ com dígito verificador inválido |
| `$.84448.cnpj` | `documento` | `19.556.101/0001-70` | CNPJ com dígito verificador inválido |
| `$.84452.cnpj` | `documento` | `31.775.802/0001-64` | CNPJ com dígito verificador inválido |
| `$.84458.cnpj` | `documento` | `08.123.456/0001-27` | CNPJ com dígito verificador inválido |
| `$.84461.cnpj` | `documento` | `02.994.181/0001-88` | CNPJ com dígito verificador inválido |
| `$.84467.cnpj` | `documento` | `17.663.220/0001-45` | CNPJ com dígito verificador inválido |
| `$.84472.cnpj` | `documento` | `25.114.789/0001-52` | CNPJ com dígito verificador inválido |
| `$.84478.cnpj` | `documento` | `13.885.223/0001-04` | CNPJ com dígito verificador inválido |
| `$.84483.cnpj` | `documento` | `29.447.116/0001-89` | CNPJ com dígito verificador inválido |
| `$.84489.cnpj` | `documento` | `00.732.981/0001-72` | CNPJ com dígito verificador inválido |
| `$.84495.cnpj` | `documento` | `16.994.552/0001-38` | CNPJ com dígito verificador inválido |
| `$.84501.cnpj` | `documento` | `21.336.774/0001-16` | CNPJ com dígito verificador inválido |
| `$.84508.cnpj` | `documento` | `32.114.855/0001-93` | CNPJ com dígito verificador inválido |
| `$.84512.cnpj` | `documento` | `18.223.664/0001-15` | CNPJ com dígito verificador inválido |
| `$.84518.cnpj` | `documento` | `04.556.882/0001-49` | CNPJ com dígito verificador inválido |
| `$.84523.cnpj` | `documento` | `28.771.033/0001-61` | CNPJ com dígito verificador inválido |
| `$.84528.cnpj` | `documento` | `10.884.552/0001-77` | CNPJ com dígito verificador inválido |
| `$.84532.cnpj` | `documento` | `23.667.114/0001-08` | CNPJ com dígito verificador inválido |

### `clientes-outras.json` (29)

| Caminho | Regra | Valor | Motivo |
|---|---|---|---|
| `$[0].cnpj` | `documento` | `19.884.556/0001-02` | CNPJ com dígito verificador inválido |
| `$[1].cnpj` | `documento` | `15.223.998/0001-11` | CNPJ com dígito verificador inválido |
| `$[2].cnpj` | `documento` | `08.774.223/0001-56` | CNPJ com dígito verificador inválido |
| `$[3].cnpj` | `documento` | `30.114.665/0001-83` | CNPJ com dígito verificador inválido |
| `$[4].cnpj` | `documento` | `11.556.889/0001-40` | CNPJ com dígito verificador inválido |
| `$[5].cnpj` | `documento` | `18.774.663/0001-27` | CNPJ com dígito verificador inválido |
| `$[6].cnpj` | `documento` | `02.669.335/0001-91` | CNPJ com dígito verificador inválido |
| `$[7].cnpj` | `documento` | `25.115.884/0001-09` | CNPJ com dígito verificador inválido |
| `$[8].cnpj` | `documento` | `13.884.552/0001-16` | CNPJ com dígito verificador inválido |
| `$[9].cnpj` | `documento` | `16.223.998/0001-72` | CNPJ com dígito verificador inválido |
| `$[10].cnpj` | `documento` | `09.445.881/0001-25` | CNPJ com dígito verificador inválido |
| `$[11].cnpj` | `documento` | `27.116.884/0001-58` | CNPJ com dígito verificador inválido |
| `$[12].cnpj` | `documento` | `11.667.229/0001-40` | CNPJ com dígito verificador inválido |
| `$[13].cnpj` | `documento` | `19.882.114/0001-93` | CNPJ com dígito verificador inválido |
| `$[14].cnpj` | `documento` | `05.114.556/0001-07` | CNPJ com dígito verificador inválido |
| `$[15].cnpj` | `documento` | `20.884.556/0001-49` | CNPJ com dígito verificador inválido |
| `$[16].cnpj` | `documento` | `14.223.667/0001-83` | CNPJ com dígito verificador inválido |
| `$[17].cnpj` | `documento` | `22.114.885/0001-56` | CNPJ com dígito verificador inválido |
| `$[18].cnpj` | `documento` | `07.556.229/0001-70` | CNPJ com dígito verificador inválido |
| `$[19].cnpj` | `documento` | `31.664.225/0001-88` | CNPJ com dígito verificador inválido |
| `$[20].cnpj` | `documento` | `18.881.116/0001-27` | CNPJ com dígito verificador inválido |
| `$[21].cnpj` | `documento` | `15.442.885/0001-16` | CNPJ com dígito verificador inválido |
| `$[22].cnpj` | `documento` | `27.115.882/0001-93` | CNPJ com dígito verificador inválido |
| `$[23].cnpj` | `documento` | `11.885.663/0001-05` | CNPJ com dígito verificador inválido |
| `$[24].cnpj` | `documento` | `05.114.667/0001-30` | CNPJ com dígito verificador inválido |
| `$[25].cnpj` | `documento` | `19.223.554/0001-14` | CNPJ com dígito verificador inválido |
| `$[26].cnpj` | `documento` | `22.881.664/0001-79` | CNPJ com dígito verificador inválido |
| `$[27].cnpj` | `documento` | `30.114.885/0001-62` | CNPJ com dígito verificador inválido |
| `$[28].cnpj` | `documento` | `16.885.221/0001-97` | CNPJ com dígito verificador inválido |

### `oportunidade-1517613.json` (1)

| Caminho | Regra | Valor | Motivo |
|---|---|---|---|
| `$.cliente.cnpj` | `documento` | `12.345.678/0001-90` | CNPJ com dígito verificador inválido |

## Metodologia e limitações

- O campo `numero` só é tratado como telefone dentro de um array `telefones[]` — em todo outro contexto ele é ambíguo demais (número de pedido, "S/N" de endereço) para validar sem falso positivo.
- A regra de texto livre roda em **toda** string-folha do JSON, mesmo campos que já têm regra própria (ex.: um e-mail com espaço duplo aparece nas duas regras).
- Datas no formato `AAAA-MM` (só ano e mês, usado em `historico_horas`) são aceitas e checadas só pelo ano.
- Inscrição estadual, endereço em texto livre e placa não têm regra própria neste script porque não têm um formato único nacional verificável sem tabela de UF por UF (documento 16, catálogo, item 2).
