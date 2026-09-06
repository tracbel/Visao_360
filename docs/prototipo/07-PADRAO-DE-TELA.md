# O padrão de tela do CRM

> **Documento 07** · Versão 1.0 · 05/09/2026
> Escrito a partir das duas primeiras telas ligadas à API — **Clientes** e **Equipamentos**
> (`src/Tracbel.Crm.Web/src/telas/cadastro/`). É o que o passo 3 aplica nas demais.

Este documento **decide**, não descreve. Onde ele diz "é assim", a tela nova segue sem
reabrir a discussão; onde uma tela precisar divergir, a divergência entra aqui com o motivo.

A queixa que originou isto foi literal: *"o front está lindo mas bagunçado"*, e as três coisas
apontadas foram **telas demais mostrando a mesma coisa**, **falta de padrão entre as telas** e
**uma tela vazia com pontas soltas**. Este documento resolve a segunda.

---

## 1. As quatro telas que existem, e só elas

| Tela | Rota | O que é |
|---|---|---|
| **Lista** | `/clientes`, `/equipamentos` | busca, filtro, ordenação, paginação e a porta para as outras |
| **Ficha** | `/clientes/{chave}` | o registro em modo leitura, com as ações que ele admite |
| **Edição** | a MESMA rota da ficha, em modo `editando` | os mesmos campos, liberados |
| **Cadastro** | `/clientes/novo` | os mesmos campos, vazios |

**Ficha e edição são a mesma tela, em modos diferentes.** Ver e editar são o mesmo conjunto de
campos com permissão de escrita diferente; separar em duas telas obriga a manter duas marcações em
sincronia, e é assim que nasce "telas demais mostrando a mesma coisa". O cadastro é a terceira
instância do mesmo componente, com `chave` ausente.

Na prática: `ClienteCadastro.tsx` atende `/clientes/novo` **e** `/clientes/{chave}`, e o estado
`modo` (`novo` | `ficha` | `editando`) é a única diferença.

> **Armadilha comprovada.** Trocar entre `/clientes/novo` e `/clientes/{chave}` **não remonta** o
> componente: o React reaproveita a instância. Sem um `useEffect` que reinicia o estado quando
> `chave` muda, "Novo cliente" abre com os campos do cliente anterior, desabilitados. Foi um
> defeito real, encontrado percorrendo a tela com o Playwright. Toda tela que servir mais de uma
> rota precisa desse reinício.

---

## 2. A lista

### 2.1 A ordem dos elementos, de cima para baixo

```
page-header      título · subtítulo que diz de onde vem o dado · [ação primária à direita]
cad-aviso        aviso da API, quando existe (procedência com aviso, limite de filtro…)
cad-barra        busca · filtros · [limpar filtros, só quando há filtro]
card
 ├ card-header   "X desta filial" · contagem · selo de procedência à direita
 ├ tabela        OU um dos três estados
 └ cad-paginacao faixa "1–25 de 312" à esquerda · tamanho e botões à direita
```

### 2.2 Decisões

**A ação primária mora no `page-header`, à direita, e é uma só.** "Novo cliente", "Novo
equipamento". Ação de linha mora na linha, na última coluna, alinhada à direita.

**A busca vai à API, com espera de 350 ms.** O que está no campo e o que foi pedido são estados
diferentes; sem essa separação, cada tecla vira uma requisição e a lista pisca a cada letra.

**Filtro de catálogo é `<select>`, alimentado por `/api/v1/catalogos`.** Nunca uma lista escrita no
componente. Nunca texto livre.

**Só é cabeçalho clicável a coluna que a API sabe ordenar.** `ordenarPor` é domínio fechado
(`Nome`, `CriadoEm`, `Situacao`, `AlteradoEm` em cliente; `Chassi`, `CriadoEm`, `Situacao`,
`AnoModelo` em equipamento). Oferecer ordenação onde a API recusa é um controle que não funciona.
A coluna ativa leva `aria-sort` e uma seta.

**A tabela rola dentro do próprio contêiner** (`.cad-tabela-wrap { overflow-x: auto }`). A página
nunca rola na horizontal.

**A troca de página não apaga a tabela.** O dado anterior fica na tela enquanto o próximo carrega
(`recarregando`), porque apagar faz o conteúdo piscar e a pessoa perde o lugar onde estava olhando.
Na **ficha** a regra se inverte: a ficha de outro registro não serve para nada, então ela não conta
como "já tenho o dado".

**"Limpar filtros" só aparece quando há filtro.** Botão desabilitado permanente é ruído.

---

## 3. O formulário

### 3.1 O erro da API cai no campo. Isto é metade da usabilidade do cadastro.

A API devolve `application/problem+json` com uma lista `erros`, cada item com `campo`, `mensagem` e
`valorRecebido` (documento 23, seção 3). O caminho é:

```
ErroDaApi.porCampo()      →  { nomeRazao: "Informe a razão social…", documento: "Um CPF pertence…" }
errosDeCampo[nome]        →  <CampoTexto erro={errosDeCampo.documento} />
                          →  aria-invalid, aria-describedby, borda vermelha, frase abaixo do campo
```

**O aviso do topo NÃO recebe erro de campo.** Ele fica para três casos, e só eles:

1. **conflito de concorrência** (409 `/concorrencia`);
2. **recusa sem lista de campos** (conflito de estado, dependência fora do ar);
3. **mensagem endereçada a um campo que esta tela não mostra** — `errosForaDoFormulario()` compara
   com a lista `CAMPOS_DA_TELA` de cada tela, para nenhuma recusa da API desaparecer no caminho.

Nos casos 1 e 2 o topo traz a frase inteira. No caso comum, ele traz só o título da recusa e a
contagem: *"2 campos a corrigir — a mensagem de cada um está no próprio campo."*

**A mensagem some quando a pessoa mexe no campo.** Erro embaixo de um valor já corrigido faz o
formulário parecer travado.

**Recusa de três campos de uma vez, não do primeiro.** É a API que faz isso; a tela só precisa não
desperdiçar: mostra os três.

Exemplo real, exercitado nesta rodada — razão social em branco + CPF declarado como jurídica:

| Campo | O que apareceu embaixo dele |
|---|---|
| `nomeRazao` | Informe a razão social ou o nome do cliente. |
| `documento` | Um CPF pertence a pessoa física. Troque o tipo de pessoa ou informe um CNPJ. |

E o conflito de documento duplicado, que é 409 e não 422, cai igualmente no campo:
*"Este documento já está no cliente «Fazenda Teste Passo 2 862166 Ltda» desta filial. Se for o mesmo
cliente, edite o cadastro existente em vez de criar outro."*

### 3.2 Campo com catálogo não aceita digitação

`<select>`, sempre, alimentado por `/api/v1/catalogos`. O que vai para a API é o **código**; o que a
pessoa lê é a **descrição**. Código é estável e vai para o histórico; descrição pode ser corrigida
sem quebrar registro antigo.

**Item aposentado continua na lista quando é o que está gravado**, marcado *"(fora do catálogo
atual)"*. Escondê-lo faria o `<select>` mostrar outra coisa e a pessoa gravar sem perceber.

**Marca, família e modelo** são três seleções encadeadas montadas de um catálogo só: a API compõe a
descrição de `MODELO_EQUIPAMENTO` como `Marca · Família · Modelo`, e `modelosDeFrota()` desmonta.
Escolher o modelo preenche marca e família de volta.

**Cliente, contato, equipamento e oportunidade** não são `<select>`: são **busca com sugestão**
(`SeletorDeCliente`), porque a listagem tem teto de 200 linhas e um `<select>` cheio mentiria por
omissão. O que a tela manda é a chave; o nome digitado nunca vira dado.

### 3.3 Campo que não se edita mostra por quê

`CampoSomenteLeitura` — moldura tracejada, cadeado no rótulo e uma frase dizendo o motivo. Vale para
o que a API não aceita alterar (chassi e origem do equipamento) e para o que vem de outro sistema
(regra do documento 05 §5: campo do Protheus não é editável no CRM).

Frase de exemplo, na ficha do equipamento:

> **Chassi** 🔒 — *Não entra na alteração: é a identidade da máquina e a chave de deduplicação.
> Chassi errado se corrige baixando o registro e cadastrando o certo, para o histórico não mudar de
> dono em silêncio.*

**Campo desabilitado não mostra texto de exemplo.** Placeholder em campo cinza é lido como valor
gravado, e a pessoa acredita que o cliente tem um CNPJ que ele não tem.

### 3.4 Nada do que foi digitado se perde

| Situação | O que acontece |
|---|---|
| a API recusou (422 ou 409) | o formulário fica exatamente como estava, com os erros nos campos |
| alguém alterou o registro antes (409 `/concorrencia`) | aviso no topo + **dois botões**: *Recarregar a versão do servidor* (traz a versão nova, **mantém o digitado**) e *Descartar minhas alterações* |
| a pessoa clica em Cancelar com alteração pendente | diálogo de confirmação; sem alteração pendente, sai direto |
| a pessoa fecha a aba | `beforeunload`, só enquanto há alteração pendente |
| chega uma releitura enquanto ela edita | o formulário **não** é sobrescrito; só a referência de "o que mudou" |

Exercitado ao vivo: com o campo preenchido com `O QUE EU DIGITEI`, outra requisição alterou o mesmo
cliente por fora; o `PUT` voltou 409, o aviso apareceu, **o texto continuou na tela**, o botão de
recarregar trouxe a versão nova sem apagá-lo, e a segunda gravação passou.

### 3.5 O rodapé

`Cancelar` (secundário) à esquerda de `Gravar alterações` / `Cadastrar X` (primário), os dois à
direita. Enquanto grava, os dois desabilitam e o primário vira *"Gravando…"*.

---

## 4. O que é destrutivo pede confirmação

Inativar cliente e baixar equipamento são **exclusão lógica** — nada é apagado —, e mesmo assim
passam por `DialogoConfirmacao`, porque o registro sai da lista e some do trabalho de todo mundo.

O diálogo:

- **diz o nome do registro** no subtítulo. *"Confirma?"* sozinho não deixa a pessoa perceber que
  clicou na linha errada;
- **explica o que a ação faz e o que ela não faz**: *"nada é apagado; o cliente sai da lista padrão
  e continua visível com o filtro Mostrar inativados"*;
- **aceita o formulário que a ação exige.** O motivo da inativação de cliente é de catálogo e
  obrigatório: pedi-lo depois de confirmar seria uma segunda janela; antes, uma tela inteira para um
  campo. O botão de confirmar fica desabilitado enquanto o motivo não for escolhido;
- **prende o foco, fecha com `Esc` e devolve o foco ao sair.**

O botão de confirmar nomeia a ação (*"Inativar cliente"*), nunca *"OK"*.

---

## 5. Os estados

Três componentes, em `componentes/cadastro/EstadosDeTela.tsx`. São três e não um `if` em cada tela
porque pedem **ações diferentes** — esperar, criar o primeiro registro, ou tentar de novo — e quando
cada tela escreve o seu, a que falhou acaba parecendo a que não tem resultado.

| Estado | Componente | Regra |
|---|---|---|
| **carregando** | `BlocoCarregando` | diz **o que** está buscando, não só "carregando" |
| **vazio** | `BlocoVazio` | **sempre com a ação que faz sentido ali** |
| **erro** | `BlocoErro` | traduz a falha no **que fazer a respeito** |

**Vazio tem dois sabores, e o botão é diferente:**

| Situação | Texto | Ação |
|---|---|---|
| lista vazia de verdade | "Esta filial ainda não tem clientes" | **Cadastrar o primeiro cliente** |
| busca sem resultado | explica **como a busca compara** | **Limpar filtros** |

O segundo caso é onde a tela paga uma dívida da API em vez de esconder: a busca compara o
**documento e o chassi por valor inteiro** (dívida D-2 do documento 23), então quem digita um pedaço
de CNPJ ou os últimos dígitos do chassi não acha nada. O estado vazio **diz isso**, senão a pessoa
conclui que a máquina não está cadastrada.

**Erro traduz, não repete.** `"Erro ao carregar"` não ajuda ninguém:

| Falha | O que a tela diz |
|---|---|
| a API não respondeu | *"A API do CRM não respondeu. Confira se ela está no ar (`dotnet run --project src/Tracbel.Crm.Api`) e tente de novo."* |
| 404 | *"Este registro não está ao alcance desta filial. Ele pode não existir, ou pertencer a outra filial. Troque a filial no cabeçalho para conferir — a API não distingue os dois casos de propósito."* |
| 503 | a frase da API, inteira (ela já diz "quase sempre é a VPN") |

O botão **Tentar de novo** aparece em todo erro de leitura.

---

## 6. A procedência

Toda leitura da API vem envelopada: `sistema`, `objeto`, `lidoEmUtc`, `dadoMaisRecenteEm`,
`estaDesatualizado`, `aviso` (documento 23, seção 1). Isso aparece em **dois lugares, com papéis
diferentes**:

| Onde | Componente | Papel |
|---|---|---|
| no `card-header`, à direita | `SeloProcedencia` | discreto, permanente: `CRM Tracbel · comercial.Cliente · lido em 04/09/2026, 22:52` |
| acima do cartão | `AvisoDeProcedencia` | só quando a API escreveu um `aviso`; é frase para **ler**, não `title` escondido |

**Dado velho muda de cor e ganha a palavra.** Quando `estaDesatualizado` é verdadeiro, o selo vai
para laranja e acrescenta **dado desatualizado**. É a resposta direta aos 17 meses de faturamento
parado passando por atual no legado.

**O nosso próprio banco também é carimbado.** Parece redundante e não é: a mesma tela vai misturar o
cadastro nosso com a leitura da ponte do Vórtice, e se só o legado viesse marcado, a origem do resto
seria deduzida pela ausência de marca. Dedução é o que o carimbo existe para eliminar.

---

## 7. A filial é a fronteira, e ela fica no cabeçalho

`SeletorDeFilial`, alimentado pelo catálogo `EMPRESA` — as **13 filiais em operação**, nunca uma
lista escrita no front. Define o cabeçalho `X-Tracbel-Empresa` de toda requisição.

**Trocar de filial muda o que se vê, e isso é o ponto.** O filtro global do `CrmDbContext` roda em
toda consulta: o mesmo registro responde 200 numa filial e 404 na outra, na leitura e na escrita.
Exercitado nesta rodada:

```
filial 010101 → 3 clientes na lista
filial 010103 → 0 clientes · "Esta filial ainda não tem clientes"
a MESMA ficha, na filial 010103 → "Este registro não está ao alcance desta filial"
```

**O seletor só aparece nas rotas com `usaApi`** (`rotas.tsx`). Nas telas que ainda leem o JSON do
protótipo a filial não existe, e um controle que não faz nada é exatamente o "botão que não faz
nada" do documento 05 §3. Também é o que mantém as outras telas idênticas às capturas de
referência.

**A filial nunca vai no corpo da requisição.** Aceitá-la no JSON abriria um caminho para gravar na
filial de outro, e a fronteira valeria só para a leitura.

---

## 8. Teclado, leitor de tela e telas pequenas

| Regra | Como |
|---|---|
| rótulo de verdade em todo campo | `<label for>` — `CampoTexto`, `CampoSelecao` e `SeletorDeCliente` não têm caminho para nascer sem |
| obrigatoriedade anunciada | `aria-required`. O asterisco é enfeite visual (`aria-hidden`). **`required` nativo não entra**: bloquearia o envio no navegador, e quem recusa campo a campo é a API |
| erro associado ao campo | `aria-invalid` + `aria-describedby` apontando para a frase, com `role="alert"` |
| ordenação anunciada | `aria-sort` no `<th>` ativo; o cabeçalho clicável é um `<button>`, não um `<th>` com `onClick` |
| tabela apresentada | `<caption>` só para leitor de tela (`.cad-so-leitor`), dizendo a filial e a ordem |
| valor somente leitura | `role="group"` + `aria-labelledby`. **Não** `<output>`: `<output>` é região viva e reanunciaria a ficha inteira a cada re-render |
| foco visível | contorno verde de 2 px em tudo que recebe teclado |
| diálogo | foco preso dentro, `Esc` fecha, foco devolvido ao fechar |
| celular e tablet | a grade de duas colunas vira uma em 900 px; filtros empilham; a tabela rola no próprio contêiner; os botões do rodapé ocupam metade cada em 640 px |

---

## 9. Onde as coisas moram

```
src/Tracbel.Crm.Web/src/
├─ tipos/api.ts                       os contratos, campo a campo iguais aos do documento 23
├─ dados/api/
│  ├─ http.ts                         fetch, cabeçalhos de contexto, ErroDaApi/ErroDeRede
│  ├─ contexto.tsx                    a filial e o usuário da sessão
│  ├─ useRecurso.ts                   os quatro estados de uma leitura
│  ├─ catalogos.ts                    catálogos + desmonte de Marca · Família · Modelo
│  ├─ clientes.ts / equipamentos.ts   as cinco operações de cada um
├─ componentes/cadastro/
│  ├─ EstadosDeTela.tsx               carregando · vazio · erro
│  ├─ CamposDeFormulario.tsx          CampoTexto · CampoSelecao · CampoSomenteLeitura · AvisoDoFormulario
│  ├─ SeloProcedencia.tsx             selo + aviso
│  ├─ DialogoConfirmacao.tsx          confirmação com formulário dentro
│  ├─ BarraDePaginacao.tsx            faixa, tamanho e botões
│  ├─ SeletorDeFilial.tsx             o cabeçalho
│  └─ SeletorDeCliente.tsx            busca com sugestão
└─ telas/cadastro/                    as quatro telas + formato.ts
```

**A camada de dados atende as duas origens.** `dados/carregar.ts` e `dados/useDados.ts` continuam
servindo as telas que leem JSON do protótipo, sem uma linha alterada; `dados/api/` é a origem nova.
As duas convivem até o passo 3 terminar.

**CSS:** bloco comentado no fim de `estilos/design-system.css`, prefixo `cad-`, **sem uma cor,
fonte, raio ou sombra nova** — tudo sai das variáveis do topo do arquivo. Classe nova só onde a
estrutura é nova; onde já havia componente (`.card`, `.btn`, `.form-field`, `.form-grid`,
`.modal-ficha-indispo`, `.mfi-*`), a tela usa o que existe.

---

## 10. Dívidas que este padrão herdou, e não criou

| # | O que é | Onde aparece na tela | Quem quita |
|---|---|---|---|
| **P-1** | `.btn-secondary` **perde a borda em toda a aplicação**: uma regra do bloco de Pipeline o redefine com `var(--bg-primary)` e `var(--border-primary)`, que não existem em `:root` — variável indefinida invalida a declaração e `border-style` volta a `none` | botão secundário vira texto solto | restaurado **só** dentro de `.conteudo-cadastro`, porque consertar a regra global muda o pixel das outras telas e quebraria a comparação visual. **O conserto amplo é do passo 3** |
| **P-2** | busca por **pedaço** de documento e de chassi não funciona (dívida D-2 do documento 23) | dito no estado vazio e no texto de ajuda do campo | a API, com uma coluna de busca em texto puro |
| **P-3** | a API **não filtra por modelo** | marca, família e modelo são filtrados na tela; com o filtro ligado ela lê o teto de 200 linhas e **avisa** quando o total passa disso | a API, com um parâmetro `modeloCodigo` na listagem |
| **P-4** | `MOTIVO_INATIVACAO` tem um item `OUTRO` que **exige observação**, e o `DELETE` não tem campo para ela (dívida D-4) | nota no diálogo quando o motivo escolhido pede observação | o negócio, entregando a lista de motivos de verdade |
| **P-5** | a API devolve o **identificador** do proprietário, não o nome | *"Usuário 902"*, com a razão escrita ao lado | um endpoint de usuário |
| **P-6** | paginação por `OFFSET`, não por cursor (dívida D-3) | numeração de páginas com total | quando `processo.Interacao` entrar |

Nenhuma delas é corrigida com um valor inventado na tela. **Onde o dado não existe, a tela diz que
não existe** — princípio 1.4 do documento 16.
