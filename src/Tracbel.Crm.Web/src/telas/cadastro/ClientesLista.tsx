/**
 * Clientes — a lista, ligada à nossa API.
 *
 * O QUE MUDOU EM RELAÇÃO AO PORTE DO PROTÓTIPO: esta tela não junta mais três
 * arquivos JSON em memória para filtrar e ordenar no navegador. Busca, filtro,
 * ordenação e paginação são PARÂMETROS DA CONSULTA, e quem responde é
 * `/api/v1/clientes` — dentro da fronteira da filial escolhida no cabeçalho.
 *
 * DUAS CONSEQUÊNCIAS QUE APARECEM NA TELA:
 *
 * - **`ordenarPor` é domínio fechado.** Nome, CriadoEm, Situacao e AlteradoEm, e
 *   nada mais. Coluna que a API não sabe ordenar não vira cabeçalho clicável —
 *   melhor não oferecer do que oferecer e a ordem não mudar.
 * - **O total é o desta filial.** Trocar a filial no cabeçalho muda a lista e o
 *   total, porque o filtro global do `CrmDbContext` roda em toda consulta.
 */

import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { BarraDePaginacao } from '../../componentes/cadastro/BarraDePaginacao';
import { BotaoDeNovoCadastro } from '../../componentes/cadastro/BotaoDeNovoCadastro';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, SeloProcedencia } from '../../componentes/cadastro/SeloProcedencia';
import { itensDe, useCatalogos } from '../../dados/api/catalogos';
import { CONSULTA_INICIAL, listarClientes } from '../../dados/api/clientes';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { CATALOGO, type ConsultaDeClientes, type OrdemDeCliente } from '../../tipos/api';
import { formatarDataHora } from './formato';

/** As colunas que a API sabe ordenar. O resto é exibição. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeCliente; alinhamento?: 'right' }[] = [
  { rotulo: 'Cliente', ordem: 'Nome' },
  { rotulo: 'Documento' },
  { rotulo: 'Tipo' },
  { rotulo: 'Situação', ordem: 'Situacao' },
  { rotulo: 'Cadastrado em', ordem: 'CriadoEm' },
  { rotulo: 'Última alteração', ordem: 'AlteradoEm' },
];

/** Espera antes de mandar a busca à API, para não consultar a cada tecla. */
const ESPERA_DA_BUSCA_MS = 350;

export function ClientesLista() {
  const { contexto } = useContextoDeAcesso();
  const navegar = useNavigate();
  const { catalogos } = useCatalogos(contexto);

  // A BUSCA DO CABEÇALHO ENTRA POR AQUI. `?busca=` é o caminho que a busca
  // global do `Layout` usa para mandar o termo a esta tela — sem ele, a pessoa
  // digitaria duas vezes: uma lá em cima e outra aqui. Só o valor inicial vem
  // da rota; a partir daí quem manda é o campo.
  const [parametros] = useSearchParams();
  const buscaDaRota = parametros.get('busca') ?? '';

  const [consulta, setConsulta] = useState<ConsultaDeClientes>(() =>
    buscaDaRota ? { ...CONSULTA_INICIAL, termo: buscaDaRota } : CONSULTA_INICIAL,
  );
  const [termoDigitado, setTermoDigitado] = useState(buscaDaRota);

  // Buscar de novo pelo cabeçalho, já estando nesta tela, troca o termo.
  useEffect(() => {
    if (buscaDaRota) setTermoDigitado(buscaDaRota);
  }, [buscaDaRota]);

  // O que está no campo e o que foi pedido à API são coisas diferentes: sem essa
  // separação, cada tecla vira uma requisição e a lista pisca a cada letra.
  useEffect(() => {
    const relogio = setTimeout(
      () => setConsulta((c) => (c.termo === termoDigitado ? c : { ...c, termo: termoDigitado, pagina: 1 })),
      ESPERA_DA_BUSCA_MS,
    );
    return () => clearTimeout(relogio);
  }, [termoDigitado]);

  const leitura = useRecurso(
    (sinal) => listarClientes(contexto, consulta, sinal),
    [contexto.empresa, contexto.usuario, JSON.stringify(consulta)],
  );

  const situacoes = itensDe(catalogos, CATALOGO.situacaoCliente);
  const tipos = itensDe(catalogos, CATALOGO.tipoDePessoa);
  const temFiltro = useMemo(
    () =>
      consulta.termo !== '' ||
      consulta.situacao !== '' ||
      consulta.tipoDePessoa !== '' ||
      consulta.incluirInativos,
    [consulta],
  );

  function trocarOrdem(campo: OrdemDeCliente) {
    setConsulta((c) =>
      c.ordenarPor === campo
        ? { ...c, descendente: !c.descendente, pagina: 1 }
        : { ...c, ordenarPor: campo, descendente: false, pagina: 1 },
    );
  }

  function limparFiltros() {
    setTermoDigitado('');
    setConsulta({ ...CONSULTA_INICIAL, tamanho: consulta.tamanho });
  }

  const pagina = leitura.dados;

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Clientes</h1>
          <p className="page-subtitle">
            Cadastro do CRM · a filial do cabeçalho define o que aparece aqui e o que pode ser gravado
          </p>
        </div>
        <div className="page-actions">
          <BotaoDeNovoCadastro para="/clientes/novo" rotulo="Novo cliente" />
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
            aria-label="Buscar cliente por nome, nome fantasia ou documento"
            placeholder="Buscar por nome, nome fantasia ou documento…"
            value={termoDigitado}
            onChange={(e) => setTermoDigitado(e.target.value)}
          />
        </div>

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
          Tipo de pessoa
          <select
            value={consulta.tipoDePessoa}
            onChange={(e) => setConsulta((c) => ({ ...c, tipoDePessoa: e.target.value, pagina: 1 }))}
          >
            <option value="">Todos</option>
            {tipos.map((t) => (
              <option key={t.codigo} value={t.codigo}>
                {t.descricao}
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
          Mostrar inativados
        </label>

        {temFiltro && (
          <button type="button" className="btn btn-secondary btn-sm" onClick={limparFiltros}>
            Limpar filtros
          </button>
        )}
      </div>

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Clientes desta filial</div>
            <div className="card-subtitle">
              {leitura.recarregando ? 'Atualizando…' : `${pagina?.total ?? 0} no total`}
            </div>
          </div>
          <SeloProcedencia procedencia={leitura.procedencia} />
        </div>

        {leitura.carregando && <BlocoCarregando oQue="os clientes" />}
        {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

        {pagina && !leitura.erro && pagina.itens.length === 0 && (
          <BlocoVazio
            titulo={temFiltro ? 'Nenhum cliente com esses filtros' : 'Esta filial ainda não tem clientes'}
            texto={
              temFiltro
                ? 'A busca compara nome e nome fantasia por trecho, e o documento por valor inteiro — pedaço de CNPJ não encontra (dívida D-2 do documento 23).'
                : 'O cadastro nasce na filial escolhida no cabeçalho. Cadastre o primeiro para a lista aparecer.'
            }
            acao={
              temFiltro ? (
                <button type="button" className="btn btn-secondary" onClick={limparFiltros}>
                  Limpar filtros
                </button>
              ) : (
                <Link to="/clientes/novo" className="btn btn-primary">
                  Cadastrar o primeiro cliente
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
                  Clientes da filial {contexto.empresa}, ordenados por {consulta.ordenarPor}
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
                  {pagina.itens.map((cliente) => (
                    <tr
                      key={cliente.chave}
                      className={cliente.estaInativo ? 'cad-linha-inativa' : undefined}
                      onDoubleClick={() => navegar(`/clientes/${cliente.chave}`)}
                    >
                      <td>
                        <Link to={`/clientes/${cliente.chave}`} className="cad-link-forte">
                          {cliente.nomeRazao}
                        </Link>
                        {cliente.nomeFantasia && <div className="cad-sub">{cliente.nomeFantasia}</div>}
                      </td>
                      <td className="cad-mono">{cliente.documento ?? <span className="cad-vazio">—</span>}</td>
                      <td>{cliente.tipoDePessoa === 'Fisica' ? 'Física' : 'Jurídica'}</td>
                      <td>
                        <span className={`cad-selo cad-selo-${cliente.situacao.toLowerCase()}`}>
                          {cliente.situacao}
                        </span>
                        {cliente.estaInativo && <span className="cad-selo cad-selo-inativo">inativado</span>}
                      </td>
                      <td className="cad-mono">{formatarDataHora(cliente.criadoEm)}</td>
                      <td className="cad-mono">
                        {cliente.alteradoEm ? formatarDataHora(cliente.alteradoEm) : <span className="cad-vazio">—</span>}
                      </td>
                      <td className="cad-col-acoes">
                        <Link to={`/clientes/${cliente.chave}`} className="btn btn-secondary btn-sm">
                          Abrir
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <BarraDePaginacao
              pagina={pagina}
              oQue="clientes"
              aoTrocarPagina={(p) => setConsulta((c) => ({ ...c, pagina: p }))}
              aoTrocarTamanho={(t) => setConsulta((c) => ({ ...c, tamanho: t, pagina: 1 }))}
            />
          </>
        )}
      </div>
    </>
  );
}

function ariaOrdem(campo: OrdemDeCliente | undefined, consulta: ConsultaDeClientes) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

function seta(campo: OrdemDeCliente | undefined, consulta: ConsultaDeClientes) {
  if (!campo || consulta.ordenarPor !== campo) return '';
  return consulta.descendente ? '▾' : '▴';
}
