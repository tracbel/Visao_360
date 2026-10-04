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
import { CampoDoFiltro } from '../comum/CampoDoFiltro';

export function FiltroDoPerfil({ perfil, aoTrocar }: { perfil: PerfilId; aoTrocar?: (p: PerfilId) => void }) {
  return (
    <CampoDoFiltro
      icone={UserRound}
      rotulo={
        <>
          Perfil
          <InfoTooltip
            rotulo="O que muda com o perfil"
            texto="Diretoria e Gerente abrem o painel consolidado das filiais em operação. O CEN abre o trabalho do dia — tarefas e clientes sem contato — e o 360 de qualquer cliente da filial do cabeçalho."
          />
        </>
      }
      bloco="perfil"
    >
      <select value={perfil} onChange={(e) => aoTrocar?.(e.target.value as PerfilId)} disabled={!aoTrocar}>
        {PERFIS.map((p) => (
          <option key={p.id} value={p.id}>
            {p.rotulo}
          </option>
        ))}
      </select>
    </CampoDoFiltro>
  );
}
