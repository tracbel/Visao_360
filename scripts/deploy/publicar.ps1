<#
  publicar.ps1 - atualiza o CRM no servidor A PARTIR DESTA ESTACAO. Ninguem entra no servidor.

      .\scripts\deploy\publicar.ps1
      .\scripts\deploy\publicar.ps1 -PularTestes     # so quando os testes acabaram de rodar

  =================================================================================================
  O MESMO MODELO DO USER-ONBOARDING, que ja roda neste servidor (user-onboarding\deploy.ps1):

    1. testa aqui;
    2. gera o pacote aqui (API self-contained + front);
    3. para o servico remotamente, com sc.exe;
    4. copia pelo compartilhamento administrativo C$;
    5. sobe o servico - e a PROPRIA API aplica as migracoes do banco ao subir (Program.cs);
    6. confere a prova de vida pelo nome DNS, com o certificado de verdade;
    7. confere por hash, arquivo a arquivo, que o servidor ficou identico ao pacote.

  O deploy so diz "pronto" quando os passos 6 e 7 passam. Data de arquivo copiado nao prova conteudo.

  POR QUE SMB E sc.exe, E NAO WinRM: o servidor nao esta no dominio. WinRM por IP exigiria por a
  maquina em TrustedHosts e guardar a senha de uma conta local. O C$ e o sc.exe respondem com a
  credencial que ja esta na sessao.

  =================================================================================================
  O QUE ELE NAO TOCA: appsettings.Production.json (senha do banco e segredo do Entra ID) e ssl\
  (certificado). Sobrescreve-los trocaria a configuracao de producao pela desta estacao.

  O instalar-no-servidor.ps1 continua existindo para o que so se faz UMA VEZ: instalar o SQL Server,
  restaurar o banco, criar o servico, o certificado e a regra de firewall.
#>

[CmdletBinding()]
param(
    [string] $Servidor = '10.150.4.249',
    [string] $NomeDns  = '360-TracbelAgro.tracbel.com.br',
    [int]    $Porta    = 5443,
    [string] $Servico  = 'TracbelCrmApi',
    [string] $Destino  = 'C$\aplicacoes\tracbel-crm',
    [switch] $PularTestes
)

$ErrorActionPreference = 'Stop'

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t)    { Write-Host "   OK  $t" -ForegroundColor Green }

$raiz   = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$remoto = "\\$Servidor\$Destino"

# O PACOTE MONTA NUMA PASTA TEMPORARIA, e nao em publicacao\. Aquela pasta guarda o pacote de
# INSTALACAO, com o backup do banco; publicar por cima dela misturaria as duas coisas.
$pacote = Join-Path ([IO.Path]::GetTempPath()) 'tracbel-crm-publicacao'

function EstadoDoServico {
    $saida = & sc.exe "\\$Servidor" query $Servico 2>&1
    if ($saida -match 'RUNNING') { return 'RUNNING' }
    if ($saida -match 'STOPPED') { return 'STOPPED' }
    if ($saida -match 'PENDING') { return 'PENDING' }
    return 'DESCONHECIDO'
}

function EsperarEstado([string] $alvo, [int] $segundos) {
    $limite = (Get-Date).AddSeconds($segundos)
    while ((Get-Date) -lt $limite) {
        if ((EstadoDoServico) -eq $alvo) { return $true }
        Start-Sleep -Seconds 2
    }
    return $false
}

# -------------------------------------------------------------------------------------------------
Passo '0. Alcance'
# -------------------------------------------------------------------------------------------------
if (-not (Test-Path $remoto)) { throw "Nao alcancei $remoto - confira a VPN e a credencial da sessao." }
Ok "servidor alcancavel ($remoto)"
Ok ("servico {0}: {1}" -f $Servico, (EstadoDoServico))

# -------------------------------------------------------------------------------------------------
Passo '1. Testes'
# -------------------------------------------------------------------------------------------------
if ($PularTestes) {
    Write-Host '   !   testes pulados a pedido' -ForegroundColor Yellow
} else {
    Push-Location $raiz
    try {
        & dotnet test --nologo -v q
        if ($LASTEXITCODE -ne 0) { throw 'Os testes falharam. Nada foi enviado ao servidor.' }
    } finally { Pop-Location }
    Ok 'testes passando'
}

# -------------------------------------------------------------------------------------------------
Passo '2. Pacote'
# -------------------------------------------------------------------------------------------------
if (Test-Path $pacote) { Remove-Item $pacote -Recurse -Force }

& dotnet publish (Join-Path $raiz 'src\Tracbel.Crm.Api\Tracbel.Crm.Api.csproj') `
    -c Release -r win-x64 --self-contained true -o $pacote --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'O dotnet publish falhou. Nada foi enviado ao servidor.' }

Push-Location (Join-Path $raiz 'src\Tracbel.Crm.Web')
try {
    & npm run build
    if ($LASTEXITCODE -ne 0) { throw 'O build do front falhou. Nada foi enviado ao servidor.' }
} finally { Pop-Location }

Copy-Item (Join-Path $raiz 'src\Tracbel.Crm.Web\dist') (Join-Path $pacote 'wwwroot') -Recurse -Force

$arquivos = Get-ChildItem $pacote -Recurse -File
Ok ("pacote com {0} arquivos, {1} MB" -f $arquivos.Count, [math]::Round(($arquivos | Measure-Object Length -Sum).Sum / 1MB, 1))

# -------------------------------------------------------------------------------------------------
Passo '3. Parando o servico'
# -------------------------------------------------------------------------------------------------
# PARAR ANTES DE COPIAR, e nao copiar com ele no ar: o executavel e as DLLs ficam presos enquanto o
# processo roda, e uma copia parcial deixaria o servidor com versao misturada.
& sc.exe "\\$Servidor" stop $Servico | Out-Null
if (-not (EsperarEstado 'STOPPED' 60)) { throw 'O servico nao parou em 60 s. Nada foi copiado.' }
Start-Sleep -Seconds 3
Ok 'servico parado'

# -------------------------------------------------------------------------------------------------
Passo '4. Copiando'
# -------------------------------------------------------------------------------------------------
& robocopy $pacote $remoto /E /XF appsettings.Production.json /XD ssl logs /NFL /NDL /NJH /NJS /NP /R:3 /W:5 /MT:8 | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw ("A copia falhou (robocopy {0}). O servico ficou PARADO de proposito: subir com metade dos " +
           "arquivos novos rodaria versao misturada. Resolva e rode de novo." -f $LASTEXITCODE)
}
Ok 'arquivos copiados (configuracao e certificado de producao preservados)'

# -------------------------------------------------------------------------------------------------
Passo '5. Subindo o servico'
# -------------------------------------------------------------------------------------------------
& sc.exe "\\$Servidor" start $Servico | Out-Null

# 3 minutos, e nao 30 segundos: se houver migracao pendente, ela roda antes de a API atender.
if (-not (EsperarEstado 'RUNNING' 180)) {
    throw ("O servico nao chegou a RUNNING. Se houve migracao, ela pode ter falhado - a API nao sobe " +
           "com o banco pela metade. Veja o Log de Aplicacao do Windows no servidor.")
}
Ok 'servico RUNNING'

# -------------------------------------------------------------------------------------------------
Passo '6. Prova de vida'
# -------------------------------------------------------------------------------------------------
# PELO NOME DNS, com o certificado validado. Conferir por IP pularia justamente a parte que o
# usuario usa: o certificado cobre o NOME, e o login do Entra ID so aceita o endereco cadastrado.
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$saude = $null
$limite = (Get-Date).AddSeconds(90)
while ((Get-Date) -lt $limite -and -not $saude) {
    try { $saude = Invoke-RestMethod -Uri "https://$NomeDns`:$Porta/saude/banco" -TimeoutSec 15 }
    catch { Start-Sleep -Seconds 3 }
}
if (-not $saude) { throw "https://$NomeDns`:$Porta/saude/banco nao respondeu em 90 s." }
if (-not $saude.conectado) { throw 'A API subiu, mas nao alcanca o banco.' }
if (@($saude.migracoesPendentes).Count -gt 0) {
    throw ('Ficaram migracoes pendentes: ' + (@($saude.migracoesPendentes) -join ', '))
}
Ok 'API no ar, banco conectado, nenhuma migracao pendente'

# -------------------------------------------------------------------------------------------------
Passo '7. Conferencia por hash'
# -------------------------------------------------------------------------------------------------
$diferentes = @()
$faltando = @()
foreach ($a in $arquivos) {
    $relativo = $a.FullName.Substring($pacote.Length).TrimStart('\')
    $noServidor = Join-Path $remoto $relativo
    if (-not (Test-Path $noServidor)) { $faltando += $relativo; continue }
    if ((Get-FileHash $noServidor -Algorithm SHA256).Hash -ne (Get-FileHash $a.FullName -Algorithm SHA256).Hash) {
        $diferentes += $relativo
    }
}
if ($faltando.Count)   { Write-Host ('   FALTANDO: ' + ($faltando -join ', ')) -ForegroundColor Red }
if ($diferentes.Count) { Write-Host ('   DIFERENTES: ' + ($diferentes -join ', ')) -ForegroundColor Red }
if ($faltando.Count -or $diferentes.Count) { throw 'O servidor nao ficou identico ao pacote. Veja a lista acima.' }
Ok ("{0} arquivos identicos ao pacote" -f $arquivos.Count)

Write-Host ''
Write-Host "Pronto: https://$NomeDns`:$Porta" -ForegroundColor Green
Write-Host ''
