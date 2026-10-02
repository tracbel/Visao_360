/**
 * A PRIORIDADE DOS MUNICÍPIOS — as cinco classes, e o filtro da tela (28/09/2026; desenho da maquete em 02/10/2026).
 *
 * A RÉGUA EM CIMA É A ESCALA, e não a proporção: as cinco cores na mesma largura, da máxima à manutenção, como a maquete
 * desenha — a proporção está escrita em cada cartão. Cada cartão é um botão que filtra o mapa e a tabela, e clicar de
 * novo mostra todas.
 *
 * OS PESOS E AS LIMITAÇÕES SAÍRAM DAQUI para o "Entenda os indicadores", no título da seção: a maquete não os desenha
 * neste painel, e eles continuam a um clique.
 */

import { PainelDoMomento } from '../mercado/momento/pecas';
import type { ClasseDePrioridade, ResumoDoDiagnostico } from '../../tipos/mercado';
import { CLASSES, n } from './diagnostico';

export function DistribuicaoPorClasse({
  resumo,
  classe,
  aoEscolherClasse,
}: {
  resumo: ResumoDoDiagnostico;
  classe: ClasseDePrioridade | null;
  aoEscolherClasse: (classe: ClasseDePrioridade | null) => void;
}) {
  const quantos = (c: ClasseDePrioridade) => resumo[(c.charAt(0).toLowerCase() + c.slice(1)) as Uncapitalize<ClasseDePrioridade>];
  const comIndice = resumo.total - resumo.semIndice;

  return (
    <PainelDoMomento
      titulo="Prioridade dos municípios"
      dica={
        'O IOC vai de 0 a 100: alto é muito a ganhar — potencial grande e pouco explorado, com crédito e preço a favor. É ' +
        'uma ordem de prioridade entre os municípios, e não previsão de venda. As classes vão de 20 em 20 pontos. ' +
        'Componente sem dado num município sai da conta e os pesos dos outros são normalizados. Clique numa classe para ' +
        'ver só os municípios dela no mapa e na tabela.'
      }
      data-bloco="distribuicao"
    >
      <div className="diag-distribuicao" role="group" aria-label="Filtrar pela classe de prioridade">
        <div className="diag-distribuicao-barra" aria-hidden="true">
          {CLASSES.map((c) => (
            <span key={c.chave} style={{ background: c.cor }} data-apagada={classe !== null && classe !== c.chave ? 'true' : undefined} />
          ))}
        </div>
        <div className="diag-distribuicao-legenda">
          {CLASSES.map((c) => {
            const q = quantos(c.chave);
            const ativa = classe === c.chave;
            return (
              <button
                key={c.chave}
                type="button"
                className="diag-classe-botao"
                aria-pressed={ativa}
                onClick={() => aoEscolherClasse(ativa ? null : c.chave)}
                data-apagada={classe !== null && !ativa ? 'true' : undefined}
                style={{ '--diag-faixa': c.cor } as React.CSSProperties}
              >
                <span className="diag-classe-nome">{c.rotulo}</span>
                <strong className="diag-classe-quantos">{n(q, 0)}</strong>
                <span className="diag-classe-faixa" title={comIndice > 0 ? `${n((100 * q) / comIndice, 0)}% dos municípios com IOC` : undefined}>
                  {c.faixa} • mun.
                  {comIndice > 0 && ` • ${n((100 * q) / comIndice, 0)}%`}
                </span>
              </button>
            );
          })}
        </div>
        {classe !== null && (
          <button type="button" className="diag-limpar" onClick={() => aoEscolherClasse(null)}>
            Mostrar todas as classes
          </button>
        )}
      </div>

      {resumo.semIndice > 0 && (
        <p className="diag-nota">
          {n(resumo.semIndice, 0)} {resumo.semIndice === 1 ? 'município ficou' : 'municípios ficaram'} sem IOC: nenhum
          componente com peso tem dado neles.
        </p>
      )}
    </PainelDoMomento>
  );
}
