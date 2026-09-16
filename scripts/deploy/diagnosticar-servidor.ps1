<#
  diagnosticar-servidor.ps1 - levanta, desta estacao, o ambiente REAL da VM do servidor. So leitura.
  Documento 35, secao 12.

      .\scripts\deploy\diagnosticar-servidor.ps1

  NAO PARA, NAO REINICIA, NAO INSTALA E NAO REMOVE NADA. Roda no servidor por tarefa agendada
  temporaria (_remoto.ps1) com a conta SYSTEM - a mesma do servico TracbelCrmSincronizacaoArt -, e
  por isso tambem prova que essa conta le as configuracoes.

  O QUE SAI:
    - maquina, Windows, CPU, memoria, disco e sinais de virtualizacao;
    - recursos do Windows para conteiner e virtualizacao (Hyper-V, Containers, WSL) e se o Hyper-V
      consegue de fato rodar maquina virtual dentro desta VM;
    - servicos da aplicacao, da integracao, do SQL Server e de Docker/containerd/WSL;
    - recuperacao de falha dos servicos e sessoes de usuario abertas (so contagem);
    - Docker em tres camadas separadas: arquivos instalados, mecanismo em execucao, contextos e
      conteineres;
    - destino de banco das configuracoes (servidor e banco, nunca usuario ou senha);
    - instancia, edicao, sistema operacional e caminhos do SQL Server; arquivos do banco; backups;
    - ultima sincronizacao bem-sucedida e ultimas execucoes;
    - onde ficam logs e backups.

  ASCII puro: o PowerShell 5.1 do servidor le .ps1 como ANSI.
#>

[CmdletBinding()]
param(
    [string] $ConfigDaApi = 'C:\aplicacoes\tracbel-crm\appsettings.Production.json',
    [string] $ConfigDaSincronizacao = 'C:\aplicacoes\tracbel-crm-sincronizacao\appsettings.Production.json'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

$script = @'
$ErrorActionPreference = 'Continue'
function Secao([string] $t) { Write-Output ''; Write-Output ('== ' + $t) }

function Rodar([string] $exe, [string] $argumentos, [int] $segundos = 30) {
  $o = [IO.Path]::GetTempFileName(); $e = [IO.Path]::GetTempFileName()
  try {
    $p = Start-Process -FilePath $exe -ArgumentList $argumentos -NoNewWindow -PassThru -RedirectStandardOutput $o -RedirectStandardError $e
    $null = $p.Handle
    if (-not $p.WaitForExit($segundos * 1000)) { try { $p.Kill() } catch {}; return ('  (sem resposta em ' + $segundos + ' s)') }
    $p.WaitForExit()
    $txt = ([string](Get-Content $o -Raw) + [string](Get-Content $e -Raw)) -replace "`0", ''
    $linhas = @($txt -split "`r?`n" | Where-Object { $_.Trim() -ne '' } | ForEach-Object { '    ' + $_ })
    if ($linhas.Count -eq 0) { $linhas = @('    (sem saida)') }
    return (@('  codigo de saida ' + $p.ExitCode) + $linhas) -join "`n"
  } catch { return ('  (nao executou: ' + $_.Exception.Message + ')') }
  finally { Remove-Item $o, $e -ErrorAction SilentlyContinue }
}

function LerDestino([string] $arquivo) {
  $j = Get-Content $arquivo -Raw -ErrorAction Stop | ConvertFrom-Json
  $b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
  foreach ($parte in ([string]$j.ConnectionStrings.Crm).Split(';')) {
    $kv = $parte.Split('=', 2)
    if ($kv.Count -ne 2) { continue }
    $k = $kv[0].Trim().ToLowerInvariant(); $v = $kv[1].Trim()
    if ($k -in 'server','data source','address','addr') { $b['Data Source'] = $v }
    elseif ($k -in 'database','initial catalog') { $b['Initial Catalog'] = $v }
    elseif ($k -in 'user id','uid','user') { $b['User ID'] = $v }
    elseif ($k -in 'password','pwd') { $b['Password'] = $v }
    elseif ($k -in 'integrated security','trusted_connection') { $b['Integrated Security'] = $v }
  }
  return @{ Json = $j; Builder = $b; Cadeia = [string]$j.ConnectionStrings.Crm }
}

# -------------------------------------------------------------------------------------------------
Secao 'MAQUINA E WINDOWS'
$cs = Get-CimInstance Win32_ComputerSystem
$os = Get-CimInstance Win32_OperatingSystem
$bios = Get-CimInstance Win32_BIOS
$cv = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
$cpu = @(Get-CimInstance Win32_Processor)
Write-Output ('  nome: ' + $env:COMPUTERNAME + ' | dominio ou grupo: ' + $cs.Domain + ' | membro de dominio: ' + $cs.PartOfDomain)
Write-Output ('  fabricante e modelo: ' + $cs.Manufacturer + ' / ' + $cs.Model + ' | hypervisor presente: ' + $cs.HypervisorPresent)
Write-Output ('  BIOS: ' + $bios.Manufacturer + ' | ' + $bios.SMBIOSBIOSVersion)
Write-Output ('  Windows: ' + $os.Caption + ' | versao ' + $os.Version + ' | build ' + $cv.CurrentBuild + '.' + $cv.UBR + ' | DisplayVersion ' + $cv.DisplayVersion + ' | EditionID ' + $cv.EditionID + ' | InstallationType ' + $cv.InstallationType + ' | ' + $os.OSArchitecture)
Write-Output ('  ultimo boot: ' + $os.LastBootUpTime.ToString('yyyy-MM-dd HH:mm:ss') + ' | agora: ' + (Get-Date).ToString('yyyy-MM-dd HH:mm:ss') + ' | fuso: ' + (Get-TimeZone).Id)
Write-Output ('  CPU: ' + $cpu[0].Name + ' | processadores ' + $cpu.Count + ' | nucleos ' + ($cpu | Measure-Object NumberOfCores -Sum).Sum + ' | logicos ' + $cs.NumberOfLogicalProcessors)
Write-Output ('  extensoes de virtualizacao visiveis dentro da VM: VMMonitorModeExtensions=' + $cpu[0].VMMonitorModeExtensions + ' | VirtualizationFirmwareEnabled=' + $cpu[0].VirtualizationFirmwareEnabled)
Write-Output ('  memoria: ' + [math]::Round($cs.TotalPhysicalMemory / 1GB, 1) + ' GB total | ' + [math]::Round($os.FreePhysicalMemory * 1KB / 1GB, 1) + ' GB livres agora')
foreach ($d in Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3') {
  Write-Output ('  disco ' + $d.DeviceID + ' ' + [math]::Round($d.Size / 1GB, 1) + ' GB | livre ' + [math]::Round($d.FreeSpace / 1GB, 1) + ' GB')
}

# -------------------------------------------------------------------------------------------------
Secao 'RECURSOS DO WINDOWS PARA CONTEINER E VIRTUALIZACAO'
foreach ($f in 'Microsoft-Hyper-V', 'Containers', 'VirtualMachinePlatform', 'Microsoft-Windows-Subsystem-Linux') {
  try {
    $r = Get-WindowsOptionalFeature -Online -FeatureName $f -ErrorAction Stop
    if ($r) { Write-Output ('  ' + $f + ': ' + $r.State) } else { Write-Output ('  ' + $f + ': nao existe nesta edicao') }
  } catch { Write-Output ('  ' + $f + ': nao consultado (' + $_.Exception.Message.Split([char]10)[0] + ')') }
}
Write-Output '  -- o Hyper-V consegue rodar maquina virtual aqui dentro?'
try { Write-Output ('  maquinas virtuais do Hyper-V nesta VM: ' + @(Get-VM -ErrorAction Stop).Count) } catch { Write-Output ('  Get-VM: ' + $_.Exception.Message.Split([char]10)[0]) }
Write-Output (Rodar 'bcdedit.exe' '/enum {current}' 15) -split "`n" | Where-Object { $_ -match 'hypervisorlaunchtype|codigo de saida' }
try {
  $ev = Get-WinEvent -LogName 'Microsoft-Windows-Hyper-V-Hypervisor-Operational' -MaxEvents 3 -ErrorAction Stop
  foreach ($x in $ev) { Write-Output ('  evento do hypervisor ' + $x.TimeCreated.ToString('yyyy-MM-dd HH:mm') + ' id ' + $x.Id + ': ' + (($x.Message -replace '\s+', ' ') -replace '^(.{0,220}).*$', '$1')) }
} catch { Write-Output '  sem eventos no log Hyper-V-Hypervisor-Operational' }
try {
  $ev = Get-WinEvent -FilterHashtable @{ LogName = 'System'; ProviderName = 'Microsoft-Windows-Hyper-V-Hypervisor' } -MaxEvents 3 -ErrorAction Stop
  foreach ($x in $ev) { Write-Output ('  evento de sistema do hypervisor ' + $x.TimeCreated.ToString('yyyy-MM-dd HH:mm') + ' id ' + $x.Id + ': ' + (($x.Message -replace '\s+', ' ') -replace '^(.{0,220}).*$', '$1')) }
} catch { Write-Output '  sem eventos do provedor Hyper-V-Hypervisor no log de Sistema' }

# -------------------------------------------------------------------------------------------------
Secao 'SERVICOS DA APLICACAO, INTEGRACAO, SQL SERVER E CONTEINER'
$servicos = Get-CimInstance Win32_Service | Where-Object {
  $_.Name -like 'Tracbel*' -or $_.Name -like 'MSSQL*' -or $_.Name -like 'SQL*' -or
  $_.Name -match 'docker|containerd|vmcompute|^hns$|WSLService|LxssManager|^vmms$'
}
if (-not $servicos) { Write-Output '  nenhum' }
foreach ($s in $servicos | Sort-Object Name) {
  $atrasado = (Get-ItemProperty ('HKLM:\SYSTEM\CurrentControlSet\Services\' + $s.Name) -Name DelayedAutostart -ErrorAction SilentlyContinue).DelayedAutostart
  $sessao = '-'; $desde = '-'
  if ($s.ProcessId -gt 0) {
    $p = Get-CimInstance Win32_Process -Filter ('ProcessId=' + $s.ProcessId)
    if ($p) { $sessao = $p.SessionId; $desde = $p.CreationDate.ToString('yyyy-MM-dd HH:mm:ss') }
  }
  $modo = [string]$s.StartMode
  if ($atrasado -eq 1) { $modo = $modo + ' (atrasado)' }
  Write-Output ('  ' + $s.Name + ' | ' + $s.State + ' | inicio ' + $modo + ' | conta ' + $s.StartName + ' | PID ' + $s.ProcessId + ' | sessao ' + $sessao + ' | processo desde ' + $desde)
  Write-Output ('      ' + $s.PathName)
  if ($s.Name -eq 'TracbelVerificacao') {
    $app = (Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\TracbelVerificacao\Parameters' -ErrorAction SilentlyContinue)
    if ($app) { Write-Output ('      NSSM executa: ' + $app.Application + ' ' + $app.AppParameters + ' | pasta ' + $app.AppDirectory) }
    Write-Output ('      descricao: ' + $s.Description)
  }
}

Secao 'RECUPERACAO DE FALHA'
foreach ($n in 'TracbelCrmSincronizacaoArt', 'TracbelCrmApi', 'MSSQLSERVER') {
  Write-Output ('  -- ' + $n)
  Write-Output ((Rodar 'sc.exe' ('qfailure ' + $n) 15) -split "`n" | Where-Object { $_ -match 'RESET_PERIOD|FAILURE_ACTIONS|RESTART|codigo' })
  Write-Output ((Rodar 'sc.exe' ('qfailureflag ' + $n) 15) -split "`n" | Where-Object { $_ -match 'NONCRASH' })
}

Secao 'SESSOES DE USUARIO NA VM (so contagem e estado)'
$q = Rodar 'query.exe' 'user' 15
$abertas = @($q -split "`n" | Where-Object { $_ -match '\s(Ativo|Active|Disco\w*|Disc\w*)\s' })
Write-Output ('  sessoes de usuario abertas agora: ' + $abertas.Count)
foreach ($l in $abertas) { if ($l -match '\s(Ativo|Active|Disco\w*|Disc\w*)\s') { Write-Output ('  estado de uma sessao: ' + $Matches[1]) } }

# -------------------------------------------------------------------------------------------------
Secao 'DOCKER 1/3: O QUE ESTA INSTALADO (arquivos e programas)'
$caminhos = 'C:\Program Files\Docker\Docker\Docker Desktop.exe',
            'C:\Program Files\Docker\Docker\resources\bin\docker.exe',
            'C:\Program Files\DockerEngine\docker\dockerd.exe',
            'C:\Program Files\DockerEngine\docker\docker.exe',
            'C:\ProgramData\DockerDesktop',
            'C:\ProgramData\docker'
foreach ($c in $caminhos) {
  if (Test-Path $c) {
    $i = Get-Item $c; $v = ''
    if (-not $i.PSIsContainer) { $v = ' | versao ' + $i.VersionInfo.ProductVersion + ' | gravado em ' + $i.LastWriteTime.ToString('yyyy-MM-dd') }
    else { $v = ' | pasta criada em ' + $i.CreationTime.ToString('yyyy-MM-dd') + ' | ' + [math]::Round((Get-ChildItem $c -Recurse -File -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum / 1MB, 1) + ' MB' }
    Write-Output ('  EXISTE     ' + $c + $v)
  } else { Write-Output ('  nao existe ' + $c) }
}
foreach ($cmd in 'docker', 'dockerd', 'wsl') {
  $g = @(Get-Command $cmd -All -ErrorAction SilentlyContinue)
  if ($g.Count) { Write-Output ('  no PATH: ' + $cmd + ' -> ' + (($g | ForEach-Object Source) -join ' ; ')) } else { Write-Output ('  no PATH: ' + $cmd + ' -> nao encontrado') }
}
Write-Output ('  DOCKER_HOST da maquina: ' + $(if ([Environment]::GetEnvironmentVariable('DOCKER_HOST', 'Machine')) { [Environment]::GetEnvironmentVariable('DOCKER_HOST', 'Machine') } else { '(vazio)' }))
foreach ($hive in Get-ChildItem 'Registry::HKEY_USERS' -ErrorAction SilentlyContinue | Where-Object { $_.PSChildName -match '^S-1-5-21-[\d-]+$' }) {
  $dh = (Get-ItemProperty ('Registry::HKEY_USERS\' + $hive.PSChildName + '\Environment') -Name DOCKER_HOST -ErrorAction SilentlyContinue).DOCKER_HOST
  Write-Output ('  DOCKER_HOST de usuario carregado ' + $hive.PSChildName.Substring($hive.PSChildName.Length - 4) + ': ' + $(if ($dh) { $dh } else { '(vazio)' }))
}
$programas = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*', 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
  Where-Object { $_.DisplayName -match 'Docker|Windows Subsystem for Linux|^WSL' }
if ($programas) { foreach ($r in $programas) { Write-Output ('  programa instalado: ' + $r.DisplayName + ' ' + $r.DisplayVersion) } }
else { Write-Output '  nenhum programa Docker ou WSL na lista de programas instalados' }
$svcDocker = Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\docker' -ErrorAction SilentlyContinue
if ($svcDocker) { Write-Output ('  servico docker registrado com: ' + $svcDocker.ImagePath) }
if (Test-Path 'C:\ProgramData\docker\config\daemon.json') { Write-Output ('  daemon.json do Docker Engine: ' + ((Get-Content 'C:\ProgramData\docker\config\daemon.json' -Raw) -replace '\s+', ' ')) } else { Write-Output '  daemon.json do Docker Engine: nao existe (configuracao padrao)' }

Secao 'DOCKER 2/3: MECANISMO EM EXECUCAO (processos e pipes)'
$procs = Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match 'docker|containerd|vmmem|wsl|com\.docker' }
if ($procs) { foreach ($p in $procs) { Write-Output ('  processo ' + $p.ProcessName + ' | PID ' + $p.Id + ' | sessao ' + $p.SessionId + ' | memoria ' + [math]::Round($p.WorkingSet64 / 1MB, 0) + ' MB') } }
else { Write-Output '  nenhum processo de Docker, containerd, WSL ou vmmem em execucao' }
$pipes = @([IO.Directory]::GetFiles('\\.\pipe\') | Where-Object { $_ -match 'docker' })
if ($pipes.Count) { $pipes | ForEach-Object { Write-Output ('  pipe aberto: ' + $_) } }
else { Write-Output '  nenhum named pipe do Docker aberto' }

Secao 'DOCKER 3/3: CONTEXTOS POR PERFIL E O QUE O CLIENTE ALCANCA'
$perfis = @(Get-ChildItem 'C:\Users' -Directory -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }) + 'C:\Windows\System32\config\systemprofile'
foreach ($perfil in $perfis) {
  $dir = Join-Path $perfil '.docker'
  if (-not (Test-Path $dir)) { continue }
  $atual = ''
  try { $atual = (Get-Content (Join-Path $dir 'config.json') -Raw -ErrorAction Stop | ConvertFrom-Json).currentContext } catch {}
  Write-Output ('  perfil ' + (Split-Path $perfil -Leaf) + ': contexto gravado = ' + $(if ($atual) { $atual } else { '(padrao)' }))
  foreach ($m in Get-ChildItem (Join-Path $dir 'contexts\meta') -Recurse -Filter meta.json -ErrorAction SilentlyContinue) {
    try { $j = Get-Content $m.FullName -Raw | ConvertFrom-Json; Write-Output ('    contexto ' + $j.Name + ' -> ' + $j.Endpoints.docker.Host) } catch {}
  }
}
$docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
if (Test-Path $docker) {
  Write-Output ('  cliente usado (como SYSTEM): ' + $docker)
  Write-Output '  -- docker context ls'; Write-Output (Rodar $docker 'context ls' 30)
  Write-Output '  -- docker info (pelo DOCKER_HOST da maquina)'; Write-Output (Rodar $docker 'info --format "servidor={{.ServerVersion}} nome={{.Name}} so={{.OperatingSystem}} tipo={{.OSType}} isolamento={{.Isolation}} raiz={{.DockerRootDir}} conteineres={{.Containers}} rodando={{.ContainersRunning}} imagens={{.Images}}"' 45)
  Write-Output '  -- docker ps -a';  Write-Output (Rodar $docker 'ps -a --format "{{.Names}}|{{.Image}}|{{.Status}}|{{.Ports}}"' 45)
  Write-Output '  -- docker images'; Write-Output (Rodar $docker 'images --format "{{.Repository}}:{{.Tag}}|{{.Size}}"' 45)
  Write-Output '  -- docker volume ls'; Write-Output (Rodar $docker 'volume ls' 45)
  Write-Output '  -- motor do Docker Desktop (contexto desktop-linux)'; Write-Output (Rodar $docker '--context desktop-linux info --format "{{.ServerVersion}}"' 30)
}

# -------------------------------------------------------------------------------------------------
Secao 'CONFIGURACOES LIDAS PELA CONTA SYSTEM (a mesma do servico de sincronizacao)'
$api = $null; $sinc = $null
try {
  $api = LerDestino '__CONFIG_API__'
  Write-Output ('  API: servidor=' + $api.Builder.DataSource + ' | banco=' + $api.Builder.InitialCatalog + ' | autenticacao=' + $(if ($api.Builder.IntegratedSecurity) { 'Windows' } else { 'login SQL' }) + ' | secoes: ' + (($api.Json.PSObject.Properties | ForEach-Object Name) -join ', '))
} catch { Write-Output ('  API: nao li o arquivo (' + $_.Exception.Message + ')') }
try {
  $sinc = LerDestino '__CONFIG_SINC__'
  $a = $sinc.Json.Sincronizacao.Art
  Write-Output ('  sincronizacao: servidor=' + $sinc.Builder.DataSource + ' | banco=' + $sinc.Builder.InitialCatalog + ' | secoes: ' + (($sinc.Json.PSObject.Properties | ForEach-Object Name) -join ', '))
  Write-Output ('  sincronizacao: Habilitada=' + $a.Habilitada + ' | IntervaloMinutos=' + $a.IntervaloMinutos + ' | Tentativas=' + $a.Tentativas + ' | chaves do ART preenchidas: ' + (($sinc.Json.Art.PSObject.Properties | Where-Object { $_.Value } | ForEach-Object Name) -join ', ') + ' | ProtheusBanco presente: ' + [bool]$sinc.Json.ProtheusBanco)
  if ($api) { Write-Output ('  a sincronizacao usa exatamente a mesma cadeia de conexao da API: ' + ($api.Cadeia -ceq $sinc.Cadeia)) }
  $acl = Get-Acl '__CONFIG_SINC__'
  Write-Output ('  permissao do arquivo da sincronizacao: heranca desligada=' + $acl.AreAccessRulesProtected + ' | ' + (($acl.Access | ForEach-Object { [string]$_.IdentityReference + ' ' + $_.FileSystemRights }) -join '; '))
} catch { Write-Output ('  sincronizacao: nao li o arquivo (' + $_.Exception.Message + ')') }

# -------------------------------------------------------------------------------------------------
Secao 'SQL SERVER (consultado com a conta da aplicacao)'
$dirBackup = $null; $errorLog = $null
if ($api) {
  $cn = New-Object System.Data.SqlClient.SqlConnection $api.Builder.ConnectionString
  function Tabela([string] $titulo, [string] $sql) {
    Write-Output ''
    Write-Output ('  ' + $titulo)
    $leitor = $null
    try {
      $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 120
      $leitor = $cmd.ExecuteReader()
      $nomes = @(); for ($i = 0; $i -lt $leitor.FieldCount; $i++) { $nomes += $leitor.GetName($i) }
      Write-Output ('    ' + ($nomes -join ' | '))
      while ($leitor.Read()) {
        $valores = @(); for ($i = 0; $i -lt $leitor.FieldCount; $i++) { $valores += [string]$leitor.GetValue($i) }
        Write-Output ('    ' + ($valores -join ' | '))
      }
    } catch { Write-Output ('    sem acesso ou erro: ' + $(if ($_.Exception.InnerException -is [System.Data.SqlClient.SqlException]) { 'SQL ' + $_.Exception.InnerException.Number } elseif ($_.Exception -is [System.Data.SqlClient.SqlException]) { 'SQL ' + $_.Exception.Number } else { $_.Exception.GetType().Name })) }
    finally { if ($leitor) { $leitor.Close() } }
  }
  try {
    $cn.Open()
    Tabela 'INSTANCIA' "SELECT @@SERVERNAME AS Servidor, SERVERPROPERTY('MachineName') AS Maquina, ISNULL(CAST(SERVERPROPERTY('InstanceName') AS nvarchar(100)), 'MSSQLSERVER (instancia padrao)') AS Instancia, SERVERPROPERTY('Edition') AS Edicao, SERVERPROPERTY('ProductVersion') AS Versao, DB_NAME() AS BancoDaAplicacao"
    Tabela 'SISTEMA ONDE O SQL SERVER EXECUTA' "SELECT host_platform, host_distribution, host_release FROM sys.dm_os_host_info"
    Tabela 'CAMINHOS PADRAO DA INSTANCIA' "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(400)) AS PastaDeDados, CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(400)) AS PastaDeLogDeTransacao, CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400)) AS PastaDeBackup, CAST(SERVERPROPERTY('ErrorLogFileName') AS nvarchar(400)) AS ArquivoDeErrorLog"
    Tabela 'ARQUIVOS DO BANCO DA APLICACAO' "SELECT name, type_desc, physical_name, CAST(size * 8 / 1024.0 AS decimal(10,1)) AS MB FROM sys.database_files"
    Tabela 'BANCO' "SELECT name, recovery_model_desc, collation_name, CONVERT(varchar(19), create_date, 120) AS criado FROM sys.databases WHERE name = DB_NAME()"
    Tabela 'BACKUPS REGISTRADOS NO MSDB' "SELECT TOP 5 CONVERT(varchar(19), b.backup_finish_date, 120) AS fim, b.type, b.is_copy_only, CAST(b.backup_size / 1048576.0 AS decimal(10,1)) AS MB, m.physical_device_name FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id = b.media_set_id WHERE b.database_name = DB_NAME() ORDER BY b.backup_finish_date DESC"
    Tabela 'SINCRONIZACAO DO ART: ULTIMO SUCESSO' "SELECT TOP 1 FORMAT(IniciadaEm, 'yyyy-MM-dd HH:mm:ss') AS IniciadaUtc, FORMAT(TerminadaEm, 'yyyy-MM-dd HH:mm:ss') AS TerminadaUtc, Resultado, Tentativas, RegistrosLidos, Incluidos, Atualizados, Pendentes, Maquina, LEFT(Mensagem, 200) AS Mensagem FROM integracao.ExecucaoDeSincronizacao WHERE Resultado = 'Sucesso' ORDER BY TerminadaEm DESC"
    Tabela 'SINCRONIZACAO DO ART: ULTIMAS EXECUCOES' "SELECT TOP 5 FORMAT(IniciadaEm, 'yyyy-MM-dd HH:mm:ss') AS IniciadaUtc, FORMAT(TerminadaEm, 'HH:mm:ss') AS FimUtc, Resultado, Tentativas, RegistrosLidos AS Lidos, Incluidos, Atualizados, Pendentes, Maquina FROM integracao.ExecucaoDeSincronizacao ORDER BY IniciadaEm DESC"
    Tabela 'PONTO DE SINCRONISMO' "SELECT p.Fluxo, p.UltimoValor, FORMAT(p.ProcessadoEm, 'yyyy-MM-dd HH:mm:ss') AS ProcessadoUtc, p.RegistrosLidos, p.RegistrosGravados, p.RegistrosErro FROM integracao.PontoDeSincronismo p JOIN integracao.Sistema s ON s.Id = p.SistemaId WHERE s.Codigo = 'ART'"
    $cmd = $cn.CreateCommand(); $cmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000)), CAST(SERVERPROPERTY('ErrorLogFileName') AS nvarchar(4000))"
    $leitor = $cmd.ExecuteReader()
    try { if ($leitor.Read()) { $dirBackup = [string]$leitor.GetValue(0); $errorLog = [string]$leitor.GetValue(1) } } finally { $leitor.Close() }
  } catch { Write-Output ('  falha na consulta: ' + $_.Exception.GetType().Name) }
  finally { $cn.Close() }
}

# -------------------------------------------------------------------------------------------------
Secao 'ONDE FICAM BACKUPS E LOGS NO DISCO'
if ($dirBackup -and (Test-Path $dirBackup)) {
  $baks = @(Get-ChildItem $dirBackup -Filter *.bak -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending)
  Write-Output ('  backups em ' + $dirBackup + ': ' + $baks.Count + ' arquivo(s), ' + [math]::Round(($baks | Measure-Object Length -Sum).Sum / 1GB, 2) + ' GB')
  foreach ($b in $baks | Select-Object -First 6) { Write-Output ('    ' + $b.LastWriteTime.ToString('yyyy-MM-dd HH:mm') + ' | ' + [math]::Round($b.Length / 1MB, 1) + ' MB | ' + $b.Name) }
} else { Write-Output '  pasta de backup nao identificada' }
if ($errorLog) { $dirLog = Split-Path $errorLog; Write-Output ('  log do SQL Server: ' + $dirLog + ' (' + @(Get-ChildItem $dirLog -Filter 'ERRORLOG*' -ErrorAction SilentlyContinue).Count + ' arquivos ERRORLOG)') }
foreach ($pasta in 'C:\aplicacoes\tracbel-crm\logs', 'C:\aplicacoes\tracbel-crm-sincronizacao\logs') {
  if (Test-Path $pasta) {
    $arqs = @(Get-ChildItem $pasta -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending)
    Write-Output ('  ' + $pasta + ': ' + $arqs.Count + ' arquivo(s)' + $(if ($arqs.Count) { ', mais recente ' + $arqs[0].Name + ' em ' + $arqs[0].LastWriteTime.ToString('yyyy-MM-dd HH:mm') } else { '' }))
  } else { Write-Output ('  ' + $pasta + ': nao existe') }
}
Write-Output '  Log de Aplicativo do Windows, origens do projeto e do SQL Server nos ultimos 3000 eventos:'
Get-EventLog -LogName Application -Newest 3000 -ErrorAction SilentlyContinue |
  Where-Object { $_.Source -match 'Tracbel|MSSQLSERVER|^\.NET Runtime$|ASP\.NET|docker' } |
  Group-Object Source | ForEach-Object { Write-Output ('    ' + $_.Name + ': ' + $_.Count + ' evento(s), o mais recente em ' + ($_.Group | Sort-Object TimeGenerated -Descending | Select-Object -First 1).TimeGenerated.ToString('yyyy-MM-dd HH:mm:ss')) }
Write-Output '  ultimas entradas da origem TracbelCrmSincronizacaoArt:'
foreach ($e in Get-EventLog -LogName Application -Source 'TracbelCrmSincronizacaoArt' -Newest 6 -ErrorAction SilentlyContinue) {
  $m = ($e.Message -replace '\s+', ' '); if ($m.Length -gt 260) { $m = $m.Substring(0, 260) + '...' }
  Write-Output ('    ' + $e.TimeGenerated.ToString('yyyy-MM-dd HH:mm:ss') + ' ' + $e.EntryType + ' ' + $m)
}
'@

$script = $script -replace '__CONFIG_API__', $ConfigDaApi -replace '__CONFIG_SINC__', $ConfigDaSincronizacao
Invoke-NoServidor -Script $script -Nome 'crm-diagnostico' -TimeoutSegundos 600
