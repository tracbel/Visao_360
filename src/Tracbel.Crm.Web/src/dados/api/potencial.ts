/**
 * Os parâmetros do potencial (issues 71 e 77) e o painel de fontes públicas.
 *
 * NÃO HÁ "ALTERAR" NEM "EXCLUIR": mudar um parâmetro é registrar uma vigência nova, e o erro se desfaz
 * revogando a vigência que ainda não passou de hoje. É o que deixa o cálculo de uma data passada usar o
 * parâmetro daquela data.
 */

import type {
  CatalogoDoMercado,
  HistoricoDosParametrosDoPotencial,
  NovaPercepcaoDoGestor,
  NovaRegraDePotencial,
  NovoParametroDoPotencial,
  OpcoesDosParametros,
  PainelDeCoberturaDoMotor,
  PainelDeFontesPublicas,
  ParametrosDoPotencialVigentes,
  ParametrosGeraisDetalhe,
  PercepcaoDoGestorDetalhe,
  RegraDePotencialDetalhe,
} from '../../tipos/potencial';
import { ler, pedir, type ContextoDeAcesso } from './http';

const BASE = '/v1/admin/parametros-do-potencial';

/** Os parâmetros que valem numa data (`aaaa-mm-dd`); sem data, hoje. */
export function obterParametrosDoPotencial(contexto: ContextoDeAcesso, em: string | undefined, sinal?: AbortSignal) {
  return ler<ParametrosDoPotencialVigentes>(BASE, contexto, { parametros: { em }, sinal });
}

/**
 * O CATÁLOGO DE CULTURAS E CATEGORIAS (issue 165).
 *
 * É ele que substitui as listas fixas de cultura que viviam dentro dos painéis
 * de preço e de custo: a tela pede o catálogo e desenha o que vier, e cultura
 * nova passa a entrar pelo Administrador, sem publicação.
 */
export function obterCatalogoDoMercado(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<CatalogoDoMercado>(`${BASE}/catalogo`, contexto, { sinal });
}

/** Todas as vigências, com autor e justificativa. */
export function listarHistoricoDosParametros(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<HistoricoDosParametrosDoPotencial>(`${BASE}/historico`, contexto, { sinal });
}

/** Os produtos da PAM e os municípios da ADR, para as listas de escolha. */
export function listarOpcoesDosParametros(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<OpcoesDosParametros>(`${BASE}/opcoes`, contexto, { sinal });
}

export function informarParametrosGerais(contexto: ContextoDeAcesso, corpo: NovoParametroDoPotencial) {
  return pedir<ParametrosGeraisDetalhe>(`${BASE}/geral`, contexto, { metodo: 'POST', corpo });
}

export function informarRegraDePotencial(contexto: ContextoDeAcesso, corpo: NovaRegraDePotencial) {
  return pedir<RegraDePotencialDetalhe>(`${BASE}/culturas`, contexto, { metodo: 'POST', corpo });
}

export function informarPercepcaoDoGestor(contexto: ContextoDeAcesso, corpo: NovaPercepcaoDoGestor) {
  return pedir<PercepcaoDoGestorDetalhe>(`${BASE}/percepcoes`, contexto, { metodo: 'POST', corpo });
}

/** O que identifica uma vigência para revogar: o tipo, a chave (produto ou município) e a data de início. */
export type AlvoDaRevogacao =
  | { tipo: 'geral'; vigenteDesde: string }
  | { tipo: 'cultura'; produtoCodigoIbge: number; vigenteDesde: string }
  | { tipo: 'percepcao'; municipioCodigoIbge: number; vigenteDesde: string };

/** O caminho da revogação de cada tipo de vigência. */
export function caminhoDaRevogacao(alvo: AlvoDaRevogacao): string {
  switch (alvo.tipo) {
    case 'geral':
      return `${BASE}/geral/${alvo.vigenteDesde}/revogacao`;
    case 'cultura':
      return `${BASE}/culturas/${alvo.produtoCodigoIbge}/${alvo.vigenteDesde}/revogacao`;
    case 'percepcao':
      return `${BASE}/percepcoes/${alvo.municipioCodigoIbge}/${alvo.vigenteDesde}/revogacao`;
  }
}

export function revogarVigencia(contexto: ContextoDeAcesso, alvo: AlvoDaRevogacao, motivo: string) {
  return pedir<unknown>(caminhoDaRevogacao(alvo), contexto, { metodo: 'POST', corpo: { motivo } });
}

/** O painel de fontes públicas. */
export function obterFontesPublicas(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<PainelDeFontesPublicas>('/v1/integracoes/fontes-publicas', contexto, { sinal });
}

/** A cobertura dos dados que o motor de mercado vai usar (issue 150). */
export function obterCoberturaDoMotor(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<PainelDeCoberturaDoMotor>('/v1/integracoes/cobertura-do-motor', contexto, { sinal });
}
