/**
 * Momento do mercado — Rentabilidade | Crédito | Termo de troca | Percepção
 * comercial (documento 50, §4.6).
 *
 * OS TRÊS PAINÉIS DO FIM DA PÁGINA MUDARAM DE LUGAR, NÃO DE CONTEÚDO. Preço,
 * custo e crédito estavam empilhados no rodapé, cada um com o mesmo peso visual
 * de tudo o mais; agora são as abas do bloco que responde "o mercado está melhor
 * ou pior que antes".
 *
 * DUAS ABAS NASCEM VAZIAS, E ISSO É A RESPOSTA CERTA. Termo de troca precisa do
 * preço de máquina (issue 70) e percepção precisa que esta tela leia o parâmetro
 * (issue 71): as duas dizem o que falta, no lugar onde o número apareceria. Um
 * número plausível e errado leva a uma decisão; um espaço explicado leva a uma
 * pergunta.
 *
 * A COMPOSIÇÃO DO FATOR ABRE O BLOCO (fase T3.1). O resumo executivo mostra o
 * fator agregado e mais nada; a conta que o produz — cultura por cultura, com as
 * três parcelas de cada uma — é a primeira aba daqui. É o lugar certo: quem
 * pergunta "por que 0,88?" já está olhando o momento do mercado.
 */

import { useState } from 'react';
import { MetricaAusente } from '../comum/MetricaAusente';
import { ReferenciaNaoEMunicipal } from '../comum/Comparacao';
import { PainelDeCredito } from '../territorio/PainelDeCredito';
import { PainelDeCustos } from '../territorio/PainelDeCustos';
import { PainelDePrecos } from '../territorio/PainelDePrecos';
import { PainelDoPrecoImplicito } from '../territorio/PainelDoPrecoImplicito';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { AbasInternas } from './AbasInternas';
import { ComposicaoDoFator } from './ComposicaoDoFator';
import { PainelDeRentabilidade } from './PainelDeRentabilidade';
import type { MomentoDoRecorte } from '../../tipos/territorio';

type SubAba = 'composicao' | 'rentabilidade' | 'credito' | 'troca' | 'percepcao';

export function BlocoDoMomento({
  municipioSelecionado = null,
  nomeDoMunicipio = null,
  produtosDoMunicipio = [],
  momento = null,
}: {
  municipioSelecionado?: number | null;
  nomeDoMunicipio?: string | null;
  /** Os produtos da PAM do municipio, por area — priorizam as culturas de preco e custo (issue 168). */
  produtosDoMunicipio?: readonly number[];
  /** O momento do recorte — a composição por cultura do fator agregado (fase T3.1). */
  momento?: MomentoDoRecorte | null;
} = {}) {
  const [subAba, setSubAba] = useState<SubAba>('composicao');

  return (
    <section data-bloco="momento-do-mercado">
      <TituloDaSecao
        titulo="Momento do mercado"
        subtitulo="O que mudou desde a safra passada."
        metodologia={
          'Preço, custo, crédito e a leitura do comercial. Este bloco anda todo mês, diferente do potencial ' +
          'estrutural, que anda devagar — e é por isso que os dois ficam separados: "mercado grande, agora retraído" ' +
          'é uma decisão diferente de "mercado pequeno e aquecido".'
        }
      />

      <AbasInternas
        rotulo="O que o momento mostra"
        ativa={subAba}
        aoTrocar={setSubAba}
        abas={[
          {
            id: 'composicao',
            rotulo: 'Composição do fator',
            // A CONTA DO NÚMERO DO TOPO, aberta: cultura, índice dela, fator dela
            // e as três parcelas. Some a coluna e confira.
            conteudo: <ComposicaoDoFator momento={momento} />,
          },
          {
            id: 'rentabilidade',
            rotulo: 'Rentabilidade',
            // A RESPOSTA VEM PRIMEIRO (fase T4.7). Esta aba abria com duas
            // tabelas de dez colunas — preço e custo — e a pergunta que a
            // diretoria traz ("onde sobra dinheiro por hectare?") tinha de ser
            // montada cruzando as duas com a cabeça. Agora o painel de margem
            // abre a aba, e preço e custo ficam abaixo, como a evidência que
            // eles são. Nenhuma tabela foi removida.
            conteudo: (
              <>
                {/* A FONTE É ESTADUAL, e a tela diz isso (issue 168): escolher um
                    município não reparte um preço de São Paulo por município. */}
                <ReferenciaNaoEMunicipal nomeDoMunicipio={nomeDoMunicipio} fonte="São Paulo" />
                <PainelDeRentabilidade produtosDoMunicipio={produtosDoMunicipio} />

                <details className="cad-recolhivel" data-bloco="rentabilidade-fontes">
                  <summary>As duas séries que compõem a margem — preço e custo</summary>
                  <PainelDePrecos produtosDoMunicipio={produtosDoMunicipio} />

                  {/* A SEGUNDA SÉRIE DE PREÇO, DO IBGE (issue 198) — ao lado da CONAB, e NUNCA emendada
                      nela. A da CONAB é mensal e por UF; esta é anual e por município, vai até 2010 e
                      entrega as médias de 3 e 5 anos. A distância entre as duas, medida em 22/09, vai de
                      +1,1% na soja a −28,4% no amendoim: emendá-las criaria um degrau artificial na
                      virada, e é por isso que são dois painéis. */}
                  <PainelDoPrecoImplicito
                    municipioCodigoIbge={municipioSelecionado}
                    nomeDoMunicipio={nomeDoMunicipio}
                    produtosDoMunicipio={produtosDoMunicipio}
                  />
                  <ReferenciaNaoEMunicipal
                    nomeDoMunicipio={nomeDoMunicipio}
                    fonte="a localidade de referência da CONAB"
                  />
                  <PainelDeCustos produtosDoMunicipio={produtosDoMunicipio} />
                </details>
              </>
            ),
          },
          {
            id: 'credito',
            rotulo: 'Crédito',
            // O SICOR PUBLICA POR MUNICÍPIO: aqui o recorte muda o que se lê.
            conteudo: <PainelDeCredito municipioSelecionado={municipioSelecionado} />,
          },
          {
            id: 'troca',
            rotulo: 'Termo de troca',
            conteudo: (
              <MetricaAusente metrica="Termo de troca"
                motivo="Quantas sacas o produtor precisa hoje para comprar uma máquina, contra cinco anos atrás. Precisa do preço de máquina por modelo ao longo do tempo (issue 70), que não existe no CRM — e o preço da saca sozinho não responde a pergunta."
              />
            ),
          },
          {
            id: 'percepcao',
            rotulo: 'Percepção comercial',
            conteudo: (
              <MetricaAusente metrica="Percepção comercial"
                motivo="O parâmetro existe, tem vigência e é informado em Configurações (issue 71), mas esta tela ainda não o lê: ele chega junto dos indicadores do recorte, na fase T3. Mostrar zero aqui seria afirmar neutralidade que ninguém declarou."
              />
            ),
          },
        ]}
      />
    </section>
  );
}
