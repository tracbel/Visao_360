<#
  carregar-territorio-no-servidor.ps1 - leva o TERRITORIO (municipio oficial do IBGE, ADR, responsaveis e
  area plantada) direto para o banco do servidor. Nada passa pelo banco desta estacao.

      Set-Location C:\projetos\tracbel-crm
      powershell -ExecutionPolicy Bypass -File .\scripts\deploy\carregar-territorio-no-servidor.ps1

  =================================================================================================
  POR QUE ESTE SCRIPT EXISTE (documento 32, secao 11.9)

  Em 14/09/2026 a tela do territorio abriu vazia no servidor. A causa, medida no banco de la: a carga do
  territorio so tinha rodado no banco desta estacao. O banco do servidor veio de um backup de 08/09 e
  tinha 0 municipio com codigo IBGE, 0 linha de area de atuacao, 0 responsavel e 0 area plantada.

  O CAMINHO CERTO E A CARGA GRAVAR ONDE O DADO VAI SER LIDO. Este script roda a mesma carga que a estacao
  usa, apontada para o SQL Server do servidor, com a conta do Windows de quem executa - sem senha em
  arquivo, sem backup restaurado por cima, sem copiar banco de uma maquina para outra.

  ENSAIADO ANTES em uma copia identica do banco do servidor (SQL Server 2025 local): 5.458 municipios
  reconhecidos, 113 criados, 192 enderecos corrigidos (2 pendentes), 203 da ADR, 203 + 406 responsaveis,
  45.582 linhas de area plantada; a segunda rodada gravou zero; o total da tela bateu com o SQL de
  conciliacao ao centavo.

  =================================================================================================
  O QUE ELE FAZ, NA ORDEM
    0. confere que o banco do servidor responde e que quem executa e administrador do SQL Server;
    1. tira uma COPIA DE SEGURANCA (COPY_ONLY, com CHECKSUM, conferida) no proprio servidor;
    2. mostra o estado ANTES;
    3. roda a carga do territorio (le as duas planilhas e o IBGE; grava no banco do servidor);
    4. mostra o estado DEPOIS - e para com erro se nao chegar aos 203 municipios da ADR.

  Reexecutar e seguro: a carga e idempotente e a segunda rodada nao grava nada.
  A volta atras, se precisar, e restaurar a copia do passo 1.
#>

[CmdletBinding()]
param(
    [string] $Servidor      = '10.150.4.249',
    [string] $Banco         = 'TracbelCrm',
    [string] $AreaDeAtuacao = '',
    [string] $CenEGestor    = '',
    [switch] $SemCopiaDeSeguranca
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

# ESTE SCRIPT RODA NA ESTACAO DE DESENVOLVIMENTO, NAO NO SERVIDOR. Ele precisa do codigo-fonte da carga,
# do SDK do .NET 9 e das duas planilhas - o servidor tem so a aplicacao publicada. E a carga que fala com
# o banco do servidor, pela rede.
if (-not (Test-Path (Join-Path $raiz 'src\Tracbel.Crm.Carga\Tracbel.Crm.Carga.csproj'))) {
    throw "Nao achei o codigo da carga em $raiz. Rode este script na estacao de desenvolvimento, dentro do repositorio tracbel-crm - nao no servidor."
}
# A VERSAO EXIGIDA E A DO global.json, e nao um numero escrito aqui: foi assim que esta checagem
# ficou presa no 9 depois da migracao para o .NET 10 (issue 084) e so apareceu ao rodar a carga.
$sdkExigido = ((Get-Content (Join-Path $raiz 'global.json') -Raw | ConvertFrom-Json).sdk.version -split '\.')[0]
$sdks = @()
if (Get-Command dotnet -ErrorAction SilentlyContinue) { $sdks = @(& dotnet --list-sdks) }
if (-not ($sdks | Where-Object { $_ -match "^$sdkExigido\." })) {
    throw "Esta maquina nao tem o SDK do .NET $sdkExigido (o que o global.json fixa). Rode este script na estacao de desenvolvimento - nao no servidor."
}

# AS PLANILHAS SAO PROCURADAS NA RAIZ DO REPOSITORIO quando o caminho nao vem. O nome de uma delas tem
# acento, e o PowerShell 5.1 le este arquivo como ANSI: escrever o nome aqui estragaria o acento.
if (-not $AreaDeAtuacao) { $AreaDeAtuacao = (Get-ChildItem $raiz -Filter 'Area de Atua*o.xlsx' | Select-Object -First 1).FullName }
if (-not $CenEGestor)    { $CenEGestor    = (Get-ChildItem $raiz -Filter 'CEN e Gestor por Municipio.xlsx' | Select-Object -First 1).FullName }

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t)    { Write-Host "   OK  $t" -ForegroundColor Green }

# A CONEXAO E A DO WINDOWS DE QUEM EXECUTA. Nenhuma senha passa por aqui.
$conexao = "Server=$Servidor,1433;Database=$Banco;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15"

function Escalar([string] $sql, [int] $timeout = 120) {
    $c = New-Object System.Data.SqlClient.SqlConnection $conexao
    $c.Open()
    try {
        $cmd = $c.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $sql
        return $cmd.ExecuteScalar()
    } finally { $c.Dispose() }
}

function Retrato([string] $rotulo) {
    $medidas = [ordered]@{
        'municipios com codigo IBGE'  = 'SELECT COUNT(*) FROM organizacao.Municipio WHERE CodigoIbge IS NOT NULL'
        'municipios da ADR'           = 'SELECT COUNT(*) FROM organizacao.MunicipioDaAreaDeAtuacao WHERE PertenceAAdr = 1 AND EncerradoEm IS NULL'
        'responsaveis vigentes'       = 'SELECT COUNT(*) FROM organizacao.ResponsavelPeloMunicipio WHERE EncerradoEm IS NULL'
        'linhas de producao agricola' = 'SELECT COUNT(*) FROM organizacao.ProducaoAgricolaNoMunicipio'
        'linhas do total do estado'   = 'SELECT COUNT(*) FROM organizacao.ProducaoAgricolaNoEstado'
        'enderecos corrigidos (trilha)' = "SELECT COUNT(*) FROM auditoria.AlteracaoDeCampo WHERE Entidade = 'Endereco' AND Campo = 'MunicipioId' AND ValorNovo LIKE '%IBGE%'"
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
Passo '0. Banco do servidor'
# -------------------------------------------------------------------------------------------------
foreach ($arquivo in $AreaDeAtuacao, $CenEGestor) {
    if (-not $arquivo -or -not (Test-Path $arquivo)) { throw "Nao achei a planilha '$arquivo'. Passe o caminho com -AreaDeAtuacao / -CenEGestor." }
}
Ok 'as duas planilhas estao nesta estacao'

$quem = Escalar 'SELECT SUSER_SNAME()'
if ([int](Escalar "SELECT IS_SRVROLEMEMBER('sysadmin')") -ne 1) {
    throw "A conta $quem entra no SQL Server do servidor mas nao e sysadmin: a copia de seguranca nao pode ser feita. Nada foi gravado."
}
Ok "SQL Server de $Servidor responde; conta $quem com permissao de administrador"

# -------------------------------------------------------------------------------------------------
Passo '1. Copia de seguranca (no proprio servidor)'
# -------------------------------------------------------------------------------------------------
if ($SemCopiaDeSeguranca) {
    Write-Host '   !   copia pulada a pedido (-SemCopiaDeSeguranca)' -ForegroundColor Yellow
} else {
    $pasta = Escalar "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400))"
    $destino = Join-Path $pasta ("{0}-antes-do-territorio-{1:yyyyMMdd-HHmm}.bak" -f $Banco, (Get-Date))
    Escalar "BACKUP DATABASE [$Banco] TO DISK = N'$destino' WITH COPY_ONLY, CHECKSUM, INIT" 1800 | Out-Null
    Escalar "RESTORE VERIFYONLY FROM DISK = N'$destino' WITH CHECKSUM" 1800 | Out-Null
    Ok "copia feita e conferida: $destino"
}

# -------------------------------------------------------------------------------------------------
Passo '2. Antes'
# -------------------------------------------------------------------------------------------------
Retrato 'estado do banco do servidor antes da carga:' | Out-Null

# -------------------------------------------------------------------------------------------------
Passo '3. Carga do territorio'
# -------------------------------------------------------------------------------------------------
$env:ConnectionStrings__Crm = $conexao
try {
    Push-Location $raiz
    & dotnet run --project src\Tracbel.Crm.Carga -c Release -- --somente-territorio --area-de-atuacao $AreaDeAtuacao --cen-e-gestor $CenEGestor
    if ($LASTEXITCODE -ne 0) { throw "A carga parou (codigo $LASTEXITCODE). A etapa em curso foi desfeita pela propria carga; o motivo esta acima." }
} finally {
    Pop-Location
    Remove-Item Env:ConnectionStrings__Crm -ErrorAction SilentlyContinue
}
Ok 'carga concluida'

# -------------------------------------------------------------------------------------------------
Passo '4. Depois'
# -------------------------------------------------------------------------------------------------
$depois = Retrato 'estado do banco do servidor depois da carga:'
if ($depois['municipios da ADR'] -ne 203) {
    throw "Esperava 203 municipios da ADR e o banco tem $($depois['municipios da ADR']). Confira a saida da carga acima."
}
Ok 'territorio carregado: a tela de Indicadores Geograficos ja mostra os mapas no servidor (nao precisa republicar)'
Write-Host ''
