"""Inventário do histórico Git: credenciais, identificadores de usuário e dados pessoais (documento 33).

Uso (só leitura; nunca imprime nem grava o VALOR encontrado — só caminho, categoria e contagem):
    python scripts/historico/inventariar-historico.py [--repo <clone normal>] [--mensagens] [--saida <json>]

- --repo: o repositório a inventariar (padrão: este). Tem de ser um clone NORMAL, não um espelho
  (--mirror): a lista dos arquivos atuais vem da árvore do HEAD.
- --mensagens: lê também as mensagens de commit e o nome e o e-mail de autor e de quem commitou.

Entradas locais, fora do Git:
- dados-locais/historico/usuarios-crm.txt: NomePrincipal|NomeCompleto|NomeExibicao dos usuários do CRM;
- dados-locais/historico/documentos-hash.txt: tabela|sha256 dos CPF/CNPJ do CRM.
A saída padrão vai para dados-locais/historico/inventario.json.
"""
import argparse
import collections
import csv
import hashlib
import io
import json
import os
import re
import subprocess
import sys
import unicodedata

sys.stdout.reconfigure(encoding='utf-8')
RAIZ = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))

CHAVE_SECRETA = re.compile(r'(?i)\b(password|passwd|pwd|senha|secret|client_secret|clientsecret|api[_-]?key|access[_-]?token|token)\b\s*["\']?\s*[:=]\s*["\']?([^\s"\';,<>{}()$|]{6,})')
MARCADOR = re.compile(r'(?i)(redig|omit|\*{3}|x{4}|<|>|troque|exemplo|changeme)')
JWT = re.compile(r'\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{5,}')
TOKEN_GITHUB = re.compile(r'\bgh[pousr]_[A-Za-z0-9]{30,}')
CHAVE_PRIVADA = re.compile(r'-----BEGIN (?:RSA |EC |OPENSSH |)PRIVATE KEY-----')
EMAIL = re.compile(r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b')
EMAIL_TECNICO = re.compile(r'(?i)(example\.|exemplo|teste|ficticia|ficticio|invalid|noreply|no-reply|localhost)')
DOCUMENTO = re.compile(r'(?<!\d)(\d{3}\.\d{3}\.\d{3}-\d{2}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}|\d{11}|\d{14})(?!\d)')
TELEFONE = re.compile(r'\(\d{2}\)\s?9?\d{4}-\d{4}')
LOGIN_VORTICE = re.compile(r'(?<![A-Za-z0-9_.])([A-Za-z]{3,})[._]([A-Za-z]{3,})(?![A-Za-z0-9_])')
BINARIO = re.compile(r'(?i)\.(png|jpe?g|gif|ico|pdf|xlsx?|docx?|zip|dll|exe|pfx|woff2?|ttf|mp4)$')

REPO = RAIZ


def git(*argumentos):
    return subprocess.run(['git', '-C', REPO, *argumentos], capture_output=True, check=True).stdout


def normalizar(texto):
    return unicodedata.normalize('NFKD', texto).encode('ascii', 'ignore').decode().upper()


def dv_cpf(n):
    if len(set(n)) == 1:
        return False
    for k in (9, 10):
        if (sum(int(n[i]) * (k + 1 - i) for i in range(k)) * 10) % 11 % 10 != int(n[k]):
            return False
    return True


def dv_cnpj(n):
    if len(set(n)) == 1:
        return False
    for k, pesos in ((12, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]), (13, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2])):
        s = sum(int(n[i]) * pesos[i] for i in range(k))
        if (0 if s % 11 < 2 else 11 - s % 11) != int(n[k]):
            return False
    return True


def main():
    global REPO
    a = argparse.ArgumentParser()
    a.add_argument('--repo', default=RAIZ)
    a.add_argument('--mensagens', action='store_true')
    a.add_argument('--usuarios', default=os.path.join(RAIZ, 'dados-locais', 'historico', 'usuarios-crm.txt'))
    a.add_argument('--documentos', default=os.path.join(RAIZ, 'dados-locais', 'historico', 'documentos-hash.txt'))
    a.add_argument('--saida', default=os.path.join(RAIZ, 'dados-locais', 'historico', 'inventario.json'))
    args = a.parse_args()
    REPO = os.path.abspath(args.repo)

    logins, pares, prenomes, sobrenomes = set(), set(), set(), set()
    for linha in open(args.usuarios, encoding='utf-8', errors='ignore'):
        partes = [p.strip() for p in linha.split('|')]
        if len(partes) < 3:
            continue
        local = normalizar(partes[0].split('@')[0])
        if '.' in local and len(local) >= 5:
            logins.add(local)
        for nome in partes[1:3]:
            tokens = [t for t in re.split(r'[^A-Z]+', normalizar(nome)) if len(t) >= 3 and t not in {'DOS', 'DAS', 'DES', 'DEL'}]
            if len(tokens) >= 2:
                pares.add((tokens[0], tokens[-1]))
                prenomes.add(tokens[0])
                sobrenomes.update(tokens[1:])

    documentos = collections.defaultdict(set)
    for linha in open(args.documentos, encoding='utf-8', errors='ignore'):
        partes = linha.strip().split('|')
        if len(partes) == 2 and len(partes[1]) == 64:
            documentos[partes[1].lower()].add(partes[0])

    def tem_nome_de_usuario(norm):
        return sum(1 for p, u in pares if u in norm and p in norm and re.search(rf'\b{p}\b(?:\s+[A-Z]+){{0,3}}\s+{u}\b', norm))

    def examinar(caminho, texto):
        c = collections.Counter()
        for m in CHAVE_SECRETA.finditer(texto):
            valor = m.group(2)
            c['credencial_marcada_ou_codigo' if MARCADOR.search(valor) or re.fullmatch(r'[A-Za-z_][A-Za-z0-9_.]*', valor) else 'credencial_a_conferir'] += 1
        c['credencial_jwt'] += len(JWT.findall(texto))
        c['credencial_token_github'] += len(TOKEN_GITHUB.findall(texto))
        c['credencial_chave_privada'] += len(CHAVE_PRIVADA.findall(texto))
        emails = {e.lower() for e in EMAIL.findall(texto) if not EMAIL_TECNICO.search(e)}
        c['pessoal_email'] = len(emails)
        c['pessoal_email_corporativo'] = sum(1 for e in emails if e.endswith('tracbel.com.br'))
        for bruto in {re.sub(r'\D', '', x) for x in DOCUMENTO.findall(texto)}:
            valido = (len(bruto) == 11 and dv_cpf(bruto)) or (len(bruto) == 14 and dv_cnpj(bruto))
            if not valido:
                continue
            tipo = 'cpf' if len(bruto) == 11 else 'cnpj'
            onde = documentos.get(hashlib.sha256(bruto.encode()).hexdigest())
            c[f'pessoal_{tipo}_de_cliente_real' if onde else f'pessoal_{tipo}_valido_sem_correspondencia'] += 1
        c['pessoal_telefone'] = len(set(TELEFONE.findall(texto)))
        norm = normalizar(texto)
        c['pessoal_nome_e_sobrenome_de_usuario'] = tem_nome_de_usuario(norm)
        c['identificador_login_de_usuario_do_crm'] = sum(
            1 for l in logins if l in norm and re.search(r'(?<![A-Z0-9.])' + re.escape(l) + r'(?![A-Z0-9])', norm))
        c['identificador_login_formato_vortice'] = len({f'{x}.{y}' for x, y in LOGIN_VORTICE.findall(norm) if x in prenomes and y in sobrenomes})
        c['identificador_caminho_de_perfil'] = len(set(re.findall(r'(?i)[A-Z]:\\+Users\\+([A-Za-z0-9._-]+)', texto)))
        if caminho.lower().endswith('.csv') and 'seguranca-banco' in caminho:
            c['identificador_conta_de_banco'] = max(0, len(list(csv.reader(io.StringIO(texto)))) - 1)
        return c

    commits = git('rev-list', '--all').decode().split()
    presenca = collections.defaultdict(set)
    for commit in commits:
        for linha in git('ls-tree', '-r', commit).decode('utf-8', errors='ignore').splitlines():
            meta, caminho = linha.split('\t', 1)
            _, tipo, sha = meta.split()
            if tipo == 'blob':
                presenca[(caminho, sha)].add(commit)

    atuais = set(git('ls-tree', '-r', '--name-only', 'HEAD').decode('utf-8', errors='ignore').splitlines())
    por_arquivo = collections.defaultdict(lambda: {'commits': set(), 'contagem': collections.Counter()})
    nao_inspecionados = collections.Counter()
    cache = {}
    for (caminho, sha), cs in presenca.items():
        item = por_arquivo[caminho]
        item['commits'] |= cs
        if BINARIO.search(caminho):
            nao_inspecionados[os.path.splitext(caminho)[1].lower()] += 1
            continue
        if sha not in cache:
            cache[sha] = examinar(caminho, git('cat-file', '-p', sha).decode('utf-8', errors='ignore'))
        for k, v in cache[sha].items():
            item['contagem'][k] = max(item['contagem'][k], v)

    saida = []
    for caminho, item in sorted(por_arquivo.items()):
        achados = {k: v for k, v in sorted(item['contagem'].items()) if v and k != 'credencial_marcada_ou_codigo'}
        if achados:
            saida.append({'caminho': caminho, 'rastreado_hoje': caminho in atuais, 'commits': len(item['commits']), **achados})

    resultado = {'commits': len(commits), 'caminhos': len(por_arquivo), 'nao_inspecionados': nao_inspecionados, 'arquivos': saida}

    if args.mensagens:
        # MENSAGEM DE COMMIT E IDENTIDADE DE AUTOR também são publicadas — e --replace-text não as altera.
        bruto = git('log', '--all', '--format=%an%x1f%ae%x1f%cn%x1f%ce%x1f%B%x1e').decode('utf-8', errors='ignore')
        nas_mensagens = collections.Counter()
        identidades = set()
        for registro in bruto.split('\x1e'):
            partes = registro.strip('\n').split('\x1f')
            if len(partes) < 5:
                continue
            autor, email_autor, quem, email_quem, corpo = partes[:5]
            for k, v in examinar('(mensagem de commit)', corpo).items():
                if k != 'credencial_marcada_ou_codigo':
                    nas_mensagens[k] += v
            identidades.update({(autor, email_autor.lower()), (quem, email_quem.lower())})
        resultado['mensagens'] = {k: v for k, v in sorted(nas_mensagens.items()) if v}
        resultado['identidades_de_commit'] = {
            'distintas': len(identidades),
            'com_nome_e_sobrenome_de_usuario': sum(1 for nome, _ in identidades if tem_nome_de_usuario(normalizar(nome))),
            'com_email_corporativo': sum(1 for _, e in identidades if e.endswith('tracbel.com.br')),
            'com_email_noreply': sum(1 for _, e in identidades if 'noreply' in e),
        }

    os.makedirs(os.path.dirname(os.path.abspath(args.saida)), exist_ok=True)
    json.dump(resultado, open(args.saida, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)

    total = collections.Counter()
    arquivos = collections.Counter()
    for item in saida:
        for k, v in item.items():
            if isinstance(v, int) and not isinstance(v, bool) and k != 'commits':
                total[k] += v
                arquivos[k] += 1
    print(f'commits {len(commits)} · caminhos {len(por_arquivo)} · arquivos com achado {len(saida)} · não inspecionados {dict(nao_inspecionados)}')
    for k in sorted(total):
        print(f'  {k:48} arquivos={arquivos[k]:4} soma={total[k]}')
    if args.mensagens:
        print('mensagens de commit:', resultado['mensagens'] or 'nenhum achado')
        print('identidades de commit:', resultado['identidades_de_commit'])


if __name__ == '__main__':
    main()
