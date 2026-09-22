/**
 * O editor de perfil (issue 113, parte 2b): a matriz do catálogo agrupada por área, com a profundidade de cada
 * permissão.
 *
 * TRÊS MODOS: ver (perfil do sistema — fixo, com "Duplicar"), editar (perfil próprio) e novo (do zero ou a
 * partir de outro). O que a tela manda é o conjunto final; a API recusa perfil do sistema e qualquer permissão
 * maior do que a de quem edita.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import {
  criarPerfil,
  desativarPerfil,
  editarPerfil,
  reativarPerfil,
  type PerfilNaAdministracao,
  type PermissaoDoCatalogo,
} from '../../../dados/api/perfis';
import { ALCANCE, PROFUNDIDADES, agruparPorArea } from '../areasDePermissao';
import { CardConfig } from '../ConfigPartes';
import { useEnvio } from '../potencial/useEnvio';

export type ModoDoEditor =
  | { tipo: 'ver'; perfil: PerfilNaAdministracao }
  | { tipo: 'editar'; perfil: PerfilNaAdministracao }
  | { tipo: 'novo'; base: PerfilNaAdministracao | null };

type Props = {
  modo: ModoDoEditor;
  catalogo: PermissaoDoCatalogo[];
  aoGravar: (perfil: PerfilNaAdministracao) => void;
  aoDuplicar: (perfil: PerfilNaAdministracao) => void;
  aoFechar: () => void;
};

function escolhasDe(perfil: PerfilNaAdministracao | null): Record<string, string> {
  return Object.fromEntries((perfil?.permissoes ?? []).map((p) => [p.codigo, p.profundidade]));
}

export function EditorDePerfil({ modo, catalogo, aoGravar, aoDuplicar, aoFechar }: Props) {
  const { contexto } = useContextoDeAcesso();
  const envio = useEnvio(['codigo', 'nome', 'descricao', 'permissoes']);

  const perfil = modo.tipo === 'novo' ? null : modo.perfil;
  const base = modo.tipo === 'novo' ? modo.base : modo.perfil;
  const somenteLeitura = modo.tipo === 'ver';

  const [codigo, setCodigo] = useState(modo.tipo === 'novo' && base ? `${base.codigo}_PROPRIO` : '');
  const [nome, setNome] = useState(modo.tipo === 'novo' ? (base ? `${base.nome} (próprio)` : '') : (perfil?.nome ?? ''));
  const [descricao, setDescricao] = useState(base?.descricao ?? '');
  const [escolhas, setEscolhas] = useState<Record<string, string>>(escolhasDe(base));

  // NA LEITURA, SÓ O QUE O PERFIL DÁ: o catálogo inteiro com "não dá" em quase tudo faria parecer que a Gerência
  // não lê clientes — quando isso vem do Padrão. Na edição, a matriz inteira, para escolher.
  const areas = agruparPorArea(somenteLeitura ? catalogo.filter((p) => escolhas[p.codigo]) : catalogo);
  const ehOPadrao = (base ?? perfil)?.ehPadrao ?? false;
  const permissoes = Object.entries(escolhas)
    .filter(([, profundidade]) => profundidade)
    .map(([codigoDaPermissao, profundidade]) => ({ codigo: codigoDaPermissao, profundidade }));

  async function gravar() {
    const gravado = await envio.enviar(() =>
      modo.tipo === 'novo'
        ? criarPerfil(contexto, { codigo, nome, descricao, permissoes })
        : editarPerfil(contexto, perfil!.codigo, { nome, descricao, permissoes }),
    );
    if (gravado) aoGravar(gravado);
  }

  async function ligarOuDesligar() {
    if (!perfil) return;
    const gravado = await envio.enviar(() => (perfil.estaAtivo ? desativarPerfil(contexto, perfil.codigo) : reativarPerfil(contexto, perfil.codigo)));
    if (gravado) aoGravar(gravado);
  }

  const titulo = modo.tipo === 'novo' ? (base ? `Novo perfil a partir de ${base.nome}` : 'Novo perfil') : perfil!.nome;

  return (
    <CardConfig titulo={titulo}>
      {somenteLeitura && (
        <p className="config-hint">
          Perfil do sistema: vem do código e fica como está. Para ajustar, duplique e edite a cópia — depois conceda a
          cópia na tela de Usuários.
        </p>
      )}

      {envio.aviso && (
        <div className="config-hint adm-aviso" role="alert">
          <strong>{envio.aviso.titulo}</strong>
          {envio.aviso.texto && <div>{envio.aviso.texto}</div>}
        </div>
      )}

      {!somenteLeitura && (
        <div className="form-grid perfil-dados">
          {modo.tipo === 'novo' && (
            <div className="form-field">
              <label htmlFor="perfil-codigo">Código</label>
              <input id="perfil-codigo" value={codigo} placeholder="DIRETORIA_REGIONAL" onChange={(e) => setCodigo(e.target.value.toUpperCase())} />
              {envio.erros.codigo && <div className="form-erro">{envio.erros.codigo}</div>}
            </div>
          )}
          <div className="form-field">
            <label htmlFor="perfil-nome">Nome</label>
            <input id="perfil-nome" value={nome} onChange={(e) => setNome(e.target.value)} />
            {envio.erros.nome && <div className="form-erro">{envio.erros.nome}</div>}
          </div>
          <div className="form-field form-field-full">
            <label htmlFor="perfil-descricao">Descrição</label>
            <input id="perfil-descricao" value={descricao} placeholder="Para que serve" onChange={(e) => setDescricao(e.target.value)} />
          </div>
        </div>
      )}

      {!ehOPadrao && (
        <p className="conta-alcance-geral">
          {somenteLeitura ? 'O que este perfil acrescenta' : 'O que o perfil acrescenta'} ao <strong>Padrão</strong>, que todo usuário já
          recebe.
        </p>
      )}
      {somenteLeitura && areas.length === 0 && <div className="config-tabela-vazia">Este perfil não dá nenhuma permissão.</div>}

      <div className="conta-areas perfil-matriz">
        {areas.map((area) => (
          <div className="conta-area" key={area.nome}>
            <div className="conta-area-topo">
              <span className="clr-title">{area.nome}</span>
            </div>
            <ul className="perfil-permissoes">
              {area.itens.map((p) => (
                <li key={p.codigo}>
                  <span className={escolhas[p.codigo] ? '' : 'perfil-sem'}>{p.descricao}</span>
                  {somenteLeitura ? (
                    <span className="conta-alcance">{escolhas[p.codigo] ? ALCANCE[escolhas[p.codigo]] : 'não dá'}</span>
                  ) : (
                    <select
                      aria-label={`Onde vale: ${p.descricao}`}
                      value={escolhas[p.codigo] ?? ''}
                      onChange={(e) => setEscolhas({ ...escolhas, [p.codigo]: e.target.value })}
                    >
                      <option value="">não dá</option>
                      {PROFUNDIDADES.map((d) => (
                        <option key={d} value={d}>
                          {ALCANCE[d]}
                        </option>
                      ))}
                    </select>
                  )}
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>

      <div className="adm-acoes">
        {perfil && (
          <button type="button" className="btn btn-secondary" onClick={() => aoDuplicar(perfil)}>
            Duplicar
          </button>
        )}
        {modo.tipo === 'editar' && (
          <button type="button" className="btn btn-secondary" disabled={envio.enviando} onClick={ligarOuDesligar}>
            {perfil!.estaAtivo ? 'Desativar perfil' : 'Reativar perfil'}
          </button>
        )}
        <button type="button" className="btn btn-secondary" onClick={aoFechar}>
          Fechar
        </button>
        {!somenteLeitura && (
          <button type="button" className="btn btn-primary" disabled={!nome.trim() || envio.enviando} onClick={gravar}>
            {modo.tipo === 'novo' ? 'Criar perfil' : 'Salvar alterações'}
          </button>
        )}
      </div>
    </CardConfig>
  );
}
