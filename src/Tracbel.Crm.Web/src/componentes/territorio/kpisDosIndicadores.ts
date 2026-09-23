/**
 * As duas linhas de indicadores do topo (issue 170, parte A).
 *
 * A primeira é a carteira da ADR; a segunda é a seção "O mercado da região" —
 * o que o território tem, e que fatia de São Paulo isso representa. Sem o
 * denominador do estado, "62 mil tratores" é um número solto; com ele, é a
 * posição da Tracbel Agro no mercado paulista.
 */

import type { Indicador } from '../cadastro/Indicadores';
import type { IndicadoresTerritoriais, PotencialDoRecorteNoMapa } from '../../tipos/territorio';
import { reaisCompactos, reaisDaProducao } from './escalas';
import { nº, porcento } from './indicadoresDaAdr';
import { diferencaParaASoma, type FatiaNoEstado, type TotaisDaAdr } from './totaisDaAdr';

/** O que as duas linhas precisam saber sobre o estado da leitura. */
export type ContextoDosKpis = {
  indicadores: IndicadoresTerritoriais | null;
  /** Respondeu e o território está carregado. */
  comTerritorio: boolean;
  territorioNaoCarregado: boolean;
  municipiosDaAdr: number;
  totais: TotaisDaAdr;
  fatiaNoEstado: FatiaNoEstado | null;
  coberturaDaAdr: number | null;
  recorte: PotencialDoRecorteNoMapa | null;
  vendasForaDoMapa: number;
  anoDoCenso: number | null;
  anoDoRebanho: number | null;
};

const SEM_TERRITORIO = 'território não carregado neste banco';

export function kpisDaCarteira(c: ContextoDosKpis): Indicador[] {
  return [
    {
      rotulo: 'Municípios da ADR',
      valor: c.comTerritorio ? c.municipiosDaAdr : null,
      deOnde: `${nº(c.totais.clientes)} clientes com endereço neles`,
      semDado: c.indicadores ? SEM_TERRITORIO : 'área de atuação não carregada',
    },
    {
      rotulo: 'Cobertura pela cadência',
      valor: c.comTerritorio && c.coberturaDaAdr !== null ? porcento(c.coberturaDaAdr) : null,
      tom: 'atencao',
      deOnde: `${nº(c.totais.cobertos)} de ${nº(c.totais.elegiveis)} vínculos elegíveis no prazo · ${nº(c.totais.pendentes)} pendentes · regra provisória`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'sem vínculo elegível',
    },
    {
      rotulo: 'Vendas no período',
      valor: c.comTerritorio ? reaisCompactos(c.totais.vendas) : null,
      deOnde: `máquina ${reaisCompactos(c.totais.maquina)} · pós-venda ${reaisCompactos(c.totais.posVenda)} (composição provisória)`,
      semDado: c.territorioNaoCarregado ? `${reaisCompactos(c.vendasForaDoMapa)} no período, todos fora do mapa` : '—',
    },
    {
      rotulo: 'Parque teórico de máquinas',
      valor: c.comTerritorio && c.totais.municipiosComArea > 0 ? nº(Math.round(c.totais.maquinasTeoricas)) : null,
      deOnde: `${nº(Math.round(c.totais.hectares))} ha úteis em ${nº(c.totais.municipiosComArea)} municípios${
        c.recorte?.demandaAnualDeMaquinas != null ? ` · ${nº(Math.round(c.recorte.demandaAnualDeMaquinas))} por ano` : ''
      }${c.totais.potencialEstimado ? ' · estimativa, regra a confirmar' : ''}`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'sem regra de potencial',
    },
  ];
}

export function kpisDoMercado(c: ContextoDosKpis): Indicador[] {
  return [
    {
      rotulo: 'Parque de tratores',
      valor: c.comTerritorio ? nº(c.totais.tratores) : null,
      deOnde: `Censo Agropecuário${c.anoDoCenso ? ` ${c.anoDoCenso}` : ''} · ${nº(c.totais.municipiosComTratores)} municípios divulgados${c.fatiaNoEstado?.tratores != null ? ` · ${porcento(c.fatiaNoEstado.tratores)} do total publicado de São Paulo` : ''}${diferencaParaASoma(c.indicadores?.estado?.tratores)}`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'Censo não carregado',
    },
    {
      rotulo: 'Propriedades',
      valor: c.comTerritorio ? nº(c.totais.estabelecimentos) : null,
      deOnde: `estabelecimentos agropecuários${c.fatiaNoEstado?.estabelecimentos != null ? ` · ${porcento(c.fatiaNoEstado.estabelecimentos)} do total publicado de São Paulo` : ''}${diferencaParaASoma(c.indicadores?.estado?.estabelecimentos)}`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'Censo não carregado',
    },
    {
      rotulo: 'Valor da lavoura',
      valor: c.comTerritorio && c.totais.lavouraValor > 0 ? reaisDaProducao(c.totais.lavouraValor) : null,
      deOnde: `o que a região COLHE, não o que a Tracbel vende · PAM${c.fatiaNoEstado ? ` ${c.fatiaNoEstado.ano}` : ''}${c.fatiaNoEstado?.valor != null ? ` · ${porcento(c.fatiaNoEstado.valor)} de São Paulo` : ''}`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'produção agrícola não carregada',
    },
    {
      rotulo: 'Usinas de etanol',
      valor: c.comTerritorio ? nº(c.totais.usinas) : null,
      deOnde: `em ${nº(c.totais.municipiosComUsina)} municípios · ${nº(c.totais.capacidadeDeEtanol)} m³/dia autorizados (ANP) · não inclui usina só de açúcar`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'ANP não carregada',
    },
    {
      rotulo: 'Rebanho bovino',
      valor: c.comTerritorio && c.totais.bovinos > 0 ? nº(c.totais.bovinos) : null,
      deOnde: `cabeças · Pesquisa da Pecuária Municipal${c.anoDoRebanho ? ` ${c.anoDoRebanho}` : ''}, anual${c.fatiaNoEstado?.rebanho != null ? ` · ${porcento(c.fatiaNoEstado.rebanho)} do total publicado de São Paulo` : ''}${diferencaParaASoma(c.indicadores?.estado?.rebanho)}`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'rebanho não carregado',
    },
  ];
}
