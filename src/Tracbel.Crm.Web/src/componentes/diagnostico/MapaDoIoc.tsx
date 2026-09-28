/**
 * O MAPA DO IOC — a ADR pintada pela classe de prioridade de cada município (28/09/2026).
 *
 * É O MESMO MAPA DOS INDICADORES GEOGRÁFICOS (`MapaDeMunicipios`, a mesma malha do IBGE e o mesmo enquadramento na
 * ADR), com a mesma regra de três aparências: fora da ADR é fundo, sem IOC é hachurado, e o resto tem a cor da classe.
 * Clicar num município abre a ficha dele ao lado — a mesma escolha que o campo Município e a tabela fazem.
 */

import { Map as IconeMapa } from 'lucide-react';
import { LegendaDoMapa, MapaDeMunicipios, type EstadoNoMapa, type PoligonoProjetado } from '../territorio/MapaDeMunicipios';
import type { Faixa } from '../territorio/escalas';
import type { Enquadramento } from '../territorio/projecao';
import type { ClasseDePrioridade, MunicipioNoDiagnostico } from '../../tipos/mercado';
import { CLASSES, COR_DA_CLASSE, n, ROTULO_DA_CLASSE } from './diagnostico';

/** A legenda na ordem da escala — o `LegendaDoMapa` a inverte, e a máxima fica em cima. */
const FAIXAS: Faixa[] = [...CLASSES].reverse().map((c, i) => ({ ate: i, cor: c.cor, rotulo: c.rotulo }));

export function MapaDoIoc({
  desenho,
  porCodigo,
  adr,
  classe,
  selecionado,
  aoSelecionar,
}: {
  desenho: { enquadramento: Enquadramento; poligonos: PoligonoProjetado[] } | null;
  porCodigo: ReadonlyMap<number, MunicipioNoDiagnostico>;
  adr: ReadonlySet<number>;
  /** A classe escolhida na distribuição: as outras ficam apagadas, sem sair do mapa. */
  classe: ClasseDePrioridade | null;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
}) {
  const estadoDe = (codigo: number): EstadoNoMapa => {
    const m = porCodigo.get(codigo);
    if (!m) return { tipo: 'fora', detalhe: 'Fora da área de atuação' };
    if (m.ioc === null || m.classe === null) return { tipo: 'semDado', detalhe: `${m.nome}: sem IOC — nenhum componente com dado` };
    const apagado = classe !== null && m.classe !== classe;
    return {
      tipo: 'valor',
      cor: apagado ? '#E4E8E4' : COR_DA_CLASSE[m.classe],
      detalhe: `${m.nome}: IOC ${n(m.ioc)} · ${ROTULO_DA_CLASSE[m.classe]}`,
    };
  };

  return (
    <div className="card cad-cartao terr-mapa diag-mapa" data-bloco="mapa-do-ioc">
      <div className="terr-mapa-cabecalho">
        <span className="terr-mapa-icone" aria-hidden="true">
          <IconeMapa size={16} strokeWidth={2} />
        </span>
        <div className="card-title">Onde está a oportunidade</div>
      </div>
      {desenho ? (
        <div className="diag-mapa-corpo">
          <MapaDeMunicipios
            id="ioc"
            titulo="Municípios da ADR pela classe de prioridade do IOC"
            enquadramento={desenho.enquadramento}
            poligonos={desenho.poligonos}
            estadoDe={estadoDe}
            adr={adr}
            selecionado={selecionado}
            aoSelecionar={aoSelecionar}
          />
          <LegendaDoMapa faixas={FAIXAS} unidade="classe de prioridade" semDado="Sem IOC" />
        </div>
      ) : (
        <p className="diag-nota">Carregando a malha dos municípios…</p>
      )}
    </div>
  );
}
