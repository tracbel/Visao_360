# Território, ADR e indicadores geográficos — cadastros, conciliação e o painel de mapas

> **Documento 32** · Versão 1.9 · 20/09/2026 — **o quarto mapa**: a estrutura agropecuária (tratores,
> propriedades, rebanho, densidade e usinas) e o painel "O mercado da região" chegaram à tela; o mapa
> C ganhou alternador entre máquinas teóricas, área plantada e valor da produção (issues 65 e 103).
> Detalhe em §8.4.
> Versão 1.8 · 20/09/2026 — **a PAM inteira, e um erro de ano na planilha do
> comercial**: a tabela passou a guardar as quatro medidas (área plantada, colhida, quantidade e
> valor) e ganhou ao lado a linha do total do estado; ao conferir contra o protótipo, as colunas de
> ÁREA dele mostraram-se rotuladas com um ano a mais, e a de valor, certa (issue 64). Detalhe em §8.3.1
> e §8.3.2.
> Versão 1.7 · 20/09/2026 — **o servidor tem área plantada de verdade pela primeira
> vez**: 54.570 linhas do ano 2025, pela variável 8331. No caminho, dois fatos que este documento
> afirmava errado: o servidor estava com **0 linha** (a sanitização de 15/09 levou as 45.582) e a
> leitura de todos os produtos de uma vez passou a ser recusada pelo SIDRA, agora em lotes de 20
> (issues 83, 95 e a publicação da 51). Detalhe em §3.3 (D-12) e §8.3.
> Versão 1.6 · 19/09/2026 — **a "área plantada" do mapa C era a área colhida**: o
> leitor pedia a variável 216 do SIDRA em vez da 8331. Corrigido no código com teste que prende a
> variável (§3.3 D-12 e §8.3).
> Versão 1.5 · 14/09/2026 — a taxonomia de produto que faltava (§3.5) chegou pelo ART:
> classificação de produto com porte nos filtros de equipamento, as vendas de máquina no banco local e a
> gestão "Grandes Contas" preservada por venda, sem virar SAM/KAM (§3.4). Detalhe no **documento 35, §10**;
> pendências P-31 a P-39.
> Versão 1.4 · 14/09/2026 — os três mapas lado a lado no quadro da ADR, cobertura
> vermelho→verde, período explícito, potencial por recorte, responsáveis das carteiras e os cinco
> cartões da Visão 360 com fonte e regra: tudo no **documento 36** (§11.10); P-2 e P-4 com as medidas
> novas; pendências P-26 a P-30.
> Versão 1.3 · 14/09/2026 — a tela do território vazia no servidor: causa medida,
> correção ensaiada numa cópia idêntica do banco, execução no servidor pendente, ART e fontes (§11.9 e
> documento 35).
> Versão 1.2 · 13/09/2026 — entrega parcial: a recarga do Vórtice deixa de
> desfazer as grafias corrigidas (§4.6.1); a visão da empresa foi validada na aplicação com perfis de
> teste (§8.5.5); os valores estão conciliados numa tabela só (§8.6); a matriz separa implementação
> de validação comercial (§2, §8.7); e os materiais de coleta estão no documento 34.
> Versão 1.1: correções da revisão independente, grafias cortadas corrigidas, visões de filial e de
> empresa, nota sem cliente no total e a matriz do pedido inteiro.
> Pedido de origem: orientações do gerente comercial para cadastros, limpeza do CRM, organização
> territorial e evolução da Visão 360 (três mapas de São Paulo por município).
> Convenções: **[medido]** = obtido ao vivo em 13/09/2026 (banco de desenvolvimento com a carga de
> 05–06/09/2026, e APIs públicas do IBGE); **[fonte]** = lido de planilha ou imagem recebida;
> **[proposta]** = sugestão ainda não confirmada pelo negócio; **[pendente]** = decisão ou dado que
> falta.
>
> **Este documento não tem nome de pessoa.** As planilhas trazem nome de funcionário por município;
> o detalhe nominal da conciliação fica em `dados-locais/` (fora do Git, ver `.gitignore`). Aqui
> ficam as contagens, as regras e os municípios — que são dado público.

---

## 0. Resumo

1. **Os 203 municípios da ADR estão confirmados por duas fontes**, e casam 203 de 203 com o
   cadastro oficial do IBGE. As duas planilhas concordam sobre **quais** municípios e sobre **qual
   loja** atende cada um; **discordam sobre o CEN em 82 deles** (e em outros 46 escrevem o nome de
   formas diferentes). Nenhuma das duas tem data de vigência. As duas foram guardadas lado a lado,
   sem escolher uma.
2. **A chave `%ChaveTerritorio` da planilha de CEN é o código IBGE sem o prefixo 35** — verificado
   em 203 de 203. É ela que liga o território à malha oficial do mapa, sem depender de grafia.
3. **Mapa de cobertura e mapa de vendas têm dado real.** O de potencial tem **só a parte que as
   fontes sustentam**: área plantada oficial do IBGE (PAM 2024) e a única regra informada — "1
   trator 3036N a cada 10 ha de café" —, que entra **marcada como não confirmada**. A área do IBGE
   serve para estimar a necessidade teórica **da região**: não comprova a área de propriedade de
   nenhum cliente e não substitui regra comercial de potencial (§8.3). Potencial de cliente e de
   não cliente não tem fonte hoje.
4. **Os números da maquete são ilustrativos.** Exemplo: a maquete mostra Ribeirão Preto com
   R$ 12,4 mi em máquinas; o banco mostra R$ 6,25 mi em máquina nos 12 meses fechados
   (set/2025–ago/2026) para clientes com endereço em Ribeirão Preto.
5. **Três filtros pedidos não têm dado para funcionar**: tipo de cliente (SAM/KAM/Varejo), tipo de
   produto e modelo nas vendas. A tela os mostra desligados, dizendo por quê.
6. **Grafias cortadas: 192 dos 194 endereços recuperados, e a recarga do Vórtice não os desfaz
   mais.** A carga do Vórtice mantém a correção enquanto a origem não muda e, no fim, roda sozinha a
   mesma conferência da carga do território. O ciclo completo — carga do Vórtice, carga do
   território e nova carga do Vórtice, com leitura real — não mudou o município de nenhum endereço.
   Os **2 endereços com coordenada conflitante** continuam na fila de conferência, sem associação
   (§4.6.1).
7. **Vendas nas duas visões.** A visão da **filial** usa as notas que a filial emitiu e o cadastro
   ao alcance dela; a visão da **empresa** junta as filiais e põe cada venda no município do
   cliente. A da empresa exige `Empresa.AlcanceEntreFiliais`, e **nenhum usuário real a tem**. Ela
   foi validada na aplicação com dois perfis de teste num banco isolado: o autorizado entra; o sem
   autorização vê a opção desligada, com o motivo, e recebe 403 mesmo forçando (§8.5.5). **Atenção:** a
   troca de **filial** pelo cabeçalho não confere se a filial é do usuário — defeito anterior a esta
   entrega, que depende de decidir quais filiais cada pessoa atende (D-11, P-20).
8. **Os valores numa tabela só (§8.6).** Em 12 meses foram emitidos **R$ 897,5 mi** =
   **R$ 559,5 mi** com cliente nas 13 filiais ativas (dos quais **R$ 321,6 mi** de cliente
   cadastrado em outra filial) + **R$ 17,4 mi** com cliente nas 3 filiais inativas +
   **R$ 320,6 mi** sem cliente no CRM (**R$ 315,9 mi** nas ativas + R$ 4,7 mi nas inativas). Os
   **R$ 217,1 mi** da captura são só a filial 010101 na visão da filial. O mapa-base tem os **645**
   municípios de São Paulo; o filtro ADR mostra os **203** conciliados.
9. **O que é medido e o que é provisório estão separados** na matriz (§2) e na tela (§8.7): vendas
   são medidas; pendência de visita e pós-venda usam regra comercial provisória; potencial é
   estimativa; o CEN tem duas fontes a confirmar, preservadas nos 82 municípios que divergem.
10. **Dado da empresa no GitHub:** inventário completo — **nenhuma credencial exposta**; login de
    funcionário em 80 arquivos, nome em 70, e-mail corporativo em 24 e documento de cliente real em
    4 (já trocado por fictício no disco). Recomendação: opção C+. **Não executado**: aguarda a sua
    aprovação (documento 33).
11. **Coleta:** 11 planilhas-modelo e 17 decisões priorizadas para o gerente, pré-preenchidas com o
    que o CRM e o Vórtice já têm (documento 34).
12. **A tela vazia no servidor (14/09/2026):** o território nunca tinha sido carregado no banco do
    servidor — 0 municípios com código IBGE, 0 ADR. A carga, ensaiada numa cópia idêntica desse banco,
    trouxe os 203 municípios, bateu ao centavo com a conciliação e não grava nada na segunda rodada. A
    execução no servidor ficou com você (`scripts/deploy/carregar-territorio-no-servidor.ps1`), e a tela
    passou a dizer "território não carregado" em vez de mostrar zero (§11.9, documento 35).

---

## 1. Fontes analisadas

| # | Fonte | O que foi lido | Estado |
|---|---|---|---|
| A | `Tabelas Exemplo.xlsx` | aba **Cliente**: 28 colunas, 3 linhas de exemplo; aba **Equipamento**: 19 colunas, 9 linhas de exemplo | lida por inteiro |
| B | Imagem com anotações do gerente sobre os cinco indicadores da Visão 360 | 5 anotações em vermelho | lida; ver 1.1 |
| B′ | Duas maquetes "VISÃO 360 — Indicadores Geográficos" | títulos, subtítulos, legendas, avisos, rodapé | lidas; trechos pequenos do rodapé da 2ª maquete **não são legíveis** e não foram interpretados |
| C | `Area de Atuação.xlsx` | 1 aba, 238 linhas, 12 colunas | lida por inteiro |
| D | `CEN e Gestor por Municipio.xlsx` | 1 aba, 293 linhas, 6 colunas | lida por inteiro |
| E | Projeto (código, documentos 24–31), banco de desenvolvimento, Vórtice (extração já salva), IBGE (localidades, malha municipal, PAM/SIDRA 5457) | — | ver seções 3 e 4 |
| — | ART (MySQL) | `aftracbel.tracbel.com.br` agora resolve para **10.235.0.59**; porta 3306 **sem resposta** desta estação [medido] | **inacessível** |

### 1.1 As anotações do gerente, uma a uma

| Indicador na tela | Anotação legível | Requisito extraído |
|---|---|---|
| Faturamento em curso | "ART ou Protheus" | R-01: faturamento vem do ERP (Protheus) ou do ART |
| Previsão FY 2026 | "ART ou Arquivo de Meta" | R-02: previsão depende de meta (arquivo) ou do ART |
| Clientes na carteira | "Sanitização (CRM e Protheus) e parte transacional de cadastros" | R-03: base de clientes saneada cruzando CRM e Protheus |
| Cobertura ativa | "Transacional de visitas por carteira (definição no CRM)" | R-04: cobertura mede visitas por carteira, com a definição que o CRM declara |
| Conhecimento de mercado | "ART + Vendas perdidas (Transacional CRM) + Dados externos" | R-05: mercado combina ART, vendas perdidas e dado externo |

### 1.2 O que as maquetes pedem, e o que nelas é ilustrativo

As duas maquetes mostram três mapas de São Paulo com a ADR em destaque, um cabeçalho com
"CEN · Gerente · Diretora", legendas em faixas e avisos. **Os nomes de pessoa e todos os números
das maquetes são fictícios**: a segunda maquete diz isso no próprio rodapé, e os valores não
batem com o banco (seção 0, item 4).

Duas inconsistências das próprias maquetes, que precisam de decisão (seção 10):

- O mapa de vendas diz **"FYTD"** no título e **"últimos 12 meses"** no subtítulo. Não é o mesmo
  período.
- O mapa de cobertura usa **A 30d · B 60d · C 90d · D 120d**. A cadência que o Vórtice declara é
  outra: 180/180/180/360 dias em Máquinas, 120/120/120/180 em Prospecção, 360 em Peças e AMS
  (documento 27). Os números 30/60/90/120 vêm do protótipo, não do sistema.

---

## 2. Matriz do pedido — todos os requisitos

Classificação da versão 1.2, que **separa implementação de validação comercial**:

- **Validado** — *implementado e validado*: construído, testado e conferido contra fonte
  independente, sem depender de regra comercial em aberto;
- **Provisório** — *implementado com regra comercial provisória*: funciona e está conferido
  tecnicamente, mas a regra que dá sentido comercial ao número não foi confirmada; a tela diz isso
  com um selo (§8.7);
- **Pendente técnico** — *pendente de implementação técnica*: falta trabalho de TI que não depende de
  decisão;
- **Bloqueado** — *bloqueado por dado ou decisão específica*: a coluna "O que falta" diz qual.

**Pós-venda, visita e potencial não aparecem como validados comercialmente** enquanto as definições
estiverem pendentes. A evidência aponta para a seção onde a prova está; a planilha citada é do
documento 34. Os identificadores R-xx da versão 1.0 continuam citados no §5: R-13 = O-19,
R-14 = O-20, R-15 = O-35, R-18 = O-30.

### 2.1 Cadastros, limpeza e rastreabilidade

| ID | Requisito | Classificação | Evidência | O que falta |
|---|---|---|---|---|
| O-01 | Cadastro de **clientes** conforme `Tabelas Exemplo.xlsx` | **Bloqueado** — mapeado campo a campo; modelo não alterado | §6.1, §6.3 | Fonte dos campos novos: o ART, inacessível (P-11). Decisões: unidade de "Extensão Area", origem do "ID Cliente", cliente com CPF **e** CNPJ, sentido de "Filial Atendimento" (planilha 05) |
| O-02 | Cadastro de **equipamentos** conforme `Tabelas Exemplo.xlsx` | **Bloqueado** — mapeado; ano, horímetro e local vazios nos 3.888 | §6.2, §3.1 | Mesma fonte (ART / Operation Center). Decisões: "POPs", "Peso", "Motor", "Filial Cliente", taxonomia (planilhas 06 e 07) |
| O-03 | Limpeza do CRM **sem fundir nem completar registro ambíguo** | **Validado** para município e endereço | §4.4, §4.6 | A limpeza de cliente CRM × Protheus não foi refeita nesta rodada (documentos 16 e 31) |
| O-04 | Rastreabilidade: valor original preservado, correção registrada | **Validado** | §4.4, §4.6, §4.6.1 | — |
| O-05 | Importação idempotente, inclusive recarga do Vórtice seguida da carga do território | **Validado** — ciclo completo sem alteração indevida | §4.6.1, §11.2 | — |
| O-06 | Conciliar os 203 municípios da ADR com o IBGE | **Validado** — 203 de 203; 645 no mapa-base | §4.2, §8.6 | **Bloqueado**: se a lista é a vigente, e desde quando (P-1, C-5) |
| O-07 | Município × CEN × gestor, com origem e vigência | **Bloqueado** na vigência — origem preservada, as duas fontes lado a lado com a comparação | §4.3, §8.7 | As planilhas não têm vigência; 82 municípios com CEN diferente (P-1; planilha 02) |
| O-08 | Corrigir grafias cortadas só com correspondência inequívoca | **Validado** — 192 de 194, mantidas em toda recarga | §4.6, §4.6.1 | 2 endereços com coordenada em outro município (P-15) |
| O-09 | Lista das tabelas e dados que você precisa fornecer | **Validado** — com planilhas-modelo e decisões priorizadas | §5, §10.1, documento 34 | — |

### 2.2 As observações da imagem do gerente

| ID | Anotação | Classificação | Evidência | O que falta |
|---|---|---|---|---|
| O-10 | Faturamento em curso — "ART ou Protheus" | **Provisório** — valor medido e conciliado ao centavo; período e devolução sem decisão | documento 30; §8.2, §8.6 | FYTD ou 12 meses (P-4); devolução e cancelamento (P-5) |
| O-11 | Previsão FY 2026 — "ART ou Arquivo de Meta" | **Bloqueado** | `organizacao.Meta` = 0 linhas [medido] | Arquivo de meta ou ART, e o início do ano fiscal (P-4; planilha 04) |
| O-12 | Clientes na carteira — "sanitização CRM e Protheus" | **Provisório** | documento 31, seção 4; §3.1 | Confirmar a regra de sanitização CRM × Protheus do documento 31 |
| O-13 | Cobertura ativa — "transacional de visitas por carteira" | **Provisório** — cadência declarada no CRM; "visita" = qualquer interação | §8.1, §8.7; D-1 | O que conta como visita e a periodicidade (P-2, P-3; planilha 10) |
| O-14 | Conhecimento de mercado — "ART + vendas perdidas + dados externos" | **Bloqueado** | `processo.VendaPerdida` = 165 linhas [medido]; ART inacessível | Acesso ao ART (P-11) e fonte externa de parque ou emplacamento (§5, grupo 14) |

### 2.3 Painel e indicadores

| ID | Requisito | Classificação | Evidência | O que falta |
|---|---|---|---|---|
| O-15 | Mapa A — pendência de visita e cobertura de carteira | **Provisório** — selo "Regra provisória" | §8.1, §8.7, §11.3 | Definição de visita, periodicidade e unidade (P-2, P-3) |
| O-16 | Mapa B — vendas por município | **Provisório** — valor conferido ao centavo (§8.6); o período e o abatimento de devolução dependem de decisão | §8.2, §8.5, §8.6 | P-4, P-5 |
| O-17 | Mapa C — potencial por cultura × área | **Provisório** — selo "Estimativa · regra a confirmar" | §8.3, §8.7 | Confirmar a regra do café e dar as das outras culturas (P-8; planilha 08) |
| O-18 | Valores de pós-venda | **Provisório** — peça + serviço, sem dupla contagem; selo "Composição provisória" | §8.4, §8.7, §11.3 | O que a diretoria considera pós-venda (planilha 11) |
| O-19 | Filtro SAM, KAM e Varejo | **Bloqueado** | §3.4 | Definição e lista de clientes classificados (P-6; planilha 03). O ART tem `gestao` Varejo / Grandes Contas **por venda** — insumo para a decisão, não classificação de cliente (documento 35, §7.7) |
| O-20 | Filtros tipo de produto e modelo | **Bloqueado** | §3.5 | Taxonomia de produto e autorização para trazer o item da nota (P-7; planilha 07). O ART traz `linha` (14) e `produto` (126) por chassi vendido — fonte candidata para a taxonomia (documento 35, §7.7) |
| O-21 | Filtros município, ADR, período, visão, filial, CEN, gestor e cultura | **Validado** para município, região e loja da ADR, período, visão, filial que vendeu e filial do cliente; **Bloqueado** para CEN e gestor; cultura segue O-17 | §8.5.5, §8.6, §11.4 | P-1 |
| O-22 | Potencial de **clientes** | **Bloqueado** | `Endereco.Hectares` e `CulturaId` vazios em 100% (§3.1) | Propriedades e culturas por cliente (P-11; planilha 05) |
| O-23 | Potencial de **não clientes** | **Bloqueado** | não existe cadastro de propriedade de não cliente | Base de propriedades rurais (ART ou CAR/SICAR; planilha 05) |
| O-24 | Potencial da **região** | **Provisório** — estimativa | §8.3, §8.7 | Regras por cultura (P-8) |
| O-25 | Cultura e área no cálculo | **Provisório** — área regional do IBGE, só café | §8.3 | Área e cultura por propriedade (ART) |
| O-26 | Ciclo de troca no cálculo | **Bloqueado** | §5, grupo 13 | A regra de vida útil por categoria (P-9; planilha 09) |
| O-27 | Concorrentes no cálculo | **Bloqueado** | catálogos `CONCORRENTE` (23) e `REVENDA_CONCORRENTE` (32) existem; parque concorrente não | Fonte de parque ou de emplacamento (P-9) |
| O-28 | Área plantada do IBGE com finalidade explícita | **Validado** | §8.3; tela (subtítulo e aviso do mapa C); API (`potencialDosNaoClientes`) | — |
| O-29 | Definição de cada indicador | **Validado** — com o que é provisório nomeado | §8, §8.7 | — |
| O-30 | Rascunho visual antes de consolidar | **Validado** — fora do Git | §11.4 | — |

### 2.4 Visões, permissões e vendas por filial

| ID | Requisito | Classificação | Evidência | O que falta |
|---|---|---|---|---|
| O-31 | Visão consolidada da empresa | **Validado** tecnicamente — testes e aplicação com perfis de teste; **Bloqueado** na distribuição | §8.5.2, §8.5.5 | Quem recebe o acesso (P-10; planilha 01). Nenhum usuário real tem. Atenção: a **filial** do cabeçalho não é conferida contra o usuário (D-11, P-20) |
| O-32 | Filtro por filial | **Validado** o filtro; **Bloqueado** a restrição de quais filiais cada usuário pode escolher | §8.5.1, §11.3 | D-11, P-20 |
| O-33 | Filial que vendeu × filial de cadastro do cliente | **Validado** — dois filtros independentes | §8.5.1, §8.5.5 | — |
| O-34 | Explicar os valores sem omissão nem dupla contagem | **Validado** — tabela única de conciliação | §8.5.3, §8.5.4, §8.6 | A quem pertence o cliente que compra em várias filiais (P-16) |
| O-35 | Visões de CEN, gerente e diretor | **Bloqueado** | §3.6 | Lista oficial de usuários com cargo e gestor, e o que cada cargo vê (P-10; planilha 01) |

### 2.5 Segurança, processo e entrega

| ID | Requisito | Classificação | Evidência | O que falta |
|---|---|---|---|---|
| O-36 | Remover dado da empresa do histórico remoto | **Bloqueado** — procedimento preparado, não executado | documento 33 | Sua aprovação escrita, com a opção (recomendada: C+) (P-12) |
| O-37 | Inventário do histórico: credenciais, identificadores e dados pessoais, inclusive pesquisa e comentários | **Validado** | documento 33, §1 | — |
| O-38 | Fontes somente leitura (Vórtice, Protheus) | **Validado** — nenhuma escrita | a carga lê planilha, IBGE, Vórtice e o banco do CRM | — |
| O-39 | Testes, validação no navegador, revisão independente | **Validado** | §11 | — |
| O-40 | Documentação e memória atualizadas | **Validado** | este documento, documentos 33 e 34, memória do projeto | — |
| O-41 | Repositório em `E:\CraftOps` | **Bloqueado** | — | P-13 |
| O-42 | A recarga do Vórtice não desfaz a correção das grafias, sem depender da ordem das cargas | **Validado** | §4.6.1 | — |
| O-43 | Estimativas e regras provisórias identificadas na interface | **Validado** | §8.7, §8.5.5 | — |
| O-44 | As duas fontes preservadas nos 82 municípios com CEN divergente | **Validado** — preservação; a vigência segue O-07 | §4.3, §8.7 | P-1 |
| O-45 | Planilhas-modelo e decisões priorizadas para a coleta | **Validado** | documento 34 | Encaminhar ao gerente |
| O-46 | Conferir o texto das 47 capturas de tela do histórico, que a varredura não lê | **Pendente técnico** | documento 33, §1.2 | Leitura das imagens; não muda a recomendação do documento 33 |
| O-47 | Importar as planilhas devolvidas | **Pendente técnico** | documento 34, §5 | Depois que o gerente validar o formato |
| O-48 | Publicação no servidor | **Validado** — publicado em 13/09/2026, migração aplicada, arquivos conferidos por hash | §11.8 | Carga do território no banco do servidor; consentimento do Entra ID (P-19) |
| O-49 | Teste automatizado da carga do Vórtice mantendo a correção (`GravarEnderecosAsync`, `GravarMunicipiosAsync`) | **Pendente técnico** | §4.6.1 — provado em rodada real; a conferência das grafias tem teste próprio | Teste com banco em memória e leitor falso do Vórtice |
| O-50 | Dados reais visíveis na tela do servidor | **Validado no banco** — a carga do território rodou no servidor em 14/09/2026 12:49: 5.571 municípios com código IBGE, 203 da ADR e 45.582 linhas de área plantada (medido); a conferência visual com login no servidor é sua | documento 35, §2, §3 e §5 | Abrir a tela no servidor e conferir |
| O-51 | Diferenciar ausência de registro, falta de permissão e falha de carregamento | **Validado** localmente — aviso de território não carregado, 403 e 401 com mensagem própria; não publicado | documento 35, §4 | Publicar quando liberado |
| O-52 | Conectar ao banco do ART e inspecionar o esquema | **Validado** — conecta, autentica e lê (14/09/2026); o usuário só enxerga a view de vendas `bi_art_veiculos` (4.144 vendas, 72 colunas) | documento 35, §7.4 a §7.6 | Leitura das tabelas de propriedade, área, cultura e horímetro (P-11) |
| O-53 | Parar de levar dados da estação para o servidor | **Pendente técnico** — cargas já podem gravar direto no servidor (script do território); falta instalar e agendar a carga no próprio servidor | documento 35, §6 | Decisão e acesso de administrador no servidor (P-24) |
| O-54 | Fonte que sustenta cada informação | **Validado** para CRM, Protheus, Vórtice, IBGE e planilhas; **Bloqueado** para o ART | documento 35, §8 | P-11; credencial do Protheus na sessão para refazer a comparação (P-25) |

---

## 3. Diagnóstico — de onde cada número chega na tela

### 3.1 O que existe e está preenchido [medido]

| Tabela | Linhas | O que importa para este pedido |
|---|---:|---|
| `comercial.Cliente` | 23.945 | classe ABC apurada (A 428 · B 610 · C 2.767 · D 20.140) |
| `comercial.Endereco` | 19.641 | um endereço **Fiscal** por cliente; município do catálogo em 19.220; **`Hectares` e `CulturaId` vazios em 100%**; coordenada em 4.154 |
| `comercial.ClienteCarteira` | 49.109 | vínculo cliente × carteira com a data da última interação |
| `organizacao.Carteira` | 142 | carteira comercial, administrativa ou de teste |
| `organizacao.LinhaDeNegocio` | 14 | cadência declarada em 4 linhas (180/120/360) |
| `organizacao.Municipio` | 9.750 | **nenhum com código IBGE**; 951 de SP, dos quais 334 não são municípios do IBGE (distritos, "A CADASTRAR") |
| `organizacao.CarteiraMunicipio` | 532 | cidades por carteira, do Vórtice |
| `comercial.FaturamentoDoCliente` | 37.867 | cliente × filial × mês, set/2023 a set/2026, com quebra máquina/peça/serviço/outros |
| `processo.Interacao` | 122.002 | 01/01/2026 a 06/09/2026; só 2 tipos de tarefa têm "visita" no nome |
| `frota.Equipamento` | 3.888 | todos com cliente; **ano, horímetro e endereço vazios em 100%** |
| `frota.Modelo` | 253 | nomes sujos (11 variações de 5090E); **não existe 3036N** |
| `seguranca.Usuario` | 276 | papel declarado em 3; **gestor declarado em 0** |
| `organizacao.HierarquiaComercial`, `organizacao.Meta`, `organizacao.Praca` | 0 | vazias |

### 3.2 O caminho de cada indicador geográfico

| Indicador | Fonte | Transformação | Cálculo | Exibição |
|---|---|---|---|---|
| Cobertura (mapa A) | `ClienteCarteira.UltimaInteracaoEm` ← `processo.Interacao` ← Vórtice | vínculo em carteira comercial; município pelo endereço principal do cliente | coberto / fora da cadência / nunca contatado / sem cadência, contra a cadência da linha (documento 27) | % pendente por município |
| Vendas (mapa B) | `FaturamentoDoCliente` ← SD2 do Protheus | CFOP de venda, nota normal, grupo do item classifica máquina/peça/serviço/outros (documento 30) | soma por município do endereço principal do cliente, no período | R$ por município |
| Potencial (mapa C) | PAM/IBGE (área plantada) + `RegraDePotencial` | área do produto da regra | área ÷ hectares por máquina | máquinas teóricas por município |

### 3.3 Defeitos e riscos encontrados

| # | Achado | Efeito | Tratamento |
|---|---|---|---|
| D-1 | "Cobertura ativa" da Visão 360 conta **contato em 30 dias**, número fixo | contradiz a cadência declarada (180/120/360); mostra 29% como se fosse descumprimento | já apontado no documento 31; o mapa novo usa a cadência declarada |
| D-2 | Catálogo de municípios sem código IBGE, com nome cortado em 20 caracteres e grafias duplicadas por apóstrofo | 6 municípios da ADR só casam por prefixo; 5 aparecem em duas linhas | reconhecimento pelo IBGE, §4.4; endereços das grafias cortadas corrigidos, §4.6 |
| D-3 | Nenhuma área nem cultura por cliente | potencial de cliente impossível | bloqueado pelo ART, §5 |
| D-4 | Faturamento sem produto nem quantidade | filtro por modelo e tipo de produto impossível nas vendas | §3.5 |
| D-5 | 4.725 clientes sem município no endereço, com R$ 24,4 mi faturados em 12 meses (quase tudo máquina) | ficam fora do mapa | mostrados como "sem município" no total, nunca somados a uma cidade |
| D-6 | O faturamento é atribuído ao **endereço do cliente**, não ao local da entrega nem da propriedade | cliente com fazendas em várias cidades cai numa só | declarado na tela; §8.2 |
| D-7 | Filiais Guaíra, Ituverava e Monte Alto estão **inativas** no CRM (documento 26) mas respondem por 11 municípios da ADR na planilha | divergência entre fontes | registrada, §4.5 |
| D-8 | `Tabelas Exemplo.xlsx`, a extração do Vórtice e trechos de documentos, testes e protótipo com dado de cliente e de funcionário no histórico do GitHub | exposição de dado pessoal e de identificadores; nenhuma credencial | inventário e recomendação no documento 33 v2.0; documentos de cliente real trocados por fictícios no disco; reescrita não autorizada |
| D-9 | Nota sem cliente no CRM fora do total dos mapas — achado na revisão | R$ 320,6 mi de 12 meses sumiam da conta | corrigido, §8.5.4 |
| D-10 | A recarga do Vórtice devolvia 78 endereços corrigidos para a grafia cortada | a correção dependia de alguém rodar a carga do território depois | corrigido, §4.6.1 |
| D-11 | **A filial vem do cabeçalho e não é conferida contra o usuário** — anterior a esta entrega, achado na 3ª revisão. Com o Entra ID ligado ou desligado, qualquer usuário autenticado escolhe qualquer filial ativa. **Fora deste documento, cite este achado como P-20**: `D-11` sozinho, nos documentos 40, 41 e 46, é a decisão da semente estrutural (colisão C-11, resolvida em 20/09/2026 pela #44) | vê clientes, vendas e cobertura de outra filial; somando as 13, reconstrói quase toda a visão da empresa sem `Empresa.AlcanceEntreFiliais` | **não corrigido**: restringir exige decidir quais filiais cada pessoa atende (P-20) |
| D-12 | **A "área plantada" era a área colhida** — o leitor do IBGE pedia a variável 216 ("Área colhida") da tabela 5457 e chamava o resultado de área plantada; a variável certa é a **8331** ("Área plantada ou destinada à colheita") | o mapa C e a métrica `potencialDosNaoClientes` saíam **subestimados** onde há plantio novo: em cultura perene (café, laranja) a área colhida fica abaixo da plantada enquanto o cafezal ou o pomar não produz | ✅ **fechado em 20/09/2026.** Código corrigido em 19/09 (issue 83) com teste que prende a variável; servidor recarregado em 20/09 com **54.570 linhas do ano 2025** pela 8331. Duas correções de rota no caminho: o servidor estava com **0 linha** (a sanitização de 15/09 as levou, e este documento afirmava 45.582) e o SIDRA passou a recusar a consulta de todos os produtos, resolvida com lotes de 20 (issue 95). Detalhe em §8.3 |

### 3.4 SAM, KAM e Varejo: não existe classificação por cliente [medido]

A sigla aparece em quatro lugares do Vórtice, nenhum deles uma classificação de cliente:

| Onde | O que é |
|---|---|
| Formulário `ACOMP_VENDA_DIRETA`, pergunta "Tipo de Cliente" | opções KAM e SAM — resposta **por venda direta** |
| Formulário `ACOMP_VENDA_DIRETAJD`, pergunta "Tipo de Cliente" | opções SAM, KAM, Consultivo e CAP — idem |
| Parâmetro `CRMPES_FAIXAFATURA` e atributo `RENDAFAT` | SAM e KAM misturados com faixas de faturamento em reais |
| `GE_Pessoa.Grupo` | "Varejo" em 85.483 pessoas, ao lado de Cliente, Diversos, Contas Chave, Funcionário… |
| `GE_PessoaClasse.Classe` | SAM em 46 linhas, ao lado de FILIAL, FUNCIONÁRIOS, FALECIDO |

"KAM" na tabela VV1 do Protheus é **código de marca**, não classificação. **Conclusão:** o filtro
precisa de uma definição do negócio e de uma lista — ver §5, grupo 4.

**Versão 1.5:** o ART traz `gestao` (Varejo 3.235, Grandes Contas 909 vendas) **por venda**. O valor ficou
em `frota.VendaDeMaquina.GestaoNaOrigem`, como veio, e não classifica o cliente nem vira SAM/KAM: um
cliente pode ter compras nas duas gestões. A regra comercial é a pendência P-31 (documento 35, §10.10).

### 3.5 Tipo de produto e modelo nas vendas

O faturamento carregado tem o grão cliente × filial × mês. O produto existe na nota (SD2) mas não
foi trazido, e o catálogo de modelos não separa trator grande, médio e compacto: `frota.Familia`
tem "Trator", "Colhedora de Cana", "Pulverizador", "Implemento". A planilha de equipamento traz
`Linha Produtos` ("TRATORES COMPACTOS", "TRATORES MÉDIOS") — é a taxonomia que falta, mas ela vem
de uma fonte que não alcançamos.

**Versão 1.5 — a taxonomia existe.** `frota.LinhaDeProduto` guarda a classificação com porte (trator
pequeno, médio e grande, colhedora de cana, colheitadeira, plantadeira, pulverizador, plataforma de corte
e implementos), apontando para a família compatível. As 14 linhas do ART entram por tabela explícita, e a
lista de equipamentos filtra por classificação, porte e "com venda" no banco. No banco local, 2.106
máquinas com venda do ART: 823 tratores pequenos, 227 médios, 527 grandes (documento 35, §10.7). O filtro
por porte no **mapa B** ainda não existe: o faturamento continua no grão cliente × filial × mês.

### 3.6 Perfis CEN, gerente e diretor

A fronteira que existe é a **filial** (documento 05). Com o login do Entra ID ou com a ponte
provisória, o usuário alcança a filial dele e as abaixo — e as 18 empresas cadastradas **não têm
hierarquia** (todas no nível 0) [medido]. Portanto:

- o **CEN** e o **gerente** veem os clientes e as vendas da filial escolhida;
- o **diretor** tem a visão da empresa implementada; ela exige `Empresa.AlcanceEntreFiliais` e **nenhum usuário real a tem**: a API responde 403. A concessão explícita por conjunto de permissão funciona e foi validada só com perfis de teste (§8.5.5);
- não há papel confirmado por usuário (3 de 276 têm `Papel`), nem gestor declarado.

Não foi inventada regra de permissão. O mapa respeita a fronteira que existe e diz na tela qual
alcance está sendo mostrado. O que falta está no §5, grupo 2.

---

## 4. Conciliação da ADR

### 4.1 As duas planilhas [fonte] [medido]

| | `Area de Atuação.xlsx` | `CEN e Gestor por Municipio.xlsx` |
|---|---|---|
| Linhas de dado | 238 | 293 |
| Linhas com município | 238 | 203 (90 linhas só com a chave e "-") |
| Marcados como ADR | **203** ("Área ADR"), 35 sem marcação | — |
| Municípios únicos após normalização | 238 | 203 |
| Duplicidades | 0 | 0 |
| Identificador | `%ChvMunicipio` = NOME\|SP\|BRASIL | `%ChaveTerritorio` = **código IBGE sem "35"** (203/203) |
| Grafia | oficial, com acento e apóstrofo | sem acento, "De"/"Da" em maiúscula |
| Região | Noroeste 120 · Norte 83 | — |
| Loja/filial | 16 lojas, código de 3 letras | 16 lojas, nome |
| CEN | coluna `CEN` (45 valores distintos, incluindo 4 "a contratar") | coluna `Vendedor_Territorio` (46 valores) |
| Gestor | — | coluna `Gerente_Territorio` (12 valores) |
| Coordenadas | latitude/longitude em 203 | — |

### 4.2 Resultado

| Verificação | Resultado |
|---|---|
| ADR da planilha C ∖ planilha D | **vazio** |
| Planilha D ∖ ADR da planilha C | **vazio** |
| Municípios da planilha D entre os 35 fora da ADR | **nenhum** |
| Casam com o IBGE por nome (645 municípios de SP) | **238 de 238** e **203 de 203** |
| Loja divergente entre as planilhas | **0 de 203** |
| Diferença só de grafia (acento, maiúscula, apóstrofo) | 96 municípios — mesmo município, nenhuma decisão necessária |
| Coordenada da planilha fora do contorno IBGE do município | 0 |
| Os 35 fora da ADR | não têm CEN, filial, região nem coordenada na fonte |

**O total de 203 se confirma sem remover nem acrescentar nada.** Cuidado com a leitura: as duas
planilhas trazem exatamente a mesma contagem de municípios por loja, o que sugere uma **origem
comum**. A concordância entre elas não é prova independente de que a lista está atual.

### 4.3 Os responsáveis: onde as fontes discordam

**CEN**, comparando `CEN` (planilha C) com `Vendedor_Territorio` (planilha D), município a
município:

| Comparação | Municípios |
|---|---:|
| Mesmo nome, ignorando ponto, caixa e acento | 75 |
| Mesmo primeiro nome ou um contido no outro (provável mesma pessoa, **não confirmado**) | 46 |
| Nomes diferentes | **82** |
| …dos quais a planilha C declara vaga "a contratar" e a D traz um nome | 19 |
| Célula com duas pessoas ("X // Y") na planilha C | 4 |

A presença de vagas "a contratar" na planilha C e de nomes na D, para os mesmos municípios, é
compatível com a C ser um **planejamento mais recente** e a D, um **retrato do CRM** — é hipótese,
não fato. **Nenhuma das duas foi escolhida.**

**Na tela e na API (versão 1.2)** cada município traz `comparacaoDoCen`, com a regra da tabela acima
(`ResponsavelPeloMunicipio.CompararCen`): `MesmoNome`, `ProvavelMesmaPessoa` ou `NomesDiferentes`. O
cartão mostra **82** municípios com CEN diferente, "mais 46 com grafia diferente (provável mesma
pessoa, não confirmado)", e o detalhe mostra as duas fontes com o selo "Duas fontes · vigência a
confirmar". A comparação é rótulo, não fusão: as duas afirmações continuam gravadas até existir
definição oficial (P-1).

**Identificação no cadastro de usuários**, por igualdade exata do nome normalizado com o login, o
nome de exibição ou o nome completo (sem aproximação):

| Coluna | Valores distintos | Casam com 1 usuário | Sem usuário |
|---|---:|---:|---:|
| `CEN` (C) | 45 | 17 | 28 (inclui 4 vagas e 2 células com duas pessoas) |
| `Vendedor_Territorio` (D) | 46 | 36 | 10 |
| `Gerente_Territorio` (D) | 12 | 8 | 4 |

**Gestor:** só a planilha D o declara. Todo município da ADR tem gestor nela; cada loja tem um só
gestor, e há gestor com mais de uma loja.

### 4.4 O catálogo de municípios contra o IBGE [medido]

Regra de reconhecimento, na ordem, e **nenhuma fusão automática**:

1. nome normalizado igual ao do IBGE na mesma UF (maiúscula, sem acento, hífen como espaço,
   apóstrofo preservado);
2. igual ao IBGE depois de trocar apóstrofo por espaço — **só se o código ainda não foi tomado**
   por outra linha;
3. nome com exatamente 20 caracteres (a marca d'água de truncamento do documento 26) que é prefixo
   de **um único** município do IBGE na UF, com o código livre;
4. o resto fica sem código e é contado.

A segunda linha de uma grafia duplicada (ex.: `ESTRELA D OESTE` ao lado de `ESTRELA D'OESTE`) **não
é fundida**: fica sem código, e os endereços que apontam para ela aparecem no total como "município
sem código IBGE" para revisão. O nome anterior de toda linha renomeada vai para
`auditoria.AlteracaoDeCampo`.

Resultado da carga sobre o catálogo inteiro [medido]:

| | Linhas |
|---|---:|
| Municípios oficiais lidos do IBGE | 5.571 |
| Reconhecidas por nome igual | 5.071 |
| Reconhecidas pela grafia do apóstrofo | 2 |
| Reconhecidas por nome cortado em 20 caracteres | 385 |
| Municípios oficiais criados (não havia linha) | 113 |
| Segundas grafias, **não fundidas** | 10 |
| Linhas sem município do IBGE (distrito, "A CADASTRAR", UF errada) | 4.282 |
| Nome anterior gravado em `auditoria.AlteracaoDeCampo` | 5.849 registros (código + nome) |

**A trilha não guarda a troca que só muda caixa e acento** (`RIBEIRAO PRETO` → `Ribeirão Preto`):
a restrição `CK_AlteracaoDeCampo_Mudou` compara sob a colação do banco, para a qual os dois são o
mesmo texto. A grafia original continua recuperável pela chave de origem do Vórtice em
`integracao.ChaveExterna`.

### 4.5 Divergências registradas para decisão

| # | Divergência | Valores | Decisão necessária |
|---|---|---|---|
| C-1 | CEN diferente entre as planilhas | 82 municípios (§4.3) | qual planilha é a vigente, e desde quando |
| C-2 | Vaga "a contratar" numa fonte e nome na outra | 19 municípios | a vaga está aberta hoje? |
| C-3 | Duas pessoas na mesma célula | 4 municípios | quem responde, ou se é responsabilidade compartilhada |
| C-4 | Filial inativa no CRM respondendo por município da ADR | Guaíra (2), Ituverava (5), Monte Alto (4) | as três lojas operam? |
| C-5 | Os 35 municípios fora da ADR na planilha C | sem nenhum atributo | o que eles são: prospecção, histórico, erro? |
| C-6 | Nomes que não casam com usuário do CRM | 28 + 10 + 4 | cadastro de usuários do comercial (§5, grupo 2) |

### 4.6 As grafias cortadas: 192 de 194 endereços recuperados [medido]

**535 endereços** apontavam para linhas do catálogo que ficaram sem código IBGE (§4.4). Três dessas
linhas são grafias cortadas em 20 caracteres de municípios da ADR, cujo código já pertence à linha
de nome completo. Com a autorização de 13/09/2026, os endereços delas foram corrigidos **só com
correspondência inequívoca**:

| Condição | Como foi verificada |
|---|---|
| A grafia cortada é prefixo de **um único** município do IBGE na mesma UF | reconhecimento do catálogo (§4.4, regra 3) |
| O código IBGE desse município já está na linha de nome completo | catálogo |
| A UF do endereço é a do município oficial | `Endereco.Uf` |
| Se o endereço tem coordenada, ela cai **dentro do contorno oficial** do município | malha municipal do IBGE, ponto no polígono |

**Nenhum endereço foi associado por semelhança de nome.** A correção muda o ponteiro de município
de endereços que **já apontavam** para a grafia cortada. Endereço excluído não é tocado.

| Grafia cortada (linha do catálogo) | Município oficial | Endereços | Recuperados | Pendentes |
|---|---|---:|---:|---:|
| `SAO JOSE DO RIO PRET` (9490) | São José do Rio Preto (3549805) | 114 | 114 | 0 |
| `SANTANA DA PONTE PEN` (9450) | Santana da Ponte Pensa (3547205) | 47 | 46 | 1 |
| `SAO JOAO DAS DUAS PO` (9478) | São João das Duas Pontes (3549201) | 33 | 32 | 1 |
| **Total** | | **194** | **192** | **2** |

Dos 192, **38 têm coordenada e ela cai dentro do contorno oficial**; **154 não têm coordenada** e
foram corrigidos pelas outras três condições. Os outros 341 endereços sem código (535 − 194) apontam
para distrito, "A CADASTRAR" ou segunda grafia de apóstrofo e não entram nesta regra.

**Os 2 pendentes, e por quê:**

| Endereço | Aponta para | Motivo gravado em `integracao.MensagemDescartada` (fluxo `IBGE.GRAFIA_CORTADA`) |
|---|---|---|
| 54012 | `SANTANA DA PONTE PEN` | a coordenada do endereço cai em Santa Rita d'Oeste (3547403), e não em Santana da Ponte Pensa |
| 53535 | `SAO JOAO DAS DUAS PO` | a coordenada do endereço cai em Fernandópolis (3515509), e não em São João das Duas Pontes |

O texto do município e a coordenada discordam, e a carga não escolhe entre os dois. Alguém precisa
conferir o endereço desses dois clientes (P-15).

**O rastro de cada correção:**

- `auditoria.AlteracaoDeCampo`: 192 linhas em `Endereco.MunicipioId` (114 + 46 + 32), com o valor
  anterior — por exemplo `9490 (SAO JOSE DO RIO PRET)` — e o novo —
  `9491 (São José do Rio Preto, IBGE 3549805)`. Nas 192, o município atual do endereço é o do valor
  novo;
- a grafia cortada **continua no catálogo**, sem código: é o valor original;
- a chave de origem do Vórtice (`integracao.ChaveExterna`) foi reapontada, na versão 1.1, só na
  grafia que ficou sem endereço, São José do Rio Preto (1 linha, auditada). **A versão 1.2 não
  reaponta chave**: a correção se mantém pela regra do §4.6.1, e a chave continua dizendo o que a
  origem diz;
- 2ª rodada: 2 endereços lidos, 0 reapontados, 2 pendentes.

#### 4.6.1 A recarga do Vórtice mantém a correção [medido]

**O problema da versão 1.1.** Uma recarga do Vórtice devolvia os 78 endereços de Santana da Ponte
Pensa e São João das Duas Pontes para a grafia cortada, e só a carga do território, rodada depois,
os corrigia de novo. A correção dependia de alguém lembrar a ordem.

**A solução tem duas partes, e as duas preservam a origem:**

1. **Normalização persistente na própria carga do Vórtice.** Ao gravar um endereço que a origem
   aponta para uma grafia cortada, a carga mantém o município oficial **se a correção ainda se
   sustenta**: o endereço já está no município oficial reconhecido para aquela grafia, a UF é a
   mesma e a coordenada da origem é a mesma que está gravada, com diferença menor que uma unidade na
   7ª casa decimal (`Endereco.CorrecaoDeGrafiaCortadaSeMantem`). Se a origem mudou a UF ou a
   coordenada, a correção **não** é mantida e o endereço volta a ser conferido.
2. **Sequência automática.** Terminados os endereços, a carga do Vórtice roda a mesma conferência
   da carga do território (`ConsolidacaoDeGrafiasCortadas`, com a malha municipal do IBGE): corrige
   caso novo inequívoco e registra o pendente com o motivo. Sem a malha (IBGE fora do ar), endereço
   com coordenada não é corrigido, e o motivo diz isso.

Além disso, os municípios que o IBGE renomeou são **reencontrados pela chave de origem** (391 por
rodada) e não recriados — sem isso a recarga duplicaria linhas do catálogo. A grafia cortada continua
no catálogo, sem código, como valor original; a chave de origem não muda; e a trilha de auditoria só
ganha linha quando uma correção é feita **ou desfeita** — quando a origem muda a UF ou a coordenada, a
volta para a grafia cortada também é registrada.

**Premissa da regra.** O prefixo único é conferido contra os municípios oficiais que o catálogo já
tem. A carga do território cria os que faltam (113 na primeira rodada), então o catálogo reconhecido
tem os 5.571; num catálogo que nunca passou por ela não há código, e nenhuma grafia é corrigida.

**O ciclo completo, com leitura real do Vórtice (somente leitura), num banco isolado**
(`TracbelCrmValidacao`, cópia do banco de desenvolvimento no contêiner local):

| Etapa | O que rodou | Correções mantidas pela carga | Reapontados | Pendentes | Dos 192, no município oficial | Voltaram à grafia | Linhas na trilha | Catálogo |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| S0 | cópia do banco de desenvolvimento | — | — | 2 | 192 | 0 | 192 | 9.863 |
| S1 | carga do Vórtice, 1ª versão da regra | 72 | 6 | 2 | 192 | 0 | 198 | 9.863 |
| S2 | carga do Vórtice, regra corrigida | **78** | **0** | 2 | 192 | 0 | 198 | 9.863 |
| S3 | carga do território | — | **0** | 2 | 192 | 0 | 198 | 9.863 |
| S4 | carga do Vórtice de novo | **78** | **0** | 2 | 192 | 0 | 198 | 9.863 |

- **De S1 a S4, comparando o retrato (endereço, município) de cada etapa com o da anterior: nenhum
  endereço mudou de município.** Em toda etapa: 0 município com nome duplicado, 9.985 chaves de
  município, 391 municípios reencontrados pela chave.
- **A carga do território em S3 gravou zero** em tudo: catálogo, 238 linhas da área de atuação,
  203 + 406 afirmações de responsável e 45.582 linhas de área plantada, todas mantidas.
- **Os 2 endereços com coordenada conflitante são os mesmos em todas as etapas**, com o mesmo motivo
  na fila de revisão (54012 → coordenada em Santa Rita d'Oeste; 53535 → em Fernandópolis). Nenhum foi
  associado.
- Os endereços ativos passaram de 19.641 para 19.657 porque a leitura de 13/09 trouxe endereços novos
  do recorte (24.221 → 24.303 pessoas); é por isso que os três municípios oficiais foram de 224 para
  226 endereços.

**Os 6 de S1, e a correção da regra.** A primeira versão comparava a coordenada arredondando para 7
casas com o arredondamento bancário do .NET; o SQL Server grava arredondando para longe do zero. A
origem tem 11 casas, e em 6 endereços as duas regras discordaram na 7ª casa. A carga devolveu esses 6
à grafia, e a conferência automática da mesma rodada os corrigiu de novo: o estado final ficou certo,
mas a trilha ganhou 6 linhas repetidas. A regra passou a aceitar diferença menor que uma unidade na
7ª casa, com testes para os dois arredondamentos e para a diferença de uma unidade
(`CorrecaoDeGrafiaCortadaNaRecargaTestes`). A partir de S2 a trilha não cresceu. **No banco de
desenvolvimento a trilha continua com 192 linhas**; as 198 são só do banco de validação.

**Teste automatizado da conferência** (`ConsolidacaoDeGrafiasCortadasTestes`, banco em memória, 3
casos): corrige só o endereço sem coordenada e o de coordenada dentro do contorno, com trilha; deixa
pendentes, com o motivo, o de coordenada em outro município e o de outra UF; não lê o excluído; a
segunda rodada não grava nada; sem o IBGE, só o endereço sem coordenada é corrigido; e, sem endereço
com coordenada, o contorno nem é lido. A ligação dessa regra dentro da carga do Vórtice foi provada
na rodada real acima e ainda não tem teste automatizado (O-49).

---

## 5. Tabelas e dados que você precisa providenciar

Para cada grupo: se existe, o que falta, e o que precisa chegar. **Campos em itálico são
[proposta]**; os demais existem no modelo ou na fonte citada.

| # | Grupo | Finalidade | Existe no projeto? | Campos mínimos | Chaves e relações | Fonte esperada | O que você precisa fornecer | Prioridade | Indicadores que dependem |
|---|---|---|---|---|---|---|---|---|---|
| 1 | Municípios e ADR | território oficial | **Sim** — `Municipio` (+ código IBGE nesta rodada) e `MunicipioDaAreaDeAtuacao` | código IBGE, nome, UF, pertence à ADR, região, loja | município ← IBGE; loja → `Empresa` | IBGE + planilha C | confirmar a lista de 203 e a data; dizer o que são os 35 (C-5); confirmar Norte/Noroeste | **Alta** | os três mapas |
| 2 | CEN, gestores, usuários, perfis, hierarquia | quem vê o quê | Parcial — `Usuario` (276), `HierarquiaComercial` vazia, conjuntos de permissão vazios | login corporativo (UPN do Entra), nome, cargo (*CEN/Gerente/Diretor*), gestor imediato, filial de casa, ativo | usuário → gestor (hierarquia) | RH/AD + comercial | **lista oficial**: e-mail corporativo, cargo, gestor, loja, data de início | **Alta** | visões por perfil (R-15), responsáveis (C-6) |
| 3 | Município × CEN × gestor, com vigência | quem atende cada cidade | **Sim** — `ResponsavelPeloMunicipio` (afirmação por fonte) | município, papel, pessoa, *vigente desde*, *vigente até* | município; usuário | comercial | qual planilha vale (C-1…C-3) e a vigência; ou uma planilha nova única | **Alta** | detalhe do município, filtro por CEN/gestor |
| 4 | Clientes e classificação SAM/KAM/Varejo | segmentar | Cliente sim; **classificação não** (§3.4) | cliente, *tipo de cliente* (catálogo), *vigente desde*, *critério* | cliente → catálogo | comercial | **definição** de cada tipo e a **lista** de clientes classificados | **Alta** | filtro R-13 |
| 5 | Propriedades rurais (clientes e não clientes) | onde está a área | Parcial — `Endereco` tem `Hectares`, mas vazio; não cliente não existe | *propriedade*, município, área total, área agricultável, proprietário (CPF/CNPJ), coordenada | propriedade → município; → cliente quando houver | **ART**, CAR/SICAR (público) | acesso ao ART a partir desta rede **ou** exportação da base de propriedades | **Alta** | potencial de cliente e de não cliente |
| 6 | Culturas e áreas por propriedade | potencial por cultura | Regional sim (PAM, nesta rodada); **por propriedade não** | propriedade, cultura (catálogo `CULTURA`), área, safra/ano | propriedade; cultura | ART | idem grupo 5, com a safra | **Alta** | mapa C (clientes) |
| 7 | Tipos de produto, fabricantes, modelos | filtrar e aplicar regra | Parcial — `Marca` 13, `Familia` 22, `Modelo` 253, sem linha/série/porte | fabricante, *linha*, família, *série*, modelo, *categoria (colhedora, trator grande/médio/compacto)* | modelo → família → marca | Protheus (VV2/SB1) ou tabela John Deere | a **taxonomia** usada pelo comercial, com o critério de porte (cv?) | Média | filtros R-14, regra de potencial |
| 8 | Equipamentos e parque instalado | base instalada | Parcial — 3.888, sem ano/horímetro/local | chassi, modelo, cliente, propriedade, ano, entrega técnica, horímetro + data + origem | → modelo, → cliente | ART (Operation Center) | idem grupo 5 | Média | ciclo de troca, potencial ajustado |
| 9 | Visitas realizadas, planejadas, periodicidade | cobertura | Parcial — interações e tarefas 2026; cadência em 4 linhas | tipo de contato que **conta como visita**, data, cliente, carteira; periodicidade por classe | → cliente, → carteira | CRM | **quais tipos contam como visita**; periodicidade (30/60/90/120 ou a declarada?); quem é elegível | **Alta** | mapa A |
| 10 | Vendas e itens | desempenho | Parcial — mês × cliente, sem item | nota, item, produto/modelo, quantidade, valor, data, CFOP, situação | → cliente, → modelo | Protheus SD2/SF2/SD1 | decisão sobre **devolução** (SD1) e **cancelamento**; autorização para trazer o item | **Alta** | mapa B por modelo, R-14 |
| 11 | Pós-venda | margem recorrente | Parcial — peça e serviço pelo grupo do item | natureza (peça balcão, oficina, serviço, AMS, PUK, garantia), valor, data, cliente | → cliente | Protheus (SD2 + OS) | **o que compõe pós-venda** para a diretoria | Média | cartão de pós-venda |
| 12 | Regras de potencial | teórico por área | **Sim** — `RegraDePotencial`, 1 regra a confirmar | cultura, modelo, hectares por máquina, *horizonte*, *arredondamento*, *preço*, vigência | → produto IBGE; → *modelo* | comercial | confirmar a regra do café e informar as demais culturas (cana ocupa 2,77 milhões de ha da ADR) | **Alta** | mapa C |
| 13 | Ciclo de troca | potencial ajustado | **Não** | categoria, anos ou horas de vida útil, fonte | → categoria de produto | comercial / John Deere | a regra (a maquete cita 7–10 anos, sem fonte) | Média | potencial ajustado |
| 14 | Concorrentes e presença | share | Parcial — catálogos `CONCORRENTE` (23) e `REVENDA_CONCORRENTE` (32); máquina de concorrente cabe em `Equipamento` | marca, modelo, cliente/propriedade, ano, fonte da informação | → marca, → cliente | CRM (declarado pelo CEN), emplacamento | a fonte de emplacamento ou de parque concorrente | Baixa | share, potencial ajustado |
| 15 | Importações e inconsistências | rastro | **Sim** — `ChaveExterna`, `MensagemDescartada`, `PontoDeSincronismo`, `AlteracaoDeCampo` | — | — | — | nada | — | todos |

---

## 6. Mapeamento das planilhas para o sistema

"Obrigatoriedade" registra **o que a fonte mostra**; a planilha de exemplo não declara campo
obrigatório, e nenhuma obrigatoriedade nova foi criada.

### 6.1 `Tabelas Exemplo.xlsx`, aba Cliente

| Coluna | Tipo e formato observados | Campo no sistema | Situação | Validação/transformação | Observação |
|---|---|---|---|---|---|
| ID Cliente | inteiro (3000123) | `integracao.ChaveExterna` | existe | chave de origem, nunca PK | origem do ID não informada — Protheus? ART? **[pendente]** |
| Nome | texto | `Cliente.NomeRazao` | existe | aparar espaços | — |
| Nome Fantasia | texto | `Cliente.NomeFantasia` | existe | — | — |
| CNPJ | texto com máscara | `Cliente.Documento` + `TipoDePessoa` | existe | só dígitos, dígito verificador | — |
| CPF | texto com máscara ou "-" | idem | existe | "-" = ausente | **a linha 2 tem CNPJ e CPF ao mesmo tempo**; o modelo guarda um documento por cliente **[pendente]** |
| Insc Estadual | número ou "-" | `Cliente.InscricaoEstadual` | existe | "-" = ausente | — |
| Insc Municipal | número ou "-" | — | **não existe** | — | *campo novo* **[proposta]** |
| Filial Atendimento | nome da cidade | `Cliente.EmpresaId` | existe | nome → código da filial | "atendimento" é a filial dona do cadastro? **[pendente]** |
| Carteira Máquinas / Peças / Serviços / PUK | nome de pessoa | `ClienteCarteira` na linha de negócio (MAQ_NOVOS, MAQ_PECAS, MAQ_SERV, DSI_PUK) | existe | nome → usuário responsável | a carteira é da linha, não do cliente |
| Produtor Rural | Sim/Não | — | **não existe** | booleano | *campo novo* **[proposta]** |
| Cidade, UF | texto ("Sp" em minúscula numa linha) | `Endereco.MunicipioId` | existe | seleção do catálogo IBGE | nunca texto livre (documento 26) |
| CEP | inteiro de 8 dígitos | `Endereco.Cep` | existe | texto de 8 dígitos | como número, o zero à esquerda se perde |
| Endereço, Bairro | texto | `Endereco.Logradouro`, `Bairro` | existe | — | exceção de texto livre justificada (documento 26 §8.3) |
| Telefone Principal | inteiro de 10–11 dígitos | `comercial.CanalContato` | existe, vazio | E.164 | — |
| Data de Cadastro | data | `Cliente.CriadoEm` | existe | a data da origem, não a da carga | — |
| Cadastrado Por | login | `Cliente.CriadoPorId` | existe | login → usuário | — |
| Extensão Area | número | — | **não existe** | ≥ 0 | unidade não declarada; os valores sugerem hectare **[pendente]** — *área total da propriedade* |
| Atividade  Principal | texto ("Usina de Cana", "Plantação de Café") | — | **não existe** | catálogo | cabeçalho com espaço duplo na fonte — *catálogo de atividade* **[proposta]** |
| Area Agriculturável | número | — | **não existe** | ≤ área total | *campo novo* **[proposta]** |
| Cultivo Principal / Secundário | texto ou "-" | catálogo `CULTURA` (3 itens) | parcial | seleção | "Cana de açucar" sem acento na fonte |
| Area Cultivo Principal / Secundário | número ou "-" | `Endereco.Hectares` (1 cultura) | parcial | ≥ 0; soma ≤ agricultável | *cultivo por propriedade, N culturas* **[proposta]** — a planilha tem 2 posições fixas |

Coerência observada nas 3 linhas: principal + secundário = agricultável (100 = 90 + 10) e
agricultável ≤ extensão (1.400 ≤ 1.500). **É uma regra candidata, não confirmada.**

### 6.2 `Tabelas Exemplo.xlsx`, aba Equipamento

| Coluna | Tipo e formato | Campo no sistema | Situação | Observação |
|---|---|---|---|---|
| ID Equipamento | inteiro | `ChaveExterna` | existe | origem do ID **[pendente]** |
| Fabricante | texto | `frota.Marca` | existe | — |
| Linha Produtos | "TRATORES COMPACTOS", "TRATORES MÉDIOS" | — | **não existe** | *nível de linha* **[proposta]** — é o "tipo de produto" do filtro R-14 |
| Familia | "TRATORES DA FAMÍLIA 5" | `frota.Familia` | existe com outro sentido | a família de hoje é "Trator"; a da planilha é a família John Deere |
| Serie do Produto | "SÉRIE 5E" | — | **não existe** | *nível de série* **[proposta]** |
| Modelo | "5078E" | `frota.Modelo` | existe | catálogo precisa de saneamento (§3.1) |
| Chassi | texto de 17 | `Equipamento.Chassi` | existe | identidade da máquina |
| Motor | inteiro (78, 85, 82) | `Modelo.PotenciaCv`? | existe | os valores coincidem com a potência do modelo (5078E → 78) — **[pendente]** confirmar |
| Peso | inteiro | — | **não existe** | unidade não declarada **[pendente]** |
| Ano Fabricação | texto ("2019") | `Equipamento.AnoFabricacao` | existe | texto → inteiro |
| Data E.T. | data | — | **não existe** | *entrega técnica* **[proposta]** |
| Cliente Atual | nome | `Equipamento.ClienteId` | existe | casar por documento, **nunca por nome** |
| Filial Cliente | "201573" | — | — | não é código de filial do CRM (0101NN); o mesmo cliente aparece com dois códigos **[pendente]** |
| Nome Filial | cidade em maiúscula | `Equipamento.EmpresaId` | existe | — |
| Horimetro atual | texto ("5737") | `Equipamento.HorimetroAtual` + `LeituraDeHorimetro` | existe | texto → decimal |
| Data do Registro Horimetro | data | `HorimetroAtualizadoEm` / `LeituraDeHorimetro.LidaEm` | existe | — |
| Origem da atualização Horimetro | "Operation Center", "Manualmente", "Simova" | `LeituraDeHorimetro.Fonte` | existe, **texto** | deve virar catálogo **[proposta]** |
| Registrado POPs / Data Ultimo Registro POPs | Sim/Não, data | — | **não existe** | significado de "POPs" **[pendente]** |

### 6.3 Por que o cadastro de cliente e equipamento não mudou nesta rodada

Todas as colunas novas pedidas pela planilha (área, culturas, produtor rural, linha, série, entrega
técnica, POPs) têm **a mesma fonte, o ART**, que não responde desta rede — e várias têm semântica
não confirmada (unidade da área, "POPs", "Filial Cliente", CPF e CNPJ no mesmo cliente). Criar as
colunas agora produziria campos vazios com significado adivinhado. O mapeamento acima é o contrato
para a rodada em que a fonte chegar; o que ele exige de decisão está no §10.

### 6.4 `Area de Atuação.xlsx`

| Coluna | Uso | Destino |
|---|---|---|
| `%ChvMunicipio` | conferência do nome | não gravada (redundante com o código IBGE) |
| `CEN` | afirmação de responsável | `ResponsavelPeloMunicipio` (papel CEN, fonte planilha C), nome preservado |
| `CONCAT` | conferência | não gravada |
| `Filial` | código de 3 letras da loja | convertido para `Empresa` pela coluna `Loja (Responsável)` |
| `FlgADR` | "Área ADR" ou "-" | `MunicipioDaAreaDeAtuacao.PertenceAAdr` |
| `Latitude`, `Longitude` | conferência contra o contorno IBGE | não gravadas — o mapa usa a malha oficial |
| `Loja (Responsável)` | nome da loja | `MunicipioDaAreaDeAtuacao.EmpresaResponsavelId` |
| `Município` | identificação | nome → código IBGE (SP) |
| `Pais`, `UF` | conferência | não gravadas (todas Brasil/SP) |
| `Região` | Norte/Noroeste | `MunicipioDaAreaDeAtuacao.Regiao` |

### 6.5 `CEN e Gestor por Municipio.xlsx`

| Coluna | Uso | Destino |
|---|---|---|
| `%ChaveTerritorio` | **código IBGE sem "35"** | identificação do município; guardado em `ChaveNaOrigem` |
| `Gerente_Territorio` | afirmação de gestor | `ResponsavelPeloMunicipio` (papel Gestor, fonte planilha D) |
| `IdtLocalizacaoTerritorio` | conferência | não gravada |
| `Loja_Territorio` | conferência contra a planilha C | 0 divergências |
| `Municipio_Territorio` | conferência do código contra o nome | linha recusada se não bater |
| `Vendedor_Territorio` | afirmação de CEN | `ResponsavelPeloMunicipio` (papel CEN, fonte planilha D) |

As 90 linhas sem município vão para `integracao.MensagemDescartada` com o motivo.

---

## 7. O modelo: quatro tabelas novas, e as sete perguntas do documento 17

A conta passa de **68 para 72 tabelas**, todas em `organizacao` (8 → 12). O teto do documento 17 é
100.

| Pergunta | `MunicipioDaAreaDeAtuacao` | `ResponsavelPeloMunicipio` | `AreaPlantadaNoMunicipio` | `RegraDePotencial` |
|---|---|---|---|---|
| 1. Conceito, na palavra do comercial | "município da ADR" | "CEN e gestor por município" | "área plantada por cultura" | "potencial: 1 trator a cada N ha" |
| 2. Qual tabela já guarda isto? | `Municipio` é nacional; `CarteiraMunicipio` é a carteira do Vórtice, não a ADR | `Carteira.ResponsavelId` é a carteira, não o município; não guarda fonte nem divergência | `Endereco.Hectares` é por endereço de cliente e está vazio | `Praca.PotencialEstimado` é valor pronto por praça, sem regra |
| 3. Critério da regra 8 | ciclo de vida próprio (entra/sai por revisão da planilha) | cardinalidade > 1 (duas fontes, dois papéis por município) | volume e ciclo próprios (um ano por leitura) | precisa ser apontada e confirmada |
| 4. Derivável? | não | não | não | não |
| 5. Dono do dado | comercial (planilha) | comercial (planilha) | IBGE | comercial |
| 6. Retenção | encerrar, nunca apagar | encerrar, nunca apagar | por ano; a releitura substitui o ano | desativar, nunca apagar |
| 7. Catálogo de cada texto | nenhum texto livre (`ArquivoDeOrigem` é rastro da carga) | `NomeNaOrigem` é rastro da carga, exceção justificada: é a prova da decisão | `ProdutoNome` é o rótulo oficial do código IBGE | `ModeloDeReferencia` é texto porque o catálogo não tem o 3036N; vira FK quando tiver |

**A filial responsável da ADR não usa a coluna `EmpresaId`**, porque essa coluna é a fronteira de
multiempresa e filtraria a ADR por filial — o que esconderia o mapa de quem precisa dele. É
atributo do território, e se chama `EmpresaResponsavelId`.

**Quem importou é chave estrangeira.** `ImportadoPorId` aponta para `seguranca.Usuario`, com índice,
nas três tabelas que a carga grava (`MunicipioDaAreaDeAtuacao`, `ResponsavelPeloMunicipio`,
`AreaPlantadaNoMunicipio`), e muda para quem fez a última conferência. `RegraDePotencial` não tem a
coluna: nasce da migração, com a origem escrita.

---

## 8. Indicadores — definição, fórmula e limites

### 8.1 Mapa A — pendência de visita

| | |
|---|---|
| Unidade | **vínculo** cliente × carteira comercial (a mesma do documento 27) |
| Elegível | vínculo vigente em carteira de natureza Comercial, cuja linha de negócio **declara cadência** |
| Coberto | última interação dentro da cadência da classe ABC do cliente naquela linha |
| Pendente | fora da cadência **+** nunca contatado |
| Fórmula | `% pendente = pendentes ÷ elegíveis` |
| Fora da conta | vínculo em linha sem cadência declarada — contado à parte, nunca como pendente |
| Município | do endereço principal do cliente, pelo código IBGE |
| Referência | a data da consulta; a tela mostra também a data da interação mais recente carregada |
| Cor | vermelho mais intenso = maior % pendente; a tela alterna para **quantidade** de pendentes |
| Sem dado | município da ADR sem vínculo elegível: hachurado, nunca zero |
| Limite | "interação" é qualquer contato registrado, não só visita; a definição de visita está pendente (§10) |

### 8.2 Mapa B — vendas

| | |
|---|---|
| Base | `FaturamentoDoCliente` (SD2 do Protheus, CFOP de venda, nota normal — documento 30) |
| Valor | líquido, em R$, com a quebra máquina / peça / serviço / outros |
| Período | competências inclusivas; padrão = **12 meses fechados** até o mês anterior ao corrente |
| Município | do endereço principal do cliente |
| Devolução e cancelamento | **não** abatidos (a carga lê notas de saída); declarado na tela **[pendente]** |
| Fora do mapa | cliente sem município, com município sem código IBGE, de outra UF, de outra filial, inativado, e **nota sem cliente no CRM** por natureza da contraparte — somados à parte, nunca a uma cidade |
| Conferência | municípios + grupos fora do mapa = tudo o que a filial (ou a empresa) emitiu no período (§8.5.4) |
| Limite | valor absoluto favorece cidade grande (aviso da própria maquete); não há quantidade nem modelo |
| Filial | filial que vendeu × filial de cadastro do cliente, e as visões da filial e da empresa (§8.5) |

### 8.3 Mapa C — potencial teórico por área

| | |
|---|---|
| Base | área plantada da PAM/IBGE — tabela SIDRA **5457**, variável **8331** ("Área plantada ou destinada à colheita"), último ano publicado. Desde a issue 64 a carga traz junto a **216** (área colhida), a **214** (quantidade produzida) e a **215** (valor da produção, em MIL reais) — o mapa C usa a 8331; as outras três servem à comparação com a planilha do comercial e ao painel de potencial (documento 48) |
| Regra | cada `RegraDePotencial` ativa: `máquinas teóricas = área do produto ÷ hectares por máquina` |
| Hoje | 1 regra: café (em grão) total, 3036N, 10 ha — **a confirmar** |
| O que é | necessidade teórica de frota da **região inteira**, não venda anual e não valor em reais |
| O que não é | potencial de cliente, de não cliente, ajustado por ciclo de troca, parque ou concorrência |
| Zero × sem dado | IBGE "-" = 0; "..." e "X" = não disponível (hachurado) |
| Limite | uma cultura só; cana (a maior área da ADR) sem regra |

**A variável, e por que ela é citada aqui (D-12, corrigido em 19/09/2026).** Até a issue 83 o leitor
pedia a variável **216**, que é a **área colhida**, e gravava o resultado como área plantada. As duas
só coincidem em lavoura temporária bem-sucedida: em cultura perene a colhida fica abaixo da plantada
enquanto o cafezal ou o pomar novo não produz, e uma frustração de safra derruba a colhida sem mudar o
que foi plantado. O efeito é um mapa C **subestimado justamente onde há plantio novo**. O código já pede
a 8331, com teste que prende a variável (`LeitorDoIbgeTestes`).

**Corrigido no servidor em 20/09/2026 [medido].** Duas coisas que este documento afirmava e não eram
verdade, descobertas ao publicar:

1. **O servidor não tinha as 45.582 linhas.** Ele estava com **zero** — a sanitização de 15/09/2026
   (documento 38) levou junto a área plantada, a ADR e os responsáveis, e nem aquele documento nem
   este registraram a perda. O que havia ali de 14/09 já não existia.
2. **A leitura de todos os produtos de uma vez parou de funcionar.** O SIDRA passou a recusar a
   consulta com `400 Bad Request` (são ~55 mil valores); com 10 produtos ela responde. Não é efeito
   da troca de variável — a URL antiga, com a 216, falha igual. O leitor passou a **pedir em lotes de
   20 produtos**, com a lista vinda dos metadados da tabela (issue 95).

Depois disso, a carga rodou contra o banco do servidor e gravou **54.570 linhas de área plantada do
ano 2025**, com 9.967 "não disponível" preservados como nulo e 37.455 zeros. A soma dá **9.405.743
ha**, que é a soma do SIDRA (9.210.103) **mais o "Café (em grão) Total"** — a contagem tripla do café
(Total, Arábica e Canephora) continua no dado bruto, e quem somar precisa escolher um dos três (é o
que a #64 trata). A ADR voltou aos 203 municípios e os responsáveis, a 609.

#### 8.3.1 As quatro medidas e o total do estado (issue 64, 20/09/2026)

A tabela deixou de guardar uma coluna e passou a guardar as quatro medidas que a PAM publica, na
mesma linha de (município, ano, produto): **8331** área plantada, **216** área colhida, **214**
quantidade produzida e **215** valor da produção em mil reais. A tabela mudou de nome junto —
`AreaPlantadaNoMunicipio` virou `organizacao.ProducaoAgricolaNoMunicipio`, por migração que
**renomeia**, sem recriar, para não apagar as linhas já carregadas.

A **112 (rendimento médio) fica de fora de propósito:** ela é quantidade ÷ área colhida. Guardar um
número derivado ao lado das duas parcelas cria uma terceira fonte para a mesma verdade — a que diverge
primeiro, e sempre em silêncio.

Ao lado dela nasceu `organizacao.ProducaoAgricolaNoEstado`, com a linha que o IBGE publica para a UF
inteira. **Ela não é a soma dos municípios**, e a diferença é medível: em 2024, o valor da produção de
São Paulo publicado pelo estado é R$ 118.021.046 mil, e a soma dos 645 municípios dá R$ 118.021.202
mil — R$ 156 mil de diferença, que é o valor municipal sigiloso entrando no total do estado sem
aparecer embaixo. Sem a linha do estado não há denominador honesto para "que fatia da cultura de SP
está na área de atuação".

**O lote caiu de 20 para 10 produtos.** O teto do SIDRA é de tamanho de resposta, não de número de
produtos: com uma variável, 20 produtos cabiam; com quatro, não. Medido em 20/09/2026, para os 645
municípios de SP: 4 variáveis × 10 produtos devolveu 25.681 linhas e 5,3 MB, com HTTP 200; 4 × 20
devolveu **400**. Dez deixa metade do teto de folga.

#### 8.3.2 O ano das colunas da planilha do comercial está adiantado [medido em 20/09/2026]

Ao conferir os totais do protótipo contra o SIDRA, três dos quatro números bateram ao dígito — mas
**não no ano que a planilha diz**:

| O que o documento 48, §3.8, chama de… | é, na verdade, a PAM de… | ADR (203) | São Paulo |
|---|---|---:|---:|
| Área plantada **2024** | **2023** | 3.715.896 ha ✔ | 9.217.695 ha (o doc traz 9.216.**795**) |
| Área plantada **2025** (preliminar) | **2024** | 3.706.356 ha ✔ | 9.155.949 ha ✔ |
| Valor da produção **2024** | **2024** ✔ | R$ 50.465.781 mil ✔ | R$ 118.021.202 mil ✔ |

Ou seja: **as colunas de área estão rotuladas com um ano a mais; a de valor está certa.** E o total de
São Paulo do documento tem uma transposição de dígitos (9.216.795 no lugar de 9.217.695, 900 ha).

Isso não é defeito da carga — é o rótulo da fonte. Mas muda o que a decisão **D-P10** (ano de
referência, issue 63) está escolhendo, e por isso a carga passou a trazer **três anos** da PAM: quem
duvidar confere no banco, sem consulta avulsa.

#### 8.3.3 A rotina anual, no servidor

A PAM é a **única** etapa do território que o servidor consegue rodar sozinho — que é o que a regra
R-5 do documento 46 pede. As outras três dependem das duas planilhas do comercial, que trazem nome de
funcionário por município; levá-las até o servidor só para atualizar o IBGE seria levar dado pessoal
onde ele não precisa estar. Por isso a carga ganhou o modo **`--somente-pam`**, que não usa planilha
nenhuma e conta com o catálogo de municípios já reconhecido.

| | |
|---|---|
| Instalação | `scripts/deploy/agendar-fontes-publicas-no-servidor.ps1` — publica a carga em `C:\aplicacoes\tracbel-crm-carga`, grava a conexão **integrada** (nenhuma senha em arquivo) e registra a tarefa |
| Tarefa | `TracbelCrmFontesPublicas`, **1º de outubro às 03:00**, todo ano, como SYSTEM — escrita como `/SC MONTHLY /M OCT /D 1`, porque **não existe `/SC YEARLY`** no `schtasks`. Ela roda `--somente-pam` e `--somente-estrutura` em sequência, e o código de saída é o pior dos dois: uma indisponibilidade do SIDRA não pode levar junto a leitura da ANP |
| Registro | `integracao.PontoDeSincronismo`, fluxo `IBGE.PRODUCAO_AGRICOLA`, com lidos, gravados e recusados; e um arquivo de log por rodada no servidor, guardado por três anos |
| Trava | `sp_getapplock` no próprio banco, tomada **antes** da leitura do SIDRA |

**Por que a trava é do banco, e não um arquivo.** A rotina roda no servidor e a carga manual roda na
estação — duas máquinas, um banco só; um arquivo numa delas não enxerga a outra. E ela **recusa em
vez de enfileirar**: medido em 20/09/2026, com a trava tomada por outra sessão, a carga parou em
**4 segundos**, sem ler o IBGE e sem gravar nada. Enfileirar faria a segunda rodada esperar quatro
minutos para depois refazer o que a primeira acabou de fazer.

### 8.4 O quarto mapa — a estrutura agropecuária (issue 103)

Os mapas A, B e C respondem sobre a **operação da Tracbel** (cobertura, vendas) e sobre a **lavoura**.
Nenhum deles dizia o que já existe instalado no território. O mapa D preenche isso, com alternador
entre cinco recortes:

| Recorte | O que pinta | Fonte e ano |
|---|---|---|
| Tratores | o parque existente | Censo Agropecuário, **2017** |
| Tratores / mil km² | a **densidade** — sem ela, o mapa de tratores é quase um mapa de tamanho do município | Censo ÷ área territorial |
| Propriedades | estabelecimentos agropecuários | Censo Agropecuário, 2017 |
| Rebanho bovino | cabeças | Pesquisa da Pecuária Municipal, **anual** |
| Usinas de etanol | capacidade autorizada, em m³/dia | ANP, mensal |

**O mapa C ganhou alternador junto:** máquinas teóricas (a cultura da regra), área plantada e valor
da produção (a **lavoura inteira**, todas as culturas). São bases diferentes, e o rodapé do mapa diz
isso ao trocar.

**Três avisos que o dado obriga, e que a tela dá:**

1. o Censo é de 2017 e o próximo sai em **2028** — o parque tem essa idade, e o rebanho ao lado dele
   é de outro ano;
2. **hachurado é sigilo do IBGE, não zero.** Em São Paulo, 79 linhas de tratores vêm ocultas porque
   poucos estabelecimentos as compõem. E as faixas de potência **não somam o total**: o "Total" do
   IBGE é uma categoria ao lado delas, e nem sempre é a soma — quando o sigilo esconde uma parte, o
   total continua publicado;
3. a ANP só enxerga usina de **etanol**: município sem usina na lista fica hachurado, porque a
   ausência ali não prova ausência de usina.

**O painel "O mercado da região"** fica entre os indicadores da operação e os mapas, e compara a
região com São Paulo. O denominador é o **total publicado** pelo IBGE (`ProducaoAgricolaNoEstado`), e
não a soma dos municípios — o valor municipal sigiloso entra nele sem aparecer embaixo.

**A quantidade produzida não tem total, de propósito.** O IBGE publica cada produto na unidade dele
— tonelada para grãos, **mil frutos** para laranja, **mil cachos** para banana. Somar isso daria um
número sem unidade; ela aparece por produto, nunca agregada.

**O tamanho do erro [medido em 19/09/2026]** — lido ao vivo do SIDRA (tabela 5457, PAM **2025**, que é
o último ano publicado hoje; em 17/09/2026 a API recusou esta estação, e em 19/09 respondeu):

| Recorte | Área plantada | Área colhida | Diferença |
|---|---:|---:|---:|
| São Paulo, 58 produtos (fora os agregados "Total") | 9.210.103 ha | 9.188.169 ha | 21.934 ha · **0,24%** |
| Cana-de-açúcar, SP | 5.430.681 ha | 5.415.896 ha | 0,27% |
| Café (em grão) total, SP | 195.640 ha | 193.599 ha | **1,04%** |
| Laranja, SP | 348.223 ha | 347.958 ha | 0,08% |

**No estado a diferença é pequena; no município, que é o recorte do mapa, não é.** Dos 470 municípios
paulistas com cana, **8 perdem 5% ou mais** de área ao usar a colhida — Clementina cai de 10.500 para
6.300 ha (40%) e Braúna de 8.500 para 5.100 ha (40%). No café, 4 dos 145 municípios: São Sebastião da
Grama cai de 5.200 para 4.200 ha (19,2%) e Arandu de 1.000 para 800 ha (20%). Laranja: Araras, de 700
para 450 ha (35,7%). Borracha: Junqueirópolis, de 808 para 570 ha (29,5%). É exatamente nessas cidades
que o mapa C mostrava menos máquinas teóricas do que a área comporta. A recarga também avança o ano da
PAM (o servidor carregou quando o último ano publicado era outro).

**Para que serve a área plantada do IBGE, e para que não serve.** A PAM é a área plantada **por
município**, estimada pelo IBGE. Ela serve para uma estimativa **regional**: quanto de café existe
numa cidade e, com uma regra comercial confirmada, quantas máquinas essa área comportaria. Ela **não
comprova a área de nenhuma propriedade**, não diz quem é cliente, não separa cliente de não cliente e
**não substitui as regras comerciais de potencial**, que dependem de propriedade, parque instalado,
ciclo de troca e concorrência (§5, grupos 5, 6, 8, 13 e 14). A tela diz isso no subtítulo e no aviso
do mapa C, e a API na métrica `potencialDosNaoClientes`.

### 8.4 Pós-venda

`pós-venda = peça + serviço`, pelo grupo do item na nota (documento 30). **Máquina e "outros"
(repasse de fábrica e grupos não classificados) ficam fora**, e as quatro parcelas somam o valor
líquido — não há dupla contagem por construção. A composição exata que a diretoria considera
pós-venda está pendente (§5, grupo 11).

### 8.5 Vendas por filial e da empresa

#### 8.5.1 As duas filiais de uma venda

| Conceito | De onde vem | Filtro na tela e na API |
|---|---|---|
| **Filial que vendeu** | a filial que emitiu a nota (`FaturamentoDoCliente.EmpresaId`) | "Filial que vendeu" · `filialDaVenda` — mexe só nas vendas |
| **Filial de cadastro do cliente** | a filial dona do cadastro (`Cliente.EmpresaId`), que é também a do endereço | "Filial de cadastro do cliente" · `filialDoCliente` — mexe na cobertura e nas vendas |

Os dois filtros são independentes e combináveis. A nota sem cliente no CRM não tem filial de
cadastro: o filtro `filialDoCliente` a exclui.

#### 8.5.2 As duas visões, e quem vê cada uma

| | Visão da filial (padrão) | Visão da empresa |
|---|---|---|
| Alcance | a filial do cabeçalho e as que o contexto de acesso alcança — hoje só ela, porque as 18 empresas não têm hierarquia | todas as filiais, inclusive as inativas |
| Vendas | as notas emitidas pela filial | as notas de todas as filiais |
| Município da venda | o do endereço do cliente, **se o cadastro do cliente estiver ao alcance**; senão, o grupo `ClienteDeOutraFilial` | sempre o do endereço do cliente; o grupo `ClienteDeOutraFilial` não existe |
| Permissão | a do login (ou da ponte provisória) | `Empresa.AlcanceEntreFiliais` em profundidade Organização, por concessão explícita de conjunto de permissão ativo e não vencido; a abertura do alcance vai para o diário |
| Quem tem hoje | todo usuário autenticado — e **em qualquer filial ativa**: a filial do cabeçalho não é conferida contra o usuário (D-11, P-20) | **nenhum usuário real** — só os perfis de teste do banco isolado (§8.5.5); para os demais a API responde **403** |
| Filtro `filialDoCliente` fora do alcance | **403** | — |
| Cabeçalho com outra filial ativa | **200** — aceito sem conferir o usuário (D-11) | — |

A permissão é conferida duas vezes: no caso de uso (403) e no `CrmDbContext`, que se recusa a abrir o
alcance sem ela (teste `Sem_a_permissao_a_camada_de_dados_tambem_recusa_a_visao_da_empresa`). A tela
desliga a opção "Empresa inteira" e diz por quê.

**Nenhuma permissão foi concedida a usuário real nem alterada no servidor.** A visão da empresa está
provada com o repositório e o filtro global de verdade num banco de teste (§11.1) e, com dado real,
na aplicação, com dois perfis de teste num banco isolado (§8.5.5).

#### 8.5.3 Os R$ 321,6 milhões de "cliente de outra filial" [medido]

Doze meses fechados (set/2025–ago/2026), 13 filiais ativas. Na visão da filial, a nota que a filial
**A** emitiu para um cliente **cadastrado na filial B** não pode ir para o município do cliente: o
cadastro e o endereço dele são da filial B e estão fora do alcance de A. Ela é somada no grupo
`ClienteDeOutraFilial`. **Não é venda perdida nem duvidosa**: é a mesma venda que, na visão da
empresa, vai para o município do cliente.

| | Linhas (cliente × filial × mês) | Valor |
|---|---:|---:|
| Nota para cliente cadastrado na própria filial | 6.385 | R$ 237.887.780,14 |
| Nota para cliente cadastrado em outra filial | 6.914 | **R$ 321.609.399,93** |
| Nota para cliente inativado | 0 | R$ 0,00 |
| **Total com cliente, 13 filiais ativas** | **13.299** | **R$ 559.497.180,07** |

- **1.706 clientes** receberam nota de filial diferente da do cadastro; **910 clientes** compraram de
  mais de uma filial no período. Nenhum deles está cadastrado em filial inativa.
- Maiores pares (filial que vendeu → filial do cadastro): 010113 → 010114 R$ 25,7 mi;
  010109 → 010101 R$ 22,1 mi; 010109 → 010113 R$ 21,5 mi; 010107 → 010101 R$ 17,4 mi;
  010114 → 010113 R$ 17,4 mi.
- Por filial de cadastro do cliente: 010113 R$ 94,0 mi; 010101 R$ 90,6 mi; 010114 R$ 36,8 mi;
  010102 R$ 26,1 mi; 010103 R$ 21,2 mi; as demais R$ 53,0 mi.

O valor alto é um fato da operação, e não da tela: filiais emitem nota para cliente que o CRM
cadastrou em outra loja. Se o cadastro deve seguir a filial que mais vende para o cliente é pergunta
de negócio (P-16).

#### 8.5.4 Sem omissão e sem dupla contagem [medido]

**Por construção.** Cada linha de faturamento tem **uma** filial emissora, e por isso entra em
**uma** visão de filial. Dentro da visão, cada cliente cai em **um** grupo — um município ou um
grupo fora do mapa — ou em nenhum, quando o filtro o exclui; a nota sem cliente cai em **um** grupo
pela natureza. Não existe caminho que some a mesma linha duas vezes. No banco, o grão
`(cliente, filial, mês)` não tem duplicata (0) e, em toda linha do período,
máquina + peça + serviço + outros = líquido (0 exceções).

**Por conta, filial a filial** — API na visão de cada filial × consulta SQL escrita à parte. Em todas
as 13 filiais, as cinco colunas da API são **iguais** às do SQL, ao centavo:

| Filial que vendeu | Com cliente | Cliente da própria filial | Cliente de outra filial | Nota sem cliente no CRM |
|---|---:|---:|---:|---:|
| 010101 | 126.726.787,62 | 86.896.431,61 | 39.830.356,01 | 90.379.488,14 |
| 010102 | 18.223.679,29 | 4.745.339,36 | 13.478.339,93 | 5.025.761,16 |
| 010103 | 22.352.960,78 | 4.872.390,83 | 17.480.569,95 | 7.700.497,32 |
| 010107 | 38.633.786,41 | 8.374.875,61 | 30.258.910,80 | 9.242.736,78 |
| 010109 | 84.043.036,74 | 10.455.315,39 | 73.587.721,35 | 42.799.233,73 |
| 010111 | 38.175.203,91 | 26.575.101,18 | 11.600.102,73 | 20.683.224,24 |
| 010112 | 2.739.315,96 | 1.329.604,45 | 1.409.711,51 | 1.543.386,43 |
| 010113 | 75.587.882,54 | 25.297.139,33 | 50.290.743,21 | 21.553.659,78 |
| 010114 | 76.176.444,12 | 41.162.506,87 | 35.013.937,25 | 25.061.394,17 |
| 010115 | 20.714.907,49 | 7.696.195,43 | 13.018.712,06 | 24.009.251,05 |
| 010116 | 34.387.405,09 | 10.473.481,97 | 23.913.923,12 | 15.724.143,46 |
| 010117 | 14.239.768,67 | 7.986.704,82 | 6.253.063,85 | 19.181.038,69 |
| 010118 | 7.496.001,45 | 2.022.693,29 | 5.473.308,16 | 33.011.568,49 |
| **Soma** | **559.497.180,07** | **237.887.780,14** | **321.609.399,93** | **315.915.383,44** |

A coluna "cliente inativado" é zero em todas e ficou de fora da tabela. Em cada linha,
com cliente = própria + outra filial + inativado.

**A conta da empresa inteira** (12 meses):

| | Valor |
|---|---:|
| Notas com cliente, 13 filiais ativas | R$ 559.497.180,07 |
| Notas com cliente, filiais inativas | R$ 17.370.644,95 |
| **Notas com cliente, todas as filiais** | **R$ 576.867.825,02** |
| Notas sem cliente no CRM, 13 filiais ativas | R$ 315.915.383,44 |
| Notas sem cliente no CRM, filiais inativas | R$ 4.731.244,79 |
| **Tudo o que foi emitido** | **R$ 897.514.453,25** |

As filiais inativas não aparecem em nenhuma visão de filial — o cabeçalho as recusa com 422 (§11.3) —
e entram na visão da empresa. É a única diferença entre "soma das 13 filiais" e "empresa inteira", e
ela está nomeada acima.

**A nota sem cliente no CRM estava fora do total — corrigido nesta revisão.** A versão 1.0 dizia que
ela era "somada à parte", mas o repositório não a lia. Agora ela entra em quatro grupos fora do mapa,
pela natureza da contraparte (documento 30): `ContraparteSemCadastro` (cliente não cadastrado, nota
sem documento ou contraparte não classificada — a única falha de cadastro acionável),
`RepasseDeFabrica`, `EmpresaDoGrupo` e `OutraRevenda`. Nas 13 filiais ativas: contraparte sem
cadastro R$ 230,7 mi, fábrica R$ 81,8 mi, empresa do grupo R$ 3,5 mi, outra revenda R$ 0. Esses
grupos nunca vão para um município.

#### 8.5.5 Validação na aplicação, com perfis de teste [medido]

**Ambiente isolado:** banco `TracbelCrmValidacao` (cópia do banco de desenvolvimento no contêiner
local), API desta versão em Desenvolvimento em `localhost`, front pelo Vite local e Playwright a
1600 px. Nenhum servidor e nenhum usuário real foram tocados.

**Os perfis** (`scripts/banco/validacao/perfis-de-teste-da-visao-da-empresa.sql`, que se recusa a
rodar em outro banco):

- dois usuários de natureza **Teste**, com e-mail fictício de domínio de exemplo, os dois na filial
  010101: um autorizado e um sem autorização;
- um conjunto de permissão só de validação, com `Empresa.AlcanceEntreFiliais` em profundidade
  Organização, concedido **só** ao perfil autorizado e com vencimento em 7 dias;
- conferência no fim do script: **0 concessões para quem não é perfil de teste**.

Para a ponte provisória reconhecer a concessão, ela passou a ler as concessões explícitas **só em
Desenvolvimento** (`HonrarConcessoesExplicitas`); com o Entra ID a leitura vale sempre. Concessão
vencida e conjunto desativado não dão alcance (testes de API com 403).

| Verificação | Perfil autorizado | Perfil sem autorização |
|---|---|---|
| Opção "Empresa inteira" no filtro Visão | habilitada, "disponível para o seu perfil" | **desligada**, com o motivo: "exige a permissão de alcance entre filiais; o seu perfil não a tem, e a distribuição oficial dos acessos está pendente" |
| API `visao=Empresa` | 200 | **403** |
| Tentativa forçada, com a tela adulterada para pedir a visão da empresa | — | **403**; a tela volta para a filial |
| Filtro "filial de cadastro do cliente" de outra loja, na visão da filial | 403 | 403 |
| O mesmo filtro, na visão da empresa | 200 | 403 |
| Grupos fora do mapa na visão da filial | 7, com `ClienteDeOutraFilial` | 7, com `ClienteDeOutraFilial` |
| Grupos fora do mapa na visão da empresa | 6: `ClienteDeOutraFilial` deixa de existir | não alcança |
| "Filial que vendeu" = 010102, na visão da empresa | o cartão de vendas vai de R$ 533,8 mi para R$ 17,7 mi; municípios, pendência e potencial não mudam, porque o filtro só mexe nas vendas | não alcança |
| Detalhe de Ribeirão Preto, abrir e fechar | sem erro | — |
| Polígonos em cada um dos três mapas | 645, dos quais 203 na ADR | 645, dos quais 203 na ADR |
| Filtro Região da ADR | Norte 83 · Noroeste 120 · Norte e Noroeste 203 | o mesmo |
| Selos de classificação | os cinco do §8.7 | os mesmos |
| Erros no console | 0 | 0 |

A abertura do alcance entre filiais foi para o diário, com o motivo, a cada consulta da visão da
empresa. As capturas e o relatório `validacao-perfis.json` estão em `dados-locais/capturas/`, fora do
Git.

### 8.6 A conciliação dos valores, numa tabela só [medido]

**Recorte.** Competências de 09/2025 a 08/2026 (12 meses fechados, inclusive); valor líquido das
notas de saída de venda do Protheus (documento 30), **sem abater devolução nem cancelamento** (P-5).
Filiais ativas: as 13 que o cabeçalho aceita (010101, 010102, 010103, 010107, 010109 e 010111 a
010118). Filiais inativas: `EstaAtiva = 0`. O cadastro tem 18 empresas — 16 filiais `0101NN` (13
ativas; 3 inativas: Guaíra, Ituverava e Monte Alto) e 2 empresas antigas inativas —, e **no período só
as 3 lojas inativas tiveram nota** (com cliente: R$ 9,3 mi, R$ 6,1 mi e R$ 1,9 mi). Nenhum outro filtro.

**Reprodução.** `scripts/banco/conferencias/conciliacao-das-vendas-do-territorio.sql`, somente
leitura, com `@De = '2025-09-01'`, `@Ate = '2026-08-01'` e `@Filial = '010101'` (a filial da
captura).

**Categorias sem sobreposição:** cada linha de faturamento (cliente ou contraparte × filial × mês)
cai em **exatamente uma** linha da tabela.

| Linha | Categoria | Filiais | Na visão da filial | Na visão da empresa | Linhas | Valor (R$) |
|---|---|---|---|---|---:|---:|
| A1 | Nota com cliente no CRM, cadastrado na **própria** filial que vendeu | 13 ativas | município do cliente¹ | município do cliente¹ | 6.385 | 237.887.780,14 |
| A2 | Nota com cliente no CRM, cadastrado em **outra** filial | 13 ativas | grupo `ClienteDeOutraFilial`, fora do mapa | município do cliente¹ | 6.914 | 321.609.399,93 |
| A3 | Nota com cliente inativado no CRM | 13 ativas | grupo `ClienteInativado` | grupo `ClienteInativado` | 0 | 0,00 |
| A4 | Nota com cliente no CRM | 3 inativas | não aparece: filial inativa não é selecionável | município do cliente¹ | 1.648 | 17.370.644,95 |
| B1 | Nota sem cliente no CRM — contraparte sem cadastro (não cadastrada, sem documento ou não classificada) | 13 ativas | grupo `ContraparteSemCadastro` | o mesmo grupo | 7.802 | 230.675.187,56 |
| B2 | Nota sem cliente no CRM — repasse de fábrica | 13 ativas | grupo `RepasseDeFabrica` | o mesmo grupo | 395 | 81.756.350,45 |
| B3 | Nota sem cliente no CRM — empresa do grupo | 13 ativas | grupo `EmpresaDoGrupo` | o mesmo grupo | 302 | 3.483.845,43 |
| B4 | Nota sem cliente no CRM — outra revenda | 13 ativas | grupo `OutraRevenda` | o mesmo grupo | 0 | 0,00 |
| B5 | Nota sem cliente no CRM, todas as naturezas | 3 inativas | não aparece | grupo da natureza | 699 | 4.731.244,79 |
| **Total** | **Tudo o que foi emitido** | **16** | | | **24.145** | **897.514.453,25** |

¹ Ou, quando o endereço do cliente não tem município utilizável, um dos grupos de endereço fora do
mapa (`SemMunicipio`, `MunicipioSemCodigoIbge`, `OutraUf`) — nunca somado a uma cidade.

**Como os valores citados se relacionam** — todos saem da tabela acima:

| Valor citado | O que é | Composição | Valor exato (R$) |
|---|---|---|---:|
| R$ 559,5 mi | com cliente, 13 filiais ativas | A1 + A2 + A3 | 559.497.180,07 |
| R$ 321,6 mi | a **parte** dos R$ 559,5 mi de cliente cadastrado em outra filial — não é valor adicional | A2 | 321.609.399,93 |
| R$ 315,9 mi | sem cliente no CRM, 13 filiais ativas | B1 + B2 + B3 + B4 | 315.915.383,44 |
| R$ 320,6 mi | sem cliente no CRM, **todas** as filiais: os R$ 315,9 mi mais os R$ 4,7 mi das inativas | B1 + B2 + B3 + B4 + B5 | 320.646.628,23 |
| Filiais inativas | com cliente R$ 17,4 mi + sem cliente R$ 4,7 mi | A4 + B5 | 22.101.889,74 |
| R$ 897,5 mi | empresa inteira, tudo o que foi emitido | R$ 559,5 mi + R$ 17,4 mi + R$ 320,6 mi = A1 a B5 | 897.514.453,25 |
| R$ 217,1 mi | total da captura: **só a filial 010101**, na visão da filial | cliente cadastrado nela 86.896.431,61 (parte de A1) + cliente de outra filial 39.830.356,01 (parte de A2) + cliente inativado 0,00 (A3) + nota sem cliente 90.379.488,14 (parte de B1 a B3) | 217.106.275,76 |
| R$ 533,8 mi | cartão "Vendas no período" da visão da empresa (§8.5.5): **só** os clientes com endereço nos 203 municípios da ADR | parte de A1 + A2 + A4 | medido na tela, no banco de validação |

**Por que esses números não se somam entre si:** R$ 217,1 mi é uma filial; R$ 559,5 mi e
R$ 315,9 mi são as 13 filiais ativas; R$ 897,5 mi é a empresa. O cartão de vendas soma só os
municípios da ADR; o total da consulta soma também os grupos fora do mapa. A abertura das 13 filiais,
uma a uma, está no §8.5.4.

**Os 645 e os 203.** O mapa-base é a malha municipal oficial do IBGE de São Paulo
(`public/geo/sp-municipios.json`): **645** polígonos em cada um dos três mapas, nas duas visões e nos
dois perfis — os 645 municípios de SP do cadastro do IBGE (§4.2). O filtro Região da ADR destaca
Norte **83**, Noroeste **120** e, juntos, **203** — os mesmos 203 conciliados no §4.2; o resto de SP
fica em cinza, "fora da ADR" (§8.5.5).

### 8.7 O que é medido e o que é provisório, na API e na tela

A resposta da API traz `classificacoes`, uma por indicador. A tela mostra o selo no título de cada
mapa, no cartão e no detalhe do município, e o quadro "Como ler estes números" explica cada um.

| Indicador | Classificação na API | Selo na tela | Por quê | Pendência |
|---|---|---|---|---|
| Vendas | `Medido` | "Medido" | nota fiscal do Protheus, conciliada ao centavo (§8.6) | período e devolução (P-4, P-5) |
| Pendência de visita | `RegraComercialProvisoria` | "Regra provisória" | "visita" é qualquer interação registrada; cadência declarada no CRM | P-2, P-3 |
| Pós-venda | `RegraComercialProvisoria` | "Composição provisória" | peça + serviço pelo grupo do item da nota | composição (documento 34, decisão 8) |
| Potencial | `Estimativa` | "Estimativa · regra a confirmar" | área regional do IBGE × regra do café não confirmada | P-8 |
| CEN do município | `FonteAConfirmar` | "Duas fontes · vigência a confirmar" | duas planilhas sem vigência; 82 municípios com nomes diferentes e 46 com grafia diferente, preservados lado a lado | P-1 |

**Nenhum indicador de pós-venda, visita ou potencial aparece como validado comercialmente** — nem na
tela, nem na API, nem na matriz do §2. Teste de API:
`O_cen_e_comparado_sem_fusao_e_cada_indicador_diz_como_ler`.

---

## 9. Plano de execução

| Etapa | O que | Depende de | Critério de aceite |
|---|---|---|---|
| E0 | Proteção de dados: planilhas fora do Git | — | `git status` sem planilha; `.gitignore` cobre `*.xlsx` e `dados-locais/` |
| E1 | Reconhecer o catálogo de municípios no IBGE | internet (API pública) | 203/203 da ADR com código; rodar duas vezes grava zero na segunda; nome anterior em `AlteracaoDeCampo` |
| E2 | Carregar ADR e responsáveis das duas planilhas | E1 | 203 ADR + 35 fora; afirmações por fonte; 90 linhas vazias em `MensagemDescartada`; reexecução idempotente |
| E3 | Área plantada PAM e a regra de potencial | E1 | ano mais recente de SP carregado; zero × não disponível preservados |
| E4 | API de indicadores territoriais | E1–E3 | totais batem com a consulta de conferência independente; filtros validados com 422 |
| E5 | Rascunho visual | E4 | imagem com os três mapas, filtros, legendas, detalhe e perfis; ilustrativo marcado |
| E6 | Tela de mapas | E4, E5 | geometria oficial; zero, sem dado e fora da ADR distinguíveis; detalhe por município |
| E7 | Testes, validação visual, revisão independente | E1–E6 | testes passando; capturas de tela com amostras rastreáveis |
| E8 | Documentação e memória | E7 | este documento com o §11 preenchido |
| E9 | Corrigir os endereços das grafias cortadas (autorizado em 13/09/2026) | E1 + malha municipal do IBGE | 192 de 194 com trilha; pendentes com motivo; 2ª rodada grava zero |

**Ordem das cargas.** Deixou de importar (versão 1.2): a carga do Vórtice mantém as correções e roda
a conferência das grafias no fim; a carga do território faz a mesma conferência. Rodadas em qualquer
ordem, a segunda não muda nenhum endereço (§4.6.1).

**Recuperação.** A migração cria quatro tabelas, não remove coluna e **recria 10 restrições CHECK** de
nome de entidade com listas ampliadas — nenhuma linha existente é rejeitada; a maior tabela afetada,
`integracao.ChaveExterna`, tem cerca de 400 mil linhas no banco de desenvolvimento. Ela foi regenerada
nesta revisão, antes de qualquer commit ou publicação, para incluir a chave estrangeira de
`ImportadoPorId`. O reconhecimento IBGE altera `Municipio.Nome` e `CodigoIbge`, e a correção das
grafias altera `Endereco.MunicipioId`; o valor anterior dos dois fica em `AlteracaoDeCampo`, e o
`Down` da migração não desfaz dado — a volta é pela auditoria.

---

## 10. Pendências e decisões

| # | Pendência | Impacto | Quem resolve |
|---|---|---|---|
| P-1 | Qual planilha de CEN vale e desde quando (C-1 a C-3) | o detalhe mostra as duas fontes; filtro por CEN e gestor desligado | gerente comercial |
| P-2 | O que conta como **visita**; periodicidade (30/60/90/120 da maquete ou a declarada); elegibilidade | o mapa A e o cartão de cobertura usam a regra declarada no CRM. Medido em 14/09/2026: **0 de 179** tipos de atividade marcados como visita (`ContaParaCobertura`), e 30.488 das 122.002 interações são registro do sistema — com elas, 39.441 vínculos comerciais têm contato; só com contato Ativo ou Receptivo, 24.318 (documento 36, §3.4) | gerente comercial |
| P-3 | Unidade da pendência: cliente ou vínculo | o mapa A usa vínculo | gerente comercial |
| P-4 | Período das vendas: FYTD (com qual início de ano fiscal?) ou 12 meses | o mapa B usa 12 meses fechados e oferece "ano civil até o último mês fechado"; os cartões usam ano civil; FYTD não é oferecido (documento 36) | diretoria |
| P-5 | Devolução e cancelamento nas vendas | o mapa B não abate | fiscal/controladoria |
| P-6 | Definição e lista de SAM/KAM/Varejo | filtro desligado | comercial |
| P-7 | Taxonomia de produto e porte; trazer o item da nota | filtros de produto e modelo desligados | comercial + TI |
| P-8 | Regra do café (horizonte, arredondamento, vigência), regras das demais culturas, preço | o mapa C só em máquinas teóricas, só café | comercial |
| P-9 | Ciclo de troca e concorrência | sem potencial ajustado | comercial |
| P-10 | Perfis e hierarquia (CEN, gerente, diretor), e **quem recebe `Empresa.AlcanceEntreFiliais`** | nenhum usuário real tem a visão da empresa; ela foi validada só com perfis de teste (§8.5.5) | TI + diretoria (documento 34, decisão 4) |
| P-11 | ART: **a conexão funciona** (14/09/2026), mas o usuário só lê a view de vendas `bi_art_veiculos`. Falta o dono do ART dizer onde estão propriedade, área, cultura e horímetro e conceder leitura; e confirmar a rota do servidor 10.150.4.249 até 10.100.5.134:3306 para a sincronização agendada (documento 35, §7.8) | propriedade, cultura, área e horímetro seguem bloqueados; parque vendido, linha e produto já têm fonte | dono do ART + infraestrutura |
| P-12 | **Aprovar** a reescrita do histórico do GitHub — procedimento pronto e não executado (documento 33; recomendação: opção C+) | dado de cliente e de funcionário publicado | Ricardo (aprovação escrita) |
| P-13 | O repositório deveria morar em `E:\CraftOps\` pela regra do ambiente | nenhum na execução | Ricardo |
| P-14 | Guaíra, Ituverava e Monte Alto operam? (C-4) | 11 municípios da ADR com loja inativa no CRM | diretoria |
| P-15 | Conferir os **2 endereços** cuja coordenada contradiz o município (§4.6) | 2 clientes fora do mapa | comercial (cadastro) |
| P-16 | A quem pertence o cliente que compra em mais de uma filial (1.706 clientes; R$ 321,6 mi com filial de cadastro diferente da que vendeu) | na visão da filial esse valor fica à parte | diretoria + comercial |
| P-17 | Cadastrar as contrapartes sem cliente no CRM (R$ 230,7 mi de contraparte sem cadastro em 12 meses nas 13 filiais ativas, linha B1 do §8.6) | o valor fica fora dos municípios | comercial (cadastro) |
| P-18 | Dado pessoal e identificadores no histórico do GitHub: login de funcionário em 80 arquivos, nome em 70, e-mail corporativo em 24, 19 CPFs de terceiros (documento 33, §1) | exposição de dado pessoal | Ricardo + encarregado de dados |
| P-19 | Consentimento do administrador do Entra ID para o aplicativo "Tracbel Agro - CRM": o login corporativo mostra "Aprovação necessária" | **resolvida**: 4 pessoas já entraram pelo Entra ID no servidor (medido em 14/09/2026) | administrador do Entra ID |
| P-20 | **Quais filiais cada usuário pode escolher.** Hoje, qualquer filial ativa pelo cabeçalho (D-11). Implementação pronta para fazer assim que decidido: filial de casa e as concedidas explicitamente; as demais, 403 | com login ativo, qualquer pessoa vê clientes e vendas de qualquer filial | TI + diretoria (junto com a decisão 4 do documento 34) |
| P-21 | Este documento traz faturamento por filial ao centavo e contagem de clientes. Pode ir para o GitHub, ou a tabela fica em `dados-locais/` e o documento guarda só o método e o script? | dado comercial da empresa no repositório | Ricardo, antes do push |
| P-22 | Carga do território no banco do servidor — **resolvida**: rodada em 14/09/2026 12:49; o banco do servidor tem 203 municípios da ADR (medido) | — | Ricardo |
| P-23 | Publicar o aviso de território não carregado e as mensagens de permissão | no servidor, a tela ainda mostra zero quando o território falta | Ricardo (publicação) |
| P-24 | Onde e quando as cargas rodam no servidor, e a rotina de cópia de segurança (destino, retenção, modo de recuperação `FULL` sem backup de log) | dados chegam ao servidor só quando alguém roda a carga; banco oficial sem backup rotineiro | Ricardo + administrador do servidor |
| P-25 | Completar o documento dos clientes pela SA1 do Protheus (387 CNPJs que faturam existem no CRM só por nome). **A credencial existe** no `.env` da raiz e foi testada: lê a `SA1010`; o usuário é `db_owner`, e a integração só lê | correção de cadastro CRM × Protheus ainda não implementada | TI (implementação) |
| P-26 | Cadastrar as metas de faturamento: `organizacao.Meta` tem 0 linhas e nenhuma meta preenchida foi recebida (a planilha-modelo `04-metas.xlsx` do documento 34 só tem exemplos fictícios; `IVS_UsrMeta` do Vórtice tem 1 linha) | o cartão "Meta e realizado" mostra só o realizado | comercial/diretoria |
| P-27 | Regra de sanitização de cliente: 4 documentos em 8 cadastros de filiais diferentes e 7.391 clientes em carteira sem CPF/CNPJ | nada foi fundido nem completado (documento 36, §3.3) | comercial (cadastro) + TI |
| P-28 | Classe de cliente cadastrado em outra filial, na visão por filial: 501 vínculos medidos pela cadência D | diferença de 38 vínculos no prazo no consolidado (documento 36, §3.4) | TI + diretoria (com P-10/P-20) |
| P-29 | Fonte de mercado (emplacamento) e venda perdida por município | sem participação de mercado; perda sem município | comercial + TI |
| P-30 | Publicar os cartões e os mapas novos (documento 36) | o servidor mostra a versão anterior | Ricardo (publicação) |
| P-31 a P-39 | Integração do ART: regra de Grandes Contas × SAM/KAM, 91 produtos sem correspondência, 851 compradores ausentes, filiais inativas com vendas, 47 divergências de dono, chassi curto, famílias faltantes, acesso a propriedade/área/cultura/horímetro e a tela de revisão com ação | detalhadas no documento 35, §10.10 | comercial, cadastro, responsável pelo ART, TI |

**Resolvidas nesta revisão:** a correção das grafias cortadas (antiga P-15, autorizada em 13/09/2026)
e a escolha entre venda por filial ou por empresa (antiga P-16: as duas visões existem, §8.5).

### 10.1 O que você precisa fornecer — a lista curta

As planilhas-modelo (com instruções, exemplos fictícios e a versão pré-preenchida com o que já
existe) e a lista das 17 decisões em ordem de prioridade, para encaminhar ao gerente, estão no
**documento 34**.

1. **Acesso ao ART** a partir da rede do CRM, **ou** a exportação de: propriedades (cliente e não
   cliente, município, área total e agricultável, CPF/CNPJ do proprietário), culturas por propriedade
   e safra, e parque de máquinas (chassi, modelo, ano, horímetro com data e origem).
2. **Arquivo de meta**: filial, carteira ou CEN × mês × valor, e o início do ano fiscal.
3. **Lista oficial de usuários do comercial**: e-mail corporativo, cargo (CEN, gerente, diretor),
   gestor imediato, loja e data de início — e quem recebe a visão da empresa.
4. **Uma planilha única e datada** de município × CEN × gestor, ou a decisão de qual das duas vale.
5. **Definição e lista** de SAM, KAM e Varejo por cliente.
6. **Taxonomia de produto** (linha, família, série, porte) e autorização para trazer o item da nota
   do Protheus.
7. **Regras de potencial** por cultura (hectares por máquina, modelo, horizonte, arredondamento,
   vigência), com a confirmação da regra do café.
8. **Ciclo de troca** por categoria e uma fonte de parque concorrente ou de emplacamento.
9. **Decisões**: o que conta como visita e a periodicidade; FYTD ou 12 meses; abater devolução e
   cancelamento; o que compõe pós-venda; o que são os 35 municípios fora da ADR; se Guaíra, Ituverava
   e Monte Alto operam; a quem pertence o cliente que compra em várias filiais.
10. **Semântica da planilha de exemplo**: unidade da área, "POPs", "Peso", "Motor", "Filial Cliente",
    cliente com CPF e CNPJ.
11. **Aprovação escrita** da reescrita do histórico, com a opção (documento 33).

---

## 11. Verificação

Tudo abaixo é saída real de 13/09/2026, no banco de desenvolvimento (carga de 05–06/09/2026), com a
API rodando o código desta revisão.

### 11.1 Construção e testes

| Verificação | Resultado |
|---|---|
| `dotnet build Tracbel.Crm.sln` | 0 erro, 0 aviso |
| Migração `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial` | **regenerada nesta revisão**: a versão anterior, nunca commitada nem publicada, foi revertida no banco local e removida, e a nova inclui chave estrangeira e índice de `ImportadoPorId`. `has-pending-model-changes`: nenhuma mudança pendente |
| Testes de domínio | **183** passando (inclui `Organizacao/AreaDeAtuacaoTestes`, `CorrecaoDeGrafiaCortadaTestes`, `CorrecaoDeGrafiaCortadaNaRecargaTestes` e `ComparacaoDoCenTestes`) |
| Testes de integração | **101** passando (inclui `Carga/SaneamentoDeTerritorioTestes`, `Carga/VariantesCortadasDoCatalogoTestes` e `Ibge/PoligonoMunicipalTestes`; o saneamento do legado usa documentos fictícios) |
| Testes de aplicação | **56** passando (inclui `Territorio/VisaoDaEmpresaTestes`, com o repositório e o filtro global de verdade, `Seguranca/ConcessaoExplicitaNaPonteProvisoriaTestes` e `Carga/ConsolidacaoDeGrafiasCortadasTestes`) |
| Testes de API | **71** passando (inclui `IndicadoresTerritoriaisTestes`: outra filial, inativado, nota sem cliente, perfil autorizado, concessão vencida, conjunto desativado, filtros na visão da empresa, comparação do CEN, classificações, 403 e 422) |
| Testes de arquitetura | **62** passando, com os de migração em contêiner (72 tabelas) |
| **Total (versão 1.2, 13/09/2026, depois da 3ª revisão)** | **473 passando, 0 falha**; `dotnet build` com 0 aviso e 0 erro; `tsc -b` sem erro |
| `tsc -b` e `oxlint` | sem erro; nenhum aviso nos arquivos do território (os 8 avisos são de telas anteriores) |

### 11.2 A carga

| Etapa | Rodada 1 (depois de regenerar a migração) | Rodada 2 (idempotência) |
|---|---|---|
| Catálogo × IBGE | 0 gravadas — as linhas já estavam reconhecidas desde a rodada da manhã (5.458 gravadas, 113 criadas) | 0 |
| Grafias cortadas → município oficial | 194 endereços lidos, **192 reapontados**, 2 pendentes, 1 chave de origem reapontada | 2 lidos, 0 reapontados, 2 pendentes, 0 chaves |
| Área de atuação | 238 linhas novas (203 ADR, 35 fora) | 238 mantidas |
| Responsáveis da planilha C | 203 afirmações | 203 mantidas |
| Responsáveis da planilha D | 406 afirmações; 90 linhas vazias recusadas | 406 mantidas |
| Área plantada PAM 2024 | 45.582 linhas (34.107 zero, 5.248 não disponível) | 45.582 mantidas |

O ciclo de recarga da versão 1.2 — carga do Vórtice, carga do território e nova carga do Vórtice,
com leitura real — está no §4.6.1.

### 11.3 A API contra consulta SQL independente

Filial 010101, 12 meses fechados (set/2025–ago/2026). A consulta de conferência foi escrita à
parte, sem usar o repositório:

| Métrica | API | SQL |
|---|---:|---:|
| Clientes (mapa + fora do mapa) | 10.796 | 10.796 |
| Vendas com cliente (mapa + fora do mapa) | R$ 126.726.787,62 | R$ 126.726.787,62 |
| Nota sem cliente no CRM | R$ 90.379.488,14 | R$ 90.379.488,14 |
| Vínculos elegíveis | 5.962 | 5.962 |
| Vínculos pendentes | 1.668 | 1.668 |
| Ribeirão Preto — clientes | 446 | 446 |
| Ribeirão Preto — vendas | R$ 7.714.547,18 | R$ 7.714.547,18 |
| Ribeirão Preto — pós-venda | R$ 2.892.701,03 | R$ 2.892.701,03 |
| Ribeirão Preto — elegíveis / pendentes | 420 / 317 | 420 / 317 |
| Clientes sem município | 2.801 | 2.801 |
| Vendas com cliente somadas das 13 filiais | R$ 559.497.180,07 | R$ 559.497.180,07 |

As 13 filiais, uma a uma, estão no §8.5.4. Os cenários de visão e de filtro:

| Cenário (cabeçalho 010101, 12 meses fechados) | Resultado |
|---|---|
| Sem filtro | 200; total da tela R$ 217.106.275,76 |
| `filialDaVenda=010101` | 200; igual ao sem filtro |
| `filialDoCliente=010101` | 200; R$ 86.896.431,61, só cliente da própria filial (igual ao SQL), sem os grupos de outra filial e de nota sem cliente |
| Sem filtro = cliente da filial + outra filial + inativado + sem cliente | 217.106.275,76 = 86.896.431,61 + 39.830.356,01 + 0,00 + 90.379.488,14 |
| `visao=Empresa` | **403** `sem-acesso`, nomeando a permissão que falta |
| `filialDoCliente=010103` (fora do alcance) | **403** `sem-acesso` |
| `visao=Regional` | **422**, campo `visao` |
| `competenciaInicial=2025-10&competenciaFinal=2026-09` (até o mês corrente) | 200, com a métrica `periodoParcial` |
| Cabeçalho com filial inativa (010104) | **422**, campo `X-Tracbel-Empresa` |
| As 13 filiais ativas | 13 × 200 |

Depois da correção das grafias, as 13 filiais somam **145 clientes** em São José do Rio Preto
(R$ 1.066.696,35 em 12 meses); o grupo `MunicipioSemCodigoIbge` ficou com 343 clientes.

Amostras rastreáveis: Altinópolis com 10.000 ha de café na PAM → 1.000 tratores teóricos; Serrana
90 ha → 9; São Carlos 700 ha → 70.

### 11.4 Validação visual

Playwright contra a aplicação rodando (API desta revisão + Vite), filial 010101, 1600 px de largura:

- os três mapas desenham os 645 polígonos oficiais; nenhum erro no console;
- "sem dado" hachurado: 98 municípios no mapa de pendência e 6 no de vendas (eram 99 e 8 antes da
  correção das grafias), 0 no de potencial; o resto de SP em cinza "fora da ADR";
- filtros novos: "Visão", com "Empresa inteira" desligada e o motivo; "Filial que vendeu"; "Filial de
  cadastro do cliente";
- os 4 filtros sem dado aparecem desligados, com o motivo;
- o quadro "O que estes mapas não dizem" lista a visão da empresa, o cliente de outra filial e a
  nota sem cliente no CRM, com o valor.

**Versão 1.2:** a validação com os dois perfis de teste, os selos de classificação, os 645 polígonos
e o filtro da ADR estão no §8.5.5.

**Defeitos achados na validação e corrigidos (versão 1.0):** a tabela de responsáveis vazava por cima
das colunas do detalhe; cartões sem espaçamento interno; valor em reais quebrando linha na tabela.

As capturas (rascunho visual) estão em `dados-locais/capturas/` e o relatório nominal da conciliação
em `dados-locais/conciliacao-adr-responsaveis-2026-09-13.csv` — **fora do Git**, porque têm número
real e nome de funcionário.

### 11.5 Revisão independente

**Primeira revisão** (agente revisor separado, sobre a versão 1.0): **COMMENT** — nenhum bloqueio, 3
achados médios e 4 baixos. Conferiu sem problema a partição da cobertura, vendas e pós-venda,
potencial zero × nulo, totais, idempotência, reconhecimento municipal, estados visuais, ausência de
dado da empresa no Git, padrões de banco, migração, divergência de CEN e a consistência deste
documento.

| # | Achado | Severidade | Tratamento nesta revisão |
|---|---|---|---|
| 1 | Pasta de depuração do PowerShell na raiz, com resposta da API | média | removida; o `.gitignore` passa a cobrir `@{*}/` |
| 2 | A tela checava `potencial[0]` de forma frágil | média | `m.potencial.length > 0 && m.potencial[0].maquinasTeoricas !== null` |
| 3 | `AreaPlantadaNoMunicipio` sem `ImportadoPorId` | média | coluna criada e preenchida pela carga |
| 4 | `MunicipioDaAreaDeAtuacao.Conferir` não atualizava `ImportadoPorId` | baixa | atualiza, com teste |
| 5 | `ImportadoPorId` sem chave estrangeira nem justificativa | baixa | chave estrangeira para `Usuario` e índice nas três tabelas (migração regenerada) |
| 6 | `aria-selected` em `<tr>` fora de grade | baixa | `aria-current` |
| 7 | Período até o mês corrente sem aviso de parcial | baixa | métrica `periodoParcial` na resposta e na tela |

**Achados desta revisão, além dos do revisor:**

| # | Achado | Tratamento |
|---|---|---|
| 8 | **Omissão:** nota sem cliente no CRM (R$ 320,6 mi em 12 meses) fora do total, com o §8.2 da versão 1.0 dizendo o contrário | quatro grupos fora do mapa por natureza; testes de API e de aplicação; conferido filial a filial (§8.5.4) |
| 9 | E-mail de pessoa real em 2 arquivos de teste | trocado por e-mail fictício; o histórico está no documento 33 |
| 10 | Protótipo com 2 logins reais e extração do Vórtice com e-mail de funcionário, publicados | documento 33, §1.2, e opção B+ — depende de aprovação |

### 11.6 Parecer da segunda revisão independente

Agente revisor separado, sobre o estado final desta revisão (código, testes, documentos 32 e 33 e as
evidências de SQL e de API): **APPROVE**.

- **Vendas:** nenhuma omissão nem dupla contagem; a conferência SQL independente bate ao centavo nas
  13 filiais, e a nota sem cliente entra nos quatro grupos por natureza.
- **Permissão:** a visão da empresa é recusada sem `Empresa.AlcanceEntreFiliais`, e o filtro de
  filial fora do alcance também; `SemPermissao` vira 403.
- **Grafias cortadas:** a regra "só inequívoco" é respeitada (nome cortado com prefixo único, UF e
  contorno oficial); 192 de 194, 2 pendentes por coordenada; trilha completa; 2ª rodada idempotente.
- **Documento 33:** preparado e não executado (`main` local igual a `origin/main`), com cópia de
  segurança, conferência antes do envio e volta atrás; sem dado sensível no texto.

| Achado | Severidade | Tratamento |
|---|---|---|
| Nome real que sobrou em `UsuarioTestes.cs` depois da troca dos e-mails | baixa | o revisor apontou o comentário da linha 43; a conferência achou o mesmo nome também no login e no nome do usuário auxiliar do teste, e as quatro ocorrências foram trocadas por dado fictício |

**Riscos residuais apontados:** o histórico remoto continua com e-mail de funcionário até a
aprovação do documento 33; a soma do faturamento é feita em memória e precisa ser observada se o
volume crescer muito além de dezenas de milhares de linhas por consulta; a carga usa
`DateTime.UtcNow` direto, e não o relógio injetável, o que dificulta testar o instante exato.

### 11.7 Parecer da terceira revisão independente (versão 1.2)

Agente revisor separado, sobre a versão 1.2 antes do commit (código, testes, scripts e documentos 32
a 34): **REQUEST CHANGES**, com dois achados altos. O revisor rodou a solução: 470 testes passando.
Conferiu sem problema a regra das concessões (conjunto ativo, não vencida, só em Desenvolvimento pela
ponte), a regra das grafias, a migração, a classificação na tela e na API, as contas do §8.6, o SQL
de conciliação, os scripts e a ausência de credencial e de planilha no que vai para o commit.

| # | Achado | Severidade | Tratamento |
|---|---|---|---|
| 1 | A filial do cabeçalho não é conferida contra o usuário (anterior a esta entrega) | alta | **não corrigido**: restringir exige decidir quais filiais cada pessoa atende. Registrado em D-11 e P-20, no §0, no §8.5.2 e na matriz (O-31, O-32) |
| 2 | O §7.6 do documento 33 usava `reset --soft`, que republicaria o índice antigo | alta | `reset --mixed`, com conferência do índice vazio e do `main` sem commit à frente |
| 3 | A conferência do §7.4 do documento 33 não lia o clone reescrito | média | o script aceita `--repo` e lista os arquivos pela árvore do `HEAD`; o §7.4 clona o espelho numa pasta normal |
| 4 | O documento 33 dava como feitos o commit e o novo inventário | média | virou passo a fazer; o resultado vai para o §11.8 |
| 5 | O inventário não lia mensagem de commit nem autor | média | opção `--mensagens` (documento 33, §1.6); a C+ passa a usar `--replace-message` |
| 6 | Exemplos com formato de login e nome real em comentário de `SaneamentoDeTerritorio.cs` | média | trocados por fictícios |
| 7 | Documento 24 com e-mail, telefone, razão social e nome de pessoa nos exemplos | média | trocados por fictícios; onde o dígito verificador não importa, o documento fictício tem dígito inválido de propósito |
| 8 | A correção desfeita na recarga não ia para a trilha | média | a carga do Vórtice registra a volta em `AlteracaoDeCampo` |
| 9 | Sem teste automatizado das peças da recarga | média | `ConsolidacaoDeGrafiasCortadasTestes`, com banco em memória; a ligação dentro da carga do Vórtice segue sem teste automatizado (O-49) |
| 10 | O-16 classificado como Validado, dependendo de P-4 e P-5 | média | reclassificado como Provisório |
| 11 | O prefixo único é conferido só contra o catálogo | baixa | premissa escrita no §4.6.1 |
| 12 | Endereço excluído contado como pendente a cada rodada | baixa | filtrado na consulta |
| 13 | O contorno do IBGE era baixado sem necessidade, com prazo de minutos | baixa | só é lido quando há endereço com coordenada, com prazo de 90 s por UF |
| 14 | O-48 remetia a um §11.8 inexistente | baixa | criado |
| 15 | "Migração aditiva", mas ela recria 10 restrições CHECK | baixa | o §9 descreve as duas coisas |
| 16 | Contagens de filiais diferentes entre documentos | baixa | o §8.6 e o SQL explicam: 18 empresas = 16 filiais + 2 antigas; só as 3 lojas inativas tiveram nota no período |
| 17 | Premissa de "cliente de outra filial" no SQL | baixa | anotada no script |
| 18 | O script de perfis atribuía a concessão de teste ao primeiro usuário ativo | baixa | usa a conta de sistema |
| 19 | Pasta de saída das capturas relativa à pasta do comando | baixa | relativa ao script |
| 20 | Documentos fictícios com dígito válido | baixa | inválidos onde o dígito não importa; os testes precisam de dígito válido, e nenhum corresponde a cliente do CRM |
| 21 | E-mail real em comentário de `ResolvedorDeContextoDoEntraId.cs` | baixa | trocado; os outros arquivos seguem para a troca de texto da C+ |
| 22 | `scripts/coleta` também cita a extração do Vórtice | baixa | incluído no documento 33, §4 |
| 23 | Faturamento por filial ao centavo versionado | decisão | com você, antes do push (P-21) |

**Riscos residuais para publicar.** Concessões que já existissem no banco do servidor passariam a
valer com o Entra ID — o banco de desenvolvimento, de onde o do servidor foi restaurado, tem 0, e
nenhum código grava concessão. A migração recria 10 restrições CHECK antes de a API atender. As
tabelas do território ficam vazias no servidor até alguém rodar a carga do território nele. E a
troca de filial pelo cabeçalho (D-11) continua valendo.

### 11.8 Commit, push e publicação (13/09/2026)

| Etapa | Situação |
|---|---|
| Commit | **feito**, local, na `main`: `e0419b8` — a versão publicada —, com 75 arquivos; este registro da publicação vai num segundo commit, só de documentação. Os dois estão à frente de `origin/main` (código, testes, documentos 24 e 32 a 34, scripts e `.gitignore`); nenhum arquivo de `dados-locais/`, planilha, captura ou CSV |
| Inventário depois do commit, antes do push | 16 commits. Nenhuma credencial, documento de cliente real, CPF de terceiro ou nome de usuário real **novo**. Os aumentos são os documentos fictícios de dígito válido que entraram no lugar dos reais (documento 24, dois comentários de código e o teste do saneamento) e e-mails fictícios de domínio corporativo num teste. O único login que casou com usuário do CRM, num teste da API, é o da conta de desenvolvimento semeada por `scripts/banco/seed/02-usuarios-de-desenvolvimento.sql` — não é de pessoa —, e mesmo assim foi trocado por texto fictício |
| Push | **não feito**. O GitHub responde "Repository not found" para `tracbel/Visao_360` com a conta ativa nesta estação. Falta ativar a conta que tem acesso — e decidir a P-21 antes de publicar o faturamento por filial |
| Publicação no servidor | **feita** pelo Ricardo nesta estação, com o `publicar.ps1` (a sessão de trabalho não teve permissão para rodá-lo). Conferido depois, só com leitura: serviço `RUNNING`; `/saude/banco` com o banco conectado e **0 migração pendente** — a migração `20260913155645` foi aplicada na subida; `Tracbel.Crm.Api.dll` e `Tracbel.Crm.Infraestrutura.dll` do servidor iguais, por hash, ao pacote gerado às 22:44; a rota do território responde 401 sem login |
| Como publicar | nesta estação, com a VPN: `.\scripts\deploy\publicar.ps1`. Ele roda os testes, gera o pacote, para o serviço, copia, sobe — e a API aplica a migração deste commit ao subir — e só diz "pronto" depois da saúde pelo nome DNS e da conferência por hash |
| Depois de publicar | a tela do território fica sem ADR, responsáveis e área plantada até a carga do território rodar contra o banco do servidor (`--somente-territorio`); e ninguém entra pelo login corporativo até o consentimento do administrador do Entra ID (P-19) |

### 11.9 A tela vazia no servidor (14/09/2026)

Tudo no **documento 35**. Em resumo, com evidência medida:

| Etapa | Resultado |
|---|---|
| Causa | banco do servidor com 0 municípios com código IBGE, 0 ADR, 0 responsáveis e 0 área plantada; a carga do território tinha rodado só nesta estação. API 200, sem erro de conexão nem de permissão |
| Reprodução | cópia `COPY_ONLY` do banco do servidor restaurada num SQL Server 2025 local; a tela ficou igual à do servidor (645 polígonos, 0 na ADR, "0 municípios · R$ 0") |
| Correção ensaiada | carga do território na cópia: 5.458 municípios reconhecidos, 113 criados, 192 endereços corrigidos, 203 da ADR, 203 + 406 responsáveis, 45.582 linhas de área plantada; tela com os três mapas preenchidos |
| Conferência | total da filial 010101 R$ 217.106.275,76 na API e no SQL de conciliação; Ribeirão Preto igual ao validado no §11.3; segunda carga grava zero |
| Tela | aviso de território não carregado, 403 e 401 com mensagem própria; teste de API novo; 72 testes da API passando; `tsc -b` e `oxlint` sem erro |
| Servidor | carga **não executada** pela sessão (política de permissões); script pronto e ensaiado (P-22). Cópia de segurança do banco do servidor feita e conferida antes |
| ART | sem rota interna, sem credencial no ambiente, sessão do Fluig expirada (P-11) — corrigido na versão 1.2 do documento 35: a credencial existe e a conexão funciona |
| Commit e publicação | não feitos — restrição mantida |

### 11.10 Os mapas e os cartões pelas referências (14/09/2026)

Tudo no **documento 36**. Em resumo, com evidência medida na cópia do banco do servidor:

| Etapa | Resultado |
|---|---|
| Mapas | quadro da ADR comum aos três (645 polígonos, 203 com contorno); cobertura vermelho→verde com pendência e quantidade; foco compartilhado; período "12 meses fechados" ou "ano civil"; potencial Região total / Clientes / Não clientes; responsáveis das carteiras na ficha |
| Conferência dos mapas | filial 010101: 2.566 elegíveis e 1.688 no prazo; vendas R$ 77.060.969,36; ano civil R$ 45.346.860,86; Norte 83, Noroeste 120; potencial 9.932 em 99.322 ha — tela = API = SQL |
| Cartões | rota `/relatorios/indicadores-executivos`, por partição; faturamento R$ 10.652.619,09 (com a nota sem cliente); 21.001 clientes únicos; cobertura 14.503 de 21.774; 165 vendas perdidas; meta sem cadastro — API = SQL |
| Estados | carregando, sem permissão, falha parcial, sem dado e mapas sem permissão, simulados no navegador |
| Testes | 484 .NET passando (10 novos; 1 teste de data fixa corrigido); `tsc -b` e `oxlint` sem erro |
| Commit e publicação | não feitos — restrição mantida |
