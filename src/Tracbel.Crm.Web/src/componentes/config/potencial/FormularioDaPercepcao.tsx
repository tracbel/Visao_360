/**
 * Uma vigência nova da percepção do gestor sobre um município (issue 71, D-P04). O limite não é deste
 * formulário: é o dos parâmetros gerais vigentes na data de início, e quem confere é a API.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarPercepcaoDoGestor } from '../../../dados/api/potencial';
import type { MunicipioDaAdrOpcao, NovaPercepcaoDoGestor } from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoSelecao, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { useEnvio } from './useEnvio';
import { itensDeSelecao, numero } from './vigencias';

const CAMPOS = ['municipioCodigoIbge', 'percentual', 'vigenteDesde', 'justificativa'] as const;

export function FormularioDaPercepcao({
  municipios,
  limiteDeHoje,
  hoje,
  aoGravar,
  aoCancelar,
}: {
  municipios: MunicipioDaAdrOpcao[];
  limiteDeHoje: number | null;
  hoje: string;
  aoGravar: (mensagem: string) => void;
  aoCancelar: () => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [valores, setValores] = useState<NovaPercepcaoDoGestor>({
    municipioCodigoIbge: '', percentual: '', vigenteDesde: hoje, justificativa: '',
  });
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(CAMPOS);

  function mudar(campo: keyof NovaPercepcaoDoGestor, valor: string) {
    setValores((v) => ({ ...v, [campo]: valor }));
    limparErro(campo);
  }

  async function gravar(evento: React.FormEvent) {
    evento.preventDefault();
    const gravada = await enviar(() => informarPercepcaoDoGestor(contexto, valores));
    if (gravada) aoGravar(`Percepção de ${gravada.municipioNome} registrada: ${gravada.percentual > 0 ? '+' : ''}${numero(gravada.percentual)}%.`);
  }

  const campo = (nome: keyof NovaPercepcaoDoGestor) => ({
    valor: valores[nome],
    aoMudar: (v: string) => mudar(nome, v),
    erro: erros[nome],
  });

  return (
    <form className="pot-formulario" onSubmit={gravar} noValidate>
      <div className="pot-formulario-titulo">Nova percepção sobre um município</div>
      {aviso && (
        <AvisoDoFormulario titulo={aviso.titulo}>
          <span>{aviso.texto}</span>
        </AvisoDoFormulario>
      )}
      <div className="form-grid cols-3">
        <CampoSelecao rotulo="Município da ADR" obrigatorio largo
          itens={itensDeSelecao(municipios.map((m) => ({ codigo: String(m.codigoIbge), descricao: m.nome })))}
          {...campo('municipioCodigoIbge')} />
        <CampoTexto rotulo="Ajuste (%)" obrigatorio {...campo('percentual')} exemplo="-2,5"
          ajuda={limiteDeHoje === null
            ? 'O limite é o dos parâmetros gerais vigentes na data de início.'
            : `Hoje, de −${numero(limiteDeHoje)}% a +${numero(limiteDeHoje)}%. Negativo reduz o potencial.`} />
        <CampoTexto rotulo="Vigente a partir de" tipo="date" obrigatorio {...campo('vigenteDesde')} ajuda="Hoje ou depois." />
        <CampoTextoLongo rotulo="Justificativa" obrigatorio largo {...campo('justificativa')}
          exemplo="Por que o município está acima ou abaixo do que os números mostram — geada, usina nova, concorrente…" />
      </div>
      <div className="pot-acoes">
        <button type="button" className="btn btn-secondary" onClick={aoCancelar} disabled={enviando}>
          Cancelar
        </button>
        <button type="submit" className="btn btn-primary" disabled={enviando}>
          {enviando ? 'Registrando…' : 'Registrar percepção'}
        </button>
      </div>
    </form>
  );
}
