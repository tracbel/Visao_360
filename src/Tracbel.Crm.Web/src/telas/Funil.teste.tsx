/**
 * O FUNIL NA TELA (27/09/2026, documento 52) — os seis estágios do Vórtice, a chave coorte × fluxo e o vazio com o
 * motivo verdadeiro.
 *
 * O CAMINHO É O DE PRODUÇÃO: as rotas são lidas por um `fetch` de mentira que responde com as amostras do harness da
 * Visão 360, e anota o que a tela pediu.
 */

import { fireEvent, render, waitFor, within } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { respostaDaVisao360, type EstadoDaVisao360 } from '../dev/amostrasDaVisao360';
import { Funil } from './Funil';

vi.setConfig({ testTimeout: 20_000 });

const guardado = new Map<string, string>();
const pedidos: string[] = [];

function montar(estado: EstadoDaVisao360) {
  vi.stubGlobal('localStorage', {
    getItem: (chave: string) => guardado.get(chave) ?? null,
    setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
    removeItem: (chave: string) => void guardado.delete(chave),
    clear: () => guardado.clear(),
  });
  vi.stubGlobal(
    'fetch',
    vi.fn(async (entrada: string, init?: RequestInit) => {
      pedidos.push(entrada);
      const [caminho, consulta = ''] = entrada.replace(/^.*\/api/, '').split('?');
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
  return render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <Funil />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
}

beforeEach(() => {
  guardado.clear();
  pedidos.length = 0;
});
afterEach(() => vi.unstubAllGlobals());

describe('Funil de Vendas — o funil por estágio (documento 52)', () => {
  it('abre na coorte do ano fiscal, com os seis estágios e os dois percentuais', async () => {
    const { container } = montar('completo');
    const bloco = await waitFor(() => {
      const b = container.querySelector('[data-bloco="funil-por-estagio"]') as HTMLElement;
      expect(b.querySelector('#funil-svg')).not.toBeNull();
      return b;
    });

    expect(bloco).toHaveTextContent('Coorte · nov/2025 a ago/2026 · ano fiscal até o último mês fechado');
    // NA MAQUETE DE 29/09/2026 a tabela dos seis estágios saiu do painel do funil e ganhou um cartão só dela.
    const tabela = container.querySelector('[data-bloco="funil-tabela"]') as HTMLElement;
    const linhas = within(tabela).getAllByRole('row').slice(1);
    // O ESTÁGIO É O CABEÇALHO DA LINHA (`th scope="row"`), como nas tabelas dos Indicadores (29/09/2026).
    expect(linhas.map((l) => l.querySelector('th')?.textContent)).toEqual([
      'Lead', 'Qualificado', 'Cobertura', 'Negociação', 'Pedido', 'Faturamento',
    ]);
    expect(linhas[0]).toHaveTextContent('100%');
    expect(pedidos.some((p) => p.includes('/relatorios/funil-por-estagio?base=abertura'))).toBe(true);
    expect(container).toHaveTextContent('Para quem perdemos');
  });

  it('a chave troca para o fluxo', async () => {
    const { container, getByRole } = montar('completo');
    await waitFor(() => expect(container.querySelector('#funil-svg')).not.toBeNull());

    fireEvent.click(getByRole('button', { name: 'Fluxo' }));

    await waitFor(() => expect(pedidos.some((p) => p.includes('base=etapa'))).toBe(true));
    await waitFor(() => expect(container.querySelector('[data-bloco="funil-por-estagio"]')).toHaveTextContent('Fluxo ·'));
  });

  it('sem a rotina ter rodado, cada estágio é "—" com o motivo, e não zero', async () => {
    const { container } = montar('vazio');
    const tabela = await waitFor(() => {
      const t = container.querySelector('[data-bloco="funil-tabela"]') as HTMLElement;
      expect(within(t).getAllByRole('row')).toHaveLength(7);
      return t;
    });

    expect(container.querySelector('[data-bloco="funil-por-estagio"] #funil-svg')).toBeNull();
    expect(tabela.querySelectorAll('.cad-ausente').length).toBeGreaterThanOrEqual(6);
    // O PAINEL DO FUNIL DIZ POR QUÊ, e a conversão fica com os seis estágios e o traço.
    expect(container.querySelector('[data-bloco="funil-por-estagio"] .funil-sem-grafico')).toHaveTextContent('ainda não rodou');
    const conversao = container.querySelector('[data-bloco="funil-conversao"]') as HTMLElement;
    expect(within(conversao).getAllByRole('row')).toHaveLength(7);
    expect(conversao.querySelector('.funil-conversao-barra')).toBeNull();
    expect(container).toHaveTextContent('ainda não rodou');
  });
});

describe('Funil de Vendas — o desenho da maquete de 29/09/2026', () => {
  it('a conversão entre estágios fica num painel ao lado, com a barra e o percentual na cor da faixa', async () => {
    const { container } = montar('completo');
    const conversao = await waitFor(() => {
      const c = container.querySelector('[data-bloco="funil-conversao"]') as HTMLElement;
      expect(c.querySelectorAll('.funil-conversao-barra')).toHaveLength(5);
      return c;
    });

    const linhas = within(conversao).getAllByRole('row').slice(1);
    expect(linhas.map((l) => l.querySelector('th')?.textContent)).toEqual([
      'Lead', 'Qualificado', 'Cobertura', 'Negociação', 'Pedido', 'Faturamento',
    ]);
    // O PRIMEIRO ESTÁGIO NÃO TEM DE ONDE CONVERTER: travessão, e não percentual.
    expect(linhas[0].querySelector('[data-tom]')).toBeNull();
    // A MESMA REGRA DE COR DA LEGENDA ANTIGA: 70% ou mais, verde; de 40% a 70%, laranja; abaixo, vermelho.
    for (const linha of linhas.slice(1)) {
      const pct = linha.querySelector('[data-tom]') as HTMLElement;
      const valor = Number(pct.textContent?.replace(/[^\d]/g, ''));
      expect(pct.dataset.tom).toBe(valor >= 70 ? 'bom' : valor >= 40 ? 'medio' : 'baixo');
    }
    // A CHAVE COORTE × FLUXO mora em cima da conversão.
    expect(container.querySelector('.funil-lado .funil-chave')).not.toBeNull();
  });

  it('as perdas abrem na faixa laranja com o que a distribuição não diz, e as vendas perdidas em "Para quem perdemos"', async () => {
    const { container, getByRole } = montar('completo');
    const aviso = await waitFor(() => {
      const a = container.querySelector('[data-bloco="perdas-resumo"] .funil-perdas-aviso') as HTMLElement;
      expect(a).not.toBeNull();
      return a;
    });

    expect(aviso).toHaveTextContent('O que a distribuição de perdas não diz');
    expect(aviso).toHaveTextContent(/derrotas que foram registradas, e não de todos os [\d.]+ processos perdidos no período/);
    expect(container.querySelector('section.dash-secao[data-bloco="secao-perdas"] .terr-secao-titulo')).toHaveTextContent(
      'As perdas do período',
    );

    // COMO NA MAQUETE, a aba aberta é "Para quem perdemos"; "Por motivo" continua a um clique.
    const painel = container.querySelector('[data-bloco="perdas-por-motivo"]') as HTMLElement;
    expect(getByRole('button', { name: 'Para quem perdemos' })).toHaveAttribute('aria-pressed', 'true');
    expect(painel.querySelector('thead th')).toHaveTextContent('Concorrente');
    fireEvent.click(getByRole('button', { name: 'Por motivo' }));
    expect(painel.querySelector('thead th')).toHaveTextContent('Motivo');

    // O PREÇO E O DENOMINADOR NA MESMA LINHA.
    expect(painel.querySelector('.funil-preco .funil-preco-base')).toHaveTextContent(/^de \d+ de \d+$/);
  });
});
