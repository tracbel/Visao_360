/**
 * Equipamento — a ficha, o cadastro, a alteração e a baixa, numa tela só.
 *
 * O MESMO PADRÃO DA TELA DE CLIENTE, de propósito: um modo por vez (ficha,
 * editando, novo), erro da API caindo no campo, aviso do topo só para o que não
 * é de campo, confirmação para o que é destrutivo. É o padrão que
 * `docs/prototipo/07-PADRAO-DE-TELA.md` descreve, e é ele que o passo 3 vai
 * aplicar nas outras telas.
 *
 * O QUE ESTA TELA TEM DE DIFERENTE, e vem do domínio:
 *
 * - **Chassi e origem só existem no cadastro.** Na alteração eles aparecem como
 *   somente leitura, com a razão escrita ao lado: o chassi é a identidade da
 *   máquina e a chave de deduplicação; a origem diz QUEM afirma que a máquina
 *   existe — o ERP, o CEN ou uma venda do ART.
 * - **Marca, família e modelo são três seleções encadeadas** montadas do
 *   catálogo `MODELO_EQUIPAMENTO`, cuja descrição a API compõe como
 *   `Marca · Família · Modelo`. O que vai para a API é só o código do modelo.
 * - **A classificação de produto** (trator pequeno, médio, grande…) é outro
 *   catálogo, e é opcional (documento 35, seção 10).
 * - **O dono é escolhido por busca**, nunca digitado. O COMPRADOR de uma venda
 *   não é o dono: ele aparece no histórico comercial, com a data da venda.
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom';
import {
  AvisoDoFormulario,
  CampoSelecao,
  CampoSomenteLeitura,
  CampoTexto,
} from '../../componentes/cadastro/CamposDeFormulario';
import { DialogoConfirmacao } from '../../componentes/cadastro/DialogoConfirmacao';
import { BlocoCarregando, BlocoErro } from '../../componentes/cadastro/EstadosDeTela';
import { SeletorDeCliente } from '../../componentes/cadastro/SeletorDeCliente';
import { AvisoDeProcedencia, SeloProcedencia } from '../../componentes/cadastro/SeloProcedencia';
import { descricaoDe, distintosDe, itensDe, modelosDeFrota, useCatalogos } from '../../dados/api/catalogos';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import {
  alterarEquipamento,
  criarEquipamento,
  inativarEquipamento,
  listarVendasDoEquipamento,
  obterEquipamento,
} from '../../dados/api/equipamentos';
import { ErroDaApi } from '../../dados/api/http';
import { useRecurso } from '../../dados/api/useRecurso';
import {
  CATALOGO,
  type EquipamentoDetalhe,
  type ItemDeSelecao,
  type NovoEquipamento,
  type VendaDaMaquina,
} from '../../tipos/api';
import { formatarDataHora, formatarNumero } from './formato';
import '../../estilos/frota-comercial.css';

/** Os campos que ESTA tela mostra. O que a API recusar fora disto vai para o topo. */
const CAMPOS_DA_TELA = [
  'chassi',
  'modeloCodigo',
  'linhaDeProdutoCodigo',
  'clienteChave',
  'situacao',
  'origem',
  'anoFabricacao',
  'anoModelo',
  'numeroSerie',
  'placa',
  'localizacaoDescrita',
] as const;

const FORMULARIO_VAZIO: NovoEquipamento = {
  chassi: '',
  modeloCodigo: '',
  clienteChave: '',
  situacao: '',
  origem: '',
  anoFabricacao: '',
  anoModelo: '',
  numeroSerie: '',
  placa: '',
  localizacaoDescrita: '',
  linhaDeProdutoCodigo: '',
};

function doDetalhe(maquina: EquipamentoDetalhe): NovoEquipamento {
  return {
    chassi: maquina.chassi,
    modeloCodigo: maquina.modeloCodigo ?? '',
    clienteChave: maquina.clienteChave ?? '',
    situacao: maquina.situacao,
    origem: maquina.origem,
    anoFabricacao: maquina.anoFabricacao?.toString() ?? '',
    anoModelo: maquina.anoModelo?.toString() ?? '',
    numeroSerie: maquina.numeroSerie ?? '',
    placa: maquina.placa ?? '',
    localizacaoDescrita: maquina.localizacaoDescrita ?? '',
    linhaDeProdutoCodigo: maquina.classificacaoCodigo ?? '',
  };
}

function dataCurta(valor: string | null): string {
  if (!valor) return '—';
  const [ano, mes, dia] = valor.slice(0, 10).split('-');
  return `${dia}/${mes}/${ano}`;
}

type Modo = 'novo' | 'ficha' | 'editando';

export function EquipamentoCadastro() {
  const { chave } = useParams<{ chave: string }>();
  const ehNovo = chave === undefined;
  const navegar = useNavigate();
  const local = useLocation();
  const { contexto } = useContextoDeAcesso();
  const { catalogos, erro: erroDeCatalogo } = useCatalogos(contexto);

  const [modo, setModo] = useState<Modo>(ehNovo ? 'novo' : 'ficha');
  const [formulario, setFormulario] = useState<NovoEquipamento>(FORMULARIO_VAZIO);
  const [original, setOriginal] = useState<NovoEquipamento>(FORMULARIO_VAZIO);
  const [nomeDoCliente, setNomeDoCliente] = useState('');
  const [marca, setMarca] = useState('');
  const [familia, setFamilia] = useState('');
  const [versao, setVersao] = useState<string | null>(null);
  const [errosDeCampo, setErrosDeCampo] = useState<Record<string, string>>({});
  const [avisoDoTopo, setAvisoDoTopo] = useState<{ titulo: string; texto: string; tom: 'erro' | 'atencao' } | null>(null);
  const [gravando, setGravando] = useState(false);
  const [confirmandoBaixa, setConfirmandoBaixa] = useState(false);
  const [confirmandoSaida, setConfirmandoSaida] = useState(false);

  const recemCriado = (local.state as { recemCriado?: string } | null)?.recemCriado ?? null;
  const [recemGravado, setRecemGravado] = useState<string | null>(null);

  // Mesma razão da tela de cliente: trocar de rota não remonta o componente, e
  // sem este reinício o formulário de "Novo equipamento" abria com a máquina
  // anterior nos campos.
  useEffect(() => {
    setModo(ehNovo ? 'novo' : 'ficha');
    setFormulario(FORMULARIO_VAZIO);
    setOriginal(FORMULARIO_VAZIO);
    setNomeDoCliente('');
    setMarca('');
    setFamilia('');
    setVersao(null);
    setErrosDeCampo({});
    setAvisoDoTopo(null);
    setRecemGravado(recemCriado ? `Equipamento ${recemCriado} cadastrado na filial do contexto.` : null);
  }, [chave, ehNovo, recemCriado]);

  const leitura = useRecurso<EquipamentoDetalhe | null>(
    (sinal) => (ehNovo ? Promise.resolve({ dados: null, procedencia: null }) : obterEquipamento(contexto, chave, sinal)),
    [contexto.empresa, contexto.usuario, chave ?? 'novo'],
  );

  const historico = useRecurso<VendaDaMaquina[]>(
    (sinal) => (ehNovo ? Promise.resolve({ dados: [], procedencia: null }) : listarVendasDoEquipamento(contexto, chave, sinal)),
    [contexto.empresa, contexto.usuario, chave ?? 'novo'],
  );

  const maquina = leitura.dados && leitura.dados.chave === chave ? leitura.dados : null;

  const modelos = useMemo(() => modelosDeFrota(catalogos), [catalogos]);
  const porCodigo = useMemo(() => new Map(modelos.map((m) => [m.codigo, m])), [modelos]);

  // Mesma regra da tela de cliente: o formulário só é preenchido quando a
  // máquina muda de identidade. Releitura do mesmo registro não sobrescreve o
  // que a pessoa digitou.
  const [chaveCarregada, setChaveCarregada] = useState<string | null>(null);
  useEffect(() => {
    if (!maquina) return;
    setVersao(maquina.versao);
    const valores = doDetalhe(maquina);
    setOriginal(valores);
    if (maquina.chave === chaveCarregada) return;
    setChaveCarregada(maquina.chave);
    setFormulario(valores);
    setNomeDoCliente(maquina.clienteNome ?? '');
    setMarca(maquina.marca ?? '');
    setFamilia(maquina.familia ?? '');
  }, [maquina, chaveCarregada]);

  const sujo = useMemo(
    () =>
      (Object.keys(formulario) as (keyof NovoEquipamento)[]).some(
        (campo) => formulario[campo] !== original[campo],
      ),
    [formulario, original],
  );

  useEffect(() => {
    if (!sujo || modo === 'ficha') return;
    function aoSair(evento: BeforeUnloadEvent) {
      evento.preventDefault();
    }
    window.addEventListener('beforeunload', aoSair);
    return () => window.removeEventListener('beforeunload', aoSair);
  }, [sujo, modo]);

  const mudar = useCallback((campo: keyof NovoEquipamento, valor: string) => {
    setFormulario((f) => ({ ...f, [campo]: valor }));
    setErrosDeCampo((e) => {
      if (!(campo in e)) return e;
      const resto = { ...e };
      delete resto[campo];
      return resto;
    });
  }, []);

  function tratarRecusa(causa: unknown) {
    if (causa instanceof ErroDaApi) {
      setErrosDeCampo(causa.porCampo());

      if (causa.ehConcorrencia) {
        setAvisoDoTopo({
          titulo: 'Outra pessoa alterou esta máquina enquanto você editava',
          texto:
            'O que você digitou continua na tela e não foi perdido. Recarregue a versão do ' +
            'servidor para conferir o que mudou e grave de novo, ou descarte suas alterações.',
          tom: 'atencao',
        });
        return;
      }

      const sobras = causa.errosForaDoFormulario(CAMPOS_DA_TELA);
      setAvisoDoTopo({
        titulo: causa.message,
        texto:
          sobras.length > 0
            ? sobras.map((e) => `${e.campo}: ${e.mensagem}`).join(' · ')
            : causa.erros.length > 0
              ? `${causa.erros.length} ${causa.erros.length === 1 ? 'campo a corrigir' : 'campos a corrigir'} — a mensagem de cada um está no próprio campo.`
              : (causa.detalhe ?? ''),
        tom: 'erro',
      });
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
        const criado = await criarEquipamento(contexto, formulario);
        navegar(`/equipamentos/${criado.chave}`, { replace: true, state: { recemCriado: criado.chassi } });
        return;
      }
      // Chassi e origem NÃO viajam na alteração: o contrato do PUT não os tem.
      const { chassi: _chassi, origem: _origem, ...alteraveis } = formulario;
      const alterado = await alterarEquipamento(contexto, chave, { ...alteraveis, versao });
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
      const resposta = await obterEquipamento(contexto, chave);
      setVersao(resposta.dados.versao);
      const valores = doDetalhe(resposta.dados);
      setOriginal(valores);
      if (descartando) {
        setFormulario(valores);
        setNomeDoCliente(resposta.dados.clienteNome ?? '');
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

  async function confirmarBaixa() {
    if (!chave) return;
    setGravando(true);
    try {
      await inativarEquipamento(contexto, chave, { versao });
      setConfirmandoBaixa(false);
      setRecemGravado('Máquina baixada. A linha e o histórico de horímetro continuam no banco.');
      leitura.recarregar();
    } catch (causa) {
      setConfirmandoBaixa(false);
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
      navegar('/equipamentos');
      return;
    }
    setModo('ficha');
    setAvisoDoTopo(null);
    setErrosDeCampo({});
  }

  const editando = modo === 'novo' || modo === 'editando';

  const marcas = useMemo(() => distintosDe(modelos, 'marca'), [modelos]);
  const familias = useMemo(
    () => distintosDe(marca ? modelos.filter((m) => m.marca === marca) : modelos, 'familia'),
    [modelos, marca],
  );
  const modelosOferecidos = useMemo(
    () => modelos.filter((m) => (!marca || m.marca === marca) && (!familia || m.familia === familia)),
    [modelos, marca, familia],
  );

  const comoItens = (valores: string[]): ItemDeSelecao[] =>
    valores.map((v, i) => ({ codigo: v, descricao: v, ordem: i + 1, exigeObservacao: false }));

  const modeloEscolhido = formulario.modeloCodigo ? porCodigo.get(formulario.modeloCodigo) : undefined;

  // A máquina do ART sem correspondência segura de produto pode continuar sem modelo — ninguém precisa
  // escolher um modelo parecido para confirmar o dono ou classificar.
  const modeloPendenteDoArt = maquina?.origem === 'Art' && !maquina.modeloCodigo;

  if (!ehNovo && !maquina && !leitura.erro) {
    return (
      <div className="card cad-cartao">
        <BlocoCarregando oQue="a ficha do equipamento" />
      </div>
    );
  }

  if (!ehNovo && leitura.erro) {
    return (
      <>
        <div className="page-header">
          <div>
            <h1 className="page-title">Equipamento</h1>
          </div>
          <div className="page-actions">
            <Link to="/equipamentos" className="btn btn-secondary">
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
          <h1 className="page-title">{ehNovo ? 'Novo equipamento' : maquina?.chassi}</h1>
          <p className="page-subtitle">
            {ehNovo ? (
              <>
                O cadastro nasce na filial <strong>{contexto.empresa}</strong> — a do cabeçalho, nunca do
                formulário
              </>
            ) : (
              <>
                {[maquina?.marca, maquina?.familia, maquina?.modeloNome].filter(Boolean).join(' · ') ||
                  'sem modelo'}
                {maquina?.classificacaoNome ? ` · ${maquina.classificacaoNome}` : ''}
                {maquina?.estaInativo ? ' · baixado' : ''}
                {maquina?.origem === 'Art' && <span className="cad-selo cad-selo-art">ART</span>}
              </>
            )}
          </p>
        </div>
        <div className="page-actions">
          <Link to="/equipamentos" className="btn btn-secondary">
            Voltar para a lista
          </Link>
          {modo === 'ficha' && !maquina?.estaInativo && (
            <>
              <button type="button" className="btn btn-secondary" onClick={() => setConfirmandoBaixa(true)}>
                Baixar
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
            Sem eles não há lista de modelos para escolher, e o cadastro fica sem o campo mais
            importante. {erroDeCatalogo.message}
          </p>
        </AvisoDoFormulario>
      )}
      {maquina?.situacao === 'ProprietarioNaoConfirmado' && (
        <AvisoDoFormulario titulo="Dono atual não confirmado" tom="atencao">
          <p>
            Esta máquina chegou por uma venda do ART. O comprador daquela venda está no histórico comercial abaixo, com a
            data — mas quem comprou não prova quem tem a máquina hoje. Para confirmar o dono, edite a ficha, escolha o
            cliente proprietário e a situação Ativo.
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
            <div className="card-title">Identificação da máquina</div>
            <div className="card-subtitle">
              {editando ? 'Campos com * são obrigatórios' : 'Somente leitura — use Editar para alterar'}
            </div>
          </div>
          <SeloProcedencia procedencia={leitura.procedencia} />
        </div>

        <div className="card-body">
          <div className="form-grid cad-grade">
            {ehNovo ? (
              <CampoTexto
                rotulo="Chassi"
                obrigatorio
                largo
                valor={formulario.chassi}
                aoMudar={(v) => mudar('chassi', v.toUpperCase())}
                erro={errosDeCampo.chassi}
                exemplo="1JD8R340ABC123456"
                ajuda="17 caracteres, sem as letras I, O e Q. É a identidade da máquina e não muda depois."
              />
            ) : (
              <CampoSomenteLeitura
                rotulo="Chassi"
                largo
                valor={<code>{maquina?.chassi}</code>}
                ajuda="Não entra na alteração: é a identidade da máquina e a chave de deduplicação. Chassi errado se corrige baixando o registro e cadastrando o certo, para o histórico não mudar de dono em silêncio."
              />
            )}

            <CampoSelecao
              rotulo="Marca"
              valor={marca}
              aoMudar={(v) => {
                setMarca(v);
                setFamilia('');
                mudar('modeloCodigo', '');
              }}
              itens={comoItens(marcas)}
              desabilitado={!editando}
              vazio="Todas"
              ajuda="Vem do catálogo de frota, junto com a família e o modelo."
            />
            <CampoSelecao
              rotulo="Família"
              valor={familia}
              aoMudar={(v) => {
                setFamilia(v);
                mudar('modeloCodigo', '');
              }}
              itens={comoItens(familias)}
              desabilitado={!editando}
              vazio="Todas"
            />
            <CampoSelecao
              rotulo="Modelo"
              largo
              valor={formulario.modeloCodigo}
              aoMudar={(v) => {
                mudar('modeloCodigo', v);
                const escolhido = porCodigo.get(v);
                if (escolhido) {
                  setMarca(escolhido.marca);
                  setFamilia(escolhido.familia);
                }
              }}
              itens={modelosOferecidos.map((m, i) => ({
                codigo: m.codigo,
                descricao: m.descricao,
                ordem: i + 1,
                exigeObservacao: false,
              }))}
              erro={errosDeCampo.modeloCodigo}
              desabilitado={!editando}
              vazio={modeloPendenteDoArt ? 'Sem modelo — produto do ART pendente de revisão' : undefined}
              ajuda={
                modeloEscolhido
                  ? `${modeloEscolhido.marca} · ${modeloEscolhido.familia} · ${modeloEscolhido.modelo}`
                  : modeloPendenteDoArt
                    ? `O produto do ART (“${maquina?.produtoNaOrigem ?? '—'}”) não tem correspondência segura no catálogo. Ele fica preservado na venda; escolha o modelo só se tiver certeza.`
                    : 'Marca e família filtram a lista; escolher o modelo preenche as duas de volta.'
              }
            />

            <CampoSelecao
              rotulo="Classificação de produto"
              valor={formulario.linhaDeProdutoCodigo}
              aoMudar={(v) => mudar('linhaDeProdutoCodigo', v)}
              itens={itensDe(catalogos, CATALOGO.linhaDeProduto)}
              erro={errosDeCampo.linhaDeProdutoCodigo}
              desabilitado={!editando}
              vazio="Sem classificação"
              ajuda="Trator pequeno, médio, grande, colhedora… É o filtro de segmentação da lista. Opcional."
            />

            <SeletorDeCliente
              rotulo="Cliente proprietário"
              largo
              valor={formulario.clienteChave}
              nome={nomeDoCliente}
              aoEscolher={(chaveDoCliente, nome) => {
                mudar('clienteChave', chaveDoCliente);
                setNomeDoCliente(nome);
              }}
              erro={errosDeCampo.clienteChave}
              desabilitado={!editando}
              ajuda="O dono atual. Obrigatório quando a situação é Ativo ou Vendido. O comprador de uma venda do ART não é preenchido aqui automaticamente."
            />

            <CampoSelecao
              rotulo="Situação"
              valor={formulario.situacao}
              aoMudar={(v) => mudar('situacao', v)}
              itens={itensDe(catalogos, CATALOGO.situacaoEquipamento)}
              erro={errosDeCampo.situacao}
              desabilitado={!editando}
              vazio={ehNovo ? 'Ativo (padrão da API)' : 'Selecione…'}
            />

            {ehNovo ? (
              <CampoSelecao
                rotulo="Origem"
                valor={formulario.origem}
                aoMudar={(v) => mudar('origem', v)}
                itens={itensDe(catalogos, CATALOGO.origemEquipamento)}
                erro={errosDeCampo.origem}
                vazio="Crm (padrão da API)"
                ajuda="Quem afirma que a máquina existe: o ERP (Protheus) ou o CEN, pelo CRM."
              />
            ) : (
              <CampoSomenteLeitura
                rotulo="Origem"
                valor={maquina?.origem}
                ajuda="Não entra na alteração: diz quem afirma que a máquina existe, e é o que a tela de Cobertura usa."
              />
            )}

            <CampoTexto
              rotulo="Ano de fabricação"
              tipo="number"
              valor={formulario.anoFabricacao}
              aoMudar={(v) => mudar('anoFabricacao', v)}
              erro={errosDeCampo.anoFabricacao}
              desabilitado={!editando}
            />
            <CampoTexto
              rotulo="Ano do modelo"
              tipo="number"
              valor={formulario.anoModelo}
              aoMudar={(v) => mudar('anoModelo', v)}
              erro={errosDeCampo.anoModelo}
              desabilitado={!editando}
            />
            <CampoTexto
              rotulo="Número de série"
              valor={formulario.numeroSerie}
              aoMudar={(v) => mudar('numeroSerie', v)}
              erro={errosDeCampo.numeroSerie}
              desabilitado={!editando}
              ajuda="Buscável por trecho, diferente do chassi."
            />
            <CampoTexto
              rotulo="Placa"
              valor={formulario.placa}
              aoMudar={(v) => mudar('placa', v.toUpperCase())}
              erro={errosDeCampo.placa}
              desabilitado={!editando}
            />
            <CampoTexto
              rotulo="Localização"
              largo
              valor={formulario.localizacaoDescrita}
              aoMudar={(v) => mudar('localizacaoDescrita', v)}
              erro={errosDeCampo.localizacaoDescrita}
              desabilitado={!editando}
              exemplo="Fazenda Santa Clara · talhão 4"
              ajuda="Texto livre: é a descrição do CEN, e não há catálogo de talhão."
            />
          </div>
        </div>

        {editando && (
          <div className="card-body cad-rodape-form">
            <button type="button" className="btn btn-secondary" onClick={sairDaEdicao} disabled={gravando}>
              Cancelar
            </button>
            <button type="submit" className="btn btn-primary" disabled={gravando}>
              {gravando ? 'Gravando…' : ehNovo ? 'Cadastrar equipamento' : 'Gravar alterações'}
            </button>
          </div>
        )}
      </form>

      {!ehNovo && maquina && maquina.divergenciasAbertas.length > 0 && (
        <div className="card cad-cartao">
          <div className="card-header">
            <div className="card-title">Divergências abertas</div>
            <div className="card-subtitle">
              Onde ART, CRM e Protheus não concordam sobre esta máquina — nada é trocado automaticamente: a correção é
              decisão de quem revisa o cadastro
            </div>
          </div>
          <div className="cad-tabela-wrap">
            <table className="cad-tabela">
              <thead>
                <tr>
                  <th scope="col">Detectada em</th>
                  <th scope="col">Tipo</th>
                  <th scope="col">O que é</th>
                </tr>
              </thead>
              <tbody>
                {maquina.divergenciasAbertas.map((divergencia) => (
                  <tr key={`${divergencia.tipo}-${divergencia.detectadaEm}`}>
                    <td className="cad-mono">{formatarDataHora(divergencia.detectadaEm)}</td>
                    <td>
                      <span className="cad-selo cad-selo-pendente">
                        {divergencia.tipo.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase()}
                      </span>
                    </td>
                    <td>{divergencia.descricao}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {!ehNovo && maquina && (
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Histórico comercial</div>
              <div className="card-subtitle">
                Cada venda com o comprador NELA, a filial e as datas — o comprador de uma venda não é o dono atual
              </div>
            </div>
            <SeloProcedencia procedencia={historico.procedencia} />
          </div>
          {historico.carregando && <BlocoCarregando oQue="o histórico comercial" />}
          {historico.erro && <BlocoErro erro={historico.erro} aoTentarDeNovo={historico.recarregar} />}
          {historico.dados && historico.dados.length === 0 && (
            <div className="card-body">
              <p className="cad-nota">Nenhuma venda registrada para este chassi.</p>
            </div>
          )}
          {historico.dados && historico.dados.length > 0 && (
            <div className="cad-tabela-wrap">
              <table className="cad-tabela">
                <thead>
                  <tr>
                    <th scope="col">Venda</th>
                    <th scope="col">Comprador na venda</th>
                    <th scope="col">Produto e linha no ART</th>
                    <th scope="col">Filial</th>
                    <th scope="col">Gestão da venda</th>
                    <th scope="col">Faturada · entregue</th>
                    <th scope="col">Origem</th>
                  </tr>
                </thead>
                <tbody>
                  {historico.dados.map((venda) => (
                    <tr key={venda.chave}>
                      <td className="cad-mono">{dataCurta(venda.vendidaEm)}</td>
                      <td>
                        {venda.compradorChave ? (
                          <Link to={`/clientes/${venda.compradorChave}`}>{venda.compradorNome}</Link>
                        ) : (
                          <span className="cad-vazio">comprador fora do seu alcance</span>
                        )}
                        <div className="cad-sub">
                          <span className="cad-selo cad-selo-comprador">
                            {venda.natureza === 'CompradorNaVenda' ? 'comprador na venda' : (venda.natureza ?? 'sem vínculo')}
                          </span>
                          {venda.vinculoEncerradoEm && ` encerrado em ${formatarDataHora(venda.vinculoEncerradoEm)}: ${venda.motivoDoEncerramento}`}
                        </div>
                      </td>
                      <td>
                        {venda.produtoNaOrigem}
                        <div className="cad-sub">{venda.linhaNaOrigem}</div>
                      </td>
                      <td className="cad-mono">
                        {venda.filialCodigo}
                        {venda.filialDoFaturamentoCodigo && venda.filialDoFaturamentoCodigo !== venda.filialCodigo && (
                          <div className="cad-sub">faturou {venda.filialDoFaturamentoCodigo}</div>
                        )}
                      </td>
                      <td>{venda.gestaoNaOrigem ?? '—'}</td>
                      <td className="cad-mono">
                        {dataCurta(venda.faturadaEm)}
                        <div className="cad-sub">{dataCurta(venda.entregueEm)}</div>
                      </td>
                      <td>
                        {venda.sistemaCodigo} · <span className="cad-mono">{venda.chaveOrigem}</span>
                        <div className="cad-sub">importada {formatarDataHora(venda.importadaEm)}</div>
                        {venda.transformacoes && <div className="cad-sub">{venda.transformacoes}</div>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          <div className="card-body">
            <p className="cad-nota">
              “Gestão” (Varejo ou Grandes Contas) é atributo da venda, como o ART escreve — não é classificação do cliente nem
              equivale a SAM ou KAM.
            </p>
          </div>
        </div>
      )}

      {!ehNovo && maquina && (
        <div className="card cad-cartao">
          <div className="card-header">
            <div className="card-title">Registro</div>
            <div className="card-subtitle">O que o CRM sabe sobre o próprio cadastro</div>
          </div>
          <div className="card-body">
            <div className="form-grid cad-grade">
              <CampoSomenteLeitura rotulo="Chave pública" largo valor={<code>{maquina.chave}</code>} />
              <CampoSomenteLeitura
                rotulo="Filial dona do registro"
                valor={`${descricaoDe(catalogos, CATALOGO.empresa, contexto.empresa)} · id interno ${maquina.empresaId}`}
                ajuda="A fronteira de acesso, e é sempre a filial do cabeçalho: em outra filial esta ficha responde 404."
              />
              <CampoSomenteLeitura
                rotulo="Marca representada"
                valor={
                  maquina.marcaRepresentada === null
                    ? '—'
                    : maquina.marcaRepresentada
                      ? 'Sim — marca que a Tracbel representa'
                      : 'Não — máquina de concorrente'
                }
                ajuda="É o que separa a frota nossa da do concorrente na Cobertura de Carteira."
              />
              <CampoSomenteLeitura
                rotulo="Horímetro atual"
                valor={maquina.horimetroAtual === null ? '—' : `${formatarNumero(maquina.horimetroAtual, 1)} h`}
                ajuda="Vem do histórico de horímetro; não se digita nesta tela. O horímetro do ART ainda não está acessível (documento 37)."
              />
              <CampoSomenteLeitura rotulo="Cadastrado em" valor={formatarDataHora(maquina.criadoEm)} />
              <CampoSomenteLeitura rotulo="Última alteração" valor={formatarDataHora(maquina.alteradoEm)} />
            </div>
          </div>
        </div>
      )}

      {confirmandoBaixa && (
        <DialogoConfirmacao
          titulo="Baixar esta máquina?"
          subtitulo={maquina?.chassi}
          rotuloConfirmar="Baixar equipamento"
          gravando={gravando}
          aoCancelar={() => setConfirmandoBaixa(false)}
          aoConfirmar={confirmarBaixa}
        >
          <p>
            A baixa é <strong>exclusão lógica</strong>: a linha e o histórico de horímetro continuam no
            banco. A máquina sai da lista padrão e continua visível com o filtro{' '}
            <em>Mostrar baixados</em>.
          </p>
          <p>
            O <code>DELETE</code> de equipamento não pede motivo — diferente do de cliente. É o
            contrato do documento 23, seção 2.2.
          </p>
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
              navegar('/equipamentos');
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
