#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
detectar-duplicatas.py — encontra tabelas do Vortice que fazem a mesma coisa.

Nao olha o nome da tabela: compara a ASSINATURA DE COLUNAS. Duas passadas.

  Passada 1 — conjunto de colunas IDENTICO.
              Agrupa por frozenset(colunas). Pega backup, staging e clone exato.

  Passada 2 — similaridade de Jaccard >= 0.75 entre os conjuntos de colunas,
              com os pares agrupados em componentes conexas (union-find).
              Pega o clone que ganhou ou perdeu uma ou duas colunas.

Entrada:
    <schema>/tabelas.csv   colunas: tabela, qtd_colunas, linhas
    <schema>/colunas.csv   colunas: tabela, ord, coluna, tipo, ...
    Padrao: c:/projetos/vortice-crm-agent/schema
    Snapshot de 03/06/2026, 767 tabelas de usuario.

Uso:
    python scripts/banco/detectar-duplicatas.py
    python scripts/banco/detectar-duplicatas.py --schema OUTRO/CAMINHO
    python scripts/banco/detectar-duplicatas.py --jaccard 0.80 --min-colunas 3

Saida: relatorio em texto no stdout, no mesmo formato de
       docs/extracao-vortice/tabelas-duplicadas.txt

Citado em: docs/projeto/17-MODELO-UNIFICADO.md, secao 3.
"""

import argparse
import collections
import csv
import itertools
import os
import sys

SCHEMA_PADRAO = os.path.join("c:", os.sep, "projetos", "vortice-crm-agent", "schema")


def ler(schema_dir):
    """Devolve {tabela: (qtd_colunas, linhas)} e {tabela: frozenset(colunas em maiuscula)}."""
    tabelas = {}
    with open(os.path.join(schema_dir, "tabelas.csv"), encoding="utf-8-sig", newline="") as fh:
        for reg in csv.DictReader(fh):
            tabelas[reg["tabela"]] = (int(reg["qtd_colunas"]), int(reg["linhas"]))

    brutas = collections.defaultdict(set)
    with open(os.path.join(schema_dir, "colunas.csv"), encoding="utf-8-sig", newline="") as fh:
        for reg in csv.DictReader(fh):
            # colunas.csv tambem lista as 411 views; so interessam tabelas reais
            if reg["tabela"] in tabelas:
                brutas[reg["tabela"]].add(reg["coluna"].upper())

    return tabelas, {t: frozenset(c) for t, c in brutas.items()}


def jaccard(a, b):
    uniao = len(a | b)
    return len(a & b) / uniao if uniao else 0.0


class UnionFind:
    """Agrupa os pares similares em componentes conexas."""

    def __init__(self):
        self.pai = {}

    def achar(self, x):
        self.pai.setdefault(x, x)
        while self.pai[x] != x:
            self.pai[x] = self.pai[self.pai[x]]
            x = self.pai[x]
        return x

    def unir(self, a, b):
        ra, rb = self.achar(a), self.achar(b)
        if ra != rb:
            self.pai[rb] = ra


def main():
    ap = argparse.ArgumentParser(
        description="Encontra tabelas com a mesma assinatura de colunas.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    ap.add_argument("--schema", default=SCHEMA_PADRAO,
                    help="pasta com tabelas.csv e colunas.csv")
    ap.add_argument("--jaccard", type=float, default=0.75,
                    help="limiar de similaridade da passada 2 (padrao 0.75)")
    ap.add_argument("--min-colunas", type=int, default=3,
                    help="ignora tabelas com menos colunas que isto (padrao 3)")
    args = ap.parse_args()

    tabelas, assinatura = ler(args.schema)
    analisadas = {t: s for t, s in assinatura.items() if len(s) >= args.min_colunas}

    def linhas(t):
        return tabelas.get(t, (0, 0))[1]

    print("Tabelas analisadas: {} (de {}; ignoradas as com menos de {} colunas)".format(
        len(analisadas), len(tabelas), args.min_colunas))

    # ---------- passada 1: conjunto de colunas identico ----------
    por_assinatura = collections.defaultdict(list)
    for tab, sig in analisadas.items():
        por_assinatura[sig].append(tab)
    identicos = [sorted(g, key=lambda t: -linhas(t))
                 for g in por_assinatura.values() if len(g) > 1]
    identicos.sort(key=lambda g: -linhas(g[0]))

    print()
    print("=" * 78)
    print("1. GRUPOS COM CONJUNTO DE COLUNAS IDENTICO: {} grupos, {} tabelas".format(
        len(identicos), sum(len(g) for g in identicos)))
    print("=" * 78)
    print()
    for grupo in identicos:
        n = len(assinatura[grupo[0]])
        print("  [{} colunas] ".format(n) +
              " | ".join("{} ({:,})".format(t, linhas(t)) for t in grupo))
        print()

    # ---------- passada 2: Jaccard + componentes conexas ----------
    uf = UnionFind()
    pares = 0
    for a, b in itertools.combinations(sorted(analisadas), 2):
        if jaccard(analisadas[a], analisadas[b]) >= args.jaccard:
            uf.unir(a, b)
            pares += 1

    componentes = collections.defaultdict(list)
    for tab in analisadas:
        componentes[uf.achar(tab)].append(tab)
    conjuntos = [sorted(g, key=lambda t: -linhas(t))
                 for g in componentes.values() if len(g) > 1]
    conjuntos.sort(key=lambda g: -linhas(g[0]))

    print("=" * 78)
    print("2. PARES QUASE IDENTICOS (Jaccard >= {})".format(args.jaccard))
    print("=" * 78)
    print()
    print("{} pares".format(pares))
    print()
    print("agrupados em {} conjuntos:".format(len(conjuntos)))
    print()
    for grupo in conjuntos:
        com_dados = sum(1 for t in grupo if linhas(t) > 0)
        medio = round(sum(len(analisadas[t]) for t in grupo) / len(grupo))
        print("  * {} tabelas ({} com dados) ~ {} colunas aprox.".format(
            len(grupo), com_dados, medio))
        for t in grupo:
            print("      {:<44s} {:>12,}".format(t, linhas(t)))

    # ---------- a conta que interessa ----------
    so_identicas = {t for g in identicos for t in g}
    so_conjuntos = {t for g in conjuntos for t in g}
    envolvidas = so_identicas | so_conjuntos
    sobreviventes = len(conjuntos) + len(so_identicas - so_conjuntos)

    print()
    print("=" * 78)
    print("3. A CONTA")
    print("=" * 78)
    print("  tabelas envolvidas em duplicacao ....... {}".format(len(envolvidas)))
    print("  sobreviveriam (uma por conjunto) ....... {}".format(sobreviventes))
    print("  ELIMINAVEIS SO POR DEDUPLICACAO ........ {}".format(len(envolvidas) - sobreviventes))
    print("  linhas guardadas nessas tabelas ........ {:,}".format(
        sum(linhas(t) for t in envolvidas)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
