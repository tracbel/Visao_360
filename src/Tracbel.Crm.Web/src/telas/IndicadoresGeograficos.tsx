/**
 * Indicadores Geográficos da ADR — os três mapas do pedido do gerente comercial
 * (documento 32, seção 8), com dado do banco do CRM e do IBGE.
 *
 * O QUE ESTA TELA NÃO FAZ, e diz na própria tela:
 * - não filtra por SAM/KAM/Varejo, tipo de produto nem modelo — não há dado;
 * - não mostra potencial de cliente nem de não cliente — não há área por
 *   propriedade; o potencial é o do município inteiro, por uma regra ainda a
 *   confirmar;
 * - não mostra valor em reais no potencial — não há preço confirmado.
 *
 * OS NÚMEROS DA MAQUETE NÃO ESTÃO AQUI. Ela era ilustrativa; nenhum valor desta
 * tela é simulado.
 */

import { useEffect, useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DetalheDoMunicipio } from '../componentes/territorio/DetalheDoMunicipio';
import {
  FAIXAS_PENDENCIA_PERCENTUAL,
  FAIXAS_PENDENCIA_QUANTIDADE,
  FAIXAS_POTENCIAL,
  FAIXAS_VENDAS,
  faixaDe,
  reaisCompactos,
} from '../componentes/territorio/escalas';
import {
  LegendaDoMapa,
  MapaDeMunicipios,
  type EstadoNoMapa,
  type PoligonoProjetado,
} from '../componentes/territorio/MapaDeMunicipios';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { carregarMalhaDeSaoPaulo, obterIndicadoresTerritoriais } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import type { ClassificacaoDeIndicador, FiltrosTerritoriais, IndicadoresDoMunicipio } from '../tipos/territorio';
import '../estilos/territorio.css';

/** A largura de referência do desenho; o SVG escala para o cartão. */
const LARGURA_DO_DESENHO = 520;

type ModoDePendencia = 'percentual' | 'quantidade';
type RecorteDeVendas = 'valorLiquido' | 'maquina' | 'posVenda';

const ROTULO_DE_VENDAS: Record<RecorteDeVendas, string> = {
  valorLiquido: 'Total líquido',
  maquina: 'Máquina',
  posVenda: 'Pós-venda',
};

const nº = (v: number) => v.toLocaleString('pt-BR');

/** `2025-09-01` vira `set/2025`. */
function mes(competencia: string): string {
  const [ano, m] = competencia.split('-');
  return `${['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'][Number(m) - 1]}/${ano}`;
}

export function IndicadoresGeograficos() {
  const { contexto } = useContextoDeAcesso();

  const [filtros, setFiltros] = useState<FiltrosTerritoriais>({
    competenciaInicial: '',
    competenciaFinal: '',
    regiao: '',
    lojaCodigo: '',
    visao: 'Filial',
    filialDaVenda: '',
    filialDoCliente: '',
  });
  const [modoDePendencia, setModoDePendencia] = useState<ModoDePendencia>('percentual');
  const [recorteDeVendas, setRecorteDeVendas] = useState<RecorteDeVendas>('valorLiquido');
  const [selecionado, setSelecionado] = useState<number | null>(null);
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());

  // AS LOJAS DO FILTRO SÃO AS QUE A ADR JÁ MOSTROU, acumuladas a cada resposta.
  // Tirá-las só da resposta corrente faria a lista encolher ao escolher uma
  // loja, e não daria para voltar.
  const painel = useRecurso(
    (sinal) =>
      obterIndicadoresTerritoriais(contexto, filtros, sinal).then((resposta) => {
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of resposta.dados.indicadores.municipios)
            if (m.lojaCodigo && m.lojaNome) proximo.set(m.lojaCodigo, m.lojaNome.replace(/^.*—\s*/, ''));
          return proximo.size === atual.size ? atual : proximo;
        });
        return resposta;
      }),
    [
      contexto.empresa,
      contexto.usuario,
      filtros.competenciaInicial,
      filtros.competenciaFinal,
      filtros.regiao,
      filtros.lojaCodigo,
      filtros.visao,
      filtros.filialDaVenda,
      filtros.filialDoCliente,
    ],
  );

  const [malha, setMalha] = useState<ColecaoMunicipal | null>(null);
  const [erroDaMalha, setErroDaMalha] = useState<Error | null>(null);

  useEffect(() => {
    const controlador = new AbortController();
    carregarMalhaDeSaoPaulo(controlador.signal)
      .then(setMalha)
      .catch((causa: unknown) => {
        if (!controlador.signal.aborted) setErroDaMalha(causa instanceof Error ? causa : new Error(String(causa)));
      });
    return () => controlador.abort();
  }, []);

  const indicadores = painel.dados?.indicadores ?? null;
  const municipios = useMemo(() => indicadores?.municipios ?? [], [indicadores]);
  const porCodigo = useMemo(() => new Map(municipios.map((m) => [m.codigoIbge, m])), [municipios]);
  const adr = useMemo(() => new Set(municipios.filter((m) => m.pertenceAAdr).map((m) => m.codigoIbge)), [municipios]);
  const regra = indicadores?.regras[0] ?? null;

  const desenho = useMemo(() => {
    if (!malha) return null;
    const enquadramento = enquadrar(malha, LARGURA_DO_DESENHO);
    const poligonos: PoligonoProjetado[] = malha.features.map((f) => ({
      codigo: Number(f.properties.codarea),
      nome: f.properties.nome,
      caminho: caminhoSvg(enquadramento, f.geometry),
    }));
    return { enquadramento, poligonos };
  }, [malha]);

  const daAdr = useMemo(() => municipios.filter((m) => m.pertenceAAdr), [municipios]);

  const totais = useMemo(() => {
    const soma = (f: (m: IndicadoresDoMunicipio) => number) => daAdr.reduce((s, m) => s + f(m), 0);
    // PRIMEIRO EXISTE, DEPOIS TEM VALOR: sem regra ativa a lista vem vazia, e sem área o valor vem nulo.
    const comArea = daAdr.filter((m) => m.potencial.length > 0 && m.potencial[0].maquinasTeoricas !== null);
    return {
      elegiveis: soma((m) => m.cobertura.vinculosComCadencia),
      pendentes: soma((m) => m.cobertura.pendentes),
      clientes: soma((m) => m.cobertura.clientes),
      vendas: soma((m) => m.vendas.valorLiquido),
      maquina: soma((m) => m.vendas.maquina),
      posVenda: soma((m) => m.vendas.posVenda),
      maquinasTeoricas: comArea.reduce((s, m) => s + (m.potencial[0].maquinasTeoricas ?? 0), 0),
      cenDiferente: daAdr.filter((m) => m.comparacaoDoCen === 'NomesDiferentes').length,
      cenGrafiaDiferente: daAdr.filter((m) => m.comparacaoDoCen === 'ProvavelMesmaPessoa').length,
    };
  }, [daAdr]);

  function estadoDaPendencia(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };
    if (m.cobertura.percentualPendente === null)
      return { tipo: 'semDado', detalhe: 'sem vínculo elegível para medir' };
    const valor = modoDePendencia === 'percentual' ? m.cobertura.percentualPendente : m.cobertura.pendentes;
    const faixas = modoDePendencia === 'percentual' ? FAIXAS_PENDENCIA_PERCENTUAL : FAIXAS_PENDENCIA_QUANTIDADE;
    return {
      tipo: 'valor',
      cor: faixaDe(faixas, valor).cor,
      detalhe: `${nº(m.cobertura.pendentes)} de ${nº(m.cobertura.vinculosComCadencia)} vínculos pendentes (${m.cobertura.percentualPendente.toLocaleString('pt-BR')}%)`,
    };
  }

  function estadoDasVendas(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };
    const valor = m.vendas[recorteDeVendas];
    if (m.cobertura.clientes === 0 && valor === 0) return { tipo: 'semDado', detalhe: 'nenhum cliente com endereço aqui' };
    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_VENDAS, valor).cor,
      detalhe: `${ROTULO_DE_VENDAS[recorteDeVendas]}: ${reaisCompactos(valor)}`,
    };
  }

  function estadoDoPotencial(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };
    const potencial = m.potencial[0];
    if (!potencial || potencial.maquinasTeoricas === null)
      return { tipo: 'semDado', detalhe: 'área plantada não disponível' };
    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_POTENCIAL, potencial.maquinasTeoricas).cor,
      detalhe: `${nº(potencial.areaPlantadaHectares ?? 0)} ha · ${nº(potencial.maquinasTeoricas)} teóricos`,
    };
  }

  const classificacao = (indicador: ClassificacaoDeIndicador['indicador']) =>
    painel.dados?.classificacoes.find((c) => c.indicador === indicador) ?? null;

  const kpis: Indicador[] = [
    {
      rotulo: 'Municípios da ADR',
      valor: indicadores ? daAdr.length : null,
      deOnde: `${nº(totais.clientes)} clientes com endereço neles`,
      semDado: 'área de atuação não carregada',
    },
    {
      rotulo: 'Pendência de visita',
      valor: indicadores && totais.elegiveis > 0 ? `${((100 * totais.pendentes) / totais.elegiveis).toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%` : null,
      tom: 'atencao',
      deOnde: `${nº(totais.pendentes)} de ${nº(totais.elegiveis)} vínculos elegíveis · regra provisória`,
      semDado: 'sem vínculo elegível',
    },
    {
      rotulo: 'Vendas no período',
      valor: indicadores ? reaisCompactos(totais.vendas) : null,
      deOnde: `máquina ${reaisCompactos(totais.maquina)} · pós-venda ${reaisCompactos(totais.posVenda)} (composição provisória)`,
      semDado: '—',
    },
    {
      rotulo: regra ? `${regra.modeloDeReferencia} teóricos` : 'Potencial teórico',
      valor: indicadores && regra ? nº(Math.round(totais.maquinasTeoricas)) : null,
      deOnde: regra ? `1 a cada ${regra.hectaresPorMaquina} ha de ${regra.produtoNome} · estimativa regional, regra a confirmar` : '',
      semDado: 'sem regra de potencial',
    },
    {
      rotulo: 'CEN diferente entre as planilhas',
      valor: indicadores ? totais.cenDiferente : null,
      tom: totais.cenDiferente > 0 ? 'atencao' : undefined,
      deOnde: `mais ${nº(totais.cenGrafiaDiferente)} com grafia diferente (provável mesma pessoa, não confirmado) · as duas fontes seguem preservadas`,
      semDado: '—',
    },
  ];

  const escolhido = selecionado === null ? null : porCodigo.get(selecionado) ?? null;
  const ordenados = useMemo(() => [...daAdr].sort((a, b) => b.vendas.valorLiquido - a.vendas.valorLiquido), [daAdr]);
  const semFiltro = filtros.regiao === '' && filtros.lojaCodigo === '';

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Indicadores Geográficos da ADR</h1>
          <p className="page-subtitle">
            Área de atuação da Tracbel Agro em São Paulo, município a município: cobertura de visita, vendas e potencial
            por área. Dado do banco do CRM e do IBGE — nenhum número desta tela é ilustrativo.
          </p>
        </div>
        <SeloProcedencia procedencia={painel.procedencia} />
      </div>

      <div className="card cad-cartao terr-cartao">
        <div className="terr-alcance">
          <span>
            <strong>Visão:</strong>{' '}
            {filtros.visao === 'Empresa'
              ? 'empresa inteira — cada venda no município do cliente, qualquer que seja a filial que faturou.'
              : `filial ${contexto.empresa || 'do login'} e as abaixo dela — os clientes cadastrados nela e as notas que ela emitiu.`}
          </span>
          <span>
            <strong>Empresa inteira:</strong>{' '}
            {painel.dados?.podeVerEmpresaInteira
              ? 'disponível para o seu perfil.'
              : 'exige a permissão de alcance entre filiais; o seu perfil não a tem, e a distribuição oficial dos acessos está pendente (documento 32, P-10).'}
          </span>
          <span>A ADR, os responsáveis e a área plantada são da empresa inteira e aparecem para todos.</span>
        </div>
      </div>

      <div className="card cad-cartao terr-cartao">
        <div className="terr-filtros">
          <label className="terr-filtro">
            Vendas de
            <input
              type="month"
              value={filtros.competenciaInicial}
              onChange={(e) => setFiltros((f) => ({ ...f, competenciaInicial: e.target.value }))}
            />
          </label>
          <label className="terr-filtro">
            até
            <input
              type="month"
              value={filtros.competenciaFinal}
              onChange={(e) => setFiltros((f) => ({ ...f, competenciaFinal: e.target.value }))}
            />
          </label>
          <label className="terr-filtro">
            Região da ADR
            <select
              value={filtros.regiao}
              onChange={(e) => setFiltros((f) => ({ ...f, regiao: e.target.value as FiltrosTerritoriais['regiao'] }))}
            >
              <option value="">Norte e Noroeste</option>
              <option value="Norte">Norte</option>
              <option value="Noroeste">Noroeste</option>
            </select>
          </label>
          <label className="terr-filtro">
            Loja responsável
            <select value={filtros.lojaCodigo} onChange={(e) => setFiltros((f) => ({ ...f, lojaCodigo: e.target.value }))}>
              <option value="">Todas</option>
              {[...lojasConhecidas.entries()]
                .sort((a, b) => a[1].localeCompare(b[1]))
                .map(([codigo, nome]) => (
                  <option key={codigo} value={codigo}>
                    {nome}
                  </option>
                ))}
            </select>
          </label>
          <label className="terr-filtro">
            Visão
            <select
              value={filtros.visao}
              onChange={(e) => setFiltros((f) => ({ ...f, visao: e.target.value as FiltrosTerritoriais['visao'] }))}
            >
              <option value="Filial">Filial do cabeçalho</option>
              <option value="Empresa" disabled={painel.dados ? !painel.dados.podeVerEmpresaInteira : false}>
                Empresa inteira
              </option>
            </select>
            {painel.dados && !painel.dados.podeVerEmpresaInteira && (
              <span className="terr-filtro-motivo">empresa inteira: seu perfil não tem a permissão</span>
            )}
          </label>
          <FiltroDeFilial
            rotulo="Filial que vendeu"
            aplicaA="só às vendas"
            valor={filtros.filialDaVenda}
            lojas={lojasConhecidas}
            aoMudar={(valor) => setFiltros((f) => ({ ...f, filialDaVenda: valor }))}
          />
          <FiltroDeFilial
            rotulo="Filial de cadastro do cliente"
            aplicaA="à cobertura e às vendas"
            valor={filtros.filialDoCliente}
            lojas={lojasConhecidas}
            aoMudar={(valor) => setFiltros((f) => ({ ...f, filialDoCliente: valor }))}
          />
          <label className="terr-filtro">
            Cultura da regra
            <select value={regra?.produtoCodigoIbge ?? ''} disabled={!regra}>
              {regra && <option value={regra.produtoCodigoIbge}>{regra.produtoNome}</option>}
            </select>
            <span className="terr-filtro-motivo">só há uma regra informada</span>
          </label>
          <FiltroSemDado rotulo="Tipo de cliente" opcoes="SAM · KAM · Varejo" motivo="não há classificação por cliente" />
          <FiltroSemDado rotulo="Tipo de produto" opcoes="colhedora · trator grande · médio" motivo="vendas sem item da nota" />
          <FiltroSemDado rotulo="Modelo" opcoes="5090E · 3036N …" motivo="vendas sem item da nota" />
          <FiltroSemDado rotulo="CEN / gestor" opcoes="—" motivo="duas planilhas divergem; vigência a confirmar" />
        </div>
        {indicadores && (
          <p className="cad-sub">
            Vendas de <strong>{mes(indicadores.competenciaInicial)}</strong> a{' '}
            <strong>{mes(indicadores.competenciaFinal)}</strong>
            {filtros.competenciaInicial === '' && filtros.competenciaFinal === '' && ' (12 meses fechados)'} · cobertura
            medida em {new Date(indicadores.referenciaDaCobertura).toLocaleDateString('pt-BR')}, interação mais recente
            carregada em{' '}
            {indicadores.interacaoMaisRecente ? new Date(indicadores.interacaoMaisRecente).toLocaleDateString('pt-BR') : '—'}
            {indicadores.anoDaAreaPlantada && ` · área plantada PAM/IBGE ${indicadores.anoDaAreaPlantada}`}.
          </p>
        )}
      </div>

      {painel.dados && painel.dados.classificacoes.length > 0 && (
        <div className="card cad-cartao terr-cartao">
          <div className="terr-classificacoes" aria-label="Como ler estes números">
            <strong>Como ler estes números</strong>
            {painel.dados.classificacoes.map((c) => (
              <span key={c.indicador} className="terr-classificacao">
                <SeloDeClassificacao classificacao={c} /> {c.motivo}
              </span>
            ))}
          </div>
        </div>
      )}

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}
      {erroDaMalha && <BlocoErro erro={erroDaMalha} />}

      <PainelDeIndicadores indicadores={kpis} carregando={painel.carregando} />

      {(painel.carregando || !desenho) && !painel.erro && !erroDaMalha && <BlocoCarregando oQue="os mapas da ADR" />}

      {indicadores && desenho && (
        <div className="terr-grade-mapas">
          <div className="card cad-cartao terr-mapa">
            <div className="card-title">Cobertura de visitas — pendência <SeloDeClassificacao classificacao={classificacao('coberturaDeVisita')} /></div>
            <div className="card-subtitle">
              Vínculos em carteira comercial fora da cadência declarada ou nunca contatados, sobre os elegíveis.
            </div>
            <div className="terr-alternador" role="group" aria-label="Pendência em">
              {(['percentual', 'quantidade'] as const).map((modo) => (
                <button key={modo} type="button" aria-pressed={modoDePendencia === modo} onClick={() => setModoDePendencia(modo)}>
                  {modo === 'percentual' ? '% dos elegíveis' : 'quantidade'}
                </button>
              ))}
            </div>
            <MapaDeMunicipios
              id="pendencia"
              titulo="Mapa de pendência de visita por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDaPendencia}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
            />
            <LegendaDoMapa
              faixas={modoDePendencia === 'percentual' ? FAIXAS_PENDENCIA_PERCENTUAL : FAIXAS_PENDENCIA_QUANTIDADE}
              unidade={modoDePendencia === 'percentual' ? '% de vínculos pendentes' : 'vínculos pendentes'}
            />
            <p className="terr-aviso">
              {modoDePendencia === 'percentual'
                ? 'Percentual compara municípios de tamanhos diferentes; uma cidade com 3 vínculos pode ficar vermelha com 2 pendências.'
                : 'Quantidade favorece cidades grandes; alterne para % para comparar.'}{' '}
              "Visita" é qualquer interação registrada — a definição está pendente.
            </p>
          </div>

          <div className="card cad-cartao terr-mapa">
            <div className="card-title">Vendas realizadas <SeloDeClassificacao classificacao={classificacao('vendas')} /></div>
            <div className="card-subtitle">Faturamento líquido pelo endereço principal do cliente, no período.</div>
            <div className="terr-alternador" role="group" aria-label="Recorte das vendas">
              {(Object.keys(ROTULO_DE_VENDAS) as RecorteDeVendas[]).map((recorte) => (
                <button key={recorte} type="button" aria-pressed={recorteDeVendas === recorte} onClick={() => setRecorteDeVendas(recorte)}>
                  {ROTULO_DE_VENDAS[recorte]}
                </button>
              ))}
            </div>
            <MapaDeMunicipios
              id="vendas"
              titulo="Mapa de vendas por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDasVendas}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
            />
            <LegendaDoMapa faixas={FAIXAS_VENDAS} unidade={`R$ — ${ROTULO_DE_VENDAS[recorteDeVendas].toLowerCase()}`} />
            <p className="terr-aviso">
              Valor absoluto favorece cidades grandes. Devolução e cancelamento não são abatidos. Pós-venda = peça +
              serviço, <strong>composição provisória</strong>.
            </p>
          </div>

          <div className="card cad-cartao terr-mapa">
            <div className="card-title">Potencial teórico — cultura × área <SeloDeClassificacao classificacao={classificacao('potencial')} /></div>
            <div className="card-subtitle">
              {regra
                ? `1 ${regra.modeloDeReferencia} a cada ${regra.hectaresPorMaquina} ha de ${regra.produtoNome} · área plantada do município (IBGE): estimativa da região, não área de cliente`
                : 'Sem regra de potencial ativa'}
            </div>
            <div className="terr-alternador" role="group" aria-label="Recorte do potencial">
              <button type="button" aria-pressed="true">
                Região total
              </button>
              <button type="button" disabled title="Sem área por cultura de cliente">
                Clientes
              </button>
            </div>
            <MapaDeMunicipios
              id="potencial"
              titulo="Mapa de potencial teórico por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDoPotencial}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
            />
            <LegendaDoMapa faixas={FAIXAS_POTENCIAL} unidade="máquinas teóricas (necessidade de frota, não venda)" />
            <p className="terr-aviso terr-aviso-alerta">
              Regra a confirmar. A área do IBGE não comprova a área de nenhuma propriedade e não substitui a regra
              comercial. Não considera ciclo de troca, parque instalado, concorrência nem outras culturas; não separa
              cliente de não cliente.
            </p>
          </div>
        </div>
      )}

      {escolhido && indicadores && (
        <DetalheDoMunicipio municipio={escolhido} regras={indicadores.regras} aoFechar={() => setSelecionado(null)} />
      )}

      <MetricasSemDado metricas={painel.dados?.metricasSemDado} titulo="O que estes mapas não dizem" />

      {indicadores && (
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Municípios da ADR e o que ficou fora do mapa</div>
              <div className="card-subtitle">
                A soma das linhas é o total da consulta. Clique numa linha para abrir o detalhe.
              </div>
            </div>
          </div>
          <div className="cad-tabela-wrap terr-tabela-municipios">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Indicadores por município</caption>
              <thead>
                <tr>
                  <th scope="col">Município</th>
                  <th scope="col">Região · loja</th>
                  <th scope="col">Elegíveis</th>
                  <th scope="col">Pendentes</th>
                  <th scope="col">Vendas</th>
                  <th scope="col">Pós-venda <span className="cad-sub">(provisório)</span></th>
                  <th scope="col">{regra ? `${regra.modeloDeReferencia} teóricos` : 'Potencial'}</th>
                </tr>
              </thead>
              <tbody>
                {ordenados.map((m) => (
                  <tr key={m.codigoIbge} aria-current={m.codigoIbge === selecionado ? 'true' : undefined}>
                    <td>
                      <button type="button" className="cad-th-ordenar" onClick={() => setSelecionado(m.codigoIbge)}>
                        {m.nome}
                      </button>
                      {m.comparacaoDoCen === 'NomesDiferentes' && <div className="cad-sub cad-atencao">CEN diferente nas planilhas</div>}
                      {m.comparacaoDoCen === 'ProvavelMesmaPessoa' && <div className="cad-sub">CEN com grafia diferente</div>}
                    </td>
                    <td>
                      {m.regiao}
                      <div className="cad-sub">{m.lojaNome?.replace(/^.*—\s*/, '') ?? '—'}</div>
                    </td>
                    <td className="cad-mono">{nº(m.cobertura.vinculosComCadencia)}</td>
                    <td className="cad-mono">
                      {nº(m.cobertura.pendentes)}
                      {m.cobertura.percentualPendente !== null && (
                        <div className="cad-sub">{m.cobertura.percentualPendente.toLocaleString('pt-BR')}%</div>
                      )}
                    </td>
                    <td className="cad-mono">{reaisCompactos(m.vendas.valorLiquido)}</td>
                    <td className="cad-mono">{reaisCompactos(m.vendas.posVenda)}</td>
                    <td className="cad-mono">{m.potencial[0]?.maquinasTeoricas === null || !m.potencial[0] ? '—' : nº(m.potencial[0].maquinasTeoricas)}</td>
                  </tr>
                ))}
                <tr className="terr-linha-total">
                  <td>Total da ADR {semFiltro ? '' : '(filtro)'}</td>
                  <td>{daAdr.length} municípios</td>
                  <td className="cad-mono">{nº(totais.elegiveis)}</td>
                  <td className="cad-mono">{nº(totais.pendentes)}</td>
                  <td className="cad-mono">{reaisCompactos(totais.vendas)}</td>
                  <td className="cad-mono">{reaisCompactos(totais.posVenda)}</td>
                  <td className="cad-mono">{nº(Math.round(totais.maquinasTeoricas))}</td>
                </tr>
                {semFiltro && (
                  <LinhaDeGrupo
                    rotulo="São Paulo fora da ADR"
                    descricao="municípios com cliente que a planilha não marca como ADR"
                    itens={municipios.filter((m) => !m.pertenceAAdr)}
                  />
                )}
                {indicadores.foraDoMapa.map((g) => (
                  <tr key={g.grupo}>
                    <td>
                      {g.grupo}
                      <div className="cad-sub">{g.descricao}</div>
                    </td>
                    <td>—</td>
                    <td className="cad-mono">{nº(g.cobertura.vinculosComCadencia)}</td>
                    <td className="cad-mono">{nº(g.cobertura.pendentes)}</td>
                    <td className="cad-mono">{reaisCompactos(g.vendas.valorLiquido)}</td>
                    <td className="cad-mono">{reaisCompactos(g.vendas.posVenda)}</td>
                    <td>—</td>
                  </tr>
                ))}
                {semFiltro && (
                  <LinhaDeTotal municipios={municipios} foraDoMapa={indicadores.foraDoMapa} />
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </>
  );
}

/** O selo de como ler um indicador — medido, regra provisória, estimativa —, com o motivo no título. */
function SeloDeClassificacao({ classificacao }: { classificacao: ClassificacaoDeIndicador | null }) {
  if (!classificacao) return null;
  return (
    <span className={`terr-selo terr-selo-${classificacao.situacao}`} title={classificacao.motivo}>
      {classificacao.selo}
    </span>
  );
}

/** Um filtro pedido que o dado não sustenta: aparece, desligado, dizendo por quê. */
function FiltroSemDado({ rotulo, opcoes, motivo }: { rotulo: string; opcoes: string; motivo: string }) {
  return (
    <label className="terr-filtro">
      {rotulo}
      <select disabled title={motivo}>
        <option>{opcoes}</option>
      </select>
      <span className="terr-filtro-motivo">sem dado: {motivo}</span>
    </label>
  );
}

/** Uma linha que soma um grupo de municípios. */
function LinhaDeGrupo({ rotulo, descricao, itens }: { rotulo: string; descricao: string; itens: IndicadoresDoMunicipio[] }) {
  const soma = (f: (m: IndicadoresDoMunicipio) => number) => itens.reduce((s, m) => s + f(m), 0);
  return (
    <tr>
      <td>
        {rotulo}
        <div className="cad-sub">
          {descricao} ({itens.length})
        </div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.posVenda))}</td>
      <td>—</td>
    </tr>
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
}) {
  return (
    <label className="terr-filtro">
      {rotulo}
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
      <span className="terr-filtro-motivo">aplica-se {aplicaA}</span>
    </label>
  );
}

/**
 * O total da consulta: municípios do mapa mais o que ficou fora dele. É o número
 * que a conferência SQL do documento 32 compara — se não fechar, algo sumiu ou
 * foi contado duas vezes.
 */
function LinhaDeTotal({
  municipios,
  foraDoMapa,
}: {
  municipios: IndicadoresDoMunicipio[];
  foraDoMapa: { cobertura: { vinculosComCadencia: number; pendentes: number }; vendas: { valorLiquido: number; posVenda: number } }[];
}) {
  const somaMapa = (f: (m: IndicadoresDoMunicipio) => number) => municipios.reduce((s, m) => s + f(m), 0);
  const somaFora = (f: (g: (typeof foraDoMapa)[number]) => number) => foraDoMapa.reduce((s, g) => s + f(g), 0);
  return (
    <tr className="terr-linha-total">
      <td>
        Total da consulta
        <div className="cad-sub">municípios do mapa + tudo o que ficou fora dele</div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.vinculosComCadencia) + somaFora((g) => g.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.pendentes) + somaFora((g) => g.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.valorLiquido) + somaFora((g) => g.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.posVenda) + somaFora((g) => g.vendas.posVenda))}</td>
      <td>—</td>
    </tr>
  );
}
