# Linked servers do banco CRM

| Nome | Produto | Provider | Data source | Catalog | É linked | Acesso a dados | RPC out | Alterado em |
|---|---|---|---|---|:-:|:-:|:-:|---|
| `COLWCRM` | SQL Server | SQLNCLI | COLWCRM | — | False | False | True | 21/02/2022 09:30:09 |
| `TOTVS` | totvs6 | MSDASQL | totvs6 | — | True | True | False | 11/08/2022 10:13:50 |

## Leitura

- **`COLWCRM`** não é um linked server: é o **próprio servidor local** (`is_linked = False`).
  O nome revela a origem histórica da instalação (Colorado Máquinas, antes da Tracbel Agro) e
  reaparece no argumento do job `VORTICOSERVER` (`COLWCRMBD`).
- **`TOTVS`** é o único linked server de verdade. Aponta para `totvs6` via **MSDASQL**, ou seja, um
  **DSN ODBC configurado no Windows do servidor de banco**. A string de conexão efetiva (driver,
  host, porta, usuário) **não está no SQL Server**: está no registro/ODBC do host. Isso importa —
  a integração com o ERP depende de configuração que vive fora do banco e fora de qualquer
  repositório versionado.
- `is_rpc_out_enabled = False` no TOTVS: o CRM **lê** do ERP, não executa procedures remotas.
- `sys.linked_logins` devolveu 0 linhas para o login de leitura, então **não foi possível ver o
  mapeamento de credenciais** do linked server. É um item para o DBA.
- `provider_string` veio vazia para os dois servidores; se algum dia vier preenchida, o script
  `06-seguranca-banco.ps1` já mascara usuário e senha antes de gravar o CSV.

## Objetos que dependem do linked server

| Objeto que referencia | Servidor | Banco remoto | Entidade remota |
|---|---|---|---|
| `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS` | TOTVS | TMPRD | `X_V_BI_FATURAMENTO_PECAS` |
| `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS` | TOTVS | TMPRD | `X_V_BI_FATURAMENTO_SERVICOS` |
| `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS_N` | TOTVS | TMPRD | `X_V_COL_CLIENTE_CRM` |
| `X_P_REL_001` | TOTVS | TMPRD | `X_V_COL_CRM_PESSOA` |
| `X_TOTVS_ATUA_CRM_FATURAMENTO` | TOTVS | TMPRD | `X_V_CRM_FATURAMENTO_MAQUINAS` |
| `X_TOTVS_ATUA_CRM_FATURAMENTO` | TOTVS | TMPRD | `X_V_CRM_FATURAMENTO_PECAS` |
| `X_TOTVS_ATUA_CRM_FATURAMENTO` | TOTVS | TMPRD | `X_V_CRM_FATURAMENTO_SERVICOS` |
| `X_P_IMP_CRM_TITULO` | TOTVS | TMPRD | `X_V_IMP_CRM_TITULO` |

São **8 referências remotas**, todas para o banco `TMPRD` do TOTVS.

> **Cuidado ao interpretar este número.** O catálogo do SQL Server só registra a dependência
> quando o objeto foi compilado com o nome de quatro partes (`TOTVS.TMPRD.dbo.tabela`).
> Consultas montadas com `OPENQUERY` ou SQL dinâmico **não aparecem aqui** — a superfície real
> de integração com o ERP pode ser maior.

