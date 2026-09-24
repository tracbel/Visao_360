import { Sprout } from 'lucide-react';
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
      titulo="Potencial teórico"
      icone={Sprout}
      // O SELO DE ESTIMATIVA FICA, DISCRETO. A maquete não mostra selo neste
      // cartão; mas o potencial é necessidade TEÓRICA, com regra a confirmar, e a
      // tela não deixa esse número parecer medido (documento 32, P-8) — há teste
      // que exige a palavra na primeira camada. Ele fica na ponta do título, em
      // letra de metadado e sem fundo, com o motivo na dica dele. Sem a
      // classificação da API, o próprio dado acende o selo (`potencialEstimado`).
      selo={
        classificacao ? (
          <SeloDeClassificacao classificacao={classificacao} />
        ) : totais.potencialEstimado ? (
          <span className="terr-selo terr-selo-Estimativa">estimativa</span>
        ) : null
      }
      metodologia={
        <>
          {totais.potencialEstimado && (
            <p>Estimativa: alguma regra que dimensionou máquina aqui ainda não foi confirmada pelo comercial (D-P01).</p>
          )}
          <p>{metodologia(recorteDoPotencial, regra, enderecos, enderecosComArea)}</p>
        </>
      }
      // A REGRA CONTINUA NO RESUMO, e agora como metadado à direita — que é onde
      // a maquete põe "1.303 N / 10 ha de café". Ela é operacional e não é a
      // resposta do cartão: a resposta é quantas máquinas o recorte comporta.
      resumo={{
        valor: totais.municipiosComArea > 0 ? nº(Math.round(totais.maquinasTeoricas)) : null,
        rotulo: 'máquinas potenciais',
        rotuloEmbaixo: true,
        semValor: 'sem área plantada ou regra para calcular',
        meta:
          totais.municipiosComArea > 0
            ? [
                { valor: nº(totais.municipiosComArea), rotulo: 'municípios' },
                ...(regra
                  ? [
                      {
                        valor: `1 ${regra.modeloDeReferencia}`,
                        rotulo: `/ ${regra.hectaresPorMaquina} ha de ${regra.produtoNome}`,
                      },
                    ]
                  : []),
              ]
            : undefined,
      }}
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
      semDado={{
        rotulo: 'Sem valor',
        explicacao:
          recorteDoPotencial === 'maquinas'
            ? 'área plantada sob sigilo do IBGE, ou nenhuma cultura do município com regra de hectares por máquina'
            : 'produção agrícola não carregada para o município',
      }}
    />
  );
}
