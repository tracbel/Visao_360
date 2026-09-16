/**
 * Projeção da malha municipal para SVG — sem biblioteca de mapa.
 *
 * POR QUE NÃO LEAFLET NEM D3. O mapa não tem fundo de ruas, zoom de satélite
 * nem camada externa: são 645 polígonos pintados por valor. Uma projeção
 * equirretangular corrigida pelo cosseno da latitude média é exata o bastante
 * para um estado do tamanho de São Paulo (a distorção entre o norte e o sul do
 * estado fica abaixo de 3%), e evita trazer para o pacote um motor de mapa
 * inteiro — e um provedor de tiles que o CSP do servidor teria de liberar.
 */

export type Anel = [number, number][];

export type Geometria =
  | { type: 'Polygon'; coordinates: Anel[] }
  | { type: 'MultiPolygon'; coordinates: Anel[][] };

export type FeicaoMunicipal = {
  type: 'Feature';
  properties: { codarea: string; nome: string };
  geometry: Geometria;
};

export type ColecaoMunicipal = { type: 'FeatureCollection'; features: FeicaoMunicipal[] };

/** Como a malha cabe no quadro: a caixa geográfica e a escala. */
export type Enquadramento = {
  largura: number;
  altura: number;
  oeste: number;
  norte: number;
  escala: number;
  cossenoDaLatitude: number;
  margem: number;
};

function poligonosDe(geometria: Geometria): Anel[][] {
  return geometria.type === 'Polygon' ? [geometria.coordinates] : geometria.coordinates;
}

/**
 * Enquadra a coleção numa largura dada; a altura sai da proporção real.
 *
 * Com `incluir`, o quadro é a caixa só dos municípios escolhidos — os outros continuam desenhados, e o
 * que passa da borda é cortado pelo próprio SVG. É o que deixa a ADR ocupar o mapa inteiro sem perder
 * o contexto dos vizinhos. Se nenhum município passar no critério, vale o estado inteiro.
 */
export function enquadrar(
  colecao: ColecaoMunicipal,
  largura: number,
  margem = 8,
  incluir?: (codigo: number) => boolean,
): Enquadramento {
  let oeste = Infinity;
  let leste = -Infinity;
  let sul = Infinity;
  let norte = -Infinity;

  const escolhidas = incluir ? colecao.features.filter((f) => incluir(Number(f.properties.codarea))) : [];
  const feicoes = escolhidas.length > 0 ? escolhidas : colecao.features;

  for (const feicao of feicoes) {
    for (const poligono of poligonosDe(feicao.geometry)) {
      for (const anel of poligono) {
        for (const [lon, lat] of anel) {
          if (lon < oeste) oeste = lon;
          if (lon > leste) leste = lon;
          if (lat < sul) sul = lat;
          if (lat > norte) norte = lat;
        }
      }
    }
  }

  const cossenoDaLatitude = Math.cos((((norte + sul) / 2) * Math.PI) / 180);
  const larguraGeografica = (leste - oeste) * cossenoDaLatitude;
  const escala = (largura - margem * 2) / larguraGeografica;
  const altura = (norte - sul) * escala + margem * 2;

  return { largura, altura, oeste, norte, escala, cossenoDaLatitude, margem };
}

/** Longitude e latitude viram x e y no quadro. */
export function projetar(e: Enquadramento, lon: number, lat: number): [number, number] {
  return [
    e.margem + (lon - e.oeste) * e.cossenoDaLatitude * e.escala,
    e.margem + (e.norte - lat) * e.escala,
  ];
}

/** O atributo `d` de um `<path>`, com uma casa decimal — o suficiente a qualquer tamanho de tela. */
export function caminhoSvg(e: Enquadramento, geometria: Geometria): string {
  const partes: string[] = [];
  for (const poligono of poligonosDe(geometria)) {
    for (const anel of poligono) {
      anel.forEach(([lon, lat], i) => {
        const [x, y] = projetar(e, lon, lat);
        partes.push(`${i === 0 ? 'M' : 'L'}${x.toFixed(1)} ${y.toFixed(1)}`);
      });
      partes.push('Z');
    }
  }
  return partes.join('');
}
