/**
 * As integrações configuráveis (issue 136) — `/api/v1/admin/integracoes`.
 *
 * Ler é `Integracao.Ler` (Gerência para cima); configurar, gravar a credencial, testar, reagendar e "rodar agora" é
 * `Integracao.Administrar` (o Administrador). A senha vai por uma chamada própria e NUNCA volta: a tela só sabe se há
 * uma, de onde ela vem e quem a gravou.
 */

import type { ComProcedencia } from '../../tipos/api';
import { ler, pedir, type ContextoDeAcesso } from './http';

const BASE = '/v1/admin/integracoes';

export type TipoDeConexao = 'ApiRest' | 'SqlServer' | 'MySql' | 'FontePublica' | 'Monitorada';
export type OrigemDaCredencial = 'NaoSeAplica' | 'Tela' | 'Ambiente' | 'Nenhuma';
export type CadenciaDaRotina = 'Anual' | 'Mensal' | 'Diaria' | 'Intervalo';

export type ConexaoNaTela = {
  codigo: string;
  nome: string;
  descricao: string | null;
  tipo: TipoDeConexao;
  ehDoSistema: boolean;
  estaAtiva: boolean;
  endereco: string | null;
  porta: number | null;
  banco: string | null;
  objeto: string | null;
  usuario: string | null;
  nomeDoCabecalho: string | null;
  statusEsperado: number;
  minutosEntreVerificacoes: number | null;
  enderecoEditavel: boolean;
  aceitaSegredo: boolean;
  origemDaCredencial: OrigemDaCredencial;
  temSegredoNaTela: boolean;
  segredoAlteradoEm: string | null;
  segredoAlteradoPor: string | null;
  ultimaVerificacaoEm: string | null;
  ultimaVerificacaoOk: boolean | null;
  ultimaVerificacaoResumo: string | null;
  rotinas: string[];
};

export type RotinaNaTela = {
  codigo: string;
  nome: string;
  descricao: string;
  cargas: string[];
  cadencia: CadenciaDaRotina;
  mes: number | null;
  dia: number | null;
  hora: string | null;
  intervaloMinutos: number | null;
  agenda: string;
  estaLigada: boolean;
  proximaExecucaoEm: string | null;
  naFila: boolean;
  execucaoPedidaEm: string | null;
  execucaoPedidaPor: string | null;
  ultimaExecucaoIniciadaEm: string | null;
  ultimaExecucaoTerminadaEm: string | null;
  ultimoResultado: string | null;
  ultimaMensagem: string | null;
  conexoes: string[];
  pendencia: string | null;
  /** O primeiro ano da série histórica, nas rotinas que têm uma (issue 156). */
  anoInicialDoHistorico: number | null;
  /** Se esta rotina busca série histórica — só então o campo aparece. */
  aceitaAnoInicialDoHistorico: boolean;
};

export type PainelDeIntegracoes = { conexoes: ConexaoNaTela[]; rotinas: RotinaNaTela[]; podeAdministrar: boolean };

export type VerificacaoNaTela = { verificadaEm: string; verificadaPor: string | null; ok: boolean; resumo: string; latenciaMs: number };

export type ExecucaoNaTela = {
  iniciadaEm: string;
  terminadaEm: string | null;
  motivo: 'Agenda' | 'Pedido' | 'PrimeiraCarga';
  pedidaPor: string | null;
  resultado: string;
  codigoDeSaida: number | null;
  mensagem: string | null;
  maquina: string;
};

export type ResultadoDoTeste = { ok: boolean; resumo: string; latenciaMs: number; testadoEm: string; conexao: ConexaoNaTela };

export type ConfiguracaoDeConexao = {
  endereco?: string | null;
  porta?: number | null;
  banco?: string | null;
  objeto?: string | null;
  usuario?: string | null;
  nomeDoCabecalho?: string | null;
  nome?: string | null;
  descricao?: string | null;
  statusEsperado?: number | null;
  minutosEntreVerificacoes?: number | null;
};

export type NovaApiMonitorada = {
  codigo: string;
  nome: string;
  descricao: string;
  endereco: string;
  nomeDoCabecalho: string;
  statusEsperado: number;
  minutosEntreVerificacoes: number | null;
};

export type AgendaNaTela = { cadencia: CadenciaDaRotina; mes: number | null; dia: number | null; hora: string | null; intervaloMinutos: number | null; ligada: boolean; anoInicialDoHistorico: number | null };

const conexao = (codigo: string) => `${BASE}/conexoes/${encodeURIComponent(codigo)}`;
const rotina = (codigo: string) => `${BASE}/rotinas/${encodeURIComponent(codigo)}`;

export function obterPainelDeIntegracoes(contexto: ContextoDeAcesso, sinal?: AbortSignal): Promise<ComProcedencia<PainelDeIntegracoes>> {
  return ler<PainelDeIntegracoes>(BASE, contexto, { sinal });
}

export function listarVerificacoes(contexto: ContextoDeAcesso, codigo: string, sinal?: AbortSignal): Promise<ComProcedencia<VerificacaoNaTela[]>> {
  return ler<VerificacaoNaTela[]>(`${conexao(codigo)}/verificacoes`, contexto, { sinal });
}

export function listarExecucoes(contexto: ContextoDeAcesso, codigo: string, sinal?: AbortSignal): Promise<ComProcedencia<ExecucaoNaTela[]>> {
  return ler<ExecucaoNaTela[]>(`${rotina(codigo)}/execucoes`, contexto, { sinal });
}

export function configurarConexao(contexto: ContextoDeAcesso, codigo: string, corpo: ConfiguracaoDeConexao) {
  return pedir<ConexaoNaTela>(conexao(codigo), contexto, { metodo: 'PUT', corpo });
}

/** A senha vai sozinha, numa chamada própria. A resposta é a conexão — sem ela. */
export function definirSegredo(contexto: ContextoDeAcesso, codigo: string, segredo: string) {
  return pedir<ConexaoNaTela>(`${conexao(codigo)}/segredo`, contexto, { metodo: 'PUT', corpo: { segredo } });
}

export function removerSegredo(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<ConexaoNaTela>(`${conexao(codigo)}/segredo/remocao`, contexto, { metodo: 'POST' });
}

export function testarConexao(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<ResultadoDoTeste>(`${conexao(codigo)}/teste`, contexto, { metodo: 'POST' });
}

export function criarApiMonitorada(contexto: ContextoDeAcesso, corpo: NovaApiMonitorada) {
  return pedir<ConexaoNaTela>(`${BASE}/conexoes`, contexto, { metodo: 'POST', corpo });
}

export function desativarConexao(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<ConexaoNaTela>(`${conexao(codigo)}/desativacao`, contexto, { metodo: 'POST' });
}

export function reativarConexao(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<ConexaoNaTela>(`${conexao(codigo)}/reativacao`, contexto, { metodo: 'POST' });
}

export function reagendarRotina(contexto: ContextoDeAcesso, codigo: string, corpo: AgendaNaTela) {
  return pedir<RotinaNaTela>(rotina(codigo), contexto, { metodo: 'PUT', corpo });
}

export function pedirExecucao(contexto: ContextoDeAcesso, codigo: string) {
  return pedir<RotinaNaTela>(`${rotina(codigo)}/execucao`, contexto, { metodo: 'POST' });
}
