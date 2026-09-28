/**
 * O PERFIL DA VISÃO 360, como um filtro da barra dos Indicadores Geográficos (28/09/2026).
 *
 * Ele não é decorativo: TROCA DE TELA. O CEN abre o trabalho do dia e o 360 de um cliente; Gerente e Diretoria abrem o
 * painel consolidado das filiais. Era uma faixa de três botões com avatar ao lado do título — ~650 px que, numa tela
 * no desenho dos Indicadores, disputavam a linha com o "Dados atualizados" e quebravam o cabeçalho. Na barra, ele é um
 * campo como o Período, com selo de ícone e rótulo em cima.
 */

import { UserRound } from 'lucide-react';
import { InfoTooltip } from '../InfoTooltip';
import { PERFIS, type PerfilId } from './perfis';

export function FiltroDoPerfil({ perfil, aoTrocar }: { perfil: PerfilId; aoTrocar?: (p: PerfilId) => void }) {
  return (
    <label className="dash-filtro" data-bloco="perfil">
      <span className="dash-filtro-icone" aria-hidden="true">
        <UserRound size={17} strokeWidth={2} />
      </span>
      <span className="dash-filtro-corpo">
        <span className="dash-filtro-rotulo">
          Perfil
          <InfoTooltip
            rotulo="O que muda com o perfil"
            texto="Diretoria e Gerente abrem o painel consolidado das filiais em operação. O CEN abre o trabalho do dia — tarefas e clientes sem contato — e o 360 de qualquer cliente da filial do cabeçalho."
          />
        </span>
        <select value={perfil} onChange={(e) => aoTrocar?.(e.target.value as PerfilId)} disabled={!aoTrocar}>
          {PERFIS.map((p) => (
            <option key={p.id} value={p.id}>
              {p.rotulo}
            </option>
          ))}
        </select>
      </span>
    </label>
  );
}
