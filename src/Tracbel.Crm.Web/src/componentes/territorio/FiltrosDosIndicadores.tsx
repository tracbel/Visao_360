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
 *
 * DOIS FILTROS PASSARAM A FILTRAR (27/09/2026):
 *   - TIPO DE PRODUTO é a categoria de máquina, e a lista vem da leitura — do
 *     catálogo de categorias e do de-para da linha de produto —, e não de uma
 *     lista escrita aqui. Ele filtra as unidades do ART e, com elas, a captura
 *     e a oportunidade; os reais não mudam, porque o faturamento carregado não
 *     tem o item da nota;
 *   - CEN / GESTOR é o responsável da carteira comercial no CRM (issue 107: a
 *     carteira é a fonte). Ele filtra cobertura, reais e unidades pelos clientes
 *     das carteiras dele.
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
import { anoCivilFechado, anoFiscalFechado, dozeMesesFechados, mes, nomeDoAnoFiscal } from './indicadoresDaAdr';
import { CampoDoFiltro } from '../comum/CampoDoFiltro';

export function FiltrosDosIndicadores({
  filtros,
  aoMudarFiltros,
  lojasConhecidas,
  regras,
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
  /** As regras de potencial vigentes — uma por cultura desde a D-P01 (27/09/2026). */
  regras: RegraDePotencialAplicada[];
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
  const dozeMeses = dozeMesesFechados();
  const igual = (p: typeof anoCivil) =>
    filtros.competenciaInicial === p.competenciaInicial && filtros.competenciaFinal === p.competenciaFinal;
  // O FILTRO VAZIO É O ANO FISCAL (decisão do Ricardo de 27/09/2026): é o padrão
  // do servidor, que aplica o ano fiscal até o último mês fechado quando o pedido
  // não traz período. Os doze meses deixaram de ser o vazio e passaram a dizer os
  // meses por extenso.
  //
  // A ORDEM DA CONFERÊNCIA DECIDE NOS DOIS MESES EM QUE DOIS RECORTES COINCIDEM:
  // em novembro, o ano fiscal que acabou de fechar É os doze meses fechados, e em
  // janeiro o ano civil fechado é os doze meses. Nos dois casos o nome que fica é o
  // do preset mais específico — o ano fiscal, e depois os doze meses.
  const presetDoPeriodo =
    (filtros.competenciaInicial === '' && filtros.competenciaFinal === '') || igual(anoFiscal)
      ? 'anoFiscal'
      : igual(dozeMeses)
        ? '12meses'
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
        filtros.categoriaDeMaquina !== '',
        filtros.responsavel !== '',
      ].filter(Boolean).length,
    [filtros.visao, filtros.filialDaVenda, filtros.filialDoCliente, filtros.categoriaDeMaquina, filtros.responsavel, presetDoPeriodo],
  );

  // AS CATEGORIAS VÊM DA LEITURA, na ordem do catálogo. A escolhida fica na lista
  // mesmo antes da resposta, para o campo não mostrar "Todas" com um filtro ativo.
  const categorias = useMemo(() => [...(indicadores?.categoriasDeMaquina ?? [])].sort((a, b) => a.ordem - b.ordem), [indicadores]);
  const responsaveis = useMemo(
    () => [...(indicadores?.responsaveisDasCarteiras ?? [])].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR')),
    [indicadores],
  );
  const semGestor = responsaveis.filter((r) => r.gestor === null).length;

  // AS CULTURAS COM REGRA, uma vez cada — uma cultura pode ter regra em mais de
  // uma categoria de máquina — e em ordem de nome, que é a ordem de quem lê.
  const culturasDasRegras = useMemo(
    () => [...new Set(regras.map((r) => r.produtoNome))].sort((a, b) => a.localeCompare(b, 'pt-BR')),
    [regras],
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
        <CampoDoFiltro
          icone={CalendarDays}
          rotulo={
            <>
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
                    {/* O PADRÃO FOI DECIDIDO EM 27/09/2026 — e a dica diz qual é, e contra
                        o que ele se compara, porque é isso que o "vs. ano anterior" dos
                        cartões está medindo. */}
                    <p>
                      O período padrão é o <strong>ano fiscal até o último mês fechado</strong>, comparado com o{' '}
                      <strong>mesmo trecho do ano fiscal anterior</strong> — nov/2025 a ago/2026 contra nov/2024 a
                      ago/2025, por exemplo. Os doze meses fechados e o ano civil continuam aqui, como escolha.
                    </p>
                    <p>
                      O mês em curso fica fora dos três recortes, porque comparar um mês pela metade com meses
                      cheios erra para baixo sem aviso.
                    </p>
                  </>
                }
              />
            </>
          }
        >
          <select
            value={presetDoPeriodo}
            onChange={(e) => {
              // O ANO FISCAL É O FILTRO VAZIO: quem decide o intervalo é o servidor,
              // pela mesma regra do domínio — o que a tela mostra é o que ele aplicou.
              if (e.target.value === 'anoFiscal') aoMudarFiltros((f) => ({ ...f, competenciaInicial: '', competenciaFinal: '' }));
              else if (e.target.value === '12meses') aoMudarFiltros((f) => ({ ...f, ...dozeMeses }));
              else if (e.target.value === 'anoCivil') aoMudarFiltros((f) => ({ ...f, ...anoCivil }));
            }}
          >
            {/* O ANO FISCAL É O PRIMEIRO E O PADRÃO (27/09/2026): é o calendário em
                que a Tracbel fecha o ano. O rótulo em vigor é "FY2026 (nov/2025 a
                ago/2026)" — o nome sempre ao lado do intervalo, e sem "Ano fiscal"
                na frente, porque com as duas coisas o texto não cabe no campo e o
                ano saía cortado. O civil fica, porque é o calendário de toda fonte
                pública com que a tela compara — IBGE, CONAB, SICOR. */}
            <option value="anoFiscal">
              {presetDoPeriodo === 'anoFiscal' && intervalo
                ? `${nomeDoAnoFiscal(indicadores?.competenciaFinal ?? '') ?? 'Ano fiscal'} (${intervalo})`
                : 'Ano fiscal'}
            </option>
            <option value="12meses">12 meses{presetDoPeriodo === '12meses' && intervalo ? ` (${intervalo})` : ''}</option>
            <option value="anoCivil">Ano civil{presetDoPeriodo === 'anoCivil' && intervalo ? ` (${intervalo})` : ''}</option>
            {/* A opção personalizada só existe quando ela está em vigor: quem a
                escolhe é o par de campos de mês em "Mais filtros", e uma opção
                que não se pode escolher daqui não fica na lista prometendo. */}
            {presetDoPeriodo === 'personalizado' && (
              <option value="personalizado">Personalizado{intervalo ? ` (${intervalo})` : ''}</option>
            )}
          </select>
        </CampoDoFiltro>

        {/* SUB-REGIÃO, E NÃO "REGIÃO" (issue 163): Norte e Noroeste são partes da
            Região Tracbel, que é a ADR inteira. Chamar isto de "região" fazia
            "4,2% da região" ser lido como fatia da ADR quando era fatia do Norte. */}
        <CampoDoFiltro
          icone={User}
          rotulo={
            <>
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
            </>
          }
        >
          <select
            value={filtros.regiao}
            onChange={(e) => aoMudarFiltros((f) => ({ ...f, regiao: e.target.value as FiltrosTerritoriais['regiao'] }))}
          >
            <option value="">Região Tracbel inteira</option>
            <option value="Norte">Norte</option>
            <option value="Noroeste">Noroeste</option>
          </select>
        </CampoDoFiltro>

        <CampoDoFiltro icone={Store} rotulo="Loja">
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
        </CampoDoFiltro>

        {/* O MUNICÍPIO É FILTRO DE RECORTE e vale para as duas abas: o lugar dele
            é aqui, junto dos outros, e não flutuando entre blocos.

            ELE ERA UM CHIP COM ×, e virou o campo de escolha da maquete. O
            teclado ganhou com isso: antes só se escolhia município pelo mapa
            (ponteiro) ou pela tabela de Território; agora o campo alcança os
            duzentos pelo Tab e pelas setas, de qualquer aba. */}
        <CampoDoFiltro icone={MapPin} rotulo="Município">
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
        </CampoDoFiltro>

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

                {/* O ÚLTIMO MÊS ACEITO É O ÚLTIMO FECHADO (revisão de 27/09/2026): um período que termina no
                    mês em curso compara um mês pela metade com o mesmo mês inteiro do ano anterior. O servidor
                    também recusa a comparação nesse caso — o limite aqui só evita pedir o que não se compara. */}
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">Vendas de</span>
                  <input
                    type="month"
                    max={dozeMeses.competenciaFinal}
                    value={filtros.competenciaInicial}
                    onChange={(e) => aoMudarFiltros((f) => ({ ...f, competenciaInicial: e.target.value }))}
                  />
                </label>
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">até</span>
                  <input
                    type="month"
                    max={dozeMeses.competenciaFinal}
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

                {/* A LISTA SÃO TODAS AS CULTURAS COM REGRA (D-P01, 27/09/2026), e não a
                    primeira: com uma regra por cultura, a "primeira" era o amendoim, e a
                    dica ainda dizia "só há uma regra informada". O campo nunca filtrou —
                    o potencial soma as culturas —, e agora diz isso em vez de parecer
                    uma escolha feita. */}
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    Cultura da regra
                    <InfoTooltip
                      rotulo="O que esta lista mostra"
                      texto={
                        culturasDasRegras.length === 0
                          ? 'Não há regra de potencial vigente (D-P01, issue 63).'
                          : culturasDasRegras.length === 1
                            ? `O potencial do mapa vem da única cultura com regra de potencial: ${culturasDasRegras[0]}.`
                            : `O potencial do mapa soma as ${culturasDasRegras.length} culturas com regra de potencial: ${culturasDasRegras.join(', ')}. Ver uma delas sozinha ainda não existe.`
                      }
                    />
                  </span>
                  <select disabled>
                    <option>{culturasDasRegras.length === 0 ? '—' : culturasDasRegras.join(' · ')}</option>
                  </select>
                </label>

                {/* As citações "(documento 32, seção 3.4)" e "(documento 32, seção 3.5)" saíram
                    das dicas: a referência continua aqui, no código. */}
                <FiltroSemDado rotulo="Tipo de cliente" opcoes="SAM · KAM · Varejo" motivo="não há classificação por cliente em nenhuma fonte carregada" />
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    Tipo de produto
                    <InfoTooltip
                      rotulo="O que o tipo de produto filtra"
                      texto={
                        <>
                          <p>
                            A categoria da máquina, pela linha de produto do ART e pelo de-para do catálogo. Ela filtra
                            as <strong>máquinas vendidas em unidades</strong> e, com elas, a captura e a oportunidade,
                            que passam a usar só a demanda desta categoria.
                          </p>
                          <p>
                            Os valores em reais não mudam: o faturamento carregado é por cliente e mês, sem o item da
                            nota, e não separa uma colhedora de um trator. Venda de linha sem categoria no de-para sai
                            da conta enquanto o filtro está ligado.
                          </p>
                          {categorias.length === 0 && respondeu && <p>O catálogo de categorias não voltou nesta leitura.</p>}
                        </>
                      }
                    />
                  </span>
                  <select
                    value={filtros.categoriaDeMaquina}
                    disabled={categorias.length === 0 && filtros.categoriaDeMaquina === ''}
                    onChange={(e) => aoMudarFiltros((f) => ({ ...f, categoriaDeMaquina: e.target.value }))}
                  >
                    <option value="">Todas as categorias</option>
                    {filtros.categoriaDeMaquina !== '' && !categorias.some((c) => c.codigo === filtros.categoriaDeMaquina) && (
                      <option value={filtros.categoriaDeMaquina}>{filtros.categoriaDeMaquina}</option>
                    )}
                    {categorias.map((c) => (
                      <option key={c.codigo} value={c.codigo}>
                        {c.nome}
                      </option>
                    ))}
                  </select>
                </label>
                <FiltroSemDado
                  rotulo="Modelo"
                  opcoes="modelo da máquina"
                  motivo="o faturamento em reais é por cliente e mês, sem o item da nota. As unidades do ART trazem o modelo, mas o recorte por modelo não foi construído — o recorte por categoria está em Tipo de produto"
                />
                <label className="dash-filtro">
                  <span className="dash-filtro-rotulo">
                    CEN / gestor
                    <InfoTooltip
                      rotulo="O que o CEN / gestor filtra"
                      texto={
                        <>
                          <p>
                            O CEN é o <strong>responsável da carteira comercial</strong> no CRM — a carteira é a fonte, e
                            não a planilha (issue 107). O filtro fica com os clientes das carteiras dele: cobertura,
                            vendas em reais e máquinas em unidades. O potencial e a lavoura não mudam, porque são do
                            município inteiro.
                          </p>
                          {responsaveis.length > 0 && (
                            <p>
                              {responsaveis.length} responsáveis com carteira comercial ao seu alcance
                              {semGestor > 0
                                ? `; ${semGestor === responsaveis.length ? 'nenhum tem' : `${semGestor} não têm`} gestor cadastrado no CRM, e o recorte por gestor espera esse cadastro`
                                : ''}
                              .
                            </p>
                          )}
                        </>
                      }
                    />
                  </span>
                  <select
                    value={filtros.responsavel}
                    disabled={responsaveis.length === 0 && filtros.responsavel === ''}
                    onChange={(e) => aoMudarFiltros((f) => ({ ...f, responsavel: e.target.value }))}
                  >
                    <option value="">Todos os CENs</option>
                    {filtros.responsavel !== '' && !responsaveis.some((r) => String(r.id) === filtros.responsavel) && (
                      <option value={filtros.responsavel}>Responsável {filtros.responsavel}</option>
                    )}
                    {responsaveis.map((r) => (
                      <option key={r.id} value={String(r.id)}>
                        {r.nome}
                        {r.gestor ? ` — gestor ${r.gestor}` : ''}
                      </option>
                    ))}
                  </select>
                </label>

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
