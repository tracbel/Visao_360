<#
  binarios-04-swagger.ps1 - Baixa o spec Swagger da wsVorticeCrmApi e gera o markdown de endpoints,
  comparando com a referencia ja existente no repositorio do agente.

  Somente leitura (HTTP GET). Nao envia credencial.

  Uso:
    powershell -ExecutionPolicy Bypass -File .\binarios-04-swagger.ps1 `
      -BaseUrl 'http://10.150.6.230/wsVorticeCrmApi' -OutDir <pasta> `
      -DocAtual 'c:\projetos\vortice-crm-agent\docs\API-wsVorticeCrmApi.md'
#>
[CmdletBinding()]
param(
    [string]$BaseUrl  = 'http://10.150.6.230/wsVorticeCrmApi',
    [string]$OutDir   = '.',
    [string]$DocAtual = 'c:\projetos\vortice-crm-agent\docs\API-wsVorticeCrmApi.md',
    [switch]$UsarArquivoLocal
)

$ProgressPreference = 'SilentlyContinue'
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
$specFile = Join-Path $OutDir 'swagger-wsVorticeCrmApi.json'

if (-not $UsarArquivoLocal) {
    $baixou = $false
    foreach ($v in 'v2','v1','v3') {
        try {
            Invoke-WebRequest "$BaseUrl/swagger/docs/$v" -UseBasicParsing -TimeoutSec 60 -OutFile $specFile -ErrorAction Stop
            "spec baixado de /swagger/docs/$v ($((Get-Item $specFile).Length) bytes)"; $baixou = $true; break
        } catch { }
    }
    if (-not $baixou) { throw "Nao foi possivel baixar o spec a partir de $BaseUrl" }
}

$j = Get-Content $specFile -Raw -Encoding UTF8 | ConvertFrom-Json

# ---- agrupamento por familia de rota ----
function Familia { param([string]$p)
    $s = $p -replace '^/api/',''
    if ($s -match '^crm/imp/')  { return 'crm/imp (importacao ERP)' }
    if ($s -match '^crm/')      { return 'crm (nucleo)' }
    ($s -split '/')[0]
}

$rotas = foreach ($pp in $j.paths.PSObject.Properties) {
    foreach ($m in $pp.Value.PSObject.Properties) {
        if ($m.Name -notin 'get','post','put','delete','patch','head','options') { continue }
        $op = $m.Value
        [pscustomobject]@{
            Familia   = Familia $pp.Name
            Metodo    = $m.Name.ToUpper()
            Rota      = $pp.Name
            Resumo    = ($op.summary   -replace '\s+',' ')
            Descricao = ($op.description -replace '\s+',' ')
            Tags      = ($op.tags -join ', ')
            OperationId = $op.operationId
            Parametros = (($op.parameters | ForEach-Object { "$($_.name) [$($_.'in')]$(if($_.required){'*'})" }) -join '; ')
        }
    }
}

$rotas | Sort-Object Familia, Rota, Metodo |
    Export-Csv (Join-Path $OutDir 'API-endpoints.csv') -NoTypeInformation -Encoding UTF8

# ---- comparacao com a doc existente ----
$documentadas = @()
if (Test-Path $DocAtual) {
    $txt = Get-Content $DocAtual -Raw
    $documentadas = [regex]::Matches($txt, '/api/[A-Za-z0-9_\-\{\}/\.]+') | ForEach-Object { $_.Value.TrimEnd('.',',') } | Sort-Object -Unique
}
function Normaliza { param([string]$r) ($r.ToLower() -replace '\{[^}]*\}','{}').TrimEnd('/') }
$docNorm = $documentadas | ForEach-Object { Normaliza $_ }

$novas = $rotas | Where-Object { (Normaliza $_.Rota) -notin $docNorm } | Sort-Object Rota, Metodo
$semSpec = $documentadas | Where-Object { (Normaliza $_) -notin ($rotas | ForEach-Object { Normaliza $_.Rota }) }

# ---- markdown ----
$sb = New-Object System.Text.StringBuilder
function A { param($t) [void]$sb.AppendLine($t) }

A "# API ``wsVorticeCrmApi`` - endpoints completos (extraidos do Swagger)"
A ''
A "- **Spec:** ``$BaseUrl/swagger/docs/v2`` (Swagger 2.0, Swashbuckle) - copia em ``swagger-wsVorticeCrmApi.json``"
A "- **Titulo:** $($j.info.title)"
A "- **basePath:** ``$($j.basePath)`` - **schemes:** $($j.schemes -join ', ')"
A "- **Rotas:** $(($j.paths.PSObject.Properties|Measure-Object).Count) - **Operacoes:** $($rotas.Count) - **Definicoes (modelos):** $(($j.definitions.PSObject.Properties|Measure-Object).Count)"
$sd = ($j.securityDefinitions.PSObject.Properties.Name) -join ', '
A "- **securityDefinitions:** $(if($sd){$sd}else{'**nenhuma** - o spec nao declara esquema de autenticacao'})"
A "- Coletado em $(Get-Date -Format 'yyyy-MM-dd HH:mm')."
A ''
A '## Resumo por familia'
A ''
A '| Familia | Operacoes |'
A '|---|---:|'
$rotas | Group-Object Familia | Sort-Object Name | ForEach-Object { A "| $($_.Name) | $($_.Count) |" }
A ''

A '## Endpoints'
A ''
foreach ($g in ($rotas | Group-Object Familia | Sort-Object Name)) {
    A "### $($g.Name)"
    A ''
    A '| Metodo | Rota | Resumo | Parametros |'
    A '|---|---|---|---|'
    foreach ($r in ($g.Group | Sort-Object Rota, Metodo)) {
        $res = if ($r.Resumo) { $r.Resumo } else { $r.Descricao }
        A ("| {0} | ``{1}`` | {2} | {3} |" -f $r.Metodo, $r.Rota, ($res -replace '\|','/'), ($r.Parametros -replace '\|','/'))
    }
    A ''
}

A '## Diferenca em relacao a `docs/API-wsVorticeCrmApi.md` do agente'
A ''
A "Referencia comparada: ``$DocAtual``"
A ''
A "### Rotas presentes no Swagger e AUSENTES na doc ($($novas.Count))"
A ''
if ($novas) {
    A '| Metodo | Rota | Resumo |'
    A '|---|---|---|'
    foreach ($r in $novas) { A ("| {0} | ``{1}`` | {2} |" -f $r.Metodo, $r.Rota, (($r.Resumo -replace '\|','/'))) }
} else { A '_nenhuma._' }
A ''
A "### Rotas citadas na doc e AUSENTES no Swagger ($($semSpec.Count))"
A ''
if ($semSpec) { foreach ($r in $semSpec) { A "- ``$r``" } } else { A '_nenhuma._' }
A ''

A '## Modelos (definitions)'
A ''
A ($j.definitions.PSObject.Properties.Name | Sort-Object | ForEach-Object { "- ``$_``" }) -join "`n"
A ''

Set-Content -LiteralPath (Join-Path $OutDir 'API-endpoints-completo.md') -Value $sb.ToString() -Encoding UTF8
"gravado: $(Join-Path $OutDir 'API-endpoints-completo.md')  ($($rotas.Count) operacoes, $($novas.Count) novas)"
