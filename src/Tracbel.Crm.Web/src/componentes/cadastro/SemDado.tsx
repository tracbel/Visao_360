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
 * 2. {@link LacunaConhecida} mostra o que **não tem rota nenhuma para pedir** —
 *    na ficha do cliente, hoje, os títulos em aberto e as ordens de serviço, que
 *    o CRM ainda não carrega do Protheus. Aqui o texto é escrito, porque não há
 *    consulta que o produza — e por isso ele precisa ser conferido no código
 *    quando o dado mudar: em 27/09/2026 três frases daqui eram falsas (o
 *    faturamento e a frota "parados", que estavam no banco havia dias).
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
  naDica = false,
}: {
  metricas: MetricaSemDado[] | undefined;
  titulo?: string;
  /**
   * Desenha a lista para morar DENTRO DE UMA DICA (fidelidade às maquetes,
   * 23/09/2026).
   *
   * Na Visão Diretoria esta informação é auditoria — nível 4 da hierarquia. A
   * T2.1 a recolheu num `<details>` de uma linha embaixo dos mapas; a maquete
   * não tem essa linha, e a decisão do usuário é que ela vá para a dica ao lado
   * do título "Visão geográfica". O texto é o mesmo, inteiro, com o rodapé que
   * diz de onde ele vem — só sem a caixa e sem o `<details>`, que dentro de um
   * balão seriam moldura em volta de moldura. O padrão continua aberto para as
   * telas que já dependiam dele.
   */
  naDica?: boolean;
}) {
  if (!metricas || metricas.length === 0) return null;

  if (naDica) {
    return (
      <>
        <p>
          <strong>{titulo}</strong>
        </p>
        <ul>
          {metricas.map((m) => (
            <li key={m.metrica}>
              <code>{m.metrica}</code>: {m.motivo}
            </li>
          ))}
        </ul>
        <p>
          Medido pela API na mesma consulta que produziu os números da tela. Quando o dado melhorar, este texto muda
          sozinho.
        </p>
      </>
    );
  }

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
 * O que a ficha do cliente ainda não mostra, com o motivo de HOJE — conferido no
 * código em 27/09/2026.
 *
 * ATÉ ESSA DATA ERAM QUATRO, e duas frases eram falsas. "O faturamento parou em
 * 11/04/2025" era a cópia que o Vórtice recebia (`EXT_NFS`): o faturamento do
 * Protheus estava no banco, de 09/2023 a 09/2026, e ganhou bloco próprio. "A
 * frota do ERP parou em 24/05/2024" era a `EXT_Veic` do Vórtice: o cadastro de
 * veículos do Protheus (VV1) é lido pela sincronia do parque, e a frota pelo dono
 * atual também ganhou bloco. As duas que ficaram não dependiam do Vórtice: o CRM
 * não lia nem guardava esses dados do Protheus.
 *
 * EM 02/10/2026 AS ORDENS DE SERVIÇO SAÍRAM DAQUI: a rotina 15 POS_VENDA_PROTHEUS as
 * traz das views do BI, e a ficha tem o bloco delas. Fica a de títulos em aberto.
 */
export const LACUNAS_DA_FICHA_DO_CLIENTE = [
  {
    metrica: 'Títulos em aberto',
    motivo:
      'O CRM ainda não lê os títulos do Protheus: não há carga, tabela nem rota do contas a ' +
      'receber (SE1). Saldo, vencimento e inadimplência ficam no ERP até essa leitura existir — ' +
      'pelo mesmo caminho do faturamento, direto do banco do Protheus.',
  },
] as const;
