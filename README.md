# CRM Tracbel

CRM próprio da Tracbel, construído para substituir gradualmente o Vórtice.

**Página do projeto:** https://claude.ai/code/artifact/2bbcde10-3bf1-4f95-8c1d-120969e9cb25

---

## Começar

```bash
git clone <repo> && cd tracbel-crm
dotnet restore
dotnet test        # 63 testes devem passar
```

Requisitos: **SDK do .NET** (ver seção "Versão do .NET" abaixo) e SQL Server 2019+ para as fases seguintes.

---

## Estrutura

```
tracbel-crm/
├── docs/
│   ├── projeto/        os 8 documentos do dossiê — comece pelo README de lá
│   └── pesquisa/       15 relatórios de investigação (Vórtice, Salesforce, Dynamics)
├── src/
│   ├── Tracbel.Crm.Dominio/          o CORAÇÃO. Zero dependência externa
│   ├── Tracbel.Crm.Aplicacao/        casos de uso
│   ├── Tracbel.Crm.Infraestrutura/   EF Core, Entra ID, jobs, outbox
│   ├── Tracbel.Crm.Integracao/       QUARENTENA do vocabulário do Vórtice
│   └── Tracbel.Crm.Api/              ASP.NET Core, hospedada no IIS
├── tests/
│   ├── Tracbel.Crm.Dominio.Testes/
│   ├── Tracbel.Crm.Aplicacao.Testes/
│   └── Tracbel.Crm.Arquitetura.Testes/   ← estes bloqueiam merge se a estrutura for violada
└── Directory.Build.props   ← a versão do .NET é declarada AQUI, e só aqui
```

**A regra de dependência:** as setas apontam sempre para dentro. `Api` conhece `Aplicacao`;
`Aplicacao` conhece `Dominio`; **`Dominio` não conhece ninguém.** Há teste automatizado que
falha o build se isso for violado.

---

## Versão do .NET

O `Directory.Build.props` está em **`net9.0`** hoje, porque é o único SDK instalado na máquina
de desenvolvimento — não porque seja a escolha certa.

| Versão | Tipo | Fim do suporte |
|---|---|---|
| .NET 8 | LTS | 10/11/2026 |
| .NET 9 | STS | 12/05/2026 — **já venceu** |
| **.NET 10** | **LTS** | **nov/2028** ← o alvo |

**Antes de qualquer coisa ir para produção:** instale o SDK do .NET 10 e troque a linha
`<TargetFramework>` no `Directory.Build.props`. É uma linha só — foi desenhado assim de propósito.

---

## O que já existe

| Peça | Situação |
|---|---|
| `EntidadeBase`, eventos de domínio, `Resultado<T>` | ✅ pronta, testada |
| Tipos de valor: `Telefone`, `Email`, `CpfCnpj` | ✅ pronta, testada — cada um corrige um defeito medido no Vórtice |
| `Lead` com ciclo de vida completo | ✅ pronta, testada |
| Motor de workflow + log de execução | ✅ pronta, testada |
| Portas (interfaces que o domínio exige) | ✅ declaradas |
| Persistência (EF Core, migrations) | ⬜ fase 0 |
| Autenticação Entra ID | ⬜ fase 0 |
| Motor de permissão | ⬜ fase 0 |
| API | ⬜ fase 0 |
| Frontend React | ⬜ fase 0 |

---

## Convenções

1. **Português no domínio, inglês no técnico.** `Conta`, `DarAndamento`; `Repository`, `Handler`.
2. **Nada de abreviação.** `ProcessoRepositorio`, não `ProcRepo`.
3. **Comentário explica o PORQUÊ.** `// incrementa o contador` é ruído.
4. **Marcador `[V]`** em todo comentário que registre uma lição do Vórtice. Torna rastreável a razão
   de cada decisão estranha — e evita que alguém "simplifique" de volta para o defeito.
   **Nunca remova um `[V]` sem entender o que ele evita.**
5. **Erro de negócio devolve `Resultado<T>`; erro de programação lança exceção.** Nunca o inverso.
6. **`async` sempre com `CancellationToken`**, propagado até o fim.
7. **Um arquivo, um tipo.** Passou de 400 linhas? Faz coisa demais, divida.
8. **Warning é erro.** `TreatWarningsAsErrors` está ligado — um aviso ignorado hoje é um bug depois.

---

## Testes

```bash
dotnet test                                          # tudo
dotnet test --filter Categoria=Dominio               # regra de negócio
dotnet test --filter Categoria=Workflow              # motor de regras
dotnet test --filter Categoria=Arquitetura           # a estrutura não foi violada
```

### No CI

Todo PR para a `main` roda o workflow **CI** (`.github/workflows/ci.yml`), com três checagens:
`backend` (build com aviso como erro, migration pendente e os testes contra um SQL Server 2022 de
verdade), `frontend` (lint e build) e `seguranca` (varredura de segredos em arquivos e histórico).
**Teste pulado quebra o CI** — o resumo da execução mostra a tabela por projeto e o motivo de cada pulado.

Para conferir os mesmos resultados na sua máquina, sem SQL Server:

```powershell
dotnet test --logger trx --results-directory TestResults
./scripts/ci/conferir-testes.ps1 -Pasta TestResults -PermitirPulados
```

Desenho e decisões: `docs/projeto/47-CI-CD.md`.

**Portão de qualidade da fase 1** (documento 06): cobertura de domínio ≥ 90%, **cobertura de regra
= 100%** (cada regra testada disparando *e não disparando*), matriz de autorização completa,
e cinco usuários reais completando três tarefas sem ajuda.

---

## Leitura obrigatória antes de codar

| Ordem | Documento |
|---|---|
| 1 | [`docs/projeto/00-BRIEFING.md`](docs/projeto/00-BRIEFING.md) |
| 2 | [`docs/projeto/03-ARQUITETURA.md`](docs/projeto/03-ARQUITETURA.md) |
| 3 | [`docs/projeto/04-MODELO-DADOS.md`](docs/projeto/04-MODELO-DADOS.md), seção 1 |
| 4 | [`docs/projeto/05-SEGURANCA.md`](docs/projeto/05-SEGURANCA.md), seções 2 e 5 |
| 5 | o seu plano em [`docs/projeto/07-PLANO-POR-COLABORADOR.md`](docs/projeto/07-PLANO-POR-COLABORADOR.md) |

Uma tarde de leitura. É o investimento que evita semanas de retrabalho.
