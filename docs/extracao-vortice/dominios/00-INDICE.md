# Domínios reais das colunas de código do Vórtice CRM

Gerado por `scripts/vortice-extracao/05-dominios.ps1` em 2026-09-02, sobre o banco de produção.

Arquivo de dados: **`valores.csv`** — 379 colunas, 1.115 valores distintos, com contagem e percentual.
Descartes registrados em `../_raw/dominios-descartados.csv` (432 colunas: tabela vazia, tabela de
backup, ou mais de 30 valores distintos).

---

## Por que este arquivo existe

**O banco `CRM` não tem uma única `CHECK CONSTRAINT`.** Zero, em 767 tabelas. Não há tipos
enumerados, não há tabela de domínio para a maioria das colunas de código, e o dicionário de dados
da aplicação (`GE_Col`, 217 linhas) cobre uma fração mínima das 10.744 colunas.

Isso significa que **os valores válidos de cada coluna de código existem apenas dentro do binário
do Vórtice**. Para quem vai substituir o sistema, a única fonte confiável de "quais valores esta
coluna aceita" são os próprios dados. Este arquivo é essa fonte.

**Critério de seleção.** Entraram colunas de tabelas dos prefixos `IV_`, `GE_`, `EXT_`, `IVS_`,
`IVF_`, `DMN_` e `GEP_` (excluindo as ~200 tabelas `IV_Q_*` de questionário e as de backup/teste)
que sejam `char`/`varchar` de até 3 posições **ou** cujo nome comece por `Ind`, `Tipo`, `Status`,
`Situacao`, `Classe` ou `Flag`. Só permaneceram as que têm **até 30 valores distintos**. Tabelas
acima de 3 milhões de linhas foram amostradas com `TOP (500000)` — a coluna `amostrada` do CSV
marca esses casos.

---

## As 40 colunas de domínio que mais importam

Cruzadas, onde aplicável, com `vortice-crm-agent/docs/REGRAS-DE-NEGOCIO.md` e com os relatórios
de `docs/pesquisa/`.

### Pessoa e contato

| # | Coluna | Valores (com participação) | Leitura |
|---:|---|---|---|
| 1 | `GE_Pessoa.Status` | `P` 63,8% · `A` 33,2% · *(vazio)* 1,8% · `S` 1,2% · `F` 0,05% · `O` 0,02% · `I` 0,004% | **Não é A/I.** `P` = *prospect* (a maioria da base!), `A` = ativo, `S` = suspenso, `F`/`O`/`I` residuais. Confirma `REGRAS-DE-NEGOCIO.md` §4. Filtrar por `Status='A'` esconde dois terços da base. |
| 2 | `GE_Pessoa.FisicaJuridica` | `J` 63,6% · `F` 35,0% · *(nulo)* 1,4% · `0` 32 linhas · `O` 1 linha | O valor `0` e o `O` são sujeira de importação. |
| 3 | `GE_Pessoa.Sexo` | `J` 34,8% · `M` 33,0% · *(nulo)* 31,5% · `F` 0,7% · `O` 10 linhas | **`J` em coluna de sexo** = pessoa jurídica. O campo carrega dois conceitos; 31,5% nulo. |
| 4 | `GE_Contato.NivelDecisao` | `N` 35,6% · `D` 33,8% · `I` 16,3% · *(nulo)* 14,4% | `D` = decisor, `I` = influenciador, `N` = não decisor. Dado de qualificação com boa cobertura (85,6%). |
| 5 | `GE_Contato.Posicionamento` | `0` 65,8% · *(nulo)* 21,3% · `+` 12,6% · `-` 0,4% | Sentimento do contato: `+` favorável, `-` contrário, `0` neutro. Só 13% classificado. |
| 6 | `GE_Contato.EstadoCivil` | *(nulo)* 79,2% · *(vazio)* 13,7% · `C` 5,9% · `S` 0,7% · `O`/`D`/`V` residual | 93% sem informação — campo praticamente morto. |
| 7 | `GE_PessoaClasse.Classe` | `FILIAL`, `FUNCIONÁRIOS`, `PARCEIROS`, `FALECIDO`, `REDE CONCESSIONÁRIOS`, `SAM` | Classificações especiais de pessoa. `FALECIDO` como classe é relevante para LGPD. |
| 8 | `GE_PessoaRelacao.TipoRelacionamento` | 14 valores, de `Vinculado/Principal` e `Relacionado Negócio/Principal` a `Pais`, `Filhos`, `Cônjuge`, `Sócios`, `Cunhado(a)` | Mistura relacionamento **societário** com **familiar** na mesma coluna. Há duas entradas com o mesmo texto `Relacionado Negócio/Principal`, o que indica duplicidade de cadastro. |
| 9 | `GE_Email.IndPreferencial` / `IndUsoMkt` / `IndUsoFiscal` / `IndUsoPessoal` / `IndUsoProfissional` | cada uma `1`/`0`/*(nulo)* | Cinco flags independentes de finalidade do e-mail — a base de consentimento de marketing. `IndUsoMkt` é o campo de opt-in de fato. |
| 10 | `GE_PessoaFone.INDWHATSAPP` | *(nulo)* maioria · `0` · `1` | O CRM sabe quais telefones são WhatsApp, mas **não há nenhuma regra de Message Center usando canal que não seja e-mail**. |

### Motor BPM

| # | Coluna | Valores | Leitura |
|---:|---|---|---|
| 11 | `IV_Processo.Status` | 20+ valores livres: `EM ABERTO` 483.522 · *(vazio)* 437.694 · `Cobrança Finalizada` 55.336 · `CANCELADO` 41.390 · `EM ANDAMENTO` 27.241 · `FINALIZADO` 18.416 · `A iniciar` 17.944 · `FATURADO` 12.850 · `FINALIZADA` 11.563 · … | **Vocabulário não normalizado.** Convivem `FINALIZADO` e `FINALIZADA`, `CANCELADO` e `CANCELADA`, e 437 mil linhas com status em branco. O status vem de `IV_ProcResultado`, ou seja, cada modelo de processo inventou o seu texto. Qualquer relatório que filtre por status precisa de uma lista de sinônimos. |
| 12 | `IV_Processo.Fase` | `* Não iniciado *` 662.562 · `Monitoramento` 165.791 · `Prospecção` 90.113 · `COBRANÇA` 59.887 · `Afericao` 50.505 · `Finalizado` 37.900 · … | Mesmo problema: texto livre, com e sem acento (`Afericao`), com e sem caixa alta. O valor sentinela `* Não iniciado *` cobre **56% dos processos**. |
| 13 | `IV_Processo.Realizado` | `1` 1.075.720 · `0` 108.204 · *(vazio)* 6.844 | 90% dos processos estão marcados como realizados. |
| 14 | `IV_Agenda.Realizada` | `S` 96,3% · `N` 3,7% (35.662 abertas) | A carga de trabalho pendente do CRM inteiro são ~35 mil agendas. |
| 15 | `IV_Agenda.Classe` | `O` 66,2% · `X` 16,0% · `T` 14,1% · `P` 3,4% · `C` 0,3% · `W`/`E`/`R`/`M` residuais | Espelha `IV_Acao.Classe`: `O` = operacional (ex.: "ABRIR OS IMPLEMENTO"), `X` = teste/obsoleta, `T` = acompanhamento, `P` = pós-venda, `W` = web. **`X` com 154.583 agendas** é volume real produzido por ações marcadas como teste. |
| 16 | `IV_Agenda.Status` | `C` 85,6% · *(nulo)* 12,1% · `P` 1,3% · *(vazio)* 1,0% | `C` = concluída, `P` = pendente. 13% sem status apesar de 96% "realizada" — as duas colunas não concordam. |
| 17 | `IV_Agenda.TipoAgendamento` | `A` 84,6% (automático) · `M` 14,7% (manual) · *(nulo)* 0,7% | **85% das agendas são geradas pelo motor `IV_AcaoAuto`, não por decisão humana.** É a medida mais direta de quanto o sistema é dirigido por automação. |
| 18 | `IV_Agenda.TarefaCompromisso` | `T` 99,7% · `N` 0,26% · `C` 0,05% | O CRM diferencia tarefa (`T`) de compromisso (`C`) — na prática, tudo é tarefa. |
| 19 | `IV_Agenda.TipoAcesso` | *(nulo)* 72,5% · `P` 27,5% | `P` = privado. Um quarto das agendas é restrito. |
| 20 | `IV_Historico.Natureza` | `A` 57,6% (1.430.590) · `R` 42,5% (1.055.097) · `M` 75 | `A` = agendamento, `R` = realização. O histórico guarda os dois lados do ciclo; `M` (75 linhas) é resíduo. |
| 21 | `IV_ProcDado.AtivoReceptivo` | `R` 65,4% · `A` 30,8% · *(nulo)* 3,9% · `M` 75 | `R` = receptivo (cliente procurou), `A` = ativo (empresa procurou). Dois terços do movimento é receptivo. |
| 22 | `IV_Acao.EmUso` | `N` 61,4% (602) · `S` 38,6% (378) | **Só 378 das 980 ações estão em uso.** |
| 23 | `IV_Acao.Classe` | `O` 672 · `X` 226 · `T` 42 · `P` 31 · `W` 5 · `?` 2 · `R` 2 | Existe literalmente a classe `?`. |
| 24 | `IV_Acao.Exige*` (`Depto`, `Motivo`, `Produto`, `Resposta`) | `N` em **100%** das 980 ações | Quatro mecanismos de obrigatoriedade do produto que a Tracbel **nunca ativou**. `ExigeDetalhe` (28,5% `S`) e `ExigeFormulario` (0,9% `S`) são as únicas usadas. |
| 25 | `IV_CodProcesso.TipoProcesso` | `VENDA` 26 · `PROSPECÇÃO` 11 · `OUTRO` 11 · *(nulo)* 9 · `SERVIÇOS` 3 · `AFERIÇÃO` 1 · *(vazio)* 1 | 10 dos 62 modelos não têm tipo — não entram em nenhuma segmentação por tipo de fluxo. |
| 26 | `IV_AcaoAuto.Exigida` | `S` 5.336 · `N` 622 | 90% das ações automáticas geram tarefa **obrigatória**. |
| 27 | `IV_AcaoAuto.TipoAgendamento` | `A` 5.388 · `C` 570 | Coerente com o 84,6% de agendas automáticas. |
| 28 | `IV_AcaoAuto.AtivoReceptivo` | `A` em **100%** das 5.958 regras | Toda ação automática nasce como "ativa", mesmo quando o processo é receptivo. |
| 29 | `IV_ProcSt.StatusOrdem` | 29 valores numéricos: 5, 10, 15, 20, 30, 40, 45, 50, 55, 60, 65, 70, 80, 90… | A ordenação das fases é feita por número com passo 10 — o padrão clássico de "deixar espaço para inserir". |
| 30 | `IV_ClientePropr.Ativo` | `S` 97,7% · `N` 2,3% · *(nulo)* 4 | A tabela que dispara o único trigger do banco. |

### Formulários e objetos dinâmicos

| # | Coluna | Valores | Leitura |
|---:|---|---|---|
| 31 | `IV_Questao.TipoDado` | 10 valores: `C`, `M`, `V`, `S`, `N`, `L`, `D`, `X`, `R`, `H` | O sistema de tipos dos formulários. `C` texto, `N` numérico, `D` data, `L` lista, `M` memo, `V` valor, `H` hora — os demais precisam ser confirmados na aplicação. **Este é o contrato que o sistema novo precisa reproduzir para migrar 2.303 questões.** |
| 32 | `IV_Formulario.EmUso` | `S` · `I` · `N` | Três estados, não dois: existe `I` (inativo?) além de `S`/`N`. |
| 33 | `IV_Formulario.Layout` | `F` · `S` | Formulário (`F`) ou script (`S`). |
| 34 | `GE_ObjDinamico.Tipo` | `TESTE` 168 · `LINHA_INFORMAÇÃO` 25 · `PESQUISA_PESSOA` 3 | Os 168 `TESTE` são as **regras de validação de fluxo** escritas em SQL dentro do banco. |
| 35 | `IV_ListSQL.Tipo` | `QUESTAO` 278 · `PROPRIED` 54 · `GLBPAR` 28 | Onde as listas de valores das telas são resolvidas — mais SQL guardado em tabela. |

### Integração e ERP

| # | Coluna | Valores | Leitura |
|---:|---|---|---|
| 36 | `GEP_Import.Status` | `E` 94,4% · `X` 3,5% · *(nulo)* 2,2% | `E` = executado, `X` = erro. **3,5% da fila de importação está em erro permanente** e ninguém limpa. |
| 37 | `GEP_Import.Separador` | `~` 76,2% · `;` 22,9% · `\|` 0,9% | Três formatos de linha convivendo na mesma fila genérica. O trigger de auditoria usa `\|`. |
| 38 | `EXT_Titulo.Status` | `Pago` 53,5% · `B` 43,5% · `A` 2,2% · `Compensado` 0,8% · `A Depositar` · `Devolvido` · `Cancelado` | **Mistura código de uma letra com texto por extenso na mesma coluna** — sintoma de duas integrações diferentes gravando na mesma tabela em épocas diferentes. |
| 39 | `EXT_OS.SITUACAO` / `TipoOS` / `TIPOSERVICO` | `A` 67,1% · `F` 32,6% · `C` 0,3%; **`TipoOS` e `TIPOSERVICO` vazios em 100%** | A ordem de serviço chega sem tipo. Combinado com `Dtaabertura` 100% nula e última alteração em **25/05/2024**, confirma que a integração de OS está morta. |
| 40 | `EXT_NFS.TipoVenda` / `TIPOPEDIDO` | `NORMAL` em 100% das 390.755 · `TIPOPEDIDO` **nulo em 100%** | Colunas que existem para segmentar faturamento e nunca receberam valor. |

---

## Cinco padrões que atravessam todos os domínios

1. **Ausência total de constraint.** Nenhuma `CHECK`, nenhuma tabela de domínio para a maioria das
   colunas. O banco aceita qualquer valor; a integridade é responsabilidade exclusiva da aplicação.
   Nas colunas de texto livre (`IV_Processo.Status`, `IV_Processo.Fase`) isso já produziu
   vocabulário duplicado em produção.

2. **Nulo, vazio e zero significam coisas diferentes — e ninguém padronizou.** Em
   `GE_Pessoa.Status` convivem *(vazio)* (2.126 linhas) e valores reais; em `GE_Pessoa.IndContribICMS`
   convivem `N`, *(nulo)* e `0` para a mesma ideia. Toda migração de dados precisará de uma regra
   explícita de coalescência por coluna.

3. **Flags mortas em massa.** Dezenas de colunas têm **um único valor em 100% das linhas**:
   `EXT_NFS.IndEstorno` = `0`, `EXT_Titulo.IndAtivo` = `1`, `DMN_Doc.Status` = `N`,
   `GE_Usuario.INDSILO` = `0`, `IV_Acao.ExigeProduto` = `N`, `IV_AcaoAuto.AtivoReceptivo` = `A`.
   São recursos do produto que a Tracbel nunca ligou — **e que não precisam ser reimplementados**.
   Das 379 colunas analisadas, **112 (29,6%) têm um único valor em toda a tabela**; outras 252 têm
   entre 2 e 8 valores, e essas são as candidatas naturais a enum no modelo novo.

4. **Códigos de uma letra sem legenda.** `IV_Operador` tem 20+ colunas de permissão com o mesmo
   alfabeto `G`/`E`/`D`/`N` (provavelmente Grupo / Equipe / Departamento / Nenhum, a confirmar com
   a Vórtice). É o modelo de autorização por agenda, e não está documentado em lugar nenhum.

5. **A mesma coluna carrega dois conceitos.** `GE_Pessoa.Sexo` guarda `J` para pessoa jurídica;
   `EXT_Titulo.Status` guarda código e descrição. São decisões de modelagem que precisam ser
   desfeitas na migração, não copiadas.

---

## Como usar `valores.csv`

| Coluna | Conteúdo |
|---|---|
| `tabela`, `coluna` | identificação |
| `tipo` | tipo SQL da coluna |
| `valor` | o valor encontrado; `(NULO)` e `(VAZIO)` são marcadores explícitos |
| `qtd` | número de linhas com esse valor |
| `pct` | participação percentual dentro da coluna |
| `amostrada` | `True` quando a tabela tem mais de 3 milhões de linhas e a contagem veio de `TOP (500000)` |

Para achar candidatas a enum no modelo novo: filtrar por colunas com 2 a 8 valores e nenhum
`(NULO)` acima de 5%. Para achar campo morto: filtrar por colunas com um único valor.
