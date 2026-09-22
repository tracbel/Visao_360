/**
 * UMA CONEXÃO no cartão "Sistemas conectados" (issue 136) — o desenho do protótipo (`renderSecaoIntegracoes`), com
 * dado real: onde está, de onde vem a credencial, quem usa, e a última verificação.
 *
 * "Testar" roda NO SERVIDOR, só leitura, e fica no histórico com quem apertou. "Configurar" abre o editor aqui mesmo.
 * A senha nunca aparece: a tela só sabe se há uma, de onde vem e quem a gravou.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import {
  desativarConexao,
  listarVerificacoes,
  reativarConexao,
  testarConexao,
  type ConexaoNaTela,
  type TipoDeConexao,
} from '../../../dados/api/integracoes';
import { useRecurso } from '../../../dados/api/useRecurso';
import { formatarDataHora } from '../../../telas/cadastro/formato';
import { useEnvio } from '../potencial/useEnvio';
import { EditorDeConexao } from './EditorDeConexao';

const ICONE: Record<TipoDeConexao, string> = {
  ApiRest: '🏭',
  SqlServer: '🗄️',
  MySql: '🗄️',
  FontePublica: '🌐',
  Monitorada: '📡',
};

const TIPO: Record<TipoDeConexao, string> = {
  ApiRest: 'API REST',
  SqlServer: 'SQL Server',
  MySql: 'MySQL',
  FontePublica: 'Fonte pública',
  Monitorada: 'API monitorada',
};

export function Situacao({ ok, nunca = 'Nunca testada' }: { ok: boolean | null; nunca?: string }) {
  const s = ok === null ? { l: nunca, cor: '#94A3B8' } : ok ? { l: 'No ar', cor: '#22C55E' } : { l: 'Com falha', cor: '#EF4444' };
  return (
    <span className="int-status" style={{ background: `${s.cor}20`, color: s.cor, borderColor: `${s.cor}40` }}>
      <span className="int-dot" style={{ background: s.cor }} />
      {s.l}
    </span>
  );
}

function credencial(c: ConexaoNaTela): string {
  if (c.tipo === 'Monitorada') return c.temSegredoNaTela ? 'segredo gravado na tela' : 'sem segredo (a API é aberta)';
  switch (c.origemDaCredencial) {
    case 'Tela':
      return `gravada na tela${c.segredoAlteradoPor ? ` por ${c.segredoAlteradoPor}` : ''}${c.segredoAlteradoEm ? ` em ${formatarDataHora(c.segredoAlteradoEm)}` : ''}`;
    case 'Ambiente':
      return 'variável de ambiente do servidor';
    case 'Nenhuma':
      return 'não configurada';
    default:
      return 'não precisa — fonte aberta';
  }
}

function endereco(c: ConexaoNaTela): string {
  if (!c.endereco) return c.origemDaCredencial === 'Ambiente' ? 'na variável de ambiente do servidor' : 'não configurado';
  if (c.tipo === 'MySql') return `${c.endereco}:${c.porta ?? 3306} / ${c.banco ?? '—'} · ${c.objeto ?? '—'}`;
  if (c.tipo === 'SqlServer') return `${c.endereco} / ${c.banco ?? '—'}`;
  return c.endereco;
}

function Historico({ codigo }: { codigo: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => listarVerificacoes(contexto, codigo, sinal), [contexto.empresa, codigo]);
  if (leitura.carregando) return <div className="muted">Carregando o histórico…</div>;
  if (leitura.erro) return <div className="muted">Não foi possível ler o histórico.</div>;
  if (!leitura.dados?.length) return <div className="muted">Ainda não foi testada.</div>;
  return (
    <ul className="int-historico">
      {leitura.dados.map((v) => (
        <li key={`${v.verificadaEm}-${v.resumo}`}>
          <Situacao ok={v.ok} />
          <span className="mono">{formatarDataHora(v.verificadaEm)}</span>
          <span className="muted">{v.verificadaPor ?? 'orquestrador'} · {v.latenciaMs} ms</span>
          <span>{v.resumo}</span>
        </li>
      ))}
    </ul>
  );
}

type Props = { conexao: ConexaoNaTela; podeAdministrar: boolean; aoMudar: () => void };

export function CartaoDeConexao({ conexao: c, podeAdministrar, aoMudar }: Props) {
  const { contexto } = useContextoDeAcesso();
  const envio = useEnvio([]);
  const [editando, setEditando] = useState(false);
  const [historico, setHistorico] = useState(false);
  const [resultado, setResultado] = useState<{ ok: boolean; resumo: string } | null>(null);

  async function testar() {
    const r = await envio.enviar(() => testarConexao(contexto, c.codigo));
    if (r) {
      setResultado({ ok: r.ok, resumo: r.resumo });
      aoMudar();
    }
  }

  async function ligarOuDesligar() {
    const r = await envio.enviar(() => (c.estaAtiva ? desativarConexao(contexto, c.codigo) : reativarConexao(contexto, c.codigo)));
    if (r) aoMudar();
  }

  return (
    <div className={`integracao-item ${c.estaAtiva ? '' : 'int-desativada'}`}>
      <div className="int-icon" aria-hidden>
        {ICONE[c.tipo]}
      </div>
      <div className="int-info">
        <div className="int-header">
          <strong>{c.nome}</strong>
          <span className="int-tipo">{TIPO[c.tipo]}</span>
          <Situacao ok={c.ultimaVerificacaoOk} />
          {!c.estaAtiva && <span className="int-tipo">fora do monitoramento</span>}
        </div>
        <div className="int-meta">
          <div>
            <span>Endereço:</span> <span className="mono">{endereco(c)}</span>
          </div>
          <div>
            <span>Credencial:</span> {c.origemDaCredencial === 'Nenhuma' ? <strong className="int-falta">{credencial(c)}</strong> : credencial(c)}
          </div>
          <div>
            <span>Usada por:</span> {c.rotinas.length > 0 ? c.rotinas.join(', ') : c.tipo === 'Monitorada' ? 'monitoramento' : 'a busca ao vivo'}
          </div>
          <div>
            <span>Última verificação:</span>{' '}
            {c.ultimaVerificacaoEm ? <span className="mono">{formatarDataHora(c.ultimaVerificacaoEm)}</span> : <span className="muted">nunca</span>}
            {c.minutosEntreVerificacoes ? ` · a cada ${c.minutosEntreVerificacoes} min` : ''}
          </div>
        </div>
        {c.descricao && <div className="int-notas">{c.descricao}</div>}
        {c.ultimaVerificacaoResumo && !resultado && <div className="int-resumo muted">{c.ultimaVerificacaoResumo}</div>}
        {resultado && (
          <div className={`int-resultado ${resultado.ok ? 'int-ok' : 'int-erro'}`} role="status">
            {resultado.ok ? 'Teste passou: ' : 'Teste falhou: '}
            {resultado.resumo}
          </div>
        )}
        {envio.aviso && (
          <div className="config-hint adm-aviso" role="alert">
            <strong>{envio.aviso.titulo}</strong>
            {envio.aviso.texto && <div>{envio.aviso.texto}</div>}
          </div>
        )}
        {historico && <Historico codigo={c.codigo} />}
        {editando && (
          <EditorDeConexao
            conexao={c}
            aoSalvar={() => {
              setEditando(false);
              aoMudar();
            }}
            aoCancelar={() => setEditando(false)}
          />
        )}
      </div>
      <div className="int-acoes">
        {podeAdministrar && (
          <button type="button" className="btn-config-inline" disabled={envio.enviando} onClick={testar}>
            {envio.enviando ? 'Testando…' : 'Testar'}
          </button>
        )}
        {podeAdministrar && c.tipo !== 'FontePublica' && (
          <button type="button" className="btn-config-inline" onClick={() => setEditando((v) => !v)} aria-expanded={editando}>
            Configurar
          </button>
        )}
        <button type="button" className="btn-config-inline" onClick={() => setHistorico((v) => !v)} aria-expanded={historico}>
          Histórico
        </button>
        {podeAdministrar && c.tipo === 'Monitorada' && (
          <button type="button" className="btn-config-inline" disabled={envio.enviando} onClick={ligarOuDesligar}>
            {c.estaAtiva ? 'Desativar' : 'Reativar'}
          </button>
        )}
      </div>
    </div>
  );
}
