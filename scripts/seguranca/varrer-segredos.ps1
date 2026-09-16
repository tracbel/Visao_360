<#
.SYNOPSIS
    Procura segredos POR PADRÃO no repositório: arquivos versionados, arquivos novos ainda não
    ignorados e todo o histórico do Git. Nunca imprime um valor.

.DESCRIPTION
    Issue [001] do backlog mestre (docs/projeto/46A-PLANO-DO-BACKLOG-MESTRE.md).

    POR PADRÃO, E NÃO POR VALOR: o script não lê o .env nem compara com senha nenhuma. Procura a FORMA
    de um segredo — "Password=...", "senha = '...'", bloco de chave privada, token do GitHub, JWT,
    segredo de aplicativo do Entra ID, chave da AWS — e classifica cada achado:

      REFERENCIA  o valor é variável, interpolação ou marcador ($env:X, $(..), {config.Senha}, ***,
                  DEFINA-..., <senha>): não há segredo ali;
      TESTE       o valor está num arquivo de teste (credencial inventada para testar o mascaramento);
      SUSPEITO    todo o resto. Precisa de olho humano.

    Cada achado sai como arquivo:linha (ou commit:arquivo), o padrão, a classe e a CHAVE com o valor
    trocado por «oculto:N», onde N é o tamanho. O valor em si nunca vai para a tela nem para o relatório.

    Sai com código 1 se houver SUSPEITO, e 0 se não houver.

.PARAMETER SemHistorico
    Não varre o histórico (só os arquivos atuais). Mais rápido.

.PARAMETER Relatorio
    Caminho de um arquivo Markdown para gravar o resultado, além da tela.

.EXAMPLE
    ./scripts/seguranca/varrer-segredos.ps1
    ./scripts/seguranca/varrer-segredos.ps1 -Relatorio dados-locais/varredura.md
#>
param(
    [switch] $SemHistorico,
    [string] $Relatorio
)

$ErrorActionPreference = 'Stop'
$raiz = (git rev-parse --show-toplevel).Trim()
Set-Location $raiz

# ---------------------------------------------------------------------------------------------
# Padrões. O grupo "v" é o valor, e é só ele que se mascara. O grupo "k", quando existe, é a chave.
# ---------------------------------------------------------------------------------------------
$padroes = @(
    # "N'" é o prefixo de texto Unicode do SQL Server (PASSWORD = N'$senha'): não faz parte do valor.
    @{ Nome = 'cadeia-de-conexao'; Filtro = 'password|pwd'
       Regex = [regex]::new('(?i)\b(?<k>password|pwd)\s*=\s*(N(?=''))?''?(?<v>[^;"''\s]+)', 'Compiled') }
    @{ Nome = 'atribuicao-entre-aspas'; Filtro = 'senha|password|passwd|secret|api_key|apikey|api-key|token'
       Regex = [regex]::new('(?i)\b(?<k>senha|password|passwd|client[_-]?secret|secret|api[_-]?key|access[_-]?token|token)\b["'']?\s*[:=]\s*["''](?<v>[^"'']{6,})["'']', 'Compiled') }
    @{ Nome = 'variavel-de-ambiente'; Filtro = 'senha|password|secret|token|_key'
       Regex = [regex]::new('^\s*(?<k>[A-Z][A-Z0-9_]*(SENHA|PASSWORD|SECRET|TOKEN|_KEY)[A-Z0-9_]*)\s*=\s*(?<v>\S{4,})\s*$', 'Compiled') }
    @{ Nome = 'chave-privada'; Filtro = 'private key'
       Regex = [regex]::new('(?<v>-----BEGIN [A-Z ]*PRIVATE KEY-----)', 'Compiled') }
    @{ Nome = 'token-do-github'; Filtro = 'gh'
       Regex = [regex]::new('\b(?<v>gh[pousr]_[A-Za-z0-9]{30,})', 'Compiled') }
    @{ Nome = 'jwt'; Filtro = 'eyj'
       Regex = [regex]::new('\b(?<v>eyJ[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{15,}\.[A-Za-z0-9_-]{10,})', 'Compiled') }
    @{ Nome = 'segredo-de-aplicativo-entra'; Filtro = '8q~'
       Regex = [regex]::new('(?<v>[A-Za-z0-9~_.-]{3}8Q~[A-Za-z0-9~_.-]{31,34})', 'Compiled') }
    @{ Nome = 'chave-aws'; Filtro = 'akia'
       Regex = [regex]::new('\b(?<v>AKIA[0-9A-Z]{16})\b', 'Compiled') }
)

# O valor é referência, e não segredo, quando é variável, interpolação ou marcador.
$referencia = [regex]::new(
    '^(\$|%|\{|<|\[|@|\(|\*+$|\.{3}|#\{)|\$\(|\$\{|\$env:|\{\d+\}|DEFINA|PLACEHOLDER|EXEMPLO|EXAMPLE|CHANGEME|SEU[_-]|SUA[_-]|YOUR[_-]|xxx|^(null|true|false|none|string|required)$|config\.|opcoes\.|\bSenha\b[!)]?$',
    'Compiled, IgnoreCase')

# Marcador de redação ("***REDIGIDO***"), definição de expressão regular (o próprio detector de
# segredos de outro script) e valor curto ou só de pontuação — que é documentação citando o padrão
# entre crases, e não senha.
$redacao = [regex]::new('^\*{2,}.*\*{2,}$|REDIGID|OMITID|REDACT|MASCARAD', 'Compiled, IgnoreCase')
$definicaoDePadrao = [regex]::new('^(re\.compile|new Regex|\[regex\]|Regex\()|\(\?[imsx]+\)', 'Compiled, IgnoreCase')

function Classificar([string] $arquivo, [string] $valor) {
    if ($referencia.IsMatch($valor)) { return 'REFERENCIA' }
    if ($redacao.IsMatch($valor) -or $definicaoDePadrao.IsMatch($valor)) { return 'REFERENCIA' }
    if ($valor.Length -lt 4 -or $valor -notmatch '[A-Za-z0-9]') { return 'REFERENCIA' }
    if ($arquivo -match '(^|/)tests?/|Testes?\.cs$|\.test\.|\.spec\.') { return 'TESTE' }
    return 'SUSPEITO'
}

$achados = [System.Collections.Generic.List[object]]::new()

function Examinar([string] $origem, [string] $arquivo, [int] $linha, [string] $texto) {
    $minusculo = $texto.ToLowerInvariant()
    foreach ($p in $padroes) {
        $gatilho = $false
        foreach ($f in $p.Filtro.Split('|')) { if ($minusculo.Contains($f)) { $gatilho = $true; break } }
        if (-not $gatilho) { continue }

        foreach ($m in $p.Regex.Matches($texto)) {
            $valor = $m.Groups['v'].Value
            $chave = if ($m.Groups['k'].Success) { $m.Groups['k'].Value } else { $p.Nome }
            $script:achados.Add([pscustomobject]@{
                Origem  = $origem
                Arquivo = $arquivo
                Linha   = $linha
                Padrao  = $p.Nome
                Classe  = Classificar $arquivo $valor
                Trecho  = "$chave=«oculto:$($valor.Length)»"
            })
        }
    }
}

# ---------------------------------------------------------------------------------------------
# 1. Arquivos atuais: versionados + novos que o .gitignore não bloqueia.
# ---------------------------------------------------------------------------------------------
$binario = '\.(png|jpe?g|gif|ico|pdf|zip|7z|gz|dll|exe|pdb|bak|mdf|ldf|xlsx?|docx?|pptx?|woff2?|ttf|eot|mp4|db|sqlite)$'
$arquivos = @(git ls-files) + @(git ls-files --others --exclude-standard) |
    Where-Object { $_ -notmatch $binario } | Sort-Object -Unique

$nArquivos = 0
foreach ($arquivo in $arquivos) {
    if (-not (Test-Path -LiteralPath $arquivo -PathType Leaf)) { continue }
    $nArquivos++
    $n = 0
    foreach ($texto in [System.IO.File]::ReadLines((Join-Path $raiz $arquivo))) {
        $n++
        Examinar 'atual' $arquivo $n $texto
    }
}

# ---------------------------------------------------------------------------------------------
# 2. Histórico: toda linha acrescentada em qualquer commit de qualquer ref.
# ---------------------------------------------------------------------------------------------
$nCommits = 0
if (-not $SemHistorico) {
    $commit = ''; $arquivo = ''
    git log --all -p -U0 --no-color --no-ext-diff --format='commit %h' | ForEach-Object {
        $texto = $_
        if ($texto.StartsWith('commit ')) { $commit = $texto.Substring(7); $script:nCommits++; return }
        if ($texto.StartsWith('+++ ')) { $arquivo = $texto -replace '^\+\+\+ (b/)?', ''; return }
        if ($texto.StartsWith('+') -and $arquivo -notmatch $binario) {
            Examinar "historico:$commit" $arquivo 0 $texto.Substring(1)
        }
    }
}

# ---------------------------------------------------------------------------------------------
# Resultado. Achados iguais no histórico e no arquivo atual aparecem uma vez por origem.
# ---------------------------------------------------------------------------------------------
$unicos = $achados | Sort-Object Classe, Arquivo, Origem, Linha, Trecho -Unique
$porClasse = $unicos | Group-Object Classe | ForEach-Object { "$($_.Name): $($_.Count)" }
$suspeitos = @($unicos | Where-Object Classe -eq 'SUSPEITO')

$saida = [System.Collections.Generic.List[string]]::new()
$saida.Add("Varredura de segredos por padrão — $(Get-Date -Format 'yyyy-MM-dd HH:mm')")
$saida.Add("HEAD: $((git rev-parse --short HEAD).Trim()) · arquivos atuais lidos: $nArquivos · commits no histórico: $(if ($SemHistorico) { 'não varrido' } else { $nCommits })")
$saida.Add("Achados: $($unicos.Count) · " + ($porClasse -join ' · '))
$saida.Add('')
foreach ($a in $unicos) {
    $local = if ($a.Linha -gt 0) { "$($a.Arquivo):$($a.Linha)" } else { $a.Arquivo }
    $saida.Add(("{0,-10} {1,-26} {2,-22} {3}  {4}" -f $a.Classe, $a.Padrao, $a.Origem, $local, $a.Trecho))
}

$saida | ForEach-Object { Write-Host $_ }

if ($Relatorio) {
    $md = @('# Varredura de segredos por padrão', '', '```text') + $saida + @('```')
    $md | Set-Content -LiteralPath $Relatorio -Encoding UTF8
    Write-Host "relatório: $Relatorio"
}

if ($suspeitos.Count -gt 0) { exit 1 } else { exit 0 }
