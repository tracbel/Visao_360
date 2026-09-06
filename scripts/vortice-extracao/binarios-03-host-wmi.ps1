<#
  binarios-03-host-wmi.ps1 - Coleta SOMENTE LEITURA de um host Windows via WMI/DCOM (porta 135)
  + leitura do share administrativo, quando disponivel.

  Nao executa nada no servidor: usa apenas classes Get-* / StdRegProv.EnumKey|GetStringValue,
  que sao operacoes de consulta. Nada e criado, parado ou alterado.

  Uso:
    powershell -ExecutionPolicy Bypass -File .\binarios-03-host-wmi.ps1 -Alvo 10.150.5.172 -OutDir <pasta>
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Alvo,
    [string]$OutDir = '.',
    [string]$Rotulo
)

if (-not $Rotulo) { $Rotulo = $Alvo }
$HKLM = 2147483650
$op = New-CimSessionOption -Protocol Dcom
$s  = New-CimSession -ComputerName $Alvo -SessionOption $op -OperationTimeoutSec 60 -ErrorAction Stop

function Reg-SubKeys { param($Caminho)
    (Invoke-CimMethod -CimSession $s -ClassName StdRegProv -Namespace root\default -MethodName EnumKey `
        -Arguments @{hDefKey=[uint32]$HKLM; sSubKeyName=$Caminho}).sNames
}
function Reg-Str { param($Caminho,$Nome)
    (Invoke-CimMethod -CimSession $s -ClassName StdRegProv -Namespace root\default -MethodName GetStringValue `
        -Arguments @{hDefKey=[uint32]$HKLM; sSubKeyName=$Caminho; sValueName=$Nome}).sValue
}
function Reg-Dword { param($Caminho,$Nome)
    (Invoke-CimMethod -CimSession $s -ClassName StdRegProv -Namespace root\default -MethodName GetDWORDValue `
        -Arguments @{hDefKey=[uint32]$HKLM; sSubKeyName=$Caminho; sValueName=$Nome}).uValue
}
function Reg-Valores { param($Caminho)
    (Invoke-CimMethod -CimSession $s -ClassName StdRegProv -Namespace root\default -MethodName EnumValues `
        -Arguments @{hDefKey=[uint32]$HKLM; sSubKeyName=$Caminho}).sNames
}

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
$sb = New-Object System.Text.StringBuilder
function Add { param($t) [void]$sb.AppendLine($t) }
function Mask { param($t) ($t -replace '((?:10|172|192)\.\d{1,3}\.\d{1,3})\.\d{1,3}','$1.xx') }

Add "# Host $Rotulo - coleta WMI/DCOM (somente leitura)"
Add ""
Add "_Coletado em $(Get-Date -Format 'yyyy-MM-dd HH:mm')._"
Add ""

# ---------- 1. Sistema ----------
$os = Get-CimInstance Win32_OperatingSystem -CimSession $s
$cs = Get-CimInstance Win32_ComputerSystem  -CimSession $s
Add '## 1. Sistema operacional'
Add ''
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

# ---------- 2. Discos ----------
Add '## 2. Volumes'
Add ''
Add '| Unidade | Rotulo | Total GB | Livre GB | % livre |'
Add '|---|---|---:|---:|---:|'
Get-CimInstance Win32_LogicalDisk -CimSession $s -Filter 'DriveType=3' | ForEach-Object {
    $tot=[math]::Round($_.Size/1GB,1); $liv=[math]::Round($_.FreeSpace/1GB,1)
    $pc = if ($_.Size) { [math]::Round(100*$_.FreeSpace/$_.Size,1) } else { 0 }
    Add "| $($_.DeviceID) | $($_.VolumeName) | $tot | $liv | $pc% |"
}
Add ''

# ---------- 3. Servicos ----------
Add '## 3. Servicos Windows (Vortice / Vtc / SQL / IIS / FTP)'
Add ''
Add '| Nome | Estado | Inicio | Conta | Binario |'
Add '|---|---|---|---|---|'
Get-CimInstance Win32_Service -CimSession $s |
  Where-Object { $_.Name -match '(?i)vortice|vortico|vtc|jupiter|filezilla|mssql|sqlserver|w3svc|was|ftpsvc|termservice' -or $_.PathName -match '(?i)vortice|vortico|vtc' } |
  Sort-Object Name | ForEach-Object {
    Add "| $($_.Name) | $($_.State) | $($_.StartMode) | $($_.StartName) | ``$(Mask $_.PathName)`` |"
  }
Add ''
Add '### Todos os servicos com binario fora de C:\Windows'
Add ''
Add '| Nome | Estado | Inicio | Binario |'
Add '|---|---|---|---|'
Get-CimInstance Win32_Service -CimSession $s |
  Where-Object { $_.PathName -and $_.PathName -notmatch '(?i)^"?C:\\Windows' } |
  Sort-Object Name | ForEach-Object { Add "| $($_.Name) | $($_.State) | $($_.StartMode) | ``$(Mask $_.PathName)`` |" }
Add ''

# ---------- 4. .NET Framework ----------
Add '## 4. .NET Framework instalado (HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP)'
Add ''
Add '| Chave | Version | Release |'
Add '|---|---|---|'
$ndp = 'SOFTWARE\Microsoft\NET Framework Setup\NDP'
foreach ($k in (Reg-SubKeys $ndp)) {
    $v = Reg-Str "$ndp\$k" 'Version'
    if ($v) { Add "| $k | $v | $(Reg-Dword "$ndp\$k" 'Release') |" }
    foreach ($k2 in (Reg-SubKeys "$ndp\$k")) {
        $v2 = Reg-Str "$ndp\$k\$k2" 'Version'
        if ($v2) { Add "| $k\$k2 | $v2 | $(Reg-Dword "$ndp\$k\$k2" 'Release') |" }
    }
}
Add ''

# ---------- 5. ODBC ----------
Add '## 5. DSNs ODBC'
Add ''
foreach ($par in @(@{T='64 bits';P='SOFTWARE\ODBC\ODBC.INI'}, @{T='32 bits (WOW6432Node)';P='SOFTWARE\WOW6432Node\ODBC\ODBC.INI'})) {
    Add "### $($par.T)"
    Add ''
    Add '| DSN | Driver | Server | Database |'
    Add '|---|---|---|---|'
    foreach ($dsn in (Reg-SubKeys $par.P)) {
        if ($dsn -eq 'ODBC Data Sources') { continue }
        $drv = Reg-Str "$($par.P)\$dsn" 'Driver'
        $srv = Reg-Str "$($par.P)\$dsn" 'Server'
        $db  = Reg-Str "$($par.P)\$dsn" 'Database'
        Add "| $dsn | $drv | $(Mask $srv) | $db |"
    }
    Add ''
    Add "Drivers instalados ($($par.T)):"
    Add ''
    $dp = $par.P -replace 'ODBC\.INI','ODBCINST.INI'
    Add ('- ' + (((Reg-SubKeys $dp) | Where-Object { $_ -ne 'ODBC Drivers' }) -join "`n- "))
    Add ''
}

# ---------- 6. RemoteApp ----------
Add '## 6. RemoteApp publicados (TSAppAllowList)'
Add ''
$tsa = 'SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList'
Add "fDisabledAllowList = $(Reg-Dword $tsa 'fDisabledAllowList')"
Add ''
Add '| Chave (alias) | Name | Caminho | ShortPath |'
Add '|---|---|---|---|'
foreach ($a in (Reg-SubKeys "$tsa\Applications")) {
    Add "| $a | $(Reg-Str "$tsa\Applications\$a" 'Name') | ``$(Mask (Reg-Str "$tsa\Applications\$a" 'Path'))`` | $(Reg-Str "$tsa\Applications\$a" 'ShortPath') |"
}
Add ''

# ---------- 7. PATH do sistema ----------
Add '## 7. PATH do sistema'
Add ''
Add '```'
$path = Reg-Str 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment' 'Path'
if ($path) { ($path -split ';') | Where-Object { $_ } | ForEach-Object { Add (Mask $_) } }
Add '```'
Add ''

# ---------- 8. Software instalado ----------
Add '## 8. Software instalado (Uninstall)'
Add ''
Add '| Produto | Versao | Fabricante | Bits |'
Add '|---|---|---|---|'
foreach ($par in @(@{B='64';P='SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall'}, @{B='32';P='SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall'})) {
    foreach ($k in (Reg-SubKeys $par.P)) {
        $n = Reg-Str "$($par.P)\$k" 'DisplayName'
        if (-not $n) { continue }
        Add "| $n | $(Reg-Str "$($par.P)\$k" 'DisplayVersion') | $(Reg-Str "$($par.P)\$k" 'Publisher') | $($par.B) |"
    }
}
Add ''

# ---------- 9. Compartilhamentos ----------
Add '## 9. Compartilhamentos'
Add ''
Add '| Nome | Caminho | Descricao |'
Add '|---|---|---|'
Get-CimInstance Win32_Share -CimSession $s | Sort-Object Name | ForEach-Object { Add "| $($_.Name) | ``$($_.Path)`` | $($_.Description) |" }
Add ''

# ---------- 10. Rede ----------
Add '## 10. Interfaces de rede com IP'
Add ''
Add '| Adaptador | IP | Mascara | Gateway | DNS |'
Add '|---|---|---|---|---|'
Get-CimInstance Win32_NetworkAdapterConfiguration -CimSession $s -Filter 'IPEnabled=True' | ForEach-Object {
    Add "| $($_.Description) | $(Mask ($_.IPAddress -join ', ')) | $($_.IPSubnet -join ', ') | $(Mask ($_.DefaultIPGateway -join ', ')) | $(Mask ($_.DNSServerSearchOrder -join ', ')) |"
}
Add ''

Remove-CimSession $s
$arq = Join-Path $OutDir ("host-" + ($Rotulo -replace '[^\w\.\-]','_') + ".md")
Set-Content -LiteralPath $arq -Value $sb.ToString() -Encoding UTF8
"gravado: $arq"
