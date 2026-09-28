/**
 * Os filtros do Diagnóstico Comercial — o mesmo desenho dos Indicadores Geográficos (28/09/2026): selo de ícone,
 * rótulo pequeno em cima do campo com moldura própria, os de decisão sempre à vista e o resto em **Mais filtros**.
 *
 * O PERÍODO VAZIO SÃO OS 12 MESES FECHADOS, e não o ano fiscal dos Indicadores: o IOC compara a venda com a demanda
 * de UM ANO, e doze meses seguidos são um ano inteiro qualquer que seja o começo — sem depender da sazonalidade. O ano
 * fiscal continua na lista, e aí a venda é levada a um ano pela sazonalidade vigente (a tela diz isso no período).
 *
 * O MUNICÍPIO NÃO FILTRA A CONSULTA: ele escolhe o município da ficha, como o clique no mapa e na tabela. O IOC é uma
 * ordem entre municípios, e a régua do potencial é o percentil 90 deles — tirar os outros mudaria o número do escolhido.
 */

import * as Popover from '@radix-ui/react-popover';
import { CalendarDays, Funnel, MapPin, Store, Tractor, User } from 'lucide-react';
import { useMemo } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import { anoFiscalFechado, dozeMesesFechados, mes as mesPorExtenso, nomeDoAnoFiscal } from '../territorio/indicadoresDaAdr';
import type { DiagnosticoComercialDaRegiao, FiltrosDoDiagnostico, MunicipioNoDiagnostico } from '../../tipos/mercado';

type Preset = '12meses' | 'anoFiscal' | 'personalizado';

export function FiltrosDoDiagnosticoComercial({
  filtros,
  aoMudar,
  dados,
  lojasConhecidas,
  municipios,
  escolhido,
  aoEscolher,
}: {
  filtros: FiltrosDoDiagnostico;
  aoMudar: (parcial: Partial<FiltrosDoDiagnostico>) => void;
  dados: DiagnosticoComercialDaRegiao | null;
  /** As lojas que as respostas já mostraram — a lista não encolhe ao filtrar uma sub-região. */
  lojasConhecidas: ReadonlyMap<string, string>;
  municipios: readonly MunicipioNoDiagnostico[];
  escolhido: MunicipioNoDiagnostico | null;
  aoEscolher: (codigo: number | null) => void;
}) {
  const anoFiscal = anoFiscalFechado();
  const dozeMeses = dozeMesesFechados();
  const preset: Preset =
    !filtros.competenciaInicial && !filtros.competenciaFinal
      ? '12meses'
      : filtros.competenciaInicial === anoFiscal.competenciaInicial && filtros.competenciaFinal === anoFiscal.competenciaFinal
        ? 'anoFiscal'
        : 'personalizado';

  // O INTERVALO EM VIGOR vem da resposta — é a competência que o servidor aplicou, e não uma conta local.
  const intervalo = dados
    ? `${mesPorExtenso(dados.competenciaInicial.slice(0, 7))} a ${mesPorExtenso(dados.competenciaFinal.slice(0, 7))}`
    : null;

  const secundariosAtivos = (preset === 'personalizado' ? 1 : 0) + (filtros.lojaCodigo ? 1 : 0);

  const opcoesDeMunicipio = useMemo(
    () => [...municipios].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR')),
    [municipios],
  );

  const categorias = dados?.categorias.length ? dados.categorias : [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }];

  return (
    <div className="dash-filtros" data-bloco="filtros">
      <div className="dash-filtros-linha">
        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <CalendarDays size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">
              Período
              <InfoTooltip
                rotulo="O período das vendas e como ele entra no IOC"
                texto={
                  <>
                    <p>
                      O período vale para as <strong>vendas</strong>. O crédito e o preço são o momento mais recente — a
                      janela dos parâmetros do potencial —, e a carteira e a cobertura são o estado de hoje, no alcance da
                      filial do cabeçalho.
                    </p>
                    <p>
                      O padrão são os <strong>12 meses fechados</strong>: o IOC compara a venda com a demanda de um ano, e
                      doze meses seguidos são um ano inteiro. Em outro período, a venda é levada a um ano pela{' '}
                      <strong>sazonalidade</strong> vigente, e não por regra de três — meses de safra pesam mais.
                    </p>
                    {dados && dados.fracaoDoAnoNoPeriodo !== 1 && (
                      <p>
                        Este período vale <strong>{Math.round(dados.fracaoDoAnoNoPeriodo * 100)}% do ano</strong> pela
                        sazonalidade.
                      </p>
                    )}
                  </>
                }
              />
            </span>
            <select
              value={preset}
              onChange={(e) => {
                if (e.target.value === '12meses') aoMudar({ competenciaInicial: undefined, competenciaFinal: undefined });
                else if (e.target.value === 'anoFiscal') aoMudar(anoFiscal);
              }}
            >
              <option value="12meses">12 meses{preset === '12meses' && intervalo ? ` (${intervalo})` : ''}</option>
              <option value="anoFiscal">
                {preset === 'anoFiscal' && intervalo
                  ? `${nomeDoAnoFiscal(anoFiscal.competenciaFinal) ?? 'Ano fiscal'} (${intervalo})`
                  : `Ano fiscal (${nomeDoAnoFiscal(anoFiscal.competenciaFinal) ?? ''} até o último mês)`}
              </option>
              {preset === 'personalizado' && <option value="personalizado">Personalizado{intervalo ? ` (${intervalo})` : ''}</option>}
            </select>
          </span>
        </label>

        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <Tractor size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">
              Tipo de máquina
              <InfoTooltip
                rotulo="Por que o tipo de máquina muda o índice"
                texto="A demanda, a meta (share-alvo) e as vendas são da categoria escolhida. Em todas as categorias, as três somam as categorias com regra de potencial."
              />
            </span>
            <select value={filtros.categoria ?? dados?.categoria ?? 'TRATOR'} onChange={(e) => aoMudar({ categoria: e.target.value })}>
              {categorias.map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  {c.nome}
                </option>
              ))}
              <option value="TODAS">Todas as categorias</option>
            </select>
          </span>
        </label>

        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <User size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Sub-região</span>
            <select value={filtros.regiao ?? ''} onChange={(e) => aoMudar({ regiao: e.target.value || undefined })}>
              <option value="">Região Tracbel inteira</option>
              <option value="Norte">Norte</option>
              <option value="Noroeste">Noroeste</option>
            </select>
          </span>
        </label>

        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <MapPin size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Município</span>
            <select
              value={escolhido ? String(escolhido.codigoIbge) : ''}
              onChange={(e) => aoEscolher(e.target.value === '' ? null : Number(e.target.value))}
              data-ativo={escolhido ? 'true' : undefined}
            >
              <option value="">Todos os municípios</option>
              {opcoesDeMunicipio.map((m) => (
                <option key={m.codigoIbge} value={m.codigoIbge}>
                  {m.nome}
                </option>
              ))}
            </select>
          </span>
        </label>

        <div className="dash-filtros-acao">
          <Popover.Root>
            <Popover.Trigger asChild>
              <button type="button" className="dash-mais-filtros" data-bloco="mais-filtros">
                <Funnel size={15} strokeWidth={2} aria-hidden="true" />
                Mais filtros
                {secundariosAtivos > 0 && <span className="dash-mais-filtros-selo">{secundariosAtivos}</span>}
              </button>
            </Popover.Trigger>
            <Popover.Portal>
              <Popover.Content className="dash-popover" sideOffset={6} collisionPadding={16} align="end">
                <div className="dash-popover-titulo">Mais filtros</div>
                {/* A LOJA MORA AQUI, e não na linha (28/09/2026): com cinco campos na linha, o período e a sub-região
                    saíam cortados a 1.536 px. A linha fica com os quatro dos Indicadores — período, tipo, sub-região e
                    município —, e o selo do botão conta a loja quando ela está escolhida. */}
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    <Store size={14} strokeWidth={2} aria-hidden="true" /> Loja
                  </span>
                  <select value={filtros.lojaCodigo ?? ''} onChange={(e) => aoMudar({ lojaCodigo: e.target.value || undefined })}>
                    <option value="">Todas</option>
                    {[...lojasConhecidas.entries()]
                      .sort((a, b) => a[1].localeCompare(b[1], 'pt-BR'))
                      .map(([codigo, nome]) => (
                        <option key={codigo} value={codigo}>
                          {nome}
                        </option>
                      ))}
                  </select>
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">Vendas de</span>
                  <input
                    type="month"
                    max={dozeMeses.competenciaFinal}
                    value={filtros.competenciaInicial ?? ''}
                    onChange={(e) => aoMudar({ competenciaInicial: e.target.value || undefined })}
                  />
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">até</span>
                  <input
                    type="month"
                    max={dozeMeses.competenciaFinal}
                    value={filtros.competenciaFinal ?? ''}
                    onChange={(e) => aoMudar({ competenciaFinal: e.target.value || undefined })}
                  />
                </label>
                {/* A VISÃO DA EMPRESA INTEIRA NÃO ESTÁ AQUI: esta leitura não diz se o perfil pode vê-la, e a opção
                    levaria a uma recusa sem aviso. O alcance é o da filial do cabeçalho, como diz a dica do período. */}
              </Popover.Content>
            </Popover.Portal>
          </Popover.Root>
        </div>
      </div>
    </div>
  );
}
