import { useState } from 'react';
import type { ClassificacaoDeIndicador } from '../../../tipos/territorio';
import { FAIXAS_VENDAS, faixaDe, reaisCompactos } from '../escalas';
import { ROTULO_DE_VENDAS, mes, nº, type RecorteDeVendas } from '../indicadoresDaAdr';
import { SeloDeClassificacao } from '../SeloDeClassificacao';
import type { TotaisDaAdr } from '../totaisDaAdr';
import { CartaoDeMapa, type LigacaoDoMapa } from './CartaoDeMapa';
import { foraDoRecorte } from './foraDoRecorte';
import type { EstadoNoMapa } from '../MapaDeMunicipios';

/** Faturamento líquido pelo endereço principal do cliente. */
export function MapaDeVendas({
  ligacao,
  totais,
  competenciaInicial,
  competenciaFinal,
  classificacao,
}: {
  ligacao: LigacaoDoMapa;
  totais: TotaisDaAdr;
  competenciaInicial: string;
  competenciaFinal: string;
  classificacao: ClassificacaoDeIndicador | null;
}) {
  const [recorteDeVendas, setRecorteDeVendas] = useState<RecorteDeVendas>('valorLiquido');

  function estadoDasVendas(codigo: number): EstadoNoMapa {
    const m = ligacao.porCodigo.get(codigo);
    const fora = foraDoRecorte(m);
    if (fora) return fora;
    const valor = m!.vendas[recorteDeVendas];
    if (m!.cobertura.clientes === 0 && valor === 0) return { tipo: 'semDado', detalhe: 'nenhum cliente com endereço aqui' };
    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_VENDAS, valor).cor,
      detalhe: `${ROTULO_DE_VENDAS[recorteDeVendas]}: ${reaisCompactos(valor)} · máquina ${reaisCompactos(m!.vendas.maquina)} · pós-venda ${reaisCompactos(m!.vendas.posVenda)} · ${nº(m!.vendas.clientesQueCompraram)} clientes compraram`,
    };
  }

  return (
    <CartaoDeMapa
      mapa="vendas"
      id="vendas"
      ligacao={ligacao}
      titulo="Vendas realizadas"
      selo={<SeloDeClassificacao classificacao={classificacao} />}
      // Fonte, período e as três ressalvas que moravam no parágrafo do rodapé.
      metodologia={
        'Fonte: CRM Tracbel — faturamento, pelo endereço principal do cliente. ' +
        `Competência: ${mes(competenciaInicial)} a ${mes(competenciaFinal)}. ` +
        'Método: total = máquina + peça + serviço + outros; pós-venda = peça + serviço, composição provisória. ' +
        'Ressalvas: valor absoluto favorece cidades grandes; devolução e cancelamento não são abatidos; nota sem ' +
        'cliente no CRM não tem município e fica na tabela, fora do mapa.'
      }
      resumo={{
        valor: reaisCompactos(totais.vendas),
        rotulo: 'total no período',
        meta: [
          { valor: reaisCompactos(totais.maquina), rotulo: 'máquina' },
          { valor: reaisCompactos(totais.posVenda), rotulo: 'pós-venda' },
          { valor: nº(totais.clientesQueCompraram), rotulo: 'clientes compraram' },
        ],
      }
      }
      alternador={
        <div className="terr-alternador" role="group" aria-label="Recorte das vendas">
          {(Object.keys(ROTULO_DE_VENDAS) as RecorteDeVendas[]).map((recorte) => (
            <button key={recorte} type="button" aria-pressed={recorteDeVendas === recorte} onClick={() => setRecorteDeVendas(recorte)}>
              {ROTULO_DE_VENDAS[recorte]}
            </button>
          ))}
        </div>
      }
      tituloDoMapa="Mapa de vendas por município"
      estadoDe={estadoDasVendas}
      faixas={FAIXAS_VENDAS}
      unidade={`R$ no período — ${ROTULO_DE_VENDAS[recorteDeVendas].toLowerCase()}`}
    />
  );
}
