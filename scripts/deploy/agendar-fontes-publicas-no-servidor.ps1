<#
  agendar-fontes-publicas-no-servidor.ps1 - leva a carga das FONTES PUBLICAS para o servidor e deixa
  DUAS ROTINAS agendadas la, para que o CRM se atualize sozinho (issues 64, 65 e 66; regra R-5 do
  doc 46).

      Set-Location C:\projetos\tracbel-crm
      powershell -ExecutionPolicy Bypass -File .\scripts\deploy\agendar-fontes-publicas-no-servidor.ps1

  =================================================================================================
  O QUE AS ROTINAS ATUALIZAM

  TracbelCrmFontesPublicas - ANUAL, 1 de outubro:
    --somente-pam         a Producao Agricola Municipal: area plantada, colhida, quantidade e valor,
                          por cultura e municipio, em tres anos (issue 64);
    --somente-estrutura   o que ja existe para mecanizar (issue 65): tratores por potencia e
                          estabelecimentos por tamanho (Censo Agropecuario), rebanho bovino (PPM),
                          area territorial (Censo Demografico) e as usinas de etanol (ANP).

  TracbelCrmPrecos - MENSAL, dia 20:
    --somente-precos      o preco recebido pelo produtor em SP (CONAB), o preco do kg de ATR da cana
                          (Socicana) e o dolar PTAX mensal (Banco Central) (issue 66);
    --somente-custos      o custo de producao das culturas em SP, das series historicas da CONAB
                          (issue 67);
    --somente-credito     o credito rural de investimento do SICOR, por municipio e mes (issue 68).

  POR QUE DUAS TAREFAS, E NAO UMA. As fontes anuais saem uma vez por ano e as de preco todo mes: uma
  tarefa mensal para tudo releria o SIDRA doze vezes por ano para achar a mesma PAM; uma anual para
  tudo deixaria o preco onze meses parado. E a CONAB so publica os ultimos 12 meses - uma rodada
  mensal perdida se conserta na seguinte, mas um ano sem rodar perde meses para sempre.

  POR QUE O DIA 20. A CONAB fecha o mes anterior na primeira quinzena, e o Banco Central publica a
  media mensal do PTAX no primeiro dia util. O dia 20 pega os dois com folga.

  =================================================================================================
  POR QUE SO ESTAS, E NAO O TERRITORIO INTEIRO

  A carga do territorio tem quatro etapas, e TRES dependem de duas planilhas do comercial, que trazem
  nome de funcionario por municipio e ficam fora do repositorio: elas mudam quando o comercial as
  reenvia, e a carga delas continua saindo da estacao (carregar-territorio-no-servidor.ps1).

  As fontes destas rotinas nao usam planilha nenhuma: sao APIs e arquivos publicos, e o servidor
  alcanca todos - MEDIDO em 20/09/2026, quando a primeira rodada leu 163.965 linhas do SIDRA de dentro
  dele. E exatamente o que a regra R-5 pede: o servidor busca os dados, ninguem precisa lembrar de
  rodar nada.

  Levar as planilhas ate o servidor so para atualizar o IBGE seria levar dado pessoal onde ele nao
  precisa estar.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM
    0. confere que o banco do servidor responde e que o CRM ja tem o catalogo de municipios do IBGE
       (as cargas anuais se penduram nele; sem catalogo, nao ha onde gravar);
    1. publica a carga (self-contained, como a API) em C:\aplicacoes\tracbel-crm-carga no servidor;
    2. leva o `registrar-rotinas.ps1` ao servidor e o roda: ele grava os dois scripts de rotina (conexao
       LOCAL integrada, chamadas e log) em C:\aplicacoes\tracbel-crm-rotinas, so para o SYSTEM e os
       administradores, e registra as tarefas `TracbelCrmFontesPublicas` (anual) e `TracbelCrmPrecos`
       (mensal), removendo a `TracbelCrmPam` da primeira versao;
    3. (o mesmo arquivo roda a cada publicacao automatica: este script so e preciso ANTES de o agente
       de publicacao estar instalado);
    4. RODA as duas uma vez, espera e mostra o retrato do banco depois.

  A TRAVA NAO E DESTE SCRIPT: ela e do proprio programa (sp_getapplock no banco, uma por fluxo, tomada
  ANTES da leitura). E por isso que uma rotina agendada e uma carga manual da estacao nunca escrevem
  ao mesmo tempo, mesmo sendo maquinas diferentes.

  Reexecutar e seguro: a publicacao substitui os arquivos, as tarefas sao recriadas e as cargas sao
  idempotentes - a segunda rodada nao grava nada.
#>

[CmdletBinding()]
param(
    [string] $Servidor   = '10.150.4.249',
    [string] $Banco      = 'TracbelCrm',
    [string] $Destino    = 'C:\aplicacoes\tracbel-crm-carga',

    # Os scripts das rotinas e os logs. FORA da pasta da carga: a publicacao automatica apaga a
    # pasta da carga antes de copiar a versao nova (ver registrar-rotinas.ps1).
    [string] $PastaDasRotinas = 'C:\aplicacoes\tracbel-crm-rotinas',
    [string] $NomeTarefa = 'TracbelCrmFontesPublicas',

    # A tarefa que a primeira versao deste script instalava, e que agora e substituida.
    [string] $TarefaAntiga = 'TracbelCrmPam',

    # QUANDO A PAM SAI. O IBGE divulga a Producao Agricola Municipal no segundo semestre; 1 de outubro
    # pega a divulgacao do ano com folga, e a carga traz tres anos - entao uma rodada perdida se
    # conserta sozinha na seguinte.
    [string] $Mes        = 'OCT',
    [int]    $DiaDoMes   = 1,
    [string] $Hora       = '03:00',

    # A ROTINA MENSAL DOS PRECOS (issue 66). Ver "POR QUE O DIA 20" no cabecalho.
    [string] $NomeTarefaMensal = 'TracbelCrmPrecos',
    [int]    $DiaDaRotinaMensal = 20,
    [string] $HoraMensal = '04:00',

    # Rodar as tarefas agora, ao fim da instalacao, para provar que elas funcionam.
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
        'cotacoes de produtos'          = 'SELECT COUNT(*) FROM organizacao.CotacaoDeProduto'
        'meses de dolar PTAX'           = 'SELECT COUNT(*) FROM organizacao.CotacaoDoDolar'
        'custos de producao'            = 'SELECT COUNT(*) FROM organizacao.CustoDeProducao'
        'credito rural (SICOR)'         = 'SELECT COUNT(*) FROM organizacao.CreditoRuralDeInvestimento'
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

# AS TABELAS DE PRECO SO EXISTEM DEPOIS DA MIGRACAO DA ISSUE 66, que a publicacao automatica aplica.
# Sem elas o retrato quebraria com "nome de objeto invalido" - e a mensagem certa e outra.
$temPrecos = [int](Escalar "SELECT COUNT(*) FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = 'organizacao' AND t.name IN ('CotacaoDeProduto', 'CustoDeProducao', 'CreditoRuralDeInvestimento')")
if ($temPrecos -lt 3) {
    throw "O banco de $Servidor ainda nao tem as tabelas de precos e custos: as migracoes PrecosDeMercado, CustosDeProducao e CreditoRuralDoSicor (issues 66 a 68) nao foram aplicadas. Espere a publicacao automatica da main levar a versao nova e rode de novo. Nada foi instalado."
}

Retrato 'estado das fontes publicas no servidor, antes:' | Out-Null

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
Passo "2 e 3. As rotinas e as tarefas agendadas (registrar-rotinas.ps1)"
# -------------------------------------------------------------------------------------------------
# A DEFINICAO DAS ROTINAS MORA NUM LUGAR SO: scripts/deploy/registrar-rotinas.ps1. Este script o leva
# ao servidor e o roda; a publicacao automatica roda o MESMO arquivo, que viaja dentro do pacote do
# CI, a cada deploy. Duas copias da mesma logica divergiriam na primeira fonte nova.
#
# A CONEXAO E INTEGRADA: as tarefas rodam como SYSTEM, que no dominio e a conta da MAQUINA, e o banco
# esta na mesma maquina. Nenhuma senha em arquivo, nenhum segredo neste repositorio.
$conexaoLocal = "Server=localhost,1433;Database=$Banco;Integrated Security=True;TrustServerCertificate=True"

New-Item -ItemType Directory -Force -Path $Global:RaizPorSmb | Out-Null
Copy-Item (Join-Path $PSScriptRoot 'registrar-rotinas.ps1') (Join-Path $Global:RaizPorSmb 'registrar-rotinas.ps1') -Force

# OS PARAMETROS VAO NUMA HASHTABLE (splat), e nao numa linha com continuacao de crase: a continuacao
# nao sobrevive a viagem pelo _remoto.ps1 - foi o que quebrou a primeira versao deste script.
$saida = Invoke-NoServidor -Nome 'crm-rotinas' -TimeoutSegundos 300 -Script @"
`$p = @{}
`$p.DestinoDaCarga   = '$Destino'
`$p.PastaDasRotinas  = '$PastaDasRotinas'
`$p.Conexao          = '$conexaoLocal'
`$p.NomeTarefaAnual  = '$NomeTarefa'
`$p.MesAnual         = '$Mes'
`$p.DiaAnual         = $DiaDoMes
`$p.HoraAnual        = '$Hora'
`$p.NomeTarefaMensal = '$NomeTarefaMensal'
`$p.DiaMensal        = $DiaDaRotinaMensal
`$p.HoraMensal       = '$HoraMensal'
`$p.TarefaAntiga     = '$TarefaAntiga'
& '$Global:RaizRemota\registrar-rotinas.ps1' @p
& schtasks.exe /Query /TN '$NomeTarefa' /FO LIST
& schtasks.exe /Query /TN '$NomeTarefaMensal' /FO LIST
"@
Write-Host $saida
if ($saida -notmatch 'codigo das rotinas: 0') {
    throw "Nao consegui registrar as rotinas no servidor. A saida esta acima."
}
Ok "tarefa $NomeTarefa registrada para $DiaDoMes/$Mes as $Hora, todo ano, como SYSTEM"
Ok "tarefa $NomeTarefaMensal registrada para todo dia $DiaDaRotinaMensal as $HoraMensal, como SYSTEM"
# -------------------------------------------------------------------------------------------------
Passo '4. Rodar uma vez e conferir'
# -------------------------------------------------------------------------------------------------
if ($NaoRodarAgora) {
    Write-Host '   !   primeira execucao pulada a pedido (-NaoRodarAgora)' -ForegroundColor Yellow
    return
}

# UMA HORA DE ESPERA: a PAM sao 3 anos x 9 lotes x 2 recortes de consulta ao SIDRA, com ~5 MB cada;
# a estrutura sao mais quatro consultas ao SIDRA e um ZIP da ANP, que juntos levam menos de um minuto.
# Os precos sao tres arquivos pequenos e levam segundos - por isso rodam primeiro.
foreach ($par in @(@($NomeTarefaMensal, 'precos'), @($NomeTarefa, 'fontes-publicas'))) {
    $tarefa, $prefixo = $par
    $saida = Invoke-NoServidor -Nome "crm-fontes-primeira-$prefixo" -TimeoutSegundos 3600 -Script @"
schtasks /Run /TN '$tarefa' | Out-Null
`$limite = (Get-Date).AddMinutes(55)
while ((Get-Date) -lt `$limite) {
    Start-Sleep -Seconds 15
    `$linha = schtasks /Query /TN '$tarefa' /FO LIST | Select-String 'Status:|Estado:'
    if (`$linha -and (`$linha -join ' ') -notmatch 'Running|Em execu') { break }
}
`$log = Get-ChildItem '$PastaDasRotinas\logs\$prefixo-*.log' | Sort-Object LastWriteTime | Select-Object -Last 1
if (`$log) {
    Write-Output ('log: ' + `$log.FullName)
    Get-Content `$log.FullName -Tail 40
} else {
    Write-Output 'NENHUM LOG FOI ESCRITO: a tarefa $tarefa pode nao ter chegado a rodar.'
}
"@
    Write-Host $saida
}

$depois = Retrato 'estado das fontes publicas no servidor, depois:'

# CADA FONTE E CONFERIDA SEPARADAMENTE. Olhar so uma deixaria passar o caso em que a PAM carregou e a
# ANP nao - que e exatamente o que cada carga nova acrescenta de risco.
$vazias = $depois.Keys | Where-Object { $_ -notlike 'anos*' -and $depois[$_] -eq 0 }
if ($vazias) {
    throw "As tarefas rodaram e estas tabelas continuam vazias: $($vazias -join ', '). O log do servidor esta acima."
}

Ok 'o servidor atualiza a PAM e a estrutura uma vez por ano, e os precos todo mes, sozinho'
Write-Host ''
