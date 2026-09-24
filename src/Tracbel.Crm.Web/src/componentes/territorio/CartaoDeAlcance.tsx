/**
 * O QUE A CONSULTA ALCANÇA — hoje, um parágrafo dentro da dica da Sub-região.
 *
 * A HISTÓRIA DESTE TEXTO. Era um cartão de largura inteira com três parágrafos,
 * acima dos filtros e dos números; a T4.6 o reduziu a uma linha de metadado
 * ("Visão: filial … e as abaixo dela ⓘ"). A fidelidade às maquetes (23/09/2026)
 * deu o passo seguinte: a maquete não tem essa linha, e a decisão do usuário é
 * que o que existe hoje e não está na maquete sai do corpo da página e vai para
 * a dica mais próxima do assunto — sem perder uma palavra.
 *
 * POR QUE A DICA DA SUB-REGIÃO, e não a de "Dados atualizados em…": o alcance é
 * um RECORTE — diz se o número é de uma filial ou da empresa inteira —, e a
 * sub-região é o filtro que explica a hierarquia desse recorte (São Paulo →
 * Região Tracbel → sub-região → loja → município). A dica do cabeçalho responde
 * outra pergunta: de onde e de quando veio o dado.
 *
 * O CONTEÚDO NÃO É NECESSÁRIO MENOS DO QUE ERA: sem ele ninguém sabe se está
 * vendo uma filial ou a empresa toda, e o mesmo número significa coisas
 * diferentes nos dois casos. Ele só deixou de ocupar uma linha permanente.
 */

export function AlcanceDaConsulta({
  visao,
  empresa,
  podeVerEmpresaInteira,
}: {
  visao: 'Filial' | 'Empresa';
  empresa: string;
  /** Nulo enquanto a resposta não chegou — o texto não afirma permissão que ainda não leu. */
  podeVerEmpresaInteira: boolean | undefined;
}) {
  return (
    <p>
      <strong>Visão:</strong>{' '}
      {visao === 'Empresa' ? 'empresa inteira. ' : `filial ${empresa || 'do login'} e as abaixo dela. `}
      {visao === 'Empresa'
        ? 'Cada venda entra no município do cliente, qualquer que seja a filial que faturou. '
        : 'Entram os clientes cadastrados nela e as notas que ela emitiu. '}
      {podeVerEmpresaInteira === undefined
        ? ''
        : podeVerEmpresaInteira
          ? 'A visão da empresa inteira está disponível para o seu perfil. '
          : 'A visão da empresa inteira exige a permissão de alcance entre filiais; o seu perfil não a tem, e a ' +
            'distribuição oficial dos acessos está pendente (documento 32, P-10). '}
      A ADR e a área plantada são da empresa inteira e aparecem para todos.
    </p>
  );
}
