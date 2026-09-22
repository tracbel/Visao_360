/**
 * Configurações › TI e Integrações › Fontes públicas › Cobertura dos dados do motor (issue 150).
 *
 * O que cada fonte cobre, medido pelo servidor a cada leitura: municípios da ADR com a área de cada cultura
 * divulgada, meses de cada série de preço, abas de custo com custo total, a base do índice de crédito e o
 * preenchimento do dado interno. **Só contagem** — nenhum valor em reais, nenhum nome de cliente.
 *
 * A TELA NÃO CALCULA NADA: situação, percentual e frase chegam prontos do servidor (documento 49, C-09). O item
 * incompleto fica em destaque, como a fonte atrasada na tabela de cima.
 */

import { useState } from 'react';
import { formatarNumero } from '../../telas/cadastro/formato';
import type {
  BlocoDaCoberturaDoMotor,
  ItemDaCoberturaDoMotor,
  PainelDeCoberturaDoMotor,
  SituacaoDaCobertura,
} from '../../tipos/potencial';

const SITUACAO: Record<SituacaoDaCobertura, { rotulo: string; cor: string; classe: string }> = {
  Completa: { rotulo: 'Completa', cor: '#22C55E', classe: '' },
  Parcial: { rotulo: 'Parcial', cor: '#F59E0B', classe: 'pot-fonte-semdado' },
  Vazia: { rotulo: 'Vazia', cor: '#EF4444', classe: 'pot-fonte-atrasada' },
};

/** Blocos longos (as culturas da PAM, as séries de preço) mostram os primeiros e escondem o resto atrás de um botão. */
const VISIVEIS = 8;

function Periodo({ item }: { item: ItemDaCoberturaDoMotor }) {
  if (!item.periodoInicial) return <>—</>;
  return item.periodoInicial === item.periodoFinal ? (
    <>{item.periodoInicial}</>
  ) : (
    <>
      {item.periodoInicial} a {item.periodoFinal}
    </>
  );
}

function BlocoDeCobertura({ bloco }: { bloco: BlocoDaCoberturaDoMotor }) {
  const [todos, setTodos] = useState(false);
  const itens = todos ? bloco.itens : bloco.itens.slice(0, VISIVEIS);

  return (
    <section className="pot-cobertura-bloco" aria-label={bloco.nome}>
      <div className="pot-cobertura-cabecalho">
        <strong>{bloco.nome}</strong>
        <span className="pot-sub">
          {bloco.fonte}
          {bloco.ehInterno && ' · dado interno, só contagem'}
        </span>
        <span className={bloco.incompletos > 0 ? 'pot-alerta' : 'pot-sub'}>
          {bloco.incompletos === 0 ? 'tudo completo' : `${bloco.incompletos} ${bloco.incompletos === 1 ? 'incompleto' : 'incompletos'}`}
        </span>
      </div>
      <div className="cad-tabela-wrap">
        <table className="cad-tabela pot-tabela">
          <thead>
            <tr>
              <th scope="col" className="pot-col-fonte">O que se mede</th>
              <th scope="col">Situação</th>
              <th scope="col">Cobertura</th>
              <th scope="col">Período</th>
            </tr>
          </thead>
          <tbody>
            {itens.map((item) => {
              const situacao = SITUACAO[item.situacao];
              return (
                <tr key={item.codigo} className={situacao.classe || undefined} data-situacao={item.situacao}>
                  <td>
                    <strong>{item.nome}</strong>
                    {item.detalhe && <div className="pot-sub">{item.detalhe}</div>}
                  </td>
                  <td>
                    <span
                      className="int-status"
                      style={{ background: `${situacao.cor}20`, color: situacao.cor, borderColor: `${situacao.cor}40` }}
                    >
                      <span className="int-dot" style={{ background: situacao.cor }} />
                      {situacao.rotulo}
                    </span>
                    {item.situacao !== 'Completa' && <div className="pot-motivo">{item.motivo}</div>}
                  </td>
                  <td className="cad-mono">
                    {formatarNumero(item.cobertos)} de {formatarNumero(item.total)}
                    <div className="pot-sub">
                      {item.unidade}
                      {item.percentual !== null && ` · ${item.percentual.toLocaleString('pt-BR')}%`}
                    </div>
                  </td>
                  <td className="cad-mono">
                    <Periodo item={item} />
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      {bloco.itens.length > VISIVEIS && (
        <button type="button" className="btn btn-ghost btn-sm" onClick={() => setTodos((t) => !t)}>
          {todos ? 'Mostrar só os primeiros' : `Mostrar todos os ${bloco.itens.length}`}
        </button>
      )}
    </section>
  );
}

/** O painel — separado da leitura para o teste montá-lo com uma resposta pronta. */
export function PainelDeCobertura({ painel }: { painel: PainelDeCoberturaDoMotor }) {
  return (
    <>
      <div className="pot-resumo" role="status">
        <span className={painel.incompletos > 0 ? 'pot-alerta' : undefined}>
          <strong>{painel.incompletos}</strong> {painel.incompletos === 1 ? 'item incompleto' : 'itens incompletos'}
        </span>
        <span>
          municípios sobre os <strong>{painel.municipiosDaAdr}</strong> da ADR
        </span>
        <span>
          dado interno contado em{' '}
          <strong>
            {painel.todasAsFiliais
              ? 'todas as filiais'
              : `${painel.filiaisNoAlcance} ${painel.filiaisNoAlcance === 1 ? 'filial' : 'filiais'} ao seu alcance`}
          </strong>
        </span>
      </div>
      {painel.grupos.map((bloco) => (
        <BlocoDeCobertura key={bloco.codigo} bloco={bloco} />
      ))}
    </>
  );
}
