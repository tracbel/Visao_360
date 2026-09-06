# Pendentes — o que falta e as perguntas para o negócio

Nada nesta pasta foi inventado. Onde a fonte (protótipo ou legado) não bastava, o arquivo foi
gerado com `itens: []` (três casos) ou com a evidência disponível claramente marcada como parcial
ou reconstruída (quatro casos). Este documento lista os sete casos e mais seis achados de
qualidade de dado encontrados **enquanto os catálogos eram montados** — nenhum deles é hipotético,
todos vêm de arquivo real lido nesta rodada.

---

## 1. Catálogos vazios — sem fonte utilizável

### `familia.json` — 0 itens
Doc 17 §8.5 cita `EXT_VeicFam` (68 linhas no Vórtice) como a tabela substituída por
`frota.Familia`, mas **essa tabela não está entre os 116 catálogos exportados** em
`docs/extracao-vortice/catalogos-bpm/` — só as tabelas declarativas/BPM foram exportadas
integralmente; catálogos de produto do ERP (marca, modelo, família) não passaram por essa
extração. O protótipo (`catalogo-modelos.json`) também não tem um nível entre linha de negócio e
modelo (vai direto de "Tratores" para "5075E", "6110J" etc.).
**Pergunta objetiva:** alguém consegue exportar `EXT_VeicFam` do Vórtice (68 linhas, mesmo
processo de extração dos outros catálogos), ou a Tracbel prefere definir a família de forma nova
(ex.: agrupar por série comercial John Deere — 5E, 6M/6J, 7R/7J, 8R, série S de colheitadeiras)?
**Depende de:** quem tem acesso de leitura ao Vórtice para reexportar, ou do time comercial para
definir a taxonomia nova.

### `condicao_de_pagamento.json` — 0 itens
Doc 17 §6.1 cita três tabelas no Vórtice — `EXT_CONDPAGTO` (21 linhas), `IMP_FORMAPGTO` (0),
`EXT_FormaPgto` (0) — nenhuma exportada. O protótipo só menciona condição de pagamento como texto
narrativo dentro de uma aprovação (`oportunidade-1517613.json`: *"Financiamento BNDES Finame 7
anos + entrada 20%"*), nunca como lista fechada.
**Pergunta objetiva:** qual é a lista real das 21 condições de pagamento em `EXT_CONDPAGTO`? Ela
provavelmente mistura formas mortas (a tabela tem 21 linhas, mas duas tabelas irmãs com 0 linhas
sugerem que parte do cadastro de forma de pagamento já não é usada) — precisa da mesma triagem
"em uso vs. catálogo morto" que foi feita para `tipo_de_tarefa`/`IV_Acao`.
**Depende de:** exportação de `EXT_CONDPAGTO` do Vórtice.

### `resultado.json` — 0 itens
O Vórtice tem `IV_Resultado` com **4.209 linhas**, mas cada uma é específica de uma Ação
(`IV_Acao`, 980 linhas) e carrega 35 colunas de flag de comportamento (`CTRL*`) — não é uma lista
plana reaproveitável sem decisão de negócio sobre como consolidar. `IV_ClasseRes` (40 linhas,
exportada) agrupa por segmento×etapa de funil (Cobertura/Oportunidade/Pedido/Faturamento ×
Manitou/Usado/Peças/Pneus/Serviço/Prospecção) — um recorte de **relatório**, não um vocabulário de
desfecho de tarefa. O protótipo só mostra "resultado" como texto narrativo livre
(`cliente-84391.json.interacoes[].resultado`: *"Cliente confirmou análise até 30/08"*) ou como
campo binário ganho/perdido do processo inteiro (`pipeline.json`), nunca como seleção fechada por
tarefa.
**Pergunta objetiva:** qual é o vocabulário universal de desfecho que a Tracbel quer usar por
`tipo_de_tarefa`/`categoria_de_interacao`? Um rascunho **não confirmado**, só para provocar a
decisão (não faz parte do catálogo): algo como *Realizado com sucesso · Cliente ausente ·
Remarcado · Sem interesse no momento · Aguardando retorno do cliente · Cancelado*. Precisa
decidir se é uma lista única para todo tipo de tarefa ou se cada tipo de tarefa tem seu próprio
subconjunto (como o Vórtice faz, só que com 4.209 opções).
**Depende de:** Gestor Regional/Admin Comercial definir a lista; provavelmente também de olhar
alguma amostra dos 4.209 `IV_Resultado.Descricao` mais usados para não perder um caso real comum.

---

## 2. Catálogos reconstruídos por inferência — precisam de confirmação

### `marca.json` — 5 itens (John Deere, Manitou, Husqvarna, Valley, Colorado)
**Nenhuma tabela de marca foi exportada** (`EXT_VeicMarca`, citada pelo doc 17 como tendo 5
linhas, não está nos 116 catálogos de `catalogos-bpm/`). Os 5 itens vieram de menções
textuais reais em outras tabelas exportadas: nomes de `CodProcesso` (`IV_CodProcesso.csv`:
"Venda Equipamento **Manitou**", "Venda Produto **Colorado**") e de departamento
(`IVS_Depto.csv`: "MAQ-**HUSQV**", "IRRIGACAO — **VALLEY** IRRIGACAO"). É evidência real, mas
**não é a mesma coisa que ler a tabela de marca de verdade** — pode faltar item, e a
"Colorado" está marcada `ativo: false` porque as duas empresas com esse nome em `GE_Empresa`
estão `EmUso=0`.
**Pergunta objetiva:** (a) alguém consegue exportar `EXT_VeicMarca` (5 linhas, extração rápida)?
(b) a marca "Colorado" ainda vende alguma coisa hoje, ou é mesmo legado descontinuado? (c) existe
alguma outra marca que a Tracbel revende e não apareceu em nenhuma menção textual (ex.: pneus,
implementos de terceiros)?
**Depende de:** acesso de leitura ao Vórtice para o item (a); Gestor Comercial para (b) e (c).

### `concorrente.json` — 2 itens (Case IH, New Holland)
Vieram do **texto** de dois motivos de perda do protótipo ("Concorrente Case IH", "Concorrente
New Holland") — não de um campo de catálogo dedicado. O mercado brasileiro de máquinas agrícolas
tem outros fabricantes relevantes que não aparecem em nenhuma fonte lida nesta rodada (AGCO/Massey
Ferguson/Valtra, CNH, Stara, Jacto, Valtra, etc.).
**Pergunta objetiva:** qual é a lista real de concorrentes que a Tracbel acompanha em venda
perdida e na aba "Competidores" da Ficha do Cliente?
**Depende de:** Gestor Regional/Diretoria Comercial.

---

## 3. Achado geográfico — a praça e a filial não batem entre si

**A geografia do protótipo é fictícia, e a extração real do Vórtice mostra outra coisa
inteiramente.**

- `praca.json` (protótipo, `mercado-pracas.json`) usa **MT Norte, MT Sul, GO, BA** — as mesmas
  regiões aparecem em `cens.json`, `cidades-latlng.json` (Sinop, Sorriso, Lucas do Rio Verde, Nova
  Mutum, Campo Novo dos Parecis, Sapezal — todas cidades de Mato Grosso) e em toda a Ficha do
  Cliente de referência (`cliente-84391.json`, cliente fictício "Agroindustrial Salvador Arena",
  Lucas do Rio Verde/MT, culturas soja/milho safrinha/algodão).
- `empresa.json` (legado, `GE_Empresa.csv`, 18 linhas, 16 ativas) mostra que **as filiais reais da
  Tracbel Agro estão todas em São Paulo**: Ribeirão Preto, Araraquara, Barretos, Guaíra, Ituverava,
  Franca, Orlândia, Bebedouro, Monte Alto, Itápolis, São José do Rio Preto, Catanduva, Jales,
  Votuporanga, Tupã, Marília. **Nenhuma filial em MT, GO ou BA.**

Isso não é um erro de leitura — é o snapshot de `03/06/2026` do banco de produção. Duas
explicações possíveis, e são objetivamente diferentes para o projeto:

1. O protótipo usou uma região qualquer só para ilustrar a interface (MT é "genérico" para
   agronegócio) e **a praça real de atuação é São Paulo** — cana-de-açúcar, citricultura,
   pecuária, não soja/milho/algodão. Nesse caso `praca.json` e `cultura.json` desta entrega
   precisam ser **substituídos** antes de qualquer carga real, e telas como a Cobertura Regional
   (que usa `cidades-latlng.json` de MT) também.
2. A Tracbel realmente expandiu para MT/GO/BA **depois** da extração do Vórtice (03/06/2026) e o
   cadastro de filiais no legado está desatualizado/incompleto.

**Pergunta objetiva direta para o Ricardo:** qual das duas é verdade? Se for a (1), a lista real
de praças e a lista real de culturas agrícolas (pedidas explicitamente no escopo desta tarefa)
precisam vir de quem conhece a operação de SP, não do protótipo. Se for a (2), falta uma fonte de
dado mais recente que `tabelas.csv`/`GE_Empresa.csv` de 03/06/2026.

Nota lateral: `GE_Empresa.Regional` está **em branco nas 18 linhas** — o legado não tem
agrupamento regional cadastrado nem para as filiais de SP.

---

## 4. Achado de nomenclatura — duas tabelas do Vórtice não são o que o nome sugere

### `IV_Motivo` não é "motivo de perda" — é origem de lead
O doc 17 §6.1 lista `IV_Motivo` (45 linhas) como a tabela do Vórtice substituída por
`processo.MotivoDePerda`. **O conteúdo real da tabela**, lido nesta rodada
(`docs/extracao-vortice/catalogos-bpm/IV_Motivo.csv`), é uma lista de **canais de origem de
lead/contato**: Facebook, Facebook Anúncio, Google anúncio, Indicação, Site, Site Arremaq,
WhatsApp, WhatsApp Prospecção, John Deere Conecta, Rádio, TV, SMS, Show Room, SAC, Prospecção
Ativa, etc. — não tem nenhum item que se pareça com "preço alto" ou "concorrente ganhou".
`motivo_de_perda.json` desta entrega foi populado com `config-motivos-perda.json` do protótipo
(semântica correta, 8 itens); `IV_Motivo` foi realocado para `origem_de_lead.json` (45 itens),
que é onde o conteúdo realmente se encaixa (doc 17 também cita `GE_Pessoa.Origem` como "origem do
lead... texto livre com lixo de teste" — `IV_Motivo` é candidato mais forte a virar essa FK).
**Pergunta objetiva:** confirmar com quem escreveu o doc 17 se a citação de `IV_Motivo` na linha
"Motivo de perda" foi engano de nome de tabela, e atualizar o documento.

Achado menor dentro da própria tabela: as linhas `SeqMotivo 44` ("Call Now" / descrição "MF
Rural") e `SeqMotivo 45` ("MF Rural" / descrição "Call Now") parecem ter `Motivo`/`Descricao`
trocados entre si no legado — preservados literalmente em `origem_de_lead.json`, com nota em
`observacao` nos dois itens.

### `IVS_Depto` não é "linha de negócio" no sentido do protótipo
Doc 17 §8.1 e doc 15 §3.3 apontam `IVS_Depto` (29 linhas) como a tabela do Vórtice substituída por
`organizacao.LinhaDeNegocio`. O conteúdo real de `IVS_Depto.csv` é uma lista de **departamentos
de venda por produto+função** (MAQ-NOVOS, MAQ-PEÇAS, MAQ-AMS, MAQ-PNEUS, MAQ-ADM, MAQ-SERV,
MAQ-USADOS, EQUIP-VENDAS, EQUIP-ADM, EQUIP-PNEUS, EQUIP-USADOS, EQUIP-POS VD, EQUIP-LOCAC,
EQUIP-SERVIC, EQUIP-PEÇAS, EXECUT-VENDA, PROSP-MAQ, PECAS_EXTERN, PROSP-PECAS, PROSP-POSVEN,
MAQ-HUSQV, IRRIGACAO, VENDAS-DIGIT, DADOS-CADAST, DSI, DSI-PUK, MARKETING, BLOQUEADO, TI) — muito
mais parecido com **equipe/carteira interna** (que já tem `organizacao.Carteira` no modelo) do que
com as 6 linhas de produto do protótipo (Tratores, Colheitadeiras, Pulverizadores, Plantadeiras,
Implementos, Peças & Serviços). `linha_de_negocio.json` desta entrega usa as 6 do protótipo — o
que a tela de Nova Oportunidade realmente exibe.
**Pergunta objetiva:** `organizacao.LinhaDeNegocio` deveria ser só as 6 linhas de produto, ou o
modelo unificado também precisa de um catálogo separado para o departamento/equipe interna (que
pode ser mais próximo do que `IVS_Depto` realmente descreve)? Ver também a divergência de
`mix-linhas.json` abaixo.

**Divergência dentro do próprio protótipo:** `linha-icon.json`/`catalogo-modelos.json` usam 6
linhas (Tratores, Colheitadeiras, Pulverizadores, Plantadeiras, **Implementos**, "Peças **&**
Serviços"); `mix-linhas.json` (gráfico da Visão 360) usa só **5** — sem "Implementos" separado, e
grafando "Peças **e** Serviços" (com "e", não "&"). `linha_de_negocio.json` seguiu a lista de 6
(mais completa); confirmar com quem manteve o protótipo se "Implementos" deveria estar dentro de
"Peças & Serviços" no gráfico de mix, ou se o gráfico está desatualizado.

---

## 5. Achado sobre classificação do cliente — `GE_PessoaClasse` não é A/B/C/D

`classe_de_cliente.json` usa A/B/C/D (30/60/90/120 dias de frequência), que é o que o protótipo
usa em toda tela. O doc 15 §3.1 já confirma que essa classificação **"não existe formalizado"**
como coluna própria no Vórtice — e a medição desta rodada mostra por quê: a tabela
`GE_PessoaClasse` (1.878 linhas, citada pelo doc 17 como "tabela para 4 valores") na verdade tem
**6 valores completamente diferentes** de A/B/C/D:

| Valor real de `GE_PessoaClasse.Classe` | Linhas |
|---|---:|
| FILIAL | 873 |
| FUNCIONÁRIOS | 612 |
| PARCEIROS | 238 |
| FALECIDO | 71 |
| REDE CONCESSIONÁRIOS | 69 |
| SAM | 46 |

Isso não é uma classificação comercial ABCD — é uma marcação de **tipo de relacionamento**
(é filial da própria Tracbel? é funcionário? é parceiro/revenda? está falecido — relevante para
LGPD?). **Não deve ser confundida com nem migrada para `classe_de_cliente`.**
**Pergunta objetiva:** o modelo novo precisa de um catálogo separado para este "tipo de
relacionamento" (`FILIAL`/`FUNCIONARIO`/`PARCEIRO`/`FALECIDO`/`REDE_CONCESSIONARIOS`/`SAM`), já
que são 1.878 registros reais que hoje vivem em algum lugar? Não foi criado arquivo aqui porque
não estava no escopo pedido nem no doc 17 §6.1 — fica registrado para avaliação.

Relacionado: os códigos residuais de `situacao_do_cliente` (`S`, `F`, `O`, `I` — juntos 1,27% da
base de `GE_Pessoa.Status`) não têm mapeamento confirmado para `CLIENTE_INATIVO`/`ENCERRADA`
(ver o item de cada um em `situacao_do_cliente.json`). Confirmar com quem conhece o Vórtice de
perto o que cada letra residual significa na prática.

---

## 6. Achado sobre fase — três granularidades diferentes, não reconciliadas

`fase.json` usa as 6 colunas do kanban do protótipo (`pipeline-fases.json`). Só que existem
**outras duas** representações de fase de oportunidade, todas reais, nenhuma delas igual:

1. **Kanban (6 fases)** — protótipo, `pipeline-fases.json`: Qualificação, Diagnóstico, Proposta,
   Negociação, Fechamento, Ganho/Perdido.
2. **Timeline da Ficha de Oportunidade (8 fases)** — protótipo,
   `oportunidade-1517613.json.timeline_fases`: Projeto, Orçamento, Proposta, Negócio Fechado,
   Pedido Alocado, Processo Liberado, Faturamento Liberado, Faturado.
3. **Fluxo real do Vórtice (17 fases)** — legado, `IV_ProcFase.csv`, `CodProcesso=50` ("Venda
   Equipamento Tracbel Agro", o processo ativo mais próximo da Oportunidade): Qualificação,
   Monitoramento, Negociação, Visita à Fábrica, Demonstração, Cliente Referência, Pedido de Venda,
   Proposta Apresentada, Apresentação, Montagem, Análise, Aprovação, Formalização, Autorização,
   Registrando Cédula, Preparação, Recebimento, Faturamento, Entrega, Pós-Entrega, Finalizado
   (21 linhas no CSV, algumas com a mesma fase entre `CodProcesso` 41 e 50).

As três descrevem o mesmo fluxo de negócio em zoom diferente, e nenhuma bate 1:1 com as outras
(ex.: "Fechamento" do kanban parece cobrir "Formalização"+"Autorização"+"Registrando Cédula" da
granularidade fina; "Faturado" da timeline de 8 parece ser o mesmo momento que "Faturamento"+
"Entrega" da granularidade fina).
**Pergunta objetiva:** o modelo novo deveria adotar a granularidade fina (17, mais fiel ao
processo real, mas mais pesada para o kanban) e tratar as 6/8 como **agrupamento visual**
(atributo `GrupoVisual` na fase), ou manter as 6 do kanban como a fase "oficial" e tratar o
detalhamento de 17 como sub-etapas dentro de "Fechamento"? Esta decisão pertence a quem desenhar
`processo.Fase` (fora do escopo desta tarefa, que é só o dado).

---

## 7. Outros achados menores (não bloqueiam, ficam registrados)

- **`GE_Empresa` tem duas empresas "Colorado"** (NroEmpresa 6 "Colorado Desativado" e 891
  "Colorado Equipamentos"), ambas `EmUso=0`, e a primeira divide a Sigla `CFRA` com a filial ativa
  de Franca (NroEmpresa 11) — resolvido em `empresa.json` dando o código limpo `CFRA` à filial
  ativa e `CFRA_COLORADO_6` à inativa, mas confirmar se a marca/empresa Colorado precisa
  reativação em algum cenário.
- **`IV_CodProcesso` (CodProcesso 22)** tem a descrição "Análise de **Cédito**" — parece erro de
  digitação de "Crédito" no próprio legado. Preservado literalmente em `tipo_de_processo.json`
  (regra de não inventar/corrigir dado de origem), com nota em `observacao`.
- **`DMN_DocTp`** tinha duas linhas idênticas (`SeqDocTp` 58 e 59, ambas código e descrição
  "CONTRATO") — unificadas num único item em `tipo_de_documento.json`, com os dois `SeqDocTp`
  citados em `observacao`.
- **Candidatos a catálogo fora do escopo desta rodada**, encontrados no protótipo mas não pedidos
  pelo doc 17 nem pela lista de arquivos desta tarefa: `segmento` (Agroindústria, Cana, Grãos,
  Pecuária — em `clientes-extra.json`/`clientes-outras.json`) e `porte` (Pequeno, Médio, Grande,
  Corporate — com inconsistência de caixa, "GRANDE" maiúsculo em alguns registros e "Grande" em
  outros). Não foi criado arquivo para nenhum dos dois; ficam aqui para quando entrarem no escopo.
- **Dois catálogos de sistema nasceram sem arquivo de dados (04/09/2026).** A migração
  `ChaveCompostaDeCatalogoEDominioDeEntidade` semeou os oito catálogos que o **esquema**
  referencia (documento 21, achado I-1). Seis já têm arquivo aqui — `papel_de_contato`,
  `origem_de_lead`, `cultura`, `tipo_de_documento`, `condicao_de_pagamento`, `concorrente`.
  Os outros dois, **`MOTIVO_INATIVACAO`** (`comercial.Cliente.MotivoInativacaoId`) e
  **`MOTIVO_DESCARTE`** (`comercial.Lead.MotivoDescarteId`), existem como catálogo vazio: a
  coluna é anulável, então nada quebra, mas enquanto não houver item a tela não tem o que
  oferecer. **Pergunta objetiva:** qual é a lista de motivos de inativação de cliente e a de
  descarte de lead que a Tracbel usa hoje? **Depende de:** Gestor Regional/Admin Comercial.
- **CEN responsável não virou catálogo aqui.** Doc 17 §6.1 lista "CEN responsável" como campo que
  puxa de `seguranca.Usuario` — mas isso é cadastro de usuário/pessoa (dado mestre com CPF, e-mail,
  papel de acesso), não uma lista de referência estática como as demais 20. O protótipo já tem
  `cens.json` (8 CENs) e `config-usuarios.json` (12 usuários) prontos, mas são de outra frente
  (a que está montando `seguranca.Usuario`) — não duplicado aqui para não gerar duas fontes da
  verdade para o mesmo dado.

---

## Resumo para quem for decidir

| # | Catálogo | Pendência | Decide quem |
|---|---|---|---|
| 1 | `familia` | Exportar `EXT_VeicFam` ou definir série comercial nova | TI/Vórtice + Comercial |
| 2 | `condicao_de_pagamento` | Exportar `EXT_CONDPAGTO` (21 linhas) e triar em uso | TI/Vórtice + Financeiro |
| 3 | `resultado` | Definir vocabulário universal de desfecho por tipo de tarefa | Gestor Regional/Admin Comercial |
| 4 | `marca` | Confirmar lista e status da Colorado | TI/Vórtice + Comercial |
| 5 | `concorrente` | Lista real de concorrentes acompanhados | Diretoria Comercial |
| 6 | `praca` + `cultura` | Geografia real (SP legado vs. MT/GO/BA protótipo) | **Ricardo** |
| 7 | `linha_de_negocio` | `IVS_Depto` é outra coisa — confirmar se precisa de 2º catálogo | Ricardo/quem escreveu doc 17 |
| 8 | `situacao_do_cliente` | Mapear códigos residuais `S`/`F`/`O`/`I` | Quem conhece o Vórtice de perto |
| 9 | `classe_de_cliente` / `GE_PessoaClasse` | Decidir se "tipo de relacionamento" (FILIAL/FUNCIONÁRIO/…) vira catálogo à parte | Ricardo |
| 10 | `fase` | Granularidade oficial (6 vs. 8 vs. 17) | Quem desenhar `processo.Fase` |
