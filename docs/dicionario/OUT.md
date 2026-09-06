# OUT - Modulo outbound / base auxiliar de pessoas

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**5 tabelas · 109 colunas · 3.746 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`OUT_Contato`](#out_contato) | isolada | 25 | 147 |
| [`OUT_Log`](#out_log) | isolada | 12 | 1.550 |
| [`OUT_Pessoa`](#out_pessoa) | isolada | 61 | 1.918 |
| [`OUT_PessoaLink`](#out_pessoalink) | isolada | 4 | 122 |
| [`OUT_PessoaRelacao`](#out_pessoarelacao) | isolada | 7 | 9 |

---

### OUT_Contato

`classe: isolada` · `25 colunas` · `147 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, SeqContato`

**Funcao:** OUT_CONTATO 'I' 147 e 'A' 9, parados em 2016/2017; (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `SeqContato` | `numeric(4,0)` | **nao** | - | **PK** |
| 3 | `Observacao` | `varchar(250)` | sim | - | texto livre |
| 4 | `LinkWeb` | `numeric(1,0)` | sim | - |  |
| 5 | `Email` | `varchar(50)` | sim | - |  |
| 6 | `DtaNascimento` | `datetime` | sim | - |  |
| 7 | `EstadoCivil` | `char(1)` | sim | - |  |
| 8 | `SEXO` | `char(1)` | sim | - |  |
| 9 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 10 | `FoneNro2` | `numeric(12,0)` | sim | - |  |
| 11 | `FoneCmpl2` | `varchar(12)` | sim | - |  |
| 12 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 13 | `FoneNro1` | `numeric(12,0)` | sim | - |  |
| 14 | `FoneCmpl1` | `varchar(12)` | sim | - |  |
| 15 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 16 | `FaxNro` | `numeric(12,0)` | sim | - |  |
| 17 | `Contato` | `varchar(40)` | sim | - |  |
| 18 | `Saudacao` | `varchar(20)` | sim | - |  |
| 19 | `DigCPF` | `numeric(2,0)` | sim | - |  |
| 20 | `CPF` | `numeric(18,0)` | sim | - |  |
| 21 | `RG` | `varchar(20)` | sim | - |  |
| 22 | `AreaAtuacao` | `varchar(20)` | sim | - |  |
| 23 | `TipoContato` | `varchar(30)` | sim | - |  |
| 24 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 25 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |

---

### OUT_Log

`classe: isolada` · `12 colunas` · `1.550 linhas (snapshot 03/06/2026)` · `PK: SeqLog`

**Funcao:** O defeito: OutOk = 0 em 100% das 1.765 linhas de OUT_Log — nenhum evento jamais foi confirmado como entregue, e OutDta nunca é preenchida. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqLog` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Tabela` | `varchar(20)` | sim | - |  |
| 3 | `NroEmpresa` | `numeric(6,0)` | sim | - | multiempresa - filial/empresa |
| 4 | `Chave1N` | `numeric(18,0)` | sim | - |  |
| 5 | `Chave2N` | `numeric(18,0)` | sim | - |  |
| 6 | `Chave1C` | `varchar(20)` | sim | - |  |
| 7 | `TpOper` | `char(1)` | sim | - |  |
| 8 | `DtaIns` | `datetime` | sim | - |  |
| 9 | `UsrIns` | `varchar(20)` | sim | - |  |
| 10 | `ObsIns` | `varchar(30)` | sim | - |  |
| 11 | `OutOk` | `numeric(1,0)` | sim | - |  |
| 12 | `OutDta` | `datetime` | sim | - |  |

---

### OUT_Pessoa

`classe: isolada` · `61 colunas` · `1.918 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de pessoa (cliente/prospect/contato), no modulo `OUT` (modulo outbound). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `NroCGCCPF` | `decimal(13,0)` | sim | - |  |
| 3 | `DigCGCCPF` | `decimal(2,0)` | sim | - |  |
| 4 | `PessoaLinkOrigem` | `varchar(30)` | sim | - |  |
| 5 | `Pessoalink` | `varchar(250)` | sim | - |  |
| 6 | `NomeRazao` | `varchar(100)` | sim | - |  |
| 7 | `Fantasia` | `varchar(50)` | sim | - |  |
| 8 | `Versao` | `decimal(2,0)` | sim | - |  |
| 9 | `Status` | `varchar(1)` | **nao** | - | status - validar dominio real por tabela |
| 10 | `DtaAtivacao` | `datetime` | sim | - |  |
| 11 | `FisicaJuridica` | `varchar(1)` | sim | - |  |
| 12 | `Sexo` | `varchar(1)` | sim | - |  |
| 13 | `Cidade` | `varchar(50)` | sim | - |  |
| 14 | `Uf` | `varchar(2)` | sim | - |  |
| 15 | `Pais` | `varchar(25)` | sim | - |  |
| 16 | `Bairro` | `varchar(50)` | sim | - |  |
| 17 | `TipoLogradouro` | `varchar(15)` | sim | - |  |
| 18 | `Logradouro` | `varchar(80)` | sim | - |  |
| 19 | `NroLogradouro` | `varchar(10)` | sim | - |  |
| 20 | `CmpltoLogradouro` | `varchar(30)` | sim | - |  |
| 21 | `Cep` | `varchar(12)` | sim | - |  |
| 22 | `CxPostal` | `varchar(7)` | sim | - |  |
| 23 | `Latitude` | `decimal(14,11)` | sim | - | geolocalizacao |
| 24 | `Longitude` | `decimal(14,11)` | sim | - | geolocalizacao |
| 25 | `FoneDDD1` | `varchar(5)` | sim | - |  |
| 26 | `FoneNro1` | `decimal(12,0)` | sim | - |  |
| 27 | `FoneCmpl1` | `varchar(20)` | sim | - |  |
| 28 | `FoneDDD2` | `varchar(5)` | sim | - |  |
| 29 | `FoneNro2` | `decimal(12,0)` | sim | - |  |
| 30 | `FoneCmpl2` | `varchar(20)` | sim | - |  |
| 31 | `FoneDDD3` | `varchar(5)` | sim | - |  |
| 32 | `FoneNro3` | `decimal(12,0)` | sim | - |  |
| 33 | `FoneCmpl3` | `varchar(20)` | sim | - |  |
| 34 | `FaxDDD` | `varchar(5)` | sim | - |  |
| 35 | `FaxNro` | `decimal(12,0)` | sim | - |  |
| 36 | `InscricaoRG` | `varchar(20)` | sim | - |  |
| 37 | `UFEmissor` | `varchar(2)` | sim | - |  |
| 38 | `OrgaoEmissor` | `varchar(10)` | sim | - |  |
| 39 | `DtaNascFund` | `datetime` | sim | - |  |
| 40 | `Email` | `varchar(70)` | sim | - |  |
| 41 | `HomePage` | `varchar(80)` | sim | - |  |
| 42 | `EstadoCivil` | `varchar(20)` | sim | - |  |
| 43 | `Atividade` | `varchar(30)` | sim | - |  |
| 44 | `RendaFaturamento` | `varchar(30)` | sim | - |  |
| 45 | `GrauInstrucao` | `varchar(30)` | sim | - |  |
| 46 | `Grupo` | `varchar(30)` | sim | - |  |
| 47 | `Porte` | `varchar(30)` | sim | - |  |
| 48 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 49 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 50 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 51 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 52 | `DtaInativacao` | `datetime` | sim | - | data de inativacao |
| 53 | `UsuInativacao` | `varchar(20)` | sim | - |  |
| 54 | `ObsInativacao` | `varchar(50)` | sim | - |  |
| 55 | `Telefonema` | `numeric(1,0)` | sim | - |  |
| 56 | `Correspondencia` | `numeric(1,0)` | sim | - |  |
| 57 | `RecebeEmail` | `numeric(1,0)` | sim | - |  |
| 58 | `NaoPossuiEmail` | `numeric(1,0)` | sim | - |  |
| 59 | `IndContribICMS` | `varchar(1)` | sim | - |  |
| 60 | `Origem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 61 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |

---

### OUT_PessoaLink

`classe: isolada` · `4 colunas` · `122 linhas (snapshot 03/06/2026)` · `PK: Pessoalink, Origem`

**Funcao:** _(inferido)_ Pelo nome, e uma tabela de vinculo/de-para relacionada a pessoa (cliente/prospect/contato), no modulo `OUT` (modulo outbound). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Pessoalink` | `varchar(30)` | **nao** | - | **PK** |
| 2 | `Origem` | `varchar(20)` | **nao** | - | **PK**; sistema de origem do dado |
| 3 | `SeqPessoa` | `numeric(10,0)` | sim | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 4 | `PessoaLinkNro` | `numeric(18,0)` | sim | - |  |

---

### OUT_PessoaRelacao

`classe: isolada` · `7 colunas` · `9 linhas (snapshot 03/06/2026)` · `PK: SeqPrincipal, SeqPessoa, TipoRelacionamento`

**Funcao:** _(inferido)_ Pelo nome, e um relacionamento relacionada a pessoa (cliente/prospect/contato), no modulo `OUT` (modulo outbound). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPrincipal` | `numeric(8,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `TipoRelacionamento` | `varchar(30)` | **nao** | - | **PK** |
| 4 | `Link` | `varchar(50)` | sim | - |  |
| 5 | `LinkOrigem` | `varchar(20)` | sim | - |  |
| 6 | `Obs` | `varchar(80)` | sim | - | texto livre |
| 7 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |

---
