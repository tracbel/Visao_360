/**
 * Uma vigência nova do share-alvo de uma categoria de máquina (issue 256) — a fatia da demanda que a Tracbel planeja
 * entregar. As categorias vêm do catálogo; a desligada não recebe vigência nova.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { informarShareAlvo } from '../../../dados/api/potencial';
import type { CategoriaNoCatalogo, NovoShareAlvo, ShareAlvoDetalhe } from '../../../tipos/potencial';
import { AvisoDoFormulario, CampoSelecao, CampoTexto, CampoTextoLongo } from '../../cadastro/CamposDeFormulario';
import { useEnvio } from './useEnvio';
import { itensDeSelecao, numero } from './vigencias';

const CAMPOS = ['categoriaDeMaquinaCodigo', 'percentual', 'vigenteDesde', 'justificativa'] as const;

export function FormularioDoShare({
  categorias,
  vigentes,
  hoje,
  aoGravar,
  aoCancelar,
}: {
  categorias: CategoriaNoCatalogo[];
  /** O share de hoje de cada categoria — escolher a categoria preenche o valor atual. */
  vigentes: ShareAlvoDetalhe[];
  hoje: string;
  aoGravar: (mensagem: string) => void;
  aoCancelar: () => void;
}) {
  const { contexto } = useContextoDeAcesso();
  const [valores, setValores] = useState<NovoShareAlvo>({
    categoriaDeMaquinaCodigo: '', percentual: '', vigenteDesde: hoje, justificativa: '',
  });
  const { enviando, erros, aviso, enviar, limparErro } = useEnvio(CAMPOS);

  function mudar(campo: keyof NovoShareAlvo, valor: string) {
    setValores((v) => {
      const novo = { ...v, [campo]: valor };
      // ESCOLHER A CATEGORIA TRAZ O SHARE DE HOJE: quem muda de 31% para 35% não precisa lembrar que era 31%.
      if (campo === 'categoriaDeMaquinaCodigo') {
        const atual = vigentes.find((s) => s.categoriaDeMaquinaCodigo === valor);
        novo.percentual = atual ? String(atual.percentual).replace('.', ',') : '';
      }
      return novo;
    });
    limparErro(campo);
  }

  async function gravar(evento: React.FormEvent) {
    evento.preventDefault();
    const gravado = await enviar(() => informarShareAlvo(contexto, valores));
    if (gravado) aoGravar(`Share-alvo de ${gravado.categoriaDeMaquinaNome} registrado: ${numero(gravado.percentual)}%.`);
  }

  const campo = (nome: keyof NovoShareAlvo) => ({
    valor: valores[nome],
    aoMudar: (v: string) => mudar(nome, v),
    erro: erros[nome],
  });

  return (
    <form className="pot-formulario" onSubmit={gravar} noValidate>
      <div className="pot-formulario-titulo">Novo share-alvo de uma categoria</div>
      {aviso && (
        <AvisoDoFormulario titulo={aviso.titulo}>
          <span>{aviso.texto}</span>
        </AvisoDoFormulario>
      )}
      <div className="form-grid cols-3">
        <CampoSelecao rotulo="Categoria de máquina" obrigatorio
          itens={itensDeSelecao(categorias.filter((c) => c.estaAtiva).map((c) => ({ codigo: c.codigo, descricao: c.nome })))}
          {...campo('categoriaDeMaquinaCodigo')} />
        <CampoTexto rotulo="Share-alvo (%)" obrigatorio exemplo="31" {...campo('percentual')}
          ajuda="Maior que 0 e até 100. Para a categoria não ter meta de planejamento, não registre share." />
        <CampoTexto rotulo="Vigente a partir de" tipo="date" obrigatorio {...campo('vigenteDesde')} ajuda="Hoje ou depois." />
        <CampoTextoLongo rotulo="Justificativa" obrigatorio largo {...campo('justificativa')}
          exemplo="A decisão por trás do número — meta da diretoria, histórico de participação…" />
      </div>
      <div className="pot-acoes">
        <button type="button" className="btn btn-secondary" onClick={aoCancelar} disabled={enviando}>
          Cancelar
        </button>
        <button type="submit" className="btn btn-primary" disabled={enviando}>
          {enviando ? 'Registrando…' : 'Registrar share-alvo'}
        </button>
      </div>
    </form>
  );
}
