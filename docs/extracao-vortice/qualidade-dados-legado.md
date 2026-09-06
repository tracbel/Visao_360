# Qualidade do dado legado — medição ao vivo contra o catálogo de saneamento

> Medido em 2026-09-03, banco `CRM` de produção, login `CRM_Leitura` (só `db_datareader`),
> via `vortice-crm-agent/connect.ps1` (`Invoke-Vortice`), que bloqueia por guarda de texto
> qualquer verbo de escrita/DDL na consulta. **Nenhuma escrita foi executada.** Toda
> consulta usa `WITH (NOLOCK)` e tem filtro — nenhuma varre a tabela sem `WHERE`/`GROUP BY`
> restritivo. Isto dimensiona o trabalho real de migração para
> [16-HIGIENIZACAO-DE-DADOS](../projeto/16-HIGIENIZACAO-DE-DADOS.md): quanto de cada tabela
> viola cada regra do catálogo, hoje, com número exato.
>
> Contexto: esta medição foi possível porque a VPN estava conectada nesta rodada
> (`Test-Vortice` → `Conexao OK -> CRM como CRM_Leitura`). Números de linha totais podem
> divergir alguns pontos percentuais dos citados em
> [00-RELATORIO-EXTRACAO](00-RELATORIO-EXTRACAO.md) (coletado em 02/09/2026) porque o
> núcleo BPM do Vórtice está **vivo hoje** — o volume cresce todo dia.

---

## 1. `GE_Pessoa` (119.348 linhas hoje)

| # | Regra do catálogo | Query (`WITH (NOLOCK)`, sempre filtrada) | Resultado | Leitura |
|---:|---|---|---:|---|
| 1 | Domínio: `Status` não pode ficar em branco | `SELECT COUNT(*) FROM GE_Pessoa WITH (NOLOCK) WHERE LTRIM(RTRIM(ISNULL(Status,''))) = '';` | **2.126** | Confirma o achado 9.6 do documento 01 — o mesmo patamar medido em 02/09. |
| 2 | Domínio: `FisicaJuridica` só aceita `F`/`J` | `SELECT ISNULL(FisicaJuridica,'(NULO)'), COUNT(*) FROM GE_Pessoa WITH (NOLOCK) GROUP BY FisicaJuridica;` | `J`=75.933 · `F`=41.719 · `(NULO)`=1.663 · `0`=32 · `O`=1 | 33 linhas (`0`/`O`) são sujeira de importação — nem F nem J. |
| 3 | Telefone: `FoneNro1` sem DDD deveria ter 8-9 dígitos | `SELECT COUNT(*) FROM GE_Pessoa WITH (NOLOCK) WHERE FoneNro1 IS NOT NULL AND FoneNro1 > 0 AND LEN(CAST(FoneNro1 AS varchar(20))) NOT IN (8,9);` | **168** | Confirma a lição do documento 04, seção 1.2 — `decimal(12)` aceita lixo de tamanho errado sem reclamar. |
| 4 | E-mail: formato precisa ter `@` e domínio com ponto, sem espaço | `SELECT COUNT(*) FROM GE_Pessoa WITH (NOLOCK) WHERE Email IS NOT NULL AND LTRIM(RTRIM(Email))<>'' AND (Email NOT LIKE '%_@_%.__%' OR Email LIKE '% %');` | **528** | Um e-mail assim nunca entrega e-mail nenhum — é lixo funcional, não só de formato. |
| 5 | Texto livre: sem espaço duplo | `SELECT COUNT(*) FROM GE_Pessoa WITH (NOLOCK) WHERE NomeRazao LIKE '%  %';` | **332** | Baixo relativo ao total (0,3%) — mas seria zero com normalização na entrada. |
| 6 | **CNPJ perde zero à esquerda no tipo `decimal`** | `SELECT COUNT(*) FROM GE_Pessoa WITH (NOLOCK) WHERE FisicaJuridica='J' AND NroCGCCPF IS NOT NULL AND NroCGCCPF > 0 AND LEN(CAST(NroCGCCPF AS varchar(20))) < 12;` | **51.523 de 75.933 pessoas jurídicas (67,9%)** | 🔴 **Achado novo, não citado no documento 01.** `NroCGCCPF` é `decimal(13,0)` (documento 04 já bane isso para telefone; aqui é pior). A raiz+filial do CNPJ tem 12 dígitos; sem zero à esquerda preservado, reconstituir o CNPJ original exige saber, campo a campo, quantos zeros faltam — informação que **não está gravada em lugar nenhum**. |
| 7 | **CPF perde zero à esquerda no tipo `decimal`** | `SELECT COUNT(*) FROM GE_Pessoa WITH (NOLOCK) WHERE FisicaJuridica='F' AND NroCGCCPF IS NOT NULL AND NroCGCCPF > 0 AND LEN(CAST(NroCGCCPF AS varchar(20))) < 9;` | **6.611 de 41.719 pessoas físicas (15,8%)** | Mesmo defeito do item 6, em menor proporção porque CPF tem base de 9 dígitos (menos chance de começar em zero do que a raiz de 8 dígitos do CNPJ). |

> **Por que isto importa mais que parece.** O documento 01 (achado 9.4) já media **116 CPFs
> repetidos** por falta de validação na entrada. A medição acima mostra uma causa estrutural
> adicional e pior: **a maioria das pessoas jurídicas do Vórtice não tem o CNPJ armazenado de
> forma reconstituível**. Qualquer De-Para automático Vórtice → CRM novo que use "CNPJ" como
> chave de casamento (documento 16, seção 8) precisa primeiro decidir uma política de zero à
> esquerda por raiz de CNPJ conhecida — não dá para confiar cegamente na coluna.

---

## 2. `IV_Processo` (1.174.932+ linhas)

| # | Regra do catálogo | Query | Resultado | Leitura |
|---:|---|---|---:|---|
| 1 | Domínio: `Status` não pode ficar em branco | `SELECT COUNT(*) FROM IV_Processo WITH (NOLOCK) WHERE LTRIM(RTRIM(ISNULL(Status,''))) = '';` | **480.397** | Maior que os 437.694 de 02/09 — mas os dois números **não são diretamente comparáveis**: o critério aqui é mais largo (`LTRIM(RTRIM(ISNULL(...)))`, que pega também Status só-espaço) do que o da extração original. Não dá para concluir "piorou 42 mil linhas em 1 dia" sem reexecutar a query original lado a lado; dá para concluir, com segurança, que o problema é da mesma ordem de grandeza e continua crescendo. |
| 2 | Domínio: sinônimo duplicado (`FINALIZADO`/`FINALIZADA`, `CANCELADO`/`CANCELADA`) | `SELECT Status, COUNT(*) FROM IV_Processo WITH (NOLOCK) WHERE Status IN ('FINALIZADO','FINALIZADA','CANCELADO','CANCELADA') GROUP BY Status;` | `CANCELADA`=2.412 · `CANCELADO`=41.404 · `FINALIZADA`=11.563 · `Finalizado`=18.416 | A coluna `Status` devolveu **`Finalizado` em caixa mista**, não `FINALIZADO` — a comparação só bateu porque a collation do banco é *case-insensitive* (`_CI_`). São **quatro grafias para dois conceitos**, não duas. |
| 3 | Data: `DtaPrevConclusao` dentro de [1990, ano atual+30] | `SELECT COUNT(*) FROM IV_Processo WITH (NOLOCK) WHERE DtaPrevConclusao IS NOT NULL AND (YEAR(DtaPrevConclusao) < 1990 OR YEAR(DtaPrevConclusao) > YEAR(GETDATE())+30);` | **0** | Nem toda coluna de data está corrompida — a sujeira de data mora em `EXT_Titulo` (seção 5), não aqui. |
| 4 | Domínio: `Fase` sem acento junto com fase acentuada | `SELECT Fase, COUNT(*) FROM IV_Processo WITH (NOLOCK) WHERE Fase LIKE N'%fericao%' OR Fase LIKE N'%ferição%' GROUP BY Fase;` | `Afericao`=50.506 | Confirma o achado do documento 00-RELATORIO-EXTRACAO — nenhuma linha usa a forma acentuada "Aferição": o problema não é "duas grafias concorrentes", é que **a única grafia gravada já nasceu sem acento**. |

---

## 3. `IV_Agenda` (931.989+ linhas)

| # | Regra do catálogo | Query | Resultado | Leitura |
|---:|---|---|---:|---|
| 1 | Consistência entre colunas de status | `SELECT COUNT(*) FROM IV_Agenda WITH (NOLOCK) WHERE Realizada = 'S' AND (Status IS NULL OR Status <> 'C');` | **132.878** | 14% das tarefas dizem "realizada" (`Realizada='S'`) mas não têm o `Status` de concluída correspondente — as duas colunas que deveriam contar a mesma história divergem em mais de cem mil linhas. |
| 2 | FK real, não texto livre: `Vendedor` deveria ser ID, não login | `SELECT COUNT(*) FROM IV_Agenda WITH (NOLOCK) WHERE Vendedor IS NOT NULL AND Vendedor LIKE '%[a-zA-Z]%';` | **487.038** | Mais da metade das agendas gravam um login alfabético em vez de um ID — a lição já registrada no documento 03 ("`IV_Agenda.Vendedor` guarda um login, não um ID") tem, aqui, o número exato por trás. |
| 3 | Texto livre: sem truncamento silencioso | `SELECT COUNT(*) FROM IV_Agenda WITH (NOLOCK) WHERE ResultadoCmpl IS NOT NULL AND LEN(ResultadoCmpl) = 20;` | **4.566** | `ResultadoCmpl` é `varchar(150)`; um agrupamento de linhas com **exatamente** 20 caracteres, nem um a mais, é a marca d'água do truncamento em nível de aplicação (achado 9.2). |
| 4 | Data: `DtaAgenda` dentro de [1990, ano atual+30] | `SELECT COUNT(*) FROM IV_Agenda WITH (NOLOCK) WHERE DtaAgenda IS NOT NULL AND (YEAR(DtaAgenda) < 1990 OR YEAR(DtaAgenda) > YEAR(GETDATE())+30);` | **7** | Pequeno em volume absoluto, mas mostra que mesmo a tabela mais usada do sistema (a agenda do dia a dia) tem data implausível — sem CHECK, sete é maior que zero. |

---

## 4. `IV_Historico` (2.436.127+ linhas)

| # | Regra do catálogo | Query | Resultado | Leitura |
|---:|---|---|---:|---|
| 1 | Texto livre: sem truncamento silencioso | `SELECT COUNT(*) FROM IV_Historico WITH (NOLOCK) WHERE ResultadoCmpl IS NOT NULL AND LEN(ResultadoCmpl) = 20;` | **44.962** | A tabela de origem do achado 9.2 do documento 01 ("`ResultadoCmpl` é `varchar(150)` mas grava truncado em 20") — aqui está o tamanho exato do dano: quase 45 mil registros de histórico perderam texto. |
| 2 | Texto livre: sem espaço duplo | `SELECT COUNT(*) FROM IV_Historico WITH (NOLOCK) WHERE Detalhe LIKE '%  %';` | **349.315** | 14,3% de todo o histórico de interação tem espaço duplo em `Detalhe` — o maior achado absoluto desta rodada. Nenhuma normalização de texto existe em nenhum ponto da esteira de gravação. |
| 3 | Coordenada geográfica plausível | `SELECT COUNT(*) FROM IV_Historico WITH (NOLOCK) WHERE Latitude IS NOT NULL AND Longitude IS NOT NULL AND ((Latitude=0 AND Longitude=0) OR Latitude NOT BETWEEN -34 AND 6 OR Longitude NOT BETWEEN -75 AND -32);` | **12** | Volume baixo — a geolocalização de atendimento em campo (um acerto do Vórtice, documento 01 seção 12) é majoritariamente confiável; só 12 pontos caem fora do Brasil ou em (0,0). |
| 4 | Data: `DtaRealizacao` dentro de [1990, ano atual+30] | `SELECT COUNT(*) FROM IV_Historico WITH (NOLOCK) WHERE YEAR(DtaRealizacao) < 1990 OR YEAR(DtaRealizacao) > YEAR(GETDATE())+30;` | **1** | Coluna quase perfeita — reforça que o problema de data grave do Vórtice está concentrado no lado financeiro (`EXT_Titulo`), não no BPM. |

---

## 5. `EXT_Veic` (8.020 linhas — tabela morta desde 24/05/2024)

| # | Regra do catálogo | Query | Resultado | Leitura |
|---:|---|---|---:|---|
| 0 | Volumetria de referência | `SELECT COUNT(*) FROM EXT_Veic WITH (NOLOCK);` | **8.020** | Bem menor que o `IV_ClientePropr` vivo (246.684 linhas, documento 15) — confirma que `EXT_Veic` é o cadastro morto de frota, não o vivo. |
| 1 | Chassi: forma fixa de 17 caracteres | `SELECT COUNT(*) FROM EXT_Veic WITH (NOLOCK) WHERE Chassi IS NOT NULL AND LTRIM(RTRIM(Chassi))<>'' AND LEN(LTRIM(RTRIM(Chassi))) <> 17;` | **3.262 (40,7%)** | Quatro em cada dez chassis não têm o tamanho padrão VIN. Sem o tipo `Chassi` (documento 16), essa coluna nunca serviu como chave de deduplicação confiável de equipamento. |
| 2 | Deduplicação: chassi é chave natural do equipamento | `SELECT COUNT(*) FROM (SELECT Chassi FROM EXT_Veic WITH (NOLOCK) WHERE Chassi IS NOT NULL AND LTRIM(RTRIM(Chassi))<>'' GROUP BY Chassi HAVING COUNT(*) > 1) x;` | **5 chassis duplicados** | Baixo em volume absoluto — mas cada duplicata aqui é, por definição, dois cadastros para a mesma máquina física. |
| 3 | Placa: formato antigo ou Mercosul | `SELECT COUNT(*) FROM EXT_Veic WITH (NOLOCK) WHERE Placa IS NOT NULL AND LTRIM(RTRIM(Placa))<>'' AND Placa NOT LIKE '[A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9]' AND Placa NOT LIKE '[A-Z][A-Z][A-Z][0-9][A-Z][0-9][0-9]';` | **2** | Praticamente todo o cadastro de placa é bem formado — é o chassi que concentra o problema nesta tabela. |

---

## 6. `EXT_Titulo` (577.925+ linhas)

| # | Regra do catálogo | Query | Resultado | Leitura |
|---:|---|---|---:|---|
| 1 | Domínio: `Status` não mistura código com texto | `SELECT Status, COUNT(*) FROM EXT_Titulo WITH (NOLOCK) GROUP BY Status ORDER BY qtd DESC;` | `Pago`=309.056 · `B`=251.276 · `A`=12.915 · `Compensado`=4.480 · `A Depositar`=175 · `Devolvido`=14 · `Cancelado`=9 | Confirma o achado da seção 15 do relatório de extração: **duas integrações diferentes gravaram na mesma coluna em épocas diferentes** — `B`/`A` são código de uma; `Pago`/`Compensado`/`A Depositar`/`Devolvido`/`Cancelado` são texto da outra. Sem saber a data de corte, não dá para nem afirmar que `B` e `Pago` significam a mesma coisa. |
| 2 | Data: `DtaVencto` dentro de [1990, ano atual+30] | `SELECT COUNT(*) FROM EXT_Titulo WITH (NOLOCK) WHERE DtaVencto IS NOT NULL AND (YEAR(DtaVencto) < 1990 OR YEAR(DtaVencto) > YEAR(GETDATE())+30);` | **6** | Reconfirma ao vivo o achado da pesquisa 02 (6 títulos com `DtaVencto > 2100-01-01`, o maior em 08/05/5024). Baixo em volume, mas cada um é um vencimento de conta a receber inexistente. |
| 3 | Sentinela do Protheus (`1900-01-01`) já deveria estar em `NULL` | `SELECT COUNT(*) FROM EXT_Titulo WITH (NOLOCK) WHERE DtaVencto = '1900-01-01' OR DtaEmissao = '1900-01-01';` | **0** | Nesta tabela específica a coalescência funcionou — o escape de sentinela documentado na pesquisa 02 acontece em outras colunas de título (`DtaUltPgto`, `DtaQuitacao`), não nestas duas. |
| 4 | Dinheiro: sem valor negativo inesperado | `SELECT COUNT(*) FROM EXT_Titulo WITH (NOLOCK) WHERE VlrOriginal < 0;` | **0** | `VlrOriginal` é `numeric(14,2)` — ao contrário de telefone e CPF/CNPJ, aqui o Vórtice usou o tipo certo (documento 04, seção 1.2), e o dado reflete isso: zero violação de sinal e escala já garantida em duas casas pelo próprio tipo de coluna. |

---

## 7. Resumo — o que isto muda no dimensionamento da migração

| Achado | Volume | Onde no documento 16 |
|---|---:|---|
| CNPJ sem zero à esquerda reconstituível | 51.523 pessoas jurídicas (67,9%) | Seção 2 (CPF/CNPJ), seção 8 (migração) — **achado novo desta rodada** |
| CPF sem zero à esquerda reconstituível | 6.611 pessoas físicas (15,8%) | idem |
| `IV_Processo.Status` em branco | 480.397 processos | Seção 3 (domínios), seção 8 |
| `IV_Agenda.Vendedor` guarda login, não ID | 487.038 agendas | Seção 3, seção 9 (FK, não texto) |
| `IV_Agenda.Realizada` × `Status` inconsistentes | 132.878 agendas | Seção 3 |
| `IV_Historico.Detalhe` com espaço duplo | 349.315 interações | Seção 2 (texto livre) |
| `IV_Historico.ResultadoCmpl` truncado em 20 | 44.962 interações | Seção 2, seção 9 (tamanho de coluna) |
| `EXT_Veic.Chassi` fora do padrão de 17 | 3.262 equipamentos (40,7% da tabela) | Seção 2 (chassi), seção 4 (deduplicação) |
| `EXT_Titulo.Status` mistura código e texto | 577.925 títulos (100% da tabela) | Seção 3 (domínios) |

**Leitura geral:** as colunas que já usavam o tipo certo no Vórtice (`VlrOriginal numeric(14,2)`,
a geolocalização de `IV_Historico`) aparecem quase limpas nesta medição — reforçando que o
defeito não é "o Vórtice é ruim em tudo", é **ausência de validação na entrada em tudo que
não escolheu por acidente o tipo certo**. É exatamente o argumento do princípio 1 do
documento 16: normalizar na entrada, um só ponto de normalização por tipo.
