/**
 * Um cartão de mapa (issue 170, parte A).
 *
 * O mapa de cobertura serve de prova da casca que os quatro compartilham: o
 * alternador troca a medida, a legenda acompanha, e a linha do cursor diz o
 * número do município sob ele.
 */

import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { CAFELANDIA, municipioDeTeste } from '../../../testes/territorio';
import type { LigacaoDoMapa } from './CartaoDeMapa';
import { MapaDeCobertura } from './MapaDeCobertura';
import { calcularTotais } from '../totaisDaAdr';

const MUNICIPIO = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia' });

function ligacao(parcial: Partial<LigacaoDoMapa> = {}): LigacaoDoMapa {
  return {
    enquadramento: { largura: 520, altura: 400, oeste: -50, norte: -20, escala: 100, cossenoDaLatitude: 0.94, margem: 10 },
    poligonos: [{ codigo: CAFELANDIA, nome: 'Cafelândia', caminho: 'M0,0 L10,0 L10,10 Z' }],
    adr: new Set([CAFELANDIA]),
    porCodigo: new Map([[CAFELANDIA, MUNICIPIO]]),
    nomeDoPoligono: new Map([[CAFELANDIA, 'Cafelândia']]),
    selecionado: null,
    aoSelecionar: vi.fn(),
    emFoco: null,
    aoPassar: vi.fn(),
    ...parcial,
  };
}

function montar(parcial: Partial<LigacaoDoMapa> = {}) {
  render(
    <MapaDeCobertura
      ligacao={ligacao(parcial)}
      totais={calcularTotais([MUNICIPIO])}
      coberturaDaAdr={(100 * 11) / 18}
      classificacao={{
        indicador: 'coberturaDeVisita',
        situacao: 'RegraComercialProvisoria',
        selo: 'Regra provisória',
        motivo: 'a cadência ainda não foi confirmada',
      }}
    />,
  );
}

/** A linha do resumo, acima do alternador. */
const resumo = () => document.querySelector('.terr-mapa-resumo')!.textContent ?? '';

/** A linha que segue o cursor, abaixo do mapa. */
const linhaDoCursor = () => document.querySelector('.terr-mapa-foco')!.textContent ?? '';

describe('o cartão do mapa de cobertura', () => {
  // O CONTRATO MUDOU NA FASE T2: o motivo do selo saía num `title=`. Agora o
  // próprio selo é o gatilho de uma dica, e abre pelo teclado (issue 167).
  it('mostra o selo de como ler o número, e o motivo abre pelo teclado', () => {
    montar();

    const selo = screen.getByRole('button', { name: 'Por que este número está marcado como Regra provisória' });
    expect(selo).toHaveTextContent('Regra provisória');

    fireEvent.focus(selo);
    expect(screen.getByRole('tooltip')).toHaveTextContent('a cadência ainda não foi confirmada');
  });

  it('o resumo traz a cobertura em destaque e as três parcelas ao lado', () => {
    montar();
    // Escopado ao resumo: o mesmo texto aparece no `<title>` de cada polígono do
    // SVG, que é o que o leitor de tela lê ao entrar no mapa.
    //
    // O RESUMO DEIXOU DE SER UMA FRASE CORRIDA (fase T4.9 — maquete): a
    // cobertura virou o número do cartão e as três parcelas viraram metadados ao
    // lado dele. NENHUMA SAIU, e é isso que este teste protege — antes ele
    // afirmava a pontuação da frase, que é o que menos importa aqui.
    const texto = resumo();
    expect(texto).toMatch(/61,1%/);
    expect(texto).toMatch(/dos vínculos elegíveis estão no prazo/);
    expect(texto).toMatch(/18\s*elegíveis/);
    expect(texto).toMatch(/11\s*no prazo/);
    expect(texto).toMatch(/pendentes/);
  });

  it('o alternador troca a medida, e a legenda troca junto', () => {
    montar();

    expect(screen.getByText(/verde é mais coberto/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: '% pendente' }));
    expect(screen.getByText(/vermelho é mais pendente/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'pendentes (qtd.)' }));
    expect(screen.getByText(/vínculos pendentes \(fora do prazo \+ nunca contatados\)/)).toBeInTheDocument();
  });

  // O CONTRATO MUDOU NA T2.1: o aviso era um parágrafo permanente embaixo do
  // mapa. Agora é a dica do título — mesmo texto, sem ocupar a primeira camada.
  it('a metodologia não ocupa a tela, e muda com a medida escolhida', () => {
    montar();

    // Nada disso está no corpo do cartão.
    expect(screen.queryByText(/Percentual compara municípios de tamanhos diferentes/)).not.toBeInTheDocument();
    expect(document.querySelector('.terr-mapa .terr-aviso')).toBeNull();

    const gatilho = screen.getByRole('button', { name: 'Fonte e método deste mapa' });
    fireEvent.focus(gatilho);
    expect(screen.getByRole('tooltip')).toHaveTextContent(/Percentual compara municípios de tamanhos diferentes/);
    expect(screen.getByRole('tooltip')).toHaveTextContent(/nenhum tipo de atividade está marcado como visita/);
    fireEvent.blur(gatilho);

    fireEvent.click(screen.getByRole('button', { name: 'pendentes (qtd.)' }));
    fireEvent.focus(screen.getByRole('button', { name: 'Fonte e método deste mapa' }));
    expect(screen.getByRole('tooltip')).toHaveTextContent(/Quantidade favorece cidades grandes/);
  });

  // O CONTRATO MUDOU NA T2.1: a linha dizia permanentemente "Passe o cursor…
  // nos três mapas" — instrução fixa em quatro cartões, e ainda errada: são
  // QUATRO mapas. Ela agora só fala quando há cursor.
  it('sem cursor, a linha fica calada — nada de instrução permanente', () => {
    montar();
    expect(linhaDoCursor()).toBe('');
    expect(document.body.textContent).not.toMatch(/três mapas/);
  });

  it('com o município em foco, a linha do cursor traz o detalhe dele', () => {
    montar({ emFoco: CAFELANDIA });
    expect(linhaDoCursor()).toMatch(/^Cafelândia — 18 elegíveis/);
  });

  it('município fora da ADR é dito como fora, e não como sem dado', () => {
    const fora = municipioDeTeste({ codigoIbge: CAFELANDIA, nome: 'Cafelândia', pertenceAAdr: false });
    montar({ emFoco: CAFELANDIA, porCodigo: new Map([[CAFELANDIA, fora]]) });

    expect(linhaDoCursor()).toBe('Cafelândia — fora da ADR');
  });
});
