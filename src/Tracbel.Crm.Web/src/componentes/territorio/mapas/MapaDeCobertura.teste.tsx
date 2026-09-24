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

  it('o resumo traz a cobertura em destaque, com o denominador verdadeiro, e as três parcelas na dica', () => {
    montar();
    // Escopado ao resumo: o mesmo texto aparece no `<title>` de cada polígono do
    // SVG, que é o que o leitor de tela lê ao entrar no mapa.
    //
    // O RESUMO É O NÚMERO DA MAQUETE (fidelidade às maquetes, 23/09/2026): a
    // cobertura com o nome dela ao lado — e o nome diz o denominador de verdade,
    // vínculos elegíveis, e não o "municípios com vínculo" da maquete.
    const texto = resumo();
    expect(texto).toMatch(/61,1%/);
    expect(texto).toMatch(/dos vínculos elegíveis no prazo/);
    expect(texto).not.toMatch(/municípios com vínculo/);

    // AS TRÊS PARCELAS SAÍRAM DO CARTÃO, E NÃO DA TELA: estão na dica do título.
    // NENHUMA SAIU, e é isso que este teste protege.
    const gatilho = screen.getByRole('button', { name: 'Fonte e método deste mapa' });
    fireEvent.focus(gatilho);
    const dica = screen.getByRole('tooltip').textContent ?? '';
    expect(dica).toMatch(/18 vínculos elegíveis/);
    expect(dica).toMatch(/11 no prazo/);
    expect(dica).toMatch(/7 pendentes/);
    fireEvent.blur(gatilho);
  });

  it('o alternador troca a medida, e a legenda troca junto', () => {
    montar();

    // A UNIDADE POR EXTENSO SAIU DA LEGENDA VISÍVEL (maquete) e ficou no nome
    // acessível dela — e na dica do título. Trocar a medida troca as duas.
    expect(screen.getByRole('group', { name: /verde é mais coberto/ })).toBeInTheDocument();
    expect(screen.getByRole('group', { name: /Legenda/ })).toHaveTextContent('> 90%');

    fireEvent.click(screen.getByRole('button', { name: '% pendente' }));
    expect(screen.getByRole('group', { name: /vermelho é mais pendente/ })).toHaveTextContent('> 75%');

    fireEvent.click(screen.getByRole('button', { name: 'Pendentes (qtd.)' }));
    expect(
      screen.getByRole('group', { name: /vínculos pendentes \(fora do prazo \+ nunca contatados\)/ }),
    ).toHaveTextContent('> 150');
  });

  it('a legenda vai da maior faixa para a menor, e o sem dado continua hachurado — não é zero', () => {
    montar();

    const legenda = screen.getByRole('group', { name: /Legenda/ });
    const itens = [...legenda.querySelectorAll('.terr-legenda-item')].map((i) => i.textContent);
    // OS CORTES SÃO OS DE SEMPRE (25, 50, 75, 90): só a ordem e o texto mudaram.
    expect(itens).toEqual(['> 90%', '75% – 90%', '50% – 75%', '25% – 50%', '≤ 25%', 'Sem dado']);

    // O SEM DADO NÃO É UMA FAIXA DA ESCALA: a amostra dele é a hachura, a mesma
    // do mapa, e não uma cor — e a dica do título diz que hachurado não é zero.
    const semDado = [...legenda.querySelectorAll('.terr-legenda-item')].at(-1)!;
    expect(semDado.querySelector('.terr-amostra-hachura')).not.toBeNull();

    const gatilho = screen.getByRole('button', { name: 'Fonte e método deste mapa' });
    fireEvent.focus(gatilho);
    const dica = screen.getByRole('tooltip').textContent ?? '';
    expect(dica).toMatch(/hachurado\): município sem vínculo elegível para medir; não é zero/);
    // "fora da ADR" e o contorno saíram da legenda visível — e estão aqui.
    expect(dica).toMatch(/fora da ADR/);
    expect(legenda).not.toHaveTextContent(/fora da ADR/);
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

    fireEvent.click(screen.getByRole('button', { name: 'Pendentes (qtd.)' }));
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
