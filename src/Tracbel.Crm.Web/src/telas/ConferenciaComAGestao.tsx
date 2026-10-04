/**
 * Conferência com a Gestão de Negócios (28/09/2026) — os números do CRM contra o gabarito da GN.
 *
 * - **filial a filial e mês a mês**: a meta (sem consórcio) e o realizado (só a máquina entregue, no mês da entrega), lá e
 *   aqui, com a diferença;
 * - **chassi a chassi**: cada máquina do realizado que não bate, com o tipo — só na GN, pendente no ART, sem a entrega no
 *   CRM, em outra filial, em outro mês, só no CRM — e o que cada lado diz.
 *
 * É apurada pela rotina 13, uma vez por dia. Nenhum nome de cliente ou de CEN.
 *
 * 29/09/2026 — NO PADRÃO DOS INDICADORES GEOGRÁFICOS (#293, bloco 3): página na coluna inteira, o tipo e o chassi na
 * barra de filtros, os cinco números em cartões de decisão e duas seções — os números lá e aqui, e as máquinas que não
 * batem. Nenhum número nem texto mudou; os seis tipos de divergência são os mesmos.
 */

import { Crosshair, ListFilter, RefreshCw, ScanSearch, Search, Target, Truck, TriangleAlert } from 'lucide-react';
import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { obterConferenciaComAGestao } from '../dados/api/conferencia';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { DivergenciaNaTela, NumerosDaConferencia } from '../tipos/conferencia';
import { formatarMesComAno, formatarNumero } from '../dados/formatadores';
import { CampoDoFiltro } from '../componentes/comum/CampoDoFiltro';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';
import '../estilos/conferencia.css';

const NAO_APURADA = 'ainda não apurada';

/** A diferença como a tela escreve: o CRM menos a GN, com sinal; zero é "bate". */
export function diferenca(crm: number, gn: number): string {
  const d = crm - gn;
  return d === 0 ? 'bate' : `${d > 0 ? '+' : '−'}${formatarNumero(Math.abs(d))}`;
}

/** As divergências do filtro, e o CSV leva exatamente essas. */
export function filtrar(divergencias: DivergenciaNaTela[], tipo: string, busca: string): DivergenciaNaTela[] {
  const termo = busca.trim().toUpperCase();
  return divergencias.filter((d) => (!tipo || d.tipo === tipo) && (!termo || d.chassi.includes(termo)));
}

const CABECALHO_DO_CSV = ['Tipo', 'Chassi', 'Filial', 'Na Gestão de Negócios', 'No CRM', 'O que é', 'Detectada em'];

function Numeros({ numeros: x }: { numeros: NumerosDaConferencia }) {
  return (
    <>
      <td className="mom-num">{formatarNumero(x.metaNaGestao)}</td>
      <td className="mom-num">{formatarNumero(x.metaNoCrm)}</td>
      <td className={`mom-num conf-dif${x.metaNoCrm === x.metaNaGestao ? ' bate' : ''}`}>{diferenca(x.metaNoCrm, x.metaNaGestao)}</td>
      <td className="mom-num">{formatarNumero(x.realizadoNaGestao)}</td>
      <td className="mom-num">{formatarNumero(x.realizadoNoCrm)}</td>
      <td className={`mom-num conf-dif${x.realizadoNoCrm === x.realizadoNaGestao ? ' bate' : ''}`}>
        {diferenca(x.realizadoNoCrm, x.realizadoNaGestao)}
      </td>
    </>
  );
}

function CabecalhoDosNumeros({ primeira }: { primeira: string }) {
  return (
    <thead>
      <tr>
        <th scope="col" rowSpan={2}>{primeira}</th>
        <th scope="colgroup" colSpan={3}>Meta (PO)</th>
        <th scope="colgroup" colSpan={3}>Realizado (entregue)</th>
      </tr>
      <tr>
        <th scope="col" className="mom-num">GN</th>
        <th scope="col" className="mom-num">CRM</th>
        <th scope="col" className="mom-num">CRM − GN</th>
        <th scope="col" className="mom-num">GN</th>
        <th scope="col" className="mom-num">CRM</th>
        <th scope="col" className="mom-num">CRM − GN</th>
      </tr>
    </thead>
  );
}

export function ConferenciaComAGestao() {
  const { contexto } = useContextoDeAcesso();
  const [tipo, setTipo] = useState('');
  const [busca, setBusca] = useState('');

  const conferencia = useRecurso((sinal) => obterConferenciaComAGestao(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const dados = conferencia.dados;
  const visiveis = useMemo(() => (dados ? filtrar(dados.divergencias, tipo, busca) : []), [dados, tipo, busca]);

  const apurada = dados?.apuradaEm != null;
  const t = dados?.totais;

  /** Um número da conferência: sem apuração, o traço — e a linha de baixo diz que não foi apurada, como a faixa antiga. */
  const apurado = (valor: number | undefined) => (t && apurada && valor !== undefined ? formatarNumero(valor) : null);
  const linhaDeBaixo = (deOnde: string) => (conferencia.carregando ? null : t && apurada ? deOnde : NAO_APURADA);
  const motivo = dados ? 'A conferência ainda não foi apurada pela rotina 13, que roda uma vez por dia.' : undefined;

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Conferência com a Gestão de Negócios</h1>
          <p className="page-subtitle">
            A meta e o realizado de máquinas como a Gestão de Negócios conta e como o CRM conta — e cada máquina que não bate, com
            o motivo.
          </p>
        </div>
        <p className="dash-atualizado">
          {conferencia.procedencia ? <DadosAtualizadosEm procedencia={conferencia.procedencia} /> : 'Lendo a conferência…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={conferencia.recarregar}
            disabled={conferencia.carregando}
            data-carregando={conferencia.carregando ? 'true' : 'false'}
            aria-label="Reler a conferência"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES: o tipo e o chassi filtram a lista das máquinas que não batem. Os cartões e as tabelas
          por filial e por mês são sempre da conferência inteira. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <CampoDoFiltro icone={ListFilter} rotulo="Tipo" bloco="tipo">
            <select value={tipo} onChange={(e) => setTipo(e.target.value)}>
              <option value="">Todos</option>
              {dados?.porTipo.map((p) => (
                <option key={p.tipo} value={p.tipo}>
                  {p.rotulo}
                </option>
              ))}
            </select>
          </CampoDoFiltro>

          <CampoDoFiltro icone={Search} rotulo="Chassi" bloco="busca">
            <input type="search" value={busca} placeholder="Buscar pelo chassi" onChange={(e) => setBusca(e.target.value)} />
          </CampoDoFiltro>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={conferencia.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="Realizado na GN"
          icone={Truck}
          tom="demanda"
          valor={apurado(t?.realizadoNaGestao)}
          carregando={conferencia.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('máquinas entregues, na performance da GN')}
          sobre="As máquinas entregues que a Gestão de Negócios conta na performance — só a máquina entregue, no mês da entrega."
        />
        <CartaoDeDecisao
          rotulo="Realizado no CRM"
          icone={Crosshair}
          tom="captura"
          valor={apurado(t?.realizadoNoCrm)}
          carregando={conferencia.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo(t ? `${diferenca(t.realizadoNoCrm, t.realizadoNaGestao)} contra a GN` : '')}
          sobre="As máquinas que o CRM conta como realizado pela mesma régua da GN — a entregue no ART, no mês da entrega. A linha de baixo é o CRM menos a GN; zero é “bate”."
        />
        <CartaoDeDecisao
          rotulo="Meta na GN"
          icone={Target}
          tom="mercado"
          valor={apurado(t?.metaNaGestao)}
          carregando={conferencia.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('o PO, sem consórcio')}
          sobre="A meta de máquinas (o PO) na Gestão de Negócios, sem consórcio."
        />
        <CartaoDeDecisao
          rotulo="Meta no CRM"
          icone={ScanSearch}
          tom="oportunidade"
          valor={apurado(t?.metaNoCrm)}
          carregando={conferencia.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo(t ? `${diferenca(t.metaNoCrm, t.metaNaGestao)} contra a GN` : '')}
          sobre="A meta de máquinas que o CRM tem, sem consórcio. A linha de baixo é o CRM menos a GN; zero é “bate”."
        />
        <CartaoDeDecisao
          rotulo="Máquinas que não batem"
          icone={TriangleAlert}
          tom="neutro"
          valor={dados && apurada ? formatarNumero(dados.divergencias.length) : null}
          carregando={conferencia.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('divergências abertas, chassi a chassi')}
          sobre="As máquinas do realizado que não batem entre a GN e o CRM, chassi a chassi, com o tipo da divergência. A lista está na seção de baixo."
        />
      </div>

      <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que esta conferência não afirma" />

      {conferencia.carregando && <BlocoCarregando oQue="a conferência" />}
      {conferencia.erro && <BlocoErro erro={conferencia.erro} aoTentarDeNovo={conferencia.recarregar} />}

      {dados && (
        <section className="dash-secao" data-bloco="secao-numeros">
          <TituloDaSecao titulo="Os números lá e aqui" subtitulo="A meta e o realizado na GN e no CRM, com a diferença." />

          <PainelDoMomento
            titulo="Por filial"
            data-bloco="conferencia-por-filial"
            subtitulo={
              <>
                {dados.alcance === 'Organizacao' ? 'todas as filiais' : 'a filial escolhida'}
                {dados.apuradaEm && <> · apurada em {new Date(dados.apuradaEm).toLocaleString('pt-BR')}</>}
              </>
            }
          >
            {dados.porFilial.length === 0 ? (
              <BlocoVazio titulo="Nada apurado" texto="A conferência vem da rotina 13, uma vez por dia." />
            ) : (
              <div className="mom-tabela-rolagem">
                <table className="mom-tabela conf-tabela">
                  <caption className="cad-so-leitor">Meta e realizado por filial, na GN e no CRM</caption>
                  <CabecalhoDosNumeros primeira="Filial" />
                  <tbody>
                    {dados.porFilial.map((f) => (
                      <tr key={f.filial}>
                        <th scope="row">{f.filial}</th>
                        <Numeros numeros={f.numeros} />
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </PainelDoMomento>

          {dados.porMes.length > 0 && (
            <PainelDoMomento titulo="Por mês" data-bloco="conferencia-por-mes" subtitulo="o mês da entrega, para o realizado">
              <div className="mom-tabela-rolagem">
                <table className="mom-tabela conf-tabela">
                  <caption className="cad-so-leitor">Meta e realizado por mês, na GN e no CRM</caption>
                  <CabecalhoDosNumeros primeira="Mês" />
                  <tbody>
                    {dados.porMes.map((m) => (
                      <tr key={m.competencia}>
                        <th scope="row">{formatarMesComAno(m.competencia)}</th>
                        <Numeros numeros={m.numeros} />
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </PainelDoMomento>
          )}
        </section>
      )}

      {dados && (
        <section className="dash-secao" data-bloco="secao-divergencias">
          <TituloDaSecao titulo="As máquinas que não batem" subtitulo="Chassi a chassi, com o tipo e o que cada lado diz." />

          <PainelDoMomento
            titulo="Máquinas que não batem"
            data-bloco="divergencias"
            subtitulo={dados.porTipo.filter((p) => p.quantidade > 0).map((p) => `${p.rotulo}: ${formatarNumero(p.quantidade)}`).join(' · ') || 'nenhuma divergência aberta'}
            direita={
              <button
                type="button"
                className="btn btn-secondary btn-sm"
                disabled={visiveis.length === 0}
                onClick={() =>
                  baixarCsv(`conferencia-gn-${carimboDeData()}`, CABECALHO_DO_CSV,
                    visiveis.map((d) => [d.rotulo, d.chassi, d.filial, d.naGestao ?? '', d.noCrm ?? '', d.descricao, d.detectadaEm.slice(0, 10)]))
                }
              >
                Exportar CSV
              </button>
            }
          >
            {visiveis.length === 0 ? (
              <p className="conf-nada">Nenhuma máquina com esses filtros.</p>
            ) : (
              <div className="mom-tabela-rolagem">
                <table className="mom-tabela conf-lista">
                  <caption className="cad-so-leitor">Máquinas do realizado que não batem entre a GN e o CRM</caption>
                  <thead>
                    <tr>
                      <th scope="col">Chassi</th>
                      <th scope="col">Tipo</th>
                      <th scope="col">Na GN</th>
                      <th scope="col">No CRM</th>
                      <th scope="col">O que é</th>
                    </tr>
                  </thead>
                  <tbody>
                    {visiveis.map((d) => (
                      <tr key={`${d.tipo}-${d.chassi}`}>
                        <td className="cad-mono">{d.chassi}</td>
                        <td>{d.rotulo}</td>
                        <td>{d.naGestao ?? '—'}</td>
                        <td>{d.noCrm ?? '—'}</td>
                        <td className="conf-texto">{d.descricao}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </PainelDoMomento>
        </section>
      )}
    </PaginaDoPainel>
  );
}
