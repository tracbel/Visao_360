<#
.SYNOPSIS
    Aplica a semente versionada no banco do CRM: dados de referência sempre, usuários de
    desenvolvimento só quando pedido.

.DESCRIPTION
    É o gancho que `subir-banco.ps1` já procurava (seção "GANCHO DE SEED"). Aplica, em ordem,
    os arquivos de `scripts/banco/seed/`:

      01-dados-de-referencia.sql        filiais, itens de catálogo, marcas, famílias, modelos
      02-usuarios-de-desenvolvimento.sql  andaime, só com -ComUsuariosDeDesenvolvimento

    Regras que este script obedece (documento 14, seção 8):
      8.1  o dado de referência nasce de seed versionado no Git, nunca de INSERT manual;
      8.2  o seed é IDEMPOTENTE — rodar duas vezes não duplica linha nem lança exceção.

    A conexão é montada a partir de infra/.env, do mesmo jeito que `subir-banco.ps1` faz, e o
    sqlcmd roda DENTRO do container — assim não é preciso ter ferramenta de SQL Server instalada
    na máquina.

.PARAMETER ComUsuariosDeDesenvolvimento
    Aplica também 02-usuarios-de-desenvolvimento.sql. Usuário de verdade vem do Entra ID; esta
    bandeira existe para o cadastro poder ser exercitado enquanto a autenticação não entra.

.EXAMPLE
    ./scripts/banco/rodar-seed.ps1

.EXAMPLE
    ./scripts/banco/rodar-seed.ps1 -ComUsuariosDeDesenvolvimento
#>
[CmdletBinding()]
param(
    [switch]$ComUsuariosDeDesenvolvimento
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$env_ = Join-Path $raiz 'infra/.env'

if (-not (Test-Path $env_)) {
    throw "Não achei infra/.env. Rode ./scripts/banco/subir-banco.ps1 primeiro."
}

# Lê infra/.env sem depender de módulo externo: são pares CHAVE=valor, e comentário começa com #.
$config = @{}
foreach ($linha in Get-Content $env_) {
    if ($linha -match '^\s*([A-Z_]+)\s*=\s*(.*)$') { $config[$Matches[1]] = $Matches[2].Trim() }
}

$container = 'tracbel-crm-db'
$banco = $config['DB_NOME']
$senha = $config['DB_SENHA']

if (-not $banco -or -not $senha) { throw "infra/.env não tem DB_NOME ou DB_SENHA." }

$arquivos = @(Join-Path $PSScriptRoot 'seed/01-dados-de-referencia.sql')

if ($ComUsuariosDeDesenvolvimento) {
    $arquivos += Join-Path $PSScriptRoot 'seed/02-usuarios-de-desenvolvimento.sql'
    Write-Host "Incluindo os usuários de DESENVOLVIMENTO — andaime, não vale para outro ambiente." -ForegroundColor Yellow
}

foreach ($arquivo in $arquivos) {
    if (-not (Test-Path $arquivo)) { throw "Arquivo de seed não encontrado: $arquivo" }

    $nome = Split-Path -Leaf $arquivo
    Write-Host "Aplicando $nome..." -ForegroundColor Cyan

    # COPIA O ARQUIVO PARA DENTRO DO CONTAINER em vez de mandá-lo pelo cano da entrada padrão.
    #
    # Por quê: o Windows PowerShell 5.1 escreve BOM ao repassar texto para um programa externo, e
    # o sqlcmd trata o BOM como caractere de comando — o erro é "Incorrect syntax near '?'" na
    # linha 1, que não diz nada sobre a causa. Com `docker cp` + `-i`, o sqlcmd lê o arquivo do
    # disco e `-f 65001` diz a ele que o arquivo é UTF-8, que é como os acentos dos nomes das
    # filiais chegam inteiros.
    $destino = "/tmp/$nome"
    docker cp $arquivo "${container}:$destino" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Não consegui copiar $nome para o container." }

    # -b faz o sqlcmd devolver código de saída diferente de zero quando o script falha; sem ele,
    # um erro de SQL passaria despercebido e o script "daria certo" sem ter feito nada.
    docker exec $container /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -P $senha -C -b -I -d $banco -f 65001 -i $destino

    if ($LASTEXITCODE -ne 0) { throw "O seed falhou em $nome (exit $LASTEXITCODE)." }
}

Write-Host "Seed aplicado." -ForegroundColor Green
