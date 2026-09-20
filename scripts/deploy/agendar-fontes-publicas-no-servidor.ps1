<#
  agendar-fontes-publicas-no-servidor.ps1 - leva a carga das FONTES PUBLICAS para o servidor e deixa
  uma ROTINA ANUAL agendada la, para que o CRM se atualize sozinho (issues 64 e 65; regra R-5 do
  doc 46).

      Set-Location C:\projetos\tracbel-crm
      powershell -ExecutionPolicy Bypass -File .\scripts\deploy\agendar-fontes-publicas-no-servidor.ps1

  =================================================================================================
  O QUE A ROTINA ATUALIZA

    --somente-pam         a Producao Agricola Municipal: area plantada, colhida, quantidade e valor,
                          por cultura e municipio, em tres anos (issue 64);
    --somente-estrutura   o que ja existe para mecanizar (issue 65): tratores por potencia e
                          estabelecimentos por tamanho (Censo Agropecuario), rebanho bovino (PPM),
                          area territorial (Censo Demografico) e as usinas de etanol (ANP).

  UMA TAREFA SO PARA AS DUAS. Elas saem na mesma epoca do ano, vem das mesmas agencias publicas e
  falham pelos mesmos motivos; duas tarefas seriam dois lugares para olhar quando algo der errado.

  =================================================================================================
  POR QUE SO ESTAS, E NAO O TERRITORIO INTEIRO

  A carga do territorio tem quatro etapas, e TRES dependem de duas planilhas do comercial, que trazem
  nome de funcionario por municipio e ficam fora do repositorio: elas mudam quando o comercial as
  reenvia, e a carga delas continua saindo da estacao (carregar-territorio-no-servidor.ps1).

  As fontes desta rotina nao usam planilha nenhuma: sao APIs e arquivos publicos, e o servidor alcanca
  todos - MEDIDO em 20/09/2026, quando a primeira rodada leu 163.965 linhas do SIDRA de dentro dele.
  E exatamente o que a regra R-5 pede: o servidor busca os dados, ninguem precisa lembrar de rodar
  nada.

  Levar as planilhas ate o servidor so para atualizar o IBGE seria levar dado pessoal onde ele nao
  precisa estar.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM
    0. confere que o banco do servidor responde e que o CRM ja tem o catalogo de municipios do IBGE
       (as duas cargas se penduram nele; sem catalogo, nao ha onde gravar);
    1. publica a carga (self-contained, como a API) em C:\aplicacoes\tracbel-crm-carga no servidor;
    2. grava o `rodar-fontes-publicas.ps1` - a conexao LOCAL integrada, as chamadas e o log -, so
       para o SYSTEM;
    3. registra a tarefa agendada anual `TracbelCrmFontesPublicas`, que chama esse script, e remove a
       `TracbelCrmPam` que a versao anterior deste script instalava;
    4. RODA a tarefa uma vez, espera e mostra o retrato do banco depois.

  A TRAVA NAO E DESTE SCRIPT: ela e do proprio programa (sp_getapplock no banco, uma por fluxo, tomada
  ANTES da leitura). E por isso que a rotina anual e uma carga manual da estacao nunca escrevem ao
  mesmo tempo, mesmo sendo maquinas diferentes.

  Reexecutar e seguro: a publicacao substitui os arquivos, a tarefa e recriada e as cargas sao
  idempotentes - a segunda rodada nao grava nada.
#>

[CmdletBinding()]
param(
    [string] $Servidor   = '10.150.4.249',
    [string] $Banco      = 'TracbelCrm',
    [string] $Destino    = 'C:\aplicacoes\tracbel-crm-carga',
    [string] $NomeTarefa = 'TracbelCrmFontesPublicas',

    # A tarefa que a versao anterior deste script instalava, e que agora e substituida.
    [string] $TarefaAntiga = 'TracbelCrmPam',

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
        'producao agricola (municipio)' = 'SELECT COUNT(*) FROM organizacao.ProducaoAgricolaNoMunicipio'
        'producao agricola (estado)'    = 'SELECT COUNT(*) FROM organizacao.ProducaoAgricolaNoEstado'
        'anos distintos da PAM'         = 'SELECT COUNT(DISTINCT Ano) FROM organizacao.ProducaoAgricolaNoMunicipio'
        'frota de tratores'             = 'SELECT COUNT(*) FROM organizacao.FrotaDeTratoresNoMunicipio'
        'estabelecimentos por area'     = 'SELECT COUNT(*) FROM organizacao.EstabelecimentosPorAreaNoMunicipio'
        'rebanho'                       = 'SELECT COUNT(*) FROM organizacao.RebanhoNoMunicipio'
        'area territorial'              = 'SELECT COUNT(*) FROM organizacao.AreaTerritorialDoMunicipio'
        'usinas de etanol'              = 'SELECT COUNT(*) FROM organizacao.UsinaDeEtanol'
    }
    Write-Host "   $rotulo"
    $resultado = @{}
    foreach ($k in $medidas.Keys) {
        $valor = [int](Escalar $medidas[$k])
        $resultado[$k] = $valor
        Write-Host ("     {0,-32} {1,8:N0}" -f $k, $valor)
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

# AS DUAS CARGAS RODAM MESMO QUE A PRIMEIRA FALHE, e o codigo de saida e o PIOR das duas. Parar na
# primeira faria uma indisponibilidade do SIDRA levar junto a leitura da ANP, que nao tem nada a ver.
# O log guarda as duas, uma embaixo da outra, e a tarefa so diz "0" quando as duas deram certo.
#
# UMA CODIFICACAO SO NO LOG, E UTF-8. A primeira versao misturava `Tee-Object -FilePath` (que no
# PowerShell 5.1 grava UTF-16) com `Add-Content` (que grava ANSI) NO MESMO ARQUIVO: o resultado, lido
# em 20/09/2026, era ilegivel — "l i n h a s   m a n t i d a s". O log so serve se puder ser lido no
# dia em que algo der errado, e foi exatamente o que aconteceu.
$rotina = @"
# rodar-fontes-publicas.ps1 - gerado por agendar-fontes-publicas-no-servidor.ps1.
# Nao edite aqui: edite o script de origem, no repositorio.
`$ErrorActionPreference = 'Continue'
`$env:ConnectionStrings__Crm = '$conexaoLocal'
`$pasta = '$Destino\logs'
New-Item -ItemType Directory -Force -Path `$pasta | Out-Null
`$log = Join-Path `$pasta ('fontes-publicas-' + (Get-Date -Format 'yyyyMMdd-HHmm') + '.log')

function Anotar(`$texto) { Out-File -FilePath `$log -Append -Encoding utf8 -InputObject `$texto }

`$pior = 0
foreach (`$modo in '--somente-pam', '--somente-estrutura') {
    Anotar ''
    Anotar ('=== ' + `$modo + ' em ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
    & '$Destino\Tracbel.Crm.Carga.exe' `$modo *>&1 | ForEach-Object { Anotar `$_ }
    `$codigo = `$LASTEXITCODE
    Anotar ('codigo de saida de ' + `$modo + ': ' + `$codigo)
    if (`$codigo -gt `$pior) { `$pior = `$codigo }
}
Anotar ('codigo de saida: ' + `$pior)
Get-ChildItem `$pasta -Filter '*.log' | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddYears(-3) } | Remove-Item -Force
exit `$pior
"@
# ASCII PURO, pelo mesmo motivo do _remoto.ps1: o PowerShell 5.1 do servidor le .ps1 como ANSI.
Set-Content -Path (Join-Path $destinoPorSmb 'rodar-fontes-publicas.ps1') -Value $rotina -Encoding ASCII

# OS GRUPOS VAO POR SID, E NAO POR NOME. Num Windows em portugues, 'SYSTEM' se chama 'SISTEMA' e
# 'Administrators' se chama 'Administradores': o icacls com o nome em ingles falha, e falha DEPOIS de
# ter tirado a heranca - deixando o arquivo sem dono nenhum. O SID e o mesmo em qualquer idioma.
$saida = Invoke-NoServidor -Nome 'crm-fontes-permissao' -Script @"
`$a = '$Destino\rodar-fontes-publicas.ps1'
icacls `$a /inheritance:r /grant '*S-1-5-18:(RX)' '*S-1-5-32-544:(RX)'
Write-Output ('codigo do icacls: ' + `$LASTEXITCODE)
"@
Write-Host $saida
if ($saida -notmatch 'codigo do icacls: 0') {
    throw "Nao consegui restringir o acesso a $Destino\rodar-fontes-publicas.ps1. A saida esta acima."
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
$saida = Invoke-NoServidor -Nome 'crm-fontes-tarefa' -TimeoutSegundos 300 -Script @"
New-Item -ItemType Directory -Force -Path '$Destino\logs' | Out-Null
`$argumentos = @(
    '/Create', '/TN', '$NomeTarefa',
    '/TR', 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Destino\rodar-fontes-publicas.ps1',
    '/SC', 'MONTHLY', '/M', '$Mes', '/D', '$DiaDoMes',
    '/ST', '$Hora', '/RU', 'SYSTEM', '/RL', 'HIGHEST', '/F')
& schtasks.exe @argumentos
Write-Output ('codigo do schtasks: ' + `$LASTEXITCODE)

# A TAREFA ANTIGA SAI DEPOIS QUE A NOVA ENTRA, e so entao: deixar as duas agendadas faria a PAM
# carregar duas vezes na mesma madrugada - a trava recusaria a segunda, e o log diria "falhou" sem
# que nada estivesse errado.
if (`$LASTEXITCODE -eq 0 -and '$TarefaAntiga' -ne '') {
    & schtasks.exe /Query /TN '$TarefaAntiga' *> `$null
    if (`$LASTEXITCODE -eq 0) {
        & schtasks.exe /Delete /TN '$TarefaAntiga' /F
        Write-Output ('tarefa antiga $TarefaAntiga removida: ' + `$LASTEXITCODE)
        Remove-Item '$Destino\rodar-pam.ps1' -Force -ErrorAction SilentlyContinue
    } else {
        Write-Output 'tarefa antiga $TarefaAntiga nao existia'
    }
}

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

# UMA HORA DE ESPERA: a PAM sao 3 anos x 9 lotes x 2 recortes de consulta ao SIDRA, com ~5 MB cada;
# a estrutura sao mais quatro consultas ao SIDRA e um ZIP da ANP, que juntos levam menos de um minuto.
$saida = Invoke-NoServidor -Nome 'crm-fontes-primeira' -TimeoutSegundos 3600 -Script @"
schtasks /Run /TN '$NomeTarefa' | Out-Null
`$limite = (Get-Date).AddMinutes(55)
while ((Get-Date) -lt `$limite) {
    Start-Sleep -Seconds 15
    `$linha = schtasks /Query /TN '$NomeTarefa' /FO LIST | Select-String 'Status:|Estado:'
    if (`$linha -and (`$linha -join ' ') -notmatch 'Running|Em execu') { break }
}
`$log = Get-ChildItem '$Destino\logs\fontes-publicas-*.log' | Sort-Object LastWriteTime | Select-Object -Last 1
if (`$log) {
    Write-Output ('log: ' + `$log.FullName)
    Get-Content `$log.FullName -Tail 40
} else {
    Write-Output 'NENHUM LOG FOI ESCRITO: a tarefa pode nao ter chegado a rodar.'
}
"@
Write-Host $saida

$depois = Retrato 'estado das fontes publicas no servidor, depois:'

# CADA FONTE E CONFERIDA SEPARADAMENTE. Olhar só uma deixaria passar o caso em que a PAM carregou e a
# ANP nao — que e exatamente o que a segunda carga acrescenta de risco.
$vazias = $depois.Keys | Where-Object { $_ -notlike 'anos*' -and $depois[$_] -eq 0 }
if ($vazias) {
    throw "A tarefa rodou e estas tabelas continuam vazias: $($vazias -join ', '). O log do servidor esta acima."
}

Ok 'o servidor atualiza a PAM e a estrutura agropecuaria sozinho, uma vez por ano'
Write-Host ''
