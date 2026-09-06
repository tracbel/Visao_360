# As melhorias combinadas — o que muda depois do porte

> Decidido com o Ricardo em 03/09/2026, depois de ver o protótipo local rodando.
> **A identidade visual fica.** O protótipo do gerente é o alvo aprovado: mesma paleta John Deere,
> mesma tipografia, mesma estrutura de navegação. O que muda é o que falta para virar produto e o
> que a evidência da pesquisa mostra que o usuário precisa.

Instrução literal: *"vamos manter mas melhorando o visual, usabilidade das pessoas, deixar algo
fácil mas com esse visual. E um painel 360 mas em forma de CRM. Vamos integrar o Protheus. Também
quando tiver que dar seguimento em um formulário do CEN, a maioria dos campos vão puxar dados de
tabelas e não coisa manual escrita."*

---

## 1. A Visão 360 vira o 360 do cliente

Hoje a tela inicial é um painel executivo: faturamento, cobertura, ranking, mix. É um relatório
bonito. O pedido é outro — **um painel de CRM**, centrado no cliente, de onde se trabalha.

| Hoje | Passa a ser |
|---|---|
| Números da regional e ranking de CENs | O cliente no centro: quem é, em que carteira está, qual a frota |
| Gráficos que só se olham | Blocos que levam à ação: abrir oportunidade, registrar interação, agendar visita |
| Nenhum caminho para o registro | Cada bloco é um atalho para a ficha, a oportunidade ou a tarefa |

**O que a tela precisa mostrar de um cliente, numa página:** identificação e situação, carteira e
CEN responsável, frota instalada com horímetro e próxima manutenção, últimas interações com
resultado, oportunidades abertas e o estágio de cada uma, faturamento dos últimos doze meses,
títulos em aberto e vencidos, ordens de serviço recentes, documentos do processo, e os alertas que
pedem ação — cliente classe A sem visita há noventa dias, título vencido, revisão programada
próxima.

Os painéis gerenciais **não somem**: viram uma visão separada, para quem é gerente ou diretor. O
seletor de perfil que já existe continua sendo o que troca entre elas.

**A evidência que sustenta isso:** o Vórtice já mostra que o CEN não usa o CRM como ferramenta de
campo. O pico de uso é às 17h de sexta-feira, ou seja, ele preenche depois. Um painel que exige
navegar por seis telas para entender um cliente empurra para esse comportamento. Um 360 que responde
"quem é esse cliente e o que eu faço agora" muda o incentivo.

---

## 2. Formulário que puxa de tabela

Nenhum campo com catálogo aceita digitação. Vale na tela, na interface de programação e no banco.

| Campo | De onde vem |
|---|---|
| Motivo de perda, resultado da tarefa, tipo de tarefa, categoria de interação | catálogo do processo |
| Marca, família, modelo do equipamento | catálogo da frota |
| Linha de negócio, praça, classe do cliente, situação | catálogo da organização |
| Cultura, concorrente, condição de pagamento | catálogo comercial |
| Cliente, contato, equipamento, oportunidade | busca com sugestão, nunca texto solto |

O que continua livre é a exceção justificada: a observação do que foi conversado. E mesmo ela passa
pela normalização de texto.

**Por que isso é usabilidade, não só banco:** escolher numa lista é mais rápido que digitar, não
erra, e permite o preenchimento automático do resto. Quando o CEN escolhe o modelo, o sistema já
sabe a família, a marca e a faixa de preço. É o que transforma "dar seguimento" em três toques.

**Os dados dos catálogos** estão sendo montados em `dados-referencia/`, tirados do próprio protótipo
e dos catálogos reais do Vórtice.

---

## 3. Usabilidade — o que falta para virar produto

O protótipo é uma demonstração e assume o caminho feliz. Um produto não pode.

| Falta | O que fazer |
|---|---|
| **Funcionar fora do desktop** | Hoje é fixo em 1280 pixels. O CEN trabalha em campo: no Vórtice houve 18.360 alterações de agenda pelo celular em noventa dias, com cinquenta usuários. A agenda, a ficha do cliente e o registro de interação precisam funcionar em celular e tablet |
| **Estado de carregando** | Nenhuma tela mostra que está buscando. Com dado vindo do banco e do Protheus, isso deixa de ser detalhe |
| **Estado vazio** | Carteira sem cliente, cliente sem oportunidade, agenda sem tarefa. Cada um com a ação que faz sentido ali |
| **Estado de erro** | Inclusive o mais importante: **dado velho aparece marcado como velho**. Se a última carga do Protheus falhou, a tela diz isso. É a lição direta dos 17 meses de faturamento congelado sem ninguém notar |
| **Teclado e leitor de tela** | Navegação por tabulação, foco visível, rótulo em todo campo, contraste conferido |
| **Botão que não faz nada** | O protótipo tem ações decorativas. Ou funcionam, ou saem |
| **Confirmação e desfazer** | Nada destrutivo sem confirmação; o que der para desfazer, desfaz |
| **Busca que encontra** | A busca do topo hoje é enfeite. Precisa achar cliente por nome, apelido, CNPJ, cidade e chassi, com tolerância a erro de digitação |

---

## 4. O prazo que não existe

Achado da pesquisa que vira requisito de tela: **nenhuma das 170 ações em uso no Vórtice tem prazo
definido**. Por isso 44% do passivo de agenda está em conta de usuário morto e ninguém percebe.

No CRM novo, toda tarefa nasce com prazo vindo da política do tipo de tarefa, e a tela mostra o
prazo, o quanto falta e o que está vencido. A agenda ordena por urgência real, não por data de
criação.

---

## 5. O que o Protheus muda na tela

Com a integração, alguns campos deixam de ser digitados e passam a ser lidos: cadastro fiscal do
cliente, faturamento, títulos, ordens de serviço, produtos e equipamentos faturados.

Regra de tela que vem junto: **campo que é do Protheus não é editável no CRM**, e mostra de onde
veio e quando foi atualizado. Quem precisa corrigir, corrige na origem. É o que impede o CRM de
virar a terceira versão da verdade, como aconteceu no legado com o cadastro de pessoa em sete
cópias.

---

## 6. Ordem de execução

1. Terminar o porte das quinze telas, com a comparação visual verde.
2. Subir o banco com os catálogos carregados.
3. Ligar as telas ao banco, trocando só a camada de dados.
4. Refazer a Visão 360 como painel do cliente.
5. Converter os campos de digitação em seleção de catálogo.
6. Estados, responsividade e acessibilidade, tela por tela.
7. Integração com o Protheus, com o indicador de dado velho.

Cada passo mantém a comparação visual como rede de proteção: o que não deveria mudar, não muda.

---

## 7. Dívida técnica registrada durante o porte — **quitada em 04/09/2026**

**Componentes de gráfico alteravam a fonte padrão global do Chart.js.**
`GraficoBarraCobertura`, `GraficoBarraVendasPerdidas` e `GraficoGauge` executavam
`ChartJS.defaults.font.family = "'Inter', sans-serif"` no carregamento do módulo, o que valia para
todos os gráficos da aplicação. O protótipo original nunca altera esse padrão. O efeito medido: na
Visão 360, a Inter mais larga fazia o Chart.js reservar mais espaço para o rótulo e a primeira letra
saía cortada ("reço acima do concorrente" em vez de "Preço").

**Como foi resolvido:** os três componentes foram reescritos como desenho manual em `canvas`, porte
literal das funções do original, e o gráfico de barras horizontais da Visão 360 passou a declarar a
própria fonte. Nenhum componente altera mais estado global. A Visão 360 caiu de 0,11% para **0,00%**
de diferença.

---

## 8. A Visão 360 saiu da comparação com o protótipo — **feito em 04/09/2026**

O passo 4 da ordem de execução ("refazer a Visão 360 como painel do cliente") foi executado. Com
ele, a rota `/` deixa de ser comparável com a captura de referência, **de propósito**.

### O que mudou

O seletor de perfil parou de trocar três variações do mesmo painel e passou a trocar **duas telas
diferentes**:

| Perfil | O que abre |
|---|---|
| **CEN** (padrão) | `PainelCen` — o painel de CRM, em duas camadas: "o meu dia" e o 360 do cliente |
| **Gerente Regional** e **Diretora Comercial** | `PainelExecutivo` — o painel executivo de antes, preservado sem uma linha alterada |

**Camada 1, "o meu dia"** (nenhum cliente escolhido): busca de cliente em destaque; três cartões de
ação pendente (tarefas vencidas, tarefas de hoje, aprovações esperando); a fila correspondente, com
**prazo, e quanto falta ou quanto passou, em toda tarefa**; a lista de clientes que pedem ação, cada
um com o botão que abre o 360; e os alertas da carteira cuja ação tem tela.

**Camada 2, o 360 do cliente**: alertas · identificação e situação · carteira e responsáveis · frota
instalada com horímetro e próxima manutenção · oportunidades abertas com a fase · últimas interações
com o resultado · faturamento dos últimos doze meses · títulos em aberto e vencidos · documentos do
processo · ordens de serviço recentes.

### Por que a comparação de `/` cai

A comparação pixel a pixel media "o que não deveria mudar, não mudou". A Visão 360 **deveria** mudar
— era o passo 4. A captura de referência (3134x3962) e a tela nova (2560x7136) não têm sequer o
mesmo tamanho; comparar as duas mede a mudança que foi pedida, não um defeito.

A captura da tela nova fica em [`capturas-atual/visao-360.png`](capturas-atual/visao-360.png), como
o novo ponto de partida.

**As outras catorze rotas continuam na comparação e não pioraram** — os números batem exatamente com
os registrados em `RETOMAR.md`:

```
/agenda                          0,01%    /oportunidades/nova     0,02%
/cobertura                       0,01%    /equipamentos           0,02%
/pipeline                        0,01%    /relatorios/funil       0,01%
/clientes                        0,01%    /relatorios/performance 0,02%
/clientes/84391                  0,04%    /relatorios/cobertura   0,07%
/equipamentos/1RW7250PVMR123456  0,06%    /config                 0,01%
/oportunidades/1517613           0,04%    /inicio-antigo          0,01%
```

### O que a tela nova cumpre desta lista

| Item | Onde |
|---|---|
| §1 — a Visão 360 vira o 360 do cliente | as duas camadas do `PainelCen` |
| §3 — carregando, vazio e erro | `BlocoPainel`: todo bloco tem os quatro estados |
| §3 — funcionar fora do desktop | o painel do CEN responde em 1024px, 900px e 640px; a barra lateral vira faixa de navegação no celular |
| §3 — teclado e leitor de tela | busca é `combobox` com seta/Enter/Esc, foco visível, rótulo em todo campo, cabeçalho de tabela e legenda para leitor de tela |
| §3 — botão que não faz nada | só aparece botão onde existe tela; alerta sem rota não ganha botão |
| §3 — confirmação e desfazer | concluir tarefa é reversível na própria linha |
| §4 — o prazo que não existe | `PRAZO_DIAS_POR_TIPO` em `dados/painel360.ts`; a fila ordena por urgência real |
| §5 — o que o Protheus muda na tela | `SeloFonte`: bloco carimbado com a data da carga, campo marcado "somente leitura", e **dado velho aparece marcado como velho** acima de 7 dias |

### O que ficou de fora, por falta de dado no protótipo

- **A lista de títulos.** O protótipo tem o resumo financeiro do cliente (limite, uso, inadimplência,
  maior atraso, última fatura), não os títulos um a um. O bloco mostra o resumo e diz que a lista vem
  do Protheus.
- **O faturamento de máquinas mês a mês.** Existe o total anual na ficha e a série mensal de peças e
  serviços no pós-vendas. O gráfico é o que existe, e declara o que está mostrando.
- **Registrar interação a partir do bloco.** Não há tela de registro de interação no protótipo, então
  não há botão — pela regra de que botão que aparece funciona.
- **Ficha rica para os outros 22 clientes da carteira.** Só o cliente 84391 tem cadastro completo,
  frota, pós-vendas e oportunidade. Para os demais, cada bloco mostra o estado vazio dizendo de qual
  tabela o dado virá.

### Um defeito antigo que continua lá, de propósito

Nos perfis Gerente e Diretora, o painel executivo estoura 1.280px de largura (a página fica com
1.573px) porque a coluna de alertas tem botão que não quebra linha e o gráfico de faturamento tem
canvas de 600px fixos. **Medido também no protótipo original, com o mesmo número**: é defeito
herdado, não regressão. Como a instrução foi preservar os painéis executivos como estão, ele não foi
tocado — fica registrado aqui para a rodada que for cuidar dessas duas telas.

---

## 9. Clientes e Equipamentos saíram da comparação com o protótipo — **feito em 05/09/2026**

Os passos 3 e 5 da ordem de execução ("ligar as telas ao banco, trocando só a camada de dados" e
"converter os campos de digitação em seleção de catálogo") foram executados nas duas primeiras
telas. Com eles, as rotas `/clientes` e `/equipamentos` deixam de ser comparáveis com as capturas de
referência, **de propósito** — pela mesma razão que a Visão 360 saiu na seção 8.

### O que mudou

| Rota | Antes | Agora |
|---|---|---|
| `/clientes` | 52 clientes montados em memória de três JSONs, com filtros de persona | listagem de `comercial.Cliente` pela API, com busca, filtro, ordenação e paginação **do servidor**, dentro da filial do cabeçalho |
| `/clientes/novo` | não existia | cadastro |
| `/clientes/{chave}` | não existia | ficha, alteração e inativação com motivo de catálogo |
| `/equipamentos` | **marcador de 45 linhas** dizendo "envie os detalhes desta tela" | listagem de `frota.Equipamento`, com busca por chassi, número de série e placa, e filtro por marca, família, modelo, situação e origem |
| `/equipamentos/novo` | não existia | cadastro |
| `/equipamentos/{chave}` | não existia | ficha, alteração e baixa |

O padrão que emergiu dessas duas telas está escrito em
[`07-PADRAO-DE-TELA.md`](07-PADRAO-DE-TELA.md) e é o que o passo 3 aplica nas demais.

### Por que a comparação das duas cai

`/equipamentos` era um **marcador**: comparar a lista de verdade com a captura de "tela aguardando
specs" mede a construção que foi pedida, não um defeito. `/clientes` trocou a base de 52 linhas de
JSON pelo cadastro real da filial — os números, as colunas e a paginação são outros porque o dado é
outro.

As duas capturas novas ficam em
[`comparacao/clientes-novo.png`](comparacao/clientes-novo.png) e
[`comparacao/equipamentos-novo.png`](comparacao/equipamentos-novo.png), como o novo ponto de
partida.

O script ainda mede as duas — `/clientes` cai em *"tamanho diferente"* e `/equipamentos`, por
coincidência de altura, sai como **1,98%** —, e nos dois casos **o número não significa nada**: ele
mede a construção que foi pedida. Quando as capturas de referência forem refeitas, as duas linhas
voltam a valer.

### As outras treze rotas

Medição de 05/09/2026, com a API e o banco no ar:

```
/agenda                          0,11% (1)   /oportunidades/nova     0,02%
/cobertura                       0,01% (2)   /relatorios/funil       0,01%
/pipeline                        0,01%       /relatorios/performance 0,02%
/clientes/84391                  0,04%       /relatorios/cobertura   0,07%
/equipamentos/1RW7250PVMR123456  0,06%       /config                 0,01%
/oportunidades/1517613           0,04%       /inicio-antigo          0,01%
/                                fora da comparação desde a seção 8
```

Onze das doze rotas medidas batem **exatamente** com os números da seção 8 e do `RETOMAR.md`.

**(1) `/agenda` está em 0,11% e não nos 0,01% registrados, e a diferença é anterior a esta
rodada.** A causa foi medida: o terceiro botão de cada linha de tarefa ("Mais opções") **foi
removido de propósito** numa rodada anterior — o comentário está em `telas/Agenda.tsx`, logo abaixo
do botão de reagendar: *"era um botão vazio (os `<circle>` estavam fora de um `<svg>`) e sem
ação"* —, e a captura de referência ainda tem o botão vazio. Os ~23 mil pixels de diferença são o
retângulo desse botão em cada uma das linhas.

A prova de que não é regressão desta rodada: medida a rota sozinha, `/agenda` dá **25.198 px com o
bloco de CSS novo e os mesmos 25.198 px com ele removido** do `design-system.css`. O número não muda porque a
diferença é de marcação, não de estilo. A captura de referência precisa ser refeita para essa rota,
ou o botão volta — decisão de quem cuidar da Agenda.

**(2) `/cobertura` oscila entre 0,01% e 2,15% entre execuções**, e a causa também não é desta
rodada: a tela tem o mapa do Leaflet, que busca os ladrilhos pela rede. Quando eles não chegam a
tempo do `waitForTimeout` do script, o mapa sai cinza e a diferença explode. Medido três vezes
seguidas nesta sessão: 2,15% · 0,01% · 0,01%. **Antes de tratar um número dessa rota como
regressão, rode a comparação de novo.**

### As telas que continuam lendo JSON não foram tocadas

`dados/carregar.ts` e `dados/useDados.ts` seguem sem uma linha alterada, e a origem nova vive em
`dados/api/`. As duas convivem durante a transição. As fichas ricas do protótipo — `/clientes/84391`
(cadastro completo, frota, pós-vendas, financeiro) e `/equipamentos/1RW7250PVMR123456` (telemetria,
revisões, garantia, peças) — **continuam lendo JSON e continuam na comparação**: nenhum desses
blocos existe na API, e reescrevê-los agora seria inventar dado. Unificá-las com as fichas novas é o
passo 3, e é exatamente a queixa de "telas demais mostrando a mesma coisa".

---

## 10. As treze telas restantes saíram da comparação — **feito em 05/09/2026**

O passo 3 aplicou nas demais telas o padrão escrito em
[`07-PADRAO-DE-TELA.md`](07-PADRAO-DE-TELA.md), varreu as pontas soltas
([`06-ACOES-SEM-EFEITO.md`](06-ACOES-SEM-EFEITO.md)) e decidiu a navegação
([`08-NAVEGACAO.md`](08-NAVEGACAO.md)). Com ele, **as quinze rotas saem da comparação pixel a
pixel** — as treze desta rodada, mais as duas que já tinham saído nas seções 8 e 9.

### 10.1 Por que as quinze caem de uma vez, e não só as treze

Duas mudanças são do **cabeçalho comum**, e o cabeçalho aparece em toda captura:

- a **busca global** era um campo decorativo em todas as telas — o pedido literal do §3 desta lista
  (*"a busca do topo hoje é enfeite"*) — e passou a levar o termo à tela Clientes;
- os botões de **notificações** e de **sincronização** saíram, com o ponto vermelho de "não lido"
  que não contava nada.

E uma é do CSS: a **dívida P-1** do documento 07, §10, que aquele documento já atribuía a este
passo. Uma regra do bloco de Pipeline redefinia `.btn-secondary` **sem escopo nenhum**, usando
`var(--bg-primary)` e `var(--border-primary)`, que não existem em `:root`. Variável indefinida
invalida a declaração, e a `border` voltava a `border-style: none`: **em toda a aplicação o botão
secundário perdia a borda e virava texto solto**, e ainda herdava dali um `padding` e um `font-size`
menores. A regra recebeu o escopo que sempre deveria ter tido (`.pipeline-filtros`), e com isso a
remenda `.conteudo-cadastro .btn-secondary`, que restaurava a borda só nas telas de cadastro, foi
apagada.

**Nenhuma das quinze mudou sem intenção.** As treze eram a tarefa; `/`, `/clientes` e
`/equipamentos` já estavam fora desde as seções 8 e 9.

### 10.2 O que mudou em cada tela

| Rota | O que mudou |
|---|---|
| **cabeçalho** (todas) | busca global funciona com `Ctrl+K` e leva a Clientes · notificações e sincronização saíram |
| `/agenda` | os 4 filtros filtram · "Limpar filtros" só aparece com filtro · **"Nova tarefa" cria tarefa**, com cliente da carteira e prazo da política do tipo (§4) · estados do padrão |
| `/cobertura` | **registrar contato grava e muda a tela** — a última interação, o contador e os KPIs · exportar CSV de verdade · CENs sem carteira desabilitados com o motivo · o `confirm` do navegador virou aviso na tela · estados do padrão |
| `/pipeline` | fechar oportunidade usa `DialogoConfirmacao` com **motivo de catálogo** no lugar de `confirm` + `prompt` · "Marcar como perdida" funciona · "Registrar contato" e "Adicionar nota" saíram (o registro mora na Cobertura) · filtro de linha montado do dado · estados do padrão |
| `/relatorios/funil` | 2 filtros reais no lugar de 4 decorativos · busca da tabela funciona · Atualizar e Exportar funcionam · "Salvar visão", "Colunas" e a paginação falsa saíram · estados do padrão |
| `/relatorios/performance` | **o ranking ordena**, como o subtítulo sempre prometeu · estados do padrão |
| `/relatorios/cobertura` | Atualizar e Exportar funcionam · Personalizar, 3 filtros, 5 menus ⋮ e 4 "Exibir relatório completo" saíram · estados do padrão |
| `/clientes/84391` | as 4 ações do cabeçalho ganharam destino (`tel:`, `mailto:`, Agenda, Nova Oportunidade) · o lápis "Editar bloco" saiu de todos os blocos · seta e cartão só clicáveis onde existe ficha · declara que é leitura e que o cadastro é em Clientes |
| `/equipamentos/1RW…` | copiar chassi e exportar funcionam · "Ver no JD Connect", "Abrir chamado", "Ver chamado" e "Reagendar" saíram · estados do padrão |
| `/oportunidades/1517613` | **os 18 botões que não faziam nada saíram** · declara que é leitura e leva ao Pipeline · exportar histórico funciona · estados do padrão |
| `/oportunidades/nova` | **Cancelar não descarta mais em silêncio**: confirmação + `beforeunload` · o quadro "Impacto ao salvar" parou de dizer "Qualificação" com outra fase escolhida · estados do padrão |
| `/config` | **"Salvar alterações" salva** e o aviso diz onde · Cancelar reverte · os botões só habilitam com alteração pendente · trocar de seção pergunta antes de descartar · busca e filtros de auditoria e de usuários funcionam · 9 botões sem destino saíram · estados do padrão |
| `/inicio-antigo` | virou explicitamente o **mapa do protótipo**, ferramenta de desenvolvimento, com os selos de status corrigidos e os contadores tirados da tabela de rotas |
| `/` | painel executivo: alerta sem rota deixou de ter botão, em vez de responder com `alert` |

### 10.3 A medição de 05/09/2026, depois do passo 3

```
/                                tamanho diferente   (fora desde a seção 8)
/clientes                        tamanho diferente   (fora desde a seção 9)
/equipamentos                    2,57%               (fora desde a seção 9)
/agenda                          0,19%
/cobertura                       0,05%  (1)
/pipeline                        0,19%
/oportunidades/nova              0,10%
/clientes/84391                  tamanho diferente
/equipamentos/1RW7250PVMR123456  tamanho diferente
/oportunidades/1517613           tamanho diferente
/relatorios/funil                tamanho diferente
/relatorios/performance          tamanho diferente
/relatorios/cobertura            tamanho diferente
/config                          tamanho diferente
/inicio-antigo                   tamanho diferente
```

**Todos os números são a construção que foi pedida, não defeito.** As capturas de referência
precisam ser refeitas para as quinze rotas; quando forem, a comparação volta a valer como rede de
proteção — e volta a valer inteira, porque a partir daqui nenhuma tela tem controle decorativo para
a próxima rodada remover.

**(1) `/cobertura` continua exigindo medição isolada.** Em lote ela deu 2,18%; medida sozinha, três
vezes seguidas, **0,05% estáveis**. A causa é a mesma registrada na seção 9, nota (2): o mapa do
Leaflet busca os ladrilhos pela rede e, quando eles não chegam a tempo do `waitForTimeout` do
script, o mapa sai cinza. **Antes de tratar um número dessa rota como regressão, rode a comparação
de novo, sozinha.**

### 10.4 A verificação, com o que ela exercitou

Não bastou a tela abrir: **58 verificações percorreram os controles consertados com o Playwright, e
as 58 passaram** — `Ctrl+K` põe o foco na busca e `Enter` leva o termo preenchido a Clientes; o
filtro de classe da Agenda muda a lista e volta ao limpar; a nova tarefa barra sem cliente, sugere o
título, mostra o prazo, entra na lista e sai no desfazer; o registro de contato barra sem categoria,
recalcula o KPI de cobertura e volta no desfazer; o fechamento no Pipeline trava o confirmar
enquanto não houver motivo de catálogo; o estado vazio do Funil aparece e o botão dele limpa; o
ranking muda de ordem e marca `aria-sort`; Configurações nasce com o salvar desabilitado, grava, e o
valor sobrevive ao recarregar; Cancelar na Nova Oportunidade pergunta e mantém o texto; o chassi vai
mesmo para a área de transferência; e as cinco exportações baixam arquivo.

**As quinze rotas abrem sem um único erro de console** e nenhuma rola na horizontal (todas em
1.280px). **Clientes e Equipamentos continuam de pé**, listando pela API com o seletor de filial.

### 10.5 O que este passo cumpre desta lista

| Item | Onde |
|---|---|
| §3 — **botão que não faz nada** | 64 achados, 64 tratados: 24 passaram a funcionar, 37 saíram, 3 eram perda de dado. Documento 06 |
| §3 — **confirmação e desfazer** | fechar oportunidade, criar tarefa, registrar contato, descartar formulário e trocar de seção com pendência — todos com confirmação; criar tarefa e registrar contato com desfazer |
| §3 — **carregando, vazio e erro** | as treze telas usam `BlocoCarregando`/`BlocoVazio`/`BlocoErro`, e **quatro delas tinham o `if` de erro depois do de carregando**: toda falha de leitura aparecia como "Carregando…" para sempre |
| §3 — **busca que encontra** | a busca do cabeçalho leva a Clientes, que busca na API |
| §2 — **formulário que puxa de tabela** | o motivo de perda do Pipeline virou catálogo (era `window.prompt` de texto livre); a próxima ação da Cobertura e o cliente da nova tarefa também |
| §4 — **o prazo que não existe** | toda tarefa criada na Agenda nasce com o prazo da política do tipo, e a tela mostra quando vence |
| §5 — **o que o Protheus muda na tela** | o que vem do AD em Configurações deixou de ser editável e passou a dizer que se corrige na origem |

### 10.6 O que ficou como proposta, e não foi executado

Estão no documento 08, seção 5, cada uma com o que falta para decidir:

1. **fundir a camada 2 da Visão 360 com a Ficha do Cliente** — depende do Protheus (passo 7);
2. **tirar `/clientes/84391` de dentro de `/clientes/*`** — anda junto com a anterior;
3. **absorver "clientes que pedem ação" na Cobertura, ou o contrário** — depende de medir uso real;
4. **renomear a seção "Executivo" do menu** — uma palavra, mas move as quinze capturas de uma vez;
   vale juntar com a próxima mudança que mexer no menu.

**Nenhuma tela foi fundida, apagada nem mudou de endereço nesta rodada.** A regra seguida foi a que
veio com a queixa: fundir errado custa mais que conviver mais uma rodada.

---

## 11. As telas restantes ligadas ao dado real — **05/09/2026**

O passo 3 tinha aplicado o **padrão visual** nas treze telas; elas continuavam lendo os JSON do
protótipo. Esta rodada troca a **origem do dado**: cada tela abaixo passa a ler o banco do CRM
pela API, com a carga de 2026 dentro — **23.945 clientes · 45.402 processos · 103.346 tarefas ·
121.986 interações · 142 carteiras · 9.750 municípios**.

A instrução literal foi *"todas as nossas telas precisam pegar dados reais para que possamos ter
métricas reais"* e *"nosso app ainda está com dados fictícios, tire tudo de fictício"*.

### 11.0 A regra que não se quebra, e como ela aparece na tela

**Nenhuma tela mostra número que não venha do banco.** Onde o dado não sustenta a métrica, a tela
diz que não há dado **e por quê**, com o número medido do lado — nunca um valor plausível.

São dois componentes, e a diferença entre eles importa:

| Componente | O que mostra | De onde vem o texto |
|---|---|---|
| `MetricasSemDado` | o que a **API mediu nesta requisição** (`metricasSemDado`) | inteiro da resposta, com a contagem apurada na mesma consulta. Quando o dado melhorar, o texto muda sozinho, sem release |
| `LacunaConhecida` | o que **não tem rota nenhuma** para pedir | escrito, porque não há consulta que o produza — mas com a data e o número que os documentos 23 §6.1 e 25 §9 mediram |

### 11.1 O conserto de largura que valia para a aplicação inteira

A queixa era *"cartões que não se reorganizam em tela estreita e conteúdo que sai pela borda"*. A
causa estava em uma linha do `index.html`:

```html
<meta name="viewport" content="width=1280" />   <!-- antes -->
<meta name="viewport" content="width=device-width, initial-scale=1" />   <!-- agora -->
```

Com `width=1280`, o navegador do celular desenhava a página como se a tela tivesse 1.280 pixels e
depois encolhia tudo. **Nenhuma regra `@media` de largura estreita chegava a valer** — as telas
nunca souberam ser estreitas porque nunca lhes foi contado que a tela era estreita. Em 1.280px o
resultado é idêntico ao de antes, então a comparação com as capturas continua valendo.

### 11.2 A persona saiu do rodapé da barra lateral

O rodapé dizia **"João Ribeiro · CEN · Regional MT"** em todas as quinze telas. As duas metades
estavam erradas: a pessoa não existe no cadastro, e a **regional não existe em lugar nenhum** —
`IVS_Regional` tem zero linhas no Vórtice, e as treze filiais em operação estão todas no interior
de São Paulo (documento 26, §1). Um rótulo de geografia inventada em toda tela vira fato depois da
terceira leitura.

No lugar entra o **contexto de acesso de verdade**: o usuário e a filial que vão nos cabeçalhos da
API, com a palavra *provisório* escrita, porque cabeçalho HTTP não autentica ninguém (dívida D-1).

### 11.3 `/agenda` — a Agenda do CEN

| | Antes | Agora |
|---|---|---|
| Origem | `public/dados/agenda.json`, 9 tarefas de exemplo | `/api/v1/tarefas` — **9.527 pendentes** na filial 010101 |
| Contadores | somados no navegador sobre as 9 linhas | `/api/v1/relatorios/agenda`, um `GROUP BY` no banco dentro do filtro de filial |
| Conclusão de tarefa | gravava em `localStorage` e a lista mudava | **saiu** — nenhuma rota de relacionamento escreve (dívida D-9), e um botão que gravasse só no navegador daria a impressão de que a agenda mudou |

Números reais lidos ao vivo na filial 010101: **9.527 pendentes · 4.067 atrasadas · 6 para hoje ·
227 nos próximos sete dias · 2.524 concluídas em 30 dias**.

**As três coisas que a tela precisou explicar:**

1. **O atraso é medido contra a data agendada, e não contra o prazo.** 81.571 das 103.339 tarefas
   migradas não têm prazo limite, porque nenhuma das 178 ações em uso na origem declara prazo
   (documento 25, §3.1). A coluna de prazo mostra **"não declarado"**, e o painel devolve a métrica
   ausente com o número: *"4.856 de 9.527 tarefas pendentes não têm prazo limite declarado."*
2. **"Só as minhas" devolve zero, e a tela diz por quê.** A agenda migrada pertence às 273 pessoas
   que vieram do Vórtice; o usuário do cabeçalho é o provisório de desenvolvimento e não é uma
   delas.
3. **Em 390px a tabela de sete colunas vira uma ficha por tarefa.** Rolar para o lado esconderia
   justamente a coluna que identifica a linha.

**Verificação:** `npx tsc --noEmit` limpo; aberta em 1.280px e em 390px com a API e o banco no ar,
**sem erro de console e sem rolagem horizontal** nas duas larguras.

### 11.4 `/pipeline` — o Pipeline de Vendas

| | Antes | Agora |
|---|---|---|
| Colunas do kanban | **seis escritas no código**: Qualificação, Diagnóstico, Proposta, Negociação, Fechamento, Ganho/Perdido | vêm de `/api/v1/relatorios/funil`, um `GROUP BY` no banco por fluxo × fase |
| Cartões | `public/dados/pipeline.json` | `/api/v1/processos` — **16.032 abertos** na filial 010101 |
| Fechar / marcar como perdida | gravava em `localStorage` | **saiu** — nenhuma rota escreve (dívida D-9) |

**Quatro das seis fases do protótipo não existem no dado.** O funil real do fluxo de vendas é
Apresentação → Monitoramento → Negociação → Pedido de Venda → Montagem → Análise → Aprovação →
Autorização → Faturamento → Recebimento, e a coluna com **6.205 dos 7.017 processos abertos** do
fluxo chama-se **Apresentação** — nome que o protótipo não tinha (documento 25, §8.1). Uma coluna
de kanban vazia é pior do que uma coluna com nome estranho: as seis do protótipo produziriam cinco
vazias e uma com tudo dentro.

Como são **32 pares fluxo × fase**, um seletor abre um fluxo por vez — e os fluxos do seletor saem
do dado, ordenados pelo tamanho, sem lista escrita na tela.

**O valor nunca aparece sozinho.** Cada coluna mostra a soma **e quantos processos a sustentam**:
*"R$ 700.000 — somados de 4 de 6.205 processos; o resto não declara valor."* Onde nenhum processo
declara, a coluna diz **"Sem valor declarado"** em vez de `R$ 0` — a API devolve `null`, e não
zero, porque zero diria "o funil vale nada" e o que se sabe é que ninguém preencheu.

### 11.4.1 Uma dívida nova da API, encontrada exercitando a tela: **P-7**

> **O filtro por fase e por fluxo da listagem é aplicado DEPOIS de o banco paginar.**

`ListarProcessos` pagina no repositório e só então aplica `faseCodigo`/`tipoProcessoCodigo` sobre
a página que voltou. Medido ao vivo contra o banco real:

```
GET /api/v1/processos?tamanho=25                          -> 25 itens, total 16.039
GET /api/v1/processos?tamanho=25&faseCodigo=APRESENTACAO  -> 12 itens, total 16.039
```

A contagem não corresponde às linhas, e nenhuma página seguinte corrige isso. **A tela não oferece
esse filtro** — oferecê-lo seria mostrar um número errado com cara de certo, que é o defeito que
este projeto existe para corrigir. A lista filtra só pelo que o banco resolve: busca, situação e
encerrados. O quadro de fases acima continua correto, porque vem do agregado.

**O conserto é do lado da API** — empurrar os dois códigos para dentro de `ConsultaDeProcessos`,
como `situacao` já está. É a mesma natureza de **P-3** (a API não filtra por modelo).

**Verificação:** `npx tsc --noEmit` limpo; 1.280px e 390px sem erro de console e sem rolagem
horizontal.

### 11.5 `/cobertura` — a Cobertura de Carteira

| | Antes | Agora |
|---|---|---|
| Origem | `carteira-cen.json` + `cobertura.json` | `/api/v1/cobertura` — **15.104 vínculos** na filial 010101 |
| Resumo por carteira | somado no navegador | `/api/v1/relatorios/cobertura` — **41 carteiras**, contadas no banco |
| Ordem padrão | **classe A, depois B** | **quem está há mais tempo sem contato**, e o nunca-contatado antes de todos |
| Mapa (Leaflet) | coordenadas de MT, GO e BA | **saiu** |
| Registrar contato | gravava em `localStorage` e mexia nos KPIs | **saiu** — interação é fato imutável e nenhuma rota escreve |

Números reais na filial 010101: **41 carteiras · 15.104 vínculos · 5.496 com contato em 30 dias ·
9.709 em 90 dias · 2.091 nunca contatados**.

**A ordenação por classe saiu, e é o caso mais importante desta rodada.** Era a métrica mais
visível do protótipo e ela não se sustenta: **59 de 49.109 vínculos** têm classe lida da origem; os
outros 49.050 entraram como `C` por assunção, porque `IVS_Pes.Potencial` é `varchar(3)` sem
catálogo e guarda `64`, `43`, `22` e `85` — resquício de outro domínio (documento 25, §5.1).
**Ordenar por um campo assumido em 99,9% dos casos é ordenar por nada.** A coluna saiu, o filtro
por classe não é oferecido, e a tela escreve o número que sustenta a decisão.

No lugar entra o tempo sem contato, que é **dado real e verificável**: `UltimaInteracaoEm` não foi
copiada da origem — é calculada das interações que efetivamente entraram. Copiar a coluna do legado
teria alcançado 14.496 vínculos; calcular alcançou **39.589** (documento 25, §6.1).

**O mapa saiu porque plotava geografia que não existe.** As coordenadas eram de Mato Grosso, Goiás
e Bahia; as treze filiais estão todas no interior de São Paulo. Efeito colateral bem-vindo: a rota
deixa de oscilar entre 0,05% e 2,18% na comparação visual, que era o Leaflet buscando ladrilhos
pela rede (notas (1) e (2) das seções 9 e 10).

**Três lacunas declaradas no rodapé da tela**, com o número de cada uma: segmentação por classe,
interações por canal (as 178 ações entraram todas como `Interna`) e o mapa do território.

**Verificação:** `npx tsc --noEmit` limpo; 1.280px e 390px sem erro de console e sem rolagem
horizontal.

### 11.6 `/relatorios/cobertura` — a Cobertura Regional virou **Cobertura por Filial e Carteira**

**A tela inteira estava desenhada sobre uma premissa falsa.** O protótipo agrupava a cobertura em
sete regionais — MT Norte, GO, BA Oeste e mais quatro. Quem corrigiu a premissa foi o gerente
comercial:

> *"Se você olhar no CRM da Vórtice temos as carteiras dos CENs, e nas carteiras temos a filial que
> ele vai atuar e quais cidades ele terá na carteira."*

Verificado ao vivo: **`IVS_Regional` existe no esquema do Vórtice e tem ZERO linhas** — nunca
recebeu uma. Não há tabela, coluna nem valor de texto que sustente aquelas sete regionais, e as
treze filiais em operação estão **todas no interior de São Paulo**.

| | Antes | Agora |
|---|---|---|
| Título e menu | "Cobertura de Carteira · Painel Regional" / "Cobertura Regional" | **"Cobertura por Filial e Carteira"** / "Cobertura por Filial" |
| Agrupamento | sete regionais inventadas | **filial → carteira → cidades**, de `/api/v1/cobertura/filiais` e `/carteiras` |
| Origem | JSON do protótipo | `organizacao.CarteiraMunicipio` — **532 vínculos** carregados |

Números reais na filial 010101, e batem com a tabela do documento 26 §8.1: **41 carteiras · 11 com
cidade · 86 municípios · SC e SP**.

**A correção da premissa fica na tela, não só neste documento.** Quem abriu esta rota ontem viu
sete regionais e pode voltar procurando por elas; um aviso no topo diz o que mudou e por quê.

**A lacuna aparece em vez de sumir.** 30 das 41 carteiras da filial não declaram nenhuma cidade, e
a API devolve esse número em `metricasSemDado`. **A carteira sem cidade aparece com a lista vazia**
— escondê-la faria a tela mostrar uma operação menor do que ela é.

**Um achado que a tela expõe:** entre os estados alcançados aparece **SC**, de uma linha
`SAO CARLOS/SC`. São Carlos é de SP — é um dos 11 cadastros com UF errada que o documento 26 §6.2
já tinha contado. O dado é real e o defeito é real; a tela mostra o que existe em vez de limpar em
silêncio.

**Verificação:** `npx tsc --noEmit` limpo; 1.280px e 390px sem erro de console e sem rolagem
horizontal.

### 11.7 `/relatorios/funil` — o Funil de Vendas

| | Antes | Agora |
|---|---|---|
| Fases | `public/dados/funil.json` | `/api/v1/relatorios/funil` — **32 pares fluxo × fase** |
| Desfechos | escritos no JSON | cinco `COUNT` no banco, um por situação do domínio fechado |
| Perdas por motivo | `vendas-perdidas-motivos.json` | `/api/v1/relatorios/perdas` |
| Título | "Funil de Vendas **do Mês**" | "Funil de Vendas" — o recorte carregado é o **ano** de 2026, e a rota não aceita período |

Números reais na filial 010101: **16.032 abertos · 600 ganhos · 190 perdidos · 3.675 cancelados**.

**O cancelamento é a lixeira, e agora dá para ver.** Os cancelados são **19 vezes** os perdidos —
3.675 contra 190. É o mesmo achado do documento 25 §2 (um quarto do fluxo de vendas termina em
cancelado, 5.938 de 23.212), agora visível na tela em vez de enterrado num relatório.

**A taxa de conversão do funil inteiro não é mostrada, e o motivo está na tela.** O denominador não
é o que parece: **16.422 processos de fluxos que não são de venda** — aferição de qualidade,
pré-entrega, entrega física, prospecção de peças — ficam abertos para sempre porque
`SituacaoDoProcesso` não tem o valor `Encerrado`, e inflam o funil em **46%**. O que a tela mostra
é **ganhos ÷ (ganhos + perdidos) = 76%**, com o denominador escrito no próprio cartão e a ressalva
de que ele não cobre os cancelados.

**Quatro lacunas declaradas no rodapé**, cada uma com o número: valor do funil e ticket médio (9 de
16.032 declaram valor nesta filial), probabilidade por fase (`Fase.ProbabilidadePercentual` nula
nas 50 fases), atingimento de meta (`organizacao.Meta` vazia) e **faturamento realizado, parado
desde 11/04/2025**.

### 11.7.1 Um defeito de CSS que valia para a aplicação inteira

`.cad-so-leitor` — o texto que só o leitor de tela lê — é `position: absolute`, e **um elemento
absoluto só é recortado pelo `overflow` de um ancestral que seja o containing block dele**.
`.cad-tabela-wrap` tinha `overflow-x: auto` e `position: static`, então um texto de acessibilidade
dentro de uma célula à direita ficava na coordenada estática dela — a 500px, numa tabela de 860px
— **escapava do recorte e esticava o documento inteiro**.

Medido no Funil em 390px: a página ia a **497px** de largura e rolava na horizontal. Com
`position: relative` no contêiner, volta a **390px**. O defeito era latente em todas as tabelas e
só apareceu quando uma coluna passou a ter conteúdo acessível à direita.

**Verificação:** `npx tsc --noEmit` limpo; 1.280px e 390px sem erro de console e sem rolagem
horizontal.

### 11.8 `/relatorios/performance` — a Performance de CEN, que **encolheu de propósito**

Esta é a tela onde a regra doeu mais. O protótipo mostrava **faturamento por CEN, atingimento de
meta, tendência de doze meses e taxa de conversão**, com cinco pessoas escritas no JavaScript.
**Nenhuma das quatro tem lastro.**

| Métrica do protótipo | Por que não pode ser mostrada |
|---|---|
| Faturamento e ranking de vendas | `EXT_NFS` **parada desde 11/04/2025**. A tabela continua respondendo e continua cheia — é por isso que o número pareceria atual |
| Atingimento de meta | `organizacao.Meta` está **vazia**; no protótipo as metas eram números escritos no JavaScript |
| Processos e tarefas por CEN | **falta rota.** `ProprietarioId` existe na consulta do domínio mas o endpoint não o expõe, e `/tarefas` só aceita `minhas` |
| Tempo médio de atendimento | `Duracao` é **zero em 122.812 de 122.812** interações |
| Taxa de conversão por CEN | depende das duas de cima, e ainda esbarra no cancelamento sem motivo |

**O que sobrou é um agregado por pessoa que é real:** a cobertura de carteira.
`/api/v1/relatorios/cobertura` devolve uma linha por carteira com o CEN responsável já contado no
banco, e agrupá-las por responsável responde a pergunta que o gerente faz de verdade — **quem está
cobrindo a carteira e quem não está.**

Números reais na filial 010101: **31 CENs · 15.104 vínculos · 36% com contato em 30 dias · 2.091
nunca contatados**. Cada percentual aparece com o denominador escrito embaixo (*"77 de 1.128"*),
porque percentual sem denominador não significa nada.

**A soma é feita na tela, e isso está justificado no arquivo**: a rota já devolveu o agregado
pronto (41 linhas, não 45 mil), não existe rota que agrupe por pessoa, e somar carteira por
responsável é exato — cada carteira tem um responsável só. O que **não** é somável é o cliente:
metade está em duas ou mais carteiras, então a coluna diz **vínculos**, e não clientes.

**Verificação:** `npx tsc --noEmit` limpo; 1.280px e 390px sem erro de console e sem rolagem
horizontal.

### 11.9 `/` — a Visão 360, a última tela que ainda lia JSON

**Duas camadas, e as duas do banco:**

| Camada | Quando aparece | De onde vem |
|---|---|---|
| **O meu dia** | nenhum cliente escolhido | `/relatorios/agenda` + `/tarefas?somenteAtrasadas` + `/cobertura` |
| **O 360 do cliente** | um cliente escolhido na busca | cinco leituras em paralelo, todas por `clienteChave` |

A busca é a mesma `SeletorDeCliente` do cadastro — sugestão contra `/api/v1/clientes`, dentro da
filial do cabeçalho. O que a tela guarda é a **chave**; o nome digitado nunca vira dado.

**Qualquer um dos 23.945 clientes abre agora.** No protótipo, **só o cliente 84391** tinha ficha
completa; os outros 22 da carteira abriam com estado vazio. Os cinco blocos do 360 —
identificação, frota, oportunidades, agenda e linha do tempo — saem de `comercial.Cliente`,
`frota.Equipamento`, `processo.Processo`, `processo.Tarefa` e `processo.Interacao`.

### 11.9.1 O painel executivo saiu, e é a remoção mais pesada desta rodada

O seletor de perfil trocava a tela por um painel de Gerente e um de Diretora, com **faturamento de
doze meses, ranking de CENs, mix de linhas e cobertura regional**. Os quatro vinham de arquivos
JSON e **nenhum tem lastro**:

| Bloco | Por quê |
|---|---|
| Faturamento 12 meses | `EXT_NFS` parada desde **11/04/2025** |
| Ranking de CENs | não existe rota que agrupe processo ou tarefa por pessoa |
| Mix de linhas | não há mix carregado |
| Cobertura regional | **a regional não existe** — `IVS_Regional` tem zero linhas |

**Um painel de diretoria é o pior lugar possível para um número plausível e errado**: é a tela em
que a decisão é tomada e em que ninguém confere a origem. No lugar, a tela aponta para as três que
têm dado real — Performance de CEN, Cobertura por Filial e Carteira, e Funil de Vendas.

### 11.9.2 Os blocos mortos continuam na tela, vazios e com a data

Faturamento, títulos e ordens de serviço são o que o CEN mais pede na ficha do cliente, e as três
integrações estão paradas na origem. **Some-los faria a tela parecer completa**; mostrá-los com o
último número que sobrou é o defeito de 17 meses. O que fica é o bloco, vazio, com a data em que a
integração parou: **faturamento 11/04/2025 · títulos 05/2025 · ordens de serviço nunca promovidas ·
frota do ERP 24/05/2024**.

**Uma lacuna de rota registrada:** `/api/v1/cobertura` **não aceita `clienteChave`** — só classe,
dias sem contato e ordenação. Por isso o 360 não mostra em quais carteiras o cliente está nem quem
é o CEN dele, que é justamente o que metade dos clientes tem em duplicidade (49% estão em duas ou
mais carteiras).

### 11.9.3 Dois defeitos de largura consertados aqui

`.p360-bloco-header-lado` tinha **`flex-shrink: 0`**. O lado direito do cabeçalho fora desenhado
para o selo curto de fonte externa; ao receber o selo de **procedência** completo, recusava-se a
encolher e empurrava o bloco para fora da coluna da grade — **1.338px de documento em 1.280px de
janela, e 580px em 390px**. Passou a encolher e quebrar linha, e o título ganhou prioridade
(`flex: 1 1 220px`), porque o selo estava espremendo o título a metade do bloco.

**Verificação:** `npx tsc --noEmit` limpo; as duas camadas percorridas com o Playwright em 1.280px
e 390px — **abrir o 360 de um cliente pela fila de "sem contato" funciona**, e o console fica limpo
nas duas larguras, sem rolagem horizontal.
