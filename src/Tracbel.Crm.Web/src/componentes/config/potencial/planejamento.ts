/**
 * O planejamento comercial na tela de Configurações (issue 256): os meses, os pesos do IOC com os nomes que a tela
 * mostra, o formulário já preenchido com o que vale hoje e as linhas da trilha.
 */

import type {
  HistoricoDoPlanejamento,
  NovoParametroDoPlanejamento,
  ParametroDoPlanejamentoDetalhe,
  PesosDoIoc,
} from '../../../tipos/potencial';
import { estadoDaVigencia, iniciosVigentes, numero, type LinhaDoHistorico } from './vigencias';

export const MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'] as const;

/** Os sete componentes do IOC, na ordem do índice, com o campo do formulário e o que cada um mede. */
export const COMPONENTES_DO_IOC: readonly {
  chave: keyof PesosDoIoc;
  campo: keyof NovoParametroDoPlanejamento;
  rotulo: string;
  mede: string;
}[] = [
  { chave: 'potencial', campo: 'pesoDoPotencial', rotulo: 'Potencial', mede: 'o potencial do município ajustado pelo momento' },
  { chave: 'cobertura', campo: 'pesoDaCobertura', rotulo: 'Cobertura', mede: 'os clientes da carteira sem contato nos últimos 90 dias' },
  { chave: 'credito', campo: 'pesoDoCredito', rotulo: 'Crédito', mede: 'o momento do crédito de mecanização no município' },
  { chave: 'rentabilidade', campo: 'pesoDaRentabilidade', rotulo: 'Rentabilidade', mede: 'o momento do preço da cultura principal' },
  { chave: 'clientes', campo: 'pesoDosClientes', rotulo: 'Clientes', mede: 'poucos clientes frente ao potencial' },
  { chave: 'realizacao', campo: 'pesoDaRealizacao', rotulo: 'Realização', mede: 'a distância até a meta de share' },
  { chave: 'penetracao', campo: 'pesoDaPenetracao', rotulo: 'Penetração', mede: 'a baixa penetração da Tracbel no potencial' },
];

/** A soma dos doze meses, em número, ignorando o que ainda não é número. */
export function somaDaSazonalidade(meses: readonly string[]): number {
  return meses.reduce((soma, mes) => {
    const valor = Number(mes.replace(',', '.'));
    return Number.isFinite(valor) ? soma + valor : soma;
  }, 0);
}

/** Cada peso como fatia do índice — a normalização pela soma, que é o que o cálculo faz. */
export function fatiasDoIoc(pesos: PesosDoIoc): Record<keyof PesosDoIoc, number> {
  const soma = Object.values(pesos).reduce((a, b) => a + b, 0);
  const fatias = {} as Record<keyof PesosDoIoc, number>;
  for (const { chave } of COMPONENTES_DO_IOC) fatias[chave] = soma > 0 ? (pesos[chave] / soma) * 100 : 0;
  return fatias;
}

const texto = (valor: number) => String(valor).replace('.', ',');

/** O formulário já preenchido com a vigência de hoje: mudar um mês é registrar o conjunto com ele trocado. */
export function formularioDoPlanejamento(vigente: ParametroDoPlanejamentoDetalhe | null, hoje: string): NovoParametroDoPlanejamento {
  const pesos = vigente?.pesos;
  return {
    vigenteDesde: hoje,
    sazonalidade: vigente ? vigente.sazonalidade.map(texto) : MESES.map(() => ''),
    pesoDoPotencial: pesos ? texto(pesos.potencial) : '',
    pesoDaCobertura: pesos ? texto(pesos.cobertura) : '',
    pesoDoCredito: pesos ? texto(pesos.credito) : '',
    pesoDaRentabilidade: pesos ? texto(pesos.rentabilidade) : '',
    pesoDosClientes: pesos ? texto(pesos.clientes) : '',
    pesoDaRealizacao: pesos ? texto(pesos.realizacao) : '',
    pesoDaPenetracao: pesos ? texto(pesos.penetracao) : '',
    justificativa: '',
  };
}

/** As linhas da trilha do planejamento, no mesmo formato das do potencial — a trilha da tela é uma só. */
export function montarHistoricoDoPlanejamento(historico: HistoricoDoPlanejamento, hoje: string): LinhaDoHistorico[] {
  const planejamentos = iniciosVigentes(historico.planejamentos, () => 'planejamento', hoje);
  const shares = iniciosVigentes(historico.shares, (s) => s.categoriaDeMaquinaCodigo, hoje);

  return [
    ...historico.planejamentos.map((p): LinhaDoHistorico => {
      const pico = p.sazonalidade.indexOf(Math.max(...p.sazonalidade));
      return {
        id: `planejamento-${p.vigencia.vigenteDesde}-${p.vigencia.informadoEm}`,
        tipo: 'Sazonalidade e pesos do IOC',
        chave: '—',
        resumo:
          `Pico em ${MESES[pico]} (${numero(p.sazonalidade[pico])}%) · pesos ` +
          COMPONENTES_DO_IOC.map((c) => `${c.rotulo.toLowerCase()} ${numero(p.pesos[c.chave])}`).join(', '),
        vigencia: p.vigencia,
        estado: estadoDaVigencia(p.vigencia, planejamentos.get('planejamento'), hoje),
        alvo: { tipo: 'planejamento', vigenteDesde: p.vigencia.vigenteDesde },
      };
    }),
    ...historico.shares.map((s): LinhaDoHistorico => ({
      id: `share-${s.categoriaDeMaquinaCodigo}-${s.vigencia.vigenteDesde}-${s.vigencia.informadoEm}`,
      tipo: 'Share-alvo',
      chave: s.categoriaDeMaquinaNome,
      resumo: `${numero(s.percentual)}% da demanda`,
      vigencia: s.vigencia,
      estado: estadoDaVigencia(s.vigencia, shares.get(s.categoriaDeMaquinaCodigo), hoje),
      alvo: { tipo: 'share', categoriaDeMaquinaCodigo: s.categoriaDeMaquinaCodigo, vigenteDesde: s.vigencia.vigenteDesde },
    })),
  ];
}
