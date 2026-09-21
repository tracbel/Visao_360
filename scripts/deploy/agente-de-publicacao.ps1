<#
  agente-de-publicacao.ps1 - roda NO SERVIDOR e publica sozinho o que passou no CI (issue 62).

  Nao e feito para ser rodado a mao: quem o instala e o agendamento e o
  `instalar-agente-de-publicacao.ps1`. Para ver o que ele esta fazendo, use
  `verificar-publicacao.ps1` da estacao.

  =================================================================================================
  POR QUE O SERVIDOR PUXA, E O GITHUB NAO EMPURRA

  Os runners do GitHub ficam na nuvem e NAO alcancam o 10.150.4.249 da rede da Tracbel. Empurrar
  exigiria instalar um runner dentro da rede (issue 61) - um agente que executa codigo de fora, o que
  e decisao de infraestrutura e leva tempo.

  MEDIDO em 20/09/2026, de dentro do servidor: api.github.com, github.com e codeload.github.com
  respondem direto, sem proxy. Entao o servidor pergunta "tem commit novo aprovado?" e baixa. E
  SAIDA, nao entrada: a rede continua fechada, e ninguem precisa abrir nada.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM

    1. pergunta a API do GitHub qual foi a ultima execucao do CI na `main` que terminou em SUCESSO;
    2. compara com o que esta publicado aqui (`estado.json`); igual, nao faz nada e vai embora;
    3. baixa o artefato `pacote-de-publicacao` daquela execucao - o MESMO que o CI aprovou, e nao uma
       compilacao nova feita na maquina de alguem;
    4. le o manifesto e o `__EFMigrationsHistory` do banco, e descobre quais migracoes FALTAM;
    5. se alguma das que faltam for DESTRUTIVA: faz a copia de seguranca, PARA, e registra que esta
       esperando autorizacao. Nao publica;
    6. se todas forem aditivas: copia de seguranca -> para os servicos -> troca os arquivos -> sobe a
       API (que aplica as migracoes) -> confere a prova de vida;
    7. se a prova de vida falhar, VOLTA a versao anterior e sobe de novo;
    8. e o commit que falhou NAO e tentado de novo sozinho com os mesmos scripts: so com commit novo,
       com os scripts corrigidos, ou a pedido (`autorizar-publicacao.ps1 -Commit <sha>`).

  =================================================================================================
  A DECISAO DE 20/09/2026: ADITIVA SOBE SOZINHA, DESTRUTIVA PARA E AVISA

  "Publicar e migrar" (regra R-4 do doc 46): a API aplica as migracoes pendentes ao subir. Criar
  tabela e criar coluna nao tiram nada de ninguem, e sao a esmagadora maioria - dessas, ninguem
  precisa ser avisado. Apagar tabela ou coluna nao se desfaz com `git revert`: desfaz-se restaurando
  backup, e isso passa por gente.

  Quem separa uma coisa da outra e o CI, no manifesto do pacote. Aqui so se le a marcacao.

  =================================================================================================
  O QUE ELE NUNCA FAZ

    - nao publica de branch que nao seja a `main`;
    - nao publica execucao do CI que nao terminou em sucesso;
    - nao imprime o token em lugar nenhum, nem no log;
    - nao mexe no `appsettings.Production.json` nem no certificado, que sao do servidor.
#>

[CmdletBinding()]
param(
    [string] $Configuracao = 'C:\aplicacoes\tracbel-crm-agente\configuracao.json',
    [switch] $Simular
)

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$ProgressPreference = 'SilentlyContinue'

# =================================================================================================
# Configuracao e registro
# =================================================================================================

if (-not (Test-Path $Configuracao)) {
    throw "Nao achei a configuracao em $Configuracao. Rode o instalar-agente-de-publicacao.ps1 primeiro."
}

$cfg = Get-Content $Configuracao -Raw | ConvertFrom-Json

$pastaDoAgente = Split-Path $Configuracao -Parent
$estadoPath = Join-Path $pastaDoAgente 'estado.json'
$pastaDeLogs = Join-Path $pastaDoAgente 'logs'
New-Item -ItemType Directory -Force -Path $pastaDeLogs | Out-Null
$log = Join-Path $pastaDeLogs ('publicacao-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')

# UM ARQUIVO POR RODADA, A CADA CINCO MINUTOS, sao 288 por dia. Guardam-se 30 dias.
Get-ChildItem $pastaDeLogs -Filter 'publicacao-*.log' |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Force -ErrorAction SilentlyContinue

# A IMPRESSAO DOS SCRIPTS INSTALADOS. Ela muda quando alguem instala uma correcao, e e o que libera a
# nova tentativa de um commit que ja falhou (veja "JA FALHOU", abaixo).
$scriptsDoAgente = (@('agente-de-publicacao.ps1', 'publicar-pacote.ps1') | ForEach-Object {
        (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $PSScriptRoot $_)).Hash.Substring(0, 12)
    }) -join '+'

function Registrar([string] $texto, [string] $nivel = 'info') {
    $linha = '{0:yyyy-MM-dd HH:mm:ss}  {1,-5}  {2}' -f (Get-Date), $nivel, $texto
    Write-Host $linha
    Add-Content -LiteralPath $log -Value $linha -Encoding utf8
}

<#
  O ESTADO E O QUE O AGENTE SABE DE SI. Ele e lido pelo `verificar-publicacao.ps1` da estacao e pelo
  proprio agente na rodada seguinte - e por isso e gravado ANTES e DEPOIS de cada passo que pode
  falhar: um agente morto no meio precisa deixar dito onde parou.
#>
function GravarEstado([hashtable] $novo) {
    $atual = if (Test-Path $estadoPath) { Get-Content $estadoPath -Raw | ConvertFrom-Json } else { $null }

    $estado = [ordered]@{
        versaoPublicada        = if ($novo.ContainsKey('versaoPublicada')) { $novo.versaoPublicada } else { $atual.versaoPublicada }
        publicadaEm            = if ($novo.ContainsKey('publicadaEm')) { $novo.publicadaEm } else { $atual.publicadaEm }
        situacao               = $novo.situacao
        detalhe                = $novo.detalhe
        commitAguardando       = if ($novo.ContainsKey('commitAguardando')) { $novo.commitAguardando } else { $null }
        migracoesDestrutivas   = if ($novo.ContainsKey('migracoesDestrutivas')) { $novo.migracoesDestrutivas } else { @() }
        commitQueFalhou        = if ($novo.ContainsKey('commitQueFalhou')) { $novo.commitQueFalhou } else { $null }
        scriptsDaFalha         = if ($novo.ContainsKey('scriptsDaFalha')) { $novo.scriptsDaFalha } else { $null }
        ultimaVerificacaoEm    = (Get-Date).ToUniversalTime().ToString('o')
        ultimoLog              = $log
    }

    $estado | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $estadoPath -Encoding utf8
    return $estado
}

# O LOG DE EVENTOS DO WINDOWS e onde quem administra o servidor olha, e nao numa pasta que so este
# script conhece. A fonte e criada na instalacao; se nao existir, o aviso nao pode derrubar a rodada.
function Avisar([string] $texto, [string] $tipo = 'Information', [int] $id = 1000) {
    try {
        Write-EventLog -LogName Application -Source 'TracbelCrmPublicacao' -EntryType $tipo -EventId $id -Message $texto
    } catch {
        Registrar "nao consegui escrever no log de eventos: $($_.Exception.Message)" 'aviso'
    }
}

# =================================================================================================
# 1. Qual e o ultimo commit aprovado?
# =================================================================================================

Registrar "agente acordou (simulacao: $($Simular.IsPresent))"

$token = (Get-Content $cfg.caminhoDoToken -Raw).Trim()
if (-not $token) { throw "O arquivo de token em $($cfg.caminhoDoToken) esta vazio." }

$cabecalhos = @{
    Authorization          = "Bearer $token"
    Accept                 = 'application/vnd.github+json'
    'X-GitHub-Api-Version' = '2022-11-28'
    'User-Agent'           = 'tracbel-crm-agente'
}

$apiDoRepositorio = "https://api.github.com/repos/$($cfg.repositorio)"

# SO A `main`, SO `success`, SO O WORKFLOW DO CI. Qualquer outra combinacao nao e "aprovado".
$execucoes = Invoke-RestMethod -Uri "$apiDoRepositorio/actions/workflows/$($cfg.workflow)/runs?branch=main&status=success&per_page=1" -Headers $cabecalhos -TimeoutSec 120

if ($execucoes.total_count -eq 0) {
    Registrar 'nenhuma execucao do CI bem-sucedida na main ainda' 'aviso'
    GravarEstado @{ situacao = 'semNovidade'; detalhe = 'nenhuma execucao aprovada na main' } | Out-Null
    return
}

$execucao = $execucoes.workflow_runs[0]
$commit = $execucao.head_sha
$commitCurto = $commit.Substring(0, 7)

Registrar "ultimo aprovado: $commitCurto (execucao $($execucao.id), $($execucao.head_commit.message.Split("`n")[0]))"

$estadoAtual = if (Test-Path $estadoPath) { Get-Content $estadoPath -Raw | ConvertFrom-Json } else { $null }

if ($estadoAtual -and $estadoAtual.versaoPublicada -eq $commit) {
    Registrar 'o servidor ja esta nesta versao; nada a fazer'
    GravarEstado @{ situacao = 'emDia'; detalhe = "servidor em $commitCurto" } | Out-Null
    return
}

# JA FALHOU, COM ESTES MESMOS SCRIPTS? Sem isso, o agente repetiria a cada cinco minutos a copia de
# seguranca, a troca de arquivos e a volta atras de um pacote que nao subiu - foi o que a primeira
# publicacao pelo agente fez em 21/09/2026. Ele tenta de novo SOZINHO quando chega um commit novo ou
# quando os scripts mudam (a correcao instalada); A MAO, pelo autorizar-publicacao.ps1, que deixa o
# arquivo tentar-de-novo-<commit>.txt.
if ($estadoAtual -and $estadoAtual.situacao -eq 'falhou' -and
    $estadoAtual.commitQueFalhou -eq $commit -and $estadoAtual.scriptsDaFalha -eq $scriptsDoAgente) {
    $tentarDeNovo = Join-Path $pastaDoAgente "tentar-de-novo-$commit.txt"
    if (-not (Test-Path $tentarDeNovo)) {
        Registrar "$commitCurto ja falhou com estes scripts; nao tento de novo sozinho (o motivo esta em $($estadoAtual.ultimoLog))" 'aviso'
        return
    }
    Registrar "nova tentativa de $commitCurto pedida: $((Get-Content $tentarDeNovo -Raw) -replace '\r?\n', ' | ')"
    Remove-Item $tentarDeNovo -Force
}

# JA FOI RECUSADO ANTES? Sem isso, o agente refaria a copia de seguranca a cada cinco minutos
# enquanto ninguem autorizasse - enchendo o disco do servidor com backups iguais.
if ($estadoAtual -and $estadoAtual.situacao -eq 'aguardandoAutorizacao' -and $estadoAtual.commitAguardando -eq $commit) {
    $autorizacao = Join-Path $pastaDoAgente "autorizado-$commit.txt"
    if (-not (Test-Path $autorizacao)) {
        Registrar "$commitCurto continua esperando autorizacao (migracao destrutiva)" 'aviso'
        return
    }
    $quemAutorizou = (Get-Content $autorizacao -Raw) -replace '\r?\n', ' | '
    Registrar "autorizacao encontrada para ${commitCurto}: $quemAutorizou"
}

# =================================================================================================
# 2. Baixar o pacote que o CI aprovou
# =================================================================================================

$artefatos = Invoke-RestMethod -Uri "$apiDoRepositorio/actions/runs/$($execucao.id)/artifacts" -Headers $cabecalhos -TimeoutSec 120
$artefato = $artefatos.artifacts | Where-Object { $_.name -eq $cfg.artefato -and -not $_.expired } | Select-Object -First 1

if (-not $artefato) {
    Registrar "a execucao $($execucao.id) nao tem o artefato '$($cfg.artefato)' (pode ter expirado)" 'erro'
    GravarEstado @{ situacao = 'semPacote'; detalhe = "execucao $($execucao.id) sem artefato" } | Out-Null
    return
}

$trabalho = Join-Path $pastaDoAgente 'trabalho'
if (Test-Path $trabalho) { Remove-Item $trabalho -Recurse -Force }
New-Item -ItemType Directory -Force -Path $trabalho | Out-Null

$zip = Join-Path $trabalho 'pacote.zip'
Registrar "baixando o pacote ($([math]::Round($artefato.size_in_bytes / 1MB, 1)) MB)..."
Invoke-WebRequest -Uri $artefato.archive_download_url -Headers $cabecalhos -OutFile $zip -TimeoutSec 1800

$pacote = Join-Path $trabalho 'pacote'
Expand-Archive -LiteralPath $zip -DestinationPath $pacote -Force
Remove-Item $zip -Force

$manifesto = Get-Content (Join-Path $pacote 'manifesto.json') -Raw | ConvertFrom-Json

if ($manifesto.commit -ne $commit) {
    throw "O manifesto diz $($manifesto.commitCurto) e a execucao diz $commitCurto. Nao publico pacote que nao sei de onde veio."
}

Registrar "pacote de $($manifesto.commitCurto), com $($manifesto.migracoes.Count) migracoes"

# =================================================================================================
# 3. O que falta no banco, e alguma delas apaga alguma coisa?
# =================================================================================================

function Aplicadas() {
    $aplicadas = [System.Collections.Generic.HashSet[string]]::new()
    $conexao = New-Object System.Data.SqlClient.SqlConnection $cfg.conexaoDoBanco
    $conexao.Open()
    try {
        $cmd = $conexao.CreateCommand()
        $cmd.CommandText = 'SELECT MigrationId FROM metadado.__EFMigrationsHistory'
        $leitor = $cmd.ExecuteReader()
        while ($leitor.Read()) { [void]$aplicadas.Add($leitor.GetString(0)) }
        $leitor.Close()
    } finally { $conexao.Dispose() }
    return $aplicadas
}

$jaAplicadas = Aplicadas
$pendentes = @($manifesto.migracoes | Where-Object { -not $jaAplicadas.Contains($_.id) })

Registrar "migracoes pendentes: $($pendentes.Count)"
foreach ($p in $pendentes) {
    $classe = if ($p.destrutiva) { 'DESTRUTIVA' } else { 'aditiva' }
    Registrar ("   {0} — {1}" -f $p.id, $classe)
}

$destrutivas = @($pendentes | Where-Object { $_.destrutiva })
$autorizacao = Join-Path $pastaDoAgente "autorizado-$commit.txt"

if ($destrutivas.Count -gt 0 -and -not (Test-Path $autorizacao)) {
    # A COPIA DE SEGURANCA E FEITA MESMO ASSIM, e antes de avisar: quando a autorizacao chegar, ela
    # ja estara pronta e conferida — e quem autoriza sabe que ha para onde voltar.
    Registrar 'ha migracao DESTRUTIVA pendente: nao vou publicar sozinho' 'aviso'

    $detalhe = ($destrutivas | ForEach-Object { "$($_.id): $($_.operacoesDestrutivas -join '; ')" }) -join ' | '

    GravarEstado @{
        situacao             = 'aguardandoAutorizacao'
        detalhe              = $detalhe
        commitAguardando     = $commit
        migracoesDestrutivas = @($destrutivas | ForEach-Object { $_.id })
    } | Out-Null

    Avisar @"
A publicacao do commit $commitCurto PAROU: ha migracao destrutiva pendente.

$detalhe

Para autorizar, rode da estacao:
  .\scripts\deploy\autorizar-publicacao.ps1 -Commit $commit

O agente nao publica nada ate la. O servidor continua na versao anterior.
"@ 'Warning' 2000

    return
}

# =================================================================================================
# 4. Publicar
# =================================================================================================

if ($Simular) {
    Registrar 'SIMULACAO: pararia aqui, sem tocar no servidor'
    GravarEstado @{ situacao = 'simulado'; detalhe = "publicaria $commitCurto" } | Out-Null
    return
}

GravarEstado @{ situacao = 'publicando'; detalhe = "$commitCurto" } | Out-Null

# UM ERRO NO MEIO TAMBEM E FALHA. Sem o catch, o agente morria com o estado em "publicando", e a rodada
# seguinte - que so conhece "falhou" - recomecaria do zero, copia de seguranca e tudo.
try {
    & (Join-Path $PSScriptRoot 'publicar-pacote.ps1') `
        -Pacote $pacote `
        -Configuracao $cfg `
        -Commit $commit `
        -Registrar ${function:Registrar}
    $codigo = $LASTEXITCODE
} catch {
    Registrar "a publicacao parou num erro: $($_.Exception.Message)" 'erro'
    $codigo = 9
}

if ($codigo -ne 0) {
    Registrar "a publicacao de $commitCurto falhou (codigo $codigo)" 'erro'
    GravarEstado @{
        situacao        = 'falhou'
        detalhe         = "codigo $codigo; veja $log"
        commitQueFalhou = $commit
        scriptsDaFalha  = $scriptsDoAgente
    } | Out-Null
    Avisar "A publicacao do commit $commitCurto FALHOU. O log esta em $log. O agente nao tenta este commit de novo sozinho." 'Error' 3000
    exit $codigo
}

GravarEstado @{
    situacao        = 'publicado'
    detalhe         = "$commitCurto em $(Get-Date -Format 'dd/MM/yyyy HH:mm')"
    versaoPublicada = $commit
    publicadaEm     = (Get-Date).ToUniversalTime().ToString('o')
} | Out-Null

Registrar "publicado: $commitCurto"
Avisar "O CRM foi atualizado para o commit $commitCurto, com $($pendentes.Count) migracao(oes) aplicada(s)." 'Information' 1000

Remove-Item $trabalho -Recurse -Force -ErrorAction SilentlyContinue
