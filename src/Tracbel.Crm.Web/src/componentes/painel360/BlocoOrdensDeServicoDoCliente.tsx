/**
 * AS ORDENS DE SERVIÇO DO CLIENTE no 360 — o bloco que tira "Ordens de serviço" do cartão "O que a ficha deste cliente
 * não pode mostrar" (02/10/2026).
 *
 * Lê `/api/v1/clientes/{chave}/ordens-de-servico`, da tabela que a rotina 15 `POS_VENDA_PROTHEUS` mantém a partir das
 * views do BI no banco do Protheus. Os recortes são os do painel de pós-venda do BI: em aberto (aberta ou liberada, o
 * "empenhado"), há mais de 45 dias (a faixa vermelha), e os doze meses de peças e serviços.
 *
 * O QUE O BLOCO NÃO PODE ESCONDER: a data da carga (a rotina nasce desligada; sem a data, a lista pareceria de hoje), a
 * fronteira (só as OS das filiais ao alcance de quem lê) e o casamento (a OS entra pelo CPF/CNPJ do proprietário).
 */

import { Link } from 'react-router-dom';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { listarOrdensDeServicoDoCliente } from '../../dados/api/equipamentos';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarData, formatarDinheiro, formatarNumero } from '../../telas/cadastro/formato';
import type { OrdemDeServicoResumida, OrdensDeServicoResumidas } from '../../tipos/ordensDeServico';
import { MetricasSemDado } from '../cadastro/SemDado';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { ValorAusente } from '../comum/ValorAusente';
import { BlocoPainel, type EstadoBloco } from './BlocoPainel';
import { Dado } from './DadoDoPainel';
import { DIAS_DA_FAIXA_VERMELHA, rotuloDaSituacao } from './regrasDasOrdensDeServico';
import { formatarDiaEHora } from '../../dados/formatadores';

/** Quantas OS a lista mostra — o resto fica contado. */
const LINHAS = 6;

function estadoDe(carregando: boolean, erro: Error | null, dados: OrdensDeServicoResumidas | null): EstadoBloco {
  if (carregando) return 'carregando';
  if (erro) return 'erro';
  return dados ? 'ok' : 'vazio';
}

export function BlocoOrdensDeServicoDoCliente({ chave }: { chave: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => listarOrdensDeServicoDoCliente(contexto, chave, sinal), [contexto.empresa, chave]);
  const dados = leitura.dados;

  const subtitulo = dados?.carregadoEm
    ? `oficina do Protheus, pelo CPF/CNPJ do proprietário na OS · até a carga de ${formatarDiaEHora(dados.carregadoEm)}`
    : 'oficina do Protheus, pelo CPF/CNPJ do proprietário na OS';

  return (
    <BlocoPainel
      id="ordens-de-servico"
      titulo="Ordens de serviço"
      subtitulo={subtitulo}
      fonte={<SeloProcedencia procedencia={leitura.procedencia} />}
      estado={estadoDe(leitura.carregando, leitura.erro, dados)}
      mensagemErro={leitura.erro?.message}
    >
      {dados && <Conteudo dados={dados} />}
    </BlocoPainel>
  );
}

function Conteudo({ dados }: { dados: OrdensDeServicoResumidas }) {
  if (dados.totalDeOrdens === 0) {
    const motivo =
      dados.metricasSemDado[0]?.motivo ?? 'Nenhuma ordem de serviço deste cliente nas filiais ao seu alcance.';
    return (
      <div className="ficha-ordens-de-servico">
        <dl className="p360-dados">
          <Dado rotulo="Ordens de serviço" valor={<ValorAusente motivo={motivo} oQue="o histórico de ordens de serviço" />} />
        </dl>
        <MetricasSemDado metricas={dados.metricasSemDado} titulo="O que este bloco não diz" />
      </div>
    );
  }

  return (
    <div className="ficha-ordens-de-servico">
      <dl className="p360-dados">
        <Dado
          rotulo="Na oficina agora"
          valor={formatarNumero(dados.emAberto)}
          detalhe={
            dados.diasDaMaisAntigaEmAberto === null
              ? 'nenhuma aberta ou liberada'
              : `a mais antiga há ${formatarNumero(dados.diasDaMaisAntigaEmAberto)} dias`
          }
        />
        <Dado
          rotulo={`Há mais de ${DIAS_DA_FAIXA_VERMELHA} dias`}
          valor={formatarNumero(dados.emAbertoHaMaisDe45Dias)}
          detalhe="abertas ou liberadas"
        />
        <Dado rotulo="Peças e serviços em aberto" valor={formatarDinheiro(dados.valorEmAberto)} detalhe="o empenhado na oficina" />
        <Dado
          rotulo="Fechadas em 12 meses"
          valor={formatarNumero(dados.nosUltimos12Meses)}
          detalhe={`peças ${formatarDinheiro(dados.pecasNosUltimos12Meses)} · serviços ${formatarDinheiro(dados.servicosNosUltimos12Meses)}`}
        />
      </dl>

      <ul className="p360-lista">
        {dados.ordens.slice(0, LINHAS).map((item) => (
          <ItemDaOrdem key={item.ordem.chave} item={item} />
        ))}
      </ul>
      {dados.totalDeOrdens > LINHAS && (
        <p className="p360-item-obs">
          {formatarNumero(dados.totalDeOrdens)} ordens de serviço no total; as {LINHAS} acima são as abertas primeiro e depois as
          mais recentes.
        </p>
      )}

      <p className="p360-item-obs">
        Peças e serviços já sem desconto, na régua do painel de pós-venda do BI. A OS entra aqui quando o CPF/CNPJ do
        proprietário, na OS do Protheus, é o deste cliente; só aparecem as OS das filiais ao seu alcance.
      </p>

      <MetricasSemDado metricas={dados.metricasSemDado} titulo="O que este bloco não diz" />
    </div>
  );
}

function ItemDaOrdem({ item }: { item: OrdemDeServicoResumida }) {
  const { ordem, diasEmAberto } = item;
  const classe =
    diasEmAberto !== null && diasEmAberto > DIAS_DA_FAIXA_VERMELHA
      ? 'p360-item p360-item-critico'
      : diasEmAberto !== null
        ? 'p360-item p360-item-aviso'
        : 'p360-item';
  const maquina = ordem.modelo ?? ordem.chassi ?? 'sem chassi na OS';

  return (
    <li className={classe}>
      <div className="p360-item-topo">
        <span className="p360-item-titulo">
          OS <span className="cad-mono">{ordem.numero}</span> ·{' '}
          {ordem.equipamentoChave ? <Link to={`/equipamentos/${ordem.equipamentoChave}`}>{maquina}</Link> : maquina}
        </span>
        <span className="p360-item-data cad-mono">{formatarData(ordem.abertaEm)}</span>
      </div>
      <div className="p360-item-meta">
        {rotuloDaSituacao(ordem.situacao)}
        {diasEmAberto !== null && ` há ${formatarNumero(diasEmAberto)} ${diasEmAberto === 1 ? 'dia' : 'dias'}`}
        {ordem.fechadaEm && ` · fechada em ${formatarData(ordem.fechadaEm)}`} · {ordem.filialNome}
        {ordem.tipoDeAtendimento ? ` · ${ordem.tipoDeAtendimento.toLowerCase()}` : ''}
      </div>
      <div className="p360-item-obs">
        peças {formatarDinheiro(ordem.valorDePecas)} · serviços {formatarDinheiro(ordem.valorDeServicos)}
        {ordem.horimetro !== null && ` · horímetro ${formatarNumero(ordem.horimetro, 1)} h`}
      </div>
    </li>
  );
}
