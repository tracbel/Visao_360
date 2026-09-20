<#
  gerar-manifesto-do-pacote.ps1 - o cartao de identidade de um pacote de publicacao (issue 62).

      ./scripts/ci/gerar-manifesto-do-pacote.ps1 -Destino pacote/manifesto.json

  =================================================================================================
  O QUE ELE RESPONDE, E POR QUE ISSO IMPORTA

  O agente de publicacao do servidor le este arquivo ANTES de tocar em qualquer coisa. Ele precisa
  saber duas coisas:

    1. DE QUAL COMMIT este pacote veio - para nao republicar o que ja esta no ar, e para o registro
       dizer o que foi publicado;
    2. SE A SUBIDA VAI APAGAR ALGUMA COISA - porque "publicar e migrar" (regra R-4 do doc 46): a API
       aplica as migracoes pendentes ao subir, e uma migracao destrutiva em producao nao se desfaz
       com `git revert`; desfaz-se restaurando backup.

  A DECISAO DE 20/09/2026: migracao ADITIVA sobe sozinha, migracao DESTRUTIVA para e avisa. Este
  script e quem separa uma da outra.

  =================================================================================================
  POR QUE A ANALISE E FEITA AQUI, NO CI, E NAO NO SERVIDOR

  Aqui existe o codigo-fonte. O `dotnet ef migrations script` gera o SQL EXATO que a subida vai
  executar, comando a comando — inclusive o que o EF monta sozinho e que ninguem escreveu a mao. Ler
  os arquivos .cs seria pior: uma `DropColumn` pode nascer de uma mudanca de tipo que o EF resolve
  recriando a coluna, e isso nao aparece no .cs como "Drop".

  No servidor chega so a marcacao por migracao. Ele nao precisa saber interpretar SQL, e nao precisa
  do SDK do .NET instalado.

  =================================================================================================
  UMA MARCACAO POR MIGRACAO, E NAO UMA PARA O PACOTE INTEIRO

  A primeira versao deste script analisava o script IDEMPOTENTE, que traz a cadeia inteira desde a
  migracao inicial. Resultado, medido em 20/09/2026: "destrutiva: SIM" para sempre, porque a fase 1
  do documento 41 apagou 31 tabelas e ela nunca sai do historico. Toda publicacao pararia pedindo
  autorizacao, e o portao viraria ruido — o jeito mais rapido de ensinar alguem a aprovar sem ler.

  Entao cada migracao e analisada SOZINHA, com o script do passo anterior para ela. O agente do
  servidor consulta o proprio banco para saber quais ainda faltam, e olha so essas.

  =================================================================================================
  O QUE CONTA COMO DESTRUTIVO

  DROP TABLE, DROP COLUMN e TRUNCATE: os tres apagam dado que existia. Ficam de fora, de proposito:

    - DROP INDEX, DROP CONSTRAINT: mexem em estrutura de apoio, nao em dado;
    - sp_rename: preserva o dado (foi assim que a tabela da PAM mudou de nome na issue 64);
    - DELETE: aparece em migracao de semente e apaga linha de configuracao, nao de negocio. Se um dia
      uma migracao precisar apagar dado com DELETE, ela deve dizer isso no nome - e este script vai
      deixar passar. E a fronteira honesta deste desenho.
#>

[CmdletBinding()]
param(
    [string] $Destino = 'pacote/manifesto.json',
    [string] $Raiz = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)
)

$ErrorActionPreference = 'Stop'

$projeto = Join-Path $Raiz 'src/Tracbel.Crm.Infraestrutura/Tracbel.Crm.Infraestrutura.csproj'
$inicial = Join-Path $Raiz 'src/Tracbel.Crm.Api/Tracbel.Crm.Api.csproj'

<#
  .SYNOPSIS
  O que, num trecho de SQL, apaga dado que existia.

  .DESCRIPTION
  DROP TABLE, DROP COLUMN e TRUNCATE. Ficam de fora, de proposito: DROP INDEX e DROP CONSTRAINT
  (estrutura de apoio, nao dado) e sp_rename (preserva o dado — foi assim que a tabela da PAM mudou
  de nome na issue 64).
#>
function Destrutivas([string] $sql) {
    $padroes = [ordered]@{
        'DROP TABLE'  = '(?im)^\s*DROP\s+TABLE\s+(?<alvo>\[?[\w\.\[\]]+)'
        'DROP COLUMN' = '(?im)ALTER\s+TABLE\s+(?<tabela>\[?[\w\.\[\]]+)\s+DROP\s+COLUMN\s+(?<alvo>\[?[\w\[\]]+)'
        'TRUNCATE'    = '(?im)^\s*TRUNCATE\s+TABLE\s+(?<alvo>\[?[\w\.\[\]]+)'
    }

    $achados = [System.Collections.Generic.List[string]]::new()

    foreach ($nome in $padroes.Keys) {
        foreach ($achado in [regex]::Matches($sql, $padroes[$nome])) {
            $alvo = $achado.Groups['alvo'].Value.Trim()
            $tabela = $achado.Groups['tabela'].Value.Trim()
            $texto = if ($tabela) { "$nome $tabela.$alvo" } else { "$nome $alvo" }
            if (-not $achados.Contains($texto)) { $achados.Add($texto) }
        }
    }

    return $achados
}

Push-Location $Raiz
try {
    & dotnet tool restore | Out-Null

    # A LISTA DE MIGRACOES, na ordem em que o EF as aplica.
    $saida = & dotnet tool run dotnet-ef migrations list --project $projeto --startup-project $inicial --no-build --prefix-output
    if ($LASTEXITCODE -ne 0) { throw "Nao consegui listar as migracoes (codigo $LASTEXITCODE)." }

    # O `--prefix-output` marca cada linha de dado com "data:"; o resto e log do proprio EF.
    $migracoes = @($saida | Where-Object { $_ -match '^data:\s+\S' } | ForEach-Object { ($_ -replace '^data:\s+', '').Trim() } |
        Where-Object { $_ -and $_ -notmatch '^No migrations' })

    # CADA MIGRACAO SOZINHA: o script do passo anterior para ela. `0` e o banco vazio, que e a
    # origem da primeira.
    $detalhadas = [System.Collections.Generic.List[object]]::new()

    for ($i = 0; $i -lt $migracoes.Count; $i++) {
        $de = if ($i -eq 0) { '0' } else { $migracoes[$i - 1] }
        $para = $migracoes[$i]

        $sqlPath = Join-Path ([System.IO.Path]::GetTempPath()) "migracao-$([guid]::NewGuid()).sql"
        & dotnet tool run dotnet-ef migrations script $de $para --project $projeto --startup-project $inicial --no-build --output $sqlPath | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Nao consegui gerar o script de $para (codigo $LASTEXITCODE)." }

        $sql = Get-Content $sqlPath -Raw
        Remove-Item $sqlPath -Force -ErrorAction SilentlyContinue

        $achados = Destrutivas $sql
        $detalhadas.Add([ordered]@{
            id                   = $para
            destrutiva           = $achados.Count -gt 0
            operacoesDestrutivas = @($achados)
        })

        Write-Host ("  {0,-2} {1,-60} {2}" -f ($i + 1), $para, $(if ($achados.Count -gt 0) { "DESTRUTIVA ($($achados.Count))" } else { 'aditiva' }))
    }
}
finally { Pop-Location }

# O COMMIT VEM DO AMBIENTE DO GITHUB quando ha um; fora dele, do proprio git. Assim o script roda
# igual na estacao de quem quiser conferir o que um pacote carrega.
$commit = if ($env:GITHUB_SHA) { $env:GITHUB_SHA } else { (& git -C $Raiz rev-parse HEAD).Trim() }
$execucao = if ($env:GITHUB_RUN_ID) { $env:GITHUB_RUN_ID } else { $null }

$manifesto = [ordered]@{
    commit       = $commit
    commitCurto  = $commit.Substring(0, 7)
    execucaoDoCi = $execucao
    geradoEm     = (Get-Date).ToUniversalTime().ToString('o')

    # A ORDEM IMPORTA: e a ordem em que o EF as aplica, e o agente precisa dela para saber o que
    # ainda falta no banco dele.
    migracoes    = @($detalhadas)
}

$pasta = Split-Path $Destino -Parent
if ($pasta -and -not (Test-Path $pasta)) { New-Item -ItemType Directory -Force -Path $pasta | Out-Null }

$manifesto | ConvertTo-Json -Depth 5 | Set-Content -Path $Destino -Encoding utf8

$destrutivas = @($detalhadas | Where-Object { $_.destrutiva })

Write-Host ''
Write-Host "Manifesto em $Destino"
Write-Host "  commit ................ $($manifesto.commitCurto)"
Write-Host "  migracoes ............. $($detalhadas.Count)"
Write-Host "  destrutivas ........... $($destrutivas.Count)"
foreach ($m in $destrutivas) {
    Write-Host "     $($m.id)"
    foreach ($o in $m.operacoesDestrutivas) { Write-Host "        $o" }
}
