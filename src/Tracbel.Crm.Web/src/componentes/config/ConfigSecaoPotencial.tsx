/**
 * Configurações › Comercial › Potencial de mercado — a tela do administrador dos parâmetros (issue 77).
 *
 * O QUE ELA FAZ, na ordem da tela:
 *
 * 1. **Mostra o que vale numa data** (padrão: hoje) e, em frase, **o que falta decidir** — o motor não usa
 *    valor padrão para o que está em aberto.
 * 2. **Registra vigências novas** dos parâmetros gerais, da regra de cada cultura e da percepção do gestor. Não
 *    há "editar": a vigência anterior continua valendo para as datas em que valia.
 * 3. **Mostra a trilha** — quem registrou, quando e por quê — e revoga o que ainda não passou de hoje.
 *
 * Os botões aparecem conforme o perfil (`usePermissoes`). Quem protege de verdade é a API, que responde 403.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { PERMISSAO, usePermissoes } from '../../dados/api/permissoes';
import {
  listarHistoricoDosParametros,
  listarOpcoesDosParametros,
  obterCatalogoDoMercado,
  obterParametrosDoPotencial,
} from '../../dados/api/potencial';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarDataHora } from '../../telas/cadastro/formato';
import type { ParametrosGeraisDetalhe, VigenciaDoParametro } from '../../tipos/potencial';
import { AvisoDoFormulario, CampoTexto } from '../cadastro/CamposDeFormulario';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { CardConfig } from './ConfigPartes';
import { FormularioDaPercepcao } from './potencial/FormularioDaPercepcao';
import { FormularioDaRegra } from './potencial/FormularioDaRegra';
import { FormularioDosGerais } from './potencial/FormularioDosGerais';
import { HistoricoDosParametros } from './potencial/HistoricoDosParametros';
import { dataCurta, hojeEmSaoPaulo, iniciosVigentes, montarHistorico, numero } from './potencial/vigencias';
import '../../estilos/potencial.css';

type Formulario = 'geral' | 'cultura' | 'percepcao' | null;

function EmAberto() {
  return <span className="pot-em-aberto">em aberto</span>;
}

function LinhaDaVigencia({ vigencia }: { vigencia: VigenciaDoParametro }) {
  return (
    <div className="pot-vigencia">
      Vigente desde <strong>{dataCurta(vigencia.vigenteDesde)}</strong> ·{' '}
      {/* A SEMENTE NÃO TEM HORA: ela nasceu na migração, e a hora gravada é só a meia-noite da data. */}
      {vigencia.informadoPor
        ? `registrado por ${vigencia.informadoPor} em ${formatarDataHora(vigencia.informadoEm)}`
        : 'semente da migração'}
      <br />
      {vigencia.justificativa}
    </div>
  );
}

function ParametrosGerais({ geral }: { geral: ParametrosGeraisDetalhe }) {
  const pesos = [geral.pesoDoIndicadorDePreco, geral.pesoDoIndicadorDeCredito, geral.pesoDoIndicadorComercial];
  return (
    <>
      <dl className="pot-lista">
        <div>
          <dt>Janela dos índices</dt>
          <dd>
            {geral.mesesDaJanela} meses contra os {geral.mesesDaJanela} anteriores
          </dd>
        </div>
        <div>
          <dt>Índice de crédito</dt>
          <dd>
            {numero(geral.pesoDosContratosNoCredito * 100, 0)}% contratos · {numero(geral.pesoDoValorNoCredito * 100, 0)}% valor
          </dd>
        </div>
        <div>
          <dt>Faixas de mercado</dt>
          <dd>
            &lt; {numero(geral.limiteDeRetracao)} retraído · até {numero(geral.limiteDeAquecimento)}{' '}
            {geral.nomeDaFaixaIntermediaria ?? <EmAberto />} · &gt; {numero(geral.limiteDeAquecimento)} aquecido · &gt;{' '}
            {numero(geral.limiteDeSuperaquecimento)} superaquecido
          </dd>
        </div>
        <div>
          <dt>Percepção do gestor</dt>
          <dd>
            de −{numero(geral.limiteDaPercepcao)}% a +{numero(geral.limiteDaPercepcao)}% por município
          </dd>
        </div>
        <div>
          <dt>Pesos: preço · crédito · comercial</dt>
          <dd>{pesos.every((p) => p === null) ? <EmAberto /> : pesos.map((p) => numero(p, 3)).join(' · ')}</dd>
        </div>
        <div>
          <dt>Limites do fator de ciclo</dt>
          <dd>
            {geral.fatorMinimo === null ? <EmAberto /> : `${numero(geral.fatorMinimo, 3)} a ${numero(geral.fatorMaximo, 3)}`}
          </dd>
        </div>
      </dl>
      <LinhaDaVigencia vigencia={geral.vigencia} />
    </>
  );
}

export function ConfigSecaoPotencial() {
  const { contexto } = useContextoDeAcesso();
  const permissoes = usePermissoes();
  const hoje = hojeEmSaoPaulo();
  const [em, setEm] = useState(hoje);
  const [aberto, setAberto] = useState<Formulario>(null);
  const [recado, setRecado] = useState<string | null>(null);

  const vigentes = useRecurso(
    (sinal) => obterParametrosDoPotencial(contexto, em || undefined, sinal),
    [contexto.empresa, contexto.usuario, em],
  );
  const historico = useRecurso((sinal) => listarHistoricoDosParametros(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const opcoes = useRecurso((sinal) => listarOpcoesDosParametros(contexto, sinal), [contexto.empresa, contexto.usuario]);
  // O CATÁLOGO ALIMENTA OS DOIS CAMPOS NOVOS DA REGRA (D-P01): cultura e categoria de máquina. Ele é a
  // fonte única do que é uma cultura — a lista não volta a ser escrita no código do formulário.
  const catalogo = useRecurso((sinal) => obterCatalogoDoMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);

  const podeAdministrar = permissoes.tem(PERMISSAO.parametroDoPotencialAdministrar);
  const podeInformar = permissoes.tem(PERMISSAO.percepcaoDoGestorInformar);

  // O QUE VALE HOJE sai do histórico, que tem tudo — é o ponto de partida dos formulários, mesmo quando a tela
  // está mostrando outra data.
  const todos = historico.dados;
  const inicioDosGerais = todos ? iniciosVigentes(todos.gerais, () => 'geral', hoje).get('geral') : undefined;
  const geraisDeHoje = todos?.gerais.find((g) => !g.vigencia.revogadoEm && g.vigencia.vigenteDesde === inicioDosGerais) ?? null;
  const iniciosDasCulturas = todos ? iniciosVigentes(todos.culturas, (r) => String(r.produtoCodigoIbge), hoje) : new Map<string, string>();
  const culturasDeHoje = todos?.culturas.filter(
    (r) => !r.vigencia.revogadoEm && iniciosDasCulturas.get(String(r.produtoCodigoIbge)) === r.vigencia.vigenteDesde,
  ) ?? [];

  function gravou(mensagem: string) {
    setRecado(mensagem);
    setAberto(null);
    vigentes.recarregar();
    historico.recarregar();
  }

  function abrir(formulario: Formulario) {
    setRecado(null);
    setAberto(formulario);
  }

  const dados = vigentes.dados;

  return (
    <>
      {recado && <AvisoDoFormulario titulo={recado} tom="sucesso" />}

      <CardConfig titulo="Parâmetros do potencial de mercado">
        <div className="pot-cabecalho">
          <div className="config-hint">
            Cada parâmetro tem data de início, autor e justificativa. Mudar um valor registra uma vigência nova; a
            anterior continua valendo para as datas em que valia — o cálculo de uma data passada usa o parâmetro daquela
            data.
          </div>
          <CampoTexto
            rotulo="Ver o que vale em"
            tipo="date"
            valor={em}
            aoMudar={(v) => setEm(v)}
            ajuda={em && em !== hoje ? `Mostrando ${dataCurta(em)}, não hoje.` : undefined}
          />
        </div>
        {vigentes.carregando && <BlocoCarregando oQue="os parâmetros" />}
        {vigentes.erro && <BlocoErro erro={vigentes.erro} aoTentarDeNovo={vigentes.recarregar} />}
        {dados && dados.pendencias.length > 0 && (
          <AvisoDoFormulario titulo="O que falta decidir para o potencial sair completo" tom="atencao">
            <ul className="pot-pendencias">
              {dados.pendencias.map((p) => (
                <li key={p}>{p}</li>
              ))}
            </ul>
          </AvisoDoFormulario>
        )}
        {!permissoes.carregando && !podeAdministrar && !podeInformar && (
          <div className="config-hint" style={{ marginTop: 12 }}>
            O seu perfil lê os parâmetros. Registrar vigência exige a permissão ParametroDoPotencial.Administrar (perfil
            Administrador); a percepção por município, PercepcaoDoGestor.Informar (perfil Gestor comercial).
          </div>
        )}
      </CardConfig>

      {dados && (
        <>
          <CardConfig titulo="Parâmetros gerais">
            {dados.geral ? (
              <ParametrosGerais geral={dados.geral} />
            ) : (
              <div className="config-hint">Não há parâmetros gerais vigentes em {dataCurta(dados.em)}.</div>
            )}
            {podeAdministrar && aberto !== 'geral' && (
              <div className="pot-botao-linha">
                <button type="button" className="btn btn-secondary" onClick={() => abrir('geral')} disabled={!todos}>
                  Registrar nova vigência
                </button>
              </div>
            )}
            {aberto === 'geral' && (
              <FormularioDosGerais vigente={geraisDeHoje} hoje={hoje} aoGravar={gravou} aoCancelar={() => setAberto(null)} />
            )}
          </CardConfig>

          <CardConfig titulo="Regra de cada cultura">
            {dados.culturas.length === 0 ? (
              <div className="config-hint">Nenhuma cultura tem regra vigente em {dataCurta(dados.em)}.</div>
            ) : (
              <div className="cad-tabela-wrap">
                <table className="cad-tabela pot-tabela">
                  <thead>
                    <tr>
                      <th scope="col">Cultura</th>
                      <th scope="col">1 máquina a cada</th>
                      <th scope="col">Renovação</th>
                      <th scope="col">Modelo</th>
                      <th scope="col">Situação</th>
                      <th scope="col">Vigente desde</th>
                      <th scope="col">Registrado por</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dados.culturas.map((r) => (
                      <tr key={r.produtoCodigoIbge}>
                        <td>{r.produtoNome}</td>
                        <td className="cad-mono">{numero(r.hectaresPorMaquina)} ha</td>
                        <td className="cad-mono">
                          {r.anosDeRenovacao === null ? <EmAberto /> : `${numero(r.anosDeRenovacao, 1)} anos`}
                        </td>
                        <td>{r.modeloDeReferencia}</td>
                        <td>{r.situacao === 'AConfirmar' ? 'A confirmar' : 'Confirmada'}</td>
                        <td className="cad-mono">{dataCurta(r.vigencia.vigenteDesde)}</td>
                        <td>{r.vigencia.informadoPor ?? 'semente da migração'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            {podeAdministrar && aberto !== 'cultura' && (
              <div className="pot-botao-linha">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => abrir('cultura')}
                  // O CATÁLOGO É PRÉ-REQUISITO: sem ele os campos de cultura e de categoria abririam vazios,
                  // e a regra não pode nascer sem os dois.
                  disabled={!opcoes.dados || !catalogo.dados || !todos}
                >
                  Registrar regra de uma cultura
                </button>
              </div>
            )}
            {aberto === 'cultura' && opcoes.dados && catalogo.dados && (
              <FormularioDaRegra
                produtos={opcoes.dados.produtos}
                culturas={catalogo.dados.culturas}
                categorias={catalogo.dados.categorias}
                vigentes={culturasDeHoje}
                hoje={hoje}
                aoGravar={gravou}
                aoCancelar={() => setAberto(null)}
              />
            )}
          </CardConfig>

          <CardConfig titulo="Percepção do gestor por município">
            {dados.percepcoes.length === 0 ? (
              <div className="config-hint">
                Nenhum município tem percepção vigente em {dataCurta(dados.em)}. Sem ela, o indicador comercial é neutro.
              </div>
            ) : (
              <div className="cad-tabela-wrap">
                <table className="cad-tabela pot-tabela">
                  <thead>
                    <tr>
                      <th scope="col">Município</th>
                      <th scope="col">Ajuste</th>
                      <th scope="col">Vigente desde</th>
                      <th scope="col">Registrado por</th>
                      <th scope="col">Justificativa</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dados.percepcoes.map((p) => (
                      <tr key={p.municipioCodigoIbge}>
                        <td>
                          {p.municipioNome}/{p.uf}
                        </td>
                        <td className="cad-mono">
                          {p.percentual > 0 ? '+' : ''}
                          {numero(p.percentual)}%
                        </td>
                        <td className="cad-mono">{dataCurta(p.vigencia.vigenteDesde)}</td>
                        <td>{p.vigencia.informadoPor ?? '—'}</td>
                        <td>{p.vigencia.justificativa}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            {podeInformar && aberto !== 'percepcao' && (
              <div className="pot-botao-linha">
                <button type="button" className="btn btn-secondary" onClick={() => abrir('percepcao')} disabled={!opcoes.dados}>
                  Registrar percepção
                </button>
              </div>
            )}
            {aberto === 'percepcao' && opcoes.dados && (
              <FormularioDaPercepcao
                municipios={opcoes.dados.municipios}
                limiteDeHoje={geraisDeHoje?.limiteDaPercepcao ?? null}
                hoje={hoje}
                aoGravar={gravou}
                aoCancelar={() => setAberto(null)}
              />
            )}
          </CardConfig>
        </>
      )}

      <CardConfig titulo="Histórico e trilha">
        {historico.carregando && <BlocoCarregando oQue="o histórico dos parâmetros" />}
        {historico.erro && <BlocoErro erro={historico.erro} aoTentarDeNovo={historico.recarregar} />}
        {todos && (
          <HistoricoDosParametros
            linhas={montarHistorico(todos, hoje)}
            hoje={hoje}
            podeRevogar={(linha) => (linha.alvo.tipo === 'percepcao' ? podeInformar : podeAdministrar)}
            aoRevogar={gravou}
          />
        )}
      </CardConfig>
    </>
  );
}
