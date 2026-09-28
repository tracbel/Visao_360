/**
 * A META DE VENDA × O REALIZADO, filial a filial (#138) — `GET /api/v1/relatorios/metas`.
 *
 * A mesma ponte do consolidado (P-8): uma leitura por filial, com o cabeçalho da filial, no máximo quatro de cada vez,
 * e a tela soma por partição — cada meta e cada venda são de UMA filial. Três desfechos por filial, e nenhum vira zero
 * sem dizer:
 *
 * - respondeu: entra na soma;
 * - 403: a filial está fora do alcance de quem lê (o vendedor, no perfil Padrão, só lê a própria) — não é falha;
 * - outra falha: a filial fica fora da soma, NOMEADA, e o cartão diz quantas responderam.
 *
 * Quando TODAS dizem 403, quem lê não tem `Meta.Ler`: o cartão diz isso, e não "meta zero".
 */

import type { MetricaSemDado } from '../../tipos/relacionamento';
import type {
  AlcanceDaMeta,
  MetaERealizadoDaFilial,
  MetaERealizadoDoConsultor,
  MetaERealizadoNaLinha,
  OrigemDaMetaDeVenda,
  PeriodoDaMeta,
} from '../../tipos/metas';
import { comLimite, listarFiliais, type Filial } from './consolidado';
import { ErroDaApi, ler, type ContextoDeAcesso } from './http';

/** A resposta de UMA filial — ou por que ela não entrou. */
export type MetaDaFilial = {
  filial: Filial;
  meta: MetaERealizadoDaFilial | null;
  erro: Error | null;
  /** A filial respondeu 403: fora do alcance de quem lê, e não falha. */
  foraDoAlcance: boolean;
};

/** A meta e o realizado somados entre as filiais que responderam. */
export type MetasConsolidadas = {
  filiais: MetaDaFilial[];
  respondidas: number;
  foraDoAlcance: number;
  /** As filiais que falharam (e não disseram 403): ficam fora da soma, e a tela as nomeia. */
  falhas: Filial[];
  /** As vendas do período sem vendedor no ART, somadas das filiais. */
  vendasSemVendedor: number;
  /**
   * O que a soma não diz, em frases já consolidadas para a dica do cartão (revisão do PR #248): as que têm número são
   * refeitas da soma — a frase da API é de uma filial só —, e as que valem para todas vêm da primeira filial.
   */
  observacoes: string[];
  /** Nenhuma filial respondeu e todas disseram 403: quem lê não tem `Meta.Ler`. */
  semPermissao: boolean;
  periodo: PeriodoDaMeta | null;
  alcance: AlcanceDaMeta | null;
  metaMaquinas: number;
  realizadoMaquinas: number;
  /** Nulo quando nenhuma filial mediu (o alcance Próprios não atribui pendente a pessoa). */
  pendentesNoArt: number | null;
  metaConsorcio: number;
  /** As cotas de consórcio vendidas, somadas das filiais que mediram; nulo quando nenhuma mediu (o consórcio não foi lido). */
  realizadoConsorcio: number | null;
  /** As vendidas e ainda não entregues, somadas das filiais — fora do realizado (a régua da GN). */
  aguardandoEntrega: number;
  realizadoNoAnterior: number;
  mesEmCurso: { competencia: string; metaMaquinas: number; realizadoMaquinas: number } | null;
  porLinha: MetaERealizadoNaLinha[];
  porConsultor: MetaERealizadoDoConsultor[];
  /** A leitura mais recente do cadastro entre as filiais. */
  origem: OrigemDaMetaDeVenda | null;
  /** As lacunas de cada tipo, a primeira de cada — a frase com número é de uma filial, e a tela diz isso. */
  lacunas: MetricaSemDado[];
};

const somar = <T,>(lista: T[], valor: (x: T) => number) => lista.reduce((s, x) => s + valor(x), 0);

/** A meta e o realizado da filial do cabeçalho — a Performance de CEN, que é de uma filial por vez. */
export function obterMetaDaFilial(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<MetaERealizadoDaFilial>('/v1/relatorios/metas', contexto, { sinal });
}

/** Lê a meta de cada filial em operação e soma. */
export async function obterMetasConsolidadas(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<{ dados: MetasConsolidadas; procedencia: null }> {
  const filiais = await listarFiliais(contexto, sinal);
  const linhas = await comLimite(filiais, 4, async (filial): Promise<MetaDaFilial> => {
    try {
      const resposta = await ler<MetaERealizadoDaFilial>('/v1/relatorios/metas', { ...contexto, empresa: filial.codigo }, { sinal });
      return { filial, meta: resposta.dados, erro: null, foraDoAlcance: false };
    } catch (causa) {
      if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
      if (causa instanceof ErroDaApi && causa.status === 403) return { filial, meta: null, erro: causa, foraDoAlcance: true };
      return { filial, meta: null, erro: causa instanceof Error ? causa : new Error(String(causa)), foraDoAlcance: false };
    }
  });

  return { dados: somarMetas(linhas), procedencia: null };
}

/**
 * Soma as filiais. Toda conta é de uma partição que não se sobrepõe entre filiais — a meta e a venda pela filial dona —,
 * e o percentual é refeito na tela a partir da soma, nunca somado.
 */
export function somarMetas(filiais: MetaDaFilial[]): MetasConsolidadas {
  const vivas = filiais.filter((f) => f.meta !== null).map((f) => f.meta!);
  const foraDoAlcance = filiais.filter((f) => f.foraDoAlcance).length;

  const comPendente = vivas.filter((m) => m.totais.pendentesNoArt !== null);
  const comConsorcio = vivas.filter((m) => m.totais.realizadoConsorcio != null);
  const emCurso = vivas.map((m) => m.mesEmCurso).filter((m) => m !== null);

  const linhas = new Map<string, MetaERealizadoNaLinha>();
  for (const l of vivas.flatMap((m) => m.porLinha)) {
    const atual = linhas.get(l.codigo);
    linhas.set(l.codigo, atual ? { ...atual, meta: atual.meta + l.meta, realizado: atual.realizado + l.realizado } : { ...l });
  }

  // POR CONSULTOR CONTA A PESSOA: quem tem meta em duas filiais aparece uma vez, com as duas somadas.
  const consultores = new Map<string, MetaERealizadoDoConsultor>();
  for (const c of vivas.flatMap((m) => m.porConsultor)) {
    const atual = consultores.get(c.consultor);
    consultores.set(
      c.consultor,
      atual
        ? { ...atual, temConta: atual.temConta || c.temConta, meta: atual.meta + c.meta, realizado: atual.realizado + c.realizado }
        : { ...c },
    );
  }

  const lacunas = new Map<string, MetricaSemDado>();
  for (const l of vivas.flatMap((m) => m.metricasSemDado)) if (!lacunas.has(l.metrica)) lacunas.set(l.metrica, l);

  const porPessoa = [...consultores.values()];
  const vendasSemVendedor = somar(vivas, (m) => m.totais.vendasSemVendedor ?? 0);
  const semConta = porPessoa.filter((c) => !c.temConta && c.meta > 0).length;
  const alcance = vivas[0]?.alcance ?? null;

  // AS FRASES DA DICA DO CARTÃO. Com número, refeitas da soma: a da API é da filial que respondeu primeiro. Sem número
  // de filial (o fim do cadastro, as pendentes sem filial, o login que não casa), a primeira vale para todas.
  const observacoes = [
    lacunas.get('loginSemCasamento')?.motivo,
    lacunas.get('vendasPelaFilial')?.motivo,
    lacunas.get('metaParcial')?.motivo,
    alcance === 'Filial' && semConta > 0
      ? `${semConta.toLocaleString('pt-BR')} consultor(es) com meta não têm conta no CRM: a meta deles aparece só na visão da filial.`
      : undefined,
    vendasSemVendedor > 0
      ? `${vendasSemVendedor.toLocaleString('pt-BR')} venda(s) do período sem vendedor no ART contam no total e em consultor nenhum.`
      : undefined,
    lacunas.get('pendentesSemFilial')?.motivo,
  ].filter((o): o is string => typeof o === 'string' && o.length > 0);

  const origem =
    vivas
      .map((m) => m.origem)
      .filter((o): o is OrigemDaMetaDeVenda => o !== null)
      .sort((a, b) => a.lidaEm.localeCompare(b.lidaEm))
      .at(-1) ?? null;

  return {
    filiais,
    respondidas: vivas.length,
    foraDoAlcance,
    falhas: filiais.filter((f) => f.erro !== null && !f.foraDoAlcance).map((f) => f.filial),
    vendasSemVendedor,
    observacoes,
    semPermissao: vivas.length === 0 && filiais.length > 0 && foraDoAlcance === filiais.length,
    periodo: vivas[0]?.periodo ?? null,
    alcance,
    metaMaquinas: somar(vivas, (m) => m.totais.metaMaquinas),
    realizadoMaquinas: somar(vivas, (m) => m.totais.realizadoMaquinas),
    pendentesNoArt: comPendente.length === 0 ? null : somar(comPendente, (m) => m.totais.pendentesNoArt ?? 0),
    metaConsorcio: somar(vivas, (m) => m.totais.metaConsorcio),
    aguardandoEntrega: somar(vivas, (m) => m.totais.aguardandoEntrega ?? 0),
    realizadoConsorcio: comConsorcio.length === 0 ? null : somar(comConsorcio, (m) => m.totais.realizadoConsorcio ?? 0),
    realizadoNoAnterior: somar(vivas, (m) => m.mesmoTrechoDoFyAnterior.realizadoMaquinas),
    mesEmCurso:
      emCurso.length === 0
        ? null
        : {
            competencia: emCurso[0]!.competencia,
            metaMaquinas: somar(emCurso, (m) => m!.metaMaquinas),
            realizadoMaquinas: somar(emCurso, (m) => m!.realizadoMaquinas),
          },
    porLinha: [...linhas.values()].sort((a, b) => b.meta - a.meta || b.realizado - a.realizado || a.nome.localeCompare(b.nome)),
    porConsultor: porPessoa.sort(
      (a, b) => b.meta - a.meta || b.realizado - a.realizado || a.consultor.localeCompare(b.consultor),
    ),
    origem,
    lacunas: [...lacunas.values()],
  };
}
