/**
 * A DISTRIBUIÇÃO DOS MUNICÍPIOS PELAS CINCO CLASSES — e o filtro da tela (28/09/2026).
 *
 * ERAM SEIS CARTÕES IGUAIS, um por classe e um de total, que diziam cinco números sem dizer a proporção: 20 de alta
 * prioridade é muito ou pouco? A barra responde num relance, e cada classe é um botão que filtra o mapa e a tabela —
 * o que antes era um seletor "Classe" escondido no meio dos filtros da tabela.
 *
 * OS PESOS FICAM AQUI, EM UMA LINHA, porque são eles que decidem a classe de cada município: quem olha a
 * distribuição precisa saber que ela sai de pesos ainda a confirmar.
 */

import { Link } from 'react-router-dom';
import { InfoTooltip } from '../InfoTooltip';
import { Painel } from '../dashboard/Dashboard';
import { MetricasSemDado } from '../cadastro/SemDado';
import type { ClasseDePrioridade, DiagnosticoComercialDaRegiao } from '../../tipos/mercado';
import { CLASSES, COMPONENTES, dataCurta, n } from './diagnostico';

export function DistribuicaoPorClasse({
  dados,
  classe,
  aoEscolherClasse,
}: {
  dados: DiagnosticoComercialDaRegiao;
  classe: ClasseDePrioridade | null;
  aoEscolherClasse: (classe: ClasseDePrioridade | null) => void;
}) {
  const resumo = dados.resumo;
  const quantos = (c: ClasseDePrioridade) => resumo[(c.charAt(0).toLowerCase() + c.slice(1)) as Uncapitalize<ClasseDePrioridade>];
  const comIndice = resumo.total - resumo.semIndice;

  return (
    <Painel
      titulo="Prioridade dos municípios"
      metodologia={
        'O IOC vai de 0 a 100: alto é muito a ganhar — potencial grande e pouco explorado, com crédito e preço a favor. É ' +
        'uma ordem de prioridade entre os municípios, e não previsão de venda. As classes vão de 20 em 20 pontos. ' +
        'Componente sem dado num município sai da conta e os pesos dos outros são normalizados.'
      }
      acao={
        dados.lacunas.length > 0 ? (
          // AS LIMITAÇÕES MORAM NUMA DICA, como nos Indicadores Geográficos: são auditoria, e não a primeira leitura.
          <span className="diag-limitacoes">
            Limitações dos dados
            <InfoTooltip
              rotulo="Limitações dos dados do diagnóstico"
              texto={<MetricasSemDado metricas={dados.lacunas} titulo="O que este diagnóstico não afirma" naDica />}
            />
          </span>
        ) : undefined
      }
      data-bloco="distribuicao"
    >
      <div className="diag-distribuicao" role="group" aria-label="Filtrar pela classe de prioridade">
        <div className="diag-distribuicao-barra" aria-hidden="true">
          {CLASSES.map((c) => {
            const q = quantos(c.chave);
            return q > 0 && comIndice > 0 ? (
              <span
                key={c.chave}
                style={{ width: `${(100 * q) / comIndice}%`, background: c.cor }}
                data-apagada={classe !== null && classe !== c.chave ? 'true' : undefined}
              />
            ) : null;
          })}
        </div>
        <div className="diag-distribuicao-legenda">
          {CLASSES.map((c) => {
            const q = quantos(c.chave);
            const ativa = classe === c.chave;
            return (
              <button
                key={c.chave}
                type="button"
                className="diag-classe-botao"
                aria-pressed={ativa}
                onClick={() => aoEscolherClasse(ativa ? null : c.chave)}
                data-apagada={classe !== null && !ativa ? 'true' : undefined}
              >
                <span className="diag-classe-amostra" style={{ background: c.cor }} aria-hidden="true" />
                <span className="diag-classe-nome">{c.rotulo}</span>
                <strong className="diag-classe-quantos">{n(q, 0)}</strong>
                <span className="diag-classe-faixa">
                  {c.faixa}
                  {comIndice > 0 && ` · ${n((100 * q) / comIndice, 0)}%`}
                </span>
              </button>
            );
          })}
        </div>
        {classe !== null && (
          <button type="button" className="diag-limpar" onClick={() => aoEscolherClasse(null)}>
            Mostrar todas as classes
          </button>
        )}
      </div>

      <div className="diag-pesos" data-bloco="pesos">
        <span>
          <strong>Pesos do IOC</strong>{' '}
          {dados.pesos
            ? COMPONENTES.map((c) => `${c.rotulo.toLowerCase()} ${n(dados.pesos![c.chave], 2)}`).join(' · ')
            : 'não registrados'}
          {dados.pesosVigentesDesde && <> — vigentes desde {dataCurta(dados.pesosVigentesDesde)}</>}
          {dados.pesosDoPrototipo && <span className="diag-a-confirmar">protótipo, a confirmar</span>}
        </span>
        {dados.shares.length > 0 && (
          <span>
            <strong>Share-alvo</strong> {dados.shares.map((s) => `${s.categoriaNome} ${n(s.percentual)}%`).join(' · ')}
          </span>
        )}
        <Link to="/config" className="diag-link">
          Ajustar em Configurações › Potencial de mercado
        </Link>
      </div>

      {resumo.semIndice > 0 && (
        <p className="diag-nota">
          {n(resumo.semIndice, 0)} {resumo.semIndice === 1 ? 'município ficou' : 'municípios ficaram'} sem IOC: nenhum
          componente com peso tem dado neles.
        </p>
      )}
    </Painel>
  );
}
