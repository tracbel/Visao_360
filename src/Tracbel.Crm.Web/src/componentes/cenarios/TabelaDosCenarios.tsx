/**
 * O PLANEJAMENTO POR MUNICÍPIO (issue 263) — a tabela da aba "Cenários de Mercado" do protótipo, com a escolha gravada.
 *
 * CADA LINHA: o município e a loja; o momento (o efeito do preço e do crédito no mercado dele, o fator de ciclo); o realizado
 * do ano fiscal, com o intervalo e o ano anterior embaixo; a média dos quatro anos fechados; os clientes; o potencial com o
 * mercado ajustado; a meta estrutural; o share estrutural e o ajustado; o que entregar; os botões C / M / O e o número
 * manual; e a situação — o enquadramento do manual, ou a recomendação.
 *
 * A ESCOLHA É GRAVADA NA HORA (PUT por linha), e o número dos três cenários é calculado no servidor. Enquanto ninguém
 * escolheu, vale o moderado — o botão M aparece tracejado, "padrão, não gravado". Quem não pode gravar vê os botões
 * desligados, com o motivo na dica.
 */

import { useMemo, useRef, useState } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { CabecalhoOrdenavel } from '../comum/CabecalhoOrdenavel';
import { ordenar, proximaOrdem, type Ordem } from '../comum/ordenacao';
import { nomeProprio } from '../../telas/cadastro/formato';
import { ErroDaApi } from '../../dados/api/http';
import type { CenarioDeMercado, CenarioDoMunicipio, EscolhaDoCenario } from '../../tipos/mercado';
import { aEntregarDe, comSinal, enquadramento, metaDoCenario, n, NOME_DO_CENARIO, numeroOuTraco, recomendacao, setaDoEfeito } from './cenarios';

type Coluna = 'nome' | 'realizado' | 'media' | 'clientes' | 'potencial' | 'meta' | 'share' | 'aEntregar';

const BOTOES: readonly { cenario: CenarioDeMercado; letra: string }[] = [
  { cenario: 'Conservador', letra: 'C' },
  { cenario: 'Moderado', letra: 'M' },
  { cenario: 'Otimista', letra: 'O' },
];

export type AoEscolher = (codigoIbge: number, cenario: CenarioDeMercado, valorManual?: string) => Promise<void>;

export function TabelaDosCenarios({
  municipios,
  escolhas,
  busca,
  anoFiscal,
  shareAlvo,
  podeGravar,
  porQueNaoGrava,
  aoEscolher,
}: {
  municipios: readonly CenarioDoMunicipio[];
  /** As escolhas gravadas nesta visita, por cima das que vieram na leitura. */
  escolhas: ReadonlyMap<number, EscolhaDoCenario | null>;
  busca: string;
  anoFiscal: number;
  shareAlvo: number | null;
  podeGravar: boolean;
  porQueNaoGrava: string | null;
  aoEscolher: AoEscolher;
}) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'aEntregar', sentido: -1 });
  const [tamanho, setTamanho] = useState(25);
  const [gravando, setGravando] = useState<ReadonlySet<number>>(() => new Set());
  const [erros, setErros] = useState<ReadonlyMap<number, string>>(() => new Map());
  const [rascunhos, setRascunhos] = useState<ReadonlyMap<number, string>>(() => new Map());
  // A TRAVA DO ENVIO, fora do estado: o Enter tira o foco e o blur grava — um segundo blur antes da próxima renderização
  // ainda veria a linha livre e mandaria de novo.
  const emVoo = useRef(new Set<number>());

  const escolhaDe = (m: CenarioDoMunicipio) => (escolhas.has(m.codigoIbge) ? (escolhas.get(m.codigoIbge) ?? null) : m.escolha);

  const linhas = useMemo(() => {
    const termo = busca.trim().toLocaleLowerCase('pt-BR');
    const filtradas = termo
      ? municipios.filter((m) => m.nome.toLocaleLowerCase('pt-BR').includes(termo) || (m.loja ?? '').toLocaleLowerCase('pt-BR').includes(termo))
      : municipios;
    const valorDe = (m: CenarioDoMunicipio, coluna: Coluna): number | string | null => {
      switch (coluna) {
        case 'nome': return m.nome;
        case 'realizado': return m.realizadoNoAno;
        case 'media': return m.mediaDosAnosAnteriores;
        case 'clientes': return m.clientes;
        case 'potencial': return m.potencial;
        case 'meta': return m.metaEstrutural;
        case 'share': return m.shareEstrutural;
        case 'aEntregar': return aEntregarDe(m, escolhas.has(m.codigoIbge) ? (escolhas.get(m.codigoIbge) ?? null) : m.escolha);
      }
    };
    return ordenar(filtradas, ordem, valorDe);
  }, [municipios, busca, ordem, escolhas]);

  // A PÁGINA VOLTA PARA A PRIMEIRA quando a lista ou a ordem muda — derivado na renderização, como nas outras tabelas.
  const chave = `${linhas.length}|${ordem.coluna}|${ordem.sentido}`;
  const [pagina, setPaginaEscolhida] = useState({ chave, pagina: 1 });
  const atualDaChave = pagina.chave === chave ? pagina.pagina : 1;
  const setPagina = (p: number) => setPaginaEscolhida({ chave, pagina: p });
  const totalDePaginas = Math.max(1, Math.ceil(linhas.length / tamanho));
  const atual = Math.min(atualDaChave, totalDePaginas);
  const daPagina = linhas.slice((atual - 1) * tamanho, atual * tamanho);
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };

  async function escolher(codigoIbge: number, cenario: CenarioDeMercado, valorManual?: string) {
    if (emVoo.current.has(codigoIbge)) return;
    emVoo.current.add(codigoIbge);
    setGravando((g) => new Set(g).add(codigoIbge));
    setErros((e) => {
      const proximo = new Map(e);
      proximo.delete(codigoIbge);
      return proximo;
    });
    try {
      await aoEscolher(codigoIbge, cenario, valorManual);
      setRascunhos((r) => {
        const proximo = new Map(r);
        proximo.delete(codigoIbge);
        return proximo;
      });
    } catch (erro) {
      const mensagem =
        erro instanceof ErroDaApi ? (erro.erros[0]?.mensagem ?? erro.message) : erro instanceof Error ? erro.message : 'A meta não foi gravada.';
      setErros((e) => new Map(e).set(codigoIbge, mensagem));
    } finally {
      emVoo.current.delete(codigoIbge);
      setGravando((g) => {
        const proximo = new Set(g);
        proximo.delete(codigoIbge);
        return proximo;
      });
    }
  }

  if (municipios.length === 0) return <p className="cen-nota">Nenhum município do recorte tem potencial, realizado ou meta nesta categoria.</p>;

  const share = shareAlvo === null ? 'share-alvo' : `${n(shareAlvo, 0)}%`;

  return (
    <>
      <div className="mom-tabela-rolagem">
        <table className="mom-tabela diag-tabela cen-tabela">
          <caption className="cad-so-leitor">O planejamento da meta de cada município no FY{anoFiscal}</caption>
          <thead>
            <tr>
              <CabecalhoOrdenavel {...cab} coluna="nome" rotulo="Município" texto />
              <th scope="col" title="O quanto o preço das culturas e o crédito do município movem o mercado dele (o fator de ciclo)">
                Momento
              </th>
              <CabecalhoOrdenavel {...cab} coluna="realizado" rotulo={`Realizado FY${anoFiscal}`} titulo="Máquinas entregues no ano fiscal (ART, pela data da entrega)" />
              <CabecalhoOrdenavel {...cab} coluna="media" rotulo="Média 4 anos" titulo="A média por ano dos quatro anos fiscais fechados antes do escolhido" />
              <CabecalhoOrdenavel {...cab} coluna="clientes" rotulo="Clientes" titulo="Clientes distintos que receberam máquina no ano fiscal" />
              <CabecalhoOrdenavel {...cab} coluna="potencial" rotulo="Potencial" titulo="A demanda estrutural (100% do mercado); embaixo, o mercado ajustado pelo momento" />
              <CabecalhoOrdenavel {...cab} coluna="meta" rotulo={`Meta ${share}`} titulo="Potencial × share-alvo: o que entregar para atingir o share no potencial" />
              <CabecalhoOrdenavel {...cab} coluna="share" rotulo="Share estr. / ajust." titulo="Realizado ÷ potencial e realizado ÷ mercado ajustado — não é share de mercado, pode passar de 100%" />
              <CabecalhoOrdenavel {...cab} coluna="aEntregar" rotulo="A entregar" titulo="A meta combinada, ou o moderado enquanto ninguém escolheu" />
              <th scope="col" title="Conservador (−5%), Moderado ou Otimista (+5%) sobre o mercado ajustado × share, ou um número digitado">
                Cenário
              </th>
              <th scope="col" className="cen-col-situacao">Situação / recomendação</th>
            </tr>
          </thead>
          <tbody>
            {daPagina.map((m) => {
              const escolha = escolhaDe(m);
              const ocupado = gravando.has(m.codigoIbge);
              const bloqueio = !podeGravar ? (porQueNaoGrava ?? 'Você não grava metas.') : !m.gravavel ? 'Este município é de uma filial fora do seu alcance.' : null;
              const enq = enquadramento(m, escolha);
              const rec = recomendacao(m);
              const preco = setaDoEfeito(m.efeitoDoPreco);
              const credito = setaDoEfeito(m.efeitoDoCredito);
              const manualAtual = escolha?.cenario === 'Manual' ? String(escolha.metaCombinada).replace('.', ',') : '';
              const rascunho = rascunhos.get(m.codigoIbge) ?? manualAtual;
              const aEntregar = aEntregarDe(m, escolha);
              const hoje = escolha && escolha.cenario !== 'Manual' ? metaDoCenario(m, escolha.cenario) : null;
              const mudouDesdeAEscolha = hoje !== null && escolha !== null && Math.abs(hoje - escolha.metaCombinada) >= 0.05;

              const gravarManual = () => {
                const texto = rascunho.trim();
                if (texto === '' || texto === manualAtual) return;
                void escolher(m.codigoIbge, 'Manual', texto);
              };

              return (
                <tr key={m.codigoIbge} data-municipio={m.codigoIbge} aria-busy={ocupado || undefined}>
                  <th scope="row">
                    <span className="cen-municipio">{m.nome}</span>
                    <div className="cad-sub">
                      {m.loja ? lojaCurta(m.loja) : 'Sem loja responsável'}
                      {m.culturaPrincipal && <> · {m.culturaPrincipal.toLocaleLowerCase('pt-BR')}</>}
                    </div>
                  </th>
                  <td className="cen-momento">
                    <span className={`cen-efeito ${preco.sentido}`} title="O efeito do preço das culturas no mercado do município">
                      Preço {preco.seta} {comSinal(m.efeitoDoPreco)}
                    </span>
                    <span className={`cen-efeito ${credito.sentido}`} title="O efeito do crédito rural do município no mercado dele">
                      Crédito {credito.seta} {comSinal(m.efeitoDoCredito)}
                    </span>
                  </td>
                  <td className="mom-num">
                    <strong>{numeroOuTraco(m.realizadoNoAno, 0)}</strong>
                    <div className="cad-sub" title="No intervalo escolhido e no ano fiscal anterior inteiro">
                      interv. {numeroOuTraco(m.realizadoNoIntervalo, 0)} · FY{anoFiscal - 1}: {numeroOuTraco(m.realizadoNoAnoAnterior, 0)}
                    </div>
                  </td>
                  <td className="mom-num">{numeroOuTraco(m.mediaDosAnosAnteriores)}</td>
                  <td className="mom-num">{numeroOuTraco(m.clientes, 0)}</td>
                  <td className="mom-num">
                    {numeroOuTraco(m.potencial)}
                    <div className="cad-sub">{m.mercadoAjustado === null ? 'sem ajuste' : `ajust. ${n(m.mercadoAjustado)}`}</div>
                  </td>
                  <td className="mom-num cen-meta">{numeroOuTraco(m.metaEstrutural)}</td>
                  <td className="mom-num">
                    <span className={`cen-share ${tomDoShare(m.shareEstrutural)}`}>{m.shareEstrutural === null ? '—' : `${n(m.shareEstrutural, 0)}%`}</span>
                    <span className="cen-share-ajustado"> / {m.shareAjustado === null ? '—' : `${n(m.shareAjustado, 0)}%`}</span>
                  </td>
                  <td className="mom-num cen-a-entregar">
                    <strong>{numeroOuTraco(aEntregar)}</strong>
                    {mudouDesdeAEscolha && <div className="cad-sub" title="O mesmo cenário com o mercado de hoje">hoje: {n(hoje!)}</div>}
                    {!m.baseAjustada && m.moderado !== null && <div className="cad-sub" title="O fator de ciclo não tem dado: os cenários partem da meta estrutural">sem ajuste</div>}
                  </td>
                  <td className="cen-escolha">
                    <div className="cen-botoes" role="group" aria-label={`Cenário de ${m.nome}`}>
                      {BOTOES.map((b) => {
                        const ativo = escolha?.cenario === b.cenario;
                        const padrao = !escolha && b.cenario === 'Moderado';
                        const valor = metaDoCenario(m, b.cenario);
                        return (
                          <button
                            key={b.cenario}
                            type="button"
                            className={`cen-botao ${ativo ? 'ativo' : ''} ${padrao ? 'padrao' : ''}`}
                            aria-pressed={ativo}
                            disabled={bloqueio !== null || ocupado || valor === null}
                            title={
                              bloqueio ??
                              (valor === null
                                ? 'Sem mercado calculado nesta categoria: use o número manual.'
                                : `${NOME_DO_CENARIO[b.cenario]}: ${n(valor)} máquinas${padrao ? ' — o padrão, ainda não gravado' : ''}`)
                            }
                            onClick={() => void escolher(m.codigoIbge, b.cenario)}
                          >
                            {b.letra}
                          </button>
                        );
                      })}
                      <input
                        type="text"
                        inputMode="decimal"
                        className={`cen-manual ${escolha?.cenario === 'Manual' ? 'ativo' : ''}`}
                        placeholder="man."
                        aria-label={`Meta manual de ${m.nome}, em máquinas`}
                        title={bloqueio ?? 'Digite a meta e tecle Enter para gravar'}
                        disabled={bloqueio !== null || ocupado}
                        value={rascunho}
                        onChange={(e) => setRascunhos((r) => new Map(r).set(m.codigoIbge, e.target.value))}
                        onBlur={gravarManual}
                        onKeyDown={(e) => {
                          // O ENTER SÓ TIRA O FOCO: quem grava é o blur, uma vez só (gravar nos dois mandaria duas vezes).
                          if (e.key === 'Enter') e.currentTarget.blur();
                          if (e.key === 'Escape')
                            setRascunhos((r) => {
                              const proximo = new Map(r);
                              proximo.delete(m.codigoIbge);
                              return proximo;
                            });
                        }}
                      />
                    </div>
                    {escolha?.gravadaPor && (
                      <div className="cad-sub" title={`Gravada em ${new Date(escolha.gravadaEm).toLocaleString('pt-BR')}`}>
                        por {escolha.gravadaPor}
                      </div>
                    )}
                    {erros.has(m.codigoIbge) && (
                      <div className="cen-erro" role="alert">
                        {erros.get(m.codigoIbge)}
                      </div>
                    )}
                  </td>
                  <td className="cen-situacao">
                    {enq ? (
                      <span className={`cen-enquadramento ${enq.doModerado >= 0 ? 'sobe' : 'desce'}`}>
                        <strong>{enq.nome}</strong> ({comSinal(enq.doModerado, 0)} do moderado)
                      </span>
                    ) : rec ? (
                      <span className={`cen-recomendacao ${rec.tom}`} title={rec.texto}>
                        {rec.linhas ? (
                          <>
                            <strong>Sugerido</strong> {rec.linhas[0]}
                            <span className="cen-kpi-linha">{rec.linhas[1]}</span>
                          </>
                        ) : (
                          rec.texto
                        )}
                      </span>
                    ) : (
                      <span className="cad-sub">sem realizado para comparar</span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      <BarraDePaginacao
        pagina={{ itens: daPagina, pagina: atual, tamanho, total: linhas.length, totalDePaginas, temProxima: atual < totalDePaginas }}
        oQue="municípios"
        aoTrocarPagina={setPagina}
        aoTrocarTamanho={(t) => {
          setTamanho(t);
          setPagina(1);
        }}
      />
    </>
  );
}

/** "TRACBEL AGRO — RIBEIRÃO PRETO" vira "Ribeirão Preto": a empresa é a mesma em toda linha, e a coluna precisa da largura. */
function lojaCurta(loja: string): string {
  return nomeProprio(loja).replace(/^Tracbel Agro\s*[—–-]\s*/i, '');
}

/** A cor do share estrutural, como no protótipo: metade ou mais do potencial é verde; algum, amarelo; nada, vermelho. */
function tomDoShare(share: number | null): string {
  if (share === null) return '';
  return share >= 50 ? 'alto' : share > 0 ? 'medio' : 'zero';
}
