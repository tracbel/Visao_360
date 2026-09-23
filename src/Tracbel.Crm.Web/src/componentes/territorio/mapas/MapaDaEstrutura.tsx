import { useState } from 'react';
import { faixaDe } from '../escalas';
import {
  FAIXAS_DA_ESTRUTURA,
  ROTULO_DA_ESTRUTURA,
  UNIDADE_DA_ESTRUTURA,
  nº,
  type RecorteDaEstrutura,
} from '../indicadoresDaAdr';
import type { TotaisDaAdr } from '../totaisDaAdr';
import { CartaoDeMapa, type LigacaoDoMapa } from './CartaoDeMapa';
import { foraDoRecorte } from './foraDoRecorte';
import type { EstadoNoMapa } from '../MapaDeMunicipios';

/**
 * O mapa da estrutura agropecuária.
 *
 * SEM USINA NÃO É CAPACIDADE ZERO. Município fora da lista da ANP fica hachurado, porque a
 * ausência ali só prova que não há usina de ETANOL — usina que só faz açúcar não é autorizada
 * pela ANP e não aparece. Já a usina parada, com capacidade zerada, é um valor e entra na escala.
 */
export function MapaDaEstrutura({
  ligacao,
  totais,
  anoDoCenso,
  anoDoRebanho,
}: {
  ligacao: LigacaoDoMapa;
  totais: TotaisDaAdr;
  anoDoCenso: number | null;
  anoDoRebanho: number | null;
}) {
  const [recorteDaEstrutura, setRecorteDaEstrutura] = useState<RecorteDaEstrutura>('tratores');

  function estadoDaEstrutura(codigo: number): EstadoNoMapa {
    const m = ligacao.porCodigo.get(codigo);
    const fora = foraDoRecorte(m);
    if (fora) return fora;

    const e = m!.estrutura;
    const valor =
      recorteDaEstrutura === 'tratores'
        ? e.tratores
        : recorteDaEstrutura === 'densidade'
          ? e.tratoresPorMilKm2
          : recorteDaEstrutura === 'estabelecimentos'
            ? e.estabelecimentos
            : recorteDaEstrutura === 'rebanho'
              ? e.bovinos
              : e.usinas.length === 0
                ? null
                : (e.capacidadeDeEtanolM3Dia ?? 0);

    if (valor === null)
      return {
        tipo: 'semDado',
        detalhe:
          recorteDaEstrutura === 'usinas'
            ? 'sem usina de etanol autorizada aqui (a ANP não enxerga usina só de açúcar)'
            : 'sigilo do IBGE ou fonte não carregada — não é zero',
      };

    const parque =
      e.tratores === null
        ? 'tratores sob sigilo'
        : `${nº(e.tratores)} tratores${e.tratoresAbaixoDe100Cv !== null ? ` (${nº(e.tratoresAbaixoDe100Cv)} < 100 cv)` : ''}`;

    const usinas =
      e.usinas.length === 0
        ? 'sem usina'
        : `${nº(e.usinas.length)} usina${e.usinas.length > 1 ? 's' : ''}${e.capacidadeDeEtanolM3Dia !== null ? ` · ${nº(e.capacidadeDeEtanolM3Dia)} m³/d` : ''}`;

    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_DA_ESTRUTURA[recorteDaEstrutura], valor).cor,
      detalhe: `${parque}${e.anoDoCenso ? ` (${e.anoDoCenso})` : ''} · ${e.estabelecimentos !== null ? `${nº(e.estabelecimentos)} propriedades` : 'propriedades sob sigilo'} · ${e.bovinos !== null ? `${nº(e.bovinos)} bovinos${e.anoDoRebanho ? ` (${e.anoDoRebanho})` : ''}` : 'sem rebanho'} · ${usinas}`,
    };
  }

  return (
    <CartaoDeMapa
      mapa="estrutura"
      id="estrutura"
      ligacao={ligacao}
      titulo="Estrutura agropecuária — o que já existe para mecanizar"
      subtitulo={
        <>
          Censo Agropecuário{anoDoCenso ? ` ${anoDoCenso}` : ''} (tratores e propriedades) · Pesquisa da Pecuária
          Municipal{anoDoRebanho ? ` ${anoDoRebanho}` : ''} (rebanho) · ANP (usinas)
        </>
      }
      resumo={
        <>
          {nº(totais.tratores)} tratores em {nº(totais.municipiosComTratores)} municípios ·{' '}
          {nº(totais.estabelecimentos)} propriedades · {nº(totais.bovinos)} bovinos ·{' '}
          {nº(totais.usinas)} usinas em {nº(totais.municipiosComUsina)} municípios
        </>
      }
      alternador={
        <div className="terr-alternador" role="group" aria-label="Recorte da estrutura">
          {(Object.keys(ROTULO_DA_ESTRUTURA) as RecorteDaEstrutura[]).map((r) => (
            <button key={r} type="button" aria-pressed={recorteDaEstrutura === r} onClick={() => setRecorteDaEstrutura(r)}>
              {ROTULO_DA_ESTRUTURA[r]}
            </button>
          ))}
        </div>
      }
      tituloDoMapa="Mapa da estrutura agropecuária por município"
      estadoDe={estadoDaEstrutura}
      faixas={FAIXAS_DA_ESTRUTURA[recorteDaEstrutura]}
      unidade={UNIDADE_DA_ESTRUTURA[recorteDaEstrutura]}
      aviso={
        <>
          <strong>O Censo Agropecuário é de {anoDoCenso ?? '2017'}</strong> e o próximo sai em 2028: o parque tem essa
          idade. O rebanho é anual e está em {anoDoRebanho ?? '—'}. Hachurado é <strong>sigilo do IBGE</strong>, que não
          é zero — ele oculta o número quando poucos estabelecimentos o compõem. As faixas de potência não se somam ao
          total: o "Total" do IBGE é uma categoria ao lado delas. A ANP só enxerga usina de <strong>etanol</strong>:
          ausência aqui não prova ausência de usina.
        </>
      }
    />
  );
}
