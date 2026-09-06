<#
  binarios-02-configs.ps1 - Leitura SOMENTE LEITURA dos arquivos de configuracao do Vortice,
  com REDACAO automatica de segredos e do ultimo octeto de enderecos IP.

  Nao grava nada no servidor. Nao imprime senha/token/chave.

  Uso:
    powershell -ExecutionPolicy Bypass -File .\binarios-02-configs.ps1 `
        -Raiz '\\10.150.5.172\c$\Vortice' -OutFile '...\configs-redigidos.md'
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Raiz,
    [string]$OutFile,
    [string]$RotuloHost = 'host'
)

# ---------- redacao ----------
function Protect-Texto {
    param([string]$t)
    if ($null -eq $t) { return '' }

    # 1) chave=valor sensivel  (ini / connstring / xml key="pwd" value="...")
    $chavesSensiveis = 'pw|pwd|pass|passwd|password|senha|secret|token|apikey|api_key|appkey|clientsecret|client_secret|accountkey|privatekey|credential|authorization|dbpw|defdbpw'
    $t = [regex]::Replace($t, "(?im)^(\s*[\w\.\-\[\]]*($chavesSensiveis)[\w\.\-]*\s*[:=]\s*)(.+)$", '${1}***REDIGIDO***')
    $t = [regex]::Replace($t, "(?i)\b(($chavesSensiveis))\s*=\s*([^;""'>\s]+)", '${1}=***REDIGIDO***')

    # 2) atributos XML value="..." quando o name/key e sensivel
    $t = [regex]::Replace($t, "(?i)((?:key|name)\s*=\s*""[^""]*($chavesSensiveis)[^""]*""\s+value\s*=\s*"")([^""]*)("")", '${1}***REDIGIDO***${4}')

    # 3) credencial embutida em URL  scheme://user:senha@host
    $t = [regex]::Replace($t, '(?i)(://[^/@:\s]+:)([^/@\s]+)(@)', '${1}***REDIGIDO***${3}')

    # 4) ultimo octeto de IPv4 privado
    $t = [regex]::Replace($t, '\b((?:10|172|192)\.\d{1,3}\.\d{1,3})\.(\d{1,3})\b', '${1}.xx')

    # 5) strings base64 longas (chaves/certs)
    $t = [regex]::Replace($t, '(?m)^[A-Za-z0-9+/]{60,}={0,2}$', '***BLOB-REDIGIDO***')

    # 6) elementos XML sensiveis  <PasLogin>...</PasLogin>, <Senha>, <Pwd>, <Token>
    $t = [regex]::Replace($t, "(?is)<(\w*(?:pas|pwd|pw|senha|secret|token|key|credential)\w*)>(.*?)</\1>", '<${1}>***REDIGIDO***</${1}>')

    # 7) blobs criptografados do proprio Vortice  [DPT]...[/DPT]
    $t = [regex]::Replace($t, "(?is)\[DPT\].*?\[/DPT\]", '[DPT]***REDIGIDO***[/DPT]')

    # 8) chave de licenca/sessao  DefUSerConnect=USUARIO/AAAAAA-BBBBBB-...
    $t = [regex]::Replace($t, "(?im)^(\s*[\w]*userconnect\s*=\s*)([\w]+)(/)([\w\-]+)", '${1}${2}${3}***REDIGIDO***')

    # 9) serial solto no formato AAAAAA-BBBBBB-CCCCCC-...
    $t = [regex]::Replace($t, "\b([0-9A-F]{6}-){2,}[0-9A-F]{2,6}\b", '***SERIAL-REDIGIDO***')
    return $t
}

$alvos = @(
    'TDev\SQL.ini','TDev\SQLx.ini','TDev\SQL_Modelo.ini',
    'TDev\Vortice_Config.INI','TDev\Vortice_Config_Modelo.INI','TDev\vaCONFIG.ini',
    'TDev\utl\vaReplace.ini',
    'TDBin\sql.ini','TDBin\DCC.INI',
    'NT\Config\app.xml','NT\Config\config.xml',
    'NT\AppMonitorService.exe.config','NT\Vortico Server.exe.config','NT\Jupiter Service.exe.config',
    'NT\Vortico Admin.exe.config','NT\Config Con.exe.config','NT\SQL net.exe.config',
    'NT\Bi Integrador.exe.config','NT\Smtp Tester.exe.config','NT\Database Check.exe.config',
    'Install\silent.ini'
)

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# Configuracoes do Vortice - $RotuloHost (redigido)")
[void]$sb.AppendLine()
[void]$sb.AppendLine("> Coletado em $(Get-Date -Format 'yyyy-MM-dd HH:mm') a partir de ``$Raiz`` (somente leitura).")
[void]$sb.AppendLine("> Senhas, tokens e chaves aparecem como ``***REDIGIDO***``; o ultimo octeto dos IPs privados vira ``.xx``.")
[void]$sb.AppendLine()

foreach ($a in $alvos) {
    $p = Join-Path $Raiz $a
    [void]$sb.AppendLine("## ``$a``")
    if (-not (Test-Path -LiteralPath $p)) { [void]$sb.AppendLine(); [void]$sb.AppendLine('_nao existe neste host._'); [void]$sb.AppendLine(); continue }
    $fi = Get-Item -LiteralPath $p
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("_$($fi.Length) bytes ? modificado em $($fi.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))_")
    [void]$sb.AppendLine()
    try   { $txt = Get-Content -LiteralPath $p -Raw -ErrorAction Stop }
    catch { [void]$sb.AppendLine('```'); [void]$sb.AppendLine("ERRO DE LEITURA: $($_.Exception.Message)"); [void]$sb.AppendLine('```'); [void]$sb.AppendLine(); continue }
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine((Protect-Texto $txt).TrimEnd())
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine()
}

$saida = $sb.ToString()
if ($OutFile) { Set-Content -LiteralPath $OutFile -Value $saida -Encoding UTF8; "gravado: $OutFile" }
else { $saida }
