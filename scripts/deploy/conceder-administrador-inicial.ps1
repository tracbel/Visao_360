<#
  conceder-administrador-inicial.ps1 - da o perfil Administrador a PRIMEIRA conta (issue 132).

      .\scripts\deploy\conceder-administrador-inicial.ps1 -NomePrincipal <e-mail da conta Microsoft>

  Pergunta a justificativa se ela nao vier em -Justificativa. Conta que ainda espera liberacao (nasceu
  no primeiro login) precisa de -Filial <codigo>, a filial de casa dela.

  =================================================================================================
  POR QUE EXISTE

  Conceder perfil e trabalho da tela de administracao, e a tela exige quem ja administra. No banco
  novo nao ha ninguem. Este script roda, NO SERVIDOR, o comando da carga que faz a primeira concessao
  pelo dominio: com justificativa, dentro da trilha de auditoria, em nome da propria conta. Ele recusa
  se ja existir administrador ativo - dai em diante, quem concede e quem administra, pela tela.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM

    1. roda o comando com --simular: tudo numa transacao desfeita, e a saida mostra o que aconteceria;
    2. mostra essa saida e pede CONCEDER;
    3. roda de verdade e mostra o resultado.

  Nada de senha: o comando roda como SYSTEM no servidor, com a mesma conexao integrada das rotinas.
  O e-mail e a justificativa viajam em Base64, e nao dentro do texto do script remoto.

  ASCII PURO, sem acento: o script remoto e lido pelo PowerShell 5.1 do servidor.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $NomePrincipal,
    [string] $Justificativa = '',
    [string] $Filial = '',
    [string] $Servidor = '10.150.4.249',
    [string] $DestinoDaCarga = 'C:\aplicacoes\tracbel-crm-carga',
    [string] $Conexao = 'Server=localhost,1433;Database=TracbelCrm;Integrated Security=True;TrustServerCertificate=True'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

if ($NomePrincipal -notmatch '^[^@\s"]+@[^@\s"]+$') { throw "'$NomePrincipal' nao parece o e-mail de uma conta Microsoft." }

if (-not $Justificativa) {
    $Justificativa = Read-Host '   Justificativa (por que esta conta e a primeira administradora, e por autorizacao de quem)'
}
if (-not $Justificativa.Trim()) { throw 'A justificativa e obrigatoria. Nada foi feito.' }

# ASPAS DUPLAS NAO PASSAM inteiras para um executavel no PowerShell 5.1: seriam cortadas no caminho.
if ($Justificativa.Contains('"')) { throw 'Escreva a justificativa sem aspas duplas. Nada foi feito.' }

$exe = Join-Path $DestinoDaCarga 'Tracbel.Crm.Carga.exe'
$exePorSmb = "\\$Servidor\$($exe -replace ':', '$')"
if (-not (Test-Path $exePorSmb)) { throw "Nao achei a carga publicada em $Servidor ($exe). A publicacao ja rodou?" }

function EmBase64([string] $texto) { [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($texto)) }

function RodarNoServidor([bool] $simular) {
    $marca = if ($simular) { '$argumentos += ''--simular''' } else { '' }
    $filialB64 = if ($Filial) { EmBase64 $Filial.Trim() } else { '' }

    return Invoke-NoServidor -Nome 'crm-administrador-inicial' -TimeoutSegundos 300 -Script @"
try { [Console]::OutputEncoding = [Text.Encoding]::UTF8 } catch { }
function Texto(`$b64) { [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(`$b64)) }
`$env:ConnectionStrings__Crm = '$Conexao'
`$argumentos = @('--conceder-administrador-inicial', (Texto '$(EmBase64 $NomePrincipal.Trim())'), '--justificativa', (Texto '$(EmBase64 $Justificativa.Trim())'))
if ('$filialB64') { `$argumentos += @('--filial', (Texto '$filialB64')) }
$marca
& '$exe' @argumentos 2>&1 | ForEach-Object { [string] `$_ }
Write-Output ('codigo de saida: ' + `$LASTEXITCODE)
"@
}

function CodigoDe([string] $saida) {
    $m = [regex]::Match($saida, 'codigo de saida: (\d+)')
    if ($m.Success) { [int] $m.Groups[1].Value } else { -1 }
}

# -------------------------------------------------------------------------------------------------
Write-Host ''
Write-Host '== 1. Simulando (nada e gravado)' -ForegroundColor Cyan
# -------------------------------------------------------------------------------------------------
$simulacao = RodarNoServidor $true
Write-Host $simulacao

# A CARGA PUBLICADA AINDA NAO TEM O COMANDO: sem ele, a linha de comando cai na trava do Vortice.
if ($simulacao -match 'CONGELADA') {
    Write-Host ''
    Write-Host '   A carga publicada no servidor ainda nao tem este comando. Espere o agente publicar a main' -ForegroundColor Yellow
    Write-Host '   (.\scripts\deploy\verificar-publicacao.ps1) e rode de novo. Nada foi feito.' -ForegroundColor Yellow
    return
}

$codigo = CodigoDe $simulacao
if ($codigo -ne 0) {
    Write-Host ''
    Write-Host "   A simulacao recusou (codigo $codigo). O motivo esta acima. Nada foi feito." -ForegroundColor Yellow
    return
}

if ($simulacao -match 'administrador\. Nada foi feito') {
    Write-Host ''
    Write-Host '   Esta conta ja e administradora. Nada a fazer.' -ForegroundColor Green
    return
}

# -------------------------------------------------------------------------------------------------
Write-Host ''
Write-Host '== 2. Confirmacao' -ForegroundColor Cyan
# -------------------------------------------------------------------------------------------------
$resposta = Read-Host "   Conceder o perfil Administrador a $NomePrincipal de verdade? (digite CONCEDER)"
if ($resposta -ne 'CONCEDER') {
    Write-Host '   Nada foi concedido.' -ForegroundColor Yellow
    return
}

# -------------------------------------------------------------------------------------------------
Write-Host ''
Write-Host '== 3. Concedendo' -ForegroundColor Cyan
# -------------------------------------------------------------------------------------------------
$saida = RodarNoServidor $false
Write-Host $saida

if ((CodigoDe $saida) -eq 0) {
    Write-Host ''
    Write-Host '   Feito. Saia e entre no CRM de novo para a conta carregar o perfil novo.' -ForegroundColor Green
} else {
    Write-Host ''
    Write-Host '   O comando nao concluiu. O motivo esta acima; nada foi gravado pela metade.' -ForegroundColor Yellow
}
