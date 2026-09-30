/**
 * O PIPELINE NA MAQUETE (30/09/2026) — o que a tela afirma, pelo caminho de produção: um `fetch` de mentira que responde
 * com as MESMAS amostras do harness `#/dev/visao360-visual?rota=/pipeline`. O desenho é da conferência visual
 * (`testes-visuais/telas-no-padrao.spec.ts`); aqui ficam as regras:
 *
 * - a cor de cada fase é a da maquete PELO NOME (decisão do Ricardo), e a fase fora da imagem fica cinza;
 * - o fundo pinta só a fase com valor declarado, e o número verde é o da maior fase;
 * - a seta de cada fase abre a lista daquela fase, com o total dela (a dívida P-7 paga na API);
 * - o aviso laranja é o motivo medido pela API, com os mesmos números dos cartões;
 * - o Exportar leva a lista com os filtros da tela, e o menu de cada linha leva ao processo e ao cliente.
 */

import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { Pipeline } from './Pipeline';

vi.setConfig({ testTimeout: 20_000 });

const baixados: { nome: string; cabecalho: string[]; linhas: unknown[][] }[] = [];
vi.mock('../dados/exportarCsv', () => ({
  baixarCsv: (nome: string, cabecalho: string[], linhas: unknown[][]) => void baixados.push({ nome, cabecalho, linhas }),
  carimboDeData: () => '20260930',
}));

const guardado = new Map<string, string>();
let pedidos: string[] = [];

function instalarApi(estado: EstadoDaVisao360) {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string, init?: RequestInit) => {
      const [caminho, consulta = ''] = entrada.replace(/^.*\/api/, '').split('?');
      pedidos.push(entrada.replace(/^.*\/api/, ''));
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
}

async function montar(estado: EstadoDaVisao360 = 'completo') {
  instalarApi(estado);
  const tela = render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <Pipeline />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
  await waitFor(() => expect(tela.container.querySelector('[data-kpi="Processos abertos"]')).toHaveTextContent(/\d/));
  await waitFor(() => expect(tela.container.querySelectorAll('.pip-fase').length).toBeGreaterThan(0));
  await screen.findAllByText(/Trator fictício 6110J/);
  return tela;
}

/** A busca espera 350ms depois da última tecla, e a suíte inteira roda com a máquina carregada: folga de 5 segundos. */
const ESPERA = { timeout: 5_000 };

const fase = (container: HTMLElement, nome: string) =>
  [...container.querySelectorAll<HTMLElement>('.pip-fase')].find((f) => f.querySelector('.pip-fase-nome')?.textContent === nome)!;

beforeEach(() => {
  guardado.clear();
  pedidos = [];
  baixados.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

describe('Pipeline na maquete (30/09/2026)', () => {
  it('o título é "Pipeline", e o aviso laranja diz o motivo da API com os números dos cartões', async () => {
    const { container } = await montar();

    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent(/^Pipeline/);

    const valor = (kpi: string) => container.querySelector(`[data-kpi="${kpi}"] .mv-kpi-valor strong`)!.textContent!;
    const aviso = container.querySelector('.cad-semdado')!;
    expect(aviso).toHaveTextContent('O que estes números não dizem');
    expect(aviso).toHaveTextContent(
      `Só ${valor('Declaram valor')} de ${valor('Processos abertos')} processos abertos declaram valor`,
    );
    expect(aviso).toHaveTextContent(/\(\d+,\d%\)/);
  });

  it('a cor de cada fase é a da maquete pelo nome, e a fase fora da imagem fica cinza', async () => {
    const { container } = await montar();

    expect(fase(container, 'Apresentação')).toHaveAttribute('data-cor', 'verde');
    expect(fase(container, 'Monitoramento')).toHaveAttribute('data-cor', 'azul');
    expect(fase(container, 'Negociação')).toHaveAttribute('data-cor', 'laranja');
    expect(fase(container, 'Análise')).toHaveAttribute('data-cor', 'roxo');
    expect(fase(container, 'Não iniciado')).toHaveAttribute('data-cor', 'cinza');
    expect(fase(container, 'Pedido de Venda')).toHaveAttribute('data-cor', 'verde');

    // OUTRO FLUXO: as fases dele não estão na imagem, e ficam cinza.
    fireEvent.change(container.querySelector('[data-bloco="fluxo"] select')!, { target: { value: 'PROSPECCAO' } });
    await waitFor(() => expect(fase(container, 'Prospecção')).toBeDefined());
    expect(fase(container, 'Prospecção')).toHaveAttribute('data-cor', 'cinza');
    expect(fase(container, 'Qualificação')).toHaveAttribute('data-cor', 'cinza');
  });

  it('o fundo pinta só a fase com valor, o valor vem com quantos o sustentam, e o verde do número é da maior fase', async () => {
    const { container } = await montar();

    const apresentacao = fase(container, 'Apresentação');
    expect(apresentacao).toHaveAttribute('data-com-valor', 'true');
    expect(apresentacao).toHaveTextContent(/R\$\s?[\d.]+somados de [\d.]+ de [\d.]+ processos — o resto não declara valor\./);

    const monitoramento = fase(container, 'Monitoramento');
    expect(monitoramento).toHaveAttribute('data-com-valor', 'false');
    expect(monitoramento).toHaveTextContent('— sem valor declarado');

    const maiores = [...container.querySelectorAll('.pip-fase-numero[data-maior="true"]')];
    expect(maiores).toHaveLength(1);
    expect(maiores[0].closest('.pip-fase')).toBe(apresentacao);
  });

  it('a seta da fase abre a lista daquela fase, e a mesma seta ou o ✕ tiram o filtro', async () => {
    const { container } = await montar();

    const seta = within(fase(container, 'Negociação')).getByRole('button', { name: /Ver na lista os [\d.]+ processos da fase Negociação/ });
    fireEvent.click(seta);

    await waitFor(() =>
      expect(pedidos.some((p) => p.startsWith('/v1/processos?') && p.includes('faseCodigo=NEGOCIACAO') && p.includes('tipoProcessoCodigo=VENDA'))).toBe(true),
    );
    const secao = container.querySelector('[data-bloco="secao-processos"]') as HTMLElement;
    await waitFor(() => expect(secao).toHaveTextContent('com os filtros da barra e a fase Negociação.'));
    expect(seta).toHaveAttribute('aria-pressed', 'true');
    expect(fase(container, 'Negociação')).toHaveAttribute('data-escolhida', 'true');

    // O TOTAL DA LISTA É O DA FASE (a P-7 paga): o mesmo número do cartão dela.
    const naFase = fase(container, 'Negociação').querySelector('.pip-fase-numero')!.textContent!;
    expect(secao).toHaveTextContent(`${naFase} no total`);
    for (const linha of within(secao).getAllByRole('row').slice(1)) expect(linha).toHaveTextContent('Negociação');

    fireEvent.click(within(secao).getByRole('button', { name: 'Tirar o filtro da fase Negociação' }));
    await waitFor(() => expect(secao).not.toHaveTextContent('e a fase Negociação'));
    expect(fase(container, 'Negociação')).toHaveAttribute('data-escolhida', 'false');
  });

  it('trocar o fluxo do quadro tira o filtro da fase da lista', async () => {
    const { container } = await montar();

    fireEvent.click(within(fase(container, 'Montagem')).getByRole('button', { name: /fase Montagem/ }));
    const secao = container.querySelector('[data-bloco="secao-processos"]') as HTMLElement;
    await waitFor(() => expect(secao).toHaveTextContent('e a fase Montagem'));

    fireEvent.change(container.querySelector('[data-bloco="fluxo"] select')!, { target: { value: 'POS_VENDA' } });
    await waitFor(() => expect(secao).not.toHaveTextContent('e a fase Montagem'));
  });

  it('os dias parados passam de 90 na pílula vermelha, com o alerta dito ao leitor de tela', async () => {
    await montar();

    const antigo = screen.getAllByText(/1\.302 dias/)[0].closest('.pip-parado')!;
    expect(antigo).toHaveAttribute('data-alerta', 'true');
    expect(antigo).toHaveTextContent('parado há mais de 90 dias');

    const recente = screen.getAllByText('37 dias')[0];
    expect(recente).not.toHaveAttribute('data-alerta');
  });

  it('o Exportar leva a lista com a busca da tela', async () => {
    await montar();

    fireEvent.change(screen.getByPlaceholderText('Título ou número…'), { target: { value: '48213' } });
    await waitFor(() => expect(screen.queryAllByText(/Trator fictício 6110J/)).toHaveLength(0), ESPERA);

    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Exportar' }));
    });
    await waitFor(() => expect(baixados).toHaveLength(1), ESPERA);

    const [{ nome, cabecalho, linhas }] = baixados;
    expect(nome).toMatch(/^pipeline-/);
    expect(cabecalho.slice(0, 5)).toEqual(['Número', 'Processo', 'Cliente', 'Fluxo', 'Fase']);
    expect(linhas).toHaveLength(1);
    expect(linhas[0][0]).toBe(48_213);
    expect(pedidos.some((p) => p.includes('tamanho=200') && p.includes('termo=48213'))).toBe(true);
  });

  it('o menu de cada linha leva ao processo e à ficha do cliente', async () => {
    await montar();

    fireEvent.click(screen.getByRole('button', { name: 'Ações do processo 48210' }));
    const menu = screen.getByRole('menu');
    expect(within(menu).getByRole('menuitem', { name: 'Abrir o processo' })).toHaveAttribute(
      'href',
      '/oportunidades/00000000-0000-4000-8000-000000000900',
    );
    expect(within(menu).getByRole('menuitem', { name: 'Abrir a ficha do cliente' })).toHaveAttribute(
      'href',
      '/clientes/00000000-0000-4000-8000-000000000700',
    );

    fireEvent.keyDown(document, { key: 'Escape' });
    expect(screen.queryByRole('menu')).toBeNull();
  });

  it('nada da tela antiga sumiu: o que a tela ainda não faz, agora com a tendência dos cartões', async () => {
    const { container } = await montar();

    const bloco = [...container.querySelectorAll('details')].find((d) => d.textContent?.includes('O que esta tela ainda não faz'))!;
    expect(bloco).toHaveTextContent('Mudar de fase, fechar e marcar como perdida ainda não estão aqui.');
    expect(bloco).toHaveTextContent('A tendência de cada cartão');
  });
});
