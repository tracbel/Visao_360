<#
  _remoto.ps1 - executa um passo de instalacao no servidor de aplicacao 10.150.4.249.

  CONTEXTO: implantacao autorizada do CRM Tracbel Agro no servidor interno da propria empresa,
  conduzida pelo responsavel de TI. O mesmo servidor ja hospeda o projeto user-onboarding, cujo
  deploy usa este mesmo mecanismo (ver user-onboarding\deploy.ps1).

  POR QUE TAREFA AGENDADA, E NAO WinRM. O servidor NAO esta no dominio: WinRM por IP exigiria
  TrustedHosts nesta estacao mais credencial explicita, ou seja, guardar a senha de um
  administrador local em arquivo. O agendador aceita a credencial de dominio que o operador ja
  tem - e o direito de administrador esta comprovado, porque escrever em C$ so administrador
  consegue.

  O script vai por SMB, roda, escreve a saida num arquivo e a tarefa e APAGADA em seguida. Nada
  fica agendado no servidor.
#>

$Global:Servidor   = '10.150.4.249'
$Global:RaizRemota = 'C:\projetos\tracbel-crm-deploy'
$Global:RaizPorSmb = "\\10.150.4.249\C$\projetos\tracbel-crm-deploy"

function Invoke-NoServidor {
    param(
        [Parameter(Mandatory = $true)] [string] $Script,
        [string] $Nome = 'crm-passo',
        [int]    $TimeoutSegundos = 900
    )

    New-Item -ItemType Directory -Force -Path $Global:RaizPorSmb | Out-Null

    $id        = "$Nome-$(Get-Random -Maximum 999999)"
    $psLocal   = Join-Path $Global:RaizPorSmb "$id.ps1"
    $saidaLocal= Join-Path $Global:RaizPorSmb "$id.out"
    $psRemoto  = Join-Path $Global:RaizRemota "$id.ps1"
    $saidaRem  = Join-Path $Global:RaizRemota "$id.out"

    # A SAIDA E REDIRECIONADA DENTRO DO SCRIPT. A tarefa agendada devolve codigo de saida e mais
    # nada; sem o transcript, um erro do comando remoto seria invisivel - que e exatamente o tipo
    # de silencio que este projeto persegue.
    $envolvido = @"
`$ErrorActionPreference = 'Continue'
Start-Transcript -Path '$saidaRem' -Force | Out-Null
try {
$Script
} catch {
  Write-Output ('ERRO: ' + `$_.Exception.Message)
}
Stop-Transcript | Out-Null
"@

    # ASCII PURO. O PowerShell 5.1 do servidor le .ps1 como ANSI, e um acento sem BOM quebra o
    # parser - defeito que ja custou uma extracao inteira neste projeto.
    Set-Content -Path $psLocal -Value $envolvido -Encoding ASCII

    $comando = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$psRemoto`""
    & schtasks /Create /S $Global:Servidor /TN $id /TR $comando /SC ONCE /ST 23:59 /RU SYSTEM /RL HIGHEST /F | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Nao consegui criar a tarefa $id no servidor." }

    & schtasks /Run /S $Global:Servidor /TN $id | Out-Null

    $limite = (Get-Date).AddSeconds($TimeoutSegundos)
    $terminou = $false
    while ((Get-Date) -lt $limite) {
        Start-Sleep -Seconds 3
        $linha = & schtasks /Query /S $Global:Servidor /TN $id /FO LIST 2>&1 | Select-String 'Status:|Estado:'
        if ($linha -and ($linha -join ' ') -notmatch 'Running|Em execu') { $terminou = $true; break }
    }

    & schtasks /Delete /S $Global:Servidor /TN $id /F | Out-Null
    Remove-Item $psLocal -Force -ErrorAction SilentlyContinue

    if (-not $terminou) { Write-Warning "O passo $Nome passou de $TimeoutSegundos s; a saida pode estar parcial." }

    if (Test-Path $saidaLocal) {
        $texto = Get-Content $saidaLocal -Raw
        Remove-Item $saidaLocal -Force -ErrorAction SilentlyContinue
        # O transcript embrulha tudo em cabecalho e rodape; so o miolo interessa.
        $miolo = ($texto -split '\*{22}') | Select-Object -Skip 2
        return (($miolo -join '') -replace '(?m)^(Transcript|Comando de in|Host de |Nome de usu|Computador|Vers|Configura|Aplicativo|Processo|ID de).*$', '').Trim()
    }

    return '(sem saida)'
}
