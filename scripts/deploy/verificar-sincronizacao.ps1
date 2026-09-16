<#
  verificar-sincronizacao.ps1 - mostra, desta estacao, o que o servico de sincronizacao do ART fez
  no servidor. So leitura. Documento 35, secao 11.

      .\scripts\deploy\verificar-sincronizacao.ps1

  O QUE SAI:
    - estado e tipo de inicio do servico TracbelCrmSincronizacaoArt;
    - as ultimas entradas dele no Log de Aplicativo do Windows;
    - as ultimas execucoes em integracao.ExecucaoDeSincronizacao e o ponto de sincronismo;
    - contagens do banco central (vendas, vinculos, maquinas do ART, fila, divergencias) e a
      conferencia de duplicidade (venda por codigo da origem, chassi).

  Roda no servidor por tarefa agendada temporaria (_remoto.ps1): a cadeia de conexao e lida do
  arquivo da API NO SERVIDOR e nao passa por esta estacao. Nada de credencial aparece na saida.
  Mesmas consultas podem ser coladas no DBeaver do servidor.
#>

[CmdletBinding()]
param(
    [string] $Servico = 'TracbelCrmSincronizacaoArt',
    [string] $ConfigDaApi = 'C:\aplicacoes\tracbel-crm\appsettings.Production.json'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

$script = @'
$svc = Get-Service -Name '__SERVICO__' -ErrorAction SilentlyContinue
if ($svc) { Write-Output ('SERVICO  {0}  inicio {1}' -f $svc.Status, $svc.StartType) } else { Write-Output 'SERVICO  nao registrado' }

Write-Output ''
Write-Output 'LOG DE APLICATIVO (mais recentes primeiro)'
try {
  foreach ($e in (Get-EventLog -LogName Application -Source '__SERVICO__' -Newest 10 -ErrorAction Stop)) {
    $m = ($e.Message -replace '\s+', ' ')
    if ($m.Length -gt 400) { $m = $m.Substring(0, 400) + '...' }
    Write-Output ('  {0:yyyy-MM-dd HH:mm:ss}  {1,-11} {2}' -f $e.TimeGenerated, $e.EntryType, $m)
  }
} catch { Write-Output '  (nenhuma entrada ainda)' }

$cfg = Get-Content '__CONFIG__' -Raw | ConvertFrom-Json
$b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
foreach ($parte in ([string]$cfg.ConnectionStrings.Crm).Split(';')) {
  $kv = $parte.Split('=', 2)
  if ($kv.Count -ne 2) { continue }
  $k = $kv[0].Trim().ToLowerInvariant(); $v = $kv[1].Trim()
  if ($k -in 'server','data source','address','addr') { $b['Data Source'] = $v }
  elseif ($k -in 'database','initial catalog') { $b['Initial Catalog'] = $v }
  elseif ($k -in 'user id','uid','user') { $b['User ID'] = $v }
  elseif ($k -in 'password','pwd') { $b['Password'] = $v }
  elseif ($k -in 'integrated security','trusted_connection') { $b['Integrated Security'] = $v }
}
$cn = New-Object System.Data.SqlClient.SqlConnection $b.ConnectionString

function Tabela([string] $titulo, [string] $sql) {
  Write-Output ''
  Write-Output $titulo
  try {
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 120
    $leitor = $cmd.ExecuteReader()
    $nomes = @(); for ($i = 0; $i -lt $leitor.FieldCount; $i++) { $nomes += $leitor.GetName($i) }
    Write-Output ('  ' + ($nomes -join ' | '))
    while ($leitor.Read()) {
      $valores = @(); for ($i = 0; $i -lt $leitor.FieldCount; $i++) { $valores += [string]$leitor.GetValue($i) }
      Write-Output ('  ' + ($valores -join ' | '))
    }
    $leitor.Close()
  } catch [System.Data.SqlClient.SqlException] {
    Write-Output ('  erro SQL {0}' -f $_.Exception.Number)
  }
}

try {
  $cn.Open()
  Write-Output ''
  Write-Output ('BANCO  {0} em {1}' -f $cn.Database, $cn.DataSource)

  Tabela 'ULTIMAS EXECUCOES' "SELECT TOP 10 FORMAT(IniciadaEm,'yyyy-MM-dd HH:mm:ss') AS IniciadaUtc, FORMAT(TerminadaEm,'HH:mm:ss') AS FimUtc, Resultado, Tentativas, RegistrosLidos AS Lidos, Incluidos, Atualizados, Pendentes, Maquina, LEFT(Mensagem, 300) AS Mensagem FROM integracao.ExecucaoDeSincronizacao ORDER BY IniciadaEm DESC"
  Tabela 'PONTO DE SINCRONISMO' "SELECT s.Codigo, p.Fluxo, p.UltimoValor, FORMAT(p.ProcessadoEm,'yyyy-MM-dd HH:mm:ss') AS ProcessadoUtc, p.RegistrosLidos, p.RegistrosGravados, p.RegistrosErro FROM integracao.PontoDeSincronismo p JOIN integracao.Sistema s ON s.Id = p.SistemaId WHERE s.Codigo = 'ART'"
  Tabela 'CONTAGENS' @"
SELECT 'vendas de maquina' AS Item, COUNT(*) AS Quantidade FROM frota.VendaDeMaquina
UNION ALL SELECT 'vinculos comprador na venda ativos', COUNT(*) FROM frota.VinculoDeClienteComEquipamento WHERE EncerradoEm IS NULL
UNION ALL SELECT 'maquinas de origem Art', COUNT(*) FROM frota.Equipamento WHERE Origem = 'Art'
UNION ALL SELECT 'registros de origem lidos', COUNT(*) FROM integracao.RegistroDeOrigem
UNION ALL SELECT 'compradores na fila', COUNT(*) FROM integracao.CompradorPendente
UNION ALL SELECT 'divergencias abertas', COUNT(*) FROM integracao.DivergenciaDeIntegracao WHERE Situacao = 'Aberta'
UNION ALL SELECT 'classificacoes de produto', COUNT(*) FROM frota.LinhaDeProduto
UNION ALL SELECT 'DUPLICIDADE venda por codigo da origem', COUNT(*) FROM (SELECT SistemaId, ChaveOrigem FROM frota.VendaDeMaquina GROUP BY SistemaId, ChaveOrigem HAVING COUNT(*) > 1) d
UNION ALL SELECT 'DUPLICIDADE chassi ativo', COUNT(*) FROM (SELECT Chassi FROM frota.Equipamento WHERE ExcluidoEm IS NULL GROUP BY Chassi HAVING COUNT(*) > 1) c
"@
} catch [System.Data.SqlClient.SqlException] {
  Write-Output ('BANCO  erro SQL {0}' -f $_.Exception.Number)
} finally { $cn.Close() }
'@

$script = $script -replace '__SERVICO__', $Servico -replace '__CONFIG__', $ConfigDaApi
Invoke-NoServidor -Script $script -Nome 'crm-verificar-sincronizacao' -TimeoutSegundos 300
