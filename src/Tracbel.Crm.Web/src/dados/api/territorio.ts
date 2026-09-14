/**
 * O acesso ao painel geográfico e à malha municipal.
 *
 * A MALHA NÃO VEM DA API: é o arquivo público do IBGE em `public/geo`, com o
 * nome oficial em cada polígono. Ela muda por decreto, não por operação — e
 * servida como arquivo estático fica no cache do navegador em vez de trafegar
 * a cada troca de filtro.
 */

import type { ComProcedencia } from '../../tipos/api';
import type { FiltrosTerritoriais, PainelTerritorial } from '../../tipos/territorio';
import type { ColecaoMunicipal } from '../../componentes/territorio/projecao';
import { ler, type ContextoDeAcesso } from './http';

/** Os indicadores por município, dentro da fronteira de acesso. */
export function obterIndicadoresTerritoriais(
  contexto: ContextoDeAcesso,
  filtros: FiltrosTerritoriais,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PainelTerritorial>> {
  return ler<PainelTerritorial>('/v1/territorio/indicadores', contexto, {
    sinal,
    parametros: {
      competenciaInicial: filtros.competenciaInicial,
      competenciaFinal: filtros.competenciaFinal,
      regiao: filtros.regiao,
      lojaCodigo: filtros.lojaCodigo,
      visao: filtros.visao,
      filialDaVenda: filtros.filialDaVenda,
      filialDoCliente: filtros.filialDoCliente,
    },
  });
}

/** A malha municipal de São Paulo (IBGE, qualidade mínima). */
export async function carregarMalhaDeSaoPaulo(sinal?: AbortSignal): Promise<ColecaoMunicipal> {
  const resposta = await fetch(`${import.meta.env.BASE_URL}geo/sp-municipios.json`, { signal: sinal });
  if (!resposta.ok) throw new Error(`A malha municipal não carregou (HTTP ${resposta.status}).`);
  return (await resposta.json()) as ColecaoMunicipal;
}
