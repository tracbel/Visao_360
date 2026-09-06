/**
 * Os catálogos que alimentam os campos de seleção.
 *
 * A REGRA QUE ISTO TORNA EXEQUÍVEL: campo com catálogo não aceita digitação
 * livre (documento 05-MELHORIAS-COMBINADAS §2). A tela só consegue cumprir a
 * parte dela se tiver de onde puxar as opções — e é `/api/v1/catalogos`. Sem
 * isso, a alternativa real é o front embutir listas próprias, que foi como o
 * legado chegou a ter FINALIZADO e FINALIZADA no mesmo catálogo.
 *
 * A LISTA É LIDA UMA VEZ POR SESSÃO. Catálogo muda por decisão de negócio, não
 * a cada tela aberta, e são dezesseis listas de uma vez — reler a cada
 * formulário aberto seria desperdício visível no tempo de abrir a tela.
 */

import { useEffect, useState } from 'react';
import type { CatalogoDeSelecao, ComProcedencia, ItemDeSelecao } from '../../tipos/api';
import { ler, type ContextoDeAcesso } from './http';

/** Os catálogos indexados pelo código. */
export type MapaDeCatalogos = Record<string, CatalogoDeSelecao>;

let promessaEmAndamento: Promise<MapaDeCatalogos> | null = null;
let guardados: MapaDeCatalogos | null = null;

/** Busca todos os catálogos, reaproveitando a leitura já feita nesta sessão. */
export function carregarCatalogos(contexto: ContextoDeAcesso): Promise<MapaDeCatalogos> {
  if (guardados) return Promise.resolve(guardados);
  promessaEmAndamento ??= ler<CatalogoDeSelecao[]>('/v1/catalogos', contexto)
    .then((resposta: ComProcedencia<CatalogoDeSelecao[]>) => {
      const mapa: MapaDeCatalogos = {};
      for (const catalogo of resposta.dados) mapa[catalogo.codigo] = catalogo;
      guardados = mapa;
      return mapa;
    })
    .catch((causa: unknown) => {
      // Sem isto, uma falha de rede na primeira carga deixaria a promessa
      // rejeitada guardada para sempre e nenhuma tentativa seguinte funcionaria.
      promessaEmAndamento = null;
      throw causa;
    });
  return promessaEmAndamento;
}

/** Estado de leitura dos catálogos, no mesmo formato dos outros recursos. */
export type LeituraDeCatalogos = {
  catalogos: MapaDeCatalogos | null;
  carregando: boolean;
  erro: Error | null;
};

/** Hook de leitura dos catálogos. */
export function useCatalogos(contexto: ContextoDeAcesso): LeituraDeCatalogos {
  const [estado, setEstado] = useState<LeituraDeCatalogos>(() =>
    guardados ? { catalogos: guardados, carregando: false, erro: null } : { catalogos: null, carregando: true, erro: null },
  );

  useEffect(() => {
    if (guardados) return;
    let ativo = true;
    setEstado({ catalogos: null, carregando: true, erro: null });
    carregarCatalogos(contexto)
      .then((mapa) => ativo && setEstado({ catalogos: mapa, carregando: false, erro: null }))
      .catch((causa: unknown) => {
        if (!ativo) return;
        setEstado({
          catalogos: null,
          carregando: false,
          erro: causa instanceof Error ? causa : new Error(String(causa)),
        });
      });
    return () => {
      ativo = false;
    };
  }, [contexto]);

  return estado;
}

/** Os itens de um catálogo, na ordem em que a tela deve oferecê-los. */
export function itensDe(catalogos: MapaDeCatalogos | null, codigo: string): ItemDeSelecao[] {
  const itens = catalogos?.[codigo]?.itens ?? [];
  return [...itens].sort((a, b) => a.ordem - b.ordem || a.descricao.localeCompare(b.descricao, 'pt-BR'));
}

/**
 * A descrição de um código — inclusive de item aposentado, que continua sendo
 * exibido em registro antigo. Devolve o próprio código quando não encontra, para
 * nunca esconder do usuário o que está gravado.
 */
export function descricaoDe(catalogos: MapaDeCatalogos | null, codigo: string, valor: string | null): string {
  if (!valor) return '—';
  return catalogos?.[codigo]?.itens.find((i) => i.codigo === valor)?.descricao ?? valor;
}

/* ---------------------------------------------------------------------- */
/* Modelo de equipamento: marca, família e modelo                          */
/* ---------------------------------------------------------------------- */

/** Um modelo do catálogo de frota, com a marca e a família separadas. */
export type ModeloDeFrota = {
  codigo: string;
  marca: string;
  familia: string;
  modelo: string;
  /** O rótulo inteiro, como a API o compôs. */
  descricao: string;
};

/** O separador que `RepositorioDeCatalogos` usa para compor `Marca · Família · Modelo`. */
const SEPARADOR = ' · ';

/**
 * Desmonta a descrição do catálogo `MODELO_EQUIPAMENTO` em marca, família e modelo.
 *
 * POR QUE DESMONTAR EM VEZ DE PEDIR TRÊS LISTAS: a API compõe a descrição do
 * modelo como `Marca · Família · Modelo` (`RepositorioDeCatalogos.ListarAsync`),
 * e não publica catálogo separado de marca nem de família. Desmontar aqui é o
 * que permite a tela oferecer os três campos encadeados sem inventar listas
 * próprias — que é justamente o que a regra do §2 proíbe.
 *
 * Quando a descrição não tem as três partes, o modelo entra com marca e família
 * vazias em vez de sumir: item de catálogo que a tela não entende continua
 * selecionável, senão o cadastro fica impossível por causa de um rótulo.
 */
export function modelosDeFrota(catalogos: MapaDeCatalogos | null): ModeloDeFrota[] {
  return itensDe(catalogos, 'MODELO_EQUIPAMENTO').map((item) => {
    const partes = item.descricao.split(SEPARADOR).map((p) => p.trim());
    const [marca, familia, modelo] = partes.length === 3 ? partes : ['', '', item.descricao];
    return { codigo: item.codigo, marca, familia, modelo, descricao: item.descricao };
  });
}

/** Os valores distintos de um atributo dos modelos, em ordem alfabética. */
export function distintosDe(modelos: ModeloDeFrota[], campo: 'marca' | 'familia'): string[] {
  return [...new Set(modelos.map((m) => m[campo]).filter((v) => v !== ''))].sort((a, b) =>
    a.localeCompare(b, 'pt-BR'),
  );
}
