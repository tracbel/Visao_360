/**
 * Tipos da Ficha do Equipamento — espelham `EQUIPAMENTO_DATA`
 * (`prototipo/referencia/assets/app.js`, linha 2212) e o JSON extraído em
 * `public/dados/equipamento-1RW7250PVMR123456.json`.
 */

export type EquipamentoCliente = {
  id: number;
  razao: string;
  classe: string;
};

export type EquipamentoAquisicao = {
  tipo: string;
  data_venda: string;
  data_entrega: string;
  vendedor: string;
  valor_faturado: number;
  valor_lista: number;
  desconto: number;
  pedido_protheus: string;
  nota_fiscal: string;
  concessionaria: string;
};

export type EquipamentoGarantia = {
  tipo: string;
  inicio: string;
  fim: string;
  horas_limite: number;
  horas_atual: number;
  percentual_uso: number;
  cobertura: string;
  exclusoes: string;
  contrato: string;
};

export type EquipamentoHorasOperacao = {
  atual: number;
  media_diaria_30d: number;
  media_diaria_90d: number;
  ultima_leitura: string;
  fonte: string;
  proxima_revisao_horas: number;
  horas_para_proxima_revisao: number;
  proxima_revisao_previsao: string;
};

export type EquipamentoHistoricoHoras = {
  data: string;
  horas: number;
};

export type EquipamentoRevisao = {
  id: number;
  data: string;
  horas: number;
  tipo: string;
  tecnico: string;
  duracao_h: number;
  custo: number;
  os: string;
  status: string;
  obs: string;
};

export type EquipamentoChamado = {
  id: number;
  tipo: string;
  prioridade: string;
  abertura: string;
  previsao: string;
  tecnico_agendado: string;
  descricao: string;
};

export type EquipamentoPeca = {
  data: string;
  codigo: string;
  descricao: string;
  qtd: number;
  valor: number;
  os: string;
};

export type EquipamentoTelemetria = {
  ultimo_sync: string;
  consumo_medio_lh: number;
  consumo_ideal_lh: number;
  eficiencia: number;
  horas_ociosas_pct: number;
  velocidade_media_trabalho: number;
  autotrac_uso_pct: number;
  alertas_ativos: number;
  codigos_falha_30d: number;
  codigos_falha_historico: number;
};

export type Equipamento = {
  chassi: string;
  numero_serie: string;
  modelo: string;
  linha: string;
  marca: string;
  potencia: string;
  potencia_faixa: string;
  ano_modelo: number;
  ano_fabricacao: number;
  cor: string;
  transmissao: string;
  cabine: string;
  tracao: string;
  pneus_diant: string;
  pneus_tras: string;
  peso_operacional: string;
  tanque_diesel: string;
  status: string;
  cliente: EquipamentoCliente;
  localizacao: string;
  fazenda_id: string;
  operador_principal: string;
  aquisicao: EquipamentoAquisicao;
  garantia: EquipamentoGarantia;
  horas_operacao: EquipamentoHorasOperacao;
  historico_horas: EquipamentoHistoricoHoras[];
  revisoes: EquipamentoRevisao[];
  chamados_abertos: EquipamentoChamado[];
  pecas: EquipamentoPeca[];
  telemetria: EquipamentoTelemetria;
};
