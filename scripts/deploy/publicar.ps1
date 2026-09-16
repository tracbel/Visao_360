<#
  publicar.ps1 - atualiza o CRM no servidor A PARTIR DESTA ESTACAO. Ninguem entra no servidor.

      .\scripts\deploy\publicar.ps1
      .\scripts\deploy\publicar.ps1 -PularTestes                 # so quando os testes acabaram de rodar
      .\scripts\deploy\publicar.ps1 -ReconfigurarSincronizacao   # regrava a configuracao do servico do ART

  =================================================================================================
  DOIS SERVICOS DO WINDOWS, UM COMANDO (documento 35, secao 11):

    TracbelCrmApi               API e telas               C:\aplicacoes\tracbel-crm
    TracbelCrmSincronizacaoArt  ART -> banco central      C:\aplicacoes\tracbel-crm-sincronizacao

  A sincronizacao roda NO SERVIDOR, sozinha, a cada IntervaloMinutos: ninguem precisa rodar carga
  desta estacao. Este script so entrega versao nova dos dois servicos.

  =================================================================================================
  O MESMO MODELO DO USER-ONBOARDING, que ja roda neste servidor (user-onboarding\deploy.ps1):

    1. testa aqui;
    2. gera os pacotes aqui (API self-contained + front; servico de sincronizacao self-contained -
       o servidor so tem os runtimes .NET 6 e 8);
    3. faz a COPIA DE SEGURANCA do banco central (COPY_ONLY, CHECKSUM) antes de qualquer parada -
       a API aplica as migracoes ao subir, e migracao e alteracao do banco central;
    4. para os servicos remotamente, com sc.exe (a sincronizacao primeiro);
    5. copia pelo compartilhamento administrativo C$;
    6. grava a configuracao do servico de sincronizacao, se ainda nao existe, e cria o servico;
    7. sobe a API - e a PROPRIA API aplica as migracoes do banco ao subir (Program.cs);
    8. confere a prova de vida pelo nome DNS, com o certificado de verdade;
    9. sobe a sincronizacao, DEPOIS das migracoes;
   10. confere por hash, arquivo a arquivo, que o servidor ficou identico aos pacotes.

  O deploy so diz "pronto" quando os passos 8 e 10 passam. Data de arquivo copiado nao prova conteudo.

  POR QUE SMB E sc.exe, E NAO WinRM: o C$ e o sc.exe respondem com a credencial que ja esta na
  sessao, sem configurar nada no servidor. CORRECAO de 14/09/2026 (diagnosticar-servidor.ps1): a VM
  E membro do dominio tracbel.com.br - este comentario dizia o contrario. O mecanismo nao muda.

  =================================================================================================
  O QUE ELE NAO TOCA: appsettings.Production.json da API (senha do banco e segredo do Entra ID) e
  ssl\ (certificado). O appsettings.Production.json da sincronizacao so e gravado quando nao existe
  (ou com -ReconfigurarSincronizacao): a cadeia do banco vem do arquivo da API NO SERVIDOR, e as
  credenciais do ART e do Protheus vem do .env desta estacao. Nada disso e impresso, e o arquivo fica
  restrito a administradores e SYSTEM.

  O instalar-no-servidor.ps1 continua existindo para o que so se faz UMA VEZ: instalar o SQL Server,
  restaurar o banco, criar o servico da API, o certificado e a regra de firewall.
#>

[CmdletBinding()]
param(
    [string] $Servidor = '10.150.4.249',
    [string] $NomeDns  = '360-TracbelAgro.tracbel.com.br',
    [int]    $Porta    = 5443,
    [string] $Servico  = 'TracbelCrmApi',
    [string] $Destino  = 'C$\aplicacoes\tracbel-crm',
    [string] $ServicoDeSincronizacao = 'TracbelCrmSincronizacaoArt',
    [string] $DestinoDaSincronizacao = 'C$\aplicacoes\tracbel-crm-sincronizacao',
    [int]    $IntervaloMinutos = 60,
    [switch] $PularTestes,
    [switch] $ReconfigurarSincronizacao
)

$ErrorActionPreference = 'Stop'

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t)    { Write-Host "   OK  $t" -ForegroundColor Green }
function Aviso([string] $t) { Write-Host "   !   $t" -ForegroundColor Yellow }

$raiz       = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$remoto     = "\\$Servidor\$Destino"
$remotoSinc = "\\$Servidor\$DestinoDaSincronizacao"
$localApi   = 'C:\' + $Destino.Substring(3)
$localSinc  = 'C:\' + $DestinoDaSincronizacao.Substring(3)

. (Join-Path $PSScriptRoot '_remoto.ps1')

# O PACOTE MONTA NUMA PASTA TEMPORARIA, e nao em publicacao\. Aquela pasta guarda o pacote de
# INSTALACAO, com o backup do banco; publicar por cima dela misturaria as duas coisas.
$pacote     = Join-Path ([IO.Path]::GetTempPath()) 'tracbel-crm-publicacao'
$pacoteSinc = Join-Path ([IO.Path]::GetTempPath()) 'tracbel-crm-publicacao-sincronizacao'

function EstadoDoServico([string] $nome) {
    $saida = & sc.exe "\\$Servidor" query $nome 2>&1
    if ($LASTEXITCODE -eq 1060) { return 'INEXISTENTE' }
    if ($saida -match 'RUNNING') { return 'RUNNING' }
    if ($saida -match 'STOPPED') { return 'STOPPED' }
    if ($saida -match 'PENDING') { return 'PENDING' }
    return 'DESCONHECIDO'
}

function EsperarEstado([string] $nome, [string] $alvo, [int] $segundos) {
    $limite = (Get-Date).AddSeconds($segundos)
    while ((Get-Date) -lt $limite) {
        if ((EstadoDoServico $nome) -eq $alvo) { return $true }
        Start-Sleep -Seconds 2
    }
    return $false
}

function Ler-Env([string] $Arquivo) {
    if (-not (Test-Path $Arquivo)) { throw "Nao achei $Arquivo." }
    $valores = @{}
    foreach ($linha in Get-Content $Arquivo) {
        if ($linha -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') {
            $valores[$Matches[1]] = $Matches[2].Trim().Trim('"').Trim("'")
        }
    }
    $valores
}

function Conferir-Hash([string] $origem, [string] $destino) {
    $diferentes = @()
    $faltando = @()
    $lista = Get-ChildItem $origem -Recurse -File
    foreach ($a in $lista) {
        $relativo = $a.FullName.Substring($origem.Length).TrimStart('\')
        $noServidor = Join-Path $destino $relativo
        if (-not (Test-Path $noServidor)) { $faltando += $relativo; continue }
        if ((Get-FileHash $noServidor -Algorithm SHA256).Hash -ne (Get-FileHash $a.FullName -Algorithm SHA256).Hash) {
            $diferentes += $relativo
        }
    }
    if ($faltando.Count)   { Write-Host ('   FALTANDO: ' + ($faltando -join ', ')) -ForegroundColor Red }
    if ($diferentes.Count) { Write-Host ('   DIFERENTES: ' + ($diferentes -join ', ')) -ForegroundColor Red }
    if ($faltando.Count -or $diferentes.Count) { throw "O servidor nao ficou identico ao pacote em $destino. Veja a lista acima." }
    return $lista.Count
}

# A cadeia de conexao vem do arquivo da API NO SERVIDOR e e lida pelo proprio servidor: nao passa por
# esta estacao. So o resultado sai; erro de SQL sai so com o numero. ASCII puro: o PowerShell 5.1 do
# servidor le .ps1 como ANSI.
$abrirBancoNoServidor = @'
$cfg = Get-Content '__CONFIG__' -Raw | ConvertFrom-Json
$b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
foreach ($parte in ([string]$cfg.ConnectionStrings.Crm).Split(';')) {
  $kv = $parte.Split('=', 2)
  if ($kv.Count -ne 2) { continue }
  $k = $kv[0].Trim().ToLowerInvariant(); $v = $kv[1].Trim()
  if ($k -in 'server','data source','address','addr') { $b['Data Source'] = $v }
  elseif ($k -in 'database','initial catalog') { $b['Initial Catalog'] = $v }
  elseif ($k -in 'user id','uid','user') { $b['User ID'] = $v }
  elseif ($k -in 'password','pwd') { $b['Password'] = $v }
  elseif ($k -in 'integrated security','trusted_connection') { $b['Integrated Security'] = $v }
}
$cn = New-Object System.Data.SqlClient.SqlConnection $b.ConnectionString
'@

# -------------------------------------------------------------------------------------------------
Passo '0. Alcance'
# -------------------------------------------------------------------------------------------------
if (-not (Test-Path $remoto)) { throw "Nao alcancei $remoto - confira a VPN e a credencial da sessao." }
Ok "servidor alcancavel ($remoto)"
Ok ("servico {0}: {1}" -f $Servico, (EstadoDoServico $Servico))
Ok ("servico {0}: {1}" -f $ServicoDeSincronizacao, (EstadoDoServico $ServicoDeSincronizacao))

# -------------------------------------------------------------------------------------------------
Passo '1. Testes'
# -------------------------------------------------------------------------------------------------
if ($PularTestes) {
    Aviso 'testes pulados a pedido'
} else {
    Push-Location $raiz
    try {
        & dotnet test --nologo -v q
        if ($LASTEXITCODE -ne 0) { throw 'Os testes falharam. Nada foi enviado ao servidor.' }
    } finally { Pop-Location }
    Ok 'testes passando'
}

# -------------------------------------------------------------------------------------------------
Passo '2. Pacotes'
# -------------------------------------------------------------------------------------------------
foreach ($p in $pacote, $pacoteSinc) { if (Test-Path $p) { Remove-Item $p -Recurse -Force } }

& dotnet publish (Join-Path $raiz 'src\Tracbel.Crm.Api\Tracbel.Crm.Api.csproj') `
    -c Release -r win-x64 --self-contained true -o $pacote --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'O dotnet publish da API falhou. Nada foi enviado ao servidor.' }

Push-Location (Join-Path $raiz 'src\Tracbel.Crm.Web')
try {
    & npm run build
    if ($LASTEXITCODE -ne 0) { throw 'O build do front falhou. Nada foi enviado ao servidor.' }
} finally { Pop-Location }

Copy-Item (Join-Path $raiz 'src\Tracbel.Crm.Web\dist') (Join-Path $pacote 'wwwroot') -Recurse -Force

& dotnet publish (Join-Path $raiz 'src\Tracbel.Crm.Carga\Tracbel.Crm.Carga.csproj') `
    -c Release -r win-x64 --self-contained true -o $pacoteSinc --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'O dotnet publish da sincronizacao falhou. Nada foi enviado ao servidor.' }

$arquivos = Get-ChildItem $pacote -Recurse -File
$arquivosSinc = Get-ChildItem $pacoteSinc -Recurse -File
Ok ("API: {0} arquivos, {1} MB" -f $arquivos.Count, [math]::Round(($arquivos | Measure-Object Length -Sum).Sum / 1MB, 1))
Ok ("sincronizacao: {0} arquivos, {1} MB" -f $arquivosSinc.Count, [math]::Round(($arquivosSinc | Measure-Object Length -Sum).Sum / 1MB, 1))

# -------------------------------------------------------------------------------------------------
Passo '3. Copia de seguranca do banco central'
# -------------------------------------------------------------------------------------------------
# ANTES DE PARAR QUALQUER COISA. Se a copia nao se confirmar, nada foi parado nem copiado.
# A QUEBRA DE LINHA ENTRE OS DOIS BLOCOS E EXPLICITA: o here-string nao guarda a ultima quebra, e sem ela a
# linha do New-Object colava no "try {" do bloco seguinte (falha de 14/09/2026, antes de qualquer parada).
$scriptDoBackup = ($abrirBancoNoServidor -replace '__CONFIG__', "$localApi\appsettings.Production.json") + "`r`n" + @'
try {
  $cn.Open()
  $banco = $cn.Database
  $cmd = $cn.CreateCommand(); $cmd.CommandTimeout = 3600
  $cmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))"
  $dir = [string]$cmd.ExecuteScalar()
  $arquivo = Join-Path $dir ('{0}-antes-da-publicacao-{1}.bak' -f $banco, (Get-Date -Format 'yyyyMMdd-HHmmss'))
  $cmd.CommandText = "BACKUP DATABASE [$banco] TO DISK = @arq WITH COPY_ONLY, CHECKSUM, INIT"
  $cmd.Parameters.AddWithValue('@arq', $arquivo) | Out-Null
  $cmd.ExecuteNonQuery() | Out-Null
  $verificado = 'sim'
  try { $cmd.CommandText = 'RESTORE VERIFYONLY FROM DISK = @arq WITH CHECKSUM'; $cmd.ExecuteNonQuery() | Out-Null }
  catch { $verificado = 'nao (sem permissao para VERIFYONLY; o BACKUP WITH CHECKSUM conferiu as paginas)' }
  $mb = [math]::Round((Get-Item $arquivo).Length / 1MB, 1)
  Write-Output ('BACKUP_OK|{0}|{1}|{2}|{3}' -f $banco, $arquivo, $mb, $verificado)
} catch [System.Data.SqlClient.SqlException] {
  Write-Output ('BACKUP_FALHOU|erro SQL {0}' -f $_.Exception.Number)
} finally { $cn.Close() }
'@
$saidaDoBackup = Invoke-NoServidor -Script $scriptDoBackup -Nome 'crm-backup' -TimeoutSegundos 3600
$linhaDoBackup = ($saidaDoBackup -split "`r?`n") | Where-Object { $_ -like 'BACKUP_OK|*' } | Select-Object -First 1
if (-not $linhaDoBackup) {
    throw ("A copia de seguranca do banco central nao se confirmou. Nada foi parado nem copiado. Saida: " + $saidaDoBackup)
}
$partes = $linhaDoBackup.Split('|')
Ok ("banco {0}: {1} ({2} MB), verificado: {3}" -f $partes[1], $partes[2], $partes[3], $partes[4])

# -------------------------------------------------------------------------------------------------
Passo '4. Parando os servicos'
# -------------------------------------------------------------------------------------------------
# PARAR ANTES DE COPIAR, e nao copiar com eles no ar: o executavel e as DLLs ficam presos enquanto o
# processo roda, e uma copia parcial deixaria o servidor com versao misturada. A sincronizacao para
# primeiro: um ciclo interrompido desfaz a propria transacao e fica registrado como falha.
if ((EstadoDoServico $ServicoDeSincronizacao) -notin 'INEXISTENTE', 'STOPPED') {
    & sc.exe "\\$Servidor" stop $ServicoDeSincronizacao | Out-Null
    if (-not (EsperarEstado $ServicoDeSincronizacao 'STOPPED' 120)) { throw 'A sincronizacao nao parou em 120 s. Nada foi copiado.' }
    Ok 'sincronizacao parada'
}
& sc.exe "\\$Servidor" stop $Servico | Out-Null
if (-not (EsperarEstado $Servico 'STOPPED' 60)) { throw 'O servico da API nao parou em 60 s. Nada foi copiado.' }
Start-Sleep -Seconds 3
Ok 'API parada'

# -------------------------------------------------------------------------------------------------
Passo '5. Copiando'
# -------------------------------------------------------------------------------------------------
& robocopy $pacote $remoto /E /XF appsettings.Production.json /XD ssl logs /NFL /NDL /NJH /NJS /NP /R:3 /W:5 /MT:8 | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw ("A copia da API falhou (robocopy {0}). Os servicos ficaram PARADOS de proposito: subir com metade dos " +
           "arquivos novos rodaria versao misturada. Resolva e rode de novo." -f $LASTEXITCODE)
}
Ok 'API copiada (configuracao e certificado de producao preservados)'

New-Item -ItemType Directory -Force -Path $remotoSinc | Out-Null
& robocopy $pacoteSinc $remotoSinc /E /XF appsettings.Production.json /XD logs /NFL /NDL /NJH /NJS /NP /R:3 /W:5 /MT:8 | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw ("A copia da sincronizacao falhou (robocopy {0}). Os servicos ficaram PARADOS. Resolva e rode de novo." -f $LASTEXITCODE)
}
Ok 'sincronizacao copiada (configuracao de producao preservada)'

# -------------------------------------------------------------------------------------------------
Passo '6. Servico de sincronizacao: configuracao e registro no Windows'
# -------------------------------------------------------------------------------------------------
$configDaSincronizacao = Join-Path $remotoSinc 'appsettings.Production.json'
if ($ReconfigurarSincronizacao -or -not (Test-Path $configDaSincronizacao)) {
    $fontes = Ler-Env (Join-Path $raiz '.env')
    foreach ($chave in 'ART_DB_SERVER', 'ART_DB_DATABASE', 'ART_DB_USER', 'ART_DB_PASSWORD', 'ART_VIEW') {
        if (-not $fontes[$chave]) { throw ".env nao tem $chave. A configuracao da sincronizacao nao foi gravada." }
    }

    $daApi = Get-Content (Join-Path $remoto 'appsettings.Production.json') -Raw | ConvertFrom-Json
    if (-not $daApi.ConnectionStrings.Crm) { throw 'O appsettings.Production.json da API no servidor nao tem ConnectionStrings:Crm.' }

    $config = [ordered]@{
        ConnectionStrings = [ordered]@{ Crm = [string]$daApi.ConnectionStrings.Crm }
        Art = [ordered]@{
            Servidor = $fontes['ART_DB_SERVER']
            Porta    = [int]$(if ($fontes['ART_DB_PORT']) { $fontes['ART_DB_PORT'] } else { '3306' })
            Banco    = $fontes['ART_DB_DATABASE']
            Usuario  = $fontes['ART_DB_USER']
            Senha    = $fontes['ART_DB_PASSWORD']
            Visao    = $fontes['ART_VIEW']
        }
        Sincronizacao = [ordered]@{
            Art = [ordered]@{
                Habilitada = $true
                IntervaloMinutos = $IntervaloMinutos
                EsperaInicialSegundos = 60
                Tentativas = 3
                EsperaEntreTentativasSegundos = 30
            }
        }
        Logging = [ordered]@{ LogLevel = [ordered]@{ Default = 'Warning'; Tracbel = 'Information' } }
    }

    if ($fontes['TOTVS_DB_SERVER'] -and $fontes['TOTVS_DB_USER'] -and $fontes['TOTVS_DB_PASSWORD']) {
        $config['ProtheusBanco'] = [ordered]@{
            Servidor = $(if ($fontes['TOTVS_DB_PORT']) { "$($fontes['TOTVS_DB_SERVER']),$($fontes['TOTVS_DB_PORT'])" } else { $fontes['TOTVS_DB_SERVER'] })
            Banco    = $fontes['TOTVS_DB_DATABASE']
            Usuario  = $fontes['TOTVS_DB_USER']
            Senha    = $fontes['TOTVS_DB_PASSWORD']
        }
    } else {
        Aviso '.env sem TOTVS_DB_*: a sincronizacao roda sem a conferencia de dono no Protheus'
    }

    # O ARQUIVO NASCE RESTRITO e so depois recebe o conteudo: nao ha instante em que as credenciais
    # fiquem legiveis pela permissao herdada da pasta.
    if (-not (Test-Path $configDaSincronizacao)) { New-Item -ItemType File -Path $configDaSincronizacao | Out-Null }
    $acl = Get-Acl $configDaSincronizacao
    $acl.SetAccessRuleProtection($true, $false)
    foreach ($regra in @($acl.Access)) { $acl.RemoveAccessRule($regra) | Out-Null }
    # PELO SID, e nao pelo nome: numa estacao em portugues "BUILTIN\Administrators" nao se traduz (e
    # "Administradores"), e a publicacao de 14/09/2026 parou aqui, com a API ja parada.
    $administradores = New-Object Security.Principal.SecurityIdentifier('S-1-5-32-544')
    $sistema = New-Object Security.Principal.SecurityIdentifier('S-1-5-18')
    $acl.AddAccessRule((New-Object Security.AccessControl.FileSystemAccessRule($administradores, 'FullControl', 'Allow')))
    $acl.AddAccessRule((New-Object Security.AccessControl.FileSystemAccessRule($sistema, 'FullControl', 'Allow')))
    Set-Acl $configDaSincronizacao $acl
    [IO.File]::WriteAllText($configDaSincronizacao, ($config | ConvertTo-Json -Depth 6), (New-Object Text.UTF8Encoding($false)))
    Ok 'configuracao da sincronizacao gravada (credenciais nao exibidas; so administradores e SYSTEM leem)'
} else {
    Ok 'configuracao da sincronizacao ja existe no servidor e foi preservada'
}

if ((EstadoDoServico $ServicoDeSincronizacao) -eq 'INEXISTENTE') {
    # O caminho nao tem espaco, e o argumento vai junto: binPath inteiro entre aspas.
    $argumentos = "\\$Servidor create $ServicoDeSincronizacao binPath= `"$localSinc\Tracbel.Crm.Carga.exe --servico-art`" " +
                  "start= delayed-auto depend= MSSQLSERVER DisplayName= `"Tracbel CRM Agro - Sincronizacao do ART`""
    $criacao = Start-Process -FilePath sc.exe -ArgumentList $argumentos -Wait -NoNewWindow -PassThru
    if ($criacao.ExitCode -ne 0) { throw "Nao consegui criar o servico $ServicoDeSincronizacao (sc.exe $($criacao.ExitCode))." }

    & sc.exe "\\$Servidor" description $ServicoDeSincronizacao 'Le as vendas de maquina do ART (somente leitura) e grava no banco central do CRM. Documento 35, secao 11.' | Out-Null
    # Se o processo cair, o Windows o sobe de novo: 1, 2 e 5 minutos; a contagem zera em um dia.
    & sc.exe "\\$Servidor" failure $ServicoDeSincronizacao reset= 86400 actions= restart/60000/restart/120000/restart/300000 | Out-Null
    & sc.exe "\\$Servidor" failureflag $ServicoDeSincronizacao 1 | Out-Null
    Ok "servico $ServicoDeSincronizacao criado: inicio automatico (atrasado), depende do MSSQLSERVER, reinicia se cair"
} else {
    Ok "servico $ServicoDeSincronizacao ja registrado"
}

# -------------------------------------------------------------------------------------------------
Passo '7. Subindo a API'
# -------------------------------------------------------------------------------------------------
& sc.exe "\\$Servidor" start $Servico | Out-Null

# 3 minutos, e nao 30 segundos: se houver migracao pendente, ela roda antes de a API atender.
if (-not (EsperarEstado $Servico 'RUNNING' 180)) {
    throw ("O servico da API nao chegou a RUNNING. Se houve migracao, ela pode ter falhado - a API nao sobe " +
           "com o banco pela metade. Veja o Log de Aplicativo do Windows no servidor. A sincronizacao ficou parada.")
}
Ok 'API RUNNING'

# -------------------------------------------------------------------------------------------------
Passo '8. Prova de vida'
# -------------------------------------------------------------------------------------------------
# PELO NOME DNS, com o certificado validado. Conferir por IP pularia justamente a parte que o
# usuario usa: o certificado cobre o NOME, e o login do Entra ID so aceita o endereco cadastrado.
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$saude = $null
$limite = (Get-Date).AddSeconds(90)
while ((Get-Date) -lt $limite -and -not $saude) {
    try { $saude = Invoke-RestMethod -Uri "https://$NomeDns`:$Porta/saude/banco" -TimeoutSec 15 }
    catch { Start-Sleep -Seconds 3 }
}
if (-not $saude) { throw "https://$NomeDns`:$Porta/saude/banco nao respondeu em 90 s. A sincronizacao ficou parada." }
if (-not $saude.conectado) { throw 'A API subiu, mas nao alcanca o banco. A sincronizacao ficou parada.' }
if (@($saude.migracoesPendentes).Count -gt 0) {
    throw ('Ficaram migracoes pendentes: ' + (@($saude.migracoesPendentes) -join ', ') + '. A sincronizacao ficou parada.')
}
Ok 'API no ar, banco conectado, nenhuma migracao pendente'

# -------------------------------------------------------------------------------------------------
Passo '9. Subindo a sincronizacao'
# -------------------------------------------------------------------------------------------------
# DESABILITADA FICA PARADA. Desde 15/09/2026 a sincronizacao esta desligada de proposito (banco limpo para
# reestruturar as tabelas, documento 35, secao 13): publicar versao nova nao pode religa-la sozinha.
if ((& sc.exe "\\$Servidor" qc $ServicoDeSincronizacao 2>&1) -match 'DISABLED') {
    Aviso ("sincronizacao $ServicoDeSincronizacao DESABILITADA: continua parada. Para religar: " +
           "sc.exe \\$Servidor config $ServicoDeSincronizacao start= delayed-auto  e depois  sc.exe \\$Servidor start $ServicoDeSincronizacao")
} else {
    & sc.exe "\\$Servidor" start $ServicoDeSincronizacao | Out-Null
    if (-not (EsperarEstado $ServicoDeSincronizacao 'RUNNING' 90)) {
        throw ("A sincronizacao nao chegou a RUNNING. Veja o Log de Aplicativo do servidor, origem " +
               "$ServicoDeSincronizacao - a mensagem diz qual configuracao falta.")
    }
    Ok ("sincronizacao RUNNING - primeiro ciclo em cerca de 1 minuto, depois a cada {0} min" -f $IntervaloMinutos)
}

# -------------------------------------------------------------------------------------------------
Passo '10. Conferencia por hash'
# -------------------------------------------------------------------------------------------------
Ok ("API: {0} arquivos identicos ao pacote" -f (Conferir-Hash $pacote $remoto))
Ok ("sincronizacao: {0} arquivos identicos ao pacote" -f (Conferir-Hash $pacoteSinc $remotoSinc))

Write-Host ''
Write-Host "Pronto: https://$NomeDns`:$Porta" -ForegroundColor Green
Write-Host 'Para conferir a sincronizacao no banco central: .\scripts\deploy\verificar-sincronizacao.ps1' -ForegroundColor Green
Write-Host ''
