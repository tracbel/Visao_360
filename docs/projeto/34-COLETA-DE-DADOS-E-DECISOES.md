# Coleta de dados e decisões pendentes — planilhas-modelo e lista para o gerente

> **Documento 34** · Versão 1.0 · 13/09/2026 — materiais para coletar o que falta ao território e à
> Visão 360, e a lista priorizada de decisões comerciais. Base: documento 32, versão 1.2.
> Convenção: **[medido]** = contado em 13/09/2026 no banco de desenvolvimento e na extração do Vórtice.
>
> **Este documento não tem nome de pessoa nem dado de cliente.** As planilhas **pré-preenchidas** têm
> (login, nome, chassi): ficam em `dados-locais/coleta/`, que o Git ignora, e só circulam por canal
> interno.

---

## 0. Resumo

1. **Antes de pedir, conferimos o que as fontes já têm** (§1). Nada do que existe é pedido de novo:
   vai pré-preenchido, para confirmar ou corrigir.
2. **11 planilhas-modelo** (§2), cada uma com quatro abas — *Instruções*, *Dados*, *Dicionário* e
   *Listas* — e exemplos **fictícios**. **7** também saem numa versão pré-preenchida.
3. **17 decisões em ordem de prioridade** (§3), cada uma com pergunta objetiva, opções, impacto e o
   que fica bloqueado enquanto não houver resposta. É a lista para encaminhar ao gerente.

---

## 1. O que já existe e não será pedido de novo [medido]

| Dado | Onde está | Quantidade | Como entra na coleta |
|---|---|---|---|
| Filiais e situação | `organizacao.Empresa` | 16 filiais `0101NN` (13 ativas); o cadastro tem 18 empresas | lista de filiais em todas as planilhas |
| Linhas de negócio e cadência de visita | `organizacao.LinhaDeNegocio` | 14; cadência declarada em 4 | decisão 3 |
| Usuários com login, nome e filial de casa | `seguranca.Usuario` (pessoas ativas) | 263 | modelo 01, pré-preenchido |
| Supervisor e gerente de cada operador | extração do Vórtice (`IV_Operador`, texto livre) | 107 supervisores e 135 gerentes casam com um login do CRM | modelo 01, como **sugestão** a confirmar |
| Municípios da ADR, região e loja | `MunicipioDaAreaDeAtuacao` | 203 (Norte 83 · Noroeste 120) | modelo 02, pré-preenchido |
| CEN segundo cada planilha, e o gestor | `ResponsavelPeloMunicipio` | 203 municípios: 75 mesmo nome · 46 provável mesma pessoa · 82 nomes diferentes; gestor em 203 | modelo 02, com a comparação |
| Modelos de máquina | `frota.Modelo` | 253 (233 com máquina no parque) | modelo 07, pré-preenchido |
| Parque: chassi, modelo e filial | `frota.Equipamento` | 3.888, em 11 filiais; ano e horímetro vazios em todos | modelo 06, pré-preenchido |
| Tipos de tarefa usados em 2026 | `processo.Interacao` × `TipoTarefa` | 169 tipos, 122.002 interações; 2 tipos com "visita" no nome | modelo 10, pré-preenchido |
| Área plantada por cultura na ADR | `AreaPlantadaNoMunicipio` (PAM/IBGE 2024) | as 15 culturas de maior área | modelo 08, pré-preenchido |
| Regra de potencial | `RegraDePotencial` | 1 (café, **a confirmar**) | modelo 08 |
| Faturamento por cliente, filial e mês, com máquina/peça/serviço/outros | `FaturamentoDoCliente` | 12 meses conciliados (documento 32, §8.6) | modelo 11; decisões 5, 6 e 11 |
| Vendas perdidas com concorrente | `processo.VendaPerdida` | 165 (163 com modelo, 156 com preço) | decisão 16 |

**O que nenhuma fonte acessível tem:** metas (0 linhas no CRM, 1 no Vórtice), classificação SAM,
KAM e Varejo por cliente, propriedade, área e cultura por cliente (0 de 19.657 endereços), ano e
horímetro das máquinas, taxonomia de produto e ciclo de troca. É isso que as planilhas pedem.

---

## 2. As planilhas

Nome do arquivo = número + nome abaixo, em `dados-locais/coleta/modelos/` (só exemplo fictício) e
`dados-locais/coleta/pre-preenchidas/` (com o dado real que já existe).

| Nº | Planilha | Para quê | Vai pré-preenchida com | Quem responde | Desbloqueia (documento 32) |
|---|---|---|---|---|---|
| 01 | `usuarios-cargos-e-gestores` | visões por cargo e quem recebe a visão da empresa | 263 logins, nome e filial; sugestão de supervisor e gerente | RH + gerente comercial | O-31 (distribuição), O-35 |
| 02 | `municipio-cen-e-gestor-vigentes` | CEN e gestor **vigentes**, com data | 203 municípios, o que cada planilha diz e a comparação | gerente comercial | O-07, O-21 (CEN e gestor) |
| 03 | `classificacao-sam-kam-varejo` | filtro de tipo de cliente | — | comercial | O-19 |
| 04 | `metas` | previsão FY 2026 | — | comercial + controladoria | O-11 |
| 05 | `propriedades-e-culturas` | potencial de cliente e de não cliente | — (preferir a exportação do ART) | ART / comercial | O-01, O-22, O-23, O-25 |
| 06 | `parque-de-maquinas` | ciclo de troca e potencial ajustado | 3.888 chassis com modelo e filial | ART / pós-venda | O-02, O-26 |
| 07 | `taxonomia-de-produto` | filtros de produto e modelo | 253 modelos | comercial | O-20 |
| 08 | `regras-de-potencial` | mapa de potencial | 15 culturas com a área na ADR e a regra do café | comercial | O-17, O-24 |
| 09 | `ciclo-de-troca` | potencial ajustado por idade do parque | — | comercial | O-26 |
| 10 | `definicao-de-visita` | mapa de pendência de visita | 169 tipos de tarefa com uso e coordenada | gerente comercial | O-13, O-15 |
| 11 | `composicao-do-pos-venda` | valor de pós-venda | 10 componentes propostos para marcar | diretoria | O-18 |

**Como cada planilha é organizada:**

- **Instruções:** para que serve, o que já existe e como preencher.
- **Dados:** cabeçalho com os campos propostos. Linha **amarela** = exemplo fictício (apagar antes de
  devolver); célula **azul** = pré-preenchida, para confirmar ou corrigir; colunas "A INFORMAR" e
  "A DECIDIR" são o que se pede.
- **Dicionário:** campo, obrigatório, formato, exemplo fictício e origem.
- **Listas:** os valores aceitos; a célula da aba *Dados* só aceita valor da lista (nada de texto
  solto onde há catálogo).

**Convenções de preenchimento:** pessoa identificada pelo **login corporativo**, nunca pelo nome;
CPF/CNPJ só com dígitos; data `dd/mm/aaaa`; mês `aaaa-mm`; município pelo código IBGE de 7 dígitos;
filial pelo código `0101NN`; área em hectares.

---

## 3. Decisões para o gerente, em ordem de prioridade

| # | Prioridade | Pergunta | Opções | Impacto | Fica bloqueado | O que o sistema já tem | Planilha | Doc. 32 |
|---:|---|---|---|---|---|---|---|---|
| 1 | Alta | Qual das duas planilhas de CEN vale hoje, e desde quando? | a) Área de Atuação · b) CEN e Gestor por Município · c) uma planilha nova e datada | define CEN e gestor dos 203 municípios; hoje 82 têm CEN diferente e 46 grafia diferente | filtro por CEN e gestor; responsável no detalhe; visão do CEN | as duas planilhas lado a lado, com a comparação | 02 | P-1 |
| 2 | Alta | O que conta como visita para a cobertura da carteira? | a) só os 2 tipos de tarefa de visita (47 interações em 2026) · b) interação com coordenada (29,5 mil de 122 mil) · c) resultados marcados como externos no Vórtice (22) · d) qualquer interação (regra provisória de hoje) | muda numerador e denominador da pendência de visita | validação comercial do mapa A; meta de visita | tipos de tarefa com uso e coordenada | 10 | P-2 |
| 3 | Alta | Qual periodicidade de visita vale, e ela se mede por cliente ou por vínculo com a carteira? | a) a declarada no CRM (máquinas 180/180/180/360 dias; prospecção 120/120/120/180; peças e AMS 360) · b) 30/60/90/120 da maquete · c) o ciclo por potencial do Vórtice | define quem está fora da cadência | mapa A; alerta de visita vencida | cadência por linha e classe | 10 | P-3 |
| 4 | Alta | Quem recebe a visão da empresa inteira, e quais são os cargos? | a) só diretoria · b) diretoria e gerência regional · c) outra regra | libera a visão consolidada para usuários reais | visão da empresa (hoje só perfil de teste); visões por cargo | 263 usuários; sugestão de gestor do Vórtice | 01 | P-10 |
| 5 | Alta | O mapa de vendas usa o ano fiscal até a data (FYTD) ou os 12 meses fechados? Se FYTD, quando começa o ano fiscal? | a) 12 meses fechados (hoje) · b) FYTD, com a data de início | muda o período de todos os valores de venda | leitura oficial do mapa B | o filtro de período já existe | — | P-4 |
| 6 | Alta | Devolução e cancelamento devem ser abatidos das vendas? | a) sim, pela nota de entrada do Protheus · b) não | reduz vendas e pós-venda | valor líquido comercial | notas de saída lidas; a de entrada não | — | P-5 |
| 7 | Alta | A regra "1 trator 3036N a cada 10 ha de café" vale? Quais são as regras das outras culturas? | confirmar ou corrigir o café; informar cana, soja, laranja, amendoim e milho | a estimativa regional vira potencial com regra aprovada | mapa C confirmado; potencial em valor | área plantada do IBGE por município e cultura | 08 | P-8 |
| 8 | Média | O que a diretoria considera pós-venda? | marcar os componentes | muda o valor de pós-venda | pós-venda com composição aprovada | peça e serviço já separados | 11 | §5, grupo 11 |
| 9 | Média | Como se define SAM, KAM e Varejo, e quais clientes são de cada tipo? | critério e lista | liga o filtro de tipo de cliente | filtro SAM/KAM/Varejo | nenhuma classificação nas fontes | 03 | P-6 |
| 10 | Média | Como o comercial agrupa os produtos (linha, série, porte)? | classificar os modelos | liga os filtros de produto e modelo (com o item da nota) | filtros de produto e modelo | 253 modelos | 07 | P-7 |
| 11 | Média | A quem pertence o cliente que compra em mais de uma filial? | a) à filial do cadastro (hoje) · b) à que mais vende para ele · c) às duas, com divisão | 1.706 clientes; R$ 321,6 mi em 12 meses com filial de cadastro diferente da que vendeu | metas e carteira por filial | conciliação por filial pronta | — | P-16 |
| 12 | Média | Guaíra, Ituverava e Monte Alto operam? | a) sim — reativar no CRM · b) não — definir quem atende os municípios | 11 municípios da ADR; as três emitiram R$ 17,4 mi com cliente e R$ 4,7 mi sem cliente em 12 meses | visão de filial dessas lojas | o faturamento delas já é lido | — | P-14 |
| 13 | Média | O que são os 35 municípios fora da ADR na planilha de área de atuação? | a) prospecção · b) histórico · c) erro — remover | define se entram em algum mapa | tratamento desses municípios | carregados, marcados como fora da ADR | — | C-5 |
| 14 | Média | Quem cadastra as contrapartes que compram e não têm cliente no CRM? | responsável e prazo | R$ 230,7 mi em 12 meses (13 filiais ativas) ficam fora dos municípios | vendas por município completas | a lista por documento já existe | — | P-17 |
| 15 | Baixa | Quem confere os 2 endereços cuja coordenada contradiz o município? | responsável do cadastro | 2 clientes fora do mapa | — | os dois na fila de revisão, com o motivo | — | P-15 |
| 16 | Baixa | Qual o ciclo de troca por categoria, e há fonte de parque concorrente além das vendas perdidas? | informar no modelo | potencial ajustado por idade do parque | potencial ajustado | 165 vendas perdidas com concorrente | 09 | P-9 |
| 17 | Baixa | De onde vêm as metas: ART ou arquivo? | a) ART · b) arquivo mensal | previsão FY 2026 | cartão de previsão | metas vazias no CRM e no Vórtice | 04 | O-11 |

A mesma lista está em `dados-locais/coleta/modelos/00-decisoes-para-o-gerente.xlsx`, pronta para
encaminhar.

**Decisões que não são do gerente** e continuam com o Ricardo: aprovar a limpeza do histórico do
GitHub (documento 33), acesso ao ART a partir da rede do CRM (P-11) e comunicar ao encarregado de
dados a exposição de dado pessoal no histórico (P-18).

---

## 4. Como gerar de novo

```powershell
# 1. Exportar o que já existe (só leitura; a saída vai para dados-locais/coleta/entrada)
$env:ConnectionStrings__Crm = '<cadeia de conexão do CRM>'   # nunca versionada
./scripts/coleta/extrair-pre-preenchimento.ps1

# 2. Gerar os modelos (exemplo fictício) e as versões pré-preenchidas
python scripts/coleta/gerar-modelos-de-coleta.py
```

Sem a etapa 1, o gerador produz só os modelos com exemplo fictício. As duas etapas são
reexecutáveis e sobrescrevem a saída.

---

## 5. Limites e cuidados

- **A sugestão de supervisor e gerente é só sugestão.** Vem de texto livre do Vórtice casado pelo
  nome normalizado com o login do CRM; homônimo e cadastro antigo são possíveis.
- **A planilha 05 não substitui a exportação do ART.** Digitar propriedade por propriedade é lento e
  sujeito a erro; ela serve de contrato de formato para a exportação.
- **As planilhas pré-preenchidas têm dado pessoal e da empresa.** Circulam só por canal interno; não
  vão para e-mail externo, repositório ou ferramenta pública.
- **A importação das planilhas devolvidas ainda não existe** (documento 32, O-47). Ela entra depois
  que o gerente validar o formato, com a mesma regra das cargas: origem, data, quem informou,
  reexecução sem duplicar e nada de fusão automática.
