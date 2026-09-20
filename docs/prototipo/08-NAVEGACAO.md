# A navegação — qual tela abrir para quê

> **Documento 08** · Versão 1.1 · 19/09/2026 — a Agenda saiu do menu e continua sendo tela (§3.1,
> issue 029). A §3, que em 05/09 registrou "nada sai do menu", fica como estava e ganha o adendo: ela
> descreve aquela rodada, não uma regra permanente.
> Versão 1.0 · 05/09/2026
> Responde à primeira das três queixas: ***"telas demais mostrando a mesma coisa — o cliente aparece
> na Visão 360, em Clientes, na Cobertura e na Ficha, cada uma com um recorte, e não fica claro qual
> abrir para quê."***

Este documento **decide antes de mexer**. A regra que ele segue é a que veio junto com a queixa:
**fundir errado custa mais que conviver mais uma rodada.** Onde a fusão é segura, ela está na seção
4, executada. Onde depende de dado que ainda não existe ou de decisão do negócio, ela está na seção
5, como proposta, com o que falta para decidir.

---

## 1. A tabela que resolve a queixa

**Quero fazer X → vou em Y.** Se uma linha desta tabela não for verdade na tela, é defeito.

| Quero… | Vou em | Por que ali, e não na vizinha |
|---|---|---|
| saber quem é este cliente e **o que faço agora** | **Visão 360** (`/`) | é a única que junta, numa página, alerta + frota + oportunidade + interação + financeiro, e leva daí para a ação |
| ver **o meu dia**: o que venceu, o que é de hoje, o que espera aprovação | **Visão 360** (`/`) | a camada 1 do painel do CEN é a fila do dia, ordenada por urgência real |
| **cadastrar** um cliente novo | **Clientes** (`/clientes/novo`) | é a única tela que grava no banco |
| **corrigir** o cadastro — razão social, documento, situação | **Clientes** → a ficha (`/clientes/{chave}`) | idem. A ficha rica de `/clientes/84391` é leitura |
| **inativar** um cliente | **Clientes** → a ficha | é lá que existe o motivo de catálogo e a confirmação |
| **achar** um cliente por nome ou documento | a **busca do cabeçalho**, em qualquer tela | ela leva a Clientes com o termo aplicado, e Clientes busca na API dentro da filial |
| saber **quem estou deixando sem contato** | **Cobertura de Carteira** (`/cobertura`) | é a única que calcula o status contra a meta de frequência por classe, e a única com o mapa |
| **registrar um contato** que aconteceu | **Cobertura de Carteira** | é onde o registro tem efeito: a linha do cliente muda, o contador zera (ou não zera) e a cobertura se recalcula |
| **concluir uma tarefa** e gerar a próxima | **Agenda** (`/agenda`) | é o fluxo de andamento — resultado de catálogo, fase, próxima tarefa |
| **agendar** uma visita ou ligação | **Agenda** → *Nova tarefa* | a tarefa nasce com prazo da política do tipo |
| ver **tudo** do cliente de demonstração: frota, pós-vendas, financeiro, histórico | **Ficha do Cliente** (`/clientes/84391`) | é a única com pós-vendas, títulos e ordens de serviço — dado que só existe neste cliente |
| **criar** uma oportunidade | **Nova Oportunidade** (`/oportunidades/nova`) | — |
| **mover de fase** ou **fechar** uma oportunidade | **Pipeline** (`/pipeline`) | é a única que grava a mudança. A ficha da oportunidade é leitura |
| ler o **processo inteiro** de uma oportunidade: itens, aprovações, documentos, histórico | **Ficha de Oportunidade** (`/oportunidades/1517613`) | — |
| **cadastrar** ou **dar baixa** num equipamento | **Equipamentos** (`/equipamentos`) | é a única que grava no banco |
| ver **telemetria, revisões e garantia** de um chassi | **Ficha do Equipamento** (`/equipamentos/1RW…`) | dado que só existe neste chassi |
| ver **conversão por estágio** do mês | **Funil** (`/relatorios/funil`) | — |
| comparar **CENs** entre si | **Performance de CEN** (`/relatorios/performance`) | — |
| ver a **cobertura das regionais**, acima do meu CEN | **Cobertura Regional** (`/relatorios/cobertura`) | — |
| mudar meta, alçada, SLA, taxonomia | **Configurações** (`/config`) | — |
| conferir **o que existe no protótipo** | **Mapa do protótipo** (`#/inicio-antigo`) | não está no menu, e não é tela de produto |

---

## 2. As quatro telas que mostram cliente, e o que cada uma é de verdade

A queixa está certa: são quatro. Mas as quatro **não mostram a mesma coisa** — mostram quatro
recortes, e o que faltava era dizer qual é qual. Nenhuma sobra.

| Tela | O recorte | O que só ela tem | O que ela **não** faz |
|---|---|---|---|
| **Visão 360** (`/`) | **um cliente, agora** | os alertas que pedem ação, a fila do dia, e o caminho para as outras quatro | não grava cadastro, não calcula cobertura da carteira inteira |
| **Clientes** (`/clientes`) | **o cadastro, na filial** | escrita: criar, alterar, inativar. Busca, filtro, ordenação e paginação **do servidor** | não mostra frota, faturamento nem interação |
| **Cobertura** (`/cobertura`) | **a carteira, por atraso** | o status contra a meta de frequência por classe, o mapa, e o registro de contato que muda o status | não mostra o processo comercial nem o cadastro |
| **Ficha do Cliente** (`/clientes/84391`) | **um cliente, inteiro** | pós-vendas, títulos, ordens de serviço, histórico de interações completo | não grava nada |

**A diferença que resolve a confusão, em uma frase:** a Visão 360 é a **porta**, Clientes é a
**escrita**, Cobertura é a **priorização**, e a Ficha é o **arquivo**.

E é assim que elas já se comportam: `componentes/painel360/Cliente360.tsx` — a camada 2 da Visão
360 — **já leva** à Cobertura, à Agenda, a Equipamentos, ao Pipeline, à ficha da oportunidade e à
Ficha do Cliente. Ela é a porta, e sempre foi. O que faltava era o caminho de volta e a declaração,
em cada tela, do que ela é.

---

## 3. Por que nenhuma sai do menu

O menu tem dez itens. Nenhum deles é uma das quatro telas de cliente duplicadas, e é isso que
desarma metade da queixa:

- **Visão 360**, **Cobertura**, **Clientes** estão no menu porque são **três intenções diferentes** —
  trabalhar, priorizar, cadastrar — e cada uma é a porta de uma delas.
- **Ficha do Cliente** (`/clientes/84391`) **nunca esteve no menu**. Chega-se a ela pela Visão 360,
  pela Cobertura ou pelo Pipeline. Ela é destino, não porta.
- **Mapa do protótipo** (`#/inicio-antigo`) **continua fora do menu** — a justificativa inteira está
  no documento 06, seção 4.

**O que sai do menu nesta rodada: nada.** Tirar um item do menu de dez para resolver uma confusão de
quatro telas trocaria uma queixa por outra — "sumiu a tela que eu usava".

### 3.1 A Agenda saiu do menu em 19/09/2026 — e continua sendo tela

A frase acima valia para **aquela** rodada, cujo assunto eram as quatro telas de cliente. A Agenda
não é uma delas, e o pedido veio depois (issue 029): **o menu deixa de oferecer a Agenda do CEN, e a
tela continua inteira**.

| | |
|---|---|
| O que saiu | o item `Agenda do CEN` da seção Comercial do menu lateral |
| O que ficou | a rota `/agenda`, a tela, a escrita de tarefa e os dados |
| Como se chega | pela URL e pelos links que já levavam lá: Visão 360, painel executivo, Ficha do Cliente, aba de pós-vendas e o cartão do Mapa do protótipo |
| Por quê | o dia do CEN começa na **Cobertura de Carteira**, que é onde a prioridade é calculada; a Agenda é o andamento de uma tarefa já escolhida. Duas portas permanentes para o mesmo começo de dia é o caminho de volta para "telas demais" (§5.2) |
| O que **não** mudou | tarefa continua alimentando o Meu dia, a Visão 360, a ficha de oportunidade e o consolidado; a §1 continua valendo — "concluir uma tarefa" e "agendar uma visita" seguem sendo na Agenda |
| Conferido por teste | `src/componentes/Layout.teste.tsx`: o menu não oferece a Agenda, `/agenda` continua abrindo, e todo item do menu aponta para uma rota declarada |

Fica **pendente**, porque é decisão de quem usa e não de quem programa: a proposta da §5.4 (renomear
a seção "Executivo") e o destino de longo prazo dos links "Ver agenda" espalhados pelas telas — hoje
eles continuam funcionando, que é o comportamento que ninguém perde.

---

## 4. O que foi executado nesta rodada

Tudo aqui é **cada tela dizer o que é e para onde manda**. Nenhuma tela foi fundida, nenhuma foi
apagada, nenhuma rota mudou de endereço.

### 4.1 A busca do cabeçalho virou a porta de "achar um cliente"

Era um campo decorativo em **todas** as telas. Agora `Ctrl+K` põe o foco nele e `Enter` leva o termo
a **Clientes**, com a busca já aplicada (`/clientes?busca=…`). Foi a decisão de navegação com maior
efeito por linha escrita: a pergunta "onde eu procuro um cliente?" passou a ter a mesma resposta em
qualquer tela.

O texto de exemplo também mudou. Ele prometia *"cliente, chassi, oportunidade"*, e a busca de
cliente não acha nem chassi nem oportunidade: **chassi é em Equipamentos** e **oportunidade é no
Pipeline**, cada uma com a própria busca. Prometer as três num campo só era o engano.

### 4.2 A Ficha do Cliente declara que é leitura, e diz onde se escreve

`/clientes/84391` e `/clientes/{chave}` são **duas telas diferentes no mesmo formato de endereço** —
a rica, que lê JSON, e a do cadastro, que lê e grava na API. Quem cai na primeira procurando alterar
o cadastro não encontrava o caminho: encontrava um botão *"Solicitar alteração cadastral"* que não
fazia nada.

O botão saiu e, no lugar dele, ficou a frase: **alterar cadastro é pela tela Clientes, que grava na
API; esta ficha é leitura.**

### 4.3 A Ficha de Oportunidade declara que é leitura, e leva ao Pipeline

Mesmo caso. A ficha tinha cinco ações no cabeçalho prometendo escrita — inclusive *"Marcar como
perdida"* e *"Avançar para Pedido Alocado"* — e nenhuma funcionava. As duas ações que existem no
produto, **mover de fase e fechar**, moram no Pipeline. O cabeçalho passou a dizer isso e a levar
até lá.

### 4.4 O registro de contato tem um dono só

O painel do Pipeline tinha um botão *"Registrar contato"* que não fazia nada. Registrar contato
**tem efeito** — muda o status de cobertura do cliente — e esse efeito só existe na Cobertura de
Carteira. Duas portas para a mesma ação, uma delas sem efeito, é exatamente a bagunça da queixa. O
botão do Pipeline saiu; a Cobertura ficou sendo a porta, e agora grava de verdade.

### 4.5 O Mapa do protótipo diz, na primeira linha, que não é tela de produto

Ver documento 06, seção 4.

---

## 5. O que fica como proposta — e o que falta para decidir

### 5.1 Fundir a camada 2 da Visão 360 com a Ficha do Cliente

**É a fusão de verdade, e é a que mais paga.** As duas respondem "tudo sobre este cliente": a
camada 2 do `PainelCen` e `/clientes/84391` mostram identificação, carteira, frota, oportunidades,
interações e financeiro. A diferença é que a Ficha tem **pós-vendas, títulos e ordens de serviço**, e
a camada 2 tem **os alertas e os botões de ação**.

**Por que não agora.** A camada 2 monta o 360 de qualquer um dos 23 clientes da carteira, com estado
vazio onde falta dado; a Ficha existe para **um** cliente, o 84391, e é a única com dado rico. Fundir
hoje significa uma de duas coisas, e as duas são ruins:

- levar os blocos ricos da Ficha para a camada 2 → 22 dos 23 clientes ficariam com a tela quase
  toda vazia, e a Visão 360 pioraria para quase todo mundo;
- levar os alertas e as ações da camada 2 para a Ficha → a Visão 360 perde a razão de existir, e
  volta a ser painel de relatório, desfazendo o passo 4.

**O que falta para decidir:** o dado. Pós-vendas, títulos e ordens de serviço vêm do **Protheus**, e
a integração é o passo 7 da ordem de execução do documento 05. Quando os blocos ricos existirem para
todos os clientes, a fusão deixa de ser uma escolha entre dois prejuízos e passa a ser só trabalho.
**A rodada certa para isto é a que ligar o Protheus.**

### 5.2 Tirar `/clientes/84391` de dentro do espaço de `/clientes/*`

**O problema é real e é de roteamento.** `/clientes/84391` e `/clientes/{chave}` têm o mesmo formato
e abrem telas completamente diferentes. Hoje isso funciona por uma ordem frágil declarada em
`rotas.tsx`: a rota fixa precisa vir **antes** da rota com parâmetro, porque `acharRota` casa a
primeira que bater. Um dia alguém troca a ordem e a ficha rica some sem ninguém entender por quê — o
comentário no arquivo existe exatamente porque isso já é sabido.

**Proposta:** mover a ficha rica para um endereço que diga o que ela é, por exemplo
`/clientes/84391/360`, e deixar `/clientes/*` inteiro para o cadastro.

**Por que não agora, e o que falta.** A rota `/clientes/84391` é uma das quinze medidas por
`scripts/prototipo/comparar-telas.mjs` contra a captura de referência. Mudar o endereço obriga a
atualizar o script e a refazer a captura, e essa decisão anda junto com 5.1 — se as duas telas se
fundirem, o endereço novo é outro. **Fazer as duas juntas custa menos que fazer as duas separadas.**

### 5.3 Absorver "clientes que pedem ação" da Visão 360 na Cobertura, ou o contrário

A camada 1 da Visão 360 lista *"clientes que pedem ação"*; a Cobertura lista a carteira ordenada por
urgência. Há sobreposição real de conteúdo — mas **não de intenção**: a Visão 360 mostra os poucos
que exigem ação hoje, a Cobertura mostra os 23 com o cálculo, o mapa e o registro de contato.

**Por que não agora.** É a fusão mais arriscada das três, porque as duas telas são as mais usadas do
CEN e a sobreposição é de conteúdo, não de função. Juntar as duas cria uma tela grande que faz
priorização e trabalho ao mesmo tempo, e é assim que se volta a "telas demais".

**O que falta para decidir:** medir. Com o CRM no ar, saber quantos CENs abrem as duas na mesma
sessão e em que ordem responde a pergunta melhor que qualquer argumento — é o tipo de coisa que a
pesquisa do Vórtice mediu (o pico às 17h de sexta) e que decidiu o passo 4.

### 5.4 A seção "Executivo" do menu, que hoje tem uma tela de CEN dentro

O menu lateral tem a seção **Executivo** com um item só: **Visão 360**. Desde o passo 4, a Visão 360
abre o **painel do CEN** por padrão, e só vira painel executivo quando se troca o perfil. A seção
diz uma coisa e o conteúdo é outra.

**Proposta:** renomear a seção para algo como **"Meu dia"**, ou juntar a Visão 360 ao bloco
Comercial.

**Por que não agora.** É uma palavra, mas ela fica na barra lateral, que aparece em **todas** as
quinze capturas — trocá-la move a comparação visual de todas de uma vez, por um ganho pequeno. Vale
juntar essa troca com a próxima mudança que já for mexer no menu, e refazer as capturas uma vez só.

---

## 6. O que este documento promete que a tela cumpre

Cada linha da tabela da seção 1 corresponde a um caminho que existe e funciona. As três que foram
construídas nesta rodada — **achar cliente pela busca do cabeçalho**, **registrar contato na
Cobertura** e **criar tarefa na Agenda** — não existiam antes, e estão registradas no documento 06.

**O que não está na tabela, não tem tela.** Registrar interação avulsa fora da Cobertura, aprovar
desconto, anexar documento, abrir chamado, exportar visão salva: nenhuma dessas tem porta, e por
isso nenhuma tem mais botão (documento 06, seção 2). Quando tiverem, entram aqui primeiro.
