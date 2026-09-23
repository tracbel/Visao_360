/** O que a consulta alcança: a visão escolhida e se o perfil pode ver a empresa inteira. */
export function CartaoDeAlcance({
  visao,
  empresa,
  podeVerEmpresaInteira,
}: {
  visao: 'Filial' | 'Empresa';
  empresa: string;
  /** Nulo enquanto a resposta não chegou — o cartão não afirma permissão que ainda não leu. */
  podeVerEmpresaInteira: boolean | undefined;
}) {
  return (
    <div className="card cad-cartao terr-cartao" data-bloco="alcance">
      <div className="terr-alcance">
        <span>
          <strong>Visão:</strong>{' '}
          {visao === 'Empresa'
            ? 'empresa inteira — cada venda no município do cliente, qualquer que seja a filial que faturou.'
            : `filial ${empresa || 'do login'} e as abaixo dela — os clientes cadastrados nela e as notas que ela emitiu.`}
        </span>
        <span>
          <strong>Empresa inteira:</strong>{' '}
          {podeVerEmpresaInteira
            ? 'disponível para o seu perfil.'
            : 'exige a permissão de alcance entre filiais; o seu perfil não a tem, e a distribuição oficial dos acessos está pendente (documento 32, P-10).'}
        </span>
        <span>A ADR e a área plantada são da empresa inteira e aparecem para todos.</span>
      </div>
    </div>
  );
}
