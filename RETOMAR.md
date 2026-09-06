# Onde estamos — 04/09/2026

Estado do projeto ao fim da rodada intensa. Tudo verificado com saída real, não presumido.

```
dotnet build       → sem erro, sem aviso
dotnet test        → 189 testes, todos passando (138 domínio + 39 arquitetura + 12 aplicação)
npm run build      → passa (src/Tracbel.Crm.Web)
npx tsc --noEmit   → limpo
docker compose     → container do PostgreSQL saudável, banco migrado
comparar-telas     → 15 de 15 rotas abaixo de 0,07% de diferença
```

---

## 1. O protótipo local — **completo**

As quinze telas portadas do protótipo do gerente, medidas contra a captura de referência:

| Rota | Diferença | | Rota | Diferença |
|---|---|---|---|---|
| `/` Visão 360 | **0,00%** | | `/oportunidades/nova` | 0,02% |
| `/agenda` | 0,01% | | `/equipamentos` | 0,02% |
| `/cobertura` | 0,01% | | `/relatorios/funil` | 0,01% |
| `/pipeline` | 0,01% | | `/relatorios/performance` | 0,02% |
| `/clientes` | 0,01% | | `/relatorios/cobertura` | 0,07% |
| `/clientes/84391` | 0,04% | | `/config` | 0,01% |
| `/equipamentos/1RW…` | 0,06% | | `/inicio-antigo` | 0,01% |
| `/oportunidades/1517613` | 0,04% | | | |

O que resta é ruído de suavização de fonte. Nenhuma diferença de layout, cor, espaçamento ou texto.

```bash
cd src/Tracbel.Crm.Web && npm run dev     # http://localhost:5173
cd scripts/prototipo && node comparar-telas.mjs
```

**Três defeitos encontrados e corrigidos durante a verificação**, todos com causa raiz identificada:
o rótulo do gráfico da Visão 360 cortava a primeira letra porque três componentes alteravam a fonte
padrão global do Chart.js; a coluna da tabela de Cobertura ficava 3 pixels mais larga porque o JSX
descarta um espaço que o HTML preserva entre dois botões; e o card da Home usava o título da rota em
vez do texto do original. Detalhe em [05-MELHORIAS-COMBINADAS](docs/prototipo/05-MELHORIAS-COMBINADAS.md).

---

## 2. O banco — **de pé, em PostgreSQL**

Decidido em 03/09/2026 ([doc 19](docs/projeto/19-DECISAO-POSTGRESQL.md)): sai SQL Server, entra
PostgreSQL 17 em container. Com isso o banco também roda em container, o que fecha o argumento de
portabilidade do [doc 12](docs/projeto/12-DECISAO-CONTAINERS.md).

**63 tabelas em 10 schemas**, exatamente a conta do [modelo unificado](docs/projeto/17-MODELO-UNIFICADO.md):

| Schema | Tabelas | | Schema | Tabelas |
|---|---|---|---|---|
| `processo` | 13 | | `organizacao` | 6 |
| `comercial` | 9 | | `frota` | 5 |
| `seguranca` | 8 | | `auditoria` | 3 |
| `metadado` | 8 | | `relatorio` | 3 |
| `integracao` | 6 | | `documento` | 2 |

Mais 425 partições mensais nas quatro tabelas de log e auditoria. Estrutura física: 730 colunas,
161 chaves estrangeiras, 106 restrições de verificação, 305 índices, e **zero colunas de data sem
fuso horário**. O Vórtice tem 767 tabelas e zero restrições de verificação.

```bash
./scripts/banco/subir-banco.ps1     # sobe o container, migra, mostra a contagem
./scripts/banco/resetar-banco.ps1
```

---

## 3. Documentação — 20 documentos de projeto

Novos nesta rodada: [14 padrão de banco](docs/projeto/14-PADRAO-DE-BANCO.md),
[15 glossário e nomes](docs/projeto/15-GLOSSARIO-E-NOMES.md),
[16 higienização de dados](docs/projeto/16-HIGIENIZACAO-DE-DADOS.md),
[17 modelo unificado](docs/projeto/17-MODELO-UNIFICADO.md),
[18 integração Protheus](docs/projeto/18-INTEGRACAO-PROTHEUS.md),
[19 decisão PostgreSQL](docs/projeto/19-DECISAO-POSTGRESQL.md).

Mais [dados-referencia/](dados-referencia/) com 20 catálogos e 306 itens, que é o que faz o
formulário do CEN puxar de tabela em vez de aceitar digitação.

---

## 4. O que vem agora

1. **Seed**: carregar os catálogos e os dados do protótipo no banco, passando pelo saneamento.
2. **API**: expor o que as telas consomem.
3. **Ligar as telas ao banco**, trocando só `src/dados/carregar.ts`.
4. **Refazer a Visão 360** como painel do cliente, conforme o combinado.
5. **Campos de seleção** vindos dos catálogos, no lugar de digitação.
6. **Estados, responsividade e acessibilidade**, tela por tela.
7. **Integração com o Protheus**, com o indicador de dado velho.

Detalhe de cada item em [05-MELHORIAS-COMBINADAS](docs/prototipo/05-MELHORIAS-COMBINADAS.md).

---

## 5. Pendências que dependem de outras pessoas

| Pendência | De quem |
|---|---|
| Geografia: o protótipo usa Mato Grosso, o Vórtice mostra as 16 filiais em São Paulo | Ricardo |
| Existe contrato corporativo com a Microsoft que cubra SQL Server sem custo? É o único risco capaz de reverter a decisão do banco | Ricardo / contratos |
| A Algar opera PostgreSQL? Com qual ferramenta de backup? | infraestrutura |
| Jobs do SQL Agent do Vórtice, hoje inacessíveis | DBA |
| Captura do SQL do cliente Gupta | DBA |
| Cinco correções urgentes no Vórtice, incluindo o backup de 29,9 GB exposto na rede | infraestrutura |

---

## 6. O repositório continua sem nenhum commit

Mais de cem arquivos versionáveis: a investigação do Vórtice, os vinte documentos, o protótipo
portado e o banco. **Nada disso está protegido por histórico.**
