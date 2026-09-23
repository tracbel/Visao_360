/**
 * O município escolhido, acima das abas (fase T1 do documento 50).
 *
 * ELE FICA FORA DAS ABAS DE PROPÓSITO: a escolha é do recorte, e não da aba.
 * Clicar num município no mapa (Mercado) e ir para Território tem de continuar
 * falando do mesmo município — é o que a issue 163 pede, e é o que este chip
 * torna visível.
 *
 * SOME QUANDO NÃO HÁ ESCOLHA, em vez de ficar um espaço reservado dizendo
 * "nenhum município": um rótulo permanente treinaria o olho a ignorá-lo.
 */

export function ChipDoMunicipio({ nome, aoLimpar }: { nome: string | null; aoLimpar: () => void }) {
  if (!nome) return null;

  return (
    <div className="terr-chip" data-bloco="chip-municipio">
      <span className="terr-chip-rotulo">Município selecionado:</span>
      <span className="terr-chip-nome">{nome}</span>
      <button type="button" onClick={aoLimpar} aria-label={`Tirar o recorte de ${nome}`}>
        ×
      </button>
    </div>
  );
}
