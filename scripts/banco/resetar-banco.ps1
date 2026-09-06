<#
.SYNOPSIS
    Apaga o SQL Server de desenvolvimento do CRM Tracbel — container E volume de dados — e,
    por padrão, sobe um banco novo em seguida.

.DESCRIPTION
    Operação destrutiva: todo dado gravado no volume local é perdido. É exatamente o que se
    espera de um banco de desenvolvimento descartável (docs/projeto/12-DECISAO-CONTAINERS.md,
    seção 5) — nunca aponte este script para infra/.env de um ambiente compartilhado, e NUNCA
    contra a instância de produção que hospeda o Vórtice.

    Pede confirmação antes de remover (ConfirmImpact = High). Use -Confirm:$false para pular a
    pergunta em automação, ou -WhatIf para ver o que aconteceria sem executar.

.PARAMETER SemSubirDeNovo
    Só remove; não chama subir-banco.ps1 em seguida.

.EXAMPLE
    ./scripts/banco/resetar-banco.ps1

.EXAMPLE
    ./scripts/banco/resetar-banco.ps1 -Confirm:$false
#>
[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param(
    [switch]$SemSubirDeNovo
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
if (-not (Test-Path $envFile)) { $envFile = 'infra/.env.exemplo' }

$alvo = "container e volume 'tracbel_crm_dados' do SQL Server de desenvolvimento"
if ($PSCmdlet.ShouldProcess($alvo, "remover (todo dado local é perdido)")) {
    Write-Host "Derrubando o banco e removendo o volume de dados..." -ForegroundColor Yellow
    # --profile administracao aqui garante que o Adminer também desça, se estiver de pé.
    Invoke-Externo docker @(
        'compose', '-f', $composeFile, '--env-file', $envFile,
        '--profile', 'administracao', 'down', '--volumes') -Falha "'docker compose down' falhou"
    Write-Host "Removido." -ForegroundColor Green

    if (-not $SemSubirDeNovo) {
        & (Join-Path $PSScriptRoot 'subir-banco.ps1')
    }
}
