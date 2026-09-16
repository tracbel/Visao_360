# Sanitização dos dados do CRM — base limpa para a reestruturação

> **Documento 38** · Versão 1.0 · 15/09/2026
> Os dados operacionais e técnicos antigos foram removidos do **banco central do servidor** e do **banco local
> de desenvolvimento**. A estrutura do banco foi preservada: nenhuma tabela, coluna, índice, restrição,
> relacionamento ou migração foi criado, alterado ou removido.
> Convenção: **[medido]** = obtido em 15/09/2026 por consulta ao banco, à API ou às telas.

---

## 0. Resumo

| Item | Resultado |
|---|---|
| Banco central (`AGRO-SISTEMAS-W` / `TracbelCrm`) | **975.466 → 6.142 linhas** em 79 s, COMMIT depois de todas as validações [medido] |
| Banco local (`tracbel-crm-db` / `TracbelCrm`) | **975.461 → 6.147 linhas** em 44 s [medido] |
| Estrutura | 82 tabelas, 213 chaves estrangeiras, 12 migrações — iguais antes e depois |
| Chaves estrangeiras e CHECK | 0 desabilitadas, 0 não confiáveis, 0 violações (`DBCC CHECKCONSTRAINTS`) |
| Ponto de restauração | backups de 15/09/2026 anteriores à limpeza (§2), intocados |
| Sincronização com o ART | **parada e desabilitada** (§9) |
| Sistema | API, telas, build, testes e lint OK (§8) |

---

## 1. Por que e o que foi decidido

O objetivo é deixar o CRM praticamente novo para revisar entidades, relacionamentos, módulos, telas,
permissões e integrações a partir de uma base limpa, sem perder a arquitetura. Os dados antigos continuam
disponíveis nos backups e numa cópia restaurada só para consulta.

Decisões do responsável, em 15/09/2026:

| Tema | Decisão |
|---|---|
| Onde limpar | banco central do servidor (protótipo ainda não liberado à empresa) e banco local |
| Filiais | preservar as 18 (13 ativas; Guaíra, Ituverava, Monte Alto e as duas "Colorado" inativas) |
| Usuários | manter só as contas que entram no app hoje: no servidor `ricardo.moretti@tracbel.com.br` e `hugo.rocha@tracbel.com.br`; no banco local o usuário fictício de desenvolvimento `cen.ribeiraopreto@tracbel.com.br` |
| Configuração herdada do Vórtice | apagar (tipos de processo, fases, tipos de tarefa, resultados, motivos de perda, linhas de negócio, regra de potencial) |
| Municípios | manter os que têm código do IBGE e remover a base antiga sem código |
| Sincronização do ART | continua parada e desabilitada até revisão específica |

---

## 2. Ponto de restauração — não mexer

| Onde | Arquivo ou banco | Tamanho | Conferência |
|---|---|---:|---|
| Servidor | `C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\Backup\TracbelCrm-arquivo-antes-da-limpeza-20260915-095042.bak` | 514,3 MB | `BACKUP ... COPY_ONLY, CHECKSUM` |
| Estação | `dados-locais\bancos\TracbelCrm-arquivo-antes-da-limpeza-20260915-095042.bak` (cópia do servidor) | 514,3 MB | hash SHA-256 igual ao do servidor |
| Estação | `dados-locais\bancos\TracbelCrm-local-arquivo-antes-da-limpeza-20260915-100112.bak` (banco local) | 514,2 MB | `RESTORE VERIFYONLY` válido |
| Contêiner `tracbel-crm-ensaio` (porta 14334) | banco **`TracbelCrmArquivo20260915`**, restaurado do backup do servidor, **só para consulta** | — | 23.945 clientes, 122.002 interações, 276 usuários, 9.863 municípios — conferido depois da limpeza |

`dados-locais/` está fora do Git. Cópias temporárias dos arquivos `.bak` também ficaram dentro dos
contêineres (`/var/opt/mssql/data/` em `tracbel-crm-db` e em `tracbel-crm-ensaio`); não foram apagadas.

**Restaurar o servidor** (só se decidido): restaurar o `.bak` do servidor sobre `TracbelCrm` com a API parada, pelo
DBeaver com uma conta administrativa da instância.

---

## 3. Classificação de todas as tabelas [medido]

A = estrutural · B = cadastro necessário · C = operacional · D = derivado/técnico. "Antes" e "depois" são do
servidor; o banco local tem os mesmos números, salvo onde indicado.

### 3.1 Preservadas (A e B)

| Tabela | Cat. | Antes | Depois | Justificativa |
|---|:-:|---:|---:|---|
| `metadado.__EFMigrationsHistory` | A | 12 | 12 | histórico de migrações do EF Core |
| `dbo.__EFMigrationsHistory` | A | 0 | 0 | histórico de migrações (vazio neste schema) |
| `metadado.Catalogo` / `CatalogoItem` | A | 10 / 227 | 10 / 227 | catálogos de domínio do seed, usados por formulários e validação da API |
| `frota.Marca` / `Familia` / `Modelo` | A | 13 / 22 / 253 | igual | catálogo de máquinas (local: 28 famílias) |
| `frota.LinhaDeProduto` | A | 10 | 10 | classificação de produto, criada por migração |
| `integracao.Sistema` | A | 4 | 4 | catálogo dos sistemas de origem |
| `seguranca.Permissao`, `ConjuntoDePermissao`, `ConjuntoDePermissaoItem`, `UsuarioConjuntoDePermissao` | A | 0 | 0 | estrutura de permissões e perfis |
| `auditoria.CampoAuditado`, `metadado.CampoPersonalizado`, `TratadorDeEvento`, `Formulario`, `Pergunta`, `processo.Regra`, `relatorio.Fonte`, `FonteCampo`, `Relatorio` | A | 0 | 0 | configuração (vazia) |
| `organizacao.Empresa` | B | 18 | 18 | filiais: fronteira de acesso do sistema |
| `seguranca.Usuario` | B | 276 | **2** | ficam os administradores (local: **1**) |
| `organizacao.Municipio` | B | 9.863 | **5.571** | ficam só os municípios com código do IBGE; 4.292 sem código removidos |

### 3.2 Removidas

| Tabela | Cat. | Antes (servidor) | Depois |
|---|:-:|---:|---:|
| `comercial.Cliente` | C | 23.945 | 0 |
| `comercial.Contato` / `ClienteContato` | C | 23.313 / 23.313 | 0 |
| `comercial.Endereco` | C | 19.641 | 0 |
| `comercial.ClienteCarteira` | C | 49.109 | 0 |
| `comercial.FaturamentoDoCliente` / `FaturamentoSemCliente` | C | 37.867 / 20.474 | 0 |
| `comercial.Lead`, `Alerta`, `CanalContato`, `ConsentimentoComunicacao` | C | 0 | 0 |
| `organizacao.Carteira` / `CarteiraMunicipio` | C | 142 / 532 | 0 |
| `organizacao.MunicipioDaAreaDeAtuacao` / `ResponsavelPeloMunicipio` | C | 238 / 609 | 0 |
| `organizacao.Meta`, `Praca`, `HierarquiaComercial` | C | 0 | 0 |
| `processo.Processo` (oportunidades e funil) | C | 45.402 | 0 |
| `processo.Tarefa` (agenda) | C | 103.353 | 0 |
| `processo.Interacao` (linha do tempo) | C | 122.002 | 0 |
| `processo.VendaPerdida` | C | 165 | 0 |
| `processo.InteracaoParticipante`, `PassagemDeFase`, `ItemDeProposta` | C | 0 | 0 |
| `frota.Equipamento` | C | 5.964 (local 5.963) | 0 |
| `frota.VendaDeMaquina` / `VinculoDeClienteComEquipamento` (ART) | C | 2.109 / 2.109 (local 2.108) | 0 |
| `frota.LeituraDeHorimetro` | C | 0 | 0 |
| `integracao.RegistroDeOrigem` / `CorrespondenciaDaOrigem` / `CompradorPendente` / `DivergenciaDeIntegracao` (ART) | C | 4.145 / 156 / 851 / 47 | 0 |
| `documento.Documento` / `Vinculo` | C | 0 / 0 | 0 |
| `seguranca.Equipe`, `EquipeMembro`, `CompartilhamentoDeRegistro` | C | 0 | 0 |
| `metadado.Preenchimento`, `Resposta` | C | 0 | 0 |
| `processo.TipoProcesso` / `Fase` / `TipoTarefa` / `Resultado` / `MotivoDePerda` | B (config. legado) | 16 / 50 / 179 / 484 / 8 | 0 |
| `organizacao.LinhaDeNegocio` / `RegraDePotencial` | B (config. legado) | 14 / 1 | 0 |
| `organizacao.AreaPlantadaNoMunicipio` | D | 45.582 | 0 |
| `integracao.ChaveExterna` (de-para do legado) | D | 401.909 | 0 |
| `integracao.MensagemDescartada` | D | 24.946 (local 24.948) | 0 |
| `integracao.PontoDeSincronismo` | D | 21 | 0 |
| `integracao.ExecucaoDeSincronizacao` | D | 16 (local 4) | 0 |
| `auditoria.AlteracaoDeCampo` | D | 6.046 (local 6.049) | 0 |
| `auditoria.EventoDeAcesso`, `integracao.Recepcao`, `MensagemDeSaida`, `processo.RegraExecucao` | D | 0 | 0 |

**Por que os técnicos saíram:** o de-para e as filas descrevem registros que não existem mais; o ponto de
sincronismo guardava a posição das cargas antigas, e mantê-lo faria uma carga futura recomeçar do meio sobre
um banco vazio; a trilha de auditoria e o histórico de sincronização se referem a registros removidos e
continuam no backup.

---

## 4. Como a exclusão respeitou os relacionamentos

- **Nenhuma restrição foi desabilitada.** As 57 tabelas foram esvaziadas na ordem natural das 213 chaves
  estrangeiras (filha antes da mãe), calculada da lista real do banco.
- **O único ciclo** é `processo.Tarefa` ↔ `processo.Interacao`. Ele foi quebrado com
  `processo.Interacao.TarefaId = NULL` antes da exclusão — a coluna aceita nulo, nenhuma CHECK a cita e não há
  gatilho no banco.
- **Auto-referências** (`frota.Equipamento` pai e substituto, `comercial.Cliente` matriz, `seguranca.Usuario`
  gestor) saíram no mesmo `DELETE` que esvaziou a tabela; os administradores mantidos não tinham gestor.
- **Tabelas preservadas não apontam para as removidas:** as quatro chaves que saem de tabela preservada
  (`Formulario`, `Regra`, `Relatorio`, `UsuarioConjuntoDePermissao`) estão em tabelas vazias.
- **Tudo numa transação.** As validações rodaram antes do COMMIT; qualquer falha teria desfeito tudo.

## 5. IDENTITY

Reiniciado **só depois do COMMIT** e só nas **36 tabelas esvaziadas que já tinham recebido linhas**: o próximo
registro volta a ser 1. As tabelas preservadas e as mantidas em parte (`Usuario`, `Municipio`) não foram
tocadas — o contador atual continua acima do maior Id existente (ex.: local `Usuario` 1450 > 902,
`Municipio` 11001 > 10114). As 34 colunas IDENTITY que nunca receberam linha ficaram como estavam.

## 6. Arquivos físicos

Não há arquivo órfão. `documento.Documento` tinha **0 linhas** nos dois bancos, o código não tem implementação
de upload ou armazenamento de anexo (só a coluna `CaminhoArmazenamento`), e o servidor não tem pasta de
upload, anexo ou documento em `C:\aplicacoes` nem em `D:\` [medido].

## 7. Execução

| Etapa | Local | Servidor |
|---|---|---|
| Simulação (tudo executado, validado e desfeito) | 72 s, 0 violações; banco conferido intacto depois | 181 s, 0 violações |
| Gravação (`-Confirmar`) | 44 s, GRAVADO | 79 s, GRAVADO |

Scripts:

- `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql` — o SQL completo, com a classificação das 82
  tabelas, as guardas (só roda no banco `TracbelCrm`, recusa tabela sem classificação e administrador
  inexistente), a transação, as validações e o relatório. Padrão é **simulação**.
- `scripts/banco/sanitizacao/executar-sanitizacao.ps1 -Onde Local|Servidor [-Confirmar]` — roda o SQL no
  contêiner local ou no servidor (por tarefa agendada temporária; a cadeia de conexão fica no servidor).
- No DBeaver: selecionar o arquivo inteiro e executar como **um lote** (Ctrl+Enter com o texto selecionado);
  "executar script" (Alt+X) separa os comandos e perde as variáveis.

## 8. Validação depois da limpeza [medido]

### 8.1 Banco

| Verificação | Servidor | Local |
|---|---|---|
| Tabelas com dados | 11: `Familia` 22, `LinhaDeProduto` 10, `Marca` 13, `Modelo` 253, `Sistema` 4, `__EFMigrationsHistory` 12, `Catalogo` 10, `CatalogoItem` 227, `Empresa` 18, `Municipio` 5.571, `Usuario` 2 | as mesmas 11, com `Familia` 28 e `Usuario` 1 |
| Tabelas vazias | 71 de 82 | 71 de 82 |
| Chaves estrangeiras / CHECK ruins | 0 / 0 | 0 / 0 |
| `DBCC CHECKCONSTRAINTS` | 0 violações | 0 violações |
| Órfãos | nenhum (coberto pelo `DBCC CHECKCONSTRAINTS`) | nenhum |
| Usuários | `hugo.rocha@tracbel.com.br`, `ricardo.moretti@tracbel.com.br` | `cen.ribeiraopreto@tracbel.com.br` |
| Migrações pendentes | 0 (`/saude/banco`) | — |

### 8.2 Aplicação

| Verificação | Resultado |
|---|---|
| API do servidor | `/saude/banco`: conectado, 0 migrações pendentes; rota de dados sem login responde 401 (exige Entra ID) |
| Login no servidor | contas mantidas com o vínculo do Entra ID; **o login interativo não foi testado pela sessão de trabalho** |
| API local, 21 rotas com o banco vazio | todas 200: clientes, equipamentos, processos, tarefas, interações e cobertura com total 0; agenda, funil, faturamento, CEN, cobertura, indicadores executivos, vendas perdidas, cobertura por filial e carteira e território respondendo; municípios (busca "Ribeir": 22); catálogos (18); sincronizações (0); nenhum erro no log |
| Login local | `/auth/eu` 200 em modo provisório com o administrador local |
| Telas locais | Visão 360 (Diretoria e CEN), Clientes, Equipamentos, Agenda, Pipeline, Cobertura, Funil, Performance, Cobertura por Filial, Indicadores Geográficos e Configurações: **nenhuma quebra, 0 erros de console, 0 respostas de erro da API**; capturas em `dados-locais/capturas/vazio-*.png` (`scripts/prototipo/capturar-crm-vazio.mjs`) |
| Build do backend | 0 erros, 0 avisos |
| Build do front (`tsc -b` e `vite build`) | OK |
| Lint do front (`oxlint`) | OK — só avisos que já existiam |
| Testes | **535 aprovados**: domínio 186, integração 133, API 90, aplicação 64, arquitetura 62. Os testes de banco real usam bancos próprios (`TracbelCrmMigracaoTeste`, `TracbelCrmColacaoTeste`) e não tocam no `TracbelCrm` — conferido depois da execução |

### 8.3 Situação final

| Item | Estado |
|---|---|
| API | OK |
| Frontend | OK |
| Login | OK local; servidor não testado interativamente |
| Banco | OK |
| Foreign Keys | OK |
| Build | OK |
| Testes | OK |
| Sincronização ART | **DESABILITADA** |

## 9. Sincronização com o ART

O serviço `TracbelCrmSincronizacaoArt` está **parado e com início desabilitado** no servidor [medido]. O
`publicar.ps1` foi ajustado para não religá-lo: se o serviço estiver desabilitado, a publicação avisa e o
mantém parado. Nenhuma rotina de inicialização da API o habilita. A reativação depende de revisão específica e
de autorização explícita.

## 10. O que continua pendente

| Ponto | Situação |
|---|---|
| Permissões e perfis | estrutura preservada e vazia; o acesso dos administradores vem do escopo padrão da API (filial escolhida e abaixo). Nenhuma concessão foi criada |
| Tela de Configurações | continua mostrando dados do protótipo (arquivos JSON), não do banco |
| Funil, agenda, linhas de negócio e território | sem configuração: serão redefinidos na reestruturação |
| Municípios | 5.571 com código do IBGE; área de atuação, responsáveis e área plantada precisam ser recarregados quando o território for revisto |
| Cópias de ensaio antigas | `TracbelCrmServidor`, `TracbelCrmServidorVazio` e `TracbelCrmEnsaioArt` continuam no contêiner de ensaio com os dados antigos, intocadas |
| Dados de desenvolvimento | nenhum dado fictício foi criado; o conjunto pequeno e controlado fica para a próxima etapa |
