/**
 * Ranking em barras horizontais, em HTML — não em canvas.
 *
 * POR QUE NÃO É UM GRÁFICO DE VERDADE: o que este bloco mostra é uma lista
 * ordenada de no máximo uma dúzia de itens com um número cada. Em barras
 * verticais desenhadas em canvas, como estava, o nome de cada item vira um
 * rótulo de eixo — e nome real de carteira ("TBA_PNEUS_02", "INTELIGÊNCIA DE
 * MERCADO TRACBEL AGRO") não cabe embaixo de uma barra de 60px: os rótulos
 * encavalam e saem "TBA_PNEUS_02TBA_PNEUS_01" grudado.
 *
 * Em HTML o nome é texto de verdade: quebra, corta com reticências, tem o
 * inteiro no `title`, é selecionável, é lido por leitor de tela e fica nítido
 * em qualquer densidade de pixel. É o mesmo desenho do cartão "Top CENs" da
 * Visão 360, que já resolvia isso.
 */

export type ItemDeBarra = {
  /** Identifica a linha; também é o texto principal quando não há `titulo`. */
  chave: string;
  titulo: string;
  /** Uma linha menor embaixo do título (ex.: "4.016 vínculos"). */
  detalhe?: string;
  /** O que a barra mede, na unidade que `formatar` souber escrever. */
  valor: number;
  /** A cor da barra. Sem ela, o verde da marca. */
  cor?: string;
};

const COR_PADRAO = '#367C2B'; // var(--jd-green)

export function ListaDeBarras({
  itens,
  formatar,
  maximo,
}: {
  itens: ItemDeBarra[];
  /** Como o número aparece à direita (ex.: `(v) => v.toFixed(1) + '%'`). */
  formatar: (valor: number) => string;
  /**
   * O denominador do comprimento da barra.
   *
   * Passar 100 num ranking de percentuais faz cada barra significar a fração
   * do total possível. Sem ele, o maior item vira a régua — o que compara os
   * itens entre si, mas não diz nada sobre o todo.
   */
  maximo?: number;
}) {
  const teto = maximo ?? Math.max(...itens.map((i) => i.valor), 1);

  return (
    <ul className="cad-barras">
      {itens.map((item) => (
        <li className="cad-barras-linha" key={item.chave}>
          <div className="cad-barras-rotulo">
            <span className="cad-barras-titulo" title={item.titulo}>
              {item.titulo}
            </span>
            {item.detalhe && <span className="cad-barras-detalhe">{item.detalhe}</span>}
          </div>
          <span className="cad-barra-trilho" aria-hidden="true">
            <span
              className="cad-barra-mini"
              style={{
                width: `${Math.min(100, (item.valor / teto) * 100)}%`,
                background: item.cor ?? COR_PADRAO,
              }}
            />
          </span>
          <span className="cad-barras-valor">{formatar(item.valor)}</span>
        </li>
      ))}
    </ul>
  );
}
