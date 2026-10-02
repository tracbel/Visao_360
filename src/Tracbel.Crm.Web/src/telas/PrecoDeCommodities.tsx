/**
 * Preço de Commodities — a evolução do preço de cada cultura e o momento dele (alta ou queda) em quatro horizontes, para
 * orientar o timing comercial (issue 260, a aba "Preço de Commodities" do protótipo da pasta 360).
 *
 * O DESENHO É O DAS TELAS DE MERCADO: filtros na linha (cultura, horizonte e janela da série), os quatro números da cultura
 * escolhida, e duas seções — o momento de todas as culturas, horizonte a horizonte, e a série histórica da escolhida.
 *
 * O R12 É O MOMENTO DOS INDICADORES: o mesmo número que entra no fator de ciclo da demanda, inclusive o preço anual da PAM
 * enquanto a série mensal não fecha 24 meses. As faixas são as do CRM, e não os cortes de ±10% e ±20% do protótipo.
 */

import { CalendarRange, Coins, History, RefreshCw, Sprout, TrendingUp } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { GraficoLinhaMensal } from '../componentes/GraficoLinhaMensal';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { MolduraDeGrafico } from '../componentes/MolduraDeGrafico';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { mesCurto } from '../componentes/mercado/momento/formatos';
import { GraficoSemSerie, PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { MomentoPorCultura } from '../componentes/precos/MomentoPorCultura';
import { HORIZONTES, horizonteDe, motivoDoHorizonte, NOME_DA_FAIXA, preco, sentido, variacao, type Horizonte } from '../componentes/precos/precos';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterPrecosDasCulturas } from '../dados/api/mercado';
import { useRecurso } from '../dados/api/useRecurso';
import type { PrecosDasCulturas } from '../tipos/mercado';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';
import '../estilos/precos.css';

const JANELAS = [
  { id: 12, rotulo: '12 meses' },
  { id: 24, rotulo: '24 meses' },
  { id: 36, rotulo: '36 meses' },
  { id: 0, rotulo: 'Tudo' },
] as const;

export function PrecoDeCommodities() {
  const { contexto } = useContextoDeAcesso();
  const [codigo, setCodigo] = useState<string | null>(null);
  const [horizonte, setHorizonte] = useState<Horizonte>(12);
  const [janela, setJanela] = useState<number>(24);

  const leitura = useRecurso((sinal) => obterPrecosDasCulturas(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const dados = leitura.dados;
  const comSerie = dados?.culturas.filter((c) => c.serie.length > 0) ?? [];
  const cultura = dados?.culturas.find((c) => c.codigo === codigo) ?? comSerie[0] ?? dados?.culturas[0] ?? null;
  const doHorizonte = cultura ? horizonteDe(cultura, horizonte)?.indice : undefined;
  const doCiclo = cultura ? horizonteDe(cultura, 12)?.indice : undefined;
  const r3 = cultura ? horizonteDe(cultura, 3)?.indice : undefined;
  const extenso = HORIZONTES.find((h) => h.id === horizonte)!.extenso;
  const serie = cultura ? (janela > 0 ? cultura.serie.slice(-janela) : cultura.serie) : [];

  return (
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Preço de Commodities</h1>
          <p className="page-subtitle">
            A evolução do preço de cada cultura e o momento dele — alta ou queda —, para orientar o timing comercial.
          </p>
        </div>
        <p className="dash-atualizado">
          {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} /> : 'Lendo os preços…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={leitura.recarregar}
            disabled={leitura.carregando}
            data-carregando={leitura.carregando ? 'true' : 'false'}
            aria-label="Reler os preços"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="cultura">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Sprout size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Cultura</span>
              <select value={cultura?.codigo ?? ''} onChange={(e) => setCodigo(e.target.value)} disabled={!dados}>
                {(dados?.culturas ?? []).map((c) => (
                  <option key={c.codigo} value={c.codigo}>
                    {c.nome}
                    {c.serie.length === 0 ? ' (sem série)' : ''}
                  </option>
                ))}
              </select>
            </span>
          </label>
          <label className="dash-filtro" data-bloco="horizonte">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarRange size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Horizonte de comparação
                <InfoTooltip
                  rotulo="Os horizontes"
                  texto="A média dos últimos N meses sobre a dos N anteriores: o mês (1 contra 1), o trimestre (R3), o semestre (R6) e o ciclo (R12). O R12 é o que entra no fator de ciclo da demanda."
                />
              </span>
              <select value={horizonte} onChange={(e) => setHorizonte(Number(e.target.value) as Horizonte)}>
                {HORIZONTES.map((h) => (
                  <option key={h.id} value={h.id}>
                    {h.rotulo}
                  </option>
                ))}
              </select>
            </span>
          </label>
          <label className="dash-filtro" data-bloco="janela">
            <span className="dash-filtro-icone" aria-hidden="true">
              <History size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Série histórica</span>
              <select value={janela} onChange={(e) => setJanela(Number(e.target.value))}>
                {JANELAS.map((j) => (
                  <option key={j.id} value={j.id}>
                    {j.rotulo}
                  </option>
                ))}
              </select>
            </span>
          </label>
        </div>
      </div>

      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo={`Preço médio atual — ${cultura?.nome ?? 'cultura'}`}
          icone={Coins}
          tom="mercado"
          valor={doHorizonte?.mediaRecente != null ? preco(doHorizonte.mediaRecente, cultura!.unidade) : null}
          carregando={leitura.carregando && !dados}
          variacao={cultura ? `média de ${doHorizonte?.mesesRecentes ?? horizonte} ${horizonte === 1 ? 'mês' : 'meses'} · ${cultura.fonte}` : null}
          motivoSemDado={doHorizonte ? motivoDoHorizonte(horizonte, doHorizonte.motivo) : 'A cultura não tem série de preço carregada.'}
          sobre="A média dos meses mais recentes da série da cultura, no horizonte escolhido, na unidade em que a fonte publica."
        />
        <CartaoDeDecisao
          rotulo="Preço comparado"
          icone={History}
          tom="neutro"
          valor={doHorizonte?.mediaAnterior != null ? preco(doHorizonte.mediaAnterior, cultura!.unidade) : null}
          carregando={leitura.carregando && !dados}
          variacao={`a janela anterior de ${extenso}`}
          motivoSemDado={doHorizonte ? motivoDoHorizonte(horizonte, doHorizonte.motivo) : 'A cultura não tem série de preço carregada.'}
          sobre="A média dos meses imediatamente anteriores, do mesmo tamanho."
        />
        <CartaoDeDecisao
          rotulo={`Variação — ${HORIZONTES.find((h) => h.id === horizonte)!.rotulo}`}
          icone={TrendingUp}
          tom="captura"
          valor={doHorizonte?.indice != null ? variacao(doHorizonte.indice) : null}
          carregando={leitura.carregando && !dados}
          variacao={doHorizonte?.faixa ? NOME_DA_FAIXA[doHorizonte.faixa] : null}
          motivoSemDado={doHorizonte ? motivoDoHorizonte(horizonte, doHorizonte.motivo) : 'A cultura não tem série de preço carregada.'}
          sobre="O preço médio atual sobre o comparado, menos 1. A faixa é a do momento do CRM."
        />
        <CartaoDeDecisao
          rotulo="R12 — renda do ciclo"
          icone={Coins}
          tom="demanda"
          valor={doCiclo?.indice != null ? variacao(doCiclo.indice) : null}
          carregando={leitura.carregando && !dados}
          variacao={
            doCiclo?.indice != null
              ? `${doCiclo.faixa ? NOME_DA_FAIXA[doCiclo.faixa].toLowerCase() : 'sem faixa'}${doCiclo.serie === 'AnualPam' ? ' · preço anual da PAM' : ''} · tendência R3 ${r3?.indice != null ? `${sentido(r3.indice).seta} ${variacao(r3.indice)}` : '—'}`
              : null
          }
          motivoSemDado={doCiclo ? motivoDoHorizonte(12, doCiclo.motivo) : 'A cultura não tem série de preço carregada.'}
          sobre="O momento de preço que entra no fator de ciclo da demanda: 12 meses contra os 12 anteriores, ou o preço anual da PAM enquanto a série mensal não fecha 24 meses. A seta é a tendência recente (R3)."
        />
      </div>

      {leitura.carregando && !dados && <BlocoCarregando oQue="os preços" />}

      {dados && cultura && (
        <>
          <section className="dash-secao" data-bloco="secao-momento">
            <TituloDaSecao
              titulo="O momento por cultura"
              subtitulo={`variação de preço em cada horizonte · ordenado por ${extenso}`}
              metodologia="A média dos últimos N meses sobre a dos N anteriores, com as duas janelas cheias. A cana é medida pelo ATR mensal da Socicana; as outras culturas, pelo preço recebido da CONAB."
            />
            <PainelDoMomento
              titulo="Momento econômico por cultura"
              dica="↑ em alta · ↓ em queda · → estável. O R12 ★ é o momento que entra no fator de ciclo da demanda; quando vem da PAM, a célula diz. Clique numa cultura para ver os números e a série dela."
              direita={<Limitacoes dados={dados} />}
              data-bloco="momento-por-cultura"
            >
              <MomentoPorCultura culturas={dados.culturas} horizonte={horizonte} aoEscolher={setCodigo} />
            </PainelDoMomento>
          </section>

          <section className="dash-secao" data-bloco="secao-serie">
            <TituloDaSecao
              titulo={`Série histórica — ${cultura.nome}`}
              subtitulo={cultura.serie.length ? `${cultura.unidade || 'R$'} · ${cultura.fonte} · até ${mesCurto(cultura.serie[cultura.serie.length - 1].mes)}` : 'sem série carregada'}
              metodologia="O preço mês a mês, como a fonte publica. A CONAB é uma janela móvel de 12 meses que o CRM acumula desde 09/2025; o ATR da Socicana tem mais de dez anos."
            />
            <PainelDoMomento titulo="Preço mês a mês" data-bloco="serie">
              {serie.length > 1 ? (
                <MolduraDeGrafico altura={260}>
                  {(l, a) => (
                    <GraficoLinhaMensal
                      rotulos={serie.map((m) => mesCurto(m.mes))}
                      valores={serie.map((m) => m.valor)}
                      largura={l}
                      altura={a}
                      formatar={(v) => preco(v, '')}
                    />
                  )}
                </MolduraDeGrafico>
              ) : (
                <GraficoSemSerie
                  altura={260}
                  frase="Série sem meses suficientes"
                  motivo="A cultura não tem preço mensal carregado em mais de um mês."
                  oQue="a série de preço"
                />
              )}
            </PainelDoMomento>
          </section>
        </>
      )}
    </PaginaDoPainel>
  );
}

function Limitacoes({ dados }: { dados: PrecosDasCulturas }) {
  if (dados.lacunas.length === 0) return null;
  return (
    <span className="diag-limitacoes">
      Limitações dos dados
      <InfoTooltip
        rotulo="Limitações dos dados dos preços"
        texto={<MetricasSemDado metricas={dados.lacunas} titulo="O que esta leitura não afirma" naDica />}
      />
    </span>
  );
}
