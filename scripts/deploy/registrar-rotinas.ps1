<#
  registrar-rotinas.ps1 - cria ou atualiza, NO SERVIDOR, as rotinas agendadas das fontes publicas
  (issues 64, 65 e 66; regra R-5 do doc 46).

  Roda NO SERVIDOR, como administrador ou SYSTEM. Quem chama:
    - o `publicar-pacote.ps1`, a CADA publicacao automatica, com o script que veio DENTRO do pacote;
    - o `agendar-fontes-publicas-no-servidor.ps1`, da estacao, antes de o agente estar instalado.

  =================================================================================================
  AS ROTINAS

    TracbelCrmFontesPublicas  ANUAL, 1 de outubro, 03:00  --somente-pam e --somente-estrutura
    TracbelCrmPrecos          MENSAL, dia 20, 04:00       --somente-precos, --somente-custos e --somente-credito

  Por que duas, por que o dia 20 e por que outubro: ver o cabecalho de
  agendar-fontes-publicas-no-servidor.ps1.

  =================================================================================================
  POR QUE ESTE SCRIPT VIAJA NO PACOTE

  As rotinas mudam junto com a carga: a issue 66 trouxe o --somente-precos, as proximas trarao os
  custos da CONAB e o SICOR. Se a definicao delas morasse so num script de estacao, cada fonte nova
  exigiria alguem lembrar de roda-lo - exatamente o passo manual que a publicacao automatica existe
  para eliminar. Vindo no pacote, a rotina nova chega ao servidor com o codigo que a usa.

  =================================================================================================
  POR QUE UMA PASTA PROPRIA (C:\aplicacoes\tracbel-crm-rotinas)

  A publicacao APAGA a pasta da carga antes de copiar a versao nova (publicar-pacote.ps1, passo 4).
  Os scripts das rotinas e os logs moravam la: a primeira publicacao automatica os teria apagado, e a
  tarefa anual falharia em silencio em outubro - achado em 21/09/2026, antes de o agente ser
  instalado. Fora da pasta da carga, nenhuma troca de versao os alcanca.

  =================================================================================================
  A PRIMEIRA CARGA NAO ESPERA O CALENDARIO. Registrar uma tarefa nao a roda. Por isso, ao fim, cada
  rotina e conferida pelas SUAS tabelas, e disparada na hora se alguma estiver vazia - venha a chamada do
  agente, do agendamento pela estacao ou do publicar.ps1. MEDIDO em 21/09/2026: as rotinas foram
  registradas as 11:41, a mensal nunca tinha rodado e a proxima execucao era 20/10 - a tela ficaria um
  mes sem preco no servidor.

  IDEMPOTENTE: os scripts sao reescritos e as tarefas recriadas com /F. Rodar duas vezes da o mesmo
  resultado. A ultima linha da saida e "codigo das rotinas: N", e N = 0 e sucesso.

  ASCII PURO, sem travessao nem acento: o PowerShell 5.1 do servidor le .ps1 sem BOM como ANSI.
#>

[CmdletBinding()]
param(
    # A pasta da carga publicada - onde esta o Tracbel.Crm.Carga.exe.
    [string] $DestinoDaCarga = 'C:\aplicacoes\tracbel-crm-carga',

    # A pasta das rotinas: os scripts e os logs. Fora da pasta da carga, de proposito.
    [string] $PastaDasRotinas = 'C:\aplicacoes\tracbel-crm-rotinas',

    # A conexao LOCAL e INTEGRADA: as tarefas rodam como SYSTEM, que no dominio e a conta da maquina,
    # e o banco esta na mesma maquina. Nenhuma senha em arquivo.
    [string] $Conexao = 'Server=localhost,1433;Database=TracbelCrm;Integrated Security=True;TrustServerCertificate=True',

    [string] $NomeTarefaAnual  = 'TracbelCrmFontesPublicas',
    [string] $MesAnual         = 'OCT',
    [int]    $DiaAnual         = 1,
    [string] $HoraAnual        = '03:00',

    [string] $NomeTarefaMensal = 'TracbelCrmPrecos',
    [int]    $DiaMensal        = 20,
    [string] $HoraMensal       = '04:00',

    # A tarefa que a primeira versao do agendamento instalava, e que foi substituida.
    [string] $TarefaAntiga     = 'TracbelCrmPam',

    # Para quem vai rodar as tarefas ele mesmo e esperar por elas (agendar-fontes-publicas-no-servidor.ps1).
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
# Os scripts das rotinas
# -------------------------------------------------------------------------------------------------
# AS CARGAS DE UMA ROTINA RODAM MESMO QUE A PRIMEIRA FALHE, e o codigo de saida e o PIOR delas: uma
# indisponibilidade do SIDRA nao pode levar junto a leitura da ANP.
#
# UMA CODIFICACAO SO NO LOG, E UTF-8. Misturar Tee-Object (UTF-16 no 5.1) com Add-Content (ANSI) no
# mesmo arquivo deu, em 20/09/2026, um log ilegivel - "l i n h a s   m a n t i d a s".
function Rotina([string] $arquivo, [string] $prefixoDoLog, [string[]] $modos) {
    $lista = ($modos | ForEach-Object { "'$_'" }) -join ', '
    return @"
# $arquivo - gerado por registrar-rotinas.ps1, a cada publicacao.
# Nao edite aqui: a proxima publicacao reescreve. Edite scripts/deploy/registrar-rotinas.ps1.
`$ErrorActionPreference = 'Continue'
`$env:ConnectionStrings__Crm = '$Conexao'
`$pasta = '$PastaDasRotinas\logs'
New-Item -ItemType Directory -Force -Path `$pasta | Out-Null
`$log = Join-Path `$pasta ('$prefixoDoLog-' + (Get-Date -Format 'yyyyMMdd-HHmm') + '.log')

function Anotar(`$texto) { Out-File -FilePath `$log -Append -Encoding utf8 -InputObject `$texto }

`$pior = 0
foreach (`$modo in $lista) {
    Anotar ''
    Anotar ('=== ' + `$modo + ' em ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
    & '$DestinoDaCarga\Tracbel.Crm.Carga.exe' `$modo *>&1 | ForEach-Object { Anotar `$_ }
    `$codigo = `$LASTEXITCODE
    Anotar ('codigo de saida de ' + `$modo + ': ' + `$codigo)
    if (`$codigo -gt `$pior) { `$pior = `$codigo }
}
Anotar ('codigo de saida: ' + `$pior)
Get-ChildItem `$pasta -Filter '$prefixoDoLog-*.log' | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddYears(-3) } | Remove-Item -Force
exit `$pior
"@
}

$anual  = Join-Path $PastaDasRotinas 'rodar-fontes-publicas.ps1'
$mensal = Join-Path $PastaDasRotinas 'rodar-precos.ps1'

Set-Content -Path $anual  -Encoding ASCII -Value (Rotina 'rodar-fontes-publicas.ps1' 'fontes-publicas' @('--somente-pam', '--somente-estrutura'))
Set-Content -Path $mensal -Encoding ASCII -Value (Rotina 'rodar-precos.ps1' 'precos' @('--somente-precos', '--somente-custos', '--somente-credito'))

$pior = 0

# OS GRUPOS VAO POR SID, E NAO POR NOME: num Windows em portugues 'SYSTEM' se chama 'SISTEMA', e o
# icacls com o nome em ingles falha DEPOIS de ter tirado a heranca, deixando o arquivo sem dono.
foreach ($a in $anual, $mensal) {
    & icacls $a /inheritance:r /grant '*S-1-5-18:(RX)' '*S-1-5-32-544:(RX)' | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Output "icacls falhou em $a (codigo $LASTEXITCODE)"; $pior = [Math]::Max($pior, $LASTEXITCODE) }
}

# -------------------------------------------------------------------------------------------------
# As tarefas
# -------------------------------------------------------------------------------------------------
# NAO EXISTE /SC YEARLY. "Uma vez por ano" e /SC MONTHLY /M OCT /D 1; a mensal e o mesmo sem o /M.
# A alternativa /SC MONTHLY /MO 12 /SD ... foi MEDIDA em 20/09/2026 e agenda para dezembro.
#
# OS ARGUMENTOS VAO NUMA LISTA: com linha de comando escrita a mao, a continuacao com crase nao
# sobreviveu a viagem pelo _remoto.ps1 ("Mandatory option 'sc' is missing").
$tarefas = @(
    @('/Create', '/TN', $NomeTarefaAnual,
      '/TR', "powershell.exe -NoProfile -ExecutionPolicy Bypass -File $anual",
      '/SC', 'MONTHLY', '/M', $MesAnual, '/D', "$DiaAnual",
      '/ST', $HoraAnual, '/RU', 'SYSTEM', '/RL', 'HIGHEST', '/F'),
    @('/Create', '/TN', $NomeTarefaMensal,
      '/TR', "powershell.exe -NoProfile -ExecutionPolicy Bypass -File $mensal",
      '/SC', 'MONTHLY', '/D', "$DiaMensal",
      '/ST', $HoraMensal, '/RU', 'SYSTEM', '/RL', 'HIGHEST', '/F')
)

foreach ($argumentos in $tarefas) {
    & schtasks.exe @argumentos | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Output "schtasks falhou em $($argumentos[2]) (codigo $LASTEXITCODE)"; $pior = [Math]::Max($pior, $LASTEXITCODE) }
    else { Write-Output "tarefa $($argumentos[2]) registrada" }
}

# A TAREFA ANTIGA SAI DEPOIS QUE AS NOVAS ENTRAM, e so entao: deixar as duas faria a PAM carregar duas
# vezes na mesma madrugada, e a trava recusaria a segunda com um "falhou" que nao e falha.
if ($pior -eq 0 -and $TarefaAntiga) {
    & schtasks.exe /Query /TN $TarefaAntiga *> $null
    if ($LASTEXITCODE -eq 0) {
        & schtasks.exe /Delete /TN $TarefaAntiga /F | Out-Null
        Write-Output "tarefa antiga $TarefaAntiga removida"
    }
}

# OS SCRIPTS DE ROTINA DA VERSAO ANTERIOR moravam na pasta da carga. Se ainda estiverem la, sao
# restos: as tarefas agora apontam para a pasta das rotinas.
foreach ($resto in 'rodar-fontes-publicas.ps1', 'rodar-pam.ps1', 'rodar-precos.ps1') {
    Remove-Item (Join-Path $DestinoDaCarga $resto) -Force -ErrorAction SilentlyContinue
}

# -------------------------------------------------------------------------------------------------
# A primeira carga
# -------------------------------------------------------------------------------------------------
# CADA ROTINA PELAS SUAS TABELAS: a mensal pelos precos, custos e credito; a anual pela PAM e pela
# estrutura. Basta uma vazia para disparar - foi o caso das usinas em 20/09/2026, que ficaram vazias
# enquanto o resto da estrutura carregou.
#
# AS DUAS PODEM RODAR JUNTAS: a trava da carga e por fluxo (TravaDeFluxo), e as duas rotinas nao dividem
# fluxo nenhum. O disparo e assincrono - quem registrou nao espera a carga terminar.
#
# NAO MUDA O CODIGO DAS ROTINAS: elas estao registradas e rodam no calendario de qualquer jeito. Tabela
# que ainda nao existe (migracao nao aplicada) cai no catch e sai como aviso.
if ($pior -eq 0 -and -not $NaoDispararPrimeiraCarga) {
    $primeiras = @(
        @{ Tarefa = $NomeTarefaMensal
           Tabelas = @('CotacaoDeProduto', 'CotacaoDoDolar', 'CustoDeProducao', 'CreditoRuralDeInvestimento') },
        @{ Tarefa = $NomeTarefaAnual
           Tabelas = @('ProducaoAgricolaNoMunicipio', 'FrotaDeTratoresNoMunicipio', 'EstabelecimentosPorAreaNoMunicipio',
                       'RebanhoNoMunicipio', 'AreaTerritorialDoMunicipio', 'UsinaDeEtanol') }
    )

    try {
        $banco = New-Object System.Data.SqlClient.SqlConnection $Conexao
        $banco.Open()
        try {
            foreach ($rotina in $primeiras) {
                $vazias = @()
                foreach ($tabela in $rotina.Tabelas) {
                    $consulta = $banco.CreateCommand()
                    $consulta.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM organizacao.[$tabela]) THEN 1 ELSE 0 END"
                    if ([int]$consulta.ExecuteScalar() -eq 0) { $vazias += $tabela }
                }

                if ($vazias.Count -gt 0) {
                    & schtasks.exe /Run /TN $rotina.Tarefa | Out-Null
                    Write-Output ("primeira carga disparada: {0} (vazias: {1})" -f $rotina.Tarefa, ($vazias -join ', '))
                } else {
                    Write-Output ("primeira carga: {0} ja tem dado, segue o calendario" -f $rotina.Tarefa)
                }
            }
        } finally { $banco.Dispose() }
    } catch {
        Write-Output ("primeira carga nao conferida: " + $_.Exception.Message)
    }
}

Write-Output "codigo das rotinas: $pior"
exit $pior
