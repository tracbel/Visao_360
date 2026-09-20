# 23A — A matriz das 39 rotas: caso de uso, tabela, permissão, teste e tela

> **Documento 23A** · Versão 1.0 · 19/09/2026 · anexo do [documento 23](23-API.md) · issue **#3**
> **Levantamento, não proposta.** Toda linha das seções 2 e 3 foi lida no código em 19/09/2026 —
> endpoint, caso de uso, repositório, teste e consumidor no front. A **única** coluna que não é
> leitura é a "permissão proposta" (§2.3): ela é sugestão, e espera a revisão de quem decide a
> **#46**. Nada aqui altera rota.
> **Autorização:** nenhuma — é leitura de código.

---

## 1. Como a matriz foi levantada

| Coluna | De onde veio |
|---|---|
| Método, rota, arquivo:linha | os `Map*` do projeto `Tracbel.Crm.Api` (8 arquivos) |
| Caso de uso | o tipo injetado no delegate de cada rota, em `Tracbel.Crm.Aplicacao` |
| Tabelas | o repositório que o caso de uso recebe, pelos `contexto.<DbSet>` que ele consulta |
| Grava? | o caso de uso recebe `IUnidadeDeTrabalho` (quem grava) |
| Permissão exigida hoje | busca por `ExigirPermissao` / `RequireAuthorization` em `src/` |
| Teste | os caminhos citados em `tests/Tracbel.Crm.Api.Testes` |
| Tela | `src/Tracbel.Crm.Web/src/dados/api/*.ts` e quem importa cada função |
| Fase que altera | as fases do documento 41 e as issues do backlog mestre (46A) |

**São 39 rotas**: 52 chamadas de `Map*` menos os 13 `MapGroup`. O número bate com o C-5 do
documento 46.

---

## 2. A matriz

### 2.1 Cadastro e catálogo — o que grava no banco do CRM

| # | Método · Rota | Arquivo:linha | Caso de uso | Tabelas | Grava? |
|---|---|---|---|---|---|
| 1 | `GET /api/v1/clientes` | `EndpointsDeCliente.cs:23` | `ListarClientes` | `comercial.Cliente`, `metadado.CatalogoItem` | não |
| 2 | `GET /api/v1/clientes/{chave}` | `EndpointsDeCliente.cs:40` | `ObterCliente` | idem | não |
| 3 | `POST /api/v1/clientes` | `EndpointsDeCliente.cs:45` | `CriarCliente` | `comercial.Cliente`, `metadado.CatalogoItem`, `auditoria.AlteracaoDeCampo` | **sim** |
| 4 | `PUT /api/v1/clientes/{chave}` | `EndpointsDeCliente.cs:51` | `AlterarCliente` | idem | **sim** |
| 5 | `DELETE /api/v1/clientes/{chave}` | `EndpointsDeCliente.cs:65` | `InativarCliente` | idem | **sim** (inativa) |
| 6 | `GET /api/v1/equipamentos` | `EndpointsDeEquipamento.cs:17` | `ListarEquipamentos` | `frota.Equipamento`, `frota.Modelo`, `frota.Familia`, `frota.Marca`, `frota.LinhaDeProduto`, `frota.VendaDeMaquina`, `comercial.Cliente`, `integracao.Sistema` | não |
| 7 | `GET /api/v1/equipamentos/{chave}` | `EndpointsDeEquipamento.cs:40` | `ObterEquipamento` | idem | não |
| 8 | `GET /api/v1/equipamentos/{chave}/vendas` | `EndpointsDeEquipamento.cs:45` | `ListarVendasDoEquipamento` | `frota.VendaDeMaquina`, `frota.VinculoDeClienteComEquipamento`, `frota.Equipamento`, `comercial.Cliente`, `organizacao.Empresa`, `integracao.Sistema` | não |
| 9 | `GET /api/v1/clientes/{chave}/maquinas-compradas` | `EndpointsDeEquipamento.cs:54` | `ListarMaquinasCompradasPeloCliente` | idem da 8 | não |
| 10 | `POST /api/v1/equipamentos` | `EndpointsDeEquipamento.cs:61` | `CriarEquipamento` | `frota.Equipamento`, `comercial.Cliente`, `metadado.CatalogoItem`, `integracao.DivergenciaDeIntegracao`, `auditoria.AlteracaoDeCampo` | **sim** |
| 11 | `PUT /api/v1/equipamentos/{chave}` | `EndpointsDeEquipamento.cs:67` | `AlterarEquipamento` | idem | **sim** |
| 12 | `DELETE /api/v1/equipamentos/{chave}` | `EndpointsDeEquipamento.cs:75` | `InativarEquipamento` | `frota.Equipamento`, `auditoria.AlteracaoDeCampo` | **sim** (baixa) |
| 13 | `GET /api/v1/catalogos` | `EndpointsDeCatalogo.cs:23` | `ListarCatalogos` | `metadado.Catalogo`, `metadado.CatalogoItem`, `frota.Modelo`, `frota.Familia`, `frota.Marca`, `frota.LinhaDeProduto`, `organizacao.Empresa` | não |
| 14 | `GET /api/v1/catalogos/{codigo}` | `EndpointsDeCatalogo.cs:28` | `ListarCatalogos` | idem | não |

### 2.2 Leitura de relacionamento, território, integração e vida

| # | Método · Rota | Arquivo:linha | Caso de uso | Tabelas | Grava? |
|---|---|---|---|---|---|
| 15 | `GET /api/v1/processos` | `EndpointsDeRelacionamento.cs:26` | `ListarProcessos` | `processo.Processo`, `processo.TipoProcesso`, `processo.Fase`, `processo.MotivoDePerda`, `comercial.Cliente`, `seguranca.Usuario` | não |
| 16 | `GET /api/v1/processos/{chave}` | `EndpointsDeRelacionamento.cs:45` | `ObterProcesso` | idem | não |
| 17 | `GET /api/v1/tarefas` | `EndpointsDeRelacionamento.cs:60` | `ListarTarefas` | `processo.Tarefa`, `processo.TipoTarefa`, `processo.Resultado`, `processo.Processo`, `comercial.Cliente`, `seguranca.Usuario` | não |
| 18 | `GET /api/v1/interacoes` | `EndpointsDeRelacionamento.cs:89` | `ListarInteracoes` | `processo.Interacao`, `processo.Processo`, `processo.TipoTarefa`, `processo.Resultado`, `comercial.Cliente`, `seguranca.Usuario` | não |
| 19 | `GET /api/v1/cobertura` | `EndpointsDeRelacionamento.cs:112` | `ListarCobertura` | `organizacao.Carteira`, `comercial.ClienteCarteira`, `comercial.Cliente`, `organizacao.LinhaDeNegocio`, `seguranca.Usuario` | não |
| 20 | `GET /api/v1/relatorios/funil` | `EndpointsDeRelacionamento.cs:144` | `ObterFunil` | como a 15 | não |
| 21 | `GET /api/v1/relatorios/perdas` | `EndpointsDeRelacionamento.cs:149` | `ObterPerdas` | como a 15 | não |
| 22 | `GET /api/v1/relatorios/vendas-perdidas` | `EndpointsDeRelacionamento.cs:154` | `ObterVendasPerdidas` | `processo.VendaPerdida`, `processo.MotivoDePerda`, `metadado.CatalogoItem`, + as da 15 | não |
| 23 | `GET /api/v1/relatorios/faturamento` | `EndpointsDeRelacionamento.cs:161` | `ObterFaturamento` | `comercial.FaturamentoDoCliente`, `comercial.Cliente` | não |
| 24 | `GET /api/v1/relatorios/indicadores-executivos` | `EndpointsDeRelacionamento.cs:168` | `ObterIndicadoresExecutivos` | `comercial.FaturamentoDoCliente`, `comercial.FaturamentoSemCliente`, `organizacao.Carteira`, `organizacao.LinhaDeNegocio`, `comercial.ClienteCarteira`, `comercial.Cliente`, `processo.TipoTarefa`, `processo.VendaPerdida` | não |
| 25 | `GET /api/v1/relatorios/cen` | `EndpointsDeRelacionamento.cs:179` | `ObterPainelDoCen` | `organizacao.Carteira`, `seguranca.Usuario`, `organizacao.LinhaDeNegocio`, `comercial.ClienteCarteira`, `comercial.Cliente`, `processo.Processo`, `processo.VendaPerdida`, `comercial.FaturamentoDoCliente` | não |
| 26 | `GET /api/v1/relatorios/agenda` | `EndpointsDeRelacionamento.cs:188` | `ObterPainelDaAgenda` | como a 17 | não |
| 27 | `GET /api/v1/relatorios/cobertura` | `EndpointsDeRelacionamento.cs:193` | `ObterResumoDeCobertura` | como a 19 | não |
| 28 | `GET /api/v1/municipios` | `EndpointsDeTerritorio.cs:28` | `ListarMunicipios` | `organizacao.Municipio` | não |
| 29 | `GET /api/v1/territorio/indicadores` | `EndpointsDeTerritorio.cs:54` | `ObterIndicadoresTerritoriais` | `organizacao.Municipio`, `organizacao.MunicipioDaAreaDeAtuacao`, `organizacao.ResponsavelPeloMunicipio`, `organizacao.AreaPlantadaNoMunicipio`, `organizacao.RegraDePotencial`, `organizacao.Empresa`, `organizacao.Carteira`, `organizacao.LinhaDeNegocio`, `comercial.Cliente`, `comercial.Endereco`, `comercial.ClienteCarteira`, `comercial.FaturamentoDoCliente`, `comercial.FaturamentoSemCliente`, `seguranca.Usuario` | não |
| 30 | `GET /api/v1/cobertura/filiais` | `EndpointsDeTerritorio.cs:84` | `ObterCoberturaPorFilial` | `organizacao.Carteira`, `organizacao.CarteiraMunicipio`, `organizacao.Municipio`, `organizacao.Empresa`, `organizacao.LinhaDeNegocio`, `seguranca.Usuario` | não |
| 31 | `GET /api/v1/cobertura/carteiras` | `EndpointsDeTerritorio.cs:93` | `ListarTerritorioPorCarteira` | idem | não |
| 32 | `GET /api/v1/integracoes/sincronizacoes` | `EndpointsDeSincronizacao.cs:18` | `ListarSincronizacoes` | `integracao.Sistema`, `integracao.ExecucaoDeSincronizacao` | não |
| 33 | `GET /api/v1/legado/clientes` | `EndpointsDoLegado.cs:31` | `BuscarClientesNoLegado` | **nenhuma do CRM** — lê o Vórtice pela ponte, somente leitura | não |
| 34 | `GET /api/v1/legado/clientes/{identificador}/parque` | `EndpointsDoLegado.cs:40` | `ListarParqueNoLegado` | idem | não |
| 35 | `GET /api/v1/legado/saude` | `EndpointsDoLegado.cs:46` | `VerificarPonteDoLegado` | idem | não |
| 36 | `GET /saude/banco` | `Program.cs:331` | — (consulta direta, contexto de sistema) | nenhuma de negócio: `CanConnect` + migrations pendentes | não |
| 37 | `GET /auth/eu` | `RotasDeAutenticacao.cs:90` | `ResolvedorDeContextoDoEntraId` | `seguranca.Usuario`, `organizacao.Empresa` (por `EscopoDeAcesso`) | não |
| 38 | `GET /auth/entrar` | `RotasDeAutenticacao.cs:112` | — (desafio do OpenID Connect) | nenhuma | não |
| 39 | `GET /auth/sair` | `RotasDeAutenticacao.cs:120` | — (encerra cookie e sessão na Microsoft) | nenhuma | não |

### 2.3 Permissão, teste, tela e fase

**A coluna "permissão exigida hoje" é a mesma em 39 de 39 linhas: nenhuma.** Por isso ela não se
repete na tabela — está provada na §3.1. A coluna "proposta" usa o vocabulário que já existe em
`EscopoDeAcesso.PermissoesConcedidas` (`Entidade.Verbo`), e **espera revisão da #46**.

| # | Rota | Permissão proposta | Teste HTTP | Tela / consumidor | Fase que altera |
|---|---|---|---|---|---|
| 1 | `GET /clientes` | `Cliente.Ler` | `EndpointsDeClienteTestes`, `FronteiraDeEmpresaNaApiTestes` | Clientes, Visão 360, `SeletorDeCliente` | 5 (#53) |
| 2 | `GET /clientes/{chave}` | `Cliente.Ler` | idem | ficha de cliente, `Cliente360Api` | 5 (#53) |
| 3 | `POST /clientes` | `Cliente.Criar` | `EndpointsDeClienteTestes`, `AuditoriaAutomaticaNaApiTestes` | cadastro de cliente | 5 (#53) |
| 4 | `PUT /clientes/{chave}` | `Cliente.Editar` | idem | cadastro de cliente | 5 (#53) |
| 5 | `DELETE /clientes/{chave}` | `Cliente.Excluir` | idem | cadastro de cliente | 5 (#53) |
| 6 | `GET /equipamentos` | `Equipamento.Ler` | `IntegracaoDoArtTestes`, `FronteiraDeEmpresaNaApiTestes` | Equipamentos, `Cliente360Api` | 7 (#16) |
| 7 | `GET /equipamentos/{chave}` | `Equipamento.Ler` | `IntegracaoDoArtTestes` | cadastro de equipamento | 7 (#16) |
| 8 | `GET /equipamentos/{chave}/vendas` | `Equipamento.Ler` | `IntegracaoDoArtTestes` | cadastro de equipamento | 7 (#16) |
| 9 | `GET /clientes/{chave}/maquinas-compradas` | `Equipamento.Ler` | `IntegracaoDoArtTestes` | `MaquinasCompradasDoCliente` | 7 (#16) |
| 10 | `POST /equipamentos` | `Equipamento.Criar` | `IntegracaoDoArtTestes` | cadastro de equipamento | 7 (#16) |
| 11 | `PUT /equipamentos/{chave}` | `Equipamento.Editar` | `IntegracaoDoArtTestes` | cadastro de equipamento | 7 (#16) |
| 12 | `DELETE /equipamentos/{chave}` | `Equipamento.Excluir` | `IntegracaoDoArtTestes` | cadastro de equipamento | 7 (#16) |
| 13 | `GET /catalogos` | `Catalogo.Ler` | `EndpointsDeClienteTestes` | todos os formulários (`useCatalogos`) | 4 (#45) |
| 14 | `GET /catalogos/{codigo}` | `Catalogo.Ler` | `EndpointsDeClienteTestes`, `IntegracaoDoArtTestes` | formulários, `listarFiliais` do consolidado | 4 (#45) |
| 15 | `GET /processos` | `Processo.Ler` | `EndpointsDeRelacionamentoTestes` | Pipeline, Funil, `Cliente360Api`, consolidado | 6 (#52) |
| 16 | `GET /processos/{chave}` | `Processo.Ler` | `EndpointsDeRelacionamentoTestes` | ficha de oportunidade | 6 (#52) |
| 17 | `GET /tarefas` | `Tarefa.Ler` | `EndpointsDeRelacionamentoTestes` | Agenda, Visão 360, ficha de oportunidade, `Cliente360Api` | 6 (#52) |
| 18 | `GET /interacoes` | `Interacao.Ler` | `EndpointsDeRelacionamentoTestes` | ficha de oportunidade, `Cliente360Api` | 6 (#52) |
| 19 | `GET /cobertura` | `Carteira.Ler` | `EndpointsDeRelacionamentoTestes` | Cobertura de Carteira, Visão 360 | 9 (#54); a classe e a cadência mudam pela #47 (D-13) |
| 20 | `GET /relatorios/funil` | `Relatorio.Ler` | `EndpointsDeRelacionamentoTestes` | Funil, Pipeline, consolidado | 6 (#52) |
| 21 | `GET /relatorios/perdas` | `Relatorio.Ler` | `EndpointsDeRelacionamentoTestes` | consolidado (painel executivo) | 6 (#52) |
| 22 | `GET /relatorios/vendas-perdidas` | `Relatorio.Ler` | **nenhum** | Funil, consolidado | 6 (#52) |
| 23 | `GET /relatorios/faturamento` | `Relatorio.Ler` | **nenhum** | consolidado (painel executivo) | 8 (#19) |
| 24 | `GET /relatorios/indicadores-executivos` | `Relatorio.Ler` | `IndicadoresExecutivosTestes` | painel executivo da Visão 360 | 8 (#19) |
| 25 | `GET /relatorios/cen` | `Relatorio.Ler` | **nenhum** | Performance de CEN | 6 (#52) |
| 26 | `GET /relatorios/agenda` | `Tarefa.Ler` | `EndpointsDeRelacionamentoTestes` | Agenda, Visão 360, consolidado | 6 (#52) |
| 27 | `GET /relatorios/cobertura` | `Carteira.Ler` | `EndpointsDeRelacionamentoTestes` | Cobertura de Carteira, Cobertura por Filial, Performance de CEN, consolidado | 9 (#54); idem #47 |
| 28 | `GET /municipios` | `Municipio.Ler` | **nenhum** | **nenhuma** (ver §3.3) | 9 (#54) |
| 29 | `GET /territorio/indicadores` | `Territorio.Ler` | `IndicadoresTerritoriaisTestes` | Indicadores Geográficos | 9 (#54) |
| 30 | `GET /cobertura/filiais` | `Carteira.Ler` | **nenhum** | Cobertura por Filial | 9 (#54) |
| 31 | `GET /cobertura/carteiras` | `Carteira.Ler` | **nenhum** | Cobertura por Filial | 9 (#54) |
| 32 | `GET /integracoes/sincronizacoes` | `Integracao.Ler` (administração) | `IntegracaoDoArtTestes` | Configurações › Integrações | 10 (#55) |
| 33 | `GET /legado/clientes` | `Legado.Ler` | `EndpointsDeClienteTestes` | **nenhuma** (ver §3.3) | 8 (#20) |
| 34 | `GET /legado/clientes/{id}/parque` | `Legado.Ler` | `EndpointsDeClienteTestes` | **nenhuma** | 8 (#20) |
| 35 | `GET /legado/saude` | `Legado.Ler` | `EndpointsDeClienteTestes` | **nenhuma** | 8 (#20) |
| 36 | `GET /saude/banco` | nenhuma, de propósito (prova de vida) | `SaudeTestes` | script de subida e CD (#62) | — |
| 37 | `GET /auth/eu` | autenticado, sem permissão | `AutenticacaoTestes` | `ProvedorDeSessao` (toda tela) | 3 (#46) |
| 38 | `GET /auth/entrar` | anônima, de propósito | `AutenticacaoTestes` | tela de login | 3 (#46) |
| 39 | `GET /auth/sair` | autenticado | **nenhum** | menu lateral (`Sair`) | 3 (#46) |

---

## 3. Os achados

### 3.1 Zero rotas exigem permissão — e o motor existe

`Autorizador.ExigirPermissao` está escrito em `src/Tracbel.Crm.Dominio/Seguranca/Autorizador.cs:143`
e **ninguém o chama**: a busca por `ExigirPermissao`, `RequireAuthorization` e `TemPermissao` em
`src/` devolve **uma única ocorrência, a própria definição**. O que existe hoje é:

- `EscopoDeAcesso.PermissoesConcedidas` — dez permissões (`Cliente.*`, `Equipamento.*`,
  `Catalogo.Ler`, `Lead.Ler`) concedidas a **todo usuário**, montadas no contexto de acesso;
- a **fronteira de multiempresa**, essa sim aplicada, pelo filtro global do `CrmDbContext` — mas
  ela responde "de qual filial", não "quem pode".

Ou seja: qualquer um que alcance a porta faz as seis operações de escrita e lê os 39 endpoints. É a
issue **#46**, e é o motivo de a coluna "permissão proposta" existir nesta matriz.

### 3.2 Sete rotas sem nenhum teste HTTP

`/api/v1/relatorios/vendas-perdidas`, `/api/v1/relatorios/faturamento`, `/api/v1/relatorios/cen`,
`/api/v1/cobertura/filiais`, `/api/v1/cobertura/carteiras`, `/api/v1/municipios` e `/auth/sair`.

Três delas sustentam tela em uso hoje (Performance de CEN, Cobertura por Filial e o painel
executivo). **32 de 39 têm teste** — 82%.

### 3.3 Quatro rotas sem consumidor na tela

| Rota | Situação |
|---|---|
| `/api/v1/legado/clientes`, `/legado/clientes/{id}/parque`, `/legado/saude` | nenhuma tela chama; a ponte é exercitada por teste e por chamada manual. É a **#6** (rotas sem consumidor) e depende da decisão sobre a aposentadoria do Vórtice (#20) |
| `/api/v1/municipios` | tem cliente no front (`relacionamento.ts:334`, `listarMunicipios`) e **nenhuma tela o importa** — o campo de município do endereço, que é a razão declarada da rota no documento 23 §2.6, ainda não a usa |

`/saude/banco` não tem tela de propósito: quem a consome é o roteiro de subida.

### 3.4 O documento 23 descreve 28 das 39 rotas

Onze rotas existem no código e **não estão na §2 do documento 23**:

| Rota | Onde deveria entrar |
|---|---|
| `GET /api/v1/equipamentos/{chave}/vendas` | §2.2 — a seção diz "mesmas cinco operações" e são sete |
| `GET /api/v1/clientes/{chave}/maquinas-compradas` | §2.1 ou §2.2 |
| `GET /api/v1/relatorios/vendas-perdidas` | §2.5 |
| `GET /api/v1/relatorios/faturamento` | §2.5 |
| `GET /api/v1/relatorios/cen` | §2.5 |
| `GET /api/v1/relatorios/indicadores-executivos` | §2.5 |
| `GET /api/v1/territorio/indicadores` | §2.6 |
| `GET /api/v1/integracoes/sincronizacoes` | seção nova, de administração |
| `GET /auth/eu`, `GET /auth/entrar`, `GET /auth/sair` | §2.7, ao lado da vida |

Mais duas divergências de conteúdo:

1. **§2 abre com "todas exigem os dois cabeçalhos de contexto de acesso"** — `/saude/banco` (a
   própria §2.7 diz) e as três de `/auth` ficam fora do meio de campo que lê os cabeçalhos.
2. **§2.6 mostra `codigoIbge: null` no exemplo de `/api/v1/municipios`**, com a justificativa de que
   `GE_Cidade` não tem a coluna. Depois da carga de território de 14/09/2026 o catálogo tem código
   IBGE (documento 32: 5.571 municípios reconhecidos) — o exemplo envelheceu. **A conferir no banco**
   antes de corrigir o texto: nesta rodada o banco não respondeu desta estação.

Corrigir o documento 23 é a **#4** (padronizar o OpenAPI) e a **#5** (catálogo técnico); esta matriz
é a lista de trabalho das duas.

---

## 4. A proposta que vai para a #46: um teste de arquitetura

Enquanto a autorização não existir, a matriz envelhece no dia em que alguém acrescentar uma rota.
O que impede isso é um teste de arquitetura no mesmo espírito dos que já existem:

> **"Toda rota declara a permissão que exige."** O teste varre os endpoints registrados
> (`EndpointDataSource`), e falha quando um deles não traz os metadados de permissão — seja a
> permissão exigida, seja a marca explícita de "anônima de propósito", que é o caso de
> `/saude/banco` e `/auth/entrar`.

Ele nasce junto com a aplicação da permissão (#46), não antes: um teste que exige metadado sem nada
que o leia seria cerimônia.

---

## 5. O que falta

1. **A coluna "permissão proposta" precisa da revisão de quem decide a #46** — os nomes seguem o
   vocabulário existente, mas quem pode o quê é decisão do negócio.
2. **O município no formulário de endereço** (§3.3): a rota existe e a tela não a usa.
3. **A conferência do `codigoIbge`** no exemplo do documento 23, quando o banco responder.
