<#
  04-volumetria.ps1
  -----------------
  Volumetria do banco CRM: linhas por tabela, espaco em disco, crescimento anual
  das tabelas transacionais e ultima atividade (MAX da data de inclusao) por tabela.

  OBS. DE PERMISSAO: o login CRM_Leitura NAO tem VIEW DATABASE STATE, entao
  sys.dm_db_partition_stats esta negado. Usamos sys.partitions + sys.allocation_units,
  que sao views de catalogo e retornam os mesmos numeros.

  Saida:
    docs/extracao-vortice/volumetria/linhas-por-tabela.csv
    docs/extracao-vortice/volumetria/tamanho-em-disco.csv
    docs/extracao-vortice/volumetria/arquivos-do-banco.csv
    docs/extracao-vortice/volumetria/crescimento-anual.csv
    docs/extracao-vortice/volumetria/ultima-atividade.csv
    docs/extracao-vortice/volumetria/comparacao-junho.csv
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Vol = Join-Path $Global:VorticeRaiz 'volumetria'
if (-not (Test-Path $Vol)) { New-Item -ItemType Directory -Path $Vol -Force | Out-Null }

# ---------------------------------------------------------------- 1) linhas por tabela
$linhas = Consulta -Rotulo 'linhas por tabela' -Sql @'
SELECT t.name AS tabela, SUM(p.rows) AS linhas
FROM sys.partitions p WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY SUM(p.rows) DESC
'@
[void](ExportaCsv $linhas (Join-Path $Vol 'linhas-por-tabela.csv'))

# comparacao com o snapshot de 2025-06 guardado no repositorio do agente
$snapshot = 'c:\projetos\vortice-crm-agent\schema\tabelas.csv'
if (Test-Path $snapshot) {
    $antigo = @{}
    foreach ($r in (Import-Csv $snapshot)) {
        $nome = $r.PSObject.Properties | Where-Object { $_.Name -match '(?i)^(tabela|name|table_name|nome)$' } | Select-Object -First 1
        # ATENCAO: casar o nome EXATO da coluna de linhas. Um -match frouxo com 'qtd'
        # acerta antes em 'qtd_colunas' e a comparacao sai toda errada.
        $qtd = $r.PSObject.Properties | Where-Object { $_.Name -match '(?i)^(linhas|rows|row_count|qtd_linhas)$' } | Select-Object -First 1
        if ($nome -and $qtd) { $antigo[([string]$nome.Value).ToLower()] = [int64]([string]$qtd.Value -replace '[^\d-]', '') }
    }
    $comp = New-Object System.Collections.ArrayList
    foreach ($r in $linhas) {
        $n = ([string]$r['tabela']).ToLower()
        $ant = if ($antigo.ContainsKey($n)) { $antigo[$n] } else { $null }
        [void]$comp.Add([pscustomobject]@{
                tabela         = [string]$r['tabela']
                linhas_agora   = [int64]$r['linhas']
                linhas_snapshot = $(if ($null -ne $ant) { $ant } else { '' })
                delta          = $(if ($null -ne $ant) { [int64]$r['linhas'] - $ant } else { '' })
                pct            = $(if ($null -ne $ant -and $ant -gt 0) { [math]::Round((([int64]$r['linhas'] - $ant) * 100.0 / $ant), 1) } else { '' })
            })
    }
    $comp | Sort-Object { if ($_.delta -eq '') { -9223372036854775808 } else { [int64]$_.delta } } -Descending |
        Export-Csv (Join-Path $Vol 'comparacao-junho.csv') -NoTypeInformation -Encoding UTF8
    Write-Host ('  comparacao com snapshot: ' + $comp.Count + ' tabelas')
}

# ---------------------------------------------------------------- 2) espaco em disco
$disco = Consulta -Rotulo 'espaco por tabela/indice' -Sql @'
SELECT t.name                        AS tabela,
       ISNULL(i.name, '(heap)')      AS indice,
       i.type_desc                   AS tipo_indice,
       SUM(a.total_pages) * 8        AS reservado_kb,
       SUM(a.used_pages)  * 8        AS usado_kb,
       SUM(a.data_pages)  * 8        AS dados_kb,
       SUM(p.rows)                   AS linhas
FROM sys.allocation_units a WITH (NOLOCK)
JOIN sys.partitions p WITH (NOLOCK) ON p.partition_id = a.container_id
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = p.object_id
LEFT JOIN sys.indexes i WITH (NOLOCK) ON i.object_id = p.object_id AND i.index_id = p.index_id
GROUP BY t.name, i.name, i.type_desc
ORDER BY SUM(a.total_pages) DESC
'@
[void](ExportaCsv $disco (Join-Path $Vol 'tamanho-em-disco.csv'))

$arquivos = Consulta -Rotulo 'arquivos do banco' -Sql @'
SELECT name AS arquivo_logico, physical_name AS arquivo_fisico, type_desc,
       size * 8 / 1024        AS tamanho_mb,
       CASE WHEN is_percent_growth = 1 THEN CONVERT(varchar(20), growth) + ' %'
            ELSE CONVERT(varchar(20), growth * 8 / 1024) + ' MB' END AS crescimento,
       CASE WHEN max_size = -1 THEN 'ilimitado'
            WHEN max_size = 0  THEN 'sem crescimento'
            ELSE CONVERT(varchar(20), max_size * 8 / 1024) + ' MB' END AS tamanho_maximo,
       state_desc
FROM sys.database_files WITH (NOLOCK)
'@
[void](ExportaCsv $arquivos (Join-Path $Vol 'arquivos-do-banco.csv'))

# ---------------------------------------------------------------- 3) descoberta de colunas de data
# Preferimos, nesta ordem, a coluna que representa "quando a linha nasceu".
$prefDatas = @('DtaInclusao', 'DTAINCLUSAO', 'DtInclusao', 'DtaCadastro', 'DtaCriacao',
    'DtaAbertura', 'DtaRealizacao', 'DtaEmissaoNF', 'DtaEmissao', 'DtaMovto', 'DtaLog',
    'DtaOcorrencia', 'DtaEnvio', 'DtaProcessamento', 'DtaAlteracao', 'Data', 'DtaDoc')

$colsData = Consulta -Rotulo 'colunas de data' -Sql @'
SELECT OBJECT_NAME(c.object_id) AS tabela, c.name AS coluna, c.column_id
FROM sys.columns c WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = c.object_id
JOIN sys.types ty WITH (NOLOCK) ON ty.user_type_id = c.user_type_id
WHERE ty.name IN ('datetime', 'smalldatetime', 'date', 'datetime2')
ORDER BY c.object_id, c.column_id
'@
$dataPorTab = @{}
foreach ($c in $colsData) {
    $t = [string]$c['tabela']
    if (-not $dataPorTab.ContainsKey($t)) { $dataPorTab[$t] = New-Object System.Collections.ArrayList }
    [void]$dataPorTab[$t].Add([string]$c['coluna'])
}
function EscolheColunaData($tabela) {
    if (-not $dataPorTab.ContainsKey($tabela)) { return $null }
    $disp = $dataPorTab[$tabela]
    foreach ($p in $prefDatas) {
        $achou = $disp | Where-Object { $_ -eq $p } | Select-Object -First 1
        if ($achou) { return $achou }
    }
    foreach ($p in $prefDatas) {
        $achou = $disp | Where-Object { $_ -like ($p + '*') } | Select-Object -First 1
        if ($achou) { return $achou }
    }
    return ($disp | Select-Object -First 1)
}

$linhasPorTab = @{}
foreach ($r in $linhas) { $linhasPorTab[[string]$r['tabela']] = [int64]$r['linhas'] }

# ---------------------------------------------------------------- 4) crescimento anual
$alvosCrescimento = @('IV_Processo', 'IV_Historico', 'IV_Agenda', 'IV_Questionario', 'GE_Pessoa',
    'EXT_NFS', 'EXT_OS', 'EXT_Titulo', 'EXT_Veic', 'GE_LOG_PROCESSO', 'GE_LgTb', 'DMN_Doc',
    'IV_AgendaLog', 'IV_ProcDado', 'IV_Interacao', 'GEP_EMAILSENT', 'IMP_Titulo', 'IMP_OS')

$cresc = New-Object System.Collections.ArrayList
foreach ($t in $alvosCrescimento) {
    if (-not $linhasPorTab.ContainsKey($t)) {
        Write-Warning ("crescimento: tabela inexistente " + $t); continue
    }
    # Varias tabelas tem a coluna "natural" de data inteiramente NULA (ex.: EXT_OS.Dtaabertura).
    # Por isso tentamos os candidatos em ordem ate um deles produzir linhas.
    $candidatas = New-Object System.Collections.ArrayList
    $primeira = EscolheColunaData $t
    if ($primeira) { [void]$candidatas.Add($primeira) }
    if ($dataPorTab.ContainsKey($t)) {
        foreach ($p in $prefDatas) {
            foreach ($d in $dataPorTab[$t]) { if ($d -like ($p + '*') -and -not $candidatas.Contains($d)) { [void]$candidatas.Add($d) } }
        }
        foreach ($d in $dataPorTab[$t]) { if (-not $candidatas.Contains($d)) { [void]$candidatas.Add($d) } }
    }
    if ($candidatas.Count -eq 0) { Write-Warning ("crescimento: sem coluna de data em " + $t); continue }

    $achou = $false
    foreach ($col in $candidatas) {
        $sql = "SELECT YEAR([$col]) AS ano, COUNT(*) AS linhas FROM [$t] WITH (NOLOCK) WHERE [$col] IS NOT NULL GROUP BY YEAR([$col]) ORDER BY 1"
        $r = @(Consulta -Rotulo ("crescimento " + $t + ' (' + $col + ')') -Sql $sql -TimeoutSec 120)
        if ($r.Count -gt 0) {
            foreach ($x in $r) {
                [void]$cresc.Add([pscustomobject]@{ tabela = $t; coluna_data = $col; ano = [string]$x['ano']; linhas = [int64]$x['linhas'] })
            }
            $achou = $true
            break
        }
        Write-Warning ("  $t.$col esta 100% nula - tentando a proxima coluna de data")
    }
    if (-not $achou) { Write-Warning ("crescimento: nenhuma coluna de data preenchida em " + $t) }
}
$cresc | Export-Csv (Join-Path $Vol 'crescimento-anual.csv') -NoTypeInformation -Encoding UTF8

# ---------------------------------------------------------------- 5) ultima atividade
# MAX(data) por tabela. Tabelas acima de 5 milhoes de linhas sao puladas para
# nao segurar o servidor; ficam registradas como "pulada por volume".
$ult = New-Object System.Collections.ArrayList
$limiteLinhas = 5000000
foreach ($t in ($dataPorTab.Keys | Sort-Object)) {
    if (-not $linhasPorTab.ContainsKey($t)) { continue }
    $qtd = $linhasPorTab[$t]
    if ($qtd -eq 0) {
        [void]$ult.Add([pscustomobject]@{ tabela = $t; linhas = 0; coluna_data = ''; ultima_data = ''; primeira_data = ''; situacao = 'VAZIA' })
        continue
    }
    $col = EscolheColunaData $t
    if (-not $col) { continue }
    if ($qtd -gt $limiteLinhas) {
        [void]$ult.Add([pscustomobject]@{ tabela = $t; linhas = $qtd; coluna_data = $col; ultima_data = ''; primeira_data = ''; situacao = 'PULADA POR VOLUME' })
        continue
    }
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    # ATENCAO: 'return' no PowerShell desenrola array de 1 elemento; sem o @()
    # $r[0] indexaria a COLUNA 0 do DataRow em vez da primeira linha.
    $r = @(Consulta -Silencioso -TimeoutSec 60 -Rotulo $t -Sql "SELECT MAX([$col]) AS ultima, MIN([$col]) AS primeira FROM [$t] WITH (NOLOCK)")
    $sw.Stop()
    if ($r.Count -eq 0) {
        [void]$ult.Add([pscustomobject]@{ tabela = $t; linhas = $qtd; coluna_data = $col; ultima_data = ''; primeira_data = ''; situacao = 'ERRO/TIMEOUT' })
        continue
    }
    $u = Valor $r[0] 'ultima'; $p = Valor $r[0] 'primeira'
    $sit = 'ok'
    if ($u -is [datetime]) {
        $dias = ((Get-Date) - $u).TotalDays
        if ($dias -gt 365) { $sit = 'PARADA (>1 ano)' }
        elseif ($dias -gt 90) { $sit = 'fria (>90 dias)' }
        else { $sit = 'viva' }
    }
    [void]$ult.Add([pscustomobject]@{
            tabela        = $t
            linhas        = $qtd
            coluna_data   = $col
            ultima_data   = $(if ($u -is [datetime]) { $u.ToString('yyyy-MM-dd') } else { '' })
            primeira_data = $(if ($p -is [datetime]) { $p.ToString('yyyy-MM-dd') } else { '' })
            situacao      = $sit
            segundos      = [math]::Round($sw.Elapsed.TotalSeconds, 1)
        })
}
$ult | Sort-Object ultima_data -Descending | Export-Csv (Join-Path $Vol 'ultima-atividade.csv') -NoTypeInformation -Encoding UTF8

Write-Host ('OK: ' + @($linhas).Count + ' tabelas | ' + $cresc.Count + ' linhas de crescimento | ' +
    $ult.Count + ' tabelas com data avaliadas') -ForegroundColor Green
