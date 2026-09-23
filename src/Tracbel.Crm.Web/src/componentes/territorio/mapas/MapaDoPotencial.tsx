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
    // Misturá-los faria a área do café aparecer ao lado do valor da lavoura inteira como se fossem
    // a mesma base.
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

    // O ANO DA CULTURA SÓ APARECE QUANDO DIFERE DO DA LAVOURA ao lado (issue 152): os dois números estão na mesma
    // linha, e o leitor precisa saber que não são do mesmo ano.
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
      titulo={<>Potencial teórico — cultura × área <SeloDeClassificacao classificacao={classificacao} /></>}
      subtitulo={
        regra
          ? `1 ${regra.modeloDeReferencia} a cada ${regra.hectaresPorMaquina} ha de ${regra.produtoNome} · área plantada do município (IBGE)`
          : 'Sem regra de potencial ativa'
      }
      resumo={
        totais.municipiosComArea > 0
          ? `${nº(Math.round(totais.maquinasTeoricas))} máquinas teóricas · ${nº(Math.round(totais.hectares))} ha úteis em ${nº(totais.municipiosComArea)} municípios com área divulgada`
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
      classeDoAviso="terr-aviso terr-aviso-alerta"
      aviso={
        recorteDoPotencial === 'maquinas' ? (
          <>
            Regra a confirmar. É a área do município inteiro — clientes e não clientes juntos, sem separar: somar
            "clientes" e "não clientes" a ela contaria a mesma área duas vezes. Clientes: {nº(enderecosComArea)} de{' '}
            {nº(enderecos)} endereços com área e cultura. Não considera ciclo de troca, parque instalado, concorrência
            nem outras culturas.
          </>
        ) : (
          <>
            Agora o mapa mostra a <strong>lavoura inteira</strong> do município, e não só a cultura da regra — são
            bases diferentes. O valor da produção é o que o município <strong>colhe</strong>, publicado pelo IBGE em
            mil reais; não é venda da Tracbel nem preço de máquina. O café entra uma vez só (o "Total" do IBGE, sem
            somar Arábica e Canephora de novo).
          </>
        )
      }
    />
  );
}
