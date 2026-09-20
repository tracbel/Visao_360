# 47 — CI/CD: integração contínua, regras da `main` e o caminho até a publicação

> **Data:** 17/09/2026 · **Status:** §3 **aprovada** por Ricardo em 17/09/2026; §4 e §5 propostas;
> §6 (CD) depende da decisão da #60.
> **Issues:** #56, #58, #59 (CI) · #60, #61, #62 (CD) — tabela na §8.
> **Relação com outros documentos:** substitui o escopo "CI mínimo" do plano 46A (#56); parte da
> recomendação do [documento 12](12-DECISAO-CONTAINERS.md) (§14 e §15) para a publicação.

---

## 0. Resumo

1. **Até 17/09/2026 não havia CI.** Os 514 testes só rodavam na estação de quem programa, e a `main`
   aceitava push direto.
2. **O CI roda em Linux**, nos runners hospedados pelo GitHub, mesmo com o servidor em Windows: o CI
   verifica o **código**, e o .NET e o React são os mesmos nos dois sistemas. (O alvo era .NET 9 no
   desenho; desde 19/09/2026 é **.NET 10**, pela issue #84 — o CI pega a versão do `global.json`, e a
   migração foi uma linha lá.)
3. **Cada PR roda três jobs em paralelo:** backend (com SQL Server de verdade em contêiner), frontend e
   segurança. **Nenhum teste pode ser pulado.**
4. **A `main` só muda por PR com o CI verde.** Não se exige aprovação formal, porque os PRs saem da conta
   do Ricardo e o GitHub não deixa o autor aprovar o próprio PR. **Quem faz o merge é o Ricardo.**
5. **A issue fecha sozinha no merge**, pelo `Closes #N` no corpo do PR.
6. **A publicação automática (CD) vem depois** e depende de uma decisão: continuar no Windows Server
   ou ir para Docker numa VM Linux, como o documento 12 já recomendava (#60).

---

## 1. Estado medido (17/09/2026)

| Item | Medido |
|---|---|
| Repositório | `tracbel/Visao_360`, privado, organização no plano **Team** (16 assentos) |
| Proteção da `main` | nenhuma — sem proteção de branch e sem ruleset |
| Automação | sem `.github/`, sem workflow, sem webhook, sem runner no repositório |
| Testes | 514 casos em 5 projetos; 9 pulam sem SQL Server (`FatoSeHouverSqlServer`, conexão por `ConnectionStrings__Crm`); cerca de 18 s na estação |
| Frontend | `npm run build` (`tsc -b && vite build`), `npm run lint` (oxlint), `package-lock.json` presente; Vite 8 e TypeScript 6 |
| Ferramentas fixadas | nenhuma: sem `global.json`, sem `.nvmrc`, `dotnet-ef` 9.0.10 só global (hoje: `global.json` em 10.0.401 e `dotnet-ef` 10.0.12, pela #84) |
| Dependência de Windows no código | `UseWindowsService()` na API e log de eventos do serviço do ART — os dois já protegidos por plataforma; testes montam caminhos com `Path.Combine` |
| Varredura de segredos | `scripts/seguranca/varrer-segredos.ps1` na `main` desde o #57 |
| Publicação | `scripts/deploy/publicar.ps1`, rodado da estação; backup `COPY_ONLY`; a API aplica as migrations ao subir |
| Servidor | Windows Server numa VM sem virtualização aninhada (registro de 14/09/2026): não roda contêiner Linux |

---

## 2. Decisões tomadas

| # | Decisão | Quem e quando |
|---|---|---|
| D-CI-1 | **CI antes do potencial de mercado**; o CD anda em paralelo quando a infraestrutura permitir | Ricardo, 17/09/2026 |
| D-CI-2 | **Checagens obrigatórias, sem aprovação formal obrigatória; Ricardo faz o merge** | Ricardo, 17/09/2026 |
| D-CI-3 | **CI em runners Linux** hospedados pelo GitHub ("conseguimos usar o linux mesmo nossa máquina sendo windows") | Ricardo, 17/09/2026 |
| D-CI-4 | Desenho da §3 aprovado ("está boa sim") | Ricardo, 17/09/2026 |

**Alternativas descartadas para o CI:** runner Windows (os testes de banco continuariam pulados, porque
runner Windows não sobe contêiner Linux, e o minuto conta em dobro na cota) e runner próprio na rede
(dependeria de autorização da infraestrutura já e executaria código de PR dentro da rede).

---

## 3. O que roda em cada PR — aprovada (#56)

Arquivo `.github/workflows/ci.yml`. Dispara em todo PR para a `main`, em todo push na `main` e sob
demanda. Um push novo no mesmo PR cancela a execução anterior. Permissão mínima: `contents: read`.

| Job (paralelos, Linux) | O que faz | Falha quando |
|---|---|---|
| **backend** | .NET fixado por `global.json`; restore com cache; `dotnet build -c Release -warnaserror`; sobe **SQL Server 2022 em contêiner** com colação `Latin1_General_CI_AI` e senha gerada e mascarada na própria execução; `dotnet test` com `ConnectionStrings__Crm`; `dotnet ef migrations has-pending-model-changes` pelo manifesto `.config/dotnet-tools.json`; resultados `.trx` anexados | erro ou aviso de compilação, teste falhando, **teste pulado**, migration faltando |
| **frontend** | Node 24 LTS por `.nvmrc`; `npm ci`; `npm run lint`; `npm run build` | lint, tipagem ou build |
| **seguranca** | checkout com histórico completo; varredura de segredos; `dotnet list package --vulnerable --include-transitive` e `npm audit` como **relatório** no resumo | segredo SUSPEITO (as vulnerabilidades só bloqueiam depois da #59) |

**Por que "teste pulado quebra o CI".** Hoje os 9 testes de banco pulam em silêncio quando não há SQL
Server. No CI, isso esconderia exatamente o que ele existe para provar.

**Por que a senha nasce na execução.** O SQL Server do CI é descartável: a senha é gerada, mascarada no
log e some com o contêiner. Nenhum segredo é cadastrado no GitHub para o CI.

**Arquivos novos:** `.github/workflows/ci.yml`, `global.json`, `.config/dotnet-tools.json`,
`src/Tracbel.Crm.Web/.nvmrc` e `scripts/ci/conferir-testes.ps1` (o conferidor que barra teste pulado).

**Tempo medido (17/09/2026, PR #82):** `backend` de 180 s a 190 s, `seguranca` de 67 s a 86 s, `frontend` de
23 s a 29 s. Como os jobs correm em paralelo, **cada PR espera cerca de 3 min 10 s**, abaixo da estimativa de
5 min. Uma execução gasta cerca de 4,8 minutos de runner somando os três jobs: a cota de 3.000 minutos por mês
do plano Team cobre por volta de 600 execuções.

**Prova de que o CI barra** (aceite da #56, no espírito da prova por mutação da #1), feita em 17/09/2026:

- a execução 35242539860 ficou **vermelha pelos dois motivos certos** — `backend` com 1 teste quebrado de
  propósito (`SincronizacaoDoArtTestes.Segredo_curto_demais_nao_apaga_a_mensagem_inteira`) e `seguranca` com
  1 SUSPEITO num arquivo com segredo inventado;
- o segredo foi **gerado na execução**, e não commitado: commitado, ele ficaria no histórico e a varredura o
  acusaria para sempre, e removê-lo exigiria force push;
- depois da reversão, a execução 35242984773 voltou ao verde, com 514/514/0 e **0 SUSPEITO** no histórico.

**O que o Linux revelou:** um teste de arquitetura lia `..\Projeto\Projeto.csproj` com
`Path.GetFileNameWithoutExtension`, que no Linux não separa na barra invertida. Corrigido no próprio teste.
Detalhes e links de todas as execuções: [`47A-PLANO-DE-IMPLEMENTACAO-DO-CI.md`](47A-PLANO-DE-IMPLEMENTACAO-DO-CI.md),
seção "Resultado".

---

## 4. Regras da `main` — proposta (#58)

### 4.1 Ruleset "main protegida"

| Regra | Valor |
|---|---|
| Alvo | branch padrão |
| Exigir PR | sim, com **0 aprovações** (D-CI-2) |
| Checagens exigidas | `backend`, `frontend`, `seguranca`, com a branch atualizada com a `main` |
| Force push e exclusão | bloqueados |
| Bypass | só o papel de administrador do repositório, e **só por PR** (emergência registrada no próprio PR) |
| Métodos de merge | squash (padrão) e merge commit; rebase desligado |

A regra só pode exigir checagens que já existam, por isso ela entra **depois** de a #56 estar na `main`
com o CI verde. O JSON abaixo é o que se aplica pela API (`POST /repos/tracbel/Visao_360/rulesets`); se a
ferramenta recusar a escrita, o mesmo conteúdo se aplica pela tela.

```json
{
  "name": "main protegida",
  "target": "branch",
  "enforcement": "active",
  "conditions": { "ref_name": { "include": ["~DEFAULT_BRANCH"], "exclude": [] } },
  "bypass_actors": [
    { "actor_id": 5, "actor_type": "RepositoryRole", "bypass_mode": "pull_request" }
  ],
  "rules": [
    { "type": "deletion" },
    { "type": "non_fast_forward" },
    {
      "type": "pull_request",
      "parameters": {
        "required_approving_review_count": 0,
        "dismiss_stale_reviews_on_push": false,
        "require_code_owner_review": false,
        "require_last_push_approval": false,
        "required_review_thread_resolution": false,
        "allowed_merge_methods": ["squash", "merge"]
      }
    },
    {
      "type": "required_status_checks",
      "parameters": {
        "strict_required_status_checks_policy": true,
        "required_status_checks": [
          { "context": "backend" },
          { "context": "frontend" },
          { "context": "seguranca" }
        ]
      }
    }
  ]
}
```

`actor_id` 5 é o papel de administrador do repositório nos rulesets; conferir na tela antes de ativar.

### 4.2 Configurações do repositório

| Configuração | Valor | Por quê |
|---|---|---|
| Squash | ligado, padrão | um commit por issue na `main` |
| Título e corpo do squash | título do PR e corpo do PR | a mensagem detalhada vive no PR e vira o commit |
| Merge commit | ligado | para PR cujos commits separados importam (como o #57, cujos hashes são citados nos documentos) |
| Rebase | desligado | reescreve os hashes |
| Apagar branch depois do merge | ligado | links apontam para o PR, não para a branch |
| Auto-merge | permitido | o Ricardo liga no PR e ele entra quando o CI ficar verde |

### 4.3 Convenções

- **Branch:** `tipo/ID-descricao-curta`, com os tipos `feat`, `fix`, `docs`, `ci`, `refactor`, `test`
  e `chore`. Ex.: `ci/056-workflow`, `feat/pot-09-potencial-estrutural`.
- **Título do PR:** Conventional Commits em português, como os commits de 16/09/2026
  (`feat(db): …`, `feat(seguranca): …`).
- **Corpo do PR:** o template. **`Closes #N`** fecha a issue no merge.
- **Template de PR** (`.github/pull_request_template.md`): resumo; `Closes #N`; o que mudou; como
  verificar; banco (migration? backup?); riscos e rollback; dados pessoais e segredos; checklist.
- **Template de issue** (`.github/ISSUE_TEMPLATE/tarefa.md`): o formato das issues do backlog — objetivo,
  contexto medido, escopo, fora de escopo, tarefas, aceite, dependências, autorização, riscos,
  rollback, dados pessoais e segredos, testes, documentação, DoD.

---

## 5. Dependências e vulnerabilidades — **aplicada em 20/09/2026** (#59)

### 5.1 O que ficou valendo

| Peça | Onde | O que faz |
|---|---|---|
| Dependabot | `.github/dependabot.yml` | PR **semanal** (segunda, 08:00, fuso de São Paulo) para NuGet, npm e GitHub Actions, com correção e versão menor **agrupadas num PR por ecossistema**. Versão **maior é ignorada** de propósito: subir de major é migração, com nota de versão para ler e analisador novo virando erro — nasce como issue, não como PR automático (foi o que a #84 mostrou) |
| Auditoria no `restore` | `Directory.Build.props` | `NuGetAudit` com `NuGetAuditMode=all` (**inclui transitivo**) e `NuGetAuditLevel=low` (relata tudo). Alta e crítica (NU1903/NU1904) viram **erro** pela regra "aviso é erro"; baixa e moderada (NU1901/NU1902) ficam como aviso, por `WarningsNotAsErrors` |
| Tabela e bloqueio no CI | `scripts/ci/conferir-vulneraveis.ps1`, job `seguranca` | lê o JSON do `dotnet list package --vulnerable --include-transitive`, imprime a tabela no resumo (projeto, pacote, versão, origem, severidade, se barra) e **sai com erro** em alta ou crítica sem exceção |
| npm | job `seguranca` | relatório completo no resumo e `npm audit --audit-level=high` como bloqueio |
| Exceção | `scripts/ci/vulneraveis-aceitas.json` | **hoje vazia**. Cada entrada precisa de `aviso`, `motivo`, `dono` e `ate` — exceção **vencida volta a bloquear sozinha** |

**O corte é alta e crítica, e não tudo.** Barrar moderada travaria o repositório em achados que muitas
vezes não têm correção publicada, e o time aprenderia a ignorar o bloqueio — que é o pior resultado
possível. O que não barra continua aparecendo na tabela.

### 5.2 O que foi medido em 20/09/2026

| Medida | Resultado |
|---|---|
| `dotnet list package --vulnerable --include-transitive` | **0** em 11 projetos |
| `npm audit --package-lock-only` | **0 vulnerabilidades** |
| Lista de exceções | **vazia** |
| Bloqueio de alta, provado | `System.Net.Http` 4.3.0 acrescentado de propósito: o **`restore` falhou** com `NU1903` (GHSA-7jgj-8wvc-jh57) antes mesmo do script — revertido |
| Detecção transitiva, provada | `Microsoft.AspNetCore.Authentication.JwtBearer` 6.0.0 arrastou `System.IdentityModel.Tokens.Jwt` 6.10.0 e `Microsoft.IdentityModel.JsonWebTokens` 6.10.0, **moderadas** (GHSA-59j7-ghrg-fj52): apareceram na tabela como "não (só relatório)" e o `restore` seguiu com código 0 — exatamente a política — revertido |

Fica fora: code scanning pago (GitHub Advanced Security) e varredura de imagem (entra com a #60, se Docker).

---

## 6. Publicação (CD)

O CI não publica nada. Duas issues cuidam da publicação, depois da decisão já tomada:

| Issue | O que resolve | Bloqueio |
|---|---|---|
| ~~#60~~ | **Onde o CRM roda — DECIDIDO em 17/09/2026: continua no Windows Server** (§6.1) | — |
| #61 | Runner **Windows** dentro da rede, só para publicação, com ambiente `producao` e aprovação obrigatória do Ricardo | autorização da infraestrutura |
| #62 | Publicação pelo pipeline: mesmo artefato do CI, backup com evidência, migration, saúde, reversão automática do código | #61; #51 antes da primeira migration destrutiva |

### 6.1 A decisão de 17/09/2026 e o que a sustentou

Ricardo indicou uma máquina Linux com Docker (`ecs-st-agro-sistemas-linux`, 10.150.4.227) e autorizou o
acesso. O inventário e os testes de rede, todos medidos naquele dia, levaram a **manter a aplicação no
Windows Server por enquanto**:

| Fato medido | Consequência |
|---|---|
| Da máquina Linux, **o Protheus não responde** (banco 1433 e REST 5891 fechados) | as cargas do faturamento e do cadastro (#18) não rodariam a partir dela |
| Da máquina Linux, **o SQL Server do Windows responde** (10.150.4.249:1433) | o caminho continua aberto no futuro, com o banco onde está |
| A máquina Linux tem **3,4 GB livres, sem swap**, e divide espaço com um projeto que usa 2,4 GB | sem folga para produção; o estouro cairia no vizinho |
| **O servidor Windows não roda WSL 2 nem contêiner Linux** — VM OpenStack sem VMX (doc 35 §12.2) | "usar o Linux do Windows" não existe como opção no servidor; o WSL fica como ferramenta da estação |
| Da máquina Linux, o SICOR, os metadados do IBGE e o login da Microsoft respondem | ela é candidata futura para as cargas de dados de mercado (#64 a #68) |

**Consequências práticas:** o runner da #61 é **Windows**; a publicação da #62 continua pelo `publicar.ps1`,
que já gera pacote **self-contained** e por isso não exige runtime no servidor — nem depois da migração para
o .NET 10 (#84). O acesso por chave preparado na máquina Linux fica válido para quando ela for usada.

**O que faria a decisão ser revista:** liberação do Protheus para o host Linux (ou aceitar as cargas no
Windows), máquina Linux com folga de memória e atualização do registro do aplicativo no Entra ID.

**O que o documento 12 já decidiu e continua valendo:** a tag é o SHA do commit; produção recebe o mesmo
artefato que passou antes; aprovação manual só para produção; a verificação depois da publicação faz parte
dela; schema destrutivo volta por restauração de backup.

**O que o CD não substitui:** a autorização de aplicar as fases 1 e 2 ao banco central (#51) e a de pôr o
faturamento do Protheus no servidor (#18).

---

## 7. Ordem de implantação

1. ~~#57 na `main`~~ — feito em 16/09/2026 (traz o script de varredura que o job `seguranca` usa).
2. **#56** numa branch `ci/056-workflow`: primeira execução, correção das diferenças do Linux (caixa de
   nomes, caminhos, fim de linha) até ficar verde; provas de bloqueio; merge pelo Ricardo. — **Feito em
   17/09/2026 no PR #82** (três verdes seguidas e as duas provas); falta o merge.
3. **#58** com a `main` já verde: ruleset, configurações e templates.
4. PRs abertos são atualizados com a `main` para rodar o CI antes do merge.
5. **#59**: Dependabot e bloqueio por vulnerabilidade.
6. **#60 → #61 → #62**, quando a infraestrutura permitir.

**Critério de pronto do CI:** três execuções verdes seguidas; 514 testes executados e 0 pulados; as duas
provas de bloqueio registradas; tempo medido anotado na §3.

---

## 8. Issues

| Código | Issue | Título | Milestone | Prioridade |
|---|---|---|---|---|
| CICD-01 | #56 | CI: build, testes com SQL Server, lint e varredura de segredos | M0 — Foundation | P0 |
| CICD-02 | #58 | Regras da main: PR obrigatório, checagens exigidas e fechamento automático das issues | M0 — Foundation | P0 |
| CICD-03 | #59 | Dependabot e bloqueio de pacotes vulneráveis | M9 — Hardening | P2 |
| CD-01 | #60 | Decisão: onde o CRM roda em produção — Windows Server ou Docker em VM Linux | M10 — Entrega contínua e hospedagem | P1 |
| CD-02 | #61 | Runner na rede da Tracbel e ambiente de produção com aprovação | M10 | P1 |
| CD-03 | #62 | Publicação pelo pipeline: artefato do CI, backup, migração, saúde e reversão | M10 | P1 |

A #56 existia no plano 46A como "[055] CI mínimo" (build, testes sem contêiner e lint). Em 17/09/2026 o
escopo foi trocado pelo desenho da §3, e ela subiu de P1 para P0 e de M9 para M0.

---

## 9. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Diferença de plataforma aparece na primeira execução (caixa de nome de arquivo, fim de linha) | média | baixo | corrigir na #56 antes do merge; é exatamente o que o CI deve revelar |
| Contêiner do SQL Server demora a subir e o teste falha por tempo | média | baixo | esperar a saúde do contêiner antes dos testes |
| Cota de minutos | baixa | médio | cancelamento de execução antiga; cache de NuGet e npm |
| Regra da `main` trava merges | baixa | médio | bypass de administrador só por PR; ruleset pode ser desativado |
| Escrita de configuração recusada pela ferramenta de automação | média | baixo | JSON versionado na §4.1 para aplicar pela tela |
| Runner na rede (CD) executa código | — | alto | só publicação aprovada a partir da `main` (#61) |
