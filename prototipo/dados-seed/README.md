# dados-seed — dados extraídos do protótipo

Esta pasta contém os dados mock do protótipo `prototipo/referencia/assets/app.js` extraídos para
arquivos `.json` individuais, prontos para inspeção, diffing e para alimentar a documentação em
`docs/prototipo/`. Todo o conteúdo é **gerado**; a fonte da verdade continua sendo o `app.js`.

## Como reexecutar

```bash
# Gera/atualiza os 45 arquivos desta pasta (44 dados + manifesto.json)
node prototipo/dados-seed/extrair.mjs

# Só valida — não escreve nada, útil em CI ou antes de commitar
node prototipo/dados-seed/extrair.mjs --check
```

O extrator (`extrair.mjs`) carrega o `app.js` real dentro de um contexto `node:vm` isolado, com
stubs de `window`/`document`/`localStorage`/`Chart`/`L` (Leaflet) suficientes para o script rodar
sem lançar exceção, e então lê de volta cada constante de topo pelo nome. Detalhe técnico registrado
no cabeçalho do arquivo: declarações `const`/`let` de topo dentro de `vm.runInContext` não viram
propriedades do objeto de contexto — elas vivem no *global lexical scope* do contexto, que persiste
entre chamadas. Por isso o extrator roda o `app.js` primeiro e, na sequência, um segundo script no
mesmo contexto que só referencia os identificadores e devolve um objeto com eles.

## Se o `app.js` mudar

1. Rode `node prototipo/dados-seed/extrair.mjs --check` primeiro. A tabela impressa mostra o status
   de cada constante (`ok`, `ok (materializado)`, `NAO CAPTURADO` ou `FALHOU: <mensagem>`).
2. Se alguma constante nova precisar ser capturada, ou uma existente for renomeada/movida, edite o
   array `MAPA` (ou `ESCALARES`, para valores soltos) no topo de `extrair.mjs` — cada entrada é
   `[nomeDaConstante, arquivoDeSaida, descricaoCurta, linhaAproximada]`. A linha é só informativa
   (não é usada para localizar nada em runtime); atualize-a se quiser manter o manifesto preciso.
3. Rode `node prototipo/dados-seed/extrair.mjs` (sem `--check`) para regravar os `.json` e o
   `manifesto.json`.
4. Releia os arquivos que mudaram antes de usá-los em documentação — o extrator serializa o valor
   exatamente como está em memória; se o texto de um rótulo mudou no `app.js`, ele muda aqui também.

**Estado atual (verificado nesta rodada):** `node prototipo/dados-seed/extrair.mjs --check` reporta
as 44 constantes mapeadas com status `ok` (ou `ok (materializado)` para `PERF_SERIES`) — nenhuma
`NAO CAPTURADO` nem `FALHOU`. Veja a seção final para o que causaria cada um desses dois status caso
apareçam no futuro.

## Índice dos 45 arquivos

Cada linha abaixo corresponde a uma constante de `app.js`. "Linha app.js" é onde a constante é
**declarada** (`const NOME = ...`); múltiplas linhas nos escalares indicam declarações separadas
agrupadas num único arquivo de saída.

| Arquivo | Constante em app.js | Linha app.js | Registros | O que é / o que representa no negócio |
|---|---|---:|---:|---|
| `routes.json` | `ROUTES` | 3 | 15 | Tabela do router por hash (`#/caminho` → `{title, crumb, render, mount?}`). É o índice de todas as telas navegáveis do protótipo — ver `docs/prototipo/01-ESPEC-UI.md` para o detalhamento tela a tela. |
| `funil.json` | `FUNIL_DATA` | 281 | 3 (`stages`, `totals`, `detalhamento`) | Dados do relatório **Funil de Vendas do Mês** (`/relatorios/funil`): 7 estágios com quantidade/valor/nº de oportunidades, totais agregados e as 10 linhas de detalhamento por oportunidade da tabela. Entidade de negócio: **oportunidade agregada por estágio de funil comercial**. |
| `cobertura.json` | `COBERTURA_DATA` | 693 | 4 (`meta`, `geral`, `regionais[7]`, `vendas_perdidas`) | Dados do painel **Cobertura de Carteira · Regional** (`/relatorios/cobertura`): meta de cobertura (80%), consolidado geral, 7 regionais com cobertura/vendas perdidas, e breakdown de motivo de venda perdida. Entidade: **cobertura de carteira agregada por regional**. |
| `agenda.json` | `AGENDA_DATA` | 1125 | 3 (`cen_atual`, `cens_disponiveis[4]`, `tarefas[27]`) | Dados da **Agenda do CEN** (`/agenda`): o CEN logado, os 4 CENs alternáveis no filtro, e as 27 tarefas (visitas/ligações/propostas/monitoramentos) que alimentam os 5 grupos por data (atrasada/hoje/amanhã/semana/futura). Entidade: **tarefa/atividade agendada**, equivalente a `wf.Tarefa` no doc 04. |
| `tipo-meta.json` | `TIPO_META` | 1173 | 4 | Taxonomia dos 4 tipos de tarefa da agenda (`monitorar`, `visitar`, `ligar`, `proposta`) com rótulo, ícone e cores. Entidade: **catálogo `wf.TipoTarefa`**. |
| `cliente-84391.json` | `CLIENTE_DATA` | 1634 | 30 campos de topo | Ficha 360 completa do cliente-referência **Agroindustrial Salvador Arena Ltda (#84391)**: identificação, endereços, contatos, telefones/e-mails, segmentação, frota (7 equipamentos), oportunidades (3), interações (5), financeiro e alterações cadastrais pendentes. Entidade: **`crm.Conta`** (+ `crm.Contato`, `crm.CanalContato`, `crm.ContaCarteira`, `equip.Equipamento`, `wf.Processo`, `wf.Atividade`). |
| `equipamento-1RW7250PVMR123456.json` | `EQUIPAMENTO_DATA` | 2212 | 30 campos de topo | Ficha completa do equipamento-referência **Trator 7250R, chassi 1RW7250PVMR123456**: especificações técnicas, dados de aquisição, garantia PowerGard, histórico de horas (7 meses), 5 revisões, 1 chamado aberto, peças aplicadas e telemetria JDLink. Entidade: **`equip.Equipamento`**. |
| `oportunidade-1517613.json` | `OPORTUNIDADE_DATA` | 2712 | 22 campos de topo | Ficha completa da oportunidade-referência **OP-2026-08471 · Renovação Frota Cana · 2× Trator 8R 250**: 3 itens de proposta, timeline de 8 fases, 6 aprovações, documentação (proposta + faturamento) e 9 eventos de histórico. Entidade: **`wf.Processo`** (+ `wf.ProcessoEstagioTrilha`, aprovações ≈ `seg.CompartilhamentoRegistro`/regra de alçada, itens não têm tabela própria no doc 04 — ver divergências). |
| `meta-frequencia.json` | `META_FREQ` | 3376 | 4 | Meta de dias entre contatos por classe ABCD do cliente (A=30d, B=60d, C=90d, D=120d) — usada em `statusCobertura()` para calcular o status de cobertura de cada cliente. Entidade: parâmetro de `wf.Regra`/config comercial (equivale ao "SLA de frequência" citado no doc 04 mas não modelado como tabela própria — ver divergências). |
| `config-categorias-interacao.json` | `CAT_INTERACAO` | 3379 | 5 | Categorias de interação usadas no **drawer de registro de contato da Cobertura** (visita, ligação, whatsapp, email, remota) com cor e path de ícone SVG. Note: existe uma **segunda** taxonomia quase idêntica em `config-categorias-interacao-cfg.json` (ver abaixo) — são catálogos distintos no código-fonte, não o mesmo dado duplicado por engano; um alimenta o drawer de Cobertura, o outro a tela de Configurações › Taxonomias. |
| `carteira-cen.json` | `CARTEIRA_CEN` | 3389 | 23 | Os 23 clientes da carteira do CEN José Rufino/João Ribeiro (varia por tela) usados em **Cobertura de Carteira** (`/cobertura`) e como fonte do seletor "Cliente da carteira" em **Nova Oportunidade**. Cada registro tem geolocalização (`lat`/`lng`) para os pins do mapa Leaflet. Entidade: **`crm.Conta` + `crm.ContaCarteira`** (visão operacional de um CEN). |
| `status-cobertura-labels.json` | `STATUS_LABELS` | 3445 | 5 | Rótulos e cores dos 5 status de cobertura (`em_dia`, `aviso`, `atraso`, `critico`, `nunca`) calculados por `statusCobertura()`. |
| `estado-carteira.json` | `CARTEIRA_STATE` | 3718 | 2 | Estado inicial (em memória) dos filtros da tela Cobertura de Carteira: `filtros.{status,classe,cat,exige}` e `cidade` (usado para o clique no pin do mapa filtrar a tabela). |
| `cidades-latlng.json` | `CIDADE_LATLNG` | 3750 | 6 | Geocodificação (lat/lng) das 6 cidades de MT Norte usadas no mapa da Cobertura de Carteira. |
| `pipeline-fases.json` | `PIPELINE_FASES` | 4049 | 6 | As 6 colunas do kanban de **Pipeline de Vendas**: `qualificacao`, `diagnostico`, `proposta`, `negociacao`, `fechamento`, `ganho_perdido`, cada uma com rótulo e hint. Entidade: **`wf.Estagio`** de um `wf.TipoProcesso` de venda. |
| `cens.json` | `CENS` | 4059 | 8 | Cadastro dos 8 CENs (consultores comerciais) com nome, sigla, regional e cor — usado para colorir avatares no kanban do Pipeline. Entidade: **`seg.Usuario`** (subconjunto de campos). |
| `linha-icon.json` | `LINHA_ICON` | 4071 | 6 | Emoji por linha de produto (Tratores 🚜, Colheitadeiras 🌾, Pulverizadores 💧, Plantadeiras 🌱, Peças & Serviços 🔧, Implementos ⚙️) usado como decoração em selects e cards. |
| `pipeline.json` | `PIPELINE_MOCK` | 4081 | 34 | As 34 oportunidades mock que populam o kanban de Pipeline de Vendas — cada uma com fase, valor, probabilidade, cliente, CEN. Entidade: **`wf.Processo`** (visão simplificada, sem os campos de auditoria completos que `OPORTUNIDADE_DATA` tem). |
| `estado-pipeline.json` | `PIPELINE_STATE` | 4130 | 8 | Estado inicial dos filtros/seleção do Pipeline: persona, CEN, regional, filtro de linha, valor mínimo, busca, card selecionado, item em drag. |
| `clientes-extra.json` | `CLIENTES_EXTRA` | 4622 | 24 | Enriquecimento (CNPJ, segmento, porte, nº de equipamentos, CEN dono, última compra) para 24 clientes da carteira do CEN atual — mesclado com `CARTEIRA_CEN` na tela **Clientes** (lista). Chave é o `id` do cliente. |
| `clientes-outras.json` | `CLIENTES_OUTRAS` | 4651 | 29 | 29 clientes adicionais de **outras regionais/CENs** (Marcelo Silva/MT Sul e outros), usados quando a tela Clientes está no escopo "Regional" ou "Nacional" no switcher CEN/Regional/Nacional. |
| `estado-clientes.json` | `CLIENTES_STATE` | 4721 | 6 | Estado inicial da tela Clientes: persona, CEN, regional, objeto `filtros` (classe/cidade/status/segmento/porte), busca, e ordenação (`campo`+`dir`). |
| `estado-config.json` | `CONFIG_STATE` | 5096 | 2 | Estado inicial de Configurações: aba ativa (`aba: "preferencias"`) e seção ativa dentro dela (`secao: "perfil"`). |
| `config-metas.json` | `CONFIG_METAS` | 5102 | 8 | Metas comerciais e SLAs configuráveis: frequência de visita/ligação por classe ABCD, SLA de aprovação (diretor 48h, gerente 24h, financeiro 12h) e alçada de desconto por nível (gerente 5%, diretor 10%, presidente 20%). Entidade: parâmetros de **`wf.Regra`**/alçada, hoje só representados como config solta (ver divergências). |
| `config-motivos-perda.json` | `CONFIG_MOTIVOS_PERDA` | 5113 | 8 | Taxonomia editável de motivos de perda de venda, com contagem de uso nos últimos 90 dias. Entidade: catálogo próximo de **`wf.Desfecho`** (classe `Perda`). |
| `config-categorias-interacao-cfg.json` | `CONFIG_CATEGORIAS_INTERACAO` | 5124 | 6 | Taxonomia editável de categorias de interação (para a tela Configurações › Taxonomias), com flag `vale_cobertura` — decide se aquela categoria conta para o cálculo de cobertura. É uma tabela de **configuração administrativa**; a irmã `config-categorias-interacao.json` é o catálogo de **apresentação** (cor+ícone) usado no drawer de contato. |
| `config-integracoes.json` | `CONFIG_INTEGRACOES` | 5133 | 7 | As 7 integrações do CRM (TOTVS Protheus, Entra ID, Microsoft 365, JD Operations Center, GLPI, WhatsApp Business API, Vórtice legado) com status, endpoint, frequência de sync e notas. Entidade: aproxima-se de **`intg.Watermark`**/`intg.ChaveExterna` (visão de monitoramento, não a tabela transacional). |
| `config-usuarios.json` | `CONFIG_USUARIOS` | 5220 | 12 | Os 12 usuários do sistema com role e último login. Entidade: **`seg.Usuario`**. |
| `config-roles.json` | `CONFIG_ROLES` | 5235 | 5 | Os 5 perfis de acesso (Admin TI, Diretor Comercial, Gerente Regional, CEN, Admin Comercial) com contagem de usuários e descrição textual do escopo de permissão. Entidade: aproxima-se de **`seg.ConjuntoPermissao`**, mas sem a granularidade de `seg.Permissao`/`Profundidade` do doc 04 — é descritivo, não uma matriz de permissão real (ver divergências). |
| `estado-performance.json` | `PERF_STATE` | 5923 | 4 | Estado inicial da tela Performance de CEN: persona, CEN selecionado, regional, métrica de breakdown selecionada. |
| `fytd-meses.json` | `FYTD_MESES` | 5931 | 10 | Calendário fiscal Tracbel: 10 meses de nov/25 a ago/26 (o ano fiscal vai de nov a out), cada um com dias úteis; ago/26 é marcado `parcial: true` (17 de ~22 dias úteis). Base de todas as séries FYTD do protótipo. |
| `performance-cens.json` | `PERF_CENS` | 5945 | 8 | Os 8 CENs com meta de vendas (mês e FYTD) e data de admissão — usados no relatório Performance de CEN e no ranking. |
| `performance-series.json` | `PERF_SERIES` | 6022 (chamada) / função geradora em 5966 | 8 (uma série por CEN, 10 pontos mensais cada) | Séries mensais FYTD por CEN: vendas, pipeline criado, oportunidades ganhas/perdidas, conversão, visitas, ticket médio, cobertura. **Não é uma constante literal** — é *materializada* chamando `gerarSerieCEN(cenId, seed)` (gerador pseudoaleatório `mulberry32`) para cada CEN de `PERF_CENS`, por isso o extrator trata este arquivo como caso especial (ver "materializado" abaixo). Fórmulas exatas documentadas em `docs/prototipo/01-ESPEC-UI.md` (seção Performance de CEN). |
| `estado-nova-oportunidade.json` | `NOVA_OP_STATE` | 6583 | 14 | Estado inicial (e também o "estado zerado" para o qual `resetNovaOpState()` volta) do formulário **Nova Oportunidade**: cliente, linha, modelo, quantidade, valor, fase inicial (`qualificacao`), previsão, probabilidade (15%), título, observação. |
| `catalogo-modelos.json` | `CATALOGO_MODELOS` | 6601 | 6 (linhas de produto, cada uma com N modelos) | Catálogo de modelos por linha de produto (Tratores, Colheitadeiras, Pulverizadores, Plantadeiras, Implementos, Peças & Serviços) com valor de referência — alimenta o select em cascata Linha→Modelo do formulário Nova Oportunidade. |
| `mercado-pracas.json` | `MERCADO_JD_PRACAS` | 7203 | 4 | Total de máquinas do mercado por praça/filial (MT Norte, MT Sul, GO, BA) segundo John Deere Connect, com quanto a Tracbel vendeu, quanto perdeu com registro, e quanto está sem conhecimento. Base do KPI "Conhecimento de mercado" da Visão 360. |
| `vendas-perdidas-motivos.json` | `VENDAS_PERDIDAS_MOTIVOS` | 7211 | 6 | Vendas perdidas por motivo nos últimos 12 meses (quantidade + valor + cor), usado no gráfico de barras horizontais da Visão 360. |
| `faturamento-12m.json` | `FATURAMENTO_12M` | 7222 | 4 (`labels`, `realizado_global`, `previsto_global`, `meta_global`) | Série global de faturamento (R$ milhões) dos últimos 12 meses (set/25–ago/26) — Realizado × Previsto × Meta. É a base para as séries por perfil, escaladas pelos multiplicadores em `constantes-escalares.json`. |
| `mix-linhas.json` | `MIX_LINHAS` | 7233 | 5 | Participação percentual de cada linha de produto no faturamento FYTD, com cor — alimenta o donut "Mix por linha" da Visão 360. |
| `top-clientes.json` | `TOP_CLIENTES_GLOBAL` | 7242 | 8 | Ranking global dos 8 maiores clientes por faturamento FYTD — usado nos perfis Gerente/Diretor da Visão 360 (o perfil CEN usa `CARTEIRA_CEN` de verdade em vez deste mock). |
| `alertas.json` | `ALERTAS_POR_PERFIL` | 7254 | 3 (uma lista por perfil) | 3 alertas gerenciais por persona (CEN/Gerente/Diretor) exibidos no card "Alertas gerenciais" da Visão 360, cada um com tipo (crítico/aviso/info), texto e ação. |
| `perfis-360.json` | `PERFIS_360` | 7273 | 3 | Os 3 perfis/personas alternáveis na Visão 360 (CEN João Ribeiro, Gerente Roberto Silveira, Diretora Cláudia Nunes) com nome, cargo, avatar, cor e rótulo de escopo. |
| `pos-vendas-cliente-84391.json` | `POSVENDAS_CLIENTE_84391` | 8095 | 8 (`fat_pv`, `fat_12m`, `os_abertas`, `pmp_pendentes`, `alertas_criticos`, `contratos_jdcp`, `frota_sem_jdcp`, `nps`) | Dados completos da aba **Pós-vendas** da ficha do cliente 84391: faturamento de peças/serviços, 4 ordens de serviço abertas, 3 PMPs (campanhas de campo John Deere), 4 alertas críticos de telemetria/garantia, 2 contratos JDCP ativos, 2 equipamentos sem JDCP (oportunidade) e NPS. Entidade: mistura **`wf.Processo`/`wf.Tarefa`** (OS), **`equip.Equipamento`** (PMP/contrato) e **`wf.Atividade`** (NPS) — não tem contraparte direta única no doc 04 (ver divergências: pós-venda/oficina não é modelado na Fase 1). |
| `constantes-escalares.json` | `NOVAS_STORAGE_KEY`, `CONTADOR_STORAGE_KEY`, `AGENDA_HOJE`, `HOJE_CARTEIRA`, `FAT_MULT_MT_NORTE`, `FAT_MULT_CEN_JOAO` | 66, 67, 1123, 3373, 7229, 7230 | 6 | Seis valores escalares soltos, agrupados num único arquivo: as duas chaves de `localStorage` (ver `docs/prototipo/01-ESPEC-UI.md`), as duas datas "hoje" fixas do protótipo (`AGENDA_HOJE` = 25/08/2026, `HOJE_CARTEIRA` = mesma data em outro fuso de serialização) e os dois multiplicadores usados por `agregar360()` para estimar o faturamento de "Gerente MT Norte" (32% do global) e "CEN João" (11% do global) a partir da série global. |
| `manifesto.json` | — (gerado pelo próprio extrator, não existe em app.js) | — | 44 linhas de relatório | Índice de auditoria: para cada constante mapeada, nome, arquivo de saída, linha aproximada, contagem de registros e status da extração. É o próprio "log" que o `extrair.mjs` imprime como tabela no console, salvo em disco. |

**Nota sobre "PERF_SERIES (materializado)":** diferente de todas as outras entradas, `PERF_SERIES`
começa vazio (`const PERF_SERIES = {}`) e só ganha conteúdo quando alguma tela chama
`gerarSerieCEN(cenId, seed)` sob demanda. O extrator força essa materialização rodando, dentro do
mesmo contexto `vm`, `PERF_CENS.forEach(c => PERF_SERIES[c.id] ??= gerarSerieCEN(...))` antes de
serializar — por isso a coluna de status do manifesto diz `ok (materializado)` em vez de `ok`.

## Constantes que não puderam ser capturadas (e por quê isso aconteceria)

Na extração mais recente (`node prototipo/dados-seed/extrair.mjs --check`, conferida nesta rodada de
documentação), **as 44 constantes mapeadas foram capturadas com sucesso** — o `manifesto.json` não
tem nenhuma linha `NAO CAPTURADO` nem `FALHOU: ...`. Não há, portanto, nenhuma lacuna de dados hoje.

O mecanismo de falha existe em `extrair.mjs` (função `main()`) para dois cenários que **poderiam**
acontecer se o `app.js` mudar sem atualizar o extrator:

1. **`NAO CAPTURADO`** — acontece quando `capturado[NOME]` volta `undefined` depois de avaliar
   `typeof NOME !== 'undefined' ? NOME : undefined` no contexto `vm`. Na prática, isso ocorre se uma
   constante do array `MAPA` for renomeada, remove do escopo léxico de topo do arquivo (por exemplo,
   movida para dentro de uma função), ou nunca existiu com aquele nome exato. Quando isso acontece o
   arquivo de saída correspondente **não é escrito** (o extrator pula) e um aviso é impresso no
   console (`[aviso] NOME não foi capturado`).
2. **`FALHOU: <mensagem>`** — reservado hoje só para a materialização de `PERF_SERIES`: se
   `gerarSerieCEN`, `PERF_CENS` ou `mulberry32` forem removidos/renomeados em `app.js`, a expressão
   `PERF_CENS.forEach(...)` lança dentro do `vm.runInContext` e o `catch` grava esse status em vez de
   travar a extração inteira.

Se uma futura rodada de `--check` mostrar qualquer um desses dois status, o próximo passo é abrir
`app.js` na linha aproximada indicada no `manifesto.json`, confirmar o novo nome/local da constante e
atualizar a entrada correspondente em `MAPA` (ou em `ESCALARES`) neste próprio arquivo.
