/**
 * Os filtros da tela de Indicadores Geográficos (issue 170; redesenhados na T4.6).
 *
 * O PROBLEMA QUE A T4.6 CORRIGE: eram treze filtros em duas fileiras, cada um com
 * o motivo escrito EMBAIXO, permanente — "sem dado: não há classificação por
 * cliente", "aplica-se só às vendas", "só há uma regra informada". Isso tomava a
 * primeira dobra inteira. Quem abria a tela para saber o tamanho do mercado lia
 * primeiro um parágrafo sobre o que a tela NÃO filtra.
 *
 * A ORDEM AGORA É A DA DECISÃO: período, sub-região, loja e o município escolhido
 * ficam sempre visíveis, numa linha; o resto vai para **Mais filtros**.
 *
 * NADA FOI REMOVIDO, e isso importa: os filtros sem dado continuam lá, desligados
 * e dizendo por quê — é o pedido do documento 32, e sumir com eles faria a tela
 * parecer completa. O que mudou é onde eles moram e onde o motivo é lido.
 *
 * O MOTIVO VIROU DICA (issue 33, nível 2). Ele continua inteiro, a um passo, em
 * vez de ocupar uma linha permanente embaixo de cada campo. O botão mostra
 * **quantos filtros secundários estão ativos**, para nada ficar escondido sem
 * aviso: um filtro que muda o número da tela não pode estar fora da vista sem o
 * leitor saber.
 *
 * POR QUE RADIX POPOVER: foco preso enquanto aberto, devolvido ao fechar, `Esc`,
 * clique fora, posicionamento com detecção de colisão e portal. É a mesma lista
 * de coisas que a dica precisava e que não vale reimplementar.
 *
 * ============================================================================
 * O DESENHO DA MAQUETE (fidelidade às maquetes, 23/09/2026).
 *
 * Cada filtro é um SELO DE ÍCONE e, ao lado, o rótulo pequeno em cima de um
 * CAMPO COM A PRÓPRIA MOLDURA. Antes a moldura era da caixa inteira e o campo
 * não tinha borda — o inverso da maquete, e o campo não parecia clicável.
 *
 * O MUNICÍPIO VIROU UM CAMPO DE ESCOLHA DE VERDADE: "Todos os municípios" e os
 * da ADR em ordem alfabética. Escolher aqui faz exatamente o que o clique no
 * mapa e na tabela fazem — escreve `?municipio=` na URL, que é a fonte única da
 * escolha (issue 163). Voltar para "Todos os municípios" é o que o × do chip
 * fazia: tira o recorte e apaga o parâmetro.
 *
 * DUAS LINHAS SAÍRAM DO CORPO E FORAM PARA AS DICAS, sem perder uma palavra: a
 * procedência do recorte ("Vendas de … a … · cobertura medida em …") está na
 * dica do Período, e o alcance da consulta ("Visão: filial … e as abaixo
 * dela"), na da Sub-região. A maquete não tem nenhuma das duas.
 * ============================================================================
 */

import * as Popover from '@radix-ui/react-popover';
import { CalendarDays, Funnel, MapPin, Store, User } from 'lucide-react';
import { useMemo, type Dispatch, type ReactNode, type SetStateAction } from 'react';
import { InfoTooltip } from '../InfoTooltip';
import type {
  FiltrosTerritoriais,
  IndicadoresDoMunicipio,
  IndicadoresTerritoriais,
  RegraDePotencialAplicada,
} from '../../tipos/territorio';
import { AlcanceDaConsulta } from './CartaoDeAlcance';
import { anoCivilFechado, anoFiscalFechado, mes, nomeDoAnoFiscal } from './indicadoresDaAdr';

export function FiltrosDosIndicadores({
  filtros,
  aoMudarFiltros,
  lojasConhecidas,
  regra,
  indicadores,
  respondeu,
  podeVerEmpresaInteira,
  empresa,
  municipios,
  municipioEscolhido,
  aoEscolherMunicipio,
}: {
  filtros: FiltrosTerritoriais;
  aoMudarFiltros: Dispatch<SetStateAction<FiltrosTerritoriais>>;
  lojasConhecidas: Map<string, string>;
  regra: RegraDePotencialAplicada | null;
  indicadores: IndicadoresTerritoriais | null;
  /** Se a leitura já respondeu — antes disso a tela não afirma nada sobre permissão. */
  respondeu: boolean;
  podeVerEmpresaInteira: boolean;
  /** A filial do cabeçalho — o alcance da consulta a nomeia. */
  empresa: string;
  /** Os municípios da ADR no recorte, que são as opções do campo de município. */
  municipios: readonly IndicadoresDoMunicipio[];
  /** O município escolhido — ele é filtro de recorte, e mora na linha com os outros. */
  municipioEscolhido: IndicadoresDoMunicipio | null;
  /** Escolhe (ou, com `null`, tira) o município — a casca escreve isso na URL. */
  aoEscolherMunicipio: (codigo: number | null) => void;
}) {
  const anoCivil = anoCivilFechado();
  const anoFiscal = anoFiscalFechado();
  const igual = (p: typeof anoCivil) =>
    filtros.competenciaInicial === p.competenciaInicial && filtros.competenciaFinal === p.competenciaFinal;
  // OS DOIS RECORTES NUNCA COINCIDEM: terminam no mesmo mês fechado, mas começam em
  // meses diferentes — janeiro e novembro —, então a competência inicial sempre os
  // separa e a ordem da conferência não decide nada.
  const presetDoPeriodo =
    filtros.competenciaInicial === '' && filtros.competenciaFinal === ''
      ? '12meses'
      : igual(anoFiscal)
        ? 'anoFiscal'
        : igual(anoCivil)
          ? 'anoCivil'
          : 'personalizado';

  // O INTERVALO EM VIGOR, escrito no próprio campo (maquete). Ele vem da
  // resposta, e não de uma conta local: é a competência que o servidor aplicou.
  const intervalo = indicadores ? `${mes(indicadores.competenciaInicial)} a ${mes(indicadores.competenciaFinal)}` : null;

  // QUANTOS SECUNDÁRIOS ESTÃO ATIVOS. Um filtro que muda o número da tela não
  // pode estar fora da vista sem o leitor saber — o contador é o que impede isso.
  const secundariosAtivos = useMemo(
    () =>
      [
        filtros.visao !== 'Filial',
        filtros.filialDaVenda !== '',
        filtros.filialDoCliente !== '',
        presetDoPeriodo === 'personalizado',
      ].filter(Boolean).length,
    [filtros.visao, filtros.filialDaVenda, filtros.filialDoCliente, presetDoPeriodo],
  );

  // AS OPÇÕES DO MUNICÍPIO SÃO AS DA ADR, EM ORDEM ALFABÉTICA — é como se procura
  // um nome numa lista de duzentos. A ordem por venda é a da tabela, que
  // responde outra pergunta.
  const opcoesDeMunicipio = useMemo(
    () => [...municipios].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR')),
    [municipios],
  );

  // UM MUNICÍPIO DE FORA DA ADR TAMBÉM PODE ESTAR ESCOLHIDO: o mapa desenha os
  // vizinhos com cliente, e clicar num deles escolhe. Sem uma opção para ele, o
  // campo mostraria "Todos os municípios" com um recorte ativo — afirmando o
  // contrário do que a página está mostrando.
  const escolhidoForaDaLista =
    municipioEscolhido !== null && !municipios.some((m) => m.codigoIbge === municipioEscolhido.codigoIbge)
      ? municipioEscolhido
      : null;

  return (
    <div className="dash-filtros" data-bloco="filtros">
      <div className="dash-filtros-linha">
        {/* PERÍODO — o primeiro, porque todo número da tela é dele.

            ELE VIROU UM CAMPO DE ESCOLHA (fase T4.9 — maquete). Eram três botões
            num alternador, que numa caixa de filtro de 200px ficavam apertados e
            não cabia o intervalo escrito. A maquete mostra um campo só, com o
            intervalo no próprio rótulo: "12 meses (set/2025 a ago/2026)".

            O INTERVALO SÓ APARECE NA OPÇÃO EM VIGOR, e é por isso que ele não
            está escrito nas duas: o que a API devolve é a competência do recorte
            APLICADO. Escrever o intervalo do ano civil embaixo de "12 meses"
            exigiria calcular aqui de novo a janela que o servidor calcula — duas
            contas para a mesma data é como elas passam a discordar. */}
        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <CalendarDays size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">
              Período
              {/* A PROCEDÊNCIA DO RECORTE MORA NESTA DICA (fidelidade às
                  maquetes, 23/09/2026). Era uma linha de metadado embaixo dos
                  filtros — nível 4 da issue 33 —, e a maquete não a tem. O
                  assunto dela é o período em vigor, então é aqui que ela fica. */}
              <InfoTooltip
                rotulo="O período em vigor e o calendário fiscal"
                texto={
                  <>
                    {indicadores && (
                      <p>
                        Vendas de <strong>{mes(indicadores.competenciaInicial)}</strong> a{' '}
                        <strong>{mes(indicadores.competenciaFinal)}</strong>
                        {presetDoPeriodo === '12meses' && ' (12 meses fechados)'}
                        {presetDoPeriodo === 'anoCivil' && ' (ano civil até o último mês fechado)'}
                        {presetDoPeriodo === 'anoFiscal' &&
                          ` (${nomeDoAnoFiscal(indicadores.competenciaFinal) ?? 'ano fiscal'}, até o último mês fechado)`}{' '}
                        · cobertura
                        medida em {new Date(indicadores.referenciaDaCobertura).toLocaleDateString('pt-BR')}
                        {indicadores.anoDaAreaPlantada && ` · área plantada PAM/IBGE ${indicadores.anoDaAreaPlantada}`}.
                      </p>
                    )}
                    {/* A citação "(documento 32, P-4)" saiu da dica: o número do
                        documento não diz nada a quem lê, e a referência fica aqui. */}
                    {/* O CALENDÁRIO FISCAL FOI CONFIRMADO em 24/09/2026, e esta dica
                        dizia o contrário até então. O nome do ano fiscal nunca aparece
                        sozinho: "FY2026" sem o intervalo escrito é lido como ano civil
                        por quem não conhece o calendário, e erra por dois meses. */}
                    <p>
                      O <strong>ano fiscal da Tracbel vai de novembro a outubro</strong>, e leva o nome do ano em
                      que termina: o FY2026 é de nov/2025 a out/2026. É assim que a diretoria compara um ano com o
                      outro.
                    </p>
                    <p>
                      O mês em curso fica fora dos três recortes, porque comparar um mês pela metade com meses
                      cheios erra para baixo sem aviso.
                    </p>
                  </>
                }
              />
            </span>
            <select
              value={presetDoPeriodo}
              onChange={(e) => {
                if (e.target.value === '12meses') aoMudarFiltros((f) => ({ ...f, competenciaInicial: '', competenciaFinal: '' }));
                else if (e.target.value === 'anoCivil') aoMudarFiltros((f) => ({ ...f, ...anoCivil }));
                else if (e.target.value === 'anoFiscal') aoMudarFiltros((f) => ({ ...f, ...anoFiscal }));
              }}
            >
              <option value="12meses">12 meses{presetDoPeriodo === '12meses' && intervalo ? ` (${intervalo})` : ''}</option>
              {/* O ANO FISCAL VEM ANTES DO CIVIL (24/09/2026): é o calendário em que a
                  Tracbel fecha o ano, e a planilha do comercial conta assim. O civil
                  fica, porque é o calendário de toda fonte pública com que a tela
                  compara — IBGE, CONAB, SICOR. */}
              <option value="anoFiscal">
                Ano fiscal
                {presetDoPeriodo === 'anoFiscal' && intervalo
                  ? ` ${nomeDoAnoFiscal(indicadores?.competenciaFinal ?? '') ?? ''} (${intervalo})`.replace('  ', ' ')
                  : ''}
              </option>
              <option value="anoCivil">Ano civil{presetDoPeriodo === 'anoCivil' && intervalo ? ` (${intervalo})` : ''}</option>
              {/* A opção personalizada só existe quando ela está em vigor: quem a
                  escolhe é o par de campos de mês em "Mais filtros", e uma opção
                  que não se pode escolher daqui não fica na lista prometendo. */}
              {presetDoPeriodo === 'personalizado' && (
                <option value="personalizado">Personalizado{intervalo ? ` (${intervalo})` : ''}</option>
              )}
            </select>
          </span>
        </label>

        {/* SUB-REGIÃO, E NÃO "REGIÃO" (issue 163): Norte e Noroeste são partes da
            Região Tracbel, que é a ADR inteira. Chamar isto de "região" fazia
            "4,2% da região" ser lido como fatia da ADR quando era fatia do Norte. */}
        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <User size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">
              Sub-região
              {/* O ALCANCE DA CONSULTA MORA NESTA DICA (fidelidade às
                  maquetes): é recorte — filial ou empresa inteira —, e a
                  sub-região é o filtro que explica a hierarquia do recorte.
                  O porquê da escolha está em `CartaoDeAlcance.tsx`. */}
              <InfoTooltip
                rotulo="O que é a sub-região e o que esta consulta alcança"
                texto={
                  <>
                    <p>
                      A hierarquia é São Paulo → Região Tracbel → sub-região → loja → município. Norte e Noroeste são
                      SUB-REGIÕES; a Região Tracbel é a área de atuação inteira, e é ela o denominador das fatias
                      desta tela.
                    </p>
                    <AlcanceDaConsulta
                      visao={filtros.visao}
                      empresa={empresa}
                      podeVerEmpresaInteira={respondeu ? podeVerEmpresaInteira : undefined}
                    />
                  </>
                }
              />
            </span>
            <select
              value={filtros.regiao}
              onChange={(e) => aoMudarFiltros((f) => ({ ...f, regiao: e.target.value as FiltrosTerritoriais['regiao'] }))}
            >
              <option value="">Região Tracbel inteira</option>
              <option value="Norte">Norte</option>
              <option value="Noroeste">Noroeste</option>
            </select>
          </span>
        </label>

        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <Store size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Loja</span>
            <select value={filtros.lojaCodigo} onChange={(e) => aoMudarFiltros((f) => ({ ...f, lojaCodigo: e.target.value }))}>
              <option value="">Todas</option>
              {[...lojasConhecidas.entries()]
                .sort((a, b) => a[1].localeCompare(b[1]))
                .map(([codigo, nome]) => (
                  <option key={codigo} value={codigo}>
                    {nome}
                  </option>
                ))}
            </select>
          </span>
        </label>

        {/* O MUNICÍPIO É FILTRO DE RECORTE e vale para as duas abas: o lugar dele
            é aqui, junto dos outros, e não flutuando entre blocos.

            ELE ERA UM CHIP COM ×, e virou o campo de escolha da maquete. O
            teclado ganhou com isso: antes só se escolhia município pelo mapa
            (ponteiro) ou pela tabela de Território; agora o campo alcança os
            duzentos pelo Tab e pelas setas, de qualquer aba. */}
        <label className="dash-filtro">
          <span className="dash-filtro-icone" aria-hidden="true">
            <MapPin size={17} strokeWidth={2} />
          </span>
          <span className="dash-filtro-corpo">
            <span className="dash-filtro-rotulo">Município</span>
            <select
              value={municipioEscolhido ? String(municipioEscolhido.codigoIbge) : ''}
              onChange={(e) => aoEscolherMunicipio(e.target.value === '' ? null : Number(e.target.value))}
              data-bloco="municipio"
              // O RECORTE ATIVO APARECE NO CAMPO: um município escolhido muda a
              // página inteira, e o campo ganha a borda verde para isso não
              // passar por "Todos" num relance.
              data-ativo={municipioEscolhido ? 'true' : undefined}
            >
              <option value="">Todos os municípios</option>
              {escolhidoForaDaLista && (
                <option value={escolhidoForaDaLista.codigoIbge}>{escolhidoForaDaLista.nome} (fora da ADR)</option>
              )}
              {opcoesDeMunicipio.map((m) => (
                <option key={m.codigoIbge} value={m.codigoIbge}>
                  {m.nome}
                </option>
              ))}
            </select>
          </span>
        </label>

        {/* "MAIS FILTROS" LOGO DEPOIS DO MUNICÍPIO (maquete): ele não é o quinto
            filtro, é a porta para o resto deles — e na maquete ele fica colado
            ao último campo, depois de um filete, e não empurrado para a ponta. */}
        <div className="dash-filtros-acao">
          <Popover.Root>
            <Popover.Trigger asChild>
              <button type="button" className="dash-mais-filtros" data-bloco="mais-filtros">
                {/* O FUNIL É O ÍCONE DA MAQUETE. O `sliders` que estava aqui é o de
                    ajuste fino; funil é o de recorte, que é o que este botão faz. */}
                <Funnel size={15} strokeWidth={2} aria-hidden="true" />
                Mais filtros
                {secundariosAtivos > 0 && <span className="dash-mais-filtros-selo">{secundariosAtivos}</span>}
              </button>
            </Popover.Trigger>
            <Popover.Portal>
              <Popover.Content className="dash-popover" sideOffset={6} collisionPadding={16} align="end">
                <div className="dash-popover-titulo">Mais filtros</div>

                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">Vendas de</span>
                  <input
                    type="month"
                    value={filtros.competenciaInicial}
                    onChange={(e) => aoMudarFiltros((f) => ({ ...f, competenciaInicial: e.target.value }))}
                  />
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">até</span>
                  <input
                    type="month"
                    value={filtros.competenciaFinal}
                    onChange={(e) => aoMudarFiltros((f) => ({ ...f, competenciaFinal: e.target.value }))}
                  />
                </label>

                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    Visão
                    {/* A citação "(documento 32, P-10)" saiu da dica; a referência fica aqui. */}
                    {respondeu && !podeVerEmpresaInteira && (
                      <InfoTooltip
                        rotulo="Por que a empresa inteira está desligada"
                        texto="A visão da empresa inteira exige a permissão de alcance entre filiais em profundidade Organização, e o seu perfil não a tem."
                      />
                    )}
                  </span>
                  <select
                    value={filtros.visao}
                    onChange={(e) => aoMudarFiltros((f) => ({ ...f, visao: e.target.value as FiltrosTerritoriais['visao'] }))}
                  >
                    <option value="Filial">Filial do cabeçalho</option>
                    <option value="Empresa" disabled={respondeu ? !podeVerEmpresaInteira : false}>
                      Empresa inteira
                    </option>
                  </select>
                </label>

                <FiltroDeFilial
                  rotulo="Filial que vendeu"
                  aplicaA="só às vendas"
                  valor={filtros.filialDaVenda}
                  lojas={lojasConhecidas}
                  aoMudar={(valor) => aoMudarFiltros((f) => ({ ...f, filialDaVenda: valor }))}
                />
                <FiltroDeFilial
                  rotulo="Filial de cadastro do cliente"
                  aplicaA="à cobertura e às vendas"
                  valor={filtros.filialDoCliente}
                  lojas={lojasConhecidas}
                  aoMudar={(valor) => aoMudarFiltros((f) => ({ ...f, filialDoCliente: valor }))}
                />

                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    Cultura da regra
                    <InfoTooltip rotulo="Por que há uma cultura só" texto="Só há uma regra de potencial informada; a lista cresce quando o comercial confirmar as demais (D-P01, issue 63)." />
                  </span>
                  {/* `defaultValue`, e não `value`: o campo é desligado e não tem
                      `onChange`, e o React avisa no console a cada render que um
                      `value` sem `onChange` vira campo somente leitura. O aviso
                      era verdadeiro e barulhento. */}
                  <select defaultValue={regra?.produtoCodigoIbge ?? ''} disabled={!regra}>
                    {regra && <option value={regra.produtoCodigoIbge}>{regra.produtoNome}</option>}
                  </select>
                </label>

                {/* As citações "(documento 32, seção 3.4)" e "(documento 32, seção 3.5)" saíram
                    das dicas: a referência continua aqui, no código. */}
                <FiltroSemDado rotulo="Tipo de cliente" opcoes="SAM · KAM · Varejo" motivo="não há classificação por cliente em nenhuma fonte carregada" />
                <FiltroSemDado rotulo="Tipo de produto" opcoes="colhedora · trator grande · médio" motivo="o faturamento carregado é por cliente e mês, sem o item da nota" />
                <FiltroSemDado rotulo="Modelo" opcoes="modelo da máquina" motivo="o faturamento carregado é por cliente e mês, sem o item da nota" />
                <FiltroSemDado
                  rotulo="CEN / gestor"
                  opcoes="—"
                  motivo="a carteira do CRM ainda não diz quem atende cada município (issue 107); o que ela já tem está na ficha do município"
                />

                <Popover.Close className="dash-popover-fechar">Fechar</Popover.Close>
              </Popover.Content>
            </Popover.Portal>
          </Popover.Root>
        </div>
      </div>

      {/* A LINHA DA PROCEDÊNCIA DO RECORTE que morava aqui ("Vendas de … a … ·
          cobertura medida em … · área plantada PAM/IBGE …") foi para a dica do
          Período, inteira (fidelidade às maquetes, 23/09/2026). */}
    </div>
  );
}

/** Um filtro pedido que o dado não sustenta: aparece, desligado, com o motivo na dica. */
function FiltroSemDado({ rotulo, opcoes, motivo }: { rotulo: string; opcoes: string; motivo: string }) {
  return (
    <label className="dash-filtro">
      <span className="dash-filtro-rotulo">
        {rotulo}
        <InfoTooltip rotulo={`Por que ${rotulo.toLowerCase()} não filtra`} texto={`Sem dado: ${motivo}.`} />
      </span>
      <select disabled>
        <option>{opcoes}</option>
      </select>
    </label>
  );
}

/** Uma filial para filtrar — pelas lojas que a ADR já mostrou, com o que o filtro alcança. */
function FiltroDeFilial({
  rotulo,
  aplicaA,
  valor,
  lojas,
  aoMudar,
}: {
  rotulo: string;
  aplicaA: string;
  valor: string;
  lojas: Map<string, string>;
  aoMudar: (valor: string) => void;
}): ReactNode {
  return (
    <label className="dash-filtro">
      <span className="dash-filtro-rotulo">
        {rotulo}
        <InfoTooltip rotulo={`O que ${rotulo.toLowerCase()} alcança`} texto={`Aplica-se ${aplicaA}.`} />
      </span>
      <select value={valor} onChange={(e) => aoMudar(e.target.value)}>
        <option value="">Todas ao alcance</option>
        {[...lojas.entries()]
          .sort((a, b) => a[1].localeCompare(b[1]))
          .map(([codigo, nome]) => (
            <option key={codigo} value={codigo}>
              {nome}
            </option>
          ))}
      </select>
    </label>
  );
}
