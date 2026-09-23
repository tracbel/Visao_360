/**
 * Os filtros da tela de Indicadores Geográficos (issue 170, parte A).
 *
 * OS FILTROS SEM DADO APARECEM, DESLIGADOS. Sumir com eles faria a tela parecer
 * completa; mostrá-los com o motivo diz o que falta para respondê-los, que é o
 * pedido do documento 32.
 */

import type { Dispatch, SetStateAction } from 'react';
import type {
  FiltrosTerritoriais,
  IndicadoresTerritoriais,
  RegraDePotencialAplicada,
} from '../../tipos/territorio';
import { anoCivilFechado, mes } from './indicadoresDaAdr';

export function FiltrosDosIndicadores({
  filtros,
  aoMudarFiltros,
  lojasConhecidas,
  regra,
  indicadores,
  respondeu,
  podeVerEmpresaInteira,
}: {
  filtros: FiltrosTerritoriais;
  aoMudarFiltros: Dispatch<SetStateAction<FiltrosTerritoriais>>;
  lojasConhecidas: Map<string, string>;
  regra: RegraDePotencialAplicada | null;
  indicadores: IndicadoresTerritoriais | null;
  /** Se a leitura já respondeu — antes disso a tela não afirma nada sobre permissão. */
  respondeu: boolean;
  podeVerEmpresaInteira: boolean;
}) {
  const anoCivil = anoCivilFechado();
  const presetDoPeriodo =
    filtros.competenciaInicial === '' && filtros.competenciaFinal === ''
      ? '12meses'
      : filtros.competenciaInicial === anoCivil.competenciaInicial && filtros.competenciaFinal === anoCivil.competenciaFinal
        ? 'anoCivil'
        : 'personalizado';

  return (
    <div className="card cad-cartao terr-cartao" data-bloco="filtros">
      <div className="terr-filtros">
        <div className="terr-periodo" role="group" aria-label="Período das vendas">
          Período das vendas
          <div className="terr-alternador">
            <button
              type="button"
              aria-pressed={presetDoPeriodo === '12meses'}
              onClick={() => aoMudarFiltros((f) => ({ ...f, competenciaInicial: '', competenciaFinal: '' }))}
            >
              12 meses fechados
            </button>
            <button
              type="button"
              aria-pressed={presetDoPeriodo === 'anoCivil'}
              onClick={() => aoMudarFiltros((f) => ({ ...f, ...anoCivil }))}
            >
              Ano civil até o último mês fechado
            </button>
            <button type="button" aria-pressed={presetDoPeriodo === 'personalizado'} disabled={presetDoPeriodo !== 'personalizado'}>
              Personalizado
            </button>
          </div>
          <span className="terr-filtro-motivo">
            FYTD não é oferecido: o calendário fiscal não foi confirmado (documento 32, P-4). O mês em curso fica fora do padrão.
          </span>
        </div>
        <label className="terr-filtro">
          Vendas de
          <input
            type="month"
            value={filtros.competenciaInicial}
            onChange={(e) => aoMudarFiltros((f) => ({ ...f, competenciaInicial: e.target.value }))}
          />
        </label>
        <label className="terr-filtro">
          até
          <input
            type="month"
            value={filtros.competenciaFinal}
            onChange={(e) => aoMudarFiltros((f) => ({ ...f, competenciaFinal: e.target.value }))}
          />
        </label>
        <label className="terr-filtro">
          Região da ADR
          <select
            value={filtros.regiao}
            onChange={(e) => aoMudarFiltros((f) => ({ ...f, regiao: e.target.value as FiltrosTerritoriais['regiao'] }))}
          >
            <option value="">Norte e Noroeste</option>
            <option value="Norte">Norte</option>
            <option value="Noroeste">Noroeste</option>
          </select>
        </label>
        <label className="terr-filtro">
          Loja responsável
          <select value={filtros.lojaCodigo} onChange={(e) => aoMudarFiltros((f) => ({ ...f, lojaCodigo: e.target.value }))}>
            <option value="">Todas</option>
            {[...lojasConhecidas.entries()]
              .sort((a, b) => a[1].localeCompare(b[1]))
              .map(([codigo, nome]) => (
                <option key={codigo} value={codigo}>
                  {nome}
                </option>
              ))}
          </select>
        </label>
        <label className="terr-filtro">
          Visão
          <select
            value={filtros.visao}
            onChange={(e) => aoMudarFiltros((f) => ({ ...f, visao: e.target.value as FiltrosTerritoriais['visao'] }))}
          >
            <option value="Filial">Filial do cabeçalho</option>
            <option value="Empresa" disabled={respondeu ? !podeVerEmpresaInteira : false}>
              Empresa inteira
            </option>
          </select>
          {respondeu && !podeVerEmpresaInteira && (
            <span className="terr-filtro-motivo">empresa inteira: seu perfil não tem a permissão</span>
          )}
        </label>
        <FiltroDeFilial
          rotulo="Filial que vendeu"
          aplicaA="só às vendas"
          valor={filtros.filialDaVenda}
          lojas={lojasConhecidas}
          aoMudar={(valor) => aoMudarFiltros((f) => ({ ...f, filialDaVenda: valor }))}
        />
        <FiltroDeFilial
          rotulo="Filial de cadastro do cliente"
          aplicaA="à cobertura e às vendas"
          valor={filtros.filialDoCliente}
          lojas={lojasConhecidas}
          aoMudar={(valor) => aoMudarFiltros((f) => ({ ...f, filialDoCliente: valor }))}
        />
        <label className="terr-filtro">
          Cultura da regra
          <select value={regra?.produtoCodigoIbge ?? ''} disabled={!regra}>
            {regra && <option value={regra.produtoCodigoIbge}>{regra.produtoNome}</option>}
          </select>
          <span className="terr-filtro-motivo">só há uma regra informada</span>
        </label>
        <FiltroSemDado rotulo="Tipo de cliente" opcoes="SAM · KAM · Varejo" motivo="não há classificação por cliente" />
        <FiltroSemDado rotulo="Tipo de produto" opcoes="colhedora · trator grande · médio" motivo="vendas sem item da nota" />
        <FiltroSemDado rotulo="Modelo" opcoes="modelo da máquina" motivo="vendas sem item da nota" />
        <FiltroSemDado
          rotulo="CEN / gestor"
          opcoes="—"
          motivo="a carteira do CRM ainda não diz quem atende cada município (issue 107); o que ela já tem está na ficha do município"
        />
      </div>
      {indicadores && (
        <p className="cad-sub">
          Vendas de <strong>{mes(indicadores.competenciaInicial)}</strong> a{' '}
          <strong>{mes(indicadores.competenciaFinal)}</strong>
          {presetDoPeriodo === '12meses' && ' (12 meses fechados)'}
          {presetDoPeriodo === 'anoCivil' && ' (ano civil até o último mês fechado)'} · cobertura medida em{' '}
          {new Date(indicadores.referenciaDaCobertura).toLocaleDateString('pt-BR')}, interação mais recente carregada em{' '}
          {indicadores.interacaoMaisRecente ? new Date(indicadores.interacaoMaisRecente).toLocaleDateString('pt-BR') : '—'}
          {indicadores.anoDaAreaPlantada && ` · área plantada PAM/IBGE ${indicadores.anoDaAreaPlantada}`}.
        </p>
      )}
    </div>
  );
}

/** Um filtro pedido que o dado não sustenta: aparece, desligado, dizendo por quê. */
function FiltroSemDado({ rotulo, opcoes, motivo }: { rotulo: string; opcoes: string; motivo: string }) {
  return (
    <label className="terr-filtro">
      {rotulo}
      <select disabled title={motivo}>
        <option>{opcoes}</option>
      </select>
      <span className="terr-filtro-motivo">sem dado: {motivo}</span>
    </label>
  );
}

/** Uma filial para filtrar — pelas lojas que a ADR já mostrou, com o que o filtro alcança. */
function FiltroDeFilial({
  rotulo,
  aplicaA,
  valor,
  lojas,
  aoMudar,
}: {
  rotulo: string;
  aplicaA: string;
  valor: string;
  lojas: Map<string, string>;
  aoMudar: (valor: string) => void;
}) {
  return (
    <label className="terr-filtro">
      {rotulo}
      <select value={valor} onChange={(e) => aoMudar(e.target.value)}>
        <option value="">Todas ao alcance</option>
        {[...lojas.entries()]
          .sort((a, b) => a[1].localeCompare(b[1]))
          .map(([codigo, nome]) => (
            <option key={codigo} value={codigo}>
              {nome}
            </option>
          ))}
      </select>
      <span className="terr-filtro-motivo">aplica-se {aplicaA}</span>
    </label>
  );
}
