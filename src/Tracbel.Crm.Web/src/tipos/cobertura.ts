/**
 * Tipos compartilhados pelas telas de Cobertura de Carteira (`#/cobertura`) e
 * Cobertura Regional (`#/relatorios/cobertura`) — refletem as estruturas do
 * protótipo de referência (`prototipo/referencia/assets/app.js`):
 * `CARTEIRA_CEN` (linha 3389) e `COBERTURA_DATA` (linha 693).
 */

export type Classe = 'A' | 'B' | 'C' | 'D';

export type CategoriaInteracao = 'visita' | 'ligacao' | 'whatsapp' | 'email' | 'remota';

export type StatusCobertura = 'em_dia' | 'aviso' | 'atraso' | 'critico' | 'nunca';

export type UltimaInteracao = {
  data: string;
  cat: CategoriaInteracao;
  autor: string;
};

export type ClienteCarteira = {
  id: number;
  razao: string;
  apelido: string;
  classe: Classe;
  cidade: string;
  uf: string;
  lat: number;
  lng: number;
  exige_visita: boolean;
  ult_int: UltimaInteracao | null;
  fat_12m: number;
  oportunidades: number;
  oportunidades_valor?: number;
  obs: string;
};

export type StatusLabelInfo = { label: string; cor: string; bg: string };

export type CategoriaInteracaoInfo = { label: string; cor: string; icone: string };

export type CidadeLatLng = { lat: number; lng: number };

export type RegionalCobertura = {
  key: string;
  label: string;
  clientesAB: number;
  tocados: number;
  cobertura: number;
  vendas_perdidas: number;
  gpe_perdidas: number;
};

export type MotivoPerda = { motivo: string; qtd: number };

export type CoberturaData = {
  meta: number;
  geral: {
    cobertura: number;
    totalClientesAB: number;
    tocados120d: number;
    naoTocados: number;
  };
  regionais: RegionalCobertura[];
  vendas_perdidas: {
    total_mes_anterior: number;
    valor_perdido: number;
    percentual_gpe: number;
    breakdown_motivo: MotivoPerda[];
  };
};
