# 49D — Histórico longo de preço: investigação das fontes

> Anexo do [documento 49](49-INTELIGENCIA-DE-MERCADO.md) · 22/09/2026 · issue **IM-09 (#158)**.
> **Pergunta:** existe série longa, aberta e do mesmo conceito para destravar o momento de preço (12 ÷ 12) e
> as médias de 3 e 5 anos **antes de 09/2027**?
> **Resposta curta:** para o momento mensal, **não** — nenhuma fonte aberta publica série longa de preço
> recebido pelo produtor paulista. Para as médias plurianuais, **sim, e ela já está no banco**: o preço
> implícito da PAM, que o CRM passou a guardar desde 2010 com a issue #156.
>
> **[M]** = medido nesta investigação, com data. O que não tem **[M]** é leitura de documentação.

---

## 1. O que o CRM tem hoje, e por que isso trava

| | |
|---|---|
| Fonte carregada | CONAB, `PrecosMensalUF.txt` — **preço recebido pelo produtor**, por UF e mês, em R$/kg |
| Janela do arquivo | **12 meses** — `09/2025` a `08/2026` **[M 22/09/2026]** |
| Série no CRM | desde `09/2025`, crescendo **um mês por rodada** (a carga nunca apaga o mês que saiu do arquivo) |
| Momento 12 ÷ 12 | precisa de **24 meses**; com um mês por rodada, fecha em **09/2027** |

O problema não é a carga, é a fonte: **a CONAB publica uma janela, não um histórico**. Quem quiser
antecipar o índice precisa de outra série — e ela tem de ser do **mesmo conceito**, porque preço recebido
pelo produtor, preço em praça de referência e preço de atacado são três coisas diferentes, e emendá-las
produziria um degrau que ninguém saberia explicar.

---

## 2. As candidatas, uma a uma

### 2.1 CONAB — outros arquivos do portal **[M 22/09/2026]**

Varri o diretório de downloads do Portal de Informações. **Três arquivos respondem 200**; os demais nomes
prováveis dão 404.

| Arquivo | Conteúdo | Período **[M]** | Serve? |
|---|---|---|---|
| `PrecosMensalUF.txt` | preço mensal por UF e nível | `2025-09` a `2026-08` — **12 meses**, 20.753 linhas | é o que já usamos |
| `PrecosSemanalUF.txt` | preço **semanal** por UF e nível | `2025-09` a `2026-09` — **13 meses**, 89.901 linhas | **não**: mesma janela, só mais fina |
| `SerieHistoricaGraos.txt` | área, produção e produtividade de grãos | — | **não**: é produção, não preço |

O arquivo semanal foi uma descoberta desta investigação e **não resolve o histórico** — ele tem a mesma
janela de 12 meses, com granularidade semanal. Vale como melhoria futura de *granularidade* (quatro a cinco
observações por mês em vez de uma), não de *extensão*.

Endereço: `https://portaldeinformacoes.conab.gov.br/downloads/arquivos/<arquivo>`.
Termos: dado aberto, sem cadastro; os quatro níveis de comercialização (atacado, varejo, pago e recebido
pelo produtor) estão no mesmo arquivo **[M]**.

### 2.2 IEA-SP — Instituto de Economia Agrícola **[M 22/09/2026]**

A candidata mais promissora no papel: o IEA publica **preços médios recebidos pelos agricultores
paulistas**, e a documentação do próprio instituto fala em série de café desde **julho de 1948**.

Na prática, **os formulários públicos não oferecem o histórico**. Li as opções de período de cada sistema:

| Sistema | Endereço | Períodos oferecidos **[M]** |
|---|---|---|
| Preços recebidos pelos agricultores | `infoiea.agricultura.sp.gov.br/nia1/precos_medios.aspx?cod_sis=2` | **18** — `2025/JAN` a `2026/JUN` |
| Atacado (RMSP) | `…/Precos_Medios.aspx?cod_sis=3` | 20 — `2025/JAN` a `2026/AGO` |
| Varejo na capital | `…/precos_medios.aspx?cod_sis=4` | 19 — `2025/JAN` a `2026/JUL` |
| Preços pagos pela agricultura | `…/pagos.aspx?cod_sis=5` | 6 — `2023/Jan` a `2024/Jan` |

Ou seja: **dezoito meses**, não setenta e sete anos. A série longa existe no acervo do IEA e aparece nos
textos dele, mas não está exposta na consulta pública — e o formulário é ASP.NET com `__VIEWSTATE`, o que
torna a automação frágil mesmo para os 18 meses.

Dois detalhes operacionais, medidos:

- o host antigo `ciagri.iea.sp.gov.br` **apresenta certificado de outro domínio** (`*.agricultura.sp.gov.br`):
  qualquer leitor apontado para ele falha no TLS. O host atual é `infoiea.agricultura.sp.gov.br`;
- a página do Banco de Dados **não traz termos de uso, licença, cadastro nem CAPTCHA** — é consulta aberta.

**Conclusão:** não serve para o histórico. Serve, se algum dia for preciso, como **segunda opinião** sobre os
mesmos 12 a 18 meses que já temos — e aí seria um leitor novo para confirmar o que a CONAB diz, não para
estender a série.

### 2.3 IPEADATA **[M 22/09/2026]**

Baixei o catálogo inteiro: **3.605 séries**. Filtrei as de preço agropecuário.

| Achado **[M]** | Consequência |
|---|---|
| Todas as séries de "preço recebido pelo agricultor" são do **Paraná** (Seab-PR): boi, café, cana, leite, milho, soja, trigo… | conceito certo, **estado errado** |
| A única série paulista de preço agrícola é `DEPAE12_ATMI12` — "preço médio — **atacado** — milho — 60 kg — **SP**", fonte Conab/IE, **488 observações, de 01/1982 a 08/2026** | série longa de verdade, mas de **atacado**: conceito diferente do recebido pelo produtor |
| Nenhuma série com fonte IEA (`FNTSIGLA` contendo "IEA": **zero**) | o IPEADATA não republica o acervo do IEA |
| Nenhum índice de preços recebidos pela agricultura | não serve nem para deflacionar/estender |

A API é aberta (`http://www.ipeadata.gov.br/api/odata4/Metadados`), sem cadastro. O filtro `$filter` com
`contains` devolve **400**; o catálogo inteiro vem numa requisição e se filtra localmente **[M]**.

**Conclusão:** não serve. Emendar atacado com recebido pelo produtor é exatamente o que a regra desta issue
proíbe. O Paraná não é São Paulo, e "usar o PR como proxy do SP" seria inventar um dado.

### 2.4 CEPEA **[M 22/09/2026]**

O texto-base do potencial cita o CEPEA. A investigação achou o motivo de ele continuar fora, e agora com
base documental e não por suposição:

> **"Licença de uso de dados: CEPEA (CC BY-NC 4.0)"**
> — aviso publicado **duas vezes** na página do indicador da soja, e repetido na página de termos de uso.

`CC BY-NC 4.0` é **Creative Commons Atribuição-NãoComercial**: a cláusula **NC proíbe uso comercial**. Um CRM
que apoia a decisão comercial de uma revenda é uso comercial — não há leitura honesta que enquadre isso como
não comercial.

**Conclusão:** o CEPEA continua **bloqueado**, e não por barreira técnica: por **licença**. A decisão D-P11
(#117) deixa de ser "vale a pena assinar?" e passa a ser **"a Tracbel precisa de licença comercial do CEPEA
para usar o indicador"**. Contato institucional no próprio site; nada aqui contorna a licença, e a coleta
automatizada não deve ser tentada enquanto não houver contrato.

### 2.5 Socicana — já no CRM

ATR da cana, **11 safras**, já carregado (issue #66). É a única série longa de preço que o CRM já tem, e
vale só para cana. Continua valendo; não resolve as outras culturas.

---

## 3. A saída que estava dentro de casa: o preço implícito da PAM

O IBGE **não publica preço** na PAM. Mas publica *quantidade produzida* e *valor da produção* — e define o
segundo assim:

> "O valor da produção é uma variável derivada, calculada pela **média ponderada das informações de
> quantidade e preço médio corrente pago ao produtor**, segundo os períodos de colheita e comercialização."

Ou seja: **`valor ÷ quantidade` devolve o preço médio recebido pelo produtor naquele ano**, ponderado pela
colheita — a mesma família de conceito da CONAB, em outra frequência e em outro recorte:

| | CONAB (carregada) | PAM implícito (derivável hoje) |
|---|---|---|
| Conceito | preço recebido pelo produtor | preço médio recebido pelo produtor, ponderado pela colheita |
| Frequência | **mensal** | **anual** |
| Recorte | UF (SP) | **município** — os 645 |
| Período no CRM | desde `09/2025` | **desde 2010**, com a issue #156 |
| Unidade | R$/kg | R$/t (derivar para R$/kg na leitura) |

### 3.1 A conferência, com números **[M 22/09/2026]**

Comparei o preço implícito da PAM de **2025 em SP** com a média dos meses de 2025 que o arquivo da CONAB
ainda tem (setembro a dezembro):

| Cultura | PAM implícito 2025 | CONAB recebido (meses de 2025) | Diferença |
|---|---|---|---|
| Soja (em grãos) | R$ 2,0579/kg | R$ 2,0800/kg | **+1,1%** |
| Milho (em grãos) | R$ 1,0859/kg | R$ 0,9975/kg | −8,1% |
| Café (arábica tipo 6) | R$ 33,8917/kg | R$ 37,3825/kg | +10,3% |
| Amendoim (em casca) | R$ 3,8010/kg | R$ 2,7200/kg | −28,4% |

**O que isto prova e o que não prova.** Prova que as duas medidas são da mesma família e da mesma ordem de
grandeza — a soja fica a 1% de distância. **Não** prova que são intercambiáveis: a diferença varia de 1% a
28% conforme a cultura, por dois motivos conhecidos: (a) a janela da CONAB disponível hoje cobre só quatro
meses de 2025, e não o ano inteiro ponderado pela colheita, e (b) a classificação do produto difere entre as
duas (a PAM tem "amendoim em casca" somando safras que a CONAB classifica à parte).

Por isso, e pela regra desta issue — **série de conceito diferente não se emenda** —, a recomendação **não**
é preencher o passado da série mensal da CONAB com a PAM. É publicar a PAM implícita como **série própria,
anual, municipal e rotulada**, ao lado da outra.

---

## 4. Recomendação

| Necessidade | Fonte | Quando fica disponível |
|---|---|---|
| **Momento de preço 12 ÷ 12** (mensal) | CONAB, acumulando no CRM | **09/2027** — ou antes, só com licença comercial do CEPEA |
| **Médias de 3 e 5 anos** (anual) | **PAM implícita**, já no banco desde 2010 | **agora**, com uma leitura derivada |
| **Preço por município** | **PAM implícita** — a CONAB só tem UF | **agora** |
| ATR da cana | Socicana, 11 safras | já está |

1. **Abrir o leitor? Não — e a issue de desdobramento é a #198.** A fonte viável não precisa de leitor: o dado já está gravado em
   `organizacao.ProducaoAgricolaNoMunicipio`. O que falta é uma **leitura derivada** — preço implícito =
   `ValorDaProducaoMilReais × 1000 ÷ QuantidadeProduzida`, com a unidade da issue #152 —, uma tela que a
   mostre como o que ela é, e o cuidado de **não somá-la nem emendá-la** à série mensal.
2. **Não gravar o preço implícito como coluna.** Ele é derivado de duas colunas que já estão no banco;
   guardá-lo criaria uma terceira verdade para o mesmo número — a mesma recusa que o modelo já fez com o
   rendimento médio da PAM.
3. **Rever D-P11 (#117) com o fato novo:** o CEPEA não está bloqueado por robô, está por **licença NC**. A
   decisão é jurídica e comercial, não técnica.
4. **Não usar o Paraná como proxy de São Paulo**, e não emendar atacado com recebido pelo produtor.
5. **Anotar o arquivo semanal da CONAB** como melhoria de granularidade, se algum dia o momento precisar de
   resolução menor que o mês.

---

## 5. O que ficou sem resposta

- **O acervo longo do IEA existe e não está exposto.** Não perguntei ao instituto se há outra via (arquivo,
  pedido formal, LAI). Se a diretoria quiser antecipar o histórico paulista sem pagar licença, **esse é o
  caminho a tentar** — e é pedido a órgão público, não raspagem.
- **Quanto custa a licença do CEPEA** — não há tabela pública; depende de contato institucional.
- **A conciliação ano a ano** entre PAM implícita e CONAB só poderá ser feita quando o CRM tiver 12 meses
  cheios de um ano civil — o primeiro será **2026**, fechando em 01/2027.
