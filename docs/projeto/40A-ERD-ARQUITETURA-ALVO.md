# 40A — Diagrama ER da arquitetura alvo

> Anexo do [documento 40](40-ARQUITETURA-ALVO-DO-BANCO.md). **Proposta — nada foi alterado.**
> Mostra somente o modelo recomendado. O modelo atual está no [anexo 39B](39B-DIAGRAMA-ER-ATUAL.md).
> Versão 1.0 · 15/09/2026.

## 1. Atual × alvo

| Medida | Atual | Alvo | Diferença |
|---|---:|---:|---|
| Tabelas físicas | **82** | **42** | −40 |
| Tabelas do modelo EF (domínio) | 80 | 41 | −39 |
| Tabelas técnicas | 2 | 1 | sai `dbo.__EFMigrationsHistory` |
| Chaves estrangeiras | **213** | **86** | −127 |
| FKs para `Empresa` | 32 | 18 | |
| FKs para `Usuario` | 32 | 7 | autoria (`CriadoPorId`, `ImportadoPorId`...) deixa de ser FK |
| FKs para `Cliente` | 18 | 12 | |
| FKs compostas de catálogo | 13 | 6 | |
| Auto-referências | 7 | 4 | |
| Ciclos | 1 | **0** | |
| Tabelas polimórficas (`Entidade`, `RegistroId`) | 10 | 2 | `RegistroDeOrigem`, `AlteracaoDeCampo` |
| Tabelas particionadas | 4 | 1 | `AlteracaoDeCampo` |
| Schemas | 10 + `dbo` | 8 | saem `documento`, `relatorio` e `dbo` |
| Tabelas que nunca tiveram linha | 35 | 0 | |

**Regra de FK do alvo.** FK existe para relação de negócio. Coluna de autoria (`CriadoPorId`,
`AlteradoPorId`, `ConcedidoPorId`, `VinculadoPorId`, `RevisadaPorId`) é referência lógica, sem FK, em
todas as tabelas — hoje metade tem e metade não. Tabela de log (`AlteracaoDeCampo`) não tem FK.

## 2. Visão geral por domínio

Números nas setas = FKs de um domínio para outro. FKs internas entre parênteses.

```mermaid
flowchart LR
    IDN["IDENTIDADE E ACESSO<br/>4 tabelas (4)"]
    ORG["ORGANIZAÇÃO<br/>1 tabela (1)"]
    TER["TERRITÓRIO<br/>5 tabelas (3)"]
    CRM["CRM<br/>5 tabelas (5)"]
    CFG["CONFIGURAÇÃO COMERCIAL<br/>6 tabelas (3)"]
    ATV["ATIVIDADES<br/>2 tabelas (1)"]
    COM["COMERCIAL<br/>3 tabelas (1)"]
    PAR["PARQUE DE MÁQUINAS<br/>2 tabelas (1)"]
    CAT["CATÁLOGO<br/>6 tabelas (5)"]
    INT["INTEGRAÇÕES<br/>6 tabelas (5)"]
    AUD["AUDITORIA<br/>1 tabela (0)"]

    IDN -- 1 --> ORG
    TER -- 1 --> ORG
    TER -- 1 --> CRM
    CRM -- 4 --> ORG
    CRM -- 2 --> IDN
    CRM -- 1 --> TER
    CRM -- 1 --> CFG
    CRM -- 3 --> CAT
    ATV -- 2 --> ORG
    ATV -- 2 --> IDN
    ATV -- 4 --> CRM
    ATV -- 3 --> CFG
    ATV -- 2 --> COM
    COM -- 3 --> ORG
    COM -- 1 --> IDN
    COM -- 5 --> CRM
    COM -- 3 --> CFG
    COM -- 3 --> CAT
    PAR -- 3 --> ORG
    PAR -- 2 --> CRM
    PAR -- 2 --> CAT
    INT -- 3 --> ORG
    INT -- 1 --> CRM
    INT -- 2 --> PAR
    INT -- 2 --> CAT
```

Conferência: 29 FKs internas + 57 entre domínios = **86**.

A integração aponta para o domínio; **nenhuma tabela de domínio aponta para a integração.** Hoje
`VendaDeMaquina` e o vínculo apontam para `Sistema`.

## 3. Identidade e acesso — 4 tabelas, 5 FKs

```mermaid
erDiagram
    Usuario {
        bigint Id PK
        uniqueidentifier IdentidadeExterna UK
        nvarchar NomePrincipal UK
        nvarchar NomeExibicao
        int EmpresaId FK "filial de casa"
        bigint GestorId FK "única hierarquia"
        varchar Natureza "Pessoa, Departamento, Sistema"
        bit EstaAtivo
    }
    Perfil {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        bit EhPadrao "vale para todo usuário ativo"
        bit EstaAtivo
    }
    PerfilPermissao {
        int Id PK
        int PerfilId FK
        varchar CodigoPermissao "catálogo em código"
        varchar Profundidade
    }
    UsuarioPerfil {
        bigint Id PK
        bigint UsuarioId FK
        int PerfilId FK
        datetime2 ConcedidoEm
        datetime2 ExpiraEm
    }
    Empresa ||--o{ Usuario : "EmpresaId"
    Usuario |o--o{ Usuario : "GestorId"
    Perfil ||--o{ PerfilPermissao : "PerfilId"
    Usuario ||--o{ UsuarioPerfil : "UsuarioId"
    Perfil ||--o{ UsuarioPerfil : "PerfilId"
```

## 4. Organização e território — 6 tabelas, 6 FKs

```mermaid
erDiagram
    Empresa {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        int EmpresaPaiId FK
        varchar Caminho "caminho materializado"
        bit EstaAtiva
    }
    Municipio {
        int Id PK
        int CodigoIbge UK "obrigatório"
        nvarchar Nome
        char Uf
    }
    MunicipioDaAreaDeAtuacao {
        int Id PK
        int MunicipioId FK
        int EmpresaResponsavelId FK "filial responsável"
        bit PertenceAAdr
        varchar Regiao
        datetime2 EncerradoEm
    }
    CarteiraMunicipio {
        bigint Id PK
        bigint CarteiraId FK "CEN por linha"
        int MunicipioId FK
        datetime2 VinculadoEm
        datetime2 DesvinculadoEm
    }
    AreaPlantadaNoMunicipio {
        bigint Id PK
        int MunicipioId FK
        smallint Ano
        int ProdutoCodigoIbge
        decimal AreaPlantadaHectares
    }
    RegraDePotencial {
        int Id PK
        int ProdutoCodigoIbge
        decimal HectaresPorMaquina
        nvarchar ModeloDeReferencia
        varchar Situacao
    }
    Empresa |o--o{ Empresa : "EmpresaPaiId"
    Municipio ||--o{ MunicipioDaAreaDeAtuacao : "MunicipioId"
    Empresa |o--o{ MunicipioDaAreaDeAtuacao : "EmpresaResponsavelId"
    Carteira ||--o{ CarteiraMunicipio : "CarteiraId"
    Municipio ||--o{ CarteiraMunicipio : "MunicipioId"
    Municipio ||--o{ AreaPlantadaNoMunicipio : "MunicipioId"
```

## 5. CRM — 5 tabelas, 16 FKs

```mermaid
erDiagram
    Cliente {
        bigint Id PK
        int EmpresaId FK
        nvarchar NomeRazao
        varchar TipoDePessoa
        varchar Documento UK "por empresa"
        varchar Situacao "Suspect, Prospect, Cliente, ClienteInativo, Encerrado"
        bigint ProprietarioId FK
        bigint ClienteMatrizId FK
        int OrigemId FK "catálogo"
        int MotivoInativacaoId FK "catálogo"
        varchar Classe "apurada do faturamento"
        datetime2 ClasseApuradaEm
    }
    Contato {
        bigint Id PK
        int EmpresaId FK
        bigint ClienteId FK
        nvarchar Nome
        int PapelId FK "catálogo"
        nvarchar Email
        varchar Telefone
        varchar Celular
    }
    Endereco {
        bigint Id PK
        int EmpresaId FK
        bigint ClienteId FK
        varchar Tipo
        nvarchar Logradouro
        int MunicipioId FK "obrigatório"
        char Cep
        decimal Latitude
        decimal Longitude
        bit EhPrincipal
    }
    Carteira {
        bigint Id PK
        int EmpresaId FK
        int LinhaDeNegocioId FK
        varchar Codigo UK
        nvarchar Nome
        bigint ResponsavelId FK "o CEN"
        varchar Natureza
        bit EstaAtiva
    }
    ClienteCarteira {
        bigint Id PK
        bigint ClienteId FK
        bigint CarteiraId FK
        datetime2 VinculadoEm
        datetime2 DesvinculadoEm
    }
    Empresa ||--o{ Cliente : "EmpresaId"
    Usuario ||--o{ Cliente : "ProprietarioId"
    Cliente |o--o{ Cliente : "ClienteMatrizId"
    CatalogoItem |o--o{ Cliente : "OrigemId"
    CatalogoItem |o--o{ Cliente : "MotivoInativacaoId"
    Empresa ||--o{ Contato : "EmpresaId"
    Cliente ||--o{ Contato : "ClienteId"
    CatalogoItem |o--o{ Contato : "PapelId"
    Empresa ||--o{ Endereco : "EmpresaId"
    Cliente ||--o{ Endereco : "ClienteId"
    Municipio ||--o{ Endereco : "MunicipioId"
    Empresa ||--o{ Carteira : "EmpresaId"
    LinhaDeNegocio ||--o{ Carteira : "LinhaDeNegocioId"
    Usuario ||--o{ Carteira : "ResponsavelId"
    Cliente ||--o{ ClienteCarteira : "ClienteId"
    Carteira ||--o{ ClienteCarteira : "CarteiraId"
```

## 6. Configuração comercial — 6 tabelas, 3 FKs

Tudo aqui é administrado pela área de configuração do CRM, com semente mínima estrutural.

```mermaid
erDiagram
    LinhaDeNegocio {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        smallint DiasCicloClasseA
        smallint DiasCicloClasseB
        smallint DiasCicloClasseC
        smallint DiasCicloClasseD
        bit EstaAtiva
    }
    Pipeline {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        int LinhaDeNegocioId FK
        bit EstaAtivo
    }
    Etapa {
        int Id PK
        int PipelineId FK
        varchar Codigo
        nvarchar Nome
        smallint Ordem
        smallint ProbabilidadePercentual
        bit EhFinal
    }
    TipoDeAtividade {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        varchar Categoria
        smallint PrazoDiasUteis
        bit ContaParaCobertura
        varchar Cor
    }
    ResultadoDeAtividade {
        int Id PK
        int TipoDeAtividadeId FK
        varchar Codigo
        nvarchar Nome
        varchar Classe
    }
    MotivoDePerda {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        varchar Categoria
        bit ExigeConcorrente
        bit ExigeObservacao
    }
    LinhaDeNegocio |o--o{ Pipeline : "LinhaDeNegocioId"
    Pipeline ||--o{ Etapa : "PipelineId"
    TipoDeAtividade ||--o{ ResultadoDeAtividade : "TipoDeAtividadeId"
```

## 7. Atividades — 2 tabelas, 14 FKs, sem ciclo

A **Agenda não é tabela**: é a leitura de `Tarefa` por responsável e data.

```mermaid
erDiagram
    Tarefa {
        bigint Id PK
        int EmpresaId FK
        bigint OportunidadeId FK
        bigint ClienteId FK
        bigint ContatoId FK
        int TipoDeAtividadeId FK
        nvarchar Assunto
        bigint ResponsavelId FK
        datetime2 AgendadaPara
        datetime2 PrazoLimite
        smallint Prioridade
        varchar Situacao "Pendente, EmAndamento, Concluida, Cancelada"
        datetime2 ConcluidaEm
    }
    Interacao {
        bigint Id PK
        int EmpresaId FK
        int TipoDeAtividadeId FK
        bigint OportunidadeId FK
        bigint ClienteId FK
        bigint ContatoId FK
        bigint TarefaId FK "a tarefa que ela realizou"
        int ResultadoId FK
        varchar Natureza "Ativa, Receptiva, Sistema"
        datetime2 OcorridaEm
        bigint RegistradoPorId FK
    }
    Empresa ||--o{ Tarefa : "EmpresaId"
    Oportunidade |o--o{ Tarefa : "OportunidadeId"
    Cliente |o--o{ Tarefa : "ClienteId"
    Contato |o--o{ Tarefa : "ContatoId"
    TipoDeAtividade ||--o{ Tarefa : "TipoDeAtividadeId"
    Usuario ||--o{ Tarefa : "ResponsavelId"
    Empresa ||--o{ Interacao : "EmpresaId"
    TipoDeAtividade ||--o{ Interacao : "TipoDeAtividadeId"
    Oportunidade |o--o{ Interacao : "OportunidadeId"
    Cliente |o--o{ Interacao : "ClienteId"
    Contato |o--o{ Interacao : "ContatoId"
    Tarefa |o--o{ Interacao : "TarefaId"
    ResultadoDeAtividade |o--o{ Interacao : "ResultadoId"
    Usuario ||--o{ Interacao : "RegistradoPorId"
```

## 8. Comercial — 3 tabelas, 16 FKs

```mermaid
erDiagram
    Oportunidade {
        bigint Id PK
        bigint Numero UK "por empresa"
        int EmpresaId FK
        int PipelineId FK
        int EtapaId FK
        datetime2 EtapaDesde
        bigint ClienteId FK
        bigint ContatoId FK
        bigint CarteiraId FK "carteira de origem"
        nvarchar Titulo
        varchar Situacao "Aberta, Suspensa, Ganha, Perdida, Cancelada"
        decimal ValorEstimado
        decimal ValorFinal
        date PrevisaoConclusao
        date PrevisaoConclusaoOriginal
        datetime2 ConcluidaEm
        bigint ProprietarioId FK
    }
    VendaPerdida {
        bigint Id PK
        int EmpresaId FK
        bigint OportunidadeId FK "único, anulável"
        bigint ClienteId FK
        int MotivoDePerdaId FK
        nvarchar Observacao
        int ConcorrenteId FK "catálogo"
        int RevendaDoConcorrenteId FK "catálogo"
        int TipoDeEquipamentoId FK "catálogo"
        decimal PrecoDoConcorrente
        decimal PrecoOfertado
        date OcorridaEm
    }
    Faturamento {
        bigint Id PK
        int EmpresaId FK
        date Competencia
        varchar Documento "da nota; chave do grão"
        bigint ClienteId FK "anulável"
        varchar Natureza "Cliente, ClienteNaoCadastrado, Fabrica, EmpresaDoGrupo, OutraRevenda, SemDocumento"
        nvarchar NomeNaNota
        decimal ValorLiquido
        decimal ValorEmMaquina
        decimal ValorEmPeca
        decimal ValorEmServico
        decimal ValorEmOutros
        int Notas
        int Itens
    }
    Empresa ||--o{ Oportunidade : "EmpresaId"
    Pipeline ||--o{ Oportunidade : "PipelineId"
    Etapa ||--o{ Oportunidade : "EtapaId"
    Cliente ||--o{ Oportunidade : "ClienteId"
    Contato |o--o{ Oportunidade : "ContatoId"
    Carteira |o--o{ Oportunidade : "CarteiraId"
    Usuario ||--o{ Oportunidade : "ProprietarioId"
    Empresa ||--o{ VendaPerdida : "EmpresaId"
    Oportunidade |o--o| VendaPerdida : "OportunidadeId"
    Cliente ||--o{ VendaPerdida : "ClienteId"
    MotivoDePerda ||--o{ VendaPerdida : "MotivoDePerdaId"
    CatalogoItem |o--o{ VendaPerdida : "ConcorrenteId"
    CatalogoItem |o--o{ VendaPerdida : "RevendaDoConcorrenteId"
    CatalogoItem |o--o{ VendaPerdida : "TipoDeEquipamentoId"
    Empresa ||--o{ Faturamento : "EmpresaId"
    Cliente |o--o{ Faturamento : "ClienteId"
```

## 9. Parque de máquinas — 2 tabelas, 8 FKs

```mermaid
erDiagram
    Equipamento {
        bigint Id PK
        int EmpresaId FK
        bigint ClienteId FK "dono atual; nulo = estoque"
        int ModeloId FK
        int ClassificacaoDeProdutoId FK "só sem modelo"
        varchar Chassi UK
        varchar NumeroSerie
        smallint AnoFabricacao
        decimal HorimetroAtual
        nvarchar LocalizacaoDescrita
        varchar Situacao
    }
    VendaDeMaquina {
        bigint Id PK
        int EmpresaId FK "filial que vendeu"
        int EmpresaDoFaturamentoId FK
        bigint EquipamentoId FK
        bigint CompradorId FK "comprador nesta venda"
        date VendidaEm
        date FaturadaEm
        date EntregueEm
        varchar NumeroDoPedido
        varchar NumeroDaNotaFiscal
    }
    Empresa ||--o{ Equipamento : "EmpresaId"
    Cliente |o--o{ Equipamento : "ClienteId"
    Modelo |o--o{ Equipamento : "ModeloId"
    ClassificacaoDeProduto |o--o{ Equipamento : "ClassificacaoDeProdutoId"
    Empresa ||--o{ VendaDeMaquina : "EmpresaId"
    Empresa |o--o{ VendaDeMaquina : "EmpresaDoFaturamentoId"
    Equipamento ||--o{ VendaDeMaquina : "EquipamentoId"
    Cliente ||--o{ VendaDeMaquina : "CompradorId"
```

## 10. Catálogo — 6 tabelas, 5 FKs

```mermaid
erDiagram
    Catalogo {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
    }
    CatalogoItem {
        int Id PK
        int CatalogoId FK
        varchar Codigo
        nvarchar Descricao
        int ItemPaiId FK
        bit EstaAtivo
    }
    Marca {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        bit EhRepresentada
    }
    Familia {
        int Id PK
        int MarcaId FK
        varchar Codigo
        nvarchar Nome
    }
    Modelo {
        int Id PK
        int FamiliaId FK
        int ClassificacaoDeProdutoId FK
        varchar Codigo UK
        nvarchar Nome
        smallint PotenciaCv
    }
    ClassificacaoDeProduto {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        varchar Porte
    }
    Catalogo ||--o{ CatalogoItem : "CatalogoId"
    CatalogoItem |o--o{ CatalogoItem : "CatalogoId,ItemPaiId"
    Marca ||--o{ Familia : "MarcaId"
    Familia ||--o{ Modelo : "FamiliaId"
    ClassificacaoDeProduto |o--o{ Modelo : "ClassificacaoDeProdutoId"
```

## 11. Integrações — 6 tabelas, 13 FKs

```mermaid
erDiagram
    Sistema {
        int Id PK
        varchar Codigo UK
        nvarchar Nome
        bit EstaAtivo
    }
    RegistroDeOrigem {
        bigint Id PK
        int SistemaId FK
        varchar Fluxo
        varchar ChaveOrigem "UK com sistema e fluxo"
        varchar Entidade "entidade interna, quando houver"
        bigint RegistroId "registro interno, quando houver"
        varchar HashDoConteudo
        varchar Decisao "Importado, Pendente, Rejeitado"
        varchar Motivos
        nvarchar Dados "JSON com o que só a origem entende"
        datetime2 PrimeiraLeituraEm
        datetime2 UltimaLeituraEm
        datetime2 AusenteNaOrigemDesde
    }
    CorrespondenciaDaOrigem {
        int Id PK
        int SistemaId FK
        varchar Tipo
        varchar CodigoNaOrigem
        int ClassificacaoDeProdutoId FK
        int ModeloId FK
        int EmpresaCorrespondenteId FK
        varchar Situacao
    }
    DivergenciaDeIntegracao {
        bigint Id PK
        int EmpresaId FK
        int SistemaId FK
        varchar Tipo
        bigint EquipamentoId FK
        bigint VendaDeMaquinaId FK
        varchar Situacao
    }
    ExecucaoDeSincronizacao {
        bigint Id PK
        int SistemaId FK
        varchar Fluxo
        datetime2 IniciadaEm
        datetime2 TerminadaEm
        varchar Resultado
        varchar UltimoValorLido "marca d'água"
        int RegistrosLidos
        int Incluidos
        int Atualizados
        int Pendentes
    }
    PendenciaDeCadastro {
        bigint Id PK
        int EmpresaId FK
        int SistemaId FK
        varchar Documento
        nvarchar NomeNaOrigem
        varchar Situacao "AguardandoCadastro, Cadastrado, Descartada"
        bigint ClienteId FK "quando cadastrado"
        datetime2 UltimaOcorrenciaEm
    }
    Sistema ||--o{ RegistroDeOrigem : "SistemaId"
    Sistema ||--o{ CorrespondenciaDaOrigem : "SistemaId"
    ClassificacaoDeProduto |o--o{ CorrespondenciaDaOrigem : "ClassificacaoDeProdutoId"
    Modelo |o--o{ CorrespondenciaDaOrigem : "ModeloId"
    Empresa |o--o{ CorrespondenciaDaOrigem : "EmpresaCorrespondenteId"
    Empresa ||--o{ DivergenciaDeIntegracao : "EmpresaId"
    Sistema ||--o{ DivergenciaDeIntegracao : "SistemaId"
    Equipamento |o--o{ DivergenciaDeIntegracao : "EquipamentoId"
    VendaDeMaquina |o--o{ DivergenciaDeIntegracao : "VendaDeMaquinaId"
    Sistema ||--o{ ExecucaoDeSincronizacao : "SistemaId"
    Empresa ||--o{ PendenciaDeCadastro : "EmpresaId"
    Sistema ||--o{ PendenciaDeCadastro : "SistemaId"
    Cliente |o--o{ PendenciaDeCadastro : "ClienteId"
```

## 12. Auditoria — 1 tabela, 0 FKs

```mermaid
erDiagram
    AlteracaoDeCampo {
        bigint Id PK
        datetime2 AlteradoEm PK "partição mensal"
        int EmpresaId
        varchar Entidade
        bigint RegistroId
        varchar Operacao "Criacao, Alteracao, Exclusao"
        varchar Campo
        nvarchar ValorAnterior
        nvarchar ValorNovo
        bigint AlteradoPorId
        varchar Origem "Usuario, Integracao, Importacao, Sistema, Job"
        int SistemaId
        uniqueidentifier CorrelacaoId
    }
```

## 13. Contagem das FKs previstas

| Domínio | Tabelas | FKs de saída |
|---|---:|---:|
| Identidade e acesso | 4 | 5 |
| Organização | 1 | 1 |
| Território | 5 | 5 |
| CRM | 5 | 16 |
| Configuração comercial | 6 | 3 |
| Atividades | 2 | 14 |
| Comercial | 3 | 16 |
| Parque de máquinas | 2 | 8 |
| Catálogo | 6 | 5 |
| Integrações | 6 | 13 |
| Auditoria | 1 | 0 |
| **Total** | **41** | **86** |

Mais a tabela técnica `metadado.__EFMigrationsHistory` = **42 tabelas físicas**.
