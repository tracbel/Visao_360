/**
 * A tabela de municípios (issue 170, parte A).
 *
 * O QUE ESTÁ SOB PROVA É A CONFERÊNCIA: a soma das linhas tem de ser o total da
 * consulta — é o que o documento 32 compara com o SQL. Se não fechar, algo sumiu
 * ou foi contado duas vezes.
 */

import { fireEvent, render, screen, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ARARAQUARA, CAFELANDIA, municipioDeTeste } from '../../testes/territorio';
import { TabelaDeMunicipios } from './TabelaDeMunicipios';
import { calcularTotais } from './totaisDaAdr';

const DA_ADR = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' });
const FORA_DA_ADR = municipioDeTeste({ codigoIbge: ARARAQUARA, nome: 'Araraquara', pertenceAAdr: false });

const FORA_DO_MAPA = [
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
];

function montar(parcial: Partial<Parameters<typeof TabelaDeMunicipios>[0]> = {}) {
  const aoSelecionar = vi.fn();
  render(
    <TabelaDeMunicipios
      municipios={[DA_ADR, FORA_DA_ADR]}
      daAdr={[DA_ADR]}
      foraDoMapa={FORA_DO_MAPA}
      totais={calcularTotais([DA_ADR])}
      selecionado={null}
      aoSelecionar={aoSelecionar}
      territorioNaoCarregado={false}
      semFiltro
      {...parcial}
    />,
  );
  return { aoSelecionar };
}

/** A linha inteira como texto, para conferir a soma sem depender da posição da coluna. */
function linha(rotulo: string): string {
  const celula = screen.getByText(rotulo);
  return celula.closest('tr')!.textContent ?? '';
}

describe('a tabela de municípios', () => {
  it('lista os municípios da ADR e deixa clicar num deles', () => {
    const { aoSelecionar } = montar();

    fireEvent.click(screen.getByRole('button', { name: 'Cafelândia' }));
    expect(aoSelecionar).toHaveBeenCalledWith(CAFELANDIA);
  });

  it('o total da ADR soma só os municípios da ADR', () => {
    montar();
    // 18 elegíveis, 11 no prazo, 7 pendentes — os do único município da ADR.
    expect(linha('Total da ADR')).toContain('18');
    expect(linha('Total da ADR')).toContain('11');
  });

  it('o total da consulta soma o mapa e o que ficou fora dele', () => {
    montar();
    // 18 (ADR) + 18 (fora da ADR, mas no mapa) + 2 (fora do mapa) = 38 elegíveis.
    const total = screen.getByText('Total da consulta').closest('tr')!;
    expect(within(total).getByText('38')).toBeInTheDocument();
  });

  it('com filtro, os grupos de fora não aparecem — o total seria de outro recorte', () => {
    montar({ semFiltro: false });

    expect(screen.queryByText('São Paulo fora da ADR')).not.toBeInTheDocument();
    expect(screen.queryByText('Total da consulta')).not.toBeInTheDocument();
    expect(screen.getByText('Total da ADR (filtro)')).toBeInTheDocument();
  });

  it('território não carregado troca o total da ADR por uma explicação, e não por zeros', () => {
    montar({ territorioNaoCarregado: true, daAdr: [], totais: calcularTotais([]) });

    expect(screen.getByText(/território não carregado neste banco/)).toBeInTheDocument();
    // A linha da ADR some; o TOTAL DA CONSULTA fica, e é o que prova que as vendas
    // continuam no banco — 18 + 18 do mapa mais os 2 do grupo fora dele.
    const total = screen.getByText('Total da consulta').closest('tr')!;
    expect(within(total).getByText('38')).toBeInTheDocument();
  });

  it('município sem parque mostra traço, e o motivo fica no lugar do número', () => {
    const semParque = municipioDeTeste({
      codigoIbge: CAFELANDIA,
      nome: 'Cafelândia',
      potencialEstrutural: {
        parqueDeMaquinas: null,
        demandaAnualDeMaquinas: null,
        areaUtilHectares: null,
        estimativa: false,
        motivoSemParque: 'SemArea',
        motivoSemDemanda: 'SemArea',
      },
    });
    montar({ daAdr: [semParque], municipios: [semParque], totais: calcularTotais([semParque]) });

    const celula = screen.getByTitle('área plantada não divulgada aqui (sigilo do IBGE)');
    expect(celula).toHaveTextContent('—');
  });
});
