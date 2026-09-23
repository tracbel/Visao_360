/**
 * A REDE DA TELA (issue 170 parte A, ampliada na fase T1 do documento 50).
 *
 * Este teste não julga o desenho: ele afirma que a tela continua **montando os
 * mesmos blocos, nos lugares acordados**. No T0 ele provou que a componentização
 * não mexeu em nada; no T1 o contrato mudou de propósito — a página passou a ter
 * duas abas — e ele foi **reescrito, não apagado**: o que era uma lista única de
 * blocos virou a lista de cada aba, mais as afirmações que amarram o recorte
 * compartilhado entre elas.
 *
 * POR QUE `data-bloco` E NÃO O TEXTO: texto muda de redação a cada issue, e um
 * teste que quebra quando alguém corrige uma vírgula é um teste que as pessoas
 * aprendem a ignorar. O atributo é invisível — nenhuma regra de CSS o usa.
 *
 * OS TRÊS PAINÉIS DO MOMENTO E A CALCULADORA ENTRAM COMO DUBLÊ: cada um tem a
 * própria leitura da API e o próprio teste. Aqui o que está sob prova é a
 * composição da tela, e não o conteúdo deles.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { ARARAQUARA, CAFELANDIA, municipioDeTeste } from '../testes/territorio';
import type { PainelTerritorial } from '../tipos/territorio';
import { IndicadoresGeograficos } from './IndicadoresGeograficos';

const obterIndicadoresTerritoriais = vi.hoisted(() => vi.fn());
const carregarMalhaDeSaoPaulo = vi.hoisted(() => vi.fn());

vi.mock('../dados/api/territorio', async (original) => ({
  ...(await original<typeof import('../dados/api/territorio')>()),
  obterIndicadoresTerritoriais,
  carregarMalhaDeSaoPaulo,
}));

vi.mock('../componentes/territorio/PainelDePrecos', () => ({
  PainelDePrecos: () => <div data-bloco="precos" />,
}));
vi.mock('../componentes/territorio/PainelDeCustos', () => ({
  PainelDeCustos: () => <div data-bloco="custos" />,
}));
vi.mock('../componentes/territorio/PainelDeCredito', () => ({
  PainelDeCredito: () => <div data-bloco="credito" />,
}));
vi.mock('../componentes/mercado/Calculadora', () => ({
  Calculadora: () => <div data-bloco="calculadora" />,
}));

// O `localStorage` do Node 25 não guarda nada sem `--localstorage-file`, e o
// contexto de acesso lê dele na primeira renderização.
const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (chave: string) => guardado.get(chave) ?? null,
  setItem: (chave: string, valor: string) => void guardado.set(chave, valor),
  removeItem: (chave: string) => void guardado.delete(chave),
  clear: () => guardado.clear(),
});

function painel(): PainelTerritorial {
  return {
    indicadores: {
      competenciaInicial: '2025-09',
      competenciaFinal: '2026-08',
      referenciaDaCobertura: '2026-09-01T00:00:00Z',
      interacaoMaisRecente: '2026-08-30T00:00:00Z',
      anoDaAreaPlantada: 2024,
      regras: [
        {
          produtoCodigoIbge: 2502,
          produtoNome: 'Café (Total)',
          hectaresPorMaquina: 10,
          modeloDeReferencia: '3036N',
          situacao: 'AConfirmar',
          justificativa: 'regra do protótipo, a confirmar',
          vigenteDesde: '2026-09-21',
          anosDeRenovacao: null,
        },
      ],
      municipios: [
        municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' }),
        municipioDeTeste({ codigoIbge: ARARAQUARA, nome: 'Araraquara', pertenceAAdr: false }),
      ],
      foraDoMapa: [
        {
          grupo: 'MunicipioSemCodigoIbge',
          descricao: 'clientes cujo município não tem código IBGE',
          cobertura: {
            clientes: 1,
            vinculos: 1,
            vinculosComCadencia: 2,
            cobertos: 1,
            foraDaCadencia: 1,
            nuncaContatados: 0,
            semCadencia: 0,
            pendentes: 1,
            percentualPendente: 50,
          },
          vendas: {
            clientesQueCompraram: 1,
            valorLiquido: 10_000,
            maquina: 0,
            peca: 6_000,
            servico: 4_000,
            outros: 0,
            posVenda: 10_000,
          },
        },
      ],
      enderecos: 40,
      enderecosComArea: 12,
      visao: 'Filial',
      estado: {
        ano: 2024,
        areaPlantadaHectares: 8_000_000,
        valorDaProducaoMilReais: 90_000_000,
        areaColhidaHectares: 7_900_000,
        tratores: { publicado: 120_000, somaDosMunicipios: 118_000 },
        estabelecimentos: { publicado: 180_000, somaDosMunicipios: 179_000 },
        anoDoCenso: 2017,
        rebanho: { publicado: 10_000_000, somaDosMunicipios: 9_900_000 },
        anoDoRebanho: 2024,
      },
      culturasNoEstado: [],
      potencialDoRecorte: {
        parqueDeMaquinas: 1_240,
        demandaAnualDeMaquinas: null,
        areaUtilHectares: 12_400,
        estimativa: true,
        motivoSemParque: 'Nenhum',
        motivoSemDemanda: 'SemCicloDeRenovacao',
        frase: 'estimativa: a regra do café ainda não foi confirmada',
        municipiosComParque: 1,
        porCultura: [],
        porCategoria: [],
        relevanciaNoEstado: {
          fatiaDaAreaPlantada: 0.47,
          fatiaDaAreaColhida: 0.46,
          fatiaDaQuantidade: null,
          fatiaDoValor: 0.52,
          produtividadeDoRecorte: null,
          produtividadeNoEstado: null,
          razaoDeProdutividade: null,
        },
        relevanciaPorCultura: [],
      },
    },
    metricasSemDado: [{ metrica: 'participacaoDeMercado', motivo: 'emplacamento não integrado' }],
    podeVerEmpresaInteira: false,
    classificacoes: [
      { indicador: 'potencial', situacao: 'Estimativa', selo: 'estimativa', motivo: 'regra a confirmar' },
    ],
  };
}

const MALHA = {
  type: 'FeatureCollection' as const,
  features: [
    {
      type: 'Feature' as const,
      properties: { codarea: String(CAFELANDIA), nome: 'Cafelândia' },
      geometry: {
        type: 'Polygon' as const,
        coordinates: [
          [
            [-48.0, -21.0],
            [-48.2, -21.0],
            [-48.2, -21.2],
            [-48.0, -21.2],
            [-48.0, -21.0],
          ],
        ],
      },
    },
    {
      type: 'Feature' as const,
      properties: { codarea: String(ARARAQUARA), nome: 'Araraquara' },
      geometry: {
        type: 'Polygon' as const,
        coordinates: [
          [
            [-48.3, -21.3],
            [-48.5, -21.3],
            [-48.5, -21.5],
            [-48.3, -21.5],
            [-48.3, -21.3],
          ],
        ],
      },
    },
  ],
};

/**
 * A ordem dos blocos como o DOM os entrega.
 *
 * `PainelDeIndicadores` e `MetricasSemDado` são componentes compartilhados com
 * outras telas, e por isso são reconhecidos pela classe da raiz deles em vez de
 * receberem um atributo só para este teste.
 */
function blocosNaOrdem(): string[] {
  const nós = document.querySelectorAll<HTMLElement>('[data-bloco], .cad-kpis, .cad-semdado');
  return [...nós].map((nó) => nó.dataset.bloco ?? (nó.classList.contains('cad-kpis') ? 'kpis' : 'metricas-sem-dado'));
}

const bloco = (nome: string) => document.querySelector<HTMLElement>(`[data-bloco="${nome}"]`);
const painelDaAba = () => document.querySelector<HTMLElement>('[role="tabpanel"]')!;

function abrir() {
  render(
    <ProvedorDeContextoDeAcesso>
      <IndicadoresGeograficos />
    </ProvedorDeContextoDeAcesso>,
  );
}

function responder() {
  obterIndicadoresTerritoriais.mockResolvedValue({
    dados: painel(),
    procedencia: {
      sistema: 'CRM Tracbel',
      objeto: 'territorio.Municipio',
      lidoEmUtc: '2026-09-23T12:00:00Z',
      dadoMaisRecenteEm: null,
    },
  });
  carregarMalhaDeSaoPaulo.mockResolvedValue(MALHA);
}

/** Espera o primeiro conteúdo que depende da resposta da API aparecer. */
const esperarACarga = () => screen.findByRole('tab', { name: 'Mercado' });

const irPara = (aba: 'Mercado' | 'Território') => fireEvent.click(screen.getByRole('tab', { name: aba }));

describe('Indicadores Geográficos — as duas abas', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  it('Mercado é a aba padrão e Território existe ao lado', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(screen.getByRole('tab', { name: 'Mercado' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Território' })).toHaveAttribute('aria-selected', 'false');
    expect(painelDaAba().dataset.aba).toBe('mercado');
  });

  it('os filtros ficam ACIMA das abas — é o que faz as duas serem o mesmo recorte', async () => {
    responder();
    abrir();
    await esperarACarga();

    const ordem = blocosNaOrdem();
    expect(ordem.indexOf('filtros')).toBeLessThan(ordem.indexOf('abas'));
    expect(ordem.indexOf('alcance')).toBeLessThan(ordem.indexOf('abas'));

    // E fora do painel da aba: se estivessem dentro, trocar de aba os trocaria.
    expect(painelDaAba().contains(bloco('filtros'))).toBe(false);
  });

  it('a aba Mercado monta os cinco blocos na ordem do documento 50', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(blocosNaOrdem()).toEqual([
      'cabecalho',
      'alcance',
      'filtros',
      'como-ler',
      'abas',
      'mercado-da-regiao',
      'kpis',
      'visao-geografica',
      'mapas',
      'metricas-sem-dado',
      'potencial-estrutural',
      'momento-do-mercado',
      'precos',
      'custos',
      'performance-tracbel',
      'kpis',
    ]);
  });

  it('os quatro mapas continuam juntos, na mesma grade, dentro de Mercado', async () => {
    responder();
    abrir();
    await esperarACarga();

    const grade = bloco('mapas')!;
    expect(bloco('visao-geografica')!.contains(grade)).toBe(true);
    expect(painelDaAba().contains(grade)).toBe(true);

    const mapas = [...grade.querySelectorAll<HTMLElement>('[data-mapa]')];
    expect(mapas.map((m) => m.dataset.mapa)).toEqual(['cobertura', 'vendas', 'potencial', 'estrutura']);

    // Filhos DIRETOS da grade: é o que o CSS transforma em 2×2. Aninhar um deles
    // quebraria o desenho sem quebrar nenhuma outra afirmação.
    for (const mapa of mapas) expect(mapa.parentElement).toBe(grade);
  });

  it('a tabela operacional está em Território, e não em Mercado', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(bloco('tabela-municipios')).toBeNull();

    irPara('Território');
    expect(painelDaAba().dataset.aba).toBe('territorio');
    expect(painelDaAba().contains(bloco('tabela-municipios'))).toBe(true);
    expect(bloco('mapas')).toBeNull();
  });

  it('o município escolhido sobrevive à troca de aba', async () => {
    responder();
    abrir();
    await esperarACarga();

    irPara('Território');
    fireEvent.click(await screen.findByRole('button', { name: 'Cafelândia' }));

    const chip = bloco('chip-municipio')!;
    expect(chip).toHaveTextContent('Cafelândia');
    // O chip vive ACIMA das abas: é do recorte, não da aba.
    expect(painelDaAba().contains(chip)).toBe(false);

    irPara('Mercado');
    expect(bloco('chip-municipio')).toHaveTextContent('Cafelândia');
    expect(screen.getByRole('heading', { name: 'Quem atende este município' })).toBeInTheDocument();

    // E o × devolve o recorte inteiro.
    fireEvent.click(screen.getByRole('button', { name: 'Tirar o recorte de Cafelândia' }));
    expect(bloco('chip-municipio')).toBeNull();
  });

  it('nada se perde ao trocar de aba: o que sai de uma está na outra', async () => {
    responder();
    abrir();
    await esperarACarga();

    const emMercado = new Set(blocosNaOrdem());
    irPara('Território');
    const emTerritorio = new Set(blocosNaOrdem());

    // Os blocos de fora das abas aparecem nas duas.
    for (const global of ['cabecalho', 'alcance', 'filtros', 'como-ler', 'abas']) {
      expect(emMercado.has(global)).toBe(true);
      expect(emTerritorio.has(global)).toBe(true);
    }

    // E cada bloco de conteúdo mora em exatamente uma aba — nenhum sumiu das duas.
    expect([...emMercado].filter((b) => b.startsWith('mapas') || b === 'visao-geografica')).toHaveLength(2);
    expect(emTerritorio.has('tabela-municipios')).toBe(true);
  });
});

describe('Indicadores Geográficos — ausência de dado é ausência de dado', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  /**
   * Uma aba interna que não tem fonte diz o que falta, no lugar do número.
   *
   * A busca é escopada ao conteúdo da aba: o rótulo do botão que abre a aba tem
   * o mesmo texto do nome da métrica, e sem o escopo a busca acharia os dois.
   */
  function esperarLacuna(metrica: string) {
    const conteudos = [...document.querySelectorAll<HTMLElement>('.terr-subaba-conteudo')];
    const lacuna = conteudos
      .flatMap((c) => [...c.querySelectorAll<HTMLElement>('.cad-lacuna')])
      .find((l) => l.textContent?.includes(metrica));

    expect(lacuna, `nenhuma lacuna para "${metrica}"`).toBeDefined();
    expect(within(lacuna!).getByText('sem dado')).toBeInTheDocument();
    return lacuna!;
  }

  it('termo de troca e percepção comercial dizem o que falta, sem número', async () => {
    responder();
    abrir();
    await esperarACarga();

    fireEvent.click(screen.getByRole('button', { name: 'Termo de troca' }));
    expect(esperarLacuna('Termo de troca')).toHaveTextContent(/issue 70/);

    fireEvent.click(screen.getByRole('button', { name: 'Percepção comercial' }));
    expect(esperarLacuna('Percepção comercial')).toHaveTextContent(/issue 71/);
  });

  it('captura e não capturado dizem que dependem das vendas em unidades', async () => {
    responder();
    abrir();
    await esperarACarga();

    fireEvent.click(screen.getByRole('button', { name: 'Captura' }));
    expect(esperarLacuna('Captura Tracbel')).toHaveTextContent(/issue 69/);

    fireEvent.click(screen.getByRole('button', { name: 'Não capturado' }));
    esperarLacuna('Potencial não capturado');
  });

  it('nenhum RÓTULO da tela usa "share" — o número se chama captura (issue 162)', async () => {
    responder();
    abrir();
    await esperarACarga();

    fireEvent.click(screen.getByRole('button', { name: 'Captura' }));
    expect(esperarLacuna('Captura Tracbel')).toHaveTextContent('Captura Tracbel');

    // O texto do motivo PODE dizer "não é market share" — é justamente ali que a
    // diferença se explica. O que não pode é um rótulo, uma aba ou um título
    // nomear o número assim.
    const rotulos = document.querySelectorAll<HTMLElement>(
      '.cad-lacuna-metrica, [role="tab"], .terr-alternador button, h1, h2, h3, .card-title',
    );
    const comShare = [...rotulos].filter((r) => /share/i.test(r.textContent ?? ''));
    expect(comShare.map((r) => r.textContent)).toEqual([]);
  });

  it('a demanda anual sai vazia com o motivo, e os cenários dizem que ainda não existem', async () => {
    responder();
    abrir();
    await esperarACarga();

    fireEvent.click(screen.getByRole('button', { name: 'Demanda anual' }));
    expect(esperarLacuna('Demanda anual')).toHaveTextContent(/falta o ciclo de renovação/);

    fireEvent.click(screen.getByRole('button', { name: 'Cenários' }));
    esperarLacuna('Cenários do potencial');
  });

  it('o parque, que TEM dado, aparece com o número — a lacuna não engoliu o que existe', async () => {
    responder();
    abrir();
    await esperarACarga();

    const potencial = bloco('potencial-estrutural')!;
    expect(potencial).toHaveTextContent('1.240');
    expect(potencial).toHaveTextContent('municípios com parque');
    expect(potencial.querySelector('.cad-lacuna')).toBeNull();
  });

  it('simular cenário é ação secundária do bloco de potencial, e abre a calculadora', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(bloco('calculadora')).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Simular cenário' }));

    const calculadora = bloco('calculadora')!;
    expect(bloco('potencial-estrutural')!.contains(calculadora)).toBe(true);
    // Ela não mora mais dentro do cartão do mapa de potencial.
    expect(document.querySelector('[data-mapa="potencial"]')!.contains(calculadora)).toBe(false);
  });
});
