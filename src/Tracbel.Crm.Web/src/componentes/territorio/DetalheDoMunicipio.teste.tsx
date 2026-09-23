/**
 * A ficha do município — issue 152, reorganizada em três camadas na fase T4.
 *
 * O QUE ESTE TESTE SEMPRE PRENDEU continua preso: a cultura diz o ano dela, a
 * quantidade vem com a unidade que o servidor mandou, a produtividade aparece ao
 * lado da de São Paulo, e sem unidade ou sem colheita não se mostra número.
 *
 * O CONTRATO MUDOU NA T4, e foi ajustado de propósito: o detalhe por cultura
 * saiu da primeira camada e virou EVIDÊNCIA recolhida (documento 50, §6). O
 * conteúdo é o mesmo — e este teste abre o detalhe antes de conferir, que é o
 * que uma pessoa faria.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type {
  CulturaNoEstado,
  IndicadoresDoMunicipio,
  PotencialEstruturalDoMunicipio,
  PotencialTerritorial,
  ProcedenciasDoTerritorio,
  RegraDePotencialAplicada,
  TotaisDaRegiaoTracbel,
  TotaisDoEstado,
} from '../../tipos/territorio';
import { DetalheDoMunicipio } from './DetalheDoMunicipio';

const REGRA: RegraDePotencialAplicada = {
  produtoCodigoIbge: 40139,
  produtoNome: 'Café (em grão) Total',
  hectaresPorMaquina: 10,
  modeloDeReferencia: '3036N',
  situacao: 'AConfirmar',
  justificativa: 'exemplo do gerente comercial',
  vigenteDesde: '2026-09-13',
  anosDeRenovacao: null,
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
};

function potencial(parcial: Partial<PotencialTerritorial> = {}): PotencialTerritorial {
  return {
    produtoCodigoIbge: 40139,
    areaPlantadaHectares: 70,
    maquinasTeoricas: 7,
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

function abrir(
  m: IndicadoresDoMunicipio,
  culturasNoEstado: CulturaNoEstado[] = [],
  denominadores = true,
) {
  render(
    <DetalheDoMunicipio
      municipio={m}
      regras={[REGRA]}
      culturasNoEstado={culturasNoEstado}
      regiaoTracbel={denominadores ? REGIAO : null}
      estado={denominadores ? SAO_PAULO : null}
      procedencias={PROCEDENCIAS}
      aoFechar={() => {}}
    />,
  );
}

/** Abre o detalhe recolhido — nada se perdeu, mudou de camada. */
function abrirEvidencia(qual: string): HTMLElement {
  const bloco = document.querySelector<HTMLElement>(`[data-evidencia="${qual}"]`)!;
  fireEvent.click(bloco.querySelector('summary')!);
  return bloco;
}

describe('DetalheDoMunicipio — o que a ficha sempre disse', () => {
  it('diz o ano da cultura, a quantidade com a unidade e a produtividade ao lado da de SP', () => {
    abrir(municipio(potencial()), [CAFE_EM_SP]);
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).getByText(/Área plantada de Café \(em grão\) Total/)).toHaveTextContent('(2024)');
    expect(within(potencialRecolhido).getByText('200 toneladas')).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText(/3,33 t\/ha/)).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText(/SP 1,76 t\/ha/)).toBeInTheDocument();
  });

  it('mostra o parque do motor, e diz por que a demanda anual não saiu', () => {
    abrir(municipio(potencial()));
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).getByText(/70 ha úteis/)).toBeInTheDocument();
    expect(within(potencialRecolhido).getByText(/ainda não foi confirmada pelo comercial/)).toBeInTheDocument();

    // O MOTIVO DA DEMANDA subiu para a camada executiva, e abre pelo teclado.
    fireEvent.focus(screen.getByRole('button', { name: 'Por que a demanda anual não aparece' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent(/de quantos em quantos anos a máquina é trocada/);
  });

  it('sem regra de potencial vigente, a ficha não inventa parque', () => {
    abrir(municipio(potencial(), null));
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).queryByText('Parque teórico do município')).not.toBeInTheDocument();
  });

  it('sem unidade ou sem colheita, não mostra número', () => {
    abrir(municipio(potencial({ unidadeDaQuantidade: null, unidadeDaProdutividade: null, produtividade: null })));
    const potencialRecolhido = abrirEvidencia('potencial');

    expect(within(potencialRecolhido).queryByText(/toneladas/)).not.toBeInTheDocument();
    expect(within(potencialRecolhido).queryByText(/t\/ha/)).not.toBeInTheDocument();
    expect(within(potencialRecolhido).getAllByText('não disponível').length).toBeGreaterThanOrEqual(2);
  });
});

describe('DetalheDoMunicipio — as três camadas (documento 50, §6)', () => {
  const comLavouraEEstrutura = () =>
    municipio(potencial(), undefined, {
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
      responsaveisPelasCarteiras: [{ nome: 'Carteira Norte', natureza: 'Pessoa', vinculos: 12, carteiras: 1 }],
    });

  it('a camada executiva mostra os quatro números de decisão, sempre aberta', () => {
    abrir(comLavouraEEstrutura());

    const executiva = document.querySelector<HTMLElement>('[data-camada="executiva"]')!;
    expect(executiva.closest('details')).toBeNull();
    for (const rotulo of ['Demanda anual', 'Mercado anual', 'Captura Tracbel', 'Oportunidade'])
      expect(executiva).toHaveTextContent(rotulo);
  });

  it('a camada intermediária fica aberta e compara com a Região Tracbel e SP', () => {
    abrir(comLavouraEEstrutura());

    const lavoura = document.querySelector<HTMLElement>('[data-camada="lavoura"]')!;
    expect(lavoura.closest('details')).toBeNull();
    // 37.226 / 886.333 = 4,2%; / 8.000.000 = 0,5%.
    expect(lavoura).toHaveTextContent('4,2% da Região Tracbel');
    expect(lavoura).toHaveTextContent('0,5% de SP');

    const estrutura = document.querySelector<HTMLElement>('[data-camada="estrutura"]')!;
    // 422 / 38.364 = 1,1%; / 120.000 = 0,4%.
    expect(estrutura).toHaveTextContent('1,1% da Região Tracbel');
    expect(estrutura).toHaveTextContent('0,4% de SP');
  });

  it('a camada de evidência nasce RECOLHIDA, e nada se perde dentro dela', () => {
    abrir(comLavouraEEstrutura());

    const recolhidos = [...document.querySelectorAll<HTMLElement>('[data-evidencia]')];
    expect(recolhidos.map((d) => d.dataset.evidencia)).toEqual([
      'potencial',
      'faixas',
      'usinas',
      'carteira',
      'fontes',
    ]);
    for (const d of recolhidos) expect(d.hasAttribute('open')).toBe(false);

    // O operacional de carteira continua inteiro — só mudou de camada.
    const carteira = abrirEvidencia('carteira');
    expect(carteira).toHaveTextContent('Carteira Norte');
    expect(carteira).toHaveTextContent('Cobertura de visita');
    expect(carteira).toHaveTextContent('Vendas no período');
    expect(carteira).toHaveTextContent('Pós-venda');
  });

  it('sigilo do IBGE é dito como sigilo, e nunca como zero', () => {
    abrir(comLavouraEEstrutura());

    const estrutura = document.querySelector<HTMLElement>('[data-camada="estrutura"]')!;
    const gatilho = within(estrutura).getByRole('button', { name: 'Por que os tratores de 100 cv e mais não aparece' });
    fireEvent.focus(gatilho);

    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent(/Sigilo NÃO é zero/);
    expect(gatilho.closest('dd')).not.toHaveTextContent('0');
  });

  it('sem denominador, a fatia não aparece — e não vira 0%', () => {
    abrir(comLavouraEEstrutura(), [], false);

    const lavoura = document.querySelector<HTMLElement>('[data-camada="lavoura"]')!;
    expect(lavoura).not.toHaveTextContent('% da Região Tracbel');
    expect(lavoura).not.toHaveTextContent('0% de SP');
    // Mas o número continua lá.
    expect(lavoura).toHaveTextContent('37.226 ha');
  });

  it('a medida traz a própria procedência, e ela abre pelo teclado', () => {
    abrir(comLavouraEEstrutura());

    fireEvent.focus(screen.getByRole('button', { name: 'De onde vem a área plantada' }));
    const dica = screen.getByRole('tooltip');
    expect(dica).toHaveTextContent('Fonte: IBGE/SIDRA');
    expect(dica).toHaveTextContent('Tabela: 5457');
  });

  it('nenhum `title=` cru na ficha', () => {
    abrir(comLavouraEEstrutura());
    expect([...document.querySelectorAll('[title]')]).toEqual([]);
  });
});
