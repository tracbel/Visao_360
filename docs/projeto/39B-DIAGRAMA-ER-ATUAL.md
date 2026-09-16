# Diagrama ER da arquitetura atual — anexo do documento 39

> Gerado por `scripts/banco/auditoria/gerar-inventario-do-banco.ps1` em 15/09/2026 13:24, a partir das
> 213 chaves estrangeiras reais do catálogo (`sys.foreign_keys`), sem interpretação. `||` = obrigatória, `|o` = opcional.
> Para não esconder o desenho atrás de dois hubs, as FKs para `Empresa` e `Usuario` só aparecem nos domínios deles;
> em cada outro domínio a quantidade omitida é informada. As tabelas de histórico de migração ficam de fora.

## Visão geral: FKs entre domínios

```mermaid
flowchart LR
    D1["IDENTIDADE E ACESSO<br/>8 tabelas"]
    D2["ORGANIZACAO E TERRITORIO<br/>12 tabelas"]
    D3["CRM - CLIENTE E RELACIONAMENTO<br/>9 tabelas"]
    D4["COMERCIAL - PIPELINE, ATIVIDADES E FATURAMENTO<br/>16 tabelas"]
    D5["FROTA<br/>8 tabelas"]
    D6["INTEGRACOES<br/>11 tabelas"]
    D7["EXTENSIBILIDADE E RELATORIOS<br/>11 tabelas"]
    D8["AUDITORIA E DOCUMENTOS<br/>5 tabelas"]
    D2 -- 10 --> D1
    D4 -- 10 --> D3
    D3 -- 10 --> D2
    D4 -- 9 --> D1
    D4 -- 9 --> D2
    D4 -- 6 --> D7
    D3 -- 6 --> D7
    D6 -- 5 --> D5
    D3 -- 5 --> D1
    D5 -- 4 --> D2
    D7 -- 4 --> D4
    D5 -- 4 --> D3
    D6 -- 3 --> D2
    D8 -- 3 --> D1
    D1 -- 3 --> D2
    D6 -- 2 --> D1
    D5 -- 2 --> D6
    D7 -- 2 --> D3
    D7 -- 2 --> D1
    D4 -- 2 --> D5
    D8 -- 2 --> D2
    D7 -- 2 --> D2
    D7 -- 1 --> D6
    D5 -- 1 --> D1
    D7 -- 1 --> D5
    D3 -- 1 --> D4
    D8 -- 1 --> D7
```

## IDENTIDADE E ACESSO

Tabelas: `seguranca.CompartilhamentoDeRegistro`, `seguranca.ConjuntoDePermissao`, `seguranca.ConjuntoDePermissaoItem`, `seguranca.Equipe`, `seguranca.EquipeMembro`, `seguranca.Permissao`, `seguranca.Usuario`, `seguranca.UsuarioConjuntoDePermissao`.

FKs de saída desenhadas: 8; omitidas para `Empresa`/`Usuario`: 3.

```mermaid
erDiagram
    CompartilhamentoDeRegistro {
        bigint Id PK
        int EmpresaId FK
        bigint EquipeId FK
        bigint UsuarioId FK
    }
    ConjuntoDePermissao {
        int Id PK
    }
    ConjuntoDePermissaoItem {
        int Id PK
        int ConjuntoPermissaoId FK
    }
    Equipe {
        bigint Id PK
        int EmpresaId FK
    }
    EquipeMembro {
        bigint Id PK
        bigint EquipeId FK
        bigint UsuarioId FK
    }
    Permissao {
        int Id PK
    }
    Usuario {
        bigint Id PK
        int EmpresaId FK
        bigint GestorId FK
    }
    UsuarioConjuntoDePermissao {
        bigint Id PK
        int ConjuntoPermissaoId FK
        bigint UsuarioId FK
    }
    ConjuntoDePermissao ||--o{ ConjuntoDePermissaoItem : "ConjuntoPermissaoId"
    ConjuntoDePermissao ||--o{ UsuarioConjuntoDePermissao : "ConjuntoPermissaoId"
    Equipe |o--o{ CompartilhamentoDeRegistro : "EquipeId"
    Equipe ||--o{ EquipeMembro : "EquipeId"
    Usuario |o--o{ CompartilhamentoDeRegistro : "UsuarioId"
    Usuario ||--o{ EquipeMembro : "UsuarioId"
    Usuario |o--o{ Usuario : "GestorId"
    Usuario ||--o{ UsuarioConjuntoDePermissao : "UsuarioId"
```

## ORGANIZACAO E TERRITORIO

Tabelas: `organizacao.AreaPlantadaNoMunicipio`, `organizacao.Carteira`, `organizacao.CarteiraMunicipio`, `organizacao.Empresa`, `organizacao.HierarquiaComercial`, `organizacao.LinhaDeNegocio`, `organizacao.Meta`, `organizacao.Municipio`, `organizacao.MunicipioDaAreaDeAtuacao`, `organizacao.Praca`, `organizacao.RegraDePotencial`, `organizacao.ResponsavelPeloMunicipio`.

FKs de saída desenhadas: 15 (referências a outros domínios: `seguranca.Equipe`); omitidas para `Empresa`/`Usuario`: 9.

```mermaid
erDiagram
    AreaPlantadaNoMunicipio {
        bigint Id PK
        bigint ImportadoPorId FK
        int MunicipioId FK
    }
    Carteira {
        bigint Id PK
        int EmpresaId FK
        bigint EquipeId FK
        int LinhaDeNegocioId FK
        int PracaId FK
        bigint ResponsavelId FK
        bigint SupervisorId FK
    }
    CarteiraMunicipio {
        bigint Id PK
        bigint CarteiraId FK
        int MunicipioId FK
    }
    Empresa {
        int Id PK
        int EmpresaPaiId FK
    }
    HierarquiaComercial {
        bigint Id PK
        bigint AncestralId FK
        bigint DescendenteId FK
    }
    LinhaDeNegocio {
        int Id PK
    }
    Meta {
        bigint Id PK
        bigint CarteiraId FK
        int EmpresaId FK
        int LinhaDeNegocioId FK
        bigint UsuarioId FK
    }
    Municipio {
        int Id PK
    }
    MunicipioDaAreaDeAtuacao {
        int Id PK
        int EmpresaResponsavelId FK
        bigint ImportadoPorId FK
        int MunicipioId FK
    }
    Praca {
        int Id PK
        int LinhaDeNegocioId FK
    }
    RegraDePotencial {
        int Id PK
    }
    ResponsavelPeloMunicipio {
        bigint Id PK
        bigint ImportadoPorId FK
        int MunicipioId FK
        bigint UsuarioId FK
    }
    Carteira ||--o{ CarteiraMunicipio : "CarteiraId"
    Carteira |o--o{ Meta : "CarteiraId"
    Empresa ||--o{ Carteira : "EmpresaId"
    Empresa |o--o{ Empresa : "EmpresaPaiId"
    Empresa ||--o{ Meta : "EmpresaId"
    Empresa |o--o{ MunicipioDaAreaDeAtuacao : "EmpresaResponsavelId"
    LinhaDeNegocio ||--o{ Carteira : "LinhaDeNegocioId"
    LinhaDeNegocio |o--o{ Meta : "LinhaDeNegocioId"
    LinhaDeNegocio ||--o{ Praca : "LinhaDeNegocioId"
    Municipio ||--o{ AreaPlantadaNoMunicipio : "MunicipioId"
    Municipio ||--o{ CarteiraMunicipio : "MunicipioId"
    Municipio ||--o{ MunicipioDaAreaDeAtuacao : "MunicipioId"
    Municipio ||--o{ ResponsavelPeloMunicipio : "MunicipioId"
    Praca |o--o{ Carteira : "PracaId"
    Equipe |o--o{ Carteira : "EquipeId"
```

## CRM - CLIENTE E RELACIONAMENTO

Tabelas: `comercial.Alerta`, `comercial.CanalContato`, `comercial.Cliente`, `comercial.ClienteCarteira`, `comercial.ClienteContato`, `comercial.ConsentimentoComunicacao`, `comercial.Contato`, `comercial.Endereco`, `comercial.Lead`.

FKs de saída desenhadas: 23 (referências a outros domínios: `organizacao.LinhaDeNegocio`, `processo.Processo`, `metadado.CatalogoItem`, `organizacao.Municipio`, `organizacao.Carteira`, `seguranca.Equipe`); omitidas para `Empresa`/`Usuario`: 11.

```mermaid
erDiagram
    Alerta {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
    }
    CanalContato {
        bigint Id PK
        bigint ClienteId FK
        bigint ContatoId FK
        int EmpresaId FK
    }
    Cliente {
        bigint Id PK
        bigint ClienteMatrizId FK
        int EmpresaId FK
        bigint ProprietarioEquipeId FK
        bigint ProprietarioId FK
    }
    ClienteCarteira {
        bigint Id PK
        bigint CarteiraId FK
        bigint ClienteId FK
    }
    ClienteContato {
        bigint Id PK
        bigint ClienteId FK
        bigint ContatoId FK
    }
    ConsentimentoComunicacao {
        bigint Id PK
        bigint ClienteId FK
        bigint ContatoId FK
        int EmpresaId FK
    }
    Contato {
        bigint Id PK
        int EmpresaId FK
        bigint ProprietarioId FK
    }
    Endereco {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
        int MunicipioId FK
    }
    Lead {
        bigint Id PK
        bigint ClienteGeradoId FK
        bigint ContatoGeradoId FK
        int EmpresaId FK
        int LinhaNegocioId FK
        bigint ProcessoGeradoId FK
        bigint ProprietarioId FK
        bigint QualificadoPorId FK
    }
    Cliente ||--o{ Alerta : "ClienteId"
    Cliente |o--o{ CanalContato : "ClienteId"
    Cliente |o--o{ Cliente : "ClienteMatrizId"
    Cliente ||--o{ ClienteCarteira : "ClienteId"
    Cliente ||--o{ ClienteContato : "ClienteId"
    Cliente |o--o{ ConsentimentoComunicacao : "ClienteId"
    Cliente ||--o{ Endereco : "ClienteId"
    Cliente |o--o{ Lead : "ClienteGeradoId"
    Contato |o--o{ CanalContato : "ContatoId"
    Contato ||--o{ ClienteContato : "ContatoId"
    Contato |o--o{ ConsentimentoComunicacao : "ContatoId"
    Contato |o--o{ Lead : "ContatoGeradoId"
    CatalogoItem |o--o{ Cliente : "CatalogoDaOrigemId,OrigemId"
    CatalogoItem |o--o{ Cliente : "CatalogoDoMotivoInativacaoId,MotivoInativacaoId"
    CatalogoItem ||--o{ ClienteContato : "CatalogoDoPapelId,PapelId"
    CatalogoItem |o--o{ Endereco : "CatalogoDaCulturaId,CulturaId"
    CatalogoItem |o--o{ Lead : "CatalogoDoMotivoDescarteId,MotivoDescarteId"
    CatalogoItem ||--o{ Lead : "CatalogoDaOrigemId,OrigemId"
    Carteira ||--o{ ClienteCarteira : "CarteiraId"
    LinhaDeNegocio |o--o{ Lead : "LinhaNegocioId"
    Municipio |o--o{ Endereco : "MunicipioId"
    Processo |o--o{ Lead : "ProcessoGeradoId"
    Equipe |o--o{ Cliente : "ProprietarioEquipeId"
```

## COMERCIAL - PIPELINE, ATIVIDADES E FATURAMENTO

Tabelas: `comercial.FaturamentoDoCliente`, `comercial.FaturamentoSemCliente`, `processo.Fase`, `processo.Interacao`, `processo.InteracaoParticipante`, `processo.ItemDeProposta`, `processo.MotivoDePerda`, `processo.PassagemDeFase`, `processo.Processo`, `processo.Regra`, `processo.RegraExecucao`, `processo.Resultado`, `processo.Tarefa`, `processo.TipoProcesso`, `processo.TipoTarefa`, `processo.VendaPerdida`.

FKs de saída desenhadas: 57 (referências a outros domínios: `frota.Equipamento`, `frota.Modelo`, `metadado.CatalogoItem`, `comercial.Cliente`, `organizacao.LinhaDeNegocio`, `comercial.Contato`, `comercial.Lead`, `seguranca.Equipe`, `organizacao.Carteira`, `metadado.Formulario`); omitidas para `Empresa`/`Usuario`: 14.

```mermaid
erDiagram
    FaturamentoDoCliente {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
    }
    FaturamentoSemCliente {
        bigint Id PK
        int EmpresaId FK
    }
    Fase {
        int Id PK
        int TipoProcessoId FK
    }
    Interacao {
        bigint Id PK
        bigint ClienteId FK
        bigint ContatoId FK
        int EmpresaId FK
        bigint LeadId FK
        bigint ProcessoId FK
        bigint RegistradoPorId FK
        int ResultadoId FK
        bigint TarefaId FK
        int TipoTarefaId FK
    }
    InteracaoParticipante {
        bigint Id PK
        bigint ContatoId FK
        bigint InteracaoId FK
        bigint UsuarioId FK
    }
    ItemDeProposta {
        bigint Id PK
        int EmpresaId FK
        bigint EquipamentoId FK
        int ModeloId FK
        bigint ProcessoId FK
    }
    MotivoDePerda {
        int Id PK
    }
    PassagemDeFase {
        bigint Id PK
        bigint EntrouPorId FK
        int FaseId FK
        bigint InteracaoOrigemId FK
        bigint ProcessoId FK
        int RegraOrigemId FK
    }
    Processo {
        bigint Id PK
        bigint CarteiraId FK
        bigint ClienteId FK
        bigint ContatoId FK
        int EmpresaId FK
        int FaseId FK
        int MotivoDePerdaId FK
        bigint ProprietarioEquipeId FK
        bigint ProprietarioId FK
        int TipoProcessoId FK
    }
    Regra {
        int Id PK
        int EfeitoFaseId FK
        int EfeitoTipoTarefaId FK
        int ResultadoId FK
        int TipoProcessoId FK
        int TipoTarefaId FK
    }
    RegraExecucao {
        bigint Id PK
        bigint DestinatarioResolvidoId FK
        bigint InteracaoId FK
        bigint ProcessoId FK
        int RegraId FK
        bigint TarefaCriadaId FK
        bigint TarefaId FK
    }
    Resultado {
        int Id PK
        int FaseDestinoId FK
        int TipoTarefaId FK
    }
    Tarefa {
        bigint Id PK
        bigint ClienteId FK
        bigint ConcluidaPorId FK
        bigint ContatoId FK
        int CriadaPorRegraId FK
        int EmpresaId FK
        bigint InteracaoConclusaoId FK
        bigint InteracaoOrigemId FK
        bigint ProcessoId FK
        bigint ResponsavelEquipeId FK
        bigint ResponsavelId FK
        int ResultadoId FK
        int TipoTarefaId FK
    }
    TipoProcesso {
        int Id PK
        int LinhaDeNegocioId FK
    }
    TipoTarefa {
        int Id PK
        int FormularioId FK
        int TipoProcessoId FK
    }
    VendaPerdida {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
        int MotivoDePerdaId FK
        bigint ProcessoId FK
    }
    Cliente ||--o{ FaturamentoDoCliente : "ClienteId"
    Cliente |o--o{ Interacao : "ClienteId"
    Cliente ||--o{ Processo : "ClienteId"
    Cliente |o--o{ Tarefa : "ClienteId"
    Cliente |o--o{ VendaPerdida : "ClienteId"
    Contato |o--o{ Interacao : "ContatoId"
    Contato |o--o{ InteracaoParticipante : "ContatoId"
    Contato |o--o{ Processo : "ContatoId"
    Contato |o--o{ Tarefa : "ContatoId"
    Lead |o--o{ Interacao : "LeadId"
    Equipamento |o--o{ ItemDeProposta : "EquipamentoId"
    Modelo |o--o{ ItemDeProposta : "ModeloId"
    CatalogoItem |o--o{ ItemDeProposta : "CatalogoDaCondicaoPagamentoId,CondicaoPagamentoId"
    CatalogoItem |o--o{ Processo : "CatalogoDoConcorrenteId,ConcorrenteId"
    CatalogoItem |o--o{ VendaPerdida : "CatalogoDaRevendaId,RevendaDoConcorrenteId"
    CatalogoItem |o--o{ VendaPerdida : "CatalogoDoConcorrenteId,ConcorrenteId"
    CatalogoItem |o--o{ VendaPerdida : "CatalogoDoTipoDeEquipamentoId,TipoDeEquipamentoId"
    Formulario |o--o{ TipoTarefa : "FormularioId"
    Carteira |o--o{ Processo : "CarteiraId"
    LinhaDeNegocio |o--o{ TipoProcesso : "LinhaDeNegocioId"
    Fase ||--o{ PassagemDeFase : "FaseId"
    Fase ||--o{ Processo : "FaseId"
    Fase |o--o{ Regra : "EfeitoFaseId"
    Fase |o--o{ Resultado : "FaseDestinoId"
    Interacao ||--o{ InteracaoParticipante : "InteracaoId"
    Interacao |o--o{ PassagemDeFase : "InteracaoOrigemId"
    Interacao |o--o{ RegraExecucao : "InteracaoId"
    Interacao |o--o{ Tarefa : "InteracaoConclusaoId"
    Interacao |o--o{ Tarefa : "InteracaoOrigemId"
    MotivoDePerda |o--o{ Processo : "MotivoDePerdaId"
    MotivoDePerda ||--o{ VendaPerdida : "MotivoDePerdaId"
    Processo |o--o{ Interacao : "ProcessoId"
    Processo ||--o{ ItemDeProposta : "ProcessoId"
    Processo ||--o{ PassagemDeFase : "ProcessoId"
    Processo |o--o{ RegraExecucao : "ProcessoId"
    Processo |o--o{ Tarefa : "ProcessoId"
    Processo |o--o{ VendaPerdida : "ProcessoId"
    Regra |o--o{ PassagemDeFase : "RegraOrigemId"
    Regra ||--o{ RegraExecucao : "RegraId"
    Regra |o--o{ Tarefa : "CriadaPorRegraId"
    Resultado |o--o{ Interacao : "ResultadoId"
    Resultado |o--o{ Regra : "ResultadoId"
    Resultado |o--o{ Tarefa : "ResultadoId"
    Tarefa |o--o{ Interacao : "TarefaId"
    Tarefa |o--o{ RegraExecucao : "TarefaCriadaId"
    Tarefa |o--o{ RegraExecucao : "TarefaId"
    TipoProcesso ||--o{ Fase : "TipoProcessoId"
    TipoProcesso ||--o{ Processo : "TipoProcessoId"
    TipoProcesso |o--o{ Regra : "TipoProcessoId"
    TipoProcesso |o--o{ TipoTarefa : "TipoProcessoId"
    TipoTarefa ||--o{ Interacao : "TipoTarefaId"
    TipoTarefa |o--o{ Regra : "TipoTarefaId"
    TipoTarefa |o--o{ Regra : "EfeitoTipoTarefaId"
    TipoTarefa ||--o{ Resultado : "TipoTarefaId"
    TipoTarefa ||--o{ Tarefa : "TipoTarefaId"
    Equipe |o--o{ Processo : "ProprietarioEquipeId"
    Equipe |o--o{ Tarefa : "ResponsavelEquipeId"
```

## FROTA

Tabelas: `frota.Equipamento`, `frota.Familia`, `frota.LeituraDeHorimetro`, `frota.LinhaDeProduto`, `frota.Marca`, `frota.Modelo`, `frota.VendaDeMaquina`, `frota.VinculoDeClienteComEquipamento`.

FKs de saída desenhadas: 17 (referências a outros domínios: `comercial.Cliente`, `integracao.Sistema`, `comercial.Endereco`); omitidas para `Empresa`/`Usuario`: 5.

```mermaid
erDiagram
    Equipamento {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
        bigint EnderecoId FK
        bigint EquipamentoPaiId FK
        bigint EquipamentoSubstitutoId FK
        int LinhaDeProdutoId FK
        int ModeloId FK
    }
    Familia {
        int Id PK
        int MarcaId FK
    }
    LeituraDeHorimetro {
        bigint Id PK
        bigint EquipamentoId FK
        bigint RegistradoPorId FK
    }
    LinhaDeProduto {
        int Id PK
        int FamiliaId FK
    }
    Marca {
        int Id PK
    }
    Modelo {
        int Id PK
        int FamiliaId FK
    }
    VendaDeMaquina {
        bigint Id PK
        bigint CompradorId FK
        int EmpresaDoFaturamentoId FK
        int EmpresaId FK
        bigint EquipamentoId FK
        int SistemaId FK
    }
    VinculoDeClienteComEquipamento {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
        bigint EquipamentoId FK
        int SistemaId FK
        bigint VendaDeMaquinaId FK
    }
    Cliente |o--o{ Equipamento : "ClienteId"
    Cliente ||--o{ VendaDeMaquina : "CompradorId"
    Cliente ||--o{ VinculoDeClienteComEquipamento : "ClienteId"
    Endereco |o--o{ Equipamento : "EnderecoId"
    Equipamento |o--o{ Equipamento : "EquipamentoSubstitutoId"
    Equipamento |o--o{ Equipamento : "EquipamentoPaiId"
    Equipamento ||--o{ LeituraDeHorimetro : "EquipamentoId"
    Equipamento ||--o{ VendaDeMaquina : "EquipamentoId"
    Equipamento ||--o{ VinculoDeClienteComEquipamento : "EquipamentoId"
    Familia |o--o{ LinhaDeProduto : "FamiliaId"
    Familia ||--o{ Modelo : "FamiliaId"
    LinhaDeProduto |o--o{ Equipamento : "LinhaDeProdutoId"
    Marca ||--o{ Familia : "MarcaId"
    Modelo |o--o{ Equipamento : "ModeloId"
    VendaDeMaquina |o--o{ VinculoDeClienteComEquipamento : "VendaDeMaquinaId"
    Sistema ||--o{ VendaDeMaquina : "SistemaId"
    Sistema |o--o{ VinculoDeClienteComEquipamento : "SistemaId"
```

## INTEGRACOES

Tabelas: `integracao.ChaveExterna`, `integracao.CompradorPendente`, `integracao.CorrespondenciaDaOrigem`, `integracao.DivergenciaDeIntegracao`, `integracao.ExecucaoDeSincronizacao`, `integracao.MensagemDeSaida`, `integracao.MensagemDescartada`, `integracao.PontoDeSincronismo`, `integracao.Recepcao`, `integracao.RegistroDeOrigem`, `integracao.Sistema`.

FKs de saída desenhadas: 15 (referências a outros domínios: `frota.LinhaDeProduto`, `frota.Modelo`, `frota.Equipamento`, `frota.VendaDeMaquina`); omitidas para `Empresa`/`Usuario`: 5.

```mermaid
erDiagram
    ChaveExterna {
        bigint Id PK
        int SistemaId FK
    }
    CompradorPendente {
        bigint Id PK
        int EmpresaId FK
        int SistemaId FK
    }
    CorrespondenciaDaOrigem {
        int Id PK
        int EmpresaCorrespondenteId FK
        int LinhaDeProdutoId FK
        int ModeloId FK
        bigint RevisadaPorId FK
        int SistemaId FK
    }
    DivergenciaDeIntegracao {
        bigint Id PK
        int EmpresaId FK
        bigint EquipamentoId FK
        int SistemaId FK
        bigint VendaDeMaquinaId FK
    }
    ExecucaoDeSincronizacao {
        bigint Id PK
        int SistemaId FK
    }
    MensagemDeSaida {
        bigint Id PK
        int SistemaId FK
    }
    MensagemDescartada {
        bigint Id PK
        bigint MensagemDeSaidaId FK
        bigint TratadaPorId FK
    }
    PontoDeSincronismo {
        int Id PK
        int SistemaId FK
    }
    Recepcao {
        bigint Id PK
        int SistemaId FK
    }
    RegistroDeOrigem {
        bigint Id PK
        int SistemaId FK
        bigint VendaDeMaquinaId FK
    }
    Sistema {
        int Id PK
    }
    Equipamento |o--o{ DivergenciaDeIntegracao : "EquipamentoId"
    LinhaDeProduto |o--o{ CorrespondenciaDaOrigem : "LinhaDeProdutoId"
    Modelo |o--o{ CorrespondenciaDaOrigem : "ModeloId"
    VendaDeMaquina |o--o{ DivergenciaDeIntegracao : "VendaDeMaquinaId"
    VendaDeMaquina |o--o{ RegistroDeOrigem : "VendaDeMaquinaId"
    MensagemDeSaida |o--o{ MensagemDescartada : "MensagemDeSaidaId"
    Sistema ||--o{ ChaveExterna : "SistemaId"
    Sistema ||--o{ CompradorPendente : "SistemaId"
    Sistema ||--o{ CorrespondenciaDaOrigem : "SistemaId"
    Sistema ||--o{ DivergenciaDeIntegracao : "SistemaId"
    Sistema ||--o{ ExecucaoDeSincronizacao : "SistemaId"
    Sistema ||--o{ MensagemDeSaida : "SistemaId"
    Sistema ||--o{ PontoDeSincronismo : "SistemaId"
    Sistema ||--o{ Recepcao : "SistemaId"
    Sistema ||--o{ RegistroDeOrigem : "SistemaId"
```

## EXTENSIBILIDADE E RELATORIOS

Tabelas: `metadado.CampoPersonalizado`, `metadado.Catalogo`, `metadado.CatalogoItem`, `metadado.Formulario`, `metadado.Pergunta`, `metadado.Preenchimento`, `metadado.Resposta`, `metadado.TratadorDeEvento`, `relatorio.Fonte`, `relatorio.FonteCampo`, `relatorio.Relatorio`.

FKs de saída desenhadas: 20 (referências a outros domínios: `processo.TipoProcesso`, `integracao.Sistema`, `comercial.Cliente`, `processo.Interacao`, `processo.Processo`, `processo.Tarefa`, `frota.Equipamento`); omitidas para `Empresa`/`Usuario`: 4.

```mermaid
erDiagram
    CampoPersonalizado {
        int Id PK
        int CatalogoId FK
    }
    Catalogo {
        int Id PK
    }
    CatalogoItem {
        int Id PK
        int CatalogoId FK
    }
    Formulario {
        int Id PK
        int TipoProcessoId FK
    }
    Pergunta {
        int Id PK
        int CatalogoId FK
        int FormularioId FK
        int PerguntaCondicaoId FK
    }
    Preenchimento {
        bigint Id PK
        bigint ClienteId FK
        int EmpresaId FK
        int FormularioId FK
        bigint InteracaoId FK
        bigint PreenchidoPorId FK
        bigint ProcessoId FK
        bigint TarefaId FK
    }
    Resposta {
        bigint Id PK
        int CatalogoItemId FK
        bigint ClienteId FK
        bigint EquipamentoId FK
        int PerguntaId FK
        bigint PreenchimentoId FK
    }
    TratadorDeEvento {
        int Id PK
    }
    Fonte {
        int Id PK
        int SistemaId FK
    }
    FonteCampo {
        int Id PK
        int FonteId FK
    }
    Relatorio {
        bigint Id PK
        int EmpresaId FK
        int FonteId FK
        bigint ProprietarioId FK
    }
    Cliente |o--o{ Preenchimento : "ClienteId"
    Cliente |o--o{ Resposta : "ClienteId"
    Equipamento |o--o{ Resposta : "EquipamentoId"
    Sistema |o--o{ Fonte : "SistemaId"
    Catalogo |o--o{ CampoPersonalizado : "CatalogoId"
    Catalogo ||--o{ CatalogoItem : "CatalogoId"
    Catalogo |o--o{ Pergunta : "CatalogoId"
    CatalogoItem |o--o{ CatalogoItem : "CatalogoId,ItemPaiId"
    CatalogoItem |o--o{ Resposta : "CatalogoItemId"
    Formulario ||--o{ Pergunta : "FormularioId"
    Formulario ||--o{ Preenchimento : "FormularioId"
    Pergunta |o--o{ Pergunta : "PerguntaCondicaoId"
    Pergunta ||--o{ Resposta : "PerguntaId"
    Preenchimento ||--o{ Resposta : "PreenchimentoId"
    Interacao |o--o{ Preenchimento : "InteracaoId"
    Processo |o--o{ Preenchimento : "ProcessoId"
    Tarefa |o--o{ Preenchimento : "TarefaId"
    TipoProcesso |o--o{ Formulario : "TipoProcessoId"
    Fonte ||--o{ FonteCampo : "FonteId"
    Fonte ||--o{ Relatorio : "FonteId"
```

## AUDITORIA E DOCUMENTOS

Tabelas: `auditoria.AlteracaoDeCampo`, `auditoria.CampoAuditado`, `auditoria.EventoDeAcesso`, `documento.Documento`, `documento.Vinculo`.

FKs de saída desenhadas: 2 (referências a outros domínios: `metadado.CatalogoItem`); omitidas para `Empresa`/`Usuario`: 5.

```mermaid
erDiagram
    AlteracaoDeCampo {
        bigint Id PK
        bigint AlteradoPorId FK
        int EmpresaId FK
    }
    CampoAuditado {
        int Id PK
    }
    EventoDeAcesso {
        bigint Id PK
        bigint UsuarioId FK
    }
    Documento {
        bigint Id PK
        int EmpresaId FK
    }
    Vinculo {
        bigint Id PK
        bigint DocumentoId FK
        bigint VinculadoPorId FK
    }
    Documento ||--o{ Vinculo : "DocumentoId"
    CatalogoItem ||--o{ Documento : "CatalogoDoTipoDocumentoId,TipoDocumentoId"
```

