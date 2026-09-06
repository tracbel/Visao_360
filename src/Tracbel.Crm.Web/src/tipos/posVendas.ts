/**
 * Tipos do JSON `pos-vendas-cliente-84391.json` (POSVENDAS_CLIENTE_84391,
 * prototipo/referencia/assets/app.js:8095-8330) — dados da aba Pós-Vendas da
 * ficha do cliente.
 */

export interface PosVendasFaturamento {
  fytd_pecas: number;
  fytd_servicos: number;
  fytd_total: number;
  ytd_2026_pecas: number;
  ytd_2026_servicos: number;
  ytd_2026_total: number;
  ano_2025_total: number;
  ano_2024_total: number;
  ticket_medio_pecas: number;
  mix_canal: {
    balcao: number;
    oficina_interna: number;
    campo_tecnico: number;
  };
}

export interface PosVendasFaturamentoMes {
  mes: string;
  pecas: number;
  servicos: number;
}

export type CorStatusPv = 'red' | 'amber' | 'green' | 'blue' | 'gray' | 'info';

export interface PosVendasOsAberta {
  id: string;
  chassi: string;
  modelo: string;
  tipo: string;
  abertura: string;
  previsao: string;
  status: string;
  status_cor: CorStatusPv;
  valor: number;
  tecnico: string;
  diagnostico: string;
  dias_aberta: number;
  sla_dias: number;
}

export interface PosVendasPmp {
  id: string;
  codigo_jd: string;
  titulo: string;
  criticidade: string;
  criticidade_cor: CorStatusPv;
  chassi_afetado: string[];
  modelo: string;
  qtd_equipamentos: number;
  publicacao: string;
  prazo: string;
  dias_restantes: number;
  cobertura: string;
  duracao_estimada: string;
  descricao: string;
  status: string;
  status_cor: CorStatusPv;
  agenda?: string;
}

export interface PosVendasAlertaCritico {
  id: string;
  tipo: string;
  cor: 'red' | 'amber';
  titulo: string;
  detalhe: string;
  origem: string;
  criado_em: string;
  acao_cta: string;
  acao_link: string;
}

export interface PosVendasContratoJdcp {
  chassi: string;
  modelo: string;
  plano: string;
  inicio: string;
  fim: string;
  horas_cobertas: string;
  valor_anual: number;
}

export interface PosVendasFrotaSemJdcp {
  chassi: string;
  modelo: string;
  ano: number;
  potencial: number;
}

export interface PosVendasNps {
  score: number;
  respostas: number;
  detratores: number;
  neutros: number;
  promotores: number;
  comentario_recente: {
    data: string;
    autor: string;
    nota: number;
    texto: string;
  };
}

export interface PosVendasCliente {
  fat_pv: PosVendasFaturamento;
  fat_12m: PosVendasFaturamentoMes[];
  os_abertas: PosVendasOsAberta[];
  pmp_pendentes: PosVendasPmp[];
  alertas_criticos: PosVendasAlertaCritico[];
  contratos_jdcp: PosVendasContratoJdcp[];
  frota_sem_jdcp: PosVendasFrotaSemJdcp[];
  nps: PosVendasNps;
}
