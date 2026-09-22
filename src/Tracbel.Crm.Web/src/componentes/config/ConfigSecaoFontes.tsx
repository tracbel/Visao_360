/**
 * Configurações › TI e Integrações › Fontes públicas — o painel de fontes do potencial (issue 77).
 *
 * Uma linha por fonte que o servidor carrega sozinho: quando rodou pela última vez, que período e quantos
 * municípios da ADR o banco cobre, o que foi recusado e quando roda de novo. **A fonte atrasada ou sem dado
 * fica em destaque**, com a frase que diz por quê — é o aceite da issue.
 *
 * A última atualização é a rodada que a própria carga gravou no banco (não uma data digitada, como na aba
 * "Controle de Fontes" da planilha). Rodada que falha não grava nada: por isso a fonte aparece atrasada.
 */

import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterFontesPublicas } from '../../dados/api/potencial';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarData, formatarDataHora, formatarNumero } from '../../telas/cadastro/formato';
import type { FontePublicaResumo, PainelDeFontesPublicas, SituacaoDaFonte } from '../../tipos/potencial';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { CardConfig } from './ConfigPartes';
import '../../estilos/potencial.css';

const SITUACAO: Record<SituacaoDaFonte, { rotulo: string; cor: string; classe: string }> = {
  EmDia: { rotulo: 'Em dia', cor: '#22C55E', classe: '' },
  Atrasada: { rotulo: 'Atrasada', cor: '#EF4444', classe: 'pot-fonte-atrasada' },
  SemDado: { rotulo: 'Sem dado', cor: '#F59E0B', classe: 'pot-fonte-semdado' },
};

function Periodo({ fonte }: { fonte: FontePublicaResumo }) {
  if (!fonte.periodoInicial) return <>—</>;
  return fonte.periodoInicial === fonte.periodoFinal ? (
    <>{fonte.periodoInicial}</>
  ) : (
    <>
      {fonte.periodoInicial} a {fonte.periodoFinal}
    </>
  );
}

/** A tabela do painel — separada da leitura para o teste montá-la com um painel pronto. */
export function TabelaDeFontes({ painel }: { painel: PainelDeFontesPublicas }) {
  return (
    <>
      <div className="pot-resumo" role="status">
        <span>
          <strong>{painel.fontes.length}</strong> fontes
        </span>
        <span className={painel.atrasadas > 0 ? 'pot-alerta' : undefined}>
          <strong>{painel.atrasadas}</strong> {painel.atrasadas === 1 ? 'atrasada' : 'atrasadas'}
        </span>
        <span className={painel.semDado > 0 ? 'pot-alerta' : undefined}>
          <strong>{painel.semDado}</strong> sem dado
        </span>
        <span>
          cobertura sobre os <strong>{painel.municipiosDaAdr}</strong> municípios da ADR
        </span>
      </div>
      <div className="cad-tabela-wrap">
        <table className="cad-tabela pot-tabela">
          <thead>
            <tr>
              <th scope="col" className="pot-col-fonte">Fonte</th>
              <th scope="col">Situação</th>
              <th scope="col">Última atualização</th>
              <th scope="col">Período</th>
              <th scope="col">Linhas</th>
              <th scope="col">Municípios da ADR</th>
              <th scope="col">Recusas</th>
              <th scope="col">Próxima execução</th>
            </tr>
          </thead>
          <tbody>
            {painel.fontes.map((fonte) => {
              const situacao = SITUACAO[fonte.situacao];
              return (
                <tr key={fonte.fluxo} className={situacao.classe || undefined} data-situacao={fonte.situacao}>
                  <td>
                    <strong>{fonte.nome}</strong>
                    <div className="pot-sub">{fonte.orgao}</div>
                    <div className="pot-sub">{fonte.oQueTraz}</div>
                  </td>
                  <td>
                    <span
                      className="int-status"
                      style={{ background: `${situacao.cor}20`, color: situacao.cor, borderColor: `${situacao.cor}40` }}
                    >
                      <span className="int-dot" style={{ background: situacao.cor }} />
                      {situacao.rotulo}
                    </span>
                    {fonte.situacao !== 'EmDia' && <div className="pot-motivo">{fonte.motivo}</div>}
                  </td>
                  <td className="cad-mono">
                    {fonte.ultimaAtualizacaoEm ? formatarDataHora(fonte.ultimaAtualizacaoEm) : 'nunca'}
                    {fonte.ultimaAtualizacaoEm && (
                      <div className="pot-sub">
                        {formatarNumero(fonte.registrosLidos)} lidas · {formatarNumero(fonte.registrosGravados)} gravadas
                      </div>
                    )}
                  </td>
                  <td className="cad-mono">
                    <Periodo fonte={fonte} />
                  </td>
                  <td className="cad-mono">{formatarNumero(fonte.linhas)}</td>
                  <td className="cad-mono">
                    {fonte.municipiosCobertos === null ? '—' : `${fonte.municipiosCobertos} de ${painel.municipiosDaAdr}`}
                  </td>
                  <td>
                    {fonte.recusasPendentes === 0 ? (
                      '—'
                    ) : (
                      <>
                        <span className="cad-mono">{formatarNumero(fonte.recusasPendentes)}</span>
                        {fonte.exemplosDeRecusa.map((e) => (
                          <div className="pot-sub" key={e}>
                            {e}
                          </div>
                        ))}
                      </>
                    )}
                  </td>
                  <td className="cad-mono" title={`Rotina ${fonte.rotina}: ${fonte.agenda}`}>
                    {fonte.proximaExecucaoEm ? formatarData(fonte.proximaExecucaoEm) : 'desligada'}
                    <div className="pot-sub">{fonte.agenda}</div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </>
  );
}

export function ConfigSecaoFontes() {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => obterFontesPublicas(contexto, sinal), [contexto.empresa, contexto.usuario]);

  return (
    <CardConfig titulo="Fontes públicas do potencial">
      {leitura.carregando && <BlocoCarregando oQue="as fontes públicas" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {leitura.dados && <TabelaDeFontes painel={leitura.dados} />}
      <div className="config-hint" style={{ marginTop: 16 }}>
        As fontes são lidas pelo próprio servidor, em duas rotinas — as fontes anuais e os preços, custos e crédito —, com a agenda que se muda em
        Configurações › Administração › Integrações. A rotina que nunca rodou e encontra tabela vazia roda na primeira volta do orquestrador. Nenhuma fonte depende hoje de envio manual
        de arquivo; o CEPEA, que dependeria, aguarda a decisão sobre a licença (D-P11).
      </div>
    </CardConfig>
  );
}
