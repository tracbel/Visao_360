/**
 * O estoque e a cobertura (28/09/2026) — `GET /api/v1/relatorios/estoque`. Uma leitura só: a API soma pela filial do
 * cabeçalho (ou por todas, em "Todas as filiais").
 */

import type { ComProcedencia } from '../../tipos/api';
import type { EstoqueECobertura } from '../../tipos/estoque';
import { ler, type ContextoDeAcesso } from './http';

export function obterEstoqueECobertura(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<EstoqueECobertura>> {
  return ler<EstoqueECobertura>('/v1/relatorios/estoque', contexto, { sinal });
}
