/**
 * O FILTRO DO PADRÃO (documento 54 §3.5): o `CampoDoFiltro` substitui o mesmo JSX que as telas repetiam — e tem de gerar
 * exatamente o HTML de antes, com e sem os opcionais, para nenhuma tela mudar.
 */

import { CalendarDays, MapPin } from 'lucide-react';
import { renderToStaticMarkup } from 'react-dom/server';
import { describe, expect, it } from 'vitest';
import { InfoTooltip } from '../InfoTooltip';
import { CampoDoFiltro } from './CampoDoFiltro';

describe('CampoDoFiltro', () => {
  it('gera o HTML do filtro do padrão, com o bloco e um rótulo de texto', () => {
    const antes = renderToStaticMarkup(
      <label className="dash-filtro" data-bloco="cultura">
        <span className="dash-filtro-icone" aria-hidden="true">
          <MapPin size={17} strokeWidth={2} />
        </span>
        <span className="dash-filtro-corpo">
          <span className="dash-filtro-rotulo">Cultura</span>
          <select defaultValue="">
            <option value="">Todas</option>
          </select>
        </span>
      </label>,
    );

    const depois = renderToStaticMarkup(
      <CampoDoFiltro icone={MapPin} rotulo="Cultura" bloco="cultura">
        <select defaultValue="">
          <option value="">Todas</option>
        </select>
      </CampoDoFiltro>,
    );

    expect(depois).toBe(antes);
  });

  it('sem bloco, com o ⓘ no rótulo', () => {
    const antes = renderToStaticMarkup(
      <label className="dash-filtro">
        <span className="dash-filtro-icone" aria-hidden="true">
          <CalendarDays size={17} strokeWidth={2} />
        </span>
        <span className="dash-filtro-corpo">
          <span className="dash-filtro-rotulo">
            Período
            <InfoTooltip rotulo="Como o período é contado" texto="O ano fiscal até o último mês fechado." />
          </span>
          <input type="date" defaultValue="2026-10-01" />
        </span>
      </label>,
    );

    const depois = renderToStaticMarkup(
      <CampoDoFiltro
        icone={CalendarDays}
        rotulo={
          <>
            Período
            <InfoTooltip rotulo="Como o período é contado" texto="O ano fiscal até o último mês fechado." />
          </>
        }
      >
        <input type="date" defaultValue="2026-10-01" />
      </CampoDoFiltro>,
    );

    expect(depois).toBe(antes);
  });

  it('com a classe extra, a cor, a dica e o ícone maior da Cobertura Regional', () => {
    const antes = renderToStaticMarkup(
      <label className="dash-filtro cobf-filtro" data-bloco="regional" data-cor="azul" title="A regional do recorte.">
        <span className="dash-filtro-icone" aria-hidden="true">
          <MapPin size={18} strokeWidth={2.1} />
        </span>
        <span className="dash-filtro-corpo">
          <span className="dash-filtro-rotulo">Regional</span>
          <select defaultValue="">
            <option value="">Todas</option>
          </select>
        </span>
      </label>,
    );

    const depois = renderToStaticMarkup(
      <CampoDoFiltro
        icone={MapPin}
        rotulo="Regional"
        bloco="regional"
        cor="azul"
        dica="A regional do recorte."
        classe="cobf-filtro"
        tamanhoDoIcone={18}
        espessura={2.1}
      >
        <select defaultValue="">
          <option value="">Todas</option>
        </select>
      </CampoDoFiltro>,
    );

    expect(depois).toBe(antes);
  });
});
