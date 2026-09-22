/**
 * Uma vigência nova dos parâmetros gerais (issue 71). Vem preenchido com o que vale hoje: mudar um valor é
 * registrar o conjunto de novo com aquele valor trocado — a API guarda a vigência anterior inteira.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarParametrosGerais } from '../../../dados/api/potencial';
import type { NovoParametroDoPotencial, ParametrosGeraisDetalhe } from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { useEnvio } from './useEnvio';
import { formularioDosGerais } from './vigencias';

const CAMPOS = [
  'vigenteDesde', 'mesesDaJanela', 'pesoDosContratosNoCredito', 'limiteDeRetracao', 'limiteDeAquecimento',
  'limiteDeSuperaquecimento', 'nomeDaFaixaIntermediaria', 'limiteDaPercepcao', 'pesoDoIndicadorDePreco',
  'pesoDoIndicadorDeCredito', 'pesoDoIndicadorComercial', 'fatorMinimo', 'fatorMaximo', 'mesesDeCarenciaDoSicor', 'justificativa',
] as const;

export function FormularioDosGerais({
  vigente,
  hoje,
  aoGravar,
  aoCancelar,
}: {
  vigente: ParametrosGeraisDetalhe | null;
  hoje: string;
  aoGravar: (mensagem: string) => void;
  aoCancelar: () => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [valores, setValores] = useState<NovoParametroDoPotencial>(() => formularioDosGerais(vigente, hoje));
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(CAMPOS);

  function mudar(campo: keyof NovoParametroDoPotencial, valor: string) {
    setValores((v) => ({ ...v, [campo]: valor }));
    limparErro(campo);
  }

  async function gravar(evento: React.FormEvent) {
    evento.preventDefault();
    const gravado = await enviar(() => informarParametrosGerais(contexto, valores));
    if (gravado) aoGravar(`Parâmetros gerais registrados, vigentes a partir de ${gravado.vigencia.vigenteDesde.split('-').reverse().join('/')}.`);
  }

  const campo = (nome: keyof NovoParametroDoPotencial) => ({
    valor: valores[nome],
    aoMudar: (v: string) => mudar(nome, v),
    erro: erros[nome],
  });

  return (
    <form className="pot-formulario" onSubmit={gravar} noValidate>
      <div className="pot-formulario-titulo">Nova vigência dos parâmetros gerais</div>
      {aviso && (
        <AvisoDoFormulario titulo={aviso.titulo}>
          <span>{aviso.texto}</span>
        </AvisoDoFormulario>
      )}
      <div className="form-grid cols-3">
        <CampoTexto rotulo="Vigente a partir de" tipo="date" obrigatorio {...campo('vigenteDesde')}
          ajuda="Hoje ou depois. O que já valeu num dia que passou não muda." />
        <CampoTexto rotulo="Meses de cada lado do índice" obrigatorio {...campo('mesesDaJanela')}
          ajuda="12 = os últimos 12 meses contra os 12 anteriores." />
        <CampoTexto rotulo="Peso dos contratos no crédito" obrigatorio {...campo('pesoDosContratosNoCredito')}
          ajuda="De 0 a 1. 0,70 = 70% contratos e 30% valor." />
        <CampoTexto rotulo="Limite de retração" obrigatorio {...campo('limiteDeRetracao')}
          ajuda="Abaixo dele, o mercado está retraído." />
        <CampoTexto rotulo="Limite de aquecimento" obrigatorio {...campo('limiteDeAquecimento')}
          ajuda="Acima dele, aquecido." />
        <CampoTexto rotulo="Limite de superaquecimento" obrigatorio {...campo('limiteDeSuperaquecimento')}
          ajuda="Acima dele, superaquecido." />
        <CampoTexto rotulo="Nome da faixa intermediária" {...campo('nomeDaFaixaIntermediaria')}
          ajuda="Em aberto (D-P02): o texto diz “= 1 anual”." />
        <CampoTexto rotulo="Limite da percepção do gestor (%)" obrigatorio {...campo('limiteDaPercepcao')}
          ajuda="5 = de −5% a +5% por município." />
        <CampoTexto rotulo="Peso do indicador de preço" {...campo('pesoDoIndicadorDePreco')}
          ajuda="Em aberto (D-P05)." />
        <CampoTexto rotulo="Peso do indicador de crédito" {...campo('pesoDoIndicadorDeCredito')}
          ajuda="Em aberto (D-P05)." />
        <CampoTexto rotulo="Peso do indicador comercial" {...campo('pesoDoIndicadorComercial')}
          ajuda="Em aberto (D-P05)." />
        <CampoTexto rotulo="Fator mínimo" {...campo('fatorMinimo')}
          ajuda="Junto com o máximo: entre 0 e 1." />
        <CampoTexto rotulo="Fator máximo" {...campo('fatorMaximo')}
          ajuda="Acima de 1." />
        <CampoTexto rotulo="Carência do SICOR (meses)" {...campo('mesesDeCarenciaDoSicor')}
          ajuda="Em aberto (D-IM-03): quantos meses recentes ficam de fora da janela, porque o Banco Central ainda acrescenta contrato com atraso. Vazio = nenhum mês descartado." />
        <CampoTextoLongo rotulo="Justificativa" obrigatorio largo {...campo('justificativa')}
          exemplo="A decisão ou a fonte destes valores — fica na trilha, com o seu nome." />
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
