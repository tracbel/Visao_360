# 49A — Matriz indicador → fonte

> Anexo do [documento 49](49-INTELIGENCIA-DE-MERCADO.md) · 22/09/2026.
> **Uma linha por indicador que a Inteligência de Mercado mostra ou usa**, com a fonte oficial, o dataset, o
> campo, a frequência, a competência e a regra de cálculo. As colunas "No CRM" e "Situação" dizem o que já
> está no banco (**[M]** medido no código) e o que falta.
> **Convenções:** `m` município, `c` cultura, `k` categoria de máquina, `a` ano da PAM, `t` mês.
> "Região" = soma dos municípios da Região Tracbel; "SP" = linha **publicada** do estado, quando a fonte tem.

> Os códigos IM-xx citados na coluna "Situação" são as issues #150 a #171 — o mapa está no anexo 49B.

Situação: ✅ carregado e usado · 🟡 carregado, não usado ou incompleto · ⛔ não existe · 🔒 depende de decisão
ou acesso.

---

## 1. Território e estrutura agropecuária

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 1.1 | Municípios da Região Tracbel | Tracbel (decisão comercial) | hoje: planilha da área de atuação | `PertenceAAdr` | por decisão | vigência | contagem dos vigentes | `organizacao.MunicipioDaAreaDeAtuacao` | 🟡 vem de planilha (#107) |
| 1.2 | Loja responsável | Tracbel | idem | `EmpresaResponsavelId` | por decisão | vigência | — | idem | 🟡 idem; nível Loja da hierarquia (D-IM-07) |
| 1.3 | Área territorial (km²) | IBGE | SIDRA 4714 | v6318 | anual | ano da apuração | Região = Σ m; SP = publicado | `AreaTerritorialDoMunicipio` | ✅ |
| 1.4 | Tratores (total, < 100 cv, ≥ 100 cv) | IBGE — Censo Agropecuário | SIDRA 6871 | v1862, c12605 (113521 total, 113522, 113523) | decenal (2017; próximo 2028) | 2017 | Região = Σ m (sigilo = nulo, fora da soma, com a contagem de municípios divulgados); SP = **publicado (n3)** | `FrotaDeTratoresNoMunicipio` | 🟡 SP é soma, não publicado (IM-06) |
| 1.5 | Estabelecimentos com trator | idem | SIDRA 6871 | v1918 | decenal | 2017 | idem | idem | 🟡 idem |
| 1.6 | Densidade de tratores | derivado | — | — | — | 2017 × ano da área | tratores × 1.000 ÷ km² | calculado na leitura | ✅ (anos diferentes, dito) |
| 1.7 | Estabelecimentos por porte (8 faixas + sem área) | IBGE — Censo | SIDRA 6780 | v183, c220 (18 grupos + sem área + total) | decenal | 2017 | reagrupamento na leitura; faixa nula quando todos os grupos vieram sob sigilo | `EstabelecimentosPorAreaNoMunicipio` | 🟡 SP é soma (IM-06) |
| 1.8 | Rebanho bovino | IBGE — PPM | SIDRA 3939 | v105, c79 = 2670 | anual | ano | Região = Σ m; SP = publicado | `RebanhoNoMunicipio` | ✅ |
| 1.9 | Usinas de etanol e capacidade | ANP | dados abertos — produtores de etanol autorizados | CNPJ, município, capacidade de anidro e hidratado (m³/dia) | mensal | mês de referência | contagem e Σ capacidade; ausência ≠ "sem usina" (usina só de açúcar não aparece) | `UsinaDeEtanol` | 🟡 apaga a que sai; casa por nome (IM-04, IM-05) |

## 2. Produção agrícola

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 2.1 | Área plantada ou destinada à colheita | IBGE — PAM | SIDRA 5457 | v8331, c782 | anual (~set/out de a+1) | ano civil `a` | Região = Σ m; SP = n3 35; café só pelo "Total" (40139) | `ProducaoAgricolaNoMunicipio` / `…NoEstado` | ✅ 3 anos (IM-07) |
| 2.2 | Área colhida | idem | SIDRA 5457 | v216 | anual | `a` | idem | idem | ✅ |
| 2.3 | Quantidade produzida | idem | SIDRA 5457 | v214 (unidade `MN` por produto) | anual | `a` | por produto, **nunca somada entre culturas** | idem | 🟡 unidade não guardada (IM-03) |
| 2.4 | Valor da produção | idem | SIDRA 5457 | v215 (mil R$, nominal) | anual | `a` | Σ; nominal, sem deflator | idem | ✅ |
| 2.5 | Produtividade | derivado (IBGE) | SIDRA 5457 (conferência: v112, kg/ha) | — | anual | `a` | `Q(a) ÷ A_colhida(a)`; unidade comercial pelo fator do catálogo | — | ⛔ (C-01) |
| 2.6 | Relevância da Região em SP | derivado | — | — | anual | `a` (mesmo ano nos dois lados) | Região ÷ SP publicado, por medida e cultura | parcial (lavoura inteira) | 🟡 falta por cultura e quantidade |
| 2.7 | Produtividade Região ÷ SP | derivado | — | — | anual | `a` | `Y(Região) ÷ Y(SP)`, ambas sobre área colhida | — | ⛔ |
| 2.8 | Perfil por segmento | derivado | catálogo de culturas | `Cultura.Segmento` | anual | `a` | % da área plantada por segmento, por nível da hierarquia | — | ⛔ (IM-16) |
| 2.9 | Milho 1ª e 2ª safra | IBGE — PAM | SIDRA 839 (a conferir) | área, quantidade por safra | anual | `a` | separar a área de milho safrinha para o compartilhamento com a soja | — | ⛔ (IM-07, D-IM-01) |
| 2.10 | Série histórica | IBGE — PAM | SIDRA 5457 | as quatro | anual | 1974– | série sem sobrescrita, com trilha nas revisões | 3 anos | 🟡 (IM-07) |

## 3. Preços e câmbio

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 3.1 | Preço recebido pelo produtor (SP) | CONAB | `portaldeinformacoes.conab.gov.br/downloads/arquivos/PrecosMensalUF.txt` | produto, classificação, nível, UF, mês, R$/kg | mensal (janela de 12 meses) | mês | × fator comercial (saca 60 kg, caixa 40,8 kg, arroba…) | `CotacaoDeProduto` | ✅ desde 09/2025 |
| 3.2 | Preço do kg de ATR | Socicana | página "preço do kg" | mensal e acumulado da safra | mensal | mês / safra | — | `CotacaoDeProduto` (SOCICANA) | ✅ desde 2015/16 |
| 3.3 | Dólar PTAX | Banco Central | SGS 3698 | venda, média mensal | mensal | mês | — | `CotacaoDoDolar` | ✅ desde 01/2015 |
| 3.4 | Preço em US$ | derivado | — | — | mensal | mês | `p(t) ÷ PTAX(t)`; mês sem PTAX = sem dólar | calculado na leitura | ✅ |
| 3.5 | Preço atual | derivado | — | — | mensal | último mês **fechado** | último mês da série | tela | ✅ |
| 3.6 | Médias 3, 6, 12 meses, 3 e 5 anos | derivado | — | — | mensal | janela | média dos meses da janela; **faltou mês, indisponível** | — | ⛔ (#73) |
| 3.7 | Atual ÷ média | derivado | — | — | mensal | janela | `p(t) ÷ média` | — | ⛔ |
| 3.8 | Variação em 12 meses | derivado | — | — | mensal | t e t−12 | `p(t) ÷ p(t−12) − 1` | tela ("em 1 ano") | ✅ |
| 3.9 | Momento de preço 12 ÷ 12 | derivado | — | — | mensal | t−23..t | `média(t−11..t) ÷ média(t−23..t−12)`; exige 24 meses completos | — | ⛔ **09/2027** — nenhuma fonte aberta tem série mensal longa de preço recebido em SP (investigado em 49D) |
| 3.10 | Faixa de mercado | parâmetro | `ParametroDoPotencial` | limites 1,00 / 1,20 / 1,40 | por vigência | data do cálculo | < retração: retraído; ≤ aquecimento: (nome a decidir); ≤ superaquecimento: aquecido; acima: superaquecido | `ParametroDoPotencial.FaixaDe` | ✅ regra; ⛔ uso |
| 3.11 | Preço CEPEA | CEPEA | indicadores | — | diária/mensal | — | — | — | 🔒 **licença CC BY-NC 4.0** — o uso comercial exige contrato (D-P11, #117; medido em 49D) |
| 3.12 | **Preço implícito da PAM** | IBGE/PAM | SIDRA 5457 | `215 ÷ 214` | **anual** | ano da PAM | `ValorDaProducaoMilReais × 1000 ÷ QuantidadeProduzida`; é o preço médio recebido pelo produtor, ponderado pela colheita — **não se emenda** à série mensal da CONAB | `ProducaoAgricolaNoMunicipio` (desde 2010, IM-07) | 🟡 carregado, leitura derivada a fazer (49D §4) |

## 4. Custo e rentabilidade

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 4.1 | Custo operacional / ha e / unidade | CONAB | séries históricas `.xls` (abas de SP) | custo variável + fixo | anual | safra (e mês do relatório) | leitura por rótulo | `CustoDeProducao` | ✅ 166 abas |
| 4.2 | Custo total / ha e / unidade | idem | idem | operacional + renda de fatores | anual | safra | nulo quando a CONAB parou no operacional | idem | ✅ |
| 4.3 | Local e camada de referência por cultura | parâmetro | — | local CONAB, operacional ou total | por vigência | data do cálculo | escolha do administrador | — | ⛔ (IM-10, D-P07) |
| 4.4 | Receita / ha | derivado | IBGE + CONAB | — | anual | `a` (produtividade) + meses da safra (preço) | `Y_com(m,c,a) × preço médio da safra`; cana: `ATR (kg/t) da safra × preço do kg de ATR × t/ha` | — | ⛔ |
| 4.5 | Margem / ha | derivado | — | — | anual | safra | `receita/ha − custo/ha` (camada de referência); competências das três partes no tooltip | — | ⛔ |
| 4.6 | Margem total | derivado | — | — | anual | safra | `margem/ha × área colhida` | — | ⛔ (C-07) |
| 4.7 | Índice de rentabilidade | derivado | — | — | anual | safra | `(receita ÷ custo) da safra ÷ média das N safras anteriores` (N por parâmetro) | — | ⛔ (D-IM-04) |

## 5. Máquinas: preço e poder de compra

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 5.1 | Preço de referência da máquina | Tracbel (interno) | candidatas: item da nota do Protheus (SD2), vendas do ART, preço ofertado no formulário de venda perdida, tabela John Deere | preço por modelo | mensal | mês | **mediana** por modelo de referência e mês (nunca preço de uma negociação) | — | 🔒 #70 (#18) |
| 5.2 | Sacas necessárias | derivado | — | — | mensal | t | `preço da máquina(k,t) ÷ preço da unidade(c,t)` | — | ⛔ |
| 5.3 | Índice de poder de compra | derivado | — | — | mensal | t e base | `sacas(base) ÷ sacas(t)`; > 1 = mais poder de compra; base 1,00 (D-IM-02) | — | ⛔ (C-03) |
| 5.4 | Valor de mercado anual | derivado | — | — | anual | ano × mês do preço | `demanda anual × preço de referência` | — | ⛔ (D-P12) |

## 6. Crédito rural (SICOR)

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 6.1 | Linhas de investimento em máquinas, 12 m | Banco Central — SICOR | OData `InvestMunicipioProduto` | contagem de linhas dos produtos de máquina | contínua; relê ano corrente e anterior | mês de emissão | Σ linhas na janela; **linha ≠ contrato** | `CreditoRuralDeInvestimento` | 🟡 janela termina em mês aberto (IM-08) |
| 6.2 | Valor financiado, 12 m | idem | idem | `VlInvest` | idem | idem | Σ valor | idem | 🟡 idem |
| 6.3 | Ticket médio | derivado | — | — | mensal | janela | `valor ÷ linhas` | tela | ✅ |
| 6.4 | Variações 12 × 12 | derivado | — | — | mensal | janela | razão − 1 | tela | ✅ |
| 6.5 | Índice de crédito | derivado | — | — | mensal | janela fechada | `w_linhas × razão das linhas + w_valor × razão do valor`; suavização e limites por parâmetro | — | ⛔ (C-04, IM-17) |
| 6.6 | Crédito Região × SP | derivado | — | — | mensal | janela | Σ Região ÷ Σ SP (SP = todos os municípios do SICOR) | — | ⛔ (IM-08) |
| 6.7 | Produtos de máquina | parâmetro | `ItemDoSicor` (produto) | código do produto → categoria | por vigência | — | trator 7080, máquinas e implementos 4860, colheitadeiras 2700 (hoje fixo no código) | constante | 🟡 (IM-08, IM-16) |

## 7. Potencial, ciclo e cenários

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 7.1 | Parque necessário | derivado | PAM + parâmetros | área plantada; ha/máquina | anual | `a` + vigência | `área útil(m,k,grupo) ÷ ha por máquina(c,k)` | só uma regra | 🟡 (#72) |
| 7.2 | Demanda anual estrutural | derivado | idem | ciclo em anos | anual | `a` + vigência | `parque ÷ ciclo(c,k)` | — | ⛔ (#72) |
| 7.3 | Percepção comercial | Tracbel (gestor) | CRM | percentual, autor, justificativa | por vigência | data do cálculo | `π(m)` dentro do limite vigente | `PercepcaoDoGestor` | ✅ registro; ⛔ uso |
| 7.4 | Fator de mercado | derivado | — | — | mensal | data do cálculo | `trava[mín,máx](1 + w₁(I₁−1) + w₂(Crédito−1) + π)` — forma e pesos: D-P05 | — | ⛔ (#74) |
| 7.5 | Demanda ajustada | derivado | — | — | mensal | idem | `demanda estrutural × fator` | — | ⛔ |
| 7.6 | Cenários | derivado | — | — | mensal | idem | conservador / moderado / otimista pelas bandas de sensibilidade (D-IM-05) | — | ⛔ |
| 7.7 | Calculadora | derivado | — | — | sob demanda | parâmetros de hoje | o mesmo motor sobre área, cultura, categoria e cenário informados | — | ⛔ (IM-12) |

## 8. Vendas, share e oportunidade

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 8.1 | Vendas Tracbel em R$ | Protheus | notas de saída | valor líquido, máquina, peça, serviço | mensal | mês | pelo endereço principal do cliente | `comercial.FaturamentoDoCliente` | ✅ (sem modelo) |
| 8.2 | Vendas Tracbel em unidades, por modelo | ART / Protheus | vendas de máquina; item da nota | quantidade, modelo, data de entrega | mensal | mês | por município do comprador, ano civil | `frota.VendaDeMaquina` | 🔒 ART desligado; #69 |
| 8.3 | Captura | derivado | — | — | anual | ano civil | `vendas (un) ÷ demanda anual (un)` — nunca "market share" | — | ⛔ (IM-13) |
| 8.4 | Potencial capturável | derivado | — | — | anual | ano | `max(0, demanda ajustada − vendas)` | — | ⛔ |
| 8.5 | Share do crédito | derivado | SICOR + API GN | venda financiada identificada | mensal | janela | valor financiado de máquinas vendidas pela Tracbel ÷ valor do SICOR no município | — | 🔒 #12 |
| 8.6 | Venda perdida registrada | Tracbel | formulário | concorrente, modelo, quantidade, preços | contínua | data da ocorrência | contagem por município do cliente | `processo.VendaPerdida` | ✅ registro; ⛔ no mapa |
| 8.7 | Oportunidade inferida | derivado | — | — | mensal | janela | indício com confiança (§9.5 do doc 49); nunca vira venda perdida sozinha | — | ⛔ (IM-13, D-IM-09) |

## 9. Cliente, renovação e cobertura

| # | Indicador | Fonte oficial | Dataset / API | Campo | Frequência | Competência | Regra de cálculo | No CRM | Situação |
|---|---|---|---|---|---|---|---|---|---|
| 9.1 | Área do cliente por cultura | a decidir: cadastro pelo CEN, ART (propriedades), CAR/SICAR | — | hectares, cultura | contínua | data do cadastro | — | `comercial.Endereco.Hectares` / `CulturaId` | ⛔ 0% preenchido (D-P13) |
| 9.2 | Potencial do cliente | derivado | — | — | como o município | idem | o mesmo motor sobre a área do cliente | — | ⛔ (#79) |
| 9.3 | Clientes por faixa de porte × estabelecimentos | derivado | Censo + CRM | — | — | 2017 × hoje | distribuição por faixa; estabelecimento ≠ cliente ≠ imóvel | — | ⛔ (#79) |
| 9.4 | Idade do chassi | CRM / ART | cadastro do equipamento | ano de fabricação, ano do modelo, data da venda | contínua | ano corrente | `ano corrente − ano de fabricação` (ou da venda) | `frota.Equipamento` | 🟡 preenchimento a medir (IM-01) |
| 9.5 | Situação de renovação | derivado | — | ciclo da categoria | anual | ano corrente | Dentro do ciclo / Entrando na janela / Renovação provável / Acima do ciclo (D-IM-10) | — | ⛔ (IM-15) |
| 9.6 | Cobertura de carteira | CRM | vínculos e interações | cadência da linha | contínua | data da consulta | já calculado | `comercial.ClienteCarteira` | ✅ |
| 9.7 | Cobertura × potencial | derivado | — | — | mensal | idem | pendentes ponderados pela demanda do município | — | ⛔ (#76) |
