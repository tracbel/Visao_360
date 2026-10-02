/**
 * O que a ficha do cliente e a da máquina dizem igual sobre as ordens de serviço (02/10/2026): a faixa vermelha do painel
 * de pós-venda do BI e o nome da situação da capa.
 */

/** A faixa vermelha do painel de pós-venda: aberta há mais de 45 dias. */
export const DIAS_DA_FAIXA_VERMELHA = 45;

/** Como a pessoa lê a situação da capa. */
export function rotuloDaSituacao(situacao: string): string {
  switch (situacao) {
    case 'Aberta':
      return 'aberta';
    case 'Liberada':
      return 'liberada';
    case 'Fechada':
      return 'fechada';
    case 'Cancelada':
      return 'cancelada';
    default:
      return situacao.toLowerCase();
  }
}
