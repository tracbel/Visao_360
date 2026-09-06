# O protótipo: o que é, de onde veio e como vira produto

A gerência produziu um protótipo navegável do CRM Tracbel Agro. Ele define **como o sistema
deve parecer e se comportar**. Este documento explica o que fizemos com ele.

---

## 1. O original

Protótipo estático, gerado no Perplexity Labs, entregue como pasta de arquivos.

| Item | O que é |
|---|---|
| Tecnologia | HTML, CSS e JavaScript puros. Sem framework, sem build, sem servidor |
| Tamanho | 19.531 linhas: `app.css` com 6.790 e `app.js` com 8.755 |
| Telas | 16 rotas num router por hash (`#/cobertura`, `#/clientes/84391`, ...) |
| Visual | design system próprio com as cores John Deere, tipografia Inter e JetBrains Mono |
| Gráficos | Chart.js 4.4.1 · **Mapa** Leaflet 1.9.4 com tiles do OpenStreetMap |
| Dados | fixos dentro do JavaScript, em 45 estruturas |
| Persistência | `localStorage`, só para as oportunidades criadas na tela de cadastro |

Os arquivos `*-block.js` que acompanham a pasta **não são carregados** pelo `index.html`: são
fragmentos de desenvolvimento que acabaram incorporados ao `app.js`.

O original também trazia um bloco de script do Perplexity para edição visual dentro do
iframe deles. Ele foi removido da cópia que guardamos, por não ter função aqui.

---

## 2. O que fizemos

### 2.1 Congelamos a referência

[`prototipo/referencia/`](../../prototipo/referencia/) é a cópia intocável do original.
Serve de fonte para comparar e para consultar. **Não se edita.**

### 2.2 Registramos como cada tela deve ficar

[`capturas-referencia/`](capturas-referencia/) tem uma captura de página inteira por rota,
tirada com Playwright em viewport de 1280 por 900 e densidade 2. É o critério de aceite:
a tela nova está pronta quando fica igual à imagem.

Refazer:

```bash
node scripts/prototipo/capturar-referencia.mjs
```

### 2.3 Tiramos os dados de dentro do código

[`prototipo/dados-seed/`](../../prototipo/dados-seed/) tem as 45 estruturas em JSON, extraídas
por um script que executa o `app.js` num contexto isolado e serializa cada constante. Reexecutável:

```bash
node prototipo/dados-seed/extrair.mjs          # regrava os JSONs
node prototipo/dados-seed/extrair.mjs --check  # só valida
```

Esses JSONs são duas coisas ao mesmo tempo: a fonte de dados da tela enquanto a API não existe,
e a **semente do banco** quando ela existir. Por isso foram extraídos fielmente, sem normalização.

### 2.4 Reconstruímos em React

[`src/Tracbel.Crm.Web/`](../../src/Tracbel.Crm.Web/) é o protótipo local: React, TypeScript e Vite,
que é a escolha registrada em [03-ARQUITETURA](../projeto/03-ARQUITETURA.md).

O que garante que fique idêntico:

1. O `app.css` do original virou `src/estilos/design-system.css` **sem uma linha alterada**.
2. Os componentes reproduzem a mesma marcação e as mesmas classes.
3. Chart.js e Leaflet estão travados nas versões do protótipo. Subir versão muda o desenho.
4. As datas-base do original são constantes fixas e continuam fixas. Trocar por "hoje" quebraria
   a comparação e, pior, mudaria os números que a gerência já validou.

### 2.5 Provamos que ficou igual

```bash
cd src/Tracbel.Crm.Web && npm run dev -- --port 5199
cd scripts/prototipo && node comparar-telas.mjs
```

Cada rota é capturada e comparada pixel a pixel com a referência. O diff sai em
`docs/prototipo/comparacao/`. Ruído de antialias em fonte, gráfico e tiles de mapa é esperado.
Diferença de layout, cor, espaçamento, ordem ou texto não é.

---

## 3. Por que não seguimos com o protótipo original

Ele é ótimo como especificação e inviável como produto:

| Limite do original | O que isso impede |
|---|---|
| Dado fixo dentro do JavaScript | nenhum usuário pode alterar nada; nada persiste |
| Sem servidor e sem banco | não há multiusuário, permissão, auditoria nem histórico |
| Sem tipagem | qualquer campo renomeado quebra em silêncio, em produção |
| Sem testes e sem build | nenhuma proteção contra regressão |
| Tudo num arquivo de 8.755 linhas | duas pessoas não conseguem trabalhar ao mesmo tempo |

O port resolve os cinco pontos **sem mexer no visual**, que é justamente a parte aprovada.

---

## 4. Como isto se liga ao banco

A ordem é esta, e a razão de ser assim está em [14-PADRAO-DE-BANCO](../projeto/14-PADRAO-DE-BANCO.md):

1. As 45 estruturas do protótipo revelam o modelo que a interface pressupõe. Está mapeado em
   [03-MODELO-DE-DADOS-IMPLICITO](03-MODELO-DE-DADOS-IMPLICITO.md), confrontado com o modelo
   já desenhado em [04-MODELO-DADOS](../projeto/04-MODELO-DADOS.md).
2. Esse modelo vira tabela nos schemas por domínio, nunca em `dbo`, com as regras do padrão
   valendo desde a primeira migration.
3. Os JSONs viram seed. A mesma tela, os mesmos números, agora vindos do banco.
4. `src/dados/carregar.ts` troca arquivo por chamada de API. **Só essa camada muda.**

É esse encadeamento que responde ao receio de chegarmos a centenas de tabelas: o banco cresce
por decisão registrada, com teste que barra o que foge do padrão, e não por acúmulo.

---

## 5. Índice

| Documento | Conteúdo |
|---|---|
| [01-ESPEC-UI](01-ESPEC-UI.md) | cada tela, componente a componente, com os textos exatos |
| [02-DESIGN-SYSTEM](02-DESIGN-SYSTEM.md) | tokens, tipografia, catálogo de componentes CSS |
| [03-MODELO-DE-DADOS-IMPLICITO](03-MODELO-DE-DADOS-IMPLICITO.md) | o modelo que a interface pressupõe |
| [04-INVENTARIO-DE-TELAS.csv](04-INVENTARIO-DE-TELAS.csv) | as 16 rotas com complexidade de port |
| [capturas-referencia/](capturas-referencia/) | a baseline visual |
| [comparacao/](comparacao/) | o resultado da última comparação |
