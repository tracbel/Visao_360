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
 *
 * 29/09/2026 — NO PADRÃO DOS INDICADORES GEOGRÁFICOS (#293, bloco 5): página na coluna inteira, cabeçalho com a hora da
 * leitura e o reler, a busca e os filtros na barra do padrão e a tabela num painel da seção. Nenhum cartão foi inventado:
 * a lista não tinha número de decisão, e continua sem. Nenhum texto mudou.
 */

import { ListFilter, RefreshCw, Search, Users } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { BarraDePaginacao } from '../../componentes/cadastro/BarraDePaginacao';
import { BotaoDeNovoCadastro } from '../../componentes/cadastro/BotaoDeNovoCadastro';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../../componentes/cadastro/SeloProcedencia';
import { PaginaDoPainel } from '../../componentes/dashboard/Dashboard';
import { PainelDoMomento } from '../../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../../componentes/territorio/TituloDaSecao';
import { itensDe, useCatalogos } from '../../dados/api/catalogos';
import { CONSULTA_INICIAL, listarClientes } from '../../dados/api/clientes';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { CATALOGO, type ConsultaDeClientes, type OrdemDeCliente } from '../../tipos/api';
import { formatarDataHora } from './formato';
import '../../estilos/dashboard.css';
import '../../estilos/momento.css';
import '../../estilos/territorio.css';

/** As colunas que a API sabe ordenar. O resto é exibição. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeCliente; numerica?: boolean }[] = [
  { rotulo: 'Cliente', ordem: 'Nome' },
  { rotulo: 'Documento' },
  { rotulo: 'Tipo' },
  { rotulo: 'Situação', ordem: 'Situacao' },
  { rotulo: 'Cadastrado em', ordem: 'CriadoEm', numerica: true },
  { rotulo: 'Última alteração', ordem: 'AlteradoEm', numerica: true },
];

/** A ordem da lista, dita como a tela a chama. */
const ROTULO_DA_ORDEM: Record<OrdemDeCliente, string> = {
  Nome: 'nome',
  Situacao: 'situação',
  CriadoEm: 'data de cadastro',
  AlteradoEm: 'última alteração',
};

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
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Clientes</h1>
          <p className="page-subtitle">
            Cadastro do CRM · a filial do cabeçalho define o que aparece aqui e o que pode ser gravado
          </p>
        </div>
        <div className="page-actions">
          <p className="dash-atualizado">
            {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} /> : 'Lendo os clientes…'}
            <button
              type="button"
              className="dash-recarregar"
              onClick={leitura.recarregar}
              disabled={leitura.carregando}
              data-carregando={leitura.carregando ? 'true' : 'false'}
              aria-label="Reler os clientes"
            >
              <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
            </button>
          </p>
          <BotaoDeNovoCadastro para="/clientes/novo" rotulo="Novo cliente" />
        </div>
      </div>

      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="busca">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Search size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Buscar cliente</span>
              <input
                type="search"
                aria-label="Buscar cliente por nome, nome fantasia ou documento"
                placeholder="Buscar por nome, nome fantasia ou documento…"
                value={termoDigitado}
                onChange={(e) => setTermoDigitado(e.target.value)}
              />
            </span>
          </label>

          <label className="dash-filtro" data-bloco="situacao">
            <span className="dash-filtro-icone" aria-hidden="true">
              <ListFilter size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Situação</span>
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
            </span>
          </label>

          <label className="dash-filtro" data-bloco="tipo-de-pessoa">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Users size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Tipo de pessoa</span>
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
            </span>
          </label>

          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input
                type="checkbox"
                checked={consulta.incluirInativos}
                onChange={(e) => setConsulta((c) => ({ ...c, incluirInativos: e.target.checked, pagina: 1 }))}
              />
              Mostrar inativados
            </label>
          </div>

          {temFiltro && (
            <div className="dash-filtros-acao">
              <button type="button" className="dash-mais-filtros" onClick={limparFiltros}>
                Limpar filtros
              </button>
            </div>
          )}
        </div>
      </div>

      <AvisoDeProcedencia procedencia={leitura.procedencia} />

      <section className="dash-secao" data-bloco="secao-clientes">
        <TituloDaSecao
          titulo="Os clientes"
          subtitulo={`Ordenados por ${ROTULO_DA_ORDEM[consulta.ordenarPor] ?? consulta.ordenarPor}${consulta.descendente ? ', do maior para o menor' : ''}.`}
        />

        <PainelDoMomento
          titulo="Clientes desta filial"
          data-bloco="clientes"
          subtitulo={leitura.recarregando ? 'Atualizando…' : `${pagina?.total ?? 0} no total`}
          dica="Clique no título de uma coluna com seta para ordenar por ela. O nome do cliente, ou o Abrir, abre a ficha; o duplo clique na linha também."
        >
          {leitura.carregando && <BlocoCarregando oQue="os clientes" />}
          {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

          {pagina && !leitura.erro && pagina.itens.length === 0 && (
            <BlocoVazio
              titulo={temFiltro ? 'Nenhum cliente com esses filtros' : 'Esta filial ainda não tem clientes'}
              texto={
                temFiltro
                  ? 'A busca compara nome e nome fantasia por trecho, e o documento por valor inteiro — pedaço de CNPJ não encontra.'
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
              <div className="mom-tabela-rolagem">
                <table className="mom-tabela">
                  <caption className="cad-so-leitor">
                    Clientes da filial {contexto.empresa}, ordenados por {consulta.ordenarPor}
                  </caption>
                  <thead>
                    <tr>
                      {COLUNAS.map((coluna) => (
                        <th
                          key={coluna.rotulo}
                          scope="col"
                          className={coluna.numerica ? 'mom-num' : undefined}
                          aria-sort={ariaOrdem(coluna.ordem, consulta)}
                        >
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
                      <th scope="col" className="mom-acoes">
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
                        <th scope="row">
                          <Link to={`/clientes/${cliente.chave}`} className="cad-link-forte">
                            {cliente.nomeRazao}
                          </Link>
                          {cliente.nomeFantasia && <div className="cad-sub">{cliente.nomeFantasia}</div>}
                        </th>
                        <td className="cad-mono">{cliente.documento ?? <span className="cad-vazio">—</span>}</td>
                        <td>{cliente.tipoDePessoa === 'Fisica' ? 'Física' : 'Jurídica'}</td>
                        <td>
                          <span className={`cad-selo cad-selo-${cliente.situacao.toLowerCase()}`}>
                            {cliente.situacao}
                          </span>
                          {cliente.estaInativo && <span className="cad-selo cad-selo-inativo">inativado</span>}
                        </td>
                        <td className="mom-num">{formatarDataHora(cliente.criadoEm)}</td>
                        <td className="mom-num">
                          {cliente.alteradoEm ? formatarDataHora(cliente.alteradoEm) : <span className="cad-vazio">—</span>}
                        </td>
                        <td className="mom-acoes">
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
        </PainelDoMomento>
      </section>
    </PaginaDoPainel>
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
