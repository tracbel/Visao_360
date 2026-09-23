import { useState } from 'react';
import type { ClassificacaoDeIndicador } from '../../../tipos/territorio';
import { faixaDe } from '../escalas';
import {
  FAIXAS_DE_COBERTURA,
  ROTULO_DE_COBERTURA,
  UNIDADE_DE_COBERTURA,
  nº,
  porcento,
  type ModoDeCobertura,
} from '../indicadoresDaAdr';
import { SeloDeClassificacao } from '../SeloDeClassificacao';
import type { TotaisDaAdr } from '../totaisDaAdr';
import { CartaoDeMapa, type LigacaoDoMapa } from './CartaoDeMapa';
import { foraDoRecorte } from './foraDoRecorte';
import type { EstadoNoMapa } from '../MapaDeMunicipios';

/** Último contato de cada vínculo contra a cadência declarada da linha. */
export function MapaDeCobertura({
  ligacao,
  totais,
  coberturaDaAdr,
  classificacao,
}: {
  ligacao: LigacaoDoMapa;
  totais: TotaisDaAdr;
  coberturaDaAdr: number | null;
  classificacao: ClassificacaoDeIndicador | null;
}) {
  const [modoDeCobertura, setModoDeCobertura] = useState<ModoDeCobertura>('cobertura');

  function estadoDaCobertura(codigo: number): EstadoNoMapa {
    const m = ligacao.porCodigo.get(codigo);
    const fora = foraDoRecorte(m);
    if (fora) return fora;
    const c = m!.cobertura;
    if (c.vinculosComCadencia === 0) return { tipo: 'semDado', detalhe: 'sem vínculo elegível para medir' };
    const noPrazo = (100 * c.cobertos) / c.vinculosComCadencia;
    const valor =
      modoDeCobertura === 'cobertura' ? noPrazo : modoDeCobertura === 'pendencia' ? (c.percentualPendente ?? 0) : c.pendentes;
    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_DE_COBERTURA[modoDeCobertura], valor).cor,
      detalhe: `${nº(c.vinculosComCadencia)} elegíveis · ${nº(c.cobertos)} no prazo (${porcento(noPrazo)}) · ${nº(c.pendentes)} pendentes (${nº(c.foraDaCadencia)} fora do prazo + ${nº(c.nuncaContatados)} nunca)`,
    };
  }

  return (
    <CartaoDeMapa
      mapa="cobertura"
      id="cobertura"
      ligacao={ligacao}
      titulo={<>Cobertura de carteira <SeloDeClassificacao classificacao={classificacao} /></>}
      subtitulo="Último contato registrado de cada vínculo em carteira comercial, contra a cadência declarada da linha."
      resumo={
        totais.elegiveis > 0
          ? `${nº(totais.elegiveis)} elegíveis · ${nº(totais.cobertos)} no prazo (${porcento(coberturaDaAdr ?? 0)}) · ${nº(totais.pendentes)} pendentes`
          : 'nenhum vínculo elegível no recorte'
      }
      alternador={
        <div className="terr-alternador" role="group" aria-label="Cobertura em">
          {(Object.keys(ROTULO_DE_COBERTURA) as ModoDeCobertura[]).map((modo) => (
            <button key={modo} type="button" aria-pressed={modoDeCobertura === modo} onClick={() => setModoDeCobertura(modo)}>
              {ROTULO_DE_COBERTURA[modo]}
            </button>
          ))}
        </div>
      }
      tituloDoMapa="Mapa de cobertura de carteira por município"
      estadoDe={estadoDaCobertura}
      faixas={FAIXAS_DE_COBERTURA[modoDeCobertura]}
      unidade={UNIDADE_DE_COBERTURA[modoDeCobertura]}
      aviso={
        <>
          {modoDeCobertura === 'quantidade'
            ? 'Quantidade favorece cidades grandes; alterne para % para comparar.'
            : 'Percentual compara municípios de tamanhos diferentes; uma cidade com 3 vínculos muda de faixa com 1 contato.'}{' '}
          Contato é qualquer interação registrada, inclusive registro gerado pelo sistema: nenhum tipo de atividade está
          marcado como visita (documento 32, P-2).
        </>
      }
    />
  );
}
