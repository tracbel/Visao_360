<#
  03-exporta-catalogos.ps1
  ------------------------
  Exporta INTEGRALMENTE (todas as linhas) os catalogos declarativos do Vortice CRM -
  o "codigo-fonte" do sistema que mora em tabela, nao em arquivo: modelos de processo,
  fases, acoes, resultados, acoes automaticas, formularios, questoes, politicas de
  seguranca, parametros globais, Message Center, Objetos Dinamicos e jobs do
  Servidor de Processos.

  Regras de seguranca aplicadas:
    - Colunas cujo nome contenha senha/hash/token/pasw/passw/pwd/secret sao OMITIDAS.
    - Nada e escrito no banco.

  Saida: docs/extracao-vortice/catalogos-bpm/<TABELA>.csv
         docs/extracao-vortice/_raw/catalogos-resumo.csv
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$Dest = Join-Path $Global:VorticeRaiz 'catalogos-bpm'
$Raw  = Join-Path $Global:VorticeRaiz '_raw'

# Nome real das tabelas confirmado no catalogo do banco:
#   - nao existe "IV_Fase": as fases dos modelos de processo estao em IV_ProcFase
#   - nao existe "GE_Parametro": os parametros estao em GE_ParametroGlobal (+ IV_GlobalPar)
#   - Message Center = IV_ResMsgPapel (regras de mensagem por resultado/papel/canal)
#   - Objetos Dinamicos = GE_ObjDinamico (coluna Comando guarda o SQL) + GE_ObjDinAplic
$tabelas = @(
    # --- nucleo BPM: modelo de processo ---
    'IV_CodProcesso', 'IV_CodPrcEmpr', 'IV_CodProcComent', 'IV_ProcFase', 'IV_ProcSt',
    'IV_ProcPersp', 'IV_ProcVinc', 'IV_ProcAcao', 'IV_ProcResultado', 'IV_ObjFlow',
    # --- acoes, resultados, automacao ---
    'IV_Acao', 'IV_AcaoCtrl', 'IV_AcaoRem', 'IV_Resultado', 'IV_ClasseRes', 'IV_Motivo',
    'IV_AcaoAuto', 'IV_AcaoAutoCtrl', 'IV_ResultadoReq', 'IV_ResParam', 'IV_RetProcRegra',
    'IV_Evento', 'IV_Distribui', 'IV_Operador', 'IV_OpBloq',
    # --- formularios / questionarios ---
    'IV_Formulario', 'IV_Questao', 'IV_QuestaoLista', 'IV_Atributo', 'IV_AtribLista',
    'IV_Pcte', 'IV_PcteAtrFx',
    # --- textos, parametros e listas ---
    'IV_TxtPadrao', 'IV_Propriedade', 'IV_GlobalPar', 'IV_GlobalParLista', 'IV_GlobalParCtrl',
    'IV_ListSQL', 'IV_Campanha',
    # --- Message Center / canais de saida ---
    'IV_ResMsgPapel', 'IV_OPTEMAIL', 'IV_OPTFONE', 'GE_EMAILTEMPLATE', 'GE_EMAILAGD',
    'IV_CBRCRITERIOMSG', 'IV_CbrCriterio', 'IV_CbrCriterioDef',
    # --- segmentacao / carteiras ---
    'IVS_Depto', 'IVS_DeptoEmpr', 'IVS_DeptoRes', 'IVS_DEPTOPOT', 'IVS_Carteira',
    'IVS_CartDepto', 'IVS_CartCid', 'IVS_Segm', 'IVS_UsrMeta', 'IV_VENDEDOR',
    # --- plataforma: sistema, modulos, aplicacoes, seguranca ---
    'GE_Sistema', 'GE_Modulo', 'GE_Mod', 'GE_AppModulo', 'GE_Aplicacao',
    'GE_PolSeg', 'GE_PolSegItem', 'GE_PolSegItLst', 'GE_PolSegAces', 'GE_PolSegCtrl',
    'GE_POLSEGPERM', 'GE_PolSegParams', 'GE_Permissao', 'GE_CampoPerm', 'GE_CampoExig',
    'GE_Tab', 'GE_Col', 'GE_Alias', 'GE_AtributoFixo', 'GE_Sequencia', 'GE_Empresa',
    # --- parametros globais ---
    'GE_ParametroGlobal', 'GE_ParamLista', 'GE_SyncParam',
    # --- objetos dinamicos (SQL gravado no banco) ---
    'GE_ObjDinamico', 'GE_ObjDinAplic', 'GE_Consulta', 'GE_ConsultaVar', 'GE_CONSSQL',
    # --- relatorios / QlikView ---
    'GE_QVCons', 'GE_QVConsCol', 'GE_QVConsVar', 'GE_QVPasta', 'GE_QVPastaCons', 'GE_QVRegra',
    # --- servidor de processos / jobs ---
    'GEP_JOBCAD', 'GEP_JOBAGD', 'GEP_JOBFILA', 'GEP_JobMonitor', 'GEP_JOBNOTIFICAR',
    'GEP_Notificar', 'GEP_Fila', 'GEP_Processo', 'GEP_ProcControle',
    'GEP_ParEnvia', 'GEP_ParRecebe', 'GEP_ParRecEnvia', 'GEP_UsrSat', 'GEP_IMPORT_GUI',
    # --- e-mail / OUT / documentos ---
    'OUT_Contato', 'OUT_Pessoa', 'OUT_PessoaLink', 'OUT_PessoaRelacao',
    'DMN_DocTp', 'IV_DoctoTipo', 'IV_DoctoApl', 'IV_DoctoAplUso',
    # --- de/para e integracao ---
    'IVT_DePara', 'GE_IMPORTA_CART'
)

# --- colunas sensiveis: nunca exportar ---
$padraoSensivel = '(?i)(senha|hash|token|pasw|passw|pwd|secret)'

Write-Host 'Lendo metadados de colunas...' -ForegroundColor Cyan
$todasColunas = Consulta -Rotulo 'sys.columns (todas)' -Sql @'
SELECT OBJECT_NAME(c.object_id) AS tabela, c.name AS coluna, c.column_id, ty.name AS tipo
FROM sys.columns c WITH (NOLOCK)
JOIN sys.tables t WITH (NOLOCK) ON t.object_id = c.object_id
JOIN sys.types ty WITH (NOLOCK) ON ty.user_type_id = c.user_type_id
ORDER BY c.object_id, c.column_id
'@
$colPorTabela = @{}
foreach ($c in $todasColunas) {
    $t = [string]$c['tabela']
    if (-not $colPorTabela.ContainsKey($t)) { $colPorTabela[$t] = New-Object System.Collections.ArrayList }
    [void]$colPorTabela[$t].Add($c)
}
# indice case-insensitive nome real da tabela
$nomeReal = @{}
foreach ($k in $colPorTabela.Keys) { $nomeReal[$k.ToLower()] = $k }

$resumo = New-Object System.Collections.ArrayList

foreach ($t in $tabelas) {
    $real = $nomeReal[$t.ToLower()]
    if (-not $real) {
        Write-Warning ("tabela inexistente, pulada: " + $t)
        [void]$resumo.Add([pscustomobject]@{ tabela = $t; existe = $false; linhas = 0; colunas = 0; colunas_omitidas = ''; arquivo = '' })
        continue
    }

    $cols = $colPorTabela[$real]
    $omitidas = @($cols | Where-Object { [string]$_['coluna'] -match $padraoSensivel } | ForEach-Object { [string]$_['coluna'] })
    $usadas = @($cols | Where-Object { [string]$_['coluna'] -notmatch $padraoSensivel } | ForEach-Object {
            # text/ntext/image nao podem entrar em ORDER BY nem DISTINCT; convertidos para leitura
            $tp = [string]$_['tipo']
            $nm = [string]$_['coluna']
            if ($tp -in @('text', 'ntext')) { "CONVERT(nvarchar(max), [$nm]) AS [$nm]" }
            elseif ($tp -eq 'image') { "'(binario omitido)' AS [$nm]" }
            else { "[$nm]" }
        })

    $sql = 'SELECT ' + ($usadas -join ', ') + " FROM [$real] WITH (NOLOCK)"
    $linhas = Consulta -Rotulo $real -Sql $sql -TimeoutSec 180
    $arq = Join-Path $Dest ($real + '.csv')
    [void](ExportaCsv $linhas $arq)

    [void]$resumo.Add([pscustomobject]@{
            tabela           = $real
            existe           = $true
            linhas           = @($linhas).Count
            colunas          = $usadas.Count
            colunas_omitidas = ($omitidas -join '; ')
            arquivo          = 'catalogos-bpm/' + $real + '.csv'
        })
}

$resumo | Export-Csv (Join-Path $Raw 'catalogos-resumo.csv') -NoTypeInformation -Encoding UTF8
Write-Host ('OK: ' + @($resumo | Where-Object { $_.existe }).Count + ' catalogos exportados; ' +
    (@($resumo | Measure-Object linhas -Sum).Sum) + ' linhas no total.') -ForegroundColor Green
