# API do CRM — cadastro próprio e ponte de leitura do Vórtice

> **Documento 23** · Versão 1.0 · 04/09/2026
> Cobre `src/Tracbel.Crm.Api`, `src/Tracbel.Crm.Aplicacao` e `src/Tracbel.Crm.Integracao/Vortice`.
> É o passo 1 de três: a fundação sobre a qual o front (passo 2) é construído.

---

## 1. As duas capacidades, e por que elas são separadas

A API faz **duas coisas diferentes**, e a separação está no caminho da URL de propósito:

| Prefixo | O que é | Quem é dono do dado | Escreve? |
|---|---|---|---|
| `/api/v1/clientes`, `/api/v1/equipamentos`, `/api/v1/catalogos` | O **nosso cadastro**, no banco `TracbelCrm` | nós | sim |
| `/api/v1/legado/...` | Uma **ponte de leitura** do Vórtice | o Vórtice | **nunca** |

**Por que não misturar.** A origem do dado muda o que ele significa. O que sai da ponte é uma
*fotografia* de um sistema que ninguém aqui mantém, lida sem gravar nada — e o front precisa
marcar isso na tela. Um cliente do nosso banco e um cliente do legado no mesmo endpoint seriam
indistinguíveis na hora de decidir se dá para confiar.

**Toda resposta de leitura carrega procedência**, nos dois casos, no mesmo formato:

```json
"procedencia": {
  "sistema": "CRM Tracbel",
  "objeto": "comercial.Cliente",
  "lidoEmUtc": "2026-09-04T22:44:55.3856309Z",
  "dadoMaisRecenteEm": null,
  "estaDesatualizado": false,
  "aviso": null
}
```

Marcar a própria casa parece redundante e não é: se só o legado viesse carimbado, o front teria de
deduzir a origem do resto pela **ausência** de carimbo — e dedução é o que esse campo existe para
eliminar. `[V]` É a resposta direta aos 17 meses de faturamento parado passando por atual.

---

## 2. As rotas

Todas exigem os dois cabeçalhos de contexto de acesso da seção 4 — **exceto** `/saude/banco`
(seção 2.7) e as três rotas de sessão (`/auth/eu`, `/auth/entrar`, `/auth/sair`), que ficam fora do
meio de campo que lê os cabeçalhos.

> **São 39 rotas, e esta seção descreve 28 delas.** O inventário completo — rota por rota, com
> arquivo e linha, caso de uso, tabelas, permissão (hoje: **nenhuma**, nas 39), teste, tela e a fase
> que altera cada uma — está no **[documento 23A](23A-MATRIZ-DAS-ROTAS.md)** (issue #3, levantado em
> 19/09/2026). Lá também estão as onze rotas que faltam aqui e as duas divergências de conteúdo
> encontradas nesta seção. Trazer tudo isso para cá é a #4.

### 2.1 Cliente — `/api/v1/clientes`

| Verbo | Rota | O que faz | Sucesso |
|---|---|---|---|
| `GET` | `/api/v1/clientes` | Lista com paginação, filtro e ordenação | 200 |
| `GET` | `/api/v1/clientes/{chave}` | A ficha completa | 200 |
| `POST` | `/api/v1/clientes` | Cadastra na filial do contexto | 201 + `Location` |
| `PUT` | `/api/v1/clientes/{chave}` | Altera | 200 |
| `DELETE` | `/api/v1/clientes/{chave}` | **Inativa** — exclusão lógica, com motivo de catálogo | 200 |

**Parâmetros da listagem:** `pagina` (começa em 1), `tamanho` (padrão 25, **teto 200**), `termo`,
`situacao`, `tipoDePessoa`, `ordenarPor` (`Nome`, `CriadoEm`, `Situacao`, `AlteradoEm`),
`descendente`, `incluirInativos`.

`ordenarPor` é **domínio fechado**, não nome de coluna: o que não está na lista é recusado com as
opções na mensagem. Ordenação que chega como texto vira nome de coluna concatenado em SQL, ou um
`switch` com um `default` silencioso que ordena por outra coisa e ninguém percebe.

O `DELETE` **leva corpo** — `{ "motivoCodigo": "...", "versao": "..." }`. O motivo é obrigatório e
vem do catálogo `MOTIVO_INATIVACAO`; pôr o motivo na *query string* colocaria dado de negócio no
log do servidor web e na barra do navegador.

### 2.2 Equipamento — `/api/v1/equipamentos`

Mesmas cinco operações. Parâmetros da listagem: `pagina`, `tamanho`, `termo`, `situacao`,
`origem`, **`clienteChave`** (é o que a Visão 360 usa), `ordenarPor` (`Chassi`, `CriadoEm`,
`Situacao`, `AnoModelo`), `descendente`, `incluirInativos`.

**Chassi e origem não entram no `PUT`.** O chassi é a identidade da máquina e a chave de
deduplicação; a origem diz **quem afirma que a máquina existe** — o ERP (`Protheus`) ou o CEN
(`Crm`) —, e é justamente a informação que a tela de Cobertura usa. Nenhum dos dois muda por
edição de tela. Chassi digitado errado se corrige inativando o registro e cadastrando o certo,
para o histórico não mudar de dono em silêncio.

### 2.3 Catálogos — `/api/v1/catalogos`

| Verbo | Rota | O que faz |
|---|---|---|
| `GET` | `/api/v1/catalogos` | Todas as listas de seleção |
| `GET` | `/api/v1/catalogos/{codigo}` | Uma lista |

É o endpoint que torna **exequível** a regra mais repetida do documento 16, seção 3.1: *campo com
catálogo não aceita digitação livre*. A tela só consegue cumprir a parte dela se tiver de onde
puxar as opções. Sem isto, a alternativa real é o front embutir listas próprias — e listas
embutidas divergem.

**Duas famílias, e a resposta declara qual é qual:**

| Família | Exemplos | `permiteItemNovo` | Acrescentar item |
|---|---|---|---|
| Catálogo de banco | `ORIGEM_LEAD`, `MOTIVO_INATIVACAO`, `CULTURA`, `TIPO_DOCUMENTO`, `CONCORRENTE`, `MODELO_EQUIPAMENTO` | `true` | linha nova, sem release |
| Domínio fechado de código | `SITUACAO_CLIENTE`, `TIPO_DE_PESSOA`, `SITUACAO_EQUIPAMENTO`, `ORIGEM_EQUIPAMENTO` | `false` | exige release **e** migração |
| Filiais | `EMPRESA` | `false` | é a lista oficial de operação |

### 2.4 Ponte do Vórtice — `/api/v1/legado` (somente leitura)

| Verbo | Rota | O que faz |
|---|---|---|
| `GET` | `/api/v1/legado/saude` | Se a ponte responde, e quais objetos do legado estão vivos e quais estão parados |
| `GET` | `/api/v1/legado/clientes?termo=&limite=` | Busca clientes no Vórtice (mínimo 3 caracteres, teto 100) |
| `GET` | `/api/v1/legado/clientes/{identificador}/parque` | O parque de máquinas do cliente |

### 2.5 Relacionamento — processo, tarefa, interação e cobertura (somente leitura)

> Acrescentado em 05/09/2026, junto com a carga do [documento 25](25-CARGA-PROCESSO-AGENDA-CARTEIRA.md).
> São as rotas que fazem as telas de Pipeline, Agenda, Cobertura e Visão 360 mostrarem **número
> real** em vez de exemplo.

**São todas de leitura, e isso é uma decisão, não uma etapa faltando.** A escrita de processo e de
tarefa carrega o motor de regras — mudar de fase dispara automação, concluir tarefa gera a
próxima —, e ele entra junto com a tela que o exercita.

| Verbo | Rota | O que faz | O que sustenta |
|---|---|---|---|
| `GET` | `/api/v1/processos` | Lista com paginação, filtro e ordenação | Pipeline, Funil, 360 |
| `GET` | `/api/v1/processos/{chave}` | A ficha da oportunidade | ficha de oportunidade |
| `GET` | `/api/v1/tarefas` | A agenda, com prazo e atraso calculados | Agenda do CEN |
| `GET` | `/api/v1/interacoes` | A linha do tempo de contatos | Visão 360 |
| `GET` | `/api/v1/cobertura` | A carteira cliente a cliente, com a data do último contato | Cobertura |
| `GET` | `/api/v1/relatorios/funil` | O funil por fase, **agrupado no banco** | Pipeline, Funil |
| `GET` | `/api/v1/relatorios/perdas` | As perdas por motivo | relatório de perdas |
| `GET` | `/api/v1/relatorios/agenda` | O painel do CEN | painel inicial |
| `GET` | `/api/v1/relatorios/cobertura` | A cobertura por carteira | Cobertura |

**Parâmetros de `/processos`:** `pagina`, `tamanho` (teto 200), `termo` (título ou número),
`situacao` (`Aberto`, `Suspenso`, `Ganho`, `Perdido`, `Cancelado`), `clienteChave`, `faseCodigo`,
`tipoProcessoCodigo`, `ordenarPor` (`Numero`, `Titulo`, `ValorEstimado`, `PrevisaoConclusao`,
`FaseDesde`, `CriadoEm`), `descendente`, `incluirEncerrados`.

**Parâmetros de `/tarefas`:** `pagina`, `tamanho`, **`minhas`** (usa o usuário do contexto de
acesso), `situacao` (`Pendente`, `EmAndamento`, `Concluida`, `Cancelada`, `Reatribuida`),
`clienteChave`, `de`, `ate`, **`somenteAtrasadas`**, `ordenarPor` (`AgendadaPara`, `Prioridade`,
`PrazoLimite`, `Assunto`), `descendente`.

**Parâmetros de `/interacoes`:** `pagina`, `tamanho`, `clienteChave`, `natureza` (`Ativa`,
`Receptiva`, `Sistema`), `de`, `ate`. A ordem é sempre a mais recente primeiro — é o índice de
`(ClienteId, OcorridaEm DESC)`, o mais importante do modelo.

**Parâmetros de `/cobertura`:** `pagina`, `tamanho`, `classe` (`A`, `B`, `C`, `D`),
**`diasSemContato`**, **`somenteSemContato`**, `ordenarPor` (`UltimaInteracaoEm`, `Classe`,
`Nome`), `descendente`. A ordem padrão é **quem está há mais tempo sem contato primeiro**, e o
nunca-contatado vem antes de todos.

`/relatorios/agenda` aceita `minhas`. Os demais agregados não têm parâmetro: devolvem tudo o que
está ao alcance do contexto de acesso, e isso já é a filial.

**A `natureza` da interação separa três coisas que a linha do tempo do legado mostra
misturadas** — nós procuramos o cliente (`Ativa`), o cliente nos procurou (`Receptiva`) e o
servidor carimbou um evento (`Sistema`, 30.747 das 121.983 interações carregadas). `[V]` Um quarto
do "histórico de relacionamento" do legado não é relacionamento, e lá isso aparece sem nenhuma
distinção visual.

#### `metricasSemDado` — a resposta quando o dado não sustenta a métrica

**Todo agregado devolve duas listas:** `itens` e `metricasSemDado`. A segunda é o que diferencia
esta API da tela do legado — ela diz, com o número medido na mesma consulta, o que a tela **não**
pode afirmar:

```json
{
  "dados": {
    "itens": [ { "faseCodigo": "APRESENTACAO", "faseNome": "Apresentacao", "faseOrdem": 5,
                 "processos": 6201, "processosComValor": 4, "valorTotal": 1290000.00 } ],
    "metricasSemDado": [
      { "metrica": "valorDoFunilConfiavel",
        "motivo": "So 9 de 16028 processos abertos declaram valor (0,1%). O total vem preenchido,
                   mas ele representa essa fracao - nao o funil inteiro." }
    ]
  },
  "procedencia": { "sistema": "CRM Tracbel", "objeto": "processo.Processo", "lidoEmUtc": "..." }
}
```

**`valorTotal` vem `null`, e não `0`, quando nenhum processo da fase declara valor.** Zero diria
"o funil vale nada"; o que se sabe é outra coisa — que ninguém preencheu. Mesma regra em
`estaForaDoCiclo` da cobertura: nulo quando não há cadência declarada, porque `false` diria "está
em dia", e não é isso que se sabe.

`[V]` É a resposta direta aos 17 meses de faturamento parado que passaram por atual porque a tela
mostrava um número sem dizer de onde ele vinha. A lista completa das sete métricas do protótipo
que hoje não têm dado está na seção 9 do
[documento 25](25-CARGA-PROCESSO-AGENDA-CARTEIRA.md).

#### O agregado é calculado no banco, e a fronteira vale nele

O funil, as perdas, o painel da agenda e a cobertura por carteira saem de um `GROUP BY` — a tela
recebe dez linhas, não 45 mil processos para somar no navegador. E o `GROUP BY` roda **dentro** do
filtro global de empresa: `[V]` é exatamente o ponto onde o legado mais vaza, com 125 dos 134
relatórios sem predicado de usuário nenhum.

Exercitado ao vivo contra o SQL Server real, com a carga do documento 25 dentro:

```
                                          010101     010103
GET /api/v1/processos                      16.035      1.867
GET /api/v1/tarefas                        32.975      4.436
GET /api/v1/cobertura                      15.104      5.128
GET /api/v1/relatorios/cobertura         41 linhas  13 linhas
GET /api/v1/processos/{chave de RP}        200 OK        404
GET /api/v1/interacoes?clienteChave={cliente de RP}
                                           200 (20)      404
```

### 2.6 Território — município e cobertura por filial e por carteira (somente leitura)

> Acrescentado em 05/09/2026, junto com o [documento 26](26-CARTEIRA-E-MUNICIPIO.md).
> São as rotas do agrupamento que **existe no dado** — filial e carteira. **Não há rota de
> regional**, e não é omissão: `IVS_Regional` existe no Vórtice e tem **zero linhas**. As sete
> regionais que a tela de Cobertura mostra hoje vieram do protótipo e não têm lastro nenhum.

| Verbo | Rota | O que faz | O que sustenta |
|---|---|---|---|
| `GET` | `/api/v1/municipios` | Busca município por começo do nome, com filtro de UF | o **campo de município** do endereço |
| `GET` | `/api/v1/cobertura/filiais` | A cobertura por filial: carteiras, municípios e estados | Cobertura, nível 1 |
| `GET` | `/api/v1/cobertura/carteiras` | O território de cada carteira, com as cidades | Cobertura, nível 2 |

#### `/api/v1/municipios` — o que fecha o campo de texto livre

**É a rota que torna a regra exequível.** O [documento 16, seção 3](16-HIGIENIZACAO-DE-DADOS.md)
diz que campo com catálogo não aceita digitação livre; sem uma rota que entregue a lista, a única
alternativa real da tela é aceitar digitação. `comercial.Endereco.Municipio` era texto livre até
05/09/2026, e a carga precisou normalizar **275 municípios com espaço duplo** por causa disso.

**Parâmetros:** `termo`, `uf`, `pagina`, `tamanho`.

- **A busca é por PREFIXO.** `?termo=ribeir` encontra Ribeirão Preto; `?termo=eirão` não encontra
  nada. É decisão de desempenho declarada no contrato: busca por trecho não usa índice e varreria
  as 9.750 linhas a cada tecla.
- **Acento e caixa não importam** — `?termo=sao` encontra `São`, pela colação
  `Latin1_General_CI_AI`. O front não normaliza nada antes de mandar.
- **`uf` é validada contra as 27** e devolve `400` com o campo nomeado quando não é uma delas. UF
  inválida devolvendo lista vazia pareceria "não existe cidade" — resposta errada para a pergunta
  errada.
- **O formulário devolve o `id`**, nunca o nome.
- **Não existe rota de criação de município, e é decisão.** Este catálogo é a lista oficial do
  Brasil, não um campo que cresce com o que o usuário digitou — a diferença entre ele e os
  catálogos de `metadado.Catalogo`, que trazem `permiteItemNovo = true`. Município que não aparece
  é falta de carga de catálogo: um chamado, não um item novo criado pela tela.

```
GET /api/v1/municipios?uf=SP&termo=ribeir
{
  "dados": {
    "itens": [ { "id": 8091, "nome": "RIBEIRAO PRETO", "uf": "SP", "codigoIbge": null } ],
    "pagina": 1, "tamanho": 25, "total": 4
  },
  "procedencia": { "sistema": "CRM Tracbel", "objeto": "organizacao.Municipio", "lidoEmUtc": "..." }
}
```

`codigoIbge` vem `null` porque `GE_Cidade` **não tem essa coluna** — a identificação de hoje é por
nome mais UF, e o risco disso está medido no
[documento 26, seção 4.5](26-CARTEIRA-E-MUNICIPIO.md).

#### `/api/v1/cobertura/filiais` e `/api/v1/cobertura/carteiras`

`/filiais` não tem parâmetro. `/carteiras` aceita `empresaCodigo` para abrir uma filial, e devolve
`404` com o texto explicando quando o código não está ao alcance do contexto de acesso.

**Os dois declaram a lacuna em `metricasSemDado`**, no mesmo formato da seção 2.5: das 142
carteiras carregadas, **73 têm cidade e 69 não têm**, e a resposta diz isso com o número em vez de
mostrar uma cobertura menor sem explicar. **A carteira sem cidade aparece com a lista vazia** em vez
de sumir — escondê-la faria a tela mostrar uma operação menor do que ela é.

**A fronteira de multiempresa vale nas duas**, e chega pela carteira: as consultas partem de
`organizacao.Carteira`, que tem `EmpresaId` e o filtro global. `/municipios` é a única rota
deliberadamente **não** filtrada por filial — município é dado nacional, e o CEN de Ribeirão Preto
precisa poder cadastrar um cliente em Uberaba.

### 2.7 Vida

`GET /saude/banco` — não exige contexto de acesso. É o que o script de subida confere.

---

## 3. O formato de erro

Toda recusa é `application/problem+json`. O status vem da **natureza** da falha, declarada pelo
caso de uso em `Resultado<T>.Tipo` — a API não adivinha nada a partir do texto da mensagem.

| Natureza | Status | `type` | Quando |
|---|---|---|---|
| `Validacao` | **422** | `.../validacao` | a entrada não serve |
| `NaoEncontrado` | **404** | `.../nao-encontrado` | não existe **ou** não está ao seu alcance |
| `Conflito` | **409** | `.../conflito` | o estado atual não admite a operação |
| `Concorrencia` | **409** | `.../concorrencia` | alguém alterou antes; recarregue e refaça |
| `DependenciaIndisponivel` | **503** | `.../dependencia-indisponivel` | a ponte do legado não respondeu |

**422 e não 400:** o 400 diz *"não entendi a requisição"*; aqui ela foi entendida perfeitamente e o
**conteúdo** é que não passa. A distinção importa para quem depura.

**404 para "não é seu" também:** distinguir contaria a quem não pode ver que o registro existe.

Erro de entrada traz a lista `erros`, campo a campo — resposta real da API:

```json
{
  "type": "https://crm.tracbel.com.br/erros/validacao",
  "title": "O cadastro do cliente tem campos a corrigir.",
  "status": 422,
  "detail": "A requisição foi entendida, mas o conteúdo não passa nas regras. Veja a lista 'erros'.",
  "erros": [
    { "campo": "nomeRazao",
      "mensagem": "Informe a razão social ou o nome do cliente.",
      "valorRecebido": "   " },
    { "campo": "documento",
      "mensagem": "Um CPF pertence a pessoa física. Troque o tipo de pessoa ou informe um CNPJ.",
      "valorRecebido": "529.982.247-25" },
    { "campo": "origemCodigo",
      "mensagem": "Origem fora do catálogo ORIGEM_LEAD. Consulte /api/v1/catalogos/ORIGEM_LEAD para ver as opções.",
      "valorRecebido": "VEIO_DO_NADA" }
  ]
}
```

Três erros de uma vez, e não o primeiro: recusar campo a campo faria o usuário corrigir, reenviar
e descobrir o próximo — o mesmo trabalho repartido em três viagens.

`[V]` A API do Vórtice devolve `{"Message":"An error has occurred."}` tanto para um telefone com
DDI quanto para uma falha real de infraestrutura. É a diferença que este formato existe para fazer.

### 3.1 Concorrência otimista

O `GET` da ficha devolve `versao` (o `rowversion` em base64). O `PUT` e o `DELETE` a devolvem; se
o registro tiver mudado, a resposta é **409 `/concorrencia`**.

São **duas camadas**, e elas somam:

1. **A versão que o cliente leu**, conferida em `EntidadeBase.VersaoConfere` — protege a janela
   longa: o usuário abre a tela às 9h, o colega salva às 9h05, o primeiro salva às 9h10.
2. **O `rowversion` do SQL Server**, no `UPDATE` — protege a corrida entre duas requisições
   simultâneas; a `DbUpdateConcurrencyException` é traduzida em `Resultado` pela unidade de
   trabalho, nunca vira 500.

Quem **não** manda `versao` dispensa a primeira camada. É concessão declarada: um cliente de
integração que não guarda versão precisa continuar gravando, e o que ele perde é a proteção.

---

## 4. O contexto de acesso — como funciona hoje e como vai funcionar

### 4.1 Hoje: dois cabeçalhos. **Isto não autentica ninguém.**

```
X-Tracbel-Usuario: cen.ribeiraopreto@tracbel.com.br
X-Tracbel-Empresa: 010101
```

`MeioDeCampoDeContextoDeAcesso` resolve os dois contra `seguranca.Usuario` e
`organizacao.Empresa` e monta o `ContextoAcesso` — o **mesmo objeto** que a autenticação real vai
montar. Cabeçalho é uma coisa que qualquer um escreve: isto serve para o front ser construído
contra dado real e para a fronteira de multiempresa ser exercitada de verdade, **não** para
proteger nada.

**As três amarras que impedem isso de virar permanente por descuido:**

1. **Fora de Desenvolvimento não existe valor padrão.** Sem cabeçalho, a requisição é recusada — a
   API nunca "assume alguém". `Program.cs` liga `PermitirPadrao` pelo ambiente, e só lá.
2. **Todo uso do padrão sai no log como aviso**, com a palavra `PROVISÓRIO`; a subida da API loga
   o mesmo aviso uma vez.
3. **As profundidades param em `EmpresaEAbaixo`**, e a permissão que abre a fronteira entre
   filiais (`Empresa.AlcanceEntreFiliais`) **não é concedida**. Esta ponte não entrega alcance de
   organização nem por engano.

A ordem importa: o meio de campo roda **antes** de qualquer endpoint resolver o `CrmDbContext`,
porque o filtro global é pré-computado no construtor dele. `ContextoAcessoDaRequisicao` **lança**
se alguém o ler antes da hora, em vez de devolver um contexto vazio — contexto vazio silencioso
faria as consultas filtrarem por empresa 0 e devolverem nada, e alguém gastaria horas procurando o
dado que "sumiu".

### 4.2 Amanhã: Entra ID

Nasce um `ResolvedorDeContextoDoEntraId` em `Infraestrutura/Identidade/`, lendo as reivindicações
do token validado em vez dos cabeçalhos, e o registro no `Program.cs` troca de nome.
**Nada além disso muda:** o filtro global, as profundidades, a via de escape e todos os casos de
uso já consomem do `ContextoAcesso`, que é o objeto definitivo.

### 4.3 A fronteira de multiempresa, provada

A filial **não vem do corpo da requisição** — vem do contexto. Aceitar `EmpresaId` no JSON abriria
um caminho para gravar na filial de outro, e a fronteira valeria só para a leitura.

Prova exercitada contra o SQL Server real (o mesmo registro, a mesma chave, a mesma API):

```
GET /api/v1/clientes/dee8b41c-38f9-4222-91fd-a088aa667178
  X-Tracbel-Empresa: 010101  → 200 OK
  X-Tracbel-Empresa: 010103  → 404 {"type":".../nao-encontrado", ...}

GET /api/v1/clientes?termo=Santa
  X-Tracbel-Empresa: 010103  → 200 {"dados":{"itens":[],"total":0}}
```

Vale para a **escrita** também: `PUT` e `DELETE` no contexto da outra filial respondem 404.

---

## 5. Onde cada coisa mora

```
Dominio/Portas/           IRepositorioClientes, IRepositorioEquipamentos,
                          IRepositorioCatalogos, IUnidadeDeTrabalho,
                          IPonteDeLeituraDoVortice        ← o que o domínio EXIGE
Aplicacao/Clientes/       ListarClientes, ObterCliente, CriarCliente,
                          AlterarCliente, InativarCliente ← os casos de uso
Aplicacao/Equipamentos/   idem para máquina
Aplicacao/Catalogos/      ListarCatalogos
Aplicacao/Legado/         BuscarClientesNoLegado, ListarParqueNoLegado, VerificarPonteDoLegado
Infraestrutura/Persistencia/Repositorios/   os adaptadores sobre o CrmDbContext
Infraestrutura/Identidade/                  o contexto de acesso provisório
Integracao/Vortice/       PonteDeLeituraDoVortice, SaneamentoDoLegado  ← a quarentena
Api/Endpoints/            as rotas: recebem, delegam, traduzem
Api/Comum/                RespostaDeErro, MeioDeCampoDeContextoDeAcesso
```

**Uma seta nova no grafo de referências:** `Tracbel.Crm.Api` passou a referenciar
`Tracbel.Crm.Integracao`. A seta aponta para dentro (Integracao depende só do Domínio) e existe
pela mesma razão da referência a Infraestrutura: **a Api é a raiz de composição**, o único lugar
autorizado a conhecer adaptadores, porque é ele que os liga às portas no boot. O caso de uso
continua conhecendo só `IPonteDeLeituraDoVortice`. Registrado no grafo permitido de
`SolidTestes.As_referencias_de_projeto_apontam_sempre_para_dentro`.

---

## 6. A ponte do Vórtice, em detalhe

### 6.1 O que está vivo e o que está morto

Esta tabela é conhecimento caro e some se não ficar escrito. As tabelas paradas **respondem à
consulta**, têm centenas de milhares de linhas e **parecem** disponíveis.

| Objeto | Situação (medida em 04/09/2026) | A ponte lê? |
|---|---|---|
| `GE_Pessoa` | vivo — cadastro de pessoas, alterado diariamente | **sim** |
| `IV_ClientePropr` | vivo — parque de máquinas do CEN, **alterado hoje** | **sim** |
| `IV_Processo`, `IV_Agenda`, `IV_Historico` | vivos | **não pela ponte** — quem os lê é a carga do [documento 25](25-CARGA-PROCESSO-AGENDA-CARTEIRA.md) |
| `GE_Usuario`, `IVS_Carteira`, `IVS_Pes`, `IVS_Depto`, `IV_VENDEDOR` | vivos | **idem** — só a carga, e **nunca** a coluna de senha |
| `EXT_Veic` | **PARADO desde 24/05/2024** — frota do ERP | **não** |
| `EXT_NFS` | **PARADO desde 11/04/2025** — faturamento | **não** |
| `EXT_OS` | **PARADO** — ordens de serviço nunca promovidas | **não** |
| `EXT_Titulo` | **PARADO** — títulos presos em staging desde 05/2025 | **não** |

A lista vive em `PonteDeLeituraDoVortice.ObjetosDoLegado`, sai no endpoint de saúde e é vigiada por
teste — se alguém apagar uma linha achando que é comentário morto, o teste avisa.

**Medição ao vivo que fundamenta a escolha do parque de máquinas** (via o conector do agente do
Vórtice, 04/09/2026): das 45 categorias de `IV_ClientePropr`, a de **Trator** tem 25.477 linhas com
última alteração em **04/09/2026 16:36** — hoje. Colhedora de Cana, Plantadeira, Agricultura de
Precisão, Pulverizador e Colheitadeira de Grãos foram alteradas em 01 e 02/09/2026. As categorias
de numeração antiga (`NO-*`) pararam entre 2009 e 2018 e ficam de fora.

### 6.2 O saneamento aplicado na leitura, e o que ele corrige

**O defeito de origem, medido:** `GE_Pessoa` guarda o documento em **duas colunas numéricas** — a
base em `NroCGCCPF` e os dígitos verificadores em `DigCGCCPF`. Coluna numérica não preserva zero à
esquerda. Medição ao vivo em 04/09/2026:

> **51.523 de 71.303 pessoas jurídicas (72,3%)** têm o CNPJ gravado sem os zeros que a raiz+filial
> de 12 dígitos exige.

**O que a ponte faz:** devolve os zeros ao lugar. A base é preenchida à esquerda até 12 dígitos
(jurídica) ou 9 (física), o verificador até 2, e as partes são coladas.

| Campo | O que é normalizado | O que acontece quando não dá |
|---|---|---|
| `documento` | zeros à esquerda recompostos; dígito verificador conferido | sai com `documentoConfere: false` e o ajuste registrado — **nunca** como se fosse válido |
| `telefone` | DDD + número, sem DDI, pelo tipo `Telefone` | omitido, com o motivo |
| `email` | espaço e caixa, pelo tipo `Email` | omitido, com o motivo |
| `nomeRazao`, `cidade`, `uf` | espaço colapsado e aparado | — |
| `ano` (parque) | texto livre virando número, faixa 1900–2100 | omitido, com o motivo |
| `situacaoNoLegado` | **não é traduzida** além de `A` e `P` | a letra crua sai marcada como sem significado documentado |

**Toda correção sai junto do dado**, no campo `ajustes` de cada linha — princípio 1.4 do documento
16. Nada é corrigido em silêncio, e nada é chutado: recompor é reversível e verificável; adivinhar
o dígito certo não é.

### 6.3 Credencial e indisponibilidade

A cadeia de conexão vem da variável de ambiente **`Vortice__Conexao`** e **não existe em arquivo
nenhum do repositório**. Sem ela, a ponte responde 503 dizendo como configurá-la.

A ponte exige VPN e **pode estar fora do ar**. Quando está, os três endpoints de `/api/v1/legado`
respondem **503** e **nenhum outro endpoint é afetado** — verificado por teste e exercitado ao
vivo:

```
GET /api/v1/legado/saude → 503
{"type":"https://crm.tracbel.com.br/erros/dependencia-indisponivel",
 "title":"O sistema legado não respondeu. Quase sempre é a VPN: confira se ela está conectada e
          tente de novo. O cadastro do CRM não depende desta ponte e continua funcionando
          normalmente.",
 "status":503}

GET /api/v1/clientes   → 200
GET /api/v1/catalogos  → 200
```

O detalhe técnico (nome do servidor, exceção do driver) vai para o **log**, nunca para a resposta.

**Estado da verificação da ponte em 04/09/2026.** A VPN esteve de pé no começo da sessão — foi com
ela que as medições da seção 6.1 e 6.2 foram feitas, pelo conector do agente do Vórtice
(`vortice-crm-agent/connect.ps1`) — e **caiu antes de a ponte da API poder ser exercitada ao vivo**.
O que ficou provado, e o que não:

| Item | Estado |
|---|---|
| a ponte devolve 503 com frase acionável quando o legado não responde | **exercitado ao vivo e coberto por teste** |
| o cadastro continua respondendo 200 com a ponte caída | **exercitado ao vivo e coberto por teste** |
| a recomposição do documento e a normalização de telefone/e-mail | **coberto por teste**, sem depender de rede |
| a lista de objetos vivos e parados | **medida ao vivo**; guardada por teste |
| uma leitura de cliente e de parque **pela rota da API** | **pendente** — depende da VPN voltar |

O erro devolvido enquanto a VPN esteve fora, exatamente como o endpoint o entrega:

```
GET /api/v1/legado/saude → 503
{"type":"https://crm.tracbel.com.br/erros/dependencia-indisponivel",
 "title":"O sistema legado não respondeu. Quase sempre é a VPN: confira se ela está conectada e
          tente de novo. O cadastro do CRM não depende desta ponte e continua funcionando
          normalmente.",
 "status":503,
 "detail":"Um sistema externo não respondeu. O restante da API continua funcionando."}
```

---

## 7. Dados de referência

A migração `ModeloInicial` já semeia os **oito catálogos de sistema** (`metadado.Catalogo`), porque
o `Id` deles é parte do esquema. O que faltava — filiais, itens de catálogo e catálogo de frota —
entra por **seed versionado e idempotente**, no gancho que `subir-banco.ps1` já procurava:

```
scripts/banco/rodar-seed.ps1                      # o gancho
scripts/banco/seed/01-dados-de-referencia.sql     # gerado de dados-referencia/*.json
scripts/banco/seed/02-usuarios-de-desenvolvimento.sql  # andaime, só com -ComUsuariosDeDesenvolvimento
```

Aplicado no banco de desenvolvimento em 04/09/2026:

| Tabela | Linhas | Ativas |
|---|---:|---:|
| `organizacao.Empresa` | 18 | **13** |
| `metadado.CatalogoItem` | 166 | 166 |
| `frota.Marca` | 7 | 7 |
| `frota.Familia` | 7 | 7 |
| `frota.Modelo` | 18 | 18 |

**As 13 filiais em operação** entram ativas; as demais entram com `EstaAtiva = 0`. O legado marca
algumas delas como ativas e o negócio não as confirmou — tratá-las como operacionais faria a API
oferecer, no seletor de filial, uma filial onde ninguém trabalha. Elas ficam no banco (não se apaga
filial: o histórico aponta para ela) e fora da lista de seleção, que só devolve ativas.

A hierarquia é **plana**: `Caminho = '/'` e `Nivel = 0` em todas. As filiais são irmãs, e por isso
"esta empresa e as abaixo" resolve para a própria — que é exatamente o isolamento que a fronteira
deve dar hoje.

---

## 8. A dívida, nomeada

| # | O que é | Por que ficou assim | O que fazer |
|---|---|---|---|
| **D-1** | **A identidade vem de cabeçalho HTTP e não autentica ninguém** | a autenticação é a fase 0 do documento 13 e ainda não entrou | `ResolvedorDeContextoDoEntraId` em `Infraestrutura/Identidade/`; troca de **uma classe**. As três amarras da seção 4.1 seguram até lá |
| **D-2** | **Busca por *pedaço* de documento e de chassi não funciona** | `CpfCnpj` e `Chassi` são tipos de valor com conversor, e o EF aplica o conversor da propriedade **ao parâmetro** — um `LIKE '%1234%'` estoura com *"Invalid cast from String"*. Vale para `.Contains` e `EF.Functions.Like`, os dois exercitados | coluna de busca em texto puro, alimentada do mesmo valor, com índice próprio. Migração **aditiva** |
| **D-3** | **Paginação por `OFFSET`, e não por cursor** | o documento 03, seção 7, pede cursor; a tela aprovada tem numeração de páginas e contagem total, e nenhuma das duas tabelas desta fase tem volume que faça o `OFFSET` doer | quando `processo.Interacao` entrar — essa sim com milhões de linhas — nasce por cursor |
| **D-4** | **`MOTIVO_INATIVACAO` e `MOTIVO_DESCARTE` são listas provisórias** | não existe fonte: nem o legado nem o protótipo têm a lista (`dados-referencia/PENDENTES.md`) | pergunta objetiva ao negócio; o item `OUTRO` já nasce com observação obrigatória |
| **D-5** | **`frota.Familia` é uma linha "A confirmar" por marca** | `dados-referencia/familia.json` está vazio, e `frota.Modelo` exige uma família | substituir o bloco do seed quando a frente de dados de referência entregar; é dado, não exige migração |
| **D-6** | **Reatribuição de proprietário não existe** | o cliente nasce com o proprietário do contexto; trocar o dono é outra operação, com outra permissão | entra com a carteirização |
| **D-7** | **Nenhum endpoint aceita `Idempotency-Key`** | o documento 03, seção 7, prevê; nenhum consumidor de integração existe ainda | quando a primeira integração de entrada chegar, gravando em `intg.ChaveExterna` |
| **D-8** | **A ponte lê dois objetos do legado, não cinco** | cliente e parque de máquinas são o que o passo 1 precisa | **Resolvido de outro jeito, em 05/09/2026:** processo, agenda e histórico entraram pela CARGA (documento 25), não pela ponte — a tela lê do nosso banco, com 45.397 processos, 103.339 tarefas e 121.983 interações dentro. A ponte continua servindo à busca ao vivo de cliente e de parque |
| **D-9** | **Nenhuma rota de relacionamento escreve** | processo e tarefa carregam o motor de regras: mudar de fase dispara automação, concluir tarefa gera a próxima | entra junto com a tela que exercita o motor. A leitura, que é o que o front precisa agora, está pronta (seção 2.5) |
| **D-10** | **A soma de valor do funil é uma segunda consulta, e não um `SUM`** | é a dívida **D-2** num lugar novo: `SUM` sobre propriedade com conversor de valor não traduz para SQL (defeito nº 4 da seção 9.1) | a mesma correção aditiva de D-2 — coluna de valor em decimal puro ao lado do tipo de valor |

---

## 9. Verificação

**Build e suíte:** `dotnet build` sem aviso; `dotnet test` verde.

**Testes novos** (a base era 221):

| Projeto | O que cobre |
|---|---|
| `Tracbel.Crm.Aplicacao.Testes/Cadastro/` | os casos de uso contra banco de verdade (SQLite), com os **repositórios de verdade** — caminho feliz e caminho de recusa, mais a fronteira de empresa e a **prova negativa** |
| `Tracbel.Crm.Api.Testes` | a API inteira em memória (`WebApplicationFactory`): rota, status, formato de erro, contexto de acesso, ponte caída e a fronteira **por HTTP**, com prova negativa |
| `Tracbel.Crm.Integracao.Testes` | o saneamento do legado (recomposição de documento, telefone, e-mail) e a ponte recusando sem derrubar nada |
| `Tracbel.Crm.Dominio.Testes/Comum/ConcorrenciaOtimistaTestes` | a conferência de versão, sem banco |

**A prova negativa**, em dois lugares (caso de uso e API): a mesma consulta roda com
`IgnoreQueryFilters` e o registro da outra filial **aparece**. Sem ela, os testes de fronteira
seriam igualmente compatíveis com "o banco está vazio" — e teste que passa pelo motivo errado dá
segurança falsa.

E a prova foi **executada**: com a linha `AplicarFronteiraDeEmpresa(modelo)` comentada em
`CrmDbContext.OnModelCreating`, **14 testes de fronteira falham** — os 5 de API, os 5 de caso de
uso e os 4 de persistência que já existiam:

```
Failed  FronteiraDeEmpresaNaApiTestes.A_MESMA_requisicao_GET_devolve_200_numa_filial_e_404_na_outra
Failed  FronteiraDeEmpresaNaApiTestes.A_listagem_da_outra_filial_nao_traz_o_registro_nem_no_total
Failed  FronteiraDeEmpresaNaApiTestes.A_escrita_tambem_bate_na_fronteira_e_nao_so_a_leitura
Failed  FronteiraDeEmpresaNaApiTestes.O_equipamento_da_outra_filial_tambem_nao_aparece
Failed  FronteiraDeEmpresaNaApiTestes.Prova_negativa_sem_o_filtro_global_...
Failed  FronteiraDeEmpresaNoCasoDeUsoTestes.Obter_no_contexto_de_outra_filial_devolve_nao_encontrado
Failed  FronteiraDeEmpresaNoCasoDeUsoTestes.Listar_no_contexto_de_outra_filial_nao_traz_o_registro_nem_no_total
Failed  FronteiraDeEmpresaNoCasoDeUsoTestes.Alterar_e_inativar_de_outra_filial_tambem_batem_na_fronteira
Failed  FronteiraDeEmpresaNoCasoDeUsoTestes.O_equipamento_de_outra_filial_tambem_nao_aparece
Failed  FronteiraDeEmpresaNoCasoDeUsoTestes.Prova_negativa_sem_o_filtro_global_...
Failed  FronteiraDeEmpresaTestes.A_consulta_sem_where_nenhum_nao_devolve_o_cliente_de_outra_filial
Failed  FronteiraDeEmpresaTestes.Nem_o_Where_do_chamador_nem_o_Id_direto_furam_a_fronteira
Failed  FronteiraDeEmpresaTestes.A_fronteira_vira_clausula_SQL_e_nao_e_avaliada_em_memoria
Failed  FronteiraDeEmpresaTestes.O_administrador_so_atravessa_a_fronteira_depois_de_declarar_o_motivo
```

Restaurada a linha, a suíte volta a verde. É isso que separa "o teste passa" de "o teste prova".

### 9.1 Três defeitos reais encontrados **exercitando** a API

Nenhum dos três apareceria em teste de unidade de caso de uso:

1. **`MapDelete` não infere corpo.** Sem `[FromBody]`, a aplicação nem constrói a rota e devolve
   500 na primeira requisição a **qualquer** endpoint.
2. **`OrderBy(e => e.Chassi.Numero)` não traduz para SQL.** Acesso a membro dentro de um valor
   convertido estoura em tempo de execução.
3. **`.Contains` sobre propriedade com conversor** aplica o conversor ao parâmetro do `LIKE` —
   origem da dívida **D-2**.

4. **`SUM` sobre propriedade com conversor de valor não traduz** *(encontrado em 05/09/2026, com o
   funil de 45 mil processos)*. `GroupBy(...).Select(g => g.Sum(p => p.ValorEstimado!.Value.Valor))`
   compila e estoura em tempo de execução: acesso a membro dentro de um valor convertido não vira
   SQL. A correção **não** foi somar na tela — o agrupamento continua no banco, e a soma virou uma
   segunda consulta que o banco filtra por `ValorEstimado != null`, trazendo só as linhas que a
   soma alcança (0,8% dos processos, e o agrupamento já disse exatamente quantas são).

---

## 10. Como subir e exercitar

```powershell
./scripts/banco/subir-banco.ps1                              # container + migrations
./scripts/banco/rodar-seed.ps1 -ComUsuariosDeDesenvolvimento # referência + andaime

$env:ASPNETCORE_ENVIRONMENT = 'Development'
# Só se for usar a ponte do legado — exige VPN. NUNCA versione este valor.
$env:Vortice__Conexao = 'Server=<ip>,1433;Database=<base>;User Id=<login>;Password=<senha>;TrustServerCertificate=True;Encrypt=False;Connect Timeout=30'

dotnet run --project src/Tracbel.Crm.Api
```

```bash
curl -H "X-Tracbel-Usuario: cen.ribeiraopreto@tracbel.com.br" \
     -H "X-Tracbel-Empresa: 010101" \
     http://localhost:5145/api/v1/catalogos/ORIGEM_LEAD
```

Em Desenvolvimento os dois cabeçalhos podem ser omitidos — a API cai no padrão de
`appsettings.Development.json` e **loga o aviso `PROVISÓRIO`**.
