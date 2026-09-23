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
 */

import { useState } from 'react';
import { MetricaAusente } from '../comum/MetricaAusente';
import { ReferenciaNaoEMunicipal } from '../comum/Comparacao';
import { PainelDeCredito } from '../territorio/PainelDeCredito';
import { PainelDeCustos } from '../territorio/PainelDeCustos';
import { PainelDePrecos } from '../territorio/PainelDePrecos';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { AbasInternas } from './AbasInternas';

type SubAba = 'rentabilidade' | 'credito' | 'troca' | 'percepcao';

export function BlocoDoMomento({
  municipioSelecionado = null,
  nomeDoMunicipio = null,
  produtosDoMunicipio = [],
}: {
  municipioSelecionado?: number | null;
  nomeDoMunicipio?: string | null;
  /** Os produtos da PAM do municipio, por area — priorizam as culturas de preco e custo (issue 168). */
  produtosDoMunicipio?: readonly number[];
} = {}) {
  const [subAba, setSubAba] = useState<SubAba>('rentabilidade');

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
            id: 'rentabilidade',
            rotulo: 'Rentabilidade',
            conteudo: (
              <>
                {/* A FONTE É ESTADUAL, e a tela diz isso (issue 168): escolher um
                    município não reparte um preço de São Paulo por município. */}
                <ReferenciaNaoEMunicipal nomeDoMunicipio={nomeDoMunicipio} fonte="São Paulo" />
                <PainelDePrecos produtosDoMunicipio={produtosDoMunicipio} />
                <ReferenciaNaoEMunicipal
                  nomeDoMunicipio={nomeDoMunicipio}
                  fonte="a localidade de referência da CONAB"
                />
                <PainelDeCustos produtosDoMunicipio={produtosDoMunicipio} />
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
