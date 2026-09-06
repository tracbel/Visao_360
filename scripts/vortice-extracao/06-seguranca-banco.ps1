<#
  06-seguranca-banco.ps1
  ----------------------
  Fotografa o modelo de seguranca do BANCO (nao o do aplicativo): principals,
  papeis, membros, permissoes explicitas e logins visiveis. Nenhuma senha ou
  hash e lido - apenas metadados de autorizacao.

  Saida: docs/extracao-vortice/seguranca-banco/*.csv
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Seg = Join-Path $Global:VorticeRaiz 'seguranca-banco'
if (-not (Test-Path $Seg)) { New-Item -ItemType Directory -Path $Seg -Force | Out-Null }

# --- principals do banco ---
$principals = Consulta -Rotulo 'sys.database_principals' -Sql @'
SELECT dp.principal_id, dp.name AS principal, dp.type_desc, dp.default_schema_name,
       dp.create_date, dp.modify_date, dp.is_fixed_role, dp.authentication_type_desc,
       dp.owning_principal_id
FROM sys.database_principals dp WITH (NOLOCK)
ORDER BY dp.type_desc, dp.name
'@
[void](ExportaCsv $principals (Join-Path $Seg 'principals-do-banco.csv'))

# --- membros de papeis ---
$membros = Consulta -Rotulo 'sys.database_role_members' -Sql @'
SELECT p.name AS papel, m.name AS membro, m.type_desc AS tipo_membro
FROM sys.database_role_members rm WITH (NOLOCK)
JOIN sys.database_principals p WITH (NOLOCK) ON p.principal_id = rm.role_principal_id
JOIN sys.database_principals m WITH (NOLOCK) ON m.principal_id = rm.member_principal_id
ORDER BY p.name, m.name
'@
[void](ExportaCsv $membros (Join-Path $Seg 'membros-de-papeis.csv'))

# --- permissoes explicitas no banco ---
$perms = Consulta -Rotulo 'sys.database_permissions' -Sql @'
SELECT pr.name              AS principal,
       pr.type_desc         AS tipo_principal,
       perm.class_desc      AS classe,
       perm.permission_name AS permissao,
       perm.state_desc      AS estado,
       CASE perm.class
            WHEN 0 THEN '(banco)'
            WHEN 1 THEN ISNULL(OBJECT_SCHEMA_NAME(perm.major_id) + '.', '') + ISNULL(OBJECT_NAME(perm.major_id), CONVERT(varchar(20), perm.major_id))
            WHEN 3 THEN SCHEMA_NAME(perm.major_id)
            ELSE CONVERT(varchar(20), perm.major_id) END AS objeto,
       perm.minor_id
FROM sys.database_permissions perm WITH (NOLOCK)
JOIN sys.database_principals pr WITH (NOLOCK) ON pr.principal_id = perm.grantee_principal_id
ORDER BY pr.name, perm.class_desc, perm.permission_name
'@
[void](ExportaCsv $perms (Join-Path $Seg 'permissoes-explicitas.csv'))

# --- logins visiveis no servidor ---
$logins = Consulta -Rotulo 'sys.server_principals' -Sql @'
SELECT name AS login, type_desc, is_disabled, create_date, modify_date, default_database_name
FROM sys.server_principals WITH (NOLOCK)
ORDER BY type_desc, name
'@
[void](ExportaCsv $logins (Join-Path $Seg 'logins-do-servidor.csv'))

# --- membros de papeis de servidor ---
$srvRoles = Consulta -Rotulo 'sys.server_role_members' -Sql @'
SELECT r.name AS papel_servidor, m.name AS membro, m.type_desc
FROM sys.server_role_members rm WITH (NOLOCK)
JOIN sys.server_principals r WITH (NOLOCK) ON r.principal_id = rm.role_principal_id
JOIN sys.server_principals m WITH (NOLOCK) ON m.principal_id = rm.member_principal_id
ORDER BY r.name, m.name
'@
[void](ExportaCsv $srvRoles (Join-Path $Seg 'papeis-de-servidor.csv'))

# --- donos de esquema e de banco ---
$esquemas = Consulta -Rotulo 'sys.schemas' -Sql @'
SELECT s.name AS esquema, p.name AS dono
FROM sys.schemas s WITH (NOLOCK)
LEFT JOIN sys.database_principals p WITH (NOLOCK) ON p.principal_id = s.principal_id
ORDER BY s.name
'@
[void](ExportaCsv $esquemas (Join-Path $Seg 'esquemas.csv'))

# --- linked servers e seus mapeamentos de login (sem senha) ---
$servers = Consulta -Rotulo 'sys.servers' -Sql @'
SELECT server_id, name, product, provider, data_source, catalog, provider_string,
       is_linked, is_remote_login_enabled, is_rpc_out_enabled, is_data_access_enabled,
       modify_date, connect_timeout, query_timeout
FROM sys.servers WITH (NOLOCK)
'@
# provider_string de um linked server MSDASQL pode carregar usuario/senha da string
# ODBC. Mascaramos antes de gravar - a extracao nunca publica credencial.
$serversSeg = ParaObjetos $servers
foreach ($s in $serversSeg) {
    if ($s.provider_string) {
        $s.provider_string = [regex]::Replace([string]$s.provider_string,
            '(?i)\b(pwd|password|senha|uid|user id|usuario)\s*=\s*[^;]*', '$1=***')
    }
}
$serversSeg | Export-Csv (Join-Path $Seg 'linked-servers.csv') -NoTypeInformation -Encoding UTF8

$mapLogins = Consulta -Rotulo 'sys.linked_logins' -Sql @'
SELECT s.name AS servidor_vinculado, ll.local_principal_id,
       ISNULL(sp.name, '(todos)') AS principal_local,
       ll.uses_self_credential, ll.remote_name, ll.modify_date
FROM sys.linked_logins ll WITH (NOLOCK)
JOIN sys.servers s WITH (NOLOCK) ON s.server_id = ll.server_id
LEFT JOIN sys.server_principals sp WITH (NOLOCK) ON sp.principal_id = ll.local_principal_id
'@
[void](ExportaCsv $mapLogins (Join-Path $Seg 'linked-logins.csv'))

# --- configuracao do banco (auditoria / trustworthy / recovery) ---
$dbcfg = Consulta -Rotulo 'sys.databases (CRM)' -Sql @'
SELECT name, collation_name, recovery_model_desc, is_trustworthy_on, is_db_chaining_on,
       is_auto_close_on, is_auto_shrink_on, is_read_committed_snapshot_on,
       snapshot_isolation_state_desc, page_verify_option_desc, compatibility_level,
       user_access_desc, state_desc, is_encrypted, is_broker_enabled, log_reuse_wait_desc
FROM sys.databases WITH (NOLOCK)
WHERE name IN ('CRM', 'CRM_HOMO')
'@
[void](ExportaCsv $dbcfg (Join-Path $Seg 'configuracao-do-banco.csv'))

# --- resumo: quem tem escrita ---
$escrita = @($perms | Where-Object {
        [string]$_['estado'] -eq 'GRANT' -and
        ([string]$_['permissao']) -match '(?i)^(INS|UPD|DEL|ALT|CONTROL|TAKE OWNERSHIP|EXEC|IMPERSONATE)'
    })
$resumo = New-Object System.Collections.ArrayList
foreach ($p in ($principals | Where-Object { [string]$_['type_desc'] -match 'USER' })) {
    $nome = [string]$p['principal']
    $papeis = @($membros | Where-Object { [string]$_['membro'] -eq $nome } | ForEach-Object { [string]$_['papel'] })
    $temEscrita = ($papeis | Where-Object { $_ -match '(?i)db_owner|db_datawriter|db_ddladmin|db_accessadmin|db_securityadmin' }).Count -gt 0 -or
                  (@($escrita | Where-Object { [string]$_['principal'] -eq $nome }).Count -gt 0)
    [void]$resumo.Add([pscustomobject]@{
            principal   = $nome
            tipo        = [string]$p['type_desc']
            autenticacao = [string]$p['authentication_type_desc']
            papeis      = ($papeis -join '; ')
            tem_escrita = $temEscrita
            criado_em   = $(if ($p['create_date'] -is [datetime]) { ([datetime]$p['create_date']).ToString('yyyy-MM-dd') } else { '' })
        })
}
$resumo | Sort-Object -Property @{Expression = 'tem_escrita'; Descending = $true }, principal |
    Export-Csv (Join-Path $Seg 'resumo-por-principal.csv') -NoTypeInformation -Encoding UTF8

Write-Host ("OK: {0} principals | {1} vinculos de papel | {2} permissoes | {3} logins | {4} com escrita" -f
    @($principals).Count, @($membros).Count, @($perms).Count, @($logins).Count,
    @($resumo | Where-Object { $_.tem_escrita }).Count) -ForegroundColor Green
