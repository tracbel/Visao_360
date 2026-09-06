<#
  01-frota-vv1.ps1
  ----------------
  A FROTA, como a diretoria pede: segmentada por filial, marca, tipo e modelo.

  Le a VV1 (Cadastro de Veiculos, 38.358 linhas) inteira e resolve os catalogos que dao nome ao
  codigo — VV8 para tipo de veiculo, VV2 para modelo. Somente leitura.

  CUIDADO COM O DICIONARIO: a SX3 lista campos que a tabela FISICA nao tem (VV1_DESMAR e
  VV1_ANOMOD estao no dicionario e nao voltam na consulta). Por isso os campos pedidos aqui sao
  so os que foram vistos voltando com valor. Pedir campo inexistente nao da erro — ele some da
  resposta em silencio, e um agrupamento por coluna ausente vira "(vazio)" para tudo.

  Saida: docs/extracao-protheus/frota-vv1.md

  Uso:  ./scripts/protheus/01-frota-vv1.ps1
#>

. (Join-Path $PSScriptRoot '_comum.ps1')

$Saida = Join-Path $PSScriptRoot '..\..\docs\extracao-protheus'
if (-not (Test-Path $Saida)) { New-Item -ItemType Directory -Path $Saida -Force | Out-Null }

Write-Host 'Lendo catalogos de apoio...' -ForegroundColor Cyan

$tipos = @{}
foreach ($t in (Get-ProtheusTabela -Tabela 'VV8' -Campos 'VV8_TIPVEI,VV8_DESCRI' -Maximo 0 -Silencioso)) {
    $tipos[(Txt $t.vv8_tipvei)] = (Txt $t.vv8_descri)
}
Write-Host ("  VV8 (tipos de veiculo): {0}" -f $tipos.Count)

# A VVX e quem resolve 'JD' para 'John Deere'. A VV1 tem um VV1_DESMAR no dicionario, mas ele
# NAO volta na consulta — a coluna nao existe na tabela fisica deste Protheus.
$marcas = @{}
foreach ($m in (Get-ProtheusTabela -Tabela 'VVX' -Campos 'VVX_CODMAR,VVX_DESMAR' -Maximo 0 -Silencioso)) {
    $cod = Txt $m.vvx_codmar
    if ($cod -and -not $marcas.ContainsKey($cod)) { $marcas[$cod] = (Txt $m.vvx_desmar) }
}
Write-Host ("  VVX (marcas): {0}" -f $marcas.Count)

Write-Host 'Lendo a VV1 inteira — 38 mil linhas, ~39 requisicoes...' -ForegroundColor Cyan
$frota = Get-ProtheusTabela -Tabela 'VV1' `
    -Campos 'VV1_FILIAL,VV1_CODMAR,VV1_MODVEI,VV1_TIPVEI,VV1_CHASSI' `
    -Tamanho 1000 -Maximo 0

Write-Host ("Total lido: {0:N0} veiculos" -f $frota.Count) -ForegroundColor Green

function Bloco($titulo, $linhas) {
    $texto = "`n## $titulo`n`n| Valor | Veiculos | % |`n|---|---:|---:|`n"
    $total = ($linhas | Measure-Object -Property Count -Sum).Sum
    foreach ($l in $linhas) {
        $texto += "| {0} | {1:N0} | {2:N1}% |`n" -f $l.Name, $l.Count, (100 * $l.Count / $total)
    }
    return $texto
}

$md = @"
# Frota no Protheus — tabela VV1

> Extraido de ``VV1 — Cadastro de Veiculos`` em $(Get-Date -Format 'dd/MM/yyyy HH:mm'), somente
> leitura, pela API de producao (``genericQuery``). **$($frota.Count.ToString('N0')) veiculos.**

## O que a VV1 e

E o cadastro de veiculo do modulo de concessionaria (SIGAVEI). Uma linha por chassi — nao por
modelo e nao por venda. Quem quiser venda usa a VV0/VVA (Saidas de Veiculos); quem quiser
cadastro de modelo usa a VV2.

**Nem toda linha da VV1 e maquina em poder do cliente.** A tabela guarda o chassi desde a
entrada, entao ela mistura estoque, vendido e devolvido. A separacao sai de ``VV1_SITVEI`` e
``VV1_STATUS`` — que estao **vazios na amostra medida**, e por isso este relatorio nao os usa.
Confirmar com o negocio antes de tratar qualquer recorte como "frota em campo".
"@

$md += Bloco 'Por filial' ($frota | Group-Object { Rotulo $_.vv1_filial } | Sort-Object Count -Descending)
$porMarca = $frota | Group-Object { Rotulo $_.vv1_codmar } | Sort-Object Count -Descending |
    Select-Object -First 25 | ForEach-Object {
        $nome = if ($marcas.ContainsKey($_.Name)) { "$($_.Name) - $($marcas[$_.Name])" } else { $_.Name }
        [pscustomobject]@{ Name = $nome; Count = $_.Count }
    }
$md += Bloco 'Por marca (VV1_CODMAR, nome da VVX)' $porMarca

$porTipo = $frota | Group-Object { Rotulo $_.vv1_tipvei } | Sort-Object Count -Descending | ForEach-Object {
    $nome = if ($tipos.ContainsKey($_.Name)) { "$($_.Name) — $($tipos[$_.Name])" } else { $_.Name }
    [pscustomobject]@{ Name = $nome; Count = $_.Count }
}
$md += Bloco 'Por tipo de veiculo (VV1_TIPVEI, nome da VV8)' $porTipo
$md += Bloco 'Modelos mais frequentes (VV1_MODVEI)' ($frota | Group-Object { Rotulo $_.vv1_modvei } | Sort-Object Count -Descending | Select-Object -First 30)

$arquivo = Join-Path $Saida 'frota-vv1.md'
[System.IO.File]::WriteAllText($arquivo, $md, (New-Object System.Text.UTF8Encoding($false)))
Write-Host ("Gravado: {0}" -f $arquivo) -ForegroundColor Green
