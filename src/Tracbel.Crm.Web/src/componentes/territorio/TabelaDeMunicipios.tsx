/**
 * A tabela de municípios da ADR (issue 170, parte A; redesenhada na fidelidade
 * às maquetes, 23/09/2026 — fase 4, `territorio-ficha.png`).
 *
 * A SOMA DAS LINHAS É O TOTAL DA CONSULTA — é a conferência do documento 32: se
 * não fechar, algo sumiu ou foi contado duas vezes.
 *
 * O QUE MUDOU NA FASE 4, e por quê:
 * - AS LINHAS DE TOTAL SAÍRAM DO CORPO e foram para a dica do título (decisão 3
 *   do usuário). A conta continua a mesma e continua testada — é
 *   `conferenciaDaConsulta`, em `totaisDaAdr.ts`.
 * - PAGINAÇÃO no lugar da rolagem interna de 520px: dez linhas por vez, com a
 *   página escrita, em vez de uma lista presa num cartão.
 * - TODA COLUNA ORDENA pelo clique no cabeçalho, com `aria-sort`.
 * - A ENGRENAGEM escolhe as colunas visíveis (guardado no navegador).
 * - OS CABEÇALHOS SÃO VISÍVEIS, inclusive "Ação", e em duas linhas pequenas como
 *   na maquete: é o que faz as nove colunas caberem em ~750px, ao lado da ficha,
 *   sem rolagem lateral.
 */

import { ArrowDown, ArrowUp, ChevronRight, ChevronsUpDown, Download, MapPin, Search } from 'lucide-react';
import { useMemo, useState, type ReactNode } from 'react';
import type { IndicadoresDoMunicipio, IndicadoresForaDoMapa } from '../../tipos/territorio';
import { InfoTooltip } from '../InfoTooltip';
import { ValorAusente } from '../comum/ValorAusente';
import { ConferenciaDosTotais } from './carteira/ConferenciaDosTotais';
import { EscolhaDeColunas } from './carteira/EscolhaDeColunas';
import { PaginacaoDaTabela } from './carteira/PaginacaoDaTabela';
import { COLUNAS_OPCIONAIS, useColunasOcultas, type ColunaOpcional } from './carteira/colunas';
import { usePeriodoDaLeitura } from './carteira/periodo';
import { reaisCompactos } from './escalas';
import { MOTIVO_SEM_PARQUE, nº } from './indicadoresDaAdr';
import { conferenciaDaConsulta, type TotaisDaAdr } from './totaisDaAdr';

/** Tira acento e caixa: procurar "sao jose" acha "São José". */
const achatar = (t: string) =>
  t
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase();

/** "010101 — Catanduva" vira "Catanduva": o código é da base, o nome é o que se lê. */
const nomeDaLoja = (m: IndicadoresDoMunicipio) => m.lojaNome?.replace(/^.*—\s*/, '') ?? null;

/**
 * A LISTA NA TELA, EM CSV — o "Exportar" da maquete.
 *
 * Ele exporta O QUE ESTÁ NA TELA, incluindo o filtro de busca e a ordem: um botão
 * que baixa uma lista diferente da que se está olhando é pior do que botão
 * nenhum. A PÁGINA NÃO CORTA: a paginação é uma janela sobre a lista, e a
 * planilha é a lista.
 *
 * O "% PENDENTE" ENTROU NA PLANILHA quando saiu da linha da tabela (fase 4): o
 * número ficava embaixo de "Pendentes", e a maquete não o tem.
 *
 * Ponto e vírgula e vírgula decimal porque o Excel em português lê assim; o BOM
 * no começo é o que evita "São José" virar "SÃ£o JosÃ©" ao abrir.
 */
function baixarCsv(linhas: IndicadoresDoMunicipio[]) {
  const cabecalho = [
    'Município',
    'Código IBGE',
    'Sub-região',
    'Loja',
    'Elegíveis',
    'No prazo',
    'Pendentes',
    '% pendente',
    'Vendas (R$)',
    'Pós-venda (R$)',
    'Máquinas teóricas',
  ];

  const celula = (v: string | number | null) =>
    v === null ? '' : typeof v === 'number' ? String(v).replace('.', ',') : `"${v.replace(/"/g, '""')}"`;

  const corpo = linhas.map((m) =>
    [
      celula(m.nome),
      celula(m.codigoIbge),
      celula(m.regiao),
      celula(nomeDaLoja(m)),
      celula(m.cobertura.vinculosComCadencia),
      celula(m.cobertura.cobertos),
      celula(m.cobertura.pendentes),
      celula(m.cobertura.percentualPendente),
      celula(m.vendas.valorLiquido),
      celula(m.vendas.posVenda),
      celula(m.potencialEstrutural?.parqueDeMaquinas ?? null),
    ].join(';'),
  );

  const conteudo = `﻿${[cabecalho.join(';'), ...corpo].join('\r\n')}`;
  const url = URL.createObjectURL(new Blob([conteudo], { type: 'text/csv;charset=utf-8' }));
  const a = document.createElement('a');
  a.href = url;
  a.download = `municipios-da-adr-${new Date().toISOString().slice(0, 10)}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}

type ChaveDeOrdem = 'municipio' | ColunaOpcional;
type Ordem = { por: ChaveDeOrdem; direcao: 'asc' | 'desc' };

/** O que cada coluna compara. Nulo (sem parque) vai para o fim nas duas direções: ausência não é o menor número. */
const VALOR_DE_ORDEM: Record<ChaveDeOrdem, (m: IndicadoresDoMunicipio) => number | string | null> = {
  municipio: (m) => m.nome,
  hierarquia: (m) => `${m.regiao} ${nomeDaLoja(m) ?? ''}`,
  elegiveis: (m) => m.cobertura.vinculosComCadencia,
  noPrazo: (m) => m.cobertura.cobertos,
  pendentes: (m) => m.cobertura.pendentes,
  vendas: (m) => m.vendas.valorLiquido,
  posVenda: (m) => m.vendas.posVenda,
  maquinas: (m) => m.potencialEstrutural?.parqueDeMaquinas ?? null,
};

function ordenar(lista: IndicadoresDoMunicipio[], ordem: Ordem): IndicadoresDoMunicipio[] {
  const valor = VALOR_DE_ORDEM[ordem.por];
  const sinal = ordem.direcao === 'asc' ? 1 : -1;
  const porNome = (a: IndicadoresDoMunicipio, b: IndicadoresDoMunicipio) => a.nome.localeCompare(b.nome, 'pt-BR');

  return [...lista].sort((a, b) => {
    const va = valor(a);
    const vb = valor(b);
    if (va === null || vb === null) return va === vb ? porNome(a, b) : va === null ? 1 : -1;
    const diferenca = typeof va === 'string' ? va.localeCompare(String(vb), 'pt-BR') : va - Number(vb);
    // EMPATE SE DESFAZ PELO NOME: sem isso, dois municípios com o mesmo número
    // trocariam de lugar a cada clique, e a lista pareceria instável.
    return diferenca !== 0 ? sinal * diferenca : porNome(a, b);
  });
}

/** O padrão é o de antes da fase 4: quem vendeu mais, primeiro. */
const ORDEM_PADRAO: Ordem = { por: 'vendas', direcao: 'desc' };

/** Um cabeçalho que ordena. O nome acessível do botão é o nome da coluna; o estado está no `aria-sort`. */
function CabecalhoOrdenavel({
  chave,
  ordem,
  aoOrdenar,
  numero = false,
  semprePista = false,
  sub,
  dica,
  children,
}: {
  chave: ChaveDeOrdem;
  ordem: Ordem;
  aoOrdenar: (chave: ChaveDeOrdem) => void;
  numero?: boolean;
  /** A maquete desenha ↕ em Município e Sub-região mesmo sem ordem ativa. */
  semprePista?: boolean;
  /** A segunda linha pequena do cabeçalho — "12 meses", "(provisório)". */
  sub?: string;
  dica?: ReactNode;
  children: ReactNode;
}) {
  const ativa = ordem.por === chave;
  return (
    <th
      scope="col"
      data-coluna={chave}
      className={numero ? 'terr-coluna-numero' : undefined}
      aria-sort={ativa ? (ordem.direcao === 'asc' ? 'ascending' : 'descending') : 'none'}
    >
      <span className="terr-th">
        <button type="button" className="terr-ordenar" onClick={() => aoOrdenar(chave)}>
          {children}
          {ativa ? (
            ordem.direcao === 'asc' ? (
              <ArrowUp size={11} strokeWidth={2.4} aria-hidden="true" />
            ) : (
              <ArrowDown size={11} strokeWidth={2.4} aria-hidden="true" />
            )
          ) : semprePista ? (
            <ChevronsUpDown size={11} strokeWidth={2.4} aria-hidden="true" />
          ) : null}
        </button>
        {!sub && dica}
      </span>
      {/* COM SEGUNDA LINHA, A DICA VAI NELA — "12 meses ⓘ": na primeira, ao
          lado de "VENDAS ↓", ela quebrava sozinha numa linha do meio quando a
          tabela fica ao lado da ficha em 1.300px. */}
      {sub && (
        <span className="terr-th-sub">
          {sub}
          {dica}
        </span>
      )}
    </th>
  );
}

export function TabelaDeMunicipios({
  municipios,
  daAdr,
  foraDoMapa,
  totais,
  selecionado,
  aoSelecionar,
  territorioNaoCarregado,
  semFiltro,
}: {
  /** Todos os municípios da resposta — os da ADR e os de fora dela. */
  municipios: IndicadoresDoMunicipio[];
  /** Só os da ADR, já ordenados por venda. */
  daAdr: IndicadoresDoMunicipio[];
  foraDoMapa: IndicadoresForaDoMapa[];
  totais: TotaisDaAdr;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  territorioNaoCarregado: boolean;
  semFiltro: boolean;
}) {
  const periodo = usePeriodoDaLeitura();
  const { ocultas, alternar, mostrarTodas } = useColunasOcultas();
  const visivel = (c: ColunaOpcional) => !ocultas.includes(c);

  // A BUSCA FILTRA O QUE JÁ ESTÁ NA MÃO, e não vai à API: a resposta traz a ADR
  // inteira, então procurar um município é percorrer uma lista que já chegou.
  const [busca, setBusca] = useState('');
  const [ordem, setOrdem] = useState<Ordem>(ORDEM_PADRAO);
  const [pagina, setPagina] = useState(1);
  const [tamanho, setTamanho] = useState(10);

  const filtrados = useMemo(() => {
    const termo = achatar(busca.trim());
    if (!termo) return daAdr;
    return daAdr.filter(
      (m) => achatar(m.nome).includes(termo) || achatar(m.regiao ?? '').includes(termo) || achatar(m.lojaNome ?? '').includes(termo),
    );
  }, [daAdr, busca]);

  const ordenados = useMemo(() => ordenar(filtrados, ordem), [filtrados, ordem]);

  // O MUNICÍPIO ESCOLHIDO FORA DA TABELA APARECE NELA. Escolher pelo mapa ou pelo
  // campo do filtro abre a ficha ao lado — e a linha dele precisa estar na
  // página, marcada, e não na página 14. A troca acontece só quando a ESCOLHA
  // muda (ajuste de estado durante a renderização, como o React recomenda para
  // estado derivado de prop), e por isso não briga com quem está folheando.
  const [selecionadoVisto, setSelecionadoVisto] = useState<number | null>(null);
  if (selecionado !== selecionadoVisto) {
    setSelecionadoVisto(selecionado);
    const posicao = ordenados.findIndex((m) => m.codigoIbge === selecionado);
    if (posicao >= 0) setPagina(Math.floor(posicao / tamanho) + 1);
  }

  const totalDePaginas = Math.max(1, Math.ceil(ordenados.length / tamanho));
  const paginaAtual = Math.min(pagina, totalDePaginas);
  const naPagina = ordenados.slice((paginaAtual - 1) * tamanho, paginaAtual * tamanho);

  const conferencia = useMemo(
    () => conferenciaDaConsulta({ municipios, daAdr, foraDoMapa, totais, semFiltro, territorioNaoCarregado }),
    [municipios, daAdr, foraDoMapa, totais, semFiltro, territorioNaoCarregado],
  );

  /** Clicar na coluna ativa inverte; noutra, texto começa de A a Z e número do maior para o menor. */
  const aoOrdenar = (chave: ChaveDeOrdem) => {
    setOrdem((atual) =>
      atual.por === chave
        ? { por: chave, direcao: atual.direcao === 'asc' ? 'desc' : 'asc' }
        : { por: chave, direcao: chave === 'municipio' || chave === 'hierarquia' ? 'asc' : 'desc' },
    );
    setPagina(1);
  };

  const colunasNaTela = 2 + COLUNAS_OPCIONAIS.filter((c) => visivel(c.id)).length;
  const cabecalho = { ordem, aoOrdenar };

  return (
    <div className="card cad-cartao terr-tabela-cartao" data-bloco="tabela-municipios">
      <div className="terr-tabela-cabecalho">
        <div className="terr-tabela-cabecalho-texto">
          {/* A CONTAGEM ENTRA NO TÍTULO (maquete): "Municípios da ADR (203)". A
              dica ao lado é a conferência — os totais que eram linhas no fim. */}
          <h2 className="card-title terr-tabela-titulo">
            Municípios da ADR ({nº(daAdr.length)})
            <InfoTooltip rotulo="Os totais da ADR e da consulta" texto={<ConferenciaDosTotais conferencia={conferencia} />} />
          </h2>
          <p className="card-subtitle">
            A soma das linhas é o total da consulta. Clique em um município para ver os detalhes, desempenho e
            oportunidades.
          </p>
        </div>

        {/* A BARRA DE FERRAMENTAS DA MAQUETE — busca, exportar e a engrenagem. */}
        <div className="terr-tabela-barra">
          <label className="terr-busca">
            <Search size={15} strokeWidth={2} aria-hidden="true" />
            <input
              type="search"
              value={busca}
              onChange={(e) => {
                setBusca(e.target.value);
                setPagina(1);
              }}
              placeholder="Buscar município..."
              aria-label="Buscar município, sub-região ou loja na lista"
            />
          </label>
          <button type="button" className="btn btn-secondary terr-exportar" onClick={() => baixarCsv(ordenados)} disabled={ordenados.length === 0}>
            <Download size={14} strokeWidth={2} aria-hidden="true" />
            Exportar
          </button>
          <EscolhaDeColunas ocultas={ocultas} aoAlternar={alternar} aoMostrarTodas={mostrarTodas} />
        </div>
      </div>

      <div className="cad-tabela-wrap terr-tabela-municipios">
        <table className="cad-tabela">
          <caption className="cad-so-leitor">Indicadores por município, página {paginaAtual} de {totalDePaginas}</caption>
          <thead>
            <tr>
              <CabecalhoOrdenavel chave="municipio" semprePista {...cabecalho}>
                Município
              </CabecalhoOrdenavel>
              {/* "SUB-REGIÃO · LOJA", e não "Região · Loja" como a maquete
                  escreve (decisão 2): Norte e Noroeste são sub-regiões da Região
                  Tracbel (issue 163). */}
              {visivel('hierarquia') && (
                <CabecalhoOrdenavel chave="hierarquia" semprePista {...cabecalho}>
                  Sub-região · Loja
                </CabecalhoOrdenavel>
              )}
              {visivel('elegiveis') && (
                <CabecalhoOrdenavel chave="elegiveis" numero {...cabecalho}>
                  Elegíveis
                </CabecalhoOrdenavel>
              )}
              {visivel('noPrazo') && (
                <CabecalhoOrdenavel chave="noPrazo" numero {...cabecalho}>
                  No prazo
                </CabecalhoOrdenavel>
              )}
              {visivel('pendentes') && (
                <CabecalhoOrdenavel
                  chave="pendentes"
                  numero
                  {...cabecalho}
                  // O "38,9%" QUE FICAVA EMBAIXO DO NÚMERO saiu da linha (maquete):
                  // ele está na ficha e na planilha, e a dica diz onde.
                  dica={
                    <InfoTooltip
                      rotulo="O que conta como pendente"
                      texto="Pendentes são os vínculos elegíveis fora do prazo da cadência mais os nunca contatados. O percentual pendente de cada município está na ficha dele (aba Estrutura, em Cobertura de visita) e na planilha do Exportar."
                    />
                  }
                >
                  Pendentes
                </CabecalhoOrdenavel>
              )}
              {visivel('vendas') && (
                <CabecalhoOrdenavel
                  chave="vendas"
                  numero
                  sub={periodo?.rotulo}
                  {...cabecalho}
                  dica={
                    <InfoTooltip
                      rotulo="O que entra nas vendas"
                      texto={`Vendas líquidas no período${periodo ? ` (${periodo.intervalo})` : ''}, pelo endereço principal do cliente: máquina + peça + serviço + outros. O período é o do filtro, no alto da página.`}
                    />
                  }
                >
                  Vendas
                </CabecalhoOrdenavel>
              )}
              {visivel('posVenda') && (
                <CabecalhoOrdenavel chave="posVenda" numero sub="(provisório)" {...cabecalho}>
                  Pós-venda
                </CabecalhoOrdenavel>
              )}
              {visivel('maquinas') && (
                <CabecalhoOrdenavel chave="maquinas" numero sub="(teórico)" {...cabecalho}>
                  Máquinas
                </CabecalhoOrdenavel>
              )}
              {/* A COLUNA DE AÇÃO (maquete): o chevron que abre a ficha. O nome
                  do município continua sendo botão — esta é a segunda porta para
                  a mesma ficha, na ponta da linha, que é onde o dedo vai. */}
              <th scope="col" data-coluna="acao" className="terr-coluna-acao">
                Ação
              </th>
            </tr>
          </thead>
          <tbody>
            {naPagina.map((m) => {
              const escolhido = m.codigoIbge === selecionado;
              return (
                <tr key={m.codigoIbge} aria-current={escolhido ? 'true' : undefined}>
                  <td data-coluna="municipio">
                    <button type="button" className="terr-nome-municipio" onClick={() => aoSelecionar(m.codigoIbge)}>
                      <span className="terr-pin" aria-hidden="true">
                        <MapPin size={15} strokeWidth={2} />
                      </span>
                      {m.nome}
                    </button>
                  </td>
                  {visivel('hierarquia') && (
                    <td data-coluna="hierarquia" className="terr-coluna-hierarquia">
                      {m.regiao}
                      <div className="terr-loja">{nomeDaLoja(m) ?? '—'}</div>
                    </td>
                  )}
                  {visivel('elegiveis') && (
                    <td data-coluna="elegiveis" className="cad-mono">
                      {nº(m.cobertura.vinculosComCadencia)}
                    </td>
                  )}
                  {visivel('noPrazo') && (
                    <td data-coluna="noPrazo" className="cad-mono">
                      {nº(m.cobertura.cobertos)}
                    </td>
                  )}
                  {visivel('pendentes') && (
                    <td data-coluna="pendentes" className="cad-mono">
                      {nº(m.cobertura.pendentes)}
                    </td>
                  )}
                  {visivel('vendas') && (
                    <td data-coluna="vendas" className="cad-mono">
                      {reaisCompactos(m.vendas.valorLiquido)}
                    </td>
                  )}
                  {visivel('posVenda') && (
                    <td data-coluna="posVenda" className="cad-mono">
                      {reaisCompactos(m.vendas.posVenda)}
                    </td>
                  )}
                  {visivel('maquinas') && (
                    <td data-coluna="maquinas" className="cad-mono">
                      {m.potencialEstrutural?.parqueDeMaquinas == null ? (
                        <ValorAusente
                          motivo={
                            m.potencialEstrutural
                              ? MOTIVO_SEM_PARQUE[m.potencialEstrutural.motivoSemParque]
                              : 'nenhuma regra de potencial vigente alcança este município'
                          }
                          oQue={`o parque de ${m.nome}`}
                        />
                      ) : (
                        nº(m.potencialEstrutural.parqueDeMaquinas)
                      )}
                    </td>
                  )}
                  <td data-coluna="acao" className="terr-coluna-acao">
                    <button
                      type="button"
                      className="terr-abrir-ficha"
                      onClick={() => aoSelecionar(m.codigoIbge)}
                      aria-label={`Abrir a ficha de ${m.nome}`}
                    >
                      <ChevronRight size={15} strokeWidth={2.4} aria-hidden="true" />
                    </button>
                  </td>
                </tr>
              );
            })}

            {/* NENHUM MUNICÍPIO NÃO É TABELA VAZIA SEM EXPLICAÇÃO. */}
            {naPagina.length === 0 && (
              <tr>
                <td colSpan={colunasNaTela} className="terr-tabela-vazia">
                  {territorioNaoCarregado
                    ? 'Território não carregado neste banco: nenhum município marcado como ADR. O que a consulta encontrou está na dica do título.'
                    : busca.trim()
                      ? 'Nenhum município da ADR tem esse nome, sub-região ou loja.'
                      : 'Nenhum município da ADR neste recorte.'}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <PaginacaoDaTabela
        total={ordenados.length}
        deQuantos={daAdr.length}
        pagina={paginaAtual}
        tamanho={tamanho}
        oQue="municípios"
        aoTrocarPagina={setPagina}
        aoTrocarTamanho={(t) => {
          setTamanho(t);
          setPagina(1);
        }}
      />
    </div>
  );
}
