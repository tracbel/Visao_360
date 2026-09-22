/**
 * "+ CONECTAR NOVA API" (issue 136): uma API cadastrada pela tela para ser MONITORADA — GET no endereço, o status que
 * conta como no ar e, se ela pedir, um segredo num cabeçalho. Trazer dado dela para o CRM exige código; monitorar,
 * não.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { criarApiMonitorada, definirSegredo } from '../../../dados/api/integracoes';
import { useEnvio } from '../potencial/useEnvio';

const CAMPOS = ['codigo', 'nome', 'descricao', 'endereco', 'nomeDoCabecalho', 'statusEsperado', 'minutosEntreVerificacoes', 'segredo'] as const;

export function NovaApiMonitorada({ aoCriar, aoCancelar }: { aoCriar: () => void; aoCancelar: () => void }) {
  const { contexto } = useContextoDeAcesso();
  const envio = useEnvio(CAMPOS);
  const [codigo, setCodigo] = useState('');
  const [nome, setNome] = useState('');
  const [endereco, setEndereco] = useState('');
  const [descricao, setDescricao] = useState('');
  const [cabecalho, setCabecalho] = useState('');
  const [segredo, setSegredo] = useState('');
  const [status, setStatus] = useState('200');
  const [minutos, setMinutos] = useState('60');

  async function criar() {
    const criada = await envio.enviar(async () => {
      const nova = await criarApiMonitorada(contexto, {
        codigo,
        nome,
        descricao,
        endereco,
        nomeDoCabecalho: cabecalho,
        statusEsperado: status ? Number(status) : 200,
        minutosEntreVerificacoes: minutos ? Number(minutos) : null,
      });
      return segredo ? await definirSegredo(contexto, nova.codigo, segredo) : nova;
    });
    setSegredo('');
    if (criada) aoCriar();
  }

  const campo = (id: string, rotulo: string, valor: string, mudar: (v: string) => void, extra: Partial<React.InputHTMLAttributes<HTMLInputElement>> = {}) => (
    <div className="form-field">
      <label htmlFor={`nova-api-${id}`}>{rotulo}</label>
      <input id={`nova-api-${id}`} value={valor} onChange={(e) => mudar(e.target.value)} {...extra} />
      {envio.erros[id] && <div className="form-erro">{envio.erros[id]}</div>}
    </div>
  );

  return (
    <div className="int-editor int-nova">
      <strong>Nova API monitorada</strong>
      {envio.aviso && (
        <div className="config-hint adm-aviso" role="alert">
          <strong>{envio.aviso.titulo}</strong>
          {envio.aviso.texto && <div>{envio.aviso.texto}</div>}
        </div>
      )}
      <div className="form-grid">
        {campo('codigo', 'Código', codigo, (v) => setCodigo(v.toUpperCase()), { placeholder: 'CLIMA_TEMPO' })}
        {campo('nome', 'Nome', nome, setNome)}
        {campo('endereco', 'Endereço (URL do GET)', endereco, setEndereco, { placeholder: 'https://…' })}
        {campo('statusEsperado', 'Status esperado', status, (v) => setStatus(v.replace(/\D/g, '')), { inputMode: 'numeric' })}
        {campo('minutosEntreVerificacoes', 'Testar sozinho a cada (minutos)', minutos, (v) => setMinutos(v.replace(/\D/g, '')), {
          inputMode: 'numeric',
          placeholder: 'vazio: só pelo botão',
        })}
        {campo('nomeDoCabecalho', 'Cabeçalho do segredo (se pedir)', cabecalho, setCabecalho, { placeholder: 'X-Api-Key' })}
        {campo('segredo', 'Segredo', segredo, setSegredo, { type: 'password', autoComplete: 'new-password' })}
        {campo('descricao', 'Para que serve', descricao, setDescricao)}
      </div>
      <div className="adm-acoes">
        <button type="button" className="btn btn-secondary" onClick={aoCancelar}>
          Cancelar
        </button>
        <button type="button" className="btn btn-primary" disabled={envio.enviando} onClick={criar}>
          {envio.enviando ? 'Cadastrando…' : 'Cadastrar e monitorar'}
        </button>
      </div>
    </div>
  );
}
