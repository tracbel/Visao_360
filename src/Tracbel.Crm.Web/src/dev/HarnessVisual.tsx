/**
 * O HARNESS VISUAL — `#/dev/mercado-visual`, só em desenvolvimento (fase T4.5).
 *
 * POR QUE ELE EXISTE. Conferir a tela de Mercado exigia banco, VPN e sessão. É
 * justamente quando falta a VPN — que é quase sempre — que ninguém consegue
 * olhar a página inteira, e ajuste de layout vira aposta. Aqui a tela monta
 * inteira, sobre amostras fictícias, em qualquer máquina, sem rede da empresa.
 *
 * COMO ELE ALIMENTA A TELA: interceptando `fetch`, e não injetando dependência
 * na tela. Assim o caminho exercitado é o de produção do começo ao fim — o
 * `pedir()`, o envelope da procedência, o `useRecurso` e os quatro estados dele.
 * A tela não tem uma linha a mais por causa do harness, e não sabe que ele
 * existe.
 *
 * A MALHA REAL PASSA DIRETO. `geo/sp-municipios.json` é geografia pública que a
 * aplicação já publica; deixá-la passar é o que faz os quatro mapas desenharem
 * de verdade — que é o que se quer olhar. Só os `/api/` são respondidos daqui.
 *
 * O CARIMBO NÃO SAI DA TELA. Toda captura leva "AMOSTRA FICTÍCIA" na barra do
 * topo: se uma delas for parar numa apresentação, ela se denuncia sozinha.
 *
 * ELE NUNCA ENTRA NA NAVEGAÇÃO NEM NO PACOTE. `App.tsx` só o alcança sob
 * `import.meta.env.DEV`, por import dinâmico — no `build` o ramo inteiro morre e
 * este arquivo não chega ao `dist`.
 */

import { useEffect, useState } from 'react';
import { MemoryRouter } from 'react-router-dom';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { IndicadoresGeograficos } from '../telas/IndicadoresGeograficos';
import type { ColecaoMunicipal } from '../componentes/territorio/projecao';
import { ESTADOS, painelFicticio, type NomeDoEstado } from './amostras';
import { creditoFicticio, custosFicticios, precosFicticios, rentabilidadeFicticia } from './amostrasDeMercado';
import '../estilos/design-system.css';

/** O município fixo dos estados que abrem a ficha — sempre o mesmo, para a captura comparar. */
const MUNICIPIO_DA_FICHA = 'municipio';

function estadoDaUrl(): NomeDoEstado {
  const consulta = window.location.hash.split('?')[1] ?? '';
  const pedido = new URLSearchParams(consulta).get('estado');
  return ESTADOS.some((e) => e.id === pedido) ? (pedido as NomeDoEstado) : 'completo';
}

/** Uma resposta de API pronta, no envelope que o `pedir()` espera. */
function envelope(dados: unknown): Response {
  return new Response(
    JSON.stringify({
      dados,
      procedencia: {
        sistema: 'HARNESS VISUAL — amostra fictícia',
        objeto: 'nenhum: nada aqui veio de banco',
        lidoEmUtc: '2026-09-23T12:00:00Z',
        dadoMaisRecenteEm: null,
      },
    }),
    { status: 200, headers: { 'Content-Type': 'application/json' } },
  );
}

/**
 * OS QUATRO PAINÉIS DE MERCADO TÊM AMOSTRA (fase T4.7).
 *
 * A T4.5 os deixou vazios e escreveu que enchê-los era passo seguinte. O passo
 * chegou pelo pior caminho: pediram a revisão visual de Rentabilidade e Crédito
 * e não havia como olhar nenhum dos dois — os dois abriam dizendo "a carga não
 * rodou". Um harness que não mostra o painel não serve para revisar o painel.
 *
 * O estado VAZIO continua alcançável pelo `parcialmenteVazio`, que é onde ele
 * precisa estar: como um dos estados da tela, e não como o único.
 */
function respostasDeMercado(malha: ColecaoMunicipal, estado: NomeDoEstado): Record<string, unknown> {
  if (estado === 'parcialmenteVazio')
    return {
      '/v1/territorio/precos': { series: [], primeiroMesDoDolar: null, ultimoMesDoDolar: null },
      '/v1/territorio/rentabilidade': [],
      '/v1/territorio/custos': [],
      '/v1/territorio/credito': {
        ultimoMes: null,
        janela: null,
        porAno: [],
        porProduto: [],
        porMunicipio: [],
        regiao: null,
        saoPaulo: null,
        procedencia: null,
      },
    };

  const municipios = malha.features.map((f) => ({ codigo: Number(f.properties.codarea), nome: f.properties.nome }));

  return {
    '/v1/territorio/precos': precosFicticios(),
    '/v1/territorio/rentabilidade': rentabilidadeFicticia(),
    '/v1/territorio/custos': custosFicticios(),
    '/v1/territorio/credito': creditoFicticio(municipios),
  };
}

const fetchDeVerdade = window.fetch.bind(window);

/**
 * O INTERCEPTADOR É INSTALADO UMA VEZ, NO CARREGAMENTO DO MÓDULO — e não num
 * `useEffect`.
 *
 * POR QUE ISSO IMPORTA (medido em 23/09/2026): a aplicação roda em `StrictMode`,
 * e o React 19 monta, **limpa** e monta de novo todo efeito em desenvolvimento.
 * A primeira versão instalava no `useEffect` e devolvia a desinstalação como
 * limpeza — então o React desinstalava o interceptador no instante seguinte ao
 * primeiro render, as chamadas iam para o proxy do Vite, a API não estava no ar
 * e a tela inteira caía no estado de erro. Sete dos nove estados não
 * desenhavam, e o sintoma não apontava para o StrictMode em lugar nenhum.
 *
 * Instalado no módulo, ele não tem ciclo de vida para o React interferir. O
 * estado corrente é lido da URL A CADA REQUISIÇÃO, e não capturado num closure:
 * assim trocar de estado não precisa trocar de interceptador.
 */
function instalarInterceptador(): void {
  window.fetch = async (entrada, init) => {
    const estado = estadoDaUrl();
    const url = typeof entrada === 'string' ? entrada : entrada instanceof URL ? entrada.href : entrada.url;

    // Tudo que não é a nossa API segue para a rede: a malha, os assets do Vite.
    if (!url.includes('/api/v1/')) return fetchDeVerdade(entrada, init);

    const caminho = url.replace(/^.*\/api/, '').split('?')[0];

    // Os painéis de mercado precisam da malha para a lista de municípios do
    // crédito — ela vem do mesmo arquivo público que os mapas usam.
    if (caminho.startsWith('/v1/territorio/') && caminho !== '/v1/territorio/indicadores') {
      const malhaDosPaineis = (await (
        await fetchDeVerdade(`${import.meta.env.BASE_URL}geo/sp-municipios.json`)
      ).json()) as ColecaoMunicipal;

      const respostas = respostasDeMercado(malhaDosPaineis, estado);
      if (caminho in respostas) return envelope(respostas[caminho]);
    }

    if (caminho === '/v1/territorio/indicadores') {
      // CARREGANDO É UMA PROMESSA QUE NÃO RESOLVE — é o que a tela vê enquanto a
      // API pensa, e o único jeito honesto de capturar esse instante parado.
      if (estado === 'carregando') return new Promise<Response>(() => {});

      if (estado === 'erro') {
        return new Response(
          JSON.stringify({
            type: 'https://tracbel/erros/dependencia',
            title: 'A consulta de indicadores não respondeu (amostra do harness).',
            detail: 'Estado "erro" do harness visual: nenhuma falha real aconteceu.',
          }),
          { status: 503, headers: { 'Content-Type': 'application/problem+json' } },
        );
      }

      const malha = (await (await fetchDeVerdade(`${import.meta.env.BASE_URL}geo/sp-municipios.json`)).json()) as ColecaoMunicipal;
      return envelope(painelFicticio(malha, estado));
    }

    // QUALQUER OUTRA ROTA RESPONDE 404 EM VEZ DE IR À REDE: um endpoint novo que
    // ninguém lembrou de simular tem de aparecer como falha, e não sair pedindo
    // dado de verdade a um servidor que o harness não devia tocar.
    return new Response(
      JSON.stringify({ title: `O harness não simula ${caminho}.` }),
      { status: 404, headers: { 'Content-Type': 'application/problem+json' } },
    );
  };
}

instalarInterceptador();

export function HarnessVisual() {
  const [estado, setEstado] = useState<NomeDoEstado>(estadoDaUrl);

  // A URL MANDA, porque é ela que o Playwright abre e que se cola num chat.
  useEffect(() => {
    const aoTrocarHash = () => setEstado(estadoDaUrl());
    window.addEventListener('hashchange', aoTrocarHash);
    return () => window.removeEventListener('hashchange', aoTrocarHash);
  }, []);

  const comFicha = estado === 'municipioSelecionado' || estado === 'fichaAberta';

  // AS EVIDÊNCIAS ABERTAS SÃO O ESTADO MAIS ALTO DA PÁGINA, e é onde o layout
  // costuma estourar. Abrir à mão numa conferência é o tipo de passo manual que
  // some na pressa — aqui ele é o próprio estado.
  useEffect(() => {
    if (estado !== 'fichaAberta') return;
    const abrir = () =>
      document.querySelectorAll<HTMLDetailsElement>('details').forEach((d) => {
        d.open = true;
      });
    const id = window.setTimeout(abrir, 300);
    return () => window.clearTimeout(id);
  }, [estado]);

  const escolhido = ESTADOS.find((e) => e.id === estado)!;

  return (
    <div data-harness="mercado-visual" data-estado={estado}>
      {/* A FAIXA NÃO GRUDA NO TOPO (corrigido na T4.7).

          Ela era `position: sticky`, e numa captura de página inteira isso a
          desenha no meio da imagem, tapando justamente o pedaço da tela que se
          foi revisar. O carimbo continua no alto de toda captura — que é o que
          ele precisa fazer — sem cobrir nada. */}
      <header
        style={{
          background: '#7F1D1D',
          color: '#FFF',
          padding: '8px 16px',
          font: '600 13px/1.4 system-ui, sans-serif',
        }}
      >
        <div>
          AMOSTRA FICTÍCIA — harness visual de desenvolvimento. Nenhum número desta página é dado da Tracbel.
        </div>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginTop: 8 }}>
          {ESTADOS.map((e) => (
            <a
              key={e.id}
              href={`#/dev/mercado-visual?estado=${e.id}`}
              data-estado-link={e.id}
              style={{
                padding: '3px 8px',
                borderRadius: 4,
                textDecoration: 'none',
                background: e.id === estado ? '#FFF' : 'rgba(255,255,255,.15)',
                color: e.id === estado ? '#7F1D1D' : '#FFF',
              }}
            >
              {e.titulo}
            </a>
          ))}
        </div>
        <div style={{ marginTop: 6, fontWeight: 400, opacity: 0.9 }}>{escolhido.oQueProva}</div>
      </header>

      <ProvedorDeContextoDeAcesso>
        {/* A CHAVE REMONTA A TELA ao trocar de estado: sem ela, o dado do estado
            anterior ficaria na tela enquanto o novo carrega, que é o que
            `useRecurso` faz de propósito — e a captura sairia misturada. */}
        <MemoryRouter
          key={estado}
          initialEntries={[comFicha ? `/cobertura?${MUNICIPIO_DA_FICHA}=${3500105}` : '/cobertura']}
        >
          {/* AS MESMAS CLASSES DA ÁREA DE CONTEÚDO DO `Layout` (fidelidade às
              maquetes, 23/09/2026). Era uma `div` com 16px de folga, e a tela
              de verdade tem 32px de cada lado (24 em cima) — a captura de 1300
              mostrava 32px de conteúdo a mais do que o usuário vê numa janela
              de 1300px de conteúdo. Com as classes reais, a largura da janela do
              harness É a largura da coluna de conteúdo do aplicativo, e as
              quebras medidas aqui são as que ele vai ver. */}
          <div className="content conteudo-cadastro conteudo-largo">
            <IndicadoresGeograficos />
          </div>
        </MemoryRouter>
      </ProvedorDeContextoDeAcesso>
    </div>
  );
}
