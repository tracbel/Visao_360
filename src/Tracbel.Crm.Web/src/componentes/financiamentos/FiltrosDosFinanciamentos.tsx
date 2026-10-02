/**
 * Os filtros da Gestão de Financiamentos (issue 261) — Período, Produto, Programa e Regional / Loja na linha, como o padrão
 * das telas de mercado; o intervalo à mão (De e Até) e a usina em **Mais filtros**. Tudo aplica na hora, sem botão.
 *
 * O PERÍODO VAZIO SÃO OS 12 MESES DO PAINEL DO CRÉDITO — os que terminam no último mês do SICOR descontada a carência. Os
 * atalhos de 3 e 6 meses terminam no mesmo mês; o intervalo à mão pode ser qualquer um dentro da série.
 *
 * O RECORTE VAI ALÉM DA ADR, como o protótipo: São Paulo inteiro e "fora da região" também são mercado de crédito, e é ali
 * que se vê se a Região cresce mais ou menos que o estado.
 */

import * as Popover from '@radix-ui/react-popover';
import { CalendarDays, Factory, Funnel, Landmark, MapPin, Tractor } from 'lucide-react';
import { InfoTooltip } from '../InfoTooltip';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { FiltrosDosFinanciamentos, FinanciamentosDoSicor } from '../../tipos/mercado';
import { comoFiltro, textoDoPeriodo } from './financiamentos';

type Atalho = '12' | '6' | '3' | 'personalizado';

/** O mês `aaaa-mm` somado de meses — os atalhos contam a partir do fim do período padrão. */
function somarMeses(aaaaMm: string, meses: number): string {
  const [ano, mes] = aaaaMm.split('-').map(Number);
  const total = ano * 12 + (mes - 1) + meses;
  return `${Math.floor(total / 12)}-${String((total % 12) + 1).padStart(2, '0')}`;
}

export function FiltrosDosFinanciamentosDoSicor({
  filtros,
  aoMudar,
  dados,
  fimPadrao,
}: {
  filtros: FiltrosDosFinanciamentos;
  aoMudar: (parcial: Partial<FiltrosDosFinanciamentos>) => void;
  dados: FinanciamentosDoSicor | null;
  /** O último mês do período padrão (aaaa-mm), lembrado da primeira resposta. */
  fimPadrao: string | null;
}) {
  const atalho: Atalho =
    !filtros.de && !filtros.ate
      ? '12'
      : fimPadrao && filtros.ate === fimPadrao && filtros.de === somarMeses(fimPadrao, -5)
        ? '6'
        : fimPadrao && filtros.ate === fimPadrao && filtros.de === somarMeses(fimPadrao, -2)
          ? '3'
          : 'personalizado';
  const intervalo = dados ? textoDoPeriodo(dados.de, dados.ate) : null;
  const regionalOuLoja = filtros.lojaCodigo ? `loja:${filtros.lojaCodigo}` : filtros.recorte && filtros.recorte !== 'adr' ? `recorte:${filtros.recorte}` : '';
  const secundariosAtivos = (atalho === 'personalizado' ? 1 : 0) + (filtros.usina ? 1 : 0);
  const minimo = comoFiltro(dados?.primeiroMesDoSicor ?? null);
  const maximo = comoFiltro(dados?.ultimoMesDoSicor ?? null);

  return (
    <div className="dash-filtros" data-bloco="filtros">
      <div className="dash-filtros-linha">
        <label className="dash-filtro" data-bloco="periodo">
          <span className="dash-filtro-icone" aria-hidden="true">
            <CalendarDays size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">
              Período
              <InfoTooltip
                rotulo="O período e a comparação"
                texto="Toda variação é contra o MESMO período do ano anterior, que respeita a sazonalidade da safra. O padrão são os 12 meses que terminam no último mês do SICOR descontada a carência — o Banco Central ainda completa os meses recentes com registro atrasado."
              />
            </span>
            <select
              value={atalho}
              onChange={(e) => {
                const escolha = e.target.value as Atalho;
                if (escolha === '12') aoMudar({ de: undefined, ate: undefined });
                else if (fimPadrao && escolha !== 'personalizado')
                  aoMudar({ ate: fimPadrao, de: somarMeses(fimPadrao, -(Number(escolha) - 1)) });
              }}
            >
              <option value="12">12 meses{atalho === '12' && intervalo ? ` (${intervalo})` : ''}</option>
              <option value="6" disabled={!fimPadrao}>6 meses{atalho === '6' && intervalo ? ` (${intervalo})` : ''}</option>
              <option value="3" disabled={!fimPadrao}>3 meses{atalho === '3' && intervalo ? ` (${intervalo})` : ''}</option>
              {atalho === 'personalizado' && <option value="personalizado">Personalizado{intervalo ? ` (${intervalo})` : ''}</option>}
            </select>
          </span>
        </label>

        <label className="dash-filtro" data-bloco="produto">
          <span className="dash-filtro-icone" aria-hidden="true">
            <Tractor size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Produto</span>
            <select value={filtros.produto ?? ''} onChange={(e) => aoMudar({ produto: e.target.value || undefined })}>
              <option value="">Todos ({dados?.produtos.length ?? 3} de máquina)</option>
              {(dados?.produtos ?? []).map((p) => (
                <option key={p.codigo} value={String(p.codigo)}>
                  {nomeProprio(p.nome)}
                </option>
              ))}
            </select>
          </span>
        </label>

        <label className="dash-filtro" data-bloco="programa">
          <span className="dash-filtro-icone" aria-hidden="true">
            <Landmark size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Programa</span>
            <select value={filtros.programa ?? ''} onChange={(e) => aoMudar({ programa: e.target.value || undefined })}>
              <option value="">Todos os programas</option>
              {(dados?.programas ?? []).map((p) => (
                <option key={p.codigo} value={String(p.codigo)} title={p.nome}>
                  {p.nome.length > 40 ? `${p.nome.slice(0, 40)}…` : p.nome}
                </option>
              ))}
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
                aoMudar({ recorte: tipo === 'recorte' ? valor : undefined, lojaCodigo: tipo === 'loja' ? valor : undefined });
              }}
            >
              <option value="">Região Tracbel</option>
              <optgroup label="Sub-região da ADR">
                <option value="recorte:norte">Região Norte</option>
                <option value="recorte:noroeste">Região Noroeste</option>
              </optgroup>
              {(dados?.lojas.length ?? 0) > 0 && (
                <optgroup label="Loja responsável">
                  {dados!.lojas.map((l) => (
                    <option key={l.codigo} value={`loja:${l.codigo}`}>
                      {nomeProprio(l.nome)}
                    </option>
                  ))}
                </optgroup>
              )}
              <optgroup label="Além da ADR">
                <option value="recorte:sp">Estado de São Paulo</option>
                <option value="recorte:fora">Fora da Região Tracbel</option>
              </optgroup>
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
                  <span className="dash-filtro-rotulo">De</span>
                  <input
                    type="month"
                    min={minimo}
                    max={maximo}
                    value={filtros.de ?? comoFiltro(dados?.de ?? null) ?? ''}
                    onChange={(e) => aoMudar({ de: e.target.value || undefined, ate: filtros.ate ?? comoFiltro(dados?.ate ?? null) })}
                  />
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">Até</span>
                  <input
                    type="month"
                    min={minimo}
                    max={maximo}
                    value={filtros.ate ?? comoFiltro(dados?.ate ?? null) ?? ''}
                    onChange={(e) => aoMudar({ ate: e.target.value || undefined, de: filtros.de ?? comoFiltro(dados?.de ?? null) })}
                  />
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    <Factory size={14} strokeWidth={2} aria-hidden="true" /> Usina de etanol
                  </span>
                  <select value={filtros.usina ?? ''} onChange={(e) => aoMudar({ usina: e.target.value || undefined })}>
                    <option value="">Com ou sem usina</option>
                    <option value="com">Só municípios com usina</option>
                    <option value="sem">Só municípios sem usina</option>
                  </select>
                </label>
              </Popover.Content>
            </Popover.Portal>
          </Popover.Root>
        </div>
      </div>
    </div>
  );
}
