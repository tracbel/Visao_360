import { useState } from 'react';
import type { ClassificacaoDeIndicador, RegraDePotencialAplicada } from '../../../tipos/territorio';
import { faixaDe, reaisDaProducao } from '../escalas';
import {
  FAIXAS_DO_POTENCIAL,
  MOTIVO_SEM_PARQUE,
  ROTULO_DO_POTENCIAL,
  UNIDADE_DO_POTENCIAL,
  nº,
  type RecorteDoPotencial,
} from '../indicadoresDaAdr';
import { SeloDeClassificacao } from '../SeloDeClassificacao';
import type { TotaisDaAdr } from '../totaisDaAdr';
import { CartaoDeMapa, type LigacaoDoMapa } from './CartaoDeMapa';
import { foraDoRecorte } from './foraDoRecorte';
import type { EstadoNoMapa } from '../MapaDeMunicipios';

/**
 * O MAIOR PARÁGRAFO FIXO DA TELA VIROU DICA (fase T2.1, issues 31 e 33).
 *
 * Eram oito linhas permanentes sobre regra a confirmar, área do município
 * inteiro, cliente × não cliente, ciclo de troca, parque instalado, concorrência
 * e valor da produção — no cartão onde a diretoria quer ver o mapa. Está tudo
 * aqui, palavra por palavra, a um `ⓘ` de distância.
 */
function metodologia(
  recorte: RecorteDoPotencial,
  regra: RegraDePotencialAplicada | null,
  enderecos: number,
  enderecosComArea: number,
): string {
  if (recorte === 'maquinas') {
    return (
      'Fonte: IBGE/PAM — área plantada do município — com a regra de potencial do CRM. ' +
      (regra
        ? `Método: 1 ${regra.modeloDeReferencia} a cada ${regra.hectaresPorMaquina} ha de ${regra.produtoNome}. `
        : 'Método: sem regra de potencial vigente. ') +
      'Regra a confirmar pelo comercial (D-P01, issue 63). ' +
      'É a área do município INTEIRO — clientes e não clientes juntos, sem separar: somar "clientes" e "não clientes" ' +
      `a ela contaria a mesma área duas vezes. Clientes: ${nº(enderecosComArea)} de ${nº(enderecos)} endereços com área ` +
      'e cultura. Ressalva: não considera ciclo de troca, parque instalado, concorrência nem outras culturas.'
    );
  }

  return (
    'Fonte: IBGE/SIDRA — PAM, Produção Agrícola Municipal. ' +
    'Método: agora o mapa mostra a LAVOURA INTEIRA do município, e não só a cultura da regra — são bases diferentes. ' +
    'Ressalvas: o valor da produção é o que o município COLHE, publicado em mil reais; não é venda da Tracbel nem ' +
    'preço de máquina. O café entra uma vez só, pelo "Total" do IBGE, sem somar Arábica e Canephora de novo.'
  );
}

/** Potencial teórico — a regra de cultura × área, e a lavoura que a sustenta. */
export function MapaDoPotencial({
  ligacao,
  totais,
  regra,
  enderecos,
  enderecosComArea,
  classificacao,
}: {
  ligacao: LigacaoDoMapa;
  totais: TotaisDaAdr;
  regra: RegraDePotencialAplicada | null;
  enderecos: number;
  enderecosComArea: number;
  classificacao: ClassificacaoDeIndicador | null;
}) {
  const [recorteDoPotencial, setRecorteDoPotencial] = useState<RecorteDoPotencial>('maquinas');

  function estadoDoPotencial(codigo: number): EstadoNoMapa {
    const m = ligacao.porCodigo.get(codigo);
    const fora = foraDoRecorte(m);
    if (fora) return fora;

    // AS CULTURAS COM REGRA E A LAVOURA INTEIRA SÃO COISAS DIFERENTES: o recorte "máquinas teóricas"
    // olha só o que tem regra de potencial, e os outros dois olham TODAS as culturas do município.
    const potencial = m!.potencial[0];
    const motor = m!.potencialEstrutural;
    const producao = m!.producao;

    const valor =
      recorteDoPotencial === 'maquinas'
        ? (motor?.parqueDeMaquinas ?? null)
        : recorteDoPotencial === 'areaPlantada'
          ? (producao?.areaPlantadaHectares ?? null)
          : (producao?.valorDaProducaoMilReais ?? null);

    if (valor === null)
      return {
        tipo: 'semDado',
        detalhe:
          recorteDoPotencial === 'maquinas'
            ? MOTIVO_SEM_PARQUE[motor?.motivoSemParque ?? 'SemRegra']
            : 'produção agrícola não carregada para este município',
      };

    // O ANO DA CULTURA SÓ APARECE QUANDO DIFERE DO DA LAVOURA ao lado (issue 152).
    const anoDaCultura = potencial?.ano != null && potencial.ano !== producao?.ano ? ` (${potencial.ano})` : '';
    const daRegra = motor
      ? `${nº(Math.round(motor.areaUtilHectares ?? 0))} ha úteis${anoDaCultura} · ${nº(motor.parqueDeMaquinas ?? 0)} máquinas teóricas${motor.demandaAnualDeMaquinas != null ? ` · ${nº(motor.demandaAnualDeMaquinas)} por ano` : ''}${motor.estimativa ? ' · estimativa' : ''}`
      : 'sem regra de potencial vigente';

    const daLavoura = producao
      ? `lavoura ${nº(Math.round(producao.areaPlantadaHectares ?? 0))} ha em ${nº(producao.culturasComArea)} culturas · ${reaisDaProducao(producao.valorDaProducaoMilReais ?? 0)} (${producao.ano})`
      : 'lavoura não carregada';

    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_DO_POTENCIAL[recorteDoPotencial], valor).cor,
      detalhe: `${daRegra} · ${daLavoura}`,
    };
  }

  return (
    <CartaoDeMapa
      mapa="potencial"
      id="potencial"
      ligacao={ligacao}
      titulo={<>Potencial teórico <SeloDeClassificacao classificacao={classificacao} /></>}
      metodologia={metodologia(recorteDoPotencial, regra, enderecos, enderecosComArea)}
      // O RESUMO ABSORVEU A REGRA, que era subtítulo: ela é operacional e cabe
      // numa linha. O resto do subtítulo virou metodologia.
      resumo={
        totais.municipiosComArea > 0
          ? `${regra ? `1 ${regra.modeloDeReferencia} / ${regra.hectaresPorMaquina} ha de ${regra.produtoNome} · ` : ''}${nº(Math.round(totais.maquinasTeoricas))} máquinas em ${nº(totais.municipiosComArea)} municípios${totais.potencialEstimado ? ' · estimativa' : ''}`
          : 'sem área plantada ou regra para calcular'
      }
      alternador={
        <div className="terr-alternador" role="group" aria-label="O que o mapa mostra">
          {(Object.keys(ROTULO_DO_POTENCIAL) as RecorteDoPotencial[]).map((r) => (
            <button key={r} type="button" aria-pressed={recorteDoPotencial === r} onClick={() => setRecorteDoPotencial(r)}>
              {ROTULO_DO_POTENCIAL[r]}
            </button>
          ))}
        </div>
      }
      tituloDoMapa="Mapa de potencial teórico por município"
      estadoDe={estadoDoPotencial}
      faixas={FAIXAS_DO_POTENCIAL[recorteDoPotencial]}
      unidade={UNIDADE_DO_POTENCIAL[recorteDoPotencial]}
    />
  );
}
