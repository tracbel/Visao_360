/**
 * As operações de equipamento contra o nosso banco (documento 23, seção 2.2).
 *
 * O QUE A API AINDA NÃO FILTRA, e a tela precisa saber: `termo` compara o CHASSI
 * POR VALOR INTEIRO — 17 caracteres válidos — e número de série e placa por
 * trecho. Buscar pelos últimos dígitos do chassi não devolve nada, e não é
 * defeito da tela: é a dívida D-2 do documento 23 (`CpfCnpj` e `Chassi` são
 * tipos de valor com conversor, e o EF aplica o conversor ao parâmetro do LIKE).
 * A tela diz isso no estado vazio em vez de deixar o usuário achar que a máquina
 * não existe.
 *
 * A API TAMBÉM NÃO FILTRA POR MODELO. O filtro de marca, família e modelo é
 * aplicado na tela, sobre as linhas lidas. A CLASSIFICAÇÃO E O PORTE, esses sim,
 * são filtros de banco (documento 35, seção 10).
 */

import type {
  AlteracaoDeEquipamento,
  BaixaDeEquipamento,
  ComProcedencia,
  ConsultaDeEquipamentos,
  EquipamentoDetalhe,
  EquipamentoResumo,
  MaquinaCompradaPeloCliente,
  NovoEquipamento,
  PaginaDe,
  VendaDaMaquina,
} from '../../tipos/api';
import { ler, pedir, type ContextoDeAcesso } from './http';

/** Os valores iniciais de uma consulta de equipamentos. */
export const CONSULTA_INICIAL: ConsultaDeEquipamentos = {
  pagina: 1,
  tamanho: 25,
  termo: '',
  situacao: '',
  origem: '',
  clienteChave: '',
  ordenarPor: 'Chassi',
  descendente: false,
  incluirInativos: false,
  linhaDeProduto: '',
  porte: '',
  somenteComVenda: false,
};

/** Lista equipamentos da filial do contexto. */
export function listarEquipamentos(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeEquipamentos,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<EquipamentoResumo>>> {
  return ler<PaginaDe<EquipamentoResumo>>('/v1/equipamentos', contexto, {
    sinal,
    parametros: {
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      termo: consulta.termo.trim(),
      situacao: consulta.situacao,
      origem: consulta.origem,
      clienteChave: consulta.clienteChave,
      ordenarPor: consulta.ordenarPor,
      descendente: consulta.descendente,
      incluirInativos: consulta.incluirInativos,
      linhaDeProduto: consulta.linhaDeProduto,
      porte: consulta.porte,
      somenteComVenda: consulta.somenteComVenda,
    },
  });
}

/** Traz a ficha de uma máquina. 404 significa "não existe OU não é desta filial". */
export function obterEquipamento(
  contexto: ContextoDeAcesso,
  chave: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<EquipamentoDetalhe>> {
  return ler<EquipamentoDetalhe>(`/v1/equipamentos/${chave}`, contexto, { sinal });
}

/** O histórico comercial da máquina: as vendas, da mais recente para a mais antiga. */
export function listarVendasDoEquipamento(
  contexto: ContextoDeAcesso,
  chave: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<VendaDaMaquina[]>> {
  return ler<VendaDaMaquina[]>(`/v1/equipamentos/${chave}/vendas`, contexto, { sinal });
}

/**
 * As máquinas que o cliente comprou, pelo vínculo "comprador na venda" — da venda
 * mais recente para a mais antiga. Comprar não faz do cliente o dono atual.
 */
export function listarMaquinasCompradasPeloCliente(
  contexto: ContextoDeAcesso,
  chaveDoCliente: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<MaquinaCompradaPeloCliente[]>> {
  return ler<MaquinaCompradaPeloCliente[]>(`/v1/clientes/${chaveDoCliente}/maquinas-compradas`, contexto, { sinal });
}

/** Cadastra uma máquina — inclusive a do concorrente, que é o que a Cobertura usa. */
export function criarEquipamento(contexto: ContextoDeAcesso, corpo: NovoEquipamento): Promise<EquipamentoDetalhe> {
  return pedir<EquipamentoDetalhe>('/v1/equipamentos', contexto, { metodo: 'POST', corpo });
}

/** Altera uma máquina. Chassi e origem não entram — veja `AlteracaoDeEquipamento`. */
export function alterarEquipamento(
  contexto: ContextoDeAcesso,
  chave: string,
  corpo: AlteracaoDeEquipamento,
): Promise<EquipamentoDetalhe> {
  return pedir<EquipamentoDetalhe>(`/v1/equipamentos/${chave}`, contexto, { metodo: 'PUT', corpo });
}

/** Baixa uma máquina — exclusão lógica. A linha e o histórico de horímetro ficam. */
export function inativarEquipamento(
  contexto: ContextoDeAcesso,
  chave: string,
  corpo: BaixaDeEquipamento,
): Promise<EquipamentoDetalhe> {
  return pedir<EquipamentoDetalhe>(`/v1/equipamentos/${chave}`, contexto, { metodo: 'DELETE', corpo });
}
