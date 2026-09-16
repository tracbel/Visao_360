<#
  executar-sanitizacao.ps1 - roda sanitizar-dados-2026-09-15.sql no banco local ou no banco central.
  Documento 38.

      .\scripts\banco\sanitizacao\executar-sanitizacao.ps1 -Onde Local              # simulacao: executa e desfaz
      .\scripts\banco\sanitizacao\executar-sanitizacao.ps1 -Onde Local -Confirmar   # grava
      .\scripts\banco\sanitizacao\executar-sanitizacao.ps1 -Onde Servidor           # simulacao no servidor
      .\scripts\banco\sanitizacao\executar-sanitizacao.ps1 -Onde Servidor -Confirmar

  SEM -Confirmar E SEMPRE SIMULACAO: o SQL roda inteiro dentro de uma transacao, valida e faz ROLLBACK.
  Com -Confirmar, faz COMMIT so se todas as validacoes passarem; qualquer erro desfaz tudo.

  Local:    conteiner tracbel-crm-db, banco TracbelCrm, credencial de infra/.env (nao impressa).
  Servidor: tarefa agendada temporaria (_remoto.ps1); a cadeia de conexao e lida do arquivo da API NO
            SERVIDOR e nao passa por esta estacao. O .sql vai por SMB e e apagado depois.

  O proprio SQL recusa rodar fora do banco TracbelCrm (o arquivo restaurado e as copias de ensaio
  ficam intocados) e antes de conferir que os administradores existem.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [ValidateSet('Local', 'Servidor')] [string] $Onde,
    [switch] $Confirmar
)

$ErrorActionPreference = 'Stop'
$raiz = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$origem = Join-Path $PSScriptRoot 'sanitizar-dados-2026-09-15.sql'
$sql = [IO.File]::ReadAllText($origem)

$marca = 'DECLARE @Confirmar bit = 0;'
if (-not $sql.Contains($marca)) { throw "Nao achei '$marca' no SQL. Nada foi executado." }
if ($Confirmar) { $sql = $sql.Replace($marca, 'DECLARE @Confirmar bit = 1;') }

$modo = if ($Confirmar) { 'GRAVANDO (COMMIT se todas as validacoes passarem)' } else { 'SIMULACAO (ROLLBACK no fim)' }
Write-Host "Sanitizacao em $Onde - $modo" -ForegroundColor Cyan

if ($Onde -eq 'Local') {
    $config = @{}
    foreach ($l in Get-Content (Join-Path $raiz 'infra\.env')) {
        $t = $l.Trim(); if ($t -eq '' -or $t.StartsWith('#')) { continue }
        $p = $t.Split('=', 2); if ($p.Count -eq 2) { $config[$p[0].Trim()] = $p[1].Trim() }
    }
    $temporario = Join-Path ([IO.Path]::GetTempPath()) 'tracbel-sanitizacao.sql'
    [IO.File]::WriteAllText($temporario, $sql, (New-Object Text.UTF8Encoding($false)))
    & docker cp $temporario 'tracbel-crm-db:/tmp/tracbel-sanitizacao.sql' | Out-Null
    & docker exec -e "SQLCMDPASSWORD=$($config.DB_SENHA)" tracbel-crm-db /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -C -b -W -s '|' -d $config.DB_NOME -i /tmp/tracbel-sanitizacao.sql
    $codigo = $LASTEXITCODE
    Remove-Item $temporario -Force -ErrorAction SilentlyContinue
    exit $codigo
}

. (Join-Path $raiz 'scripts\deploy\_remoto.ps1')
$nomeRemoto = 'tracbel-sanitizacao-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.sql'
New-Item -ItemType Directory -Force -Path $Global:RaizPorSmb | Out-Null
$arquivoSmb = Join-Path $Global:RaizPorSmb $nomeRemoto
[IO.File]::WriteAllText($arquivoSmb, $sql, (New-Object Text.UTF8Encoding($false)))

$script = @'
$cfg = Get-Content 'C:\aplicacoes\tracbel-crm\appsettings.Production.json' -Raw | ConvertFrom-Json
$b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
foreach ($parte in ([string]$cfg.ConnectionStrings.Crm).Split(';')) {
  $kv = $parte.Split('=', 2)
  if ($kv.Count -ne 2) { continue }
  $k = $kv[0].Trim().ToLowerInvariant(); $v = $kv[1].Trim()
  if ($k -in 'server','data source','address','addr') { $b['Data Source'] = $v }
  elseif ($k -in 'database','initial catalog') { $b['Initial Catalog'] = $v }
  elseif ($k -in 'user id','uid','user') { $b['User ID'] = $v }
  elseif ($k -in 'password','pwd') { $b['Password'] = $v }
}
$sql = [IO.File]::ReadAllText('__ARQUIVO__')
$cn = New-Object System.Data.SqlClient.SqlConnection $b.ConnectionString
$cn.add_InfoMessage({ param($s, $e) Write-Output ('  mensagem: ' + $e.Message) })
try {
  $cn.Open()
  $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 7200
  $r = $cmd.ExecuteReader()
  try {
    do {
      $n = @(); for ($i = 0; $i -lt $r.FieldCount; $i++) { $n += $r.GetName($i) }
      if ($n.Count -gt 0) { Write-Output ''; Write-Output ('  ' + ($n -join ' | ')) }
      while ($r.Read()) { $v = @(); for ($i = 0; $i -lt $r.FieldCount; $i++) { $v += [string]$r.GetValue($i) }; Write-Output ('  ' + ($v -join ' | ')) }
    } while ($r.NextResult())
  } finally { $r.Close() }
  Write-Output 'EXECUCAO_TERMINOU'
} catch [System.Data.SqlClient.SqlException] {
  Write-Output ('ERRO_SQL ' + $_.Exception.Number + ': ' + $_.Exception.Message)
} finally { $cn.Close() }
'@
$script = $script -replace '__ARQUIVO__', (Join-Path $Global:RaizRemota $nomeRemoto)

try {
    $saida = Invoke-NoServidor -Script $script -Nome 'crm-sanitizacao' -TimeoutSegundos 7200
    $saida
} finally {
    Remove-Item $arquivoSmb -Force -ErrorAction SilentlyContinue
}
if ($saida -notmatch 'EXECUCAO_TERMINOU') { exit 1 }
