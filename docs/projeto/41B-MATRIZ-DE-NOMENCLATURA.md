# 41B — Matriz de nomenclatura: nome atual → nome proposto

> **Versão 1.0 · 15/09/2026 · decisão D-9.** Aplica o [padrão 41](41-PADRAO-DE-NOMENCLATURA.md), nome a
> nome. **Nada foi renomeado.** Cada linha só é executada dentro da fase indicada, no
> [plano executivo](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md).
>
> Só entram aqui os nomes que **sobrevivem** ao modelo alvo. Nome de tabela que sai (as 32 da fase 1, as
> consolidadas depois) não é renomeado — é removido; a lista delas está no
> [40B](40B-MATRIZ-ATUAL-PARA-ALVO.md).

## 1. Tabelas, entidades e `DbSet`

| Nome atual (tabela · entidade · `DbSet`) | Nome proposto | Motivo | Impacto | Fase |
|---|---|---|---|:-:|
| `seguranca.ConjuntoDePermissao` · `ConjuntoPermissao` · `ConjuntosPermissao` | `seguranca.Perfil` · `Perfil` · `Perfis` | é o perfil de acesso; entidade diverge da tabela (doc 39, A-6) | 1 migração; `EscopoDeAcesso`; telas de administração (novas) | 3 |
| `seguranca.ConjuntoDePermissaoItem` · `ItemConjuntoPermissao` · *(sem `DbSet`)* | `seguranca.PerfilPermissao` · `PerfilPermissao` · `PerfisPermissoes` | vínculo perfil × permissão; ganha `DbSet` próprio | mesma migração; `EscopoDeAcesso` (`Set<T>` sai) | 3 |
| `seguranca.UsuarioConjuntoDePermissao` · `UsuarioConjuntoPermissao` · `ConcessoesPermissao` | `seguranca.UsuarioPerfil` · `UsuarioPerfil` · `UsuarioPerfis` | vínculo usuário × perfil; o `DbSet` não lembrava a tabela | mesma migração; `EscopoDeAcesso` | 3 |
| `processo.TipoProcesso` · `TipoProcesso` · `TiposDeProcesso` | `processo.Pipeline` · `Pipeline` · `Pipelines` | é o funil que a tela chama de pipeline | 1 migração; `RepositorioDeProcessos`; contratos de funil; telas Pipeline e Funil | 4 |
| `processo.Fase` · `Fase` · `Fases` | `processo.Etapa` · `Etapa` · `Etapas` | "etapa do pipeline" é o termo do negócio | mesma migração; funil; Pipeline; Ficha de Oportunidade | 4 |
| `processo.TipoTarefa` · `TipoTarefa` · `TiposDeTarefa` | `processo.TipoDeAtividade` · `TipoDeAtividade` · `TiposDeAtividade` | serve a tarefa **e** a interação; "tipo de tarefa" escondia metade do uso | mesma migração; Agenda; Visão 360; cobertura (`ContaParaCobertura`) | 4 |
| `processo.Resultado` · `Resultado` · `Resultados` | `processo.ResultadoDeAtividade` · `ResultadoDeAtividade` · `ResultadosDeAtividade` | "resultado" sozinho colide com o desfecho de execução de integração | mesma migração; Agenda; Ficha de Oportunidade | 4 |
| `processo.Processo` · `Processo` · `Processos` | `comercial.Oportunidade` · `Oportunidade` · `Oportunidades` | a tela, a diretoria e o funil dizem oportunidade; "processo" veio do Vórtice | 1 migração (com mudança de schema); rota `/api/v1/processos` → `/api/v1/oportunidades`; contratos; 6 telas | 6 |
| `frota.LinhaDeProduto` · `LinhaDeProduto` · `LinhasDeProduto` | `frota.ClassificacaoDeProduto` · `ClassificacaoDeProduto` · `ClassificacoesDeProduto` | colide com linha de negócio; o conceito é classificação comercial do produto | 1 migração; catálogos; Equipamentos; máquinas compradas | 7 |
| `integracao.CompradorPendente` · `CompradorPendente` · `CompradoresPendentes` | `integracao.PendenciaDeCadastro` · `PendenciaDeCadastro` · `PendenciasDeCadastro` | vale para qualquer origem e qualquer parte, não só comprador do ART | 1 migração; carga do ART; Protheus | 7 |
| `comercial.FaturamentoDoCliente` · `FaturamentoDoCliente` · `FaturamentoDosClientes` | `comercial.Faturamento` · `Faturamento` · `Faturamentos` | passa a cobrir também a contraparte sem cliente | 1 migração; 4 repositórios; Visão 360; Performance; Indicadores | 8 |
| `organizacao.Carteira` (schema) | `comercial.Carteira` | carteira é relação com o cliente | move de schema na mesma migração da fase | 4 |
| `organizacao.LinhaDeNegocio` (schema) | `processo.LinhaDeNegocio` | é configuração do processo comercial | idem | 4 |
| `processo.VendaPerdida` (schema) | `comercial.VendaPerdida` | é resultado comercial | idem | 6 |

## 2. Colunas

| Nome atual | Nome proposto | Motivo | Impacto | Fase |
|---|---|---|---|:-:|
| `ConjuntoDePermissaoItem.ConjuntoPermissaoId` | `PerfilPermissao.PerfilId` | FK usa o nome completo da entidade (doc 39, A-6) | `EscopoDeAcesso` | 3 |
| `Usuario.Papel` | *(removida)* | não autoriza nada; o papel é o perfil | `/auth/eu`; cabeçalho da tela | 3 |
| `TipoProcesso.Versao` (`int`) | *(removida)* | `Versao` é reservada para concorrência (doc 39, A-7) | nenhuma leitura | 4 |
| `Fase.*` → `Etapa.*`; `Processo.FaseId` | `Oportunidade.EtapaId` | acompanha a tabela | funil, pipeline | 4, 6 |
| `Processo.FaseDesde` | `Oportunidade.EtapaDesde` | idem | funil | 6 |
| `Tarefa.TipoTarefaId`, `Interacao.TipoTarefaId`, `Resultado.TipoTarefaId` | `TipoDeAtividadeId` | acompanha a tabela | Agenda, Visão 360, cobertura | 4, 6 |
| `Cliente.Documento` | `Cliente.CpfCnpj` | `Documento` fica reservado para arquivo (doc 39, A-8) | cadastro de cliente; busca; contratos; tela | 5 |
| `CompradorPendente.Documento` | `PendenciaDeCadastro.CpfCnpj` | idem | carga do ART | 7 |
| `FaturamentoSemCliente.Documento` | `Faturamento.CpfCnpj` | idem | integração do Protheus; indicadores | 8 |
| `FaturamentoSemCliente.Natureza` | `Faturamento.NaturezaDaContraparte` | `Natureza` só quando o enum é `NaturezaDo<Entidade>` | indicadores; Visão 360 | 8 |
| `FaturamentoSemCliente.Nome` | `Faturamento.NomeNaNota` | diz de onde o nome vem | idem | 8 |
| `ExecucaoDeSincronizacao.Resultado` | `Desfecho` | coluna sem `Id` chamada `Resultado` confunde com a FK `ResultadoId` | Configurações › Integrações | 7 |
| `EstaAtiva` em `LinhaDeNegocio`, `Carteira`, `Marca`, `Familia`, `ClassificacaoDeProduto`, `RegraDePotencial` | `EstaAtivo` | uma grafia só para o mesmo conceito (hoje 16 × 13) | consultas e filtros das telas de catálogo | 4, 5, 7 |
| `Equipamento.LinhaDeProdutoId`, `CorrespondenciaDaOrigem.LinhaDeProdutoId` | `ClassificacaoDeProdutoId` | acompanha a tabela | Equipamentos; carga do ART | 7 |
| `Equipamento.Origem` | *(removida)* | é derivável das vendas; vocabulário de integração no domínio | ficha e lista de Equipamentos; Cobertura | 7 |
| `ClienteCarteira.Classe`, `ClienteCarteira.DiasCicloContato` | *(removidas)* | segunda classe e segunda cadência (documento 42) | Cobertura de Carteira | 5 |
| `MunicipioDaAreaDeAtuacao.ArquivoDeOrigem`, `LinhaNaOrigem`, `ImportadoEm`, `ImportadoPorId` | *(removidas — vão para `RegistroDeOrigem`)* | linhagem de planilha em tabela de negócio | carga de território | 9 |
| `VendaDeMaquina.*NaOrigem` (10), `HashDaOrigem`, `Transformacoes`, `SistemaId`, `ChaveOrigem`, `ImportadaEm`, `AtualizadaPelaOrigemEm` | *(removidas — vão para `RegistroDeOrigem`)* | vocabulário do ART no domínio | ficha de Equipamento; máquinas compradas | 7 |

## 3. Contratos, portas, casos de uso e rotas

| Nome atual | Nome proposto | Motivo | Impacto | Fase |
|---|---|---|---|:-:|
| `ProcessoResumo`, `ProcessoDetalhe` | `OportunidadeResumo`, `OportunidadeDetalhe` | vocabulário | `tipos/relacionamento.ts`; Pipeline; Ficha | 6 |
| `FaseDoFunil` | `EtapaDoFunil` | vocabulário | Funil | 6 |
| `BaixaDeEquipamento` | `InativacaoDeEquipamento` | padrão de contrato de baixa (`InativacaoDe<Entidade>`) | rota `DELETE /equipamentos/{chave}` | 7 |
| `IRepositorioClientes`, `IRepositorioEquipamentos`, … (17 portas) | `IRepositorioDeClientes`, `IRepositorioDeEquipamentos`, … | a implementação já usa "De"; a porta, não | só código; sem banco | 3 |
| `GET /api/v1/processos`, `/processos/{chave}` | `/api/v1/oportunidades`, `/oportunidades/{chave}` | vocabulário | `relacionamento.ts`, `consolidado.ts`; Pipeline; Ficha; Visão 360 | 6 |
| `GET /api/v1/relatorios/perdas` e `/relatorios/vendas-perdidas` | `/relatorios/perdas` (uma rota) | hoje são duas leituras do mesmo assunto | Funil; Visão 360 | 6 |
| `dados/api/relacionamento.ts` | mantém | o módulo cobre oportunidade, tarefa, interação e cobertura | — | — |

## 4. Enums

| Nome atual | Nome proposto | Motivo | Fase |
|---|---|---|:-:|
| `NaturezaDoParceiro` | `NaturezaDaContraparte` | acompanha a coluna | 8 |
| `ResultadoDaExecucao` | `DesfechoDaExecucao` | acompanha a coluna | 7 |
| `SituacaoDoProcesso` | `SituacaoDaOportunidade` | vocabulário | 6 |
| `OrigemDaAtribuicao` (`Processo/Tarefa.cs`) e `OrigemAtribuicao` (`Workflow/ContextoRegra.cs`) | *(removidos)* | dois enums para o mesmo conceito, nenhum em uso | 1, 6 |
| `OrigemDoEquipamento`, `NaturezaDoVinculoComEquipamento` | *(removidos)* | conceito sai com a coluna e com a tabela | 7 |
| `SituacaoLead`, `TipoDeCanal`, `CanalDeConsentimento`, `FinalidadeDeConsentimento`, `SeveridadeDeAlerta`, `TipoDeEquipe`, `NivelDeCompartilhamento`, `MotivoDeCompartilhamento`, `TipoDeMeta`, `TipoDeCampo`, `MomentoDoTratador`, `TipoDeResposta`, `SituacaoDoPreenchimento`, `SituacaoDaRecepcao`, `SituacaoDaMensagem`, `PapelNaInteracao` | *(removidos com as tabelas)* | — | 1 |
| `ClasseDeCliente`, `SituacaoDoCliente`, `TipoDePessoa`, `NaturezaDaInteracao`, `SituacaoDaTarefa`, `NaturezaDaCarteira`, `NaturezaDoUsuario`, `Profundidade`, `PapelNoMunicipio`, `RegiaoDaAreaDeAtuacao` | mantidos | já seguem o padrão | — |

## 5. Migrações propostas (uma por fase)

| Fase | Nome da migração |
|:-:|---|
| 1 | `RemocaoDeEstruturasSemUso` |
| 2 | `AuditoriaComOrigemDaOperacao` |
| 3 | `PerfilDeAcessoESubstituicaoDoPapel` |
| 4 | `ConfiguracaoComercialAdministravel` |
| 5 | `ClienteContatoEnderecoComFonteUnica` |
| 6 | `OportunidadeAtividadeSemCiclo` |
| 7 | `RastroDeOrigemUnicoEVendaEnxuta` |
| 8 | `FaturamentoUnico` |
| 9 | `TerritorioConciliado` |

## 6. Resumo por fase

| Fase | Tabelas renomeadas | Colunas renomeadas ou removidas por nome | Contratos e rotas |
|:-:|---:|---:|---:|
| 3 | 3 | 2 | 17 portas |
| 4 | 4 (+2 mudanças de schema) | 6 | contratos de funil e agenda |
| 5 | — | 3 | contratos de cliente |
| 6 | 1 (+1 schema) | 4 | 2 rotas, 3 contratos, 6 telas |
| 7 | 2 | 20 | 2 contratos, 3 telas |
| 8 | 1 | 4 | contratos de faturamento |
| 9 | — | 4 | — |

**Regra de execução:** o renome de uma tabela, das suas colunas, dos contratos, das rotas e das telas
que a citam acontece **na mesma entrega**. Nenhuma fase termina com metade do sistema no nome antigo.
