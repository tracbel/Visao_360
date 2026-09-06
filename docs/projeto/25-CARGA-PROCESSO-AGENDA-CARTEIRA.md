# Carga do relacionamento — processo, agenda, histórico e carteira

> **Documento 25** · Versão 1.0 · 05/09/2026
> Cobre `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs` (o comando),
> `src/Tracbel.Crm.Integracao/Carga/LeitorDeCargaDoVortice.Processo.cs` (a leitura) e
> `src/Tracbel.Crm.Integracao/Carga/SaneamentoDeProcesso.cs` (as traduções).
> Continuação do [documento 24](24-CARGA-DE-DADOS-2026.md): lá veio **quem** é o cliente, aqui vem
> **o que aconteceu** com ele.
> Executada ao vivo contra o banco de produção do Vórtice, **somente leitura**, com a VPN de pé.
> Todo número deste documento saiu de uma execução real, não de estimativa.

---

## 1. O recorte é o mesmo, e é de propósito

**A mesma pessoa, a mesma filial, o mesmo ano.** O recorte do documento 24 — quem teve interação
em 2026 numa das treze filiais em operação — não foi reescrito nem afrouxado: a consulta é a
mesma constante, reusada pelas onze leituras novas. Duas cargas com recortes diferentes
produziriam processos sem cliente e tarefas sem dono, que é exatamente o defeito que estamos
migrando para longe.

| O que veio | Lidas | Gravadas |
|---|---:|---:|
| **Usuário** (`GE_Usuario`) | 273 | **273** |
| **Linha de negócio** (`IVS_Depto`) | — | **13** |
| **Carteira** (`IVS_Carteira`) | 142 | **142** |
| **Carteirização** (`IVS_Pes`) | 49.801 | **49.109** |
| **Tipo de processo** (`IV_CodProcesso`) | — | **16** |
| **Fase** (de onde os processos estão) | — | **50** |
| **Tipo de tarefa** (`IV_Acao`) | — | **178** |
| **Desfecho** (`IV_Resultado`) | — | **484** |
| **Processo** (`IV_Processo` × `IV_ProcDado`) | 45.861 | **45.397** |
| **Tarefa** (`IV_Agenda`) | 104.152 | **103.339** |
| **Interação** (`IV_Historico`) | 124.591 | **121.983** |

A carga inteira — cadastro do documento 24 mais este relacionamento — leva **8 min 43 s**.

### Os 1,17 milhão de processos viraram 45.861

A tabela de origem tem **1.190.993 linhas**. O filtro de 2026 sozinho derruba isso para 47.966; o
recorte das treze filiais, para **45.867**; e as pessoas do recorte, para **45.861**. As seis
linhas de diferença são processos de 2026 em filial em operação cuja pessoa **não teve atividade
em 2026** — o processo existe, o relacionamento não.

---

## 2. O que veio, por filial

Contado no banco depois da carga:

| Código | Filial | Processos | Tarefas | Interações | Carteiras | Vínculos | Usuários |
|---|---|---:|---:|---:|---:|---:|---:|
| 010101 | Ribeirão Preto | 20.500 | 32.975 | 35.096 | 41 | 15.104 | 83 |
| 010116 | Votuporanga | 2.929 | 8.476 | 10.902 | 11 | 4.425 | 14 |
| 010117 | Tupã | 2.881 | 7.069 | 8.002 | 7 | 2.218 | 13 |
| 010113 | São José do Rio Preto | 2.880 | 9.269 | 10.768 | 12 | 2.653 | 28 |
| 010102 | Araraquara | 2.732 | 6.839 | 7.754 | 8 | 2.527 | 22 |
| 010107 | Orlândia | 2.517 | 5.422 | 6.317 | 9 | 933 | 13 |
| 010103 | Barretos | 2.045 | 4.436 | 5.371 | 13 | 5.128 | 20 |
| 010114 | Catanduva | 1.950 | 5.817 | 6.916 | 10 | 3.553 | 21 |
| 010115 | Jales | 1.662 | 6.301 | 8.484 | 6 | 1.714 | 11 |
| 010111 | Franca | 1.615 | 6.657 | 8.069 | 6 | 2.443 | 15 |
| 010109 | Bebedouro | 1.500 | 3.761 | 5.487 | 8 | 6.551 | 18 |
| 010112 | Itápolis | 1.261 | 3.278 | 5.040 | 7 | 1.320 | 10 |
| 010118 | Marília | 925 | 3.039 | 3.777 | 4 | 540 | 8 |
| | **Total** | **45.397** | **103.339** | **121.983** | **142** | **49.109** | **276** |

Os 276 usuários são os **273 migrados** mais os **3 de desenvolvimento** que o seed já criava.

**Bebedouro é a linha que salta**, e é dado real, não erro: 1.500 processos e **6.551 vínculos de
carteira** — quatro vezes mais vínculos que processos. Uma única carteira responde por 5.000
deles: `DSI_PUK2_06`, de Soluções Integradas. Ribeirão Preto tem o par de pneus, com 4.008 e
4.016 vínculos cada. **Carteira grande não é carteira ativa**, e a seção 6.1 mostra a diferença.

### Por fluxo, com o desfecho

| Fluxo | Processos | Abertos | Ganhos | Perdidos | Cancelados |
|---|---:|---:|---:|---:|---:|
| Venda Equipamento Tracbel Agro | 23.212 | 15.463 | 988 | 777 | **5.938** |
| Prospecção Peças/Serviços | 7.967 | 7.967 | 0 | 0 | 0 |
| Aferição Qualidade JDE | 6.317 | 6.317 | 0 | 0 | 0 |
| Venda Peças/Pneus Tracbel Agro | 3.160 | 1.764 | 1.395 | 1 | 0 |
| SEGUROS (FY25) | 2.306 | 1.904 | 315 | 87 | 0 |
| Pre-Entrega | 1.326 | 1.326 | 0 | 0 | 0 |
| Entrega Física / Técnica | 812 | 812 | 0 | 0 | 0 |
| Demonstração Tracbel Agro | 164 | 164 | 0 | 0 | 0 |
| *demais 8 fluxos* | 133 | 115 | 11 | 7 | 0 |
| | **45.397** | **35.832** | **2.709** | **872** | **5.938** |

🔴 **Um quarto do fluxo de vendas termina em cancelado** — 5.938 de 23.212. É o mesmo número que
a pesquisa 16 já tinha medido, e ele atravessou a carga intacto porque não é defeito de carga: é
o que a origem diz. Ver seção 5.

---

## 3. As decisões que a carga teve de tomar

Onze decisões, cada uma com a contagem de quantas linhas ela alcançou. Nenhuma foi tomada em
silêncio: todas saem no relatório da execução e as cinco mais discutíveis têm teste.

| Linhas | Decisão |
|---:|---|
| **81.571** | Tarefas **sem prazo limite** — a origem não declara prazo nem na ação nem na agenda |
| **42.917** | Processos cujo **título foi composto do número**, por falta de resumo na origem |
| **30.747** | Interações **escritas pelo servidor** da origem, não por gente — marcadas como `Sistema` |
| **1.721** | Processos cujo **login de responsável não existe** no cadastro de usuários |
| **993** | Tarefas cuja **ação a origem não declara** — entraram no tipo `NAO_INFORMADA` |
| **872** | Processos **perdidos sem motivo de perda** na origem |
| **358** | Desfechos **sem mapeamento** de fase nem de status — entraram como `Manutenção` |
| **273** | Usuários migrados **sem e-mail** — o cadastro de origem não guarda nenhum |
| **178** | Tipos de tarefa **sem prazo declarado** na origem |
| **178** | Tipos de tarefa **sem categoria de contato** na origem |
| **142** | Carteiras **sem supervisor** na origem — a coluna é nula em 100% delas |

### 3.1 O prazo ausente: a decisão que o requisito pediu por escrito

**Nenhuma das 178 ações em uso preenche prazo.** O produto tem cinco colunas de prazo no cadastro
de ação — `PrazoRealizacao`, `PrazoMaxInicio`, `PrazoMaxReag`, `TempoMedio`, `ExigeDtaLimite` — e
todas estão vazias nas ações que geraram agenda em 2026.

**A decisão, em duas partes:**

1. **O tipo de tarefa entra com prazo `0`, e zero significa "a origem não declara prazo".** A
   entidade `TipoTarefa` admitia zero na coluna (`CK_TipoTarefa_Prazo` exige `>= 0`), mas o valor
   padrão do domínio era `1` dia útil. Escrever "1 dia útil" em 178 tipos faria **toda tarefa
   migrada nascer com um atraso que a origem nunca declarou** — um indicador vermelho fabricado
   por carga.
2. **A tarefa só ganha prazo limite quando a linha da agenda traz um.** São **22.301 de 103.339**
   (21,6%); as outras **81.571 ficam com o campo vazio**, e o relatório conta.

**A consequência disso na API está declarada na resposta**, e não escondida no código: o painel da
agenda devolve `metricasSemDado` com o texto medido —
*"4.851 de 9.522 tarefas pendentes não têm prazo limite declarado. O indicador cobre só o
restante."* E o atraso da tarefa é medido **contra a data agendada**, não contra o prazo: medir
pelo prazo faria 79% da agenda parecer em dia por falta de dado, que é o pior tipo de indicador
verde.

### 3.2 O status em branco: 480.428 linhas, e o que se faz com elas

`IV_Processo.Status` é `varchar(20)` livre. O domínio real, medido na tabela inteira:

| Situação na origem | Linhas | Vira, no CRM |
|---|---:|---|
| `EM ABERTO` | 483.575 | Aberto |
| ***(em branco)*** | **480.428** | **Aberto** |
| `Cobrança Finalizada` | 55.336 | Aberto |
| `CANCELADO` | 41.410 | Cancelado |
| `EM ANDAMENTO` | 27.247 | Aberto |
| **`FINALIZADO`** | **18.416** | **Aberto** |
| `A INICIAR` | 17.944 | Aberto |
| `FATURADO` | 12.888 | Ganho |
| **`FINALIZADA`** | **11.563** | **Aberto** |
| `DESISTIU DA COMPRA` | 11.345 | Perdido |
| `VENDA REALIZADA` | 6.436 | Ganho |
| `CANCELADA` | 2.412 | Cancelado |
| `VENDA PERDIDA` | 2.695 | Perdido |
| *outras 22 grafias* | ~14.000 | conforme a tabela de tradução |

No recorte de 2026 das treze filiais, a tradução produziu:

| Situação no CRM | Processos | O que a origem dizia |
|---|---:|---|
| **Aberto** | 35.832 | `EM ABERTO` (17.050), *em branco* (18.415), `EM ANDAMENTO`, `PEDIDO REALIZADO`, `A INICIAR`… |
| **Cancelado** | 5.938 | `CANCELADO` / `CANCELADA` |
| **Ganho** | 2.709 | `FATURADO`, `VENDA REALIZADA` |
| **Perdido** | 872 | `VENDA PERDIDA`, `DESISTIU DA COMPRA`, `PEDIDO NÃO APROVADO`, `DEVOLVIDO` |
| **Suspenso** | 46 | `PENDENTE`, `PARADO` |

**A decisão, e é a mais delicada da carga.** Um processo que a origem marca como realizado mas
cujo status **não declara desfecho comercial** — `FINALIZADO`, `FINALIZADA`, `CONCLUIDO`, ou em
branco — **continua Aberto**. São **15.425 processos de 2026** com status em branco e
`Realizado = 1`.

Por que não fechá-los:

- **Ganho** inventaria uma venda. Não houve.
- **Perdido** inventaria uma perda. Também não houve.
- **Cancelado** repetiria exatamente o defeito do legado, onde o cancelamento virou a lixeira que
  engoliu 5.991 negociações sem motivo nenhum.
- E `Realizado` **não é o desfecho do negócio**: ele acompanha o último passo do fluxo. A prova
  está no próprio dado — 5.958 dos 5.992 processos `CANCELADO` de 2026 estão com `Realizado = 1`.
  Usar essa coluna para fechar processo seria encodar um significado que ela não carrega.

> **Recomendação:** `SituacaoDoProcesso` precisa de um sexto valor — **`Encerrado`**, "o fluxo
> terminou e não havia venda em jogo" — para os fluxos que não são de venda: aferição de
> qualidade (6.317 processos), pré-entrega (1.326), entrega física (812) e prospecção de peças
> (7.967). Hoje esses **16.422 processos ficam abertos para sempre** e inflam o funil em 46%.
> **Não foi feito aqui:** é mudança de domínio fechado, exige migração e decisão de produto — a
> mesma natureza da recomendação de I/O/Q no chassi do documento 24.

### 3.3 O motivo de perda que não existe

`processo.Processo` tem uma restrição de verificação: **perdido exige motivo**
(`CK_Processo_MotivoDePerda`). O legado encerra sem exigir motivo nenhum — 83% das saídas do
funil de 2026 não declaram um.

**A decisão:** nasce um item `NAO_INFORMADO_NA_ORIGEM` — *"Não informado na origem"*, categoria
`Outro` — e os **872 processos perdidos** entram apontando para ele. É o mesmo precedente do
`PAPEL_CONTATO / NAO_INFORMADO` do documento 24: o valor diz exatamente o que se sabe, que é nada.

**E a API não deixa isso passar por distribuição.** O relatório de perdas devolve o total e uma
métrica ausente com o motivo medido:

```json
{ "itens": [ { "codigo": "NAO_INFORMADO_NA_ORIGEM", "nome": "Não informado na origem",
               "quantidade": 190 } ],
  "metricasSemDado": [ { "metrica": "perdasPorMotivo",
    "motivo": "Os 190 processos perdidos não têm motivo declarado na origem. O sistema legado
               encerra o processo sem exigir motivo, e por isso não existe distribuição de perda
               para mostrar — só o total." } ] }
```

### 3.4 A categoria de contato que não veio

`CategoriaDeInteracao` diz se o contato foi visita, ligação, WhatsApp, e-mail, reunião remota ou
passo interno. **As 178 ações migradas entraram todas como `Interna`.**

`IV_Acao.Classe` é `char(1)` com sete valores — `O` (672 linhas), `X` (226), `T` (42), `P` (31),
`W` (5), `R` (2), `?` (2) — **sem tabela de domínio no banco**: o significado mora dentro do
cliente Gupta. Classificar por palavra do nome foi considerado e recusado: *"Monitorar Cliente"*
(25.230 tarefas) pode ser visita, ligação ou WhatsApp, e o dado não diz qual. `Interna` é o único
valor do domínio fechado que **não afirma** contato com o cliente.

> **Pergunta objetiva ao negócio:** das 178 ações em uso, quais são visita, quais são ligação,
> quais são WhatsApp? É uma triagem de 178 linhas, e ela destrava a segmentação por canal na
> Visão 360 e na Cobertura. Enquanto não vier, a métrica "interações por canal" **não tem dado**.

### 3.5 O usuário sem e-mail, e o domínio que não pode existir

**Zero dos 273 usuários necessários tem e-mail no cadastro de origem.** A coluna `EMAILTRAB`
está vazia em todos, e `LOGIN` também. O que existe é `CodUsuario` — o login, no formato
`NOME.SOBRENOME`.

Nossa coluna de e-mail é obrigatória, porque o usuário do CRM é espelho do Entra ID e o endereço
é a identidade.

**A decisão:** o e-mail e o nome principal nascem como `login@sem-email.vortice.invalid`. O
sufixo `.invalid` é **reservado pela RFC 2606 exatamente para isto** — é um domínio que não pode
existir. O valor é único por usuário (o índice exige), é legível na tela como "não informado", e
**não afirma nada**. Construir `login@tracbel.com.br` teria sido inventar um endereço que pode
não existir e que alguém, um dia, usaria para mandar mensagem.

**E a senha não é lida.** Não aparece na consulta: nem para conferir, nem para migrar, nem para
contar. [V] a senha do legado tem 30 bytes sem sal e 80 usuários compartilham exatamente o mesmo
valor armazenado; um espelho disso seria um passivo de segurança criado de propósito.

**A identidade externa é derivada e declaradamente provisória:** o GUID nasce de um resumo MD5 do
login. É **estável entre execuções** — o que faz a reexecução reencontrar a mesma pessoa em vez de
estourar o índice único — e nunca colide com um GUID de verdade, porque não foi sorteado. Some no
dia em que o Entra ID entrar.

---

## 4. O achado da carga: o duplo ponteiro só funciona numa direção

A pesquisa 16, seção 4.1, elogia o duplo ponteiro como *"o melhor detalhe de modelagem do
Vórtice"*: a agenda aponta para o histórico que a gerou (`HistoricoOrigem`) e, quando concluída,
para o que a concluiu (`UltHistorico`). **Medindo para carregar, o ponteiro de ida quase não
existe.**

| Medida (agendas de 2026, treze filiais) | Linhas |
|---|---:|
| Agendas marcadas como realizadas | 79.096 |
| …com `DtaRealizacao` | 79.096 (100%) |
| …com `UltResultado` | 78.985 (99,9%) |
| …**com `UltHistorico`** | **10.442 (13,2%)** |
| Agendas com um histórico apontando de volta por `AgendaOrigem` | **89.283 (113%)** |

O ponteiro **de volta** é gravado quase sempre; o **de ida**, quase nunca.

**A primeira execução desta carga usou só o ponteiro de ida, e o resultado foi visível na hora:
69.521 tarefas entraram pendentes com a origem dizendo que estavam realizadas.** A agenda migrada
nasceria com um passivo falso de setenta mil linhas.

Corrigido: a conclusão é procurada **nos dois sentidos**, com preferência para o ponteiro de ida
quando ele existe. O efeito, medido:

| | Antes | Depois |
|---|---:|---:|
| Tarefas concluídas | 33.818 | **72.923** |
| Tarefas pendentes | 69.521 | **30.416** |
| Recusas de conclusão (`Realizada = S` sem as três coisas) | 69.521 | **1.417** |
| Duplo ponteiro fechado no CRM | 5.238 | **72.914** |

> **Lição para a migração completa:** o invariante que a pesquisa mediu por amostra pode não
> valer no volume. `UltHistorico` foi lido como "existe e é confiável" porque existe na
> modelagem; só a tentativa de usá-lo para 104 mil linhas mostrou que ele é preenchido em 13%
> dos casos. **Toda coluna de ponteiro do legado precisa ser medida em cobertura antes de
> virar chave de junção.**

### 4.1 Concluir exige três coisas, e a falta de qualquer uma deixa pendente

A restrição `CK_Tarefa_Conclusao` do banco cobra data, quem concluiu e desfecho. A carga confere
as três antes de chamar `Concluir`, e a linha que não tem as três **entra pendente, com a recusa
de campo registrada** — visível na agenda, em vez de fechada sem ninguém conseguir dizer com que
desfecho. São 1.417 casos.

---

## 5. O que foi normalizado, por regra

**192.563 correções de campo** na etapa de relacionamento, cada uma com valor de origem, valor
entregue e motivo — o princípio 1.4 do [documento 16](16-HIGIENIZACAO-DE-DADOS.md).

| Campo | Corrigidos | Exemplo real (origem → entregue) |
|---|---:|---|
| **classeNaCarteira** | **49.541** | `22` → `C` |
| detalheDaTarefa | 47.920 | espaço duplo e quebra de linha colapsados |
| detalheDaInteracao | 42.879 | idem |
| **situacaoDoProcesso** | **36.225** | `EM ABERTO` → `Aberto` |
| descricaoDoProcesso | 14.230 | `Fase: Pedido de Venda - Descr: Fase: Pedido de Venda - Descr:…` |
| tituloDoProcesso | 1.719 | `Pedido Realizado - ch570` |
| natureza | 39 | `M` → `Ativa` |
| assuntoDaInteracao | 10 | espaço colapsado |

E **124.503 campos recusados**, sem derrubar a linha:

| Campo | Recusados | Por quê |
|---|---:|---|
| **duracao** | **122.812** | a coluna existe na origem e é **zero em 100%** do histórico do período |
| **conclusaoDaTarefa** | **1.417** | `Realizada = S` sem data, sem desfecho ou sem quem concluiu |
| **emailDoUsuario** | **273** | o cadastro de usuário da origem não guarda e-mail |
| coordenada | 1 | par `(0,0)` — cai no golfo da Guiné |

🔴 **Não existe tempo de atendimento para migrar.** A coluna `Duracao` está lá, tem 122.812
oportunidades de ser preenchida no ano e não foi preenchida nenhuma vez. Qualquer métrica de
produtividade por tempo é impossível — e é uma das métricas do protótipo que a seção 9 lista como
sem dado.

### 5.1 A classe de cliente que quase toda entrou assumida

**49.541 dos 49.109 vínculos** tiveram a classe assumida (o número passa do total porque a
contagem inclui as linhas recusadas depois). O domínio real de `IVS_Pes.Potencial`, `varchar(3)`
sem catálogo:

| Classe no CRM | Vínculos | De onde veio |
|---|---:|---|
| **C** | **49.050** | 99,9% assumida — vazio, ou `64`/`43`/`22`/`85`, que são resquício de outro domínio |
| B | 45 | lida |
| D | 9 | lida |
| A | 5 | lida |

🔴 **A segmentação por classe de cliente não tem dado.** Cinquenta e nove clientes classificados
de verdade em 49.109 vínculos. É a métrica mais visível do protótipo (a Cobertura ordena por
classe) e ela não se sustenta.

---

## 6. A carteirização — o melhor ativo do modelo antigo, medido

✅ **A carteirização multi-linha-de-negócio veio inteira, e ela é o motivo de esta carga existir.**

| Medida | Valor |
|---|---:|
| Clientes carteirizados | **21.001** de 23.942 (88%) |
| Clientes em **uma** carteira | 10.611 |
| Clientes em **duas ou mais** | **10.390 (49%)** |
| Vínculos totais | 49.109 |
| Carteiras | 142 |
| Linhas de negócio | 13 |

**Metade dos clientes carregados está em duas ou mais carteiras ao mesmo tempo** — o mesmo
produtor atendido por um CEN de máquinas, um de peças e um de pneus, cada um com o seu ciclo. Nem
Salesforce nem Dynamics entregam isso de fábrica.

As maiores carteiras, com a cobertura calculada no banco:

| Carteira | Linha de negócio | Responsável | Clientes |
|---|---|---|---:|
| TBA_PNEUS_02 | Venda de Pneus | JOAO PESSOA DOS SANTOS | 4.016 |
| TBA_PNEUS_01 | Venda de Pneus | ROBERSON ROSA MOREIRA | 4.008 |
| RAO_AMS_01 | Venda de AMS | MATHEUS AUGUSTO CRUZ | 920 |
| ORL_AMS_07 | Venda de AMS | TASSO TOLEDO | 705 |
| DGT_FORAREG_02 | Venda de Máquinas e Implemento | LUAN.MORI | 691 |
| INTELIGÊNCIA DE MERCADO TRACBEL AGRO | Vendas Digitais | INT.MERCADO | 526 |

### 6.1 A data do último contato NÃO foi copiada — e essa é a decisão mais valiosa da carga

`IVS_Pes.DtaUltCtto` existe e está preenchida em 14.496 dos vínculos do recorte. **Ela não foi
migrada.** Em vez disso, `ClienteCarteira.UltimaInteracaoEm` é **calculada das interações que
efetivamente entraram** — uma única instrução de conjunto, ao fim da carga.

Por quê, com o que a pesquisa 16 mediu:

1. A coluna da origem só é preenchida para desfechos marcados em `IVS_DeptoRes` como "conta como
   contato" — uma tabela de configuração com **63 linhas em 7 departamentos**, de 29.
2. Consequência: **DSI, Pneus, AMS e Dados Cadastrais somam 31.556 clientes carteirizados cuja
   data de último contato nunca poderá ser preenchida** — não porque ninguém os atenda, mas
   porque nenhum resultado desses departamentos foi marcado.
3. A procedure que a atualizava (`vrtc_p_atualizar_data_ultimo_contato`) **não roda desde
   07/08/2025**: ela não está no agendador.

**O resultado da nossa conta:**

| Medida | Vínculos |
|---|---:|
| **Com data de último contato calculada** | **39.589 (81%)** |
| Sem interação registrada no recorte | 9.520 (19%) |

Copiar a coluna do legado teria entregue 14.496; calcular do fato entregou **39.589** — e cada
uma delas é verificável, porque aponta para uma interação que está no banco.

> **É a única exceção consciente à regra de não guardar dado derivado**, e ela já estava
> registrada na própria entidade antes desta carga: a tela de Cobertura ordena dezenas de
> milhares de clientes por essa data, e calculá-la a cada abertura custaria varrer a tabela de
> interações.

---

## 7. O que foi recusado, e por quê

**4.598 linhas na fila de descarte** da etapa de relacionamento, cada uma em
`integracao.MensagemDescartada` com o conteúdo cru em JSON e o motivo em português. Somadas às
15.906 do cadastro, são **20.504** consultáveis:

```sql
SELECT Fluxo, Erro, COUNT(*) FROM integracao.MensagemDescartada
WHERE Fluxo LIKE 'VORTICE.CARGA.%' AND TratadaEm IS NULL
GROUP BY Fluxo, Erro ORDER BY 3 DESC;
```

| Linhas | Regra que recusou | Exemplo real |
|---:|---|---|
| **1.779** | Interação — **sem autor na origem** | `SeqUsuario` nulo ou zero no histórico |
| 826 | Interação — sem cliente e sem processo no CRM | a pessoa não entrou no recorte |
| 809 | Tarefa — sem cliente e sem processo no CRM | idem |
| 692 | Carteirização — cliente do vínculo fora da carga | o cliente foi recusado no cadastro |
| **464** | Processo — **cliente do processo fora da carga** | a pessoa foi recusada por documento |
| 21 | Desfecho — sem ação dona na origem | `IV_Resultado.Acao` nulo |
| 4 | Tarefa — responsável fora do cadastro de usuários | `SeqUsuario` que não existe em `GE_Usuario` |
| 3 | Interação — autor fora do cadastro de usuários | idem |

### As 1.779 interações sem autor são um achado, não um erro da carga

O histórico é **fato imutável** no modelo novo, e a coluna de quem registrou é obrigatória: uma
interação sem autor entraria na linha do tempo do cliente sem ninguém para responder por ela.
Na origem, `IV_Historico.SeqUsuario` é nulo ou zero em 1.779 das 124.591 linhas de 2026 — 1,4%.
**A recusa é a lista.** Ela existe, é consultável, e alguém pode tratá-la.

### Os 464 processos recusados vêm do documento 24

Nenhum deles é problema desta carga: são processos cuja **pessoa foi recusada no cadastro** — 190
por documento não reconstituível, 82 por tipo de pessoa fora de `F`/`J`, 4 por documento
repetido. É a cascata funcionando como se espera: um cliente que não entrou não deixa processo
órfão atrás de si, e o motivo já está escrito no descarte do cliente.

---

## 8. Os catálogos, e o que divergiu de `dados-referencia/`

Os quatro catálogos que processo, tarefa e interação exigem **nasceram do dado de 2026**, e não
do cadastro do fornecedor. A diferença entre "marcado em uso" e "usado" é a diferença entre um
catálogo cuidado e um catálogo abandonado.

| Catálogo | No legado | Marcado em uso | **Usado em 2026** | Carregado |
|---|---:|---:|---:|---:|
| `IV_CodProcesso` → `processo.TipoProcesso` | 62 | 58 | **16** | **16** |
| fases (`IV_Processo.Fase`) → `processo.Fase` | 273 cadastradas | — | **50 pares fluxo×fase** | **50** |
| `IV_Acao` → `processo.TipoTarefa` | **980** | **378** | **178** | **178** |
| `IV_Resultado` → `processo.Resultado` | **4.209** | — | **505** | **484** |
| `IVS_Depto` → `organizacao.LinhaDeNegocio` | 29 | — | **13 com gente** | **13** + 1 |
| `IVS_Carteira` → `organizacao.Carteira` | 655 | 168 com gente | **142 nas 13 filiais** | **142** |

Os 21 desfechos que faltam dos 505 são os que **não apontam ação dona** na origem — sem tipo de
tarefa, eles não teriam onde aparecer.

### 8.1 A conciliação com `dados-referencia/`, e ela diverge muito

| Arquivo | O que ele tem | O que o dado real tem | Veredito |
|---|---|---|---|
| `tipo_de_processo.json` | **62 itens**, o catálogo inteiro do legado | **16 fluxos** com processo em 2026 | 🟡 **46 itens mortos.** O arquivo migrou a lista, não o uso |
| `fase.json` | **6 itens do protótipo**: `QUALIFICACAO`, `DIAGNOSTICO`, `PROPOSTA`, `NEGOCIACAO`, `FECHAMENTO`, `GANHO_PERDIDO` | **22 códigos distintos**, em 50 pares fluxo×fase | 🔴 **Só 2 dos 6 existem** (`NEGOCIACAO`, `FECHAMENTO`). `QUALIFICACAO`, `DIAGNOSTICO`, `PROPOSTA` e `GANHO_PERDIDO` **não existem no dado** |
| `tipo_de_tarefa.json` | **4 itens do protótipo**: `MONITORAR`, `VISITAR`, `LIGAR`, `PROPOSTA` | **178 ações** em uso | 🔴 **Nenhum código coincide.** O protótipo descreve categorias; o legado tem ações operacionais |
| `resultado.json` | **vazio** — o próprio `PENDENTES.md` declara "sem fonte utilizável" | **484 desfechos** | ✅ A carga preenche o vazio que o arquivo declarava |
| `linha_de_negocio.json` | **6 itens**: `TRATORES`, `COLHEITADEIRAS`, `PULVERIZADORES`, `PLANTADEIRAS`, `IMPLEMENTOS`, `PECAS_SERVICOS` | **13 departamentos**: `MAQ_NOVOS`, `MAQ_PECAS`, `MAQ_PNEUS`, `DSI`, `DSI_PUK`, `VENDAS_DIGIT`… | 🔴 **Zero coincidência, e são coisas diferentes**: o arquivo lista **famílias de produto**, o dado lista **departamentos comerciais** |
| `categoria_de_interacao.json` | 6 itens, incluindo `FEIRA_EVENTO` | nenhuma categoria vem da origem (seção 3.4) | 🟡 `FEIRA_EVENTO` não existe no domínio fechado do código, e `Interna` não existe no arquivo |
| `classe_de_cliente.json` | `A`, `B`, `C`, `D` | as mesmas quatro | ✅ **Único que bate.** Mas 99,9% dos vínculos entraram em `C` por assunção |

**A divergência que mais importa é a de fase**, porque é a que a tela de Pipeline usa. As seis
colunas do kanban aprovado — Qualificação, Diagnóstico, Proposta, Negociação, Fechamento,
Ganho/Perdido — **não correspondem ao funil que existe**. O funil real do fluxo de vendas é
Apresentação → Negociação → Pedido de Venda → Montagem → Análise → Aprovação → Autorização →
Formalização → Faturamento → Recebimento → Entrega → Preparação → Finalizado, e a coluna com
**14.302 dos 15.463 processos abertos** chama-se **Apresentação** — nome que não existe no
protótipo.

> **Decisão desta carga:** as fases vieram do **dado**, não do protótipo. Uma coluna de kanban
> vazia é pior do que uma coluna com nome estranho — e as seis do protótipo produziriam cinco
> colunas vazias e uma com tudo dentro. A reconciliação entre as três granularidades (protótipo,
> `IV_ProcFase` e o funil real medido) continua sendo a pergunta aberta que
> `dados-referencia/PENDENTES.md` já registrava; agora ela tem o terceiro lado medido.

---

## 9. As métricas do protótipo que **não têm dado real** que as sustente

Esta seção é o cumprimento literal do requisito: *"onde uma métrica não tiver dado real que a
sustente, não invente — devolva vazio com o motivo"*. Cada linha aqui devolve vazio pela API, com
o motivo **medido na mesma consulta**.

| Métrica da tela | Situação | O número que sustenta a afirmação |
|---|---|---|
| **Valor do funil / ticket médio** | 🔴 **sem dado** | **358 de 45.397 processos (0,8%)** declaram valor. Em Ribeirão Preto: 9 de 16.028 (0,1%) |
| **Perdas por motivo** | 🔴 **sem dado** | **872 processos perdidos, 872 sem motivo declarado** na origem |
| **Tempo médio de atendimento** | 🔴 **sem dado** | `Duracao` é **zero em 122.812 de 122.812** interações |
| **Interações por canal** (visita / ligação / WhatsApp) | 🔴 **sem dado** | as **178** ações entraram como `Interna`; a origem não classifica canal (seção 3.4) |
| **Segmentação por classe de cliente** | 🔴 **quase sem dado** | **59 de 49.109** vínculos têm classe lida; os outros 49.050 foram assumidos como `C` |
| **Cumprimento de prazo / SLA** | 🔴 **sem dado de prazo** | **81.571 de 103.339** tarefas sem prazo limite; **0 de 178** tipos com prazo declarado |
| **Atingimento de meta** | 🔴 **sem dado** | `organizacao.Meta` está **vazia** — não há fonte no legado nem no protótipo |
| **Previsão de fechamento / forecast** | 🟡 **7,6%** | **3.467 de 45.397** processos têm previsão de conclusão |
| **Probabilidade por fase** | 🔴 **sem dado** | `Fase.ProbabilidadePercentual` é nula nas 50 fases; a origem tem `Perspectiva` preenchida em 3.922 processos, mas por processo, não por fase |
| **Georreferência do atendimento** | 🟡 **24%** | **29.491 de 121.983** interações têm coordenada |
| Contagem do funil por fase | ✅ **tem dado** | 35.832 processos abertos, distribuídos em 32 pares fluxo×fase |
| Agenda: pendentes, atrasadas, hoje, 7 dias | ✅ **tem dado** | 30.416 pendentes, 4.067 atrasadas só em Ribeirão Preto |
| Linha do tempo do cliente | ✅ **tem dado** | 121.983 interações, com autor, desfecho e data |
| Cobertura de carteira | ✅ **tem dado** | 39.589 vínculos com data de último contato calculada |

**O formato da recusa, na resposta real da API:**

```json
"metricasSemDado": [
  { "metrica": "valorDoFunilConfiavel",
    "motivo": "Só 9 de 16028 processos abertos declaram valor (0,1%). O total vem preenchido,
               mas ele representa essa fração — não o funil inteiro." }
]
```

`[V]` É a resposta direta ao defeito que este projeto existe para corrigir: no legado, 17 meses de
faturamento parado passaram por atual porque a tela mostrava um número sem dizer de onde ele
vinha.

---

## 10. Como o comando funciona

O mesmo executável do documento 24, com duas etapas separáveis:

```bash
$env:Vortice__Conexao = "..."                                          # a credencial NUNCA é versionada
dotnet run --project src/Tracbel.Crm.Carga -- --somente-medir           # conta e sai
dotnet run --project src/Tracbel.Crm.Carga -- --recomecar               # zera e carrega as duas etapas
dotnet run --project src/Tracbel.Crm.Carga -- --somente-relacionamento  # só esta etapa, reconciliando
dotnet run --project src/Tracbel.Crm.Carga -- --somente-cadastro        # só a etapa do documento 24
```

**A ordem interna não é arbitrária, é a das chaves estrangeiras:** sem usuário não há dono de
carteira nem de tarefa; sem linha de negócio não há carteira; sem tipo de processo não há fase;
sem fase não há processo; sem tarefa não há interação com ponteiro. Cada etapa devolve o de-para
que a seguinte consome, e nenhuma delas consulta o banco linha a linha.

### Somente leitura no Vórtice, por construção

Não existe `INSERT`, `UPDATE`, `SELECT INTO` nem chamada de procedimento em
`LeitorDeCargaDoVortice`. Toda consulta usa `WITH (NOLOCK)` e **toda consulta tem filtro** — de
ano, de filial, ou de pertencer ao recorte. A maior delas (o histórico, 2,4 milhões de linhas na
origem) é lida por um ano só. A lista de filiais entra em **parâmetros**, um por filial.

As tabelas novas lidas são `GE_Usuario`, `IVS_Depto`, `IVS_Carteira`, `IVS_Pes`, `IV_VENDEDOR`,
`IV_CodProcesso`, `IV_Acao`, `IV_Resultado`, `IV_ProcResultado`, `IV_Processo`, `IV_ProcDado`,
`IV_Agenda` e `IV_Historico` — **todas vivas**. As paradas de frota do ERP, faturamento, ordem de
serviço e título financeiro continuam fora.

### O fuso horário, declarado

A origem grava data e hora **local, sem fuso**. O Brasil não tem horário de verão desde 2019,
então a conversão para UTC é uma soma fixa de três horas. Para dado anterior a 2019 isso erraria
uma hora nos meses de verão; o recorte é o ano corrente e não alcança essa faixa — a exceção é a
data de entrada do vínculo de carteira, que pode ser de 2015, e uma hora de erro numa data de
vinculação não muda nenhuma decisão.

### A marca de sincronismo

Sete linhas novas em `integracao.PontoDeSincronismo`, ao lado das quatro do documento 24:

| Fluxo | Lidos | Gravados | Recusados |
|---|---:|---:|---:|
| `VORTICE.CARGA.USUARIO` | 273 | 273 | 0 |
| `VORTICE.CARGA.CARTEIRA` | 142 | 142 | 0 |
| `VORTICE.CARGA.CLIENTE_CARTEIRA` | 49.801 | 49.109 | 0 |
| `VORTICE.CARGA.CATALOGO` | — | 178 | 0 |
| `VORTICE.CARGA.PROCESSO` | 45.861 | 45.397 | 0 |
| `VORTICE.CARGA.TAREFA` | 104.152 | 103.339 | 0 |
| `VORTICE.CARGA.INTERACAO` | 124.591 | 121.983 | 1.779 |

### O que só se sabe depois: as duas escritas em conjunto

Duas coisas dependem de tudo já estar gravado, e as duas são feitas **em conjunto**, não linha a
linha:

1. **O duplo ponteiro** — ligar cada tarefa concluída à interação que a concluiu (72.914 linhas).
2. **A data do último contato** — calcular `UltimaInteracaoEm` das interações carregadas (39.589
   vínculos).

São 122 mil interações contra 103 mil tarefas e 49 mil vínculos: percorrer isso pelo rastreador de
mudanças levaria mais tempo do que a carga inteira, e o resultado seria idêntico. **São as únicas
duas escritas em conjunto, e as duas gravam no nosso banco** — o Vórtice continua intocado.

---

## 11. Idempotência: rodar de novo reconcilia, não duplica

**391.718 linhas de de-para** em `integracao.ChaveExterna`, zero repetida.

A segunda execução, feita **sem** `--recomecar`, com `--somente-relacionamento`:

| Tabela | Depois da 1ª | Depois da 2ª |
|---|---:|---:|
| `seguranca.Usuario` | 276 | **276** |
| `organizacao.LinhaDeNegocio` | 14 | **14** |
| `organizacao.Carteira` | 142 | **142** |
| `comercial.ClienteCarteira` | 49.109 | **49.109** |
| `processo.TipoProcesso` | 16 | **16** |
| `processo.Fase` | 50 | **50** |
| `processo.TipoTarefa` | 179 | **179** |
| `processo.Resultado` | 484 | **484** |
| `processo.Processo` | 45.397 | **45.397** |
| `processo.Tarefa` | 103.339 | **103.339** |
| `processo.Interacao` | 121.983 | **121.983** |
| `integracao.ChaveExterna` | 391.718 | **391.718** |
| chaves externas repetidas | 0 | **0** |

### A agenda é a tabela que mais muda entre duas execuções

A reconciliação não é enfeite aqui: entre duas rodadas, a tarefa de ontem foi concluída ou foi
empurrada para a semana que vem. **27,6% das agendas de 2026 já foram reagendadas pelo menos uma
vez** na origem. Por isso a tarefa existente é reprogramada e concluída na reexecução, em vez de
ser pulada — *"rodei de novo e nada mudou"* seria o defeito, não o objetivo.

**A interação é o oposto, e de propósito:** ela é somente-acrescentar. Numa reexecução o de-para
reencontra a linha e a rodada apenas carimba a conciliação — o fato que aconteceu não muda de
forma.

---

## 12. A API nova

Nove rotas de leitura, todas no padrão do [documento 23](23-API.md): paginação com teto de 200,
domínio fechado nos filtros e na ordenação, procedência em toda resposta e **a fronteira de
multiempresa valendo**.

| Verbo | Rota | O que sustenta |
|---|---|---|
| `GET` | `/api/v1/processos` | Pipeline, Funil, aba de oportunidades da Visão 360 |
| `GET` | `/api/v1/processos/{chave}` | ficha da oportunidade |
| `GET` | `/api/v1/tarefas` | Agenda do CEN |
| `GET` | `/api/v1/interacoes` | linha do tempo da Visão 360 |
| `GET` | `/api/v1/cobertura` | Cobertura, cliente a cliente |
| `GET` | `/api/v1/relatorios/funil` | funil por fase, **agrupado no banco** |
| `GET` | `/api/v1/relatorios/perdas` | perdas por motivo |
| `GET` | `/api/v1/relatorios/agenda` | painel do CEN |
| `GET` | `/api/v1/relatorios/cobertura` | cobertura por carteira |

O detalhe de parâmetros está no documento 23, seção 2.5.

### 12.1 O agregado é do banco, e a fronteira vale nele

**É o ponto onde o legado mais vaza:** dos 134 relatórios do Vórtice, **125 não têm predicado de
usuário nenhum**. Aqui o `GROUP BY` roda dentro do filtro global do contexto de persistência, e o
teste prova: a mesma rota de funil devolve uma linha por fase para cada filial, e nunca a soma
das duas.

Exercitado ao vivo contra o SQL Server real, com a carga dentro:

```
                                      010101   010103
GET /api/v1/processos                  16.035    1.867
GET /api/v1/tarefas                    32.975    4.436
GET /api/v1/cobertura                  15.104    5.128
GET /api/v1/relatorios/cobertura      41 linhas 13 linhas
GET /api/v1/processos/{chave de RP}    200 OK   404
GET /api/v1/interacoes?clienteChave=<cliente de RP>
                                       200 (20) 404
```

### 12.2 O defeito nº 4 encontrado exercitando a API

O documento 23 registrou três defeitos que só apareceram com a API de pé. Este é o quarto, e é a
dívida **D-2** reaparecendo num lugar novo:

> `SUM` sobre uma propriedade com conversor de valor **não traduz para SQL**. `ValorEstimado` é um
> `Dinheiro`, e somar exige que o EF entenda `.Value.Valor` — acesso a membro dentro de um valor
> convertido. A consulta compila e estoura em tempo de execução com *"The LINQ expression could
> not be translated"*.

**A correção não foi somar na tela.** O agrupamento por fase continua no banco (contagem de
processos e contagem de processos **com** valor), e a soma vira uma segunda consulta que o banco
filtra: só as linhas que **têm** valor, que são as únicas que a soma alcança. Aqui isso é 0,8% dos
processos, e a contagem anterior já disse exatamente quantas linhas são — não é um `ToList()`
cego.

---

## 13. Verificação, com saída real

| Verificação | Resultado |
|---|---|
| `dotnet build` | **0 aviso, 0 erro** |
| `dotnet test` | **331 testes, 331 passando** (143 domínio · 62 arquitetura · 44 aplicação · **25 API** · **57 integração**) |
| Carga completa (`--recomecar`) | **8 min 43 s**, números na seção 1 |
| Reexecução sem `--recomecar` | **contagens idênticas**, zero duplicata (seção 11) |
| Consulta no banco | contagem por entidade e por filial na seção 2 |
| Endpoints novos | nove rotas exercitadas ao vivo, com resposta real (seção 12) |
| Fronteira de empresa nas rotas novas | **provada por HTTP**, ao vivo e por teste (seção 12.1) |

**Os testes novos** (a base era 277):

| Projeto | O que cobre | Novos |
|---|---|---:|
| `Tracbel.Crm.Integracao.Testes/Carga/` | as cinco traduções de domínio aberto para fechado, com os valores REAIS medidos na origem — `FINALIZADO` ao lado de `FINALIZADA`, `64` onde deveria haver classe, a letra `M` sem significado | **44** |
| `Tracbel.Crm.Api.Testes` | as nove rotas por HTTP: fronteira de empresa em cada uma, o agregado que declara ausência de dado, o atraso calculado e a procedência | **10** |

---

## 14. O que isto ensina para a migração completa

**1. Coluna de ponteiro do legado precisa ser medida em COBERTURA antes de virar chave de
junção.** `UltHistorico` existe na modelagem, é elogiado na pesquisa e está preenchido em 13% dos
casos. A primeira execução desta carga produziu 69.521 tarefas pendentes falsas por causa disso, e
o erro só apareceu porque o número saiu na tela. **Nenhuma junção nova deve entrar sem a contagem
de quantas linhas ela alcança.**

**2. O catálogo do legado mede o cadastro, não o uso — e a diferença é de uma ordem de
grandeza.** 980 ações cadastradas, 378 marcadas em uso, **178 usadas**. 4.209 desfechos, **505
lançados**. 62 fluxos, **16 abertos**. Migrar por "em uso" traria o dobro do necessário; migrar
por uso real traz o que a tela precisa oferecer. A regra vale para toda a migração completa.

**3. O que a tela do protótipo pede e o dado não tem precisa aparecer na API, não num
documento.** Sete das treze métricas do protótipo não se sustentam. Escrevê-las aqui e deixar a
tela mostrar zero seria repetir o defeito; devolvê-las como `metricasSemDado` com a contagem
medida faz a tela dizer "sem dado" com o motivo do lado.

**4. Domínio fechado que não tem o valor de que o dado precisa é um achado, não um obstáculo.** Os
16.422 processos de fluxos que não são de venda ficam Abertos porque `SituacaoDoProcesso` não tem
`Encerrado`. Empurrá-los para o valor mais próximo teria escondido a lacuna; deixá-los abertos e
contá-los produziu a recomendação da seção 3.2.

**5. A carteirização multi-linha atravessou intacta, e ela é o ativo.** 49% dos clientes
carregados estão em duas ou mais carteiras. Nenhum dos dois produtos de mercado que o documento 02
avaliou entrega isso de fábrica, e o CRM novo entrega no dia um porque o modelo foi desenhado para
isso.

**6. Calcular o derivado é melhor do que copiá-lo — quando o derivado do legado está quebrado.**
A data de último contato copiada teria alcançado 14.496 vínculos; calculada das interações,
alcançou **39.589**. A diferença não é técnica: é uma tabela de configuração que quatro
departamentos nunca povoaram, e uma procedure que parou de rodar em agosto de 2025.

**7. Duas etapas separáveis é o formato que a migração completa vai usar.** Cadastro e
relacionamento têm volumes, janelas e riscos diferentes: 49 segundos contra 8 minutos, 24 mil
linhas contra 270 mil. Rodá-las por bandeira separada no ensaio é o que permite rodá-las em
janelas separadas na virada.
