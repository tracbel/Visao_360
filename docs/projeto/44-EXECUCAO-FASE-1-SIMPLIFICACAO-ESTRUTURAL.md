# 44 — Execução da Fase 1: simplificação estrutural

**Data:** 15/09/2026
**Autorização:** dada nesta sessão, com escopo fechado ("A Fase 1 está AUTORIZADA").
**Plano de origem:** [41 — Plano executivo da reestruturação](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md), fase 1.
**Estado anterior registrado em:** [41A — Snapshot antes da reestruturação](41A-SNAPSHOT-ANTES-DA-REESTRUTURACAO.md).
**Migration:** `20260916010353_SimplificacaoEstruturalFase1` (+ `SimplificacaoEstruturalFase1.Residuos.cs`).

O que esta fase fez, em uma frase: **removeu do banco as 31 tabelas que nasceram com o modelo
inicial e nunca receberam uma linha, mais as 9 colunas de tabelas vivas que apontavam para elas, e
o histórico de migração órfão em `dbo`** — sem mudar nenhuma regra de negócio, nenhuma tela e
nenhum contrato de API.

**Ponto de restauração:** `dados-locais/bancos/antes-fase-1-reestruturacao-20260915.bak`
(14,4 MB, `COPY_ONLY`). Os backups da sanitização e o banco de arquivo `TracbelCrmArquivo20260915`
não foram tocados.

---

## 1. Antes

Medido no banco alvo (`TracbelCrm`, contêiner `tracbel-crm-db`) imediatamente antes de aplicar a
migration, e conferido contra o documento 41A:

| Medida | Antes |
|---|---:|
| Tabelas | 82 |
| Colunas | 1.040 |
| Índices | 390 |
| Restrições de verificação (CHECK) | 189 |
| Chaves estrangeiras | 213 |
| Restrições de valor padrão | 48 |
| Restrições de unicidade | 1 |
| Schemas com tabela | 11 |
| Tabelas particionadas por data | 4 |
| Funções / esquemas de partição | 4 / 4 |
| Migrações aplicadas | 12 |
| Tabelas em `dbo` | 1 |
| Linhas (todas as tabelas) | 6.147 |

### 1.1 Conferência de vazio, refeita no banco alvo antes de qualquer DROP

A exigência era `COUNT(*) = 0` em cada tabela, contado de novo na hora — não a confiança no
levantamento do documento 39. A consulta foi **gerada a partir da própria lista de `DropTable` da
migration**, para não haver chance de conferir uma lista e remover outra.

| # | Tabela | Linhas | # | Tabela | Linhas |
|---:|---|---:|---:|---|---:|
| 1 | `comercial.Alerta` | 0 | 17 | `seguranca.Permissao` | 0 |
| 2 | `auditoria.CampoAuditado` | 0 | 18 | `organizacao.Praca` | 0 |
| 3 | `metadado.CampoPersonalizado` | 0 | 19 | `integracao.Recepcao` | 0 |
| 4 | `seguranca.CompartilhamentoDeRegistro` | 0 | 20 | `processo.RegraExecucao` | 0 |
| 5 | `comercial.ConsentimentoComunicacao` | 0 | 21 | `relatorio.Relatorio` | 0 |
| 6 | `seguranca.EquipeMembro` | 0 | 22 | `metadado.Resposta` | 0 |
| 7 | `auditoria.EventoDeAcesso` | 0 | 23 | `metadado.TratadorDeEvento` | 0 |
| 8 | `relatorio.FonteCampo` | 0 | 24 | `documento.Vinculo` | 0 |
| 9 | `organizacao.HierarquiaComercial` | 0 | 25 | `seguranca.Equipe` | 0 |
| 10 | `processo.InteracaoParticipante` | 0 | 26 | `processo.Regra` | 0 |
| 11 | `processo.ItemDeProposta` | 0 | 27 | `relatorio.Fonte` | 0 |
| 12 | `comercial.Lead` | 0 | 28 | `metadado.Pergunta` | 0 |
| 13 | `frota.LeituraDeHorimetro` | 0 | 29 | `metadado.Preenchimento` | 0 |
| 14 | `integracao.MensagemDeSaida` | 0 | 30 | `documento.Documento` | 0 |
| 15 | `organizacao.Meta` | 0 | 31 | `metadado.Formulario` | 0 |
| 16 | `processo.PassagemDeFase` | 0 | 32 | `dbo.__EFMigrationsHistory` | 0 |

**Tabelas com registro: 0.** Nenhuma tabela com dado foi removida — a regra "se qualquer tabela
possuir registro: não remover essa tabela" não precisou ser acionada.

---

## 2. Removido

### 2.1 As 31 tabelas (lista nominal)

| Schema | Tabelas |
|---|---|
| `organizacao` (3) | `HierarquiaComercial`, `Meta`, `Praca` |
| `seguranca` (4) | `Equipe`, `EquipeMembro`, `Permissao`, `CompartilhamentoDeRegistro` |
| `comercial` (3) | `Lead`, `Alerta`, `ConsentimentoComunicacao` |
| `processo` (5) | `Regra`, `RegraExecucao`, `PassagemDeFase`, `ItemDeProposta`, `InteracaoParticipante` |
| `auditoria` (2) | `CampoAuditado`, `EventoDeAcesso` |
| `integracao` (2) | `Recepcao`, `MensagemDeSaida` |
| `frota` (1) | `LeituraDeHorimetro` |
| `documento` (2) | `Documento`, `Vinculo` — **o schema inteiro** |
| `metadado` (6) | `CampoPersonalizado`, `Formulario`, `Pergunta`, `Preenchimento`, `Resposta`, `TratadorDeEvento` |
| `relatorio` (3) | `Relatorio`, `Fonte`, `FonteCampo` — **o schema inteiro** |

### 2.2 As 9 colunas de tabelas vivas

Cada uma apontava para uma das tabelas acima. A coluna é removida, a tabela fica.

| Tabela viva | Coluna | Apontava para |
|---|---|---|
| `comercial.Cliente` | `ProprietarioEquipeId` | `seguranca.Equipe` |
| `processo.Processo` | `ProprietarioEquipeId` | `seguranca.Equipe` |
| `processo.Tarefa` | `ResponsavelEquipeId` | `seguranca.Equipe` |
| `processo.Tarefa` | `CriadaPorRegraId` | `processo.Regra` |
| `processo.TipoTarefa` | `FormularioId` | `metadado.Formulario` |
| `processo.Interacao` | `LeadId` | `comercial.Lead` |
| `organizacao.Carteira` | `PracaId` | `organizacao.Praca` |
| `organizacao.Carteira` | `EquipeId` | `seguranca.Equipe` |
| `integracao.MensagemDescartada` | `MensagemDeSaidaId` | `integracao.MensagemDeSaida` |

### 2.3 As 9 chaves estrangeiras e os 9 índices dessas colunas

| Chave estrangeira removida | Índice removido |
|---|---|
| `FK_Cliente_Equipe_ProprietarioEquipeId` | `IX_Cliente_ProprietarioEquipeId` |
| `FK_Processo_Equipe_ProprietarioEquipeId` | `IX_Processo_ProprietarioEquipeId` |
| `FK_Tarefa_Equipe_ResponsavelEquipeId` | `IX_Tarefa_ResponsavelEquipeId` |
| `FK_Tarefa_Regra_CriadaPorRegraId` | `IX_Tarefa_CriadaPorRegraId` |
| `FK_TipoTarefa_Formulario_FormularioId` | `IX_TipoTarefa_FormularioId` |
| `FK_Interacao_Lead_LeadId` | `IX_Interacao_LeadId` |
| `FK_Carteira_Praca_PracaId` | `IX_Carteira_PracaId` |
| `FK_Carteira_Equipe_EquipeId` | `IX_Carteira_EquipeId` |
| `FK_MensagemDescartada_MensagemDeSaida_MensagemDeSaidaId` | `IX_MensagemDescartada_MensagemDeSaidaId` |

Os demais índices e restrições removidos saíram junto com as tabelas a que pertenciam, pelo
`DROP TABLE`.

### 2.4 As 3 restrições de verificação reescritas

Não foram apagadas: foram **recriadas menores**, porque a lista fechada que elas guardam encolheu
junto com o modelo.

| Restrição | Antes | Depois |
|---|---|---|
| `processo.CK_Interacao_TemVinculo` | `ProcessoId OR ClienteId OR ContatoId OR LeadId` | `ProcessoId OR ClienteId OR ContatoId` |
| `integracao.CK_ChaveExterna_Entidade` | lista de 80 entidades (4.799 caracteres) | lista de 49 entidades |
| `auditoria.CK_AlteracaoDeCampo_Entidade` | lista de 80 entidades | lista de 49 entidades |

### 2.5 Os três resíduos que o EF Core não remove sozinho

Escritos à mão em `SimplificacaoEstruturalFase1.Residuos.cs`:

1. **Partições órfãs.** `DROP TABLE` não leva junto a função nem o esquema de partição. Saíram
   `PF_Mensal_EventoDeAcesso`/`PS_Mensal_EventoDeAcesso`, `PF_Mensal_RegraExecucao`/
   `PS_Mensal_RegraExecucao` e `PF_Mensal_Recepcao`/`PS_Mensal_Recepcao`. Ficou só a partição de
   `auditoria.AlteracaoDeCampo`, que é a tabela de log que o sistema de fato escreve.
2. **Schemas vazios.** `documento` e `relatorio` perderam todas as suas tabelas e foram removidos —
   com conferência em `sys.objects` antes, para o `DROP SCHEMA` não falhar por um objeto esquecido.
3. **`dbo.__EFMigrationsHistory`.** O histórico órfão, de uma execução anterior à configuração do
   histórico em `metadado`. A remoção é **condicional**: se a tabela tiver ganhado alguma linha
   desde a conferência, ela fica e a migração segue sem erro.

---

## 3. Depois — contagem real, medida no banco

Medida no banco alvo com a mesma consulta usada no "antes", depois da migration aplicada:

| Medida | Antes | Depois | Diferença | Previsto no plano 41 |
|---|---:|---:|---:|---|
| Tabelas | 82 | **50** | −32 | 50 ✔ |
| Colunas | 1.040 | **665** | −375 | −364 ✘ (ver §6.1) |
| Índices | 390 | **232** | −158 | −148 ✘ (ver §6.1) |
| CHECK | 189 | **111** | −78 | −78 ✔ |
| Chaves estrangeiras | 213 | **122** | −91 | −91 ✔ |
| Valor padrão | 48 | 36 | −12 | — |
| Unicidade | 1 | 1 | 0 | — |
| Schemas com tabela | 11 | **8** | −3 | — |
| Tabelas particionadas | 4 | **1** | −3 | 1 ✔ |
| Funções / esquemas de partição | 4 / 4 | **1 / 1** | −3 / −3 | — |
| Tabelas em `dbo` | 1 | **0** | −1 | 0 ✔ |
| Migrações aplicadas | 12 | 13 | +1 | — |
| Linhas (todas as tabelas) | 6.147 | **6.148** | +1 | — |

A única linha a mais é o registro da própria migration em `metadado.__EFMigrationsHistory`.
**Nenhum dado operacional foi perdido.**

### 3.1 Integridade

| Verificação | Resultado |
|---|---|
| Chaves estrangeiras não confiáveis (`is_not_trusted`) | **0** |
| Restrições de verificação não confiáveis | **0** |
| Migrações pendentes | **0** |
| Registros órfãos | não aplicável — nenhuma tabela removida tinha linha |

### 3.2 As 49 tabelas de modelo, por schema

| Schema | Tabelas |
|---|---:|
| `organizacao` | 9 |
| `integracao` | 9 |
| `processo` | 9 |
| `comercial` | 8 |
| `frota` | 7 |
| `seguranca` | 4 |
| `metadado` | 2 |
| `auditoria` | 1 |
| **Total de modelo** | **49** |
| + `metadado.__EFMigrationsHistory` (técnica) | 50 |

---

## 4. Código removido

Nada de componente compartilhado foi tocado. Saíram entidade, configuração de EF, `DbSet`, porta e
teste **exclusivos** das estruturas removidas.

### 4.1 Arquivos apagados (20)

| Projeto | Arquivos |
|---|---|
| `Dominio/Organizacao` | `Praca.cs`, `HierarquiaComercial.cs`, `Meta.cs` |
| `Dominio/Seguranca` | `Equipe.cs`, `CompartilhamentoDeRegistro.cs` |
| `Dominio/Comercial` | `Alerta.cs`, `ConsentimentoComunicacao.cs` |
| `Dominio/Crm` | `Lead.cs`, `EventosLead.cs` — **a pasta inteira** |
| `Dominio/Workflow` | `Regra.cs`, `MotorWorkflow.cs`, `ContextoRegra.cs` — **a pasta inteira** |
| `Dominio/Metadado` | `Extensibilidade.cs`, `Formulario.cs` |
| `Dominio/Relatorio` | `Relatorio.cs` — **a pasta inteira** |
| `Dominio/Documento` | `Documento.cs` — **a pasta inteira** |
| `Infraestrutura/…/Configuracoes` | `LeadConfiguracao.cs`, `RelatorioConfiguracao.cs` |
| `tests/…Dominio.Testes` | `Crm/LeadTestes.cs`, `Workflow/MotorWorkflowTestes.cs` |

### 4.2 Classes órfãs removidas de arquivos que ficaram

Cada uma tinha a tabela removida e o `DbSet` removido, mas a classe sobrevivia no arquivo de um
vizinho. Em cada ponto ficou um comentário dizendo **o que saiu, por que, e em que condição volta** —
não um vazio silencioso.

| Arquivo | Classes removidas |
|---|---|
| `Dominio/Processo/Processo.cs` | `PassagemDeFase`, `ItemDeProposta` |
| `Dominio/Processo/Interacao.cs` | `InteracaoParticipante`, `enum PapelNaInteracao` |
| `Dominio/Seguranca/Permissao.cs` | `Permissao` (ficam `ConjuntoPermissao`, `ItemConjuntoPermissao`, `UsuarioConjuntoPermissao`) |
| `Dominio/Auditoria/Auditoria.cs` | `CampoAuditado`, `EventoDeAcesso`, `enum TipoDeEventoDeAcesso` (fica `AlteracaoDeCampo`) |
| `Dominio/Integracao/Integracao.cs` | `Recepcao`, `MensagemDeSaida`, `enum SituacaoDaRecepcao`, `enum SituacaoDaMensagem` |
| `Dominio/Frota/Equipamento.cs` | `LeituraDeHorimetro` |

### 4.3 Alterações pontuais

| Arquivo | O que mudou |
|---|---|
| `Infraestrutura/…/CrmDbContext.cs` | 31 `DbSet` removidos (79 → 48); o filtro próprio do `Lead` e os cinco campos pré-computados dele saíram; `FiltroProprio` ficou vazia **com o mecanismo intacto**; `CompartilhamentoDeRegistro` saiu de `FronteiraDeEmpresaJustificada` |
| `Dominio/Portas/Portas.cs` | sobrou `ICalendarioUtil`; as portas de workflow saíram |
| 9 arquivos de `…/Configuracoes` | 27 classes `IEntityTypeConfiguration<T>` removidas; 18 declarações das 9 colunas removidas |
| `Dominio/Processo/Interacao.cs` | `Registrar` perdeu o parâmetro `leadId`; a guarda passou a exigir cliente, processo **ou** contato |
| `Infraestrutura/…/RepositorioDeIndicadoresExecutivos.cs` | a consulta a `Metas` saiu; o cartão do ano passou a declarar a lacuna `metaDeFaturamento` em vez de somar de uma tabela vazia |
| `Carga/Program.cs` | os três `DELETE FROM` das tabelas removidas saíram do `--recomecar` |
| `Carga/CargaDeProcessoDoVortice.cs` | uma chamada com argumento nomeado `leadId` corrigida |

### 4.4 Cobertura de teste que saiu junto

| Teste | Por quê |
|---|---|
| `Dominio.Testes/Crm/LeadTestes.cs` | exclusivo do `Lead` |
| `Dominio.Testes/Workflow/MotorWorkflowTestes.cs` | exclusivo do motor de regras |
| `Api.Testes/IndicadoresExecutivosTestes` — o teste da meta somada | a meta não tem mais fonte; o teste vizinho, que já exigia "sem meta é sem meta, e não meta zero", ficou e passou a valer para o ano corrente também |
| `Aplicacao.Testes/…/FiltroSegurancaTestes` | **reescrito, não apagado** — ver §6.2 |

---

## 5. Testes executados

### 5.1 A migration, ensaiada antes de tocar o banco alvo

O ensaio não foi feito num banco novo: foi feito numa **restauração do ponto de restauração** no
contêiner `tracbel-crm-ensaio` (`TracbelCrmEnsaioFase1`), ou seja, contra a estrutura exata de
antes.

| Passo | Resultado |
|---|---|
| `database update` (up) | ✅ 82 → 50 tabelas, 213 → 122 FKs, 4 → 1 partição, `dbo` zerado |
| `database update <anterior>` (down) | ✅ **as 17 medidas voltaram idênticas ao antes** — 82 / 1.040 / 390 / 189 / 213 / 48 / 1 / 11 / 4 / 4 / 4 / 0 / 0 / 12 / 1 / 6.147 |
| `database update` (up de novo) | ✅ mesmos números do primeiro up |
| Aplicação no banco alvo | ✅ (§3) |

### 5.2 Build e testes

| Verificação | Resultado |
|---|---|
| `dotnet build` da solução | ✅ **0 erros, 0 avisos** |
| `Tracbel.Crm.Dominio.Testes` | ✅ 163 |
| `Tracbel.Crm.Integracao.Testes` | ✅ 133 |
| `Tracbel.Crm.Aplicacao.Testes` | ✅ 55 |
| `Tracbel.Crm.Api.Testes` | ✅ 90 |
| `Tracbel.Crm.Arquitetura.Testes` | ✅ 62 (inclui os que sobem contêiner e rodam a cadeia inteira de migrações do zero) |
| **Total** | **✅ 503 casos, 0 falhas** |
| `npm run build` (frontend) | ✅ |
| `npm run lint` (frontend) | ✅ (só os avisos que já existiam antes, em telas não tocadas) |

### 5.3 A aplicação de pé, contra o banco migrado

`node scripts/prototipo/capturar-crm-vazio.mjs`, com API e frontend rodando sobre o banco já
migrado:

**12 telas · 0 quebradas · 0 erros de console · 0 respostas de erro da API.**

Visão 360 (diretoria e CEN), Clientes, Equipamentos, Agenda, Pipeline, Cobertura, Funil,
Performance de CEN, Cobertura por Filial, Indicadores Geográficos e Configurações. Nenhuma
funcionalidade visível desapareceu — que era o sinal combinado de classificação errada.

### 5.4 O que continuou intocado

| Item | Estado |
|---|---|
| Integração do ART | continua **DESABILITADA**; `publicar.ps1` não foi alterado; nenhuma importação real foi executada |
| Faturamento / Protheus | compila e funciona; `--somente-faturamento` não passa pelo Vórtice e segue livre |
| Cliente, Classe, Cadência, municípios, permissões, Agenda, Tarefa, Interação | sem mudança de regra |
| Backups da sanitização e `TracbelCrmArquivo20260915` | não sobrescritos |

---

## 6. Divergências e decisões tomadas durante a execução

### 6.1 Colunas e índices: o plano subestimou

O plano 41 previa −364 colunas e −148 índices; o real foi **−375 e −158**. A diferença é para
mais, não para menos, e tem uma causa só: a estimativa do plano foi feita sobre a lista de tabelas
do documento 39 e **não somou as 9 colunas de tabelas vivas nem os índices que o `DROP TABLE`
levou junto** em algumas das tabelas menores. Nenhuma tabela a mais foi removida — o número de
tabelas, CHECK e chaves estrangeiras bateu exatamente com o previsto. O que vale daqui para frente
é a contagem medida da §3.

### 6.2 `FiltroSegurancaTestes` foi reescrito, e não apagado

O arquivo provava a **profundidade** (próprios / equipe / empresa / organização) usando o `Lead`,
a única entidade do modelo com filtro próprio. Com a tabela removida, o mecanismo de filtro por
profundidade ficou **sem nenhuma entidade** — ele continua em `CrmDbContext`, dormente e
documentado, mas não há mais o que testar por ali.

A remoção do arquivo foi **recusada pela política de segurança do ambiente** (remoção de teste de
segurança). A recusa foi acatada: em vez de insistir, o arquivo foi **reescrito** para provar o que
continua valendo para as 50 tabelas que ficaram — que a fronteira de empresa chega ao SQL de toda
entidade com `EmpresaId`, que os tipos de valor atravessam o banco sem se desfazer e que o serviço
de sistema enxerga o que o usuário da filial não enxerga. A profundidade em si continua coberta por
`Dominio.Testes/Seguranca/AutorizadorTestes`, e a via de escape entre empresas por
`FronteiraDeEmpresaTestes`.

### 6.3 Um defeito real na migration, achado pelos testes de contêiner

A primeira versão do `DROP TABLE [dbo].[__EFMigrationsHistory]` referenciava a tabela **dentro do
mesmo lote** do `IF` que a protegia. O SQL Server resolve os nomes do lote inteiro antes de
executar, inclusive os do ramo não tomado: num banco onde a órfã não existe — todo banco nascido
depois da configuração do histórico em `metadado`, e é o caso dos bancos dos testes — a migração
morria com `Invalid object name`. Sete testes de contêiner pegaram isso. A correção põe o `SELECT` e
o `DROP` dentro de um `EXEC`, deixando no lote de fora só o `OBJECT_ID`, que recebe o nome como
texto. **Este defeito só apareceu porque a suíte roda a cadeia inteira de migrações num banco do
zero — e é por isso que ela existe.**

### 6.4 Doze testes de arquitetura tiveram números atualizados

Não foram enfraquecidos: continuam sendo listas fechadas, agora com os valores reais. É o "portão"
funcionando no sentido da remoção, e não só no da criação.

| Teste | De | Para |
|---|---|---|
| `EsquemaENomenclatura…somam_oitenta_tabelas` | 80 em 10 schemas | 49 em 8 schemas (renomeado) |
| `MigracaoNoContainer…setenta_e_nove_tabelas` | 80 em 10 schemas | 49 em 8 schemas (renomeado) |
| `…As_quatro_tabelas_…_particionadas` | 4 tabelas | 1 tabela (renomeado) |
| `Multiempresa…excecoes_da_fronteira` | 2 exceções | 1 exceção (`Usuario`) |
| `DominioDeEntidade…nomeia_entidade` | 10 colunas | 2 colunas |
| `DominioDeEntidade…nomeia_campo` | 4 colunas | 1 coluna |
| `TiposDeColuna…TextoIlimitadoJustificado` | 8 colunas de JSON | 1 coluna |
| `TiposDeColuna…booleana_anulavel` | 1 exceção justificada | nenhuma |
| `IntegridadeReferencial…CascadeJustificado` | 2 justificativas | 1 justificativa |
| `CatalogoDeSistema…ReferenciaGenericaJustificada` | 1 justificativa | nenhuma |
| `Arquitetura…Eventos_de_dominio_sao_imutaveis` | exigia "pelo menos um evento" | exige a FORMA, que vale para zero |
| `SolidTestes` / `ArquiteturaTestes` | ancoravam o assembly em `typeof(Lead)` | ancoram em `typeof(Cliente)` |

### 6.5 O Vórtice foi congelado — e o congelamento é real, não só comentário

Decisão D-12 cumprida em duas camadas:

- **Marcação.** `CargaDoVortice.cs` e `CargaDeProcessoDoVortice.cs` abrem com
  `LEGADO / SOMENTE REFERÊNCIA — CONGELADO NA FASE 1`, dizendo por que o código fica (é a
  documentação executável de como o dado foi interpretado) e quando sai (fase 8).
- **Freio operacional.** `Carga/Program.cs` passou a **recusar** qualquer modo que leia o Vórtice
  sem a declaração explícita `--legado-somente-referencia-eu-sei-o-que-estou-fazendo`. Conferido:
  rodar a carga sem argumento agora termina com código 2, sem ler nem gravar nada.
- **O que continua livre**, porque não lê o Vórtice: `--somente-faturamento` (Protheus),
  `--somente-territorio`, `--somente-art` e `--somente-medir`.

O congelamento **não** foi levado ao build: o faturamento do Protheus mora numa parte da mesma
classe e precisa continuar compilando e funcionando até a fase 8.

### 6.6 Nada foi aplicado ao servidor

A migration foi aplicada **só** ao banco de desenvolvimento local e ao banco de ensaio. O banco
central do servidor não foi tocado — isso exige autorização própria.

---

## 7. Critérios de aceite

| Critério | Resultado |
|---|---|
| Nenhum dado operacional perdido | ✅ 6.147 → 6.148 linhas (+1, o registro da migration) |
| Nenhuma tabela com registro removida | ✅ as 32 conferidas em 0 |
| Banco consistente | ✅ 0 constraint não confiável, 0 migração pendente |
| Build e testes passando | ✅ 0 erros, 0 avisos, 503 casos verdes |
| Aplicação funcionando | ✅ 12 telas, 0 quebras, 0 erro de API |
| ART continua desabilitado | ✅ |
| Faturamento / Protheus não quebrado | ✅ compila e o caminho segue livre |
| Nenhuma mudança de regra de negócio | ✅ |
| Migration reversível | ✅ o `down` devolveu as 17 medidas idênticas |

**Todos os critérios foram atendidos.** A fase 2 pode ser considerada — mediante autorização
própria, como as anteriores.

---

## 8. O que fica pendente desta fase

1. **Commit.** As mudanças estão na árvore de trabalho, **sem commit**, aguardando autorização.
   Mensagem sugerida: `refactor(db): simplifica estruturas não utilizadas`.
2. **Servidor.** A migration não foi aplicada ao banco central. Quando for, o roteiro é o mesmo:
   ponto de restauração, conferência de vazio refeita **naquele** banco, `up`, medição.
3. **Documentos que citam os números antigos** (14 seção 2.1, 17 seção 8.12, 21, 39) continuam
   descrevendo o modelo de 80 tabelas. Atualizá-los é trabalho da fase 2, junto com a auditoria.
4. **O que voltou para a fila de desenho**, com o motivo escrito no próprio código: participante de
   interação, passagem de fase, item de proposta, série de horímetro, caixa de saída transacional,
   evento de acesso para a LGPD, formulário dinâmico e relatório salvo.
