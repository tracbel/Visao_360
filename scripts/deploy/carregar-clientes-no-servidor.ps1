<#
  carregar-clientes-no-servidor.ps1 - leva o CADASTRO DE CLIENTES (SA1 do Protheus) direto para o banco
  do servidor. Nada passa pelo banco desta estacao.

      Set-Location C:\projetos\tracbel-crm
      powershell -ExecutionPolicy Bypass -File .\scripts\deploy\carregar-clientes-no-servidor.ps1

  =================================================================================================
  POR QUE ESTE SCRIPT EXISTE (documento 48, secao 5.5)

  Em 24/09/2026 a Visao Diretoria abria vazia no servidor. A causa, medida no banco de la:
  comercial.Cliente com ZERO linhas - efeito da sanitizacao de 15/09 que nunca foi seguida de recarga.
  Sem cliente nao ha endereco, sem endereco nao ha municipio, e as 4.183 vendas que o ART le ficavam
  todas pendentes com COMPRADOR_AUSENTE_NO_CRM (100% delas).

  O CAMINHO E O MESMO DO TERRITORIO (scripts/deploy/carregar-territorio-no-servidor.ps1): a carga roda
  nesta estacao, que tem o codigo e o SDK, e GRAVA NO BANCO DO SERVIDOR, que e onde o dado vai ser lido.
  Sem senha em arquivo, sem backup restaurado por cima, sem copiar banco de uma maquina para outra.

  ENSAIADO ANTES em simulacao contra a propria producao (transacao desfeita no fim): 38.744 lojas lidas,
  33.641 documentos distintos, 27.336 clientes e 27.336 enderecos a criar; 6.298 pendentes por
  FORA_DA_AREA_DE_ATUACAO, 6 por documento invalido, 1 por municipio nao reconhecido.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM
    0. confere que o banco do servidor responde e que quem executa e administrador do SQL Server;
    1. tira uma COPIA DE SEGURANCA (COPY_ONLY, com CHECKSUM, conferida) no proprio servidor;
    2. mostra o estado ANTES;
    3. roda a carga EM SIMULACAO e exige que ela feche - so entao roda de verdade;
    4. mostra o estado DEPOIS - e para com erro se nao chegar ao minimo esperado.

  Reexecutar e seguro: a carga e idempotente (reencontra o cliente pelo documento) e a segunda rodada
  nao cria nada. A volta atras, se precisar, e restaurar a copia do passo 1.

  A ORDEM IMPORTA: este script vem ANTES da carga do ART e da de faturamento. Sem cliente, nenhuma das
  duas casa dono, e as duas gravam menos do que deveriam sem reclamar.
#>

[CmdletBinding()]
param(
    [string] $Servidor = '10.150.4.249',
    [string] $Banco    = 'TracbelCrm',
    [int]    $MinimoDeClientes = 20000,
    [string] $ArquivoEnv = '',
    [switch] $SemCopiaDeSeguranca,
    [switch] $SomenteSimular
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

function Passo([string]$t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string]$t)    { Write-Host "   ok  $t" -ForegroundColor Green }

# ESTE SCRIPT RODA NA ESTACAO DE DESENVOLVIMENTO, NAO NO SERVIDOR: ele precisa do codigo-fonte da carga
# e do SDK. E a carga que fala com o banco do servidor, pela rede.
if (-not (Test-Path (Join-Path $raiz 'src\Tracbel.Crm.Carga\Tracbel.Crm.Carga.csproj'))) {
    throw "Nao achei o codigo da carga em $raiz. Rode este script na estacao de desenvolvimento, dentro do repositorio - nao no servidor."
}

# A VERSAO EXIGIDA E A DO global.json, e nao um numero escrito aqui.
$sdkExigido = ((Get-Content (Join-Path $raiz 'global.json') -Raw | ConvertFrom-Json).sdk.version -split '\.')[0]
$sdks = @()
if (Get-Command dotnet -ErrorAction SilentlyContinue) { $sdks = @(& dotnet --list-sdks) }
if (-not ($sdks | Where-Object { $_ -match "^$sdkExigido\." })) {
    throw "Esta maquina nao tem o SDK do .NET $sdkExigido (o que o global.json fixa)."
}

# -------------------------------------------------------------------------------------------------
# As credenciais do Protheus vem do .env e NUNCA sao impressas - mesmo padrao de
# scripts/integracao/rodar-carga-do-art.ps1.
# -------------------------------------------------------------------------------------------------
function Ler-Env([string]$caminho) {
    $mapa = @{}
    if (-not (Test-Path $caminho)) { return $mapa }
    foreach ($linha in Get-Content $caminho) {
        $t = $linha.Trim()
        if (-not $t -or $t.StartsWith('#')) { continue }
        $i = $t.IndexOf('=')
        if ($i -lt 1) { continue }
        $mapa[$t.Substring(0, $i).Trim()] = $t.Substring($i + 1).Trim()
    }
    return $mapa
}

# O .env NAO E VERSIONADO e mora no checkout principal, nao em cada worktree. Este script roda tanto do
# principal quanto de um worktree de publicacao, entao ele procura nos dois - e o segundo endereco sai do
# proprio git (`--git-common-dir` aponta para o .git do principal), nao de um caminho cravado aqui.
$candidatos = @((Join-Path $raiz '.env'))
try {
    $gitComum = (& git -C $raiz rev-parse --git-common-dir 2>$null)
    if ($gitComum) {
        if (-not [System.IO.Path]::IsPathRooted($gitComum)) { $gitComum = Join-Path $raiz $gitComum }
        $candidatos += (Join-Path (Split-Path $gitComum -Parent) '.env')
    }
}
catch { }
if ($ArquivoEnv) { $candidatos = @($ArquivoEnv) }

$arquivoDeFontes = $candidatos | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $arquivoDeFontes) {
    throw "Nao achei o .env com as credenciais do Protheus. Procurei em:`n  " + ($candidatos -join "`n  ") + "`nPasse o caminho com -ArquivoEnv. Nada foi gravado."
}

$fontes = Ler-Env $arquivoDeFontes
foreach ($chave in 'TOTVS_DB_SERVER', 'TOTVS_DB_DATABASE', 'TOTVS_DB_USER', 'TOTVS_DB_PASSWORD') {
    if (-not $fontes[$chave]) { throw "$arquivoDeFontes nao tem $chave - sem ele a SA1 nao pode ser lida. Nada foi gravado." }
}
Ok "credenciais do Protheus lidas de $arquivoDeFontes (nenhum valor e impresso)"

$conexao = "Server=$Servidor,1433;Database=$Banco;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30"
$odbc    = "Driver={ODBC Driver 18 for SQL Server};Server=$Servidor,1433;Database=$Banco;Trusted_Connection=yes;TrustServerCertificate=yes;"

function Escalar([string]$sql, [int]$tempo = 120) {
    $c = New-Object System.Data.Odbc.OdbcConnection $odbc
    try {
        $c.Open()
        $cmd = $c.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = $tempo
        return $cmd.ExecuteScalar()
    } finally { $c.Close() }
}

function Retrato([string]$rotulo) {
    $medidas = [ordered]@{
        'comercial.Cliente'           = 'SELECT COUNT(*) FROM comercial.Cliente'
        'comercial.Endereco'          = 'SELECT COUNT(*) FROM comercial.Endereco'
        '  com municipio do catalogo' = 'SELECT COUNT(*) FROM comercial.Endereco WHERE MunicipioId IS NOT NULL'
        'filiais com cliente'         = 'SELECT COUNT(DISTINCT EmpresaId) FROM comercial.Cliente'
        'registros do ART pendentes'  = "SELECT COUNT(*) FROM integracao.RegistroDeOrigem WHERE Decisao = 'Pendente'"
        'frota.VendaDeMaquina'        = 'SELECT COUNT(*) FROM frota.VendaDeMaquina'
    }
    Write-Host "   $rotulo"
    $r = @{}
    foreach ($k in $medidas.Keys) {
        $v = [int](Escalar $medidas[$k])
        $r[$k] = $v
        Write-Host ("     {0,-30} {1,8:N0}" -f $k, $v)
    }
    return $r
}

function Rodar([switch]$Simulando) {
    $variaveis = @{
        'ConnectionStrings__Crm'    = $conexao
        'ProtheusBanco__Servidor'   = $(if ($fontes['TOTVS_DB_PORT']) { "$($fontes['TOTVS_DB_SERVER']),$($fontes['TOTVS_DB_PORT'])" } else { $fontes['TOTVS_DB_SERVER'] })
        'ProtheusBanco__Banco'      = $fontes['TOTVS_DB_DATABASE']
        'ProtheusBanco__Usuario'    = $fontes['TOTVS_DB_USER']
        'ProtheusBanco__Senha'      = $fontes['TOTVS_DB_PASSWORD']
    }
    foreach ($k in $variaveis.Keys) { Set-Item -Path "Env:$k" -Value $variaveis[$k] }

    try {
        Push-Location (Join-Path $raiz 'src\Tracbel.Crm.Carga')
        $argumentos = @('run', '--', '--somente-clientes-protheus')
        if ($Simulando) { $argumentos += '--simular' }
        # `Out-Host` DE PROPOSITO: em PowerShell tudo o que uma funcao emite e nao e capturado vira
        # valor de retorno. Sem isto, o texto da carga se mistura com o codigo de saida e o
        # `-ne 0` do chamador passa a comparar um array - a carga "falha" sempre, sem motivo visivel.
        & dotnet @argumentos | Out-Host
        $codigo = $LASTEXITCODE
    }
    finally {
        Pop-Location
        foreach ($k in $variaveis.Keys) { Remove-Item -Path "Env:$k" -ErrorAction SilentlyContinue }
    }
    return $codigo
}

# -------------------------------------------------------------------------------------------------
Passo '0. Banco do servidor'
# -------------------------------------------------------------------------------------------------
$quem = Escalar 'SELECT SUSER_SNAME()'
if ([int](Escalar "SELECT IS_SRVROLEMEMBER('sysadmin')") -ne 1) {
    throw "A conta $quem entra no SQL Server de $Servidor mas nao e sysadmin: a copia de seguranca nao pode ser feita. Nada foi gravado."
}
Ok "SQL Server de $Servidor responde; conta $quem com permissao de administrador"

# -------------------------------------------------------------------------------------------------
Passo '1. Copia de seguranca (no proprio servidor)'
# -------------------------------------------------------------------------------------------------
if ($SemCopiaDeSeguranca) {
    Write-Host '   !   copia pulada a pedido (-SemCopiaDeSeguranca)' -ForegroundColor Yellow
} else {
    $pasta = Escalar "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400))"
    $destino = Join-Path $pasta ("{0}-antes-dos-clientes-{1:yyyyMMdd-HHmm}.bak" -f $Banco, (Get-Date))
    Escalar "BACKUP DATABASE [$Banco] TO DISK = N'$destino' WITH COPY_ONLY, CHECKSUM, INIT" 3600 | Out-Null
    Escalar "RESTORE VERIFYONLY FROM DISK = N'$destino' WITH CHECKSUM" 3600 | Out-Null
    Ok "copia feita e conferida: $destino"
}

# -------------------------------------------------------------------------------------------------
Passo '2. Antes'
# -------------------------------------------------------------------------------------------------
$antes = Retrato 'estado do banco do servidor antes da carga:'

# -------------------------------------------------------------------------------------------------
Passo '3. Simulacao (transacao desfeita no fim)'
# -------------------------------------------------------------------------------------------------
# A SIMULACAO NAO E CERIMONIA: ela ja pegou duas vezes dado que derrubaria a carga - um CPF que nao
# passa no digito verificador e A1_PESSOA discordando do documento. Rodar de verdade sem ela e
# descobrir isso com a transacao aberta.
if ((Rodar -Simulando) -ne 0) { throw 'A SIMULACAO FALHOU. Nada foi gravado.' }
Ok 'simulacao fechou'

if ($SomenteSimular) {
    Write-Host ''
    Write-Host 'Parando aqui a pedido (-SomenteSimular). Nada foi gravado.' -ForegroundColor Yellow
    return
}

# -------------------------------------------------------------------------------------------------
Passo '4. Carga de verdade'
# -------------------------------------------------------------------------------------------------
if ((Rodar) -ne 0) { throw 'A CARGA FALHOU. Confira a copia de seguranca do passo 1.' }
Ok 'carga concluida'

# -------------------------------------------------------------------------------------------------
Passo '5. Depois'
# -------------------------------------------------------------------------------------------------
$depois = Retrato 'estado do banco do servidor depois da carga:'

$criados = $depois['comercial.Cliente'] - $antes['comercial.Cliente']
Write-Host ''
Write-Host ("   clientes criados nesta rodada: {0:N0}" -f $criados)

if ($depois['comercial.Cliente'] -lt $MinimoDeClientes) {
    throw ("So ha {0:N0} clientes no banco, e o esperado era pelo menos {1:N0}. Algo ficou pelo caminho - confira as pendencias na saida da carga." -f $depois['comercial.Cliente'], $MinimoDeClientes)
}
Ok ("{0:N0} clientes no banco do servidor" -f $depois['comercial.Cliente'])

Write-Host ''
Write-Host 'PROXIMO PASSO: o servico do ART (TracbelCrmSincronizacaoArt) volta a casar comprador no' -ForegroundColor Cyan
Write-Host 'proximo ciclo. Para nao esperar, reinicie-o:' -ForegroundColor Cyan
Write-Host "  sc.exe \\$Servidor stop TracbelCrmSincronizacaoArt; sc.exe \\$Servidor start TracbelCrmSincronizacaoArt"
