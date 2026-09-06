<#
  02-faturamento-sd2.ps1
  ----------------------
  O FATURAMENTO REAL E ATUAL, direto do Protheus — e a separacao entre maquina e peca.

  ---------------------------------------------------------------------------------------------
  POR QUE ESTE SCRIPT EXISTE. O projeto operou meses com a premissa de que o faturamento tinha
  parado em 11/04/2025. A premissa vinha da tabela que o Vortice RECEBE do Protheus
  (X_TOTVS_CRM_FATURAMENTO), que de fato para nessa data. Medido aqui, na origem: a SD2 tem
  nota emitida ate hoje. **O que morreu foi a integracao, nao o faturamento.**

  ---------------------------------------------------------------------------------------------
  MAQUINA OU PECA sai de D2_GRUPO, que viaja NA PROPRIA LINHA da nota — nao precisa juntar com a
  SB1. O catalogo de grupos e a SBM:

      VEIC        MAQUINAS
      1001..10xx  PECAS (uma faixa por linha: colhedoras, tratores, plantadeiras...)
      SRV         SERVICOS
      MO_O        MAO DE OBRA OFICINA
      AMS         A.M.S

  Somente leitura.

  Saida: docs/extracao-protheus/faturamento-sd2.md

  Uso:  ./scripts/protheus/02-faturamento-sd2.ps1 [-Desde 20260101]
#>

param([string]$Desde = '20260101')

. (Join-Path $PSScriptRoot '_comum.ps1')

$Saida = Join-Path $PSScriptRoot '..\..\docs\extracao-protheus'
if (-not (Test-Path $Saida)) { New-Item -ItemType Directory -Path $Saida -Force | Out-Null }

Write-Host 'Lendo o catalogo de grupos de produto (SBM)...' -ForegroundColor Cyan
$grupos = @{}
foreach ($g in (Get-ProtheusTabela -Tabela 'SBM' -Campos 'BM_GRUPO,BM_DESC' -Maximo 0 -Silencioso)) {
    $grupos[(Txt $g.bm_grupo)] = (Txt $g.bm_desc)
}
Write-Host ("  {0} grupos" -f $grupos.Count)

Write-Host ("Lendo a SD2 desde {0}..." -f $Desde) -ForegroundColor Cyan
$itens = Get-ProtheusTabela -Tabela 'SD2' `
    -Campos 'D2_FILIAL,D2_EMISSAO,D2_GRUPO,D2_COD,D2_TOTAL,D2_QUANT,D2_CLIENTE,D2_DOC' `
    -Onde "D2_EMISSAO >= '$Desde'" -Tamanho 1000 -Maximo 0

Write-Host ("Total lido: {0:N0} itens de nota" -f $itens.Count) -ForegroundColor Green

# O total vem como texto na resposta da API. Converter com InvariantCulture: o separador decimal
# la e o ponto, e a maquina esta em pt-BR — sem isso, "674.45" vira 67445.
function Valor($v) {
    $t = Txt $v
    if ($t -eq '') { return 0.0 }
    return [double]::Parse($t, [Globalization.CultureInfo]::InvariantCulture)
}

$comValor = $itens | ForEach-Object {
    $g = Rotulo $_.d2_grupo
    [pscustomobject]@{
        Filial   = Rotulo $_.d2_filial
        Mes      = (Txt $_.d2_emissao).Substring(0, 7)
        Grupo    = $g
        GrupoDesc= $(if ($grupos.ContainsKey($g)) { $grupos[$g] } else { $g })
        Total    = Valor $_.d2_total
        Cliente  = Rotulo $_.d2_cliente
        Nota     = Rotulo $_.d2_doc
    }
}

$somaGeral = ($comValor | Measure-Object -Property Total -Sum).Sum

function Tabela($titulo, $agrupado, $colunaNome) {
    $texto = "`n## $titulo`n`n| $colunaNome | Itens | Valor | % |`n|---|---:|---:|---:|`n"
    foreach ($g in $agrupado) {
        $soma = ($g.Group | Measure-Object -Property Total -Sum).Sum
        $texto += "| {0} | {1:N0} | R$ {2:N2} | {3:N1}% |`n" -f $g.Name, $g.Count, $soma, (100 * $soma / $somaGeral)
    }
    return $texto
}

$md = @"
# Faturamento no Protheus — tabela SD2

> Extraido de ``SD2 — Itens de Nota Fiscal de Saida`` em $(Get-Date -Format 'dd/MM/yyyy HH:mm'),
> somente leitura, pela API de producao. Recorte: emissao a partir de **$Desde**.
> **$($comValor.Count.ToString('N0')) itens**, **R$ $($somaGeral.ToString('N2'))**.

## A premissa que este numero derruba

O projeto operou meses com "o faturamento parou em 11/04/2025". A frase vinha de
``X_TOTVS_CRM_FATURAMENTO``, a tabela que o **Vortice recebe** do Protheus — e essa de fato para
naquela data. Na **origem**, nao para: ha nota emitida ate hoje.

**O que morreu foi a integracao Protheus -> Vortice, nao o faturamento.** Toda tela que hoje diz
"depende do Protheus, que esta parado" pode passar a mostrar numero atual, lendo daqui.

## Como maquina se separa de peca

``D2_GRUPO`` viaja na propria linha da nota — nao precisa juntar com a SB1. O catalogo e a
``SBM``: ``VEIC`` e maquina, a faixa ``1001..10xx`` e peca por linha de produto, ``SRV`` e
servico, ``MO_O`` e mao de obra de oficina.
"@

$md += Tabela 'Por mes' ($comValor | Group-Object Mes | Sort-Object Name) 'Mes'
$md += Tabela 'Por grupo de produto' ($comValor | Group-Object GrupoDesc | Sort-Object { ($_.Group | Measure-Object -Property Total -Sum).Sum } -Descending | Select-Object -First 25) 'Grupo'
$md += Tabela 'Por filial' ($comValor | Group-Object Filial | Sort-Object { ($_.Group | Measure-Object -Property Total -Sum).Sum } -Descending) 'Filial'

$arquivo = Join-Path $Saida 'faturamento-sd2.md'
[System.IO.File]::WriteAllText($arquivo, $md, (New-Object System.Text.UTF8Encoding($false)))
Write-Host ("Gravado: {0}" -f $arquivo) -ForegroundColor Green
