/**
 * OS PRIMITIVES DO PAINEL EXECUTIVO (fase T4.6).
 *
 * O PROBLEMA QUE ELES RESOLVEM não é falta de biblioteca: é falta de regra. A
 * tela funcionava, mas cada cartão escolhia o próprio `padding` no lugar onde
 * foi escrito, cada título tinha o tamanho que pareceu certo naquele dia, e o
 * estado vazio estava desenhado de três formas diferentes. Nada errado o
 * bastante para alguém apontar — e nada intencional o bastante para parecer um
 * painel de diretoria.
 *
 * ELES NÃO SÃO UM SEGUNDO DESIGN SYSTEM. Não há cor, tamanho nem medida escrita
 * aqui: tudo sai dos tokens do `design-system.css`, inclusive as duas escalas
 * que a T4.6 acrescentou (espaço `--e-*` e tipografia `--t-*`). O que eles fazem
 * é **impedir a escolha local** — é para isso que servem.
 *
 * O QUE JÁ EXISTIA CONTINUA SENDO USADO, e não foi duplicado:
 *   `TituloDaSecao`  é o cabeçalho de seção;
 *   `AbasInternas`   é o alternador dentro de um painel;
 *   `ValorAusente`   e `MetricaAusente` são a ausência com motivo.
 * Criar `SectionHeader`, `DashboardTabs` e `EmptyMetric` ao lado deles seria
 * exatamente o segundo vocabulário que não se quer.
 *
 * POR QUE O CARTÃO DE INDICADOR É NOVO, e não o `cad-kpi` de sempre: aquele é
 * compartilhado com as telas de cadastro (Clientes, Equipamentos, Configurações),
 * que esta fase não cobre e que o harness visual não consegue conferir. Mudar o
 * `cad-kpi` mexeria no que não dá para provar. Este nasce ao lado, sobre os
 * mesmos tokens, e as telas de cadastro podem adotá-lo quando forem revistas.
 */

import type { ComponentType, ReactNode } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';
import { ValorAusente } from '../comum/ValorAusente';
import type { ProcedenciaDoIndicador } from '../../tipos/territorio';

/**
 * O CONTAINER DA PÁGINA — largura máxima, centralizado, folga lateral que cede.
 *
 * Sem ele, num monitor de 1920 os cartões esticavam por quase 1800px: a linha de
 * leitura ficava longa demais e a grade de quatro KPIs virava quatro faixas
 * horizontais separadas por vazio. A largura escolhida está no CSS, e foi
 * conferida no harness antes de ser fixada.
 *
 * `className` deixa uma tela ajustar a PRÓPRIA largura sem mudar a das outras —
 * a Visão 360 ocupa a coluna inteira até 2.100px de janela (24/09/2026).
 */
export function PaginaDoPainel({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={className ? `dash-pagina ${className}` : 'dash-pagina'}>{children}</div>;
}

/**
 * UMA SEÇÃO — separada por ESPAÇO, e não por borda.
 *
 * A tela tinha borda em quase tudo, e borda demais não separa nada: quando todo
 * bloco tem moldura, nenhuma moldura significa "isto é outra coisa". O espaço da
 * escala faz esse trabalho sem desenhar mais uma linha.
 */
export function SecaoDoPainel({
  children,
  bloco,
  ...resto
}: {
  children: ReactNode;
  /** O `data-bloco`, que os testes de composição usam para achar a seção. */
  bloco?: string;
} & React.HTMLAttributes<HTMLElement>) {
  return (
    <section className="dash-secao" data-bloco={bloco} {...resto}>
      {children}
    </section>
  );
}

/**
 * UM PAINEL — a caixa branca com cabeçalho opcional.
 *
 * É o único lugar onde `padding` de cartão é decidido. Um painel dentro de outro
 * não ganha moldura de novo: `aninhado` tira a borda e a sombra e deixa só o
 * espaçamento, porque caixa dentro de caixa é o visual de formulário, não de
 * painel.
 */
export function Painel({
  titulo,
  acao,
  metodologia,
  aninhado = false,
  children,
  ...resto
}: {
  titulo?: ReactNode;
  /** Uma ação à direita do título. */
  acao?: ReactNode;
  /** Fonte e método — vai para a dica ao lado do título, nunca para a tela. */
  metodologia?: string;
  aninhado?: boolean;
  children: ReactNode;
} & React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div className={aninhado ? 'dash-painel dash-painel-aninhado' : 'dash-painel'} {...resto}>
      {titulo && (
        <div className="dash-painel-cabecalho">
          <h3 className="dash-painel-titulo">
            {titulo}
            {metodologia && (
              <InfoTooltip texto={metodologia} rotulo={`Fonte e método de ${String(titulo).toLowerCase()}`} />
            )}
          </h3>
          {acao}
        </div>
      )}
      {children}
    </div>
  );
}

/**
 * A GRADE DOS NÚMEROS DE DECISÃO — quatro colunas no desktop, uma no celular.
 *
 * COLUNAS FIXAS, E NÃO `auto-fit`. A grade antiga era `auto-fit` com largura
 * mínima, o que dava cinco colunas num monitor largo e três em outro: os quatro
 * números de decisão mudavam de arranjo conforme o monitor de quem olhava, e
 * "os quatro do topo" deixava de ser uma coisa reconhecível. Aqui são quatro, e
 * a quebra é declarada.
 */
export function GradeDeIndicadores({ children, ...resto }: { children: ReactNode } & React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div className="dash-kpis" {...resto}>
      {children}
    </div>
  );
}

/**
 * UM NÚMERO DE DECISÃO.
 *
 * TODOS TÊM A MESMA ALTURA, o mesmo `padding`, o rótulo no mesmo ponto, o valor
 * no mesmo eixo e a dica no mesmo canto. Isso não é capricho: quando um cartão é
 * mais alto que o vizinho porque o texto dele é maior, o olho lê a diferença de
 * altura como diferença de importância — e aqui os quatro são igualmente
 * importantes.
 *
 * SEM DADO É UM TRAVESSÃO E UM ⓘ, nunca um parágrafo dentro do cartão. O motivo
 * inteiro — com a issue que o destrava — vai na dica. Um parágrafo ali empurra o
 * número para baixo, desalinha a linha dos quatro e transforma o painel em
 * leitura.
 */
export function CartaoDeIndicador({
  rotulo,
  valor,
  unidade,
  contexto,
  oQue,
  motivoSemDado,
  procedencia,
  destaque = false,
  icone: Icone,
  tom = 'neutro',
}: {
  rotulo: string;
  /**
   * A unidade, ao lado do número e menor que ele — "máquinas", "R$", "/ha".
   *
   * Separada do valor de propósito: junto, ela entra no mesmo corpo de 30px e
   * rouba metade do peso do número; ao lado e menor, o olho lê o número primeiro
   * e a unidade depois, que é a ordem em que a pergunta se responde.
   */
  unidade?: string;
  /** O ícone do selo. Vem do `lucide-react`; sem ele o selo não é desenhado. */
  icone?: ComponentType<{ size?: number | string; strokeWidth?: number | string; 'aria-hidden'?: boolean }>;
  /**
   * A cor do selo — e SÓ do selo.
   *
   * Ela separa os quatro números de decisão uns dos outros num relance, e não
   * carrega significado nenhum: o valor, o rótulo e a comparação continuam em
   * preto sobre branco, e a tela inteira continua legível em escala de cinza.
   * Cor aqui é endereço, não semáforo.
   */
  tom?: 'neutro' | 'demanda' | 'mercado' | 'captura' | 'oportunidade';
  /** O valor pronto para a tela. `null` mostra travessão — nunca zero. */
  valor: ReactNode | null;
  /** A comparação, em uma linha curta: fatia, distância ou de onde veio. */
  contexto?: ReactNode;
  /**
   * Como o leitor de tela chama este número na dica — "a demanda anual".
   *
   * O padrão é o rótulo em minúsculas, que serve para a maioria. Existe porque
   * o artigo muda a frase: "Por que **a** demanda anual não aparece" é o que a
   * ficha do município já anunciava, e trocar isso mudaria o nome acessível de
   * um controle que as pessoas já conhecem.
   */
  oQue?: string;
  /** Por que não há valor, com a issue que destrava. Só aparece na dica. */
  motivoSemDado?: string;
  procedencia?: ProcedenciaDoIndicador | null;
  /** O número que a diretoria olha primeiro, quando houver um. */
  destaque?: boolean;
}) {
  const vazio = valor === null || valor === undefined;
  const nome = oQue ?? rotulo.toLowerCase();

  return (
    <div
      className={destaque ? 'dash-kpi dash-kpi-destaque' : 'dash-kpi'}
      data-kpi={rotulo}
      data-tom={tom}
    >
      <div className="dash-kpi-cabecalho">
        {/* O SELO É DECORAÇÃO COM ENDEREÇO: ele não diz nada que o rótulo já não
            diga, e por isso some do leitor de tela. O que ele faz é dar ao
            cartão uma marca que o olho reconhece de longe, para "o de demanda"
            e "o de captura" pararem de ser quatro retângulos iguais. */}
        {Icone && (
          <span className="dash-kpi-selo" aria-hidden="true">
            <Icone size={18} strokeWidth={2} />
          </span>
        )}
        <div className="dash-kpi-rotulo">
          <span>{rotulo}</span>
          <Procedencia procedencia={procedencia} oQue={nome} />
        </div>
      </div>

      {/* O TRAÇO E O MOTIVO SÃO O `ValorAusente` DE SEMPRE, e não uma segunda
          forma de dizer a mesma coisa: o rótulo da dica ("Por que … não
          aparece"), o traço e o `sem dado` do leitor de tela já estão
          padronizados na tela inteira. Duas frases para a mesma ausência é
          exatamente a inconsistência que esta fase veio acabar. */}
      <div className="dash-kpi-valor">
        {vazio ? (
          motivoSemDado ? (
            <ValorAusente motivo={motivoSemDado} oQue={nome} />
          ) : (
            <span className="dash-vazio">—</span>
          )
        ) : (
          <>
            {valor}
            {unidade && <span className="dash-kpi-unidade">{unidade}</span>}
          </>
        )}
      </div>

      {/* A LINHA DE CONTEXTO EXISTE SEMPRE, mesmo vazia: é ela que mantém os
          quatro cartões com a mesma altura sem precisar fixar altura em pixel —
          o que quebraria assim que alguém escrevesse um rótulo de duas linhas. */}
      <div className="dash-kpi-contexto">{vazio ? '' : contexto}</div>
    </div>
  );
}
