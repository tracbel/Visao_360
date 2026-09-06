<#
  08-gera-indices.ps1
  -------------------
  Monta os arquivos de leitura (Markdown) a partir dos CSV ja extraidos pelos
  scripts 01 a 07. NAO acessa o banco: le apenas os arquivos de saida, entao
  pode ser reexecutado offline para reformatar a documentacao.

  ATENCAO: este arquivo tem acentos e PRECISA ser salvo em UTF-8 COM BOM.
  O Windows PowerShell 5.1 le .ps1 sem BOM como ANSI (cp1252) e corrompe o texto.

  Gera:
    docs/extracao-vortice/modulos/00-INDICE.md
    docs/extracao-vortice/ddl/linked-server.md
    docs/extracao-vortice/volumetria/00-INDICE.md
#>
. (Join-Path $PSScriptRoot '_comum.ps1')

$R   = $Global:VorticeRaiz
$Raw = Join-Path $R '_raw'

function LeCsv($caminho) {
    if (-not (Test-Path $caminho)) { Write-Warning ("faltando: " + $caminho); return @() }
    return @(Import-Csv $caminho -Encoding UTF8)
}

$sb = New-Object System.Text.StringBuilder
function W($t) { [void]$sb.AppendLine($t) }
$BQ = [char]96   # crase: em string de aspas duplas o backtick e caractere de escape

# ==========================================================================
#  1) modulos/00-INDICE.md
# ==========================================================================
$mods = LeCsv (Join-Path $Raw 'modulos-meta.csv')
$deps = LeCsv (Join-Path $Raw 'dependencias.csv')

function Classifica($m) {
    $n = $m.nome.ToUpper()
    $refs = ($m.tabelas_ref + ' ' + $m.alvos_escrita + ' ' + $m.outras_ref).ToUpper()
    $tags = New-Object System.Collections.ArrayList
    if ($refs -match 'IV_AGENDA|IV_HISTORICO|IV_PROCESSO|IV_ACAOAUTO|IV_PROCDADO|IV_PROCFASE|IV_PROCST') { [void]$tags.Add('BPM') }
    if ($n -match '^(IMP_|X_|PR_VTC_INT|PR_INT|PR_INTSPR|PR_ATULINK|JDE)' -or $n -match 'TOTVS|IMPORT|INTEGRA' -or $refs -match 'GEP_IMPORT|IMP_|X_TOTVS|EXT_NFS|EXT_TITULO|EXT_OS|EXT_VEIC') { [void]$tags.Add('Integração') }
    if ($n -match 'IVS|CARTEIRA|COBERTURA' -or $refs -match 'IVS_') { [void]$tags.Add('Segmentação') }
    if ($n -match '^(BI_|VW_REL|QVW|TBA|X_BI_|X_CRM_BI)' -or $n -match 'REL_|RELATORIO|_BI_') { [void]$tags.Add('Relatório') }
    if ($n -match '^SP_.*DIAGRAM|FN_DIAGRAMOBJECTS|SYSDIAGRAM') { [void]$tags.Add('SSMS (nativo)') }
    if ($n -match 'TESTE|_BKP|BKP_') { [void]$tags.Add('Teste/backup') }
    if ($tags.Count -eq 0) { [void]$tags.Add('—') }
    return ($tags -join ', ')
}

$procs  = @($mods | Where-Object { $_.tipo -eq 'SQL_STORED_PROCEDURE' } | Sort-Object modify_date -Descending)
$funcs  = @($mods | Where-Object { $_.tipo -like '*FUNCTION*' } | Sort-Object nome)
$trigs  = @($mods | Where-Object { $_.tipo -eq 'SQL_TRIGGER' })
$views  = @($mods | Where-Object { $_.tipo -eq 'VIEW' } | Sort-Object nome)
$escrev = @($mods | Where-Object { $_.escreve -eq 'True' })
$procsEscrevem = @($procs | Where-Object { $_.escreve -eq 'True' }).Count

W '# Índice dos 548 objetos programáveis do banco CRM (Vórtice CRM / Tracbel)'
W ''
W ('Extraído em ' + (Get-Date -Format 'yyyy-MM-dd HH:mm') + ' do banco de **produção** ' + $BQ + 'CRM' + $BQ +
  ' (SQL Server 2019, nível de compatibilidade 100), somente leitura.')
W ''
W '## Sumário'
W ''
W '| Tipo | Qtd | Escrevem em tabela | Pasta |'
W '|---|---:|---:|---|'
W ('| Stored procedures | ' + $procs.Count + ' | ' + $procsEscrevem + ' | ' + $BQ + 'modulos/procedures/' + $BQ + ' |')
W ('| Functions escalares | ' + $funcs.Count + ' | ' + @($funcs | Where-Object { $_.escreve -eq 'True' }).Count + ' | ' + $BQ + 'modulos/functions/' + $BQ + ' |')
W ('| Triggers | ' + $trigs.Count + ' | ' + @($trigs | Where-Object { $_.escreve -eq 'True' }).Count + ' | ' + $BQ + 'modulos/triggers/' + $BQ + ' |')
W ('| Views | ' + $views.Count + ' | ' + @($views | Where-Object { $_.escreve -eq 'True' }).Count + ' | ' + $BQ + 'modulos/views/' + $BQ + ' |')
W ('| **Total** | **' + $mods.Count + '** | **' + $escrev.Count + '** | |')
W ''
W ('Arestas de dependência registradas em ' + $BQ + 'sys.sql_expression_dependencies' + $BQ + ': **' + $deps.Count + '**.')
W ''
W '### Como ler a coluna "Escreve"'
W ''
W 'A marcação foi calculada **sobre o texto já baixado**, procurando os verbos de escrita'
W '(INSERT / UPDATE / DELETE / MERGE / TRUNCATE) fora do servidor — nenhuma consulta de escrita'
W 'foi enviada ao banco. A coluna "Alvos" lista as tabelas que aparecem imediatamente depois desses'
W 'verbos e que existem de fato no banco. É o mapa de **onde a lógica de negócio mora dentro do banco**,'
W 'em vez de morar na aplicação.'
W ''
W ('Resultado central: **nenhuma das ' + $funcs.Count + ' functions e nenhuma das ' + $views.Count +
  ' views escreve**. Toda a escrita em tabela vinda de código T-SQL está concentrada em **' +
  $procsEscrevem + ' procedures e 1 trigger**.')
W ''

# ---------- o trigger ----------
W '## O único trigger do banco: `VTC_T_AUDITORIA`'
W ''
foreach ($t in $trigs) {
    W ('- **Tabela**: ' + $BQ + $t.tabela_pai + $BQ + '  |  **Evento**: AFTER INSERT, UPDATE, DELETE  |  **' + $t.linhas + ' linhas**')
    W ('- **Criado em** ' + $t.create_date.Substring(0, 10) + ', **alterado em** ' + $t.modify_date.Substring(0, 10) +
       ' — a autoria está declarada em comentário no próprio código (André Rizzatti, 15/08/2019)')
}
W ''
W @'
**O que ele faz, em prosa.** É um gatilho de *auditoria de propriedades do cliente*. Quando um
registro de `IV_ClientePropr` (as "propriedades" de um cliente: origem da receita, tratores,
implementos, colhedoras, NJUR) é inserido ou alterado, o trigger:

1. Lê a linha afetada em `inserted` para variáveis escalares;
2. Descobre o **assistente** do vendedor que fez a alteração consultando `IV_GlobalPar` com
   `SeqGlbPar = 12` ("Auditoria"), casando `Campo2` com o usuário da alteração e trazendo a
   empresa por `GE_EMPRESA.NomeReduzido`;
3. Verifica se **já existe agenda de auditoria em aberto** para aquela pessoa — `IV_Agenda` com
   `Acao = 358` ("Validação da Propriedade") e `Realizada = 'N'`, ligada por
   `IV_ProcLink.LinkDocto = 'AUDIT'`;
4. Se houver assistente, não houver agenda aberta e a propriedade for uma das cinco codificadas
   **em números literais no corpo do trigger** (`9400` Origem da Receita, `9402` Trator,
   `9406` Implemento, `9411` Colhedora, `9414` NJUR), monta uma linha delimitada por `|` usando
   `dbo.f_s_dado` / `dbo.f_s_dado_N` e **grava em `gep_import`** pedindo a inclusão de um registro
   em `IV_HISTORICO`, com `dtageracao = GETDATE() - 15 minutos` para que o Servidor de Processos
   pegue o registro já no ciclo seguinte.

**Três observações que importam para a substituição:**

- Ele **não trata DELETE**: o ramo de exclusão só executa `PRINT 'DELETANDO'`, embora esteja
  declarado `AFTER INSERT, UPDATE, DELETE`.
- Ele **assume uma única linha por operação** (`SELECT @var = I.coluna FROM inserted`). Num UPDATE
  que atinja várias linhas, apenas a última linha lida é auditada — silenciosamente.
- Ele **não escreve direto no histórico**: enfileira em `gep_import`, a mesma fila genérica usada
  pelas integrações. Ou seja, até a auditoria interna do CRM trafega pelo barramento de importação.
'@
W ''

# ---------- famílias ----------
W '## Famílias funcionais'
W ''
$cls = @{}
foreach ($m in $mods) {
    foreach ($tag in ((Classifica $m) -split ', ')) {
        if (-not $cls.ContainsKey($tag)) { $cls[$tag] = 0 }
        $cls[$tag]++
    }
}
$descFam = @{
    'BPM'           = 'toca IV_Agenda / IV_Historico / IV_Processo / IV_AcaoAuto / IV_ProcDado — o motor de workflow'
    'Integração'    = 'prefixos IMP_ / X_ / PR_VTC_INT / PR_INT, ou grava em GEP_Import, EXT_* e X_TOTVS_*'
    'Segmentação'   = 'toca as tabelas IVS_ (departamento, carteira, cobertura de clientes)'
    'Relatório'     = 'prefixos BI_ / VW_REL / TBA / X_CRM_BI — alimentam QlikView, relatórios QRP e planilhas'
    'SSMS (nativo)' = 'objetos de diagrama do próprio SQL Server Management Studio; não fazem parte do produto'
    'Teste/backup'  = 'nome contendo TESTE ou BKP — resíduo que deveria ter sido removido'
    '—'             = 'sem classificação automática (em sua maioria views de consulta)'
}
W '| Família | Objetos | O que caracteriza |'
W '|---|---:|---|'
foreach ($k in ($cls.Keys | Sort-Object { -$cls[$_] })) {
    W ('| ' + $k + ' | ' + $cls[$k] + ' | ' + $(if ($descFam.ContainsKey($k)) { $descFam[$k] } else { '' }) + ' |')
}
W ''

# ---------- procedures ----------
W ('## Stored procedures (' + $procs.Count + '), da mais recente para a mais antiga')
W ''
W '| # | Procedure | Modificada | Linhas | Escreve | Alvos de escrita | Família |'
W '|---:|---|---|---:|:-:|---|---|'
$i = 0
foreach ($p in $procs) {
    $i++
    $alvos = if ($p.alvos_escrita) { $BQ + ($p.alvos_escrita -replace '; ', ($BQ + ', ' + $BQ)) + $BQ } else { '—' }
    W ('| ' + $i + ' | ' + $BQ + $p.nome + $BQ + ' | ' + $p.modify_date.Substring(0, 10) + ' | ' + $p.linhas + ' | ' +
        $(if ($p.escreve -eq 'True') { 'SIM' } else { '—' }) + ' | ' + $alvos + ' | ' + (Classifica $p) + ' |')
}
W ''

# ---------- functions ----------
W ('## Functions escalares (' + $funcs.Count + ')')
W ''
W 'Nenhuma escreve. São utilitários de formatação (`fva_*`, `fn_*`), extração de dados do BPM'
W '(`fIV_*`, `fva_Get*`) e serialização para a fila de importação (`f_s_Dado`, `f_s_Dado_N`,'
W '`f_s_Campo`) — estas últimas são a "cola" usada pelo trigger e por várias procedures para montar'
W 'as linhas de largura fixa que alimentam `gep_import`.'
W ''
W '| Function | Linhas | Modificada | Tabelas referenciadas |'
W '|---|---:|---|---|'
foreach ($f in $funcs) {
    W ('| ' + $BQ + $f.nome + $BQ + ' | ' + $f.linhas + ' | ' + $f.modify_date.Substring(0, 10) + ' | ' +
        $(if ($f.tabelas_ref) { $f.tabelas_ref } else { '—' }) + ' |')
}
W ''

# ---------- views ----------
$viewsGrandes = @($views | Where-Object { [int]$_.linhas -ge 80 })
W ('## Views (' + $views.Count + ')')
W ''
W ('Nenhuma view escreve, mas várias concentram lógica de negócio pesada. As **' + $viewsGrandes.Count +
  ' views com 80 linhas ou mais** são, na prática, relatórios compilados dentro do banco — e o'
)
W 'principal risco de reescrita silenciosa quando o CRM for substituído.'
W ''
W '### Views com 80 linhas ou mais (lógica de negócio embutida)'
W ''
W '| View | Linhas | Família | Tabelas referenciadas |'
W '|---|---:|---|---|'
foreach ($v in ($viewsGrandes | Sort-Object { [int]$_.linhas } -Descending)) {
    W ('| ' + $BQ + $v.nome + $BQ + ' | ' + $v.linhas + ' | ' + (Classifica $v) + ' | ' +
        $(if ($v.tabelas_ref) { $v.tabelas_ref } else { '—' }) + ' |')
}
W ''
W '### Todas as views'
W ''
W '| View | Linhas | Modificada | Família |'
W '|---|---:|---|---|'
foreach ($v in $views) {
    W ('| ' + $BQ + $v.nome + $BQ + ' | ' + $v.linhas + ' | ' + $v.modify_date.Substring(0, 10) + ' | ' + (Classifica $v) + ' |')
}
W ''

GravaUtf8 (Join-Path $R 'modulos\00-INDICE.md') $sb.ToString()
Write-Host ('gerado modulos/00-INDICE.md (' + $mods.Count + ' objetos)') -ForegroundColor Green

# ==========================================================================
#  2) ddl/linked-server.md
# ==========================================================================
$srv = LeCsv (Join-Path $R 'seguranca-banco\linked-servers.csv')
$depRem = @($deps | Where-Object { $_.servidor_ref })

$sb = New-Object System.Text.StringBuilder
W '# Linked servers do banco CRM'
W ''
W '| Nome | Produto | Provider | Data source | Catalog | É linked | Acesso a dados | RPC out | Alterado em |'
W '|---|---|---|---|---|:-:|:-:|:-:|---|'
foreach ($s in $srv) {
    W ('| ' + $BQ + $s.name + $BQ + ' | ' + $s.product + ' | ' + $s.provider + ' | ' + $s.data_source + ' | ' +
        $(if ($s.catalog) { $s.catalog } else { '—' }) + ' | ' + $s.is_linked + ' | ' +
        $s.is_data_access_enabled + ' | ' + $s.is_rpc_out_enabled + ' | ' + $s.modify_date + ' |')
}
W ''
W @'
## Leitura

- **`COLWCRM`** não é um linked server: é o **próprio servidor local** (`is_linked = False`).
  O nome revela a origem histórica da instalação (Colorado Máquinas, antes da Tracbel Agro) e
  reaparece no argumento do job `VORTICOSERVER` (`COLWCRMBD`).
- **`TOTVS`** é o único linked server de verdade. Aponta para `totvs6` via **MSDASQL**, ou seja, um
  **DSN ODBC configurado no Windows do servidor de banco**. A string de conexão efetiva (driver,
  host, porta, usuário) **não está no SQL Server**: está no registro/ODBC do host. Isso importa —
  a integração com o ERP depende de configuração que vive fora do banco e fora de qualquer
  repositório versionado.
- `is_rpc_out_enabled = False` no TOTVS: o CRM **lê** do ERP, não executa procedures remotas.
- `sys.linked_logins` devolveu 0 linhas para o login de leitura, então **não foi possível ver o
  mapeamento de credenciais** do linked server. É um item para o DBA.
- `provider_string` veio vazia para os dois servidores; se algum dia vier preenchida, o script
  `06-seguranca-banco.ps1` já mascara usuário e senha antes de gravar o CSV.

## Objetos que dependem do linked server

'@
if ($depRem.Count -eq 0) {
    W '_Nenhuma dependência remota registrada._'
} else {
    $modPorId = @{}
    foreach ($m in $mods) { $modPorId[$m.object_id] = $m.nome }
    W '| Objeto que referencia | Servidor | Banco remoto | Entidade remota |'
    W '|---|---|---|---|'
    foreach ($d in ($depRem | Sort-Object entidade_ref)) {
        $nome = if ($modPorId.ContainsKey($d.referencing_id)) { $modPorId[$d.referencing_id] } else { '(object_id ' + $d.referencing_id + ')' }
        W ('| ' + $BQ + $nome + $BQ + ' | ' + $d.servidor_ref + ' | ' + $d.banco_ref + ' | ' + $BQ + $d.entidade_ref + $BQ + ' |')
    }
    W ''
    W ('São **' + $depRem.Count + ' referências remotas**, todas para o banco ' + $BQ + 'TMPRD' + $BQ + ' do TOTVS.')
    W ''
    W ('> **Cuidado ao interpretar este número.** O catálogo do SQL Server só registra a dependência'
    )
    W '> quando o objeto foi compilado com o nome de quatro partes (`TOTVS.TMPRD.dbo.tabela`).'
    W '> Consultas montadas com `OPENQUERY` ou SQL dinâmico **não aparecem aqui** — a superfície real'
    W '> de integração com o ERP pode ser maior.'
}
W ''
GravaUtf8 (Join-Path $R 'ddl\linked-server.md') $sb.ToString()
Write-Host 'gerado ddl/linked-server.md' -ForegroundColor Green

# ==========================================================================
#  3) volumetria/00-INDICE.md
# ==========================================================================
$lin  = LeCsv (Join-Path $R 'volumetria\linhas-por-tabela.csv')
$disc = LeCsv (Join-Path $R 'volumetria\tamanho-em-disco.csv')
$arq  = LeCsv (Join-Path $R 'volumetria\arquivos-do-banco.csv')
$cres = LeCsv (Join-Path $R 'volumetria\crescimento-anual.csv')
$ult  = LeCsv (Join-Path $R 'volumetria\ultima-atividade.csv')
$comp = LeCsv (Join-Path $R 'volumetria\comparacao-junho.csv')

$totalLinhas = ($lin | Measure-Object linhas -Sum).Sum
$porTabelaKb = @{}
foreach ($d in $disc) {
    if (-not $porTabelaKb.ContainsKey($d.tabela)) { $porTabelaKb[$d.tabela] = [int64]0 }
    $porTabelaKb[$d.tabela] += [int64]$d.reservado_kb
}
$totalKb = ($porTabelaKb.Values | Measure-Object -Sum).Sum

$sb = New-Object System.Text.StringBuilder
W '# Volumetria do banco CRM'
W ''
W ('Coletado em ' + (Get-Date -Format 'yyyy-MM-dd HH:mm') + '. Fonte: `sys.partitions` e `sys.allocation_units`.')
W ''
W '> `sys.dm_db_partition_stats` está **negado** para o login de leitura (falta `VIEW DATABASE STATE`).'
W '> As views de catálogo usadas aqui devolvem os mesmos números de linhas e de páginas.'
W ''
W '| Métrica | Valor |'
W '|---|---:|'
W ('| Tabelas | ' + $lin.Count + ' |')
W ('| Linhas somadas | ' + ('{0:N0}' -f $totalLinhas) + ' |')
W ('| Espaço reservado | ' + ('{0:N1} GB' -f ($totalKb / 1024 / 1024)) + ' |')
foreach ($a in $arq) {
    W ('| Arquivo ' + $a.type_desc + ' ' + $BQ + $a.arquivo_logico + $BQ + ' | ' + $a.tamanho_mb +
       ' MB (crescimento ' + $a.crescimento + ', máximo ' + $a.tamanho_maximo + ') |')
}
W ''
W '## As 25 maiores tabelas'
W ''
W '| # | Tabela | Linhas | Reservado (MB) |'
W '|---:|---|---:|---:|'
$i = 0
foreach ($t in ($lin | Sort-Object { [int64]$_.linhas } -Descending | Select-Object -First 25)) {
    $i++
    $kb = if ($porTabelaKb.ContainsKey($t.tabela)) { $porTabelaKb[$t.tabela] } else { 0 }
    W ('| ' + $i + ' | ' + $BQ + $t.tabela + $BQ + ' | ' + ('{0:N0}' -f [int64]$t.linhas) + ' | ' + ('{0:N1}' -f ($kb / 1024)) + ' |')
}
W ''
W '## Crescimento por ano'
W ''
$tabsCres = @($cres | Select-Object -ExpandProperty tabela -Unique)
$anos = @($cres | Select-Object -ExpandProperty ano -Unique | Where-Object { [int]$_ -ge 2019 -and [int]$_ -le 2030 } | Sort-Object)
W ('| Tabela | Coluna de data | ' + ($anos -join ' | ') + ' |')
W ('|---|---|' + (($anos | ForEach-Object { '---:' }) -join '|') + '|')
foreach ($t in $tabsCres) {
    $linhasT = @($cres | Where-Object { $_.tabela -eq $t })
    $col = $linhasT[0].coluna_data
    $cells = foreach ($a in $anos) {
        $x = $linhasT | Where-Object { $_.ano -eq $a } | Select-Object -First 1
        if ($x) { '{0:N0}' -f [int64]$x.linhas } else { '—' }
    }
    W ('| ' + $BQ + $t + $BQ + ' | ' + $BQ + $col + $BQ + ' | ' + ($cells -join ' | ') + ' |')
}
W ''
W '## Última atividade por tabela'
W ''
W '| Situação | Tabelas |'
W '|---|---:|'
foreach ($s in ($ult | Group-Object situacao | Sort-Object Count -Descending)) {
    W ('| ' + $s.Name + ' | ' + $s.Count + ' |')
}
W ''
W 'Critério: `MAX()` da coluna de data de inclusão mais provável de cada tabela. **viva** = recebeu'
W 'linha nos últimos 90 dias; **fria** = entre 90 e 365 dias; **PARADA** = mais de um ano sem linha nova.'
W ''
W '### Tabelas PARADAS há mais de um ano com mais de 10.000 linhas'
W ''
W '| Tabela | Linhas | Primeira | Última | Coluna |'
W '|---|---:|---|---|---|'
foreach ($u in ($ult | Where-Object { $_.situacao -like 'PARADA*' -and [int64]$_.linhas -gt 10000 } | Sort-Object { [int64]$_.linhas } -Descending)) {
    W ('| ' + $BQ + $u.tabela + $BQ + ' | ' + ('{0:N0}' -f [int64]$u.linhas) + ' | ' + $u.primeira_data + ' | ' +
        $u.ultima_data + ' | ' + $BQ + $u.coluna_data + $BQ + ' |')
}
W ''
W '## Comparação com o snapshot de 2025-06 (`vortice-crm-agent/schema/tabelas.csv`)'
W ''
W 'As 20 tabelas que mais cresceram entre o snapshot e agora:'
W ''
W '| Tabela | Agora | Snapshot | Delta | % |'
W '|---|---:|---:|---:|---:|'
foreach ($c in ($comp | Where-Object { $_.delta -ne '' } | Sort-Object { [int64]$_.delta } -Descending | Select-Object -First 20)) {
    W ('| ' + $BQ + $c.tabela + $BQ + ' | ' + ('{0:N0}' -f [int64]$c.linhas_agora) + ' | ' +
        ('{0:N0}' -f [int64]$c.linhas_snapshot) + ' | ' + ('{0:N0}' -f [int64]$c.delta) + ' | ' + $c.pct + ' |')
}
W ''
GravaUtf8 (Join-Path $R 'volumetria\00-INDICE.md') $sb.ToString()
Write-Host 'gerado volumetria/00-INDICE.md' -ForegroundColor Green
