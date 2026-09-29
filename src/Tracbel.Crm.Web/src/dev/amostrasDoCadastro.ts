/**
 * AS AMOSTRAS DO CADASTRO NO HARNESS DA VISÃO 360 (29/09/2026, #293 bloco 5) — só em desenvolvimento.
 *
 * As listas de Clientes e de Equipamentos passaram para o padrão dos Indicadores, e conferir o desenho delas pedia banco
 * e VPN. Aqui elas abrem pelo mesmo harness (`#/dev/visao360-visual?rota=/clientes`), sobre respostas com a forma do
 * contrato: `/v1/catalogos`, `/v1/clientes` e `/v1/equipamentos`.
 *
 * NADA AQUI É DADO DA TRACBEL. Os nomes dizem que são fictícios, e o chassi começa com `1FC`, que não é prefixo de
 * fábrica nenhuma.
 *
 * A CONSULTA É RESPEITADA COMO A API A RESPEITA — busca, filtros de banco, inativos, ordem e página —, para a tela
 * exercitar o mesmo caminho da produção: o total que ela mostra é o total do filtro, e a página seguinte existe.
 *
 * Mora num arquivo próprio, e não em `amostrasDaVisao360.ts`, para os blocos do #293 abertos juntos não disputarem as
 * mesmas linhas.
 */

import { SEM_CLASSIFICACAO, type CatalogoDeSelecao, type ClienteResumo, type EquipamentoResumo, type PaginaDe } from '../tipos/api';
import type { EstadoDaVisao360 } from './amostrasDaVisao360';

function catalogo(codigo: string, nome: string, itens: [string, string][]): CatalogoDeSelecao {
  return {
    codigo,
    nome,
    descricao: null,
    permiteItemNovo: false,
    itens: itens.map(([c, descricao], ordem) => ({ codigo: c, descricao, ordem, exigeObservacao: false })),
  };
}

const CATALOGOS: CatalogoDeSelecao[] = [
  catalogo('SITUACAO_CLIENTE', 'Situação do cliente', [
    ['Suspect', 'Suspect'],
    ['Prospect', 'Prospect'],
    ['Cliente', 'Cliente'],
    ['ClienteInativo', 'Cliente inativo'],
  ]),
  catalogo('TIPO_DE_PESSOA', 'Tipo de pessoa', [
    ['Fisica', 'Física'],
    ['Juridica', 'Jurídica'],
  ]),
  catalogo('ORIGEM_LEAD', 'Origem do lead', [
    ['FEIRA', 'Feira'],
    ['INDICACAO', 'Indicação'],
  ]),
  catalogo('MOTIVO_INATIVACAO', 'Motivo da inativação', [
    ['DUPLICADO', 'Cadastro duplicado'],
    ['ENCERROU', 'Encerrou a atividade'],
  ]),
  catalogo('SITUACAO_EQUIPAMENTO', 'Situação do equipamento', [
    ['Estoque', 'Estoque'],
    ['Ativo', 'Ativo'],
    ['Vendido', 'Vendido'],
    ['Baixado', 'Baixado'],
    ['ProprietarioNaoConfirmado', 'Dono não confirmado'],
  ]),
  catalogo('ORIGEM_EQUIPAMENTO', 'Origem do equipamento', [
    ['Protheus', 'Protheus'],
    ['Crm', 'CRM'],
    ['Art', 'ART'],
  ]),
  catalogo('LINHA_DE_PRODUTO', 'Classificação de produto', [
    ['TRATOR_PEQUENO', 'Trator pequeno'],
    ['TRATOR_MEDIO', 'Trator médio'],
    ['TRATOR_GRANDE', 'Trator grande'],
    ['COLHEDORA', 'Colhedora'],
    ['PULVERIZADOR', 'Pulverizador'],
  ]),
  catalogo('PORTE_DE_MAQUINA', 'Porte da máquina', [
    ['Pequeno', 'Pequeno'],
    ['Medio', 'Médio'],
    ['Grande', 'Grande'],
    ['NaoSeAplica', 'Não se aplica'],
  ]),
  catalogo('MODELO_EQUIPAMENTO', 'Modelo do equipamento', [
    ['FIC-6155M', 'Marca Fictícia · Série 6M · 6155M'],
    ['FIC-7230J', 'Marca Fictícia · Série 7J · 7230J'],
    ['FIC-8R340', 'Marca Fictícia · Série 8R · 8R 340'],
    ['FIC-S760', 'Marca Fictícia · Colhedora S · S760'],
    ['FIC-M4040', 'Marca Fictícia · Pulverizador M · M4040'],
    ['CONC-T7', 'Concorrente Fictício · Série T · T7.245'],
  ]),
];

/** Uma página como a API devolve, depois do filtro e da ordem. */
function paginar<T>(itens: T[], consulta: URLSearchParams): PaginaDe<T> {
  const pagina = Math.max(1, Number(consulta.get('pagina') ?? '1'));
  const tamanho = Math.max(1, Number(consulta.get('tamanho') ?? '25'));
  const inicio = (pagina - 1) * tamanho;
  return {
    itens: itens.slice(inicio, inicio + tamanho),
    pagina,
    tamanho,
    total: itens.length,
    totalDePaginas: Math.max(1, Math.ceil(itens.length / tamanho)),
    temProxima: inicio + tamanho < itens.length,
  };
}

/** A ordem pedida, pelo campo que a API sabe ordenar. */
function ordenar<T>(itens: T[], consulta: URLSearchParams, chave: (item: T, campo: string) => string | number): T[] {
  const campo = consulta.get('ordenarPor') ?? '';
  const sentido = consulta.get('descendente') === 'true' ? -1 : 1;
  return [...itens].sort((a, b) => {
    const x = chave(a, campo);
    const y = chave(b, campo);
    return (typeof x === 'number' && typeof y === 'number' ? x - y : String(x).localeCompare(String(y), 'pt-BR')) * sentido;
  });
}

const RAZOES = [
  'Cooperativa Agroindustrial Fictícia dos Produtores de Cana e Laranja do Norte Paulista',
  'Agropecuária Fictícia Santa Clara Ltda',
  'Usina Fictícia Boa Vista Açúcar e Etanol S.A.',
  'Fazenda Fictícia Três Irmãos',
  'Produtor Fictício Alfa',
  'Produtor Fictício Beta',
  'Citrícola Fictícia Vale do Rio Grande',
  'Grãos Fictícios do Cerrado Ltda',
  'Produtor Fictício Gama',
  'Pecuária Fictícia Ribeirão Ltda',
  'Produtor Fictício Delta',
  'Cafeicultura Fictícia Alta Mogiana',
];
const SITUACOES_DE_CLIENTE = ['Cliente', 'Prospect', 'Suspect', 'Cliente', 'ClienteInativo', 'Prospect'];

const CLIENTES: ClienteResumo[] = Array.from({ length: 36 }, (_, i) => {
  const juridica = i % 3 !== 1;
  const razao = RAZOES[i % RAZOES.length]!;
  return {
    chave: `cliente-ficticio-${String(i + 1).padStart(3, '0')}`,
    nomeRazao: i < RAZOES.length ? razao : `${razao} ${Math.floor(i / RAZOES.length) + 1}`,
    nomeFantasia: juridica && i % 4 === 0 ? `Fantasia Fictícia ${i + 1}` : null,
    tipoDePessoa: juridica ? 'Juridica' : 'Fisica',
    documento: i % 7 === 5 ? null : juridica ? `00.000.${String(100 + i).padStart(3, '0')}/0001-00` : `000.000.${String(100 + i).padStart(3, '0')}-00`,
    situacao: SITUACOES_DE_CLIENTE[i % SITUACOES_DE_CLIENTE.length]!,
    criadoEm: `2026-0${(i % 9) + 1}-${String((i % 27) + 1).padStart(2, '0')}T09:${String(i % 60).padStart(2, '0')}:00`,
    alteradoEm: i % 3 === 0 ? null : `2026-09-${String((i % 28) + 1).padStart(2, '0')}T14:30:00`,
    estaInativo: i === 7 || i === 22,
  };
});

function listaDeClientes(estado: EstadoDaVisao360, consulta: URLSearchParams): PaginaDe<ClienteResumo> {
  if (estado === 'vazio') return paginar([], consulta);
  const termo = (consulta.get('termo') ?? '').trim().toLocaleLowerCase('pt-BR');
  const situacao = consulta.get('situacao') ?? '';
  const tipo = consulta.get('tipoDePessoa') ?? '';
  const inativos = consulta.get('incluirInativos') === 'true';
  const filtrados = CLIENTES.filter(
    (c) =>
      (inativos || !c.estaInativo) &&
      (!situacao || c.situacao === situacao) &&
      (!tipo || c.tipoDePessoa === tipo) &&
      (!termo || `${c.nomeRazao} ${c.nomeFantasia ?? ''}`.toLocaleLowerCase('pt-BR').includes(termo) || c.documento === termo),
  );
  return paginar(
    ordenar(filtrados, consulta, (c, campo) =>
      campo === 'Situacao' ? c.situacao : campo === 'CriadoEm' ? c.criadoEm : campo === 'AlteradoEm' ? (c.alteradoEm ?? '') : c.nomeRazao,
    ),
    consulta,
  );
}

type ModeloDaAmostra = { codigo: string; nome: string; marca: string; nossa: boolean; classe: string; classeNome: string; porte: string };
const MODELOS: ModeloDaAmostra[] = [
  { codigo: 'FIC-6155M', nome: '6155M', marca: 'Marca Fictícia', nossa: true, classe: 'TRATOR_MEDIO', classeNome: 'Trator médio', porte: 'Medio' },
  { codigo: 'FIC-7230J', nome: '7230J', marca: 'Marca Fictícia', nossa: true, classe: 'TRATOR_GRANDE', classeNome: 'Trator grande', porte: 'Grande' },
  { codigo: 'FIC-8R340', nome: '8R 340', marca: 'Marca Fictícia', nossa: true, classe: 'TRATOR_GRANDE', classeNome: 'Trator grande', porte: 'Grande' },
  { codigo: 'FIC-S760', nome: 'S760', marca: 'Marca Fictícia', nossa: true, classe: 'COLHEDORA', classeNome: 'Colhedora', porte: 'NaoSeAplica' },
  { codigo: 'FIC-M4040', nome: 'M4040', marca: 'Marca Fictícia', nossa: true, classe: 'PULVERIZADOR', classeNome: 'Pulverizador', porte: 'NaoSeAplica' },
  { codigo: 'CONC-T7', nome: 'T7.245', marca: 'Concorrente Fictício', nossa: false, classe: 'TRATOR_MEDIO', classeNome: 'Trator médio', porte: 'Medio' },
];
const SITUACOES_DE_MAQUINA = ['Ativo', 'ProprietarioNaoConfirmado', 'Ativo', 'Estoque', 'Vendido', 'ProprietarioNaoConfirmado', 'Ativo'];
const ORIGENS = ['Protheus', 'Art', 'Protheus', 'Crm', 'Protheus'];
const EVIDENCIAS = ['NotaDeVenda', 'OrdemDeServico', 'CadastroAntigo', 'VendaNoArt'];

const EQUIPAMENTOS: EquipamentoResumo[] = Array.from({ length: 44 }, (_, i) => {
  const modelo = i % 11 === 10 ? null : MODELOS[i % MODELOS.length]!;
  const situacao = SITUACOES_DE_MAQUINA[i % SITUACOES_DE_MAQUINA.length]!;
  const dono = CLIENTES[(i * 5) % CLIENTES.length]!;
  const confirmado = situacao === 'Ativo' && i % 2 === 0;
  const comVenda = i % 3 === 0;
  const comprador = CLIENTES[(i * 7 + 3) % CLIENTES.length]!;
  return {
    chave: `maquina-ficticia-${String(i + 1).padStart(3, '0')}`,
    chassi: `1FC${(modelo?.nome ?? 'X000').replace(/[^A-Z0-9]/gi, '').toUpperCase().padEnd(5, '0').slice(0, 5)}P${String(100000 + i * 137).slice(0, 6)}${'ABCDEFGH'[i % 8]}${String(i).padStart(2, '0')}`.slice(0, 17),
    modeloCodigo: modelo?.codigo ?? null,
    modeloNome: modelo?.nome ?? null,
    marca: modelo?.marca ?? null,
    marcaRepresentada: modelo ? modelo.nossa : null,
    situacao,
    origem: ORIGENS[i % ORIGENS.length]!,
    anoModelo: i % 9 === 8 ? null : 2014 + (i % 13),
    clienteChave: confirmado ? dono.chave : null,
    clienteNome: confirmado ? dono.nomeRazao : null,
    criadoEm: `2026-0${(i % 9) + 1}-${String((i % 27) + 1).padStart(2, '0')}T08:15:00`,
    alteradoEm: null,
    estaInativo: situacao === 'Vendido' && i % 2 === 0,
    classificacaoCodigo: modelo?.classe ?? null,
    classificacaoNome: modelo?.classeNome ?? null,
    porte: modelo?.porte ?? null,
    vendas: comVenda ? 1 + (i % 2) : 0,
    ultimaVendaEm: comVenda ? `2025-${String((i % 12) + 1).padStart(2, '0')}-15` : null,
    compradorNaUltimaVendaChave: comVenda && i % 4 !== 3 ? comprador.chave : null,
    compradorNaUltimaVendaNome: comVenda && i % 4 !== 3 ? comprador.nomeRazao : null,
    naturezaDoVinculo: comVenda ? 'CompradorNaVenda' : null,
    produtoNaOrigem: comVenda ? `${modelo?.marca ?? 'Marca'} ${modelo?.nome ?? 'sem modelo'}` : null,
    sistemaDaVenda: comVenda ? 'ART' : null,
    donoAtualChave: !confirmado && i % 5 !== 4 ? dono.chave : null,
    donoAtualNome: !confirmado && i % 5 !== 4 ? dono.nomeRazao : null,
    evidenciaDoDonoAtual: !confirmado && i % 5 !== 4 ? EVIDENCIAS[i % EVIDENCIAS.length]! : null,
    evidenciaDoDonoAtualEm: !confirmado && i % 5 !== 4 ? `2025-${String((i % 12) + 1).padStart(2, '0')}-01` : null,
    relacaoComOCliente: null,
  };
});

function listaDeEquipamentos(estado: EstadoDaVisao360, consulta: URLSearchParams): PaginaDe<EquipamentoResumo> {
  if (estado === 'vazio') return paginar([], consulta);
  const termo = (consulta.get('termo') ?? '').trim().toUpperCase();
  const filtro = (nome: string) => consulta.get(nome) ?? '';
  const inativos = consulta.get('incluirInativos') === 'true';
  const comVenda = consulta.get('somenteComVenda') === 'true';
  const cliente = filtro('clienteChave');
  const filtrados = EQUIPAMENTOS.filter(
    (m) =>
      (inativos || !m.estaInativo) &&
      (!termo || m.chassi === termo) &&
      (!filtro('situacao') || m.situacao === filtro('situacao')) &&
      (!filtro('origem') || m.origem === filtro('origem')) &&
      (!filtro('porte') || m.porte === filtro('porte')) &&
      (!filtro('linhaDeProduto') ||
        (filtro('linhaDeProduto') === SEM_CLASSIFICACAO ? m.classificacaoCodigo === null : m.classificacaoCodigo === filtro('linhaDeProduto'))) &&
      (!comVenda || m.vendas > 0) &&
      (!cliente || m.clienteChave === cliente || m.donoAtualChave === cliente || m.compradorNaUltimaVendaChave === cliente),
  ).map((m) =>
    cliente
      ? {
          ...m,
          relacaoComOCliente: {
            ehDonoAtual: m.donoAtualChave === cliente,
            ehDonoConfirmado: m.clienteChave === cliente,
            compradaEm: m.compradorNaUltimaVendaChave === cliente ? m.ultimaVendaEm : null,
            compradaPeloDonoNoProtheus: false,
          },
        }
      : m,
  );
  return paginar(
    ordenar(filtrados, consulta, (m, campo) =>
      campo === 'AnoModelo' ? (m.anoModelo ?? 0) : campo === 'Situacao' ? m.situacao : campo === 'CriadoEm' ? m.criadoEm : m.chassi,
    ),
    consulta,
  );
}

/** A amostra de cada rota do cadastro que as listas leem — ou `undefined` para a rota que ninguém simulou. */
export function respostaDoCadastro(caminho: string, consulta: URLSearchParams, estado: EstadoDaVisao360): unknown {
  switch (caminho) {
    case '/v1/catalogos':
      return CATALOGOS;
    case '/v1/clientes':
      return listaDeClientes(estado, consulta);
    case '/v1/equipamentos':
      return listaDeEquipamentos(estado, consulta);
    default:
      return undefined;
  }
}
