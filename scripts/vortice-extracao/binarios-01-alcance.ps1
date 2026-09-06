<#
  binarios-01-alcance.ps1 - Teste de alcance (somente leitura) dos hosts do Vortice CRM.
  Nao usa credenciais. Nao altera nada remoto.
  Saida: CSV + resumo em tela.
  Uso: powershell -ExecutionPolicy Bypass -File .\binarios-01-alcance.ps1 -OutDir 'c:\projetos\tracbel-crm\docs\extracao-vortice\binarios'
#>
[CmdletBinding()]
param(
    [string]$OutDir = (Join-Path $PSScriptRoot '..\..\docs\extracao-vortice\binarios'),
    [int]$TimeoutMs = 1500
)

$hosts = @(
    [pscustomobject]@{ Host='10.150.14.65';   Nome='COLWCRM';       Papel='Banco SQL Server 2019 + FileZilla FTP (Doc Manager) + IIS' }
    [pscustomobject]@{ Host='10.150.6.230';   Nome='agrocrm/VRTCSERVER'; Papel='App server: IIS (CRMWeb, wsVorticeCrmApi), VtcAppMonitorService, C:\Vortice\NT' }
    [pscustomobject]@{ Host='10.150.5.54';    Nome='COLWRDP01';     Papel='Terminal server antigo (dominio coloradomaq)' }
    [pscustomobject]@{ Host='10.150.5.172';   Nome='ECS-ST-TSPLUS'; Papel='Terminal server novo (dominio tracbel)' }
    [pscustomobject]@{ Host='192.168.109.220';Nome='COLWCRM (IP antigo)'; Papel='MORTO - pre migracao de data center' }
    [pscustomobject]@{ Host='192.168.109.209';Nome='app server (IP antigo)'; Papel='MORTO - pre migracao de data center' }
)

$portas = @(
    @{P=21;   S='FTP (Doc Manager)'}
    @{P=80;   S='HTTP/IIS'}
    @{P=135;  S='RPC/WMI'}
    @{P=139;  S='NetBIOS'}
    @{P=443;  S='HTTPS'}
    @{P=445;  S='SMB / share admin c$'}
    @{P=1433; S='SQL Server'}
    @{P=3389; S='RDP'}
    @{P=5985; S='WinRM HTTP'}
    @{P=5986; S='WinRM HTTPS'}
    @{P=8080; S='HTTP alternativo'}
    @{P=14148;S='FileZilla Admin (loopback-only esperado)'}
)

function Test-Porta {
    param([string]$Alvo,[int]$Porta,[int]$Ms)
    $c = New-Object System.Net.Sockets.TcpClient
    try {
        $iar = $c.BeginConnect($Alvo,$Porta,$null,$null)
        if ($iar.AsyncWaitHandle.WaitOne($Ms,$false) -and $c.Connected) { $c.EndConnect($iar); return 'ABERTA' }
        return 'fechada/filtrada'
    } catch { return 'fechada/filtrada' } finally { $c.Close() }
}

$res = foreach ($h in $hosts) {
    $ping = Test-Connection -ComputerName $h.Host -Count 1 -Quiet -ErrorAction SilentlyContinue
    foreach ($p in $portas) {
        $st = Test-Porta -Alvo $h.Host -Porta $p.P -Ms $TimeoutMs
        [pscustomobject]@{
            Host=$h.Host; Nome=$h.Nome; Papel=$h.Papel
            Ping=$(if($ping){'responde'}else{'sem resposta'})
            Porta=$p.P; Servico=$p.S; Status=$st
            Coletado=(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
        }
    }
}

# Shares administrativos e de atualizacao (somente listagem)
$shares = foreach ($h in $hosts | Where-Object { $_.Host -notlike '192.168.*' }) {
    foreach ($s in 'c$','Vortice_Atualiza','admin$') {
        $unc = "\\$($h.Host)\$s"
        $ok = $false; $erro = ''
        try { $null = Get-ChildItem -LiteralPath $unc -ErrorAction Stop; $ok = $true }
        catch { $erro = $_.Exception.Message -replace '\s+',' ' }
        [pscustomobject]@{ Host=$h.Host; Nome=$h.Nome; Share=$unc; Acessivel=$ok; Erro=$erro }
    }
}

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
$res    | Export-Csv (Join-Path $OutDir 'inventario-alcance-portas.csv') -NoTypeInformation -Encoding UTF8
$shares | Export-Csv (Join-Path $OutDir 'inventario-alcance-shares.csv') -NoTypeInformation -Encoding UTF8

$res | Where-Object Status -eq 'ABERTA' | Format-Table Host,Nome,Porta,Servico -AutoSize
"--- portas abertas por host ---"
$res | Group-Object Host | ForEach-Object {
    $ab = ($_.Group | Where-Object Status -eq 'ABERTA' | Select-Object -Expand Porta) -join ', '
    "{0,-16} ping={1,-12} abertas: {2}" -f $_.Name, ($_.Group[0].Ping), $(if($ab){$ab}else{'NENHUMA'})
}
"--- shares ---"
$shares | Format-Table Host,Share,Acessivel -AutoSize

