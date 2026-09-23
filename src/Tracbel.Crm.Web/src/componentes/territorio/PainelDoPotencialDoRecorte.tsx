/**
 * O POTENCIAL DO RECORTE CONSULTADO, pelo motor (issue 72).
 *
 * Ele responde três perguntas que o número sozinho não responde: **de que máquina**
 * estamos falando (as categorias), **de qual cultura** vem o parque (as parcelas, com
 * quem divide a terra), e **que fatia de São Paulo** está aqui.
 *
 * A demanda anual sai VAZIA COM O MOTIVO quando falta o ciclo de renovação de alguma
 * cultura: somar só as que têm daria um total menor que o real, com cara de completo.
 */

import type { PotencialDoRecorteNoMapa } from '../../tipos/territorio';
import { MOTIVO_SEM_PARQUE, nº, porcento } from './indicadoresDaAdr';

export function PainelDoPotencialDoRecorte({
  recorte,
  comFiltro,
}: {
  recorte: PotencialDoRecorteNoMapa;
  comFiltro: boolean;
}) {
  const relevancia = recorte.relevanciaNoEstado;

  return (
    <div className="terr-recorte">
      <p className="terr-recorte-titulo">
        No recorte consultado{comFiltro ? ' (com filtro)' : ''} — {nº(recorte.municipiosComParque)} municípios com parque
      </p>

      <ul className="terr-recorte-linhas">
        <li>
          <strong>{recorte.parqueDeMaquinas == null ? '—' : nº(Math.round(recorte.parqueDeMaquinas))}</strong> máquinas de
          parque{' '}
          {recorte.demandaAnualDeMaquinas == null ? (
            <span className="cad-sub">· demanda anual: {MOTIVO_SEM_PARQUE[recorte.motivoSemDemanda]}</span>
          ) : (
            <span className="cad-sub">· {nº(Math.round(recorte.demandaAnualDeMaquinas))} por ano</span>
          )}
        </li>

        {recorte.porCategoria.map((c) => (
          <li key={c.categoriaCodigo}>
            {c.categoriaNome}: <strong>{c.parqueDeMaquinas == null ? '—' : nº(Math.round(c.parqueDeMaquinas))}</strong>
            {c.demandaAnualDeMaquinas != null && (
              <span className="cad-sub"> · {nº(Math.round(c.demandaAnualDeMaquinas))} por ano</span>
            )}
          </li>
        ))}

        {recorte.porCultura.map((p) => (
          <li key={p.culturaCodigo}>
            {p.cultura}: {p.parque == null ? '—' : nº(Math.round(p.parque))} máquinas em{' '}
            {nº(Math.round(p.areaUtilHectares ?? 0))} ha
            {p.compartilhada.length > 0 && (
              <span className="cad-sub"> · área compartilhada com {p.compartilhada.join(', ')}</span>
            )}
          </li>
        ))}

        {relevancia && (
          <li>
            Fatia de São Paulo:{' '}
            {relevancia.fatiaDaAreaPlantada == null ? '—' : porcento(relevancia.fatiaDaAreaPlantada)} da área plantada ·{' '}
            {relevancia.fatiaDoValor == null ? '—' : porcento(relevancia.fatiaDoValor)} do valor da produção
          </li>
        )}
      </ul>

      {recorte.frase && <p className="terr-aviso">{recorte.frase}.</p>}

      {recorte.relevanciaPorCultura.length > 0 && (
        <table className="cad-tabela terr-recorte-tabela">
          <caption className="cad-sub">Relevância por cultura, contra o total publicado de São Paulo</caption>
          <thead>
            <tr>
              <th scope="col">Cultura</th>
              <th scope="col">Ano</th>
              <th scope="col">Área plantada</th>
              <th scope="col">Quantidade</th>
              <th scope="col">Valor</th>
              <th scope="col">Produtividade × SP</th>
            </tr>
          </thead>
          <tbody>
            {recorte.relevanciaPorCultura.map((c) => (
              <tr key={c.produtoCodigoIbge}>
                <td>{c.produtoNome}</td>
                <td className="cad-mono">{c.ano}</td>
                <td className="cad-mono">
                  {c.relevancia.fatiaDaAreaPlantada == null ? '—' : porcento(c.relevancia.fatiaDaAreaPlantada)}
                </td>
                <td className="cad-mono">
                  {c.relevancia.fatiaDaQuantidade == null ? '—' : porcento(c.relevancia.fatiaDaQuantidade)}
                </td>
                <td className="cad-mono">
                  {c.relevancia.fatiaDoValor == null ? '—' : porcento(c.relevancia.fatiaDoValor)}
                </td>
                <td
                  className="cad-mono"
                  title={
                    c.relevancia.produtividadeDoRecorte == null
                      ? undefined
                      : `${nº(Math.round(c.relevancia.produtividadeDoRecorte))} aqui contra ${nº(Math.round(c.relevancia.produtividadeNoEstado ?? 0))} em SP (${c.unidadeDaProdutividade})`
                  }
                >
                  {c.relevancia.razaoDeProdutividade == null
                    ? '—'
                    : `${c.relevancia.razaoDeProdutividade.toLocaleString('pt-BR', { maximumFractionDigits: 2 })}×`}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
