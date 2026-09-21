<#
  publicar-pacote.ps1 - troca a versao do CRM no servidor, com volta atras (issue 62).

  Roda NO SERVIDOR, chamado pelo `agente-de-publicacao.ps1`. Nao fala com o GitHub e nao decide nada:
  quem escolhe o commit e quem le o manifesto e o agente. Aqui so se troca arquivo, sobe servico e
  confere se a aplicacao respondeu.

  =================================================================================================
  A ORDEM, E POR QUE ELA E ESSA

    1. COPIA DE SEGURANCA DO BANCO, antes de qualquer coisa. A API aplica as migracoes ao subir
       (regra R-4 do doc 46): depois que ela sobe, ja e tarde para decidir que se queria um backup.
    2. GUARDA A VERSAO ATUAL numa pasta ao lado. E ela que volta se a prova de vida falhar — voltar
       pelo Git exigiria compilar no servidor, que nao tem SDK.
    3. PARA os servicos. A sincronizacao primeiro, que escreve no banco; a API depois.
    4. TROCA os arquivos, PRESERVANDO o que e do servidor: `appsettings.Production.json` (senha do
       banco e segredo do Entra ID) e a pasta `ssl`. Um deploy que sobrescrevesse esses dois
       derrubaria a aplicacao sem nenhum erro de compilacao para avisar.
    5. SOBE a API — e e ela que aplica as migracoes.
    6. PROVA DE VIDA. Se falhar, VOLTA a versao guardada e sobe de novo.
    7. ROTINAS: recria as tarefas agendadas das fontes publicas com o registrar-rotinas.ps1 do pacote.

  A volta atras cobre o CODIGO. Migracao destrutiva nao volta assim — volta restaurando a copia do
  passo 1 —, e por isso o agente nem chega aqui quando ha uma sem autorizacao.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Pacote,
    [Parameter(Mandatory = $true)] $Configuracao,
    [Parameter(Mandatory = $true)] [string] $Commit,
    $Registrar
)

$ErrorActionPreference = 'Stop'

$cfg = $Configuracao
$commitCurto = $Commit.Substring(0, 7)

function Diga([string] $texto, [string] $nivel = 'info') {
    if ($Registrar) { & $Registrar $texto $nivel } else { Write-Host "$nivel  $texto" }
}

# O QUE E DO SERVIDOR E NAO VEM NO PACOTE. Preservar isto e o que separa um deploy de um desastre.
$doServidor = @('appsettings.Production.json', 'ssl')

function Escalar([string] $sql, [int] $timeout = 1800) {
    $c = New-Object System.Data.SqlClient.SqlConnection $cfg.conexaoDoBanco
    $c.Open()
    try {
        $cmd = $c.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $sql
        return $cmd.ExecuteScalar()
    } finally { $c.Dispose() }
}

function Servico([string] $acao, [string] $nome) {
    $s = Get-Service -Name $nome -ErrorAction SilentlyContinue
    if (-not $s) { Diga "servico $nome nao existe aqui" 'aviso'; return }

    if ($acao -eq 'parar' -and $s.Status -ne 'Stopped') {
        Stop-Service -Name $nome -Force
        (Get-Service $nome).WaitForStatus('Stopped', '00:02:00')
        Diga "servico $nome parado"
    }
    elseif ($acao -eq 'subir' -and $s.Status -ne 'Running') {
        # O ART FICA COMO ESTIVER: ele esta DESABILITADO desde a sanitizacao de 15/09 (doc 38 §9), e
        # religa-lo por causa de um deploy seria reativar integracao sem ninguem pedir.
        if ($s.StartType -eq 'Disabled') { Diga "servico $nome esta desabilitado; continua parado" 'aviso'; return }
        Start-Service -Name $nome
        (Get-Service $nome).WaitForStatus('Running', '00:02:00')
        Diga "servico $nome no ar"
    }
}

<# A prova de vida: a API responde, e responde que o banco esta conectado. #>
function EstaViva() {
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 5
        try {
            $r = Invoke-RestMethod -Uri $cfg.provaDeVida -TimeoutSec 20 -SkipCertificateCheck
            if ($r.banco -eq 'conectado' -or $r.status -eq 'ok' -or $r.saudavel -eq $true) { return $true }
            Diga "prova de vida respondeu, mas sem confirmar o banco: $($r | ConvertTo-Json -Compress)" 'aviso'
        } catch {
            # Os primeiros segundos sao normais: a API esta aplicando migracoes.
        }
    }
    return $false
}

# -------------------------------------------------------------------------------------------------
Diga "1. copia de seguranca do banco"
# -------------------------------------------------------------------------------------------------
$pastaDeBackup = Escalar "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400))" 60
$destinoDoBackup = Join-Path $pastaDeBackup ("{0}-antes-de-{1}-{2:yyyyMMdd-HHmm}.bak" -f $cfg.banco, $commitCurto, (Get-Date))

Escalar "BACKUP DATABASE [$($cfg.banco)] TO DISK = N'$destinoDoBackup' WITH COPY_ONLY, CHECKSUM, INIT" | Out-Null
Diga "copia em $destinoDoBackup"

# -------------------------------------------------------------------------------------------------
Diga "2. guardar a versao que esta no ar"
# -------------------------------------------------------------------------------------------------
$anterior = "$($cfg.destinoDaApi).anterior"
if (Test-Path $anterior) { Remove-Item $anterior -Recurse -Force }
if (Test-Path $cfg.destinoDaApi) {
    Copy-Item $cfg.destinoDaApi $anterior -Recurse -Force
    Diga "versao anterior guardada em $anterior"
}

# -------------------------------------------------------------------------------------------------
Diga "3. parar os servicos"
# -------------------------------------------------------------------------------------------------
Servico 'parar' $cfg.servicoDaSincronizacao
Servico 'parar' $cfg.servicoDaApi

# -------------------------------------------------------------------------------------------------
Diga "4. trocar os arquivos"
# -------------------------------------------------------------------------------------------------
function Trocar([string] $origem, [string] $destino) {
    if (-not (Test-Path $origem)) { Diga "o pacote nao traz $origem" 'aviso'; return }
    New-Item -ItemType Directory -Force -Path $destino | Out-Null

    $guardados = @{}
    foreach ($nome in $doServidor) {
        $caminho = Join-Path $destino $nome
        if (Test-Path $caminho) {
            $temporario = Join-Path ([System.IO.Path]::GetTempPath()) ("guardado-" + [guid]::NewGuid())
            Copy-Item $caminho $temporario -Recurse -Force
            $guardados[$nome] = $temporario
        }
    }

    Get-ChildItem $destino -Force | Remove-Item -Recurse -Force
    Copy-Item (Join-Path $origem '*') $destino -Recurse -Force

    foreach ($nome in $guardados.Keys) {
        Copy-Item $guardados[$nome] (Join-Path $destino $nome) -Recurse -Force
        Remove-Item $guardados[$nome] -Recurse -Force
        Diga "preservado do servidor: $nome"
    }
}

Trocar (Join-Path $Pacote 'api') $cfg.destinoDaApi
Trocar (Join-Path $Pacote 'carga') $cfg.destinoDaCarga

# -------------------------------------------------------------------------------------------------
Diga "5. subir a API (e com ela as migracoes)"
# -------------------------------------------------------------------------------------------------
Servico 'subir' $cfg.servicoDaApi

# -------------------------------------------------------------------------------------------------
Diga "6. prova de vida"
# -------------------------------------------------------------------------------------------------
if (EstaViva) {
    Diga "a aplicacao respondeu: $commitCurto no ar"
    Servico 'subir' $cfg.servicoDaSincronizacao
    Remove-Item $anterior -Recurse -Force -ErrorAction SilentlyContinue

    # ---------------------------------------------------------------------------------------------
    Diga "7. rotinas das fontes publicas"
    # ---------------------------------------------------------------------------------------------
    # AS ROTINAS AGENDADAS VEM NO PACOTE e sao recriadas a cada publicacao (registrar-rotinas.ps1).
    # Assim a rotina de uma fonte nova - a dos precos, na issue 66 - chega ao servidor junto com o
    # codigo que a usa, sem ninguem rodar script nenhum. E os scripts delas moram FORA da pasta da
    # carga, que o passo 4 apaga: foi o defeito achado em 21/09/2026, antes de o agente existir.
    #
    # FALHAR AQUI NAO DERRUBA A PUBLICACAO: a aplicacao ja esta no ar e provou que responde. O erro
    # vai para o registro, que e onde a publicacao inteira se conta.
    $registrar = Join-Path $Pacote 'rotinas\registrar-rotinas.ps1'
    if (Test-Path $registrar) {
        $saidaDasRotinas = & $registrar -DestinoDaCarga $cfg.destinoDaCarga -Conexao $cfg.conexaoDoBanco 2>&1 | Out-String
        if ($saidaDasRotinas -match 'codigo das rotinas: 0') {
            Diga 'rotinas das fontes publicas registradas (anual e mensal)'

            # A PRIMEIRA CARGA NAO ESPERA O CALENDARIO. Registrar a tarefa nao a roda: com a tabela
            # de precos vazia e o dia 20 ja passado, a tela ficaria um mes sem preco no servidor. O
            # disparo e assincrono - a publicacao nao espera a carga terminar.
            if ([int](Escalar 'SELECT COUNT(*) FROM organizacao.CotacaoDeProduto') -eq 0) {
                & schtasks.exe /Run /TN 'TracbelCrmPrecos' | Out-Null
                Diga 'tabela de precos vazia: a rotina TracbelCrmPrecos foi disparada agora'
            }
        } else {
            Diga "as rotinas das fontes publicas NAO foram registradas: $($saidaDasRotinas.Trim())" 'erro'
        }
    } else {
        Diga 'o pacote nao traz rotinas\registrar-rotinas.ps1; as rotinas ficam como estavam' 'aviso'
    }

    exit 0
}

# -------------------------------------------------------------------------------------------------
Diga "a prova de vida FALHOU: voltando a versao anterior" 'erro'
# -------------------------------------------------------------------------------------------------
if (-not (Test-Path $anterior)) {
    Diga 'nao ha versao anterior guardada para voltar. O servico fica parado, e o banco tem a copia do passo 1.' 'erro'
    exit 4
}

Servico 'parar' $cfg.servicoDaApi
Trocar $anterior $cfg.destinoDaApi
Servico 'subir' $cfg.servicoDaApi

if (EstaViva) {
    Diga 'a versao anterior voltou e esta no ar' 'aviso'
    exit 3
}

Diga 'nem a versao anterior subiu. O CRM esta FORA DO AR e precisa de gente.' 'erro'
exit 5
