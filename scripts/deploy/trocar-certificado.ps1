<#
  trocar-certificado.ps1 - troca o certificado HTTPS do CRM no servidor, com volta atras.

      .\scripts\deploy\trocar-certificado.ps1 -SoConferir      # so abre e confere o .pfx, nao toca no servidor
      .\scripts\deploy\trocar-certificado.ps1                  # troca de verdade

  O .pfx padrao e o da pasta Downloads (wildcard_tracbel_com_br.pfx); outro vai por -Pfx.

  =================================================================================================
  ONDE O CERTIFICADO MORA

  A API le `C:\aplicacoes\tracbel-crm\ssl\crm.pfx`, com a senha em `Kestrel:Endpoints:Https:Certificate`
  do `appsettings.Production.json` (instalar-no-servidor.ps1, passo 4). Os dois sao DO SERVIDOR: toda
  publicacao os preserva. Trocar o certificado e trocar esses dois, e mais nada.

  =================================================================================================
  A ORDEM, E POR QUE ELA E ESSA

    1. ABRE O .pfx AQUI, na estacao, com a senha: tem chave privada, esta na validade, cobre o nome
       que o usuario usa. Um .pfx que nao abre derruba o servico na partida sem dizer por que - o
       defeito da primeira instalacao (instalar-no-servidor.ps1, passo 4).
    2. NAO TROCA NO MEIO DE UMA PUBLICACAO: o agente reinicia a API, e as duas coisas juntas
       confundiriam a prova de vida das duas.
    3. GUARDA o crm.pfx e o appsettings.Production.json atuais (o .pfx ao lado, com a data).
    4. COPIA o novo e grava a senha - POR SMB, DESTA ESTACAO. A senha nao vai no script remoto: la ela
       ficaria num arquivo e no transcript do _remoto.ps1.
    5. REINICIA a API e confere DESTA ESTACAO, pelo nome e com o certificado validado, que o servidor
       entrega o certificado NOVO.
    6. SE NAO ENTREGAR, VOLTA os dois arquivos e reinicia de novo.

  A SENHA e pedida na tela (nao aparece nem fica no historico), nao e impressa e nao entra no
  repositorio. No servidor, fica onde ja ficava a do certificado anterior.
#>

[CmdletBinding()]
param(
    [string] $Pfx = (Join-Path $env:USERPROFILE 'Downloads\wildcard_tracbel_com_br.pfx'),
    [string] $Servidor = '10.150.4.249',
    [string] $DestinoDaApi = 'C:\aplicacoes\tracbel-crm',
    [string] $ServicoDaApi = 'TracbelCrmApi',
    [string] $PastaDoAgente = 'C:\aplicacoes\tracbel-crm-agente',
    [string] $NomeDns = '360-TracbelAgro.tracbel.com.br',
    [int]    $PortaApi = 5443,
    [switch] $SoConferir
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

# O DnsNameList DO CERTIFICADO VEM DESTE MODULO. O 5.1 o carrega sozinho; o 7 nao, e a lista chega
# vazia - "nao cobre o nome" para um certificado que cobre (medido em 21/09/2026).
Import-Module Microsoft.PowerShell.Security

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t) { Write-Host "   OK  $t" -ForegroundColor Green }
function Aviso([string] $t) { Write-Host "   !   $t" -ForegroundColor Yellow }

<# O nome cabe no certificado? O curinga cobre UM nivel: *.tracbel.com.br cobre 360-TracbelAgro.tracbel.com.br. #>
function CobreONome($certificado, [string] $nome) {
    foreach ($n in @($certificado.DnsNameList | ForEach-Object { "$_" })) {
        if ($n -ieq $nome) { return $true }
        if ($n.StartsWith('*.')) {
            $ponto = $nome.IndexOf('.')
            if ($ponto -gt 0 -and $nome.Substring($ponto + 1) -ieq $n.Substring(2)) { return $true }
        }
    }
    return $false
}

<# O certificado que o servidor ENTREGA agora, pelo nome - sem validar, so para ler a impressao. #>
function CertificadoEntregue() {
    $tcp = New-Object System.Net.Sockets.TcpClient($NomeDns, $PortaApi)
    try {
        $ssl = New-Object System.Net.Security.SslStream($tcp.GetStream(), $false, { $true })
        $ssl.AuthenticateAsClient($NomeDns)
        return New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($ssl.RemoteCertificate)
    } finally { $tcp.Dispose() }
}

<# A API responde pelo nome, COM o certificado validado, e alcanca o banco? #>
function RespondeComCertificadoValido() {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $limite = (Get-Date).AddSeconds(120)
    $ultimo = 'nenhuma resposta'
    while ((Get-Date) -lt $limite) {
        Start-Sleep -Seconds 5
        try {
            $r = Invoke-RestMethod -Uri "https://${NomeDns}:$PortaApi/saude/banco" -TimeoutSec 15
            if ($r.conectado -eq $true) { return $true }
            $ultimo = "respondeu sem banco conectado"
        } catch { $ultimo = $_.Exception.Message }
    }
    Aviso "a API nao respondeu com certificado valido em 120 s: $ultimo"
    return $false
}

function ReiniciarApi() {
    $saida = Invoke-NoServidor -Nome 'crm-reiniciar-api' -TimeoutSegundos 300 -Script @"
Restart-Service -Name '$ServicoDaApi' -Force
(Get-Service '$ServicoDaApi').WaitForStatus('Running', '00:02:00')
Write-Output ('servico: ' + (Get-Service '$ServicoDaApi').Status)
"@
    Write-Host "   $saida"
}

# -------------------------------------------------------------------------------------------------
Passo '1. Abrir e conferir o certificado novo, aqui na estacao'
# -------------------------------------------------------------------------------------------------
if (-not (Test-Path -LiteralPath $Pfx)) { throw "Nao achei o certificado em $Pfx. Passe o caminho com -Pfx." }
Ok "arquivo: $Pfx"

$senha = Read-Host '   Senha do .pfx (nao aparece na tela; ENTER se ele nao tiver senha)' -AsSecureString
$senhaTexto = [System.Net.NetworkCredential]::new('', $senha).Password

try {
    $novo = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
        $Pfx, $senha, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
} catch {
    throw "Nao consegui abrir o .pfx (senha errada ou arquivo corrompido): $($_.Exception.Message.Split([char]13)[0])"
}

Ok ("titular ............ {0}" -f $novo.Subject)
Ok ("emissor ............ {0}" -f $novo.Issuer)
Ok ("validade ........... {0:dd/MM/yyyy} a {1:dd/MM/yyyy}" -f $novo.NotBefore, $novo.NotAfter)
Ok ("impressao .......... {0}" -f $novo.Thumbprint)

if (-not $novo.HasPrivateKey) { throw 'O .pfx nao tem chave privada: nao serve para servir HTTPS. Nada foi trocado.' }
if ($novo.NotAfter -lt (Get-Date)) { throw 'Este certificado JA VENCEU. Nada foi trocado.' }
if ($novo.NotBefore -gt (Get-Date)) { throw "Este certificado so vale a partir de $($novo.NotBefore.ToString('dd/MM/yyyy')). Nada foi trocado." }
if (-not (CobreONome $novo $NomeDns)) {
    throw "O certificado nao cobre $NomeDns (nomes: $(@($novo.DnsNameList) -join ', ')). Nada foi trocado."
}
Ok "tem chave privada, esta na validade e cobre $NomeDns"

$cadeia = New-Object System.Security.Cryptography.X509Certificates.X509Chain
if ($cadeia.Build($novo)) { Ok 'a cadeia fecha nesta estacao' } else { Aviso 'a cadeia NAO fecha nesta estacao - a conferencia do passo 5 dira se o navegador aceita' }

$atual = CertificadoEntregue
Ok ("o servidor entrega hoje: {0}, vence em {1:dd/MM/yyyy}" -f $atual.Thumbprint, $atual.NotAfter)
if ($atual.Thumbprint -eq $novo.Thumbprint) {
    Ok 'o servidor JA entrega este certificado. Nada a trocar.'
    return
}

if ($SoConferir) {
    Ok 'so conferencia (-SoConferir): nada foi trocado no servidor'
    return
}

# -------------------------------------------------------------------------------------------------
Passo '2. Nenhuma publicacao em andamento'
# -------------------------------------------------------------------------------------------------
$estadoPorSmb = "\\$Servidor\$($PastaDoAgente -replace ':', '$')\estado.json"
if (Test-Path -LiteralPath $estadoPorSmb) {
    $estado = Get-Content -LiteralPath $estadoPorSmb -Raw | ConvertFrom-Json
    if ($estado.situacao -eq 'publicando') {
        throw "O agente esta publicando agora ($($estado.detalhe)). Espere terminar (verificar-publicacao.ps1) e rode de novo. Nada foi trocado."
    }
    Ok "o agente nao esta publicando (situacao: $($estado.situacao))"
} else {
    Ok 'o agente de publicacao nao esta instalado; nada para esperar'
}

# -------------------------------------------------------------------------------------------------
Passo '3. Guardar o certificado e a configuracao atuais'
# -------------------------------------------------------------------------------------------------
$apiPorSmb = "\\$Servidor\$($DestinoDaApi -replace ':', '$')"
$pfxNoServidor = Join-Path $apiPorSmb 'ssl\crm.pfx'
$configNoServidor = Join-Path $apiPorSmb 'appsettings.Production.json'

if (-not (Test-Path -LiteralPath $configNoServidor)) { throw "Nao achei $configNoServidor. Nada foi trocado." }

$guardaDoPfx = Join-Path $apiPorSmb ("ssl\crm-ate-{0:yyyyMMdd-HHmm}.pfx" -f (Get-Date))
if (Test-Path -LiteralPath $pfxNoServidor) {
    Copy-Item -LiteralPath $pfxNoServidor $guardaDoPfx -Force
    Ok "certificado atual guardado em ssl\$(Split-Path $guardaDoPfx -Leaf)"
}

# A CONFIGURACAO FICA NA MEMORIA, byte a byte, para voltar exatamente como estava.
$configOriginal = [System.IO.File]::ReadAllBytes($configNoServidor)
$config = [System.Text.Encoding]::UTF8.GetString($configOriginal).TrimStart([char]0xFEFF) | ConvertFrom-Json
$cert = $config.Kestrel.Endpoints.Https.Certificate
if (-not $cert) { throw 'O appsettings.Production.json nao tem Kestrel:Endpoints:Https:Certificate. Nada foi trocado.' }
Ok 'configuracao atual lida e guardada na memoria'

# -------------------------------------------------------------------------------------------------
Passo '4. Copiar o certificado novo e gravar a senha'
# -------------------------------------------------------------------------------------------------
Copy-Item -LiteralPath $Pfx $pfxNoServidor -Force
Ok 'certificado novo em ssl\crm.pfx'

# SEM SENHA, O CAMPO NAO ENTRA: "Password": "" faz o Kestrel abrir COM senha vazia, que e diferente de
# abrir SEM senha (instalar-no-servidor.ps1, passo 5).
$cert.Path = "$DestinoDaApi\ssl\crm.pfx"
if ($senhaTexto) {
    if ($cert.PSObject.Properties['Password']) { $cert.Password = $senhaTexto }
    else { $cert | Add-Member -NotePropertyName Password -NotePropertyValue $senhaTexto }
} elseif ($cert.PSObject.Properties['Password']) {
    $cert.PSObject.Properties.Remove('Password')
}
$senhaTexto = $null

$json = $config | ConvertTo-Json -Depth 20
[System.IO.File]::WriteAllText($configNoServidor, $json, (New-Object System.Text.UTF8Encoding($false)))
$json = $null
Ok 'senha gravada no appsettings.Production.json (nao impressa)'

# -------------------------------------------------------------------------------------------------
Passo '5. Reiniciar a API e conferir o certificado entregue'
# -------------------------------------------------------------------------------------------------
ReiniciarApi
$respondeu = RespondeComCertificadoValido
$entregue = $null
try { $entregue = CertificadoEntregue } catch { Aviso "nao consegui ler o certificado entregue: $($_.Exception.Message)" }

if ($respondeu -and $entregue -and $entregue.Thumbprint -eq $novo.Thumbprint) {
    Ok ("o servidor entrega o certificado NOVO ({0}), valido ate {1:dd/MM/yyyy}, e a API responde com o banco conectado" -f $entregue.Thumbprint, $entregue.NotAfter)
    Write-Host ''
    Write-Host "   O anterior ficou em $DestinoDaApi\ssl\$(Split-Path $guardaDoPfx -Leaf)." -ForegroundColor Green
    Write-Host '   Apague o .pfx da pasta Downloads: a copia que vale e a do servidor.' -ForegroundColor Green
    return
}

# -------------------------------------------------------------------------------------------------
Passo '6. NAO DEU CERTO: voltando o certificado e a configuracao anteriores'
# -------------------------------------------------------------------------------------------------
if (Test-Path -LiteralPath $guardaDoPfx) { Copy-Item -LiteralPath $guardaDoPfx $pfxNoServidor -Force }
[System.IO.File]::WriteAllBytes($configNoServidor, $configOriginal)
ReiniciarApi

if (RespondeComCertificadoValido) {
    Aviso 'o certificado anterior voltou e a API responde. O novo NAO foi aplicado - veja os avisos acima.'
    exit 3
}
Aviso 'nem o certificado anterior respondeu. Confira o servico TracbelCrmApi no servidor e o log de eventos.'
exit 5
