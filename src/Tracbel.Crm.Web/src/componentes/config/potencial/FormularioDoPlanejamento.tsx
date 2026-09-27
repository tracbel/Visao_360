/**
 * Uma vigência nova da sazonalidade e dos pesos do IOC (issue 256). Vem o conjunto inteiro, já preenchido com o que
 * vale hoje. A soma dos meses aparece enquanto se digita — a API recusa o que não fecha em 100%, e a tela avisa antes.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarParametroDoPlanejamento } from '../../../dados/api/potencial';
import type { NovoParametroDoPlanejamento, ParametroDoPlanejamentoDetalhe } from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { COMPONENTES_DO_IOC, formularioDoPlanejamento, MESES, somaDaSazonalidade } from './planejamento';
import { useEnvio } from './useEnvio';
import { numero } from './vigencias';

const CAMPOS = [
  'vigenteDesde',
  'justificativa',
  'sazonalidade',
  ...MESES.map((_, i) => `sazonalidade[${i}]`),
  ...COMPONENTES_DO_IOC.map((c) => c.campo),
];

export function FormularioDoPlanejamento({
  vigente,
  hoje,
  aoGravar,
  aoCancelar,
}: {
  vigente: ParametroDoPlanejamentoDetalhe | null;
  hoje: string;
  aoGravar: (mensagem: string) => void;
  aoCancelar: () => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [valores, setValores] = useState<NovoParametroDoPlanejamento>(() => formularioDoPlanejamento(vigente, hoje));
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(CAMPOS);

  function mudarMes(indice: number, valor: string) {
    setValores((v) => ({ ...v, sazonalidade: v.sazonalidade.map((m, i) => (i === indice ? valor : m)) }));
    limparErro(`sazonalidade[${indice}]`);
    limparErro('sazonalidade');
  }

  function mudar(campo: Exclude<keyof NovoParametroDoPlanejamento, 'sazonalidade'>, valor: string) {
    setValores((v) => ({ ...v, [campo]: valor }));
    limparErro(campo);
  }

  async function gravar(evento: React.FormEvent) {
    evento.preventDefault();
    const gravado = await enviar(() => informarParametroDoPlanejamento(contexto, valores));
    if (gravado) aoGravar('Sazonalidade e pesos do IOC registrados.');
  }

  const soma = somaDaSazonalidade(valores.sazonalidade);
  const fecha = Math.abs(soma - 100) <= 0.05;

  return (
    <form className="pot-formulario" onSubmit={gravar} noValidate>
      <div className="pot-formulario-titulo">Nova vigência da sazonalidade e dos pesos do IOC</div>
      {aviso && (
        <AvisoDoFormulario titulo={aviso.titulo}>
          <span>{aviso.texto}</span>
        </AvisoDoFormulario>
      )}

      <fieldset className="pot-conjunto">
        <legend>Sazonalidade — % da demanda do ano em cada mês</legend>
        <div className="pot-grade-meses">
          {MESES.map((mes, i) => (
            <CampoTexto
              key={mes}
              rotulo={mes}
              valor={valores.sazonalidade[i] ?? ''}
              aoMudar={(v) => mudarMes(i, v)}
              erro={erros[`sazonalidade[${i}]`]}
            />
          ))}
        </div>
        <div className={`pot-soma${fecha ? '' : ' pot-soma-errada'}`} role="status" aria-live="polite">
          Soma: <strong>{numero(soma)}%</strong>
          {fecha ? ' — fecha o ano.' : ' — precisa dar 100%: a sazonalidade reparte a demanda do ano, não a aumenta.'}
        </div>
        {erros.sazonalidade && (
          <span className="cad-erro-campo" role="alert">
            {erros.sazonalidade}
          </span>
        )}
      </fieldset>

      <fieldset className="pot-conjunto">
        <legend>Pesos do IOC — o quanto cada componente conta (normalizados pela soma)</legend>
        <div className="pot-grade-pesos">
          {COMPONENTES_DO_IOC.map((c) => (
            <CampoTexto
              key={c.chave}
              rotulo={c.rotulo}
              valor={valores[c.campo] as string}
              aoMudar={(v) => mudar(c.campo as Exclude<keyof NovoParametroDoPlanejamento, 'sazonalidade'>, v)}
              erro={erros[c.campo]}
              ajuda={c.mede}
            />
          ))}
        </div>
      </fieldset>

      <div className="form-grid">
        <CampoTexto rotulo="Vigente a partir de" tipo="date" obrigatorio valor={valores.vigenteDesde}
          aoMudar={(v) => mudar('vigenteDesde', v)} erro={erros.vigenteDesde} ajuda="Hoje ou depois." />
        <CampoTextoLongo rotulo="Justificativa" obrigatorio largo valor={valores.justificativa}
          aoMudar={(v) => mudar('justificativa', v)} erro={erros.justificativa}
          exemplo="De onde vêm estes números — a média de vendas dos últimos anos, a reunião do comercial…" />
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
