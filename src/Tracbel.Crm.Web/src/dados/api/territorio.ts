/**
 * O acesso ao painel geográfico e à malha municipal.
 *
 * A MALHA NÃO VEM DA API: é o arquivo público do IBGE em `public/geo`, com o
 * nome oficial em cada polígono. Ela muda por decreto, não por operação — e
 * servida como arquivo estático fica no cache do navegador em vez de trafegar
 * a cada troca de filtro.
 */

import type { ComProcedencia } from '../../tipos/api';
import type { PainelDeCreditoRural, PrecosDeMercado, RentabilidadeDaCultura, SerieDeCusto } from '../../tipos/mercado';
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

/**
 * Os preços das culturas em SP, mês a mês (issue 66). Não depende de filial:
 * preço de mercado é o mesmo para todas.
 */
export function obterPrecosDeMercado(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PrecosDeMercado>> {
  return ler<PrecosDeMercado>('/v1/territorio/precos', contexto, { sinal });
}

/**
 * A rentabilidade por cultura (issue 159): receita, custo e margem por hectare.
 *
 * A margem vem VAZIA COM O MOTIVO enquanto a cultura não tiver local de
 * referência e camada de custo escolhidos (D-P07) — nunca com um local
 * escolhido por conta própria.
 */
export function obterRentabilidadeDasCulturas(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<RentabilidadeDaCultura[]>> {
  return ler<RentabilidadeDaCultura[]>('/v1/territorio/rentabilidade', contexto, { sinal });
}

/** O custo de produção das culturas em SP, das séries da CONAB (issue 67). */
export function obterCustosDeProducao(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<SerieDeCusto[]>> {
  return ler<SerieDeCusto[]>('/v1/territorio/custos', contexto, { sinal });
}

/** O crédito rural de investimento de SP, do SICOR (issue 68). */
export function obterCreditoRural(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PainelDeCreditoRural>> {
  return ler<PainelDeCreditoRural>('/v1/territorio/credito', contexto, { sinal });
}

/** A malha municipal de São Paulo (IBGE, qualidade mínima). */
export async function carregarMalhaDeSaoPaulo(sinal?: AbortSignal): Promise<ColecaoMunicipal> {
  const resposta = await fetch(`${import.meta.env.BASE_URL}geo/sp-municipios.json`, { signal: sinal });
  if (!resposta.ok) throw new Error(`A malha municipal não carregou (HTTP ${resposta.status}).`);
  return (await resposta.json()) as ColecaoMunicipal;
}
