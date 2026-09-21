/**
 * O envio de um formulário de parâmetro, com a recusa da API desmontada no lugar certo: a mensagem de cada
 * campo no campo, o resto no aviso do topo (o mesmo tratamento do cadastro de cliente).
 */

import { useState } from 'react';
import { ErroDaApi } from '../../../dados/api/http';

export type AvisoDoEnvio = { titulo: string; texto: string } | null;

export function useEnvio(camposDaTela: readonly string[]) {
  const [enviando, setEnviando] = useState(false);
  const [erros, setErros] = useState<Record<string, string>>({});
  const [aviso, setAviso] = useState<AvisoDoEnvio>(null);

  /** Executa a gravação. Devolve o resultado, ou nulo quando a API recusou — e aí os erros já estão na tela. */
  async function enviar<T>(acao: () => Promise<T>): Promise<T | null> {
    setEnviando(true);
    setErros({});
    setAviso(null);
    try {
      return await acao();
    } catch (causa) {
      if (causa instanceof ErroDaApi) {
        setErros(causa.porCampo());
        const sobras = causa.errosForaDoFormulario(camposDaTela);
        setAviso({
          titulo: causa.message,
          texto:
            sobras.length > 0
              ? sobras.map((e) => e.mensagem).join(' · ')
              : causa.erros.length > 0
                ? 'A mensagem de cada campo está no próprio campo.'
                : (causa.detalhe ?? ''),
        });
      } else {
        setAviso({ titulo: 'Não foi possível gravar', texto: causa instanceof Error ? causa.message : String(causa) });
      }
      return null;
    } finally {
      setEnviando(false);
    }
  }

  /** O erro some quando a pessoa mexe no campo: manter um aviso sobre valor já corrigido faz a tela parecer travada. */
  function limparErro(campo: string) {
    setErros((atuais) => {
      if (!(campo in atuais)) return atuais;
      const resto = { ...atuais };
      delete resto[campo];
      return resto;
    });
  }

  return { enviando, erros, aviso, enviar, limparErro };
}
