# 41A — Snapshot da estrutura antes da reestruturação

> **Versão 1.0 · 15/09/2026.** É o **ponto técnico anterior à reconstrução**: o estado exato do banco e
> do código no momento em que o plano executivo foi aprovado para começar.
>
> Medido ao vivo no banco local `TracbelCrm` (contêiner `tracbel-crm-db`) com consultas somente leitura
> ao catálogo de sistema, e no repositório. **Nada foi alterado.**
>
> Detalhe por tabela: [39A](39A-INVENTARIO-DETALHADO-DO-BANCO.md) · Diagrama:
> [39B](39B-DIAGRAMA-ER-ATUAL.md) · Análise: [39](39-AUDITORIA-ARQUITETURA-BANCO.md) · Alvo:
> [40](40-ARQUITETURA-ALVO-DO-BANCO.md).

## 1. Identificação da versão

| Item | Valor |
|---|---|
| Commit (HEAD) | `81b12211a27d3c3f785a48cb37a211aa6469ad44` (`81b1221`) |
| Branch | `main` |
| Data do commit | 13/09/2026 — *Documento 32: a publicação de 13/09/2026, conferida no servidor* |
| Trabalho não commitado | **104 arquivos** modificados ou não rastreados (integração do ART de 14/09, sanitização de 15/09 e os documentos 35 a 42) |
| Identificação da estrutura | SHA-256 do conjunto das 12 migrações + designers + snapshot do modelo (25 arquivos): **`F066DFE78A7E5723…`** |
| Snapshot do modelo EF | `CrmDbContextModelSnapshot.cs` — SHA-256 **`0164407E8E8687FF…`** |

> Os dois resumos existem para responder, no futuro, "esta estrutura é a mesma de antes da
> reestruturação?" sem depender de memória: basta recalcular o SHA-256 dos mesmos arquivos.

## 2. O banco

| Item | Valor |
|---|---|
| Banco medido | `TracbelCrm` (local, contêiner `tracbel-crm-db`, SQL Server 2022) |
| Colação | `Latin1_General_CI_AI` · nível de compatibilidade 160 |
| Banco central | `AGRO-SISTEMAS-W\TracbelCrm` (SQL Server 2025 Express), **mesma estrutura** — as mesmas 12 migrações (documento 38) |
| Migrações aplicadas | **12** |

### 2.1 Objetos

| Objeto | Quantidade |
|---|---:|
| **Tabelas** | **82** (80 de domínio + 2 históricos de migração) |
| Colunas | **1.040** |
| Índices (sem a heap) | **390** — 82 chaves primárias, 89 únicos além da PK, 65 filtrados |
| Chaves estrangeiras | **213** — 211 `NO ACTION`, 2 `CASCADE`; 105 obrigatórias, 108 opcionais; 13 compostas; 7 auto-referências; **1 ciclo** (`Tarefa` ↔ `Interacao`) |
| Restrições `CHECK` | **189** |
| Restrições `DEFAULT` | **48** |
| Restrições `UNIQUE` declaradas | **1** |
| Colunas identity | **80** |
| Tabelas particionadas por mês | **4** — `auditoria.AlteracaoDeCampo`, `auditoria.EventoDeAcesso`, `processo.RegraExecucao`, `integracao.Recepcao` |
| Views, procedures, funções, gatilhos, sinônimos, sequências | **0** |

### 2.2 Tabelas por schema

| Schema | Tabelas |
|---|---:|
| `processo` | 14 |
| `organizacao` | 12 |
| `comercial` | 11 |
| `integracao` | 11 |
| `metadado` | 9 (8 + histórico de migração) |
| `frota` | 8 |
| `seguranca` | 8 |
| `auditoria` | 3 |
| `relatorio` | 3 |
| `documento` | 2 |
| `dbo` | 1 (histórico de migração órfão) |
| **Total** | **82** |

### 2.3 As 12 migrações

| # | Migração |
|---:|---|
| 1 | `20260904120040_ModeloInicial` |
| 2 | `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade` |
| 3 | `20260905191622_MunicipioEAgrupamentoDeCarteira` |
| 4 | `20260906122942_VendaPerdida` |
| 5 | `20260906134331_NaturezaDeCarteiraEUsuario` |
| 6 | `20260906144125_FaturamentoEClasseDoCliente` |
| 7 | `20260908141700_QuebraDeFaturamentoEParceiroSemCliente` |
| 8 | `20260910113323_ParticipacaoNaNegociacaoDeixaDeSerBooleanoAnulavel` |
| 9 | `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial` |
| 10 | `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo` |
| 11 | `20260914180619_ModeloPendenteSoNaOrigemArt` |
| 12 | `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao` |

### 2.4 Os dados

| Item | Valor |
|---|---|
| Linhas no banco local | **6.147** |
| Linhas no banco central | 6.142 (documento 38) |
| Tabelas com dado | 11: `Municipio` 5.571, `Modelo` 253, `CatalogoItem` 227, `Familia` 28, `Empresa` 18, `Marca` 13, `__EFMigrationsHistory` 12, `Catalogo` 10, `LinhaDeProduto` 10, `Sistema` 4, `Usuario` 1 |
| Tabelas que nunca tiveram linha | **35** de domínio (36 físicas, com a órfã `dbo.__EFMigrationsHistory`) |
| Base anterior à sanitização | 975.461 linhas — preservada em `TracbelCrmArquivo20260915` (contêiner `tracbel-crm-ensaio`, porta 14334). **Somente leitura; não alterar.** |
| Backups | servidor: `...\MSSQL\Backup\TracbelCrm-arquivo-antes-da-limpeza-20260915-095042.bak`; estação: `dados-locais/bancos/` (documento 38) |

## 3. O código

| Item | Valor |
|---|---|
| Projetos | 6 em `src/` (Dominio, Aplicacao, Infraestrutura, Api, Carga, Integracao, Web) + 5 de teste |
| `DbSet` | 79 (uma entidade, `ItemConjuntoPermissao`, é acessada por `Set<T>`) |
| Configurações do EF | 12 arquivos + `LigacaoDeCatalogo.cs` |
| Repositórios | 17 + `UnidadeDeTrabalho` |
| Casos de uso | 34 |
| Rotas | 39 — 35 em `/api/v1`, 3 em `/auth`, 1 de saúde |
| Serviço em segundo plano | 1 — `ServicoDeSincronizacaoDoArt` (**parado e desabilitado**) |
| Testes | **355 métodos** (`[Fact]`/`[Theory]`) · **535 casos executados** na última medição |
| Frontend | 19 rotas declaradas, 12 telas de produto no menu |

## 4. O que muda a partir daqui

O [plano executivo](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md) leva estas 82 tabelas a 42, em nove fases.
A cada fase, a contagem esperada está no critério de aceite; qualquer divergência se compara com este
documento.

| Marco | Tabelas | FKs |
|---|---:|---:|
| **Hoje (este snapshot)** | 82 | 213 |
| Depois da fase 1 | 50 | — |
| Depois da fase 5 | 48 | — |
| Depois da fase 7 | 44 | — |
| **Alvo (fase 8)** | **42** | **86** |

## 5. Como refazer este snapshot

```powershell
.\scripts\banco\auditoria\gerar-inventario-do-banco.ps1
```

O script é somente leitura: consulta o catálogo de sistema, lê o código e regrava
`39A-INVENTARIO-DETALHADO-DO-BANCO.md`, `39B-DIAGRAMA-ER-ATUAL.md` e o JSON em
`dados-locais/auditoria/` (fora do Git). A identificação da versão se recalcula com o SHA-256 dos
arquivos de migração e do snapshot do modelo.
