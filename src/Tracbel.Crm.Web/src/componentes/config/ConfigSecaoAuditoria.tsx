/**
 * Aba TI e Integrações › Auditoria e logs — porte de `renderSecaoAuditoria`
 * (prototipo/referencia/assets/app.js linha 5826). Os eventos já eram uma
 * lista local à função no original (não um `CONFIG_*`/JSON), então seguem
 * locais aqui também.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a busca, os dois filtros e o "Exportar log" passaram a funcionar.
 *
 * Eram quatro controles decorativos numa tela de auditoria, que é justamente
 * onde alguém procura uma linha específica: digitar o IP e a lista não mudar
 * leva à conclusão errada de que o evento não existe. Como os eventos já estão
 * todos na memória, filtrar e exportar é barato, e o texto do rodapé — que diz
 * que a exportação é registrada — passa a ter algo a que se referir.
 */
import { useMemo, useState } from 'react';
import { baixarCsv, carimboDeData } from '../../dados/exportarCsv';
import type { EventoAuditoria } from '../../tipos/configuracoes';
import { CardConfig } from './ConfigPartes';

/**
 * A janela de tempo de cada opção, contada a partir do evento mais recente da
 * lista — e não de `new Date()`, que faria a tela mostrar zero evento fora do
 * dia em que os dados de exemplo foram escritos.
 */
const JANELAS: { rotulo: string; horas: number | null }[] = [
  { rotulo: 'Últimos 7 dias', horas: 24 * 7 },
  { rotulo: 'Últimas 24h', horas: 24 },
  { rotulo: 'Últimas 4h', horas: 4 },
];

/** O tipo de cada evento, deduzido do que ele diz — não há coluna para isso. */
function tipoDoEvento(e: EventoAuditoria): string {
  const acao = e.acao.toLowerCase();
  if (acao.startsWith('aprovou') || acao.includes('aprovação')) return 'Aprovação';
  if (acao.includes('exportou')) return 'Exportação';
  if (acao.includes('sync') || acao.includes('integração') || acao.includes('rate limit')) return 'Integração';
  return 'Configuração';
}

/** `2026-08-25 18:32` não é ISO. Vira data sem depender do parser do navegador. */
function paraData(texto: string): Date {
  const [dia, hora] = texto.split(' ');
  const [a, m, d] = dia.split('-').map(Number);
  const [h, min] = hora.split(':').map(Number);
  return new Date(a, m - 1, d, h, min);
}

const EVENTOS: EventoAuditoria[] = [
  {
    data: '2026-08-25 18:32',
    usuario: 'Marina Costa · Admin Comercial',
    acao: 'Alterou motivo de perda #4 "Concorrente Case IH"',
    ip: '10.42.18.203',
  },
  {
    data: '2026-08-25 17:45',
    usuario: 'Rafael Menezes · Diretor',
    acao: 'Aprovou desconto 12% · oportunidade OP-2026-08473',
    ip: '10.42.18.7',
  },
  {
    data: '2026-08-25 15:40',
    usuario: 'Sistema · WhatsApp API',
    acao: 'Rate limit atingido · integração marcada degradada',
    ip: 'external',
  },
  {
    data: '2026-08-25 14:22',
    usuario: 'Hugo Rocha · Admin TI',
    acao: 'Atualizou SLA aprovação Diretor de 36h para 48h',
    ip: '10.42.18.221',
  },
  {
    data: '2026-08-25 11:08',
    usuario: 'Cláudia Batista · Gerente MT Norte',
    acao: 'Reatribuiu 3 clientes do CEN Ricardo para Fernanda',
    ip: '10.42.18.145',
  },
  {
    data: '2026-08-25 09:15',
    usuario: 'Sistema · Protheus sync',
    acao: 'Sync noturno completo · 42 clientes atualizados',
    ip: 'internal',
  },
  {
    data: '2026-08-24 19:40',
    usuario: 'João Ribeiro · CEN MT Norte',
    acao: 'Exportou 24 clientes como CSV',
    ip: '10.42.18.98',
  },
  {
    data: '2026-08-24 16:22',
    usuario: 'Rafael Menezes · Diretor',
    acao: 'Alterou desconto máximo Presidência de 25% para 20%',
    ip: '10.42.18.7',
  },
];

export function ConfigSecaoAuditoria() {
  const [busca, setBusca] = useState('');
  const [janela, setJanela] = useState('Últimos 7 dias');
  const [tipo, setTipo] = useState('');

  const maisRecente = useMemo(
    () => Math.max(...EVENTOS.map((e) => paraData(e.data).getTime())),
    [],
  );

  const eventos = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    const horas = JANELAS.find((j) => j.rotulo === janela)?.horas ?? null;
    const limite = horas === null ? -Infinity : maisRecente - horas * 3600 * 1000;
    return EVENTOS.filter((e) => {
      if (paraData(e.data).getTime() < limite) return false;
      if (tipo && tipoDoEvento(e) !== tipo) return false;
      if (termo && !`${e.usuario} ${e.acao} ${e.ip}`.toLowerCase().includes(termo)) return false;
      return true;
    });
  }, [busca, janela, tipo, maisRecente]);

  function exportarLog() {
    baixarCsv(
      `auditoria-${carimboDeData()}`,
      ['Timestamp', 'Usuário', 'Tipo', 'Ação', 'IP'],
      eventos.map((e) => [e.data, e.usuario, tipoDoEvento(e), e.acao, e.ip]),
    );
  }

  return (
    <CardConfig titulo="Eventos recentes">
      <div className="config-filtros-inline">
        <input
          type="search"
          aria-label="Buscar evento por usuário, ação ou IP"
          placeholder="Buscar por usuário, ação, IP..."
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
        />
        <select aria-label="Janela de tempo" value={janela} onChange={(e) => setJanela(e.target.value)}>
          {JANELAS.map((j) => (
            <option key={j.rotulo}>{j.rotulo}</option>
          ))}
        </select>
        <select aria-label="Tipo de ação" value={tipo} onChange={(e) => setTipo(e.target.value)}>
          <option value="">Todas as ações</option>
          <option value="Configuração">Configuração</option>
          <option value="Aprovação">Aprovação</option>
          <option value="Integração">Integração</option>
          <option value="Exportação">Exportação</option>
        </select>
        <button type="button" className="btn-config-inline" style={{ marginLeft: 'auto' }} onClick={exportarLog}>
          Exportar log
        </button>
      </div>

      <table className="config-tabela">
        <thead>
          <tr>
            <th>Timestamp</th>
            <th>Usuário</th>
            <th>Ação</th>
            <th>IP</th>
          </tr>
        </thead>
        <tbody>
          {eventos.length === 0 ? (
            <tr>
              <td colSpan={4} className="config-tabela-vazia">
                Nenhum evento com esses filtros. A busca compara usuário, ação e IP por trecho — tente um termo mais
                curto, ou abra a janela de tempo.
              </td>
            </tr>
          ) : (
            eventos.map((e, i) => (
              <tr key={i}>
                <td className="mono muted">{e.data}</td>
                <td>{e.usuario}</td>
                <td>{e.acao}</td>
                <td className="mono muted">{e.ip}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
      <div className="config-hint" style={{ marginTop: 12 }}>
        Mostrando {eventos.length} de {EVENTOS.length} eventos. Retenção de logs: 12 meses on-line · 24 meses
        arquivo. Exportação requer justificativa e é registrada.
      </div>
    </CardConfig>
  );
}
