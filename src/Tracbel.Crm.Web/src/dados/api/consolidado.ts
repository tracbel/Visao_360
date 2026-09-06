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
 */

import type { CatalogoDeSelecao, ComProcedencia, ItemDeSelecao, PaginaDe } from '../../tipos/api';
import { CATALOGO } from '../../tipos/api';
import type {
  Agregado,
  ContagemPorRotulo,
  FaseDoFunil,
  Faturamento,
  FatiaDeVendaPerdida,
  PainelDaAgenda,
  ProcessoResumo,
  ClienteNoRanking,
  MesDeFaturamento,
  ResumoDeCobertura,
  VendasPerdidas,
} from '../../tipos/relacionamento';
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
  /** As linhas de negócio desta filial, com o número de vínculos de cada uma. */
  porLinhaDeNegocio: { nome: string; clientes: number }[];
  /** Os CENs desta filial, com as carteiras deles somadas. */
  porResponsavel: { nome: string; carteiras: number; clientes: number; em30: number; em90: number; nunca: number }[];
  /**
   * As perdas por motivo desta filial, contadas em `processo.Processo`.
   *
   * O motivo é "não informado" em 100% — o Vórtice encerra o processo sem coluna de motivo.
   * Quem responde por quê é `vendasPerdidas`, que lê o formulário do CEN.
   */
  perdasPorMotivo: ContagemPorRotulo[];

  /** As vendas perdidas registradas no formulário: motivo, concorrente e diferença de preço. */
  vendasPerdidas: VendasPerdidas | null;

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

/** Roda as promessas com um teto de simultaneidade, para não abrir 65 conexões de uma vez. */
async function comLimite<T, R>(itens: T[], limite: number, tarefa: (item: T) => Promise<R>): Promise<R[]> {
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
    vendasPerdidas: null,
    faturamento: null,
    fases: [],
  };

  try {
    const [cobertura, agenda, funil, perdas, vendasPerdidas, faturamento, ganhos, perdidos] =
      await Promise.all([
      ler<Agregado<ResumoDeCobertura>>('/v1/relatorios/cobertura', contexto, { sinal }),
      ler<Agregado<PainelDaAgenda>>('/v1/relatorios/agenda', contexto, { sinal }),
      ler<Agregado<FaseDoFunil>>('/v1/relatorios/funil', contexto, { sinal }),
      ler<Agregado<ContagemPorRotulo>>('/v1/relatorios/perdas', contexto, { sinal }),
      ler<VendasPerdidas>('/v1/relatorios/vendas-perdidas', contexto, { sinal }),
      ler<Faturamento>('/v1/relatorios/faturamento', contexto, { sinal }),
      contar(contexto, 'Ganho', sinal),
      contar(contexto, 'Perdido', sinal),
    ]);

    const carteiras = cobertura.dados.itens;
    const painel = agenda.dados.itens[0];

    const porLinha = new Map<string, number>();
    for (const c of carteiras) {
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
      vendasPerdidas: vendasPerdidas.dados,
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
 * O consolidado das filiais em operação.
 *
 * Devolve no formato de `useRecurso` para a tela não precisar saber que por
 * trás disso existem 65 requisições.
 */
export async function obterConsolidado(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<{ dados: Consolidado; procedencia: null }> {
  const filiais = await listarFiliais(contexto, sinal);
  const linhas = await comLimite(filiais, 4, (f) => lerFilial(contexto, f, sinal));

  return {
    dados: {
      filiais: linhas,
      falhas: linhas.filter((l) => l.falhou).length,
      lidoEm: new Date().toISOString(),
    },
    procedencia: null,
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

/** Junta as linhas de negócio de todas as filiais — é o "mix" do painel executivo. */
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
export function vendasPerdidasConsolidadas(c: Consolidado | null): {
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

  const vivas = (c?.filiais ?? []).filter((f) => !f.falhou && f.vendasPerdidas !== null);

  return {
    registradas: vivas.reduce((s, f) => s + (f.vendasPerdidas?.registradas ?? 0), 0),
    processosPerdidos: vivas.reduce((s, f) => s + (f.vendasPerdidas?.processosPerdidos ?? 0), 0),
    porMotivo: juntar(vivas.map((f) => f.vendasPerdidas!.porMotivo)),
    porConcorrente: juntar(vivas.map((f) => f.vendasPerdidas!.porConcorrente)),
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
