/**
 * O PAINEL EXECUTIVO DA VISÃO 360 DIZ A VERDADE (24/09/2026).
 *
 * Este teste não julga o desenho — a largura é da conferência visual
 * (`testes-visuais/visao360.spec.ts`). Ele prende os TEXTOS que estavam errados
 * e as regras que os consertaram:
 *
 * - o ano é civil, e a tela parou de dizer que o calendário fiscal "não foi
 *   confirmado": foi, em 24/09 — a visão por FY é o próximo passo, na dica;
 * - a meta não tem tabela desde a fase 1: nada afirma que ela existe, e o
 *   motivo (issue 138) está na dica do "—";
 * - "participação de mercado" virou Captura Tracbel (issue 162), e o cartão de
 *   mercado não diz mais que o ART não está no banco;
 * - nenhum `title=` — a explicação é dica que abre pelo teclado (issue 167);
 * - nenhuma citação de documento interno no texto da tela;
 * - o zero de "Clientes na carteira" diz que é falta de VÍNCULO, e não de cliente.
 *
 * O CAMINHO É O DE PRODUÇÃO: o consolidado e os cinco cartões são lidos filial a
 * filial por um `fetch` de mentira que responde com as MESMAS amostras do harness
 * `#/dev/visao360-visual`. Os gráficos entram como dublê — o jsdom não tem canvas.
 */

import { act, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import { respostaDaVisao360, type EstadoDaVisao360 } from '../../dev/amostrasDaVisao360';
import { PainelExecutivo } from './PainelExecutivo';

vi.setConfig({ testTimeout: 20_000 });

vi.mock('../GraficoDonutCentro', () => ({ GraficoDonutCentro: () => <div data-grafico="donut" /> }));
vi.mock('../GraficoLinhaMensal', () => ({ GraficoLinhaMensal: () => <div data-grafico="linha" /> }));
vi.mock('../GraficoBarrasHorizontais', () => ({ GraficoBarrasHorizontais: () => <div data-grafico="barras" /> }));
vi.mock('../MolduraDeGrafico', () => ({
  MolduraDeGrafico: ({ children, altura }: { children: (l: number, a: number) => React.ReactNode; altura: number }) => (
    <div>{children(600, altura)}</div>
  ),
}));

// O `localStorage` do Node não guarda nada sem `--localstorage-file`, e o
// contexto de acesso lê dele na primeira renderização.
const guardado = new Map<string, string>();

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
      const empresa = new Headers(init?.headers).get('X-Tracbel-Empresa') ?? '';
      const dados = respostaDaVisao360(caminho, new URLSearchParams(consulta), empresa, estado);
      return dados === undefined
        ? new Response('{"title":"não simulado"}', { status: 404 })
        : new Response(JSON.stringify({ dados, procedencia: null }), { status: 200 });
    }),
  );
}

async function montar(estado: EstadoDaVisao360) {
  instalarApi(estado);
  const tela = render(
    <ProvedorDeContextoDeAcesso>
      <MemoryRouter>
        <PainelExecutivo />
      </MemoryRouter>
    </ProvedorDeContextoDeAcesso>,
  );
  // Os cinco cartões e o consolidado são duas leituras; a tela está pronta quando as duas voltaram.
  await waitFor(() => expect(tela.container.querySelector('[data-bloco="kpis"] [data-kpi]')).not.toBeNull());
  await waitFor(() => expect(tela.container.querySelector('[data-bloco="linha-3"]')).not.toBeNull());
  return tela;
}

/** Abre uma dica pelo foco — o que a tecla Tab faz — e devolve o texto do balão. */
function textoDaDica(gatilho: HTMLElement): string {
  act(() => gatilho.focus());
  const texto = screen.getByRole('tooltip').textContent ?? '';
  act(() => gatilho.blur());
  return texto;
}

beforeEach(() => guardado.clear());
afterEach(() => vi.unstubAllGlobals());

describe('Painel executivo da Visão 360 — textos verdadeiros', () => {
  it('o ano é civil, sem dizer que o fiscal não foi confirmado, e a dica anuncia o FY', async () => {
    const { container } = await montar('completo');

    expect(container).toHaveTextContent('ano civil (jan–dez)');
    expect(container.textContent).not.toMatch(/não confirmado|não foi confirmado|não há FY/i);

    const dica = textoDaDica(screen.getByRole('button', { name: 'Por que o ano é civil, e quando vem o ano fiscal' }));
    expect(dica).toContain('novembro a outubro');
    expect(dica).toContain('próxima etapa');
  });

  it('a meta aparece como "—" com o motivo, e nenhum texto diz que ela tem tabela', async () => {
    const { container } = await montar('completo');

    expect(container.textContent).not.toContain('organizacao.Meta');
    expect(container.textContent).not.toMatch(/nenhuma cadastrada|sem meta/);

    const [doCartao] = screen.getAllByRole('button', { name: 'Por que a meta não aparece' });
    expect(textoDaDica(doCartao)).toContain('issue 138');

    // A regra do cartão também deixou de citar a tabela removida.
    const regra = textoDaDica(screen.getByRole('button', { name: 'Como se conta: meta e realizado · 2026' }));
    expect(regra).toContain('issue 138');
    expect(regra).not.toContain('organizacao.Meta');
  });

  it('o mercado fala em Captura Tracbel, nunca em participação ou share, e não diz que o ART está fora do banco', async () => {
    const { container } = await montar('completo');

    expect(container).toHaveTextContent('Captura Tracbel');
    expect(container.textContent).not.toMatch(/participação de mercado|\bshare\b|emplacamento/i);
    expect(container.textContent).not.toContain('não estão no banco do CRM');

    const regra = textoDaDica(screen.getByRole('button', { name: 'Como se conta: conhecimento de mercado' }));
    expect(regra).toContain('Captura Tracbel');
    expect(regra).toContain('unidades');
    expect(regra).not.toContain('não estão no banco do CRM');
  });

  it('não há `title=` no painel: toda explicação é dica que abre pelo teclado (issue 167)', async () => {
    const { container } = await montar('completo');

    expect(container.querySelectorAll('[title]')).toHaveLength(0);

    // A regra de cada cartão com número virou ⓘ ao lado do título.
    for (const titulo of ['clientes na carteira', 'cobertura pela cadência', 'conhecimento de mercado']) {
      expect(screen.getByRole('button', { name: `Como se conta: ${titulo}` })).toBeInTheDocument();
    }

    // O nome inteiro do cliente — cortado com reticências na tela — e a classe dele.
    const cliente = screen.getByRole('button', { name: /^COOPERATIVA FICTÍCIA .* classe A$/ });
    expect(textoDaDica(cliente)).toContain('INTERIOR PAULISTA LTDA — classe A na curva ABC');

    // Os nomes completos das linhas do mix, que saíram do `title` de cada rótulo.
    expect(textoDaDica(screen.getByRole('button', { name: 'Como o mix por linha é contado' }))).toContain(
      'Venda de Pulverizadores Autopropelidos',
    );
  });

  it('o texto da tela não cita documento interno', async () => {
    const { container } = await montar('completo');

    expect(container.textContent).not.toMatch(/§|\bdocumento\s+\d|\bdoc\s+\d|\bseção\b|\(P-\d+\)/i);
  });

  it('o mix mostra só linha de carteira comercial', async () => {
    const { container } = await montar('completo');

    const legenda = container.querySelector('.v360-mix-legenda')!;
    expect(legenda).toHaveTextContent('Prospecção de Novos Clientes');
    expect(legenda.textContent).not.toContain('Linha de Teste');
  });
});

describe('Painel executivo da Visão 360 — sem carteira e vazio', () => {
  it('com clientes cadastrados e nenhum vínculo, o zero diz que falta vínculo, e não cliente', async () => {
    const { container } = await montar('semCarteira');

    const cartao = container.querySelector('[data-kpi="Clientes na carteira"]')!;
    expect(cartao).toHaveTextContent('nenhum cliente com vínculo ativo em carteira');
    expect(cartao).toHaveTextContent('cliente cadastrado sem carteira não entra nesta conta');
    // A divisão por situação é de quem TEM vínculo: com zero, ela não aparece como "0 suspects".
    expect(cartao.textContent).not.toMatch(/suspects/);

    const regra = textoDaDica(screen.getByRole('button', { name: 'Como se conta: clientes na carteira' }));
    expect(regra).toContain('não que o cadastro está vazio');
  });

  it('sem vínculo, cobertura, CENs e mix dizem por que estão vazios', async () => {
    const { container } = await montar('semCarteira');

    expect(container).toHaveTextContent('Sem dado para a cobertura');
    expect(container).toHaveTextContent('Sem dado para o ranking de CENs');
    expect(container).toHaveTextContent('Sem dado para o mix por linha');
  });

  it('sem processo perdido, a tela não diz que "os 0 processos perdidos existem"', async () => {
    const { container } = await montar('vazio');

    expect(container).toHaveTextContent('Nenhum processo perdido nas filiais que responderam.');
    expect(container.textContent).not.toContain('Os 0 processos perdidos existem');
  });
});
