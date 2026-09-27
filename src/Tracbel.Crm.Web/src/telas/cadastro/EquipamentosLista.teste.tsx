/**
 * A LISTA DE EQUIPAMENTOS FILTRADA POR CLIENTE (27/09/2026). O "Ver todas" do 360 abre `/equipamentos?cliente=`, e
 * até aqui a lista ignorava o parâmetro e mostrava a filial inteira. O que se prende:
 *
 * - o cliente da rota vai para a consulta, e a lista diz de quem são as máquinas;
 * - com o filtro, a coluna "Relação com o cliente" diz o que cada máquina é para ele;
 * - a coluna do dono mostra o dono atual da sincronia, com a evidência, quando não há dono confirmado;
 * - "Ver todas as máquinas" tira o filtro.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import type { EquipamentoResumo } from '../../tipos/api';
import { EquipamentosLista } from './EquipamentosLista';

const equipamentos = vi.hoisted(() => ({ listarEquipamentos: vi.fn() }));
vi.mock('../../dados/api/equipamentos', async (original) => ({
  ...(await original<typeof import('../../dados/api/equipamentos')>()),
  listarEquipamentos: equipamentos.listarEquipamentos,
}));

const clientes = vi.hoisted(() => ({ obterCliente: vi.fn() }));
vi.mock('../../dados/api/clientes', () => clientes);

vi.mock('../../dados/api/catalogos', async (original) => ({
  ...(await original<typeof import('../../dados/api/catalogos')>()),
  useCatalogos: () => ({ catalogos: null, carregando: false, erro: null }),
}));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'frota.Equipamento', lidoEmUtc: '2026-09-27T12:00:00Z', dadoMaisRecenteEm: null, estaDesatualizado: false, aviso: null };
const CHAVE = '11111111-2222-3333-4444-555555555555';

const pelaNota: EquipamentoResumo = {
  chave: 'm1', chassi: '1RW7250PVMR000001', modeloCodigo: null, modeloNome: '7250R', marca: 'John Deere', marcaRepresentada: true,
  situacao: 'ProprietarioNaoConfirmado', origem: 'Protheus', anoModelo: 2021, clienteChave: null, clienteNome: null,
  criadoEm: '2026-09-24T20:41:06', alteradoEm: null, estaInativo: false, classificacaoCodigo: null, classificacaoNome: null,
  porte: null, vendas: 0, ultimaVendaEm: null, compradorNaUltimaVendaChave: null, compradorNaUltimaVendaNome: null,
  naturezaDoVinculo: null, produtoNaOrigem: null, sistemaDaVenda: null, donoAtualChave: CHAVE, donoAtualNome: 'Cliente de amostra',
  evidenciaDoDonoAtual: 'OrdemDeServico', evidenciaDoDonoAtualEm: '2025-08-01',
  relacaoComOCliente: { ehDonoAtual: true, ehDonoConfirmado: false, compradaEm: null, compradaPeloDonoNoProtheus: false },
};

function pagina(itens: EquipamentoResumo[]) {
  return { dados: { itens, pagina: 1, tamanho: 25, total: itens.length, totalDePaginas: 1, temProxima: false }, procedencia: PROCEDENCIA };
}

function montar(rota: string) {
  render(
    <MemoryRouter initialEntries={[rota]}>
      <ProvedorDeContextoDeAcesso>
        <Routes>
          <Route path="/equipamentos" element={<EquipamentosLista />} />
        </Routes>
      </ProvedorDeContextoDeAcesso>
    </MemoryRouter>,
  );
}

describe('EquipamentosLista filtrada por cliente', () => {
  afterEach(() => {
    vi.clearAllMocks();
    guardado.clear();
  });

  it('lê o cliente da rota, mostra a relação de cada máquina e sai do filtro por "Ver todas as máquinas"', async () => {
    equipamentos.listarEquipamentos.mockResolvedValue(pagina([pelaNota]));
    clientes.obterCliente.mockResolvedValue({ dados: { chave: CHAVE, nomeRazao: 'Cliente de amostra' }, procedencia: PROCEDENCIA });

    montar(`/equipamentos?cliente=${CHAVE}`);

    expect(await screen.findByText('Máquinas do cliente')).toBeInTheDocument();
    expect(equipamentos.listarEquipamentos.mock.calls[0][1]).toMatchObject({ clienteChave: CHAVE });
    const aviso = (await screen.findByText(/Máquinas de/)).closest('.cad-aviso-cliente') as HTMLElement;
    expect(await within(aviso).findByRole('link', { name: 'Cliente de amostra' })).toHaveAttribute('href', `/clientes/${CHAVE}`);
    expect(screen.getByRole('columnheader', { name: 'Relação com o cliente' })).toBeInTheDocument();

    const linha = screen.getByText('1RW7250PVMR000001').closest('tr') as HTMLElement;
    expect(within(linha).getAllByText(/por ordem de serviço no Protheus de 01\/08\/2025/).length).toBeGreaterThan(0);
    expect(within(linha).getByText('dono atual', { selector: '.cad-selo' })).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Ver todas as máquinas' }));
    await waitFor(() => expect(screen.getByText('Máquinas desta filial')).toBeInTheDocument());
    expect(equipamentos.listarEquipamentos.mock.calls.at(-1)?.[1]).toMatchObject({ clienteChave: '' });
    expect(screen.queryByRole('columnheader', { name: 'Relação com o cliente' })).not.toBeInTheDocument();
  });

  it('sem o filtro, a coluna do dono mostra o dono atual da sincronia com a evidência', async () => {
    equipamentos.listarEquipamentos.mockResolvedValue(pagina([{ ...pelaNota, relacaoComOCliente: null }]));

    montar('/equipamentos');

    const linha = (await screen.findByText('1RW7250PVMR000001')).closest('tr') as HTMLElement;
    expect(within(linha).getByRole('link', { name: 'Cliente de amostra' })).toHaveAttribute('href', `/clientes/${CHAVE}`);
    expect(within(linha).getByText(/dono atual/)).toBeInTheDocument();
    expect(clientes.obterCliente).not.toHaveBeenCalled();
  });
});
