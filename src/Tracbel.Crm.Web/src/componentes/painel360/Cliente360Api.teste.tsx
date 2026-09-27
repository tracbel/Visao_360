/**
 * O 360 DO CLIENTE COM O DADO CRUZADO (27/09/2026). O que se prende:
 *
 * - a frota diz, máquina a máquina, a relação com o cliente e a evidência do dono atual — e a máquina que ele só
 *   comprou no ART diz quem é o dono hoje;
 * - o faturamento traz o total dos doze meses, a quebra em reais, a filial que emitiu a nota e a DATA DA CARGA;
 * - as carteiras trazem o CEN, a filial, a classe do cadastro e a cadência — e o último contato vazio vem como traço
 *   com o motivo;
 * - o bloco "O que a ficha não pode mostrar" não repete as frases falsas de antes (faturamento e frota "parados").
 */

import { render, screen, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import type { EquipamentoResumo } from '../../tipos/api';
import type { CarteirasDoCliente, FaturamentoDoCliente } from '../../tipos/relacionamento';
import { Cliente360Api } from './Cliente360Api';

const clientes = vi.hoisted(() => ({ obterCliente: vi.fn() }));
vi.mock('../../dados/api/clientes', () => clientes);

const equipamentos = vi.hoisted(() => ({ listarEquipamentos: vi.fn(), CONSULTA_INICIAL: { pagina: 1, tamanho: 25 } }));
vi.mock('../../dados/api/equipamentos', () => equipamentos);

const relacionamento = vi.hoisted(() => ({
  listarProcessos: vi.fn(),
  listarTarefas: vi.fn(),
  listarInteracoes: vi.fn(),
  obterFaturamentoDoCliente: vi.fn(),
  listarCarteirasDoCliente: vi.fn(),
  PROCESSOS_INICIAL: {},
  TAREFAS_INICIAL: {},
  INTERACOES_INICIAL: {},
}));
vi.mock('../../dados/api/relacionamento', () => relacionamento);

// O jsdom não tem canvas nem ResizeObserver: o gráfico entra como dublê, com os valores à vista.
vi.mock('../GraficoLinhaMensal', () => ({
  GraficoLinhaMensal: ({ valores, ultimoParcial }: { valores: number[]; ultimoParcial?: boolean }) => (
    <div data-grafico="linha" data-valores={valores.join(',')} data-parcial={String(Boolean(ultimoParcial))} />
  ),
}));
vi.mock('../MolduraDeGrafico', () => ({
  MolduraDeGrafico: ({ children, altura }: { children: (l: number, a: number) => React.ReactNode; altura: number }) => (
    <div>{children(600, altura)}</div>
  ),
}));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'teste', lidoEmUtc: '2026-09-27T12:00:00Z', dadoMaisRecenteEm: null, estaDesatualizado: false, aviso: null };
const CHAVE = '11111111-2222-3333-4444-555555555555';

const vazia = { itens: [], pagina: 1, tamanho: 8, total: 0, totalDePaginas: 1, temProxima: false };

const maquinaBase: EquipamentoResumo = {
  chave: 'm1', chassi: '1RW7250PVMR000001', modeloCodigo: null, modeloNome: '7250R', marca: 'John Deere',
  marcaRepresentada: true, situacao: 'ProprietarioNaoConfirmado', origem: 'Protheus', anoModelo: 2021, clienteChave: null,
  clienteNome: null, criadoEm: '2026-09-24T20:41:06', alteradoEm: null, estaInativo: false, classificacaoCodigo: null,
  classificacaoNome: 'Trator grande', porte: 'Grande', vendas: 0, ultimaVendaEm: null, compradorNaUltimaVendaChave: null,
  compradorNaUltimaVendaNome: null, naturezaDoVinculo: null, produtoNaOrigem: null, sistemaDaVenda: null,
  donoAtualChave: CHAVE, donoAtualNome: 'Cliente de amostra', evidenciaDoDonoAtual: 'NotaDeVenda', evidenciaDoDonoAtualEm: '2025-03-10',
  relacaoComOCliente: { ehDonoAtual: true, ehDonoConfirmado: false, compradaEm: null, compradaPeloDonoNoProtheus: false },
};

const revendida: EquipamentoResumo = {
  ...maquinaBase,
  chave: 'm2', chassi: '1RW6135MCMR000002', modeloNome: '6135M', origem: 'Art', donoAtualChave: 'outro',
  donoAtualNome: 'Outro dono', evidenciaDoDonoAtual: 'OrdemDeServico', evidenciaDoDonoAtualEm: '2025-08-01',
  relacaoComOCliente: { ehDonoAtual: false, ehDonoConfirmado: false, compradaEm: '2023-03-20', compradaPeloDonoNoProtheus: false },
};

const faturamento: FaturamentoDoCliente = {
  primeiraCompetenciaDaCarga: '2023-09-01',
  competenciaMaisRecente: '2026-09-01',
  carregadoEm: '2026-09-24T16:27:48',
  ultimoMesEstaIncompleto: true,
  ultimaCompraEm: '2026-09-01',
  dozeMeses: { de: '2025-10-01', ate: '2026-09-01', valorLiquido: 591853.92, maquina: 281500, peca: 300000, servico: 10353.92, outros: 0, notas: 40 },
  janelaCarregada: { de: '2023-09-01', ate: '2026-09-01', valorLiquido: 1024795.06, maquina: 399500, peca: 615000, servico: 10295.06, outros: 0, notas: 70 },
  serie: Array.from({ length: 12 }, (_, i) => ({
    competencia: `2026-${String(i + 1).padStart(2, '0')}-01`, valorLiquido: i === 11 ? 5000 : 1000, maquina: 0, notas: 1,
  })),
  porFilial: [
    { filialCodigo: '010116', filialNome: 'Votuporanga', dozeMeses: 530765.47, maquinaNosDozeMeses: 251000, notasNosDozeMeses: 30, naJanela: 840785.87, ultimaNotaEm: '2026-09-01' },
    { filialCodigo: '010118', filialNome: 'Marília', dozeMeses: 0, maquinaNosDozeMeses: 0, notasNosDozeMeses: 0, naJanela: 118000, ultimaNotaEm: '2025-04-01' },
  ],
  metricasSemDado: [{ metrica: 'mesIncompleto', motivo: '09/2026 não está inteiro: a carga gravou esse mês até 24/09/2026 16:27 (UTC).' }],
};

const carteiras: CarteirasDoCliente = {
  classe: 'A',
  classeApuradaEm: '2026-09-24T16:28:00',
  carteiras: [
    {
      carteiraChave: 'c1', carteiraCodigo: 'MAQ_16VOT_03_302', carteiraNome: 'Máquinas Votuporanga', naturezaDaCarteira: 'Comercial',
      linhaDeNegocioNome: 'Máquinas novos', responsavelNome: 'CEN de amostra', naturezaDoResponsavel: 'Pessoa',
      filialCodigo: '010116', filialNome: 'Votuporanga', diasDeCadencia: 180, vinculadoEm: '2025-01-06T19:49:52',
      ultimaInteracaoEm: null, diasSemContato: null, estaForaDaCadencia: null,
    },
  ],
  metricasSemDado: [{ metrica: 'ultimoContato', motivo: 'Nenhum dos 1 vínculos deste cliente tem data de último contato.' }],
};

function montar() {
  render(
    <MemoryRouter>
      <ProvedorDeContextoDeAcesso>
        <Cliente360Api chave={CHAVE} aoLimpar={() => {}} />
      </ProvedorDeContextoDeAcesso>
    </MemoryRouter>,
  );
}

describe('Cliente360Api', () => {
  afterEach(() => {
    vi.clearAllMocks();
    guardado.clear();
  });

  it('mostra a frota pelo dono atual, o faturamento com a data da carga e as carteiras com o CEN', async () => {
    clientes.obterCliente.mockResolvedValue({
      dados: {
        chave: CHAVE, nomeRazao: 'Cliente de amostra', nomeFantasia: null, tipoDePessoa: 'Juridica', documento: null,
        situacao: 'Cliente', criadoEm: '2026-09-24T12:00:00', alteradoEm: null, estaInativo: false, documentoSemMascara: null,
        inscricaoEstadual: null, atividadeEconomica: null, situacaoDesde: '2026-09-24T12:00:00', empresaId: 1, proprietarioId: 1,
        origemCodigo: null, motivoInativacaoCodigo: null, versao: null,
      },
      procedencia: PROCEDENCIA,
    });
    equipamentos.listarEquipamentos.mockResolvedValue({
      dados: { ...vazia, itens: [maquinaBase, revendida], total: 2 },
      procedencia: PROCEDENCIA,
    });
    relacionamento.listarProcessos.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.listarTarefas.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.listarInteracoes.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.obterFaturamentoDoCliente.mockResolvedValue({ dados: faturamento, procedencia: PROCEDENCIA });
    relacionamento.listarCarteirasDoCliente.mockResolvedValue({ dados: carteiras, procedencia: PROCEDENCIA });

    montar();

    // A frota: o filtro por cliente é pedido à listagem, e cada máquina diz a relação.
    const frota = (await screen.findByText('7250R')).closest('[data-bloco="frota"]') as HTMLElement;
    expect(equipamentos.listarEquipamentos.mock.calls[0][1]).toMatchObject({ clienteChave: CHAVE });
    expect(within(frota).getByText('dono atual')).toBeInTheDocument();
    expect(within(frota).getByText(/por nota de venda no Protheus de 10\/03\/2025/)).toBeInTheDocument();
    expect(within(frota).getByText('comprou no ART')).toBeInTheDocument();
    expect(within(frota).getByText(/hoje é de Outro dono/)).toBeInTheDocument();

    // O faturamento: o total dos doze meses, a quebra, a filial e a data da carga no subtítulo.
    const bloco = (await screen.findByText('Últimos 12 meses')).closest('[data-bloco="faturamento"]') as HTMLElement;
    expect(within(bloco).getByText(/até a carga de/)).toBeInTheDocument();
    expect(within(bloco).getByText(/591\.854/)).toBeInTheDocument();
    expect(within(bloco).getByText(/281\.500/)).toBeInTheDocument();
    expect(within(bloco).getByText('Votuporanga')).toBeInTheDocument();
    expect(within(bloco).getByText(/não se somam a estes valores/)).toBeInTheDocument();
    expect(bloco.querySelector('[data-grafico="linha"]')).toHaveAttribute('data-parcial', 'true');

    // As carteiras: o CEN, a filial, a classe do cadastro e a cadência; o último contato é traço com motivo.
    const cart = (await screen.findByText('Máquinas Votuporanga')).closest('[data-bloco="carteiras"]') as HTMLElement;
    expect(within(cart).getByText(/CEN: CEN de amostra/)).toBeInTheDocument();
    expect(within(cart).getByText(/cadência de 180 dias para a classe A/)).toBeInTheDocument();
    expect(within(cart).getByRole('button', { name: 'Por que o último contato não aparece' })).toBeInTheDocument();

    // As lacunas que ficam dizem o motivo de hoje — e as frases falsas saíram.
    expect(screen.getByText('Títulos em aberto')).toBeInTheDocument();
    expect(screen.getByText('Ordens de serviço')).toBeInTheDocument();
    expect(screen.queryByText(/11\/04\/2025/)).not.toBeInTheDocument();
    expect(screen.queryByText(/24\/05\/2024/)).not.toBeInTheDocument();
    expect(screen.queryByText(/Falta rota/)).not.toBeInTheDocument();
  });

  it('sem máquina ao alcance, a frota diz por quê; sem nota, o faturamento mostra o traço com o motivo', async () => {
    clientes.obterCliente.mockResolvedValue({ dados: null, procedencia: PROCEDENCIA });
    equipamentos.listarEquipamentos.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.listarProcessos.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.listarTarefas.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.listarInteracoes.mockResolvedValue({ dados: vazia, procedencia: PROCEDENCIA });
    relacionamento.obterFaturamentoDoCliente.mockResolvedValue({
      dados: {
        ...faturamento,
        ultimaCompraEm: null,
        porFilial: [],
        dozeMeses: { ...faturamento.dozeMeses!, valorLiquido: 0, maquina: 0, peca: 0, servico: 0, notas: 0 },
        janelaCarregada: { ...faturamento.janelaCarregada!, valorLiquido: 0 },
        metricasSemDado: [{ metrica: 'faturamentoDoCliente', motivo: 'Nenhuma nota de venda deste cliente nas filiais ao seu alcance.' }],
      },
      procedencia: PROCEDENCIA,
    });
    relacionamento.listarCarteirasDoCliente.mockResolvedValue({
      dados: { classe: 'D', classeApuradaEm: null, carteiras: [], metricasSemDado: [{ metrica: 'carteiras', motivo: 'Nenhuma carteira ao seu alcance.' }] },
      procedencia: PROCEDENCIA,
    });

    montar();

    expect(await screen.findByText(/Nenhuma máquina deste cliente ao seu alcance/)).toBeInTheDocument();
    const bloco = (await screen.findByText('Nota mais recente')).closest('[data-bloco="faturamento"]') as HTMLElement;
    expect(within(bloco).getByRole('button', { name: 'Por que a nota mais recente não aparece' })).toBeInTheDocument();
    expect(within(bloco).getByText(/Nenhuma nota de venda deste cliente/)).toBeInTheDocument();
  });
});
