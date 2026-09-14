<#
  Exporta o que o CRM (e a extração do Vórtice) JÁ TEM para pré-preencher as planilhas de coleta
  (documento 34). Só leitura.

  Uso:
    $env:ConnectionStrings__Crm = '<cadeia de conexão do CRM>'   # nunca versionada
    ./scripts/coleta/extrair-pre-preenchimento.ps1
    python scripts/coleta/gerar-modelos-de-coleta.py

  A saída tem nome de funcionário, chassi e número real: vai para dados-locais/coleta/entrada, que o
  Git ignora. Nenhum valor de conexão é impresso.
#>
param(
    [string]$Saida = (Join-Path $PSScriptRoot '..\..\dados-locais\coleta\entrada'),
    [string]$OperadoresDoVortice = (Join-Path $PSScriptRoot '..\..\docs\extracao-vortice\catalogos-bpm\IV_Operador.csv')
)

$ErrorActionPreference = 'Stop'
if (-not $env:ConnectionStrings__Crm) { throw 'Defina ConnectionStrings__Crm (leitura basta). Nada foi exportado.' }
New-Item -ItemType Directory -Force $Saida | Out-Null

function Consultar([string]$sql) {
    $conexao = New-Object System.Data.SqlClient.SqlConnection $env:ConnectionStrings__Crm
    $conexao.Open()
    try {
        $comando = $conexao.CreateCommand()
        $comando.CommandText = $sql
        $comando.CommandTimeout = 300
        $leitor = $comando.ExecuteReader()
        $linhas = New-Object System.Collections.Generic.List[object]
        while ($leitor.Read()) {
            $linha = [ordered]@{}
            for ($i = 0; $i -lt $leitor.FieldCount; $i++) { $linha[$leitor.GetName($i)] = if ($leitor.IsDBNull($i)) { '' } else { $leitor.GetValue($i) } }
            $linhas.Add([pscustomobject]$linha)
        }
        return $linhas
    }
    finally { $conexao.Dispose() }
}

function Gravar($linhas, [string]$nome) {
    $linhas | Export-Csv (Join-Path $Saida $nome) -Delimiter ';' -NoTypeInformation -Encoding utf8
    "{0,-28} {1,6} linhas" -f $nome, @($linhas).Count
}

# A MESMA CHAVE de ResponsavelPeloMunicipio.ChaveDoNome: sem domínio, maiúsculas, sem acento, . _ - viram espaço.
function ChaveDoNome([string]$nome) {
    if ([string]::IsNullOrWhiteSpace($nome)) { return '' }
    $texto = $nome.Split('@')[0].Trim().ToUpperInvariant().Normalize([Text.NormalizationForm]::FormD)
    $sb = [Text.StringBuilder]::new()
    foreach ($c in $texto.ToCharArray()) {
        if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($c) -eq [Globalization.UnicodeCategory]::NonSpacingMark) { continue }
        if ($c -eq '.' -or $c -eq '_' -or $c -eq '-' -or [char]::IsWhiteSpace($c)) { [void]$sb.Append(' ') } else { [void]$sb.Append($c) }
    }
    return (($sb.ToString() -split ' ') | Where-Object { $_ }) -join ' '
}

# A MESMA REGRA de ResponsavelPeloMunicipio.CompararCen — rótulo, não fusão.
function CompararCen([string]$numa, $usuarioNuma, [string]$outra, $usuarioOutra) {
    if ([string]::IsNullOrWhiteSpace($numa) -or [string]::IsNullOrWhiteSpace($outra)) { return 'UmaFonteSo' }
    if ("$usuarioNuma" -ne '' -and "$usuarioNuma" -eq "$usuarioOutra") { return 'MesmoNome' }
    $a = ChaveDoNome $numa; $b = ChaveDoNome $outra
    if ($a -eq $b) { return 'MesmoNome' }
    if ($a -and $b -and (($a -split ' ')[0] -eq ($b -split ' ')[0] -or $a.Contains($b) -or $b.Contains($a))) { return 'ProvavelMesmaPessoa' }
    return 'NomesDiferentes'
}

Gravar (Consultar "SELECT Codigo, Nome, CAST(EstaAtiva AS int) AS EstaAtiva FROM organizacao.Empresa WHERE Codigo LIKE '0101%' ORDER BY Codigo") 'filiais.csv'

Gravar (Consultar "SELECT Codigo, Nome, DiasCicloClasseA, DiasCicloClasseB, DiasCicloClasseC, DiasCicloClasseD FROM organizacao.LinhaDeNegocio ORDER BY Ordem") 'linhas-de-negocio.csv'

Gravar (Consultar @"
SELECT TOP 15 a.ProdutoCodigoIbge, a.ProdutoNome, CAST(SUM(a.AreaPlantadaHectares) AS decimal(14, 0)) AS HectaresNaAdr
FROM organizacao.AreaPlantadaNoMunicipio a
JOIN organizacao.MunicipioDaAreaDeAtuacao d ON d.MunicipioId = a.MunicipioId AND d.PertenceAAdr = 1 AND d.EncerradoEm IS NULL
WHERE a.Ano = (SELECT MAX(Ano) FROM organizacao.AreaPlantadaNoMunicipio)
GROUP BY a.ProdutoCodigoIbge, a.ProdutoNome
ORDER BY SUM(a.AreaPlantadaHectares) DESC
"@) 'culturas-na-adr.csv'

Gravar (Consultar @"
SELECT tt.Codigo, tt.Nome, COUNT(i.Id) AS Interacoes2026, SUM(CASE WHEN i.Latitude IS NOT NULL THEN 1 ELSE 0 END) AS ComCoordenada
FROM processo.TipoTarefa tt
JOIN processo.Interacao i ON i.TipoTarefaId = tt.Id AND i.OcorridaEm >= '2026-01-01'
GROUP BY tt.Codigo, tt.Nome
ORDER BY COUNT(i.Id) DESC
"@) 'tipos-de-tarefa.csv'

Gravar (Consultar @"
SELECT ma.Nome AS Marca, f.Nome AS Familia, m.Nome AS Modelo, COUNT(e.Id) AS Maquinas
FROM frota.Modelo m
JOIN frota.Familia f ON f.Id = m.FamiliaId
JOIN frota.Marca ma ON ma.Id = f.MarcaId
LEFT JOIN frota.Equipamento e ON e.ModeloId = m.Id AND e.ExcluidoEm IS NULL
GROUP BY ma.Nome, f.Nome, m.Nome
ORDER BY COUNT(e.Id) DESC, m.Nome
"@) 'modelos.csv'

Gravar (Consultar @"
SELECT e.Chassi, m.Nome AS Modelo, emp.Codigo AS Filial
FROM frota.Equipamento e
JOIN frota.Modelo m ON m.Id = e.ModeloId
JOIN organizacao.Empresa emp ON emp.Id = e.EmpresaId
WHERE e.ExcluidoEm IS NULL
ORDER BY emp.Codigo, m.Nome, e.Chassi
"@) 'parque.csv'

# USUÁRIOS, com a SUGESTÃO de supervisor e gerente da extração do Vórtice (login em texto, casado pela chave do nome).
$usuarios = Consultar @"
SELECT u.NomePrincipal AS Login, u.NomeCompleto AS Nome, emp.Codigo AS FilialDeCasa, c.ChaveOrigem AS ChaveVortice
FROM seguranca.Usuario u
LEFT JOIN organizacao.Empresa emp ON emp.Id = u.EmpresaId
LEFT JOIN integracao.ChaveExterna c ON c.Entidade = 'Usuario' AND c.RegistroId = u.Id
WHERE u.Natureza = 'Pessoa' AND u.EstaAtivo = 1 AND u.ExcluidoEm IS NULL
ORDER BY u.NomeCompleto
"@
$loginPorChave = @{}
foreach ($u in $usuarios) { $loginPorChave[(ChaveDoNome $u.Login)] = $u.Login }
$operadores = @{}
if (Test-Path $OperadoresDoVortice) {
    # A extração do Vórtice grava com VÍRGULA; o separador é lido do cabeçalho para não depender disso.
    $separador = if ((Get-Content $OperadoresDoVortice -TotalCount 1) -match '";"') { ';' } else { ',' }
    foreach ($o in (Import-Csv $OperadoresDoVortice -Delimiter $separador)) { $operadores["$($o.SeqUsuario)".Trim()] = $o }
}
$comSugestao = foreach ($u in $usuarios) {
    $o = $operadores["$($u.ChaveVortice)".Trim()]
    [pscustomobject]@{
        Login = $u.Login; Nome = $u.Nome; FilialDeCasa = $u.FilialDeCasa
        SupervisorSugerido = if ($o) { $loginPorChave[(ChaveDoNome $o.Supervisor)] } else { '' }
        GerenteSugerido = if ($o) { $loginPorChave[(ChaveDoNome $o.Gerente)] } else { '' }
    }
}
Gravar $comSugestao 'usuarios.csv'

$municipios = Consultar @"
SELECT m.CodigoIbge, m.Nome AS Municipio, d.Regiao, loja.Codigo AS Loja,
       MAX(CASE WHEN r.Papel = 'Cen' AND r.Fonte = 'PlanilhaAreaDeAtuacao' THEN r.NomeNaOrigem END) AS CenAreaDeAtuacao,
       MAX(CASE WHEN r.Papel = 'Cen' AND r.Fonte = 'PlanilhaAreaDeAtuacao' THEN r.UsuarioId END) AS UsuarioCenAreaDeAtuacao,
       MAX(CASE WHEN r.Papel = 'Cen' AND r.Fonte = 'PlanilhaCenEGestorPorMunicipio' THEN r.NomeNaOrigem END) AS CenCenEGestor,
       MAX(CASE WHEN r.Papel = 'Cen' AND r.Fonte = 'PlanilhaCenEGestorPorMunicipio' THEN r.UsuarioId END) AS UsuarioCenCenEGestor,
       MAX(CASE WHEN r.Papel = 'Gestor' THEN r.NomeNaOrigem END) AS GestorCenEGestor
FROM organizacao.MunicipioDaAreaDeAtuacao d
JOIN organizacao.Municipio m ON m.Id = d.MunicipioId
LEFT JOIN organizacao.Empresa loja ON loja.Id = d.EmpresaResponsavelId
LEFT JOIN organizacao.ResponsavelPeloMunicipio r ON r.MunicipioId = d.MunicipioId AND r.EncerradoEm IS NULL
WHERE d.PertenceAAdr = 1 AND d.EncerradoEm IS NULL
GROUP BY m.CodigoIbge, m.Nome, d.Regiao, loja.Codigo
ORDER BY m.Nome
"@
$comComparacao = foreach ($m in $municipios) {
    [pscustomobject]@{
        CodigoIbge = $m.CodigoIbge; Municipio = $m.Municipio; Regiao = $m.Regiao; Loja = $m.Loja
        CenAreaDeAtuacao = $m.CenAreaDeAtuacao; CenCenEGestor = $m.CenCenEGestor; GestorCenEGestor = $m.GestorCenEGestor
        Comparacao = CompararCen $m.CenAreaDeAtuacao $m.UsuarioCenAreaDeAtuacao $m.CenCenEGestor $m.UsuarioCenCenEGestor
    }
}
Gravar $comComparacao 'municipios-da-adr.csv'
"comparação do CEN: " + (($comComparacao | Group-Object Comparacao | Sort-Object Name | ForEach-Object { "$($_.Name)=$($_.Count)" }) -join ' · ')
