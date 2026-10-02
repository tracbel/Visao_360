/**
 * AS ORDENS DE SERVIÇO NA FICHA DA MÁQUINA (02/10/2026). O que se prende: a aberta há mais de 45 dias leva o selo da faixa
 * vermelha e os dias; a OS sem cliente do CRM diz que não casou; o horímetro sai com uma casa; e, sem OS, o quadro diz o
 * motivo que o servidor mandou.
 */

import { render, screen, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import type { OrdensDeServicoResumidas } from '../../tipos/ordensDeServico';
import { OrdensDeServicoDaMaquina } from './OrdensDeServicoDaMaquina';

const equipamentos = vi.hoisted(() => ({ listarOrdensDeServicoDoEquipamento: vi.fn() }));
vi.mock('../../dados/api/equipamentos', () => equipamentos);

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'frota.OrdemDeServico', lidoEmUtc: '2026-10-02T12:00:00Z', dadoMaisRecenteEm: null, estaDesatualizado: false, aviso: null };

const base = {
  filialCodigo: '010101', filialNome: 'Ribeirão Preto', tipoDeAtendimento: 'CAMPO', liberadaEm: null, canceladaEm: null,
  chassi: '1RW7250PVMR000001', modelo: 'TRATOR 7250R', equipamentoChave: 'm1', itensDePeca: 1, itensDeServico: 1,
};

const ordens: OrdensDeServicoResumidas = {
  emAberto: 1, emAbertoHaMaisDe45Dias: 1, diasDaMaisAntigaEmAberto: 50, valorEmAberto: 1500, nosUltimos12Meses: 1,
  pecasNosUltimos12Meses: 300, servicosNosUltimos12Meses: 150, ultimaAbertaEm: '2026-08-13', totalDeOrdens: 2,
  ordens: [
    {
      ordem: { ...base, chave: 'os1', numero: '00012345', situacao: 'Aberta', abertaEm: '2026-08-13', fechadaEm: null, horimetro: 3420.5,
        clienteChave: 'c1', clienteNome: 'Cliente de amostra', valorDePecas: 1000, valorDeServicos: 500 },
      diasEmAberto: 50,
    },
    {
      ordem: { ...base, chave: 'os2', numero: '00011002', situacao: 'Fechada', abertaEm: '2026-03-02', fechadaEm: '2026-03-09', horimetro: null,
        clienteChave: null, clienteNome: null, valorDePecas: 300, valorDeServicos: 150 },
      diasEmAberto: null,
    },
  ],
  carregadoEm: '2026-10-02T09:00:00Z',
  metricasSemDado: [],
};

function montar() {
  render(
    <MemoryRouter>
      <ProvedorDeContextoDeAcesso>
        <OrdensDeServicoDaMaquina chave="m1" />
      </ProvedorDeContextoDeAcesso>
    </MemoryRouter>,
  );
}

describe('OrdensDeServicoDaMaquina', () => {
  afterEach(() => {
    vi.clearAllMocks();
    guardado.clear();
  });

  it('mostra a aberta na faixa vermelha, a fechada sem cliente casado e o horímetro de cada OS', async () => {
    equipamentos.listarOrdensDeServicoDoEquipamento.mockResolvedValue({ dados: ordens, procedencia: PROCEDENCIA });

    montar();

    const aberta = (await screen.findByText('00012345')).closest('tr') as HTMLElement;
    expect(within(aberta).getByText('aberta')).toHaveClass('cad-selo-pendente');
    expect(within(aberta).getByText('há 50 dias')).toBeInTheDocument();
    expect(within(aberta).getByText(/3\.420,5 h/)).toBeInTheDocument();
    expect(within(aberta).getByRole('link', { name: 'Cliente de amostra' })).toHaveAttribute('href', '/clientes/c1');

    const fechada = screen.getByText('00011002').closest('tr') as HTMLElement;
    expect(within(fechada).getByText('fechada')).not.toHaveClass('cad-selo-pendente');
    expect(within(fechada).getByText('não casou com cliente do CRM')).toBeInTheDocument();
    expect(screen.getByText(/2 OS · 1 na oficina agora/)).toBeInTheDocument();
  });

  it('sem OS, diz o motivo do servidor', async () => {
    equipamentos.listarOrdensDeServicoDoEquipamento.mockResolvedValue({
      dados: {
        ...ordens, emAberto: 0, totalDeOrdens: 0, ordens: [],
        metricasSemDado: [{ metrica: 'ordensDeServicoDoRecorte', motivo: 'Nenhuma ordem de serviço desta máquina nos últimos três anos.' }],
      },
      procedencia: PROCEDENCIA,
    });

    montar();

    expect(await screen.findByText(/Nenhuma ordem de serviço desta máquina/)).toBeInTheDocument();
  });
});
