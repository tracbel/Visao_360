/**
 * Momento do mercado — Composição do fator | Rentabilidade | Crédito | Termo de
 * troca | Percepção comercial (documento 50, §4.6; na forma das maquetes
 * `momento-*.png` desde a fidelidade às maquetes, fase 3).
 *
 * O BLOCO É UM CARTÃO BRANCO EM LARGURA INTEIRA, logo abaixo dos quatro números
 * (a fase 1 o pôs no lugar). À esquerda o ícone de tendência, o título e uma
 * frase; à direita "Simular cenário"; embaixo as cinco abas SUBLINHADAS — e cada
 * aba segue o mesmo padrão da maquete: quatro cartões de resumo, dois ou três
 * painéis lado a lado e a tabela de detalhamento.
 *
 * DUAS ABAS TÊM MAIS LUGAR QUE DADO, E ISSO É A RESPOSTA CERTA. Termo de troca
 * precisa do preço de máquina (issue 70) e a percepção por município ainda não
 * tem série nem índice de 0 a 100 (issue 71): o layout da maquete fica inteiro,
 * e no lugar de cada número que não existe, o traço e o motivo (decisão 1 do
 * usuário). Um número plausível e errado leva a uma decisão; um espaço
 * explicado leva a uma pergunta.
 *
 * A COMPOSIÇÃO DO FATOR ABRE O BLOCO (fase T3.1): quem pergunta "por que 0,88?"
 * já está olhando o momento do mercado.
 */

// O REGISTRO DO CHART.JS ANTES DE QUALQUER GRÁFICO DO BLOCO: o crédito e a
// rentabilidade carregam o `padraoDosGraficos`, que precisa do balão já
// registrado (ver `registroDoGraficoCombinado`).
import './momento/registroDoGraficoCombinado';
import { SlidersHorizontal, TrendingUp } from 'lucide-react';
import { useState } from 'react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import type { IndicadoresDoMunicipio, MomentoDoRecorte } from '../../tipos/territorio';
import { InfoTooltip } from '../InfoTooltip';
import type { RecorteFiltrado } from '../territorio/indicadoresDaAdr';
import { PainelDeCredito } from '../territorio/PainelDeCredito';
import { Calculadora } from './Calculadora';
import { ComposicaoDoFator } from './ComposicaoDoFator';
import { PainelDeRentabilidade } from './PainelDeRentabilidade';
import { AbaPercepcaoComercial } from './momento/AbaPercepcaoComercial';
import { AbasDoMomento } from './momento/AbasDoMomento';
import { AbaTermoDeTroca } from './momento/AbaTermoDeTroca';
import '../../estilos/momento.css';

type SubAba = 'composicao' | 'rentabilidade' | 'credito' | 'troca' | 'percepcao';

const ABAS: readonly { id: SubAba; rotulo: string }[] = [
  { id: 'composicao', rotulo: 'Composição do fator' },
  { id: 'rentabilidade', rotulo: 'Rentabilidade' },
  { id: 'credito', rotulo: 'Crédito' },
  { id: 'troca', rotulo: 'Termo de troca' },
  { id: 'percepcao', rotulo: 'Percepção comercial' },
];

export function BlocoDoMomento({
  municipioSelecionado = null,
  nomeDoMunicipio = null,
  produtosDoMunicipio = [],
  momento = null,
  municipios = [],
  carregando = false,
  recorte = null,
}: {
  municipioSelecionado?: number | null;
  nomeDoMunicipio?: string | null;
  /** Os produtos da PAM do municipio, por area — priorizam as culturas de preco e custo (issue 168). */
  produtosDoMunicipio?: readonly number[];
  /** O momento do recorte — a composição por cultura do fator agregado (fase T3.1). */
  momento?: MomentoDoRecorte | null;
  /**
   * Os municípios da leitura — a área colhida da Região Tracbel (peso da margem
   * média) e o responsável pela carteira de cada um (coluna Gestor da percepção).
   */
  municipios?: readonly IndicadoresDoMunicipio[];
  /** A leitura dos indicadores ainda não voltou. */
  carregando?: boolean;
  /**
   * O recorte dos filtros (sub-região, loja), quando há. Os municípios acima já
   * vêm filtrados por ele, e o texto que fala deles diz o nome do recorte — e
   * não "Região Tracbel", que é a área de atuação inteira (issue 163).
   */
  recorte?: RecorteFiltrado | null;
} = {}) {
  const { contexto } = useContextoDeAcesso();
  const [subAba, setSubAba] = useState<SubAba>('composicao');
  const [simulando, setSimulando] = useState(false);

  return (
    <section className="mom-bloco" data-bloco="momento-do-mercado">
      <div className="mom-cabecalho">
        <div className="mom-cabecalho-titulos">
          <span className="mom-cabecalho-icone" aria-hidden="true">
            <TrendingUp size={28} strokeWidth={2.2} />
          </span>
          <h2 className="mom-titulo">
            Momento do mercado
            <InfoTooltip
              rotulo="Fonte e método de momento do mercado"
              texto={
                'Preço, custo, crédito e a leitura do comercial — o que mudou desde a safra passada. Este bloco ' +
                'anda todo mês, diferente do potencial estrutural, que anda devagar — e é por isso que os dois ficam ' +
                'separados: "mercado grande, agora retraído" é uma decisão diferente de "mercado pequeno e aquecido".'
              }
            />
          </h2>
          {/* "NA SUA ÁREA DE ATUAÇÃO", e não "na sua região" (decisão 2 do
              usuário): "região" sozinha confunde a Região Tracbel com a
              sub-região Norte ou Noroeste. */}
          <p className="mom-subtitulo">
            Principais fatores que influenciam a rentabilidade, o crédito e o comportamento do mercado na sua área de
            atuação.
          </p>
        </div>

        {/* "SIMULAR CENÁRIO" ABRE A MESMA CALCULADORA do Potencial estrutural.
            Ela é o único lugar onde os cenários do momento já existem (issue
            74): a demanda simulada volta ajustada pelo crédito, pela percepção
            e pelo preço de cada cultura, com os três cenários. */}
        <button
          type="button"
          className="mom-simular"
          aria-pressed={simulando}
          aria-label={simulando ? 'Fechar a simulação do momento do mercado' : 'Simular cenário com o momento do mercado'}
          onClick={() => setSimulando((s) => !s)}
        >
          <SlidersHorizontal size={15} strokeWidth={2} aria-hidden="true" />
          {simulando ? 'Fechar a simulação' : 'Simular cenário'}
        </button>
      </div>

      {simulando && (
        <Calculadora contexto={contexto} municipioCodigoIbge={municipioSelecionado} aoFechar={() => setSimulando(false)} />
      )}

      <AbasDoMomento rotulo="O que o momento mostra" abas={ABAS} ativa={subAba} aoTrocar={setSubAba}>
        {subAba === 'composicao' && <ComposicaoDoFator momento={momento} carregando={carregando} />}

        {subAba === 'rentabilidade' && (
          <PainelDeRentabilidade
            produtosDoMunicipio={produtosDoMunicipio}
            nomeDoMunicipio={nomeDoMunicipio}
            municipios={municipios}
            carregando={carregando}
            recorte={recorte}
          />
        )}

        {/* O SICOR PUBLICA POR MUNICÍPIO: aqui o recorte muda o que se lê. */}
        {subAba === 'credito' && <PainelDeCredito municipioSelecionado={municipioSelecionado} />}

        {subAba === 'troca' && <AbaTermoDeTroca produtosDoMunicipio={produtosDoMunicipio} />}

        {subAba === 'percepcao' && <AbaPercepcaoComercial momento={momento} municipios={municipios} recorte={recorte} />}
      </AbasDoMomento>
    </section>
  );
}
