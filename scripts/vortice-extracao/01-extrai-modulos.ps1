<#
  01-extrai-modulos.ps1
  ---------------------
  Extrai o codigo-fonte dos 548 objetos programaveis do banco CRM (Vortice CRM / Tracbel):
  85 stored procedures, 51 functions, 1 trigger e 411 views.

  Saida:
    docs/extracao-vortice/modulos/procedures/<nome>.sql
    docs/extracao-vortice/modulos/functions/<nome>.sql
    docs/extracao-vortice/modulos/triggers/<nome>.sql
    docs/extracao-vortice/modulos/views/<nome>.sql
    docs/extracao-vortice/_raw/modulos-meta.csv    (metadados + flag de escrita)
    docs/extracao-vortice/_raw/dependencias.csv    (sys.sql_expression_dependencies)

  Somente leitura. Reexecutavel (sobrescreve).
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Mod = Join-Path $Global:VorticeRaiz 'modulos'
$Raw = Join-Path $Global:VorticeRaiz '_raw'

$mapaPasta = @{
    'SQL_STORED_PROCEDURE'             = 'procedures'
    'SQL_SCALAR_FUNCTION'              = 'functions'
    'SQL_INLINE_TABLE_VALUED_FUNCTION' = 'functions'
    'SQL_TABLE_VALUED_FUNCTION'        = 'functions'
    'SQL_TRIGGER'                      = 'triggers'
    'VIEW'                             = 'views'
}

Write-Host 'Baixando catalogos...' -ForegroundColor Cyan

$mods = Consulta -Rotulo 'sys.sql_modules' -Sql @'
SELECT o.object_id, s.name AS esquema, o.name AS nome, o.type_desc,
       o.create_date, o.modify_date, m.definition,
       CASE WHEN o.type = 'TR' THEN OBJECT_NAME(o.parent_object_id) ELSE NULL END AS tabela_pai
FROM sys.sql_modules m WITH (NOLOCK)
JOIN sys.objects  o WITH (NOLOCK) ON o.object_id = m.object_id
JOIN sys.schemas  s WITH (NOLOCK) ON s.schema_id = o.schema_id
ORDER BY o.type_desc, o.name
'@

# sys.sql_expression_dependencies NAO possui a coluna is_updated (essa existe apenas
# em sys.dm_sql_referenced_entities). Aqui pegamos so o grafo de referencias.
$deps = Consulta -Rotulo 'sys.sql_expression_dependencies' -Sql @'
SELECT d.referencing_id,
       ISNULL(d.referenced_schema_name, 'dbo') AS esquema_ref,
       d.referenced_entity_name              AS entidade_ref,
       d.referenced_id,
       d.referenced_class_desc,
       ISNULL(d.referenced_server_name, '')   AS servidor_ref,
       ISNULL(d.referenced_database_name, '') AS banco_ref
FROM sys.sql_expression_dependencies d WITH (NOLOCK)
WHERE d.referenced_entity_name IS NOT NULL
'@

# Conjunto de nomes de tabelas reais, para separar "referencia a tabela" de "referencia a view/funcao".
$tabelasReais = @{}
foreach ($t in (Consulta -Rotulo 'sys.tables' -Sql "SELECT name FROM sys.tables WITH (NOLOCK)")) {
    $tabelasReais[[string]$t['name']] = $true
}

$depPorObj = @{}
foreach ($d in $deps) {
    $id = [string]$d['referencing_id']
    if (-not $depPorObj.ContainsKey($id)) { $depPorObj[$id] = New-Object System.Collections.ArrayList }
    [void]$depPorObj[$id].Add([string]$d['entidade_ref'])
}

Write-Host 'Gravando arquivos e analisando o texto...' -ForegroundColor Cyan

# Palavras de escrita procuradas no TEXTO JA BAIXADO (a analise e local, nunca no servidor).
# ATENCAO: os parenteses sao obrigatorios - no PowerShell o operador virgula tem
# precedencia MAIOR que o '+', entao @('A'+'B', 'C'+'D') vira uma unica string errada.
$verbosEscrita = @(('INS' + 'ERT'), ('UPD' + 'ATE'), ('DEL' + 'ETE'), ('MER' + 'GE'), ('TRUNC' + 'ATE'))
$verboExec     = 'EX' + 'EC'

$meta      = New-Object System.Collections.ArrayList
$invalidos = [System.IO.Path]::GetInvalidFileNameChars()
$gravados  = 0

foreach ($m in $mods) {
    $tipo  = Texto $m 'type_desc'
    $pasta = $mapaPasta[$tipo]
    if (-not $pasta) { $pasta = 'outros' }

    $nome    = Texto $m 'nome'
    $arqNome = $nome
    foreach ($c in $invalidos) { $arqNome = $arqNome.Replace($c, '_') }

    $id   = [string]$m['object_id']
    $refs = @()
    if ($depPorObj.ContainsKey($id)) { $refs = @($depPorObj[$id] | Sort-Object -Unique) }
    $refsTabela = @($refs | Where-Object { $tabelasReais.ContainsKey($_) })

    $def    = Texto $m 'definition'
    $linhas = ($def -split "`n").Count

    # --- deteccao de escrita e de alvos, sobre o texto local ---
    $escreve = $false
    $verbosAchados = New-Object System.Collections.ArrayList
    foreach ($v in $verbosEscrita) {
        if ([regex]::IsMatch($def, '\b' + $v + '\b', 'IgnoreCase')) {
            $escreve = $true
            [void]$verbosAchados.Add($v)
        }
    }
    $chamaProc = [regex]::IsMatch($def, '\b' + $verboExec + '(UTE)?\b', 'IgnoreCase')

    # tabelas efetivamente gravadas (padrao "VERBO [INTO] <tabela>")
    $alvos = New-Object System.Collections.ArrayList
    foreach ($mt in [regex]::Matches($def, '(?is)\b(ins' + 'ert\s+into|upd' + 'ate|del' + 'ete\s+from|mer' + 'ge\s+into|trunc' + 'ate\s+table)\s+\[?([A-Za-z0-9_\.]+)\]?')) {
        $alvo = $mt.Groups[2].Value -replace '^dbo\.', ''
        if ($tabelasReais.ContainsKey($alvo) -and -not $alvos.Contains($alvo)) { [void]$alvos.Add($alvo) }
    }

    $cab = New-Object System.Collections.ArrayList
    [void]$cab.Add('/* ==============================================================')
    [void]$cab.Add('   Objeto ..........: ' + (Texto $m 'esquema') + '.' + $nome)
    [void]$cab.Add('   Tipo ............: ' + $tipo)
    if ((Texto $m 'tabela_pai') -ne '') { [void]$cab.Add('   Tabela pai ......: ' + (Texto $m 'tabela_pai')) }
    [void]$cab.Add('   Criado em .......: ' + ([datetime]$m['create_date']).ToString('yyyy-MM-dd HH:mm:ss'))
    [void]$cab.Add('   Modificado em ...: ' + ([datetime]$m['modify_date']).ToString('yyyy-MM-dd HH:mm:ss'))
    [void]$cab.Add('   Linhas ..........: ' + $linhas)
    [void]$cab.Add('   Escreve em tabela: ' + $(if ($escreve) { 'SIM (' + ($verbosAchados -join ', ') + ')' } else { 'nao' }))
    if ($alvos.Count) { [void]$cab.Add('   Alvos de escrita : ' + ($alvos -join ', ')) }
    [void]$cab.Add('   Tabelas referidas: ' + $(if ($refsTabela.Count) { ($refsTabela -join ', ') } else { '(nenhuma)' }))
    $outras = @($refs | Where-Object { -not $tabelasReais.ContainsKey($_) })
    if ($outras.Count) { [void]$cab.Add('   Outras refs .....: ' + ($outras -join ', ')) }
    [void]$cab.Add('   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura')
    [void]$cab.Add('   ============================================================== */')
    [void]$cab.Add('')

    GravaUtf8 (Join-Path (Join-Path $Mod $pasta) ($arqNome + '.sql')) (($cab -join "`r`n") + "`r`n" + $def)
    $gravados++

    [void]$meta.Add([pscustomobject]@{
            object_id     = $id
            esquema       = Texto $m 'esquema'
            nome          = $nome
            tipo          = $tipo
            pasta         = $pasta
            tabela_pai    = Texto $m 'tabela_pai'
            create_date   = ([datetime]$m['create_date']).ToString('yyyy-MM-dd HH:mm:ss')
            modify_date   = ([datetime]$m['modify_date']).ToString('yyyy-MM-dd HH:mm:ss')
            linhas        = $linhas
            caracteres    = $def.Length
            escreve       = $escreve
            verbos_escrita = ($verbosAchados -join ' ')
            alvos_escrita = ($alvos -join '; ')
            chama_proc    = $chamaProc
            qtd_refs      = $refs.Count
            tabelas_ref   = ($refsTabela -join '; ')
            outras_ref    = ($outras -join '; ')
        })
}

$meta | Export-Csv (Join-Path $Raw 'modulos-meta.csv') -NoTypeInformation -Encoding UTF8
[void](ExportaCsv $deps (Join-Path $Raw 'dependencias.csv'))

Write-Host ('OK: ' + $gravados + ' arquivos .sql gravados; ' +
    @($meta | Where-Object { $_.escreve }).Count + ' objetos escrevem em tabela.') -ForegroundColor Green
