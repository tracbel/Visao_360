<#
.SYNOPSIS
    Roda a carga das vendas de máquina do ART no banco LOCAL de desenvolvimento (documento 35, seção 10).

.DESCRIPTION
    Monta, só na memória deste processo, as variáveis que a carga lê:

      ConnectionStrings__Crm   de infra/.env (DB_USUARIO, DB_SENHA_APLICACAO, DB_NOME, DB_PORTA)
      Art__*                   de .env (ART_DB_SERVER, ART_DB_PORT, ART_DB_DATABASE, ART_DB_USER,
                               ART_DB_PASSWORD, ART_VIEW)
      ProtheusBanco__*         de .env (TOTVS_DB_SERVER, TOTVS_DB_PORT, TOTVS_DB_DATABASE,
                               TOTVS_DB_USER, TOTVS_DB_PASSWORD) — leitura com ApplicationIntent=ReadOnly

    Nada é impresso além do destino (servidor e banco), e as variáveis são removidas ao final.

    O ART é lido em sessão READ ONLY; o Protheus, só com SELECT. A escrita acontece apenas no banco
    do CRM, e este script RECUSA qualquer servidor que não seja a máquina local.

.PARAMETER Simular
    Executa a carga inteira numa transação desfeita no fim: mostra os números sem mudar o banco.

.EXAMPLE
    ./scripts/integracao/rodar-carga-do-art.ps1 -Simular

.EXAMPLE
    ./scripts/integracao/rodar-carga-do-art.ps1
#>
[CmdletBinding()]
param([switch]$Simular)

$ErrorActionPreference = 'Stop'
$raiz = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

function Ler-Env([string]$Arquivo) {
    if (-not (Test-Path $Arquivo)) { throw "Não achei $Arquivo." }
    $valores = @{}
    foreach ($linha in Get-Content $Arquivo) {
        if ($linha -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') {
            $valores[$Matches[1]] = $Matches[2].Trim().Trim('"').Trim("'")
        }
    }
    $valores
}

$infra = Ler-Env (Join-Path $raiz 'infra/.env')
$fontes = Ler-Env (Join-Path $raiz '.env')

$porta = if ($infra['DB_PORTA']) { $infra['DB_PORTA'] } else { '1433' }
$banco = if ($infra['DB_NOME']) { $infra['DB_NOME'] } else { 'TracbelCrm' }
$usuario = if ($infra['DB_USUARIO']) { $infra['DB_USUARIO'] } else { 'TracbelCrm' }
if (-not $infra['DB_SENHA_APLICACAO']) { throw 'infra/.env não tem DB_SENHA_APLICACAO.' }

foreach ($chave in 'ART_DB_SERVER', 'ART_DB_DATABASE', 'ART_DB_USER', 'ART_DB_PASSWORD', 'ART_VIEW') {
    if (-not $fontes[$chave]) { throw ".env não tem $chave." }
}

$variaveis = [ordered]@{
    'ConnectionStrings__Crm'  = "Server=localhost,$porta;Database=$banco;User Id=$usuario;Password=$($infra['DB_SENHA_APLICACAO']);TrustServerCertificate=True"
    'Art__Servidor'           = $fontes['ART_DB_SERVER']
    'Art__Porta'              = $(if ($fontes['ART_DB_PORT']) { $fontes['ART_DB_PORT'] } else { '3306' })
    'Art__Banco'              = $fontes['ART_DB_DATABASE']
    'Art__Usuario'            = $fontes['ART_DB_USER']
    'Art__Senha'              = $fontes['ART_DB_PASSWORD']
    'Art__Visao'              = $fontes['ART_VIEW']
}

if ($fontes['TOTVS_DB_SERVER'] -and $fontes['TOTVS_DB_USER'] -and $fontes['TOTVS_DB_PASSWORD']) {
    $servidorDoProtheus = if ($fontes['TOTVS_DB_PORT']) { "$($fontes['TOTVS_DB_SERVER']),$($fontes['TOTVS_DB_PORT'])" } else { $fontes['TOTVS_DB_SERVER'] }
    $variaveis['ProtheusBanco__Servidor'] = $servidorDoProtheus
    $variaveis['ProtheusBanco__Banco'] = $fontes['TOTVS_DB_DATABASE']
    $variaveis['ProtheusBanco__Usuario'] = $fontes['TOTVS_DB_USER']
    $variaveis['ProtheusBanco__Senha'] = $fontes['TOTVS_DB_PASSWORD']
}

Write-Host "Destino da gravação: localhost,$porta / $banco (somente o banco local)." -ForegroundColor Cyan

$argumentos = @('run', '--project', (Join-Path $raiz 'src/Tracbel.Crm.Carga'), '-c', 'Release', '--', '--somente-art')
if ($Simular) { $argumentos += '--simular' }

try {
    foreach ($par in $variaveis.GetEnumerator()) { [Environment]::SetEnvironmentVariable($par.Key, $par.Value, 'Process') }
    & dotnet @argumentos
    $saida = $LASTEXITCODE
}
finally {
    foreach ($nome in $variaveis.Keys) { [Environment]::SetEnvironmentVariable($nome, $null, 'Process') }
}

exit $saida
