/**
 * A PERFORMANCE DE CEN — a meta de venda de cada consultor (#138, 27/09/2026) e a tela na maquete do Ricardo
 * (01/10/2026).
 *
 * A meta: a cota da API Gestão de Negócios, em máquinas, contra as vendas do ART em que o consultor é o vendedor (D-M2).
 * Este teste prende:
 *
 * - a tabela meta × realizado por consultor, com o atingimento refeito na tela e o consultor sem conta nomeado;
 * - o cadastro não lido diz isso, e não "meta zero";
 * - o 403 é falta de permissão, sem o botão de tentar de novo;
 * - o "Atingimento de meta" saiu do que a tela não sustenta.
 *
 * A maquete: os quatro filtros mudam a tela na hora (a classe e a carteira vão à API; o período, ao funil, às perdas e à
 * meta); os cartões dizem o recorte contra a filial; o principal alerta é a classe com mais vínculos fora da cadência e
 * nunca contatados; o CEN escolhido aparece em destaque entre os outros; os quatro cartões do fim abrem um por vez.
 *
 * O CAMINHO É O DE PRODUÇÃO: a rota é lida por um `fetch` de mentira que responde com as amostras do harness da Visão
 * 360.
 */

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { metasDeVenda, respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { PerformanceCen } from './PerformanceCen';

vi.setConfig({ testTimeout: 20_000 });

const guardado = new Map<string, string>();
let pedidos: string[] = [];

/** A filial do contexto padrão; a amostra responde a ela com a primeira filial fictícia. */
const FILIAL = '010101';

/** Como a rota das metas responde: a amostra, 403, ou uma resposta dada pelo teste. */
type RespostaDasMetas = 'amostra' | 'recusada' | { dados: unknown };

function instalarApi(estado: EstadoDaVisao360, metas: RespostaDasMetas = 'amostra') {
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
      if (caminho === '/v1/relatorios/metas' && metas === 'recusada') {
        return new Response(JSON.stringify({ title: 'Sem a permissão Meta.Ler' }), { status: 403 });
      }
      if (caminho === '/v1/relatorios/metas' && typeof metas === 'object') {
        return new Response(JSON.stringify({ dados: metas.dados, procedencia: null }), { status: 200 });
      }
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
}

/** Monta a tela e devolve o corpo do cartão da meta quando a leitura dela voltou. */
async function montar(estado: EstadoDaVisao360, metas: RespostaDasMetas = 'amostra') {
  instalarApi(estado, metas);
  const tela = render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <PerformanceCen />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
  const bloco = await waitFor(() => {
    const b = tela.container.querySelector<HTMLElement>('[data-recolhivel-corpo="meta"]');
    expect(b).not.toBeNull();
    expect(b!.textContent).not.toContain('Carregando a meta de venda');
    return b!;
  });
  return { ...tela, bloco, botaoDaMeta: tela.container.querySelector<HTMLElement>('[data-recolhivel="meta"]')! };
}

/** Espera a cobertura voltar: os cartões com número e as barras por classe desenhadas. */
async function montarComCobertura() {
  const tela = await montar('completo');
  await waitFor(() => expect(tela.container.querySelector('[data-kpi="CENs com carteira"] .mv-kpi-valor strong')).toHaveTextContent(/\d/));
  await waitFor(() => expect(tela.container.querySelectorAll('.pcen-barras-linha').length).toBeGreaterThan(0));
  return tela;
}

const valorDo = (container: HTMLElement, kpi: string) =>
  container.querySelector(`[data-kpi="${kpi}"] .mv-kpi-valor strong`)?.textContent ?? '';

const contextoDo = (container: HTMLElement, kpi: string) => container.querySelector(`[data-kpi="${kpi}"] .mv-kpi-contexto`)?.textContent ?? '';

const filtro = (container: HTMLElement, bloco: string) => container.querySelector<HTMLSelectElement>(`[data-bloco="${bloco}"] select`)!;

beforeEach(() => {
  guardado.clear();
  pedidos = [];
});
afterEach(() => vi.unstubAllGlobals());

describe('Performance de CEN — a meta de venda por consultor (#138)', () => {
  it('desenha meta × realizado de cada consultor, com o atingimento refeito na tela', async () => {
    const { bloco, botaoDaMeta } = await montar('completo');
    const amostra = metasDeVenda('completo', FILIAL);
    const [um, dois] = amostra.porConsultor;

    expect(botaoDaMeta).toHaveTextContent('Meta de venda × realizado, por consultor');
    expect(botaoDaMeta).toHaveTextContent('2 consultores');
    expect(bloco.querySelector('.pcen-recolhivel-resumo')).toHaveTextContent(
      `${amostra.totais.realizadoMaquinas.toLocaleString('pt-BR')} de ${amostra.totais.metaMaquinas.toLocaleString('pt-BR')} máquinas · nov/2025 a ago/2026 · 2 consultores`,
    );

    const linhas = within(bloco).getAllByRole('row', { hidden: true }).slice(1);
    expect(linhas).toHaveLength(2);
    expect(linhas[0]).toHaveTextContent(um!.consultor);
    expect(linhas[0]).toHaveTextContent(`${Math.round((um!.realizado / um!.meta) * 100)}%`);
    expect(linhas[0]).not.toHaveTextContent('sem conta no CRM');
    // A conta que a GN nomeia e o CRM não tem aparece nomeada — o número dela conta do mesmo jeito.
    expect(linhas[1]).toHaveTextContent(dois!.consultor);
    expect(linhas[1]).toHaveTextContent('sem conta no CRM');

    expect(bloco).toHaveTextContent('a venda sem vendedor conta no total da filial e em ninguém');
    expect(bloco).toHaveTextContent('o ano fiscal até o último mês fechado');
  });

  it('o "Atingimento de meta" saiu do que a tela não sustenta', async () => {
    const { container } = await montar('completo');

    const botao = container.querySelector('[data-recolhivel="lacunas"]')!;
    const corpo = container.querySelector('[data-recolhivel-corpo="lacunas"]')!;
    expect(botao).toHaveTextContent('O que esta métrica não faz');
    expect(botao).not.toHaveTextContent(/\bmeta\b/);
    expect(corpo).not.toHaveTextContent('Atingimento de meta');
    expect(container.textContent).not.toMatch(/organizacao\.Meta\b/);
  });

  it('sem o cadastro lido, diz que a rotina não rodou — e não desenha meta zero', async () => {
    const { bloco } = await montar('vazio');

    expect(bloco.querySelector('.pcen-recolhivel-resumo')).toHaveTextContent(
      'o cadastro de metas da API Gestão de Negócios ainda não foi lido',
    );
    expect(bloco).toHaveTextContent('O cadastro de metas ainda não foi lido');
    expect(within(bloco).queryByRole('table', { hidden: true })).toBeNull();
  });

  it('no alcance Próprios, o vazio é "em seu nome", as vendas contam pela sua filial, e a nota do login aparece', async () => {
    const lida = metasDeVenda('completo', FILIAL);
    const { bloco, botaoDaMeta } = await montar('completo', {
      dados: {
        ...lida,
        alcance: 'Proprios',
        porConsultor: [],
        metricasSemDado: [{ metrica: 'loginSemCasamento', motivo: 'Seu login não casa com consultor nenhum da API Gestão de Negócios.' }],
      },
    });

    expect(botaoDaMeta).toHaveTextContent('Sua meta de venda × o realizado');
    expect(bloco).toHaveTextContent('Nenhuma meta nem venda de máquina em seu nome');
    expect(bloco).not.toHaveTextContent('esta filial não tem meta');
    expect(bloco).toHaveTextContent('vendas pela sua filial');
    expect(bloco).toHaveTextContent('Seu login não casa');
  });

  it('o 403 é falta de permissão, e o bloco não oferece tentar de novo', async () => {
    const { bloco } = await montar('completo', 'recusada');

    expect(bloco).toHaveTextContent('Sem permissão para esta consulta');
    expect(within(bloco).queryByRole('button', { name: 'Tentar de novo', hidden: true })).toBeNull();
    expect(within(bloco).queryByRole('table', { hidden: true })).toBeNull();
  });
});

describe('Performance de CEN na maquete (01/10/2026)', () => {
  it('os cartões dizem o recorte contra a filial, e a comparação com o período anterior fica de fora com a lacuna', async () => {
    const { container } = await montarComCobertura();

    expect(contextoDo(container, 'CENs com carteira')).toMatch(/^de [\d.]+ CENs da filial$/);
    expect(contextoDo(container, 'Vínculos atendidos')).toMatch(/^de [\d.]+ vínculos$/);
    expect(contextoDo(container, 'Cobertura em 30 dias')).toMatch(/^[\d.]+ de [\d.]+ vínculos$/);
    expect(container.querySelector('[data-bloco="kpis"]')).not.toHaveTextContent('período anterior');

    const lacunas = container.querySelector('[data-recolhivel-corpo="lacunas"]')!;
    expect(lacunas).toHaveTextContent('A comparação com o período anterior');
  });

  it('o principal alerta é a classe com mais vínculos fora da cadência e nunca contatados', async () => {
    const { container } = await montarComCobertura();

    const alerta = container.querySelector('[data-bloco="principal-alerta"]')!;
    // NA AMOSTRA, a classe D tem o maior volume das duas faixas somadas.
    expect(alerta).toHaveTextContent(/Classe D concentra o maior volume de vínculos fora da cadência \(\d+%\) e nunca contatados \(\d+%\)\./);
    expect(alerta).toHaveTextContent(/Essa classe representa \d+% de todos os vínculos/);
  });

  it('a classe do cliente vai à API, recorta os cartões e deixa só a barra dela', async () => {
    const { container } = await montarComCobertura();
    const antes = valorDo(container, 'Vínculos atendidos');

    fireEvent.change(filtro(container, 'classe'), { target: { value: 'A' } });

    await waitFor(() => expect(pedidos.some((p) => p.startsWith('/v1/relatorios/cobertura?') && p.includes('classe=A'))).toBe(true));
    await waitFor(() => expect(valorDo(container, 'Vínculos atendidos')).not.toBe(antes));
    await waitFor(() => expect(container.querySelectorAll('.pcen-barras-linha')).toHaveLength(1));
    expect(container.querySelector('.pcen-barras-linha')).toHaveTextContent('Classe A');
    // COM UMA CLASSE SÓ NÃO HÁ O QUE COMPARAR, e o alerta some.
    expect(container.querySelector('[data-bloco="principal-alerta"]')).toBeNull();
  });

  it('a carteira vai ao painel e ao funil, e o período ao funil, às perdas e à meta', async () => {
    const { container } = await montarComCobertura();

    const carteira = [...filtro(container, 'carteira').options].find((o) => o.value !== '')!.value;
    fireEvent.change(filtro(container, 'carteira'), { target: { value: carteira } });
    await waitFor(() =>
      expect(pedidos.some((p) => p.startsWith('/v1/relatorios/cen?') && p.includes(`carteira=${encodeURIComponent(carteira)}`))).toBe(true),
    );
    expect(pedidos.some((p) => p.startsWith('/v1/relatorios/funil-por-estagio?') && p.includes('carteira='))).toBe(true);

    fireEvent.change(filtro(container, 'periodo'), { target: { value: '90' } });
    await waitFor(() => expect(pedidos.some((p) => p.startsWith('/v1/relatorios/funil-por-estagio?') && /de=\d{4}-\d{2}-\d{2}/.test(p))).toBe(true));
    expect(pedidos.some((p) => p.startsWith('/v1/relatorios/vendas-perdidas?') && /ate=\d{4}-\d{2}-\d{2}/.test(p))).toBe(true);
    expect(pedidos.some((p) => p.startsWith('/v1/relatorios/metas?') && /competenciaInicial=\d{4}-\d{2}/.test(p))).toBe(true);

    fireEvent.click(screen.getByRole('button', { name: 'Limpar filtros' }));
    await waitFor(() => expect(filtro(container, 'carteira').value).toBe(''));
    expect(filtro(container, 'periodo').value).toBe('fy');
  });

  it('o CEN escolhido recorta os cartões e aparece em destaque entre os outros, com o nome abreviado da maquete', async () => {
    const { container } = await montarComCobertura();
    const censNaFilial = contextoDo(container, 'CENs com carteira');

    const alfa = [...filtro(container, 'cen').options].find((o) => o.textContent?.startsWith('ALFA MATIAS DE ARAUJO CRUZ'))!;
    fireEvent.change(filtro(container, 'cen'), { target: { value: alfa.value } });

    await waitFor(() => expect(valorDo(container, 'CENs com carteira')).toBe('1'));
    expect(contextoDo(container, 'CENs com carteira')).toBe(censNaFilial);

    const destaque = container.querySelector('.pcen-cens-linha[data-escolhido="true"]')!;
    expect(destaque).toHaveTextContent('Alfa M. Cruz');
    expect(container.querySelectorAll('.pcen-cens-linha').length).toBeGreaterThan(1);
    expect(container.querySelector('.pcen-ranking-linha[data-escolhido="true"]')).toHaveTextContent('Alfa M. Cruz');
  });

  it('o ranking abre na cobertura em 30 dias, da maior para a menor', async () => {
    const { container } = await montarComCobertura();

    expect(container.querySelector('[data-bloco="ranking"]')).toHaveTextContent(/Top \d+ CENs por cobertura em 30 dias\./);
    const valores = [...container.querySelectorAll('.pcen-ranking-valor')].map((v) => Number(v.textContent!.replace('%', '')));
    expect(valores.length).toBeGreaterThan(1);
    expect([...valores].sort((a, b) => b - a)).toEqual(valores);
  });

  it('os cartões do fim abrem um por vez, e o funil do CEN mora no quarto', async () => {
    const { container } = await montarComCobertura();

    const funil = container.querySelector<HTMLButtonElement>('[data-recolhivel="funil"]')!;
    const numeros = container.querySelector<HTMLButtonElement>('[data-recolhivel="numeros"]')!;
    expect(funil).toHaveAttribute('aria-expanded', 'false');

    fireEvent.click(funil);
    expect(funil).toHaveAttribute('aria-expanded', 'true');
    const corpo = container.querySelector<HTMLElement>('[data-recolhivel-corpo="funil"]')!;
    expect(corpo).toBeVisible();
    await waitFor(() => expect(within(corpo).getByRole('table')).toHaveTextContent('Faturamento'));
    expect(corpo).toHaveTextContent('Perdas no período');

    fireEvent.click(numeros);
    expect(funil).toHaveAttribute('aria-expanded', 'false');
    expect(corpo).not.toBeVisible();
    expect(container.querySelector('[data-recolhivel-corpo="numeros"]')).toBeVisible();
  });
});
