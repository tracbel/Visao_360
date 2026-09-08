<#
  instalar-no-servidor.ps1 - instala o CRM Tracbel Agro no servidor de aplicacao.

  =================================================================================================
  EXECUTAR NO PROPRIO SERVIDOR (10.150.4.249), COMO ADMINISTRADOR.

      cd C:\projetos\tracbel-crm-deploy
      powershell -NoProfile -ExecutionPolicy Bypass -File .\instalar-no-servidor.ps1

  =================================================================================================
  O QUE ELE INSTALA, E POR QUE ASSIM.

  UM SERVICO SO, e nao IIS na frente da API. Separar front e API em duas portas exigiria CORS na
  API (que ela nao publica de proposito), um segundo certificado e o modulo de proxy do IIS (que
  nao esta instalado nesta maquina). A API serve o front do proprio wwwroot: mesma origem, um
  certificado, uma porta.

  A API VAI SELF-CONTAINED. Este servidor tem .NET 6 e 8; o projeto e .NET 9. Publicar
  self-contained leva o runtime dentro do pacote e evita instalar um runtime novo numa maquina que
  ja hospeda o user-onboarding em producao.

  O BANCO VAI EM CONTAINER, com teto de memoria explicito. O SQL Server toma memoria para o buffer
  pool e nao devolve: medido na estacao de desenvolvimento, ele estava com 4,5 GB para um banco de
  1,5 GB, porque ninguem tinha posto limite. Aqui ele tem teto, e o teto do Docker e MAIOR que o do
  SQL Server de proposito - assim quem segura e o proprio SQL Server, em vez de o Linux matar o
  processo no meio de uma consulta.

  =================================================================================================
  O QUE ELE NAO FAZ: nao mexe em nada do user-onboarding, nao altera o IIS, nao instala runtime.
#>

[CmdletBinding()]
param(
    [string] $Raiz        = 'C:\projetos\tracbel-crm-deploy',
    [string] $Destino     = 'C:\aplicacoes\tracbel-crm',
    [int]    $PortaApi    = 5443,
    [int]    $PortaBanco  = 14330,
    [string] $NomeServico = 'TracbelCrmApi',
    [string] $NomeContainer = 'tracbel-crm-db',
    [int]    $TetoDockerGB = 3,
    [int]    $TetoSqlMB    = 2560
)

$ErrorActionPreference = 'Stop'

function Passo([string] $texto) {
    Write-Host ''
    Write-Host "== $texto" -ForegroundColor Cyan
}
function Ok([string] $texto)   { Write-Host "   OK  $texto" -ForegroundColor Green }
function Aviso([string] $t)    { Write-Host "   !   $t" -ForegroundColor Yellow }

# -------------------------------------------------------------------------------------------------
# 0. Pre-requisitos. Falhar aqui e barato; falhar na metade nao e.
# -------------------------------------------------------------------------------------------------
Passo '0. Conferindo pre-requisitos'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Este script precisa rodar como Administrador.'
}
Ok 'sessao com privilegio de administrador'

if (-not (Test-Path (Join-Path $Raiz 'publicacao\Tracbel.Crm.Api.exe'))) {
    throw "Nao achei a publicacao em $Raiz\publicacao. Copie o pacote antes de rodar."
}
Ok 'pacote da aplicacao presente'

$bak = Join-Path $Raiz 'publicacao\_banco\TracbelCrm.bak'
if (-not (Test-Path $bak)) { throw "Nao achei o backup do banco em $bak." }
Ok ("backup do banco presente ({0} MB)" -f [math]::Round((Get-Item $bak).Length/1MB,1))

try { $null = & docker version --format '{{.Server.Version}}' 2>&1 }
catch { throw 'O Docker nao respondeu. Suba o Docker Desktop antes de continuar.' }
if ($LASTEXITCODE -ne 0) { throw 'O Docker nao respondeu. Suba o Docker Desktop antes de continuar.' }
Ok 'docker respondendo'

# A PORTA TEM DE ESTAR LIVRE, e a checagem e antes de qualquer mudanca. Subir um servico numa porta
# ocupada derruba quem estava la - e quem esta la, nesta maquina, pode ser o user-onboarding.
foreach ($p in @($PortaApi, $PortaBanco)) {
    if (Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue) {
        throw "A porta $p ja esta em uso neste servidor. Escolha outra e rode de novo com -PortaApi/-PortaBanco."
    }
}
Ok "portas $PortaApi e $PortaBanco livres"

# -------------------------------------------------------------------------------------------------
# 1. A senha do banco. Nao vem em arquivo nenhum do pacote.
# -------------------------------------------------------------------------------------------------
Passo '1. Senha do banco'

$senha = Read-Host 'Senha para o usuario sa do SQL Server (nao aparece na tela)' -AsSecureString
$senhaTexto = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($senha))

if ($senhaTexto.Length -lt 12) { throw 'A senha do SQL Server precisa de pelo menos 12 caracteres.' }
Ok 'senha recebida'

# -------------------------------------------------------------------------------------------------
# 2. O container do banco.
# -------------------------------------------------------------------------------------------------
Passo '2. Subindo o SQL Server em container'

$existe = & docker ps -a --filter "name=^$NomeContainer$" --format '{{.Names}}'
if ($existe) {
    Aviso "container $NomeContainer ja existe - removendo para recriar com o teto de memoria"
    & docker rm -f $NomeContainer | Out-Null
}

& docker volume create "$NomeContainer-dados" | Out-Null

# `--memory` e o teto do Docker; `MSSQL_MEMORY_LIMIT_MB` e o teto interno do SQL Server, MENOR de
# proposito. Se so o primeiro existisse, estourar significaria o processo morto pelo kernel.
& docker run -d --name $NomeContainer `
    --restart unless-stopped `
    --memory "${TetoDockerGB}g" `
    -e ACCEPT_EULA=Y `
    -e "MSSQL_SA_PASSWORD=$senhaTexto" `
    -e MSSQL_PID=Developer `
    -e "MSSQL_MEMORY_LIMIT_MB=$TetoSqlMB" `
    -e MSSQL_COLLATION=Latin1_General_CI_AI `
    -p "${PortaBanco}:1433" `
    -v "${NomeContainer}-dados:/var/opt/mssql" `
    mcr.microsoft.com/mssql/server:2022-latest | Out-Null

if ($LASTEXITCODE -ne 0) { throw 'Nao consegui subir o container do banco.' }
Ok "container no ar, teto de ${TetoDockerGB} GB (SQL Server limitado a ${TetoSqlMB} MB)"

Write-Host '   aguardando o SQL Server aceitar conexao...' -NoNewline
$pronto = $false
foreach ($i in 1..60) {
    Start-Sleep -Seconds 5
    $c = New-Object System.Data.SqlClient.SqlConnection(
        "Server=localhost,$PortaBanco;Database=master;User Id=sa;Password=$senhaTexto;TrustServerCertificate=True;Connect Timeout=5")
    try { $c.Open(); $c.Close(); $pronto = $true; break } catch { Write-Host '.' -NoNewline }
}
Write-Host ''
if (-not $pronto) { throw 'O SQL Server nao aceitou conexao em 5 minutos. Veja: docker logs ' + $NomeContainer }
Ok 'banco aceitando conexao'

# -------------------------------------------------------------------------------------------------
# 3. Restaurar o backup.
# -------------------------------------------------------------------------------------------------
Passo '3. Restaurando o banco'

& docker exec $NomeContainer mkdir -p /var/opt/mssql/backup | Out-Null
& docker cp $bak "${NomeContainer}:/var/opt/mssql/backup/TracbelCrm.bak" | Out-Null
Ok 'backup copiado para dentro do container'

function Sql([string] $comando, [int] $timeout = 3600) {
    $c = New-Object System.Data.SqlClient.SqlConnection(
        "Server=localhost,$PortaBanco;Database=master;User Id=sa;Password=$senhaTexto;TrustServerCertificate=True")
    $c.Open()
    try {
        $cmd = $c.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $comando
        return $cmd.ExecuteScalar()
    } finally { $c.Close() }
}

# O caminho dos arquivos dentro do backup nao e o mesmo do container novo: MOVE resolve.
Sql @"
RESTORE DATABASE [TracbelCrm] FROM DISK='/var/opt/mssql/backup/TracbelCrm.bak'
WITH MOVE 'TracbelCrm'     TO '/var/opt/mssql/data/TracbelCrm.mdf',
     MOVE 'TracbelCrm_log' TO '/var/opt/mssql/data/TracbelCrm_log.ldf',
     REPLACE, RECOVERY
"@ | Out-Null
Ok 'banco restaurado'

& docker exec $NomeContainer rm -f /var/opt/mssql/backup/TracbelCrm.bak | Out-Null

# O USUARIO DA APLICACAO NAO E O `sa`. O `sa` e do administrador do banco; a API entra com uma
# conta propria, que so alcanca o banco do CRM.
$senhaApp = -join ((48..57) + (65..90) + (97..122) + (35,37,42,45,95) | Get-Random -Count 24 | ForEach-Object { [char]$_ })

Sql @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name='TracbelCrm')
    CREATE LOGIN [TracbelCrm] WITH PASSWORD = '$senhaApp', CHECK_POLICY = OFF;
ELSE
    ALTER LOGIN [TracbelCrm] WITH PASSWORD = '$senhaApp';
"@ | Out-Null

Sql @"
USE [TracbelCrm];
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name='TracbelCrm')
    DROP USER [TracbelCrm];
CREATE USER [TracbelCrm] FOR LOGIN [TracbelCrm];
ALTER ROLE db_datareader ADD MEMBER [TracbelCrm];
ALTER ROLE db_datawriter ADD MEMBER [TracbelCrm];
ALTER ROLE db_ddladmin  ADD MEMBER [TracbelCrm];
"@ | Out-Null
Ok 'conta da aplicacao criada (senha gerada aqui, nunca versionada)'

$linhas = Sql "SELECT COUNT(*) FROM [TracbelCrm].comercial.FaturamentoDoCliente WHERE ExcluidoEm IS NULL"
Ok "conferencia: $linhas meses de faturamento no banco restaurado"

# -------------------------------------------------------------------------------------------------
# 4. A aplicacao.
# -------------------------------------------------------------------------------------------------
Passo '4. Instalando a aplicacao'

if (Get-Service -Name $NomeServico -ErrorAction SilentlyContinue) {
    Aviso "servico $NomeServico ja existe - parando para atualizar"
    Stop-Service -Name $NomeServico -Force -ErrorAction SilentlyContinue
    & sc.exe delete $NomeServico | Out-Null
    Start-Sleep -Seconds 3
}

New-Item -ItemType Directory -Force -Path $Destino | Out-Null
Copy-Item (Join-Path $Raiz 'publicacao\*') $Destino -Recurse -Force -Exclude '_banco'
Remove-Item (Join-Path $Destino '_banco') -Recurse -Force -ErrorAction SilentlyContinue
Ok "arquivos em $Destino"

# -------------------------------------------------------------------------------------------------
# 5. Certificado.
# -------------------------------------------------------------------------------------------------
Passo '5. Certificado'

$pastaSsl = Join-Path $Destino 'ssl'
New-Item -ItemType Directory -Force -Path $pastaSsl | Out-Null
$pfx = Join-Path $pastaSsl 'crm.pfx'
$senhaPfx = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 20 | ForEach-Object { [char]$_ })

if (-not (Test-Path $pfx)) {
    $cert = New-SelfSignedCertificate `
        -Subject 'CN=tracbel-crm' `
        -DnsName 'tracbel-crm', 'localhost', '10.150.4.249' `
        -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
        -NotAfter (Get-Date).AddYears(3) `
        -CertStoreLocation 'Cert:\LocalMachine\My' -KeyExportPolicy Exportable
    Export-PfxCertificate -Cert $cert -FilePath $pfx `
        -Password (ConvertTo-SecureString $senhaPfx -AsPlainText -Force) | Out-Null
    Ok "certificado AUTO-ASSINADO gerado (impressao: $($cert.Thumbprint))"
    Aviso 'auto-assinado faz o navegador mostrar aviso de seguranca em toda visita.'
    Aviso 'Se a Tracbel tiver CA interna ou certificado curinga, troque este .pfx pelo emitido.'
} else {
    Ok 'ja existe um crm.pfx - mantido'
    $senhaPfx = Read-Host 'Senha do crm.pfx existente'
}

# -------------------------------------------------------------------------------------------------
# 6. Configuracao e servico.
# -------------------------------------------------------------------------------------------------
Passo '6. Registrando o servico'

# A CONFIGURACAO FICA FORA DO PACOTE, para que reinstalar a aplicacao nao apague a senha do banco.
$config = @{
    ConnectionStrings = @{
        Crm = "Server=localhost,$PortaBanco;Database=TracbelCrm;User Id=TracbelCrm;Password=$senhaApp;TrustServerCertificate=True"
    }
    Kestrel = @{
        Endpoints = @{
            Https = @{
                Url = "https://0.0.0.0:$PortaApi"
                Certificate = @{ Path = $pfx; Password = $senhaPfx }
            }
        }
    }
} | ConvertTo-Json -Depth 8

Set-Content -Path (Join-Path $Destino 'appsettings.Production.json') -Value $config -Encoding UTF8

# A senha do banco esta neste arquivo: so administrador le.
$acl = Get-Acl (Join-Path $Destino 'appsettings.Production.json')
$acl.SetAccessRuleProtection($true, $false)
$acl.SetAccessRule((New-Object Security.AccessControl.FileSystemAccessRule('BUILTIN\Administrators','FullControl','Allow')))
$acl.SetAccessRule((New-Object Security.AccessControl.FileSystemAccessRule('NT AUTHORITY\SYSTEM','FullControl','Allow')))
Set-Acl (Join-Path $Destino 'appsettings.Production.json') $acl
Ok 'configuracao gravada e restrita a administradores'

& sc.exe create $NomeServico `
    binPath= "`"$Destino\Tracbel.Crm.Api.exe`"" `
    DisplayName= 'Tracbel CRM Agro - API e portal' `
    start= auto | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Nao consegui criar o servico.' }

& sc.exe description $NomeServico 'API e portal do CRM Tracbel Agro. Serve o front e le o banco em container.' | Out-Null
# Se cair, reinicia sozinho - tres vezes, com espera crescente.
& sc.exe failure $NomeServico reset= 86400 actions= restart/5000/restart/15000/restart/60000 | Out-Null

[Environment]::SetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', 'Production', 'Machine')

Start-Service -Name $NomeServico
Start-Sleep -Seconds 10
$estado = (Get-Service -Name $NomeServico).Status
if ($estado -ne 'Running') { throw "O servico subiu como $estado. Veja o Visualizador de Eventos." }
Ok "servico $NomeServico rodando"

# -------------------------------------------------------------------------------------------------
# 7. Firewall.
# -------------------------------------------------------------------------------------------------
Passo '7. Firewall'

$regra = "Tracbel CRM Agro ($PortaApi)"
Get-NetFirewallRule -DisplayName $regra -ErrorAction SilentlyContinue | Remove-NetFirewallRule
New-NetFirewallRule -DisplayName $regra -Direction Inbound -Protocol TCP `
    -LocalPort $PortaApi -Action Allow -Profile Any | Out-Null
Ok "porta $PortaApi liberada"

# A PORTA DO BANCO NAO E LIBERADA. A API fala com ele por localhost; abrir 1433 para a rede so
# aumentaria superficie sem servir a ninguem. Quem precisar do DBeaver usa tunel ou roda daqui.
Aviso "a porta do banco ($PortaBanco) NAO foi liberada no firewall - de proposito"

# -------------------------------------------------------------------------------------------------
# 8. Prova de vida.
# -------------------------------------------------------------------------------------------------
Passo '8. Conferindo'

Add-Type @'
using System.Net; using System.Security.Cryptography.X509Certificates;
public class SemChecagemCrm : ICertificatePolicy {
  public bool CheckValidationResult(ServicePoint sp, X509Certificate c, WebRequest r, int p) { return true; }
}
'@ -ErrorAction SilentlyContinue
[Net.ServicePointManager]::CertificatePolicy = New-Object SemChecagemCrm

Start-Sleep -Seconds 5
try {
    $saude = Invoke-RestMethod -Uri "https://localhost:$PortaApi/saude/banco" -TimeoutSec 30
    Ok ("API respondendo - banco conectado: {0}, migracoes pendentes: {1}" -f `
        $saude.conectado, $(if ($saude.migracoesPendentes.Count) { $saude.migracoesPendentes -join ',' } else { 'nenhuma' }))
} catch {
    Aviso ('a prova de vida falhou: ' + $_.Exception.Message)
}

try {
    $portal = Invoke-WebRequest -Uri "https://localhost:$PortaApi/" -TimeoutSec 30 -UseBasicParsing
    Ok ("portal respondendo - HTTP {0}, {1} bytes" -f $portal.StatusCode, $portal.RawContentLength)
} catch {
    Aviso ('o portal nao respondeu: ' + $_.Exception.Message)
}

Write-Host ''
Write-Host '=================================================================' -ForegroundColor Green
Write-Host " CRM no ar em:  https://10.150.4.249:$PortaApi" -ForegroundColor Green
Write-Host '=================================================================' -ForegroundColor Green
Write-Host ''
Write-Host ' O que ficou PENDENTE, e nao e pouco:' -ForegroundColor Yellow
Write-Host '  1. Backup do banco - nao existe rotina nenhuma. Este container e agora o oficial.'
Write-Host '  2. Certificado auto-assinado - todo navegador avisa. Trocar por um da CA interna.'
Write-Host '  3. Licenca: MSSQL_PID=Developer NAO cobre uso produtivo. Confirmar com quem licencia.'
Write-Host '  4. A API nao autentica ninguem ainda (identidade vem de cabecalho). Ver documento 13.'
Write-Host ''
