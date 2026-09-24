/**
 * O ÍCONE DE CADA CULTURA (fidelidade às maquetes, 23/09/2026).
 *
 * A maquete põe um ícone colorido antes do nome da cultura em todo ranking e
 * toda tabela do Momento. Ele é ENDEREÇO, e não informação: o nome está escrito
 * ao lado, e por isso o ícone é `aria-hidden`. Serve para o olho achar "a linha
 * do café" nas cinco abas sem ler.
 *
 * O ÍCONE SAI DO NOME, e não de uma lista de códigos: o catálogo de culturas
 * cresce pelo Administrador (issue 165), e cultura nova não pode quebrar a
 * tela — ela ganha a folha genérica até alguém escolher um ícone para ela.
 * Todos são do `lucide-react`, a mesma família do resto da aplicação.
 */

import {
  Banana,
  Bean,
  Beef,
  Carrot,
  Cherry,
  Citrus,
  Coffee,
  Flower2,
  Grape,
  Leaf,
  Milk,
  Nut,
  Sprout,
  TreePine,
  Wheat,
  type LucideIcon,
} from 'lucide-react';

type Estilo = { icone: LucideIcon; cor: string };

/** A primeira regra que casa vence; o nome é comparado sem acento e em minúsculas. */
const REGRAS: readonly [RegExp, Estilo][] = [
  [/cana/, { icone: Sprout, cor: '#2E7D32' }],
  [/cafe/, { icone: Coffee, cor: '#7B4A26' }],
  [/soja/, { icone: Bean, cor: '#B8860B' }],
  [/milho|sorgo|trigo|arroz|aveia|cevada/, { icone: Wheat, cor: '#C99A06' }],
  [/laranja|limao|tangerina|citr/, { icone: Citrus, cor: '#EA7A12' }],
  [/amendoim|castanha|noz/, { icone: Nut, cor: '#B45F2B' }],
  [/feijao|ervilha|grao/, { icone: Bean, cor: '#8B5A2B' }],
  [/algodao|girassol|flor/, { icone: Flower2, cor: '#7C6BB0' }],
  [/banana/, { icone: Banana, cor: '#C99A06' }],
  [/uva/, { icone: Grape, cor: '#6D28D9' }],
  [/tomate|manga|cereja|goiaba|morango/, { icone: Cherry, cor: '#DC2626' }],
  [/batata|mandioca|cenoura/, { icone: Carrot, cor: '#EA580C' }],
  [/eucalipto|pinus|silvicultura|madeira/, { icone: TreePine, cor: '#2E7D32' }],
  [/boi|bovino|pecuaria|gado/, { icone: Beef, cor: '#9A3412' }],
  [/leite/, { icone: Milk, cor: '#2563EB' }],
];

const PADRAO: Estilo = { icone: Leaf, cor: '#367C2B' };

function semAcento(texto: string): string {
  return texto.normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase();
}

function estiloDaCultura(nome: string): Estilo {
  const chave = semAcento(nome);
  return REGRAS.find(([regra]) => regra.test(chave))?.[1] ?? PADRAO;
}

export function IconeDaCultura({ nome, tamanho = 16 }: { nome: string; tamanho?: number }) {
  const { icone: Icone, cor } = estiloDaCultura(nome);
  return (
    <span className="mom-icone-cultura" style={{ color: cor }} aria-hidden="true">
      <Icone size={tamanho} strokeWidth={2} />
    </span>
  );
}

/** O nome da cultura com o ícone na frente — a célula da primeira coluna de toda tabela do Momento. */
export function NomeDaCultura({ nome }: { nome: string }) {
  return (
    <span className="mom-cultura">
      <IconeDaCultura nome={nome} />
      <span>{nome}</span>
    </span>
  );
}
