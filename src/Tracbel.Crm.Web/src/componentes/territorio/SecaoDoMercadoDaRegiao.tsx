/**
 * O título da seção "O mercado da região".
 *
 * A §10.3 do documento 49 e o documento 50 mandam preservá-la exatamente como
 * está, com a linha dela: é a seção que a diretoria já reconhece.
 */
export function SecaoDoMercadoDaRegiao() {
  return (
    <div className="terr-secao-mercado" data-bloco="mercado-da-regiao">
      <h2 className="terr-secao-titulo">O mercado da região</h2>
      <p className="terr-secao-subtitulo">
        O que existe no território, por fonte pública — e que fatia de São Paulo isso representa. O denominador é o
        total <strong>publicado</strong> pelo IBGE, que não é a soma dos municípios: o valor municipal sigiloso entra
        nele sem aparecer embaixo.
      </p>
    </div>
  );
}
