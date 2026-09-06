# Views - colunas conhecidas, SQL desconhecido

> Snapshot de 03/06/2026. Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao.**

`schema/colunas.csv` cobre **1.178 objetos**: as 767 tabelas **e** as 411 views. Ou seja, a **assinatura** (colunas e tipos) de cada view esta no catalogo - o que **nao** esta e o `SELECT` que a define, porque `sys.sql_modules.definition` volta NULL para a conta de leitura (falta `VIEW DEFINITION`; ver `REGRAS-DE-NEGOCIO.md` 2.8 e [LACUNAS.md](LACUNAS.md) item 1).

Sem o SQL nao da para saber **de onde** cada coluna vem. Use este arquivo para saber **o que** cada view entrega.

[Voltar ao indice](00-INDICE.md)

## Familias de view

| Familia | Views | Colunas | Provavel papel |
|---|---:|---:|---|
| `(outras)*` | 218 | 6.411 | nao identificado pelo nome |
| `IV$*` | 140 | 1.831 | gerada por propriedade customizada (`IV_Propriedade`) |
| `BI_*` | 38 | 605 | consumo por BI / QlikView |
| `V_*` | 7 | 62 | view utilitaria |
| `X_*` | 4 | 64 | integracao TOTVS |
| `X_V_*` | 3 | 115 | materializacao/extracao da integracao TOTVS |
| `VW_REL*` | 1 | 30 | relatorio da aplicacao (`VW_REL_TBA*` - telas de relatorio) |
| **TOTAL** | **411** | **9.118** | |

---

## Familia `(outras)*` (218 views)

### GE$CONTATO_FULL

`view` · `49 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQCONTATO` | `decimal(4,0)` | **nao** |
| 3 | `EMUSO` | `numeric(1,0)` | sim |
| 4 | `TIPOCONTATO` | `varchar(30)` | sim |
| 5 | `AREAATUACAO` | `varchar(20)` | sim |
| 6 | `RG` | `varchar(20)` | sim |
| 7 | `CPF` | `numeric(18,0)` | sim |
| 8 | `DIGCPF` | `decimal(2,0)` | sim |
| 9 | `SAUDACAO` | `varchar(20)` | sim |
| 10 | `CONTATO` | `varchar(40)` | sim |
| 11 | `FONEDDD1` | `varchar(5)` | sim |
| 12 | `FONENRO1` | `decimal(12,0)` | sim |
| 13 | `FONECMPL1` | `varchar(12)` | sim |
| 14 | `FONEDDD2` | `varchar(5)` | sim |
| 15 | `FONENRO2` | `decimal(12,0)` | sim |
| 16 | `FONECMPL2` | `varchar(12)` | sim |
| 17 | `FAXDDD` | `varchar(5)` | sim |
| 18 | `FAXNRO` | `decimal(12,0)` | sim |
| 19 | `SEXO` | `char(1)` | sim |
| 20 | `ESTADOCIVIL` | `char(1)` | sim |
| 21 | `DTANASCIMENTO` | `datetime` | sim |
| 22 | `NIVELDECISAO` | `char(1)` | sim |
| 23 | `POSICIONAMENTO` | `char(1)` | sim |
| 24 | `OBSPESSOAL` | `varchar(250)` | sim |
| 25 | `ATRIBUTO1` | `varchar(20)` | sim |
| 26 | `ATRIBUTO2` | `varchar(20)` | sim |
| 27 | `ATRIBUTO3` | `varchar(20)` | sim |
| 28 | `ATRIBDTA` | `datetime` | sim |
| 29 | `ATRIBNUM` | `numeric(18,0)` | sim |
| 30 | `EMAIL` | `varchar(50)` | sim |
| 31 | `SKYPE` | `varchar(70)` | sim |
| 32 | `LINKWEB` | `numeric(1,0)` | sim |
| 33 | `ULTORIGEM` | `varchar(20)` | sim |
| 34 | `USUALTERACAO` | `varchar(20)` | sim |
| 35 | `DTAALTERACAO` | `datetime` | sim |
| 36 | `OBSERVACAO` | `varchar(250)` | sim |
| 37 | `INDWHATSAPPF1` | `numeric(1,0)` | sim |
| 38 | `INDWHATSAPPF2` | `numeric(1,0)` | sim |
| 39 | `INDWHATSAPPFX` | `numeric(1,0)` | sim |
| 40 | `z_RG` | `varchar(20)` | sim |
| 41 | `z_CPF` | `numeric(18,0)` | sim |
| 42 | `z_SAUDACAO` | `varchar(20)` | sim |
| 43 | `z_CONTATO` | `varchar(40)` | sim |
| 44 | `z_FONENRO1` | `decimal(12,0)` | sim |
| 45 | `z_FONENRO2` | `decimal(12,0)` | sim |
| 46 | `z_FAXNRO` | `decimal(12,0)` | sim |
| 47 | `z_DTANASCIMENTO` | `datetime` | sim |
| 48 | `z_EMAIL` | `varchar(50)` | sim |
| 49 | `z_SKYPE` | `varchar(70)` | sim |

### GE$CONTATO_LGPD

`view` · `49 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQCONTATO` | `decimal(4,0)` | **nao** |
| 3 | `EMUSO` | `numeric(1,0)` | sim |
| 4 | `TIPOCONTATO` | `varchar(30)` | sim |
| 5 | `AREAATUACAO` | `varchar(20)` | sim |
| 6 | `RG` | `int(10,0)` | sim |
| 7 | `CPF` | `numeric(4,0)` | sim |
| 8 | `DIGCPF` | `decimal(2,0)` | sim |
| 9 | `SAUDACAO` | `varchar(250)` | sim |
| 10 | `CONTATO` | `varchar(250)` | sim |
| 11 | `FONEDDD1` | `varchar(5)` | sim |
| 12 | `FONENRO1` | `decimal(5,0)` | sim |
| 13 | `FONECMPL1` | `varchar(12)` | sim |
| 14 | `FONEDDD2` | `varchar(5)` | sim |
| 15 | `FONENRO2` | `decimal(5,0)` | sim |
| 16 | `FONECMPL2` | `varchar(12)` | sim |
| 17 | `FAXDDD` | `varchar(5)` | sim |
| 18 | `FAXNRO` | `decimal(5,0)` | sim |
| 19 | `SEXO` | `char(1)` | sim |
| 20 | `ESTADOCIVIL` | `char(1)` | sim |
| 21 | `DTANASCIMENTO` | `datetime` | sim |
| 22 | `NIVELDECISAO` | `char(1)` | sim |
| 23 | `POSICIONAMENTO` | `char(1)` | sim |
| 24 | `OBSPESSOAL` | `varchar(250)` | sim |
| 25 | `ATRIBUTO1` | `varchar(20)` | sim |
| 26 | `ATRIBUTO2` | `varchar(20)` | sim |
| 27 | `ATRIBUTO3` | `varchar(20)` | sim |
| 28 | `ATRIBDTA` | `datetime` | sim |
| 29 | `ATRIBNUM` | `numeric(18,0)` | sim |
| 30 | `EMAIL` | `varchar(250)` | sim |
| 31 | `SKYPE` | `varchar(250)` | sim |
| 32 | `LINKWEB` | `numeric(1,0)` | sim |
| 33 | `ULTORIGEM` | `varchar(20)` | sim |
| 34 | `USUALTERACAO` | `varchar(20)` | sim |
| 35 | `DTAALTERACAO` | `datetime` | sim |
| 36 | `OBSERVACAO` | `varchar(250)` | sim |
| 37 | `INDWHATSAPPF1` | `numeric(1,0)` | sim |
| 38 | `INDWHATSAPPF2` | `numeric(1,0)` | sim |
| 39 | `INDWHATSAPPFX` | `numeric(1,0)` | sim |
| 40 | `z_RG` | `varchar(20)` | sim |
| 41 | `z_CPF` | `numeric(18,0)` | sim |
| 42 | `z_SAUDACAO` | `varchar(20)` | sim |
| 43 | `z_CONTATO` | `varchar(40)` | sim |
| 44 | `z_FONENRO1` | `decimal(12,0)` | sim |
| 45 | `z_FONENRO2` | `decimal(12,0)` | sim |
| 46 | `z_FAXNRO` | `decimal(12,0)` | sim |
| 47 | `z_DTANASCIMENTO` | `datetime` | sim |
| 48 | `z_EMAIL` | `varchar(50)` | sim |
| 49 | `z_SKYPE` | `varchar(70)` | sim |

### GE$EMAIL_FULL

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQEMAIL` | `numeric(10,0)` | **nao** |
| 2 | `EMAIL` | `varchar(70)` | **nao** |
| 3 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 4 | `SEQUSUARIO` | `numeric(18,0)` | sim |
| 5 | `INDPREFERENCIAL` | `numeric(1,0)` | sim |
| 6 | `INDUSOMKT` | `numeric(1,0)` | sim |
| 7 | `INDUSOPROFISSIONAL` | `numeric(1,0)` | sim |
| 8 | `INDUSOPESSOAL` | `numeric(1,0)` | sim |
| 9 | `INDUSOFISCAL` | `numeric(1,0)` | sim |
| 10 | `INDEMUSO` | `numeric(1,0)` | sim |
| 11 | `SENHA` | `varchar(50)` | sim |
| 12 | `DTAALTERACAO` | `datetime` | sim |
| 13 | `USUALTEROU` | `varchar(20)` | sim |
| 14 | `OBS` | `varchar(50)` | sim |
| 15 | `MOTIVO` | `varchar(30)` | sim |
| 16 | `DTAEMAILATIVO` | `datetime` | sim |
| 17 | `z_EMAIL` | `varchar(70)` | **nao** |

### GE$EMAIL_LGPD

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQEMAIL` | `numeric(10,0)` | **nao** |
| 2 | `EMAIL` | `varchar(8000)` | sim |
| 3 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 4 | `SEQUSUARIO` | `numeric(18,0)` | sim |
| 5 | `INDPREFERENCIAL` | `numeric(1,0)` | sim |
| 6 | `INDUSOMKT` | `numeric(1,0)` | sim |
| 7 | `INDUSOPROFISSIONAL` | `numeric(1,0)` | sim |
| 8 | `INDUSOPESSOAL` | `numeric(1,0)` | sim |
| 9 | `INDUSOFISCAL` | `numeric(1,0)` | sim |
| 10 | `INDEMUSO` | `numeric(1,0)` | sim |
| 11 | `SENHA` | `varchar(50)` | sim |
| 12 | `DTAALTERACAO` | `datetime` | sim |
| 13 | `USUALTEROU` | `varchar(20)` | sim |
| 14 | `OBS` | `varchar(50)` | sim |
| 15 | `MOTIVO` | `varchar(30)` | sim |
| 16 | `DTAEMAILATIVO` | `datetime` | sim |
| 17 | `z_EMAIL` | `varchar(70)` | **nao** |

### GE$MODULOPERM

`view` · `4 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SISTEMA` | `varchar(20)` | **nao** |
| 2 | `MODULO` | `varchar(20)` | **nao** |
| 3 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 4 | `SEQUSUARIO` | `numeric(18,0)` | sim |

### GE$OBJDINAMICO

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQOBJDYN` | `numeric(18,0)` | **nao** |
| 2 | `VF` | `int(10,0)` | **nao** |
| 3 | `DESCRICAO` | `varchar(100)` | sim |
| 4 | `USO` | `varchar(200)` | sim |
| 5 | `TIPO` | `varchar(20)` | sim |
| 6 | `USOJUNCAOPESSOA` | `numeric(1,0)` | sim |
| 7 | `USOPESSOA` | `numeric(1,0)` | sim |
| 8 | `USOPROCESSO` | `numeric(1,0)` | sim |
| 9 | `USOAGENDA` | `numeric(1,0)` | sim |
| 10 | `USOHISTORICO` | `numeric(1,0)` | sim |
| 11 | `USOACAO` | `numeric(1,0)` | sim |
| 12 | `USORESULTADO` | `numeric(1,0)` | sim |
| 13 | `USOREQRESULTADO` | `numeric(1,0)` | sim |
| 14 | `USORESULTADOCMPL` | `numeric(1,0)` | sim |
| 15 | `USOWORKFLOW` | `numeric(1,0)` | sim |
| 16 | `DESCRICAOOBJ` | `varchar(50)` | sim |

### GE$PESSOA

`view` · `87 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqCidade` | `decimal(6,0)` | sim |
| 3 | `SeqBairro` | `decimal(5,0)` | sim |
| 4 | `Versao` | `decimal(2,0)` | sim |
| 5 | `Status` | `char(1)` | **nao** |
| 6 | `DtaAtivacao` | `datetime` | sim |
| 7 | `NomeRazao` | `varchar(60)` | **nao** |
| 8 | `Fantasia` | `varchar(50)` | sim |
| 9 | `PalavraChave` | `varchar(50)` | sim |
| 10 | `FisicaJuridica` | `char(1)` | sim |
| 11 | `Sexo` | `char(1)` | sim |
| 12 | `Cidade` | `varchar(50)` | sim |
| 13 | `Uf` | `varchar(2)` | sim |
| 14 | `Pais` | `varchar(25)` | sim |
| 15 | `Bairro` | `varchar(50)` | sim |
| 16 | `TipoLogradouro` | `varchar(15)` | sim |
| 17 | `Logradouro` | `varchar(80)` | sim |
| 18 | `NroLogradouro` | `varchar(10)` | sim |
| 19 | `CmpltoLogradouro` | `varchar(30)` | sim |
| 20 | `Cep` | `varchar(12)` | sim |
| 21 | `CxPostal` | `varchar(7)` | sim |
| 22 | `SeqPessoaEndCobr` | `decimal(3,0)` | sim |
| 23 | `FoneDDD1` | `varchar(5)` | sim |
| 24 | `FoneNro1` | `decimal(12,0)` | sim |
| 25 | `FoneCmpl1` | `varchar(12)` | sim |
| 26 | `FoneDDD2` | `varchar(5)` | sim |
| 27 | `FoneNro2` | `decimal(12,0)` | sim |
| 28 | `FoneCmpl2` | `varchar(12)` | sim |
| 29 | `FoneDDD3` | `varchar(5)` | sim |
| 30 | `FoneNro3` | `decimal(12,0)` | sim |
| 31 | `FoneCmpl3` | `varchar(12)` | sim |
| 32 | `FaxDDD` | `varchar(5)` | sim |
| 33 | `FaxNro` | `decimal(8,0)` | sim |
| 34 | `NroCGCCPF` | `decimal(13,0)` | sim |
| 35 | `DigCGCCPF` | `decimal(2,0)` | sim |
| 36 | `InscricaoRG` | `varchar(20)` | sim |
| 37 | `UFEmissor` | `varchar(2)` | sim |
| 38 | `OrgaoEmissor` | `varchar(10)` | sim |
| 39 | `InscMunic` | `varchar(15)` | sim |
| 40 | `InscProdutor` | `varchar(20)` | sim |
| 41 | `CNAE` | `varchar(15)` | sim |
| 42 | `DtaNascFund` | `datetime` | sim |
| 43 | `Origem` | `varchar(20)` | sim |
| 44 | `UltOrigem` | `varchar(20)` | sim |
| 45 | `Email` | `varchar(70)` | sim |
| 46 | `HomePage` | `varchar(80)` | sim |
| 47 | `EstadoCivil` | `varchar(20)` | sim |
| 48 | `Atividade` | `varchar(30)` | sim |
| 49 | `RendaFaturamento` | `varchar(30)` | sim |
| 50 | `GrauInstrucao` | `varchar(30)` | sim |
| 51 | `Grupo` | `varchar(30)` | sim |
| 52 | `Porte` | `varchar(30)` | sim |
| 53 | `DtaInclusao` | `datetime` | sim |
| 54 | `UsuInclusao` | `varchar(20)` | sim |
| 55 | `DtaAlteracao` | `datetime` | sim |
| 56 | `UsuAlteracao` | `varchar(20)` | sim |
| 57 | `DtaInativacao` | `datetime` | sim |
| 58 | `UsuInativacao` | `varchar(20)` | sim |
| 59 | `ObsInativacao` | `varchar(50)` | sim |
| 60 | `CodEquipe` | `varchar(20)` | sim |
| 61 | `Telefonema` | `numeric(1,0)` | sim |
| 62 | `Correspondencia` | `numeric(1,0)` | sim |
| 63 | `RecebeEmail` | `numeric(1,0)` | sim |
| 64 | `NaoPossuiEmail` | `numeric(1,0)` | sim |
| 65 | `ProblemaCredito` | `numeric(1,0)` | sim |
| 66 | `IndContribICMS` | `char(1)` | sim |
| 67 | `RefEndereco` | `varchar(150)` | sim |
| 68 | `Latitude` | `decimal(14,11)` | sim |
| 69 | `Longitude` | `decimal(14,11)` | sim |
| 70 | `SeqRegiao` | `decimal(6,0)` | sim |
| 71 | `SeqRota` | `decimal(6,0)` | sim |
| 72 | `RecebeSMS` | `numeric(1,0)` | sim |
| 73 | `SEQPESSOAPRC` | `numeric(10,0)` | **nao** |
| 74 | `Skype` | `varchar(70)` | sim |
| 75 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim |
| 76 | `TIPO_DE_EQUIPAMENTO` | `varchar(30)` | sim |
| 77 | `MARCA` | `varchar(30)` | sim |
| 78 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 79 | `ANO_MODELO_EQUIPAMEN` | `varchar(20)` | sim |
| 80 | `N__SERIE` | `varchar(250)` | sim |
| 81 | `HORIMETRO` | `varchar(250)` | sim |
| 82 | `DATA_INICIO_LOCACAO` | `datetime` | sim |
| 83 | `DATA_TERMINO_LOCACAO` | `datetime` | sim |
| 84 | `VALOR_MENSAL_DO_CONT` | `decimal(14,2)` | sim |
| 85 | `VALOR_TOTAL_DO_CONTR` | `decimal(14,2)` | sim |
| 86 | `DATA_RENOVACAO` | `datetime` | sim |
| 87 | `Expr1` | `numeric(10,0)` | **nao** |

### GE$PESSOAENDCORR

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(8,0)` | **nao** |
| 2 | `NOMERAZAO` | `varchar(60)` | **nao** |
| 3 | `ENDCORRESPOND` | `int(10,0)` | **nao** |
| 4 | `CORRESPONDENCIA` | `numeric(1,0)` | **nao** |
| 5 | `STATUS` | `varchar(1)` | **nao** |
| 6 | `SEXO` | `varchar(1)` | sim |
| 7 | `FISICAJURIDICA` | `varchar(1)` | sim |
| 8 | `EMAIL` | `varchar(70)` | sim |
| 9 | `TIPOENDERECO` | `varchar(1)` | **nao** |
| 10 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 11 | `LOGRADOURO` | `varchar(80)` | sim |
| 12 | `NROLOGRADOURO` | `varchar(10)` | sim |
| 13 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 14 | `ENDERECO` | `varchar(138)` | **nao** |
| 15 | `BAIRRO` | `varchar(50)` | sim |
| 16 | `CIDADE` | `varchar(50)` | sim |
| 17 | `UF` | `varchar(2)` | sim |
| 18 | `CEP` | `varchar(12)` | sim |

### GE$PESSOAEND_FULL

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQPESSOAEND` | `decimal(3,0)` | **nao** |
| 3 | `TIPOENDERECO` | `char(1)` | **nao** |
| 4 | `SEQCIDADE` | `decimal(6,0)` | sim |
| 5 | `CIDADE` | `varchar(50)` | sim |
| 6 | `UF` | `varchar(2)` | sim |
| 7 | `SEQBAIRRO` | `decimal(5,0)` | sim |
| 8 | `BAIRRO` | `varchar(50)` | sim |
| 9 | `LOGRADOURO` | `varchar(80)` | sim |
| 10 | `NROLOGRADOURO` | `varchar(10)` | sim |
| 11 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 12 | `CEP` | `varchar(12)` | sim |
| 13 | `DTAALTERACAO` | `datetime` | **nao** |
| 14 | `USUALTERACAO` | `varchar(20)` | **nao** |
| 15 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 16 | `SEQPESSOAENDCOBR` | `decimal(3,0)` | sim |
| 17 | `PAIS` | `varchar(25)` | sim |
| 18 | `CXPOSTAL` | `varchar(7)` | sim |
| 19 | `REFENDERECO` | `varchar(150)` | sim |
| 20 | `LATITUDE` | `decimal(14,11)` | sim |
| 21 | `LONGITUDE` | `decimal(14,11)` | sim |
| 22 | `DESCRICAO` | `varchar(100)` | sim |
| 23 | `SEQREGIAO` | `decimal(6,0)` | sim |
| 24 | `SEQROTA` | `decimal(6,0)` | sim |
| 25 | `CHAVEADICIONAL` | `varchar(30)` | sim |
| 26 | `INSCPRODUTOR` | `varchar(20)` | sim |
| 27 | `z_LOGRADOURO` | `varchar(80)` | sim |
| 28 | `z_NROLOGRADOURO` | `varchar(10)` | sim |

### GE$PESSOAEND_LGPD

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQPESSOAEND` | `decimal(3,0)` | **nao** |
| 3 | `TIPOENDERECO` | `char(1)` | **nao** |
| 4 | `SEQCIDADE` | `decimal(6,0)` | sim |
| 5 | `CIDADE` | `varchar(50)` | sim |
| 6 | `UF` | `varchar(2)` | sim |
| 7 | `SEQBAIRRO` | `decimal(5,0)` | sim |
| 8 | `BAIRRO` | `varchar(50)` | sim |
| 9 | `LOGRADOURO` | `varchar(250)` | sim |
| 10 | `NROLOGRADOURO` | `varchar(250)` | sim |
| 11 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 12 | `CEP` | `varchar(12)` | sim |
| 13 | `DTAALTERACAO` | `datetime` | **nao** |
| 14 | `USUALTERACAO` | `varchar(20)` | **nao** |
| 15 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 16 | `SEQPESSOAENDCOBR` | `decimal(3,0)` | sim |
| 17 | `PAIS` | `varchar(25)` | sim |
| 18 | `CXPOSTAL` | `varchar(7)` | sim |
| 19 | `REFENDERECO` | `varchar(150)` | sim |
| 20 | `LATITUDE` | `decimal(14,11)` | sim |
| 21 | `LONGITUDE` | `decimal(14,11)` | sim |
| 22 | `DESCRICAO` | `varchar(100)` | sim |
| 23 | `SEQREGIAO` | `decimal(6,0)` | sim |
| 24 | `SEQROTA` | `decimal(6,0)` | sim |
| 25 | `CHAVEADICIONAL` | `varchar(30)` | sim |
| 26 | `INSCPRODUTOR` | `varchar(20)` | sim |
| 27 | `z_LOGRADOURO` | `varchar(80)` | sim |
| 28 | `z_NROLOGRADOURO` | `varchar(10)` | sim |

### GE$PESSOAFONE_FULL

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESFONE` | `numeric(18,0)` | **nao** |
| 2 | `TIPOFONESEQPAR` | `numeric(18,0)` | **nao** |
| 3 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 4 | `DDD` | `varchar(5)` | sim |
| 5 | `NUMERO` | `numeric(12,0)` | **nao** |
| 6 | `COMPLEMENTO` | `varchar(20)` | sim |
| 7 | `OBS` | `varchar(30)` | sim |
| 8 | `INDFONEPREF` | `numeric(1,0)` | sim |
| 9 | `NROFONEPESSOA` | `numeric(1,0)` | sim |
| 10 | `DTAULTSUCESSO` | `datetime` | sim |
| 11 | `DTAULTINSUCESSO` | `datetime` | sim |
| 12 | `INDEMUSO` | `numeric(1,0)` | sim |
| 13 | `MOTIVOEMUSO` | `varchar(30)` | sim |
| 14 | `DTAALTERACAO` | `datetime` | sim |
| 15 | `USUALTERACAO` | `varchar(20)` | sim |
| 16 | `INDUSOMKT` | `numeric(1,0)` | sim |
| 17 | `DTAINCLUSAO` | `datetime` | sim |
| 18 | `USUINCLUSAO` | `varchar(20)` | sim |
| 19 | `INDWHATSAPP` | `numeric(1,0)` | sim |
| 20 | `z_NUMERO` | `numeric(12,0)` | **nao** |

### GE$PESSOAFONE_LGPD

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESFONE` | `numeric(18,0)` | **nao** |
| 2 | `TIPOFONESEQPAR` | `numeric(18,0)` | **nao** |
| 3 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 4 | `DDD` | `varchar(5)` | sim |
| 5 | `NUMERO` | `numeric(12,0)` | sim |
| 6 | `COMPLEMENTO` | `varchar(20)` | sim |
| 7 | `OBS` | `varchar(30)` | sim |
| 8 | `INDFONEPREF` | `numeric(1,0)` | sim |
| 9 | `NROFONEPESSOA` | `numeric(1,0)` | sim |
| 10 | `DTAULTSUCESSO` | `datetime` | sim |
| 11 | `DTAULTINSUCESSO` | `datetime` | sim |
| 12 | `INDEMUSO` | `numeric(1,0)` | sim |
| 13 | `MOTIVOEMUSO` | `varchar(30)` | sim |
| 14 | `DTAALTERACAO` | `datetime` | sim |
| 15 | `USUALTERACAO` | `varchar(20)` | sim |
| 16 | `INDUSOMKT` | `numeric(1,0)` | sim |
| 17 | `DTAINCLUSAO` | `datetime` | sim |
| 18 | `USUINCLUSAO` | `varchar(20)` | sim |
| 19 | `INDWHATSAPP` | `numeric(1,0)` | sim |
| 20 | `z_NUMERO` | `numeric(12,0)` | **nao** |

### GE$PESSOA_FULL

`view` · `94 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQPESSOAPRC` | `numeric(10,0)` | **nao** |
| 3 | `SEQCIDADE` | `decimal(6,0)` | sim |
| 4 | `SEQBAIRRO` | `decimal(5,0)` | sim |
| 5 | `VERSAO` | `decimal(2,0)` | sim |
| 6 | `STATUS` | `char(1)` | **nao** |
| 7 | `DTAATIVACAO` | `datetime` | sim |
| 8 | `NOMERAZAO` | `varchar(100)` | sim |
| 9 | `FANTASIA` | `varchar(50)` | sim |
| 10 | `PALAVRACHAVE` | `varchar(50)` | sim |
| 11 | `FISICAJURIDICA` | `char(1)` | sim |
| 12 | `SEXO` | `char(1)` | sim |
| 13 | `CIDADE` | `varchar(50)` | sim |
| 14 | `UF` | `varchar(2)` | sim |
| 15 | `PAIS` | `varchar(25)` | sim |
| 16 | `BAIRRO` | `varchar(50)` | sim |
| 17 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 18 | `LOGRADOURO` | `varchar(80)` | sim |
| 19 | `NROLOGRADOURO` | `varchar(10)` | sim |
| 20 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 21 | `CEP` | `varchar(12)` | sim |
| 22 | `CXPOSTAL` | `varchar(7)` | sim |
| 23 | `REFENDERECO` | `varchar(150)` | sim |
| 24 | `LATITUDE` | `decimal(14,11)` | sim |
| 25 | `LONGITUDE` | `decimal(14,11)` | sim |
| 26 | `SEQPESSOAENDCOBR` | `decimal(3,0)` | sim |
| 27 | `FONEDDD1` | `varchar(5)` | sim |
| 28 | `FONENRO1` | `decimal(12,0)` | sim |
| 29 | `FONECMPL1` | `varchar(20)` | sim |
| 30 | `FONEDDD2` | `varchar(5)` | sim |
| 31 | `FONENRO2` | `decimal(12,0)` | sim |
| 32 | `FONECMPL2` | `varchar(20)` | sim |
| 33 | `FONEDDD3` | `varchar(5)` | sim |
| 34 | `FONENRO3` | `decimal(12,0)` | sim |
| 35 | `FONECMPL3` | `varchar(20)` | sim |
| 36 | `FAXDDD` | `varchar(5)` | sim |
| 37 | `FAXNRO` | `decimal(12,0)` | sim |
| 38 | `NROCGCCPF` | `decimal(13,0)` | sim |
| 39 | `DIGCGCCPF` | `decimal(2,0)` | sim |
| 40 | `INSCRICAORG` | `varchar(20)` | sim |
| 41 | `UFEMISSOR` | `varchar(2)` | sim |
| 42 | `ORGAOEMISSOR` | `varchar(10)` | sim |
| 43 | `INSCMUNIC` | `varchar(15)` | sim |
| 44 | `INSCPRODUTOR` | `varchar(20)` | sim |
| 45 | `CNAE` | `varchar(15)` | sim |
| 46 | `DTANASCFUND` | `datetime` | sim |
| 47 | `ORIGEM` | `varchar(20)` | sim |
| 48 | `ULTORIGEM` | `varchar(20)` | sim |
| 49 | `EMAIL` | `varchar(70)` | sim |
| 50 | `SKYPE` | `varchar(70)` | sim |
| 51 | `HOMEPAGE` | `varchar(80)` | sim |
| 52 | `ESTADOCIVIL` | `varchar(20)` | sim |
| 53 | `ATIVIDADE` | `varchar(30)` | sim |
| 54 | `RENDAFATURAMENTO` | `varchar(30)` | sim |
| 55 | `GRAUINSTRUCAO` | `varchar(30)` | sim |
| 56 | `GRUPO` | `varchar(30)` | sim |
| 57 | `PORTE` | `varchar(30)` | sim |
| 58 | `DTAINCLUSAO` | `datetime` | sim |
| 59 | `USUINCLUSAO` | `varchar(20)` | sim |
| 60 | `DTAALTERACAO` | `datetime` | sim |
| 61 | `USUALTERACAO` | `varchar(20)` | sim |
| 62 | `DTAINATIVACAO` | `datetime` | sim |
| 63 | `USUINATIVACAO` | `varchar(20)` | sim |
| 64 | `OBSINATIVACAO` | `varchar(50)` | sim |
| 65 | `CODVENDEDOR` | `varchar(20)` | sim |
| 66 | `CODEQUIPE` | `varchar(20)` | sim |
| 67 | `TELEFONEMA` | `numeric(1,0)` | sim |
| 68 | `CORRESPONDENCIA` | `numeric(1,0)` | sim |
| 69 | `RECEBEEMAIL` | `numeric(1,0)` | sim |
| 70 | `RECEBESMS` | `numeric(1,0)` | sim |
| 71 | `NAOPOSSUIEMAIL` | `numeric(1,0)` | sim |
| 72 | `PROBLEMACREDITO` | `numeric(1,0)` | sim |
| 73 | `INDCONTRIBICMS` | `char(1)` | sim |
| 74 | `SEQREGIAO` | `decimal(6,0)` | sim |
| 75 | `SEQROTA` | `decimal(6,0)` | sim |
| 76 | `SMSCODIGO` | `varchar(8)` | sim |
| 77 | `SMSCODIGODTA` | `datetime` | sim |
| 78 | `z_NOMERAZAO` | `varchar(100)` | sim |
| 79 | `z_FANTASIA` | `varchar(50)` | sim |
| 80 | `z_PALAVRACHAVE` | `varchar(50)` | sim |
| 81 | `z_LOGRADOURO` | `varchar(80)` | sim |
| 82 | `z_NROLOGRADOURO` | `varchar(10)` | sim |
| 83 | `z_EMAIL` | `varchar(70)` | sim |
| 84 | `z_SKYPE` | `varchar(70)` | sim |
| 85 | `z_NROCGCCPF` | `decimal(13,0)` | sim |
| 86 | `z_FONEDDD1` | `varchar(5)` | sim |
| 87 | `z_FONENRO1` | `decimal(12,0)` | sim |
| 88 | `z_FONEDDD2` | `varchar(5)` | sim |
| 89 | `z_FONENRO2` | `decimal(12,0)` | sim |
| 90 | `z_FONEDDD3` | `varchar(5)` | sim |
| 91 | `z_FONENRO3` | `decimal(12,0)` | sim |
| 92 | `z_FAXDDD` | `varchar(5)` | sim |
| 93 | `z_FAXNRO` | `decimal(12,0)` | sim |
| 94 | `z_DTANASCFUND` | `datetime` | sim |

### GE$PESSOA_LGPD

`view` · `94 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQPESSOAPRC` | `numeric(10,0)` | **nao** |
| 3 | `SEQCIDADE` | `decimal(6,0)` | sim |
| 4 | `SEQBAIRRO` | `decimal(5,0)` | sim |
| 5 | `VERSAO` | `decimal(2,0)` | sim |
| 6 | `STATUS` | `char(1)` | **nao** |
| 7 | `DTAATIVACAO` | `datetime` | sim |
| 8 | `NOMERAZAO` | `varchar(250)` | sim |
| 9 | `FANTASIA` | `varchar(250)` | sim |
| 10 | `PALAVRACHAVE` | `varchar(250)` | sim |
| 11 | `FISICAJURIDICA` | `char(1)` | sim |
| 12 | `SEXO` | `char(1)` | sim |
| 13 | `CIDADE` | `varchar(50)` | sim |
| 14 | `UF` | `varchar(2)` | sim |
| 15 | `PAIS` | `varchar(25)` | sim |
| 16 | `BAIRRO` | `varchar(50)` | sim |
| 17 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 18 | `LOGRADOURO` | `varchar(250)` | sim |
| 19 | `NROLOGRADOURO` | `varchar(250)` | sim |
| 20 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 21 | `CEP` | `varchar(12)` | sim |
| 22 | `CXPOSTAL` | `varchar(7)` | sim |
| 23 | `REFENDERECO` | `varchar(150)` | sim |
| 24 | `LATITUDE` | `decimal(14,11)` | sim |
| 25 | `LONGITUDE` | `decimal(14,11)` | sim |
| 26 | `SEQPESSOAENDCOBR` | `decimal(3,0)` | sim |
| 27 | `FONEDDD1` | `varchar(5)` | sim |
| 28 | `FONENRO1` | `decimal(12,0)` | sim |
| 29 | `FONECMPL1` | `varchar(20)` | sim |
| 30 | `FONEDDD2` | `varchar(5)` | sim |
| 31 | `FONENRO2` | `decimal(12,0)` | sim |
| 32 | `FONECMPL2` | `varchar(20)` | sim |
| 33 | `FONEDDD3` | `varchar(5)` | sim |
| 34 | `FONENRO3` | `decimal(12,0)` | sim |
| 35 | `FONECMPL3` | `varchar(20)` | sim |
| 36 | `FAXDDD` | `varchar(5)` | sim |
| 37 | `FAXNRO` | `decimal(12,0)` | sim |
| 38 | `NROCGCCPF` | `decimal(13,0)` | sim |
| 39 | `DIGCGCCPF` | `decimal(2,0)` | sim |
| 40 | `INSCRICAORG` | `varchar(8000)` | sim |
| 41 | `UFEMISSOR` | `varchar(2)` | sim |
| 42 | `ORGAOEMISSOR` | `varchar(10)` | sim |
| 43 | `INSCMUNIC` | `varchar(15)` | sim |
| 44 | `INSCPRODUTOR` | `varchar(20)` | sim |
| 45 | `CNAE` | `varchar(15)` | sim |
| 46 | `DTANASCFUND` | `datetime` | sim |
| 47 | `ORIGEM` | `varchar(20)` | sim |
| 48 | `ULTORIGEM` | `varchar(20)` | sim |
| 49 | `EMAIL` | `varchar(250)` | sim |
| 50 | `SKYPE` | `varchar(250)` | sim |
| 51 | `HOMEPAGE` | `varchar(80)` | sim |
| 52 | `ESTADOCIVIL` | `varchar(20)` | sim |
| 53 | `ATIVIDADE` | `varchar(30)` | sim |
| 54 | `RENDAFATURAMENTO` | `varchar(30)` | sim |
| 55 | `GRAUINSTRUCAO` | `varchar(30)` | sim |
| 56 | `GRUPO` | `varchar(30)` | sim |
| 57 | `PORTE` | `varchar(30)` | sim |
| 58 | `DTAINCLUSAO` | `datetime` | sim |
| 59 | `USUINCLUSAO` | `varchar(20)` | sim |
| 60 | `DTAALTERACAO` | `datetime` | sim |
| 61 | `USUALTERACAO` | `varchar(20)` | sim |
| 62 | `DTAINATIVACAO` | `datetime` | sim |
| 63 | `USUINATIVACAO` | `varchar(20)` | sim |
| 64 | `OBSINATIVACAO` | `varchar(50)` | sim |
| 65 | `CODVENDEDOR` | `varchar(20)` | sim |
| 66 | `CODEQUIPE` | `varchar(20)` | sim |
| 67 | `TELEFONEMA` | `numeric(1,0)` | sim |
| 68 | `CORRESPONDENCIA` | `numeric(1,0)` | sim |
| 69 | `RECEBEEMAIL` | `numeric(1,0)` | sim |
| 70 | `RECEBESMS` | `numeric(1,0)` | sim |
| 71 | `NAOPOSSUIEMAIL` | `numeric(1,0)` | sim |
| 72 | `PROBLEMACREDITO` | `numeric(1,0)` | sim |
| 73 | `INDCONTRIBICMS` | `char(1)` | sim |
| 74 | `SEQREGIAO` | `decimal(6,0)` | sim |
| 75 | `SEQROTA` | `decimal(6,0)` | sim |
| 76 | `SMSCODIGO` | `varchar(8)` | sim |
| 77 | `SMSCODIGODTA` | `datetime` | sim |
| 78 | `z_NOMERAZAO` | `varchar(100)` | sim |
| 79 | `z_FANTASIA` | `varchar(50)` | sim |
| 80 | `z_PALAVRACHAVE` | `varchar(50)` | sim |
| 81 | `z_LOGRADOURO` | `varchar(80)` | sim |
| 82 | `z_NROLOGRADOURO` | `varchar(10)` | sim |
| 83 | `z_EMAIL` | `varchar(70)` | sim |
| 84 | `z_SKYPE` | `varchar(70)` | sim |
| 85 | `z_NROCGCCPF` | `decimal(13,0)` | sim |
| 86 | `z_FONEDDD1` | `varchar(5)` | sim |
| 87 | `z_FONENRO1` | `decimal(12,0)` | sim |
| 88 | `z_FONEDDD2` | `varchar(5)` | sim |
| 89 | `z_FONENRO2` | `decimal(12,0)` | sim |
| 90 | `z_FONEDDD3` | `varchar(5)` | sim |
| 91 | `z_FONENRO3` | `decimal(12,0)` | sim |
| 92 | `z_FAXDDD` | `varchar(5)` | sim |
| 93 | `z_FAXNRO` | `decimal(12,0)` | sim |
| 94 | `z_DTANASCFUND` | `datetime` | sim |

### GE$POLSEGPERM

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CODAPLICACAO` | `varchar(30)` | **nao** |
| 2 | `CHAVEAPLICACAO` | `numeric(18,0)` | **nao** |
| 3 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 4 | `SEQPOLSEG` | `decimal(6,0)` | **nao** |
| 5 | `SEQUSUARIO` | `numeric(18,0)` | **nao** |

### GE$USUARIOPERM

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CODAPLICACAO` | `varchar(20)` | **nao** |
| 2 | `CHAVEAPLICACAO` | `numeric(18,0)` | **nao** |
| 3 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 4 | `PERMISSAO` | `char(1)` | sim |
| 5 | `SEQUSUARIO` | `numeric(18,0)` | sim |

### GEL$CIDADE

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQCID` | `decimal(6,0)` | **nao** |
| 2 | `CEP` | `varchar(10)` | sim |
| 3 | `CIDADE` | `varchar(100)` | sim |
| 4 | `UF` | `varchar(2)` | **nao** |
| 5 | `SEQCIDADE` | `decimal(6,0)` | **nao** |

### GEL$LOGRADOURO

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CEP` | `varchar(10)` | **nao** |
| 2 | `TIPO` | `varchar(10)` | sim |
| 3 | `SEQCID` | `decimal(6,0)` | sim |
| 4 | `LOGRFONETICA` | `varchar(150)` | **nao** |
| 5 | `LOGRADOURO` | `varchar(100)` | sim |
| 6 | `CIDADE` | `varchar(100)` | sim |
| 7 | `CIDFONETICA` | `varchar(100)` | sim |
| 8 | `BAIRRO` | `varchar(100)` | sim |
| 9 | `UF` | `varchar(2)` | sim |
| 10 | `PAIS` | `varchar(25)` | sim |
| 11 | `CEPCIDADE` | `numeric(1,0)` | sim |
| 12 | `COMPLEMENTO` | `varchar(150)` | sim |

### GEL$TIPOLOGRADOURO

`view` · `3 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TIPOLOGRADOURO` | `varchar(15)` | **nao** |
| 2 | `DESCRICAO` | `varchar(20)` | sim |
| 3 | `SUBSTITUICOES` | `varchar(200)` | sim |

### GEL$UF

`view` · `1 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `UF` | `varchar(2)` | sim |

### IMPV_VEICULO

`view` · `46 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `idveiculo` | `numeric(18,0)` | **nao** |
| 2 | `Origem` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `numeric(10,0)` | sim |
| 4 | `PessoaLinkOrigem` | `varchar(20)` | **nao** |
| 5 | `Pessoalink` | `varchar(30)` | **nao** |
| 6 | `NroCNPJCPF` | `numeric(13,0)` | sim |
| 7 | `DigCNPJCPF` | `numeric(2,0)` | sim |
| 8 | `CNPJx` | `varchar(20)` | sim |
| 9 | `NroChassi` | `varchar(40)` | **nao** |
| 10 | `NroChassiRed` | `varchar(20)` | sim |
| 11 | `TipoVeiculo` | `varchar(5)` | sim |
| 12 | `Placa` | `varchar(9)` | sim |
| 13 | `Combustivel` | `varchar(30)` | sim |
| 14 | `Marca` | `varchar(20)` | sim |
| 15 | `CodFamilia` | `varchar(20)` | sim |
| 16 | `Familia` | `varchar(30)` | sim |
| 17 | `CodModelo` | `varchar(20)` | sim |
| 18 | `Modelo` | `varchar(30)` | sim |
| 19 | `CorExterna` | `varchar(30)` | sim |
| 20 | `CorInterna` | `varchar(30)` | sim |
| 21 | `Potencia` | `numeric(4,0)` | sim |
| 22 | `QtdeEixo` | `numeric(2,0)` | sim |
| 23 | `EstadoVenda` | `char(1)` | sim |
| 24 | `FormaPgto` | `char(1)` | sim |
| 25 | `Financiador` | `varchar(40)` | sim |
| 26 | `CanalVenda` | `varchar(20)` | sim |
| 27 | `Nronf` | `numeric(18,0)` | sim |
| 28 | `Serienf` | `varchar(12)` | sim |
| 29 | `Anofabricacao` | `numeric(4,0)` | sim |
| 30 | `AnoModelo` | `numeric(4,0)` | sim |
| 31 | `Dtavenda` | `datetime` | sim |
| 32 | `Dtaprevquitacao` | `datetime` | sim |
| 33 | `VlrVenda` | `numeric(14,2)` | sim |
| 34 | `Observacao` | `varchar(250)` | sim |
| 35 | `KMAtual` | `numeric(8,0)` | sim |
| 36 | `DtaKMAtual` | `datetime` | sim |
| 37 | `KMProxRevisao` | `numeric(8,0)` | sim |
| 38 | `DtaProxRevisao` | `datetime` | sim |
| 39 | `Revenda` | `varchar(40)` | sim |
| 40 | `Vendedor` | `varchar(40)` | sim |
| 41 | `TipoUso` | `varchar(40)` | sim |
| 42 | `UsuarioAlteracao` | `varchar(20)` | sim |
| 43 | `DtaAlteracao` | `datetime` | sim |
| 44 | `DtaGeracao` | `datetime` | **nao** |
| 45 | `StatusIMP` | `char(1)` | sim |
| 46 | `DtaImport` | `datetime` | sim |

### IV_EQUIPE

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQEQUIPE` | `numeric(18,0)` | **nao** |
| 2 | `CODEQUIPE` | `varchar(20)` | **nao** |
| 3 | `EQUIPE` | `varchar(40)` | sim |
| 4 | `CODEQUIPEFORA` | `numeric(18,0)` | sim |
| 5 | `USUALTEROU` | `varchar(20)` | sim |
| 6 | `CODEQUIPEFORAX` | `varchar(20)` | sim |
| 7 | `SEQUSUARIO` | `numeric(18,0)` | sim |
| 8 | `SEQUSUARIOLIDER` | `numeric(18,0)` | sim |

### IV_EQUIPEEMPR

`view` · `4 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQEQUIPE` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 3 | `DTAALTERACAO` | `datetime` | sim |
| 4 | `USUALTEROU` | `varchar(20)` | sim |

### IV_Q$ABERTURA_OS_REVISAO

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_ABERTURA` | `datetime` | sim |
| 19 | `NUMERO_OS` | `varchar(30)` | sim |

### IV_Q$ACOMPANHAMENTO_VENDA

`view` · `61 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MODELO_D_EQUIPAME` | `varchar(50)` | sim |
| 19 | `VALOR` | `decimal(14,2)` | sim |
| 20 | `TIPO_DE_FINANCIAME` | `varchar(20)` | sim |
| 21 | `AGENCIA` | `varchar(30)` | sim |
| 22 | `NOME_DO_GERENTE` | `varchar(30)` | sim |
| 23 | `TELEFONE_DA_AGENCI` | `varchar(30)` | sim |
| 24 | `PROPOSTA_ENTREGUE_` | `datetime` | sim |
| 25 | `PROPOSTA_PROTOCOLA` | `datetime` | sim |
| 26 | `PROCESSO_MONTADO` | `datetime` | sim |
| 27 | `PROCESSO_EM_ANALIS` | `datetime` | sim |
| 28 | `PROCESSO_APROVADO` | `datetime` | sim |
| 29 | `PROCESSO_NAO_APROV` | `datetime` | sim |
| 30 | `CONTRATO_EMITIDO` | `datetime` | sim |
| 31 | `CONTRATO_REGISTRAD` | `datetime` | sim |
| 32 | `PAC_EMITIDO` | `datetime` | sim |
| 33 | `FATURAMENTO_REALIZ` | `datetime` | sim |
| 34 | `N__PEDIDO_NA_FABRI` | `decimal(14,0)` | sim |
| 35 | `TIPO_DE_MAQUINA` | `varchar(20)` | sim |
| 36 | `PROB_APROVACAO` | `varchar(20)` | sim |
| 37 | `DTA_PREVFAT` | `datetime` | sim |
| 38 | `ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 39 | `MARCA_EQUIPAMENTO` | `varchar(20)` | sim |
| 40 | `VALOR_SINAL` | `decimal(14,2)` | sim |
| 41 | `VLR_FINANCIADO` | `decimal(14,2)` | sim |
| 42 | `NRO_PEDIDO` | `decimal(14,0)` | sim |
| 43 | `NRO_CHASSI` | `varchar(30)` | sim |
| 44 | `INST_FINANCEIRA` | `varchar(20)` | sim |
| 45 | `VLR_REC_PROPRIO` | `decimal(14,2)` | sim |
| 46 | `NRO_CONTRATO` | `decimal(14,0)` | sim |
| 47 | `NRO_PAC` | `decimal(14,0)` | sim |
| 48 | `EMAIL_CONTATO` | `varchar(50)` | sim |
| 49 | `DESC_IMPLEMENTO` | `varchar(50)` | sim |
| 50 | `NRO_NF` | `decimal(14,0)` | sim |
| 51 | `NRO_AGENCIA` | `decimal(14,0)` | sim |
| 52 | `TIPO_IMPLEMENTO` | `varchar(40)` | sim |
| 53 | `QTD_PESO_DIANTEIRO_` | `decimal(14,0)` | sim |
| 54 | `QTD_PESO_TRASEIRO_` | `decimal(14,0)` | sim |
| 55 | `LASTRO_LIQUIDO_DIANT` | `varchar(3)` | sim |
| 56 | `LASTRO_LIQUIDO_TRAS_` | `varchar(3)` | sim |
| 57 | `RODADO_DIANT__` | `varchar(20)` | sim |
| 58 | `RODADO_TRAS__` | `varchar(20)` | sim |
| 59 | `OUTRAS_CONFIGURACOES` | `varchar(250)` | sim |
| 60 | `INFORMACOES_PARA_O_F` | `varchar(250)` | sim |
| 61 | `TESTE` | `varchar(30)` | sim |

### IV_Q$ACOMPANHAM_VENDA_IMP

`view` · `55 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MODELO_IMPLEMENTO` | `varchar(250)` | sim |
| 19 | `VALOR_TOTAL_IMPLEM` | `decimal(14,2)` | sim |
| 20 | `TIPO_PAGTO_IMPL` | `varchar(40)` | sim |
| 21 | `AGENCIA_BANC` | `varchar(100)` | sim |
| 22 | `NOME_GER_BANCO` | `varchar(100)` | sim |
| 23 | `TEL_AGENCIA_BANCO` | `varchar(100)` | sim |
| 24 | `NUM_PED_FABRICA` | `decimal(14,0)` | sim |
| 25 | `TIP_IMPLEMENTO` | `varchar(100)` | sim |
| 26 | `PROBAB_APR_CREDITO` | `varchar(40)` | sim |
| 27 | `DATA_PREV_FATURAM` | `datetime` | sim |
| 28 | `ORIG_FATUR_IMPL` | `varchar(30)` | sim |
| 29 | `MARCA_IMPLEMENTO` | `varchar(40)` | sim |
| 30 | `VALOR_SINAL_IMPL` | `decimal(14,2)` | sim |
| 31 | `VALOR_FINAN_IMPL` | `decimal(14,2)` | sim |
| 32 | `NUM_PED_VENDA` | `decimal(14,0)` | sim |
| 33 | `NUM_CHASSI_IMPL` | `varchar(30)` | sim |
| 34 | `INST_FINANC` | `varchar(20)` | sim |
| 35 | `VALOR_REC_PROP` | `decimal(14,2)` | sim |
| 36 | `NRO_CONTRATO` | `varchar(30)` | sim |
| 37 | `NRO_DO_PAC` | `varchar(20)` | sim |
| 38 | `EMAIL_CONT` | `varchar(50)` | sim |
| 39 | `DESCR_DET_DIMENS` | `varchar(4000)` | sim |
| 40 | `NUMERO_NOTAFISCAL` | `decimal(14,0)` | sim |
| 41 | `NUMERO_AGENCIA` | `varchar(30)` | sim |
| 42 | `COD_FINAME_MDA_CONSO` | `varchar(10)` | sim |
| 43 | `DATA_DO_SINAL` | `datetime` | sim |
| 44 | `INFORMACOES_PARA_O_F` | `varchar(4000)` | sim |
| 45 | `INFO_AGREGA_DESAGREG` | `varchar(4000)` | sim |
| 46 | `DATA_COMBINADA_CLIEN` | `datetime` | sim |
| 47 | `DTA_PEDIDO` | `datetime` | sim |
| 48 | `BONIFICACAO_DESCONTO` | `varchar(3)` | sim |
| 49 | `VALOR_BONIFICACAO_DE` | `varchar(30)` | sim |
| 50 | `USADO_NA_NEGOCIACAO` | `varchar(3)` | sim |
| 51 | `COR` | `varchar(30)` | sim |
| 52 | `ANO_IMPLEMENTO` | `varchar(30)` | sim |
| 53 | `VENDA_IMPLEMENTO` | `varchar(20)` | sim |
| 54 | `SINAL_EMBUTIDO` | `varchar(3)` | sim |
| 55 | `LOCAL_DE_FATURAMENTO` | `varchar(20)` | sim |

### IV_Q$ACOMPANHA_COMPRA_IMP

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TP_EQUIP2` | `varchar(50)` | sim |
| 19 | `MARCAJD2` | `varchar(50)` | sim |
| 20 | `MDL_EQUIP2` | `varchar(200)` | sim |
| 21 | `QT2` | `decimal(14,0)` | sim |
| 22 | `FDD2` | `datetime` | sim |
| 23 | `QTD_PES_TRS2` | `decimal(14,0)` | sim |
| 24 | `KG_TRAS1_JD2` | `decimal(14,2)` | sim |
| 25 | `QTD_PES_TRS_OPC22` | `decimal(14,0)` | sim |
| 26 | `KG_TRS2_JD2` | `decimal(14,2)` | sim |
| 27 | `QTD_PES_DIAN2` | `decimal(14,0)` | sim |
| 28 | `KG_DIANT_JD2` | `decimal(14,2)` | sim |
| 29 | `RDD_TRS_JD2` | `varchar(20)` | sim |
| 30 | `RDD_DIAN_JD2` | `varchar(20)` | sim |
| 31 | `QTD_VCR_100_JD2` | `decimal(14,0)` | sim |
| 32 | `QTDE_VCR_300_JD2` | `decimal(14,0)` | sim |

### IV_Q$ACOMPANH_VENDA_JDE

`view` · `110 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MEU_1_JD` | `varchar(3)` | sim |
| 19 | `TIPO_EQUIP1` | `varchar(20)` | sim |
| 20 | `MARCA_1` | `varchar(20)` | sim |
| 21 | `MODEL_EQUIP1` | `varchar(50)` | sim |
| 22 | `NRO_PED_VEND1` | `decimal(14,0)` | sim |
| 23 | `QTDE_PESO_TRAS1` | `decimal(14,2)` | sim |
| 24 | `KG` | `decimal(14,2)` | sim |
| 25 | `POSICAO_TRAS1` | `varchar(20)` | sim |
| 26 | `QDTE_PESO_TRAS2` | `decimal(14,2)` | sim |
| 27 | `KG_TRAS_2` | `decimal(14,2)` | sim |
| 28 | `POSICAO_PESO_TRAS2` | `varchar(20)` | sim |
| 29 | `QTDE_PESO_DIAN1` | `decimal(14,2)` | sim |
| 30 | `KG_PESO_DIAN1` | `decimal(14,2)` | sim |
| 31 | `LASTR_LIQ_TRAS1` | `varchar(3)` | sim |
| 32 | `LASTR_DIANT_LIQ1` | `varchar(3)` | sim |
| 33 | `RODADO_TRAS1` | `varchar(20)` | sim |
| 34 | `RODADO_DIANT1` | `varchar(20)` | sim |
| 35 | `QTD_VRC_1001` | `decimal(14,0)` | sim |
| 36 | `QTD_VCR_3001` | `decimal(14,0)` | sim |
| 37 | `AMS1` | `varchar(3)` | sim |
| 38 | `AMS_DE_FABRICA1` | `varchar(3)` | sim |
| 39 | `DESCRICAO_AMS1` | `varchar(4000)` | sim |
| 40 | `OUTRAS_CONFIG1` | `varchar(4000)` | sim |
| 41 | `VLR_TOTAL1` | `decimal(14,2)` | sim |
| 42 | `VLR_SINAL1` | `decimal(14,2)` | sim |
| 43 | `DTA_SINAL1` | `datetime` | sim |
| 44 | `VLR_FINAN1` | `decimal(14,2)` | sim |
| 45 | `TP_PGTO1` | `varchar(20)` | sim |
| 46 | `INSTITUI_FINANCEIRA1` | `varchar(20)` | sim |
| 47 | `NRO_AGENC1` | `decimal(14,0)` | sim |
| 48 | `NOME_AGENC1` | `varchar(30)` | sim |
| 49 | `NOME_GERENT1` | `varchar(30)` | sim |
| 50 | `TEL_AGENCIA1` | `varchar(30)` | sim |
| 51 | `EMAIL_AGENCIA1` | `varchar(50)` | sim |
| 52 | `INF_PARA_FINANC1` | `varchar(1000)` | sim |
| 53 | `NRO_CHASSI1` | `varchar(30)` | sim |
| 54 | `PROB_APROV_CRED1` | `varchar(10)` | sim |
| 55 | `NRO_PAC1` | `varchar(20)` | sim |
| 56 | `NRO_CONTR1` | `varchar(20)` | sim |
| 57 | `PREV_FATURAM1` | `datetime` | sim |
| 58 | `NRO_NF1` | `decimal(14,0)` | sim |
| 59 | `COD_FIN_MDA_CONS1` | `varchar(10)` | sim |
| 60 | `INFORMACOES_AGREGA__` | `varchar(4000)` | sim |
| 61 | `DATA_COMBINADA_CLIEN` | `datetime` | sim |
| 62 | `EXPECTATIVA_DATA_CLI` | `datetime` | sim |
| 63 | `DATA_ATUALIZADA` | `datetime` | sim |
| 64 | `ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 65 | `FATURAMENTO_REALIZ` | `datetime` | sim |
| 66 | `DATA_PEDIDO_DE_VENDA` | `datetime` | sim |
| 67 | `PAC_EMITIDO` | `datetime` | sim |
| 68 | `BITOLA` | `varchar(150)` | sim |
| 69 | `CODIGO_MDA` | `varchar(60)` | sim |
| 70 | `TAXA_FLAT` | `varchar(3)` | sim |
| 71 | `TAXA_FLAT__` | `varchar(30)` | sim |
| 72 | `NF_DE_REFATURAMENTO` | `decimal(14,0)` | sim |
| 73 | `DATA_INICIO_BANCO` | `datetime` | sim |
| 74 | `CALCULO_DA_BITOLA` | `varchar(20)` | sim |
| 75 | `Q058_ROTACAO` | `decimal(1,0)` | sim |
| 76 | `Q058_AUTOTRAV_SF3` | `decimal(1,0)` | sim |
| 77 | `Q058_CHICOTE_PF906` | `decimal(1,0)` | sim |
| 78 | `Q058_CHICOTE_A` | `decimal(1,0)` | sim |
| 79 | `Q058_MONITOR_GEN4_42` | `decimal(1,0)` | sim |
| 80 | `Q058_MONITOR_GEN4_46` | `decimal(1,0)` | sim |
| 81 | `Q058_MONITOR_GS3_263` | `decimal(1,0)` | sim |
| 82 | `Q058_RADIO_900MHZ` | `decimal(1,0)` | sim |
| 83 | `Q058_READY` | `decimal(1,0)` | sim |
| 84 | `Q058_SUPORTEPF903` | `decimal(1,0)` | sim |
| 85 | `Q058_SF_600` | `decimal(1,0)` | sim |
| 86 | `Q058_RECEPTOR_SF6000` | `decimal(1,0)` | sim |
| 87 | `Q058_VOLANTE_200` | `decimal(1,0)` | sim |
| 88 | `Q058_ATU_300` | `decimal(1,0)` | sim |
| 89 | `Q058_ATIVACAO_RTK_RE` | `decimal(1,0)` | sim |
| 90 | `Q058_ATIVACAO_SF3_RE` | `decimal(1,0)` | sim |
| 91 | `Q058_RADIO_450_MHZ` | `decimal(1,0)` | sim |
| 92 | `Q058_RADIO_900_MHZ` | `decimal(1,0)` | sim |
| 93 | `Q058_RADIO_450MHZ` | `decimal(1,0)` | sim |
| 94 | `Q058_KIT_AL207195` | `decimal(1,0)` | sim |
| 95 | `VALIDACAO_GESTOR` | `datetime` | sim |
| 96 | `VALOR_DO_USADO` | `decimal(14,2)` | sim |
| 97 | `VALOR_DA_AVALIACAO_U` | `decimal(14,2)` | sim |
| 98 | `HAVERA_USADO_NA_NEGO` | `varchar(3)` | sim |
| 99 | `CONFIRMACAO_DE_DADOS` | `datetime` | sim |
| 100 | `BONIFICACAO_DESCONTO` | `varchar(3)` | sim |
| 101 | `VALOR_BONIFICACAO_DE` | `decimal(14,2)` | sim |
| 102 | `TIPO_DO_FRETE` | `varchar(20)` | sim |
| 103 | `VALOR_DO_FRETE` | `decimal(14,2)` | sim |
| 104 | `NOME_TRANSPORTADORA` | `varchar(45)` | sim |
| 105 | `SINAL_EMBUTIDO` | `varchar(3)` | sim |
| 106 | `MONITOR` | `varchar(20)` | sim |
| 107 | `RECEPTOR` | `varchar(20)` | sim |
| 108 | `RADIO` | `varchar(20)` | sim |
| 109 | `VOLANTE` | `varchar(20)` | sim |
| 110 | `ATIVACAO` | `varchar(20)` | sim |

### IV_Q$ACOMPAN_COMPRA_JDE

`view` · `33 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TP_EQUIP2` | `varchar(20)` | sim |
| 19 | `MARCAJD2` | `varchar(20)` | sim |
| 20 | `MDL_EQUIP2` | `varchar(20)` | sim |
| 21 | `QT2` | `decimal(14,0)` | sim |
| 22 | `FDD2` | `datetime` | sim |
| 23 | `QTD_PES_TRS2` | `decimal(14,0)` | sim |
| 24 | `KG_TRAS1_JD2` | `decimal(14,2)` | sim |
| 25 | `QTD_PES_TRS_OPC22` | `decimal(14,0)` | sim |
| 26 | `KG_TRS2_JD2` | `decimal(14,2)` | sim |
| 27 | `QTD_PES_DIAN2` | `decimal(14,0)` | sim |
| 28 | `KG_DIANT_JD2` | `decimal(14,2)` | sim |
| 29 | `RDD_TRS_JD2` | `varchar(20)` | sim |
| 30 | `RDD_DIAN_JD2` | `varchar(20)` | sim |
| 31 | `QTD_VCR_100_JD2` | `decimal(14,0)` | sim |
| 32 | `QTDE_VCR_300_JD2` | `decimal(14,0)` | sim |
| 33 | `COMPLEMENTO_OBS` | `varchar(500)` | sim |

### IV_Q$ACOMPAN_VEND_CONCESS

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `FORMA_PAGAMENTO` | `varchar(60)` | sim |
| 19 | `CHASSI` | `varchar(30)` | sim |
| 20 | `AMS` | `varchar(3)` | sim |
| 21 | `RODADO` | `varchar(40)` | sim |
| 22 | `PESO` | `varchar(30)` | sim |
| 23 | `CONFIGURACOES` | `varchar(100)` | sim |
| 24 | `NOTA_FISCAL` | `decimal(14,0)` | sim |
| 25 | `MODELO` | `varchar(20)` | sim |

### IV_Q$ACOMP_VENDA_DIRETA

`view` · `36 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `CLIENTE_SAM` | `varchar(3)` | sim |
| 19 | `Q002_COLHEDORAS` | `decimal(1,0)` | sim |
| 20 | `Q002_COLHEITADEIRAS` | `decimal(1,0)` | sim |
| 21 | `Q002_PULVERIZADORES` | `decimal(1,0)` | sim |
| 22 | `Q002_TRATORES` | `decimal(1,0)` | sim |
| 23 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 24 | `N__PEDIDO` | `decimal(14,0)` | sim |
| 25 | `VALOR_DA_VENDA` | `decimal(14,2)` | sim |
| 26 | `FRETE` | `decimal(14,2)` | sim |
| 27 | `ICMS` | `decimal(14,2)` | sim |
| 28 | `PIS___COFINS` | `decimal(14,2)` | sim |
| 29 | `BASE_DE_CALCULO` | `decimal(14,2)` | sim |
| 30 | `__COMISSAO` | `varchar(30)` | sim |
| 31 | `COMISSAO_PROVISIONAD` | `decimal(14,2)` | sim |
| 32 | `VALOR_FINAL_COMISSAO` | `decimal(14,2)` | sim |
| 33 | `CHASSI` | `varchar(30)` | sim |
| 34 | `N__NF_COMISSAO_VEND_` | `decimal(14,0)` | sim |
| 35 | `DATA_MARCADO_ENTREGU` | `datetime` | sim |
| 36 | `TIPO_CLIENTE` | `varchar(20)` | sim |

### IV_Q$ACOMP_VENDA_DIRETAJD

`view` · `110 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TIPO_DE_CLIENTE` | `varchar(20)` | sim |
| 19 | `C1__JOHN_DEERE_` | `varchar(3)` | sim |
| 20 | `TIPO_EQUIP` | `varchar(20)` | sim |
| 21 | `MODEL_EQUIP1` | `varchar(51)` | sim |
| 22 | `NUMERO_PEDIDO` | `decimal(14,0)` | sim |
| 23 | `__COMISSAO_FABRICA` | `varchar(4)` | sim |
| 24 | `COMISSAO_PROVISIONAD` | `decimal(14,2)` | sim |
| 25 | `VALOR_COMISSAO_RECEB` | `decimal(14,2)` | sim |
| 26 | `NF_COMISSAO_COLORADO` | `decimal(14,0)` | sim |
| 27 | `CHASSI` | `varchar(20)` | sim |
| 28 | `NF_FABRICA_CLIENTE` | `decimal(14,0)` | sim |
| 29 | `VALOR_VENDA_CLIENTE` | `decimal(14,2)` | sim |
| 30 | `DATA_ENTREGA` | `datetime` | sim |
| 31 | `ITENS_BONIFICADOS` | `varchar(150)` | sim |
| 32 | `VENDA_COM_AMS` | `varchar(3)` | sim |
| 33 | `AMS_DE_FABRICA` | `varchar(3)` | sim |
| 34 | `DESCRICAO_DO_AMS` | `varchar(51)` | sim |
| 35 | `QUANTIDADE_PESO_TRAS` | `varchar(4)` | sim |
| 36 | `PESO_TRASEIRO___KG` | `varchar(8)` | sim |
| 37 | `PESO_TRASEIRO___POSI` | `varchar(20)` | sim |
| 38 | `QUANT_PESO_TRAS2` | `decimal(14,0)` | sim |
| 39 | `PESO_TRASEIRO__2____` | `varchar(8)` | sim |
| 40 | `PESO_TRASEIRO__2_POS` | `varchar(20)` | sim |
| 41 | `QUANTIDADE_PESO_DIAN` | `decimal(14,0)` | sim |
| 42 | `RODADO_TRASEIRO` | `varchar(30)` | sim |
| 43 | `RODADO_DIANTEIRO` | `varchar(8)` | sim |
| 44 | `QUANTIDADE_VCR_100` | `decimal(14,0)` | sim |
| 45 | `QUANTIDADE_VCR_300` | `decimal(14,0)` | sim |
| 46 | `CHASSI_` | `varchar(20)` | sim |
| 47 | `_CHASSI` | `varchar(20)` | sim |
| 48 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim |
| 49 | `_CHASSI_` | `varchar(20)` | sim |
| 50 | `_CHASSI__` | `varchar(20)` | sim |
| 51 | `CHASS` | `varchar(20)` | sim |
| 52 | `__CHASSI_` | `varchar(20)` | sim |
| 53 | `CHA_SSI` | `varchar(20)` | sim |
| 54 | `CHASSI2` | `varchar(20)` | sim |
| 55 | `CHASSI3` | `varchar(20)` | sim |
| 56 | `CHASSI11` | `varchar(20)` | sim |
| 57 | `CHASSI12` | `varchar(20)` | sim |
| 58 | `CHASSI13` | `varchar(20)` | sim |
| 59 | `CHASSI14` | `varchar(20)` | sim |
| 60 | `CHASSI15` | `varchar(20)` | sim |
| 61 | `DATA_PEDIDO` | `datetime` | sim |
| 62 | `COTACAO` | `decimal(14,0)` | sim |
| 63 | `NOTA_FISCAL` | `decimal(14,0)` | sim |
| 64 | `ITENS_BONIF_PECAS` | `varchar(3)` | sim |
| 65 | `VLR_BONIF_PECAS` | `decimal(14,2)` | sim |
| 66 | `DESCR_PECAS_BONIF` | `varchar(400)` | sim |
| 67 | `BONIFICACAO_SERVICOS` | `varchar(3)` | sim |
| 68 | `VLR_BONIF_SERVICOS` | `decimal(14,2)` | sim |
| 69 | `DESCR_SERV_BONIFICAD` | `varchar(60)` | sim |
| 70 | `BONIFICACAO_AMS` | `varchar(3)` | sim |
| 71 | `VLR_BONIFICACAO_AMS` | `decimal(14,2)` | sim |
| 72 | `DESCRICAO_AMS_BONIF` | `varchar(60)` | sim |
| 73 | `NOTA_FISCAL_1` | `decimal(14,0)` | sim |
| 74 | `NOTA_FISCAL_2` | `decimal(14,0)` | sim |
| 75 | `NOTA_FISCAL_3` | `decimal(14,0)` | sim |
| 76 | `NOTA_FISCAL_4` | `decimal(14,0)` | sim |
| 77 | `NOTA_FISCAL_5` | `decimal(14,0)` | sim |
| 78 | `NOTA_FISCAL_6` | `decimal(14,0)` | sim |
| 79 | `NOTA_FISCAL_7` | `decimal(14,0)` | sim |
| 80 | `NOTA_FISCAL_8` | `decimal(14,0)` | sim |
| 81 | `NOTA_FISCAL_9` | `decimal(14,0)` | sim |
| 82 | `NOTA_FISCAL_10` | `decimal(14,0)` | sim |
| 83 | `NF_DESC_INCONDI_SERV` | `decimal(14,0)` | sim |
| 84 | `NF_DESC_INCONDI_PECA` | `decimal(14,0)` | sim |
| 85 | `NF_DESC_INCOND_AMS` | `decimal(14,0)` | sim |
| 86 | `DATA_ENTREGA1` | `datetime` | sim |
| 87 | `DATA_ENTREGA2` | `datetime` | sim |
| 88 | `DATA_ENTREGA3` | `datetime` | sim |
| 89 | `DATA_ENTREGA4` | `datetime` | sim |
| 90 | `DATA_ENTREGA5` | `datetime` | sim |
| 91 | `DATA_ENTREGA6` | `datetime` | sim |
| 92 | `DATA_ENTREGA7` | `datetime` | sim |
| 93 | `DATA_ENTREGA8` | `datetime` | sim |
| 94 | `DATA_ENTREGA9` | `datetime` | sim |
| 95 | `VALIDACAO_GESTOR` | `datetime` | sim |
| 96 | `VLR_CHASSI_1` | `decimal(14,2)` | sim |
| 97 | `VLR_CHASSI_2` | `decimal(14,2)` | sim |
| 98 | `VLR_CHASSI_3` | `decimal(14,2)` | sim |
| 99 | `VLR_CHASSI_4` | `decimal(14,2)` | sim |
| 100 | `VLR_CHASSI5` | `decimal(14,2)` | sim |
| 101 | `VLR_CHASSI6` | `decimal(14,2)` | sim |
| 102 | `NUMERO_COTACAO` | `varchar(30)` | sim |
| 103 | `NUMERTO_COTA_2` | `varchar(30)` | sim |
| 104 | `NUMERO_COTA_3` | `varchar(30)` | sim |
| 105 | `NUMERO_COTA_4` | `varchar(30)` | sim |
| 106 | `NUMERO_COTA_5` | `varchar(30)` | sim |
| 107 | `NUMERO_FORMULARIO` | `varchar(20)` | sim |
| 108 | `QUANTIDADE_DE_EQUIP` | `varchar(20)` | sim |
| 109 | `VALOVALOR_BONIFICACA` | `decimal(14,2)` | sim |
| 110 | `TIPO_BTIPO_ONIFICACA` | `varchar(25)` | sim |

### IV_Q$ACOMP_VENDA_FINANC

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VENDA_FINANC_CHASSI` | `varchar(50)` | sim |
| 19 | `VENDA_FINANC_NRO_NF` | `varchar(30)` | sim |
| 20 | `VENDA_FINANC_DATAFAT` | `datetime` | sim |
| 21 | `VENDA_FINANC_DATAENT` | `datetime` | sim |
| 22 | `VENDA_FINANC_FILIAL` | `varchar(30)` | sim |
| 23 | `NUMERO_PROPOSTA` | `decimal(20,0)` | sim |
| 24 | `DATA_PROPOSTA` | `datetime` | sim |
| 25 | `COMAR` | `decimal(8,0)` | sim |

### IV_Q$ACOMP_VENDA_LOCACAO

`view` · `30 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_DE_EQUIPAMENTO` | `varchar(30)` | sim |
| 19 | `MARCA` | `varchar(30)` | sim |
| 20 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 21 | `TIPO_DE_TORRE` | `varchar(60)` | sim |
| 22 | `TIPO_DE_RODADO` | `varchar(30)` | sim |
| 23 | `BITOLA` | `varchar(20)` | sim |
| 24 | `ACESSORIOS` | `varchar(150)` | sim |
| 25 | `IMPLEMENTOS` | `varchar(100)` | sim |
| 26 | `EASY_MANAGER___COMPU` | `varchar(100)` | sim |
| 27 | `VALOR_TOTAL_DA_NEGOC` | `decimal(14,2)` | sim |
| 28 | `DATA_INICIO_DA_LOCAC` | `datetime` | sim |
| 29 | `DATA_TERMINO_LOCACAO` | `datetime` | sim |
| 30 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim |

### IV_Q$ACOMP_VENDA_MANITOU

`view` · `61 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_EQUIP_MANITOU` | `varchar(30)` | sim |
| 19 | `MARCA_M` | `varchar(30)` | sim |
| 20 | `MODELO_EQUIPM` | `varchar(30)` | sim |
| 21 | `N_PEDIDO_VDM` | `decimal(14,0)` | sim |
| 22 | `TIPO_TORRE` | `varchar(60)` | sim |
| 23 | `TIPO_RODADO` | `varchar(60)` | sim |
| 24 | `BITOLA` | `varchar(60)` | sim |
| 25 | `ACESSORIOS` | `varchar(150)` | sim |
| 26 | `IMPLEMENTOS` | `varchar(100)` | sim |
| 27 | `EASY_MANAGER` | `varchar(100)` | sim |
| 28 | `OUTRAS_CONFIG` | `varchar(150)` | sim |
| 29 | `AGREGA_DESAGREG` | `varchar(100)` | sim |
| 30 | `VALOR_TOTA` | `decimal(14,2)` | sim |
| 31 | `VALOR_SINAL` | `decimal(14,2)` | sim |
| 32 | `DATA_SINAL` | `datetime` | sim |
| 33 | `VALOR_FINANC` | `decimal(14,2)` | sim |
| 34 | `TIPO_PGTO` | `varchar(60)` | sim |
| 35 | `DATA_PGTO` | `datetime` | sim |
| 36 | `DATA_ATUALIZADA` | `datetime` | sim |
| 37 | `COD_FINAME` | `decimal(14,0)` | sim |
| 38 | `TX_FLAT` | `varchar(3)` | sim |
| 39 | `TAXA_FLT_` | `varchar(3)` | sim |
| 40 | `INST_FINANC` | `varchar(20)` | sim |
| 41 | `N_AGENCIA` | `varchar(30)` | sim |
| 42 | `NOME_AGENC` | `varchar(50)` | sim |
| 43 | `NOME_GER_AGENC` | `varchar(50)` | sim |
| 44 | `TEL_AGENC` | `decimal(14,0)` | sim |
| 45 | `CONTATO_AGENCIA` | `varchar(30)` | sim |
| 46 | `INF_FINANC` | `varchar(150)` | sim |
| 47 | `CHASSI_EQUIP` | `varchar(30)` | sim |
| 48 | `APROV_CRED` | `varchar(20)` | sim |
| 49 | `N__DO_PAC` | `varchar(30)` | sim |
| 50 | `PAC_EMITIDO` | `varchar(30)` | sim |
| 51 | `N_CONTRATO` | `decimal(14,0)` | sim |
| 52 | `PREVISAO_DE_FATURAME` | `datetime` | sim |
| 53 | `DATA_PED_VD` | `datetime` | sim |
| 54 | `DATA_PROC_BC` | `datetime` | sim |
| 55 | `N__NF` | `decimal(14,0)` | sim |
| 56 | `DATA_FAT` | `datetime` | sim |
| 57 | `NF_REFAT` | `decimal(14,0)` | sim |
| 58 | `OBS` | `varchar(100)` | sim |
| 59 | `TIPO_EQUIP` | `varchar(20)` | sim |
| 60 | `MARCA` | `varchar(20)` | sim |
| 61 | `MODEL_EQUIP1` | `varchar(20)` | sim |

### IV_Q$ACOMP_VENDA_USADO

`view` · `60 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_EQUIP_MANITOU` | `varchar(30)` | sim |
| 19 | `MARCA_M` | `varchar(30)` | sim |
| 20 | `MODELO_EQUIPM` | `varchar(30)` | sim |
| 21 | `N_PEDIDO_VDM` | `decimal(14,0)` | sim |
| 22 | `TIPO_TORRE` | `varchar(60)` | sim |
| 23 | `BITOLA` | `varchar(60)` | sim |
| 24 | `ACESSORIOS` | `varchar(150)` | sim |
| 25 | `IMPLEMENTOS` | `varchar(100)` | sim |
| 26 | `EASY_MANAGER` | `varchar(100)` | sim |
| 27 | `OUTRAS_CONFIG` | `varchar(150)` | sim |
| 28 | `AGREGA_DESAGREG` | `varchar(100)` | sim |
| 29 | `VALOR_TOTA` | `decimal(14,2)` | sim |
| 30 | `VALOR_SINAL` | `decimal(14,2)` | sim |
| 31 | `DATA_SINAL` | `datetime` | sim |
| 32 | `VALOR_FINANC` | `decimal(14,2)` | sim |
| 33 | `TIPO_PGTO` | `varchar(60)` | sim |
| 34 | `DATA_PGTO` | `datetime` | sim |
| 35 | `DATA_ATUALIZADA` | `datetime` | sim |
| 36 | `COD_FINAME` | `decimal(14,0)` | sim |
| 37 | `TX_FLAT` | `varchar(3)` | sim |
| 38 | `TAXA_FLT_` | `varchar(3)` | sim |
| 39 | `INST_FINANC` | `varchar(20)` | sim |
| 40 | `N_AGENCIA` | `varchar(30)` | sim |
| 41 | `NOME_AGENC` | `varchar(50)` | sim |
| 42 | `NOME_GER_AGENC` | `varchar(50)` | sim |
| 43 | `TEL_AGENC` | `decimal(14,0)` | sim |
| 44 | `CONTATO_AGENCIA` | `varchar(30)` | sim |
| 45 | `INF_FINANC` | `varchar(150)` | sim |
| 46 | `CHASSI_EQUIP` | `varchar(30)` | sim |
| 47 | `APROV_CRED` | `varchar(20)` | sim |
| 48 | `N__DO_PAC` | `varchar(30)` | sim |
| 49 | `PAC_EMITIDO` | `varchar(30)` | sim |
| 50 | `N_CONTRATO` | `decimal(14,0)` | sim |
| 51 | `PREVISAO_DE_FATURAME` | `datetime` | sim |
| 52 | `DATA_PED_VD` | `datetime` | sim |
| 53 | `DATA_PROC_BC` | `datetime` | sim |
| 54 | `N__NF` | `decimal(14,0)` | sim |
| 55 | `DATA_FAT` | `datetime` | sim |
| 56 | `NF_REFAT` | `decimal(14,0)` | sim |
| 57 | `QTDE_VCR` | `varchar(80)` | sim |
| 58 | `TIPO_DE_EQUIPAMENTO` | `varchar(20)` | sim |
| 59 | `MARCA` | `varchar(20)` | sim |
| 60 | `MODELO_DO_EQUIPAMENT` | `varchar(50)` | sim |

### IV_Q$ACOMP_VEND_MAQUINAS

`view` · `66 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `FORMULARIO_NUMERO` | `varchar(20)` | sim |
| 19 | `CREEPER_CR` | `varchar(20)` | sim |
| 20 | `TROCA_DE_EIXO_3_METR` | `varchar(3)` | sim |
| 21 | `TROCAPNEUS` | `varchar(20)` | sim |
| 22 | `PESOS_CONFIGURACAO_F` | `varchar(3)` | sim |
| 23 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 24 | `INSTALACAO_CABINE` | `varchar(20)` | sim |
| 25 | `TROCA_DE_ASSENTO` | `varchar(20)` | sim |
| 26 | `INSTALACAO_VCRINSTAL` | `varchar(3)` | sim |
| 27 | `COTACAO` | `varchar(30)` | sim |
| 28 | `MARCA` | `varchar(20)` | sim |
| 29 | `FDD` | `datetime` | sim |
| 30 | `MODELO_EQUIPAMENTO` | `varchar(20)` | sim |
| 31 | `QTDE_PESO_TRASEIRO__` | `varchar(20)` | sim |
| 32 | `KG_PESO_TRASEIRO__1_` | `varchar(20)` | sim |
| 33 | `POSICAO_PESO_TRASEIR` | `varchar(20)` | sim |
| 34 | `QTDE_PESO_TRAS2` | `varchar(20)` | sim |
| 35 | `KG_PESO_TRASEIRO__2_` | `varchar(20)` | sim |
| 36 | `POSICAO_PESO_TRASEI2` | `varchar(20)` | sim |
| 37 | `QTDE_PESO_DIANTEIRO` | `varchar(20)` | sim |
| 38 | `KG_PESO_DIANTEIRO` | `varchar(20)` | sim |
| 39 | `LASTRO_DIANTEIRO__` | `varchar(20)` | sim |
| 40 | `LASTRO_TRASEIRO` | `varchar(20)` | sim |
| 41 | `RODADO_TRASEIRO` | `varchar(30)` | sim |
| 42 | `RODADO_DIANTEIRO` | `varchar(30)` | sim |
| 43 | `QTDE_VCR_100` | `varchar(20)` | sim |
| 44 | `QTDE_VCR_300` | `varchar(20)` | sim |
| 45 | `MEDIDA_BITOLA` | `varchar(30)` | sim |
| 46 | `POSICAO_BITOLA` | `varchar(20)` | sim |
| 47 | `AMS` | `varchar(3)` | sim |
| 48 | `RECEPTOR` | `varchar(20)` | sim |
| 49 | `RADIO` | `varchar(20)` | sim |
| 50 | `ATIVACAO` | `varchar(20)` | sim |
| 51 | `MONITOR` | `varchar(20)` | sim |
| 52 | `VOLANTE` | `varchar(20)` | sim |
| 53 | `CONECTIVIDADCONECTIV` | `varchar(20)` | sim |
| 54 | `CONFIGURACOES_AMS` | `varchar(2000)` | sim |
| 55 | `CHASSI` | `varchar(20)` | sim |
| 56 | `INFORMACAO_PREPARACA` | `varchar(4000)` | sim |
| 57 | `PREVISAO_ENTREGA_PRE` | `datetime` | sim |
| 58 | `DATA_ENTREGA` | `datetime` | sim |
| 59 | `TRANSPORTE_ENTREGA` | `varchar(20)` | sim |
| 60 | `TRANSPORTADORA` | `varchar(30)` | sim |
| 61 | `VALOR_FRETE` | `decimal(14,2)` | sim |
| 62 | `PRODUTO_NOVO_OU_USAD` | `varchar(20)` | sim |
| 63 | `FORMULARIO_NUMERO1` | `decimal(14,0)` | sim |
| 64 | `DESCONTO_INCONDICION` | `varchar(3)` | sim |
| 65 | `TRANSPORTE_ENTREGA1` | `varchar(30)` | sim |
| 66 | `MEU_PRIMEIRO_JD` | `varchar(3)` | sim |

### IV_Q$ADM_FINANCEIRO

`view` · `30 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CNPJ_CPF_DO_COMPRADO` | `varchar(30)` | sim |
| 19 | `AGENCIA` | `varchar(30)` | sim |
| 20 | `NRO_CHASSI` | `varchar(30)` | sim |
| 21 | `DATA_PROTOCOLO_NO_BA` | `datetime` | sim |
| 22 | `DATA_AUTORIZACAO_FAT` | `datetime` | sim |
| 23 | `DATA_FATURAMENTO` | `datetime` | sim |
| 24 | `NRO_NF` | `decimal(14,0)` | sim |
| 25 | `DATA_LIBERACAO_DO_RE` | `datetime` | sim |
| 26 | `VALOR_RECEBIDO` | `decimal(14,2)` | sim |
| 27 | `DATA_DA_PROPOSTA` | `datetime` | sim |
| 28 | `PFA___PREVISAO_DE_FA` | `datetime` | sim |
| 29 | `NUMERO_DA_PROPOSTA` | `decimal(14,0)` | sim |
| 30 | `LOJA` | `varchar(20)` | sim |

### IV_Q$ADM_FINANCEIRO_N

`view` · `48 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NOMERAZAO` | `varchar(100)` | sim |
| 3 | `SEQQUESTIONARIO` | `numeric(18,0)` | **nao** |
| 4 | `SEQFORMULARIO` | `numeric(18,0)` | **nao** |
| 5 | `DESCRICAO` | `varchar(20)` | sim |
| 6 | `DTAREALIZACAO` | `datetime` | sim |
| 7 | `USUINCLUSAO` | `varchar(20)` | sim |
| 8 | `DTAALTERACAO` | `datetime` | sim |
| 9 | `USUALTERACAO` | `varchar(20)` | sim |
| 10 | `OBS` | `varchar(4000)` | sim |
| 11 | `SEQHISTORICO` | `float(53,0)` | sim |
| 12 | `RESULTADO` | `decimal(6,0)` | sim |
| 13 | `PROCESSO` | `decimal(15,0)` | sim |
| 14 | `DEPARTAMENTO` | `varchar(12)` | sim |
| 15 | `LINKDOCTO` | `varchar(10)` | sim |
| 16 | `LINKNRO` | `numeric(18,0)` | sim |
| 17 | `LINKSERIE` | `varchar(250)` | sim |
| 18 | `CNPJ_CPF_DO_COMPRADO` | `varchar(30)` | sim |
| 19 | `AGENCIA` | `varchar(30)` | sim |
| 20 | `NOME_GERENTE` | `varchar(1)` | **nao** |
| 21 | `TELEFONE_DA_AGENCI` | `varchar(1)` | **nao** |
| 22 | `MES_PREVISTO` | `date` | sim |
| 23 | `NRO_COTACAO` | `varchar(1)` | **nao** |
| 24 | `FDD` | `varchar(1)` | **nao** |
| 25 | `PROBABILIDADE_APR` | `varchar(1)` | **nao** |
| 26 | `ORIGEM_FATURAMENTO` | `varchar(1)` | **nao** |
| 27 | `NRO_CHASSI` | `varchar(30)` | sim |
| 28 | `PROTOCOLO_BANCO` | `datetime` | sim |
| 29 | `APROVACAO_CREDITO` | `varchar(1)` | **nao** |
| 30 | `DTA_REGISTRO_CED` | `varchar(1)` | **nao** |
| 31 | `AUTORIZACAO_FAT` | `datetime` | sim |
| 32 | `DATA_PAC` | `varchar(1)` | **nao** |
| 33 | `DTA_FATURAMENTO` | `datetime` | sim |
| 34 | `NRO_NF` | `decimal(14,0)` | sim |
| 35 | `LOCAL_ENTREGA` | `varchar(1)` | **nao** |
| 36 | `PREV_RECBTO` | `varchar(1)` | **nao** |
| 37 | `DTA_ENTREGA` | `varchar(1)` | **nao** |
| 38 | `NUMERO_DA_PROPOSTA` | `decimal(14,0)` | sim |
| 39 | `DTA_LIB_RECURSO` | `datetime` | sim |
| 40 | `VALOR_RECEBIDO` | `decimal(14,2)` | sim |
| 41 | `MARCADO_ENTREGUE` | `varchar(1)` | **nao** |
| 42 | `DATA_PENHOR` | `varchar(1)` | **nao** |
| 43 | `DATA_INCENTIVO` | `varchar(1)` | **nao** |
| 44 | `DTA_PROPOSTA` | `datetime` | sim |
| 45 | `PFA` | `datetime` | sim |
| 46 | `DTA_CCB_EMITIDA` | `varchar(1)` | **nao** |
| 47 | `NRO_PAC` | `varchar(1)` | **nao** |
| 48 | `NRO_PROPOSTA` | `decimal(14,0)` | sim |

### IV_Q$AFERICAO_CSC

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VOCE_RECEBEU_EM_SEU_` | `varchar(3)` | sim |
| 19 | `VOCE_CHEGOU_A_ABRIR_` | `varchar(3)` | sim |
| 20 | `SE_SIM__DE_1_A_5_QUE` | `varchar(20)` | sim |
| 21 | `VOCE_RECEBEU_ALGUM_T` | `varchar(3)` | sim |
| 22 | `EM_UMA_ESCALA_DE_MUI` | `varchar(20)` | sim |

### IV_Q$AFERICAO_DE_PECAS

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFEITO_ATENDIMEN` | `varchar(3)` | sim |
| 19 | `CUMPRIMENTO` | `varchar(3)` | sim |
| 20 | `PRODUTO` | `varchar(3)` | sim |
| 21 | `SUGESTAO` | `varchar(3)` | sim |
| 22 | `NOTA_NPS` | `varchar(20)` | sim |
| 23 | `PRODUTO_DISP` | `varchar(50)` | sim |
| 24 | `SUGESTAO_PECAS` | `varchar(30)` | sim |

### IV_Q$AFERICAO_IMPLEMENTO

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFEITO_ATENDIMEN` | `varchar(3)` | sim |
| 19 | `A_AREA_COMERCIAL_ATE` | `varchar(3)` | sim |
| 20 | `COMBINADO_CUMPRIDO` | `varchar(3)` | sim |
| 21 | `ORIENTOU_EQUIPAMENTO` | `varchar(3)` | sim |
| 22 | `CONSULTAR_CONTATO` | `varchar(3)` | sim |
| 23 | `PROCESSO_FINANCEIRO` | `varchar(20)` | sim |
| 24 | `PRAZO_ENTREGA` | `varchar(3)` | sim |
| 25 | `ENTREGA_REALIZAD` | `varchar(3)` | sim |
| 26 | `FICOU_SATISFEITO` | `varchar(3)` | sim |
| 27 | `UTILIZOU_PRODUTO` | `varchar(3)` | sim |
| 28 | `DESEMPENHO` | `varchar(3)` | sim |
| 29 | `NPS_0_10` | `varchar(20)` | sim |

### IV_Q$AFERICAO_MAQ_1_CONT

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `ESTA_SATISFEITO_COM_` | `varchar(3)` | sim |
| 19 | `DURANTE_NEGOCIACAO` | `varchar(3)` | sim |
| 20 | `COMBINADO_CUMPRIDO` | `varchar(3)` | sim |
| 21 | `UTILIZOU_DEMONSTRACA` | `varchar(3)` | sim |
| 22 | `FINANCEIRO_ADEQUADO` | `varchar(3)` | sim |
| 23 | `PRAZO_CUMPRIDO` | `varchar(3)` | sim |
| 24 | `A_ENTREGA_TECNICA_FO` | `varchar(3)` | sim |
| 25 | `NPS_0_A_10` | `varchar(20)` | sim |
| 26 | `FICOU_SATISFEITO` | `varchar(3)` | sim |
| 27 | `QUAL_AVIALIACAO` | `varchar(20)` | sim |

### IV_Q$AFERICAO_MAQ_1_WEB

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFEITO_VENDA` | `varchar(3)` | sim |
| 19 | `ATENDEU_NEGOCIACAO` | `varchar(3)` | sim |
| 20 | `FOI_CUMPRIDO` | `varchar(3)` | sim |
| 21 | `PROGRAMA_DEMONSTRACA` | `varchar(3)` | sim |
| 22 | `PROCESSO_FINANCEIRO` | `varchar(3)` | sim |
| 23 | `PRAZO_ENTREGA` | `varchar(3)` | sim |
| 24 | `ENTREGA_TECNICA` | `varchar(3)` | sim |
| 25 | `NPS_MAQUINA` | `varchar(20)` | sim |

### IV_Q$AFERICAO_MAQ_2_CONT

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `UTILIZA_PRODUTO` | `varchar(3)` | sim |
| 19 | `CONTATO_CONSULTOR` | `varchar(3)` | sim |
| 20 | `COM_QUEM_FALAR` | `varchar(3)` | sim |
| 21 | `PRECISOU_ATENDIMENTO` | `varchar(3)` | sim |
| 22 | `EM_UMA_ESCALA_DE_0_A` | `varchar(20)` | sim |
| 23 | `BEM_ATENDIDO` | `varchar(3)` | sim |

### IV_Q$AFERICAO_MAQ_2_WEB

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `UTILIZA_PRODUTO` | `varchar(3)` | sim |
| 19 | `CONTATO_CONSULTOR` | `varchar(3)` | sim |

### IV_Q$AFERICAO_MAQ_3_CONT

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `ENTROU_CONTATO` | `varchar(3)` | sim |
| 19 | `CONSULTOR_CONTATO` | `varchar(3)` | sim |
| 20 | `NECESSIDADE_PRODUTO` | `varchar(3)` | sim |
| 21 | `NPS_0_A_10` | `varchar(20)` | sim |
| 22 | `SATISFACAO_ESCOLHA` | `varchar(20)` | sim |
| 23 | `SATISFACAO_DESEMPENH` | `varchar(20)` | sim |
| 24 | `SATISFACAO_DSIPONIBI` | `varchar(20)` | sim |
| 25 | `DISPONILIDADE_ESTOQU` | `varchar(20)` | sim |
| 26 | `SATISFACAO_TEMPO` | `varchar(20)` | sim |
| 27 | `SATISFACAO_RESPEITO` | `varchar(20)` | sim |
| 28 | `PROBLEMA_N_RESOLVIDO` | `varchar(3)` | sim |
| 29 | `MELHOR_EXPERIENCIA` | `varchar(90)` | sim |

### IV_Q$AFERICAO_MAQ_3_WEB

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `RECEBEU_CONTATO` | `varchar(3)` | sim |
| 19 | `O_CONSULTOR_DE_VENDA` | `varchar(3)` | sim |
| 20 | `NECESSIDADE_PRODUTO` | `varchar(3)` | sim |

### IV_Q$AFERICAO_PECAS

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |

### IV_Q$AFERICAO_POS_SOLUCAO

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `Q001_INSATISFEITO` | `decimal(1,0)` | sim |
| 19 | `Q001_INDIFERENTE` | `decimal(1,0)` | sim |
| 20 | `Q001_SATISFEITO` | `decimal(1,0)` | sim |

### IV_Q$AFERICAO_SERVICOS

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `Q001_INSATISFEITO` | `decimal(1,0)` | sim |
| 19 | `Q001_INDIFERENTE` | `decimal(1,0)` | sim |
| 20 | `Q001_SATISFEITO` | `decimal(1,0)` | sim |
| 21 | `Q004_0` | `decimal(1,0)` | sim |
| 22 | `Q004_1` | `decimal(1,0)` | sim |
| 23 | `Q004_2` | `decimal(1,0)` | sim |
| 24 | `Q004_3` | `decimal(1,0)` | sim |
| 25 | `Q004_4` | `decimal(1,0)` | sim |
| 26 | `Q004_5` | `decimal(1,0)` | sim |
| 27 | `Q004_6` | `decimal(1,0)` | sim |
| 28 | `Q004_7` | `decimal(1,0)` | sim |
| 29 | `Q004_8` | `decimal(1,0)` | sim |
| 30 | `Q004_9` | `decimal(1,0)` | sim |
| 31 | `Q004_10` | `decimal(1,0)` | sim |
| 32 | `RECOMEND_DEPTO_SERVI` | `varchar(3)` | sim |

### IV_Q$AFERICAO_SERV_WEB

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `OS_PRAZOS_AGENDADOS_` | `varchar(3)` | sim |
| 19 | `AGENDAMENTO_ENTREGA` | `varchar(3)` | sim |
| 20 | `EXPECTATIVA_TECNICO` | `varchar(3)` | sim |
| 21 | `SERV_CORRETAMENTE` | `varchar(3)` | sim |
| 22 | `Q005_1` | `decimal(1,0)` | sim |
| 23 | `Q005_2` | `decimal(1,0)` | sim |
| 24 | `Q005_3` | `decimal(1,0)` | sim |
| 25 | `Q005_4` | `decimal(1,0)` | sim |
| 26 | `Q005_5` | `decimal(1,0)` | sim |
| 27 | `Q005_6` | `decimal(1,0)` | sim |
| 28 | `Q005_7` | `decimal(1,0)` | sim |
| 29 | `Q005_8` | `decimal(1,0)` | sim |
| 30 | `Q005_9` | `decimal(1,0)` | sim |
| 31 | `Q005_` | `decimal(1,0)` | sim |
| 32 | `NOTANPS` | `varchar(20)` | sim |

### IV_Q$AFERICAO_SUPORTE_INT

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `INFORME_SATISFACAO` | `varchar(40)` | sim |

### IV_Q$AFERICAO_VENDA_MAQ

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `SATISFEITO_PERFORMAN` | `varchar(3)` | sim |
| 19 | `SATISFACAO_CUSTO_OPE` | `varchar(20)` | sim |
| 20 | `SATISFACAO_ATENDIMEN` | `varchar(20)` | sim |
| 21 | `QUAL_SETOR_HA_OPORTU` | `varchar(20)` | sim |
| 22 | `RECOMENDACAO_COLORAD` | `varchar(20)` | sim |
| 23 | `PROBLEMA_NRESOLVIDO` | `varchar(3)` | sim |
| 24 | `SR_NOS_INDICA` | `varchar(50)` | sim |
| 25 | `QUAL_COLABORADOR_DE_` | `varchar(30)` | sim |

### IV_Q$AFERICAO_VENDA_MQ_IM

`view` · `48 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `Q001_ZERO` | `decimal(1,0)` | sim |
| 19 | `Q001_UM` | `decimal(1,0)` | sim |
| 20 | `Q001_DOIS` | `decimal(1,0)` | sim |
| 21 | `Q001_TRES` | `decimal(1,0)` | sim |
| 22 | `Q001_QUATRO` | `decimal(1,0)` | sim |
| 23 | `Q001_CINCO` | `decimal(1,0)` | sim |
| 24 | `Q001_SEIS` | `decimal(1,0)` | sim |
| 25 | `Q001_SETE` | `decimal(1,0)` | sim |
| 26 | `Q001_OITO` | `decimal(1,0)` | sim |
| 27 | `Q001_NOVE` | `decimal(1,0)` | sim |
| 28 | `Q001_DEZ` | `decimal(1,0)` | sim |
| 29 | `Q002_INSATISFEITO` | `decimal(1,0)` | sim |
| 30 | `Q002_INDIFERENTE` | `decimal(1,0)` | sim |
| 31 | `Q002_SATISFEITO` | `decimal(1,0)` | sim |
| 32 | `Q003_INSATISFEITO` | `decimal(1,0)` | sim |
| 33 | `Q003_INDIFERENTE` | `decimal(1,0)` | sim |
| 34 | `Q003_SATISFEITO` | `decimal(1,0)` | sim |
| 35 | `Q004_INSATISFEITO` | `decimal(1,0)` | sim |
| 36 | `Q004_INDIFERENTE` | `decimal(1,0)` | sim |
| 37 | `Q004_SATISFEITO` | `decimal(1,0)` | sim |
| 38 | `RECOM_COLORADO_JD_VE` | `varchar(3)` | sim |
| 39 | `Q006_INSATISFEITO` | `decimal(1,0)` | sim |
| 40 | `Q006_INDIFERENTE` | `decimal(1,0)` | sim |
| 41 | `Q006_SATISFEITO` | `decimal(1,0)` | sim |
| 42 | `Q007_ESTRUTURA` | `decimal(1,0)` | sim |
| 43 | `Q007_AREA_VENDAS` | `decimal(1,0)` | sim |
| 44 | `Q007_AREA_PECAS` | `decimal(1,0)` | sim |
| 45 | `Q007_AREA_SERVICO` | `decimal(1,0)` | sim |
| 46 | `Q007_ORGANIZACAO` | `decimal(1,0)` | sim |
| 47 | `Q007_ESPACO_FISICO` | `decimal(1,0)` | sim |
| 48 | `PESQUISA_MQ_IMP` | `varchar(30)` | sim |

### IV_Q$AFE_VENDA_MQ_IM_USAD

`view` · `42 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `Q001_ZERO` | `decimal(1,0)` | sim |
| 19 | `Q001_UM` | `decimal(1,0)` | sim |
| 20 | `Q001_DOIS` | `decimal(1,0)` | sim |
| 21 | `Q001_TRES` | `decimal(1,0)` | sim |
| 22 | `Q001_QUATRO` | `decimal(1,0)` | sim |
| 23 | `Q001_CINCO` | `decimal(1,0)` | sim |
| 24 | `Q001_SEIS` | `decimal(1,0)` | sim |
| 25 | `Q001_SETE` | `decimal(1,0)` | sim |
| 26 | `Q001_OITO` | `decimal(1,0)` | sim |
| 27 | `Q001_NOVE` | `decimal(1,0)` | sim |
| 28 | `Q001_DEZ` | `decimal(1,0)` | sim |
| 29 | `Q002_INSATISFEITO` | `decimal(1,0)` | sim |
| 30 | `Q002_INDIFERENTE` | `decimal(1,0)` | sim |
| 31 | `Q002_SATISFEITO` | `decimal(1,0)` | sim |
| 32 | `RECOM_COLORADO_JD_VE` | `varchar(3)` | sim |
| 33 | `Q006_INSATISFEITO` | `decimal(1,0)` | sim |
| 34 | `Q006_INDIFERENTE` | `decimal(1,0)` | sim |
| 35 | `Q006_SATISFEITO` | `decimal(1,0)` | sim |
| 36 | `Q007_ESTRUTURA` | `decimal(1,0)` | sim |
| 37 | `Q007_AREA_VENDAS` | `decimal(1,0)` | sim |
| 38 | `Q007_AREA_PECAS` | `decimal(1,0)` | sim |
| 39 | `Q007_AREA_SERVICO` | `decimal(1,0)` | sim |
| 40 | `Q007_ORGANIZACAO` | `decimal(1,0)` | sim |
| 41 | `Q007_ESPACO_FISICO` | `decimal(1,0)` | sim |
| 42 | `PESQUISA_MQ_IMP` | `varchar(30)` | sim |

### IV_Q$AGUARDAR_PECAS

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `QUAL_TIPO_DE_COMPRA` | `varchar(20)` | sim |

### IV_Q$ALTERADO_PAGAMENTO

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `FORMA_PAGAMENTO` | `varchar(20)` | sim |
| 19 | `INSTITUICAO_FINANCEI` | `varchar(20)` | sim |
| 20 | `FINANCIADO` | `varchar(30)` | sim |

### IV_Q$ANALISE_DE_CREDITO

`view` · `120 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MEU_1_JD` | `varchar(3)` | sim |
| 19 | `TIPO_EQUIP1` | `varchar(20)` | sim |
| 20 | `MARCA_1` | `varchar(20)` | sim |
| 21 | `MODEL_EQUIP1` | `varchar(50)` | sim |
| 22 | `NRO_PED_VEND1` | `decimal(14,0)` | sim |
| 23 | `QTDE_PESO_TRAS1` | `decimal(14,2)` | sim |
| 24 | `KG` | `decimal(14,2)` | sim |
| 25 | `POSICAO_TRAS1` | `varchar(20)` | sim |
| 26 | `QDTE_PESO_TRAS2` | `decimal(14,2)` | sim |
| 27 | `KG_TRAS_2` | `decimal(14,2)` | sim |
| 28 | `POSICAO_PESO_TRAS2` | `varchar(20)` | sim |
| 29 | `QTDE_PESO_DIAN1` | `decimal(14,2)` | sim |
| 30 | `KG_PESO_DIAN1` | `decimal(14,2)` | sim |
| 31 | `LASTR_LIQ_TRAS1` | `varchar(3)` | sim |
| 32 | `LASTR_DIANT_LIQ1` | `varchar(3)` | sim |
| 33 | `RODADO_TRAS1` | `varchar(20)` | sim |
| 34 | `RODADO_DIANT1` | `varchar(20)` | sim |
| 35 | `QTD_VRC_1001` | `decimal(14,0)` | sim |
| 36 | `QTD_VCR_3001` | `decimal(14,0)` | sim |
| 37 | `AMS1` | `varchar(3)` | sim |
| 38 | `AMS_DE_FABRICA1` | `varchar(3)` | sim |
| 39 | `DESCRICAO_AMS1` | `varchar(4000)` | sim |
| 40 | `OUTRAS_CONFIG1` | `varchar(4000)` | sim |
| 41 | `VLR_TOTAL1` | `decimal(14,2)` | sim |
| 42 | `VLR_SINAL1` | `decimal(14,2)` | sim |
| 43 | `DTA_SINAL1` | `datetime` | sim |
| 44 | `VLR_FINAN1` | `decimal(14,2)` | sim |
| 45 | `TP_PGTO1` | `varchar(20)` | sim |
| 46 | `INSTITUI_FINANCEIRA1` | `varchar(20)` | sim |
| 47 | `NRO_AGENC1` | `decimal(14,0)` | sim |
| 48 | `NOME_AGENC1` | `varchar(30)` | sim |
| 49 | `NOME_GERENT1` | `varchar(30)` | sim |
| 50 | `TEL_AGENCIA1` | `varchar(30)` | sim |
| 51 | `EMAIL_AGENCIA1` | `varchar(50)` | sim |
| 52 | `INF_PARA_FINANC1` | `varchar(1000)` | sim |
| 53 | `NRO_CHASSI1` | `varchar(30)` | sim |
| 54 | `PROB_APROV_CRED1` | `varchar(10)` | sim |
| 55 | `NRO_PAC1` | `varchar(20)` | sim |
| 56 | `NRO_CONTR1` | `varchar(20)` | sim |
| 57 | `PREV_FATURAM1` | `datetime` | sim |
| 58 | `NRO_NF1` | `decimal(14,0)` | sim |
| 59 | `COD_FIN_MDA_CONS1` | `varchar(10)` | sim |
| 60 | `INFORMACOES_AGREGA__` | `varchar(4000)` | sim |
| 61 | `DATA_COMBINADA_CLIEN` | `datetime` | sim |
| 62 | `EXPECTATIVA_DATA_CLI` | `datetime` | sim |
| 63 | `DATA_ATUALIZADA` | `datetime` | sim |
| 64 | `ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 65 | `FATURAMENTO_REALIZ` | `datetime` | sim |
| 66 | `DATA_PEDIDO_DE_VENDA` | `datetime` | sim |
| 67 | `PAC_EMITIDO` | `datetime` | sim |
| 68 | `BITOLA` | `varchar(150)` | sim |
| 69 | `CODIGO_MDA` | `varchar(60)` | sim |
| 70 | `TAXA_FLAT` | `varchar(3)` | sim |
| 71 | `TAXA_FLAT__` | `varchar(30)` | sim |
| 72 | `NF_DE_REFATURAMENTO` | `decimal(14,0)` | sim |
| 73 | `DATA_INICIO_BANCO` | `datetime` | sim |
| 74 | `CALCULO_DA_BITOLA` | `varchar(20)` | sim |
| 75 | `Q058_ROTACAO` | `decimal(1,0)` | sim |
| 76 | `Q058_AUTOTRAV_SF3` | `decimal(1,0)` | sim |
| 77 | `Q058_CHICOTE_PF906` | `decimal(1,0)` | sim |
| 78 | `Q058_CHICOTE_A` | `decimal(1,0)` | sim |
| 79 | `Q058_MONITOR_GEN4_42` | `decimal(1,0)` | sim |
| 80 | `Q058_MONITOR_GEN4_46` | `decimal(1,0)` | sim |
| 81 | `Q058_MONITOR_GS3_263` | `decimal(1,0)` | sim |
| 82 | `Q058_RADIO_900MHZ` | `decimal(1,0)` | sim |
| 83 | `Q058_READY` | `decimal(1,0)` | sim |
| 84 | `Q058_SUPORTEPF903` | `decimal(1,0)` | sim |
| 85 | `Q058_SF_600` | `decimal(1,0)` | sim |
| 86 | `Q058_RECEPTOR_SF6000` | `decimal(1,0)` | sim |
| 87 | `Q058_VOLANTE_200` | `decimal(1,0)` | sim |
| 88 | `Q058_ATU_300` | `decimal(1,0)` | sim |
| 89 | `Q058_ATIVACAO_RTK_RE` | `decimal(1,0)` | sim |
| 90 | `Q058_ATIVACAO_SF3_RE` | `decimal(1,0)` | sim |
| 91 | `Q058_RADIO_450_MHZ` | `decimal(1,0)` | sim |
| 92 | `Q058_RADIO_900_MHZ` | `decimal(1,0)` | sim |
| 93 | `Q058_RADIO_450MHZ` | `decimal(1,0)` | sim |
| 94 | `Q058_KIT_AL207195` | `decimal(1,0)` | sim |
| 95 | `VALIDACAO_GESTOR` | `datetime` | sim |
| 96 | `VALOR_DO_USADO` | `decimal(14,2)` | sim |
| 97 | `VALOR_DA_AVALIACAO_U` | `decimal(14,2)` | sim |
| 98 | `HAVERA_USADO_NA_NEGO` | `varchar(3)` | sim |
| 99 | `CONFIRMACAO_DE_DADOS` | `datetime` | sim |
| 100 | `BONIFICACAO_DESCONTO` | `varchar(3)` | sim |
| 101 | `VALOR_BONIFICACAO_DE` | `decimal(14,2)` | sim |
| 102 | `TIPO_DO_FRETE` | `varchar(20)` | sim |
| 103 | `VALOR_DO_FRETE` | `decimal(14,2)` | sim |
| 104 | `NOME_TRANSPORTADORA` | `varchar(45)` | sim |
| 105 | `SINAL_EMBUTIDO` | `varchar(3)` | sim |
| 106 | `MONITOR` | `varchar(20)` | sim |
| 107 | `RECEPTOR` | `varchar(20)` | sim |
| 108 | `RADIO` | `varchar(20)` | sim |
| 109 | `VOLANTE` | `varchar(20)` | sim |
| 110 | `ATIVACAO` | `varchar(20)` | sim |
| 111 | `CONECTIVIDADE` | `varchar(20)` | sim |
| 112 | `CREEPER` | `varchar(20)` | sim |
| 113 | `TROCA_DE_EIXO_3_METR` | `varchar(20)` | sim |
| 114 | `TROCA_DE_RODADO_E_PN` | `varchar(20)` | sim |
| 115 | `PESOS_CONFIGURACAO_F` | `varchar(20)` | sim |
| 116 | `INSTALACAO_DE_CABINE` | `varchar(20)` | sim |
| 117 | `TROCA_DE_ASSENTO` | `varchar(20)` | sim |
| 118 | `INSTALACAO_DE_VCR` | `varchar(20)` | sim |
| 119 | `FDD` | `decimal(14,0)` | sim |
| 120 | `COTACAO` | `decimal(14,0)` | sim |

### IV_Q$APRESENTACAO

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TABELA_PADRAO` | `varchar(3)` | sim |
| 19 | `Q002_EVENT` | `decimal(1,0)` | sim |
| 20 | `Q002_FOLDE` | `decimal(1,0)` | sim |
| 21 | `Q002_NO_EQUIPAMENT` | `decimal(1,0)` | sim |
| 22 | `Q002_VISITA_REF2` | `decimal(1,0)` | sim |
| 23 | `QUAL_O_EVENTO` | `varchar(250)` | sim |
| 24 | `QUAL_O_CLIENTE` | `varchar(250)` | sim |

### IV_Q$APRESENTACAO_JDE

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TABELA_PADRAO` | `varchar(3)` | sim |
| 19 | `Q002_EVENT` | `decimal(1,0)` | sim |
| 20 | `Q002_FOLDE` | `decimal(1,0)` | sim |
| 21 | `Q002_NO_EQUIPAMENT` | `decimal(1,0)` | sim |
| 22 | `Q002_VISITA_REF2` | `decimal(1,0)` | sim |
| 23 | `QUAL_O_EVENTO` | `varchar(250)` | sim |
| 24 | `QUAL_O_CLIENTE` | `varchar(250)` | sim |

### IV_Q$APRESENT_EQUIPAMENTO

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |

### IV_Q$APRESENT_IMPLEMENTO

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `Q001_EQUIP` | `decimal(1,0)` | sim |
| 19 | `Q001_EVENTOS` | `decimal(1,0)` | sim |
| 20 | `Q001_FOLD` | `decimal(1,0)` | sim |
| 21 | `Q001_VIS_CL_RE` | `decimal(1,0)` | sim |
| 22 | `QUAL_EV` | `varchar(250)` | sim |
| 23 | `QUAL_CLI` | `varchar(250)` | sim |

### IV_Q$APROVACAO_TCSM

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_RETORNO_TCSM` | `datetime` | sim |

### IV_Q$ATUALIZACAO_PUK

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CHASSI_DA_MAQUINA_` | `varchar(30)` | sim |
| 19 | `VALOR_ATUALIZACAO_DO` | `decimal(14,2)` | sim |
| 20 | `CHASSI_DO_RECEPTOR` | `varchar(30)` | sim |
| 21 | `VALOR_MONITOR` | `decimal(14,2)` | sim |
| 22 | `CHASSI_DO_MONITOR` | `varchar(30)` | sim |
| 23 | `VALOR_MODEM` | `decimal(14,2)` | sim |
| 24 | `CHASSI_MODEM` | `varchar(30)` | sim |
| 25 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 26 | `FORMA_DE_PAGAMENTO` | `varchar(20)` | sim |

### IV_Q$AVALIACAO_AMS_USADO

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MODELO` | `varchar(40)` | sim |
| 19 | `TIPO` | `varchar(20)` | sim |
| 20 | `CHASSI` | `varchar(20)` | sim |
| 21 | `VALOR` | `decimal(14,2)` | sim |
| 22 | `VALOR_FINAL_DEFINIDO` | `decimal(14,2)` | sim |

### IV_Q$AVALIA_COLHEIT_USADA

`view` · `67 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `ESTADO_GERAL` | `varchar(20)` | sim |
| 19 | `LATARIA_E_PINTURA` | `varchar(20)` | sim |
| 20 | `MOTOR_FUNCIONAMENTO` | `varchar(20)` | sim |
| 21 | `MOTOR___VAZAMENTOS` | `varchar(20)` | sim |
| 22 | `PARTE_ELETRICA_FUNCI` | `varchar(20)` | sim |
| 23 | `PARTE_ELETRICA___BAT` | `varchar(20)` | sim |
| 24 | `PAINEL_DE_INSTRUMENT` | `varchar(20)` | sim |
| 25 | `TRANSMISSAO___FUNCIO` | `varchar(20)` | sim |
| 26 | `PLATAF_CORTE_GRAOS` | `varchar(30)` | sim |
| 27 | `PLATAF_CORTE_GRA_SER` | `varchar(30)` | sim |
| 28 | `PLATAF_CORTE_MILHO` | `decimal(14,0)` | sim |
| 29 | `PLATAF_CORTE_GRAOS_E` | `varchar(20)` | sim |
| 30 | `PLATAF_CORTE_MILHO_S` | `varchar(30)` | sim |
| 31 | `SIT_PLAT_CORTE_MILHO` | `varchar(20)` | sim |
| 32 | `SISTEMA_DE_DEBULHA__` | `varchar(20)` | sim |
| 33 | `SACA_PALHAS` | `varchar(20)` | sim |
| 34 | `PENEIRAS` | `varchar(20)` | sim |
| 35 | `CORREIAS_E_REDUTORES` | `varchar(20)` | sim |
| 36 | `PICADOR` | `varchar(20)` | sim |
| 37 | `DISTRIBUIDOR_DE_PALH` | `varchar(20)` | sim |
| 38 | `GRANELEIRO___SITUACA` | `varchar(20)` | sim |
| 39 | `SEM_FIM_DO_GRANELEIR` | `varchar(20)` | sim |
| 40 | `TUBO_DE_DESCARGA` | `varchar(20)` | sim |
| 41 | `PLATAFORMA_DE_OPERAC` | `varchar(20)` | sim |
| 42 | `NOTAL_GERAL_ATRIBUID` | `varchar(20)` | sim |
| 43 | `MED_PNEU_DIAN_DIREIT` | `varchar(6)` | sim |
| 44 | `MED_PNEU_DIAN_ESQUER` | `varchar(6)` | sim |
| 45 | `MED_PNEU_TRAS_DIREIT` | `varchar(6)` | sim |
| 46 | `MED_PNEU_TRAS_ESQUER` | `varchar(6)` | sim |
| 47 | `OBSERVACOES_GERAIS` | `varchar(1000)` | sim |
| 48 | `VALOR_PEDIDO_PELO_CL` | `decimal(14,2)` | sim |
| 49 | `VALOR_APURADO_NO_MER` | `decimal(14,2)` | sim |
| 50 | `VALOR_ATRIBUIDO_PARA` | `decimal(14,2)` | sim |
| 51 | `DATA_DE_VALIDADE_DA_` | `datetime` | sim |
| 52 | `PNEU_DIANT_DIR_TIPO` | `varchar(20)` | sim |
| 53 | `PNEU_DIAN_DIR_MARCA` | `varchar(30)` | sim |
| 54 | `PNEU_DIAN_DIR_VIDA_U` | `decimal(14,0)` | sim |
| 55 | `PNEU_DIAN_DIR_AVARIA` | `varchar(200)` | sim |
| 56 | `PNEU_DIAN_ESQ_TIPO` | `varchar(20)` | sim |
| 57 | `PNEU_DIAN_ESQ_MARCA` | `varchar(30)` | sim |
| 58 | `PNEU_DIAN_ESQ_VIDA_U` | `decimal(14,0)` | sim |
| 59 | `PNEU_DIAN_ESQ_AVARIA` | `varchar(200)` | sim |
| 60 | `PNEU_TRAS_DIR_TIPO` | `varchar(20)` | sim |
| 61 | `PNEU_TRAS_DIR_MARCA` | `varchar(30)` | sim |
| 62 | `PNEU_TRAS_DIR_VIDA_U` | `decimal(14,0)` | sim |
| 63 | `PNEU_TRAS_DIR_AVARIA` | `varchar(200)` | sim |
| 64 | `PNEU_TRAS_ESQ_TIPO` | `varchar(20)` | sim |
| 65 | `PNEU_TRAS_ESQ_MARCA` | `varchar(30)` | sim |
| 66 | `PNEU_TRAS_ESQ_VIDA_U` | `decimal(14,0)` | sim |
| 67 | `PNEU_TRAS_ESQ_AVARIA` | `varchar(200)` | sim |

### IV_Q$AVALIA_IMPLEM_USADO

`view` · `52 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MARCA_IMP_USADO` | `varchar(20)` | sim |
| 19 | `MODELO_IMP_USADO` | `varchar(20)` | sim |
| 20 | `ANO_IMP_USADO` | `varchar(30)` | sim |
| 21 | `VALOR_IMP_USADO` | `decimal(14,2)` | sim |
| 22 | `CHASSI_IMP_USADO` | `varchar(30)` | sim |
| 23 | `MOTOR_IMP` | `varchar(20)` | sim |
| 24 | `BOMBA_HIDRAULICA` | `varchar(20)` | sim |
| 25 | `ENGATE` | `varchar(3)` | sim |
| 26 | `BARRA_DE_TRACAO` | `varchar(3)` | sim |
| 27 | `APOIO_DE_ENGATE` | `varchar(3)` | sim |
| 28 | `QTD_DISCO` | `decimal(14,0)` | sim |
| 29 | `ESTADO_DOS_DISCOS` | `varchar(20)` | sim |
| 30 | `QUANTIDADE_DE_HASTES` | `decimal(14,0)` | sim |
| 31 | `ESTADO_HASTES` | `varchar(20)` | sim |
| 32 | `CALCOS_DE_PROFUNDIDA` | `varchar(3)` | sim |
| 33 | `MANCAIS` | `varchar(20)` | sim |
| 34 | `MANGUEIRAS` | `varchar(20)` | sim |
| 35 | `CARACOL` | `varchar(20)` | sim |
| 36 | `QTD_BICOS` | `decimal(14,0)` | sim |
| 37 | `SUPORTE_FILTROS` | `varchar(3)` | sim |
| 38 | `REGULADORES` | `varchar(3)` | sim |
| 39 | `TURBINA` | `varchar(3)` | sim |
| 40 | `CABOS_CONTROLE` | `varchar(20)` | sim |
| 41 | `BARRAS` | `varchar(20)` | sim |
| 42 | `ESTADO_TANQUE` | `varchar(20)` | sim |
| 43 | `ESTEIRAS` | `varchar(20)` | sim |
| 44 | `CORREIAS` | `varchar(3)` | sim |
| 45 | `RECOLHEDOR` | `varchar(3)` | sim |
| 46 | `MONITORES` | `varchar(3)` | sim |
| 47 | `PINTURA` | `varchar(20)` | sim |
| 48 | `PNEUS_DIANTEIRO` | `varchar(3)` | sim |
| 49 | `ESTADO_DOS_PNEUS` | `varchar(20)` | sim |
| 50 | `PNEUS_TRASEIRO` | `varchar(3)` | sim |
| 51 | `RETIRADA_USADO` | `varchar(20)` | sim |
| 52 | `VALOR_FINAL_AVA` | `decimal(14,0)` | sim |

### IV_Q$AVALIA_USADO_ENTRADA

`view` · `50 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MODELO_DO_EQUIPAMENT` | `varchar(20)` | sim |
| 19 | `ENDERECO_PARA_A_AVAL` | `varchar(100)` | sim |
| 20 | `EXPECTATIVA_DE_VALOR` | `decimal(14,2)` | sim |
| 21 | `CHASSI_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 22 | `TIPO_DO_EQUIPAMENTO` | `varchar(20)` | sim |
| 23 | `MARCA` | `varchar(20)` | sim |
| 24 | `ANO_EQUIPAMENTO` | `decimal(14,0)` | sim |
| 25 | `VALOR_DA_AVALIACAO` | `decimal(14,2)` | sim |
| 26 | `HORIMETRO` | `varchar(30)` | sim |
| 27 | `N__SERIE` | `varchar(30)` | sim |
| 28 | `COR` | `varchar(30)` | sim |
| 29 | `MOTOR___FUNCIONAMENT` | `varchar(20)` | sim |
| 30 | `VAZAMENTOS___AGUA` | `varchar(20)` | sim |
| 31 | `VAZAMENTO___OLEO` | `varchar(20)` | sim |
| 32 | `VAZAMENTO___COMBUSTI` | `varchar(20)` | sim |
| 33 | `PARTE_ELETRICA___BAT` | `varchar(30)` | sim |
| 34 | `PARTE_ELETRICA___PAR` | `varchar(20)` | sim |
| 35 | `PNEUS_DIANTEIRO_DIRE` | `varchar(20)` | sim |
| 36 | `PNEUS_DIANTEIRO_ESQU` | `varchar(20)` | sim |
| 37 | `PNEUS_TRASEIRO_DIREI` | `varchar(20)` | sim |
| 38 | `PNEUS_TRASEIRO_ESQUE` | `varchar(20)` | sim |
| 39 | `VALOR_FINAL_DE_AVALI` | `decimal(14,2)` | sim |
| 40 | `COM_RETIRADA_DO_USAD` | `varchar(3)` | sim |
| 41 | `RETIRADA_DO_USADO1` | `varchar(20)` | sim |
| 42 | `COMO_SERA_REALIZADO_` | `varchar(20)` | sim |
| 43 | `COM_CABINE` | `varchar(3)` | sim |
| 44 | `PESO_DIANTEIRO` | `varchar(30)` | sim |
| 45 | `PESO_TRASEIRO` | `varchar(30)` | sim |
| 46 | `EMBREAGEM` | `varchar(20)` | sim |
| 47 | `TERCEIRO_PONTO` | `varchar(3)` | sim |
| 48 | `TRANSMISSAO` | `varchar(20)` | sim |
| 49 | `SISTEMA_INDUSTRIAL` | `varchar(20)` | sim |
| 50 | `ELEVADOR` | `varchar(20)` | sim |

### IV_Q$AVAL_COLH_CANA_USADA

`view` · `48 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `ESTADO_GERAL` | `varchar(20)` | sim |
| 19 | `LATARIA_E_PINTURA` | `varchar(20)` | sim |
| 20 | `MOTOR___FUNCIONAMENT` | `varchar(20)` | sim |
| 21 | `MOTOR___VAZAMENTO_DE` | `varchar(20)` | sim |
| 22 | `MOTOR_VAZ_COMBUSTIVE` | `varchar(20)` | sim |
| 23 | `MOTOR_VAZ_AGUA` | `varchar(20)` | sim |
| 24 | `PARTE_ELETRICA___BAT` | `varchar(20)` | sim |
| 25 | `PARTE_ELET_PARTIDA` | `varchar(20)` | sim |
| 26 | `PAINEL_DE_INSTRUMENT` | `varchar(20)` | sim |
| 27 | `TRANSMISSAO___FUNCIO` | `varchar(20)` | sim |
| 28 | `TRANSMISSAO___VAZAME` | `varchar(20)` | sim |
| 29 | `SISTEMA_HIDRAULICO__` | `varchar(20)` | sim |
| 30 | `SISTEMA_DE_DIRECAO` | `varchar(20)` | sim |
| 31 | `ROLOS_DIVISORES` | `varchar(20)` | sim |
| 32 | `ROLOS_PICADORES` | `varchar(20)` | sim |
| 33 | `ELEVADOR___CONDICOES` | `varchar(20)` | sim |
| 34 | `ELEVADOR___TALISCAS` | `varchar(20)` | sim |
| 35 | `EXTRATOR_PRIMARIO` | `varchar(20)` | sim |
| 36 | `EXTRATOR_SECUNDARIO` | `varchar(20)` | sim |
| 37 | `CHASSI_DA_MAQUINA___` | `varchar(20)` | sim |
| 38 | `CABINE_DO_OPERADOR` | `varchar(20)` | sim |
| 39 | `NOTAL_GERAL_ATRIBUID` | `varchar(20)` | sim |
| 40 | `MATERIAL_RODANTE___S` | `varchar(20)` | sim |
| 41 | `MATERIAL_RODANTE___L` | `varchar(20)` | sim |
| 42 | `MATERIAL_RODANTE___R` | `varchar(20)` | sim |
| 43 | `MAT_ROD_ROL_INFERIOR` | `varchar(20)` | sim |
| 44 | `OBS_GERAIS` | `varchar(1000)` | sim |
| 45 | `VALOR_PEDIDO_PELO_CL` | `decimal(14,2)` | sim |
| 46 | `VALOR_APURADO_NO_MER` | `decimal(14,2)` | sim |
| 47 | `VALOR_ATRIBUIDO_PARA` | `decimal(14,2)` | sim |
| 48 | `VALIDADE_DA_AVALIACA` | `datetime` | sim |

### IV_Q$AVAL_TRATORES_USADOS

`view` · `70 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `ESTADO_GERAL` | `varchar(20)` | sim |
| 19 | `LATARIA_E_PINTURA` | `varchar(20)` | sim |
| 20 | `MOTOR___FUNCIONAMENT` | `varchar(20)` | sim |
| 21 | `MOTOR___VAZAMENTO_DE` | `varchar(20)` | sim |
| 22 | `MOTOR_VAZ_COMBUSTIVE` | `varchar(20)` | sim |
| 23 | `MOTOR_VAZ_AGUA` | `varchar(20)` | sim |
| 24 | `PARTE_ELETRICA___BAT` | `varchar(20)` | sim |
| 25 | `PARTE_ELET_PARTIDA` | `varchar(20)` | sim |
| 26 | `PARTE_ELETR_FUNCIONA` | `varchar(20)` | sim |
| 27 | `TRANSMISSAO___FUNCIO` | `varchar(20)` | sim |
| 28 | `TRANSMISSAO___VAZAME` | `varchar(20)` | sim |
| 29 | `EMBREAGEM___FUNCIONA` | `varchar(20)` | sim |
| 30 | `SISTEMA_DE_DIRECAO` | `varchar(20)` | sim |
| 31 | `PAINEL_DE_INSTRUMENT` | `varchar(20)` | sim |
| 32 | `TRACAO_DIANTEIRA_AUX` | `varchar(20)` | sim |
| 33 | `EIXO_DIANT_BUCH_TERM` | `varchar(20)` | sim |
| 34 | `EIXO_TRASEIRO___DIFE` | `varchar(20)` | sim |
| 35 | `EIXO_TRASEIRO___FREI` | `varchar(20)` | sim |
| 36 | `EIXO_TRASEIRO___VAZA` | `varchar(20)` | sim |
| 37 | `HIDRAULICO___FUNCION` | `varchar(20)` | sim |
| 38 | `HIDRAULICO___VAZAMEN` | `varchar(20)` | sim |
| 39 | `HIDRAULICO___LEVANTA` | `varchar(20)` | sim |
| 40 | `TOMADA_DE_FORCA___FU` | `varchar(20)` | sim |
| 41 | `VCR___QUANTIDADE_E_S` | `varchar(20)` | sim |
| 42 | `BARRA_DE_TRACAO___SI` | `varchar(20)` | sim |
| 43 | `PLATAFORMA_DE_OPERAC` | `varchar(20)` | sim |
| 44 | `POSSUI_IMPLEMENTO_IN` | `varchar(3)` | sim |
| 45 | `NOTAL_GERAL_ATRIBUID` | `varchar(20)` | sim |
| 46 | `PNEU_DIANTEIRO_DIREI` | `varchar(20)` | sim |
| 47 | `PNEU_DIAN_DIR_MARCA` | `varchar(30)` | sim |
| 48 | `PNEU_DIAN_DIR_MEDIDA` | `varchar(8)` | sim |
| 49 | `PNEU_DIAN_DIR_VIDA_U` | `decimal(14,0)` | sim |
| 50 | `PNEU_DIAN_DIR_AVARIA` | `varchar(200)` | sim |
| 51 | `PNEU_DIANTEIRO_ESQUE` | `varchar(20)` | sim |
| 52 | `PNEU_DIAN_ESQ_MARCA` | `varchar(30)` | sim |
| 53 | `PNEU_DIAN_ESQ_MEDIDA` | `varchar(8)` | sim |
| 54 | `PNEU_DIAN_ESQ_VIDA_U` | `decimal(14,0)` | sim |
| 55 | `PNEU_DIAN_ESQ_AVARIA` | `varchar(200)` | sim |
| 56 | `PNEU_TRASEIRO_DIREIT` | `varchar(20)` | sim |
| 57 | `PNEU_TRAS_DIR_MARCA` | `varchar(30)` | sim |
| 58 | `PNEU_TRAS_DIR_MEDIDA` | `varchar(8)` | sim |
| 59 | `PNEU_TRAS_DIR_VIDA_U` | `decimal(14,0)` | sim |
| 60 | `PNEU_TRAS_DIR_AVARIA` | `varchar(200)` | sim |
| 61 | `PNEU_TRASEITO_ESQUER` | `varchar(20)` | sim |
| 62 | `PNEU_TRAS_ESQ_MARCA` | `varchar(30)` | sim |
| 63 | `PNEU_TRAS_ESQ_MEDIDA` | `varchar(8)` | sim |
| 64 | `PNEU_TRAS_ESQ_VIDA_U` | `decimal(14,0)` | sim |
| 65 | `PNEU_TRAS_ESQ_AVARIA` | `varchar(200)` | sim |
| 66 | `OBSERVACOES_GERAIS` | `varchar(1000)` | sim |
| 67 | `VALOR_PEDIDO_PELO_CL` | `decimal(14,2)` | sim |
| 68 | `VALOR_APURADO_NO_MER` | `decimal(14,2)` | sim |
| 69 | `VALOR_ATRIBUIDO_PARA` | `decimal(14,2)` | sim |
| 70 | `DATA_DE_VALIDADE_DA_` | `datetime` | sim |

### IV_Q$AVAL_USADO_ENTRADA

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `EQUIPAMENTO` | `varchar(20)` | sim |
| 19 | `MARCA` | `varchar(20)` | sim |
| 20 | `MODELO` | `varchar(20)` | sim |
| 21 | `VALOR_USADO` | `decimal(14,2)` | sim |
| 22 | `VALOR_AVALIACAO_USAD` | `decimal(14,2)` | sim |
| 23 | `RETIRADO_DO_USADO` | `varchar(20)` | sim |
| 24 | `CHASSI` | `varchar(30)` | sim |
| 25 | `VALOR_VALIDACAO_GEST` | `decimal(14,2)` | sim |
| 26 | `ENTRADA_DO_USADO` | `varchar(20)` | sim |

### IV_Q$CADASTROS_LISTAS

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `ORIGEM_DA_RENDA` | `varchar(20)` | sim |

### IV_Q$CANCELAMENTO_SEGURO

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SEGURADORA` | `varchar(20)` | sim |
| 19 | `PRECO` | `decimal(14,2)` | sim |
| 20 | `MOTIVO` | `varchar(30)` | sim |
| 21 | `APOLICE` | `varchar(30)` | sim |

### IV_Q$CANCEL_RENOVACAO_SEG

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CANCEL_RENOV_SEGURO` | `varchar(30)` | sim |
| 19 | `CANCEL_RENOV_PRECO` | `decimal(14,2)` | sim |
| 20 | `CANCEL_SEGUR_MOTIVO` | `varchar(20)` | sim |
| 21 | `CANCEL_RENOV_SEG_APO` | `varchar(35)` | sim |

### IV_Q$CANHOTO_DIGITAL

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CANHOTO_NRO_NF` | `varchar(30)` | sim |
| 19 | `CANHOTO_RESP_RETIRAD` | `varchar(50)` | sim |
| 20 | `CANHOTO_DOC_RESP` | `varchar(30)` | sim |
| 21 | `CANHOTO_DATA_RETIRAD` | `datetime` | sim |

### IV_Q$CHASSI_ENTREGA_FISIC

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_CHASSI` | `varchar(30)` | sim |

### IV_Q$CHASSI_EQUIPAMENTO

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_CHASSI` | `varchar(30)` | sim |
| 19 | `LOJA` | `varchar(20)` | sim |

### IV_Q$CHASSI_PMP

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CHASSI` | `varchar(30)` | sim |
| 19 | `NUMERO_PMP` | `varchar(30)` | sim |
| 20 | `NUM_SOLUCAO_DTAC` | `varchar(30)` | sim |

### IV_Q$CHEGADA_IMPLEMENTO

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `DTA_CHEGADA_IMPL` | `datetime` | sim |

### IV_Q$COMISSAO

`view` · `52 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VALOR_DO_EQUIPAMENTO` | `decimal(14,2)` | sim |
| 19 | `DESCONTO_INCONDICION` | `decimal(14,2)` | sim |
| 20 | `VALOR_DO_EQUIP__USAD` | `decimal(14,2)` | sim |
| 21 | `VALOR_BASE_COMISSAO` | `decimal(14,2)` | sim |
| 22 | `__COMISSAO__RH_` | `varchar(5)` | sim |
| 23 | `VALOR_COMISSAO` | `decimal(14,2)` | sim |
| 24 | `COMISSAO_PAGA___RH_` | `varchar(3)` | sim |
| 25 | `LINHA` | `varchar(40)` | sim |
| 26 | `VLR_COMISSAO` | `varchar(20)` | sim |
| 27 | `PERC_COMISSAO` | `varchar(10)` | sim |
| 28 | `ACELERADOR___RISCO__` | `varchar(40)` | sim |
| 29 | `ACELERADOR___MARGEM_` | `varchar(40)` | sim |
| 30 | `Q013_AGRICULTURA_DE_` | `decimal(1,0)` | sim |
| 31 | `Q013_CONTRATOS_DE_PE` | `decimal(1,0)` | sim |
| 32 | `Q013_TREINAMENTO` | `decimal(1,0)` | sim |
| 33 | `Q013_VENDA_1__TRATOR` | `decimal(1,0)` | sim |
| 34 | `VENDA_DE_PACOTE_OU_P` | `varchar(20)` | sim |
| 35 | `UTILIZACAO_DE_MARGEM` | `varchar(40)` | sim |
| 36 | `VALOR_DE_VENDA_SEM_I` | `decimal(14,2)` | sim |
| 37 | `ESPECIALISTA_NA_VEND` | `varchar(3)` | sim |
| 38 | `ESPECIALISTA_VENDA_A` | `varchar(20)` | sim |
| 39 | `CAMPANHA_PROMOCIONAL` | `varchar(20)` | sim |
| 40 | `VLR_TOTAL_COMISSAO` | `varchar(30)` | sim |
| 41 | `POSSUI_ACEL_PROD_PAC` | `varchar(3)` | sim |
| 42 | `SINAL_EMBUTIDO` | `varchar(3)` | sim |
| 43 | `VLR_COMISSAO_ESPECIA` | `varchar(20)` | sim |
| 44 | `VLR_DESCONTO_INCO` | `decimal(14,2)` | sim |
| 45 | `COMISSAO_DESCONTO_IN` | `varchar(20)` | sim |
| 46 | `MARGEM_LUCRO_OPE` | `varchar(6)` | sim |
| 47 | `VLR_COMISSAO_CEN` | `decimal(14,2)` | sim |
| 48 | `MARGEM_LUCRO_OPERACI` | `varchar(30)` | sim |
| 49 | `VALOR_COMISSAO_ESP` | `decimal(14,2)` | sim |
| 50 | `VALOR_COMISSAO_CEN` | `decimal(14,2)` | sim |
| 51 | `VLR_COMISSAO_ADC_ESP` | `decimal(14,2)` | sim |
| 52 | `VLR_COMISSAO_ADC_CEN` | `decimal(14,2)` | sim |

### IV_Q$COMISSAO_AMS

`view` · `36 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VALOR_DO_AMS` | `decimal(14,2)` | sim |
| 19 | `CEN` | `varchar(20)` | sim |
| 20 | `ESPECIALISTA_AMS` | `varchar(20)` | sim |
| 21 | `__COMISSAO_CEN` | `varchar(30)` | sim |
| 22 | `VALOR_COMISSAO_CEN` | `decimal(14,2)` | sim |
| 23 | `__COMISSAO_ESPECIALI` | `varchar(30)` | sim |
| 24 | `VALORCOMISSAO_ESPECI` | `varchar(30)` | sim |
| 25 | `COMISSAO_PAGA` | `varchar(3)` | sim |
| 26 | `VALOR_FINAL_DA_COMIS` | `decimal(14,2)` | sim |
| 27 | `OBSERVACAO_GERAL` | `varchar(30)` | sim |
| 28 | `VLR_COMISSAO_CEN` | `varchar(20)` | sim |
| 29 | `VLR_COMISSAO_ESPECIA` | `varchar(20)` | sim |
| 30 | `VLR_TOTAL_COMISSAO` | `varchar(20)` | sim |
| 31 | `SINAL_EMBUTIDO` | `varchar(3)` | sim |
| 32 | `COMISSAO__ESPECIAL` | `varchar(20)` | sim |
| 33 | `VALORCOMISSAO_CEN` | `decimal(14,2)` | sim |
| 34 | `VALORCOMISSAO_ESPEC` | `decimal(14,2)` | sim |
| 35 | `MARGEM_LUCRO` | `varchar(30)` | sim |
| 36 | `COMISSAO__CEN` | `varchar(20)` | sim |

### IV_Q$COMISSAO_CONTACHAVE

`view` · `34 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VALOR_VENDA` | `decimal(14,2)` | sim |
| 19 | `CEN` | `varchar(20)` | sim |
| 20 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 21 | `Q004_TRATORES_LINHA5` | `decimal(1,0)` | sim |
| 22 | `Q004_TRATORES_LINHA6` | `decimal(1,0)` | sim |
| 23 | `Q004_TRATORES_LINHA7` | `decimal(1,0)` | sim |
| 24 | `Q004_TRATORES_LINHA8` | `decimal(1,0)` | sim |
| 25 | `Q004_COLHEDORAS` | `decimal(1,0)` | sim |
| 26 | `Q004_COLHEITADEIRAS_` | `decimal(1,0)` | sim |
| 27 | `Q004_GREEN_SYSTEM` | `decimal(1,0)` | sim |
| 28 | `Q004_PLANTADEIRAS` | `decimal(1,0)` | sim |
| 29 | `Q004_PULVERIZADORES` | `decimal(1,0)` | sim |
| 30 | `VALOR_COMISSAO` | `decimal(14,2)` | sim |
| 31 | `PREMIO_PAGO` | `varchar(3)` | sim |
| 32 | `ESPECIALISTA_VENDA` | `varchar(20)` | sim |
| 33 | `PREMIO_ESPECIALISTA` | `decimal(14,2)` | sim |
| 34 | `EQUIPAMENTO_AMS_FULL` | `varchar(3)` | sim |

### IV_Q$COMISSAO_LOCACAO

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_DE_EQUIPAMENTO_` | `varchar(40)` | sim |
| 19 | `MARCA` | `varchar(30)` | sim |
| 20 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 21 | `CHASSI` | `varchar(20)` | sim |
| 22 | `ANO_MODELO_EQUIPAMEN` | `varchar(10)` | sim |
| 23 | `HORIMETRO` | `varchar(20)` | sim |
| 24 | `DATA_INICIO_LOCACAO` | `datetime` | sim |
| 25 | `DATA_TERMINO_LOCACAO` | `datetime` | sim |
| 26 | `VALOR_MENSAL_DO_CONT` | `decimal(14,2)` | sim |
| 27 | `VALOR_TOTAL_DO_CONTR` | `decimal(14,2)` | sim |
| 28 | `DATA_RENOVACAO` | `datetime` | sim |
| 29 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim |

### IV_Q$COMISSAO_SERV_AMS

`view` · `31 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TIPO_DE_SERVICO` | `varchar(20)` | sim |
| 19 | `VALOR_DO_SERVICO` | `decimal(14,2)` | sim |
| 20 | `ESPECIALISTA_AMS` | `varchar(20)` | sim |
| 21 | `__COMISSAO_CEN` | `varchar(30)` | sim |
| 22 | `__COMISSAO_ESPECIALI` | `varchar(30)` | sim |
| 23 | `CEN` | `varchar(30)` | sim |
| 24 | `VALOR_COMISSAO_ESPEC` | `decimal(14,2)` | sim |
| 25 | `VALOR_COMISSAO_CEN` | `decimal(14,2)` | sim |
| 26 | `CEN_` | `varchar(20)` | sim |
| 27 | `VALOR_COMISSAO_ESP` | `varchar(20)` | sim |
| 28 | `VALOR_COMISSAOCEN` | `varchar(20)` | sim |
| 29 | `_COMISSAO_CEN` | `varchar(20)` | sim |
| 30 | `_COMISSAO_ESPEC` | `varchar(20)` | sim |
| 31 | `COMISSAO_PAGA` | `varchar(3)` | sim |

### IV_Q$COMISSAO_USADO

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `VALOR_DO_EQUIPAMENTO` | `varchar(30)` | sim |
| 19 | `__COMISSAO__RH_` | `varchar(20)` | sim |
| 20 | `VLR_COMISSAO_RH__R__` | `varchar(20)` | sim |
| 21 | `INDICACAO` | `varchar(30)` | sim |

### IV_Q$COMISSAO_VD_LOCACAO

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim |
| 19 | `TIPO_DE_EQUIPAMENTO` | `varchar(30)` | sim |
| 20 | `MARCA` | `varchar(30)` | sim |
| 21 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 22 | `ANO_MODELO_EQUIPAMEN` | `varchar(20)` | sim |
| 23 | `N__SERIE` | `varchar(250)` | sim |
| 24 | `HORIMETRO` | `varchar(250)` | sim |
| 25 | `DATA_INICIO_LOCACAO` | `datetime` | sim |
| 26 | `DATA_TERMINO_LOCACAO` | `datetime` | sim |
| 27 | `VALOR_MENSAL_DO_CONT` | `decimal(14,2)` | sim |
| 28 | `VALOR_TOTAL_DO_CONTR` | `decimal(14,2)` | sim |
| 29 | `DATA_RENOVACAO` | `datetime` | sim |

### IV_Q$COMPETIDORES_NA_NEG

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MARCA` | `varchar(20)` | sim |
| 19 | `REVENDA` | `varchar(30)` | sim |
| 20 | `MODELO_COMP` | `varchar(20)` | sim |
| 21 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 22 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 23 | `CONDICOES_PAGTO_CO` | `varchar(20)` | sim |
| 24 | `CONDICOES_PAGTO_JO` | `varchar(20)` | sim |
| 25 | `EXISTE_COMP` | `varchar(3)` | sim |

### IV_Q$COMPETIDORES_NA_NEGO

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MARCA` | `varchar(20)` | sim |
| 19 | `REVENDA` | `varchar(30)` | sim |
| 20 | `MODELO_COMP` | `varchar(20)` | sim |
| 21 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 22 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 23 | `CONDICOES_PAGTO_CO` | `varchar(20)` | sim |
| 24 | `CONDICOES_PAGTO_JO` | `varchar(20)` | sim |
| 25 | `EXISTE_COMP` | `varchar(3)` | sim |
| 26 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |

### IV_Q$COMPETID_NEGOC_IMPL

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MARCA_IMPL` | `varchar(20)` | sim |
| 19 | `REVENDA_CONC` | `varchar(30)` | sim |
| 20 | `MOD_IMPLEMENTO` | `varchar(60)` | sim |
| 21 | `QTDE_IMPL` | `decimal(14,0)` | sim |
| 22 | `PRECO_CONCORR` | `decimal(14,2)` | sim |
| 23 | `COND_PGTO_CONCORR` | `varchar(20)` | sim |
| 24 | `COND_PGTO_JD` | `varchar(20)` | sim |
| 25 | `COMPETID_NEGOC` | `varchar(3)` | sim |

### IV_Q$COM_INTERESSE_FUTURO

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `INTERESSE_F_TIPO_NEG` | `varchar(20)` | sim |
| 19 | `INTERESSE_F_TIPO_EQ` | `varchar(20)` | sim |
| 20 | `INTERESSE_F_MARCA_EQ` | `varchar(20)` | sim |
| 21 | `INTERESSE_F_MODELO` | `varchar(20)` | sim |
| 22 | `INTERESSE_F_COMPRA` | `varchar(20)` | sim |
| 23 | `INTERESSE_F_NIVEL` | `varchar(20)` | sim |
| 24 | `INTERESSE_F_MOTIVO` | `varchar(20)` | sim |
| 25 | `INTERESSE_F_VALOR` | `decimal(14,2)` | sim |

### IV_Q$CONDICOES_DE_VENDAS

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `COND_TIPO_EQUIP` | `varchar(20)` | sim |
| 19 | `COND_MARCA_EQUIP` | `varchar(20)` | sim |
| 20 | `COND_MODELO_EQUIP` | `varchar(20)` | sim |
| 21 | `COND_QUANTIDADE` | `varchar(20)` | sim |
| 22 | `COND_CONFIGURACAO` | `varchar(100)` | sim |
| 23 | `COND_PRECO` | `decimal(14,2)` | sim |
| 24 | `COND_BONIFICACAO` | `varchar(50)` | sim |
| 25 | `COND_COND_PAGTO` | `varchar(20)` | sim |
| 26 | `COND_COMISSAO` | `decimal(14,2)` | sim |
| 27 | `COND_PRAZO_ENTREGA` | `datetime` | sim |

### IV_Q$CONT_COMISSAO_22

`view` · `34 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VALOR_EQUIPAMENTO_IM` | `decimal(14,2)` | sim |
| 19 | `VALOR_AMS_ADICIONAL` | `decimal(14,2)` | sim |
| 20 | `__COMISSAO__RH_` | `varchar(30)` | sim |
| 21 | `VALOR_COMISSAO___RH` | `decimal(14,2)` | sim |
| 22 | `COMISSAO_PAGA_RH` | `varchar(3)` | sim |
| 23 | `LINHA_DE_EQUIPAMENTO` | `varchar(20)` | sim |
| 24 | `HOUVE_PARTICIPACAO_V` | `varchar(3)` | sim |
| 25 | `ESPECIALISTA_PARTICI` | `varchar(20)` | sim |
| 26 | `POSSUI_ACELERADOR___` | `varchar(3)` | sim |
| 27 | `VALOR_COMISSAO_ESPEC` | `varchar(20)` | sim |
| 28 | `MARGEM_LUCRO_OPERACI` | `varchar(30)` | sim |
| 29 | `VALOR_COMISSAO_CEN` | `decimal(14,2)` | sim |
| 30 | `COMISSAO_ADICIONAL_E` | `decimal(14,2)` | sim |
| 31 | `COMISSAO_AMS_ADICION` | `decimal(14,2)` | sim |
| 32 | `VALOR_PREMIO_ESPECIA` | `decimal(14,2)` | sim |
| 33 | `MARGEM_LUCROOPERACI` | `varchar(30)` | sim |
| 34 | `FORMULARIO_NUMERO` | `varchar(20)` | sim |

### IV_Q$COTA_CONSORCIO

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CONSORCIO_NOME_CLIEN` | `varchar(100)` | sim |
| 19 | `CONSORCIO_LOJA` | `varchar(20)` | sim |
| 20 | `CONSORCIO_CEN` | `varchar(20)` | sim |
| 21 | `CONSORCIO_DATA` | `datetime` | sim |
| 22 | `CONSORCIO_GRUPO` | `varchar(50)` | sim |
| 23 | `CONSORCIO_COTA` | `varchar(50)` | sim |
| 24 | `CONSORCIO_VALOR` | `decimal(14,2)` | sim |

### IV_Q$DEMONSTRACAO_JD

`view` · `49 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CONCESSIONARIO` | `varchar(20)` | sim |
| 19 | `LOJA` | `varchar(20)` | sim |
| 20 | `MODELO_DEMONSTRACAO` | `varchar(40)` | sim |
| 21 | `CHASSI` | `varchar(20)` | sim |
| 22 | `CLIENTE` | `varchar(30)` | sim |
| 23 | `FAZENDA` | `varchar(30)` | sim |
| 24 | `CULTURA` | `varchar(20)` | sim |
| 25 | `DATA_DEMONSTRACAO` | `datetime` | sim |
| 26 | `RESPONSAVEL` | `varchar(30)` | sim |
| 27 | `CELULAR_RESPONSAVEL` | `decimal(14,0)` | sim |
| 28 | `E_MAIL_CLIENTE` | `varchar(40)` | sim |
| 29 | `CNPJ` | `decimal(14,0)` | sim |
| 30 | `OPERACAO` | `varchar(30)` | sim |
| 31 | `IMPLEMENTO_AGRICOLA` | `varchar(300)` | sim |
| 32 | `HORIMETRO_INICIAL` | `decimal(14,0)` | sim |
| 33 | `HORIMETRO_FINAL` | `decimal(14,0)` | sim |
| 34 | `DIESEL_GASTO` | `decimal(14,0)` | sim |
| 35 | `NIVEL_DO_TANQUE` | `decimal(14,0)` | sim |
| 36 | `AREA_TRABALHADA` | `varchar(30)` | sim |
| 37 | `DEPOIMENTO_DO_CLIENT` | `varchar(999)` | sim |
| 38 | `CELULAR_DO_RESPONSAV` | `varchar(30)` | sim |
| 39 | `HORIMETROINICIAL` | `varchar(30)` | sim |
| 40 | `HORIMETROFINAL` | `varchar(30)` | sim |
| 41 | `NIVEL_TANQUE` | `varchar(30)` | sim |
| 42 | `OPERACAO1` | `varchar(20)` | sim |
| 43 | `SERA_NECESSARIO_ENTR` | `varchar(3)` | sim |
| 44 | `CLIENTE_JOHN_DEERE` | `varchar(3)` | sim |
| 45 | `POSSUI_JD` | `varchar(20)` | sim |
| 46 | `ITEM_TECNOLOGIA` | `varchar(3)` | sim |
| 47 | `VALOR_DEMONSTRACAO` | `decimal(14,2)` | sim |
| 48 | `QTD_PRODUTOS_NEG` | `varchar(20)` | sim |
| 49 | `OBJETIVO_DEMO` | `varchar(60)` | sim |

### IV_Q$DEMONSTRACAO_LOG

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VALOR_FRETE__ENTREGA` | `decimal(14,2)` | sim |
| 19 | `VALOR_FRETE_RETORNO` | `decimal(14,2)` | sim |

### IV_Q$DEMONSTRACAO_NF

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_NF` | `decimal(20,0)` | sim |

### IV_Q$DEMONSTRACAO_TRATOR

`view` · `41 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MODELO_DEMO` | `varchar(20)` | sim |
| 19 | `N__SERIE` | `varchar(20)` | sim |
| 20 | `MUNICIPIO` | `varchar(20)` | sim |
| 21 | `DEMONSTRADOR` | `varchar(30)` | sim |
| 22 | `RODAGEM_DIANTEIRA` | `varchar(20)` | sim |
| 23 | `RODAGEM_TRASEIRA` | `varchar(20)` | sim |
| 24 | `LASTREAMENTO` | `varchar(20)` | sim |
| 25 | `TIPO_DE_SERVICO` | `varchar(20)` | sim |
| 26 | `MARCA_DO_IMPLEMENT` | `varchar(20)` | sim |
| 27 | `TIPO_DO_IMPLEMENTO` | `varchar(20)` | sim |
| 28 | `GRUPO_CULTURA` | `varchar(20)` | sim |
| 29 | `TIPO_CULTURA` | `varchar(20)` | sim |
| 30 | `MARCHA_UTILIZADA` | `varchar(30)` | sim |
| 31 | `RPM_DO_MOTOR` | `decimal(14,0)` | sim |
| 32 | `VELOCIDADE_KM_H` | `decimal(14,0)` | sim |
| 33 | `PROFUNDIDADE_DE_CO` | `decimal(14,0)` | sim |
| 34 | `HORAS_TRABALHADAS` | `decimal(14,0)` | sim |
| 35 | `HORA_INICIAL` | `varchar(5)` | sim |
| 36 | `HORA_FINAL` | `varchar(5)` | sim |
| 37 | `DIAS_TRABALHADOS` | `decimal(14,0)` | sim |
| 38 | `OPERADOR` | `varchar(30)` | sim |
| 39 | `LITROS_CONSUMIDOS` | `decimal(14,0)` | sim |
| 40 | `TONELADAS_TRABALHA` | `decimal(14,0)` | sim |
| 41 | `HECTARES_TRABALHAD` | `decimal(14,0)` | sim |

### IV_Q$DEMONSTR_COLHEITAD

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_PLANTADEIRA` | `varchar(20)` | sim |
| 19 | `MODELO_PLANTADEIRA` | `varchar(20)` | sim |
| 20 | `NRO_SERIE` | `varchar(30)` | sim |
| 21 | `DEMONSTRADOR` | `varchar(30)` | sim |
| 22 | `TIPO_PLANTIO` | `varchar(20)` | sim |
| 23 | `MODELO_TRATOR` | `varchar(20)` | sim |
| 24 | `CONFIG_TRATOR` | `varchar(150)` | sim |

### IV_Q$DEMO_EQUIP_JD

`view` · `39 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DEMO_JD_MODELO` | `varchar(20)` | sim |
| 19 | `DEMO_JD_CLIENTE` | `varchar(3)` | sim |
| 20 | `DEMO_JD_USO` | `varchar(20)` | sim |
| 21 | `DEMO_JD_ESP_CSC` | `varchar(3)` | sim |
| 22 | `DEMO_JD_CHASSI` | `varchar(40)` | sim |
| 23 | `DEMO_JD_VALOR_DEMO` | `decimal(14,2)` | sim |
| 24 | `DEMO_JD_QTDE_NEGOC` | `varchar(20)` | sim |
| 25 | `DEMO_JD_OBJ_DEMO` | `varchar(70)` | sim |
| 26 | `DEMO_JD_ENDERECO` | `varchar(60)` | sim |
| 27 | `DEMO_JD_CULTURA` | `varchar(20)` | sim |
| 28 | `DEMO_JD_DATA` | `datetime` | sim |
| 29 | `DEMO_JD_RESPONSAVEL` | `varchar(60)` | sim |
| 30 | `DEMO_JD_CONTATO_RESP` | `varchar(30)` | sim |
| 31 | `DEMO_JD_EMAIL_RESP` | `varchar(50)` | sim |
| 32 | `DEMO_JD_OPERACAO` | `varchar(20)` | sim |
| 33 | `DEMO_JD_IMPLEMENTO` | `varchar(150)` | sim |
| 34 | `DEMO_JD_HORIMET_INIC` | `varchar(30)` | sim |
| 35 | `DEMO_JD_HORIM_FINAL` | `varchar(30)` | sim |
| 36 | `DEMO_JD_DIESEL_GASTO` | `varchar(30)` | sim |
| 37 | `DEMO_JD_NIVEL_TANQUE` | `varchar(30)` | sim |
| 38 | `DEMO_JD_AREA_TRAB` | `varchar(30)` | sim |
| 39 | `DEMO_JD_DEPOIMENTO` | `varchar(300)` | sim |

### IV_Q$DEMO_IMPLEMENTO

`view` · `41 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MODELO_DEMO` | `varchar(20)` | sim |
| 19 | `N__SERIE` | `varchar(20)` | sim |
| 20 | `MUNICIPIO` | `varchar(20)` | sim |
| 21 | `DEMONSTRADOR` | `varchar(30)` | sim |
| 22 | `RODAGEM_DIANTEIRA` | `varchar(20)` | sim |
| 23 | `RODAGEM_TRASEIRA` | `varchar(20)` | sim |
| 24 | `LASTREAMENTO` | `varchar(20)` | sim |
| 25 | `TIPO_DE_SERVICO` | `varchar(20)` | sim |
| 26 | `MARCA_DO_IMPLEMENT` | `varchar(20)` | sim |
| 27 | `TIPO_DO_IMPLEMENTO` | `varchar(20)` | sim |
| 28 | `GRUPO_CULTURA` | `varchar(20)` | sim |
| 29 | `TIPO_CULTURA` | `varchar(20)` | sim |
| 30 | `MARCHA_UTILIZADA` | `varchar(30)` | sim |
| 31 | `RPM_DO_MOTOR` | `decimal(14,0)` | sim |
| 32 | `VELOCIDADE_KM_H` | `decimal(14,0)` | sim |
| 33 | `PROFUNDIDADE_DE_CO` | `decimal(14,0)` | sim |
| 34 | `HORAS_TRABALHADAS` | `decimal(14,0)` | sim |
| 35 | `HORA_INICIAL` | `varchar(5)` | sim |
| 36 | `HORA_FINAL` | `varchar(5)` | sim |
| 37 | `DIAS_TRABALHADOS` | `decimal(14,0)` | sim |
| 38 | `OPERADOR` | `varchar(30)` | sim |
| 39 | `LITROS_CONSUMIDOS` | `decimal(14,0)` | sim |
| 40 | `TONELADAS_TRABALHA` | `decimal(14,0)` | sim |
| 41 | `HECTARES_TRABALHAD` | `decimal(14,0)` | sim |

### IV_Q$DEMO_MAQUINAS

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `ENDERECO_ENTREGA` | `varchar(600)` | sim |
| 19 | `TIPO_EQUIP` | `varchar(20)` | sim |
| 20 | `OPERACAO_DEMO` | `varchar(20)` | sim |
| 21 | `DATA_IDEAL` | `datetime` | sim |
| 22 | `APROVAR_GERENTE` | `datetime` | sim |
| 23 | `CULTURAS` | `varchar(20)` | sim |
| 24 | `AREA_TRABALHADA` | `decimal(14,0)` | sim |
| 25 | `POTENCIAL_DE_COMPRA` | `varchar(20)` | sim |
| 26 | `NF_DEMONSTRACAO` | `decimal(14,0)` | sim |
| 27 | `RESULTADAO_DEMONSTRA` | `varchar(500)` | sim |
| 28 | `MODELO_EQUIPA` | `varchar(20)` | sim |
| 29 | `DATA_AGENDADA` | `datetime` | sim |
| 30 | `DATA_REALIZADA` | `datetime` | sim |
| 31 | `TIPO_EQUIPAMENTO` | `varchar(30)` | sim |
| 32 | `DATA_NOTA` | `datetime` | sim |

### IV_Q$DEMO_TRATOR

`view` · `42 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MODELO_DEMO` | `varchar(20)` | sim |
| 19 | `N__SERIE` | `varchar(20)` | sim |
| 20 | `MUNICIPIO` | `varchar(20)` | sim |
| 21 | `DEMONSTRADOR` | `varchar(30)` | sim |
| 22 | `RODAGEM_DIANTEIRA` | `varchar(20)` | sim |
| 23 | `RODAGEM_TRASEIRA` | `varchar(20)` | sim |
| 24 | `LASTREAMENTO` | `varchar(20)` | sim |
| 25 | `TIPO_DE_SERVICO` | `varchar(20)` | sim |
| 26 | `MARCA_DO_IMPLEMENT` | `varchar(20)` | sim |
| 27 | `TIPO_DO_IMPLEMENTO` | `varchar(20)` | sim |
| 28 | `GRUPO_CULTURA` | `varchar(20)` | sim |
| 29 | `TIPO_CULTURA` | `varchar(20)` | sim |
| 30 | `MARCHA_UTILIZADA` | `varchar(30)` | sim |
| 31 | `RPM_DO_MOTOR` | `decimal(14,0)` | sim |
| 32 | `VELOCIDADE_KM_H` | `decimal(14,0)` | sim |
| 33 | `PROFUNDIDADE_DE_CO` | `decimal(14,0)` | sim |
| 34 | `HORAS_TRABALHADAS` | `decimal(14,0)` | sim |
| 35 | `HORA_INICIAL` | `varchar(5)` | sim |
| 36 | `HORA_FINAL` | `varchar(5)` | sim |
| 37 | `DIAS_TRABALHADOS` | `decimal(14,0)` | sim |
| 38 | `OPERADOR` | `varchar(30)` | sim |
| 39 | `LITROS_CONSUMIDOS` | `decimal(14,0)` | sim |
| 40 | `TONELADAS_TRABALHA` | `decimal(14,0)` | sim |
| 41 | `HECTARES_TRABALHAD` | `decimal(14,0)` | sim |
| 42 | `TIPO_DE_MAQUINA` | `varchar(20)` | sim |

### IV_Q$DEVOLUCAO_PECA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_NF` | `varchar(30)` | sim |

### IV_Q$DEVOLUCAO_PUK

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NF_DEVOLUCAO` | `decimal(14,0)` | sim |

### IV_Q$DOC_ANALISE_CREDITO

`view` · `44 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(500)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_DE_CLIENTE` | `varchar(20)` | sim |
| 19 | `Q002_COPIA_ULTIMO` | `decimal(1,0)` | sim |
| 20 | `Q002_D_R_E_DEMONS` | `decimal(1,0)` | sim |
| 21 | `Q002_FATUR` | `decimal(1,0)` | sim |
| 22 | `Q002_CONT_SOCIAL` | `decimal(1,0)` | sim |
| 23 | `Q002_COMPROV_END` | `decimal(1,0)` | sim |
| 24 | `Q002_TELEFONE` | `decimal(1,0)` | sim |
| 25 | `Q002_EMAIL7` | `decimal(1,0)` | sim |
| 26 | `Q002_TRES_REFERENCIA` | `decimal(1,0)` | sim |
| 27 | `Q002_REF_BANC` | `decimal(1,0)` | sim |
| 28 | `Q003_DECA3` | `decimal(1,0)` | sim |
| 29 | `Q003_IR3` | `decimal(1,0)` | sim |
| 30 | `Q003_CPF3` | `decimal(1,0)` | sim |
| 31 | `Q003_RG3` | `decimal(1,0)` | sim |
| 32 | `Q003_COPIA_COMPROVA3` | `decimal(1,0)` | sim |
| 33 | `Q003_FONE3` | `decimal(1,0)` | sim |
| 34 | `Q003_EMAIL3` | `decimal(1,0)` | sim |
| 35 | `Q003_TRES_COM3` | `decimal(1,0)` | sim |
| 36 | `Q003_TRES_BANC3` | `decimal(1,0)` | sim |
| 37 | `Q004_CPF4` | `decimal(1,0)` | sim |
| 38 | `Q004_RG4` | `decimal(1,0)` | sim |
| 39 | `Q004_COPIA_COMPROVAN` | `decimal(1,0)` | sim |
| 40 | `Q004_COPIA_DO_HOLERI` | `decimal(1,0)` | sim |
| 41 | `Q004_FONE4` | `decimal(1,0)` | sim |
| 42 | `Q004_EMAIL4` | `decimal(1,0)` | sim |
| 43 | `Q004_TRES_COM4` | `decimal(1,0)` | sim |
| 44 | `Q004_TRES_BANC4` | `decimal(1,0)` | sim |

### IV_Q$EVENTOS_AFERICAO

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `QUAL_EVENTO` | `varchar(20)` | sim |
| 19 | `O_SR_A___GOSTOU_DO_N` | `varchar(3)` | sim |
| 20 | `COMO_O_SR_A___SOUBE_` | `varchar(30)` | sim |
| 21 | `TEM_ALGUMA_SUGESTAO_` | `varchar(30)` | sim |
| 22 | `COM_BASE_NO_ULTIMO_E` | `varchar(3)` | sim |
| 23 | `DE_MODO_GERAL_A_FEIR` | `varchar(3)` | sim |
| 24 | `TEM_UMA_VISITA_AGEND` | `varchar(3)` | sim |
| 25 | `DESEJA_AGENDAR_UMA_V` | `varchar(3)` | sim |
| 26 | `A_DATA_E_DURACAO_DO_` | `varchar(3)` | sim |
| 27 | `O_HORARIO_FOI_CONVEN` | `varchar(3)` | sim |
| 28 | `DE_FORMA_GERAL__COMO` | `varchar(20)` | sim |

### IV_Q$EXP_FLUXO_MODELER

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO` | `decimal(14,0)` | sim |
| 19 | `NOME` | `varchar(30)` | sim |

### IV_Q$FORA_SERVICO_PMP

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CHASSI` | `varchar(30)` | sim |
| 19 | `DATA_DO_EVENTO` | `datetime` | sim |
| 20 | `DESCRICAO_DANO` | `varchar(300)` | sim |
| 21 | `CAUSA_DANO` | `varchar(300)` | sim |

### IV_Q$FORM_TREINO

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |

### IV_Q$GAR_DATA_SERVICO

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `INICIO_SERVICO` | `datetime` | sim |
| 19 | `TERMINO_SERVICO` | `datetime` | sim |

### IV_Q$GAR_FAB_SOL_PECA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_OS` | `varchar(30)` | sim |

### IV_Q$GESTAO_CREDITO

`view` · `70 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TAXTAXA_FLAT_FLAT` | `varchar(30)` | sim |
| 19 | `STATUS_ANALISE_BANCO` | `varchar(30)` | sim |
| 20 | `NF_DESCONTO_INCONDIC` | `varchar(30)` | sim |
| 21 | `CODIGO_MDA1` | `varchar(10)` | sim |
| 22 | `COM_CONTRATOR_GFC` | `varchar(3)` | sim |
| 23 | `FORMULARIO_NUMERO` | `varchar(20)` | sim |
| 24 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 25 | `VALOR_DO_SINAL` | `decimal(14,2)` | sim |
| 26 | `DATA_SINAL` | `datetime` | sim |
| 27 | `RECEBIMENTO_USADO` | `varchar(20)` | sim |
| 28 | `MEU_PRIMEIRO_JD` | `varchar(3)` | sim |
| 29 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 30 | `VALOR_USADO` | `decimal(14,2)` | sim |
| 31 | `VALOR_AVALIACAO_USAD` | `decimal(14,2)` | sim |
| 32 | `BONIFICACAO_DESCONTO` | `decimal(14,2)` | sim |
| 33 | `TIPO_BONIFICACAO_DES` | `varchar(25)` | sim |
| 34 | `VALOR_DESCONTO_INCON` | `decimal(14,2)` | sim |
| 35 | `DETALHES_DESCONTO_IN` | `varchar(60)` | sim |
| 36 | `SINAL_EMSINAL_EMBUTI` | `decimal(14,2)` | sim |
| 37 | `FORMA_PAGAMENTO` | `varchar(20)` | sim |
| 38 | `RECEBIMENTO_DO_EQUIP` | `varchar(3)` | sim |
| 39 | `CODIGO_FINAME` | `varchar(12)` | sim |
| 40 | `INFORMACAO_DE_FATURA` | `varchar(500)` | sim |
| 41 | `INF_FINANCEIRO` | `varchar(500)` | sim |
| 42 | `CODIGO_MDA` | `decimal(14,0)` | sim |
| 43 | `FORMULARIO_CANCELADO` | `varchar(3)` | sim |
| 44 | `TRATOR_NUMERO` | `decimal(14,0)` | sim |
| 45 | `NUMERO_AMS` | `varchar(20)` | sim |
| 46 | `NUMERO_IMP` | `varchar(20)` | sim |
| 47 | `NUMERO` | `varchar(20)` | sim |
| 48 | `INSTITUICAO_FIINSTIT` | `varchar(20)` | sim |
| 49 | `PROCESSO_STATUS` | `varchar(20)` | sim |
| 50 | `NF_REFATURAMENTO` | `decimal(14,0)` | sim |
| 51 | `COM_CONTRATO` | `varchar(3)` | sim |
| 52 | `DADATA_RESPOSTATA_RE` | `datetime` | sim |
| 53 | `DATA_RESPOSTA__1_` | `datetime` | sim |
| 54 | `DATA_RESPOSTA__2_` | `datetime` | sim |
| 55 | `TAXA_FLAT` | `varchar(6)` | sim |
| 56 | `ANALISE_BC_3` | `varchar(20)` | sim |
| 57 | `DATA_ENVIO__2_` | `datetime` | sim |
| 58 | `DATA_ENVIO__3_` | `datetime` | sim |
| 59 | `LOCAL_FATURAMENTO` | `varchar(21)` | sim |
| 60 | `FATURAMENTO_REALIZAD` | `datetime` | sim |
| 61 | `NOTA_FISCAL` | `decimal(14,0)` | sim |
| 62 | `INSTITUICAO_FINANCEI` | `varchar(20)` | sim |
| 63 | `ANALISE_3` | `varchar(20)` | sim |
| 64 | `STATUS_BC_2` | `varchar(20)` | sim |
| 65 | `GESTOR___TELEFONE___` | `varchar(100)` | sim |
| 66 | `STATUS_ANALISE_BC` | `varchar(20)` | sim |
| 67 | `DATA_ENVIO_DATA_ENVI` | `datetime` | sim |
| 68 | `ANALISE_2` | `varchar(20)` | sim |
| 69 | `STATUS_PROCESSO` | `varchar(20)` | sim |
| 70 | `PROBABILIDADE` | `varchar(20)` | sim |

### IV_Q$GESTAO_CREDITO_AMS

`view` · `63 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TAXTAXA_FLAT_FLAT` | `varchar(30)` | sim |
| 19 | `STATUS_ANALISE_BANCO` | `varchar(30)` | sim |
| 20 | `NF_DESCONTO_INCONDIC` | `varchar(30)` | sim |
| 21 | `FORMULARIO_NUMERO` | `varchar(20)` | sim |
| 22 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 23 | `VALOR_DO_SINAL` | `decimal(14,2)` | sim |
| 24 | `DATA_SINAL` | `datetime` | sim |
| 25 | `RECEBIMENTO_USADO` | `varchar(20)` | sim |
| 26 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 27 | `VALOR_USADO` | `decimal(14,2)` | sim |
| 28 | `VALOR_AVALIACAO_USAD` | `decimal(14,2)` | sim |
| 29 | `BONIFICACAO_DESCONTO` | `decimal(14,2)` | sim |
| 30 | `TIPO_BONIFICACAO_DES` | `varchar(25)` | sim |
| 31 | `VALOR_DESCONTO_INCON` | `decimal(14,2)` | sim |
| 32 | `DETALHES_DESCONTO_IN` | `varchar(60)` | sim |
| 33 | `SINAL_EMSINAL_EMBUTI` | `decimal(14,2)` | sim |
| 34 | `FORMA_PAGAMENTO` | `varchar(20)` | sim |
| 35 | `CODIGO_FINAME` | `varchar(12)` | sim |
| 36 | `INF_FINANCEIRO` | `varchar(500)` | sim |
| 37 | `CODIGO_MDA` | `decimal(14,0)` | sim |
| 38 | `NUMERO_AMS` | `varchar(20)` | sim |
| 39 | `NUMERO_IMP` | `varchar(20)` | sim |
| 40 | `NUMERO` | `varchar(20)` | sim |
| 41 | `INSTITUICAO_FIINSTIT` | `varchar(20)` | sim |
| 42 | `PROCESSO_STATUS` | `varchar(20)` | sim |
| 43 | `NF_REFATURAMENTO` | `decimal(14,0)` | sim |
| 44 | `DADATA_RESPOSTATA_RE` | `datetime` | sim |
| 45 | `DATA_RESPOSTA__1_` | `datetime` | sim |
| 46 | `DATA_RESPOSTA__2_` | `datetime` | sim |
| 47 | `TAXA_FLAT` | `varchar(6)` | sim |
| 48 | `ANALISE_BC_3` | `varchar(20)` | sim |
| 49 | `DATA_ENVIO__2_` | `datetime` | sim |
| 50 | `DATA_ENVIO__3_` | `datetime` | sim |
| 51 | `LOCAL_FATURAMENTO` | `varchar(21)` | sim |
| 52 | `FATURAMENTO_REALIZAD` | `datetime` | sim |
| 53 | `NOTA_FISCAL` | `decimal(14,0)` | sim |
| 54 | `INSTITUICAO_FINANCEI` | `varchar(20)` | sim |
| 55 | `ANALISE_3` | `varchar(20)` | sim |
| 56 | `STATUS_BC_2` | `varchar(20)` | sim |
| 57 | `GESTOR___TELEFONE___` | `varchar(100)` | sim |
| 58 | `STATUS_ANALISE_BC` | `varchar(20)` | sim |
| 59 | `DATA_ENVIO_DATA_ENVI` | `datetime` | sim |
| 60 | `ANALISE_2` | `varchar(20)` | sim |
| 61 | `STATUS_PROCESSO` | `varchar(20)` | sim |
| 62 | `PROBABILIDADE` | `varchar(20)` | sim |
| 63 | `COM_CONTRATO` | `varchar(3)` | sim |

### IV_Q$GESTAO_CREDITO_IMP

`view` · `64 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TAXTAXA_FLAT_FLAT` | `varchar(30)` | sim |
| 19 | `STATUS_ANALISE_BANCO` | `varchar(30)` | sim |
| 20 | `NF_DESCONTO_INCONDIC` | `varchar(30)` | sim |
| 21 | `FORMULARIO_NUMERO` | `varchar(20)` | sim |
| 22 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 23 | `VALOR_DO_SINAL` | `decimal(14,2)` | sim |
| 24 | `DATA_SINAL` | `datetime` | sim |
| 25 | `RECEBIMENTO_USADO` | `varchar(20)` | sim |
| 26 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 27 | `VALOR_USADO` | `decimal(14,2)` | sim |
| 28 | `VALOR_AVALIACAO_USAD` | `decimal(14,2)` | sim |
| 29 | `BONIFICACAO_DESCONTO` | `decimal(14,2)` | sim |
| 30 | `TIPO_BONIFICACAO_DES` | `varchar(25)` | sim |
| 31 | `VALOR_DESCONTO_INCON` | `decimal(14,2)` | sim |
| 32 | `DETALHES_DESCONTO_IN` | `varchar(60)` | sim |
| 33 | `SINAL_EMSINAL_EMBUTI` | `decimal(14,2)` | sim |
| 34 | `FORMA_PAGAMENTO` | `varchar(20)` | sim |
| 35 | `CODIGO_FINAME` | `varchar(12)` | sim |
| 36 | `INF_FINANCEIRO` | `varchar(500)` | sim |
| 37 | `CODIGO_MDA` | `decimal(14,0)` | sim |
| 38 | `NUMERO_AMS` | `varchar(20)` | sim |
| 39 | `NUMERO_IMP` | `varchar(20)` | sim |
| 40 | `NUMERO` | `varchar(20)` | sim |
| 41 | `INSTITUICAO_FIINSTIT` | `varchar(20)` | sim |
| 42 | `PROCESSO_STATUS` | `varchar(20)` | sim |
| 43 | `NF_REFATURAMENTO` | `decimal(14,0)` | sim |
| 44 | `DADATA_RESPOSTATA_RE` | `datetime` | sim |
| 45 | `DATA_RESPOSTA__1_` | `datetime` | sim |
| 46 | `DATA_RESPOSTA__2_` | `datetime` | sim |
| 47 | `TAXA_FLAT` | `varchar(6)` | sim |
| 48 | `ANALISE_BC_3` | `varchar(20)` | sim |
| 49 | `DATA_ENVIO__2_` | `datetime` | sim |
| 50 | `DATA_ENVIO__3_` | `datetime` | sim |
| 51 | `LOCAL_FATURAMENTO` | `varchar(21)` | sim |
| 52 | `FATURAMENTO_REALIZAD` | `datetime` | sim |
| 53 | `NOTA_FISCAL` | `decimal(14,0)` | sim |
| 54 | `INSTITUICAO_FINANCEI` | `varchar(20)` | sim |
| 55 | `ANALISE_3` | `varchar(20)` | sim |
| 56 | `STATUS_BC_2` | `varchar(20)` | sim |
| 57 | `GESTOR___TELEFONE___` | `varchar(100)` | sim |
| 58 | `STATUS_ANALISE_BC` | `varchar(20)` | sim |
| 59 | `DATA_ENVIO_DATA_ENVI` | `datetime` | sim |
| 60 | `ANALISE_2` | `varchar(20)` | sim |
| 61 | `STATUS_PROCESSO` | `varchar(20)` | sim |
| 62 | `PROBABILIDADE` | `varchar(20)` | sim |
| 63 | `COM_CONTRATO` | `varchar(3)` | sim |
| 64 | `RECEBIMENTO_DO_EQUIP` | `varchar(3)` | sim |

### IV_Q$GESTAO_PRODUTO_IMPL

`view` · `38 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `FORMULARIO_NFORMULAR` | `varchar(20)` | sim |
| 19 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 20 | `MARCA` | `varchar(20)` | sim |
| 21 | `MODELO_IMPLEMENTO` | `varchar(25)` | sim |
| 22 | `DESCRICAO_DETALHADA_` | `varchar(2000)` | sim |
| 23 | `COR_IMPLEMENTO` | `varchar(20)` | sim |
| 24 | `ANO_FABRICACAO` | `decimal(14,0)` | sim |
| 25 | `INFORMACAO_PREPARACA` | `varchar(2000)` | sim |
| 26 | `PRODUTO_NOVO_OU_USAD` | `varchar(20)` | sim |
| 27 | `NUMERO` | `varchar(30)` | sim |
| 28 | `NUMERO1` | `decimal(14,0)` | sim |
| 29 | `CHASSI` | `varchar(30)` | sim |
| 30 | `VENDA_DO_IMPLEMENTO` | `varchar(20)` | sim |
| 31 | `DESCONTO_INCONDICION` | `varchar(3)` | sim |
| 32 | `DATA_ESPERADA_PARA_E` | `datetime` | sim |
| 33 | `DATA_DE_USO_PARA_SAZ` | `datetime` | sim |
| 34 | `PREVISAO_ENTREGA_PRE` | `datetime` | sim |
| 35 | `DATA_ENTREGA` | `datetime` | sim |
| 36 | `TRANSPORTADORA` | `varchar(30)` | sim |
| 37 | `VALOR_FRETE` | `decimal(14,2)` | sim |
| 38 | `TRANSPORTE_ENTREGA` | `varchar(30)` | sim |

### IV_Q$GESTAO_PRODUTO___AMS

`view` · `45 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `ITENS_AMS` | `varchar(40)` | sim |
| 19 | `COM_INSTALACAO_AMS` | `varchar(3)` | sim |
| 20 | `ITENS_A_AGREGAR` | `varchar(200)` | sim |
| 21 | `ITENS_A_DESAGREGAR` | `varchar(200)` | sim |
| 22 | `ORDEM_SERVICO_AGREGA` | `varchar(60)` | sim |
| 23 | `QUANTIDADE_ITENS` | `varchar(20)` | sim |
| 24 | `CHASSI_AMS___1` | `varchar(30)` | sim |
| 25 | `CHASSI_AMS___2` | `varchar(30)` | sim |
| 26 | `CHASSI_AMS___3` | `varchar(30)` | sim |
| 27 | `CHASSI_AMS___4` | `varchar(30)` | sim |
| 28 | `CHASSI_AMS___5` | `varchar(30)` | sim |
| 29 | `CHASSI_AMS___6` | `varchar(30)` | sim |
| 30 | `CHASSI_AMS___7` | `varchar(30)` | sim |
| 31 | `CHASSI_AMS___8` | `varchar(30)` | sim |
| 32 | `CHASSI_AMS___9` | `varchar(30)` | sim |
| 33 | `CHASSI_AMS___10` | `varchar(30)` | sim |
| 34 | `COMAR` | `varchar(100)` | sim |
| 35 | `PRODUTO_NOVO_OU_USAD` | `varchar(20)` | sim |
| 36 | `FORMULARIO_NUMERO` | `varchar(20)` | sim |
| 37 | `NUMERO` | `decimal(14,0)` | sim |
| 38 | `DESCONTO_INCONDICION` | `varchar(3)` | sim |
| 39 | `DATA_ESPERADA_PARA_E` | `datetime` | sim |
| 40 | `DATA_DE_USO_PARA_SAZ` | `datetime` | sim |
| 41 | `PREVISAO_ENTREGA_PRE` | `datetime` | sim |
| 42 | `DATA_ENTREGA` | `datetime` | sim |
| 43 | `TRANSPORTADORA` | `varchar(30)` | sim |
| 44 | `VALOR_FRETE` | `decimal(14,2)` | sim |
| 45 | `TRANSPORTE_ENTREGA` | `varchar(30)` | sim |

### IV_Q$HORIMETRO_AGREGA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `HORIMETRO` | `varchar(30)` | sim |

### IV_Q$INCENTIVO

`view` · `59 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `PERC_INCENTIVO` | `decimal(14,0)` | sim |
| 19 | `NRO_COTACAO_ALCADA` | `decimal(14,0)` | sim |
| 20 | `BASE_CALCULO` | `decimal(14,2)` | sim |
| 21 | `TOTAL_BASE_ALCADA` | `decimal(14,2)` | sim |
| 22 | `MARCAR_ENTREGUE` | `datetime` | sim |
| 23 | `MARCAR_VENDIDO` | `datetime` | sim |
| 24 | `OUTROS_INCENTIVOS` | `varchar(3)` | sim |
| 25 | `NRO_COTACAO` | `decimal(14,0)` | sim |
| 26 | `BASE_OUTROS` | `decimal(14,2)` | sim |
| 27 | `CAMPANHA1` | `varchar(50)` | sim |
| 28 | `PERC_CMP1` | `decimal(14,0)` | sim |
| 29 | `CAMPANHA2` | `varchar(50)` | sim |
| 30 | `PERC_CMP2` | `decimal(14,0)` | sim |
| 31 | `CAMPANHA3` | `varchar(50)` | sim |
| 32 | `PERC_CMP3` | `decimal(14,0)` | sim |
| 33 | `CAMPANHA1JD` | `varchar(50)` | sim |
| 34 | `CMP_1JD` | `decimal(14,0)` | sim |
| 35 | `TOTAL_BRUTO` | `decimal(14,2)` | sim |
| 36 | `DATA_ENVIO_XML` | `datetime` | sim |
| 37 | `NRO_NF` | `decimal(14,0)` | sim |
| 38 | `DATA_EMISSAO` | `datetime` | sim |
| 39 | `TOTAL_LIQUIDO` | `decimal(14,2)` | sim |
| 40 | `COMPRAR_VENDIDO` | `varchar(3)` | sim |
| 41 | `NRO_NF_ALCADA` | `decimal(14,0)` | sim |
| 42 | `NRO_PROC_ALCADA` | `decimal(14,0)` | sim |
| 43 | `DTA_NF_ALCADA` | `datetime` | sim |
| 44 | `TIPO_CAMPANHA_1` | `varchar(20)` | sim |
| 45 | `TIPO_CAMPANHA_2` | `varchar(20)` | sim |
| 46 | `TIPO_CAMPANHA_3` | `varchar(20)` | sim |
| 47 | `TIPO_CAMPANHA_DE_INC` | `varchar(20)` | sim |
| 48 | `NOME_CAMPANHA_DE_INC` | `varchar(20)` | sim |
| 49 | `TIPO_INCENTIVO_2` | `varchar(20)` | sim |
| 50 | `NOME_INCENTIVO_2` | `varchar(20)` | sim |
| 51 | `TIPO_INCENTIVO_3` | `varchar(20)` | sim |
| 52 | `NOME_INCENTIVO_3` | `varchar(20)` | sim |
| 53 | `TIPO_INCENTIVO_4` | `varchar(20)` | sim |
| 54 | `NOME_INCENTIVO_4` | `varchar(20)` | sim |
| 55 | `__CAMPANHA_4` | `decimal(14,0)` | sim |
| 56 | `_CAMPANHA1` | `decimal(14,2)` | sim |
| 57 | `_CAMPANHA2` | `decimal(14,2)` | sim |
| 58 | `_CAMPANHA3` | `decimal(14,2)` | sim |
| 59 | `_CAMPANHA4` | `decimal(14,2)` | sim |

### IV_Q$INTERESSE_FUTURO_PRO

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |

### IV_Q$INTERESSE_PROJETO_IR

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CULTURA` | `varchar(20)` | sim |
| 19 | `AREA_HA` | `varchar(30)` | sim |
| 20 | `LANCES` | `varchar(30)` | sim |
| 21 | `STATUS` | `varchar(20)` | sim |
| 22 | `PROBABILIDADE_FECHAM` | `varchar(20)` | sim |
| 23 | `VALOR` | `decimal(14,2)` | sim |
| 24 | `DATA_FATURAMENTO` | `datetime` | sim |
| 25 | `VENDEDOR_INDICACAO` | `varchar(20)` | sim |

### IV_Q$LIBERAR_DEMONSTRACAO

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `LOJA_PRODUTO` | `varchar(25)` | sim |
| 19 | `CHASSIS` | `varchar(30)` | sim |
| 20 | `DATA_IDA` | `datetime` | sim |
| 21 | `DATA_VOLTA` | `datetime` | sim |

### IV_Q$LICENCAS_PUK

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `CHASSI_MAQUINA` | `varchar(30)` | sim |
| 19 | `LICENCA_RECEPTOR` | `varchar(35)` | sim |
| 20 | `CHASSI_RECEPTOR` | `varchar(30)` | sim |
| 21 | `VALOR_RECEPTOR` | `decimal(14,2)` | sim |
| 22 | `CODIGO_RECEPTOR` | `varchar(30)` | sim |
| 23 | `LICENCA_MONITOR` | `varchar(35)` | sim |
| 24 | `CHASSI_MONITOR` | `varchar(30)` | sim |
| 25 | `VALOR_MONITOR` | `decimal(14,2)` | sim |
| 26 | `CODIGO_MONITOR` | `varchar(30)` | sim |
| 27 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 28 | `FORMA_PAGAMENTO` | `varchar(20)` | sim |

### IV_Q$LOCACAO_COMISSAO

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `QUANTIDADE_DE_EQUIPA` | `decimal(14,0)` | sim |
| 19 | `TIPO_DE_EQUIPAMENTOS` | `varchar(30)` | sim |
| 20 | `MARCA` | `varchar(30)` | sim |
| 21 | `MODELO_DO_EQUIPAMENT` | `varchar(30)` | sim |
| 22 | `ANO_MODELO_EQUIPAMEN` | `varchar(10)` | sim |
| 23 | `N__SERIE` | `varchar(400)` | sim |
| 24 | `HORIMETRO` | `varchar(250)` | sim |
| 25 | `DATA_INICIO_LOCACAO` | `datetime` | sim |
| 26 | `DATA_TERMINO_LOCACAO` | `datetime` | sim |
| 27 | `VALOR_MENSAL_DO_CONT` | `decimal(14,2)` | sim |
| 28 | `DATA_RENOVACAO` | `datetime` | sim |
| 29 | `VALOR_TOTAL_DO_CONTR` | `decimal(14,2)` | sim |

### IV_Q$OFERECE_RENOV_SEGURO

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SEGURADORA` | `varchar(20)` | sim |
| 19 | `CORRETORA` | `varchar(20)` | sim |
| 20 | `APOLICE` | `varchar(30)` | sim |
| 21 | `VIGENCIA_INICIO` | `datetime` | sim |
| 22 | `VIGENCIA_TERMINO` | `datetime` | sim |
| 23 | `PREMIO_LIQUIDO` | `decimal(14,2)` | sim |
| 24 | `QDE_PARCELAS` | `decimal(14,0)` | sim |
| 25 | `VALOR_PARCELA` | `decimal(14,2)` | sim |
| 26 | `DATA_VENCTO_PRIMEIRA` | `datetime` | sim |
| 27 | `PART_PORCENT` | `varchar(5)` | sim |
| 28 | `NUM_PARCELA` | `decimal(14,0)` | sim |
| 29 | `VALOR_PAGO` | `decimal(14,2)` | sim |
| 30 | `HOUVE_SINISTRO` | `varchar(3)` | sim |
| 31 | `DATA_SINISTRO_UM` | `datetime` | sim |
| 32 | `DATA_SINISTRO_DOIS` | `datetime` | sim |

### IV_Q$ORIGEM_DA_RENDA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `ORIGEM_D_RENDA` | `varchar(20)` | sim |

### IV_Q$OS_ABERTA

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_DA_OS` | `varchar(30)` | sim |
| 19 | `DATA_ABERTURA_OS` | `datetime` | sim |

### IV_Q$OS_CORTESIA

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_OS` | `varchar(30)` | sim |
| 19 | `DATA_ABERTURA_OS` | `datetime` | sim |
| 20 | `TIPO_GARANTIA` | `varchar(40)` | sim |
| 21 | `CHASSI` | `varchar(30)` | sim |

### IV_Q$OS_GARANTIA

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_OS` | `varchar(30)` | sim |
| 19 | `DATA_ABERTURA_OS` | `datetime` | sim |
| 20 | `CHASSI` | `varchar(30)` | sim |
| 21 | `TIPO_GARANTIA` | `varchar(40)` | sim |

### IV_Q$OS_REVISAO_ENTREGA

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_OS` | `decimal(14,0)` | sim |
| 19 | `NUMERO_DO_CHASSI` | `varchar(50)` | sim |
| 20 | `HORIMETRO` | `decimal(14,2)` | sim |
| 21 | `TIPO_REVISAO_ENTREGA` | `varchar(20)` | sim |

### IV_Q$PECAS_AFERICAO

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFACAO_EM_RELACA` | `varchar(20)` | sim |
| 19 | `QUAL_SETOR_HA_OPORTU` | `varchar(20)` | sim |
| 20 | `SATISFACAO_AS_ULTIMA` | `varchar(3)` | sim |
| 21 | `RECOMENDA_A_COLORADO` | `varchar(3)` | sim |
| 22 | `O_SR__INDICA_UM_AMIG` | `varchar(100)` | sim |
| 23 | `QUAL_COLABORADOR_SE_` | `varchar(30)` | sim |
| 24 | `RECOMENDA` | `varchar(2)` | sim |
| 25 | `SATISFACAO` | `varchar(3)` | sim |
| 26 | `ACORDADO` | `varchar(3)` | sim |
| 27 | `PRODUTO` | `varchar(3)` | sim |
| 28 | `SUGESTAO` | `varchar(30)` | sim |
| 29 | `NOTA_NPS` | `varchar(20)` | sim |

### IV_Q$PEDIDO_GC

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `GC_TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 19 | `GC_MODELO_EQUIPAMENT` | `varchar(20)` | sim |
| 20 | `GC_PRECO_UNITARIO` | `decimal(14,2)` | sim |
| 21 | `GC_QUANTIDADE` | `decimal(10,0)` | sim |
| 22 | `GC_PRECO_TOTAL` | `decimal(14,2)` | sim |
| 23 | `GC_ENTREGA` | `datetime` | sim |
| 24 | `GC_BONIFICACAO` | `varchar(3)` | sim |
| 25 | `GC_ITENS_BONIFICADOS` | `varchar(200)` | sim |
| 26 | `GC_COMISSAO` | `varchar(30)` | sim |
| 27 | `GC_TECNOLOGIA` | `varchar(3)` | sim |
| 28 | `GC_ITENS_TECNOLOGIA` | `varchar(300)` | sim |

### IV_Q$PEDIDO_KAM

`view` · `44 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MODELO_DO_PRODUTO` | `varchar(40)` | sim |
| 19 | `QUANTIDADE` | `varchar(20)` | sim |
| 20 | `PRECO` | `decimal(14,2)` | sim |
| 21 | `COTACAO_MODELO` | `decimal(15,0)` | sim |
| 22 | `CNPJ_CPF` | `decimal(18,0)` | sim |
| 23 | `INSCRICAO_ESTADUAL` | `decimal(9,0)` | sim |
| 24 | `CONDICAO_DE_PAGAMENT` | `varchar(30)` | sim |
| 25 | `PRAZO_DE_ENTREGA` | `datetime` | sim |
| 26 | `COMISSAO` | `decimal(10,0)` | sim |
| 27 | `SINAL` | `decimal(14,2)` | sim |
| 28 | `_SINAL` | `decimal(10,0)` | sim |
| 29 | `DATA_VENCIMENTO_SINA` | `datetime` | sim |
| 30 | `SALDO` | `varchar(40)` | sim |
| 31 | `BANCO` | `varchar(30)` | sim |
| 32 | `BONIFICACAO` | `varchar(40)` | sim |
| 33 | `NOME_TEST_CONC` | `varchar(50)` | sim |
| 34 | `EMAIL_TEST_CONC` | `varchar(64)` | sim |
| 35 | `CPF_TEST_CONC` | `decimal(18,0)` | sim |
| 36 | `NOME_PROC_CLIENTE` | `varchar(50)` | sim |
| 37 | `EMAIL_PROC_CLIENTE` | `varchar(64)` | sim |
| 38 | `CPF_PROC_CLIENTE` | `decimal(18,0)` | sim |
| 39 | `NOME_TEST_CLIENTE` | `varchar(50)` | sim |
| 40 | `EMAIL_TEST_CLIENTE` | `varchar(64)` | sim |
| 41 | `CPF_TEST_CLIENTE` | `decimal(18,0)` | sim |
| 42 | `TIPO_VENDA` | `varchar(20)` | sim |
| 43 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 44 | `MARCA_EQUIPAMENTO` | `varchar(20)` | sim |

### IV_Q$PEDIDO_SAM

`view` · `35 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MODELO_DO_PRODUTO` | `varchar(40)` | sim |
| 19 | `QUANTIDADE` | `varchar(20)` | sim |
| 20 | `PRECO` | `decimal(14,2)` | sim |
| 21 | `COTACAO_MODELO` | `decimal(15,0)` | sim |
| 22 | `CNPJ_CPF` | `decimal(18,0)` | sim |
| 23 | `INSCRICAO_ESTADUAL` | `decimal(10,0)` | sim |
| 24 | `CONDICAO_DE_PAGAMENT` | `varchar(40)` | sim |
| 25 | `PRAZO_DE_ENTREGA` | `datetime` | sim |
| 26 | `COMISSAO` | `decimal(10,0)` | sim |
| 27 | `SINAL` | `decimal(14,2)` | sim |
| 28 | `_SINAL` | `decimal(10,0)` | sim |
| 29 | `DATA_VENCIMENTO_SINA` | `datetime` | sim |
| 30 | `SALDO` | `varchar(30)` | sim |
| 31 | `BANCO` | `varchar(30)` | sim |
| 32 | `BONIFICACAO` | `varchar(50)` | sim |
| 33 | `TIPO_DE_VENDA` | `varchar(20)` | sim |
| 34 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 35 | `MARCA_EQUIPAMENTO` | `varchar(20)` | sim |

### IV_Q$PERCEPCAO_JD

`view` · `33 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NOTA_DEMONSTRACAO` | `varchar(20)` | sim |
| 19 | `POSSUI_EQP_JD` | `varchar(3)` | sim |
| 20 | `DEMONSTROU_INTERESSE` | `varchar(3)` | sim |
| 21 | `MODELO_1` | `varchar(20)` | sim |
| 22 | `DEMONSTRACAO_NEGOC` | `varchar(3)` | sim |
| 23 | `MODELO_02` | `varchar(20)` | sim |
| 24 | `ERGONOMIA_ATRATIVA` | `varchar(3)` | sim |
| 25 | `PERFORMANCE_ATRATIVA` | `varchar(3)` | sim |
| 26 | `CONSUMO_EXPECTATIVA` | `varchar(3)` | sim |
| 27 | `AGRICULTURA_VALOR` | `varchar(3)` | sim |
| 28 | `VALOR_OPERATION` | `varchar(3)` | sim |
| 29 | `PERCEPCAO_GERAL` | `varchar(100)` | sim |
| 30 | `EXPECTATIVA` | `varchar(20)` | sim |
| 31 | `CONSUMO_EXPEC` | `varchar(20)` | sim |
| 32 | `QUANTIDADE` | `varchar(20)` | sim |
| 33 | `HORIMETRO_FINAL` | `decimal(10,0)` | sim |

### IV_Q$PESQUISA_NPS

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `PESQUISA_NPS_0_A_10` | `varchar(20)` | sim |

### IV_Q$PESQUISA_TI

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `SATISFEITO_COM_ATEND` | `varchar(3)` | sim |
| 19 | `ATENDIMENTO_NO_PRAZO` | `varchar(3)` | sim |
| 20 | `NOTA_DE_ATENDIMENTO` | `varchar(20)` | sim |

### IV_Q$PESQ_SATISFACAO_PECA

`view` · `49 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `CORTESIA_DO_VENDEDOR` | `decimal(14,0)` | sim |
| 19 | `Q002_INSATISFEITO` | `decimal(1,0)` | sim |
| 20 | `Q002_INDIFERENTE` | `decimal(1,0)` | sim |
| 21 | `Q002_SATISFEITO` | `decimal(1,0)` | sim |
| 22 | `Q003_INSATISFEITO` | `decimal(1,0)` | sim |
| 23 | `Q003_INDIFERENTE` | `decimal(1,0)` | sim |
| 24 | `Q003_SATISFEITO` | `decimal(1,0)` | sim |
| 25 | `Q004_INSATISFEITO` | `decimal(1,0)` | sim |
| 26 | `Q004_INDIFERENTE` | `decimal(1,0)` | sim |
| 27 | `Q004_SATISFEITO` | `decimal(1,0)` | sim |
| 28 | `RECOMENDAR_PECAS_JD` | `varchar(3)` | sim |
| 29 | `Q006_INSATISFEITO` | `decimal(1,0)` | sim |
| 30 | `Q006_INDIFERENTE` | `decimal(1,0)` | sim |
| 31 | `Q006_SATISFEITO` | `decimal(1,0)` | sim |
| 32 | `Q007_ESTRUTURA` | `decimal(1,0)` | sim |
| 33 | `Q007_AREA_DE_VENDAS` | `decimal(1,0)` | sim |
| 34 | `Q007_AREA_DE_PECAS` | `decimal(1,0)` | sim |
| 35 | `Q007_AREA_DE_SERVICO` | `decimal(1,0)` | sim |
| 36 | `Q007_ORGANIZACAO` | `decimal(1,0)` | sim |
| 37 | `Q007_ESPACO_FISICO` | `decimal(1,0)` | sim |
| 38 | `Q007_OUTROS` | `decimal(1,0)` | sim |
| 39 | `Q008_ZERO` | `decimal(1,0)` | sim |
| 40 | `Q008_UM` | `decimal(1,0)` | sim |
| 41 | `Q008_DOIS` | `decimal(1,0)` | sim |
| 42 | `Q008_TRES` | `decimal(1,0)` | sim |
| 43 | `Q008_QUATRO` | `decimal(1,0)` | sim |
| 44 | `Q008_CINCO` | `decimal(1,0)` | sim |
| 45 | `Q008_SEIS` | `decimal(1,0)` | sim |
| 46 | `Q008_SETE` | `decimal(1,0)` | sim |
| 47 | `Q008_OITO` | `decimal(1,0)` | sim |
| 48 | `Q008_NOVE` | `decimal(1,0)` | sim |
| 49 | `Q008_DEZ` | `decimal(1,0)` | sim |

### IV_Q$PREMIO_DEMO

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `EQUIPAMENTO` | `varchar(30)` | sim |
| 19 | `VALOR_PREMIO` | `decimal(14,2)` | sim |
| 20 | `COMISSAO_PAGA` | `varchar(3)` | sim |

### IV_Q$PREVISAO_RECEBIMENTO

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `DATA_DA_PREVISAO_DE_` | `datetime` | sim |

### IV_Q$PRODUTO_RD

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `PRODUTO_RD` | `varchar(60)` | sim |
| 19 | `CAMPANHA` | `varchar(50)` | sim |
| 20 | `QUANTO_TEMPO_DESEJA_` | `varchar(100)` | sim |
| 21 | `MOTIVO_MIDIA` | `varchar(100)` | sim |
| 22 | `CONDICAO` | `varchar(50)` | sim |
| 23 | `FORMA_DE_CONTATO` | `varchar(30)` | sim |
| 24 | `COTA` | `varchar(30)` | sim |

### IV_Q$PROPOSTA_COMERCIAL

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TIPO_DE_EQUIPAMENTO` | `varchar(20)` | sim |
| 19 | `MARCA` | `varchar(20)` | sim |
| 20 | `MODELO_DO_EQUIPAMENT` | `varchar(50)` | sim |
| 21 | `DESCRICAO` | `varchar(150)` | sim |
| 22 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 23 | `VALOR_SINAL` | `decimal(14,2)` | sim |
| 24 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 25 | `VALOR_RECURSO_PROP` | `decimal(14,2)` | sim |
| 26 | `TIPO_DE_PAGAMENTO` | `varchar(20)` | sim |

### IV_Q$PROSPECCAO_SERV__JD

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `ORCAMENTO` | `varchar(30)` | sim |
| 19 | `CONDICAO_DE_PAGAMENT` | `varchar(30)` | sim |
| 20 | `ORDEM_DE_SERVICO` | `decimal(14,0)` | sim |
| 21 | `VALOR` | `decimal(14,2)` | sim |

### IV_Q$QUALIDADE_PECAS

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `SATISFEITO_ATEND` | `varchar(3)` | sim |
| 19 | `MOTIVO` | `varchar(20)` | sim |
| 20 | `INDICARIA` | `varchar(20)` | sim |
| 21 | `PRECISA_FAZER` | `varchar(500)` | sim |

### IV_Q$QUALIDADE_SERVICOS

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFEITO_ATEND` | `varchar(3)` | sim |
| 19 | `MOTIVO` | `varchar(40)` | sim |
| 20 | `INDICARIA` | `varchar(20)` | sim |
| 21 | `PRECISA_FAZER` | `varchar(500)` | sim |
| 22 | `TESTE` | `varchar(3)` | sim |

### IV_Q$QUALIDADE_VENDAMAQ

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `SATISFEITO_ATEND` | `varchar(3)` | sim |
| 19 | `MOTIVO` | `varchar(20)` | sim |
| 20 | `INDICARIA` | `varchar(20)` | sim |
| 21 | `PRECISA_FAZER` | `varchar(500)` | sim |

### IV_Q$RECEBIMENTO_A_PRAZO

`view` · `33 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `RECEBIMENTO_NOME_CLI` | `varchar(50)` | sim |
| 19 | `RECEBIMENTO_CPF_CNPJ` | `varchar(50)` | sim |
| 20 | `RECEBIMENTO_CIDADE` | `varchar(50)` | sim |
| 21 | `RECEBIMENTO_ENDERECO` | `varchar(150)` | sim |
| 22 | `RECEBIMENTO_NUMERO` | `varchar(8)` | sim |
| 23 | `RECEBIMENTO_BAIRRO` | `varchar(50)` | sim |
| 24 | `RECEBIMENTO_CEP` | `varchar(30)` | sim |
| 25 | `RECEBIMENTO_AVALISTA` | `varchar(100)` | sim |
| 26 | `RECEBIMENTO_CPF_AVAL` | `varchar(30)` | sim |
| 27 | `RECEBIMENTO_RG_AVALI` | `varchar(30)` | sim |
| 28 | `RECEBIMENTO_END_AVAL` | `varchar(150)` | sim |
| 29 | `RECEBIMENTO_FORMA_PA` | `varchar(40)` | sim |
| 30 | `RECEBIMENTO_EMAIL` | `varchar(100)` | sim |
| 31 | `RECEBIMENTO_EMAIL_2` | `varchar(100)` | sim |
| 32 | `RECEBIMENTO_MARGEM` | `varchar(30)` | sim |
| 33 | `RECEBIMENTO_TEMPO_ES` | `varchar(30)` | sim |

### IV_Q$RECEBIMENTO_COMISSAO

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NRO_NF` | `decimal(14,0)` | sim |
| 19 | `VLR_COMISSAO` | `decimal(14,2)` | sim |

### IV_Q$RECEBIMENTO_FINANC

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `PREV_RECBTO` | `datetime` | sim |

### IV_Q$RECEB_FINAN_IMPLEM

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `DATA_REC_FIN_IMPLE` | `datetime` | sim |

### IV_Q$RESPONSAVEL_TECNICO

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `decimal(15,5)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `RESPONSAVEL_TECNICO` | `varchar(40)` | sim |

### IV_Q$RESULTADO_DEMO

`view` · `31 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFACAO_DO_CLIENT` | `varchar(20)` | sim |
| 19 | `COMENTARIO_DO_CLIENT` | `varchar(150)` | sim |
| 20 | `FORMAS_DE_MEDICAO` | `varchar(20)` | sim |
| 21 | `CONSUMO_DIESEL` | `decimal(14,0)` | sim |
| 22 | `CONSUMO_ESPECIFICO` | `decimal(14,0)` | sim |
| 23 | `CAPACIDADE_OPERACION` | `decimal(14,0)` | sim |
| 24 | `AREA_TRABALHADA` | `decimal(14,0)` | sim |
| 25 | `HORIMETRO_INICIAL` | `varchar(30)` | sim |
| 26 | `HORIMETRO_FINAL` | `varchar(30)` | sim |
| 27 | `TOTAL_HORAS_TRABALHA` | `varchar(30)` | sim |
| 28 | `AREA_TOTAL_TRABALHAD` | `varchar(30)` | sim |
| 29 | `RENDIMENTO_OPERACION` | `varchar(30)` | sim |
| 30 | `COMBUSTIVEL_CONSUMID` | `varchar(30)` | sim |
| 31 | `MEDIA_CONSUMO` | `varchar(30)` | sim |

### IV_Q$RETORNADO_JD

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_RETORNO` | `datetime` | sim |

### IV_Q$REVISAO_100H

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_REV_100H` | `datetime` | sim |
| 19 | `DATA_TERM_REV_100H` | `datetime` | sim |

### IV_Q$REVISAO_1100_1150H

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_REV_1100` | `datetime` | sim |
| 19 | `DATA_TERM_REV_1100` | `datetime` | sim |

### IV_Q$REVISAO_1500H

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_REV_1500` | `datetime` | sim |
| 19 | `DATA_TERM_REV_1500` | `datetime` | sim |

### IV_Q$REVISAO_450_600H

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_REV_600H` | `datetime` | sim |
| 19 | `DATA_TERM_REV_600H` | `datetime` | sim |

### IV_Q$REVISAO_800H

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_REV_800H` | `datetime` | sim |
| 19 | `DATA_TERM_REV_800H` | `datetime` | sim |

### IV_Q$REVISAO_FIM_GARANTIA

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_REV_FIMG` | `datetime` | sim |
| 19 | `DATA_TERM_REV_FIMG` | `datetime` | sim |

### IV_Q$REV_DATA_SERVICO

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA_INICIO_SERVICO` | `datetime` | sim |
| 19 | `DATA_TERMINO_SERVICO` | `datetime` | sim |

### IV_Q$ROMANEIO_DEV_PECA

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_OS` | `varchar(30)` | sim |
| 19 | `DATA_DEVOLUCAO` | `datetime` | sim |
| 20 | `DEVOLVIDO_PARA` | `varchar(30)` | sim |

### IV_Q$SEPARACAO_PEDIDO

`view` · `34 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `GC_PEDIDO_PARA` | `varchar(50)` | sim |
| 19 | `GC_TIPO_EQUIP` | `varchar(20)` | sim |
| 20 | `GC_MARCA_EQUIP` | `varchar(20)` | sim |
| 21 | `GC_MODELO_EQUIP` | `varchar(20)` | sim |
| 22 | `GC_QTDE_TOTAL` | `varchar(20)` | sim |
| 23 | `UNIDADE_1` | `varchar(40)` | sim |
| 24 | `QUANTIDADE_1` | `decimal(10,0)` | sim |
| 25 | `UNIDADE_2` | `varchar(40)` | sim |
| 26 | `QUANTIDADE_2` | `decimal(10,0)` | sim |
| 27 | `UNIDADE_3` | `varchar(40)` | sim |
| 28 | `QUANTIDADE_3` | `decimal(10,0)` | sim |
| 29 | `UNIDADE_4` | `varchar(40)` | sim |
| 30 | `QUANTIDADE_4` | `decimal(10,0)` | sim |
| 31 | `UNIDADE_5` | `varchar(40)` | sim |
| 32 | `QUANTIDADE_5` | `decimal(10,0)` | sim |
| 33 | `UNIDADE_6` | `varchar(40)` | sim |
| 34 | `QUANTIDADE_6` | `decimal(10,0)` | sim |

### IV_Q$SERVICOS_AFERICAO

`view` · `34 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SATISFACAO_EM_RELACA` | `varchar(20)` | sim |
| 19 | `QUAL_SETOR_HA_OPORTU` | `varchar(20)` | sim |
| 20 | `SATIS_EM_RELACA` | `varchar(3)` | sim |
| 21 | `RECOMENDACAO_A_COLOR` | `varchar(20)` | sim |
| 22 | `SER_REALIZADOS` | `varchar(3)` | sim |
| 23 | `AQUILO_QUE_FOI_ACORD` | `varchar(3)` | sim |
| 24 | `ATEND_TEC_EXPECTATIV` | `varchar(3)` | sim |
| 25 | `NOTA_SATISFACAO` | `varchar(20)` | sim |
| 26 | `O_SR__INDICA_UM_AMIG` | `varchar(50)` | sim |
| 27 | `QUAL_COLABORADOR_DE_` | `varchar(30)` | sim |
| 28 | `QUESTAO_TEC` | `varchar(50)` | sim |
| 29 | `PRAZO_AGENDADOS` | `varchar(3)` | sim |
| 30 | `ACORDADO_CUMPRIDO` | `varchar(3)` | sim |
| 31 | `TECNICO_ATENDEU` | `varchar(3)` | sim |
| 32 | `SERV_EXECUTADO` | `varchar(3)` | sim |
| 33 | `TEVE_RETORNAR` | `varchar(3)` | sim |
| 34 | `QUE_NOTA_O_A__SR__A_` | `varchar(20)` | sim |

### IV_Q$SERVICO_EXTERNOS_JD

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `EQUIPAMENTO` | `varchar(30)` | sim |
| 19 | `CHASSI` | `varchar(30)` | sim |
| 20 | `FROTA` | `varchar(30)` | sim |
| 21 | `CONDICAO_DE_PAGAMENT` | `varchar(30)` | sim |
| 22 | `DEFEITO_APRESENTADO` | `varchar(100)` | sim |
| 23 | `LOCALIZACAO_DO_EQUIP` | `varchar(80)` | sim |
| 24 | `CONTATO_DO_OPERADOR` | `varchar(50)` | sim |

### IV_Q$SOLICITACAO_TCAT

`view` · `38 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TCAT_DATA_SINISTRO` | `datetime` | sim |
| 19 | `TCAT_TRANSPORTADORA` | `varchar(100)` | sim |
| 20 | `TCAT_CTRE` | `varchar(100)` | sim |
| 21 | `TCAT_MOTORISTA` | `varchar(100)` | sim |
| 22 | `TCAT_CNH_MOTORISTA` | `varchar(50)` | sim |
| 23 | `TCAT_PLACA_CAVALO` | `varchar(30)` | sim |
| 24 | `TCAT_PLACA_CARRETA` | `varchar(30)` | sim |
| 25 | `TCAT_NF_EQUIPAMENTO` | `varchar(30)` | sim |
| 26 | `TCAT_DATA_NF` | `datetime` | sim |
| 27 | `TCAT_VALOR_NF` | `decimal(14,2)` | sim |
| 28 | `TCAT_ORIGEM` | `varchar(100)` | sim |
| 29 | `TCAT_DESTINO` | `varchar(100)` | sim |
| 30 | `TCAT_EQUIPAMENTO` | `varchar(100)` | sim |
| 31 | `TCAT_ANO_EQUIPAMENTO` | `varchar(30)` | sim |
| 32 | `TCAT_CHASSIS_EQUIP` | `varchar(100)` | sim |
| 33 | `TCAT_OCORRENCIA` | `varchar(20)` | sim |
| 34 | `TCAT_CAUSA` | `varchar(20)` | sim |
| 35 | `TCAT_ORIGEM_AVARIA` | `varchar(30)` | sim |
| 36 | `TCAT_DESCRICAO` | `varchar(200)` | sim |
| 37 | `TCAT_VALORES_PECAS` | `decimal(14,2)` | sim |
| 38 | `TCAT_VALORES_OBRA` | `decimal(14,2)` | sim |

### IV_Q$TESTE1

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MARCA` | `varchar(30)` | sim |
| 19 | `EQPTO` | `varchar(30)` | sim |

### IV_Q$TESTE2

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VALOR` | `decimal(14,2)` | sim |
| 19 | `FORM` | `varchar(20)` | sim |

### IV_Q$TESTE_PRIMEIRO_JD

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TST_1JD` | `varchar(3)` | sim |
| 19 | `TESTE` | `decimal(14,0)` | sim |

### IV_Q$TICKET_DSI

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NOME_DO_CLIENTE` | `varchar(30)` | sim |
| 19 | `ORGANIZACAO_OPERATIO` | `varchar(30)` | sim |
| 20 | `TITULO` | `varchar(30)` | sim |
| 21 | `RESUMO_DO_PROBLEMA` | `varchar(300)` | sim |
| 22 | `PRODUTO` | `varchar(60)` | sim |
| 23 | `CHASSI` | `varchar(30)` | sim |
| 24 | `CONTATO_DO_CLIENTE` | `varchar(30)` | sim |

### IV_Q$VENDA

`view` · `38 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TIPO_DO_EQUIPAMENTO` | `varchar(20)` | sim |
| 19 | `MARCA` | `varchar(20)` | sim |
| 20 | `MODELO_DO_EQUIPAMENT` | `varchar(40)` | sim |
| 21 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 22 | `NRO_PEDIDO_DE_VENDA` | `decimal(14,0)` | sim |
| 23 | `DATA_PEDIDO_DE_VENDA` | `datetime` | sim |
| 24 | `PEDIDO_TIRADO_PARA` | `varchar(30)` | sim |
| 25 | `ORIGEM_DO_FATURAMENT` | `varchar(20)` | sim |
| 26 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 27 | `VALOR_RECURSO_PROPRI` | `decimal(14,2)` | sim |
| 28 | `INSTITUICAO_FINANCEI` | `varchar(20)` | sim |
| 29 | `NOME_DO_GERENTE` | `varchar(30)` | sim |
| 30 | `LINHA_DE_CREDITO` | `varchar(20)` | sim |
| 31 | `NUMERO_DA_COTACAO` | `decimal(14,0)` | sim |
| 32 | `CODIGO_DO_MODELO` | `varchar(50)` | sim |
| 33 | `PRIMEIRO_JOHN_DEERE` | `varchar(3)` | sim |
| 34 | `VENDA_COMPARTILHADA` | `varchar(3)` | sim |
| 35 | `CONSULTOR_1` | `varchar(20)` | sim |
| 36 | `CONSULTOR_2` | `varchar(20)` | sim |
| 37 | `TIPO_DE_VENDA` | `varchar(20)` | sim |
| 38 | `FORMA_DE_PAGAMENTO` | `varchar(20)` | sim |

### IV_Q$VENDAPERDIDA_SEGURO

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SEGURADORA` | `varchar(20)` | sim |
| 19 | `PRECO` | `decimal(14,2)` | sim |
| 20 | `MOTIVO` | `varchar(30)` | sim |

### IV_Q$VENDA_AMS

`view` · `81 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NUMERO_PEDIDO_AMS` | `varchar(15)` | sim |
| 19 | `VALOR_DA_VENDA` | `decimal(14,2)` | sim |
| 20 | `FORMA_DE_PAGAMENTO` | `varchar(300)` | sim |
| 21 | `Q004_MONITOR_GS3` | `decimal(1,0)` | sim |
| 22 | `Q004_RECEPTOR_0908` | `decimal(1,0)` | sim |
| 23 | `Q004_VOLANTE_0520` | `decimal(1,0)` | sim |
| 24 | `Q004_CHICOTE_90687` | `decimal(1,0)` | sim |
| 25 | `Q004_SUPORTE_90385` | `decimal(1,0)` | sim |
| 26 | `Q004_CHICOTE_ISO` | `decimal(1,0)` | sim |
| 27 | `Q004_ANTI_PF90889` | `decimal(1,0)` | sim |
| 28 | `Q004_AUTOTRAC_049` | `decimal(1,0)` | sim |
| 29 | `Q004_SF3_READY` | `decimal(1,0)` | sim |
| 30 | `Q004_RADIO_900_MHZ` | `decimal(1,0)` | sim |
| 31 | `Q004_450MHZ` | `decimal(1,0)` | sim |
| 32 | `Q004_KIT_AL207195` | `decimal(1,0)` | sim |
| 33 | `Q004_RTK_7307PC` | `decimal(1,0)` | sim |
| 34 | `Q004_ANTI_ROTACAO` | `decimal(1,0)` | sim |
| 35 | `Q004_ATIVACAO_RTK` | `decimal(1,0)` | sim |
| 36 | `COM_INSTALACAO_DO_AM` | `varchar(3)` | sim |
| 37 | `INFORMAR_AGREGA` | `varchar(400)` | sim |
| 38 | `INFORMAR_DESAGREGA_A` | `varchar(400)` | sim |
| 39 | `ITENS_DO_AMS` | `varchar(40)` | sim |
| 40 | `CHASSI_DO_ITEM_AMS` | `varchar(300)` | sim |
| 41 | `NUMERO_NOTA_FISCAL` | `varchar(10)` | sim |
| 42 | `DATA_DO_PEDIDO` | `datetime` | sim |
| 43 | `TIPO_DE_PAGAMENTO` | `varchar(20)` | sim |
| 44 | `INSTITUICAO_FINANCEI` | `varchar(30)` | sim |
| 45 | `NRO_DA_AGENCIA` | `decimal(14,0)` | sim |
| 46 | `NOME_DA_AGENCIA` | `varchar(30)` | sim |
| 47 | `NOME_DO_GERENTE_DO_B` | `varchar(30)` | sim |
| 48 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 49 | `TELEFONE_DA_AGENCIA` | `varchar(30)` | sim |
| 50 | `E_MAIL_DE_CONTATO_DA` | `varchar(30)` | sim |
| 51 | `INFORMACOES_PARA_O_F` | `varchar(300)` | sim |
| 52 | `PREVISAO_DO_FATURAME` | `datetime` | sim |
| 53 | `VALOR_DO_SINAL` | `decimal(14,2)` | sim |
| 54 | `INFORMAR_COMAR` | `varchar(90)` | sim |
| 55 | `DATA_APROVACAO` | `datetime` | sim |
| 56 | `HAVERA_BONIFICACAO` | `varchar(3)` | sim |
| 57 | `VALOR_BONIFICACAO` | `decimal(14,2)` | sim |
| 58 | `CODIGO_FINAME` | `varchar(30)` | sim |
| 59 | `SERA_DESMEMBRADO` | `varchar(3)` | sim |
| 60 | `DESCONTO_INCODICIONA` | `varchar(30)` | sim |
| 61 | `NR_NOTA_FISCAL_DESCO` | `varchar(30)` | sim |
| 62 | `NUMERO_OS_AGREGA_DES` | `decimal(14,0)` | sim |
| 63 | `CHASSI_DESCONTO_INCO` | `varchar(300)` | sim |
| 64 | `FAT_CENTRO_DE_CUSTO` | `varchar(20)` | sim |
| 65 | `NF_REFATURAMENTO_AMS` | `varchar(30)` | sim |
| 66 | `QUANTIDADE_DE_ITENS` | `varchar(5)` | sim |
| 67 | `CHASSI_DO_ITEM_AMS_2` | `varchar(30)` | sim |
| 68 | `CHASSI_DO_ITEM_AMS_3` | `varchar(30)` | sim |
| 69 | `CHASSI_DO_ITEM_AMS_4` | `varchar(30)` | sim |
| 70 | `CHASSI_DO_ITEM_AMS_5` | `varchar(30)` | sim |
| 71 | `CHASSI_DO_ITEM_AMS_6` | `varchar(30)` | sim |
| 72 | `CHASSI_DO_ITEM_AMS_7` | `varchar(30)` | sim |
| 73 | `CHASI_DO_ITEM_AMS_8` | `varchar(30)` | sim |
| 74 | `CHASSI_DO_ITEM_AMS_9` | `varchar(30)` | sim |
| 75 | `CHASSI_DO_ITEM_AMS10` | `varchar(30)` | sim |
| 76 | `OS_ADICIONAIS` | `varchar(100)` | sim |
| 77 | `DESCONTO_INCODICION` | `varchar(3)` | sim |
| 78 | `SINAL_EMBUTIDO` | `varchar(3)` | sim |
| 79 | `ID_CONTROLADORA` | `varchar(30)` | sim |
| 80 | `ID_MONITOR_2` | `varchar(10)` | sim |
| 81 | `ID_MONITOR_1` | `varchar(10)` | sim |

### IV_Q$VENDA_CONSORCIO

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `PRODUTO` | `varchar(20)` | sim |
| 19 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 20 | `VALOR_ENTRADA` | `decimal(14,2)` | sim |
| 21 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 22 | `GRUPO` | `decimal(14,0)` | sim |
| 23 | `COTA` | `decimal(14,0)` | sim |

### IV_Q$VENDA_DIRETA

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VD_TIRADO_PARA` | `varchar(100)` | sim |
| 19 | `VD_NRO_COTACAO` | `varchar(15)` | sim |
| 20 | `VD_NRO_PEDIDO_VENDA` | `varchar(15)` | sim |
| 21 | `VD_DATA_PEDIDO_VEND` | `datetime` | sim |
| 22 | `VD_TIPO_EQUIP` | `varchar(20)` | sim |
| 23 | `VD_MARCA_EQUIP` | `varchar(20)` | sim |
| 24 | `VD_MODELO_EQUIP` | `varchar(20)` | sim |
| 25 | `VD_VALOR_TOTAL` | `decimal(14,2)` | sim |
| 26 | `VD_BONIFICACAO` | `varchar(350)` | sim |
| 27 | `VD_COMISSAO` | `varchar(10)` | sim |
| 28 | `VD_EMAIL_NF` | `varchar(50)` | sim |

### IV_Q$VENDA_DSI

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `PEDIDO_PARA` | `varchar(30)` | sim |
| 19 | `TIPO_SERVICO` | `varchar(30)` | sim |
| 20 | `TIPO_EQUIP` | `varchar(20)` | sim |
| 21 | `MARCA` | `varchar(20)` | sim |
| 22 | `MODELO_EQUIP` | `varchar(20)` | sim |
| 23 | `VALOR_SERVICO` | `decimal(14,2)` | sim |
| 24 | `VALOR_DESLOCAM` | `decimal(14,2)` | sim |
| 25 | `DATA_EXEC_SERVICO` | `datetime` | sim |
| 26 | `SITUACAO_CADASTRAL` | `varchar(20)` | sim |
| 27 | `QTDE_EQUIPAMENTO` | `decimal(14,0)` | sim |
| 28 | `CONDICAO_PAGAMENTO` | `varchar(20)` | sim |
| 29 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 30 | `MARCA_EQUIPAMENTO` | `varchar(20)` | sim |
| 31 | `MODELO_EQUIPAMENTO` | `varchar(20)` | sim |
| 32 | `CHASSIS_EQUIP` | `varchar(30)` | sim |

### IV_Q$VENDA_EQUIPAMENTO

`view` · `65 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VENDA_COMPRADOR` | `varchar(40)` | sim |
| 19 | `VENDA_JDQUOTE` | `varchar(30)` | sim |
| 20 | `VENDA_TIPO_VENDA` | `varchar(20)` | sim |
| 21 | `VENDA_DATA_PEDIDO` | `datetime` | sim |
| 22 | `VENDA_TIPO_EQUIPAMEN` | `varchar(20)` | sim |
| 23 | `VENDA_MARCA_EQUIPAME` | `varchar(20)` | sim |
| 24 | `VENDA_MODELO_EQUIPAM` | `varchar(40)` | sim |
| 25 | `VENDA_QTDE_EQUIPAMEN` | `varchar(20)` | sim |
| 26 | `VENDA_LASTRO_DIANT` | `varchar(20)` | sim |
| 27 | `VENDA_LASTRO_TRASEIR` | `varchar(20)` | sim |
| 28 | `VENDA_RODADO_DIANT` | `varchar(30)` | sim |
| 29 | `VENDA_RODADO_TRASEIR` | `varchar(30)` | sim |
| 30 | `VENDA_QTDE_VCR` | `varchar(20)` | sim |
| 31 | `VENDA_QTDE_VCR_DLX` | `varchar(20)` | sim |
| 32 | `VENDA_MEDIDA_BITOLA` | `varchar(60)` | sim |
| 33 | `VENDA_CREEPER` | `varchar(20)` | sim |
| 34 | `VENDA_INST_CABINE` | `varchar(20)` | sim |
| 35 | `VENDA_AMS` | `varchar(20)` | sim |
| 36 | `RECEPTOR` | `varchar(20)` | sim |
| 37 | `VENDA_AMS_ATIVACAO` | `varchar(20)` | sim |
| 38 | `VENDA_AMS_MONITOR` | `varchar(20)` | sim |
| 39 | `VENDA_AMS_VOLANTE` | `varchar(20)` | sim |
| 40 | `VENDA_AMS_RADIO` | `varchar(20)` | sim |
| 41 | `VENDAS_AMS_CONECT` | `varchar(20)` | sim |
| 42 | `VENDA_AMS_CONFIG_ADC` | `varchar(350)` | sim |
| 43 | `VENDA_VALOR_TOTAL` | `decimal(14,2)` | sim |
| 44 | `VENDA_VALOR_SINAL` | `decimal(14,2)` | sim |
| 45 | `VENDA_DATA_SINAL` | `datetime` | sim |
| 46 | `VENDA_VALOR_FINANC` | `decimal(14,2)` | sim |
| 47 | `VENDA_FORMA_PAGTO` | `varchar(20)` | sim |
| 48 | `VENDA_INST_FINANC` | `varchar(20)` | sim |
| 49 | `VENDA_LINHA_CREDITO` | `varchar(20)` | sim |
| 50 | `VENDA_CONTATO_BANCO` | `varchar(350)` | sim |
| 51 | `VENDA_COMPARTILHADA` | `varchar(3)` | sim |
| 52 | `VENDA_CONSULTOR_1` | `varchar(20)` | sim |
| 53 | `VENDA_CONSULTOR_2` | `varchar(20)` | sim |
| 54 | `VENDA_PRIMEIRO_JD` | `varchar(3)` | sim |
| 55 | `DESCONTO_INCONDICION` | `varchar(3)` | sim |
| 56 | `VENDA_MARGEM_NEGOC` | `varchar(50)` | sim |
| 57 | `VENDA_COTACAO` | `varchar(50)` | sim |
| 58 | `VENDA_FDD` | `varchar(50)` | sim |
| 59 | `VENDA_CHASSI` | `varchar(50)` | sim |
| 60 | `VENDA_INFO_PREP` | `varchar(300)` | sim |
| 61 | `VENDA_ENTREG_PREP` | `datetime` | sim |
| 62 | `VENDA_DATA_ENTREGA` | `datetime` | sim |
| 63 | `VENDA_DESCONTO_INC` | `varchar(50)` | sim |
| 64 | `VENDA_TRANSPORTADORA` | `varchar(50)` | sim |
| 65 | `VENDA_VALOR_FRETE` | `decimal(14,2)` | sim |

### IV_Q$VENDA_LOCACAO

`view` · `58 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `TIPO_EQUIP_MANITOU` | `varchar(30)` | sim |
| 19 | `MARCA_M` | `varchar(30)` | sim |
| 20 | `MODELO_EQUIPM` | `varchar(30)` | sim |
| 21 | `N_PEDIDO_VDM` | `decimal(14,0)` | sim |
| 22 | `TIPO_TORRE` | `varchar(60)` | sim |
| 23 | `TIPO_RODADO` | `varchar(60)` | sim |
| 24 | `BITOLA` | `varchar(60)` | sim |
| 25 | `ACESSORIOS` | `varchar(150)` | sim |
| 26 | `IMPLEMENTOS` | `varchar(100)` | sim |
| 27 | `EASY_MANAGER` | `varchar(100)` | sim |
| 28 | `OUTRAS_CONFIG` | `varchar(150)` | sim |
| 29 | `AGREGA_DESAGREG` | `varchar(100)` | sim |
| 30 | `VALOR_TOTA` | `decimal(14,2)` | sim |
| 31 | `VALOR_SINAL` | `decimal(14,2)` | sim |
| 32 | `DATA_SINAL` | `datetime` | sim |
| 33 | `VALOR_FINANC` | `decimal(14,2)` | sim |
| 34 | `TIPO_PGTO` | `varchar(60)` | sim |
| 35 | `DATA_PGTO` | `datetime` | sim |
| 36 | `DATA_ATUALIZADA` | `datetime` | sim |
| 37 | `COD_FINAME` | `decimal(14,0)` | sim |
| 38 | `TX_FLAT` | `varchar(3)` | sim |
| 39 | `TAXA_FLT_` | `varchar(3)` | sim |
| 40 | `INST_FINANC` | `varchar(20)` | sim |
| 41 | `N_AGENCIA` | `varchar(30)` | sim |
| 42 | `NOME_AGENC` | `varchar(50)` | sim |
| 43 | `NOME_GER_AGENC` | `varchar(50)` | sim |
| 44 | `TEL_AGENC` | `decimal(14,0)` | sim |
| 45 | `CONTATO_AGENCIA` | `varchar(30)` | sim |
| 46 | `INF_FINANC` | `varchar(150)` | sim |
| 47 | `CHASSI_EQUIP` | `varchar(30)` | sim |
| 48 | `APROV_CRED` | `varchar(20)` | sim |
| 49 | `N__DO_PAC` | `varchar(30)` | sim |
| 50 | `PAC_EMITIDO` | `varchar(30)` | sim |
| 51 | `N_CONTRATO` | `decimal(14,0)` | sim |
| 52 | `PREVISAO_DE_FATURAME` | `datetime` | sim |
| 53 | `DATA_PED_VD` | `datetime` | sim |
| 54 | `DATA_PROC_BC` | `datetime` | sim |
| 55 | `N__NF` | `decimal(14,0)` | sim |
| 56 | `DATA_FAT` | `datetime` | sim |
| 57 | `NF_REFAT` | `decimal(14,0)` | sim |
| 58 | `OBS` | `varchar(100)` | sim |

### IV_Q$VENDA_MAQUINA_FY25

`view` · `61 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `PEDIDO_TIRADO_PARA` | `varchar(40)` | sim |
| 19 | `JD_QUOTE` | `varchar(30)` | sim |
| 20 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 21 | `MARCA_EQUIPAMENTO` | `varchar(20)` | sim |
| 22 | `MODELO_EQUIPAMENTO` | `varchar(20)` | sim |
| 23 | `QTDE_EQUIPAMENTOS` | `varchar(20)` | sim |
| 24 | `QTDE_PESO_DIANTEIRO` | `varchar(20)` | sim |
| 25 | `KG_PESO_DIANTEIRO` | `varchar(20)` | sim |
| 26 | `QTDE_PESO_TRASEIRO` | `varchar(20)` | sim |
| 27 | `KG_PESO_TRASEIRO` | `varchar(20)` | sim |
| 28 | `LASTRO_DIANTEIRO` | `varchar(20)` | sim |
| 29 | `LASTRO_TRASEIRO` | `varchar(20)` | sim |
| 30 | `RODADO_DIANTEIRO` | `varchar(50)` | sim |
| 31 | `RODADO_TRASEIRO` | `varchar(50)` | sim |
| 32 | `QTDE_VCR_PADRAO` | `varchar(20)` | sim |
| 33 | `QTDE_VCR_DELUXE` | `varchar(20)` | sim |
| 34 | `MEDIDA_BITOLA` | `varchar(50)` | sim |
| 35 | `CREEPER` | `varchar(20)` | sim |
| 36 | `INSTALACAO_CABINE_CA` | `varchar(20)` | sim |
| 37 | `AMS` | `varchar(3)` | sim |
| 38 | `RECEPTOR` | `varchar(20)` | sim |
| 39 | `ATIVACAO` | `varchar(20)` | sim |
| 40 | `MONITOR` | `varchar(20)` | sim |
| 41 | `VOLANTE` | `varchar(20)` | sim |
| 42 | `RADIO` | `varchar(20)` | sim |
| 43 | `CONECTIVIDADE` | `varchar(20)` | sim |
| 44 | `CONFIGURACAO_ADICION` | `varchar(300)` | sim |
| 45 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 46 | `VALOR_SINAL` | `decimal(14,2)` | sim |
| 47 | `DATA_SINAL` | `datetime` | sim |
| 48 | `VALOR_FINANCIADO` | `decimal(14,2)` | sim |
| 49 | `FORMA_PAGAMENTO` | `varchar(20)` | sim |
| 50 | `INSTITUICAO_FINANCEI` | `varchar(20)` | sim |
| 51 | `CONTATO_BANCO` | `varchar(50)` | sim |
| 52 | `CONFIGURACAO_AMS` | `varchar(200)` | sim |
| 53 | `COTACAO` | `varchar(50)` | sim |
| 54 | `FDD` | `varchar(50)` | sim |
| 55 | `CHASSI` | `varchar(60)` | sim |
| 56 | `PREPARACAO` | `varchar(300)` | sim |
| 57 | `PREVISAO_ENTREGA_PRE` | `datetime` | sim |
| 58 | `DATA_ENTREGA` | `datetime` | sim |
| 59 | `DESCONTO_INCONDICION` | `decimal(14,2)` | sim |
| 60 | `TRANSPORTADORA` | `varchar(100)` | sim |
| 61 | `VALOR_DO_FRETE` | `decimal(14,2)` | sim |

### IV_Q$VENDA_N

`view` · `40 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NOMERAZAO` | `varchar(100)` | sim |
| 3 | `SEQQUESTIONARIO` | `numeric(18,0)` | **nao** |
| 4 | `SEQFORMULARIO` | `numeric(18,0)` | **nao** |
| 5 | `DESCRICAO` | `varchar(20)` | sim |
| 6 | `DTAREALIZACAO` | `datetime` | sim |
| 7 | `USUINCLUSAO` | `varchar(20)` | sim |
| 8 | `DTAALTERACAO` | `datetime` | sim |
| 9 | `USUALTERACAO` | `varchar(20)` | sim |
| 10 | `OBS` | `varchar(4000)` | sim |
| 11 | `SEQHISTORICO` | `float(53,0)` | sim |
| 12 | `RESULTADO` | `decimal(6,0)` | sim |
| 13 | `PROCESSO` | `decimal(15,0)` | sim |
| 14 | `DEPARTAMENTO` | `varchar(12)` | sim |
| 15 | `LINKDOCTO` | `varchar(10)` | sim |
| 16 | `LINKNRO` | `numeric(18,0)` | sim |
| 17 | `LINKSERIE` | `varchar(250)` | sim |
| 18 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 19 | `MARCA_EQUIPAM` | `varchar(20)` | sim |
| 20 | `MODELO_EQUIPAM` | `varchar(20)` | sim |
| 21 | `CONFIG_MAQUINA` | `varchar(1)` | **nao** |
| 22 | `VALOR_TOTAL` | `decimal(14,2)` | sim |
| 23 | `VALOR_REC_PROPRIO` | `int(10,0)` | **nao** |
| 24 | `NRO_PEDIDO_VENDA` | `decimal(14,0)` | sim |
| 25 | `DATA_PED_VENDA` | `datetime` | sim |
| 26 | `FINANCIADO_PARA` | `varchar(30)` | sim |
| 27 | `ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 28 | `VLR_FINANCIADO` | `decimal(14,2)` | sim |
| 29 | `VLR_REC_PROPRIO` | `decimal(14,2)` | sim |
| 30 | `TIPO_DE_FINANCIAME` | `varchar(1)` | **nao** |
| 31 | `INST_FINANCEIRA` | `varchar(20)` | sim |
| 32 | `NOME_GERENTE` | `varchar(30)` | sim |
| 33 | `PFO_PREVISAO_DE_FAT` | `varchar(1)` | **nao** |
| 34 | `OBSERVACOES` | `varchar(1)` | **nao** |
| 35 | `LINHA_DE_CREDITO` | `varchar(20)` | sim |
| 36 | `QUOTE_ID` | `decimal(14,0)` | sim |
| 37 | `TIPO_CAMBIO` | `varchar(1)` | **nao** |
| 38 | `CABINADO` | `varchar(1)` | **nao** |
| 39 | `CODMODELO` | `varchar(50)` | sim |
| 40 | `PEDIDO_TIRADO_PARA` | `varchar(30)` | sim |

### IV_Q$VENDA_PERDIDA

`view` · `30 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MARCA_VP` | `varchar(30)` | sim |
| 19 | `REVENDA` | `varchar(30)` | sim |
| 20 | `MODELO_VP` | `varchar(30)` | sim |
| 21 | `DATA_DA_VENDA` | `datetime` | sim |
| 22 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 23 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 24 | `PRECO_JOHN_DEERE` | `decimal(14,2)` | sim |
| 25 | `MODELO_JOHN_DEER` | `varchar(30)` | sim |
| 26 | `MOTIVO` | `varchar(40)` | sim |
| 27 | `TIPO_DE_EQUIPAMENTO` | `varchar(30)` | sim |
| 28 | `PARTICIPAMOS_DA_NEGO` | `varchar(3)` | sim |
| 29 | `HAVIA_MONITORAMENTO` | `varchar(3)` | sim |
| 30 | `MODELO_IMPLEMENTO` | `varchar(20)` | sim |

### IV_Q$VENDA_PERDIDA_FY25

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VP_TIPO_EQUIP_CONCOR` | `varchar(20)` | sim |
| 19 | `VP_MARCA_EQUIP_CONCO` | `varchar(20)` | sim |
| 20 | `VP_MODELO_EQUIP_CONC` | `varchar(40)` | sim |
| 21 | `VP_COD_MODELO_CONCOR` | `varchar(20)` | sim |
| 22 | `VP_QUANTIDADE` | `decimal(8,0)` | sim |
| 23 | `VP_REVENDA` | `varchar(20)` | sim |
| 24 | `VP_DATA_VP` | `datetime` | sim |
| 25 | `VP_PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 26 | `VP_PRECO_JD` | `decimal(14,2)` | sim |
| 27 | `VP_MODELO_JD` | `varchar(20)` | sim |
| 28 | `PILOTO` | `varchar(3)` | sim |
| 29 | `VP_MOTIVO_VP` | `varchar(40)` | sim |

### IV_Q$VENDA_PERDIDA_IMPL

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MARCA_VP` | `varchar(35)` | sim |
| 19 | `REVENDA` | `varchar(35)` | sim |
| 20 | `MODELO_VP` | `varchar(35)` | sim |
| 21 | `DATA_DA_VENDA` | `datetime` | sim |
| 22 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 23 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 24 | `PRECO_JOHN_DEERE` | `decimal(14,2)` | sim |
| 25 | `MODELO_JOHN_DEER` | `varchar(35)` | sim |
| 26 | `MOTIVO` | `varchar(20)` | sim |

### IV_Q$VENDA_PERDIDA_IMPLEM

`view` · `25 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(8,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(250)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `MARCA_VC_IMPL` | `varchar(20)` | sim |
| 19 | `REVENDA_CONC_IMPL` | `varchar(100)` | sim |
| 20 | `DTA_DA_VENDA` | `datetime` | sim |
| 21 | `QUANTIDADE_IMPLEMENT` | `decimal(14,0)` | sim |
| 22 | `PRECO_CONC` | `decimal(14,2)` | sim |
| 23 | `PRECO_JD` | `decimal(14,2)` | sim |
| 24 | `MODELO_JD_IMPL` | `varchar(250)` | sim |
| 25 | `MOTIVO_VD_IMP` | `varchar(20)` | sim |

### IV_Q$VENDA_PERDIDA_JDE

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MARCA_VP` | `varchar(20)` | sim |
| 19 | `REVENDA` | `varchar(20)` | sim |
| 20 | `MODELO_VP` | `varchar(20)` | sim |
| 21 | `DATA_DA_VENDA` | `datetime` | sim |
| 22 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 23 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 24 | `PRECO_JOHN_DEERE` | `decimal(14,2)` | sim |
| 25 | `MODELO_JOHN_DEER` | `varchar(20)` | sim |
| 26 | `MOTIVO` | `varchar(20)` | sim |
| 27 | `TIPO_EQUIPAMENTO` | `varchar(20)` | sim |

### IV_Q$VENDA_PERDIDA_MANITO

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `PART_NEG` | `varchar(3)` | sim |
| 19 | `REVENDA_GANH` | `varchar(50)` | sim |
| 20 | `MOD_EQUIP` | `varchar(30)` | sim |
| 21 | `DT_VENDA` | `datetime` | sim |
| 22 | `QUANTIDAD` | `decimal(14,0)` | sim |
| 23 | `PRECO_CONC` | `decimal(14,2)` | sim |
| 24 | `PRECO_COL_EQ` | `decimal(14,2)` | sim |
| 25 | `MODELO_COLORADO_EQUI` | `varchar(40)` | sim |
| 26 | `MOTIVO_EQ` | `varchar(20)` | sim |

### IV_Q$VENDA_PERDIDA_MAQIMP

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MARCA_VP` | `varchar(20)` | sim |
| 19 | `REVENDA_VP` | `varchar(20)` | sim |
| 20 | `MODELO_VP` | `varchar(20)` | sim |
| 21 | `DATA_VENDA_VP` | `datetime` | sim |
| 22 | `QUANTIDADE_VP` | `decimal(14,0)` | sim |
| 23 | `PRECO_CONCORRENTE_VP` | `decimal(14,2)` | sim |
| 24 | `PRECO_JOHN_DEERE_VP` | `decimal(14,2)` | sim |
| 25 | `MODELO_JOHN_DEERE_VP` | `varchar(20)` | sim |
| 26 | `MOTIVO_VP` | `varchar(40)` | sim |
| 27 | `TIPO_EQUIP_VP` | `varchar(20)` | sim |
| 28 | `COD_MODE_VP` | `varchar(30)` | sim |

### IV_Q$VENDA_PERDIDA_PROD

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MARCA` | `varchar(20)` | sim |
| 19 | `REVENDA` | `varchar(20)` | sim |
| 20 | `MODELO` | `varchar(30)` | sim |
| 21 | `DATA_DA_VENDA` | `datetime` | sim |
| 22 | `QUANTIDADE` | `decimal(14,0)` | sim |
| 23 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 24 | `MODELO_JOHN_DEERE` | `varchar(20)` | sim |
| 25 | `PRECO_JOHN_DEERE` | `decimal(14,2)` | sim |
| 26 | `MOTIVA` | `varchar(20)` | sim |
| 27 | `TIPO_EQUIPAMENTO` | `varchar(30)` | sim |
| 28 | `PARTICIPAMOS_DA_NEGO` | `varchar(3)` | sim |

### IV_Q$VENDA_PERDIDA_SEGURO

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `DATA` | `datetime` | sim |
| 19 | `EQUIPAMENTOS_JD` | `varchar(20)` | sim |
| 20 | `OUTROS_EQUIPAMENTOS` | `varchar(20)` | sim |
| 21 | `SEGURADORA_CONCORREN` | `varchar(20)` | sim |
| 22 | `PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 23 | `CONDICAO_COMERCIAL_C` | `decimal(14,2)` | sim |
| 24 | `SEGURADORA_COLORADO` | `varchar(20)` | sim |
| 25 | `PRECO_COLORADO` | `decimal(14,2)` | sim |
| 26 | `CONDICAO_COMERCIAL` | `decimal(14,2)` | sim |
| 27 | `MOTIVO` | `varchar(20)` | sim |
| 28 | `PRECO_CONCORRENTE1` | `varchar(20)` | sim |
| 29 | `PRECO_COLORADO1` | `varchar(20)` | sim |

### IV_Q$VENDA_PERDIDA_TESTE

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TIPO_EQUIP` | `varchar(20)` | sim |
| 19 | `MARCA_EQUIP` | `varchar(20)` | sim |
| 20 | `MODELO_EQUIP` | `varchar(20)` | sim |
| 21 | `CODIGO_DO_MODELO` | `varchar(20)` | sim |
| 22 | `QTDE_EQUIP` | `varchar(30)` | sim |
| 23 | `REVENDA_EQUIP` | `varchar(20)` | sim |
| 24 | `DATA_VENDA` | `datetime` | sim |
| 25 | `PRECO_CONC` | `decimal(14,2)` | sim |
| 26 | `PRECO_JD` | `decimal(14,2)` | sim |
| 27 | `MODELO_JD` | `varchar(20)` | sim |
| 28 | `MOTIVO_VENDA_PERDIDA` | `varchar(20)` | sim |

### IV_Q$VENDA_PNEUS

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `NRO_NF` | `decimal(10,0)` | sim |
| 19 | `MEDIDA` | `varchar(30)` | sim |
| 20 | `MARCA` | `varchar(20)` | sim |
| 21 | `QUANTIDADE` | `decimal(10,0)` | sim |
| 22 | `FORMA_DE_PAGAMENTO` | `varchar(20)` | sim |
| 23 | `VALOR_DE_VENDA` | `decimal(14,2)` | sim |

### IV_Q$VENDA_PRECISION_UP

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `PUK_TIPO_EQUIP` | `varchar(20)` | sim |
| 19 | `PUK_MARCA_EQUIP` | `varchar(20)` | sim |
| 20 | `PUK_MODELO_EQUIP` | `varchar(20)` | sim |
| 21 | `PUK_QUANTIDADE` | `varchar(30)` | sim |
| 22 | `PUK_FORMA_PAGTO` | `varchar(20)` | sim |
| 23 | `PUK_ITENS_AGREGA` | `varchar(300)` | sim |
| 24 | `PUK_INSTALACAO` | `varchar(3)` | sim |

### IV_Q$VENDA_SEMINOVO

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SMNV_TIPO_VENDA` | `varchar(40)` | sim |
| 19 | `SMNV_TIPO_EQUIPAMENT` | `varchar(20)` | sim |
| 20 | `SMNV_MARCA` | `varchar(20)` | sim |
| 21 | `SMNV_MODELO` | `varchar(20)` | sim |
| 22 | `SMNV_POTENCIA` | `varchar(20)` | sim |
| 23 | `SMNV_ANO_FABRICACAO` | `varchar(4)` | sim |
| 24 | `SMNV_HORAS_KMS` | `varchar(30)` | sim |
| 25 | `SMNV_VALOR_AVALIADO` | `decimal(14,2)` | sim |
| 26 | `SMNV_CHASSI` | `varchar(30)` | sim |

### IV_Q$VENDA_SERVICOS_AMS

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `NUMERO_PEDIDO` | `decimal(14,0)` | sim |
| 19 | `VALOR_DA_VENDA` | `decimal(14,2)` | sim |
| 20 | `FORMA_DE_PAGAMENTO` | `varchar(60)` | sim |
| 21 | `SERVICO_VENDIDO` | `varchar(20)` | sim |
| 22 | `DESCRICAO_DO_SERVICO` | `varchar(200)` | sim |

### IV_Q$VENDA_VP_PNEUS

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `MEDIDA` | `varchar(30)` | sim |
| 19 | `MARCA` | `varchar(20)` | sim |
| 20 | `QUANTIDADE` | `decimal(10,0)` | sim |
| 21 | `VALOR_DE_VENDA` | `decimal(14,2)` | sim |
| 22 | `FORNECEDOR` | `varchar(30)` | sim |

### IV_Q$VENDER_RENOVACAO_SEG

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SEGURADORA` | `varchar(20)` | sim |
| 19 | `CORRETORA` | `varchar(30)` | sim |
| 20 | `APOLICE` | `varchar(30)` | sim |
| 21 | `VIGENCIA_INICIO` | `datetime` | sim |
| 22 | `VIGENCIA_TERMINO` | `datetime` | sim |
| 23 | `PREMIO_LIQUIDO` | `decimal(14,2)` | sim |
| 24 | `QDE_PARCELAS` | `decimal(14,0)` | sim |
| 25 | `VALOR_PARCELA` | `decimal(14,2)` | sim |
| 26 | `DATA_VENCTO_PRIMEIRA` | `datetime` | sim |
| 27 | `PART_PORCENT` | `varchar(5)` | sim |
| 28 | `NUM_PARCELA` | `decimal(14,0)` | sim |
| 29 | `VALOR_PAGO` | `decimal(14,2)` | sim |
| 30 | `HOUVE_SINISTRO` | `varchar(3)` | sim |
| 31 | `DATA_SINISTRO_UM` | `datetime` | sim |
| 32 | `DATA_SINISTRO_DOIS` | `datetime` | sim |

### IV_Q$VISITA_DSI

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `QTDE_MQNS_CLIENTE` | `varchar(30)` | sim |
| 19 | `POTENCIAL_HECTARES` | `varchar(30)` | sim |
| 20 | `QTDE_PROD_TEC` | `varchar(30)` | sim |

### IV_Q$VISITA_EXP_CLIENTE

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(60)` | **nao** |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(1000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(10)` | sim |
| 18 | `FICOU_ALGUMA_DUVIDA_` | `varchar(3)` | sim |
| 19 | `O_EQUIPAMENTO_ESTA_F` | `varchar(3)` | sim |
| 20 | `TEM_ALGUM_PROBLEMA_N` | `varchar(3)` | sim |
| 21 | `O_SR_SABE_COM_QUEM_P` | `varchar(3)` | sim |
| 22 | `ALGO_FOI_PROMETIDO_N` | `varchar(3)` | sim |
| 23 | `DE_0_A_10_QUAL_NOTA_` | `decimal(14,0)` | sim |
| 24 | `O_QUE_O_SR__A__ESPER` | `varchar(100)` | sim |
| 25 | `O_QUE_A_COLORADO_MAQ` | `varchar(100)` | sim |
| 26 | `COMO_ESTA_NOSSO_ATEN` | `varchar(30)` | sim |
| 27 | `SERVICOS` | `varchar(30)` | sim |

### IV_Q$VP_COLHEDORA

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VP_CH_TIPO` | `varchar(20)` | sim |
| 19 | `VP_CH_MARCA_CONC` | `varchar(20)` | sim |
| 20 | `VP_CH_MODELO_CONC` | `varchar(20)` | sim |
| 21 | `VP_CH_QTDE` | `varchar(10)` | sim |
| 22 | `VP_CH_RODADO` | `varchar(20)` | sim |
| 23 | `VP_CH_TECNOLOGIA` | `varchar(3)` | sim |
| 24 | `VP_ITENS_TECNOLOGIA` | `varchar(350)` | sim |
| 25 | `VP_CH_PRECO_CONC` | `decimal(14,2)` | sim |
| 26 | `VP_CH_MODELO_JD` | `varchar(20)` | sim |
| 27 | `VP_CH_PRECO_JD` | `decimal(14,2)` | sim |

### IV_Q$VP_COLHEITADEIRA

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VP_CA_TIPO` | `varchar(20)` | sim |
| 19 | `VP_CA_MARCA_CONC` | `varchar(20)` | sim |
| 20 | `VP_CA_MODELO_CONC` | `varchar(20)` | sim |
| 21 | `VP_CA_QTDE_CONC` | `varchar(10)` | sim |
| 22 | `VP_CA_PLATAFORMA_CON` | `varchar(350)` | sim |
| 23 | `VP_CA_RODADO` | `varchar(20)` | sim |
| 24 | `VP_CA_TECNOLOGIA` | `varchar(20)` | sim |
| 25 | `VP_CA_ITENS_TECNOLOG` | `varchar(350)` | sim |
| 26 | `VP_CA_PRECO_CONC` | `decimal(14,2)` | sim |
| 27 | `VP_CA_MODELO_JD` | `varchar(20)` | sim |
| 28 | `VP_CA_PRECO_JD` | `decimal(14,2)` | sim |

### IV_Q$VP_PLANTADEIRA

`view` · `29 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VP_PL_TIPO_EQUIP_CON` | `varchar(20)` | sim |
| 19 | `VP_PL_MARCA_CONC` | `varchar(20)` | sim |
| 20 | `VP_PL_MODELO_CONC` | `varchar(20)` | sim |
| 21 | `VP_PL_QTDE_LINHAS` | `varchar(10)` | sim |
| 22 | `VP_PL_CAIXA_SEMENTE` | `varchar(20)` | sim |
| 23 | `VP_PL_DESLIGAMENTO` | `varchar(3)` | sim |
| 24 | `VP_PL_MARCADOR_LINHA` | `varchar(3)` | sim |
| 25 | `VP_PL_DOSADOR_MEC` | `varchar(3)` | sim |
| 26 | `VP_PL_MONITOR_PLANT` | `varchar(3)` | sim |
| 27 | `VP_PL_PRECO_CONC` | `decimal(14,2)` | sim |
| 28 | `VP_PL_MODELO_JD` | `varchar(20)` | sim |
| 29 | `VP_PL_PRECO_JD` | `decimal(14,2)` | sim |

### IV_Q$VP_PULVERIZADOR

`view` · `26 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VP_PV_TIPO_EQUIP` | `varchar(20)` | sim |
| 19 | `VP_PV_MARCA_CONC` | `varchar(20)` | sim |
| 20 | `VP_PV_MODELO_CONC` | `varchar(20)` | sim |
| 21 | `VP_PV_QTDE_CONC` | `varchar(5)` | sim |
| 22 | `VP_PV_TECNOLOGIA` | `varchar(20)` | sim |
| 23 | `VP_PV_ITENS_TECNO` | `varchar(350)` | sim |
| 24 | `VP_PV_PRECO_CONC` | `decimal(14,2)` | sim |
| 25 | `VP_PV_MODELO_JD` | `varchar(20)` | sim |
| 26 | `VP_PV_PRECO_JD` | `decimal(14,2)` | sim |

### IV_Q$VP_RENOVACAO_SEGURO

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `SEGURADORA` | `varchar(30)` | sim |
| 19 | `PRECO` | `decimal(14,2)` | sim |
| 20 | `MOTIVO` | `varchar(30)` | sim |

### IV_Q$VP_SEM_PARTICIPACAO

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `TIPO_CONCORRENTE` | `varchar(20)` | sim |
| 19 | `MARCA_CONCORRENTE` | `varchar(20)` | sim |
| 20 | `MODELO_CONCORRENTE` | `varchar(20)` | sim |
| 21 | `COD_MODELO_CONCORREN` | `varchar(20)` | sim |
| 22 | `QUANTIDADE_VP` | `decimal(8,0)` | sim |
| 23 | `REVENDA_VP` | `varchar(20)` | sim |
| 24 | `DATA_VP` | `datetime` | sim |
| 25 | `PRECO_VP` | `decimal(14,2)` | sim |
| 26 | `PRECO_JD_VP` | `decimal(14,2)` | sim |
| 27 | `MODELO_JD_VP` | `varchar(20)` | sim |
| 28 | `MOTIVO_VP` | `varchar(40)` | sim |

### IV_Q$VP_TRATOR

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `Pessoa` | `varchar(100)` | sim |
| 3 | `SeqQuestionario` | `numeric(18,0)` | **nao** |
| 4 | `SeqFormulario` | `numeric(18,0)` | **nao** |
| 5 | `Formulario` | `varchar(20)` | sim |
| 6 | `DtaRealizacao` | `datetime` | sim |
| 7 | `UsuInclusao` | `varchar(20)` | sim |
| 8 | `DtaAlteracao` | `datetime` | sim |
| 9 | `UsuAlteracao` | `varchar(20)` | sim |
| 10 | `Observacao` | `varchar(4000)` | sim |
| 11 | `SeqHistorico` | `float(53,0)` | sim |
| 12 | `Resultado` | `decimal(6,0)` | sim |
| 13 | `Processo` | `decimal(15,0)` | sim |
| 14 | `Departamento` | `varchar(12)` | sim |
| 15 | `LinkDocto` | `varchar(10)` | sim |
| 16 | `LinkNro` | `numeric(18,0)` | sim |
| 17 | `LinkSerie` | `varchar(250)` | sim |
| 18 | `VP_TIPO_EQUIPAMENTO` | `varchar(20)` | sim |
| 19 | `VP_MARCA_CONCORRENT` | `varchar(30)` | sim |
| 20 | `VP_MODELO_CONCORRENT` | `varchar(20)` | sim |
| 21 | `VP_QUANTIDADE_EQUIP` | `varchar(5)` | sim |
| 22 | `VP_VERSAO_EQUIP` | `varchar(20)` | sim |
| 23 | `VP_TRANSMISSAO` | `varchar(30)` | sim |
| 24 | `VP_TECNOLOGIA` | `varchar(20)` | sim |
| 25 | `VP_ITEM_TECNOLOGIA` | `varchar(350)` | sim |
| 26 | `VP_PRECO_CONCORRENTE` | `decimal(14,2)` | sim |
| 27 | `VP_MODELO_JD` | `varchar(20)` | sim |
| 28 | `VP_PRECO_JD` | `decimal(14,2)` | sim |

### SYSCOLUMN

`view` · `10 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TBCREATOR` | `nvarchar(128)` | sim |
| 2 | `NAME` | `nvarchar(128)` | sim |
| 3 | `TBNAME` | `nvarchar(128)` | **nao** |
| 4 | `COLNO` | `smallint(5,0)` | sim |
| 5 | `COLTYPE` | `char(8)` | sim |
| 6 | `LENGTH` | `smallint(5,0)` | **nao** |
| 7 | `NULLS` | `char(1)` | sim |
| 8 | `UPdatetimeS` | `varchar(1)` | **nao** |
| 9 | `REMARKS` | `varchar(1)` | **nao** |
| 10 | `SCALE` | `int(10,0)` | **nao** |

### SYSTABLE

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CREATOR` | `nvarchar(128)` | sim |
| 2 | `NAME` | `nvarchar(128)` | **nao** |
| 3 | `COLCOUNT` | `int(10,0)` | sim |
| 4 | `TYPE` | `char(1)` | sim |
| 5 | `REMARKS` | `varchar(1)` | **nao** |
| 6 | `PERCENTFREE` | `int(10,0)` | **nao** |
| 7 | `ID` | `int(10,0)` | **nao** |
| 8 | `DTACRIACAO` | `datetime` | **nao** |

### VBI$CONTA

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `GRUPOGESTAO` | `varchar(20)` | **nao** |
| 2 | `CODCONTA` | `varchar(20)` | **nao** |
| 3 | `IDENTIFICADOR` | `varchar(20)` | sim |
| 4 | `SEQCONTA` | `numeric(18,0)` | **nao** |
| 5 | `SEQCONTAPAI` | `numeric(18,0)` | sim |
| 6 | `NROORDEM` | `numeric(4,0)` | **nao** |
| 7 | `DESCRICAO` | `varchar(40)` | **nao** |
| 8 | `NIVEL` | `numeric(2,0)` | **nao** |
| 9 | `SINTETICA` | `int(10,0)` | **nao** |
| 10 | `TIPO` | `varchar(1)` | sim |
| 11 | `CORRGB` | `varchar(20)` | sim |
| 12 | `ORIGEM` | `varchar(6)` | **nao** |
| 13 | `CHAVEEXTERNA` | `varchar(30)` | sim |
| 14 | `SINAL` | `numeric(1,0)` | **nao** |
| 15 | `OBS` | `varchar(250)` | sim |
| 16 | `DESCRICAOFULL` | `varchar(207)` | **nao** |
| 17 | `CHAVEEXTOBS` | `varchar(100)` | sim |

### VBI$CONTAFAM

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CODCONTA` | `varchar(20)` | **nao** |
| 2 | `CONTA` | `varchar(43)` | sim |
| 3 | `ORIGEM` | `varchar(6)` | **nao** |
| 4 | `CHAVEEXTERNA` | `varchar(30)` | sim |
| 5 | `SINAL` | `numeric(1,0)` | **nao** |
| 6 | `CORRGB` | `varchar(20)` | sim |
| 7 | `NIVEL` | `numeric(2,0)` | **nao** |
| 8 | `CONTA_1` | `varchar(42)` | sim |
| 9 | `CONTA_2` | `varchar(43)` | sim |
| 10 | `CONTA_3` | `varchar(43)` | sim |
| 11 | `CONTA_4` | `varchar(43)` | sim |
| 12 | `CONTA_5` | `varchar(43)` | sim |

### ZZ_IV$NFITEM

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `idnfsaida` | `numeric(18,0)` | **nao** |
| 2 | `NRONF` | `numeric(18,0)` | sim |
| 3 | `SERIE` | `varchar(8)` | sim |
| 4 | `origem` | `varchar(20)` | sim |
| 5 | `nroempresa` | `decimal(6,0)` | **nao** |
| 6 | `iditem` | `numeric(18,0)` | **nao** |
| 7 | `codproduto` | `varchar(40)` | sim |
| 8 | `descproduto` | `varchar(100)` | sim |
| 9 | `qtdepedida` | `int(10,0)` | **nao** |
| 10 | `qtdeatendida` | `decimal(10,2)` | sim |
| 11 | `vlrunitario` | `decimal(15,2)` | sim |
| 12 | `vlrdescto` | `decimal(15,2)` | sim |
| 13 | `situacao` | `varchar(20)` | sim |
| 14 | `vlricms` | `decimal(15,2)` | sim |
| 15 | `obs` | `varchar(250)` | sim |
| 16 | `tipoitem` | `varchar(4)` | **nao** |

### ZZ_IV$NFSAIDA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDNFSAIDA` | `numeric(18,0)` | **nao** |
| 2 | `ORIGEM` | `varchar(20)` | sim |
| 3 | `NROEMPRESA` | `decimal(6,0)` | **nao** |
| 4 | `NRONF` | `numeric(18,0)` | sim |
| 5 | `SERIENF` | `varchar(8)` | sim |
| 6 | `SERIE` | `varchar(8)` | sim |
| 7 | `SEQPESSOA` | `numeric(8,0)` | sim |
| 8 | `OPERACAO` | `varchar(30)` | sim |
| 9 | `FORMAPGTO` | `varchar(25)` | sim |
| 10 | `VENDEDOR` | `varchar(80)` | sim |
| 11 | `NROVENDEDOR` | `numeric(18,0)` | sim |
| 12 | `NROPEDIDO` | `varchar(20)` | sim |
| 13 | `DTAPEDIDO` | `datetime` | sim |
| 14 | `DTAEMISSAONF` | `datetime` | sim |
| 15 | `DTAALTERACAO` | `datetime` | sim |
| 16 | `SITUACAO` | `varchar(20)` | sim |
| 17 | `USUARIO` | `varchar(20)` | sim |
| 18 | `OBS` | `varchar(250)` | sim |

### ZZ_IV$NFSCMPL

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDNFSAIDA` | `numeric(18,0)` | **nao** |
| 2 | `NRONF` | `numeric(18,0)` | sim |
| 3 | `SERIE` | `varchar(8)` | sim |
| 4 | `ORIGEM` | `varchar(20)` | sim |
| 5 | `NROEMPRESA` | `decimal(6,0)` | **nao** |
| 6 | `IDCMPL` | `decimal(4,0)` | **nao** |
| 7 | `COMPLEMENTO` | `varchar(250)` | sim |

### ZZ_IV$OSItem

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDOS` | `numeric(18,0)` | sim |
| 2 | `NROEMPRESA` | `decimal(6,0)` | sim |
| 3 | `NroOS` | `numeric(18,0)` | sim |
| 4 | `SeqItem` | `numeric(18,0)` | **nao** |
| 5 | `TipoItem` | `varchar(6)` | sim |
| 6 | `Produtivo` | `varchar(100)` | sim |
| 7 | `StatusItem` | `varchar(10)` | sim |
| 8 | `Codigo` | `varchar(40)` | sim |
| 9 | `Descricao` | `varchar(100)` | sim |
| 10 | `Qtde` | `decimal(10,2)` | sim |
| 11 | `Valor` | `decimal(26,4)` | sim |

### ZZ_IV$OSSolicitacao

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDOS` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `decimal(6,0)` | sim |
| 3 | `NROOS` | `numeric(18,0)` | sim |
| 4 | `SeqSolicitacao` | `numeric(18,0)` | **nao** |
| 5 | `Codigo` | `varchar(10)` | sim |
| 6 | `Descricao` | `varchar(250)` | sim |

### ZZ_IV$OrdemServico

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDOS` | `numeric(18,0)` | sim |
| 2 | `NROEMPRESA` | `decimal(6,0)` | sim |
| 3 | `NROOS` | `numeric(18,0)` | sim |
| 4 | `SEQPESSOA` | `numeric(8,0)` | sim |
| 5 | `NROCHASSI` | `varchar(40)` | sim |
| 6 | `PLACA` | `varchar(9)` | sim |
| 7 | `COMBUSTIVEL` | `varchar(15)` | sim |
| 8 | `CODVEICULO` | `varchar(20)` | sim |
| 9 | `MODELO` | `varchar(30)` | sim |
| 10 | `CORVEICULO` | `varchar(20)` | sim |
| 11 | `ANOFABRICACAO` | `decimal(4,0)` | sim |
| 12 | `ANOMODELO` | `decimal(4,0)` | sim |
| 13 | `DTAVENDA` | `datetime` | sim |
| 14 | `CONSULTOR` | `varchar(20)` | sim |
| 15 | `TIPOOS` | `varchar(10)` | sim |
| 16 | `DTAABERTURA` | `datetime` | sim |
| 17 | `DTAENCERRAMENTO` | `datetime` | sim |
| 18 | `DTAFECHAMENTO` | `datetime` | sim |
| 19 | `VALORLIQPECAS` | `int(10,0)` | **nao** |
| 20 | `VALORDESCPECAS` | `int(10,0)` | **nao** |
| 21 | `VALORLIQSERVICO` | `decimal(38,2)` | sim |
| 22 | `VALORDESCSERV` | `int(10,0)` | **nao** |
| 23 | `OBSERVACAO` | `varchar(250)` | sim |
| 24 | `DEPARTAMENTO` | `varchar(30)` | sim |
| 25 | `ORIGEM` | `varchar(20)` | sim |
| 26 | `NRODN` | `varchar(10)` | sim |
| 27 | `KILOMETRAGEM` | `numeric(18,0)` | sim |

### iv_w3_conglomerado

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `seqpessoa` | `numeric(10,0)` | **nao** |
| 2 | `seqprincipal` | `numeric(10,0)` | **nao** |
| 3 | `seqpessoarelacionada` | `numeric(10,0)` | **nao** |
| 4 | `tipo_relacionamento` | `varchar(19)` | **nao** |
| 5 | `link_origem` | `varchar(20)` | sim |
| 6 | `obs_relacionamento` | `varchar(127)` | sim |

### tba_clientes

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQUENCIAL` | `numeric(10,0)` | **nao** |
| 2 | `Cidade` | `varchar(50)` | sim |
| 3 | `CLIENTE` | `varchar(100)` | sim |
| 4 | `DEPTO` | `varchar(12)` | sim |
| 5 | `EMPRESA` | `varchar(30)` | **nao** |
| 6 | `CARTEIRA` | `varchar(15)` | sim |
| 7 | `RESPONSAVEL` | `varchar(20)` | sim |
| 8 | `POTENCIAL` | `varchar(12)` | sim |

### tba_usuarios

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqUsuario` | `numeric(18,0)` | **nao** |
| 2 | `CodUsuario` | `varchar(20)` | **nao** |
| 3 | `TipoUsuario` | `char(1)` | **nao** |
| 4 | `Departamento` | `varchar(20)` | sim |
| 5 | `gerente` | `varchar(20)` | sim |
| 6 | `Status` | `varchar(20)` | sim |
| 7 | `Supervisor` | `varchar(20)` | sim |

### teste_felipe

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `PROCESSO` | `numeric(18,0)` | sim |
| 2 | `PROD_GRUPO` | `varchar(11)` | sim |
| 3 | `PROD_TIPO` | `varchar(100)` | sim |
| 4 | `PROD_MARCA` | `varchar(40)` | sim |
| 5 | `PROD_MODELO` | `varchar(250)` | sim |
| 6 | `PROD_QTDE` | `decimal(10,2)` | sim |
| 7 | `PROD_VALOR` | `decimal(15,2)` | sim |
| 8 | `PROD_VENDIDO` | `varchar(3)` | **nao** |

---

## Familia `IV$*` (140 views)

### IV$ATIVREQUISITO

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `GRUPOVINC` | `varchar(10)` | sim |
| 2 | `BASE` | `varchar(18)` | **nao** |
| 3 | `DESCRICAO` | `varchar(100)` | sim |
| 4 | `VINCULO` | `varchar(5)` | **nao** |
| 5 | `SEQVINC` | `numeric(4,0)` | **nao** |
| 6 | `NROVINC` | `numeric(18,0)` | sim |
| 7 | `CODVINC` | `varchar(30)` | sim |
| 8 | `SEQPLANOATIV` | `numeric(18,0)` | **nao** |

### IV$A_BENEFICIO

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `BENEFICIO` | `varchar(30)` | sim |

### IV$A_CAMPANHA_AGRISHOW_2012

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(8,0)` | **nao** |
| 2 | `CAMPANHA_AGRISHOW_2012` | `varchar(30)` | sim |

### IV$A_COR_PREFERIDO

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(8,0)` | **nao** |
| 2 | `COR_PREFERIDO` | `varchar(30)` | sim |

### IV$A_PERFIL_CLIENTE_SERVICOS

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(8,0)` | **nao** |
| 2 | `PERFIL_CLIENTE_SERVICOS` | `varchar(30)` | sim |

### IV$A_PERSONA_JOHN_DEERE

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `PERSONA_JOHN_DEERE` | `varchar(30)` | sim |

### IV$A_POTENCIAL_CARTEIRA_PECAS

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `POTENCIAL_CARTEIRA_PECAS` | `varchar(30)` | sim |

### IV$A_PROJETO_CULTIVAR

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `PROJETO_CULTIVAR` | `varchar(30)` | sim |

### IV$A_TIME_PREFERIDO

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `TIME_PREFERIDO` | `varchar(30)` | sim |

### IV$DADOS_PESSOA_COMPLETA

`view` · `55 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `STATUS` | `char(1)` | **nao** |
| 3 | `NOMERAZAO` | `varchar(100)` | sim |
| 4 | `FANTASIA` | `varchar(50)` | sim |
| 5 | `PALAVRACHAVE` | `varchar(50)` | sim |
| 6 | `FISICAJURIDICA` | `char(1)` | sim |
| 7 | `SEXO` | `char(1)` | sim |
| 8 | `CIDADE` | `varchar(50)` | sim |
| 9 | `UF` | `varchar(2)` | sim |
| 10 | `PAIS` | `varchar(25)` | sim |
| 11 | `BAIRRO` | `varchar(50)` | sim |
| 12 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 13 | `LOGRADOURO` | `varchar(80)` | sim |
| 14 | `NROLOGRADOURO` | `varchar(10)` | sim |
| 15 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 16 | `CEP` | `varchar(12)` | sim |
| 17 | `CXPOSTAL` | `varchar(7)` | sim |
| 18 | `REFENDERECO` | `varchar(150)` | sim |
| 19 | `INSCMUNIC` | `varchar(15)` | sim |
| 20 | `INSCPRODUTOR` | `varchar(20)` | sim |
| 21 | `CNAE` | `varchar(15)` | sim |
| 22 | `FONEDDD1` | `varchar(5)` | sim |
| 23 | `FONENRO1` | `decimal(12,0)` | sim |
| 24 | `FONECMPL1` | `varchar(20)` | sim |
| 25 | `FONEDDD2` | `varchar(5)` | sim |
| 26 | `FONENRO2` | `decimal(12,0)` | sim |
| 27 | `FONECMPL2` | `varchar(20)` | sim |
| 28 | `FONEDDD3` | `varchar(5)` | sim |
| 29 | `FONENRO3` | `decimal(12,0)` | sim |
| 30 | `FONECMPL3` | `varchar(20)` | sim |
| 31 | `FAXDDD` | `varchar(5)` | sim |
| 32 | `FAXNRO` | `decimal(12,0)` | sim |
| 33 | `NROCGCCPF` | `decimal(13,0)` | sim |
| 34 | `DIGCGCCPF` | `decimal(2,0)` | sim |
| 35 | `INSCRICAORG` | `varchar(20)` | sim |
| 36 | `UFEMISSOR` | `varchar(2)` | sim |
| 37 | `ORGAOEMISSOR` | `varchar(10)` | sim |
| 38 | `DTANASCFUND` | `datetime` | sim |
| 39 | `EMAIL` | `varchar(70)` | sim |
| 40 | `HOMEPAGE` | `varchar(80)` | sim |
| 41 | `ESTADOCIVIL` | `varchar(20)` | sim |
| 42 | `ATIVIDADE` | `varchar(30)` | sim |
| 43 | `RENDAFATURAMENTO` | `varchar(30)` | sim |
| 44 | `GRAUINSTRUCAO` | `varchar(30)` | sim |
| 45 | `GRUPO` | `varchar(30)` | sim |
| 46 | `PORTE` | `varchar(30)` | sim |
| 47 | `CODREGIAO` | `varchar(15)` | sim |
| 48 | `REGIAO` | `varchar(40)` | sim |
| 49 | `CODROTA` | `varchar(15)` | sim |
| 50 | `ROTA` | `varchar(40)` | sim |
| 51 | `DEPTO` | `varchar(12)` | sim |
| 52 | `CARTEIRA` | `varchar(15)` | sim |
| 53 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 54 | `PESSOALINK` | `varchar(250)` | sim |
| 55 | `PESSOALINKORIGEM` | `varchar(20)` | sim |

### IV$DOITRES

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TIPO` | `varchar(4)` | **nao** |
| 2 | `ACAO` | `decimal(6,0)` | **nao** |
| 3 | `CODUSUARIO` | `varchar(20)` | **nao** |
| 4 | `RESULTADO` | `decimal(6,0)` | **nao** |
| 5 | `DESCRICAO` | `varchar(40)` | **nao** |
| 6 | `EXIGEDETALHE` | `int(10,0)` | **nao** |
| 7 | `TEMCOMPLEMENTO` | `int(10,0)` | **nao** |
| 8 | `NROEMPRESA` | `int(10,0)` | **nao** |
| 9 | `SEQPESSOA` | `int(10,0)` | **nao** |

### IV$FICHANEG

`view` · `68 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `SEQPESSOA` | `decimal(10,0)` | sim |
| 4 | `CODPROCESSO` | `decimal(4,0)` | sim |
| 5 | `DESCRICAO` | `varchar(1001)` | sim |
| 6 | `USURESPONSAVEL` | `varchar(20)` | sim |
| 7 | `USUINCLUSAO` | `varchar(20)` | sim |
| 8 | `DTAINCLUSAO` | `datetime` | sim |
| 9 | `USUALTERACAOPRC` | `varchar(20)` | sim |
| 10 | `DTAALTERACAOPRC` | `datetime` | sim |
| 11 | `REALIZADO` | `decimal(1,0)` | sim |
| 12 | `DTAREALIZACAO` | `datetime` | sim |
| 13 | `PERSPECTIVA` | `decimal(3,0)` | sim |
| 14 | `FASE` | `varchar(20)` | sim |
| 15 | `STATUS` | `varchar(20)` | sim |
| 16 | `DTAFASE` | `datetime` | sim |
| 17 | `DTASTATUS` | `datetime` | sim |
| 18 | `FASEORDEM` | `decimal(2,0)` | sim |
| 19 | `DTAPREVCONCLUSAO` | `datetime` | sim |
| 20 | `VALOR` | `decimal(15,2)` | sim |
| 21 | `QTDE` | `decimal(10,2)` | sim |
| 22 | `VENDEDOR` | `varchar(20)` | sim |
| 23 | `FORMAPRIMCONT` | `varchar(12)` | sim |
| 24 | `ATIVORECEPTIVO` | `char(1)` | sim |
| 25 | `MOTIVO` | `varchar(20)` | sim |
| 26 | `CAMPANHA` | `varchar(20)` | sim |
| 27 | `ORIGEM` | `varchar(20)` | sim |
| 28 | `ULTRESULTADO` | `int(10,0)` | sim |
| 29 | `DTAULTRESULTADO` | `datetime` | sim |
| 30 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 31 | `DTAPRIMRESULTADO` | `datetime` | sim |
| 32 | `PROCESSOPAI` | `numeric(18,0)` | sim |
| 33 | `PROCESSODNA` | `numeric(18,0)` | sim |
| 34 | `HISTORICOORIGEM` | `numeric(18,0)` | sim |
| 35 | `FILIAL` | `decimal(6,0)` | sim |
| 36 | `PEDVENDA` | `decimal(10,0)` | sim |
| 37 | `AVALIACAO` | `decimal(10,0)` | sim |
| 38 | `TIPOVEICULO` | `varchar(5)` | sim |
| 39 | `NOVOUSADO` | `char(1)` | sim |
| 40 | `CODMODESCOLHIDO` | `varchar(30)` | sim |
| 41 | `FAMILIAINTERESSE` | `varchar(30)` | sim |
| 42 | `MARCAINTERESSE` | `varchar(30)` | sim |
| 43 | `MODELOINTERESSE` | `varchar(40)` | sim |
| 44 | `MARCAESCOLHIDA` | `varchar(30)` | sim |
| 45 | `MODELOESCOLHIDO` | `varchar(40)` | sim |
| 46 | `INTANOMOD` | `decimal(4,0)` | sim |
| 47 | `INTANOFABR` | `decimal(4,0)` | sim |
| 48 | `COR1` | `varchar(30)` | sim |
| 49 | `COR2` | `varchar(30)` | sim |
| 50 | `CORINTERNA` | `varchar(30)` | sim |
| 51 | `NROPORTAS` | `decimal(1,0)` | sim |
| 52 | `VLRTABELA` | `decimal(15,2)` | sim |
| 53 | `VLRPROPOSTO` | `decimal(15,2)` | sim |
| 54 | `VLRENTRADA` | `decimal(15,2)` | sim |
| 55 | `VLRSEGURO` | `decimal(15,2)` | sim |
| 56 | `USADOMARCA` | `varchar(30)` | sim |
| 57 | `USADOMODELO` | `varchar(30)` | sim |
| 58 | `USADOCOR` | `varchar(30)` | sim |
| 59 | `USADOANOFABR` | `decimal(4,0)` | sim |
| 60 | `USADONROPORTAS` | `decimal(1,0)` | sim |
| 61 | `USADOVLRPRE` | `decimal(15,2)` | sim |
| 62 | `USADOOBS` | `varchar(200)` | sim |
| 63 | `USUALTERACAO` | `varchar(20)` | sim |
| 64 | `DTAALTERACAO` | `datetime` | sim |
| 65 | `USADOVLRAVALCCT` | `decimal(15,2)` | sim |
| 66 | `VLRPRESTACAO` | `decimal(15,2)` | sim |
| 67 | `QTDEPRESTACAO` | `decimal(4,0)` | sim |
| 68 | `TIPOPGTO` | `varchar(15)` | sim |

### IV$HISTORICO

`view` · `33 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQHISTORICO` | `numeric(18,0)` | **nao** |
| 2 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 3 | `CONTATO` | `varchar(20)` | sim |
| 4 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 5 | `CODUSUARIO` | `varchar(20)` | sim |
| 6 | `ACAOGERADORA` | `numeric(6,0)` | sim |
| 7 | `RESULTADO` | `decimal(6,0)` | **nao** |
| 8 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 9 | `AGENDAORIGEM` | `int(10,0)` | sim |
| 10 | `DTAREALIZACAO` | `datetime` | **nao** |
| 11 | `DETALHE` | `varchar(4000)` | sim |
| 12 | `NATUREZA` | `char(1)` | sim |
| 13 | `VENDEDOR_HIST` | `varchar(20)` | sim |
| 14 | `ULTALTERACAO` | `datetime` | sim |
| 15 | `USUALTERACAO` | `varchar(20)` | sim |
| 16 | `DEPARTAMENTO` | `varchar(18)` | sim |
| 17 | `PROCESSO` | `numeric(18,0)` | sim |
| 18 | `CODPROCESSO` | `decimal(4,0)` | sim |
| 19 | `PROCESSOPAI` | `numeric(18,0)` | sim |
| 20 | `PROCESSODNA` | `numeric(18,0)` | sim |
| 21 | `VALOR` | `decimal(15,2)` | sim |
| 22 | `QTDE` | `numeric(17,2)` | sim |
| 23 | `TEMCIENCIA` | `numeric(1,0)` | sim |
| 24 | `VENDEDOR` | `varchar(20)` | sim |
| 25 | `FORMAPRIMCONT` | `varchar(12)` | sim |
| 26 | `DURACAO` | `decimal(4,0)` | sim |
| 27 | `ATIVORECEPTIVO` | `char(1)` | sim |
| 28 | `MOTIVO` | `varchar(40)` | sim |
| 29 | `CAMPANHA` | `varchar(60)` | sim |
| 30 | `ULTRESULTADO` | `int(10,0)` | sim |
| 31 | `DTAULTRESULTADO` | `datetime` | sim |
| 32 | `ULTRESULTADOCMPL` | `varchar(20)` | sim |
| 33 | `DTAPRIMRESULTADO` | `datetime` | sim |

### IV$NFITEM

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `idnfsaida` | `numeric(18,0)` | **nao** |
| 2 | `origem` | `varchar(20)` | **nao** |
| 3 | `nroempresa` | `numeric(6,0)` | **nao** |
| 4 | `iditem` | `numeric(6,0)` | **nao** |
| 5 | `codproduto` | `varchar(50)` | sim |
| 6 | `descproduto` | `varchar(50)` | sim |
| 7 | `qtdepedida` | `int(10,0)` | **nao** |
| 8 | `qtdeatendida` | `numeric(10,3)` | sim |
| 9 | `vlrunitario` | `numeric(14,2)` | sim |
| 10 | `vlrdescto` | `numeric(14,2)` | sim |
| 11 | `situacao` | `char(1)` | sim |
| 12 | `vlricms` | `numeric(14,2)` | sim |
| 13 | `obs` | `varchar(250)` | sim |
| 14 | `tipoitem` | `varchar(4)` | **nao** |

### IV$NFSAIDA

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDNFSAIDA` | `numeric(18,0)` | **nao** |
| 2 | `ORIGEM` | `varchar(20)` | **nao** |
| 3 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 4 | `NRONF` | `numeric(18,0)` | **nao** |
| 5 | `SERIENF` | `varchar(12)` | sim |
| 6 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 7 | `OPERACAO` | `varchar(12)` | sim |
| 8 | `FORMAPGTO` | `varchar(25)` | sim |
| 9 | `VENDEDOR` | `varchar(60)` | sim |
| 10 | `NROVENDEDOR` | `numeric(18,0)` | sim |
| 11 | `NROPEDIDO` | `varchar(20)` | sim |
| 12 | `DTAPEDIDO` | `datetime` | sim |
| 13 | `DTAEMISSAONF` | `datetime` | sim |
| 14 | `DTAALTERACAO` | `datetime` | sim |
| 15 | `SITUACAO` | `char(1)` | sim |
| 16 | `USUARIO` | `varchar(20)` | sim |
| 17 | `OBS` | `varchar(250)` | sim |

### IV$NFSCMPL

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDNFSAIDA` | `numeric(18,0)` | **nao** |
| 2 | `ORIGEM` | `varchar(20)` | **nao** |
| 3 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 4 | `IDCMPL` | `numeric(4,0)` | **nao** |
| 5 | `COMPLEMENTO` | `varchar(250)` | sim |

### IV$OPERADOR

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CODUSUARIO` | `varchar(20)` | **nao** |
| 2 | `SEQUSUARIO` | `numeric(18,0)` | **nao** |
| 3 | `NIVEL` | `decimal(1,0)` | sim |
| 4 | `NOME` | `varchar(40)` | sim |
| 5 | `GRUPO` | `numeric(18,0)` | **nao** |
| 6 | `FUNCAO` | `char(1)` | sim |
| 7 | `IDENTIFICACAO` | `varchar(40)` | sim |
| 8 | `RECEBECIENCIA` | `numeric(1,0)` | sim |
| 9 | `TIPOUSUARIO` | `char(1)` | **nao** |

### IV$OPERGRUPO

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `CODUSUARIO` | `varchar(20)` | **nao** |
| 2 | `SEQUSUARIO` | `numeric(18,0)` | **nao** |
| 3 | `NIVEL` | `decimal(1,0)` | sim |
| 4 | `NOME` | `varchar(40)` | sim |
| 5 | `GRUPO` | `numeric(18,0)` | **nao** |
| 6 | `FUNCAO` | `varchar(1)` | sim |
| 7 | `TIPOUSUARIO` | `char(1)` | **nao** |
| 8 | `IDENTIFICACAO` | `varchar(40)` | sim |
| 9 | `STATUS` | `varchar(20)` | sim |
| 10 | `JUSTIFDISPON` | `varchar(100)` | sim |
| 11 | `SUPERVISOR` | `varchar(20)` | sim |

### IV$OSItem

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDOS` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `NroOS` | `numeric(18,0)` | sim |
| 4 | `SeqItem` | `numeric(18,0)` | **nao** |
| 5 | `TipoItem` | `char(1)` | sim |
| 6 | `Produtivo` | `varchar(30)` | sim |
| 7 | `StatusItem` | `char(1)` | sim |
| 8 | `Codigo` | `varchar(40)` | sim |
| 9 | `Descricao` | `varchar(100)` | sim |
| 10 | `Qtde` | `numeric(10,2)` | sim |
| 11 | `Valor` | `numeric(14,2)` | sim |

### IV$OSSolicitacao

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDOS` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `NROOS` | `numeric(18,0)` | sim |
| 4 | `SeqSolicitacao` | `numeric(18,0)` | **nao** |
| 5 | `Codigo` | `varchar(16)` | sim |
| 6 | `Descricao` | `varchar(250)` | sim |

### IV$OrdemServico

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDOS` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `NROOS` | `numeric(18,0)` | sim |
| 4 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 5 | `NROCHASSI` | `varchar(40)` | sim |
| 6 | `PLACA` | `varchar(9)` | sim |
| 7 | `COMBUSTIVEL` | `varchar(15)` | sim |
| 8 | `CODVEICULO` | `varchar(20)` | sim |
| 9 | `MODELO` | `varchar(30)` | sim |
| 10 | `CORVEICULO` | `varchar(20)` | sim |
| 11 | `ANOFABRICACAO` | `numeric(4,0)` | sim |
| 12 | `ANOMODELO` | `numeric(4,0)` | sim |
| 13 | `DTAVENDA` | `datetime` | sim |
| 14 | `CONSULTOR` | `varchar(30)` | sim |
| 15 | `TIPOOS` | `varchar(30)` | sim |
| 16 | `DTAABERTURA` | `datetime` | sim |
| 17 | `DTAENCERRAMENTO` | `datetime` | sim |
| 18 | `DTAFECHAMENTO` | `datetime` | sim |
| 19 | `VALORLIQPECAS` | `numeric(38,2)` | sim |
| 20 | `VALORLIQSERVICO` | `numeric(38,2)` | sim |
| 21 | `VALORDESCPECAS` | `numeric(38,2)` | sim |
| 22 | `VALORDESCSERV` | `numeric(38,2)` | sim |
| 23 | `OBSERVACAO` | `varchar(250)` | sim |
| 24 | `DEPARTAMENTO` | `varchar(1)` | **nao** |
| 25 | `CODORIGEM` | `varchar(40)` | sim |
| 26 | `NRODN` | `varchar(10)` | sim |
| 27 | `KILOMETRAGEM` | `numeric(8,0)` | sim |
| 28 | `ORIGEM` | `varchar(20)` | sim |

### IV$PESSOA_GEOSIGA

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(8,0)` | **nao** |
| 2 | `CLIENTE` | `varchar(60)` | **nao** |
| 3 | `ATENDENTE` | `varchar(20)` | sim |
| 4 | `ACAO` | `varchar(40)` | **nao** |
| 5 | `RESULTADO` | `varchar(40)` | **nao** |
| 6 | `DATA` | `varchar(30)` | sim |

### IV$PG_AGRICULTURA_PRECISAO

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `REVISADO` | `numeric(1,0)` | sim |
| 8 | `MODELO` | `varchar(100)` | sim |
| 9 | `TIPO` | `varchar(100)` | sim |

### IV$PG_ALCADA

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `VALOR` | `decimal(15,2)` | sim |
| 7 | `TIPO` | `varchar(100)` | sim |

### IV$PG_AMS_TOTVS

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_ANO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `ANO` | `varchar(20)` | sim |

### IV$PG_AUDITORIA

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `EMPRESA` | `varchar(20)` | sim |
| 7 | `VENDEDOR` | `varchar(20)` | sim |
| 8 | `ASSISTENTE` | `varchar(20)` | sim |

### IV$PG_CAMPANHA_DE_INCENTIV

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `VALOR` | `decimal(15,2)` | sim |
| 7 | `TIPO_DE_INCETIVO` | `varchar(100)` | sim |
| 8 | `NOME_DO_INCETIVO` | `varchar(100)` | sim |

### IV$PG_CLASSE_RESULTADO

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `CLASSE` | `varchar(20)` | sim |
| 7 | `RESULTADO` | `decimal(15,2)` | sim |

### IV$PG_COLHEDORA_CANA_TOTVS

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_COLHEDORA_DE_CANA

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `ANO` | `varchar(20)` | sim |
| 8 | `REVISADO` | `numeric(1,0)` | sim |
| 9 | `MODELO` | `varchar(100)` | sim |

### IV$PG_COLHEITADEIRA_ANO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `ANO` | `varchar(100)` | sim |

### IV$PG_COLHEITADEIRA_GRAOS

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `TIPO_DA_PLATAFORMA` | `varchar(20)` | sim |
| 8 | `SEPARADOR` | `varchar(20)` | sim |
| 9 | `ANO` | `varchar(20)` | sim |
| 10 | `REVISADO` | `numeric(1,0)` | sim |
| 11 | `MODELO` | `varchar(100)` | sim |

### IV$PG_COLHEITADEIRA_TOTVS

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_CONSORCIO

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `MODELO` | `varchar(100)` | sim |

### IV$PG_CULTURA

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `CULTURA` | `varchar(100)` | sim |

### IV$PG_EQUIPAMENTOS

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `REVISADO` | `numeric(1,0)` | sim |
| 8 | `MODELO` | `varchar(100)` | sim |
| 9 | `FAMILIA` | `varchar(100)` | sim |

### IV$PG_FAMILIA_DEPTOS

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `FAMILIA` | `varchar(20)` | sim |
| 7 | `_TIPO` | `varchar(100)` | sim |

### IV$PG_FENO_E_FORRAGEM

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `TIPO` | `varchar(20)` | sim |
| 8 | `REVISADO` | `numeric(1,0)` | sim |

### IV$PG_GREEN_SYSTEM

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `TIPO` | `varchar(20)` | sim |

### IV$PG_GRUPO_REGIONAL

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `CARTEIRA` | `varchar(20)` | sim |
| 7 | `ORDEM` | `decimal(15,2)` | sim |
| 8 | `_NOME_GRUPO` | `varchar(100)` | sim |

### IV$PG_IMPLEMENTOS

`view` · `10 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `REVISADO` | `numeric(1,0)` | sim |
| 8 | `DEMONSTRACAO` | `varchar(30)` | sim |
| 9 | `MODELO` | `varchar(100)` | sim |
| 10 | `TIPO` | `varchar(100)` | sim |

### IV$PG_IMPLEMENTOS_TOTVS

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_IMP_GREEN_SYSTEM

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `TIPO` | `varchar(20)` | sim |

### IV$PG_INST_FINANCEIRA

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `NUMERO_DO_BANCO` | `decimal(15,2)` | sim |
| 7 | `NOME` | `varchar(100)` | sim |

### IV$PG_INTEGRACAO_FAMILIA

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `FAMILIA_PROTHEUS` | `varchar(20)` | sim |
| 7 | `PROPRIEDADE_REF` | `varchar(20)` | sim |

### IV$PG_MARCAS

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `TRATOR` | `numeric(1,0)` | sim |
| 7 | `COLHEITADEIRA_DE_GRA` | `varchar(30)` | sim |
| 8 | `COLHEDORA_DE_CANA` | `varchar(30)` | sim |
| 9 | `PLANTADEIRA` | `varchar(30)` | sim |
| 10 | `AGRICULTURA_DE_PRECI` | `varchar(30)` | sim |
| 11 | `PULVERIZADOR` | `varchar(30)` | sim |
| 12 | `FENO_E_FORRAGEM` | `varchar(30)` | sim |
| 13 | `IMP_GREEN_SYSTEM` | `varchar(30)` | sim |
| 14 | `IMPLEMENTOS` | `varchar(30)` | sim |
| 15 | `OUTROS` | `varchar(30)` | sim |
| 16 | `MARCA` | `varchar(100)` | sim |

### IV$PG_MARCAS_E_FAMILIA

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(100)` | sim |
| 7 | `FAMILIA` | `varchar(100)` | sim |
| 8 | `TIPO__MAQ_OU_IMPL` | `varchar(100)` | sim |

### IV$PG_MARCA_TOTVS

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(100)` | sim |
| 7 | `DESCRICAO` | `varchar(100)` | sim |

### IV$PG_META_DEPTO

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `DEPTO` | `varchar(20)` | sim |
| 7 | `POTENCIAL` | `varchar(20)` | sim |
| 8 | `META` | `decimal(15,2)` | sim |

### IV$PG_ORIGEM_DA_RECEITA

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `GRUPO` | `varchar(20)` | sim |
| 7 | `TIPO` | `varchar(100)` | sim |

### IV$PG_PLANTADEIRA

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `ANO` | `varchar(20)` | sim |
| 8 | `NUMERO_DE_LINHAS` | `decimal(15,2)` | sim |
| 9 | `REVISADO` | `numeric(1,0)` | sim |
| 10 | `DEMONSTRACAO` | `varchar(30)` | sim |
| 11 | `MODELO` | `varchar(100)` | sim |
| 12 | `TIPO` | `varchar(100)` | sim |

### IV$PG_PLANTADEIRA_TOTVS

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_PLATAFORMA_ADICIONAL

`view` · `10 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `TIPO_DA_PLATAFORMA` | `varchar(20)` | sim |
| 8 | `TAMANHO_PLATAFORMA` | `decimal(15,2)` | sim |
| 9 | `REVISADO` | `numeric(1,0)` | sim |
| 10 | `MODELO` | `varchar(100)` | sim |

### IV$PG_POLITICA_DE_COMISSAO

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `__LINHA` | `varchar(100)` | sim |
| 7 | `_PERCENTUAL` | `varchar(100)` | sim |

### IV$PG_PRODUTOSISDIA

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_PULVERIZADOR

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `ANO` | `varchar(20)` | sim |
| 8 | `CAPAC_TANQUE_SOLUCAO` | `decimal(15,2)` | sim |
| 9 | `REVISADO` | `numeric(1,0)` | sim |
| 10 | `DEMONSTRACAO` | `varchar(30)` | sim |
| 11 | `MODELO` | `varchar(100)` | sim |

### IV$PG_PULVERIZADOR_TOTVS

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |

### IV$PG_RESPONSAVEL_TECNICO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `NOME` | `varchar(100)` | sim |

### IV$PG_RETAIL

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `VALOR` | `decimal(15,2)` | sim |
| 7 | `TIPO` | `varchar(100)` | sim |

### IV$PG_REVENDAS

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `REVENDA` | `varchar(100)` | sim |

### IV$PG_SEGURO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `NUMERO` | `decimal(15,2)` | sim |

### IV$PG_SEM_INCENTIVO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `TIPO` | `varchar(100)` | sim |

### IV$PG_STATUS_PESSOA_DEPTO

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `QTDE_MESES__MAQ` | `decimal(15,2)` | sim |
| 7 | `QTDE_MESES__PECAS` | `decimal(15,2)` | sim |
| 8 | `QTDE_MESES__SERVICO` | `decimal(15,2)` | sim |
| 9 | `QTDE_MESES__EQUIP` | `decimal(15,2)` | sim |

### IV$PG_TIPO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `TIPO` | `varchar(20)` | sim |

### IV$PG_TRATORES

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `FAIXA_DE_POTENCIA` | `varchar(20)` | sim |
| 8 | `ANO` | `varchar(20)` | sim |
| 9 | `REVISADO` | `numeric(1,0)` | sim |
| 10 | `DEMONSTRACAO` | `varchar(30)` | sim |
| 11 | `MODELO` | `varchar(100)` | sim |

### IV$PG_TRATOR_TESTE

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `FAIXA` | `varchar(20)` | sim |
| 8 | `MODELO` | `varchar(100)` | sim |

### IV$PG_TRATOR_TOTVS

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(100)` | sim |
| 7 | `MODELO` | `varchar(100)` | sim |
| 8 | `TIPO` | `varchar(100)` | sim |

### IV$PG_TURF

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `REVISADO` | `numeric(1,0)` | sim |
| 8 | `MODELO` | `varchar(100)` | sim |
| 9 | `TIPO` | `varchar(100)` | sim |

### IV$PG_VENDEDOR

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPar` | `numeric(18,0)` | **nao** |
| 2 | `Parametro` | `varchar(20)` | **nao** |
| 3 | `NroEmpresa` | `varchar(18)` | sim |
| 4 | `DtaAlteracao` | `datetime` | sim |
| 5 | `UsuAlteracao` | `varchar(20)` | sim |
| 6 | `NOME_VENDEDOR` | `varchar(100)` | sim |

### IV$PROCESSO

`view` · `35 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `SEQPESSOA` | `decimal(10,0)` | sim |
| 4 | `CODPROCESSO` | `decimal(4,0)` | sim |
| 5 | `PROCESSOPAI` | `numeric(18,0)` | sim |
| 6 | `PROCESSODNA` | `numeric(18,0)` | sim |
| 7 | `RESUMO` | `varchar(100)` | sim |
| 8 | `DESCRICAO` | `varchar(4000)` | sim |
| 9 | `USURESPONSAVEL` | `varchar(20)` | sim |
| 10 | `USUINCLUSAO` | `varchar(20)` | sim |
| 11 | `DTAINCLUSAO` | `datetime` | sim |
| 12 | `USUALTERACAO` | `varchar(20)` | sim |
| 13 | `DTAALTERACAO` | `datetime` | sim |
| 14 | `REALIZADO` | `decimal(1,0)` | sim |
| 15 | `DTAREALIZACAO` | `datetime` | sim |
| 16 | `STATUS` | `varchar(20)` | sim |
| 17 | `PERSPECTIVA` | `decimal(3,0)` | sim |
| 18 | `FASE` | `varchar(20)` | sim |
| 19 | `FASEORDEM` | `decimal(2,0)` | sim |
| 20 | `DTAPREVCONCLUSAO` | `datetime` | sim |
| 21 | `VALOR` | `decimal(15,2)` | sim |
| 22 | `QTDE` | `decimal(10,2)` | sim |
| 23 | `VENDEDOR` | `varchar(20)` | sim |
| 24 | `FORMAPRIMCONT` | `varchar(12)` | sim |
| 25 | `ATIVORECEPTIVO` | `char(1)` | sim |
| 26 | `MOTIVO` | `varchar(40)` | sim |
| 27 | `CAMPANHA` | `varchar(60)` | sim |
| 28 | `ULTRESULTADO` | `int(10,0)` | sim |
| 29 | `DTAULTRESULTADO` | `datetime` | sim |
| 30 | `RESULTADOCMPL` | `varchar(150)` | sim |
| 31 | `DTAPRIMRESULTADO` | `datetime` | sim |
| 32 | `STATUSDESC` | `varchar(150)` | sim |
| 33 | `TIPOPROCESSO` | `varchar(10)` | sim |
| 34 | `MODELOPROCESSO` | `varchar(15)` | sim |
| 35 | `MODELODESCRICAO` | `varchar(30)` | sim |

### IV$PRODUTO

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `NROEMPRESA` | `int(10,0)` | **nao** |
| 2 | `SEQPRODUTO` | `numeric(18,0)` | **nao** |
| 3 | `CODPRODUTO` | `varchar(214)` | sim |
| 4 | `DESCRICAO` | `varchar(243)` | sim |
| 5 | `FAMILIA` | `varchar(20)` | **nao** |
| 6 | `MARCA` | `varchar(20)` | sim |
| 7 | `MODELO` | `varchar(100)` | sim |
| 8 | `PRECO1` | `numeric(2,2)` | **nao** |
| 9 | `EMUSO` | `varchar(1)` | **nao** |
| 10 | `CATEGORIA` | `varchar(1)` | **nao** |
| 11 | `NEGOCIO` | `varchar(1)` | **nao** |
| 12 | `CODFAMILIA` | `varchar(1)` | **nao** |
| 13 | `NOVO` | `varchar(1)` | **nao** |
| 14 | `USADO` | `varchar(1)` | **nao** |
| 15 | `CHASSI` | `varchar(1)` | **nao** |
| 16 | `ESTRUTURA` | `varchar(1)` | **nao** |
| 17 | `OBS` | `varchar(1)` | **nao** |

### IV$P_AGRICULTURA_PRECISAO

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |

### IV$P_CNAE

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |

### IV$P_COLHEDORA_DE_CANA

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `varchar(40)` | sim |

### IV$P_COLHEITADEIRA_GRAOS

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `SEPARADOR` | `varchar(40)` | sim |
| 14 | `TIPO_DA_PLATAFORMA` | `varchar(40)` | sim |
| 15 | `ANO` | `varchar(40)` | sim |
| 16 | `QUANTIDADE` | `varchar(40)` | sim |
| 17 | `TAM_PLATAFORMA__L_P` | `decimal(15,2)` | sim |

### IV$P_CONTRATO_GFC

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `HORIMETRO` | `decimal(15,2)` | sim |
| 13 | `DATA_DE_ATUALIZACAO` | `datetime` | sim |

### IV$P_DISTRIBUIDORA

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |

### IV$P_EQUIPAMENTOS

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `FAMILIA` | `varchar(30)` | sim |
| 13 | `MODELO` | `varchar(30)` | sim |
| 14 | `NOVO_USADO` | `varchar(30)` | sim |
| 15 | `ANO` | `varchar(30)` | sim |

### IV$P_EQUIPAMENTOS111

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `FAMILIA` | `varchar(30)` | sim |
| 13 | `MODELO` | `varchar(30)` | sim |
| 14 | `NOVO_USADO` | `varchar(30)` | sim |
| 15 | `ANO` | `varchar(30)` | sim |

### IV$P_EQUIPAMENTOS_MANITOU

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `FAMILIA` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `NOVO_USADO` | `varchar(40)` | sim |
| 15 | `ANO` | `varchar(40)` | sim |

### IV$P_EQUIPAMENTOS_NOVO

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `FAMILIA` | `varchar(30)` | sim |
| 13 | `MODELO` | `varchar(30)` | sim |
| 14 | `NOVO_USADO` | `varchar(30)` | sim |
| 15 | `ANO` | `varchar(30)` | sim |

### IV$P_EQUIPAMENTO_SISDIA

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(30)` | sim |
| 13 | `COMBUSTIVEL` | `varchar(30)` | sim |
| 14 | `ESTADO_DA_COMPRA` | `varchar(30)` | sim |
| 15 | `FORMA_DE_COMPRA` | `varchar(30)` | sim |
| 16 | `FORMA_DE_NEGOCIACAO` | `varchar(30)` | sim |
| 17 | `ANO_MODELO` | `decimal(15,2)` | sim |
| 18 | `ANO_FABRICACAO` | `decimal(15,2)` | sim |
| 19 | `VALOR` | `decimal(15,2)` | sim |
| 20 | `DATA_DA_VENDA` | `datetime` | sim |
| 21 | `DATA_PREV_QUITACAO` | `datetime` | sim |
| 22 | `REVENDA` | `varchar(40)` | sim |
| 23 | `VENDEDOR` | `varchar(40)` | sim |
| 24 | `FINANCIADOR` | `varchar(40)` | sim |
| 25 | `USO_DO_VEICULO` | `varchar(40)` | sim |
| 26 | `COR` | `varchar(40)` | sim |
| 27 | `PLACA` | `varchar(40)` | sim |

### IV$P_FENO_E_FORRAGEM

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |

### IV$P_FROTA

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |

### IV$P_IMPLEMENTO

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |

### IV$P_LOCACAO

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MARCA` | `varchar(30)` | sim |
| 13 | `FAMILIA` | `varchar(30)` | sim |
| 14 | `MODELO` | `varchar(30)` | sim |
| 15 | `VALOR_MENSAL` | `decimal(15,2)` | sim |
| 16 | `VALOR_TOTAL` | `decimal(15,2)` | sim |
| 17 | `INICIO_CONTRATO` | `datetime` | sim |
| 18 | `RENOVACAO` | `datetime` | sim |
| 19 | `TERMINO_CONTRATO` | `datetime` | sim |
| 20 | `MARCA8` | `varchar(40)` | sim |
| 21 | `FAMILIA9` | `varchar(40)` | sim |
| 22 | `MODELO10` | `varchar(40)` | sim |
| 23 | `N__CONTRATO` | `varchar(40)` | sim |

### IV$P_NJUR

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |

### IV$P_NO_AGRICULTURA_PREC

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |

### IV$P_NO_AREA_PLANTADA

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO_CULTURA` | `varchar(40)` | sim |
| 13 | `HECTARES` | `decimal(15,2)` | sim |
| 14 | `CIDADE` | `varchar(40)` | sim |

### IV$P_NO_AREA_TOTAL

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `AREA_IRRIG_GOTEJO__H` | `decimal(15,2)` | sim |
| 13 | `AREA_PECUARIA__HA` | `decimal(15,2)` | sim |
| 14 | `AREA_GRAOS__HA` | `decimal(15,2)` | sim |
| 15 | `AREA_ALGODAO__HA` | `decimal(15,2)` | sim |
| 16 | `AREA_CANA__HA` | `decimal(15,2)` | sim |
| 17 | `AREA_IRRGI_ASPERS__H` | `decimal(15,2)` | sim |
| 18 | `QTDE_ARRENDADA__HA` | `varchar(40)` | sim |
| 19 | `QTDE_PROPRIA__HA` | `varchar(40)` | sim |

### IV$P_NO_CHACARA

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `ENDERECO` | `varchar(40)` | sim |
| 13 | `MUNICIPIO` | `varchar(40)` | sim |
| 14 | `CNPJ` | `varchar(40)` | sim |
| 15 | `IE` | `varchar(40)` | sim |

### IV$P_NO_COLHEDORA_DE_CAN

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |

### IV$P_NO_COLHEITADEIRA_GR

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `SEPARADOR` | `varchar(40)` | sim |
| 14 | `TIPO_DA_PLATAFORMA` | `varchar(40)` | sim |
| 15 | `ANO` | `varchar(40)` | sim |
| 16 | `QUANTIDADE` | `decimal(15,2)` | sim |
| 17 | `TAM_PLATAFORMA__L_P` | `decimal(15,2)` | sim |

### IV$P_NO_CONSORCIO

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |

### IV$P_NO_ESTANCIA

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `ENDERECO` | `varchar(40)` | sim |
| 13 | `MUNICIPIO` | `varchar(40)` | sim |
| 14 | `CNPJ` | `varchar(40)` | sim |
| 15 | `I_E` | `varchar(40)` | sim |

### IV$P_NO_FAZENDA

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `NOME_DA_FAZENDA` | `varchar(40)` | sim |
| 13 | `CNPJ` | `varchar(40)` | sim |
| 14 | `MUNICIPIO` | `varchar(40)` | sim |
| 15 | `I_E` | `varchar(40)` | sim |
| 16 | `AREA` | `varchar(40)` | sim |

### IV$P_NO_FENO_E_FORRAGEM

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |

### IV$P_NO_FROTA

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `POTENCIA` | `varchar(40)` | sim |
| 15 | `QUANTIDADE` | `varchar(40)` | sim |

### IV$P_NO_IMOVEL

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MARCA` | `varchar(40)` | sim |
| 13 | `SEGURADO` | `numeric(1,0)` | sim |

### IV$P_NO_IMPLEMENTO

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |
| 15 | `MODELO` | `varchar(40)` | sim |

### IV$P_NO_IMPLEMENTOS

`view` · `20 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `AGENTE` | `varchar(40)` | sim |
| 13 | `LINHA` | `varchar(40)` | sim |
| 14 | `EVENTO` | `varchar(40)` | sim |
| 15 | `STATUS` | `varchar(40)` | sim |
| 16 | `VALOR` | `decimal(15,2)` | sim |
| 17 | `COND__PAGTO` | `decimal(15,2)` | sim |
| 18 | `MODELO` | `varchar(40)` | sim |
| 19 | `MARCA` | `varchar(40)` | sim |
| 20 | `SERIE` | `varchar(40)` | sim |

### IV$P_NO_JOHN_DEERE

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `COMBUSTIVERL` | `varchar(40)` | sim |
| 14 | `NOVO_USADO` | `varchar(40)` | sim |
| 15 | `FORMA_DE_COMPRA` | `varchar(40)` | sim |
| 16 | `ANO_MODELO` | `decimal(15,2)` | sim |
| 17 | `ANO_FABRICACAO` | `decimal(15,2)` | sim |
| 18 | `VALOR_COMPRA` | `decimal(15,2)` | sim |
| 19 | `DATA_VENDA` | `datetime` | sim |
| 20 | `REVENDA` | `varchar(40)` | sim |
| 21 | `VENDEDOR` | `varchar(40)` | sim |
| 22 | `COR` | `varchar(40)` | sim |
| 23 | `FROTA` | `varchar(40)` | sim |

### IV$P_NO_MAQUINA_IMPLEMENT

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MARCA` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `CODMODELO` | `varchar(40)` | sim |
| 15 | `NOVO_USADO` | `varchar(40)` | sim |
| 16 | `SEGURADO` | `numeric(1,0)` | sim |
| 17 | `ANO_FABRIC` | `decimal(15,2)` | sim |
| 18 | `HORIMETRO` | `decimal(15,2)` | sim |
| 19 | `FROTA` | `varchar(40)` | sim |

### IV$P_NO_ORIGEM_DA_RECEIT

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `PESO` | `varchar(40)` | sim |
| 14 | `TIPO_DE_AREA` | `varchar(40)` | sim |
| 15 | `AREA_HA` | `decimal(15,2)` | sim |

### IV$P_NO_PLANTADEIRA

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `ANO` | `varchar(40)` | sim |
| 15 | `QUANTIDADE` | `decimal(15,2)` | sim |
| 16 | `N__DE_LINHAS` | `decimal(15,2)` | sim |

### IV$P_NO_PLATAFORMA_ADICI

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |
| 15 | `TAM_PLATAFORMA__L_P` | `decimal(15,2)` | sim |

### IV$P_NO_PROPRIEDADE_RURAL

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `NOME` | `varchar(40)` | sim |
| 13 | `ENDERECO` | `varchar(40)` | sim |
| 14 | `I_E` | `varchar(40)` | sim |
| 15 | `CNPJ` | `varchar(40)` | sim |
| 16 | `CEP` | `varchar(40)` | sim |
| 17 | `CIDADE` | `varchar(40)` | sim |

### IV$P_NO_PULVERIZADOR

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |
| 15 | `CAP__TANQUE_SOLUCAO` | `decimal(15,2)` | sim |

### IV$P_NO_SEGURO_DE_VIDA

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MARCA` | `varchar(40)` | sim |
| 13 | `SEGURADO` | `numeric(1,0)` | sim |

### IV$P_NO_SITIO

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `NOME` | `varchar(40)` | sim |
| 13 | `MUNICIPIO` | `varchar(40)` | sim |
| 14 | `CNPJ` | `varchar(40)` | sim |
| 15 | `IE` | `varchar(40)` | sim |

### IV$P_NO_TIPO_CULTURA

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `PESO_CULTURA` | `varchar(40)` | sim |

### IV$P_NO_TRATOR

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `FAIXA_DE_POTENCIA` | `varchar(40)` | sim |
| 14 | `ANO` | `varchar(40)` | sim |
| 15 | `QUANTIDADE` | `decimal(15,2)` | sim |

### IV$P_NO_TRATOR_MANUAL_FRO

`view` · `17 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `COMBUSTIVEL` | `varchar(40)` | sim |
| 13 | `NOVO_USADO` | `varchar(40)` | sim |
| 14 | `FORMA_DE_COMPRA` | `varchar(40)` | sim |
| 15 | `MODELO` | `decimal(15,2)` | sim |
| 16 | `ANO_MODELO` | `decimal(15,2)` | sim |
| 17 | `ANO_FABRICACAO` | `decimal(15,2)` | sim |

### IV$P_NO_TRATOR_SISDIA

`view` · `23 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `COMBUSTIVEL` | `varchar(40)` | sim |
| 14 | `NOVO_USADO` | `varchar(40)` | sim |
| 15 | `FORMA_DE_COMPRA` | `varchar(40)` | sim |
| 16 | `ANO_MODELO` | `decimal(15,2)` | sim |
| 17 | `ANO_FABRICACAO` | `decimal(15,2)` | sim |
| 18 | `VALOR_COMPRA` | `decimal(15,2)` | sim |
| 19 | `DATA_VENDA` | `datetime` | sim |
| 20 | `REVENDA` | `varchar(40)` | sim |
| 21 | `VENDEDOR` | `varchar(40)` | sim |
| 22 | `COR` | `varchar(40)` | sim |
| 23 | `FROTA` | `varchar(40)` | sim |

### IV$P_NO_TURF

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `decimal(15,2)` | sim |

### IV$P_NO_VEICULO

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MARCA` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `CODMODELO` | `varchar(40)` | sim |
| 15 | `NOVO_USADO` | `varchar(40)` | sim |
| 16 | `SEGURADO` | `numeric(1,0)` | sim |
| 17 | `ANO_FABRIC` | `decimal(15,2)` | sim |
| 18 | `FROTA` | `varchar(40)` | sim |

### IV$P_OPERATIONS_CENTER

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `USUARIOS` | `varchar(40)` | sim |
| 13 | `SENHA` | `varchar(40)` | sim |

### IV$P_ORIGEM_DA_RECEITA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `PESO` | `varchar(40)` | sim |
| 14 | `AREA_COMPARTILHADA` | `varchar(40)` | sim |
| 15 | `AREA_PRINCIPAL` | `varchar(40)` | sim |
| 16 | `AREA_HA` | `decimal(15,2)` | sim |
| 17 | `TERMINO_ARRENDAMENTO` | `datetime` | sim |
| 18 | `TIPO_DE_PROPRIEDADE` | `varchar(40)` | sim |

### IV$P_PLANTADEIRA

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `ANO` | `varchar(40)` | sim |
| 15 | `QUANTIDADE` | `varchar(40)` | sim |
| 16 | `N__DE_LINHAS` | `decimal(15,2)` | sim |

### IV$P_PLATAFORMA_ADICIONAL

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `ANO` | `varchar(40)` | sim |
| 15 | `TAM_PLATAFORMA__L_P` | `decimal(15,2)` | sim |

### IV$P_PULVERIZADOR

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `MODELO` | `varchar(40)` | sim |
| 13 | `ANO` | `varchar(40)` | sim |
| 14 | `QUANTIDADE` | `varchar(40)` | sim |
| 15 | `CAP__TANQUE_SOLUCAO` | `decimal(15,2)` | sim |

### IV$P_SEGURO

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `SEGURADORA` | `varchar(40)` | sim |
| 13 | `CORRETORA` | `varchar(40)` | sim |
| 14 | `CONDICAO_PAGAMENTO` | `varchar(40)` | sim |
| 15 | `APOLICE` | `decimal(15,2)` | sim |
| 16 | `COBERTURA_INICIAL` | `datetime` | sim |
| 17 | `COBERTURA_FINAL` | `datetime` | sim |
| 18 | `FATURAMENTO_MAQUINA` | `datetime` | sim |
| 19 | `VALOR_COBERTURA` | `varchar(40)` | sim |
| 20 | `VALOR_SEGURO` | `varchar(40)` | sim |
| 21 | `EQUIPAMENTO` | `varchar(40)` | sim |
| 22 | `CHASSI` | `varchar(40)` | sim |
| 23 | `VENDEDOR_SEGURO` | `varchar(40)` | sim |
| 24 | `VALOR_DO_BEM` | `varchar(40)` | sim |
| 25 | `ANO_DO_BEM` | `varchar(40)` | sim |
| 26 | `PARCELAS` | `varchar(40)` | sim |
| 27 | `__DE_COMISSAO` | `varchar(40)` | sim |

### IV$P_TRATOR

`view` · `16 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `FAMILIA` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |
| 14 | `FAIXA_DE_POTENCIA` | `varchar(40)` | sim |
| 15 | `ANO` | `varchar(40)` | sim |
| 16 | `QUANTIDADE` | `varchar(40)` | sim |

### IV$P_TURF

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** |
| 2 | `SeqPropriedade` | `numeric(4,0)` | **nao** |
| 3 | `Referencia` | `varchar(30)` | sim |
| 4 | `Ativo` | `char(1)` | sim |
| 5 | `Notas` | `varchar(250)` | sim |
| 6 | `CodOrigem` | `varchar(20)` | sim |
| 7 | `DtaInclusao` | `datetime` | sim |
| 8 | `UsuInclusao` | `varchar(20)` | sim |
| 9 | `DtaAlteracao` | `datetime` | sim |
| 10 | `UsuAlteracao` | `varchar(20)` | sim |
| 11 | `Identificador` | `varchar(30)` | sim |
| 12 | `TIPO` | `varchar(40)` | sim |
| 13 | `MODELO` | `varchar(40)` | sim |

### IV$RESULTADO

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `resultado` | `decimal(6,0)` | **nao** |
| 2 | `acao` | `decimal(6,0)` | sim |
| 3 | `descricao` | `varchar(40)` | **nao** |
| 4 | `descreduzida` | `varchar(20)` | **nao** |
| 5 | `ordem` | `decimal(3,0)` | sim |
| 6 | `descricaofull` | `varchar(83)` | **nao** |
| 7 | `descredfull` | `varchar(43)` | **nao** |
| 8 | `acaodescricao` | `varchar(40)` | **nao** |
| 9 | `acaodescreduzida` | `varchar(20)` | **nao** |
| 10 | `emuso` | `numeric(1,0)` | sim |
| 11 | `temcomplemento` | `varchar(1)` | **nao** |
| 12 | `ctrlcomplemento` | `numeric(1,0)` | sim |
| 13 | `codprocesso` | `decimal(4,0)` | sim |
| 14 | `tipoprocesso` | `varchar(15)` | sim |

### IV$RESULTADOFULL

`view` · `76 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `RESULTADO` | `decimal(6,0)` | **nao** |
| 2 | `ACAO` | `decimal(6,0)` | sim |
| 3 | `DESCRICAO` | `varchar(40)` | **nao** |
| 4 | `DESCREDUZIDA` | `varchar(20)` | **nao** |
| 5 | `ORDEM` | `decimal(3,0)` | sim |
| 6 | `DESCRICAOFULL` | `varchar(83)` | **nao** |
| 7 | `DESCREDFULL` | `varchar(43)` | **nao** |
| 8 | `ACAODESCRICAO` | `varchar(40)` | **nao** |
| 9 | `DESCREDUZIDAACAO` | `varchar(20)` | **nao** |
| 10 | `EMUSO` | `numeric(1,0)` | sim |
| 11 | `CODPROCESSO` | `decimal(4,0)` | sim |
| 12 | `TIPOPROCESSO` | `varchar(15)` | sim |
| 13 | `ADVERTENCIA` | `varchar(200)` | sim |
| 14 | `ASSUNTOCMPL` | `varchar(40)` | sim |
| 15 | `ASSUNTOEMAIL` | `varchar(40)` | sim |
| 16 | `CANALPADRAO` | `varchar(20)` | sim |
| 17 | `CMPLTTXTPADRAO` | `varchar(250)` | sim |
| 18 | `DESCQTDE` | `varchar(20)` | sim |
| 19 | `DESCVALOR` | `varchar(20)` | sim |
| 20 | `PCTE` | `varchar(4)` | sim |
| 21 | `QTDEMAXPORACAO` | `decimal(2,0)` | sim |
| 22 | `CTRLINTERATIVO` | `numeric(1,0)` | sim |
| 23 | `CTRLCONCLUSAO` | `numeric(1,0)` | sim |
| 24 | `CTRLDETALHE` | `numeric(1,0)` | sim |
| 25 | `CTRLFORMACONTATO` | `numeric(1,0)` | sim |
| 26 | `CTRLREAGENDA` | `numeric(1,0)` | sim |
| 27 | `CTRLCONFIRMAREAG` | `numeric(1,0)` | sim |
| 28 | `CTRLREAGENDASILO` | `numeric(1,0)` | sim |
| 29 | `CTRLMUDARATDREAG` | `numeric(1,0)` | sim |
| 30 | `CTRLFORMULARIO` | `numeric(1,0)` | sim |
| 31 | `CTRLDURACAO` | `numeric(1,0)` | sim |
| 32 | `CTRLVALOR` | `numeric(1,0)` | sim |
| 33 | `CTRLPRODUTIVO` | `numeric(1,0)` | sim |
| 34 | `CTRLPRODUTO` | `numeric(1,0)` | sim |
| 35 | `CTRLVENDEDOR` | `numeric(1,0)` | sim |
| 36 | `CTRLMOTIVO` | `numeric(1,0)` | sim |
| 37 | `CTRLDEPARTAMENTO` | `numeric(1,0)` | sim |
| 38 | `CTRLCAMPANHA` | `numeric(1,0)` | sim |
| 39 | `CTRLCONTATOPF` | `numeric(1,0)` | sim |
| 40 | `CTRLCONTATOPJ` | `numeric(1,0)` | sim |
| 41 | `CTRLCLIENTEATIVO` | `numeric(1,0)` | sim |
| 42 | `CTRLDETALHEPROC` | `numeric(1,0)` | sim |
| 43 | `CTRLDESCRSTATUSPROC` | `numeric(1,0)` | sim |
| 44 | `CTRLSTATUSPROC` | `numeric(1,0)` | sim |
| 45 | `CTRLPERSPECTIVAPROC` | `numeric(1,0)` | sim |
| 46 | `CTRLVALORPROC` | `numeric(1,0)` | sim |
| 47 | `CTRLDTAENCERRAPROC` | `numeric(1,0)` | sim |
| 48 | `CTRLPROJETO` | `numeric(1,0)` | sim |
| 49 | `CTRLIMAGEM` | `numeric(1,0)` | sim |
| 50 | `CTRLCTIDISPONIVEL` | `numeric(1,0)` | sim |
| 51 | `CTRLRESUMOPROC` | `numeric(1,0)` | sim |
| 52 | `INDCOMPLEMENTO` | `int(10,0)` | **nao** |
| 53 | `INDPERMCONCLUIR` | `int(10,0)` | **nao** |
| 54 | `INDCONCLUI` | `int(10,0)` | **nao** |
| 55 | `INDINCLMANUAL` | `int(10,0)` | **nao** |
| 56 | `INDPERMREAGENDAR` | `int(10,0)` | **nao** |
| 57 | `INDPERMEMAIL` | `int(10,0)` | **nao** |
| 58 | `RQPESSOAATIVA` | `int(10,0)` | **nao** |
| 59 | `RQAGENDACONF` | `int(10,0)` | **nao** |
| 60 | `INDCOMPLEMENTOSQL` | `int(10,0)` | **nao** |
| 61 | `INDPRODUTIVO` | `int(10,0)` | **nao** |
| 62 | `EXIGEDETALHE` | `varchar(1)` | **nao** |
| 63 | `CONCLUSIVO` | `varchar(1)` | **nao** |
| 64 | `FORCACONCLUSAO` | `varchar(1)` | **nao** |
| 65 | `RECEPTIVO` | `varchar(1)` | **nao** |
| 66 | `ATIVO` | `varchar(1)` | **nao** |
| 67 | `IDENTSITUACAO` | `varchar(1)` | **nao** |
| 68 | `EXIGEVENDEDOR` | `varchar(1)` | **nao** |
| 69 | `EXIGEMOTIVO` | `varchar(1)` | **nao** |
| 70 | `EXIGEPRODUTO` | `varchar(1)` | **nao** |
| 71 | `EXIGEDEPTO` | `varchar(1)` | **nao** |
| 72 | `EXIGECAMPANHA` | `varchar(1)` | **nao** |
| 73 | `EMUSOX` | `varchar(1)` | **nao** |
| 74 | `EXIGECLIENTEATIVO` | `varchar(1)` | **nao** |
| 75 | `EXIGEQTDE` | `varchar(1)` | **nao** |
| 76 | `EXIGEVALOR` | `varchar(1)` | **nao** |

### IV$S_AGENDA

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 2 | `ACAO` | `varchar(40)` | **nao** |
| 3 | `ACAO_COD` | `decimal(6,0)` | sim |
| 4 | `DATA_AGENDA` | `datetime` | **nao** |
| 5 | `DATA_AGENORIGINAL` | `datetime` | sim |
| 6 | `REALIZADA` | `char(1)` | sim |
| 7 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 8 | `ATENDENTE` | `varchar(20)` | sim |
| 9 | `CLASSE` | `char(1)` | **nao** |
| 10 | `PRIORIDADE` | `decimal(1,0)` | **nao** |
| 11 | `GERADA_POR` | `varchar(20)` | sim |
| 12 | `TAREFA_COMPROMISSO` | `char(1)` | sim |
| 13 | `PERSPECTIVA` | `varchar(20)` | sim |

### IV$S_CARTEIRA

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NOMERAZAO` | `varchar(100)` | sim |
| 3 | `FISICAJURIDICA` | `char(1)` | sim |
| 4 | `CIDADE` | `varchar(50)` | sim |
| 5 | `UF` | `varchar(2)` | sim |
| 6 | `BAIRRO` | `varchar(50)` | sim |
| 7 | `ATIVIDADE` | `varchar(30)` | sim |
| 8 | `INCL_CLIENTE` | `char(30)` | sim |
| 9 | `CARTEIRA` | `varchar(15)` | sim |
| 10 | `CART_DESCRICAO` | `varchar(40)` | sim |
| 11 | `POTENCIAL` | `varchar(12)` | sim |
| 12 | `CODUSUARIO` | `varchar(20)` | **nao** |
| 13 | `DEPTO_DESCRICAO` | `varchar(30)` | sim |
| 14 | `DEPTO` | `varchar(12)` | sim |
| 15 | `USRRESP` | `varchar(20)` | **nao** |

### IV$S_CONTATO

`view` · `33 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `SEQ_PESSOA` | `numeric(10,0)` | **nao** |
| 3 | `EMUSO` | `varchar(3)` | **nao** |
| 4 | `TIPOCONTATO` | `varchar(30)` | sim |
| 5 | `AREAATUACAO` | `varchar(20)` | sim |
| 6 | `RG` | `varchar(20)` | sim |
| 7 | `CPF` | `numeric(18,0)` | sim |
| 8 | `DIGCPF` | `decimal(2,0)` | sim |
| 9 | `SAUDACAO` | `varchar(20)` | sim |
| 10 | `CONTATO` | `varchar(40)` | sim |
| 11 | `FONEDDD1` | `varchar(5)` | sim |
| 12 | `FONENRO1` | `decimal(12,0)` | sim |
| 13 | `FONECMPL1` | `varchar(12)` | sim |
| 14 | `FONEDDD2` | `varchar(5)` | sim |
| 15 | `FONENRO2` | `decimal(12,0)` | sim |
| 16 | `FONECMPL2` | `varchar(12)` | sim |
| 17 | `FAXDDD` | `varchar(5)` | sim |
| 18 | `FAXNRO` | `decimal(12,0)` | sim |
| 19 | `SEXO` | `char(1)` | sim |
| 20 | `ESTADOCIVIL` | `varchar(13)` | **nao** |
| 21 | `DTANASCIMENTO` | `datetime` | sim |
| 22 | `ATRIBUTO1` | `varchar(20)` | sim |
| 23 | `ATRIBUTO2` | `varchar(20)` | sim |
| 24 | `ATRIBUTO3` | `varchar(20)` | sim |
| 25 | `ATRIBDTA` | `datetime` | sim |
| 26 | `ATRIBNUM` | `numeric(18,0)` | sim |
| 27 | `EMAIL` | `varchar(50)` | sim |
| 28 | `OBSERVACAO` | `varchar(250)` | sim |
| 29 | `LINKWEB` | `numeric(1,0)` | sim |
| 30 | `PAPEL` | `varchar(20)` | sim |
| 31 | `DIA_NASC` | `int(10,0)` | sim |
| 32 | `MES_NASC` | `int(10,0)` | sim |
| 33 | `ANO_NASC` | `int(10,0)` | sim |

### IV$S_ENDERECOADIC

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `TIPOENDERECO` | `varchar(15)` | **nao** |
| 3 | `SEQCIDADE` | `decimal(6,0)` | sim |
| 4 | `CIDADE` | `varchar(50)` | sim |
| 5 | `UF` | `varchar(2)` | sim |
| 6 | `SEQBAIRRO` | `decimal(5,0)` | sim |
| 7 | `BAIRRO` | `varchar(50)` | sim |
| 8 | `TIPOLOGRADOURO` | `varchar(15)` | sim |
| 9 | `LOGRADOURO` | `varchar(80)` | sim |
| 10 | `NROLOGRADOURO` | `varchar(10)` | sim |
| 11 | `CMPLTOLOGRADOURO` | `varchar(30)` | sim |
| 12 | `CXPOSTAL` | `varchar(7)` | sim |
| 13 | `CEP` | `varchar(12)` | sim |

### IV$S_HISTORICO

`view` · `35 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `CONTATO` | `varchar(20)` | sim |
| 3 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 4 | `ATENDENTE` | `varchar(20)` | sim |
| 5 | `ACAO_GERADORA` | `varchar(40)` | **nao** |
| 6 | `COD_ACAO_GERADORA` | `numeric(6,0)` | sim |
| 7 | `COD_RESULTADO` | `decimal(6,0)` | **nao** |
| 8 | `RESULTADO` | `varchar(72)` | sim |
| 9 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 10 | `AGENDAORIGEM` | `int(10,0)` | sim |
| 11 | `DTAREALIZACAO` | `datetime` | **nao** |
| 12 | `DETALHE` | `varchar(4000)` | sim |
| 13 | `NATUREZA` | `char(1)` | sim |
| 14 | `VENDEDOR_HIST` | `varchar(20)` | sim |
| 15 | `ULTALTERACAO` | `datetime` | sim |
| 16 | `USUALTERACAO` | `varchar(20)` | sim |
| 17 | `DEPARTAMENTO` | `varchar(18)` | sim |
| 18 | `CODPROCESSO` | `decimal(4,0)` | sim |
| 19 | `PROCESSO` | `numeric(18,0)` | sim |
| 20 | `VALOR` | `decimal(15,2)` | sim |
| 21 | `QTDE` | `numeric(17,2)` | sim |
| 22 | `VENDEDOR` | `varchar(20)` | sim |
| 23 | `DURACAO` | `decimal(4,0)` | sim |
| 24 | `FORMA_PRIM_CONTATO` | `varchar(12)` | sim |
| 25 | `ULTRESULTADO` | `int(10,0)` | sim |
| 26 | `DTAULTRESULTADO` | `datetime` | sim |
| 27 | `ULTRESULTADOCMPL` | `varchar(20)` | sim |
| 28 | `DTAPRIMRESULTADO` | `datetime` | sim |
| 29 | `VENDEDOR_PROCESSO` | `varchar(20)` | sim |
| 30 | `COD_PRODUTO` | `varchar(50)` | sim |
| 31 | `QTDE_PROD` | `decimal(10,2)` | sim |
| 32 | `VALOR_PROD` | `decimal(15,2)` | sim |
| 33 | `CAMPANHA` | `varchar(60)` | sim |
| 34 | `ORIGEM` | `varchar(40)` | sim |
| 35 | `MOTIVO` | `varchar(40)` | sim |

### IV$S_PESSOA

`view` · `46 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `VERSAO` | `decimal(2,0)` | sim |
| 3 | `STATUS` | `varchar(8)` | **nao** |
| 4 | `NOMERAZAO` | `varchar(100)` | sim |
| 5 | `FANTASIA` | `varchar(50)` | sim |
| 6 | `FISICA_JURIDICA` | `varchar(8)` | **nao** |
| 7 | `SEXO` | `char(1)` | sim |
| 8 | `CIDADE` | `varchar(50)` | sim |
| 9 | `UF` | `varchar(2)` | sim |
| 10 | `PAIS` | `varchar(25)` | sim |
| 11 | `BAIRRO` | `varchar(50)` | sim |
| 12 | `LOGRADOURO` | `varchar(80)` | sim |
| 13 | `FONEDDD1` | `varchar(5)` | sim |
| 14 | `FONENRO1` | `decimal(12,0)` | sim |
| 15 | `FONEDDD2` | `varchar(5)` | sim |
| 16 | `FONENRO2` | `decimal(12,0)` | sim |
| 17 | `FONEDDD3` | `varchar(5)` | sim |
| 18 | `FONENRO3` | `decimal(12,0)` | sim |
| 19 | `FAXDDD` | `varchar(5)` | sim |
| 20 | `FAXNRO` | `decimal(12,0)` | sim |
| 21 | `NROCGCCPF` | `decimal(13,0)` | sim |
| 22 | `DIGCGCCPF` | `decimal(2,0)` | sim |
| 23 | `INSCRICAORG` | `varchar(20)` | sim |
| 24 | `DTANASCFUND` | `datetime` | sim |
| 25 | `ORIGEM` | `varchar(20)` | sim |
| 26 | `EMAIL` | `varchar(70)` | sim |
| 27 | `HOMEPAGE` | `varchar(80)` | sim |
| 28 | `CXPOSTAL` | `varchar(7)` | sim |
| 29 | `ESTADO_CIVIL` | `varchar(13)` | **nao** |
| 30 | `ATIVIDADE` | `varchar(30)` | sim |
| 31 | `RENDA_FATURAMENTO` | `varchar(30)` | sim |
| 32 | `GRAU_INSTRUCAO` | `varchar(30)` | sim |
| 33 | `GRUPO` | `varchar(30)` | sim |
| 34 | `DTAINCLUSAO` | `datetime` | sim |
| 35 | `USUINCLUSAO` | `varchar(20)` | sim |
| 36 | `DTAALTERACAO` | `datetime` | sim |
| 37 | `USUALTERACAO` | `varchar(20)` | sim |
| 38 | `VENDEDOR` | `varchar(20)` | sim |
| 39 | `RECEBE_TELEFONEMA` | `varchar(3)` | **nao** |
| 40 | `RECEBE_CORRESPONDENCIA` | `varchar(3)` | **nao** |
| 41 | `TEM_PROBLEMACREDITO` | `varchar(3)` | **nao** |
| 42 | `POSSUI_EMAIL` | `varchar(3)` | **nao** |
| 43 | `RECEBE_EMAIL` | `varchar(3)` | **nao** |
| 44 | `DIA_NASC` | `int(10,0)` | sim |
| 45 | `MES_NASC` | `int(10,0)` | sim |
| 46 | `ANO_NASC` | `int(10,0)` | sim |

### IV$S_PESSOA_CLASSE

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `CLASSE` | `varchar(20)` | **nao** |

### IV$S_RFV

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `RECENCIA` | `decimal(1,0)` | sim |
| 3 | `FRENQUENCIA` | `numeric(1,0)` | sim |
| 4 | `VALOR` | `numeric(1,0)` | sim |
| 5 | `CARTEIRA` | `varchar(15)` | sim |
| 6 | `POTENCIAL` | `varchar(12)` | **nao** |
| 7 | `DEPARTAMENTO` | `varchar(12)` | sim |

### IV$TITULO

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDTITULO` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 3 | `NROEMPRESACOBR` | `numeric(6,0)` | **nao** |
| 4 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 5 | `ESPECIE` | `varchar(20)` | sim |
| 6 | `NROTITULO` | `varchar(30)` | sim |
| 7 | `VLRORIGINAL` | `numeric(14,2)` | sim |
| 8 | `DTAEMISSAO` | `datetime` | sim |
| 9 | `DTAVENCTO` | `datetime` | sim |
| 10 | `QTDEDIA` | `int(10,0)` | sim |
| 11 | `QUITADO` | `numeric(1,0)` | **nao** |
| 12 | `NRODOCTO` | `varchar(30)` | sim |
| 13 | `LOCALCOBR` | `varchar(25)` | sim |
| 14 | `TIPOCOBR` | `varchar(20)` | sim |
| 15 | `VLRABERTO` | `numeric(15,2)` | sim |
| 16 | `VLRACRESCIMO` | `numeric(13,2)` | sim |
| 17 | `VLRABATIMENTO` | `numeric(14,2)` | sim |
| 18 | `VLRPAGO` | `numeric(14,2)` | sim |
| 19 | `DTAPGTO` | `datetime` | sim |
| 20 | `COBJURIDICA` | `numeric(1,0)` | sim |
| 21 | `ORIGEM` | `varchar(20)` | **nao** |
| 22 | `OBS` | `varchar(30)` | sim |

### IV$TITULOEMPR

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `EMPRESA` | `varchar(12)` | sim |
| 2 | `NROEMPRESA` | `numeric(6,0)` | **nao** |

### IV$TITULOORIGEM

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 2 | `ORIGEM` | `varchar(8)` | **nao** |

### IV$TITULOVENC

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `IDTITULO` | `numeric(18,0)` | **nao** |
| 2 | `NROEMPRESACOBR` | `numeric(6,0)` | **nao** |
| 3 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 4 | `QTDEDIA` | `int(10,0)` | sim |
| 5 | `TIPOCOBR` | `varchar(20)` | sim |
| 6 | `VLRABERTO` | `numeric(14,2)` | sim |
| 7 | `ORIGEM` | `varchar(20)` | **nao** |
| 8 | `DTAVENCTO` | `datetime` | sim |

### IV$_PONTUACAO

`view` · `3 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TIPO` | `varchar(4)` | **nao** |
| 2 | `PROCESSO` | `decimal(15,0)` | sim |
| 3 | `PONTO` | `decimal(38,2)` | sim |

---

## Familia `BI_*` (38 views)

### BI_AGENDA

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 2 | `AGD_SEQAGENDA` | `numeric(18,0)` | **nao** |
| 3 | `AGD_ASSUNTO` | `varchar(50)` | sim |
| 4 | `AGD_DATA_AGENDA` | `datetime` | **nao** |
| 5 | `SEQUSUARIO` | `numeric(18,0)` | **nao** |
| 6 | `AGD_DETALHE` | `varchar(250)` | sim |
| 7 | `AGD_DATA_AGENDA_X` | `varchar(10)` | sim |
| 8 | `AGD_SEMANA` | `varchar(12)` | sim |
| 9 | `AGD_CLASSE` | `varchar(8)` | **nao** |
| 10 | `AGD_PROCESSO` | `numeric(18,0)` | sim |
| 11 | `AGD_CODUSUARIO` | `varchar(20)` | sim |
| 12 | `AGD_CODPROCESSO` | `decimal(4,0)` | **nao** |
| 13 | `AGD_DESCPROCESSO` | `varchar(30)` | **nao** |

### BI_AGENDA_CEN

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 2 | `AGDCEN_SEQAGENDA` | `numeric(18,0)` | **nao** |
| 3 | `AGDCEN_ASSUNTO` | `varchar(50)` | sim |
| 4 | `AGDCEN_DATA_AGENDA` | `datetime` | **nao** |
| 5 | `AGDCEN_DATA_BASESEM` | `datetime` | sim |
| 6 | `AGDCEN_DETALHE` | `varchar(250)` | sim |
| 7 | `AGDCEN_CLASSE` | `varchar(8)` | **nao** |
| 8 | `AGDCEN_PONTUALIDADE` | `varchar(11)` | **nao** |
| 9 | `AGDCEN_DIAS_ATRZ` | `datetime` | sim |

### BI_AGENDA_PROSPECCAO

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `ORIGEM` | `varchar(8)` | **nao** |
| 2 | `SeqAgenda` | `numeric(18,0)` | **nao** |
| 3 | `UltResultado` | `numeric(18,0)` | sim |
| 4 | `Processo` | `numeric(18,0)` | sim |
| 5 | `DataCriacao` | `date` | sim |
| 6 | `DtaUltResultado` | `date` | sim |
| 7 | `AssuntoCmpl` | `varchar(40)` | sim |
| 8 | `DataAgenda` | `date` | sim |
| 9 | `is_ultresultado` | `int(10,0)` | **nao** |
| 10 | `desc_acao` | `varchar(40)` | sim |
| 11 | `SeqPessoa` | `numeric(10,0)` | sim |
| 12 | `SeqUsuario` | `numeric(18,0)` | **nao** |
| 13 | `NomeUsuario` | `varchar(20)` | sim |
| 14 | `Nome` | `varchar(100)` | sim |
| 15 | `Realizada` | `char(1)` | sim |
| 16 | `CPF_CNPJ` | `varchar(30)` | sim |
| 17 | `cidade_pessoa` | `varchar(50)` | sim |
| 18 | `desc_responsavel` | `numeric(18,0)` | sim |
| 19 | `Nome_Responsavel` | `varchar(40)` | sim |
| 20 | `ResultadoCompl` | `varchar(40)` | sim |
| 21 | `filial` | `varchar(30)` | sim |

### BI_CARTEIRA_EQUIP

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 3 | `CART_EMPRESA` | `varchar(12)` | sim |
| 4 | `CART_POTENCIAL` | `varchar(3)` | sim |
| 5 | `CARTEIRA` | `varchar(15)` | sim |
| 6 | `SEQDEPTO` | `decimal(4,0)` | **nao** |
| 7 | `CART_CEN` | `varchar(36)` | sim |
| 8 | `CART_DATA_ULT_CONTATO` | `datetime` | sim |
| 9 | `CART_DATA_ULT_VISITA` | `datetime` | sim |
| 10 | `CART_CICLO` | `decimal(3,0)` | sim |
| 11 | `CART_DIAS_ULT_VISITA` | `datetime` | sim |
| 12 | `CART_IND_VISITADO` | `int(10,0)` | **nao** |
| 13 | `CART_Acompanhamento` | `varchar(12)` | **nao** |

### BI_CARTEIRA_MANITOU

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 3 | `CART_EMPRESA` | `varchar(12)` | sim |
| 4 | `CART_POTENCIAL` | `varchar(3)` | sim |
| 5 | `CARTEIRA` | `varchar(15)` | sim |
| 6 | `SEQDEPTO` | `decimal(4,0)` | **nao** |
| 7 | `CART_CEN` | `varchar(36)` | sim |
| 8 | `CART_DATA_ULT_CONTATO` | `datetime` | sim |
| 9 | `CART_DATA_ULT_VISITA` | `datetime` | sim |
| 10 | `CART_CICLO` | `decimal(3,0)` | sim |
| 11 | `CART_DIAS_ULT_VISITA` | `datetime` | sim |
| 12 | `CART_IND_VISITADO` | `int(10,0)` | **nao** |
| 13 | `CART_Acompanhamento` | `varchar(12)` | **nao** |

### BI_CARTEIRA_PUK

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `CART_EMPRESA` | `varchar(12)` | sim |
| 4 | `CART_POTENCIAL` | `varchar(1)` | sim |
| 5 | `CARTEIRA` | `varchar(15)` | sim |
| 6 | `SEQDEPTO` | `decimal(4,0)` | **nao** |
| 7 | `DEPTOCARTEIRA` | `varchar(12)` | sim |
| 8 | `CART_CEN` | `varchar(36)` | **nao** |
| 9 | `CART_DATA_ULT_CONTATO` | `datetime` | sim |
| 10 | `CART_DATA_ULT_VISITA` | `datetime` | sim |
| 11 | `POTENCIAL_TESTE` | `varchar(3)` | sim |
| 12 | `CART_CICLO` | `int(10,0)` | **nao** |
| 13 | `CART_DIAS_ULT_VISITA` | `datetime` | sim |
| 14 | `CART_IND_VISITADO` | `int(10,0)` | **nao** |
| 15 | `CART_Acompanhamento` | `varchar(12)` | **nao** |
| 16 | `CART_PRINCIPAL` | `varchar(15)` | sim |
| 17 | `CONGLOMERADO` | `varchar(100)` | sim |
| 18 | `Forma_contato` | `varchar(30)` | sim |
| 19 | `PROCESSO` | `numeric(18,0)` | sim |
| 20 | `NOME_VENDEDOR` | `varchar(40)` | sim |
| 21 | `CART_TEMPO_CONTATO` | `varchar(16)` | sim |
| 22 | `SEQPESSOACONGLO` | `numeric(10,0)` | sim |

### BI_CARTEIRA_USADO

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 3 | `CART_EMPRESA` | `varchar(12)` | sim |
| 4 | `CART_POTENCIAL` | `varchar(3)` | sim |
| 5 | `CARTEIRA` | `varchar(15)` | sim |
| 6 | `SEQDEPTO` | `decimal(4,0)` | **nao** |
| 7 | `CART_CEN` | `varchar(36)` | sim |
| 8 | `CART_DATA_ULT_CONTATO` | `datetime` | sim |
| 9 | `CART_DATA_ULT_VISITA` | `datetime` | sim |
| 10 | `CART_CICLO` | `decimal(3,0)` | sim |
| 11 | `CART_DIAS_ULT_VISITA` | `datetime` | sim |
| 12 | `CART_IND_VISITADO` | `int(10,0)` | **nao** |
| 13 | `CART_Acompanhamento` | `varchar(12)` | **nao** |

### BI_CARTEIRA_VN

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `CART_EMPRESA` | `varchar(12)` | sim |
| 4 | `CART_POTENCIAL` | `varchar(3)` | sim |
| 5 | `CARTEIRA` | `varchar(15)` | sim |
| 6 | `SEQDEPTO` | `decimal(4,0)` | **nao** |
| 7 | `DEPTOCARTEIRA` | `varchar(12)` | sim |
| 8 | `CART_CEN` | `varchar(36)` | sim |
| 9 | `CART_DATA_ULT_CONTATO` | `datetime` | sim |
| 10 | `CART_DATA_ULT_VISITA` | `datetime` | sim |
| 11 | `CART_CICLO` | `decimal(3,0)` | sim |
| 12 | `CART_DIAS_ULT_VISITA` | `datetime` | sim |
| 13 | `CART_IND_VISITADO` | `int(10,0)` | **nao** |
| 14 | `CART_Acompanhamento` | `varchar(12)` | **nao** |
| 15 | `CART_PRINCIPAL` | `varchar(15)` | sim |
| 16 | `CONGLOMERADO` | `varchar(100)` | sim |
| 17 | `Forma_contato` | `varchar(30)` | sim |
| 18 | `PROCESSO` | `numeric(18,0)` | sim |
| 19 | `NOME_VENDEDOR` | `varchar(40)` | sim |
| 20 | `CART_TEMPO_CONTATO` | `varchar(16)` | sim |
| 21 | `SEQPESSOACONGLO` | `numeric(10,0)` | sim |

### BI_COBERTURA_PROSPECCAO

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `count_pessoa` | `int(10,0)` | sim |
| 2 | `count_seminteresse` | `int(10,0)` | sim |
| 3 | `count_interessefuturo` | `int(10,0)` | sim |
| 4 | `is_ultresultado` | `int(10,0)` | **nao** |
| 5 | `ano` | `int(10,0)` | sim |
| 6 | `mes` | `char(2)` | sim |
| 7 | `SEQHISTORICO` | `numeric(18,0)` | **nao** |
| 8 | `seq_pessoa` | `int(10,0)` | sim |
| 9 | `cpf_cnpj_pessoa` | `varchar(30)` | sim |
| 10 | `nro_processo` | `int(10,0)` | sim |
| 11 | `classe` | `varchar(30)` | sim |
| 12 | `resultado` | `varchar(30)` | sim |
| 13 | `nro_resultado` | `decimal(6,0)` | sim |
| 14 | `oportunidade` | `varchar(30)` | sim |
| 15 | `empresa` | `int(10,0)` | sim |
| 16 | `data_historico` | `date` | sim |
| 17 | `data_historico_final` | `date` | sim |
| 18 | `DtaUltResultado` | `datetime` | sim |
| 19 | `cip` | `varchar(30)` | sim |
| 20 | `campanha` | `varchar(30)` | sim |
| 21 | `acao` | `varchar(30)` | sim |
| 22 | `seq_conglo` | `numeric(10,0)` | sim |

### BI_COBERTURA_PROSPECCAO_MAQUINAS

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `count_pessoa` | `int(10,0)` | sim |
| 2 | `count_seminteresse` | `int(10,0)` | sim |
| 3 | `count_interessefuturo` | `int(10,0)` | sim |
| 4 | `is_ultresultado` | `int(10,0)` | **nao** |
| 5 | `ano` | `int(10,0)` | sim |
| 6 | `mes` | `char(2)` | sim |
| 7 | `SEQHISTORICO` | `numeric(18,0)` | **nao** |
| 8 | `seq_pessoa` | `int(10,0)` | sim |
| 9 | `cpf_cnpj_pessoa` | `varchar(30)` | sim |
| 10 | `nro_processo` | `int(10,0)` | sim |
| 11 | `classe` | `varchar(30)` | sim |
| 12 | `resultado` | `varchar(30)` | sim |
| 13 | `nro_resultado` | `decimal(6,0)` | sim |
| 14 | `oportunidade` | `varchar(30)` | sim |
| 15 | `empresa` | `int(10,0)` | sim |
| 16 | `data_historico` | `date` | sim |
| 17 | `data_historico_final` | `date` | sim |
| 18 | `DtaUltResultado` | `datetime` | sim |
| 19 | `cip` | `varchar(30)` | sim |
| 20 | `campanha` | `varchar(30)` | sim |
| 21 | `acao` | `varchar(30)` | sim |
| 22 | `seq_conglo` | `numeric(10,0)` | sim |

### BI_DEPTO

`view` · `3 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQDEPTO` | `decimal(4,0)` | **nao** |
| 2 | `DEPTO` | `varchar(12)` | sim |
| 3 | `DEPARTAMENTO` | `varchar(30)` | sim |

### BI_DTAPROVCREDITO

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `PROCESSO` | `numeric(18,0)` | **nao** |
| 2 | `DTVENDA_AUTORIZADA` | `datetime` | sim |
| 3 | `DTVENDA_PAGTO` | `datetime` | sim |
| 4 | `DTAPROVACAO_CREDITOTRT` | `datetime` | sim |
| 5 | `DTAPROVACAO_CREDITOIMP` | `datetime` | sim |
| 6 | `DTAPROVACAO_CREDITOAMS` | `datetime` | sim |

### BI_EMPRESA

`view` · `3 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `NROEMPRESA` | `numeric(6,0)` | **nao** |
| 2 | `EMPRESA` | `varchar(12)` | sim |
| 3 | `MATRIZ` | `varchar(12)` | sim |

### BI_FROTA

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `FROTA_TIPO` | `varchar(20)` | **nao** |
| 3 | `FROTA_MARCA` | `varchar(30)` | sim |
| 4 | `FROTA_MODELO` | `varchar(40)` | sim |
| 5 | `FROTA_POTENCIA` | `varchar(20)` | sim |
| 6 | `FROTA_ANO` | `varchar(40)` | sim |
| 7 | `FROTA_QTDE` | `decimal(15,2)` | **nao** |
| 8 | `FRT_DATA_ALTERACAO` | `datetime` | sim |
| 9 | `FRT_DADO_ATUAL` | `varchar(3)` | **nao** |
| 10 | `SEQPRINC` | `numeric(10,0)` | sim |
| 11 | `Propriedade` | `varchar(20)` | **nao** |
| 12 | `FROTA_CHASSI` | `varchar(30)` | sim |

### BI_GRUPO_USR

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `GRUPO_USUARIO` | `varchar(20)` | **nao** |
| 2 | `USUARIO` | `numeric(18,0)` | **nao** |

### BI_OPORTUNIDADE_ENC

`view` · `24 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `decimal(10,0)` | sim |
| 2 | `ONE_PROCESSO` | `numeric(18,0)` | **nao** |
| 3 | `ONE_CAMPANHAENC` | `varchar(20)` | sim |
| 4 | `ONE_ETAPA` | `varchar(11)` | **nao** |
| 5 | `ONE_FASE` | `varchar(23)` | sim |
| 6 | `ONE_FASE_ORDEM` | `decimal(2,0)` | sim |
| 7 | `ONE_STATUS` | `varchar(20)` | sim |
| 8 | `ONE_STATUSDESC` | `varchar(20)` | sim |
| 9 | `ONE_DATA_INICIO` | `datetime` | sim |
| 10 | `ONE_DATA_REALIZADO` | `datetime` | sim |
| 11 | `ONE_ANOMES_REALIZADO` | `varchar(5)` | sim |
| 12 | `ONE_MES_REALIZADO` | `varchar(2)` | sim |
| 13 | `FORM` | `int(10,0)` | sim |
| 14 | `FATUR_REALIZADO` | `datetime` | sim |
| 15 | `ONE_DURACAO_DIAS` | `int(10,0)` | sim |
| 16 | `ONE_VALOR` | `decimal(15,2)` | sim |
| 17 | `ONE_PERSP_PERCT` | `decimal(3,0)` | sim |
| 18 | `ONE_PERSPECTIVA` | `varchar(20)` | sim |
| 19 | `ONE_CEN` | `varchar(20)` | sim |
| 20 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 21 | `SEQPRINC` | `numeric(10,0)` | sim |
| 22 | `ONE_DESC_ACAO` | `varchar(40)` | sim |
| 23 | `ONE_ATENDENTE` | `varchar(20)` | sim |
| 24 | `ONE_CODPROCESSO` | `decimal(4,0)` | sim |

### BI_OPORT_ABERTAS

`view` · `27 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `Processo` | `numeric(18,0)` | **nao** |
| 2 | `Seqpessoa` | `decimal(10,0)` | sim |
| 3 | `Nomerazao` | `varchar(100)` | sim |
| 4 | `Acao` | `varchar(40)` | sim |
| 5 | `Resultado` | `varchar(40)` | sim |
| 6 | `Data` | `datetime` | sim |
| 7 | `Conclusivo` | `varchar(1)` | sim |
| 8 | `AgendaAberta` | `varchar(1)` | sim |
| 9 | `Fase` | `varchar(19)` | sim |
| 10 | `DtaAgenda` | `datetime` | sim |
| 11 | `Nrodias` | `int(10,0)` | sim |
| 12 | `Nrohoras` | `int(10,0)` | sim |
| 13 | `Nrominutos` | `int(10,0)` | sim |
| 14 | `Codacao` | `numeric(6,0)` | sim |
| 15 | `OrdemFase` | `varchar(21)` | sim |
| 16 | `TemEntrega` | `varchar(1)` | sim |
| 17 | `HorasIni` | `int(10,0)` | sim |
| 18 | `MinIni` | `int(10,0)` | sim |
| 19 | `Dias` | `int(10,0)` | sim |
| 20 | `HorasFim` | `int(10,0)` | sim |
| 21 | `MinFim` | `int(10,0)` | sim |
| 22 | `Minutos` | `int(10,0)` | sim |
| 23 | `MinutosDias` | `int(10,0)` | sim |
| 24 | `TempoTotal` | `varchar(8000)` | sim |
| 25 | `TempoDias` | `int(10,0)` | sim |
| 26 | `TempoHoras` | `int(10,0)` | sim |
| 27 | `TempoMinutos` | `int(10,0)` | sim |

### BI_OPORT_ABERTASRESUMO

`view` · `39 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `Processo` | `numeric(18,0)` | **nao** |
| 2 | `Seqpessoa` | `decimal(10,0)` | sim |
| 3 | `Dias1` | `int(10,0)` | sim |
| 4 | `Horas1` | `int(10,0)` | sim |
| 5 | `Min1` | `int(10,0)` | sim |
| 6 | `Dias2` | `int(10,0)` | sim |
| 7 | `Horas2` | `int(10,0)` | sim |
| 8 | `Min2` | `int(10,0)` | sim |
| 9 | `Dias3` | `int(10,0)` | sim |
| 10 | `Horas3` | `int(10,0)` | sim |
| 11 | `Min3` | `int(10,0)` | sim |
| 12 | `Dias4` | `int(10,0)` | sim |
| 13 | `Horas4` | `int(10,0)` | sim |
| 14 | `Min4` | `int(10,0)` | sim |
| 15 | `Dias5` | `int(10,0)` | sim |
| 16 | `Horas5` | `int(10,0)` | sim |
| 17 | `Min5` | `int(10,0)` | sim |
| 18 | `Dias6` | `int(10,0)` | sim |
| 19 | `Horas6` | `int(10,0)` | sim |
| 20 | `Min6` | `int(10,0)` | sim |
| 21 | `Dias7` | `int(10,0)` | sim |
| 22 | `Horas7` | `int(10,0)` | sim |
| 23 | `Min7` | `int(10,0)` | sim |
| 24 | `Dias8` | `int(10,0)` | sim |
| 25 | `Horas8` | `int(10,0)` | sim |
| 26 | `Min8` | `int(10,0)` | sim |
| 27 | `Dias9` | `int(10,0)` | sim |
| 28 | `Horas9` | `int(10,0)` | sim |
| 29 | `Min9` | `int(10,0)` | sim |
| 30 | `AgdAberta9` | `varchar(1)` | sim |
| 31 | `AgdAberta8` | `varchar(1)` | sim |
| 32 | `AgdAberta7` | `varchar(1)` | sim |
| 33 | `AgdAberta6` | `varchar(1)` | sim |
| 34 | `AgdAberta5` | `varchar(1)` | sim |
| 35 | `AgdAberta4` | `varchar(1)` | sim |
| 36 | `AgdAberta3` | `varchar(1)` | sim |
| 37 | `AgdAberta2` | `varchar(1)` | sim |
| 38 | `AgdAberta1` | `varchar(1)` | sim |
| 39 | `TemEntrega` | `varchar(1)` | sim |

### BI_OPORT_FUNIL

`view` · `4 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `ONE_PROCESSO` | `numeric(18,0)` | **nao** |
| 2 | `ONE_CAMPANHAFUN` | `varchar(20)` | sim |
| 3 | `FUNIL_TIPO` | `varchar(13)` | **nao** |
| 4 | `FUNIL_VALOR` | `int(10,0)` | **nao** |

### BI_OPORT_FUNIL2

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `ONE_PROCESSO` | `numeric(18,0)` | **nao** |
| 2 | `ONE_CAMPANHAFUN` | `varchar(20)` | sim |
| 3 | `FUNIL_TIPO` | `varchar(18)` | **nao** |
| 4 | `STAT_DTAMONIT` | `datetime` | sim |
| 5 | `ONE_DATA_REALIZADOF` | `datetime` | sim |
| 6 | `STAT_STATUS` | `varchar(20)` | sim |
| 7 | `FUNIL_VALOR` | `int(10,0)` | **nao** |

### BI_OPORT_FUNIL2_BKP

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `ONE_PROCESSO` | `numeric(18,0)` | **nao** |
| 2 | `ONE_CAMPANHAFUN` | `varchar(20)` | sim |
| 3 | `FUNIL_TIPO` | `varchar(13)` | **nao** |
| 4 | `STAT_DTAMONIT` | `datetime` | sim |
| 5 | `ONE_DATA_REALIZADOF` | `datetime` | sim |
| 6 | `STAT_STATUS` | `varchar(20)` | sim |
| 7 | `FUNIL_VALOR` | `int(10,0)` | **nao** |

### BI_ORIGEM_RECEITA

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `ORIGEM_TIPO` | `varchar(30)` | sim |
| 3 | `ORIGE_TIPO_AREA` | `varchar(40)` | sim |
| 4 | `ORIGEM_SUB_TIPO` | `varchar(40)` | sim |
| 5 | `ORIGEM_PESO` | `varchar(40)` | sim |
| 6 | `ORIGEM_HA` | `decimal(15,2)` | sim |
| 7 | `ORIGEM_HA_PRINCIPAL` | `decimal(15,2)` | sim |
| 8 | `ORIGEM_DATA_ALTERACAO` | `datetime` | sim |
| 9 | `ORIGEM_DADO_ATUAL` | `varchar(3)` | **nao** |
| 10 | `ORIGEM_AREA_COMPART` | `varchar(40)` | sim |
| 11 | `ORIGEM_AREAHA` | `decimal(15,2)` | sim |
| 12 | `ORIGEM_TIPOPROPRIEDADE` | `varchar(40)` | sim |
| 13 | `ORIGEM_AREAPRINC` | `varchar(40)` | sim |
| 14 | `SEQPRINC` | `numeric(10,0)` | sim |

### BI_PESSOA

`view` · `38 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `CLIENTE` | `varchar(60)` | **nao** |
| 3 | `CIDADE` | `varchar(50)` | sim |
| 4 | `ESTADO` | `varchar(2)` | sim |
| 5 | `NROCGCCPF` | `decimal(13,0)` | sim |
| 6 | `DIGCGCCPF` | `decimal(2,0)` | sim |
| 7 | `ATIVIDADE` | `varchar(30)` | sim |
| 8 | `FJ` | `char(1)` | sim |
| 9 | `FONE1` | `varchar(38)` | sim |
| 10 | `FONE2` | `varchar(38)` | sim |
| 11 | `FONE3` | `varchar(38)` | sim |
| 12 | `EMAIL` | `varchar(70)` | sim |
| 13 | `LATITUDE` | `decimal(14,11)` | sim |
| 14 | `LONGITUDE` | `decimal(14,11)` | sim |
| 15 | `LOCATION` | `varchar(8000)` | sim |
| 16 | `POSSUI_FROTA` | `int(10,0)` | **nao** |
| 17 | `PES_ORIGEM_OK` | `varchar(3)` | **nao** |
| 18 | `PES_TRATOR_OK` | `varchar(3)` | **nao** |
| 19 | `PES_TRATOR_QTDE` | `decimal(38,2)` | **nao** |
| 20 | `PES_COLHE_GRAO_OK` | `varchar(3)` | **nao** |
| 21 | `PES_COLHE_GRAO_QTDE` | `decimal(38,2)` | **nao** |
| 22 | `PES_COLHE_CANA_OK` | `varchar(3)` | **nao** |
| 23 | `PES_COLHE_CANA_QTDE` | `decimal(38,2)` | **nao** |
| 24 | `PES_PLANTADERIA_OK` | `varchar(3)` | **nao** |
| 25 | `PES_PLANTADERIA_QTDE` | `decimal(38,2)` | **nao** |
| 26 | `PES_DATA_INCLUSAO` | `datetime` | sim |
| 27 | `PES_CADASTRADO_POR` | `varchar(20)` | sim |
| 28 | `PES_STATUS` | `char(1)` | **nao** |
| 29 | `PES_VENCULO` | `varchar(11)` | **nao** |
| 30 | `SEQ_CONGLOMERADO` | `numeric(10,0)` | **nao** |
| 31 | `NOME_CONGLOMERADO` | `varchar(60)` | sim |
| 32 | `CONGLOMERADO` | `varchar(93)` | sim |
| 33 | `GRUPO` | `varchar(30)` | sim |
| 34 | `DTANASCFUND` | `datetime` | sim |
| 35 | `RECEBE_TELEFONEMA` | `varchar(3)` | **nao** |
| 36 | `RECEBE_CORRESPONDENCIA` | `varchar(3)` | **nao** |
| 37 | `RECEBE_EMAIL` | `varchar(3)` | **nao** |
| 38 | `RECEBE_SMS` | `varchar(3)` | **nao** |

### BI_PESSOAOP

`view` · `40 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `CLIENTE` | `varchar(100)` | sim |
| 3 | `CIDADE` | `varchar(50)` | sim |
| 4 | `ESTADO` | `varchar(2)` | sim |
| 5 | `ATIVIDADE` | `varchar(30)` | sim |
| 6 | `FJ` | `char(1)` | sim |
| 7 | `FONE1` | `varchar(38)` | sim |
| 8 | `FONE2` | `varchar(38)` | sim |
| 9 | `FONE3` | `varchar(38)` | sim |
| 10 | `EMAIL` | `varchar(70)` | sim |
| 11 | `NROCGCCPF` | `decimal(13,0)` | sim |
| 12 | `DIGCGCCPF` | `decimal(2,0)` | sim |
| 13 | `LATITUDE` | `decimal(14,11)` | sim |
| 14 | `RECEBE_TELEFONEMA` | `numeric(1,0)` | sim |
| 15 | `RECEBE_CORRESPONDENCIA` | `numeric(1,0)` | sim |
| 16 | `RECEBE_EMAIL` | `numeric(1,0)` | sim |
| 17 | `RECEBE_SMS` | `numeric(1,0)` | sim |
| 18 | `SEQ_CONGLOMERADO` | `numeric(10,0)` | sim |
| 19 | `NOME_CONGLOMERADO` | `varchar(100)` | sim |
| 20 | `CONGLOMERADO` | `varchar(133)` | sim |
| 21 | `POSSUI_FROTA` | `int(10,0)` | **nao** |
| 22 | `GRUPO` | `varchar(30)` | sim |
| 23 | `DTANASCFUND` | `datetime` | sim |
| 24 | `LONGITUDE` | `decimal(14,11)` | sim |
| 25 | `LOCATION` | `varchar(8000)` | sim |
| 26 | `PES_ORIGEM_OK` | `varchar(3)` | **nao** |
| 27 | `PES_TRATOR_OK` | `varchar(3)` | **nao** |
| 28 | `PES_TRATOR_QTDE` | `decimal(38,2)` | **nao** |
| 29 | `PES_COLHE_GRAO_OK` | `varchar(3)` | **nao** |
| 30 | `PES_COLHE_GRAO_QTDE` | `decimal(38,2)` | **nao** |
| 31 | `PES_COLHE_CANA_OK` | `varchar(3)` | **nao** |
| 32 | `PES_COLHE_CANA_QTDE` | `decimal(38,2)` | **nao** |
| 33 | `PES_PLANTADERIA_OK` | `varchar(3)` | **nao** |
| 34 | `PES_PLANTADERIA_QTDE` | `decimal(38,2)` | **nao** |
| 35 | `PES_DATA_INCLUSAO` | `datetime` | sim |
| 36 | `PES_CADASTRADO_POR` | `varchar(20)` | sim |
| 37 | `PES_STATUS` | `char(1)` | **nao** |
| 38 | `PES_VINCULO` | `varchar(11)` | **nao** |
| 39 | `SEQPESSOAPRC` | `numeric(10,0)` | **nao** |
| 40 | `VALIDACAO_CARTEIRA` | `varchar(15)` | **nao** |

### BI_QUALIDADE

`view` · `14 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `DTAREALIZACAO` | `datetime` | **nao** |
| 2 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 3 | `NomeRazao` | `varchar(60)` | **nao** |
| 4 | `Carteira` | `varchar(15)` | sim |
| 5 | `PROCESSO` | `numeric(18,0)` | sim |
| 6 | `ACAO` | `varchar(40)` | sim |
| 7 | `AREA` | `varchar(8)` | **nao** |
| 8 | `CLASSE` | `varchar(20)` | sim |
| 9 | `RESULTADO` | `varchar(40)` | sim |
| 10 | `SATISFACAO_ATENDIMENTO` | `varchar(3)` | sim |
| 11 | `MOTIVO_INSATISFACAO` | `varchar(40)` | sim |
| 12 | `NOTA` | `varchar(7)` | sim |
| 13 | `PROMOTOR` | `varchar(11)` | sim |
| 14 | `DETALHE_FRM` | `varchar(500)` | sim |

### BI_USUARIO

`view` · `3 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQUSUARIO` | `numeric(18,0)` | **nao** |
| 2 | `USR_CODIGO` | `varchar(20)` | **nao** |
| 3 | `USR_NOME` | `varchar(20)` | sim |

### BI_VENDA_MAQ

`view` · `32 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `PROCESSO` | `decimal(15,0)` | sim |
| 2 | `VDA_TIPO_FINANCIAMENTO` | `varchar(20)` | sim |
| 3 | `VDA_INST_FINANCEIRA` | `varchar(20)` | sim |
| 4 | `VDA_VALOR` | `decimal(14,2)` | sim |
| 5 | `VDA_MODELO` | `varchar(50)` | sim |
| 6 | `VDA_MARCA` | `varchar(20)` | sim |
| 7 | `VDA_CHASSI` | `varchar(340)` | sim |
| 8 | `VDA_TIPO_MAQUINA` | `varchar(20)` | sim |
| 9 | `VDA_PROB_APROVACAO` | `varchar(20)` | sim |
| 10 | `VDA_PAC_EMITIDO` | `varchar(3)` | **nao** |
| 11 | `VDA_DATA_PAC` | `datetime` | sim |
| 12 | `VDA_FATURAMENTO_REALIZADO` | `varchar(3)` | **nao** |
| 13 | `VDA_DATA_FATURAMENTO` | `datetime` | sim |
| 14 | `VDA_DATA_PREVISTA_FATURAMENTO` | `datetime` | sim |
| 15 | `VDA_MES_PREVISTO_FATURAMENTO` | `varchar(5)` | sim |
| 16 | `VDA_ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 17 | `VDA_DIAS_PEDIDO_PAC` | `datetime` | sim |
| 18 | `VDA_DIAS_PAC_FATURAMENTO` | `datetime` | sim |
| 19 | `data_pedido_de_venda` | `datetime` | sim |
| 20 | `VDA_DIAS_PEDIDO_FATURAMENTO` | `datetime` | sim |
| 21 | `APROVACAO_GESTOR` | `varchar(40)` | sim |
| 22 | `FDD` | `datetime` | sim |
| 23 | `PREVISAO_ENTREGA` | `datetime` | sim |
| 24 | `STATUS_BC1` | `varchar(20)` | sim |
| 25 | `STATUS_BC2` | `varchar(20)` | sim |
| 26 | `STATUS_BC3` | `varchar(20)` | sim |
| 27 | `COTACAO` | `varchar(30)` | sim |
| 28 | `INFORMACAO_PREPARACA` | `varchar(4000)` | sim |
| 29 | `DATA_ENTREGA` | `datetime` | sim |
| 30 | `TRANSPORTADORA` | `varchar(30)` | sim |
| 31 | `PREVISAO_ENTREGA_PRE` | `datetime` | sim |
| 32 | `INST_SELECIONADA` | `varchar(20)` | sim |

### BI_VENDA_MAQ_ENC

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TIPOFORM` | `int(10,0)` | **nao** |
| 2 | `ONE_PROCESSO` | `decimal(15,0)` | sim |
| 3 | `VDAE_TIPO_FINANCIAMENTO` | `varchar(20)` | sim |
| 4 | `VDAE_INST_FINANCEIRA` | `varchar(20)` | sim |
| 5 | `VDAE_VALOR` | `decimal(14,2)` | sim |
| 6 | `VDAE_MODELO` | `varchar(50)` | sim |
| 7 | `VDAE_MARCA` | `varchar(20)` | sim |
| 8 | `VDAE_TIPO_MAQUINA` | `varchar(20)` | sim |
| 9 | `VDAE_PROB_APROVACAO` | `varchar(20)` | sim |
| 10 | `VDAE_PAC_EMITIDO` | `varchar(3)` | **nao** |
| 11 | `VDAE_DATA_PAC` | `datetime` | sim |
| 12 | `VDAE_DATA_FAT_AUTORIZ` | `datetime` | sim |
| 13 | `VDAE_FATURAMENTO_REALIZADO` | `varchar(3)` | **nao** |
| 14 | `VDAE_DATA_FATURAMENTO` | `datetime` | sim |
| 15 | `VDAE_DATA_PREVISTA_FATURAMENTO` | `datetime` | sim |
| 16 | `VDAE_MES_PREVISTO_FATURAMENTO` | `varchar(5)` | sim |
| 17 | `VDAE_MES_FATURAMENTO` | `varchar(5)` | sim |
| 18 | `VDAE_ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 19 | `VDAE_DIAS_PEDIDO_PAC` | `int(10,0)` | sim |
| 20 | `VDAE_DIAS_PEDIDO_FATURAMENTO` | `int(10,0)` | sim |
| 21 | `VDAE_DIAS_PAC_FATURAMENTO` | `int(10,0)` | sim |
| 22 | `VDAE_DTVENDA_AUTORIZADA` | `datetime` | sim |
| 23 | `VDAE_DTVENDA_PAGTO` | `datetime` | sim |
| 24 | `VDAE_DTAPROVACAO_CREDITOTRT` | `datetime` | sim |
| 25 | `VDAE_DTAPROVACAO_CREDITOIMP` | `datetime` | sim |
| 26 | `VDAE_DTAPROVACAO_CREDITOAMS` | `datetime` | sim |
| 27 | `INST_SELECIONADA` | `varchar(20)` | sim |
| 28 | `VDAE_DT_LIBERACAO` | `datetime` | sim |

### BI_VENDA_MAQ_NEW

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `processo` | `decimal(15,0)` | sim |
| 2 | `VDA_TIPO_FINANCIAMENTO` | `varchar(20)` | sim |
| 3 | `VDA_INST_FINANCEIRA` | `varchar(20)` | sim |
| 4 | `VDA_VALOR` | `decimal(14,2)` | sim |
| 5 | `VDA_MODELO` | `varchar(50)` | sim |
| 6 | `VDA_MARCA` | `varchar(20)` | sim |
| 7 | `VDA_TIPO_MAQUINA` | `varchar(20)` | sim |
| 8 | `VDA_PROB_APROVACAO` | `varchar(7)` | **nao** |
| 9 | `VDA_PAC_EMITIDO` | `varchar(3)` | **nao** |
| 10 | `VDA_DATA_PAC` | `datetime` | sim |
| 11 | `VDA_FATURAMENTO_REALIZADO` | `varchar(3)` | **nao** |
| 12 | `VDA_DATA_FATURAMENTO` | `datetime` | sim |
| 13 | `VDA_DATA_PREVISTA_FATURAMENTO` | `datetime` | sim |
| 14 | `VDA_MES_PREVISTO_FATURAMENTO` | `varchar(5)` | sim |
| 15 | `VDA_ORIGEM_FATURAMENTO` | `varchar(20)` | sim |
| 16 | `VDA_DIAS_PEDIDO_PAC` | `datetime` | sim |
| 17 | `VDA_DIAS_PEDIDO_FATURAMENTO` | `int(10,0)` | sim |
| 18 | `VDA_DIAS_PAC_FATURAMENTO` | `datetime` | sim |
| 19 | `data_pedido_de_venda` | `datetime` | sim |

### BI_VENDA_PERDIDA

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `seqquestionario` | `numeric(18,0)` | **nao** |
| 2 | `VPERD_DATA` | `datetime` | sim |
| 3 | `VPERD_OBS` | `varchar(500)` | sim |
| 4 | `ONE_PROCESSO` | `decimal(15,0)` | sim |
| 5 | `VPERD_marca_vp` | `varchar(30)` | sim |
| 6 | `VPERD_revenda` | `varchar(30)` | sim |
| 7 | `VPERD_modelo_vp` | `varchar(30)` | sim |
| 8 | `VPERD_data_da_venda` | `datetime` | sim |
| 9 | `VPERD_MES_da_venda` | `varchar(5)` | sim |
| 10 | `VPERD_quantidade` | `decimal(14,0)` | sim |
| 11 | `VPERD_preco_concorrente` | `decimal(14,2)` | sim |
| 12 | `VPERD_preco_NOSSO` | `decimal(14,2)` | sim |
| 13 | `VPERD_modelo_john_deer` | `varchar(30)` | sim |
| 14 | `VPERD_motivo` | `varchar(40)` | sim |
| 15 | `VPERD_tipo_de_maquina` | `varchar(30)` | sim |
| 16 | `VPERD_particip_negociacao` | `varchar(3)` | sim |
| 17 | `VPERD_havia_monitoramento` | `varchar(3)` | sim |
| 18 | `VPERD_DETALHE` | `varchar(1001)` | sim |

### BI_VENDA_PERDIDA_PROD

`view` · `18 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `seqquestionario` | `numeric(18,0)` | **nao** |
| 2 | `VPERD_DATA` | `datetime` | sim |
| 3 | `VPERD_OBS` | `varchar(4000)` | sim |
| 4 | `ONE_PROCESSO` | `decimal(15,0)` | sim |
| 5 | `VPERD_marca_vp` | `varchar(30)` | sim |
| 6 | `VPERD_revenda` | `varchar(30)` | sim |
| 7 | `VPERD_modelo_vp` | `varchar(30)` | sim |
| 8 | `VPERD_data_da_venda` | `datetime` | sim |
| 9 | `VPERD_MES_da_venda` | `varchar(5)` | sim |
| 10 | `VPERD_quantidade` | `decimal(14,0)` | sim |
| 11 | `VPERD_preco_concorrente` | `decimal(14,2)` | sim |
| 12 | `VPERD_preco_NOSSO` | `decimal(14,2)` | sim |
| 13 | `VPERD_modelo_john_deer` | `varchar(30)` | sim |
| 14 | `VPERD_motivo` | `varchar(40)` | sim |
| 15 | `VPERD_tipo_de_maquina` | `varchar(30)` | sim |
| 16 | `VPERD_particip_negociacao` | `varchar(3)` | sim |
| 17 | `VPERD_havia_monitoramento` | `varchar(3)` | sim |
| 18 | `VPERD_DETALHE` | `varchar(4000)` | sim |

### BI_VISITA

`view` · `12 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQAGDDPLAN` | `numeric(20,0)` | sim |
| 2 | `SEQUSUARIO` | `numeric(18,0)` | sim |
| 3 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 4 | `VISITA_DATA_PLANEJADA` | `datetime` | sim |
| 5 | `VISITA_ASSUNTO` | `varchar(50)` | sim |
| 6 | `VISITA_OK` | `varchar(3)` | **nao** |
| 7 | `VISITA_DATA_REALIZADA` | `datetime` | sim |
| 8 | `VISITA_RESULTADO` | `varchar(20)` | sim |
| 9 | `VISITA_RESULTADO_CMPL` | `varchar(20)` | sim |
| 10 | `VISITA_REALIZADA_POR` | `varchar(20)` | sim |
| 11 | `VISITA_CANAL` | `varchar(12)` | sim |
| 12 | `VISITA_EXTERNA` | `varchar(3)` | **nao** |

### bi_Oportunidades

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `PROCESSO` | `numeric(18,0)` | sim |
| 3 | `ProcessoDNA` | `numeric(18,0)` | sim |
| 4 | `ProcessoPai` | `numeric(18,0)` | sim |
| 5 | `CLASSE` | `varchar(20)` | **nao** |
| 6 | `COD_RESULTADO` | `decimal(6,0)` | **nao** |
| 7 | `RESULTADO` | `varchar(40)` | **nao** |
| 8 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 9 | `DTAREALIZACAO` | `datetime` | **nao** |
| 10 | `VENDEDOR` | `varchar(20)` | sim |
| 11 | `CAMPANHA` | `varchar(20)` | sim |

### bi_cobertura

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `PROCESSO` | `numeric(18,0)` | sim |
| 3 | `ProcessoDNA` | `numeric(18,0)` | sim |
| 4 | `ProcessoPai` | `numeric(18,0)` | sim |
| 5 | `CLASSE` | `varchar(20)` | **nao** |
| 6 | `COD_RESULTADO` | `decimal(6,0)` | **nao** |
| 7 | `RESULTADO` | `varchar(40)` | **nao** |
| 8 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 9 | `DTAREALIZACAO` | `datetime` | **nao** |
| 10 | `VENDEDOR` | `varchar(20)` | sim |
| 11 | `CAMPANHA` | `varchar(20)` | sim |

### bi_faturamentos

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `PROCESSO` | `numeric(18,0)` | sim |
| 3 | `ProcessoDNA` | `numeric(18,0)` | sim |
| 4 | `ProcessoPai` | `numeric(18,0)` | sim |
| 5 | `CLASSE` | `varchar(20)` | **nao** |
| 6 | `COD_RESULTADO` | `decimal(6,0)` | **nao** |
| 7 | `RESULTADO` | `varchar(40)` | **nao** |
| 8 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 9 | `DTAREALIZACAO` | `datetime` | **nao** |
| 10 | `VENDEDOR` | `varchar(20)` | sim |
| 11 | `CAMPANHA` | `varchar(20)` | sim |

### bi_oportunidade

`view` · `15 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `decimal(10,0)` | sim |
| 2 | `NROEMPRESA` | `numeric(6,0)` | sim |
| 3 | `PROCESSO` | `numeric(18,0)` | **nao** |
| 4 | `ON_CAMPANHA` | `varchar(20)` | sim |
| 5 | `ON_ETAPA` | `varchar(18)` | **nao** |
| 6 | `ON_FASE` | `varchar(20)` | sim |
| 7 | `ON_FASE_ORDEM` | `decimal(2,0)` | sim |
| 8 | `ON_STATUS` | `varchar(20)` | sim |
| 9 | `ON_DATA_INICIO` | `datetime` | sim |
| 10 | `ON_DATA_REALIZADO` | `datetime` | sim |
| 11 | `ON_DATA_ULT_RESULTADO` | `datetime` | sim |
| 12 | `ON_VALOR` | `decimal(15,2)` | sim |
| 13 | `ON_PERSP_PERCT` | `decimal(3,0)` | sim |
| 14 | `ON_PERSPECTIVA` | `varchar(20)` | sim |
| 15 | `ON_CEN` | `varchar(20)` | sim |

### bi_oportunidade_produto

`view` · `7 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `PROCESSO` | `numeric(18,0)` | sim |
| 2 | `PROD_TIPO` | `varchar(20)` | sim |
| 3 | `PROD_MARCA` | `varchar(20)` | sim |
| 4 | `PROD_MODELO` | `varchar(100)` | sim |
| 5 | `PROD_QTDE` | `decimal(10,2)` | sim |
| 6 | `PROD_VALOR` | `decimal(15,2)` | sim |
| 7 | `PROD_VENDIDO` | `varchar(3)` | sim |

### bi_pedidos

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `SEQPESSOA` | `numeric(10,0)` | **nao** |
| 2 | `PROCESSO` | `numeric(18,0)` | sim |
| 3 | `ProcessoDNA` | `numeric(18,0)` | sim |
| 4 | `ProcessoPai` | `numeric(18,0)` | sim |
| 5 | `CLASSE` | `varchar(20)` | **nao** |
| 6 | `COD_RESULTADO` | `decimal(6,0)` | **nao** |
| 7 | `RESULTADO` | `varchar(40)` | **nao** |
| 8 | `RESULTADOCMPL` | `varchar(20)` | sim |
| 9 | `DTAREALIZACAO` | `datetime` | **nao** |
| 10 | `VENDEDOR` | `varchar(20)` | sim |
| 11 | `CAMPANHA` | `varchar(20)` | sim |

---

## Familia `V_*` (7 views)

### V_C5SYSCOLUMN

`view` · `10 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `COLNOME` | `nvarchar(128)` | sim |
| 2 | `TBNOME` | `nvarchar(128)` | **nao** |
| 3 | `TBDONO` | `nvarchar(128)` | sim |
| 4 | `COLTIPODADO` | `char(8)` | sim |
| 5 | `COLTAMANHO` | `smallint(5,0)` | **nao** |
| 6 | `COLdecimal` | `int(10,0)` | **nao** |
| 7 | `COLNULL` | `char(1)` | sim |
| 8 | `COLROTULO` | `int(10,0)` | sim |
| 9 | `COLNOTA` | `varchar(1)` | **nao** |
| 10 | `COLNRO` | `smallint(5,0)` | sim |

### V_C5SYSCONSTRAINT

`view` · `8 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `DONO` | `nvarchar(128)` | sim |
| 2 | `TBNOME` | `nvarchar(128)` | sim |
| 3 | `TBREF` | `nvarchar(128)` | sim |
| 4 | `CONSTRNAME` | `nvarchar(128)` | **nao** |
| 5 | `CONSTRTYPE` | `char(2)` | sim |
| 6 | `TABID` | `int(10,0)` | **nao** |
| 7 | `UPDRULE` | `nvarchar(60)` | sim |
| 8 | `DELRULE` | `nvarchar(60)` | sim |

### V_C5SYSINDEX

`view` · `9 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TBDONO` | `nvarchar(128)` | **nao** |
| 2 | `TBNOME` | `nvarchar(128)` | **nao** |
| 3 | `IDXNOME` | `nvarchar(128)` | sim |
| 4 | `IDXTYPE` | `nvarchar(60)` | sim |
| 5 | `COLNOME` | `nvarchar(128)` | sim |
| 6 | `COLORDER` | `int(10,0)` | **nao** |
| 7 | `COLISINCLUDED` | `bit` | sim |
| 8 | `IDXISPRIMARYKEY` | `bit` | sim |
| 9 | `IDXISUNIQUE` | `bit` | sim |

### V_C5SYSPK

`view` · `5 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TBDONO` | `nvarchar(128)` | sim |
| 2 | `TBNOME` | `nvarchar(128)` | sim |
| 3 | `COLNOME` | `nvarchar(128)` | sim |
| 4 | `POSICAO` | `smallint(5,0)` | sim |
| 5 | `PKNOME` | `nvarchar(128)` | sim |

### V_C5SYSREFERENCE

`view` · `2 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TBNOME` | `nvarchar(128)` | **nao** |
| 2 | `TBREF` | `nvarchar(128)` | **nao** |

### V_C5SYSTABLE

`view` · `6 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `TBNOME` | `nvarchar(128)` | **nao** |
| 2 | `TBDONO` | `nvarchar(128)` | sim |
| 3 | `QTDECOLUNA` | `int(10,0)` | sim |
| 4 | `TBTIPO` | `char(1)` | sim |
| 5 | `COLROTULO` | `int(10,0)` | sim |
| 6 | `COLNOTA` | `varchar(1)` | **nao** |

### V_UTIL_TELEFONES_CONTATOS

`view` · `22 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `Chave` | `varchar(83)` | **nao** |
| 2 | `Seq` | `numeric(10,0)` | **nao** |
| 3 | `Seq_Contato` | `decimal(4,0)` | **nao** |
| 4 | `Nome` | `varchar(100)` | sim |
| 5 | `CONTATO` | `varchar(100)` | sim |
| 6 | `ORIGEM` | `varchar(7)` | **nao** |
| 7 | `FoneDDD1` | `varchar(5)` | sim |
| 8 | `FoneNro1` | `decimal(12,0)` | sim |
| 9 | `DDD_FONE_1` | `varchar(49)` | sim |
| 10 | `DDI_DDD_FONE_1` | `varchar(48)` | sim |
| 11 | `FoneDDD2` | `varchar(5)` | sim |
| 12 | `FoneNro2` | `decimal(12,0)` | sim |
| 13 | `DDD_FONE_2` | `varchar(49)` | sim |
| 14 | `DDI_DDD_FONE_2` | `varchar(48)` | sim |
| 15 | `FoneDDD3` | `varchar(5)` | sim |
| 16 | `FoneNro3` | `decimal(12,0)` | sim |
| 17 | `DDD_FONE_3` | `varchar(49)` | sim |
| 18 | `DDI_DDD_FONE_3` | `varchar(48)` | sim |
| 19 | `Email` | `varchar(70)` | sim |
| 20 | `DtaInclusao` | `datetime` | sim |
| 21 | `DtaAlteracao` | `datetime` | sim |
| 22 | `DT_MAX` | `datetime` | sim |

---

## Familia `X_*` (4 views)

### X_CRM_BI_CONGLOMERADO

`view` · `11 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `cnpj_pessoa` | `varchar(15)` | sim |
| 2 | `seq_pessoa` | `numeric(10,0)` | **nao** |
| 3 | `status_pessoa` | `char(1)` | **nao** |
| 4 | `nome_pessoa` | `varchar(100)` | sim |
| 5 | `tipo_pessoa` | `char(1)` | sim |
| 6 | `cnpj_conglo` | `varchar(15)` | sim |
| 7 | `seq_conglo` | `numeric(10,0)` | **nao** |
| 8 | `status_conglo` | `char(1)` | **nao** |
| 9 | `cliente_conglo` | `varchar(100)` | sim |
| 10 | `tipo_conglo` | `char(1)` | sim |
| 11 | `principal` | `varchar(1)` | **nao** |

### X_CRM_BI_FATURAMENTO_PECA_CARTEIRA

`view` · `19 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `seq_carteira` | `decimal(4,0)` | sim |
| 2 | `carteira` | `varchar(12)` | **nao** |
| 3 | `cod_carteira` | `varchar(15)` | sim |
| 4 | `nome_carteira` | `varchar(40)` | sim |
| 5 | `seq_cip` | `numeric(18,0)` | sim |
| 6 | `nome_cip` | `varchar(20)` | sim |
| 7 | `filial` | `varchar(30)` | sim |
| 8 | `empresa` | `varchar(30)` | sim |
| 9 | `status_pessoa` | `char(1)` | **nao** |
| 10 | `seq_pessoa` | `varchar(15)` | sim |
| 11 | `cod_pessoa` | `numeric(10,0)` | **nao** |
| 12 | `cliente` | `varchar(100)` | sim |
| 13 | `cidade_pessoa` | `varchar(50)` | sim |
| 14 | `tipo_pessoa` | `char(1)` | sim |
| 15 | `seq_conglo` | `varchar(15)` | sim |
| 16 | `cod_conglo` | `numeric(10,0)` | **nao** |
| 17 | `cidade_conglo` | `varchar(50)` | sim |
| 18 | `grupo_conglo` | `varchar(100)` | sim |
| 19 | `principal` | `varchar(1)` | **nao** |

### X_CRM_BI_FUNIL_PECA_CARTEIRA

`view` · `21 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `departamento` | `varchar(10)` | **nao** |
| 2 | `seq_carteira` | `decimal(4,0)` | sim |
| 3 | `carteira` | `varchar(12)` | **nao** |
| 4 | `cod_carteira` | `varchar(15)` | sim |
| 5 | `nome_carteira` | `varchar(40)` | sim |
| 6 | `seq_cip` | `numeric(18,0)` | sim |
| 7 | `nome_cip` | `varchar(20)` | sim |
| 8 | `filial` | `varchar(30)` | sim |
| 9 | `empresa` | `varchar(30)` | sim |
| 10 | `status_pessoa` | `char(1)` | **nao** |
| 11 | `seq_pessoa` | `varchar(15)` | sim |
| 12 | `cod_pessoa` | `numeric(10,0)` | **nao** |
| 13 | `cliente` | `varchar(100)` | sim |
| 14 | `cidade_pessoa` | `varchar(50)` | sim |
| 15 | `tipo_pessoa` | `char(1)` | sim |
| 16 | `seq_conglo` | `varchar(15)` | sim |
| 17 | `cod_conglo` | `numeric(10,0)` | **nao** |
| 18 | `cidade_conglo` | `varchar(50)` | sim |
| 19 | `grupo_conglo` | `varchar(100)` | sim |
| 20 | `principal` | `varchar(1)` | **nao** |
| 21 | `prospeccao` | `varchar(7)` | **nao** |

### X_CRM_BI_FUNIL_PECA_CLASSE

`view` · `13 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `origem` | `varchar(14)` | **nao** |
| 2 | `cod_pessoa` | `numeric(10,0)` | **nao** |
| 3 | `nro_processo` | `numeric(18,0)` | sim |
| 4 | `dna` | `numeric(18,0)` | sim |
| 5 | `CLASSE` | `varchar(20)` | sim |
| 6 | `cod_acao` | `numeric(6,0)` | sim |
| 7 | `desc_acao` | `varchar(20)` | sim |
| 8 | `cod_resultado` | `decimal(6,0)` | sim |
| 9 | `resultado` | `varchar(40)` | sim |
| 10 | `compl_resultado` | `varchar(20)` | sim |
| 11 | `data_historico` | `datetime` | **nao** |
| 12 | `cip` | `varchar(20)` | sim |
| 13 | `CAMPANHA` | `varchar(20)` | sim |

---

## Familia `X_V_*` (3 views)

### X_V_BI_DESPESAS_VENDA_MAQUINAS

`view` · `28 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `ORIGEM` | `varchar(20)` | **nao** |
| 2 | `NRO_NF1` | `decimal(14,0)` | sim |
| 3 | `NF_DE_REFATURAMENTO` | `decimal(14,0)` | sim |
| 4 | `SEQQUESTIONARIO` | `numeric(18,0)` | sim |
| 5 | `TIPO_RECURSO` | `varchar(20)` | sim |
| 6 | `CHASSIS_CRM` | `varchar(30)` | sim |
| 7 | `CGCCPF_CRM` | `varchar(15)` | sim |
| 8 | `SEQ_PESSOA_CRM` | `numeric(10,0)` | sim |
| 9 | `NRO_PROCESSO_CRM` | `decimal(15,0)` | sim |
| 10 | `VLR_TOTAL_CRM` | `float(53,0)` | sim |
| 11 | `VALOR_FINANCIADO_CRM` | `float(53,0)` | sim |
| 12 | `VALOR_USADO_CRM` | `float(53,0)` | sim |
| 13 | `TAXA_FLAT_CRM` | `varchar(3)` | **nao** |
| 14 | `TAXA_FLAT_PERCENTUAL_CRM` | `decimal(10,2)` | sim |
| 15 | `VLR_TAXA_FLAT_CRM` | `float(53,0)` | sim |
| 16 | `BONIFICACAO_CRM` | `varchar(25)` | sim |
| 17 | `VLR_BONIFICACAO_CRM` | `float(53,0)` | sim |
| 18 | `DESC_INCONDICIONAL` | `varchar(60)` | sim |
| 19 | `VLR_DESC_INCONDICIONAL` | `float(53,0)` | sim |
| 20 | `VLR_INCENTIVO_LIQ_CRM` | `float(53,0)` | sim |
| 21 | `VLR_INCENTIVO_BRU_CRM` | `float(53,0)` | sim |
| 22 | `TP_FRETE_CRM` | `varchar(30)` | sim |
| 23 | `VLR_FRETE_CRM` | `float(53,0)` | sim |
| 24 | `TRANSPORTADORA_CRM` | `varchar(30)` | sim |
| 25 | `BANCO_CRM` | `varchar(20)` | sim |
| 26 | `FUNDO_RESERVA_CRM` | `float(53,0)` | sim |
| 27 | `VLR_COMISSAO_CRM` | `float(53,0)` | sim |
| 28 | `STATUS_COMISSAO` | `varchar(13)` | sim |

### X_V_CRM_IMP_IMP_NFS

`view` · `46 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `COD_CHAVE_NF` | `varchar(58)` | **nao** |
| 2 | `IdNFS` | `int(10,0)` | sim |
| 3 | `Origem` | `varchar(8)` | **nao** |
| 4 | `NroEmpresa` | `int(10,0)` | sim |
| 5 | `Nronf` | `numeric(18,0)` | sim |
| 6 | `Serienf` | `varchar(20)` | sim |
| 7 | `NroEmpresaVda` | `int(10,0)` | sim |
| 8 | `NroCNPJCPF` | `int(10,0)` | sim |
| 9 | `DigCNPJCPF` | `int(10,0)` | sim |
| 10 | `CNPJx` | `int(10,0)` | sim |
| 11 | `PessoaLinkOrigem` | `varchar(8)` | **nao** |
| 12 | `Pessoalink` | `varchar(8000)` | sim |
| 13 | `Segmento` | `varchar(10)` | **nao** |
| 14 | `Departamento` | `varchar(9)` | **nao** |
| 15 | `TipoVenda` | `varchar(6)` | **nao** |
| 16 | `CFOP` | `varchar(5)` | sim |
| 17 | `CodOperacao` | `varchar(3)` | **nao** |
| 18 | `Operacao` | `varchar(12)` | **nao** |
| 19 | `CanalVenda` | `varchar(6)` | **nao** |
| 20 | `Setor` | `int(10,0)` | sim |
| 21 | `Formapgto` | `varchar(5)` | sim |
| 22 | `CondicaoPgto` | `varchar(20)` | sim |
| 23 | `Vendedor` | `varchar(42)` | sim |
| 24 | `CodVendedor` | `varchar(6)` | **nao** |
| 25 | `Dtapedido` | `datetime` | sim |
| 26 | `Nropedido` | `varchar(8)` | sim |
| 27 | `Dtaemissaonf` | `datetime` | sim |
| 28 | `Situacao` | `varchar(1)` | sim |
| 29 | `CodTransportador` | `int(10,0)` | sim |
| 30 | `Transportador` | `int(10,0)` | sim |
| 31 | `Usuario` | `int(10,0)` | sim |
| 32 | `Obs` | `int(10,0)` | sim |
| 33 | `DtaAlteracaoERP` | `int(10,0)` | sim |
| 34 | `DtaGeracao` | `datetime` | sim |
| 35 | `StatusIMP` | `int(10,0)` | sim |
| 36 | `DtaImport` | `int(10,0)` | sim |
| 37 | `WhereItem` | `int(10,0)` | sim |
| 38 | `PERCBASECOMISSAO` | `int(10,0)` | sim |
| 39 | `IDNFSEXTERNO` | `int(10,0)` | sim |
| 40 | `LOTECARGA` | `int(10,0)` | sim |
| 41 | `PAIIDNFSEXTERNO` | `int(10,0)` | sim |
| 42 | `EVENTOCMPL` | `int(10,0)` | sim |
| 43 | `TIPOPEDIDO` | `int(10,0)` | sim |
| 44 | `IDENTIFICADO` | `int(10,0)` | sim |
| 45 | `NROCPFVENDEDOR` | `int(10,0)` | sim |
| 46 | `NROVOUCHER` | `int(10,0)` | sim |

### X_V_CRM_IMP_NFSItem

`view` · `41 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `COD_CHAVE_ITEM` | `varchar(97)` | **nao** |
| 2 | `IdNFSItem` | `int(10,0)` | sim |
| 3 | `Origem` | `varchar(8)` | **nao** |
| 4 | `NroEmpresa` | `int(10,0)` | sim |
| 5 | `Nronf` | `numeric(18,0)` | sim |
| 6 | `Serienf` | `varchar(20)` | sim |
| 7 | `NroItem` | `int(10,0)` | sim |
| 8 | `CodProduto` | `varchar(28)` | sim |
| 9 | `NCM` | `varchar(12)` | sim |
| 10 | `CodBarra` | `int(10,0)` | sim |
| 11 | `Departamento` | `varchar(9)` | **nao** |
| 12 | `Marca` | `varchar(6)` | sim |
| 13 | `CodFamilia` | `int(10,0)` | sim |
| 14 | `Familia` | `int(10,0)` | sim |
| 15 | `Descproduto` | `varchar(50)` | sim |
| 16 | `Qtde` | `float(53,0)` | **nao** |
| 17 | `VlrLiqItem` | `float(53,0)` | **nao** |
| 18 | `Vlrdescto` | `float(53,0)` | **nao** |
| 19 | `VlrResult` | `int(10,0)` | sim |
| 20 | `VlrCustoMkt` | `int(10,0)` | sim |
| 21 | `VlrICM` | `float(53,0)` | **nao** |
| 22 | `VlrImposto` | `int(10,0)` | sim |
| 23 | `Situacao` | `varchar(1)` | sim |
| 24 | `Vendedor` | `varchar(42)` | sim |
| 25 | `CodVendedor` | `varchar(6)` | **nao** |
| 26 | `Identificador` | `int(10,0)` | sim |
| 27 | `Obs` | `varchar(38)` | sim |
| 28 | `DtaGeracao` | `datetime` | sim |
| 29 | `StatusIMP` | `int(10,0)` | sim |
| 30 | `DtaImport` | `int(10,0)` | sim |
| 31 | `TIPO` | `int(10,0)` | sim |
| 32 | `LOTECARGA` | `int(10,0)` | sim |
| 33 | `SETORITEM` | `int(10,0)` | sim |
| 34 | `VLRCUSTO` | `int(10,0)` | sim |
| 35 | `IDNFSEXTERNO` | `int(10,0)` | sim |
| 36 | `VLRICMSRETIDO` | `int(10,0)` | sim |
| 37 | `VLRICMSSUBS` | `float(53,0)` | **nao** |
| 38 | `VLRPIS` | `float(53,0)` | **nao** |
| 39 | `VLRCOFINS` | `float(53,0)` | **nao** |
| 40 | `NROCPFVENDEDOR` | `int(10,0)` | sim |
| 41 | `VLRDESCTOVCHR` | `int(10,0)` | sim |

---

## Familia `VW_REL*` (1 views)

### VW_REL_TBA101

`view` · `30 colunas`

| ord | coluna | tipo | nulo? |
|---:|---|---|:---:|
| 1 | `EMPRESA` | `nvarchar(120)` | sim |
| 2 | `CEN` | `nvarchar(60)` | sim |
| 3 | `JD_QUOTE` | `nvarchar(120)` | sim |
| 4 | `TIPO_VENDA` | `nvarchar(120)` | sim |
| 5 | `TIPO_DE_EQUIPAMENTO` | `nvarchar(120)` | sim |
| 6 | `MARCA` | `nvarchar(120)` | sim |
| 7 | `MODELO` | `nvarchar(200)` | sim |
| 8 | `SEQPESSOA` | `numeric(10,0)` | sim |
| 9 | `NOMERAZAO` | `nvarchar(200)` | sim |
| 10 | `CNPJCPF` | `nvarchar(60)` | sim |
| 11 | `LINHA_CREDITO` | `nvarchar(120)` | sim |
| 12 | `INST_FINANCEIRA` | `nvarchar(120)` | sim |
| 13 | `FINANC_CHASSI` | `nvarchar(120)` | sim |
| 14 | `ULT_RESULTADO` | `nvarchar(400)` | sim |
| 15 | `DTA_FATURAMENTO_D` | `datetime` | sim |
| 16 | `NF_N` | `nvarchar(60)` | sim |
| 17 | `DTA_PED_D` | `datetime` | sim |
| 18 | `DTA_PRENCH_FORM_D` | `datetime` | sim |
| 19 | `VALOR_N` | `decimal(18,2)` | sim |
| 20 | `DTA_ULT_ANDAMENTO_D` | `datetime` | sim |
| 21 | `ULT_HISTORICO_L` | `nvarchar(max)` | sim |
| 22 | `PROCESSO_N` | `numeric(18,0)` | sim |
| 23 | `CARTEIRA` | `nvarchar(60)` | sim |
| 24 | `CIDADE` | `nvarchar(150)` | sim |
| 25 | `DATA_PEDIDO_HIST_D` | `datetime` | sim |
| 26 | `CAMPANHA` | `nvarchar(150)` | sim |
| 27 | `PROCESSODNA` | `numeric(18,0)` | sim |
| 28 | `DTAPROVADO` | `nvarchar(4000)` | sim |
| 29 | `DATA_MARCADO_ENTREGUE` | `varchar(10)` | sim |
| 30 | `STATUS` | `varchar(20)` | sim |

---
