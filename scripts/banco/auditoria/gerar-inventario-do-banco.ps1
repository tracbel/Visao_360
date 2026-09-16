<#
  gerar-inventario-do-banco.ps1 - inventario SOMENTE LEITURA do banco cruzado com o codigo.
  Documento 39.

      .\scripts\banco\auditoria\gerar-inventario-do-banco.ps1

  O QUE LE
    - banco local de desenvolvimento (conteiner tracbel-crm-db, banco TracbelCrm): tabelas, linhas,
      colunas, chave primaria, chaves estrangeiras, indices, CHECK, defaults, IDENTITY e particionamento.
      A estrutura e a mesma do banco central (mesmas 12 migracoes); so SELECT em catalogo de sistema.
    - codigo: mapeamento EF (ToTable), DbSets, migracoes (quem criou e quem alterou cada tabela) e uso
      por camada - acesso por DbSet, Set<T>, SQL em texto e mencao ao tipo da entidade -, inclusive testes
      e scripts.

  O QUE GRAVA
    - docs/projeto/39A-INVENTARIO-DETALHADO-DO-BANCO.md  (uma secao por tabela)
    - dados-locais/auditoria/inventario-do-banco.json    (o mesmo, para analise; fora do Git)

  NAO ALTERA NADA no banco nem no codigo.
#>

[CmdletBinding()]
param(
    [string] $Saida = 'docs\projeto\39A-INVENTARIO-DETALHADO-DO-BANCO.md',
    [string] $Dados = 'dados-locais\auditoria\inventario-do-banco.json'
)

$ErrorActionPreference = 'Stop'
$raiz = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

# ------------------------------------------------------------------------------------------------
# 1. Banco (somente catalogo de sistema)
# ------------------------------------------------------------------------------------------------
$config = @{}
foreach ($l in Get-Content (Join-Path $raiz 'infra\.env')) {
    $t = $l.Trim(); if ($t -eq '' -or $t.StartsWith('#')) { continue }
    $p = $t.Split('=', 2); if ($p.Count -eq 2) { $config[$p[0].Trim()] = $p[1].Trim() }
}

function Consultar([string] $sql) {
    $linhas = & docker exec -e "SQLCMDPASSWORD=$($config.DB_SENHA)" tracbel-crm-db /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -C -b -y 0 -d $config.DB_NOME -Q ("SET NOCOUNT ON; " + $sql) 2>&1
    if ($LASTEXITCODE -ne 0) { throw ("Consulta ao catalogo falhou: " + (($linhas | Select-Object -First 3) -join ' ')) }
    return @($linhas | ForEach-Object { "$_".Trim() } | Where-Object { $_ -match '^[A-Z]\|' })
}

$limpar = { param($coluna) "REPLACE(REPLACE(REPLACE(ISNULL(CONVERT(nvarchar(max), $coluna), N''), N'|', N'/'), CHAR(10), N' '), CHAR(13), N' ')" }

$qTabelas = @"
SELECT CONCAT(N'T|', s.name, N'.', t.name, N'|',
    (SELECT SUM(p.rows) FROM sys.partitions p WHERE p.object_id = t.object_id AND p.index_id IN (0,1)), N'|',
    CONVERT(varchar(19), t.create_date, 120))
FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id;
"@

$qColunas = @"
SELECT CONCAT(N'C|', s.name, N'.', t.name, N'|', c.column_id, N'|', c.name, N'|',
    TYPE_NAME(c.user_type_id) +
    CASE WHEN TYPE_NAME(c.user_type_id) IN ('varchar','char','varbinary','binary') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length AS varchar(10)) END + ')'
         WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar','nchar') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length / 2 AS varchar(10)) END + ')'
         WHEN TYPE_NAME(c.user_type_id) IN ('decimal','numeric') THEN '(' + CAST(c.precision AS varchar(3)) + ',' + CAST(c.scale AS varchar(3)) + ')'
         WHEN TYPE_NAME(c.user_type_id) IN ('datetime2','time','datetimeoffset') THEN '(' + CAST(c.scale AS varchar(3)) + ')'
         ELSE '' END, N'|',
    c.is_nullable, N'|', c.is_identity, N'|', c.is_computed, N'|', $(& $limpar 'dc.definition'))
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id;
"@

$qFks = @"
SELECT CONCAT(N'F|', fk.name COLLATE DATABASE_DEFAULT, N'|', OBJECT_SCHEMA_NAME(fk.parent_object_id) COLLATE DATABASE_DEFAULT, N'.', OBJECT_NAME(fk.parent_object_id) COLLATE DATABASE_DEFAULT, N'|',
    (SELECT STRING_AGG(COL_NAME(x.parent_object_id, x.parent_column_id) COLLATE DATABASE_DEFAULT, ',') WITHIN GROUP (ORDER BY x.constraint_column_id) FROM sys.foreign_key_columns x WHERE x.constraint_object_id = fk.object_id), N'|',
    OBJECT_SCHEMA_NAME(fk.referenced_object_id) COLLATE DATABASE_DEFAULT, N'.', OBJECT_NAME(fk.referenced_object_id) COLLATE DATABASE_DEFAULT, N'|',
    (SELECT STRING_AGG(COL_NAME(x.referenced_object_id, x.referenced_column_id) COLLATE DATABASE_DEFAULT, ',') WITHIN GROUP (ORDER BY x.constraint_column_id) FROM sys.foreign_key_columns x WHERE x.constraint_object_id = fk.object_id), N'|',
    (SELECT MAX(CAST(c.is_nullable AS int)) FROM sys.foreign_key_columns x JOIN sys.columns c ON c.object_id = x.parent_object_id AND c.column_id = x.parent_column_id WHERE x.constraint_object_id = fk.object_id), N'|',
    fk.delete_referential_action_desc COLLATE DATABASE_DEFAULT)
FROM sys.foreign_keys fk;
"@

$qIndices = @"
SELECT CONCAT(N'I|', s.name COLLATE DATABASE_DEFAULT, N'.', t.name COLLATE DATABASE_DEFAULT, N'|', i.name COLLATE DATABASE_DEFAULT, N'|', i.type_desc COLLATE DATABASE_DEFAULT, N'|', i.is_unique, N'|', i.is_primary_key, N'|', i.is_unique_constraint, N'|',
    $(& $limpar 'i.filter_definition') COLLATE DATABASE_DEFAULT, N'|',
    (SELECT STRING_AGG(COL_NAME(ic.object_id, ic.column_id) COLLATE DATABASE_DEFAULT + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE '' END, ',') WITHIN GROUP (ORDER BY ic.key_ordinal) FROM sys.index_columns ic WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0), N'|',
    (SELECT STRING_AGG(COL_NAME(ic.object_id, ic.column_id) COLLATE DATABASE_DEFAULT, ',') FROM sys.index_columns ic WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1))
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE i.index_id > 0;
"@

$qChecks = @"
SELECT CONCAT(N'X|', OBJECT_SCHEMA_NAME(cc.parent_object_id), N'.', OBJECT_NAME(cc.parent_object_id), N'|', cc.name, N'|', $(& $limpar 'cc.definition'))
FROM sys.check_constraints cc;
"@

$qParticoes = @"
SELECT CONCAT(N'P|', OBJECT_SCHEMA_NAME(i.object_id), N'.', OBJECT_NAME(i.object_id), N'|', ps.name)
FROM sys.indexes i JOIN sys.partition_schemes ps ON ps.data_space_id = i.data_space_id
WHERE i.index_id IN (0,1);
"@

Write-Host 'Lendo o catalogo do banco local (somente SELECT)...' -ForegroundColor Cyan
$tabelas = [ordered]@{}
foreach ($l in Consultar $qTabelas) {
    $c = $l.Split('|')
    $tabelas[$c[1]] = [ordered]@{
        Tabela = $c[1]; Schema = $c[1].Split('.')[0]; Nome = $c[1].Split('.')[1]; Linhas = [long]$c[2]; CriadaNoBanco = $c[3]
        Colunas = [System.Collections.Generic.List[object]]::new(); Indices = [System.Collections.Generic.List[object]]::new()
        Checks = [System.Collections.Generic.List[object]]::new(); FksSaida = [System.Collections.Generic.List[object]]::new()
        FksEntrada = [System.Collections.Generic.List[object]]::new(); Particionada = ''
        Entidade = ''; DbSet = ''; Configuracao = ''; MigracaoQueCriou = ''; MigracoesQueAlteraram = [System.Collections.Generic.List[string]]::new()
        Uso = [ordered]@{}; ArquivosComUso = [System.Collections.Generic.List[string]]::new(); CamposSuspeitos = [System.Collections.Generic.List[string]]::new()
    }
}
foreach ($l in Consultar $qColunas) {
    $c = $l.Split('|')
    $tabelas[$c[1]].Colunas.Add([ordered]@{ Ordem = [int]$c[2]; Nome = $c[3]; Tipo = $c[4]; Nulo = $c[5] -eq '1'; Identity = $c[6] -eq '1'; Computada = $c[7] -eq '1'; Default = $c[8] })
}
foreach ($t in $tabelas.Values) { $t.Colunas = @($t.Colunas | Sort-Object { $_.Ordem }) }
foreach ($l in Consultar $qFks) {
    $c = $l.Split('|')
    $fk = [ordered]@{ Nome = $c[1]; Origem = $c[2]; Colunas = $c[3]; Destino = $c[4]; ColunasDestino = $c[5]; Opcional = $c[6] -eq '1'; AoApagar = $c[7] }
    $tabelas[$c[2]].FksSaida.Add($fk)
    $tabelas[$c[4]].FksEntrada.Add($fk)
}
foreach ($l in Consultar $qIndices) {
    $c = $l.Split('|')
    $tabelas[$c[1]].Indices.Add([ordered]@{ Nome = $c[2]; Tipo = $c[3]; Unico = $c[4] -eq '1'; Pk = $c[5] -eq '1'; RestricaoUnica = $c[6] -eq '1'; Filtro = $c[7]; Chave = $c[8]; Inclui = $c[9] })
}
foreach ($l in Consultar $qChecks) {
    $c = $l.Split('|', 4)
    $tabelas[$c[1]].Checks.Add([ordered]@{ Nome = $c[2]; Definicao = $c[3] })
}
foreach ($l in Consultar $qParticoes) { $c = $l.Split('|'); $tabelas[$c[1]].Particionada = $c[2] }

# ------------------------------------------------------------------------------------------------
# 2. Mapeamento EF Core: configuracoes (ToTable) e DbSets
# ------------------------------------------------------------------------------------------------
Write-Host 'Lendo o mapeamento do EF Core...' -ForegroundColor Cyan
$persistencia = Join-Path $raiz 'src\Tracbel.Crm.Infraestrutura\Persistencia'
foreach ($arq in Get-ChildItem $persistencia -Recurse -Filter *.cs) {
    $linhas = Get-Content $arq.FullName
    $entidade = $null
    foreach ($linha in $linhas) {
        if ($linha -match 'IEntityTypeConfiguration<([\w\.]+)>') { $entidade = ($Matches[1] -split '\.')[-1] }
        elseif ($linha -match 'EntityTypeBuilder<([\w\.]+)>' -and $Matches[1] -ne 'T') { $entidade = ($Matches[1] -split '\.')[-1] }
        if ($linha -match 'ToTable\("(\w+)",\s*"(\w+)"') {
            $nome = "$($Matches[2]).$($Matches[1])"
            if ($tabelas.Contains($nome)) {
                $tabelas[$nome].Entidade = $entidade
                $tabelas[$nome].Configuracao = $arq.FullName.Substring($raiz.Length + 1).Replace('\', '/')
            }
        }
    }
}
$dbsets = @{}
foreach ($m in [regex]::Matches((Get-Content (Join-Path $persistencia 'CrmDbContext.cs') -Raw), 'DbSet<([\w\.]+)>\s+(\w+)')) {
    $dbsets[($m.Groups[1].Value -split '\.')[-1]] = $m.Groups[2].Value
}
foreach ($t in $tabelas.Values) { if ($t.Entidade -and $dbsets.ContainsKey($t.Entidade)) { $t.DbSet = $dbsets[$t.Entidade] } }

# ------------------------------------------------------------------------------------------------
# 3. Migracoes: quem criou e quem alterou (so o Up)
# ------------------------------------------------------------------------------------------------
Write-Host 'Lendo as migracoes...' -ForegroundColor Cyan
$schemas = 'organizacao|seguranca|comercial|processo|frota|documento|auditoria|integracao|metadado|relatorio'
$migracoes = Get-ChildItem (Join-Path $raiz 'src\Tracbel.Crm.Infraestrutura\Migrations') -Filter '2026*.cs' | Where-Object { $_.Name -notlike '*.Designer.cs' } | Sort-Object Name
$resumoDasMigracoes = [System.Collections.Generic.List[object]]::new()
foreach ($mig in $migracoes) {
    $nomeMig = [IO.Path]::GetFileNameWithoutExtension($mig.Name)
    $texto = Get-Content $mig.FullName -Raw
    $up = ($texto -split 'protected override void Down')[0]
    $porTabela = @{}
    foreach ($op in [regex]::Matches($up, '(?s)migrationBuilder\s*\.\s*(\w+)\((.*?)\);')) {
        $metodo = $op.Groups[1].Value; $argumentos = $op.Groups[2].Value
        $alvos = @()
        if ($metodo -eq 'Sql') {
            foreach ($s in [regex]::Matches($argumentos, "\b($schemas)\.(\w+)\b")) { $alvos += "$($s.Groups[1].Value).$($s.Groups[2].Value)" }
        } elseif ($metodo -in 'CreateTable', 'DropTable', 'RenameTable') {
            if ($argumentos -match 'name:\s*"(\w+)"' ) { $n = $Matches[1]; if ($argumentos -match 'schema:\s*"(\w+)"') { $alvos += "$($Matches[1]).$n" } }
        } else {
            if ($argumentos -match 'table:\s*"(\w+)"') { $n = $Matches[1]; if ($argumentos -match 'schema:\s*"(\w+)"') { $alvos += "$($Matches[1]).$n" } }
        }
        $soDominioDeEntidade = ($metodo -in 'DropCheckConstraint', 'AddCheckConstraint') -and ($argumentos -match 'name:\s*"CK_\w+_(Entidade|EntidadeRaiz)"')
        foreach ($a in ($alvos | Select-Object -Unique)) {
            if (-not $porTabela.ContainsKey($a)) { $porTabela[$a] = [System.Collections.Generic.List[string]]::new() }
            $porTabela[$a].Add($(if ($soDominioDeEntidade) { 'CK_Entidade' } else { $metodo }))
        }
    }
    foreach ($a in $porTabela.Keys) {
        if (-not $tabelas.Contains($a)) { continue }
        $ops = $porTabela[$a] | Group-Object | ForEach-Object { if ($_.Count -gt 1) { "$($_.Name) x$($_.Count)" } else { $_.Name } }
        if ($porTabela[$a] -contains 'CreateTable') { $tabelas[$a].MigracaoQueCriou = $nomeMig }
        else { $tabelas[$a].MigracoesQueAlteraram.Add("$nomeMig (" + ($ops -join ', ') + ')') }
    }
    $resumoDasMigracoes.Add([ordered]@{
        Migracao = $nomeMig
        Criou = @($porTabela.Keys | Where-Object { $porTabela[$_] -contains 'CreateTable' } | Sort-Object)
        Alterou = @($porTabela.Keys | Where-Object { $porTabela[$_] -notcontains 'CreateTable' -and ($porTabela[$_] | Where-Object { $_ -ne 'CK_Entidade' }) } | Sort-Object)
        SoRestricaoDeEntidade = @($porTabela.Keys | Where-Object { -not ($porTabela[$_] | Where-Object { $_ -ne 'CK_Entidade' }) } | Sort-Object)
    })
}

# ------------------------------------------------------------------------------------------------
# 4. Uso no codigo, por camada
# ------------------------------------------------------------------------------------------------
Write-Host 'Varrendo o codigo (src, tests, scripts)...' -ForegroundColor Cyan
function Camada([string] $caminho) {
    $r = $caminho.Substring($raiz.Length + 1).Replace('\', '/')
    switch -Regex ($r) {
        '^src/Tracbel\.Crm\.Dominio/'                                   { return 'Dominio' }
        '^src/Tracbel\.Crm\.Aplicacao/'                                 { return 'Aplicacao' }
        '^src/Tracbel\.Crm\.Infraestrutura/Persistencia/Configuracoes/' { return 'ConfiguracaoEF' }
        '^src/Tracbel\.Crm\.Infraestrutura/Persistencia/Repositorios/'  { return 'Repositorio' }
        '^src/Tracbel\.Crm\.Infraestrutura/Persistencia/CrmDbContext'   { return 'DbContext' }
        '^src/Tracbel\.Crm\.Infraestrutura/'                            { return 'InfraOutros' }
        '^src/Tracbel\.Crm\.Api/'                                       { return 'Api' }
        '^src/Tracbel\.Crm\.Carga/'                                     { return 'Carga' }
        '^src/Tracbel\.Crm\.Integracao/'                                { return 'Integracao' }
        '^tests/'                                                       { return 'Testes' }
        '^scripts/'                                                     { return 'Scripts' }
        default                                                         { return 'Outros' }
    }
}
$arquivos = @()
$arquivos += Get-ChildItem (Join-Path $raiz 'src') -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(bin|obj|Migrations)\\' }
$arquivos += Get-ChildItem (Join-Path $raiz 'tests') -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
$arquivos += Get-ChildItem (Join-Path $raiz 'scripts') -Recurse -Include *.sql, *.ps1 | Where-Object { $_.FullName -notmatch '\\node_modules\\' -and $_.FullName -notmatch 'gerar-inventario-do-banco' }
$conteudos = foreach ($a in $arquivos) { [pscustomobject]@{ Caminho = $a.FullName; Rel = $a.FullName.Substring($raiz.Length + 1).Replace('\', '/'); Camada = (Camada $a.FullName); Texto = [IO.File]::ReadAllText($a.FullName) } }

$camadas = 'Dominio', 'Aplicacao', 'Repositorio', 'InfraOutros', 'Api', 'Carga', 'Integracao', 'Testes', 'Scripts'
foreach ($t in $tabelas.Values) {
    $padroesFortes = @()
    if ($t.DbSet) { $padroesFortes += '\.' + [regex]::Escape($t.DbSet) + '\b' }
    if ($t.Entidade) { $padroesFortes += 'Set<(?:[\w\.]+\.)?' + [regex]::Escape($t.Entidade) + '>' }
    $padroesFortes += '\b' + [regex]::Escape($t.Schema) + '\.' + [regex]::Escape($t.Nome) + '\b'
    $padroesFortes += '\[' + [regex]::Escape($t.Schema) + '\]\.\[' + [regex]::Escape($t.Nome) + '\]'
    $forte = [regex]::new(($padroesFortes -join '|'))
    $fraco = if ($t.Entidade) { [regex]::new('\b' + [regex]::Escape($t.Entidade) + '\b') } else { $null }
    foreach ($c in $camadas) { $t.Uso[$c] = [ordered]@{ Acesso = 0; ArquivosComAcesso = 0; MencaoAoTipo = 0 } }
    foreach ($arq in $conteudos) {
        if ($arq.Camada -notin $camadas) { continue }
        $nForte = $forte.Matches($arq.Texto).Count
        if ($arq.Camada -eq 'DbContext') { $nForte = 0 }
        if ($nForte -gt 0) {
            $t.Uso[$arq.Camada].Acesso += $nForte
            $t.Uso[$arq.Camada].ArquivosComAcesso += 1
            $t.ArquivosComUso.Add("$($arq.Rel) ($nForte)")
        }
        if ($fraco) { $t.Uso[$arq.Camada].MencaoAoTipo += $fraco.Matches($arq.Texto).Count }
    }
}

# Campos suspeitos de duplicacao: texto ao lado de chave para o mesmo conceito, e nomes quase iguais.
foreach ($t in $tabelas.Values) {
    $nomes = $t.Colunas | ForEach-Object { $_.Nome }
    foreach ($col in $nomes) {
        if ($col -match '^(\w+)Id$') {
            $base = $Matches[1]
            foreach ($par in @($base, "${base}Nome", "${base}Codigo", "${base}Descricao", "${base}Texto", "NomeDo$base", "NomeDa$base", "${base}NaOrigem")) {
                if ($par -ne $col -and $nomes -contains $par) { $t.CamposSuspeitos.Add("$col + $par") }
            }
        }
    }
}

# ------------------------------------------------------------------------------------------------
# 5. Saida
# ------------------------------------------------------------------------------------------------
$jsonArq = Join-Path $raiz $Dados
New-Item -ItemType Directory -Force -Path (Split-Path $jsonArq) | Out-Null
[ordered]@{ Tabelas = @($tabelas.Values); Migracoes = @($resumoDasMigracoes) } | ConvertTo-Json -Depth 8 | Set-Content -Path $jsonArq -Encoding utf8

$sb = [System.Text.StringBuilder]::new()
function L([string] $texto = '') { [void]$sb.AppendLine($texto) }
function Cel([string] $texto) { return ($texto -replace '\|', '\|') }

L '# Inventário detalhado do banco — anexo do documento 39'
L ''
L ('> Gerado por `scripts/banco/auditoria/gerar-inventario-do-banco.ps1` em ' + (Get-Date -Format 'dd/MM/yyyy HH:mm') + ', somente leitura.')
L '> Fonte: catálogo do banco local `TracbelCrm` (mesma estrutura do banco central: 12 migrações) e o código do repositório.'
L '> "Acesso" conta referências por DbSet, `Set<T>` e SQL em texto (`schema.Tabela`); "menção ao tipo" conta o nome da entidade'
L '> como palavra e é evidência fraca (nomes como `Resultado`, `Fase` e `Meta` colidem com outros tipos). Linhas: banco local'
L '> sanitizado; o banco central tem os mesmos números, salvo `frota.Familia` (22) e `seguranca.Usuario` (2).'
L ''
L '## Migrações em ordem'
L ''
L '| Migração | Criou | Alterou (além da restrição de domínio de entidade) |'
L '|---|---|---|'
foreach ($m in $resumoDasMigracoes) { L ('| `' + $m.Migracao + '` | ' + (($m.Criou | ForEach-Object { '`' + $_ + '`' }) -join ', ') + ' | ' + (($m.Alterou | ForEach-Object { '`' + $_ + '`' }) -join ', ') + ' |') }
L ''
L '## Resumo por tabela'
L ''
L '| Tabela | Entidade | DbSet | Col. | Nulas | FK saída | FK entrada | Índices | CHECK | Identity | Linhas | Acesso (repositório/aplicação/API/carga/integração/infra) | Testes |'
L '|---|---|---|---:|---:|---:|---:|---:|---:|:-:|---:|---|---:|'
foreach ($t in @($tabelas.Values | Sort-Object { $_.Tabela })) {
    $id = if ($t.Colunas | Where-Object { $_.Identity }) { 'sim' } else { '' }
    $u = $t.Uso
    L ("| ``$($t.Tabela)`` | $($t.Entidade) | $($t.DbSet) | $($t.Colunas.Count) | $(@($t.Colunas | Where-Object { $_.Nulo }).Count) | $($t.FksSaida.Count) | $($t.FksEntrada.Count) | $($t.Indices.Count) | $($t.Checks.Count) | $id | $($t.Linhas) | $($u.Repositorio.Acesso)/$($u.Aplicacao.Acesso)/$($u.Api.Acesso)/$($u.Carga.Acesso)/$($u.Integracao.Acesso)/$($u.InfraOutros.Acesso) | $($u.Testes.Acesso + $u.Testes.MencaoAoTipo) |")
}
L ''

foreach ($t in @($tabelas.Values | Sort-Object { $_.Tabela })) {
    L "## ``$($t.Tabela)``"
    L ''
    $pk = ($t.Indices | Where-Object { $_.Pk } | ForEach-Object { $_.Chave }) -join '; '
    L "- **Entidade EF:** $(if ($t.Entidade) { '`' + $t.Entidade + '`' } else { '— (sem entidade)' }) · **DbSet:** $(if ($t.DbSet) { '`' + $t.DbSet + '`' } else { '— (sem DbSet)' }) · **Configuração:** $(if ($t.Configuracao) { '`' + $t.Configuracao + '`' } else { '—' })"
    L "- **Linhas:** $($t.Linhas) · **Chave primária:** ``$pk`` · **Identity:** $(if ($t.Colunas | Where-Object { $_.Identity }) { (($t.Colunas | Where-Object { $_.Identity }).Nome -join ', ') } else { 'não' })$(if ($t.Particionada) { ' · **Particionada:** `' + $t.Particionada + '`' })"
    L "- **Migração que criou:** $(if ($t.MigracaoQueCriou) { '`' + $t.MigracaoQueCriou + '`' } else { '—' })"
    if ($t.MigracoesQueAlteraram.Count) { L ('- **Migrações que alteraram:** ' + (($t.MigracoesQueAlteraram | ForEach-Object { '`' + $_ + '`' }) -join '; ')) }
    if ($t.CamposSuspeitos.Count) { L ('- **Campos a conferir (possível duplicação):** ' + (($t.CamposSuspeitos | ForEach-Object { '`' + $_ + '`' }) -join ', ')) }
    L ''
    L '| # | Coluna | Tipo | Nulo | Identity | Default |'
    L '|---:|---|---|:-:|:-:|---|'
    foreach ($c in $t.Colunas) { L ("| $($c.Ordem) | ``$($c.Nome)`` | $($c.Tipo) | $(if ($c.Nulo) { 'sim' }) | $(if ($c.Identity) { 'sim' }) | $(Cel $c.Default) |") }
    L ''
    if ($t.FksSaida.Count) {
        L '**Referencia (FK de saída):**'
        L ''
        # @(...) em todo foreach sobre pipeline: um único dicionário ordenado seria enumerado
        # pelas suas entradas (chave/valor), e não como um objeto.
        foreach ($f in @($t.FksSaida | Sort-Object { $_.Destino })) { L ("- ``$($t.Nome).$($f.Colunas)`` → ``$($f.Destino).$($f.ColunasDestino)``$(if ($f.Opcional) { ' (opcional)' } else { ' (obrigatória)' })$(if ($f.AoApagar -ne 'NO_ACTION') { ' · ' + $f.AoApagar })") }
        L ''
    }
    if ($t.FksEntrada.Count) {
        L '**Dependem dela (FK de entrada):**'
        L ''
        foreach ($f in @($t.FksEntrada | Sort-Object { $_.Origem })) { L ("- ``$($f.Origem).$($f.Colunas)``$(if ($f.Opcional) { ' (opcional)' })") }
        L ''
    }
    L '**Índices:**'
    L ''
    foreach ($i in @($t.Indices | Sort-Object { -[int]$_.Pk }, { $_.Nome })) { L ("- ``$($i.Nome)`` ($($i.Tipo.ToLower())$(if ($i.Unico) { ', único' })): $($i.Chave)$(if ($i.Inclui) { ' inclui ' + $i.Inclui })$(if ($i.Filtro) { ' · filtro ' + $i.Filtro })") }
    L ''
    if ($t.Checks.Count) {
        L '**Restrições CHECK:**'
        L ''
        foreach ($x in @($t.Checks | Sort-Object { $_.Nome })) {
            $def = $x.Definicao; if ($def.Length -gt 300) { $def = $def.Substring(0, 300) + ' …' }
            L ("- ``$($x.Nome)``: $(Cel $def)")
        }
        L ''
    }
    L '**Uso no código** (acesso · arquivos com acesso · menção ao tipo):'
    L ''
    L ('- ' + (($camadas | ForEach-Object { "$_ $($t.Uso[$_].Acesso)·$($t.Uso[$_].ArquivosComAcesso)·$($t.Uso[$_].MencaoAoTipo)" }) -join ' | '))
    if ($t.ArquivosComUso.Count) { L ('- Arquivos com acesso: ' + (($t.ArquivosComUso | Select-Object -First 12 | ForEach-Object { '`' + $_ + '`' }) -join ', ') + $(if ($t.ArquivosComUso.Count -gt 12) { " e mais $($t.ArquivosComUso.Count - 12)" })) }
    L ''
}

$md = Join-Path $raiz $Saida
[IO.File]::WriteAllText($md, $sb.ToString(), (New-Object Text.UTF8Encoding($false)))
Write-Host "Inventario gravado: $Saida" -ForegroundColor Green
Write-Host "Dados para analise: $Dados" -ForegroundColor Green

# ------------------------------------------------------------------------------------------------
# 6. Diagrama ER por dominio (Mermaid), gerado das chaves estrangeiras reais
# ------------------------------------------------------------------------------------------------
$dominios = [ordered]@{
    'IDENTIDADE E ACESSO'                             = @('seguranca.*')
    'ORGANIZACAO E TERRITORIO'                        = @('organizacao.*')
    'CRM - CLIENTE E RELACIONAMENTO'                  = @('comercial.Cliente', 'comercial.Contato', 'comercial.ClienteContato', 'comercial.CanalContato', 'comercial.Endereco', 'comercial.ConsentimentoComunicacao', 'comercial.ClienteCarteira', 'comercial.Lead', 'comercial.Alerta')
    'COMERCIAL - PIPELINE, ATIVIDADES E FATURAMENTO'  = @('processo.*', 'comercial.FaturamentoDoCliente', 'comercial.FaturamentoSemCliente')
    'FROTA'                                           = @('frota.*')
    'INTEGRACOES'                                     = @('integracao.*')
    'EXTENSIBILIDADE E RELATORIOS'                    = @('metadado.*', 'relatorio.*')
    'AUDITORIA E DOCUMENTOS'                          = @('auditoria.*', 'documento.*')
}
$hubs = 'organizacao.Empresa', 'seguranca.Usuario'
# O diagrama lê o JSON recém-gravado: objetos simples, sem as armadilhas de enumeração dos
# dicionários ordenados usados acima.
$dadosEr = Get-Content $jsonArq -Raw | ConvertFrom-Json
$porTabela = @{}
foreach ($t in $dadosEr.Tabelas) { $porTabela[$t.Tabela] = $t }
$dominioDe = @{}
foreach ($d in $dominios.Keys) {
    foreach ($t in $dadosEr.Tabelas) {
        if ($t.Nome -eq '__EFMigrationsHistory') { continue }
        if ($dominios[$d] | Where-Object { $t.Tabela -like $_ }) { if (-not $dominioDe.ContainsKey($t.Tabela)) { $dominioDe[$t.Tabela] = $d } }
    }
}
$todasAsFks = @($dadosEr.Tabelas | ForEach-Object { $_.FksSaida } | Where-Object { $_ })
$er = [System.Text.StringBuilder]::new()
function E([string] $texto = '') { [void]$er.AppendLine($texto) }
E '# Diagrama ER da arquitetura atual — anexo do documento 39'
E ''
E ('> Gerado por `scripts/banco/auditoria/gerar-inventario-do-banco.ps1` em ' + (Get-Date -Format 'dd/MM/yyyy HH:mm') + ', a partir das')
E '> 213 chaves estrangeiras reais do catálogo (`sys.foreign_keys`), sem interpretação. `||` = obrigatória, `|o` = opcional.'
E '> Para não esconder o desenho atrás de dois hubs, as FKs para `Empresa` e `Usuario` só aparecem nos domínios deles;'
E '> em cada outro domínio a quantidade omitida é informada. As tabelas de histórico de migração ficam de fora.'
E ''
E '## Visão geral: FKs entre domínios'
E ''
E '```mermaid'
E 'flowchart LR'
$ids = @{}; $i = 0
foreach ($d in $dominios.Keys) { $i++; $ids[$d] = "D$i"; $n = @($dominioDe.Keys | Where-Object { $dominioDe[$_] -eq $d }).Count; E ("    D$i[""$d<br/>$n tabelas""]") }
$todasAsFks | Where-Object { $dominioDe.ContainsKey($_.Origem) -and $dominioDe.ContainsKey($_.Destino) -and $dominioDe[$_.Origem] -ne $dominioDe[$_.Destino] } |
    Group-Object { $dominioDe[$_.Origem] + '>' + $dominioDe[$_.Destino] } | Sort-Object Count -Descending | ForEach-Object {
        $p = $_.Name.Split('>'); E ("    $($ids[$p[0]]) -- $($_.Count) --> $($ids[$p[1]])")
    }
E '```'
E ''
foreach ($d in $dominios.Keys) {
    $membros = @($dominioDe.Keys | Where-Object { $dominioDe[$_] -eq $d } | Sort-Object)
    $fksDeSaida = @($todasAsFks | Where-Object { $membros -contains $_.Origem })
    $omitidas = @($fksDeSaida | Where-Object { $hubs -contains $_.Destino -and $membros -notcontains $_.Destino })
    $desenhadas = @($fksDeSaida | Where-Object { -not ($hubs -contains $_.Destino -and $membros -notcontains $_.Destino) })
    $externas = @($desenhadas | Where-Object { $membros -notcontains $_.Destino } | ForEach-Object Destino | Select-Object -Unique)
    E "## $d"
    E ''
    E ("Tabelas: " + (($membros | ForEach-Object { '`' + $_ + '`' }) -join ', ') + '.')
    E ''
    E ("FKs de saída desenhadas: $($desenhadas.Count)" + $(if ($externas.Count) { ' (referências a outros domínios: ' + (($externas | ForEach-Object { '`' + $_ + '`' }) -join ', ') + ')' }) + $(if ($omitidas.Count) { "; omitidas para ``Empresa``/``Usuario``: $($omitidas.Count)" }) + '.')
    E ''
    E '```mermaid'
    E 'erDiagram'
    foreach ($m in $membros) {
        $t = $porTabela[$m]
        $pk = $t.Colunas | Where-Object { $_.Nome -eq 'Id' } | Select-Object -First 1
        $tipo = if ($pk) { ($pk.Tipo -replace '\(.*\)', '') } else { 'int' }
        E ("    $($t.Nome) {")
        E ("        $tipo Id PK")
        foreach ($f in @($t.FksSaida | Where-Object { $_.Colunas -notlike '*,*' } | Sort-Object { $_.Colunas })) {
            $col = $t.Colunas | Where-Object { $_.Nome -eq $f.Colunas } | Select-Object -First 1
            E ("        $(($col.Tipo -replace '\(.*\)', '')) $($f.Colunas) FK")
        }
        E '    }'
    }
    foreach ($f in @($desenhadas | Sort-Object { $_.Destino }, { $_.Origem })) {
        $origem = $porTabela[$f.Origem]
        $umParaUm = $origem.Indices | Where-Object { $_.Unico -and -not $_.Pk -and -not $_.Filtro -and $_.Chave -eq $f.Colunas }
        $pai = if ($f.Opcional) { '|o' } else { '||' }
        $filho = if ($umParaUm) { 'o|' } else { 'o{' }
        E ("    $($f.Destino.Split('.')[1]) $pai--$filho $($origem.Nome) : ""$($f.Colunas)""")
    }
    E '```'
    E ''
}
$erArq = Join-Path $raiz 'docs\projeto\39B-DIAGRAMA-ER-ATUAL.md'
[IO.File]::WriteAllText($erArq, $er.ToString(), (New-Object Text.UTF8Encoding($false)))
Write-Host 'Diagrama ER gravado: docs\projeto\39B-DIAGRAMA-ER-ATUAL.md' -ForegroundColor Green
