/**
 * OS MUNICÍPIOS DE ALTO POTENCIAL E BAIXA COBERTURA (issue 259): os 20 primeiros pela prioridade = valor da produção ×
 * lacuna de cobertura (a fatia dos clientes sem contato em 120 dias), entre os municípios com pelo menos três clientes.
 *
 * A COBERTURA É A DE 90 DIAS, como o protótipo: a barra e o número dizem a mesma coisa, e a cor só reforça — vermelho é
 * pouco coberto, verde é bem coberto.
 */

import type { MunicipioPrioritario } from '../../tipos/mercado';
import { n, pct, reaisDeMil } from './dimensionamento';

const tomDaCobertura = (c: number) => (c < 25 ? 'critica' : c < 50 ? 'baixa' : c < 75 ? 'media' : 'alta');

export function MunicipiosPrioritarios({ municipios }: { municipios: readonly MunicipioPrioritario[] }) {
  if (municipios.length === 0)
    return <p className="dim-nota">Nenhum município do recorte tem três clientes ou mais com lacuna de cobertura.</p>;

  const maior = Math.max(...municipios.map((m) => m.prioridade ?? 0), 1);

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela diag-tabela dim-tabela dim-prioritarios">
        <caption className="cad-so-leitor">Os municípios de maior valor de produção sem cobertura</caption>
        <thead>
          <tr>
            <th scope="col">Município</th>
            <th scope="col" className="mom-num">Valor da produção</th>
            <th scope="col" className="mom-num">Clientes</th>
            <th scope="col" className="mom-num">Cobertura em 90 dias</th>
            <th scope="col">Prioridade</th>
          </tr>
        </thead>
        <tbody>
          {municipios.map((m) => (
            <tr key={m.codigoIbge}>
              <th scope="row">{m.nome}</th>
              <td className="mom-num">{reaisDeMil(m.valor)}</td>
              <td className="mom-num">{n(m.clientes)}</td>
              <td className="mom-num">
                <span className="dim-barra-cobertura" data-tom={tomDaCobertura(m.coberturaAte90Percentual)}>
                  <span className="dim-barra-trilho" aria-hidden="true">
                    <span style={{ width: `${Math.min(100, m.coberturaAte90Percentual)}%` }} />
                  </span>
                  <span className="dim-barra-numero">{pct(m.coberturaAte90Percentual, 0)}</span>
                </span>
              </td>
              <td>
                <span className="dim-barra-prioridade" title={`${reaisDeMil(m.prioridade)} sem cobertura (${pct(m.lacunaPercentual, 0)} dos clientes)`}>
                  <span style={{ width: `${Math.max(4, ((m.prioridade ?? 0) / maior) * 100)}%` }} />
                </span>
                <span className="cad-so-leitor">
                  {reaisDeMil(m.prioridade)} sem cobertura, {pct(m.lacunaPercentual, 0)} dos clientes
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
