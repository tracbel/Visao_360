/**
 * Diagnóstico Comercial — o Índice de Oportunidade Comercial (IOC) de cada município da ADR (issue 257).
 *
 * É a aba 5 do protótipo da pasta 360 (`renderDiag`), com a mesma pergunta — onde a Tracbel tem mais a ganhar
 * agindo agora — e as mesmas contas. O que muda é a estrutura:
 *
 * - **os números vêm do banco do CRM**, pela mesma apuração dos Indicadores Geográficos, e não de JSON embutido;
 * - **os pesos são parâmetro com vigência** (Configurações › Potencial de mercado), e não um campo que cada
 *   navegador guarda do seu jeito;
 * - **componente sem dado sai da conta e é dito** na linha: sem ART não há venda, e o protótipo a tratava como zero.
 *
 * A tabela ordena por qualquer coluna, filtra por classe e exporta para CSV exatamente o que está na tela. Clicar
 * num município abre os sete componentes.
 */

import { Fragment, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterDiagnosticoComercial } from '../dados/api/mercado';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type {
  ClasseDePrioridade,
  ComponentesDoIoc,
  FiltrosDoDiagnostico,
  MunicipioNoDiagnostico,
} from '../tipos/mercado';
import '../estilos/diagnostico.css';

/** As cinco classes: o nome inteiro nos cartões e no CSV; o curto no selo da tabela, com o inteiro na dica. */
export const CLASSES: { chave: ClasseDePrioridade; rotulo: string; curto: string; faixa: string }[] = [
  { chave: 'Maxima', rotulo: 'Prioridade máxima', curto: 'Máxima', faixa: 'IOC 80 ou mais' },
  { chave: 'Alta', rotulo: 'Alta prioridade', curto: 'Alta', faixa: 'IOC 60 a 80' },
  { chave: 'Moderada', rotulo: 'Prioridade moderada', curto: 'Moderada', faixa: 'IOC 40 a 60' },
  { chave: 'Baixa', rotulo: 'Baixa prioridade', curto: 'Baixa', faixa: 'IOC 20 a 40' },
  { chave: 'Manutencao', rotulo: 'Manutenção', curto: 'Manutenção', faixa: 'IOC abaixo de 20' },
];

const ROTULO_DA_CLASSE = Object.fromEntries(CLASSES.map((c) => [c.chave, c.rotulo])) as Record<ClasseDePrioridade, string>;
const CURTO_DA_CLASSE = Object.fromEntries(CLASSES.map((c) => [c.chave, c.curto])) as Record<ClasseDePrioridade, string>;

const COMPONENTES: { chave: keyof ComponentesDoIoc; rotulo: string; mede: string }[] = [
  { chave: 'potencial', rotulo: 'Potencial', mede: 'a demanda do município sobre o percentil 90 da ADR' },
  { chave: 'cobertura', rotulo: 'Cobertura', mede: 'os vínculos da carteira fora da cadência' },
  { chave: 'credito', rotulo: 'Crédito', mede: 'o crédito de mecanização (SICOR), de −40% a +40%' },
  { chave: 'rentabilidade', rotulo: 'Rentabilidade', mede: 'o preço da cultura principal, de −40% a +40%' },
  { chave: 'clientes', rotulo: 'Clientes', mede: 'poucos clientes frente à demanda' },
  { chave: 'realizacao', rotulo: 'Realização', mede: 'a distância até a meta de planejamento (share-alvo)' },
  { chave: 'penetracao', rotulo: 'Penetração', mede: 'a venda da Tracbel sobre a demanda estrutural' },
];

type Coluna =
  | 'nome'
  | 'culturaPrincipal'
  | 'ioc'
  | 'demandaEstrutural'
  | 'vendidasNoPeriodo'
  | 'clientes'
  | 'cobertura'
  | 'penetracao'
  | 'indiceDeCredito';

// A LOJA E A REGIÃO VÃO NA SUB-LINHA DO MUNICÍPIO, e a demanda ajustada na da demanda: com uma coluna para cada, a
// tabela passava da largura do cartão a 1.536 px e o crédito, a situação e o plano ficavam escondidos na rolagem.
const COLUNAS: { chave: Coluna; rotulo: string; texto?: boolean }[] = [
  { chave: 'nome', rotulo: 'Município', texto: true },
  { chave: 'culturaPrincipal', rotulo: 'Cultura principal', texto: true },
  { chave: 'ioc', rotulo: 'IOC' },
  { chave: 'demandaEstrutural', rotulo: 'Demanda/ano' },
  { chave: 'vendidasNoPeriodo', rotulo: 'Vendidas' },
  { chave: 'clientes', rotulo: 'Clientes' },
  { chave: 'cobertura', rotulo: 'Cobertura' },
  { chave: 'penetracao', rotulo: 'Penetração' },
  { chave: 'indiceDeCredito', rotulo: 'Crédito' },
];

const SEM_ART = 'O ART não trouxe as vendas de máquina deste recorte: ausência de carga não é venda zero.';

const n = (v: number, casas = 1) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });
const pct = (v: number) => `${n(v * 100, 0)}%`;
const variacao = (indice: number) => {
  const d = (indice - 1) * 100;
  return `${d > 0 ? '+' : d < 0 ? '−' : ''}${n(Math.abs(d), 0)}%`;
};
const mes = (aaaammdd: string) => `${aaaammdd.slice(5, 7)}/${aaaammdd.slice(0, 4)}`;

/** Ordena com os vazios no fim, nos dois sentidos — "sem dado" nunca é o maior nem o menor. */
export function ordenar(linhas: MunicipioNoDiagnostico[], coluna: Coluna, sentido: 1 | -1): MunicipioNoDiagnostico[] {
  return [...linhas].sort((a, b) => {
    const va = a[coluna];
    const vb = b[coluna];
    if (va === null && vb === null) return 0;
    if (va === null) return 1;
    if (vb === null) return -1;
    if (typeof va === 'string' && typeof vb === 'string') return sentido * va.localeCompare(vb, 'pt-BR');
    return sentido * ((va as number) - (vb as number));
  });
}

/** As linhas da tela como o CSV as leva — a mesma ordem e os mesmos filtros. */
export function linhasDoCsv(linhas: MunicipioNoDiagnostico[]): unknown[][] {
  const num = (v: number | null, casas = 2) =>
    v === null ? '' : v.toLocaleString('pt-BR', { maximumFractionDigits: casas, useGrouping: false });
  return linhas.map((l) => [
    l.nome,
    l.regiao,
    l.loja ?? '',
    l.culturaPrincipal ?? '',
    num(l.ioc, 1),
    l.classe ? ROTULO_DA_CLASSE[l.classe] : '',
    num(l.demandaEstrutural),
    num(l.demandaAjustada),
    num(l.metaDePlanejamento),
    l.vendidasNoPeriodo ?? '',
    num(l.vendidasNoAno),
    l.clientes,
    l.cobertura === null ? '' : num(l.cobertura * 100, 1),
    l.penetracao === null ? '' : num(l.penetracao * 100, 1),
    num(l.indiceDeCredito, 3),
    num(l.indiceDePreco, 3),
    l.situacao,
    l.planoDeAcao,
    l.componentesAusentes.join(' | '),
  ]);
}

const CABECALHO_DO_CSV = [
  'Município', 'Região', 'Loja', 'Cultura principal', 'IOC', 'Classe', 'Demanda estrutural (máq/ano)', 'Demanda ajustada',
  'Meta de planejamento', 'Vendidas no período', 'Vendidas no ano', 'Clientes', 'Cobertura (%)', 'Penetração (%)',
  'Índice de crédito', 'Índice de preço', 'Situação', 'Plano de ação', 'Fora da conta',
];

export function DiagnosticoComercial() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDoDiagnostico>({});
  const [classe, setClasse] = useState<ClasseDePrioridade | ''>('');
  const [busca, setBusca] = useState('');
  const [ordem, setOrdem] = useState<{ coluna: Coluna; sentido: 1 | -1 }>({ coluna: 'ioc', sentido: -1 });
  const [aberto, setAberto] = useState<number | null>(null);

  const diagnostico = useRecurso(
    (sinal) => obterDiagnosticoComercial(contexto, filtros, sinal),
    [contexto.empresa, contexto.usuario, filtros.competenciaInicial, filtros.competenciaFinal, filtros.regiao, filtros.categoria],
  );
  const dados = diagnostico.dados;

  const visiveis = useMemo(() => {
    if (!dados) return [];
    const termo = busca.trim().toLocaleLowerCase('pt-BR');
    const filtradas = dados.municipios.filter(
      (m) => (!classe || m.classe === classe) && (!termo || m.nome.toLocaleLowerCase('pt-BR').includes(termo)),
    );
    return ordenar(filtradas, ordem.coluna, ordem.sentido);
  }, [dados, classe, busca, ordem]);

  const mudar = (parcial: Partial<FiltrosDoDiagnostico>) => setFiltros((f) => ({ ...f, ...parcial }));

  function ordenarPor(coluna: Coluna, texto?: boolean) {
    setOrdem((o) => (o.coluna === coluna ? { coluna, sentido: o.sentido === 1 ? -1 : 1 } : { coluna, sentido: texto ? 1 : -1 }));
  }

  const resumo = dados?.resumo;
  const indicadores: Indicador[] = [
    ...CLASSES.map(
      (c): Indicador => ({
        rotulo: c.rotulo,
        valor: resumo ? resumo[primeiraMinuscula(c.chave)] : null,
        deOnde: c.faixa,
        // SÓ A MÁXIMA EM DESTAQUE: o tom de alerta pinta de vermelho, e prioridade alta é oportunidade, não problema.
        tom: c.chave === 'Maxima' ? 'bom' : 'neutro',
      }),
    ),
    {
      rotulo: 'Municípios da ADR',
      valor: resumo ? resumo.total : null,
      deOnde: resumo?.iocMedio != null ? `IOC médio ${n(resumo.iocMedio)}` : 'sem IOC calculado',
    },
  ];

  const semArt = dados?.lacunas.some((l) => l.metrica === 'vendasEmUnidades') ?? false;

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Diagnóstico Comercial</h1>
          <p className="page-subtitle">
            O Índice de Oportunidade Comercial (IOC) de cada município da ADR: onde a Tracbel tem mais a ganhar agindo
            agora — muito potencial, pouca exploração e mercado a favor.
          </p>
        </div>
        {dados && (
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => baixarCsv(`diagnostico-comercial-${dados.categoria.toLowerCase()}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(visiveis))}
            disabled={visiveis.length === 0}
          >
            Exportar CSV
          </button>
        )}
      </div>

      <AvisoDeProcedencia procedencia={diagnostico.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={diagnostico.carregando} />

      {dados && (
        <div className="diag-pesos" data-bloco="pesos">
          <span>
            <strong>Pesos do IOC</strong>
            {dados.pesos ? (
              <>
                {' '}
                {COMPONENTES.map((c) => `${c.rotulo.toLowerCase()} ${n(dados.pesos![c.chave], 2)}`).join(' · ')}
                {dados.pesosVigentesDesde && <> — vigentes desde {dataCurta(dados.pesosVigentesDesde)}</>}
                {dados.pesosDoPrototipo && <span className="diag-a-confirmar">protótipo, a confirmar</span>}
              </>
            ) : (
              ' não registrados'
            )}
          </span>
          {dados.shares.length > 0 && (
            <span>
              <strong>Share-alvo</strong>{' '}
              {dados.shares.map((s) => `${s.categoriaNome} ${n(s.percentual)}%`).join(' · ')}
            </span>
          )}
          <Link to="/config" className="diag-link">
            Ajustar em Configurações › Potencial de mercado
          </Link>
        </div>
      )}

      <MetricasSemDado metricas={dados?.lacunas} titulo="O que este diagnóstico não afirma" />

      <div className="card cad-cartao" data-bloco="municipios-por-prioridade">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Municípios por prioridade</div>
            <div className="card-subtitle">
              {dados
                ? `${dados.categoriaNome} · vendas de ${mes(dados.competenciaInicial)} a ${mes(dados.competenciaFinal)}` +
                  (dados.fracaoDoAnoNoPeriodo !== 1 ? ` (levadas a um ano pela sazonalidade: ${pct(dados.fracaoDoAnoNoPeriodo)} do ano)` : '')
                : 'o IOC de cada município, do maior para o menor'}
            </div>
          </div>
          <SeloProcedencia procedencia={diagnostico.procedencia} />
        </div>

        <div className="cad-barra diag-barra-de-filtros">
          <label className="cad-filtro">
            Categoria
            <select value={filtros.categoria ?? dados?.categoria ?? 'TRATOR'} onChange={(e) => mudar({ categoria: e.target.value })}>
              {(dados?.categorias.length ? dados.categorias : [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }]).map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  {c.nome}
                </option>
              ))}
              <option value="TODAS">Todas as categorias</option>
            </select>
          </label>
          <label className="cad-filtro">
            Região
            <select value={filtros.regiao ?? ''} onChange={(e) => mudar({ regiao: e.target.value || undefined })}>
              <option value="">ADR inteira</option>
              <option value="Norte">Norte</option>
              <option value="Noroeste">Noroeste</option>
            </select>
          </label>
          <label className="cad-filtro">
            Vendas de
            <input
              type="month"
              value={filtros.competenciaInicial ?? dados?.competenciaInicial.slice(0, 7) ?? ''}
              onChange={(e) => e.target.value && mudar({ competenciaInicial: e.target.value })}
            />
          </label>
          <label className="cad-filtro">
            Até
            <input
              type="month"
              value={filtros.competenciaFinal ?? dados?.competenciaFinal.slice(0, 7) ?? ''}
              onChange={(e) => e.target.value && mudar({ competenciaFinal: e.target.value })}
            />
          </label>
          <label className="cad-filtro">
            Classe
            <select value={classe} onChange={(e) => setClasse(e.target.value as ClasseDePrioridade | '')}>
              <option value="">Todas</option>
              {CLASSES.map((c) => (
                <option key={c.chave} value={c.chave}>
                  {c.rotulo}
                </option>
              ))}
            </select>
          </label>
          <label className="cad-filtro">
            Município
            <input type="search" value={busca} placeholder="Buscar pelo nome" onChange={(e) => setBusca(e.target.value)} />
          </label>
          {(filtros.competenciaInicial || filtros.competenciaFinal) && (
            <div className="cad-filtro">
              &nbsp;
              <button type="button" className="funil-aba" onClick={() => mudar({ competenciaInicial: undefined, competenciaFinal: undefined })}>
                Últimos 12 meses fechados
              </button>
            </div>
          )}
        </div>

        {diagnostico.carregando && <BlocoCarregando oQue="o diagnóstico" />}
        {diagnostico.erro && <BlocoErro erro={diagnostico.erro} aoTentarDeNovo={diagnostico.recarregar} />}

        {dados && dados.municipios.length === 0 && (
          <BlocoVazio
            titulo="Nenhum município da ADR neste recorte"
            texto="A área de atuação entra pela carga do território; sem ela, o diagnóstico não tem município para ordenar."
          />
        )}

        {dados && dados.municipios.length > 0 && (
          <div className="cad-tabela-wrap">
            <table className="cad-tabela diag-tabela">
              <caption className="cad-so-leitor">Municípios da ADR pelo Índice de Oportunidade Comercial</caption>
              <thead>
                <tr>
                  {COLUNAS.map((c) => (
                    <th
                      key={c.chave}
                      scope="col"
                      aria-sort={ordem.coluna === c.chave ? (ordem.sentido === 1 ? 'ascending' : 'descending') : 'none'}
                    >
                      <button type="button" className="diag-ordenar" onClick={() => ordenarPor(c.chave, c.texto)}>
                        {c.rotulo}
                        {ordem.coluna === c.chave && <span aria-hidden="true">{ordem.sentido === 1 ? ' ▲' : ' ▼'}</span>}
                      </button>
                    </th>
                  ))}
                  <th scope="col">Situação e plano de ação</th>
                </tr>
              </thead>
              <tbody>
                {visiveis.map((m) => (
                  <Fragment key={m.codigoIbge}>
                    <LinhaDoMunicipio
                      municipio={m}
                      semArt={semArt}
                      aberta={aberto === m.codigoIbge}
                      alternar={() => setAberto((a) => (a === m.codigoIbge ? null : m.codigoIbge))}
                    />
                    {aberto === m.codigoIbge && (
                      <tr className="diag-detalhe">
                        <td colSpan={COLUNAS.length + 1}>
                          <Componentes municipio={m} />
                        </td>
                      </tr>
                    )}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {dados && visiveis.length === 0 && dados.municipios.length > 0 && (
          <p className="diag-nada">Nenhum município com esses filtros.</p>
        )}

        <div className="funil-scope-note">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 8h.01" />
            <path d="M11 12h1v4h1" />
          </svg>
          <div>
            <strong>Como ler:</strong> IOC alto é muito a ganhar — potencial grande e pouco explorado, com crédito e preço a
            favor. É uma ordem de prioridade entre os municípios, e não previsão de venda. Clique num município para ver os
            sete componentes e o que ficou fora da conta.
          </div>
        </div>
      </div>
    </>
  );
}

function LinhaDoMunicipio({
  municipio: m,
  semArt,
  aberta,
  alternar,
}: {
  municipio: MunicipioNoDiagnostico;
  semArt: boolean;
  aberta: boolean;
  alternar: () => void;
}) {
  const semDemanda = 'O município não tem demanda estimada nesta categoria: falta regra de potencial ou área plantada.';
  return (
    <tr className={aberta ? 'diag-linha aberta' : 'diag-linha'}>
      <td>
        <button type="button" className="diag-municipio" aria-expanded={aberta} onClick={alternar}>
          {m.nome}
        </button>
        <div className="cad-sub">
          {m.regiao}
          {m.loja && <> · {m.loja}</>}
        </div>
      </td>
      <td>
        {m.culturaPrincipal ?? '—'}
        {m.indiceDePreco !== null && (
          <span className={`diag-seta ${m.indiceDePreco >= 1 ? 'sobe' : 'desce'}`} title="Momento de preço da cultura">
            {' '}
            {variacao(m.indiceDePreco)}
          </span>
        )}
      </td>
      <td>
        {m.ioc === null || m.classe === null ? (
          <ValorAusente motivo="Nenhum componente com peso tem dado neste município." oQue={`o IOC de ${m.nome}`} />
        ) : (
          <span className={`diag-ioc diag-classe-${m.classe}`} title={ROTULO_DA_CLASSE[m.classe]}>
            <strong>{n(m.ioc)}</strong>
            <span className="diag-classe-rotulo">{CURTO_DA_CLASSE[m.classe]}</span>
          </span>
        )}
      </td>
      <td className="cad-mono">
        {m.demandaEstrutural === null ? <ValorAusente motivo={semDemanda} oQue="a demanda" /> : n(m.demandaEstrutural)}
        {m.demandaAjustada !== null && m.demandaAjustada !== m.demandaEstrutural && (
          <div className="cad-sub" title="A demanda ajustada pelo momento do município: preço, crédito e percepção">
            ajustada {n(m.demandaAjustada)}
          </div>
        )}
      </td>
      <td className="cad-mono">
        {m.vendidasNoPeriodo === null ? (
          <ValorAusente motivo={semArt ? SEM_ART : 'Sem venda do ART para este município.'} oQue="as vendas" />
        ) : (
          n(m.vendidasNoPeriodo, 0)
        )}
      </td>
      <td className="cad-mono">{n(m.clientes, 0)}</td>
      <td className="cad-mono">
        {m.cobertura === null ? (
          <ValorAusente motivo="Nenhum vínculo de carteira com cadência declarada neste município." oQue="a cobertura" />
        ) : (
          <>
            {pct(m.cobertura)}
            <div className="cad-sub">
              {n(m.cobertos, 0)} de {n(m.vinculosComCadencia, 0)}
            </div>
          </>
        )}
      </td>
      <td className="cad-mono">
        {m.penetracao === null ? (
          <ValorAusente motivo={semArt ? SEM_ART : 'Sem demanda estimada para comparar.'} oQue="a penetração" />
        ) : (
          pct(m.penetracao)
        )}
      </td>
      <td className="cad-mono">
        {m.indiceDeCredito === null ? (
          <ValorAusente motivo="O SICOR não formou o índice de crédito deste município." oQue="o crédito" />
        ) : (
          <>
            <span className={`diag-seta ${m.indiceDeCredito >= 1 ? 'sobe' : 'desce'}`}>{variacao(m.indiceDeCredito)}</span>
            {m.creditoBasePequena && <div className="cad-sub">base pequena</div>}
          </>
        )}
      </td>
      <td className="diag-texto">
        {m.situacao}
        {m.planoDeAcao && <div className="diag-plano">{m.planoDeAcao}</div>}
      </td>
    </tr>
  );
}

/** Os sete componentes de um município, em barras de 0 a 1, e o que ficou fora da conta. */
function Componentes({ municipio: m }: { municipio: MunicipioNoDiagnostico }) {
  return (
    <div className="diag-componentes">
      <ul>
        {COMPONENTES.map((c) => {
          const valor = m.componentes?.[c.chave] ?? null;
          return (
            <li key={c.chave}>
              <span className="diag-componente-nome">{c.rotulo}</span>
              <span className="diag-barra" aria-hidden="true">
                {valor !== null && <span style={{ width: `${valor * 100}%` }} />}
              </span>
              <span className="diag-componente-valor cad-mono">{valor === null ? 'fora da conta' : n(valor, 2)}</span>
              <span className="diag-componente-mede">{c.mede}</span>
            </li>
          );
        })}
      </ul>
      {m.componentesAusentes.length > 0 && (
        <p className="diag-ausentes">
          <strong>Fora da conta:</strong> {m.componentesAusentes.join('; ')}. Os pesos dos outros componentes foram
          normalizados.
        </p>
      )}
      {m.metaDePlanejamento !== null && (
        <p className="diag-ausentes">
          Meta de planejamento: <strong>{n(m.metaDePlanejamento)}</strong> máquinas por ano (demanda × share-alvo)
          {m.vendidasNoAno !== null && <> · vendidas no ano: {n(m.vendidasNoAno)}</>}.
        </p>
      )}
    </div>
  );
}

function primeiraMinuscula<T extends string>(texto: T) {
  return (texto.charAt(0).toLowerCase() + texto.slice(1)) as Uncapitalize<T>;
}

function dataCurta(aaaammdd: string) {
  const [ano, mesDoAno, dia] = aaaammdd.split('-');
  return `${dia}/${mesDoAno}/${ano}`;
}
