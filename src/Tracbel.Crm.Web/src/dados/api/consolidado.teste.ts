/**
 * O MIX POR LINHA CONTA SÓ CARTEIRA COMERCIAL (24/09/2026).
 *
 * O defeito que isto impede de voltar: `lerFilial` somava por linha de negócio
 * TODAS as carteiras da cobertura. A administrativa — depósito de cadastro, com
 * milhares de vínculos numa linha só — e a de teste entravam no mix como se
 * fossem venda, e a fatia da linha delas crescia sem ninguém vender nada.
 *
 * O teste passa pelo caminho de produção inteiro: `obterConsolidado` lê as
 * rotas por filial (aqui um `fetch` de mentira responde) e `mixDeLinhas` junta.
 * Só a regra do mix muda — o total de clientes do consolidado continua somando
 * todas as carteiras, e o teste prende isso também.
 */

import { afterEach, describe, expect, it, vi } from 'vitest';
import type { ResumoDeCobertura } from '../../tipos/relacionamento';
import { mixDeLinhas, obterConsolidado, somarConsolidado } from './consolidado';

function carteira(linha: string, natureza: string, clientes: number): ResumoDeCobertura {
  return {
    carteiraChave: `${linha}-${natureza}`,
    carteiraCodigo: natureza.slice(0, 3),
    carteiraNome: `Carteira ${natureza}`,
    linhaDeNegocioNome: linha,
    responsavelNome: natureza === 'Comercial' ? 'CEN DE TESTE' : 'SISTEMA',
    clientes,
    comContatoEm30Dias: 0,
    comContatoEm90Dias: 0,
    nuncaContatados: clientes,
    ultimoContatoEm: null,
    naturezaDaCarteira: natureza,
    naturezaDoResponsavel: natureza === 'Comercial' ? 'Pessoa' : 'Sistema',
  };
}

const CARTEIRAS: ResumoDeCobertura[] = [
  carteira('Venda de Peças', 'Comercial', 300),
  carteira('Venda de Máquinas e Implemento', 'Comercial', 100),
  // AS DUAS QUE NÃO ENTRAM: maiores que qualquer comercial, de propósito.
  carteira('Venda de Máquinas e Implemento', 'Administrativa', 9_000),
  carteira('Linha de Teste', 'Teste', 500),
];

/** Uma resposta de leitura, no envelope que o `ler()` espera. */
function envelope(dados: unknown): Response {
  return new Response(JSON.stringify({ dados, procedencia: null }), {
    status: 200,
    headers: { 'Content-Type': 'application/json' },
  });
}

/** As rotas que `lerFilial` lê, para UMA filial; só a cobertura interessa aqui. */
function responder(url: string): Response {
  const caminho = url.replace(/^.*\/api/, '').split('?')[0];
  switch (caminho) {
    case '/v1/catalogos/EMPRESA':
      return envelope([{ codigo: 'EMPRESA', nome: 'Filiais', descricao: null, permiteItemNovo: false, itens: [{ codigo: '010101', descricao: 'Filial de teste', ordem: 1, exigeObservacao: false }] }]);
    case '/v1/relatorios/cobertura':
      return envelope({ itens: CARTEIRAS, metricasSemDado: [] });
    case '/v1/relatorios/agenda':
    case '/v1/relatorios/funil':
    case '/v1/relatorios/perdas':
      return envelope({ itens: [], metricasSemDado: [] });
    case '/v1/relatorios/vendas-perdidas':
      return envelope({ registradas: 0, processosPerdidos: 0, porMotivo: [], porConcorrente: [], metricasSemDado: [] });
    case '/v1/relatorios/faturamento':
      return envelope({ competenciaMaisRecente: null, valorDoUltimoMes: 0, ultimoMesEstaAberto: false, serie: [], topClientes: [], metricasSemDado: [] });
    case '/v1/processos':
      return envelope({ itens: [], pagina: 1, tamanho: 1, total: 0, totalDePaginas: 0, temProxima: false });
    default:
      return new Response('{}', { status: 404 });
  }
}

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('o consolidado da Visão 360', () => {
  it('o mix por linha conta só os vínculos de carteira comercial', async () => {
    vi.stubGlobal('fetch', vi.fn(async (entrada: string) => responder(entrada)));

    const { dados } = await obterConsolidado({ usuario: 'teste@exemplo.invalid', empresa: '010101' });
    const mix = mixDeLinhas(dados);

    expect(mix).toEqual([
      { nome: 'Venda de Peças', clientes: 300 },
      { nome: 'Venda de Máquinas e Implemento', clientes: 100 },
    ]);
    expect(mix.map((l) => l.nome)).not.toContain('Linha de Teste');
  });

  it('o total de clientes do consolidado continua somando todas as carteiras — só o mix mudou', async () => {
    vi.stubGlobal('fetch', vi.fn(async (entrada: string) => responder(entrada)));

    const { dados } = await obterConsolidado({ usuario: 'teste@exemplo.invalid', empresa: '010101' });

    expect(somarConsolidado(dados).clientes).toBe(300 + 100 + 9_000 + 500);
  });
});
