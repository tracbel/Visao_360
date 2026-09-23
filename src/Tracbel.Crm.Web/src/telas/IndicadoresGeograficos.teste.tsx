/**
 * A REDE DA COMPONENTIZAÇÃO (issue 170, parte A — fase T0 do documento 50).
 *
 * Este teste não julga o desenho: ele afirma que a tela continua **montando os
 * mesmos blocos, na mesma ordem**. É o que separa "quebrei o arquivo em
 * componentes" de "mexi na tela sem querer".
 *
 * POR QUE `data-bloco` E NÃO O TEXTO: texto muda de redação a cada issue, e um
 * teste que quebra quando alguém corrige uma vírgula é um teste que as pessoas
 * aprendem a ignorar. O atributo é invisível — nenhuma regra de CSS o usa —, e
 * atravessa a reorganização das fases T1 a T5 continuando a dizer a mesma coisa.
 *
 * OS TRÊS PAINÉIS DO FIM E A CALCULADORA ENTRAM COMO DUBLÊ: cada um tem a
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
            vinculosComCadencia: 1,
            cobertos: 0,
            foraDaCadencia: 1,
            nuncaContatados: 0,
            semCadencia: 0,
            pendentes: 1,
            percentualPendente: 100,
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

function abrir() {
  render(
    <ProvedorDeContextoDeAcesso>
      <IndicadoresGeograficos />
    </ProvedorDeContextoDeAcesso>,
  );
}

describe('Indicadores Geográficos — a estrutura da tela', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

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

  it('monta os blocos na ordem acordada, do cabeçalho aos painéis do fim', async () => {
    responder();
    abrir();

    // A tabela é o último bloco que depende da resposta da API; esperá-la
    // garante que a tela terminou de montar antes da conferência da ordem.
    expect(await screen.findByText('Municípios da ADR e o que ficou fora do mapa')).toBeInTheDocument();

    expect(blocosNaOrdem()).toEqual([
      'cabecalho',
      'alcance',
      'filtros',
      'como-ler',
      'kpis',
      'mercado-da-regiao',
      'kpis',
      'mapas',
      'metricas-sem-dado',
      'tabela-municipios',
      'precos',
      'custos',
      'credito',
    ]);
  });

  it('os quatro mapas ficam juntos, lado a lado, na mesma grade e nesta ordem', async () => {
    responder();
    abrir();

    const grade = await screen.findByText('Cobertura de carteira', { exact: false }).then(() => document.querySelector<HTMLElement>('[data-bloco="mapas"]'));
    expect(grade).not.toBeNull();

    const mapas = [...grade!.querySelectorAll<HTMLElement>('[data-mapa]')].map((m) => m.dataset.mapa);
    expect(mapas).toEqual(['cobertura', 'vendas', 'potencial', 'estrutura']);

    // Os quatro são filhos DIRETOS da grade: é o que o CSS transforma em 2x2, e
    // o que a issue 76 manda preservar. Aninhar um deles quebraria o desenho sem
    // quebrar nenhuma outra afirmação.
    for (const mapa of grade!.querySelectorAll<HTMLElement>('[data-mapa]')) {
      expect(mapa.parentElement).toBe(grade);
    }
  });

  it('a seção "O mercado da região" continua entre as duas linhas de indicadores', async () => {
    responder();
    abrir();

    await screen.findByText('Municípios da ADR e o que ficou fora do mapa');

    const secao = document.querySelector<HTMLElement>('[data-bloco="mercado-da-regiao"]');
    expect(within(secao!).getByRole('heading', { name: 'O mercado da região' })).toBeInTheDocument();

    const ordem = blocosNaOrdem();
    const posicao = ordem.indexOf('mercado-da-regiao');
    expect(ordem[posicao - 1]).toBe('kpis');
    expect(ordem[posicao + 1]).toBe('kpis');
  });

  it('clicar num município da tabela abre a ficha dele', async () => {
    responder();
    abrir();

    const linha = await screen.findByRole('button', { name: 'Cafelândia' });
    fireEvent.click(linha);

    expect(await screen.findByRole('heading', { name: 'Quem atende este município' })).toBeInTheDocument();
  });
});
