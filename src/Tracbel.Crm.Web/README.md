# Tracbel.Crm.Web — o CRM em React

Interface do CRM próprio. Nasce como **porte fiel do protótipo** feito pela gerência
(`prototipo/referencia/`), tela por tela, e evolui daí para consumir a API real.

## Rodar

```bash
npm install
npm run dev          # http://localhost:5173
```

Só isso. Os dados vêm de `public/dados/*.json` enquanto a API não existe.

| Comando | O que faz |
|---|---|
| `npm run dev` | servidor de desenvolvimento com recarga |
| `npm run build` | build de produção em `dist/` |
| `npm run preview` | serve o `dist/` para conferência |
| `npx tsc --noEmit` | checagem de tipos |

## Como isto está organizado

| Pasta | O que tem |
|---|---|
| `src/estilos/design-system.css` | **cópia fiel** do `app.css` do protótipo. Não editar sem decisão registrada: é o que garante que a tela nova fique igual à antiga |
| `src/componentes/` | shell (`Layout`), ícones e componentes reutilizáveis entre telas |
| `src/telas/` | uma tela por rota, com o mesmo nome da rota |
| `src/dados/` | carregamento de dados. Hoje lê JSON; quando a API existir, **só esta pasta muda** |
| `src/tipos/` | tipos TypeScript das entidades |
| `public/dados/` | os 45 JSONs extraídos do protótipo por `prototipo/dados-seed/extrair.mjs` |

## As regras do porte

1. **Mesma marcação, mesmas classes** do protótipo. O CSS não muda, então a tela sai igual.
2. **Nada de CSS novo.** Se parecer que falta uma classe, ela está no `design-system.css`.
3. **Versões travadas** onde o visual depende delas: `chart.js` 4.4.1 e `leaflet` 1.9.4, as mesmas
   do protótipo. Atualizar muda o desenho dos gráficos e do mapa.
4. **Datas fixas.** O protótipo congela "hoje" em constantes (`AGENDA_HOJE`, `HOJE_CARTEIRA`).
   Usar `new Date()` quebra a comparação com as capturas de referência.
5. **Dado nenhum embutido em componente.** Tudo passa por `src/dados/`.

## Como se prova que ficou igual

```bash
npm run dev -- --port 5199
# em outro terminal
cd ../../scripts/prototipo
node comparar-telas.mjs
```

Cada rota é capturada e comparada pixel a pixel com
`docs/prototipo/capturas-referencia/`. O diff sai em `docs/prototipo/comparacao/`.
Ruído de antialias em fonte, gráfico e tiles de mapa é esperado. Diferença de layout, cor,
espaçamento, ordem ou texto não é.

No Git Bash, rotas passadas como argumento precisam de `MSYS_NO_PATHCONV=1`, senão o shell
converte `/cobertura` em caminho do Windows.

## O que ainda não existe

A API. Enquanto ela não sobe, `src/dados/carregar.ts` lê arquivo estático. A troca é nessa
camada, sem tocar nas telas.
