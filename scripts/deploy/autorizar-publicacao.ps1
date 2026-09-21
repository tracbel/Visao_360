<#
  autorizar-publicacao.ps1 - libera uma publicacao que PAROU por ter migracao destrutiva (issue 62).

      .\scripts\deploy\autorizar-publicacao.ps1 -Commit a39fa53

  O commit pode ser o curto; o script o resolve pelo Git local.

  =================================================================================================
  QUANDO ISTO E NECESSARIO

  Quase nunca. A decisao de 20/09/2026 e que migracao ADITIVA (criar tabela, criar coluna) sobe
  sozinha - que e a esmagadora maioria. Este script existe para o outro caso: quando a subida vai
  APAGAR tabela ou coluna.

  "Publicar e migrar" (regra R-4 do doc 46). Uma migracao destrutiva nao se desfaz com `git revert`:
  desfaz-se restaurando o backup. Por isso ela passa por uma pessoa.

  =================================================================================================
  O QUE ELE MOSTRA ANTES DE PERGUNTAR

  O que exatamente vai ser apagado, tabela por tabela e coluna por coluna, como o CI as encontrou no
  SQL que a subida vai rodar. Autorizar sem ver isso seria o mesmo que nao ter portao nenhum.

  A autorizacao vale para UM commit: um pacote novo, com outra migracao destrutiva, para de novo.

  =================================================================================================
  E QUANDO A PUBLICACAO FALHOU

  O agente nao tenta de novo sozinho o commit que falhou com os mesmos scripts (21/09/2026). Este
  script, com o mesmo -Commit, pede UMA nova tentativa - depois de alguem ter visto o motivo no
  `verificar-publicacao.ps1`. Se o defeito era dos scripts do agente, o caminho e outro: atualiza-los
  com `instalar-agente-de-publicacao.ps1 -SoAtualizarOsScripts`, e a nova tentativa sai sozinha.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Commit,
    [string] $Servidor = '10.150.4.249',
    [string] $PastaDoAgente = 'C:\aplicacoes\tracbel-crm-agente',
    [string] $Motivo = ''
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

# O COMMIT CURTO VALE. Exigir os 40 caracteres so fazia a pessoa errar na primeira tentativa (medido em
# 21/09/2026: o primeiro uso real parou aqui). O Git local resolve o curto para o inteiro; se ele for
# ambiguo ou nao existir, o Git recusa e nada acontece.
if ($Commit -notmatch '^[0-9a-f]{40}$') {
    $inteiro = & git -C (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) rev-parse --verify --quiet "$Commit^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0 -or $inteiro -notmatch '^[0-9a-f]{40}$') {
        throw "Nao reconheco o commit '$Commit' neste repositorio. Rode 'git pull' e confira com .\scripts\deploy\verificar-publicacao.ps1"
    }
    Write-Host "   commit $Commit = $inteiro"
    $Commit = $inteiro
}

# -------------------------------------------------------------------------------------------------
Write-Host ''
Write-Host '== O que esta esperando' -ForegroundColor Cyan
# -------------------------------------------------------------------------------------------------
# O AGENTE PRECISA EXISTIR. Sem ele nao ha publicacao automatica nenhuma, e portanto nada esperando
# autorizacao - a mensagem generica "nao esta esperando" escondia isso (primeiro uso real, 21/09/2026).
$pastaPorSmb = "\\$Servidor\$($PastaDoAgente -replace ':', '$')"
if (-not (Test-Path $pastaPorSmb)) {
    Write-Host "   O agente de publicacao NAO esta instalado em $Servidor ($PastaDoAgente nao existe)." -ForegroundColor Yellow
    Write-Host '   Sem ele, nada e publicado sozinho - e nada fica esperando autorizacao. Instale uma vez:'
    Write-Host '      .\scripts\deploy\instalar-agente-de-publicacao.ps1'
    Write-Host '   Em ate 5 minutos ele encontra a main; se houver migracao destrutiva, para e espera por este script.'
    return
}

$estado = Invoke-NoServidor -Nome 'crm-ler-estado' -TimeoutSegundos 120 -Script @"
`$e = Join-Path '$PastaDoAgente' 'estado.json'
if (Test-Path `$e) { Get-Content `$e -Raw } else { '{}' }
"@

# O transcript do _remoto.ps1 embrulha a saida; o JSON e a parte entre as chaves.
$json = [regex]::Match($estado, '(?s)\{.*\}').Value
$e = if ($json) { $json | ConvertFrom-Json } else { $null }

# UMA PUBLICACAO QUE FALHOU TAMBEM PASSA POR AQUI. O agente nao tenta o mesmo commit de novo sozinho
# (21/09/2026: sem isso, ele refazia copia de seguranca, troca e volta atras a cada cinco minutos), e
# pedir outra tentativa e decisao de gente - como autorizar uma migracao destrutiva.
if ($e -and $e.situacao -eq 'falhou') {
    if ($e.commitQueFalhou -and $e.commitQueFalhou -ne $Commit) {
        throw "A publicacao que falhou foi a do commit $($e.commitQueFalhou), e nao $Commit. Nada foi feito."
    }
    Write-Host "   commit ............ $Commit"
    Write-Host "   falhou ............ $($e.detalhe)"
    Write-Host ''
    Write-Host '   Veja o motivo antes de pedir de novo:  .\scripts\deploy\verificar-publicacao.ps1'
    Write-Host '   Se o defeito era dos scripts do agente, atualize-os - a nova tentativa entao e automatica:'
    Write-Host '      .\scripts\deploy\instalar-agente-de-publicacao.ps1 -SoAtualizarOsScripts'
    Write-Host ''

    $resposta = Read-Host '   Tentar publicar este commit de novo? (digite TENTAR)'
    if ($resposta -ne 'TENTAR') {
        Write-Host '   Nada foi feito.' -ForegroundColor Yellow
        return
    }
    if (-not $Motivo) { $Motivo = Read-Host '   Por que (uma linha, fica no registro)' }

    $quem = "$env:USERDOMAIN\$env:USERNAME"
    $quando = (Get-Date).ToString('o')

    $saida = Invoke-NoServidor -Nome 'crm-tentar-de-novo' -TimeoutSegundos 120 -Script @"
`$arquivo = Join-Path '$PastaDoAgente' 'tentar-de-novo-$Commit.txt'
Set-Content -LiteralPath `$arquivo -Encoding ASCII -Value @'
pedido por: $quem
em: $quando
motivo: $Motivo
'@
Write-Output ('gravado: ' + `$arquivo)
schtasks /Run /TN 'TracbelCrmPublicacao'
Write-Output ('disparo da tarefa: ' + `$LASTEXITCODE)
"@

    Write-Host $saida
    Write-Host ''
    Write-Host '   Pedido. Acompanhe com:' -ForegroundColor Green
    Write-Host '      .\scripts\deploy\verificar-publicacao.ps1 -Log'
    Write-Host ''
    return
}

if (-not $e -or $e.situacao -ne 'aguardandoAutorizacao') {
    Write-Host "   O agente nao esta esperando autorizacao nenhuma (situacao: $($e.situacao))." -ForegroundColor Yellow
    Write-Host '   Nada foi feito.'
    return
}

if ($e.commitAguardando -ne $Commit) {
    throw "O agente esta esperando o commit $($e.commitAguardando), e nao $Commit. Nada foi feito."
}

Write-Host "   commit ............ $Commit"
Write-Host "   migracoes ......... $($e.migracoesDestrutivas -join ', ')"
Write-Host ''
Write-Host '   O QUE VAI SER APAGADO:' -ForegroundColor Yellow
foreach ($linha in ($e.detalhe -split '\|')) { Write-Host "      $($linha.Trim())" -ForegroundColor Yellow }
Write-Host ''
Write-Host '   O agente faz uma copia de seguranca COPY_ONLY antes de aplicar. Voltar atras de uma'
Write-Host '   migracao destrutiva e RESTAURAR essa copia - nao e reverter o commit.'
Write-Host ''

$resposta = Read-Host '   Autorizar esta publicacao? (digite AUTORIZO)'
if ($resposta -ne 'AUTORIZO') {
    Write-Host '   Nada foi autorizado.' -ForegroundColor Yellow
    return
}

if (-not $Motivo) { $Motivo = Read-Host '   Por que (uma linha, fica no registro)' }

# -------------------------------------------------------------------------------------------------
Write-Host ''
Write-Host '== Gravando a autorizacao no servidor' -ForegroundColor Cyan
# -------------------------------------------------------------------------------------------------
$quem = "$env:USERDOMAIN\$env:USERNAME"
$quando = (Get-Date).ToString('o')

# A autorizacao e um ARQUIVO com o nome do commit: o agente procura exatamente por ele. Um pacote
# novo tem outro commit, e para de novo - a autorizacao nao vira um interruptor permanente.
$saida = Invoke-NoServidor -Nome 'crm-autorizar' -TimeoutSegundos 120 -Script @"
`$arquivo = Join-Path '$PastaDoAgente' 'autorizado-$Commit.txt'
Set-Content -LiteralPath `$arquivo -Encoding ASCII -Value @'
autorizado por: $quem
em: $quando
motivo: $Motivo
'@
Write-Output ('gravado: ' + `$arquivo)

# RODA NA HORA, para nao esperar a proxima passada do agendamento.
schtasks /Run /TN 'TracbelCrmPublicacao'
Write-Output ('disparo da tarefa: ' + `$LASTEXITCODE)
"@

Write-Host $saida
Write-Host ''
Write-Host '   Autorizado. Acompanhe com:' -ForegroundColor Green
Write-Host '      .\scripts\deploy\verificar-publicacao.ps1 -Log'
Write-Host ''
