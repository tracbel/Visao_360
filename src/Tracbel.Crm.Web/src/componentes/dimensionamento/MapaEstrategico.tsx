/**
 * O MAPA ESTRATÉGICO — Tracbel no Estado de São Paulo (issue 259): os 645 municípios, com os do recorte em verde e os
 * demais em cinza, e a intensidade pela variável escolhida (produção, área, quantidade ou clientes).
 *
 * É O MESMO `MapaDeMunicipios` dos Indicadores e do Diagnóstico, com a mesma regra de aparências — sem dado é hachurado,
 * nunca zero —, mas enquadrado no ESTADO INTEIRO: a pergunta aqui é "que pedaço de São Paulo a Tracbel atende", e a ADR
 * enquadrada sozinha esconderia o resto.
 *
 * As faixas são pelos quantis de cada grupo (recorte e demais), separados: a cana de um município pesa cem vezes o café de
 * outro, e uma escala só deixaria o recorte inteiro na mesma cor.
 */

import { useMemo, useState } from 'react';
import { MapaDeMunicipios, type EstadoNoMapa, type PoligonoProjetado } from '../territorio/MapaDeMunicipios';
import type { Enquadramento } from '../territorio/projecao';
import { Seletor } from '../mercado/momento/pecas';
import type { MunicipioNoMapaDoDimensionamento } from '../../tipos/mercado';
import { cortes, faixaDe, formatarNoMapa, TONS_DO_FOCO, TONS_DOS_DEMAIS, VARIAVEIS_DO_MAPA, type VariavelDoMapa } from './dimensionamento';

export function MapaEstrategico({
  desenho,
  municipios,
}: {
  desenho: { enquadramento: Enquadramento; poligonos: PoligonoProjetado[] } | null;
  municipios: readonly MunicipioNoMapaDoDimensionamento[];
}) {
  const [variavel, setVariavel] = useState<VariavelDoMapa>('valor');
  const [selecionado, setSelecionado] = useState<number | null>(null);

  const { porCodigo, foco, limitesDoFoco, limitesDosDemais } = useMemo(() => {
    const porCodigo = new Map(municipios.map((m) => [m.codigoIbge, m]));
    const foco = new Set(municipios.filter((m) => m.emFoco).map((m) => m.codigoIbge));
    const valores = (emFoco: boolean) =>
      municipios.filter((m) => m.emFoco === emFoco).map((m) => m[variavel]).filter((v): v is number => v !== null);
    return {
      porCodigo,
      foco,
      limitesDoFoco: cortes(valores(true), TONS_DO_FOCO.length),
      limitesDosDemais: cortes(valores(false), TONS_DOS_DEMAIS.length),
    };
  }, [municipios, variavel]);

  const estadoDe = (codigo: number): EstadoNoMapa => {
    const m = porCodigo.get(codigo);
    if (!m) return { tipo: 'semDado', detalhe: 'sem dado da PAM' };
    const v = m[variavel];
    const grupo = m.emFoco ? 'no recorte' : 'demais SP';
    if (v === null || v <= 0)
      return variavel === 'clientes'
        ? { tipo: 'fora', detalhe: `${grupo} · sem cliente em carteira` }
        : { tipo: 'semDado', detalhe: `${grupo} · sem dado divulgado pelo IBGE` };
    const cor = m.emFoco ? TONS_DO_FOCO[faixaDe(v, limitesDoFoco)] : TONS_DOS_DEMAIS[faixaDe(v, limitesDosDemais)];
    return { tipo: 'valor', cor, detalhe: `${grupo} · ${formatarNoMapa(variavel, v)}` };
  };

  const rotulo = VARIAVEIS_DO_MAPA.find((v) => v.id === variavel)!.rotulo;
  const escolhido = selecionado === null ? null : porCodigo.get(selecionado) ?? null;

  return (
    <div className="dim-mapa" data-bloco="mapa-estrategico">
      <div className="dim-mapa-controle">
        <Seletor rotulo="Intensidade" valor={variavel} opcoes={VARIAVEIS_DO_MAPA} aoMudar={setVariavel} />
      </div>
      {desenho ? (
        <div className="dim-mapa-corpo">
          <div className="dim-mapa-area">
            <MapaDeMunicipios
              id="dimensionamento"
              titulo={`Municípios de São Paulo por ${rotulo.toLowerCase()}, com o recorte da Tracbel em verde`}
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDe}
              adr={foco}
              selecionado={selecionado}
              aoSelecionar={(codigo) => setSelecionado((atual) => (atual === codigo ? null : codigo))}
            />
          </div>
          <div className="dim-mapa-lado">
            <ul className="dim-legenda" aria-label={`Legenda — ${rotulo.toLowerCase()}`}>
              <li>
                <span className="dim-legenda-rampa" aria-hidden="true">
                  {TONS_DO_FOCO.map((cor) => (
                    <i key={cor} style={{ background: cor }} />
                  ))}
                </span>
                <span>Em foco (o recorte) — do menor ao maior</span>
              </li>
              <li>
                <span className="dim-legenda-rampa" aria-hidden="true">
                  {TONS_DOS_DEMAIS.map((cor) => (
                    <i key={cor} style={{ background: cor }} />
                  ))}
                </span>
                <span>Demais municípios de SP</span>
              </li>
              <li>
                <span className="dim-legenda-rampa" aria-hidden="true">
                  <i className="terr-amostra-hachura" />
                </span>
                <span>{variavel === 'clientes' ? 'Sem dado' : 'Sem dado divulgado'}</span>
              </li>
            </ul>
            {escolhido ? (
              <dl className="dim-mapa-ficha" aria-live="polite">
                <dt>{escolhido.nome}</dt>
                <dd>{escolhido.emFoco ? 'No recorte' : 'Fora do recorte'}</dd>
                <dd>Produção: {escolhido.valor === null ? '—' : formatarNoMapa('valor', escolhido.valor)}</dd>
                <dd>Área: {escolhido.area === null ? '—' : formatarNoMapa('area', escolhido.area)}</dd>
                <dd>Quantidade: {escolhido.quantidade === null ? '—' : formatarNoMapa('quantidade', escolhido.quantidade)}</dd>
                <dd>Clientes em carteira: {escolhido.clientes ?? 0}</dd>
              </dl>
            ) : (
              <p className="dim-nota">Clique num município para ver os números dele.</p>
            )}
          </div>
        </div>
      ) : (
        <p className="dim-nota">Carregando a malha dos municípios…</p>
      )}
    </div>
  );
}
