/**
 * AS AMOSTRAS DO CADASTRO NO HARNESS DA VISÃO 360 (29/09/2026, #293 bloco 5) — só em desenvolvimento.
 *
 * As listas de Clientes e de Equipamentos passaram para o padrão dos Indicadores, e conferir o desenho delas pedia banco
 * e VPN. Aqui elas abrem pelo mesmo harness (`#/dev/visao360-visual?rota=/clientes`), sobre respostas com a forma do
 * contrato: `/v1/catalogos`, `/v1/clientes` e `/v1/equipamentos`. As fichas (bloco 5b) leem daqui também: a do cliente,
 * a da máquina, as vendas da máquina e as máquinas que o cliente comprou.
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

import {
  SEM_CLASSIFICACAO,
  type CatalogoDeSelecao,
  type ClienteDetalhe,
  type ClienteResumo,
  type EquipamentoDetalhe,
  type EquipamentoResumo,
  type MaquinaCompradaPeloCliente,
  type PaginaDe,
  type VendaDaMaquina,
} from '../tipos/api';
import type { OrdensDeServicoResumidas } from '../tipos/ordensDeServico';
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

/** A família do modelo, como o catálogo a escreve (`Marca · Família · Modelo`). */
function familiaDo(modeloCodigo: string | null): string | null {
  const item = CATALOGOS.find((c) => c.codigo === 'MODELO_EQUIPAMENTO')?.itens.find((i) => i.codigo === modeloCodigo);
  return item?.descricao.split(' · ')[1] ?? null;
}

/** A ficha do cliente: o resumo da lista, com o que só a ficha mostra. */
function fichaDoCliente(chave: string): ClienteDetalhe | undefined {
  const cliente = CLIENTES.find((c) => c.chave === chave);
  if (!cliente) return undefined;
  return {
    ...cliente,
    documentoSemMascara: cliente.documento?.replace(/\D/g, '') ?? null,
    inscricaoEstadual: cliente.tipoDePessoa === 'Juridica' ? '000.000.000.000' : null,
    atividadeEconomica: 'Atividade econômica fictícia',
    situacaoDesde: cliente.criadoEm,
    empresaId: 1,
    proprietarioId: 1,
    origemCodigo: 'FEIRA',
    motivoInativacaoCodigo: cliente.estaInativo ? 'DUPLICADO' : null,
    versao: 'AAAAAAAAB9E=',
  };
}

/** A ficha da máquina: o resumo da lista, com a telemetria, o registro e — na 004 — duas divergências abertas. */
function fichaDoEquipamento(chave: string): EquipamentoDetalhe | undefined {
  const indice = EQUIPAMENTOS.findIndex((m) => m.chave === chave);
  if (indice < 0) return undefined;
  const { relacaoComOCliente: _semRelacao, ...maquina } = EQUIPAMENTOS[indice]!;
  const comTelemetria = indice % 2 === 1;
  return {
    ...maquina,
    numeroSerie: `NS-FIC-${String(indice + 1).padStart(5, '0')}`,
    placa: null,
    familia: familiaDo(maquina.modeloCodigo),
    anoFabricacao: maquina.anoModelo ? maquina.anoModelo - 1 : null,
    horimetroAtual: comTelemetria ? 3_482.5 + indice * 97 : null,
    horimetroAtualizadoEm: comTelemetria ? '2026-09-28T06:55:00Z' : null,
    posicaoLatitude: comTelemetria ? -21.1775 : null,
    posicaoLongitude: comTelemetria ? -47.8103 : null,
    posicaoEm: comTelemetria ? '2026-09-28T06:40:00Z' : null,
    municipioDaPosicao: comTelemetria ? 'Município Fictício' : null,
    localizacaoDescrita: 'Fazenda Fictícia Santa Clara · talhão 4',
    empresaId: 1,
    versao: 'AAAAAAAAC1Q=',
    divergenciasAbertas:
      indice === 3
        ? [
            {
              tipo: 'CompradorDiferenteDoDono',
              descricao: 'O comprador da venda no ART não é o dono atual pela sincronia do parque — texto fictício, do harness.',
              detectadaEm: '2026-09-27T05:10:00Z',
            },
            {
              tipo: 'ModeloSemCorrespondencia',
              descricao: 'O produto do ART não tem correspondência segura no catálogo de modelos — texto fictício, do harness.',
              detectadaEm: '2026-09-26T05:10:00Z',
            },
          ]
        : [],
  };
}

/** As vendas da máquina, da mais recente para a mais antiga: a primeira com o comprador da lista, a outra encerrada. */
function vendasDaMaquina(chave: string): VendaDaMaquina[] | undefined {
  const maquina = EQUIPAMENTOS.find((m) => m.chave === chave);
  if (!maquina) return undefined;
  return Array.from({ length: maquina.vendas }, (_, k) => ({
    chave: `${chave}-venda-${k + 1}`,
    sistemaCodigo: 'ART',
    chaveOrigem: `FIC-${String(900001 + k)}`,
    vendidaEm: k === 0 ? maquina.ultimaVendaEm : '2021-03-10',
    faturadaEm: k === 0 ? maquina.ultimaVendaEm : '2021-03-15',
    entregueEm: k === 0 ? maquina.ultimaVendaEm : '2021-04-02',
    registradaNaOrigemEm: null,
    filialCodigo: '010101',
    filialNome: 'Filial Fictícia Alfa',
    filialDoFaturamentoCodigo: k === 1 ? '010102' : '010101',
    compradorChave: k === 0 ? maquina.compradorNaUltimaVendaChave : null,
    compradorNome: k === 0 ? maquina.compradorNaUltimaVendaNome : null,
    natureza: 'CompradorNaVenda',
    vinculoReferenciaEm: null,
    vinculoEncerradoEm: k === 1 ? '2025-01-15T00:00:00Z' : null,
    motivoDoEncerramento: k === 1 ? 'máquina revendida (fictício)' : null,
    linhaNaOrigem: 'Linha fictícia',
    produtoNaOrigem: maquina.produtoNaOrigem ?? 'Produto fictício',
    gestaoNaOrigem: k === 0 ? 'Varejo' : 'Grandes Contas',
    situacaoNaOrigem: 'Entregue',
    numeroDoPedido: null,
    numeroDaNotaFiscal: null,
    vendaDireta: false,
    repasseDireto: false,
    unidadeNaOrigem: null,
    unidadeDoFaturamentoNaOrigem: null,
    transformacoes: k === 1 ? 'valor com vírgula decimal convertido — texto fictício, do harness' : null,
    importadaEm: '2026-09-29T06:00:00Z',
    atualizadaPelaOrigemEm: null,
  }));
}

/** As máquinas que o cliente comprou: as da lista em que ele é o comprador da última venda. */
function maquinasCompradas(chaveDoCliente: string): MaquinaCompradaPeloCliente[] | undefined {
  if (!CLIENTES.some((c) => c.chave === chaveDoCliente)) return undefined;
  return EQUIPAMENTOS.filter((m) => m.compradorNaUltimaVendaChave === chaveDoCliente).map((m) => ({
    equipamentoChave: m.chave,
    chassi: m.chassi,
    modeloNome: m.modeloNome,
    classificacaoNome: m.classificacaoNome,
    produtoNaOrigem: m.produtoNaOrigem,
    vendidaEm: m.ultimaVendaEm,
    natureza: 'CompradorNaVenda',
    filialCodigo: '010101',
    sistemaCodigo: 'ART',
    ehDonoAtual: m.donoAtualChave === chaveDoCliente || m.clienteChave === chaveDoCliente,
  }));
}

/**
 * As ordens de serviço de uma ficha (02/10/2026): uma aberta há 52 dias (na faixa vermelha) e uma fechada — números
 * fictícios, do harness.
 */
function ordensDeServico(chassi: string | null, modelo: string | null): OrdensDeServicoResumidas {
  const base = {
    filialCodigo: '010101', filialNome: 'Filial Fictícia Alfa', tipoDeAtendimento: 'OFICINA', liberadaEm: null, canceladaEm: null,
    chassi, modelo, equipamentoChave: null, clienteChave: CLIENTES[0]?.chave ?? null, clienteNome: CLIENTES[0]?.nomeRazao ?? null,
  };
  const aberta = {
    ...base, chave: 'os-ficticia-1', numero: '00012345', situacao: 'Aberta' as const, abertaEm: '2026-08-11', fechadaEm: null,
    horimetro: 3420.5, valorDePecas: 18450.9, valorDeServicos: 4200, itensDePeca: 7, itensDeServico: 3,
  };
  const fechada = {
    ...base, chave: 'os-ficticia-2', numero: '00011002', situacao: 'Fechada' as const, abertaEm: '2026-03-02', fechadaEm: '2026-03-09',
    horimetro: 3105, valorDePecas: 2310, valorDeServicos: 1500, itensDePeca: 2, itensDeServico: 1,
  };
  return {
    emAberto: 1, emAbertoHaMaisDe45Dias: 1, diasDaMaisAntigaEmAberto: 52, valorEmAberto: 22650.9, nosUltimos12Meses: 1,
    pecasNosUltimos12Meses: 2310, servicosNosUltimos12Meses: 1500, ultimaAbertaEm: '2026-08-11', totalDeOrdens: 2,
    ordens: [{ ordem: aberta, diasEmAberto: 52 }, { ordem: fechada, diasEmAberto: null }],
    carregadoEm: '2026-10-02T09:00:00Z', metricasSemDado: [],
  };
}

/** As rotas das fichas, pelo caminho com a chave: sem o registro, `undefined` — e o harness responde 404. */
function respostaDeFicha(caminho: string, estado: EstadoDaVisao360): unknown {
  if (estado === 'vazio') return undefined;
  const cliente = /^\/v1\/clientes\/([^/]+)(\/maquinas-compradas|\/ordens-de-servico)?$/.exec(caminho);
  if (cliente) {
    if (cliente[2] === '/ordens-de-servico') return CLIENTES.some((c) => c.chave === cliente[1]) ? ordensDeServico(null, null) : undefined;
    return cliente[2] ? maquinasCompradas(cliente[1]!) : fichaDoCliente(cliente[1]!);
  }
  const maquina = /^\/v1\/equipamentos\/([^/]+)(\/vendas|\/ordens-de-servico)?$/.exec(caminho);
  if (maquina) {
    if (maquina[2] === '/ordens-de-servico') {
      const encontrada = EQUIPAMENTOS.find((m) => m.chave === maquina[1]);
      return encontrada ? ordensDeServico(encontrada.chassi, encontrada.modeloNome) : undefined;
    }
    return maquina[2] ? vendasDaMaquina(maquina[1]!) : fichaDoEquipamento(maquina[1]!);
  }
  return undefined;
}

/** A amostra de cada rota do cadastro que as listas e as fichas leem — ou `undefined` para a rota que ninguém simulou. */
export function respostaDoCadastro(caminho: string, consulta: URLSearchParams, estado: EstadoDaVisao360): unknown {
  const ficha = respostaDeFicha(caminho, estado);
  if (ficha !== undefined) return ficha;
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
