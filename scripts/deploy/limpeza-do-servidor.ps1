<#
  limpeza-do-servidor.ps1 - a limpeza de todo dia do servidor do CRM (02/10/2026).

  Roda NO SERVIDOR, como SYSTEM, pela tarefa `TracbelCrmLimpeza` (todo dia as 03:30), que o
  `instalar-agente-de-publicacao.ps1` registra - inclusive com -SoAtualizarOsScripts. Para ver o que
  ela achou, rode da estacao:

      .\scripts\deploy\verificar-publicacao.ps1

  =================================================================================================
  POR QUE ELA EXISTE

  Pedido do Ricardo em 02/10/2026: "verifique a memoria no servidor, deixe sempre para limpar". O que
  foi medido naquele dia, so lendo:

    - memoria: 16 GB, ~11,7 GB em uso de verdade e 4,1 GB de cache. O CRM inteiro (API, sincronizacao
      do ART e rotinas) usa ~340 MB. O resto e de outros sistemas e de quatro sessoes de Area de
      Trabalho Remota DESCONECTADAS, esquecidas abertas, cada uma segurando de 1,8 a 4,3 GB;
    - disco C:: o log do banco (`TracbelCrm_log.ldf`) com 6,6 GB para um banco de 1 GB, crescendo
      ~300 MB por dia. Recuperacao FULL sem backup de log: o log nunca era reaproveitado.

  O CACHE NAO E LIXO: e memoria livre que o Windows emprestou para arquivo, e ele a devolve sozinho
  quando alguem precisa. Forcar o "esvaziamento" so faria o servidor ler de novo do disco o que ja
  tinha em memoria. Por isso esta limpeza nao mexe nele.

  =================================================================================================
  O QUE ELA FAZ, NA ORDEM (cada passo no seu try: um que falha nao impede os outros)

    1. O LOG DO BANCO. Decisao do Ricardo em 02/10/2026: recuperacao SIMPLES. Sem backup de log, a
       FULL nao dava volta a ponto nenhum no tempo - so fazia o log crescer. Na SIMPLES o SQL Server
       reaproveita o log a cada checkpoint. Se o arquivo passar de 2 GB, ele e encolhido para 1 GB.
       As copias do agente (COPY_ONLY, completas) continuam valendo como antes.
    2. AS COPIAS DO AGENTE. Ficam as 5 mais recentes `<banco>-antes-de-*.bak` - a mesma regra do
       `publicar-pacote.ps1`, que guarda 4 antes de tirar a nova. E rede de seguranca: o disco encheu
       em 28/09 e em 30/09 porque a retencao do agente nao tinha chegado ao servidor.
    3. O RETRATO: memoria, cache, disco, o que o CRM e o SQL Server ocupam e os processos que mais
       ocupam.
    4. AS SESSOES DE AREA DE TRABALHO REMOTA DESCONECTADAS HA MAIS DE 24 HORAS viram AVISO, com quanto
       de memoria cada uma segura. Decisao do Ricardo em 02/10/2026: a limpeza NAO encerra a sessao de
       ninguem - quem esta desconectado pode ter trabalho aberto.

  Tudo vai para o log de eventos do Windows (fonte `TracbelCrmLimpeza`), para `limpeza.json` na pasta
  do agente - que o `verificar-publicacao.ps1` mostra - e para `logs\limpeza-<data>.log`.

  =================================================================================================
  O QUE ELA NUNCA FAZ

    - nao encerra nem desconecta sessao, e nao para processo nenhum;
    - nao encolhe o arquivo de DADOS do banco: encolher dado fragmenta os indices, e o arquivo volta
      a crescer na primeira carga;
    - nao apaga copia feita a mao, nem as copias recentes do agente;
    - nao esvazia o cache de memoria do Windows.
#>

[CmdletBinding()]
param(
    [string] $Configuracao = 'C:\aplicacoes\tracbel-crm-agente\configuracao.json',
    [int] $LogMaximoEmMb = 2048,
    [int] $LogAlvoEmMb = 1024,
    [int] $CopiasDoAgenteMantidas = 5,
    [int] $HorasParaAvisarSessao = 24,
    [int] $DiscoMinimoEmGb = 10,
    [switch] $Simular
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Configuracao)) {
    throw "Nao achei a configuracao em $Configuracao. Rode o instalar-agente-de-publicacao.ps1 primeiro."
}

$cfg = Get-Content $Configuracao -Raw | ConvertFrom-Json

$pastaDoAgente = Split-Path $Configuracao -Parent
$pastaDeLogs = Join-Path $pastaDoAgente 'logs'
New-Item -ItemType Directory -Force -Path $pastaDeLogs | Out-Null
$log = Join-Path $pastaDeLogs ('limpeza-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')

# UM ARQUIVO POR DIA. Guardam-se 30 dias, como os do agente.
Get-ChildItem $pastaDeLogs -Filter 'limpeza-*.log' |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Force -ErrorAction SilentlyContinue

$fonte = 'TracbelCrmLimpeza'
$avisos = New-Object System.Collections.Generic.List[string]
$erros = New-Object System.Collections.Generic.List[string]

function Registrar([string] $texto, [string] $nivel = 'info') {
    $linha = '{0:yyyy-MM-dd HH:mm:ss}  {1,-5}  {2}' -f (Get-Date), $nivel, $texto
    Write-Host $linha
    Add-Content -LiteralPath $log -Value $linha -Encoding utf8
}

# O LOG DE EVENTOS DO WINDOWS e onde quem administra o servidor olha. A fonte nasce aqui mesmo (a tarefa
# roda como SYSTEM, que pode cria-la); se nao der, o aviso nao derruba a limpeza.
function Avisar([string] $texto, [string] $tipo = 'Information', [int] $id = 4000) {
    try {
        if (-not [System.Diagnostics.EventLog]::SourceExists($fonte)) {
            New-EventLog -LogName Application -Source $fonte
        }
        Write-EventLog -LogName Application -Source $fonte -EntryType $tipo -EventId $id -Message $texto
    } catch {
        Registrar "nao consegui escrever no log de eventos: $($_.Exception.Message)" 'aviso'
    }
}

# A CONEXAO E A DO AGENTE: integrada, como SYSTEM, que no SQL Server deste servidor e administrador (o
# grupo BUILTIN\Administrators recebeu sysadmin na instalacao) - o mesmo direito com que o agente faz o
# BACKUP de cada publicacao.
function Consultar([string] $sql, [int] $timeout = 120) {
    $conexao = New-Object System.Data.SqlClient.SqlConnection $cfg.conexaoDoBanco
    $conexao.Open()
    try {
        $cmd = $conexao.CreateCommand()
        $cmd.CommandTimeout = $timeout
        $cmd.CommandText = $sql
        $tabela = New-Object System.Data.DataTable
        $tabela.Load($cmd.ExecuteReader())
        return , $tabela
    } finally { $conexao.Dispose() }
}

function Executar([string] $sql, [int] $timeout = 1800) {
    $conexao = New-Object System.Data.SqlClient.SqlConnection $cfg.conexaoDoBanco
    $conexao.Open()
    try {
        $cmd = $conexao.CreateCommand()
        $cmd.CommandTimeout = $timeout
        $cmd.CommandText = $sql
        [void] $cmd.ExecuteNonQuery()
    } finally { $conexao.Dispose() }
}

function ArquivoDeLog() {
    return (Consultar @"
SELECT name AS nome,
       CAST(size / 128.0 AS decimal(12, 1)) AS mb,
       CAST(FILEPROPERTY(name, 'SpaceUsed') / 128.0 AS decimal(12, 1)) AS usadoMb
FROM sys.database_files
WHERE type_desc = 'LOG'
"@).Rows[0]
}

Registrar "limpeza acordou (simulacao: $($Simular.IsPresent))"

# =================================================================================================
# 1. O log do banco: recuperacao SIMPLES e, acima de 2 GB, encolhido para 1 GB
# =================================================================================================

$banco = [ordered]@{
    recuperacaoAntes = $null
    recuperacao      = $null
    logArquivo       = $null
    logMbAntes       = $null
    logMbDepois      = $null
    logUsadoMb       = $null
    esperaDoLog      = $null
    encolhido        = $false
}

try {
    $estado = (Consultar 'SELECT recovery_model_desc AS recuperacao FROM sys.databases WHERE name = DB_NAME()').Rows[0]
    $banco.recuperacaoAntes = [string] $estado.recuperacao

    if ($banco.recuperacaoAntes -ne 'SIMPLE') {
        if ($Simular) {
            Registrar "SIMULACAO: poria o banco $($cfg.banco) em recuperacao SIMPLES (hoje $($banco.recuperacaoAntes))"
        } else {
            Executar "ALTER DATABASE [$($cfg.banco)] SET RECOVERY SIMPLE"
            Executar 'CHECKPOINT'
            Registrar "banco $($cfg.banco): recuperacao $($banco.recuperacaoAntes) -> SIMPLE"
            Avisar ("O banco $($cfg.banco) passou da recuperacao $($banco.recuperacaoAntes) para a SIMPLES (decisao de 02/10/2026). " +
                'O log passa a ser reaproveitado a cada checkpoint; as copias completas do agente continuam valendo.') 'Information' 4001
        }
    }

    $arquivo = ArquivoDeLog
    $banco.logArquivo = [string] $arquivo.nome
    $banco.logMbAntes = [double] $arquivo.mb
    $banco.logUsadoMb = [double] $arquivo.usadoMb
    $banco.logMbDepois = $banco.logMbAntes
    Registrar ("log do banco: {0} MB, {1} MB em uso" -f $banco.logMbAntes, $banco.logUsadoMb)

    if ($banco.logMbAntes -gt $LogMaximoEmMb) {
        if ($Simular) {
            Registrar "SIMULACAO: encolheria o log $($banco.logArquivo) de $($banco.logMbAntes) MB para $LogAlvoEmMb MB"
        } else {
            # SO O ARQUIVO DE LOG, pelo nome, e nunca o banco inteiro (SHRINKDATABASE encolheria o de dados).
            Executar 'CHECKPOINT'
            Executar ("DBCC SHRINKFILE (N'{0}', {1}) WITH NO_INFOMSGS" -f $banco.logArquivo.Replace("'", "''"), $LogAlvoEmMb)
            $depois = ArquivoDeLog
            $banco.logMbDepois = [double] $depois.mb
            $banco.logUsadoMb = [double] $depois.usadoMb
            $banco.encolhido = $true
            Registrar ("log encolhido: {0} MB -> {1} MB" -f $banco.logMbAntes, $banco.logMbDepois)
            Avisar ("O log do banco $($cfg.banco) foi encolhido de {0:N0} MB para {1:N0} MB." -f $banco.logMbAntes, $banco.logMbDepois) 'Information' 4002
        }
    }

    $final = (Consultar 'SELECT recovery_model_desc AS recuperacao, log_reuse_wait_desc AS espera FROM sys.databases WHERE name = DB_NAME()').Rows[0]
    $banco.recuperacao = [string] $final.recuperacao
    $banco.esperaDoLog = [string] $final.espera

    # A PARTE ATIVA DO LOG PODE ESTAR NO FIM DO ARQUIVO, e ai o encolhimento para antes do alvo. Nao e erro:
    # o log segue sendo reaproveitado, e a proxima rodada tenta de novo.
    if (-not $Simular -and $banco.logMbDepois -gt $LogMaximoEmMb) {
        $avisos.Add(("O log do banco continua com {0:N0} MB (espera do log: {1}); a proxima rodada tenta encolher de novo." -f $banco.logMbDepois, $banco.esperaDoLog))
    }
} catch {
    $erros.Add("log do banco: $($_.Exception.Message)")
    Registrar "o passo do log do banco parou num erro: $($_.Exception.Message)" 'erro'
}

# =================================================================================================
# 2. As copias antigas do agente
# =================================================================================================

$copias = [ordered]@{ pasta = $null; doAgente = 0; apagadas = 0; gbLiberados = 0 }

try {
    $pastaDeBackup = [string] (Consultar "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400)) AS pasta").Rows[0].pasta
    $copias.pasta = $pastaDeBackup

    # O MESMO NOME QUE A PUBLICACAO DA A COPIA: "<banco>-antes-de-<commit>-<data>.bak". As feitas a mao tem
    # outro nome e NAO sao tocadas.
    $doAgente = @(Get-ChildItem -Path $pastaDeBackup -Filter ("{0}-antes-de-*.bak" -f $cfg.banco) -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending)
    $copias.doAgente = $doAgente.Count

    $velhas = @($doAgente | Select-Object -Skip $CopiasDoAgenteMantidas)
    if ($velhas.Count -gt 0) {
        $bytes = ($velhas | Measure-Object -Property Length -Sum).Sum
        if ($Simular) {
            Registrar ("SIMULACAO: apagaria {0} copias antigas do agente ({1:N1} GB)" -f $velhas.Count, ($bytes / 1GB))
        } else {
            foreach ($velha in $velhas) { Remove-Item -LiteralPath $velha.FullName -Force }
            $copias.apagadas = $velhas.Count
            $copias.gbLiberados = [math]::Round($bytes / 1GB, 1)
            Registrar ("{0} copias antigas do agente apagadas ({1:N1} GB); ficam as {2} mais recentes" -f $velhas.Count, ($bytes / 1GB), $CopiasDoAgenteMantidas)
            Avisar ("A limpeza apagou {0} copias antigas do agente ({1:N1} GB) em {2}; ficam as {3} mais recentes." -f $velhas.Count, ($bytes / 1GB), $pastaDeBackup, $CopiasDoAgenteMantidas) 'Information' 4003
        }
    }
} catch {
    $erros.Add("copias do agente: $($_.Exception.Message)")
    Registrar "o passo das copias do agente parou num erro: $($_.Exception.Message)" 'erro'
}

# =================================================================================================
# 3. O retrato: memoria, cache, disco e quem ocupa
# =================================================================================================

$memoria = [ordered]@{ totalMb = $null; disponivelMb = $null; cacheMb = $null; apiMb = $null; sincronizacaoMb = $null; sqlServerMb = $null; maiores = @() }
$disco = [ordered]@{ livreGb = $null; totalGb = $null }
$processos = @()

try {
    $sistema = Get-CimInstance Win32_OperatingSystem
    $contadores = Get-CimInstance Win32_PerfFormattedData_PerfOS_Memory
    $memoria.totalMb = [math]::Round($sistema.TotalVisibleMemorySize / 1024)
    $memoria.disponivelMb = [int] $contadores.AvailableMBytes
    $memoria.cacheMb = [math]::Round(([double] $contadores.StandbyCacheCoreBytes + [double] $contadores.StandbyCacheNormalPriorityBytes + [double] $contadores.StandbyCacheReserveBytes) / 1MB)

    $processos = @(Get-Process | Select-Object Name, Id, SessionId, WorkingSet64)
    $porId = @{}
    foreach ($p in $processos) { $porId[[int] $p.Id] = $p }

    function MemoriaDoServico([string] $nome) {
        $servico = Get-CimInstance Win32_Service -Filter "Name='$nome'"
        if (-not $servico -or $servico.ProcessId -le 0 -or -not $porId.ContainsKey([int] $servico.ProcessId)) { return $null }
        return [math]::Round($porId[[int] $servico.ProcessId].WorkingSet64 / 1MB)
    }

    $memoria.apiMb = MemoriaDoServico $cfg.servicoDaApi
    $memoria.sincronizacaoMb = MemoriaDoServico $cfg.servicoDaSincronizacao
    $memoria.sqlServerMb = MemoriaDoServico 'MSSQLSERVER'
    $memoria.maiores = @($processos | Sort-Object WorkingSet64 -Descending | Select-Object -First 8 | ForEach-Object {
            [ordered]@{ processo = $_.Name; sessao = $_.SessionId; mb = [math]::Round($_.WorkingSet64 / 1MB) }
        })

    Registrar ("memoria: {0:N0} MB disponiveis de {1:N0} MB ({2:N0} MB de cache); API {3} MB, sincronizacao {4} MB, SQL Server {5} MB" -f
        $memoria.disponivelMb, $memoria.totalMb, $memoria.cacheMb, $memoria.apiMb, $memoria.sincronizacaoMb, $memoria.sqlServerMb)

    # O CACHE CONTA COMO DISPONIVEL - o Windows o devolve sozinho. Pouca memoria disponivel e o que pede gente.
    if ($memoria.totalMb -gt 0 -and $memoria.disponivelMb -lt 0.1 * $memoria.totalMb) {
        $avisos.Add(("Memoria disponivel baixa: {0:N0} MB de {1:N0} MB." -f $memoria.disponivelMb, $memoria.totalMb))
    }

    $c = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'"
    $disco.livreGb = [math]::Round($c.FreeSpace / 1GB, 1)
    $disco.totalGb = [math]::Round($c.Size / 1GB, 1)
    Registrar ("disco C: {0:N1} GB livres de {1:N1} GB" -f $disco.livreGb, $disco.totalGb)
    if ($disco.livreGb -lt $DiscoMinimoEmGb) {
        $avisos.Add(("Disco C: com {0:N1} GB livres (o minimo que esta limpeza aceita calada e {1} GB)." -f $disco.livreGb, $DiscoMinimoEmGb))
    }
} catch {
    $erros.Add("retrato: $($_.Exception.Message)")
    Registrar "o retrato da memoria e do disco parou num erro: $($_.Exception.Message)" 'erro'
}

# =================================================================================================
# 4. As sessoes de Area de Trabalho Remota desconectadas: SO AVISO
# =================================================================================================

# PELA API DO WINDOWS (wtsapi32), e nao pelo `quser`: a saida do `quser` vem traduzida ("Disco") e em
# colunas que mudam com o tamanho do nome, e a data da desconexao nem aparece nela.
$codigoDasSessoes = @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SessaoDoWindows
{
    public int Id;
    public string Usuario;
    public string Dominio;
    public int Estado;
    public DateTime? DesconectadaEm;
    public DateTime? EntrouEm;
}

public static class SessoesDoWindows
{
    [DllImport("wtsapi32.dll", SetLastError = true)]
    static extern bool WTSEnumerateSessionsW(IntPtr servidor, int reservado, int versao, out IntPtr sessoes, out int quantas);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    static extern bool WTSQuerySessionInformationW(IntPtr servidor, int sessao, int classe, out IntPtr buffer, out int bytes);

    [DllImport("wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr memoria);

    [StructLayout(LayoutKind.Sequential)]
    struct WTS_SESSION_INFO { public int SessionId; public IntPtr WinStationName; public int State; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WTSINFO
    {
        public int State;
        public int SessionId;
        public int IncomingBytes;
        public int OutgoingBytes;
        public int IncomingFrames;
        public int OutgoingFrames;
        public int IncomingCompressedBytes;
        public int OutgoingCompressedBytes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string WinStationName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)] public string Domain;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)] public string UserName;
        public long ConnectTime;
        public long DisconnectTime;
        public long LastInputTime;
        public long LogonTime;
        public long CurrentTime;
    }

    const int WTSSessionInfo = 24;

    static DateTime? Data(long filetime)
    {
        if (filetime <= 0) return null;
        return DateTime.FromFileTime(filetime);
    }

    public static List<SessaoDoWindows> Listar()
    {
        var lista = new List<SessaoDoWindows>();
        IntPtr sessoes;
        int quantas;
        if (!WTSEnumerateSessionsW(IntPtr.Zero, 0, 1, out sessoes, out quantas)) return lista;
        try
        {
            int tamanho = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            for (int i = 0; i < quantas; i++)
            {
                var s = (WTS_SESSION_INFO)Marshal.PtrToStructure(new IntPtr(sessoes.ToInt64() + i * tamanho), typeof(WTS_SESSION_INFO));
                IntPtr buffer;
                int bytes;
                if (!WTSQuerySessionInformationW(IntPtr.Zero, s.SessionId, WTSSessionInfo, out buffer, out bytes)) continue;
                try
                {
                    var info = (WTSINFO)Marshal.PtrToStructure(buffer, typeof(WTSINFO));
                    if (string.IsNullOrEmpty(info.UserName)) continue;
                    lista.Add(new SessaoDoWindows
                    {
                        Id = s.SessionId,
                        Usuario = info.UserName,
                        Dominio = info.Domain,
                        Estado = info.State,
                        DesconectadaEm = Data(info.DisconnectTime),
                        EntrouEm = Data(info.LogonTime)
                    });
                }
                finally { WTSFreeMemory(buffer); }
            }
        }
        finally { WTSFreeMemory(sessoes); }
        return lista;
    }
}
'@

$desconectadas = New-Object System.Collections.Generic.List[object]

try {
    if (-not ('SessoesDoWindows' -as [type])) { Add-Type -TypeDefinition $codigoDasSessoes -Language CSharp }

    # O ESTADO 4 E "DESCONECTADA" (WTSDisconnected): a pessoa fechou a janela da Area de Trabalho Remota sem
    # sair, e tudo o que ela tinha aberto continua ocupando memoria.
    foreach ($sessao in [SessoesDoWindows]::Listar()) {
        if ($sessao.Estado -ne 4 -or -not $sessao.DesconectadaEm) { continue }
        $horas = [math]::Round(((Get-Date) - $sessao.DesconectadaEm).TotalHours)
        $bytesDaSessao = ($processos | Where-Object { $_.SessionId -eq $sessao.Id } | Measure-Object -Property WorkingSet64 -Sum).Sum
        $item = [ordered]@{
            sessao    = $sessao.Id
            usuario   = if ($sessao.Dominio) { "$($sessao.Dominio)\$($sessao.Usuario)" } else { $sessao.Usuario }
            desde     = $sessao.DesconectadaEm.ToString('s')
            horas     = $horas
            memoriaMb = [math]::Round([double] $bytesDaSessao / 1MB)
        }
        $desconectadas.Add($item)
        Registrar ("sessao {0} de {1} desconectada ha {2} h, com {3:N0} MB" -f $item.sessao, $item.usuario, $item.horas, $item.memoriaMb)

        if ($horas -ge $HorasParaAvisarSessao) {
            $avisos.Add(("Sessao {0} de {1} desconectada ha {2} h, segurando {3:N0} MB de memoria. A limpeza nao encerra sessao: quem a abriu decide." -f
                    $item.sessao, $item.usuario, $item.horas, $item.memoriaMb))
        }
    }
} catch {
    $erros.Add("sessoes: $($_.Exception.Message)")
    Registrar "a leitura das sessoes parou num erro: $($_.Exception.Message)" 'erro'
}

# =================================================================================================
# O resultado: limpeza.json, o log de eventos e o codigo da tarefa
# =================================================================================================

# AS LISTAS VAO COMO .ToArray(), e nao como @($lista): no PowerShell 5.1, @() de uma List[T] dentro de uma hashtable
# literal quebra com "Os tipos de argumento nao correspondem" - e no 7 da estacao nao (medido em 02/10/2026).
$resultado = [ordered]@{
    quando               = (Get-Date).ToString('s')
    simulacao            = $Simular.IsPresent
    banco                = $banco
    copiasDoAgente       = $copias
    memoria              = $memoria
    disco                = $disco
    sessoesDesconectadas = $desconectadas.ToArray()
    avisos               = $avisos.ToArray()
    erros                = $erros.ToArray()
    log                  = $log
}

if (-not $Simular) {
    $resultado | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $pastaDoAgente 'limpeza.json') -Encoding utf8
}

foreach ($aviso in $avisos) { Registrar $aviso 'aviso' }
foreach ($erro in $erros) { Registrar $erro 'erro' }

$resumo = ("Limpeza do servidor: log do banco com {0} MB (recuperacao {1}); {2} copias antigas apagadas; {3} MB de memoria disponiveis; {4} GB livres no C:." -f
    $banco.logMbDepois, $banco.recuperacao, $copias.apagadas, $memoria.disponivelMb, $disco.livreGb)

if ($erros.Count -gt 0) {
    Avisar ($resumo + "`r`n`r`nErros:`r`n" + ($erros -join "`r`n") + "`r`n`r`nO log esta em $log") 'Error' 4900
    exit 1
}

if ($avisos.Count -gt 0) {
    Avisar ($resumo + "`r`n`r`nAvisos:`r`n" + ($avisos -join "`r`n")) 'Warning' 4100
} else {
    Avisar $resumo 'Information' 4000
}

Registrar 'limpeza terminada'
exit 0
