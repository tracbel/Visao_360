/**
 * Cliente — a ficha, o cadastro, a alteração e a inativação, numa tela só.
 *
 * POR QUE UMA TELA E NÃO TRÊS: ver, editar e cadastrar são o MESMO conjunto de
 * campos com permissões diferentes de escrita. Separar em telas obriga a manter
 * três marcações em sincronia, e é exatamente a queixa de "telas demais
 * mostrando a mesma coisa". Aqui o que muda é o modo, não a tela.
 *
 * ONDE O ERRO DA API APARECE, que é metade da usabilidade do cadastro: a API
 * devolve `erros[]` com `campo`, `mensagem` e `valorRecebido`, e cada mensagem
 * vai para o SEU campo (`errosDeCampo[nome]`), com `aria-invalid` e
 * `aria-describedby`. O aviso do topo fica só para o que não pertence a campo
 * nenhum — conflito de concorrência, ou uma mensagem endereçada a um campo que
 * esta tela não mostra. Nenhuma recusa da API desaparece pelo caminho.
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';
import { AvisoDoFormulario, CampoSelecao, CampoSomenteLeitura, CampoTexto } from '../../componentes/cadastro/CamposDeFormulario';
import { DialogoConfirmacao } from '../../componentes/cadastro/DialogoConfirmacao';
import { BlocoCarregando, BlocoErro } from '../../componentes/cadastro/EstadosDeTela';
import { MaquinasCompradasDoCliente } from '../../componentes/cadastro/MaquinasCompradasDoCliente';
import { AvisoDeProcedencia, SeloProcedencia } from '../../componentes/cadastro/SeloProcedencia';
import { descricaoDe, itensDe, useCatalogos } from '../../dados/api/catalogos';
import { alterarCliente, criarCliente, inativarCliente, obterCliente } from '../../dados/api/clientes';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { ErroDaApi } from '../../dados/api/http';
import { useRecurso } from '../../dados/api/useRecurso';
import { CATALOGO, type ClienteDetalhe, type NovoCliente } from '../../tipos/api';
import { formatarDataHora } from './formato';

/** Os campos que ESTA tela mostra. O que a API recusar fora desta lista vai para o topo. */
const CAMPOS_DA_TELA = [
  'nomeRazao',
  'nomeFantasia',
  'tipoDePessoa',
  'documento',
  'inscricaoEstadual',
  'atividadeEconomica',
  'situacao',
  'origemCodigo',
] as const;

const FORMULARIO_VAZIO: NovoCliente = {
  nomeRazao: '',
  nomeFantasia: '',
  tipoDePessoa: '',
  documento: '',
  inscricaoEstadual: '',
  atividadeEconomica: '',
  situacao: '',
  origemCodigo: '',
};

/** Traduz a ficha lida no formulário editável. */
function doDetalhe(cliente: ClienteDetalhe): NovoCliente {
  return {
    nomeRazao: cliente.nomeRazao,
    nomeFantasia: cliente.nomeFantasia ?? '',
    tipoDePessoa: cliente.tipoDePessoa,
    documento: cliente.documento ?? '',
    inscricaoEstadual: cliente.inscricaoEstadual ?? '',
    atividadeEconomica: cliente.atividadeEconomica ?? '',
    situacao: cliente.situacao,
    origemCodigo: cliente.origemCodigo ?? '',
  };
}

type Modo = 'novo' | 'ficha' | 'editando';

export function ClienteCadastro() {
  const { chave } = useParams<{ chave: string }>();
  const ehNovo = chave === undefined;
  const navegar = useNavigate();
  const { contexto } = useContextoDeAcesso();
  const { catalogos, erro: erroDeCatalogo } = useCatalogos(contexto);

  const [modo, setModo] = useState<Modo>(ehNovo ? 'novo' : 'ficha');
  const [formulario, setFormulario] = useState<NovoCliente>(FORMULARIO_VAZIO);
  const [original, setOriginal] = useState<NovoCliente>(FORMULARIO_VAZIO);
  const [versao, setVersao] = useState<string | null>(null);
  const [errosDeCampo, setErrosDeCampo] = useState<Record<string, string>>({});
  const [avisoDoTopo, setAvisoDoTopo] = useState<{ titulo: string; texto: string; tom: 'erro' | 'atencao' } | null>(null);
  const [gravando, setGravando] = useState(false);
  const [confirmandoInativacao, setConfirmandoInativacao] = useState(false);
  const [confirmandoSaida, setConfirmandoSaida] = useState(false);
  const [motivoDeInativacao, setMotivoDeInativacao] = useState('');
  const local = useLocation();
  const recemCriado = (local.state as { recemCriado?: string } | null)?.recemCriado ?? null;
  const [recemGravado, setRecemGravado] = useState<string | null>(null);

  // TROCAR DE ROTA NÃO REMONTA A TELA. `/clientes/novo` e `/clientes/{chave}`
  // renderizam o MESMO componente, e o React reaproveita a instância: sem este
  // reinício, abrir "Novo cliente" a partir de uma ficha mostrava os campos do
  // cliente anterior, desabilitados. Foi um defeito real, encontrado
  // percorrendo a tela com o Playwright.
  useEffect(() => {
    setModo(ehNovo ? 'novo' : 'ficha');
    setFormulario(FORMULARIO_VAZIO);
    setOriginal(FORMULARIO_VAZIO);
    setVersao(null);
    setErrosDeCampo({});
    setAvisoDoTopo(null);
    setMotivoDeInativacao('');
    setRecemGravado(recemCriado ? `Cliente "${recemCriado}" cadastrado na filial do contexto.` : null);
  }, [chave, ehNovo, recemCriado]);

  const leitura = useRecurso<ClienteDetalhe | null>(
    (sinal) =>
      ehNovo
        ? Promise.resolve({ dados: null, procedencia: null })
        : obterCliente(contexto, chave, sinal),
    [contexto.empresa, contexto.usuario, chave ?? 'novo'],
  );

  // A leitura anterior fica na tela enquanto a próxima carrega — o que é bom numa
  // lista e errado numa ficha. Aqui a ficha de OUTRO cliente não serve para nada,
  // então ela não conta como "já tenho o dado".
  const cliente = leitura.dados && leitura.dados.chave === chave ? leitura.dados : null;

  // A ficha lida vira o formulário e a referência do "que mudou".
  //
  // O FORMULÁRIO SÓ É PREENCHIDO QUANDO O REGISTRO MUDA DE IDENTIDADE. Uma
  // releitura do MESMO cliente — a que acontece depois de gravar, ou quando a
  // versão do servidor é recarregada num conflito — atualiza a versão e a
  // referência de comparação, e não toca no que a pessoa digitou. É a regra de
  // nunca perder o que foi digitado, aplicada no ponto exato onde ela poderia
  // ser quebrada sem ninguém notar.
  const [chaveCarregada, setChaveCarregada] = useState<string | null>(null);
  useEffect(() => {
    if (!cliente) return;
    setVersao(cliente.versao);
    const valores = doDetalhe(cliente);
    setOriginal(valores);
    if (cliente.chave !== chaveCarregada) {
      setFormulario(valores);
      setChaveCarregada(cliente.chave);
    }
  }, [cliente, chaveCarregada]);

  const sujo = useMemo(
    () => (Object.keys(formulario) as (keyof NovoCliente)[]).some((campo) => formulario[campo] !== original[campo]),
    [formulario, original],
  );

  // Fechar a aba com alteração não gravada pede confirmação do navegador. É a
  // única proteção que funciona quando a saída não passa pelo nosso roteador.
  useEffect(() => {
    if (!sujo || modo === 'ficha') return;
    function aoSair(evento: BeforeUnloadEvent) {
      evento.preventDefault();
    }
    window.addEventListener('beforeunload', aoSair);
    return () => window.removeEventListener('beforeunload', aoSair);
  }, [sujo, modo]);

  const mudar = useCallback((campo: keyof NovoCliente, valor: string) => {
    setFormulario((f) => ({ ...f, [campo]: valor }));
    // A mensagem some assim que a pessoa mexe no campo: manter o erro embaixo de
    // um valor já corrigido faz o formulário parecer travado.
    setErrosDeCampo((e) => {
      if (!(campo in e)) return e;
      const resto = { ...e };
      delete resto[campo];
      return resto;
    });
  }, []);

  /** Desmonta a recusa da API: campo a campo nos campos, o resto no topo. */
  function tratarRecusa(causa: unknown) {
    if (causa instanceof ErroDaApi) {
      setErrosDeCampo(causa.porCampo());

      if (causa.ehConcorrencia) {
        setAvisoDoTopo({
          titulo: 'Outra pessoa alterou este cliente enquanto você editava',
          texto:
            'O que você digitou continua na tela e não foi perdido. Recarregue a versão do ' +
            'servidor para conferir o que mudou e grave de novo, ou descarte suas alterações.',
          tom: 'atencao',
        });
        return;
      }

      const sobras = causa.errosForaDoFormulario(CAMPOS_DA_TELA);
      setAvisoDoTopo(
        sobras.length > 0 || causa.erros.length === 0
          ? {
              titulo: causa.message,
              texto:
                sobras.length > 0
                  ? sobras.map((e) => `${e.campo}: ${e.mensagem}`).join(' · ')
                  : (causa.detalhe ?? ''),
              tom: 'erro',
            }
          : {
              titulo: causa.message,
              texto: `${causa.erros.length} ${causa.erros.length === 1 ? 'campo a corrigir' : 'campos a corrigir'} — a mensagem de cada um está no próprio campo.`,
              tom: 'erro',
            },
      );
      return;
    }

    setAvisoDoTopo({
      titulo: 'Não foi possível gravar',
      texto: causa instanceof Error ? causa.message : String(causa),
      tom: 'erro',
    });
  }

  async function gravar(evento: React.FormEvent) {
    evento.preventDefault();
    setGravando(true);
    setErrosDeCampo({});
    setAvisoDoTopo(null);
    setRecemGravado(null);

    try {
      if (ehNovo) {
        const criado = await criarCliente(contexto, formulario);
        navegar(`/clientes/${criado.chave}`, { replace: true, state: { recemCriado: criado.nomeRazao } });
        return;
      }
      const alterado = await alterarCliente(contexto, chave, { ...formulario, versao });
      const valores = doDetalhe(alterado);
      setFormulario(valores);
      setOriginal(valores);
      setVersao(alterado.versao);
      setModo('ficha');
      setRecemGravado('Alterações gravadas.');
      leitura.recarregar();
    } catch (causa) {
      tratarRecusa(causa);
    } finally {
      setGravando(false);
    }
  }

  async function recarregarDoServidor(descartando: boolean) {
    if (!chave) return;
    try {
      const resposta = await obterCliente(contexto, chave);
      setVersao(resposta.dados.versao);
      const valores = doDetalhe(resposta.dados);
      setOriginal(valores);
      if (descartando) {
        setFormulario(valores);
        setModo('ficha');
      }
      setAvisoDoTopo(
        descartando
          ? null
          : {
              titulo: 'Versão do servidor recarregada',
              texto:
                'O que você digitou continua na tela. Confira se ainda faz sentido diante do que ' +
                'o outro usuário gravou e clique em Gravar de novo.',
              tom: 'atencao',
            },
      );
    } catch (causa) {
      tratarRecusa(causa);
    }
  }

  async function confirmarInativacao() {
    if (!chave) return;
    setGravando(true);
    setErrosDeCampo({});
    try {
      await inativarCliente(contexto, chave, { motivoCodigo: motivoDeInativacao, versao });
      setConfirmandoInativacao(false);
      setRecemGravado('Cliente inativado. Nada foi apagado — o registro continua no banco e no histórico.');
      leitura.recarregar();
    } catch (causa) {
      setConfirmandoInativacao(false);
      tratarRecusa(causa);
    } finally {
      setGravando(false);
    }
  }

  function sairDaEdicao() {
    if (sujo) {
      setConfirmandoSaida(true);
      return;
    }
    if (ehNovo) {
      navegar('/clientes');
      return;
    }
    setModo('ficha');
    setAvisoDoTopo(null);
    setErrosDeCampo({});
  }

  const editando = modo === 'novo' || modo === 'editando';
  const motivos = itensDe(catalogos, CATALOGO.motivoInativacao);
  const motivoEscolhido = motivos.find((m) => m.codigo === motivoDeInativacao);

  if (!ehNovo && !cliente && !leitura.erro) {
    return (
      <div className="card cad-cartao">
        <BlocoCarregando oQue="a ficha do cliente" />
      </div>
    );
  }

  if (!ehNovo && leitura.erro) {
    return (
      <>
        <div className="page-header">
          <div>
            <h1 className="page-title">Cliente</h1>
          </div>
          <div className="page-actions">
            <Link to="/clientes" className="btn btn-secondary">
              Voltar para a lista
            </Link>
          </div>
        </div>
        <div className="card cad-cartao">
          <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />
        </div>
      </>
    );
  }

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">{ehNovo ? 'Novo cliente' : cliente?.nomeRazao}</h1>
          <p className="page-subtitle">
            {ehNovo ? (
              <>O cadastro nasce na filial <strong>{contexto.empresa}</strong> — a do cabeçalho, nunca do formulário</>
            ) : (
              <>
                {cliente?.nomeFantasia ? `${cliente.nomeFantasia} · ` : ''}
                {descricaoDe(catalogos, CATALOGO.situacaoCliente, cliente?.situacao ?? null)}
                {cliente?.estaInativo ? ' · inativado' : ''}
              </>
            )}
          </p>
        </div>
        <div className="page-actions">
          <Link to="/clientes" className="btn btn-secondary">
            Voltar para a lista
          </Link>
          {modo === 'ficha' && !cliente?.estaInativo && (
            <>
              <button type="button" className="btn btn-secondary" onClick={() => setConfirmandoInativacao(true)}>
                Inativar
              </button>
              <button type="button" className="btn btn-primary" onClick={() => setModo('editando')}>
                Editar
              </button>
            </>
          )}
        </div>
      </div>

      {recemGravado && <AvisoDoFormulario titulo={recemGravado} tom="sucesso" />}
      {erroDeCatalogo && (
        <AvisoDoFormulario titulo="Os catálogos não carregaram" tom="atencao">
          <p>
            Sem eles, os campos de seleção ficam vazios e o cadastro não tem como oferecer as opções
            válidas. {erroDeCatalogo.message}
          </p>
        </AvisoDoFormulario>
      )}
      {avisoDoTopo && (
        <AvisoDoFormulario titulo={avisoDoTopo.titulo} tom={avisoDoTopo.tom}>
          <p>{avisoDoTopo.texto}</p>
          {avisoDoTopo.tom === 'atencao' && !ehNovo && (
            <div className="cad-aviso-acoes">
              <button type="button" className="btn btn-secondary btn-sm" onClick={() => recarregarDoServidor(false)}>
                Recarregar a versão do servidor
              </button>
              <button type="button" className="btn btn-ghost btn-sm" onClick={() => recarregarDoServidor(true)}>
                Descartar minhas alterações
              </button>
            </div>
          )}
        </AvisoDoFormulario>
      )}
      <AvisoDeProcedencia procedencia={leitura.procedencia} />

      <form className="card cad-cartao" onSubmit={gravar} noValidate>
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Identificação</div>
            <div className="card-subtitle">
              {editando ? 'Campos com * são obrigatórios' : 'Somente leitura — use Editar para alterar'}
            </div>
          </div>
          <SeloProcedencia procedencia={leitura.procedencia} />
        </div>

        <div className="card-body">
          <div className="form-grid cad-grade">
            <CampoTexto
              rotulo="Razão social ou nome"
              obrigatorio
              largo
              valor={formulario.nomeRazao}
              aoMudar={(v) => mudar('nomeRazao', v)}
              erro={errosDeCampo.nomeRazao}
              desabilitado={!editando}
              exemplo="Fazenda Santa Clara Grãos Ltda"
            />
            <CampoTexto
              rotulo="Nome fantasia"
              valor={formulario.nomeFantasia}
              aoMudar={(v) => mudar('nomeFantasia', v)}
              erro={errosDeCampo.nomeFantasia}
              desabilitado={!editando}
            />
            <CampoSelecao
              rotulo="Tipo de pessoa"
              obrigatorio
              valor={formulario.tipoDePessoa}
              aoMudar={(v) => mudar('tipoDePessoa', v)}
              itens={itensDe(catalogos, CATALOGO.tipoDePessoa)}
              erro={errosDeCampo.tipoDePessoa}
              desabilitado={!editando}
              ajuda="Define o documento aceito: 11 dígitos na física, 14 na jurídica."
            />
            <CampoTexto
              rotulo="CPF ou CNPJ"
              valor={formulario.documento}
              aoMudar={(v) => mudar('documento', v)}
              erro={errosDeCampo.documento}
              desabilitado={!editando}
              exemplo="18.245.339/0001-13"
              ajuda="Com ou sem máscara. O dígito verificador é conferido pela API."
            />
            <CampoTexto
              rotulo="Inscrição estadual"
              valor={formulario.inscricaoEstadual}
              aoMudar={(v) => mudar('inscricaoEstadual', v)}
              erro={errosDeCampo.inscricaoEstadual}
              desabilitado={!editando}
            />
            <CampoTexto
              rotulo="Atividade econômica"
              valor={formulario.atividadeEconomica}
              aoMudar={(v) => mudar('atividadeEconomica', v)}
              erro={errosDeCampo.atividadeEconomica}
              desabilitado={!editando}
            />
            <CampoSelecao
              rotulo="Situação"
              valor={formulario.situacao}
              aoMudar={(v) => mudar('situacao', v)}
              itens={itensDe(catalogos, CATALOGO.situacaoCliente)}
              erro={errosDeCampo.situacao}
              desabilitado={!editando}
              vazio={ehNovo ? 'Prospect (padrão da API)' : 'Selecione…'}
              ajuda="Domínio fechado no código e no banco: acrescentar item exige release e migração."
            />
            <CampoSelecao
              rotulo="Origem do lead"
              valor={formulario.origemCodigo}
              aoMudar={(v) => mudar('origemCodigo', v)}
              itens={itensDe(catalogos, CATALOGO.origemLead)}
              erro={errosDeCampo.origemCodigo}
              desabilitado={!editando}
              ajuda="Catálogo de banco: o negócio acrescenta item sem release."
            />
          </div>
        </div>

        {editando && (
          <div className="card-body cad-rodape-form">
            <button type="button" className="btn btn-secondary" onClick={sairDaEdicao} disabled={gravando}>
              Cancelar
            </button>
            <button type="submit" className="btn btn-primary" disabled={gravando}>
              {gravando ? 'Gravando…' : ehNovo ? 'Cadastrar cliente' : 'Gravar alterações'}
            </button>
          </div>
        )}
      </form>

      {!ehNovo && cliente && <MaquinasCompradasDoCliente contexto={contexto} chaveDoCliente={cliente.chave} />}

      {!ehNovo && cliente && (
        <div className="card cad-cartao">
          <div className="card-header">
            <div className="card-title">Registro</div>
            <div className="card-subtitle">O que o CRM sabe sobre o próprio cadastro</div>
          </div>
          <div className="card-body">
            <div className="form-grid cad-grade">
              <CampoSomenteLeitura rotulo="Chave pública" valor={<code>{cliente.chave}</code>} largo
                ajuda="É o GUID que a API usa. O identificador sequencial nunca sai da API — quem enumera /clientes/1, /clientes/2 conta quantos clientes a Tracbel tem." />
              <CampoSomenteLeitura
                rotulo="Filial dona do cadastro"
                valor={`${descricaoDe(catalogos, CATALOGO.empresa, contexto.empresa)} · id interno ${cliente.empresaId}`}
                ajuda="A fronteira de acesso, e é sempre a filial do cabeçalho: o que outra filial enxergasse não teria chegado até aqui — responderia 404." />
              <CampoSomenteLeitura
                rotulo="Proprietário"
                valor={`Usuário ${cliente.proprietarioId}`}
                ajuda="A API devolve o identificador, não o nome — não há endpoint de usuário ainda. Reatribuir dono é outra operação, com outra permissão (dívida D-6)." />
              <CampoSomenteLeitura rotulo="Cadastrado em" valor={formatarDataHora(cliente.criadoEm)} />
              <CampoSomenteLeitura rotulo="Última alteração" valor={formatarDataHora(cliente.alteradoEm)} />
              <CampoSomenteLeitura rotulo="Nesta situação desde" valor={formatarDataHora(cliente.situacaoDesde)} />
              <CampoSomenteLeitura
                rotulo="Documento sem máscara"
                valor={cliente.documentoSemMascara ?? '—'}
                ajuda="É o que a integração usa."
              />
              {cliente.estaInativo && (
                <CampoSomenteLeitura
                  rotulo="Motivo da inativação"
                  valor={descricaoDe(catalogos, CATALOGO.motivoInativacao, cliente.motivoInativacaoCodigo)}
                  largo
                  ajuda="Exclusão lógica: a linha continua no banco e o histórico continua apontando para ela."
                />
              )}
            </div>
          </div>
        </div>
      )}

      {confirmandoInativacao && (
        <DialogoConfirmacao
          titulo="Inativar este cliente?"
          subtitulo={cliente?.nomeRazao}
          rotuloConfirmar="Inativar cliente"
          podeConfirmar={motivoDeInativacao !== ''}
          gravando={gravando}
          aoCancelar={() => setConfirmandoInativacao(false)}
          aoConfirmar={confirmarInativacao}
        >
          <p>
            A inativação é <strong>exclusão lógica</strong>: nada é apagado. O cliente sai da lista
            padrão e continua visível com o filtro <em>Mostrar inativados</em>, e o histórico continua
            apontando para ele.
          </p>
          <div className="form-grid">
            <CampoSelecao
              rotulo="Motivo"
              obrigatorio
              largo
              valor={motivoDeInativacao}
              aoMudar={setMotivoDeInativacao}
              itens={motivos}
              erro={errosDeCampo.motivoCodigo}
              ajuda="O motivo é obrigatório e vem do catálogo MOTIVO_INATIVACAO."
            />
          </div>
          {motivoEscolhido?.exigeObservacao && (
            <p className="cad-nota">
              Este motivo pede observação, e o contrato do <code>DELETE</code> ainda não tem campo
              para ela — é uma dívida conhecida do cadastro. Escolha um motivo específico
              quando houver um.
            </p>
          )}
        </DialogoConfirmacao>
      )}

      {confirmandoSaida && (
        <DialogoConfirmacao
          titulo="Descartar o que você digitou?"
          subtitulo="Há alterações que ainda não foram gravadas"
          rotuloConfirmar="Descartar alterações"
          aoCancelar={() => setConfirmandoSaida(false)}
          aoConfirmar={() => {
            setConfirmandoSaida(false);
            setErrosDeCampo({});
            setAvisoDoTopo(null);
            if (ehNovo) {
              navegar('/clientes');
              return;
            }
            setFormulario(original);
            setModo('ficha');
          }}
        >
          <p>O formulário volta ao que estava gravado. Não há como desfazer depois.</p>
        </DialogoConfirmacao>
      )}
    </>
  );
}
