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
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import type {
  CulturaNoEstado,
  IndicadoresDoMunicipio,
  NumerosDeDecisao,
  PotencialEstruturalDoMunicipio,
  PotencialTerritorial,
  ProcedenciasDoTerritorio,
  RegraDePotencialAplicada,
  TotaisDaRegiaoTracbel,
  TotaisDoEstado,
} from '../../tipos/territorio';
import { ProvedorDoPeriodo } from './carteira/periodo';
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

function abrir(m: IndicadoresDoMunicipio, culturasNoEstado: CulturaNoEstado[] = [], denominadores = true, aoFechar = () => {}) {
  render(
    <ProvedorDoPeriodo value={{ meses: 12, rotulo: '12 meses', intervalo: 'out/2025 a set/2026' }}>
      <DetalheDoMunicipio
        municipio={m}
        regras={[REGRA]}
        culturasNoEstado={culturasNoEstado}
        regiaoTracbel={denominadores ? REGIAO : null}
        estado={denominadores ? SAO_PAULO : null}
        procedencias={PROCEDENCIAS}
        numerosDeDecisao={NUMEROS}
        aoFechar={aoFechar}
      />
    </ProvedorDoPeriodo>,
  );
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
const comLavouraEEstrutura = () =>
  municipio(potencial({ areaPlantadaHectares: 23_000 }), undefined, {
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

    expect(textoDaDica('Por que a variação de vendas no período não aparece')).toMatch(/issue 69/);
    // O período é o da página, e o seletor diz qual.
    expect((screen.getByRole('combobox', { name: 'Período do resumo' }) as HTMLSelectElement).disabled).toBe(true);
    expect(screen.getByRole('combobox', { name: 'Período do resumo' })).toHaveTextContent('12 meses');
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
    expect(lista).toHaveTextContent('aguarda vendas por município (#69) e a confiança da #162');
    for (const nivel of ['Alta', 'Média', 'Baixa']) expect(lista).toHaveTextContent(nivel);
    expect(textoDaDica('Por que não há oportunidades listadas')).toMatch(/não com exemplos/);
  });

  it('a aba Histórico diz que a série não existe, e não desenha linha', () => {
    abrir(comLavouraEEstrutura());
    irPara('Histórico');

    expect(painelAtivo()).toHaveTextContent('Sem série histórica');
    expect(painelAtivo().querySelector('canvas')).toBeNull();
    expect(textoDaDica('Por que o histórico do município está vazio')).toMatch(/série histórica do município ainda não existe/);
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
  '3036N teóricos (1 a cada 10 ha)',
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
