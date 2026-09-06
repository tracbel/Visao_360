# Dicionário de dados — CRM Tracbel

> Gerado automaticamente por `scripts/banco/gerar-dicionario-crm.ps1` a partir do
> modelo do EF Core (`Tracbel.Crm.Infraestrutura.Persistencia.CrmDbContext`).
> **Não edite à mão** — rode o gerador de novo depois de mudar o modelo.
>
> Padrão de banco (schema, nomenclatura, tipos, integridade):
> [14-PADRAO-DE-BANCO](../projeto/14-PADRAO-DE-BANCO.md). O modelo completo,
> tabela a tabela: [17-MODELO-UNIFICADO](../projeto/17-MODELO-UNIFICADO.md), seção 8.
>
> Banco: **SQL Server**, com nomes em PascalCase — as palavras do documento 15,
> letra por letra iguais em C# e no banco. Decisão em
> [20-DECISAO-SQL-SERVER](../projeto/20-DECISAO-SQL-SERVER.md).
>
> Quatro tabelas são PARTICIONADAS POR MÊS e por isso têm a data na chave primária:
> `auditoria.AlteracaoDeCampo`, `auditoria.EventoDeAcesso`,
> `processo.RegraExecucao` e `integracao.Recepcao`. A coluna `Versao`
> (`rowversion`) faz o controle de concorrência otimista, gerada pelo banco.

Gerado em 2026-09-04 16:23 — 10 schema(s), 63 tabela(s), 754 coluna(s).

---

## Schema `auditoria`

### `auditoria.AlteracaoDeCampo`

**Check constraints:**
- `CK_AlteracaoDeCampo_Campo`: `[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'`
- `CK_AlteracaoDeCampo_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_AlteracaoDeCampo_Mudou`: `([ValorAnterior] IS NOT NULL OR [ValorNovo] IS NOT NULL) AND ([ValorAnterior] IS NULL OR [ValorNovo] IS NULL OR [ValorAnterior] <> [ValorNovo])`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | não | **PK** |  |  |  |
| `AlteradoPorId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `Campo` | `varchar(60)` | não |  |  |  |  |
| `CorrelacaoId` | `uniqueidentifier` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `Entidade` | `varchar(40)` | não |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `RegistroId` | `bigint` | não |  |  |  |  |
| `ValorAnterior` | `nvarchar(400)` | sim |  |  |  |  |
| `ValorNovo` | `nvarchar(400)` | sim |  |  |  |  |

### `auditoria.CampoAuditado`

**Check constraints:**
- `CK_CampoAuditado_Campo`: `[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'`
- `CK_CampoAuditado_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_CampoAuditado_Retencao`: `[RetencaoMeses] BETWEEN 1 AND 120`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Campo` | `varchar(60)` | não |  |  | sim |  |
| `Entidade` | `varchar(40)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `RetencaoMeses` | `smallint` | não |  |  |  |  |

### `auditoria.EventoDeAcesso`

**Check constraints:**
- `CK_EventoDeAcesso_Entidade`: `[Entidade] IS NULL OR ([Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo'))`
- `CK_EventoDeAcesso_Registro`: `([Entidade] IS NULL AND [RegistroId] IS NULL) OR ([Entidade] IS NOT NULL AND [RegistroId] IS NOT NULL)`
- `CK_EventoDeAcesso_Tipo`: `[Tipo] IN ('Login','LoginFalhou','Logout','AcessoNegado','ExportacaoDados','LeituraDadoSensivel')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AgenteUsuario` | `nvarchar(400)` | sim |  |  |  |  |
| `Detalhe` | `nvarchar(1000)` | sim |  |  |  |  |
| `EnderecoIp` | `varchar(45)` | sim |  |  |  |  |
| `Entidade` | `varchar(40)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `NomePrincipal` | `nvarchar(200)` | sim |  |  |  |  |
| `OcorreuEm` | `datetime2(3)` | não | **PK** |  |  |  |
| `RegistroId` | `bigint` | sim |  |  |  |  |
| `Tipo` | `varchar(30)` | não |  |  |  |  |
| `UsuarioId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |

---

## Schema `comercial`

### `comercial.Alerta`

**Check constraints:**
- `CK_Alerta_Severidade`: `[Severidade] IN ('Informativo','Atencao','Critico')`
- `CK_Alerta_Vigencia`: `[VigenteAte] IS NULL OR [VigenteAte] >= [VigenteDe]`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteId` | `bigint` | não |  | **FK** |  | → `comercial.Cliente` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Detalhe` | `nvarchar(2000)` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Severidade` | `varchar(20)` | não |  |  |  |  |
| `Titulo` | `nvarchar(200)` | não |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |
| `VigenteAte` | `date` | sim |  |  |  |  |
| `VigenteDe` | `date` | não |  |  |  |  |

### `comercial.CanalContato`

**Check constraints:**
- `CK_CanalContato_Tipo`: `[Tipo] IN ('Email','Telefone','Celular','WhatsApp')`
- `CK_CanalContato_UmDono`: `([ClienteId] IS NOT NULL AND [ContatoId] IS NULL) OR ([ClienteId] IS NULL AND [ContatoId] IS NOT NULL)`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `ContatoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `EhPrincipal` | `bit` | não |  |  |  |  |
| `EhValido` | `bit` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Rotulo` | `nvarchar(40)` | sim |  |  |  |  |
| `Tipo` | `varchar(20)` | não |  |  |  |  |
| `ValidadoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Valor` | `nvarchar(200)` | não |  |  |  |  |
| `ValorNormalizado` | `varchar(200)` | não |  |  |  |  |

### `comercial.Cliente`

**Check constraints:**
- `CK_Cliente_CatalogoDaOrigemId`: `[CatalogoDaOrigemId] = 2`
- `CK_Cliente_CatalogoDoMotivoInativacaoId`: `[CatalogoDoMotivoInativacaoId] = 3`
- `CK_Cliente_Documento`: `[Documento] IS NULL OR ([TipoDePessoa] = 'Fisica' AND LEN([Documento]) = 11) OR ([TipoDePessoa] = 'Juridica' AND LEN([Documento]) = 14)`
- `CK_Cliente_Situacao`: `[Situacao] IN ('Suspect','Prospect','Cliente','ClienteInativo','Encerrado')`
- `CK_Cliente_TipoDePessoa`: `[TipoDePessoa] IN ('Fisica','Juridica')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `AtividadeEconomica` | `varchar(7)` | sim |  |  |  |  |
| `CatalogoDaOrigemId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `CatalogoDoMotivoInativacaoId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteMatrizId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Documento` | `varchar(14)` | sim |  |  | sim |  |
| `EmpresaId` | `int` | não |  | **FK** | sim | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `InscricaoEstadual` | `varchar(20)` | sim |  |  |  |  |
| `MotivoInativacaoId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `NomeFantasia` | `nvarchar(200)` | sim |  |  |  |  |
| `NomeRazao` | `nvarchar(200)` | não |  |  |  |  |
| `OrigemId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `ProprietarioEquipeId` | `bigint` | sim |  | **FK** |  | → `seguranca.Equipe` |
| `ProprietarioId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `Situacao` | `varchar(20)` | não |  |  |  |  |
| `SituacaoDesde` | `datetime2(3)` | não |  |  |  |  |
| `TipoDePessoa` | `varchar(10)` | não |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `comercial.ClienteCarteira`

**Check constraints:**
- `CK_ClienteCarteira_Ciclo`: `[DiasCicloContato] IS NULL OR [DiasCicloContato] > 0`
- `CK_ClienteCarteira_Classe`: `[Classe] IN ('A','B','C','D')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CarteiraId` | `bigint` | não |  | **FK** | sim | → `organizacao.Carteira` |
| `Classe` | `char(1)` | não |  |  |  |  |
| `ClienteId` | `bigint` | não |  | **FK** | sim | → `comercial.Cliente` |
| `DesvinculadoEm` | `datetime2(3)` | sim |  |  |  |  |
| `DiasCicloContato` | `smallint` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `PotencialAnual` | `decimal(18,2)` | sim |  |  |  |  |
| `UltimaInteracaoEm` | `datetime2(3)` | sim |  |  |  |  |
| `VinculadoEm` | `datetime2(3)` | não |  |  |  |  |
| `VinculadoPorId` | `bigint` | não |  |  |  |  |

### `comercial.ClienteContato`

**Check constraints:**
- `CK_ClienteContato_CatalogoDoPapelId`: `[CatalogoDoPapelId] = 1`
- `CK_ClienteContato_Periodo`: `[EncerrouEm] IS NULL OR [IniciouEm] IS NULL OR [EncerrouEm] >= [IniciouEm]`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CatalogoDoPapelId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ClienteId` | `bigint` | não |  | **FK** | sim | → `comercial.Cliente` |
| `ContatoId` | `bigint` | não |  | **FK** | sim | → `comercial.Contato` |
| `EhPrincipal` | `bit` | não |  |  |  |  |
| `EncerrouEm` | `date` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `IniciouEm` | `date` | sim |  |  |  |  |
| `PapelId` | `int` | não |  | **FK** | sim | → `metadado.CatalogoItem` |

### `comercial.ConsentimentoComunicacao`

**Check constraints:**
- `CK_ConsentimentoComunicacao_Canal`: `[Canal] IN ('Email','Sms','WhatsApp','Telefone','Correspondencia')`
- `CK_ConsentimentoComunicacao_Finalidade`: `[Finalidade] IN ('Marketing','Transacional','Pesquisa','Cobranca')`
- `CK_ConsentimentoComunicacao_UmTitular`: `([ClienteId] IS NOT NULL AND [ContatoId] IS NULL) OR ([ClienteId] IS NULL AND [ContatoId] IS NOT NULL)`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Canal` | `varchar(20)` | não |  |  |  |  |
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `Concedido` | `bit` | não |  |  |  |  |
| `ContatoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `DecididaEm` | `datetime2(3)` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EnderecoIp` | `varchar(45)` | sim |  |  |  |  |
| `Finalidade` | `varchar(40)` | não |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `OrigemEvidencia` | `varchar(60)` | não |  |  |  |  |
| `ReferenciaEvidencia` | `nvarchar(400)` | sim |  |  |  |  |
| `RegistradoPorId` | `bigint` | sim |  |  |  |  |

### `comercial.Contato`

**Check constraints:**
- `CK_Contato_Documento`: `[Documento] IS NULL OR LEN([Documento]) = 11`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `Cargo` | `nvarchar(80)` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `DataNascimento` | `date` | sim |  |  |  |  |
| `Documento` | `varchar(14)` | sim |  |  | sim |  |
| `EmpresaId` | `int` | não |  | **FK** | sim | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `ProprietarioId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `Sobrenome` | `nvarchar(120)` | sim |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `comercial.Endereco`

**Check constraints:**
- `CK_Endereco_CatalogoDaCulturaId`: `[CatalogoDaCulturaId] = 5`
- `CK_Endereco_Coordenada`: `([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)`
- `CK_Endereco_Hectares`: `[Hectares] IS NULL OR [Hectares] > 0`
- `CK_Endereco_Tipo`: `[Tipo] IN ('Fiscal','Entrega','Cobranca','Fazenda')`
- `CK_Endereco_Uf`: `[Uf] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z]'`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `Bairro` | `nvarchar(120)` | sim |  |  |  |  |
| `CatalogoDaCulturaId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `Cep` | `char(8)` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteId` | `bigint` | não |  | **FK** | sim | → `comercial.Cliente` |
| `Complemento` | `nvarchar(120)` | sim |  |  |  |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `CulturaId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `EhPrincipal` | `bit` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Hectares` | `decimal(12,2)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Identificacao` | `nvarchar(120)` | sim |  |  |  |  |
| `Latitude` | `decimal(10,7)` | sim |  |  |  |  |
| `Logradouro` | `nvarchar(200)` | não |  |  |  |  |
| `Longitude` | `decimal(10,7)` | sim |  |  |  |  |
| `Municipio` | `nvarchar(120)` | não |  |  |  |  |
| `Numero` | `varchar(20)` | sim |  |  |  |  |
| `Tipo` | `varchar(20)` | não |  |  |  |  |
| `Uf` | `char(2)` | não |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `comercial.Lead`

**Check constraints:**
- `CK_Lead_CatalogoDaOrigemId`: `[CatalogoDaOrigemId] = 2`
- `CK_Lead_CatalogoDoMotivoDescarteId`: `[CatalogoDoMotivoDescarteId] = 4`
- `CK_Lead_PayloadOriginalJson`: `[PayloadOriginal] IS NULL OR ISJSON([PayloadOriginal]) = 1`
- `CK_Lead_Qualificacao`: `[Situacao] <> 'Qualificado' OR ([QualificadoEm] IS NOT NULL AND [QualificadoPorId] IS NOT NULL AND [ClienteGeradoId] IS NOT NULL)`
- `CK_Lead_Situacao`: `[Situacao] IN ('Novo','EmContato','Qualificado','Descartado','Duplicado')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `CatalogoDaOrigemId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `CatalogoDoMotivoDescarteId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteGeradoId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `ContatoGeradoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Documento` | `varchar(14)` | sim |  |  |  |  |
| `Email` | `nvarchar(200)` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Interesse` | `nvarchar(400)` | sim |  |  |  |  |
| `LinhaNegocioId` | `int` | sim |  | **FK** |  | → `organizacao.LinhaDeNegocio` |
| `MotivoDescarteId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `NomeContato` | `nvarchar(200)` | não |  |  |  |  |
| `NomeEmpresa` | `nvarchar(200)` | sim |  |  |  |  |
| `OrigemId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `PayloadOriginal` | `nvarchar(max)` | sim |  |  |  |  |
| `ProcessoGeradoId` | `bigint` | sim |  | **FK** |  | → `processo.Processo` |
| `ProprietarioId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `QualificadoEm` | `datetime2(3)` | sim |  |  |  |  |
| `QualificadoPorId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `Situacao` | `varchar(20)` | não |  |  |  |  |
| `Telefone` | `varchar(20)` | sim |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

---

## Schema `documento`

### `documento.Documento`

**Check constraints:**
- `CK_Documento_CatalogoDoTipoDocumentoId`: `[CatalogoDoTipoDocumentoId] = 6`
- `CK_Documento_NumeroVersao`: `[NumeroVersao] >= 1`
- `CK_Documento_Resumo`: `[ResumoConteudo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^0-9a-f]%'`
- `CK_Documento_Tamanho`: `[TamanhoBytes] > 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `CaminhoArmazenamento` | `nvarchar(600)` | não |  |  |  |  |
| `CatalogoDoTipoDocumentoId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(260)` | não |  |  |  |  |
| `NumeroVersao` | `int` | não |  |  |  |  |
| `ResumoConteudo` | `char(64)` | não |  |  |  |  |
| `TamanhoBytes` | `bigint` | não |  |  |  |  |
| `TipoConteudo` | `varchar(120)` | não |  |  |  |  |
| `TipoDocumentoId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ValidoAte` | `date` | sim |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `documento.Vinculo`

**Check constraints:**
- `CK_Vinculo_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `DocumentoId` | `bigint` | não |  | **FK** | sim | → `documento.Documento` |
| `Entidade` | `varchar(40)` | não |  |  | sim |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `RegistroId` | `bigint` | não |  |  | sim |  |
| `VinculadoEm` | `datetime2(3)` | não |  |  |  |  |
| `VinculadoPorId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |

---

## Schema `frota`

### `frota.Equipamento`

**Check constraints:**
- `CK_Equipamento_Ano`: `([AnoFabricacao] IS NULL OR [AnoFabricacao] BETWEEN 1900 AND 2100) AND ([AnoModelo] IS NULL OR [AnoModelo] BETWEEN 1900 AND 2100)`
- `CK_Equipamento_Hierarquia`: `([EquipamentoPaiId] IS NULL OR [EquipamentoPaiId] <> [Id]) AND ([EquipamentoSubstitutoId] IS NULL OR [EquipamentoSubstitutoId] <> [Id])`
- `CK_Equipamento_Horimetro`: `[HorimetroAtual] IS NULL OR [HorimetroAtual] >= 0`
- `CK_Equipamento_Origem`: `[Origem] IN ('Protheus','Crm')`
- `CK_Equipamento_Situacao`: `[Situacao] IN ('Estoque','Ativo','Vendido','Baixado')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `AnoFabricacao` | `smallint` | sim |  |  |  |  |
| `AnoModelo` | `smallint` | sim |  |  |  |  |
| `Chassi` | `varchar(40)` | não |  |  | sim |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EnderecoId` | `bigint` | sim |  | **FK** |  | → `comercial.Endereco` |
| `EquipamentoPaiId` | `bigint` | sim |  | **FK** |  | → `frota.Equipamento` |
| `EquipamentoSubstitutoId` | `bigint` | sim |  | **FK** |  | → `frota.Equipamento` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `GarantiaAte` | `date` | sim |  |  |  |  |
| `HorimetroAtual` | `decimal(12,2)` | sim |  |  |  |  |
| `HorimetroAtualizadoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `LocalizacaoDescrita` | `nvarchar(200)` | sim |  |  |  |  |
| `ModeloId` | `int` | não |  | **FK** |  | → `frota.Modelo` |
| `NumeroSerie` | `varchar(40)` | sim |  |  |  |  |
| `Origem` | `varchar(20)` | não |  |  |  |  |
| `Placa` | `varchar(10)` | sim |  |  |  |  |
| `Situacao` | `varchar(20)` | não |  |  |  |  |
| `VendidoEm` | `date` | sim |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `frota.Familia`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(20)` | não |  |  | sim |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `MarcaId` | `int` | não |  | **FK** | sim | → `frota.Marca` |
| `Nome` | `nvarchar(80)` | não |  |  |  |  |

### `frota.LeituraDeHorimetro`

**Check constraints:**
- `CK_LeituraDeHorimetro_Horas`: `[Horas] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `EquipamentoId` | `bigint` | não |  | **FK** | sim | → `frota.Equipamento` |
| `Fonte` | `varchar(40)` | não |  |  |  |  |
| `Horas` | `decimal(12,2)` | não |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `LidaEm` | `datetime2(3)` | não |  |  | sim |  |
| `RegistradoPorId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |

### `frota.Marca`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(20)` | não |  |  | sim |  |
| `EhRepresentada` | `bit` | não |  |  |  |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(80)` | não |  |  |  |  |

### `frota.Modelo`

**Check constraints:**
- `CK_Modelo_IntervaloManutencao`: `[IntervaloManutencaoHoras] IS NULL OR [IntervaloManutencaoHoras] > 0`
- `CK_Modelo_Potencia`: `[PotenciaCv] IS NULL OR [PotenciaCv] > 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `FamiliaId` | `int` | não |  | **FK** |  | → `frota.Familia` |
| `Id` | `int` | não | **PK** |  |  |  |
| `IntervaloManutencaoHoras` | `int` | sim |  |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `PotenciaCv` | `smallint` | sim |  |  |  |  |

---

## Schema `integracao`

### `integracao.ChaveExterna`

**Check constraints:**
- `CK_ChaveExterna_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `ChaveOrigem` | `varchar(200)` | não |  |  | sim |  |
| `Entidade` | `varchar(40)` | não |  |  | sim |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `RegistroId` | `bigint` | não |  |  |  |  |
| `SincronizadoEm` | `datetime2(3)` | não |  |  |  |  |
| `SistemaId` | `int` | não |  | **FK** | sim | → `integracao.Sistema` |

### `integracao.MensagemDeSaida`

**Check constraints:**
- `CK_MensagemDeSaida_ConteudoJson`: `ISJSON([Conteudo]) = 1`
- `CK_MensagemDeSaida_Entrega`: `[Situacao] <> 'Entregue' OR [EntregueEm] IS NOT NULL`
- `CK_MensagemDeSaida_Situacao`: `[Situacao] IN ('Pendente','Entregue','Falhou','DescartadaAposLimite')`
- `CK_MensagemDeSaida_Tentativas`: `[Tentativas] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Conteudo` | `nvarchar(max)` | não |  |  |  |  |
| `CorrelacaoId` | `uniqueidentifier` | não |  |  |  |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `EntregueEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `ProximaTentativaEm` | `datetime2(3)` | sim |  |  |  |  |
| `SistemaId` | `int` | não |  | **FK** |  | → `integracao.Sistema` |
| `Situacao` | `varchar(30)` | não |  |  |  |  |
| `Tentativas` | `smallint` | não |  |  |  |  |
| `Tipo` | `varchar(60)` | não |  |  |  |  |
| `UltimoErro` | `nvarchar(2000)` | sim |  |  |  |  |

### `integracao.MensagemDescartada`

**Check constraints:**
- `CK_MensagemDescartada_ConteudoJson`: `ISJSON([Conteudo]) = 1`
- `CK_MensagemDescartada_Tentativas`: `[Tentativas] >= 0`
- `CK_MensagemDescartada_Tratativa`: `[TratadaEm] IS NULL OR ([TratadaPorId] IS NOT NULL AND [Tratativa] IS NOT NULL)`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Conteudo` | `nvarchar(max)` | não |  |  |  |  |
| `DescartadaEm` | `datetime2(3)` | não |  |  |  |  |
| `Erro` | `nvarchar(4000)` | não |  |  |  |  |
| `Fluxo` | `varchar(60)` | não |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `MensagemDeSaidaId` | `bigint` | sim |  | **FK** |  | → `integracao.MensagemDeSaida` |
| `Tentativas` | `smallint` | não |  |  |  |  |
| `TratadaEm` | `datetime2(3)` | sim |  |  |  |  |
| `TratadaPorId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `Tratativa` | `nvarchar(1000)` | sim |  |  |  |  |

### `integracao.PontoDeSincronismo`

**Check constraints:**
- `CK_PontoDeSincronismo_Alarme`: `[MinutosParaAlarme] > 0`
- `CK_PontoDeSincronismo_Contadores`: `[RegistrosLidos] >= 0 AND [RegistrosGravados] >= 0 AND [RegistrosErro] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Fluxo` | `varchar(60)` | não |  |  | sim |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `MinutosParaAlarme` | `int` | não |  |  |  |  |
| `ProcessadoEm` | `datetime2(3)` | não |  |  |  |  |
| `RegistrosErro` | `int` | não |  |  |  |  |
| `RegistrosGravados` | `int` | não |  |  |  |  |
| `RegistrosLidos` | `int` | não |  |  |  |  |
| `SistemaId` | `int` | não |  | **FK** |  | → `integracao.Sistema` |
| `UltimoValor` | `varchar(100)` | não |  |  |  |  |

### `integracao.Recepcao`

**Check constraints:**
- `CK_Recepcao_ConteudoJson`: `ISJSON([Conteudo]) = 1`
- `CK_Recepcao_Desfecho`: `([Situacao] <> 'Falhou' OR [Erro] IS NOT NULL) AND ([Situacao] <> 'Processada' OR [ProcessadaEm] IS NOT NULL)`
- `CK_Recepcao_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_Recepcao_Expurgo`: `[ExpurgarApos] > [RecebidaEm]`
- `CK_Recepcao_Situacao`: `[Situacao] IN ('Recebida','Processada','Falhou','Ignorada')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `ChaveOrigem` | `varchar(200)` | não |  |  |  |  |
| `Conteudo` | `nvarchar(max)` | não |  |  |  |  |
| `Entidade` | `varchar(40)` | não |  |  |  |  |
| `Erro` | `nvarchar(4000)` | sim |  |  |  |  |
| `ExpurgarApos` | `datetime2(3)` | não |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `ProcessadaEm` | `datetime2(3)` | sim |  |  |  |  |
| `RecebidaEm` | `datetime2(3)` | não | **PK** |  |  |  |
| `SistemaId` | `int` | não |  | **FK** |  | → `integracao.Sistema` |
| `Situacao` | `varchar(20)` | não |  |  |  |  |

### `integracao.Sistema`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(20)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `MeioDeAcesso` | `varchar(40)` | não |  |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `ResponsavelTecnico` | `nvarchar(120)` | sim |  |  |  |  |

---

## Schema `metadado`

### `metadado.CampoPersonalizado`

**Check constraints:**
- `CK_CampoPersonalizado_Campo`: `[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'`
- `CK_CampoPersonalizado_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_CampoPersonalizado_Lista`: `[TipoDeCampo] <> 'Lista' OR [CatalogoId] IS NOT NULL`
- `CK_CampoPersonalizado_Tipo`: `[TipoDeCampo] IN ('Texto','Numero','Data','Booleano','Lista','Referencia')`
- `CK_CampoPersonalizado_ValidacaoJson`: `[Validacao] IS NULL OR ISJSON([Validacao]) = 1`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Campo` | `varchar(60)` | não |  |  | sim |  |
| `CatalogoId` | `int` | sim |  | **FK** |  | → `metadado.Catalogo` |
| `CondicaoVisibilidade` | `nvarchar(1000)` | sim |  |  |  |  |
| `EhObrigatorio` | `bit` | não |  |  |  |  |
| `EhPersonalizado` | `bit` | não |  |  |  |  |
| `Entidade` | `varchar(40)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Grupo` | `nvarchar(60)` | sim |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `NoResumo` | `bit` | não |  |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |
| `Rotulo` | `nvarchar(80)` | não |  |  |  |  |
| `TamanhoMaximo` | `int` | sim |  |  |  |  |
| `TipoDeCampo` | `varchar(20)` | não |  |  |  |  |
| `Validacao` | `nvarchar(max)` | sim |  |  |  |  |

### `metadado.Catalogo`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `Descricao` | `nvarchar(400)` | sim |  |  |  |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `PermiteItemNovo` | `bit` | não |  |  |  |  |

### `metadado.CatalogoItem`

**Check constraints:**
- `CK_CatalogoItem_Ordem`: `[Ordem] >= 0`
- `CK_CatalogoItem_Pai`: `[ItemPaiId] IS NULL OR [ItemPaiId] <> [Id]`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CatalogoId` | `int` | não |  | **FK** | sim | → `metadado.Catalogo` |
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `Descricao` | `nvarchar(200)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `ExigeObservacao` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `ItemPaiId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `Ordem` | `smallint` | não |  |  |  |  |
| `UltimoUsoEm` | `datetime2(3)` | sim |  |  |  |  |

### `metadado.Formulario`

**Check constraints:**
- `CK_Formulario_Versao`: `[VersaoPublicada] >= 1`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `Descricao` | `nvarchar(400)` | sim |  |  |  |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `TipoProcessoId` | `int` | sim |  | **FK** |  | → `processo.TipoProcesso` |
| `VersaoPublicada` | `int` | não |  |  |  |  |

### `metadado.Pergunta`

**Check constraints:**
- `CK_Pergunta_Catalogo`: `[TipoDeResposta] NOT IN ('Catalogo','CatalogoMultiplo') OR [CatalogoId] IS NOT NULL`
- `CK_Pergunta_Condicao`: `[PerguntaCondicaoId] IS NULL OR [PerguntaCondicaoId] <> [Id]`
- `CK_Pergunta_TipoDeResposta`: `[TipoDeResposta] IN ('Texto','Numero','Data','Booleano','Catalogo','Cliente','Equipamento','CatalogoMultiplo')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CatalogoId` | `int` | sim |  | **FK** |  | → `metadado.Catalogo` |
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `EhObrigatoria` | `bit` | não |  |  |  |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `FormularioId` | `int` | não |  | **FK** | sim | → `metadado.Formulario` |
| `Id` | `int` | não | **PK** |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |
| `PerguntaCondicaoId` | `int` | sim |  | **FK** |  | → `metadado.Pergunta` |
| `Texto` | `nvarchar(1000)` | não |  |  |  |  |
| `TipoDeResposta` | `varchar(20)` | não |  |  |  |  |
| `ValorCondicao` | `nvarchar(200)` | sim |  |  |  |  |

### `metadado.Preenchimento`

**Check constraints:**
- `CK_Preenchimento_Situacao`: `[Situacao] IN ('EmAndamento','Concluido','Cancelado')`
- `CK_Preenchimento_TemVinculo`: `[ClienteId] IS NOT NULL OR [ProcessoId] IS NOT NULL OR [TarefaId] IS NOT NULL OR [InteracaoId] IS NOT NULL`
- `CK_Preenchimento_Versao`: `[VersaoFormulario] >= 1`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `FormularioId` | `int` | não |  | **FK** |  | → `metadado.Formulario` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `InteracaoId` | `bigint` | sim |  | **FK** |  | → `processo.Interacao` |
| `PreenchidoEm` | `datetime2(3)` | não |  |  |  |  |
| `PreenchidoPorId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `ProcessoId` | `bigint` | sim |  | **FK** |  | → `processo.Processo` |
| `Situacao` | `varchar(20)` | não |  |  |  |  |
| `TarefaId` | `bigint` | sim |  | **FK** |  | → `processo.Tarefa` |
| `VersaoFormulario` | `int` | não |  |  |  |  |

### `metadado.Resposta`

**Check constraints:**
- `CK_Resposta_UmValor`: `(CASE WHEN [ValorTexto]        IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorNumero]       IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorData]         IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorBooleano]     IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [CatalogoItemId]   IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ClienteId]         IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [EquipamentoId]     IS NOT NULL THEN 1 ELSE 0 END +  CASE WHEN [ValorEstruturado]  IS NOT NULL THEN 1 ELSE 0 END) = 1`
- `CK_Resposta_ValorEstruturadoJson`: `[ValorEstruturado] IS NULL OR ISJSON([ValorEstruturado]) = 1`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CatalogoItemId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `EquipamentoId` | `bigint` | sim |  | **FK** |  | → `frota.Equipamento` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `PerguntaId` | `int` | não |  | **FK** | sim | → `metadado.Pergunta` |
| `PreenchimentoId` | `bigint` | não |  | **FK** | sim | → `metadado.Preenchimento` |
| `ValorBooleano` | `bit` | sim |  |  |  |  |
| `ValorData` | `datetime2(3)` | sim |  |  |  |  |
| `ValorEstruturado` | `nvarchar(max)` | sim |  |  |  |  |
| `ValorNumero` | `decimal(18,6)` | sim |  |  |  |  |
| `ValorTexto` | `nvarchar(4000)` | sim |  |  |  |  |

### `metadado.TratadorDeEvento`

**Check constraints:**
- `CK_TratadorDeEvento_Assincrono`: `[EhAssincrono] = 0 OR [Momento] = 'PosOperacao'`
- `CK_TratadorDeEvento_Momento`: `[Momento] IN ('PreValidacao','PreOperacao','PosOperacao')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CamposFiltro` | `varchar(400)` | sim |  |  |  |  |
| `EhAssincrono` | `bit` | não |  |  |  |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Evento` | `varchar(60)` | não |  |  | sim |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Momento` | `varchar(20)` | não |  |  | sim |  |
| `Ordem` | `smallint` | não |  |  |  |  |
| `TipoImplementacao` | `varchar(200)` | não |  |  | sim |  |

---

## Schema `organizacao`

### `organizacao.Carteira`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EquipeId` | `bigint` | sim |  | **FK** |  | → `seguranca.Equipe` |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `LinhaDeNegocioId` | `int` | não |  | **FK** |  | → `organizacao.LinhaDeNegocio` |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `PracaId` | `int` | sim |  | **FK** |  | → `organizacao.Praca` |
| `ResponsavelId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `SupervisorId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `organizacao.Empresa`

**Check constraints:**
- `CK_Empresa_Nivel`: `[Nivel] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Caminho` | `varchar(400)` | não |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `Cnpj` | `varchar(14)` | sim |  |  |  |  |
| `Codigo` | `varchar(20)` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `EmpresaPaiId` | `int` | sim |  | **FK** |  | → `organizacao.Empresa` |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nivel` | `smallint` | não |  |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |

### `organizacao.HierarquiaComercial`

**Check constraints:**
- `CK_HierarquiaComercial_Profundidade`: `[Profundidade] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AncestralId` | `bigint` | não |  | **FK** | sim | → `seguranca.Usuario` |
| `DescendenteId` | `bigint` | não |  | **FK** | sim | → `seguranca.Usuario` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Profundidade` | `smallint` | não |  |  |  |  |

### `organizacao.LinhaDeNegocio`

**Check constraints:**
- `CK_LinhaDeNegocio_Ciclo`: `COALESCE([DiasCicloClasseA], 1) > 0 AND COALESCE([DiasCicloClasseB], 1) > 0 AND COALESCE([DiasCicloClasseC], 1) > 0 AND COALESCE([DiasCicloClasseD], 1) > 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(20)` | não |  |  | sim |  |
| `DiasCicloClasseA` | `smallint` | sim |  |  |  |  |
| `DiasCicloClasseB` | `smallint` | sim |  |  |  |  |
| `DiasCicloClasseC` | `smallint` | sim |  |  |  |  |
| `DiasCicloClasseD` | `smallint` | sim |  |  |  |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(80)` | não |  |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |

### `organizacao.Meta`

**Check constraints:**
- `CK_Meta_Alvo`: `[Alvo] >= 0`
- `CK_Meta_Periodo`: `[PeriodoFim] >= [PeriodoInicio]`
- `CK_Meta_Tipo`: `[Tipo] IN ('Faturamento','Cobertura','Frequencia','Volume')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `Alvo` | `decimal(18,2)` | não |  |  |  |  |
| `CarteiraId` | `bigint` | sim |  | **FK** |  | → `organizacao.Carteira` |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `LinhaDeNegocioId` | `int` | sim |  | **FK** |  | → `organizacao.LinhaDeNegocio` |
| `Observacao` | `nvarchar(400)` | sim |  |  |  |  |
| `PeriodoFim` | `date` | não |  |  |  |  |
| `PeriodoInicio` | `date` | não |  |  |  |  |
| `Tipo` | `varchar(20)` | não |  |  |  |  |
| `UsuarioId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `organizacao.Praca`

**Check constraints:**
- `CK_Praca_AnoReferencia`: `[AnoReferencia] BETWEEN 2000 AND 2100`
- `CK_Praca_Uf`: `[Uf] IS NULL OR [Uf] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z]'`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AnoReferencia` | `smallint` | não |  |  | sim |  |
| `Codigo` | `varchar(20)` | não |  |  | sim |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `LinhaDeNegocioId` | `int` | não |  | **FK** | sim | → `organizacao.LinhaDeNegocio` |
| `MaquinasEstimadas` | `int` | sim |  |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `PotencialEstimado` | `decimal(18,2)` | sim |  |  |  |  |
| `Uf` | `char(2)` | sim |  |  |  |  |

---

## Schema `processo`

### `processo.Fase`

**Check constraints:**
- `CK_Fase_Probabilidade`: `[ProbabilidadePercentual] IS NULL OR [ProbabilidadePercentual] BETWEEN 0 AND 100`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `EhFinal` | `bit` | não |  |  |  |  |
| `ExigeCamposObrigatorios` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Marco` | `nvarchar(60)` | sim |  |  |  |  |
| `Nome` | `nvarchar(80)` | não |  |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |
| `ProbabilidadePercentual` | `smallint` | sim |  |  |  |  |
| `TipoProcessoId` | `int` | não |  | **FK** | sim | → `processo.TipoProcesso` |

### `processo.Interacao`

**Check constraints:**
- `CK_Interacao_Coordenada`: `([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)`
- `CK_Interacao_Duracao`: `[DuracaoMinutos] IS NULL OR [DuracaoMinutos] >= 0`
- `CK_Interacao_Natureza`: `[Natureza] IN ('Ativa','Receptiva','Sistema')`
- `CK_Interacao_TemVinculo`: `[ProcessoId] IS NOT NULL OR [ClienteId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [LeadId] IS NOT NULL`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Assunto` | `nvarchar(200)` | não |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `ContatoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `Detalhe` | `nvarchar(4000)` | sim |  |  |  |  |
| `DuracaoMinutos` | `int` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Latitude` | `decimal(10,7)` | sim |  |  |  |  |
| `LeadId` | `bigint` | sim |  | **FK** |  | → `comercial.Lead` |
| `Longitude` | `decimal(10,7)` | sim |  |  |  |  |
| `Natureza` | `varchar(12)` | não |  |  |  |  |
| `OcorridaEm` | `datetime2(3)` | não |  |  |  |  |
| `ProcessoId` | `bigint` | sim |  | **FK** |  | → `processo.Processo` |
| `RegistradoPorId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `ResultadoComplemento` | `nvarchar(200)` | sim |  |  |  |  |
| `ResultadoId` | `int` | sim |  | **FK** |  | → `processo.Resultado` |
| `TarefaId` | `bigint` | sim |  | **FK** |  | → `processo.Tarefa` |
| `TipoTarefaId` | `int` | não |  | **FK** |  | → `processo.TipoTarefa` |

### `processo.InteracaoParticipante`

**Check constraints:**
- `CK_InteracaoParticipante_Identificado`: `[UsuarioId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [NomeExterno] IS NOT NULL`
- `CK_InteracaoParticipante_Papel`: `[Papel] IN ('Autor','Destinatario','Copia','Participante')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `ContatoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `InteracaoId` | `bigint` | não |  | **FK** |  | → `processo.Interacao` |
| `NomeExterno` | `nvarchar(200)` | sim |  |  |  |  |
| `Papel` | `varchar(20)` | não |  |  |  |  |
| `UsuarioId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |

### `processo.ItemDeProposta`

**Check constraints:**
- `CK_ItemDeProposta_CatalogoDaCondicaoPagamentoId`: `[CatalogoDaCondicaoPagamentoId] = 7`
- `CK_ItemDeProposta_Desconto`: `[DescontoPercentual] IS NULL OR [DescontoPercentual] BETWEEN 0 AND 100`
- `CK_ItemDeProposta_Quantidade`: `[Quantidade] > 0`
- `CK_ItemDeProposta_ValorTotal`: `[ValorTotal] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `CatalogoDaCondicaoPagamentoId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CondicaoPagamentoId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `DescontoPercentual` | `decimal(5,2)` | sim |  |  |  |  |
| `Descricao` | `nvarchar(400)` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EquipamentoId` | `bigint` | sim |  | **FK** |  | → `frota.Equipamento` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `ModeloId` | `int` | sim |  | **FK** |  | → `frota.Modelo` |
| `Ordem` | `smallint` | não |  |  | sim |  |
| `PrecoUnitario` | `decimal(18,2)` | sim |  |  |  |  |
| `ProcessoId` | `bigint` | não |  | **FK** | sim | → `processo.Processo` |
| `Quantidade` | `decimal(12,3)` | não |  |  |  |  |
| `ValorTotal` | `decimal(18,2)` | não |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `processo.MotivoDePerda`

**Check constraints:**
- `CK_MotivoDePerda_Categoria`: `[Categoria] IN ('Preco','Prazo','Produto','Financiamento','Desistencia','Concorrencia','Outro')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Categoria` | `varchar(20)` | não |  |  |  |  |
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `ExigeConcorrente` | `bit` | não |  |  |  |  |
| `ExigeObservacao` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |

### `processo.PassagemDeFase`

**Check constraints:**
- `CK_PassagemDeFase_Ordem`: `[Ordem] >= 1`
- `CK_PassagemDeFase_Periodo`: `[SaiuEm] IS NULL OR [SaiuEm] >= [EntrouEm]`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `EntrouEm` | `datetime2(3)` | não |  |  |  |  |
| `EntrouPorId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `FaseId` | `int` | não |  | **FK** |  | → `processo.Fase` |
| `HorasUteis` | `decimal(10,2)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `InteracaoOrigemId` | `bigint` | sim |  | **FK** |  | → `processo.Interacao` |
| `Ordem` | `int` | não |  |  | sim |  |
| `ProcessoId` | `bigint` | não |  | **FK** | sim | → `processo.Processo` |
| `RegraOrigemId` | `int` | sim |  | **FK** |  | → `processo.Regra` |
| `SaiuEm` | `datetime2(3)` | sim |  |  |  |  |

### `processo.Processo`

**Check constraints:**
- `CK_Processo_CatalogoDoConcorrenteId`: `[CatalogoDoConcorrenteId] = 8`
- `CK_Processo_Encerramento`: `[Situacao] IN ('Aberto','Suspenso') OR [ConcluidoEm] IS NOT NULL`
- `CK_Processo_MotivoDePerda`: `[Situacao] <> 'Perdido' OR [MotivoDePerdaId] IS NOT NULL`
- `CK_Processo_Situacao`: `[Situacao] IN ('Aberto','Suspenso','Ganho','Perdido','Cancelado')`
- `CK_Processo_Valores`: `([ValorEstimado] IS NULL OR [ValorEstimado] >= 0) AND ([ValorFinal] IS NULL OR [ValorFinal] >= 0)`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `CarteiraId` | `bigint` | sim |  | **FK** |  | → `organizacao.Carteira` |
| `CatalogoDoConcorrenteId` | `int` | não |  | **FK** |  | → `metadado.CatalogoItem` |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteId` | `bigint` | não |  | **FK** |  | → `comercial.Cliente` |
| `ConcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `ConcorrenteId` | `int` | sim |  | **FK** |  | → `metadado.CatalogoItem` |
| `ContatoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Descricao` | `nvarchar(4000)` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** | sim | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `FaseDesde` | `datetime2(3)` | não |  |  |  |  |
| `FaseId` | `int` | não |  | **FK** |  | → `processo.Fase` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `MotivoDePerdaId` | `int` | sim |  | **FK** |  | → `processo.MotivoDePerda` |
| `Numero` | `bigint` | não |  |  | sim |  |
| `ObservacaoDaPerda` | `nvarchar(1000)` | sim |  |  |  |  |
| `PrevisaoConclusao` | `date` | sim |  |  |  |  |
| `PrevisaoConclusaoOriginal` | `date` | sim |  |  |  |  |
| `ProprietarioEquipeId` | `bigint` | sim |  | **FK** |  | → `seguranca.Equipe` |
| `ProprietarioId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `Quantidade` | `decimal(12,3)` | sim |  |  |  |  |
| `Situacao` | `varchar(20)` | não |  |  |  |  |
| `SituacaoDesde` | `datetime2(3)` | não |  |  |  |  |
| `TipoProcessoId` | `int` | não |  | **FK** |  | → `processo.TipoProcesso` |
| `Titulo` | `nvarchar(200)` | não |  |  |  |  |
| `ValorEstimado` | `decimal(18,2)` | sim |  |  |  |  |
| `ValorFinal` | `decimal(18,2)` | sim |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `processo.Regra`

**Check constraints:**
- `CK_Regra_Efeito`: `[Efeito] IN ('CriarTarefa','MoverFase','EncerrarProcesso','Notificar','ChamarWebhook','AtribuirCarteira')`
- `CK_Regra_EfeitoParametrosJson`: `[EfeitoParametros] IS NULL OR ISJSON([EfeitoParametros]) = 1`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `Codigo` | `varchar(60)` | não |  |  | sim |  |
| `Condicao` | `nvarchar(2000)` | sim |  |  |  |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Descricao` | `nvarchar(1000)` | sim |  |  |  |  |
| `Efeito` | `varchar(40)` | não |  |  |  |  |
| `EfeitoFaseId` | `int` | sim |  | **FK** |  | → `processo.Fase` |
| `EfeitoParametros` | `nvarchar(max)` | sim |  |  |  |  |
| `EfeitoTipoTarefaId` | `int` | sim |  | **FK** |  | → `processo.TipoTarefa` |
| `EhCritica` | `bit` | não |  |  |  |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Evento` | `varchar(40)` | não |  |  |  |  |
| `ExpressaoDestinatario` | `nvarchar(400)` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(200)` | não |  |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |
| `PrazoDiasUteis` | `int` | não |  |  |  |  |
| `ResultadoId` | `int` | sim |  | **FK** |  | → `processo.Resultado` |
| `TipoProcessoId` | `int` | sim |  | **FK** |  | → `processo.TipoProcesso` |
| `TipoTarefaId` | `int` | sim |  | **FK** |  | → `processo.TipoTarefa` |

### `processo.RegraExecucao`

**Check constraints:**
- `CK_RegraExecucao_Resultado`: `[Resultado] IN ('Disparou','CondicaoFalsa','RegraInativa','SemDestinatario','Erro','Suprimida')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CorrelacaoId` | `uniqueidentifier` | não |  |  |  |  |
| `DestinatarioResolvidoId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `DuracaoMs` | `int` | não |  |  |  |  |
| `Evento` | `varchar(40)` | não |  |  |  |  |
| `ExecutadoEm` | `datetime2(3)` | não | **PK** |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `InteracaoId` | `bigint` | sim |  | **FK** |  | → `processo.Interacao` |
| `Motivo` | `nvarchar(1000)` | sim |  |  |  |  |
| `ProcessoId` | `bigint` | sim |  | **FK** |  | → `processo.Processo` |
| `RegraId` | `int` | não |  | **FK** |  | → `processo.Regra` |
| `Resultado` | `varchar(20)` | não |  |  |  |  |
| `TarefaCriadaId` | `bigint` | sim |  | **FK** |  | → `processo.Tarefa` |
| `TarefaId` | `bigint` | sim |  | **FK** |  | → `processo.Tarefa` |

### `processo.Resultado`

**Check constraints:**
- `CK_Resultado_Classe`: `[Classe] IN ('Avanco','Manutencao','Perda','Cancelamento')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Classe` | `varchar(20)` | não |  |  |  |  |
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `ExigeJustificativa` | `bit` | não |  |  |  |  |
| `FaseDestinoId` | `int` | sim |  | **FK** |  | → `processo.Fase` |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `TipoTarefaId` | `int` | não |  | **FK** | sim | → `processo.TipoTarefa` |
| `UltimoUsoEm` | `datetime2(3)` | sim |  |  |  |  |

### `processo.Tarefa`

**Check constraints:**
- `CK_Tarefa_Conclusao`: `[Situacao] <> 'Concluida' OR ([ConcluidaEm] IS NOT NULL AND [ConcluidaPorId] IS NOT NULL AND [ResultadoId] IS NOT NULL)`
- `CK_Tarefa_OrigemAtribuicao`: `[OrigemAtribuicao] IN ('Regra','Manual','Hierarquia','Carteira','Rodizio')`
- `CK_Tarefa_Prioridade`: `[Prioridade] BETWEEN 1 AND 5`
- `CK_Tarefa_Situacao`: `[Situacao] IN ('Pendente','EmAndamento','Concluida','Cancelada','Reatribuida')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AgendadaPara` | `datetime2(3)` | não |  |  |  |  |
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `Assunto` | `nvarchar(200)` | não |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `ClienteId` | `bigint` | sim |  | **FK** |  | → `comercial.Cliente` |
| `ConcluidaEm` | `datetime2(3)` | sim |  |  |  |  |
| `ConcluidaPorId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `ContatoId` | `bigint` | sim |  | **FK** |  | → `comercial.Contato` |
| `CriadaPorRegraId` | `int` | sim |  | **FK** |  | → `processo.Regra` |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Detalhe` | `nvarchar(4000)` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `InteracaoConclusaoId` | `bigint` | sim |  | **FK** |  | → `processo.Interacao` |
| `InteracaoOrigemId` | `bigint` | sim |  | **FK** |  | → `processo.Interacao` |
| `OrigemAtribuicao` | `varchar(20)` | não |  |  |  |  |
| `PrazoLimite` | `datetime2(3)` | sim |  |  |  |  |
| `Prioridade` | `smallint` | não |  |  |  |  |
| `ProcessoId` | `bigint` | sim |  | **FK** |  | → `processo.Processo` |
| `ResponsavelEquipeId` | `bigint` | sim |  | **FK** |  | → `seguranca.Equipe` |
| `ResponsavelId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `ResultadoId` | `int` | sim |  | **FK** |  | → `processo.Resultado` |
| `Situacao` | `varchar(20)` | não |  |  |  |  |
| `TipoTarefaId` | `int` | não |  | **FK** |  | → `processo.TipoTarefa` |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `processo.TipoProcesso`

**Check constraints:**
- `CK_TipoProcesso_Versao`: `[Versao] >= 1`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `LinhaDeNegocioId` | `int` | sim |  | **FK** |  | → `organizacao.LinhaDeNegocio` |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `Versao` | `int` | não |  |  | sim |  |

### `processo.TipoTarefa`

**Check constraints:**
- `CK_TipoTarefa_Categoria`: `[Categoria] IN ('Visita','Ligacao','WhatsApp','Email','Remota','Interna')`
- `CK_TipoTarefa_Cor`: `[Cor] IS NULL OR ([Cor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND LEN([Cor]) = 7)`
- `CK_TipoTarefa_Prazo`: `[PrazoDiasUteis] >= 0`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Categoria` | `varchar(20)` | não |  |  |  |  |
| `Codigo` | `varchar(40)` | não |  |  | sim |  |
| `ContaParaCobertura` | `bit` | não |  |  |  |  |
| `Cor` | `varchar(7)` | sim |  |  |  |  |
| `EhAprovacao` | `bit` | não |  |  |  |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `ExigeGeorreferencia` | `bit` | não |  |  |  |  |
| `FormularioId` | `int` | sim |  | **FK** |  | → `metadado.Formulario` |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `PrazoDiasUteis` | `smallint` | não |  |  |  |  |
| `TipoProcessoId` | `int` | sim |  | **FK** |  | → `processo.TipoProcesso` |
| `UltimoUsoEm` | `datetime2(3)` | sim |  |  |  |  |

---

## Schema `relatorio`

### `relatorio.Fonte`

**Check constraints:**
- `CK_Fonte_EntidadeRaiz`: `[EntidadeRaiz] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_Fonte_Materializada`: `[EhMaterializada] = 0 OR [AtualizadaEm] IS NOT NULL`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AtualizadaEm` | `datetime2(3)` | sim |  |  |  |  |
| `Codigo` | `varchar(60)` | não |  |  | sim |  |
| `Descricao` | `nvarchar(400)` | sim |  |  |  |  |
| `EhMaterializada` | `bit` | não |  |  |  |  |
| `EntidadeRaiz` | `varchar(40)` | não |  |  |  |  |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `NomeDaVisao` | `varchar(120)` | não |  |  |  |  |
| `SistemaId` | `int` | sim |  | **FK** |  | → `integracao.Sistema` |

### `relatorio.FonteCampo`

**Check constraints:**
- `CK_FonteCampo_Campo`: `[Campo] COLLATE Latin1_General_BIN2 LIKE '[A-Z]%' AND [Campo] COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Za-z0-9]%'`
- `CK_FonteCampo_TipoDeDado`: `[TipoDeDado] IN ('Texto','Numero','Data','Booleano','Moeda','Percentual')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Campo` | `varchar(60)` | não |  |  | sim |  |
| `FonteId` | `int` | não |  | **FK** | sim | → `relatorio.Fonte` |
| `Id` | `int` | não | **PK** |  |  |  |
| `Ordem` | `smallint` | não |  |  |  |  |
| `PermiteAgrupar` | `bit` | não |  |  |  |  |
| `PermiteFiltrar` | `bit` | não |  |  |  |  |
| `PermiteSomar` | `bit` | não |  |  |  |  |
| `Rotulo` | `nvarchar(80)` | não |  |  |  |  |
| `TipoDeDado` | `varchar(20)` | não |  |  |  |  |

### `relatorio.Relatorio`

**Check constraints:**
- `CK_Relatorio_DefinicaoJson`: `ISJSON([Definicao]) = 1`
- `CK_Relatorio_Visibilidade`: `[Visibilidade] IN ('Privado','Equipe','Empresa')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Definicao` | `nvarchar(max)` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `FonteId` | `int` | não |  | **FK** |  | → `relatorio.Fonte` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |
| `ProprietarioId` | `bigint` | não |  | **FK** |  | → `seguranca.Usuario` |
| `Versao` | `rowversion` | sim |  |  |  |  |
| `Visibilidade` | `varchar(20)` | não |  |  |  |  |

---

## Schema `seguranca`

### `seguranca.CompartilhamentoDeRegistro`

**Check constraints:**
- `CK_CompartilhamentoDeRegistro_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_CompartilhamentoDeRegistro_Motivo`: `[Motivo] IN ('Manual','Regra','Equipe','Hierarquia','Delegacao')`
- `CK_CompartilhamentoDeRegistro_Nivel`: `[Nivel] IN ('Leitura','Edicao')`
- `CK_CompartilhamentoDeRegistro_UmSujeito`: `([UsuarioId] IS NOT NULL AND [EquipeId] IS NULL) OR ([UsuarioId] IS NULL AND [EquipeId] IS NOT NULL)`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `Entidade` | `varchar(40)` | não |  |  |  |  |
| `EquipeId` | `bigint` | sim |  | **FK** |  | → `seguranca.Equipe` |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `ExpiraEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Motivo` | `varchar(20)` | não |  |  |  |  |
| `Nivel` | `varchar(10)` | não |  |  |  |  |
| `RegistroId` | `bigint` | não |  |  |  |  |
| `RegraOrigemId` | `int` | sim |  |  |  |  |
| `UsuarioId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `seguranca.ConjuntoDePermissao`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(60)` | não |  |  | sim |  |
| `Descricao` | `nvarchar(400)` | sim |  |  |  |  |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  |  |  |

### `seguranca.ConjuntoDePermissaoItem`

**Check constraints:**
- `CK_ConjuntoDePermissaoItem_Profundidade`: `[Profundidade] IN ('Proprios','Equipe','Empresa','EmpresaEAbaixo','Organizacao')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `CodigoPermissao` | `varchar(80)` | não |  |  | sim |  |
| `ConjuntoPermissaoId` | `int` | não |  | **FK** | sim | → `seguranca.ConjuntoDePermissao` |
| `Id` | `int` | não | **PK** |  |  |  |
| `Profundidade` | `varchar(20)` | não |  |  |  |  |

### `seguranca.Equipe`

**Check constraints:**
- `CK_Equipe_Tipo`: `[Tipo] IN ('Proprietaria','Acesso')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `Descricao` | `nvarchar(400)` | sim |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** | sim | → `organizacao.Empresa` |
| `EstaAtiva` | `bit` | não |  |  |  |  |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `Nome` | `nvarchar(120)` | não |  |  | sim |  |
| `Tipo` | `varchar(20)` | não |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `seguranca.EquipeMembro`

**Check constraints:**
- `CK_EquipeMembro_Periodo`: `[SaiuEm] IS NULL OR [SaiuEm] >= [EntrouEm]`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `EhLider` | `bit` | não |  |  |  |  |
| `EntrouEm` | `datetime2(3)` | não |  |  |  |  |
| `EquipeId` | `bigint` | não |  | **FK** | sim | → `seguranca.Equipe` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `SaiuEm` | `datetime2(3)` | sim |  |  |  |  |
| `UsuarioId` | `bigint` | não |  | **FK** | sim | → `seguranca.Usuario` |

### `seguranca.Permissao`

**Check constraints:**
- `CK_Permissao_Entidade`: `[Entidade] COLLATE Latin1_General_BIN2 IN ('Alerta', 'AlteracaoDeCampo', 'CampoAuditado', 'CampoPersonalizado', 'CanalContato', 'Carteira', 'Catalogo', 'CatalogoItem', 'ChaveExterna', 'Cliente', 'ClienteCarteira', 'ClienteContato', 'CompartilhamentoDeRegistro', 'ConjuntoPermissao', 'ConsentimentoComunicacao', 'Contato', 'Documento', 'Empresa', 'Endereco', 'Equipamento', 'Equipe', 'EquipeMembro', 'EventoDeAcesso', 'ExecucaoRegra', 'Familia', 'Fase', 'Fonte', 'FonteCampo', 'Formulario', 'HierarquiaComercial', 'Interacao', 'InteracaoParticipante', 'ItemConjuntoPermissao', 'ItemDeProposta', 'Lead', 'LeituraDeHorimetro', 'LinhaDeNegocio', 'Marca', 'MensagemDeSaida', 'MensagemDescartada', 'Meta', 'Modelo', 'MotivoDePerda', 'PassagemDeFase', 'Pergunta', 'Permissao', 'PontoDeSincronismo', 'Praca', 'Preenchimento', 'Processo', 'Recepcao', 'Regra', 'Relatorio', 'Resposta', 'Resultado', 'Sistema', 'Tarefa', 'TipoProcesso', 'TipoTarefa', 'TratadorDeEvento', 'Usuario', 'UsuarioConjuntoPermissao', 'Vinculo')`
- `CK_Permissao_Verbo`: `[Verbo] IN ('Ler','Criar','Editar','Excluir','Atribuir','Compartilhar')`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `Codigo` | `varchar(80)` | não |  |  | sim |  |
| `Descricao` | `nvarchar(200)` | não |  |  |  |  |
| `Entidade` | `varchar(40)` | não |  |  |  |  |
| `Id` | `int` | não | **PK** |  |  |  |
| `Verbo` | `varchar(20)` | não |  |  |  |  |

### `seguranca.Usuario`

**Check constraints:**
- `CK_Usuario_Desativacao`: `[EstaAtivo] = 1 OR [DesativadoEm] IS NOT NULL`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `AlteradoEm` | `datetime2(3)` | sim |  |  |  |  |
| `AlteradoPorId` | `bigint` | sim |  |  |  |  |
| `ChavePublica` | `uniqueidentifier` | não |  |  | sim |  |
| `CriadoEm` | `datetime2(3)` | não |  |  |  |  |
| `CriadoPorId` | `bigint` | não |  |  |  |  |
| `DesativadoEm` | `datetime2(3)` | sim |  |  |  |  |
| `Email` | `nvarchar(200)` | não |  |  |  |  |
| `EmpresaId` | `int` | não |  | **FK** |  | → `organizacao.Empresa` |
| `EstaAtivo` | `bit` | não |  |  |  |  |
| `ExcluidoEm` | `datetime2(3)` | sim |  |  |  |  |
| `GestorId` | `bigint` | sim |  | **FK** |  | → `seguranca.Usuario` |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `IdentidadeExterna` | `uniqueidentifier` | não |  |  | sim |  |
| `NomeCompleto` | `nvarchar(200)` | não |  |  |  |  |
| `NomeExibicao` | `nvarchar(80)` | não |  |  |  |  |
| `NomePrincipal` | `nvarchar(200)` | não |  |  | sim |  |
| `Papel` | `varchar(40)` | sim |  |  |  |  |
| `UltimoLoginEm` | `datetime2(3)` | sim |  |  |  |  |
| `Versao` | `rowversion` | sim |  |  |  |  |

### `seguranca.UsuarioConjuntoDePermissao`

| Coluna | Tipo | Anulável | PK | FK | Único | Observação |
|---|---|---|---|---|---|---|
| `ConcedidoEm` | `datetime2(3)` | não |  |  |  |  |
| `ConcedidoPorId` | `bigint` | não |  |  |  |  |
| `ConjuntoPermissaoId` | `int` | não |  | **FK** | sim | → `seguranca.ConjuntoDePermissao` |
| `ExpiraEm` | `datetime2(3)` | sim |  |  |  |  |
| `Id` | `bigint` | não | **PK** |  |  |  |
| `UsuarioId` | `bigint` | não |  | **FK** | sim | → `seguranca.Usuario` |

