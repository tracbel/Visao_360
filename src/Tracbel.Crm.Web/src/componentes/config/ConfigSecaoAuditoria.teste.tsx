/**
 * A tela de Auditoria (issue 135). O que se prende: o evento aparece como alguém contaria ("Concedeu o perfil …"),
 * abre com o antes e o depois de cada campo, e "ver o histórico deste registro" pede só aquele registro à API.
 */

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import type { EventoDaAuditoria } from '../../dados/api/auditoria';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { ConfigSecaoAuditoria } from './ConfigSecaoAuditoria';

const api = vi.hoisted(() => ({
  consultarTrilha: vi.fn(),
  listarEntidadesAuditadas: vi.fn(),
}));

vi.mock('../../dados/api/auditoria', () => api);

// Armazenamento em memória: o `localStorage` do Node 25 não guarda nada sem `--localstorage-file`.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

const PROCEDENCIA = { sistema: 'CRM Tracbel', objeto: 'auditoria.AlteracaoDeCampo', lidoEmUtc: '2026-09-22T12:00:00Z', dadoMaisRecenteEm: null };

const CONCESSAO: EventoDaAuditoria = {
  quando: '2026-09-22T12:30:00',
  entidade: 'UsuarioPerfil',
  entidadeRotulo: 'Concessão de perfil',
  registroId: 42,
  registro: 'Gerência para Maria Exemplo',
  operacao: 'Inclusao',
  origem: 'Usuario',
  sistema: null,
  autor: 'Administrador Exemplo',
  autorLogin: 'administrador@exemplo.invalid',
  filialCodigo: '010101',
  filialNome: 'Tracbel Agro — Ribeirão Preto',
  correlacao: null,
  campos: [
    { campo: 'PerfilId', rotulo: 'Perfil', antes: null, depois: 'Gerência' },
    { campo: 'Justificativa', rotulo: 'Justificativa', antes: null, depois: 'gerente desde setembro' },
    { campo: 'ExpiraEm', rotulo: 'Vale até', antes: null, depois: '2026-12-31T02:59:59.999' },
  ],
};

const pagina = (itens: EventoDaAuditoria[]) => ({
  dados: { itens, pagina: 1, tamanho: 25, total: itens.length, totalDePaginas: 1, temProxima: false },
  procedencia: PROCEDENCIA,
});

describe('ConfigSecaoAuditoria', () => {
  afterEach(() => {
    Object.values(api).forEach((f) => f.mockClear());
    guardado.clear();
  });

  it('mostra o evento como frase, abre o antes e o depois e filtra pelo registro', async () => {
    api.consultarTrilha.mockResolvedValue(pagina([CONCESSAO]));
    api.listarEntidadesAuditadas.mockResolvedValue({ dados: [{ codigo: 'UsuarioPerfil', rotulo: 'Concessão de perfil' }], procedencia: PROCEDENCIA });

    render(
      <ProvedorDeContextoDeAcesso>
        <ConfigSecaoAuditoria filialAtual={{ codigo: '010101', nome: 'Tracbel Agro — Ribeirão Preto', ehCasa: true }} podeVerTodasAsFiliais />
      </ProvedorDeContextoDeAcesso>,
    );

    expect(screen.getByText(/Todas as filiais/)).toBeInTheDocument();
    const evento = await screen.findByRole('button', { name: /Concedeu o perfil Gerência para Maria Exemplo/ });
    expect(api.consultarTrilha.mock.calls[0][1]).toMatchObject({ entidade: '', registro: null, pagina: 1 });

    fireEvent.click(evento);
    expect(await screen.findByText('gerente desde setembro')).toBeInTheDocument();
    expect(screen.getByRole('rowheader', { name: 'Vale até' })).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Ver o histórico deste registro' }));
    await waitFor(() => expect(api.consultarTrilha).toHaveBeenLastCalledWith(
      expect.anything(),
      expect.objectContaining({ entidade: 'UsuarioPerfil', registro: 42, pagina: 1 }),
      expect.anything(),
    ));
    expect(await screen.findByText(/Só o histórico de/)).toBeInTheDocument();
  });

  it('diz quando não há nada no período', async () => {
    api.consultarTrilha.mockResolvedValue(pagina([]));
    api.listarEntidadesAuditadas.mockResolvedValue({ dados: [], procedencia: PROCEDENCIA });

    render(
      <ProvedorDeContextoDeAcesso>
        <ConfigSecaoAuditoria filialAtual={{ codigo: 'TODAS', nome: 'Todas as filiais', ehCasa: false }} podeVerTodasAsFiliais />
      </ProvedorDeContextoDeAcesso>,
    );

    expect(await screen.findByText('Nenhuma alteração registrada com este filtro.')).toBeInTheDocument();
    expect(screen.getByText('todas as filiais')).toBeInTheDocument();
  });
});
