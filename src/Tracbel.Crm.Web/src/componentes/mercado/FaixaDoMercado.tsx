/**
 * A RÉGUA DO MERCADO — momento, porte e o que a região tem (fase T4.8; desenho
 * da maquete desde 23/09/2026).
 *
 * UM CARTÃO BRANCO NUMA LINHA SÓ: o momento, um filete, o porte, outro filete, e
 * os cinco indicadores estruturais. Entre os cinco não há filete — na maquete
 * eles são um grupo, e o que os separa é espaço.
 *
 * POR QUE ELES FICAM JUNTOS. Antes eram dois blocos empilhados — uma faixa de
 * "porte e momento" e outra de "o que a região tem". São a mesma leitura: *como
 * está o mercado, e o que existe nele*.
 *
 * O QUE SAIU DO CORPO FOI PARA AS DICAS, e nada se perdeu:
 *
 * - O FATOR NUMÉRICO E A PROCEDÊNCIA DO MOMENTO saíram de dentro da pílula. A
 *   pílula diz a faixa ("RETRAÍDO"), a frase ao lado diz a leitura, e o número,
 *   a conta e a fonte estão na dica ao lado do título — uma só, como na maquete.
 * - A COMPARAÇÃO E A PROCEDÊNCIA DE CADA INDICADOR eram duas dicas empilhadas
 *   por número. Viraram uma, com as duas partes separadas dentro: a fatia (que a
 *   tela calcula) e a fonte (que vem do contrato, nunca escrita à mão — issue
 *   167).
 * - O PORTE CONTINUA SEM NOME enquanto a issue 166 não tiver bandas: no lugar da
 *   pílula "MÉDIO" da maquete fica o traço com o motivo.
 */

import {
  Beef,
  ChartNoAxesColumn,
  Factory,
  Landmark,
  Sprout,
  Tractor,
  TrendingUp,
  type LucideIcon,
} from 'lucide-react';
import { InfoTooltip } from '../InfoTooltip';
import { frasesDaProcedencia } from '../comum/comparacoes';
import { ValorAusente } from '../comum/ValorAusente';
import type { Indicador } from '../cadastro/Indicadores';
import type { MomentoDoRecorte } from '../../tipos/territorio';

/**
 * O ícone de cada indicador estrutural, pelo rótulo.
 *
 * Por rótulo e não por posição: a ordem da faixa vem de `kpisDoMercado`, e
 * amarrar o ícone ao índice faria o trator virar boi no dia em que alguém
 * reordenasse a lista. Rótulo desconhecido simplesmente não ganha ícone — é
 * melhor que ganhar o ícone errado.
 */
const ICONES: Record<string, LucideIcon> = {
  'Parque de tratores': Tractor,
  Propriedades: Landmark,
  'Valor da lavoura': Sprout,
  'Usinas de etanol': Factory,
  'Rebanho bovino': Beef,
};

/** Casas fixas: um fator neutro tem de sair `1,00`, e não `1`. */
const pt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Por que o agregado não saiu — dito na língua de quem lê, com a decisão que falta. */
function motivoEmPortugues(motivo: string): string {
  if (motivo === 'SemDemandaEstrutural')
    return (
      'Nenhuma cultura do recorte tem ciclo de renovação informado, então não há demanda estrutural para ajustar. ' +
      'A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro ' +
      'juntas. Sem ela o fator fica ausente: "não há base para dizer" é diferente de "o mercado está neutro".'
    );

  if (motivo === 'SemFatorPorCultura')
    return (
      'Há demanda estrutural, mas nenhuma cultura produziu fator: faltam os pesos das sensibilidades (D-P05). ' +
      'Peso é decisão registrada, não conta — sem ele a tela mostra a demanda estrutural sozinha.'
    );

  return 'O fator agregado não pôde ser apurado para este recorte.';
}

/**
 * Uma estatística da régua: ícone, número em cima, nome embaixo e UMA dica.
 *
 * A DICA JUNTA A FATIA E A FONTE, cada uma no seu parágrafo marcado: a fatia é
 * conta da tela (documento 50, §7), a fonte é carimbo do contrato (issue 167).
 * Juntas no mesmo balão, e nunca na mesma frase — é o que impede a fonte de ser
 * escrita à mão dentro da comparação.
 */
function Estatistica({ indicador }: { indicador: Indicador }) {
  const { rotulo, valor, deOnde, procedencia: fonte, semDado } = indicador;
  const Icone = ICONES[rotulo];
  const nome = rotulo.toLowerCase();
  const vazio = valor === null || valor === undefined;
  const comparacao = deOnde === '—' ? null : deOnde;

  // O NOME DA DICA DIZ O QUE HÁ DENTRO: só a fatia, só a fonte, ou as duas.
  const rotuloDaDica =
    comparacao && fonte
      ? `De onde vem ${nome} e quanto representa`
      : comparacao
        ? `Quanto ${nome} representa`
        : `De onde vem ${nome}`;

  return (
    <div className="mv-faixa-item" data-faixa={rotulo}>
      {Icone && (
        <span className="mv-faixa-icone" aria-hidden="true">
          <Icone size={17} strokeWidth={1.8} />
        </span>
      )}
      <div className="mv-faixa-corpo">
        {/* A DICA FICA NA LINHA DO NÚMERO, e não na do nome: numa coluna de
            ~1300px cada estatística tem ~140px, e "Parque de tratores" com um ⓘ
            ao lado quebrava em três linhas. O número é curto e tem sobra. */}
        <span className="mv-faixa-valor">
          {/* A MESMA AUSÊNCIA DA TELA INTEIRA — traço, "sem dado" para o leitor e
              a dica "Por que … não aparece" —, no lugar do número. */}
          {vazio ? <ValorAusente motivo={semDado ?? 'sem dado'} oQue={nome} /> : <strong>{valor}</strong>}
          {!vazio && (comparacao || fonte) && (
            <InfoTooltip
              rotulo={rotuloDaDica}
              texto={
                <>
                  {comparacao && <p data-parte="comparacao">{comparacao}</p>}
                  {fonte && <p data-parte="procedencia">{frasesDaProcedencia(fonte)}</p>}
                </>
              }
            />
          )}
        </span>
        <span className="mv-faixa-rotulo">{rotulo}</span>
      </div>
    </div>
  );
}

export function FaixaDoMercado({
  indicadores,
  momento,
}: {
  indicadores: Indicador[];
  momento: MomentoDoRecorte | null;
}) {
  if (indicadores.length === 0 && !momento) return null;

  const fatorAgregado = momento?.fatorAgregado ?? null;
  const faixa = momento?.faixaDoMomento ?? null;

  return (
    // "ÁREA DE ATUAÇÃO", e não "região" sozinha (decisão 2 do usuário): o
    // leitor de tela também lê este nome, e "região" confunde a Região Tracbel
    // com a sub-região Norte ou Noroeste.
    <div
      className="mv-faixa"
      data-bloco="faixa-do-mercado"
      role="group"
      aria-label="O momento do mercado e o que a área de atuação tem"
    >
      {/* ---- MOMENTO ---- */}
      <div className="mv-faixa-leitura" data-bloco="porte-e-momento">
        <TrendingUp className="mv-faixa-glifo" data-tom="momento" size={26} strokeWidth={2.2} aria-hidden="true" />
        <div className="mv-faixa-texto">
          <span className="mv-faixa-titulo">
            Momento do mercado
            {/* A DICA DO TÍTULO LEVA O NÚMERO, A CONTA E A FONTE. As três parcelas
                não cabem aqui porque são de cada cultura; quem quiser vê-las abre
                o bloco "Momento do mercado", na composição do fator. */}
            <InfoTooltip
              rotulo="Como o momento do mercado é composto"
              texto={
                <>
                  {fatorAgregado != null && (
                    <p>
                      <strong>
                        Fator agregado {pt(fatorAgregado)}
                        {faixa ? ` — ${faixa}` : ''}.
                      </strong>
                    </p>
                  )}
                  <p>
                    Este número é a razão entre a demanda ajustada somada e a demanda estrutural somada — cada
                    cultura pesa pela demanda que representa. As três parcelas (rentabilidade, crédito e percepção)
                    são calculadas POR CULTURA e estão abertas em "Momento do mercado", na aba "Composição do fator",
                    uma linha por cultura. Não há três setas aqui porque não existe uma decomposição agregada:
                    inventá-la só para desenhar as setas seria um número sem conta.
                  </p>
                  {momento?.procedencia && <p>{frasesDaProcedencia(momento.procedencia)}</p>}
                </>
              }
            />
          </span>
          <span className="mv-faixa-linha">
            {fatorAgregado == null ? (
              <ValorAusente motivo={motivoEmPortugues(momento?.motivoSemFator ?? '')} oQue="o momento do mercado" />
            ) : (
              <span className="mv-pilula" data-faixa={faixa ?? undefined}>
                {faixa ?? 'sem faixa'}
              </span>
            )}
            {/* A FRASE DO MOMENTO, NA LINHA DA PÍLULA (maquete: "Demanda estável
                e preços firmes"). Ela vem pronta da API (`leitura`) — "Mercado
                grande, agora retraído" é a leitura que a palavra sozinha não dá.
                Vazia quando falta um dos dois lados. */}
            {momento?.leitura && <span className="mv-faixa-frase">{momento.leitura}</span>}
          </span>
        </div>
      </div>

      {/* ---- PORTE ---- */}
      <div className="mv-faixa-leitura">
        <ChartNoAxesColumn className="mv-faixa-glifo" size={26} strokeWidth={2.2} aria-hidden="true" />
        <div className="mv-faixa-texto">
          <span className="mv-faixa-titulo">
            Porte estrutural
            <InfoTooltip
              rotulo="O que é o porte estrutural"
              texto={
                'O tamanho do mercado pela demanda estrutural — as máquinas que o parque renova por ano, antes do ' +
                'momento. O número está em "Demanda anual", no alto da aba; o nome da faixa depende das bandas da ' +
                'issue 166.'
              }
            />
          </span>
          <span className="mv-faixa-linha">
            {momento?.porte ? (
              <span className="mv-pilula" data-faixa="porte">
                {momento.porte}
              </span>
            ) : (
              <ValorAusente
                motivo={
                  'O nome do porte — pequeno, médio ou grande — depende de bandas registradas, e elas ainda não ' +
                  'foram decididas (issue 166). Um corte sem dono é parâmetro inventado, então a tela mostra o ' +
                  'número da demanda anual acima e não dá nome. Nulo aqui NÃO quer dizer "pequeno".'
                }
                oQue="o nome do porte"
              />
            )}
          </span>
        </div>
      </div>

      {/* ---- OS CINCO INDICADORES ESTRUTURAIS ---- */}
      <div className="mv-faixa-numeros">
        {indicadores.map((i) => (
          <Estatistica key={i.rotulo} indicador={i} />
        ))}
      </div>
    </div>
  );
}
