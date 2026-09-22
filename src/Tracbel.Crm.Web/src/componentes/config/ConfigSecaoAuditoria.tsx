/**
 * Configurações › Administração › Auditoria (issue 135) — a trilha real, no lugar da lista fixa do protótipo
 * ("Eventos recentes", `renderSecaoAuditoria` em prototipo/referencia/assets/app.js).
 *
 * QUEM VÊ: `Auditoria.Ler` (Diretoria e Administrador). A API decide; esconder aqui só evita oferecer o que vai ser
 * recusado.
 *
 * UM EVENTO É UMA GRAVAÇÃO NUM REGISTRO: a concessão de um perfil grava cinco campos, e aparece como uma linha só.
 * Clicar abre o antes e o depois de cada campo, com nomes no lugar de identificadores.
 *
 * A FRONTEIRA É A DE FILIAL: a trilha mostra a filial escolhida no seletor; "Todas as filiais" mostra todas. O que
 * não tem filial (perfil, município) fica na filial de casa de quem alterou.
 */

import { Fragment, useEffect, useState } from 'react';
import type { FilialDoEscopo } from '../../dados/api/acesso';
import { consultarTrilha, listarEntidadesAuditadas, type EventoDaAuditoria, type OrigemDaOperacao } from '../../dados/api/auditoria';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarDataHora } from '../../telas/cadastro/formato';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { CardConfig } from './ConfigPartes';

const ESPERA_DA_BUSCA_MS = 350;
const TODAS_AS_FILIAIS = 'TODAS';

type Periodo = '1' | '7' | '30' | '90' | 'personalizado';

const PERIODOS: { valor: Periodo; rotulo: string }[] = [
  { valor: '1', rotulo: 'Hoje' },
  { valor: '7', rotulo: 'Últimos 7 dias' },
  { valor: '30', rotulo: 'Últimos 30 dias' },
  { valor: '90', rotulo: 'Últimos 90 dias' },
  { valor: 'personalizado', rotulo: 'Escolher o período' },
];

const ORIGEM: Record<OrigemDaOperacao, string> = {
  Usuario: 'pessoa',
  Integracao: 'integração',
  Importacao: 'importação',
  Sistema: 'sistema',
  Job: 'rotina',
};

const VERBO = { Inclusao: 'Incluiu', Alteracao: 'Alterou', Exclusao: 'Excluiu' } as const;

/** O dia de hoje, menos `dias - 1`, em aaaa-mm-dd no fuso de quem vê. */
function diaLocal(diasAtras = 0): string {
  const data = new Date();
  data.setDate(data.getDate() - diasAtras);
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');
  return `${data.getFullYear()}-${mes}-${dia}`;
}

/** O valor como a pessoa lê: data ISO no fuso de quem vê; o resto como a API já mandou. */
function valorLegivel(valor: string | null): string {
  if (valor === null) return '—';
  if (/^\d{4}-\d{2}-\d{2}T/.test(valor)) return formatarDataHora(valor);
  const soData = /^(\d{4})-(\d{2})-(\d{2})$/.exec(valor);
  return soData ? `${soData[3]}/${soData[2]}/${soData[1]}` : valor;
}

/**
 * A FRASE DO EVENTO, como alguém contaria: "concedeu o perfil", "liberou a conta de". O caso geral é o verbo da
 * operação com o nome da entidade.
 */
function acao(evento: EventoDaAuditoria): string {
  const campo = (nome: string) => evento.campos.find((c) => c.campo === nome);
  switch (evento.entidade) {
    case 'UsuarioPerfil':
      if (evento.operacao === 'Inclusao') return 'Concedeu o perfil';
      if (campo('RevogadaEm')?.depois) return 'Revogou o perfil';
      break;
    case 'Usuario': {
      const espera = campo('AguardandoLiberacaoDesde');
      if (espera?.antes && !espera.depois) return 'Liberou a conta de';
      const ativo = campo('EstaAtivo');
      if (ativo?.depois === 'não') return 'Desativou a conta de';
      if (ativo?.depois === 'sim') return 'Reativou a conta de';
      break;
    }
    case 'Perfil':
      if (evento.operacao === 'Inclusao') return 'Criou o perfil';
      break;
    case 'PerfilPermissao':
      if (evento.operacao === 'Inclusao') return 'Deu a permissão';
      if (evento.operacao === 'Exclusao') return 'Tirou a permissão';
      return 'Mudou onde vale a permissão';
  }
  return `${VERBO[evento.operacao]} ${evento.entidadeRotulo.toLowerCase()}`;
}

const chave = (e: EventoDaAuditoria) => `${e.quando}|${e.entidade}|${e.registroId}|${e.operacao}`;

type Props = { filialAtual: FilialDoEscopo; podeVerTodasAsFiliais: boolean };

export function ConfigSecaoAuditoria({ filialAtual, podeVerTodasAsFiliais }: Props) {
  const { contexto } = useContextoDeAcesso();

  const [periodo, setPeriodo] = useState<Periodo>('30');
  const [de, setDe] = useState(diaLocal(29));
  const [ate, setAte] = useState(diaLocal());
  const [entidade, setEntidade] = useState('');
  const [operacao, setOperacao] = useState('');
  const [origem, setOrigem] = useState('');
  const [autorDigitado, setAutorDigitado] = useState('');
  const [autor, setAutor] = useState('');
  const [registro, setRegistro] = useState<{ entidade: string; id: number; descricao: string } | null>(null);
  const [pagina, setPagina] = useState(1);
  const [tamanho, setTamanho] = useState(25);
  const [aberto, setAberto] = useState<string | null>(null);

  useEffect(() => {
    const relogio = setTimeout(() => {
      setAutor(autorDigitado.trim());
      setPagina(1);
    }, ESPERA_DA_BUSCA_MS);
    return () => clearTimeout(relogio);
  }, [autorDigitado]);

  const intervalo = periodo === 'personalizado' ? { de, ate } : { de: diaLocal(Number(periodo) - 1), ate: diaLocal() };
  const entidadeDoFiltro = registro?.entidade ?? entidade;

  const entidades = useRecurso((sinal) => listarEntidadesAuditadas(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const leitura = useRecurso(
    (sinal) =>
      consultarTrilha(
        contexto,
        { ...intervalo, entidade: entidadeDoFiltro, registro: registro?.id ?? null, operacao, origem, autor, pagina, tamanho },
        sinal,
      ),
    [contexto.empresa, contexto.usuario, intervalo.de, intervalo.ate, entidadeDoFiltro, registro?.id, operacao, origem, autor, pagina, tamanho],
  );
  const dados = leitura.dados;

  /** Todo filtro novo volta à primeira página e fecha o evento aberto. */
  function mudar(aplicar: () => void) {
    aplicar();
    setPagina(1);
    setAberto(null);
  }

  const emTodas = filialAtual.codigo === TODAS_AS_FILIAIS;

  return (
    <CardConfig titulo="Eventos recentes">
      <p className="config-hint aud-fronteira">
        {emTodas ? (
          <>Mostrando <strong>todas as filiais</strong>.</>
        ) : (
          <>
            Mostrando a trilha de <strong>{filialAtual.nome}</strong>
            {podeVerTodasAsFiliais && <> — para ver todas, escolha “Todas as filiais” no seletor do topo</>}.
          </>
        )}
      </p>

      <div className="config-filtros-inline aud-filtros">
        <select aria-label="Período" value={periodo} onChange={(e) => mudar(() => setPeriodo(e.target.value as Periodo))}>
          {PERIODOS.map((p) => (
            <option key={p.valor} value={p.valor}>
              {p.rotulo}
            </option>
          ))}
        </select>
        {periodo === 'personalizado' && (
          <>
            <input type="date" aria-label="Primeiro dia" value={de} max={ate} onChange={(e) => mudar(() => setDe(e.target.value))} />
            <input type="date" aria-label="Último dia" value={ate} min={de} onChange={(e) => mudar(() => setAte(e.target.value))} />
          </>
        )}
        <select aria-label="Entidade" value={entidade} disabled={registro !== null} onChange={(e) => mudar(() => setEntidade(e.target.value))}>
          <option value="">Todas as entidades</option>
          {(entidades.dados ?? []).map((e) => (
            <option key={e.codigo} value={e.codigo}>
              {e.rotulo}
            </option>
          ))}
        </select>
        <select aria-label="Operação" value={operacao} onChange={(e) => mudar(() => setOperacao(e.target.value))}>
          <option value="">Todas as operações</option>
          <option value="Inclusao">Inclusões</option>
          <option value="Alteracao">Alterações</option>
          <option value="Exclusao">Exclusões</option>
        </select>
        <select aria-label="Origem" value={origem} onChange={(e) => mudar(() => setOrigem(e.target.value))}>
          <option value="">Todas as origens</option>
          {(Object.keys(ORIGEM) as OrigemDaOperacao[]).map((o) => (
            <option key={o} value={o}>
              {ORIGEM[o][0].toUpperCase() + ORIGEM[o].slice(1)}
            </option>
          ))}
        </select>
        <input
          type="search"
          aria-label="Buscar por quem alterou"
          placeholder="Quem alterou (nome ou e-mail)…"
          value={autorDigitado}
          onChange={(e) => setAutorDigitado(e.target.value)}
        />
      </div>

      {registro && (
        <div className="aud-registro">
          Só o histórico de <strong>{registro.descricao}</strong>
          <button type="button" className="btn btn-sm btn-secondary" onClick={() => mudar(() => setRegistro(null))}>
            Ver todos os registros
          </button>
        </div>
      )}

      {leitura.carregando && <BlocoCarregando oQue="a trilha" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {dados && dados.itens.length === 0 && <div className="config-tabela-vazia">Nenhuma alteração registrada com este filtro.</div>}
      {dados && dados.itens.length > 0 && (
        <div className="cad-tabela-wrap">
          <table className="config-tabela aud-tabela">
            <thead>
              <tr>
                <th scope="col">Quando</th>
                <th scope="col">Quem</th>
                <th scope="col">O que</th>
                <th scope="col" className="aud-col-filial">
                  Filial
                </th>
              </tr>
            </thead>
            <tbody>
              {dados.itens.map((e) => {
                const id = chave(e);
                const estaAberto = aberto === id;
                return (
                  <Fragment key={id}>
                    <tr className={`adm-linha ${estaAberto ? 'adm-linha-ativa' : ''}`} onClick={() => setAberto(estaAberto ? null : id)}>
                      <td className="aud-col-quando mono muted">{formatarDataHora(e.quando)}</td>
                      <td className="aud-col-quem">
                        <div>{e.autor}</div>
                        {e.origem !== 'Usuario' && (
                          <span className="badge aud-origem">
                            {ORIGEM[e.origem]}
                            {e.sistema ? ` · ${e.sistema}` : ''}
                          </span>
                        )}
                      </td>
                      <td className="aud-col-oque">
                        <button type="button" className="adm-link" aria-expanded={estaAberto} onClick={() => setAberto(estaAberto ? null : id)}>
                          {acao(e)} <strong>{e.registro}</strong>
                        </button>
                        <div className="muted">
                          {e.entidadeRotulo} · {e.campos.length} {e.campos.length === 1 ? 'campo' : 'campos'}
                        </div>
                      </td>
                      <td className="aud-col-filial muted">{e.filialNome}</td>
                    </tr>
                    {estaAberto && (
                      <tr className="aud-detalhe">
                        <td colSpan={4}>
                          <table className="aud-campos">
                            <thead>
                              <tr>
                                <th scope="col">Campo</th>
                                <th scope="col">Antes</th>
                                <th scope="col">Depois</th>
                              </tr>
                            </thead>
                            <tbody>
                              {e.campos.map((c) => (
                                <tr key={c.campo}>
                                  <th scope="row">{c.rotulo}</th>
                                  <td data-rotulo="antes" className={c.antes === null ? 'muted' : 'aud-antes'}>{valorLegivel(c.antes)}</td>
                                  <td data-rotulo="depois" className={c.depois === null ? 'muted' : 'aud-depois'}>{valorLegivel(c.depois)}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                          <div className="aud-rodape">
                            <span className="muted">
                              {e.autorLogin && <>{e.autorLogin} · </>}
                              {e.filialCodigo} · registro nº {e.registroId}
                            </span>
                            {!registro && (
                              <button
                                type="button"
                                className="btn btn-sm btn-secondary"
                                onClick={() => mudar(() => setRegistro({ entidade: e.entidade, id: e.registroId, descricao: e.registro }))}
                              >
                                Ver o histórico deste registro
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    )}
                  </Fragment>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
      {dados && dados.total > 0 && (
        <BarraDePaginacao
          pagina={dados}
          oQue="eventos"
          aoTrocarPagina={(nova) => {
            setPagina(nova);
            setAberto(null);
          }}
          aoTrocarTamanho={(novo) => mudar(() => setTamanho(novo))}
        />
      )}

      <div className="config-hint aud-nota">
        A trilha é gravada junto com o dado, na mesma transação, e ninguém a altera. Registra os campos que alguém pode
        querer contestar — dono, situação, valores, acessos —, não os carimbos nem o que é recalculado por rotina. Nenhuma
        rotina apaga a trilha hoje; o prazo de retenção ainda será decidido.
      </div>
    </CardConfig>
  );
}
