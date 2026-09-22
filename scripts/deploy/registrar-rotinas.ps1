<#
  registrar-rotinas.ps1 - registra, NO SERVIDOR, o ORQUESTRADOR das rotinas do CRM (issue 136; antes, as
  duas tarefas das fontes publicas das issues 64, 65 e 66).

  Roda NO SERVIDOR, como administrador ou SYSTEM. Quem chama:
    - o `publicar-pacote.ps1`, a CADA publicacao automatica, com o script que veio DENTRO do pacote;
    - o `publicar.ps1` e o `agendar-fontes-publicas-no-servidor.ps1`, da estacao.

  =================================================================================================
  O QUE MUDOU EM 22/09/2026 (issue 136)

  Ate aqui havia duas tarefas do Windows com o calendario escrito neste script:

    TracbelCrmFontesPublicas  ANUAL, 1 de outubro, 03:00
    TracbelCrmPrecos          MENSAL, dia 20, 04:00

  Mudar a data exigia editar este arquivo e publicar. Agora a AGENDA MORA NO BANCO (integracao.Rotina),
  editavel em Configuracoes > Integracoes, e o servidor tem UMA tarefa:

    TracbelCrmOrquestrador    a cada 5 minutos: Tracbel.Crm.Carga.exe --orquestrar

  que le a agenda, roda o que venceu (ou o que alguem pediu com "Rodar agora"), grava o resultado em
  integracao.ExecucaoDeRotina e testa as APIs monitoradas. As duas tarefas antigas sao APAGADAS aqui,
  depois que a nova entra - deixar as tres faria a mesma carga rodar duas vezes.

  A PRIMEIRA CARGA CONTINUA NAO ESPERANDO O CALENDARIO: o orquestrador roda na primeira volta a rotina
  que nunca rodou e tem tabela vazia. Nao ha mais disparo daqui.

  =================================================================================================
  POR QUE O ORQUESTRADOR RODA DE UMA COPIA DA CARGA

  A publicacao APAGA a pasta da carga antes de copiar a versao nova (publicar-pacote.ps1, passo 4). Com
  uma tarefa rodando dali a cada cinco minutos, o executavel estaria aberto justamente quando a publicacao
  tenta apaga-lo - e a publicacao falharia. Por isso o orquestrador roda de
  C:\aplicacoes\tracbel-crm-rotinas\carga, uma copia que o proprio script da rotina atualiza, e SO
  quando a pasta de origem parou de mudar ha dois minutos: nunca se copia uma publicacao pela metade.

  =================================================================================================
  POR QUE UMA PASTA PROPRIA (C:\aplicacoes\tracbel-crm-rotinas)

  A publicacao apaga a pasta da carga; os scripts das rotinas, os logs e a copia da carga moram fora
  dela, e nenhuma troca de versao os alcanca (achado de 21/09/2026).

  IDEMPOTENTE: o script da rotina e reescrito e a tarefa recriada com /F. Rodar duas vezes da o mesmo
  resultado. A ultima linha da saida e "codigo das rotinas: N", e N = 0 e sucesso.

  ASCII PURO, sem travessao nem acento: o PowerShell 5.1 do servidor le .ps1 sem BOM como ANSI.
#>

[CmdletBinding()]
param(
    # A pasta da carga publicada - onde esta o Tracbel.Crm.Carga.exe.
    [string] $DestinoDaCarga = 'C:\aplicacoes\tracbel-crm-carga',

    # A pasta das rotinas: o script, os logs e a copia da carga. Fora da pasta da carga, de proposito.
    [string] $PastaDasRotinas = 'C:\aplicacoes\tracbel-crm-rotinas',

    # A conexao LOCAL e INTEGRADA: a tarefa roda como SYSTEM, que no dominio e a conta da maquina, e o banco
    # esta na mesma maquina. Nenhuma senha em arquivo.
    [string] $Conexao = 'Server=localhost,1433;Database=TracbelCrm;Integrated Security=True;TrustServerCertificate=True',

    [string] $NomeTarefa = 'TracbelCrmOrquestrador',

    # De quantos em quantos minutos o orquestrador acorda. A agenda de cada rotina e do banco.
    [int]    $MinutosEntreVoltas = 5,

    # AS TAREFAS DE ANTES, apagadas quando o orquestrador entra. Os parametros de calendario que o
    # agendar-fontes-publicas-no-servidor.ps1 ainda passa sao aceitos e ignorados: a agenda e do banco.
    [string[]] $TarefasAntigas = @('TracbelCrmFontesPublicas', 'TracbelCrmPrecos', 'TracbelCrmPam'),
    [string] $NomeTarefaAnual  = '',
    [string] $MesAnual         = '',
    [int]    $DiaAnual         = 0,
    [string] $HoraAnual        = '',
    [string] $NomeTarefaMensal = '',
    [int]    $DiaMensal        = 0,
    [string] $HoraMensal       = '',
    [string] $TarefaAntiga     = '',
    [switch] $NaoDispararPrimeiraCarga
)

$ErrorActionPreference = 'Stop'

foreach ($pasta in $DestinoDaCarga, $PastaDasRotinas) {
    # PASTA SEM ESPACO. O /TR do schtasks guarda uma linha de comando, e um espaco no caminho exigiria
    # mais um nivel de aspas dentro de uma string que ja atravessa tres.
    if ($pasta -match '\s') { throw "A pasta nao pode ter espaco: '$pasta'." }
}

New-Item -ItemType Directory -Force -Path (Join-Path $PastaDasRotinas 'logs') | Out-Null

# -------------------------------------------------------------------------------------------------
# O script da rotina
# -------------------------------------------------------------------------------------------------
# A COPIA SO E ATUALIZADA QUANDO A ORIGEM PAROU DE MUDAR. O sinal e a data de CRIACAO do arquivo mais novo
# da pasta da carga: a publicacao cria os arquivos na hora da copia, e a data de ALTERACAO vem do pacote. Com
# o mais novo criado ha mais de dois minutos, a copia terminou.
#
# UMA CODIFICACAO SO NO LOG, E UTF-8. Misturar Tee-Object (UTF-16 no 5.1) com Add-Content (ANSI) no mesmo
# arquivo deu, em 20/09/2026, um log ilegivel.
$script = Join-Path $PastaDasRotinas 'rodar-orquestrador.ps1'
$sombra = Join-Path $PastaDasRotinas 'carga'
$logs   = Join-Path $PastaDasRotinas 'logs'

$conteudo = @"
# rodar-orquestrador.ps1 - gerado por registrar-rotinas.ps1, a cada publicacao.
# Nao edite aqui: a proxima publicacao reescreve. Edite scripts/deploy/registrar-rotinas.ps1.
`$ErrorActionPreference = 'Continue'
`$env:ConnectionStrings__Crm = '$Conexao'
`$origem = '$DestinoDaCarga'
`$sombra = '$sombra'
`$pasta  = '$logs'
New-Item -ItemType Directory -Force -Path `$pasta | Out-Null
`$log = Join-Path `$pasta ('orquestrador-' + (Get-Date -Format 'yyyyMMdd') + '.log')
function Anotar(`$texto) { Out-File -FilePath `$log -Append -Encoding utf8 -InputObject `$texto }

`$exe = Join-Path `$origem 'Tracbel.Crm.Carga.exe'
if (Test-Path `$exe) {
    `$maisNovo = (Get-ChildItem `$origem -Recurse -File | Sort-Object CreationTime -Descending | Select-Object -First 1).CreationTime
    `$marca = Join-Path `$sombra '.copiado-de'
    `$anterior = ''
    if (Test-Path `$marca) { `$anterior = (Get-Content `$marca -Raw).Trim() }
    `$assinatura = `$maisNovo.ToUniversalTime().Ticks.ToString()
    if ((`$maisNovo -lt (Get-Date).AddMinutes(-2)) -and (`$assinatura -ne `$anterior)) {
        & robocopy `$origem `$sombra /MIR /R:2 /W:5 /NFL /NDL /NJH /NJS /NP | Out-Null
        if (`$LASTEXITCODE -lt 8) {
            Set-Content -Path `$marca -Value `$assinatura -Encoding ASCII
            Anotar ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' copia da carga atualizada')
        } else {
            Anotar ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' robocopy falhou com codigo ' + `$LASTEXITCODE + '; segue a copia anterior')
        }
    }
}

`$rodar = Join-Path `$sombra 'Tracbel.Crm.Carga.exe'
if (-not (Test-Path `$rodar)) {
    Anotar ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' ainda nao ha copia da carga em ' + `$sombra)
    exit 2
}

& `$rodar '--orquestrar' '--pasta-de-logs' `$pasta *>&1 | ForEach-Object { Anotar `$_ }
`$codigo = `$LASTEXITCODE
Anotar ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' codigo de saida: ' + `$codigo)
Get-ChildItem `$pasta -Filter '*.log' | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddYears(-3) } | Remove-Item -Force
exit `$codigo
"@

Set-Content -Path $script -Encoding ASCII -Value $conteudo

$pior = 0

# OS GRUPOS VAO POR SID, E NAO POR NOME: num Windows em portugues 'SYSTEM' se chama 'SISTEMA', e o icacls com
# o nome em ingles falha DEPOIS de ter tirado a heranca, deixando o arquivo sem dono.
& icacls $script /inheritance:r /grant '*S-1-5-18:(RX)' '*S-1-5-32-544:(RX)' | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Output "icacls falhou em $script (codigo $LASTEXITCODE)"; $pior = [Math]::Max($pior, $LASTEXITCODE) }

# -------------------------------------------------------------------------------------------------
# A tarefa
# -------------------------------------------------------------------------------------------------
# OS ARGUMENTOS VAO NUMA LISTA: com linha de comando escrita a mao, a continuacao com crase nao sobreviveu a
# viagem pelo _remoto.ps1 ("Mandatory option 'sc' is missing").
#
# UMA INSTANCIA DE CADA VEZ: a tarefa criada pelo schtasks nao inicia outra enquanto a anterior roda (a PAM
# leva perto de uma hora). O orquestrador ainda tem a trava dele no banco.
$argumentos = @('/Create', '/TN', $NomeTarefa,
    '/TR', "powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script",
    '/SC', 'MINUTE', '/MO', "$MinutosEntreVoltas",
    '/RU', 'SYSTEM', '/RL', 'HIGHEST', '/F')

& schtasks.exe @argumentos | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Output "schtasks falhou em $NomeTarefa (codigo $LASTEXITCODE)"; $pior = [Math]::Max($pior, $LASTEXITCODE) }
else { Write-Output "tarefa $NomeTarefa registrada: a cada $MinutosEntreVoltas minutos, como SYSTEM" }

# AS TAREFAS ANTIGAS SAEM DEPOIS QUE A NOVA ENTRA, e so entao.
#
# PELO Get-ScheduledTask, E NAO PELO schtasks /Query. No PowerShell 5.1, com $ErrorActionPreference = 'Stop', o
# stderr de um executavel vira EXCECAO quando redirecionado - mesmo mandado para $null (21/09/2026).
if ($pior -eq 0) {
    foreach ($antiga in $TarefasAntigas) {
        if ($antiga -and (Get-ScheduledTask -TaskName $antiga -ErrorAction SilentlyContinue)) {
            & schtasks.exe /Delete /TN $antiga /F | Out-Null
            Write-Output "tarefa antiga $antiga removida: a agenda agora e do banco (integracao.Rotina)"
        }
    }
}

# OS SCRIPTS DAS TAREFAS ANTIGAS sao restos: a do orquestrador substitui os dois.
foreach ($resto in 'rodar-fontes-publicas.ps1', 'rodar-pam.ps1', 'rodar-precos.ps1') {
    Remove-Item (Join-Path $PastaDasRotinas $resto) -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $DestinoDaCarga $resto) -Force -ErrorAction SilentlyContinue
}

Write-Output 'primeira carga: a cargo do orquestrador - a rotina que nunca rodou e tem tabela vazia roda na primeira volta'
Write-Output "codigo das rotinas: $pior"
exit $pior
