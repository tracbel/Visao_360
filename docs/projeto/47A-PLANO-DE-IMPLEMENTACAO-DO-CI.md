# 47A — Plano de implementação do CI (#56)

> **Execução:** inline, uma tarefa por vez, seguindo `superpowers:executing-plans` — **sem subagentes**,
> por regra do Ricardo. Os passos usam caixas (`- [ ]`) para acompanhar.

**Objetivo:** todo PR para a `main` roda build, os 514 testes (inclusive os de banco, contra um SQL
Server de verdade), lint e build do frontend e a varredura de segredos, e fica vermelho se qualquer
coisa falhar ou se algum teste for pulado.

**Arquitetura:** um workflow do GitHub Actions com três jobs em paralelo em runners Linux. O backend
sobe um SQL Server 2022 descartável com `docker run`, com senha gerada e mascarada na execução. Um script
PowerShell confere os `.trx` resultado por resultado e barra teste pulado. Versões de SDK, ferramenta
e Node ficam fixadas em arquivos do repositório.

**Tecnologia:** GitHub Actions (`checkout@v7`, `setup-dotnet@v6`, `setup-node@v7`, `cache@v6`,
`upload-artifact@v7`), .NET SDK 9.0.306, `dotnet-ef` 9.0.10, Node 24, PowerShell 7, SQL Server 2022.

**Especificação:** [`47-CI-CD.md`](47-CI-CD.md), §3 (aprovada em 17/09/2026) — no PR #81 até o merge.

## Restrições globais

- Runners **Linux** hospedados pelo GitHub (`ubuntu-latest`); nada de runner próprio.
- Permissão do workflow: `contents: read`, e `persist-credentials: false` no checkout.
- **Nenhum segredo cadastrado no GitHub.** A senha do SQL Server nasce, é mascarada e morre na execução.
- Colação do SQL Server do CI: `Latin1_General_CI_AI` (a mesma do `infra/.env.exemplo`).
- **Teste pulado quebra o CI.** Aviso de compilação quebra o CI (`-warnaserror`).
- Vulnerabilidades de pacote: **só relatório** até a #59.
- Nomes dos jobs exatamente `backend`, `frontend` e `seguranca` — a #58 vai exigi-los pelo nome.
- Push e PR autorizados; **merge só pelo Ricardo**; nada de force push nem reescrita de histórico.
- Adicionar arquivos **pelo nome** (`git add <arquivo>`): a pasta `360/` não é ignorada nesta branch.

## Fatos medidos que o plano usa (17/09/2026)

| Fato | Consequência |
|---|---|
| `Directory.Build.props` já tem `TreatWarningsAsErrors` e `GenerateDocumentationFile` | `dotnet build -c Release -warnaserror` local: **0 avisos, 0 erros** (55 s) |
| No `.trx`, teste pulado do xUnit sai como `UnitTestResult outcome="NotExecuted"`, e o contador `notExecuted` do resumo fica **0** (arquitetura: `total=64 executed=55 notExecuted=0`) | o conferidor conta resultado por resultado; olhar o resumo deixaria passar o pulado |
| Testes de banco conectam em `master` (`ConexaoDeSqlServer.Verificar`) e criam o próprio banco (`TracbelCrmMigracaoTeste`) | `ConnectionStrings__Crm` do CI aponta para `master` com o usuário `sa` do contêiner |
| `package-lock.json` (v3) traz os binários `linux-x64` de oxlint, rolldown e lightningcss | `npm ci` funciona no Linux |
| `dotnet ef migrations has-pending-model-changes --project src/Tracbel.Crm.Infraestrutura/... --startup-project src/Tracbel.Crm.Api/... --configuration Release --no-build` | local: "No changes have been made to the model since the last migration.", código 0 |
| `scripts/seguranca/varrer-segredos.ps1` usa `git ls-files` (barras normais) e UTF-8; nenhum arquivo versionado tem nome com acento | roda igual no PowerShell 7 do Linux |
| `setup-dotnet` só faz cache com `packages.lock.json`, que o projeto não tem | cache do NuGet por `actions/cache@v6` em `~/.nuget/packages` |
| `TestResults/`, `bin/`, `obj/` e `node_modules/` já estão no `.gitignore` | nada a mudar no `.gitignore` |

## Arquivos

| Arquivo | Responsabilidade |
|---|---|
| `global.json` (novo) | fixa o SDK do .NET em 9.0.306 |
| `.config/dotnet-tools.json` (novo) | fixa o `dotnet-ef` em 9.0.10 para `dotnet tool restore` |
| `src/Tracbel.Crm.Web/.nvmrc` (novo) | fixa o Node em 24 |
| `scripts/ci/conferir-testes.ps1` (novo) | lê os `.trx`, resume por projeto e falha com teste falho, pulado ou ausente |
| `.github/workflows/ci.yml` (novo) | os jobs `backend`, `frontend` e `seguranca` |
| `README.md` (alterado, seção "Testes") | como ler o CI e como rodar o conferidor localmente |
| este arquivo | o plano e, no fim, o resultado medido |

---

### Tarefa 1: fixar SDK, ferramenta e Node

**Arquivos:**
- Criar: `global.json`
- Criar: `.config/dotnet-tools.json`
- Criar: `src/Tracbel.Crm.Web/.nvmrc`

**Interfaces:**
- Produz: `global.json` (lido por `setup-dotnet` com `global-json-file`), manifesto de ferramentas
  (lido por `dotnet tool restore`), `.nvmrc` (lido por `setup-node` com `node-version-file`).

- [ ] **Passo 1: criar o `global.json`**

```json
{
  "sdk": {
    "version": "9.0.306",
    "rollForward": "latestFeature"
  }
}
```

- [ ] **Passo 2: conferir que o SDK resolve**

Rodar: `dotnet --version`
Esperado: `9.0.306` (ou um 9.0.3xx mais novo, pelo `latestFeature`).

- [ ] **Passo 3: criar o manifesto e instalar o `dotnet-ef` local**

Rodar:
```powershell
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 9.0.10
```
Esperado: `.config/dotnet-tools.json` com `"dotnet-ef": { "version": "9.0.10", ... }`.

- [ ] **Passo 4: conferir a ferramenta pelo manifesto e a checagem de migration**

Rodar:
```powershell
dotnet tool restore
dotnet tool run dotnet-ef --version
dotnet build Tracbel.Crm.sln -c Release -warnaserror --nologo -v q
dotnet tool run dotnet-ef migrations has-pending-model-changes --project src/Tracbel.Crm.Infraestrutura/Tracbel.Crm.Infraestrutura.csproj --startup-project src/Tracbel.Crm.Api/Tracbel.Crm.Api.csproj --configuration Release --no-build
```
Esperado: versão `9.0.10`; build com 0 avisos e 0 erros; "No changes have been made to the model since
the last migration." com código 0.

- [ ] **Passo 5: criar o `.nvmrc`**

Conteúdo de `src/Tracbel.Crm.Web/.nvmrc` (uma linha):

```text
24
```

- [ ] **Passo 6: commit**

```powershell
git add global.json .config/dotnet-tools.json src/Tracbel.Crm.Web/.nvmrc
git commit -m "ci: fixa o SDK do .NET, o dotnet-ef e o Node usados no CI"
```

---

### Tarefa 2: conferidor de resultados de teste

**Arquivos:**
- Criar: `scripts/ci/conferir-testes.ps1`

**Interfaces:**
- Consome: arquivos `.trx` produzidos por `dotnet test --logger trx --results-directory <pasta>`.
- Produz: `./scripts/ci/conferir-testes.ps1 -Pasta <pasta> [-PermitirPulados]`. Código 0 quando todo
  teste passou; 1 com falha, pulado (sem `-PermitirPulados`) ou nenhum resultado. Grava o resumo em
  `$env:GITHUB_STEP_SUMMARY` quando a variável existe.

- [ ] **Passo 1: gerar `.trx` reais sem SQL Server (9 testes pulados)**

Rodar:
```powershell
dotnet test Tracbel.Crm.sln -c Release --no-build --nologo -v q --logger trx --results-directory TestResults
```
Esperado: 5 arquivos `.trx` em `TestResults/`; arquitetura com "Skipped: 9".

- [ ] **Passo 2: rodar o conferidor antes de ele existir**

Rodar: `./scripts/ci/conferir-testes.ps1 -Pasta TestResults`
Esperado: FALHA — o arquivo não existe.

- [ ] **Passo 3: escrever o conferidor**

```powershell
<#
.SYNOPSIS
    Confere os resultados .trx do `dotnet test` e falha se algum teste falhou, se algum foi pulado
    ou se não há resultado nenhum.

.DESCRIPTION
    Issue #56 (docs/projeto/47-CI-CD.md, §3).

    POR QUE CONTAR RESULTADO POR RESULTADO. No .trx, o contador `notExecuted` do resumo fica em 0
    mesmo quando o xUnit pula testes: o pulado só aparece como `UnitTestResult` com
    `outcome="NotExecuted"`. Medido em 17/09/2026 no projeto de arquitetura: total=64, executed=55,
    notExecuted=0, com 9 testes pulados. Olhar só o resumo deixaria passar exatamente o que o CI
    existe para barrar: os testes de banco pulando em silêncio.

    Imprime a contagem por projeto e a lista de testes pulados ou com falha (nome e o começo da
    mensagem). Dentro do GitHub Actions, grava o mesmo resumo em $env:GITHUB_STEP_SUMMARY.

.PARAMETER Pasta
    Pasta com os arquivos .trx. Procura também nas subpastas.

.PARAMETER PermitirPulados
    Não falha por teste pulado. Só para uso local, quando não há SQL Server.

.EXAMPLE
    ./scripts/ci/conferir-testes.ps1 -Pasta TestResults
#>
param(
    [Parameter(Mandatory)] [string] $Pasta,
    [switch] $PermitirPulados
)

$ErrorActionPreference = 'Stop'
$espacoDeNomes = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }

function Contar([object[]] $resultados, [string] $situacao) {
    @($resultados | Where-Object { $_.outcome -eq $situacao }).Count
}

$arquivos = @(Get-ChildItem -LiteralPath $Pasta -Filter '*.trx' -Recurse -File -ErrorAction SilentlyContinue)
if ($arquivos.Count -eq 0) {
    Write-Host "Nenhum arquivo .trx em '$Pasta'. O dotnet test rodou com --logger trx?"
    exit 1
}

$projetos = [System.Collections.Generic.List[object]]::new()
$problemas = [System.Collections.Generic.List[object]]::new()

foreach ($arquivo in $arquivos) {
    [xml] $trx = Get-Content -LiteralPath $arquivo.FullName -Raw
    $resultados = @(Select-Xml -Xml $trx -XPath '//t:UnitTestResult' -Namespace $espacoDeNomes |
        ForEach-Object Node)

    $primeiroTeste = Select-Xml -Xml $trx -XPath '//t:UnitTest' -Namespace $espacoDeNomes | Select-Object -First 1
    $nome = if ($primeiroTeste) {
        [System.IO.Path]::GetFileNameWithoutExtension([string] $primeiroTeste.Node.storage)
    } else { $arquivo.BaseName }

    $passou = Contar $resultados 'Passed'
    $pulado = Contar $resultados 'NotExecuted'
    $projetos.Add([pscustomobject]@{
        Projeto = $nome
        Total   = $resultados.Count
        Passou  = $passou
        Falhou  = $resultados.Count - $passou - $pulado
        Pulado  = $pulado
    })

    foreach ($r in $resultados | Where-Object { $_.outcome -ne 'Passed' }) {
        $mensagem = [string] $r.Output.ErrorInfo.Message
        if (-not $mensagem) { $mensagem = [string] $r.Output.StdOut }
        $mensagem = ($mensagem -replace '\s+', ' ').Trim()
        if ($mensagem.Length -gt 200) { $mensagem = $mensagem.Substring(0, 200) + '…' }

        $problemas.Add([pscustomobject]@{
            Situacao = if ($r.outcome -eq 'NotExecuted') { 'PULADO' } else { "FALHOU ($($r.outcome))" }
            Teste    = [string] $r.testName
            Mensagem = $mensagem
        })
    }
}

$total = ($projetos | Measure-Object Total -Sum).Sum
$passouTotal = ($projetos | Measure-Object Passou -Sum).Sum
$falhouTotal = ($projetos | Measure-Object Falhou -Sum).Sum
$puladoTotal = ($projetos | Measure-Object Pulado -Sum).Sum

$saida = [System.Collections.Generic.List[string]]::new()
$saida.Add('## Testes')
$saida.Add('')
$saida.Add('| Projeto | Total | Passou | Falhou | Pulado |')
$saida.Add('|---|---:|---:|---:|---:|')
foreach ($p in $projetos | Sort-Object Projeto) {
    $saida.Add("| $($p.Projeto) | $($p.Total) | $($p.Passou) | $($p.Falhou) | $($p.Pulado) |")
}
$saida.Add("| **Total** | **$total** | **$passouTotal** | **$falhouTotal** | **$puladoTotal** |")

if ($problemas.Count -gt 0) {
    $saida.Add('')
    $saida.Add('| Situação | Teste | Mensagem |')
    $saida.Add('|---|---|---|')
    foreach ($p in $problemas | Sort-Object Situacao, Teste) {
        $saida.Add("| $($p.Situacao) | $($p.Teste) | $($p.Mensagem -replace '\|', '/') |")
    }
}

$saida | ForEach-Object { Write-Host $_ }
if ($env:GITHUB_STEP_SUMMARY) {
    $saida | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}

if ($total -eq 0) {
    Write-Host 'Nenhum resultado de teste nos arquivos .trx.'
    exit 1
}
if ($falhouTotal -gt 0) {
    Write-Host "$falhouTotal teste(s) falharam."
    exit 1
}
if ($puladoTotal -gt 0 -and -not $PermitirPulados) {
    Write-Host "$puladoTotal teste(s) pulados. No CI todo teste precisa rodar — o motivo está na tabela acima."
    exit 1
}

if ($puladoTotal -gt 0) {
    Write-Host "OK com pulados permitidos: $passouTotal aprovados e $puladoTotal pulados, de $total."
} else {
    Write-Host "OK: $total testes, todos executados e aprovados."
}
exit 0
```

- [ ] **Passo 4: com os 9 pulados, o conferidor falha**

Rodar: `./scripts/ci/conferir-testes.ps1 -Pasta TestResults; "código: $LASTEXITCODE"`
Esperado: tabela com Total 514, Pulado 9; 9 linhas `PULADO` com "Nenhum SQL Server respondendo";
"9 teste(s) pulados"; **código 1**.

- [ ] **Passo 5: com `-PermitirPulados`, passa**

Rodar: `./scripts/ci/conferir-testes.ps1 -Pasta TestResults -PermitirPulados; "código: $LASTEXITCODE"`
Esperado: **código 0**.

- [ ] **Passo 6: com um teste falho, falha; com pasta vazia, falha**

Rodar (cópia fora do repositório, com um resultado trocado para `Failed`):
```powershell
$prova = Join-Path $env:TEMP 'claude\prova-conferidor'
New-Item -ItemType Directory -Force $prova | Out-Null
$origem = Get-ChildItem TestResults -Filter *.trx | Select-Object -First 1
$texto = Get-Content -LiteralPath $origem.FullName -Raw
$regex = [regex]'outcome="Passed"'
$regex.Replace($texto, 'outcome="Failed"', 1) | Set-Content -LiteralPath (Join-Path $prova 'falho.trx') -Encoding utf8
./scripts/ci/conferir-testes.ps1 -Pasta $prova -PermitirPulados; "código com falha: $LASTEXITCODE"
$vazia = Join-Path $env:TEMP 'claude\prova-conferidor-vazia'
New-Item -ItemType Directory -Force $vazia | Out-Null
./scripts/ci/conferir-testes.ps1 -Pasta $vazia; "código sem trx: $LASTEXITCODE"
```
Esperado: "1 teste(s) falharam" com **código 1**; "Nenhum arquivo .trx" com **código 1**. (O resumo do
`.trx` usa `outcome="Completed"`; o primeiro `outcome="Passed"` é sempre de um `UnitTestResult`.)

- [ ] **Passo 7: commit**

```powershell
git add scripts/ci/conferir-testes.ps1
git commit -m "ci: conferidor dos resultados de teste que barra teste pulado"
```

---

### Tarefa 3: o workflow

**Arquivos:**
- Criar: `.github/workflows/ci.yml`

**Interfaces:**
- Consome: `global.json`, `.config/dotnet-tools.json`, `src/Tracbel.Crm.Web/.nvmrc` (tarefa 1);
  `scripts/ci/conferir-testes.ps1 -Pasta TestResults` (tarefa 2);
  `scripts/seguranca/varrer-segredos.ps1 -Relatorio <arquivo>` (issue #1: código 1 com SUSPEITO).
- Produz: checagens `backend`, `frontend` e `seguranca`, que a #58 vai exigir.

- [ ] **Passo 1: escrever o workflow**

```yaml
# CI do CRM Tracbel — issue #56, desenho em docs/projeto/47-CI-CD.md §3.
#
# Três jobs em paralelo, em Linux. O nome de cada job (backend, frontend, seguranca) é o nome da
# checagem que a regra da main exige (#58): renomear aqui quebra a regra.
name: CI

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read

# Push novo no mesmo PR cancela a execução anterior; na main, cada push roda até o fim.
concurrency:
  group: ci-${{ github.event.pull_request.number || github.ref }}
  cancel-in-progress: ${{ github.event_name == 'pull_request' }}

env:
  DOTNET_NOLOGO: "true"
  DOTNET_CLI_TELEMETRY_OPTOUT: "true"

jobs:
  backend:
    name: backend
    runs-on: ubuntu-latest
    timeout-minutes: 30
    steps:
      - name: Baixar o código
        uses: actions/checkout@v7
        with:
          persist-credentials: false

      - name: Instalar o .NET do global.json
        uses: actions/setup-dotnet@v6
        with:
          global-json-file: global.json

      - name: Cache dos pacotes NuGet
        uses: actions/cache@v6
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', 'Directory.Build.props', 'global.json') }}
          restore-keys: nuget-${{ runner.os }}-

      - name: Restaurar
        run: dotnet restore Tracbel.Crm.sln

      - name: Compilar (aviso é erro)
        run: dotnet build Tracbel.Crm.sln -c Release --no-restore -warnaserror

      - name: Modelo e migrations batem
        run: |
          dotnet tool restore
          dotnet tool run dotnet-ef migrations has-pending-model-changes \
            --project src/Tracbel.Crm.Infraestrutura/Tracbel.Crm.Infraestrutura.csproj \
            --startup-project src/Tracbel.Crm.Api/Tracbel.Crm.Api.csproj \
            --configuration Release --no-build

      # O SQL Server do CI é descartável: a senha é gerada aqui, mascarada no log e some com o
      # contêiner. Nenhum segredo fica cadastrado no GitHub.
      - name: Subir SQL Server 2022 descartável
        run: |
          senha="Ci-$(openssl rand -hex 16)-Aa1"
          echo "::add-mask::$senha"
          echo "SENHA_SQL_CI=$senha" >> "$GITHUB_ENV"
          docker run --detach --name sql-ci --publish 1433:1433 \
            --env ACCEPT_EULA=Y \
            --env MSSQL_PID=Developer \
            --env MSSQL_COLLATION=Latin1_General_CI_AI \
            --env "MSSQL_SA_PASSWORD=$senha" \
            mcr.microsoft.com/mssql/server:2022-latest
          for tentativa in $(seq 1 60); do
            if docker exec sql-ci /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$senha" -C -b -Q "SELECT 1" > /dev/null 2>&1; then
              echo "SQL Server respondendo depois de $((tentativa * 2)) s"
              exit 0
            fi
            sleep 2
          done
          echo "SQL Server não respondeu em 120 s"
          docker logs sql-ci | tail -n 50
          exit 1

      - name: Testes
        run: |
          export ConnectionStrings__Crm="Server=localhost,1433;Database=master;User Id=sa;Password=${SENHA_SQL_CI};TrustServerCertificate=True"
          dotnet test Tracbel.Crm.sln -c Release --no-build --logger trx --results-directory TestResults

      - name: Conferir resultados (nenhum teste pulado)
        if: always()
        shell: pwsh
        run: ./scripts/ci/conferir-testes.ps1 -Pasta TestResults

      - name: Anexar resultados dos testes
        if: always()
        uses: actions/upload-artifact@v7
        with:
          name: resultados-dos-testes
          path: TestResults/*.trx
          if-no-files-found: warn
          retention-days: 14

  frontend:
    name: frontend
    runs-on: ubuntu-latest
    timeout-minutes: 15
    defaults:
      run:
        working-directory: src/Tracbel.Crm.Web
    steps:
      - name: Baixar o código
        uses: actions/checkout@v7
        with:
          persist-credentials: false

      - name: Instalar o Node do .nvmrc
        uses: actions/setup-node@v7
        with:
          node-version-file: src/Tracbel.Crm.Web/.nvmrc
          cache: npm
          cache-dependency-path: src/Tracbel.Crm.Web/package-lock.json

      - name: Instalar dependências pelo lockfile
        run: npm ci

      - name: Lint
        run: npm run lint

      - name: Build
        run: npm run build

  seguranca:
    name: seguranca
    runs-on: ubuntu-latest
    timeout-minutes: 15
    steps:
      # Histórico completo: a varredura lê toda linha acrescentada em qualquer commit.
      - name: Baixar o código com todo o histórico
        uses: actions/checkout@v7
        with:
          fetch-depth: 0
          persist-credentials: false

      - name: Varredura de segredos (arquivos e histórico)
        shell: pwsh
        run: |
          ./scripts/seguranca/varrer-segredos.ps1 -Relatorio varredura-de-segredos.md
          $codigo = $LASTEXITCODE
          $resumo = Get-Content varredura-de-segredos.md | Where-Object { $_ -match '^(HEAD|Achados):' }
          @('## Varredura de segredos', '') + $resumo | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
          exit $codigo

      - name: Anexar o relatório da varredura
        if: always()
        uses: actions/upload-artifact@v7
        with:
          name: varredura-de-segredos
          path: varredura-de-segredos.md
          if-no-files-found: warn
          retention-days: 14

      # Vulnerabilidades: só relatório até a #59. Os dois passos nunca derrubam o job.
      - name: Instalar o .NET do global.json
        if: always()
        uses: actions/setup-dotnet@v6
        with:
          global-json-file: global.json

      - name: Pacotes .NET vulneráveis (relatório)
        if: always()
        continue-on-error: true
        run: |
          dotnet restore Tracbel.Crm.sln
          {
            echo "## Pacotes .NET vulneráveis (relatório — bloqueio só depois da #59)"
            echo '```text'
            dotnet list Tracbel.Crm.sln package --vulnerable --include-transitive
            echo '```'
          } >> "$GITHUB_STEP_SUMMARY"

      - name: Instalar o Node do .nvmrc
        if: always()
        uses: actions/setup-node@v7
        with:
          node-version-file: src/Tracbel.Crm.Web/.nvmrc

      - name: Pacotes npm vulneráveis (relatório)
        if: always()
        continue-on-error: true
        working-directory: src/Tracbel.Crm.Web
        run: |
          {
            echo "## Pacotes npm vulneráveis (relatório — bloqueio só depois da #59)"
            echo '```text'
            npm audit --package-lock-only --audit-level=high || true
            echo '```'
          } >> "$GITHUB_STEP_SUMMARY"
```

- [ ] **Passo 2: a varredura local não acusa o workflow**

Rodar: `./scripts/seguranca/varrer-segredos.ps1 -SemHistorico; "código: $LASTEXITCODE"`
Esperado: **código 0**. Os trechos `Password=${SENHA_SQL_CI}` e `senha="Ci-$(openssl ...)"` saem como
REFERENCIA (começam com `$` ou contêm `$(`).

- [ ] **Passo 3: commit, push e PR**

Corpo do PR, gravado fora do repositório (scratchpad) como `pr-ci-056.md`:

```markdown
## Resumo

CI em cada PR para a `main`, conforme `docs/projeto/47-CI-CD.md` §3 (aprovada em 17/09/2026).
Plano e resultado: `docs/projeto/47A-PLANO-DE-IMPLEMENTACAO-DO-CI.md`.

Closes #56

## O que muda

| Arquivo | O quê |
|---|---|
| `.github/workflows/ci.yml` | jobs `backend`, `frontend` e `seguranca` em Linux |
| `scripts/ci/conferir-testes.ps1` | confere os `.trx` resultado por resultado e barra teste pulado |
| `global.json`, `.config/dotnet-tools.json`, `src/Tracbel.Crm.Web/.nvmrc` | SDK 9.0.306, `dotnet-ef` 9.0.10 e Node 24 fixados |
| `README.md` | como ler o CI |

## Como verificar

- a aba **Checks** deste PR mostra `backend`, `frontend` e `seguranca` verdes;
- no resumo do `backend`: Total 514, Pulado 0;
- as provas de bloqueio (teste quebrado e segredo inventado) estão linkadas no plano 47A.

## Riscos e rollback

Baixo. Rollback: remover `.github/workflows/ci.yml`. Nenhum segredo cadastrado no GitHub; nada muda no
servidor nem no banco.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

```powershell
git add .github/workflows/ci.yml
git commit -m "ci: workflow com backend, frontend e seguranca em runners Linux"
$env:GH_TOKEN = (gh auth token --user Ricardo-Moretti)
git -c credential.helper= -c "credential.helper=!gh auth git-credential" push -u origin ci/056-workflow
gh pr create --repo tracbel/Visao_360 --base main --head ci/056-workflow --draft `
  --title "ci: build, testes com SQL Server, lint e varredura de segredos em cada PR" `
  --body-file <scratchpad>/pr-ci-056.md
```
Esperado: PR em rascunho aberto; a execução "CI" começa sozinha.

- [ ] **Passo 4: acompanhar a primeira execução**

Rodar:
```powershell
gh run list --repo tracbel/Visao_360 --branch ci/056-workflow --limit 1
gh run watch <id> --repo tracbel/Visao_360 --exit-status
```
Esperado: `backend`, `frontend` e `seguranca` verdes; no resumo de `backend`, Total 514 e Pulado 0.

- [ ] **Passo 5: se algum job falhar, achar a causa antes de mexer**

Rodar: `gh run view <id> --repo tracbel/Visao_360 --log-failed`
Seguir `superpowers:systematic-debugging`: ler o erro inteiro, reproduzir localmente quando possível,
corrigir a causa (caixa de nome de arquivo, caminho, fim de linha, tempo de subida do SQL Server) num
commit próprio com mensagem `fix(ci): …`, e repetir o passo 4. Não desligar teste, não afrouxar o
conferidor e não trocar `-warnaserror` para fazer passar.

- [ ] **Passo 6: três execuções verdes seguidas**

A primeira verde conta como 1. Rodar de novo mais duas vezes:
`gh run rerun <id> --repo tracbel/Visao_360` e `gh run watch <id> --exit-status`.
Esperado: três verdes seguidas; anotar a duração de cada job (`gh run view <id> --json jobs`).

---

### Tarefa 4: provar que o CI barra

**Arquivos:**
- Alterar temporariamente: `tests/Tracbel.Crm.Aplicacao.Testes/Carga/SincronizacaoDoArtTestes.cs:28`
- Alterar temporariamente: `.github/workflows/ci.yml` (um passo antes da varredura)

**Interfaces:**
- Consome: o workflow da tarefa 3.
- Produz: dois links de execução — uma vermelha com a prova e uma verde depois da reversão.

**Por que o segredo é gerado na execução, e não commitado:** um segredo inventado commitado ficaria no
histórico da branch, e a varredura — que lê o histórico — acusaria SUSPEITO em toda execução seguinte,
mesmo depois da reversão. Remover do histórico exigiria force push, que não está autorizado. Gerado na
execução, o arquivo existe só no runner. O workflow commitado contém apenas o gerador, que não casa com
nenhum padrão da varredura.

- [ ] **Passo 1: quebrar um teste de propósito**

Em `SincronizacaoDoArtTestes.cs`, linha 28, trocar o valor esperado:

```csharp
Sigilo.Mascarar("erro SQL 18456", ["sa", "1"]).Should().Be("erro SQL 18457 — PROVA DO CI, REVERTER");
```

- [ ] **Passo 2: gerar um segredo inventado na execução**

Em `ci.yml`, no job `seguranca`, logo antes do passo "Varredura de segredos", acrescentar:

```yaml
      - name: PROVA TEMPORÁRIA — arquivo com segredo inventado gerado na execução
        run: |
          printf 'se%s: "%s"\n' 'nha' "Prova$(openssl rand -hex 8)" > prova-segredo-inventado.txt
```

**Em bloco literal (`run: |`), obrigatoriamente.** A primeira versão deste plano punha o comando num valor YAML
simples, e o `: ` dentro dele quebra o arquivo ("mapping values are not allowed here"): o GitHub registrou a
execução 35242253846 sem nenhum job. Validar o YAML antes do push.

- [ ] **Passo 3: commit e push**

```powershell
git add tests/Tracbel.Crm.Aplicacao.Testes/Carga/SincronizacaoDoArtTestes.cs .github/workflows/ci.yml
git commit -m "test(ci): PROVA TEMPORÁRIA — teste quebrado e segredo inventado (reverter em seguida)"
git -c credential.helper= -c "credential.helper=!gh auth git-credential" push
```

- [ ] **Passo 4: conferir que ficou vermelho pelo motivo certo**

Rodar: `gh run watch <id> --repo tracbel/Visao_360` e `gh run view <id> --log-failed`
Esperado: `backend` vermelho com "1 teste(s) falharam" e
`Tracbel.Crm.Aplicacao.Testes.Carga.SincronizacaoDoArtTestes.Segredo_curto_demais_nao_apaga_a_mensagem_inteira`
como FALHOU; `seguranca` vermelho com 1 SUSPEITO em `prova-segredo-inventado.txt`; `frontend` verde.

- [ ] **Passo 5: reverter e conferir verde**

```powershell
git revert --no-edit HEAD
git -c credential.helper= -c "credential.helper=!gh auth git-credential" push
gh run watch <id> --repo tracbel/Visao_360 --exit-status
```
Esperado: os três jobs verdes de novo.

---

### Tarefa 5: documentar, fechar o PR e comentar a issue

**Arquivos:**
- Alterar: `README.md` (seção "Testes", depois do bloco de comandos)
- Alterar: este arquivo (seção "Resultado", no fim)
- Alterar: `docs/projeto/47-CI-CD.md` §3 — **só se o PR #81 já estiver na `main`**; se estiver,
  `git merge origin/main` na branch antes (merge, nunca rebase)

- [ ] **Passo 1: README**

Acrescentar logo depois do bloco de comandos da seção "Testes":

```markdown
### No CI

Todo PR para a `main` roda o workflow **CI** (`.github/workflows/ci.yml`), com três checagens:
`backend` (build com aviso como erro, migration pendente e os testes contra um SQL Server 2022 de
verdade), `frontend` (lint e build) e `seguranca` (varredura de segredos em arquivos e histórico).
**Teste pulado quebra o CI** — o resumo da execução mostra a tabela por projeto e o motivo de cada pulado.

Para conferir os mesmos resultados na sua máquina, sem SQL Server:

    dotnet test --logger trx --results-directory TestResults
    ./scripts/ci/conferir-testes.ps1 -Pasta TestResults -PermitirPulados

Desenho e decisões: `docs/projeto/47-CI-CD.md`.
```

- [ ] **Passo 2: resultado neste plano**

Preencher a seção "Resultado" abaixo com: links das três execuções verdes, duração por job, link da
execução vermelha da prova e da verde depois da reversão, e o que precisou ser corrigido no Linux.

- [ ] **Passo 3: doc 47 (condicional)**

Se o #81 já estiver na `main`: na §3, trocar "Estimativa (não medida): cerca de 5 minutos por PR" pelo
tempo medido e acrescentar os links das provas.

- [ ] **Passo 4: commit, push e PR pronto para revisão**

```powershell
git add README.md docs/projeto/47A-PLANO-DE-IMPLEMENTACAO-DO-CI.md
git commit -m "docs(ci): como ler o CI e o resultado medido da #56"
git -c credential.helper= -c "credential.helper=!gh auth git-credential" push
gh pr ready <número> --repo tracbel/Visao_360
```
Esperado: CI verde no último commit; PR fora do rascunho, com `Closes #56`, esperando o merge do Ricardo.

- [ ] **Passo 5: comentário detalhado na #56**

Com `gh issue comment 56 --body-file <arquivo>`: o que foi feito, as três execuções verdes, as duas provas
com links, os tempos, o que o Linux revelou e o que fica para a #58 (a regra da `main` só pode exigir
as checagens depois deste merge).

---

## Resultado (17/09/2026)

Todas as execuções em `https://github.com/tracbel/Visao_360/actions/runs/<id>`, no PR #82.

| Execução | Commit | Resultado | backend | seguranca | frontend | O que mostrou |
|---|---|---|---|---|---|---|
| 35240187111 | `85bd5b0` | vermelha | 183 s ✗ | 86 s ✓ | 25 s ✓ | o SQL Server do CI funcionou (os 9 testes de banco rodaram); **1 teste falhou só no Linux** |
| 35240816369, tentativa 1 | `d6df530` | **verde** | 187 s | 67 s | 29 s | 514 executados, 514 aprovados, 0 pulados; SQL Server respondendo em 2 s |
| 35240816369, tentativa 2 | `d6df530` | **verde** | 183 s | 80 s | 23 s | |
| 35240816369, tentativa 3 | `d6df530` | **verde** | 190 s | 79 s | 25 s | três verdes seguidas |
| 35242253846 | `d251dce` | não rodou | — | — | — | arquivo de workflow inválido — **erro deste plano** (ver abaixo) |
| 35242539860 | `d886928` | **vermelha — a prova** | 180 s ✗ | 82 s ✗ | 24 s ✓ | `backend`: 1 falha em `SincronizacaoDoArtTestes.Segredo_curto_demais_nao_apaga_a_mensagem_inteira`; `seguranca`: 1 SUSPEITO em `prova-segredo-inventado.txt:1` (`senha=«oculto:21»`) |
| 35242984773 | `c73baa1` | **verde** | 181 s | 81 s | 23 s | reversão da prova: 514/514/0/0; varredura com 109 achados e **0 SUSPEITO** — o histórico ficou limpo |

**Tempo.** Cada PR espera cerca de **3 min 10 s** (o `backend`; os três jobs correm em paralelo), abaixo da
estimativa de 5 min do doc 47. Somando os três jobs, uma execução gasta cerca de **4,8 minutos de runner**:
a cota de 3.000 minutos por mês do plano Team cobre por volta de 600 execuções.

**O que o Linux revelou (1 defeito, no teste).** `SolidTestes.As_referencias_de_projeto_apontam_sempre_para_dentro`
lia `..\Projeto\Projeto.csproj` com `Path.GetFileNameWithoutExtension`, que no Linux não separa na barra
invertida: toda referência de projeto virou "extra" (o FluentAssertions só mostrou a primeira). Corrigido em
`d6df530`, trocando `\` por `/` antes de extrair o nome — funciona nos dois sistemas.

**O que este plano errou (1 vez).** O passo 2 da tarefa 4 punha o gerador do segredo num valor YAML simples,
com `: ` dentro: "mapping values are not allowed here", linha 148, coluna 26. O GitHub registrou a execução
35242253846 sem jobs. Diagnóstico confirmado com PyYAML num ambiente isolado; corrigido em `d886928` com bloco
literal, e o passo 2 deste plano foi corrigido.

**Desvios do plano.** O próprio plano entrou como primeiro commit da branch (`cb000fe`), e não na tarefa 5; a
mensagem final do conferidor com `-PermitirPulados` passou a dizer quantos testes foram pulados, em vez de
"todos executados"; o tempo medido vai para o doc 47 dentro do PR #81, onde o documento mora.
