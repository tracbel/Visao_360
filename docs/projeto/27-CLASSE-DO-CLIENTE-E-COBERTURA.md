# Classe do cliente e cobertura por cadência — CRM Tracbel

> Documento 27 · Versão 1.0 · 06/09/2026
> Escopo: de onde vem a letra A/B/C/D de cada cliente, de onde vem o prazo de visita de cada
> classe, e o que exatamente a tela quer dizer quando escreve que um cliente está "coberto".
> Todo acesso ao Vórtice usado para escrever este documento foi **somente leitura**.
> Convenção: `[medido]` = valor obtido ao vivo contra o banco em 06/09/2026.

---

## Resumo em cinco linhas

A classe A/B/C/D **não existe** como dado declarado no Vórtice — as três colunas que deveriam
guardá-la estão vazias, e o protótipo a mostrava porque ela estava escrita no JavaScript. A
cadência de visita **existe** e está preenchida, por departamento. A saída foi apurar a letra da
curva ABC do faturamento real (R$ 2,49 bilhões, 7.252 clientes, 2022 a 11/04/2025) e cruzá-la com
a cadência que o negócio declarou. O resultado é reproduzível: dois auditores chegam ao mesmo
número, e a tela diz de onde ele veio.

---

## 1. A letra não estava lá — três colunas vazias

O modelo do Vórtice tem três lugares que poderiam guardar a classificação do cliente. Nenhum
está preenchido `[medido]`:

| Coluna | Linhas | O que tem dentro |
|---|---:|---|
| `IVS_Pes.Potencial` `varchar(3)` | 139.072 | 134.757 em branco. Dos preenchidos, a maioria guarda `64`, `43`, `22` e `85` — resquício de outro domínio. Só **487** têm uma letra, e 353 delas são `C`. |
| `IVS_Pes.Classe` `varchar(30)` | 139.072 | **100% em branco.** |
| `GE_Pessoa.Porte` `varchar(30)` | 119.365 | Em branco, `.` ou a palavra literal `string`. |

Também foi verificado o ciclo por cliente: `IVS_Pes.Ciclo` está **nulo em 139.070 de 139.072**
vínculos `[medido]`. Não há como deduzir a classe do prazo individual, porque o prazo individual
também não existe.

**Conclusão:** três colunas para a mesma coisa, e nenhuma usada. É o mesmo padrão que o documento
17 descreve para o resto do modelo — o campo existe, o processo nunca o preencheu.

---

## 2. O prazo estava lá — e é por departamento

O que **está** preenchido é a cadência de visita por classe, em `IVS_Depto` `[medido]`:

| Departamento | Classe A | Classe B | Classe C | Classe D |
|---|---:|---:|---:|---:|
| Venda de Máquinas e Implemento | 180 | 180 | 180 | 360 |
| Prospecção de Máquinas e Implementos | 120 | 120 | 120 | 180 |
| Venda de AMS | 360 | 360 | 360 | 360 |
| Venda de Peças | 360 | 360 | 360 | 360 |
| Col Equip Serviços / Usados / Vendas | 360 | 360 | 360 | 360 |

Os outros **dezoito departamentos não declaram cadência nenhuma**.

Dois fatos que essa tabela revela, e que valem para a leitura das telas:

1. **A, B e C têm o mesmo prazo em quase toda linha.** Só a classe D se distingue. Isso não é
   defeito de cadastro: é a operação dizendo que, hoje, a distinção entre as três primeiras
   classes não muda a frequência de visita. A letra continua útil para priorizar e para medir
   faturamento por faixa — mas quem esperava quatro prazos diferentes precisa saber que são dois.
2. **O prazo de 180 dias é semestral, não mensal.** O protótipo usava 30/60/90/120 dias, números
   escritos no JavaScript. Os reais são de três a doze vezes maiores, e uma tela calibrada nos
   números do protótipo classificaria como "atrasado" quase toda a carteira.

> **Defeito corrigido em 06/09/2026:** esses quatro valores eram lidos pela carga e **descartados
> na gravação** — `LinhaDeNegocio.Criar` não recebia os parâmetros. Por isso a Cobertura de
> Carteira mostrava "sem cadência declarada" em 100% das linhas. Hoje quatro linhas de negócio
> têm cadência gravada.

---

## 3. A letra passa a ser apurada — curva ABC do faturamento

### 3.1 A base

`X_TOTVS_CRM_FATURAMENTO`, a tabela que o Vórtice recebe do Protheus `[medido]`:

- **809.821** itens de nota, **182.680** notas, **7.252** clientes, 16 filiais.
- **R$ 2,49 bilhões** líquidos entre 01/01/2022 e 11/04/2025.
- Numa amostra dos 300 maiores clientes por faturamento, **204 casaram** com o nosso cadastro
  pelo CPF/CNPJ. Os 96 restantes são de filiais fora do recorte das treze da Agro.

**A data-limite é 11/04/2025** — a mesma em que `EXT_NFS` parou. A integração inteira com o ERP
morreu naquele dia. Não existe faturamento posterior em lugar nenhum do banco; foi procurado.

### 3.2 O método

Curva ABC clássica, **por filial**:

1. Soma-se o faturamento líquido de cada cliente na janela de três anos.
2. Ordena-se do maior para o menor.
3. Acumula-se: quem cabe nos primeiros **80%** é **A**; de 80% a **95%** é **B**; o resto é **C**.
4. Quem não aparece no faturamento da janela é **D**.

**Por que por filial, e não da rede inteira.** Um cliente que responde por 3% do faturamento de
Votuporanga é grande em Votuporanga e desaparece ao lado de Ribeirão Preto, que fatura vinte vezes
mais. Como a carteira e o CEN são de uma filial, a classe tem de ser da mesma filial — senão o CEN
de uma praça pequena não teria cliente A nenhum para visitar.

**Um cliente que fatura em várias filiais** é classificado pela filial onde mais comprou, que é a
praça em que ele é cliente de verdade.

### 3.3 O que "D" significa

**D é quem não comprou na janela apurada** — não é "cliente ruim". É a maior fatia da carteira, e
é exatamente ela que a cobertura existe para atacar. A distinção importa na leitura: um painel que
mostre "82% dos clientes são D" está dizendo que a maior parte da carteira não comprou em três
anos, e não que a maior parte da carteira é de baixo valor.

### 3.4 O que a apuração NÃO é

- **Não é potencial.** Potencial é quanto o cliente *poderia* comprar; isto é quanto ele
  *comprou*. Um produtor grande que nunca comprou da Tracbel sai como D, e está certo — ele é
  prioridade de prospecção, não de relacionamento.
- **Não é permanente.** A classe é uma fotografia e carrega a data da apuração
  (`Cliente.ClasseApuradaEm`). Um "cliente A" sem essa data não diz se é A hoje ou se era A em
  2023.
- **Não substitui a classificação do negócio.** Se a diretoria quiser declarar a classe por
  critério próprio, o campo aceita — e aí a apuração vira o valor inicial, não a verdade final.

---

## 4. O que "coberto" quer dizer na tela

Um vínculo cliente × carteira está em um de **quatro** estados, e nunca em dois:

| Estado | Definição |
|---|---|
| **Coberto** | A última interação cabe dentro da cadência declarada para a classe dele naquela linha de negócio. |
| **Fora da cadência** | Tem interação, mas mais antiga que o prazo. É a fila de trabalho: cliente que o CEN conhece e deixou vencer. |
| **Nunca contatado** | Não tem nenhuma interação registrada. |
| **Sem cadência declarada** | Está em linha de negócio que não declara prazo. **Não conta como coberto nem como atrasado.** |

**Por que a quarta coluna existe.** Dezoito dos vinte e nove departamentos não declaram cadência.
Empurrar esses vínculos para "coberto" inflaria o indicador; empurrá-los para "atrasado" acusaria
o CEN de descumprir um prazo que ninguém definiu. A resposta honesta é a terceira: não há prazo
contra o que medi-los.

**Por que "fora da cadência" e "nunca contatado" são separados.** Somá-los esconderia a diferença
mais acionável do painel — quem o CEN conhece e deixou vencer não é o mesmo problema que quem ele
nunca procurou. O primeiro é retomada; o segundo é prospecção.

**A unidade é o vínculo, não o cliente.** O mesmo cliente está em várias carteiras, cada uma com
sua cadência. Contar cliente distinto esconderia que ele está coberto em Peças e vencido em
Máquinas.

---

## 5. Onde isso aparece

- **Performance de CEN** — seletor de responsável e a cobertura por classe, com o gráfico
  empilhado dos quatro estados e a tabela com a cadência de cada classe ao lado.
- **API** — `GET /api/v1/relatorios/cen?responsavel={chave}`. Sem o parâmetro, devolve o
  consolidado de todos os responsáveis.

---

## 6. Pendências que este documento abre

1. **Confirmar a janela de três anos com o negócio.** Foi escolhida por ser o que a origem tem e o
   que a prática de concessionária usa, não por decisão da Tracbel.
2. **Confirmar os cortes 80/95.** São o padrão da curva ABC; se a diretoria usa outro corte, é uma
   constante a trocar.
3. **Reconectar o Protheus.** Enquanto o faturamento parar em 11/04/2025, a classe envelhece: um
   cliente que virou A em 2026 continuará aparecendo como D. Ver documento 18.
4. **A família do produto continua fora.** `DESC_FAMILIA` mistura categoria com modelo —
   "TRATORES" e "TRATOR JOHN DEERE 7230J" na mesma coluna —, e somar por ela produziria um mix
   errado. É uma limpeza própria.
