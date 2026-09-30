/**
 * O CONSOLIDADO DAS TREZE FILIAIS — o que a diretoria e a gerência abrem para
 * ver tudo de uma vez.
 *
 * ---------------------------------------------------------------------------
 * COMO ISTO É OBTIDO, E POR QUE PRECISA ESTAR ESCRITO
 *
 * **Não existe rota consolidada na API, e não é esquecimento: é a fronteira de
 * multiempresa funcionando.** Todo agregado roda dentro do filtro global do
 * contexto de persistência, as profundidades param em `EmpresaEAbaixo`, e a
 * permissão que abre a fronteira entre filiais (`Empresa.AlcanceEntreFiliais`)
 * **não é concedida** (documento 23, seção 4.1). Uma filial por contexto.
 *
 * O que este módulo faz é **repetir a leitura, uma vez por filial**, trocando o
 * cabeçalho `X-Tracbel-Empresa` — exatamente o que o seletor de filial do
 * cabeçalho já faz quando alguém troca de filial na mão. Cada número que volta
 * é real, medido no banco, dentro do filtro daquela filial; o que a tela faz é
 * colocá-los lado a lado.
 *
 * **Isto é uma ponte, e ela tem prazo.** Hoje o contexto de acesso viaja em
 * cabeçalho HTTP e não autentica ninguém (dívida D-1) — então repetir a leitura
 * com outro código de filial não atravessa proteção nenhuma, porque não há
 * proteção. **No dia em que o Entra ID entrar, isto para de funcionar, e deve
 * parar**: o consolidado passa a exigir um agregado do lado do servidor, com a
 * permissão de alcance entre filiais concedida a quem é diretoria. Registrado
 * como dívida **P-8**.
 *
 * A alternativa seria a tela da diretoria não existir até lá, e ela é pior: o
 * pedido é justamente ver o negócio inteiro, e os números existem no banco.
 *
 * ---------------------------------------------------------------------------
 * A PONTE SAIU DO CAMINHO DA TELA (30/09/2026, #313)
 *
 * Abrir a Visão 360 custava ~147 chamadas — sete leituras do painel, uma dos
 * cartões, duas de perdas e funil e uma da meta, POR FILIAL —, e a tela esperava
 * a filial mais lenta. O Ricardo: "algumas coisas demoram para carregar, não
 * podemos ter isso"; e decidiu que o seletor de filial do topo FILTRA a Visão
 * 360: uma filial, ou "Todas as filiais".
 *
 * O servidor já sabe somar: com `TODAS` no cabeçalho, a pessoa que tem o alcance
 * entre filiais enxerga todas pelo filtro global, e as mesmas consultas somam
 * tudo numa leitura só (é o que o Forecast, o Estoque e a Conferência já fazem).
 * Então cada painel é UMA leitura, na filial do seletor. A ponte filial a filial
 * ficou só para a tabela de composição — que é, ela mesma, "filial a filial" —,
 * e só quando alguém a abre em "Todas as filiais".
 */

import type { CatalogoDeSelecao, ComProcedencia, ItemDeSelecao, PaginaDe } from '../../tipos/api';
import { CATALOGO } from '../../tipos/api';
import type {
  Agregado,
  ContagemPorRotulo,
  FaseDoFunil,
  Faturamento,
  FatiaDeVendaPerdida,
  FunilPorEstagio,
  PainelDaAgenda,
  ProcessoResumo,
  ClienteNoRanking,
  MesDeFaturamento,
  ResumoDeCobertura,
  VendasPerdidas,
} from '../../tipos/relacionamento';
import type {
  CarteiraDaFilial,
  CoberturaDaFilial,
  FaturamentoDaCompetencia,
  MaquinasEntreguesNoArt,
  MercadoDaFilial,
  PainelExecutivoDaFilial,
} from '../../tipos/painelExecutivo';
import { TODAS_AS_FILIAIS } from './acesso';
import { ler, type ContextoDeAcesso } from './http';

/** Uma filial em operação, como o catálogo `EMPRESA` a declara. */
/** Quantos meses a série consolidada mostra. O mesmo número que o cartão promete no título. */
const MESES_NA_SERIE = 12;

/** As naturezas de responsável que contam como operação. Sistema, fornecedor e teste não. */
export const ENTRAM_NO_RANKING = new Set(['Pessoa', 'Departamento']);

export type Filial = { codigo: string; nome: string };

/** Tudo o que o painel consolidado precisa saber de UMA filial. */
export type ConsolidadoDaFilial = {
  filial: Filial;
  /** Nulo quando a leitura daquela filial falhou — a tela mostra a falha, não zero. */
  falhou: boolean;
  clientes: number;
  comContatoEm30Dias: number;
  comContatoEm90Dias: number;
  nuncaContatados: number;
  carteiras: number;
  processosAbertos: number;
  processosComValor: number;
  ganhos: number;
  perdidos: number;
  tarefasPendentes: number;
  tarefasAtrasadas: number;
  /** As linhas de negócio desta filial, com o número de vínculos de cada uma — só carteira comercial. */
  porLinhaDeNegocio: { nome: string; clientes: number }[];
  /** Os CENs desta filial, com as carteiras deles somadas. */
  porResponsavel: { nome: string; carteiras: number; clientes: number; em30: number; em90: number; nunca: number }[];
  /**
   * As perdas por motivo desta filial, contadas em `processo.Processo`.
   *
   * O motivo é "não informado" em 100% — o Vórtice encerra o processo sem coluna de motivo.
   * Quem responde por quê é `obterPerdasEFunilConsolidados`, que lê o formulário do CEN no período.
   */
  perdasPorMotivo: ContagemPorRotulo[];

  /** O faturamento desta filial, lido da SD2 do Protheus. */
  faturamento: Faturamento | null;
  /** As fases do funil desta filial, para o consolidado somar por fase. */
  fases: FaseDoFunil[];
};

/**
 * As treze filiais em operação, do catálogo `EMPRESA`.
 *
 * **Nunca uma lista escrita no front.** O catálogo só devolve as ativas, e é ele
 * que define quais filiais operam — o legado marca outras como ativas e o
 * negócio não as confirmou (documento 23, seção 7).
 */
export async function listarFiliais(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<Filial[]> {
  const resposta = await ler<CatalogoDeSelecao[]>(`/v1/catalogos/${CATALOGO.empresa}`, contexto, { sinal });
  const catalogo = Array.isArray(resposta.dados) ? resposta.dados[0] : (resposta.dados as CatalogoDeSelecao);
  return (catalogo?.itens ?? []).map((i: ItemDeSelecao) => ({ codigo: i.codigo, nome: i.descricao }));
}

/** O que a tela escreve quando o seletor está em "Todas as filiais". */
export const NOME_DE_TODAS_AS_FILIAIS = 'Todas as filiais';

/**
 * A FILIAL DO SELETOR DO TOPO (30/09/2026, #313) — uma filial, ou "Todas as filiais". É o recorte de toda leitura da
 * Visão 360; o nome sai do catálogo de filiais, e a filial que ele não traz fica com o código.
 */
export async function filialDaSelecao(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<Filial> {
  if (contexto.empresa === TODAS_AS_FILIAIS) return { codigo: TODAS_AS_FILIAIS, nome: NOME_DE_TODAS_AS_FILIAIS };
  try {
    const filiais = await listarFiliais(contexto, sinal);
    return { codigo: contexto.empresa, nome: filiais.find((f) => f.codigo === contexto.empresa)?.nome ?? contexto.empresa };
  } catch (causa) {
    // O NOME NÃO SEGURA A TELA: sem o catálogo, a filial fica com o código, e as leituras seguem.
    if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
    return { codigo: contexto.empresa, nome: contexto.empresa };
  }
}

/** A frase do recorte, para os textos que dizem onde o número foi medido: "em Barretos" ou "nas filiais que você alcança". */
export function noRecorte(filial: Filial | null | undefined): string {
  if (!filial) return 'na filial escolhida';
  return filial.codigo === TODAS_AS_FILIAIS ? 'nas filiais que você alcança' : `em ${filial.nome}`;
}

/**
 * Roda as promessas com um teto de simultaneidade, para não abrir 65 conexões de uma vez. Exportada para a leitura da
 * meta de venda (`metas.ts`), que usa a mesma ponte filial a filial na tabela de composição.
 */
export async function comLimite<T, R>(itens: T[], limite: number, tarefa: (item: T) => Promise<R>): Promise<R[]> {
  const resultados: R[] = new Array(itens.length);
  let proximo = 0;

  async function trabalhador() {
    while (proximo < itens.length) {
      const meu = proximo++;
      resultados[meu] = await tarefa(itens[meu]);
    }
  }

  await Promise.all(Array.from({ length: Math.min(limite, itens.length) }, trabalhador));
  return resultados;
}

/** Conta processos de uma situação naquela filial. É um `COUNT` no banco. */
async function contar(
  contexto: ContextoDeAcesso,
  situacao: string,
  sinal?: AbortSignal,
): Promise<number> {
  const r = await ler<PaginaDe<ProcessoResumo>>('/v1/processos', contexto, {
    sinal,
    parametros: { pagina: 1, tamanho: 1, situacao, incluirEncerrados: situacao !== 'Aberto' },
  });
  return r.dados.total;
}

/**
 * Lê os agregados de UMA filial, trocando só o cabeçalho de empresa.
 *
 * São cinco leituras, e todas já vêm agrupadas do banco: cobertura por carteira,
 * painel da agenda, funil por fase, e as contagens de ganho e de perdido. A tela
 * recebe dezenas de linhas por filial, e não os 45 mil processos.
 */
async function lerFilial(
  base: ContextoDeAcesso,
  filial: Filial,
  sinal?: AbortSignal,
): Promise<ConsolidadoDaFilial> {
  const contexto: ContextoDeAcesso = { ...base, empresa: filial.codigo };

  const vazio: ConsolidadoDaFilial = {
    filial,
    falhou: true,
    clientes: 0,
    comContatoEm30Dias: 0,
    comContatoEm90Dias: 0,
    nuncaContatados: 0,
    carteiras: 0,
    processosAbertos: 0,
    processosComValor: 0,
    ganhos: 0,
    perdidos: 0,
    tarefasPendentes: 0,
    tarefasAtrasadas: 0,
    porLinhaDeNegocio: [],
    porResponsavel: [],
    perdasPorMotivo: [],
    faturamento: null,
    fases: [],
  };

  try {
    const [cobertura, agenda, funil, perdas, faturamento, ganhos, perdidos] =
      await Promise.all([
      ler<Agregado<ResumoDeCobertura>>('/v1/relatorios/cobertura', contexto, { sinal }),
      ler<Agregado<PainelDaAgenda>>('/v1/relatorios/agenda', contexto, { sinal }),
      ler<Agregado<FaseDoFunil>>('/v1/relatorios/funil', contexto, { sinal }),
      ler<Agregado<ContagemPorRotulo>>('/v1/relatorios/perdas', contexto, { sinal }),
      ler<Faturamento>('/v1/relatorios/faturamento', contexto, { sinal }),
      contar(contexto, 'Ganho', sinal),
      contar(contexto, 'Perdido', sinal),
    ]);

    const carteiras = cobertura.dados.itens;
    const painel = agenda.dados.itens[0];

    // O MIX CONTA SÓ CARTEIRA COMERCIAL (24/09/2026). Somava todas: a carteira
    // administrativa — depósito de cadastro, com milhares de vínculos numa linha
    // só — e a de teste entravam como se fossem venda, e a fatia da linha delas
    // crescia sem ninguém vender nada. É a mesma regra do ranking de CENs logo
    // abaixo e do cartão de cobertura: vínculo que conta é o de carteira comercial.
    const porLinha = new Map<string, number>();
    for (const c of carteiras.filter((k) => k.naturezaDaCarteira === 'Comercial')) {
      porLinha.set(c.linhaDeNegocioNome, (porLinha.get(c.linhaDeNegocioNome) ?? 0) + c.clientes);
    }

    // POR RESPONSÁVEL: cada carteira tem um CEN só, então somar carteira por
    // pessoa é exato e nenhuma linha é contada duas vezes. O que NÃO é somável
    // é o cliente — metade está em duas ou mais carteiras —, e por isso o campo
    // se chama `clientes` mas conta VÍNCULOS, como a tela escreve.
    const porCen = new Map<string, { nome: string; carteiras: number; clientes: number; em30: number; em90: number; nunca: number }>();

    // QUEM ENTRA NO RANKING. Pessoa e área entram — a carteira da Inteligência de Mercado tem
    // 5.000 clientes reais e a cobertura deles conta como a de qualquer outra. Ficam de fora
    // sistema, fornecedor e teste, que não são operação, e a carteira administrativa ou de
    // teste, que é depósito de cadastro e não carteira de ninguém.
    for (const c of carteiras.filter(
      (k) => ENTRAM_NO_RANKING.has(k.naturezaDoResponsavel) && k.naturezaDaCarteira === 'Comercial',
    )) {
      const atual = porCen.get(c.responsavelNome);
      if (atual) {
        atual.carteiras += 1;
        atual.clientes += c.clientes;
        atual.em30 += c.comContatoEm30Dias;
        atual.em90 += c.comContatoEm90Dias;
        atual.nunca += c.nuncaContatados;
      } else {
        porCen.set(c.responsavelNome, {
          nome: c.responsavelNome,
          carteiras: 1,
          clientes: c.clientes,
          em30: c.comContatoEm30Dias,
          em90: c.comContatoEm90Dias,
          nunca: c.nuncaContatados,
        });
      }
    }

    return {
      filial,
      falhou: false,
      clientes: carteiras.reduce((s, c) => s + c.clientes, 0),
      comContatoEm30Dias: carteiras.reduce((s, c) => s + c.comContatoEm30Dias, 0),
      comContatoEm90Dias: carteiras.reduce((s, c) => s + c.comContatoEm90Dias, 0),
      nuncaContatados: carteiras.reduce((s, c) => s + c.nuncaContatados, 0),
      carteiras: carteiras.length,
      processosAbertos: funil.dados.itens.reduce((s, f) => s + f.processos, 0),
      processosComValor: funil.dados.itens.reduce((s, f) => s + f.processosComValor, 0),
      ganhos,
      perdidos,
      tarefasPendentes: painel?.pendentes ?? 0,
      tarefasAtrasadas: painel?.atrasadas ?? 0,
      porLinhaDeNegocio: [...porLinha.entries()]
        .map(([nome, clientes]) => ({ nome, clientes }))
        .sort((a, b) => b.clientes - a.clientes),
      porResponsavel: [...porCen.values()].sort((a, b) => b.clientes - a.clientes),
      perdasPorMotivo: perdas.dados.itens,
      faturamento: faturamento.dados,
      fases: funil.dados.itens,
    };
  } catch (causa) {
    // UMA FILIAL QUE NÃO RESPONDE NÃO VIRA ZERO. Zero somaria ao consolidado
    // como se a filial não tivesse operação; `falhou` deixa a tela contar
    // quantas leituras faltaram e dizer isso em vez de mostrar um total menor.
    if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
    return vazio;
  }
}

/** O envelope que a tela recebe: uma linha por filial, mais a conta do que falhou. */
export type Consolidado = {
  filiais: ConsolidadoDaFilial[];
  /** Quantas filiais não responderam. A tela mostra este número, e não o esconde. */
  falhas: number;
  lidoEm: string;
};

/**
 * O painel da filial do seletor — uma filial, ou "Todas as filiais" numa leitura só (#313).
 *
 * Devolve no formato de `useRecurso`, com a filial lida em `filiais`: as contas da tela (`somarConsolidado`, o mix, o
 * ranking de CENs, o faturamento) são as mesmas, sobre uma linha.
 */
export async function obterConsolidado(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<{ dados: Consolidado; procedencia: null }> {
  const filial = await filialDaSelecao(contexto, sinal);
  const linha = await lerFilial(contexto, filial, sinal);

  return {
    dados: {
      filiais: [linha],
      falhas: linha.falhou ? 1 : 0,
      lidoEm: new Date().toISOString(),
    },
    procedencia: null,
  };
}

/* ------------------------------------------------------------------------ */
/* Os cinco cartões (documento 36)                                           */
/* ------------------------------------------------------------------------ */

/** Os indicadores executivos de UMA filial — ou a falha dela, que nunca vira zero. */
export type ExecutivoDaFilial = {
  filial: Filial;
  painel: PainelExecutivoDaFilial | null;
  erro: Error | null;
};

/** Os cinco cartões somados entre as filiais que responderam. */
export type ExecutivoConsolidado = {
  ano: number;
  filiais: ExecutivoDaFilial[];
  respondidas: number;
  /** A competência mais recente entre as filiais; as que estão em outro mês ficam fora da soma, nomeadas. */
  faturamentoDoMes:
    | (FaturamentoDaCompetencia & { filiaisNoMes: number; filiaisEmOutroMes: string[] })
    | null;
  realizadoDoAno: {
    primeiraCompetencia: string | null;
    ultimaCompetencia: string | null;
    comCliente: number;
    semCliente: number;
    total: number;
  };
  /**
   * O FATURAMENTO PELO ART (29/09/2026): as máquinas entregues somadas entre as filiais — no ano, no mesmo trecho do ano
   * anterior e no mês em curso. Nulo quando nenhuma filial o trouxe.
   */
  entregues: {
    ano: MaquinasEntreguesNoArt | null;
    anterior: MaquinasEntreguesNoArt | null;
    mesEmCurso: MaquinasEntreguesNoArt | null;
    /**
     * Mês a mês, os doze que terminam no mês em curso, somados entre as filiais que trouxeram a série. Nulo quando
     * nenhuma a trouxe (servidor anterior a 29/09/2026).
     */
    porMes: MaquinasEntreguesNoArt[] | null;
    /** Quantas filiais que responderam trouxeram a série mês a mês. */
    filiaisComSerie: number;
  };
  /** Sem `clientesNasCarteirasDaFilial`: essa contagem não se soma. */
  carteira: Omit<CarteiraDaFilial, 'clientesNasCarteirasDaFilial'>;
  cobertura: CoberturaDaFilial;
  mercado: MercadoDaFilial;
};

/**
 * Os cinco cartões, lidos filial a filial pela mesma ponte do consolidado (P-8).
 *
 * É UMA LEITURA SEPARADA do consolidado, e não mais uma dentro de `lerFilial`: trocar o ano do
 * cartão de meta não pode refazer as 65 leituras do painel inteiro. E a falha dela não derruba a
 * filial nos outros gráficos.
 *
 * O ANO É O FISCAL (27/09/2026): novembro a outubro, com o nome do ano em que termina. A rota ainda
 * aceita `ano` como ano civil, e esta tela deixou de pedi-lo.
 */
export async function obterExecutivoConsolidado(
  contexto: ContextoDeAcesso,
  ano: number,
  sinal?: AbortSignal,
): Promise<{ dados: ExecutivoConsolidado; procedencia: null }> {
  // UMA LEITURA, NA FILIAL DO SELETOR (#313): em "Todas as filiais" o servidor soma todas as que a pessoa alcança.
  const filial = await filialDaSelecao(contexto, sinal);
  return { dados: somarExecutivo(ano, [await lerExecutivo(contexto, filial, ano, sinal)]), procedencia: null };
}

/**
 * OS CINCO CARTÕES FILIAL A FILIAL — só a tabela de composição, e só quando alguém a abre em "Todas as filiais" (#313).
 * É a ponte P-8 de antes, fora do caminho da tela: a composição É o detalhe por filial.
 */
export async function obterExecutivoPorFilial(
  contexto: ContextoDeAcesso,
  ano: number,
  sinal?: AbortSignal,
): Promise<{ dados: ExecutivoDaFilial[]; procedencia: null }> {
  const filiais = await listarFiliais(contexto, sinal);
  return { dados: await comLimite(filiais, 4, (filial) => lerExecutivo(contexto, filial, ano, sinal)), procedencia: null };
}

/** Os cinco cartões de uma filial (ou de "Todas") — ou a falha dela, que nunca vira zero. */
async function lerExecutivo(contexto: ContextoDeAcesso, filial: Filial, ano: number, sinal?: AbortSignal): Promise<ExecutivoDaFilial> {
  try {
    const resposta = await ler<PainelExecutivoDaFilial>(
      '/v1/relatorios/indicadores-executivos',
      { ...contexto, empresa: filial.codigo },
      { sinal, parametros: { anoFiscal: ano } },
    );
    return { filial, painel: resposta.dados, erro: null };
  } catch (causa) {
    if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
    return { filial, painel: null, erro: causa instanceof Error ? causa : new Error(String(causa)) };
  }
}

const somar = <T,>(lista: T[], valor: (x: T) => number) => lista.reduce((s, x) => s + valor(x), 0);
const textos = (lista: (string | null | undefined)[]) => lista.filter((t): t is string => !!t).sort();

/**
 * As máquinas entregues no ART somadas entre as filiais. Cada registro é de UMA filial — a da unidade que vendeu —, e por
 * isso a soma não conta nada duas vezes. A janela é a mesma em todas; a menor e a maior ficam, por garantia.
 */
function somarEntregues(lista: (MaquinasEntreguesNoArt | null | undefined)[]): MaquinasEntreguesNoArt | null {
  const vivas = lista.filter((e): e is MaquinasEntreguesNoArt => !!e);
  if (vivas.length === 0) return null;
  return {
    inicio: textos(vivas.map((e) => e.inicio))[0],
    fim: textos(vivas.map((e) => e.fim)).at(-1)!,
    maquinas: somar(vivas, (e) => e.maquinas),
    valor: somar(vivas, (e) => e.valor),
    semValor: somar(vivas, (e) => e.semValor),
    aguardandoNoCrm: somar(vivas, (e) => e.aguardandoNoCrm),
  };
}

/**
 * O FATURAMENTO MÊS A MÊS PELO ART somado entre as filiais, mês com mês (29/09/2026). Cada registro é de uma filial só — a
 * da unidade que vendeu —, e o mesmo mês se soma com ele mesmo; a ordem é a do calendário.
 */
function somarPorMes(listas: (MaquinasEntreguesNoArt[] | null | undefined)[]): MaquinasEntreguesNoArt[] | null {
  const comSerie = listas.filter((l): l is MaquinasEntreguesNoArt[] => !!l && l.length > 0);
  if (comSerie.length === 0) return null;
  const porMes = new Map<string, MaquinasEntreguesNoArt[]>();
  for (const mes of comSerie.flat()) porMes.set(mes.inicio, [...(porMes.get(mes.inicio) ?? []), mes]);
  return [...porMes.keys()].sort().map((inicio) => somarEntregues(porMes.get(inicio)!)!);
}

/**
 * Soma os cinco cartões. CADA NÚMERO É DE UMA PARTIÇÃO QUE NÃO SE SOBREPÕE ENTRE FILIAIS:
 * faturamento pela filial que emitiu, vínculo pela filial da carteira, cliente único pela filial
 * de cadastro, meta e venda perdida pela filial dona. Percentual nunca se soma — ele é refeito
 * aqui a partir do numerador e do denominador somados.
 */
export function somarExecutivo(ano: number, filiais: ExecutivoDaFilial[]): ExecutivoConsolidado {
  const vivas = filiais.filter((f) => f.painel !== null).map((f) => ({ filial: f.filial, i: f.painel!.indicadores }));

  // O MÊS DO CARTÃO É UM SÓ. Somar setembro de uma filial com agosto de outra daria um número de mês
  // nenhum; a filial cuja carga está em outro mês fica fora da soma e aparece nomeada.
  const competencia = textos(vivas.map((v) => v.i.faturamentoDoMes?.competencia)).at(-1) ?? null;
  const noMes = vivas.map((v) => v.i.faturamentoDoMes).filter((m): m is FaturamentoDaCompetencia => m?.competencia === competencia);

  const faturamentoDoMes =
    competencia === null
      ? null
      : {
          competencia,
          comCliente: somar(noMes, (m) => m.comCliente),
          contraparteSemCadastro: somar(noMes, (m) => m.contraparteSemCadastro),
          repasseDeFabrica: somar(noMes, (m) => m.repasseDeFabrica),
          empresaDoGrupo: somar(noMes, (m) => m.empresaDoGrupo),
          outraRevenda: somar(noMes, (m) => m.outraRevenda),
          maquina: somar(noMes, (m) => m.maquina),
          peca: somar(noMes, (m) => m.peca),
          servico: somar(noMes, (m) => m.servico),
          outros: somar(noMes, (m) => m.outros),
          notas: somar(noMes, (m) => m.notas),
          carregadoEm: textos(noMes.map((m) => m.carregadoEm)).at(-1) ?? null,
          semCliente: somar(noMes, (m) => m.semCliente),
          total: somar(noMes, (m) => m.total),
          filiaisNoMes: noMes.length,
          filiaisEmOutroMes: vivas
            .filter((v) => v.i.faturamentoDoMes !== null && v.i.faturamentoDoMes.competencia !== competencia)
            .map((v) => v.filial.nome),
        };

  const anos = vivas.map((v) => v.i.ano);
  const carteiras = vivas.map((v) => v.i.carteira);
  const coberturas = vivas.map((v) => v.i.cobertura);
  const mercados = vivas.map((v) => v.i.mercado);

  return {
    ano,
    filiais,
    respondidas: vivas.length,
    faturamentoDoMes,
    realizadoDoAno: {
      primeiraCompetencia: textos(anos.map((a) => a.primeiraCompetencia))[0] ?? null,
      ultimaCompetencia: textos(anos.map((a) => a.ultimaCompetencia)).at(-1) ?? null,
      comCliente: somar(anos, (a) => a.comCliente),
      semCliente: somar(anos, (a) => a.semCliente),
      total: somar(anos, (a) => a.total),
    },
    entregues: {
      ano: somarEntregues(vivas.map((v) => v.i.entreguesNoAno)),
      anterior: somarEntregues(vivas.map((v) => v.i.entreguesNoMesmoTrechoDoAnoAnterior)),
      mesEmCurso: somarEntregues(vivas.map((v) => v.i.entreguesNoMesEmCurso)),
      porMes: somarPorMes(vivas.map((v) => v.i.entreguesPorMes)),
      filiaisComSerie: vivas.filter((v) => (v.i.entreguesPorMes?.length ?? 0) > 0).length,
    },
    carteira: {
      clientesCadastradosComVinculo: somar(carteiras, (c) => c.clientesCadastradosComVinculo),
      clientes: somar(carteiras, (c) => c.clientes),
      prospects: somar(carteiras, (c) => c.prospects),
      suspects: somar(carteiras, (c) => c.suspects),
      outrasSituacoes: somar(carteiras, (c) => c.outrasSituacoes),
      semDocumento: somar(carteiras, (c) => c.semDocumento),
      vinculos: somar(carteiras, (c) => c.vinculos),
      vinculosComerciais: somar(carteiras, (c) => c.vinculosComerciais),
      carteiras: somar(carteiras, (c) => c.carteiras),
      carteirasComerciais: somar(carteiras, (c) => c.carteirasComerciais),
    },
    cobertura: {
      vinculosComerciais: somar(coberturas, (c) => c.vinculosComerciais),
      elegiveis: somar(coberturas, (c) => c.elegiveis),
      cobertos: somar(coberturas, (c) => c.cobertos),
      foraDaCadencia: somar(coberturas, (c) => c.foraDaCadencia),
      nuncaContatados: somar(coberturas, (c) => c.nuncaContatados),
      semCadencia: somar(coberturas, (c) => c.semCadencia),
      contatoMaisRecente: textos(coberturas.map((c) => c.contatoMaisRecente)).at(-1) ?? null,
      // O catálogo de tipos de atividade é da empresa: o mesmo número em toda filial, e não se soma.
      tiposDeAtividade: coberturas[0]?.tiposDeAtividade ?? 0,
      tiposMarcadosComoVisita: coberturas[0]?.tiposMarcadosComoVisita ?? 0,
      pendentes: somar(coberturas, (c) => c.pendentes),
    },
    mercado: {
      vendasPerdidasRegistradas: somar(mercados, (m) => m.vendasPerdidasRegistradas),
      comConcorrente: somar(mercados, (m) => m.comConcorrente),
      comModeloDoConcorrente: somar(mercados, (m) => m.comModeloDoConcorrente),
      comOsDoisPrecos: somar(mercados, (m) => m.comOsDoisPrecos),
      unidades: somar(mercados, (m) => m.unidades),
      primeiraEm: textos(mercados.map((m) => m.primeiraEm))[0] ?? null,
      ultimaEm: textos(mercados.map((m) => m.ultimaEm)).at(-1) ?? null,
    },
  };
}

/** Soma o consolidado inteiro, ignorando as filiais que não responderam. */
export function somarConsolidado(c: Consolidado | null) {
  const ok = (c?.filiais ?? []).filter((f) => !f.falhou);
  const soma = (f: (x: ConsolidadoDaFilial) => number) => ok.reduce((s, x) => s + f(x), 0);

  return {
    filiais: ok.length,
    clientes: soma((x) => x.clientes),
    em30: soma((x) => x.comContatoEm30Dias),
    em90: soma((x) => x.comContatoEm90Dias),
    nunca: soma((x) => x.nuncaContatados),
    carteiras: soma((x) => x.carteiras),
    abertos: soma((x) => x.processosAbertos),
    comValor: soma((x) => x.processosComValor),
    ganhos: soma((x) => x.ganhos),
    perdidos: soma((x) => x.perdidos),
    pendentes: soma((x) => x.tarefasPendentes),
    atrasadas: soma((x) => x.tarefasAtrasadas),
  };
}

/** Junta as fases de todas as filiais num funil só, somando por fluxo × fase. */
export function funilConsolidado(c: Consolidado | null): FaseDoFunil[] {
  const mapa = new Map<string, FaseDoFunil>();

  for (const filial of c?.filiais ?? []) {
    if (filial.falhou) continue;
    for (const f of filial.fases) {
      const chave = `${f.tipoProcessoCodigo}|${f.faseCodigo}`;
      const atual = mapa.get(chave);
      if (atual) {
        mapa.set(chave, {
          ...atual,
          processos: atual.processos + f.processos,
          processosComValor: atual.processosComValor + f.processosComValor,
          // Nulo mais nulo continua nulo: se nenhuma filial declara valor nesta
          // fase, o consolidado também não declara. Somar como zero afirmaria
          // que a fase vale nada, e o que se sabe é que ninguém preencheu.
          valorTotal:
            atual.valorTotal === null && f.valorTotal === null
              ? null
              : (atual.valorTotal ?? 0) + (f.valorTotal ?? 0),
        });
      } else {
        mapa.set(chave, { ...f });
      }
    }
  }

  return [...mapa.values()];
}

/**
 * Junta as linhas de negócio de todas as filiais — é o "mix" do painel executivo.
 * Cada filial já chega só com as carteiras comerciais (ver `lerFilial`).
 */
export function mixDeLinhas(c: Consolidado | null): { nome: string; clientes: number }[] {
  const mapa = new Map<string, number>();
  for (const filial of c?.filiais ?? []) {
    if (filial.falhou) continue;
    for (const l of filial.porLinhaDeNegocio) {
      mapa.set(l.nome, (mapa.get(l.nome) ?? 0) + l.clientes);
    }
  }
  return [...mapa.entries()]
    .map(([nome, clientes]) => ({ nome, clientes }))
    .sort((a, b) => b.clientes - a.clientes);
}

/** O tipo do envelope que `useRecurso` recebe, para a tela tipar sem repetir. */
export type LeituraConsolidada = ComProcedencia<Consolidado>;

/** Junta os CENs de todas as filiais. Cada carteira tem um responsável só. */
export function censConsolidados(c: Consolidado | null) {
  const mapa = new Map<string, { nome: string; carteiras: number; clientes: number; em30: number; em90: number; nunca: number }>();

  for (const filial of c?.filiais ?? []) {
    if (filial.falhou) continue;
    for (const r of filial.porResponsavel) {
      const atual = mapa.get(r.nome);
      if (atual) {
        atual.carteiras += r.carteiras;
        atual.clientes += r.clientes;
        atual.em30 += r.em30;
        atual.em90 += r.em90;
        atual.nunca += r.nunca;
      } else {
        mapa.set(r.nome, { ...r });
      }
    }
  }

  return [...mapa.values()].sort((a, b) => b.clientes - a.clientes);
}

/** Junta as perdas por motivo de todas as filiais. */
export function perdasConsolidadas(c: Consolidado | null): ContagemPorRotulo[] {
  const mapa = new Map<string, ContagemPorRotulo>();

  for (const filial of c?.filiais ?? []) {
    if (filial.falhou) continue;
    for (const p of filial.perdasPorMotivo) {
      const atual = mapa.get(p.codigo);
      if (atual) mapa.set(p.codigo, { ...atual, quantidade: atual.quantidade + p.quantidade });
      else mapa.set(p.codigo, { ...p });
    }
  }

  return [...mapa.values()].sort((a, b) => b.quantidade - a.quantidade);
}

/**
 * Junta as vendas perdidas de todas as filiais, por motivo e por concorrente.
 *
 * A DIFERENÇA MÉDIA DE PREÇO NÃO SE SOMA — ela se pondera. Somar as médias de treze filiais e
 * dividir por treze daria peso igual a uma filial com uma perda e a outra com quarenta. Aqui a
 * média volta a ser total dividido por denominador, com `comOsDoisPrecos` de cada filial como
 * peso, que é a única forma de o número consolidado significar o mesmo que o número de uma
 * filial só.
 */
export function somarVendasPerdidas(filiais: VendasPerdidas[]): {
  registradas: number;
  processosPerdidos: number;
  porMotivo: FatiaDeVendaPerdida[];
  porConcorrente: FatiaDeVendaPerdida[];
} {
  const juntar = (fatias: FatiaDeVendaPerdida[][]): FatiaDeVendaPerdida[] => {
    const mapa = new Map<string, FatiaDeVendaPerdida & { somaDaDiferenca: number }>();

    for (const lista of fatias) {
      for (const f of lista) {
        const atual = mapa.get(f.codigo);
        const soma = (f.diferencaMediaDePreco ?? 0) * f.comOsDoisPrecos;

        if (atual) {
          atual.quantidade += f.quantidade;
          atual.maquinas += f.maquinas;
          atual.comOsDoisPrecos += f.comOsDoisPrecos;
          atual.somaDaDiferenca += soma;
        } else {
          mapa.set(f.codigo, { ...f, somaDaDiferenca: soma });
        }
      }
    }

    return [...mapa.values()]
      .map(({ somaDaDiferenca, ...f }) => ({
        ...f,
        diferencaMediaDePreco:
          f.comOsDoisPrecos > 0 ? somaDaDiferenca / f.comOsDoisPrecos : null,
      }))
      .sort((a, b) => b.quantidade - a.quantidade);
  };

  return {
    registradas: filiais.reduce((s, f) => s + f.registradas, 0),
    processosPerdidos: filiais.reduce((s, f) => s + f.processosPerdidos, 0),
    porMotivo: juntar(filiais.map((f) => f.porMotivo)),
    porConcorrente: juntar(filiais.map((f) => f.porConcorrente)),
  };
}

/* ------------------------------------------------------------------------ */
/* As perdas e o funil do período (documento 52, 27/09/2026)                 */
/* ------------------------------------------------------------------------ */

/** As vendas perdidas e o alerta de processo parado, somados entre as filiais que responderam. */
export type PerdasEFunilConsolidados = {
  vendas: ReturnType<typeof somarVendasPerdidas>;
  /** O período das vendas perdidas, escrito — o mesmo em toda filial. */
  periodoTexto: string | null;
  /** Os processos parados em Negociação ou Pedido; nulo quando nenhuma filial tem funil. */
  parados: number | null;
  diasParaParado: number;
  /** O motivo verdadeiro de o funil não ter dado — a primeira filial que o disse. */
  motivoSemFunil: string | null;
  respondidas: number;
  /** As filiais cuja leitura falhou, pelo nome. */
  falhas: string[];
};

/**
 * AS PERDAS E O FUNIL DO ANO FISCAL, filial a filial pela mesma ponte do consolidado (P-8).
 *
 * É UMA LEITURA SEPARADA, como a dos cinco cartões: o período dela segue o ano fiscal escolhido, e trocar o ano não
 * pode refazer as leituras do painel inteiro. O ano corrente vai sem `de`/`ate` — o servidor usa o ano fiscal até o
 * último mês fechado —; um ano fechado vai inteiro, de novembro a outubro.
 *
 * O FUNIL VEM PELO FLUXO (`base=etapa`): os alertas usam o fluxo (decisão de 27/09/2026). O parado não depende do
 * período — é o estado de hoje.
 */
export async function obterPerdasEFunilConsolidados(
  contexto: ContextoDeAcesso,
  ano: number,
  anoCorrente: number,
  sinal?: AbortSignal,
): Promise<{ dados: PerdasEFunilConsolidados; procedencia: null }> {
  const periodo = ano === anoCorrente ? {} : { de: `${ano - 1}-11-01`, ate: `${ano}-10-31` };
  // UMA LEITURA DE CADA, NA FILIAL DO SELETOR (#313).
  const filial = await filialDaSelecao(contexto, sinal);
  const daFilial = { ...contexto, empresa: filial.codigo };
  let linha: { filial: Filial; vendas: VendasPerdidas | null; funil: FunilPorEstagio | null };
  try {
    const [vendas, funil] = await Promise.all([
      ler<VendasPerdidas>('/v1/relatorios/vendas-perdidas', daFilial, { sinal, parametros: periodo }),
      ler<FunilPorEstagio>('/v1/relatorios/funil-por-estagio', daFilial, { sinal, parametros: { ...periodo, base: 'etapa' } }),
    ]);
    linha = { filial, vendas: vendas.dados, funil: funil.dados };
  } catch (causa) {
    if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
    linha = { filial, vendas: null, funil: null };
  }
  const linhas = [linha];

  const vivas = linhas.filter((l) => l.vendas !== null && l.funil !== null);
  const comFunil = vivas.filter((l) => l.funil!.paradosEmNegociacaoOuPedido !== null);

  return {
    dados: {
      vendas: somarVendasPerdidas(vivas.map((l) => l.vendas!)),
      periodoTexto: vivas[0]?.vendas!.periodo.texto ?? null,
      parados: comFunil.length > 0 ? comFunil.reduce((s, l) => s + l.funil!.paradosEmNegociacaoOuPedido!, 0) : null,
      diasParaParado: vivas[0]?.funil!.diasParaParado ?? 60,
      motivoSemFunil: comFunil.length > 0 ? null : (vivas[0]?.funil!.metricasSemDado[0]?.motivo ?? null),
      respondidas: vivas.length,
      falhas: linhas.filter((l) => l.vendas === null).map((l) => l.filial.nome),
    },
    procedencia: null,
  };
}

/**
 * Junta o faturamento das treze filiais numa série só, e num ranking só.
 *
 * A SÉRIE SE SOMA POR COMPETÊNCIA, e não se concatena: o mesmo mês existe em cada filial, e
 * emendar as listas produziria treze pontos de janeiro em vez de um.
 *
 * O RANKING DE CLIENTES SE SOMA POR CLIENTE. Um grupo que compra em Ribeirão Preto e em
 * Votuporanga aparece nas duas listas, e é o total dele que interessa à diretoria.
 */
export function faturamentoConsolidado(c: Consolidado | null): {
  competenciaMaisRecente: string | null;
  valorDoUltimoMes: number;
  ultimoMesEstaAberto: boolean;
  serie: MesDeFaturamento[];
  topClientes: ClienteNoRanking[];
} {
  const vivas = (c?.filiais ?? []).filter((f) => !f.falhou && f.faturamento !== null);
  const comDado = vivas.map((f) => f.faturamento!);

  const porMes = new Map<string, MesDeFaturamento>();
  for (const filial of comDado) {
    for (const mes of filial.serie) {
      const atual = porMes.get(mes.competencia);
      if (atual) {
        atual.valorLiquido += mes.valorLiquido;
        atual.clientes += mes.clientes;
        atual.notas += mes.notas;
      } else {
        porMes.set(mes.competencia, { ...mes });
      }
    }
  }

  // OS ÚLTIMOS DOZE MESES DA UNIÃO, e não a união inteira.
  //
  // Cada filial devolve os doze meses dela, ancorados na competência mais recente DELA. Uma
  // filial que parou de faturar em 2024 traz meses de 2024; emendadas, as treze séries cobriam
  // vinte e nove meses num cartão que diz "12 meses". Cortar no fim mantém o cartão honesto e
  // preserva o que interessa: o período recente, que é o que a diretoria compara.
  const serie = [...porMes.values()]
    .sort((a, b) => a.competencia.localeCompare(b.competencia))
    .slice(-MESES_NA_SERIE);

  const porCliente = new Map<string, ClienteNoRanking>();
  for (const filial of comDado) {
    for (const cliente of filial.topClientes) {
      const atual = porCliente.get(cliente.clienteChave);
      if (atual) atual.valorLiquido += cliente.valorLiquido;
      else porCliente.set(cliente.clienteChave, { ...cliente });
    }
  }

  return {
    competenciaMaisRecente: serie.at(-1)?.competencia ?? null,
    valorDoUltimoMes: serie.at(-1)?.valorLiquido ?? 0,
    // BASTA UMA FILIAL COM O MÊS ABERTO para o consolidado ser parcial: o total já não é de um
    // mês inteiro, e apresentá-lo como se fosse subestimaria o consolidado inteiro.
    ultimoMesEstaAberto: comDado.some((f) => f.ultimoMesEstaAberto),
    serie,
    topClientes: [...porCliente.values()]
      .sort((a, b) => b.valorLiquido - a.valorLiquido)
      .slice(0, 5),
  };
}
