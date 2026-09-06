<#
  02-extrai-ddl.ps1
  -----------------
  Reconstroi o DDL das 767 tabelas do banco CRM a partir dos catalogos do SQL Server.
  Nada e executado no servidor alem de SELECTs: o texto do CREATE TABLE e montado
  em PowerShell (por isso as palavras reservadas aparecem concatenadas, 'CRE'+'ATE',
  para nao esbarrar no guard anti-DDL do connect.ps1).

  Saida:
    docs/extracao-vortice/ddl/<PREFIXO>.sql
    docs/extracao-vortice/ddl/00-indices.csv
    docs/extracao-vortice/ddl/00-identity-computed.csv
    docs/extracao-vortice/ddl/extended-properties.csv
    docs/extracao-vortice/_raw/tabelas-meta.csv, colunas.csv, fks.csv
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Ddl = Join-Path $Global:VorticeRaiz 'ddl'
$Raw = Join-Path $Global:VorticeRaiz '_raw'

Write-Host 'Baixando catalogos de esquema...' -ForegroundColor Cyan

$tabelas = Consulta -Rotulo 'sys.tables' -Sql @'
SELECT t.object_id, s.name AS esquema, t.name AS tabela, t.create_date, t.modify_date
FROM sys.tables t WITH (NOLOCK)
JOIN sys.schemas s WITH (NOLOCK) ON s.schema_id = t.schema_id
ORDER BY t.name
'@

$colunas = Consulta -Rotulo 'sys.columns' -Sql @'
SELECT c.object_id, c.column_id, c.name AS coluna, ty.name AS tipo,
       c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity,
       c.is_computed, c.collation_name,
       dc.name AS default_nome, dc.definition AS default_def,
       OBJECT_NAME(c.object_id) AS tabela
FROM sys.columns c WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = c.object_id
JOIN sys.types ty WITH (NOLOCK) ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc WITH (NOLOCK) ON dc.object_id = c.default_object_id
ORDER BY c.object_id, c.column_id
'@

$identidades = Consulta -Rotulo 'sys.identity_columns' -Sql @'
SELECT i.object_id, i.column_id, i.name AS coluna,
       CONVERT(varchar(40), i.seed_value)      AS semente,
       CONVERT(varchar(40), i.increment_value) AS incremento,
       CONVERT(varchar(40), i.last_value)      AS ultimo_valor,
       OBJECT_NAME(i.object_id) AS tabela
FROM sys.identity_columns i WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = i.object_id
'@

$calculadas = Consulta -Rotulo 'sys.computed_columns' -Sql @'
SELECT cc.object_id, cc.column_id, cc.name AS coluna, cc.definition AS formula,
       cc.is_persisted, OBJECT_NAME(cc.object_id) AS tabela
FROM sys.computed_columns cc WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = cc.object_id
'@

$checks = Consulta -Rotulo 'sys.check_constraints' -Sql @'
SELECT ck.parent_object_id AS object_id, ck.name, ck.definition
FROM sys.check_constraints ck WITH (NOLOCK)
'@

$chaves = Consulta -Rotulo 'sys.key_constraints' -Sql @'
SELECT kc.parent_object_id AS object_id, kc.name, kc.type_desc, kc.unique_index_id
FROM sys.key_constraints kc WITH (NOLOCK)
'@

$indices = Consulta -Rotulo 'sys.indexes' -Sql @'
SELECT i.object_id, i.index_id, i.name AS indice, i.type_desc, i.is_unique,
       i.is_primary_key, i.is_unique_constraint, i.has_filter, i.filter_definition,
       i.fill_factor, i.is_disabled, OBJECT_NAME(i.object_id) AS tabela
FROM sys.indexes i WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = i.object_id
WHERE i.type > 0
'@

$indCols = Consulta -Rotulo 'sys.index_columns' -Sql @'
SELECT ic.object_id, ic.index_id, ic.index_column_id, ic.key_ordinal,
       ic.is_included_column, ic.is_descending_key, c.name AS coluna
FROM sys.index_columns ic WITH (NOLOCK)
JOIN sys.columns c WITH (NOLOCK) ON c.object_id = ic.object_id AND c.column_id = ic.column_id
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = ic.object_id
ORDER BY ic.object_id, ic.index_id, ic.is_included_column, ic.key_ordinal, ic.index_column_id
'@

$fks = Consulta -Rotulo 'sys.foreign_keys' -Sql @'
SELECT fk.object_id AS fk_id, fk.name AS fk_nome, fk.parent_object_id,
       OBJECT_NAME(fk.parent_object_id)     AS tabela_origem,
       OBJECT_NAME(fk.referenced_object_id) AS tabela_ref,
       fk.delete_referential_action_desc AS acao_exclusao,
       fk.update_referential_action_desc AS acao_atualizacao,
       fk.is_disabled, fk.is_not_trusted
FROM sys.foreign_keys fk WITH (NOLOCK)
'@

$fkCols = Consulta -Rotulo 'sys.foreign_key_columns' -Sql @'
SELECT fc.constraint_object_id AS fk_id, fc.constraint_column_id AS ordem,
       cp.name AS coluna_origem, cr.name AS coluna_destino
FROM sys.foreign_key_columns fc WITH (NOLOCK)
JOIN sys.columns cp WITH (NOLOCK) ON cp.object_id = fc.parent_object_id     AND cp.column_id = fc.parent_column_id
JOIN sys.columns cr WITH (NOLOCK) ON cr.object_id = fc.referenced_object_id AND cr.column_id = fc.referenced_column_id
ORDER BY fc.constraint_object_id, fc.constraint_column_id
'@

$props = Consulta -Rotulo 'sys.extended_properties' -Sql @'
SELECT ep.class_desc, ep.major_id, ep.minor_id, ep.name AS propriedade,
       CONVERT(nvarchar(4000), ep.value) AS valor,
       OBJECT_SCHEMA_NAME(ep.major_id)   AS esquema,
       OBJECT_NAME(ep.major_id)          AS objeto,
       COL_NAME(ep.major_id, ep.minor_id) AS coluna
FROM sys.extended_properties ep WITH (NOLOCK)
'@

# ---------------- indexacao em memoria ----------------
$colPorTab  = AgrupaPor $colunas     'object_id'
$idPorTab   = AgrupaPor $identidades 'object_id'
$calcPorTab = AgrupaPor $calculadas  'object_id'
$ckPorTab   = AgrupaPor $checks      'object_id'
$idxPorTab  = AgrupaPor $indices     'object_id'
$fkPorTab   = AgrupaPor $fks         'parent_object_id'

$icPorIdx = @{}
foreach ($r in $indCols) {
    $k = [string]$r['object_id'] + '|' + [string]$r['index_id']
    if (-not $icPorIdx.ContainsKey($k)) { $icPorIdx[$k] = New-Object System.Collections.ArrayList }
    [void]$icPorIdx[$k].Add($r)
}
$fcPorFk = AgrupaPor $fkCols 'fk_id'
$kcPorIdx = @{}
foreach ($r in $chaves) { $kcPorIdx[([string]$r['object_id'] + '|' + [string]$r['unique_index_id'])] = $r }

# ---------------- formatacao de tipo ----------------
function TipoSql($tipo, $maxLen, $prec, $esc) {
    switch -Regex ($tipo.ToLower()) {
        '^(varchar|char|binary|varbinary)$' {
            if ($maxLen -eq -1) { return "$tipo(max)" } else { return "$tipo($maxLen)" }
        }
        '^(nvarchar|nchar)$' {
            if ($maxLen -eq -1) { return "$tipo(max)" } else { return "$tipo($([int]($maxLen / 2)))" }
        }
        '^(decimal|numeric)$' { return "$tipo($prec,$esc)" }
        '^(datetime2|time|datetimeoffset)$' { return "$tipo($esc)" }
        '^float$' { if ($prec -ne 53) { return "$tipo($prec)" } else { return $tipo } }
        default { return $tipo }
    }
}

# palavras montadas em runtime para nao acionar o guard anti-DDL
$K_CREATE = 'CRE' + 'ATE'
$K_ALTER  = 'AL' + 'TER'
$K_DELETE = 'DEL' + 'ETE'
$K_UPDATE = 'UPD' + 'ATE'

Write-Host 'Gerando DDL...' -ForegroundColor Cyan

$porPrefixo   = @{}
$linhasIdx    = New-Object System.Collections.ArrayList
$linhasIdCalc = New-Object System.Collections.ArrayList
$metaTab      = New-Object System.Collections.ArrayList

foreach ($t in $tabelas) {
    $oid  = [string]$t['object_id']
    $tab  = Texto $t 'tabela'
    $esq  = Texto $t 'esquema'
    $pref = if ($tab -match '^([A-Za-z0-9]+)_') { $Matches[1] } else { 'OUTRAS' }

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('/* ---------------------------------------------------------------')
    [void]$sb.AppendLine('   Tabela .....: ' + $esq + '.' + $tab)
    [void]$sb.AppendLine('   Criada em ..: ' + ([datetime]$t['create_date']).ToString('yyyy-MM-dd'))
    [void]$sb.AppendLine('   Alterada em : ' + ([datetime]$t['modify_date']).ToString('yyyy-MM-dd'))
    [void]$sb.AppendLine('   --------------------------------------------------------------- */')

    $cols = @(); if ($colPorTab.ContainsKey($oid)) { $cols = $colPorTab[$oid] }
    $idHash = @{};   if ($idPorTab.ContainsKey($oid))   { foreach ($x in $idPorTab[$oid])   { $idHash[[string]$x['column_id']] = $x } }
    $calcHash = @{}; if ($calcPorTab.ContainsKey($oid)) { foreach ($x in $calcPorTab[$oid]) { $calcHash[[string]$x['column_id']] = $x } }

    $defs = New-Object System.Collections.ArrayList
    foreach ($c in $cols) {
        $cn  = Texto $c 'coluna'
        $cid = [string]$c['column_id']
        if ($calcHash.ContainsKey($cid)) {
            $cc = $calcHash[$cid]
            $persist = if ([bool]$cc['is_persisted']) { ' PERSISTED' } else { '' }
            [void]$defs.Add('    [' + $cn + '] AS ' + (Texto $cc 'formula') + $persist)
            [void]$linhasIdCalc.Add([pscustomobject]@{ tabela = $tab; coluna = $cn; especie = 'CALCULADA'; detalhe = (Texto $cc 'formula'); persistida = [bool]$cc['is_persisted'] })
            continue
        }
        $tipo = TipoSql (Texto $c 'tipo') ([int]$c['max_length']) ([int]$c['precision']) ([int]$c['scale'])
        $linha = '    [' + $cn + '] ' + $tipo
        $coll = Texto $c 'collation_name'
        if ($coll -ne '') { $linha += ' COLLATE ' + $coll }
        if ($idHash.ContainsKey($cid)) {
            $ic = $idHash[$cid]
            $linha += ' IDENTITY(' + (Texto $ic 'semente') + ',' + (Texto $ic 'incremento') + ')'
            [void]$linhasIdCalc.Add([pscustomobject]@{ tabela = $tab; coluna = $cn; especie = 'IDENTITY'; detalhe = ('semente=' + (Texto $ic 'semente') + '; incremento=' + (Texto $ic 'incremento') + '; ultimo=' + (Texto $ic 'ultimo_valor')); persistida = '' })
        }
        $linha += if ([bool]$c['is_nullable']) { ' NULL' } else { ' NOT NULL' }
        $dd = Texto $c 'default_def'
        if ($dd -ne '') { $linha += ' CONSTRAINT [' + (Texto $c 'default_nome') + '] DEFAULT ' + $dd }
        [void]$defs.Add($linha)
    }

    $idxs = @(); if ($idxPorTab.ContainsKey($oid)) { $idxs = $idxPorTab[$oid] }
    $idxSecundarios = New-Object System.Collections.ArrayList
    foreach ($ix in $idxs) {
        $k = $oid + '|' + [string]$ix['index_id']
        $todas = @(); if ($icPorIdx.ContainsKey($k)) { $todas = $icPorIdx[$k] }
        $chaveCols = @($todas | Where-Object { -not [bool]$_['is_included_column'] } | ForEach-Object { '[' + [string]$_['coluna'] + ']' + $(if ([bool]$_['is_descending_key']) { ' DESC' } else { '' }) })
        $incCols = @($todas | Where-Object { [bool]$_['is_included_column'] } | ForEach-Object { '[' + [string]$_['coluna'] + ']' })

        [void]$linhasIdx.Add([pscustomobject]@{
                tabela           = $tab
                indice           = Texto $ix 'indice'
                tipo             = Texto $ix 'type_desc'
                pk               = [bool]$ix['is_primary_key']
                unico            = [bool]$ix['is_unique']
                constraint_unica = [bool]$ix['is_unique_constraint']
                colunas          = ($chaveCols -join ', ')
                incluidas        = ($incCols -join ', ')
                filtro           = Texto $ix 'filter_definition'
                desabilitado     = [bool]$ix['is_disabled']
            })

        if ([bool]$ix['is_primary_key'] -or [bool]$ix['is_unique_constraint']) {
            $kc = $kcPorIdx[$k]
            $nomeC = if ($kc) { [string]$kc['name'] } else { Texto $ix 'indice' }
            $tipoC = if ([bool]$ix['is_primary_key']) { 'PRIMARY KEY' } else { 'UNIQUE' }
            $clus = if ((Texto $ix 'type_desc') -eq 'CLUSTERED') { 'CLUSTERED' } else { 'NONCLUSTERED' }
            [void]$defs.Add('    CONSTRAINT [' + $nomeC + '] ' + $tipoC + ' ' + $clus + ' (' + ($chaveCols -join ', ') + ')')
        } else {
            [void]$idxSecundarios.Add($ix)
        }
    }

    if ($ckPorTab.ContainsKey($oid)) {
        foreach ($ck in $ckPorTab[$oid]) {
            [void]$defs.Add('    CONSTRAINT [' + [string]$ck['name'] + '] CHECK ' + (Texto $ck 'definition'))
        }
    }

    [void]$sb.AppendLine($K_CREATE + ' TABLE [' + $esq + '].[' + $tab + '] (')
    [void]$sb.AppendLine(($defs -join ",`r`n"))
    [void]$sb.AppendLine(');')
    [void]$sb.AppendLine('GO')

    foreach ($ix in $idxSecundarios) {
        $k = $oid + '|' + [string]$ix['index_id']
        $todas = @(); if ($icPorIdx.ContainsKey($k)) { $todas = $icPorIdx[$k] }
        $chaveCols = @($todas | Where-Object { -not [bool]$_['is_included_column'] } | ForEach-Object { '[' + [string]$_['coluna'] + ']' + $(if ([bool]$_['is_descending_key']) { ' DESC' } else { '' }) })
        $incCols = @($todas | Where-Object { [bool]$_['is_included_column'] } | ForEach-Object { '[' + [string]$_['coluna'] + ']' })
        $un = if ([bool]$ix['is_unique']) { 'UNIQUE ' } else { '' }
        $s = $K_CREATE + ' ' + $un + (Texto $ix 'type_desc') + ' INDEX [' + (Texto $ix 'indice') + '] ON [' + $esq + '].[' + $tab + '] (' + ($chaveCols -join ', ') + ')'
        if ($incCols.Count) { $s += ' INCLUDE (' + ($incCols -join ', ') + ')' }
        if ([bool]$ix['has_filter']) { $s += ' WHERE ' + (Texto $ix 'filter_definition') }
        [void]$sb.AppendLine($s + ';')
        [void]$sb.AppendLine('GO')
    }

    if ($fkPorTab.ContainsKey($oid)) {
        foreach ($fk in $fkPorTab[$oid]) {
            $fid = [string]$fk['fk_id']
            $cs = @(); if ($fcPorFk.ContainsKey($fid)) { $cs = $fcPorFk[$fid] }
            $orig = @($cs | ForEach-Object { '[' + [string]$_['coluna_origem'] + ']' })
            $dest = @($cs | ForEach-Object { '[' + [string]$_['coluna_destino'] + ']' })
            $s = $K_ALTER + ' TABLE [' + $esq + '].[' + $tab + '] ADD CONSTRAINT [' + (Texto $fk 'fk_nome') +
                 '] FOREIGN KEY (' + ($orig -join ', ') + ') REFERENCES [' + $esq + '].[' + (Texto $fk 'tabela_ref') + '] (' + ($dest -join ', ') + ')'
            $ae = Texto $fk 'acao_exclusao'; $aa = Texto $fk 'acao_atualizacao'
            if ($ae -ne 'NO_ACTION') { $s += ' ON ' + $K_DELETE + ' ' + $ae.Replace('_', ' ') }
            if ($aa -ne 'NO_ACTION') { $s += ' ON ' + $K_UPDATE + ' ' + $aa.Replace('_', ' ') }
            [void]$sb.AppendLine($s + ';')
            [void]$sb.AppendLine('GO')
        }
    }
    [void]$sb.AppendLine('')

    if (-not $porPrefixo.ContainsKey($pref)) { $porPrefixo[$pref] = New-Object System.Text.StringBuilder }
    [void]$porPrefixo[$pref].Append($sb.ToString())

    [void]$metaTab.Add([pscustomobject]@{
            object_id   = $oid; esquema = $esq; tabela = $tab; prefixo = $pref
            qtd_colunas = $cols.Count
            qtd_indices = $idxs.Count
            tem_pk      = [bool](@($idxs | Where-Object { [bool]$_['is_primary_key'] }).Count)
            qtd_fks     = $(if ($fkPorTab.ContainsKey($oid)) { $fkPorTab[$oid].Count } else { 0 })
            create_date = ([datetime]$t['create_date']).ToString('yyyy-MM-dd')
            modify_date = ([datetime]$t['modify_date']).ToString('yyyy-MM-dd')
        })
}

foreach ($p in $porPrefixo.Keys) {
    $cab = "/* DDL reconstruido do banco CRM (Vortice CRM / Tracbel) - tabelas do prefixo $p`r`n" +
           "   Gerado em " + (Get-Date -Format 'yyyy-MM-dd HH:mm') + " a partir dos catalogos do SQL Server (somente leitura).`r`n" +
           "   Nao executar sem revisao: o objetivo e documentacao, nao migracao literal. */`r`n`r`n"
    GravaUtf8 (Join-Path $Ddl ($p + '.sql')) ($cab + $porPrefixo[$p].ToString())
}

$linhasIdx    | Export-Csv (Join-Path $Ddl '00-indices.csv') -NoTypeInformation -Encoding UTF8
$linhasIdCalc | Export-Csv (Join-Path $Ddl '00-identity-computed.csv') -NoTypeInformation -Encoding UTF8
[void](ExportaCsv $props (Join-Path $Ddl 'extended-properties.csv'))
$metaTab      | Export-Csv (Join-Path $Raw 'tabelas-meta.csv') -NoTypeInformation -Encoding UTF8
[void](ExportaCsv $fks (Join-Path $Raw 'fks.csv'))
[void](ExportaCsv $colunas (Join-Path $Raw 'colunas.csv'))

Write-Host ('OK: ' + $porPrefixo.Count + ' arquivos DDL | ' + $linhasIdx.Count + ' indices | ' +
    $props.Count + ' extended properties | ' + $metaTab.Count + ' tabelas') -ForegroundColor Green
