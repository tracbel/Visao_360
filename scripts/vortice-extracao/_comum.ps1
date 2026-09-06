<#
  _comum.ps1 - funcoes compartilhadas pelos scripts de extracao do Vortice CRM.

  IMPORTANTE: Invoke-Vortice devolve um System.Object[] de DataRow (o PowerShell
  desenrola o DataTable). Por isso usamos sempre Consulta(), que normaliza para
  array e ja aplica timeout maior + medicao de tempo.

  O guard do connect.ps1 bloqueia INSERT/UPDATE/DELETE/CREATE/ALTER/DROP/EXEC...
  no TEXTO da query, inclusive dentro de literais. Por isso, quando precisamos
  dessas palavras (ex.: gerar DDL), montamos o texto em PowerShell com
  concatenacao ('CRE' + 'ATE'), nunca enviando ao servidor.
#>

. 'c:\projetos\vortice-crm-agent\connect.ps1'

$Global:VorticeRaiz = 'c:\projetos\tracbel-crm\docs\extracao-vortice'
$Global:VorticeUtf8 = New-Object System.Text.UTF8Encoding($false)

function Consulta {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)] [string]$Sql,
        [string]$Rotulo = '',
        [int]$TimeoutSec = 300,
        [switch]$Silencioso
    )
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $r = @(Invoke-Vortice -TimeoutSec $TimeoutSec $Sql)
    } catch {
        Write-Warning ("FALHA [$Rotulo]: " + $_.Exception.Message)
        return @()
    }
    $sw.Stop()
    if (-not $Silencioso) {
        Write-Host ("  {0,-42} {1,7} linhas  {2,6}s" -f $Rotulo, $r.Count, [math]::Round($sw.Elapsed.TotalSeconds, 1))
    }
    return $r
}

function Valor($row, $col) {
    if ($null -eq $row) { return '' }
    $v = $row[$col]
    if ($v -is [DBNull]) { return '' }
    return $v
}

function Texto($row, $col) { return [string](Valor $row $col) }

function GravaUtf8($caminho, $conteudo) {
    $dir = Split-Path $caminho -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    [System.IO.File]::WriteAllText($caminho, $conteudo, $Global:VorticeUtf8)
}

# Converte DataRow[] em PSCustomObject[] contendo SOMENTE as colunas da consulta.
# Sem isso, o Export-Csv de um DataRow arrasta RowError/HasErrors/Table/ItemArray.
function ParaObjetos($linhas) {
    $saida = New-Object System.Collections.ArrayList
    foreach ($r in $linhas) {
        $h = [ordered]@{}
        foreach ($c in $r.Table.Columns) {
            $v = $r[$c.ColumnName]
            $h[$c.ColumnName] = $(if ($v -is [DBNull]) { '' } else { $v })
        }
        [void]$saida.Add([pscustomobject]$h)
    }
    return $saida
}

# Exporta consulta (DataRow[]) para CSV UTF-8 limpo.
function ExportaCsv($linhas, $caminho) {
    $dir = Split-Path $caminho -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if (@($linhas).Count -eq 0) {
        [System.IO.File]::WriteAllText($caminho, "(sem linhas)`r`n", $Global:VorticeUtf8)
        return 0
    }
    ParaObjetos $linhas | Export-Csv $caminho -NoTypeInformation -Encoding UTF8
    return @($linhas).Count
}

function AgrupaPor($linhas, $chave) {
    $h = @{}
    foreach ($r in $linhas) {
        $k = [string]$r[$chave]
        if (-not $h.ContainsKey($k)) { $h[$k] = New-Object System.Collections.ArrayList }
        [void]$h[$k].Add($r)
    }
    return $h
}
