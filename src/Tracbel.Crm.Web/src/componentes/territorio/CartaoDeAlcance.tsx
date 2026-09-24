/**
 * O QUE A CONSULTA ALCANÇA — uma linha, não um cartão (redesenhado na T4.6).
 *
 * Isto era um cartão de largura inteira com três parágrafos, acima dos filtros e
 * dos números. O conteúdo é necessário — sem ele ninguém sabe se está vendo uma
 * filial ou a empresa toda, e o mesmo número significa coisas diferentes nos dois
 * casos — mas ele é **contexto do recorte**, não um indicador, e estava desenhado
 * com o peso de um.
 *
 * Virou uma linha de metadado com o essencial dito e o resto na dica: quem
 * precisa saber por que a empresa inteira está indisponível continua a um passo
 * dessa explicação.
 */

import { InfoTooltip } from '../InfoTooltip';

export function CartaoDeAlcance({
  visao,
  empresa,
  podeVerEmpresaInteira,
}: {
  visao: 'Filial' | 'Empresa';
  empresa: string;
  /** Nulo enquanto a resposta não chegou — a linha não afirma permissão que ainda não leu. */
  podeVerEmpresaInteira: boolean | undefined;
}) {
  return (
    <p className="dash-alcance" data-bloco="alcance">
      <strong>Visão:</strong>{' '}
      {visao === 'Empresa' ? 'empresa inteira' : `filial ${empresa || 'do login'} e as abaixo dela`}
      <InfoTooltip
        rotulo="O que esta consulta alcança"
        texto={
          (visao === 'Empresa'
            ? 'Empresa inteira: cada venda entra no município do cliente, qualquer que seja a filial que faturou. '
            : `Filial ${empresa || 'do login'} e as abaixo dela: os clientes cadastrados nela e as notas que ela emitiu. `) +
          (podeVerEmpresaInteira
            ? 'A visão da empresa inteira está disponível para o seu perfil.'
            : 'A visão da empresa inteira exige a permissão de alcance entre filiais; o seu perfil não a tem, e a ' +
              'distribuição oficial dos acessos está pendente.') +
          ' A ADR e a área plantada são da empresa inteira e aparecem para todos.'
        }
      />
    </p>
  );
}
