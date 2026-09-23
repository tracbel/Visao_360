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
import { MemoryRouter } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { EspelhoDaUrl } from '../testes/EspelhoDaUrl';
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
      // O DENOMINADOR DA REGIÃO TRACBEL É A ADR INTEIRA, e não o recorte
      // consultado: 37.226 ha de Cafelândia sobre 886.333 ha da ADR dão 4,2%.
      regiaoTracbel: {
        anoDaLavoura: 2024,
        areaPlantadaHectares: 886_333,
        areaColhidaHectares: 870_000,
        valorDaProducaoMilReais: 11_160_000,
        anoDoCenso: 2017,
        tratores: 38_364,
        estabelecimentos: 25_300,
        anoDoRebanho: 2024,
        bovinos: 1_240_000,
        municipios: 203,
      },
      procedencias: {
        areaPlantada: {
          fonte: 'IBGE/SIDRA',
          pesquisa: 'PAM — Produção Agrícola Municipal',
          tabela: '5457',
          variavel: 'Área plantada',
          competencia: '2024',
          ultimaCargaUtc: '2026-09-22T03:00:00Z',
          ressalva: 'O município com produção sigilosa não entra na soma — e ausência não é zero.',
        },
        valorDaProducao: {
          fonte: 'IBGE/SIDRA',
          pesquisa: 'PAM — Produção Agrícola Municipal',
          tabela: '5457',
          variavel: 'Valor da produção',
          competencia: '2024',
          ultimaCargaUtc: '2026-09-22T03:00:00Z',
          ressalva: 'É o que o município COLHE, em mil reais.',
        },
        tratores: {
          fonte: 'IBGE/SIDRA',
          pesquisa: 'Censo Agropecuário',
          tabela: '6778',
          variavel: 'Tratores existentes',
          competencia: '2017',
          ultimaCargaUtc: '2026-09-22T03:00:00Z',
          ressalva: 'O Censo é de 2017 e o próximo sai em 2028: o parque tem essa idade.',
        },
        estabelecimentos: {
          fonte: 'IBGE/SIDRA',
          pesquisa: 'Censo Agropecuário',
          tabela: '6779',
          variavel: 'Estabelecimentos agropecuários',
          competencia: '2017',
          ultimaCargaUtc: '2026-09-22T03:00:00Z',
          ressalva: null,
        },
        rebanho: {
          fonte: 'IBGE/SIDRA',
          pesquisa: 'PPM — Pesquisa da Pecuária Municipal',
          tabela: '3939',
          variavel: 'Efetivo dos rebanhos — bovino',
          competencia: '2024',
          ultimaCargaUtc: '2026-09-22T03:00:00Z',
          ressalva: 'A PPM é anual e anda sozinha.',
        },
        usinas: null,
      },
      // O MOMENTO DO RECORTE COMO O DADO DE HOJE O PRODUZ (fase T3.1).
      //
      // O `potencialDoRecorte` acima não tem demanda nem cultura — o ciclo de
      // renovação é o D-P01 —, então o fator agregado sai AUSENTE, com o motivo.
      // Antes da T3.1 esta mesma amostra trazia fator 0,88 com demanda nula, o
      // que só era possível porque o fator vinha do índice de UMA cultura: a
      // razão entre as duas somas não existe sem denominador. A amostra com
      // composição está em `comMomento()`.
      momento: {
        fatorAgregado: null,
        motivoSemFator: 'SemDemandaEstrutural',
        demandaEstruturalTotal: null,
        demandaAjustadaTotal: null,
        porCultura: [],
        predominante: null,
        indiceDeCredito: 0.88,
        percepcaoPercentual: 2,
        porte: null,
        faixaDoMomento: null,
        leitura: '',
        procedencia: {
          fonte: 'CRM Tracbel',
          pesquisa: 'Fator de ciclo de mercado (issue 74), agregado pela demanda',
          tabela: null,
          variavel: 'Demanda ajustada total ÷ demanda estrutural total',
          competencia: 'preço até 06/2026',
          ultimaCargaUtc: '2026-09-23T12:00:00Z',
          ressalva: 'O fator é de CADA CULTURA. Indicador ausente vale desvio ZERO.',
        },
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
  const nós = document.querySelectorAll<HTMLElement>('[data-bloco], .cad-kpis');
  return [...nós].map((nó) => nó.dataset.bloco ?? 'kpis');
}

const bloco = (nome: string) => document.querySelector<HTMLElement>(`[data-bloco="${nome}"]`);

/**
 * O cartão de indicador inteiro, pelo rótulo.
 *
 * Escopado porque vários rótulos de KPI ("Usinas de etanol", "Rebanho bovino")
 * são também opções do alternador do mapa da estrutura.
 */
const cartaoDeKpi = (rotulo: string) =>
  [...document.querySelectorAll<HTMLElement>('.cad-kpi')].find((c) =>
    c.querySelector('.cad-kpi-rotulo')?.textContent?.startsWith(rotulo),
  );
const painelDaAba = () => document.querySelector<HTMLElement>('[role="tabpanel"]')!;


const urlAtual = () => document.querySelector<HTMLElement>('[data-url]')!.textContent ?? '';
const voltarNoHistorico = () => fireEvent.click(document.querySelector<HTMLElement>('[data-voltar]')!);

function abrir(entrada = '/cobertura') {
  render(
    <MemoryRouter initialEntries={[entrada]}>
      <ProvedorDeContextoDeAcesso>
        <IndicadoresGeograficos />
        <EspelhoDaUrl />
      </ProvedorDeContextoDeAcesso>
    </MemoryRouter>,
  );
}

/**
 * A MESMA TELA COM DEMANDA E COMPOSIÇÃO (fase T3.1).
 *
 * A ARITMÉTICA FECHA DE PROPÓSITO, e é ela que está sob prova: Café renova 300
 * máquinas por ano com fator 0,84 (→ 252) e Cana renova 100 com fator 1,00 (→
 * 100). Σ ajustada 352 ÷ Σ estrutural 400 = **0,88** — o número que aparece no
 * topo. Some a coluna da tela e dá isso.
 *
 * A CANA TEM A MAIOR ÁREA e o CAFÉ manda no fator, porque quem pesa é a demanda.
 * É exatamente o caso que a regra antiga errava: ela pegaria o índice de preço da
 * cana (1,30) e o recorte inteiro sairia aquecido.
 */
function comMomento(p: PainelTerritorial): PainelTerritorial {
  const café = {
    culturaCodigo: 'CAFE',
    cultura: 'Café (Total)',
    demandaEstrutural: 300,
    areaUtilHectares: 4_200,
    indiceDePreco: 0.8,
    fator: {
      fator: 0.84,
      parcelaDePreco: -0.08,
      parcelaDaPercepcao: 0.02,
      parcelaDeCredito: -0.06,
      fatorSemLimite: 0.84,
      cortadoPeloLimite: false,
      indicadoresUsados: 3,
      estimativa: true,
      motivo: 'Nenhum',
    },
    demandaAjustada: 252,
  };

  const cana = {
    culturaCodigo: 'CANA',
    cultura: 'Cana-de-açúcar',
    demandaEstrutural: 100,
    areaUtilHectares: 5_800,
    indiceDePreco: 1.3,
    fator: {
      fator: 1.0,
      parcelaDePreco: 0.04,
      parcelaDaPercepcao: 0.02,
      parcelaDeCredito: -0.06,
      fatorSemLimite: 1.0,
      cortadoPeloLimite: false,
      indicadoresUsados: 3,
      estimativa: true,
      motivo: 'Nenhum',
    },
    demandaAjustada: 100,
  };

  return {
    ...p,
    indicadores: {
      ...p.indicadores,
      potencialDoRecorte: { ...p.indicadores.potencialDoRecorte!, demandaAnualDeMaquinas: 400 },
      momento: {
        ...p.indicadores.momento!,
        fatorAgregado: 0.88,
        motivoSemFator: 'Nenhum',
        demandaEstruturalTotal: 400,
        demandaAjustadaTotal: 352,
        porCultura: [café, cana],
        predominante: {
          cultura: 'Cana-de-açúcar',
          fatia: 58,
          criterio: 'maior área útil entre as culturas com regra de potencial',
        },
        faixaDoMomento: 'Retraído',
        leitura: 'Mercado retraído.',
      },
    },
  };
}

function responder(ajustar: (p: PainelTerritorial) => PainelTerritorial = (p) => p) {
  obterIndicadoresTerritoriais.mockResolvedValue({
    dados: ajustar(painel()),
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
      // Os quatro números de decisão vêm primeiro (fase T3), depois porte ×
      // momento, e só então a linha preservada — que vira a EVIDÊNCIA do porte.
      'kpis-executivos',
      'kpis',
      'porte-e-momento',
      'kpis',
      'visao-geografica',
      'mapas',
      'limitacoes',
      'potencial-estrutural',
      'momento-do-mercado',
      // O BLOCO ABRE NA COMPOSIÇÃO (fase T3.1): é a conta do número que a tela
      // mostra lá em cima. Preços e custos continuam na aba Rentabilidade.
      'composicao-do-fator',
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
    // E a URL não muda ao trocar de aba: a aba não é do recorte.
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);

    // E o × devolve o recorte inteiro.
    fireEvent.click(screen.getByRole('button', { name: 'Tirar o recorte de Cafelândia' }));
    expect(bloco('chip-municipio')).toBeNull();
    expect(urlAtual()).toBe('');
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

describe('Indicadores Geográficos — o recorte mora na URL (issue 163)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  it('escolher no mapa ou na tabela escreve o mesmo parâmetro na URL', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(urlAtual()).toBe('');

    irPara('Território');
    fireEvent.click(await screen.findByRole('button', { name: 'Cafelândia' }));
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);
  });

  it('a URL restaura o município: recarregar a página não perde o recorte', async () => {
    responder();
    abrir(`/cobertura?municipio=${CAFELANDIA}`);
    await esperarACarga();

    expect(bloco('chip-municipio')).toHaveTextContent('Cafelândia');
    expect(await screen.findByRole('heading', { name: 'Quem atende este município' })).toBeInTheDocument();
  });

  it('código IBGE inválido não dá erro nem inventa município — a tela abre inteira', async () => {
    responder();
    abrir('/cobertura?municipio=banana');
    await esperarACarga();

    expect(bloco('chip-municipio')).toBeNull();
    expect(bloco('mapas')).not.toBeNull();
    expect(screen.queryByRole('heading', { name: 'Quem atende este município' })).not.toBeInTheDocument();
  });

  it('código que não existe no recorte também não inventa ficha', async () => {
    responder();
    abrir('/cobertura?municipio=9999999');
    await esperarACarga();

    expect(bloco('chip-municipio')).toBeNull();
    expect(bloco('mapas')).not.toBeNull();
  });

  it('cada escolha é uma entrada no histórico — voltar devolve o recorte anterior', async () => {
    responder();
    abrir();
    await esperarACarga();

    irPara('Território');
    fireEvent.click(await screen.findByRole('button', { name: 'Cafelândia' }));
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);

    fireEvent.click(screen.getByRole('button', { name: 'Tirar o recorte de Cafelândia' }));
    expect(urlAtual()).toBe('');

    voltarNoHistorico();
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);
    expect(bloco('chip-municipio')).toHaveTextContent('Cafelândia');
  });
});

describe('Indicadores Geográficos — somável × razão (documento 50, §7)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  it('grandeza somável mostra as DUAS fatias: Região Tracbel e SP', async () => {
    responder();
    abrir();
    await esperarACarga();

    // 422 tratores sobre 38.364 da ADR = 1,1%; sobre 120.000 publicados de SP = 0,4%.
    const tratores = cartaoDeKpi('Parque de tratores')!;
    expect(tratores).toHaveTextContent('1,1% da Região Tracbel');
    expect(tratores).toHaveTextContent('0,4% de SP');
  });

  it('a fatia diz "Região Tracbel", e nunca só "região" — Norte e Noroeste são sub-regiões', async () => {
    responder();
    abrir();
    await esperarACarga();

    const contexto = [...document.querySelectorAll<HTMLElement>('.cad-kpi-fonte')].map((n) => n.textContent ?? '');
    const comFatia = contexto.filter((t) => /% da /.test(t));
    expect(comFatia.length).toBeGreaterThan(0);
    for (const texto of comFatia) expect(texto).toMatch(/% da Região Tracbel/);
  });

  it('sem denominador, a fatia não aparece — e não vira 0%', async () => {
    const semRegiao = painel();
    semRegiao.indicadores.regiaoTracbel = null;
    semRegiao.indicadores.estado = null;
    obterIndicadoresTerritoriais.mockResolvedValue({ dados: semRegiao, procedencia: null });
    carregarMalhaDeSaoPaulo.mockResolvedValue(MALHA);
    abrir();
    await esperarACarga();

    const tratores = cartaoDeKpi('Parque de tratores')!;
    expect(tratores).not.toHaveTextContent('0% da Região Tracbel');
    expect(tratores).not.toHaveTextContent('0% de SP');
    expect(tratores).not.toHaveTextContent('% da Região Tracbel');
  });
});

describe('Indicadores Geográficos — procedência por indicador (issue 167)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  it('a procedência completa de um indicador do IBGE abre pelo teclado', async () => {
    responder();
    abrir();
    await esperarACarga();

    const gatilho = screen.getByRole('button', { name: 'De onde vem parque de tratores' });

    // Pelo TECLADO: o `title=` cru nunca abriu assim.
    fireEvent.focus(gatilho);
    const dica = await screen.findByRole('tooltip');

    expect(dica).toHaveTextContent('Fonte: IBGE/SIDRA');
    expect(dica).toHaveTextContent('Pesquisa: Censo Agropecuário');
    expect(dica).toHaveTextContent('Tabela: 6778');
    expect(dica).toHaveTextContent('Variável: Tratores existentes');
    expect(dica).toHaveTextContent('Competência: 2017');
    expect(dica).toHaveTextContent('Última carga: 22/09/2026');
    expect(dica).toHaveTextContent('o próximo sai em 2028');
  });

  it('fonte não carregada não ganha carimbo inventado', async () => {
    responder();
    abrir();
    await esperarACarga();

    // `usinas: null` no contrato — o cartão existe, a dica não. (O mesmo texto
    // também é um botão do alternador do mapa da estrutura, daí o escopo.)
    expect(cartaoDeKpi('Usinas de etanol')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'De onde vem usinas de etanol' })).not.toBeInTheDocument();
  });

  it('a tela não escreve fonte à mão: o texto embaixo do número não repete o carimbo', async () => {
    responder();
    abrir();
    await esperarACarga();

    const tratores = cartaoDeKpi('Parque de tratores')!;
    const deOnde = tratores.querySelector('.cad-kpi-fonte')!.textContent ?? '';
    expect(deOnde).not.toMatch(/Censo Agropecuário/);
    expect(deOnde).not.toMatch(/IBGE/);
  });
});

describe('Indicadores Geográficos — os quatro KPIs e o momento (fase T3)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  const executivo = (rotulo: string) =>
    [...bloco('kpis-executivos')!.querySelectorAll<HTMLElement>('.cad-kpi')].find((c) =>
      c.querySelector('.cad-kpi-rotulo')?.textContent?.startsWith(rotulo),
    )!;

  it('os quatro números de decisão estão lá, nesta ordem', async () => {
    responder();
    abrir();
    await esperarACarga();

    const rotulos = [...bloco('kpis-executivos')!.querySelectorAll('.cad-kpi-rotulo')].map((n) =>
      (n.textContent ?? '').trim(),
    );
    expect(rotulos).toEqual(['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade']);
  });

  it('os três sem dado dizem o que falta e a issue que destrava — nunca um número', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(executivo('Mercado anual')).toHaveTextContent(/issue 70/);
    expect(executivo('Captura Tracbel')).toHaveTextContent(/issue 69/);
    expect(executivo('Oportunidade')).toHaveTextContent(/issue 69/);

    // Nenhum deles inventou número.
    for (const rotulo of ['Mercado anual', 'Captura Tracbel', 'Oportunidade'])
      expect(executivo(rotulo).querySelector('.cad-kpi-valor')!.textContent).toMatch(/^—?$/);
  });

  it('a demanda anual sai vazia apontando a D-P01, e não zero', async () => {
    responder();
    abrir();
    await esperarACarga();

    const demanda = executivo('Demanda anual');
    expect(demanda).toHaveTextContent(/ciclo de renovação/);
    expect(demanda).toHaveTextContent(/issue 63/);
    expect(demanda.querySelector('.cad-kpi-valor')!.textContent).not.toContain('0 máq');
  });

  it('o porte nasce SEM NOME, e a dica diz que nulo não é "pequeno"', async () => {
    responder();
    abrir();
    await esperarACarga();

    const porte = bloco('porte-e-momento')!;
    expect(porte).toHaveTextContent('Porte:');
    for (const nome of ['Mercado pequeno', 'Mercado médio', 'Mercado grande'])
      expect(porte).not.toHaveTextContent(nome);

    fireEvent.focus(within(porte).getByRole('button', { name: 'Por que o nome do porte não aparece' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent(/issue 166/);
    expect(dica).toHaveTextContent(/NÃO quer dizer "pequeno"/);
  });

  it('sem demanda estrutural o momento sai AUSENTE com o motivo — e nunca 1,00', async () => {
    // A amostra padrão é o dado de hoje: potencial sem ciclo de renovação. Sem
    // denominador não há razão, e "não há base para dizer" não é "está neutro".
    responder();
    abrir();
    await esperarACarga();

    const porte = bloco('porte-e-momento')!;
    expect(porte).toHaveTextContent('Momento:');
    expect(porte).not.toHaveTextContent('1,00');
    expect(porte).not.toHaveTextContent('Retraído');

    fireEvent.focus(within(porte).getByRole('button', { name: 'Por que o momento do mercado não aparece' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent(/ciclo de renovação/);
    expect(dica).toHaveTextContent(/issue 63/);
    expect(dica).toHaveTextContent(/diferente de "o mercado está neutro"/);
  });

  it('o momento TEM nome e número quando há demanda — ele já tem faixas decididas', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const porte = bloco('porte-e-momento')!;
    expect(porte).toHaveTextContent('Retraído');
    expect(porte).toHaveTextContent('0,88');
    expect(porte).toHaveTextContent('Mercado retraído.');
  });

  it('o resumo executivo NÃO tem as três setas agregadas (fase T3.1)', async () => {
    // O QUE ESTE TESTE IMPEDE: que alguém volte a desenhar Rentabilidade,
    // Crédito e Percepção no topo. Elas são calculadas por CULTURA; agregá-las
    // exigiria decompor o quociente em três pedaços que o domínio não produz.
    responder(comMomento);
    abrir();
    await esperarACarga();

    const porte = bloco('porte-e-momento')!;
    expect(porte.querySelectorAll('.terr-parcela-seta')).toHaveLength(0);
    expect(porte.querySelectorAll('.terr-parcela-nome')).toHaveLength(0);

    // E o ⓘ do topo diz onde a composição está, em vez de a conta sumir.
    fireEvent.focus(within(porte).getByRole('button', { name: 'Como o momento do mercado é composto' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent(/Composição do fator/);
    expect(dica).toHaveTextContent(/POR CULTURA/);
  });

  it('a composição mostra uma linha por cultura, cada uma com o índice DELA', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const composicao = bloco('composicao-do-fator')!;
    const linhas = [...composicao.querySelectorAll<HTMLElement>('.terr-composicao-linha')];
    expect(linhas.map((l) => l.dataset.cultura)).toEqual(['CAFE', 'CANA']);

    // Índices diferentes na mesma tela — o que a regra antiga escondia atrás de um só.
    expect(linhas[0]).toHaveTextContent('0,80');
    expect(linhas[0]).toHaveTextContent('0,84');
    expect(linhas[1]).toHaveTextContent('1,30');
    // A demanda do café é o que manda no agregado: 300 das 400 máquinas/ano.
    expect(linhas[0]).toHaveTextContent('300 → 252');
    expect(linhas[1]).toHaveTextContent('100 → 100');
  });

  it('as parcelas são TRÊS por cultura, e o custo não é uma quarta seta', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const café = bloco('composicao-do-fator')!.querySelector<HTMLElement>('[data-cultura="CAFE"]')!;
    const parcelas = [...café.querySelectorAll('.terr-parcela-nome')].map((n) => n.textContent ?? '');
    expect(parcelas).toEqual(['Commodity', 'Crédito', 'Percepção']);
    expect(parcelas).not.toContain('Custo');
  });

  it('a direção da seta carrega o significado, e a dica diz que o índice é DESTA cultura', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const composicao = bloco('composicao-do-fator')!;
    const café = composicao.querySelector<HTMLElement>('[data-cultura="CAFE"]')!;
    const cana = composicao.querySelector<HTMLElement>('[data-cultura="CANA"]')!;

    // Café: preço −0,08 → baixa; crédito −0,06 → baixa; percepção +0,02 → sobe.
    expect([...café.querySelectorAll('.terr-parcela-seta')].map((n) => n.textContent)).toEqual(['↓', '↓', '↑']);
    // Cana: o preço dela SOBE. Duas culturas, dois sentidos, na mesma tela.
    expect([...cana.querySelectorAll('.terr-parcela-seta')].map((n) => n.textContent)).toEqual(['↑', '↓', '↑']);

    fireEvent.focus(
      within(café).getByRole('button', { name: 'Como commodity entra no fator de Café (Total)' }),
    );
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent(/O custo entra AQUI/);
    expect(dica).toHaveTextContent(/o índice é DESTA cultura/);
  });

  it('o total fecha a conta que o topo mostra: 352 ÷ 400 = 0,88', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const total = bloco('composicao-do-fator')!.querySelector('.terr-composicao-total')!;
    expect(total).toHaveTextContent('352');
    expect(total).toHaveTextContent('400');
    expect(total).toHaveTextContent('0,88');
    expect(bloco('porte-e-momento')!).toHaveTextContent('0,88');
  });

  it('a principal cultura é contexto, e a dica diz que a área não entra no cálculo', async () => {
    // A CANA TEM A MAIOR ÁREA e o fator é 0,88, puxado pelo café: se a área
    // mandasse, o índice 1,30 da cana deixaria o recorte aquecido.
    responder(comMomento);
    abrir();
    await esperarACarga();

    const contexto = bloco('porte-e-momento')!.querySelector<HTMLElement>('[data-contexto="predominante"]')!;
    expect(contexto).toHaveTextContent('Cana-de-açúcar');
    expect(contexto).toHaveTextContent('58%');

    fireEvent.focus(within(contexto).getByRole('button', { name: 'Qual é o critério da principal cultura' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent(/maior área útil entre as culturas com regra de potencial/);
    expect(dica).toHaveTextContent(/não muda o número acima/);
  });

  it('o fator explica a própria origem, com a competência do preço', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    fireEvent.focus(
      within(bloco('porte-e-momento')!).getByRole('button', { name: 'De onde vem o momento do mercado' }),
    );
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('Fonte: CRM Tracbel');
    expect(dica).toHaveTextContent('preço até 06/2026');
    expect(dica).toHaveTextContent(/desvio ZERO/);
  });
});

describe('Indicadores Geográficos — densidade da primeira camada (issues 31 e 33)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  /** Quanto texto permanente um bloco despeja na tela, sem contar o que está recolhido. */
  function textoPermanente(no: HTMLElement): string {
    const copia = no.cloneNode(true) as HTMLElement;
    // O que está dentro de um `<details>` fechado e o que só existe com a dica
    // aberta não ocupa a tela — e por isso não conta aqui.
    copia.querySelectorAll('details, [role="tooltip"]').forEach((d) => d.remove());
    return (copia.textContent ?? '').replace(/\s+/g, ' ').trim();
  }

  it('"Como interpretar os indicadores" não é mais um card permanente', async () => {
    responder();
    abrir();
    await esperarACarga();

    const comoLer = bloco('como-ler')!;
    expect(comoLer.tagName).toBe('DETAILS');
    expect(comoLer.classList.contains('card')).toBe(false);

    // Fechado, ele ocupa só o rótulo — e o conteúdo continua lá dentro.
    expect(comoLer.querySelector('summary')!.textContent).toBe('Como interpretar os indicadores');
    expect(comoLer.hasAttribute('open')).toBe(false);
    expect(comoLer.textContent).toContain('regra a confirmar');
  });

  it('"Limitações dos dados" é recolhível, e o conteúdo de auditoria não se perde', async () => {
    responder();
    abrir();
    await esperarACarga();

    const limitacoes = bloco('limitacoes')!;
    expect(limitacoes.tagName).toBe('DETAILS');
    expect(limitacoes.hasAttribute('open')).toBe(false);
    expect(limitacoes.querySelector('summary')!.textContent).toBe('Limitações dos dados');
    expect(limitacoes.textContent).toContain('participacaoDeMercado');
    expect(limitacoes.textContent).toContain('emplacamento não integrado');
  });

  it('nenhum dos quatro mapas despeja parágrafo metodológico na tela', async () => {
    responder();
    abrir();
    await esperarACarga();

    const mapas = [...bloco('mapas')!.querySelectorAll<HTMLElement>('[data-mapa]')];
    expect(mapas).toHaveLength(4);

    for (const mapa of mapas) {
      // O parágrafo de aviso sumiu do corpo de todos eles.
      expect(mapa.querySelector('.terr-aviso')).toBeNull();

      // E a metodologia está a um Tab de distância, não na cara de quem lê.
      const gatilho = within(mapa).getByRole('button', { name: 'Fonte e método deste mapa' });
      fireEvent.focus(gatilho);
      const dica = screen.getByRole('tooltip');
      expect(dica.textContent!.length).toBeGreaterThan(80);
      expect(dica.textContent).toMatch(/Fonte:|Fontes:/);
      fireEvent.blur(gatilho);
    }
  });

  it('o mapa de potencial perdeu o texto sobre ciclo, concorrência e cliente × não cliente', async () => {
    responder();
    abrir();
    await esperarACarga();

    const potencial = document.querySelector<HTMLElement>('[data-mapa="potencial"]')!;
    const corpo = textoPermanente(potencial);

    for (const sumiu of ['ciclo de troca', 'concorrência', 'não clientes', 'contaria a mesma área duas vezes']) {
      expect(corpo, `"${sumiu}" ainda ocupa a primeira camada`).not.toContain(sumiu);
    }

    // Mas o resumo continua dizendo a regra, que é executiva e cabe numa linha.
    expect(corpo).toContain('3036N');
    expect(corpo).toContain('estimativa');

    fireEvent.focus(within(potencial).getByRole('button', { name: 'Fonte e método deste mapa' }));
    const dica = screen.getByRole('tooltip').textContent!;
    for (const preservado of ['ciclo de troca', 'concorrência', 'não clientes', 'contaria a mesma área duas vezes']) {
      expect(dica, `"${preservado}" se perdeu`).toContain(preservado);
    }
  });

  it('o mapa da estrutura perdeu o parágrafo de Censo, sigilo e ANP — e ele está na dica', async () => {
    responder();
    abrir();
    await esperarACarga();

    const estrutura = document.querySelector<HTMLElement>('[data-mapa="estrutura"]')!;
    const corpo = textoPermanente(estrutura);

    for (const sumiu of ['próximo sai em 2028', 'faixas de potência', 'só faz açúcar']) {
      expect(corpo, `"${sumiu}" ainda ocupa a primeira camada`).not.toContain(sumiu);
    }
    expect(corpo).toContain('422 tratores');

    fireEvent.focus(within(estrutura).getByRole('button', { name: 'Fonte e método deste mapa' }));
    const dica = screen.getByRole('tooltip').textContent!;
    for (const preservado of ['próximo sai em 2028', 'faixas de potência', 'só faz açúcar', 'SIGILO']) {
      expect(dica, `"${preservado}" se perdeu`).toContain(preservado);
    }
  });

  it('nenhum `title=` cru voltou para a tela', async () => {
    responder();
    abrir();
    await esperarACarga();

    const comTitle = [...document.querySelectorAll('[title]')].map((n) => n.tagName + ': ' + n.getAttribute('title'));
    expect(comTitle).toEqual([]);
  });

  it('os subtítulos das seções são de uma frase — nenhum vira parágrafo', async () => {
    responder();
    abrir();
    await esperarACarga();

    const subtitulos = [...document.querySelectorAll<HTMLElement>('.terr-secao-subtitulo')];
    expect(subtitulos.length).toBeGreaterThan(0);

    for (const s of subtitulos) {
      const texto = (s.textContent ?? '').trim();
      expect(texto.length, `subtítulo longo: "${texto}"`).toBeLessThanOrEqual(90);
      // Uma frase: no máximo um ponto final, e ele no fim.
      expect(texto.split('.').filter(Boolean).length, `mais de uma frase: "${texto}"`).toBe(1);
    }
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
      // O CONTRATO MUDOU NA T2.1: onde o espaço é de MÉTRICA, a ausência é
      // compacta — rótulo, traço e dica —, e não um cartão de texto.
      .flatMap((c) => [...c.querySelectorAll<HTMLElement>('.cad-metrica-ausente')])
      .find((l) => l.textContent?.includes(metrica));

    expect(lacuna, `nenhuma lacuna compacta para "${metrica}"`).toBeDefined();
    expect(within(lacuna!).getByText('sem dado')).toBeInTheDocument();

    // O motivo NÃO ocupa espaço permanente: ele só existe quando a dica abre.
    fireEvent.focus(within(lacuna!).getByRole('button'));
    return screen.getByRole('tooltip');
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
    esperarLacuna('Captura Tracbel');

    // O texto do motivo PODE dizer "não é market share" — é justamente ali que a
    // diferença se explica. O que não pode é um rótulo, uma aba ou um título
    // nomear o número assim.
    const rotulos = document.querySelectorAll<HTMLElement>(
      '.cad-metrica-ausente-rotulo, .cad-kpi-rotulo, [role="tab"], .terr-alternador button, h1, h2, h3, .card-title',
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
