<#
  07-diff-homologacao.ps1
  -----------------------
  Compara o banco de PRODUCAO (CRM) com o de HOMOLOGACAO (CRM_HOMO):
  tabelas, colunas e modulos que existem num e nao no outro, e modulos
  cujo texto diverge. Serve para saber o que ja foi customizado em
  producao sem passar por homologacao (e vice-versa).

  Saida: docs/extracao-vortice/_raw/diff-homologacao-*.csv
         (o resumo em prosa entra no 00-RELATORIO-EXTRACAO.md)
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Raw = Join-Path $Global:VorticeRaiz '_raw'

function Conjunto($linhas, $col) {
    $h = @{}
    foreach ($r in $linhas) { $h[([string]$r[$col])] = $r }
    return $h
}

# ---------------- tabelas ----------------
$tProd = Conjunto (Consulta -Rotulo 'CRM: tabelas'      -Sql "SELECT name FROM CRM.sys.tables WITH (NOLOCK)") 'name'
$tHomo = Conjunto (Consulta -Rotulo 'CRM_HOMO: tabelas' -Sql "SELECT name FROM CRM_HOMO.sys.tables WITH (NOLOCK)") 'name'

$difTab = New-Object System.Collections.ArrayList
foreach ($n in ($tProd.Keys | Sort-Object)) { if (-not $tHomo.ContainsKey($n)) { [void]$difTab.Add([pscustomobject]@{ objeto = $n; tipo = 'TABELA'; situacao = 'so em PRODUCAO' }) } }
foreach ($n in ($tHomo.Keys | Sort-Object)) { if (-not $tProd.ContainsKey($n)) { [void]$difTab.Add([pscustomobject]@{ objeto = $n; tipo = 'TABELA'; situacao = 'so em HOMOLOGACAO' }) } }

# ---------------- modulos ----------------
$mProd = Consulta -Rotulo 'CRM: modulos' -Sql @'
SELECT o.name, o.type_desc, m.definition
FROM CRM.sys.sql_modules m WITH (NOLOCK)
JOIN CRM.sys.objects o WITH (NOLOCK) ON o.object_id = m.object_id
'@
$mHomo = Consulta -Rotulo 'CRM_HOMO: modulos' -Sql @'
SELECT o.name, o.type_desc, m.definition
FROM CRM_HOMO.sys.sql_modules m WITH (NOLOCK)
JOIN CRM_HOMO.sys.objects o WITH (NOLOCK) ON o.object_id = m.object_id
'@
$hProd = Conjunto $mProd 'name'
$hHomo = Conjunto $mHomo 'name'

$difMod = New-Object System.Collections.ArrayList
foreach ($n in ($hProd.Keys | Sort-Object)) {
    if (-not $hHomo.ContainsKey($n)) {
        [void]$difMod.Add([pscustomobject]@{ objeto = $n; tipo = [string]$hProd[$n]['type_desc']; situacao = 'so em PRODUCAO'; chars_prod = ([string]$hProd[$n]['definition']).Length; chars_homo = '' })
    } else {
        $a = [string]$hProd[$n]['definition']; $b = [string]$hHomo[$n]['definition']
        if ($a -ne $b) {
            [void]$difMod.Add([pscustomobject]@{ objeto = $n; tipo = [string]$hProd[$n]['type_desc']; situacao = 'TEXTO DIFERENTE'; chars_prod = $a.Length; chars_homo = $b.Length })
        }
    }
}
foreach ($n in ($hHomo.Keys | Sort-Object)) {
    if (-not $hProd.ContainsKey($n)) {
        [void]$difMod.Add([pscustomobject]@{ objeto = $n; tipo = [string]$hHomo[$n]['type_desc']; situacao = 'so em HOMOLOGACAO'; chars_prod = ''; chars_homo = ([string]$hHomo[$n]['definition']).Length })
    }
}

# ---------------- colunas (apenas de tabelas presentes nos dois) ----------------
$cProd = Consulta -Rotulo 'CRM: colunas' -Sql @'
SELECT OBJECT_NAME(c.object_id) AS tabela, c.name AS coluna
FROM CRM.sys.columns c WITH (NOLOCK)
JOIN CRM.sys.tables t WITH (NOLOCK) ON t.object_id = c.object_id
'@
$cHomo = Consulta -Rotulo 'CRM_HOMO: colunas' -Sql @'
SELECT OBJECT_NAME(c.object_id) AS tabela, c.name AS coluna
FROM CRM_HOMO.sys.columns c WITH (NOLOCK)
JOIN CRM_HOMO.sys.tables t WITH (NOLOCK) ON t.object_id = c.object_id
'@
$setProd = @{}; foreach ($r in $cProd) { $setProd[([string]$r['tabela'] + '.' + [string]$r['coluna'])] = $true }
$setHomo = @{}; foreach ($r in $cHomo) { $setHomo[([string]$r['tabela'] + '.' + [string]$r['coluna'])] = $true }

$difCol = New-Object System.Collections.ArrayList
foreach ($k in ($setProd.Keys | Sort-Object)) {
    $tab = $k.Split('.')[0]
    if ($tHomo.ContainsKey($tab) -and -not $setHomo.ContainsKey($k)) {
        [void]$difCol.Add([pscustomobject]@{ tabela = $tab; coluna = $k.Substring($tab.Length + 1); situacao = 'so em PRODUCAO' })
    }
}
foreach ($k in ($setHomo.Keys | Sort-Object)) {
    $tab = $k.Split('.')[0]
    if ($tProd.ContainsKey($tab) -and -not $setProd.ContainsKey($k)) {
        [void]$difCol.Add([pscustomobject]@{ tabela = $tab; coluna = $k.Substring($tab.Length + 1); situacao = 'so em HOMOLOGACAO' })
    }
}

$difTab | Export-Csv (Join-Path $Raw 'diff-homologacao-tabelas.csv') -NoTypeInformation -Encoding UTF8
$difMod | Export-Csv (Join-Path $Raw 'diff-homologacao-modulos.csv') -NoTypeInformation -Encoding UTF8
$difCol | Export-Csv (Join-Path $Raw 'diff-homologacao-colunas.csv') -NoTypeInformation -Encoding UTF8

Write-Host ("PROD: {0} tabelas, {1} modulos | HOMO: {2} tabelas, {3} modulos" -f
    $tProd.Count, @($mProd).Count, $tHomo.Count, @($mHomo).Count) -ForegroundColor Cyan
Write-Host ("Diferencas: {0} tabelas, {1} colunas, {2} modulos ({3} com texto divergente)" -f
    $difTab.Count, $difCol.Count, $difMod.Count,
    @($difMod | Where-Object { $_.situacao -eq 'TEXTO DIFERENTE' }).Count) -ForegroundColor Green
