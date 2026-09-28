/**
 * A ficha do município — issue 152; três camadas na fase T4; ABAS na fase 4
 * (fidelidade às maquetes, 23/09/2026).
 *
 * O QUE ESTE TESTE SEMPRE PRENDEU continua preso: a cultura diz o ano dela, a
 * quantidade vem com a unidade que o servidor mandou, a produtividade aparece ao
 * lado da de São Paulo, e sem unidade ou sem colheita não se mostra número.
 *
 * O CONTRATO MUDOU NA FASE 4, e foi ajustado de propósito: a ficha virou cinco
 * abas, como na maquete. A camada intermediária e a de evidência foram para as
 * abas Lavoura e Estrutura; a executiva, para o topo da aba Oportunidades. O
 * conteúdo é o mesmo — e este teste troca de aba antes de conferir, que é o que
 * uma pessoa faria. O último bloco compara a lista de medidas de ANTES da fase
 * 4 com a de agora: nenhuma pode ter sumido.
 *
 * 27/09/2026: a variação "vs. ano anterior" dos cartõezinhos passou a ter número
 * onde há número, a lavoura da Visão geral traz todas as culturas quando o
 * histórico do município chega, e a aba Histórico ganhou as séries que existem.
 */

import { act, cleanup, fireEvent, render, screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProvedorDeContextoDeAcesso } from '../../dados/api/contexto';
import type {
  CulturaNoEstado,
  HistoricoDoMunicipio,
  IndicadoresDoMunicipio,
  NumerosDeDecisao,
  PeriodoAnterior,
  PotencialEstruturalDoMunicipio,
  PotencialTerritorial,
  ProcedenciasDoTerritorio,
  RegraDePotencialAplicada,
  TotaisDaRegiaoTracbel,
  TotaisDoEstado,
} from '../../tipos/territorio';
import { ProvedorDoPeriodo } from './carteira/periodo';
import { ProvedorDaComparacao } from './comparacao';
import { DetalheDoMunicipio } from './DetalheDoMunicipio';

const obterHistoricoDoMunicipio = vi.hoisted(() => vi.fn());

vi.mock('../../dados/api/territorio', async (original) => ({
  ...(await original<typeof import('../../dados/api/territorio')>()),
  obterHistoricoDoMunicipio,
}));

const guardado = new Map<string, string>();
vi.stubGlobal('localStorage', {
  getItem: (c: string) => guardado.get(c) ?? null,
  setItem: (c: string, v: string) => void guardado.set(c, v),
  removeItem: (c: string) => void guardado.delete(c),
  clear: () => guardado.clear(),
});

afterEach(() => {
  obterHistoricoDoMunicipio.mockReset();
  guardado.clear();
});

const REGRA: RegraDePotencialAplicada = {
  produtoCodigoIbge: 40139,
  produtoNome: 'Café (em grão) Total',
  hectaresPorMaquina: 10,
  modeloDeReferencia: '3036N',
  situacao: 'AConfirmar',
  justificativa: 'exemplo do gerente comercial',
  vigenteDesde: '2026-09-13',
  anosDeRenovacao: null,
  categoriaCodigo: 'TRATOR',
  categoriaNome: 'Trator',
};

const REGIAO: TotaisDaRegiaoTracbel = {
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
};

const SAO_PAULO: TotaisDoEstado = {
  ano: 2024,
  areaPlantadaHectares: 8_000_000,
  valorDaProducaoMilReais: 90_000_000,
  areaColhidaHectares: 7_900_000,
  tratores: { publicado: 120_000, somaDosMunicipios: 118_000 },
  estabelecimentos: { publicado: 180_000, somaDosMunicipios: 179_000 },
  anoDoCenso: 2017,
  rebanho: { publicado: 10_000_000, somaDosMunicipios: 9_900_000 },
  anoDoRebanho: 2024,
};

const PROCEDENCIAS: ProcedenciasDoTerritorio = {
  areaPlantada: {
    fonte: 'IBGE/SIDRA',
    pesquisa: 'PAM — Produção Agrícola Municipal',
    tabela: '5457',
    variavel: 'Área plantada',
    competencia: '2024',
    ultimaCargaUtc: '2026-09-22T03:00:00Z',
    ressalva: null,
  },
  valorDaProducao: null,
  tratores: null,
  estabelecimentos: null,
  rebanho: null,
  usinas: null,
  maquinasVendidas: null,
};

/**
 * OS NÚMEROS DE DECISÃO COMO A API OS MANDA (issue 69, parte A) — o estado de
 * hoje, com frases que não existem em lugar nenhum do front: se a ficha mostrar
 * uma delas, ela veio do servidor, e não de uma constante escrita na tela.
 */
const FRASE_SEM_PRECO = 'FRASE DO SERVIDOR — sem preço de referência de máquina (issue 70).';
const FRASE_SEM_VENDAS = 'FRASE DO SERVIDOR — sem as vendas em MÁQUINAS (issue 69).';
const NUMEROS: NumerosDeDecisao = {
  demandaAnual: { valor: null, motivo: 'SemDemandaAnual', frase: 'FRASE DO SERVIDOR — sem demanda (issue 63).' },
  mercadoAnual: { valor: null, motivo: 'SemPrecoDeMaquina', frase: FRASE_SEM_PRECO, parcial: false, categoriasSemPreco: [] },
  capturaPercentual: { valor: null, motivo: 'SemVendasEmUnidades', frase: FRASE_SEM_VENDAS },
  oportunidade: { valor: null, motivo: 'SemVendasEmUnidades', frase: FRASE_SEM_VENDAS },
  baseDaCaptura: null,
};

function potencial(parcial: Partial<PotencialTerritorial> = {}): PotencialTerritorial {
  return {
    produtoCodigoIbge: 40139,
    areaPlantadaHectares: 70,
    maquinasTeoricas: 7,
    // SEM DETALHE POR CATEGORIA a ficha mostra a linha única de antes, pela regra do produto.
    porCategoria: [],
    areaColhidaHectares: 60,
    valorDaProducaoMilReais: 900,
    ano: 2024,
    quantidadeProduzida: 200,
    unidadeDaQuantidade: 'toneladas',
    produtividade: 3.3333,
    unidadeDaProdutividade: 't/ha',
    ...parcial,
  };
}

function municipio(
  p: PotencialTerritorial,
  potencialEstrutural: PotencialEstruturalDoMunicipio | null = {
    parqueDeMaquinas: 7,
    demandaAnualDeMaquinas: null,
    areaUtilHectares: 70,
    estimativa: true,
    motivoSemParque: 'Nenhum',
    motivoSemDemanda: 'SemCicloDeRenovacao',
  },
  parcial: Partial<IndicadoresDoMunicipio> = {},
): IndicadoresDoMunicipio {
  return {
    potencialEstrutural,
    maquinasVendidas: null,
    codigoIbge: 3543402,
    nome: 'Ribeirão Preto',
    pertenceAAdr: true,
    listadoNaAreaDeAtuacao: true,
    regiao: 'Norte',
    lojaCodigo: '010101',
    lojaNome: 'Tracbel Agro — Ribeirão Preto',
    lojaAtivaNoCrm: true,
    cobertura: { clientes: 0, vinculos: 0, vinculosComCadencia: 0, cobertos: 0, foraDaCadencia: 0, nuncaContatados: 0, semCadencia: 0, pendentes: 0, percentualPendente: null },
    vendas: { clientesQueCompraram: 0, valorLiquido: 0, maquina: 0, peca: 0, servico: 0, outros: 0, posVenda: 0 },
    potencial: [p],
    responsaveisPelasCarteiras: [],
    vendasNoPeriodoAnterior: null,
    maquinasVendidasNoPeriodoAnterior: null,
    producao: null,
    estrutura: {
      anoDoCenso: null, tratores: null, tratoresAbaixoDe100Cv: null, tratoresDe100CvEMais: null, estabelecimentosComTrator: null,
      estabelecimentos: null, faixasDeArea: [], anoDoRebanho: null, bovinos: null, areaKm2: null, usinas: [], tratoresPorMilKm2: null,
      capacidadeDeEtanolM3Dia: null,
    },
    ...parcial,
  };
}

const CAFE_EM_SP: CulturaNoEstado = {
  produtoCodigoIbge: 40139,
  produtoNome: 'Café (em grão) Total',
  ano: 2024,
  areaPlantadaHectares: 190_405,
  areaColhidaHectares: 190_255,
  quantidadeProduzida: 335_310,
  unidadeDaQuantidade: 'toneladas',
  valorDaProducaoMilReais: null,
  produtividade: 1.7624,
  unidadeDaProdutividade: 't/ha',
};

/** A janela anterior como a API a manda — o faturamento cobre, o ART não. */
const PERIODO_ANTERIOR: PeriodoAnterior = {
  competenciaInicial: '2024-11-01',
  competenciaFinal: '2025-08-01',
  primeiraCompetenciaDoFaturamento: '2023-01-01',
  primeiroMesDoArt: '2025-01-01',
  maquinasVendidas: null,
  serieAtual: [],
  serieAnterior: [],
  vendasCobertas: true,
  maquinasCobertas: false,
  motivoSemVendas: null,
  motivoSemMaquinas: 'FRASE DO SERVIDOR — o ART começa em jan/2025.',
  mesEmCurso: null,
};

const FILTROS = { visao: 'Filial', filialDaVenda: '', filialDoCliente: '', categoriaDeMaquina: '', responsavel: '' } as const;

function abrir(
  m: IndicadoresDoMunicipio,
  culturasNoEstado: CulturaNoEstado[] = [],
  denominadores = true,
  aoFechar = () => {},
  {
    periodoAnterior = null,
    comFiltros = false,
    numeros = NUMEROS,
  }: { periodoAnterior?: PeriodoAnterior | null; comFiltros?: boolean; numeros?: NumerosDeDecisao } = {},
) {
  render(
    <ProvedorDeContextoDeAcesso>
      <ProvedorDoPeriodo
        value={{ meses: 10, rotulo: 'Ano fiscal', descricao: 'o ano fiscal até o último mês fechado', intervalo: 'nov/2025 a ago/2026' }}
      >
        <ProvedorDaComparacao periodoAnterior={periodoAnterior} numeros={null}>
          <DetalheDoMunicipio
            municipio={m}
            regras={[REGRA]}
            culturasNoEstado={culturasNoEstado}
            regiaoTracbel={denominadores ? REGIAO : null}
            estado={denominadores ? SAO_PAULO : null}
            procedencias={PROCEDENCIAS}
            numerosDeDecisao={numeros}
            filtros={comFiltros ? FILTROS : undefined}
            aoFechar={aoFechar}
          />
        </ProvedorDaComparacao>
      </ProvedorDoPeriodo>
    </ProvedorDeContextoDeAcesso>,
  );
}

/** O histórico como a API o manda: FY26 em curso, FY25 inteiro, o mesmo trecho do FY25 e dois anos da PAM. */
const HISTORICO: HistoricoDoMunicipio = {
  codigoIbge: 3543402,
  nome: 'Ribeirão Preto',
  primeiraCompetenciaDoFaturamento: '2024-11-01',
  primeiroMesDoArt: '2025-01-01',
  anosFiscais: [
    {
      anoFiscal: 2025,
      inicio: '2024-11-01',
      fim: '2025-10-01',
      emCurso: false,
      vendas: { clientesQueCompraram: 3, valorLiquido: 2_000_000, maquina: 1_500_000, peca: 300_000, servico: 200_000, outros: 0, posVenda: 500_000 },
      motivoSemVendas: null,
      maquinasVendidas: null,
      motivoSemMaquinas: 'FRASE DO SERVIDOR — o ART começa em jan/2025, depois do começo do FY25.',
    },
    {
      anoFiscal: 2026,
      inicio: '2025-11-01',
      fim: '2026-08-01',
      emCurso: true,
      vendas: { clientesQueCompraram: 4, valorLiquido: 1_250_000, maquina: 900_000, peca: 200_000, servico: 150_000, outros: 0, posVenda: 350_000 },
      motivoSemVendas: null,
      maquinasVendidas: 6,
      motivoSemMaquinas: null,
    },
  ],
  mesmoTrechoDoAnoAnterior: {
    anoFiscal: 2025,
    inicio: '2024-11-01',
    fim: '2025-08-01',
    emCurso: true,
    vendas: { clientesQueCompraram: 3, valorLiquido: 1_000_000, maquina: 800_000, peca: 120_000, servico: 80_000, outros: 0, posVenda: 200_000 },
    motivoSemVendas: null,
    maquinasVendidas: null,
    motivoSemMaquinas: 'FRASE DO SERVIDOR — o ART começa em jan/2025, depois do começo do FY25.',
  },
  lavoura: [
    {
      ano: 2023,
      areaPlantadaHectares: 36_000,
      areaColhidaHectares: 35_000,
      valorDaProducaoMilReais: 400_000,
      culturas: [{ produtoCodigoIbge: 40106, produtoNome: 'Cana-de-açúcar', areaPlantadaHectares: 30_000, areaColhidaHectares: 29_000, valorDaProducaoMilReais: 300_000 }],
    },
    {
      ano: 2024,
      areaPlantadaHectares: 37_226,
      areaColhidaHectares: 36_000,
      valorDaProducaoMilReais: 468_600,
      culturas: [
        { produtoCodigoIbge: 40106, produtoNome: 'Cana-de-açúcar', areaPlantadaHectares: 10_000, areaColhidaHectares: 10_000, valorDaProducaoMilReais: 100_000 },
        { produtoCodigoIbge: 40139, produtoNome: 'Café (em grão) Total', areaPlantadaHectares: 23_000, areaColhidaHectares: 22_000, valorDaProducaoMilReais: 300_000 },
        { produtoCodigoIbge: 40124, produtoNome: 'Soja (em grão)', areaPlantadaHectares: 2_000, areaColhidaHectares: 2_000, valorDaProducaoMilReais: 30_000 },
        { produtoCodigoIbge: 40112, produtoNome: 'Laranja', areaPlantadaHectares: 1_000, areaColhidaHectares: 1_000, valorDaProducaoMilReais: 20_000 },
        { produtoCodigoIbge: 40114, produtoNome: 'Milho (em grão)', areaPlantadaHectares: 1_000, areaColhidaHectares: 1_000, valorDaProducaoMilReais: 10_000 },
        { produtoCodigoIbge: 40110, produtoNome: 'Feijão (em grão)', areaPlantadaHectares: 226, areaColhidaHectares: 200, valorDaProducaoMilReais: 8_600 },
      ],
    },
  ],
};

/** A ficha com o histórico lido — espera a promessa resolver. */
async function abrirComHistorico(m: IndicadoresDoMunicipio, historico: HistoricoDoMunicipio = HISTORICO) {
  obterHistoricoDoMunicipio.mockResolvedValue({ dados: historico, procedencia: null });
  await act(async () => abrir(m, [], true, () => {}, { comFiltros: true, periodoAnterior: PERIODO_ANTERIOR }));
}

/** Troca de aba como uma pessoa troca: clicando no nome dela. */
const irPara = (aba: string) => fireEvent.click(screen.getByRole('tab', { name: aba }));

/** O painel da aba ativa — o único visível. */
const painelAtivo = () => screen.getByRole('tabpanel');

/** Abre o detalhe recolhido — nada se perdeu, mudou de casa. */
function abrirEvidencia(qual: string): HTMLElement {
  const bloco = document.querySelector<HTMLElement>(`[data-evidencia="${qual}"]`)!;
  fireEvent.click(bloco.querySelector('summary')!);
  return bloco;
}

/** Abre uma dica pelo teclado, lê e FECHA — o balão vive num portal. */
function textoDaDica(rotulo: string): string {
  const gatilho = screen.getByRole('button', { name: rotulo });
  fireEvent.focus(gatilho);
  const texto = screen.getByRole('tooltip').textContent ?? '';
  fireEvent.blur(gatilho);
  return texto;
}

describe('DetalheDoMunicipio — o que a ficha sempre disse', () => {
  it('diz o ano da cultura, a quantidade com a unidade e a produtividade ao lado da de SP', () => {
    abrir(municipio(potencial()), [CAFE_EM_SP]);
    irPara('Lavoura');
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).getByText(/Área plantada de Café \(em grão\) Total/)).toHaveTextContent('(2024)');
    expect(within(potencialRecolhido).getByText('200 toneladas')).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText(/3,33 t\/ha/)).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText(/SP 1,76 t\/ha/)).toBeInTheDocument();
  });

  it('mostra o parque do motor, e diz por que a demanda anual não saiu', () => {
    abrir(municipio(potencial()));
    irPara('Lavoura');
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).getByText(/70 ha úteis/)).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText(/ainda não foi confirmada pelo comercial/)).toBeInTheDocument();

    // O MOTIVO DA DEMANDA mora no topo da aba Oportunidades, e abre pelo teclado.
    irPara('Oportunidades');
    expect(textoDaDica('Por que a demanda anual não aparece')).toMatch(/de quantos em quantos anos a máquina é trocada/);
  });

  it('sem regra de potencial vigente, a ficha não inventa parque', () => {
    abrir(municipio(potencial(), null));
    irPara('Lavoura');
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).queryByText('Parque teórico do município')).not.toBeInTheDocument();
  });

  it('com trator e colheitadeira no mesmo produto, a ficha mostra as duas contas e a soma (issue 240)', () => {
    abrir(
      municipio(
        potencial({
          produtoCodigoIbge: 40124,
          maquinasTeoricas: 12.7,
          porCategoria: [
            { categoriaCodigo: 'TRATOR', categoriaNome: 'Trator', hectaresPorMaquina: 200, modeloDeReferencia: 'trator', maquinas: 11 },
            {
              categoriaCodigo: 'COLHEITADEIRA',
              categoriaNome: 'Colheitadeira',
              hectaresPorMaquina: 1500,
              modeloDeReferencia: 'colheitadeira',
              maquinas: 1.7,
            },
          ],
        }),
      ),
    );
    irPara('Lavoura');
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).getByText('Máquinas teóricas (1 trator a cada 200 ha)')).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText('Máquinas teóricas (1 colheitadeira a cada 1.500 ha)')).toBeInTheDocument();
    const soma = within(potencialRecolhido).getByText('Máquinas teóricas somadas');
    expect(soma.nextElementSibling).toHaveTextContent('12,7');
  });

  it('sem unidade ou sem colheita, não mostra número', () => {
    abrir(municipio(potencial({ unidadeDaQuantidade: null, unidadeDaProdutividade: null, produtividade: null })));
    irPara('Lavoura');
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).queryByText(/toneladas/)).not.toBeInTheDocument();
    expect(within(potencialRecolhido).queryByText(/t\/ha/)).not.toBeInTheDocument();
    expect(within(potencialRecolhido).getAllByText('não disponível').length).toBeGreaterThanOrEqual(2);
  });
});

/** Um município com lavoura, estrutura e carteira — a ficha cheia. */
const comLavouraEEstrutura = (extra: Partial<IndicadoresDoMunicipio> = {}) =>
  municipio(potencial({ areaPlantadaHectares: 23_000 }), undefined, {
    ...extra,
    producao: {
      ano: 2024,
      areaPlantadaHectares: 37_226,
      areaColhidaHectares: 36_000,
      valorDaProducaoMilReais: 468_600,
      culturasComArea: 9,
    },
    estrutura: {
      anoDoCenso: 2017,
      tratores: 422,
      tratoresAbaixoDe100Cv: 300,
      tratoresDe100CvEMais: null,
      estabelecimentosComTrator: 150,
      estabelecimentos: 253,
      faixasDeArea: [{ ordem: 1, rotulo: 'até 10 ha', estabelecimentos: null }],
      anoDoRebanho: 2024,
      bovinos: 12_400,
      areaKm2: 900,
      usinas: [],
      tratoresPorMilKm2: 468,
      capacidadeDeEtanolM3Dia: null,
    },
    cobertura: { clientes: 12, vinculos: 20, vinculosComCadencia: 18, cobertos: 11, foraDaCadencia: 5, nuncaContatados: 2, semCadencia: 2, pendentes: 7, percentualPendente: 38.9 },
    vendas: { clientesQueCompraram: 4, valorLiquido: 1_250_000, maquina: 900_000, peca: 200_000, servico: 150_000, outros: 0, posVenda: 350_000 },
    responsaveisPelasCarteiras: [{ nome: 'Carteira Norte', natureza: 'Pessoa', vinculos: 12, carteiras: 1 }],
  });

describe('DetalheDoMunicipio — o cabeçalho e as abas da maquete', () => {
  it('o cabeçalho tem o nome, "Selecionado", a sub-região e a loja — o código IBGE mora na dica', () => {
    const aoFechar = vi.fn();
    abrir(comLavouraEEstrutura(), [], true, aoFechar);

    const ficha = document.querySelector<HTMLElement>('[data-bloco="ficha-do-municipio"]')!;
    expect(screen.getByRole('heading', { level: 2, name: 'Ribeirão Preto' })).toBeInTheDocument();
    expect(ficha).toHaveTextContent('Selecionado');
    expect(ficha.querySelector('.terr-ficha-local')).toHaveTextContent(/Norte\s*•\s*Loja Ribeirão Preto/);
    expect(ficha.querySelector('.terr-ficha-local')).not.toHaveTextContent('3543402');

    const dica = textoDaDica('Código IBGE e área de atuação de Ribeirão Preto');
    expect(dica).toContain('IBGE 3543402');
    expect(dica).toContain('ADR · sub-região Norte');

    fireEvent.click(screen.getByRole('button', { name: 'Fechar a ficha de Ribeirão Preto' }));
    expect(aoFechar).toHaveBeenCalled();
  });

  it('as cinco abas da maquete, e abre na Visão geral', () => {
    abrir(comLavouraEEstrutura());

    const abas = within(screen.getByRole('tablist', { name: 'Leituras de Ribeirão Preto' })).getAllByRole('tab');
    expect(abas.map((a) => a.textContent)).toEqual(['Visão geral', 'Lavoura', 'Estrutura', 'Oportunidades', 'Histórico']);
    expect(abas[0]).toHaveAttribute('aria-selected', 'true');
    expect(painelAtivo()).toHaveAttribute('aria-labelledby', abas[0].id);
    // Os outros painéis existem — o `aria-controls` aponta para algo —, escondidos;
    // o conteúdo deles só monta quando a aba abre pela primeira vez.
    expect(document.querySelectorAll('[role="tabpanel"][hidden]')).toHaveLength(4);
    expect(document.querySelector('[data-aba-da-ficha="lavoura"]')).toBeEmptyDOMElement();
    irPara('Lavoura');
    irPara('Visão geral');
    expect(document.querySelector('[data-aba-da-ficha="lavoura"]')).not.toBeEmptyDOMElement();
  });

  it('as abas andam pelo teclado: setas dão a volta, Home e End vão às pontas', () => {
    abrir(comLavouraEEstrutura());
    const aba = (nome: string) => screen.getByRole('tab', { name: nome });
    const teclar = (key: string) => fireEvent.keyDown(document.activeElement!, { key });

    aba('Visão geral').focus();
    teclar('ArrowRight');
    expect(aba('Lavoura')).toHaveAttribute('aria-selected', 'true');
    expect(aba('Lavoura')).toHaveFocus();
    // SÓ A ABA ATIVA ENTRA NA ORDEM DO TAB.
    expect(aba('Lavoura')).toHaveAttribute('tabindex', '0');
    expect(aba('Visão geral')).toHaveAttribute('tabindex', '-1');
    expect(painelAtivo()).toHaveAttribute('data-aba-da-ficha', 'lavoura');

    teclar('End');
    expect(aba('Histórico')).toHaveFocus();
    expect(aba('Histórico')).toHaveAttribute('aria-selected', 'true');

    teclar('ArrowRight');
    expect(aba('Visão geral')).toHaveFocus();

    teclar('ArrowLeft');
    expect(aba('Histórico')).toHaveFocus();

    teclar('Home');
    expect(aba('Visão geral')).toHaveAttribute('aria-selected', 'true');
  });
});

describe('DetalheDoMunicipio — a Visão geral', () => {
  it('o resumo executivo tem os quatro números do município, e a variação diz por que não há', () => {
    abrir(comLavouraEEstrutura());
    const visao = painelAtivo();

    const mini = (id: string) => visao.querySelector<HTMLElement>(`[data-mini="${id}"]`)!;
    expect(mini('vendas')).toHaveTextContent('R$ 1,3 mi');
    expect(mini('posVenda')).toHaveTextContent('R$ 350 mil');
    expect(mini('maquinas')).toHaveTextContent('7');
    // "Clientes ativos" da maquete vira "Clientes com endereço" (decisão 2).
    expect(mini('clientes')).toHaveTextContent('Clientes com endereço');
    expect(mini('clientes')).toHaveTextContent('12');
    expect(visao).not.toHaveTextContent('Clientes ativos');

    // O período é o da página, e o seletor diz qual.
    expect((screen.getByRole('combobox', { name: 'Período do resumo' }) as HTMLSelectElement).disabled).toBe(true);
    expect(screen.getByRole('combobox', { name: 'Período do resumo' })).toHaveTextContent('Ano fiscal');
  });

  it('vendas e pós-venda comparam com o mesmo trecho do ano anterior; parque e clientes dizem por que não', () => {
    abrir(
      municipio(potencial({ areaPlantadaHectares: 23_000 }), undefined, {
        vendas: { clientesQueCompraram: 4, valorLiquido: 1_250_000, maquina: 900_000, peca: 200_000, servico: 150_000, outros: 0, posVenda: 350_000 },
        vendasNoPeriodoAnterior: { clientesQueCompraram: 3, valorLiquido: 1_000_000, maquina: 800_000, peca: 120_000, servico: 80_000, outros: 0, posVenda: 200_000 },
      }),
      [],
      true,
      () => {},
      { periodoAnterior: PERIODO_ANTERIOR },
    );
    const mini = (id: string) => document.querySelector<HTMLElement>(`[data-mini="${id}"]`)!;

    // 1.250.000 / 1.000.000 = +25%; 350.000 / 200.000 = +75%.
    expect(mini('vendas')).toHaveTextContent(/↑ \+25%/);
    expect(mini('posVenda')).toHaveTextContent(/↑ \+75%/);
    expect(textoDaDica('A variação de vendas no período')).toContain('nov/2024 a ago/2025, o mesmo trecho do ano anterior');

    expect(textoDaDica('Por que a variação de máquinas teóricas não aparece')).toMatch(/número estrutural/);
    expect(textoDaDica('Por que a variação de clientes com endereço não aparece')).toMatch(/cadastro de hoje/);
  });

  it('sem a janela anterior coberta, a variação de vendas diz o motivo do servidor — e não vira queda', () => {
    abrir(comLavouraEEstrutura(), [], true, () => {}, {
      periodoAnterior: { ...PERIODO_ANTERIOR, vendasCobertas: false, motivoSemVendas: 'FRASE DO SERVIDOR — faturamento começa depois.' },
    });

    expect(document.querySelector('[data-mini="vendas"]')).not.toHaveTextContent('↓');
    expect(textoDaDica('Por que a variação de vendas no período não aparece')).toBe('FRASE DO SERVIDOR — faturamento começa depois.');
  });

  it('a lavoura lista as culturas do detalhe e "Outros" até o total da PAM', () => {
    abrir(comLavouraEEstrutura());
    const lavoura = document.querySelector<HTMLElement>('[data-bloco-da-ficha="lavoura"]')!;

    const linhas = [...lavoura.querySelectorAll<HTMLElement>('tbody tr')].map((tr) => tr.dataset.cultura);
    expect(linhas).toEqual(['Café (em grão) Total', 'Outros']);
    // 23.000 / 37.226 = 62%; os 14.226 ha restantes são 38%.
    expect(lavoura.querySelector('[data-cultura="Café (em grão) Total"]')).toHaveTextContent(/23\.000\s*62%/);
    expect(lavoura.querySelector('[data-cultura="Outros"]')).toHaveTextContent(/14\.226\s*38%/);

    const dica = textoDaDica('De onde vem a lavoura do município');
    expect(dica).toMatch(/só das culturas com regra de potencial/);
    expect(dica).toMatch(/de 9 com área divulgada/);
  });

  it('cada cultura entra no ÚLTIMO ANO em que a área dela foi divulgada, e não só o ano mais recente (issue 152)', async () => {
    // O AMENDOIM SÓ TEM ÁREA EM 2023: com o ano mais recente da lista ele sumiria da lavoura — pela regra da
    // issue 152, ele entra com a área de 2023, e a dica diz que os anos são de 2023 a 2024.
    const comAmendoimSoEm2023: HistoricoDoMunicipio = {
      ...HISTORICO,
      lavoura: [
        {
          ...HISTORICO.lavoura[0],
          culturas: [
            ...HISTORICO.lavoura[0].culturas,
            { produtoCodigoIbge: 40101, produtoNome: 'Amendoim (em casca)', areaPlantadaHectares: 5_000, areaColhidaHectares: 5_000, valorDaProducaoMilReais: 1 },
          ],
        },
        HISTORICO.lavoura[1],
      ],
    };
    await abrirComHistorico(comLavouraEEstrutura(), comAmendoimSoEm2023);
    const lavoura = document.querySelector<HTMLElement>('[data-bloco-da-ficha="lavoura"]')!;

    // CAFÉ 23.000, CANA 10.000 (2024, e não os 30.000 de 2023), AMENDOIM 5.000 (2023), SOJA 2.000.
    const linhas = [...lavoura.querySelectorAll<HTMLElement>('tbody tr')].map((tr) => tr.dataset.cultura);
    expect(linhas).toEqual(['Café (em grão) Total', 'Cana-de-açúcar', 'Amendoim (em casca)', 'Soja (em grão)', 'Outros']);
    expect(lavoura.querySelector('[data-cultura="Cana-de-açúcar"]')).toHaveTextContent(/10\.000/);
    expect(textoDaDica('De onde vem a lavoura do município')).toMatch(/último ano em que a área dela foi divulgada \(de 2023 a 2024\)/);
  });

  it('com o histórico lido, a lavoura traz TODAS as culturas do ano mais recente (issue 168)', async () => {
    await abrirComHistorico(comLavouraEEstrutura());
    const lavoura = document.querySelector<HTMLElement>('[data-bloco-da-ficha="lavoura"]')!;

    const linhas = [...lavoura.querySelectorAll<HTMLElement>('tbody tr')].map((tr) => tr.dataset.cultura);
    // AS QUATRO MAIORES, E "OUTROS" É O QUE FALTA ATÉ O TOTAL: milho e feijão, 1.226 ha.
    expect(linhas).toEqual(['Café (em grão) Total', 'Cana-de-açúcar', 'Soja (em grão)', 'Laranja', 'Outros']);
    expect(lavoura.querySelector('[data-cultura="Outros"]')).toHaveTextContent(/1\.226\s*3%/);
    expect(textoDaDica('De onde vem a lavoura do município')).toMatch(/todas as culturas que a PAM divulgou com área aqui \(6\)/);
    expect(obterHistoricoDoMunicipio).toHaveBeenCalledWith(expect.anything(), 3543402, FILTROS, expect.anything());
  });

  it('cultura com regra e área nula é "sem área divulgada (sigilo ou não cultivada)" — e não sigilo afirmado', () => {
    // O REPOSITÓRIO GERA A LINHA DA REGRA EM TODO MUNICÍPIO (revisão de
    // 24/09/2026): nula tanto onde o IBGE ocultou quanto onde a cultura não é
    // plantada. A dica dizia "sob sigilo do IBGE" nos dois casos.
    const m = comLavouraEEstrutura();
    m.potencial = [potencial({ areaPlantadaHectares: 23_000 }), potencial({ produtoCodigoIbge: 40140, areaPlantadaHectares: null })];
    abrir(m);

    const dica = textoDaDica('De onde vem a lavoura do município');
    expect(dica).toContain('1 cultura(s) sem área divulgada no município (sigilo do IBGE ou não cultivada)');
    expect(dica).not.toMatch(/sob sigilo/);
  });

  it('a estrutura agropecuária mostra o que existe e diz o que falta, sem inventar', () => {
    abrir(comLavouraEEstrutura());
    const estrutura = document.querySelector<HTMLElement>('[data-bloco-da-ficha="estrutura"]')!;

    expect(estrutura).toHaveTextContent(/253\s*propriedades rurais/);
    for (const [oQue, issue] of [
      ['a área total das propriedades', /issue 174/],
      ['o tamanho médio da propriedade', /issue 174/],
      ['a vocação agrícola', /issue 166/],
    ] as const) {
      expect(textoDaDica(`Por que ${oQue} não aparece`)).toMatch(issue);
    }
  });

  it('as oportunidades: potencial e máquinas sem dado com o motivo, cobertura real — e "Ver detalhes" leva à aba', () => {
    abrir(comLavouraEEstrutura());

    expect(textoDaDica('Por que o potencial incremental não aparece')).toMatch(/issue 70/);
    // AS MÁQUINAS POTENCIAIS SÃO A OPORTUNIDADE EM MÁQUINAS: o motivo é o do servidor, e não uma redação daqui.
    expect(textoDaDica('Por que as máquinas potenciais não aparece')).toBe(FRASE_SEM_VENDAS);
    // 11 de 18 vínculos no prazo.
    expect(document.querySelector('.terr-ficha-oport')).toHaveTextContent(/61,1%\s*cobertura atual/);

    fireEvent.click(screen.getByRole('button', { name: /Ver detalhes/ }));
    expect(screen.getByRole('tab', { name: 'Oportunidades' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Oportunidades' })).toHaveFocus();
  });
});

describe('DetalheDoMunicipio — Lavoura, Estrutura, Oportunidades e Histórico', () => {
  it('a aba Lavoura compara com a Região Tracbel e SP, aberta', () => {
    abrir(comLavouraEEstrutura());
    irPara('Lavoura');

    const lavoura = within(painelAtivo()).getByText(/A lavoura/).closest<HTMLElement>('section')!;
    expect(lavoura).toHaveAttribute('data-camada', 'lavoura');
    expect(lavoura.closest('details')).toBeNull();
    // 37.226 / 886.333 = 4,2%; / 8.000.000 = 0,5%.
    expect(lavoura).toHaveTextContent('4,2% da Região Tracbel');
    expect(lavoura).toHaveTextContent('0,5% de SP');
  });

  it('a aba Estrutura compara com a Região Tracbel e SP, e o sigilo é dito como sigilo', () => {
    abrir(comLavouraEEstrutura());
    irPara('Estrutura');

    const estrutura = painelAtivo().querySelector<HTMLElement>('[data-camada="estrutura"]')!;
    // 422 / 38.364 = 1,1%; / 120.000 = 0,4%.
    expect(estrutura).toHaveTextContent('1,1% da Região Tracbel');
    expect(estrutura).toHaveTextContent('0,4% de SP');

    const gatilho = within(estrutura).getByRole('button', { name: 'Por que os tratores de 100 cv e mais não aparece' });
    fireEvent.focus(gatilho);
    expect(screen.getByRole('tooltip')).toHaveTextContent(/Sigilo NÃO é zero/);
    expect(gatilho.closest('dd')).not.toHaveTextContent('0');
  });

  it('as evidências nascem RECOLHIDAS nas abas delas, e nada se perde dentro delas', () => {
    abrir(comLavouraEEstrutura());
    // O conteúdo de uma aba monta na primeira visita, e fica.
    irPara('Lavoura');
    irPara('Estrutura');
    irPara('Visão geral');

    const naAba = (aba: string) =>
      [...document.querySelectorAll<HTMLElement>(`[data-aba-da-ficha="${aba}"] [data-evidencia]`)].map((d) => d.dataset.evidencia);
    expect(naAba('lavoura')).toEqual(['potencial', 'fontes']);
    expect(naAba('estrutura')).toEqual(['faixas', 'usinas', 'carteira']);
    for (const d of document.querySelectorAll('[data-evidencia]')) expect(d.hasAttribute('open')).toBe(false);

    // O operacional de carteira continua inteiro — só mudou de casa.
    irPara('Estrutura');
    const carteira = abrirEvidencia('carteira');
    expect(carteira).toHaveTextContent('Carteira Norte');
    expect(carteira).toHaveTextContent('Cobertura de visita');
    expect(carteira).toHaveTextContent('7 (38,9%)');
    expect(carteira).toHaveTextContent('Pós-venda');
  });

  it('as máquinas conectadas vêm da telemetria, e sem nenhuma a linha diz isso em vez de mostrar zero', () => {
    abrir(
      comLavouraEEstrutura({
        parqueConectado: { maquinas: 12, comHorimetro: 10, semUsoHa30Dias: 3, horimetroMediano: 4200, referencia: '2026-09-25T10:00:00Z' },
      }),
    );
    irPara('Estrutura');

    const linha = painelAtivo().querySelector<HTMLElement>('[data-medida="parque-conectado"]')!;
    expect(linha).toHaveTextContent('12 com a última posição aqui (Operations Center)');
    expect(linha).toHaveTextContent('horímetro mediano 4.200 h');
    expect(linha).toHaveTextContent('3 de 10 sem hora nova nos 30 dias até 25/09/2026');

    cleanup();
    abrir(comLavouraEEstrutura());
    irPara('Estrutura');
    const vazia = painelAtivo().querySelector<HTMLElement>('[data-medida="parque-conectado"]')!;
    expect(vazia).toHaveTextContent('nenhuma máquina do parque do CRM com a última posição aqui');
    expect(vazia).not.toHaveTextContent(/\b0\b/);
  });

  it('sem denominador, a fatia não aparece — e não vira 0%', () => {
    abrir(comLavouraEEstrutura(), [], false);
    irPara('Lavoura');

    const lavoura = painelAtivo().querySelector<HTMLElement>('[data-camada="lavoura"]')!;
    expect(lavoura).not.toHaveTextContent('% da Região Tracbel');
    expect(lavoura).not.toHaveTextContent('0% de SP');
    // Mas o número continua lá.
    expect(lavoura).toHaveTextContent('37.226 ha');
  });

  it('a medida traz a própria procedência, e ela abre pelo teclado', () => {
    abrir(comLavouraEEstrutura());
    irPara('Lavoura');

    const dica = textoDaDica('De onde vem a área plantada');
    expect(dica).toContain('Fonte: IBGE/SIDRA');
    expect(dica).toContain('Tabela: 5457');
  });

  it('a aba Oportunidades abre com os quatro números de decisão, e a lista vazia diz o que falta', () => {
    abrir(comLavouraEEstrutura());
    irPara('Oportunidades');

    const decisao = painelAtivo().querySelector<HTMLElement>('[data-camada="decisao"]')!;
    for (const rotulo of ['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade'])
      expect(decisao).toHaveTextContent(rotulo);

    // OS TRÊS SEM DADO DIZEM O QUE O SERVIDOR DIZ (issue 69, parte A) — a mesma frase do topo da aba
    // Mercado, e não uma terceira redação escrita na ficha.
    expect(textoDaDica('Por que mercado anual não aparece')).toBe(FRASE_SEM_PRECO);
    expect(textoDaDica('Por que captura tracbel não aparece')).toBe(FRASE_SEM_VENDAS);
    expect(textoDaDica('Por que oportunidade não aparece')).toBe(FRASE_SEM_VENDAS);

    const lista = painelAtivo().querySelector<HTMLElement>('[data-bloco-da-ficha="lista-de-oportunidades"]')!;
    expect(within(lista).getAllByRole('columnheader').map((c) => c.textContent)).toEqual(['Oportunidade', 'Confiança', 'Origem']);
    expect(lista).toHaveTextContent('aguarda a confiança da #162');
    expect(lista).not.toHaveTextContent('#69');
    for (const nivel of ['Alta', 'Média', 'Baixa']) expect(lista).toHaveTextContent(nivel);
    const dica = textoDaDica('Por que não há oportunidades listadas');
    expect(dica).toMatch(/não com exemplos/);
    expect(dica).toMatch(/já existem \(issue 69/);
  });

  it('quando o recorte tem o número, o município não o herda — e diz por quê', () => {
    abrir(comLavouraEEstrutura(), [], true, () => {}, {
      numeros: {
        ...NUMEROS,
        mercadoAnual: { valor: 9_000_000, motivo: 'Nenhum', frase: '', parcial: false, categoriasSemPreco: [] },
        capturaPercentual: { valor: 12.5, motivo: 'Nenhum', frase: '' },
        oportunidade: { valor: 40, motivo: 'Nenhum', frase: '' },
      },
    });
    irPara('Oportunidades');

    expect(painelAtivo().querySelector('[data-camada="decisao"]')).not.toHaveTextContent('12,5');
    expect(textoDaDica('Por que captura tracbel não aparece')).toMatch(/existe para o recorte inteiro/);
    expect(textoDaDica('Por que oportunidade não aparece')).toMatch(/ainda não por município/);

    // O MERCADO ANUAL NÃO USA VENDAS (revisão de 27/09/2026): o que falta ao município é a demanda por categoria.
    const mercado = textoDaDica('Por que mercado anual não aparece');
    expect(mercado).toMatch(/demanda de cada categoria vezes o preço de referência/);
    expect(mercado).not.toMatch(/vendas em unidades/);
  });

  // O PREÇO DA MÁQUINA CHEGOU (issue 70, 27/09/2026): com a conta do município vinda do servidor, a ficha mostra o
  // mercado anual e o potencial incremental em reais — com o preço usado escrito e a marca de parcial ao lado.
  it('com o preço da máquina, a ficha mostra o mercado anual e o potencial incremental do município', () => {
    const precos = [{ categoriaCodigo: 'TRATOR', categoria: 'Trator', preco: 475_000, meses: 2, ultimoMes: '2026-09-01' }];
    abrir(
      municipio(potencial(), undefined, {
        numerosDeDecisao: {
          ...NUMEROS,
          mercadoAnual: { valor: 3_325_000, motivo: 'Nenhum', frase: '', parcial: true, categoriasSemPreco: ['Plantadeira'], precos },
          oportunidade: { valor: 2.4, motivo: 'Nenhum', frase: '' },
          potencialIncremental: { valor: 1_140_000, motivo: 'Nenhum', frase: '', parcial: false, categoriasSemPreco: [], precos },
        },
      }),
    );

    expect(document.querySelector('.terr-ficha-oport')).toHaveTextContent(/R\$ 1,1 mi\s*potencial incremental/);

    irPara('Oportunidades');
    const decisao = painelAtivo().querySelector<HTMLElement>('[data-camada="decisao"]')!;
    expect(decisao).toHaveTextContent('R$ 3,3 mi');
    expect(decisao).toHaveTextContent('parcial — sem preço de Plantadeira');
    expect(decisao).toHaveTextContent(/Trator: R\$ 475 mil \(mediana de 2 meses de notas, até 09\/2026\)/);
  });

  it('a aba Histórico sem os filtros da página não lê — e diz por quê', () => {
    abrir(comLavouraEEstrutura());
    irPara('Histórico');

    expect(painelAtivo()).toHaveTextContent('Histórico não lido');
    expect(obterHistoricoDoMunicipio).not.toHaveBeenCalled();
    expect(textoDaDica('Por que o histórico do município não foi lido')).toMatch(/filtros de alcance da página/);
  });

  it('a aba Histórico mostra as vendas por ano fiscal, o mesmo trecho e a lavoura ano a ano', async () => {
    await abrirComHistorico(comLavouraEEstrutura());
    irPara('Histórico');

    const vendas = painelAtivo().querySelector<HTMLElement>('[data-historico="vendas"]')!;
    const linhas = [...vendas.querySelectorAll<HTMLElement>('tbody tr')];
    // O ANO MAIS RECENTE PRIMEIRO, e o mesmo trecho do anterior por último.
    expect(linhas.map((l) => l.dataset.anoFiscal)).toEqual(['2026', '2025', '2025']);
    expect(linhas[0]).toHaveTextContent(/FY26\s*nov\/2025 a ago\/2026 · em curso/);
    expect(linhas[0]).toHaveTextContent('R$ 1,3 mi');
    expect(linhas[2]).toHaveAttribute('data-mesmo-trecho', 'true');
    expect(linhas[2]).toHaveTextContent(/o mesmo trecho/);

    // REAIS E UNIDADES EM COLUNAS SEPARADAS; o ano que o ART não cobre fica com o traço e o motivo.
    expect(linhas[0]).toHaveTextContent('6');
    expect(within(linhas[1]).getByRole('button', { name: 'Por que as máquinas vendidas no FY25 não aparece' })).toBeInTheDocument();
    expect(painelAtivo().querySelector('[data-comparacao-do-historico]')).toHaveTextContent(
      'FY26 até ago/2026 contra FY25 no mesmo trecho: +25% em reais.',
    );

    const lavoura = painelAtivo().querySelector<HTMLElement>('[data-historico="lavoura"]')!;
    expect([...lavoura.querySelectorAll<HTMLElement>('tbody tr')].map((l) => l.dataset.anoDaPam)).toEqual(['2024', '2023']);
    expect(lavoura).toHaveTextContent(/Café \(em grão\) Total 23\.000 ha · Cana-de-açúcar 10\.000 ha · Soja \(em grão\) 2\.000 ha · mais 3/);

    // A COBERTURA NÃO TEM SÉRIE, e diz por quê; nenhuma linha é desenhada.
    expect(painelAtivo()).toHaveTextContent('Sem série histórica');
    expect(painelAtivo().querySelector('canvas')).toBeNull();
    expect(textoDaDica('Por que a cobertura de visita não tem histórico')).toMatch(/medida no instante/);
  });

  it('o histórico de outro município não aparece na ficha deste', async () => {
    await abrirComHistorico(comLavouraEEstrutura(), { ...HISTORICO, codigoIbge: 3500000 });
    irPara('Histórico');

    expect(painelAtivo().querySelector('[data-historico="vendas"]')).toBeNull();
    expect(painelAtivo()).toHaveTextContent('Lendo o histórico de Ribeirão Preto');
  });

  it('nenhum `title=` cru na ficha', () => {
    abrir(comLavouraEEstrutura());
    expect([...document.querySelectorAll('[title]')]).toEqual([]);
  });
});

/**
 * NENHUMA MEDIDA DA FICHA SUMIU (decisão 3 do usuário).
 *
 * A lista abaixo é a da ficha ANTES da fase 4 — títulos, rótulos das medidas,
 * cabeçalhos e detalhes recolhidos, tirada da ficha de três camadas montada com
 * esta mesma amostra (23/09/2026). A ficha em abas tem de ter cada um deles, em
 * alguma aba.
 */
const MEDIDAS_ANTES_DA_FASE_4 = [
  'O que este município decide',
  'Demanda anual',
  'Mercado anual',
  'Captura Tracbel',
  'Oportunidade',
  'A lavoura (2024)',
  'Área plantada',
  'Área colhida',
  'Valor da produção',
  'Culturas com área divulgada',
  'O que já existe para mecanizar',
  'Tratores (2017)',
  'Menos de 100 cv',
  'De 100 cv e mais',
  'Propriedades',
  'Propriedades com trator',
  'Rebanho bovino (2024)',
  'Área do município',
  'Potencial por cultura',
  'Área plantada de Café (em grão) Total (2024)',
  'Área colhida da mesma cultura',
  'Quantidade produzida',
  'Produtividade',
  'Valor da produção dela',
  'Máquinas teóricas (1 3036N a cada 10 ha)',
  'Parque teórico do município',
  'Propriedades por tamanho',
  'até 10 ha',
  'Usinas de etanol',
  'Usina X',
  'Quem atende, cobertura e vendas',
  'Quem atende este município',
  'Responsável no CRM',
  'Natureza',
  'Vínculos aqui',
  'Carteiras',
  'Cobertura de visita',
  'Elegíveis',
  'Cobertos',
  'Fora da cadência',
  'Nunca contatados',
  'Pendentes',
  'Vendas no período',
  'Máquina',
  'Peça',
  'Serviço',
  'Outros',
  'Total líquido',
  'Pós-venda',
  'Fontes e competências',
];

describe('DetalheDoMunicipio — nada sai (decisão 3)', () => {
  it('toda medida da ficha de antes continua em alguma aba', () => {
    const m = comLavouraEEstrutura();
    m.estrutura.faixasDeArea = [{ ordem: 1, rotulo: 'até 10 ha', estabelecimentos: 5 }];
    m.estrutura.usinas = [{ razaoSocial: 'Usina X', capacidadeM3Dia: 100 }];
    abrir(m, [], false);
    // Uma pessoa passa pelas cinco abas; o conteúdo de cada uma monta na
    // primeira visita e fica no documento.
    for (const aba of ['Lavoura', 'Estrutura', 'Oportunidades', 'Histórico', 'Visão geral']) irPara(aba);

    // O MESMO SELETOR da lista de antes — títulos, rótulos, cabeçalhos,
    // detalhes e os rótulos dos cartões de decisão —, em TODAS as abas.
    const agora = new Set(
      [...document.querySelectorAll('[data-bloco="ficha-do-municipio"] :is(dt, th, summary, h3, h4, .dash-kpi-rotulo)')].map((n) =>
        (n.textContent ?? '').replace(/\s+/g, ' ').trim(),
      ),
    );

    const sumiram = MEDIDAS_ANTES_DA_FASE_4.filter((medida) => !agora.has(medida));
    expect(sumiram).toEqual([]);
  });
});
