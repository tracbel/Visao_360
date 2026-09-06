# Vórtice CRM — Pessoas, Segmentação, Financiamento e Documentos (GE_Pessoa, IVS_, IVF_, DMN_, IVP_, IVM_, OUT_, GEL_)

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

O núcleo de pessoas do Vórtice é um cadastro único (GE_Pessoa, 76 colunas, 118.463 linhas, vivo — última alteração 30/08/2026) que mistura cliente, prospect, suspect e falecido num único char(1) Status, cujo domínio real foi confirmado pela própria view do fornecedor IV$S_PESSOA: P=PROSPECT (76.048), A=ATIVO (39.624), S=SUSPECT (1.416), F=FALECIDO (54), O=OUTRO (25), I=INATIVO (5) e 2.126 linhas com espaço em branco (o CASE do fornecedor devolve '???'). Contato NÃO é pessoa: GE_Contato é entidade fraca com PK (SeqPessoa, SeqContato), sem FK e sem identidade própria — 625 contatos órfãos e 116 CPFs repetidos entre clientes. A carteirização multi-linha-de-negócio funciona e é o melhor ativo conceitual: IVS_Pes (139.036 linhas / 99.810 pessoas) permite até 7 carteiras por pessoa, uma por departamento/linha de negócio, com PK real (SeqPessoa, SeqDepto, SeqPesDepto) mais UNIQUE (SeqPessoa, SeqCarteira). Já a inteligência de segmentação prometida está MORTA: todo o RFV de IVS_Pes (Rec, Freq, Vlr, Pto, Score, Classe, Perspectiva, Ciclo, DtaCalc, QtdTrans) está zerado ou nulo em 100% das linhas; o que realmente segmenta é IVS_DEPTOPOT (potencial + DIASCICLOCTTO/DIASCICLOVISITA por departamento), preenchido em 109.118 linhas. Módulos inteiros nunca foram implantados: IVF_ (financiamento, 20 tabelas), IVM_ (materiais, 4), IVP_ (4) e GEL_ (CEP/logística, 6) têm ZERO linhas. Em LGPD o achado é grave: as views GE$PESSOA_LGPD / GE$CONTATO_LGPD / GE$PESSOAFONE_LGPD / GE$PESSOAEND_LGPD / GE$EMAIL_LGPD mascaram a coluna e, na mesma view, republicam o valor CRU em colunas z_NOMERAZAO, z_NROCGCCPF, z_EMAIL, z_FONENRO1 etc. — a anonimização é puramente decorativa e contornável com um SELECT.

## Entidades / objetos mapeados

### `GE_Pessoa`

**Funcao:** Cadastro mestre único de pessoas: cliente, prospect, suspect, falecido, fornecedor, funcionário — tudo na mesma tabela, diferenciado por Status char(1) e Grupo varchar(30). 76 colunas, 118.463 linhas, viva (última inclusão/alteração 30/08/2026). 50 FKs apontam para ela.

**Campos-chave:** SeqPessoa numeric(10,0) PK; Status char(1) NOT NULL; FisicaJuridica char(1); SEQPESSOAPRC numeric(10,0) NOT NULL (grupo econômico / titular principal, auto-referência SEM FK); NroCGCCPF decimal(13,0)+DigCGCCPF decimal(2,0); Versao decimal(2,0); Origem/UltOrigem varchar(20); CODVENDEDOR varchar(20)

**Relacoes:** → GE_Cidade.SeqCidade, GE_Bairro (2 FKs single-column separadas, não composta), GE_Regiao.SeqRegiao, GE_Rota.SeqRota. ← IVS_Pes, DMN_DocPes, IV_ClientePropr, GE_PessoaEmail, GE_PessoaJur, GE_PessoaNota, GE_PessoaPasw, GE_PessoaEndOutroBc, GE_PESSOAREDESOCIAL, GE_PessoaSimilar(SeqPessoa1), IVF_Financeira, IVM_MatPessoa, EXT_*, IV_*

### `GE_Contato`

**Funcao:** Contatos (pessoas de contato) DENTRO de um cliente. Entidade FRACA: 41.276 linhas, 41 colunas, PK composta (SeqPessoa, SeqContato) — o contato não existe fora do cliente. Não é GE_Pessoa. Só 2.148 têm CPF; 6.026 têm e-mail; NivelDecisao preenchido em 35.344 (85%).

**Campos-chave:** SeqPessoa+SeqContato (PK); TipoContato varchar(30) texto livre; Cpf numeric(18,0)+DigCPF; NivelDecisao char(1); Posicionamento char(1); EmUso numeric(1); FoneNro1/2 decimal(12,0); INDWHATSAPPF1/F2/FX

**Relacoes:** ZERO FKs declaradas (nem para GE_Pessoa) → 625 contatos órfãos. Complementada por GE_ContatoPapel (SeqPessoa+SeqContato+Papel varchar(20), 43.489 linhas) e GE_ContatoEmail (0 linhas)

### `IVS_Pes`

**Funcao:** Carteirização pessoa × departamento × carteira + segmentação RFV. 139.036 linhas / 99.810 pessoas distintas / 168 carteiras / 13 deptos. É o mecanismo que permite um cliente estar em várias linhas de negócio ao mesmo tempo (até 7 carteiras). As colunas de RFV estão 100% mortas.

**Campos-chave:** PK REAL = (SeqPessoa, SeqDepto, SeqPesDepto) — SCHEMA_MAP.md documenta errado. UNIQUE UNQ_IVS_PESCRT = (SeqPessoa, SeqCarteira). Mortas: Rec, Freq, Vlr, Pto, Score decimal(2,0), Classe varchar(30), Situacao varchar(2), Perspectiva, Ciclo, DtaCalc, QtdTrans, Recalc. Viva: SEQPOTENCIALDP numeric(6,0)

**Relacoes:** → GE_Pessoa.SeqPessoa (FK, 0 órfãos), IVS_Depto.SeqDepto, IVS_DEPTOPOT.SEQPOTENCIALDP. SeqCarteira NÃO tem FK para IVS_Carteira

### `IVS_Carteira / IVS_CartDepto / IVS_CartCid`

**Funcao:** Catálogo de carteiras comerciais (655 cadastradas, apenas 168 em uso). IVS_CartDepto liga carteira↔departamento (205 linhas — 449 das 655 carteiras não têm departamento). IVS_CartCid liga carteira↔cidade (670 linhas), base do roteamento geográfico.

**Campos-chave:** SeqCarteira decimal(4,0); NroEmpresa numeric(6,0); Carteira varchar(15); SeqVendedor numeric(18,0) (preenchido em 141/655 — é o 'CEN'); SeqUsrResp (655/655); SeqUrSuperv; SeqRegional (0/655 — morto); SeqCanal (0/655 — morto); AUTOSINCCID; AUTOSINCSTATUS

**Relacoes:** IVS_Carteira tem ZERO FKs. IVS_CartDepto → IVS_Carteira, IVS_Depto. IVS_CartCid → IVS_Carteira, GE_Cidade

### `IVS_Depto / IVS_Segm`

**Funcao:** Hierarquia organizacional comercial: IVS_Segm (8 segmentos: PEÇAS, VENDA, ADM, SERVIÇOS, AGRIC PRECISÃO, MARKETING, TI, BLOQUEADO) → IVS_Depto (29 departamentos = linhas de negócio: MAQ-NOVOS, MAQ-PEÇAS, DSI, VENDAS-DIGIT, EQUIP-*, PROSP-*...).

**Campos-chave:** IVS_Depto.SeqDepto decimal(4,0); Depto varchar(12); SeqSegm; MultCarteira numeric(1) (=1 SÓ em MAQ-NOVOS); CtrlPorEmpresa; CodProcesso decimal(4,0) → define o tipo de processo BPM padrão do depto; CicloA..CicloE + CicloAExt..CicloEExt decimal(3,0); AcaoContato/AcaoExterna numeric(8,0)

**Relacoes:** IVS_Depto → IVS_Segm, IV_CodProcesso, GE_Usuario. Complementos: IVS_DeptoEmpr (13), IVS_DeptoRes (63, resultados válidos por depto), IVS_DEPTOPOT (126)

### `IVS_DEPTOPOT`

**Funcao:** A segmentação que REALMENTE funciona: potencial do cliente POR DEPARTAMENTO, com SLA de cadência. 126 linhas. Referenciada por 109.118 linhas de IVS_Pes.

**Campos-chave:** SEQPOTENCIALDP numeric(6,0) PK; SEQDEPTO numeric(4,0); POTENCIAL varchar(12); ORDEM numeric(2,0); DIASCICLOCTTO numeric(4,0); DIASCICLOVISITA numeric(4,0)

**Relacoes:** ← IVS_Pes.SEQPOTENCIALDP (FK). → IVS_Depto

### `DMN_Doc / DMN_DocTp / DMN_DocPes / DMN_DocHst`

**Funcao:** Gestão documental. DMN_Doc 71.400 documentos, DMN_DocPes 76.520 vínculos com pessoa, DMN_DocHst 189.393 eventos. DMN_DocTp (107 tipos) é um motor de política documental declarativo — o melhor conceito do módulo.

**Campos-chave:** DMN_Doc: SeqDocto numeric(18,0) PK, SeqDocTp, SeqVrs decimal(4,0) (sempre 0), Arq varchar(250) = CAMINHO RELATIVO no FTP ('000000\12\30\123090\123090_77022_000033261.pdf'), Ext varchar(5), Status char(1) (sempre 'N'), DtaCkIn/UsuCkIn/DtaCkOut/UsuCkOut, Validade, QtdeCons. DMN_DocTp: ExigArq/ExigAutent/ExigDtaBase/ExigValidade, VincPessoa/VincProcesso/VincPropriedade/VincProjeto/VINCOS, UmPorPessoa/UmPorProcesso/UmPorPropriedade/UmPorProjeto/UMPOROS, TamanhoMax, EXTENSOES varchar(80)

**Relacoes:** DMN_Doc tem ZERO FKs. DMN_DocPes → GE_Pessoa (3 FKs duplicadas) mas NENHUMA para DMN_Doc → 5.480 vínculos órfãos. DMN_DocProp → IV_ClientePropr. DMN_DocProj → IV_Projeto. DMN_DocArq/DocVrs/DocObs/DocProj/DocProp = 0 linhas

### `IV_ClientePropr / IV_Propriedade`

**Funcao:** Construtor de entidades customizadas por cliente (o análogo de 'GE_PessoaProp'). IV_Propriedade (52 tipos) define os rótulos dos slots; IV_ClientePropr (246.813 linhas / 55.544 pessoas, ativo em ago/2026) guarda as instâncias. Tipos reais: Trator, Colheitadeira Grãos, Plantadeira, Pulverizador, Colhedora de Cana, Implemento, Agricultura Precisão, Frota, Seguro, Operations Center, Origem da Receita, Locação, Contrato GFC.

**Campos-chave:** IV_ClientePropr: SeqPropPessoa numeric(18,0) PK; SeqPessoa; SeqPropriedade; Referencia/Identificador varchar(30); Ativo char(1); Campo1..Campo8 varchar(40); Numero1..Numero6 decimal(15,2); Data1..Data6 datetime; SimNao1..SimNao6 numeric(1); Literal1..Literal10 varchar(40). IV_Propriedade: 70 colunas de metadados (Campo1..Campo6 + Campo1Sql.., Numero1..6, Data1..6, UmPorPessoa, CampoListar, Nivel)

**Relacoes:** → GE_Pessoa (4 FKs, 0 órfãos). ← DMN_DocProp. Listas de valores em IV_PropriLista (7.375)

### `GE_Email / GE_PessoaEmail / GE_Pessoa.Email`

**Funcao:** TRÊS modelos de e-mail concorrentes. GE_Email (39.363 linhas) é o real e tem o melhor modelo de finalidade por canal. GE_PessoaEmail tem 0 linhas. GE_Pessoa.Email varchar(70) tem 35.959 preenchidos.

**Campos-chave:** GE_Email: SeqEmail numeric(10,0) PK; eMail varchar(70) NOT NULL; SeqPessoa; SeqUsuario; IndPreferencial; IndUsoMkt (1=32.221 / 0=1.104 / NULL=6.038); IndUsoProfissional; IndUsoPessoal; IndUsoFiscal; IndEmUso (1=39.358); MOTIVO varchar(30) (só 6 preenchidos); DTAEMAILATIVO (0 preenchidos); Senha varchar(50) (vazia, mas existe no schema)

**Relacoes:** GE_Email tem ZERO FKs. GE_PessoaEmail → GE_Pessoa (1 FK) mas está vazia

### `GE_PessoaFone`

**Funcao:** Telefones (113.567 linhas). Modelo tabular correto, mas convive com as 3 colunas FoneNro1/2/3 desnormalizadas dentro de GE_Pessoa.

**Campos-chave:** SeqPesFone numeric(18,0) PK; TipoFoneSeqPar numeric(18,0) → GE_ParamLista('CRMPES_TIPOFONE': 69=Fixo, 70=Comercial, 71=Celular); SeqPessoa; DDD varchar(5); Numero numeric(12,0); IndFonePref; INDEMUSO; MOTIVOEMUSO varchar(30); INDUSOMKT (1=65.198 / NULL=48.186); INDWHATSAPP (1 em apenas 1 linha); DTAULTSUCESSO/DTAULTINSUCESSO

**Relacoes:** ZERO FKs → 246 órfãos de pessoa e 2.027 linhas com TipoFoneSeqPar (92/93/94) inexistente no catálogo

### `IVF_* (financiamento)`

**Funcao:** Módulo de financiamento COMPLETO no schema e com ZERO linhas em TODAS as 20 tabelas: IVF_Financeira(23 col), IVF_Plano(19), IVF_Prazo(3), IVF_Proposta(60), IVF_PropostaHst/Obs/Result, IVF_Tabela(24), IVF_TabelaFiltro, IVF_TabEmpr, IVF_TabIndice, IVF_Acordo, IVF_Agregado, IVF_AgregCC, IVF_TipoAgregado, IVF_TpAgrPessoa, IVF_FinancEmpr, IVF_FinancImpTx, IVF_Molicar, IVF_PlanoIndic. O catálogo de status da proposta (GE_ParamLista 'CRMFI_SITUACAO', 12 itens, criado em 02/01/2012) existe mas nunca foi usado.

**Campos-chave:** IVF_Financeira.SeqFinanc; IVF_Proposta.SeqProposta + ProcessoFN → IV_FichaNegVeic.Processo; IVF_Tabela.SeqTabela; IVF_Plano.SeqPlano + Prazo

**Relacoes:** IVF_Financeira → GE_Pessoa. IVF_Proposta → IVF_Financeira, IVF_Plano, IV_FichaNegVeic. IV_FichaNegVeic → IVF_Financeira

### `OUT_* (outbox de integração)`

**Funcao:** Padrão outbox para publicar mudanças de cadastro para sistemas externos. OUT_Pessoa (1.918, snapshot de 61 colunas), OUT_Contato (147), OUT_PessoaRelacao (9), OUT_PessoaLink (122), OUT_Log (1.765, a fila). Quebrado: OutOk=0 em 100% das linhas.

**Campos-chave:** OUT_Log: SeqLog numeric(18,0); Tabela varchar(20); TpOper char(1) (I=inclusão, A=alteração, E=exclusão); Chave1N/Chave2N/Chave1C; DtaIns; OutOk numeric(1); OutDta

**Relacoes:** Sem FKs. OUT_PROCESSO 'E' ainda gera eventos até 25/08/2026; OUT_PESSOA 'A' parou em 19/01/2017

### `GE_PessoaLink`

**Funcao:** Cross-reference de identidade entre o CRM e sistemas externos — o melhor padrão de integração do banco. 32.831+ linhas. Origens reais: INTEGRAÇÃONOROESTE (28.037, carga de 3 dias em dez/2024), Protheus (4.063), RD (883, ativa até 30/08/2026), EEMOVEL (220), PES_JUNCAO (117, rastro de fusão de cadastros).

**Campos-chave:** SEQPESSOALINK numeric(18,0); Pessoalink varchar(250) (o ID no sistema externo); Origem varchar(20) NOT NULL; SeqPessoa; DtaGeracao

**Relacoes:** ZERO FKs. Espelho de saída em OUT_PessoaLink

### `GE_PessoaSimilar`

**Funcao:** Motor de detecção de duplicatas + fila de curadoria. 124.222 pares candidatos, 124.213 marcados Duplicado=1, SeqPessoaFica sugerido em 123.693 — mas apenas 9 revisados e a última revisão foi em 27/02/2018. Fila abandonada.

**Campos-chave:** SeqPessoa1+SeqPessoa2; Probabilidade decimal(3,0); DtaGeracao (última: 12/09/2025); Revisado; DtaRevisao; Duplicado; Parecer varchar(30); UsuParecer; SeqPessoaFica; ObsFica; UsuEmTrabalho/DtaEmTrabalho (lock de curadoria)

**Relacoes:** Só SeqPessoa1 tem FK para GE_Pessoa; SeqPessoa2 não tem. Busca fonética em GE_PessoaFonema (455.993) e GE_PessoaNomeFonema (151.583)

### `GE_Cidade / GE_Bairro / GE_Regiao / GE_Rota / GEL_*`

**Funcao:** Geografia. GE_Cidade (10.214) e GE_Bairro são usados; GE_Regiao e GE_Rota têm 0 linhas apesar de GE_Pessoa.SeqRegiao/SeqRota terem FK. Todo o stack paralelo GEL_ (GEL_CEP, GEL_CepOrig, GEL_CepAlerta, GEL_Cidade, GEL_Conv, GEL_ConvDe) tem 0 linhas — modelo de CEP/logística duplicado e nunca implantado.

**Campos-chave:** GE_Cidade: SeqCidade decimal(6,0), Cidade varchar(50), Uf varchar(2), CEPInicial/CEPFinal varchar(12), Ddd, Populacao, DtaFeriado1/2, ExgBairro/ExgLogradouro char(1). GE_Bairro: PK (SeqCidade, SeqBairro)

**Relacoes:** GE_CIdadePref.SeqCidade → GE_Cidade_CRM (tabela legada/backup!), não para GE_Cidade

### `GE_ParamLista`

**Funcao:** Catálogo genérico único ('uma tabela para todos os dropdowns') com slots ListaStr/ListaNro/Str1..Str5/Nro1..Nro6/Ind1..Ind4/DTA1..DTA2/STRL1 discriminados pela coluna Parametro. Contém CRMPES_TIPOFONE(3), CRMPES_ATIVIDADEPF(11), CRMPES_ATIVIDADEPJ(11), CRMPES_FAIXAFATURA(12), CRMFI_SITUACAO(12), CRM_DESTINOTBL(12), RDSTATION_MAP_FIELD(148), RDSTATION_MAP_EVENT(11), RDSTATION_MAP_CAB(10).

**Campos-chave:** SeqParamLista; Parametro varchar; NroEmpresa; ListaStr; ListaNro; Str1..Str5; Nro1..Nro6; Ind1..Ind4

**Relacoes:** Referenciada por SeqPar em GE_PessoaFone.TipoFoneSeqPar, GE_PessoaFis.*SeqPar — sempre SEM FK

### `IVM_* / IVP_* (materiais e produtos)`

**Funcao:** IVM_Material, IVM_MatPessoa, IVM_ProcMat, IVM_ProcMatItem (materiais aplicados a processos) e IVP_TabPreco, IVP_Vendedor, IVP_PedCritica, IVP_ProdImagem: TODOS com 0 linhas. Módulos entregues e nunca ativados.

**Campos-chave:** IVM_Material.SeqMaterial; IVM_ProcMatItem.Qtde/vlrUnitario/DescTotal; IVP_TabPreco.Tabela+SeqCanal+SeqDepto+VigorDe; IVP_Vendedor.SalarioFixo/Comissao1/Comissao2

**Relacoes:** IVM_MatPessoa → GE_Pessoa, IVM_Material. IVM_ProcMat → IV_Processo. IVP_TabPreco → IVS_CanalVenda (também vazia), IVS_Depto

## Achados

### 1. GE_Pessoa completa — as 76 colunas, com os erros de tipagem apontados

1 SeqPessoa numeric(10,0) NOT NULL [PK] · 2 SeqCidade decimal(6,0) · 3 SeqBairro decimal(5,0) · 4 Versao decimal(2,0) · 5 Status char(1) NOT NULL · 6 DtaAtivacao datetime · 7 NomeRazao varchar(100) · 8 Fantasia varchar(50) · 9 PalavraChave varchar(50) · 10 FisicaJuridica char(1) · 11 Sexo char(1) · 12 Cidade varchar(50) · 13 Uf varchar(2) · 14 Pais varchar(25) · 15 Bairro varchar(50) · 16 TipoLogradouro varchar(15) · 17 Logradouro varchar(80) · 18 NroLogradouro varchar(10) · 19 CmpltoLogradouro varchar(30) · 20 Cep varchar(12) · 21 CxPostal varchar(7) · 22 SeqPessoaEndCobr decimal(3,0) · 23 FoneDDD1 varchar(5) · 24 FoneNro1 decimal(12,0) · 25 FoneCmpl1 varchar(20) · 26 FoneDDD2 varchar(5) · 27 FoneNro2 decimal(12,0) · 28 FoneCmpl2 varchar(20) · 29 FoneDDD3 varchar(5) · 30 FoneNro3 decimal(12,0) · 31 FoneCmpl3 varchar(20) · 32 FaxDDD varchar(5) · 33 FaxNro decimal(12,0) · 34 NroCGCCPF decimal(13,0) · 35 DigCGCCPF decimal(2,0) · 36 InscricaoRG varchar(20) · 37 UFEmissor varchar(2) · 38 OrgaoEmissor varchar(10) · 39 InscMunic varchar(15) · 40 InscProdutor varchar(20) · 41 CNAE varchar(15) · 42 DtaNascFund datetime · 43 Origem varchar(20) · 44 UltOrigem varchar(20) · 45 Email varchar(70) · 46 HomePage varchar(80) · 47 EstadoCivil varchar(20) · 48 Atividade varchar(30) · 49 RendaFaturamento varchar(30) · 50 GrauInstrucao varchar(30) · 51 Grupo varchar(30) · 52 Porte varchar(30) · 53 DtaInclusao datetime · 54 UsuInclusao varchar(20) · 55 DtaAlteracao datetime · 56 UsuAlteracao varchar(20) · 57 DtaInativacao datetime · 58 UsuInativacao varchar(20) · 59 ObsInativacao varchar(50) · 60 CODVENDEDOR varchar(20) · 61 Telefonema numeric(1,0) · 62 Correspondencia numeric(1,0) · 63 RecebeEmail numeric(1,0) · 64 NaoPossuiEmail numeric(1,0) · 65 ProblemaCredito numeric(1,0) · 66 IndContribICMS char(1) · 67 RefEndereco varchar(150) · 68 Latitude decimal(14,11) · 69 Longitude decimal(14,11) · 70 SeqRegiao decimal(6,0) · 71 SeqRota decimal(6,0) · 72 RecebeSMS numeric(1,0) · 73 SEQPESSOAPRC numeric(10,0) NOT NULL · 74 Skype varchar(70) · 75 SMSCODIGO varchar(8) · 76 SMSCODIGODTA datetime.

ERROS DE TIPAGEM CONFIRMADOS: (a) FoneNro1/2/3 e FaxNro são decimal(12,0) — número de telefone tratado como NÚMERO. Consequências medidas: 166 linhas em GE_Pessoa e 175 em GE_PessoaFone têm menos de 8 dígitos (zero à esquerda perdido ou dado truncado) e 2 linhas têm 10+ dígitos (DDD embutido). Impossível guardar +55, ramal, ou formatação; o DDD tem de morar em coluna separada varchar(5). (b) NroCGCCPF decimal(13,0) + DigCGCCPF decimal(2,0) — CPF/CNPJ quebrado em corpo+dígito numérico: perde zeros à esquerda (CPF 012.345.678-90 vira 12345678) e obriga toda query a reconcatenar. (c) Cidade/Uf/Bairro/Pais duplicam GE_Cidade/GE_Bairro em texto livre: 2.394 valores distintos em Cidade contra 2.293 SeqCidade distintos, e 118.078 linhas com texto contra 114.645 com FK — 3.433 pessoas têm nome de cidade sem cidade cadastrada. (d) Telefonema/Correspondencia/RecebeEmail/RecebeSMS/NaoPossuiEmail/ProblemaCredito são numeric(1,0) NULLABLE — booleano de três estados. (e) EstadoCivil é varchar(20) embora a view do fornecedor IV$S_PESSOA faça CASE sobre char(1) (C/S/D/Q/V).

**Evidencia:** schema/colunas.csv (grep '^"GE_Pessoa"'); queries de contagem por faixa de dígitos em GE_Pessoa.FoneNro1 e GE_PessoaFone.Numero; COUNT(DISTINCT Cidade) vs COUNT(DISTINCT SeqCidade)

**Licao para a Tracbel:** EVITAR integralmente. No CRM próprio: telefone = tabela `contato_telefone` com `e164 varchar(20)` (normalizado +55DDNNNNNNNNN) + `ddd`, `numero`, `ramal`, `tipo_id` FK, `is_whatsapp bool`, `verificado_em`. Documento = `documento varchar(14)` texto puro com dígitos, com CHECK de tamanho (11=CPF, 14=CNPJ) e validação de DV na aplicação, NUNCA numérico e NUNCA quebrado em corpo+dígito. Endereço = só FK para município (IBGE), sem colunas de texto redundantes. Flags de consentimento = NOT NULL DEFAULT false, nunca tri-state.

### 2. O domínio REAL de GE_Pessoa.Status — confirmado pela própria view do fornecedor

A view IV$S_PESSOA do fornecedor traz o CASE canônico: 'P'→PROSPECT, 'S'→SUSPECT, 'A'→ATIVO, 'I'→INATIVO, 'F'→FALECIDO, 'O'→OUTRO, ELSE '???'. Distribuição medida em produção (118.463 linhas): P=76.048 (64,2%), A=39.624 (33,4%), ' ' espaço em branco=2.126 (1,8%, ASCII 32, LEN=0/DATALENGTH=1 — a coluna é NOT NULL, então gravaram espaço para burlar), S=1.416, F=54, O=25, I=5.

O Status é um funil (Suspect → Prospect → Ativo) MISTURADO com um estado de ciclo de vida (Inativo) e com um fato civil (Falecido) na mesma coluna. Isso é semanticamente inconsistente: um cliente ATIVO que morre perde a informação de que era ativo. Pior: 'A' não significa 'comprou' — só 6.599 dos 39.624 ATIVOS têm nota fiscal em EXT_NFS, e 288 PROSPECTs têm NF. Ou seja, o Status é uma marcação manual/ERP que não reflete transação.

Agravante decisivo: DtaAtivacao está NULA em 100% das 118.463 linhas e DtaInativacao também está NULA em 100%. Existem as colunas UsuInativacao e ObsInativacao e elas também não são usadas. Não há NENHUM registro de quando ou por que uma pessoa mudou de status — não existe trilha do funil.

**Evidencia:** OBJECT_DEFINITION de IV$S_PESSOA (CASE STATUS WHEN 'P' THEN 'PROSPECT'...); SELECT Status, ASCII(Status), LEN, DATALENGTH, COUNT(*) FROM GE_Pessoa GROUP BY Status; SUM(CASE WHEN DtaAtivacao IS NOT NULL...) por Status = 0 em todos

**Licao para a Tracbel:** SEPARAR três eixos hoje colapsados: (1) `estagio_funil` (suspect/prospect/lead/cliente) como enum + tabela `pessoa_estagio_hist` com data_de/data_ate/motivo/usuario — funil é série temporal, não coluna; (2) `ativo boolean` para o ciclo de vida operacional; (3) `falecido_em date` como atributo factual, não status. Regra: nenhuma coluna de estado sem tabela de histórico correspondente. E 'cliente' deve ser DERIVADO de existir faturamento, não digitado à mão — a divergência 39.624 ATIVOS vs 6.599 com NF é a prova do custo de deixar humano marcar.

### 3. 🔴 LGPD: as views de anonimização republicam o dado cru na MESMA view (colunas z_*)

Existem 5 views de mascaramento — GE$PESSOA_LGPD, GE$CONTATO_LGPD, GE$PESSOAFONE_LGPD, GE$PESSOAEND_LGPD, GE$EMAIL_LGPD — apoiadas na função dbo.fva_StrMaskLGPD(texto, n_inicio, n_fim) que preserva as n primeiras e n últimas letras e troca o miolo por '*'.

O defeito: cada view mascara a coluna E ENTÃO adiciona a coluna original sem máscara com prefixo z_. Em GE$PESSOA_LGPD são 19 colunas cruas republicadas: z_NOMERAZAO, z_FANTASIA, z_PALAVRACHAVE, z_LOGRADOURO, z_NROLOGRADOURO, z_EMAIL, z_SKYPE, z_NROCGCCPF, z_FONEDDD1, z_FONENRO1, z_FONEDDD2, z_FONENRO2, z_FONEDDD3, z_FONENRO3, z_FAXDDD, z_FAXNRO, z_DTANASCFUND. Em GE$CONTATO_LGPD: z_RG, z_CPF, z_SAUDACAO, z_CONTATO, z_FONENRO1, z_FONENRO2, z_FAXNRO, z_DTANASCIMENTO, z_EMAIL, z_SKYPE. Idem em GE$PESSOAFONE_LGPD (z_NUMERO), GE$PESSOAEND_LGPD (z_LOGRADOURO, z_NROLOGRADOURO) e GE$EMAIL_LGPD (z_EMAIL).

Qualquer usuário com acesso à view 'anonimizada' obtém o dado pessoal íntegro com `SELECT z_NOMERAZAO, z_NROCGCCPF FROM GE$PESSOA_LGPD`. A anonimização é decorativa.

Segundo defeito, independente: a máscara só é aplicada quando FisicaJuridica='F'. Existem 1.696 linhas com FisicaJuridica inválido (NULL=1.663, '0'=32, 'O'=1) — todas passam pelo ramo ELSE e saem SEM máscara, mesmo sendo pessoas físicas. Terceiro: o mascaramento é fraco por construção — CPF é `nrocgccpf % 10000` (preserva os 4 últimos dígitos), o CPF do contato é `CPF % 1000`, e a data de nascimento vira ano 1804 preservando dia e mês exatos (aniversário completo continua exposto, que é o dado usado em campanha).

**Evidencia:** sys.sql_modules/OBJECT_DEFINITION das views GE$PESSOA_LGPD, GE$CONTATO_LGPD, GE$PESSOAFONE_LGPD, GE$PESSOAEND_LGPD, GE$EMAIL_LGPD e da função fva_StrMaskLGPD; SELECT FisicaJuridica, ASCII(FisicaJuridica), COUNT(*) FROM GE_Pessoa GROUP BY FisicaJuridica

**Licao para a Tracbel:** EVITAR de forma absoluta — este é o antipadrão mais perigoso do banco e não pode ser copiado nem 'melhorado'. No CRM próprio: mascaramento NUNCA na camada de view com escape; usar Dynamic Data Masking / RLS do SQL Server ou, melhor, resolver na aplicação com um único serviço de leitura de PII que decide por papel do usuário e REGISTRA todo acesso (`pii_access_log`: quem, qual titular, qual campo, quando, para quê). Regra de ouro: se existe um caminho de leitura que devolve o dado cru, o mascaramento não existe. Além disso, mascarar por FINALIDADE e por PAPEL, nunca por um campo de dados (FisicaJuridica) que pode estar sujo. E anonimizar data de nascimento significa remover dia/mês, não só o ano.

### 4. 🔴 LGPD: opt-in sem data, sem origem e sem prova de consentimento

O consentimento vive em 4 flags numeric(1,0) NULLABLE dentro de GE_Pessoa: RecebeEmail (1=93.881 / 0=4.104 / NULL=21.313), RecebeSMS (1=94.797 / 0=3.098 / NULL=21.403), Telefonema (1=63.785 / 0=2.652 / NULL=52.861), Correspondencia (1=63.359 / 0=3.208 / NULL=52.731). Mais NaoPossuiEmail (1=572) — um flag negativo que inverte a lógica de leitura.

Três problemas estruturais: (1) NULL é ambíguo — 21.313 pessoas não têm posição registrada sobre e-mail e 52.861 sobre telefone. A view IV$S_PESSOA resolve com `CASE TELEFONEMA WHEN 1 THEN 'SIM' ELSE 'NÃO' END`, ou seja, o fornecedor colapsa 'desconhecido' em 'não' no relatório, enquanto o número bruto (94% marcados com 1) sugere que a operação trata o default como opt-in. (2) Não existe QUANDO nem COMO: nenhuma coluna DtaOptIn, OrigemConsentimento, TextoAceite, IP, versão da política. Não é possível provar o consentimento perante a ANPD. (3) Não existe REVOGAÇÃO rastreável: a flag é sobrescrita, sem histórico.

Em GE_Email o modelo é conceitualmente melhor — consentimento por FINALIDADE (IndUsoMkt, IndUsoProfissional, IndUsoPessoal, IndUsoFiscal, IndEmUso) + MOTIVO varchar(30) + DTAEMAILATIVO datetime. Mas na prática: DTAEMAILATIVO está NULA em 100% das 39.363 linhas e MOTIVO preenchido em apenas 6. O campo de data de consentimento existe e nunca foi usado. Em GE_PessoaFone existe INDUSOMKT (1 em 65.198, NULL em 48.186) sem data nem motivo. GE_PessoaDestino (opt-in por canal de envio, ligada a GEP_ParEnvia) tem 0 linhas.

Complemento LGPD relevante: GE_PessoaClasse marca 71 pessoas como 'FALECIDO' e GE_Pessoa.Status tem 54 com 'F' — dois lugares para o mesmo fato, sem processo de expurgo.

**Evidencia:** Contagens por flag em GE_Pessoa (GROUP BY RecebeEmail/RecebeSMS/Telefonema/Correspondencia); GE_Email WHERE DTAEMAILATIVO IS NOT NULL = 0; GE_Email WHERE MOTIVO <> '' = 6; GE_PessoaDestino COUNT = 0; view IV$S_PESSOA (CASE ... ELSE 'NÃO')

**Licao para a Tracbel:** COPIAR o conceito de finalidade por canal do GE_Email (marketing / profissional / pessoal / fiscal são bases legais diferentes na LGPD), mas materializá-lo como EVENTO, não como flag. Modelo: `consentimento (id, pessoa_id, canal enum[email,sms,voz,whatsapp,correio], finalidade enum[marketing,transacional,fiscal], estado enum[concedido,negado,revogado], base_legal enum[consentimento,legitimo_interesse,contrato,obrigacao_legal], concedido_em, revogado_em, origem varchar, evidencia jsonb, politica_versao)` — append-only, o estado atual é a última linha. Nunca NULL: ausência de linha = sem consentimento (opt-out por padrão), o que já resolve os 21.313 casos ambíguos. Adicionar `pessoa_id` + `direito_titular` (acesso, correção, portabilidade, eliminação) com SLA, porque a LGPD exige atender pedido do titular e o Vórtice não tem onde registrar isso.

### 5. Cliente × Prospect × Contato: contato é entidade FRACA, sem identidade e sem FK

O Vórtice usa cadastro ÚNICO para cliente e prospect: ambos são linhas de GE_Pessoa diferenciadas só por Status (P vs A). Isso é BOM — evita a migração 'converter lead em cliente' e preserva todo o histórico de IV_Historico/IV_Agenda na mesma SeqPessoa.

Mas o contato é modelado de forma oposta e ruim: GE_Contato tem PK composta (SeqPessoa, SeqContato decimal(4,0)) — é uma entidade FRACA que só existe dentro de um cliente. Consequências medidas em 41.276 contatos: (a) ZERO FKs declaradas na tabela, resultando em 625 contatos órfãos apontando para SeqPessoa inexistente; (b) sem identidade própria — apenas 2.148 contatos têm CPF, e há 2.032 CPFs distintos entre eles, ou seja 116 CPFs se repetem: a mesma pessoa física é comprador em 2+ clientes e vira N linhas desconexas, sem visão 360; (c) só 6.026 dos 41.276 (14,6%) têm e-mail — o contato, que é quem realmente recebe a comunicação, é o registro pior preenchido do banco.

O papel do contato está partido em dois lugares: GE_Contato.TipoContato varchar(30) (relação com o cliente) e GE_ContatoPapel.Papel varchar(20) (função comercial, 43.489 linhas, N papéis por contato). O segundo é o modelo certo (many-to-many). GE_Contato.NivelDecisao char(1) está preenchido em 35.344 (85%) — é o campo de qualificação que realmente funciona.

Existem ainda GE_CONTATOAPP (0 linhas, login do contato no app com SENHA varchar(250) e FCMTOKEN) e GE_ContatoEmail (0 linhas).

**Evidencia:** sys.indexes: PK__GE_Conta__3F7BFE1F99FB1C33 = (SeqPessoa, SeqContato); COUNT de FKs em sys.foreign_keys para GE_Contato = 0; LEFT JOIN GE_Contato×GE_Pessoa = 625 órfãos; COUNT(*) com CPF=2.148 vs COUNT(DISTINCT Cpf)=2.032; contatos com e-mail = 6.026

**Licao para a Tracbel:** COPIAR o cadastro único cliente/prospect (um só `pessoa`, com estágio de funil). EVITAR a entidade fraca: no CRM próprio o contato deve ser uma `pessoa` de verdade (pessoa física com id próprio e documento), e o vínculo com a empresa vira `pessoa_vinculo (pessoa_fisica_id, organizacao_id, papel_id, nivel_decisao, ativo, desde, ate)` — many-to-many com temporalidade. Assim a mesma pessoa que troca de fazenda leva o histórico junto e a Tracbel enxerga que aquele comprador atende 3 clientes. Manter o conceito de N papéis por vínculo (GE_ContatoPapel acertou) e o `nivel_decisao` (85% de preenchimento prova que o vendedor usa). Papel e tipo de contato viram tabelas de catálogo com FK, nunca varchar.

### 6. Carteirização multi-linha-de-negócio: como funciona de verdade (o melhor ativo do modelo)

A resposta concreta: um cliente fica em VÁRIAS carteiras porque IVS_Pes tem uma linha por (pessoa, departamento) e cada departamento é uma linha de negócio. Distribuição medida sobre 99.810 pessoas: 82.063 em 1 carteira, 8.132 em 2, 1.720 em 3, 4.845 em 4, 2.197 em 5, 787 em 6 e 66 em 7 carteiras — total 139.036 linhas.

A PK real (sys.indexes) é PK__IVS_Pes__1295DCF6 = (SeqPessoa, SeqDepto, SeqPesDepto) — NÃO inclui SeqCarteira, ao contrário do que SCHEMA_MAP.md documenta. Existe ainda um UNIQUE separado UNQ_IVS_PESCRT = (SeqPessoa, SeqCarteira), que impede a mesma pessoa de estar duas vezes na mesma carteira. SeqPesDepto é o discriminador que permitiria MAIS DE UMA carteira no MESMO departamento, governado por IVS_Depto.MultCarteira numeric(1) — mas MultCarteira=1 apenas no depto 2 (MAQ-NOVOS) e, na prática, SeqPesDepto=1 em 139.032 das 139.036 linhas (só 4 linhas com valor 2). Ou seja: o modelo suporta multi-carteira por linha de negócio, mas a operação usa 1 carteira por linha de negócio.

Deptos por volume: VENDAS-DIGIT 54.117, MAQ-NOVOS 41.673, DSI 12.330, MAQ-PNEUS 9.252, DSI-PUK 8.858, DADOS-CADAST 5.944, MAQ-AMS 4.030, MAQ-PEÇAS 2.518, PECAS_EXTERN 288, e caudas mínimas em PROSP-PECAS/PROSP-MAQ/MAQ-SERV/IRRIGACAO.

Hierarquia: IVS_Segm (8) → IVS_Depto (29) → IVS_Carteira (655) → IVS_Pes. A carteira aponta o dono por SeqVendedor (o 'CEN', preenchido em 141/655), SeqUsrResp (655/655) e SeqUrSuperv. O roteamento geográfico está em IVS_CartCid (carteira × cidade, 670 linhas) com AUTOSINCCID/AUTOSINCSTATUS para re-carteirizar automaticamente por cidade.

Defeitos: (a) 655 carteiras cadastradas mas só 168 em uso — 74% de catálogo morto; (b) 449 das 655 carteiras NÃO têm linha em IVS_CartDepto, então o vínculo carteira→departamento é desconhecido para a maioria, e existem DOIS caminhos concorrentes para saber o depto (IVS_Pes.SeqDepto direto vs IVS_CartDepto) que podem divergir; (c) IVS_Carteira tem ZERO FKs — SeqCarteira em IVS_Pes não é validado contra o catálogo; (d) 19.488 pessoas (16,5% de 118.463) não têm nenhuma linha em IVS_Pes, isto é, não estão carteirizadas.

**Evidencia:** sys.indexes sobre IVS_Pes; GROUP BY n_carteiras por SeqPessoa; COUNT(DISTINCT SeqCarteira) em IVS_Pes=168 vs IVS_Carteira=655; LEFT JOIN IVS_Carteira×IVS_CartDepto = 449 sem depto; LEFT JOIN GE_Pessoa×IVS_Pes = 19.488 sem carteirização; GROUP BY SeqPesDepto

**Licao para a Tracbel:** COPIAR o conceito, que é excelente e raro: carteira é por LINHA DE NEGÓCIO, não global — o mesmo cliente pode ter vendedor de máquinas, vendedor de peças e especialista DSI simultaneamente. Modelo próprio: `carteira (id, linha_negocio_id, filial_id, responsavel_id, supervisor_id, ativo)` + `carteira_pessoa (carteira_id, pessoa_id, desde, ate, motivo_entrada, motivo_saida)` com UNIQUE parcial (pessoa_id, linha_negocio_id) WHERE ate IS NULL. ADAPTAR: (1) tornar a atribuição TEMPORAL — hoje a troca de carteira sobrescreve e a Tracbel não sabe quem era o dono na data da venda (isso é a raiz do problema já documentado de aprovação/comissão retroativa); (2) uma única fonte para carteira→linha de negócio (derivar da carteira, não gravar em duplicidade na linha da pessoa); (3) FK obrigatória sobre carteira_id; (4) regra explícita para os 16,5% sem carteira — carteira 'sem dono/pool' em vez de ausência de linha.

### 7. 🔴 O motor RFV do IVS_Pes está 100% MORTO — 10 colunas de segmentação nunca calculadas

IVS_Pes carrega o vocabulário completo de RFV mas nenhuma delas foi calculada alguma vez em 139.036 linhas: Rec decimal(1,0) = 0 em 131.336 e NULL em 7.700 (nenhum outro valor); Freq numeric(1,0) = idem; Vlr numeric(1,0) = idem; Score decimal(2,0) = 0/NULL apenas; Pto numeric(1,0) = NULL em 69.675 e 0 em 69.361; Recalc = 0/NULL; Classe varchar(30) = string vazia em 93.693 e NULL em 45.343 (zero valores reais); Situacao varchar(2) = idem; Perspectiva decimal(2,0) = NULL em 139.036 (100%); Ciclo decimal(3,0) = NULL em 139.034 de 139.036 (só 2 linhas com 30 e 60); DtaCalc datetime = NULL em 139.036 (100% — o job de cálculo NUNCA rodou); QtdTrans numeric(18,0) = nunca maior que zero em nenhuma linha, e DtaIniTrans/DtaUltTrans/ProcUltTrans acompanham.

O campo Potencial varchar(3) é o retrato do abandono: NULL em 128.866 linhas e, nas 5.855 preenchidas, o domínio mistura números (64, 43, 22, 85, 1) com letras (A, B, C, D, E, Z) na mesma coluna.

Portanto IVS_Pes NÃO é uma tabela de segmentação — é, de fato, apenas a tabela associativa pessoa↔departamento↔carteira. Todo o restante é peso morto que o time de dados enxerga no schema e supõe estar populado.

**Evidencia:** GROUP BY sobre Rec, Freq, Vlr, Pto, Score, Classe, Situacao, Perspectiva, Ciclo, Recalc, YEAR(DtaCalc) e CASE WHEN QtdTrans>0 em IVS_Pes (139.036 linhas)

**Licao para a Tracbel:** EVITAR carregar colunas analíticas na tabela transacional. Lição dupla: (1) não modele o que você não vai calcular — 10 colunas mortas destroem a confiança de qualquer analista no schema inteiro e são a causa provável de relatórios errados; (2) RFV/score é MATÉRIA DE PIPELINE ANALÍTICO, não de coluna OLTP. No CRM próprio: manter `carteira_pessoa` puramente relacional e materializar o RFV numa tabela/visão separada `pessoa_score` (pessoa_id, linha_negocio_id, recencia_dias, frequencia_12m, valor_12m, score, classe, calculado_em) recalculada por job, com `calculado_em` NOT NULL para que a ausência de cálculo seja visível. Se em 6 meses ninguém consumir, DELETAR a tabela em vez de deixá-la vazia.

### 8. A segmentação que REALMENTE funciona: IVS_DEPTOPOT (potencial + cadência por departamento)

Enquanto o RFV está morto, IVS_Pes.SEQPOTENCIALDP está preenchido em 109.118 das 139.036 linhas (78%) e aponta para IVS_DEPTOPOT (126 linhas), que é o modelo vivo: SEQPOTENCIALDP numeric(6,0) PK, SEQDEPTO numeric(4,0), POTENCIAL varchar(12), ORDEM numeric(2,0), DIASCICLOCTTO numeric(4,0) e DIASCICLOVISITA numeric(4,0).

O conceito é ótimo e específico do negócio: o potencial do cliente é definido POR DEPARTAMENTO (o mesmo cliente pode ser potencial A em máquinas e D em peças) e cada faixa de potencial define uma CADÊNCIA obrigatória — DIASCICLOCTTO (dias entre contatos) e DIASCICLOVISITA (dias entre visitas). Exemplos reais: depto 2 (MAQ-NOVOS) potencial A = 30/30 dias, B = 60/60; depto 3 (MAQ-PEÇAS) A = 360/360; depto 19 (PROSP-MAQ) A = 30/30, Médio = 45/45.

Defeito grave de domínio: a coluna POTENCIAL varchar(12) mistura DUAS escalas incompatíveis entre departamentos — letras (A/B/C/D/E, em 7 deptos cada) e palavras (Alto/Médio/Baixo/Muito baixo, em 18-19 deptos cada), mais 'PAN' em 4. Não há tabela de escala; a comparação entre departamentos é impossível sem um de-para manual. O uso real é dominado por letras: D=53.873 em VENDAS-DIGIT, B=13.348 e D=11.296 em MAQ-NOVOS, D=12.313 em DSI, C=5.003 em MAQ-NOVOS.

Segundo defeito: a cadência está modelada DUAS VEZES. IVS_Depto tem CicloA, CicloB, CicloC, CicloD, CicloN, CicloP, CICLOE e os equivalentes externos CicloAExt..CicloEExt (decimal(3,0)) — os mesmos dias-por-faixa que IVS_DEPTOPOT.DIASCICLOCTTO/DIASCICLOVISITA. Dois modelos concorrentes e nada garante que concordem.

Terceiro: 53.873 pessoas (quase toda a carteira VENDAS-DIGIT) estão classificadas como potencial 'D' — a classificação existe mas não discrimina.

**Evidencia:** SELECT * FROM IVS_DEPTOPOT (126 linhas); JOIN IVS_Pes×IVS_DEPTOPOT×IVS_Depto GROUP BY POTENCIAL, Depto; IVS_Pes WHERE SEQPOTENCIALDP IS NOT NULL = 109.118; schema/colunas.csv IVS_Depto colunas 9-18 e 23-24

**Licao para a Tracbel:** COPIAR o conceito inteiro — potencial POR linha de negócio com cadência de contato/visita embutida é exatamente a regra de negócio de revenda de máquinas, e é o que sustenta a agenda do vendedor. Modelo: `potencial_faixa (id, linha_negocio_id, codigo, rotulo, ordem, dias_ciclo_contato, dias_ciclo_visita)` + `carteira_pessoa.potencial_faixa_id` FK NOT NULL. ADAPTAR: (1) escala ÚNICA e ordenada em toda a empresa (A/B/C/D com `ordem` numérica), rótulo por linha de negócio se necessário, mas nunca duas escalas na mesma coluna; (2) ELIMINAR a duplicação — a cadência mora só em potencial_faixa, jamais também no departamento; (3) usar `ordem` para comparabilidade cross-departamento; (4) monitorar concentração — se 99% cai numa faixa, a régua está errada e o sistema deve alertar.

### 9. 🔴 Módulos inteiros entregues e nunca ativados: IVF_ (financiamento), IVM_, IVP_, GEL_ — 34 tabelas com ZERO linhas

Contagem ao vivo por sys.partitions: TODAS as 20 tabelas IVF_ têm 0 linhas — IVF_Financeira (23 col), IVF_Plano (19), IVF_Prazo (3), IVF_Proposta (60 col!), IVF_PropostaHst, IVF_PropostaObs, IVF_PropostaResult, IVF_Tabela (24), IVF_TabelaFiltro, IVF_TabEmpr, IVF_TabIndice, IVF_Acordo, IVF_Agregado, IVF_AgregCC, IVF_TipoAgregado, IVF_TpAgrPessoa, IVF_FinancEmpr, IVF_FinancImpTx, IVF_Molicar (tabela FIPE/Molicar de valor de usados), IVF_PlanoIndic. O módulo tem até FK para o processo comercial (IVF_Proposta.ProcessoFN → IV_FichaNegVeic.Processo, IV_FichaNegVeic.SeqFinanc → IVF_Financeira) e o catálogo de status da proposta existe em GE_ParamLista com Parametro='CRMFI_SITUACAO' e 12 estágios bem desenhados (Em simulação → Escolhida → Definida → Env. Financeira → Em análise fin. → Aprovada / Recusada / Rec.Definitiva / Descartada / Venda perdida → Utilizada → Recebida), criado em 02/01/2012 pelo usuário VTCCONS. Nunca uma linha.

Idem IVM_ (IVM_Material, IVM_MatPessoa, IVM_ProcMat, IVM_ProcMatItem = 0), IVP_ (IVP_TabPreco, IVP_Vendedor, IVP_PedCritica, IVP_ProdImagem = 0) e todo o stack de CEP/logística GEL_ (GEL_CEP, GEL_CepOrig, GEL_CepAlerta, GEL_Cidade, GEL_Conv, GEL_ConvDe = 0), que ainda por cima DUPLICA a geografia já existente em GE_Cidade (10.214 linhas) / GE_Bairro.

No módulo documental o mesmo padrão em escala menor: DMN_DocArq, DMN_DocVrs, DMN_DocObs, DMN_DocProj e DMN_DocProp têm 0 linhas, e IVS_Regional, IVS_CanalVenda, IVS_Negocio, IVS_Categoria, IVS_CartCategoria, IVS_NegCategoria, GE_PessoaDestino, GE_PESSOAREDESOCIAL, GE_PessoaUnidade, GE_PessoaAlt, GE_PessoaEndAlt e EXT_PESSOACONTATO também. Reflexo direto: IVS_Carteira.SeqRegional está NULO em 655/655 e SeqCanal em 655/655.

**Evidencia:** SELECT t.name, SUM(p.rows) FROM sys.tables t JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN (0,1) WHERE t.name LIKE 'IVF[_]%' OR 'IVP[_]%' OR 'IVM[_]%' OR 'GEL[_]%' OR 'DMN[_]%'; SELECT * FROM GE_ParamLista WHERE Parametro='CRMFI_SITUACAO'; contagens de IVS_Carteira WHERE SeqRegional/SeqCanal IS NOT NULL = 0

**Licao para a Tracbel:** EVITAR o modelo de produto-de-prateleira que entrega 767 tabelas das quais um terço nunca será usado — é a maior fonte de custo cognitivo do Vórtice e a razão de todo diagnóstico exigir 'confirmar se a tabela tem linhas'. No CRM próprio: só criar tabela quando existir tela e usuário; se um módulo for descontinuado, DROPAR (o histórico vai para arquivo), não deixar vazio. APROVEITAR uma coisa: o catálogo CRMFI_SITUACAO é um bom desenho de máquina de estados de proposta de financiamento (simulação → aprovação → utilização) e serve de ponto de partida se a Tracbel um dia modelar financiamento — mas construir com tabelas próprias, não ressuscitar IVF_Proposta com suas 60 colunas.

### 10. 🔴 Integridade referencial ausente onde mais importa — e a prova causal está nos dados

Contagem de FKs de saída por tabela (sys.foreign_keys) mostra ZERO constraints em: GE_Contato, GE_ContatoPapel, GE_PessoaFone, GE_PessoaFis, GE_PessoaClasse, GE_PessoaRelacao, GE_PessoaLink, GE_PessoaAlerta, GE_PessoaMural, GE_PessoaVersao, GE_Email, DMN_Doc e IVS_Carteira. Têm FK: GE_PessoaEmail(1), GE_PessoaJur(1), GE_PessoaNota(1), GE_PESSOAREDESOCIAL(1), GE_PessoaSimilar(1), GE_PessoaEnd(2), IVS_CartDepto(2), DMN_DocPes(3), IVS_CartCid(3), IV_ClientePropr(4), IVS_Pes(4).

A correlação é perfeita e mensurada. SEM FK → órfãos: GE_Contato 625 órfãos em 41.276; GE_PessoaFone 246 em 113.567; GE_PessoaEnd 138 em 6.480; GE_PessoaFis 58 em 5.305; GE_PessoaClasse 28 em 1.907; GE_PessoaRelacao 22 em 5.601; GE_PessoaFone com TipoFoneSeqPar (92/93/94) inexistente em GE_ParamLista = 2.027 linhas; DMN_Doc com SeqDocTp inválido = 82; DMN_DocPes apontando para DMN_Doc inexistente = 5.480 (a tabela tem 3 FKs para GE_Pessoa e NENHUMA para DMN_Doc). COM FK → zero órfãos: IVS_Pes 0 em 139.036; IV_ClientePropr 0 em 246.813.

Dois defeitos adicionais de desenho das FKs que existem: (a) redundância — várias FKs estão declaradas 2 ou 3 vezes sobre o mesmo par de colunas (GE_Pessoa.SeqRegiao→GE_Regiao 3×, GE_Pessoa.SeqRota→GE_Rota 3×, DMN_DocPes.SeqPessoa→GE_Pessoa 3×, IVS_Pes.SeqPessoa→GE_Pessoa 2×), o que só custa escrita; (b) chave composta partida — GE_Pessoa referencia GE_Bairro (PK composta SeqCidade+SeqBairro) por DUAS FKs de coluna única (SeqCidade→GE_Bairro.SeqCidade e SeqBairro→GE_Bairro.SeqBairro), o que em tese permitiria um par cidade/bairro inexistente. Na prática as 2.978 pessoas com ambos preenchidos estão consistentes, mas a garantia é acidental, não estrutural. (c) GE_CIdadePref.SeqCidade tem FK para GE_Cidade_CRM (729 linhas, tabela legada) e não para GE_Cidade (10.214). (d) GE_PessoaSimilar declara FK só em SeqPessoa1; SeqPessoa2 fica solto.

**Evidencia:** sys.foreign_keys agrupado por tabela; LEFT JOIN de cada satélite contra GE_Pessoa contando SeqPessoa IS NULL; LEFT JOIN DMN_DocPes×DMN_Doc = 5.480; LEFT JOIN GE_PessoaFone×GE_ParamLista(CRMPES_TIPOFONE) = 2.027; schema/fks.csv (FKs duplicadas e o par GE_Pessoa→GE_Bairro)

**Licao para a Tracbel:** EVITAR. A evidência é definitiva e vale como argumento de projeto: onde o Vórtice declarou FK há 0 órfãos, onde não declarou há órfãos em TODAS as tabelas. No CRM próprio: FK obrigatória em toda coluna de referência, sem exceção, com ON DELETE RESTRICT como padrão; chave composta referenciada por FK COMPOSTA; nenhuma FK duplicada; nenhuma referência a tabela de backup/legado; e um teste automatizado no CI que falha se alguma coluna terminada em `_id` não tiver FK. Bônus: os 5.480 vínculos documentais órfãos são o mesmo defeito já documentado em docs/REGRAS-DE-NEGOCIO.md (IV_ProcDocto sem DMN_Doc) — a causa raiz é única: exclusão de documento sem cascata porque não há FK.

### 11. Três modelos concorrentes de e-mail e três de telefone dentro do mesmo banco

E-MAIL: (1) GE_Pessoa.Email varchar(70) — coluna única desnormalizada, 35.959 preenchidas; (2) GE_PessoaEmail (SeqPessoa+SeqContato, Preferencial, Origem, Obs) — 0 linhas, modelo abandonado; (3) GE_Email — 39.363 linhas, o modelo real e melhor, com SeqEmail PK própria, IndPreferencial e consentimento por finalidade (IndUsoMkt/IndUsoProfissional/IndUsoPessoal/IndUsoFiscal/IndEmUso), MOTIVO e DTAEMAILATIVO; (4) ainda há GE_Contato.EMail varchar(50) e GE_ContatoEmail (0 linhas) e EXT_EMAIL (com FK para GE_Pessoa). Não existe regra declarada de qual vence; um envio de campanha precisa escolher entre 4 fontes.

TELEFONE: (1) GE_Pessoa.FoneDDD1/FoneNro1/FoneCmpl1 + 2 + 3 + FaxDDD/FaxNro — 6 pares desnormalizados, 61.546 pessoas com FoneNro1; (2) GE_PessoaFone — 113.567 linhas, modelo tabular correto com tipo, preferência, INDEMUSO/MOTIVOEMUSO, INDUSOMKT, INDWHATSAPP e DTAULTSUCESSO/DTAULTINSUCESSO (telemetria de discagem, ótimo conceito); (3) GE_Contato.FoneDDD1/FoneNro1/FoneDDD2/FoneNro2/FaxDDD/FaxNro + INDWHATSAPPF1/F2/FX. As views LGPD mascaram GE_Pessoa.FONENRO* e GE_PESSOAFONE.NUMERO separadamente, mas NÃO mascaram GE_Contato.FONENRO* fora da view de contato — cada modelo exige tratamento próprio.

Sintoma da fragmentação: INDWHATSAPP=1 em apenas 1 linha das 113.567 de GE_PessoaFone, embora WhatsApp seja o canal principal no agro e o banco tenha uma tabela IV_WHATSAPP. O flag existe no lugar certo e ninguém preenche porque o dado real está espalhado.

**Evidencia:** COUNT em GE_Email=39.363, GE_PessoaEmail=0, GE_Pessoa.Email preenchido=35.959, GE_Contato com e-mail=6.026; GE_PessoaFone=113.567 vs GE_Pessoa.FoneNro1 preenchido=61.546; GE_PessoaFone WHERE INDWHATSAPP=1 → 1 linha; definições das views GE$PESSOA_LGPD e GE$PESSOAFONE_LGPD

**Licao para a Tracbel:** EVITAR a coexistência. No CRM próprio: UMA tabela `contato_meio (id, pessoa_id, tipo enum[email,telefone,whatsapp,social], valor_normalizado, valor_exibicao, preferencial bool, verificado_em, ultimo_sucesso_em, ultimo_insucesso_em, ativo, motivo_inativacao)` — sem nenhuma coluna de e-mail ou telefone na tabela `pessoa`. COPIAR de GE_PessoaFone a telemetria DTAULTSUCESSO/DTAULTINSUCESSO (saber que um número não atende há 8 meses é ouro para o call center) e o par INDEMUSO/MOTIVOEMUSO. COPIAR de GE_Email o consentimento por finalidade, mas como evento (ver achado de LGPD). Migração: consolidar as 4 fontes com prioridade explícita e registrar a origem em `contato_meio.origem`.

### 12. Campos de texto livre onde já existe catálogo — e lixo de teste em produção

O banco TEM catálogos prontos em GE_ParamLista: CRMPES_ATIVIDADEPF (11 itens: Aeroporto, Agronegócio, Construtora, Financeira, Indústria, Outros Negócios, Outros Órgão Públicos, Pecuária, Prefeitura, Rodovia, Usina), CRMPES_ATIVIDADEPJ (os mesmos 11) e CRMPES_FAIXAFATURA (12 itens). Mas GE_Pessoa grava tudo como varchar sem FK, e o resultado é divergência:

• Atividade varchar(30): 64 valores distintos contra 11 no catálogo (preenchido em só 7.650 de 118.463 = 6%). Reais: Agronegócio 4.362, Comprador 539, Produtor(a) 521, Agropecuária 269, Fazendeiro(a) 248, Citricultor(a) 202 — vários fora do catálogo.
• RendaFaturamento varchar(30): 33 distintos contra 12 no catálogo. Contém faixas OBSOLETAS que não existem mais no catálogo ('DE R$ 5,1 MM A R$ 10,0 MM' 589, 'DE R$ 10,1 MM A R$ 30,0 MM' 441, 'DE R$ 50,1 MM A R$ 100,0 MM' 95) — a régua foi alterada e os registros históricos ficaram órfãos de significado. Mistura ainda três escalas: faixas em R$, adjetivos (Médio 458, Alto 280, Baixo 119) e siglas (CAP 123, SAM 123, CON 119). E 24.530 linhas contêm literalmente o caractere '.'.
• Porte varchar(30): 3 valores distintos, dos quais 25.322 de 25.323 preenchidos são o caractere '.' — o campo é 100% lixo — e 1 linha contém a string literal 'string'.
• Grupo varchar(30): 19 distintos, 115.719 preenchidos (97,7%, o mais usado). Varejo 85.483, Cliente 24.351, Diversos 3.233, Contas Chave 814, CLIENTES 766 (duplicata de 'Cliente'), Funcionário 461, Concessionária 345, Fornecedor 166, Ex-Funcionário 37, PROSPECT 33, Concorrente 16 — mistura tipo de relacionamento comercial, papel na cadeia e status, e contém 'Teste' 1 e 'string' 1.
• EstadoCivil varchar(20): 11 distintos com apenas 1.086 preenchidos — C 695, IN 276, S 74, V 20, D 14, 'O ??' 3, 'IN ??' 1, 'string ?? ??' 1 (corrupção de acento + dado de teste), Q 1, O 1 — enquanto a view IV$S_PESSOA faz CASE sobre char(1) C/S/D/Q/V.
• Sexo char(1): 'J' em 41.531 linhas (o valor de FisicaJuridica vazando para a coluna de sexo), M 39.341, vazio/NULL 37.598, F apenas 818, O 10.

A string literal 'string' aparece em Porte, Grupo e EstadoCivil — assinatura clássica de chamada de API testada com o payload de exemplo do Swagger direto contra produção.

**Evidencia:** SELECT Parametro, ListaStr FROM GE_ParamLista WHERE Parametro IN ('CRMPES_FAIXAFATURA','CRMPES_ATIVIDADEPF','CRMPES_ATIVIDADEPJ'); COUNT(DISTINCT ...) e GROUP BY por Atividade, Porte, Grupo, RendaFaturamento, EstadoCivil, Sexo em GE_Pessoa; view IV$S_PESSOA (CASE ESTADOCIVIL WHEN 'C'...)

**Licao para a Tracbel:** EVITAR. Toda dimensão vira tabela com FK — `atividade`, `faixa_faturamento`, `porte`, `segmento_relacionamento`, `estado_civil` — e a coluna na pessoa é `*_id` NOT NULL/NULL explícito, nunca varchar. Quando a régua mudar (como aconteceu com as faixas de faturamento), a FK preserva o significado histórico e permite versionar o catálogo (`vigencia_de/ate`) em vez de deixar 1.125 registros apontando para faixas que não existem mais. EVITAR também o antipadrão GE_ParamLista (catálogo genérico único com slots Str1..Str5/Nro1..Nro6/Ind1..Ind4 discriminados por uma coluna `Parametro`) — dá a ilusão de catálogo sem entregar integridade: é justamente por ser genérico que 2.027 telefones apontam para tipos 92/93/94 inexistentes. E: validação de entrada na API + ambiente de homologação separado, para que 'string' nunca chegue à produção.

### 13. Deduplicação: motor bom, fila de 124 mil pares abandonada desde 2018

GE_PessoaSimilar implementa um fluxo de match-and-merge bem pensado: SeqPessoa1/SeqPessoa2, Probabilidade decimal(3,0), DtaGeracao, ObsGeracao, Revisado + DtaRevisao, Duplicado, Parecer varchar(30) + UsuParecer, SeqPessoaFica (qual registro sobrevive) + ObsFica, e até um lock de curadoria com UsuEmTrabalho/DtaEmTrabalho. A detecção usa busca fonética própria em GE_PessoaFonema (455.993 partículas) e GE_PessoaNomeFonema (151.583).

O estado real: 124.222 pares candidatos, 124.213 marcados Duplicado=1, SeqPessoaFica sugerido em 123.693 — mas apenas 9 pares foram revisados (Revisado=1) e a última revisão foi em 27/02/2018. A última geração de candidatos foi em 12/09/2025, ou seja, o motor continua produzindo e ninguém consome. É uma fila com mais de 8 anos de backlog.

Contexto que explica o volume: 24.345 pessoas (20,5%) não têm CPF/CNPJ (NroCGCCPF nulo ou zero) — sem documento, a única chave é o nome, daí o volume fonético. Entre as que têm documento, a duplicação real é pequena: apenas 132 grupos de (NroCGCCPF, DigCGCCPF) repetido, envolvendo 264 pessoas. Não existe UNIQUE sobre o documento.

O rastro de fusões que já ocorreram existe em GE_PessoaLink com Origem='PES_JUNCAO' (117 linhas) — ou seja, quando um merge acontece o ID antigo é preservado como alias, o que é correto.

**Evidencia:** COUNT em GE_PessoaSimilar total=124.222, Duplicado=1→124.213, Revisado=1→9, MAX(DtaRevisao)=27/02/2018, MAX(DtaGeracao)=12/09/2025, SeqPessoaFica preenchido=123.693; GE_Pessoa WHERE NroCGCCPF IS NULL OR =0 → 24.345; GROUP BY NroCGCCPF,DigCGCCPF HAVING COUNT(*)>1 → 132 grupos/264 pessoas; GE_PessoaLink WHERE Origem='PES_JUNCAO' → 117

**Licao para a Tracbel:** COPIAR o desenho (score de similaridade + par candidato + registro sobrevivente + parecer humano + alias pós-merge) — é o modelo certo. EVITAR o modo de falha: fila sem dono e sem SLA vira dívida invisível. No CRM próprio: (1) UNIQUE em `documento` quando presente, resolvendo por construção a duplicata exata; (2) bloqueio no cadastro — não deixar salvar pessoa sem CPF/CNPJ salvo exceção justificada, já que 20,5% sem documento é a origem do problema; (3) auto-merge acima de um limiar de confiança e fila humana só na faixa cinzenta, com meta de zerar semanalmente e alerta quando o backlog crescer; (4) manter SEMPRE o alias do id antigo (o padrão PES_JUNCAO está certo) para que integrações externas não quebrem.

### 14. IV_ClientePropr / IV_Propriedade: o construtor de entidades customizadas (parque de máquinas e perfil da fazenda)

É o análogo de 'GE_PessoaProp' e um dos módulos mais usados: IV_ClientePropr tem 246.813 linhas para 55.544 pessoas (≈4,4 registros por cliente), 241.114 com Ativo='S', ainda ativo (última inclusão 26/08/2026), com FK para GE_Pessoa e 0 órfãos.

O desenho é metadata-driven: IV_Propriedade (52 tipos, 70 colunas de metadados) define, para cada SeqPropriedade, o RÓTULO de cada slot genérico — Campo1..Campo6 varchar(20) (mais Campo1Sql..Campo6Sql numeric(1) indicando se o valor vem de consulta SQL), Numero1..Numero6, Data1..Data6, além de Nivel, NivelIdentificador, CampoListar, UmPorPessoa, MostraRef, Usacomplemento, TipoRefVinculado, ReferenciaSQL. As listas de valores permitidos ficam em IV_PropriLista (7.375 linhas, chaveada por SeqPropriedade+Campo).

IV_ClientePropr guarda as instâncias em slots de largura fixa: Campo1..Campo8 varchar(40), Numero1..Numero6 decimal(15,2), Data1..Data6 datetime, SimNao1..SimNao6 numeric(1), Literal1..Literal10 varchar(40), mais Referencia/Identificador varchar(30), Notas varchar(250), Ativo char(1), CodOrigem/UltOrigem e auditoria. Texto longo vai para IV_ClientePropCmpl (SeqPropPessoa + Complemento text) — 0 linhas.

Tipos reais cadastrados: Trator, Colheitadeira Grãos, Plataforma Adicional, Plantadeira, Implemento, Agricultura Precisão, Feno e Forragem, Turf, Pulverizador, Colhedora de Cana, Distribuidora, Equipamentos Manitou, Equip.Antigo, Equipamento(Sisdia), Locação, Seguro, Contrato GFC, Frota, Operations Center, Origem da Receita. Os rótulos dos slots variam por tipo: para Trator, Campo1='Família', Campo2='Modelo', Campo3='Faixa de Potência'; para Seguro, Campo1='Seguradora', Campo2='Corretora', Campo3='Condição Pagamento'.

Problemas: (1) tipagem inexistente — 'Faixa de Potência' e 'Seguradora' são ambos varchar(40) no mesmo Campo3; (2) limite rígido de slots (6 campos, 6 números, 6 datas, 6 booleanos, 10 literais) — passou disso, não cabe; (3) impossível indexar ou consultar por atributo de negócio sem saber o mapeamento; (4) alguns rótulos vêm com asterisco ('Tipo*', 'Modelo*') indicando obrigatoriedade codificada no texto do rótulo; (5) DMN_DocProp (documentos anexados à propriedade) tem 0 linhas apesar da FK existir.

**Evidencia:** COUNT IV_ClientePropr=246.813 / 55.544 pessoas / Ativo='S'=241.114 / MAX(DtaInclusao)=26/08/2026; SELECT TOP 20 SeqPropriedade, Propriedade, CampoListar, Campo1..3, Numero1, Data1 FROM IV_Propriedade; schema/colunas.csv IV_ClientePropr (51 col) e IV_Propriedade (70 col)

**Licao para a Tracbel:** COPIAR a INTENÇÃO — a Tracbel precisa mesmo registrar o parque do cliente (tratores, colheitadeiras, pulverizadores, seguros, contratos) com atributos que mudam por tipo, e 246 mil registros provam que o vendedor usa. EVITAR os slots de largura fixa. Modelo próprio recomendado: entidade forte `equipamento_cliente` para o que é catalogável (marca/família/modelo/série/ano com FK ao catálogo de produtos e integração JDE/EXT_Veic) + `atributo_definicao (tipo_entidade, chave, rotulo, tipo_dado, obrigatorio, ordem, lista_valores_id)` e `atributo_valor (entidade_id, definicao_id, valor_texto/valor_num/valor_data/valor_bool)` com índice por (definicao_id, valor_*) — EAV tipado e sem teto, em vez de 34 colunas genéricas. Obrigatoriedade em coluna booleana, jamais em asterisco no rótulo.

### 15. Gestão documental DMN: política de tipo excelente, execução vazia; arquivo mora no FTP por caminho relativo

DMN_DocTp (107 tipos) é o melhor conceito do módulo — um motor declarativo de política documental com 24 colunas: exigências (ExigArq, ExigAutent, ExigDtaBase, ExigValidade), a QUE se vincula (VincPessoa, VincProcesso, VincPropriedade, VincProjeto, VINCOS), cardinalidade (UmPorPessoa, UmPorProcesso, UmPorPropriedade, UmPorProjeto, UMPOROS), além de TamanhoMax decimal(8,0), EXTENSOES varchar(80), Formato varchar(3), QtdDiaValidade e QtdDiaProcEncer (dias para encerrar o processo).

Uso real (71.400 documentos): DOC GARANTIA 14.869, NF 10.947, OUTROS 10.114, PEDIDO JD QUOTE 4.200, NF MAQ/IMPLEMENTO 3.841, PLANILHA_PREÇO 3.532, CHECK LIST_PRÉ_ENTREGA 1.995, NF ASSINADA 1.724, AUTOR FATURA 1.479, PEDIDO VENDA 1.266. Quase todos com VincProcesso=1 — o documento é anexo do processo comercial, não do cliente (embora DMN_DocPes, o vínculo com pessoa, tenha 76.520 linhas).

O que está morto: (a) Status char(1) = 'N' em 100% das 71.400 linhas — não há workflow de aprovação/rejeição de documento; (b) versionamento — DMN_Doc.SeqVrs existe e vale 0 nos documentos recentes, e DMN_DocVrs (a tabela de versões) tem 0 linhas; (c) DMN_DocArq (localização física: Bloco/Sala/Endereço, para documento em papel) = 0; (d) DMN_DocObs = 0; (e) Validade preenchida em só 2.180 de 71.400, apesar de ExigValidade existir — o controle de vencimento documental (crítico para garantia e financiamento) não opera; (f) EXTENSOES quase sempre vazia, então TamanhoMax/EXTENSOES não restringem nada.

Armazenamento: DMN_Doc.Arq varchar(250) guarda um CAMINHO RELATIVO com barra invertida do Windows, particionado pelos dígitos do SeqPessoa — ex. '000000\12\30\123090\123090_77022_000033261.pdf'. O arquivo em si vive no servidor de arquivos/FTP (o 'Doc Manager'), fora do banco. Não há hash, tamanho, mime-type (só Ext varchar(5)) nem qualquer verificação de que o arquivo existe.

Integridade: DMN_Doc tem ZERO FKs (82 documentos com SeqDocTp inválido) e DMN_DocPes tem 3 FKs para GE_Pessoa mas nenhuma para DMN_Doc — daí 5.480 vínculos apontando para documentos inexistentes, o mesmo defeito já registrado em docs/REGRAS-DE-NEGOCIO.md pelo lado de IV_ProcDocto. DMN_DocHst (189.393 eventos, com TipoMov/Descr/Usuario) sobrevive à exclusão do documento e é o único rastro do que foi apagado.

**Evidencia:** GROUP BY Status em DMN_Doc → 'N' 71.400; contagem DMN_DocVrs/DocArq/DocObs/DocProj/DocProp = 0; SELECT TOP 5 SeqDocto, Arq, Ext, SeqVrs, Status FROM DMN_Doc; JOIN DMN_DocTp×DMN_Doc por tipo; LEFT JOIN DMN_Doc×DMN_DocTp = 82 inválidos; LEFT JOIN DMN_DocPes×DMN_Doc = 5.480 órfãos; DMN_Doc WHERE Validade IS NOT NULL = 2.180

**Licao para a Tracbel:** COPIAR DMN_DocTp quase literalmente — política de documento declarativa (o que exige, a que se vincula, quantos são permitidos, extensões, tamanho, validade em dias) é um desenho maduro que a Tracbel deve preservar, inclusive porque encaixa com garantia e pré-entrega. ADAPTAR o armazenamento: object storage (S3/Azure Blob) com `arquivo (id, storage_key, sha256, bytes, mime_type, nome_original, enviado_por, enviado_em)` — hash obrigatório para detectar corrupção e deduplicar, mime real em vez de extensão, e chave opaca em vez de caminho Windows (o caminho relativo com backslash é o que amarra o sistema ao FTP e já causou incidentes de migração de IP). EVITAR: coluna de status que nunca muda (se não há workflow, não crie a coluna; se há, modele as transições) e versionamento fantasma (DMN_DocVrs vazia). E FK obrigatória do vínculo para o documento com ON DELETE CASCADE — os 5.480 órfãos e os 3.259 SeqDocto apagados desaparecem por construção.

### 16. Padrão outbox de integração (OUT_*) — conceito certo, implementação quebrada

O Vórtice implementa um outbox para publicar mudanças de cadastro a sistemas externos: OUT_Log é a fila (SeqLog, Tabela varchar(20), NroEmpresa, Chave1N/Chave2N/Chave1C, TpOper char(1) I/A/E, DtaIns, UsrIns, ObsIns, OutOk numeric(1), OutDta) e OUT_Pessoa (61 colunas), OUT_Contato (25), OUT_PessoaRelacao (7) e OUT_PessoaLink (4) são os snapshots do payload a publicar.

O defeito: OutOk = 0 em 100% das 1.765 linhas de OUT_Log — nenhum evento jamais foi confirmado como entregue, e OutDta nunca é preenchida. A fila só cresce. Detalhamento por fluxo: OUT_PESSOA tipo 'A' (alteração) 915 eventos, parados entre 12/08/2014 e 19/01/2017 — fluxo morto há 9 anos; OUT_PESSOA tipo 'I' (inclusão) 187, ainda produzindo até 29/08/2026; OUT_PROCESSO tipo 'E' 499 eventos, ativo até 25/08/2026; OUT_CONTATO 'I' 147 e 'A' 9, parados em 2016/2017; OUT_PESSOARELACAO 5+3, parados em 2016.

OU o consumidor nunca atualizou o flag (e então não há como saber o que foi entregue e o reprocessamento é impossível de determinar), OU o consumidor morreu e ninguém percebeu — em ambos os casos não existe observabilidade. Não há coluna de tentativas, erro, ou dead-letter.

O contraponto positivo do mesmo domínio é GE_PessoaLink, que resolve a IDENTIDADE entre sistemas: Pessoalink varchar(250) (o ID externo) + Origem varchar(20) + SeqPessoa. Origens reais e seu comportamento: INTEGRAÇÃONOROESTE 28.037 (carga concentrada em 3 dias, 27–30/12/2024 — migração de base), Protheus 4.063 (2015–2024), RD 883 e ATIVA (22/01/2026 a 30/08/2026 — RD Station), EEMOVEL 220, PES_JUNCAO 117 (alias de cadastros fundidos). Isso permite reprocessar e reconciliar sem depender de nome.

**Evidencia:** SELECT Tabela, TpOper, OutOk, COUNT(*), MIN(DtaIns), MAX(DtaIns) FROM OUT_Log GROUP BY ... → OutOk=0 em todos os grupos; SELECT Origem, COUNT(*), MIN/MAX(DtaGeracao) FROM GE_PessoaLink GROUP BY Origem; contagens OUT_Pessoa=1.918, OUT_Contato=147

**Licao para a Tracbel:** COPIAR ambos os conceitos — outbox transacional e tabela de identidade externa são exatamente o que a Tracbel precisa para conviver com TOTVS, JDE e RD Station. ADAPTAR o outbox para ser operável: `evento_saida (id, agregado, agregado_id, tipo_operacao, payload jsonb, criado_em, status enum[pendente,enviado,falha,descartado], tentativas int, ultimo_erro text, enviado_em, destino)` + índice parcial em status='pendente' + alerta automático quando a fila mais antiga passar de X minutos. A lição concreta do OutOk=0: um flag de confirmação que ninguém escreve é pior que nenhum flag, porque cria falsa sensação de rastreabilidade — o monitoramento tem de ser externo à tabela. COPIAR GE_PessoaLink como `identidade_externa (pessoa_id, sistema, id_externo, criado_em)` com UNIQUE (sistema, id_externo), incluindo o uso como ALIAS pós-fusão (padrão PES_JUNCAO).

### 17. Histórico e ciclo de vida do cadastro: versionamento com justificativa (bom) e trilha de status inexistente (ruim)

GE_PessoaVersao (12.021 linhas / 6.473 pessoas) guarda o histórico das alterações CADASTRAIS sensíveis: NomeRazao, Cidade, Uf, Bairro, EnderecoCompleto, Cep, Pais, NroCGCCPF+DigCGCCPF, InscricaoRG, InscMunic, InscProdutor, CNAE, mais DtaAtualizacao, UsuAlterou, TipoAlteracao char(1) e Justificativa varchar(150). Exigir JUSTIFICATIVA para mudar razão social ou CNPJ é um controle excelente, raro em CRM.

Mas: (a) cobertura de 6.473 pessoas em 118.463 (5,5%) e GE_Pessoa.Versao > 0 em apenas 6.237; (b) TipoAlteracao é um char(1) sem catálogo — em branco 7.350 (o maior grupo, congelado em 30/05/2017), 'D' 2.938 (até 01/07/2024), 'C' 927 (até 07/07/2026), 'M' 788 (até 15/06/2026), 'S' 18. O significado das letras não está em lugar nenhum; (c) só cobre identidade e endereço — mudança de Status, de carteira, de consentimento ou de vendedor não é versionada em lugar algum.

O buraco maior é o ciclo de vida: DtaAtivacao e DtaInativacao estão NULAS em 100% das 118.463 linhas, e UsuInativacao/ObsInativacao acompanham. Não existe resposta para 'quando este prospect virou cliente?' nem 'quando e por que este cadastro foi inativado?'.

SEQPESSOAPRC numeric(10,0) NOT NULL é o ponteiro de grupo econômico / titular principal: difere de SeqPessoa em 12.045 linhas, nunca é 0 e nunca aponta para pessoa inexistente (0 dangling) — mas NÃO tem FK declarada, nem CHECK impedindo ciclo, nem coluna de tipo de vínculo. Exemplos reais: SeqPessoa 28 'PEDRO REDEMPTOR GUIDI' → principal 91452 'ANGELO DE PAULA GUIDI E OUTRO'; SeqPessoa 48 'SUELY APARECIDA RAVAZZI E OUTROS' → 89379 'KMAIS IMPORTACAO E EXPORTACAO LTDA'. O status do principal pode divergir do da filial (P vs A), sem regra.

O relacionamento entre pessoas tem uma segunda via, GE_PessoaRelacao (5.601 linhas), com TipoRelacionamento varchar(30) codificando a DIREÇÃO dentro do texto ('Vinculado/Principal' 4.896, 'Principal/ Relacionado Negócio' 198, 'Relacionado Negócio/Principal' 194 e — separadamente — ' Relacionado Negócio/Principal' com espaço à esquerda 128), além de laços familiares (Pais 34, Filhos 33, Irmãos 29, Sócios 22, Conjuge 13). Duas modelagens concorrentes de grupo econômico, uma sem FK e outra com direção em texto livre e valores duplicados por espaço em branco.

Abandono correlato: GE_PessoaMural (145.313 avisos na ficha do cliente) está congelada desde 19/12/2019; GE_PessoaAlerta tem 237 linhas; GE_PessoaNota 1.231; GE_PessoaDestino e GE_PESSOAREDESOCIAL têm 0.

**Evidencia:** GROUP BY TipoAlteracao em GE_PessoaVersao com MAX(DtaAtualizacao); GE_Pessoa WHERE DtaAtivacao/DtaInativacao IS NOT NULL = 0; COUNT WHERE SEQPESSOAPRC <> SeqPessoa = 12.045 e LEFT JOIN self = 0 dangling; GROUP BY TipoRelacionamento em GE_PessoaRelacao; MAX(DtaAlteracao) GE_PessoaMural = 19/12/2019

**Licao para a Tracbel:** COPIAR a exigência de justificativa em alteração de dado sensível (razão social, CNPJ, inscrição) — vira `pessoa_alteracao (pessoa_id, campo, valor_anterior, valor_novo, motivo_id, justificativa, usuario_id, em)` cobrindo TODOS os campos críticos, não só 5,5% da base. EVITAR char(1) sem catálogo: motivo vira FK. ADAPTAR o grupo econômico: uma única modelagem `pessoa_relacao (pessoa_origem_id, pessoa_destino_id, tipo_id FK, papel enum, desde, ate)` com direção nas COLUNAS e tipo em catálogo — nunca 'A/B' embutido em varchar (os 128 registros perdidos por um espaço à esquerda são o custo exato desse atalho). Manter o conceito de matriz/filial, mas com FK, CHECK anti-ciclo e regra de propagação de status. E instrumentar features: se um mural com 145 mil registros ficou 6 anos parado, o sistema deveria ter sinalizado.

## Lacunas declaradas

- Não apurei se a aplicação (CRM desktop Gupta e wsVorticeCrmApi) realmente LÊ pelas views GE$*_LGPD ou acessa GE_Pessoa/GE_Contato diretamente. Se acessa direto, as views de LGPD são apenas decorativas e nunca estiveram no caminho de execução — o que muda a gravidade do achado de 'controle contornável' para 'controle inexistente'. Exigiria inspecionar o binário/telas ou capturar o SQL emitido pelo cliente.
- Não apurei o significado do domínio de GE_PessoaVersao.TipoAlteracao (D=2.938, C=927, M=788, S=18, em branco=7.350). Não há catálogo em GE_ParamLista nem CHECK constraint. Suposições plausíveis (D=Dados, C=Cadastral, M=Manual, S=Sistema) não foram confirmadas por nenhuma fonte.
- Não apurei o significado de DMN_Doc.Status='N' (valor único em 71.400 linhas). Sem outro valor em produção não é possível inferir a máquina de estados pretendida pelo fornecedor.
- Não apurei o caminho-base físico onde os arquivos de DMN_Doc.Arq residem (o prefixo '000000\12\30\...' é relativo). A raiz fica em parâmetro do Doc Manager/FTP, fora das tabelas consultadas — não localizei a chave de configuração correspondente em GE_ParametroGlobal/GE_ParamLista.
- Não apurei se as tabelas IVF_* já tiveram dados em algum momento e foram truncadas, ou se nunca receberam carga. O catálogo CRMFI_SITUACAO datado de 02/01/2012 sugere implantação planejada e abortada, mas não há evidência direta (logs de DDL/truncate não foram consultados).
- Não apurei a regra que popula e mantém GE_Pessoa.SEQPESSOAPRC (grupo econômico). Não há trigger visível nem procedure identificada; pode vir do ERP via integração ou de tela específica do CRM desktop. Também não apurei se existe validação anti-ciclo na aplicação.
- Não apurei o significado de IVS_Pes.Situacao varchar(2) e IVS_Pes.Status char(1) porque ambos estão vazios/nulos em praticamente toda a base (Situacao: '' 93.693 + NULL 45.343; Status: '' 93.693 + NULL 35.276 + 'A' 10.067). Não há view do fornecedor que decodifique esses dois campos, ao contrário de GE_Pessoa.Status.
- Não apurei o conteúdo dos catálogos GE_PessoaClasse além da distribuição (FILIAL 873, FUNCIONÁRIOS 612, PARCEIROS 236, FALECIDO 71, REDE CONCESSIONÁRIOS 69, SAM 46) — as colunas ORIGEM e IDENTORIGEM estão vazias na amostra, então não sei que sistema aplica essas classes.
- Não aprofundei o algoritmo de busca fonética (GE_PessoaFonema 455.993 partículas / GE_PessoaNomeFonema 151.583) que alimenta GE_PessoaSimilar — não sei se é Soundex, Metaphone adaptado ao português ou algoritmo proprietário, o que importaria para decidir se vale replicar ou trocar por trigram/pg_trgm no CRM próprio.
- Não apurei a relação exata entre IVS_Carteira.SeqVendedor e IV_VENDEDOR/IV_VENDEDOREMPR (o 'CEN' descrito na memória do projeto) — confirmei apenas que SeqVendedor está preenchido em 141 das 655 carteiras e que SeqUsrResp está em 655/655, mas não validei a cadeia de hierarquia de aprovação neste levantamento.
- Não medi o impacto de GE_Pessoa.Sexo='J' (41.531 linhas): não confirmei se corresponde exatamente às pessoas com FisicaJuridica='J' (75.927) ou se é um subconjunto vindo de uma origem específica de integração. A divergência entre os dois números fica sem explicação.
