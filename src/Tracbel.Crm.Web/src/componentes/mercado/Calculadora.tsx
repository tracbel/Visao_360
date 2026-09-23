/**
 * A CALCULADORA DE MÁQUINAS (issue 161) — "quantas máquinas esta área comporta,
 * e quantas por ano".
 *
 * Ela **não tem conta nenhuma aqui dentro**. Toda vez que a área muda, a
 * pergunta vai ao servidor e volta pelo mesmo motor que pinta o mapa — que é o
 * critério de aceite da issue: a mesma área de um município devolve o mesmo
 * resultado que o motor gravou para ele. Uma fórmula repetida no navegador
 * divergiria no dia em que o motor mudasse.
 *
 * A lista de culturas também vem do servidor (issue 165): cultura nova entra
 * pelo Administrador e aparece aqui sem publicação.
 */

import { useMemo, useState } from 'react';
import { simularMaquinas } from '../../dados/api/territorio';
import { ErroDaApi, type ContextoDeAcesso } from '../../dados/api/http';
import { useRecurso } from '../../dados/api/useRecurso';
import type { CulturaNaCalculadora } from '../../tipos/territorio';

const nº = (v: number) => v.toLocaleString('pt-BR');

/** Arredonda para mostrar: máquina é coisa inteira, e "7,3 tratores" não existe no pátio. */
const maquinas = (v: number | null) => (v === null ? '—' : nº(Math.round(v)));

/** Um índice de mercado, em torno de 1: 1,15 é "15% acima da janela anterior". */
const indice = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export type CalculadoraProps = {
  contexto: ContextoDeAcesso;
  /** O município selecionado no mapa, quando há — a calculadora parte da área medida dele. */
  municipioCodigoIbge?: number | null;
  aoFechar: () => void;
};

export function Calculadora({ contexto, municipioCodigoIbge, aoFechar }: CalculadoraProps) {
  /** O que foi digitado, por código de cultura. Vazio = "usa a área medida". */
  const [digitado, setDigitado] = useState<Record<string, string>>({});

  /** O que já foi enviado — é ele que dispara a consulta, e não cada tecla. */
  const [enviado, setEnviado] = useState<Record<string, string>>({});

  const leitura = useRecurso(
    (sinal) => {
      const areas = Object.entries(enviado)
        .filter(([, valor]) => valor.trim() !== '')
        .map(([culturaCodigo, areaHectares]) => ({ culturaCodigo, areaHectares: areaHectares.trim() }));

      return simularMaquinas(
        contexto,
        {
          municipioCodigoIbge: municipioCodigoIbge ? String(municipioCodigoIbge) : undefined,
          areas: areas.length > 0 ? areas : undefined,
        },
        sinal,
      );
    },
    [contexto, municipioCodigoIbge, enviado],
  );

  const resultado = leitura.dados;

  // A RECUSA DO SERVIDOR VEM COM O MOTIVO — "a cultura X não tem regra" —, e é ele que a tela mostra.
  // Uma mensagem genérica aqui apagaria a única informação acionável.
  const erro = useMemo(() => {
    if (leitura.erro === null) return null;
    if (!(leitura.erro instanceof ErroDaApi)) return 'A simulação não respondeu.';
    return leitura.erro.erros.length > 0
      ? leitura.erro.erros.map((e) => e.mensagem).join(' ')
      : (leitura.erro.detalhe ?? leitura.erro.message);
  }, [leitura.erro]);

  const carregando = leitura.carregando || leitura.recarregando;
  const porCategoria = useMemo(() => resultado?.porCategoria ?? [], [resultado]);

  /** As culturas, sem repetir a mesma em duas categorias: o campo de área é um só por cultura. */
  const culturas = useMemo(() => {
    const mapa = new Map<string, CulturaNaCalculadora>();
    for (const c of resultado?.culturas ?? []) if (!mapa.has(c.codigo)) mapa.set(c.codigo, c);
    return [...mapa.values()];
  }, [resultado]);

  function simular(evento: React.FormEvent) {
    evento.preventDefault();
    setEnviado({ ...digitado });
  }

  function limpar() {
    setDigitado({});
    setEnviado({});
  }

  return (
    <aside className="card cad-cartao terr-calculadora" aria-label="Calculadora de máquinas">
      <div className="terr-calculadora-topo">
        <div>
          <div className="card-title">Calculadora de máquinas</div>
          <div className="card-subtitle">
            {resultado?.municipioNome
              ? `Parte da área medida de ${resultado.municipioNome} — mude o que quiser e simule`
              : 'Informe a área de cada cultura e veja o parque que ela comporta'}
          </div>
        </div>
        <button type="button" className="cad-th-ordenar" onClick={aoFechar} aria-label="Fechar a calculadora">
          fechar
        </button>
      </div>

      {erro && <p className="terr-aviso terr-aviso-alerta">{erro}</p>}

      <form onSubmit={simular} className="terr-calculadora-form">
        {culturas.map((c) => (
          <label key={c.codigo} className="terr-calculadora-campo">
            <span>
              {c.nome}
              <span className="cad-sub">
                {' '}
                1 a cada {nº(c.hectaresPorMaquina)} ha
                {c.areaMedidaHectares !== null
                  ? ` · medido ${nº(Math.round(c.areaMedidaHectares))} ha${c.anoDaArea ? ` (${c.anoDaArea})` : ''}`
                  : municipioCodigoIbge
                    ? ' · área não divulgada aqui'
                    : ''}
              </span>
            </span>
            <input
              type="text"
              inputMode="decimal"
              className="cad-mono"
              value={digitado[c.codigo] ?? ''}
              placeholder={c.areaMedidaHectares !== null ? nº(Math.round(c.areaMedidaHectares)) : '0'}
              onChange={(e) => setDigitado({ ...digitado, [c.codigo]: e.target.value })}
              aria-label={`Área de ${c.nome} em hectares`}
            />
          </label>
        ))}

        <div className="terr-calculadora-acoes">
          <button type="submit" disabled={carregando}>
            {carregando ? 'Calculando…' : 'Simular'}
          </button>
          <button type="button" className="cad-th-ordenar" onClick={limpar}>
            voltar à área medida
          </button>
        </div>
      </form>

      {resultado && (
        <>
          <dl className="terr-numeros">
            <dt>Parque de máquinas</dt>
            <dd>
              <strong>{maquinas(resultado.parqueDeMaquinas)}</strong>
              {resultado.areaUtilHectares !== null && (
                <span className="cad-sub"> · {nº(Math.round(resultado.areaUtilHectares))} ha úteis</span>
              )}
            </dd>
            <dt>Demanda anual</dt>
            <dd>
              {resultado.demandaAnualDeMaquinas === null ? (
                <span className="cad-sub">
                  {resultado.motivoSemDemanda === 'SemCicloDeRenovacao'
                    ? 'a regra não informou de quantos em quantos anos a máquina é trocada'
                    : 'sem parque, não há o que renovar'}
                </span>
              ) : (
                <strong>{maquinas(resultado.demandaAnualDeMaquinas)}</strong>
              )}
            </dd>
          </dl>

          {porCategoria.length > 1 && (
            <ul className="terr-recorte-linhas">
              {porCategoria.map((c) => (
                <li key={c.codigo}>
                  {c.nome}: <strong>{maquinas(c.parqueDeMaquinas)}</strong>
                  {c.demandaAnualDeMaquinas !== null && (
                    <span className="cad-sub"> · {maquinas(c.demandaAnualDeMaquinas)} por ano</span>
                  )}
                </li>
              ))}
            </ul>
          )}

          {resultado.porCultura.some((p) => p.compartilhada.length > 0) && (
            <ul className="terr-recorte-linhas">
              {resultado.porCultura
                .filter((p) => p.compartilhada.length > 0)
                .map((p) => (
                  <li key={p.culturaCodigo} className="cad-sub">
                    {p.cultura}: área compartilhada com {p.compartilhada.join(', ')} — a mesma terra, contada uma vez
                  </li>
                ))}
            </ul>
          )}

          {resultado.frase && <p className="terr-aviso">{resultado.frase}.</p>}

          {resultado.mercado && (
            <>
              <p className="terr-recorte-titulo">O momento do mercado</p>
              <ul className="terr-recorte-linhas">
                <li>
                  Crédito:{' '}
                  {resultado.mercado.indiceDeCredito === null ? (
                    <span className="cad-sub">sem índice — falta município ou parâmetro vigente</span>
                  ) : (
                    <>
                      <strong>{indice(resultado.mercado.indiceDeCredito)}</strong>
                      {resultado.mercado.faixaDoCredito && (
                        <span className="cad-sub"> · {resultado.mercado.faixaDoCredito.toLowerCase()}</span>
                      )}
                      {/* LINHA NÃO É CONTRATO: o Banco Central não publica quantidade de contrato. */}
                      <span className="cad-sub">
                        {' '}
                        · {nº(resultado.mercado.linhasDoCredito)} linhas do SICOR
                        {resultado.mercado.basePequenaNoCredito ? ' — base pequena, leia com cuidado' : ''}
                      </span>
                    </>
                  )}
                </li>
                {resultado.mercado.percepcaoDoGestor !== null && (
                  <li>
                    Percepção do gestor: <strong>{resultado.mercado.percepcaoDoGestor.toLocaleString('pt-BR')}%</strong>
                  </li>
                )}
                {resultado.mercado.porCultura.map((c) => (
                  <li key={c.culturaCodigo}>
                    {c.cultura}: fator <strong>{c.fator === null ? '—' : indice(c.fator)}</strong>
                    {c.indiceDePreco !== null && (
                      <span className="cad-sub"> · preço {indice(c.indiceDePreco)}</span>
                    )}
                    {c.demandaAjustada !== null && (
                      <span className="cad-sub"> · {maquinas(c.demandaAjustada)} por ano ajustados</span>
                    )}
                  </li>
                ))}
              </ul>

              <table className="cad-tabela terr-recorte-tabela">
                <caption className="cad-sub">
                  Cenários — cada índice na borda da faixa em que já está; a percepção do gestor não varia
                </caption>
                <thead>
                  <tr>
                    <th scope="col">Cenário</th>
                    <th scope="col">Fator</th>
                    <th scope="col">Demanda por ano</th>
                    <th scope="col">Variação</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.mercado.cenarios.map((c) => (
                    <tr key={c.nome}>
                      <td>{c.nome}</td>
                      <td className="cad-mono">{c.fator === null ? '—' : indice(c.fator)}</td>
                      <td className="cad-mono">{c.demandaAjustada === null ? '—' : maquinas(c.demandaAjustada)}</td>
                      <td className="cad-mono">
                        {c.variacaoPercentual === null
                          ? '—'
                          : `${c.variacaoPercentual.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {resultado.mercado.frase && <p className="terr-aviso">{resultado.mercado.frase}.</p>}
            </>
          )}

          <p className="cad-sub">{resultado.sobreOsCenarios}</p>
          <p className="cad-sub">Nada aqui é gravado: a simulação é uma pergunta, não um registro.</p>
        </>
      )}
    </aside>
  );
}
