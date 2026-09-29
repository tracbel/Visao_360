/**
 * O PAINEL EXECUTIVO DA VISÃO 360 DIZ A VERDADE (24/09/2026).
 *
 * Este teste não julga o desenho — a largura é da conferência visual
 * (`testes-visuais/visao360.spec.ts`). Ele prende os TEXTOS que estavam errados
 * e as regras que os consertaram:
 *
 * - o ano é o FISCAL (nov→out), o período padrão desde 27/09/2026, e a tela não
 *   diz mais que o calendário "não foi confirmado" nem que o FY "vem depois";
 * - a meta é a de VENDA da API Gestão de Negócios, em máquinas, no ano fiscal
 *   (#138, 27/09/2026) — e o cartão diz quando o cadastro não foi lido e quando
 *   falta a permissão, em vez de mostrar meta zero;
 * - "participação de mercado" virou Captura Tracbel (issue 162), e o cartão de
 *   mercado não diz mais que o ART não está no banco;
 * - nenhum `title=` — a explicação é dica que abre pelo teclado (issue 167);
 * - nenhuma citação de documento interno no texto da tela;
 * - o zero de "Clientes na carteira" diz que é falta de VÍNCULO, e não de cliente;
 * - o desenho da maquete de 29/09/2026 não inventa número: selo só onde há comparação, seletor sem alternativa
 *   desligado e com o motivo, e a "última atualização" é a hora da leitura do painel.
 *
 * O CAMINHO É O DE PRODUÇÃO: o consolidado e os cinco cartões são lidos filial a
 * filial por um `fetch` de mentira que responde com as MESMAS amostras do harness
 * `#/dev/visao360-visual`. Os gráficos entram como dublê — o jsdom não tem canvas.
 */

import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
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

/**
 * O ano fiscal do ÚLTIMO MÊS FECHADO — o padrão desde 27/09/2026: em novembro é o ano que acabou de fechar, e
 * em dezembro já é o do ano civil seguinte.
 */
const ULTIMO_FECHADO = new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1);
const ANO_FISCAL =
  ULTIMO_FECHADO.getMonth() + 1 >= 11 ? ULTIMO_FECHADO.getFullYear() + 1 : ULTIMO_FECHADO.getFullYear();

beforeEach(() => guardado.clear());
afterEach(() => vi.unstubAllGlobals());

describe('Painel executivo da Visão 360 — textos verdadeiros', () => {
  it('o ano é o fiscal (27/09/2026), sempre com o intervalo ao lado, e a dica não promete mais a próxima etapa', async () => {
    const { container } = await montar('completo');

    expect(container).toHaveTextContent('ano fiscal (nov–out)');
    expect(container.textContent).not.toMatch(/não confirmado|não foi confirmado|não há FY|ano civil \(jan–dez\)/i);

    // O NOME DO ANO NUNCA SOZINHO: "FY2026" sem o intervalo se lê como ano civil.
    const opcao = screen.getByRole('option', { name: `FY${ANO_FISCAL} (nov/${ANO_FISCAL - 1} a out/${ANO_FISCAL})` });
    expect((opcao as HTMLOptionElement).selected).toBe(true);

    const dica = textoDaDica(screen.getByRole('button', { name: 'Como o ano fiscal é contado' }));
    expect(dica).toContain('novembro a outubro');
    expect(dica).not.toContain('próxima etapa');
    // ATÉ O ÚLTIMO MÊS FECHADO, como os Indicadores (27/09/2026): o mês em curso fica no cartão do mês.
    expect(dica).toContain('até o ÚLTIMO MÊS FECHADO');
    expect(dica).not.toContain('até hoje');
  });

  it('a meta é a de venda, em máquinas, no ano fiscal — e a de faturamento, que nunca teve fonte, sumiu (#138)', async () => {
    const { container } = await montar('completo');

    const cartao = await waitFor(() => {
      const c = container.querySelector('[data-kpi="Meta e realizado · FY2026"]');
      expect(c).not.toBeNull();
      return c!;
    });
    // O CARTÃO É CURTO (28/09/2026, desenho dos Indicadores): o realizado, a meta, o percentual e o período; o resto da
    // composição do número está na dica ao lado do nome.
    await waitFor(() => expect(cartao).toHaveTextContent(/\d+\s*de \d+/));
    expect(cartao).toHaveTextContent('da meta');
    expect(cartao).toHaveTextContent('nov/2025 a ago/2026');

    // A TABELA ANTIGA (`organizacao.Meta`, sem linha desde a fase 1) não é citada; a nova é `organizacao.MetaDeVenda`.
    expect(container.textContent).not.toMatch(/organizacao\.Meta\b/);
    expect(container.textContent).not.toContain('issue 138');

    const regra = textoDaDica(screen.getByRole('button', { name: 'Fonte e método: Meta e realizado · FY2026' }));
    expect(regra).toContain('Máquinas vendidas');
    expect(regra).toContain('vendas aguardam na integração do ART (cadastro, chassi ou outro motivo)');
    expect(regra).toContain('consórcio');
    expect(regra).toContain('set/2026 em curso');
    expect(regra).toContain('API Gestão de Negócios');
    expect(regra).toContain('em máquinas');
    expect(regra).toContain('último mês fechado');
    // AS LACUNAS DA META NA DICA (revisão do PR #248): as com número refeitas da soma das filiais.
    expect(regra).toMatch(/\d+ consultor\(es\) com meta não têm conta no CRM/);
    expect(regra).toMatch(/\d+ venda\(s\) do período sem vendedor no ART/);
    expect(regra).toContain('unidade sem filial no CRM');

    // NA COMPOSIÇÃO, a meta e o realizado são em máquinas, e a coluna em reais chama-se Faturamento.
    // A composição abre pelo botão do painel dela.
    const composicao = container.querySelector<HTMLElement>('[data-bloco="composicao"]')!;
    fireEvent.click(within(composicao).getByRole('button', { name: 'Ver filial a filial' }));
    const cabecalho = composicao.querySelector('thead')!;
    expect(cabecalho).toHaveTextContent('Meta FY2026 (máq.)');
    expect(cabecalho).toHaveTextContent('Realizado FY2026 (máq.)');
    expect(cabecalho).toHaveTextContent(`Faturamento FY${ANO_FISCAL}`);
    expect(cabecalho).toHaveTextContent('Máquinas entregues');
    expect(cabecalho).toHaveTextContent(`NF Protheus FY${ANO_FISCAL}`);
  });

  it('o faturamento do ano é o ART das máquinas entregues, com a quantidade, e a nota do Protheus é só conferência (29/09/2026)', async () => {
    const { container } = await montar('completo');

    // "O FATURAMENTO REAL DO ANO FISCAL VEM DO ART DE MÁQUINAS ENTREGUES" — o valor, e a quantidade na linha de baixo.
    const cartao = container.querySelector(`[data-kpi="Faturamento FY${ANO_FISCAL}"]`)!;
    expect(cartao).toHaveTextContent(/R\$ [\d,]+ M/);
    expect(cartao).toHaveTextContent(/[\d.]+ máquinas entregues · até [a-z]{3}\/\d{4}/);
    expect(container.textContent).not.toContain('Faturamento em curso');

    const regra = textoDaDica(screen.getByRole('button', { name: `Fonte e método: Faturamento FY${ANO_FISCAL}` }));
    expect(regra).toContain('valor de venda do ART das máquinas ENTREGUES');
    expect(regra).toContain('com e sem comprador no CRM');
    expect(regra).toContain('ainda não viraram venda no CRM');
    expect(regra).toContain('sem valor de venda no ART contam nas máquinas');
    expect(regra).toContain('Mesmo trecho do ano anterior');
    expect(regra).toContain('em curso e à parte');
    expect(regra).toContain('Conferência, sem somar: a nota de saída do Protheus');
  });

  it('sem máquina entregue no período, o faturamento é o traço com o motivo — e não R$ 0', async () => {
    const { container } = await montar('vazio');

    const cartao = container.querySelector(`[data-kpi="Faturamento FY${ANO_FISCAL}"]`)!;
    expect(cartao.textContent).not.toMatch(/R\$ 0/);
    const motivo = textoDaDica(screen.getByRole('button', { name: 'Por que o faturamento do ano não aparece' }));
    expect(motivo).toContain('Nenhuma máquina com entrega');
  });

  it('sem a rotina das metas ter rodado, o cartão diz que o cadastro não foi lido — e não "meta zero"', async () => {
    const { container } = await montar('vazio');

    const cartao = await waitFor(() => {
      const c = container.querySelector('[data-kpi="Meta e realizado · FY2026"]');
      expect(c).not.toBeNull();
      return c!;
    });
    expect(cartao).toHaveTextContent('o cadastro de metas da Gestão de Negócios ainda não foi lido');
  });

  it('a filial cuja meta falhou fica fora da soma NOMEADA, no cartão e no total da composição', async () => {
    instalarApi('completo');
    const original = globalThis.fetch;
    vi.stubGlobal(
      'fetch',
      vi.fn(async (entrada: string, init?: RequestInit) =>
        entrada.includes('/relatorios/metas') && new Headers(init?.headers).get('X-Tracbel-Empresa') === '990002'
          ? new Response(JSON.stringify({ title: 'Falha interna' }), { status: 500 })
          : original(entrada, init),
      ),
    );
    const tela = render(
      <ProvedorDeContextoDeAcesso>
        <MemoryRouter>
          <PainelExecutivo />
        </MemoryRouter>
      </ProvedorDeContextoDeAcesso>,
    );

    const cartao = await waitFor(() => {
      const c = tela.container.querySelector('[data-kpi="Meta e realizado · FY2026"]');
      expect(c).toHaveTextContent(/\d+ de \d+/);
      return c!;
    });
    expect(cartao.querySelector('.v360-periodo-alerta')).toHaveTextContent('4 de 5 filiais — fora: Filial Fictícia Beta');

    await waitFor(() => expect(tela.container.querySelector('[data-bloco="linha-3"]')).not.toBeNull());
    const composicao = tela.container.querySelector<HTMLElement>('[data-bloco="composicao"]')!;
    fireEvent.click(within(composicao).getByRole('button', { name: 'Ver filial a filial' }));
    const total = composicao.querySelector('tr[data-linha="total"]')!;
    expect(total).toHaveTextContent('(4 de 5)');
  });

  it('sem a permissão Meta.Ler, o cartão diz que falta a permissão', async () => {
    instalarApi('completo');
    const original = globalThis.fetch;
    vi.stubGlobal(
      'fetch',
      vi.fn(async (entrada: string, init?: RequestInit) =>
        entrada.includes('/relatorios/metas')
          ? new Response(JSON.stringify({ title: 'Sem permissão', type: 'https://tracbel/erros/sem-permissao' }), { status: 403 })
          : original(entrada, init),
      ),
    );
    const tela = render(
      <ProvedorDeContextoDeAcesso>
        <MemoryRouter>
          <PainelExecutivo />
        </MemoryRouter>
      </ProvedorDeContextoDeAcesso>,
    );

    await waitFor(() => expect(tela.container.querySelector('[data-kpi="Meta e realizado"]')).not.toBeNull());
    await waitFor(() => screen.getByRole('button', { name: 'Por que a meta e o realizado não aparece' }));
    expect(textoDaDica(screen.getByRole('button', { name: 'Por que a meta e o realizado não aparece' }))).toContain('Meta.Ler');
  });

  it('o mercado fala em Captura Tracbel, nunca em participação ou share, e não diz que o ART está fora do banco', async () => {
    const { container } = await montar('completo');

    expect(container.textContent).not.toMatch(/participação de mercado|\bshare\b|emplacamento/i);
    expect(container.textContent).not.toContain('não estão no banco do CRM');

    // PARA QUEM PERDEMOS, a aba das vendas perdidas por concorrente, aponta a Captura Tracbel nos Indicadores.
    const perdas = container.querySelector<HTMLElement>('[data-bloco="linha-4"]')!;
    fireEvent.click(within(perdas).getByRole('button', { name: 'Para quem perdemos' }));
    expect(perdas).toHaveTextContent('Captura Tracbel');

    const regra = textoDaDica(screen.getByRole('button', { name: 'Fonte e método: Conhecimento de mercado' }));
    expect(regra).toContain('Captura Tracbel');
    expect(regra).toContain('unidades');
    expect(regra).not.toContain('não estão no banco do CRM');
  });

  it('não há `title=` no painel: toda explicação é dica que abre pelo teclado (issue 167)', async () => {
    const { container } = await montar('completo');

    expect(container.querySelectorAll('[title]')).toHaveLength(0);

    // A regra de cada cartão com número virou ⓘ ao lado do título.
    for (const titulo of ['Clientes na carteira', 'Cobertura pela cadência', 'Conhecimento de mercado']) {
      expect(screen.getByRole('button', { name: `Fonte e método: ${titulo}` })).toBeInTheDocument();
    }

    // O nome inteiro do cliente — cortado com reticências na tela — e a classe dele.
    const cliente = screen.getByRole('button', { name: /^COOPERATIVA FICTÍCIA .* classe A$/ });
    expect(textoDaDica(cliente)).toContain('INTERIOR PAULISTA LTDA — classe A na curva ABC');

    // Os nomes completos das linhas do mix, que saíram do `title` de cada rótulo.
    expect(textoDaDica(screen.getByRole('button', { name: 'Como ler mix por linha' }))).toContain(
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
    // A divisão por situação é de quem TEM vínculo: com zero, ela não aparece como "0 suspects".
    expect(cartao.textContent).not.toMatch(/suspects/);

    const regra = textoDaDica(screen.getByRole('button', { name: 'Fonte e método: Clientes na carteira' }));
    expect(regra).toContain('cliente cadastrado sem carteira não entra nesta conta');
    expect(regra).toContain('não que o cadastro está vazio');
    expect(regra).not.toMatch(/suspects/);
  });

  it('sem vínculo, cobertura, CENs e mix dizem por que estão vazios', async () => {
    const { container } = await montar('semCarteira');

    expect(container).toHaveTextContent('Sem dado para a cobertura');
    expect(container).toHaveTextContent('Sem dado para o ranking de CENs');
    expect(container).toHaveTextContent('Sem dado para o mix por linha');
  });

  it('sem processo perdido, a tela não diz que "os 0 processos perdidos existem" — diz o motivo verdadeiro', async () => {
    const { container } = await montar('vazio');

    // A ROTINA DO FUNIL AINDA NÃO RODOU (27/09/2026): o processo perdido vem dela, e o cartão diz isso.
    await waitFor(() => expect(container.querySelector('[data-bloco="linha-4"]')).toHaveTextContent('ainda não rodou'));
    expect(container.textContent).not.toContain('Os 0 processos perdidos existem');
  });

  it('sem funil, o alerta dos processos parados é "—" com o motivo, e não zero', async () => {
    const { container } = await montar('vazio');

    await waitFor(() =>
      expect(container.querySelector('.v360-alertas-list')).toHaveTextContent('Processos parados em Negociação ou Pedido:'),
    );
    const motivo = textoDaDica(screen.getByRole('button', { name: 'Por que os processos parados não aparece' }));
    expect(motivo).toContain('ainda não rodou');
  });
});

describe('Painel executivo da Visão 360 — as perdas e o funil do período (documento 52)', () => {
  it('conta os processos parados em Negociação ou Pedido somando as filiais', async () => {
    const { container } = await montar('completo');

    await waitFor(() =>
      expect(container.querySelector('.v360-alertas-list')).toHaveTextContent(
        /\d+ processos parados em Negociação ou Pedido há mais de 60 dias/,
      ),
    );
    expect(container.querySelector('.v360-alertas-list')).not.toHaveTextContent('processos abertos');
  });

  it('as vendas perdidas são do período, e o período está escrito nos dois cartões', async () => {
    const { container } = await montar('completo');

    const perdas = container.querySelector<HTMLElement>('[data-bloco="linha-4"]')!;
    await waitFor(() => expect(perdas).toHaveTextContent('nov/2025 a ago/2026'));
    fireEvent.click(within(perdas).getByRole('button', { name: 'Para quem perdemos' }));
    expect(perdas).toHaveTextContent('Pela venda perdida principal · nov/2025 a ago/2026');
  });
});

describe('Painel executivo da Visão 360 — o desenho da maquete de 29/09/2026', () => {
  it('o selo do faturamento é a variação da dica, o da meta é o percentual da linha de baixo, e cartão sem comparação não tem selo', async () => {
    const { container } = await montar('completo');

    // O SELO DO FATURAMENTO: a mesma variação da dica, contra o mesmo trecho do ano anterior, dita por extenso ao leitor.
    const faturamento = container.querySelector(`[data-kpi="Faturamento FY${ANO_FISCAL}"]`)!;
    const selo = faturamento.querySelector<HTMLElement>('.mv-kpi-selo')!;
    expect(selo).toHaveTextContent('no valor, contra o mesmo trecho do ano anterior');
    const desenho = selo.querySelector('.v360-selo-desenho')!.textContent!;
    expect(desenho).toMatch(/^[\d,]+%$/);
    const regra = textoDaDica(screen.getByRole('button', { name: `Fonte e método: Faturamento FY${ANO_FISCAL}` }));
    expect(regra).toContain(`${desenho} no valor`);

    // O SELO DA META é o percentual que a linha de baixo já diz.
    const seloDaMeta = await waitFor(() => {
      const s = container.querySelector('[data-kpi="Meta e realizado · FY2026"] .mv-kpi-selo');
      expect(s).not.toBeNull();
      return s!;
    });
    expect(container.querySelector('[data-kpi="Meta e realizado · FY2026"] .mv-kpi-contexto')).toHaveTextContent(
      `${seloDaMeta.textContent} da meta`,
    );

    // A MAQUETE TEM SELO EM CLIENTES E EM MERCADO, e o CRM não tem período anterior para essas contas: nada é inventado.
    for (const nome of ['Clientes na carteira', 'Cobertura pela cadência', 'Conhecimento de mercado']) {
      expect(container.querySelector(`[data-kpi="${nome}"] .mv-kpi-selo`)).toBeNull();
    }
  });

  it('o cabeçalho diz quais filiais responderam e a hora da última leitura do painel', async () => {
    const { container } = await montar('completo');

    await waitFor(() => expect(container.querySelector('.v360-atualizado-em')).not.toBeNull());
    expect(container.querySelector('.v360-atualizado-em')).toHaveTextContent(
      /^Última atualização: \d{2}\/\d{2}\/\d{4} \d{2}:\d{2}$/,
    );

    const dica = textoDaDica(screen.getByRole('button', { name: 'Quais filiais responderam' }));
    expect(dica).toContain('uma a uma');
    expect(dica).toMatch(/Responderam: .*Filial Fictícia/);
  });

  it('os seletores da maquete sem alternativa ficam desligados, com o motivo na dica', async () => {
    await montar('completo');

    expect(screen.getByRole('combobox', { name: 'Unidade do faturamento' })).toBeDisabled();
    expect(textoDaDica(screen.getByRole('button', { name: 'Por que unidade do faturamento está desligado' }))).toContain(
      'milhões de reais',
    );

    expect(screen.getByRole('combobox', { name: 'Ordem do ranking' })).toBeDisabled();
    expect(textoDaDica(screen.getByRole('button', { name: 'Por que ordem do ranking está desligado' }))).toContain(
      'vínculos em carteira comercial',
    );
  });

  it('as abas das vendas perdidas moram na linha do título e abrem em "Por motivo"', async () => {
    const { container } = await montar('completo');

    const perdas = container.querySelector<HTMLElement>('[data-bloco="linha-4"] [data-area="v360-perdas"]')!;
    const direita = perdas.querySelector<HTMLElement>('.mom-painel-direita')!;
    expect(within(direita).getByRole('button', { name: 'Por motivo' })).toHaveAttribute('aria-pressed', 'true');

    fireEvent.click(within(direita).getByRole('button', { name: 'Para quem perdemos' }));
    expect(within(direita).getByRole('button', { name: 'Para quem perdemos' })).toHaveAttribute('aria-pressed', 'true');
    await waitFor(() => expect(perdas).toHaveTextContent('Pela venda perdida principal'));
  });
});
