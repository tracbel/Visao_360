<#
.SYNOPSIS
    Falha quando a solução depende de pacote NuGet com vulnerabilidade de severidade alta ou
    crítica — inclusive por dependência transitiva.

.DESCRIPTION
    Issue #59 (docs/projeto/47-CI-CD.md, §5). Até aqui o CI só RELATAVA: o `dotnet list package
    --vulnerable` sempre sai com código 0, mesmo achando vulnerabilidade, e um relatório que ninguém
    é obrigado a ler é um alarme desligado.

    POR QUE INCLUIR O TRANSITIVO. A dependência que morde quase nunca é a que está no `.csproj`: é a
    que ela arrasta. Foi o caso do `Microsoft.Data.SqlClient` na migração para o .NET 10 (#84), que
    chegou pelo EF Core.

    O corte é alta e crítica, e não tudo: barrar "moderate" travaria o repositório em avisos que
    muitas vezes não têm correção publicada — e o time aprenderia a ignorar o bloqueio, que é o pior
    resultado possível. O que fica de fora aparece na tabela do resumo, com a severidade.

    Exceção com prazo e dono se registra em `scripts/ci/vulneraveis-aceitas.json` — cada entrada com
    o identificador do aviso, o motivo, quem aceitou e até quando. Exceção vencida volta a bloquear.

.PARAMETER Solucao
    O arquivo .sln a conferir.

.PARAMETER Excecoes
    O JSON com as exceções aceitas. Se não existir, nenhuma exceção vale.

.PARAMETER SomenteRelatorio
    Imprime e sai com 0 mesmo achando vulnerabilidade alta ou crítica. Para uso local.

.EXAMPLE
    ./scripts/ci/conferir-vulneraveis.ps1 -Solucao Tracbel.Crm.sln
#>
param(
    [string] $Solucao = 'Tracbel.Crm.sln',
    [string] $Excecoes = 'scripts/ci/vulneraveis-aceitas.json',
    [switch] $SomenteRelatorio
)

$ErrorActionPreference = 'Stop'
$severidadesQueBloqueiam = @('high', 'critical')

$bruto = & dotnet list $Solucao package --vulnerable --include-transitive --format json 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "O 'dotnet list package' falhou:"
    $bruto | ForEach-Object { Write-Host $_ }
    exit 1
}

try {
    $relatorio = ($bruto -join "`n") | ConvertFrom-Json
} catch {
    Write-Host 'Não consegui ler o JSON do dotnet list package. Saída bruta:'
    $bruto | ForEach-Object { Write-Host $_ }
    exit 1
}

# As exceções aceitas, ainda no prazo.
$aceitas = @{}
if (Test-Path -LiteralPath $Excecoes) {
    foreach ($e in (Get-Content -LiteralPath $Excecoes -Raw | ConvertFrom-Json)) {
        if ([datetime]::Parse($e.ate) -ge (Get-Date)) { $aceitas[$e.aviso] = $e }
    }
}

$achados = [System.Collections.Generic.List[object]]::new()

foreach ($projeto in $relatorio.projects) {
    $nome = [System.IO.Path]::GetFileNameWithoutExtension([string] $projeto.path)
    foreach ($framework in $projeto.frameworks) {
        foreach ($tipo in 'topLevelPackages', 'transitivePackages') {
            foreach ($pacote in $framework.$tipo) {
                foreach ($v in $pacote.vulnerabilities) {
                    $achados.Add([pscustomobject]@{
                        Projeto     = $nome
                        Pacote      = [string] $pacote.id
                        Versao      = [string] $pacote.resolvedVersion
                        Origem      = if ($tipo -eq 'topLevelPackages') { 'direta' } else { 'transitiva' }
                        Severidade  = ([string] $v.severity).ToLowerInvariant()
                        Aviso       = [string] $v.advisoryurl
                    })
                }
            }
        }
    }
}

$bloqueiam = @($achados | Where-Object {
    $_.Severidade -in $severidadesQueBloqueiam -and -not $aceitas.ContainsKey($_.Aviso)
})
$dispensados = @($achados | Where-Object { $aceitas.ContainsKey($_.Aviso) })

$saida = [System.Collections.Generic.List[string]]::new()
$saida.Add('## Pacotes .NET vulneráveis')
$saida.Add('')

if ($achados.Count -eq 0) {
    $saida.Add('Nenhum pacote vulnerável, direto ou transitivo.')
} else {
    $saida.Add('| Projeto | Pacote | Versão | Origem | Severidade | Barra? |')
    $saida.Add('|---|---|---|---|---|---|')
    foreach ($a in $achados | Sort-Object Severidade, Pacote -Unique) {
        $barra = if ($a.Severidade -in $severidadesQueBloqueiam) {
            if ($aceitas.ContainsKey($a.Aviso)) { 'exceção com prazo' } else { '**sim**' }
        } else { 'não (só relatório)' }
        $saida.Add("| $($a.Projeto) | $($a.Pacote) | $($a.Versao) | $($a.Origem) | $($a.Severidade) | $barra |")
    }
}

foreach ($d in $dispensados | Sort-Object Pacote -Unique) {
    $e = $aceitas[$d.Aviso]
    $saida.Add("")
    $saida.Add("> Exceção: **$($d.Pacote)** — $($e.motivo) (aceita por $($e.dono), até $($e.ate)).")
}

$saida | ForEach-Object { Write-Host $_ }
if ($env:GITHUB_STEP_SUMMARY) {
    $saida | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}

if ($bloqueiam.Count -gt 0 -and -not $SomenteRelatorio) {
    Write-Host ''
    Write-Host "$($bloqueiam.Count) vulnerabilidade(s) alta ou crítica sem exceção registrada."
    Write-Host 'Atualize o pacote, ou registre a exceção com prazo e dono em ' + $Excecoes + '.'
    exit 1
}

Write-Host ''
Write-Host "OK: nenhuma vulnerabilidade alta ou crítica pendente ($($achados.Count) achado(s) no total)."
exit 0
