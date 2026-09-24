/**
 * As regras das vigências que a tela precisa saber — as mesmas do servidor (`ParametroComVigencia`), para a
 * tela não oferecer o que a API vai recusar.
 *
 * - **Hoje é o dia de São Paulo** (UTC−3, sem horário de verão desde 2019): uma vigência registrada às 22h de
 *   30/09 é de 30/09.
 * - **A vigência começa hoje ou depois**, e **só se revoga o que ainda não passou de hoje**.
 * - **Vale numa data a de início mais recente até ela**, ignorando as revogadas.
 */

import type { AlvoDaRevogacao } from '../../../dados/api/potencial';
import type { ItemDeSelecao } from '../../../tipos/api';
import type {
  HistoricoDosParametrosDoPotencial,
  NovoParametroDoPotencial,
  ParametrosGeraisDetalhe,
  VigenciaDoParametro,
} from '../../../tipos/potencial';

/** O dia de hoje em São Paulo, `aaaa-mm-dd`. */
export function hojeEmSaoPaulo(agora: Date = new Date()): string {
  return new Date(agora.getTime() - 3 * 60 * 60 * 1000).toISOString().slice(0, 10);
}

/** `aaaa-mm-dd` → `dd/mm/aaaa`. */
export function dataCurta(aaaammdd: string): string {
  const [ano, mes, dia] = aaaammdd.split('-');
  return dia && mes && ano ? `${dia}/${mes}/${ano}` : aaaammdd;
}

/** Número no formato do Brasil, com as casas pedidas; travessão quando não há valor. */
export function numero(valor: number | null | undefined, casas = 2): string {
  if (valor === null || valor === undefined) return '—';
  return valor.toLocaleString('pt-BR', { minimumFractionDigits: 0, maximumFractionDigits: casas });
}

/** Uma lista de escolha que não vem de catálogo (produtos, municípios, situações), na ordem recebida. */
export function itensDeSelecao(pares: readonly { codigo: string; descricao: string }[]): ItemDeSelecao[] {
  return pares.map((par, ordem) => ({ ...par, ordem, exigeObservacao: false }));
}

/** O valor como o campo do formulário o mostra: vírgula decimal, vazio quando nulo. */
function campo(valor: number | null | undefined): string {
  return valor === null || valor === undefined ? '' : String(valor).replace('.', ',');
}

export type EstadoDaVigencia = 'vigente' | 'futura' | 'substituida' | 'revogada';

export const ROTULO_DO_ESTADO: Record<EstadoDaVigencia, { rotulo: string; cor: string }> = {
  vigente: { rotulo: 'Vigente', cor: '#22C55E' },
  futura: { rotulo: 'Futura', cor: '#3B82F6' },
  substituida: { rotulo: 'Substituída', cor: '#94A3B8' },
  revogada: { rotulo: 'Revogada', cor: '#EF4444' },
};

/**
 * O estado de uma vigência hoje.
 *
 * @param vigencia A vigência.
 * @param inicioDaVigenteNaChave O início da que vale hoje para a mesma chave (produto, município ou geral).
 * @param hoje O dia de hoje, `aaaa-mm-dd`.
 */
export function estadoDaVigencia(
  vigencia: VigenciaDoParametro,
  inicioDaVigenteNaChave: string | undefined,
  hoje: string,
): EstadoDaVigencia {
  if (vigencia.revogadoEm) return 'revogada';
  if (vigencia.vigenteDesde > hoje) return 'futura';
  return vigencia.vigenteDesde === inicioDaVigenteNaChave ? 'vigente' : 'substituida';
}

/** Pode ser revogada: não foi revogada e ainda não passou de hoje. */
export function ehRevogavel(vigencia: VigenciaDoParametro, hoje: string): boolean {
  return !vigencia.revogadoEm && vigencia.vigenteDesde >= hoje;
}

/** Para cada chave, o início da vigência que vale na data. */
export function iniciosVigentes<T extends { vigencia: VigenciaDoParametro }>(
  itens: readonly T[],
  chave: (item: T) => string,
  data: string,
): Map<string, string> {
  const inicios = new Map<string, string>();
  for (const item of itens) {
    const { revogadoEm, vigenteDesde } = item.vigencia;
    if (revogadoEm || vigenteDesde > data) continue;
    const atual = inicios.get(chave(item));
    if (!atual || vigenteDesde > atual) inicios.set(chave(item), vigenteDesde);
  }
  return inicios;
}

/**
 * O que identifica uma regra: o produto E a categoria de máquina (D-P01).
 *
 * A vigência anterior ao catálogo não tem categoria, e cai numa chave própria — ela não disputa lugar com
 * nenhuma das novas.
 */
export function chaveDaRegra(regra: { produtoCodigoIbge: number; categoriaDeMaquinaCodigo: string | null }): string {
  return `${regra.produtoCodigoIbge}|${regra.categoriaDeMaquinaCodigo ?? 'sem-categoria'}`;
}

/** Uma linha da trilha: qualquer uma das três vigências, com o que a identifica para revogar. */
export type LinhaDoHistorico = {
  id: string;
  tipo: 'Parâmetros gerais' | 'Regra da cultura' | 'Percepção do gestor';
  chave: string;
  resumo: string;
  vigencia: VigenciaDoParametro;
  estado: EstadoDaVigencia;
  alvo: AlvoDaRevogacao;
};

/**
 * A trilha inteira numa lista só, das mais novas para as mais antigas — como o administrador pergunta: "o
 * que mudou, quando e quem mudou?".
 */
export function montarHistorico(historico: HistoricoDosParametrosDoPotencial, hoje: string): LinhaDoHistorico[] {
  const gerais = iniciosVigentes(historico.gerais, () => 'geral', hoje);
  // A CHAVE DA REGRA É PRODUTO **E CATEGORIA** (D-P01). Só o produto fazia o trator e a colheitadeira do
  // café disputarem a mesma chave: uma das duas seria marcada como "substituída" pela outra, que é outra
  // decisão e não a substitui em nada.
  const culturas = iniciosVigentes(historico.culturas, chaveDaRegra, hoje);
  const percepcoes = iniciosVigentes(historico.percepcoes, (p) => String(p.municipioCodigoIbge), hoje);

  const linhas: LinhaDoHistorico[] = [
    ...historico.gerais.map((g): LinhaDoHistorico => ({
      id: `geral-${g.vigencia.vigenteDesde}-${g.vigencia.informadoEm}`,
      tipo: 'Parâmetros gerais',
      chave: '—',
      resumo:
        `Janela ${g.mesesDaJanela} × ${g.mesesDaJanela} meses · crédito ${numero(g.pesoDosContratosNoCredito * 100, 0)}% contratos · ` +
        `faixas ${numero(g.limiteDeRetracao)} / ${numero(g.limiteDeAquecimento)} / ${numero(g.limiteDeSuperaquecimento)} · ` +
        `percepção ±${numero(g.limiteDaPercepcao)}%`,
      vigencia: g.vigencia,
      estado: estadoDaVigencia(g.vigencia, gerais.get('geral'), hoje),
      alvo: { tipo: 'geral', vigenteDesde: g.vigencia.vigenteDesde },
    })),
    ...historico.culturas.map((r): LinhaDoHistorico => ({
      id: `cultura-${chaveDaRegra(r)}-${r.vigencia.vigenteDesde}-${r.vigencia.informadoEm}`,
      tipo: 'Regra da cultura',
      // A CATEGORIA VAI NA CHAVE VISÍVEL: sem ela, duas linhas escritas "Café" e nada que as distinga.
      chave: r.categoriaDeMaquinaNome ? `${r.produtoNome} · ${r.categoriaDeMaquinaNome}` : r.produtoNome,
      resumo:
        `1 ${r.modeloDeReferencia} a cada ${numero(r.hectaresPorMaquina)} ha · ` +
        (r.anosDeRenovacao === null ? 'renovação não informada' : `renovação a cada ${numero(r.anosDeRenovacao, 1)} anos`) +
        (r.situacao === 'AConfirmar' ? ' · a confirmar' : ''),
      vigencia: r.vigencia,
      estado: estadoDaVigencia(r.vigencia, culturas.get(chaveDaRegra(r)), hoje),
      alvo: {
        tipo: 'cultura',
        produtoCodigoIbge: r.produtoCodigoIbge,
        categoriaDeMaquinaCodigo: r.categoriaDeMaquinaCodigo ?? '',
        vigenteDesde: r.vigencia.vigenteDesde,
      },
    })),
    ...historico.percepcoes.map((p): LinhaDoHistorico => ({
      id: `percepcao-${p.municipioCodigoIbge}-${p.vigencia.vigenteDesde}-${p.vigencia.informadoEm}`,
      tipo: 'Percepção do gestor',
      chave: `${p.municipioNome}/${p.uf}`,
      resumo: `${p.percentual > 0 ? '+' : ''}${numero(p.percentual)}%`,
      vigencia: p.vigencia,
      estado: estadoDaVigencia(p.vigencia, percepcoes.get(String(p.municipioCodigoIbge)), hoje),
      alvo: { tipo: 'percepcao', municipioCodigoIbge: p.municipioCodigoIbge, vigenteDesde: p.vigencia.vigenteDesde },
    })),
  ];

  return linhas.sort((a, b) =>
    b.vigencia.informadoEm.localeCompare(a.vigencia.informadoEm) || b.vigencia.vigenteDesde.localeCompare(a.vigencia.vigenteDesde),
  );
}

/**
 * O formulário dos parâmetros gerais já preenchido com a vigência de hoje — mudar um valor é registrar o
 * conjunto de novo com aquele valor trocado, e ninguém deveria redigitar os outros onze.
 */
export function formularioDosGerais(vigente: ParametrosGeraisDetalhe | null, hoje: string): NovoParametroDoPotencial {
  return {
    vigenteDesde: hoje,
    mesesDaJanela: campo(vigente?.mesesDaJanela),
    pesoDosContratosNoCredito: campo(vigente?.pesoDosContratosNoCredito),
    limiteDeRetracao: campo(vigente?.limiteDeRetracao),
    limiteDeAquecimento: campo(vigente?.limiteDeAquecimento),
    limiteDeSuperaquecimento: campo(vigente?.limiteDeSuperaquecimento),
    nomeDaFaixaIntermediaria: vigente?.nomeDaFaixaIntermediaria ?? '',
    limiteDaPercepcao: campo(vigente?.limiteDaPercepcao),
    pesoDoIndicadorDePreco: campo(vigente?.pesoDoIndicadorDePreco),
    pesoDoIndicadorDeCredito: campo(vigente?.pesoDoIndicadorDeCredito),
    pesoDoIndicadorComercial: campo(vigente?.pesoDoIndicadorComercial),
    fatorMinimo: campo(vigente?.fatorMinimo),
    fatorMaximo: campo(vigente?.fatorMaximo),
    mesesDeCarenciaDoSicor: campo(vigente?.mesesDeCarenciaDoSicor),
    justificativa: '',
  };
}
