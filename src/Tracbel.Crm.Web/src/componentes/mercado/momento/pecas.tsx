/**
 * AS PEÇAS DAS CINCO ABAS DO MOMENTO (fidelidade às maquetes, 23/09/2026).
 *
 * As cinco maquetes seguem o mesmo padrão: uma fileira de quatro cartões de
 * resumo tingidos, dois ou três painéis lado a lado e uma tabela de
 * detalhamento. As peças moram aqui, uma vez, para as cinco abas saírem com o
 * mesmo corpo de letra, o mesmo respiro e a mesma forma de dizer "sem dado".
 *
 * POR QUE NÃO O `CartaoDeIndicador` DO TOPO. Aquele é o número de decisão da
 * página — rótulo em cima, valor de 30px, selo pequeno. O cartão do Momento é
 * outro nível de leitura, e a maquete o desenha de outro jeito: ícone grande à
 * esquerda, rótulo e valor empilhados, a variação embaixo e, na Composição, a
 * pílula da variação à direita. Mudar o do topo para servir aos dois mexeria
 * nos quatro KPIs, que são de outra fase.
 *
 * SEM DADO É O `ValorAusente` DE SEMPRE — o traço, o "sem dado" do leitor de
 * tela e a dica "Por que … não aparece" com o motivo e a issue. Nada de número
 * de exemplo, nunca.
 */

import { EllipsisVertical, type LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';
import { InfoTooltip } from '../../InfoTooltip';
import { frasesDaProcedencia } from '../../comum/comparacoes';
import { ValorAusente } from '../../comum/ValorAusente';
import type { ProcedenciaDoIndicador } from '../../../tipos/territorio';
import { percentualComSinal, sentido, tomDoSentido } from './formatos';

/** A tinta do cartão — endereço, não semáforo: nada aqui quer dizer bom ou ruim. */
export type TomDoCartao = 'verde' | 'azul' | 'roxo' | 'laranja' | 'neutro';

/** A fileira de quatro cartões: 4 → 2 → 1 pela largura do conteúdo. */
export function FileiraDeCartoes({ children }: { children: ReactNode }) {
  return <div className="mom-cartoes">{children}</div>;
}

/**
 * UM CARTÃO DE RESUMO — ícone, rótulo com ⓘ, valor, e a linha de apoio.
 *
 * A LINHA DE APOIO EXISTE SEMPRE, mesmo vazia: é ela que mantém os quatro
 * cartões na mesma altura sem fixar altura em pixel.
 */
export function CartaoDoMomento({
  icone: Icone,
  tom,
  rotulo,
  dica,
  procedencia,
  oQue,
  valor,
  unidade,
  motivoSemDado,
  apoio,
  lateral,
  acao,
}: {
  icone: LucideIcon;
  tom: TomDoCartao;
  rotulo: string;
  /** O que o número é e de onde vem — a dica ao lado do rótulo. */
  dica?: ReactNode;
  procedencia?: ProcedenciaDoIndicador | null;
  /** Como o leitor de tela chama o número; o padrão é o rótulo em minúsculas. */
  oQue?: string;
  /** O valor pronto; `null` mostra o traço com o motivo. */
  valor: ReactNode | null;
  unidade?: ReactNode;
  motivoSemDado?: string;
  /** A linha de baixo: variação, contexto ou a frase curta da maquete. */
  apoio?: ReactNode;
  /** O que fica à direita do corpo — a pílula da variação, na Composição. */
  lateral?: ReactNode;
  /** A ação da ponta — o chevron da Rentabilidade. */
  acao?: ReactNode;
}) {
  const nome = oQue ?? rotulo.toLowerCase();
  const vazio = valor === null || valor === undefined;

  return (
    <div className="mom-cartao" data-tom={tom} data-cartao={rotulo}>
      <span className="mom-cartao-icone" aria-hidden="true">
        <Icone size={26} strokeWidth={2} />
      </span>
      <div className="mom-cartao-corpo">
        <div className="mom-cartao-rotulo">
          <span>{rotulo}</span>
          {/* UMA DICA SÓ POR NÚMERO, com duas partes: o que ele é e de onde
              vem (issue 167). A maquete tem um ⓘ por cartão; dois lado a lado
              faziam o leitor adivinhar qual abrir. */}
          {(dica || procedencia) && (
            <InfoTooltip
              rotulo={procedencia ? `De onde vem ${nome}` : `O que é ${nome}`}
              texto={
                <div className="mom-detalhe">
                  {dica && <div data-parte="explicacao">{dica}</div>}
                  {procedencia && <p data-parte="procedencia">{frasesDaProcedencia(procedencia)}</p>}
                </div>
              }
            />
          )}
        </div>
        <div className="mom-cartao-valor">
          {vazio ? (
            <ValorAusente motivo={motivoSemDado ?? `${rotulo} não tem dado neste recorte.`} oQue={nome} />
          ) : (
            <>
              {valor}
              {/* O espaço é para o leitor de tela e a cópia ("4 municípios"); na
                  tela quem separa é o `gap`. */}
              {unidade && ' '}
              {unidade && <span className="mom-cartao-unidade">{unidade}</span>}
            </>
          )}
        </div>
        <div className="mom-cartao-apoio">{apoio}</div>
      </div>
      {lateral && <div className="mom-cartao-lateral">{lateral}</div>}
      {acao}
    </div>
  );
}

/**
 * A VARIAÇÃO DE UM NÚMERO — seta, percentual com sinal e a base da comparação.
 *
 * A SETA E O SINAL CARREGAM O SENTIDO; a cor só reforça. Sem base (a janela
 * anterior é zero, ou a série não existe), o que sai é o traço com o motivo.
 */
export function Variacao({
  fracao,
  base,
  motivoSemBase,
  oQue,
}: {
  fracao: number | null;
  /** Contra o quê — "vs. 12 meses anteriores". */
  base: string;
  motivoSemBase: string;
  oQue: string;
}) {
  if (fracao === null)
    return (
      <span className="mom-variacao">
        <ValorAusente motivo={motivoSemBase} oQue={oQue} />
        <span className="mom-variacao-base">{base}</span>
      </span>
    );

  return (
    <span className="mom-variacao">
      <span className="mom-variacao-numero" data-sentido={tomDoSentido(fracao)}>
        <span aria-hidden="true">{sentido(fracao)}</span> {percentualComSinal(fracao)}
      </span>
      <span className="mom-variacao-base">{base}</span>
    </span>
  );
}

/**
 * UM PAINEL DENTRO DA ABA — título com ⓘ, subtítulo de uma linha, e à direita
 * o controle (seletor, legenda ou botão), como na maquete.
 */
export function PainelDoMomento({
  titulo,
  dica,
  subtitulo,
  direita,
  children,
  area,
  ...resto
}: {
  titulo: string;
  dica?: ReactNode;
  subtitulo?: ReactNode;
  /** Controle, legenda ou ação à direita do título. */
  direita?: ReactNode;
  children: ReactNode;
  /** Nome da área na grade da aba — o CSS decide a largura relativa por ele. */
  area?: string;
} & React.HTMLAttributes<HTMLElement>) {
  return (
    <section className="mom-painel" data-area={area} {...resto}>
      {/* O CONTROLE MORA NA LINHA DO TÍTULO, e o subtítulo embaixo, na largura
          inteira — como na maquete. Juntos numa caixa só, um subtítulo longo
          empurrava a legenda para uma linha própria. */}
      <header className="mom-painel-cabecalho">
        <div className="mom-painel-linha">
          <h3 className="mom-painel-titulo">
            {titulo}
            {dica && <InfoTooltip texto={dica} rotulo={`Como ler ${titulo.toLowerCase()}`} />}
          </h3>
          {direita && <div className="mom-painel-direita">{direita}</div>}
        </div>
        {subtitulo && <p className="mom-painel-subtitulo">{subtitulo}</p>}
      </header>
      {children}
    </section>
  );
}

/** A linha de painéis lado a lado — as proporções de cada maquete moram no CSS. */
export function LinhaDePaineis({ variante, children }: { variante: string; children: ReactNode }) {
  return (
    <div className="mom-paineis" data-variante={variante}>
      {children}
    </div>
  );
}

/**
 * O SELETOR DA MAQUETE — rótulo ao lado e o campo com a seta.
 *
 * DESLIGADO QUANDO NÃO HÁ O QUE ESCOLHER, e com o motivo na dica: um seletor
 * que troca uma opção por outra sem mudar nada na tela é a promessa vazia que
 * esta tela não faz.
 */
export function Seletor<T extends string>({
  rotulo,
  valor,
  opcoes,
  aoMudar,
  motivoDesligado,
  rotuloVisivel = true,
}: {
  rotulo: string;
  valor: T;
  opcoes: readonly { id: T; rotulo: string }[];
  aoMudar?: (id: T) => void;
  /** Quando presente, o seletor fica desligado e isto vai na dica ao lado. */
  motivoDesligado?: string;
  /** A maquete às vezes mostra só o campo; o rótulo continua para o leitor de tela. */
  rotuloVisivel?: boolean;
}) {
  // A DICA FICA FORA DO `<label>`: dentro, o nome dela entraria no nome
  // acessível do campo, e o leitor de tela anunciaria "Visualizar Por que…".
  return (
    <span className="mom-seletor">
      <label className="mom-seletor">
        <span className={rotuloVisivel ? 'mom-seletor-rotulo' : 'cad-so-leitor'}>{rotulo}</span>
        <select
          value={valor}
          disabled={motivoDesligado !== undefined || !aoMudar}
          onChange={(e) => aoMudar?.(e.target.value as T)}
        >
          {opcoes.map((o) => (
            <option key={o.id} value={o.id}>
              {o.rotulo}
            </option>
          ))}
        </select>
      </label>
      {motivoDesligado && <InfoTooltip texto={motivoDesligado} rotulo={`Por que ${rotulo.toLowerCase()} está desligado`} />}
    </span>
  );
}

/** A legenda da maquete: bolinha (ou traço) colorida e o nome da série. */
export function Legenda({
  itens,
}: {
  itens: readonly { nome: string; cor: string; forma?: 'ponto' | 'linha' | 'anel' }[];
}) {
  return (
    <ul className="mom-legenda">
      {itens.map((i) => (
        <li key={i.nome}>
          <span
            className="mom-legenda-marca"
            data-forma={i.forma ?? 'ponto'}
            style={{ '--mom-cor': i.cor } as React.CSSProperties}
            aria-hidden="true"
          />
          {i.nome}
        </li>
      ))}
    </ul>
  );
}

/**
 * O ⋮ DA PONTA DA LINHA — o detalhe que a maquete não mostra no corpo.
 *
 * É O `InfoTooltip` com outro gatilho: abre com o ponteiro, o foco e o toque,
 * fecha com Esc. Ele só LÊ — o balão não recebe clique —, e é para isso que
 * serve: guardar a competência, a fonte e as parcelas de cada linha sem
 * despejá-las na tabela (decisão 3 do usuário: nada de substância é apagado).
 */
export function MenuDaLinha({ rotulo, children }: { rotulo: string; children: ReactNode }) {
  return (
    <span className="mom-menu-da-linha">
      <InfoTooltip rotulo={rotulo} texto={<div className="mom-detalhe">{children}</div>}>
        <EllipsisVertical size={15} strokeWidth={2} aria-hidden="true" />
      </InfoTooltip>
    </span>
  );
}

/**
 * UM GRÁFICO SEM SÉRIE — a moldura, a grade e o eixo no mesmo lugar, e no meio
 * a frase curta com o motivo na dica (decisão 1 do usuário).
 *
 * A ALTURA É FIXA, como a de todo gráfico do bloco: é o que impede o painel de
 * mudar de tamanho quando a série um dia chegar — e o que impede a página de
 * crescer sozinha, que é a "rolagem infinita" que já aconteceu aqui.
 */
export function GraficoSemSerie({
  altura,
  frase,
  motivo,
  oQue,
}: {
  altura: number;
  frase: string;
  motivo: string;
  oQue: string;
}) {
  return (
    <div className="mom-grafico-vazio" style={{ height: altura }} data-grafico-vazio>
      <div className="mom-grafico-vazio-grade" aria-hidden="true">
        {Array.from({ length: 5 }, (_, i) => (
          <span key={i} />
        ))}
      </div>
      <p className="mom-grafico-vazio-frase">
        {frase}
        <ValorAusente motivo={motivo} oQue={oQue} />
      </p>
    </div>
  );
}
