<#
.SYNOPSIS
    Confere os resultados .trx do `dotnet test` e falha se algum teste falhou, se algum foi pulado
    ou se não há resultado nenhum.

.DESCRIPTION
    Issue #56 (docs/projeto/47-CI-CD.md, §3).

    POR QUE CONTAR RESULTADO POR RESULTADO. No .trx, o contador `notExecuted` do resumo fica em 0
    mesmo quando o xUnit pula testes: o pulado só aparece como `UnitTestResult` com
    `outcome="NotExecuted"`. Medido em 17/09/2026 no projeto de arquitetura: total=64, executed=55,
    notExecuted=0, com 9 testes pulados. Olhar só o resumo deixaria passar exatamente o que o CI
    existe para barrar: os testes de banco pulando em silêncio.

    Imprime a contagem por projeto e a lista de testes pulados ou com falha (nome e o começo da
    mensagem). Dentro do GitHub Actions, grava o mesmo resumo em $env:GITHUB_STEP_SUMMARY.

.PARAMETER Pasta
    Pasta com os arquivos .trx. Procura também nas subpastas.

.PARAMETER PermitirPulados
    Não falha por teste pulado. Só para uso local, quando não há SQL Server.

.EXAMPLE
    ./scripts/ci/conferir-testes.ps1 -Pasta TestResults
#>
param(
    [Parameter(Mandatory)] [string] $Pasta,
    [switch] $PermitirPulados
)

$ErrorActionPreference = 'Stop'
$espacoDeNomes = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }

function Contar([object[]] $resultados, [string] $situacao) {
    @($resultados | Where-Object { $_.outcome -eq $situacao }).Count
}

$arquivos = @(Get-ChildItem -LiteralPath $Pasta -Filter '*.trx' -Recurse -File -ErrorAction SilentlyContinue)
if ($arquivos.Count -eq 0) {
    Write-Host "Nenhum arquivo .trx em '$Pasta'. O dotnet test rodou com --logger trx?"
    exit 1
}

$projetos = [System.Collections.Generic.List[object]]::new()
$problemas = [System.Collections.Generic.List[object]]::new()

foreach ($arquivo in $arquivos) {
    [xml] $trx = Get-Content -LiteralPath $arquivo.FullName -Raw
    $resultados = @(Select-Xml -Xml $trx -XPath '//t:UnitTestResult' -Namespace $espacoDeNomes |
        ForEach-Object Node)

    $primeiroTeste = Select-Xml -Xml $trx -XPath '//t:UnitTest' -Namespace $espacoDeNomes | Select-Object -First 1
    $nome = if ($primeiroTeste) {
        [System.IO.Path]::GetFileNameWithoutExtension([string] $primeiroTeste.Node.storage)
    } else { $arquivo.BaseName }

    $passou = Contar $resultados 'Passed'
    $pulado = Contar $resultados 'NotExecuted'
    $projetos.Add([pscustomobject]@{
        Projeto = $nome
        Total   = $resultados.Count
        Passou  = $passou
        Falhou  = $resultados.Count - $passou - $pulado
        Pulado  = $pulado
    })

    foreach ($r in $resultados | Where-Object { $_.outcome -ne 'Passed' }) {
        $mensagem = [string] $r.Output.ErrorInfo.Message
        if (-not $mensagem) { $mensagem = [string] $r.Output.StdOut }
        $mensagem = ($mensagem -replace '\s+', ' ').Trim()
        if ($mensagem.Length -gt 200) { $mensagem = $mensagem.Substring(0, 200) + '…' }

        $problemas.Add([pscustomobject]@{
            Situacao = if ($r.outcome -eq 'NotExecuted') { 'PULADO' } else { "FALHOU ($($r.outcome))" }
            Teste    = [string] $r.testName
            Mensagem = $mensagem
        })
    }
}

$total = ($projetos | Measure-Object Total -Sum).Sum
$passouTotal = ($projetos | Measure-Object Passou -Sum).Sum
$falhouTotal = ($projetos | Measure-Object Falhou -Sum).Sum
$puladoTotal = ($projetos | Measure-Object Pulado -Sum).Sum

$saida = [System.Collections.Generic.List[string]]::new()
$saida.Add('## Testes')
$saida.Add('')
$saida.Add('| Projeto | Total | Passou | Falhou | Pulado |')
$saida.Add('|---|---:|---:|---:|---:|')
foreach ($p in $projetos | Sort-Object Projeto) {
    $saida.Add("| $($p.Projeto) | $($p.Total) | $($p.Passou) | $($p.Falhou) | $($p.Pulado) |")
}
$saida.Add("| **Total** | **$total** | **$passouTotal** | **$falhouTotal** | **$puladoTotal** |")

if ($problemas.Count -gt 0) {
    $saida.Add('')
    $saida.Add('| Situação | Teste | Mensagem |')
    $saida.Add('|---|---|---|')
    foreach ($p in $problemas | Sort-Object Situacao, Teste) {
        $saida.Add("| $($p.Situacao) | $($p.Teste) | $($p.Mensagem -replace '\|', '/') |")
    }
}

$saida | ForEach-Object { Write-Host $_ }
if ($env:GITHUB_STEP_SUMMARY) {
    $saida | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}

if ($total -eq 0) {
    Write-Host 'Nenhum resultado de teste nos arquivos .trx.'
    exit 1
}
if ($falhouTotal -gt 0) {
    Write-Host "$falhouTotal teste(s) falharam."
    exit 1
}
if ($puladoTotal -gt 0 -and -not $PermitirPulados) {
    Write-Host "$puladoTotal teste(s) pulados. No CI todo teste precisa rodar — o motivo está na tabela acima."
    exit 1
}

if ($puladoTotal -gt 0) {
    Write-Host "OK com pulados permitidos: $passouTotal aprovados e $puladoTotal pulados, de $total."
} else {
    Write-Host "OK: $total testes, todos executados e aprovados."
}
exit 0
