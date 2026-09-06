<#
  _comum.ps1 — funcoes compartilhadas para LER o Protheus de producao.

  ---------------------------------------------------------------------------------------------
  COMO ESTE ACESSO FOI ENCONTRADO, porque o caminho nao e obvio e ninguem vai adivinhar depois:

  A API REST do Protheus de producao responde em http://10.100.10.98:5891/rest (HTTP puro,
  envId TMPRD). O ambiente de homologacao (10.100.10.252) NAO tem rota desta estacao.

  Os endpoints publicados sao pouquissimos: /api/framework/v1/users (SCIM) e mais nada dos
  modulos. Os customizados do projeto de onboarding (UONBMOD, UONBSA3, UONBVAI) existem SO em
  homologacao — em producao dao 404.

  O que destrava tudo e /api/framework/v1/genericQuery. Ele NAO aparece em catalogo nenhum: foi
  descoberto porque respondeu 400 (parametro faltando) onde todo o resto respondia 404. Com
  `tables` e `fields` ele le qualquer tabela do dicionario.

  ---------------------------------------------------------------------------------------------
  REGRA QUE NAO SE QUEBRA: este arquivo so faz GET. Nenhuma funcao aqui escreve no Protheus, e
  nenhuma deve passar a escrever. E um ERP de producao.

  ---------------------------------------------------------------------------------------------
  CREDENCIAL: sai de .env na raiz do repositorio (TOTVS_API_USER_PROD / TOTVS_API_PASSWORD_PROD),
  que o .gitignore bloqueia. A senha viaja na QUERY STRING — e assim que a API do Protheus
  funciona —, entao toda mensagem de erro passa por Ocultar() antes de chegar na tela ou no log.
#>

$Global:ProtheusBase = 'http://10.100.10.98:5891/rest'
$Global:ProtheusToken = $null
$Global:ProtheusTokenAte = [datetime]::MinValue

function Ocultar([string]$texto) {
    if (-not $texto) { return $texto }
    return ($texto -replace 'password=[^&\s"]*', 'password=***')
}

function Get-ProtheusCredencial {
    param([string]$Caminho = (Join-Path $PSScriptRoot '..\..\.env'))

    if (-not (Test-Path $Caminho)) {
        throw "Arquivo .env nao encontrado em $Caminho. Ele guarda TOTVS_API_USER_PROD e TOTVS_API_PASSWORD_PROD."
    }

    $cfg = @{}
    foreach ($linha in Get-Content $Caminho) {
        if ($linha -match '^\s*#') { continue }
        $p = $linha -split '=', 2
        if ($p.Count -eq 2) { $cfg[$p[0].Trim()] = $p[1].Trim() }
    }

    if (-not $cfg['TOTVS_API_USER_PROD'] -or -not $cfg['TOTVS_API_PASSWORD_PROD']) {
        throw 'Credenciais incompletas no .env (TOTVS_API_USER_PROD / TOTVS_API_PASSWORD_PROD).'
    }

    return @{ Usuario = $cfg['TOTVS_API_USER_PROD']; Senha = $cfg['TOTVS_API_PASSWORD_PROD'] }
}

<#
  O token vale 3600s. Esta funcao renova sozinha cinco minutos antes de vencer — uma extracao
  de tabela grande passa da hora, e descobrir isso no meio de 40 paginas custa a extracao toda.
#>
function Get-ProtheusToken {
    if ($Global:ProtheusToken -and (Get-Date) -lt $Global:ProtheusTokenAte) {
        return $Global:ProtheusToken
    }

    $c = Get-ProtheusCredencial
    $url = "$Global:ProtheusBase/api/oauth2/v1/token?grant_type=password" +
           "&username=$([uri]::EscapeDataString($c.Usuario))" +
           "&password=$([uri]::EscapeDataString($c.Senha))"

    try {
        $tk = Invoke-RestMethod -Uri $url -Method Post -TimeoutSec 60
    } catch {
        throw ('Falha ao autenticar no Protheus: ' + (Ocultar $_.Exception.Message))
    }

    $Global:ProtheusToken = $tk.access_token
    $Global:ProtheusTokenAte = (Get-Date).AddSeconds([int]$tk.expires_in - 300)
    return $Global:ProtheusToken
}

<#
  Uma pagina de uma tabela.

  `Campos` e OBRIGATORIO e nao aceita '*': o genericQuery devolve 500 com asterisco e 400 sem a
  lista. Os nomes saem do dicionario — use Get-ProtheusDicionario.
#>
function Invoke-ProtheusQuery {
    param(
        [Parameter(Mandatory = $true)] [string]$Tabela,
        [Parameter(Mandatory = $true)] [string]$Campos,
        [string]$Onde = '',
        [int]$Pagina = 1,
        [int]$Tamanho = 1000,
        [int]$TimeoutSec = 180
    )

    $h = @{ Authorization = "Bearer $(Get-ProtheusToken)" }
    $qs = "tables=$Tabela&fields=$([uri]::EscapeDataString($Campos))&page=$Pagina&pageSize=$Tamanho"
    if ($Onde) { $qs += "&where=$([uri]::EscapeDataString($Onde))" }

    # 428 e 503 sao intermitentes no AppServer — o proprio fornecedor documenta. Tres tentativas
    # com espera crescente, e SO em GET: repetir leitura e seguro, repetir escrita nao seria.
    for ($tentativa = 1; $tentativa -le 3; $tentativa++) {
        try {
            return Invoke-RestMethod -Uri "$Global:ProtheusBase/api/framework/v1/genericQuery?$qs" `
                -Headers $h -Method Get -TimeoutSec $TimeoutSec
        } catch {
            $codigo = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
            if ($tentativa -eq 3 -or ($codigo -notin 0, 428, 500, 503, 504)) {
                throw ("Protheus $Tabela (HTTP $codigo): " + (Ocultar $_.Exception.Message))
            }
            Start-Sleep -Seconds (2 * $tentativa)
        }
    }
}

<#
  A tabela inteira, pagina a pagina.

  `Maximo` existe para nao varrer um milhao de linhas sem querer num ERP de producao. Passe 0
  para trazer tudo, e so quando isso for a intencao.
#>
function Get-ProtheusTabela {
    param(
        [Parameter(Mandatory = $true)] [string]$Tabela,
        [Parameter(Mandatory = $true)] [string]$Campos,
        [string]$Onde = '',
        [int]$Tamanho = 1000,
        [int]$Maximo = 5000,
        [switch]$Silencioso
    )

    $todos = New-Object System.Collections.ArrayList
    $pagina = 1

    while ($true) {
        $r = Invoke-ProtheusQuery -Tabela $Tabela -Campos $Campos -Onde $Onde -Pagina $pagina -Tamanho $Tamanho
        if (-not $r.items) { break }

        foreach ($item in $r.items) { [void]$todos.Add($item) }
        if (-not $Silencioso) {
            Write-Host ("  {0} pagina {1,3} -> {2} linhas (acumulado {3} de {4})" -f `
                $Tabela, $pagina, $r.items.Count, $todos.Count, $r.total)
        }

        if (-not $r.hasNext) { break }
        if ($Maximo -gt 0 -and $todos.Count -ge $Maximo) { break }
        $pagina++
    }

    return $todos
}

<#
  O dicionario de campos de uma tabela, da propria SX3 do Protheus.

  E o que evita chutar nome de campo: o Protheus tem 203 colunas so na VV1, e adivinhar produz
  500 sem dizer qual campo nao existe.
#>
function Get-ProtheusDicionario {
    param([Parameter(Mandatory = $true)] [string]$Tabela)

    return Get-ProtheusTabela -Tabela 'SX3' `
        -Campos 'X3_ARQUIVO,X3_CAMPO,X3_TITULO,X3_TIPO,X3_TAMANHO,X3_DECIMAL' `
        -Onde "X3_ARQUIVO='$Tabela'" -Tamanho 500 -Maximo 0 -Silencioso
}

<# O texto de um campo, ja sem espaco de preenchimento e sem nulo. #>
function Txt($valor) {
    if ($null -eq $valor) { return '' }
    return ("$valor").Trim()
}

<# Um rotulo para agrupamento: vazio vira marcador explicito, e nao string vazia. #>
function Rotulo($valor) {
    $t = Txt $valor
    if ($t -eq '') { return '(vazio)' }
    return $t
}
