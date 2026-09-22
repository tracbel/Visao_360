/**
 * Quem vê o quê em Configurações (issue 134). A matriz aprovada pelo Ricardo em 22/09/2026, conferida pela
 * permissão — que é o que a página usa —, com os conjuntos que cada perfil semeado dá.
 */

import { describe, expect, it } from 'vitest';
import { PERMISSAO } from '../../dados/api/permissoes';
import { abasVisiveis, secoesVisiveis } from './secoes';

// O que o perfil Padrão dá hoje: ler as telas (sem as integrações) e ler os parâmetros do potencial.
const PADRAO = ['Cliente.Ler', 'Cliente.Criar', 'Cliente.Editar', 'Territorio.Ler', PERMISSAO.parametroDoPotencialLer];
const GERENCIA = [...PADRAO, PERMISSAO.percepcaoDoGestorInformar, PERMISSAO.integracaoLer, PERMISSAO.usuarioLer];
const DIRETORIA = [...GERENCIA, 'Empresa.AlcanceEntreFiliais'];
const ADMINISTRADOR = [...DIRETORIA, PERMISSAO.parametroDoPotencialAdministrar, PERMISSAO.usuarioAdministrar, 'Perfil.Administrar'];

function quemTem(codigos: string[]) {
  const conjunto = new Set(codigos);
  return (codigo: string) => conjunto.has(codigo);
}

describe('seções de Configurações por permissão', () => {
  it('o usuário comum vê só a própria conta', () => {
    const tem = quemTem(PADRAO);
    expect(secoesVisiveis(tem).map((s) => s.id)).toEqual(['conta']);
    expect(abasVisiveis(tem).map((a) => a.id)).toEqual(['conta']);
  });

  it('ler os parâmetros do potencial não abre a seção: ela é de quem altera alguma coisa neles', () => {
    expect(secoesVisiveis(quemTem([PERMISSAO.parametroDoPotencialLer])).map((s) => s.id)).not.toContain('potencial');
  });

  it.each([
    ['a gerência', GERENCIA],
    ['a diretoria', DIRETORIA],
    ['o administrador', ADMINISTRADOR],
  ])('%s vê o potencial, os usuários, as integrações e as fontes, nas três abas', (_, codigos) => {
    const tem = quemTem(codigos);
    expect(secoesVisiveis(tem).map((s) => s.id)).toEqual(['conta', 'potencial', 'usuarios', 'integracoes', 'fontes']);
    expect(abasVisiveis(tem).map((a) => a.rotulo)).toEqual(['Minha conta', 'Comercial', 'Administração']);
  });

  it('quem só pode alterar os parâmetros vê a aba Comercial e não a Administração', () => {
    const tem = quemTem([PERMISSAO.parametroDoPotencialAdministrar]);
    expect(abasVisiveis(tem).map((a) => a.id)).toEqual(['conta', 'comercial']);
  });
});
