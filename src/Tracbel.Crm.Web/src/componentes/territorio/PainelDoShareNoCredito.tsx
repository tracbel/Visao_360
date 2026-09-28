/**
 * O SHARE DA TRACBEL NO CRÉDITO DE MECANIZAÇÃO (issue 262, decisões do Ricardo em
 * 28/09/2026) — embaixo do crédito rural, na aba Crédito do Momento.
 *
 * O NUMERADOR É O QUE A TRACBEL FINANCIOU: o valor dos formulários da venda do
 * Vórtice, um por processo, sem recurso próprio e sem consórcio. O DENOMINADOR É
 * O SICOR: trator, máquinas e implementos e colheitadeiras, na mesma janela do
 * painel de cima.
 *
 * É ESTIMATIVA, E A TELA DIZ ISSO. O formulário tem a data do pedido e o
 * município do cadastro do cliente; o SICOR, a cédula e o município do
 * empreendimento, e não identifica revenda. Por isso o número principal é por
 * filial e na Região, e o município é indicativo: o que passa de 100% vem
 * marcado, em vez de escondido. O protótipo casava contrato a contrato pelo
 * valor exato, e só ~9% casavam — o share saía muito menor do que é.
 */

import { Building2, HandCoins, Landmark, Percent } from 'lucide-react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterShareNoCredito } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { ShareNoRecorte } from '../../tipos/mercado';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { ValorAusente } from '../comum/ValorAusente';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { InfoTooltip } from '../InfoTooltip';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { mesCurto, numero, reaisCurtos } from '../mercado/momento/formatos';
import { CartaoDoMomento, FileiraDeCartoes, GraficoSemSerie, Legenda, LinhaDePaineis, PainelDoMomento } from '../mercado/momento/pecas';

const COMO_LER =
  'Estimativa. O numerador é o crédito rural que a Tracbel financiou nas vendas (formulários do Vórtice, sem recurso ' +
  'próprio e sem consórcio), pela data do pedido e no município do cadastro do cliente. O denominador é o crédito de ' +
  'máquinas do SICOR, pela emissão da cédula e no município do empreendimento. O SICOR não identifica revenda.';

const SEM_SICOR = 'O SICOR não tem crédito de máquinas neste recorte na janela: sem denominador não há share.';

/** O share em percentual, com uma casa. */
function percentual(share: number | null): string | null {
  return share === null ? null : `${numero(share * 100, 1)}%`;
}

/** A célula do share: o percentual, ou o traço com o motivo. */
function CelulaDoShare({ s, oQue }: { s: ShareNoRecorte; oQue: string }) {
  const texto = percentual(s.share);
  return texto === null ? <ValorAusente motivo={SEM_SICOR} oQue={oQue} /> : <>{texto}</>;
}

export function PainelDoShareNoCredito() {
  const { contexto } = useContextoDeAcesso();
  const recurso = useRecurso((sinal) => obterShareNoCredito(contexto, sinal), [contexto.empresa, contexto.usuario]);

  if (recurso.carregando) return <BlocoCarregando oQue="o share no crédito" />;
  if (recurso.erro) return <BlocoErro erro={recurso.erro} aoTentarDeNovo={recurso.recarregar} />;

  const dados = recurso.dados;
  if (!dados || !dados.inicio || !dados.fim || !dados.regiao)
    return (
      <BlocoVazio
        titulo="O share no crédito ainda não tem o SICOR para comparar"
        texto="Sem linha do SICOR gravada neste banco não há janela nem denominador. Não é zero nem falta de permissão."
      />
    );

  if (!dados.ultimoPedido)
    return (
      <BlocoVazio
        titulo="O financiamento das vendas ainda não foi carregado neste banco"
        texto={
          <>
            A rota respondeu, mas não há financiamento gravado: o terceiro modo da rotina do Vórtice (
            <code>--somente-financiamentos-vortice</code>) ainda não rodou aqui. Não é share zero.
          </>
        }
      />
    );

  const regiao = dados.regiao;
  const janela = `${mesCurto(dados.inicio)} a ${mesCurto(dados.fim)}`;
  const fora = dados.foraDaRegiao;
  const acima = dados.porMunicipio.filter((m) => m.acimaDoSicor).length;
  const temMensal = dados.porMes.length > 1;

  return (
    <div className="mom-painel-da-aba" data-bloco="share-credito">
      <FileiraDeCartoes>
        <CartaoDoMomento
          icone={Percent}
          tom="verde"
          rotulo="Share Tracbel no crédito"
          oQue="o share da Tracbel no crédito de mecanização"
          dica={<p>{COMO_LER}</p>}
          procedencia={dados.procedencia}
          valor={percentual(regiao.share)}
          motivoSemDado={SEM_SICOR}
          apoio={`Região Tracbel · ${janela}`}
        />
        <CartaoDoMomento
          icone={HandCoins}
          tom="azul"
          rotulo="Financiado pela Tracbel"
          oQue="o crédito rural financiado pela Tracbel"
          dica="O valor financiado nas vendas da Região Tracbel, em crédito rural: Moderfrota, Pronaf, Pronamp, FINAME, Pró-Trator, TFBD. Recurso próprio e consórcio ficam de fora."
          valor={reaisCurtos(regiao.valorTracbel)}
          apoio={`${numero(regiao.financiamentos)} financiamentos`}
        />
        <CartaoDoMomento
          icone={Landmark}
          tom="roxo"
          rotulo="Crédito de máquinas no SICOR"
          oQue="o crédito de máquinas do SICOR na Região"
          dica="Trator, máquinas e implementos e colheitadeiras, na Região Tracbel e na mesma janela do painel de cima."
          valor={reaisCurtos(regiao.valorSicor)}
          apoio={`${numero(regiao.linhasSicor)} linhas do SICOR`}
        />
        <CartaoDoMomento
          icone={Building2}
          tom="neutro"
          rotulo="Concorrência e financiado por fora"
          oQue="o crédito do SICOR não financiado pela Tracbel"
          dica="O que o SICOR registrou na Região e não aparece nos formulários da Tracbel: outras revendas, e a máquina Tracbel financiada sem passar pelo formulário."
          valor={reaisCurtos(regiao.valorDaConcorrencia)}
          apoio="SICOR menos Tracbel"
        />
      </FileiraDeCartoes>

      <LinhaDePaineis variante="credito">
        <PainelDoMomento
          titulo="Tracbel × SICOR, mês a mês"
          dica={
            'O crédito rural financiado pela Tracbel (verde) contra o crédito de máquinas do SICOR (cinza), na Região ' +
            'Tracbel. Mês a mês o share oscila muito: o pedido de venda e a emissão da cédula não caem no mesmo mês. ' +
            'Leia a janela inteira.'
          }
          direita={
            <Legenda
              itens={[
                { nome: 'Tracbel', cor: '#367C2B', forma: 'linha' },
                { nome: 'SICOR', cor: '#9CA3AF', forma: 'linha' },
              ]}
            />
          }
        >
          {temMensal ? (
            <MolduraDeGrafico altura={210}>
              {(l, a) => (
                <GraficoLinhaMensal
                  rotulos={dados.porMes.map((m) => mesCurto(m.mes))}
                  valores={dados.porMes.map((m) => m.valorTracbel)}
                  anteriores={dados.porMes.map((m) => m.valorSicor)}
                  nomeDaSerie="Tracbel"
                  nomeDaAnterior="SICOR"
                  largura={l}
                  altura={a}
                  formatar={reaisCurtos}
                />
              )}
            </MolduraDeGrafico>
          ) : (
            <GraficoSemSerie
              altura={210}
              frase="Um mês só"
              motivo="A janela tem um mês: não há linha para desenhar."
              oQue="o mês a mês do share"
            />
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="O que a Tracbel financiou, por linha"
          dica="Na Região Tracbel e na janela. As linhas riscadas não entram no share (decisão de 28/09/2026): recurso próprio é pagamento, e consórcio não passa pelo SICOR."
        >
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela" data-bloco="share-por-linha">
              <thead>
                <tr>
                  <th scope="col">Linha</th>
                  <th scope="col" className="mom-num">Financiamentos</th>
                  <th scope="col" className="mom-num">Valor (R$)</th>
                </tr>
              </thead>
              <tbody>
                {dados.porLinha.map((l) => (
                  <tr key={`${l.linha}-${l.contaNoShare}`} data-conta={l.contaNoShare}>
                    <td>
                      {l.contaNoShare ? l.linha : <s>{l.linha}</s>}
                      {!l.contaNoShare && <span className="cad-so-leitor"> (não entra no share)</span>}
                    </td>
                    <td className="mom-num">{numero(l.financiamentos)}</td>
                    <td className="mom-num">{reaisCurtos(l.valor)}</td>
                  </tr>
                ))}
                {dados.porLinha.length === 0 && (
                  <tr className="mom-tabela-vazia">
                    <td colSpan={3}>Nenhum financiamento na Região Tracbel na janela.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Share por filial"
        dica="Cada filial com os municípios da área de atuação pelos quais ela responde, somados. É o número a ler: por filial e em 12 meses as diferenças de data e de município se diluem."
        subtitulo={
          fora.foraDaAdr + fora.semMunicipio > 0
            ? `Fora da Região: ${numero(fora.foraDaAdr)} financiamentos de clientes em municípios fora da área de atuação (${reaisCurtos(fora.valorForaDaAdr)}) e ${numero(fora.semMunicipio)} sem município casado — fora de SP ou cidade sem par (${reaisCurtos(fora.valorSemMunicipio)}).`
            : undefined
        }
      >
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela" data-bloco="share-por-filial">
            <thead>
              <tr>
                <th scope="col">Filial</th>
                <th scope="col" className="mom-num">Municípios</th>
                <th scope="col" className="mom-num">Tracbel (R$)</th>
                <th scope="col" className="mom-num">SICOR (R$)</th>
                <th scope="col" className="mom-num">Share</th>
              </tr>
            </thead>
            <tbody>
              {dados.porFilial.map((f) => (
                <tr key={f.empresaId} data-filial={f.empresaId}>
                  <td>{f.filial}</td>
                  <td className="mom-num">{numero(f.municipios)}</td>
                  <td className="mom-num">{reaisCurtos(f.share.valorTracbel)}</td>
                  <td className="mom-num">{reaisCurtos(f.share.valorSicor)}</td>
                  <td className="mom-num">
                    <CelulaDoShare s={f.share} oQue={`o share de ${f.filial}`} />
                  </td>
                </tr>
              ))}
              {dados.porFilial.length === 0 && (
                <tr className="mom-tabela-vazia">
                  <td colSpan={5}>Nenhuma filial com município responsável na área de atuação.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </PainelDoMomento>

      <PainelDoMomento
        titulo="Por município (indicativo)"
        dica="O município do cadastro do cliente não é, sempre, o do empreendimento que o SICOR registra. Onde a Tracbel passa do SICOR, o município vem marcado: o cliente mora aqui e financiou a máquina noutro lugar, ou o contrato caiu noutro mês."
        subtitulo={acima > 0 ? `${numero(acima)} municípios com a Tracbel acima do SICOR — marcados com ⚠.` : undefined}
      >
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela" data-bloco="share-por-municipio">
            <thead>
              <tr>
                <th scope="col">Município</th>
                <th scope="col">Filial</th>
                <th scope="col" className="mom-num">Tracbel (R$)</th>
                <th scope="col" className="mom-num">SICOR (R$)</th>
                <th scope="col" className="mom-num">
                  Share <InfoTooltip rotulo="Por que o share do município pode passar de 100%" texto={COMO_LER} />
                </th>
              </tr>
            </thead>
            <tbody>
              {dados.porMunicipio.map((m) => (
                <tr key={m.codigoIbge} data-municipio={m.codigoIbge} data-acima={m.acimaDoSicor}>
                  <td>
                    {m.nome}
                    {m.acimaDoSicor && (
                      <span title="A Tracbel financiou mais do que o SICOR registrou aqui">
                        {' '}
                        ⚠<span className="cad-so-leitor"> Tracbel acima do SICOR</span>
                      </span>
                    )}
                  </td>
                  <td>{m.filial ?? '—'}</td>
                  <td className="mom-num">{reaisCurtos(m.share.valorTracbel)}</td>
                  <td className="mom-num">{reaisCurtos(m.share.valorSicor)}</td>
                  <td className="mom-num">
                    <CelulaDoShare s={m.share} oQue={`o share de ${m.nome}`} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </PainelDoMomento>
    </div>
  );
}
