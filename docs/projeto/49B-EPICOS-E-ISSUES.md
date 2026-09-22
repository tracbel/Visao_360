# 49B — Épicos e issues da Inteligência de Mercado

> Anexo do [documento 49](49-INTELIGENCIA-DE-MERCADO.md) · 22/09/2026.
> **Parte A** — as 22 issues novas, cada uma com objetivo, contexto, regra de negócio, fontes, tabelas/APIs,
> arquivos impactados, dependências, critérios de aceite e testes. **Parte B** — o que muda nas 11 issues que
> já existem. **Parte C** — os 19 épicos.
> Milestones reaproveitados: **M11** (dados e parâmetros), **M12** (motor e Visão Diretoria), **M13** (CEN e
> clientes). Label comum: `market-potential`.
> Regras que valem para todas: parâmetro vazio → resultado vazio com o motivo; nada de dado de cliente, nome
> de CEN ou venda por município em arquivo versionado; toda tabela nova atualiza o documento 14 na mesma
> mudança; migration destrutiva (DROP de tabela ou coluna) pede autorização antes.

---

## Onde está cada uma no GitHub (abertas em 22/09/2026)

| Código | Issue | Título |
|---|---|---|
| IM-01 | #150 | Cobertura dos dados do motor, medida pelo servidor |
| IM-02 | #151 | De-para das culturas entre as fontes |
| IM-03 | #152 | Competência e unidade em cada medida |
| IM-04 | #153 | Nunca apagar histórico nas fontes de mercado |
| IM-05 | #154 | Município por código oficial em todas as fontes |
| IM-06 | #155 | Totais de São Paulo publicados para o Censo e a PPM |
| IM-07 | #156 | Série histórica da PAM e milho por safra |
| IM-08 | #157 | SICOR: janela fechada, máquina por parâmetro e Região × SP |
| IM-09 | #158 | Histórico longo de preço: investigação das fontes |
| IM-10 | #159 | Custo de referência por cultura e rentabilidade |
| IM-11 | #160 | Compartilhamento de máquina entre culturas |
| IM-12 | #161 | Calculadora de máquinas |
| IM-13 | #162 | Captura, share e oportunidade, com classificação de confiança |
| IM-14 | #163 | Hierarquia SP → Região → Loja → Município e recorte por município |
| IM-15 | #164 | Idade do parque do cliente × ciclo de renovação |
| IM-16 | #165 | Culturas e categorias de máquina no Administrador |
| IM-17 | #166 | Parâmetros do motor que ainda não existem |
| IM-18 | #167 | Tooltip de rastreabilidade em todo número |
| IM-19 | #168 | Preço, custo e crédito comparando Região × SP e reagindo ao recorte |
| IM-20 | #169 | Retirar o resíduo de maquete do mercado |
| IM-21 | #170 | Rodada do motor gravada e tela em componentes |
| IM-22 | #171 | Testes de ouro, conciliação e regressão visual |

| Épico | Issue |
|---|---|
| E01 — Data Discovery | #172 |
| E02 — Data Quality | #173 |
| E03 — IBGE Integration | #174 |
| E04 — SICOR Integration | #175 |
| E05 — Commodity Intelligence | #176 |
| E06 — Production Cost Intelligence | #177 |
| E07 — Machine Pricing | #178 |
| E08 — Structural Potential Engine | #179 |
| E09 — Market Cycle Engine | #180 |
| E10 — Scenario Engine | #181 |
| E11 — Market Share | #182 |
| E12 — Geographic Intelligence | #183 |
| E13 — Customer Potential | #184 |
| E14 — Machine Renewal | #185 |
| E15 — Commercial Action Plan | #186 |
| E16 — Administrator | #187 |
| E17 — Geographic Dashboard | #188 |
| E18 — Performance | #189 |
| E19 — Testing & Validation | #190 |

As 11 issues existentes da parte B receberam o comentário "Atualização de escopo — doc 49".

---

## Parte A — Issues novas

### IM-01 — Cobertura dos dados do motor, medida pelo servidor (#150)

- **Épico:** E01 Data Discovery · **Milestone:** M11 · **Labels:** `market-potential`, `data-quality`, `backend`, `P0`
- **Objetivo:** saber, com número e sem planilha, o que cada fonte cobre no servidor antes de programar o
  motor — e deixar isso visível para sempre, não medido uma vez.
- **Contexto [M]:** as 12 fontes públicas estão no servidor desde 20–21/09/2026, e o painel de fontes
  (Configurações › Fontes públicas) já mostra linhas, período e municípios cobertos por fonte. Não mostra:
  a cobertura por **cultura** da PAM e da CONAB nos municípios da Região, os meses **fechados** do SICOR, nem
  o dado interno que o motor vai usar — vendas do ART por município do comprador e por modelo,
  `Equipamento.AnoFabricacao`, vendas perdidas com modelo e preço, endereços com área e cultura (0% em
  16/09).
- **Regra de negócio:** só contagem e cobertura (linhas, período, % de municípios da Região, % nulo). Nenhum
  valor de venda, nome de cliente ou de CEN sai do servidor.
- **Fontes:** o banco do CRM, somente leitura.
- **Tabelas/APIs:** `ProducaoAgricolaNoMunicipio`, `CotacaoDeProduto`, `CustoDeProducao`,
  `CreditoRuralDeInvestimento`, `frota.VendaDeMaquina`, `frota.Equipamento`, `processo.VendaPerdida`,
  `comercial.Endereco`; rota `GET /api/v1/integracoes/fontes-publicas` estendida (ou
  `GET /api/v1/mercado/cobertura`).
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Portas/PortasDeFontesPublicas.cs`,
  `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeFontesPublicas.cs`,
  `src/Tracbel.Crm.Aplicacao/Integracoes/ObterFontesPublicas.cs`,
  `src/Tracbel.Crm.Web/src/componentes/config/ConfigSecaoFontes.tsx`, `src/Tracbel.Crm.Web/src/tipos/potencial.ts`.
- **Dependências:** nenhuma.
- **Critérios de aceite:** para cada linha da matriz 49A com dado no CRM, o painel mostra cobertura por
  cultura/produto e por município da Região; o dado interno aparece como contagem e percentual; a fonte com
  cobertura abaixo do mínimo aparece em destaque, com a frase que diz por quê.
- **Testes:** API com banco semeado (contagens conferidas à mão); componente do painel com a fonte incompleta
  em destaque; teste de que a resposta não contém nome nem valor monetário por cliente.

### IM-02 — De-para das culturas entre as fontes (#151)

- **Épico:** E01 Data Discovery · **Milestone:** M11 · **Labels:** `market-potential`, `data-quality`, `documentation`, `P0`
- **Objetivo:** para cada cultura, o código ou rótulo dela em cada fonte — a semente do catálogo de culturas
  (IM-16).
- **Contexto [M]:** a lista de culturas existe em quatro lugares com grafias diferentes (front de preços,
  front de custos, `UnidadeComercial`, produtos da PAM na regra). A planilha do comercial classifica os 71
  produtos da PAM em 9 segmentos (grãos, cana, citrus, café, fruticultura, olericultura, algodão, borracha,
  outros); o CRM não tem essa classificação.
- **Regra de negócio:** o vínculo é por **código** quando a fonte tem código (PAM: classificação 782; SICOR:
  produto), e por rótulo exato quando não tem (CONAB, Socicana). Café entra na soma da lavoura só pelo
  "Total" (40139).
- **Fontes:** SIDRA 5457 (metadados da classificação 782); `PrecosMensalUF.txt`; séries de custo da CONAB;
  Socicana; tabela de produtos do SICOR.
- **Tabelas/APIs:** leitura de `ProducaoAgricolaNoMunicipio`, `CotacaoDeProduto`, `CustoDeProducao`, `ItemDoSicor`.
- **Arquivos impactados:** `docs/projeto/49C-CATALOGO-DE-CULTURAS.md` (novo) — a tabela cultura × fonte ×
  código × unidade × fator × segmento.
- **Dependências:** nenhuma.
- **Critérios de aceite:** as seis culturas do pedido com o vínculo em todas as fontes que as têm; os 71
  produtos da PAM com segmento; todo código citado existe no banco do servidor (conferido pela IM-01).
- **Testes:** um teste de arquitetura que falha se um código do catálogo não existir na semente de teste
  (entra com a IM-16).

### IM-03 — Competência e unidade em cada medida (#152)

- **Épico:** E02 Data Quality · **Milestone:** M11 · **Labels:** `market-potential`, `data-quality`, `database`, `backend`, `P0`
- **Objetivo:** nenhuma medida sem ano, unidade e moeda; nenhuma razão entre anos diferentes sem aviso;
  produtividade calculada pela regra do IBGE.
- **Contexto [M]:** a rota escolhe `Max(Ano)` da PAM para todas as medidas e culturas; a unidade da
  quantidade não é guardada (a coluna chama `QuantidadeProduzidaToneladas`, e o IBGE publica alguns
  produtos em mil frutos); a produtividade não é calculada; a planilha a calcula sobre área plantada e com
  anos de rótulo diferentes (I-04, I-05, I-06).
- **Regra de negócio:** `produtividade(m,c,a) = quantidade(m,c,a) ÷ área colhida(m,c,a)`, na unidade do
  IBGE; colhida zero ou nula → nula. Conversão para unidade comercial só quando a unidade do IBGE for massa.
  O ano de referência de cada medida é o último com dado **para aquela medida e cultura**; se diferir do das
  outras medidas mostradas juntas, o número leva o aviso.
- **Fontes:** SIDRA 5457 e as notas da tabela. **[M 22/09]** o campo `MN` do SIDRA diz "Toneladas" para os 85 produtos (é a unidade da variável): a unidade de cada produto sai das notas 2 e 6 da tabela, como regra de domínio (`UnidadesDaPam`), e não de coluna gravada.
- **Tabelas/APIs:** `ProducaoAgricolaNoMunicipio` e `ProducaoAgricolaNoEstado` (coluna de unidade; renomear
  a quantidade **sem perda**, por `sp_rename`); contratos de `/territorio/indicadores` e das rotas novas com
  `competencia` por medida.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Organizacao/AreaDeAtuacao.cs`,
  `src/Tracbel.Crm.Integracao/Ibge/LeitorDoIbge.cs`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs`,
  `src/Tracbel.Crm.Dominio/Portas/PortasDeIndicadoresTerritoriais.cs`,
  `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs`, migration nova.
- **Dependências:** nenhuma.
- **Critérios de aceite:** a produtividade de SP por cultura (2024) bate com a variável 112 do IBGE
  (rendimento médio, kg/ha) ao quilo; a unidade de cada produto aparece junto da quantidade; nenhuma rota
  devolve medida sem competência.
- **Testes:** domínio (colhida zero, unidade mil frutos, ano faltando); leitor com resposta gravada do SIDRA
  e teste de ouro da produtividade contra a variável 112; migration no contêiner (a coluna renomeada preserva os valores).

### IM-04 — Nunca apagar histórico nas fontes de mercado (#153)

- **Épico:** E02 Data Quality · **Milestone:** M11 · **Labels:** `market-potential`, `data-quality`, `database`, `P0`
- **Objetivo:** toda revisão de fonte fica rastreável; nada que já foi mostrado some.
- **Contexto [M]:** preço, custo e crédito guardam o valor anterior na trilha; PAM, Censo, PPM e área
  territorial **sobrescrevem sem trilha**; a usina que sai da lista da ANP é **apagada**
  (`CargaDaEstruturaAgropecuaria.cs:493`); a linha reclassificada do SICOR é apagada e a trilha guarda só o
  `Valor`, sem a combinação que a identificava (`TrilhaDeAuditoria` grava só os campos da política).
- **Regra de negócio:** revisão da fonte substitui o valor e deixa o anterior na trilha, com a chave inteira;
  usina que sai da lista fica **encerrada**, não apagada; linha do SICOR que sai continua apagada (para não
  contar duas vezes a mesma operação), mas a trilha registra a combinação inteira.
- **Fontes:** as mesmas.
- **Tabelas/APIs:** `PoliticaDeAuditoria` (PAM município e estado, frota, estabelecimentos, rebanho, área
  territorial, usina; chave do SICOR); `UsinaDeEtanol.EncerradaEm` (coluna nova).
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Auditoria/PoliticaDeAuditoria.cs`,
  `src/Tracbel.Crm.Aplicacao/Auditoria/RotulosDaTrilha.cs`, `src/Tracbel.Crm.Dominio/Organizacao/UsinaDeEtanol.cs`,
  `src/Tracbel.Crm.Carga/CargaDaEstruturaAgropecuaria.cs`, `src/Tracbel.Crm.Carga/CargaDoCreditoRural.cs`, migration nova.
- **Dependências:** nenhuma.
- **Critérios de aceite:** uma revisão simulada da PAM aparece na trilha com antes e depois; usina ausente
  na leitura nova fica com data de encerramento e some só da contagem vigente; exclusão de linha do SICOR
  aparece na trilha com produto, programa, subprograma, fonte, seguro, atividade, modalidade, município e mês.
- **Testes:** carga sobre SQLite com duas leituras (antes/depois); teste de que todo campo auditado tem
  rótulo (o que já existe); migration no contêiner.

### IM-05 — Município por código oficial em todas as fontes (#154)

- **Épico:** E02 Data Quality · **Milestone:** M11 · **Labels:** `market-potential`, `data-quality`, `database`, `integration`, `P0`
- **Objetivo:** nenhuma carga casar município por nome a cada rodada; a correspondência fica gravada,
  auditada e conferível.
- **Contexto [M]:** SICOR, ANP e custos da CONAB casam por nome normalizado
  (`SaneamentoDeTerritorio.ChaveSemApostrofo`). O SICOR grava o código do Banco Central
  (`CodigoMunicipioBcb`) e refaz o casamento por nome em toda carga; a planilha do comercial errou 13
  municípios por comparar nome cru.
- **Regra de negócio:** tabela de correspondência `fonte + código (ou texto) na fonte → município`, com a
  forma do casamento (código oficial, nome normalizado único, manual) e a data. A carga consulta a tabela
  primeiro; nome novo casa por nome normalizado **só se for único**, e a correspondência é gravada; ambíguo
  ou sem par vira recusa pendente com o motivo, visível no painel de fontes.
- **Fontes:** SICOR (código BCB), ANP (conferir se o arquivo traz código IBGE), CONAB custos (local).
- **Tabelas/APIs:** `CorrespondenciaDeMunicipio` (nova); painel de fontes com pendências.
- **Arquivos impactados:** `src/Tracbel.Crm.Carga/CargaDoCreditoRural.cs`,
  `src/Tracbel.Crm.Carga/CargaDaEstruturaAgropecuaria.cs`, `src/Tracbel.Crm.Carga/CargaDeCustosDeProducao.cs`,
  `src/Tracbel.Crm.Integracao/Carga/SaneamentoDeTerritorio.cs`, domínio e configuração da tabela nova,
  `docs/projeto/14-PADRAO-DE-BANCO.md` §2.1, testes de contagem de tabelas.
- **Dependências:** D-IM-08 (schema).
- **Critérios de aceite:** a segunda rodada de cada carga não faz casamento por nome nenhum (medido por
  contador no log); os 637 municípios do SICOR ficam gravados com a forma "nome normalizado único"; nome
  ambíguo simulado vira recusa pendente.
- **Testes:** carga com município de nome repetido em duas UFs; com grafia nova; com código BCB novo.

### IM-06 — Totais de São Paulo publicados para o Censo e a PPM (#155)

- **Épico:** E02 Data Quality · **Milestone:** M11 · **Labels:** `market-potential`, `data-quality`, `integration`, `P1`
- **Objetivo:** todo "% de SP" usa o total **publicado** pelo IBGE, como a lavoura já usa.
- **Contexto [M]:** a lavoura compara a região com `ProducaoAgricolaNoEstado` (linha publicada); tratores e
  propriedades comparam com a **soma dos municípios** (`LerTotaisDoEstadoAsync`), que perde o sigilo — dois
  métodos na mesma linha de indicadores (I-03).
- **Regra de negócio:** SP = linha do estado (nível n3, código 35) em cada tabela; Região = soma dos
  municípios; a diferença entre soma e publicado aparece no tooltip.
- **Fontes:** SIDRA 6871, 6780, 3939 e 4714 no nível n3.
- **Tabelas/APIs:** tabela(s) de totais do estado para a estrutura agropecuária (desenho decidido na
  implementação contra o documento 14 — uma por pesquisa ou uma de totais publicados); `TotaisDoEstado` no contrato.
- **Arquivos impactados:** `src/Tracbel.Crm.Integracao/Ibge/LeitorDaEstruturaAgropecuaria.cs`,
  `src/Tracbel.Crm.Carga/CargaDaEstruturaAgropecuaria.cs`,
  `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs`, migration nova, doc 14.
- **Dependências:** nenhuma.
- **Critérios de aceite:** o total de tratores de SP na tela é o publicado pelo IBGE para 2017; o tooltip
  mostra quanto a soma dos municípios fica abaixo.
- **Testes:** leitor com resposta gravada do nível n3; API com os dois totais.

### IM-07 — Série histórica da PAM e milho por safra (#156)

- **Épico:** E03 IBGE Integration · **Milestone:** M11 · **Labels:** `market-potential`, `integration`, `database`, `P1`
- **Objetivo:** série longa de área, quantidade e valor por cultura e município; área de milho 1ª e 2ª safra
  separada, para o compartilhamento de máquina com a soja (IM-11).
- **Contexto [M]:** a carga traz os **3 anos** mais recentes da PAM (`AnosDaProducaoAgricola = 3`); o
  pedido quer histórico de área, quantidade, valor e produtividade. A soma de soja com milho conta duas vezes
  a terra do milho safrinha.
- **Regra de negócio:** quantos anos trazer é **parâmetro da rotina** (proposta: desde 2010), e a rotina
  anual busca só o que falta; o servidor busca sozinho, nada sai da estação. A tabela 839 do SIDRA (milho por
  safra) é conferida pelos metadados antes da carga; se tiver o nível municipal para SP, entra.
- **Fontes:** SIDRA 5457 (a mesma); SIDRA 839 (a conferir).
- **Tabelas/APIs:** `ProducaoAgricolaNoMunicipio` e `…NoEstado`; tabela de milho por safra, se a 839 servir.
- **Arquivos impactados:** `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs`, `src/Tracbel.Crm.Integracao/Ibge/LeitorDoIbge.cs`,
  `src/Tracbel.Crm.Dominio/Integracao/ConexoesERotinas.cs` (parâmetro da rotina), migration se houver tabela nova.
- **Dependências:** IM-04 (a série longa entra já com trilha).
- **Critérios de aceite:** série contínua desde o ano escolhido para os 645 municípios; a segunda rodada não
  grava nada; milho 1ª + 2ª safra = milho da 5457 por município (tolerância de arredondamento).
- **Testes:** leitor com respostas gravadas; carga idempotente sobre SQLite; conferência da soma do milho.

### IM-08 — SICOR: janela fechada, máquina por parâmetro e Região × SP (#157)

- **Épico:** E04 SICOR Integration · **Milestone:** M11 · **Labels:** `market-potential`, `backend`, `data-quality`, `P0`
- **Objetivo:** o 12 × 12 do crédito compara meses completos, a "máquina" é definida pelo negócio e a Região
  aparece sempre ao lado de SP.
- **Contexto [M]:** a janela termina no último mês com dado (`RepositorioDeCreditoRural`, linhas 43–50), e o
  Banco Central ainda acrescenta contrato registrado com atraso nos meses recentes (I-08); os produtos de
  máquina são constantes no código (`[7080, 4860, 2700]`); a tela filtra "só a ADR", mas não compara com SP.
- **Regra de negócio:** último mês da janela = último mês com dado − carência (parâmetro com vigência,
  D-IM-03); produtos de máquina = os ligados a uma categoria no catálogo (IM-16) — até lá, uma constante só,
  no domínio; linhas são linhas (nunca "contratos"); Região = municípios da Região Tracbel por código.
- **Fontes:** SICOR `InvestMunicipioProduto` (já carregado).
- **Tabelas/APIs:** `CreditoRuralDeInvestimento`, `ItemDoSicor`; `/territorio/credito` (ou `/mercado/credito`)
  com `regiao` e `sp` lado a lado e a janela explícita (início, fim, meses em carência).
- **Arquivos impactados:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCreditoRural.cs`,
  `src/Tracbel.Crm.Dominio/Portas/PortasDeCreditoRural.cs`, `src/Tracbel.Crm.Dominio/Organizacao/ParametrosDoPotencial.cs`
  (carência), `src/Tracbel.Crm.Web/src/componentes/territorio/PainelDeCredito.tsx`.
- **Dependências:** D-IM-03 (valor da carência; até decidir, a janela mostra "carência não decidida" e usa zero, dito na tela); IM-16 para a categoria.
- **Critérios de aceite:** a resposta diz o intervalo exato de cada janela; nenhum mês de carência entra;
  Região e SP aparecem com linhas, valor, ticket e variações.
- **Testes:** repositório com relógio fixo e meses sintéticos; API com Região × SP.

### IM-09 — Histórico longo de preço: investigação das fontes (#158)

- **Épico:** E05 Commodity Intelligence · **Milestone:** M11 · **Labels:** `market-potential`, `integration`, `documentation`, `P1`
- **Objetivo:** saber se existe série longa, aberta e do mesmo conceito para destravar o momento 12 ÷ 12 e
  as médias de 3 e 5 anos antes de 09/2027.
- **Contexto [M]:** a CONAB publica só os últimos 12 meses; o CRM guarda desde 09/2025 e cresce um mês por
  vez; o CEPEA exige licença (D-P11, #117); a Socicana já tem 11 safras.
- **Regra de negócio:** série de **conceito diferente não se emenda** (preço recebido pelo produtor ≠ preço em
  praça de referência); barreira de acesso (CAPTCHA, termos) não se contorna — procura-se o equivalente
  aberto e diz-se o que a troca custa.
- **Fontes candidatas:** IEA-SP (preços recebidos pelos produtores paulistas), IPEADATA, outros arquivos do
  portal da CONAB, CEPEA com licença.
- **Tabelas/APIs:** nenhuma nesta issue.
- **Arquivos impactados:** `docs/projeto/49D-HISTORICO-DE-PRECOS.md` (novo).
- **Dependências:** nenhuma.
- **Critérios de aceite:** para cada candidata: endereço, termos de uso, formato, período, culturas de SP,
  conceito do preço e se casa com a CONAB; recomendação; se houver fonte viável, a issue do leitor aberta.
- **Testes:** não se aplica.

### IM-10 — Custo de referência por cultura e rentabilidade (#159)

- **Épico:** E06 Production Cost Intelligence · **Milestone:** M12 · **Labels:** `market-potential`, `backend`, `P0`
- **Objetivo:** receita, custo e margem por hectare de cada cultura, com local, sistema, safra, fonte e data
  de cada parte.
- **Contexto [M]:** preço (CONAB/Socicana) e custo (CONAB, 166 abas de SP) estão na tela, **separados**; a
  margem não existe; qual local é "a referência de SP" de cada cultura e se a margem usa custo operacional
  ou total estão em aberto (D-P07); a planilha multiplica a margem pela área plantada (C-07) e usa um ATR para
  24 meses (C-06).
- **Regra de negócio:** `receita/ha = produtividade (IM-03) × preço médio dos meses da safra`; cana:
  `ATR (kg/t) da safra × preço do kg de ATR × t/ha`; `margem/ha = receita/ha − custo/ha` na camada e no local
  de referência vigentes; `margem total = margem/ha × área colhida`;
  `índice de rentabilidade = (receita ÷ custo) da safra ÷ média das N safras anteriores` (N parâmetro).
  Referência ausente → vazio com o motivo.
- **Fontes:** PAM (produtividade), CONAB preços e custos, Socicana.
- **Tabelas/APIs:** `CustoDeProducao`, `CotacaoDeProduto`; referência por cultura no catálogo (IM-16) ou em
  vigência própria; `GET /api/v1/mercado/culturas`.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Mercado/Rentabilidade.cs` (novo), repositório e caso de uso
  das culturas, `src/Tracbel.Crm.Web/src/componentes/territorio/PainelDeCustos.tsx`.
- **Dependências:** IM-03, IM-16; D-P07.
- **Critérios de aceite:** com Franca como referência do café em 2025, o custo total é R$ 29.279,94 (aceite
  da #67) e a margem sai com as três competências no tooltip; cultura sem custo de SP fica sem margem, dito.
- **Testes:** domínio com números públicos; bordas (custo só operacional, preço faltando no mês, colhida zero).

### IM-11 — Compartilhamento de máquina entre culturas (#160)

- **Épico:** E08 Structural Potential Engine · **Milestone:** M12 · **Labels:** `market-potential`, `backend`, `decision`, `P0`
- **Objetivo:** o potencial estrutural não contar duas vezes a mesma terra e a mesma máquina.
- **Contexto [M]:** o protótipo soma as 6 culturas; em SP, o milho safrinha é plantado depois da soja na
  mesma terra, e boa parte do amendoim é plantada em reforma de canavial (C-02).
- **Regra de negócio:** grupos de compartilhamento por categoria de máquina, configurados pelo
  administrador com vigência (D-IM-01). Proposta: a área útil de um grupo é a **maior** área entre as culturas
  dele, com os parâmetros da cultura dominante; sem grupo, cada cultura soma a sua área (neutralidade).
- **Fontes:** PAM (e a 839, se entrar pela IM-07).
- **Tabelas/APIs:** `GrupoDeCompartilhamento` (nova, com vigência) ou campo no parâmetro da cultura (IM-16).
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Mercado/PotencialEstrutural.cs`, cadastro no Administrador.
- **Dependências:** #72, IM-16, D-IM-01.
- **Critérios de aceite:** com grupo, o parque da região é menor ou igual ao da soma simples; sem grupo, é
  igual; o tooltip diz "área compartilhada com …".
- **Testes:** domínio (grupo vazio, grupo com uma cultura sem área, dois grupos).

### IM-12 — Calculadora de máquinas (#161)

- **Épico:** E10 Scenario Engine · **Milestone:** M12 · **Labels:** `market-potential`, `backend`, `frontend`, `api`, `P1`
- **Objetivo:** simular "quantas máquinas esta área comporta e quantas por ano", pelo mesmo motor da tela.
- **Contexto [D]:** R-09 do documento 48 ("calculadora de trator e de máquinas em geral") e o pedido de 22/09.
- **Regra de negócio:** entrada: área por cultura, categoria, cenário e data; saída: parque, demanda
  anual, demanda ajustada e os três cenários, com os parâmetros vigentes na data. Nada é gravado.
- **Fontes:** parâmetros vigentes; indicadores da última rodada (IM-21).
- **Tabelas/APIs:** `POST /api/v1/mercado/calculadora`.
- **Arquivos impactados:** `src/Tracbel.Crm.Api/Endpoints/EndpointsDeMercado.cs` (novo), caso de uso,
  `src/Tracbel.Crm.Web/src/componentes/mercado/Calculadora.tsx` (novo), aberta a partir do cartão de potencial.
- **Dependências:** #72, #74.
- **Critérios de aceite:** a mesma área de um município devolve o mesmo resultado que o motor gravou para ele;
  parâmetro ausente → resultado vazio com o motivo; a calculadora não aparece para quem não tem `Mercado.Ler`.
- **Testes:** API (igualdade com a rodada); componente (estado sem parâmetro).

### IM-13 — Captura, share e oportunidade, com classificação de confiança (#162)

- **Épico:** E11 Market Share · **Milestone:** M12 · **Labels:** `market-potential`, `backend`, `P1`
- **Objetivo:** mostrar quanto da demanda estimada a Tracbel captura e onde está o espaço não capturado, sem
  chamar estimativa de fato.
- **Contexto [M]:** "participação de mercado: sem dado" no painel executivo; nenhuma fonte aberta dá o total
  de máquinas vendidas por município; a planilha calcula captura com ano fiscal John Deere contra demanda em
  ano civil (I-14); o formulário de venda perdida tem concorrente, modelo, quantidade e preços.
- **Regra de negócio:** `captura = vendas Tracbel (un) ÷ demanda anual (un)`, **ano civil** nos dois lados
  (ano fiscal como leitura lado a lado); `potencial capturável = max(0, demanda ajustada − vendas)`; share do
  crédito só com venda financiada identificada; oportunidade com confiança **alta** (venda perdida
  registrada), **média** (cliente da carteira com chassi acima do ciclo e sem negociação aberta) ou **baixa**
  (crédito de máquina no município sem venda Tracbel correspondente) — nunca vira "venda perdida" sozinha (D-IM-09).
- **Fontes:** vendas (#69), demanda (#72/#74), `processo.VendaPerdida`, SICOR, API GN (#12) para financiamento.
- **Tabelas/APIs:** resultado da rodada (IM-21); `/api/v1/mercado/resumo` e `/municipios`.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Mercado/Captura.cs` (novo), repositório da rodada, aba de
  captura no cartão de vendas.
- **Dependências:** #69, #72, #74; #12 para o share do crédito; D-IM-09.
- **Critérios de aceite:** nenhum texto chama captura de market share; toda oportunidade tem confiança e
  origem; município sem venda em unidades mostra "sem dado", não zero.
- **Testes:** domínio (anos diferentes recusados, demanda zero, venda maior que demanda).

### IM-14 — Hierarquia SP → Região → Loja → Município e recorte por município (#163)

- **Épico:** E12 Geographic Intelligence · **Milestone:** M12 · **Labels:** `market-potential`, `backend`, `frontend`, `ux`, `P0`
- **Objetivo:** todo indicador lido em qualquer nível da hierarquia, sempre ao lado da Região e de SP; ao
  escolher um município, a página inteira reage.
- **Contexto [M]:** "região" no código é Norte/Noroeste (`RegiaoDaAreaDeAtuacao`), no pedido é a Região
  Tracbel inteira (I-01); a loja tem três recortes (I-02); o clique no município só abre a ficha.
- **Regra de negócio:** níveis: **SP** (publicado) → **Região Tracbel** (municípios da ADR) → **sub-região**
  (Norte/Noroeste) → **Loja** (loja responsável pelo município, D-IM-07) → **Município** → **Cliente** (E13).
  Com um município escolhido, os indicadores mostram Município × Região × SP; a escolha fica na URL
  (`?municipio=<código IBGE>`) para poder ser compartilhada.
- **Fontes:** `MunicipioDaAreaDeAtuacao` (hoje de planilha; a decisão no CRM é da #107).
- **Tabelas/APIs:** parâmetros `nivel` e `codigo` nas rotas de mercado; rótulos na API e na tela.
- **Arquivos impactados:** `src/Tracbel.Crm.Web/src/telas/IndicadoresGeograficos.tsx` e os componentes que saírem
  dele (IM-21), `src/Tracbel.Crm.Web/src/tipos/territorio.ts`, rotas de mercado, `docs/projeto/15-GLOSSARIO-E-NOMES.md`.
- **Dependências:** IM-21 (componentes), #75; D-IM-07.
- **Critérios de aceite:** a palavra "região" sozinha não aparece na tela nem no contrato; escolher um
  município muda os indicadores, o crédito, as culturas em destaque nos preços e os cenários; a ficha abre
  como hoje; recarregar a página mantém a escolha.
- **Testes:** componente (recorte liga e desliga); API por nível; regressão visual da seção preservada (IM-22).

### IM-15 — Idade do parque do cliente × ciclo de renovação (#164)

- **Épico:** E14 Machine Renewal · **Milestone:** M13 · **Labels:** `market-potential`, `backend`, `P2`
- **Objetivo:** saber, por cliente e município, quantas máquinas estão dentro do ciclo, entrando na janela,
  com renovação provável ou acima do ciclo.
- **Contexto [M]:** `frota.Equipamento` tem ano de fabricação, ano do modelo e data da venda; o
  preenchimento no servidor não foi medido (IM-01); o ciclo por categoria vem do catálogo (IM-16).
- **Regra de negócio:** `idade = ano corrente − ano de fabricação` (ou da venda, quando faltar);
  `Dentro do ciclo` se idade < ciclo − janela; `Entrando na janela` se ciclo − janela ≤ idade < ciclo;
  `Renovação provável` se ciclo ≤ idade < ciclo + tolerância; `Acima do ciclo` a partir daí (D-IM-10).
  Equipamento sem ano fica "sem ano", fora das faixas.
- **Fontes:** CRM/ART (`frota.Equipamento`, `frota.VendaDeMaquina`).
- **Tabelas/APIs:** resultado da rodada; ficha do município e ficha do cliente.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Mercado/Renovacao.cs` (novo), repositório, aba no cartão de
  estrutura ou de cobertura (a decidir na #76).
- **Dependências:** IM-01, IM-16, D-IM-10.
- **Critérios de aceite:** as quatro faixas somam os equipamentos com ano; "sem ano" aparece com a contagem;
  dado de cliente só com permissão.
- **Testes:** domínio (bordas exatas de cada faixa, ano futuro recusado).

### IM-16 — Culturas e categorias de máquina no Administrador (#165)

- **Épico:** E16 Administrator · **Milestone:** M11 · **Labels:** `market-potential`, `backend`, `frontend`, `database`, `P0`
- **Objetivo:** cultura nova entra pela tela e aparece em preço, custo, potencial e mapa sem mudar código;
  os parâmetros do potencial passam a ser por cultura × categoria de máquina.
- **Contexto [M]:** `RegraDePotencial` é por produto da PAM, sem categoria, unidade, fonte de preço/custo,
  produtividade nem sensibilidade; o front tem listas fixas de culturas (M-03); o SICOR tem produtos de
  máquina fixos (M-02); a tela lê `regras[0]`.
- **Regra de negócio:** `Cultura`: nome, segmento, unidade comercial e fator, produtos da PAM que a compõem,
  se entra na soma da lavoura, produto de preço (CONAB/Socicana), série de custo, ativa. `CategoriaDeMaquina`:
  trator, plantadeira, colheitadeira, pulverizador, implemento, agricultura de precisão (D-IM-06), ligada a
  família/linha/modelo e aos produtos do SICOR. Parâmetro da cultura × categoria **com vigência**: ha por
  máquina, ciclo em anos, família/modelo de referência, produtividade de referência (opcional), sensibilidade.
  A regra do café de 13/09/2026 é **preservada** como a primeira vigência de café × trator.
- **Fontes:** o de-para da IM-02.
- **Tabelas/APIs:** `Cultura`, `CategoriaDeMaquina`, parâmetro por cultura × categoria (evolução da
  `RegraDePotencial`, migration aditiva que preserva a linha); `/api/v1/admin/parametros-do-potencial` estendida.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Organizacao/ParametrosDoPotencial.cs`, configuração EF, migration,
  `src/Tracbel.Crm.Aplicacao/Potencial/*`, `src/Tracbel.Crm.Web/src/componentes/config/ConfigSecaoPotencial.tsx` e
  `config/potencial/*`, `src/Tracbel.Crm.Web/src/componentes/territorio/PainelDePrecos.tsx`,
  `PainelDeCustos.tsx`, `src/Tracbel.Crm.Dominio/Auditoria/PoliticaDeAuditoria.cs`, doc 14.
- **Dependências:** IM-02; D-IM-06, D-IM-08. Relacionada: #45 (taxonomias comerciais).
- **Critérios de aceite:** cadastrar uma cultura de teste faz ela aparecer nas rotas de preço, custo e
  potencial; o front não tem lista fixa de culturas; a vigência do café continua; toda inclusão entra na
  trilha; 403 para quem não administra.
- **Testes:** domínio (vigência por cultura × categoria); API (403, trilha); migration no contêiner com a
  linha do café preservada; teste de arquitetura contra lista fixa de cultura no front.

### IM-17 — Parâmetros do motor que ainda não existem (#166)

- **Épico:** E16 Administrator · **Milestone:** M11 · **Labels:** `market-potential`, `backend`, `frontend`, `database`, `P0`
- **Objetivo:** todo número do motor que depende de escolha do negócio vira parâmetro com vigência — e o que
  ninguém decidiu aparece em "o que falta decidir".
- **Contexto [M]:** `ParametroDoPotencial` tem janela, peso dos contratos, faixas, limite da percepção, pesos
  dos indicadores e limites do fator. Faltam: carência do SICOR, suavização e limites do índice de crédito,
  mínimo de linhas para índice próprio, base do poder de compra, N safras da rentabilidade, bandas de
  sensibilidade dos cenários, janela e tolerância da renovação.
- **Regra de negócio:** todos anuláveis; nulo → o cálculo que depende dele fica vazio com o motivo e o
  parâmetro entra nas `pendencias`; o conjunto continua se lendo inteiro numa data.
- **Fontes:** decisões D-IM-02, D-IM-03, D-IM-05, D-IM-10 e D-P03.
- **Tabelas/APIs:** `ParametroDoPotencial` (colunas novas, migration aditiva); leitura e escrita já existentes.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Organizacao/ParametrosDoPotencial.cs`,
  `src/Tracbel.Crm.Aplicacao/Potencial/*`, `src/Tracbel.Crm.Web/src/componentes/config/potencial/FormularioDosGerais.tsx`,
  `PoliticaDeAuditoria.cs`, `RotulosDaTrilha.cs`.
- **Dependências:** nenhuma para a estrutura; os valores, das decisões.
- **Critérios de aceite:** cada parâmetro novo aparece no formulário, na trilha e nas pendências quando vazio.
- **Testes:** domínio (validação de faixas); API (pendências); componente do formulário.

### IM-18 — Tooltip de rastreabilidade em todo número (#167)

- **Épico:** E17 Geographic Dashboard · **Milestone:** M12 · **Labels:** `market-potential`, `frontend`, `api`, `ux`, `P0`
- **Objetivo:** todo número diz de onde veio — "Fonte: IBGE/SIDRA — PAM — Tabela 5457 — Safra 2024 — última
  atualização 01/10/2026" — e os parágrafos fixos dos cartões viram esse tooltip.
- **Contexto [M]:** a procedência é um selo por painel (`SeloProcedencia`), e a da rota de indicadores lista 4
  tabelas quando a apuração lê 19 (I-18); cada cartão tem um parágrafo fixo de aviso (sigilo, ANP, Censo de 2017).
- **Regra de negócio:** a API devolve `procedencia` por indicador: fonte, pesquisa/tabela, variável,
  competência, última atualização (a rodada da carga) e ressalva. O front não escreve fonte à mão.
- **Fontes:** painel de fontes públicas (última rodada de cada fluxo).
- **Tabelas/APIs:** contratos das rotas de mercado e de território.
- **Arquivos impactados:** `src/Tracbel.Crm.Web/src/componentes/mercado/Procedencia.tsx` (novo),
  `src/Tracbel.Crm.Web/src/componentes/cadastro/Indicadores.tsx`, cartões de mapa, contratos C# e TS.
- **Dependências:** #75.
- **Critérios de aceite:** todo número da seção "O mercado da região" e dos quatro cartões tem tooltip, pelo
  teclado também; nenhum nome de fonte escrito no front; os avisos longos saem do corpo do cartão e o
  conteúdo continua no tooltip.
- **Testes:** componente (conteúdo e acessibilidade); API (todo indicador com procedência completa).

### IM-19 — Preço, custo e crédito comparando Região × SP e reagindo ao recorte (#168)

- **Épico:** E17 Geographic Dashboard · **Milestone:** M12 · **Labels:** `market-potential`, `frontend`, `P1`
- **Objetivo:** os três painéis do fim da página falam a mesma língua do resto: Região × SP e o município
  escolhido.
- **Contexto [M]:** o preço é de SP (a CONAB publica por UF), o custo é por local da CONAB e o crédito tem
  filtro "só a ADR" sem SP ao lado; nenhum dos três reage ao município.
- **Regra de negócio:** preço: atual, médias, variação, momento e faixa (da #73), com as culturas do recorte
  primeiro; custo: a série de referência de cada cultura (IM-10) em destaque; crédito: Região, SP e o
  município escolhido lado a lado (IM-08).
- **Fontes:** as mesmas.
- **Tabelas/APIs:** `/mercado/culturas`, `/mercado/credito`.
- **Arquivos impactados:** `PainelDePrecos.tsx`, `PainelDeCustos.tsx`, `PainelDeCredito.tsx`, `territorio.css`.
- **Dependências:** #73, IM-08, IM-10, IM-14.
- **Critérios de aceite:** com um município escolhido, o crédito mostra os três níveis e o preço mostra
  primeiro as culturas dele; sem série, "sem dado".
- **Testes:** componente para cada painel com e sem recorte.

### IM-20 — Retirar o resíduo de maquete do mercado (#169)

- **Épico:** E17 Geographic Dashboard · **Milestone:** M12 · **Labels:** `market-potential`, `frontend`, `P2`
- **Objetivo:** nenhum número fictício de mercado no pacote publicado.
- **Contexto [M]:** `public/dados/mercado-pracas.json` (4 praças fictícias de MT, GO e BA) continua no pacote,
  com o tipo `MercadoPraca` e a entrada no `manifesto.json`; nada o carrega hoje.
- **Regra de negócio:** o arquivo sai; o tipo sai; o manifesto sai; a tela de mercado não lê `public/dados`.
- **Fontes:** —
- **Tabelas/APIs:** —
- **Arquivos impactados:** `src/Tracbel.Crm.Web/public/dados/mercado-pracas.json`,
  `src/Tracbel.Crm.Web/public/dados/manifesto.json`, `src/Tracbel.Crm.Web/src/tipos/visao360.ts`.
- **Dependências:** nenhuma.
- **Critérios de aceite:** build e testes verdes; nenhuma referência ao arquivo.
- **Testes:** teste que falha se um componente de território ou mercado importar o carregador de `public/dados`.

### IM-21 — Rodada do motor gravada e tela em componentes (#170)

- **Épico:** E18 Performance · **Milestone:** M12 · **Labels:** `market-potential`, `backend`, `frontend`, `database`, `architecture`, `P0`
- **Objetivo:** a tela lê o potencial pronto, o número de ontem continua consultável, e a página aguenta
  as evoluções sem virar um arquivo de 2 mil linhas.
- **Contexto [M]:** a rota de território lê cliente, vínculo e faturamento inteiros a cada chamada e calcula
  em memória; o motor vai cruzar 645 municípios × culturas × categorias × cenários; `IndicadoresGeograficos.tsx`
  tem 1.149 linhas num componente só, sem teste de componente.
- **Regra de negócio:** `RodadaDoMotor` (quando, data de referência, vigências usadas, última carga de cada
  fonte) e `ResultadoDoMotor` (nível, código, cultura, categoria, medidas, parcelas do fator, cenários,
  competências). O orquestrador gera rodada quando uma carga termina ou uma vigência começa; **rodada nunca
  é apagada**. A API lê a mais recente ou a da data pedida (`em`).
- **Fontes:** as tabelas de mercado e os parâmetros.
- **Tabelas/APIs:** duas tabelas novas (schema `mercado`, D-IM-08); rotina nova no orquestrador.
- **Arquivos impactados:** `src/Tracbel.Crm.Dominio/Mercado/*`, `src/Tracbel.Crm.Carga/Orquestrador.cs`,
  `src/Tracbel.Crm.Dominio/Integracao/ConexoesERotinas.cs`, configuração EF, migration, doc 14;
  `IndicadoresGeograficos.tsx` quebrado em `componentes/territorio/cartoes/*` sem mudar o visual.
- **Dependências:** #72 para o conteúdo da rodada (a estrutura pode vir antes); D-IM-08.
- **Critérios de aceite:** p95 da rota de resumo abaixo de 500 ms com a Região inteira (medido no servidor);
  duas rodadas seguidas sem mudança de entrada não geram rodada nova; a página quebrada em componentes produz
  as mesmas capturas de antes (IM-22).
- **Testes:** orquestrador com cargas simuladas; API por data; vitest de cada cartão; regressão visual.

### IM-22 — Testes de ouro, conciliação e regressão visual (#171)

- **Épico:** E19 Testing & Validation · **Milestone:** M12 · **Labels:** `market-potential`, `testing`, `P0`
- **Objetivo:** o motor provado contra números conhecidos, a tela provada contra a API e o banco, e a
  seção aprovada protegida contra mudança acidental.
- **Contexto [M]:** com os parâmetros da planilha, a Região comporta 30.317 máquinas e renova 3.457 por ano
  (doc 48 §3.2), sobre a área que a planilha rotula de 2025 e que é a PAM de **2024** (errata do doc 48 §3.8);
  a aba de ciclo **não** serve de oráculo (I-10, I-11, I-12); não existe teste de componente da tela.
- **Regra de negócio:** teste de ouro só onde a planilha está certa (parque e demanda com os parâmetros
  dela; relevância de área e valor; custos já aceitos); onde a análise corrigiu a planilha (C-01 a C-10), o
  teste afirma a regra corrigida e diz no nome o que diverge dela.
- **Fontes:** PAM 2024 dos 203 municípios para as seis culturas (dado público, pode ser versionado como
  fixture); parâmetros da aba Administrador.
- **Tabelas/APIs:** todas as rotas de mercado.
- **Arquivos impactados:** `tests/Tracbel.Crm.Dominio.Testes/Mercado/*`, `tests/Tracbel.Crm.Api.Testes/Mercado*`,
  `scripts/conciliacao/mercado.sql` (somente leitura), capturas de referência da seção preservada (#43).
- **Dependências:** entra junto de cada entrega do motor e da tela.
- **Critérios de aceite:** o motor reproduz 30.317 e 3.457 (tolerância de arredondamento) com os parâmetros da
  planilha; tela = API = consulta independente num recorte conferido; neutralidade (fator 1 sem indicador),
  ordem dos cenários e travas; capturas desktop e 390 px da seção preservada iguais às de referência.
- **Testes:** são o entregável.

---

## Parte B — O que muda nas issues que já existem

Cada item vira um comentário "Atualização de escopo — doc 49" na issue.

### #63 — Decisões do modelo de potencial de mercado

Acrescentar as decisões **D-IM-01 a D-IM-11** (documento 49 §11): compartilhamento de máquina entre
culturas; base do poder de compra; carência do SICOR; indicador 1 do fator; regra dos cenários; categorias de
máquina; nível Loja; schema `mercado`; confiança da oportunidade; janela da renovação; permissão
`Mercado.Ler`. Registrar que o texto de 21/09 fixou a **forma** de D-P01 a D-P10 e que os **valores**
continuam pendentes. Recomendação do documento 49 para a D-P05: fator **aditivo** com trava.

### #69 — Vendas da Tracbel por município para captura e share

Entregar **unidades por modelo e categoria**, não só valor: candidatas medidas no documento 49 — vendas do ART
(`frota.VendaDeMaquina`, por município do comprador; o fluxo está desligado até a credencial) e o item da
nota do Protheus (#18). Ano civil como padrão; ano fiscal John Deere como leitura lado a lado (I-14). A
categoria vem do catálogo (IM-16). Aceite novo: vendas em unidades por município da Região, com a cobertura
medida pela IM-01.

### #70 — Preço de máquina por modelo ao longo do tempo

Candidatas acrescentadas: o preço ofertado no formulário de venda perdida (`processo.VendaPerdida`, com
modelo e data) como referência de baixa confiança; o item da nota do Protheus como principal. Preço por
**modelo de referência da categoria** (IM-16), mediana mensal. Registrar a regra: **sem série de preço de
máquina, o poder de compra fica indisponível** — preço constante faz o termo de troca repetir o preço da saca
(C-03).

### #72 — Potencial estrutural

Escopo novo: por **cultura × categoria de máquina** (IM-16); parque e demanda anual **separados**;
compartilhamento de terra e máquina (IM-11); agregação pela hierarquia (IM-14); Região × SP; resultado
gravado na rodada (IM-21); a tela deixa de ler `regras[0]`. Aceite mantido: reproduzir 30.317 / 3.457 com os
parâmetros da planilha (IM-22).

### #73 — Indicadores de mercado

Escopo novo, com as fórmulas do documento 49 §9.3: momento 12 ÷ 12 com 24 meses completos; médias de 3, 6 e
12 meses, 3 e 5 anos; variação em 12 meses; US$ pelo PTAX do mês; **poder de compra** (`sacas(base) ÷ sacas(t)`,
base 1,00, indisponível sem preço de máquina); **índice de crédito** com janela fechada (IM-08), suavização e
limites por parâmetro (IM-17). A rentabilidade passa para a IM-10.

### #74 — Fator de ciclo e cenários

Recomendação para a D-P05: fator **aditivo** `trava(1 + w₁(I₁ − 1) + w₂(Crédito − 1) + π)` — cada parcela vira
uma linha do tooltip; cenários pelas bandas de sensibilidade por indicador (D-IM-05, IM-17). A aba "Potencial
Ajustado (Ciclo)" **deixa de ser oráculo** do fator (os insumos dela contradizem o decidido — I-10, I-11,
I-12); os testes afirmam neutralidade, ordem e travas.

### #75 — API do potencial de mercado

Rotas propostas no documento 49 §10.2 (`/api/v1/mercado/resumo`, `/municipios`, `/municipios/{codigoIbge}`,
`/culturas`, `/credito`, `/calculadora`), lendo a rodada gravada (IM-21), com `nivel`, `codigo`, `cultura`,
`categoria`, `cenario` e `em`; procedência por indicador (IM-18); permissão `Mercado.Ler` (D-IM-11).
Totais e fatias calculados no servidor (C-09).

### #76 — Tela Visão Diretoria

Regra de preservação do documento 49 §10.3: "O mercado da região", a linha de indicadores e os **quatro
cartões lado a lado** ficam; os cartões evoluem por dentro (Potencial estrutural com seletores; Vendas com
Captura e Comparativo; Cobertura × potencial; Porte das propriedades); os blocos novos ("Ciclo de mercado",
"Cenários") entram **abaixo** dos mapas; o recorte por município (IM-14) e os tooltips (IM-18) com a
justificativa registrada. Primeiro passo: a quebra em componentes com teste (IM-21), antes de qualquer
evolução visual.

### #78 — Visão do CEN

O CEN lê o mercado com `Mercado.Ler` restrito aos municípios dele — a mesma API, com o nível limitado pela
carteira (#48, #107).

### #79 — Potencial por cliente

Regras do documento 49 §9.6: o mesmo motor sobre a área do cliente; soma dos clientes conferida contra o
município; **estabelecimento ≠ cliente ≠ imóvel** — o cruzamento com o Censo é distribuição por faixa, nunca
casamento um a um; a fonte de área continua na D-P13 (cadastro pelo CEN, ART, CAR/SICAR).

### #80 — Segmentação de clientes e plano de ação

Entradas novas: potencial do cliente (#79), situação de renovação (IM-15), oportunidades com confiança
(IM-13) e o ciclo de mercado do município (#74). Plano de ação por segmento continua dependendo da #47.

---

## Parte C — Os épicos

Cada épico vira uma issue com a lista das issues dele.

### E01 — Data Discovery (#172)

Saber, com número e pelo servidor, o que cada fonte cobre e como cada cultura se chama em cada fonte, antes
de programar o motor. **Issues:** IM-01, IM-02.

### E02 — Data Quality (#173)

Competência, unidade e moeda em toda medida; nada apagado sem rastro; município por código; total de SP
publicado. **Issues:** IM-03, IM-04, IM-05, IM-06.

### E03 — IBGE Integration (#174)

Série longa da PAM e milho por safra, sem sair do servidor. **Issues:** IM-07.

### E04 — SICOR Integration (#175)

Crédito com janela fechada, máquina definida pelo negócio e Região × SP. **Issues:** IM-08.

### E05 — Commodity Intelligence (#176)

Preço mensal que só cresce, com médias, variação, momento e US$; histórico longo investigado.
**Issues:** IM-09, #73.

### E06 — Production Cost Intelligence (#177)

Custo de referência por cultura e rentabilidade com a competência de cada parte. **Issues:** IM-10.

### E07 — Machine Pricing (#178)

Preço de referência por modelo e mês, de fonte interna. **Issues:** #70.

### E08 — Structural Potential Engine (#179)

Parque e demanda anual por cultura × categoria, sem dupla contagem, em toda a hierarquia.
**Issues:** #72, IM-11.

### E09 — Market Cycle Engine (#180)

Momento de preço, poder de compra, crédito e percepção comercial. **Issues:** #73.

### E10 — Scenario Engine (#181)

Fator de mercado, demanda ajustada, três cenários e a calculadora. **Issues:** #74, IM-12.

### E11 — Market Share (#182)

Vendas em unidades, captura, share do crédito e oportunidade com confiança. **Issues:** #69, IM-13.

### E12 — Geographic Intelligence (#183)

A hierarquia SP → Região → Loja → Município → Cliente e o recorte por município. **Issues:** IM-14.

### E13 — Customer Potential (#184)

O potencial do cliente pelo mesmo motor e o cruzamento com o Censo. **Issues:** #79.

### E14 — Machine Renewal (#185)

Idade do parque do cliente contra o ciclo da categoria. **Issues:** IM-15.

### E15 — Commercial Action Plan (#186)

Segmentação e plano de ação a partir do potencial, da renovação e do ciclo. **Issues:** #80.

### E16 — Administrator (#187)

Culturas, categorias e todos os parâmetros do motor pela tela, com vigência e trilha.
**Issues:** IM-16, IM-17.

### E17 — Geographic Dashboard (#188)

A API do mercado e a tela evoluída preservando o que foi aprovado. **Issues:** #75, #76, IM-18, IM-19, IM-20.

### E18 — Performance (#189)

Rodada do motor gravada e tela em componentes. **Issues:** IM-21.

### E19 — Testing & Validation (#190)

Ouro, conciliação e regressão visual. **Issues:** IM-22.
