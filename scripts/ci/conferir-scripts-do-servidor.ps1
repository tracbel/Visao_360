<#
  conferir-scripts-do-servidor.ps1 - os scripts que rodam NO SERVIDOR abrem no PowerShell 5.1?

      ./scripts/ci/conferir-scripts-do-servidor.ps1

  =================================================================================================
  POR QUE ISTO EXISTE

  O servidor tem PowerShell 5.1, e o 5.1 le arquivo .ps1 sem BOM como ANSI. Um travessao ou um
  acento em UTF-8 vira dois caracteres estranhos, e o parser quebra em algum ponto DEPOIS dele — com
  uma mensagem que nao tem nada a ver com a causa ("Token '{' inesperado", numa linha onde nao ha
  nada de errado).

  MEDIDO em 20/09/2026, ao escrever o agente de publicacao: o arquivo passava no PowerShell 7 da
  estacao e quebrava no 5.1. Sem este teste, isso so apareceria no servidor — depois de agendado, a
  cada cinco minutos, em silencio.

  A REGRA: script que vai para o servidor tem BOM UTF-8, ou so ASCII. Este script confere os dois
  caminhos e falha dizendo qual arquivo e qual linha.

  Fica de fora o que e GERADO no servidor (`rodar-fontes-publicas.ps1`, `rodar-precos.ps1`, escritos pelo
  `registrar-rotinas.ps1`), que ja nasce em ASCII por construcao, e o que roda so na estacao, onde o
  PowerShell e o 7.
#>

[CmdletBinding()]
param(
    [string] $Raiz = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
)

$ErrorActionPreference = 'Stop'

# OS QUE VIAJAM PARA O SERVIDOR. Acrescentar um script aqui e parte de manda-lo para la.
$doServidor = @(
    'scripts/deploy/agente-de-publicacao.ps1',
    'scripts/deploy/publicar-pacote.ps1',
    'scripts/deploy/registrar-rotinas.ps1'
)

$problemas = [System.Collections.Generic.List[string]]::new()

foreach ($relativo in $doServidor) {
    $caminho = Join-Path $Raiz $relativo

    if (-not (Test-Path $caminho)) {
        $problemas.Add("$relativo — nao existe")
        continue
    }

    $bytes = [System.IO.File]::ReadAllBytes($caminho)
    $temBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $soAscii = -not ($bytes | Where-Object { $_ -gt 0x7F })

    if (-not $temBom -and -not $soAscii) {
        $problemas.Add("$relativo — tem caractere fora do ASCII e NAO tem BOM UTF-8: o PowerShell 5.1 do servidor vai ler errado")
        continue
    }

    # E, com o BOM no lugar, ele abre mesmo? A leitura abaixo e a mesma que o 5.1 faria.
    $erros = $null
    [System.Management.Automation.Language.Parser]::ParseFile($caminho, [ref]$null, [ref]$erros) | Out-Null

    if ($erros) {
        foreach ($e in $erros) {
            $problemas.Add("$relativo`:$($e.Extent.StartLineNumber) — $($e.Message)")
        }
        continue
    }

    Write-Host ("OK   {0,-46} {1}" -f $relativo, $(if ($temBom) { 'BOM UTF-8' } else { 'ASCII puro' }))
}

if ($problemas.Count -gt 0) {
    Write-Host ''
    Write-Host 'Os scripts abaixo nao vao rodar no servidor:' -ForegroundColor Red
    foreach ($p in $problemas) { Write-Host "   $p" -ForegroundColor Red }
    Write-Host ''
    Write-Host 'Para consertar, grave o arquivo com BOM UTF-8:' -ForegroundColor Yellow
    Write-Host '   $t = Get-Content <arquivo> -Raw -Encoding utf8' -ForegroundColor Yellow
    Write-Host '   [System.IO.File]::WriteAllText((Resolve-Path <arquivo>), $t, (New-Object System.Text.UTF8Encoding $true))' -ForegroundColor Yellow
    exit 1
}

Write-Host ''
Write-Host "$($doServidor.Count) script(s) do servidor conferido(s)." -ForegroundColor Green
