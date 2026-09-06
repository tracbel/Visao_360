# Telas e navegação — CRM Tracbel

> **Documento 08 de 10** · Versão 1.0 · 31/08/2026
> Baseado em **11 capturas reais** do Vórtico CRM 4.04.01 em produção, cedidas em 31/08/2026.
> `[V]` = defeito observado na tela · `[SF]` Salesforce · `[DYN]` Dynamics.
>
> Este documento é a especificação do dev de frontend. O critério mais duro do portão da
> fase 1 é dele: **cinco usuários reais completando três tarefas sem ajuda**.

---

## 1. O inventário do que existe hoje

| # | Tela | Módulo | O que faz |
|---|---|---|---|
| 1 | **Agenda** (`Atendente` v4.04.01r15) | CRM_M001 | a fila de trabalho do CEN — a tela mais usada do sistema |
| 2 | **Andamento** | CRM_M001 | concluir uma tarefa escolhendo um Resultado. **O ponto onde o motor dispara** |
| 3 | **Cadastro de pessoas** (`PES1PES00`) | CRM_M001 | cliente/prospect, ~40 campos |
| 4 | **Atributos da empresa** (Tratores) | CRM_M001 | catálogo de modelos de trator |
| 5 | **Atributos da empresa** (Implementos) | CRM_M001 | catálogo de implementos |
| 6 | **Menu Configuração** (`Supervisor` v4.04.01r17) | CRM_M003 | 23 itens de parametrização |
| 7 | **Query Viewer** (v4.02.02) | QVW_M003 | árvore de relatórios |
| 8 | **Parâmetros do TBA_101XX3** | QVW_M003 | filtros do relatório de carteira de pedidos |

Ainda não capturadas: Vórtico Mobile Lite, CRM Web, Doc Manager, tela de Carteira (`IVS1DPT05`),
cadastro de Ações e Resultados.

---

## 2. Os treze problemas — cada um com sua correção

### 🔴 2.1 A agenda virou cemitério

A captura mais grave de todas, da agenda do usuário `JOSE.RUFINO`:

> **195 tarefas · 194 em atraso · 0 para hoje · 78 futuras · 169 não lidos**

**99,5% de atraso.** Quando tudo está atrasado, nada está priorizado — a fila deixa de informar.
Há tarefas abertas desde **30/07/2025**, treze meses paradas. É a contraparte visual dos 155
processos travados que medimos no banco.

Agrava: quase toda linha é `Monitorar Cliente (FY25)` / `EM ABERTO`. O sistema empilha, mas
**não diz o que fazer agora**.

> **Correção:** `[DYN]` widget **"Próxima ação"** no topo da agenda — uma tarefa por vez, com
> Concluir / Adiar / Pular. Mais três faixas: **Hoje**, **Atrasadas**, **Futuras**, com a
> contagem de atrasadas em destaque só quando passar de um limite. E um botão explícito de
> **higienização em lote** para o gestor tratar acúmulo histórico.

### 🔴 2.2 "Atividade Cancelada" não tem barreira nenhuma

Na tela de Andamento, o campo Resultado abre esta lista:

```
Desistiu da Compra      - 1612
Venda Perdida           - 1613
Contato sem Sucesso     - 1614
Negociação em Andamento - 1615
Orçamento Aprovado      - 1616
Atividade Cancelada     - 3296   ← cancela o PROCESSO INTEIRO, sem desfazer
```

Lista plana, sem separador, sem cor, sem aviso, sem confirmação. O item que **encerra o processo
de forma irreversível** tem exatamente o mesmo peso visual de "Negociação em Andamento".

É o erro mais caro do dia a dia, e a interface não faz nada para evitá-lo.

> **Correção, em quatro camadas:**
> 1. **Agrupar por classe** — Avanço, Manutenção, Perda, Cancelamento — com cabeçalho visível.
> 2. **Cor e ícone** por classe; cancelamento em vermelho, separado por régua.
> 3. **Prever a consequência antes do clique:** *"Isto vai encerrar o processo 1558647 e
>    cancelar 3 tarefas pendentes."*
> 4. **Confirmação com justificativa obrigatória** para qualquer desfecho de classe Cancelamento.
>
> E, no modelo: cancelar uma tarefa duplicada é **operação sobre a tarefa**, nunca um desfecho
> que mexe no processo (documento 09, seção 5).

### 🔴 2.3 O filtro de carteira entrega a carteira de todo mundo

Na tela de parâmetros do `TBA_101XX3`, logado como `RICARDO.MORETTI`, a lista Carteiras oferece:

```
☑ DGT_02ARA     - VANESSA.ALVES
☐ MAQ_02ARA_01  - THIAGO.SUNDFELD
☐ MAQ_02ARA_02  - YASMIN.COSTA
☐ MAQ_02ARA_03  - PEDRO.SALIONE
☐ MAQ_02ARA_04  - RICARDO.MORETTI
```

Quatro carteiras que não são dele. E — pior — **o relatório ignora o que for marcado**: dos três
ramos do `UNION`, dois filtram só por `CODUSUARIO`.

Ou seja: a tela vaza informação de escopo **e** o filtro não funciona.

> **Correção:** toda lista de seleção passa pelo mesmo filtro de segurança das consultas
> (documento 05, seção 8). Se o usuário não alcança a carteira, ela **não aparece** —
> e o que ele marca é o que o relatório usa.

### 🟡 2.4 Códigos internos na cara do usuário

`Desistiu da Compra - 1612` · `Seq.Pess. 118349` · `Sequencial` · `Versão atual 0` ·
`Processo DNA (=)`.

O usuário lê identificadores de banco e jargão interno o tempo todo.

> **Correção:** o número interno **nunca** aparece. Onde for preciso identificar
> (falar com o suporte, citar num e-mail), existe um número **legível de processo**, exibido
> como `#1558647`, copiável com um clique. `[SF]` a plataforma expõe IDs opacos e nomes,
> nunca chaves de tabela.

### 🟡 2.5 Dez abas sempre visíveis

`Agenda · Historicos · Andamento · Informações da agenda · Pessoa · Contatos · Atributos ·
Propriedades · Notas · Perfil`

Dez destinos, todos no mesmo peso, sempre presentes. O usuário precisa memorizar onde cada
informação mora — e "Históricos" versus "Andamento" versus "Informações da agenda" são três
abas que, para quem chega, parecem a mesma coisa.

> **Correção:** `[DYN]` **uma tela de registro com timeline unificada**. Históricos, andamentos
> e notas são a mesma coisa conceitualmente — atividades — e vão para uma linha do tempo só,
> com filtro por tipo. Restam **quatro** áreas: Resumo, Linha do tempo, Dados, Documentos.

### 🟡 2.6 Sete botões de ícone sem rótulo

Na barra superior da Agenda, sete ícones sem texto e sem tooltip visível. Conhecimento tribal:
quem sabe, sabe.

> **Correção:** todo botão tem **rótulo em texto**. Ícone acompanha, nunca substitui.
> Regra do projeto: se precisa de treinamento para descobrir o que o botão faz, o botão está errado.

### 🟡 2.7 Três colunas de ícone sem nome na grade

As três primeiras colunas da grade (lápis, seta, reticências) não têm cabeçalho. São as ações
por linha.

> **Correção:** ação por linha fica num **menu de contexto rotulado**, aberto por um botão único
> e identificado, com os itens escritos: *Dar andamento · Abrir processo · Transferir · Ver documentos*.

### 🟡 2.8 Metade da tela não trabalha

Na Agenda, o painel de filtros ocupa ~40% da largura útil, e o painel verde de detalhe fica
**vazio e enorme** enquanto nada está selecionado. Sobra pouco para a lista, que é o que importa.

> **Correção:** `[DYN]` **Focused View** — lista à esquerda, registro à direita, na mesma tela.
> Filtros recolhidos numa barra superior, com as **visões salvas** em destaque (`[SF]` List Views).

### 🟡 2.9 Cadastro de pessoa: 40 campos numa tela plana

`PES1PES00` mostra, de uma vez: Sequencial, Versão, Tipo, CNPJ, Status, Nome/Razão, Tratamento,
Palavra-chave, Cidade, Bairro, UF, CEP, País, Caixa postal, Referência, Número, Complemento,
eMails, Home page, Skype, 4 tipos de telefone com DDD e complemento, Inscrição, UF, Data de
fundação, Grupo, Atividade, Rota, Região, Latitude/Longitude, Faturamento/Porte, Origem inclusão,
Origem alteração, 4 caixas de opt-in — mais 4 abas.

Nenhuma hierarquia. Tudo com o mesmo peso.

> **Correção:** `[SF]` **Compact Layout** — 7 campos no cabeçalho (Nome, Documento, Situação,
> Carteira, Responsável, Telefone principal, Última interação). O resto em seções recolhíveis,
> e campos raros só aparecem quando relevantes (`[SF]` Dynamic Forms).

### 🟡 2.10 Atraso sinalizado só por cor

Na grade, a data em vermelho é o único indicador de tarefa atrasada.

> **Correção:** cor **mais** rótulo (`Atrasada há 12 dias`) **mais** ícone. Requisito de
> acessibilidade AA do portão da fase 1, e ganho de legibilidade para todos.

### 🟡 2.11 Três gerações do mesmo relatório, lado a lado

Na árvore do Query Viewer, pasta `Tracbel Agro > Vendas`:

```
TBA_101X    :: Análise da Carteira de Pedidos (Novo)
TBA_101XX3  :: Análise da Carteira de Pedidos FY25
Tba_101xx4  :: Tba_101xx4          ← a descrição É o código
```

Clonagem substituiu versionamento — e o terceiro nem foi nomeado. Há ainda uma pasta
**"Relatórios Inativos"**: relatório morto não é excluído, é mudado de pasta.

> **Correção:** relatório tem **versão** e **dono**, e sai da lista quando arquivado.
> A tela mostra quando rodou pela última vez — `[V]` só 10 dos 134 rodaram nos últimos 3 meses.

### 🟢 2.12 Afordância escondida

Rodapé do Query Viewer: *"Use botão direito sobre o item para acessar o menu."*

Se a interface precisa explicar por escrito como se usa, ela já falhou.

> **Correção:** ação visível onde está o objeto. Menu de contexto existe como atalho, nunca
> como único caminho.

### 🟢 2.13 O catálogo apodrece à vista

Na tela de Atributos (Tratores), a coluna "Ultima alteração" mostra **2013, 2017, 2018, 2019**.
Modelos de trator que ninguém toca há mais de uma década, misturados com os atuais.

> **Correção:** todo catálogo tem `UltimoUsoEm` (documento 09, seção 11) e o job mensal de
> higienização. Item sem uso em 12 meses aparece marcado na tela de administração.

---

## 3. O que o Vórtice acerta na tela — e vamos manter

Seria injusto só listar defeitos. Três coisas boas:

| Acerto | Onde | Por que importa |
|---|---|---|
| **Processo, pessoa e tarefa na mesma linha** | grade da Agenda | o CEN vê o contexto sem abrir nada |
| **Coluna CEN visível na lista** | agenda do JOSE.RUFINO | responde "de quem é esse cliente" na hora |
| **"Docs do processo" no contexto certo** | barra de ação da Agenda | documento junto da tarefa, não num módulo à parte |
| **Cabeçalho do Andamento com o contexto** | `Agendado para 11/11/25 · Processo 1558647 · Agd: Cliente com Interesse em Peças` | quem vai dar andamento sabe do que se trata |

---

## 4. O mapa de navegação novo

Contra as 10 abas + 23 itens de configuração + aplicativo separado de relatórios:

```
┌─ BARRA SUPERIOR ─────────────────────────────────────────────────┐
│  CRM Tracbel   [busca global]        [empresa ▾]  [notificações] │
└──────────────────────────────────────────────────────────────────┘
┌─ MENU LATERAL ───────┐┌─ ÁREA DE TRABALHO ──────────────────────┐
│                      ││                                          │
│  ▸ Minha agenda   ⑦  ││   lista à esquerda + registro à direita  │
│  ▸ Leads          ⑫  ││   (Focused View)                         │
│  ▸ Clientes          ││                                          │
│  ▸ Oportunidades     ││                                          │
│  ▸ Relatórios        ││                                          │
│  ─────────────────   ││                                          │
│  ▸ Administração     ││                                          │
│    (só quem tem      ││                                          │
│     permissão)       ││                                          │
└──────────────────────┘└──────────────────────────────────────────┘
```

**Cinco destinos** para o usuário comum. Administração só aparece para quem tem a permissão —
`[V]` hoje o menu Configuração com 23 itens é visível a quem abre o módulo Supervisor.

**A tela de registro tem quatro áreas, não dez abas:**

| Área | Conteúdo | Vem de |
|---|---|---|
| **Resumo** | 7 campos no cabeçalho + barra de estágios | `[SF]` Compact Layout · `[DYN]` BPF |
| **Linha do tempo** | atividades, notas e andamentos unificados, com filtro | `[DYN]` Timeline (funde 3 abas do Vórtice) |
| **Dados** | seções recolhíveis, campos raros ocultos | `[SF]` Dynamic Forms |
| **Documentos** | anexos com vínculo real | corrige os 5.302 órfãos |

---

## 5. As seis telas da fase 1

### 5.1 Minha agenda

```
┌────────────────────────────────────────────────────────────────────┐
│  PRÓXIMA AÇÃO                                                       │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Monitorar cliente · AGROPECUARIA HBC IRMAOS DE SANTI          │  │
│  │ Vence hoje · Carteira MAQ_02ARA_04 · Processo #1578116        │  │
│  │ Último contato há 47 dias: "sem verba nesta safra"            │  │
│  │                                                                │  │
│  │   [ Dar andamento ]   [ Adiar ]   [ Pular ]                   │  │
│  └──────────────────────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────────────────────┤
│  Hoje (3)   ·   Atrasadas (12)   ·   Futuras (78)   ·   Todas      │
│  [Visões: Minhas ▾]  [Filtrar]                    [+ Nova tarefa]  │
├────────────────────────────────────────────────────────────────────┤
│  ⚑  Tarefa                  Cliente              Prazo      Ações  │
│  ●  Monitorar cliente       ABEL NATAL SCANDOL.  Atrasada    ⋯     │
│                                                  há 12 dias         │
│  ○  Qualificar lead         FRUCAMP IND E COM    Hoje        ⋯     │
│  ○  Registrar interesse     TESTE MATHEUS        em 3 dias   ⋯     │
└────────────────────────────────────────────────────────────────────┘
```

**Decisões:** a próxima ação vem antes da lista, porque a pergunta do CEN é "o que eu faço
agora", não "o que existe". Atraso tem **texto**, não só cor. As ações por linha estão num
menu **rotulado**. Sem "Processo DNA (=)".

### 5.2 Dar andamento — a tela mais crítica

```
┌────────────────────────────────────────────────────────────────────┐
│  Dar andamento                                              [ ✕ ]  │
│  Vender peças balcão · TESTE MATHEUS · Processo #1558647            │
│  Agendada para 11/11/2025 · Cliente com interesse em peças          │
├────────────────────────────────────────────────────────────────────┤
│  Qual foi o desfecho?                                               │
│                                                                     │
│   ▸ AVANÇA O PROCESSO                                               │
│     ○ Orçamento aprovado                                            │
│                                                                     │
│   ▸ MANTÉM EM ANDAMENTO                                             │
│     ○ Negociação em andamento                                       │
│     ○ Contato sem sucesso                                           │
│                                                                     │
│   ▸ ENCERRA COMO PERDA                                              │
│     ○ Desistiu da compra          (exige justificativa)             │
│     ○ Venda perdida               (exige justificativa)             │
│  ─────────────────────────────────────────────────────────────────  │
│   ▸ CANCELA                                              ⚠           │
│     ○ Atividade cancelada         (exige justificativa)             │
├────────────────────────────────────────────────────────────────────┤
│  O que aconteceu?                                                   │
│  ┌───────────────────────────────────────────────────────────────┐ │
│  │                                                                │ │
│  └───────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  ⓘ  Ao confirmar: o processo avança para Negociação e uma tarefa   │
│     "Registrar interesse" será criada para você, com prazo em 2     │
│     dias úteis.                                                     │
│                                                                     │
│                              [ Cancelar ]   [ Confirmar andamento ] │
└────────────────────────────────────────────────────────────────────┘
```

**As cinco correções desta tela:**

1. **Agrupado por classe**, com cabeçalho — não uma lista plana de seis itens iguais.
2. **Cancelamento separado por régua**, com ícone de alerta.
3. **Códigos numéricos sumiram.**
4. **Previsão da consequência** antes do clique, calculada pelo motor de regras. É a mesma
   informação que vai para `wf.RegraExecucao` — o usuário vê antes o que o log registraria depois.
5. **Justificativa obrigatória** em Perda e Cancelamento.

E, para o caso que originou tudo — matar uma agenda duplicada — existe **"Cancelar esta tarefa"**
no menu da própria tarefa, que não toca no processo.

### 5.3 Lista de leads (Kanban e tabela)

`[SF]` Path e Kanban **compartilham a mesma configuração** dos estágios. Um metadado, duas telas,
alternadas por um botão. Metade do trabalho de frontend.

### 5.4 Ficha do lead / cliente

```
┌────────────────────────────────────────────────────────────────────┐
│  AGROPECUARIA HBC IRMAOS DE SANTI LTDA        Cliente    #39773    │
│  ──────────────────────────────────────────────────────────────    │
│  CNPJ 11.222.333/0001-81 · MAQ_02ARA_04 · Ricardo Moretti          │
│  (17) 99999-0000 · Última interação há 47 dias                     │
├────────────────────────────────────────────────────────────────────┤
│  Qualificação → Monitoramento → [Interesse] → Encerrado             │
│  ▲ está aqui há 12 dias                                             │
├────────────────────────────────────────────────────────────────────┤
│  Linha do tempo   │   Dados   │   Documentos   │   Equipamentos     │
└────────────────────────────────────────────────────────────────────┘
```

Sete campos no cabeçalho, contra os ~40 de hoje. A barra de estágios mostra o **caminho
percorrido**, não só o ponto atual `[DYN]`.

### 5.5 Qualificar lead

Um passo só, com deduplicação em tempo real: ao digitar o documento, o sistema já avisa
*"Já existe um cliente com este CNPJ: AGROPECUARIA HBC — deseja vincular?"*.

`[V]` A fila de deduplicação do Vórtice tem **124 mil pares abandonados desde 2018**, porque a
verificação acontece depois, em lote, e ninguém olha. Aqui acontece na hora, onde o custo de
corrigir é zero.

### 5.6 Painel do gestor

Funil por estágio, conversão por origem, tempo médio por estágio, e — o que hoje não existe —
**tarefas paradas por responsável**. Se um CEN tem 194 tarefas atrasadas, o gestor precisa ver
isso antes de o cliente reclamar.

---

## 6. Comparativo de cliques

Contagem sobre as telas reais, para a tarefa mais frequente do CEN: **registrar um contato com
um cliente da carteira**.

| Passo | Vórtice hoje | CRM novo |
|---|---|---|
| Encontrar a tarefa | abrir Agenda → escolher filtro → Recarregar → localizar na grade de 195 linhas | está na **Próxima ação**, já aberta |
| Abrir o andamento | duplo clique na linha (ou ícone sem rótulo) | **1 clique** em "Dar andamento" |
| Escolher o resultado | abrir dropdown → achar entre itens com código numérico | **1 clique** no grupo certo |
| Escrever o relato | campo de texto sem indicação de obrigatoriedade | mesmo campo, com dica |
| Confirmar | Confirma | Confirmar |
| Saber o que aconteceu | **não sabe** — precisa voltar à agenda e procurar | a consequência foi **mostrada antes** |
| **Total** | **7 a 9 interações**, com busca visual | **4 interações**, sem busca |

O ganho não está nos cliques — está em **não precisar procurar** e em **saber o que vai acontecer**.

---

## 7. As regras de UX do projeto

Nove regras, vinculantes. Cada uma responde a algo observado nas capturas.

| # | Regra | Vem de |
|---|---|---|
| 1 | **Todo botão tem rótulo em texto.** Ícone acompanha, nunca substitui. | 2.6 |
| 2 | **Nenhum código interno na tela.** Só o número legível de processo. | 2.4 |
| 3 | **Ação destrutiva é visualmente distinta** e exige justificativa. | 2.2 |
| 4 | **O sistema prevê a consequência** antes de o usuário confirmar. | 2.2 |
| 5 | **Toda lista de seleção respeita o escopo** de quem está olhando. | 2.3 |
| 6 | **Estado nunca é comunicado só por cor.** | 2.10 |
| 7 | **Sete campos no resumo.** O resto se recolhe. | 2.9 |
| 8 | **A tela responde "o que eu faço agora"**, não só "o que existe". | 2.1 |
| 9 | **Se precisa explicar por escrito como usar, refaça.** | 2.12 |

---

## 8. O que falta capturar

| Tela | Por que importa |
|---|---|
| **Carteira** (`IVS1DPT05`) | é o modelo de carteira que estamos copiando — quero ver como o CEN é escolhido |
| **Cadastro de Ações e Resultados** | como o configurador cria um desfecho hoje; alimenta a tela de administração |
| **Vórtico Mobile Lite** | o campo usa em campo; define se a fase 1 precisa de responsivo real |
| **CRM Web** | a geração web do fornecedor — vale ver o que eles já tentaram |
| **Saída do TBA_101XX3** | vi os parâmetros, não o resultado; define o formato que as pessoas esperam |

Nenhuma delas bloqueia a fase 1. As seis telas da seção 5 estão especificadas o suficiente
para o dev de frontend começar.
