/**
 * A TRILHA DOS PARÂMETROS — toda vigência já registrada, com quem registrou, quando e por quê, e quem revogou
 * (issue 77: "alteração aparece na trilha com o autor").
 *
 * Revogar só aparece onde a API aceita: vigência que ainda não passou de hoje, para quem tem a permissão
 * daquele tipo. O resto se corrige com uma vigência nova — o passado não se reescreve.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { revogarVigencia } from '../../../dados/api/potencial';
import { formatarDataHora } from '../../../telas/cadastro/formato';
import { AvisoDoFormulario, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { DialogoConfirmacao } from '../../cadastro/DialogoConfirmacao';
import { useEnvio } from './useEnvio';
import { dataCurta, ehRevogavel, ROTULO_DO_ESTADO, type LinhaDoHistorico } from './vigencias';

function Estado({ linha }: { linha: LinhaDoHistorico }) {
  const { rotulo, cor } = ROTULO_DO_ESTADO[linha.estado];
  return (
    <span className="int-status" style={{ background: `${cor}20`, color: cor, borderColor: `${cor}40` }}>
      <span className="int-dot" style={{ background: cor }} />
      {rotulo}
    </span>
  );
}

export function HistoricoDosParametros({
  linhas,
  hoje,
  podeRevogar,
  aoRevogar,
}: {
  linhas: LinhaDoHistorico[];
  hoje: string;
  /** Se o perfil pode revogar este tipo de vigência. */
  podeRevogar: (linha: LinhaDoHistorico) => boolean;
  aoRevogar: (mensagem: string) => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [alvo, setAlvo] = useState<LinhaDoHistorico | null>(null);
  const [motivo, setMotivo] = useState('');
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(['motivo']);

  function abrir(linha: LinhaDoHistorico) {
    setMotivo('');
    limparErro('motivo');
    setAlvo(linha);
  }

  async function confirmar() {
    if (!alvo) return;
    const feito = await enviar(() => revogarVigencia(contexto, alvo.alvo, motivo));
    if (feito !== null) {
      aoRevogar(`Vigência de ${dataCurta(alvo.vigencia.vigenteDesde)} revogada (${alvo.tipo.toLowerCase()}${alvo.chave === '—' ? '' : ` · ${alvo.chave}`}).`);
      setAlvo(null);
    }
  }

  return (
    <>
      <div className="cad-tabela-wrap">
        <table className="cad-tabela pot-tabela">
          <thead>
            <tr>
              <th scope="col">Situação</th>
              <th scope="col">Parâmetro</th>
              <th scope="col" className="pot-col-valores">Valores</th>
              <th scope="col">Vigente desde</th>
              <th scope="col">Registrado por</th>
              <th scope="col" className="pot-col-justificativa">Justificativa</th>
              <th scope="col" aria-label="Ações" />
            </tr>
          </thead>
          <tbody>
            {linhas.map((linha) => (
              <tr key={linha.id}>
                <td>
                  <Estado linha={linha} />
                </td>
                <td>
                  {linha.tipo}
                  {linha.chave !== '—' && <div className="pot-sub">{linha.chave}</div>}
                </td>
                <td>{linha.resumo}</td>
                <td className="cad-mono">{dataCurta(linha.vigencia.vigenteDesde)}</td>
                <td>
                  {linha.vigencia.informadoPor ? (
                    <>
                      {linha.vigencia.informadoPor}
                      <div className="pot-sub">{formatarDataHora(linha.vigencia.informadoEm)}</div>
                    </>
                  ) : (
                    // A semente não tem hora: nasceu na migração.
                    'semente da migração'
                  )}
                </td>
                <td>
                  <div className="pot-recorte" title={linha.vigencia.justificativa}>
                    {linha.vigencia.justificativa}
                  </div>
                  {linha.vigencia.revogadoEm && (
                    <div className="pot-sub">
                      Revogada por {linha.vigencia.revogadoPor ?? '—'} em {formatarDataHora(linha.vigencia.revogadoEm)}:{' '}
                      {linha.vigencia.motivoDaRevogacao}
                    </div>
                  )}
                </td>
                <td>
                  {ehRevogavel(linha.vigencia, hoje) && podeRevogar(linha) && (
                    <button type="button" className="btn btn-ghost btn-sm" onClick={() => abrir(linha)}>
                      Revogar
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {alvo && (
        <DialogoConfirmacao
          titulo="Revogar esta vigência?"
          subtitulo={`${alvo.tipo}${alvo.chave === '—' ? '' : ` · ${alvo.chave}`} · vigente desde ${dataCurta(alvo.vigencia.vigenteDesde)}`}
          rotuloConfirmar="Revogar"
          podeConfirmar={motivo.trim().length > 0}
          gravando={enviando}
          aoCancelar={() => setAlvo(null)}
          aoConfirmar={confirmar}
        >
          <p>
            A vigência <strong>deixa de valer</strong>, e a data fica livre para uma nova. Ela continua na trilha, com o
            motivo e o seu nome.
          </p>
          {aviso && (
            <AvisoDoFormulario titulo={aviso.titulo}>
              <span>{aviso.texto}</span>
            </AvisoDoFormulario>
          )}
          <CampoTextoLongo
            rotulo="Motivo"
            obrigatorio
            valor={motivo}
            aoMudar={(v) => {
              setMotivo(v);
              limparErro('motivo');
            }}
            erro={erros.motivo}
            exemplo="Por que esta vigência não deve valer."
          />
        </DialogoConfirmacao>
      )}
    </>
  );
}
