<#
.SYNOPSIS
    Sobe o SQL Server de desenvolvimento do CRM Tracbel: cria o infra/.env se faltar, sobe o
    container, espera ficar saudável, CRIA O BANCO com a colação decidida, cria o login da
    aplicação, aplica as migrations do EF Core e roda o seed — quando o seed existir no
    repositório.

.DESCRIPTION
    Passo a passo completo, do zero ao banco pronto: docs/banco/README.md.
    Regras que este script obedece: docs/projeto/14-PADRAO-DE-BANCO.md, seções 9 (migrations
    só por EF Core), 9.5 (colação nasce de script versionado, nunca de configuração à mão no
    servidor) e 8 (seed versionado). A decisão de banco está em
    docs/projeto/20-DECISAO-SQL-SERVER.md.

    A connection string é MONTADA a partir de infra/.env e exportada em
    ConnectionStrings__Crm, que tem precedência sobre appsettings.json. É por isso que trocar
    a porta em infra/.env basta: nada precisa mudar em arquivo versionado.

.PARAMETER PularMigrations
    Não tenta aplicar migrations, mesmo que existam.

.PARAMETER PularSeed
    Não tenta rodar o seed, mesmo que exista.

.PARAMETER ComAdministracao
    Sobe também o Adminer (profile 'administracao' do compose).

.EXAMPLE
    ./scripts/banco/subir-banco.ps1

.EXAMPLE
    ./scripts/banco/subir-banco.ps1 -ComAdministracao
#>
[CmdletBinding()]
param(
    [switch]$PularMigrations,
    [switch]$PularSeed,
    [switch]$ComAdministracao
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------------------------
# Executa um programa externo e falha SÓ pelo código de saída.
#
# Por que isto existe: `docker compose` escreve o progresso ("Container ... Starting") em
# stderr, e o Windows PowerShell 5.1 com $ErrorActionPreference = 'Stop' transforma qualquer
# linha de stderr de programa externo em erro terminante — o script morreria numa mensagem de
# progresso, sem nada ter dado errado. Aqui o critério é o único que significa alguma coisa:
# o código de saída.
# ---------------------------------------------------------------------------------------------
function Invoke-Externo {
    param(
        [Parameter(Mandatory, Position = 0)][string]$Programa,
        # SEM ValueFromRemainingArguments de propósito: no Windows PowerShell 5.1 ele achata um
        # array passado por posição numa ÚNICA string, e o programa externo recebe um argumento
        # gigante em vez de vários. O array entra como parâmetro normal e é espalhado abaixo.
        [Parameter(Position = 1)][string[]]$Argumentos = @(),
        [string]$Falha = 'O comando externo falhou'
    )

    $anterior = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        & $Programa @Argumentos
    }
    finally {
        $ErrorActionPreference = $anterior
    }

    if ($LASTEXITCODE -ne 0) { throw "$Falha (exit $LASTEXITCODE)." }
}

$raiz = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $raiz

$composeFile = 'infra/docker-compose.yml'
$envFile = 'infra/.env'
$envExemplo = 'infra/.env.exemplo'

if (-not (Test-Path $envFile)) {
    Write-Host "infra/.env não existe — copiando de $envExemplo." -ForegroundColor Yellow
    Write-Host "AJUSTE AS SENHAS em infra/.env antes de usar fora da sua máquina de desenvolvimento." -ForegroundColor Yellow
    Copy-Item $envExemplo $envFile
}

# ---------------------------------------------------------------------------------------------
# Lê infra/.env. É o mesmo arquivo que o compose lê, e é a ÚNICA fonte de senha e porta.
# ---------------------------------------------------------------------------------------------
$config = @{}
foreach ($linha in Get-Content $envFile) {
    $texto = $linha.Trim()
    if ($texto -eq '' -or $texto.StartsWith('#')) { continue }
    $partes = $texto.Split('=', 2)
    if ($partes.Count -eq 2) { $config[$partes[0].Trim()] = $partes[1].Trim() }
}

function Get-Config { param([string]$Chave, [string]$Padrao)
    if ($config.ContainsKey($Chave) -and -not [string]::IsNullOrWhiteSpace($config[$Chave])) { $config[$Chave] } else { $Padrao } }

$usuario  = Get-Config 'DB_USUARIO' 'TracbelCrm'
$banco    = Get-Config 'DB_NOME'    'TracbelCrm'
$porta    = Get-Config 'DB_PORTA'   '1433'
$colacao  = Get-Config 'DB_COLACAO' 'Latin1_General_CI_AI'
$senhaSa  = Get-Config 'DB_SENHA'   $null
$senhaApp = Get-Config 'DB_SENHA_APLICACAO' $null

if ([string]::IsNullOrWhiteSpace($senhaSa)) {
    throw "DB_SENHA não está definida em $envFile. Copie de $envExemplo e ajuste."
}
if ([string]::IsNullOrWhiteSpace($senhaApp)) {
    throw "DB_SENHA_APLICACAO não está definida em $envFile. Copie de $envExemplo e ajuste."
}

# O nome do banco e o do login entram em SQL dinâmico mais abaixo. Aqui eles ficam presos ao
# formato de identificador do projeto — o que fecha a porta para injeção pelo arquivo .env.
foreach ($par in @(@{ N = 'DB_NOME'; V = $banco }, @{ N = 'DB_USUARIO'; V = $usuario })) {
    if ($par.V -notmatch '^[A-Za-z][A-Za-z0-9]*$') {
        throw "$($par.N) precisa ser PascalCase sem acento, espaço ou underscore. Valor: '$($par.V)'."
    }
}
if ($colacao -notmatch '^[A-Za-z0-9_]+$') { throw "DB_COLACAO inválida: '$colacao'." }

Write-Host "Subindo o SQL Server 2022 de desenvolvimento ($composeFile)..." -ForegroundColor Cyan
$argumentosCompose = @('compose', '-f', $composeFile, '--env-file', $envFile)
if ($ComAdministracao) { $argumentosCompose += @('--profile', 'administracao') }
Invoke-Externo docker ($argumentosCompose + @('up', '-d')) -Falha "'docker compose up' falhou"

Write-Host "Esperando o banco ficar saudável..." -ForegroundColor Cyan
$anteriorPs = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
$containerId = (& docker @argumentosCompose ps -q db | Select-Object -First 1)
$ErrorActionPreference = $anteriorPs
if ($null -ne $containerId) { $containerId = $containerId.Trim() }
if ([string]::IsNullOrWhiteSpace($containerId)) {
    throw "Não encontrei o container do serviço 'db'. Rode 'docker compose -f $composeFile logs' para investigar."
}

$maximoTentativas = 40
$status = ''
for ($tentativa = 1; $tentativa -le $maximoTentativas; $tentativa++) {
    $status = (docker inspect --format='{{.State.Health.Status}}' $containerId).Trim()
    Write-Host "  [$tentativa/$maximoTentativas] status: $status"
    if ($status -eq 'healthy') { break }
    Start-Sleep -Seconds 5
}

if ($status -ne 'healthy') {
    throw "O banco não ficou saudável a tempo (status final: '$status'). Veja 'docker compose -f $composeFile logs db'."
}
Write-Host "Servidor saudável." -ForegroundColor Green

# ---------------------------------------------------------------------------------------------
# Descobre onde está o sqlcmd dentro do container. O caminho mudou entre versões da imagem.
# ---------------------------------------------------------------------------------------------
$ErrorActionPreference = 'Continue'
$sqlcmd = $null
foreach ($caminho in @('/opt/mssql-tools18/bin/sqlcmd', '/opt/mssql-tools/bin/sqlcmd')) {
    & docker exec $containerId test -x $caminho 2>$null
    if ($LASTEXITCODE -eq 0) { $sqlcmd = $caminho; break }
}
$ErrorActionPreference = 'Stop'
if ($null -eq $sqlcmd) { throw "Não encontrei o sqlcmd dentro do container." }

function Invoke-Sql {
    param([Parameter(Mandatory)][string]$Comando, [string]$Banco = 'master')
    Invoke-Externo docker @(
        'exec', $containerId, $sqlcmd,
        '-S', 'localhost', '-U', 'sa', '-P', $senhaSa, '-C', '-b', '-d', $Banco,
        '-Q', $Comando) -Falha 'O comando SQL falhou'
}

# ---------------------------------------------------------------------------------------------
# CRIAÇÃO DO BANCO — documento 14, regra 9.5: colação e configuração física nascem num script
# versionado, nunca de uma configuração feita à mão no servidor.
#
# O que fica registrado aqui, e por quê:
#   - COLLATE explícito: CI (ignora caixa) e AI (ignora acento). [V] o Vórtice usa
#     SQL_Latin1_General_CP1_CI_AS, accent-SENSITIVE — buscar "Jose" não encontra "José".
#   - AUTO_SHRINK OFF: [V] achado 9 da extração — auto-shrink LIGADO em produção e homologação,
#     num banco de 36 GB, causando fragmentação contínua de índice.
#   - RECOVERY SIMPLE e crescimento em MB FIXO, só em DESENVOLVIMENTO: [V] o arquivo do Vórtice
#     cresce de 10 em 10 por cento. Em produção o recovery é FULL — ver documento 20, seção 6.
# ---------------------------------------------------------------------------------------------
Write-Host "Criando o banco '$banco' (colação $colacao), se ainda não existir..." -ForegroundColor Cyan
Invoke-Sql @"
IF DB_ID(N'$banco') IS NULL
BEGIN
    CREATE DATABASE [$banco] COLLATE $colacao;
END;
ALTER DATABASE [$banco] SET AUTO_SHRINK OFF;
ALTER DATABASE [$banco] SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE [$banco] SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE [$banco] SET RECOVERY SIMPLE;
ALTER DATABASE [$banco] MODIFY FILE (NAME = N'$banco', FILEGROWTH = 256MB);
ALTER DATABASE [$banco] MODIFY FILE (NAME = N'${banco}_log', FILEGROWTH = 128MB);
"@

Write-Host "Garantindo o login e o usuário da aplicação ('$usuario')..." -ForegroundColor Cyan
Invoke-Sql @"
IF SUSER_ID(N'$usuario') IS NULL
    CREATE LOGIN [$usuario] WITH PASSWORD = N'$senhaApp', CHECK_POLICY = OFF, DEFAULT_DATABASE = [$banco];

-- dbcreator SÓ NO CONTAINER DE DESENVOLVIMENTO, e por um motivo específico: os testes de
-- categoria 'BancoReal' (tests/.../Banco/MigracaoNoContainerTestes.cs) criam e destroem um
-- banco PRÓPRIO a cada execução, para nunca custar o dado com que alguém está trabalhando.
-- Em homologação e produção o login da aplicação NÃO tem esta função: lá o banco é criado
-- uma vez pelo runbook da infra (documento 20, seção 6), e a aplicação só o usa.
ALTER SERVER ROLE dbcreator ADD MEMBER [$usuario];
"@
Invoke-Sql -Banco $banco @"
IF DATABASE_PRINCIPAL_ID(N'$usuario') IS NULL
    CREATE USER [$usuario] FOR LOGIN [$usuario];
ALTER ROLE db_owner ADD MEMBER [$usuario];
"@

# A connection string tem precedência sobre appsettings.json — é assim que a porta e a senha
# de infra/.env chegam ao 'dotnet ef' sem que nada versionado precise mudar.
$env:ConnectionStrings__Crm =
    "Server=localhost,$porta;Database=$banco;User Id=$usuario;Password=$senhaApp;TrustServerCertificate=True"

if ($PularMigrations) {
    Write-Host "Migrations puladas (-PularMigrations)." -ForegroundColor Yellow
}
else {
    $pastaMigrations = 'src/Tracbel.Crm.Infraestrutura/Migrations'
    if (Test-Path $pastaMigrations) {
        Write-Host "Aplicando migrations do EF Core..." -ForegroundColor Cyan
        $argumentosEf = @(
            'ef', 'database', 'update',
            '--project', 'src/Tracbel.Crm.Infraestrutura/Tracbel.Crm.Infraestrutura.csproj',
            '--startup-project', 'src/Tracbel.Crm.Api/Tracbel.Crm.Api.csproj'
        )
        Invoke-Externo dotnet $argumentosEf -Falha "'dotnet ef database update' falhou"
    }
    else {
        Write-Host ("Nenhuma migration em '$pastaMigrations' ainda — pulando. Rode " +
            "'dotnet ef migrations add <Nome>' (ver docs/projeto/14-PADRAO-DE-BANCO.md, seção 9).") -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------------------------
# GANCHO DE SEED — deixado pronto de propósito, sem conteúdo.
#
# O dado semente (permissões, tipos de processo, fases, catálogos, linhas de negócio) é de
# outra frente. Quando ela entregar, basta existir 'scripts/banco/rodar-seed.ps1' e este passo
# passa a rodar sozinho, sem tocar neste arquivo.
#
# O que aquele script precisa obedecer (documento 14, seção 8):
#   8.1 catálogo fechado nasce de seed versionado no Git, nunca de INSERT manual em ambiente;
#   8.2 o seed é IDEMPOTENTE — rodar duas vezes não duplica linha nem lança exceção;
#   8.3 o dado de referência é identificado pelo CÓDIGO estável, nunca pelo Id interno.
# ---------------------------------------------------------------------------------------------
if ($PularSeed) {
    Write-Host "Seed pulado (-PularSeed)." -ForegroundColor Yellow
}
else {
    $scriptSeed = 'scripts/banco/rodar-seed.ps1'
    if (Test-Path $scriptSeed) {
        Write-Host "Rodando o seed..." -ForegroundColor Cyan
        & $scriptSeed
        if ($LASTEXITCODE -ne 0) { throw "O seed falhou (exit $LASTEXITCODE)." }
    }
    else {
        Write-Host ("Nenhum script de seed em '$scriptSeed' ainda — pulando. O gancho está " +
            "pronto: quando o arquivo existir, este passo roda sozinho (documento 14, seção 8).") -ForegroundColor Yellow
    }
}

# ---------------------------------------------------------------------------------------------
# Prova de que o banco existe. Sai a contagem por schema, direto do catálogo do SQL Server.
#
# Conta só a tabela LÓGICA do modelo: a tabela particionada continua sendo UMA linha em
# sys.tables (a partição é física, não é conceito de negócio), e a tabela de controle de
# migração do EF Core fica de fora, porque não é do modelo.
# O total tem que ser 63 — as 63 do documento 17, seção 8.
# ---------------------------------------------------------------------------------------------
Write-Host ""
Write-Host "Conferindo o que existe no banco:" -ForegroundColor Cyan
Invoke-Sql -Banco $banco @"
SELECT ISNULL(s.name, '(TOTAL)') AS [Schema], COUNT(*) AS Tabelas
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name <> '__EFMigrationsHistory'
  AND s.name IN ('organizacao','seguranca','comercial','processo','frota',
                 'documento','auditoria','integracao','metadado','relatorio')
GROUP BY ROLLUP (s.name)
ORDER BY CASE WHEN s.name IS NULL THEN 1 ELSE 0 END, s.name;
"@

Write-Host ""
Write-Host "Pronto. Connection string local (senha e porta saem de infra/.env):" -ForegroundColor Green
Write-Host "  Server=localhost,$porta;Database=$banco;User Id=$usuario;Password=<DB_SENHA_APLICACAO>;TrustServerCertificate=True"
if ($ComAdministracao) {
    $portaAdminer = Get-Config 'ADMINER_PORTA' '8081'
    Write-Host "  Adminer: http://localhost:$portaAdminer  (sistema: MS SQL, servidor: db, usuário: $usuario)"
}
