/**
 * A ficha do município (issue 152). O que este teste prende: a cultura diz o ano dela; a quantidade vem com a
 * unidade que o servidor mandou; a produtividade aparece ao lado da de São Paulo no mesmo ano; e, sem unidade ou
 * sem colheita, não se mostra número.
 */

import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { CulturaNoEstado, IndicadoresDoMunicipio, PotencialTerritorial, RegraDePotencialAplicada } from '../../tipos/territorio';
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

function municipio(p: PotencialTerritorial): IndicadoresDoMunicipio {
  return {
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

describe('DetalheDoMunicipio', () => {
  it('diz o ano da cultura, a quantidade com a unidade e a produtividade ao lado da de SP', () => {
    render(<DetalheDoMunicipio municipio={municipio(potencial())} regras={[REGRA]} culturasNoEstado={[CAFE_EM_SP]} aoFechar={() => {}} />);

    expect(screen.getByText(/Área plantada de Café \(em grão\) Total/)).toHaveTextContent('(2024)');
    expect(screen.getByText('200 toneladas')).toBeInTheDocument();
    expect(screen.getByText(/3,33 t\/ha/)).toBeInTheDocument();
    expect(screen.getByText(/SP 1,76 t\/ha/)).toBeInTheDocument();
  });

  it('sem unidade ou sem colheita, não mostra número', () => {
    render(
      <DetalheDoMunicipio
        municipio={municipio(potencial({ unidadeDaQuantidade: null, unidadeDaProdutividade: null, produtividade: null }))}
        regras={[REGRA]}
        culturasNoEstado={[]}
        aoFechar={() => {}}
      />,
    );

    expect(screen.queryByText(/toneladas/)).not.toBeInTheDocument();
    expect(screen.queryByText(/t\/ha/)).not.toBeInTheDocument();
    expect(screen.getAllByText('não disponível').length).toBeGreaterThanOrEqual(2);
  });
});
