/**
 * Uma vigência nova da regra de uma cultura (issue 71, D-P01): "1 máquina de referência a cada N hectares,
 * renovada a cada M anos". O produto sai da PAM carregada — a regra divide a área dele —, em lista, sem
 * código digitado.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarRegraDePotencial } from '../../../dados/api/potencial';
import type {
  CategoriaNoCatalogo,
  CulturaNoCatalogo,
  NovaRegraDePotencial,
  ProdutoDaPam,
  RegraDePotencialDetalhe,
} from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoSelecao, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { useEnvio } from './useEnvio';
import { itensDeSelecao, numero } from './vigencias';

const CAMPOS = [
  'produtoCodigoIbge', 'hectaresPorMaquina', 'anosDeRenovacao', 'modeloDeReferencia', 'situacao', 'vigenteDesde',
  'justificativa', 'culturaCodigo', 'categoriaDeMaquinaCodigo',
] as const;

const SITUACOES = itensDeSelecao([
  { codigo: 'AConfirmar', descricao: 'A confirmar — o potencial sai como estimativa' },
  { codigo: 'Confirmada', descricao: 'Confirmada pelo comercial' },
]);

function texto(valor: number | null | undefined): string {
  return valor === null || valor === undefined ? '' : String(valor).replace('.', ',');
}

export function FormularioDaRegra({
  produtos,
  culturas,
  categorias,
  vigentes,
  hoje,
  aoGravar,
  aoCancelar,
}: {
  produtos: ProdutoDaPam[];
  /** As culturas ATIVAS do catálogo (issue 165) — a regra nova pertence a uma delas. */
  culturas: CulturaNoCatalogo[];
  /** As categorias de máquina ATIVAS (D-IM-06) — de qual máquina esta regra fala. */
  categorias: CategoriaNoCatalogo[];
  vigentes: RegraDePotencialDetalhe[];
  hoje: string;
  aoGravar: (mensagem: string) => void;
  aoCancelar: () => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [valores, setValores] = useState<NovaRegraDePotencial>({
    produtoCodigoIbge: '', hectaresPorMaquina: '', anosDeRenovacao: '', modeloDeReferencia: '',
    situacao: 'AConfirmar', vigenteDesde: hoje, justificativa: '',
    culturaCodigo: '', categoriaDeMaquinaCodigo: '',
  });
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(CAMPOS);

  function mudar(campo: keyof NovaRegraDePotencial, valor: string) {
    setValores((v) => ({ ...v, [campo]: valor }));
    limparErro(campo);
  }

  /**
   * Trazer os valores da regra que já vale para o par escolhido — muda-se só o que mudou.
   *
   * O PAR É PRODUTO **E CATEGORIA** (D-P01): trocar de categoria com o mesmo produto tem de trazer a regra
   * daquela categoria, e não a do trator para todas. Sem isso, escolher "colheitadeira" no café herdaria
   * "1 a cada 10 ha" do trator, que é a regra errada com cara de sugestão.
   */
  function preencherComAVigente(produto: string, categoria: string) {
    const atual = vigentes.find(
      (r) => String(r.produtoCodigoIbge) === produto && (r.categoriaDeMaquinaCodigo ?? '') === categoria,
    );

    setValores((v) => ({
      ...v,
      produtoCodigoIbge: produto,
      categoriaDeMaquinaCodigo: categoria,
      ...(atual && {
        hectaresPorMaquina: texto(atual.hectaresPorMaquina),
        anosDeRenovacao: texto(atual.anosDeRenovacao),
        modeloDeReferencia: atual.modeloDeReferencia,
        situacao: atual.situacao,
        culturaCodigo: atual.culturaCodigo ?? v.culturaCodigo,
      }),
    }));
  }

  function escolherProduto(codigo: string) {
    preencherComAVigente(codigo, valores.categoriaDeMaquinaCodigo);
    limparErro('produtoCodigoIbge');
  }

  function escolherCategoria(codigo: string) {
    preencherComAVigente(valores.produtoCodigoIbge, codigo);
    limparErro('categoriaDeMaquinaCodigo');
  }

  async function gravar(evento: React.FormEvent) {
    evento.preventDefault();
    const gravada = await enviar(() => informarRegraDePotencial(contexto, valores));
    if (gravada) aoGravar(`Regra de ${gravada.produtoNome} registrada, vigente a partir de ${gravada.vigencia.vigenteDesde.split('-').reverse().join('/')}.`);
  }

  const campo = (nome: keyof NovaRegraDePotencial) => ({
    valor: valores[nome],
    aoMudar: (v: string) => mudar(nome, v),
    erro: erros[nome],
  });

  const itensDasCulturas = itensDeSelecao(
    culturas.filter((c) => c.estaAtiva).map((c) => ({ codigo: c.codigo, descricao: c.nome })),
  );

  const itensDasCategorias = itensDeSelecao(
    categorias.filter((c) => c.estaAtiva).map((c) => ({ codigo: c.codigo, descricao: c.nome })),
  );

  const itens = itensDeSelecao(produtos.map((p) => ({
    codigo: String(p.codigoIbge),
    descricao:
      p.areaPlantadaNaAdrHectares === null
        ? `${p.nome} — sem área na ADR (PAM ${p.ano})`
        : `${p.nome} — ${numero(p.areaPlantadaNaAdrHectares, 0)} ha na ADR (PAM ${p.ano})`,
  })));

  return (
    <form className="pot-formulario" onSubmit={gravar} noValidate>
      <div className="pot-formulario-titulo">Nova vigência da regra de uma cultura</div>
      {aviso && (
        <AvisoDoFormulario titulo={aviso.titulo}>
          <span>{aviso.texto}</span>
        </AvisoDoFormulario>
      )}
      <div className="form-grid cols-3">
        <CampoSelecao rotulo="Produto da PAM" obrigatorio largo itens={itens}
          valor={valores.produtoCodigoIbge} aoMudar={escolherProduto} erro={erros.produtoCodigoIbge}
          ajuda="Os mais plantados na ADR primeiro. A regra divide a área plantada deste produto." />
        {/* AS DUAS METADES QUE FALTAVAM DA D-P01 (issue 63). A decisão fixa CULTURA × CATEGORIA × hectares
            por máquina × anos de renovação, e até aqui a rota só aceitava as duas últimas: uma decisão
            tomada em reunião não cabia no sistema. */}
        <CampoSelecao rotulo="Cultura do catálogo" obrigatorio itens={itensDasCulturas}
          valor={valores.culturaCodigo} aoMudar={(v) => mudar('culturaCodigo', v)} erro={erros.culturaCodigo}
          ajuda="É ela que liga a regra ao preço e ao custo da cultura." />
        <CampoSelecao rotulo="Categoria de máquina" obrigatorio itens={itensDasCategorias}
          valor={valores.categoriaDeMaquinaCodigo} aoMudar={escolherCategoria} erro={erros.categoriaDeMaquinaCodigo}
          ajuda="De qual máquina esta regra fala. A mesma lavoura pede um trator a cada tantos hectares e uma colheitadeira a cada outros tantos." />
        <CampoTexto rotulo="Hectares por máquina" obrigatorio {...campo('hectaresPorMaquina')}
          ajuda="Quantos hectares pedem uma máquina de referência." />
        <CampoTexto rotulo="Anos de renovação" {...campo('anosDeRenovacao')}
          ajuda="A cada quantos anos se troca. Sem ele, a demanda anual não sai." />
        <CampoTexto rotulo="Modelo de referência" obrigatorio {...campo('modeloDeReferencia')} exemplo="3036N" />
        <CampoSelecao rotulo="Situação" obrigatorio itens={SITUACOES} {...campo('situacao')} />
        <CampoTexto rotulo="Vigente a partir de" tipo="date" obrigatorio {...campo('vigenteDesde')}
          ajuda="Hoje ou depois." />
        <CampoTextoLongo rotulo="Justificativa" obrigatorio largo {...campo('justificativa')}
          exemplo="De onde vêm estes números — a decisão do comercial, a medição de campo…" />
      </div>
      <div className="pot-acoes">
        <button type="button" className="btn btn-secondary" onClick={aoCancelar} disabled={enviando}>
          Cancelar
        </button>
        <button type="submit" className="btn btn-primary" disabled={enviando}>
          {enviando ? 'Registrando…' : 'Registrar vigência'}
        </button>
      </div>
    </form>
  );
}
