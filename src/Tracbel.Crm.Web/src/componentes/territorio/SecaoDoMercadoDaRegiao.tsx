import { InfoTooltip } from '../InfoTooltip';

/**
 * O título da seção "O mercado da região".
 *
 * A §10.3 do documento 49 e o documento 50 mandam preservá-la exatamente como
 * está, com a linha dela: é a seção que a diretoria já reconhece. Por isso o
 * subtítulo continua sendo "O que existe no território, por fonte pública", e
 * não o da maquete — que promete "com comparação ao período anterior", coisa que
 * a API não devolve.
 *
 * O CONTROLE DA DIREITA É O DA MAQUETE (fase T4.9), e NASCE DESLIGADO. Ele é a
 * comparação com o período anterior, e ela não existe: a leitura desta tela
 * devolve UMA janela de competência, não duas. Um interruptor que liga uma
 * comparação inexistente é o "botão que não faz nada" do documento 05, §3.
 *
 * ENTÃO POR QUE DESENHÁ-LO. Porque é o mesmo padrão que os filtros sem dado
 * desta tela já usam há três fases: o controle pedido aparece, desligado, com o
 * motivo e a issue na dica. Some-lo faria a tela parecer completa — e o dia em
 * que a issue 69 trouxer o período anterior, o que muda aqui é uma linha.
 */
export function SecaoDoMercadoDaRegiao() {
  return (
    <div className="terr-secao-mercado" data-bloco="mercado-da-regiao">
      <div className="terr-secao-cabecalho">
        <h2 className="terr-secao-titulo">
          O mercado da região
          {/* O PARÁGRAFO SOBRE O DENOMINADOR VIROU DICA (fase T2.1): é método, e
              método é nível 2 da hierarquia da issue 33 — não primeira camada. */}
          <InfoTooltip
            rotulo="Como ler as fatias desta seção"
            texto={
              'O denominador de São Paulo é o total PUBLICADO pelo IBGE, e não a soma dos municípios: o valor ' +
              'municipal sigiloso entra no total do estado sem aparecer embaixo. O denominador da Região Tracbel é a ' +
              'área de atuação inteira, e não muda quando o filtro de sub-região muda.'
            }
          />
        </h2>

        <div className="dash-secao-controle" data-bloco="comparar-periodo">
          <span className="dash-comparar">
            Comparar com período anterior
            {/* `role="switch"` e `aria-checked` de verdade: o leitor de tela
                anuncia "interruptor, desligado, indisponível", que é o estado.
                `disabled` num `<button>` já tira do Tab — e a dica ao lado
                continua alcançável, que é onde está a explicação. */}
            <button
              type="button"
              className="dash-interruptor"
              role="switch"
              aria-checked={false}
              disabled
              aria-label="Comparar com o período anterior"
            />
            <InfoTooltip
              rotulo="Por que a comparação com o período anterior está desligada"
              texto={
                'A leitura desta tela devolve UMA janela de competência — a do filtro de período —, e não duas: não ' +
                'há período anterior para comparar. É por isso que nenhum cartão traz "+8% vs. ano anterior", e o ' +
                'único bloco com variação é o crédito, que tem duas safras na própria fonte. Ligar isto depende de a ' +
                'leitura passar a devolver a janela anterior junto (issue 69).'
              }
            />
          </span>

          {/* A unidade da variação — Δ% ou Δ absoluto. Desligada pelo mesmo
              motivo: ela escolhe COMO mostrar uma comparação que não existe. */}
          <select disabled aria-label="Unidade da comparação">
            <option>Δ %</option>
          </select>
        </div>
      </div>
      <p className="terr-secao-subtitulo">O que existe no território, por fonte pública.</p>
    </div>
  );
}
