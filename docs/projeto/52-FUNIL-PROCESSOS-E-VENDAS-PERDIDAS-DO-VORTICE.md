# 52 — Funil, processos e vendas perdidas pelo Vórtice

> **Versão 1.0 · 27/09/2026 · decisões tomadas, PR 1 (backend) em implementação.**
> **Base:** plano `plano-funil-vortice-2026-09-27` (desenho só de leitura; medições no Vórtice por sessão
> somente leitura e `NOLOCK`, e no CRM de produção por ODBC somente leitura — **só agregados**).
> **Especificação do estágio:** o extrator do BI `(Extrator) Funil de Vendas - Tracbel Agro.qvs`
> (pasta 360, lida em `INVENTARIO.md` b.12).
> **Errata correspondente:** [41 §1.2 — D-12 parcial](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md#12-errata-de-27092026--d-12-parcial--histórico-do-funil-liberado-em-27092026).

Marcação: **[M]** medido; **[D]** decidido pelo Ricardo; **[P]** proposta registrada.

---

## 0. As quatro coisas que este documento diz

1. **O estágio do funil vem do código de resultado do `IV_HISTORICO`, como no BI** [D 27/09]. Não vem da fase
   do BPM (`IV_Processo.Fase`), que é outro eixo e é da API Gestão de Negócios (§7).
2. **O funil entra inteiro, com os prospects** [D 27/09]: `processo.EstagioDoProcesso` não depende de
   `processo.Processo` nem de `comercial.Cliente`. 62% do funil não tem cliente no CRM; a rotina **não cria**
   cliente, carteira nem usuário — só liga ao que já existe.
3. **A venda perdida entra com todo o histórico, desde 2012, de oito formulários e quatro complementos** [D 27/09],
   com o formulário de origem e o papel de cada resposta. **Só a principal conta.**
4. **A carga legada `CargaDeProcessoDoVortice` continua congelada** como referência. O que volta a ler o Vórtice é
   uma carga **nova**, no padrão da `CARTEIRAS_VORTICE` (#236): planeja só com leitura, simula, toma a
   `TravaDeFluxo` e deixa a trilha em `integracao.RegistroDeOrigem`.

---

## 1. Por que reescrever, e não religar

A carga legada (`Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs`, 1.536 linhas, mais `.VendaPerdida.cs`, e o
leitor `LeitorDeCargaDoVortice.Processo.cs`) traz usuário, linha, carteira, território, catálogos, vínculo,
processo, tarefa, interação, venda perdida e faturamento. Religá-la como está quebraria oito coisas [M]:

| # | O que quebra | Por quê |
|---:|---|---|
| 1 | o cliente não casa | ela casa pela `ChaveExterna`, apagada na sanitização (doc 38); a #236 casa pelo CPF/CNPJ |
| 2 | **3.280 processos** são recusados | `DtaInclusao` nula; o BI usa `COALESCE(DtaInclusao, DtaRealizacao)` |
| 3 | contas duplicadas | e-mail `.invalid` briga com o `ResolverDono` da #236 |
| 4 | carteira em dobro | ela recria carteira, que já é da `CARTEIRAS_VORTICE` |
| 5 | faturamento dentro da carga | o faturamento já saiu (#233) |
| 6 | apaga cadastro | `--recomecar` apaga cliente, carteira e usuário |
| 7 | último contato inflado | `UltimaInteracaoEm` sai de qualquer interação |
| 8 | perda pela metade | a venda perdida sai só de 3 formulários, sem origem |

**Veredito:** reescrever. Peças reaproveitadas: `SaneamentoDeProcesso.Situacao/Codificar`,
`SaneamentoDeVendaPerdida`, `DocumentoDoVortice`. O motivo da D-12 fica resolvido: o faturamento já saiu, e a
rotina nova não cria cadastro — só lê fatos de quem existe.

---

## 2. O modelo

### 2.1 `processo.EstagioDoProcesso` — uma linha por processo do Vórtice por estágio alcançado

| Situação do processo | processos | Lead cum. | Cobertura | Negoc. | Pedido | Fat. |
|---|---:|---:|---:|---:|---:|---:|
| Casado com cliente do CRM (#236) | 24.855 | 13.280 | 12.652 | 5.712 | 4.432 | 3.179 |
| Ausente da SA1 (prospect com documento) | 20.228 | 7.567 | 6.936 | 1.471 | 792 | 351 |
| Sem documento | 17.586 | 8.336 | 7.463 | 1.584 | 783 | 470 |
| Carteira de filial fora do CRM | 5.843 | 1.090 | 742 | 83 | 57 | 26 |
| Fora da área | 1.503 | 634 | 514 | 255 | 217 | 166 |
| Sem carteira MAQ_NOVOS | 5.660 | 3.756 | 592 | 190 | 132 | 100 |
| Teste / documento inválido | 342 | 110 | 104 | 53 | 40 | 13 |
| **Total** | **76.017** | **34.773** | **29.003** | **9.348** | **6.453** | **4.305** |

**Colunas:**

| Grupo | Coluna | O que é |
|---|---|---|
| identificação | `EmpresaId` | a filial do processo (`IV_PROCDADO.NroEmpresa`), fronteira de acesso com filtro global |
| | `NumeroDoProcessoNaOrigem` | o número no Vórtice — a chave combinada com a API GN (§7) |
| | `TipoDeProcessoNaOrigem` | 31, 41 ou 50 (`CK`) |
| ligações opcionais | `ProcessoId`, `ClienteId`, `CarteiraId`, `ResponsavelId` | nulas quando o CRM não tem o registro; `ProcessoId` fica nulo até a onda 2 |
| abertura e desfecho | `AbertoEm`, `AberturaDeduzida` | `COALESCE(DtaInclusao, 1º andamento)`; deduzida quando a inclusão é nula |
| | `Desfecho`, `DesfechoEm` | a situação pela regra da carga antiga (`SaneamentoDeProcesso.Situacao`) |
| estágio | `Estagio` (`CK`), `AlcancadoEm` | o estágio e o **primeiro** resultado aceito (do processo ou do pai DNA) |
| conciliação | `UltimaAcaoDaEtapaEm` | o `DTA_ETAPA` do BI (máximo das ações geradoras da etapa) — só para conciliar |
| origem | `HerdadoDoProcessoDna`, `NumeroDoProcessoDnaNaOrigem` | se o resultado que abriu o estágio veio do pai DNA, e de qual |
| | `ResultadoQueAbriu` | o código de resultado que abriu o estágio |
| | `PelaEntradaDigital` | Lead: teve 1278; Qualificado: teve 3803; da Cobertura em diante: entrou por 1278 ou 3803 |

**Índices:** único (`NumeroDoProcessoNaOrigem`, `Estagio`); (`EmpresaId`, `Estagio`, `AbertoEm`);
(`EmpresaId`, `Estagio`, `AlcancadoEm`); `ResponsavelId`; `CarteiraId`; `ClienteId`; `ProcessoId`.
**Volume:** ~113 mil linhas com Lead e Qualificado cumulativos (~69 mil na semântica do BI).

### 2.2 `integracao.ClassificacaoDeResultadoDoVortice` — a lista que o funil e o último contato leem

Uma linha por código de resultado: `CodigoNaOrigem`, `Estagio` (o **maior** estágio que o código prova, ou nulo),
`ContaComoContato`, `Fonte`. Semeada pela migração com:

| Estágio | Códigos | Fonte |
|---|---|---|
| Lead | 1278 (Realizou Contato — digital) | extrator, l. 257 |
| Qualificado | 3803 (Lead Qualificado) | extrator, l. 384 |
| Cobertura | 250, 252, 254, 255, 260, 263, 265, 267, 299, 300, 304, 305, 306, 307, 570, 1286, 3225, 3227, 3639, 3640, 2605, 1929, 2547, 2553, 2554 | extrator, l. 520–525 |
| Negociação | 2563, 2564, 2565, 2566, 2568, 3234, 3572, 3573, 2612, 3223, **2607, 2609, 2610** | extrator, l. 666–669; os três em negrito **também** em Cobertura [D 27/09] |
| Pedido | 2548, 3231, 3232, 3239 (Venda Aprovada), 3663 | extrator, l. 803 |
| Faturamento | 2529, 2530, 3440, 3494 | extrator, l. 936 |

As listas do extrator são aninhadas (a da Cobertura contém a da Negociação, que contém a do Pedido, que contém a do
Faturamento) — **exceto** 2607/2609/2610, que o extrator põe na Negociação e esquece na Cobertura. Guardar o maior
estágio de cada código e contar "alcançou E" como "tem código de estágio ≥ E" reproduz as listas e conserta a
exceção. `ContaComoContato` é a lista da `BI_CARTEIRA_VN` (l. 86–87), **53 códigos** distintos [M] — a mesma
constante `ResultadosQueContamComoContato` do PR #244 (já na main); os testes prendem as duas à view e uma à outra.

### 2.3 `processo.VendaPerdida` ganha quatro colunas

- `FormularioDeOrigem` varchar(40) com `CK` na lista dos doze formulários;
- `Papel` — `Principal`, `Complemento` ou `Duplicata` (`CK`); **só `Principal` conta**;
- `VendaPerdidaPrincipalId` — a principal de quem é complemento ou duplicata (autorreferência, `CK` de coerência);
- `NumeroDoProcessoNaOrigem` — o processo no Vórtice, que liga a perda ao funil sem depender de `Processo`.

### 2.4 As migrações

| Migração | O que faz |
|---|---|
| M1 `EstagioDoProcessoDoVortice` | cria as duas tabelas, semeia a classificação e regenera o `CK` de `Entidade` |
| M2 `FormularioDaVendaPerdida` | acrescenta as quatro colunas da venda perdida |
| M3 `RotinaDosProcessosDoVortice` | insere a rotina `PROCESSOS_VORTICE` e atualiza a descrição da conexão 4 (Vórtice) |

---

## 3. A regra do estágio

**Universo:** processos de `IV_PROCDADO` com `CodProcesso` 31 ("Prospecção Clientes MAQ IM"), 41 ("Venda
Máquina/Implemento/AMS") e 50 ("Venda Equipamento Tracbel Agro"), com `AbertoEm` a partir da **janela única de
01/11/2023** (início do FY24) [D 27/09]. Trocar por 01/01/2024 não muda nenhum estágio [M]: os 2.071 processos de
nov–dez/2023 são do tipo 31 e nenhum tem resultado aceito.

**A regra, escrita** (`RegraDoEstagio`, pura, com teste de unidade para cada linha):

1. **Cumulativa.** O processo alcança o estágio E quando tem ao menos um resultado aceito cujo estágio é ≥ E.
   Lead e Qualificado também são cumulativos [D 27/09]: quem chegou a Cobertura conta como Lead e como Qualificado.
2. **`AlcancadoEm`** é a data do **primeiro** resultado que alcança E. "Mais recente" do BI = "alcançou alguma vez".
3. **DNA.** O pai que tem filho DNA (31/41/50) **sai**; o filho **herda** os resultados do pai, com a data mínima.
   A herança é transitiva (o neto herda do filho, que herdou do pai), e só o último da cadeia fica. Pai de outro
   tipo não transmite nada — o BI filtra o histórico por 31/41/50. `ProcessoDNA` em **ciclo** (A→B, B→A) é tratado
   como processo sem pai: sem isso os dois se substituiriam e sairiam do funil.
4. **Inclusão nula.** `AbertoEm = COALESCE(DtaInclusao, 1º andamento)`, e `AberturaDeduzida` marca o caso. Data
   de abertura **absurda** (antes de 2000 ou depois de agora + 1 dia) é recusada e vale a seguinte; sem nenhuma crível,
   o processo fica pendente com `ABERTURA_COM_DATA_INVALIDA`.
5. **Data futura.** Resultado com data depois de agora + 1 dia é recusado (há digitação em 2103 no Vórtice).
6. **Uma linha por processo por estágio** — o fan-out do BI (um registro por departamento do histórico da família
   DNA) não existe aqui.

| Estágio | Antes (próprio, com pais) | Depois (DNA, 1 por processo) | Linhas do BI (fan-out) | BI original linhas / processos |
|---|---:|---:|---:|---:|
| Lead | 13.519 | 13.521 | 27.334 | 9.898 / 5.044 |
| Qualificado | 6.405 | 6.401 | 18.653 | 6.782 / 2.793 |
| Cobertura | 31.265 | 29.003 | 52.123 | 52.158 / 29.027 |
| Negociação | 9.504 | 9.348 | 23.475 | 23.507 / 9.371 |
| Pedido | 6.538 | 6.453 | 19.758 | 19.762 / 6.454 |
| Faturamento | 4.304 | 4.305 | 14.777 | 14.781 / 4.306 |

**DNA [M]:** saem 3.140 pais (3.059 com estágio próprio); ficam 4.086 filhos; só por herança entram 719 em
Cobertura, 274 em Pedido e 15 em Faturamento; há 6 netos e 351 filhos de pai de outro tipo (181 com o pai fora de
31/41/50).

**Cumulativo [M]:** 34.773 → 29.296 → 29.003 → 9.348 → 6.453 → 4.305. Na semântica do BI, Qualificado → Cobertura
daria 453%. O subfunil digital fica visível por `PelaEntradaDigital`: 1278 = 8.274 → 3803 = 1.096 → 803 chegam a
Cobertura → 19 faturados. `PelaEntradaDigital` é **`entrou por`**, e não `teve`: o 1278/3803 precisa ter data **menor ou
igual** à `AlcancadoEm` do estágio — o processo que chegou à Cobertura por visita e só depois recebeu o contato
digital não entrou pelo digital (revisão de 27/09).

**Data da etapa:** `AlcancadoEm`. O `DTA_ETAPA` do BI (o máximo das ações geradoras) é mal definido — na
Cobertura, 23% ficam sem data e 50% diferem em mais de 30 dias [M] — e fica em `UltimaAcaoDaEtapaEm` só para
conciliar.

**Herança anterior à abertura — atenção para a visão por fluxo (PR 2).** O pai aberto **antes** da janela não entra
no funil, mas transmite: o filho da janela herda o estágio com a `AlcancadoEm` do pai, **anterior à própria abertura
(e à janela)**. Na **coorte** (pela `AbertoEm`) o processo conta no período do filho; no **fluxo** (pela
`AlcancadoEm`) esse estágio cai num período anterior — e, se o PR 2 filtrar o fluxo pela janela, some dele. O teste
`O_pai_aberto_antes_da_janela_transmite…` prende o comportamento; o PR 2 decide como mostrar.

**`ProcessoPai` × `ProcessoDNA` [M, 27/09, Vórtice, só agregados] — decisão aberta.** O BI herda **só** pelo
`ProcessoDNA`, e a regra também (D de 27/09). Mas há processos com `ProcessoPai` e **sem** DNA:

| Tipo | processos | com pai | filho DNA | pai sem DNA | pai sem DNA e o pai tem resultado aceito | tem os dois, pai ≠ DNA |
|---|---:|---:|---:|---:|---:|---:|
| 31 | 39.754 | 124 | 124 | 0 | 0 | 15 |
| 41 | 23.840 | 5.351 | 2.802 | 2.549 | 2.238 | 206 |
| 50 | 42.445 | 4.454 | 1.332 | 3.122 | 2.752 | 313 |

Dos 5.671 filhos por `ProcessoPai` sem DNA, 5.292 são do mesmo tipo do pai (41→41: 2.483; 50→50: 2.809), todos na
janela; desses 5.292, em 4.019 **o pai e o filho têm resultado aceito** — o mesmo negócio pode estar contando duas
vezes, ou são negócios distintos do mesmo cliente (o desmembramento de um pedido). Nenhum processo tem DNA sem
`ProcessoPai`, e os 534 que têm os dois apontando para processos diferentes indicam que o `ProcessoDNA` aponta a
**raiz** da família (o neto aponta o avô) — por isso a herança transitiva basta. Nenhum `ProcessoDNA` em ciclo de dois
foi achado. A rotina **não muda a regra** — seria reabrir a decisão —, mas lê o `ProcessoPai`, anota na trilha de cada
processo (`filho do processo N por ProcessoPai, sem vínculo DNA`) e conta no relatório. **Pergunta ao Ricardo:** o filho
por `ProcessoPai` sem DNA é o mesmo negócio (herda e o pai sai, como o DNA) ou outro negócio (fica como está)? A
consulta está na §11.

**FY26 [M]:** coorte (Cob., Neg., Ped., Fat.) 8.001 / 3.108 / 2.199 / 1.347; fluxo 8.833 / 3.758 / 2.563 / 1.835.
O Funil usa a coorte, com chave para o fluxo [D 27/09]; Performance de CEN, alertas e vendas do período usam o
fluxo (PR 2).

**Filial fora do CRM:** o processo cuja filial (`IV_PROCDADO.NroEmpresa`) não é filial ativa do CRM **não entra**
[D 27/09] — fica pendente em `RegistroDeOrigem` com o motivo. A medição de 27/09 (5.843) foi feita pela filial da
carteira; a rotina usa a filial do processo, que é a de onde vem o `EmpresaId`.

---

## 4. A venda perdida, formulário por formulário

| Formulário | respostas | período | FY24 | FY25 | FY26 | preço JD | cliente CRM |
|---|---:|---|---:|---:|---:|---:|---:|
| antigo `IV_Q_VENDA_PERDIDA` | 1.511 | 2012-01..2023-12 | 1 | 0 | 0 | 1.001 | 767 |
| `_MAQIMP` | 313 | 2024-02..2025-08 | 98 | 215 | 0 | 281 | 179 |
| `_JDE` | 296 | 2012..2016 | 0 | 0 | 0 | 216 | 121 |
| `_PROD` | 286 | 2022-04..2024-05 | 77 | 0 | 0 | 230 | 193 |
| `_FY25` | 264 | 2025-07..2026-09 | 0 | 57 | 207 | 259 | 107 |
| `VP_SEM_PARTICIPACAO` | 96 | 2025-08..2026-09 | 0 | 22 | 74 | 96 | 30 |
| `_IMPLEM` / `_IMPL` / `VP_*` (4) | 125 / 31 / 28 | | | | | | |
| **fora:** `_MANITO` (Colorado) | 6 | | | | | | |

**Regras [D 27/09]:**

1. **Todo o histórico** (~3,2 mil respostas) entra; a tela filtra o período.
2. **Duplicatas:** o `_JDE` gêmeo do antigo vira **Duplicata** (300 pares medidos) [M]; no par `_FY25` ×
   `SEM_PARTICIPACAO` (15 pares) [M], o **FY25 é o principal**, com participação "Não", e a resposta do
   `SEM_PARTICIPACAO` vira Duplicata. **O par** é a mesma pessoa no mesmo processo; sem processo nos dois, a mesma
   pessoa no mesmo dia. Entre candidatos, vale o mais próximo no tempo e, no empate, o menor `SeqQuestionario`.
3. **Complementos:** os `VP_*` (`VP_TRATOR`, `VP_COLHEITADEIRA`, `VP_PLANTADEIRA`, `VP_COLHEDORA`) acompanham o FY25
   pela mesma regra de par; o solto entra como principal, com o motivo `NAO_INFORMADO_NA_ORIGEM`.
4. **Participação:** `SEM_PARTICIPACAO` = "Não"; antigo, `_PROD` e `_IMPLEM` pelo campo; o resto "Não informado".
5. **Catálogo:** `processo.MotivoDePerda` tem 0 linhas em produção [M]; a rotina garante o catálogo de motivos e os
   de concorrente, tipo de equipamento e revenda, pela mesma codificação da carga antiga.
6. **Filial:** a do processo; sem processo, a do histórico em que o formulário foi preenchido; sem as duas, a do
   cliente do CRM. Filial fora do CRM → pendente. Data de preenchimento absurda (antes de 2000 ou no futuro) →
   pendente com `DATA_DE_REGISTRO_INVALIDA`; quantidade acima de 1.000 máquinas → não declarada (uma), com a correção
   anotada — o `9999999999` da origem derrubava a rodada.
7. **Quem preencheu não é lido:** o login de quem respondeu é dado de pessoa, e a pergunta da perda não precisa dele.
   `ChaveExterna` pelo `SeqQuestionario`.
8. **Só a principal conta — também nas telas que já existiam** (revisão de 27/09): o resumo por motivo e por
   concorrente, o painel do CEN, os indicadores executivos e a cobertura do motor filtram `Papel = Principal`; sem
   isso o `_JDE` gêmeo contava em dobro e o preço dele entrava na média.

---

## 5. A rotina `PROCESSOS_VORTICE`

| Item | Como é |
|---|---|
| catálogo | no FIM de `RotinasDoSistema.Todas` — Id = posição + 1 (**8** sobre a main de 27/09; **9** depois das metas, que ficam com o 8) |
| agenda | diária às **06:30**, depois das carteiras (04:30) e do parque (05:30) |
| conexão | exige `VORTICE`; nasce **desligada** — ligar é trazer dado novo para produção |
| modo | `--somente-processos-vortice [--simular]`, bloco próprio no `Program.cs`, fora de `leOVortice` |
| leitura | relê a janela inteira a cada rodada, sem marca d'água (funil 2–3 s; formulários 1 s; histórico 31/41/50 ~295 mil linhas [M]) |
| idempotência | chave (`NumeroDoProcessoNaOrigem`, `Estagio`); `ChaveExterna` pelo `SeqQuestionario`; `RegistroDeOrigem` com o motivo de cada pendência. Duas rodadas sem mudança na origem não alteram nada |
| trava | leitura vazia **ou** queda de mais de 5% em relação ao que o CRM já tem **aborta sem apagar**; estágio sumido num processo presente é removido; processo ausente só sai se não existir mais em `IV_PROCDADO` (a leitura traz todo processo 31/41/50, sem janela). A queda **legítima** (uma filial desativada tira ~7,7%) passa com `--aceitar-queda` — **só no terminal**, fora dos modos da rotina —, e a aceitação fica escrita na execução |
| gravação | em blocos de **processos inteiros** (~113 mil linhas na primeira rodada), cada bloco na sua transação: os estágios de um processo nunca ficam em blocos diferentes. Se um bloco cai, a rotina sai com código 3 e diz `Gravação parcial…; a próxima rodada completa`. As consultas por lista de ids vão em fatias de 1.000 (o SQL Server aceita 2.100 parâmetros, e as vendas perdidas passam de 3 mil). A simulação não abre transação nenhuma |
| execução | cada rodada que grava deixa uma linha em `integracao.ExecucaoDeSincronizacao` (fluxo `VORTICE.FUNIL`): início, fim, contagens e a mensagem — inclusive a queda aceita e a gravação parcial |
| último contato | **não grava** `UltimaInteracaoEm` — é da `CARTEIRAS_VORTICE`, pela regra da BI (PR #244) |

**Motivos de pendência (`RegistroDeOrigem.Motivos`):** `FILIAL_DO_PROCESSO_FORA_DO_CRM`, `SEM_RESULTADO_ACEITO`,
`PAI_SUBSTITUIDO_PELO_FILHO_DNA`, `ABERTURA_COM_DATA_INVALIDA` (processos); `FILIAL_FORA_DO_CRM`, `SEM_DATA_DE_REGISTRO`,
`DATA_DE_REGISTRO_INVALIDA` (venda perdida).

**A volta da M1** (`Down`) apaga a trilha de auditoria de `EstagioDoProcesso` e `ClassificacaoDeResultadoDoVortice`
antes de recriar o CHECK de `Entidade` sem elas — senão a volta falharia com trilha no banco.

---

## 6. Último contato — não é desta frente

A carga do histórico **não grava** `UltimaInteracaoEm`. A regra é a da `BI_CARTEIRA_VN`, na `CARTEIRAS_VORTICE`
(PR #244, decisão de 27/09). Se um dia esta carga gravar, usa a mesma lista (`ContaComoContato`), e o máximo
converge. **A lista tem 53 códigos distintos, não 55** [M].

| Regra | Pessoas [M] |
|---|---:|
| lista da BI, alguma vez | 5.413 |
| lista da BI, desde o FY24 / 360 d / 180 d / 90 d | 3.774 / 3.179 / 2.710 / 2.299 |
| qualquer interação, alguma vez / desde o FY24 / 180 d | 8.145 / 6.393 / 3.670 |
| `DtaUltCtto` / com 180 d | 3.730 / 2.703 |

---

## 7. Fronteira com a API Gestão de Negócios — combinada em 27/09

- chave = número do processo no Vórtice;
- **estágio** = esta frente; **fase do BPM** e situação corrente = a outra sessão (API GN);
- nenhuma soma: uma linha por processo, com o atributo do outro eixo anexado pela chave;
- o Pipeline corrente é da API GN e ganha o selo do estágio; Funil, conversão, Performance de CEN e alertas
  históricos são daqui; venda perdida e consórcio são daqui; pedidos e financiamento, de lá.

---

## 8. Os cinco PRs

| PR | O que entrega |
|---|---|
| **1 — backend** | doc 52 e errata; domínio (`EstagioDoProcesso`, `RegraDoEstagio`, classificação, papéis da venda perdida); M1/M2; leitores; `CargaDoFunilDoVortice` e o modo `--somente-processos-vortice`; a rotina e a M3 |
| 2 — leitura | `GET /api/v1/relatorios/funil-por-estagio?de&ate&base=abertura\|etapa&carteira&responsavel`; vendas perdidas por período e formulário; painel do CEN e executivos; frases da API |
| 3 — front | Funil, Visão 360 e Performance de CEN, com teste de texto proibido |
| 4 — onda 2 | `Processo`, `Tarefa` e `Interacao` só dos clientes casados (24.855 processos), sem `UltimaInteracaoEm` — §12 |
| 5 — front da onda 2 | Agenda, ficha 360 e ficha de oportunidade |

---

## 9. Riscos

1. 62% do funil fica sem cliente no CRM, e a ficha não abre para esses processos.
2. A herança DNA pode inflar irmãos (dois filhos do mesmo pai herdam o mesmo resultado).
3. Os números batem com os **processos distintos** do BI, não com as linhas (o fan-out do BI não existe aqui).
4. Colisão de `Processo.Numero` na onda 2: o índice único empresa+número usa o número do Vórtice, e a numeração
   nascida no CRM vai precisar de outra faixa.
5. As 113 mil linhas são gravadas em blocos; uma rodada que cai no meio converge na seguinte.
6. O dono do processo (`UsuResponsavel`) não foi medido — a rotina casa pelo login e, sem conta, usa o dono da
   carteira; sem os dois, fica nulo.
7. O classificador pode negar leitura ao vivo na estação; a rotina roda no servidor.

---

## 10. Como ligar em produção

1. **Publicar** a versão com as migrações `EstagioDoProcessoDoVortice`, `FormularioDaVendaPerdida` e
   `RotinaDosProcessosDoVortice` — a API as aplica ao subir. A rotina nasce **desligada**.
2. **Conferir a conexão** "Vórtice — sistema legado" em Configurações › Integrações: configurada e testada (a mesma
   das carteiras).
3. **A carteira antes do funil:** a `CARTEIRAS_VORTICE` precisa ter rodado ao menos uma vez — é o de-para dela que liga
   o processo à carteira e, sem conta pelo login, ao dono da carteira. Sem ela o funil entra do mesmo jeito, sem carteira.
4. **Simular no servidor** — `Tracbel.Crm.Carga.exe --somente-processos-vortice --simular` — e conferir os números com
   os das §2, §3 e §4 (Lead cumulativo ~34,8 mil, Cobertura ~29 mil, Faturamento ~4,3 mil; ~3,2 mil respostas de venda
   perdida). A simulação lê o CRM com intenção de leitura e não abre transação.
5. **Ligar a rotina** "Funil e vendas perdidas do Vórtice" (diária às 06:30) ou apertar "Rodar agora". A primeira rodada
   grava ~113 mil linhas em blocos de 2.000; as seguintes só gravam o que mudou.

## 11. Consultas prontas (só agregados, para o Ricardo rodar se quiser conferir)

```sql
-- Processos 31/41/50 e quantos têm DtaInclusao nula (o COALESCE da regra 4)
SELECT d.CodProcesso, COUNT(*) AS processos, SUM(CASE WHEN p.DtaInclusao IS NULL THEN 1 ELSE 0 END) AS sem_inclusao
FROM IV_PROCDADO d WITH (NOLOCK) JOIN IV_PROCESSO p WITH (NOLOCK) ON p.Processo = d.Processo
WHERE d.CodProcesso IN (31,41,50) GROUP BY d.CodProcesso;

-- Pares antigo × _JDE pela regra do par (mesma pessoa, mesmo dia)
SELECT COUNT(*) FROM IV_Questionario a WITH (NOLOCK)
JOIN IV_Q_VENDA_PERDIDA va WITH (NOLOCK) ON va.SEQQUESTIONARIO = a.SeqQuestionario
JOIN IV_Questionario j WITH (NOLOCK) ON j.SeqPessoa = a.SeqPessoa AND CAST(j.DtaRealizacao AS date) = CAST(a.DtaRealizacao AS date)
JOIN IV_Q_VENDA_PERDIDA_JDE vj WITH (NOLOCK) ON vj.SEQQUESTIONARIO = j.SeqQuestionario;

-- ProcessoPai × ProcessoDNA (a tabela da §3, medida em 27/09)
WITH P AS (
    SELECT d.CodProcesso, d.Processo, d.ProcessoPai, d.ProcessoDNA,
           CASE WHEN d.ProcessoPai IS NOT NULL AND d.ProcessoPai <> d.Processo THEN 1 ELSE 0 END AS TemPai,
           CASE WHEN d.ProcessoDNA IS NOT NULL AND d.ProcessoDNA <> d.Processo THEN 1 ELSE 0 END AS TemDna
    FROM IV_PROCDADO d WITH (NOLOCK) WHERE d.CodProcesso IN (31,41,50)
)
SELECT CodProcesso, COUNT(*) AS processos, SUM(TemPai) AS com_pai, SUM(TemDna) AS filho_dna,
       SUM(CASE WHEN TemPai = 1 AND TemDna = 0 THEN 1 ELSE 0 END) AS pai_sem_dna,
       SUM(CASE WHEN TemPai = 0 AND TemDna = 1 THEN 1 ELSE 0 END) AS dna_sem_pai,
       SUM(CASE WHEN TemPai = 1 AND TemDna = 1 AND ProcessoPai <> ProcessoDNA THEN 1 ELSE 0 END) AS pai_diferente_do_dna
FROM P GROUP BY CodProcesso ORDER BY CodProcesso;

-- Volume da agenda da onda 2 (NÃO medido em 27/09)
SELECT COUNT(*) FROM IV_AGENDA a WITH (NOLOCK) JOIN IV_PROCDADO d WITH (NOLOCK) ON d.Processo = a.Processo
WHERE d.CodProcesso IN (31,41,50) AND a.DtaAgenda >= '20231101';

-- Volume do histórico da onda 2 (NÃO medido em 27/09)
SELECT COUNT(*) FROM IV_HISTORICO WITH (NOLOCK)
WHERE CodProcesso IN (31,41,50) AND Processo IS NOT NULL AND DtaRealizacao >= '20231101';
```

---

## 12. Onda 2 — `Processo`, `Tarefa` e `Interacao` dos clientes casados (PR 4)

> **Versão 1.1 · 27/09/2026.** Desenho no plano `plano-pr4-pipeline-2026-09-27` (só leitura, sobre a main
> `00639cd`); decisões do Ricardo depois do merge do #249. **Não** tem tela: os textos, o filtro por processo na
> Agenda e na linha do tempo, o selo do estágio e o `VendaPerdida.ProcessoId` são do PR 5.

### 12.1 As decisões [D 27/09]

| # | Decisão |
|---|---|
| P1 | **Só o `Resumo` entra**, como `Processo.Titulo`. Em branco, o título é composto: `{tipo do processo} · nº {número}`. A `Descricao` do processo, o `Assunto`, o `AssuntoCmpl` e o `Detalhe` da agenda e o `Detalhe` e o `ResultadoCmpl` do histórico **não são lidos** — nenhuma consulta os pronuncia, e o teste do leitor prende isso como o do funil. O assunto da tarefa e da interação vem do catálogo: o nome da ação (tarefa) ou do resultado (interação). |
| P2 | Responsável da tarefa, quem a concluiu e autor da interação **sem conta no CRM → o dono do processo** (que, sem conta pelo login, é o dono da carteira, e sem os dois, o operador da rotina). A rotina conta cada caso no relatório. |
| P4 | Os catálogos mínimos apagados em 15/09 voltam: `TipoProcesso` (31/41/50), `Fase`, `TipoTarefa`, `Resultado` e `MotivoDePerda` `NAO_INFORMADO_NA_ORIGEM` — só o que os processos, as tarefas e as interações que entram usam. |
| P6 | Tarefa avulsa (sem processo) **não entra**: a agenda é lida pelo processo. |
| P7 | O **pai DNA entra como processo** — a regra DNA é do funil (§3), não do cadastro. |
| P8 | O processo **perdido** leva o motivo e o concorrente da **venda perdida principal** do mesmo número (a que a `PROCESSOS_VORTICE` já carrega, §4); sem formulário, `NAO_INFORMADO_NA_ORIGEM`. |
| P10 | Tarefa **pendente só entra se o processo está em andamento** (aberto ou suspenso); as concluídas entram todas, desde 01/11/2023. Quando o processo encerra, a pendente sai (exclusão lógica). |

### 12.2 O universo

Um processo entra quando é **31, 41 ou 50**, tem abertura crível (`COALESCE(DtaInclusao, 1º andamento)`, a mesma
faixa do funil: de 2000 até agora + 1 dia) **a partir de 01/11/2023**, a filial dele tem de-para no CRM e a pessoa
**casa pelo CPF/CNPJ com exatamente um cliente ativo** do CRM. O prospect continua só no funil. A rotina **não cria**
cliente, carteira nem usuário e **não toca** `ClienteCarteira.UltimaInteracaoEm`.

A tarefa entra quando é de processo que entrou (P6, P10). A interação entra quando é de processo que entrou.

### 12.3 O que é lido do Vórtice (só `SELECT`, `NOLOCK`, `ApplicationIntent=ReadOnly`)

| Tabela | Colunas | Filtro |
|---|---|---|
| `IV_PROCDADO`, `IV_PROCESSO`, `GE_Pessoa`, `IVS_Pes`/`IVS_Depto` | o do funil + `Fase`, `FaseOrdem`, `DtaFase`, `DtaStatus`, `Resumo`, `Valor`, `Qtde`, `DtaPrevConclusao`, `DtaPrevConcOrig` | `CodProcesso IN (31,41,50)` e abertura a partir de 01/11/2023 |
| `IV_AGENDA` (+ a linha do `IV_HISTORICO` que a concluiu) | `SeqAgenda`, `Processo`, `SeqUsuario`, `Acao`, `DtaAgenda`, `DtaLimiteExecucao`, `Prioridade`, `Realizada`, `HistoricoOrigem`, `TipoAgendamento`, `DtaGeracao`, `UltResultado`, `UltHistorico`, `DtaRealizacao` | pelo `IV_PROCDADO` 31/41/50, `DtaAgenda` a partir de 01/11/2023 |
| `IV_HISTORICO` | `SeqHistorico`, `Processo`, `SeqUsuario`, `AcaoGeradora`, `Resultado`, `AgendaOrigem`, `Natureza`, `DtaRealizacao`, `Duracao`, `Latitude`, `Longitude`; `USUINCLUSAO` **só dentro de um `CASE`** que diz se o registro é de sistema — o login nunca sai | `CodProcesso IN (31,41,50)`, com processo, a partir de 01/11/2023 |
| `IV_CodProcesso`, `IV_Acao`, `IV_Resultado`, `IV_ProcResultado` | código, nome, em uso, prazo; a classe do resultado pelo `IV_ProcResultado` dos tipos 31/41/50 | por lista de inteiros — só os códigos usados |
| `GE_Usuario` | `SeqUsuario`, `CodUsuario` | por lista — o login serve só para achar a conta, e nunca é gravado |

Não se lê: `IV_AGENDA.Contato`/`Vendedor`, `IV_HISTORICO.Contato`/`Vendedor`/`CodUsuario`, e nenhum texto livre além
do `Resumo` (P1).

### 12.4 Como cada coisa é gravada

| Entidade | Chave | Regra |
|---|---|---|
| `TipoProcesso` | `ChaveExterna(TipoProcesso, CodProcesso)` | código `{nome}_{cod}`, como a carga antiga; o que já existe com o código é adotado |
| `Fase` | único (tipo, código) | o texto de `IV_PROCESSO.Fase` dos processos que entram; vazia → `NAO_INFORMADA`; ordem = menor `FaseOrdem`; final pela palavra (`FINALIZ`, `CANCELA`, `CONCLU`) |
| `TipoTarefa` | `ChaveExterna(TipoTarefa, Acao)` | `{nome}_{ação}`, categoria `Interna` (a origem não diz se é visita), prazo da ação; + `NAO_INFORMADA` |
| `Resultado` | `ChaveExterna(Resultado, Resultado)` | `RES_{cod}`, sob a ação dona (sem ação dona, sob `NAO_INFORMADA`); classe pelo `IV_ProcResultado` 31/41/50 |
| `Processo` | `ChaveExterna(Processo, número)` | `Numero` = **número do Vórtice**; `CriadoEm` = abertura; situação pelo status (§ saneamento da carga antiga); encerrado sem data crível → `DtaStatus`, senão a abertura; valor e quantidade só positivos; dono: login → dono da carteira → operador |
| `Tarefa` | `ChaveExterna(Tarefa, SeqAgenda)` | concluída só com data, desfecho e quem concluiu; reprogramada, concluída, reaberta: acompanha a origem |
| `Interacao` | `ChaveExterna(Interacao, SeqHistorico)` | **só inclui** — o fato não muda (a tabela é somente-acrescentar). `EmpresaId` = a do processo |
| duplo ponteiro | — | `Tarefa.InteracaoConclusaoId` = a linha que a concluiu (`UltHistorico`; senão a última com `AgendaOrigem` = a tarefa); `InteracaoOrigemId` = `HistoricoOrigem`; `Interacao.TarefaId` = `AgendaOrigem` |
| `EstagioDoProcesso.ProcessoId` | — | ligado ao processo do mesmo número; desligado quando ele sai. A comparação do funil não enxerga a coluna |

### 12.5 A rotina

| Item | Como é |
|---|---|
| modo | `--somente-oportunidades-vortice [--simular] [--aceitar-queda]` — **o 2º modo da `PROCESSOS_VORTICE`**, depois do funil (que carrega a venda perdida que o P8 lê). Sem rotina nova e **sem migração**: os modos e a descrição não são semeados |
| trava e execução | fluxo próprio `VORTICE.OPORTUNIDADES` (`TravaDeFluxo` e `integracao.ExecucaoDeSincronizacao`); o funil não é tocado |
| ordem | lê o CRM → lê o Vórtice (processos, agenda **ou** histórico vazio → aborta) → planeja **só com leitura** → queda de mais de 5% em processos ou tarefas → aborta (passa só com `--aceitar-queda`, no terminal) → `--simular` para aqui → grava |
| gravação | catálogos numa transação; processos, tarefas e interações em blocos de 2.000, **um por transação, com a `ChaveExterna` no mesmo bloco**; depois o duplo ponteiro, o estágio ligado e a trilha. Bloco que cai: código 3, `Gravação parcial…`, e a próxima rodada completa |
| saída do universo | processo: exclusão lógica, e as tarefas dele também; volta → restaurado. Interação fica |
| trilha | `RegistroDeOrigem` nos fluxos `VORTICE.PROCESSO` (todo processo da janela, com o motivo de quem não entra), `VORTICE.TAREFA` e `VORTICE.INTERACAO` (só dos processos que entram) |

**Motivos de pendência:** processo — `FILIAL_DO_PROCESSO_FORA_DO_CRM`, `ABERTURA_COM_DATA_INVALIDA`,
`CLIENTE_SEM_DOCUMENTO_NO_VORTICE`, `DOCUMENTO_ZERADO_NO_VORTICE`, `DOCUMENTO_INVALIDO_NO_VORTICE`,
`CLIENTE_AUSENTE_DO_CRM`, `CLIENTE_EXCLUIDO_NO_CRM`, `CLIENTE_AMBIGUO_NO_CRM` (os da #236); tarefa —
`TAREFA_PENDENTE_DE_PROCESSO_ENCERRADO` (P10), `DATA_DA_AGENDA_INVALIDA`; interação — `DATA_DA_INTERACAO_INVALIDA`.

### 12.6 O que falta medir, e os riscos

1. **Volume** da agenda e do histórico da onda 2 (as duas consultas no fim da §11): a estimativa é de 100 a 250 mil
   linhas na primeira rodada. A simulação no servidor mostra o número antes de gravar.
2. **Numeração** (P11): `Processo.Numero` é o número do Vórtice, e o índice único (filial, número) não colide entre
   processos do Vórtice. Quando o CRM passar a abrir processo, precisa de uma faixa acima de `MAX(IV_PROCDADO.Processo)`.
3. **Filial do processo ≠ filial do cliente** (P9): a lista lê o cliente sob o filtro de filial, e a chave dele pode
   vir vazia para quem não alcança a filial do cliente. A contar na simulação.
4. As pendentes antigas de processo aberto aparecem como **vencidas** na Agenda (P10) — o que a origem diz.
5. O assunto da tarefa e da interação é o nome do catálogo, não o texto do vendedor (P1).

### 12.7 Como ligar

1. A `PROCESSOS_VORTICE` já roda o funil; o modo novo entra nela como segundo modo — **a rotina continua desligada**
   até quem administra ligar.
2. **Simular no servidor** — `Tracbel.Crm.Carga.exe --somente-oportunidades-vortice --simular` — e conferir: processos
   que entram (~24,8 mil), tarefas, interações e as contagens do P2.
3. Ligar (ou "Rodar agora"): o funil roda primeiro, e as oportunidades depois, na mesma rotina.
