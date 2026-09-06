/**
 * O seletor de filial do cabeçalho — a fronteira de multiempresa na tela.
 *
 * TROCAR AQUI MUDA O QUE SE VÊ, e é o ponto. A filial não é um filtro: é a
 * fronteira que a API aplica em toda consulta, pelo filtro global do
 * `CrmDbContext`. O MESMO registro responde 200 numa filial e 404 na outra, na
 * leitura e na escrita (documento 23, seção 4.3) — e este seletor é o que
 * permite ver isso acontecendo em vez de acreditar.
 *
 * A LISTA SÃO AS 13 FILIAIS EM OPERAÇÃO, e vem do catálogo `EMPRESA`, não de uma
 * constante daqui. O banco tem 18 filiais; cinco entraram inativas porque o
 * legado as marca como ativas e o negócio não confirmou. Oferecer uma filial
 * onde ninguém trabalha seria o front inventando operação (documento 23, seção 7).
 *
 * SÓ APARECE NAS TELAS LIGADAS À API (`rotas.tsx`, campo `usaApi`). As telas que
 * ainda leem o JSON do protótipo não têm filial: mostrar o seletor nelas
 * prometeria um efeito que não existe.
 */

import { useCatalogos, itensDe } from '../../dados/api/catalogos';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { CATALOGO } from '../../tipos/api';

export function SeletorDeFilial() {
  const { contexto, trocarEmpresa } = useContextoDeAcesso();
  const { catalogos, carregando, erro } = useCatalogos(contexto);
  const filiais = itensDe(catalogos, CATALOGO.empresa);

  if (erro) {
    return (
      <span className="cad-filial cad-filial-erro" title={erro.message}>
        Filial {contexto.empresa} · lista indisponível
      </span>
    );
  }

  return (
    <label className="cad-filial">
      <span className="cad-filial-rotulo">Filial</span>
      <select
        value={contexto.empresa}
        disabled={carregando}
        onChange={(e) => trocarEmpresa(e.target.value)}
        aria-label="Filial do contexto de acesso"
      >
        {carregando && <option value={contexto.empresa}>Carregando…</option>}
        {!carregando && !filiais.some((f) => f.codigo === contexto.empresa) && (
          <option value={contexto.empresa}>{contexto.empresa} (fora da lista de operação)</option>
        )}
        {filiais.map((filial) => (
          <option key={filial.codigo} value={filial.codigo}>
            {filial.descricao}
          </option>
        ))}
      </select>
    </label>
  );
}
