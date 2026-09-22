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
const DIRETORIA = [...GERENCIA, 'Empresa.AlcanceEntreFiliais', PERMISSAO.auditoriaLer];
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

  it('a gerência vê o potencial, os usuários, as integrações e as fontes, nas três abas', () => {
    const tem = quemTem(GERENCIA);
    expect(secoesVisiveis(tem).map((s) => s.id)).toEqual(['conta', 'potencial', 'usuarios', 'integracoes', 'fontes']);
    expect(abasVisiveis(tem).map((a) => a.rotulo)).toEqual(['Minha conta', 'Comercial', 'Administração']);
  });

  it('a diretoria vê também a auditoria', () => {
    const tem = quemTem(DIRETORIA);
    expect(secoesVisiveis(tem).map((s) => s.id)).toEqual(['conta', 'potencial', 'usuarios', 'auditoria', 'integracoes', 'fontes']);
  });

  it('o administrador vê também os perfis', () => {
    const tem = quemTem(ADMINISTRADOR);
    expect(secoesVisiveis(tem).map((s) => s.id)).toEqual(['conta', 'potencial', 'usuarios', 'perfis', 'auditoria', 'integracoes', 'fontes']);
  });

  it('quem só pode alterar os parâmetros vê a aba Comercial e não a Administração', () => {
    const tem = quemTem([PERMISSAO.parametroDoPotencialAdministrar]);
    expect(abasVisiveis(tem).map((a) => a.id)).toEqual(['conta', 'comercial']);
  });
});
