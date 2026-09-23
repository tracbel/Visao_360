/**
 * COMO ESTE NÚMERO SE COMPARA — os componentes da regra do documento 50, §7.
 *
 * As contas e a formatação moram em `comparacoes.ts`, uma vez só: espalhar a
 * regra de comparação pelos painéis é como ela se perde. Aqui ficam só os
 * componentes que a desenham.
 */

import { InfoTooltip } from '../InfoTooltip';
import type { MedidaDeRazao, MedidaSomavel } from '../../tipos/territorio';
import { distanciasEmTexto, fatiasEmTexto } from './comparacoes';
import { Procedencia } from './Procedencia';
import { ValorAusente } from './ValorAusente';

/**
 * A GRANULARIDADE DA TELA NUNCA PODE SER MAIOR QUE A DA FONTE (issue 168).
 *
 * Preço e custo são publicados por estado ou por localidade de referência da
 * CONAB. Escolher um município não os transforma em dado municipal — ele diz
 * QUAIS culturas importam ali, e só. Esta linha diz isso onde a pessoa está
 * olhando, em vez de deixar a tela sugerir uma precisão que a fonte não tem.
 */
export function ReferenciaNaoEMunicipal({
  nomeDoMunicipio,
  fonte,
}: {
  nomeDoMunicipio: string | null;
  /** Como a fonte publica — "São Paulo", "a localidade de referência da CONAB". */
  fonte: string;
}) {
  if (!nomeDoMunicipio) return null;
  return (
    <p className="cad-sub">
      Referência: <strong>{fonte}</strong>. Escolher {nomeDoMunicipio} destaca as culturas dele, e não muda o número.{' '}
      <InfoTooltip
        rotulo="Por que este número não é do município"
        texto={`Esta série é publicada para ${fonte}; não existe versão municipal dela. A tela não reparte um número estadual por município, porque isso inventaria uma precisão que a fonte não tem. O município escolhido serve para dizer quais culturas importam aqui.`}
      />
    </p>
  );
}

/**
 * Uma grandeza somável, com as duas fatias embaixo.
 *
 * @param formatar Como o número se escreve — hectares, reais compactos, inteiro.
 */
export function ComparacaoSomavel({
  medida,
  oQue,
  formatar,
}: {
  medida: MedidaSomavel;
  oQue: string;
  formatar: (valor: number) => string;
}) {
  if (medida.valor === null) {
    return <ValorAusente motivo={medida.motivoDaAusencia ?? `${oQue} não foi carregado para este recorte.`} oQue={oQue} />;
  }

  const fatias = fatiasEmTexto(medida);

  return (
    <span className="cad-comparacao">
      <span className="cad-comparacao-valor cad-mono">
        {formatar(medida.valor)}
        <Procedencia procedencia={medida.procedencia} oQue={oQue} />
      </span>
      {fatias && <span className="cad-comparacao-contexto">{fatias}</span>}
    </span>
  );
}

/** Uma grandeza do tipo razão, com a distância até as referências embaixo. */
export function ComparacaoDeRazao({
  medida,
  oQue,
  formatar,
}: {
  medida: MedidaDeRazao;
  oQue: string;
  formatar: (valor: number) => string;
}) {
  if (medida.valor === null) {
    return <ValorAusente motivo={medida.motivoDaAusencia ?? `${oQue} não foi apurado para este recorte.`} oQue={oQue} />;
  }

  const distancias = distanciasEmTexto(medida);

  return (
    <span className="cad-comparacao">
      <span className="cad-comparacao-valor cad-mono">
        {formatar(medida.valor)}
        {medida.unidade && <span className="cad-comparacao-unidade"> {medida.unidade}</span>}
        <Procedencia procedencia={medida.procedencia} oQue={oQue} />
      </span>
      {distancias && <span className="cad-comparacao-contexto">{distancias}</span>}
    </span>
  );
}
