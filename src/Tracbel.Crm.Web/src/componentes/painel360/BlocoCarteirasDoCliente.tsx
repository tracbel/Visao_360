/**
 * AS CARTEIRAS DO CLIENTE E O CEN DE CADA UMA no 360 — o bloco que dizia "Falta rota".
 *
 * 27/09/2026. A rota nova é `/api/v1/clientes/{chave}/carteiras`, no desenho das máquinas compradas: a ficha pede o
 * que é DELA pela chave do cliente, e `/api/v1/cobertura` continua sendo a lista da carteira inteira.
 *
 * O QUE O BLOCO MOSTRA, e de onde cada coisa vem:
 *
 * - **a classe** é a do cadastro do cliente (curva ABC do faturamento, por filial), e não a do vínculo: os 8.537
 *   vínculos ativos têm a classe C que a carga grava por padrão;
 * - **a cadência** é a da linha de negócio da carteira para essa classe — a mesma regra do cartão de cobertura;
 * - **o último contato** vem vazio hoje em todos os vínculos, e o traço leva o motivo que a API mediu: vazio é "sem
 *   registro no CRM", e não "nunca contatado".
 */

import { listarCarteirasDoCliente } from '../../dados/api/relacionamento';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarData } from '../../telas/cadastro/formato';
import type { CarteirasDoCliente } from '../../tipos/relacionamento';
import { MetricasSemDado } from '../cadastro/SemDado';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { ValorAusente } from '../comum/ValorAusente';
import { BlocoPainel, type EstadoBloco } from './BlocoPainel';
import { Dado } from './DadoDoPainel';
import '../../estilos/ficha-do-cliente.css';

/** O motivo do último contato vazio, quando a API não mandou um. */
const SEM_REGISTRO_DE_CONTATO = 'Nenhum contato registrado no CRM para este vínculo.';

function estadoDe(carregando: boolean, erro: Error | null, dados: CarteirasDoCliente | null): EstadoBloco {
  if (carregando) return 'carregando';
  if (erro) return 'erro';
  return dados ? 'ok' : 'vazio';
}

export function BlocoCarteirasDoCliente({ chave }: { chave: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => listarCarteirasDoCliente(contexto, chave, sinal), [contexto.empresa, chave]);
  const dados = leitura.dados;

  return (
    <BlocoPainel
      id="carteiras"
      titulo="Carteiras e CEN"
      subtitulo="comercial.ClienteCarteira — as carteiras ao seu alcance, com o CEN responsável por cada uma"
      fonte={<SeloProcedencia procedencia={leitura.procedencia} />}
      estado={estadoDe(leitura.carregando, leitura.erro, dados)}
      mensagemErro={leitura.erro?.message}
    >
      {dados && <Conteudo dados={dados} />}
    </BlocoPainel>
  );
}

function Conteudo({ dados }: { dados: CarteirasDoCliente }) {
  const motivoDoContato = dados.metricasSemDado.find((m) => m.metrica === 'ultimoContato')?.motivo ?? SEM_REGISTRO_DE_CONTATO;
  const motivoDaClasse =
    dados.metricasSemDado.find((m) => m.metrica === 'classe')?.motivo ?? 'A classe deste cliente ainda não foi apurada.';
  const motivoSemCarteira =
    dados.metricasSemDado.find((m) => m.metrica === 'carteiras')?.motivo ?? 'Nenhuma carteira ao seu alcance.';

  return (
    <div className="ficha-carteiras">
      <dl className="p360-dados">
        <Dado
          rotulo="Classe (curva ABC)"
          valor={dados.classe ?? <ValorAusente motivo={motivoDaClasse} oQue="a classe" />}
          detalhe={dados.classeApuradaEm ? `apurada do faturamento em ${formatarData(dados.classeApuradaEm)}` : undefined}
        />
        <Dado
          rotulo="Carteiras"
          valor={dados.carteiras.length > 0 ? dados.carteiras.length : <ValorAusente motivo={motivoSemCarteira} oQue="a carteira" />}
        />
      </dl>

      {dados.carteiras.length > 0 && (
        <ul className="p360-lista">
          {dados.carteiras.map((c) => (
            <li key={c.carteiraChave} className={c.naturezaDaCarteira === 'Comercial' ? 'p360-item' : 'p360-item p360-item-info'}>
              <div className="p360-item-topo">
                <span className="p360-item-titulo">{c.carteiraNome}</span>
                <span className="p360-item-data cad-mono">{c.carteiraCodigo}</span>
                {c.naturezaDaCarteira !== 'Comercial' && (
                  <span className="cad-selo">{c.naturezaDaCarteira === 'Administrativa' ? 'carteira administrativa' : 'carteira de teste'}</span>
                )}
              </div>
              <div className="p360-item-meta">
                CEN: {c.responsavelNome ?? 'sem responsável'}
                {c.naturezaDoResponsavel === 'Departamento' ? ' (área, e não pessoa)' : ''} · filial{' '}
                <span className="cad-mono">{c.filialCodigo}</span> {c.filialNome} · {c.linhaDeNegocioNome}
              </div>
              <div className="p360-item-obs ficha-carteira-rodape">
                <span>
                  {c.diasDeCadencia
                    ? `cadência de ${c.diasDeCadencia} dias para a classe ${dados.classe ?? 'D'}`
                    : 'a linha de negócio não declara cadência'}
                </span>
                <span>vinculado em {formatarData(c.vinculadoEm)}</span>
                <span className="ficha-contato">
                  último contato{' '}
                  {c.ultimaInteracaoEm ? (
                    <>
                      {formatarData(c.ultimaInteracaoEm)}
                      {c.estaForaDaCadencia ? ' · fora da cadência' : ''}
                    </>
                  ) : (
                    <ValorAusente motivo={motivoDoContato} oQue="o último contato" />
                  )}
                </span>
              </div>
            </li>
          ))}
        </ul>
      )}

      <MetricasSemDado
        metricas={dados.metricasSemDado.filter((m) => m.metrica !== 'ultimoContato')}
        titulo="O que este bloco não diz"
      />
    </div>
  );
}
