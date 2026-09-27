/**
 * O formulário dos parâmetros gerais (issues 71 e 166). O que se prende:
 * - "Calcular pelos tercis" PREENCHE as duas bandas (e a justificativa, se vazia) e não grava nada;
 * - sem corte possível, o motivo aparece e os campos ficam como estavam;
 * - as bandas e o mínimo de linhas da vigência de hoje vêm preenchidos e vão no envio — antes o formulário não
 *   mandava o mínimo, e toda vigência nova registrada pela tela o apagava.
 */

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../../dados/api/contexto';
import type { ParametrosGeraisDetalhe, SugestaoDasBandasDePorte } from '../../../tipos/potencial';
import { FormularioDosGerais } from './FormularioDosGerais';

const api = vi.hoisted(() => ({
  informarParametrosGerais: vi.fn(),
  sugerirBandasDePorte: vi.fn(),
}));

vi.mock('../../../dados/api/potencial', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'organizacao.ProducaoAgricolaNoMunicipio', lidoEmUtc: '2026-09-27T12:00:00Z', dadoMaisRecenteEm: null };

const VIGENTE: ParametrosGeraisDetalhe = {
  mesesDaJanela: 12,
  pesoDosContratosNoCredito: 0.7,
  pesoDoValorNoCredito: 0.3,
  limiteDeRetracao: 1,
  limiteDeAquecimento: 1.2,
  limiteDeSuperaquecimento: 1.4,
  nomeDaFaixaIntermediaria: null,
  limiteDaPercepcao: 5,
  pesoDoIndicadorDePreco: 0.4,
  pesoDoIndicadorDeCredito: 0.5,
  pesoDoIndicadorComercial: 1,
  fatorMinimo: 0.4,
  fatorMaximo: 1.5,
  mesesDeCarenciaDoSicor: null,
  minimoDeLinhasNoCredito: null,
  porteMedioAPartirDe: null,
  porteGrandeAPartirDe: null,
  vigencia: {
    vigenteDesde: '2026-09-23', justificativa: 'semente', informadoPor: null, informadoEm: '2026-09-23T00:00:00',
    revogadoEm: null, revogadoPor: null, motivoDaRevogacao: null,
  },
};

const sugestao = (parcial: Partial<SugestaoDasBandasDePorte>) => ({
  dados: {
    porteMedioAPartirDe: 5.3,
    porteGrandeAPartirDe: 8.7,
    municipiosDaAdr: 203,
    municipiosNaConta: 198,
    anoDaAreaPlantada: 2024,
    justificativa: 'Tercis da demanda anual de máquinas dos 198 municípios da ADR com demanda.',
    motivo: null,
    ...parcial,
  },
  procedencia: PROCEDENCIA,
});

function desenhar(vigente: ParametrosGeraisDetalhe = VIGENTE) {
  const aoGravar = vi.fn();
  render(
    <ProvedorDeContextoDeAcesso>
      <FormularioDosGerais vigente={vigente} hoje="2026-09-27" aoGravar={aoGravar} aoCancelar={() => {}} />
    </ProvedorDeContextoDeAcesso>,
  );
  return { aoGravar };
}

const campo = (rotulo: string | RegExp) => screen.getByLabelText(rotulo) as HTMLInputElement;

describe('FormularioDosGerais', () => {
  afterEach(() => {
    Object.values(api).forEach((f) => f.mockReset());
    guardado.clear();
  });

  it('"Calcular pelos tercis" preenche as bandas e a justificativa vazia, e não grava nada', async () => {
    api.sugerirBandasDePorte.mockResolvedValue(sugestao({}));
    desenhar();

    fireEvent.click(screen.getByRole('button', { name: 'Calcular pelos tercis' }));

    await waitFor(() => expect(campo('Porte médio a partir de (máq/ano)').value).toBe('5,3'));
    expect(campo('Porte grande a partir de (máq/ano)').value).toBe('8,7');
    expect(campo(/Justificativa/).value).toContain('Tercis da demanda anual');
    expect(screen.getByRole('status')).toHaveTextContent('198 de 203 municípios da ADR');
    expect(screen.getByRole('status')).toHaveTextContent('nada foi gravado ainda');
    expect(api.informarParametrosGerais).not.toHaveBeenCalled();
  });

  it('não troca a justificativa que o administrador já escreveu', async () => {
    api.sugerirBandasDePorte.mockResolvedValue(sugestao({}));
    desenhar();

    fireEvent.change(campo(/Justificativa/), { target: { value: 'decisão da diretoria' } });
    fireEvent.click(screen.getByRole('button', { name: 'Calcular pelos tercis' }));

    await waitFor(() => expect(campo('Porte médio a partir de (máq/ano)').value).toBe('5,3'));
    expect(campo(/Justificativa/).value).toBe('decisão da diretoria');
  });

  it('sem corte possível, diz por quê e deixa os campos como estavam', async () => {
    api.sugerirBandasDePorte.mockResolvedValue(
      sugestao({ porteMedioAPartirDe: null, porteGrandeAPartirDe: null, justificativa: null, motivo: 'Só 2 município(s) da ADR têm demanda anual.' }),
    );
    desenhar();

    fireEvent.click(screen.getByRole('button', { name: 'Calcular pelos tercis' }));

    await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('Só 2 município(s)'));
    expect(campo('Porte médio a partir de (máq/ano)').value).toBe('');
    expect(campo(/Justificativa/).value).toBe('');
  });

  it('as bandas e o mínimo de linhas da vigência de hoje vêm preenchidos e vão no envio', async () => {
    api.informarParametrosGerais.mockResolvedValue({ ...VIGENTE, vigencia: { ...VIGENTE.vigencia, vigenteDesde: '2026-09-27' } });
    const { aoGravar } = desenhar({ ...VIGENTE, minimoDeLinhasNoCredito: 20, porteMedioAPartirDe: 12.5, porteGrandeAPartirDe: 40 });

    expect(campo('Mínimo de linhas do SICOR').value).toBe('20');
    expect(campo('Porte médio a partir de (máq/ano)').value).toBe('12,5');

    fireEvent.change(campo(/Justificativa/), { target: { value: 'só a percepção mudou' } });
    fireEvent.click(screen.getByRole('button', { name: 'Registrar vigência' }));

    await waitFor(() => expect(aoGravar).toHaveBeenCalled());
    expect(api.informarParametrosGerais).toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ minimoDeLinhasNoCredito: '20', porteMedioAPartirDe: '12,5', porteGrandeAPartirDe: '40' }),
    );
  });
});
