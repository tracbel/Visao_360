/**
 * A CARTEIRA POR VENDEDOR (issue 259): cada responsável de carteira comercial com os clientes dele — na região e fora, por
 * classe e por faixa de dias desde o último contato.
 *
 * CADA VENDEDOR CONTA O CLIENTE PELOS VÍNCULOS DELE: o mesmo cliente em carteiras de dois CENs aparece nas duas linhas,
 * cada uma com o último contato daquela carteira. Por isso a soma da coluna pode passar do total da carteira.
 */

import { useMemo, useState } from 'react';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { CarteiraDoResponsavelNoDimensionamento } from '../../tipos/mercado';
import { CabecalhoOrdenavel } from '../comum/CabecalhoOrdenavel';
import { ordenar, proximaOrdem, type Ordem } from '../comum/ordenacao';
import { dataCurta, n } from './dimensionamento';

type Coluna = 'nome' | 'clientes' | 'naRegiao' | 'fora' | 'a' | 'b' | 'c' | 'd' | 'semClasse' | 'ate30' | 'ate60' | 'ate90' | 'ate120' | 'ultimo';

function valorDe(r: CarteiraDoResponsavelNoDimensionamento, coluna: Coluna): number | string | null {
  switch (coluna) {
    case 'nome': return r.nome;
    case 'clientes': return r.clientes.clientes;
    case 'naRegiao': return r.naRegiao;
    case 'fora': return r.fora;
    case 'a':
    case 'b':
    case 'c':
    case 'd':
    case 'semClasse':
      return r.clientes[coluna];
    case 'ate30':
    case 'ate60':
    case 'ate90':
    case 'ate120':
      return r.clientes.faixas[coluna];
    case 'ultimo': return r.clientes.ultimoContatoEm;
  }
}

export function CarteiraPorVendedor({ vendedores }: { vendedores: readonly CarteiraDoResponsavelNoDimensionamento[] }) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'clientes', sentido: -1 });
  const linhas = useMemo(() => ordenar(vendedores, ordem, valorDe), [vendedores, ordem]);
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };

  if (vendedores.length === 0) return <p className="dim-nota">Nenhuma carteira comercial ao seu alcance tem cliente neste recorte.</p>;

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela diag-tabela dim-tabela">
        <caption className="cad-so-leitor">Os clientes de cada vendedor, por região, classe e faixa de dias desde o último contato</caption>
        <thead>
          <tr>
            <CabecalhoOrdenavel {...cab} coluna="nome" rotulo="Vendedor" texto />
            <CabecalhoOrdenavel {...cab} coluna="clientes" rotulo="Clientes" />
            <CabecalhoOrdenavel {...cab} coluna="naRegiao" rotulo="Na região" />
            <CabecalhoOrdenavel {...cab} coluna="fora" rotulo="Fora" />
            <CabecalhoOrdenavel {...cab} coluna="a" rotulo="A" />
            <CabecalhoOrdenavel {...cab} coluna="b" rotulo="B" />
            <CabecalhoOrdenavel {...cab} coluna="c" rotulo="C" />
            <CabecalhoOrdenavel {...cab} coluna="d" rotulo="D" />
            <CabecalhoOrdenavel {...cab} coluna="semClasse" rotulo="S/ classe" titulo="Sem classe: sem faturamento apurado" />
            <CabecalhoOrdenavel {...cab} coluna="ate30" rotulo="< 30 d" titulo="Contato nos últimos 30 dias" />
            <CabecalhoOrdenavel {...cab} coluna="ate60" rotulo="< 60 d" titulo="Contato nos últimos 60 dias" />
            <CabecalhoOrdenavel {...cab} coluna="ate90" rotulo="< 90 d" titulo="Contato nos últimos 90 dias" />
            <CabecalhoOrdenavel {...cab} coluna="ate120" rotulo="< 120 d" titulo="Contato nos últimos 120 dias" />
            <CabecalhoOrdenavel {...cab} coluna="ultimo" rotulo="Últ. contato" />
          </tr>
        </thead>
        <tbody>
          {linhas.map((r) => (
            <tr key={r.responsavelId}>
              <th scope="row">
                {nomeProprio(r.nome)}
                {r.natureza === 'Departamento' && <span className="dim-selo">área</span>}
              </th>
              <td className="mom-num dim-destaque">{n(r.clientes.clientes)}</td>
              <td className="mom-num">{n(r.naRegiao)}</td>
              <td className="mom-num">{n(r.fora)}</td>
              <td className="mom-num">{n(r.clientes.a)}</td>
              <td className="mom-num">{n(r.clientes.b)}</td>
              <td className="mom-num">{n(r.clientes.c)}</td>
              <td className="mom-num">{n(r.clientes.d)}</td>
              <td className="mom-num">{n(r.clientes.semClasse)}</td>
              <td className="mom-num">{n(r.clientes.faixas.ate30)}</td>
              <td className="mom-num">{n(r.clientes.faixas.ate60)}</td>
              <td className="mom-num">{n(r.clientes.faixas.ate90)}</td>
              <td className="mom-num">{n(r.clientes.faixas.ate120)}</td>
              <td className="mom-num">{dataCurta(r.clientes.ultimoContatoEm)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
