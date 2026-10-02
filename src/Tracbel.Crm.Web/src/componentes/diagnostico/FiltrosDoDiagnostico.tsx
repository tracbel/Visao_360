/**
 * Os filtros do Diagnóstico Comercial — na ordem da maquete de 02/10/2026: Período, Regional / Loja, Cultura principal e
 * CEN / Gestor na linha, e o resto em **Mais filtros** (o tipo de máquina, o município da ficha e o período à mão). Tudo
 * aplica na hora, sem botão.
 *
 * O PERÍODO VAZIO SÃO OS 12 MESES FECHADOS, e não o ano fiscal dos Indicadores: o IOC compara a venda com a demanda
 * de UM ANO, e doze meses seguidos são um ano inteiro qualquer que seja o começo — sem depender da sazonalidade. O ano
 * fiscal continua na lista, e aí a venda é levada a um ano pela sazonalidade vigente (a tela diz isso no período).
 *
 * REGIONAL / LOJA É UM CAMPO SÓ, como a maquete: a sub-região da ADR (Norte ou Noroeste) ou a filial responsável — um ou
 * outro. A CULTURA PRINCIPAL recorta a lista na tela, sem pedir de novo: o IOC é uma ordem entre TODOS os municípios, e a
 * régua do potencial é o percentil 90 deles. O CEN / GESTOR (decisão de 02/10/2026) é o CEN da carteira — o mesmo filtro
 * dos Indicadores —, e vai ao servidor: os clientes das outras carteiras saem da conta.
 *
 * O TIPO DE MÁQUINA MORA EM "MAIS FILTROS" (decisão de 02/10/2026), e o selo do botão conta quando ele não é o trator; os
 * cartões dizem a categoria.
 *
 * O MUNICÍPIO NÃO FILTRA A CONSULTA: ele escolhe o município da ficha, como o clique no mapa e na tabela.
 */

import * as Popover from '@radix-ui/react-popover';
import { CalendarDays, Funnel, Leaf, MapPin, MapPinned, Tractor, Users } from 'lucide-react';
import { useMemo } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import { anoFiscalFechado, dozeMesesFechados, mes as mesPorExtenso, nomeDoAnoFiscal } from '../territorio/indicadoresDaAdr';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { DiagnosticoComercialDaRegiao, FiltrosDoDiagnostico, MunicipioNoDiagnostico } from '../../tipos/mercado';

type Preset = '12meses' | 'anoFiscal' | 'personalizado';

export function FiltrosDoDiagnosticoComercial({
  filtros,
  aoMudar,
  dados,
  lojasConhecidas,
  culturas,
  cultura,
  aoMudarCultura,
  municipios,
  escolhido,
  aoEscolher,
}: {
  filtros: FiltrosDoDiagnostico;
  aoMudar: (parcial: Partial<FiltrosDoDiagnostico>) => void;
  dados: DiagnosticoComercialDaRegiao | null;
  /** As lojas que as respostas já mostraram — a lista não encolhe ao filtrar uma sub-região. */
  lojasConhecidas: ReadonlyMap<string, string>;
  /** As culturas principais que as respostas já mostraram. */
  culturas: readonly string[];
  cultura: string | null;
  aoMudarCultura: (cultura: string | null) => void;
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

  const categoria = filtros.categoria ?? dados?.categoria ?? 'TRATOR';
  const secundariosAtivos = (preset === 'personalizado' ? 1 : 0) + (categoria !== 'TRATOR' ? 1 : 0) + (escolhido ? 1 : 0);

  const opcoesDeMunicipio = useMemo(
    () => [...municipios].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR')),
    [municipios],
  );
  const categorias = dados?.categorias.length ? dados.categorias : [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }];
  const responsaveis = dados?.responsaveis ?? [];
  const regionalOuLoja = filtros.lojaCodigo ? `loja:${filtros.lojaCodigo}` : filtros.regiao ? `regiao:${filtros.regiao}` : '';

  return (
    <div className="dash-filtros" data-bloco="filtros">
      <div className="dash-filtros-linha">
        <label className="dash-filtro" data-bloco="periodo">
          <span className="dash-filtro-icone" aria-hidden="true">
            <CalendarDays size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            {/* SEM ⓘ NO RÓTULO, como a maquete: a explicação do período vai na dica do próprio campo. */}
            <span className="dash-filtro-rotulo">Período</span>
            <select
              title={
                'O período vale para as vendas. O crédito e o preço são o momento mais recente — a janela dos parâmetros do ' +
                'potencial —, e a carteira e a cobertura são o estado de hoje, no alcance da filial do cabeçalho. O padrão são ' +
                'os 12 meses fechados: o IOC compara a venda com a demanda de um ano, e doze meses seguidos são um ano inteiro. ' +
                'Em outro período, a venda é levada a um ano pela sazonalidade vigente, e não por regra de três.' +
                (dados && dados.fracaoDoAnoNoPeriodo !== 1
                  ? ` Este período vale ${Math.round(dados.fracaoDoAnoNoPeriodo * 100)}% do ano pela sazonalidade.`
                  : '')
              }
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

        <label className="dash-filtro" data-bloco="regional">
          <span className="dash-filtro-icone" aria-hidden="true">
            <MapPin size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Regional / Loja</span>
            <select
              value={regionalOuLoja}
              onChange={(e) => {
                const [tipo, valor] = e.target.value.split(':');
                aoMudar({ regiao: tipo === 'regiao' ? valor : undefined, lojaCodigo: tipo === 'loja' ? valor : undefined });
              }}
            >
              <option value="">Todas as regiões</option>
              <optgroup label="Sub-região da ADR">
                <option value="regiao:Norte">Região Norte</option>
                <option value="regiao:Noroeste">Região Noroeste</option>
              </optgroup>
              {lojasConhecidas.size > 0 && (
                <optgroup label="Loja responsável">
                  {[...lojasConhecidas.entries()]
                    .sort((a, b) => a[1].localeCompare(b[1], 'pt-BR'))
                    .map(([codigo, nome]) => (
                      <option key={codigo} value={`loja:${codigo}`}>
                        {nome}
                      </option>
                    ))}
                </optgroup>
              )}
            </select>
          </span>
        </label>

        <label className="dash-filtro" data-bloco="cultura">
          <span className="dash-filtro-icone" aria-hidden="true">
            <Leaf size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Cultura principal</span>
            <select value={cultura ?? ''} onChange={(e) => aoMudarCultura(e.target.value || null)}>
              <option value="">Todas as culturas</option>
              {culturas.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </span>
        </label>

        <label className="dash-filtro" data-bloco="cen">
          <span className="dash-filtro-icone" aria-hidden="true">
            <Users size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">CEN / Gestor</span>
            <select
              value={filtros.responsavel ?? ''}
              onChange={(e) => aoMudar({ responsavel: e.target.value || undefined })}
              disabled={responsaveis.length === 0 && !filtros.responsavel}
              title={responsaveis.length === 0 ? 'Nenhuma carteira comercial ao seu alcance tem responsável.' : undefined}
            >
              <option value="">Todos os gestores</option>
              {responsaveis.map((r) => (
                <option key={r.id} value={String(r.id)}>
                  {nomeProprio(r.nome)}
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
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    <Tractor size={14} strokeWidth={2} aria-hidden="true" /> Tipo de máquina
                    <InfoTooltip
                      rotulo="Por que o tipo de máquina muda o índice"
                      texto="A demanda, a meta (share-alvo) e as vendas são da categoria escolhida. Em todas as categorias, as três somam as categorias com regra de potencial."
                    />
                  </span>
                  <select value={categoria} onChange={(e) => aoMudar({ categoria: e.target.value })}>
                    {categorias.map((c) => (
                      <option key={c.codigo} value={c.codigo}>
                        {c.nome}
                      </option>
                    ))}
                    <option value="TODAS">Todas as categorias</option>
                  </select>
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    <MapPinned size={14} strokeWidth={2} aria-hidden="true" /> Município (abre a ficha)
                  </span>
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
