/**
 * "ENTENDA OS INDICADORES" — o link do canto da seção "Onde agir?" na maquete de 02/10/2026.
 *
 * É ONDE MORA O QUE A MAQUETE NÃO DESENHA E A TELA NÃO PODE PERDER: como o IOC é feito, os pesos vigentes (e se ainda
 * são os do protótipo), o share-alvo de cada categoria, o caminho para ajustá-los e as limitações dos dados. Antes
 * ficavam numa caixa e numa dica dentro do painel da prioridade.
 *
 * É UM POPOVER, E NÃO A DICA (ⓘ): a dica não recebe clique, e o "Ajustar em Configurações" precisa ser um link.
 */

import * as Popover from '@radix-ui/react-popover';
import { Info } from 'lucide-react';
import { Link } from 'react-router-dom';
import { MetricasSemDado } from '../cadastro/SemDado';
import type { DiagnosticoComercialDaRegiao } from '../../tipos/mercado';
import { COMPONENTES, dataCurta, n } from './diagnostico';

export function EntendaOsIndicadores({ dados }: { dados: DiagnosticoComercialDaRegiao }) {
  return (
    <Popover.Root>
      <Popover.Trigger asChild>
        <button type="button" className="diag-entenda" data-bloco="entenda-os-indicadores">
          Entenda os indicadores
          <Info size={14} strokeWidth={2} aria-hidden="true" />
        </button>
      </Popover.Trigger>
      <Popover.Portal>
        <Popover.Content className="dash-popover diag-entenda-conteudo" sideOffset={6} collisionPadding={16} align="end">
          <div className="dash-popover-titulo">Como o IOC é feito</div>
          <p>
            O Índice de Oportunidade Comercial vai de 0 a 100 e ordena os municípios pelo que a Tracbel tem a ganhar agindo
            agora — potencial grande e pouco explorado, com crédito e preço a favor. Não é previsão de venda.
          </p>
          <ul className="diag-entenda-componentes">
            {COMPONENTES.map((c) => (
              <li key={c.chave}>
                <strong>
                  {c.rotulo}
                  {dados.pesos && ` (${n(dados.pesos[c.chave], 2)})`}
                </strong>{' '}
                — {c.mede}
              </li>
            ))}
          </ul>
          <p className="diag-entenda-pesos" data-bloco="pesos">
            <strong>Pesos do IOC</strong>{' '}
            {dados.pesos ? (dados.pesosVigentesDesde ? `vigentes desde ${dataCurta(dados.pesosVigentesDesde)}` : 'vigentes') : 'não registrados'}
            {dados.pesosDoPrototipo && <span className="diag-a-confirmar">protótipo, a confirmar</span>}
            {dados.shares.length > 0 && (
              <>
                <br />
                <strong>Share-alvo</strong> {dados.shares.map((s) => `${s.categoriaNome} ${n(s.percentual)}%`).join(' · ')}
              </>
            )}
          </p>
          <Link to="/config" className="diag-link">
            Ajustar em Configurações › Potencial de mercado
          </Link>
          {dados.lacunas.length > 0 && <MetricasSemDado metricas={dados.lacunas} titulo="O que este diagnóstico não afirma" naDica />}
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}
