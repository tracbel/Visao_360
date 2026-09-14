/**
 * O detalhe de um município: território, responsáveis por fonte, e os três
 * indicadores com a composição de cada um.
 *
 * AS DUAS PLANILHAS APARECEM LADO A LADO. Quando elas nomeiam CENs diferentes,
 * o aviso diz que há divergência e não escolhe: a atualidade de nenhuma das
 * duas está confirmada (documento 32, seção 4.3).
 */

import type { IndicadoresDoMunicipio, RegraDePotencialAplicada, ResponsavelDeclarado } from '../../tipos/territorio';
import { reaisCompactos } from './escalas';

const FONTE: Record<ResponsavelDeclarado['fonte'], string> = {
  PlanilhaAreaDeAtuacao: 'Área de Atuação',
  PlanilhaCenEGestorPorMunicipio: 'CEN e Gestor por Município',
};

const SITUACAO: Record<ResponsavelDeclarado['situacao'], string> = {
  UsuarioIdentificado: 'usuário do CRM',
  VagaAContratar: 'vaga a contratar',
  NaoIdentificado: 'sem usuário no CRM',
};

const nº = (v: number) => v.toLocaleString('pt-BR');

export function DetalheDoMunicipio({
  municipio,
  regras,
  aoFechar,
}: {
  municipio: IndicadoresDoMunicipio;
  regras: RegraDePotencialAplicada[];
  aoFechar: () => void;
}) {
  const { cobertura, vendas } = municipio;

  return (
    <div className="card cad-cartao terr-detalhe" aria-live="polite">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">{municipio.nome}</div>
          <div className="card-subtitle">
            IBGE <span className="cad-mono">{municipio.codigoIbge}</span> ·{' '}
            {municipio.pertenceAAdr
              ? `ADR · região ${municipio.regiao}`
              : municipio.listadoNaAreaDeAtuacao
                ? 'na planilha de área de atuação, fora da ADR'
                : 'fora da área de atuação'}
            {municipio.lojaNome && ` · loja ${municipio.lojaNome.replace(/^.*—\s*/, '')}`}
            {municipio.lojaAtivaNoCrm === false && (
              <strong className="cad-atencao"> · filial inativa no CRM</strong>
            )}
          </div>
        </div>
        <button type="button" className="btn btn-secondary btn-sm" onClick={aoFechar}>
          Fechar
        </button>
      </div>

      <div className="terr-detalhe-grade">
        <section className="terr-detalhe-responsaveis">
          <h3 className="terr-detalhe-titulo">Responsáveis, por fonte</h3>
          {municipio.comparacaoDoCen === 'NomesDiferentes' && (
            <p className="terr-aviso terr-aviso-alerta">
              As duas planilhas nomeiam CENs diferentes. As duas fontes ficam; nenhuma foi escolhida.
            </p>
          )}
          {municipio.comparacaoDoCen === 'ProvavelMesmaPessoa' && (
            <p className="terr-aviso">
              As duas planilhas escrevem o CEN de jeitos diferentes — provavelmente a mesma pessoa, sem confirmação. As
              duas fontes ficam.
            </p>
          )}
          {municipio.responsaveis.length === 0 ? (
            <p className="cad-nada">Nenhuma planilha declara responsável.</p>
          ) : (
            <table className="cad-tabela terr-tabela-compacta">
              <caption className="cad-so-leitor">Responsáveis declarados</caption>
              <thead>
                <tr>
                  <th scope="col">Papel</th>
                  <th scope="col">Fonte</th>
                  <th scope="col">Como a fonte escreve</th>
                  <th scope="col">No CRM</th>
                </tr>
              </thead>
              <tbody>
                {municipio.responsaveis.map((r) => (
                  <tr key={`${r.papel}-${r.fonte}`}>
                    <td>{r.papel === 'Cen' ? 'CEN' : 'Gestor'}</td>
                    <td>{FONTE[r.fonte]}</td>
                    <td className="cad-mono">{r.nomeNaOrigem}</td>
                    <td>{r.usuarioNome ?? <span className="cad-sub">{SITUACAO[r.situacao]}</span>}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          {municipio.responsaveis.length > 0 && (
            <p className="cad-sub">
              Vigência não declarada pelas planilhas; lidas em{' '}
              {new Date(municipio.responsaveis[0].importadoEm).toLocaleDateString('pt-BR')}.
            </p>
          )}
        </section>

        <section>
          <h3 className="terr-detalhe-titulo">Cobertura de visita <span className="terr-selo terr-selo-RegraComercialProvisoria">Regra provisória</span></h3>
          {cobertura.vinculosComCadencia === 0 ? (
            <p className="cad-nada">Sem vínculo em carteira comercial com cadência declarada — sem base para medir.</p>
          ) : (
            <dl className="terr-numeros">
              <dt>Elegíveis</dt>
              <dd>{nº(cobertura.vinculosComCadencia)}</dd>
              <dt>Cobertos</dt>
              <dd>{nº(cobertura.cobertos)}</dd>
              <dt>Fora da cadência</dt>
              <dd>{nº(cobertura.foraDaCadencia)}</dd>
              <dt>Nunca contatados</dt>
              <dd>{nº(cobertura.nuncaContatados)}</dd>
              <dt>Pendentes</dt>
              <dd>
                <strong>
                  {nº(cobertura.pendentes)} ({cobertura.percentualPendente?.toLocaleString('pt-BR')}%)
                </strong>
              </dd>
            </dl>
          )}
          <p className="cad-sub">
            {nº(cobertura.clientes)} clientes com endereço aqui · {nº(cobertura.semCadencia)} vínculos em linha sem
            cadência, fora da conta.
          </p>
        </section>

        <section>
          <h3 className="terr-detalhe-titulo">Vendas no período</h3>
          <dl className="terr-numeros">
            <dt>Máquina</dt>
            <dd>{reaisCompactos(vendas.maquina)}</dd>
            <dt>Peça</dt>
            <dd>{reaisCompactos(vendas.peca)}</dd>
            <dt>Serviço</dt>
            <dd>{reaisCompactos(vendas.servico)}</dd>
            <dt>Outros</dt>
            <dd>{reaisCompactos(vendas.outros)}</dd>
            <dt>Total líquido</dt>
            <dd>
              <strong>{reaisCompactos(vendas.valorLiquido)}</strong>
            </dd>
            <dt>Pós-venda (peça + serviço, composição provisória)</dt>
            <dd>
              <strong>{reaisCompactos(vendas.posVenda)}</strong>
            </dd>
          </dl>
          <p className="cad-sub">{nº(vendas.clientesQueCompraram)} clientes compraram no período.</p>
        </section>

        <section>
          <h3 className="terr-detalhe-titulo">Potencial teórico por área</h3>
          {municipio.potencial.map((p) => {
            const regra = regras.find((r) => r.produtoCodigoIbge === p.produtoCodigoIbge);
            return (
              <dl className="terr-numeros" key={p.produtoCodigoIbge}>
                <dt>Área de {regra?.produtoNome ?? p.produtoCodigoIbge}</dt>
                <dd>{p.areaPlantadaHectares === null ? 'não disponível' : `${nº(p.areaPlantadaHectares)} ha`}</dd>
                <dt>{regra ? `${regra.modeloDeReferencia} teóricos (1 a cada ${regra.hectaresPorMaquina} ha)` : 'Máquinas teóricas'}</dt>
                <dd>
                  <strong>{p.maquinasTeoricas === null ? '—' : nº(p.maquinasTeoricas)}</strong>
                </dd>
              </dl>
            );
          })}
          <p className="cad-sub">Estimativa: área do município inteiro (IBGE), clientes e não clientes juntos, por uma regra a confirmar.</p>
        </section>
      </div>
    </div>
  );
}
