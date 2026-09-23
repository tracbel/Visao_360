/**
 * Potencial estrutural — Parque | Demanda anual | Cenários (documento 50, §4.5).
 *
 * O PAINEL DO RECORTE MUDOU DE LUGAR, NÃO DE CONTEÚDO. Ele estava dentro do
 * cartão do mapa de potencial, onde eu o tinha colocado contra o que a §10.3 do
 * documento 49 já dizia; agora está no bloco a que pertence. É o mesmo
 * componente, com os mesmos números.
 *
 * A CALCULADORA VIROU AÇÃO SECUNDÁRIA. Ela é uma ferramenta de simulação e
 * estava competindo com o painel dentro do cartão do mapa: o painel manda.
 *
 * NADA AQUI CALCULA. A demanda anual e o motivo de ela faltar já vêm prontos da
 * API, e os cenários do recorte ainda não existem — nesse caso a tela diz o que
 * falta, em vez de mostrar um número derivado às pressas para encher o espaço.
 */

import { SlidersHorizontal } from 'lucide-react';
import { useState } from 'react';
import type { ContextoDeAcesso } from '../../dados/api/http';
import type { PotencialDoRecorteNoMapa } from '../../tipos/territorio';
import { MetricaAusente } from '../comum/MetricaAusente';
import { MOTIVO_SEM_PARQUE, nº } from '../territorio/indicadoresDaAdr';
import { PainelDoPotencialDoRecorte } from '../territorio/PainelDoPotencialDoRecorte';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { Calculadora } from './Calculadora';
import { AbasInternas } from './AbasInternas';

type SubAba = 'parque' | 'demanda' | 'cenarios';

export function BlocoDePotencial({
  recorte,
  semFiltro,
  contexto,
  municipioCodigoIbge,
}: {
  recorte: PotencialDoRecorteNoMapa | null;
  semFiltro: boolean;
  contexto: ContextoDeAcesso;
  municipioCodigoIbge: number | null;
}) {
  const [subAba, setSubAba] = useState<SubAba>('parque');
  const [calculadoraAberta, setCalculadoraAberta] = useState(false);

  return (
    <section data-bloco="potencial-estrutural">
      <TituloDaSecao
        titulo="Potencial estrutural"
        subtitulo="O que a área comporta, antes do momento do mercado."
        metodologia={
          'Parque instalado, renovação por ano e os três cenários. O parque é a soma dos municípios do recorte, e ' +
          'não o motor rodado sobre as áreas somadas: o compartilhamento de terra entre culturas acontece dentro do ' +
          'município. A demanda anual depende do ciclo de renovação por cultura (D-P01, issue 63).'
        }
      />

      {calculadoraAberta && (
        <Calculadora
          contexto={contexto}
          municipioCodigoIbge={municipioCodigoIbge}
          aoFechar={() => setCalculadoraAberta(false)}
        />
      )}

      <AbasInternas
        rotulo="O que o potencial mostra"
        ativa={subAba}
        aoTrocar={setSubAba}
        // "SIMULAR CENÁRIO" VAI PARA A LINHA DO ALTERNADOR (maquete). Ele estava
        // à direita do TÍTULO da seção, uma linha acima — e num painel de um
        // terço da largura o título ficava dividido com um botão.
        acao={
          <div className="terr-alternador" role="group" aria-label="Simulação">
            <button type="button" aria-pressed={calculadoraAberta} onClick={() => setCalculadoraAberta(!calculadoraAberta)}>
              <SlidersHorizontal size={13} strokeWidth={2} aria-hidden="true" />
              {calculadoraAberta ? 'Fechar a simulação' : 'Simular cenário'}
            </button>
          </div>
        }
        abas={[
          {
            id: 'parque',
            rotulo: 'Parque',
            conteudo: recorte ? (
              <PainelDoPotencialDoRecorte recorte={recorte} comFiltro={!semFiltro} />
            ) : (
              <MetricaAusente metrica="Parque de máquinas"
                motivo="Nenhuma regra de potencial está vigente para o recorte consultado, então não há quantos hectares pedem uma máquina — e sem isso não há parque a somar."
              />
            ),
          },
          {
            id: 'demanda',
            rotulo: 'Demanda anual',
            conteudo:
              recorte?.demandaAnualDeMaquinas != null ? (
                <p className="terr-recorte-titulo">
                  <strong>{nº(Math.round(recorte.demandaAnualDeMaquinas))}</strong> máquinas por ano no recorte
                  consultado — o parque dividido pelo ciclo de renovação de cada cultura.
                </p>
              ) : (
                <MetricaAusente metrica="Demanda anual"
                  motivo={`${
                    recorte ? MOTIVO_SEM_PARQUE[recorte.motivoSemDemanda] : 'não há regra de potencial vigente'
                  }. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas; sem elas, somar só as culturas que têm ciclo daria um total menor que o real, com cara de completo.`}
                />
              ),
          },
          {
            id: 'cenarios',
            rotulo: 'Cenários',
            conteudo: (
              <MetricaAusente metrica="Cenários do potencial"
                motivo="O fator de ciclo e os três cenários existem no motor (issue 74) e já aparecem na calculadora, mas ainda não são calculados para o recorte desta tela. A matriz conservador / moderado / otimista entra na fase T5."
              />
            ),
          },
        ]}
      />
    </section>
  );
}
