# 40B — Matriz: tabela atual → modelo alvo

> Anexo do [documento 40](40-ARQUITETURA-ALVO-DO-BANCO.md). **Proposta — nada foi alterado.**
> Base: [documento 39](39-AUDITORIA-ARQUITETURA-BANCO.md), anexos 39A/39B e o código em `src/`.
> Versão 1.0 · 15/09/2026.

**Contagem oficial** (seção 1 do documento 40): **82 tabelas físicas = 80 do modelo EF Core + 2
históricos de migração.** As 82 estão todas abaixo, uma vez cada.

**Legenda**

- **Linhas** = linhas antes da sanitização (banco local, carga completa). ✱ = uma das 35 tabelas do
  modelo que nunca tiveram linha (a 36ª física vazia é `dbo.__EFMigrationsHistory`).
- **Decisão**: MANTER IGUAL · MANTER E REFATORAR · CONSOLIDAR · INTEGRAÇÃO · INVESTIGAR ·
  NÃO CRIAR AINDA · REMOVER FUTURAMENTE.
- **Fase**: a fase da seção 17 do documento 40 em que a mudança acontece.

## Resumo

| Decisão | Tabelas | Ficam no alvo |
|---|---:|---:|
| MANTER IGUAL | 9 | 9 |
| MANTER E REFATORAR | 27 | 27 |
| INTEGRAÇÃO | 6 | 6 |
| CONSOLIDAR | 8 | 0 (absorvidas por tabelas que ficam) |
| INVESTIGAR | 1 | 0 (destino proposto: consolidar) |
| NÃO CRIAR AINDA | 11 | 0 (voltam com a funcionalidade) |
| REMOVER FUTURAMENTE | 20 | 0 |
| **Total** | **82** | **42** (41 de domínio + 1 técnica) |

## Matriz completa

### organizacao (12)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 1 | `organizacao.Empresa` | 18 | `Empresa` (Organização) | MANTER IGUAL | fronteira multiempresa; 32 FKs de entrada; hierarquia por `EmpresaPaiId` + `Caminho` já resolve grupo → empresa → filial — não há razão para uma tabela `Filial` | — |
| 2 | `organizacao.LinhaDeNegocio` | 14 | `LinhaDeNegocio` (Configuração comercial) | MANTER IGUAL | catálogo com cadência por classe; ganha tela administrativa e passa a ser a **única** fonte de cadência | 4 |
| 3 | `organizacao.Carteira` | 142 | `Carteira` (CRM) | MANTER E REFATORAR | sai `PracaId`, `EquipeId`, `SupervisorId` (nulo em 655 de 655 carteiras no Vórtice); ganha escrita pela área administrativa | 1, 4 |
| 4 | `organizacao.CarteiraMunicipio` | 532 | `CarteiraMunicipio` (Território) | MANTER E REFATORAR | fonte canônica de "qual CEN atende o município, por linha"; recebe a conciliação das planilhas | 8 |
| 5 | `organizacao.HierarquiaComercial` ✱ | 0 | — | REMOVER FUTURAMENTE | *closure table* nunca preenchida; `Usuario.GestorId` é a fonte | 1 |
| 6 | `organizacao.Meta` ✱ | 0 | — | NÃO CRIAR AINDA | lida e nunca gravada; volta com a tela de metas | 1 |
| 7 | `organizacao.Praca` ✱ | 0 | — | REMOVER FUTURAMENTE | ideia não implementada; o potencial vem de área plantada × regra de potencial | 1 |
| 8 | `organizacao.Municipio` | 9.863 | `Municipio` (Território) | MANTER E REFATORAR | `CodigoIbge` obrigatório (só restaram municípios do IBGE) | 5 |
| 9 | `organizacao.MunicipioDaAreaDeAtuacao` | 238 | `MunicipioDaAreaDeAtuacao` (Território) | MANTER E REFATORAR | fonte canônica da filial responsável e da ADR; a linhagem da planilha (`ArquivoDeOrigem`, `LinhaNaOrigem`, `ImportadoEm`, `ImportadoPorId`) vai para `RegistroDeOrigem` | 8 |
| 10 | `organizacao.ResponsavelPeloMunicipio` | 609 | `CarteiraMunicipio` + `RegistroDeOrigem` | INVESTIGAR | é afirmação de planilha, não atribuição; as duas planilhas discordam em 82 municípios. Depois que o comercial confirmar a vigente, vira `CarteiraMunicipio` e a tabela sai | 8 |
| 11 | `organizacao.AreaPlantadaNoMunicipio` | 45.582 | `AreaPlantadaNoMunicipio` (Território) | MANTER E REFATORAR | referência pública do IBGE; sai `ImportadoPorId` (FK obrigatória para usuário técnico) — o rastro é a execução da carga | 8 |
| 12 | `organizacao.RegraDePotencial` | 1 | `RegraDePotencial` (Território) | MANTER IGUAL | parâmetro do cálculo de potencial; ganha edição administrativa quando confirmado | — |

### seguranca (8)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 13 | `seguranca.Usuario` | 276 | `Usuario` (Identidade) | MANTER E REFATORAR | sai `Papel` (não autoriza nada); `GestorId` passa a ser a única hierarquia | 3 |
| 14 | `seguranca.Equipe` ✱ | 0 | — | REMOVER FUTURAMENTE | nunca usada; leva junto 4 colunas `...EquipeId` sempre nulas | 1 |
| 15 | `seguranca.EquipeMembro` ✱ | 0 | — | REMOVER FUTURAMENTE | idem | 1 |
| 16 | `seguranca.Permissao` ✱ | 0 | catálogo em código | REMOVER FUTURAMENTE | isolada; o código é quem verifica a permissão, logo é quem a define | 1 |
| 17 | `seguranca.ConjuntoDePermissao` ✱ | 0 | `Perfil` (Identidade) | MANTER E REFATORAR | é o perfil; renomear e marcar o perfil padrão | 3 |
| 18 | `seguranca.ConjuntoDePermissaoItem` ✱ | 0 | `PerfilPermissao` (Identidade) | MANTER E REFATORAR | código + profundidade; coluna `ConjuntoPermissaoId` vira `PerfilId` | 3 |
| 19 | `seguranca.UsuarioConjuntoDePermissao` ✱ | 0 | `UsuarioPerfil` (Identidade) | MANTER E REFATORAR | concessão com vigência; é a única autoridade de quem pode o quê | 3 |
| 20 | `seguranca.CompartilhamentoDeRegistro` ✱ | 0 | — | REMOVER FUTURAMENTE | ideia não implementada | 1 |

### comercial (11)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 21 | `comercial.Cliente` | 23.945 | `Cliente` (CRM) | MANTER E REFATORAR | sai `ProprietarioEquipeId`, `FaturamentoApurado`; `Classe` + `ClasseApuradaEm` viram a única classe; `ClienteMatrizId` sem uso vai para INVESTIGAR de campo | 1, 5 |
| 22 | `comercial.Contato` | 23.313 | `Contato` (CRM) | MANTER E REFATORAR | ganha `ClienteId`, `PapelId`, `Email`, `Telefone`, `Celular`; sai `ProprietarioId` (herda do cliente) | 5 |
| 23 | `comercial.ClienteContato` | 23.313 | `Contato.ClienteId` + `Contato.PapelId` | CONSOLIDAR | 23.313 vínculos para 23.313 contatos: na carga cada contato tinha um cliente só | 5 |
| 24 | `comercial.CanalContato` ✱ | 0 | colunas de `Contato` | CONSOLIDAR | nunca usada; o contato ficou sem telefone e sem e-mail. Três colunas resolvem o que a tela precisa | 5 |
| 25 | `comercial.ConsentimentoComunicacao` ✱ | 0 | — | NÃO CRIAR AINDA | volta com comunicação ativa (campanha, WhatsApp, e-mail em massa) | 1 |
| 26 | `comercial.Endereco` | 19.641 | `Endereco` (CRM) | MANTER E REFATORAR | `MunicipioId` obrigatório; saem `Municipio` (texto residual da migração, que não existe mais), `Uf` (derivada), `Hectares` e `CulturaId` (100% vazias) | 5 |
| 27 | `comercial.ClienteCarteira` | 49.109 | `ClienteCarteira` (CRM) | MANTER E REFATORAR | saem `Classe` (Vórtice em branco → "C" por padrão), `DiasCicloContato` (segunda fonte de cadência), `PotencialAnual` (sem uso) e `UltimaInteracaoEm` (cache calculável por índice) | 5 |
| 28 | `comercial.Lead` ✱ | 0 | `Cliente` com `Situacao = Suspect/Prospect` | CONSOLIDAR | o enum de situação do cliente já tem suspect e prospect; sai o filtro global próprio | 1 |
| 29 | `comercial.Alerta` ✱ | 0 | — | REMOVER FUTURAMENTE | os alertas da Visão 360 são calculados | 1 |
| 30 | `comercial.FaturamentoDoCliente` | 37.867 | `Faturamento` (Comercial) | MANTER E REFATORAR | vira a única tabela de faturamento: `ClienteId` anulável + `Documento`, `NomeNaNota`, `Natureza` | 8 |
| 31 | `comercial.FaturamentoSemCliente` | 20.474 | `Faturamento` | CONSOLIDAR | 17 de 18 colunas iguais; a diferença é a contraparte, que vira `Natureza` | 8 |

### processo (14)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 32 | `processo.TipoProcesso` | 16 | `Pipeline` (Configuração comercial) | MANTER E REFATORAR | nome de negócio; sai `Versao` (sem versionamento real); gerido pela área administrativa | 4 |
| 33 | `processo.Fase` | 50 | `Etapa` (Configuração comercial) | MANTER E REFATORAR | sai `ExigeCamposObrigatorios` (sem regra) | 4 |
| 34 | `processo.TipoTarefa` | 179 | `TipoDeAtividade` (Configuração comercial) | MANTER E REFATORAR | um catálogo para tarefa e interação; saem `FormularioId`, `EhAprovacao`, `ExigeGeorreferencia`, `TipoProcessoId` | 4 |
| 35 | `processo.Resultado` | 484 | `ResultadoDeAtividade` (Configuração comercial) | MANTER E REFATORAR | sai `FaseDestinoId` (automação não construída) | 4 |
| 36 | `processo.MotivoDePerda` | 8 | `MotivoDePerda` (Configuração comercial) | MANTER IGUAL | catálogo com atributos próprios | 4 |
| 37 | `processo.Processo` | 45.402 | `Oportunidade` (Comercial) | MANTER E REFATORAR | nome de negócio; sai `ProprietarioEquipeId`; a perda (`MotivoDePerdaId`, `ObservacaoDaPerda`, `ConcorrenteId`) passa para `VendaPerdida` | 6 |
| 38 | `processo.PassagemDeFase` ✱ | 0 | — | NÃO CRIAR AINDA | a auditoria de `Oportunidade.EtapaId` guarda o caminho até existir métrica de tempo por etapa | 1 |
| 39 | `processo.Tarefa` | 103.353 | `Tarefa` (Atividades) | MANTER E REFATORAR | saem `InteracaoConclusaoId`, `InteracaoOrigemId`, `ResultadoId`, `ConcluidaPorId`, `CriadaPorRegraId`, `ResponsavelEquipeId`, `OrigemAtribuicao` — acaba o ciclo | 6 |
| 40 | `processo.Interacao` | 122.002 | `Interacao` (Atividades) | MANTER E REFATORAR | `TarefaId` vira o único vínculo com a tarefa; sai `LeadId` | 6 |
| 41 | `processo.InteracaoParticipante` ✱ | 0 | — | REMOVER FUTURAMENTE | ideia não implementada | 1 |
| 42 | `processo.ItemDeProposta` ✱ | 0 | — | NÃO CRIAR AINDA | volta com proposta itemizada | 1 |
| 43 | `processo.Regra` ✱ | 0 | — | REMOVER FUTURAMENTE | motor sem repositório nem registro; automação futura será redesenhada (com empresa) | 1 |
| 44 | `processo.RegraExecucao` ✱ | 0 | — | REMOVER FUTURAMENTE | idem | 1 |
| 45 | `processo.VendaPerdida` | 165 | `VendaPerdida` (Comercial) | MANTER E REFATORAR | única fonte da perda: motivo, concorrente, preço; `OportunidadeId` único e anulável; sai `RegistradaPor` (texto) | 6 |

### frota (8)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 46 | `frota.Marca` | 13 | `Marca` (Catálogo) | MANTER IGUAL | | — |
| 47 | `frota.Familia` | 28 | `Familia` (Catálogo) | MANTER IGUAL | | — |
| 48 | `frota.Modelo` | 253 | `Modelo` (Catálogo) | MANTER E REFATORAR | ganha `ClassificacaoDeProdutoId`: a classificação passa a ser do modelo | 7 |
| 49 | `frota.LinhaDeProduto` | 10 | `ClassificacaoDeProduto` (Catálogo) | MANTER E REFATORAR | nome que não colide com linha de negócio; sai `FamiliaId` | 7 |
| 50 | `frota.Equipamento` | 5.963 | `Equipamento` (Parque de máquinas) | MANTER E REFATORAR | saem `Origem` (derivável das vendas), `VendidoEm`, `GarantiaAte`, `EquipamentoPaiId`, `EquipamentoSubstitutoId`, `EnderecoId` (sem uso); classificação só quando não há modelo | 7 |
| 51 | `frota.LeituraDeHorimetro` ✱ | 0 | — | REMOVER FUTURAMENTE | `Equipamento.HorimetroAtual` basta até existir telemetria | 1 |
| 52 | `frota.VendaDeMaquina` | 2.108 | `VendaDeMaquina` (Parque de máquinas) | MANTER E REFATORAR | fica o fato (máquina, comprador, filial, datas, pedido, nota); saem `SistemaId`, `ChaveOrigem`, 10 colunas "NaOrigem", hash, transformações, `ImportadaEm`, `AtualizadaPelaOrigemEm`, `Quantidade` | 7 |
| 53 | `frota.VinculoDeClienteComEquipamento` | 2.108 | `VendaDeMaquina.CompradorId` | CONSOLIDAR | 2.108 = 2.108; natureza com um único valor | 7 |

### integracao (11)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 54 | `integracao.Sistema` | 4 | `Sistema` (Integrações) | INTEGRAÇÃO | catálogo de sistemas externos | — |
| 55 | `integracao.ChaveExterna` | 401.909 | `RegistroDeOrigem` | CONSOLIDAR | o de-para vira o mecanismo único de rastreio | 7 |
| 56 | `integracao.PontoDeSincronismo` | 21 | `ExecucaoDeSincronizacao` | CONSOLIDAR | a marca d'água é a última execução com sucesso; os contadores já estão lá | 7 |
| 57 | `integracao.Recepcao` ✱ | 0 | — | REMOVER FUTURAMENTE | substituída na prática por `RegistroDeOrigem` | 1 |
| 58 | `integracao.MensagemDeSaida` ✱ | 0 | — | REMOVER FUTURAMENTE | não há integração de saída; o Protheus é só leitura | 1 |
| 59 | `integracao.MensagemDescartada` | 24.948 | `RegistroDeOrigem` (`Decisao = Rejeitado`) | CONSOLIDAR | rejeição de carga é uma decisão sobre a linha lida | 7 |
| 60 | `integracao.CorrespondenciaDaOrigem` | 156 | `CorrespondenciaDaOrigem` (Integrações) | INTEGRAÇÃO | de-para de valor revisável por pessoa | 7 |
| 61 | `integracao.RegistroDeOrigem` | 4.144 | `RegistroDeOrigem` generalizado (Integrações) | INTEGRAÇÃO | ganha `Entidade` + `RegistroId` + `Dados`; vira o padrão para todo dado externo | 7 |
| 62 | `integracao.CompradorPendente` | 851 | `PendenciaDeCadastro` (Integrações) | INTEGRAÇÃO | fila de documento sem cliente, de qualquer origem; a fotografia do cadastro do Protheus sai (é consultada na hora) | 7 |
| 63 | `integracao.DivergenciaDeIntegracao` | 47 | `DivergenciaDeIntegracao` (Integrações) | INTEGRAÇÃO | revisão humana de conflito entre fontes | 7 |
| 64 | `integracao.ExecucaoDeSincronizacao` | 4 | `ExecucaoDeSincronizacao` (Integrações) | INTEGRAÇÃO | ganha `UltimoValorLido` | 7 |

### metadado (9, incluindo o histórico de migração)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 65 | `metadado.Catalogo` | 10 | `Catalogo` (Catálogo) | MANTER IGUAL | | — |
| 66 | `metadado.CatalogoItem` | 227 | `CatalogoItem` (Catálogo) | MANTER IGUAL | o padrão de FK composta continua | — |
| 67 | `metadado.CampoPersonalizado` ✱ | 0 | — | REMOVER FUTURAMENTE | ideia não implementada | 1 |
| 68 | `metadado.TratadorDeEvento` ✱ | 0 | — | REMOVER FUTURAMENTE | ideia não implementada | 1 |
| 69 | `metadado.Formulario` ✱ | 0 | — | NÃO CRIAR AINDA | volta com o formulário do CEN | 1 |
| 70 | `metadado.Pergunta` ✱ | 0 | — | NÃO CRIAR AINDA | idem | 1 |
| 71 | `metadado.Preenchimento` ✱ | 0 | — | NÃO CRIAR AINDA | idem | 1 |
| 72 | `metadado.Resposta` ✱ | 0 | — | NÃO CRIAR AINDA | idem | 1 |
| 81 | `metadado.__EFMigrationsHistory` | 12 | `__EFMigrationsHistory` (técnico) | MANTER IGUAL | histórico configurado | — |

### relatorio (3)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 73 | `relatorio.Fonte` ✱ | 0 | — | REMOVER FUTURAMENTE | o banco tem 0 views; relatórios são rotas | 1 |
| 74 | `relatorio.FonteCampo` ✱ | 0 | — | REMOVER FUTURAMENTE | idem | 1 |
| 75 | `relatorio.Relatorio` ✱ | 0 | — | REMOVER FUTURAMENTE | idem | 1 |

### auditoria (3)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 76 | `auditoria.AlteracaoDeCampo` | 6.049 | `AlteracaoDeCampo` (Auditoria) | MANTER E REFATORAR | ganha `Origem`, `SistemaId`, `Operacao`; passa a ser gravada por interceptador em toda gravação | 2 |
| 77 | `auditoria.CampoAuditado` ✱ | 0 | política em código | REMOVER FUTURAMENTE | a lista do que se audita fica versionada junto das entidades | 1 |
| 78 | `auditoria.EventoDeAcesso` ✱ | 0 | — | NÃO CRIAR AINDA | volta com o requisito formal de log de acesso a dado pessoal | 1 |

### documento (2) e dbo (1)

| # | Tabela atual | Linhas | Destino no alvo | Decisão | Justificativa | Fase |
|---:|---|---:|---|---|---|:-:|
| 79 | `documento.Documento` ✱ | 0 | — | NÃO CRIAR AINDA | volta com anexos (e o armazenamento de arquivo) | 1 |
| 80 | `documento.Vinculo` ✱ | 0 | — | NÃO CRIAR AINDA | idem | 1 |
| 82 | `dbo.__EFMigrationsHistory` | 0 | — | REMOVER FUTURAMENTE | órfã; a configuração do EF aponta para `metadado` | 1 |

## Verificação da matriz

- Linhas numeradas de 1 a 82, sem repetição: organizacao 12 · seguranca 8 · comercial 11 · processo 14 ·
  frota 8 · integracao 11 · metadado 9 (8 do modelo + histórico) · relatorio 3 · auditoria 3 ·
  documento 2 · dbo 1 = **82**.
- As 35 tabelas ✱ = 20 REMOVER FUTURAMENTE (exceto `dbo.__EFMigrationsHistory`, que não é do modelo)
  − 1 + 11 NÃO CRIAR AINDA + 3 que ficam como `Perfil`/`PerfilPermissao`/`UsuarioPerfil` + 2
  consolidadas (`CanalContato`, `Lead`) = 19 + 11 + 3 + 2 = **35**.
