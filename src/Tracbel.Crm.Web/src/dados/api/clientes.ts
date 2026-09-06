/**
 * As cinco operações de cliente contra o nosso banco (documento 23, seção 2.1).
 *
 * Nenhuma regra de negócio mora aqui: estas funções montam a requisição e
 * devolvem o que a API respondeu. Quem recusa é a API, campo a campo — repetir
 * a validação no front criaria duas verdades e, quando divergissem, a do front
 * seria a errada.
 */

import type {
  AlteracaoDeCliente,
  ClienteDetalhe,
  ClienteResumo,
  ComProcedencia,
  ConsultaDeClientes,
  InativacaoDeCliente,
  NovoCliente,
  PaginaDe,
} from '../../tipos/api';
import { ler, pedir, type ContextoDeAcesso } from './http';

/** Os valores iniciais de uma consulta de clientes. */
export const CONSULTA_INICIAL: ConsultaDeClientes = {
  pagina: 1,
  tamanho: 25,
  termo: '',
  situacao: '',
  tipoDePessoa: '',
  ordenarPor: 'Nome',
  descendente: false,
  incluirInativos: false,
};

/** Lista clientes da filial do contexto, com paginação, filtro e ordenação. */
export function listarClientes(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeClientes,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<ClienteResumo>>> {
  return ler<PaginaDe<ClienteResumo>>('/v1/clientes', contexto, {
    sinal,
    parametros: {
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      termo: consulta.termo.trim(),
      situacao: consulta.situacao,
      tipoDePessoa: consulta.tipoDePessoa,
      ordenarPor: consulta.ordenarPor,
      descendente: consulta.descendente,
      incluirInativos: consulta.incluirInativos,
    },
  });
}

/** Traz a ficha de um cliente. 404 significa "não existe OU não é desta filial". */
export function obterCliente(
  contexto: ContextoDeAcesso,
  chave: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<ClienteDetalhe>> {
  return ler<ClienteDetalhe>(`/v1/clientes/${chave}`, contexto, { sinal });
}

/**
 * Cadastra um cliente NA FILIAL DO CONTEXTO.
 *
 * A filial não vai no corpo, e não é esquecimento: aceitar a empresa no JSON
 * abriria um caminho para gravar na filial de outro (documento 23, seção 4.3).
 */
export function criarCliente(contexto: ContextoDeAcesso, corpo: NovoCliente): Promise<ClienteDetalhe> {
  return pedir<ClienteDetalhe>('/v1/clientes', contexto, { metodo: 'POST', corpo });
}

/** Altera um cliente. A versão é a que veio no GET — sem ela não há proteção de concorrência. */
export function alterarCliente(
  contexto: ContextoDeAcesso,
  chave: string,
  corpo: AlteracaoDeCliente,
): Promise<ClienteDetalhe> {
  return pedir<ClienteDetalhe>(`/v1/clientes/${chave}`, contexto, { metodo: 'PUT', corpo });
}

/** Inativa um cliente — exclusão lógica, com motivo de catálogo. Nada é apagado. */
export function inativarCliente(
  contexto: ContextoDeAcesso,
  chave: string,
  corpo: InativacaoDeCliente,
): Promise<ClienteDetalhe> {
  return pedir<ClienteDetalhe>(`/v1/clientes/${chave}`, contexto, { metodo: 'DELETE', corpo });
}
