<#
  instalar-agente-de-publicacao.ps1 - instala, UMA VEZ, o agente que publica sozinho (issue 62).

      Set-Location C:\projetos\tracbel-crm
      powershell -ExecutionPolicy Bypass -File .\scripts\deploy\instalar-agente-de-publicacao.ps1

  Depois disso, toda vez que o CI fechar verde na `main`, o servidor se atualiza sozinho em ate
  cinco minutos. Ninguem roda mais nada — salvo quando a migracao for destrutiva, que e o unico caso
  que continua passando por uma pessoa (`autorizar-publicacao.ps1`).

  =================================================================================================
  O TOKEN

  O repositorio e privado, entao baixar o pacote exige um token. Ele deve ser um FINE-GRAINED TOKEN
  do GitHub, com:

    - acesso a ESTE repositorio apenas (nao "all repositories");
    - permissoes de LEITURA apenas: "Contents: Read-only" e "Actions: Read-only";
    - validade com data — e o lembrete de que ele existe.

  Este script pede o token na tela, como SecureString: ele nao aparece no historico do PowerShell,
  nao entra no repositorio e nao e impresso em log nenhum. No servidor, o arquivo fica com heranca
  removida e acesso so para o SYSTEM e os administradores.

  QUANDO O TOKEN VENCER, o agente para de publicar e o `verificar-publicacao.ps1` mostra o erro.
  Rode este script de novo para trocar.

  =================================================================================================
  O QUE ELE INSTALA

    C:\aplicacoes\tracbel-crm-agente\
        agente-de-publicacao.ps1     o agente
        publicar-pacote.ps1          a troca de versao, com volta atras
        configuracao.json            repositorio, caminhos, conexao (integrada) — sem segredo
        token.txt                    so o SYSTEM e os administradores leem
        estado.json                  o que o agente sabe de si (criado na primeira rodada)
        logs\                        um arquivo por rodada

    Tarefa agendada `TracbelCrmPublicacao`, a cada 5 minutos, como SYSTEM.
    Fonte `TracbelCrmPublicacao` no log de eventos do Windows.
#>

[CmdletBinding()]
param(
    [string] $Servidor = '10.150.4.249',
    [string] $Banco = 'TracbelCrm',
    [string] $Repositorio = 'tracbel/Visao_360',
    [string] $Workflow = 'ci.yml',
    [string] $Artefato = 'pacote-de-publicacao',
    [string] $PastaDoAgente = 'C:\aplicacoes\tracbel-crm-agente',
    [string] $DestinoDaApi = 'C:\aplicacoes\tracbel-crm',
    [string] $DestinoDaCarga = 'C:\aplicacoes\tracbel-crm-carga',
    [string] $ServicoDaApi = 'TracbelCrmApi',
    [string] $ServicoDaSincronizacao = 'TracbelCrmSincronizacaoArt',
    [string] $NomeDns = '360-TracbelAgro.tracbel.com.br',
    [int]    $PortaApi = 5443,
    [int]    $IntervaloEmMinutos = 5,
    [string] $NomeTarefa = 'TracbelCrmPublicacao',
    [switch] $SoAtualizarOToken
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t) { Write-Host "   OK  $t" -ForegroundColor Green }

if ($PastaDoAgente -match '\s' -or $DestinoDaApi -match '\s') {
    throw 'As pastas nao podem ter espaco: o /TR do schtasks guarda uma linha de comando.'
}

$porSmb = "\\$Servidor\$($PastaDoAgente -replace ':', '$')"

# -------------------------------------------------------------------------------------------------
Passo '0. O token'
# -------------------------------------------------------------------------------------------------
Write-Host '   Crie em: https://github.com/settings/personal-access-tokens/new'
Write-Host '   - Repository access: Only select repositories -> ' -NoNewline; Write-Host $Repositorio -ForegroundColor Yellow
Write-Host '   - Permissions: Contents = Read-only, Actions = Read-only'
Write-Host ''

$segredo = Read-Host '   Cole o token (ele nao aparece na tela nem no historico)' -AsSecureString
$token = [System.Net.NetworkCredential]::new('', $segredo).Password

if (-not $token) { throw 'Token vazio. Nada foi instalado.' }
if ($token.Length -lt 20) { throw 'Esse token parece curto demais. Nada foi instalado.' }

# CONFERIR ANTES DE INSTALAR. Descobrir que o token nao serve depois de agendar a tarefa faria o
# agente falhar em silencio a cada cinco minutos.
Passo '1. Conferindo o token contra o GitHub'
$cabecalhos = @{
    Authorization          = "Bearer $token"
    Accept                 = 'application/vnd.github+json'
    'X-GitHub-Api-Version' = '2022-11-28'
    'User-Agent'           = 'tracbel-crm-agente'
}

try {
    $repo = Invoke-RestMethod "https://api.github.com/repos/$Repositorio" -Headers $cabecalhos -TimeoutSec 60
    Ok "le o repositorio $($repo.full_name) (privado: $($repo.private))"
} catch {
    throw "O token nao consegue ler $Repositorio. Confira o escopo e a validade. Nada foi instalado. ($($_.Exception.Message))"
}

try {
    $runs = Invoke-RestMethod "https://api.github.com/repos/$Repositorio/actions/workflows/$Workflow/runs?per_page=1" -Headers $cabecalhos -TimeoutSec 60
    Ok "le as execucoes do $Workflow ($($runs.total_count) ate agora)"
} catch {
    throw "O token le o repositorio mas nao as Actions. Falta a permissao 'Actions: Read-only'. Nada foi instalado."
}

# -------------------------------------------------------------------------------------------------
Passo '2. Gravar o token no servidor'
# -------------------------------------------------------------------------------------------------
New-Item -ItemType Directory -Force -Path $porSmb | Out-Null
Set-Content -LiteralPath (Join-Path $porSmb 'token.txt') -Value $token -Encoding ASCII -NoNewline
$token = $null

# POR SID, E NAO POR NOME: num Windows em portugues 'SYSTEM' se chama 'SISTEMA', e o icacls com o
# nome em ingles falha DEPOIS de tirar a heranca — deixando o arquivo sem dono nenhum.
$saida = Invoke-NoServidor -Nome 'crm-agente-token' -Script @"
`$a = '$PastaDoAgente\token.txt'
icacls `$a /inheritance:r /grant '*S-1-5-18:(R)' '*S-1-5-32-544:(R)'
Write-Output ('codigo do icacls: ' + `$LASTEXITCODE)
"@
Write-Host $saida
if ($saida -notmatch 'codigo do icacls: 0') { throw "Nao consegui restringir o acesso ao token. A saida esta acima." }
Ok 'token gravado, legivel so pelo SYSTEM e pelos administradores'

if ($SoAtualizarOToken) {
    Ok 'so o token foi trocado, a pedido (-SoAtualizarOToken)'
    return
}

# -------------------------------------------------------------------------------------------------
Passo '3. Copiar o agente e a configuracao'
# -------------------------------------------------------------------------------------------------
Copy-Item (Join-Path $PSScriptRoot 'agente-de-publicacao.ps1') $porSmb -Force
Copy-Item (Join-Path $PSScriptRoot 'publicar-pacote.ps1') $porSmb -Force

# A CONEXAO E INTEGRADA: a tarefa roda como SYSTEM, que no dominio e a conta da MAQUINA, e o banco
# esta na mesma maquina. Nenhuma senha em arquivo.
$configuracao = [ordered]@{
    repositorio            = $Repositorio
    workflow               = $Workflow
    artefato               = $Artefato
    caminhoDoToken         = "$PastaDoAgente\token.txt"
    banco                  = $Banco
    conexaoDoBanco         = "Server=localhost,1433;Database=$Banco;Integrated Security=True;TrustServerCertificate=True"
    destinoDaApi           = $DestinoDaApi
    destinoDaCarga         = $DestinoDaCarga
    servicoDaApi           = $ServicoDaApi
    servicoDaSincronizacao = $ServicoDaSincronizacao
    provaDeVida            = "https://${NomeDns}:$PortaApi/saude/banco"
}

$configuracao | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $porSmb 'configuracao.json') -Encoding utf8
Ok "agente e configuracao em $PastaDoAgente"

# -------------------------------------------------------------------------------------------------
Passo '4. Fonte no log de eventos'
# -------------------------------------------------------------------------------------------------
$saida = Invoke-NoServidor -Nome 'crm-agente-evento' -Script @"
if (-not [System.Diagnostics.EventLog]::SourceExists('$NomeTarefa')) {
    New-EventLog -LogName Application -Source '$NomeTarefa'
    Write-Output 'fonte criada'
} else { Write-Output 'fonte ja existia' }
"@
Write-Host $saida
Ok "avisos do agente vao para o log de eventos, fonte $NomeTarefa"

# -------------------------------------------------------------------------------------------------
Passo "5. Tarefa $NomeTarefa, a cada $IntervaloEmMinutos minutos"
# -------------------------------------------------------------------------------------------------
# O /SC MINUTE aceita /MO em minutos, e a tarefa repete indefinidamente. Os argumentos vao numa
# LISTA porque uma linha de comando escrita a mao ja falhou aqui antes (a tarefa da PAM, em 20/09):
# continuacao de linha nao sobrevive a viagem, e as aspas somem de nivel em nivel.
$saida = Invoke-NoServidor -Nome 'crm-agente-tarefa' -TimeoutSegundos 300 -Script @"
`$argumentos = @(
    '/Create', '/TN', '$NomeTarefa',
    '/TR', 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File $PastaDoAgente\agente-de-publicacao.ps1',
    '/SC', 'MINUTE', '/MO', '$IntervaloEmMinutos',
    '/RU', 'SYSTEM', '/RL', 'HIGHEST', '/F')
& schtasks.exe @argumentos
Write-Output ('codigo do schtasks: ' + `$LASTEXITCODE)
& schtasks.exe /Query /TN '$NomeTarefa' /FO LIST /V
"@
Write-Host $saida
if ($saida -notmatch 'codigo do schtasks: 0') { throw "Nao consegui registrar a tarefa $NomeTarefa. A saida esta acima." }
Ok "tarefa registrada, a cada $IntervaloEmMinutos minutos, como SYSTEM"

# -------------------------------------------------------------------------------------------------
Passo '6. Primeira passada em SIMULACAO'
# -------------------------------------------------------------------------------------------------
# NADA E PUBLICADO AQUI. A simulacao prova que o agente acha o commit aprovado, baixa o pacote, le o
# manifesto e sabe dizer o que falta no banco — antes de deixa-lo publicar de verdade.
$saida = Invoke-NoServidor -Nome 'crm-agente-simular' -TimeoutSegundos 1800 -Script @"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File '$PastaDoAgente\agente-de-publicacao.ps1' -Simular
Write-Output ('codigo do agente: ' + `$LASTEXITCODE)
"@
Write-Host $saida

Write-Host ''
if ($saida -match 'codigo do agente: 0') {
    Ok 'o agente esta de pe: a partir de agora, CI verde na main vira publicacao sozinha'
    Write-Host ''
    Write-Host '   Para acompanhar:  .\scripts\deploy\verificar-publicacao.ps1 -Log'
    Write-Host '   Se parar por migracao destrutiva, ele diz o comando para autorizar.'
} else {
    Write-Host '   !   a simulacao nao terminou bem. A tarefa esta agendada, mas confira a saida acima.' -ForegroundColor Yellow
}
Write-Host ''
