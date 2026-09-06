/**
 * Regras do painel 360 do cliente (perfil CEN da Visão 360).
 *
 * Três coisas moram aqui, todas puras — sem React, sem DOM:
 *
 * 1. **Prazo de tarefa.** Achado da pesquisa 16 §4.4: das 170 ações que geraram
 *    agenda no Vórtice em 2026, *nenhuma* tem `PrazoRealizacao` preenchido. Não
 *    existe "atrasada" definida por política, só em relação à data que alguém
 *    digitou — e é o que produz o passivo de 35.662 tarefas pendentes, 44% delas
 *    em conta de usuário morto (§4.5). No modelo unificado (doc 17) isso vira
 *    `processo.TipoTarefa.PrazoDias`, obrigatório. Enquanto o banco não está
 *    ligado à tela, a política mora na tabela `PRAZO_DIAS_POR_TIPO`.
 *
 * 2. **Situação do dado lido do Protheus.** Doc 05 §5: campo que vem do Protheus
 *    não é editável no CRM, mostra de onde veio e quando foi atualizado, e
 *    quando a carga está velha a tela diz. É a lição direta dos 17 meses de
 *    faturamento congelado sem ninguém notar.
 *
 * 3. **Quem pede ação hoje.** Classificação do cliente da carteira por dias sem
 *    contato contra a meta de frequência da classe — mesma regra de
 *    `statusCobertura()` já portada em `CoberturaCarteira.tsx` e `Visao360.tsx`.
 */
import type { TipoTarefa } from '../tipos/agenda';
import type { ClienteCarteira, MetaFrequencia, StatusCobertura } from '../tipos/visao360';

const MS_DIA = 1000 * 60 * 60 * 24;

/** Zera a hora para comparar dias corridos, não instantes. */
function meiaNoite(d: Date): Date {
  const c = new Date(d);
  c.setHours(0, 0, 0, 0);
  return c;
}

export function diasEntre(de: Date | string, ate: Date | string): number {
  const a = meiaNoite(typeof de === 'string' ? new Date(de) : de);
  const b = meiaNoite(typeof ate === 'string' ? new Date(ate) : ate);
  return Math.round((b.getTime() - a.getTime()) / MS_DIA);
}

/* ---------------------------------------------------------------------- */
/* 1. Prazo                                                                */
/* ---------------------------------------------------------------------- */

/**
 * Política de prazo por tipo de tarefa, em dias corridos contados da data
 * agendada. No CRM real isto é `processo.TipoTarefa.PrazoDias` — um catálogo,
 * não uma constante de código. Fica aqui só enquanto a tela lê JSON.
 */
export const PRAZO_DIAS_POR_TIPO: Record<TipoTarefa, number> = {
  ligar: 1,
  proposta: 2,
  visitar: 3,
  monitorar: 5,
};

/** Prazo da aprovação, contado da data da solicitação (política de alçada). */
export const PRAZO_DIAS_APROVACAO = 2;

export type SituacaoPrazo = 'vencido' | 'hoje' | 'no_prazo';

export type Prazo = {
  /** Dia em que a tarefa deveria estar concluída. */
  vence: Date;
  /** Dias até vencer. Negativo = vencida há tantos dias. */
  dias: number;
  situacao: SituacaoPrazo;
  /** Texto pronto para a tela: "vencida há 12 dias", "vence hoje", "faltam 3 dias". */
  rotulo: string;
};

export function calcularPrazo(agendadaEm: string, prazoDias: number, hoje: Date, feminino = true): Prazo {
  const vence = meiaNoite(new Date(agendadaEm));
  vence.setDate(vence.getDate() + prazoDias);
  const dias = diasEntre(hoje, vence);
  const vencida = feminino ? 'vencida' : 'vencido';
  if (dias < 0) {
    const n = Math.abs(dias);
    return { vence, dias, situacao: 'vencido', rotulo: `${vencida} há ${n} ${n === 1 ? 'dia' : 'dias'}` };
  }
  if (dias === 0) return { vence, dias, situacao: 'hoje', rotulo: 'vence hoje' };
  return { vence, dias, situacao: 'no_prazo', rotulo: `${dias === 1 ? 'falta' : 'faltam'} ${dias} ${dias === 1 ? 'dia' : 'dias'}` };
}

/* ---------------------------------------------------------------------- */
/* 2. Dado que vem do Protheus                                             */
/* ---------------------------------------------------------------------- */

/** A partir de quantos dias sem carga o bloco do Protheus aparece marcado como velho. */
export const DIAS_ATE_DADO_VELHO = 7;

export type SituacaoFonte = 'sem_leitura' | 'atual' | 'velho';

export type CarimboFonte = {
  sistema: string;
  atualizadoEm: Date | null;
  situacao: SituacaoFonte;
  /** Dias desde a última carga. `null` quando nunca houve leitura. */
  diasDesde: number | null;
};

/**
 * Lê o carimbo de sincronismo do formato gravado no cadastro do protótipo —
 * `"Protheus-Sync · 22/08/2026 14:31"`. No CRM real o carimbo vem de
 * `integracao.Carga.ConcluidaEm`, não de um texto de auditoria.
 */
export function lerCarimboProtheus(origemAlteracao: string | null | undefined, hoje: Date): CarimboFonte {
  const sistema = 'Protheus';
  if (!origemAlteracao) return { sistema, atualizadoEm: null, situacao: 'sem_leitura', diasDesde: null };

  const m = origemAlteracao.match(/(\d{2})\/(\d{2})\/(\d{4})(?:\s+(\d{2}):(\d{2}))?/);
  if (!m) return { sistema, atualizadoEm: null, situacao: 'sem_leitura', diasDesde: null };

  const atualizadoEm = new Date(
    Number(m[3]),
    Number(m[2]) - 1,
    Number(m[1]),
    Number(m[4] ?? '0'),
    Number(m[5] ?? '0'),
  );
  const dias = diasEntre(atualizadoEm, hoje);
  return {
    sistema,
    atualizadoEm,
    situacao: dias > DIAS_ATE_DADO_VELHO ? 'velho' : 'atual',
    diasDesde: dias,
  };
}

/* ---------------------------------------------------------------------- */
/* 3. Cliente que pede ação                                                */
/* ---------------------------------------------------------------------- */

/**
 * Mesma regra de `statusCobertura()` (app.js:3431), já portada em
 * `CoberturaCarteira.tsx` e `Visao360.tsx`. Repetida aqui pelo mesmo motivo das
 * outras duas: cada tela mantém seus helpers puros.
 */
export function classificarCobertura(cliente: ClienteCarteira, hoje: Date, metaFreq: MetaFrequencia): StatusCobertura {
  if (!cliente.ult_int) return 'nunca';
  const dias = diasEntre(cliente.ult_int.data, hoje);
  const meta = metaFreq[cliente.classe];
  const zerou = cliente.exige_visita ? cliente.ult_int.cat === 'visita' : true;
  const diasEfetivos = zerou ? dias : Math.max(dias, meta + 1);
  const pct = diasEfetivos / meta;
  if (pct <= 0.6) return 'em_dia';
  if (pct <= 1.0) return 'aviso';
  if (pct <= 1.5) return 'atraso';
  return 'critico';
}

export const ROTULO_INTERACAO: Record<string, string> = {
  visita: 'Visita',
  ligacao: 'Ligação',
  email: 'E-mail',
  whatsapp: 'WhatsApp',
  remota: 'Atendimento remoto',
};

/**
 * Explica a cadência vencida em uma frase.
 *
 * A sutileza que confunde na tela: um cliente que **exige visita** não zera a
 * cadência com uma ligação. Sem essa frase, o painel dizia "cadência vencida ·
 * 4 dias sem contato", que parece erro de conta e é regra de negócio.
 */
export function textoCadencia(
  cliente: ClienteCarteira,
  diasSemContato: number | null,
  metaFreq: MetaFrequencia,
): string {
  const meta = metaFreq[cliente.classe];
  if (diasSemContato === null || !cliente.ult_int) {
    return `Nenhuma interação registrada · a meta da classe ${cliente.classe} é de ${meta} dias.`;
  }
  const naoZerou = cliente.exige_visita && cliente.ult_int.cat !== 'visita';
  if (naoZerou) {
    const cat = (ROTULO_INTERACAO[cliente.ult_int.cat] ?? cliente.ult_int.cat).toLowerCase();
    return `O último contato foi há ${diasSemContato} dias, mas por ${cat}: cliente que exige visita só zera a cadência de ${meta} dias com visita.`;
  }
  return `${diasSemContato} dias sem contato para uma meta de ${meta} dias.`;
}

export type Gravidade = 'critico' | 'aviso' | 'info';

export type MotivoAcao = {
  chave: string;
  texto: string;
  gravidade: Gravidade;
};

export type ClienteComAcao = {
  cliente: ClienteCarteira;
  status: StatusCobertura;
  diasSemContato: number | null;
  motivos: MotivoAcao[];
  gravidade: Gravidade;
};

const PESO_GRAVIDADE: Record<Gravidade, number> = { critico: 0, aviso: 1, info: 2 };

/** Dias sem visita a partir dos quais um cliente classe A entra na lista (doc 05 §1). */
export const DIAS_SEM_VISITA_CLASSE_A = 90;

/**
 * Monta a lista "clientes que pedem ação" da camada 1.
 *
 * `motivosExtras` traz o que não sai da carteira — revisão programada próxima e
 * ordem de serviço atrasada vêm do pós-vendas; título vencido viria do Protheus.
 */
export function clientesQuePedemAcao(
  carteira: ClienteCarteira[],
  hoje: Date,
  metaFreq: MetaFrequencia,
  motivosExtras: Record<number, MotivoAcao[]> = {},
): ClienteComAcao[] {
  const linhas: ClienteComAcao[] = [];

  for (const cliente of carteira) {
    const status = classificarCobertura(cliente, hoje, metaFreq);
    const diasSemContato = cliente.ult_int ? diasEntre(cliente.ult_int.data, hoje) : null;
    const motivos: MotivoAcao[] = [];

    if (cliente.classe === 'A' && (diasSemContato === null || diasSemContato > DIAS_SEM_VISITA_CLASSE_A)) {
      motivos.push({
        chave: 'sem_visita_90d',
        gravidade: 'critico',
        texto:
          diasSemContato === null
            ? 'Classe A sem nenhuma interação registrada'
            : `Classe A sem contato há ${diasSemContato} dias`,
      });
    } else if (status === 'critico' || status === 'atraso') {
      motivos.push({
        chave: 'cobertura_vencida',
        gravidade: status === 'critico' ? 'critico' : 'aviso',
        texto: `Cadência de classe ${cliente.classe} vencida · ${textoCadencia(cliente, diasSemContato, metaFreq)}`,
      });
    }

    if (cliente.oportunidades > 0 && diasSemContato !== null && diasSemContato > 30) {
      motivos.push({
        chave: 'oportunidade_parada',
        gravidade: 'aviso',
        texto: `${cliente.oportunidades} oportunidade${cliente.oportunidades > 1 ? 's' : ''} aberta${cliente.oportunidades > 1 ? 's' : ''} sem contato há ${diasSemContato} dias`,
      });
    }

    motivos.push(...(motivosExtras[cliente.id] ?? []));

    if (motivos.length === 0) continue;

    const gravidade: Gravidade = motivos.some((m) => m.gravidade === 'critico')
      ? 'critico'
      : motivos.some((m) => m.gravidade === 'aviso')
        ? 'aviso'
        : 'info';

    linhas.push({ cliente, status, diasSemContato, motivos, gravidade });
  }

  return linhas.sort((a, b) => {
    const g = PESO_GRAVIDADE[a.gravidade] - PESO_GRAVIDADE[b.gravidade];
    if (g !== 0) return g;
    return (b.diasSemContato ?? 9999) - (a.diasSemContato ?? 9999);
  });
}

/* ---------------------------------------------------------------------- */
/* Formatação compartilhada pelos blocos                                   */
/* ---------------------------------------------------------------------- */

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const DIAS_SEMANA = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

export function formatarData(iso: string | Date): string {
  const d = typeof iso === 'string' ? new Date(iso) : iso;
  return `${String(d.getDate()).padStart(2, '0')}/${MESES[d.getMonth()]}/${String(d.getFullYear()).slice(2)}`;
}

export function formatarDataHora(iso: string | Date): string {
  const d = typeof iso === 'string' ? new Date(iso) : iso;
  return `${DIAS_SEMANA[d.getDay()]} · ${formatarData(d)} · ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}
