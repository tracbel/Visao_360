/**
 * Os filtros do Dimensionamento da ADR (issue 259) — Ano-base, Cultura, Regional / Loja e CEN na linha, como o padrão das
 * telas de mercado; a classe do cliente e a usina em **Mais filtros**. Tudo aplica na hora, sem botão.
 *
 * O ANO-BASE É O DA PAM, e não o ano fiscal: a produção agrícola é anual, e o IBGE publica com um ano de atraso. As opções
 * são os anos carregados, e o comparado é sempre o anterior.
 *
 * O CEN E A CLASSE SÃO DA CARTEIRA, como o "Filtros da carteira" do protótipo — com uma diferença dita na dica: com o CEN,
 * o território também vira o dele (os municípios onde ele tem cliente).
 */

import * as Popover from '@radix-ui/react-popover';
import { CalendarDays, Factory, Funnel, MapPin, Sprout, Star, UserRound } from 'lucide-react';
import { InfoTooltip } from '../InfoTooltip';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { DimensionamentoDaAdr, FiltrosDoDimensionamento } from '../../tipos/mercado';
import { CampoDoFiltro } from '../comum/CampoDoFiltro';

export function FiltrosDoDimensionamentoDaAdr({
  filtros,
  aoMudar,
  dados,
}: {
  filtros: FiltrosDoDimensionamento;
  aoMudar: (parcial: Partial<FiltrosDoDimensionamento>) => void;
  dados: DimensionamentoDaAdr | null;
}) {
  const anos = dados?.anosDisponiveis ?? [];
  const culturas = dados?.culturas ?? [];
  const lojas = dados?.lojas ?? [];
  const responsaveis = dados?.responsaveis ?? [];
  const regionalOuLoja = filtros.lojaCodigo ? `loja:${filtros.lojaCodigo}` : filtros.regiao ? `regiao:${filtros.regiao}` : '';
  const secundariosAtivos = (filtros.classe ? 1 : 0) + (filtros.usina ? 1 : 0);

  return (
    <div className="dash-filtros" data-bloco="filtros">
      <div className="dash-filtros-linha">
        <CampoDoFiltro
          icone={CalendarDays}
          rotulo={
            <>
              Ano-base
              <InfoTooltip
                rotulo="O ano-base e o ano comparado"
                texto="O ano da Produção Agrícola Municipal do IBGE (tabela 5457). Toda variação é contra o ano anterior a ele. O IBGE publica cada ano no segundo semestre do ano seguinte."
              />
            </>
          }
          bloco="ano-base"
        >
          <select
            value={filtros.anoBase ?? (dados?.anoBase != null ? String(dados.anoBase) : '')}
            onChange={(e) => aoMudar({ anoBase: e.target.value || undefined })}
            disabled={anos.length === 0}
          >
            {anos.length === 0 && <option value="">Sem PAM carregada</option>}
            {anos.map((a) => (
              <option key={a} value={String(a)}>
                {a} (vs {a - 1})
              </option>
            ))}
          </select>
        </CampoDoFiltro>

        <CampoDoFiltro icone={Sprout} rotulo="Cultura" bloco="cultura">
          <select value={filtros.cultura ?? ''} onChange={(e) => aoMudar({ cultura: e.target.value || undefined })}>
            <option value="">Todas as culturas</option>
            {culturas.map((c) => (
              <option key={c.codigo} value={c.codigo}>
                {c.nome}
              </option>
            ))}
          </select>
        </CampoDoFiltro>

        <CampoDoFiltro icone={MapPin} rotulo="Regional / Loja" bloco="regional">
          <select
            value={regionalOuLoja}
            onChange={(e) => {
              const [tipo, valor] = e.target.value.split(':');
              aoMudar({ regiao: tipo === 'regiao' ? valor : undefined, lojaCodigo: tipo === 'loja' ? valor : undefined });
            }}
          >
            <option value="">Região Tracbel inteira</option>
            <optgroup label="Sub-região da ADR">
              <option value="regiao:Norte">Região Norte</option>
              <option value="regiao:Noroeste">Região Noroeste</option>
            </optgroup>
            {lojas.length > 0 && (
              <optgroup label="Loja responsável">
                {lojas.map((l) => (
                  <option key={l.codigo} value={`loja:${l.codigo}`}>
                    {nomeProprio(l.nome)}
                  </option>
                ))}
              </optgroup>
            )}
          </select>
        </CampoDoFiltro>

        <CampoDoFiltro
          icone={UserRound}
          rotulo={
            <>
              CEN / Vendedor
              <InfoTooltip
                rotulo="O que o CEN recorta"
                texto="Os clientes das carteiras comerciais dele — e o território também: só os municípios onde ele tem cliente entram na produção, no mapa e na matriz, como o filtro de vendedor do protótipo."
              />
            </>
          }
          bloco="cen"
        >
          <select
            value={filtros.responsavel ?? ''}
            onChange={(e) => aoMudar({ responsavel: e.target.value || undefined })}
            disabled={responsaveis.length === 0 && !filtros.responsavel}
            title={responsaveis.length === 0 ? 'Nenhuma carteira comercial ao seu alcance tem responsável.' : undefined}
          >
            <option value="">Todos os vendedores</option>
            {responsaveis.map((r) => (
              <option key={r.id} value={String(r.id)}>
                {nomeProprio(r.nome)}
              </option>
            ))}
          </select>
        </CampoDoFiltro>

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
                    <Star size={14} strokeWidth={2} aria-hidden="true" /> Classe do cliente
                    <InfoTooltip
                      rotulo="De onde vem a classe do cliente"
                      texto="A curva ABC do faturamento do Protheus — a mesma da Cobertura e do Diagnóstico. O cliente sem faturamento não tem classe e aparece à parte."
                    />
                  </span>
                  <select value={filtros.classe ?? ''} onChange={(e) => aoMudar({ classe: e.target.value || undefined })}>
                    <option value="">Todas as classes</option>
                    {['A', 'B', 'C', 'D'].map((c) => (
                      <option key={c} value={c}>
                        Classe {c}
                      </option>
                    ))}
                  </select>
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
