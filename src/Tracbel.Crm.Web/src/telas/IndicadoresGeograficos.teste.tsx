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

import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
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
vi.mock('../componentes/territorio/PainelDoPrecoImplicito', () => ({
  PainelDoPrecoImplicito: () => <div data-bloco="preco-implicito" />,
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
    // O estado de hoje: os três sem dado, com o motivo vindo da API (issue 69, parte A).
    numerosDeDecisao: {
      demandaAnual: { valor: null, motivo: 'SemDemandaAnual', frase: 'Falta o ciclo de renovação (D-P01, issue 63).' },
      mercadoAnual: {
        valor: null,
        motivo: 'SemPrecoDeMaquina',
        frase: 'Não há preço de referência de máquina no CRM (issue 70).',
        parcial: false,
        categoriasSemPreco: [],
      },
      capturaPercentual: {
        valor: null,
        motivo: 'SemVendasEmUnidades',
        frase: 'As vendas da Tracbel em MÁQUINAS não estão carregadas (issue 69).',
      },
      oportunidade: {
        valor: null,
        motivo: 'SemVendasEmUnidades',
        frase: 'As vendas da Tracbel em MÁQUINAS não estão carregadas (issue 69).',
      },
    },
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
 * `cartaoDeKpi` saiu na T4.6: os cinco indicadores estruturais deixaram de ser
 * cartões `.cad-kpi` e viraram itens de faixa, achados por `[data-faixa]`; os
 * quatro de decisão viraram `[data-kpi]` dentro de `kpis-executivos`. Procurar
 * por classe também era frágil — "Usinas de etanol" e "Rebanho bovino" são
 * igualmente rótulos do alternador do mapa da estrutura.
 */
const painelDaAba = () => document.querySelector<HTMLElement>('[role="tabpanel"]')!;


const urlAtual = () => document.querySelector<HTMLElement>('[data-url]')!.textContent ?? '';
const voltarNoHistorico = () => fireEvent.click(document.querySelector<HTMLElement>('[data-voltar]')!);

/**
 * O CAMPO DE MUNICÍPIO DOS FILTROS (fidelidade às maquetes, 23/09/2026).
 *
 * Ele substituiu o chip "Cafelândia ×": o recorte escolhido se lê no valor do
 * campo, e tirar o recorte é voltar para "Todos os municípios".
 */
const campoDeMunicipio = () => screen.getByRole('combobox', { name: 'Município' }) as HTMLSelectElement;
const escolherNoCampo = (codigo: number | '') =>
  fireEvent.change(campoDeMunicipio(), { target: { value: String(codigo) } });

/**
 * Abre uma dica pelo teclado, lê o TEXTO dela e FECHA — ver `comparacaoDe`,
 * mais abaixo, para o porquê de fechar antes de sair.
 */
function textoDaDica(rotulo: string, dentroDe: HTMLElement = document.body): string {
  const gatilho = within(dentroDe).getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

/**
 * A ficha do município, que desde 23/09/2026 mora só na aba Território.
 *
 * ERA O TÍTULO "Quem atende este município"; desde a fase 4 a ficha é em abas e
 * esse título mora na aba Estrutura, escondido enquanto a Visão geral está
 * aberta. A ficha se reconhece pelo bloco dela — e o nome do município é o
 * título dela.
 */
const fichaNaTela = () => document.querySelector<HTMLElement>('[data-bloco="ficha-do-municipio"]');

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

    // E fora do painel da aba: se estivessem dentro, trocar de aba os trocaria.
    expect(painelDaAba().contains(bloco('filtros'))).toBe(false);
  });

  it('a aba Mercado monta os blocos na ordem da maquete de rentabilidade e crédito', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(blocosNaOrdem()).toEqual([
      'cabecalho',
      // O ALCANCE ("Visão: filial … e as abaixo dela") e o "Como interpretar os
      // indicadores" SAÍRAM DO CORPO (fidelidade às maquetes, 23/09/2026): a
      // maquete não tem nenhum dos dois, e o texto deles foi para as dicas — ver
      // o bloco "o que saiu do corpo foi para as dicas", mais abaixo.
      'filtros',
      // O MUNICÍPIO É UM CAMPO DE ESCOLHA DE VERDADE (maquete), e não mais o chip.
      'municipio',
      // O BOTÃO "Mais filtros" (T4.6): os secundários saíram da primeira dobra
      // e foram para um popover, com um selo dizendo quantos estão ativos.
      'mais-filtros',
      'abas',
      // O CONTROLE DE COMPARAÇÃO COM O PERÍODO ANTERIOR (T4.9 — maquete),
      // DESLIGADO: a leitura devolve uma janela de competência, não duas. Desde
      // 23/09/2026 ele fica na LINHA DAS ABAS, à direita, como na maquete de
      // rentabilidade e crédito — e por isso vem antes da primeira seção.
      'comparar-periodo',
      // "O mercado da região" continua sendo a primeira seção, com o título só
      // para o leitor de tela: os quatro cartões vêm logo abaixo das abas.
      'mercado-da-regiao',
      'kpis-executivos',
      // O MOMENTO EM LARGURA INTEIRA, logo depois dos quatro números (decisão do
      // usuário de 23/09/2026). Ele era um terço da linha de baixo, e o Crédito
      // crescia para baixo o que não tinha de largura.
      'momento-do-mercado',
      // O BLOCO ABRE NA COMPOSIÇÃO (fase T3.1): é a conta do número que a tela
      // mostra lá em cima. Preços e custos continuam na aba Rentabilidade.
      'composicao-do-fator',
      // A RÉGUA DO MERCADO (T4.8): momento, porte e os cinco indicadores
      // estruturais numa faixa só. Eram dois blocos brancos empilhados, e são a
      // mesma leitura — como está o mercado, e o que existe nele.
      'faixa-do-mercado',
      'porte-e-momento',
      'visao-geografica',
      'mapas',
      // "Limitações dos dados" saiu de baixo dos mapas: está na dica do título
      // "Visão geográfica".
      'potencial-estrutural',
      // A PRIMEIRA CAMADA DO POTENCIAL são números grandes (T4.6); a
      // decomposição inteira — categoria, cultura e relevância — continua na
      // tela, num `<details>` recolhido. Nenhuma linha foi apagada.
      'potencial-do-recorte',
      'potencial-detalhe',
      'performance-tracbel',
    ]);
  });

  it('a linha final tem Potencial e Performance, lado a lado — e o Momento fora dela', async () => {
    responder();
    abrir();
    await esperarACarga();

    const final = document.querySelector<HTMLElement>('.dash-linha-final')!;
    expect([...final.children].map((n) => (n as HTMLElement).dataset.bloco)).toEqual([
      'potencial-estrutural',
      'performance-tracbel',
    ]);
    expect(final.contains(bloco('momento-do-mercado'))).toBe(false);
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

    // O CAMPO DE MUNICÍPIO mostra o recorte — e ele vive ACIMA das abas: é do
    // recorte, não da aba.
    expect(campoDeMunicipio().value).toBe(String(CAFELANDIA));
    expect(painelDaAba().contains(campoDeMunicipio())).toBe(false);
    expect(fichaNaTela()).toBeInTheDocument();

    irPara('Mercado');
    expect(campoDeMunicipio().value).toBe(String(CAFELANDIA));
    // A FICHA MORA SÓ EM TERRITÓRIO (fidelidade às maquetes, 23/09/2026): em
    // Mercado o município continua escolhido — os mapas o destacam e os painéis
    // reagem a ele —, mas a ficha não aparece embaixo dos mapas.
    expect(fichaNaTela()).not.toBeInTheDocument();
    // E a URL não muda ao trocar de aba: a aba não é do recorte.
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);

    irPara('Território');
    expect(fichaNaTela()).toBeInTheDocument();

    // E "Todos os municípios" devolve o recorte inteiro — o que o × do chip fazia.
    escolherNoCampo('');
    expect(campoDeMunicipio().value).toBe('');
    expect(fichaNaTela()).not.toBeInTheDocument();
    expect(urlAtual()).toBe('');
  });

  it('clicar num município no mapa escolhe o município e leva à ficha, em Território', async () => {
    responder();
    abrir();
    await esperarACarga();

    // O mapa é ponteiro: o polígono é um `path` com o nome no `<title>` dele.
    const mapa = await waitFor(() => {
      const achado = document.querySelector<HTMLElement>('[data-mapa="cobertura"]');
      expect(achado).not.toBeNull();
      return achado!;
    });
    const poligono = [...mapa.querySelectorAll<SVGPathElement>('path.terr-poligono')].find((p) =>
      (p.textContent ?? '').startsWith('Cafelândia'),
    )!;
    fireEvent.click(poligono);

    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);
    expect(screen.getByRole('tab', { name: 'Território' })).toHaveAttribute('aria-selected', 'true');
    expect(painelDaAba().contains(bloco('ficha-do-municipio'))).toBe(true);
    // O FOCO VAI PARA A ABA QUE ABRIU: o leitor de tela anuncia onde a pessoa
    // foi parar, e o Tab seguinte continua dali.
    expect(screen.getByRole('tab', { name: 'Território' })).toHaveFocus();
  });

  it('escolher no campo de município NÃO troca de aba — o campo é recorte, e vale para as duas', async () => {
    responder();
    abrir();
    await esperarACarga();

    // As opções são "Todos os municípios" e os da ADR, em ordem alfabética:
    // Araraquara está fora da ADR nesta amostra e não é opção.
    expect([...campoDeMunicipio().options].map((o) => o.textContent)).toEqual(['Todos os municípios', 'Cafelândia']);

    escolherNoCampo(CAFELANDIA);
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);
    expect(screen.getByRole('tab', { name: 'Mercado' })).toHaveAttribute('aria-selected', 'true');
    expect(bloco('mapas')).not.toBeNull();
  });

  it('nada se perde ao trocar de aba: o que sai de uma está na outra', async () => {
    responder();
    abrir();
    await esperarACarga();

    const emMercado = new Set(blocosNaOrdem());
    irPara('Território');
    const emTerritorio = new Set(blocosNaOrdem());

    // Os blocos de fora das abas aparecem nas duas.
    for (const global of ['cabecalho', 'filtros', 'municipio', 'abas']) {
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

    // O NOME DO MUNICÍPIO É O TÍTULO DA FICHA (fase 4, maquete).
    expect(await screen.findByRole('heading', { level: 2, name: 'Cafelândia' })).toBeInTheDocument();
    expect(fichaNaTela()).toBeInTheDocument();
    expect(campoDeMunicipio().value).toBe(String(CAFELANDIA));
    // A TELA ABRE ONDE A FICHA MORA (fidelidade às maquetes, 23/09/2026): um
    // endereço com município é o de quem estava olhando uma ficha.
    expect(painelDaAba().dataset.aba).toBe('territorio');
  });

  it('código IBGE inválido não dá erro nem inventa município — a tela abre inteira', async () => {
    responder();
    abrir('/cobertura?municipio=banana');
    await esperarACarga();

    expect(campoDeMunicipio().value).toBe('');
    expect(bloco('mapas')).not.toBeNull();
    expect(fichaNaTela()).not.toBeInTheDocument();
  });

  it('código que não existe no recorte também não inventa ficha', async () => {
    responder();
    abrir('/cobertura?municipio=9999999');
    await esperarACarga();

    // O CÓDIGO É UM INTEIRO VÁLIDO, então a tela abre em Território, onde uma
    // ficha moraria — e ela não aparece, porque o município não existe no
    // recorte. O campo diz "Todos os municípios", e a tabela está lá, inteira.
    expect(campoDeMunicipio().value).toBe('');
    expect(await screen.findByRole('button', { name: 'Cafelândia' })).toBeInTheDocument();
    expect(bloco('ficha-do-municipio')).toBeNull();
  });

  it('cada escolha é uma entrada no histórico — voltar devolve o recorte anterior', async () => {
    responder();
    abrir();
    await esperarACarga();

    irPara('Território');
    fireEvent.click(await screen.findByRole('button', { name: 'Cafelândia' }));
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);

    escolherNoCampo('');
    expect(urlAtual()).toBe('');

    voltarNoHistorico();
    expect(urlAtual()).toBe(`?municipio=${CAFELANDIA}`);
    expect(campoDeMunicipio().value).toBe(String(CAFELANDIA));
  });
});

/**
 * A ABA TERRITÓRIO COMO A MAQUETE `territorio-ficha.png` (fase 4, 23/09/2026).
 *
 * O desenho não se prova aqui — ele está nas capturas. O que se prova é o que a
 * troca de desenho não podia mudar: os mesmos números nos quatro cartões, a
 * ressalva que era vermelho indo para a dica, o período que não se inventa e a
 * conferência que saiu das linhas da tabela para a dica do título.
 */
describe('Indicadores Geográficos — a aba Território da maquete', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  const cartao = (id: string) => document.querySelector<HTMLElement>(`[data-kpi-carteira="${id}"]`)!;

  it('os quatro cartões da carteira, na ordem da maquete, com os números do recorte', async () => {
    responder();
    abrir();
    await esperarACarga();
    irPara('Território');

    const ids = [...document.querySelectorAll<HTMLElement>('[data-kpi-carteira]')].map((c) => c.dataset.kpiCarteira);
    expect(ids).toEqual(['municipios', 'cobertura', 'vendas', 'parque']);

    expect(cartao('municipios')).toHaveTextContent('Municípios da ADR');
    expect(cartao('municipios')).toHaveTextContent('12 clientes com endereço neles');
    // 11 de 18 vínculos no prazo = 61,1%; um município com visita.
    expect(cartao('cobertura')).toHaveTextContent('61,1%');
    expect(cartao('cobertura')).toHaveTextContent('1 municípios com visita');
    expect(within(cartao('cobertura')).getByRole('progressbar')).toHaveAttribute('aria-valuenow', '61');
    expect(cartao('vendas')).toHaveTextContent('R$ 1,3 mi');
    expect(cartao('vendas')).toHaveTextContent('em 1 municípios');
    expect(cartao('parque')).toHaveTextContent('1.240');
    expect(cartao('parque')).toHaveTextContent('estimativa na área de atuação');

    // O PAINEL DO CADASTRO, com o valor vermelho, não está mais aqui.
    expect(painelDaAba().querySelector('.cad-kpis, .cad-kpi-atencao')).toBeNull();
  });

  it('o texto longo de cada cartão foi para a dica dele — "regra provisória" inclusive', async () => {
    responder();
    abrir();
    await esperarACarga();
    irPara('Território');

    expect(cartao('cobertura')).not.toHaveTextContent('regra provisória');
    const dica = textoDaDica('De onde vem cobertura pela cadência', cartao('cobertura'));
    expect(dica).toContain('11 de 18 vínculos elegíveis no prazo');
    expect(dica).toContain('regra provisória');

    expect(textoDaDica('De onde vem vendas no período', cartao('vendas'))).toContain('composição provisória');
    expect(textoDaDica('De onde vem parque teórico de máquinas', cartao('parque'))).toContain('12.400 ha úteis');
  });

  it('a variação "vs. ano anterior" não inventa número: traço e a issue que destrava', async () => {
    responder();
    abrir();
    await esperarACarga();
    irPara('Território');

    for (const id of ['municipios', 'cobertura', 'vendas', 'parque']) {
      const pilula = cartao(id).querySelector<HTMLElement>('.terr-cart-variacao')!;
      expect(pilula).toHaveTextContent('vs. ano anterior');
      // Nenhum algarismo na pílula: sem período anterior não há "+5%".
      expect(pilula.textContent).not.toMatch(/\d/);
    }
    expect(textoDaDica('Por que a variação de municípios da adr não aparece', cartao('municipios'))).toMatch(/issue 69/);
  });

  it('o título diz "sua área de atuação" e o controle de comparação vem desligado, com o período em vigor', async () => {
    responder();
    abrir();
    await esperarACarga();
    irPara('Território');

    const secao = bloco('carteira-na-area')!;
    expect(secao).toHaveTextContent('visão consolidada da sua área de atuação');
    expect(secao).not.toHaveTextContent(/sua região/);

    expect(within(secao).getByRole('switch', { name: 'Comparar com o período anterior' })).toBeDisabled();
    const periodo = within(secao).getByRole('combobox', { name: 'Período da carteira' }) as HTMLSelectElement;
    expect(periodo).toBeDisabled();
    // set/2025 a ago/2026 são doze meses — escritos como a maquete escreve.
    expect(periodo).toHaveTextContent('12 meses');
    expect(textoDaDica('Por que a comparação e o período não mudam aqui', secao)).toContain('set/2025 a ago/2026');
  });

  it('a conferência da tabela saiu das linhas e está na dica do título, com o total da consulta', async () => {
    responder();
    abrir();
    await esperarACarga();
    irPara('Território');

    const tabela = bloco('tabela-municipios')!;
    expect(tabela.querySelector('tbody')).not.toHaveTextContent('Total da consulta');
    const dica = textoDaDica('Os totais da ADR e da consulta', tabela);
    // 18 (ADR) + 18 (fora da ADR) + 2 (fora do mapa) = 38 elegíveis.
    expect(dica).toMatch(/Total da consulta.*38 elegíveis/);
  });
});

describe('Indicadores Geográficos — somável × razão (documento 50, §7)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  /**
   * A COMPARAÇÃO MUDOU DE LUGAR NA T4.6, E NÃO DE REGRA.
   *
   * Os cinco indicadores estruturais eram cartões do tamanho dos quatro números
   * de decisão, com a fatia escrita embaixo em linha permanente. Viraram uma
   * faixa de uma linha, e a fatia foi para a dica DE CADA NÚMERO — não para uma
   * dica só no fim da faixa, que faria o leitor procurar a dele no meio de um
   * parágrafo. A §7 continua valendo: toda somável responde "que fatia isto é?".
   */
  const faixaDe = (rotulo: string) => document.querySelector<HTMLElement>(`[data-faixa="${rotulo}"]`)!;

  /**
   * Abre a dica de um indicador da faixa e devolve o TEXTO da parte da
   * comparação — a fatia.
   *
   * UMA DICA POR NÚMERO, COM DUAS PARTES (fidelidade às maquetes, 23/09/2026):
   * eram duas dicas empilhadas, a da fatia e a da procedência. Viraram uma, com
   * cada parte no seu parágrafo marcado — e é a parte da fatia que este helper
   * lê, para que a procedência não passe por fatia em nenhuma afirmação.
   *
   * ELA FECHA A DICA ANTES DE SAIR, e isso não é higiene: o balão do Radix vive
   * num portal e só some quando o gatilho perde o foco. Abrir a segunda sem
   * fechar a primeira deixa duas com `role="tooltip"` no documento, e o
   * `getByRole` falha com "found multiple elements" — o que parece defeito da
   * tela e é só o teste esquecendo de fechar a porta.
   */
  function comparacaoDe(rotulo: string): string | null {
    const nome = rotulo.toLowerCase();
    const gatilho = within(faixaDe(rotulo)).queryByRole('button', {
      name: new RegExp(`^(Quanto ${nome} representa|De onde vem ${nome} e quanto representa)$`),
    });
    if (!gatilho) return null;

    fireEvent.focus(gatilho);
    const texto = screen.getByRole('tooltip').querySelector('[data-parte="comparacao"]')?.textContent ?? '';
    fireEvent.blur(gatilho);
    return texto;
  }

  it('grandeza somável mostra as DUAS fatias: Região Tracbel e SP', async () => {
    responder();
    abrir();
    await esperarACarga();

    // 422 tratores sobre 38.364 da ADR = 1,1%; sobre 120.000 publicados de SP = 0,4%.
    const dica = comparacaoDe('Parque de tratores')!;
    expect(dica).toMatch(/1,1% da Região Tracbel/);
    expect(dica).toMatch(/0,4% de SP/);
  });

  it('a fatia diz "Região Tracbel", e nunca só "região" — Norte e Noroeste são sub-regiões', async () => {
    responder();
    abrir();
    await esperarACarga();

    let comFatia = 0;
    for (const rotulo of ['Parque de tratores', 'Propriedades', 'Valor da lavoura', 'Rebanho bovino']) {
      const texto = comparacaoDe(rotulo) ?? '';
      if (!/% da /.test(texto)) continue;
      comFatia++;
      expect(texto).toMatch(/% da Região Tracbel/);
    }

    expect(comFatia).toBeGreaterThan(0);
  });

  it('sem denominador, a fatia não aparece — e não vira 0%', async () => {
    const semRegiao = painel();
    semRegiao.indicadores.regiaoTracbel = null;
    semRegiao.indicadores.estado = null;
    obterIndicadoresTerritoriais.mockResolvedValue({ dados: semRegiao, procedencia: null });
    carregarMalhaDeSaoPaulo.mockResolvedValue(MALHA);
    abrir();
    await esperarACarga();

    const texto = comparacaoDe('Parque de tratores') ?? '';
    expect(texto).not.toMatch(/0% da Região Tracbel/);
    expect(texto).not.toMatch(/0% de SP/);
    expect(texto).not.toMatch(/% da Região Tracbel/);
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

    // UMA DICA POR NÚMERO (fidelidade às maquetes): a fatia e a procedência
    // moram no mesmo balão, e o nome dele diz as duas coisas.
    const gatilho = screen.getByRole('button', { name: 'De onde vem parque de tratores e quanto representa' });

    // Pelo TECLADO: o `title=` cru nunca abriu assim.
    fireEvent.focus(gatilho);
    const dica = (await screen.findByRole('tooltip')).querySelector<HTMLElement>('[data-parte="procedencia"]')!;

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

    // `usinas: null` no contrato — o número existe na faixa, a fonte não. A
    // dica dele é só a da fatia, e não traz carimbo nenhum.
    const usinas = document.querySelector<HTMLElement>('[data-faixa="Usinas de etanol"]')!;
    expect(usinas).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /De onde vem usinas de etanol/ })).not.toBeInTheDocument();

    const gatilho = within(usinas).getByRole('button', { name: 'Quanto usinas de etanol representa' });
    fireEvent.focus(gatilho);
    const dica = screen.getByRole('tooltip');
    expect(dica.querySelector('[data-parte="procedencia"]')).toBeNull();
    expect(dica).not.toHaveTextContent(/Fonte:/);
    fireEvent.blur(gatilho);
  });

  it('a tela não escreve fonte à mão: a comparação não repete o carimbo', async () => {
    responder();
    abrir();
    await esperarACarga();

    // A fonte mora no carimbo de procedência; a comparação diz só a fatia. Os
    // dois são PARTES SEPARADAS da mesma dica (fidelidade às maquetes — eram
    // duas dicas), e é isso que impede a fonte de ser escrita duas vezes, à mão
    // numa delas (issue 167).
    const tratores = document.querySelector<HTMLElement>('[data-faixa="Parque de tratores"]')!;
    fireEvent.focus(
      within(tratores).getByRole('button', { name: 'De onde vem parque de tratores e quanto representa' }),
    );
    const dica = screen.getByRole('tooltip');
    const comparacao = dica.querySelector('[data-parte="comparacao"]')?.textContent ?? '';

    expect(comparacao).toMatch(/Região Tracbel/);
    expect(comparacao).not.toMatch(/Censo Agropecuário/);
    expect(comparacao).not.toMatch(/IBGE/);
    // E a fonte está lá, do contrato, na parte dela.
    expect(dica.querySelector('[data-parte="procedencia"]')).toHaveTextContent('Pesquisa: Censo Agropecuário');
  });
});

describe('Indicadores Geográficos — os quatro KPIs e o momento (fase T3)', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  const executivo = (rotulo: string) => bloco('kpis-executivos')!.querySelector<HTMLElement>(`[data-kpi="${rotulo}"]`)!;

  /** Abre a dica do motivo de um KPI vazio, lê e FECHA — ver `comparacaoDe`. */
  function motivoDe(rotulo: string): string {
    // "não aparece" é a frase do `ValorAusente`, que vale na tela inteira — o
    // cartão de indicador usa o mesmo componente, e não uma segunda redação.
    const gatilho = within(executivo(rotulo)).getByRole('button', {
      name: `Por que ${rotulo.toLowerCase()} não aparece`,
    });

    fireEvent.focus(gatilho);
    const texto = screen.getByRole('tooltip').textContent ?? '';
    fireEvent.blur(gatilho);
    return texto;
  }

  it('os quatro números de decisão estão lá, nesta ordem', async () => {
    responder();
    abrir();
    await esperarACarga();

    const rotulos = [...bloco('kpis-executivos')!.querySelectorAll<HTMLElement>('[data-kpi]')].map(
      (n) => n.dataset.kpi,
    );
    expect(rotulos).toEqual(['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade']);
  });

  /**
   * O MOTIVO SAIU DE DENTRO DO CARTÃO E FOI PARA A DICA (fase T4.6).
   *
   * Ele era um parágrafo embaixo do travessão, e empurrava o número para baixo:
   * os quatro cartões ficavam com alturas diferentes, e o olho lê diferença de
   * altura como diferença de importância. O texto continua inteiro, com a issue
   * que destrava — só que a um passo, e não ocupando o lugar do número.
   */
  it('os três sem dado dizem o que falta e a issue que destrava — nunca um número', async () => {
    responder();
    abrir();
    await esperarACarga();

    // O MOTIVO É A FRASE DO SERVIDOR (issue 69, parte A), letra por letra: a tela não escreve mais por que
    // um destes números falta. Se uma constante do front voltasse, a frase deixaria de ser esta.
    const { numerosDeDecisao } = painel();
    expect(motivoDe('Mercado anual')).toBe(numerosDeDecisao.mercadoAnual.frase);
    expect(motivoDe('Captura Tracbel')).toBe(numerosDeDecisao.capturaPercentual.frase);
    expect(motivoDe('Oportunidade')).toBe(numerosDeDecisao.oportunidade.frase);
    expect(motivoDe('Mercado anual')).toMatch(/issue 70/);
    expect(motivoDe('Captura Tracbel')).toMatch(/issue 69/);

    // NENHUM DELES INVENTOU NÚMERO. O que se afirma é a ausência de dígito, e
    // não o texto exato: o "sem dado" que acompanha o traço é do `ValorAusente`
    // e só existe para o leitor de tela (`cad-so-leitor`) — na tela, o cartão
    // mostra um travessão e, no lugar dela, a unidade da maquete.
    for (const rotulo of ['Mercado anual', 'Captura Tracbel', 'Oportunidade']) {
      const valor = executivo(rotulo).querySelector('.mv-kpi-valor')!.textContent ?? '';
      expect(valor, `${rotulo} mostrou um número onde não há dado`).not.toMatch(/\d/);
      expect(valor).toContain('—');
    }
  });

  it('quando a API manda o número, o cartão mostra o valor — e o mercado parcial diz, ao lado dele, o que ficou de fora', async () => {
    // O DIA EM QUE AS ISSUES 69 E 70 CHEGAREM: nada muda na tela além do dado. O mercado anual que soma só
    // as categorias com preço sai marcado como parcial no lugar da unidade, e não só na dica — lido sem a
    // marca, afirmaria que a categoria sem preço não vale nada.
    responder((p) => ({
      ...p,
      numerosDeDecisao: {
        ...p.numerosDeDecisao,
        mercadoAnual: {
          valor: 42_000_000,
          motivo: 'Nenhum',
          frase: '',
          parcial: true,
          categoriasSemPreco: ['Colhedora de cana', 'Pulverizador'],
        },
        capturaPercentual: { valor: 14.8, motivo: 'Nenhum', frase: '' },
        oportunidade: { valor: 311.6, motivo: 'Nenhum', frase: '' },
      },
    }));
    abrir();
    await esperarACarga();

    const valor = (rotulo: string) => executivo(rotulo).querySelector<HTMLElement>('.mv-kpi-valor')!;
    expect(valor('Mercado anual')).toHaveTextContent('R$ 42 mi');
    expect(valor('Mercado anual')).toHaveTextContent('parcial — sem preço de Colhedora de cana, Pulverizador');
    expect(valor('Captura Tracbel')).toHaveTextContent('14,8%');
    expect(valor('Captura Tracbel')).toHaveTextContent('da demanda estimada');
    expect(valor('Oportunidade')).toHaveTextContent('312');
    expect(valor('Oportunidade')).toHaveTextContent('máquinas não capturadas');
    // Com número, o valor não tem "por que não aparece" (a linha da variação continua com a dela).
    for (const rotulo of ['Mercado anual', 'Captura Tracbel', 'Oportunidade'])
      expect(within(valor(rotulo)).queryByRole('button')).toBeNull();
  });

  it('enquanto a leitura não responde, os quatro mostram o traço SEM dica — não se afirma por que falta antes de saber', async () => {
    // A leitura que nunca volta: o painel fica carregando.
    obterIndicadoresTerritoriais.mockReturnValue(new Promise(() => {}));
    carregarMalhaDeSaoPaulo.mockResolvedValue(MALHA);
    abrir();
    await waitFor(() => expect(bloco('kpis-executivos')).not.toBeNull());

    for (const rotulo of ['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade']) {
      const valor = executivo(rotulo).querySelector<HTMLElement>('.mv-kpi-valor')!;
      expect(valor.textContent).toContain('—');
      expect(within(valor).queryByRole('button')).toBeNull();
    }
  });

  it('a demanda anual sai vazia apontando a D-P01, e não zero', async () => {
    responder();
    abrir();
    await esperarACarga();

    const motivo = motivoDe('Demanda anual');
    expect(motivo).toMatch(/ciclo de renovação/);
    expect(motivo).toMatch(/issue 63/);
    expect(executivo('Demanda anual').querySelector('.mv-kpi-valor')!.textContent).not.toMatch(/\d/);
  });

  it('os quatro cartões têm a mesma estrutura — rótulo, valor e variação, sempre', async () => {
    // O QUE ISTO IMPEDE: que um cartão volte a ter um parágrafo dentro e fique
    // mais alto que os vizinhos. As três partes existem em todos os quatro,
    // mesmo vazias, e é isso que iguala a altura sem fixar altura em pixel.
    responder();
    abrir();
    await esperarACarga();

    for (const rotulo of ['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade']) {
      const cartao = executivo(rotulo);
      expect(cartao.querySelector('.mv-kpi-rotulo')).not.toBeNull();
      expect(cartao.querySelector('.mv-kpi-valor')).not.toBeNull();
      expect(cartao.querySelector('.mv-kpi-contexto')).not.toBeNull();
    }
  });

  it('a linha "vs. ano anterior" existe nos quatro, com traço e o motivo — nunca um "+8%" inventado', async () => {
    // DECISÃO DO USUÁRIO (23/09/2026): o número que a maquete mostra e o
    // sistema não tem aparece no MESMO LUGAR, com "—" e a dica que diz o que
    // falta. A leitura devolve uma janela de competência, não duas (issue 69).
    responder(comMomento);
    abrir();
    await esperarACarga();

    for (const rotulo of ['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade']) {
      const linha = executivo(rotulo).querySelector<HTMLElement>('.mv-kpi-contexto')!;
      expect(linha).toHaveTextContent('vs. ano anterior');
      expect(linha.textContent, `${rotulo} inventou uma variação`).not.toMatch(/\d/);

      const gatilho = within(linha).getByRole('button', {
        name: `Por que a variação de ${rotulo.toLowerCase()} não aparece`,
      });
      fireEvent.focus(gatilho);
      expect(screen.getByRole('tooltip')).toHaveTextContent(/issue 69/);
      fireEvent.blur(gatilho);
    }

    // E A UNIDADE DA CAPTURA NÃO É "participação no mercado", como na maquete:
    // é a mesma palavra que "share", e o denominador é a demanda ESTIMADA.
    expect(executivo('Captura Tracbel')).not.toHaveTextContent(/participação/i);
    expect(executivo('Captura Tracbel')).toHaveTextContent('da demanda estimada');
  });

  it('o porte nasce SEM NOME, e a dica diz que nulo não é "pequeno"', async () => {
    responder();
    abrir();
    await esperarACarga();

    // O PORTE É A SEGUNDA CÉLULA DA RÉGUA (T4.8): `porte-e-momento` marca só a
    // primeira, que é o momento. Quem contém as duas é a faixa.
    const porte = bloco('faixa-do-mercado')!;
    expect(porte).toHaveTextContent('Porte estrutural');
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
    expect(porte).toHaveTextContent('Momento do mercado');
    expect(porte).not.toHaveTextContent('1,00');
    expect(porte).not.toHaveTextContent('RETRAÍDO');

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

    // A PÍLULA DIZ A FAIXA; O NÚMERO ABRE A DICA DO TÍTULO (fidelidade às
    // maquetes, 23/09/2026). A maquete mostra "NORMAL" sozinho na pílula, e o
    // que a tela tem e a maquete não mostra vai para a dica ao lado do título —
    // o fator continua a um Tab, com a faixa escrita junto.
    const momento = bloco('porte-e-momento')!;
    expect(momento.querySelector('.mv-pilula')).toHaveTextContent('Retraído');
    expect(momento.querySelector('.mv-pilula')).not.toHaveTextContent('0,88');

    const gatilho = within(momento).getByRole('button', { name: 'Como o momento do mercado é composto' });
    fireEvent.focus(gatilho);
    expect(screen.getByRole('tooltip')).toHaveTextContent('Fator agregado 0,88 — Retraído');
    fireEvent.blur(gatilho);
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

    // A COMPOSIÇÃO É A TABELA "Detalhamento por cultura" DA MAQUETE (fase 3):
    // comparar a mesma grandeza entre linhas é para o que a tabela existe.
    const composicao = bloco('composicao-do-fator')!;
    const linhas = [...composicao.querySelectorAll<HTMLElement>('tbody tr')];
    expect(linhas.map((l) => l.dataset.cultura)).toEqual(['CAFE', 'CANA']);

    // Fatores diferentes na mesma tela — o que a regra antiga escondia atrás de um só.
    expect(linhas[0]).toHaveTextContent('0,84');
    expect(linhas[1]).toHaveTextContent('1,00');
    // A demanda do café é o que manda no agregado: 300 das 400 máquinas/ano —
    // estrutural e ajustada em colunas próprias, como na maquete.
    const celulas = (tr: HTMLElement) => [...tr.querySelectorAll('td')].map((td) => (td.textContent ?? '').trim());
    expect(celulas(linhas[0]).slice(5, 7)).toEqual(['300', '252']);
    expect(celulas(linhas[1]).slice(5, 7)).toEqual(['100', '100']);

    // AS BARRAS DA COMPOSIÇÃO têm uma linha por cultura, na mesma ordem.
    expect([...composicao.querySelectorAll<HTMLElement>('.mom-pilha')].map((l) => l.dataset.cultura)).toEqual([
      'CAFE',
      'CANA',
    ]);

    // E o índice de CADA cultura continua dito, agora no ⋮ da linha dela.
    fireEvent.focus(within(linhas[0]).getByRole('button', { name: 'Detalhes do fator de Café (Total)' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent('0,80');
  });

  it('as parcelas são TRÊS por cultura, e o custo não ganha número próprio (D-P05)', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const composicao = bloco('composicao-do-fator')!;
    const colunas = [...composicao.querySelectorAll('thead th')].map((n) => (n.textContent ?? '').trim());
    // AS COLUNAS DA MAQUETE (fidelidade às maquetes, fase 3). O custo tem o
    // lugar dela — decisão 2 do usuário: o layout é o da maquete —, mas não tem
    // número: ele entra DENTRO da parcela de commodity.
    expect(colunas).toEqual([
      'Cultura',
      'Commodity',
      'Custo',
      'Crédito',
      'Percepção comercial',
      'Fator final',
      'Demanda estrutural',
      'Demanda ajustada',
      'Var. %',
      'Detalhes',
    ]);
    for (const tr of composicao.querySelectorAll<HTMLElement>('tbody tr'))
      expect(tr.querySelectorAll('td')[1].textContent, 'o custo ganhou um número').not.toMatch(/\d/);

    // AS BARRAS TÊM TRÊS PARCELAS, e nenhuma é de custo.
    const parcelas = new Set(
      [...composicao.querySelectorAll<HTMLElement>('.mom-pilha-trilho [data-parcela]')].map((p) => p.dataset.parcela),
    );
    expect([...parcelas].sort()).toEqual(['commodity', 'credito', 'percepcao']);

    // O CARTÃO DO CUSTO tem o traço e diz por quê.
    const custo = composicao.querySelector<HTMLElement>('[data-cartao="Custo"]')!;
    expect(custo.querySelector('.mom-cartao-valor')!.textContent).not.toMatch(/\d/);
    expect(textoDaDica('Por que o custo no fator não aparece', custo)).toMatch(/D-P05/);
  });

  it('o SINAL carrega o sentido de cada parcela, e a dica diz que o índice é da cultura', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const composicao = bloco('composicao-do-fator')!;
    const linha = (codigo: string) => composicao.querySelector<HTMLElement>(`tbody tr[data-cultura="${codigo}"]`)!;
    const parcelas = (tr: HTMLElement) =>
      [...tr.querySelectorAll('td')].slice(0, 4).map((td) => (td.textContent ?? '').trim());

    // Café: preço −0,08 → baixa; crédito −0,06 → baixa; percepção +0,02 → sobe.
    // O SINAL É ESCRITO, e não só a cor: a tela continua legível em escala de cinza.
    const cafe = parcelas(linha('CAFE'));
    expect([cafe[0], cafe[2], cafe[3]]).toEqual(['-8,0%', '-6,0%', '+2,0%']);
    // Cana: o preço dela SOBE. Duas culturas, dois sentidos, na mesma tela.
    expect(parcelas(linha('CANA'))[0]).toBe('+4,0%');

    // A VARIAÇÃO DA DEMANDA leva a seta: 300 → 252 é ↓ 16%.
    expect(linha('CAFE').querySelectorAll('td')[7]).toHaveTextContent('↓ -16%');

    const dica = textoDaDica('De quem é a parcela de commodity', composicao);
    expect(dica).toMatch(/O custo entra AQUI/);
    expect(dica).toMatch(/DE CADA CULTURA/);
  });

  it('o total fecha a conta que o topo mostra: 352 ÷ 400 = 0,88', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    // A CONTA DO AGREGADO MORA NA DICA DO DETALHAMENTO (fidelidade às
    // maquetes, fase 3): a maquete não tem linha de total, e a conta inteira —
    // Σ ajustada ÷ Σ estrutural — continua a um Tab de distância.
    const conta = textoDaDica('Como ler detalhamento por cultura', bloco('composicao-do-fator')!);
    expect(conta).toContain('352 ÷ 400 = 0,88');
    expect(conta).toMatch(/Σ demanda ajustada ÷ Σ demanda estrutural/);
    // O TOPO MOSTRA O MESMO 0,88 — na dica do título "Momento do mercado", que é
    // onde o número mora desde a fidelidade às maquetes (a pílula diz a faixa).
    const gatilho = within(bloco('porte-e-momento')!).getByRole('button', { name: 'Como o momento do mercado é composto' });
    fireEvent.focus(gatilho);
    expect(screen.getByRole('tooltip')).toHaveTextContent('Fator agregado 0,88');
    fireEvent.blur(gatilho);
  });

  it('a principal cultura é contexto, e a dica diz que a área não entra no cálculo', async () => {
    // A CANA TEM A MAIOR ÁREA e o fator é 0,88, puxado pelo café: se a área
    // mandasse, o índice 1,30 da cana deixaria o recorte aquecido.
    responder(comMomento);
    abrir();
    await esperarACarga();

    // A PREDOMINANTE VIROU O SELO DA SEÇÃO DOS MAPAS (T4.8): ela responde "o que
    // se planta aqui?", que é a pergunta de quem está olhando o mapa.
    const contexto = document.querySelector<HTMLElement>('[data-contexto="predominante"]')!;
    expect(contexto).toHaveTextContent('Principal cultura: Cana-de-açúcar');
    // A FATIA SAIU DA LINHA (maquete: "Principal cultura: Café") e abre a dica,
    // junto do critério — e a linha não diz "região" sozinha (issue 163).
    expect(contexto).not.toHaveTextContent('58%');
    expect(contexto).not.toHaveTextContent(/região/i);

    fireEvent.focus(within(contexto).getByRole('button', { name: 'Qual é o critério da principal cultura' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('58% da área relevante');
    expect(dica).toHaveTextContent(/maior área útil entre as culturas com regra de potencial/);
    // "não muda o momento", e não mais "o número acima": o selo saiu de cima do
    // número e foi para o cabeçalho dos mapas (T4.8). A afirmação é a mesma —
    // trocar a maior área não mexe no fator.
    expect(dica).toHaveTextContent(/não muda o momento/);
  });

  it('o fator explica a própria origem, com a competência do preço', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    // UMA DICA SÓ AO LADO DO TÍTULO (maquete): o número, a conta e a fonte do
    // momento moram nela — eram duas dicas dentro da pílula.
    fireEvent.focus(
      within(bloco('porte-e-momento')!).getByRole('button', { name: 'Como o momento do mercado é composto' }),
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

  /**
   * "COMO INTERPRETAR" E "LIMITAÇÕES" SAÍRAM DO CORPO, E NÃO DA TELA
   * (fidelidade às maquetes, 23/09/2026).
   *
   * Eram `<details>` de uma linha — um entre os filtros e as abas, o outro
   * embaixo dos mapas. A maquete não tem nenhum dos dois, e a decisão do usuário
   * é que o que existe hoje e não está na maquete vá para a dica ao lado do
   * assunto. Estes testes eram "é recolhível e o conteúdo não se perde"; agora
   * são "não ocupa o corpo, e o conteúdo inteiro está na dica".
   */
  it('"Como interpretar os indicadores" está na dica de Mercado e na de Território, e fora do corpo', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(bloco('como-ler')).toBeNull();
    expect(document.body.textContent).not.toContain('Como interpretar os indicadores');

    const mercado = textoDaDica('Como ler os números de Mercado');
    expect(mercado).toContain('Como interpretar os indicadores');
    expect(mercado).toContain('regra a confirmar');
    // A dica das fatias, que morava ao lado do título "O mercado da região",
    // veio junto — com a Região Tracbel como denominador, e nunca "região" só.
    expect(mercado).toMatch(/denominador da Região Tracbel/);

    irPara('Território');
    const territorio = textoDaDica('Fonte e método de a carteira na área de atuação');
    expect(territorio).toContain('Como interpretar os indicadores');
    expect(territorio).toContain('regra a confirmar');
  });

  it('"Limitações dos dados" está na dica da Visão geográfica, inteira, e fora do corpo', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(bloco('limitacoes')).toBeNull();

    const dica = textoDaDica('Fonte e método de visão geográfica');
    expect(dica).toContain('Limitações dos dados');
    expect(dica).toContain('participacaoDeMercado');
    expect(dica).toContain('emplacamento não integrado');
    // O rodapé que diz de onde o texto vem também veio.
    expect(dica).toContain('Medido pela API na mesma consulta');
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

    // AS DUAS ABAS TÊM O LAYOUT DA MAQUETE (fidelidade às maquetes, fase 3): os
    // quatro cartões estão lá, e no lugar do número que não existe, o traço e a
    // issue que o destrava — decisão 1 do usuário.
    const momento = bloco('momento-do-mercado')!;
    const valorDe = (rotulo: string) =>
      momento.querySelector<HTMLElement>(`[data-cartao="${rotulo}"] .mom-cartao-valor`)!;

    fireEvent.click(within(momento).getByRole('tab', { name: 'Termo de troca' }));
    for (const rotulo of ['Sacas para comprar 1 trator', 'Variação (5 anos)', 'Melhor cultura de troca', 'Tendência atual'])
      expect(valorDe(rotulo).textContent, rotulo).not.toMatch(/\d/);
    expect(textoDaDica('Por que as sacas por trator não aparece', momento)).toMatch(/issue 70/);

    fireEvent.click(within(momento).getByRole('tab', { name: 'Percepção comercial' }));
    expect(valorDe('Tendência dos gestores').textContent).not.toMatch(/\d/);
    expect(textoDaDica('Por que a tendência dos gestores não aparece', momento)).toMatch(/issue 71/);
  });

  it('as cinco abas do Momento são abas de verdade, e a Composição abre o bloco', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const momento = bloco('momento-do-mercado')!;
    const abas = within(momento).getAllByRole('tab');
    expect(abas.map((a) => a.textContent)).toEqual([
      'Composição do fator',
      'Rentabilidade',
      'Crédito',
      'Termo de troca',
      'Percepção comercial',
    ]);
    expect(abas[0]).toHaveAttribute('aria-selected', 'true');

    // O alternador SEGMENTADO dos outros blocos continua o de sempre: o
    // Momento não entrou nele.
    expect(momento.querySelector('.terr-alternador')).toBeNull();
  });

  it('o vocabulário decidido vale nas cinco abas: nenhum rótulo diz "contrato" nem "share"', async () => {
    responder(comMomento);
    abrir();
    await esperarACarga();

    const momento = bloco('momento-do-mercado')!;
    for (const nome of ['Composição do fator', 'Rentabilidade', 'Crédito', 'Termo de troca', 'Percepção comercial']) {
      fireEvent.click(within(momento).getByRole('tab', { name: nome }));
      const rotulos = [
        ...momento.querySelectorAll<HTMLElement>(
          '[role="tab"], .mom-titulo, .mom-cartao-rotulo, .mom-painel-titulo, th, .mom-legenda li, option',
        ),
      ].map((n) => n.textContent ?? '');
      expect(rotulos.filter((r) => /contrat|share|opera[cç][aã]o/i.test(r)), nome).toEqual([]);
      // "Região" nunca sozinha: ou é Região Tracbel, ou é sub-região.
      expect(rotulos.filter((r) => /\bregião\b(?! Tracbel)/i.test(r)), nome).toEqual([]);
    }
  });

  it('"Simular cenário" do Momento abre a mesma calculadora do Potencial', async () => {
    responder();
    abrir();
    await esperarACarga();

    const momento = bloco('momento-do-mercado')!;
    fireEvent.click(within(momento).getByRole('button', { name: 'Simular cenário com o momento do mercado' }));
    expect(momento.contains(bloco('calculadora'))).toBe(true);
  });

  it('captura e não capturado dizem que dependem das vendas em unidades', async () => {
    responder();
    abrir();
    await esperarACarga();

    // O MOTIVO É O DO SERVIDOR (issue 69, parte A) — o mesmo dos cartões do topo, e não uma terceira
    // redação escrita no bloco Performance.
    const { numerosDeDecisao } = painel();
    fireEvent.click(screen.getByRole('button', { name: 'Captura' }));
    expect(esperarLacuna('Captura Tracbel').textContent).toBe(numerosDeDecisao.capturaPercentual.frase);

    fireEvent.click(screen.getByRole('button', { name: 'Não capturado' }));
    expect(esperarLacuna('Potencial não capturado').textContent).toBe(numerosDeDecisao.oportunidade.frase);
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
    // ESCOPADO AO BLOCO: o Momento do mercado da maquete também tem um "Simular
    // cenário" no cabeçalho dele.
    const simular = within(bloco('potencial-estrutural')!).getByRole('button', { name: 'Simular cenário' });
    // UM BOTÃO À PARTE, e não um quarto segmento do alternador (maquete).
    expect(simular.closest('.terr-alternador')).toBeNull();
    fireEvent.click(simular);

    const calculadora = bloco('calculadora')!;
    expect(bloco('potencial-estrutural')!.contains(calculadora)).toBe(true);
    // Ela não mora mais dentro do cartão do mapa de potencial.
    expect(document.querySelector('[data-mapa="potencial"]')!.contains(calculadora)).toBe(false);
  });

  it('sigilo do IBGE em todo o recorte sai com traço e motivo — nunca "0 tratores"', async () => {
    // A SOMA IGNORA O MUNICÍPIO SOB SIGILO; quando NENHUM foi divulgado, a soma
    // vazia dava 0 — e zero afirma que a região não tem trator.
    responder((p) => ({
      ...p,
      indicadores: {
        ...p.indicadores,
        municipios: p.indicadores.municipios.map((m) => ({
          ...m,
          estrutura: { ...m.estrutura, tratores: null, estabelecimentos: null, tratoresPorMilKm2: null },
        })),
      },
    }));
    abrir();
    await esperarACarga();

    for (const rotulo of ['Parque de tratores', 'Propriedades']) {
      const item = document.querySelector<HTMLElement>(`[data-faixa="${rotulo}"]`)!;
      expect(item.querySelector('.mv-faixa-valor')!.textContent, `${rotulo} virou zero`).not.toMatch(/\d/);
      fireEvent.focus(within(item).getByRole('button', { name: `Por que ${rotulo.toLowerCase()} não aparece` }));
      expect(screen.getByRole('tooltip')).toHaveTextContent(/sigilo do IBGE .* não é zero/);
      fireEvent.blur(within(item).getByRole('button', { name: `Por que ${rotulo.toLowerCase()} não aparece` }));
    }

    // O cartão do mapa da estrutura diz o mesmo, e o mapa continua hachurado.
    const estrutura = document.querySelector<HTMLElement>('[data-mapa="estrutura"]')!;
    expect(estrutura.querySelector('.terr-mapa-resumo')).toHaveTextContent(/sob sigilo do IBGE .* não é zero/);
    expect(estrutura.querySelector('.terr-mapa-resumo')!.textContent).not.toMatch(/\b0 tratores/);
  });

  it('as vendas abrem primeiro, com a variação e as máquinas vendidas no lugar da maquete — com traço', async () => {
    responder();
    abrir();
    await esperarACarga();

    const performance = bloco('performance-tracbel')!;
    const abas = [...performance.querySelectorAll<HTMLButtonElement>('.terr-alternador button')];
    expect(abas.map((b) => b.textContent)).toEqual(['Vendas', 'Captura', 'Não capturado']);
    expect(abas[0]).toHaveAttribute('aria-pressed', 'true');

    const cartoes = [...performance.querySelectorAll<HTMLElement>('[data-mini]')];
    expect(cartoes.map((c) => c.dataset.mini)).toEqual(['Vendas no período', 'Máquina', 'Pós-venda']);

    // A VARIAÇÃO DA MAQUETE, SEM NÚMERO: não há ano anterior na leitura.
    for (const cartao of cartoes) {
      const variacao = cartao.querySelector<HTMLElement>('.mv-variacao')!;
      expect(variacao).toHaveTextContent('vs. ano anterior');
      expect(variacao.textContent).toContain('—');
      expect(variacao.textContent, `${cartao.dataset.mini} inventou uma variação`).not.toMatch(/\d/);
    }

    // AS MÁQUINAS VENDIDAS NÃO EXISTEM (issue 69): o traço, e não um número.
    const maquina = cartoes[1];
    expect(maquina).toHaveTextContent('máquinas vendidas');
    fireEvent.focus(
      within(maquina).getByRole('button', { name: 'Por que o número de máquinas vendidas não aparece' }),
    );
    expect(screen.getByRole('tooltip')).toHaveTextContent(/issue 69/);
  });
});

/**
 * O QUE SAIU DO CORPO FOI PARA AS DICAS (fidelidade às maquetes, 23/09/2026).
 *
 * A decisão do usuário: o que existe hoje e não está na maquete sai do corpo da
 * página e vai para o ⓘ mais perto do assunto — NADA de substância é apagado.
 * Estes testes são a prova de que o texto continua a um Tab de distância, e de
 * que as regras que ele carregava (procedência presente, "Região Tracbel" e não
 * "região", permissão só depois de lida) continuam valendo lá dentro.
 */
describe('Indicadores Geográficos — o que saiu do corpo foi para as dicas', () => {
  afterEach(() => {
    obterIndicadoresTerritoriais.mockReset();
    carregarMalhaDeSaoPaulo.mockReset();
    guardado.clear();
  });

  it('o cabeçalho mostra só "Dados atualizados em…", e a procedência inteira está na dica ao lado', async () => {
    responder();
    abrir();
    await esperarACarga();

    const cabecalho = bloco('cabecalho')!;
    expect(cabecalho).toHaveTextContent(/Dados atualizados em \d{2}\/\d{2}\/\d{4}/);
    // O SELO DE PROCEDÊNCIA INTEIRO NÃO OCUPA MAIS UMA LINHA no cabeçalho…
    expect(cabecalho.querySelector('.cad-procedencia')).toBeNull();

    // …MAS A PROCEDÊNCIA CONTINUA NA TELA, pelo teclado: sistema, objeto e instante.
    const dica = textoDaDica('De onde vem o dado desta tela', cabecalho);
    expect(dica).toContain('CRM Tracbel · territorio.Municipio');
    expect(dica).toMatch(/Lido em \d{2}\/\d{2}\/\d{4}/);

    // E o botão de reler continua sendo um botão de verdade.
    expect(within(cabecalho).getByRole('button', { name: 'Reler os indicadores desta tela' })).toBeInTheDocument();
  });

  it('o alcance da consulta está na dica da Sub-região, e não afirma permissão antes da resposta', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(bloco('alcance')).toBeNull();

    const dica = textoDaDica('O que é a sub-região e o que esta consulta alcança');
    expect(dica).toMatch(/Visão: filial .* e as abaixo dela/);
    // A amostra não tem a permissão da empresa inteira — e a dica diz isso.
    expect(dica).toContain('exige a permissão de alcance entre filiais');
    // A hierarquia que a dica já explicava continua lá, com o termo decidido.
    expect(dica).toContain('Norte e Noroeste são SUB-REGIÕES');
  });

  it('a procedência do recorte está na dica do Período, e não numa linha embaixo dos filtros', async () => {
    responder();
    abrir();
    await esperarACarga();

    expect(bloco('filtros')!.textContent).not.toContain('cobertura medida em');

    const dica = textoDaDica('O período em vigor e por que não há FYTD');
    expect(dica).toContain('Vendas de set/2025 a ago/2026 (12 meses fechados)');
    expect(dica).toContain('cobertura medida em');
    expect(dica).toContain('área plantada PAM/IBGE 2024');
    // O motivo de não haver FYTD, que já morava nesta dica, continua nela.
    expect(dica).toContain('FYTD não é oferecido');
  });
});
