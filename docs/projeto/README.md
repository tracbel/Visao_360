# Projeto CRM próprio — Tracbel

Documentação do projeto de construção de um CRM próprio para substituir gradualmente o Vórtice.

**Dossiê visual:** https://claude.ai/code/artifact/2bbcde10-3bf1-4f95-8c1d-120969e9cb25
**Peça de decisão (diretoria):** https://claude.ai/code/artifact/554494be-3b86-4655-9fa2-7c7a775ce6f0

---

## Por onde começar

| Se você… | Leia |
|---|---|
| **é novo no projeto** | [00-BRIEFING](00-BRIEFING.md) inteiro. Uma hora, e você entende tudo |
| **vai codar** | [03-ARQUITETURA](03-ARQUITETURA.md) → [04-MODELO-DADOS](04-MODELO-DADOS.md) → o seu plano em [07](07-PLANO-POR-COLABORADOR.md) |
| **quer questionar uma decisão** | [01-DIAGNOSTICO](01-DIAGNOSTICO-VORTICE.md) e [02-BENCHMARK](02-BENCHMARK-SALESFORCE-DYNAMICS.md) — todo número tem fonte |
| **precisa aprovar o investimento** | [13-PROGRAMA](13-PROGRAMA-POR-FASES-DIRETORIA.md) — é o documento da diretoria: escopo total, fases, portões, orçamento e critérios de cancelamento |
| **vai decidir onde o sistema roda** | [12-DECISAO-CONTAINERS](12-DECISAO-CONTAINERS.md) inteiro, em especial as seções 8, 18 e 19 |
| **vai mexer em segurança** | [05-SEGURANCA](05-SEGURANCA.md) inteiro |

---

## Os documentos

| # | Documento | O que contém |
|---|---|---|
| 00 | [Briefing](00-BRIEFING.md) | por que o projeto existe, diagnóstico, benchmark, tese, escopo, riscos, custo de não fazer |
| 01 | [Diagnóstico do Vórtice](01-DIAGNOSTICO-VORTICE.md) | o dossiê técnico, com a query que prova cada achado — e o que o legado acertou |
| 02 | [Benchmark Salesforce × Dynamics](02-BENCHMARK-SALESFORCE-DYNAMICS.md) | comparativo por área, com a decisão e a origem de cada uma |
| 03 | [Arquitetura](03-ARQUITETURA.md) | camadas, motor de workflow, pipeline de extensão, C# comentado, convenções, testes |
| 04 | [Modelo de dados](04-MODELO-DADOS.md) | DDL completo em T-SQL, comentado, com a lição que motivou cada decisão |
| 05 | [Segurança e permissões](05-SEGURANCA.md) | as 5 camadas, ordem de avaliação, LGPD, auditoria, hardening, matriz de teste |
| 06 | [Plano de implementação](06-PLANO-IMPLEMENTACAO.md) | 7 fases com infra, banco, backend, frontend e portão de qualidade |
| 07 | [Plano por colaborador](07-PLANO-POR-COLABORADOR.md) | um plano por pessoa, com entregas semanais e checklist de pronto |
| 08 | [Telas e navegação](08-TELAS-E-NAVEGACAO.md) | inventário de 11 capturas reais, 13 problemas medidos, mapa de navegação e wireframes das 6 telas da fase 1 |
| 09 | [Catálogo de regras da fase 1](09-CATALOGO-REGRAS-FASE1.md) | estágios, tarefas, desfechos e as 19 regras de Leads, prontos para virar seed |
| 10 | [Catálogo de fluxos e ordem de migração](10-CATALOGO-DE-FLUXOS.md) | os 62 modelos de processo cruzados com uso real: 17 vivos, 5 concentram 90%; define a ordem e o tamanho da migração |
| 11 | [Módulos, telas, permissões e licenças](11-MODULOS-TELAS-PERMISSOES.md) | os 4 executáveis, as 184 telas, as 11 políticas e o que está licenciado sem estar instalado |
| 12 | [Decisão de containers](12-DECISAO-CONTAINERS.md) | empacotamento, orquestração em dois degraus, topologia por ambiente, CI/CD, segredos, rollback, custo estimado — e as perguntas abertas para a infra |
| 13 | [Programa por fases — diretoria](13-PROGRAMA-POR-FASES-DIRETORIA.md) | o escopo TOTAL fatiado em 9 fases com portões, o espelho do Vórtice em números, linha do tempo, mapa de convivência, governança, orçamento, riscos e critérios de cancelamento |
| 14 | [Padrão de banco](14-PADRAO-DE-BANCO.md) | as regras que impedem o banco de virar o Vórtice, cada uma ligada ao teste que a verifica; ambiente local em container e gerador de dicionário |
| 15 | [Glossário e nomes](15-GLOSSARIO-E-NOMES.md) | as palavras que a Tracbel usa, os schemas com palavra inteira, o de-para das tabelas e as abreviações proibidas |
| 16 | [Higienização de dados](16-HIGIENIZACAO-DE-DADOS.md) | catálogo de saneamento por tipo de dado, domínios, deduplicação, LGPD, métricas de qualidade e as regras de migração |
| 17 | [Modelo unificado](17-MODELO-UNIFICADO.md) | **767 tabelas viram 63**: a varredura de duplicatas, as oito regras de unificação e o modelo tabela a tabela |
| 18 | [Integração Protheus](18-INTEGRACAO-PROTHEUS.md) | o que o Vórtice lê do ERP hoje, por que parou, quem é dono de qual dado e a rota recomendada |
| 19 | [Decisão PostgreSQL](19-DECISAO-POSTGRESQL.md) | ⛔ **substituído pelo 20** — fica como análise: por que se cogitou PostgreSQL, o que se perderia, a equivalência de tipos e a rota de reversão |
| 20 | [Decisão SQL Server](20-DECISAO-SQL-SERVER.md) | por que o banco é SQL Server e nasce dentro da instância do Vórtice, o histórico honesto da ida e volta, os números que mostram que desempenho não decidiu, as condições de convivência, a limitação da edição Standard e o que reverteria isso |
| 22 | [SOLID e padrões de projeto](22-SOLID-E-PADROES.md) | os cinco princípios no nosso código, com trecho real e o número do Vórtice que justifica cada um; os padrões que já usamos; SOLID no frontend React; quando **não** aplicar; as violações reais de hoje e os 13 testes que barram cada violação |
| 23 | [API do CRM](23-API.md) | as rotas de cliente, equipamento e catálogo que gravam no **nosso** banco; a **ponte de leitura somente-leitura** do Vórtice, com origem e data em toda resposta e o saneamento dos **72,3% de CNPJ sem zero à esquerda**; o formato de erro campo a campo; como o contexto de acesso funciona hoje (dois cabeçalhos, que **não autenticam ninguém**) e como vai funcionar com o Entra ID; a prova da fronteira por filial; e as **8 dívidas nomeadas** |
| 24 | [Carga de dados de 2026](24-CARGA-DE-DADOS-2026.md) | a primeira carga de dado real do Vórtice para o banco novo: o recorte (**24.218 clientes com atividade em 2026**, nas 13 filiais), o que veio por entidade e por filial, os **10.062 documentos com o zero à esquerda reconstituído**, as **15.906 linhas recusadas com motivo consultável**, as duas marcas d'água de truncamento novas (**20 caracteres em `Fantasia`, 40 em `NomeRazao`**), o defeito de idempotência que só a reexecução revelou, e o que tudo isso ensina para a migração completa |
| 25 | [Carga do relacionamento](25-CARGA-PROCESSO-AGENDA-CARTEIRA.md) | a segunda carga: **45.397 processos, 103.339 tarefas, 121.983 interações e 49.109 vínculos de carteira** de 2026, mais os 4 catálogos que nasceram do dado (**178 ações de 980, 484 desfechos de 4.209**); o achado de que **o duplo ponteiro da agenda só funciona numa direção** (13% de `UltHistorico` contra 113% de `AgendaOrigem`); as **11 decisões** que a carga teve de tomar — prazo ausente, status em branco, motivo de perda inexistente, usuário sem e-mail; a divergência medida contra `dados-referencia/`; as **7 métricas do protótipo que não têm dado real** e a API que as devolve vazias **com o motivo**; e as 9 rotas de leitura de Pipeline, Agenda, Cobertura e 360 |
| 26 | [Carteira e município](26-CARTEIRA-E-MUNICIPIO.md) | o agrupamento territorial que existe **de verdade** — a "regional" do protótipo não tem lastro (`IVS_Regional`: **zero linhas**), o que existe é **filial → carteira → municípios** (`IVS_CartCid`, 673 linhas em 91 carteiras); as duas tabelas novas (`organizacao.Municipio` e `organizacao.CarteiraMunicipio`) e por que **nenhuma das duas tem `EmpresaId`**; a carga de **9.750 municípios** e **532 vínculos** de carteira; a ligação de `comercial.Endereco` ao catálogo — **19.220 de 19.641 endereços casaram (97,86%)** e os **421 que não casaram cabem em 15 grafias**, 407 delas por um apóstrofo; a marca d'água de **truncamento em 20 caracteres em 947 nomes de cidade**; como o front passa a preencher o campo (busca por prefixo, sem botão de "criar município"); e o destino de `organizacao.Praca` |
| 27 | [Classe do cliente e cobertura](27-CLASSE-DO-CLIENTE-E-COBERTURA.md) | de onde vem a letra **A/B/C/D** de cada cliente e o prazo de visita de cada classe. As três colunas do legado que deveriam guardar a classe estão **vazias** (`IVS_Pes.Potencial` com 134.757 de 139.072 em branco e o resto guardando `64`/`43`/`22`; `IVS_Pes.Classe` **100% vazia**; `GE_Pessoa.Porte` com `.` e a palavra literal `string`), e `IVS_Pes.Ciclo` é nulo em **139.070 de 139.072** vínculos. O que **está** preenchido é a cadência por departamento — Máquinas **180/180/180/360**, Prospecção **120/120/120/180** — que a carga lia e **descartava na gravação**. A letra passa a ser apurada da **curva ABC** do faturamento real (**R$ 2,35 bi**, 6.998 clientes, três anos até 11/04/2025), por filial; e a tela passa a ter **quatro** estados de cobertura, não dois — coberto, fora da cadência, nunca contatado e **sem cadência declarada** |

**Pesquisa bruta e extração:**

| Onde | O que tem |
|---|---|
| [`../pesquisa/`](../pesquisa/) | 16 relatórios. Os 15 primeiros somam 387 achados e 249 entidades; o **16** (`16-vortice-ponta-a-ponta-*.md`) percorre o ciclo pessoa → processo → agenda → histórico com evidência |
| [`../dicionario/`](../dicionario/) | dicionário de dados das **767 tabelas**, gerado a partir do catálogo *(investigação profunda de 02/09/2026)* |
| [`../extracao-vortice/`](../extracao-vortice/) | código dos **548 objetos programáveis** (85 procedures, 51 functions, 1 trigger, 411 views), DDL completo, catálogos BPM, volumetria, domínios, segurança do banco e `binarios/` com o inventário de executáveis, DLLs e configs dos servidores *(02/09/2026)* |
| [`../prototipo/`](../prototipo/) | o protótipo do gerente congelado, os 45 arquivos de dados extraídos dele e as capturas de referência de cada tela *(03/09/2026)* |
| [`../banco/`](../banco/) | o dicionário do **nosso** banco, gerado do modelo a cada mudança, com diagrama por schema |

---

## Decisões travadas

Não reabra sem motivo novo. Cada uma custou várias rodadas de discussão.

| Decisão | Escolha |
|---|---|
| Rota | substituir o Vórtice gradualmente (*strangler fig*) |
| Stack | .NET 10 (LTS) + C# + EF Core + **SQL Server** + React/TypeScript |
| Banco | **SQL Server**, banco novo dentro da instância que já hospeda o Vórtice, decidido em 04/09/2026 ([doc 20](20-DECISAO-SQL-SERVER.md), que substitui o [19](19-DECISAO-POSTGRESQL.md)). Nomes em PascalCase, iguais em C# e no banco. Em desenvolvimento e CI, SQL Server 2022 em container |
| Autenticação | Microsoft Entra ID — o CRM não armazena senha |
| Primeiro domínio | Leads e Prospecção |
| Motor de workflow | declarativo próprio, com condição e log de execução obrigatório |
| Isolamento do legado | banco próprio (`TracbelCrm`), ainda que na mesma instância do Vórtice. **Nunca escrever no banco do Vórtice**, e a leitura só pela camada de integração — ver a ressalva da [seção 8.1 do doc 20](20-DECISAO-SQL-SERVER.md) |
| Time | 3 pessoas (A arquiteto, B backend, C frontend) |
| Empacotamento e execução | containers Linux para tudo que é nosso, **menos o banco**: em produção ele é um banco novo dentro da instância SQL Server existente ([doc 20](20-DECISAO-SQL-SERVER.md)) — o que é a regra original do [doc 12, seção 5](12-DECISAO-CONTAINERS.md), "container para o que é descartável; VM ou serviço gerenciado para o que guarda estado". Em desenvolvimento e CI o banco É container, porque lá ele é descartável. Docker Compose em VM até a fase 3, Kubernetes só com gatilho — **aguardando confirmação da infra** ([doc 12](12-DECISAO-CONTAINERS.md)) |

**Regra que não se quebra:** o vocabulário do Vórtice (`SeqPessoa`, `CodProcesso`, `IV_*`, `GE_*`)
só pode existir em `Tracbel.Crm.Integracao`. Há teste no CI que barra o merge se vazar.

---

## Estado

Planejamento concluído em 30/08/2026. O **esqueleto de código existe e roda**:
`dotnet test` na raiz do projeto executa **63 testes, todos passando**.

Falta a decisão de seguir e a montagem do time. Ver a peça de decisão acima e o [doc 13](13-PROGRAMA-POR-FASES-DIRETORIA.md).

**Investigação profunda do Vórtice concluída em 02/09/2026** (marco 0.5 do doc 13): dicionário das 767
tabelas, código dos 548 objetos programáveis, catálogos BPM exportados, volumetria, domínios, segurança
do banco, inventário de 1.397 arquivos dos servidores e o ciclo de vida ponta a ponta (pesquisa 16).
Ponto de entrada: [`../extracao-vortice/00-RELATORIO-EXTRACAO.md`](../extracao-vortice/00-RELATORIO-EXTRACAO.md)
e [`../extracao-vortice/binarios/INVENTARIO-BINARIOS.md`](../extracao-vortice/binarios/INVENTARIO-BINARIOS.md).
Único ponto cego restante: jobs do SQL Server Agent (depende do DBA) e a captura de SQL do cliente Gupta
([procedimento](../extracao-vortice/binarios/PROPOSTA-ABRIR-CAIXA-PRETA.md)).

**Correção importante:** o alvo do .NET mudou de 8 para **10 (LTS)** — o .NET 8 sai de suporte
em 10/11/2026 e o 9 já saiu em 12/05/2026. O `Directory.Build.props` está em `net9.0` porque é o
único SDK instalado na máquina de desenvolvimento; a troca é uma linha.
