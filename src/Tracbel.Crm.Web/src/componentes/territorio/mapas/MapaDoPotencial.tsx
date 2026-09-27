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
/**
 * AS REGRAS COMO O NEGÓCIO AS ESCREVEU, em ordem de nome — "1 trator a cada 20 ha de Café…".
 *
 * A lista vem do servidor pela ordem do código do produto, que não é ordem de leitura de ninguém; e,
 * desde a D-P01 (27/09/2026), são várias. Citar só a primeira — era o que a tela fazia — dizia que o
 * potencial inteiro era "1 trator a cada 200 ha de amendoim".
 */
function frasesDasRegras(regras: RegraDePotencialAplicada[]): string {
  return [...regras]
    // POR CULTURA E, DENTRO DELA, POR CATEGORIA (issue 240): o trator e a colheitadeira da soja saem
    // sempre juntos e sempre na mesma ordem.
    .sort(
      (a, b) =>
        a.produtoNome.localeCompare(b.produtoNome, 'pt-BR') ||
        (a.categoriaNome ?? '').localeCompare(b.categoriaNome ?? '', 'pt-BR'),
    )
    .map((r) => `1 ${r.modeloDeReferencia} a cada ${nº(r.hectaresPorMaquina)} ha de ${r.produtoNome}`)
    .join('; ');
}

function metodologia(
  recorte: RecorteDoPotencial,
  regras: RegraDePotencialAplicada[],
  enderecos: number,
  enderecosComArea: number,
): string {
  if (recorte === 'maquinas') {
    // "REGRA A CONFIRMAR" NÃO É MAIS FRASE FIXA: as regras da D-P01 estão confirmadas, e quando alguma
    // não estiver, o aviso de estimativa logo acima desta frase diz isso — com base no dado.
    return (
      'Fonte: IBGE/PAM — área plantada do município — com as regras de potencial do CRM. ' +
      (regras.length === 0
        ? 'Método: sem regra de potencial vigente. '
        : regras.length === 1
          ? `Método: ${frasesDasRegras(regras)}. `
          : `Método: a área plantada de cada cultura dividida pelos hectares por máquina da regra dela — ${frasesDasRegras(regras)}. `) +
      'É a área do município INTEIRO — clientes e não clientes juntos, sem separar: somar "clientes" e "não clientes" ' +
      `a ela contaria a mesma área duas vezes. Clientes: ${nº(enderecosComArea)} de ${nº(enderecos)} endereços com área ` +
      'e cultura. Ressalva: não considera ciclo de troca, parque instalado, concorrência nem as culturas sem regra.'
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
  regras,
  enderecos,
  enderecosComArea,
  classificacao,
}: {
  ligacao: LigacaoDoMapa;
  totais: TotaisDaAdr;
  /** As regras vigentes — uma por cultura desde a D-P01. */
  regras: RegraDePotencialAplicada[];
  enderecos: number;
  enderecosComArea: number;
  classificacao: ClassificacaoDeIndicador | null;
}) {
  const [recorteDoPotencial, setRecorteDoPotencial] = useState<RecorteDoPotencial>('maquinas');

  // QUANTAS CULTURAS TÊM REGRA — pelo produto, e não pela quantidade de regras: uma cultura pode ter
  // regra em mais de uma categoria de máquina, e continua sendo uma cultura.
  const culturasComRegra = new Set(regras.map((r) => r.produtoCodigoIbge)).size;

  function estadoDoPotencial(codigo: number): EstadoNoMapa {
    const m = ligacao.porCodigo.get(codigo);
    const fora = foraDoRecorte(m);
    if (fora) return fora;

    // AS CULTURAS COM REGRA E A LAVOURA INTEIRA SÃO COISAS DIFERENTES: o recorte "máquinas teóricas"
    // olha só o que tem regra de potencial, e os outros dois olham TODAS as culturas do município.
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

    // O ANO DAS CULTURAS SÓ APARECE QUANDO DIFERE DO DA LAVOURA ao lado (issue 152). Com uma regra por
    // cultura, cada uma está no ano da PAM dela: um ano só aparece como ele, e anos diferentes como
    // intervalo — e não o ano da primeira da lista, como se fosse o de todas.
    const anos = [...new Set(m!.potencial.map((p) => p.ano).filter((a): a is number => a != null))].sort();
    const anoDaCultura =
      anos.length === 0 || (anos.length === 1 && anos[0] === producao?.ano)
        ? ''
        : anos.length === 1
          ? ` (${anos[0]})`
          : ` (${anos[0]}–${anos[anos.length - 1]})`;
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
          <p>{metodologia(recorteDoPotencial, regras, enderecos, enderecosComArea)}</p>
        </>
      }
      // A REGRA CONTINUA NO RESUMO, como metadado à direita — que é onde a
      // maquete põe "1.303 N / 10 ha de café". Ela é operacional e não é a
      // resposta do cartão: a resposta é quantas máquinas o recorte comporta.
      //
      // COM MAIS DE UMA REGRA, O RESUMO CONTA AS CULTURAS e as regras vão para a
      // dica, uma por uma. Uma regra só, escolhida pela ordem da lista, diria que
      // ela é o método do mapa inteiro.
      resumo={{
        valor: totais.municipiosComArea > 0 ? nº(Math.round(totais.maquinasTeoricas)) : null,
        rotulo: 'máquinas potenciais',
        rotuloEmbaixo: true,
        semValor: 'sem área plantada ou regra para calcular',
        meta:
          totais.municipiosComArea > 0
            ? [
                { valor: nº(totais.municipiosComArea), rotulo: 'municípios' },
                ...(regras.length === 1
                  ? [
                      {
                        valor: `1 ${regras[0].modeloDeReferencia}`,
                        rotulo: `/ ${nº(regras[0].hectaresPorMaquina)} ha de ${regras[0].produtoNome}`,
                      },
                    ]
                  : regras.length > 1
                    ? [{ valor: nº(culturasComRegra), rotulo: culturasComRegra === 1 ? 'cultura com regra' : 'culturas com regra' }]
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
