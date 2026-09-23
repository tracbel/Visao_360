import { InfoTooltip } from '../InfoTooltip';

/**
 * O título da seção "O mercado da região".
 *
 * A §10.3 do documento 49 e o documento 50 mandam preservá-la exatamente como
 * está, com a linha dela: é a seção que a diretoria já reconhece.
 */
export function SecaoDoMercadoDaRegiao() {
  return (
    <div className="terr-secao-mercado" data-bloco="mercado-da-regiao">
      <h2 className="terr-secao-titulo">
        O mercado da região
        {/* O PARÁGRAFO SOBRE O DENOMINADOR VIROU DICA (fase T2.1): é método, e
            método é nível 2 da hierarquia da issue 33 — não primeira camada. */}
        <InfoTooltip
          rotulo="Como ler as fatias desta seção"
          texto={
            'O denominador de São Paulo é o total PUBLICADO pelo IBGE, e não a soma dos municípios: o valor ' +
            'municipal sigiloso entra no total do estado sem aparecer embaixo. O denominador da Região Tracbel é a ' +
            'área de atuação inteira, e não muda quando o filtro de sub-região muda.'
          }
        />
      </h2>
      <p className="terr-secao-subtitulo">O que existe no território, por fonte pública.</p>
    </div>
  );
}
