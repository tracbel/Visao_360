<#
  autorizar-publicacao.ps1 - libera uma publicacao que PAROU por ter migracao destrutiva (issue 62).

      .\scripts\deploy\autorizar-publicacao.ps1 -Commit a39fa53ba4d7e1846493a6bacfab1c5839f54f89

  =================================================================================================
  QUANDO ISTO E NECESSARIO

  Quase nunca. A decisao de 20/09/2026 e que migracao ADITIVA (criar tabela, criar coluna) sobe
  sozinha — que e a esmagadora maioria. Este script existe para o outro caso: quando a subida vai
  APAGAR tabela ou coluna.

  "Publicar e migrar" (regra R-4 do doc 46). Uma migracao destrutiva nao se desfaz com `git revert`:
  desfaz-se restaurando o backup. Por isso ela passa por uma pessoa.

  =================================================================================================
  O QUE ELE MOSTRA ANTES DE PERGUNTAR

  O que exatamente vai ser apagado, tabela por tabela e coluna por coluna, como o CI as encontrou no
  SQL que a subida vai rodar. Autorizar sem ver isso seria o mesmo que nao ter portao nenhum.

  A autorizacao vale para UM commit: um pacote novo, com outra migracao destrutiva, para de novo.
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

if ($Commit -notmatch '^[0-9a-f]{40}$') {
    throw "Passe o commit inteiro, com 40 caracteres. Veja qual e com .\scripts\deploy\verificar-publicacao.ps1"
}

# -------------------------------------------------------------------------------------------------
Write-Host ''
Write-Host '== O que esta esperando' -ForegroundColor Cyan
# -------------------------------------------------------------------------------------------------
$estado = Invoke-NoServidor -Nome 'crm-ler-estado' -TimeoutSegundos 120 -Script @"
`$e = Join-Path '$PastaDoAgente' 'estado.json'
if (Test-Path `$e) { Get-Content `$e -Raw } else { '{}' }
"@

# O transcript do _remoto.ps1 embrulha a saida; o JSON e a parte entre as chaves.
$json = [regex]::Match($estado, '(?s)\{.*\}').Value
$e = if ($json) { $json | ConvertFrom-Json } else { $null }

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
Write-Host '   migracao destrutiva e RESTAURAR essa copia — nao e reverter o commit.'
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
# novo tem outro commit, e para de novo — a autorizacao nao vira um interruptor permanente.
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
