<#
.SYNOPSIS
    Gera docs/banco/DICIONARIO.md, docs/banco/ERD.md, docs/banco/catalogo.csv e
    docs/banco/esquema.json a partir do modelo do EF Core — sem precisar de um banco rodando.

.DESCRIPTION
    Roda a ferramenta scripts/banco/DicionarioGerador (console .NET), que constrói o modelo do
    CrmDbContext com o provedor SQL Server (mesma técnica de
    tests/Tracbel.Crm.Arquitetura.Testes/Banco/ModeloBanco.cs — nenhuma conexão é aberta) e
    escreve os quatro arquivos de saída, sempre a partir do zero.

    O modelo tem 63 tabelas em 10 schemas (documento 17, seção 8.12). Se a contagem impressa no
    fim não for essa, alguém mexeu no modelo sem passar pelo portão da seção 10.2.

    Feito para rodar no CI: não depende de banco, de rede nem de segredo — só do código já
    compilado. O resultado é versionado no Git (docs/banco/*), então um PR que mudar o modelo do
    EF Core e não rodar este script vai aparecer como diff no próximo `git status` de quem rodar.

.EXAMPLE
    ./scripts/banco/gerar-dicionario-crm.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$raiz = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $raiz

Write-Host "Gerando o dicionário do banco a partir do modelo do EF Core..." -ForegroundColor Cyan
dotnet run --project scripts/banco/DicionarioGerador/DicionarioGerador.csproj --configuration Release -- $raiz
if ($LASTEXITCODE -ne 0) { throw "A geração do dicionário falhou (exit $LASTEXITCODE)." }

Write-Host ""
Write-Host "Gerado (versione estes quatro arquivos junto com a migration que mudou o modelo):" -ForegroundColor Green
Write-Host "  docs/banco/DICIONARIO.md"
Write-Host "  docs/banco/ERD.md"
Write-Host "  docs/banco/catalogo.csv"
Write-Host "  docs/banco/esquema.json"
