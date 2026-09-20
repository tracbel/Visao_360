/**
 * O MENU LATERAL, conferido pelo que ele oferece (issue [029]).
 *
 * Duas coisas que só um teste segura: que a Agenda **saiu do menu** e não volta
 * por descuido, e que ela **continua abrindo por URL** — tirar do menu não pode
 * virar apagar a tela. A terceira é a regra geral: item de menu que aponta para
 * rota inexistente leva a pessoa para a Visão 360 sem avisar, porque é isso que
 * `acharRota` faz quando não encontra o caminho.
 */

import { describe, expect, it } from 'vitest';
import { acharRota, ROTAS } from '../rotas';
import { SECOES } from './navegacao';

const CAMINHOS_DO_MENU = SECOES.flatMap((secao) => secao.itens.map((item) => item.caminho));

describe('menu lateral', () => {
  it('oferece as dez telas de trabalho, e a Agenda não está entre elas', () => {
    expect(CAMINHOS_DO_MENU).toEqual([
      '/',
      '/cobertura',
      '/pipeline',
      '/clientes',
      '/equipamentos',
      '/relatorios/funil',
      '/relatorios/performance',
      '/relatorios/cobertura',
      '/relatorios/territorio',
      '/config',
    ]);
    expect(CAMINHOS_DO_MENU).not.toContain('/agenda');
  });

  it('a Agenda continua sendo uma tela, alcançável pela URL e pelos links das outras', () => {
    expect(acharRota('/agenda').titulo).toBe('Agenda do CEN');
  });

  it('todo item do menu aponta para uma rota declarada', () => {
    const declaradas = new Set(ROTAS.map((rota) => rota.caminho));
    const orfaos = CAMINHOS_DO_MENU.filter((caminho) => !declaradas.has(caminho));

    expect(orfaos).toEqual([]);
  });
});
