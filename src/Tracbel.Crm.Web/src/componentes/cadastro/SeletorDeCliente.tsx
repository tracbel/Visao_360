/**
 * O campo "cliente" do cadastro de equipamento — busca com sugestão, nunca texto solto.
 *
 * REGRA DO DOCUMENTO 05 §2, última linha da tabela: cliente, contato,
 * equipamento e oportunidade são escolhidos por BUSCA COM SUGESTÃO. O que a tela
 * manda é a CHAVE do cliente; o nome digitado nunca vira dado.
 *
 * POR QUE NÃO UM `<select>` COM TODOS OS CLIENTES: a listagem tem teto de 200
 * linhas por página, e a base de uma filial passa disso. Um `<select>` cheio
 * mentiria por omissão — o cliente existiria e não estaria na lista. A busca vai
 * à API a cada digitação e mostra o que ela achou.
 *
 * O QUE A BUSCA DA API FAZ HOJE: nome e nome fantasia por trecho, documento por
 * valor inteiro. Pedaço de CNPJ não encontra — é a dívida D-2 do documento 23, e
 * o texto de ajuda do campo diz isso em vez de deixar o usuário concluir que o
 * cliente não existe.
 */

import { useEffect, useId, useRef, useState } from 'react';
import { listarClientes } from '../../dados/api/clientes';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import type { ClienteResumo } from '../../tipos/api';

type Props = {
  rotulo: string;
  /** A chave do cliente escolhido, ou vazio. */
  valor: string;
  /** O nome do escolhido, para a tela não precisar buscar de novo só para exibir. */
  nome: string;
  aoEscolher: (chave: string, nome: string) => void;
  erro?: string;
  ajuda?: string;
  obrigatorio?: boolean;
  largo?: boolean;
  desabilitado?: boolean;
};

/** Espera antes de ir à API, para não disparar uma busca por tecla digitada. */
const ESPERA_MS = 300;

export function SeletorDeCliente({
  rotulo,
  valor,
  nome,
  aoEscolher,
  erro,
  ajuda,
  obrigatorio,
  largo,
  desabilitado,
}: Props) {
  const id = useId();
  const { contexto } = useContextoDeAcesso();
  const [termo, setTermo] = useState('');
  const [aberto, setAberto] = useState(false);
  const [buscando, setBuscando] = useState(false);
  const [achados, setAchados] = useState<ClienteResumo[]>([]);
  const [falha, setFalha] = useState<string | null>(null);
  const caixa = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!aberto) return;
    const controlador = new AbortController();
    const relogio = setTimeout(() => {
      setBuscando(true);
      setFalha(null);
      listarClientes(contexto, {
        pagina: 1,
        tamanho: 20,
        termo,
        situacao: '',
        tipoDePessoa: '',
        ordenarPor: 'Nome',
        descendente: false,
        incluirInativos: false,
      }, controlador.signal)
        .then((resposta) => {
          if (controlador.signal.aborted) return;
          setAchados(resposta.dados.itens);
          setBuscando(false);
        })
        .catch((causa: unknown) => {
          if (controlador.signal.aborted) return;
          setAchados([]);
          setBuscando(false);
          setFalha(causa instanceof Error ? causa.message : String(causa));
        });
    }, ESPERA_MS);

    return () => {
      clearTimeout(relogio);
      controlador.abort();
    };
  }, [aberto, termo, contexto]);

  // Fechar ao clicar fora: a lista aberta cobrindo os campos de baixo atrapalha
  // mais do que ajuda quando a pessoa já escolheu e seguiu adiante.
  useEffect(() => {
    if (!aberto) return;
    function aoClicar(evento: MouseEvent) {
      if (!caixa.current?.contains(evento.target as Node)) setAberto(false);
    }
    document.addEventListener('mousedown', aoClicar);
    return () => document.removeEventListener('mousedown', aoClicar);
  }, [aberto]);

  return (
    <div className={`form-field${largo ? ' form-field-full' : ''}`} ref={caixa}>
      <label htmlFor={id}>
        {rotulo}
        {obrigatorio && (
          <span className="cad-obrigatorio" aria-hidden="true">
            *
          </span>
        )}
      </label>

      {valor && !aberto ? (
        <div className="cad-cliente-escolhido">
          <span className="cad-cliente-nome">{nome || valor}</span>
          {!desabilitado && (
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => {
                setTermo('');
                setAberto(true);
              }}
            >
              Trocar
            </button>
          )}
        </div>
      ) : (
        <input
          id={id}
          type="text"
          role="combobox"
          autoComplete="off"
          aria-expanded={aberto}
          aria-controls={`${id}-lista`}
          aria-invalid={erro ? true : undefined}
          aria-describedby={erro ? `${id}-erro` : ajuda ? `${id}-ajuda` : undefined}
          disabled={desabilitado}
          placeholder="Digite o nome ou o documento do cliente"
          value={termo}
          onFocus={() => setAberto(true)}
          onChange={(e) => {
            setTermo(e.target.value);
            setAberto(true);
          }}
          onKeyDown={(e) => {
            if (e.key === 'Escape') setAberto(false);
          }}
        />
      )}

      {aberto && (
        <ul className="cad-sugestoes" id={`${id}-lista`} role="listbox" aria-label={`Resultados de ${rotulo}`}>
          {buscando && <li className="cad-sugestao-vazia">Buscando…</li>}
          {!buscando && falha && <li className="cad-sugestao-vazia">{falha}</li>}
          {!buscando && !falha && achados.length === 0 && (
            <li className="cad-sugestao-vazia">
              Nenhum cliente encontrado nesta filial. O documento é comparado inteiro — pedaço de
              CNPJ não encontra (dívida D-2 do documento 23).
            </li>
          )}
          {!buscando &&
            !falha &&
            achados.map((cliente) => (
              <li key={cliente.chave}>
                <button
                  type="button"
                  role="option"
                  aria-selected={cliente.chave === valor}
                  className="cad-sugestao"
                  onClick={() => {
                    aoEscolher(cliente.chave, cliente.nomeRazao);
                    setAberto(false);
                  }}
                >
                  <span className="cad-sugestao-nome">{cliente.nomeRazao}</span>
                  <span className="cad-sugestao-meta">
                    {cliente.documento ?? 'sem documento'} · {cliente.situacao}
                  </span>
                </button>
              </li>
            ))}
        </ul>
      )}

      {erro ? (
        <span className="cad-erro-campo" id={`${id}-erro`} role="alert">
          {erro}
        </span>
      ) : (
        ajuda && (
          <span className="cad-ajuda-campo" id={`${id}-ajuda`}>
            {ajuda}
          </span>
        )
      )}
    </div>
  );
}
