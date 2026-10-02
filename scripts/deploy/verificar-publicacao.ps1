<#
  verificar-publicacao.ps1 - o que o agente do servidor esta fazendo (issue 62).

      .\scripts\deploy\verificar-publicacao.ps1
      .\scripts\deploy\verificar-publicacao.ps1 -Log        # mostra o ultimo log inteiro

  So leitura. Responde quatro coisas: em que versao o servidor esta, se ha alguma coisa esperando
  autorizacao, o que aconteceu na ultima rodada, e o que a ultima limpeza do servidor achou (o log do
  banco, a memoria, o disco e as sessoes de Area de Trabalho Remota desconectadas).
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

# A FALHA MOSTRA O LOG DELA, e nao o da ultima rodada: depois de falhar, o agente passa a acordar e ir
# embora a cada cinco minutos, e o log mais novo so diria isso.
if (`$e.situacao -eq 'falhou') {
    Write-Output ''
    Write-Output '=== A ULTIMA PUBLICACAO FALHOU ==='
    Write-Output ('commit ............... ' + `$e.commitQueFalhou)
    Write-Output 'O agente nao tenta este commit de novo sozinho: so com commit novo, com os scripts'
    Write-Output 'corrigidos (instalar-agente-de-publicacao.ps1 -SoAtualizarOsScripts), ou a pedido:'
    Write-Output ('   .\scripts\deploy\autorizar-publicacao.ps1 -Commit ' + `$e.commitQueFalhou)
    if (`$e.ultimoLog -and (Test-Path `$e.ultimoLog)) {
        Write-Output ''
        Write-Output ('--- ' + `$e.ultimoLog + ' (fim)')
        Get-Content `$e.ultimoLog -Tail 15
    }
}

Write-Output ''
Write-Output '=== tarefa agendada ==='
& schtasks.exe /Query /TN 'TracbelCrmPublicacao' /FO LIST /V 2>&1 |
    Select-String 'Nome da tarefa|Task Name|Pr.xima|Next Run|Status|.ltimo Resultado|Last Result'

# A LIMPEZA DO SERVIDOR (02/10/2026): o que a ultima rodada achou, e os avisos dela - entre eles as sessoes
# de Area de Trabalho Remota esquecidas abertas, que ela so avisa e nao encerra.
Write-Output ''
Write-Output '=== limpeza do servidor ==='
`$limpeza = Join-Path `$pasta 'limpeza.json'
if (Test-Path `$limpeza) {
    `$l = Get-Content `$limpeza -Raw | ConvertFrom-Json
    Write-Output ('ultima rodada ........ ' + `$l.quando)
    Write-Output ('log do banco ......... ' + `$l.banco.logMbDepois + ' MB (recuperacao ' + `$l.banco.recuperacao + ')')
    Write-Output ('copias apagadas ...... ' + `$l.copiasDoAgente.apagadas + ' (' + `$l.copiasDoAgente.gbLiberados + ' GB)')
    Write-Output ('memoria disponivel ... ' + `$l.memoria.disponivelMb + ' MB de ' + `$l.memoria.totalMb + ' MB (cache ' + `$l.memoria.cacheMb + ' MB)')
    Write-Output ('o CRM ocupa .......... API ' + `$l.memoria.apiMb + ' MB, sincronizacao ' + `$l.memoria.sincronizacaoMb + ' MB, SQL Server ' + `$l.memoria.sqlServerMb + ' MB')
    Write-Output ('disco C: ............. ' + `$l.disco.livreGb + ' GB livres de ' + `$l.disco.totalGb + ' GB')
    foreach (`$s in `$l.sessoesDesconectadas) {
        Write-Output ('sessao desconectada .. ' + `$s.usuario + ', ha ' + `$s.horas + ' h, ' + `$s.memoriaMb + ' MB')
    }
    foreach (`$a in `$l.avisos) { Write-Output ('AVISO: ' + `$a) }
    foreach (`$e in `$l.erros) { Write-Output ('ERRO: ' + `$e) }
} else {
    Write-Output 'A limpeza ainda nao rodou: rode instalar-agente-de-publicacao.ps1 -SoAtualizarOsScripts.'
}
& schtasks.exe /Query /TN 'TracbelCrmLimpeza' /FO LIST /V 2>&1 |
    Select-String 'Nome da tarefa|Task Name|Pr.xima|Next Run|.ltimo Resultado|Last Result'

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
