<#
  agendar-pam-no-servidor.ps1 - leva a carga da PAM para o servidor e deixa uma ROTINA ANUAL agendada
  la, para que o CRM atualize a producao agricola do IBGE sozinho (issue 64; regra R-5 do doc 46).

      Set-Location C:\projetos\tracbel-crm
      powershell -ExecutionPolicy Bypass -File .\scripts\deploy\agendar-pam-no-servidor.ps1

  =================================================================================================
  POR QUE ESTA ROTINA EXISTE, E POR QUE SO ELA

  A carga do territorio tem quatro etapas. TRES delas dependem de duas planilhas do comercial, que
  trazem nome de funcionario por municipio e ficam fora do repositorio: elas mudam quando o comercial
  as reenvia, e a carga delas continua saindo da estacao (carregar-territorio-no-servidor.ps1).

  A QUARTA - a Producao Agricola Municipal do IBGE - nao usa planilha nenhuma, muda UMA VEZ POR ANO e
  vem de uma API publica. E a unica que o servidor consegue rodar sozinho, e e exatamente o que a
  regra R-5 pede: o servidor busca os dados, ninguem precisa lembrar de rodar nada.

  Levar as planilhas ate o servidor so para atualizar o IBGE seria levar dado pessoal onde ele nao
  precisa estar. Por isso o modo `--somente-pam`.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM
    0. confere que o banco do servidor responde e que o CRM ja tem o catalogo de municipios do IBGE
       (a PAM se pendura nele; sem catalogo, nao ha onde gravar);
    1. publica a carga (self-contained, como a API) em C:\aplicacoes\tracbel-crm-carga no servidor;
    2. grava o `rodar-pam.ps1` - a conexao LOCAL integrada, a chamada e o log -, so para o SYSTEM;
    3. registra a tarefa agendada anual `TracbelCrmPam`, que chama esse script;
    4. RODA a tarefa uma vez, espera e mostra o retrato do banco depois.

  A TRAVA NAO E DESTE SCRIPT: ela e do proprio programa (sp_getapplock no banco, pelo tempo da
  transacao). E por isso que a rotina anual e uma carga manual da estacao nunca escrevem ao mesmo
  tempo, mesmo sendo maquinas diferentes.

  Reexecutar e seguro: a publicacao substitui os arquivos, a tarefa e recriada e a carga e
  idempotente - a segunda rodada nao grava nada.
#>

[CmdletBinding()]
param(
    [string] $Servidor   = '10.150.4.249',
    [string] $Banco      = 'TracbelCrm',
    [string] $Destino    = 'C:\aplicacoes\tracbel-crm-carga',
    [string] $NomeTarefa = 'TracbelCrmPam',

    # QUANDO A PAM SAI. O IBGE divulga a Producao Agricola Municipal no segundo semestre; 1 de outubro
    # pega a divulgacao do ano com folga, e a carga traz tres anos - entao uma rodada perdida se
    # conserta sozinha na seguinte.
    [string] $Mes        = 'OCT',
    [int]    $DiaDoMes   = 1,
    [string] $Hora       = '03:00',

    # Rodar a tarefa agora, ao fim da instalacao, para provar que ela funciona.
    [switch] $NaoRodarAgora
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $PSScriptRoot '_remoto.ps1')

$conexao = "Server=$Servidor,1433;Database=$Banco;Integrated Security=True;TrustServerCertificate=True"

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t)    { Write-Host "   OK  $t" -ForegroundColor Green }

function Escalar([string] $sql, [int] $timeout = 60) {
    $c = New-Object System.Data.SqlClient.SqlConnection $conexao
    $c.Open()
    try {
        $cmd = $c.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $sql
        return $cmd.ExecuteScalar()
    } finally { $c.Dispose() }
}

function Retrato([string] $rotulo) {
    $medidas = [ordered]@{
        'linhas de producao agricola' = 'SELECT COUNT(*) FROM organizacao.ProducaoAgricolaNoMunicipio'
        'linhas do total do estado'   = 'SELECT COUNT(*) FROM organizacao.ProducaoAgricolaNoEstado'
        'anos distintos'              = 'SELECT COUNT(DISTINCT Ano) FROM organizacao.ProducaoAgricolaNoMunicipio'
    }
    Write-Host "   $rotulo"
    $resultado = @{}
    foreach ($k in $medidas.Keys) {
        $valor = [int](Escalar $medidas[$k])
        $resultado[$k] = $valor
        Write-Host ("     {0,-30} {1,8:N0}" -f $k, $valor)
    }
    return $resultado
}

# -------------------------------------------------------------------------------------------------
Passo '0. Banco do servidor e catalogo de municipios'
# -------------------------------------------------------------------------------------------------
# PASTA SEM ESPACO. O /TR do schtasks guarda uma LINHA DE COMANDO, e a linha de comando de la chama
# `powershell -File <caminho>`: um espaco no caminho exigiria mais um nivel de aspas dentro de uma
# string que ja atravessou tres. Recusar aqui e melhor do que descobrir em outubro do ano que vem que
# a tarefa nunca rodou.
if ($Destino -match '\s') {
    throw "A pasta de destino nao pode ter espaco: '$Destino'. Passe outra em -Destino."
}

$comIbge = [int](Escalar 'SELECT COUNT(*) FROM organizacao.Municipio WHERE CodigoIbge IS NOT NULL')
if ($comIbge -eq 0) {
    throw "O banco de $Servidor nao tem nenhum municipio com codigo do IBGE. A PAM se pendura no catalogo: rode antes a carga do territorio inteira (carregar-territorio-no-servidor.ps1). Nada foi instalado."
}
Ok "banco responde; $comIbge municipios com codigo do IBGE"

Retrato 'estado da producao agricola no servidor, antes:' | Out-Null

# -------------------------------------------------------------------------------------------------
Passo '1. Publicar a carga'
# -------------------------------------------------------------------------------------------------
# SELF-CONTAINED, pelo mesmo motivo da API (instalar-no-servidor.ps1): o servidor tem .NET 6 e 8, e o
# projeto e .NET 10. Levar o runtime dentro do pacote evita instalar runtime novo numa maquina que ja
# hospeda outra aplicacao em producao.
$pacote = Join-Path $env:TEMP "tracbel-crm-carga-$(Get-Date -Format yyyyMMddHHmm)"
Push-Location $raiz
try {
    & dotnet publish src\Tracbel.Crm.Carga -c Release -r win-x64 --self-contained true -o $pacote
    if ($LASTEXITCODE -ne 0) { throw "A publicacao da carga falhou (codigo $LASTEXITCODE). Nada foi copiado." }
} finally { Pop-Location }

$destinoPorSmb = "\\$Servidor\$($Destino -replace ':', '$')"
New-Item -ItemType Directory -Force -Path $destinoPorSmb | Out-Null
Copy-Item (Join-Path $pacote '*') $destinoPorSmb -Recurse -Force
Remove-Item $pacote -Recurse -Force
Ok "carga publicada em $Destino"

# -------------------------------------------------------------------------------------------------
Passo "2. O script da rotina, com a conexao do banco"
# -------------------------------------------------------------------------------------------------
# A TAREFA CHAMA UM .PS1, E NAO UM COMANDO ESCRITO NA LINHA DO SCHTASKS. O /TR passa por tres camadas
# de aspas (PowerShell daqui, schtasks, PowerShell de la), e cada uma come um nivel: um comando longo
# ali dentro vira uma caca a aspas que quebra em silencio meses depois. Um arquivo nao tem esse
# problema, e ainda pode ser lido por quem for diagnosticar.
#
# A CONEXAO VAI NA VARIAVEL DE AMBIENTE, e nao num appsettings. A carga le `appsettings.json` ao lado
# do executavel e as variaveis de ambiente - e so isso; ela NAO tem appsettings por ambiente. Escrever
# um `appsettings.Production.json` ali seria um arquivo que ninguem le, e a carga pararia dizendo que
# nao achou a cadeia de conexao. Escrever por cima do `appsettings.json` publicado apagaria o que
# viesse nele.
#
# A CONEXAO E INTEGRADA: a tarefa roda como SYSTEM, que no dominio e a conta da MAQUINA, e o banco
# esta na mesma maquina. Nenhuma senha em arquivo, nenhum segredo neste repositorio.
#
# O LOG FICA NO SERVIDOR, um arquivo por rodada. Sem ele, uma falha da rotina anual seria invisivel
# ate alguem reparar que o mapa parou num ano antigo - que e justamente o silencio que este projeto
# persegue. Os logs com mais de tres anos sao apagados pela propria rotina: tres anos e o que a carga
# guarda de PAM.
$conexaoLocal = "Server=localhost,1433;Database=$Banco;Integrated Security=True;TrustServerCertificate=True"

$rotina = @"
# rodar-pam.ps1 - gerado por agendar-pam-no-servidor.ps1. Nao edite aqui: edite o script de origem.
`$ErrorActionPreference = 'Continue'
`$env:ConnectionStrings__Crm = '$conexaoLocal'
`$pasta = '$Destino\logs'
New-Item -ItemType Directory -Force -Path `$pasta | Out-Null
`$log = Join-Path `$pasta ('pam-' + (Get-Date -Format 'yyyyMMdd-HHmm') + '.log')
& '$Destino\Tracbel.Crm.Carga.exe' --somente-pam *>&1 | Tee-Object -FilePath `$log
`$codigo = `$LASTEXITCODE
Add-Content `$log ('codigo de saida: ' + `$codigo)
Get-ChildItem `$pasta -Filter 'pam-*.log' | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddYears(-3) } | Remove-Item -Force
exit `$codigo
"@
# ASCII PURO, pelo mesmo motivo do _remoto.ps1: o PowerShell 5.1 do servidor le .ps1 como ANSI.
Set-Content -Path (Join-Path $destinoPorSmb 'rodar-pam.ps1') -Value $rotina -Encoding ASCII

# OS GRUPOS VAO POR SID, E NAO POR NOME. Num Windows em portugues, 'SYSTEM' se chama 'SISTEMA' e
# 'Administrators' se chama 'Administradores': o icacls com o nome em ingles falha, e falha DEPOIS de
# ter tirado a heranca - deixando o arquivo sem dono nenhum. O SID e o mesmo em qualquer idioma.
$saida = Invoke-NoServidor -Nome 'crm-pam-permissao' -Script @"
`$a = '$Destino\rodar-pam.ps1'
icacls `$a /inheritance:r /grant '*S-1-5-18:(RX)' '*S-1-5-32-544:(RX)'
Write-Output ('codigo do icacls: ' + `$LASTEXITCODE)
"@
Write-Host $saida
if ($saida -notmatch 'codigo do icacls: 0') {
    throw "Nao consegui restringir o acesso a $Destino\rodar-pam.ps1. A saida esta acima."
}
Ok 'rotina gravada, com acesso so para o SYSTEM e os administradores'

# -------------------------------------------------------------------------------------------------
Passo "3. Tarefa anual $NomeTarefa"
# -------------------------------------------------------------------------------------------------

# NAO EXISTE /SC YEARLY NO SCHTASKS. Os tipos sao MINUTE, HOURLY, DAILY, WEEKLY, MONTHLY, ONCE e os
# ON*. "Uma vez por ano" se escreve como MENSAL RESTRITO A UM MES: /SC MONTHLY /M OCT /D 1.
#
# A alternativa aparentemente equivalente - /SC MONTHLY /MO 12 /SD 01/10/2026, "de 12 em 12 meses a
# partir desta data" - foi MEDIDA em 20/09/2026 e agenda para DEZEMBRO: o /SD e lido noutra ordem de
# dia e mes. Nao use.
#
# E OS ARGUMENTOS VAO NUMA LISTA, e nao numa linha de comando escrita a mao. A primeira versao usava
# continuacao de linha com crase e aspas escapadas dentro do /TR, e falhou no servidor com "Mandatory
# option 'sc' is missing": a continuacao nao sobreviveu a viagem pelo _remoto.ps1, e so a primeira
# linha rodou. Com uma lista, o PowerShell entrega cada item como um argumento e cuida das aspas
# sozinho - nao ha nivel de aspas para contar.
$saida = Invoke-NoServidor -Nome 'crm-pam-tarefa' -TimeoutSegundos 300 -Script @"
New-Item -ItemType Directory -Force -Path '$Destino\logs' | Out-Null
`$argumentos = @(
    '/Create', '/TN', '$NomeTarefa',
    '/TR', 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Destino\rodar-pam.ps1',
    '/SC', 'MONTHLY', '/M', '$Mes', '/D', '$DiaDoMes',
    '/ST', '$Hora', '/RU', 'SYSTEM', '/RL', 'HIGHEST', '/F')
& schtasks.exe @argumentos
Write-Output ('codigo do schtasks: ' + `$LASTEXITCODE)
& schtasks.exe /Query /TN '$NomeTarefa' /FO LIST /V
"@
Write-Host $saida
if ($saida -notmatch 'codigo do schtasks: 0') {
    throw "Nao consegui registrar a tarefa $NomeTarefa no servidor. A saida esta acima."
}
Ok "tarefa $NomeTarefa registrada para $DiaDoMes/$Mes as $Hora, todo ano, como SYSTEM"

# -------------------------------------------------------------------------------------------------
Passo '4. Rodar uma vez e conferir'
# -------------------------------------------------------------------------------------------------
if ($NaoRodarAgora) {
    Write-Host '   !   primeira execucao pulada a pedido (-NaoRodarAgora)' -ForegroundColor Yellow
    return
}

# UMA HORA DE ESPERA: sao 3 anos x 9 lotes x 2 recortes de consulta ao SIDRA, com ~5 MB cada.
$saida = Invoke-NoServidor -Nome 'crm-pam-primeira' -TimeoutSegundos 3600 -Script @"
schtasks /Run /TN '$NomeTarefa' | Out-Null
`$limite = (Get-Date).AddMinutes(55)
while ((Get-Date) -lt `$limite) {
    Start-Sleep -Seconds 15
    `$linha = schtasks /Query /TN '$NomeTarefa' /FO LIST | Select-String 'Status:|Estado:'
    if (`$linha -and (`$linha -join ' ') -notmatch 'Running|Em execu') { break }
}
`$log = Get-ChildItem '$Destino\logs\pam-*.log' | Sort-Object LastWriteTime | Select-Object -Last 1
Write-Output ('log: ' + `$log.FullName)
Get-Content `$log.FullName -Tail 20
"@
Write-Host $saida

$depois = Retrato 'estado da producao agricola no servidor, depois:'
if ($depois['linhas de producao agricola'] -eq 0) {
    throw "A tarefa rodou e a tabela continua vazia. O log do servidor esta acima."
}
Ok 'o servidor atualiza a producao agricola do IBGE sozinho, uma vez por ano'
Write-Host ''
