/**
 * O QUE A TELA NÃO PODE AFIRMAR — e por quê, com o número medido do lado.
 *
 * ESTE ARQUIVO É A REGRA QUE NÃO SE QUEBRA, em forma de componente: nenhuma
 * tela mostra número que não venha do banco, e onde não houver dado real que
 * sustente a métrica, a tela diz que não há dado **e o motivo**, no lugar de
 * mostrar algo plausível.
 *
 * `[V]` É a resposta direta ao defeito que este projeto existe para corrigir:
 * no legado, 17 meses de faturamento parado passaram por atual porque a tela
 * mostrava um número sem dizer de onde ele vinha. Um número plausível e errado
 * é pior do que um espaço em branco explicado — o primeiro leva a uma decisão,
 * o segundo leva a uma pergunta.
 *
 * SÃO DUAS ORIGENS, e elas são diferentes de propósito:
 *
 * 1. {@link MetricasSemDado} mostra o que **a API mediu nesta requisição**. O
 *    texto vem inteiro da resposta, com a contagem apurada na mesma consulta —
 *    a tela não reescreve nem resume. Quando o dado melhorar, o texto muda
 *    sozinho, sem release.
 * 2. {@link LacunaConhecida} mostra o que **não tem rota nenhuma para pedir**:
 *    faturamento, títulos e ordens de serviço estão parados na origem desde
 *    2024 e 2025, e a categoria de contato nunca veio. Aqui o texto é escrito,
 *    porque não há consulta que o produza — mas ele carrega a data e o número
 *    que o documento 23, seção 6.1, e o documento 25, seção 9, mediram.
 */

import type { MetricaSemDado } from '../../tipos/relacionamento';

/**
 * A lista `metricasSemDado` que todo agregado da API devolve.
 *
 * Não renderiza nada quando a lista vem vazia — e lista vazia é a resposta boa:
 * significa que tudo o que a tela mostra tem lastro.
 */
export function MetricasSemDado({
  metricas,
  titulo = 'O que estes números não dizem',
}: {
  metricas: MetricaSemDado[] | undefined;
  titulo?: string;
}) {
  if (!metricas || metricas.length === 0) return null;

  return (
    <div className="cad-semdado" role="note">
      <div className="cad-semdado-titulo">
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <circle cx="12" cy="12" r="9" />
          <path d="M12 8h.01" />
          <path d="M11 12h1v4h1" />
        </svg>
        {titulo}
      </div>
      <ul className="cad-semdado-lista">
        {metricas.map((m) => (
          <li key={m.metrica}>
            <code className="cad-semdado-nome">{m.metrica}</code>
            <span className="cad-semdado-motivo">{m.motivo}</span>
          </li>
        ))}
      </ul>
      <p className="cad-semdado-rodape">
        Medido pela API na mesma consulta que produziu os números acima. Quando o dado melhorar, este
        texto muda sozinho.
      </p>
    </div>
  );
}

/**
 * Uma lacuna que NÃO TEM ROTA para pedir — o dado não existe na origem, e por
 * isso não há agregado que possa declará-la.
 *
 * Fica no lugar onde o número apareceria, e não num rodapé: um bloco vazio sem
 * explicação é lido como "ainda não carregou", e é assim que se aprende a
 * ignorar o espaço em branco.
 */
export function LacunaConhecida({
  metrica,
  motivo,
  desde,
}: {
  /** O nome da métrica que a tela deixaria de mostrar. */
  metrica: string;
  /** Por que ela não existe, com o número ou a data que sustenta a afirmação. */
  motivo: string;
  /** Desde quando a origem parou, quando é o caso. */
  desde?: string;
}) {
  return (
    <div className="cad-lacuna" role="note">
      <div className="cad-lacuna-cabecalho">
        <span className="cad-lacuna-selo">sem dado</span>
        <span className="cad-lacuna-metrica">{metrica}</span>
        {desde && <span className="cad-lacuna-desde">parado desde {desde}</span>}
      </div>
      <p className="cad-lacuna-motivo">{motivo}</p>
    </div>
  );
}

/**
 * As três integrações mortas do legado, com a data em que cada uma parou.
 *
 * Medido em 04/09/2026 e guardado por teste em `PonteDeLeituraDoVortice`
 * (documento 23, seção 6.1). As tabelas RESPONDEM à consulta, têm centenas de
 * milhares de linhas e PARECEM disponíveis — é exatamente por isso que a tela
 * precisa dizer a data em vez de mostrar o último número que sobrou lá dentro.
 */
export const INTEGRACOES_PARADAS = [
  {
    metrica: 'Faturamento',
    desde: '11/04/2025',
    motivo:
      'A tabela de notas fiscais do ERP (EXT_NFS) parou de receber carga em 11/04/2025. Ela ' +
      'continua respondendo à consulta e continua cheia, e é por isso que o número que ela ' +
      'devolveria pareceria atual. A ponte do CRM não lê esta tabela, de propósito.',
  },
  {
    metrica: 'Títulos em aberto',
    desde: '05/2025',
    motivo:
      'Os títulos financeiros (EXT_Titulo) estão presos em staging desde maio de 2025 — nunca ' +
      'foram promovidos para a tabela que o CRM leria. Não há saldo, vencimento nem inadimplência ' +
      'com lastro para mostrar.',
  },
  {
    metrica: 'Ordens de serviço',
    desde: 'nunca promovidas',
    motivo:
      'As ordens de serviço (EXT_OS) nunca chegaram a ser promovidas da área de integração. A ' +
      'tabela existe e está vazia do ponto de vista de quem consulta.',
  },
  {
    metrica: 'Frota do ERP',
    desde: '24/05/2024',
    motivo:
      'A frota vinda do ERP (EXT_Veic) parou em 24/05/2024. O parque de máquinas que o CRM mostra ' +
      'vem de IV_ClientePropr, que é do CEN e continua sendo alterado diariamente — são duas ' +
      'fontes diferentes, e só uma está viva.',
  },
] as const;
