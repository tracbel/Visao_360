# 53 — API Gestão de Negócios: contrato, o que o CRM consome e as perguntas à Inteligência de Mercado

> Issue #12. Atualiza o §5 do documento 46 (lido em 16/09/2026) com o que foi medido e implementado de 27 a 28/09/2026.
> Onde este documento e o §5 do 46 divergem, vale este. Nenhum valor pessoal está aqui: só nomes de rota, de campo,
> contagens e datas. A chave nunca é citada: está no `.env` como `GESTAO_NEGOCIOS_API_TOKEN` e no servidor pela tela de
> Integrações.

## 1. O que a API é, hoje

- **Endereço pelo nome:** `https://negocios-agro.tracbel.com.br:5001` (D-M1). O certificado é o curinga DigiCert da
  Tracbel, validado pela cadeia pública e pelo nome. **Não se desliga a validação**, e o IP não é usado. Isso resolve o
  GN-R2 e a pergunta 10 do §5.9 do documento 46.
- **Autenticação:** `Authorization: Bearer <chave>`, uma chave para a API inteira. Sem a chave aceita, a API redireciona
  para `/entrar`. Por isso o cliente do CRM **não segue redirecionamento**: redirecionamento é recusa.
- **Só GET.** O cliente do CRM (`ClienteDaGestaoDeNegocios`) não tem nenhum método que escreva.
- **Rotas (medido em 28/09/2026):**

| Rota | Fonte | O que traz | Paginada? |
|---|---|---|---|
| `/api/v1` | — | índice | não |
| `/api/v1/filiais` | a própria GN | número, nome e código TOTVS das 20 lojas | **não** |
| `/api/v1/paineis` | — | catálogo dos painéis, com colunas, filtros e `campo_periodo` | não |
| `/api/v1/cobertura` | TOTVS (VVA010) | meses de estoque por mês (13) e por grupo (9), em quantidade | **não** |
| `/api/v1/cadastros` | a própria GN | catálogo dos cadastros | não |
| `/api/v1/cadastros/{nome}` | a própria GN | `metas`, `de_para_consultores`, `forecast`, `consorcio`, `opcionais` | sim |
| `/api/v1/paineis/art` | ART | vendas de máquina, com valor, lucro, ICMS e margem | sim |
| `/api/v1/paineis/estoque` | TOTVS | o Painel Executivo, sem colunas declaradas | sim |
| `/api/v1/paineis/estoque-pedidos` | TOTVS | estoque e pedidos à fábrica, máquina a máquina | sim |
| `/api/v1/paineis/historico-estoque` | TOTVS | estoque no fim de cada mês (13 meses) | sim |
| `/api/v1/paineis/negociacoes` | **Vórtice** | processos em negociação | sim |
| `/api/v1/paineis/pedidos` | **Vórtice** | processos em pedido, com instituição financeira e linha de crédito | sim |
| `/api/v1/paineis/processos-crm` | **Vórtice** | processos em faturamento | sim |
| `/api/v1/paineis/samkam` | **Vórtice** | pedidos SAM/KAM | sim |
| `/api/v1/paineis/performance-maquinas` | ART + metas | meta × realizado de máquinas do ano fiscal | sim |
| `/api/v1/paineis/performance-consorcio` | relatório de consórcio + metas | meta × realizado de consórcio | sim |

- **Envelope dos paginados:** `{ gerado_em, idade_segundos, total, pagina, paginas, por_pagina, linhas[] }`. O CRM lê
  até `pagina = paginas` e confere que as linhas somam `total`, senão a leitura falha.
- **Frescor medido:** a `idade_segundos` dos painéis ia de 20 s a 3 min. A exceção é a performance de consórcio, com
  76 min. O atraso de cerca de 21 h visto em 27/09 não se repetiu.

## 2. As regras de leitura que só a medição mostrou

| # | Regra | Onde |
|---|---|---|
| L-1 | **Período padrão.** Painel com `campo_periodo` aplica um período quando ninguém informa data: o `art` devolveu 84 linhas (o mês) sem `data_de` e 4.197 com. **Passe sempre `data_de`/`data_ate`** | performance de máquinas e de consórcio |
| L-2 | **A exceção é o estoque.** Em `estoque-pedidos`, o período é pela data de entrada, e o pedido à fábrica ainda não entrou. Com a janela, vêm 459 linhas e somem os 229 pedidos; sem nada, vêm as 688. **Leia sem janela**, com a trava de remoção de 20% | estoque |
| L-3 | `/filiais` e `/cobertura` **não são paginadas**: leia o documento inteiro | `LerDocumentoAsync` |
| L-4 | **Os painéis escrevem a filial pelo NOME.** O código do TOTVS vem do de-para `/filiais`, casado pelo código estável do nome (`FiliaisDaGestaoDeNegocios`) | todos |
| L-5 | **Há lojas sem código TOTVS:** Digital (0), Grandes Contas (900), Colorado Franca (6) e Colorado Equipamentos (891). Não são filial do CRM: a linha é contada à parte e não é gravada | consórcio, estoque, conferência |
| L-6 | **O realizado da performance de máquinas é a máquina ENTREGUE, no mês da entrega:** 1.321 de 1.321 linhas do FY26. Pelo faturamento, só 947 | régua do realizado (#283) |
| L-7 | O mês da performance vem como rótulo em português (`Set/2026`) | `LeitorDoPlanejamentoDaGestaoDeNegocios.Mes` |
| L-8 | No forecast, **nulo é "o gestor não informou"**, e não zero | forecast |
| L-9 | O capitão do de-para **é o gestor**: 8 de 8 gestores do forecast e da performance de consórcio. Os 59 consultores com meta e os 30 com cota estão no de-para, e 2 consultores aparecem em duas linhas (vale a vigência mais recente) | planejamento |
| L-10 | A cota vendida tem chave **grupo + cota** (185 cotas, 185 pares distintos) | consórcio |
| L-11 | `chaint` é único no estoque (690 de 690). O chassi falta no pedido que ainda não saiu da fábrica | estoque |

## 3. O que o CRM consome

| Rota | Tabela do CRM | Rotina | Issue / PR |
|---|---|---|---|
| `cadastros/metas` | `organizacao.MetaDeVenda` | 9, modo `--somente-metas-gn` | #138 / #248 |
| `cadastros/de_para_consultores`, `cadastros/forecast`, `paineis/performance-consorcio` (linhas Realizado), `filiais` | `organizacao.GestorDoConsultor`, `ForecastDaGerencia`, `CotaDeConsorcioVendida` | 9, modo `--somente-planejamento-gn` | #278 / #279 |
| `paineis/estoque-pedidos`, `cobertura`, `filiais` | `frota.EquipamentoEmEstoque`, `frota.CoberturaDoEstoque` | 12, `--somente-estoque-gn`, de hora em hora | #281 / #282 |
| `paineis/performance-maquinas`, `filiais` | `integracao.ConferenciaDaGestaoDeNegocios` e `integracao.DivergenciaDeIntegracao` | 13, `--somente-conferencia-gn` | #285 / #286 |

Todas as rotinas nascem desligadas e usam a conexão 13 (`GESTAO_NEGOCIOS`). Todas têm simulação e as travas das metas:
formato ilegível acima do limite ou remoção em massa abortam a rodada inteira, e só `--aceitar-remocao`, no terminal,
passa por cima.

**O que nunca entra:**
- nome de cliente, consorciado, vendedor ou atendente (a leitura casa por chassi, filial, mês e pela chave da pessoa
  do consultor);
- valor, lucro, ICMS, margem e custo;
- o texto livre de observação.

## 4. As decisões por painel, atualizadas

| Id (doc 46) | Painel | Em 16/09 | Agora |
|---|---|---|---|
| GN-P1 | `/filiais` | consumir como referência | **consumido**, como de-para das lojas (L-4, L-5) |
| GN-P2 | `/paineis`, `openapi.json` | verificação de deriva | lido na medição; sem rotina |
| GN-P3 | `art` | não consumir como fonte | **mantido**: o ART é lido direto. A conferência usa a `performance-maquinas`, que já é o gabarito da gestão |
| GN-P4 | `estoque` | não consumir | **mantido** (colunas não declaradas) |
| GN-P5 | `estoque-pedidos` | decidir | **consumido** (#282), sem financeiro e sem cliente |
| GN-P6 | `historico-estoque`, `/cobertura` | não consumir nesta etapa | `/cobertura` **consumida** (#282); `historico-estoque` fica para depois |
| GN-P7 | `negociacoes`, `pedidos`, `processos-crm`, `samkam` | não consumir (R-8) | **Decisão do Ricardo em 28/09/2026:** entram **só como gabarito** da conferência, e não como dado do CRM. A conferência do pipeline por eles é a próxima parte da #285 |
| — | `cadastros/*` (não existiam no §5 do 46) | — | `metas`, `de_para_consultores` e `forecast` **consumidos**. `consorcio` (a carteira desde 2015) e `opcionais` não são consumidos |
| — | `performance-maquinas`, `performance-consorcio` (não existiam no §5 do 46) | — | consórcio: o realizado, cota a cota (#279). Máquinas: gabarito da conferência (#286) |

## 5. As perguntas à equipe de Inteligência de Mercado

**Nenhuma foi enviada ainda.** Enviar é contato com terceiros e pede a autorização do Ricardo, então o dono do envio é o
Ricardo. A medição respondeu parte delas. As que continuam abertas estão abaixo, na ordem do §5.9 do documento 46.

| # | Pergunta | Situação em 28/09/2026 |
|---|---|---|
| 1 | Chave por consumidor, com escopo por painel? Quem são os consumidores e qual a janela de rotação? | **pendente**, com o Ricardo |
| 2 | `A1_COD` + `A1_LOJA` (ou CPF/CNPJ) nos painéis? | **pendente**, e só depois da 1. Hoje o CRM não precisa: casa por chassi, filial e pessoa |
| 3 | O "CRM" dos quatro painéis de processo é o Vórtice? | **respondida pela medição:** o envelope diz `fonte=CRM`, e as colunas são do Vórtice (`seq_pessoa`, `dna`, `processo`). Continua aberto o que acontece quando o Vórtice for desligado |
| 4 | Versionamento, changelog, aviso de mudança, SLA? | **pendente** |
| 5 | Leitura incremental, id estável, sinal de saída? | **pendente.** Os cadastros têm `id` estável; os painéis não. O CRM compara pelo resumo do conteúdo e exclui sem apagar |
| 6 | Quais campos o painel `estoque` deveria declarar? | **pendente** (o CRM usa o `estoque-pedidos`) |
| 7 | Semântica dos campos | **em parte respondida:** a `filial` dos painéis é o NOME (L-4); o `chaint` é único (L-11); o `mes_rotulo` da performance é o mês da entrega (L-6); a situação do ART é ENTREGUE, FAT. NÃO ENTREGUE ou NÃO FATURADO. Continuam abertos o `comar`, o `aparte`, o `fat_direto` e o `ano` (modelo ou fabricação) |
| 8 | Como são derivados os filtros sobre campos que não voltam? | **pendente** |
| 9 | Paginação, taxa, fuso de `gerado_em` | **em parte:** as leituras do CRM fecharam (linhas = `total`) em todas as medições. O fuso de `gerado_em` não é declarado, e o CRM o trata como horário local e converte |
| 10 | TLS | **respondida:** curinga DigiCert, pelo nome |
| 11 | Projeção de campos? | **pendente.** O CRM descarta na leitura o financeiro e os nomes |
| 12 | Documentação das regras do Qlik? | **pendente** |
| 13 | O painel `art` lê a mesma view? `comar`? AMS? | **em parte:** a `/cobertura` declara que as saídas vêm da VVA010 e cobrem AMS, que não passa pelo ART |
| 14 | Serviço, conta, reinício, responsável pela operação | **pendente** |

**Perguntas novas (28/09/2026):**

15. O `estoque-pedidos` com `data_de`/`data_ate` perde os pedidos à fábrica (L-2). Isso é intencional? O CRM pode
    contar com a leitura sem janela devolver tudo?
16. A performance de consórcio chegou com 76 min de idade, e os outros painéis com minutos. Qual é o ciclo dela?
17. As lojas sem código TOTVS (Digital, Grandes Contas, as duas Colorado) vão continuar fora do de-para? A venda da
    Digital tem uma filial de origem que o CRM deveria usar?
