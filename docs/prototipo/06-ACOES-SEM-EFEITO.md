# As pontas soltas — o que promete e não cumpre

> **Documento 06** · Versão 1.0 · 05/09/2026
> Varredura das **13 telas que não são Clientes nem Equipamentos**, mais o cabeçalho comum
> (`componentes/Layout.tsx`), procurando controle que engana: botão principal sem ação, link que
> navega perdendo o que foi digitado, ícone vazio, controle que parece funcionar e não funciona, e
> tela sem estado de vazio ou de erro.

A regra que este documento aplica é do documento 05, §3: ***"botão que não faz nada: ou funciona, ou
sai"***. O precedente do conserto é a Agenda, corrigida em 04/09/2026 — lá o painel de dar andamento
tinha o botão principal sem ação e um link que fechava a tela levando junto o que o usuário tinha
escrito.

**Os números de linha são os do estado ANTES desta rodada.** Depois do conserto eles mudaram; ficam
aqui porque é assim que se confere a varredura contra o código de origem (`git show HEAD~1`).

---

## A gravidade, e o que ela decide

| Gravidade | O que é | O que foi feito |
|---|---|---|
| **engana e perde dado** | a pessoa investe trabalho — digita, escolhe, preenche — e o trabalho some sem aviso, ou some depois de um aviso de sucesso | **consertado, todos** |
| **engana** | promete uma ação e não faz nada; ou afirma um estado que não é verdade | **consertado, todos** — ou fazendo funcionar, ou tirando o controle |
| **cosmético** | incomoda mas não leva a decisão errada nem custa trabalho | **listado e deixado**, com o motivo |

---

## 1. Engana e perde dado — 3 achados

### 1.1 Configurações · "Salvar alterações" avisava que salvou, e não salvava

| | |
|---|---|
| **Tela** | Configurações (`/config`), seis seções |
| **Arquivo** | `telas/Configuracoes.tsx:69` (`salvar()`), e os campos em `componentes/config/ConfigSecao{Perfil,Notificacoes,Atalhos,Metas,Aprovacoes}.tsx` |
| **O que promete** | gravar o que foi alterado — e confirma, com o aviso *"Alterações salvas · em produção grava no Protheus/AD"* |
| **O que faz** | mostra o aviso. Nada mais. **Todos** os campos das seis seções eram `defaultValue`/`defaultChecked`: ninguém lia o valor. Alterar a meta de visita da classe A, salvar, trocar de seção e voltar devolvia o número antigo |
| **Gravidade** | **engana e perde dado** — e da pior maneira, porque o aviso de sucesso ensina a pessoa a não conferir |

Junto vinham três agravantes: **"Cancelar"** avisava *"Alterações descartadas"* e não descartava nada
(o valor continuava na tela); os dois botões estavam sempre habilitados, mesmo sem nenhuma alteração
feita; e **trocar de seção descartava tudo em silêncio**, porque as seções são telas diferentes com
um rodapé só.

**Consertado.** Os campos ficaram controlados e o rascunho vive na tela, para sobreviver à troca de
seção. `Salvar alterações` grava em `dados/persistenciaConfig.ts` e o aviso diz onde ficou gravado —
*o navegador desta máquina*, não o Protheus, não o AD. `Cancelar` volta ao último gravado. Os dois
botões só habilitam quando há alteração pendente, e uma linha avisa que ela existe. Trocar de seção
com alteração pendente pede confirmação, e fechar a aba dispara o `beforeunload`.

### 1.2 Cobertura de Carteira · o painel de registrar contato jogava fora o que foi escrito

| | |
|---|---|
| **Tela** | Cobertura de Carteira (`/cobertura`), painel lateral |
| **Arquivo** | `telas/CoberturaCarteira.tsx:196` (`registrarContato()`), campos em `:576` e `:584` |
| **O que promete** | registrar o contato: categoria da interação, anotação e próxima ação |
| **O que faz** | `window.alert('Contato registrado (protótipo · sem persistência)…')` e fecha o painel. A `textarea` da anotação e o `select` da próxima ação **não tinham estado** — o que fosse digitado neles nunca era lido por ninguém |
| **Gravidade** | **engana e perde dado** — é o mesmo defeito do painel de andamento da Agenda, na tela cuja razão de existir é justamente registrar contato |

**Consertado.** A interação é gravada em `dados/persistenciaInteracoes.ts` (mesmo padrão da Agenda e
da Nova Oportunidade) **e aparece na tela**: a linha do cliente passa a mostrar o contato novo como
última interação, o contador de dias se recalcula pela regra de sempre — inclusive a de que contato
remoto não zera o contador de quem exige visita presencial — e os KPIs acompanham. Há janela de
desfazer no aviso de sucesso.

### 1.3 Nova Oportunidade · "Cancelar" descartava nove campos sem perguntar

| | |
|---|---|
| **Tela** | Nova Oportunidade (`/oportunidades/nova`) |
| **Arquivo** | `telas/NovaOportunidade.tsx:487` |
| **O que promete** | cancelar |
| **O que faz** | `navegar('/')` imediatamente. Um cadastro de três seções e nove campos, e o botão fica **encostado no de salvar** |
| **Gravidade** | **engana e perde dado** — é literalmente o precedente da Agenda: *"um link que fechava a tela levando o que o usuário escreveu"* |

**Consertado.** Sem nada preenchido, sai direto. Com alteração pendente, abre o `DialogoConfirmacao`
dizendo o que se perde, e a aba não fecha em silêncio (`beforeunload`). É a regra §3.4 do padrão de
tela.

---

## 2. Engana — 61 achados, agrupados por tela

### 2.1 Cabeçalho comum, em todas as telas — `componentes/Layout.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:127` | campo de busca *"Buscar cliente, chassi, oportunidade…"* | achar cliente, chassi e oportunidade | nada. Sem estado, sem `onSubmit`. **É a ponta solta mais visível da aplicação: aparece em todas as telas** | `Ctrl+K` põe o foco, `Enter` leva o termo à tela **Clientes**, que busca na API dentro da filial. O texto de exemplo passou a prometer só cliente — chassi é em Equipamentos e oportunidade no Pipeline, cada uma com a própria busca |
| `:129` | `<kbd>Ctrl K</kbd>` | um atalho | nada | passou a funcionar |
| `:135` | botão de notificações, com `badge-dot` de "não lido" | notificações pendentes | nada. **O ponto vermelho não contava nada** | saiu |
| `:139` | botão de sincronização | sincronizar | nada | saiu |

### 2.2 Agenda do CEN — `telas/Agenda.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:545` | **"Nova tarefa"** (ação primária) | criar tarefa | nada | **passou a criar tarefa**: painel com cliente da carteira, tipo, data, prioridade e observação; a tarefa nasce com prazo da política do tipo (documento 05 §4) e entra na lista, com desfazer |
| `:597` | `<select>` CEN | trocar de CEN | nada | os outros CENs aparecem **desabilitados, com o motivo na opção** — só a agenda do João Ribeiro existe no protótipo |
| `:607` | `<select>` Tipo de tarefa | filtrar | nada | filtra a lista e os contadores |
| `:616` | `<select>` Classe de cliente | filtrar | nada | filtra |
| `:625` | `<select>` Prioridade | filtrar | nada | filtra |
| `:625` | **"Limpar filtros"** | limpar | nada | limpa, e só aparece quando há filtro |
| — | estados | — | `<div className="card">Carregando…</div>` e *"Falha ao carregar a agenda."*, sem botão de tentar de novo | `BlocoCarregando` / `BlocoErro` |

### 2.3 Cobertura de Carteira — `telas/CoberturaCarteira.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:339` | **"Expandir"** (mapa) | expandir o mapa | nada | saiu |
| `:206` | **"Exportar CSV"** | exportar | `window.alert('Exportação CSV disparada (protótipo)…')` | **exporta de verdade**, com os filtros aplicados |
| `:201` | **"Agendar"** (ação de linha) | abrir a Agenda com o formulário pré-preenchido | navega e depois avisa, por `alert`, que o formulário não existe | leva à Agenda; o alerta que prometia o que não existe saiu |
| `:212` | switcher **"Ver carteira de"** | trocar de CEN | aceita o clique e responde com `alert` dizendo que não há dado | os CENs sem carteira aparecem **desabilitados, com o motivo na linha** — o engano deixou de vir depois do gesto |
| `:190` | `window.confirm` ao escolher categoria | perguntar se registra mesmo assim | trava a tela num diálogo do navegador só para dar um recado | virou aviso embaixo do seletor: *"o contato fica registrado, mas não zera o contador"* |
| — | estados | — | `<div className="carteira-page">Carregando…</div>`; **o erro nem era lido** da camada de dados | `BlocoCarregando` / `BlocoErro` com tentar de novo |

### 2.4 Pipeline de Vendas — `telas/Pipeline.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:542` | **"Registrar contato"** (painel) | registrar contato | nada | saiu — o registro de contato mora na Cobertura de Carteira, que é onde ele tem efeito (documento 08) |
| `:543` | **"Adicionar nota"** | adicionar nota | nada | saiu — não existe nota |
| `:549` | **"Ver detalhes da aprovação"** | abrir a aprovação | nada | virou link para a ficha, e só aparece quando a oportunidade tem ficha |
| `:550` | **"Marcar como perdida"** | fechar como perdida | nada | abre o `DialogoConfirmacao` com o motivo de catálogo |
| `:141` | `window.confirm` ao soltar em "Ganho / Perdido" | perguntar o resultado | **"Cancelar" significava "perdido"** — quem largava o cartão sem querer e apertava Cancelar para escapar marcava a oportunidade como perdida | `DialogoConfirmacao`: diz o nome, explica o que a ação faz, e o botão de confirmar fica travado enquanto o motivo não for escolhido |
| `:150` | `window.prompt('Motivo da perda:')` | registrar o motivo | aceita texto livre, e o que a pessoa não digita vira *"Sem motivo registrado"* | virou `<select>` do catálogo `config-motivos-perda.json`, obrigatório (documento 05 §2) — é o que faz o relatório de vendas perdidas por motivo contar alguma coisa |
| `:389` | `<select>` Linha | filtrar | filtra, mas a lista de opções estava **escrita no componente** | montada a partir das oportunidades do quadro (padrão §2.2) |
| — | estados | — | dois `<div className="card">` sem ação | `BlocoCarregando` / `BlocoErro` |

### 2.5 Funil de Vendas — `telas/Funil.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:147` | **"Atualizar"** | reler | nada | relê, ignorando o cache |
| `:156` | **"Exportar"** | exportar | nada | exporta o detalhamento filtrado |
| `:164` | **"Salvar visão"** (ação primária) | salvar a visão | nada | saiu — não existe visão salva |
| `:177` | `<select>` Período | filtrar | nada | saiu — o arquivo tem um mês só |
| `:187` | `<select>` Regional | filtrar | nada | saiu — o arquivo não traz regional |
| `:198` | `<select>` Linha de produto | filtrar | nada, e a lista estava escrita no componente | filtra, com as opções montadas do dado |
| `:209` | `<select>` CEN | filtrar | idem | filtra por proprietário |
| `:218` | **"Limpar filtros"** | limpar | nada | limpa, e só aparece quando há filtro |
| `:316` | busca *"Buscar oportunidade, conta, modelo…"* | buscar | nada | busca |
| `:330` | **"Colunas"** | escolher colunas | nada | saiu |
| `:349` | subtítulo *"clique em uma linha para ver detalhes"* | abrir a oportunidade | as linhas não têm `onClick`, e o arquivo não traz identificador | a promessa saiu do texto |
| `:395` `:397` | paginação *"Página 1 de 47"* | paginar | os dois números eram fixos na marcação; "Anterior" nascia desabilitado e "Próxima" não fazia nada | saiu; o rodapé passou a dizer quantas linhas estão na tela e quantos registros o resumo apurou |
| `:118` | ordem dos estados | — | **o `if` de carregando vinha antes do de erro e testava `!dados`: toda falha ficava eternamente com cara de "Carregando…"** | erro primeiro, com tentar de novo |

### 2.6 Cobertura Regional — `telas/CoberturaRegional.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:54` | **"Atualizar"** | reler | nada | relê |
| `:63` | **"Exportar"** | exportar | nada | exporta o detalhamento por regional |
| `:71` | **"Personalizar"** (ação primária) | personalizar o painel | nada | saiu |
| `:88` `:98` `:108` | 3 `<select>` (período, segmentação, tipo de toque) | filtrar | nada | saíram — o `cobertura.json` é um retrato já somado de 120 dias, para a segmentação A+B: nenhum dos três teria um número diferente para mostrar |
| `:112` | **"Limpar filtros"** | limpar | nada | saiu com os filtros |
| `:143` `:187` `:234` `:260` `:292` | 5 menus **⋮** dos widgets | abrir um menu | nada | saíram |
| ×4 | **"Exibir relatório completo →"** | abrir o relatório completo | são `div`, não link, e não existe relatório mais completo que esta tela | saíram |
| — | subtítulo *"Clique em uma linha para drill-down"* | abrir a regional | não há tela de regional | a promessa saiu do texto |
| `:31` | estados | — | `!dados` mostrava só o título solto; **o erro nem era lido** | `BlocoCarregando` / `BlocoErro` |

### 2.7 Performance de CEN — `telas/PerformanceCen.tsx` e `componentes/performance/PerfRanking.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `PerformanceCen:111` | *"Clique no cabeçalho para ordenar"* | ordenar a tabela | os cabeçalhos eram `<th>` sem ação | **ordena**, com `<button>` dentro do `<th>` e `aria-sort` na coluna ativa (padrão §8) |
| `PerformanceCen:28` | ordem dos estados | — | mesmo defeito do Funil: carregando antes de erro, testando `!cens` | erro primeiro |

### 2.8 Ficha do Cliente — `telas/ClienteFicha.tsx` e `componentes/AbaPosVendas.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:125` | **"Ligar"** | ligar | nada | `tel:` do contato decisor |
| `:131` | **"E-mail"** | escrever | nada | `mailto:` do contato decisor |
| `:138` | **"Nova visita"** | agendar | nada | leva à Agenda |
| `:144` | **"Nova oportunidade"** (ação primária) | criar oportunidade | nada | leva a Nova Oportunidade |
| `:219` `:220` | **"Ver histórico"** e **"Cancelar solicitação"** | abrir/cancelar a solicitação de alteração | nada | saíram — cancelar uma solicitação é escrita |
| `:468` | **"Solicitar alteração cadastral"** | abrir a solicitação | nada | saiu, e no lugar ficou a frase que diz onde se altera cadastro de verdade: a tela Clientes |
| `AbaPosVendas:321` | **"Ver todas as OS"** | listar as OS | nada | saiu — não existe tela de OS |
| `AbaPosVendas:375` | **"Histórico de PMPs concluídas"** | abrir o histórico | nada | saiu — o arquivo traz só as pendentes |
| `AbaPosVendas:492` | **"Criar proposta JDCP"** | criar proposta | nada | virou o caminho que existe: Nova Oportunidade |
| `:65` | ordem dos estados | — | carregando antes de erro, testando `!cliente` | erro primeiro |

### 2.9 Ficha do Equipamento — `telas/EquipamentoFicha.tsx`

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:100` | **"Copiar chassi"** | copiar | nada | copia; o rótulo confirma por dois segundos |
| `:107` | **"Exportar histórico"** | exportar | nada | exporta as revisões |
| `:115` | **"Ver no JD Connect"** | abrir o portal | nada | saiu — não há endereço do portal por chassi nesta base |
| `:121` | **"Abrir chamado"** (ação primária) | abrir chamado | nada | saiu — não existe tela de chamado |
| `:200` `:201` | **"Ver chamado"** e **"Reagendar"** | abrir/remarcar o chamado | nada | saíram |
| `:499` | **"Exportar CSV"** (peças) | exportar | nada | exporta as peças |
| `:575` | **"Abrir no JD Connect"** (telemetria) | abrir o portal | nada | saiu |
| `:48` | ordem dos estados | — | carregando antes de erro | erro primeiro |

### 2.10 Ficha de Oportunidade — `telas/OportunidadeFicha.tsx`

**Era a tela com mais pontas soltas de todas: dezoito.** Cinco ações no cabeçalho, uma de cobrar
aprovação, duas de itens, quatro por aprovação pendente, cinco por documento e uma de exportar.

| Linha | Elemento | O que promete | Conserto |
|---|---|---|---|
| `:113` `:116` `:119` `:122` `:125` | **Editar**, **Gerar proposta PDF**, **Enviar para aprovação**, **Marcar como perdida**, **Avançar para Pedido Alocado** | escrever na oportunidade | saíram. As duas ações que existem no produto — avançar de fase e fechar — moram no **Pipeline**, e o cabeçalho leva até lá |
| `:207` | **"Cobrar aprovação"** | notificar o aprovador | saiu — o ramal dele está escrito acima, que é o que dá para fazer hoje |
| `:386` `:389` | **"Importar do Protheus"**, **"Adicionar item"** | escrever na proposta | saíram |
| `:574` `:577` `:580` `:583` | **Aprovar**, **Rejeitar**, **Comentar**, **Escalar para Diretor** | decidir a aprovação | saíram. **Eram os botões mais perigosos da aplicação**: quem clicasse em "Aprovar" veria a tela não responder e ficaria sem saber se aprovou. No lugar, a frase que diz com quem está a pendência |
| `:623` `:624` `:629` `:630` `:634` `:638` | **Baixar**, **Substituir**, **Reenviar**, **Anexar** ×2, **Marcar como aplicável** | mexer nos documentos | saíram. Um "Baixar" que não baixa, numa lista de contratos, é a pior promessa da tela |
| `:713` | **"Exportar CSV"** (histórico) | exportar | **exporta** |
| `:59` | ordem dos estados | — | erro primeiro |

### 2.11 Mapa do protótipo — `telas/Inicio.tsx`

Ver a decisão inteira na seção 4.

| Linha | Elemento | O que promete | O que faz | Conserto |
|---|---|---|---|---|
| `:66` | selo **"Planejada"** em `/equipamentos` | que a tela não existe | `/equipamentos` é hoje uma das duas telas mais completas, ligada à API | os selos passaram a dizer a distinção que importa: **lê e grava na API** · **lê JSON do protótipo** · **ferramenta de desenvolvimento** |
| `:59` `:62` `:71` | selos **"Pronta"** | que está tudo igual | não distinguiam o que lê o banco do que ainda lê JSON | idem |
| `:143` | KPI *"Telas planejadas 9 / 6-10"* | contar as telas | número fixo, parado desde agosto; são quinze rotas | contado da tabela de rotas |
| `:171` | KPI *"Mobile fora do escopo do protótipo"* | — | responder fora do desktop virou requisito no documento 05 §3 | corrigido |
| `:216` | *"não persiste dados, não conecta ao Protheus"* | — | Clientes e Equipamentos gravam no banco | corrigido |

---

## 3. Cosméticos — listados e deixados, com o motivo

| Onde | O que é | Por que fica |
|---|---|---|
| `estilos/design-system.css:6777` | `.tab-badge-red:has-text("0")` — seletor que **não existe em CSS**; o `lightningcss` reclama a cada build | é do CSS do protótipo original. A regra simplesmente não se aplica; nada na tela depende dela. Apagar mexe no arquivo que a comparação visual protege, para ganhar só o silêncio de um aviso de build |
| `telas/Inicio.tsx` · `/relatorios/cobertura` | o título do card diverge do título canônico da rota | é cópia literal do original, e o comentário no código explica: usar o título da rota quebra o `<h4>` em duas linhas e estica o card vizinho. Trocar piora o que está bom |
| `componentes/config/ConfigSecaoAtalhos.tsx` | os seis atalhos de teclado listados (`Ctrl+Shift+O`, `G A`…) não existem | é uma **lista de referência**, não um controle: ninguém aperta um `<kbd>` numa tabela esperando efeito. Ganhou a linha *"ainda não estão ligados"*, que é o que faltava. Implementar os seis é trabalho de produto, não de conserto — e o `Ctrl+K`, que era o único também anunciado num controle de verdade, foi implementado |
| `telas/PerformanceCen.tsx` · `PerfContexto` | os três blocos de contexto de persona são texto fixo, não vêm de `PERF_CENS` | são fixos no original também. São rótulos de leitura, não prometem ação |
| `telas/CoberturaRegional.tsx` | a nota *"Regionais placeholder: os nomes não são confirmados"* | **já diz a verdade sobre si mesma** — é o oposto de uma ponta solta, e some quando o workshop confirmar os nomes |
| `componentes/painel360/PainelExecutivo.tsx` | estoura 1.280px de largura (a página fica com 1.573px) | **medido também no protótipo original, com o mesmo número**: é defeito herdado, não regressão. Está registrado no documento 05 §8 e a instrução foi preservar os painéis executivos como estão |
| `telas/TelaEmConstrucao.tsx` | arquivo órfão — nenhuma rota o usa desde que as quinze telas foram portadas | não aparece para ninguém, então não engana ninguém. Apagar é limpeza de código, não conserto de tela; fica para a rodada que passar o pente no que sobrou do porte |

---

## 4. O caso do `telas/Inicio.tsx` — a decisão, e por quê

**Ela fica, fora do menu, e passou a se declarar ferramenta de desenvolvimento.**

`Inicio.tsx` não é tela de produto: é o índice navegável das quinze rotas, feito para abrir o
protótipo numa apresentação e conferir o que existe. Nenhum CEN, gerente ou diretor tem trabalho a
fazer nela. Ela já estava fora do menu lateral — só se chega por `#/inicio-antigo` — e continua
assim.

**Por que não apagar.** É o único inventário navegável das quinze telas, e é usado exatamente nas
apresentações à diretoria de que o documento 05 fala. O custo de mantê-la é uma rota que ninguém vê.

**Por que não promover a tela inicial.** Seria criar uma quinta tela mostrando as mesmas coisas — o
oposto do que este passo veio resolver. A porta de entrada do produto é a Visão 360, e é ela que
está no menu (documento 08).

**O que era ponta solta nela, e foi consertado.** Um índice que mente sobre o próprio estado é pior
que índice nenhum, porque é para isso que se olha para ele. Os selos estavam parados no que era
verdade em agosto — `/equipamentos` como *"Planejada"* quando é hoje uma das duas telas ligadas ao
banco — e os contadores eram números fixos. Os selos passaram a dizer a distinção que importa hoje
(**lê e grava na API** · **lê JSON do protótipo** · **ferramenta de desenvolvimento**), os números
vêm da tabela de rotas, e o topo da tela diz, na primeira linha, que quem vem trabalhar entra pela
Visão 360.

---

## 5. O que este documento mediu, em números

| | |
|---|---|
| Telas varridas | 13 + o cabeçalho comum |
| **engana e perde dado** | **3 achados · 3 consertados** |
| **engana** | **61 achados · 61 consertados** — 24 fazendo funcionar, 37 tirando o controle |
| **cosmético** | 7 achados · listados, com o motivo de ficarem |
| Controles que passaram a funcionar | busca global, `Ctrl+K`, 11 filtros, 2 buscas de tabela, 5 exportações em CSV, 2 "Atualizar", ordenação do ranking, copiar chassi, criar tarefa, registrar contato, fechar oportunidade com motivo de catálogo, 4 ações da ficha do cliente |
| Controles retirados | 37 — cada um com o comentário no código dizendo o que era e por que saiu |

**O critério de "tirar" foi um só:** o controle não tem destino no produto de hoje, e fazê-lo
funcionar significaria inventar uma tela, um serviço ou uma regra de negócio que ninguém decidiu.
Onde havia destino, ele foi ligado.
