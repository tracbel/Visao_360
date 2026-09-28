/**
 * Os dois cartões do planejamento comercial (issue 256): a sazonalidade com os pesos do IOC, e o share-alvo de cada
 * categoria. No protótipo da pasta 360 os três ficavam no navegador de quem editava; aqui são vigências, com autor e
 * trilha — a trilha é a mesma dos outros parâmetros, no fim da seção.
 */

import type { CategoriaNoCatalogo, ParametroDoPlanejamentoDetalhe, ParametrosDoPlanejamentoVigentes, ShareAlvoDetalhe } from '../../../tipos/potencial';
import { AvisoDoFormulario } from '../../cadastro/CamposDeFormulario';
import { CardConfig } from '../ConfigPartes';
import { FormularioDoPlanejamento } from './FormularioDoPlanejamento';
import { FormularioDoShare } from './FormularioDoShare';
import { COMPONENTES_DO_IOC, fatiasDoIoc, MESES } from './planejamento';
import { dataCurta, numero } from './vigencias';

export type FormularioDoPlanejamentoAberto = 'planejamento' | 'share' | null;

function Origem({ informadoPor }: { informadoPor: string | null }) {
  return <>{informadoPor ?? 'protótipo da pasta 360 (a confirmar)'}</>;
}

function Sazonalidade({ planejamento }: { planejamento: ParametroDoPlanejamentoDetalhe }) {
  const maior = Math.max(...planejamento.sazonalidade);
  return (
    <div className="pot-sazonalidade" role="list" aria-label="Sazonalidade da demanda por mês">
      {planejamento.sazonalidade.map((valor, i) => (
        <div key={MESES[i]} className="pot-mes" role="listitem">
          <div className="pot-mes-barra" aria-hidden="true">
            <span style={{ height: `${maior > 0 ? (valor / maior) * 100 : 0}%` }} />
          </div>
          <div className="pot-mes-valor cad-mono">{numero(valor)}%</div>
          <div className="pot-mes-nome">{MESES[i]}</div>
        </div>
      ))}
    </div>
  );
}

function Pesos({ planejamento }: { planejamento: ParametroDoPlanejamentoDetalhe }) {
  const fatias = fatiasDoIoc(planejamento.pesos);
  return (
    <div className="cad-tabela-wrap">
      <table className="cad-tabela pot-tabela">
        <thead>
          <tr>
            <th scope="col">Componente</th>
            <th scope="col">O que mede</th>
            <th scope="col">Peso</th>
            <th scope="col">Fatia do índice</th>
          </tr>
        </thead>
        <tbody>
          {COMPONENTES_DO_IOC.map((c) => (
            <tr key={c.chave}>
              <td>{c.rotulo}</td>
              <td>{c.mede}</td>
              <td className="cad-mono">{numero(planejamento.pesos[c.chave])}</td>
              <td className="cad-mono">{numero(fatias[c.chave], 1)}%</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function PlanejamentoComercial({
  dados,
  planejamentoDeHoje,
  sharesDeHoje,
  categorias,
  hoje,
  podeAdministrar,
  aberto,
  abrir,
  fechar,
  aoGravar,
}: {
  dados: ParametrosDoPlanejamentoVigentes;
  /** O que vale HOJE — é o ponto de partida dos formulários, mesmo quando a tela mostra outra data. */
  planejamentoDeHoje: ParametroDoPlanejamentoDetalhe | null;
  sharesDeHoje: ShareAlvoDetalhe[];
  categorias: CategoriaNoCatalogo[] | null;
  hoje: string;
  podeAdministrar: boolean;
  aberto: FormularioDoPlanejamentoAberto;
  abrir: (formulario: Exclude<FormularioDoPlanejamentoAberto, null>) => void;
  fechar: () => void;
  aoGravar: (mensagem: string) => void;
}) {
  const { planejamento } = dados;

  return (
    <>
      <CardConfig titulo="Planejamento comercial — sazonalidade e pesos do IOC">
        <div className="config-hint">
          A sazonalidade reparte a demanda do ano entre os meses — ela é a previsão mensal. Os pesos dizem quanto cada
          componente conta no Índice de Oportunidade Comercial, que ordena os municípios por onde vale mais a pena agir.
        </div>
        {dados.pendencias.length > 0 && (
          <AvisoDoFormulario titulo="O que falta para o planejamento sair inteiro" tom="atencao">
            <ul className="pot-pendencias">
              {dados.pendencias.map((p) => (
                <li key={p}>{p}</li>
              ))}
            </ul>
          </AvisoDoFormulario>
        )}
        {planejamento ? (
          <>
            <Sazonalidade planejamento={planejamento} />
            <Pesos planejamento={planejamento} />
            <div className="pot-vigencia">
              Vigente desde <strong>{dataCurta(planejamento.vigencia.vigenteDesde)}</strong> · registrado por{' '}
              <Origem informadoPor={planejamento.vigencia.informadoPor} />
              <br />
              {planejamento.vigencia.justificativa}
            </div>
          </>
        ) : (
          <div className="config-hint">Não há sazonalidade nem pesos vigentes em {dataCurta(dados.em)}.</div>
        )}
        {podeAdministrar && aberto !== 'planejamento' && (
          <div className="pot-botao-linha">
            <button type="button" className="btn btn-secondary" onClick={() => abrir('planejamento')}>
              Registrar nova vigência
            </button>
          </div>
        )}
        {aberto === 'planejamento' && (
          <FormularioDoPlanejamento vigente={planejamentoDeHoje} hoje={hoje} aoGravar={aoGravar} aoCancelar={fechar} />
        )}
      </CardConfig>

      <CardConfig titulo="Planejamento comercial — share-alvo por categoria">
        <div className="config-hint">
          A fatia da demanda que a Tracbel planeja entregar. Demanda estimada × share-alvo é a meta de planejamento do
          município — não é a cota do vendedor, que vem da API Gestão de Negócios.
        </div>
        {dados.shares.length === 0 ? (
          <div className="config-hint">Nenhuma categoria tem share-alvo vigente em {dataCurta(dados.em)}.</div>
        ) : (
          <div className="cad-tabela-wrap">
            <table className="cad-tabela pot-tabela">
              <thead>
                <tr>
                  <th scope="col">Categoria</th>
                  <th scope="col">Share-alvo</th>
                  <th scope="col">Vigente desde</th>
                  <th scope="col">Registrado por</th>
                </tr>
              </thead>
              <tbody>
                {dados.shares.map((s) => (
                  <tr key={s.categoriaDeMaquinaCodigo}>
                    <td>{s.categoriaDeMaquinaNome}</td>
                    <td className="cad-mono">{numero(s.percentual)}%</td>
                    <td className="cad-mono">{dataCurta(s.vigencia.vigenteDesde)}</td>
                    <td>
                      <Origem informadoPor={s.vigencia.informadoPor} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {podeAdministrar && aberto !== 'share' && (
          <div className="pot-botao-linha">
            <button type="button" className="btn btn-secondary" onClick={() => abrir('share')} disabled={!categorias}>
              Registrar share-alvo
            </button>
          </div>
        )}
        {aberto === 'share' && categorias && (
          <FormularioDoShare categorias={categorias} vigentes={sharesDeHoje} hoje={hoje} aoGravar={aoGravar} aoCancelar={fechar} />
        )}
      </CardConfig>
    </>
  );
}
