/**
 * A ABA OPORTUNIDADES DA FICHA (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * NO TOPO, OS QUATRO NÚMEROS DE DECISÃO do município — Demanda anual, Mercado
 * anual, Captura Tracbel e Oportunidade. Eram a "camada executiva" da ficha, a
 * primeira coisa dela; a maquete da ficha não os tem, e eles vieram para cá
 * inteiros, com os mesmos motivos (decisão 3: nada de substância é apagado).
 * Esta é a aba do "quanto dá para vender aqui", e é a pergunta que eles
 * respondem.
 *
 * O MOTIVO DE MERCADO ANUAL, CAPTURA E OPORTUNIDADE É A FRASE DO SERVIDOR (issue
 * 69, parte A), a mesma dos cartões do topo da aba Mercado — e não uma
 * constante escrita aqui, que era a terceira redação da mesma ausência. Os
 * números da API são do RECORTE: o que falta para ele falta para o município.
 * Quando o recorte passar a ter o número, a frase vem vazia e a ficha fica com o
 * traço sem dica até existir a conta por município — ela não herda o número do
 * recorte, que seria de outro lugar.
 *
 * EMBAIXO, A LISTA DE OPORTUNIDADES COM CONFIANÇA E ORIGEM (issue 162), com o
 * desenho pronto e vazia: nenhuma oportunidade existe ainda, porque ela depende
 * das vendas em unidades por município (issue 69) e da classificação de
 * confiança da 162. Uma lista com linhas de exemplo seria dado inventado.
 */

import { Inbox } from 'lucide-react';
import type { IndicadoresDoMunicipio, NumerosDeDecisao } from '../../../tipos/territorio';
import { InfoTooltip } from '../../InfoTooltip';
import { CartaoDeIndicador, GradeDeIndicadores } from '../../dashboard/Dashboard';

const nº = (v: number) => v.toLocaleString('pt-BR');

/** As três confianças da issue 162, com a origem de cada uma — a legenda da lista. */
const CONFIANCAS = [
  { nivel: 'alta', rotulo: 'Alta', origem: 'venda perdida registrada no CRM' },
  { nivel: 'media', rotulo: 'Média', origem: 'cliente da carteira com chassi acima do ciclo e sem negociação aberta' },
  { nivel: 'baixa', rotulo: 'Baixa', origem: 'crédito de máquina no município sem venda Tracbel correspondente' },
] as const;

export function OportunidadesDoMunicipio({
  municipio,
  numerosDeDecisao,
}: {
  municipio: IndicadoresDoMunicipio;
  /** Os números de decisão do recorte — daqui só sai a frase de cada ausência. */
  numerosDeDecisao: NumerosDeDecisao | null;
}) {
  const motor = municipio.potencialEstrutural;

  return (
    <div className="terr-ficha-oportunidades">
      <section className="terr-ficha-bloco" data-camada="decisao" aria-labelledby="ficha-decide-titulo">
        <h3 id="ficha-decide-titulo" className="terr-ficha-bloco-titulo">
          O que este município decide
        </h3>
        <GradeDeIndicadores>
          <CartaoDeIndicador
            rotulo="Demanda anual"
            // COM ARTIGO, porque é assim que esta dica já se chamava aqui — e
            // mudar o nome acessível de um controle que as pessoas conhecem é
            // uma regressão silenciosa para quem usa leitor de tela.
            oQue="a demanda anual"
            destaque
            valor={motor?.demandaAnualDeMaquinas == null ? null : `${nº(motor.demandaAnualDeMaquinas)} máq/ano`}
            contexto="o que o parque daqui renova por ano"
            motivoSemDado={
              motor?.motivoSemDemanda === 'SemCicloDeRenovacao'
                ? 'A regra não informou de quantos em quantos anos a máquina é trocada. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas.'
                : 'Sem parque, não há o que renovar.'
            }
          />
          <CartaoDeIndicador
            rotulo="Mercado anual"
            valor={null}
            motivoSemDado={numerosDeDecisao?.mercadoAnual.frase}
          />
          <CartaoDeIndicador
            rotulo="Captura Tracbel"
            valor={null}
            motivoSemDado={numerosDeDecisao?.capturaPercentual.frase}
          />
          <CartaoDeIndicador
            rotulo="Oportunidade"
            valor={null}
            motivoSemDado={numerosDeDecisao?.oportunidade.frase}
          />
        </GradeDeIndicadores>
      </section>

      <section className="terr-ficha-bloco" data-bloco-da-ficha="lista-de-oportunidades" aria-labelledby="ficha-lista-titulo">
        <h3 id="ficha-lista-titulo" className="terr-ficha-bloco-titulo">
          Oportunidades
          <InfoTooltip
            rotulo="Como uma oportunidade é classificada"
            texto='Toda oportunidade traz a confiança e a origem — a legenda embaixo da lista —, e nenhuma vira "venda perdida" sozinha (issue 162, D-IM-09).'
          />
        </h3>
        <table className="terr-ficha-oport-lista">
          <caption className="cad-so-leitor">Oportunidades no município, com confiança e origem</caption>
          <thead>
            <tr>
              <th scope="col">Oportunidade</th>
              <th scope="col">Confiança</th>
              <th scope="col">Origem</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td colSpan={3}>
                <span className="terr-ficha-vazio">
                  <Inbox size={16} strokeWidth={2} aria-hidden="true" />
                  <span aria-hidden="true">—</span>
                  <span>aguarda vendas por município (#69) e a confiança da #162</span>
                  <InfoTooltip
                    rotulo="Por que não há oportunidades listadas"
                    texto="A oportunidade é a demanda ajustada menos as vendas da Tracbel em unidades, e a confiança dela vem da origem — venda perdida, chassi acima do ciclo ou crédito sem venda. As vendas em unidades por município ainda não existem (issue 69), e a classificação de confiança é a issue 162. Até lá a lista fica vazia, e não com exemplos."
                  />
                </span>
              </td>
            </tr>
          </tbody>
        </table>
        <ul className="terr-ficha-confiancas" aria-label="Níveis de confiança">
          {CONFIANCAS.map((c) => (
            <li key={c.nivel}>
              <span className="terr-ficha-confianca" data-nivel={c.nivel}>
                {c.rotulo}
              </span>
              {c.origem}
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
