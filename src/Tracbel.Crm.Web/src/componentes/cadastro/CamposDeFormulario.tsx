/**
 * Os campos do formulário de cadastro — rótulo, valor, erro e ajuda, juntos.
 *
 * A REGRA QUE ESTES COMPONENTES CARREGAM, e que nenhuma tela precisa lembrar:
 *
 * - **Todo campo tem rótulo de verdade** (`<label for>`), não um texto ao lado.
 *   É o que faz o leitor de tela anunciar o campo e o clique no rótulo focar o
 *   campo (documento 05 §3).
 * - **O erro é do campo, não da tela.** A mensagem vive em `aria-describedby` e
 *   o campo vai a `aria-invalid`, então quem navega por teclado ouve o problema
 *   ao chegar no campo — em vez de descobrir num aviso no topo que já rolou para
 *   fora da tela.
 * - **Campo de catálogo é `<select>`**, nunca `<input>` com sugestão. Não existe
 *   caminho, aqui, para digitar um código que não está na lista.
 * - **Campo somente leitura mostra de onde veio.** O que é do Protheus não é
 *   editável no CRM; quem precisa corrigir, corrige na origem (documento 05 §5).
 */

import { useId, type ReactNode } from 'react';
import type { ItemDeSelecao } from '../../tipos/api';

type Comum = {
  rotulo: string;
  /** A mensagem que a API devolveu para ESTE campo. Vazio quando não há. */
  erro?: string;
  /** Uma frase curta de ajuda, mostrada abaixo do campo enquanto não há erro. */
  ajuda?: ReactNode;
  obrigatorio?: boolean;
  /** Ocupa a linha inteira da grade. */
  largo?: boolean;
  desabilitado?: boolean;
};

/** A moldura comum: rótulo, campo, erro e ajuda. */
function Moldura({
  id,
  rotulo,
  erro,
  ajuda,
  obrigatorio,
  largo,
  children,
}: Comum & { id: string; children: ReactNode }) {
  return (
    <div className={`form-field${largo ? ' form-field-full' : ''}`}>
      <label htmlFor={id}>
        {rotulo}
        {obrigatorio && (
          <span className="cad-obrigatorio" aria-hidden="true">
            *
          </span>
        )}
      </label>
      {children}
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

/** Campo de texto livre. Só onde não há catálogo — e há poucos lugares assim. */
export function CampoTexto({
  valor,
  aoMudar,
  exemplo,
  tipo = 'text',
  ...comum
}: Comum & {
  valor: string;
  aoMudar: (valor: string) => void;
  exemplo?: string;
  /** `date` envia `aaaa-mm-dd`, o formato que a API lê nas datas de vigência. */
  tipo?: 'text' | 'number' | 'date';
}) {
  const id = useId();
  return (
    <Moldura id={id} {...comum}>
      <input
        id={id}
        type={tipo}
        value={valor}
        // O EXEMPLO SOME NA FICHA: texto de exemplo em campo desabilitado é lido
        // como valor gravado, e a pessoa acredita que o cliente tem um CNPJ que
        // ele não tem. Campo vazio na ficha fica vazio mesmo.
        placeholder={comum.desabilitado ? undefined : exemplo}
        disabled={comum.desabilitado}
        // O asterisco é enfeite visual (`aria-hidden`); quem anuncia a
        // obrigatoriedade para o leitor de tela é isto. `required` nativo não
        // entra: ele bloquearia o envio no navegador, e quem recusa campo a
        // campo — com a frase certa — é a API.
        aria-required={comum.obrigatorio || undefined}
        aria-invalid={comum.erro ? true : undefined}
        aria-describedby={comum.erro ? `${id}-erro` : comum.ajuda ? `${id}-ajuda` : undefined}
        onChange={(e) => aoMudar(e.target.value)}
      />
    </Moldura>
  );
}

/**
 * Texto de mais de uma linha — a justificativa de um parâmetro, o motivo de uma revogação. Mesma moldura,
 * mesmo erro de campo.
 */
export function CampoTextoLongo({
  valor,
  aoMudar,
  exemplo,
  ...comum
}: Comum & { valor: string; aoMudar: (valor: string) => void; exemplo?: string }) {
  const id = useId();
  return (
    <Moldura id={id} {...comum}>
      <textarea
        id={id}
        value={valor}
        rows={3}
        placeholder={comum.desabilitado ? undefined : exemplo}
        disabled={comum.desabilitado}
        aria-required={comum.obrigatorio || undefined}
        aria-invalid={comum.erro ? true : undefined}
        aria-describedby={comum.erro ? `${id}-erro` : comum.ajuda ? `${id}-ajuda` : undefined}
        onChange={(e) => aoMudar(e.target.value)}
      />
    </Moldura>
  );
}

/**
 * Campo alimentado por catálogo.
 *
 * NÃO ACEITA DIGITAÇÃO, e é o ponto inteiro: as opções vêm de
 * `/api/v1/catalogos` e o que a tela manda é o CÓDIGO, nunca a descrição. O
 * código é estável e vai para o histórico; a descrição é o que a pessoa lê e
 * pode ser corrigida sem quebrar registro antigo.
 */
export function CampoSelecao({
  valor,
  aoMudar,
  itens,
  vazio = 'Selecione…',
  ...comum
}: Comum & {
  valor: string;
  aoMudar: (valor: string) => void;
  itens: ItemDeSelecao[];
  /** O rótulo da opção "nenhum". Some quando o campo é obrigatório e já tem valor. */
  vazio?: string;
}) {
  const id = useId();
  // Item aposentado continua aparecendo quando é o que está gravado: escondê-lo
  // faria o `<select>` mostrar outra coisa e o usuário salvar sem perceber.
  const gravadoForaDaLista = valor !== '' && !itens.some((i) => i.codigo === valor);

  return (
    <Moldura id={id} {...comum}>
      <select
        id={id}
        value={valor}
        disabled={comum.desabilitado}
        aria-required={comum.obrigatorio || undefined}
        aria-invalid={comum.erro ? true : undefined}
        aria-describedby={comum.erro ? `${id}-erro` : comum.ajuda ? `${id}-ajuda` : undefined}
        onChange={(e) => aoMudar(e.target.value)}
      >
        <option value="">{vazio}</option>
        {gravadoForaDaLista && <option value={valor}>{valor} (fora do catálogo atual)</option>}
        {itens.map((item) => (
          <option key={item.codigo} value={item.codigo}>
            {item.descricao}
          </option>
        ))}
      </select>
    </Moldura>
  );
}

/**
 * Um valor que a tela mostra e não deixa editar.
 *
 * Usado onde a API não aceita alteração — chassi e origem do equipamento, por
 * exemplo — e onde o dado é de outro sistema. O texto de `ajuda` diz POR QUE
 * não se edita, senão o campo cinza vira um mistério.
 */
export function CampoSomenteLeitura({
  valor,
  rotulo,
  ajuda,
  largo,
}: {
  valor: ReactNode;
  rotulo: string;
  ajuda?: ReactNode;
  largo?: boolean;
}) {
  const id = useId();
  return (
    // `role="group"` com `aria-labelledby`, e NÃO um `<output>`: `<output>` é
    // região viva, e um valor parado dentro dela faria o leitor de tela
    // reanunciar a ficha inteira a cada re-render. Aqui o leitor anuncia o
    // rótulo do grupo e depois o valor, uma vez, ao chegar.
    <div className={`form-field${largo ? ' form-field-full' : ''}`} role="group" aria-labelledby={id}>
      <span className="cad-rotulo-leitura" id={id}>
        {rotulo}
        <svg viewBox="0 0 24 24" width="10" height="10" fill="none" stroke="currentColor" strokeWidth={2.4} aria-hidden="true">
          <rect x="4" y="11" width="16" height="10" rx="2" />
          <path d="M8 11V7a4 4 0 0 1 8 0v4" />
        </svg>
      </span>
      <span className="cad-valor-leitura">{valor}</span>
      {ajuda && <span className="cad-ajuda-campo">{ajuda}</span>}
    </div>
  );
}

/**
 * O aviso do topo do formulário.
 *
 * SÓ APARECE PARA O QUE NÃO COUBE EM CAMPO NENHUM — conflito de concorrência,
 * recusa de estado, ou uma mensagem endereçada a um campo que esta tela não
 * mostra. Erro de campo NÃO passa por aqui: ele acende o campo, que é onde a
 * pessoa vai corrigir.
 */
export function AvisoDoFormulario({
  titulo,
  children,
  tom = 'erro',
}: {
  titulo: string;
  children?: ReactNode;
  tom?: 'erro' | 'atencao' | 'sucesso';
}) {
  return (
    <div className={`cad-aviso cad-aviso-${tom}`} role="alert">
      <strong>{titulo}</strong>
      {children}
    </div>
  );
}
