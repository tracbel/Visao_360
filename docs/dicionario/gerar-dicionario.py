# -*- coding: utf-8 -*-
"""
==============================================================================
 GERADOR DO DICIONARIO DE DADOS - Vortice CRM (banco `CRM`, SQL Server)
==============================================================================

O QUE FAZ
---------
Le o catalogo de metadados extraido do banco (pasta `schema/*.csv` do repo
`vortice-crm-agent`) e escreve, em `docs/dicionario/`, um dicionario de dados
completo em Markdown PT-BR:

    00-INDICE.md          visao geral, resumo por modulo, classes, hubs, indice
    <MODULO>.md           uma pagina por prefixo de tabela (IV dividido em 4)
    GRAFO-FK.md           diagramas mermaid do nucleo relacional
    LACUNAS.md            o que o CSV nao cobre + tabelas sem funcao documentada

E DETERMINISTICO E REEXECUTAVEL: rodar de novo apos atualizar os CSVs
regenera tudo. Nenhuma informacao e inventada - tudo vem dos CSVs; as
descricoes funcionais sao extraidas dos documentos-fonte (SCHEMA_MAP.md,
REGRAS-DE-NEGOCIO.md, docs/pesquisa/*.md) ou marcadas como `(inferido)` /
`(nao documentado)`.

COMO RODAR
----------
    python docs/dicionario/gerar-dicionario.py

    # apontando para outro checkout do agente:
    python docs/dicionario/gerar-dicionario.py --agente C:/caminho/vortice-crm-agent

REQUISITOS: Python 3.8+ (so biblioteca padrao). Testado em 3.14.

FONTES (todas somente-leitura; o script NUNCA escreve em `vortice-crm-agent/`)
-----------------------------------------------------------------------------
    schema/tabelas.csv   tabela, qtd_colunas, linhas
    schema/colunas.csv   tabela, ord, coluna, tipo, tamanho, precisao, escala,
                         nullable, valor_default
    schema/pks.csv       tabela, coluna, ord
    schema/fks.csv       fk_nome, tabela_origem, coluna_origem, tabela_destino,
                         coluna_destino
    schema/views.csv     view_name
    schema/hubs.csv      tabela_referenciada, qtd_fks_apontando
    SCHEMA_MAP.md              mapa semantico curado
    docs/REGRAS-DE-NEGOCIO.md  regras do BPM confirmadas em producao
    <repo>/docs/pesquisa/*.md  15 relatorios funcionais

Todos os CSVs tem BOM UTF-8 -> abertos com encoding='utf-8-sig'.
==============================================================================
"""

import argparse
import csv
import datetime
import glob
import io
import os
import re
import sys
from collections import Counter, OrderedDict, defaultdict

# ==========================================================================
# 1. CAMINHOS
# ==========================================================================

AQUI = os.path.dirname(os.path.abspath(__file__))          # docs/dicionario
DOCS = os.path.dirname(AQUI)                               # docs
REPO = os.path.dirname(DOCS)                               # tracbel-crm
# Palpite padrao para o repo do agente (irmao de tracbel-crm)
AGENTE_PADRAO = os.path.join(os.path.dirname(REPO), 'vortice-crm-agent')


def parse_args():
    p = argparse.ArgumentParser(description='Gera o dicionario de dados do Vortice CRM.')
    p.add_argument('--agente', default=AGENTE_PADRAO,
                   help='raiz do repo vortice-crm-agent (contem schema/*.csv)')
    p.add_argument('--saida', default=AQUI,
                   help='pasta onde escrever os markdowns')
    return p.parse_args()


# ==========================================================================
# 2. LEITURA DOS CSVs
# ==========================================================================

def ler_csv(caminho):
    """Le um CSV com BOM UTF-8 e devolve lista de dicts."""
    with io.open(caminho, 'r', encoding='utf-8-sig', newline='') as f:
        return list(csv.DictReader(f))


def int0(v):
    """Converte para int tolerando vazio/None."""
    try:
        return int(str(v).strip())
    except (TypeError, ValueError):
        return 0


def fmt_num(n):
    """1234567 -> 1.234.567 (padrao PT-BR)."""
    return '{:,}'.format(int(n)).replace(',', '.')


def ancora(tabela):
    """Ancora GitHub para um heading `### NOME_TABELA`."""
    a = tabela.lower()
    a = re.sub(r'[^a-z0-9_\- ]', '', a)
    return a.replace(' ', '-')


# ==========================================================================
# 3. CLASSIFICACAO DE TABELAS
# ==========================================================================

# Padroes de lixo/backup, conforme SCHEMA_MAP.md secao "Tabelas a IGNORAR".
# Cada entrada e (regex compilado, motivo legivel). Ordem = especificidade.
RE_LIXO = [
    (re.compile(r'_bkp', re.I),            'copia de backup manual (padrao `*_BKP*`)'),
    (re.compile(r'_ita$', re.I),           'copia/variante manual (sufixo `_ITA`)'),
    (re.compile(r'_nelson$', re.I),        'copia/variante manual (sufixo `_NELSON`)'),
    (re.compile(r'^teste', re.I),          'tabela de teste (`teste*`)'),
    (re.compile(r'^mig', re.I),            'tabela de migracao pontual (`mig*`)'),
    (re.compile(r'^j1_', re.I),            'legado J1, fora do modelo Vortice'),
    (re.compile(r'^dual$', re.I),          'tabela tecnica de 1 linha (compat. Oracle DUAL)'),
    (re.compile(r'^sysdummy$', re.I),      'tabela tecnica dummy'),
    (re.compile(r'^sysconvert\d*$', re.I), 'tabela tecnica de conversao'),
    (re.compile(r'^sysdiagrams$', re.I),   'diagramas do SSMS (metadado da ferramenta)'),
    (re.compile(r'^espaco', re.I),         'medicao de espaco em disco'),
    (re.compile(r'^contas2?$', re.I),      'planilha importada solta'),
    (re.compile(r'\$$'),                   'importacao de planilha Excel (sufixo `$`)'),
    (re.compile(r'^ww$', re.I),            'tabela de rascunho'),
    (re.compile(r'^andre$', re.I),         'tabela pessoal (nome de usuario em minusculo)'),
    (re.compile(r'^temp$', re.I),          'tabela temporaria'),
    (re.compile(r'_bkp\d', re.I),          'copia de backup manual datada'),
    (re.compile(r'^iv_resultado_\d+$', re.I), 'copia pontual de IV_Resultado'),
]

RE_FORMULARIO = re.compile(r'^IV_Q_', re.I)

RE_STAGING = [
    (re.compile(r'^IMP_', re.I),     'staging de importacao do ERP'),
    (re.compile(r'^X_TOTVS_', re.I), 'staging/extrato do ERP TOTVS'),
    (re.compile(r'^X_T_', re.I),     'staging (tabela) da integracao TOTVS'),
    (re.compile(r'^X_V_', re.I),     'staging (materializacao de view) da integracao TOTVS'),
    (re.compile(r'^JDE_', re.I),     'staging da integracao JD Edwards'),
]

# Limiares da classe `catalogo`: poucas linhas + muito referenciada.
CAT_MAX_LINHAS = 2000
CAT_MIN_FKS_ENTRANDO = 2

CLASSES = ['nucleo', 'catalogo', 'staging', 'formulario-materializado',
           'vazia', 'isolada', 'lixo/backup']

DESC_CLASSE = OrderedDict([
    ('nucleo', 'Tem linhas e participa do grafo relacional (aponta para outras tabelas '
               'ou e apontada por elas). E o dado vivo do sistema.'),
    ('catalogo', 'Poucas linhas (<= %d) e referenciada por >= %d chaves estrangeiras: '
                 'tabela de dominio/parametrizacao.' % (CAT_MAX_LINHAS, CAT_MIN_FKS_ENTRANDO)),
    ('staging', 'Area de pouso da integracao com ERP (`IMP_*`, `X_TOTVS_*`, `X_T_*`, '
                '`X_V_*`, `JDE_*`). Dado bruto antes de virar `EXT_*`.'),
    ('formulario-materializado', 'Tabela fisica `IV_Q_<Formulario>` gerada por DDL pelo motor '
                                 'de formularios: 1 coluna por questao respondida.'),
    ('vazia', '0 linhas no snapshot: recurso do produto nao usado na Tracbel, ou tabela '
              'nova/abandonada.'),
    ('isolada', 'Tem linhas mas nenhuma FK entrando nem saindo: nao se conecta ao modelo '
                'por integridade declarada (pode se conectar por convencao de nome).'),
    ('lixo/backup', 'Copia manual, teste, migracao ou legado. **Nunca usar em consulta de '
                    'producao** (SCHEMA_MAP.md).'),
])


def classificar(tabela, linhas, fks_saindo, fks_entrando):
    """Devolve (classe, motivo). A ORDEM das regras importa."""
    for rx, motivo in RE_LIXO:
        if rx.search(tabela):
            return 'lixo/backup', motivo
    if RE_FORMULARIO.match(tabela):
        return 'formulario-materializado', 'tabela fisica gerada pelo motor de formularios'
    for rx, motivo in RE_STAGING:
        if rx.match(tabela):
            return 'staging', motivo
    if linhas == 0:
        return 'vazia', 'sem linhas no snapshot'
    if linhas <= CAT_MAX_LINHAS and fks_entrando >= CAT_MIN_FKS_ENTRANDO:
        return 'catalogo', '%s linhas e %d FKs apontando' % (fmt_num(linhas), fks_entrando)
    if fks_saindo or fks_entrando:
        return 'nucleo', '%d FK(s) saindo, %d entrando' % (fks_saindo, fks_entrando)
    return 'isolada', 'nenhuma FK declarada'


# ==========================================================================
# 4. MODULOS (arquivos de saida)
# ==========================================================================

# Modulo = prefixo do nome ate o primeiro `_`, em MAIUSCULAS.
# Tabelas sem `_` vao para OUTROS. Alguns prefixos ganham nome mais falante.
ARQUIVO_POR_PREFIXO = {
    'X': 'X_TOTVS',
}

TITULO_MODULO = {
    'IV-1': 'IV-1 - Nucleo BPM: processo, agenda e historico',
    'IV-2': 'IV-2 - Formularios, questionarios e propriedades customizadas',
    'IV-3': 'IV-3 - Catalogos e parametrizacao do BPM',
    'IV-4': 'IV-4 - Demais tabelas do nucleo CRM',
    'GE': 'GE - Geral / plataforma (pessoas, usuarios, seguranca, logs, configuracao)',
    'EXT': 'EXT - Espelho de dados do ERP (equipamentos, notas, OS, titulos)',
    'GEP': 'GEP - Jobs, agendador, e-mail e sincronizacao',
    'IVS': 'IVS - Segmentacao, carteira e RFV',
    'IMP': 'IMP - Staging de importacao do ERP',
    'IVF': 'IVF - Financiamento, planos e propostas',
    'DMN': 'DMN - Gestao documental (Doc Manager)',
    'X_TOTVS': 'X_* - Integracao TOTVS (staging e materializacoes)',
    'JDE': 'JDE - Integracao JD Edwards',
    'IVC': 'IVC - Call center: equipes, atendentes e ramais',
    'GEL': 'GEL - Logistica: CEP, cidades e conversoes',
    'OUT': 'OUT - Modulo outbound / base auxiliar de pessoas',
    'IVP': 'IVP - Produtos, tabela de preco e criticas de pedido',
    'IVM': 'IVM - Materiais vinculados a processo',
    'VBI': 'VBI - Plano de contas para BI',
    'CBR': 'CBR - Cobranca (contas do cliente)',
    'IVT': 'IVT - Tabela de-para de integracao',
    'CLARITY': 'CLARITY - Integracao de telefonia (BINA)',
    'LOG': 'LOG - Log de integracao de faturamento',
    'ESPACO': 'ESPACO - Medicao de espaco em disco',
    'J1': 'J1 - Legado J1',
    'MIG': 'MIG - Tabelas de migracao',
    'TESTE': 'TESTE - Tabelas de teste',
    'OUTROS': 'OUTROS - Tabelas sem prefixo (nome sem `_`)',
}

# --- Subdivisao do prefixo IV (377 tabelas -> 4 arquivos) --------------------
# Avaliado NESTA ordem; a primeira regra que casar define o arquivo.

IV2_EXATAS = {
    'IV_Formulario', 'IV_Questao', 'IV_QuestaoLista', 'IV_Questionario',
    'IV_ClientePropCmpl', 'IV_Atributo', 'IV_AtribLista', 'IV_ClienteAtrib',
    'IV_ListSQL', 'IV_Pcte', 'IV_PcteAtrFx',
}
IV2_REGEX = [re.compile(r'^IV_Q_', re.I),
             re.compile(r'^IV_Propriedade', re.I),
             re.compile(r'^IV_PropriLista', re.I),
             re.compile(r'^IV_ClientePropr', re.I)]

IV3_EXATAS = {
    'IV_Acao', 'IV_AcaoAtendente', 'IV_AcaoAuto', 'IV_AcaoAutoCtrl', 'IV_AcaoCmpl',
    'IV_AcaoCtrl', 'IV_AcaoMon', 'IV_AcaoRem', 'IV_ACAOANEXA', 'IV_ACAOURACENARIO',
    'IV_Resultado', 'IV_ResultadoCmpl', 'IV_ResultadoInstr', 'IV_ResultadoReq',
    'IV_ResultadoWeb', 'IV_ResClasse', 'IV_ResEvtOut', 'IV_ResMsgPapel',
    'IV_ResParam', 'IV_ResVinc', 'IV_RESJOB', 'IV_ClasseRes',
    'IV_CodProcesso', 'IV_CodPrcEmpr', 'IV_CodProcComent',
    'IV_ProcFase', 'IV_ProcFaseMonit', 'IV_ProcPersp', 'IV_ProcPerspMonit',
    'IV_ProcResultado', 'IV_ProcSt', 'IV_ProcStatMonit', 'IV_ProcTpRel',
    'IV_Motivo', 'IV_Departamento', 'IV_Unidade', 'IV_TpPgto',
    'IV_TxtPadrao', 'IV_TxtPadraoUso', 'IV_TxtPadConta', 'IV_STATUS_DEPTO',
    'IV_GlobalPar', 'IV_GlobalParCtrl', 'IV_GlobalParLista',
    'IV_DoctoTipo', 'IV_DoctoApl', 'IV_DoctoAplUso', 'IV_TIPOCONTEUDO',
    'IV_RetProcRegra', 'IV_PAREVTEXTRES', 'IV_Evento', 'IV_EventoAcao',
    'IV_SegPerfil', 'IV_TAGCAD', 'IV_Recurso', 'IV_RecUso',
}

IV1_REGEX = [
    re.compile(r'^IV_Proc', re.I),
    re.compile(r'^IV_Agenda', re.I),
    re.compile(r'^IV_Agd', re.I),
    re.compile(r'^IV_AGD', re.I),
    re.compile(r'^IV_Hist', re.I),
]
IV1_EXATAS = {
    'IV_Interacao', 'IV_Ciencia', 'IV_PROCTAG', 'IV_PROCPESLINK',
    'IV_AtivAgenda', 'IV_AtivProc', 'IV_Distribui', 'IV_AtdBloq',
}


def modulo_de(tabela):
    """Devolve a chave do arquivo de modulo desta tabela."""
    up = tabela.upper()
    if up.startswith('IV_'):
        if tabela in IV2_EXATAS or any(r.match(tabela) for r in IV2_REGEX):
            return 'IV-2'
        if tabela in IV3_EXATAS:
            return 'IV-3'
        if tabela in IV1_EXATAS or any(r.match(tabela) for r in IV1_REGEX):
            return 'IV-1'
        return 'IV-4'
    if '_' not in tabela:
        return 'OUTROS'
    pref = tabela.split('_', 1)[0].upper()
    return ARQUIVO_POR_PREFIXO.get(pref, pref)


NOME_ARQUIVO = {
    'IV-1': 'IV-1-processo-agenda-historico.md',
    'IV-2': 'IV-2-formularios.md',
    'IV-3': 'IV-3-catalogos-bpm.md',
    'IV-4': 'IV-4-demais.md',
}


def arquivo_de(mod):
    return NOME_ARQUIVO.get(mod, mod + '.md')


# ==========================================================================
# 5. EXTRACAO DE DESCRICOES FUNCIONAIS DOS DOCUMENTOS-FONTE
# ==========================================================================
# Cinco niveis, do mais confiavel para o menos:
#   0) derivado deterministicamente da estrutura (tabelas `IV_Q_*` de formulario)
#   1) secao `### <TABELA>` seguida de `**Funcao:**` nos relatorios de pesquisa
#   2) mencao descritiva num documento (lista, linha de tabela md ou frase corrida)
#   3) inferencia por nome + colunas  -> marcado `(inferido)`
#   4) nada -> `(nao documentado - apurar com acesso ao vivo)`

def limpar(txt):
    """Normaliza um trecho de markdown para caber num paragrafo/celula."""
    txt = re.sub(r'\s+', ' ', txt).strip()
    return txt.replace('|', '/')


def cortar_frases(txt, max_frases=2, max_chars=420):
    """Mantem no maximo `max_frases` frases e `max_chars` caracteres."""
    frases = re.split(r'(?<=[.!?]) +', txt)
    out = ' '.join(frases[:max_frases]).strip()
    if len(out) > max_chars:
        out = out[:max_chars].rsplit(' ', 1)[0] + '...'
    return out


def carregar_fontes(agente, repo):
    """Devolve OrderedDict {nome_do_arquivo: texto} dos documentos semanticos."""
    fontes = OrderedDict()
    for c in [os.path.join(agente, 'SCHEMA_MAP.md'),
              os.path.join(agente, 'docs', 'REGRAS-DE-NEGOCIO.md')]:
        if os.path.exists(c):
            fontes[os.path.basename(c)] = io.open(
                c, encoding='utf-8', errors='replace').read()
    for c in sorted(glob.glob(os.path.join(repo, 'docs', 'pesquisa', '*.md'))):
        fontes[os.path.basename(c)] = io.open(
            c, encoding='utf-8', errors='replace').read()
    return fontes


# `**Funcao:**` aparece com e sem acento nos relatorios.
RE_FUNCAO = re.compile(r'\*\*Fun\w*:?\*\*\s*(.+?)(?:\n\s*\n|\Z)', re.S | re.U)

# Mencoes inline descritivas.
RE_INLINE = [
    # "- IV_Ciencia (5 col, 173.477) = confirmacao de leitura ..."
    re.compile(r'^\s*[-*]\s+`?([A-Za-z][A-Za-z0-9_$]{2,})`?\s*(?:\([^)]*\))?\s*'
               r'[=\u2014\u2013:-]\s+(.{20,320})$', re.M),
    # "IV_Ciencia (5 col, 173.477) = confirmacao ..."
    re.compile(r'`?\b([A-Za-z][A-Za-z0-9_$]{2,})`?\s*\([^)]{2,60}\)\s*=\s*'
               r'(.{20,280}?)(?:\.\s|\n)'),
    # linha de tabela markdown: | **Acao** | `IV_Acao` | ... | o tipo de tarefa |
    re.compile(r'^\|\s*\*{0,2}`?([A-Za-z][A-Za-z0-9_$]{2,})`?\*{0,2}\s*\|[^|\n]*\|\s*'
               r'([^|\n]{20,280}?)\s*\|', re.M),
]


def indexar_descricoes(fontes, tabelas):
    """Varre as fontes; devolve {tabela: (descricao, arquivo_fonte, nivel)}."""
    por_lower = {t.lower(): t for t in tabelas}
    achados = {}   # tabela -> (nivel, tamanho, descricao, arquivo)

    def registrar(nome, desc, arq, nivel):
        real = por_lower.get(nome.lower())
        if not real:
            return
        desc = cortar_frases(limpar(desc))
        if len(desc) < 20:
            return
        ant = achados.get(real)
        # nivel menor vence; empate -> descricao mais longa vence
        if ant is None or (nivel, -len(desc)) < (ant[0], -ant[1]):
            achados[real] = (nivel, len(desc), desc, arq)

    # --- Nivel 1: secoes `###` com `**Funcao:**` -----------------------------
    for arq, txt in fontes.items():
        for bloco in re.split(r'^#{2,4} ', txt, flags=re.M)[1:]:
            cabec = bloco.split('\n', 1)[0].strip()
            m = RE_FUNCAO.search(bloco)
            if not m:
                continue
            for nome in re.findall(r'[A-Za-z][A-Za-z0-9_$]{2,}', cabec):
                registrar(nome, m.group(1), arq, 1)

    # --- Nivel 2: mencoes inline --------------------------------------------
    for arq, txt in fontes.items():
        for pat in RE_INLINE:
            for m in pat.finditer(txt):
                registrar(m.group(1), m.group(2), arq, 2)

    # --- Nivel 2b: frase corrida que menciona a tabela ----------------------
    # Rede de seguranca para tabelas citadas em prosa (ex.: "IV_ObjFlow = 726
    # objetos do desenho do fluxo"). Exige um marcador descritivo na frase para
    # nao capturar mencao puramente incidental ("consultei X, Y e Z").
    faltantes = [t for t in tabelas if t not in achados]
    if faltantes:
        # Precisa de um verbo/marcador descritivo...
        rx_marcador = re.compile(
            r'\b([eé]|s[aã]o|guarda|guardam|controla|registra|armazena|cont[eê]m|'
            r'tabela|cadastro|cat[aá]logo|log de|v[ií]nculo|define|liga|fila|motor)\b', re.I)
        # ...e NAO pode ser uma frase meta (evidencia, licao, contagem, query).
        rx_meta = re.compile(
            r'^\W*(\*\*)?(li[cç][aã]o|evid[eê]ncia|contagem|query|select|'
            r'n[aã]o (investiguei|aprofundei|explorei|validei|testei|consegui|determinei)|'
            r'confirmei|validado|medido|medi |obs|nota|fonte|exemplo|hip[oó]tese|risco|'
            r'pergunta|prova|checklist|conclus|detalhamento|distribui|escala|pico|amostra|'
            r'ordem de grandeza|comparativo|resumo)', re.I)
        # Frase que so cita caminho de arquivo/CSV nao descreve nada.
        rx_ref_arquivo = re.compile(r'\.csv|\.md\b|[a-z]:\\\\|schema[/\\]', re.I)
        rx_nome_tab = re.compile(r'\b(?:IV|GE|EXT|GEP|IVS|IMP|IVF|DMN|IVC|GEL|OUT|IVP|IVM|'
                                 r'VBI|CBR|JDE|IVT|X)_[A-Za-z0-9_$]+')
        for arq, txt in fontes.items():
            for fr in re.split(r'(?<=[.!?;])\s+|\n', txt):
                if not (40 <= len(fr) <= 320):
                    continue
                if rx_meta.match(fr) or rx_ref_arquivo.search(fr):
                    continue
                if not rx_marcador.search(fr):
                    continue
                # Frase que lista muitas tabelas e enumeracao, nao definicao.
                if len(set(rx_nome_tab.findall(fr))) > 3:
                    continue
                fr_l = fr.lower()
                for t in faltantes:
                    pos = fr_l.find(t.lower())
                    # o nome tem de ser o SUJEITO: aparecer no comeco da frase
                    if 0 <= pos <= max(60, len(fr) // 2):
                        registrar(t, fr, arq, 2)

    return {t: (d, a, n) for t, (n, _l, d, a) in achados.items()}


# --- Nivel 3: inferencia por nome + colunas ---------------------------------
# Glossario dos fragmentos recorrentes na nomenclatura Vortice.
# Avaliado do mais especifico para o mais generico; primeira regra vence.

GLOSSARIO_ENTIDADE = [
    (r'PESSOA|PES(?![A-Z])', 'pessoa (cliente/prospect/contato)'),
    (r'PROCESSO|PROC', 'processo/oportunidade do BPM'),
    (r'AGENDA|AGD', 'agenda (tarefa do usuario)'),
    (r'HISTORICO|HIST', 'historico de interacoes'),
    (r'QUESTIONARIO|QUESTAO|FORMULARIO', 'formulario/questionario'),
    (r'RESULTADO|RES(?![A-Z])', 'resultado (desfecho de um andamento)'),
    (r'ACAO', 'acao (tipo de tarefa)'),
    (r'USUARIO|USR|USU', 'usuario do sistema'),
    (r'VENDEDOR|CEN(?![A-Z])', 'vendedor/CEN'),
    (r'ATENDENTE|ATD', 'atendente'),
    (r'CARTEIRA|CART', 'carteira de clientes'),
    (r'CAMPANHA|CAMP', 'campanha de marketing'),
    (r'PROJETO|PROJ', 'projeto'),
    (r'DOCTO|DOCUMENTO|DOC(?![A-Z])', 'documento'),
    (r'TITULO|TIT(?![A-Z])', 'titulo financeiro'),
    (r'COBRANCA|COBR|CBR', 'cobranca'),
    (r'NFS|NOTA|NF(?![A-Z])', 'nota fiscal'),
    (r'VEIC', 'veiculo/equipamento'),
    (r'PRODUTO|PROD', 'produto'),
    (r'MATERIAL|MAT(?![A-Z])', 'material'),
    (r'PEDIDO|PED(?![A-Z])', 'pedido'),
    (r'EMAIL|MAIL', 'e-mail'),
    (r'SMS|WHATSAPP|PUSH', 'mensageria (SMS/WhatsApp/push)'),
    (r'FONE|RAMAL|URA|BINA|CHAMADA', 'telefonia'),
    (r'CEP|CIDADE|CID(?![A-Z])|REGIAO|ROTA|PAIS|ESTADO', 'geografia/enderecamento'),
    (r'EMPRESA|EMPR', 'empresa (multiempresa)'),
    (r'DEPTO|DEPARTAMENTO', 'departamento'),
    (r'EQUIPE', 'equipe'),
    (r'PERMISSAO|PERM|POLSEG|SEGURANCA|SEG(?![A-Z])', 'seguranca/permissao'),
    (r'MENU|APLICACAO|MODULO|SISTEMA|TELA|FORM(?![A-Z])', 'metadado de aplicacao/tela'),
    (r'JOB|AGDEXEC|SYNC', 'job/sincronizacao'),
    (r'FINANC|PLANO|PRAZO|PROPOSTA', 'financiamento'),
    (r'SEGM|RFV|POTENCIAL|SCORE|CLASSE', 'segmentacao'),
    (r'ORDEM|SERVICO|OS(?![A-Z])', 'ordem de servico'),
    (r'CONTA', 'plano de contas'),
    (r'SELECAO|SEL(?![A-Z])', 'selecao/segmento de publico'),
    (r'PROPRIEDADE|PROPRI|ATRIB', 'propriedade/atributo customizado'),
    (r'FASE', 'fase do fluxo'),
    (r'STATUS|ST(?![A-Z])', 'status'),
    (r'MOTIVO', 'motivo'),
    (r'LEAD|FACEBOOK|RDSTATION', 'lead de marketing'),
    (r'TAG', 'marcador/tag'),
    (r'CONHEC', 'base de conhecimento'),
    (r'ATIV', 'atividade'),
    (r'EVENTO|EVT', 'evento'),
    (r'RECURSO|REC(?![A-Z])', 'recurso'),
    (r'FATURAMENTO|FATUR', 'faturamento'),
    (r'PARAM|CONFIG|GLOBALPAR', 'parametrizacao'),
    (r'MOBILE', 'aplicativo mobile'),
    (r'RELATORIO|REL(?![A-Z])|QVW|CONSULTA|CONS(?![A-Z])', 'relatorio/consulta'),
    (r'PGTO|PAGAMENTO', 'forma/condicao de pagamento'),
    (r'FERIADO|DIANAOUTIL|CALENDARIO', 'calendario / dias nao uteis'),
    (r'CANAL', 'canal de venda'),
    (r'CATEGORIA|NEGOCIO', 'categoria / linha de negocio'),
    (r'PRECO|TABPRECO', 'tabela de preco'),
    (r'UNIDADE', 'unidade de medida'),
    (r'ARQ(?![A-Z])|ARQUIVO', 'arquivo binario anexado'),
    (r'FILA', 'fila de processamento'),
    (r'NOTIFIC', 'notificacao'),
    (r'PABX', 'telefonia (PABX)'),
    (r'ACORDO', 'acordo comercial/financeiro'),
    (r'BONUS|COMISSAO|INCENTIVO|PREMIO', 'bonus/comissao'),
    (r'PROMO', 'promocao'),
    (r'BLOQ', 'bloqueio'),
    (r'IMPORT', 'importacao de dados'),
    (r'CONV(?![A-Z])|CONVERSAO', 'conversao/de-para'),
    (r'GRAFICO|TEMPLATE|PASTA|HELP', 'recurso de apresentacao/relatorio'),
    (r'MIDIA', 'midia/custo de midia'),
    (r'META(?![A-Z])', 'meta comercial'),
    (r'VERSAO|VRS(?![A-Z])', 'versionamento'),
    (r'ANEXO|ANEXA', 'anexo'),
]

GLOSSARIO_PAPEL = [
    (r'LOG$|^LOG|LOG_|LG(?![A-Z])|HST$|HIST$', 'uma trilha/log de alteracoes'),
    (r'LISTA$|LIST$|LISTA_', 'uma lista de opcoes de dominio'),
    (r'TIPO$|TP$|^TP|TIPO_|TIPO(?=[A-Z])', 'um catalogo de tipos'),
    (r'ITEM$|ITENS$|ITEM_', 'a filha 1:N de itens'),
    (r'VINC$|VINCULO|LINK$|LINK_|DEPARA', 'uma tabela de vinculo/de-para'),
    (r'CTRL$|CONTROLE', 'uma tabela de controle/condicoes de uso'),
    (r'CMPL$|COMPLEMENTO', 'um complemento do registro pai'),
    (r'MON$|MONIT', 'um monitoramento'),
    (r'AUTO$', 'uma regra de automacao'),
    (r'RELACAO', 'um relacionamento'),
    (r'OBS$', 'um bloco de observacoes'),
    (r'GRUPO|GRP', 'um agrupamento'),
    (r'PADRAO|PAD$', 'um texto/modelo padrao'),
    (r'LOTE', 'um processamento em lote'),
    (r'CRIT|CRITERIO', 'um criterio configuravel'),
]

DESC_PREFIXO = {
    'IV': 'nucleo CRM/BPM',
    'GE': 'geral/plataforma',
    'EXT': 'espelho de dados do ERP',
    'GEP': 'jobs e integracao',
    'IVS': 'segmentacao/carteira',
    'IMP': 'staging de importacao',
    'IVF': 'financiamento',
    'DMN': 'gestao documental',
    'IVC': 'call center',
    'GEL': 'logistica/CEP',
    'OUT': 'modulo outbound',
    'IVP': 'produtos/precos',
    'IVM': 'materiais',
    'VBI': 'BI/plano de contas',
    'CBR': 'cobranca',
    'JDE': 'integracao JD Edwards',
    'X': 'integracao TOTVS',
    'IVT': 'de-para de integracao',
}

# Colunas cuja presenca revela vinculo semantico (usadas na inferencia).
PISTAS_COLUNA = [
    ('SEQPESSOA', 'vinculo com pessoa (`SeqPessoa`)'),
    ('PROCESSO', 'vinculo com processo (`Processo`)'),
    ('SEQAGENDA', 'vinculo com agenda (`SeqAgenda`)'),
    ('SEQHISTORICO', 'vinculo com historico (`SeqHistorico`)'),
    ('SEQUSUARIO', 'vinculo com usuario (`SeqUsuario`)'),
    ('NROEMPRESA', 'escopo multiempresa (`NroEmpresa`)'),
    ('SEQFORMULARIO', 'vinculo com formulario (`SeqFormulario`)'),
    ('IDVEIC', 'vinculo com equipamento (`IdVeic`)'),
]


def inferir(tabela, colunas, classe):
    """Descricao inferida do nome + colunas, ou None se nao houver base."""
    up = tabela.upper()
    pref = tabela.split('_', 1)[0].upper() if '_' in tabela else ''
    ent = next((txt for rx, txt in GLOSSARIO_ENTIDADE if re.search(rx, up)), None)
    papel = next((txt for rx, txt in GLOSSARIO_PAPEL if re.search(rx, up)), None)
    nomes_col = {c['coluna'].upper() for c in colunas}
    pistas = [txt for chave, txt in PISTAS_COLUNA if chave in nomes_col]

    if not ent and not papel:
        return None   # sem base -> vira "(nao documentado)"

    if papel and ent:
        frase = 'Pelo nome, e %s relacionada a %s' % (papel, ent)
    elif ent:
        frase = 'Pelo nome, guarda dados de %s' % ent
    else:
        frase = 'Pelo nome, e %s' % papel
    mod = DESC_PREFIXO.get(pref)
    if mod:
        frase += ', no modulo `%s` (%s)' % (pref, mod)
    frase += '.'
    if pistas:
        frase += ' As colunas confirmam ' + ', '.join(pistas[:3]) + '.'
    if classe == 'vazia':
        frase += ' Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.'
    return frase


# ==========================================================================
# 6. FORMATACAO DA TABELA DE COLUNAS
# ==========================================================================

# Padroes recorrentes de coluna -> observacao automatica.
PADROES_COLUNA = [
    (r'^DTAINCLUSAO$',              'auditoria de inclusao (data)'),
    (r'^USUINCLUSAO$|^USUINCLUIU$', 'auditoria de inclusao (usuario)'),
    (r'^DTAALTERACAO$',             'auditoria de alteracao (data)'),
    (r'^USUALTERACAO$|^USUALTEROU$', 'auditoria de alteracao (usuario)'),
    (r'^DTAINATIVACAO$',            'data de inativacao'),
    (r'^DTAEXCLUSAO$',              'data de exclusao logica'),
    (r'^NROEMPRESA$',               'multiempresa - filial/empresa'),
    (r'^SEQPESSOA$|^PESSOA$',       'pessoa (`GE_Pessoa.SeqPessoa`)'),
    (r'^SEQUSUARIO$',               'usuario do sistema (`GE_Usuario.SeqUsuario`)'),
    (r'^CODUSUARIO$',               'login do usuario (varchar)'),
    (r'^PROCESSO$',                 'numero do processo (`IV_Processo.Processo`)'),
    (r'^CODPROCESSO$',              'TIPO de fluxo (41/50) - nao e o numero do processo'),
    (r'^SEQHISTORICO$',             'historico (`IV_Historico.SeqHistorico`)'),
    (r'^SEQAGENDA$',                'agenda (`IV_Agenda.SeqAgenda`)'),
    (r'^SEQFORMULARIO$|^FORMULARIO$', 'formulario (`IV_Formulario.SeqFormulario`)'),
    (r'^SEQQUESTIONARIO$',          'questionario (`IV_Questionario.SeqQuestionario`)'),
    (r'^ACAO$',                     'acao (`IV_Acao.Acao`)'),
    (r'^RESULTADO$',                'resultado (`IV_Resultado.Resultado`)'),
    (r'^RESULTADOCMPL$',            'resultado complementar (texto livre; ver defeito 2.10)'),
    (r'^VENDEDOR$',                 'guarda o `CODVENDEDOR` (varchar), nao o SEQVENDEDOR'),
    (r'^CODVENDEDOR$',              'codigo/login do vendedor (varchar)'),
    (r'^SEQVENDEDOR$',              'vendedor (`IV_VENDEDOR.SEQVENDEDOR`)'),
    (r'^IDVEIC$',                   'equipamento (`EXT_Veic.IdVeic`)'),
    (r'^SEQDOCTO$',                 'documento (`DMN_Doc.SeqDocto`)'),
    (r'^EMUSO$',                    'flag de registro/regra ativa (0 = desligada)'),
    (r'^STATUS$',                   'status - validar dominio real por tabela'),
    (r'^ORIGEM$|^ULTORIGEM$',       'sistema de origem do dado'),
    (r'^OBS$|^OBSERVACAO$',         'texto livre'),
    (r'^DESCRICAO$',                'descricao do registro'),
    (r'^LINKDOCTO$|^LINKNRO$|^LINKSERIE$', 'chave de vinculo com documento do ERP'),
    (r'^LATITUDE$|^LONGITUDE$',     'geolocalizacao'),
    (r'^DTAREALIZACAO$',            'data de realizacao'),
    (r'^SEQDEPTO$|^DEPARTAMENTO$',  'departamento'),
    (r'^SEQCARTEIRA$',              'carteira (`IVS_Carteira`)'),
    (r'^SEQPROJETO$',               'projeto (`IV_Projeto`)'),
    (r'^FASE$|^FASEORDEM$',         'fase do fluxo (`IV_ProcFase`)'),
]
PADROES_COLUNA = [(re.compile(rx), txt) for rx, txt in PADROES_COLUNA]


def tipo_fmt(c):
    """Monta `varchar(60)` / `numeric(18,0)` / `datetime` a partir do CSV."""
    t = c['tipo']
    tam = (c.get('tamanho') or '').strip()
    prec = (c.get('precisao') or '').strip()
    esc = (c.get('escala') or '').strip()
    if tam:
        return '%s(max)' % t if tam == '-1' else '%s(%s)' % (t, tam)
    if prec:
        return '%s(%s,%s)' % (t, prec, esc or '0')
    return t


def obs_coluna(tabela, col, pkset, fk_por_col):
    """Monta a celula `observacao` de uma linha da tabela de colunas."""
    partes = []
    nome = col['coluna']
    if nome in pkset:
        partes.append('**PK**')
    # dedupe: fks.csv pode trazer a mesma FK duas vezes (constraints duplicadas no banco)
    for destino in sorted(set(fk_por_col.get(nome, []))):
        partes.append('FK -> `%s.%s`' % destino)
    up = nome.upper()
    for rx, txt in PADROES_COLUNA:
        if rx.match(up):
            # Nao anotar "pessoa (GE_Pessoa.SeqPessoa)" dentro da propria GE_Pessoa.
            if ('`%s.' % tabela) not in txt:
                partes.append(txt)
            break
    return '; '.join(partes)


# ==========================================================================
# 7. MAIN
# ==========================================================================

def main():
    args = parse_args()
    agente, saida = args.agente, args.saida
    sch = os.path.join(agente, 'schema')
    if not os.path.isdir(sch):
        sys.exit('ERRO: pasta de schema nao encontrada: %s\n'
                 'Use --agente para apontar a raiz do repo vortice-crm-agent.' % sch)
    if not os.path.isdir(saida):
        os.makedirs(saida)

    # ---- 7.1 carga -------------------------------------------------------
    tabelas_csv = os.path.join(sch, 'tabelas.csv')
    r_tab = ler_csv(tabelas_csv)
    r_col = ler_csv(os.path.join(sch, 'colunas.csv'))
    r_pk = ler_csv(os.path.join(sch, 'pks.csv'))
    r_fk = ler_csv(os.path.join(sch, 'fks.csv'))
    r_vw = ler_csv(os.path.join(sch, 'views.csv'))
    r_hub = ler_csv(os.path.join(sch, 'hubs.csv'))

    # Data do snapshot = mtime do tabelas.csv (o banco nao esta acessivel aqui).
    dt_snap = datetime.datetime.fromtimestamp(os.path.getmtime(tabelas_csv))
    snapshot = dt_snap.strftime('%d/%m/%Y')
    gerado_em = datetime.datetime.now().strftime('%d/%m/%Y %H:%M')

    tabelas = [r['tabela'] for r in r_tab]
    linhas = {r['tabela']: int0(r['linhas']) for r in r_tab}
    qtd_col = {r['tabela']: int0(r['qtd_colunas']) for r in r_tab}

    colunas = defaultdict(list)
    for r in r_col:
        colunas[r['tabela']].append(r)
    for t in colunas:
        colunas[t].sort(key=lambda r: int0(r['ord']))

    pks = defaultdict(list)
    for r in sorted(r_pk, key=lambda r: int0(r['ord'])):
        pks[r['tabela']].append(r['coluna'])

    fks_saindo = defaultdict(list)     # origem  -> [(col_orig, tab_dest, col_dest, nome)]
    fks_entrando = defaultdict(list)   # destino -> [(tab_orig, col_orig, col_dest)]
    for r in r_fk:
        fks_saindo[r['tabela_origem']].append(
            (r['coluna_origem'], r['tabela_destino'], r['coluna_destino'], r['fk_nome']))
        fks_entrando[r['tabela_destino']].append(
            (r['tabela_origem'], r['coluna_origem'], r['coluna_destino']))

    hubs = sorted(((r['tabela_referenciada'], int0(r['qtd_fks_apontando'])) for r in r_hub),
                  key=lambda x: -x[1])
    views = [r['view_name'] for r in r_vw]

    # ---- 7.2 classificacao e modulo -------------------------------------
    classe, motivo_classe, mod_de = {}, {}, {}
    for t in tabelas:
        cl, mot = classificar(t, linhas[t],
                              len(fks_saindo.get(t, [])), len(fks_entrando.get(t, [])))
        classe[t], motivo_classe[t] = cl, mot
        mod_de[t] = modulo_de(t)

    # ---- 7.3 descricoes funcionais --------------------------------------
    fontes = carregar_fontes(agente, REPO)
    docd = indexar_descricoes(fontes, tabelas)

    funcao, fonte_funcao, nivel_funcao = {}, {}, {}
    for t in tabelas:
        if t in docd:
            d, a, n = docd[t]
            funcao[t], fonte_funcao[t], nivel_funcao[t] = d, a, n
            continue
        # As IV_Q_* tem funcao estrutural conhecida com certeza: sao a
        # materializacao fisica de um formulario. Nao sao "nao documentadas".
        if classe[t] == 'formulario-materializado':
            funcao[t] = ('Materializacao fisica das respostas do formulario `%s`. '
                         'Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada '
                         'coluna e uma questao do formulario.' % t[5:])
            fonte_funcao[t], nivel_funcao[t] = None, 0
            continue
        inf = inferir(t, colunas.get(t, []), classe[t])
        if inf:
            funcao[t], fonte_funcao[t], nivel_funcao[t] = inf, None, 3
        else:
            funcao[t] = '(nao documentado - apurar com acesso ao vivo)'
            fonte_funcao[t], nivel_funcao[t] = None, 4

    # ---- 7.4 agrupamento por modulo -------------------------------------
    por_mod = defaultdict(list)
    for t in tabelas:
        por_mod[mod_de[t]].append(t)
    for m in por_mod:
        por_mod[m].sort(key=lambda t: t.upper())
    mods_ord = sorted(por_mod, key=lambda m: (-len(por_mod[m]), m))

    escritos = []

    # ======================================================================
    # 7.5 ARQUIVOS DE MODULO
    # ======================================================================
    for mod in mods_ord:
        cam = os.path.join(saida, arquivo_de(mod))
        ts = por_mod[mod]
        out = []
        w = out.append
        w('# %s' % TITULO_MODULO.get(mod, 'Modulo `%s`' % mod))
        w('')
        w('> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). '
          'As contagens de linha sao do **snapshot de %s** (`schema/*.csv`), '
          'nao do banco ao vivo.' % snapshot)
        w('>')
        w('> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em %s. '
          '**Nao editar a mao** - reexecute o gerador.' % gerado_em)
        w('')
        w('**%d tabelas · %s colunas · %s linhas no snapshot.**'
          % (len(ts), fmt_num(sum(qtd_col[t] for t in ts)),
             fmt_num(sum(linhas[t] for t in ts))))
        w('')
        w('[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · '
          '[Lacunas](LACUNAS.md)')
        w('')
        w('## Tabelas neste arquivo')
        w('')
        w('| Tabela | Classe | Colunas | Linhas |')
        w('|---|---|---:|---:|')
        for t in ts:
            w('| [`%s`](#%s) | %s | %d | %s |'
              % (t, ancora(t), classe[t], qtd_col[t], fmt_num(linhas[t])))
        w('')
        w('---')
        w('')

        for t in ts:
            w('### %s' % t)
            w('')
            w('`classe: %s` · `%d colunas` · `%s linhas (snapshot %s)` · `PK: %s`'
              % (classe[t], qtd_col[t], fmt_num(linhas[t]), snapshot,
                 ', '.join(pks.get(t, [])) or 'nenhuma declarada'))
            w('')
            cit = ' (fonte: `%s`)' % fonte_funcao[t] if fonte_funcao[t] else ''
            marca = '_(inferido)_ ' if nivel_funcao[t] == 3 else ''
            w('**Funcao:** %s%s%s' % (marca, funcao[t], cit))
            w('')

            if classe[t] == 'formulario-materializado':
                nome_form = t[5:]   # tudo depois de "IV_Q_"
                w('**Formulario de origem:** `%s` - tabela fisica gerada por DDL a partir do '
                  'formulario cuja `IV_Formulario.Descricao` = `%s`. Tem uma coluna por questao '
                  '(nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para '
                  '`IV_Questionario`.' % (nome_form, nome_form))
                w('')
                w('_Tabela de colunas omitida de proposito (%d colunas geradas): o rotulo legivel '
                  'de cada coluna so existe nas linhas de `IV_Questao` do formulario `%s`, que '
                  'exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._'
                  % (qtd_col[t], nome_form))
                w('')
            elif classe[t] == 'lixo/backup':
                w('> **Nao usar em producao.** Motivo da classificacao: %s '
                  '(padroes do `SCHEMA_MAP.md`).' % motivo_classe[t])
                w('')
                w('_Tabela de colunas omitida de proposito (%d colunas) para manter o documento '
                  'legivel._' % qtd_col[t])
                w('')
            else:
                cs = colunas.get(t, [])
                if cs:
                    fk_por_col = defaultdict(list)
                    for co, td, cd, _n in fks_saindo.get(t, []):
                        fk_por_col[co].append((td, cd))
                    pkset = set(pks.get(t, []))
                    w('**Colunas:**')
                    w('')
                    w('| ord | coluna | tipo | nulo? | default | observacao |')
                    w('|---:|---|---|:---:|:---:|---|')
                    for c in cs:
                        dflt = (c.get('valor_default') or '').strip() or '-'
                        w('| %s | `%s` | `%s` | %s | %s | %s |'
                          % (c['ord'], c['coluna'], tipo_fmt(c),
                             'sim' if c['nullable'] == 'YES' else '**nao**',
                             dflt, obs_coluna(t, c, pkset, fk_por_col)))
                    w('')
                else:
                    w('_Sem colunas no catalogo CSV._')
                    w('')

            ent = fks_entrando.get(t, [])
            if ent:
                w('**Referenciada por:** ' +
                  ', '.join(sorted({'`%s.%s`' % (o, c) for o, c, _d in ent})))
                w('')
            w('---')
            w('')

        io.open(cam, 'w', encoding='utf-8', newline='\n').write('\n'.join(out))
        escritos.append(cam)

    # ======================================================================
    # 7.6 00-INDICE.md
    # ======================================================================
    tot_col = sum(qtd_col.values())
    tot_lin = sum(linhas.values())
    por_classe = defaultdict(list)
    for t in tabelas:
        por_classe[classe[t]].append(t)
    cnt = Counter(nivel_funcao.values())
    ROTULO_NIVEL = {
        0: 'Derivada da estrutura com certeza (tabela `IV_Q_*` de formulario)',
        1: 'Descricao encontrada em documento (secao `### Tabela` + `**Funcao:**`)',
        2: 'Mencao descritiva encontrada em documento (lista, tabela ou frase)',
        3: 'Inferida do nome + colunas - marcada `(inferido)`',
        4: '`(nao documentado - apurar com acesso ao vivo)`',
    }
    NIVEIS = (0, 1, 2, 3, 4)

    o = []
    w = o.append
    w('# Dicionario de Dados - Vortice CRM (banco `CRM`)')
    w('')
    w('> **Snapshot de %s** - todas as contagens de linha vem dos arquivos `schema/*.csv` do '
      'repo `vortice-crm-agent`, extraidos do SQL Server nessa data. O banco **nao** foi '
      'consultado ao vivo na geracao deste documento.' % snapshot)
    w('>')
    w('> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em %s. '
      '**Nao editar a mao** - reexecute o gerador.' % gerado_em)
    w('')
    w('## 1. Numeros gerais')
    w('')
    w('| Item | Valor |')
    w('|---|---:|')
    col_views = sum(1 for r in r_col if r['tabela'] in set(views))
    w('| Tabelas | %s |' % fmt_num(len(tabelas)))
    w('| Colunas de tabelas | %s |' % fmt_num(tot_col))
    w('| Views | %s |' % fmt_num(len(views)))
    w('| Colunas de views | %s |' % fmt_num(col_views))
    w('| **Total de colunas no catalogo** | **%s** |' % fmt_num(tot_col + col_views))
    w('| Tabelas com PK declarada | %s |' % fmt_num(len(pks)))
    w('| Chaves estrangeiras declaradas | %s |' % fmt_num(len(r_fk)))
    w('| Linhas somadas (snapshot) | %s |' % fmt_num(tot_lin))
    w('| Arquivos de modulo gerados | %d |' % len(mods_ord))
    w('')
    w('> As **%s linhas** de `colunas.csv` cobrem **%s objetos**: as %d tabelas (%s colunas) '
      '**e** as %d views (%s colunas). Este dicionario documenta as tabelas nos arquivos de '
      'modulo; as colunas das views estao em [VIEWS.md](VIEWS.md). O que continua faltando das '
      'views e o **SQL** de cada uma (ver [LACUNAS.md](LACUNAS.md)).'
      % (fmt_num(tot_col + col_views), fmt_num(len(tabelas) + len(views)),
         len(tabelas), fmt_num(tot_col), len(views), fmt_num(col_views)))
    w('')
    w('Negocio: CRM + BPM da Tracbel (revenda de maquinas e equipamentos, divisao Agro), '
      'integrado aos ERPs TOTVS e JD Edwards. Fluxo central: '
      '**pessoa -> processo -> agenda -> historico -> resultado -> proxima agenda**.')
    w('')
    w('## 2. Resumo por modulo')
    w('')
    w('Modulo = prefixo do nome da tabela ate o primeiro `_` (tabelas sem `_` vao para '
      '`OUTROS`). O prefixo `IV_` tem 377 tabelas e foi dividido em 4 arquivos por assunto.')
    w('')
    w('| Modulo | Arquivo | Tabelas | Colunas | Linhas (snapshot) | % das linhas |')
    w('|---|---|---:|---:|---:|---:|')
    for m in mods_ord:
        ts = por_mod[m]
        sl = sum(linhas[t] for t in ts)
        w('| `%s` | [%s](%s) | %d | %s | %s | %.2f%% |'
          % (m, arquivo_de(m), arquivo_de(m), len(ts),
             fmt_num(sum(qtd_col[t] for t in ts)), fmt_num(sl),
             100.0 * sl / tot_lin if tot_lin else 0))
    w('| **TOTAL** | | **%d** | **%s** | **%s** | **100%%** |'
      % (len(tabelas), fmt_num(tot_col), fmt_num(tot_lin)))
    w('')
    w('> Nao existe modulo `BI_` em **tabelas** - `BI_*` sao **views** '
      '(ver [LACUNAS.md](LACUNAS.md) secao 2).')
    w('')

    w('## 3. As 30 maiores tabelas por numero de linhas')
    w('')
    w('| # | Tabela | Modulo | Classe | Colunas | Linhas | % do total |')
    w('|---:|---|---|---|---:|---:|---:|')
    for i, t in enumerate(sorted(tabelas, key=lambda x: (-linhas[x], x.upper()))[:30], 1):
        w('| %d | [`%s`](%s#%s) | `%s` | %s | %d | %s | %.2f%% |'
          % (i, t, arquivo_de(mod_de[t]), ancora(t), mod_de[t], classe[t],
             qtd_col[t], fmt_num(linhas[t]),
             100.0 * linhas[t] / tot_lin if tot_lin else 0))
    w('')

    w('## 4. Hubs - as tabelas mais referenciadas por FK')
    w('')
    w('Quanto mais FKs apontam para uma tabela, mais ela e o centro do modelo. '
      'Fonte: `schema/hubs.csv`.')
    w('')
    w('| Tabela | FKs apontando | Modulo | Classe | Linhas | Funcao (resumo) |')
    w('|---|---:|---|---|---:|---|')
    for t, n in hubs[:30]:
        if t not in linhas:
            w('| `%s` | %d | _(ausente de tabelas.csv)_ | - | - | - |' % (t, n))
            continue
        f = funcao[t].replace('|', '/')
        f = (f[:110] + '...') if len(f) > 110 else f
        w('| [`%s`](%s#%s) | %d | `%s` | %s | %s | %s |'
          % (t, arquivo_de(mod_de[t]), ancora(t), n, mod_de[t], classe[t],
             fmt_num(linhas[t]), f))
    w('')

    w('## 5. Classificacao das tabelas')
    w('')
    w('| Classe | Tabelas | % | Linhas | O que significa |')
    w('|---|---:|---:|---:|---|')
    for cl in CLASSES:
        ts = por_classe.get(cl, [])
        w('| `%s` | %d | %.1f%% | %s | %s |'
          % (cl, len(ts), 100.0 * len(ts) / len(tabelas),
             fmt_num(sum(linhas[t] for t in ts)), DESC_CLASSE[cl]))
    w('| **TOTAL** | **%d** | **100%%** | **%s** | |' % (len(tabelas), fmt_num(tot_lin)))
    w('')
    w('As regras sao avaliadas nesta ordem (a primeira que casar vence): '
      '`lixo/backup` -> `formulario-materializado` -> `staging` -> `vazia` -> `catalogo` '
      '-> `nucleo` -> `isolada`. Por isso uma `IV_Q_*` sem linhas aparece como '
      '`formulario-materializado`, e uma `IMP_*` sem linhas aparece como `staging`.')
    w('')
    for cl in CLASSES:
        ts = sorted(por_classe.get(cl, []), key=lambda x: x.upper())
        w('### Classe `%s` - %d tabelas' % (cl, len(ts)))
        w('')
        w(DESC_CLASSE[cl])
        w('')
        if not ts:
            w('_(nenhuma)_')
            w('')
            continue
        w('<details><summary>Lista completa das %d tabelas</summary>' % len(ts))
        w('')
        w('| Tabela | Modulo | Colunas | Linhas |')
        w('|---|---|---:|---:|')
        for t in ts:
            w('| [`%s`](%s#%s) | `%s` | %d | %s |'
              % (t, arquivo_de(mod_de[t]), ancora(t), mod_de[t],
                 qtd_col[t], fmt_num(linhas[t])))
        w('')
        w('</details>')
        w('')

    w('## 6. Cobertura da descricao funcional')
    w('')
    w('| Origem da coluna "Funcao" | Tabelas | % |')
    w('|---|---:|---:|')
    for k in NIVEIS:
        w('| %s | %d | %.1f%% |'
          % (ROTULO_NIVEL[k], cnt.get(k, 0), 100.0 * cnt.get(k, 0) / len(tabelas)))
    w('')
    w('As tabelas do nivel 4 estao listadas por modulo em [LACUNAS.md](LACUNAS.md) secao 3 - '
      'e o roteiro de investigacao para quando houver acesso ao vivo.')
    w('')

    w('## 7. Indice alfabetico de todas as tabelas')
    w('')
    w('| Tabela | Modulo | Arquivo | Classe | Colunas | Linhas |')
    w('|---|---|---|---|---:|---:|')
    for t in sorted(tabelas, key=lambda x: x.upper()):
        w('| [`%s`](%s#%s) | `%s` | [%s](%s) | %s | %d | %s |'
          % (t, arquivo_de(mod_de[t]), ancora(t), mod_de[t],
             arquivo_de(mod_de[t]), arquivo_de(mod_de[t]),
             classe[t], qtd_col[t], fmt_num(linhas[t])))
    w('')
    w('## 8. Os outros documentos deste dicionario')
    w('')
    w('- [GRAFO-FK.md](GRAFO-FK.md) - diagramas mermaid do nucleo relacional')
    w('- [LACUNAS.md](LACUNAS.md) - o que o catalogo CSV nao cobre e precisa de acesso ao vivo')
    w('- [gerar-dicionario.py](gerar-dicionario.py) - o gerador (deterministico, reexecutavel)')
    w('')

    cam = os.path.join(saida, '00-INDICE.md')
    io.open(cam, 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
    escritos.append(cam)

    # ======================================================================
    # 7.7 GRAFO-FK.md e 7.8 LACUNAS.md
    # ======================================================================
    escritos.append(gerar_grafo(saida, snapshot, gerado_em, tabelas, fks_saindo))
    escritos.append(gerar_views(saida, snapshot, gerado_em, views, colunas))
    escritos.append(gerar_lacunas(saida, snapshot, gerado_em, tabelas, views,
                                  nivel_funcao, mod_de, classe, linhas, qtd_col))

    # ======================================================================
    # 7.9 VERIFICACAO + RELATORIO NO CONSOLE
    # ======================================================================
    print('')
    print('=' * 78)
    print(' VERIFICACAO: cada tabela de tabelas.csv aparece em 1 arquivo de modulo?')
    print('=' * 78)
    ocorrencias = defaultdict(list)
    for mod in mods_ord:
        conteudo = io.open(os.path.join(saida, arquivo_de(mod)), encoding='utf-8').read()
        presentes = set(re.findall(r'^### (\S+)$', conteudo, re.M))
        for t in tabelas:
            if t in presentes:
                ocorrencias[t].append(arquivo_de(mod))
    faltando = [t for t in tabelas if len(ocorrencias[t]) == 0]
    duplicadas = [t for t in tabelas if len(ocorrencias[t]) > 1]
    exatas = len([t for t in tabelas if len(ocorrencias[t]) == 1])
    print(' Tabelas em tabelas.csv .................... %d' % len(tabelas))
    print(' Com secao `### <TABELA>` em exatamente 1 .. %d' % exatas)
    print(' Ausentes .................................. %d %s'
          % (len(faltando), faltando[:10] if faltando else ''))
    print(' Duplicadas em mais de 1 arquivo ........... %d %s'
          % (len(duplicadas), duplicadas[:10] if duplicadas else ''))
    ok = not faltando and not duplicadas
    print(' RESULTADO: %s'
          % ('OK - as %d tabelas aparecem em exatamente 1 arquivo de modulo' % len(tabelas)
             if ok else '*** FALHOU ***'))

    print('')
    print('=' * 78)
    print(' RESUMO POR MODULO')
    print('=' * 78)
    print(' %-9s %-38s %6s %8s %14s' % ('MODULO', 'ARQUIVO', 'TABS', 'COLS', 'LINHAS'))
    print(' ' + '-' * 76)
    for m in mods_ord:
        ts = por_mod[m]
        print(' %-9s %-38s %6d %8d %14s'
              % (m, arquivo_de(m), len(ts), sum(qtd_col[t] for t in ts),
                 fmt_num(sum(linhas[t] for t in ts))))
    print(' ' + '-' * 76)
    print(' %-9s %-38s %6d %8d %14s'
          % ('TOTAL', '', len(tabelas), tot_col, fmt_num(tot_lin)))

    print('')
    print('=' * 78)
    print(' CONTAGEM POR CLASSE')
    print('=' * 78)
    for cl in CLASSES:
        ts = por_classe.get(cl, [])
        print(' %-26s %4d tabelas  %5.1f%%  %14s linhas'
              % (cl, len(ts), 100.0 * len(ts) / len(tabelas),
                 fmt_num(sum(linhas[t] for t in ts))))
    print(' %-26s %4d tabelas  %5.1f%%  %14s linhas'
          % ('TOTAL', len(tabelas), 100.0, fmt_num(tot_lin)))

    print('')
    print('=' * 78)
    print(' COBERTURA DA DESCRICAO FUNCIONAL')
    print('=' * 78)
    for k in NIVEIS:
        print(' nivel %d  %-56s %4d' % (k, ROTULO_NIVEL[k][:56], cnt.get(k, 0)))
    print('')
    print(' >>> TABELAS "(nao documentado)": %d' % cnt.get(4, 0))

    print('')
    print('=' * 78)
    print(' ARQUIVOS GERADOS')
    print('=' * 78)
    print(' %-40s %12s %9s' % ('ARQUIVO', 'BYTES', 'LINHAS'))
    print(' ' + '-' * 76)
    total_b = 0
    for c in sorted(escritos, key=lambda x: os.path.basename(x)):
        b = os.path.getsize(c)
        total_b += b
        n = io.open(c, encoding='utf-8').read().count('\n') + 1
        print(' %-40s %12s %9s' % (os.path.basename(c), fmt_num(b), fmt_num(n)))
    print(' ' + '-' * 76)
    print(' %-40s %12s' % ('TOTAL (%d arquivos)' % len(escritos), fmt_num(total_b)))
    grandes = [os.path.basename(c) for c in escritos if os.path.getsize(c) > 1500000]
    print(' Acima de 1,5 MB: %s' % (grandes if grandes else 'nenhum - OK'))
    print('')
    return 0 if ok else 1


# ==========================================================================
# 8. GRAFO-FK.md
# ==========================================================================

def gerar_grafo(saida, snapshot, gerado_em, tabelas, fks_saindo):
    """Diagramas mermaid dos tres subgrafos centrais, limitados a ~25 nos cada."""

    # Conjuntos-semente; as arestas vem exclusivamente das FKs reais do CSV.
    NUCLEO = ['GE_Pessoa', 'IV_Processo', 'IV_ProcDado', 'IV_Agenda', 'IV_Historico',
              'IV_Interacao', 'IV_Questionario', 'IV_Formulario', 'IV_Questao',
              'IV_QuestaoLista', 'IV_Resultado', 'IV_ResultadoCmpl', 'IV_Acao',
              'IV_AcaoAuto', 'IV_ProcResultado', 'IV_ProcFase', 'IV_CodProcesso',
              'IV_Ciencia', 'IV_HistLink', 'IV_ProcDocto', 'DMN_Doc',
              'IVS_Pes', 'IVS_Carteira', 'IV_VENDEDOR', 'GE_Usuario']
    ERP = ['EXT_Veic', 'EXT_VeicMarca', 'EXT_VeicModelo', 'EXT_VeicFam',
           'EXT_VeicPlanoMan', 'EXT_VeicTipoMan', 'EXT_VeicModPlano',
           'EXT_NFS', 'EXT_NFSItem', 'EXT_NFSOper', 'EXT_OS', 'EXT_OSItem',
           'EXT_Titulo', 'EXT_TituloMov', 'EXT_Pessoa', 'EXT_Produto',
           'EXT_Pedido', 'EXT_Vendedor', 'GE_Pessoa', 'IMP_OS', 'IMP_Titulo']
    SEG = ['GE_Usuario', 'GE_PolSeg', 'GE_PolSegUsr', 'GE_PolSegItem', 'GE_Permissao',
           'GE_Aplicacao', 'GE_Modulo', 'GE_Sistema', 'GE_Empresa', 'GE_UsuarioEmpr',
           'GE_Papel', 'GE_PapelUsr', 'GE_Grupo', 'GE_GrupoUsr', 'GE_Perfil',
           'IV_Operador', 'IV_Atendente', 'IVC_EQUIPE', 'IVC_EQUIPEUSR']

    existentes = set(tabelas)

    def arestas(conj):
        """FKs reais entre as tabelas do conjunto, deduplicadas por par."""
        s, out, vistos = set(conj), [], set()
        for orig in conj:
            for co, td, cd, _n in fks_saindo.get(orig, []):
                if td in s and (orig, td) not in vistos:
                    vistos.add((orig, td))
                    out.append((orig, co, td, cd))
        return out

    def bloco_er(titulo, conj, nota):
        conj = [t for t in conj if t in existentes]
        ar = arestas(conj)
        usados = sorted({t for a in ar for t in (a[0], a[2])})
        L = ['### %s' % titulo, '', nota, '', '```mermaid', 'erDiagram']
        for orig, co, td, _cd in sorted(ar):
            L.append('    %s ||--o{ %s : "%s"' % (td, orig, co))
        L += ['```', '']
        orfaos = [t for t in conj if t not in usados]
        if orfaos:
            L.append('**Sem FK declarada dentro deste recorte** (ligam-se por convencao de nome, '
                     'nao por integridade referencial): ' +
                     ', '.join('`%s`' % t for t in orfaos))
            L.append('')
        L.append('_%d nos com FK, %d arestas._' % (len(usados), len(ar)))
        L.append('')
        return '\n'.join(L)

    o = []
    w = o.append
    w('# Grafo de chaves estrangeiras - o nucleo do modelo')
    w('')
    w('> Snapshot de %s. Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em %s. '
      '**Nao editar a mao.**' % (snapshot, gerado_em))
    w('')
    w('Nos diagramas `erDiagram`, as arestas sao **apenas FKs realmente declaradas** em '
      '`schema/fks.csv`. Boa parte das ligacoes do Vortice e feita **por convencao de nome, '
      'sem FK** - notavelmente `IV_Agenda.Processo`, `IV_Historico.Processo` e '
      '`IV_ProcDado.Processo`, que **nao** tem FK para `IV_Processo`. Essas ligacoes aparecem '
      'como linha tracejada nos diagramas `graph` que acompanham cada bloco.')
    w('')
    w('[Voltar ao indice](00-INDICE.md)')
    w('')
    w('---')
    w('')
    w('## 1. Nucleo CRM/BPM: pessoa -> processo -> agenda -> historico -> questionario')
    w('')
    w(bloco_er('1a. FKs declaradas no nucleo', NUCLEO,
               'Cada linha e uma FK real, no formato `destino ||--o{ origem : "coluna_origem"`.'))
    w('### 1b. O fluxo logico (inclui as ligacoes SEM FK)')
    w('')
    w('Linha cheia = FK declarada. Linha tracejada = ligacao por convencao de nome, **sem** '
      'integridade referencial no banco (documentado em `04-nucleo-crm-bpm...md` e em '
      '`REGRAS-DE-NEGOCIO.md` 1.2 e 1.3).')
    w('')
    w('```mermaid')
    w('graph LR')
    w('    GE_Pessoa["GE_Pessoa<br/>pessoa - PK SeqPessoa"]')
    w('    IV_Processo["IV_Processo<br/>oportunidade - PK Processo"]')
    w('    IV_ProcDado["IV_ProcDado<br/>dados do processo"]')
    w('    IV_Agenda["IV_Agenda<br/>tarefa - PK SeqAgenda"]')
    w('    IV_Historico["IV_Historico<br/>andamento - PK SeqHistorico"]')
    w('    IV_Interacao["IV_Interacao<br/>janelas da interacao"]')
    w('    IV_Questionario["IV_Questionario<br/>resposta de formulario"]')
    w('    IV_Formulario["IV_Formulario<br/>definicao do formulario"]')
    w('    IV_Questao["IV_Questao<br/>questoes"]')
    w('    IV_Q["IV_Q_*<br/>175 tabelas fisicas geradas"]')
    w('    IV_Resultado["IV_Resultado<br/>desfecho escolhido"]')
    w('    IV_Acao["IV_Acao<br/>tipo de tarefa"]')
    w('    IV_ProcResultado["IV_ProcResultado<br/>Resultado -> Fase/Status"]')
    w('    IV_AcaoAuto["IV_AcaoAuto<br/>Resultado -> proxima Acao"]')
    w('    IV_ProcFase["IV_ProcFase<br/>fases do fluxo"]')
    w('    IV_CodProcesso["IV_CodProcesso<br/>tipo de fluxo 41/50"]')
    w('    GE_Usuario["GE_Usuario<br/>usuario"]')
    w('    IV_VENDEDOR["IV_VENDEDOR<br/>CEN + hierarquia"]')
    w('    IVS_Pes["IVS_Pes<br/>carteirizacao/RFV"]')
    w('    IVS_Carteira["IVS_Carteira<br/>carteira"]')
    w('')
    w('    GE_Pessoa --> IV_Historico')
    w('    GE_Pessoa --> IV_Agenda')
    w('    GE_Pessoa --> IV_Questionario')
    w('    GE_Pessoa --> IVS_Pes')
    w('    IVS_Pes --> IVS_Carteira')
    w('    IV_Processo -.->|sem FK| IV_ProcDado')
    w('    IV_Processo -.->|sem FK| IV_Agenda')
    w('    IV_Processo -.->|sem FK| IV_Historico')
    w('    IV_Agenda -->|HistoricoOrigem| IV_Historico')
    w('    IV_Historico -->|AgendaOrigem| IV_Agenda')
    w('    IV_Historico --> IV_Interacao')
    w('    IV_Historico --> IV_Questionario')
    w('    IV_Resultado --> IV_Historico')
    w('    IV_Acao --> IV_Agenda')
    w('    IV_Resultado --> IV_ProcResultado')
    w('    IV_ProcResultado -.->|muda Fase/Status| IV_Processo')
    w('    IV_Resultado --> IV_AcaoAuto')
    w('    IV_AcaoAuto -.->|gera| IV_Agenda')
    w('    IV_AcaoAuto --> IV_Acao')
    w('    IV_CodProcesso --> IV_ProcFase')
    w('    IV_CodProcesso --> IV_ProcResultado')
    w('    IV_Formulario --> IV_Questao')
    w('    IV_Formulario --> IV_Questionario')
    w('    IV_Questao -.->|gera DDL| IV_Q')
    w('    IV_Questionario --> IV_Q')
    w('    GE_Usuario -.-> IV_Agenda')
    w('    IV_VENDEDOR -.->|SeqUsuarioLider| GE_Usuario')
    w('```')
    w('')
    w('> `IV_Agenda.CodProcesso` e `IV_Historico.CodProcesso` guardam o **tipo de fluxo** '
      '(41 ou 50), nao o numero do processo - que fica em `Processo`. `IV_Processo` **nao tem** '
      'coluna `CodProcesso` (`REGRAS-DE-NEGOCIO.md` 1.3).')
    w('')
    w('---')
    w('')
    w('## 2. ERP: equipamentos, notas fiscais, ordens de servico e titulos')
    w('')
    w(bloco_er('2a. FKs declaradas no bloco EXT_/IMP_', ERP,
               'Dados espelhados do TOTVS/JDE. `SeqPessoa` liga tudo de volta a `GE_Pessoa`.'))
    w('### 2b. As quatro arvores do ERP')
    w('')
    w('```mermaid')
    w('graph LR')
    w('    GE_Pessoa["GE_Pessoa"]')
    w('    subgraph Equipamento')
    w('      EXT_VeicMarca --> EXT_VeicFam')
    w('      EXT_VeicFam --> EXT_VeicModelo')
    w('      EXT_VeicModelo --> EXT_Veic')
    w('      EXT_VeicTipoMan --> EXT_VeicPlanoMan')
    w('      EXT_VeicPlanoMan --> EXT_Veic')
    w('    end')
    w('    subgraph Faturamento')
    w('      EXT_NFSOper --> EXT_NFS')
    w('      EXT_NFS --> EXT_NFSItem')
    w('    end')
    w('    subgraph Servico')
    w('      EXT_OS --> EXT_OSItem')
    w('      IMP_OS -.->|staging| EXT_OS')
    w('    end')
    w('    subgraph Financeiro')
    w('      EXT_Titulo --> EXT_TituloMov')
    w('      IMP_Titulo -.->|staging| EXT_Titulo')
    w('      X_T_IMP_CRM_TITULO -.->|TOTVS| IMP_Titulo')
    w('    end')
    w('    GE_Pessoa --> EXT_Veic')
    w('    GE_Pessoa --> EXT_NFS')
    w('    GE_Pessoa --> EXT_OS')
    w('    GE_Pessoa --> EXT_Titulo')
    w('    EXT_Veic --> EXT_NFS')
    w('    EXT_Veic --> EXT_OS')
    w('```')
    w('')
    w('> Freshness: `EXT_NFS` e `X_TOTVS_CRM_FATURAMENTO` param em **11/04/2025** '
      '(`SCHEMA_MAP.md`). Confirmar `MAX(<coluna_data>)` antes de afirmar tendencia recente.')
    w('')
    w('---')
    w('')
    w('## 3. Seguranca, usuarios e multiempresa')
    w('')
    w(bloco_er('3a. FKs declaradas no bloco GE_ de seguranca', SEG,
               'Usuarios, politicas de seguranca, permissoes e o cadastro de aplicacoes/telas.'))
    w('### 3b. As camadas de autorizacao')
    w('')
    w('```mermaid')
    w('graph TD')
    w('    GE_Sistema["GE_Sistema<br/>sistema"] --> GE_Modulo["GE_Modulo<br/>modulo"]')
    w('    GE_Modulo --> GE_Aplicacao["GE_Aplicacao<br/>tela/aplicacao"]')
    w('    GE_Aplicacao --> GE_Permissao["GE_Permissao<br/>permissao por objeto"]')
    w('    GE_PolSeg["GE_PolSeg<br/>politica de seguranca"] --> GE_Permissao')
    w('    GE_PolSeg --> GE_Usuario["GE_Usuario<br/>usuario do CRM"]')
    w('    GE_Usuario --> IV_Operador["IV_Operador<br/>operador"]')
    w('    GE_Usuario --> IV_Atendente["IV_Atendente<br/>atendente"]')
    w('    GE_Usuario --> IV_VENDEDOR["IV_VENDEDOR<br/>vendedor/CEN"]')
    w('    GE_Empresa["GE_Empresa<br/>empresa/filial"] -.->|NroEmpresa| GE_Usuario')
    w('    GE_Empresa -.->|NroEmpresa| GE_PolSeg')
    w('    IV_VENDEDOR -->|SeqUsuarioLider| GE_Usuario')
    w('```')
    w('')
    w('> Criar um usuario toca ~5-7 tabelas (`GE_Usuario`, `IVC_EQUIPEUSR`, `IV_Operador`, '
      '`IV_Atendente`/`IVC_ATENDENTE`, `IV_VENDEDOR`, `GE_PolSeg*`) e **nao tem API** '
      '(`REGRAS-DE-NEGOCIO.md` 2.11).')
    w('')
    w('---')
    w('')
    w('_Os diagramas `erDiagram` sao gerados a partir de `schema/fks.csv`. As arestas '
      'tracejadas dos diagramas `graph` vem de regras documentadas em `SCHEMA_MAP.md` e '
      '`REGRAS-DE-NEGOCIO.md` e **nao existem como constraint no banco**._')
    w('')

    cam = os.path.join(saida, 'GRAFO-FK.md')
    io.open(cam, 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
    return cam


# ==========================================================================
# 9. VIEWS.md
# ==========================================================================
# `colunas.csv` cobre 1.178 objetos: as 767 tabelas E as 411 views. As colunas
# das views, portanto, ESTAO no catalogo - o que falta e o SQL de cada uma.

PAPEL_FAMILIA_VIEW = [
    ('VW_REL', 'relatorio da aplicacao (`VW_REL_TBA*` - telas de relatorio)'),
    ('VW_', 'view de relatorio/consulta da aplicacao'),
    ('BI_', 'consumo por BI / QlikView'),
    ('IV$', 'gerada por propriedade customizada (`IV_Propriedade`)'),
    ('X_V_', 'materializacao/extracao da integracao TOTVS'),
    ('X_', 'integracao TOTVS'),
    ('QVW', 'motor de relatorio QVW'),
    ('V_', 'view utilitaria'),
]


def familia_view(v):
    return next((p for p, _d in PAPEL_FAMILIA_VIEW if v.upper().startswith(p)), '(outras)')


def gerar_views(saida, snapshot, gerado_em, views, colunas):
    """Catalogo de colunas das 411 views (o SQL delas continua em LACUNAS.md)."""
    o = []
    w = o.append
    w('# Views - colunas conhecidas, SQL desconhecido')
    w('')
    w('> Snapshot de %s. Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em %s. '
      '**Nao editar a mao.**' % (snapshot, gerado_em))
    w('')
    w('`schema/colunas.csv` cobre **1.178 objetos**: as 767 tabelas **e** as %d views. '
      'Ou seja, a **assinatura** (colunas e tipos) de cada view esta no catalogo - o que '
      '**nao** esta e o `SELECT` que a define, porque `sys.sql_modules.definition` volta NULL '
      'para a conta de leitura (falta `VIEW DEFINITION`; ver `REGRAS-DE-NEGOCIO.md` 2.8 e '
      '[LACUNAS.md](LACUNAS.md) item 1).' % len(views))
    w('')
    w('Sem o SQL nao da para saber **de onde** cada coluna vem. Use este arquivo para saber '
      '**o que** cada view entrega.')
    w('')
    w('[Voltar ao indice](00-INDICE.md)')
    w('')
    fam = defaultdict(list)
    for v in views:
        fam[familia_view(v)].append(v)
    w('## Familias de view')
    w('')
    w('| Familia | Views | Colunas | Provavel papel |')
    w('|---|---:|---:|---|')
    for k in sorted(fam, key=lambda x: (-len(fam[x]), x)):
        d = next((d for p, d in PAPEL_FAMILIA_VIEW if p == k), 'nao identificado pelo nome')
        nc = sum(len(colunas.get(v, [])) for v in fam[k])
        w('| `%s*` | %d | %s | %s |' % (k, len(fam[k]), fmt_num(nc), d))
    w('| **TOTAL** | **%d** | **%s** | |'
      % (len(views), fmt_num(sum(len(colunas.get(v, [])) for v in views))))
    w('')
    w('---')
    w('')
    for k in sorted(fam, key=lambda x: (-len(fam[x]), x)):
        w('## Familia `%s*` (%d views)' % (k, len(fam[k])))
        w('')
        for v in sorted(fam[k]):
            cs = colunas.get(v, [])
            w('### %s' % v)
            w('')
            w('`view` · `%d colunas`' % len(cs))
            w('')
            if not cs:
                w('_Sem colunas no catalogo CSV._')
                w('')
                continue
            w('| ord | coluna | tipo | nulo? |')
            w('|---:|---|---|:---:|')
            for c in cs:
                w('| %s | `%s` | `%s` | %s |'
                  % (c['ord'], c['coluna'], tipo_fmt(c),
                     'sim' if c['nullable'] == 'YES' else '**nao**'))
            w('')
        w('---')
        w('')

    cam = os.path.join(saida, 'VIEWS.md')
    io.open(cam, 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
    return cam


# ==========================================================================
# 10. LACUNAS.md
# ==========================================================================

# (item, por que importa, como extrair) - o que o catalogo CSV nao cobre.
LACUNAS_ITENS = [
    ('Definicao (SQL) das views',
     'As colunas das 411 views **ja estao** no catalogo (ver [VIEWS.md](VIEWS.md)) - o que '
     'falta e o `SELECT`. As views sao a camada de relatorio real do produto (`VW_REL_*`, '
     '`BI_*`, `IV$P_*`); sem o SQL nao da para saber que tabelas cada relatorio le, que '
     'filtros aplica nem de onde vem cada coluna.',
     '`SELECT v.name, m.definition FROM sys.views v JOIN sys.sql_modules m '
     'ON m.object_id = v.object_id` - `definition` volta **NULL** sem a permissao '
     '`VIEW DEFINITION` (REGRAS-DE-NEGOCIO 2.8).'),
    ('Stored procedures (6) e functions (1)',
     'Regras de negocio que rodam dentro do banco, invisiveis no catalogo de tabelas.',
     '`SELECT o.type_desc, o.name, m.definition FROM sys.objects o JOIN sys.sql_modules m '
     'ON m.object_id = o.object_id WHERE o.type IN (\'P\',\'FN\',\'IF\',\'TF\')`'),
    ('Triggers',
     'Efeitos colaterais em INSERT/UPDATE (geracao automatica de agenda, log, validacao). '
     'Explicam comportamento que nao esta em nenhuma tabela.',
     '`SELECT t.name, OBJECT_NAME(t.parent_id) AS tabela, m.definition FROM sys.triggers t '
     'JOIN sys.sql_modules m ON m.object_id = t.object_id`'),
    ('Indices (clustered, nonclustered, unique, filtrados)',
     'Sem os indices nao da para escrever consulta performatica em `IV_Historico` (2,4M) ou '
     '`GE_LOG_PROCESSO` (12,7M), nem saber que colunas tem unicidade de fato.',
     '`SELECT i.name, i.type_desc, i.is_unique, OBJECT_NAME(i.object_id), c.name FROM '
     'sys.indexes i JOIN sys.index_columns ic ON ... JOIN sys.columns c ON ...` '
     '(+ `sys.dm_db_index_usage_stats` para uso real)'),
    ('CHECK e DEFAULT constraints (com nome e expressao)',
     'A coluna `valor_default` do `colunas.csv` esta **100% vazia** nas 19.859 linhas: nenhum '
     'default foi capturado na extracao. Sem os CHECKs tambem nao se conhece o dominio '
     'validado das colunas `char(1)` (`Status`, `Natureza`, `Classe`, `Realizada`).',
     '`SELECT * FROM sys.default_constraints` e `SELECT * FROM sys.check_constraints`'),
    ('Extended properties (MS_Description)',
     'Se a Vortice tiver documentado alguma tabela/coluna no proprio banco, esta aqui. '
     'O `SCHEMA_MAP.md` afirma que nao ha nenhuma - confirmar.',
     '`SELECT * FROM sys.extended_properties WHERE name = \'MS_Description\'`'),
    ('Colunas IDENTITY, COMPUTED e PERSISTED',
     'Define quais PKs sao auto-incremento (e portanto nao devem ser preenchidas por '
     'integracao) e quais colunas sao derivadas de outras.',
     '`SELECT OBJECT_NAME(object_id), name, is_identity, is_computed FROM sys.columns` '
     '+ `sys.computed_columns`'),
    ('Collation por coluna e do banco',
     'Determina se a comparacao de texto e case/accent sensitive - critico para JOIN por '
     '`CODVENDEDOR`, `CodUsuario` e demais chaves varchar.',
     '`SELECT OBJECT_NAME(object_id), name, collation_name FROM sys.columns WHERE '
     'collation_name IS NOT NULL` + `SELECT DATABASEPROPERTYEX(DB_NAME(),\'Collation\')`'),
    ('Jobs do SQL Server Agent',
     'As cargas `IMP_*` / `X_TOTVS_*` e a sincronizacao com o ERP rodam por job. Sem isso nao '
     'se sabe a periodicidade nem por que o faturamento parou em abr/2025.',
     '`SELECT j.name, s.step_name, s.command FROM msdb.dbo.sysjobs j JOIN msdb.dbo.sysjobsteps '
     's ON s.job_id = j.job_id` + `msdb.dbo.sysjobhistory`'),
    ('Dominio real das colunas `char(1)`, `Status` e flags',
     'O `SCHEMA_MAP.md` avisa: `GE_Pessoa.Status` nao e so A/I - o real e P/A/S/F/O/I. '
     'Cada tabela tem seu proprio dominio, nao declarado em lugar nenhum.',
     '`SELECT <coluna>, COUNT(*) FROM <tabela> WITH (NOLOCK) GROUP BY <coluna>`'),
    ('Conteudo dos catalogos de parametrizacao',
     'O significado de `IV_Acao`, `IV_Resultado`, `IV_ProcFase`, `IV_ProcResultado` e '
     '`IV_AcaoAuto` esta nas **linhas**, nao no schema. E o que traduz codigo em comportamento '
     '(fluxo 41 x 50, ordem das fases, quem recebe a proxima agenda).',
     '`SELECT * FROM IV_Acao WITH (NOLOCK) WHERE EMUSO = 1`; idem para os demais'),
    ('Mapeamento `IV_Q_<X>` -> rotulos de `IV_Questao`',
     'As 175 tabelas de formulario materializado somam 2.601 colunas cujos nomes vem de '
     '`IV_Questao.Nomecoluna`. O rotulo legivel de cada coluna so existe nas linhas de '
     '`IV_Questao.Descricao`.',
     '`SELECT f.Descricao AS formulario, q.Questao, q.Nomecoluna, q.Descricao, q.TipoDado '
     'FROM IV_Questao q JOIN IV_Formulario f ON f.SeqFormulario = q.SeqFormulario ORDER BY 1,2`'),
    ('Tamanho fisico real (paginas, MB, particionamento, compressao)',
     'O CSV traz contagem de linhas, nao espaco ocupado. Necessario para dimensionar migracao '
     'e backup.',
     '`EXEC sp_spaceused \'<tabela>\'` ou `sys.dm_db_partition_stats`'),
    ('Freshness por tabela (data do dado mais recente)',
     'O `SCHEMA_MAP.md` documenta que o faturamento para em abr/2025. Isso precisa ser medido '
     'tabela a tabela, nao assumido.',
     '`SELECT MAX(DtaInclusao), MAX(DtaAlteracao) FROM <tabela> WITH (NOLOCK)`'),
    ('Permissoes efetivas e roles do banco',
     'Explica por que `sys.sql_modules.definition` volta NULL para a conta de leitura - '
     'ausencia de resultado la e permissao, nao ausencia de codigo.',
     '`SELECT * FROM sys.database_permissions` / `sys.database_role_members`'),
]

def gerar_lacunas(saida, snapshot, gerado_em, tabelas, views, nivel_funcao,
                  mod_de, classe, linhas, qtd_col):
    o = []
    w = o.append
    w('# Lacunas do catalogo - o que so o acesso ao vivo resolve')
    w('')
    w('> Snapshot de %s. Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em %s.'
      % (snapshot, gerado_em))
    w('')
    w('> **Atualizacao de 02/09/2026:** a extracao ao vivo em `../extracao-vortice/` ja preenche os itens '
      '1 a 8 e 10 a 15 desta lista (codigo dos 548 objetos, indices, identity, extended properties, '
      'dominios reais, catalogos BPM, tamanho fisico, ultima atividade, permissoes). O unico item ainda '
      'aberto e o **9 (jobs do SQL Server Agent)** - depende do DBA. Os numeros de linhas deste dicionario '
      'continuam sendo os do snapshot de junho; os atuais estao em `../extracao-vortice/volumetria/`.')
    w('')
    w('Este dicionario foi montado **inteiramente** a partir de `schema/*.csv`. Esses CSVs '
      'cobrem tabelas, colunas, tipos, nulabilidade, PKs e FKs - e mais nada. Tudo o que esta '
      'listado abaixo esta **ausente do catalogo** e precisa de uma nova extracao com o banco '
      'acessivel (VPN + conta de leitura).')
    w('')
    w('[Voltar ao indice](00-INDICE.md)')
    w('')
    w('## 1. Metadados ausentes no catalogo CSV')
    w('')
    w('| # | O que falta | Por que importa | Como extrair (SQL Server) |')
    w('|---:|---|---|---|')
    for i, (a, b, c) in enumerate(LACUNAS_ITENS, 1):
        w('| %d | **%s** | %s | %s |'
          % (i, a, b.replace('|', '/'), c.replace('|', '/')))
    w('')

    w('## 2. As %d views - colunas conhecidas, SQL desconhecido' % len(views))
    w('')
    w('Correcao importante em relacao ao que se supunha: `colunas.csv` cobre **1.178 objetos** '
      '- as 767 tabelas **e** as %d views. Portanto a **assinatura** de cada view (colunas e '
      'tipos) esta documentada em [VIEWS.md](VIEWS.md). O que falta e o **`SELECT`** que define '
      'cada uma - e sem ele nao se sabe de qual tabela vem cada coluna, nem que filtro a view '
      'aplica. Como a camada de relatorio do Vortice vive nas views, este continua sendo o '
      'maior buraco do dicionario.' % len(views))
    w('')
    fam = defaultdict(list)
    for v in views:
        fam[familia_view(v)].append(v)
    w('| Familia | Views | Provavel papel | Assinatura | SQL |')
    w('|---|---:|---|:---:|:---:|')
    for k in sorted(fam, key=lambda x: (-len(fam[x]), x)):
        d = next((d for p, d in PAPEL_FAMILIA_VIEW if p == k), 'nao identificado pelo nome')
        w('| `%s*` | %d | %s | ok | **falta** |' % (k, len(fam[k]), d))
    w('| **TOTAL** | **%d** | | | |' % len(views))
    w('')
    w('<details><summary>Lista completa das %d views</summary>' % len(views))
    w('')
    for v in sorted(views):
        w('- `%s`' % v)
    w('')
    w('</details>')
    w('')

    w('## 3. Tabelas cuja funcao ficou `(nao documentado)`')
    w('')
    nd = [t for t in tabelas if nivel_funcao[t] == 4]
    w('Sao **%d tabelas** (de %d) para as quais nem os documentos-fonte nem o nome/colunas '
      'permitiram uma descricao honesta. Esta e a **fila de investigacao** com acesso ao vivo: '
      'para cada uma, rodar `SELECT TOP 20 * FROM <tabela> WITH (NOLOCK)` e localizar a tela do '
      'CRM que a alimenta.' % (len(nd), len(tabelas)))
    w('')
    if not nd:
        w('_Nenhuma - todas as tabelas tem descricao documentada ou inferida._')
        w('')
    else:
        pm = defaultdict(list)
        for t in nd:
            pm[mod_de[t]].append(t)
        w('| Modulo | Tabelas sem funcao |')
        w('|---|---:|')
        for m in sorted(pm, key=lambda x: (-len(pm[x]), x)):
            w('| `%s` | %d |' % (m, len(pm[m])))
        w('| **TOTAL** | **%d** |' % len(nd))
        w('')
        w('Prioridade: **alta** = tem volume relevante (> 10 mil linhas) e portanto guarda dado '
          'de producao; **media** = tem linhas; **baixa** = vazia ou classificada como '
          '`lixo/backup`.')
        w('')
        for m in sorted(pm, key=lambda x: (-len(pm[x]), x)):
            w('### Modulo `%s` - %d tabelas' % (m, len(pm[m])))
            w('')
            w('| Tabela | Classe | Colunas | Linhas | Prioridade |')
            w('|---|---|---:|---:|---|')
            for t in sorted(pm[m], key=lambda x: (-linhas[x], x.upper())):
                if classe[t] == 'lixo/backup':
                    pr = 'baixa (lixo/backup)'
                elif linhas[t] == 0:
                    pr = 'baixa (vazia)'
                elif linhas[t] > 10000:
                    pr = '**alta** (volume relevante)'
                else:
                    pr = 'media'
                w('| `%s` | %s | %d | %s | %s |'
                  % (t, classe[t], qtd_col[t], fmt_num(linhas[t]), pr))
            w('')

    w('## 4. Roteiro para a proxima extracao')
    w('')
    w('1. **Rodar a extracao ampliada** cobrindo os %d itens da secao 1 e salvar os novos CSVs '
      'em `vortice-crm-agent/schema/` (mesmos nomes + novos arquivos: `views_def.csv`, '
      '`rotinas.csv`, `triggers.csv`, `indices.csv`, `constraints.csv`, `identity.csv`, '
      '`jobs.csv`, `dominios.csv`, `freshness.csv`).' % len(LACUNAS_ITENS))
    w('2. **Extrair as linhas dos catalogos de parametrizacao** (`IV_Acao`, `IV_Resultado`, '
      '`IV_ProcFase`, `IV_ProcResultado`, `IV_AcaoAuto`, `IV_CodProcesso`, `IV_Formulario`, '
      '`IV_Questao`) - e ai que mora a regra de negocio do BPM.')
    w('3. **Amostrar as tabelas da secao 3** (`SELECT TOP 20`) para fechar as descricoes que '
      'hoje estao `(nao documentado)`.')
    w('4. **Medir freshness** por tabela e anexar ao dicionario, para nao repetir a surpresa do '
      'faturamento parado em abr/2025.')
    w('5. **Reexecutar** `python docs/dicionario/gerar-dicionario.py` - o gerador absorve os '
      'CSVs atualizados e reescreve todos os markdowns.')
    w('')
    w('> Enquanto isso nao acontece: toda afirmacao deste dicionario que nao venha diretamente '
      'de `schema/*.csv` esta marcada como `(inferido)` ou citando a fonte entre parenteses. '
      'Numeros de linha sao do snapshot de %s.' % snapshot)
    w('')

    cam = os.path.join(saida, 'LACUNAS.md')
    io.open(cam, 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
    return cam


if __name__ == '__main__':
    sys.exit(main())
