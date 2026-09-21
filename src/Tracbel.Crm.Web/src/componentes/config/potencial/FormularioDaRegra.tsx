/**
 * Uma vigência nova da regra de uma cultura (issue 71, D-P01): "1 máquina de referência a cada N hectares,
 * renovada a cada M anos". O produto sai da PAM carregada — a regra divide a área dele —, em lista, sem
 * código digitado.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarRegraDePotencial } from '../../../dados/api/potencial';
import type { NovaRegraDePotencial, ProdutoDaPam, RegraDePotencialDetalhe } from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoSelecao, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { useEnvio } from './useEnvio';
import { itensDeSelecao, numero } from './vigencias';

const CAMPOS = [
  'produtoCodigoIbge', 'hectaresPorMaquina', 'anosDeRenovacao', 'modeloDeReferencia', 'situacao', 'vigenteDesde', 'justificativa',
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
  vigentes,
  hoje,
  aoGravar,
  aoCancelar,
}: {
  produtos: ProdutoDaPam[];
  vigentes: RegraDePotencialDetalhe[];
  hoje: string;
  aoGravar: (mensagem: string) => void;
  aoCancelar: () => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [valores, setValores] = useState<NovaRegraDePotencial>({
    produtoCodigoIbge: '', hectaresPorMaquina: '', anosDeRenovacao: '', modeloDeReferencia: '',
    situacao: 'AConfirmar', vigenteDesde: hoje, justificativa: '',
  });
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(CAMPOS);

  function mudar(campo: keyof NovaRegraDePotencial, valor: string) {
    setValores((v) => ({ ...v, [campo]: valor }));
    limparErro(campo);
  }

  /** Escolher um produto que já tem regra traz os valores de hoje — muda-se só o que mudou. */
  function escolherProduto(codigo: string) {
    const atual = vigentes.find((r) => String(r.produtoCodigoIbge) === codigo);
    setValores((v) => ({
      ...v,
      produtoCodigoIbge: codigo,
      ...(atual && {
        hectaresPorMaquina: texto(atual.hectaresPorMaquina),
        anosDeRenovacao: texto(atual.anosDeRenovacao),
        modeloDeReferencia: atual.modeloDeReferencia,
        situacao: atual.situacao,
      }),
    }));
    limparErro('produtoCodigoIbge');
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
        <CampoSelecao rotulo="Cultura (produto da PAM)" obrigatorio largo itens={itens}
          valor={valores.produtoCodigoIbge} aoMudar={escolherProduto} erro={erros.produtoCodigoIbge}
          ajuda="Os mais plantados na ADR primeiro. A regra divide a área plantada deste produto." />
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
