# 49C — Catálogo de culturas: o de-para entre as fontes

> Anexo do [documento 49](49-INTELIGENCIA-DE-MERCADO.md) · 22/09/2026 · issue **IM-02 (#151)**.
> **Para que serve:** é a semente do catálogo de culturas que a **IM-16 (#165)** vai pôr no Administrador.
> Enquanto ele não existe, "café" é uma palavra escrita de cinco jeitos em cinco lugares — e nenhum deles é
> chave de nada.
> **Regra:** o vínculo é por **código** quando a fonte tem código, e por rótulo exato quando não tem.
>
> **[M]** = medido na fonte em 22/09/2026. **[P]** = proposta desta análise, a confirmar.

---

## 1. Onde "cultura" aparece hoje, e por que isso é um problema

| Lugar | Como a cultura é identificada | Consequência |
|---|---|---|
| PAM (IBGE) | código da **classificação 782** — 85 produtos **[M]** | é a única fonte com código estável e cobertura municipal |
| CONAB preços | `id_produto` + nome + classificação — **34 produtos com preço recebido em SP** **[M]** | código próprio, que não é o do IBGE |
| CONAB custos | **texto do link** na página, por cultura — 6 arquivos **[M]** | sem código nenhum: o vínculo é pelo rótulo |
| SICOR (BCB) | `cdProduto` — **131 produtos em SP** nos anos 2023–2025 **[M]** | código próprio, e com agregação diferente (ver §3) |
| Socicana | não tem produto: publica o **preço do kg de ATR** da cana | série única, só cana |
| Front | listas fixas de cultura no código (M-03) | cultura nova exige publicação |

Cinco vocabulários. O catálogo é o que os liga — e é por isso que a **IM-16** depende deste documento.

---

## 2. As seis culturas do pedido, com o vínculo em cada fonte **[M]**

| Cultura | PAM (782) | CONAB preço | CONAB custo | SICOR | Socicana | Unidade comercial |
|---|---|---|---|---|---|---|
| **Café** | **40139** Café (em grão) Total — detalhados: 40140 Arábica, 40141 Canephora | **11195** `CAFE` / `ARÁBICA TIPO 6, BEBIDA DURA` | `CAFÉ ARÁBICA` | **1580** `CAFÉ` | — | saca de 60 kg |
| **Cana-de-açúcar** | **40106** Cana-de-açúcar | **4238** `CANA DE ACUCAR` / `NÃO INFORMADO` | `CANA DE AÇÚCAR` | **1840** `CANA-DE-AÇUCAR` | **preço do kg de ATR** | tonelada |
| **Soja** | **40124** Soja (em grão) | **4744** `SOJA` / `EM GRÃOS` | `SOJA` | **3841 `GRÃOS`** — não separa (§3) | — | saca de 60 kg |
| **Milho** | **40122** Milho (em grão) — safras separadas na **tabela 839**: 114253 (1ª) e 114254 (2ª) | **4742** `MILHO` / `EM GRÃOS` | `MILHO 1ª SAFRA`, `MILHO 2ª SAFRA` | **3841 `GRÃOS`** — não separa | — | saca de 60 kg |
| **Laranja** | **40151** Laranja | **12290** `LARANJA` / `INDÚSTRIA PERA` | `LARANJA` | **4140** `LARANJA` | — | caixa de 40,8 kg |
| **Amendoim** | **40101** Amendoim (em casca) | **4674** `AMENDOIM` / `EM CASCA` | `AMENDOIM` | **não tem produto próprio** em SP (§3) | — | saca de 25 kg |

Três observações que mudam conta:

1. **O café entra na soma da lavoura só pelo "Total" (40139).** A classificação traz Arábica e Canephora na
   mesma resposta; somar os três conta o café duas vezes. A regra já está no código, e fica registrada aqui.
2. **O milho da PAM é um produto só.** A separação de 1ª e 2ª safra vem da tabela **839**, classificação 81,
   que a IM-07 (#156) passou a carregar. É ela que permite descontar a terra que a soja e o milho safrinha
   dividem (IM-11, #160).
3. **A classificação do preço importa.** A CONAB publica o café como "ARÁBICA TIPO 6, BEBIDA DURA" e a laranja
   como "INDÚSTRIA PERA": são recortes específicos, não a cultura inteira. Quem calcular receita por hectare
   precisa dizer qual recorte usou — é o tooltip que a IM-10 (#159) pede.

---

## 3. O SICOR agrega culturas, e isso não tem conserto pelo de-para **[M 22/09/2026]**

Li os produtos do recurso `InvestMunicipioProduto` para São Paulo (código **27** no Banco Central, não 35)
nos anos de 2023 a 2025: **131 produtos distintos**.

| Cultura do pedido | Produto no SICOR |
|---|---|
| Café | **1580** `CAFÉ` |
| Cana-de-açúcar | **1840** `CANA-DE-AÇUCAR` |
| Laranja | **4140** `LARANJA` |
| Soja, milho, amendoim | **não aparecem como produto próprio** — o que existe é **3841 `GRÃOS`** |

**Consequência para o motor:** o crédito rural pode ser lido por cultura para café, cana e laranja, e **não
pode** para soja, milho e amendoim — para esses, o máximo honesto é "grãos". Qualquer indicador de crédito
por cultura precisa dizer isso, e a alternativa (ratear "GRÃOS" pela área de cada uma) **seria inventar um
número**. Fica registrado como limite da fonte, não como pendência de implementação.

Os produtos de **máquina** do SICOR, que o motor já usa: **7080** `TRATOR`, **4860** `MÁQUINAS E
IMPLEMENTOS`, **2700** `COLHEITADEIRAS, COLHEDEIRAS E ARRANCADEIRAS` **[M]**. Não há produto de plantadeira
nem de pulverizador no investimento — outro limite que a IM-16 (#165) vai ter de respeitar ao ligar
categoria de máquina a produto do SICOR.

---

## 4. Os 85 produtos da PAM, com segmento **[P]**

A classificação por segmento é **proposta desta análise**, a confirmar pelo comercial — ela não veio de
planilha. Os códigos e os nomes são os oficiais do IBGE, lidos dos metadados da tabela 5457 em 22/09/2026
**[M]**.

Contagem por segmento: fruticultura 30 · grãos 18 · olericultura 13 · outros 12 · café 3 · citros 3 ·
algodão 2 · borracha 2 · cana 2.

> "Grãos" inclui as oleaginosas (mamona, linho, gergelim, canola, girassol), porque o que as une para o
> motor é o sistema de máquinas, não a botânica. "Outros" guarda fibras, folhas, mandioca e temperos — o que
> não forma um grupo de máquina próprio. Os dois agrupamentos são os que mais merecem revisão.

| Código (782) | Produto | Segmento **[P]** |
|---|---|---|
| 40129 | Abacate | fruticultura |
| 40092 | Abacaxi* | fruticultura |
| 83388 | Abóbora | olericultura |
| 45982 | Açaí | fruticultura |
| 83399 | Acerola | fruticultura |
| 83389 | Alface | olericultura |
| 40329 | Alfafa fenada | outros |
| 40130 | Algodão arbóreo (em caroço) | algodão |
| 40099 | Algodão herbáceo (em caroço) | algodão |
| 40100 | Alho | olericultura |
| 40101 | Amendoim (em casca) | grãos |
| 40102 | Arroz (em casca) | grãos |
| 40103 | Aveia (em grão) | grãos |
| 40131 | Azeitona | fruticultura |
| 40136 | Banana (cacho) | fruticultura |
| 40104 | Batata-doce | olericultura |
| 40105 | Batata-inglesa | olericultura |
| 40137 | Borracha (látex coagulado) | borracha |
| 40468 | Borracha (látex líquido) | borracha |
| 40138 | Cacau (em amêndoa) | fruticultura |
| 40140 | Café (em grão) Arábica | café |
| 40141 | Café (em grão) Canephora | café |
| 40139 | Café (em grão) Total | café |
| 40330 | Caju | fruticultura |
| 40331 | Cana para forragem | cana |
| 40106 | Cana-de-açúcar | cana |
| 83390 | Canola | grãos |
| 40142 | Caqui | fruticultura |
| 40143 | Castanha de caju | fruticultura |
| 40107 | Cebola | olericultura |
| 83391 | Cenoura | olericultura |
| 40108 | Centeio (em grão) | grãos |
| 40109 | Cevada (em grão) | grãos |
| 40144 | Chá-da-índia (folha verde) | outros |
| 83392 | Chuchu | olericultura |
| 40145 | Coco-da-baía* | fruticultura |
| 83400 | Cupuaçu | fruticultura |
| 40146 | Dendê (cacho de coco) | fruticultura |
| 40147 | Erva-mate (folha verde) | outros |
| 40110 | Ervilha (em grão) | grãos |
| 40111 | Fava (em grão) | grãos |
| 40112 | Feijão (em grão) | grãos |
| 40148 | Figo | fruticultura |
| 40113 | Fumo (em folha) | outros |
| 83395 | Gergelim | grãos |
| 40114 | Girassol (em grão) | grãos |
| 40149 | Goiaba | fruticultura |
| 83401 | Graviola | fruticultura |
| 40150 | Guaraná (semente) | fruticultura |
| 83393 | Inhame | olericultura |
| 40115 | Juta (fibra) | outros |
| 40151 | Laranja | citros |
| 40152 | Limão | citros |
| 40116 | Linho (semente) | grãos |
| 40260 | Maçã | fruticultura |
| 40117 | Malva (fibra) | outros |
| 40261 | Mamão | fruticultura |
| 40118 | Mamona (baga) | grãos |
| 40119 | Mandioca | outros |
| 40262 | Manga | fruticultura |
| 40263 | Maracujá | fruticultura |
| 40264 | Marmelo | fruticultura |
| 40120 | Melancia | fruticultura |
| 40121 | Melão | fruticultura |
| 40122 | Milho (em grão) | grãos |
| 83394 | Milho verde | olericultura |
| 83396 | Morango | fruticultura |
| 40265 | Noz (fruto seco) | fruticultura |
| 40266 | Palmito | fruticultura |
| 40267 | Pera | fruticultura |
| 40268 | Pêssego | fruticultura |
| 40269 | Pimenta-do-reino | outros |
| 83397 | Pimentão | olericultura |
| 40123 | Rami (fibra) | outros |
| 83398 | Repolho | olericultura |
| 40270 | Sisal ou agave (fibra) | outros |
| 40124 | Soja (em grão) | grãos |
| 40125 | Sorgo (em grão) | grãos |
| 40271 | Tangerina | citros |
| 40126 | Tomate | olericultura |
| 40127 | Trigo (em grão) | grãos |
| 40128 | Triticale (em grão) | grãos |
| 40272 | Tungue (fruto seco) | outros |
| 40273 | Urucum (semente) | outros |
| 40274 | Uva | fruticultura |

---

## 5. O que este catálogo manda para a IM-16 (#165)

1. **A chave da cultura é o código da PAM** — é a única fonte com código estável, série longa e cobertura
   municipal. As outras se ligam a ela, não o contrário.
2. **Uma cultura pode ter mais de um produto da PAM** (o café tem três, e só o Total entra na soma), então o
   vínculo é **um para muitos**, com a marca de quem entra na soma da lavoura.
3. **Nem toda cultura tem as cinco fontes.** O catálogo precisa aceitar vínculo ausente e a tela precisa
   dizer "sem preço da CONAB para esta cultura" em vez de mostrar vazio sem motivo.
4. **A unidade comercial e o fator vêm do catálogo**, não do código: saca de 60 kg, saca de 25 kg, caixa de
   40,8 kg e tonelada aparecem nas seis culturas do pedido, e a IM-03 (#152) já provou que a quantidade da
   PAM não é tonelada em todos os produtos.
5. **O vínculo do SICOR é opcional e agregador** — e, quando for "GRÃOS", o catálogo precisa registrar que
   ele **não é exclusivo daquela cultura**.

## 6. O que ficou sem resposta

- **A classificação por segmento é minha proposta**, não do comercial. Os 12 "outros" e a fronteira entre
  grãos e oleaginosas são os pontos a confirmar.
- **Não conferi os códigos contra o banco do servidor** — a IM-01 (#150) faz essa conferência, e o acesso ao
  banco central depende de VPN. O que está aqui foi lido **das fontes originais**, que é a checagem mais
  forte: se o código existe no IBGE, na CONAB e no BCB, existe.
- **Amendoim no SICOR**: não achei produto próprio em SP entre 2023 e 2025. Pode existir em outro estado ou
  em outro recurso do SICOR (custeio, por exemplo) — a busca aqui foi no investimento, que é o que o CRM usa.
