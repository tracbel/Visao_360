/**
 * Uma vigência nova dos parâmetros gerais (issue 71). Vem preenchido com o que vale hoje: mudar um valor é
 * registrar o conjunto de novo com aquele valor trocado — a API guarda a vigência anterior inteira.
 *
 * AS BANDAS DE PORTE (issue 166) têm um botão: o critério foi decidido em 27/09/2026 — os tercis da demanda anual
 * dos municípios da ADR —, e a API calcula os dois números. O botão só PREENCHE: quem registra é o administrador,
 * que vê os números antes, e eles continuam editáveis.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarParametrosGerais, sugerirBandasDePorte } from '../../../dados/api/potencial';
import type { NovoParametroDoPotencial, ParametrosGeraisDetalhe } from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { useEnvio } from './useEnvio';
import { formularioDosGerais, numero } from './vigencias';

const CAMPOS = [
  'vigenteDesde', 'mesesDaJanela', 'pesoDosContratosNoCredito', 'limiteDeRetracao', 'limiteDeAquecimento',
  'limiteDeSuperaquecimento', 'nomeDaFaixaIntermediaria', 'limiteDaPercepcao', 'pesoDoIndicadorDePreco',
  'pesoDoIndicadorDeCredito', 'pesoDoIndicadorComercial', 'fatorMinimo', 'fatorMaximo', 'mesesDeCarenciaDoSicor',
  'minimoDeLinhasNoCredito', 'porteMedioAPartirDe', 'porteGrandeAPartirDe', 'justificativa',
] as const;

type Calculo =
  | { estado: 'parado' }
  | { estado: 'calculando' }
  | { estado: 'pronto' | 'semCorte' | 'falhou'; texto: string };

/** O número como o campo o mostra: vírgula decimal. */
const comoCampo = (valor: number) => String(valor).replace('.', ',');

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
  const [calculo, setCalculo] = useState<Calculo>({ estado: 'parado' });
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

  async function calcularPelosTercis() {
    setCalculo({ estado: 'calculando' });
    try {
      const { dados } = await sugerirBandasDePorte(contexto);
      if (dados.porteMedioAPartirDe === null || dados.porteGrandeAPartirDe === null) {
        setCalculo({ estado: 'semCorte', texto: dados.motivo ?? 'Os tercis não saíram.' });
        return;
      }

      const medio = dados.porteMedioAPartirDe;
      const grande = dados.porteGrandeAPartirDe;
      // A JUSTIFICATIVA SÓ É PREENCHIDA SE ESTIVER VAZIA: o que o administrador já escreveu é dele.
      setValores((v) => ({
        ...v,
        porteMedioAPartirDe: comoCampo(medio),
        porteGrandeAPartirDe: comoCampo(grande),
        justificativa: v.justificativa.trim() ? v.justificativa : (dados.justificativa ?? ''),
      }));
      limparErro('porteMedioAPartirDe');
      limparErro('porteGrandeAPartirDe');
      setCalculo({
        estado: 'pronto',
        texto:
          `${dados.municipiosNaConta} de ${dados.municipiosDaAdr} municípios da ADR na conta` +
          (dados.anoDaAreaPlantada ? ` (área plantada de ${dados.anoDaAreaPlantada})` : '') +
          `: médio a partir de ${numero(medio, 1)} e grande a partir de ${numero(grande, 1)} máquinas por ano. ` +
          'Confira e registre a vigência — nada foi gravado ainda.',
      });
    } catch (causa) {
      setCalculo({ estado: 'falhou', texto: causa instanceof Error ? causa.message : String(causa) });
    }
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
        <CampoTexto rotulo="Mínimo de linhas do SICOR" {...campo('minimoDeLinhasNoCredito')}
          ajuda="Em aberto (D-P03): abaixo dele o índice de crédito sai marcado como base pequena. Vazio = nada é marcado." />
        <CampoTexto rotulo="Porte médio a partir de (máq/ano)" {...campo('porteMedioAPartirDe')}
          ajuda="Demanda anual de um município. Junto com a de grande (issue 166)." />
        <CampoTexto rotulo="Porte grande a partir de (máq/ano)" {...campo('porteGrandeAPartirDe')}
          ajuda="Acima da de médio." />
        <div className="pot-tercis">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={calcularPelosTercis}
            disabled={enviando || calculo.estado === 'calculando'}
          >
            {calculo.estado === 'calculando' ? 'Calculando…' : 'Calcular pelos tercis'}
          </button>
          <span className="pot-sub" role="status" data-estado={calculo.estado}>
            {calculo.estado === 'parado' || calculo.estado === 'calculando'
              ? 'Os tercis da demanda anual dos municípios da ADR, um terço em cada porte — o critério decidido em 27/09/2026.'
              : calculo.texto}
          </span>
        </div>
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
