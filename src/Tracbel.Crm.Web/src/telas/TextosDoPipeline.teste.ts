/**
 * OS TEXTOS DA CARGA ANTIGA DE PROCESSOS NÃO VOLTAM AO PIPELINE, À AGENDA E ÀS FICHAS (28/09/2026, issue 31).
 *
 * O irmão de `TextosDoFunil.teste.ts`, para as telas que a onda 2 do Vórtice encheu (documento 52 §12). Elas diziam
 * "45.397 processos migrados, recorte de 2026", "103.339 tarefas", "as 273 pessoas que vieram do Vórtice", "50 fases" e
 * "3.922 processos com perspectiva" — os números da carga de 2026 que a sanitização de 15/09 apagou. O que chega agora
 * são os processos, as tarefas e as interações dos clientes do CRM, a partir de 01/11/2023, e nenhuma daquelas frases
 * descreve esse dado. Comentário incluído, para ninguém copiar a frase de volta de um comentário.
 */

import { describe, expect, it } from 'vitest';
import agenda from './Agenda.tsx?raw';
import inicio from './Inicio.tsx?raw';
import oportunidade from './OportunidadeFicha.tsx?raw';
import pipeline from './Pipeline.tsx?raw';
import rotas from '../rotas.tsx?raw';
import cliente360 from '../componentes/painel360/Cliente360Api.tsx?raw';

const PROIBIDOS = [
  /45\.397/,
  /103\.339/,
  /81\.571/,
  /273 pessoas/,
  /50 fases/,
  /3\.922/,
  /recorte carregado/,
  /recorte de 2026/,
];

describe('os textos velhos da carga de processos (issue 31)', () => {
  it.each([
    ['Pipeline.tsx', pipeline],
    ['Agenda.tsx', agenda],
    ['OportunidadeFicha.tsx', oportunidade],
    ['Cliente360Api.tsx', cliente360],
    ['Inicio.tsx', inicio],
    ['rotas.tsx', rotas],
  ])('%s não diz mais nenhum deles', (_, fonte) => {
    for (const proibido of PROIBIDOS) expect(fonte).not.toMatch(proibido);
  });
});
