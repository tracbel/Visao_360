/**
 * Equipamentos — a lista, ligada à nossa API.
 *
 * ESTA TELA NÃO EXISTIA. Até aqui, `/equipamentos` era um marcador de 45 linhas
 * dizendo "envie os detalhes desta tela" — herdado do protótipo, onde a rota
 * também é um `renderPlaceholder()`. Agora ela lista `frota.Equipamento` da
 * filial do contexto, no mesmo padrão da lista de Clientes.
 *
 * DUAS COISAS QUE A API AINDA NÃO FAZ, e que a tela declara em vez de esconder:
 *
 * 1. **Busca por pedaço de chassi não funciona.** O `termo` compara o chassi por
 *    VALOR INTEIRO (17 caracteres) e o número de série e a placa por trecho. É a
 *    dívida D-2 do documento 23, e o estado vazio diz isso — senão o CEN que
 *    digita os últimos dígitos conclui que a máquina não está cadastrada.
 * 2. **Não há filtro de modelo na consulta.** Marca, família e modelo são
 *    filtrados AQUI, sobre as linhas lidas. Quando o filtro está ligado, a tela
 *    lê o teto da API (200 linhas) de uma vez e avisa se o total passar disso —
 *    filtrar em silêncio sobre uma página só faria a contagem mentir.
 */

import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { BarraDePaginacao } from '../../componentes/cadastro/BarraDePaginacao';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, SeloProcedencia } from '../../componentes/cadastro/SeloProcedencia';
import { distintosDe, itensDe, modelosDeFrota, useCatalogos } from '../../dados/api/catalogos';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { CONSULTA_INICIAL, listarEquipamentos } from '../../dados/api/equipamentos';
import { useRecurso } from '../../dados/api/useRecurso';
import {
  CATALOGO,
  type ConsultaDeEquipamentos,
  type EquipamentoResumo,
  type OrdemDeEquipamento,
  type PaginaDe,
} from '../../tipos/api';
import { formatarDataHora } from './formato';

/** O teto de linhas por página da API. Pedir mais é recusado, e com razão. */
const TETO_DA_API = 200;

const ESPERA_DA_BUSCA_MS = 350;

const COLUNAS: { rotulo: string; ordem?: OrdemDeEquipamento }[] = [
  { rotulo: 'Chassi', ordem: 'Chassi' },
  { rotulo: 'Modelo' },
  { rotulo: 'Ano', ordem: 'AnoModelo' },
  { rotulo: 'Situação', ordem: 'Situacao' },
  { rotulo: 'Origem' },
  { rotulo: 'Cliente' },
  { rotulo: 'Cadastrado em', ordem: 'CriadoEm' },
];

/** O filtro de frota, que a API não conhece e a tela aplica. */
type FiltroDeFrota = { marca: string; familia: string; modelo: string };

const FROTA_VAZIA: FiltroDeFrota = { marca: '', familia: '', modelo: '' };

export function EquipamentosLista() {
  const { contexto } = useContextoDeAcesso();
  const navegar = useNavigate();
  const { catalogos } = useCatalogos(contexto);

  const [consulta, setConsulta] = useState<ConsultaDeEquipamentos>(CONSULTA_INICIAL);
  const [termoDigitado, setTermoDigitado] = useState('');
  const [frota, setFrota] = useState<FiltroDeFrota>(FROTA_VAZIA);

  const filtrandoFrota = frota.marca !== '' || frota.familia !== '' || frota.modelo !== '';

  useEffect(() => {
    const relogio = setTimeout(
      () => setConsulta((c) => (c.termo === termoDigitado ? c : { ...c, termo: termoDigitado, pagina: 1 })),
      ESPERA_DA_BUSCA_MS,
    );
    return () => clearTimeout(relogio);
  }, [termoDigitado]);

  // Com o filtro de frota ligado, a paginação passa a ser da tela: pedir uma
  // página de 25 e filtrar dentro dela devolveria três linhas na página 1 e
  // trinta na página 2, com um total que não corresponde a nada.
  const consultaEnviada: ConsultaDeEquipamentos = filtrandoFrota
    ? { ...consulta, pagina: 1, tamanho: TETO_DA_API }
    : consulta;

  const leitura = useRecurso(
    (sinal) => listarEquipamentos(contexto, consultaEnviada, sinal),
    [contexto.empresa, contexto.usuario, JSON.stringify(consultaEnviada)],
  );

  const modelos = useMemo(() => modelosDeFrota(catalogos), [catalogos]);
  const porCodigo = useMemo(() => new Map(modelos.map((m) => [m.codigo, m])), [modelos]);

  const marcas = useMemo(() => distintosDe(modelos, 'marca'), [modelos]);
  const familias = useMemo(
    () => distintosDe(frota.marca ? modelos.filter((m) => m.marca === frota.marca) : modelos, 'familia'),
    [modelos, frota.marca],
  );
  const modelosOferecidos = useMemo(
    () =>
      modelos.filter(
        (m) => (!frota.marca || m.marca === frota.marca) && (!frota.familia || m.familia === frota.familia),
      ),
    [modelos, frota.marca, frota.familia],
  );

  const situacoes = itensDe(catalogos, CATALOGO.situacaoEquipamento);
  const origens = itensDe(catalogos, CATALOGO.origemEquipamento);

  const temFiltro =
    consulta.termo !== '' ||
    consulta.situacao !== '' ||
    consulta.origem !== '' ||
    consulta.incluirInativos ||
    filtrandoFrota;

  /** Aplica o filtro de frota e repagina na tela, quando ele está ligado. */
  const pagina: PaginaDe<EquipamentoResumo> | null = useMemo(() => {
    const lida = leitura.dados;
    if (!lida) return null;
    if (!filtrandoFrota) return lida;

    const filtradas = lida.itens.filter((linha) => {
      const modelo = linha.modeloCodigo ? porCodigo.get(linha.modeloCodigo) : undefined;
      if (frota.modelo && linha.modeloCodigo !== frota.modelo) return false;
      if (frota.marca && (modelo?.marca ?? linha.marca ?? '') !== frota.marca) return false;
      if (frota.familia && (modelo?.familia ?? '') !== frota.familia) return false;
      return true;
    });

    const inicio = (consulta.pagina - 1) * consulta.tamanho;
    return {
      itens: filtradas.slice(inicio, inicio + consulta.tamanho),
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      total: filtradas.length,
      totalDePaginas: Math.max(Math.ceil(filtradas.length / consulta.tamanho), 1),
      temProxima: inicio + consulta.tamanho < filtradas.length,
    };
  }, [leitura.dados, filtrandoFrota, frota, porCodigo, consulta.pagina, consulta.tamanho]);

  const passouDoTeto = filtrandoFrota && (leitura.dados?.total ?? 0) > TETO_DA_API;

  function trocarOrdem(campo: OrdemDeEquipamento) {
    setConsulta((c) =>
      c.ordenarPor === campo
        ? { ...c, descendente: !c.descendente, pagina: 1 }
        : { ...c, ordenarPor: campo, descendente: false, pagina: 1 },
    );
  }

  function limparFiltros() {
    setTermoDigitado('');
    setFrota(FROTA_VAZIA);
    setConsulta({ ...CONSULTA_INICIAL, tamanho: consulta.tamanho });
  }

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Equipamentos</h1>
          <p className="page-subtitle">
            Parque de máquinas do CRM · inclusive as de concorrente, que é o que a Cobertura usa
          </p>
        </div>
        <div className="page-actions">
          <Link to="/equipamentos/novo" className="btn btn-primary">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Novo equipamento
          </Link>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={leitura.procedencia} />

      <div className="cad-barra">
        <div className="cad-busca">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            type="search"
            aria-label="Buscar equipamento por chassi, número de série ou placa"
            placeholder="Chassi completo, número de série ou placa…"
            value={termoDigitado}
            onChange={(e) => setTermoDigitado(e.target.value)}
          />
        </div>

        <label className="cad-filtro">
          Marca
          <select
            value={frota.marca}
            onChange={(e) => {
              setFrota({ marca: e.target.value, familia: '', modelo: '' });
              setConsulta((c) => ({ ...c, pagina: 1 }));
            }}
          >
            <option value="">Todas</option>
            {marcas.map((m) => (
              <option key={m} value={m}>
                {m}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro">
          Família
          <select
            value={frota.familia}
            onChange={(e) => {
              setFrota((f) => ({ ...f, familia: e.target.value, modelo: '' }));
              setConsulta((c) => ({ ...c, pagina: 1 }));
            }}
          >
            <option value="">Todas</option>
            {familias.map((f) => (
              <option key={f} value={f}>
                {f}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro">
          Modelo
          <select
            value={frota.modelo}
            onChange={(e) => {
              setFrota((f) => ({ ...f, modelo: e.target.value }));
              setConsulta((c) => ({ ...c, pagina: 1 }));
            }}
          >
            <option value="">Todos</option>
            {modelosOferecidos.map((m) => (
              <option key={m.codigo} value={m.codigo}>
                {m.modelo}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro">
          Situação
          <select
            value={consulta.situacao}
            onChange={(e) => setConsulta((c) => ({ ...c, situacao: e.target.value, pagina: 1 }))}
          >
            <option value="">Todas</option>
            {situacoes.map((s) => (
              <option key={s.codigo} value={s.codigo}>
                {s.descricao}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro">
          Origem
          <select
            value={consulta.origem}
            onChange={(e) => setConsulta((c) => ({ ...c, origem: e.target.value, pagina: 1 }))}
          >
            <option value="">Todas</option>
            {origens.map((o) => (
              <option key={o.codigo} value={o.codigo}>
                {o.descricao}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro cad-filtro-caixa">
          <input
            type="checkbox"
            checked={consulta.incluirInativos}
            onChange={(e) => setConsulta((c) => ({ ...c, incluirInativos: e.target.checked, pagina: 1 }))}
          />
          Mostrar baixados
        </label>

        {temFiltro && (
          <button type="button" className="btn btn-secondary btn-sm" onClick={limparFiltros}>
            Limpar filtros
          </button>
        )}
      </div>

      {passouDoTeto && (
        <div className="cad-aviso cad-aviso-atencao" role="status">
          <strong>O filtro de marca, família e modelo é aplicado na tela.</strong>
          <p>
            Esta filial tem {leitura.dados?.total} equipamentos e a API entrega no máximo {TETO_DA_API} por
            leitura — o filtro cobre só essas {TETO_DA_API} primeiras linhas. Estreite a busca ou a situação
            para caber. A consulta por modelo ainda não existe na API (documento 23, seção 2.2).
          </p>
        </div>
      )}

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Máquinas desta filial</div>
            <div className="card-subtitle">
              {leitura.recarregando ? 'Atualizando…' : `${pagina?.total ?? 0} no total`}
            </div>
          </div>
          <SeloProcedencia procedencia={leitura.procedencia} />
        </div>

        {leitura.carregando && <BlocoCarregando oQue="os equipamentos" />}
        {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

        {pagina && !leitura.erro && pagina.itens.length === 0 && (
          <BlocoVazio
            titulo={temFiltro ? 'Nenhuma máquina com esses filtros' : 'Esta filial ainda não tem máquinas cadastradas'}
            texto={
              temFiltro ? (
                <>
                  A busca compara o <strong>chassi inteiro</strong> — os 17 caracteres da plaqueta. Pedaço de
                  chassi ainda não encontra: é a dívida D-2 do documento 23. Número de série e placa são
                  comparados por trecho.
                </>
              ) : (
                'Cadastre a primeira máquina — inclusive a do concorrente, que é o que alimenta a Cobertura de Carteira.'
              )
            }
            acao={
              temFiltro ? (
                <button type="button" className="btn btn-secondary" onClick={limparFiltros}>
                  Limpar filtros
                </button>
              ) : (
                <Link to="/equipamentos/novo" className="btn btn-primary">
                  Cadastrar o primeiro equipamento
                </Link>
              )
            }
          />
        )}

        {pagina && pagina.itens.length > 0 && (
          <>
            <div className="cad-tabela-wrap">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">
                  Equipamentos da filial {contexto.empresa}, ordenados por {consulta.ordenarPor}
                </caption>
                <thead>
                  <tr>
                    {COLUNAS.map((coluna) => (
                      <th key={coluna.rotulo} scope="col" aria-sort={ariaOrdem(coluna.ordem, consulta)}>
                        {coluna.ordem ? (
                          <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem!)}>
                            {coluna.rotulo}
                            <span aria-hidden="true">{seta(coluna.ordem, consulta)}</span>
                          </button>
                        ) : (
                          coluna.rotulo
                        )}
                      </th>
                    ))}
                    <th scope="col" className="cad-col-acoes">
                      <span className="cad-so-leitor">Ações</span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {pagina.itens.map((maquina) => {
                    const modelo = maquina.modeloCodigo ? porCodigo.get(maquina.modeloCodigo) : undefined;
                    return (
                      <tr
                        key={maquina.chave}
                        className={maquina.estaInativo ? 'cad-linha-inativa' : undefined}
                        onDoubleClick={() => navegar(`/equipamentos/${maquina.chave}`)}
                      >
                        <td className="cad-mono">
                          <Link to={`/equipamentos/${maquina.chave}`} className="cad-link-forte">
                            {maquina.chassi}
                          </Link>
                        </td>
                        <td>
                          {maquina.modeloNome ?? <span className="cad-vazio">sem modelo</span>}
                          <div className="cad-sub">
                            {maquina.marca ?? '—'}
                            {modelo?.familia ? ` · ${modelo.familia}` : ''}
                            {maquina.marcaRepresentada === false && (
                              <span className="cad-selo cad-selo-concorrente">concorrente</span>
                            )}
                          </div>
                        </td>
                        <td className="cad-mono">{maquina.anoModelo ?? <span className="cad-vazio">—</span>}</td>
                        <td>
                          <span className={`cad-selo cad-selo-${maquina.situacao.toLowerCase()}`}>
                            {maquina.situacao}
                          </span>
                          {maquina.estaInativo && <span className="cad-selo cad-selo-inativo">baixado</span>}
                        </td>
                        <td>{maquina.origem}</td>
                        <td>
                          {maquina.clienteChave ? (
                            <Link to={`/clientes/${maquina.clienteChave}`}>{maquina.clienteNome}</Link>
                          ) : (
                            <span className="cad-vazio">sem dono</span>
                          )}
                        </td>
                        <td className="cad-mono">{formatarDataHora(maquina.criadoEm)}</td>
                        <td className="cad-col-acoes">
                          <Link to={`/equipamentos/${maquina.chave}`} className="btn btn-secondary btn-sm">
                            Abrir
                          </Link>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            <BarraDePaginacao
              pagina={pagina}
              oQue="equipamentos"
              aoTrocarPagina={(p) => setConsulta((c) => ({ ...c, pagina: p }))}
              aoTrocarTamanho={(t) => setConsulta((c) => ({ ...c, tamanho: t, pagina: 1 }))}
            />
          </>
        )}
      </div>
    </>
  );
}

function ariaOrdem(campo: OrdemDeEquipamento | undefined, consulta: ConsultaDeEquipamentos) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

function seta(campo: OrdemDeEquipamento | undefined, consulta: ConsultaDeEquipamentos) {
  if (!campo || consulta.ordenarPor !== campo) return '';
  return consulta.descendente ? '▾' : '▴';
}
