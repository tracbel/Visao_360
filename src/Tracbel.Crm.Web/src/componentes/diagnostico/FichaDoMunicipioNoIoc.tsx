/**
 * A FICHA DE UM MUNICÍPIO NO DIAGNÓSTICO — por que ele tem este IOC e o que fazer lá (28/09/2026).
 *
 * A situação e o plano de ação saíram da tabela e moram aqui: na coluna estreita da tabela, as duas frases quebravam
 * em cinco ou seis linhas e cada município ocupava a altura de uma tela. Aqui elas têm largura para serem lidas, ao
 * lado dos sete componentes que as explicam.
 *
 * SEM MUNICÍPIO ESCOLHIDO, A FICHA MOSTRA OS CINCO DE MAIOR IOC — o ponto de partida de quem abre a tela para saber
 * onde agir.
 */

import { X } from 'lucide-react';
import { ValorAusente } from '../comum/ValorAusente';
import type { MunicipioNoDiagnostico } from '../../tipos/mercado';
import {
  acoesDoPlano,
  COMPONENTES,
  CURTO_DA_CLASSE,
  n,
  pct,
  ROTULO_DA_CLASSE,
  SEM_ART,
  variacao,
} from './diagnostico';

export function FichaDoMunicipioNoIoc({
  municipio: m,
  primeiros,
  semArt,
  aoEscolher,
  aoFechar,
}: {
  municipio: MunicipioNoDiagnostico | null;
  /** Os de maior IOC, para a ficha vazia. */
  primeiros: MunicipioNoDiagnostico[];
  semArt: boolean;
  aoEscolher: (codigo: number) => void;
  aoFechar: () => void;
}) {
  if (!m) {
    return (
      <div className="card cad-cartao diag-ficha" data-bloco="ficha">
        <div className="diag-ficha-cabecalho">
          <div>
            <div className="card-title">Onde agir primeiro</div>
            <div className="card-subtitle">Os cinco municípios de maior IOC no recorte. Clique num deles, no mapa ou na tabela.</div>
          </div>
        </div>
        <ol className="diag-primeiros">
          {primeiros.map((p) => (
            <li key={p.codigoIbge}>
              <button type="button" className="diag-primeiro" onClick={() => aoEscolher(p.codigoIbge)}>
                <span className="diag-primeiro-nome">
                  {p.nome}
                  <span className="cad-sub">{p.loja ?? p.regiao}</span>
                </span>
                {p.ioc !== null && p.classe !== null && <SeloDoIoc ioc={p.ioc} classe={p.classe} />}
              </button>
              {p.planoDeAcao && <div className="diag-primeiro-plano">{acoesDoPlano(p.planoDeAcao)[0]}</div>}
            </li>
          ))}
        </ol>
      </div>
    );
  }

  const acoes = acoesDoPlano(m.planoDeAcao);
  const porClasse = m.clientesPorClasse;

  return (
    <div className="card cad-cartao diag-ficha" data-bloco="ficha" aria-live="polite">
      <div className="diag-ficha-cabecalho">
        <div>
          <div className="card-title">{m.nome}</div>
          <div className="card-subtitle">
            {m.regiao}
            {m.loja && <> · {m.loja}</>}
            {m.culturaPrincipal && <> · cultura principal: {m.culturaPrincipal}</>}
          </div>
        </div>
        <div className="diag-ficha-acoes">
          {m.ioc !== null && m.classe !== null ? (
            <SeloDoIoc ioc={m.ioc} classe={m.classe} grande />
          ) : (
            <ValorAusente motivo="Nenhum componente com peso tem dado neste município." oQue={`o IOC de ${m.nome}`} />
          )}
          <button type="button" className="diag-fechar" onClick={aoFechar} aria-label={`Fechar a ficha de ${m.nome}`}>
            <X size={16} strokeWidth={2} aria-hidden="true" />
          </button>
        </div>
      </div>

      {m.situacao && <p className="diag-situacao">{capitalizar(m.situacao)}.</p>}
      {acoes.length > 0 && (
        <ul className="diag-acoes" aria-label="Plano de ação">
          {acoes.map((a) => (
            <li key={a}>{a}</li>
          ))}
        </ul>
      )}

      <div className="diag-ficha-numeros">
        <Numero rotulo="Demanda/ano" valor={m.demandaEstrutural} complemento={m.demandaAjustada !== null ? `ajustada ${n(m.demandaAjustada)}` : undefined} motivo="O município não tem demanda estimada nesta categoria: falta regra de potencial ou área plantada." />
        <Numero rotulo="Meta/ano" valor={m.metaDePlanejamento} complemento="demanda × share-alvo" motivo="Sem share-alvo vigente para a categoria." />
        <Numero
          rotulo="Vendidas"
          valor={m.vendidasNoPeriodo}
          casas={0}
          complemento={m.penetracao !== null ? `penetração ${pct(m.penetracao)}` : undefined}
          motivo={semArt ? SEM_ART : 'Sem venda do ART para este município.'}
        />
        <Numero
          rotulo="Cobertura"
          valor={m.cobertura === null ? null : m.cobertura * 100}
          casas={0}
          sufixo="%"
          complemento={m.cobertura !== null ? `${n(m.cobertos, 0)} de ${n(m.vinculosComCadencia, 0)} vínculos` : undefined}
          motivo="Nenhum vínculo de carteira com cadência declarada neste município."
        />
        <Numero
          rotulo="Crédito"
          texto={m.indiceDeCredito === null ? null : variacao(m.indiceDeCredito)}
          complemento={m.creditoBasePequena ? 'base pequena' : 'SICOR, momento recente'}
          motivo="O SICOR não formou o índice de crédito deste município."
        />
        <Numero
          rotulo="Preço"
          texto={m.indiceDePreco === null ? null : variacao(m.indiceDePreco)}
          complemento={m.culturaPrincipal ?? undefined}
          motivo="Sem série de preço para a cultura principal."
        />
      </div>

      <div className="diag-clientes" aria-label="Clientes do município">
        <div className="diag-clientes-total">
          <strong>{n(m.clientes, 0)}</strong> clientes
          {m.clientesEmCarteira !== null && <> · {n(m.clientesEmCarteira, 0)} em carteira</>}
          {' · '}
          {n(m.clientesQueCompraram, 0)} compraram no período
        </div>
        {porClasse && m.clientes > 0 && (
          <div className="diag-clientes-classes">
            {(
              [
                ['A', porClasse.a],
                ['B', porClasse.b],
                ['C', porClasse.c],
                ['D', porClasse.d],
                ['sem classe', porClasse.semClasse],
              ] as const
            ).map(([rotulo, q]) => (
              <span key={rotulo} className="diag-clientes-classe">
                <span className="cad-sub">{rotulo}</span> {n(q, 0)}
              </span>
            ))}
          </div>
        )}
      </div>

      <div className="diag-componentes">
        <div className="diag-subtitulo">Os sete componentes (0 a 1 — 1 é muita oportunidade)</div>
        <ul>
          {COMPONENTES.map((c) => {
            const valor = m.componentes?.[c.chave] ?? null;
            return (
              <li key={c.chave}>
                <span className="diag-componente-nome">{c.rotulo}</span>
                <span className="diag-barra" aria-hidden="true">
                  {valor !== null && <span style={{ width: `${valor * 100}%` }} />}
                </span>
                <span className="diag-componente-valor cad-mono">{valor === null ? 'fora da conta' : n(valor, 2)}</span>
                <span className="diag-componente-mede">{c.mede}</span>
              </li>
            );
          })}
        </ul>
        {m.componentesAusentes.length > 0 && (
          <p className="diag-ausentes">
            <strong>Fora da conta:</strong> {m.componentesAusentes.join('; ')}. Os pesos dos outros componentes foram
            normalizados.
          </p>
        )}
      </div>
    </div>
  );
}

/** O IOC com a classe escrita ao lado — a cor sozinha não diz nada a quem não a distingue. */
export function SeloDoIoc({ ioc, classe, grande = false }: { ioc: number; classe: NonNullable<MunicipioNoDiagnostico['classe']>; grande?: boolean }) {
  return (
    <span className={`diag-ioc diag-classe-${classe}${grande ? ' diag-ioc-grande' : ''}`} title={ROTULO_DA_CLASSE[classe]}>
      <strong>{n(ioc)}</strong>
      <span className="diag-classe-rotulo">{CURTO_DA_CLASSE[classe]}</span>
    </span>
  );
}

function Numero({
  rotulo,
  valor,
  texto,
  casas = 1,
  sufixo = '',
  complemento,
  motivo,
}: {
  rotulo: string;
  valor?: number | null;
  /** O valor já formatado, quando não é um número simples (a variação do crédito). */
  texto?: string | null;
  casas?: number;
  sufixo?: string;
  complemento?: string;
  motivo: string;
}) {
  const conteudo = texto !== undefined ? texto : valor === null || valor === undefined ? null : `${n(valor, casas)}${sufixo}`;
  return (
    <div className="diag-numero">
      <span className="diag-numero-rotulo">{rotulo}</span>
      <span className="diag-numero-valor">
        {conteudo === null ? <ValorAusente motivo={motivo} oQue={rotulo.toLowerCase()} /> : conteudo}
      </span>
      {conteudo !== null && complemento && <span className="diag-numero-complemento">{complemento}</span>}
    </div>
  );
}

function capitalizar(texto: string) {
  return texto.charAt(0).toUpperCase() + texto.slice(1);
}
