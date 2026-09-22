/**
 * O EDITOR DE UMA CONEXÃO (issue 136): os campos que o tipo tem, e a senha à parte.
 *
 * A SENHA NUNCA VEM PREENCHIDA — a tela não a conhece. Deixar o campo vazio mantém a gravada; digitar troca. Ela vai
 * numa chamada própria, depois do endereço, e a resposta não a traz. "Retirar da tela" volta a conexão para a
 * variável de ambiente do servidor, se houver.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { configurarConexao, definirSegredo, removerSegredo, type ConexaoNaTela } from '../../../dados/api/integracoes';
import { useEnvio } from '../potencial/useEnvio';

const CAMPOS = ['endereco', 'porta', 'banco', 'objeto', 'usuario', 'nomeDoCabecalho', 'nome', 'descricao', 'statusEsperado', 'minutosEntreVerificacoes', 'segredo'] as const;

type Props = { conexao: ConexaoNaTela; aoSalvar: () => void; aoCancelar: () => void };

function Campo({ id, rotulo, erro, children }: { id: string; rotulo: string; erro?: string; children: React.ReactNode }) {
  return (
    <div className="form-field">
      <label htmlFor={id}>{rotulo}</label>
      {children}
      {erro && <div className="form-erro">{erro}</div>}
    </div>
  );
}

export function EditorDeConexao({ conexao: c, aoSalvar, aoCancelar }: Props) {
  const { contexto } = useContextoDeAcesso();
  const envio = useEnvio(CAMPOS);
  const [endereco, setEndereco] = useState(c.endereco ?? '');
  const [porta, setPorta] = useState(c.porta?.toString() ?? '');
  const [banco, setBanco] = useState(c.banco ?? '');
  const [objeto, setObjeto] = useState(c.objeto ?? '');
  const [usuario, setUsuario] = useState(c.usuario ?? '');
  const [cabecalho, setCabecalho] = useState(c.nomeDoCabecalho ?? '');
  const [nome, setNome] = useState(c.nome);
  const [descricao, setDescricao] = useState(c.descricao ?? '');
  const [status, setStatus] = useState(c.statusEsperado.toString());
  const [minutos, setMinutos] = useState(c.minutosEntreVerificacoes?.toString() ?? '');
  const [senha, setSenha] = useState('');

  const ehBanco = c.tipo === 'SqlServer' || c.tipo === 'MySql';
  const monitorada = c.tipo === 'Monitorada';
  const id = (campo: string) => `conexao-${c.codigo}-${campo}`;

  async function salvar() {
    const salva = await envio.enviar(async () => {
      let atual = await configurarConexao(contexto, c.codigo, {
        endereco,
        porta: porta ? Number(porta) : null,
        banco,
        objeto,
        usuario,
        nomeDoCabecalho: cabecalho,
        nome,
        descricao,
        statusEsperado: status ? Number(status) : 200,
        minutosEntreVerificacoes: minutos ? Number(minutos) : null,
      });
      if (senha) atual = await definirSegredo(contexto, c.codigo, senha);
      return atual;
    });
    setSenha('');
    if (salva) aoSalvar();
  }

  async function retirar() {
    const r = await envio.enviar(() => removerSegredo(contexto, c.codigo));
    if (r) aoSalvar();
  }

  return (
    <div className="int-editor">
      {envio.aviso && (
        <div className="config-hint adm-aviso" role="alert">
          <strong>{envio.aviso.titulo}</strong>
          {envio.aviso.texto && <div>{envio.aviso.texto}</div>}
        </div>
      )}
      <div className="form-grid">
        {monitorada && (
          <Campo id={id('nome')} rotulo="Nome" erro={envio.erros.nome}>
            <input id={id('nome')} value={nome} onChange={(e) => setNome(e.target.value)} />
          </Campo>
        )}
        <Campo id={id('endereco')} rotulo={ehBanco ? 'Servidor' : 'Endereço (URL)'} erro={envio.erros.endereco}>
          <input
            id={id('endereco')}
            value={endereco}
            placeholder={ehBanco ? 'servidor, servidor\\instancia ou servidor,porta' : 'https://…'}
            onChange={(e) => setEndereco(e.target.value)}
          />
        </Campo>
        {c.tipo === 'MySql' && (
          <Campo id={id('porta')} rotulo="Porta" erro={envio.erros.porta}>
            <input id={id('porta')} inputMode="numeric" value={porta} placeholder="3306" onChange={(e) => setPorta(e.target.value.replace(/\D/g, ''))} />
          </Campo>
        )}
        {ehBanco && (
          <Campo id={id('banco')} rotulo="Banco" erro={envio.erros.banco}>
            <input id={id('banco')} value={banco} onChange={(e) => setBanco(e.target.value)} />
          </Campo>
        )}
        {c.tipo === 'MySql' && (
          <Campo id={id('objeto')} rotulo="Visão lida" erro={envio.erros.objeto}>
            <input id={id('objeto')} value={objeto} onChange={(e) => setObjeto(e.target.value)} />
          </Campo>
        )}
        {!monitorada && (
          <Campo id={id('usuario')} rotulo="Usuário" erro={envio.erros.usuario}>
            <input id={id('usuario')} value={usuario} autoComplete="off" onChange={(e) => setUsuario(e.target.value)} />
          </Campo>
        )}
        {monitorada && (
          <>
            <Campo id={id('cabecalho')} rotulo="Cabeçalho do segredo (se a API pedir)" erro={envio.erros.nomeDoCabecalho}>
              <input id={id('cabecalho')} value={cabecalho} placeholder="Authorization, X-Api-Key…" onChange={(e) => setCabecalho(e.target.value)} />
            </Campo>
            <Campo id={id('status')} rotulo="Status esperado" erro={envio.erros.statusEsperado}>
              <input id={id('status')} inputMode="numeric" value={status} onChange={(e) => setStatus(e.target.value.replace(/\D/g, ''))} />
            </Campo>
            <Campo id={id('minutos')} rotulo="Testar sozinho a cada (minutos)" erro={envio.erros.minutosEntreVerificacoes}>
              <input id={id('minutos')} inputMode="numeric" value={minutos} placeholder="vazio: só pelo botão" onChange={(e) => setMinutos(e.target.value.replace(/\D/g, ''))} />
            </Campo>
          </>
        )}
        <Campo id={id('senha')} rotulo={monitorada ? 'Segredo do cabeçalho' : 'Senha'} erro={envio.erros.segredo}>
          <input
            id={id('senha')}
            type="password"
            autoComplete="new-password"
            value={senha}
            placeholder={c.temSegredoNaTela ? 'gravada — digite só para trocar' : 'digite para gravar'}
            onChange={(e) => setSenha(e.target.value)}
          />
        </Campo>
        {monitorada && (
          <div className="form-field form-field-full">
            <label htmlFor={id('descricao')}>Para que serve</label>
            <input id={id('descricao')} value={descricao} onChange={(e) => setDescricao(e.target.value)} />
          </div>
        )}
      </div>
      <p className="config-hint">
        A senha é guardada protegida no servidor e só abre lá: nenhuma tela, resposta ou log a mostra. Uma cópia do banco levada para
        outra máquina chega sem ela.
      </p>
      <div className="adm-acoes">
        {c.temSegredoNaTela && (
          <button type="button" className="btn btn-secondary" disabled={envio.enviando} onClick={retirar}>
            Retirar credencial da tela
          </button>
        )}
        <button type="button" className="btn btn-secondary" onClick={aoCancelar}>
          Cancelar
        </button>
        <button type="button" className="btn btn-primary" disabled={envio.enviando} onClick={salvar}>
          {envio.enviando ? 'Salvando…' : 'Salvar'}
        </button>
      </div>
    </div>
  );
}
