<#
  05-dominios.ps1
  ---------------
  Descobre os DOMINIOS REAIS das colunas de codigo do nucleo do Vortice CRM.
  O sistema quase nao usa CHECK constraints (zero no banco): os valores validos
  vivem so no codigo da aplicacao. Este script recupera esses dominios a partir
  dos dados, por frequencia.

  Criterio de candidata:
    - tabela com prefixo IV_, GE_, EXT_, IVS_, IVF_, DMN_ ou GEP_
    - excluidas IV_Q_* (questionarios gerados) e tabelas de backup/teste
    - coluna char/varchar de tamanho <= 3, OU nome iniciando por
      Ind / Tipo / Status / Situacao / Classe / Flag
    - so entram no resultado colunas com no maximo 30 valores distintos

  Tabelas com mais de 3.000.000 de linhas sao amostradas com TOP (500000).

  Saida: docs/extracao-vortice/dominios/valores.csv
         docs/extracao-vortice/_raw/dominios-descartados.csv
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Dom = Join-Path $Global:VorticeRaiz 'dominios'
$Raw = Join-Path $Global:VorticeRaiz '_raw'
if (-not (Test-Path $Dom)) { New-Item -ItemType Directory -Path $Dom -Force | Out-Null }

$MAX_DISTINTOS = 30
$LIMITE_AMOSTRA = 3000000
$TAM_AMOSTRA = 500000

# tabelas descartadas por serem copias, testes ou lixo
$padraoLixo = '(?i)(_bkp|bkp_|_BKPJUN|backup|_bak|teste|_old|_ITA$|_NELSON$|_PNEUS$|_02_03$|SYSCONVERT|^mig_|_bkpjun$|_bkp\d+$)'

Write-Host 'Selecionando colunas candidatas...' -ForegroundColor Cyan
$cands = Consulta -Rotulo 'colunas candidatas' -Sql @'
SELECT t.name AS tabela, c.name AS coluna, ty.name AS tipo, c.max_length
FROM sys.columns c WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = c.object_id
JOIN sys.types ty WITH (NOLOCK) ON ty.user_type_id = c.user_type_id
WHERE (t.name LIKE 'IV[_]%' OR t.name LIKE 'GE[_]%' OR t.name LIKE 'EXT[_]%'
       OR t.name LIKE 'IVS[_]%' OR t.name LIKE 'IVF[_]%' OR t.name LIKE 'DMN[_]%' OR t.name LIKE 'GEP[_]%')
  AND t.name NOT LIKE 'IV[_]Q[_]%'
  AND ( (ty.name IN ('char','varchar','nchar','nvarchar') AND c.max_length <= 3)
        OR c.name LIKE 'Ind%' OR c.name LIKE 'Tipo%' OR c.name LIKE 'Status%'
        OR c.name LIKE 'Situacao%' OR c.name LIKE 'Classe%' OR c.name LIKE 'Flag%' )
ORDER BY t.name, c.column_id
'@

$linhasTab = @{}
foreach ($r in (Consulta -Rotulo 'linhas por tabela' -Sql @'
SELECT t.name AS tabela, SUM(p.rows) AS linhas
FROM sys.partitions p WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = p.object_id
WHERE p.index_id IN (0,1)
GROUP BY t.name
'@)) { $linhasTab[[string]$r['tabela']] = [int64]$r['linhas'] }

$saida      = New-Object System.Collections.ArrayList
$descartes  = New-Object System.Collections.ArrayList
$i = 0
$total = @($cands).Count
$relogioGeral = [System.Diagnostics.Stopwatch]::StartNew()

foreach ($c in $cands) {
    $i++
    $tab = [string]$c['tabela']
    $col = [string]$c['coluna']

    if ($tab -match $padraoLixo) {
        [void]$descartes.Add([pscustomobject]@{ tabela = $tab; coluna = $col; motivo = 'tabela de backup/teste' }); continue
    }
    $qtdLinhas = if ($linhasTab.ContainsKey($tab)) { $linhasTab[$tab] } else { 0 }
    if ($qtdLinhas -eq 0) {
        [void]$descartes.Add([pscustomobject]@{ tabela = $tab; coluna = $col; motivo = 'tabela vazia' }); continue
    }

    if ($qtdLinhas -gt $LIMITE_AMOSTRA) {
        $fonte = "(SELECT TOP ($TAM_AMOSTRA) [$col] FROM [$tab] WITH (NOLOCK)) AS amostra"
        $amostrada = $true
    } else {
        $fonte = "[$tab] WITH (NOLOCK)"
        $amostrada = $false
    }

    $sql = "SELECT TOP ($($MAX_DISTINTOS + 1)) [$col] AS valor, COUNT(*) AS qtd FROM $fonte GROUP BY [$col] ORDER BY COUNT(*) DESC"
    $r = Consulta -Silencioso -TimeoutSec 60 -Rotulo "$tab.$col" -Sql $sql

    if (@($r).Count -eq 0) {
        [void]$descartes.Add([pscustomobject]@{ tabela = $tab; coluna = $col; motivo = 'erro ou timeout na consulta' }); continue
    }
    if (@($r).Count -gt $MAX_DISTINTOS) {
        [void]$descartes.Add([pscustomobject]@{ tabela = $tab; coluna = $col; motivo = "mais de $MAX_DISTINTOS valores distintos" }); continue
    }

    $totalCol = (@($r) | ForEach-Object { [int64]$_['qtd'] } | Measure-Object -Sum).Sum
    foreach ($x in $r) {
        $v = $x['valor']
        $vTxt = if ($v -is [DBNull]) { '(NULO)' } elseif ([string]$v -eq '') { '(VAZIO)' } else { ([string]$v).Trim() }
        [void]$saida.Add([pscustomobject]@{
                tabela    = $tab
                coluna    = $col
                tipo      = [string]$c['tipo']
                valor     = $vTxt
                qtd       = [int64]$x['qtd']
                pct       = $(if ($totalCol -gt 0) { [math]::Round([int64]$x['qtd'] * 100.0 / $totalCol, 2) } else { 0 })
                amostrada = $amostrada
            })
    }

    if ($i % 50 -eq 0) {
        Write-Host ("  {0}/{1} colunas analisadas ({2}s)" -f $i, $total, [math]::Round($relogioGeral.Elapsed.TotalSeconds))
    }
}

$saida     | Export-Csv (Join-Path $Dom 'valores.csv') -NoTypeInformation -Encoding UTF8
$descartes | Export-Csv (Join-Path $Raw 'dominios-descartados.csv') -NoTypeInformation -Encoding UTF8

$colsOk = (@($saida | Select-Object tabela, coluna -Unique)).Count
Write-Host ("OK: {0} colunas de dominio, {1} valores, {2} descartadas ({3}s)" -f
    $colsOk, $saida.Count, $descartes.Count, [math]::Round($relogioGeral.Elapsed.TotalSeconds)) -ForegroundColor Green
