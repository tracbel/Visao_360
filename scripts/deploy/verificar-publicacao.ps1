<#
  verificar-publicacao.ps1 - o que o agente do servidor esta fazendo (issue 62).

      .\scripts\deploy\verificar-publicacao.ps1
      .\scripts\deploy\verificar-publicacao.ps1 -Log        # mostra o ultimo log inteiro

  So leitura. Responde tres coisas: em que versao o servidor esta, se ha alguma coisa esperando
  autorizacao, e o que aconteceu na ultima rodada.
#>

[CmdletBinding()]
param(
    [string] $Servidor = '10.150.4.249',
    [string] $PastaDoAgente = 'C:\aplicacoes\tracbel-crm-agente',
    [switch] $Log
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_remoto.ps1')

$saida = Invoke-NoServidor -Nome 'crm-ver-publicacao' -TimeoutSegundos 180 -Script @"
`$pasta = '$PastaDoAgente'
`$estado = Join-Path `$pasta 'estado.json'

if (-not (Test-Path `$estado)) {
    Write-Output 'O agente ainda nao rodou nenhuma vez (nao ha estado.json).'
    return
}

`$e = Get-Content `$estado -Raw | ConvertFrom-Json
Write-Output ('situacao ............. ' + `$e.situacao)
Write-Output ('detalhe .............. ' + `$e.detalhe)
Write-Output ('versao publicada ..... ' + `$e.versaoPublicada)
Write-Output ('publicada em ......... ' + `$e.publicadaEm)
Write-Output ('ultima verificacao ... ' + `$e.ultimaVerificacaoEm)

if (`$e.situacao -eq 'aguardandoAutorizacao') {
    Write-Output ''
    Write-Output '=== ESPERANDO AUTORIZACAO ==='
    Write-Output ('commit ............... ' + `$e.commitAguardando)
    Write-Output 'migracoes destrutivas:'
    foreach (`$m in `$e.migracoesDestrutivas) { Write-Output ('   ' + `$m) }
    Write-Output ''
    Write-Output 'Para autorizar, rode da estacao:'
    Write-Output ('   .\scripts\deploy\autorizar-publicacao.ps1 -Commit ' + `$e.commitAguardando)
}

Write-Output ''
Write-Output '=== tarefa agendada ==='
& schtasks.exe /Query /TN 'TracbelCrmPublicacao' /FO LIST /V 2>&1 |
    Select-String 'Nome da tarefa|Task Name|Pr.xima|Next Run|Status|.ltimo Resultado|Last Result'

if ('$($Log.IsPresent)' -eq 'True') {
    Write-Output ''
    Write-Output '=== ultimo log ==='
    `$ultimo = Get-ChildItem (Join-Path `$pasta 'logs') -Filter '*.log' -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime | Select-Object -Last 1
    if (`$ultimo) {
        Write-Output (`$ultimo.FullName)
        Get-Content `$ultimo.FullName
    } else { Write-Output '(nenhum log ainda)' }
}
"@

Write-Host $saida
