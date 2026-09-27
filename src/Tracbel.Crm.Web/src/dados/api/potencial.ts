/**
 * Os parâmetros do potencial (issues 71 e 77) e o painel de fontes públicas.
 *
 * NÃO HÁ "ALTERAR" NEM "EXCLUIR": mudar um parâmetro é registrar uma vigência nova, e o erro se desfaz
 * revogando a vigência que ainda não passou de hoje. É o que deixa o cálculo de uma data passada usar o
 * parâmetro daquela data.
 */

import type {
  CatalogoDoMercado,
  HistoricoDoPlanejamento,
  HistoricoDosParametrosDoPotencial,
  NovaPercepcaoDoGestor,
  NovaRegraDePotencial,
  NovoParametroDoPlanejamento,
  NovoParametroDoPotencial,
  NovoShareAlvo,
  OpcoesDosParametros,
  PainelDeCoberturaDoMotor,
  PainelDeFontesPublicas,
  ParametroDoPlanejamentoDetalhe,
  ParametrosDoPlanejamentoVigentes,
  ParametrosDoPotencialVigentes,
  ParametrosGeraisDetalhe,
  PercepcaoDoGestorDetalhe,
  RegraDePotencialDetalhe,
  ShareAlvoDetalhe,
  SugestaoDasBandasDePorte,
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

/** As bandas de porte pelos tercis dos municípios da ADR (issue 166) — só calcula; quem grava é a vigência. */
export function sugerirBandasDePorte(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<SugestaoDasBandasDePorte>(`${BASE}/geral/porte-pelos-tercis`, contexto, { sinal });
}

export function informarRegraDePotencial(contexto: ContextoDeAcesso, corpo: NovaRegraDePotencial) {
  return pedir<RegraDePotencialDetalhe>(`${BASE}/culturas`, contexto, { metodo: 'POST', corpo });
}

export function informarPercepcaoDoGestor(contexto: ContextoDeAcesso, corpo: NovaPercepcaoDoGestor) {
  return pedir<PercepcaoDoGestorDetalhe>(`${BASE}/percepcoes`, contexto, { metodo: 'POST', corpo });
}

/** A sazonalidade, os pesos do IOC e o share-alvo que valem numa data (issue 256); sem data, hoje. */
export function obterParametrosDoPlanejamento(contexto: ContextoDeAcesso, em: string | undefined, sinal?: AbortSignal) {
  return ler<ParametrosDoPlanejamentoVigentes>(`${BASE}/planejamento`, contexto, { parametros: { em }, sinal });
}

/** Todas as vigências do planejamento, com autor e justificativa. */
export function listarHistoricoDoPlanejamento(contexto: ContextoDeAcesso, sinal?: AbortSignal) {
  return ler<HistoricoDoPlanejamento>(`${BASE}/planejamento/historico`, contexto, { sinal });
}

export function informarParametroDoPlanejamento(contexto: ContextoDeAcesso, corpo: NovoParametroDoPlanejamento) {
  return pedir<ParametroDoPlanejamentoDetalhe>(`${BASE}/planejamento`, contexto, { metodo: 'POST', corpo });
}

export function informarShareAlvo(contexto: ContextoDeAcesso, corpo: NovoShareAlvo) {
  return pedir<ShareAlvoDetalhe>(`${BASE}/planejamento/shares`, contexto, { metodo: 'POST', corpo });
}

/** O que identifica uma vigência para revogar: o tipo, a chave (produto ou município) e a data de início. */
export type AlvoDaRevogacao =
  | { tipo: 'geral'; vigenteDesde: string }
  // A CATEGORIA ENTRA NA CHAVE (D-P01): um produto passa a ter mais de uma regra na mesma data — o trator e
  // a colheitadeira do café —, e sem ela a revogação não sabe qual das duas derrubar.
  | { tipo: 'cultura'; produtoCodigoIbge: number; categoriaDeMaquinaCodigo: string; vigenteDesde: string }
  | { tipo: 'percepcao'; municipioCodigoIbge: number; vigenteDesde: string }
  | { tipo: 'planejamento'; vigenteDesde: string }
  | { tipo: 'share'; categoriaDeMaquinaCodigo: string; vigenteDesde: string };

/** O caminho da revogação de cada tipo de vigência. */
export function caminhoDaRevogacao(alvo: AlvoDaRevogacao): string {
  switch (alvo.tipo) {
    case 'geral':
      return `${BASE}/geral/${alvo.vigenteDesde}/revogacao`;
    case 'cultura':
      return `${BASE}/culturas/${alvo.produtoCodigoIbge}/${encodeURIComponent(alvo.categoriaDeMaquinaCodigo)}/${alvo.vigenteDesde}/revogacao`;
    case 'percepcao':
      return `${BASE}/percepcoes/${alvo.municipioCodigoIbge}/${alvo.vigenteDesde}/revogacao`;
    case 'planejamento':
      return `${BASE}/planejamento/${alvo.vigenteDesde}/revogacao`;
    case 'share':
      return `${BASE}/planejamento/shares/${encodeURIComponent(alvo.categoriaDeMaquinaCodigo)}/${alvo.vigenteDesde}/revogacao`;
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
