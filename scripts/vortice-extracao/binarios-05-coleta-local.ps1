<#
  binarios-05-coleta-local.ps1 - Coleta SOMENTE LEITURA a ser executada DENTRO do servidor
  (sessao RDP, PowerShell como Administrador). Fecha a lacuna do agrocrm (10.150.6.230) e do
  COLWCRM (10.150.14.65), que recusaram WMI/DCOM e WinRM na coleta remota de 02/09/2026.

  O QUE FAZ: le IIS, servicos, tarefas agendadas, DSNs ODBC, .NET Framework, certificados,
  PATH, compartilhamentos e os web.config das aplicacoes. Gera um .md no mesmo formato do
  host-ECS-ST-TSPLUS.md, pronto para juntar ao inventario.

  O QUE NAO FAZ: nao altera, para, cria ou apaga nada. Nao instala modulo. Nao imprime senha,
  token ou chave - tudo passa pela mesma redacao do binarios-02-configs.ps1, e o ultimo octeto
  dos IPs privados vira .xx.

  USO (no servidor, como Administrador):
      powershell -ExecutionPolicy Bypass -File .\binarios-05-coleta-local.ps1

  A saida vai para .\host-<NOME>.md na pasta atual (ou -OutDir). Copie o .md de volta para
  c:\projetos\tracbel-crm\docs\extracao-vortice\binarios\ - nao precisa copiar mais nada.
#>
[CmdletBinding()]
param(
    [string]$OutDir = (Get-Location).Path,
    [string]$Rotulo = $env:COMPUTERNAME
)

$ErrorActionPreference = 'Continue'

# ---------------------------------------------------------------- redacao
function Protect-Texto {
    param([string]$t)
    if ($null -eq $t) { return '' }
    $ch = 'pw|pwd|pass|passwd|password|senha|secret|token|apikey|api_key|appkey|clientsecret|client_secret|accountkey|privatekey|credential|authorization|dbpw|defdbpw'
    $t = [regex]::Replace($t, "(?im)^(\s*[\w\.\-\[\]]*($ch)[\w\.\-]*\s*[:=]\s*)(.+)$", '${1}***REDIGIDO***')
    $t = [regex]::Replace($t, "(?i)\b(($ch))\s*=\s*([^;""'>\s]+)", '${1}=***REDIGIDO***')
    $t = [regex]::Replace($t, "(?i)((?:key|name)\s*=\s*""[^""]*($ch)[^""]*""\s+value\s*=\s*"")([^""]*)("")", '${1}***REDIGIDO***${4}')
    $t = [regex]::Replace($t, "(?is)<(\w*(?:pas|pwd|pw|senha|secret|token|key|credential)\w*)>(.*?)</\1>", '<${1}>***REDIGIDO***</${1}>')
    $t = [regex]::Replace($t, "(?is)\[DPT\].*?\[/DPT\]", '[DPT]***REDIGIDO***[/DPT]')
    $t = [regex]::Replace($t, "(?im)^(\s*[\w]*userconnect\s*=\s*)([\w]+)(/)([\w\-]+)", '${1}${2}${3}***REDIGIDO***')
    $t = [regex]::Replace($t, "\b([0-9A-F]{6}-){2,}[0-9A-F]{2,6}\b", '***SERIAL-REDIGIDO***')
    $t = [regex]::Replace($t, '(?i)(://[^/@:\s]+:)([^/@\s]+)(@)', '${1}***REDIGIDO***${3}')
    $t = [regex]::Replace($t, '\b((?:10|172|192)\.\d{1,3}\.\d{1,3})\.(\d{1,3})\b', '${1}.xx')
    $t = [regex]::Replace($t, '(?m)^[A-Za-z0-9+/]{60,}={0,2}$', '***BLOB-REDIGIDO***')
    return $t
}
function Mask { param($t) Protect-Texto ("$t") }

$sb = New-Object System.Text.StringBuilder
function Add { param($t) [void]$sb.AppendLine($t) }

Add "# Host $Rotulo - coleta local (somente leitura)"
Add ''
Add "_Coletado em $(Get-Date -Format 'yyyy-MM-dd HH:mm') por ``$env:USERDOMAIN\$env:USERNAME`` dentro do proprio servidor._"
Add ''
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
         ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { Add '> AVISO: sessao SEM privilegio administrativo. IIS, tarefas agendadas e parte do registro nao serao lidos.'; Add '' }

# ---------------------------------------------------------------- 1. sistema
Add '## 1. Sistema operacional'
Add ''
$os = Get-CimInstance Win32_OperatingSystem
$cs = Get-CimInstance Win32_ComputerSystem
Add '| Item | Valor |'
Add '|---|---|'
Add "| Nome | $($os.CSName) |"
Add "| Sistema | $($os.Caption) |"
Add "| Versao / build | $($os.Version) / $($os.BuildNumber) |"
Add "| Arquitetura | $($os.OSArchitecture) |"
Add "| Instalado em | $($os.InstallDate) |"
Add "| Ultimo boot | $($os.LastBootUpTime) |"
Add "| Dominio | $($cs.Domain) |"
Add "| Fabricante / modelo | $($cs.Manufacturer) / $($cs.Model) |"
Add "| CPUs logicas / RAM | $($cs.NumberOfLogicalProcessors) / $([math]::Round($cs.TotalPhysicalMemory/1GB,1)) GB |"
Add ''

Add '## 2. Volumes'
Add ''
Add '| Unidade | Rotulo | Total GB | Livre GB | % livre |'
Add '|---|---|---:|---:|---:|'
Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' | ForEach-Object {
    $pc = if ($_.Size) { [math]::Round(100*$_.FreeSpace/$_.Size,1) } else { 0 }
    Add "| $($_.DeviceID) | $($_.VolumeName) | $([math]::Round($_.Size/1GB,1)) | $([math]::Round($_.FreeSpace/1GB,1)) | $pc% |"
}
Add ''

# ---------------------------------------------------------------- 3. IIS
Add '## 3. IIS - sites, bindings e application pools'
Add ''
if (Get-Module -ListAvailable -Name WebAdministration) {
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    Add '### Sites'
    Add ''
    Add '| Site | Estado | Caminho fisico | Bindings |'
    Add '|---|---|---|---|'
    foreach ($s in (Get-Website -ErrorAction SilentlyContinue)) {
        $b = ($s.Bindings.Collection | ForEach-Object { $_.bindingInformation }) -join '; '
        Add "| $($s.Name) | $($s.State) | ``$(Mask $s.PhysicalPath)`` | $(Mask $b) |"
    }
    Add ''
    Add '### Aplicacoes e diretorios virtuais'
    Add ''
    Add '| Site | Caminho | AppPool | Caminho fisico |'
    Add '|---|---|---|---|'
    foreach ($a in (Get-WebApplication -ErrorAction SilentlyContinue)) {
        Add "| $($a.GetParentElement()['name']) | $($a.path) | $($a.applicationPool) | ``$(Mask $a.PhysicalPath)`` |"
    }
    foreach ($v in (Get-WebVirtualDirectory -ErrorAction SilentlyContinue)) {
        Add "| (vdir) | $($v.path) | - | ``$(Mask $v.PhysicalPath)`` |"
    }
    Add ''
    Add '### Application pools'
    Add ''
    Add '| Pool | Estado | .NET CLR | Pipeline | Identidade | Enable32Bit |'
    Add '|---|---|---|---|---|---|'
    foreach ($p in (Get-ChildItem IIS:\AppPools -ErrorAction SilentlyContinue)) {
        $id = if ($p.processModel.identityType -eq 'SpecificUser') { "SpecificUser: $($p.processModel.userName)" } else { $p.processModel.identityType }
        $clr = if ($p.managedRuntimeVersion) { $p.managedRuntimeVersion } else { 'Sem codigo gerenciado' }
        Add "| $($p.Name) | $($p.State) | $clr | $($p.managedPipelineMode) | $(Mask $id) | $($p.enable32BitAppOnWin64) |"
    }
    Add ''
    $ver = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\InetStp' -ErrorAction SilentlyContinue)
    if ($ver) { Add "Versao do IIS: **$($ver.MajorVersion).$($ver.MinorVersion)** ($($ver.VersionString))"; Add '' }
} else {
    Add '_Modulo WebAdministration indisponivel - este host provavelmente nao tem o papel de servidor web instalado._'
    Add ''
}

# ---------------------------------------------------------------- 4. web.config
Add '## 4. web.config das aplicacoes (redigido)'
Add ''
$temIIS  = [bool](Get-Command Get-Website -ErrorAction SilentlyContinue)
$raizes  = @('C:\inetpub\wwwroot')
if ($temIIS) { $raizes += (Get-Website -ErrorAction SilentlyContinue | ForEach-Object { $_.PhysicalPath }) }
$raizes  = $raizes | Where-Object { $_ } | Sort-Object -Unique
$configs = foreach ($r in $raizes) {
    if (Test-Path $r) { Get-ChildItem $r -Recurse -Filter 'web.config' -ErrorAction SilentlyContinue -Depth 3 }
}
$configs = $configs | Sort-Object FullName -Unique
if (-not $configs) { Add '_nenhum web.config encontrado._'; Add '' }
foreach ($c in $configs) {
    Add "### ``$(Mask $c.FullName)``"
    Add ''
    Add "_$($c.Length) bytes - modificado em $($c.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))_"
    Add ''
    Add '```xml'
    Add ((Protect-Texto (Get-Content $c.FullName -Raw -ErrorAction SilentlyContinue)).TrimEnd())
    Add '```'
    Add ''
}

Add '### Apontamentos de rede encontrados nos configs'
Add ''
Add '| Arquivo | Linha | Trecho |'
Add '|---|---:|---|'
foreach ($c in $configs) {
    $m = Select-String -LiteralPath $c.FullName -Pattern '\b(?:10|172|192)\.\d{1,3}\.\d{1,3}\.\d{1,3}\b|\\\\[A-Za-z0-9_\-\.]+\\' -AllMatches -ErrorAction SilentlyContinue
    foreach ($l in $m) {
        $v = $l.Line.Trim(); if ($v.Length -gt 120) { $v = $v.Substring(0,120) + '...' }
        Add "| ``$($c.Name)`` | $($l.LineNumber) | ``$(Mask $v)`` |"
    }
}
Add ''

# ---------------------------------------------------------------- 5. servicos
Add '## 5. Servicos Windows'
Add ''
Add '### Vortice / Vtc / Jupiter / SQL / IIS / FTP'
Add ''
Add '| Nome | Estado | Inicio | Conta | Binario |'
Add '|---|---|---|---|---|'
Get-CimInstance Win32_Service |
  Where-Object { $_.Name -match '(?i)vortice|vortico|vtc|jupiter|filezilla|mssql|sqlserver|sqlagent|w3svc|^was$|ftpsvc' -or $_.PathName -match '(?i)vortice|vortico|vtc|jupiter' } |
  Sort-Object Name | ForEach-Object {
    Add "| $($_.Name) | $($_.State) | $($_.StartMode) | $(Mask $_.StartName) | ``$(Mask $_.PathName)`` |"
  }
Add ''
Add '### Demais servicos com binario fora de C:\Windows'
Add ''
Add '| Nome | Estado | Inicio | Binario |'
Add '|---|---|---|---|'
Get-CimInstance Win32_Service |
  Where-Object { $_.PathName -and $_.PathName -notmatch '(?i)^"?C:\\Windows' } |
  Sort-Object Name | ForEach-Object { Add "| $($_.Name) | $($_.State) | $($_.StartMode) | ``$(Mask $_.PathName)`` |" }
Add ''

# ---------------------------------------------------------------- 6. tarefas
Add '## 6. Tarefas agendadas (fora de \Microsoft)'
Add ''
Add '| Caminho | Nome | Estado | Ultima execucao | Resultado | Acao |'
Add '|---|---|---|---|---|---|'
try {
    Get-ScheduledTask -ErrorAction Stop | Where-Object { $_.TaskPath -notmatch '^\\Microsoft' } | Sort-Object TaskPath, TaskName | ForEach-Object {
        $i = $_ | Get-ScheduledTaskInfo -ErrorAction SilentlyContinue
        $ac = ($_.Actions | ForEach-Object { "$($_.Execute) $($_.Arguments)" }) -join ' ; '
        if ($ac.Length -gt 120) { $ac = $ac.Substring(0,120) + '...' }
        Add "| $($_.TaskPath) | $($_.TaskName) | $($_.State) | $($i.LastRunTime) | $($i.LastTaskResult) | ``$(Mask $ac)`` |"
    }
} catch { Add "| _nao foi possivel ler_ | | | | | $($_.Exception.Message -replace '\s+',' ') |" }
Add ''

# ---------------------------------------------------------------- 7. ODBC
Add '## 7. DSNs ODBC'
Add ''
foreach ($plat in '32-bit','64-bit') {
    Add "### $plat"
    Add ''
    Add '| DSN | Tipo | Driver | Server | Database |'
    Add '|---|---|---|---|---|'
    try {
        Get-OdbcDsn -Platform $plat -ErrorAction Stop | Sort-Object Name | ForEach-Object {
            Add "| $($_.Name) | $($_.DsnType) | $($_.DriverName) | $(Mask $_.Attribute['Server']) | $($_.Attribute['Database']) |"
        }
    } catch { Add "| _erro: $($_.Exception.Message -replace '\s+',' ')_ | | | | |" }
    Add ''
    Add "Drivers instalados ($plat):"
    Add ''
    try { (Get-OdbcDriver -Platform $plat -ErrorAction Stop | Sort-Object Name).Name | ForEach-Object { Add "- $_" } } catch { Add '- _nao legivel_' }
    Add ''
}

# ---------------------------------------------------------------- 8. .NET
Add '## 8. .NET Framework instalado'
Add ''
Add '| Chave | Version | Release |'
Add '|---|---|---|'
Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse -ErrorAction SilentlyContinue |
  Get-ItemProperty -Name Version, Release -ErrorAction SilentlyContinue |
  Where-Object { $_.Version } | ForEach-Object {
    Add "| $($_.PSPath -replace '.*NDP\\','') | $($_.Version) | $($_.Release) |"
  }
Add ''
Add 'Runtimes .NET Core / 5+ (se houver):'
Add ''
Add '```'
if (Get-Command dotnet -ErrorAction SilentlyContinue) { dotnet --list-runtimes 2>$null | ForEach-Object { Add $_ } } else { Add 'dotnet CLI nao instalado' }
Add '```'
Add ''

# ---------------------------------------------------------------- 9. certificados
Add '## 9. Certificados da maquina (LocalMachine\My)'
Add ''
Add '| Subject | Emissor | Valido ate | Expirado? | Thumbprint |'
Add '|---|---|---|---|---|'
Get-ChildItem Cert:\LocalMachine\My -ErrorAction SilentlyContinue | Sort-Object NotAfter | ForEach-Object {
    $exp = if ($_.NotAfter -lt (Get-Date)) { '**SIM**' } else { 'nao' }
    Add "| $($_.Subject) | $($_.Issuer) | $($_.NotAfter.ToString('yyyy-MM-dd')) | $exp | $($_.Thumbprint) |"
}
Add ''
Add 'Bindings HTTPS ativos:'
Add ''
Add '```'
(netsh http show sslcert 2>$null) | Where-Object { $_ -match 'IP:port|Hostname:port|Certificate Hash|Nome do host|Hash do Certificado' } | ForEach-Object { Add (Mask $_.Trim()) }
Add '```'
Add ''

# ---------------------------------------------------------------- 10. PATH, shares, rede
Add '## 10. PATH do sistema'
Add ''
Add '```'
([Environment]::GetEnvironmentVariable('Path','Machine') -split ';') | Where-Object { $_ } | ForEach-Object { Add (Mask $_) }
Add '```'
Add ''

Add '## 11. Compartilhamentos e suas permissoes'
Add ''
Add '| Nome | Caminho | ACL NTFS da raiz (grupos amplos) |'
Add '|---|---|---|'
Get-CimInstance Win32_Share | Sort-Object Name | ForEach-Object {
    $acl = ''
    if ($_.Path -and (Test-Path $_.Path)) {
        try {
            $acl = ((Get-Acl $_.Path).Access |
                Where-Object { $_.IdentityReference -match 'Everyone|Todos|Authenticated Users|Usuarios autenticados|BUILTIN\\Users|Usu' } |
                ForEach-Object { "$($_.IdentityReference)=$($_.FileSystemRights)" }) -join '; '
        } catch { $acl = '_nao legivel_' }
    }
    Add "| $($_.Name) | ``$($_.Path)`` | $(if($acl){$acl}else{'-'}) |"
}
Add ''

Add '## 12. Interfaces de rede'
Add ''
Add '| Adaptador | IP | Mascara | Gateway | DNS |'
Add '|---|---|---|---|---|'
Get-CimInstance Win32_NetworkAdapterConfiguration -Filter 'IPEnabled=True' | ForEach-Object {
    Add "| $($_.Description) | $(Mask ($_.IPAddress -join ', ')) | $($_.IPSubnet -join ', ') | $(Mask ($_.DefaultIPGateway -join ', ')) | $(Mask ($_.DNSServerSearchOrder -join ', ')) |"
}
Add ''
Add '## 13. Portas em escuta'
Add ''
Add '| Porta local | Endereco | Processo |'
Add '|---|---|---|'
try {
    Get-NetTCPConnection -State Listen -ErrorAction Stop | Sort-Object LocalPort -Unique | ForEach-Object {
        $p = (Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue).ProcessName
        Add "| $($_.LocalPort) | $(Mask $_.LocalAddress) | $p |"
    }
} catch { Add '| _nao legivel_ | | |' }
Add ''

# ---------------------------------------------------------------- grava
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
$arq = Join-Path $OutDir ("host-" + ($Rotulo -replace '[^\w\.\-]','_') + ".md")
Set-Content -LiteralPath $arq -Value $sb.ToString() -Encoding UTF8

# auditoria de vazamento antes de entregar
$txt = Get-Content $arq -Raw
$sus = [regex]::Matches($txt, '(?im)^.*(senha|password|pwd|secret|apikey|token)\s*[:=]\s*(?!\*\*\*)\S+.*$') |
       Where-Object { $_.Value -notmatch '(?i)publickeytoken|REDIGIDO' }
Write-Host ""
Write-Host "gravado: $arq" -ForegroundColor Green
if ($sus.Count) {
    Write-Host "ATENCAO: $($sus.Count) linha(s) possivelmente com segredo - revise antes de copiar:" -ForegroundColor Yellow
    $sus | Select-Object -First 5 -Expand Value | ForEach-Object { Write-Host "   $_" -ForegroundColor Yellow }
} else {
    Write-Host "auditoria de segredos: nenhuma ocorrencia." -ForegroundColor Green
}
Write-Host "Copie o .md para c:\projetos\tracbel-crm\docs\extracao-vortice\binarios\ - nada mais precisa sair do servidor."
