/**
 * Conferência com a Gestão de Negócios (28/09/2026) — os números do CRM contra o gabarito da GN.
 *
 * - **filial a filial e mês a mês**: a meta (sem consórcio) e o realizado (só a máquina entregue, no mês da entrega), lá e
 *   aqui, com a diferença;
 * - **chassi a chassi**: cada máquina do realizado que não bate, com o tipo — só na GN, pendente no ART, sem a entrega no
 *   CRM, em outra filial, em outro mês, só no CRM — e o que cada lado diz.
 *
 * É apurada pela rotina 13, uma vez por dia. Nenhum nome de cliente ou de CEN.
 */

import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { obterConferenciaComAGestao } from '../dados/api/conferencia';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { DivergenciaNaTela, NumerosDaConferencia } from '../tipos/conferencia';
import '../estilos/conferencia.css';

const n = (v: number) => v.toLocaleString('pt-BR');
const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const mes = (aaaammdd: string) => `${MESES[Number(aaaammdd.slice(5, 7)) - 1]}/${aaaammdd.slice(0, 4)}`;

/** A diferença como a tela escreve: o CRM menos a GN, com sinal; zero é "bate". */
export function diferenca(crm: number, gn: number): string {
  const d = crm - gn;
  return d === 0 ? 'bate' : `${d > 0 ? '+' : '−'}${n(Math.abs(d))}`;
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
      <td className="cad-mono">{n(x.metaNaGestao)}</td>
      <td className="cad-mono">{n(x.metaNoCrm)}</td>
      <td className={`cad-mono conf-dif${x.metaNoCrm === x.metaNaGestao ? ' bate' : ''}`}>{diferenca(x.metaNoCrm, x.metaNaGestao)}</td>
      <td className="cad-mono">{n(x.realizadoNaGestao)}</td>
      <td className="cad-mono">{n(x.realizadoNoCrm)}</td>
      <td className={`cad-mono conf-dif${x.realizadoNoCrm === x.realizadoNaGestao ? ' bate' : ''}`}>
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
        <th scope="col">GN</th>
        <th scope="col">CRM</th>
        <th scope="col">CRM − GN</th>
        <th scope="col">GN</th>
        <th scope="col">CRM</th>
        <th scope="col">CRM − GN</th>
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
  const indicadores: Indicador[] = [
    { rotulo: 'Realizado na GN', valor: t && apurada ? t.realizadoNaGestao : null, deOnde: 'máquinas entregues, na performance da GN', semDado: 'ainda não apurada' },
    {
      rotulo: 'Realizado no CRM',
      valor: t && apurada ? t.realizadoNoCrm : null,
      deOnde: t ? `${diferenca(t.realizadoNoCrm, t.realizadoNaGestao)} contra a GN` : '',
      tom: t && t.realizadoNoCrm === t.realizadoNaGestao ? 'bom' : 'atencao',
      semDado: 'ainda não apurada',
    },
    { rotulo: 'Meta na GN', valor: t && apurada ? t.metaNaGestao : null, deOnde: 'o PO, sem consórcio', semDado: 'ainda não apurada' },
    {
      rotulo: 'Meta no CRM',
      valor: t && apurada ? t.metaNoCrm : null,
      deOnde: t ? `${diferenca(t.metaNoCrm, t.metaNaGestao)} contra a GN` : '',
      tom: t && t.metaNoCrm === t.metaNaGestao ? 'bom' : 'atencao',
      semDado: 'ainda não apurada',
    },
    { rotulo: 'Máquinas que não batem', valor: dados && apurada ? dados.divergencias.length : null, deOnde: 'divergências abertas, chassi a chassi', semDado: 'ainda não apurada' },
  ];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Conferência com a Gestão de Negócios</h1>
          <p className="page-subtitle">
            A meta e o realizado de máquinas como a Gestão de Negócios conta e como o CRM conta — e cada máquina que não bate, com
            o motivo.
          </p>
        </div>
        {dados && (
          <button
            type="button"
            className="btn btn-secondary"
            disabled={visiveis.length === 0}
            onClick={() =>
              baixarCsv(`conferencia-gn-${carimboDeData()}`, CABECALHO_DO_CSV,
                visiveis.map((d) => [d.rotulo, d.chassi, d.filial, d.naGestao ?? '', d.noCrm ?? '', d.descricao, d.detectadaEm.slice(0, 10)]))
            }
          >
            Exportar CSV
          </button>
        )}
      </div>

      <AvisoDeProcedencia procedencia={conferencia.procedencia} />
      <PainelDeIndicadores indicadores={indicadores} carregando={conferencia.carregando} />
      <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que esta conferência não afirma" />

      {conferencia.carregando && <BlocoCarregando oQue="a conferência" />}
      {conferencia.erro && <BlocoErro erro={conferencia.erro} aoTentarDeNovo={conferencia.recarregar} />}

      {dados && (
        <div className="card cad-cartao" data-bloco="conferencia-por-filial">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Por filial</div>
              <div className="card-subtitle">
                {dados.alcance === 'Organizacao' ? 'todas as filiais' : 'a filial escolhida'}
                {dados.apuradaEm && <> · apurada em {new Date(dados.apuradaEm).toLocaleString('pt-BR')}</>}
              </div>
            </div>
            <SeloProcedencia procedencia={conferencia.procedencia} />
          </div>
          {dados.porFilial.length === 0 ? (
            <BlocoVazio titulo="Nada apurado" texto="A conferência vem da rotina 13, uma vez por dia." />
          ) : (
            <div className="cad-tabela-wrap">
              <table className="cad-tabela conf-tabela">
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
        </div>
      )}

      {dados && dados.porMes.length > 0 && (
        <div className="card cad-cartao" data-bloco="conferencia-por-mes">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Por mês</div>
              <div className="card-subtitle">o mês da entrega, para o realizado</div>
            </div>
          </div>
          <div className="cad-tabela-wrap">
            <table className="cad-tabela conf-tabela">
              <caption className="cad-so-leitor">Meta e realizado por mês, na GN e no CRM</caption>
              <CabecalhoDosNumeros primeira="Mês" />
              <tbody>
                {dados.porMes.map((m) => (
                  <tr key={m.competencia}>
                    <th scope="row">{mes(m.competencia)}</th>
                    <Numeros numeros={m.numeros} />
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {dados && (
        <div className="card cad-cartao" data-bloco="divergencias">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Máquinas que não batem</div>
              <div className="card-subtitle">
                {dados.porTipo.filter((p) => p.quantidade > 0).map((p) => `${p.rotulo}: ${n(p.quantidade)}`).join(' · ') || 'nenhuma divergência aberta'}
              </div>
            </div>
          </div>
          <div className="cad-barra">
            <label className="cad-filtro">
              Tipo
              <select value={tipo} onChange={(e) => setTipo(e.target.value)}>
                <option value="">Todos</option>
                {dados.porTipo.map((p) => (
                  <option key={p.tipo} value={p.tipo}>
                    {p.rotulo}
                  </option>
                ))}
              </select>
            </label>
            <label className="cad-filtro">
              Chassi
              <input type="search" value={busca} placeholder="Buscar pelo chassi" onChange={(e) => setBusca(e.target.value)} />
            </label>
          </div>
          {visiveis.length === 0 ? (
            <p className="conf-nada">Nenhuma máquina com esses filtros.</p>
          ) : (
            <div className="cad-tabela-wrap">
              <table className="cad-tabela conf-lista">
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
        </div>
      )}
    </>
  );
}
