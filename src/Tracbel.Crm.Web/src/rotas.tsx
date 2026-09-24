/**
 * Tabela de rotas.
 *
 * A base é o `ROUTES` do protótipo de referência
 * (`prototipo/referencia/assets/app.js`, linha 3), com os mesmos caminhos e
 * breadcrumbs, para a comparação com as capturas continuar valendo nas telas
 * que ainda são o porte do protótipo.
 *
 * **O campo `usaApi`** marca as telas ligadas ao nosso banco pela API. Ele tem
 * um efeito visível: o seletor de filial só aparece no cabeçalho dessas rotas.
 * A filial é a fronteira de acesso da API — nas telas que leem o JSON do
 * protótipo ela não existe, e mostrar o seletor lá prometeria um efeito que não
 * acontece. Também é o que mantém as outras treze rotas idênticas às capturas.
 *
 * **Rotas com parâmetro** (`:chave`) usam o GUID que a API devolve. As rotas de
 * identificador fixo do protótipo (`/clientes/84391`,
 * `/equipamentos/1RW7250PVMR123456`) continuam existindo e são declaradas ANTES
 * das rotas com parâmetro, porque `acharRota` casa a primeira que bater.
 */

import type { ComponentType } from 'react';
import { Agenda } from './telas/Agenda';
import { ClienteFicha } from './telas/ClienteFicha';
import { CoberturaCarteira } from './telas/CoberturaCarteira';
import { CoberturaRegional } from './telas/CoberturaRegional';
import { Configuracoes } from './telas/Configuracoes';
import { EquipamentoFicha } from './telas/EquipamentoFicha';
import { Funil } from './telas/Funil';
import { IndicadoresGeograficos } from './telas/IndicadoresGeograficos';
import { Inicio } from './telas/Inicio';
import { NovaOportunidade } from './telas/NovaOportunidade';
import { OportunidadeFicha } from './telas/OportunidadeFicha';
import { PerformanceCen } from './telas/PerformanceCen';
import { Pipeline } from './telas/Pipeline';
import { Visao360 } from './telas/Visao360';
import { ClienteCadastro } from './telas/cadastro/ClienteCadastro';
import { ClientesLista } from './telas/cadastro/ClientesLista';
import { EquipamentoCadastro } from './telas/cadastro/EquipamentoCadastro';
import { EquipamentosLista } from './telas/cadastro/EquipamentosLista';

export type Rota = {
  caminho: string;
  titulo: string;
  trilha: string[];
  Componente: ComponentType;
  /** Lê e grava pela nossa API. Ganha o seletor de filial no cabeçalho. */
  usaApi?: boolean;
  /**
   * Tela de análise: ocupa a largura inteira da janela, sem o teto de 1.400 px do `.content`.
   * O teto serve a formulário e lista, em que campo esticado atrapalha; num painel de mapas,
   * gráficos e tabelas ele deixava meia tela vazia num monitor largo (21/09/2026).
   */
  larga?: boolean;
};

export const ROTAS: Rota[] = [
  {
    caminho: '/',
    titulo: 'Visão 360',
    trilha: ['Visão 360'],
    Componente: Visao360,
    usaApi: true,
    // A LARGURA INTEIRA (24/09/2026), como os Indicadores Geográficos: com o teto
    // de 1.400 px, uma janela de 1.920 deixava ~320 px vazios à direita do painel.
    // Quem limita e centraliza agora é a página (`PaginaDoPainel`, até 1.580 px);
    // o perfil do CEN guarda o teto antigo dentro da própria tela.
    larga: true,
  },
  {
    // Não é tela de produto: é o índice de desenvolvimento, e fica fora do menu
    // lateral de propósito (documentos 06 §4 e 08 §3).
    caminho: '/inicio-antigo',
    titulo: 'Mapa do protótipo',
    trilha: ['Mapa do protótipo'],
    Componente: Inicio,
  },
  {
    caminho: '/agenda',
    titulo: 'Agenda do CEN',
    trilha: ['Comercial', 'Agenda do CEN'],
    Componente: Agenda,
    usaApi: true,
  },
  {
    caminho: '/cobertura',
    titulo: 'Cobertura de Carteira',
    trilha: ['Comercial', 'Cobertura'],
    Componente: CoberturaCarteira,
    usaApi: true,
  },
  {
    caminho: '/pipeline',
    titulo: 'Pipeline de Vendas',
    trilha: ['Comercial', 'Pipeline'],
    Componente: Pipeline,
    usaApi: true,
  },

  /* ---- Cadastro de clientes, pela API ---------------------------------- */
  {
    caminho: '/clientes',
    titulo: 'Clientes',
    trilha: ['Comercial', 'Clientes'],
    Componente: ClientesLista,
    usaApi: true,
  },
  {
    caminho: '/clientes/novo',
    titulo: 'Novo cliente',
    trilha: ['Comercial', 'Clientes', 'Novo'],
    Componente: ClienteCadastro,
    usaApi: true,
  },
  {
    // A ficha rica do protótipo, que lê JSON. Continua aqui, e ANTES da rota com
    // parâmetro, porque é ela que a comparação visual mede.
    caminho: '/clientes/84391',
    titulo: 'Ficha do Cliente',
    trilha: ['Comercial', 'Clientes', 'Agroindustrial Salvador Arena Ltda'],
    Componente: ClienteFicha,
  },
  {
    caminho: '/clientes/:chave',
    titulo: 'Ficha do Cliente',
    trilha: ['Comercial', 'Clientes', 'Ficha'],
    Componente: ClienteCadastro,
    usaApi: true,
  },

  /* ---- Cadastro de equipamentos, pela API ------------------------------ */
  {
    caminho: '/equipamentos',
    titulo: 'Equipamentos',
    trilha: ['Comercial', 'Equipamentos'],
    Componente: EquipamentosLista,
    usaApi: true,
  },
  {
    caminho: '/equipamentos/novo',
    titulo: 'Novo equipamento',
    trilha: ['Comercial', 'Equipamentos', 'Novo'],
    Componente: EquipamentoCadastro,
    usaApi: true,
  },
  {
    // A ficha rica do protótipo (telemetria, revisões, garantia, peças), que lê
    // JSON. Nenhum desses blocos existe na API ainda; ver o documento
    // `docs/prototipo/07-PADRAO-DE-TELA.md`, seção "o que ficou de fora".
    caminho: '/equipamentos/1RW7250PVMR123456',
    titulo: 'Ficha do Equipamento',
    trilha: ['Comercial', 'Equipamentos', 'Trator 7250R · 1RW7250PVMR123456'],
    Componente: EquipamentoFicha,
  },
  {
    caminho: '/equipamentos/:chave',
    titulo: 'Ficha do Equipamento',
    trilha: ['Comercial', 'Equipamentos', 'Ficha'],
    Componente: EquipamentoCadastro,
    usaApi: true,
  },

  {
    caminho: '/oportunidades/nova',
    titulo: 'Nova Oportunidade',
    trilha: ['Comercial', 'Oportunidades', 'Nova'],
    Componente: NovaOportunidade,
  },
  {
    // A ROTA DEIXOU DE SER FIXA. Era `/oportunidades/1517613` e servia a UMA
    // oportunidade inventada, de `public/dados/oportunidade-1517613.json` —
    // existia uma oportunidade só no sistema inteiro. Agora a chave vem da URL
    // e qualquer um dos 45.397 processos carregados abre.
    //
    // Precisa vir DEPOIS de `/oportunidades/nova`, porque `acharRota` casa a
    // primeira declaração que bater e `nova` seria engolida por `:chave`.
    caminho: '/oportunidades/:chave',
    titulo: 'Ficha de Oportunidade',
    trilha: ['Comercial', 'Oportunidades', 'Ficha'],
    Componente: OportunidadeFicha,
    usaApi: true,
  },
  {
    caminho: '/relatorios/funil',
    // "do Mês" saiu: o recorte carregado é o ANO de 2026, e a rota de funil não
    // aceita período. Prometer um mês que o dado não delimita é o mesmo defeito
    // de mostrar número sem dizer de onde ele vem.
    titulo: 'Funil de Vendas',
    trilha: ['Relatórios', 'Funil de Vendas'],
    Componente: Funil,
    usaApi: true,
  },
  {
    caminho: '/relatorios/performance',
    titulo: 'Performance de CEN',
    trilha: ['Relatórios', 'Performance de CEN'],
    Componente: PerformanceCen,
    usaApi: true,
  },
  {
    // O NOME MUDOU PORQUE O AGRUPAMENTO MUDOU. Era "Painel Regional", e a
    // regional não existe: `IVS_Regional` tem zero linhas no Vórtice
    // (documento 26, §1). O que existe preenchido é filial → carteira →
    // cidades, e é o que a tela mostra.
    caminho: '/relatorios/cobertura',
    titulo: 'Cobertura por Filial e Carteira',
    trilha: ['Relatórios', 'Cobertura por Filial e Carteira'],
    Componente: CoberturaRegional,
    usaApi: true,
  },
  {
    // Os três mapas da ADR — documento 32. Lê a API e a malha do IBGE em
    // `public/geo`; nenhum número é do protótipo.
    caminho: '/relatorios/territorio',
    titulo: 'Indicadores Geográficos da ADR',
    trilha: ['Relatórios', 'Indicadores Geográficos'],
    Componente: IndicadoresGeograficos,
    usaApi: true,
    larga: true,
  },
  {
    caminho: '/config',
    titulo: 'Configurações',
    trilha: ['Sistema', 'Configurações'],
    Componente: Configuracoes,
    // A LARGURA INTEIRA (22/09/2026): com o menu lateral e as tabelas de usuários, o teto de 1400 px deixava
    // uma faixa vazia à direita em tela larga ou com o navegador em zoom reduzido.
    larga: true,
  },
];

/** Um caminho declarado vira o padrão que casa com ele, com `:parametro` virando um trecho. */
function casa(declarado: string, caminho: string): boolean {
  if (!declarado.includes(':')) return declarado === caminho;
  const padrao = new RegExp(`^${declarado.replace(/:[^/]+/g, '[^/]+')}$`);
  return padrao.test(caminho);
}

/**
 * A rota de um caminho. Casa a PRIMEIRA declarada que bater, e é por isso que
 * `/clientes/84391` precisa vir antes de `/clientes/:chave` na lista.
 */
export function acharRota(caminho: string): Rota {
  return ROTAS.find((r) => casa(r.caminho, caminho)) ?? ROTAS[0];
}
