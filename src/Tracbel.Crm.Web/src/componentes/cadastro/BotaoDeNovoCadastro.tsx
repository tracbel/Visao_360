/**
 * O botão "Novo …" das listas de cadastro.
 *
 * EM "TODAS AS FILIAIS" ELE FICA DESLIGADO, e diz por quê. O registro novo nasce
 * na filial do contexto, e nesse modo não há uma filial escolhida — a API recusa
 * o cadastro (`ContextoAcesso.MensagemDeCadastroEmTodasAsFiliais`). Oferecer um
 * formulário que vai ser recusado no fim seria pior do que não oferecer.
 *
 * O PORQUÊ FICA NA DICA ⓘ AO LADO, e não mais no `title` (29/09/2026, #293 bloco 5): o `title` de um botão desligado
 * não abre pelo teclado nem no toque, e o motivo era justamente o que a pessoa precisava ler.
 */

import { Link } from 'react-router-dom';
import { TODAS_AS_FILIAIS } from '../../dados/api/acesso';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { InfoTooltip } from '../InfoTooltip';
import '../../estilos/ficha-do-cadastro.css';

const ICONE_MAIS = (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
    <line x1="12" y1="5" x2="12" y2="19" />
    <line x1="5" y1="12" x2="19" y2="12" />
  </svg>
);

export function BotaoDeNovoCadastro({ para, rotulo }: { para: string; rotulo: string }) {
  const { contexto } = useContextoDeAcesso();

  if (contexto.empresa === TODAS_AS_FILIAIS) {
    return (
      <span className="cad-novo-desligado">
        <button type="button" className="btn btn-primary" disabled>
          {ICONE_MAIS}
          {rotulo}
        </button>
        <InfoTooltip
          texto="Em “Todas as filiais” não dá para cadastrar: escolha no seletor a filial em que o registro vai ficar."
          rotulo={`Por que “${rotulo}” está desligado`}
        />
      </span>
    );
  }

  return (
    <Link to={para} className="btn btn-primary">
      {ICONE_MAIS}
      {rotulo}
    </Link>
  );
}
