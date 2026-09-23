/**
 * A calculadora de máquinas (issue 161).
 *
 * O que estes testes prendem: a conta **não mora aqui** — a tela manda a área e
 * mostra o que o servidor respondeu; a lista de culturas vem do servidor, e não
 * de uma lista fixa no código; e demanda ausente aparece com o MOTIVO, nunca
 * como um traço mudo.
 */

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ResultadoDaCalculadora, SimulacaoDeMaquinas } from '../../tipos/territorio';
import { Calculadora } from './Calculadora';

const simular = vi.hoisted(() => vi.fn());

vi.mock('../../dados/api/territorio', () => ({ simularMaquinas: simular }));

const CONTEXTO = { usuario: 'cen@exemplo.com', empresa: '010101' };

function resposta(parcial: Partial<ResultadoDaCalculadora> = {}): ResultadoDaCalculadora {
  return {
    data: '2026-09-23',
    municipioCodigoIbge: 3543402,
    municipioNome: 'Ribeirão Preto',
    parqueDeMaquinas: 7,
    demandaAnualDeMaquinas: null,
    areaUtilHectares: 70,
    estimativa: true,
    motivoSemParque: 'Nenhum',
    motivoSemDemanda: 'SemCicloDeRenovacao',
    frase: 'estimativa: a regra de hectares por máquina ainda não foi confirmada pelo comercial',
    porCultura: [
      { culturaCodigo: 'CAFE', cultura: 'Café', compartilhada: [], areaUtilHectares: 70, parque: 7, demandaAnual: null, motivo: 'SemCicloDeRenovacao' },
    ],
    porCategoria: [
      {
        codigo: 'TRATOR',
        nome: 'Trator',
        parqueDeMaquinas: 7,
        demandaAnualDeMaquinas: null,
        areaUtilHectares: 70,
        estimativa: true,
        motivoSemParque: 'Nenhum',
        motivoSemDemanda: 'SemCicloDeRenovacao',
        frase: '',
        porCultura: [],
      },
    ],
    culturas: [
      {
        codigo: 'CAFE',
        nome: 'Café',
        categoriaCodigo: 'TRATOR',
        categoriaNome: 'Trator',
        hectaresPorMaquina: 10,
        anosDeRenovacao: null,
        regraConfirmada: false,
        areaMedidaHectares: 70,
        anoDaArea: 2024,
        areaInformadaHectares: null,
      },
    ],
    sobreOsCenarios: 'O ajuste por cenário de mercado ainda não entra nesta conta (D-P05).',
    ...parcial,
  };
}

describe('Calculadora', () => {
  beforeEach(() => {
    simular.mockReset();
    simular.mockResolvedValue({ dados: resposta(), procedencia: null });
  });

  it('mostra o parque que o servidor devolveu e o motivo de a demanda não sair', async () => {
    render(<Calculadora contexto={CONTEXTO} municipioCodigoIbge={3543402} aoFechar={() => {}} />);

    expect(await screen.findByText('7')).toBeInTheDocument();
    expect(screen.getByText(/70 ha úteis/)).toBeInTheDocument();
    expect(screen.getByText(/de quantos em quantos anos a máquina é trocada/)).toBeInTheDocument();
    expect(screen.getByText(/ainda não foi confirmada pelo comercial/)).toBeInTheDocument();
  });

  it('monta o formulário com as culturas do servidor, e não com uma lista do código', async () => {
    simular.mockResolvedValue({
      dados: resposta({
        culturas: [
          { codigo: 'ABACAXI', nome: 'Abacaxi', categoriaCodigo: 'TRATOR', categoriaNome: 'Trator', hectaresPorMaquina: 40, anosDeRenovacao: 8, regraConfirmada: true, areaMedidaHectares: null, anoDaArea: null, areaInformadaHectares: null },
        ],
      }),
      procedencia: null,
    });

    render(<Calculadora contexto={CONTEXTO} municipioCodigoIbge={null} aoFechar={() => {}} />);

    expect(await screen.findByLabelText('Área de Abacaxi em hectares')).toBeInTheDocument();
  });

  it('não calcula nada sozinha: manda a área digitada ao servidor', async () => {
    render(<Calculadora contexto={CONTEXTO} municipioCodigoIbge={3543402} aoFechar={() => {}} />);

    fireEvent.change(await screen.findByLabelText('Área de Café em hectares'), { target: { value: '500' } });
    fireEvent.click(screen.getByRole('button', { name: 'Simular' }));

    await waitFor(() => {
      const ultima = simular.mock.calls.at(-1)?.[1] as SimulacaoDeMaquinas;
      expect(ultima.areas).toEqual([{ culturaCodigo: 'CAFE', areaHectares: '500' }]);
      expect(ultima.municipioCodigoIbge).toBe('3543402');
    });
  });

  it('sem área digitada, pergunta pela área medida do município', async () => {
    render(<Calculadora contexto={CONTEXTO} municipioCodigoIbge={3543402} aoFechar={() => {}} />);

    await waitFor(() => {
      const primeira = simular.mock.calls[0][1] as SimulacaoDeMaquinas;
      expect(primeira.areas).toBeUndefined();
    });
  });

  it('a recusa do servidor aparece com o motivo dela', async () => {
    const { ErroDaApi } = await import('../../dados/api/http');
    simular.mockRejectedValue(
      new ErroDaApi(
        422,
        {
          type: 'validacao',
          title: 'A simulação tem campos a corrigir.',
          status: 422,
          detail: 'd',
          erros: [
            { campo: 'areas', mensagem: 'A cultura UVA não tem regra de hectares por máquina vigente.', valorRecebido: 'UVA' },
          ],
        },
        'recusou',
      ),
    );

    render(<Calculadora contexto={CONTEXTO} municipioCodigoIbge={null} aoFechar={() => {}} />);

    expect(await screen.findByText(/A cultura UVA não tem regra/)).toBeInTheDocument();
  });

  it('diz que nada é gravado e por que os cenários não entram', async () => {
    render(<Calculadora contexto={CONTEXTO} municipioCodigoIbge={null} aoFechar={() => {}} />);

    expect(await screen.findByText(/D-P05/)).toBeInTheDocument();
    expect(screen.getByText(/Nada aqui é gravado/)).toBeInTheDocument();
  });
});
