/**
 * Persistência das preferências da tela Configurações.
 *
 * POR QUE ISTO EXISTE: a tela tinha um rodapé "Cancelar / Salvar alterações"
 * em seis seções, e `salvar()` só mostrava o aviso *"Alterações salvas · em
 * produção grava no Protheus/AD"*. Nada era gravado, e todos os campos eram
 * `defaultValue`/`defaultChecked` — ninguém lia o valor depois. Mudar a meta de
 * frequência da classe A, salvar, trocar de seção e voltar devolvia o número
 * antigo, **sem nenhum aviso de que a alteração tinha sido perdida**. É a
 * gravidade mais alta do documento 06: engana e perde dado.
 *
 * O QUE FOI FEITO: os campos viraram controlados, o rascunho vive na tela,
 * `Salvar alterações` grava aqui, `Cancelar` volta ao último gravado, e o
 * rodapé só habilita os botões quando há alteração pendente. O aviso de sucesso
 * passou a dizer onde ficou gravado, que é o navegador desta máquina — não o
 * Protheus, não o AD.
 *
 * O QUE **NÃO** É GRAVADO, de propósito: o que a tela mostra como leitura de
 * outro sistema (nome, e-mail, cargo e regional vêm do AD; integrações,
 * usuários, roles e auditoria são do servidor). Documento 05 §5: campo que vem
 * de fora não é editável no CRM.
 *
 * Quando a API existir, só este arquivo muda.
 */
import type { ConfigMetas } from '../tipos/configuracoes';

const CHAVE_CONFIG = 'crm-tracbel:config-preferencias:v1';

/** As preferências que esta tela grava de verdade. */
export type PreferenciasConfig = {
  /** Perfil › o único campo que o AD não manda. */
  telefone: string;
  assinaturaEmail: string;
  /** Notificações › canais. */
  notificaNavegador: boolean;
  notificaEmailCritico: boolean;
  notificaResumoDiario: boolean;
  notificaWhatsapp: boolean;
  /** Notificações › regras. */
  alertaOportunidadeParada: string;
  alertaClienteSemContato: string;
  silenciarNotificacoes: string;
  /** Comercial › metas e SLA. */
  metas: ConfigMetas;
  /** Comercial › fluxo automático de aprovação. */
  escalarAposSla: boolean;
  bloquearPropostaAcimaAlcada: boolean;
  exigirJustificativaDesconto: boolean;
  notificarFinanceiroAcimaCinco: boolean;
};

/**
 * Os valores de fábrica das preferências que não vêm de JSON. As metas vêm de
 * `config-metas.json` e por isso entram por parâmetro: o padrão delas é o que o
 * arquivo diz, não um número repetido aqui.
 */
export function preferenciasDeFabrica(metas: ConfigMetas): PreferenciasConfig {
  return {
    telefone: '(66) 99887-4321',
    assinaturaEmail:
      'Hugo Rocha\nGerente de TI · Tracbel Agro\nhugo.rocha@tracbel.com.br · (66) 99887-4321\nJoão Deere Brasil · concessionária MT/GO/BA',
    notificaNavegador: true,
    notificaEmailCritico: true,
    notificaResumoDiario: false,
    notificaWhatsapp: false,
    alertaOportunidadeParada: '14 dias',
    alertaClienteSemContato: '7 dias além da meta',
    silenciarNotificacoes: 'Fim de semana e feriados',
    metas,
    escalarAposSla: true,
    bloquearPropostaAcimaAlcada: true,
    exigirJustificativaDesconto: true,
    notificarFinanceiroAcimaCinco: false,
  };
}

/** Wrapper defensivo: só retorna o Storage se leitura/escrita realmente funcionar. */
function obterStorage(): Storage | null {
  try {
    const s = window.localStorage;
    const chaveTeste = '__crm_test__';
    s.setItem(chaveTeste, '1');
    s.removeItem(chaveTeste);
    return s;
  } catch {
    return null;
  }
}

/** true se o navegador permite gravar (falso em sandbox). */
export function storageConfigDisponivel(): boolean {
  return obterStorage() !== null;
}

/**
 * O que está gravado, completado com os valores de fábrica.
 *
 * A mescla campo a campo é o que faz uma preferência nova não apagar as antigas
 * quando esta tela ganhar mais um campo: o que estiver faltando no gravado vem
 * de fábrica, em vez de o objeto inteiro ser descartado por não bater.
 */
export function lerPreferencias(metas: ConfigMetas): PreferenciasConfig {
  const fabrica = preferenciasDeFabrica(metas);
  const s = obterStorage();
  if (!s) return fabrica;
  try {
    const bruto = s.getItem(CHAVE_CONFIG);
    if (!bruto) return fabrica;
    const gravado = JSON.parse(bruto) as Partial<PreferenciasConfig>;
    return {
      ...fabrica,
      ...gravado,
      metas: { ...fabrica.metas, ...(gravado.metas ?? {}) },
    };
  } catch (erro) {
    console.warn('[Config] falha ao carregar as preferências:', erro);
    return fabrica;
  }
}

/** Grava. Devolve `false` quando o navegador não deixou — e a tela avisa. */
export function gravarPreferencias(preferencias: PreferenciasConfig): boolean {
  const s = obterStorage();
  if (!s) return false;
  try {
    s.setItem(CHAVE_CONFIG, JSON.stringify(preferencias));
    return true;
  } catch (erro) {
    console.warn('[Config] falha ao salvar as preferências:', erro);
    return false;
  }
}
