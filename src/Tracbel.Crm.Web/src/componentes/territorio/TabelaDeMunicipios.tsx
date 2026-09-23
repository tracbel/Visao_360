/**
 * A tabela de municípios da ADR e o que ficou fora do mapa (issue 170, parte A).
 *
 * A SOMA DAS LINHAS É O TOTAL DA CONSULTA — é a conferência do documento 32: se
 * não fechar, algo sumiu ou foi contado duas vezes.
 */

import { ChevronRight, Download, MapPin, Search } from 'lucide-react';
import { useMemo, useState } from 'react';
import type { IndicadoresDoMunicipio, IndicadoresForaDoMapa } from '../../tipos/territorio';
import { ValorAusente } from '../comum/ValorAusente';
import { reaisCompactos } from './escalas';
import { MOTIVO_SEM_PARQUE, nº } from './indicadoresDaAdr';
import type { TotaisDaAdr } from './totaisDaAdr';

/** Tira acento e caixa: procurar "sao jose" acha "São José". */
const achatar = (t: string) =>
  t
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase();

/**
 * A LISTA NA TELA, EM CSV — o "Exportar" da maquete.
 *
 * Ele exporta O QUE ESTÁ NA TELA, incluindo o filtro de busca: um botão que
 * baixa uma lista diferente da que se está olhando é pior do que botão nenhum.
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
      celula(m.lojaNome?.replace(/^.*—\s*/, '') ?? null),
      celula(m.cobertura.vinculosComCadencia),
      celula(m.cobertura.cobertos),
      celula(m.cobertura.pendentes),
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
  // A BUSCA FILTRA O QUE JÁ ESTÁ NA MÃO, e não vai à API: a resposta traz a ADR
  // inteira, então procurar um município é percorrer uma lista que já chegou.
  // Uma busca que fosse ao servidor a cada tecla trocaria uma operação instantânea
  // por uma com latência, e ainda perderia a conferência das somas.
  const [busca, setBusca] = useState('');

  const filtrados = useMemo(() => {
    const termo = achatar(busca.trim());
    if (!termo) return daAdr;
    return daAdr.filter(
      (m) => achatar(m.nome).includes(termo) || achatar(m.regiao ?? '').includes(termo) || achatar(m.lojaNome ?? '').includes(termo),
    );
  }, [daAdr, busca]);

  // AS LINHAS DE TOTAL SÓ VALEM SOBRE A LISTA INTEIRA. Com a busca ativa elas
  // saem, porque "Total da ADR" embaixo de doze municípios afirmaria que a ADR
  // tem doze — e a conferência do documento 32 é justamente a soma fechar.
  const buscando = filtrados.length !== daAdr.length;

  return (
    <div className="card cad-cartao" data-bloco="tabela-municipios">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          {/* A CONTAGEM ENTRA NO TÍTULO (maquete): "Municípios da ADR (203)". */}
          <div className="card-title">Municípios da ADR ({nº(daAdr.length)})</div>
          <div className="card-subtitle">
            A soma das linhas é o total da consulta. Clique num município para abrir o detalhe.
          </div>
        </div>

        {/* A BARRA DE FERRAMENTAS DA MAQUETE — busca e exportar, à direita do
            título. A engrenagem de escolher colunas não veio junto: ela é um
            painel de preferências por usuário, e não acabamento visual. */}
        <div className="terr-tabela-barra">
          <label className="terr-busca">
            <Search size={15} strokeWidth={2} aria-hidden="true" />
            <input
              type="search"
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="Buscar município..."
              aria-label="Buscar município, sub-região ou loja na lista"
            />
          </label>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => baixarCsv(filtrados)}
            disabled={filtrados.length === 0}
          >
            <Download size={14} strokeWidth={2} aria-hidden="true" />
            Exportar
          </button>
        </div>
      </div>

      {/* O QUE A BUSCA FEZ, ESCRITO. Sem esta linha, uma lista filtrada é
          indistinguível de uma ADR que encolheu. */}
      {buscando && (
        <p className="terr-tabela-contagem" role="status">
          {nº(filtrados.length)} de {nº(daAdr.length)} municípios · os totais voltam quando a busca for limpa.
        </p>
      )}
      <div className="cad-tabela-wrap terr-tabela-municipios">
        <table className="cad-tabela">
          <caption className="cad-so-leitor">Indicadores por município</caption>
          <thead>
            <tr>
              <th scope="col">Município</th>
              <th scope="col" className="terr-coluna-hierarquia">Região · loja</th>
              {/* AS COLUNAS OPCIONAIS SAEM NO CELULAR (T4.6, corrigido na T4.7).

                  Oito colunas comprimidas em 390px não são uma tabela, são um
                  borrão — e rolagem lateral da página está proibida.

                  A T4.6 tirou três e deixou cinco, e a REVISÃO DAS CAPTURAS
                  mostrou que cinco também não cabem: em 390px a tabela virava
                  uma lista de uma coluna, com as outras quatro escondidas atrás
                  de rolagem horizontal dentro do cartão. O meu próprio teste
                  passou porque contava `:visible`, que em Playwright quer dizer
                  "não está `display:none`" — e não "cabe na tela".

                  Abaixo de 560px ficam DUAS: o município e as vendas. É o que
                  responde "onde vender" num aparelho de mão; o resto está
                  inteiro na ficha do município, a um toque. */}
              <th scope="col" className="terr-coluna-numero">Elegíveis</th>
              <th scope="col" className="terr-coluna-numero">No prazo</th>
              <th scope="col" className="terr-coluna-numero">Pendentes</th>
              <th scope="col" className="terr-coluna-numero">Vendas</th>
              <th scope="col" className="terr-coluna-numero">
                Pós-venda <span className="cad-sub">(provisório)</span>
              </th>
              <th scope="col" className="terr-coluna-numero">Máquinas teóricas</th>
              {/* A COLUNA DE AÇÃO (maquete): o chevron que abre a ficha. O nome
                  do município continua sendo botão — esta é a segunda porta para
                  a mesma ficha, na ponta da linha, que é onde o dedo vai. */}
              <th scope="col" className="terr-coluna-acao">
                <span className="cad-so-leitor">Ação</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {filtrados.map((m) => (
              <tr key={m.codigoIbge} aria-current={m.codigoIbge === selecionado ? 'true' : undefined}>
                <td>
                  <button type="button" className="cad-th-ordenar" onClick={() => aoSelecionar(m.codigoIbge)}>
                    <span className="terr-pin" aria-hidden="true">
                      <MapPin size={13} strokeWidth={2} />
                    </span>
                    {m.nome}
                  </button>
                </td>
                <td className="terr-coluna-hierarquia">
                  {m.regiao}
                  <div className="cad-sub">{m.lojaNome?.replace(/^.*—\s*/, '') ?? '—'}</div>
                </td>
                <td className="cad-mono">{nº(m.cobertura.vinculosComCadencia)}</td>
                <td className="cad-mono">{nº(m.cobertura.cobertos)}</td>
                <td className="cad-mono">
                  {nº(m.cobertura.pendentes)}
                  {m.cobertura.percentualPendente !== null && (
                    <div className="cad-sub">{m.cobertura.percentualPendente.toLocaleString('pt-BR')}%</div>
                  )}
                </td>
                <td className="cad-mono">{reaisCompactos(m.vendas.valorLiquido)}</td>
                <td className="cad-mono">{reaisCompactos(m.vendas.posVenda)}</td>
                <td className="cad-mono">
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
                <td className="terr-coluna-acao">
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
            ))}

            {/* NENHUM MUNICÍPIO ACHADO NÃO É TABELA VAZIA SEM EXPLICAÇÃO. */}
            {filtrados.length === 0 && (
              <tr>
                <td colSpan={9}>Nenhum município da ADR tem esse nome, sub-região ou loja.</td>
              </tr>
            )}

            {/* OS TOTAIS SOMEM ENQUANTO A BUSCA ESTÁ ATIVA: "Total da ADR"
                embaixo de doze linhas afirmaria que a ADR tem doze municípios, e
                a conferência do documento 32 é justamente a soma fechar. */}
            {buscando ? null : territorioNaoCarregado ? (
              <tr className="terr-linha-total">
                <td>Total da ADR</td>
                <td colSpan={8}>território não carregado neste banco — as linhas abaixo são o que a consulta encontrou</td>
              </tr>
            ) : (
              <tr className="terr-linha-total">
                <td>Total da ADR {semFiltro ? '' : '(filtro)'}</td>
                <td>{daAdr.length} municípios</td>
                <td className="cad-mono">{nº(totais.elegiveis)}</td>
                <td className="cad-mono">{nº(totais.cobertos)}</td>
                <td className="cad-mono">{nº(totais.pendentes)}</td>
                <td className="cad-mono">{reaisCompactos(totais.vendas)}</td>
                <td className="cad-mono">{reaisCompactos(totais.posVenda)}</td>
                <td className="cad-mono">{nº(Math.round(totais.maquinasTeoricas))}</td>
                <td className="terr-coluna-acao" />
              </tr>
            )}
            {!buscando && semFiltro && !territorioNaoCarregado && (
              <LinhaDeGrupo
                rotulo="São Paulo fora da ADR"
                descricao="municípios com cliente fora da ADR"
                itens={municipios.filter((m) => !m.pertenceAAdr)}
              />
            )}
            {!buscando &&
              foraDoMapa.map((g) => (
                <tr key={g.grupo}>
                  <td>
                    {g.grupo}
                    <div className="cad-sub">{g.descricao}</div>
                  </td>
                  <td>—</td>
                  <td className="cad-mono">{nº(g.cobertura.vinculosComCadencia)}</td>
                  <td className="cad-mono">{nº(g.cobertura.cobertos)}</td>
                  <td className="cad-mono">{nº(g.cobertura.pendentes)}</td>
                  <td className="cad-mono">{reaisCompactos(g.vendas.valorLiquido)}</td>
                  <td className="cad-mono">{reaisCompactos(g.vendas.posVenda)}</td>
                  <td>—</td>
                  <td className="terr-coluna-acao" />
                </tr>
              ))}
            {!buscando && semFiltro && <LinhaDeTotal municipios={municipios} foraDoMapa={foraDoMapa} />}
          </tbody>
        </table>
      </div>
    </div>
  );
}

/** Uma linha que soma um grupo de municípios. */
function LinhaDeGrupo({ rotulo, descricao, itens }: { rotulo: string; descricao: string; itens: IndicadoresDoMunicipio[] }) {
  const soma = (f: (m: IndicadoresDoMunicipio) => number) => itens.reduce((s, m) => s + f(m), 0);
  return (
    <tr>
      <td>
        {rotulo}
        <div className="cad-sub">
          {descricao} ({itens.length})
        </div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.cobertos))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.posVenda))}</td>
      <td>—</td>
      <td className="terr-coluna-acao" />
    </tr>
  );
}

/**
 * O total da consulta: municípios do mapa mais o que ficou fora dele. É o número
 * que a conferência SQL do documento 32 compara — se não fechar, algo sumiu ou
 * foi contado duas vezes.
 */
function LinhaDeTotal({
  municipios,
  foraDoMapa,
}: {
  municipios: IndicadoresDoMunicipio[];
  foraDoMapa: { cobertura: { vinculosComCadencia: number; cobertos: number; pendentes: number }; vendas: { valorLiquido: number; posVenda: number } }[];
}) {
  const somaMapa = (f: (m: IndicadoresDoMunicipio) => number) => municipios.reduce((s, m) => s + f(m), 0);
  const somaFora = (f: (g: (typeof foraDoMapa)[number]) => number) => foraDoMapa.reduce((s, g) => s + f(g), 0);
  return (
    <tr className="terr-linha-total">
      <td>
        Total da consulta
        <div className="cad-sub">municípios do mapa + tudo o que ficou fora dele</div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.vinculosComCadencia) + somaFora((g) => g.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.cobertos) + somaFora((g) => g.cobertura.cobertos))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.pendentes) + somaFora((g) => g.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.valorLiquido) + somaFora((g) => g.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.posVenda) + somaFora((g) => g.vendas.posVenda))}</td>
      <td>—</td>
      <td className="terr-coluna-acao" />
    </tr>
  );
}
