/**
 * A tela de Integrações (issue 136). O que se prende: o Administrador testa, configura — com a senha numa chamada
 * própria, sem nunca vê-la preenchida — e pede "rodar agora"; a Gerência vê tudo e não tem botão de mexer.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { ConexaoNaTela, PainelDeIntegracoes, RotinaNaTela } from '../../dados/api/integracoes';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { ConfigSecaoIntegracoes } from './ConfigSecaoIntegracoes';

const api = vi.hoisted(() => ({
  obterPainelDeIntegracoes: vi.fn(),
  listarVerificacoes: vi.fn(),
  listarExecucoes: vi.fn(),
  configurarConexao: vi.fn(),
  definirSegredo: vi.fn(),
  removerSegredo: vi.fn(),
  testarConexao: vi.fn(),
  criarApiMonitorada: vi.fn(),
  desativarConexao: vi.fn(),
  reativarConexao: vi.fn(),
  reagendarRotina: vi.fn(),
  pedirExecucao: vi.fn(),
}));

vi.mock('../../dados/api/integracoes', () => api);
const sincronizacoes = vi.hoisted(() => ({ listarSincronizacoes: vi.fn() }));
vi.mock('../../dados/api/sincronizacoes', () => sincronizacoes);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'integracao.Conexao', lidoEmUtc: '2026-09-22T12:00:00Z', dadoMaisRecenteEm: null };

const base: Omit<ConexaoNaTela, 'codigo' | 'nome' | 'tipo'> = {
  descricao: null, ehDoSistema: true, estaAtiva: true, endereco: null, porta: null, banco: null, objeto: null, usuario: null,
  nomeDoCabecalho: null, statusEsperado: 200, minutosEntreVerificacoes: null, enderecoEditavel: true, aceitaSegredo: true,
  origemDaCredencial: 'Nenhuma', temSegredoNaTela: false, segredoAlteradoEm: null, segredoAlteradoPor: null,
  ultimaVerificacaoEm: null, ultimaVerificacaoOk: null, ultimaVerificacaoResumo: null, rotinas: [],
};

const PROTHEUS: ConexaoNaTela = { ...base, codigo: 'PROTHEUS', nome: 'Protheus — API REST', tipo: 'ApiRest', rotinas: ['Faturamento do Protheus'] };
const IBGE: ConexaoNaTela = {
  ...base, codigo: 'IBGE_SIDRA', nome: 'IBGE — SIDRA', tipo: 'FontePublica', enderecoEditavel: false, aceitaSegredo: false,
  origemDaCredencial: 'NaoSeAplica', endereco: 'https://servicodados.ibge.gov.br/api/v3/agregados/5457/metadados',
};
const FATURAMENTO: RotinaNaTela = {
  codigo: 'FATURAMENTO_PROTHEUS', nome: 'Faturamento do Protheus', descricao: 'As notas de saída.', cargas: ['--somente-faturamento'],
  cadencia: 'Diaria', mes: null, dia: null, hora: '05:00', intervaloMinutos: null, agenda: 'todo dia às 05:00', estaLigada: false,
  proximaExecucaoEm: null, naFila: false, execucaoPedidaEm: null, execucaoPedidaPor: null, ultimaExecucaoIniciadaEm: null,
  ultimaExecucaoTerminadaEm: null, ultimoResultado: null, ultimaMensagem: null, conexoes: ['PROTHEUS'],
  pendencia: 'Falta a credencial de Protheus — API REST: configure-a acima. Sem ela, a rotina não roda.',
  anoInicialDoHistorico: null, aceitaAnoInicialDoHistorico: false,
};

const PRECOS: RotinaNaTela = {
  ...FATURAMENTO, codigo: 'PRECOS_MENSAIS', nome: 'Preços, custos e crédito', cargas: ['--somente-precos'], cadencia: 'Mensal', dia: 20,
  hora: '04:00', agenda: 'todo mês no dia 20, às 04:00', estaLigada: true, conexoes: [], pendencia: null,
};

const painel = (podeAdministrar: boolean): { dados: PainelDeIntegracoes; procedencia: typeof PROCEDENCIA } => ({
  dados: { conexoes: [PROTHEUS, IBGE], rotinas: [FATURAMENTO, PRECOS], podeAdministrar },
  procedencia: PROCEDENCIA,
});

function montar() {
  render(
    <ProvedorDeContextoDeAcesso>
      <ConfigSecaoIntegracoes />
    </ProvedorDeContextoDeAcesso>,
  );
}

describe('ConfigSecaoIntegracoes', () => {
  beforeEach(() => {
    sincronizacoes.listarSincronizacoes.mockResolvedValue([]);
  });

  afterEach(() => {
    Object.values(api).forEach((f) => f.mockClear());
    guardado.clear();
  });

  it('o administrador testa, configura com a senha à parte e pede rodar agora', async () => {
    api.obterPainelDeIntegracoes.mockResolvedValue(painel(true));
    api.testarConexao.mockResolvedValue({ ok: true, resumo: 'HTTP 200: a fonte está no ar.', latenciaMs: 80, testadoEm: '2026-09-22T12:01:00Z', conexao: IBGE });
    api.configurarConexao.mockResolvedValue({ ...PROTHEUS, endereco: 'http://erp.exemplo.invalid/rest', usuario: 'leitura' });
    api.definirSegredo.mockResolvedValue({ ...PROTHEUS, origemDaCredencial: 'Tela', temSegredoNaTela: true });
    api.pedirExecucao.mockResolvedValue({ ...PRECOS, naFila: true });
    montar();

    const ibge = (await screen.findByText('IBGE — SIDRA')).closest('.integracao-item') as HTMLElement;
    expect(within(ibge).queryByRole('button', { name: 'Configurar' })).not.toBeInTheDocument();
    fireEvent.click(within(ibge).getByRole('button', { name: 'Testar' }));
    expect(await within(ibge).findByText(/Teste passou/)).toBeInTheDocument();
    expect(api.testarConexao).toHaveBeenCalledWith(expect.anything(), 'IBGE_SIDRA');

    const protheus = screen.getByText('Protheus — API REST').closest('.integracao-item') as HTMLElement;
    expect(within(protheus).getByText('não configurada')).toBeInTheDocument();
    fireEvent.click(within(protheus).getByRole('button', { name: 'Configurar' }));
    const senha = within(protheus).getByLabelText('Senha') as HTMLInputElement;
    expect(senha).toHaveAttribute('type', 'password');
    expect(senha.value).toBe('');
    fireEvent.change(within(protheus).getByLabelText('Endereço (URL)'), { target: { value: 'http://erp.exemplo.invalid/rest' } });
    fireEvent.change(within(protheus).getByLabelText('Usuário'), { target: { value: 'leitura' } });
    fireEvent.change(senha, { target: { value: 'segredo-de-teste' } });
    fireEvent.click(within(protheus).getByRole('button', { name: 'Salvar' }));

    await waitFor(() => expect(api.definirSegredo).toHaveBeenCalledWith(expect.anything(), 'PROTHEUS', 'segredo-de-teste'));
    expect(api.configurarConexao.mock.calls[0][2]).toMatchObject({ endereco: 'http://erp.exemplo.invalid/rest', usuario: 'leitura' });
    expect(JSON.stringify(api.configurarConexao.mock.calls[0][2])).not.toContain('segredo-de-teste');

    // SEM A CREDENCIAL QUE A ROTINA EXIGE, "Rodar agora" nem se oferece: ela só falharia.
    const faturamento = screen.getByText('Faturamento do Protheus', { selector: 'strong' }).closest('.integracao-item') as HTMLElement;
    expect(within(faturamento).getByText(/Falta a credencial de Protheus/)).toBeInTheDocument();
    expect(within(faturamento).getByRole('button', { name: 'Rodar agora' })).toBeDisabled();

    const precos = screen.getByText('Preços, custos e crédito', { selector: 'strong' }).closest('.integracao-item') as HTMLElement;
    fireEvent.click(within(precos).getByRole('button', { name: 'Rodar agora' }));
    await waitFor(() => expect(api.pedirExecucao).toHaveBeenCalledWith(expect.anything(), 'PRECOS_MENSAIS'));
  });

  it('a gerência vê as conexões e as rotinas e não tem botão de mexer', async () => {
    api.obterPainelDeIntegracoes.mockResolvedValue(painel(false));
    montar();

    expect(await screen.findByText('Protheus — API REST')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Testar' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Configurar' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Rodar agora' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Conectar nova API/ })).not.toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: 'Histórico' }).length).toBeGreaterThan(0);
    expect(screen.getByText('Configurar e testar é do Administrador.', { exact: false })).toBeInTheDocument();
  });
});
