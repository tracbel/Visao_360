<#
  instalar-no-servidor.ps1 - instala o CRM Tracbel Agro no servidor de aplicacao.

  =================================================================================================
  EXECUTAR NO PROPRIO SERVIDOR (10.150.4.249), COMO ADMINISTRADOR.

      cd C:\projetos\tracbel-crm-deploy
      powershell -NoProfile -ExecutionPolicy Bypass -File .\instalar-no-servidor.ps1

  =================================================================================================
  O QUE ELE INSTALA, E POR QUE ASSIM.

  BANCO NATIVO, E NAO EM CONTAINER. A primeira versao deste script subia o SQL Server em Docker,
  como na estacao de desenvolvimento. Medido no servidor: o servico `docker` (Docker Engine) esta
  rodando, mas Docker Engine no Windows executa container WINDOWS, e a imagem do SQL Server e
  Linux. Quem da container Linux e o Docker Desktop, cujo servico (`com.docker.service`) esta
  parado - e Docker Desktop EXIGE usuario logado. Uma tela que a diretoria abre nao pode depender
  de alguem manter uma sessao RDP aberta para sempre.

  EXPRESS, E NAO DEVELOPER. A edicao Developer e gratuita apenas para desenvolvimento e teste; no
  momento em que esta instancia passa a servir a diretoria, o uso e produtivo e a licenca nao
  cobre. Express cobre uso produtivo e e gratuita. Os limites dela sao 10 GB por banco, 1,4 GB de
  cache e 4 nucleos - medido, o banco tem 1,5 GB de arquivo e menos de 400 MB de dado real
  (as maiores tabelas sao Interacao com 52 MB e Tarefa com 47 MB), entao cabe com folga.

  UM SERVICO SO PARA A APLICACAO, e nao IIS na frente da API. Separar front e API em duas portas
  exigiria CORS na API (que ela nao publica de proposito), um segundo certificado e o modulo de
  proxy do IIS (que nao esta instalado nesta maquina). A API serve o front do proprio wwwroot.

  A API VAI SELF-CONTAINED. Este servidor tem .NET 6 e 8; o projeto e .NET 9. Self-contained leva
  o runtime dentro do pacote e evita instalar runtime novo numa maquina que ja hospeda o
  user-onboarding em producao.

  =================================================================================================
  O QUE ELE NAO FAZ: nao mexe em nada do user-onboarding, nao altera o IIS, nao toca no Docker.
#>

[CmdletBinding()]
param(
    [string] $Raiz        = 'C:\projetos\tracbel-crm-deploy',
    [string] $Destino     = 'C:\aplicacoes\tracbel-crm',
    [int]    $PortaApi    = 5443,
    [string] $NomeServico = 'TracbelCrmApi',
    [string] $Colacao     = 'Latin1_General_CI_AI',
    [int]    $TetoDeMemoriaMB = 1400,

    # RESTAURAR POR CIMA E DESTRUTIVO, ENTAO NAO E O PADRAO.
    #
    # Este script e feito para ser reexecutado - atualizar a aplicacao, refazer o servico, trocar o
    # certificado. Se a restauracao rodasse toda vez, uma reexecucao de rotina apagaria o que o
    # banco do servidor tivesse de mais novo que o .bak. Existindo banco, ele e mantido e o script
    # diz o que achou; para substituir de proposito, passe -RestaurarBanco.
    [switch] $RestaurarBanco,

    # QUEM PODE ALCANCAR A PORTA. Enquanto a API nao autenticar ninguem, esta e a UNICA barreira
    # entre a base comercial da empresa e qualquer maquina da rede.
    #
    # A identidade da API vem de cabecalho HTTP (X-Tracbel-Usuario / X-Tracbel-Empresa), o que nao
    # autentica coisa alguma: quem abre o endereco ve faturamento, carteira e cliente de todas as
    # dezesseis filiais. Vazio abre para a rede inteira - e o script avisa alto quando for o caso.
    #
    # Exemplo: -RedesPermitidas '10.150.0.0/16','10.212.0.0/16'
    [string[]] $RedesPermitidas = @(),

    # Se a maquina nao tiver saida para a internet, baixe o instalador a mao e aponte aqui.
    [string] $InstaladorSql = ''
)

$ErrorActionPreference = 'Stop'

function Passo([string] $t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Ok([string] $t)    { Write-Host "   OK  $t" -ForegroundColor Green }
function Aviso([string] $t) { Write-Host "   !   $t" -ForegroundColor Yellow }

# -------------------------------------------------------------------------------------------------
# 0. Pre-requisitos. Falhar aqui e barato; falhar na metade nao e.
# -------------------------------------------------------------------------------------------------
Passo '0. Conferindo pre-requisitos'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Este script precisa rodar como Administrador.'
}
Ok 'sessao com privilegio de administrador'

if (-not (Test-Path (Join-Path $Raiz 'publicacao\Tracbel.Crm.Api.exe'))) {
    throw "Nao achei a publicacao em $Raiz\publicacao. Copie o pacote antes de rodar."
}
$bak = Join-Path $Raiz 'publicacao\_banco\TracbelCrm.bak'
if (-not (Test-Path $bak)) { throw "Nao achei o backup do banco em $bak." }
Ok ("pacote e backup presentes ({0} MB de backup)" -f [math]::Round((Get-Item $bak).Length/1MB,1))

# O NOSSO SERVICO PARA ANTES DA CHECAGEM DE PORTA.
#
# Reinstalar sobre uma instalacao que ja roda e o caso NORMAL, nao a excecao. Conferir a porta com
# o proprio servico no ar acusa conflito consigo mesmo e aborta uma reexecucao legitima - foi o que
# aconteceu na terceira tentativa. Este servidor hospeda dezenas de aplicacoes (nginx, MySQL,
# PostgreSQL, Node, Python, Zabbix), entao a checagem de porta continua valendo; ela so precisa
# olhar para o mundo depois que a nossa parte saiu dele.
if (Get-Service -Name $NomeServico -ErrorAction SilentlyContinue) {
    Aviso "servico $NomeServico ja existe - parando para reinstalar"
    Stop-Service -Name $NomeServico -Force -ErrorAction SilentlyContinue
    & sc.exe delete $NomeServico | Out-Null
    Start-Sleep -Seconds 3
}

$ocupada = Get-NetTCPConnection -LocalPort $PortaApi -State Listen -ErrorAction SilentlyContinue
if ($ocupada) {
    $dono = (Get-Process -Id $ocupada[0].OwningProcess -ErrorAction SilentlyContinue).ProcessName
    throw "A porta $PortaApi esta em uso por '$dono'. Rode de novo com -PortaApi <outra>."
}
Ok "porta $PortaApi livre"

$disco = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'"
$livreGB = [math]::Round($disco.FreeSpace/1GB, 1)
if ($livreGB -lt 12) { throw "So ha $livreGB GB livres em C:. O SQL Server precisa de ~8 GB para instalar." }
Ok "$livreGB GB livres em C:"

# -------------------------------------------------------------------------------------------------
# 1. O SQL Server.
# -------------------------------------------------------------------------------------------------
Passo '1. SQL Server Express'

$instanciaJaExiste = $null -ne (Get-Service -Name 'MSSQLSERVER' -ErrorAction SilentlyContinue)

if ($instanciaJaExiste) {
    Ok 'ja existe uma instancia padrao (MSSQLSERVER) - vou usar essa, sem reinstalar'
    $senhaSa = $null
} else {
    # A REGRA E DITA ANTES, E A TENTATIVA SE REPETE. Recusar depois de digitar e mandar rodar o
    # script inteiro de novo desperdicia os minutos de conferencia que acabaram de passar - e a
    # exigencia nao e capricho: o proprio instalador do SQL Server rejeita senha fraca, so que com
    # uma mensagem que aparece vinte minutos depois, no meio da instalacao.
    Write-Host ''
    Write-Host '   A senha do sa precisa de:' -ForegroundColor Yellow
    Write-Host '     - 12 caracteres ou mais'
    Write-Host '     - tres destes quatro: maiuscula, minuscula, numero, simbolo'
    Write-Host '     - nao pode conter o nome da conta nem do servidor'
    Write-Host ''

    $senhaSa = $null
    foreach ($tentativa in 1..3) {
        $senha = Read-Host "   Senha do sa (tentativa $tentativa de 3, nao aparece na tela)" -AsSecureString
        $texto = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [Runtime.InteropServices.Marshal]::SecureStringToBSTR($senha))

        $categorias = @(
            ($texto -cmatch '[A-Z]'),
            ($texto -cmatch '[a-z]'),
            ($texto -match '[0-9]'),
            ($texto -match '[^a-zA-Z0-9]')
        ) | Where-Object { $_ }

        if ($texto.Length -lt 12) {
            Aviso "tem $($texto.Length) caracteres; precisa de 12."
        } elseif ($categorias.Count -lt 3) {
            Aviso "usa $($categorias.Count) tipo(s) de caractere; precisa de 3."
        } elseif ($texto -match [regex]::Escape($env:COMPUTERNAME) -or $texto -match '(?i)\bsa\b') {
            Aviso 'nao pode conter o nome do servidor nem "sa".'
        } else {
            $senhaSa = $texto
            break
        }
    }

    if (-not $senhaSa) { throw 'Tres tentativas sem uma senha valida. Rode o script de novo.' }
    Ok 'senha aceita'

    $pastaSql = Join-Path $Raiz '_sqlserver'
    New-Item -ItemType Directory -Force -Path $pastaSql | Out-Null

    # O INSTALADOR DE VERDADE E O SQLEXPR_x64_ENU.exe. O SQL20xx-SSEI-Expr.exe e so um baixador e
    # NAO repassa os parametros de linha de comando adiante - rodar ele com /ACTION abre a tela
    # grafica e o script fica pendurado esperando alguem clicar.
    $midia = if ($InstaladorSql) { $InstaladorSql } else {
        Get-ChildItem $pastaSql -Filter 'SQLEXPR*_x64_ENU.exe' -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
    }

    if (-not $midia -or -not (Test-Path $midia)) {
        $bootstrapper = Join-Path $pastaSql 'SQL2025-SSEI-Expr.exe'

        if (-not (Test-Path $bootstrapper)) {
            Write-Host '   baixando o instalador da Microsoft...'
            try {
                Invoke-WebRequest -UseBasicParsing -TimeoutSec 600 `
                    -Uri 'https://download.microsoft.com/download/7ab8f535-7eb8-4b16-82eb-eca0fa2d38f3/SQL2025-SSEI-Expr.exe' `
                    -OutFile $bootstrapper
            } catch {
                throw @"
Nao consegui baixar o instalador ($($_.Exception.Message)).

Baixe a mao em https://www.microsoft.com/en-us/sql-server/sql-server-downloads (edicao Express),
coloque o arquivo em $pastaSql e rode de novo. Se voce ja tiver o SQLEXPR_x64_ENU.exe, aponte:
    .\instalar-no-servidor.ps1 -InstaladorSql C:\caminho\SQLEXPR_x64_ENU.exe
"@
            }
        }

        # NAO SE EXECUTA BINARIO BAIXADO SEM CONFERIR QUEM ASSINOU. Se a assinatura nao for valida
        # e da Microsoft, para aqui - o download pode ter vindo de um caminho comprometido.
        $assinatura = Get-AuthenticodeSignature $bootstrapper
        if ($assinatura.Status -ne 'Valid' -or $assinatura.SignerCertificate.Subject -notmatch 'Microsoft') {
            Remove-Item $bootstrapper -Force
            throw "O instalador baixado NAO tem assinatura valida da Microsoft (status: $($assinatura.Status)). Arquivo removido."
        }
        Ok 'instalador baixado e assinatura da Microsoft conferida'

        Write-Host '   extraindo a midia de instalacao (alguns minutos)...'
        & $bootstrapper /ACTION=Download /MEDIAPATH="$pastaSql" /MEDIATYPE=Core /QUIET | Out-Null

        $midia = Get-ChildItem $pastaSql -Filter 'SQLEXPR*_x64_ENU.exe' -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName

        if (-not $midia) {
            throw @"
O baixador rodou mas nao produziu o SQLEXPR_x64_ENU.exe em $pastaSql.

Rode o baixador a mao (ele abre uma tela), escolha "Baixar Midia" / "Download Media", salve em
$pastaSql e execute este script de novo.
"@
        }
    }
    Ok "midia de instalacao: $(Split-Path $midia -Leaf)"

    $extraido = Join-Path $pastaSql 'setup'
    if (-not (Test-Path (Join-Path $extraido 'setup.exe'))) {
        Write-Host '   descompactando...'
        & $midia /Q /X:"$extraido" | Out-Null
        Start-Sleep -Seconds 5
    }
    if (-not (Test-Path (Join-Path $extraido 'setup.exe'))) { throw "Nao achei o setup.exe em $extraido." }
    Ok 'midia descompactada'

    # A COLACAO TEM DE SER A MESMA DO BANCO. O banco restaurado carrega a propria colacao, mas a
    # tempdb usa a do SERVIDOR - e uma juncao entre tabela real e tabela temporaria com colacoes
    # diferentes falha em tempo de execucao, numa consulta especifica, muito depois da instalacao.
    Write-Host '   instalando o SQL Server (10 a 20 minutos)...'
    # TODO VALOR VAI ENTRE ASPAS, mesmo os que parecem nao precisar.
    #
    # `Start-Process -ArgumentList` junta os itens com espaco e NAO acrescenta aspas. Sem elas,
    # `/SQLSVCACCOUNT=NT AUTHORITY\NETWORK SERVICE` chega ao instalador partido em tres tokens: ele
    # le a conta como "NT" e devolve "Value cannot be null. Parameter name: userName" - vinte
    # minutos depois de comecar, e sem dizer qual parametro estragou. A senha tambem vai entre
    # aspas, porque simbolo e o que a propria regra de complexidade incentiva a usar.
    $argumentos = @(
        '/Q', '/IACCEPTSQLSERVERLICENSETERMS', '/ACTION=Install',
        '/FEATURES=SQLEngine',
        '/INSTANCENAME="MSSQLSERVER"',
        '/SQLSVCACCOUNT="NT AUTHORITY\NETWORK SERVICE"',
        '/SQLSVCSTARTUPTYPE="Automatic"',
        '/SQLSYSADMINACCOUNTS="BUILTIN\Administrators"',
        '/SECURITYMODE="SQL"', "/SAPWD=`"$senhaSa`"",
        "/SQLCOLLATION=`"$Colacao`"",
        '/TCPENABLED=1', '/NPENABLED=0',
        '/BROWSERSVCSTARTUPTYPE="Disabled"'
    )
    $p = Start-Process -FilePath (Join-Path $extraido 'setup.exe') -ArgumentList $argumentos -Wait -PassThru -NoNewWindow

    # 3010 = instalou e pede reinicializacao. Nao e falha.
    if ($p.ExitCode -notin 0, 3010) {
        # O LOG E MOSTRADO AQUI, e nao apontado. O caminho tem a versao no meio e muda a cada
        # lancamento; mandar alguem procurar "C:\...\<versao>\..." custa uma ida e volta inteira
        # para descobrir uma linha que este script ja podia ter lido.
        $resumo = Get-ChildItem 'C:\Program Files\Microsoft SQL Server' -Recurse -Filter 'Summary.txt' `
            -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1

        if ($resumo) {
            Write-Host ''
            Write-Host "   --- ultimas linhas de $($resumo.FullName) ---" -ForegroundColor Yellow
            Get-Content $resumo.FullName -ErrorAction SilentlyContinue |
                Select-String -Pattern 'Exit code|Error|Detail|Feature:|Status:' |
                Select-Object -Last 15 | ForEach-Object { "   $($_.Line.Trim())" }
            Write-Host ''
        }

        throw "A instalacao do SQL Server falhou com codigo $($p.ExitCode)."
    }
    if ($p.ExitCode -eq 3010) { Aviso 'o instalador pede reinicializacao do servidor quando for possivel' }
    Ok 'SQL Server instalado'

    Start-Service -Name 'MSSQLSERVER' -ErrorAction SilentlyContinue
}

Write-Host '   aguardando o banco aceitar conexao...' -NoNewline
$conexaoAdmin = 'Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5'
$pronto = $false
foreach ($i in 1..36) {
    Start-Sleep -Seconds 5
    $c = New-Object System.Data.SqlClient.SqlConnection $conexaoAdmin
    try { $c.Open(); $c.Close(); $pronto = $true; break } catch { Write-Host '.' -NoNewline }
}
Write-Host ''
if (-not $pronto) { throw 'O SQL Server nao aceitou conexao em 3 minutos.' }
Ok 'banco aceitando conexao'

function Sql([string] $comando, [int] $timeout = 3600) {
    $c = New-Object System.Data.SqlClient.SqlConnection 'Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True'
    $c.Open()
    try {
        $cmd = $c.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $comando
        return $cmd.ExecuteScalar()
    } finally { $c.Close() }
}
function SqlTabela([string] $comando) {
    $c = New-Object System.Data.SqlClient.SqlConnection 'Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True'
    $c.Open()
    try {
        $cmd = $c.CreateCommand(); $cmd.CommandTimeout = 600; $cmd.CommandText = $comando
        $t = New-Object System.Data.DataTable
        $t.Load($cmd.ExecuteReader())
        return $t
    } finally { $c.Close() }
}

Sql "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'max server memory (MB)', $TetoDeMemoriaMB; RECONFIGURE;" | Out-Null
Ok "teto de memoria do SQL Server em $TetoDeMemoriaMB MB"

# -------------------------------------------------------------------------------------------------
# 2. Restaurar o banco.
# -------------------------------------------------------------------------------------------------
Passo '2. O banco'

$bancoJaExiste = [int](Sql "SELECT CASE WHEN DB_ID('TracbelCrm') IS NULL THEN 0 ELSE 1 END") -eq 1

if ($bancoJaExiste -and -not $RestaurarBanco) {
    $jaTem  = Sql "SELECT COUNT(*) FROM [TracbelCrm].comercial.FaturamentoDoCliente WHERE ExcluidoEm IS NULL"
    $jaVale = Sql "SELECT CAST(ISNULL(SUM(ValorLiquido),0)/1000000 AS decimal(12,1)) FROM [TracbelCrm].comercial.FaturamentoDoCliente WHERE ExcluidoEm IS NULL"
    $ate    = Sql "SELECT CONVERT(varchar(10), MAX(Competencia), 103) FROM [TracbelCrm].comercial.FaturamentoDoCliente WHERE ExcluidoEm IS NULL"
    Ok "banco JA EXISTE - mantido ($jaTem meses, R$ $jaVale milhoes, ate $ate)"
    Aviso 'para substituir pelo backup do pacote, rode de novo com -RestaurarBanco'
} else {
    if ($bancoJaExiste) { Aviso 'o banco existente SERA SUBSTITUIDO pelo backup do pacote (-RestaurarBanco)' }

    # OS NOMES LOGICOS SAIEM DO PROPRIO BACKUP, e nao de um palpite. O backup foi feito num
    # container Linux, onde os caminhos sao /var/opt/mssql/data; aqui sao outros, e sem o MOVE a
    # restauracao falha. Ler o FILELISTONLY faz o script continuar valendo se os nomes mudarem.
    $arquivos = SqlTabela "RESTORE FILELISTONLY FROM DISK=N'$bak'"
    $dados = ($arquivos | Where-Object { $_.Type -eq 'D' } | Select-Object -First 1).LogicalName
    $log   = ($arquivos | Where-Object { $_.Type -eq 'L' } | Select-Object -First 1).LogicalName
    if (-not $dados -or -not $log) { throw 'Nao consegui ler os nomes logicos de dentro do backup.' }
    Ok "backup legivel (dados: $dados, log: $log)"

    $pastaDados = (Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(400))")
    if (-not $pastaDados) { throw 'Nao consegui descobrir a pasta de dados da instancia.' }

    Sql @"
IF DB_ID('TracbelCrm') IS NOT NULL
    ALTER DATABASE [TracbelCrm] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [TracbelCrm] FROM DISK=N'$bak'
WITH MOVE N'$dados' TO N'$pastaDados`TracbelCrm.mdf',
     MOVE N'$log'   TO N'$pastaDados`TracbelCrm_log.ldf',
     REPLACE, RECOVERY;

ALTER DATABASE [TracbelCrm] SET MULTI_USER;
"@ | Out-Null
    Ok 'banco restaurado'
}

# O USUARIO DA APLICACAO NAO E O `sa`. A API entra com conta propria, que so alcanca este banco.
$senhaApp = -join ((48..57) + (65..90) + (97..122) + (35,37,42,45,95) | Get-Random -Count 24 | ForEach-Object { [char]$_ })

Sql @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name='TracbelCrm')
    CREATE LOGIN [TracbelCrm] WITH PASSWORD = '$senhaApp', CHECK_POLICY = OFF;
ELSE
    ALTER LOGIN [TracbelCrm] WITH PASSWORD = '$senhaApp';
"@ | Out-Null

# O USUARIO DO BANCO VEM DENTRO DO BACKUP, E CHEGA ORFAO.
#
# Um usuario de banco guarda o SID do login que o criou. O backup foi feito noutro servidor, entao
# o SID gravado nao existe aqui: o usuario existe, o login existe, e os dois nao se conhecem - a
# aplicacao autentica e leva "login failed for user".
#
# APAGAR E RECRIAR NAO RESOLVE, e foi o que este script tentou primeiro: `DROP USER` falha com "the
# database principal owns a schema in the database". `ALTER USER ... WITH LOGIN` reaponta o SID sem
# tocar em propriedade de esquema nem em permissao - e e idempotente, que e o que uma instalacao
# reexecutavel precisa.
Sql @"
USE [TracbelCrm];

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name='TracbelCrm' AND type IN ('S','U'))
    ALTER USER [TracbelCrm] WITH LOGIN = [TracbelCrm];
ELSE
    CREATE USER [TracbelCrm] FOR LOGIN [TracbelCrm];

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members m
               JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
               JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
               WHERE r.name='db_datareader' AND u.name='TracbelCrm')
    ALTER ROLE db_datareader ADD MEMBER [TracbelCrm];

IF NOT EXISTS (SELECT 1 FROM sys.database_role_members m
               JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
               JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
               WHERE r.name='db_datawriter' AND u.name='TracbelCrm')
    ALTER ROLE db_datawriter ADD MEMBER [TracbelCrm];

-- db_ddladmin existe porque a API aplica migracao do EF Core na subida.
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members m
               JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
               JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
               WHERE r.name='db_ddladmin' AND u.name='TracbelCrm')
    ALTER ROLE db_ddladmin ADD MEMBER [TracbelCrm];
"@ | Out-Null
Ok 'conta da aplicacao ligada ao login (senha gerada aqui, nunca versionada)'

# A CONFERENCIA E FEITA COM A CONTA DA APLICACAO, e nao com a do administrador. Testar com
# Integrated Security provaria que o BANCO responde, nao que a API consegue entrar - e o SID orfao
# e exatamente o defeito que so aparece quando quem conecta e a conta certa.
$teste = New-Object System.Data.SqlClient.SqlConnection(
    "Server=localhost;Database=TracbelCrm;User Id=TracbelCrm;Password=$senhaApp;TrustServerCertificate=True;Connect Timeout=10")
try {
    $teste.Open()
    $cmdTeste = $teste.CreateCommand()
    $cmdTeste.CommandText = 'SELECT COUNT(*) FROM comercial.Cliente WHERE ExcluidoEm IS NULL'
    $quantos = $cmdTeste.ExecuteScalar()
    $teste.Close()
    Ok "a conta da aplicacao entra e le ($quantos clientes)"
} catch {
    throw "A conta da aplicacao NAO consegue entrar no banco: $($_.Exception.Message.Split([char]13)[0])"
}

$linhas = Sql "SELECT COUNT(*) FROM [TracbelCrm].comercial.FaturamentoDoCliente WHERE ExcluidoEm IS NULL"
$valor  = Sql "SELECT CAST(SUM(ValorLiquido)/1000000 AS decimal(12,1)) FROM [TracbelCrm].comercial.FaturamentoDoCliente WHERE ExcluidoEm IS NULL"
Ok "conferencia: $linhas meses de faturamento, R$ $valor milhoes"

# -------------------------------------------------------------------------------------------------
# 3. A aplicacao.
# -------------------------------------------------------------------------------------------------
Passo '3. Instalando a aplicacao'

# O servico ja foi parado e removido no passo 0, antes da checagem de porta.
New-Item -ItemType Directory -Force -Path $Destino | Out-Null
& robocopy (Join-Path $Raiz 'publicacao') $Destino /E /XD '_banco' /NFL /NDL /NJH /NJS /NP /R:2 /W:5 | Out-Null
if ($LASTEXITCODE -ge 8) { throw "A copia dos arquivos falhou (robocopy $LASTEXITCODE)." }
Ok "arquivos em $Destino"

# -------------------------------------------------------------------------------------------------
# 4. Certificado.
# -------------------------------------------------------------------------------------------------
Passo '4. Certificado'

$pastaSsl = Join-Path $Destino 'ssl'
New-Item -ItemType Directory -Force -Path $pastaSsl | Out-Null
$pfx = Join-Path $pastaSsl 'crm.pfx'

if (Test-Path $pfx) {
    Ok 'ja existe um crm.pfx - mantido'

    # CERTIFICADO DE CA INTERNA COSTUMA VIR SEM SENHA, e isso e normal: a protecao dele e a ACL do
    # arquivo, nao uma senha. Aceitar vazio aqui evita o que aconteceu na primeira tentativa - a
    # senha ficou em branco na configuracao, o Kestrel nao abriu o arquivo, e o servico morreu na
    # partida sem dizer por que.
    $senhaPfx = Read-Host 'Senha do crm.pfx (deixe VAZIO se o certificado nao tem senha)'

    # O ARQUIVO E ABERTO AGORA, e nao na subida do servico. Um .pfx que nao abre e o tipo de erro
    # que aparece como "servico parado", tres passos depois, sem relacao aparente com certificado.
    try {
        $conferencia = New-Object Security.Cryptography.X509Certificates.X509Certificate2(
            $pfx, $senhaPfx, [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
        Ok ("certificado valido - {0}, expira em {1}" -f `
            $conferencia.Subject, $conferencia.NotAfter.ToString('dd/MM/yyyy'))
        if (-not $conferencia.HasPrivateKey) {
            throw 'o arquivo nao tem chave privada. Um .pfx sem chave privada nao serve para servir HTTPS.'
        }
        if ($conferencia.NotAfter -lt (Get-Date)) { Aviso 'ESTE CERTIFICADO JA EXPIROU.' }
    } catch {
        throw "Nao consegui abrir $pfx : $($_.Exception.Message.Split([char]13)[0])"
    }
} else {
    $senhaPfx = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 20 | ForEach-Object { [char]$_ })
    $cert = New-SelfSignedCertificate `
        -Subject 'CN=tracbel-crm' `
        -DnsName 'tracbel-crm', 'localhost', $env:COMPUTERNAME, '10.150.4.249' `
        -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
        -NotAfter (Get-Date).AddYears(3) `
        -CertStoreLocation 'Cert:\LocalMachine\My' -KeyExportPolicy Exportable
    Export-PfxCertificate -Cert $cert -FilePath $pfx `
        -Password (ConvertTo-SecureString $senhaPfx -AsPlainText -Force) | Out-Null
    Ok "certificado AUTO-ASSINADO gerado (impressao: $($cert.Thumbprint))"
    Aviso 'auto-assinado faz o navegador avisar em toda visita.'
    Aviso 'Se a Tracbel tiver CA interna, troque este .pfx pelo certificado emitido.'
}

# -------------------------------------------------------------------------------------------------
# 5. Configuracao e servico.
# -------------------------------------------------------------------------------------------------
Passo '5. Registrando o servico'

# A CONFIGURACAO FICA FORA DO PACOTE, para que reinstalar a aplicacao nao apague a senha do banco.
$arquivoConfig = Join-Path $Destino 'appsettings.Production.json'

# SEM SENHA, O CAMPO NAO ENTRA. Escrever `"Password": ""` faz o Kestrel tentar abrir o arquivo
# COM uma senha vazia, que e diferente de abrir SEM senha - e a falha vira "servico parado".
$certificado = @{ Path = $pfx }
if (-not [string]::IsNullOrEmpty($senhaPfx)) { $certificado['Password'] = $senhaPfx }

@{
    ConnectionStrings = @{
        Crm = "Server=localhost;Database=TracbelCrm;User Id=TracbelCrm;Password=$senhaApp;TrustServerCertificate=True"
    }
    Kestrel = @{
        Endpoints = @{
            Https = @{
                Url = "https://0.0.0.0:$PortaApi"
                Certificate = $certificado
            }
        }
    }
} | ConvertTo-Json -Depth 8 | Set-Content -Path $arquivoConfig -Encoding UTF8

# A senha do banco esta neste arquivo: so administrador e o proprio servico leem.
$acl = Get-Acl $arquivoConfig
$acl.SetAccessRuleProtection($true, $false)
$acl.SetAccessRule((New-Object Security.AccessControl.FileSystemAccessRule('BUILTIN\Administrators','FullControl','Allow')))
$acl.SetAccessRule((New-Object Security.AccessControl.FileSystemAccessRule('NT AUTHORITY\SYSTEM','FullControl','Allow')))
Set-Acl $arquivoConfig $acl
Ok 'configuracao gravada e restrita a administradores'

& sc.exe create $NomeServico `
    binPath= "`"$Destino\Tracbel.Crm.Api.exe`"" `
    DisplayName= 'Tracbel CRM Agro - API e portal' `
    start= auto `
    depend= MSSQLSERVER | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Nao consegui criar o servico.' }

& sc.exe description $NomeServico 'API e portal do CRM Tracbel Agro.' | Out-Null
# Se cair, reinicia sozinho - tres vezes, com espera crescente.
& sc.exe failure $NomeServico reset= 86400 actions= restart/5000/restart/15000/restart/60000 | Out-Null

[Environment]::SetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', 'Production', 'Machine')

$momento = Get-Date
try { Start-Service -Name $NomeServico -ErrorAction Stop } catch { Aviso "Start-Service: $($_.Exception.Message.Split([char]13)[0])" }

Start-Sleep -Seconds 12
$estado = (Get-Service -Name $NomeServico).Status

if ($estado -ne 'Running') {
    # O MOTIVO E MOSTRADO AQUI, e nao apontado.
    #
    # A aplicacao que morre na partida quase nunca escreve no proprio log: ela nem chega a
    # configurar log. O que sobra e o Log de Aplicacao do Windows, e mandar alguem procurar la e
    # gastar uma ida e volta inteira para ler tres linhas que este script ja podia ter lido.
    Write-Host ''
    Write-Host '   --- por que o servico nao subiu ---' -ForegroundColor Yellow
    $eventos = Get-WinEvent -FilterHashtable @{ LogName = 'Application'; StartTime = $momento.AddMinutes(-1) } `
        -ErrorAction SilentlyContinue | Where-Object { $_.Message -match 'Tracbel|\.NET Runtime|Kestrel' }

    if ($eventos) {
        $eventos | Select-Object -First 3 | ForEach-Object {
            ($_.Message -split "`n" | Select-Object -First 8) | ForEach-Object { "   $($_.Trim())" }
            Write-Host '   ---'
        }
    } else {
        Write-Host '   (o Log de Aplicacao nao registrou nada; rodando a aplicacao no console para ver o erro)'
    }

    # ULTIMO RECURSO: roda o executavel direto e captura a saida. Um erro de certificado ou de
    # cadeia de conexao aparece na primeira linha - e nao aparece em lugar nenhum quando o
    # processo e iniciado pelo Gerenciador de Servicos.
    Write-Host ''
    Write-Host '   --- saida da aplicacao rodando no console (10 s) ---' -ForegroundColor Yellow
    $saidaTeste = Join-Path $env:TEMP 'tracbel-crm-teste.log'
    $proc = Start-Process -FilePath (Join-Path $Destino 'Tracbel.Crm.Api.exe') `
        -WorkingDirectory $Destino -PassThru -NoNewWindow `
        -RedirectStandardOutput $saidaTeste -RedirectStandardError "$saidaTeste.err"
    Start-Sleep -Seconds 10
    if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
    foreach ($arquivo in @($saidaTeste, "$saidaTeste.err")) {
        if (Test-Path $arquivo) {
            Get-Content $arquivo -ErrorAction SilentlyContinue | Select-Object -First 20 | ForEach-Object { "   $_" }
            Remove-Item $arquivo -Force -ErrorAction SilentlyContinue
        }
    }
    Write-Host ''

    throw "O servico subiu como $estado. O motivo esta acima."
}
Ok "servico $NomeServico rodando"

# -------------------------------------------------------------------------------------------------
# 6. Firewall.
# -------------------------------------------------------------------------------------------------
Passo '6. Firewall'

$regra = "Tracbel CRM Agro ($PortaApi)"
Get-NetFirewallRule -DisplayName $regra -ErrorAction SilentlyContinue | Remove-NetFirewallRule

if ($RedesPermitidas.Count -gt 0) {
    New-NetFirewallRule -DisplayName $regra -Direction Inbound -Protocol TCP `
        -LocalPort $PortaApi -Action Allow -Profile Any `
        -RemoteAddress $RedesPermitidas | Out-Null
    Ok "porta $PortaApi liberada SOMENTE para: $($RedesPermitidas -join ', ')"
} else {
    New-NetFirewallRule -DisplayName $regra -Direction Inbound -Protocol TCP `
        -LocalPort $PortaApi -Action Allow -Profile Any | Out-Null
    Ok "porta $PortaApi liberada"
    Write-Host ''
    Write-Host '   ATENCAO: a porta esta aberta para a REDE INTEIRA e a API nao autentica ninguem.' -ForegroundColor Red
    Write-Host '   Qualquer maquina que alcance este endereco ve faturamento, carteira e cliente' -ForegroundColor Red
    Write-Host '   das dezesseis filiais. Para restringir, rode de novo com:' -ForegroundColor Red
    Write-Host "       .\instalar-no-servidor.ps1 -RedesPermitidas '10.150.0.0/16'" -ForegroundColor Red
    Write-Host ''
}

# A PORTA DO BANCO NAO E LIBERADA. A API fala com ele por localhost; abrir 1433 para a rede so
# aumentaria superficie. Quem precisar do DBeaver roda no proprio servidor ou por tunel.
Aviso 'a porta 1433 do banco NAO foi liberada no firewall - de proposito'

# -------------------------------------------------------------------------------------------------
# 7. Prova de vida.
# -------------------------------------------------------------------------------------------------
Passo '7. Conferindo'

Add-Type @'
using System.Net; using System.Security.Cryptography.X509Certificates;
public class SemChecagemCrm : ICertificatePolicy {
  public bool CheckValidationResult(ServicePoint sp, X509Certificate c, WebRequest r, int p) { return true; }
}
'@ -ErrorAction SilentlyContinue
[Net.ServicePointManager]::CertificatePolicy = New-Object SemChecagemCrm
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Start-Sleep -Seconds 5
try {
    $saude = Invoke-RestMethod -Uri "https://localhost:$PortaApi/saude/banco" -TimeoutSec 30
    Ok ("API respondendo - banco conectado: {0}, migracoes pendentes: {1}" -f `
        $saude.conectado, $(if ($saude.migracoesPendentes.Count) { $saude.migracoesPendentes -join ',' } else { 'nenhuma' }))
} catch {
    Aviso ('a prova de vida da API falhou: ' + $_.Exception.Message)
}

try {
    $portal = Invoke-WebRequest -Uri "https://localhost:$PortaApi/" -TimeoutSec 30 -UseBasicParsing
    Ok ("portal respondendo - HTTP {0}, {1} bytes" -f $portal.StatusCode, $portal.RawContentLength)
} catch {
    Aviso ('o portal nao respondeu: ' + $_.Exception.Message)
}

Write-Host ''
Write-Host '=================================================================' -ForegroundColor Green
Write-Host " CRM no ar em:  https://10.150.4.249:$PortaApi" -ForegroundColor Green
Write-Host '=================================================================' -ForegroundColor Green
Write-Host ''
Write-Host ' O que ficou PENDENTE, e nao e pouco:' -ForegroundColor Yellow
Write-Host '  1. Backup: nao existe rotina. Este banco e agora o oficial e ninguem faz copia dele.'
Write-Host '  2. Certificado auto-assinado - todo navegador avisa. Trocar por um da CA interna.'
Write-Host '  3. A API nao autentica ninguem ainda: a identidade vem de cabecalho HTTP.'
Write-Host '     Qualquer um na rede que alcance a porta ve tudo. Ver documento 13.'
Write-Host '  4. Tres telas ainda mostram dado ficticio do prototipo: ficha do cliente,'
Write-Host '     ficha do equipamento e configuracoes.'
Write-Host ''
