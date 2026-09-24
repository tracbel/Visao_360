/**
 * O seletor de filial do cabeçalho — a fronteira de multiempresa na tela.
 *
 * TROCAR AQUI MUDA O QUE SE VÊ, e é o ponto. A filial não é um filtro: é a
 * fronteira que a API aplica em toda consulta, pelo filtro global do
 * `CrmDbContext`. O MESMO registro responde 200 numa filial e 404 na outra, na
 * leitura e na escrita (documento 23, seção 4.3).
 *
 * A LISTA SÃO AS FILIAIS DE QUEM ESTÁ USANDO (P-20, 21/09/2026): a de casa e as
 * que têm perfil concedido. Até a fase 3 a lista eram as 13 filiais em operação,
 * e qualquer pessoa escolhia qualquer uma. Ela vem da rota de escopo efetivo, e
 * não do catálogo `EMPRESA`.
 *
 * FILIAL GUARDADA QUE DEIXOU DE SER PERMITIDA volta sozinha para a de casa: a
 * rota de escopo responde pela filial de casa e diz qual foi recusada, e este
 * seletor troca — em vez de deixar toda tela presa num 403.
 *
 * "TODAS AS FILIAIS" É DE QUEM ADMINISTRA O CRM (21/09/2026): as 16 filiais são
 * todas raiz, então nenhuma filial escolhida cobre as outras. A opção só aparece
 * quando a rota de escopo diz que a pessoa pode, e a API registra no log cada
 * requisição feita nela. Cadastro novo não nasce aí: a API pede uma filial.
 *
 * SÓ APARECE NAS TELAS LIGADAS À API (`rotas.tsx`, campo `usaApi`).
 */

import { ChevronDown, Map as IconeMapa } from 'lucide-react';
import { useEffect, useState } from 'react';
import { obterEscopo, TODAS_AS_FILIAIS } from '../../dados/api/acesso';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';

export function SeletorDeFilial() {
  const { contexto, trocarEmpresa } = useContextoDeAcesso();
  const escopo = useRecurso((sinal) => obterEscopo(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [recusada, setRecusada] = useState<string | null>(null);
  const [dadosVistos, setDadosVistos] = useState(escopo.dados);

  const dados = escopo.dados;

  // O AVISO É AJUSTADO NA RENDERIZAÇÃO, quando chega um escopo novo — o padrão do React para estado que
  // depende de dado que mudou, sem efeito encadeando renderizações. Ele fica até a próxima troca manual.
  if (dados !== dadosVistos) {
    setDadosVistos(dados);
    if (dados?.filialPedidaRecusada) setRecusada(dados.filialPedidaRecusada);
  }

  // A TROCA DE FILIAL É SINCRONIZAÇÃO COM O PROVEDOR DE CONTEXTO, e por isso fica no efeito: a filial
  // recusada volta para a de casa, uma vez.
  useEffect(() => {
    if (dados?.filialPedidaRecusada && dados.filialAtual.codigo) trocarEmpresa(dados.filialAtual.codigo);
  }, [dados, trocarEmpresa]);

  if (escopo.erro) {
    return (
      <span className="cad-filial cad-filial-erro" title={escopo.erro.message}>
        Filial {contexto.empresa} · escopo indisponível
      </span>
    );
  }

  const filiais = dados?.filiaisPermitidas ?? [];
  const podeVerTodas = dados?.podeVerTodasAsFiliais ?? false;
  const nomeDaRecusada = recusada === TODAS_AS_FILIAIS ? 'Todas as filiais' : `a filial ${recusada}`;

  // O VISUAL É O DA MAQUETE (23/09/2026): ícone de mapa, "Filial" em cinza, o
  // nome em negrito e a seta. O controle continua o `select` nativo — teclado,
  // leitor de tela e a lista do sistema vêm de graça; só a seta é desenhada.
  // Sem escolha (uma filial só, ou carregando) a seta some: ela prometeria uma
  // lista que não abre.
  return (
    <label className="cad-filial" title={recusada ? `${nomeDaRecusada} não está entre as suas escolhas; voltamos para a de casa.` : undefined}>
      <IconeMapa className="cad-filial-icone" size={16} strokeWidth={1.75} aria-hidden="true" />
      <span className="cad-filial-rotulo">Filial</span>
      <span className="cad-filial-campo">
        <select
          value={contexto.empresa}
          disabled={escopo.carregando || (filiais.length <= 1 && !podeVerTodas)}
          onChange={(e) => {
            setRecusada(null);
            trocarEmpresa(e.target.value);
          }}
          aria-label="Filial do contexto de acesso"
        >
          {escopo.carregando && <option value={contexto.empresa}>Carregando…</option>}
          {podeVerTodas && <option value={TODAS_AS_FILIAIS}>Todas as filiais</option>}
          {filiais.map((filial) => (
            <option key={filial.codigo} value={filial.codigo}>
              {filial.nome}
              {/* O MARCADOR DE CASA SÓ SERVE QUANDO HÁ ESCOLHA — com uma filial só, ele apenas cortaria o nome. */}
              {filial.ehCasa && filiais.length > 1 ? ' (casa)' : ''}
            </option>
          ))}
        </select>
        <ChevronDown className="cad-filial-seta" size={16} aria-hidden="true" />
      </span>
      {recusada && <span className="cad-filial-aviso">{nomeDaRecusada} não está entre as suas escolhas</span>}
    </label>
  );
}
