# Dicionario de Dados - Vortice CRM (banco `CRM`)

> **Snapshot de 03/06/2026** - todas as contagens de linha vem dos arquivos `schema/*.csv` do repo `vortice-crm-agent`, extraidos do SQL Server nessa data. O banco **nao** foi consultado ao vivo na geracao deste documento.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

## 1. Numeros gerais

| Item | Valor |
|---|---:|
| Tabelas | 767 |
| Colunas de tabelas | 10.741 |
| Views | 411 |
| Colunas de views | 9.118 |
| **Total de colunas no catalogo** | **19.859** |
| Tabelas com PK declarada | 663 |
| Chaves estrangeiras declaradas | 672 |
| Linhas somadas (snapshot) | 85.455.292 |
| Arquivos de modulo gerados | 28 |

> As **19.859 linhas** de `colunas.csv` cobrem **1.178 objetos**: as 767 tabelas (10.741 colunas) **e** as 411 views (9.118 colunas). Este dicionario documenta as tabelas nos arquivos de modulo; as colunas das views estao em [VIEWS.md](VIEWS.md). O que continua faltando das views e o **SQL** de cada uma (ver [LACUNAS.md](LACUNAS.md)).

Negocio: CRM + BPM da Tracbel (revenda de maquinas e equipamentos, divisao Agro), integrado aos ERPs TOTVS e JD Edwards. Fluxo central: **pessoa -> processo -> agenda -> historico -> resultado -> proxima agenda**.

## 2. Resumo por modulo

Modulo = prefixo do nome da tabela ate o primeiro `_` (tabelas sem `_` vao para `OUTROS`). O prefixo `IV_` tem 377 tabelas e foi dividido em 4 arquivos por assunto.

| Modulo | Arquivo | Tabelas | Colunas | Linhas (snapshot) | % das linhas |
|---|---|---:|---:|---:|---:|
| `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | 195 | 3.072 | 602.428 | 0.70% |
| `GE` | [GE.md](GE.md) | 160 | 2.263 | 34.500.052 | 40.37% |
| `IV-4` | [IV-4-demais.md](IV-4-demais.md) | 89 | 1.151 | 1.493.791 | 1.75% |
| `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | 56 | 657 | 2.129.692 | 2.49% |
| `EXT` | [EXT.md](EXT.md) | 46 | 846 | 15.504.403 | 18.14% |
| `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | 37 | 393 | 19.567.539 | 22.90% |
| `GEP` | [GEP.md](GEP.md) | 36 | 352 | 5.985.987 | 7.00% |
| `IVS` | [IVS.md](IVS.md) | 27 | 255 | 335.407 | 0.39% |
| `IMP` | [IMP.md](IMP.md) | 23 | 717 | 1.620.093 | 1.90% |
| `IVF` | [IVF.md](IVF.md) | 20 | 240 | 0 | 0.00% |
| `OUTROS` | [OUTROS.md](OUTROS.md) | 16 | 96 | 98.881 | 0.12% |
| `DMN` | [DMN.md](DMN.md) | 9 | 73 | 325.959 | 0.38% |
| `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | 8 | 185 | 3.044.200 | 3.56% |
| `JDE` | [JDE.md](JDE.md) | 7 | 66 | 0 | 0.00% |
| `GEL` | [GEL.md](GEL.md) | 6 | 46 | 0 | 0.00% |
| `IVC` | [IVC.md](IVC.md) | 6 | 57 | 0 | 0.00% |
| `OUT` | [OUT.md](OUT.md) | 5 | 109 | 3.746 | 0.00% |
| `IVM` | [IVM.md](IVM.md) | 4 | 49 | 0 | 0.00% |
| `IVP` | [IVP.md](IVP.md) | 4 | 31 | 0 | 0.00% |
| `VBI` | [VBI.md](VBI.md) | 3 | 38 | 2.251 | 0.00% |
| `MIG` | [MIG.md](MIG.md) | 2 | 4 | 28.089 | 0.03% |
| `TESTE` | [TESTE.md](TESTE.md) | 2 | 2 | 471 | 0.00% |
| `CBR` | [CBR.md](CBR.md) | 1 | 7 | 0 | 0.00% |
| `CLARITY` | [CLARITY.md](CLARITY.md) | 1 | 5 | 0 | 0.00% |
| `ESPACO` | [ESPACO.md](ESPACO.md) | 1 | 6 | 735 | 0.00% |
| `IVT` | [IVT.md](IVT.md) | 1 | 8 | 12 | 0.00% |
| `J1` | [J1.md](J1.md) | 1 | 9 | 211.556 | 0.25% |
| `LOG` | [LOG.md](LOG.md) | 1 | 4 | 0 | 0.00% |
| **TOTAL** | | **767** | **10.741** | **85.455.292** | **100%** |

> Nao existe modulo `BI_` em **tabelas** - `BI_*` sao **views** (ver [LACUNAS.md](LACUNAS.md) secao 2).

## 3. As 30 maiores tabelas por numero de linhas

| # | Tabela | Modulo | Classe | Colunas | Linhas | % do total |
|---:|---|---|---|---:|---:|---:|
| 1 | [`GE_LOG_PROCESSO`](GE.md#ge_log_processo) | `GE` | isolada | 10 | 12.678.640 | 14.84% |
| 2 | [`GE_LgTb`](GE.md#ge_lgtb) | `GE` | isolada | 10 | 11.861.777 | 13.88% |
| 3 | [`IV_AgendaLog`](IV-1-processo-agenda-historico.md#iv_agendalog) | `IV-1` | isolada | 9 | 11.049.475 | 12.93% |
| 4 | [`EXT_VeicModelo`](EXT.md#ext_veicmodelo) | `EXT` | nucleo | 5 | 4.431.168 | 5.19% |
| 5 | [`GE_Log2`](GE.md#ge_log2) | `GE` | isolada | 14 | 4.394.779 | 5.14% |
| 6 | [`GEP_SyncUsrSat`](GEP.md#gep_syncusrsat) | `GEP` | isolada | 15 | 3.901.359 | 4.57% |
| 7 | [`EXT_VeicPlanoMan`](EXT.md#ext_veicplanoman) | `EXT` | nucleo | 4 | 3.571.257 | 4.18% |
| 8 | [`EXT_VeicModPlano`](EXT.md#ext_veicmodplano) | `EXT` | nucleo | 2 | 3.571.235 | 4.18% |
| 9 | [`IV_Historico`](IV-1-processo-agenda-historico.md#iv_historico) | `IV-1` | nucleo | 29 | 2.436.127 | 2.85% |
| 10 | [`IV_ProcDado`](IV-1-processo-agenda-historico.md#iv_procdado) | `IV-1` | nucleo | 24 | 1.516.214 | 1.77% |
| 11 | [`EXT_NFSItem`](EXT.md#ext_nfsitem) | `EXT` | nucleo | 27 | 1.300.126 | 1.52% |
| 12 | [`IV_Processo`](IV-1-processo-agenda-historico.md#iv_processo) | `IV-1` | nucleo | 22 | 1.174.932 | 1.37% |
| 13 | [`EXT_TituloMov`](EXT.md#ext_titulomov) | `EXT` | nucleo | 9 | 1.134.039 | 1.33% |
| 14 | [`GE_LOG_HISTORICO`](GE.md#ge_log_historico) | `GE` | isolada | 10 | 1.015.045 | 1.19% |
| 15 | [`GEP_JobAgdExecLog`](GEP.md#gep_jobagdexeclog) | `GEP` | isolada | 13 | 981.319 | 1.15% |
| 16 | [`GE_LOG_PESSOA`](GE.md#ge_log_pessoa) | `GE` | isolada | 10 | 943.712 | 1.10% |
| 17 | [`IV_Agenda`](IV-1-processo-agenda-historico.md#iv_agenda) | `IV-1` | nucleo | 40 | 931.989 | 1.09% |
| 18 | [`IV_ProcStatMonit`](IV-3-catalogos-bpm.md#iv_procstatmonit) | `IV-3` | isolada | 6 | 850.022 | 0.99% |
| 19 | [`IMP_Titulo`](IMP.md#imp_titulo) | `IMP` | staging | 42 | 823.952 | 0.96% |
| 20 | [`X_V_IMP_CRM_IMP_NF`](X_TOTVS.md#x_v_imp_crm_imp_nf) | `X_TOTVS` | staging | 6 | 815.854 | 0.95% |
| 21 | [`X_TOTVS_CRM_FATURAMENTO`](X_TOTVS.md#x_totvs_crm_faturamento) | `X_TOTVS` | staging | 41 | 809.821 | 0.95% |
| 22 | [`GE_LOG_TRANS`](GE.md#ge_log_trans) | `GE` | isolada | 10 | 782.089 | 0.92% |
| 23 | [`IV_Interacao`](IV-1-processo-agenda-historico.md#iv_interacao) | `IV-1` | nucleo | 4 | 720.525 | 0.84% |
| 24 | [`IV_ProcFaseMonit`](IV-3-catalogos-bpm.md#iv_procfasemonit) | `IV-3` | isolada | 9 | 655.144 | 0.77% |
| 25 | [`IV_HistLink`](IV-1-processo-agenda-historico.md#iv_histlink) | `IV-1` | isolada | 5 | 645.850 | 0.76% |
| 26 | [`GE_LOG_EXT`](GE.md#ge_log_ext) | `GE` | isolada | 10 | 622.075 | 0.73% |
| 27 | [`IV_ProcLink`](IV-1-processo-agenda-historico.md#iv_proclink) | `IV-1` | isolada | 7 | 602.150 | 0.70% |
| 28 | [`IV_SelecaoPessoa`](IV-4-demais.md#iv_selecaopessoa) | `IV-4` | nucleo | 3 | 582.481 | 0.68% |
| 29 | [`EXT_Titulo`](EXT.md#ext_titulo) | `EXT` | nucleo | 37 | 577.925 | 0.68% |
| 30 | [`GEP_EMAILSENT`](GEP.md#gep_emailsent) | `GEP` | nucleo | 20 | 571.866 | 0.67% |

## 4. Hubs - as tabelas mais referenciadas por FK

Quanto mais FKs apontam para uma tabela, mais ela e o centro do modelo. Fonte: `schema/hubs.csv`.

| Tabela | FKs apontando | Modulo | Classe | Linhas | Funcao (resumo) |
|---|---:|---|---|---:|---|
| [`IV_Questionario`](IV-2-formularios.md#iv_questionario) | 177 | `IV-2` | nucleo | 85.393 | Construtor de formulários: 176 formulários, 2.303 perguntas tipadas com ramificação condicional e peso, 3.995 ... |
| [`GE_Pessoa`](GE.md#ge_pessoa) | 50 | `GE` | nucleo | 118.463 | Cadastro mestre único de pessoas: cliente, prospect, suspect, falecido, fornecedor, funcionário — tudo na mesm... |
| [`GE_Usuario`](GE.md#ge_usuario) | 30 | `GE` | catalogo | 1.379 | Identidade única para USUÁRIO e GRUPO (TipoUsuario 'U'=1.068 / 'G'=318). Guarda credencial, política de segura... |
| [`IV_Resultado`](IV-3-catalogos-bpm.md#iv_resultado) | 21 | `IV-3` | nucleo | 4.201 | O motor BPM declarativo: 980 ações (378 ativas), 4.208 resultados, 5.958 regras de geração automática de agend... |
| [`EXT_VeicTipoMan`](EXT.md#ext_veictipoman) | 18 | `EXT` | catalogo | 1 | Pelo nome, e um catalogo de tipos relacionada a veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP)... |
| [`IV_Propriedade`](IV-2-formularios.md#iv_propriedade) | 15 | `IV-2` | catalogo | 52 | Construtor de entidades customizadas por cliente (o análogo de 'GE_PessoaProp'). IV_Propriedade (52 tipos) def... |
| [`EXT_VeicMarca`](EXT.md#ext_veicmarca) | 12 | `EXT` | catalogo | 5 | Pelo nome, guarda dados de veiculo/equipamento, no modulo `EXT` (espelho de dados do ERP). |
| [`IV_Acao`](IV-3-catalogos-bpm.md#iv_acao) | 11 | `IV-3` | catalogo | 979 | O motor BPM declarativo: 980 ações (378 ativas), 4.208 resultados, 5.958 regras de geração automática de agend... |
| [`IV_CodProcesso`](IV-3-catalogos-bpm.md#iv_codprocesso) | 11 | `IV-3` | catalogo | 62 | Catálogo dos TIPOS DE FLUXO (BPM). 35 col, 62 linhas, 58 EmUso=1. |
| [`IV_TxtPadrao`](IV-3-catalogos-bpm.md#iv_txtpadrao) | 10 | `IV-3` | catalogo | 212 | Já os TEXTOS PADRÃO são muito usados: IV_TxtPadrao = 212 (Aplicacao, Descricao, AssPadrao, Texto, HoraAGUARDAR... |
| [`IV_Agenda`](IV-1-processo-agenda-historico.md#iv_agenda) | 10 | `IV-1` | nucleo | 931.989 | A TAREFA aberta/atribuída. 40 col, 931.989 linhas (930.126 realizadas / 35.724 pendentes). |
| [`IVF_Financeira`](IVF.md#ivf_financeira) | 9 | `IVF` | vazia | 0 | Pelo nome, guarda dados de financiamento, no modulo `IVF` (financiamento). As colunas confirmam vinculo com pe... |
| [`IVS_Depto`](IVS.md#ivs_depto) | 8 | `IVS` | catalogo | 29 | Segmentação: carteira (655), cidades da carteira (670), departamentos da carteira (205), carteirização cliente... |
| [`EXT_Veic`](EXT.md#ext_veic) | 8 | `EXT` | nucleo | 8.020 | Equipamento/máquina e frota do cliente. EXT_Veic 8.020 linhas; EXT_VeicModelo 4.431.168 (!); EXT_VeicPlanoMan ... |
| [`GE_PolSeg`](GE.md#ge_polseg) | 7 | `GE` | catalogo | 11 | Catálogo de 108 itens de política de segurança tipados por módulo (senha, LGPD, agenda, histórico, processo, s... |
| [`IV_Projeto`](IV-4-demais.md#iv_projeto) | 7 | `IV-4` | vazia | 0 | Pelo nome, guarda dados de projeto, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavel... |
| [`IV_ProcDado`](IV-1-processo-agenda-historico.md#iv_procdado) | 6 | `IV-1` | nucleo | 1.516.214 | O CONTEXTO do processo (cliente, tipo de fluxo, vendedor, origem, linhagem). 24 col, 1.516.214 linhas (1.532.1... |
| [`IV_Formulario`](IV-2-formularios.md#iv_formulario) | 6 | `IV-2` | catalogo | 176 | Construtor de formulários: 176 formulários, 2.303 perguntas tipadas com ramificação condicional e peso, 3.995 ... |
| [`IVF_Tabela`](IVF.md#ivf_tabela) | 6 | `IVF` | vazia | 0 | (nao documentado - apurar com acesso ao vivo) |
| [`DMN_Doc`](DMN.md#dmn_doc) | 6 | `DMN` | nucleo | 68.609 | Gestão documental com 107 tipos de documento politicamente configurados, 68.609 documentos, check-in/check-out... |
| [`EXT_VeicPlanoMan`](EXT.md#ext_veicplanoman) | 6 | `EXT` | nucleo | 3.571.257 | Frota do cliente e plano de manutenção — a maior base operacional de pós-venda (3.571.257 linhas de plano de m... |
| [`EXT_Vendedor`](EXT.md#ext_vendedor) | 6 | `EXT` | catalogo | 563 | Catálogos vindos do ERP. EXT_Produto 27 col./78.749 linhas (Protheus 35.027, SISDIA 27.859, 15.860 sem origem)... |
| [`GE_Cidade`](GE.md#ge_cidade) | 5 | `GE` | nucleo | 10.214 | Geografia. GE_Cidade (10.214) e GE_Bairro são usados; GE_Regiao e GE_Rota têm 0 linhas apesar de GE_Pessoa.Seq... |
| [`GE_CONSSQL`](GE.md#ge_conssql) | 5 | `GE` | catalogo | 1 | Terceira geração do motor de consultas, esboçada e abandonada (1 / 0 / 0 / 0 linhas). Tinha modelo melhor: PK ... |
| [`IV_FichaNegVeic`](IV-4-demais.md#iv_fichanegveic) | 5 | `IV-4` | vazia | 0 | Pelo nome, guarda dados de veiculo/equipamento, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo ... |
| [`IV_Historico`](IV-1-processo-agenda-historico.md#iv_historico) | 5 | `IV-1` | nucleo | 2.436.127 | O FATO imutável — cada andamento dado. 29 col, 2.436.127 linhas. |
| [`IV_Processo`](IV-1-processo-agenda-historico.md#iv_processo) | 4 | `IV-1` | nucleo | 1.174.932 | O CASO/oportunidade — guarda apenas o ESTADO. 22 colunas, 1.174.932 linhas (1.190.252 live). |
| [`IV_CobrCrit`](IV-4-demais.md#iv_cobrcrit) | 4 | `IV-4` | vazia | 0 | Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). As colunas conf... |
| [`IV_CbrCriterio`](IV-4-demais.md#iv_cbrcriterio) | 4 | `IV-4` | catalogo | 122 | Pelo nome, e um criterio configuravel relacionada a cobranca, no modulo `IV` (nucleo CRM/BPM). |
| [`IVF_TipoAgregado`](IVF.md#ivf_tipoagregado) | 4 | `IVF` | vazia | 0 | Pelo nome, e um catalogo de tipos, no modulo `IVF` (financiamento). Sem linhas no snapshot - recurso provavelm... |

## 5. Classificacao das tabelas

| Classe | Tabelas | % | Linhas | O que significa |
|---|---:|---:|---:|---|
| `nucleo` | 117 | 15.3% | 24.833.428 | Tem linhas e participa do grafo relacional (aponta para outras tabelas ou e apontada por elas). E o dado vivo do sistema. |
| `catalogo` | 34 | 4.4% | 8.581 | Poucas linhas (<= 2000) e referenciada por >= 2 chaves estrangeiras: tabela de dominio/parametrizacao. |
| `staging` | 35 | 4.6% | 4.124.982 | Area de pouso da integracao com ERP (`IMP_*`, `X_TOTVS_*`, `X_T_*`, `X_V_*`, `JDE_*`). Dado bruto antes de virar `EXT_*`. |
| `formulario-materializado` | 175 | 22.8% | 87.532 | Tabela fisica `IV_Q_<Formulario>` gerada por DDL pelo motor de formularios: 1 coluna por questao respondida. |
| `vazia` | 235 | 30.6% | 0 | 0 linhas no snapshot: recurso do produto nao usado na Tracbel, ou tabela nova/abandonada. |
| `isolada` | 112 | 14.6% | 53.983.249 | Tem linhas mas nenhuma FK entrando nem saindo: nao se conecta ao modelo por integridade declarada (pode se conectar por convencao de nome). |
| `lixo/backup` | 59 | 7.7% | 2.417.520 | Copia manual, teste, migracao ou legado. **Nunca usar em consulta de producao** (SCHEMA_MAP.md). |
| **TOTAL** | **767** | **100%** | **85.455.292** | |

As regras sao avaliadas nesta ordem (a primeira que casar vence): `lixo/backup` -> `formulario-materializado` -> `staging` -> `vazia` -> `catalogo` -> `nucleo` -> `isolada`. Por isso uma `IV_Q_*` sem linhas aparece como `formulario-materializado`, e uma `IMP_*` sem linhas aparece como `staging`.

### Classe `nucleo` - 117 tabelas

Tem linhas e participa do grafo relacional (aponta para outras tabelas ou e apontada por elas). E o dado vivo do sistema.

<details><summary>Lista completa das 117 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`DMN_Doc`](DMN.md#dmn_doc) | `DMN` | 17 | 68.609 |
| [`DMN_DocPes`](DMN.md#dmn_docpes) | `DMN` | 4 | 73.576 |
| [`EXT_NFS`](EXT.md#ext_nfs) | `EXT` | 39 | 390.755 |
| [`EXT_NFSItem`](EXT.md#ext_nfsitem) | `EXT` | 27 | 1.300.126 |
| [`EXT_NFSOperEmp`](EXT.md#ext_nfsoperemp) | `EXT` | 3 | 330 |
| [`EXT_OS`](EXT.md#ext_os) | `EXT` | 40 | 8.099 |
| [`EXT_Pessoa`](EXT.md#ext_pessoa) | `EXT` | 77 | 57.682 |
| [`EXT_Produto`](EXT.md#ext_produto) | `EXT` | 27 | 78.749 |
| [`EXT_Titulo`](EXT.md#ext_titulo) | `EXT` | 37 | 577.925 |
| [`EXT_TituloMov`](EXT.md#ext_titulomov) | `EXT` | 9 | 1.134.039 |
| [`EXT_Veic`](EXT.md#ext_veic) | `EXT` | 28 | 8.020 |
| [`EXT_VeicKM`](EXT.md#ext_veickm) | `EXT` | 11 | 17.378 |
| [`EXT_VeicMarcaEmp`](EXT.md#ext_veicmarcaemp) | `EXT` | 4 | 2 |
| [`EXT_VeicModelo`](EXT.md#ext_veicmodelo) | `EXT` | 5 | 4.431.168 |
| [`EXT_VeicModPlano`](EXT.md#ext_veicmodplano) | `EXT` | 2 | 3.571.235 |
| [`EXT_VeicPlanoMan`](EXT.md#ext_veicplanoman) | `EXT` | 4 | 3.571.257 |
| [`EXT_VeicProp`](EXT.md#ext_veicprop) | `EXT` | 24 | 12.621 |
| [`GEP_EMAILSENT`](GEP.md#gep_emailsent) | `GEP` | 20 | 571.866 |
| [`GEP_JOBAGD`](GEP.md#gep_jobagd) | `GEP` | 18 | 25 |
| [`GEP_JOBFILA`](GEP.md#gep_jobfila) | `GEP` | 16 | 23 |
| [`GEP_ParRecebe`](GEP.md#gep_parrecebe) | `GEP` | 12 | 14 |
| [`GEP_Processo`](GEP.md#gep_processo) | `GEP` | 20 | 8 |
| [`GEP_UsrSat`](GEP.md#gep_usrsat) | `GEP` | 16 | 71 |
| [`GE_Aplicacao`](GE.md#ge_aplicacao) | `GE` | 9 | 184 |
| [`GE_AppModulo`](GE.md#ge_appmodulo) | `GE` | 3 | 30 |
| [`GE_CampoMemo`](GE.md#ge_campomemo) | `GE` | 4 | 3.129 |
| [`GE_CampoPerm`](GE.md#ge_campoperm) | `GE` | 7 | 17 |
| [`GE_Cidade`](GE.md#ge_cidade) | `GE` | 19 | 10.214 |
| [`GE_Cidade_CRM`](GE.md#ge_cidade_crm) | `GE` | 18 | 729 |
| [`GE_Consulta`](GE.md#ge_consulta) | `GE` | 29 | 2 |
| [`GE_ConsultaVar`](GE.md#ge_consultavar) | `GE` | 10 | 3 |
| [`GE_EMAILAGD`](GE.md#ge_emailagd) | `GE` | 13 | 2 |
| [`GE_EMAILTEMPLATE`](GE.md#ge_emailtemplate) | `GE` | 6 | 8 |
| [`GE_Membro`](GE.md#ge_membro) | `GE` | 3 | 1.584 |
| [`GE_ModuloPerm`](GE.md#ge_moduloperm) | `GE` | 6 | 25.435 |
| [`GE_Permissao`](GE.md#ge_permissao) | `GE` | 11 | 307 |
| [`GE_Pessoa`](GE.md#ge_pessoa) | `GE` | 76 | 118.463 |
| [`GE_PessoaEnd`](GE.md#ge_pessoaend) | `GE` | 26 | 6.480 |
| [`GE_PessoaEndOutroBc`](GE.md#ge_pessoaendoutrobc) | `GE` | 3 | 30.943 |
| [`GE_PessoaNota`](GE.md#ge_pessoanota) | `GE` | 2 | 1.232 |
| [`GE_PessoaSimilar`](GE.md#ge_pessoasimilar) | `GE` | 14 | 124.222 |
| [`GE_PolSegAces`](GE.md#ge_polsegaces) | `GE` | 7 | 3 |
| [`GE_PolSegCtrl`](GE.md#ge_polsegctrl) | `GE` | 7 | 254 |
| [`GE_PolSegItLst`](GE.md#ge_polsegitlst) | `GE` | 6 | 66 |
| [`GE_PolSegParams`](GE.md#ge_polsegparams) | `GE` | 7 | 1 |
| [`GE_POLSEGPERM`](GE.md#ge_polsegperm) | `GE` | 5 | 805 |
| [`GE_QVConsCol`](GE.md#ge_qvconscol) | `GE` | 8 | 40 |
| [`GE_QVConsHst`](GE.md#ge_qvconshst) | `GE` | 5 | 69 |
| [`GE_QVConsVar`](GE.md#ge_qvconsvar) | `GE` | 17 | 581 |
| [`GE_QVPasta`](GE.md#ge_qvpasta) | `GE` | 4 | 28 |
| [`GE_QVPastaCons`](GE.md#ge_qvpastacons) | `GE` | 2 | 113 |
| [`GE_Sistema`](GE.md#ge_sistema) | `GE` | 3 | 11 |
| [`GE_UsrParam`](GE.md#ge_usrparam) | `GE` | 7 | 16.827 |
| [`IVS_CartCid`](IVS.md#ivs_cartcid) | `IVS` | 4 | 670 |
| [`IVS_CartDepto`](IVS.md#ivs_cartdepto) | `IVS` | 4 | 205 |
| [`IVS_DeptoEmpr`](IVS.md#ivs_deptoempr) | `IVS` | 5 | 13 |
| [`IVS_DEPTOPOT`](IVS.md#ivs_deptopot) | `IVS` | 8 | 126 |
| [`IVS_DeptoRes`](IVS.md#ivs_deptores) | `IVS` | 7 | 63 |
| [`IVS_Pes`](IVS.md#ivs_pes) | `IVS` | 30 | 138.584 |
| [`IVS_UsrMeta`](IVS.md#ivs_usrmeta) | `IVS` | 13 | 1 |
| [`IV_AcaoAuto`](IV-3-catalogos-bpm.md#iv_acaoauto) | `IV-3` | 31 | 5.942 |
| [`IV_AcaoCtrl`](IV-3-catalogos-bpm.md#iv_acaoctrl) | `IV-3` | 13 | 44 |
| [`IV_AcaoRem`](IV-3-catalogos-bpm.md#iv_acaorem) | `IV-3` | 9 | 266 |
| [`IV_AgdLink`](IV-1-processo-agenda-historico.md#iv_agdlink) | `IV-1` | 11 | 4.534 |
| [`IV_Agenda`](IV-1-processo-agenda-historico.md#iv_agenda) | `IV-1` | 40 | 931.989 |
| [`IV_AgendaCtrl`](IV-1-processo-agenda-historico.md#iv_agendactrl) | `IV-1` | 6 | 963 |
| [`IV_AtribLista`](IV-2-formularios.md#iv_atriblista) | `IV-2` | 2 | 37 |
| [`IV_Atributo`](IV-2-formularios.md#iv_atributo) | `IV-2` | 11 | 10 |
| [`IV_CbrCobranca`](IV-4-demais.md#iv_cbrcobranca) | `IV-4` | 8 | 64.449 |
| [`IV_CbrCobrancaLote`](IV-4-demais.md#iv_cbrcobrancalote) | `IV-4` | 4 | 21.048 |
| [`IV_CbrCobrancaTit`](IV-4-demais.md#iv_cbrcobrancatit) | `IV-4` | 10 | 155.204 |
| [`IV_CbrCobrancaTitLog`](IV-4-demais.md#iv_cbrcobrancatitlog) | `IV-4` | 5 | 304.684 |
| [`IV_CbrCriterioDef`](IV-4-demais.md#iv_cbrcriteriodef) | `IV-4` | 16 | 231 |
| [`IV_CBRCRITMON`](IV-4-demais.md#iv_cbrcritmon) | `IV-4` | 17 | 128.167 |
| [`IV_ClasseRes`](IV-3-catalogos-bpm.md#iv_classeres) | `IV-3` | 5 | 40 |
| [`IV_ClientePropr`](IV-2-formularios.md#iv_clientepropr) | `IV-2` | 51 | 246.684 |
| [`IV_CodPrcEmpr`](IV-3-catalogos-bpm.md#iv_codprcempr) | `IV-3` | 2 | 17 |
| [`IV_CodProcComent`](IV-3-catalogos-bpm.md#iv_codproccoment) | `IV-3` | 4 | 1 |
| [`IV_ConhecFonema`](IV-4-demais.md#iv_conhecfonema) | `IV-4` | 2 | 19 |
| [`IV_ConhecLeitura`](IV-4-demais.md#iv_conhecleitura) | `IV-4` | 5 | 49 |
| [`IV_Distribui`](IV-1-processo-agenda-historico.md#iv_distribui) | `IV-1` | 3 | 609 |
| [`IV_DoctoApl`](IV-3-catalogos-bpm.md#iv_doctoapl) | `IV-3` | 15 | 6 |
| [`IV_DoctoAplUso`](IV-3-catalogos-bpm.md#iv_doctoapluso) | `IV-3` | 10 | 3 |
| [`IV_DoctoTipo`](IV-3-catalogos-bpm.md#iv_doctotipo) | `IV-3` | 5 | 3 |
| [`IV_GlobalPar`](IV-3-catalogos-bpm.md#iv_globalpar) | `IV-3` | 43 | 3.848 |
| [`IV_GlobalParLista`](IV-3-catalogos-bpm.md#iv_globalparlista) | `IV-3` | 4 | 197 |
| [`IV_Historico`](IV-1-processo-agenda-historico.md#iv_historico) | `IV-1` | 29 | 2.436.127 |
| [`IV_Interacao`](IV-1-processo-agenda-historico.md#iv_interacao) | `IV-1` | 4 | 720.525 |
| [`IV_OpBloq`](IV-4-demais.md#iv_opbloq) | `IV-4` | 8 | 4 |
| [`IV_OPTEMAIL`](IV-4-demais.md#iv_optemail) | `IV-4` | 7 | 21 |
| [`IV_OPTFONE`](IV-4-demais.md#iv_optfone) | `IV-4` | 7 | 2 |
| [`IV_PcteAtrFx`](IV-2-formularios.md#iv_pcteatrfx) | `IV-2` | 3 | 22 |
| [`IV_ProcDado`](IV-1-processo-agenda-historico.md#iv_procdado) | `IV-1` | 24 | 1.516.214 |
| [`IV_Processo`](IV-1-processo-agenda-historico.md#iv_processo) | `IV-1` | 22 | 1.174.932 |
| [`IV_ProcFase`](IV-3-catalogos-bpm.md#iv_procfase) | `IV-3` | 5 | 273 |
| [`IV_ProcPersp`](IV-3-catalogos-bpm.md#iv_procpersp) | `IV-3` | 4 | 67 |
| [`IV_ProcRef`](IV-1-processo-agenda-historico.md#iv_procref) | `IV-1` | 5 | 61.975 |
| [`IV_ProcResultado`](IV-3-catalogos-bpm.md#iv_procresultado) | `IV-3` | 5 | 2.153 |
| [`IV_ProcSt`](IV-3-catalogos-bpm.md#iv_procst) | `IV-3` | 4 | 250 |
| [`IV_Produto`](IV-4-demais.md#iv_produto) | `IV-4` | 24 | 1 |
| [`IV_PropriLista`](IV-2-formularios.md#iv_proprilista) | `IV-2` | 5 | 7.375 |
| [`IV_Questao`](IV-2-formularios.md#iv_questao) | `IV-2` | 19 | 2.300 |
| [`IV_QuestaoLista`](IV-2-formularios.md#iv_questaolista) | `IV-2` | 8 | 3.995 |
| [`IV_Questionario`](IV-2-formularios.md#iv_questionario) | `IV-2` | 18 | 85.393 |
| [`IV_ResClasse`](IV-3-catalogos-bpm.md#iv_resclasse) | `IV-3` | 2 | 236 |
| [`IV_ResMsgPapel`](IV-3-catalogos-bpm.md#iv_resmsgpapel) | `IV-3` | 28 | 1.341 |
| [`IV_Resultado`](IV-3-catalogos-bpm.md#iv_resultado) | `IV-3` | 63 | 4.201 |
| [`IV_ResultadoCmpl`](IV-3-catalogos-bpm.md#iv_resultadocmpl) | `IV-3` | 2 | 3.110 |
| [`IV_ResultadoInstr`](IV-3-catalogos-bpm.md#iv_resultadoinstr) | `IV-3` | 2 | 98 |
| [`IV_ResVinc`](IV-3-catalogos-bpm.md#iv_resvinc) | `IV-3` | 8 | 959 |
| [`IV_SelecaoCriterio`](IV-4-demais.md#iv_selecaocriterio) | `IV-4` | 7 | 2.865 |
| [`IV_SelecaoPessoa`](IV-4-demais.md#iv_selecaopessoa) | `IV-4` | 3 | 582.481 |
| [`IV_TxtPadraoUso`](IV-3-catalogos-bpm.md#iv_txtpadraouso) | `IV-3` | 4 | 5 |
| [`IV_USRSTATUS`](IV-4-demais.md#iv_usrstatus) | `IV-4` | 7 | 186 |
| [`VBI_CONTA`](VBI.md#vbi_conta) | `VBI` | 14 | 1.004 |
| [`VBI_CONTAFAM`](VBI.md#vbi_contafam) | `VBI` | 6 | 1.004 |
| [`VBI_CONTAPAI`](VBI.md#vbi_contapai) | `VBI` | 18 | 243 |

</details>

### Classe `catalogo` - 34 tabelas

Poucas linhas (<= 2000) e referenciada por >= 2 chaves estrangeiras: tabela de dominio/parametrizacao.

<details><summary>Lista completa das 34 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`EXT_NFSOper`](EXT.md#ext_nfsoper) | `EXT` | 9 | 241 |
| [`EXT_VeicFam`](EXT.md#ext_veicfam) | `EXT` | 6 | 68 |
| [`EXT_VeicMarca`](EXT.md#ext_veicmarca) | `EXT` | 5 | 5 |
| [`EXT_VeicTipoMan`](EXT.md#ext_veictipoman) | `EXT` | 8 | 1 |
| [`EXT_Vendedor`](EXT.md#ext_vendedor) | `EXT` | 6 | 563 |
| [`GEP_JOBCAD`](GEP.md#gep_jobcad) | `GEP` | 11 | 71 |
| [`GEP_ParEnvia`](GEP.md#gep_parenvia) | `GEP` | 7 | 3 |
| [`GE_AtributoFixo`](GE.md#ge_atributofixo) | `GE` | 9 | 183 |
| [`GE_Bairro`](GE.md#ge_bairro) | `GE` | 3 | 619 |
| [`GE_Col`](GE.md#ge_col) | `GE` | 6 | 217 |
| [`GE_CONSSQL`](GE.md#ge_conssql) | `GE` | 10 | 1 |
| [`GE_Empresa`](GE.md#ge_empresa) | `GE` | 33 | 18 |
| [`GE_Modulo`](GE.md#ge_modulo) | `GE` | 8 | 48 |
| [`GE_ParamLista`](GE.md#ge_paramlista) | `GE` | 25 | 333 |
| [`GE_PolSeg`](GE.md#ge_polseg) | `GE` | 3 | 11 |
| [`GE_PolSegItem`](GE.md#ge_polsegitem) | `GE` | 7 | 108 |
| [`GE_QVCons`](GE.md#ge_qvcons) | `GE` | 28 | 134 |
| [`GE_Tab`](GE.md#ge_tab) | `GE` | 2 | 12 |
| [`GE_Usuario`](GE.md#ge_usuario) | `GE` | 35 | 1.379 |
| [`IVS_Carteira`](IVS.md#ivs_carteira) | `IVS` | 16 | 655 |
| [`IVS_Depto`](IVS.md#ivs_depto) | `IVS` | 29 | 29 |
| [`IVS_Segm`](IVS.md#ivs_segm) | `IVS` | 5 | 8 |
| [`IV_Acao`](IV-3-catalogos-bpm.md#iv_acao) | `IV-3` | 37 | 979 |
| [`IV_CbrCriterio`](IV-4-demais.md#iv_cbrcriterio) | `IV-4` | 26 | 122 |
| [`IV_CodProcesso`](IV-3-catalogos-bpm.md#iv_codprocesso) | `IV-3` | 35 | 62 |
| [`IV_Conhecimento`](IV-4-demais.md#iv_conhecimento) | `IV-4` | 19 | 9 |
| [`IV_Formulario`](IV-2-formularios.md#iv_formulario) | `IV-2` | 16 | 176 |
| [`IV_GlobalParCtrl`](IV-3-catalogos-bpm.md#iv_globalparctrl) | `IV-3` | 54 | 48 |
| [`IV_Operador`](IV-4-demais.md#iv_operador) | `IV-4` | 51 | 932 |
| [`IV_ProcAcao`](IV-1-processo-agenda-historico.md#iv_procacao) | `IV-1` | 3 | 738 |
| [`IV_Propriedade`](IV-2-formularios.md#iv_propriedade) | `IV-2` | 70 | 52 |
| [`IV_Selecao`](IV-4-demais.md#iv_selecao) | `IV-4` | 12 | 543 |
| [`IV_TIPOCONTEUDO`](IV-3-catalogos-bpm.md#iv_tipoconteudo) | `IV-3` | 3 | 1 |
| [`IV_TxtPadrao`](IV-3-catalogos-bpm.md#iv_txtpadrao) | `IV-3` | 11 | 212 |

</details>

### Classe `staging` - 35 tabelas

Area de pouso da integracao com ERP (`IMP_*`, `X_TOTVS_*`, `X_T_*`, `X_V_*`, `JDE_*`). Dado bruto antes de virar `EXT_*`.

<details><summary>Lista completa das 35 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`IMP_EMAIL`](IMP.md#imp_email) | `IMP` | 13 | 0 |
| [`IMP_Evento`](IMP.md#imp_evento) | `IMP` | 26 | 0 |
| [`IMP_FORMAPGTO`](IMP.md#imp_formapgto) | `IMP` | 12 | 0 |
| [`IMP_LINKPESSOA`](IMP.md#imp_linkpessoa) | `IMP` | 4 | 20.757 |
| [`IMP_LOGAPROVACAO`](IMP.md#imp_logaprovacao) | `IMP` | 2 | 0 |
| [`IMP_NFS`](IMP.md#imp_nfs) | `IMP` | 45 | 18.378 |
| [`IMP_NFSCMPL`](IMP.md#imp_nfscmpl) | `IMP` | 9 | 0 |
| [`IMP_NFSItem`](IMP.md#imp_nfsitem) | `IMP` | 40 | 67.404 |
| [`IMP_OS`](IMP.md#imp_os) | `IMP` | 46 | 287.868 |
| [`IMP_OSITEM`](IMP.md#imp_ositem) | `IMP` | 24 | 399.040 |
| [`IMP_OSSOLIC`](IMP.md#imp_ossolic) | `IMP` | 11 | 0 |
| [`IMP_Pedido`](IMP.md#imp_pedido) | `IMP` | 61 | 0 |
| [`IMP_PedidoItem`](IMP.md#imp_pedidoitem) | `IMP` | 30 | 0 |
| [`IMP_PESSOA`](IMP.md#imp_pessoa) | `IMP` | 76 | 0 |
| [`IMP_PESSOACONTATO`](IMP.md#imp_pessoacontato) | `IMP` | 27 | 0 |
| [`IMP_PESSOAFONE`](IMP.md#imp_pessoafone) | `IMP` | 19 | 0 |
| [`IMP_PRODUTO`](IMP.md#imp_produto) | `IMP` | 29 | 0 |
| [`IMP_REL_TBA101`](IMP.md#imp_rel_tba101) | `IMP` | 29 | 318 |
| [`IMP_REL_TBA101_PG2`](IMP.md#imp_rel_tba101_pg2) | `IMP` | 31 | 208 |
| [`IMP_Titulo`](IMP.md#imp_titulo) | `IMP` | 42 | 823.952 |
| [`IMP_VEICULO`](IMP.md#imp_veiculo) | `IMP` | 49 | 0 |
| [`IMP_VEICULO_INTEGRADO`](IMP.md#imp_veiculo_integrado) | `IMP` | 50 | 0 |
| [`JDE_CATEGORY`](JDE.md#jde_category) | `JDE` | 2 | 0 |
| [`JDE_EQUIPAMENTS`](JDE.md#jde_equipaments) | `JDE` | 18 | 0 |
| [`JDE_PURCHASE`](JDE.md#jde_purchase) | `JDE` | 18 | 0 |
| [`JDE_QUOTE`](JDE.md#jde_quote) | `JDE` | 19 | 0 |
| [`JDE_QUOTE_ITEM`](JDE.md#jde_quote_item) | `JDE` | 2 | 0 |
| [`JDE_SALES_PERSON`](JDE.md#jde_sales_person) | `JDE` | 5 | 0 |
| [`JDE_SUB_CATEGORY`](JDE.md#jde_sub_category) | `JDE` | 2 | 0 |
| [`X_TOTVS_BI_FATURAMENTO_MAQUINAS`](X_TOTVS.md#x_totvs_bi_faturamento_maquinas) | `X_TOTVS` | 13 | 4.612 |
| [`X_TOTVS_BI_FATURAMENTO_POS_VENDAS`](X_TOTVS.md#x_totvs_bi_faturamento_pos_vendas) | `X_TOTVS` | 30 | 407.100 |
| [`X_TOTVS_CRM_FATURAMENTO`](X_TOTVS.md#x_totvs_crm_faturamento) | `X_TOTVS` | 41 | 809.821 |
| [`X_T_IMP_CRM_TITULO`](X_TOTVS.md#x_t_imp_crm_titulo) | `X_TOTVS` | 41 | 462.391 |
| [`X_T_IMP_CRM_VEICULO`](X_TOTVS.md#x_t_imp_crm_veiculo) | `X_TOTVS` | 7 | 7.279 |
| [`X_V_IMP_CRM_IMP_NF`](X_TOTVS.md#x_v_imp_crm_imp_nf) | `X_TOTVS` | 6 | 815.854 |

</details>

### Classe `formulario-materializado` - 175 tabelas

Tabela fisica `IV_Q_<Formulario>` gerada por DDL pelo motor de formularios: 1 coluna por questao respondida.

<details><summary>Lista completa das 175 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`IV_Q_ABERTURA_OS_REVISAO`](IV-2-formularios.md#iv_q_abertura_os_revisao) | `IV-2` | 3 | 10 |
| [`IV_Q_ACOMPANHAMENTO_VENDA`](IV-2-formularios.md#iv_q_acompanhamento_venda) | `IV-2` | 53 | 1.715 |
| [`IV_Q_ACOMPANHAM_VENDA_IMP`](IV-2-formularios.md#iv_q_acompanham_venda_imp) | `IV-2` | 39 | 3.308 |
| [`IV_Q_ACOMPANHA_COMPRA_IMP`](IV-2-formularios.md#iv_q_acompanha_compra_imp) | `IV-2` | 16 | 26 |
| [`IV_Q_ACOMPANH_VENDA_JDE`](IV-2-formularios.md#iv_q_acompanh_venda_jde) | `IV-2` | 104 | 6.343 |
| [`IV_Q_ACOMPAN_COMPRA_JDE`](IV-2-formularios.md#iv_q_acompan_compra_jde) | `IV-2` | 17 | 153 |
| [`IV_Q_ACOMPAN_VEND_CONCESS`](IV-2-formularios.md#iv_q_acompan_vend_concess) | `IV-2` | 9 | 98 |
| [`IV_Q_ACOMP_VENDA_DIRETA`](IV-2-formularios.md#iv_q_acomp_venda_direta) | `IV-2` | 20 | 125 |
| [`IV_Q_ACOMP_VENDA_DIRETAJD`](IV-2-formularios.md#iv_q_acomp_venda_diretajd) | `IV-2` | 94 | 270 |
| [`IV_Q_ACOMP_VENDA_FINANC`](IV-2-formularios.md#iv_q_acomp_venda_financ) | `IV-2` | 9 | 2.021 |
| [`IV_Q_ACOMP_VENDA_LOCACAO`](IV-2-formularios.md#iv_q_acomp_venda_locacao) | `IV-2` | 14 | 15 |
| [`IV_Q_ACOMP_VENDA_MANITOU`](IV-2-formularios.md#iv_q_acomp_venda_manitou) | `IV-2` | 45 | 21 |
| [`IV_Q_ACOMP_VENDA_USADO`](IV-2-formularios.md#iv_q_acomp_venda_usado) | `IV-2` | 44 | 256 |
| [`IV_Q_ACOMP_VEND_MAQUINAS`](IV-2-formularios.md#iv_q_acomp_vend_maquinas) | `IV-2` | 54 | 1.535 |
| [`IV_Q_ADM_FINANCEIRO`](IV-2-formularios.md#iv_q_adm_financeiro) | `IV-2` | 14 | 1.884 |
| [`IV_Q_AFERICAO_CSC`](IV-2-formularios.md#iv_q_afericao_csc) | `IV-2` | 8 | 32 |
| [`IV_Q_AFERICAO_DE_PECAS`](IV-2-formularios.md#iv_q_afericao_de_pecas) | `IV-2` | 8 | 2 |
| [`IV_Q_AFERICAO_IMPLEMENTO`](IV-2-formularios.md#iv_q_afericao_implemento) | `IV-2` | 13 | 440 |
| [`IV_Q_AFERICAO_MAQ_1_CONT`](IV-2-formularios.md#iv_q_afericao_maq_1_cont) | `IV-2` | 12 | 873 |
| [`IV_Q_AFERICAO_MAQ_1_WEB`](IV-2-formularios.md#iv_q_afericao_maq_1_web) | `IV-2` | 9 | 2 |
| [`IV_Q_AFERICAO_MAQ_2_CONT`](IV-2-formularios.md#iv_q_afericao_maq_2_cont) | `IV-2` | 8 | 541 |
| [`IV_Q_AFERICAO_MAQ_2_WEB`](IV-2-formularios.md#iv_q_afericao_maq_2_web) | `IV-2` | 6 | 1 |
| [`IV_Q_AFERICAO_MAQ_3_CONT`](IV-2-formularios.md#iv_q_afericao_maq_3_cont) | `IV-2` | 13 | 484 |
| [`IV_Q_AFERICAO_MAQ_3_WEB`](IV-2-formularios.md#iv_q_afericao_maq_3_web) | `IV-2` | 12 | 3 |
| [`IV_Q_AFERICAO_PECAS`](IV-2-formularios.md#iv_q_afericao_pecas) | `IV-2` | 33 | 1.719 |
| [`IV_Q_AFERICAO_POS_SOLUCAO`](IV-2-formularios.md#iv_q_afericao_pos_solucao) | `IV-2` | 4 | 452 |
| [`IV_Q_AFERICAO_SERVICOS`](IV-2-formularios.md#iv_q_afericao_servicos) | `IV-2` | 16 | 635 |
| [`IV_Q_AFERICAO_SERV_WEB`](IV-2-formularios.md#iv_q_afericao_serv_web) | `IV-2` | 17 | 31 |
| [`IV_Q_AFERICAO_SUPORTE_INT`](IV-2-formularios.md#iv_q_afericao_suporte_int) | `IV-2` | 2 | 0 |
| [`IV_Q_AFERICAO_VENDA_MAQ`](IV-2-formularios.md#iv_q_afericao_venda_maq) | `IV-2` | 10 | 979 |
| [`IV_Q_AFERICAO_VENDA_MQ_IM`](IV-2-formularios.md#iv_q_afericao_venda_mq_im) | `IV-2` | 32 | 604 |
| [`IV_Q_AFE_VENDA_MQ_IM_USAD`](IV-2-formularios.md#iv_q_afe_venda_mq_im_usad) | `IV-2` | 26 | 1 |
| [`IV_Q_AGUARDAR_PECAS`](IV-2-formularios.md#iv_q_aguardar_pecas) | `IV-2` | 2 | 31 |
| [`IV_Q_ALTERADO_PAGAMENTO`](IV-2-formularios.md#iv_q_alterado_pagamento) | `IV-2` | 4 | 0 |
| [`IV_Q_APRESENTACAO`](IV-2-formularios.md#iv_q_apresentacao) | `IV-2` | 8 | 211 |
| [`IV_Q_APRESENTACAO_JDE`](IV-2-formularios.md#iv_q_apresentacao_jde) | `IV-2` | 8 | 123 |
| [`IV_Q_APRESENT_EQUIPAMENTO`](IV-2-formularios.md#iv_q_apresent_equipamento) | `IV-2` | 1 | 0 |
| [`IV_Q_APRESENT_IMPLEMENTO`](IV-2-formularios.md#iv_q_apresent_implemento) | `IV-2` | 7 | 43 |
| [`IV_Q_APROVACAO_TCSM`](IV-2-formularios.md#iv_q_aprovacao_tcsm) | `IV-2` | 2 | 27 |
| [`IV_Q_ATUALIZACAO_PUK`](IV-2-formularios.md#iv_q_atualizacao_puk) | `IV-2` | 10 | 0 |
| [`IV_Q_AVALIACAO_AMS_USADO`](IV-2-formularios.md#iv_q_avaliacao_ams_usado) | `IV-2` | 6 | 4 |
| [`IV_Q_AVALIA_COLHEIT_USADA`](IV-2-formularios.md#iv_q_avalia_colheit_usada) | `IV-2` | 51 | 1 |
| [`IV_Q_AVALIA_IMPLEM_USADO`](IV-2-formularios.md#iv_q_avalia_implem_usado) | `IV-2` | 38 | 60 |
| [`IV_Q_AVALIA_USADO_ENTRADA`](IV-2-formularios.md#iv_q_avalia_usado_entrada) | `IV-2` | 35 | 823 |
| [`IV_Q_AVAL_COLH_CANA_USADA`](IV-2-formularios.md#iv_q_aval_colh_cana_usada) | `IV-2` | 32 | 1 |
| [`IV_Q_AVAL_TRATORES_USADOS`](IV-2-formularios.md#iv_q_aval_tratores_usados) | `IV-2` | 53 | 0 |
| [`IV_Q_AVAL_USADO_ENTRADA`](IV-2-formularios.md#iv_q_aval_usado_entrada) | `IV-2` | 12 | 200 |
| [`IV_Q_CADASTROS_LISTAS`](IV-2-formularios.md#iv_q_cadastros_listas) | `IV-2` | 2 | 0 |
| [`IV_Q_CANCELAMENTO_SEGURO`](IV-2-formularios.md#iv_q_cancelamento_seguro) | `IV-2` | 5 | 4 |
| [`IV_Q_CANCEL_RENOVACAO_SEG`](IV-2-formularios.md#iv_q_cancel_renovacao_seg) | `IV-2` | 5 | 0 |
| [`IV_Q_CANHOTO_DIGITAL`](IV-2-formularios.md#iv_q_canhoto_digital) | `IV-2` | 5 | 0 |
| [`IV_Q_CHASSI_ENTREGA_FISIC`](IV-2-formularios.md#iv_q_chassi_entrega_fisic) | `IV-2` | 2 | 1.064 |
| [`IV_Q_CHASSI_EQUIPAMENTO`](IV-2-formularios.md#iv_q_chassi_equipamento) | `IV-2` | 3 | 1.039 |
| [`IV_Q_CHASSI_PMP`](IV-2-formularios.md#iv_q_chassi_pmp) | `IV-2` | 4 | 767 |
| [`IV_Q_CHEGADA_IMPLEMENTO`](IV-2-formularios.md#iv_q_chegada_implemento) | `IV-2` | 2 | 61 |
| [`IV_Q_COMISSAO`](IV-2-formularios.md#iv_q_comissao) | `IV-2` | 36 | 7.165 |
| [`IV_Q_COMISSAO_AMS`](IV-2-formularios.md#iv_q_comissao_ams) | `IV-2` | 20 | 633 |
| [`IV_Q_COMISSAO_CONTACHAVE`](IV-2-formularios.md#iv_q_comissao_contachave) | `IV-2` | 18 | 144 |
| [`IV_Q_COMISSAO_SERV_AMS`](IV-2-formularios.md#iv_q_comissao_serv_ams) | `IV-2` | 15 | 448 |
| [`IV_Q_COMISSAO_USADO`](IV-2-formularios.md#iv_q_comissao_usado) | `IV-2` | 5 | 240 |
| [`IV_Q_COMISSAO_VD_LOCACAO`](IV-2-formularios.md#iv_q_comissao_vd_locacao) | `IV-2` | 13 | 0 |
| [`IV_Q_COMPETIDORES_NA_NEG`](IV-2-formularios.md#iv_q_competidores_na_neg) | `IV-2` | 9 | 104 |
| [`IV_Q_COMPETIDORES_NA_NEGO`](IV-2-formularios.md#iv_q_competidores_na_nego) | `IV-2` | 10 | 101 |
| [`IV_Q_COMPETID_NEGOC_IMPL`](IV-2-formularios.md#iv_q_competid_negoc_impl) | `IV-2` | 9 | 13 |
| [`IV_Q_COM_INTERESSE_FUTURO`](IV-2-formularios.md#iv_q_com_interesse_futuro) | `IV-2` | 9 | 0 |
| [`IV_Q_CONDICOES_DE_VENDAS`](IV-2-formularios.md#iv_q_condicoes_de_vendas) | `IV-2` | 11 | 0 |
| [`IV_Q_CONT_COMISSAO_22`](IV-2-formularios.md#iv_q_cont_comissao_22) | `IV-2` | 18 | 2.251 |
| [`IV_Q_COTA_CONSORCIO`](IV-2-formularios.md#iv_q_cota_consorcio) | `IV-2` | 8 | 73 |
| [`IV_Q_DEMONSTRACAO_JD`](IV-2-formularios.md#iv_q_demonstracao_jd) | `IV-2` | 33 | 215 |
| [`IV_Q_DEMONSTRACAO_LOG`](IV-2-formularios.md#iv_q_demonstracao_log) | `IV-2` | 3 | 25 |
| [`IV_Q_DEMONSTRACAO_NF`](IV-2-formularios.md#iv_q_demonstracao_nf) | `IV-2` | 2 | 44 |
| [`IV_Q_DEMONSTRACAO_TRATOR`](IV-2-formularios.md#iv_q_demonstracao_trator) | `IV-2` | 25 | 32 |
| [`IV_Q_DEMONSTR_COLHEITAD`](IV-2-formularios.md#iv_q_demonstr_colheitad) | `IV-2` | 8 | 0 |
| [`IV_Q_DEMO_EQUIP_JD`](IV-2-formularios.md#iv_q_demo_equip_jd) | `IV-2` | 23 | 0 |
| [`IV_Q_DEMO_IMPLEMENTO`](IV-2-formularios.md#iv_q_demo_implemento) | `IV-2` | 25 | 0 |
| [`IV_Q_DEMO_MAQUINAS`](IV-2-formularios.md#iv_q_demo_maquinas) | `IV-2` | 19 | 109 |
| [`IV_Q_DEMO_TRATOR`](IV-2-formularios.md#iv_q_demo_trator) | `IV-2` | 26 | 43 |
| [`IV_Q_DEVOLUCAO_PECA`](IV-2-formularios.md#iv_q_devolucao_peca) | `IV-2` | 2 | 581 |
| [`IV_Q_DEVOLUCAO_PUK`](IV-2-formularios.md#iv_q_devolucao_puk) | `IV-2` | 2 | 5 |
| [`IV_Q_DOC_ANALISE_CREDITO`](IV-2-formularios.md#iv_q_doc_analise_credito) | `IV-2` | 28 | 21 |
| [`IV_Q_EVENTOS_AFERICAO`](IV-2-formularios.md#iv_q_eventos_afericao) | `IV-2` | 12 | 67 |
| [`IV_Q_EXP_FLUXO_MODELER`](IV-2-formularios.md#iv_q_exp_fluxo_modeler) | `IV-2` | 3 | 0 |
| [`IV_Q_FORA_SERVICO_PMP`](IV-2-formularios.md#iv_q_fora_servico_pmp) | `IV-2` | 5 | 0 |
| [`IV_Q_FORM_TREINO`](IV-2-formularios.md#iv_q_form_treino) | `IV-2` | 1 | 0 |
| [`IV_Q_GAR_DATA_SERVICO`](IV-2-formularios.md#iv_q_gar_data_servico) | `IV-2` | 3 | 4.888 |
| [`IV_Q_GAR_FAB_SOL_PECA`](IV-2-formularios.md#iv_q_gar_fab_sol_peca) | `IV-2` | 2 | 640 |
| [`IV_Q_GESTAO_CREDITO`](IV-2-formularios.md#iv_q_gestao_credito) | `IV-2` | 54 | 1.506 |
| [`IV_Q_GESTAO_CREDITO_AMS`](IV-2-formularios.md#iv_q_gestao_credito_ams) | `IV-2` | 51 | 175 |
| [`IV_Q_GESTAO_CREDITO_IMP`](IV-2-formularios.md#iv_q_gestao_credito_imp) | `IV-2` | 51 | 807 |
| [`IV_Q_GESTAO_PRODUTO_IMPL`](IV-2-formularios.md#iv_q_gestao_produto_impl) | `IV-2` | 23 | 813 |
| [`IV_Q_GESTAO_PRODUTO___AMS`](IV-2-formularios.md#iv_q_gestao_produto___ams) | `IV-2` | 39 | 177 |
| [`IV_Q_HORIMETRO_AGREGA`](IV-2-formularios.md#iv_q_horimetro_agrega) | `IV-2` | 2 | 1 |
| [`IV_Q_INCENTIVO`](IV-2-formularios.md#iv_q_incentivo) | `IV-2` | 43 | 2.079 |
| [`IV_Q_INTERESSE_FUTURO_PRO`](IV-2-formularios.md#iv_q_interesse_futuro_pro) | `IV-2` | 5 | 10 |
| [`IV_Q_INTERESSE_PROJETO_IR`](IV-2-formularios.md#iv_q_interesse_projeto_ir) | `IV-2` | 9 | 25 |
| [`IV_Q_LIBERAR_DEMONSTRACAO`](IV-2-formularios.md#iv_q_liberar_demonstracao) | `IV-2` | 5 | 44 |
| [`IV_Q_LICENCAS_PUK`](IV-2-formularios.md#iv_q_licencas_puk) | `IV-2` | 12 | 41 |
| [`IV_Q_LOCACAO_COMISSAO`](IV-2-formularios.md#iv_q_locacao_comissao) | `IV-2` | 13 | 15 |
| [`IV_Q_OFERECE_RENOV_SEGURO`](IV-2-formularios.md#iv_q_oferece_renov_seguro) | `IV-2` | 16 | 846 |
| [`IV_Q_ORIGEM_DA_RENDA`](IV-2-formularios.md#iv_q_origem_da_renda) | `IV-2` | 2 | 0 |
| [`IV_Q_OS_ABERTA`](IV-2-formularios.md#iv_q_os_aberta) | `IV-2` | 3 | 771 |
| [`IV_Q_OS_CORTESIA`](IV-2-formularios.md#iv_q_os_cortesia) | `IV-2` | 5 | 78 |
| [`IV_Q_OS_GARANTIA`](IV-2-formularios.md#iv_q_os_garantia) | `IV-2` | 5 | 7.786 |
| [`IV_Q_OS_REVISAO_ENTREGA`](IV-2-formularios.md#iv_q_os_revisao_entrega) | `IV-2` | 5 | 0 |
| [`IV_Q_PECAS_AFERICAO`](IV-2-formularios.md#iv_q_pecas_afericao) | `IV-2` | 23 | 5.174 |
| [`IV_Q_PEDIDO_GC`](IV-2-formularios.md#iv_q_pedido_gc) | `IV-2` | 12 | 0 |
| [`IV_Q_PEDIDO_KAM`](IV-2-formularios.md#iv_q_pedido_kam) | `IV-2` | 28 | 12 |
| [`IV_Q_PEDIDO_SAM`](IV-2-formularios.md#iv_q_pedido_sam) | `IV-2` | 19 | 46 |
| [`IV_Q_PERCEPCAO_JD`](IV-2-formularios.md#iv_q_percepcao_jd) | `IV-2` | 17 | 32 |
| [`IV_Q_PESQUISA_NPS`](IV-2-formularios.md#iv_q_pesquisa_nps) | `IV-2` | 2 | 0 |
| [`IV_Q_PESQUISA_TI`](IV-2-formularios.md#iv_q_pesquisa_ti) | `IV-2` | 4 | 0 |
| [`IV_Q_PREMIO_DEMO`](IV-2-formularios.md#iv_q_premio_demo) | `IV-2` | 4 | 21 |
| [`IV_Q_PREVISAO_RECEBIMENTO`](IV-2-formularios.md#iv_q_previsao_recebimento) | `IV-2` | 2 | 4 |
| [`IV_Q_PRODUTO_RD`](IV-2-formularios.md#iv_q_produto_rd) | `IV-2` | 8 | 135 |
| [`IV_Q_PROPOSTA_COMERCIAL`](IV-2-formularios.md#iv_q_proposta_comercial) | `IV-2` | 10 | 24 |
| [`IV_Q_PROSPECCAO_SERV__JD`](IV-2-formularios.md#iv_q_prospeccao_serv__jd) | `IV-2` | 6 | 104 |
| [`IV_Q_QUALIDADE_PECAS`](IV-2-formularios.md#iv_q_qualidade_pecas) | `IV-2` | 6 | 642 |
| [`IV_Q_QUALIDADE_SERVICOS`](IV-2-formularios.md#iv_q_qualidade_servicos) | `IV-2` | 6 | 345 |
| [`IV_Q_QUALIDADE_VENDAMAQ`](IV-2-formularios.md#iv_q_qualidade_vendamaq) | `IV-2` | 5 | 141 |
| [`IV_Q_RECEBIMENTO_A_PRAZO`](IV-2-formularios.md#iv_q_recebimento_a_prazo) | `IV-2` | 17 | 16 |
| [`IV_Q_RECEBIMENTO_COMISSAO`](IV-2-formularios.md#iv_q_recebimento_comissao) | `IV-2` | 3 | 0 |
| [`IV_Q_RECEBIMENTO_FINANC`](IV-2-formularios.md#iv_q_recebimento_financ) | `IV-2` | 2 | 4 |
| [`IV_Q_RECEB_FINAN_IMPLEM`](IV-2-formularios.md#iv_q_receb_finan_implem) | `IV-2` | 2 | 0 |
| [`IV_Q_RESPONSAVEL_TECNICO`](IV-2-formularios.md#iv_q_responsavel_tecnico) | `IV-2` | 2 | 2.115 |
| [`IV_Q_RESULTADO_DEMO`](IV-2-formularios.md#iv_q_resultado_demo) | `IV-2` | 15 | 23 |
| [`IV_Q_RETORNADO_JD`](IV-2-formularios.md#iv_q_retornado_jd) | `IV-2` | 2 | 773 |
| [`IV_Q_REVISAO_100H`](IV-2-formularios.md#iv_q_revisao_100h) | `IV-2` | 3 | 1 |
| [`IV_Q_REVISAO_1100_1150H`](IV-2-formularios.md#iv_q_revisao_1100_1150h) | `IV-2` | 3 | 1 |
| [`IV_Q_REVISAO_1500H`](IV-2-formularios.md#iv_q_revisao_1500h) | `IV-2` | 3 | 0 |
| [`IV_Q_REVISAO_450_600H`](IV-2-formularios.md#iv_q_revisao_450_600h) | `IV-2` | 3 | 1 |
| [`IV_Q_REVISAO_800H`](IV-2-formularios.md#iv_q_revisao_800h) | `IV-2` | 3 | 0 |
| [`IV_Q_REVISAO_FIM_GARANTIA`](IV-2-formularios.md#iv_q_revisao_fim_garantia) | `IV-2` | 3 | 0 |
| [`IV_Q_REV_DATA_SERVICO`](IV-2-formularios.md#iv_q_rev_data_servico) | `IV-2` | 3 | 74 |
| [`IV_Q_ROMANEIO_DEV_PECA`](IV-2-formularios.md#iv_q_romaneio_dev_peca) | `IV-2` | 4 | 1 |
| [`IV_Q_SEPARACAO_PEDIDO`](IV-2-formularios.md#iv_q_separacao_pedido) | `IV-2` | 18 | 2 |
| [`IV_Q_SERVICOS_AFERICAO`](IV-2-formularios.md#iv_q_servicos_afericao) | `IV-2` | 18 | 2.204 |
| [`IV_Q_SERVICO_EXTERNOS_JD`](IV-2-formularios.md#iv_q_servico_externos_jd) | `IV-2` | 9 | 1.662 |
| [`IV_Q_SOLICITACAO_TCAT`](IV-2-formularios.md#iv_q_solicitacao_tcat) | `IV-2` | 22 | 1 |
| [`IV_Q_TESTE1`](IV-2-formularios.md#iv_q_teste1) | `IV-2` | 3 | 2 |
| [`IV_Q_TESTE2`](IV-2-formularios.md#iv_q_teste2) | `IV-2` | 6 | 1 |
| [`IV_Q_TESTE_PRIMEIRO_JD`](IV-2-formularios.md#iv_q_teste_primeiro_jd) | `IV-2` | 3 | 3 |
| [`IV_Q_TICKET_DSI`](IV-2-formularios.md#iv_q_ticket_dsi) | `IV-2` | 8 | 1 |
| [`IV_Q_VENDA`](IV-2-formularios.md#iv_q_venda) | `IV-2` | 22 | 2.806 |
| [`IV_Q_VENDAPERDIDA_SEGURO`](IV-2-formularios.md#iv_q_vendaperdida_seguro) | `IV-2` | 4 | 12 |
| [`IV_Q_VENDA_AMS`](IV-2-formularios.md#iv_q_venda_ams) | `IV-2` | 65 | 775 |
| [`IV_Q_VENDA_CONSORCIO`](IV-2-formularios.md#iv_q_venda_consorcio) | `IV-2` | 7 | 47 |
| [`IV_Q_VENDA_DIRETA`](IV-2-formularios.md#iv_q_venda_direta) | `IV-2` | 12 | 0 |
| [`IV_Q_VENDA_DSI`](IV-2-formularios.md#iv_q_venda_dsi) | `IV-2` | 16 | 1 |
| [`IV_Q_VENDA_EQUIPAMENTO`](IV-2-formularios.md#iv_q_venda_equipamento) | `IV-2` | 49 | 2.256 |
| [`IV_Q_VENDA_MAQUINA_FY25`](IV-2-formularios.md#iv_q_venda_maquina_fy25) | `IV-2` | 45 | 0 |
| [`IV_Q_VENDA_PERDIDA`](IV-2-formularios.md#iv_q_venda_perdida) | `IV-2` | 14 | 1.511 |
| [`IV_Q_VENDA_PERDIDA_FY25`](IV-2-formularios.md#iv_q_venda_perdida_fy25) | `IV-2` | 13 | 198 |
| [`IV_Q_VENDA_PERDIDA_IMPL`](IV-2-formularios.md#iv_q_venda_perdida_impl) | `IV-2` | 10 | 31 |
| [`IV_Q_VENDA_PERDIDA_IMPLEM`](IV-2-formularios.md#iv_q_venda_perdida_implem) | `IV-2` | 13 | 125 |
| [`IV_Q_VENDA_PERDIDA_JDE`](IV-2-formularios.md#iv_q_venda_perdida_jde) | `IV-2` | 11 | 296 |
| [`IV_Q_VENDA_PERDIDA_MANITO`](IV-2-formularios.md#iv_q_venda_perdida_manito) | `IV-2` | 10 | 6 |
| [`IV_Q_VENDA_PERDIDA_MAQIMP`](IV-2-formularios.md#iv_q_venda_perdida_maqimp) | `IV-2` | 12 | 313 |
| [`IV_Q_VENDA_PERDIDA_PROD`](IV-2-formularios.md#iv_q_venda_perdida_prod) | `IV-2` | 15 | 286 |
| [`IV_Q_VENDA_PERDIDA_SEGURO`](IV-2-formularios.md#iv_q_venda_perdida_seguro) | `IV-2` | 13 | 13 |
| [`IV_Q_VENDA_PERDIDA_TESTE`](IV-2-formularios.md#iv_q_venda_perdida_teste) | `IV-2` | 12 | 0 |
| [`IV_Q_VENDA_PNEUS`](IV-2-formularios.md#iv_q_venda_pneus) | `IV-2` | 7 | 9 |
| [`IV_Q_VENDA_PRECISION_UP`](IV-2-formularios.md#iv_q_venda_precision_up) | `IV-2` | 8 | 2 |
| [`IV_Q_VENDA_SEMINOVO`](IV-2-formularios.md#iv_q_venda_seminovo) | `IV-2` | 10 | 2.481 |
| [`IV_Q_VENDA_SERVICOS_AMS`](IV-2-formularios.md#iv_q_venda_servicos_ams) | `IV-2` | 8 | 468 |
| [`IV_Q_VENDA_VP_PNEUS`](IV-2-formularios.md#iv_q_venda_vp_pneus) | `IV-2` | 6 | 4 |
| [`IV_Q_VENDER_RENOVACAO_SEG`](IV-2-formularios.md#iv_q_vender_renovacao_seg) | `IV-2` | 16 | 153 |
| [`IV_Q_VISITA_DSI`](IV-2-formularios.md#iv_q_visita_dsi) | `IV-2` | 4 | 4 |
| [`IV_Q_VISITA_EXP_CLIENTE`](IV-2-formularios.md#iv_q_visita_exp_cliente) | `IV-2` | 11 | 16 |
| [`IV_Q_VP_COLHEDORA`](IV-2-formularios.md#iv_q_vp_colhedora) | `IV-2` | 11 | 1 |
| [`IV_Q_VP_COLHEITADEIRA`](IV-2-formularios.md#iv_q_vp_colheitadeira) | `IV-2` | 12 | 3 |
| [`IV_Q_VP_PLANTADEIRA`](IV-2-formularios.md#iv_q_vp_plantadeira) | `IV-2` | 13 | 3 |
| [`IV_Q_VP_PULVERIZADOR`](IV-2-formularios.md#iv_q_vp_pulverizador) | `IV-2` | 10 | 0 |
| [`IV_Q_VP_RENOVACAO_SEGURO`](IV-2-formularios.md#iv_q_vp_renovacao_seguro) | `IV-2` | 4 | 0 |
| [`IV_Q_VP_SEM_PARTICIPACAO`](IV-2-formularios.md#iv_q_vp_sem_participacao) | `IV-2` | 12 | 74 |
| [`IV_Q_VP_TRATOR`](IV-2-formularios.md#iv_q_vp_trator) | `IV-2` | 12 | 19 |

</details>

### Classe `vazia` - 235 tabelas

0 linhas no snapshot: recurso do produto nao usado na Tracbel, ou tabela nova/abandonada.

<details><summary>Lista completa das 235 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`CBR_CLIENTECONTA`](CBR.md#cbr_clienteconta) | `CBR` | 7 | 0 |
| [`CLARITY_BINA`](CLARITY.md#clarity_bina) | `CLARITY` | 5 | 0 |
| [`DMN_DocArq`](DMN.md#dmn_docarq) | `DMN` | 5 | 0 |
| [`DMN_DocObs`](DMN.md#dmn_docobs) | `DMN` | 2 | 0 |
| [`DMN_DocProj`](DMN.md#dmn_docproj) | `DMN` | 4 | 0 |
| [`DMN_DocProp`](DMN.md#dmn_docprop) | `DMN` | 4 | 0 |
| [`DMN_DocVrs`](DMN.md#dmn_docvrs) | `DMN` | 7 | 0 |
| [`EXT_EMAIL`](EXT.md#ext_email) | `EXT` | 15 | 0 |
| [`EXT_FormaPgto`](EXT.md#ext_formapgto) | `EXT` | 6 | 0 |
| [`EXT_NFSCmpl`](EXT.md#ext_nfscmpl) | `EXT` | 5 | 0 |
| [`EXT_OSSolic`](EXT.md#ext_ossolic) | `EXT` | 6 | 0 |
| [`EXT_Pedido`](EXT.md#ext_pedido) | `EXT` | 57 | 0 |
| [`EXT_PedidoItem`](EXT.md#ext_pedidoitem) | `EXT` | 22 | 0 |
| [`EXT_PESSOACONTATO`](EXT.md#ext_pessoacontato) | `EXT` | 29 | 0 |
| [`EXT_PESSOAFONE`](EXT.md#ext_pessoafone) | `EXT` | 20 | 0 |
| [`EXT_TituloCmpl`](EXT.md#ext_titulocmpl) | `EXT` | 2 | 0 |
| [`EXT_VeicAgd`](EXT.md#ext_veicagd) | `EXT` | 28 | 0 |
| [`EXT_VEICAGDERP`](EXT.md#ext_veicagderp) | `EXT` | 12 | 0 |
| [`EXT_VeicAvalFoto`](EXT.md#ext_veicavalfoto) | `EXT` | 3 | 0 |
| [`EXT_VeicAvalia`](EXT.md#ext_veicavalia) | `EXT` | 31 | 0 |
| [`EXT_VeicPlanoMnFX`](EXT.md#ext_veicplanomnfx) | `EXT` | 7 | 0 |
| [`GEL_CEP`](GEL.md#gel_cep) | `GEL` | 14 | 0 |
| [`GEL_CepAlerta`](GEL.md#gel_cepalerta) | `GEL` | 4 | 0 |
| [`GEL_CepOrig`](GEL.md#gel_ceporig) | `GEL` | 10 | 0 |
| [`GEL_Cidade`](GEL.md#gel_cidade) | `GEL` | 12 | 0 |
| [`GEL_Conv`](GEL.md#gel_conv) | `GEL` | 2 | 0 |
| [`GEL_ConvDe`](GEL.md#gel_convde) | `GEL` | 4 | 0 |
| [`GEP_CidadeEmprDest`](GEP.md#gep_cidadeemprdest) | `GEP` | 3 | 0 |
| [`GEP_EMailSend`](GEP.md#gep_emailsend) | `GEP` | 21 | 0 |
| [`GEP_EMAILSENDREL`](GEP.md#gep_emailsendrel) | `GEP` | 4 | 0 |
| [`GEP_EMAILSENTREL`](GEP.md#gep_emailsentrel) | `GEP` | 4 | 0 |
| [`GEP_ImportErro`](GEP.md#gep_importerro) | `GEP` | 4 | 0 |
| [`GEP_JOBNOTIFICAR`](GEP.md#gep_jobnotificar) | `GEP` | 14 | 0 |
| [`GEP_Notificar`](GEP.md#gep_notificar) | `GEP` | 8 | 0 |
| [`GEP_PABX`](GEP.md#gep_pabx) | `GEP` | 6 | 0 |
| [`GEP_ParRecEnvia`](GEP.md#gep_parrecenvia) | `GEP` | 2 | 0 |
| [`GEP_ProcControle`](GEP.md#gep_proccontrole) | `GEP` | 5 | 0 |
| [`GEP_ProcImport`](GEP.md#gep_procimport) | `GEP` | 9 | 0 |
| [`Gep_UsrPabx`](GEP.md#gep_usrpabx) | `GEP` | 5 | 0 |
| [`GE_CampoExig`](GE.md#ge_campoexig) | `GE` | 7 | 0 |
| [`GE_CFinConj`](GE.md#ge_cfinconj) | `GE` | 22 | 0 |
| [`GE_CFinDocto`](GE.md#ge_cfindocto) | `GE` | 7 | 0 |
| [`GE_CFinFis`](GE.md#ge_cfinfis) | `GE` | 51 | 0 |
| [`GE_CFinRPes`](GE.md#ge_cfinrpes) | `GE` | 12 | 0 |
| [`GE_CIdadePref`](GE.md#ge_cidadepref) | `GE` | 3 | 0 |
| [`GE_ColPosition`](GE.md#ge_colposition) | `GE` | 5 | 0 |
| [`GE_ColRegra`](GE.md#ge_colregra) | `GE` | 8 | 0 |
| [`GE_CONSHST`](GE.md#ge_conshst) | `GE` | 5 | 0 |
| [`GE_CONSVAR`](GE.md#ge_consvar) | `GE` | 10 | 0 |
| [`GE_CONSVARLST`](GE.md#ge_consvarlst) | `GE` | 3 | 0 |
| [`GE_CONTATOAPP`](GE.md#ge_contatoapp) | `GE` | 12 | 0 |
| [`GE_ContatoEmail`](GE.md#ge_contatoemail) | `GE` | 5 | 0 |
| [`GE_CTRLMU`](GE.md#ge_ctrlmu) | `GE` | 6 | 0 |
| [`GE_DiaNaoUtil`](GE.md#ge_dianaoutil) | `GE` | 5 | 0 |
| [`GE_EMAILINVALIDO`](GE.md#ge_emailinvalido) | `GE` | 3 | 0 |
| [`GE_EmailSpam`](GE.md#ge_emailspam) | `GE` | 4 | 0 |
| [`GE_FCadConj`](GE.md#ge_fcadconj) | `GE` | 43 | 0 |
| [`GE_FCadF`](GE.md#ge_fcadf) | `GE` | 97 | 0 |
| [`GE_FCadInstCred`](GE.md#ge_fcadinstcred) | `GE` | 13 | 0 |
| [`GE_FcadRBanc`](GE.md#ge_fcadrbanc) | `GE` | 18 | 0 |
| [`GE_FcadRCom`](GE.md#ge_fcadrcom) | `GE` | 20 | 0 |
| [`GE_FCadRPes`](GE.md#ge_fcadrpes) | `GE` | 33 | 0 |
| [`GE_FERIADO`](GE.md#ge_feriado) | `GE` | 7 | 0 |
| [`GE_FONEINVALIDO`](GE.md#ge_foneinvalido) | `GE` | 3 | 0 |
| [`GE_Grafico`](GE.md#ge_grafico) | `GE` | 26 | 0 |
| [`GE_Help`](GE.md#ge_help) | `GE` | 3 | 0 |
| [`GE_IntCtrl`](GE.md#ge_intctrl) | `GE` | 14 | 0 |
| [`GE_LogAtividade`](GE.md#ge_logatividade) | `GE` | 10 | 0 |
| [`GE_LOG_CARTCRED`](GE.md#ge_log_cartcred) | `GE` | 10 | 0 |
| [`GE_MobileConfig`](GE.md#ge_mobileconfig) | `GE` | 14 | 0 |
| [`GE_MOBILEPROP`](GE.md#ge_mobileprop) | `GE` | 2 | 0 |
| [`GE_PesDelVinc`](GE.md#ge_pesdelvinc) | `GE` | 7 | 0 |
| [`GE_PessoaAlt`](GE.md#ge_pessoaalt) | `GE` | 68 | 0 |
| [`GE_PESSOAATIVAUSR`](GE.md#ge_pessoaativausr) | `GE` | 5 | 0 |
| [`GE_PessoaDel`](GE.md#ge_pessoadel) | `GE` | 76 | 0 |
| [`GE_PessoaDestino`](GE.md#ge_pessoadestino) | `GE` | 5 | 0 |
| [`GE_PessoaEmail`](GE.md#ge_pessoaemail) | `GE` | 8 | 0 |
| [`GE_PessoaEndAlt`](GE.md#ge_pessoaendalt) | `GE` | 21 | 0 |
| [`GE_PessoaJur`](GE.md#ge_pessoajur) | `GE` | 15 | 0 |
| [`GE_PessoaPasw`](GE.md#ge_pessoapasw) | `GE` | 9 | 0 |
| [`GE_PESSOAREDESOCIAL`](GE.md#ge_pessoaredesocial) | `GE` | 10 | 0 |
| [`GE_PessoaUnidade`](GE.md#ge_pessoaunidade) | `GE` | 3 | 0 |
| [`GE_PROCESSOWEB`](GE.md#ge_processoweb) | `GE` | 5 | 0 |
| [`GE_Regiao`](GE.md#ge_regiao) | `GE` | 6 | 0 |
| [`GE_RelPasta`](GE.md#ge_relpasta) | `GE` | 3 | 0 |
| [`GE_RelPastaCons`](GE.md#ge_relpastacons) | `GE` | 2 | 0 |
| [`GE_RELTEMPLATE`](GE.md#ge_reltemplate) | `GE` | 6 | 0 |
| [`GE_Rota`](GE.md#ge_rota) | `GE` | 6 | 0 |
| [`GE_TabRegra`](GE.md#ge_tabregra) | `GE` | 5 | 0 |
| [`GE_URACENARIO`](GE.md#ge_uracenario) | `GE` | 15 | 0 |
| [`GE_UsrAcSp`](GE.md#ge_usracsp) | `GE` | 7 | 0 |
| [`GE_UsrCtrl`](GE.md#ge_usrctrl) | `GE` | 7 | 0 |
| [`GE_USUARIOCMPL`](GE.md#ge_usuariocmpl) | `GE` | 5 | 0 |
| [`GE_UsuarioLink`](GE.md#ge_usuariolink) | `GE` | 5 | 0 |
| [`IVC_ATENDENTE`](IVC.md#ivc_atendente) | `IVC` | 18 | 0 |
| [`IVC_ATENDENTELOG`](IVC.md#ivc_atendentelog) | `IVC` | 6 | 0 |
| [`IVC_CHAMADALOG`](IVC.md#ivc_chamadalog) | `IVC` | 22 | 0 |
| [`IVC_EQUIPE`](IVC.md#ivc_equipe) | `IVC` | 3 | 0 |
| [`IVC_EQUIPEUSR`](IVC.md#ivc_equipeusr) | `IVC` | 3 | 0 |
| [`IVC_RAMAL`](IVC.md#ivc_ramal) | `IVC` | 5 | 0 |
| [`IVF_Acordo`](IVF.md#ivf_acordo) | `IVF` | 10 | 0 |
| [`IVF_Agregado`](IVF.md#ivf_agregado) | `IVF` | 8 | 0 |
| [`IVF_AgregCC`](IVF.md#ivf_agregcc) | `IVF` | 8 | 0 |
| [`IVF_Financeira`](IVF.md#ivf_financeira) | `IVF` | 23 | 0 |
| [`IVF_FinancEmpr`](IVF.md#ivf_financempr) | `IVF` | 4 | 0 |
| [`IVF_FinancImpTx`](IVF.md#ivf_financimptx) | `IVF` | 6 | 0 |
| [`IVF_Molicar`](IVF.md#ivf_molicar) | `IVF` | 25 | 0 |
| [`IVF_Plano`](IVF.md#ivf_plano) | `IVF` | 19 | 0 |
| [`IVF_PlanoIndic`](IVF.md#ivf_planoindic) | `IVF` | 7 | 0 |
| [`IVF_Prazo`](IVF.md#ivf_prazo) | `IVF` | 3 | 0 |
| [`IVF_Proposta`](IVF.md#ivf_proposta) | `IVF` | 60 | 0 |
| [`IVF_PropostaHst`](IVF.md#ivf_propostahst) | `IVF` | 7 | 0 |
| [`IVF_PropostaObs`](IVF.md#ivf_propostaobs) | `IVF` | 6 | 0 |
| [`IVF_PropostaResult`](IVF.md#ivf_propostaresult) | `IVF` | 5 | 0 |
| [`IVF_Tabela`](IVF.md#ivf_tabela) | `IVF` | 24 | 0 |
| [`IVF_TabelaFiltro`](IVF.md#ivf_tabelafiltro) | `IVF` | 4 | 0 |
| [`IVF_TabEmpr`](IVF.md#ivf_tabempr) | `IVF` | 4 | 0 |
| [`IVF_TabIndice`](IVF.md#ivf_tabindice) | `IVF` | 6 | 0 |
| [`IVF_TipoAgregado`](IVF.md#ivf_tipoagregado) | `IVF` | 6 | 0 |
| [`IVF_TpAgrPessoa`](IVF.md#ivf_tpagrpessoa) | `IVF` | 5 | 0 |
| [`IVM_Material`](IVM.md#ivm_material) | `IVM` | 15 | 0 |
| [`IVM_MatPessoa`](IVM.md#ivm_matpessoa) | `IVM` | 9 | 0 |
| [`IVM_ProcMat`](IVM.md#ivm_procmat) | `IVM` | 11 | 0 |
| [`IVM_ProcMatItem`](IVM.md#ivm_procmatitem) | `IVM` | 14 | 0 |
| [`IVP_PedCritica`](IVP.md#ivp_pedcritica) | `IVP` | 9 | 0 |
| [`IVP_ProdImagem`](IVP.md#ivp_prodimagem) | `IVP` | 4 | 0 |
| [`IVP_TabPreco`](IVP.md#ivp_tabpreco) | `IVP` | 7 | 0 |
| [`IVP_Vendedor`](IVP.md#ivp_vendedor) | `IVP` | 11 | 0 |
| [`IVS_CanalVenda`](IVS.md#ivs_canalvenda) | `IVS` | 7 | 0 |
| [`IVS_CartCategoria`](IVS.md#ivs_cartcategoria) | `IVS` | 2 | 0 |
| [`IVS_Categoria`](IVS.md#ivs_categoria) | `IVS` | 3 | 0 |
| [`IVS_DeptoDePara`](IVS.md#ivs_deptodepara) | `IVS` | 2 | 0 |
| [`IVS_NegCategoria`](IVS.md#ivs_negcategoria) | `IVS` | 2 | 0 |
| [`IVS_Negocio`](IVS.md#ivs_negocio) | `IVS` | 9 | 0 |
| [`IVS_Regional`](IVS.md#ivs_regional) | `IVS` | 8 | 0 |
| [`IV_ACAOANEXA`](IV-3-catalogos-bpm.md#iv_acaoanexa) | `IV-3` | 11 | 0 |
| [`IV_AcaoAtendente`](IV-3-catalogos-bpm.md#iv_acaoatendente) | `IV-3` | 6 | 0 |
| [`IV_AcaoAutoCtrl`](IV-3-catalogos-bpm.md#iv_acaoautoctrl) | `IV-3` | 6 | 0 |
| [`IV_AcaoCmpl`](IV-3-catalogos-bpm.md#iv_acaocmpl) | `IV-3` | 2 | 0 |
| [`IV_AcaoMon`](IV-3-catalogos-bpm.md#iv_acaomon) | `IV-3` | 9 | 0 |
| [`IV_ACAOURACENARIO`](IV-3-catalogos-bpm.md#iv_acaouracenario) | `IV-3` | 2 | 0 |
| [`IV_AGDPLANACAO`](IV-1-processo-agenda-historico.md#iv_agdplanacao) | `IV-1` | 7 | 0 |
| [`IV_AGDPLANTRG`](IV-1-processo-agenda-historico.md#iv_agdplantrg) | `IV-1` | 42 | 0 |
| [`IV_AgdRec`](IV-1-processo-agenda-historico.md#iv_agdrec) | `IV-1` | 9 | 0 |
| [`IV_AgdUsr`](IV-1-processo-agenda-historico.md#iv_agdusr) | `IV-1` | 4 | 0 |
| [`IV_AGENDACMPL`](IV-1-processo-agenda-historico.md#iv_agendacmpl) | `IV-1` | 4 | 0 |
| [`IV_AGENDAITEM`](IV-1-processo-agenda-historico.md#iv_agendaitem) | `IV-1` | 7 | 0 |
| [`IV_AtdBloq`](IV-1-processo-agenda-historico.md#iv_atdbloq) | `IV-1` | 10 | 0 |
| [`IV_Atendente`](IV-4-demais.md#iv_atendente) | `IV-4` | 2 | 0 |
| [`IV_Ativ`](IV-4-demais.md#iv_ativ) | `IV-4` | 17 | 0 |
| [`IV_AtivAgenda`](IV-1-processo-agenda-historico.md#iv_ativagenda) | `IV-1` | 6 | 0 |
| [`IV_ATIVGRUPO`](IV-4-demais.md#iv_ativgrupo) | `IV-4` | 4 | 0 |
| [`IV_ATIVMODELO`](IV-4-demais.md#iv_ativmodelo) | `IV-4` | 3 | 0 |
| [`IV_ATIVPADRAO`](IV-4-demais.md#iv_ativpadrao) | `IV-4` | 8 | 0 |
| [`IV_AtivProc`](IV-1-processo-agenda-historico.md#iv_ativproc) | `IV-1` | 7 | 0 |
| [`IV_BaseInformacao`](IV-4-demais.md#iv_baseinformacao) | `IV-4` | 8 | 0 |
| [`IV_BonusCC`](IV-4-demais.md#iv_bonuscc) | `IV-4` | 8 | 0 |
| [`IV_CampPesMsg`](IV-4-demais.md#iv_camppesmsg) | `IV-4` | 22 | 0 |
| [`IV_CampPessoa`](IV-4-demais.md#iv_camppessoa) | `IV-4` | 5 | 0 |
| [`IV_CAMPSELECAO`](IV-4-demais.md#iv_campselecao) | `IV-4` | 11 | 0 |
| [`IV_CampVoucher`](IV-4-demais.md#iv_campvoucher) | `IV-4` | 9 | 0 |
| [`IV_CartContratante`](IV-4-demais.md#iv_cartcontratante) | `IV-4` | 14 | 0 |
| [`IV_CartCred`](IV-4-demais.md#iv_cartcred) | `IV-4` | 36 | 0 |
| [`IV_CartLoteCartao`](IV-4-demais.md#iv_cartlotecartao) | `IV-4` | 5 | 0 |
| [`IV_CartLoteImp`](IV-4-demais.md#iv_cartloteimp) | `IV-4` | 7 | 0 |
| [`IV_CartMarca`](IV-4-demais.md#iv_cartmarca) | `IV-4` | 5 | 0 |
| [`IV_CARTPESORIGEM`](IV-4-demais.md#iv_cartpesorigem) | `IV-4` | 4 | 0 |
| [`IV_CARTPESSOA`](IV-4-demais.md#iv_cartpessoa) | `IV-4` | 53 | 0 |
| [`IV_CartProduto`](IV-4-demais.md#iv_cartproduto) | `IV-4` | 9 | 0 |
| [`IV_CartProposta`](IV-4-demais.md#iv_cartproposta) | `IV-4` | 30 | 0 |
| [`IV_CartTitular`](IV-4-demais.md#iv_carttitular) | `IV-4` | 12 | 0 |
| [`IV_CBRCOBRANCAHST`](IV-4-demais.md#iv_cbrcobrancahst) | `IV-4` | 5 | 0 |
| [`IV_CbrCobrancaMon`](IV-4-demais.md#iv_cbrcobrancamon) | `IV-4` | 11 | 0 |
| [`IV_CbrTitulo`](IV-4-demais.md#iv_cbrtitulo) | `IV-4` | 5 | 0 |
| [`IV_CFinDocAceito`](IV-4-demais.md#iv_cfindocaceito) | `IV-4` | 4 | 0 |
| [`IV_ClientePropCmpl`](IV-2-formularios.md#iv_clientepropcmpl) | `IV-2` | 2 | 0 |
| [`IV_CobrCrit`](IV-4-demais.md#iv_cobrcrit) | `IV-4` | 11 | 0 |
| [`IV_CobrCritAgd`](IV-4-demais.md#iv_cobrcritagd) | `IV-4` | 5 | 0 |
| [`IV_CobrCritDef`](IV-4-demais.md#iv_cobrcritdef) | `IV-4` | 10 | 0 |
| [`IV_CobrCritMon`](IV-4-demais.md#iv_cobrcritmon) | `IV-4` | 17 | 0 |
| [`IV_CobrTit`](IV-4-demais.md#iv_cobrtit) | `IV-4` | 11 | 0 |
| [`IV_CustoMidia`](IV-4-demais.md#iv_customidia) | `IV-4` | 9 | 0 |
| [`IV_Departamento`](IV-3-catalogos-bpm.md#iv_departamento) | `IV-3` | 3 | 0 |
| [`IV_eMail`](IV-4-demais.md#iv_email) | `IV-4` | 15 | 0 |
| [`IV_EMAILESTATISTICA`](IV-4-demais.md#iv_emailestatistica) | `IV-4` | 20 | 0 |
| [`IV_ESTRPRODUTO`](IV-4-demais.md#iv_estrproduto) | `IV-4` | 6 | 0 |
| [`IV_EventoAcao`](IV-3-catalogos-bpm.md#iv_eventoacao) | `IV-3` | 7 | 0 |
| [`IV_FichaNegVeic`](IV-4-demais.md#iv_fichanegveic) | `IV-4` | 79 | 0 |
| [`IV_FoneCtrl`](IV-4-demais.md#iv_fonectrl) | `IV-4` | 6 | 0 |
| [`IV_FoneCtrl2`](IV-4-demais.md#iv_fonectrl2) | `IV-4` | 6 | 0 |
| [`IV_FoneCtrlHst`](IV-4-demais.md#iv_fonectrlhst) | `IV-4` | 7 | 0 |
| [`IV_HISTORICOTAG`](IV-1-processo-agenda-historico.md#iv_historicotag) | `IV-1` | 4 | 0 |
| [`IV_LEADFACEBOOK`](IV-4-demais.md#iv_leadfacebook) | `IV-4` | 21 | 0 |
| [`IV_LEADFACEITEM`](IV-4-demais.md#iv_leadfaceitem) | `IV-4` | 6 | 0 |
| [`IV_ObjVenda`](IV-4-demais.md#iv_objvenda) | `IV-4` | 13 | 0 |
| [`IV_OS`](IV-4-demais.md#iv_os) | `IV-4` | 22 | 0 |
| [`IV_PAREVTEXTRES`](IV-3-catalogos-bpm.md#iv_parevtextres) | `IV-3` | 7 | 0 |
| [`IV_Pessoa`](IV-4-demais.md#iv_pessoa) | `IV-4` | 6 | 0 |
| [`IV_PessoaStat`](IV-4-demais.md#iv_pessoastat) | `IV-4` | 3 | 0 |
| [`IV_PlanoAtiv`](IV-4-demais.md#iv_planoativ) | `IV-4` | 10 | 0 |
| [`IV_ProcAtiv`](IV-1-processo-agenda-historico.md#iv_procativ) | `IV-1` | 6 | 0 |
| [`IV_ProcComent`](IV-1-processo-agenda-historico.md#iv_proccoment) | `IV-1` | 3 | 0 |
| [`IV_PROCPESLINK`](IV-1-processo-agenda-historico.md#iv_procpeslink) | `IV-1` | 2 | 0 |
| [`IV_ProcProjeto`](IV-1-processo-agenda-historico.md#iv_procprojeto) | `IV-1` | 4 | 0 |
| [`IV_ProcRelacao`](IV-1-processo-agenda-historico.md#iv_procrelacao) | `IV-1` | 6 | 0 |
| [`IV_PROCTAG`](IV-1-processo-agenda-historico.md#iv_proctag) | `IV-1` | 4 | 0 |
| [`IV_ProcTpRel`](IV-3-catalogos-bpm.md#iv_proctprel) | `IV-3` | 5 | 0 |
| [`IV_ProjColec`](IV-4-demais.md#iv_projcolec) | `IV-4` | 4 | 0 |
| [`IV_ProjDocto`](IV-4-demais.md#iv_projdocto) | `IV-4` | 4 | 0 |
| [`IV_ProjEquipe`](IV-4-demais.md#iv_projequipe) | `IV-4` | 4 | 0 |
| [`IV_Projeto`](IV-4-demais.md#iv_projeto) | `IV-4` | 17 | 0 |
| [`IV_ProjPessoa`](IV-4-demais.md#iv_projpessoa) | `IV-4` | 4 | 0 |
| [`IV_PropriListaLk`](IV-2-formularios.md#iv_proprilistalk) | `IV-2` | 5 | 0 |
| [`IV_PUSH`](IV-4-demais.md#iv_push) | `IV-4` | 15 | 0 |
| [`IV_Recurso`](IV-3-catalogos-bpm.md#iv_recurso) | `IV-3` | 5 | 0 |
| [`IV_RecUso`](IV-3-catalogos-bpm.md#iv_recuso) | `IV-3` | 7 | 0 |
| [`IV_ResEvtOut`](IV-3-catalogos-bpm.md#iv_resevtout) | `IV-3` | 4 | 0 |
| [`IV_RESJOB`](IV-3-catalogos-bpm.md#iv_resjob) | `IV-3` | 2 | 0 |
| [`IV_ResParam`](IV-3-catalogos-bpm.md#iv_resparam) | `IV-3` | 37 | 0 |
| [`IV_ResultadoReq`](IV-3-catalogos-bpm.md#iv_resultadoreq) | `IV-3` | 6 | 0 |
| [`IV_ResultadoWeb`](IV-3-catalogos-bpm.md#iv_resultadoweb) | `IV-3` | 10 | 0 |
| [`IV_RetProcRegra`](IV-3-catalogos-bpm.md#iv_retprocregra) | `IV-3` | 7 | 0 |
| [`IV_SegPerfil`](IV-3-catalogos-bpm.md#iv_segperfil) | `IV-3` | 27 | 0 |
| [`IV_SELPROMOPRD`](IV-4-demais.md#iv_selpromoprd) | `IV-4` | 5 | 0 |
| [`IV_TAGCAD`](IV-3-catalogos-bpm.md#iv_tagcad) | `IV-3` | 9 | 0 |
| [`IV_TC_PESSOA`](IV-4-demais.md#iv_tc_pessoa) | `IV-4` | 13 | 0 |
| [`IV_TEMPLATEPROJ`](IV-4-demais.md#iv_templateproj) | `IV-4` | 7 | 0 |
| [`IV_TpPgto`](IV-3-catalogos-bpm.md#iv_tppgto) | `IV-3` | 5 | 0 |
| [`IV_TxtPadConta`](IV-3-catalogos-bpm.md#iv_txtpadconta) | `IV-3` | 5 | 0 |
| [`IV_Unidade`](IV-3-catalogos-bpm.md#iv_unidade) | `IV-3` | 10 | 0 |
| [`IV_URADISPARO`](IV-4-demais.md#iv_uradisparo) | `IV-4` | 12 | 0 |
| [`IV_URARELATORIO`](IV-4-demais.md#iv_urarelatorio) | `IV-4` | 16 | 0 |
| [`IV_USRPUSH`](IV-4-demais.md#iv_usrpush) | `IV-4` | 15 | 0 |
| [`IV_WHATSAPP`](IV-4-demais.md#iv_whatsapp) | `IV-4` | 21 | 0 |
| [`LOG_INTEGRACAO_FATURAMENTO_TOTVS`](LOG.md#log_integracao_faturamento_totvs) | `LOG` | 4 | 0 |

</details>

### Classe `isolada` - 112 tabelas

Tem linhas mas nenhuma FK entrando nem saindo: nao se conecta ao modelo por integridade declarada (pode se conectar por convencao de nome).

<details><summary>Lista completa das 112 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`DMN_DocHst`](DMN.md#dmn_dochst) | `DMN` | 6 | 183.667 |
| [`DMN_DocTp`](DMN.md#dmn_doctp) | `DMN` | 24 | 107 |
| [`EXT_AGUARDENTREGA`](EXT.md#ext_aguardentrega) | `EXT` | 3 | 50 |
| [`EXT_APROVACAO`](EXT.md#ext_aprovacao) | `EXT` | 3 | 2.224 |
| [`EXT_CONDPAGTO`](EXT.md#ext_condpagto) | `EXT` | 3 | 21 |
| [`EXT_OSITEM`](EXT.md#ext_ositem) | `EXT` | 15 | 182.238 |
| [`EXT_Pot_Pecas`](EXT.md#ext_pot_pecas) | `EXT` | 13 | 1.287 |
| [`EXT_TIT_ACRESC`](EXT.md#ext_tit_acresc) | `EXT` | 2 | 1.575 |
| [`EXT_VEICFAMREF`](EXT.md#ext_veicfamref) | `EXT` | 2 | 23 |
| [`EXT_VEICREF`](EXT.md#ext_veicref) | `EXT` | 3 | 40 |
| [`gep_excfrota`](GEP.md#gep_excfrota) | `GEP` | 7 | 1.180 |
| [`gep_excfrotarep`](GEP.md#gep_excfrotarep) | `GEP` | 11 | 425 |
| [`GEP_EXCFROTASEQ`](GEP.md#gep_excfrotaseq) | `GEP` | 1 | 1.128 |
| [`GEP_EXCFROTASEQDEL`](GEP.md#gep_excfrotaseqdel) | `GEP` | 1 | 1.082 |
| [`gep_excseqcar`](GEP.md#gep_excseqcar) | `GEP` | 1 | 3.143 |
| [`gep_excseqcarok`](GEP.md#gep_excseqcarok) | `GEP` | 1 | 1.668 |
| [`GEP_Fila`](GEP.md#gep_fila) | `GEP` | 18 | 4 |
| [`GEP_Import`](GEP.md#gep_import) | `GEP` | 15 | 344 |
| [`GEP_ImportAprovacao`](GEP.md#gep_importaprovacao) | `GEP` | 15 | 4.284 |
| [`GEP_ImportTry`](GEP.md#gep_importtry) | `GEP` | 4 | 1.870 |
| [`GEP_IMPORT_GUI`](GEP.md#gep_import_gui) | `GEP` | 15 | 52 |
| [`GEP_JobAgdExecLog`](GEP.md#gep_jobagdexeclog) | `GEP` | 13 | 981.319 |
| [`GEP_JOBFILALOG`](GEP.md#gep_jobfilalog) | `GEP` | 5 | 14.038 |
| [`GEP_JobMonitor`](GEP.md#gep_jobmonitor) | `GEP` | 10 | 1 |
| [`GEP_SyncUsrSat`](GEP.md#gep_syncusrsat) | `GEP` | 15 | 3.901.359 |
| [`GE_Alias`](GE.md#ge_alias) | `GE` | 9 | 1.413 |
| [`GE_Contato`](GE.md#ge_contato) | `GE` | 41 | 41.164 |
| [`GE_ContatoPapel`](GE.md#ge_contatopapel) | `GE` | 5 | 43.489 |
| [`GE_CTRL2`](GE.md#ge_ctrl2) | `GE` | 6 | 276 |
| [`GE_CtrlE`](GE.md#ge_ctrle) | `GE` | 9 | 16 |
| [`GE_CtrlM`](GE.md#ge_ctrlm) | `GE` | 11 | 31 |
| [`GE_CtrlME`](GE.md#ge_ctrlme) | `GE` | 6 | 36 |
| [`GE_CTRLUSR`](GE.md#ge_ctrlusr) | `GE` | 6 | 33 |
| [`GE_Email`](GE.md#ge_email) | `GE` | 16 | 38.765 |
| [`GE_Figura`](GE.md#ge_figura) | `GE` | 8 | 2.078 |
| [`GE_IMPORTA_CART`](GE.md#ge_importa_cart) | `GE` | 52 | 22 |
| [`GE_LgTb`](GE.md#ge_lgtb) | `GE` | 10 | 11.861.777 |
| [`GE_Log2`](GE.md#ge_log2) | `GE` | 14 | 4.394.779 |
| [`GE_LOG_CONFIG`](GE.md#ge_log_config) | `GE` | 10 | 108.492 |
| [`GE_LOG_CONTATO`](GE.md#ge_log_contato) | `GE` | 10 | 19.785 |
| [`GE_LOG_EXT`](GE.md#ge_log_ext) | `GE` | 10 | 622.075 |
| [`GE_LOG_HISTORICO`](GE.md#ge_log_historico) | `GE` | 10 | 1.015.045 |
| [`GE_LOG_PESSOA`](GE.md#ge_log_pessoa) | `GE` | 10 | 943.712 |
| [`GE_LOG_PROCESSO`](GE.md#ge_log_processo) | `GE` | 10 | 12.678.640 |
| [`GE_LOG_TRANS`](GE.md#ge_log_trans) | `GE` | 10 | 782.089 |
| [`GE_Mod`](GE.md#ge_mod) | `GE` | 9 | 32 |
| [`GE_ObjDinamico`](GE.md#ge_objdinamico) | `GE` | 29 | 193 |
| [`GE_ObjDinAplic`](GE.md#ge_objdinaplic) | `GE` | 4 | 122 |
| [`GE_ParametroGlobal`](GE.md#ge_parametroglobal) | `GE` | 8 | 236 |
| [`GE_PessoaAlerta`](GE.md#ge_pessoaalerta) | `GE` | 8 | 237 |
| [`ge_pessoaativa`](GE.md#ge_pessoaativa) | `GE` | 1 | 1.175 |
| [`GE_PessoaClasse`](GE.md#ge_pessoaclasse) | `GE` | 7 | 1.878 |
| [`GE_PessoaFis`](GE.md#ge_pessoafis) | `GE` | 32 | 5.136 |
| [`GE_PessoaFone`](GE.md#ge_pessoafone) | `GE` | 19 | 112.659 |
| [`GE_PessoaFonema`](GE.md#ge_pessoafonema) | `GE` | 3 | 455.993 |
| [`GE_PessoaLink`](GE.md#ge_pessoalink) | `GE` | 5 | 32.831 |
| [`GE_PessoaLinkbkp`](GE.md#ge_pessoalinkbkp) | `GE` | 5 | 58.741 |
| [`GE_PessoaMural`](GE.md#ge_pessoamural) | `GE` | 14 | 145.313 |
| [`GE_PessoaNomeFonema`](GE.md#ge_pessoanomefonema) | `GE` | 4 | 151.583 |
| [`GE_PessoaRelacao`](GE.md#ge_pessoarelacao) | `GE` | 9 | 5.595 |
| [`GE_PessoaVersao`](GE.md#ge_pessoaversao) | `GE` | 19 | 12.019 |
| [`GE_QVRegra`](GE.md#ge_qvregra) | `GE` | 7 | 234 |
| [`GE_Sequencia`](GE.md#ge_sequencia) | `GE` | 2 | 77 |
| [`GE_SyncParam`](GE.md#ge_syncparam) | `GE` | 10 | 635 |
| [`GE_TempLong`](GE.md#ge_templong) | `GE` | 3 | 6 |
| [`GE_TipoLogradouro`](GE.md#ge_tipologradouro) | `GE` | 3 | 124 |
| [`GE_UsuarioPerm`](GE.md#ge_usuarioperm) | `GE` | 5 | 38.601 |
| [`GE_UsuarioSenhaMem`](GE.md#ge_usuariosenhamem) | `GE` | 3 | 2.456 |
| [`ivs_callcenter`](IVS.md#ivs_callcenter) | `IVS` | 2 | 3.475 |
| [`ivs_callcidades`](IVS.md#ivs_callcidades) | `IVS` | 3 | 83 |
| [`IVS_CALL_FORAREGIAO`](IVS.md#ivs_call_foraregiao) | `IVS` | 1 | 2.360 |
| [`IVS_PES_MAQ_PECAS`](IVS.md#ivs_pes_maq_pecas) | `IVS` | 6 | 1.365 |
| [`IVS_PES_PNEUS`](IVS.md#ivs_pes_pneus) | `IVS` | 6 | 1.313 |
| [`IVS_Pes_RAO_Pneus_02_03`](IVS.md#ivs_pes_rao_pneus_02_03) | `IVS` | 28 | 18.261 |
| [`IVS_TGLCLICLIENT`](IVS.md#ivs_tglcliclient) | `IVS` | 25 | 16.654 |
| [`IVS_TGLCLIFIS`](IVS.md#ivs_tglclifis) | `IVS` | 16 | 11.232 |
| [`IVS_TGLCLIJUR`](IVS.md#ivs_tglclijur) | `IVS` | 10 | 5.438 |
| [`IVT_DePara`](IVT.md#ivt_depara) | `IVT` | 8 | 12 |
| [`IV_AgendaLog`](IV-1-processo-agenda-historico.md#iv_agendalog) | `IV-1` | 9 | 11.049.475 |
| [`IV_Campanha`](IV-4-demais.md#iv_campanha) | `IV-4` | 33 | 179 |
| [`IV_CBRCRITERIOMSG`](IV-4-demais.md#iv_cbrcriteriomsg) | `IV-4` | 6 | 3 |
| [`IV_CBRTITULOHST`](IV-4-demais.md#iv_cbrtitulohst) | `IV-4` | 5 | 173.530 |
| [`IV_Ciencia`](IV-1-processo-agenda-historico.md#iv_ciencia) | `IV-1` | 5 | 173.477 |
| [`IV_ClienteAtrib`](IV-2-formularios.md#iv_clienteatrib) | `IV-2` | 6 | 2.018 |
| [`IV_Evento`](IV-3-catalogos-bpm.md#iv_evento) | `IV-3` | 27 | 132 |
| [`IV_HistInfo`](IV-1-processo-agenda-historico.md#iv_histinfo) | `IV-1` | 2 | 17.250 |
| [`IV_HistLink`](IV-1-processo-agenda-historico.md#iv_histlink) | `IV-1` | 5 | 645.850 |
| [`IV_HistoricoNota`](IV-1-processo-agenda-historico.md#iv_historiconota) | `IV-1` | 2 | 94.315 |
| [`IV_ListSQL`](IV-2-formularios.md#iv_listsql) | `IV-2` | 5 | 360 |
| [`IV_Motivo`](IV-3-catalogos-bpm.md#iv_motivo) | `IV-3` | 4 | 43 |
| [`IV_ObjFlow`](IV-4-demais.md#iv_objflow) | `IV-4` | 9 | 726 |
| [`IV_OcrmAgd`](IV-4-demais.md#iv_ocrmagd) | `IV-4` | 8 | 18 |
| [`IV_OcrmDest`](IV-4-demais.md#iv_ocrmdest) | `IV-4` | 12 | 17 |
| [`IV_Pcte`](IV-2-formularios.md#iv_pcte) | `IV-2` | 3 | 1 |
| [`IV_ProcDocto`](IV-1-processo-agenda-historico.md#iv_procdocto) | `IV-1` | 5 | 73.646 |
| [`IV_ProcFaseMonit`](IV-3-catalogos-bpm.md#iv_procfasemonit) | `IV-3` | 9 | 655.144 |
| [`IV_ProcLink`](IV-1-processo-agenda-historico.md#iv_proclink) | `IV-1` | 7 | 602.150 |
| [`IV_ProcPerspMonit`](IV-3-catalogos-bpm.md#iv_procperspmonit) | `IV-3` | 6 | 126.145 |
| [`IV_ProcProduto`](IV-1-processo-agenda-historico.md#iv_procproduto) | `IV-1` | 18 | 62.578 |
| [`IV_ProcStatMonit`](IV-3-catalogos-bpm.md#iv_procstatmonit) | `IV-3` | 6 | 850.022 |
| [`IV_ProcVinc`](IV-1-processo-agenda-historico.md#iv_procvinc) | `IV-1` | 6 | 190 |
| [`IV_SelecaoColList`](IV-4-demais.md#iv_selecaocollist) | `IV-4` | 3 | 35 |
| [`IV_SMS`](IV-4-demais.md#iv_sms) | `IV-4` | 19 | 26.749 |
| [`IV_SMSLog`](IV-4-demais.md#iv_smslog) | `IV-4` | 5 | 27.430 |
| [`IV_STATUS_DEPTO`](IV-3-catalogos-bpm.md#iv_status_depto) | `IV-3` | 5 | 473.844 |
| [`IV_VENDEDOR`](IV-4-demais.md#iv_vendedor) | `IV-4` | 18 | 344 |
| [`IV_VENDEDOREMPR`](IV-4-demais.md#iv_vendedorempr) | `IV-4` | 4 | 917 |
| [`OUT_Contato`](OUT.md#out_contato) | `OUT` | 25 | 147 |
| [`OUT_Log`](OUT.md#out_log) | `OUT` | 12 | 1.550 |
| [`OUT_Pessoa`](OUT.md#out_pessoa) | `OUT` | 61 | 1.918 |
| [`OUT_PessoaLink`](OUT.md#out_pessoalink) | `OUT` | 4 | 122 |
| [`OUT_PessoaRelacao`](OUT.md#out_pessoarelacao) | `OUT` | 7 | 9 |

</details>

### Classe `lixo/backup` - 59 tabelas

Copia manual, teste, migracao ou legado. **Nunca usar em consulta de producao** (SCHEMA_MAP.md).

<details><summary>Lista completa das 59 tabelas</summary>

| Tabela | Modulo | Colunas | Linhas |
|---|---|---:|---:|
| [`andre`](OUTROS.md#andre) | `OUTROS` | 1 | 3 |
| [`CONTAS`](OUTROS.md#contas) | `OUTROS` | 8 | 1.003 |
| [`CONTAS2`](OUTROS.md#contas2) | `OUTROS` | 7 | 1.003 |
| [`DUAL`](OUTROS.md#dual) | `OUTROS` | 1 | 1 |
| [`Espaco_Tabelas`](ESPACO.md#espaco_tabelas) | `ESPACO` | 6 | 735 |
| [`EXT_Pessoa_bkpago22`](EXT.md#ext_pessoa_bkpago22) | `EXT` | 77 | 27.280 |
| [`EXT_Titulo_bkp_11_04`](EXT.md#ext_titulo_bkp_11_04) | `EXT` | 37 | 108.061 |
| [`EXT_Titulo_BKP_22_03_2023`](EXT.md#ext_titulo_bkp_22_03_2023) | `EXT` | 37 | 4.635 |
| [`EXT_Titulo_BKP_22_03_2023_BAIXADOS`](EXT.md#ext_titulo_bkp_22_03_2023_baixados) | `EXT` | 37 | 16.705 |
| [`GEP_Import_bkpjun`](GEP.md#gep_import_bkpjun) | `GEP` | 15 | 502.009 |
| [`GE_Cidade_BKPJUN`](GE.md#ge_cidade_bkpjun) | `GE` | 19 | 9.966 |
| [`GE_ContatoPapel_BKPJUN`](GE.md#ge_contatopapel_bkpjun) | `GE` | 5 | 35.413 |
| [`GE_ContatoPapel_ITA`](GE.md#ge_contatopapel_ita) | `GE` | 5 | 5.032 |
| [`GE_Contato_BKPJUN`](GE.md#ge_contato_bkpjun) | `GE` | 41 | 28.051 |
| [`GE_Contato_ITA`](GE.md#ge_contato_ita) | `GE` | 41 | 10.866 |
| [`GE_Membro_BKP20250520`](GE.md#ge_membro_bkp20250520) | `GE` | 3 | 1.915 |
| [`GE_PessoaEnd_BKPJUN`](GE.md#ge_pessoaend_bkpjun) | `GE` | 26 | 3.739 |
| [`GE_PessoaEnd_ITA`](GE.md#ge_pessoaend_ita) | `GE` | 26 | 2.779 |
| [`GE_PessoaFone_BKPJUN`](GE.md#ge_pessoafone_bkpjun) | `GE` | 19 | 69.894 |
| [`GE_PessoaFone_ITA`](GE.md#ge_pessoafone_ita) | `GE` | 19 | 42.594 |
| [`GE_PessoaLink_bkpago22`](GE.md#ge_pessoalink_bkpago22) | `GE` | 5 | 92.068 |
| [`GE_PessoaLink_BKPJUN`](GE.md#ge_pessoalink_bkpjun) | `GE` | 5 | 90.857 |
| [`GE_PessoaLink_ITA`](GE.md#ge_pessoalink_ita) | `GE` | 5 | 24.864 |
| [`GE_PessoaRelacao_BKPJUN`](GE.md#ge_pessoarelacao_bkpjun) | `GE` | 9 | 5.543 |
| [`GE_PessoaRelacao_ITA`](GE.md#ge_pessoarelacao_ita) | `GE` | 9 | 3.092 |
| [`GE_Pessoa_BKP28052025`](GE.md#ge_pessoa_bkp28052025) | `GE` | 76 | 0 |
| [`GE_Pessoa_BKPJUN`](GE.md#ge_pessoa_bkpjun) | `GE` | 76 | 88.142 |
| [`GE_Pessoa_ITA`](GE.md#ge_pessoa_ita) | `GE` | 76 | 28.377 |
| [`GE_UsuarioPerm_BKPJUN`](GE.md#ge_usuarioperm_bkpjun) | `GE` | 5 | 31.120 |
| [`GE_Usuario_BKP20250520`](GE.md#ge_usuario_bkp20250520) | `GE` | 35 | 1.302 |
| [`IMP_Titulo_bkp_11_04`](IMP.md#imp_titulo_bkp_11_04) | `IMP` | 42 | 2.168 |
| [`IVS_PES_NELSON`](IVS.md#ivs_pes_nelson) | `IVS` | 4 | 134.872 |
| [`IV_Agenda_bkp20250717`](IV-1-processo-agenda-historico.md#iv_agenda_bkp20250717) | `IV-1` | 40 | 1 |
| [`IV_ClientePropr_BKPJUN`](IV-2-formularios.md#iv_clientepropr_bkpjun) | `IV-2` | 51 | 1.000 |
| [`IV_ClientePropr_ITA`](IV-2-formularios.md#iv_clientepropr_ita) | `IV-2` | 51 | 157.900 |
| [`IV_Processo_bkp20250717`](IV-1-processo-agenda-historico.md#iv_processo_bkp20250717) | `IV-1` | 22 | 1 |
| [`IV_Propriedade_BKPJUN`](IV-2-formularios.md#iv_propriedade_bkpjun) | `IV-2` | 70 | 21 |
| [`IV_Propriedade_ITA`](IV-2-formularios.md#iv_propriedade_ita) | `IV-2` | 70 | 32 |
| [`IV_PropriLista_ITA`](IV-2-formularios.md#iv_proprilista_ita) | `IV-2` | 5 | 7.520 |
| [`IV_RESULTADO_3110`](IV-4-demais.md#iv_resultado_3110) | `IV-4` | 74 | 2.826 |
| [`J1_PRODUTO`](J1.md#j1_produto) | `J1` | 9 | 211.556 |
| [`mig_ge_pessoa`](MIG.md#mig_ge_pessoa) | `MIG` | 2 | 5 |
| [`mig_ge_pessoa_bkp`](MIG.md#mig_ge_pessoa_bkp) | `MIG` | 2 | 28.084 |
| [`nfsaida$`](OUTROS.md#nfsaida) | `OUTROS` | 23 | 62.619 |
| [`SYSCONVERT1`](OUTROS.md#sysconvert1) | `OUTROS` | 2 | 3 |
| [`SYSCONVERT2`](OUTROS.md#sysconvert2) | `OUTROS` | 2 | 24 |
| [`SYSCONVERT3`](OUTROS.md#sysconvert3) | `OUTROS` | 2 | 14 |
| [`sysdiagrams`](OUTROS.md#sysdiagrams) | `OUTROS` | 5 | 0 |
| [`SYSDUMMY`](OUTROS.md#sysdummy) | `OUTROS` | 2 | 0 |
| [`TEMP`](OUTROS.md#temp) | `OUTROS` | 3 | 1.026 |
| [`teste`](OUTROS.md#teste) | `OUTROS` | 1 | 0 |
| [`teste333`](OUTROS.md#teste333) | `OUTROS` | 11 | 24.024 |
| [`testepiv`](OUTROS.md#testepiv) | `OUTROS` | 4 | 12 |
| [`teste_acesso`](TESTE.md#teste_acesso) | `TESTE` | 1 | 471 |
| [`teste_fefa`](TESTE.md#teste_fefa) | `TESTE` | 1 | 0 |
| [`usuarios$`](OUTROS.md#usuarios) | `OUTROS` | 2 | 158 |
| [`ww`](OUTROS.md#ww) | `OUTROS` | 22 | 8.991 |
| [`X_T_IMP_CRM_TITULO_bkp_11_04`](X_TOTVS.md#x_t_imp_crm_titulo_bkp_11_04) | `X_TOTVS` | 41 | 121.381 |
| [`X_V_IMP_CRM_IMP_NF_BKP_18_09_2023`](X_TOTVS.md#x_v_imp_crm_imp_nf_bkp_18_09_2023) | `X_TOTVS` | 6 | 415.762 |

</details>

## 6. Cobertura da descricao funcional

| Origem da coluna "Funcao" | Tabelas | % |
|---|---:|---:|
| Derivada da estrutura com certeza (tabela `IV_Q_*` de formulario) | 174 | 22.7% |
| Descricao encontrada em documento (secao `### Tabela` + `**Funcao:**`) | 186 | 24.3% |
| Mencao descritiva encontrada em documento (lista, tabela ou frase) | 66 | 8.6% |
| Inferida do nome + colunas - marcada `(inferido)` | 293 | 38.2% |
| `(nao documentado - apurar com acesso ao vivo)` | 48 | 6.3% |

As tabelas do nivel 4 estao listadas por modulo em [LACUNAS.md](LACUNAS.md) secao 3 - e o roteiro de investigacao para quando houver acesso ao vivo.

## 7. Indice alfabetico de todas as tabelas

| Tabela | Modulo | Arquivo | Classe | Colunas | Linhas |
|---|---|---|---|---:|---:|
| [`andre`](OUTROS.md#andre) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 1 | 3 |
| [`CBR_CLIENTECONTA`](CBR.md#cbr_clienteconta) | `CBR` | [CBR.md](CBR.md) | vazia | 7 | 0 |
| [`CLARITY_BINA`](CLARITY.md#clarity_bina) | `CLARITY` | [CLARITY.md](CLARITY.md) | vazia | 5 | 0 |
| [`CONTAS`](OUTROS.md#contas) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 8 | 1.003 |
| [`CONTAS2`](OUTROS.md#contas2) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 7 | 1.003 |
| [`DMN_Doc`](DMN.md#dmn_doc) | `DMN` | [DMN.md](DMN.md) | nucleo | 17 | 68.609 |
| [`DMN_DocArq`](DMN.md#dmn_docarq) | `DMN` | [DMN.md](DMN.md) | vazia | 5 | 0 |
| [`DMN_DocHst`](DMN.md#dmn_dochst) | `DMN` | [DMN.md](DMN.md) | isolada | 6 | 183.667 |
| [`DMN_DocObs`](DMN.md#dmn_docobs) | `DMN` | [DMN.md](DMN.md) | vazia | 2 | 0 |
| [`DMN_DocPes`](DMN.md#dmn_docpes) | `DMN` | [DMN.md](DMN.md) | nucleo | 4 | 73.576 |
| [`DMN_DocProj`](DMN.md#dmn_docproj) | `DMN` | [DMN.md](DMN.md) | vazia | 4 | 0 |
| [`DMN_DocProp`](DMN.md#dmn_docprop) | `DMN` | [DMN.md](DMN.md) | vazia | 4 | 0 |
| [`DMN_DocTp`](DMN.md#dmn_doctp) | `DMN` | [DMN.md](DMN.md) | isolada | 24 | 107 |
| [`DMN_DocVrs`](DMN.md#dmn_docvrs) | `DMN` | [DMN.md](DMN.md) | vazia | 7 | 0 |
| [`DUAL`](OUTROS.md#dual) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 1 | 1 |
| [`Espaco_Tabelas`](ESPACO.md#espaco_tabelas) | `ESPACO` | [ESPACO.md](ESPACO.md) | lixo/backup | 6 | 735 |
| [`EXT_AGUARDENTREGA`](EXT.md#ext_aguardentrega) | `EXT` | [EXT.md](EXT.md) | isolada | 3 | 50 |
| [`EXT_APROVACAO`](EXT.md#ext_aprovacao) | `EXT` | [EXT.md](EXT.md) | isolada | 3 | 2.224 |
| [`EXT_CONDPAGTO`](EXT.md#ext_condpagto) | `EXT` | [EXT.md](EXT.md) | isolada | 3 | 21 |
| [`EXT_EMAIL`](EXT.md#ext_email) | `EXT` | [EXT.md](EXT.md) | vazia | 15 | 0 |
| [`EXT_FormaPgto`](EXT.md#ext_formapgto) | `EXT` | [EXT.md](EXT.md) | vazia | 6 | 0 |
| [`EXT_NFS`](EXT.md#ext_nfs) | `EXT` | [EXT.md](EXT.md) | nucleo | 39 | 390.755 |
| [`EXT_NFSCmpl`](EXT.md#ext_nfscmpl) | `EXT` | [EXT.md](EXT.md) | vazia | 5 | 0 |
| [`EXT_NFSItem`](EXT.md#ext_nfsitem) | `EXT` | [EXT.md](EXT.md) | nucleo | 27 | 1.300.126 |
| [`EXT_NFSOper`](EXT.md#ext_nfsoper) | `EXT` | [EXT.md](EXT.md) | catalogo | 9 | 241 |
| [`EXT_NFSOperEmp`](EXT.md#ext_nfsoperemp) | `EXT` | [EXT.md](EXT.md) | nucleo | 3 | 330 |
| [`EXT_OS`](EXT.md#ext_os) | `EXT` | [EXT.md](EXT.md) | nucleo | 40 | 8.099 |
| [`EXT_OSITEM`](EXT.md#ext_ositem) | `EXT` | [EXT.md](EXT.md) | isolada | 15 | 182.238 |
| [`EXT_OSSolic`](EXT.md#ext_ossolic) | `EXT` | [EXT.md](EXT.md) | vazia | 6 | 0 |
| [`EXT_Pedido`](EXT.md#ext_pedido) | `EXT` | [EXT.md](EXT.md) | vazia | 57 | 0 |
| [`EXT_PedidoItem`](EXT.md#ext_pedidoitem) | `EXT` | [EXT.md](EXT.md) | vazia | 22 | 0 |
| [`EXT_Pessoa`](EXT.md#ext_pessoa) | `EXT` | [EXT.md](EXT.md) | nucleo | 77 | 57.682 |
| [`EXT_PESSOACONTATO`](EXT.md#ext_pessoacontato) | `EXT` | [EXT.md](EXT.md) | vazia | 29 | 0 |
| [`EXT_PESSOAFONE`](EXT.md#ext_pessoafone) | `EXT` | [EXT.md](EXT.md) | vazia | 20 | 0 |
| [`EXT_Pessoa_bkpago22`](EXT.md#ext_pessoa_bkpago22) | `EXT` | [EXT.md](EXT.md) | lixo/backup | 77 | 27.280 |
| [`EXT_Pot_Pecas`](EXT.md#ext_pot_pecas) | `EXT` | [EXT.md](EXT.md) | isolada | 13 | 1.287 |
| [`EXT_Produto`](EXT.md#ext_produto) | `EXT` | [EXT.md](EXT.md) | nucleo | 27 | 78.749 |
| [`EXT_Titulo`](EXT.md#ext_titulo) | `EXT` | [EXT.md](EXT.md) | nucleo | 37 | 577.925 |
| [`EXT_TituloCmpl`](EXT.md#ext_titulocmpl) | `EXT` | [EXT.md](EXT.md) | vazia | 2 | 0 |
| [`EXT_TituloMov`](EXT.md#ext_titulomov) | `EXT` | [EXT.md](EXT.md) | nucleo | 9 | 1.134.039 |
| [`EXT_Titulo_bkp_11_04`](EXT.md#ext_titulo_bkp_11_04) | `EXT` | [EXT.md](EXT.md) | lixo/backup | 37 | 108.061 |
| [`EXT_Titulo_BKP_22_03_2023`](EXT.md#ext_titulo_bkp_22_03_2023) | `EXT` | [EXT.md](EXT.md) | lixo/backup | 37 | 4.635 |
| [`EXT_Titulo_BKP_22_03_2023_BAIXADOS`](EXT.md#ext_titulo_bkp_22_03_2023_baixados) | `EXT` | [EXT.md](EXT.md) | lixo/backup | 37 | 16.705 |
| [`EXT_TIT_ACRESC`](EXT.md#ext_tit_acresc) | `EXT` | [EXT.md](EXT.md) | isolada | 2 | 1.575 |
| [`EXT_Veic`](EXT.md#ext_veic) | `EXT` | [EXT.md](EXT.md) | nucleo | 28 | 8.020 |
| [`EXT_VeicAgd`](EXT.md#ext_veicagd) | `EXT` | [EXT.md](EXT.md) | vazia | 28 | 0 |
| [`EXT_VEICAGDERP`](EXT.md#ext_veicagderp) | `EXT` | [EXT.md](EXT.md) | vazia | 12 | 0 |
| [`EXT_VeicAvalFoto`](EXT.md#ext_veicavalfoto) | `EXT` | [EXT.md](EXT.md) | vazia | 3 | 0 |
| [`EXT_VeicAvalia`](EXT.md#ext_veicavalia) | `EXT` | [EXT.md](EXT.md) | vazia | 31 | 0 |
| [`EXT_VeicFam`](EXT.md#ext_veicfam) | `EXT` | [EXT.md](EXT.md) | catalogo | 6 | 68 |
| [`EXT_VEICFAMREF`](EXT.md#ext_veicfamref) | `EXT` | [EXT.md](EXT.md) | isolada | 2 | 23 |
| [`EXT_VeicKM`](EXT.md#ext_veickm) | `EXT` | [EXT.md](EXT.md) | nucleo | 11 | 17.378 |
| [`EXT_VeicMarca`](EXT.md#ext_veicmarca) | `EXT` | [EXT.md](EXT.md) | catalogo | 5 | 5 |
| [`EXT_VeicMarcaEmp`](EXT.md#ext_veicmarcaemp) | `EXT` | [EXT.md](EXT.md) | nucleo | 4 | 2 |
| [`EXT_VeicModelo`](EXT.md#ext_veicmodelo) | `EXT` | [EXT.md](EXT.md) | nucleo | 5 | 4.431.168 |
| [`EXT_VeicModPlano`](EXT.md#ext_veicmodplano) | `EXT` | [EXT.md](EXT.md) | nucleo | 2 | 3.571.235 |
| [`EXT_VeicPlanoMan`](EXT.md#ext_veicplanoman) | `EXT` | [EXT.md](EXT.md) | nucleo | 4 | 3.571.257 |
| [`EXT_VeicPlanoMnFX`](EXT.md#ext_veicplanomnfx) | `EXT` | [EXT.md](EXT.md) | vazia | 7 | 0 |
| [`EXT_VeicProp`](EXT.md#ext_veicprop) | `EXT` | [EXT.md](EXT.md) | nucleo | 24 | 12.621 |
| [`EXT_VEICREF`](EXT.md#ext_veicref) | `EXT` | [EXT.md](EXT.md) | isolada | 3 | 40 |
| [`EXT_VeicTipoMan`](EXT.md#ext_veictipoman) | `EXT` | [EXT.md](EXT.md) | catalogo | 8 | 1 |
| [`EXT_Vendedor`](EXT.md#ext_vendedor) | `EXT` | [EXT.md](EXT.md) | catalogo | 6 | 563 |
| [`GEL_CEP`](GEL.md#gel_cep) | `GEL` | [GEL.md](GEL.md) | vazia | 14 | 0 |
| [`GEL_CepAlerta`](GEL.md#gel_cepalerta) | `GEL` | [GEL.md](GEL.md) | vazia | 4 | 0 |
| [`GEL_CepOrig`](GEL.md#gel_ceporig) | `GEL` | [GEL.md](GEL.md) | vazia | 10 | 0 |
| [`GEL_Cidade`](GEL.md#gel_cidade) | `GEL` | [GEL.md](GEL.md) | vazia | 12 | 0 |
| [`GEL_Conv`](GEL.md#gel_conv) | `GEL` | [GEL.md](GEL.md) | vazia | 2 | 0 |
| [`GEL_ConvDe`](GEL.md#gel_convde) | `GEL` | [GEL.md](GEL.md) | vazia | 4 | 0 |
| [`GEP_CidadeEmprDest`](GEP.md#gep_cidadeemprdest) | `GEP` | [GEP.md](GEP.md) | vazia | 3 | 0 |
| [`GEP_EMailSend`](GEP.md#gep_emailsend) | `GEP` | [GEP.md](GEP.md) | vazia | 21 | 0 |
| [`GEP_EMAILSENDREL`](GEP.md#gep_emailsendrel) | `GEP` | [GEP.md](GEP.md) | vazia | 4 | 0 |
| [`GEP_EMAILSENT`](GEP.md#gep_emailsent) | `GEP` | [GEP.md](GEP.md) | nucleo | 20 | 571.866 |
| [`GEP_EMAILSENTREL`](GEP.md#gep_emailsentrel) | `GEP` | [GEP.md](GEP.md) | vazia | 4 | 0 |
| [`gep_excfrota`](GEP.md#gep_excfrota) | `GEP` | [GEP.md](GEP.md) | isolada | 7 | 1.180 |
| [`gep_excfrotarep`](GEP.md#gep_excfrotarep) | `GEP` | [GEP.md](GEP.md) | isolada | 11 | 425 |
| [`GEP_EXCFROTASEQ`](GEP.md#gep_excfrotaseq) | `GEP` | [GEP.md](GEP.md) | isolada | 1 | 1.128 |
| [`GEP_EXCFROTASEQDEL`](GEP.md#gep_excfrotaseqdel) | `GEP` | [GEP.md](GEP.md) | isolada | 1 | 1.082 |
| [`gep_excseqcar`](GEP.md#gep_excseqcar) | `GEP` | [GEP.md](GEP.md) | isolada | 1 | 3.143 |
| [`gep_excseqcarok`](GEP.md#gep_excseqcarok) | `GEP` | [GEP.md](GEP.md) | isolada | 1 | 1.668 |
| [`GEP_Fila`](GEP.md#gep_fila) | `GEP` | [GEP.md](GEP.md) | isolada | 18 | 4 |
| [`GEP_Import`](GEP.md#gep_import) | `GEP` | [GEP.md](GEP.md) | isolada | 15 | 344 |
| [`GEP_ImportAprovacao`](GEP.md#gep_importaprovacao) | `GEP` | [GEP.md](GEP.md) | isolada | 15 | 4.284 |
| [`GEP_ImportErro`](GEP.md#gep_importerro) | `GEP` | [GEP.md](GEP.md) | vazia | 4 | 0 |
| [`GEP_ImportTry`](GEP.md#gep_importtry) | `GEP` | [GEP.md](GEP.md) | isolada | 4 | 1.870 |
| [`GEP_Import_bkpjun`](GEP.md#gep_import_bkpjun) | `GEP` | [GEP.md](GEP.md) | lixo/backup | 15 | 502.009 |
| [`GEP_IMPORT_GUI`](GEP.md#gep_import_gui) | `GEP` | [GEP.md](GEP.md) | isolada | 15 | 52 |
| [`GEP_JOBAGD`](GEP.md#gep_jobagd) | `GEP` | [GEP.md](GEP.md) | nucleo | 18 | 25 |
| [`GEP_JobAgdExecLog`](GEP.md#gep_jobagdexeclog) | `GEP` | [GEP.md](GEP.md) | isolada | 13 | 981.319 |
| [`GEP_JOBCAD`](GEP.md#gep_jobcad) | `GEP` | [GEP.md](GEP.md) | catalogo | 11 | 71 |
| [`GEP_JOBFILA`](GEP.md#gep_jobfila) | `GEP` | [GEP.md](GEP.md) | nucleo | 16 | 23 |
| [`GEP_JOBFILALOG`](GEP.md#gep_jobfilalog) | `GEP` | [GEP.md](GEP.md) | isolada | 5 | 14.038 |
| [`GEP_JobMonitor`](GEP.md#gep_jobmonitor) | `GEP` | [GEP.md](GEP.md) | isolada | 10 | 1 |
| [`GEP_JOBNOTIFICAR`](GEP.md#gep_jobnotificar) | `GEP` | [GEP.md](GEP.md) | vazia | 14 | 0 |
| [`GEP_Notificar`](GEP.md#gep_notificar) | `GEP` | [GEP.md](GEP.md) | vazia | 8 | 0 |
| [`GEP_PABX`](GEP.md#gep_pabx) | `GEP` | [GEP.md](GEP.md) | vazia | 6 | 0 |
| [`GEP_ParEnvia`](GEP.md#gep_parenvia) | `GEP` | [GEP.md](GEP.md) | catalogo | 7 | 3 |
| [`GEP_ParRecebe`](GEP.md#gep_parrecebe) | `GEP` | [GEP.md](GEP.md) | nucleo | 12 | 14 |
| [`GEP_ParRecEnvia`](GEP.md#gep_parrecenvia) | `GEP` | [GEP.md](GEP.md) | vazia | 2 | 0 |
| [`GEP_ProcControle`](GEP.md#gep_proccontrole) | `GEP` | [GEP.md](GEP.md) | vazia | 5 | 0 |
| [`GEP_Processo`](GEP.md#gep_processo) | `GEP` | [GEP.md](GEP.md) | nucleo | 20 | 8 |
| [`GEP_ProcImport`](GEP.md#gep_procimport) | `GEP` | [GEP.md](GEP.md) | vazia | 9 | 0 |
| [`GEP_SyncUsrSat`](GEP.md#gep_syncusrsat) | `GEP` | [GEP.md](GEP.md) | isolada | 15 | 3.901.359 |
| [`Gep_UsrPabx`](GEP.md#gep_usrpabx) | `GEP` | [GEP.md](GEP.md) | vazia | 5 | 0 |
| [`GEP_UsrSat`](GEP.md#gep_usrsat) | `GEP` | [GEP.md](GEP.md) | nucleo | 16 | 71 |
| [`GE_Alias`](GE.md#ge_alias) | `GE` | [GE.md](GE.md) | isolada | 9 | 1.413 |
| [`GE_Aplicacao`](GE.md#ge_aplicacao) | `GE` | [GE.md](GE.md) | nucleo | 9 | 184 |
| [`GE_AppModulo`](GE.md#ge_appmodulo) | `GE` | [GE.md](GE.md) | nucleo | 3 | 30 |
| [`GE_AtributoFixo`](GE.md#ge_atributofixo) | `GE` | [GE.md](GE.md) | catalogo | 9 | 183 |
| [`GE_Bairro`](GE.md#ge_bairro) | `GE` | [GE.md](GE.md) | catalogo | 3 | 619 |
| [`GE_CampoExig`](GE.md#ge_campoexig) | `GE` | [GE.md](GE.md) | vazia | 7 | 0 |
| [`GE_CampoMemo`](GE.md#ge_campomemo) | `GE` | [GE.md](GE.md) | nucleo | 4 | 3.129 |
| [`GE_CampoPerm`](GE.md#ge_campoperm) | `GE` | [GE.md](GE.md) | nucleo | 7 | 17 |
| [`GE_CFinConj`](GE.md#ge_cfinconj) | `GE` | [GE.md](GE.md) | vazia | 22 | 0 |
| [`GE_CFinDocto`](GE.md#ge_cfindocto) | `GE` | [GE.md](GE.md) | vazia | 7 | 0 |
| [`GE_CFinFis`](GE.md#ge_cfinfis) | `GE` | [GE.md](GE.md) | vazia | 51 | 0 |
| [`GE_CFinRPes`](GE.md#ge_cfinrpes) | `GE` | [GE.md](GE.md) | vazia | 12 | 0 |
| [`GE_Cidade`](GE.md#ge_cidade) | `GE` | [GE.md](GE.md) | nucleo | 19 | 10.214 |
| [`GE_CIdadePref`](GE.md#ge_cidadepref) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_Cidade_BKPJUN`](GE.md#ge_cidade_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 19 | 9.966 |
| [`GE_Cidade_CRM`](GE.md#ge_cidade_crm) | `GE` | [GE.md](GE.md) | nucleo | 18 | 729 |
| [`GE_Col`](GE.md#ge_col) | `GE` | [GE.md](GE.md) | catalogo | 6 | 217 |
| [`GE_ColPosition`](GE.md#ge_colposition) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_ColRegra`](GE.md#ge_colregra) | `GE` | [GE.md](GE.md) | vazia | 8 | 0 |
| [`GE_CONSHST`](GE.md#ge_conshst) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_CONSSQL`](GE.md#ge_conssql) | `GE` | [GE.md](GE.md) | catalogo | 10 | 1 |
| [`GE_Consulta`](GE.md#ge_consulta) | `GE` | [GE.md](GE.md) | nucleo | 29 | 2 |
| [`GE_ConsultaVar`](GE.md#ge_consultavar) | `GE` | [GE.md](GE.md) | nucleo | 10 | 3 |
| [`GE_CONSVAR`](GE.md#ge_consvar) | `GE` | [GE.md](GE.md) | vazia | 10 | 0 |
| [`GE_CONSVARLST`](GE.md#ge_consvarlst) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_Contato`](GE.md#ge_contato) | `GE` | [GE.md](GE.md) | isolada | 41 | 41.164 |
| [`GE_CONTATOAPP`](GE.md#ge_contatoapp) | `GE` | [GE.md](GE.md) | vazia | 12 | 0 |
| [`GE_ContatoEmail`](GE.md#ge_contatoemail) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_ContatoPapel`](GE.md#ge_contatopapel) | `GE` | [GE.md](GE.md) | isolada | 5 | 43.489 |
| [`GE_ContatoPapel_BKPJUN`](GE.md#ge_contatopapel_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 5 | 35.413 |
| [`GE_ContatoPapel_ITA`](GE.md#ge_contatopapel_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 5 | 5.032 |
| [`GE_Contato_BKPJUN`](GE.md#ge_contato_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 41 | 28.051 |
| [`GE_Contato_ITA`](GE.md#ge_contato_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 41 | 10.866 |
| [`GE_CTRL2`](GE.md#ge_ctrl2) | `GE` | [GE.md](GE.md) | isolada | 6 | 276 |
| [`GE_CtrlE`](GE.md#ge_ctrle) | `GE` | [GE.md](GE.md) | isolada | 9 | 16 |
| [`GE_CtrlM`](GE.md#ge_ctrlm) | `GE` | [GE.md](GE.md) | isolada | 11 | 31 |
| [`GE_CtrlME`](GE.md#ge_ctrlme) | `GE` | [GE.md](GE.md) | isolada | 6 | 36 |
| [`GE_CTRLMU`](GE.md#ge_ctrlmu) | `GE` | [GE.md](GE.md) | vazia | 6 | 0 |
| [`GE_CTRLUSR`](GE.md#ge_ctrlusr) | `GE` | [GE.md](GE.md) | isolada | 6 | 33 |
| [`GE_DiaNaoUtil`](GE.md#ge_dianaoutil) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_Email`](GE.md#ge_email) | `GE` | [GE.md](GE.md) | isolada | 16 | 38.765 |
| [`GE_EMAILAGD`](GE.md#ge_emailagd) | `GE` | [GE.md](GE.md) | nucleo | 13 | 2 |
| [`GE_EMAILINVALIDO`](GE.md#ge_emailinvalido) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_EmailSpam`](GE.md#ge_emailspam) | `GE` | [GE.md](GE.md) | vazia | 4 | 0 |
| [`GE_EMAILTEMPLATE`](GE.md#ge_emailtemplate) | `GE` | [GE.md](GE.md) | nucleo | 6 | 8 |
| [`GE_Empresa`](GE.md#ge_empresa) | `GE` | [GE.md](GE.md) | catalogo | 33 | 18 |
| [`GE_FCadConj`](GE.md#ge_fcadconj) | `GE` | [GE.md](GE.md) | vazia | 43 | 0 |
| [`GE_FCadF`](GE.md#ge_fcadf) | `GE` | [GE.md](GE.md) | vazia | 97 | 0 |
| [`GE_FCadInstCred`](GE.md#ge_fcadinstcred) | `GE` | [GE.md](GE.md) | vazia | 13 | 0 |
| [`GE_FcadRBanc`](GE.md#ge_fcadrbanc) | `GE` | [GE.md](GE.md) | vazia | 18 | 0 |
| [`GE_FcadRCom`](GE.md#ge_fcadrcom) | `GE` | [GE.md](GE.md) | vazia | 20 | 0 |
| [`GE_FCadRPes`](GE.md#ge_fcadrpes) | `GE` | [GE.md](GE.md) | vazia | 33 | 0 |
| [`GE_FERIADO`](GE.md#ge_feriado) | `GE` | [GE.md](GE.md) | vazia | 7 | 0 |
| [`GE_Figura`](GE.md#ge_figura) | `GE` | [GE.md](GE.md) | isolada | 8 | 2.078 |
| [`GE_FONEINVALIDO`](GE.md#ge_foneinvalido) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_Grafico`](GE.md#ge_grafico) | `GE` | [GE.md](GE.md) | vazia | 26 | 0 |
| [`GE_Help`](GE.md#ge_help) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_IMPORTA_CART`](GE.md#ge_importa_cart) | `GE` | [GE.md](GE.md) | isolada | 52 | 22 |
| [`GE_IntCtrl`](GE.md#ge_intctrl) | `GE` | [GE.md](GE.md) | vazia | 14 | 0 |
| [`GE_LgTb`](GE.md#ge_lgtb) | `GE` | [GE.md](GE.md) | isolada | 10 | 11.861.777 |
| [`GE_Log2`](GE.md#ge_log2) | `GE` | [GE.md](GE.md) | isolada | 14 | 4.394.779 |
| [`GE_LogAtividade`](GE.md#ge_logatividade) | `GE` | [GE.md](GE.md) | vazia | 10 | 0 |
| [`GE_LOG_CARTCRED`](GE.md#ge_log_cartcred) | `GE` | [GE.md](GE.md) | vazia | 10 | 0 |
| [`GE_LOG_CONFIG`](GE.md#ge_log_config) | `GE` | [GE.md](GE.md) | isolada | 10 | 108.492 |
| [`GE_LOG_CONTATO`](GE.md#ge_log_contato) | `GE` | [GE.md](GE.md) | isolada | 10 | 19.785 |
| [`GE_LOG_EXT`](GE.md#ge_log_ext) | `GE` | [GE.md](GE.md) | isolada | 10 | 622.075 |
| [`GE_LOG_HISTORICO`](GE.md#ge_log_historico) | `GE` | [GE.md](GE.md) | isolada | 10 | 1.015.045 |
| [`GE_LOG_PESSOA`](GE.md#ge_log_pessoa) | `GE` | [GE.md](GE.md) | isolada | 10 | 943.712 |
| [`GE_LOG_PROCESSO`](GE.md#ge_log_processo) | `GE` | [GE.md](GE.md) | isolada | 10 | 12.678.640 |
| [`GE_LOG_TRANS`](GE.md#ge_log_trans) | `GE` | [GE.md](GE.md) | isolada | 10 | 782.089 |
| [`GE_Membro`](GE.md#ge_membro) | `GE` | [GE.md](GE.md) | nucleo | 3 | 1.584 |
| [`GE_Membro_BKP20250520`](GE.md#ge_membro_bkp20250520) | `GE` | [GE.md](GE.md) | lixo/backup | 3 | 1.915 |
| [`GE_MobileConfig`](GE.md#ge_mobileconfig) | `GE` | [GE.md](GE.md) | vazia | 14 | 0 |
| [`GE_MOBILEPROP`](GE.md#ge_mobileprop) | `GE` | [GE.md](GE.md) | vazia | 2 | 0 |
| [`GE_Mod`](GE.md#ge_mod) | `GE` | [GE.md](GE.md) | isolada | 9 | 32 |
| [`GE_Modulo`](GE.md#ge_modulo) | `GE` | [GE.md](GE.md) | catalogo | 8 | 48 |
| [`GE_ModuloPerm`](GE.md#ge_moduloperm) | `GE` | [GE.md](GE.md) | nucleo | 6 | 25.435 |
| [`GE_ObjDinamico`](GE.md#ge_objdinamico) | `GE` | [GE.md](GE.md) | isolada | 29 | 193 |
| [`GE_ObjDinAplic`](GE.md#ge_objdinaplic) | `GE` | [GE.md](GE.md) | isolada | 4 | 122 |
| [`GE_ParametroGlobal`](GE.md#ge_parametroglobal) | `GE` | [GE.md](GE.md) | isolada | 8 | 236 |
| [`GE_ParamLista`](GE.md#ge_paramlista) | `GE` | [GE.md](GE.md) | catalogo | 25 | 333 |
| [`GE_Permissao`](GE.md#ge_permissao) | `GE` | [GE.md](GE.md) | nucleo | 11 | 307 |
| [`GE_PesDelVinc`](GE.md#ge_pesdelvinc) | `GE` | [GE.md](GE.md) | vazia | 7 | 0 |
| [`GE_Pessoa`](GE.md#ge_pessoa) | `GE` | [GE.md](GE.md) | nucleo | 76 | 118.463 |
| [`GE_PessoaAlerta`](GE.md#ge_pessoaalerta) | `GE` | [GE.md](GE.md) | isolada | 8 | 237 |
| [`GE_PessoaAlt`](GE.md#ge_pessoaalt) | `GE` | [GE.md](GE.md) | vazia | 68 | 0 |
| [`ge_pessoaativa`](GE.md#ge_pessoaativa) | `GE` | [GE.md](GE.md) | isolada | 1 | 1.175 |
| [`GE_PESSOAATIVAUSR`](GE.md#ge_pessoaativausr) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_PessoaClasse`](GE.md#ge_pessoaclasse) | `GE` | [GE.md](GE.md) | isolada | 7 | 1.878 |
| [`GE_PessoaDel`](GE.md#ge_pessoadel) | `GE` | [GE.md](GE.md) | vazia | 76 | 0 |
| [`GE_PessoaDestino`](GE.md#ge_pessoadestino) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_PessoaEmail`](GE.md#ge_pessoaemail) | `GE` | [GE.md](GE.md) | vazia | 8 | 0 |
| [`GE_PessoaEnd`](GE.md#ge_pessoaend) | `GE` | [GE.md](GE.md) | nucleo | 26 | 6.480 |
| [`GE_PessoaEndAlt`](GE.md#ge_pessoaendalt) | `GE` | [GE.md](GE.md) | vazia | 21 | 0 |
| [`GE_PessoaEndOutroBc`](GE.md#ge_pessoaendoutrobc) | `GE` | [GE.md](GE.md) | nucleo | 3 | 30.943 |
| [`GE_PessoaEnd_BKPJUN`](GE.md#ge_pessoaend_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 26 | 3.739 |
| [`GE_PessoaEnd_ITA`](GE.md#ge_pessoaend_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 26 | 2.779 |
| [`GE_PessoaFis`](GE.md#ge_pessoafis) | `GE` | [GE.md](GE.md) | isolada | 32 | 5.136 |
| [`GE_PessoaFone`](GE.md#ge_pessoafone) | `GE` | [GE.md](GE.md) | isolada | 19 | 112.659 |
| [`GE_PessoaFonema`](GE.md#ge_pessoafonema) | `GE` | [GE.md](GE.md) | isolada | 3 | 455.993 |
| [`GE_PessoaFone_BKPJUN`](GE.md#ge_pessoafone_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 19 | 69.894 |
| [`GE_PessoaFone_ITA`](GE.md#ge_pessoafone_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 19 | 42.594 |
| [`GE_PessoaJur`](GE.md#ge_pessoajur) | `GE` | [GE.md](GE.md) | vazia | 15 | 0 |
| [`GE_PessoaLink`](GE.md#ge_pessoalink) | `GE` | [GE.md](GE.md) | isolada | 5 | 32.831 |
| [`GE_PessoaLinkbkp`](GE.md#ge_pessoalinkbkp) | `GE` | [GE.md](GE.md) | isolada | 5 | 58.741 |
| [`GE_PessoaLink_bkpago22`](GE.md#ge_pessoalink_bkpago22) | `GE` | [GE.md](GE.md) | lixo/backup | 5 | 92.068 |
| [`GE_PessoaLink_BKPJUN`](GE.md#ge_pessoalink_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 5 | 90.857 |
| [`GE_PessoaLink_ITA`](GE.md#ge_pessoalink_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 5 | 24.864 |
| [`GE_PessoaMural`](GE.md#ge_pessoamural) | `GE` | [GE.md](GE.md) | isolada | 14 | 145.313 |
| [`GE_PessoaNomeFonema`](GE.md#ge_pessoanomefonema) | `GE` | [GE.md](GE.md) | isolada | 4 | 151.583 |
| [`GE_PessoaNota`](GE.md#ge_pessoanota) | `GE` | [GE.md](GE.md) | nucleo | 2 | 1.232 |
| [`GE_PessoaPasw`](GE.md#ge_pessoapasw) | `GE` | [GE.md](GE.md) | vazia | 9 | 0 |
| [`GE_PESSOAREDESOCIAL`](GE.md#ge_pessoaredesocial) | `GE` | [GE.md](GE.md) | vazia | 10 | 0 |
| [`GE_PessoaRelacao`](GE.md#ge_pessoarelacao) | `GE` | [GE.md](GE.md) | isolada | 9 | 5.595 |
| [`GE_PessoaRelacao_BKPJUN`](GE.md#ge_pessoarelacao_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 9 | 5.543 |
| [`GE_PessoaRelacao_ITA`](GE.md#ge_pessoarelacao_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 9 | 3.092 |
| [`GE_PessoaSimilar`](GE.md#ge_pessoasimilar) | `GE` | [GE.md](GE.md) | nucleo | 14 | 124.222 |
| [`GE_PessoaUnidade`](GE.md#ge_pessoaunidade) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_PessoaVersao`](GE.md#ge_pessoaversao) | `GE` | [GE.md](GE.md) | isolada | 19 | 12.019 |
| [`GE_Pessoa_BKP28052025`](GE.md#ge_pessoa_bkp28052025) | `GE` | [GE.md](GE.md) | lixo/backup | 76 | 0 |
| [`GE_Pessoa_BKPJUN`](GE.md#ge_pessoa_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 76 | 88.142 |
| [`GE_Pessoa_ITA`](GE.md#ge_pessoa_ita) | `GE` | [GE.md](GE.md) | lixo/backup | 76 | 28.377 |
| [`GE_PolSeg`](GE.md#ge_polseg) | `GE` | [GE.md](GE.md) | catalogo | 3 | 11 |
| [`GE_PolSegAces`](GE.md#ge_polsegaces) | `GE` | [GE.md](GE.md) | nucleo | 7 | 3 |
| [`GE_PolSegCtrl`](GE.md#ge_polsegctrl) | `GE` | [GE.md](GE.md) | nucleo | 7 | 254 |
| [`GE_PolSegItem`](GE.md#ge_polsegitem) | `GE` | [GE.md](GE.md) | catalogo | 7 | 108 |
| [`GE_PolSegItLst`](GE.md#ge_polsegitlst) | `GE` | [GE.md](GE.md) | nucleo | 6 | 66 |
| [`GE_PolSegParams`](GE.md#ge_polsegparams) | `GE` | [GE.md](GE.md) | nucleo | 7 | 1 |
| [`GE_POLSEGPERM`](GE.md#ge_polsegperm) | `GE` | [GE.md](GE.md) | nucleo | 5 | 805 |
| [`GE_PROCESSOWEB`](GE.md#ge_processoweb) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_QVCons`](GE.md#ge_qvcons) | `GE` | [GE.md](GE.md) | catalogo | 28 | 134 |
| [`GE_QVConsCol`](GE.md#ge_qvconscol) | `GE` | [GE.md](GE.md) | nucleo | 8 | 40 |
| [`GE_QVConsHst`](GE.md#ge_qvconshst) | `GE` | [GE.md](GE.md) | nucleo | 5 | 69 |
| [`GE_QVConsVar`](GE.md#ge_qvconsvar) | `GE` | [GE.md](GE.md) | nucleo | 17 | 581 |
| [`GE_QVPasta`](GE.md#ge_qvpasta) | `GE` | [GE.md](GE.md) | nucleo | 4 | 28 |
| [`GE_QVPastaCons`](GE.md#ge_qvpastacons) | `GE` | [GE.md](GE.md) | nucleo | 2 | 113 |
| [`GE_QVRegra`](GE.md#ge_qvregra) | `GE` | [GE.md](GE.md) | isolada | 7 | 234 |
| [`GE_Regiao`](GE.md#ge_regiao) | `GE` | [GE.md](GE.md) | vazia | 6 | 0 |
| [`GE_RelPasta`](GE.md#ge_relpasta) | `GE` | [GE.md](GE.md) | vazia | 3 | 0 |
| [`GE_RelPastaCons`](GE.md#ge_relpastacons) | `GE` | [GE.md](GE.md) | vazia | 2 | 0 |
| [`GE_RELTEMPLATE`](GE.md#ge_reltemplate) | `GE` | [GE.md](GE.md) | vazia | 6 | 0 |
| [`GE_Rota`](GE.md#ge_rota) | `GE` | [GE.md](GE.md) | vazia | 6 | 0 |
| [`GE_Sequencia`](GE.md#ge_sequencia) | `GE` | [GE.md](GE.md) | isolada | 2 | 77 |
| [`GE_Sistema`](GE.md#ge_sistema) | `GE` | [GE.md](GE.md) | nucleo | 3 | 11 |
| [`GE_SyncParam`](GE.md#ge_syncparam) | `GE` | [GE.md](GE.md) | isolada | 10 | 635 |
| [`GE_Tab`](GE.md#ge_tab) | `GE` | [GE.md](GE.md) | catalogo | 2 | 12 |
| [`GE_TabRegra`](GE.md#ge_tabregra) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_TempLong`](GE.md#ge_templong) | `GE` | [GE.md](GE.md) | isolada | 3 | 6 |
| [`GE_TipoLogradouro`](GE.md#ge_tipologradouro) | `GE` | [GE.md](GE.md) | isolada | 3 | 124 |
| [`GE_URACENARIO`](GE.md#ge_uracenario) | `GE` | [GE.md](GE.md) | vazia | 15 | 0 |
| [`GE_UsrAcSp`](GE.md#ge_usracsp) | `GE` | [GE.md](GE.md) | vazia | 7 | 0 |
| [`GE_UsrCtrl`](GE.md#ge_usrctrl) | `GE` | [GE.md](GE.md) | vazia | 7 | 0 |
| [`GE_UsrParam`](GE.md#ge_usrparam) | `GE` | [GE.md](GE.md) | nucleo | 7 | 16.827 |
| [`GE_Usuario`](GE.md#ge_usuario) | `GE` | [GE.md](GE.md) | catalogo | 35 | 1.379 |
| [`GE_USUARIOCMPL`](GE.md#ge_usuariocmpl) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_UsuarioLink`](GE.md#ge_usuariolink) | `GE` | [GE.md](GE.md) | vazia | 5 | 0 |
| [`GE_UsuarioPerm`](GE.md#ge_usuarioperm) | `GE` | [GE.md](GE.md) | isolada | 5 | 38.601 |
| [`GE_UsuarioPerm_BKPJUN`](GE.md#ge_usuarioperm_bkpjun) | `GE` | [GE.md](GE.md) | lixo/backup | 5 | 31.120 |
| [`GE_UsuarioSenhaMem`](GE.md#ge_usuariosenhamem) | `GE` | [GE.md](GE.md) | isolada | 3 | 2.456 |
| [`GE_Usuario_BKP20250520`](GE.md#ge_usuario_bkp20250520) | `GE` | [GE.md](GE.md) | lixo/backup | 35 | 1.302 |
| [`IMP_EMAIL`](IMP.md#imp_email) | `IMP` | [IMP.md](IMP.md) | staging | 13 | 0 |
| [`IMP_Evento`](IMP.md#imp_evento) | `IMP` | [IMP.md](IMP.md) | staging | 26 | 0 |
| [`IMP_FORMAPGTO`](IMP.md#imp_formapgto) | `IMP` | [IMP.md](IMP.md) | staging | 12 | 0 |
| [`IMP_LINKPESSOA`](IMP.md#imp_linkpessoa) | `IMP` | [IMP.md](IMP.md) | staging | 4 | 20.757 |
| [`IMP_LOGAPROVACAO`](IMP.md#imp_logaprovacao) | `IMP` | [IMP.md](IMP.md) | staging | 2 | 0 |
| [`IMP_NFS`](IMP.md#imp_nfs) | `IMP` | [IMP.md](IMP.md) | staging | 45 | 18.378 |
| [`IMP_NFSCMPL`](IMP.md#imp_nfscmpl) | `IMP` | [IMP.md](IMP.md) | staging | 9 | 0 |
| [`IMP_NFSItem`](IMP.md#imp_nfsitem) | `IMP` | [IMP.md](IMP.md) | staging | 40 | 67.404 |
| [`IMP_OS`](IMP.md#imp_os) | `IMP` | [IMP.md](IMP.md) | staging | 46 | 287.868 |
| [`IMP_OSITEM`](IMP.md#imp_ositem) | `IMP` | [IMP.md](IMP.md) | staging | 24 | 399.040 |
| [`IMP_OSSOLIC`](IMP.md#imp_ossolic) | `IMP` | [IMP.md](IMP.md) | staging | 11 | 0 |
| [`IMP_Pedido`](IMP.md#imp_pedido) | `IMP` | [IMP.md](IMP.md) | staging | 61 | 0 |
| [`IMP_PedidoItem`](IMP.md#imp_pedidoitem) | `IMP` | [IMP.md](IMP.md) | staging | 30 | 0 |
| [`IMP_PESSOA`](IMP.md#imp_pessoa) | `IMP` | [IMP.md](IMP.md) | staging | 76 | 0 |
| [`IMP_PESSOACONTATO`](IMP.md#imp_pessoacontato) | `IMP` | [IMP.md](IMP.md) | staging | 27 | 0 |
| [`IMP_PESSOAFONE`](IMP.md#imp_pessoafone) | `IMP` | [IMP.md](IMP.md) | staging | 19 | 0 |
| [`IMP_PRODUTO`](IMP.md#imp_produto) | `IMP` | [IMP.md](IMP.md) | staging | 29 | 0 |
| [`IMP_REL_TBA101`](IMP.md#imp_rel_tba101) | `IMP` | [IMP.md](IMP.md) | staging | 29 | 318 |
| [`IMP_REL_TBA101_PG2`](IMP.md#imp_rel_tba101_pg2) | `IMP` | [IMP.md](IMP.md) | staging | 31 | 208 |
| [`IMP_Titulo`](IMP.md#imp_titulo) | `IMP` | [IMP.md](IMP.md) | staging | 42 | 823.952 |
| [`IMP_Titulo_bkp_11_04`](IMP.md#imp_titulo_bkp_11_04) | `IMP` | [IMP.md](IMP.md) | lixo/backup | 42 | 2.168 |
| [`IMP_VEICULO`](IMP.md#imp_veiculo) | `IMP` | [IMP.md](IMP.md) | staging | 49 | 0 |
| [`IMP_VEICULO_INTEGRADO`](IMP.md#imp_veiculo_integrado) | `IMP` | [IMP.md](IMP.md) | staging | 50 | 0 |
| [`IVC_ATENDENTE`](IVC.md#ivc_atendente) | `IVC` | [IVC.md](IVC.md) | vazia | 18 | 0 |
| [`IVC_ATENDENTELOG`](IVC.md#ivc_atendentelog) | `IVC` | [IVC.md](IVC.md) | vazia | 6 | 0 |
| [`IVC_CHAMADALOG`](IVC.md#ivc_chamadalog) | `IVC` | [IVC.md](IVC.md) | vazia | 22 | 0 |
| [`IVC_EQUIPE`](IVC.md#ivc_equipe) | `IVC` | [IVC.md](IVC.md) | vazia | 3 | 0 |
| [`IVC_EQUIPEUSR`](IVC.md#ivc_equipeusr) | `IVC` | [IVC.md](IVC.md) | vazia | 3 | 0 |
| [`IVC_RAMAL`](IVC.md#ivc_ramal) | `IVC` | [IVC.md](IVC.md) | vazia | 5 | 0 |
| [`IVF_Acordo`](IVF.md#ivf_acordo) | `IVF` | [IVF.md](IVF.md) | vazia | 10 | 0 |
| [`IVF_Agregado`](IVF.md#ivf_agregado) | `IVF` | [IVF.md](IVF.md) | vazia | 8 | 0 |
| [`IVF_AgregCC`](IVF.md#ivf_agregcc) | `IVF` | [IVF.md](IVF.md) | vazia | 8 | 0 |
| [`IVF_Financeira`](IVF.md#ivf_financeira) | `IVF` | [IVF.md](IVF.md) | vazia | 23 | 0 |
| [`IVF_FinancEmpr`](IVF.md#ivf_financempr) | `IVF` | [IVF.md](IVF.md) | vazia | 4 | 0 |
| [`IVF_FinancImpTx`](IVF.md#ivf_financimptx) | `IVF` | [IVF.md](IVF.md) | vazia | 6 | 0 |
| [`IVF_Molicar`](IVF.md#ivf_molicar) | `IVF` | [IVF.md](IVF.md) | vazia | 25 | 0 |
| [`IVF_Plano`](IVF.md#ivf_plano) | `IVF` | [IVF.md](IVF.md) | vazia | 19 | 0 |
| [`IVF_PlanoIndic`](IVF.md#ivf_planoindic) | `IVF` | [IVF.md](IVF.md) | vazia | 7 | 0 |
| [`IVF_Prazo`](IVF.md#ivf_prazo) | `IVF` | [IVF.md](IVF.md) | vazia | 3 | 0 |
| [`IVF_Proposta`](IVF.md#ivf_proposta) | `IVF` | [IVF.md](IVF.md) | vazia | 60 | 0 |
| [`IVF_PropostaHst`](IVF.md#ivf_propostahst) | `IVF` | [IVF.md](IVF.md) | vazia | 7 | 0 |
| [`IVF_PropostaObs`](IVF.md#ivf_propostaobs) | `IVF` | [IVF.md](IVF.md) | vazia | 6 | 0 |
| [`IVF_PropostaResult`](IVF.md#ivf_propostaresult) | `IVF` | [IVF.md](IVF.md) | vazia | 5 | 0 |
| [`IVF_Tabela`](IVF.md#ivf_tabela) | `IVF` | [IVF.md](IVF.md) | vazia | 24 | 0 |
| [`IVF_TabelaFiltro`](IVF.md#ivf_tabelafiltro) | `IVF` | [IVF.md](IVF.md) | vazia | 4 | 0 |
| [`IVF_TabEmpr`](IVF.md#ivf_tabempr) | `IVF` | [IVF.md](IVF.md) | vazia | 4 | 0 |
| [`IVF_TabIndice`](IVF.md#ivf_tabindice) | `IVF` | [IVF.md](IVF.md) | vazia | 6 | 0 |
| [`IVF_TipoAgregado`](IVF.md#ivf_tipoagregado) | `IVF` | [IVF.md](IVF.md) | vazia | 6 | 0 |
| [`IVF_TpAgrPessoa`](IVF.md#ivf_tpagrpessoa) | `IVF` | [IVF.md](IVF.md) | vazia | 5 | 0 |
| [`IVM_Material`](IVM.md#ivm_material) | `IVM` | [IVM.md](IVM.md) | vazia | 15 | 0 |
| [`IVM_MatPessoa`](IVM.md#ivm_matpessoa) | `IVM` | [IVM.md](IVM.md) | vazia | 9 | 0 |
| [`IVM_ProcMat`](IVM.md#ivm_procmat) | `IVM` | [IVM.md](IVM.md) | vazia | 11 | 0 |
| [`IVM_ProcMatItem`](IVM.md#ivm_procmatitem) | `IVM` | [IVM.md](IVM.md) | vazia | 14 | 0 |
| [`IVP_PedCritica`](IVP.md#ivp_pedcritica) | `IVP` | [IVP.md](IVP.md) | vazia | 9 | 0 |
| [`IVP_ProdImagem`](IVP.md#ivp_prodimagem) | `IVP` | [IVP.md](IVP.md) | vazia | 4 | 0 |
| [`IVP_TabPreco`](IVP.md#ivp_tabpreco) | `IVP` | [IVP.md](IVP.md) | vazia | 7 | 0 |
| [`IVP_Vendedor`](IVP.md#ivp_vendedor) | `IVP` | [IVP.md](IVP.md) | vazia | 11 | 0 |
| [`ivs_callcenter`](IVS.md#ivs_callcenter) | `IVS` | [IVS.md](IVS.md) | isolada | 2 | 3.475 |
| [`ivs_callcidades`](IVS.md#ivs_callcidades) | `IVS` | [IVS.md](IVS.md) | isolada | 3 | 83 |
| [`IVS_CALL_FORAREGIAO`](IVS.md#ivs_call_foraregiao) | `IVS` | [IVS.md](IVS.md) | isolada | 1 | 2.360 |
| [`IVS_CanalVenda`](IVS.md#ivs_canalvenda) | `IVS` | [IVS.md](IVS.md) | vazia | 7 | 0 |
| [`IVS_CartCategoria`](IVS.md#ivs_cartcategoria) | `IVS` | [IVS.md](IVS.md) | vazia | 2 | 0 |
| [`IVS_CartCid`](IVS.md#ivs_cartcid) | `IVS` | [IVS.md](IVS.md) | nucleo | 4 | 670 |
| [`IVS_CartDepto`](IVS.md#ivs_cartdepto) | `IVS` | [IVS.md](IVS.md) | nucleo | 4 | 205 |
| [`IVS_Carteira`](IVS.md#ivs_carteira) | `IVS` | [IVS.md](IVS.md) | catalogo | 16 | 655 |
| [`IVS_Categoria`](IVS.md#ivs_categoria) | `IVS` | [IVS.md](IVS.md) | vazia | 3 | 0 |
| [`IVS_Depto`](IVS.md#ivs_depto) | `IVS` | [IVS.md](IVS.md) | catalogo | 29 | 29 |
| [`IVS_DeptoDePara`](IVS.md#ivs_deptodepara) | `IVS` | [IVS.md](IVS.md) | vazia | 2 | 0 |
| [`IVS_DeptoEmpr`](IVS.md#ivs_deptoempr) | `IVS` | [IVS.md](IVS.md) | nucleo | 5 | 13 |
| [`IVS_DEPTOPOT`](IVS.md#ivs_deptopot) | `IVS` | [IVS.md](IVS.md) | nucleo | 8 | 126 |
| [`IVS_DeptoRes`](IVS.md#ivs_deptores) | `IVS` | [IVS.md](IVS.md) | nucleo | 7 | 63 |
| [`IVS_NegCategoria`](IVS.md#ivs_negcategoria) | `IVS` | [IVS.md](IVS.md) | vazia | 2 | 0 |
| [`IVS_Negocio`](IVS.md#ivs_negocio) | `IVS` | [IVS.md](IVS.md) | vazia | 9 | 0 |
| [`IVS_Pes`](IVS.md#ivs_pes) | `IVS` | [IVS.md](IVS.md) | nucleo | 30 | 138.584 |
| [`IVS_PES_MAQ_PECAS`](IVS.md#ivs_pes_maq_pecas) | `IVS` | [IVS.md](IVS.md) | isolada | 6 | 1.365 |
| [`IVS_PES_NELSON`](IVS.md#ivs_pes_nelson) | `IVS` | [IVS.md](IVS.md) | lixo/backup | 4 | 134.872 |
| [`IVS_PES_PNEUS`](IVS.md#ivs_pes_pneus) | `IVS` | [IVS.md](IVS.md) | isolada | 6 | 1.313 |
| [`IVS_Pes_RAO_Pneus_02_03`](IVS.md#ivs_pes_rao_pneus_02_03) | `IVS` | [IVS.md](IVS.md) | isolada | 28 | 18.261 |
| [`IVS_Regional`](IVS.md#ivs_regional) | `IVS` | [IVS.md](IVS.md) | vazia | 8 | 0 |
| [`IVS_Segm`](IVS.md#ivs_segm) | `IVS` | [IVS.md](IVS.md) | catalogo | 5 | 8 |
| [`IVS_TGLCLICLIENT`](IVS.md#ivs_tglcliclient) | `IVS` | [IVS.md](IVS.md) | isolada | 25 | 16.654 |
| [`IVS_TGLCLIFIS`](IVS.md#ivs_tglclifis) | `IVS` | [IVS.md](IVS.md) | isolada | 16 | 11.232 |
| [`IVS_TGLCLIJUR`](IVS.md#ivs_tglclijur) | `IVS` | [IVS.md](IVS.md) | isolada | 10 | 5.438 |
| [`IVS_UsrMeta`](IVS.md#ivs_usrmeta) | `IVS` | [IVS.md](IVS.md) | nucleo | 13 | 1 |
| [`IVT_DePara`](IVT.md#ivt_depara) | `IVT` | [IVT.md](IVT.md) | isolada | 8 | 12 |
| [`IV_Acao`](IV-3-catalogos-bpm.md#iv_acao) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | catalogo | 37 | 979 |
| [`IV_ACAOANEXA`](IV-3-catalogos-bpm.md#iv_acaoanexa) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 11 | 0 |
| [`IV_AcaoAtendente`](IV-3-catalogos-bpm.md#iv_acaoatendente) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 6 | 0 |
| [`IV_AcaoAuto`](IV-3-catalogos-bpm.md#iv_acaoauto) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 31 | 5.942 |
| [`IV_AcaoAutoCtrl`](IV-3-catalogos-bpm.md#iv_acaoautoctrl) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 6 | 0 |
| [`IV_AcaoCmpl`](IV-3-catalogos-bpm.md#iv_acaocmpl) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 2 | 0 |
| [`IV_AcaoCtrl`](IV-3-catalogos-bpm.md#iv_acaoctrl) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 13 | 44 |
| [`IV_AcaoMon`](IV-3-catalogos-bpm.md#iv_acaomon) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 9 | 0 |
| [`IV_AcaoRem`](IV-3-catalogos-bpm.md#iv_acaorem) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 9 | 266 |
| [`IV_ACAOURACENARIO`](IV-3-catalogos-bpm.md#iv_acaouracenario) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 2 | 0 |
| [`IV_AgdLink`](IV-1-processo-agenda-historico.md#iv_agdlink) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 11 | 4.534 |
| [`IV_AGDPLANACAO`](IV-1-processo-agenda-historico.md#iv_agdplanacao) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 7 | 0 |
| [`IV_AGDPLANTRG`](IV-1-processo-agenda-historico.md#iv_agdplantrg) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 42 | 0 |
| [`IV_AgdRec`](IV-1-processo-agenda-historico.md#iv_agdrec) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 9 | 0 |
| [`IV_AgdUsr`](IV-1-processo-agenda-historico.md#iv_agdusr) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 4 | 0 |
| [`IV_Agenda`](IV-1-processo-agenda-historico.md#iv_agenda) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 40 | 931.989 |
| [`IV_AGENDACMPL`](IV-1-processo-agenda-historico.md#iv_agendacmpl) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 4 | 0 |
| [`IV_AgendaCtrl`](IV-1-processo-agenda-historico.md#iv_agendactrl) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 6 | 963 |
| [`IV_AGENDAITEM`](IV-1-processo-agenda-historico.md#iv_agendaitem) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 7 | 0 |
| [`IV_AgendaLog`](IV-1-processo-agenda-historico.md#iv_agendalog) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 9 | 11.049.475 |
| [`IV_Agenda_bkp20250717`](IV-1-processo-agenda-historico.md#iv_agenda_bkp20250717) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | lixo/backup | 40 | 1 |
| [`IV_AtdBloq`](IV-1-processo-agenda-historico.md#iv_atdbloq) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 10 | 0 |
| [`IV_Atendente`](IV-4-demais.md#iv_atendente) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 2 | 0 |
| [`IV_Ativ`](IV-4-demais.md#iv_ativ) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 17 | 0 |
| [`IV_AtivAgenda`](IV-1-processo-agenda-historico.md#iv_ativagenda) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 6 | 0 |
| [`IV_ATIVGRUPO`](IV-4-demais.md#iv_ativgrupo) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_ATIVMODELO`](IV-4-demais.md#iv_ativmodelo) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 3 | 0 |
| [`IV_ATIVPADRAO`](IV-4-demais.md#iv_ativpadrao) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 8 | 0 |
| [`IV_AtivProc`](IV-1-processo-agenda-historico.md#iv_ativproc) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 7 | 0 |
| [`IV_AtribLista`](IV-2-formularios.md#iv_atriblista) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 2 | 37 |
| [`IV_Atributo`](IV-2-formularios.md#iv_atributo) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 11 | 10 |
| [`IV_BaseInformacao`](IV-4-demais.md#iv_baseinformacao) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 8 | 0 |
| [`IV_BonusCC`](IV-4-demais.md#iv_bonuscc) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 8 | 0 |
| [`IV_Campanha`](IV-4-demais.md#iv_campanha) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 33 | 179 |
| [`IV_CampPesMsg`](IV-4-demais.md#iv_camppesmsg) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 22 | 0 |
| [`IV_CampPessoa`](IV-4-demais.md#iv_camppessoa) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_CAMPSELECAO`](IV-4-demais.md#iv_campselecao) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 11 | 0 |
| [`IV_CampVoucher`](IV-4-demais.md#iv_campvoucher) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 9 | 0 |
| [`IV_CartContratante`](IV-4-demais.md#iv_cartcontratante) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 14 | 0 |
| [`IV_CartCred`](IV-4-demais.md#iv_cartcred) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 36 | 0 |
| [`IV_CartLoteCartao`](IV-4-demais.md#iv_cartlotecartao) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_CartLoteImp`](IV-4-demais.md#iv_cartloteimp) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 7 | 0 |
| [`IV_CartMarca`](IV-4-demais.md#iv_cartmarca) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_CARTPESORIGEM`](IV-4-demais.md#iv_cartpesorigem) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_CARTPESSOA`](IV-4-demais.md#iv_cartpessoa) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 53 | 0 |
| [`IV_CartProduto`](IV-4-demais.md#iv_cartproduto) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 9 | 0 |
| [`IV_CartProposta`](IV-4-demais.md#iv_cartproposta) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 30 | 0 |
| [`IV_CartTitular`](IV-4-demais.md#iv_carttitular) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 12 | 0 |
| [`IV_CbrCobranca`](IV-4-demais.md#iv_cbrcobranca) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 8 | 64.449 |
| [`IV_CBRCOBRANCAHST`](IV-4-demais.md#iv_cbrcobrancahst) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_CbrCobrancaLote`](IV-4-demais.md#iv_cbrcobrancalote) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 4 | 21.048 |
| [`IV_CbrCobrancaMon`](IV-4-demais.md#iv_cbrcobrancamon) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 11 | 0 |
| [`IV_CbrCobrancaTit`](IV-4-demais.md#iv_cbrcobrancatit) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 10 | 155.204 |
| [`IV_CbrCobrancaTitLog`](IV-4-demais.md#iv_cbrcobrancatitlog) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 5 | 304.684 |
| [`IV_CbrCriterio`](IV-4-demais.md#iv_cbrcriterio) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | catalogo | 26 | 122 |
| [`IV_CbrCriterioDef`](IV-4-demais.md#iv_cbrcriteriodef) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 16 | 231 |
| [`IV_CBRCRITERIOMSG`](IV-4-demais.md#iv_cbrcriteriomsg) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 6 | 3 |
| [`IV_CBRCRITMON`](IV-4-demais.md#iv_cbrcritmon) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 17 | 128.167 |
| [`IV_CbrTitulo`](IV-4-demais.md#iv_cbrtitulo) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_CBRTITULOHST`](IV-4-demais.md#iv_cbrtitulohst) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 5 | 173.530 |
| [`IV_CFinDocAceito`](IV-4-demais.md#iv_cfindocaceito) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_Ciencia`](IV-1-processo-agenda-historico.md#iv_ciencia) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 5 | 173.477 |
| [`IV_ClasseRes`](IV-3-catalogos-bpm.md#iv_classeres) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 5 | 40 |
| [`IV_ClienteAtrib`](IV-2-formularios.md#iv_clienteatrib) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | isolada | 6 | 2.018 |
| [`IV_ClientePropCmpl`](IV-2-formularios.md#iv_clientepropcmpl) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | vazia | 2 | 0 |
| [`IV_ClientePropr`](IV-2-formularios.md#iv_clientepropr) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 51 | 246.684 |
| [`IV_ClientePropr_BKPJUN`](IV-2-formularios.md#iv_clientepropr_bkpjun) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | lixo/backup | 51 | 1.000 |
| [`IV_ClientePropr_ITA`](IV-2-formularios.md#iv_clientepropr_ita) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | lixo/backup | 51 | 157.900 |
| [`IV_CobrCrit`](IV-4-demais.md#iv_cobrcrit) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 11 | 0 |
| [`IV_CobrCritAgd`](IV-4-demais.md#iv_cobrcritagd) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_CobrCritDef`](IV-4-demais.md#iv_cobrcritdef) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 10 | 0 |
| [`IV_CobrCritMon`](IV-4-demais.md#iv_cobrcritmon) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 17 | 0 |
| [`IV_CobrTit`](IV-4-demais.md#iv_cobrtit) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 11 | 0 |
| [`IV_CodPrcEmpr`](IV-3-catalogos-bpm.md#iv_codprcempr) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 2 | 17 |
| [`IV_CodProcComent`](IV-3-catalogos-bpm.md#iv_codproccoment) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 4 | 1 |
| [`IV_CodProcesso`](IV-3-catalogos-bpm.md#iv_codprocesso) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | catalogo | 35 | 62 |
| [`IV_ConhecFonema`](IV-4-demais.md#iv_conhecfonema) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 2 | 19 |
| [`IV_Conhecimento`](IV-4-demais.md#iv_conhecimento) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | catalogo | 19 | 9 |
| [`IV_ConhecLeitura`](IV-4-demais.md#iv_conhecleitura) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 5 | 49 |
| [`IV_CustoMidia`](IV-4-demais.md#iv_customidia) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 9 | 0 |
| [`IV_Departamento`](IV-3-catalogos-bpm.md#iv_departamento) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 3 | 0 |
| [`IV_Distribui`](IV-1-processo-agenda-historico.md#iv_distribui) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 3 | 609 |
| [`IV_DoctoApl`](IV-3-catalogos-bpm.md#iv_doctoapl) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 15 | 6 |
| [`IV_DoctoAplUso`](IV-3-catalogos-bpm.md#iv_doctoapluso) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 10 | 3 |
| [`IV_DoctoTipo`](IV-3-catalogos-bpm.md#iv_doctotipo) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 5 | 3 |
| [`IV_eMail`](IV-4-demais.md#iv_email) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 15 | 0 |
| [`IV_EMAILESTATISTICA`](IV-4-demais.md#iv_emailestatistica) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 20 | 0 |
| [`IV_ESTRPRODUTO`](IV-4-demais.md#iv_estrproduto) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 6 | 0 |
| [`IV_Evento`](IV-3-catalogos-bpm.md#iv_evento) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | isolada | 27 | 132 |
| [`IV_EventoAcao`](IV-3-catalogos-bpm.md#iv_eventoacao) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 7 | 0 |
| [`IV_FichaNegVeic`](IV-4-demais.md#iv_fichanegveic) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 79 | 0 |
| [`IV_FoneCtrl`](IV-4-demais.md#iv_fonectrl) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 6 | 0 |
| [`IV_FoneCtrl2`](IV-4-demais.md#iv_fonectrl2) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 6 | 0 |
| [`IV_FoneCtrlHst`](IV-4-demais.md#iv_fonectrlhst) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 7 | 0 |
| [`IV_Formulario`](IV-2-formularios.md#iv_formulario) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | catalogo | 16 | 176 |
| [`IV_GlobalPar`](IV-3-catalogos-bpm.md#iv_globalpar) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 43 | 3.848 |
| [`IV_GlobalParCtrl`](IV-3-catalogos-bpm.md#iv_globalparctrl) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | catalogo | 54 | 48 |
| [`IV_GlobalParLista`](IV-3-catalogos-bpm.md#iv_globalparlista) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 4 | 197 |
| [`IV_HistInfo`](IV-1-processo-agenda-historico.md#iv_histinfo) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 2 | 17.250 |
| [`IV_HistLink`](IV-1-processo-agenda-historico.md#iv_histlink) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 5 | 645.850 |
| [`IV_Historico`](IV-1-processo-agenda-historico.md#iv_historico) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 29 | 2.436.127 |
| [`IV_HistoricoNota`](IV-1-processo-agenda-historico.md#iv_historiconota) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 2 | 94.315 |
| [`IV_HISTORICOTAG`](IV-1-processo-agenda-historico.md#iv_historicotag) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 4 | 0 |
| [`IV_Interacao`](IV-1-processo-agenda-historico.md#iv_interacao) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 4 | 720.525 |
| [`IV_LEADFACEBOOK`](IV-4-demais.md#iv_leadfacebook) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 21 | 0 |
| [`IV_LEADFACEITEM`](IV-4-demais.md#iv_leadfaceitem) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 6 | 0 |
| [`IV_ListSQL`](IV-2-formularios.md#iv_listsql) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | isolada | 5 | 360 |
| [`IV_Motivo`](IV-3-catalogos-bpm.md#iv_motivo) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | isolada | 4 | 43 |
| [`IV_ObjFlow`](IV-4-demais.md#iv_objflow) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 9 | 726 |
| [`IV_ObjVenda`](IV-4-demais.md#iv_objvenda) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 13 | 0 |
| [`IV_OcrmAgd`](IV-4-demais.md#iv_ocrmagd) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 8 | 18 |
| [`IV_OcrmDest`](IV-4-demais.md#iv_ocrmdest) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 12 | 17 |
| [`IV_OpBloq`](IV-4-demais.md#iv_opbloq) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 8 | 4 |
| [`IV_Operador`](IV-4-demais.md#iv_operador) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | catalogo | 51 | 932 |
| [`IV_OPTEMAIL`](IV-4-demais.md#iv_optemail) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 7 | 21 |
| [`IV_OPTFONE`](IV-4-demais.md#iv_optfone) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 7 | 2 |
| [`IV_OS`](IV-4-demais.md#iv_os) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 22 | 0 |
| [`IV_PAREVTEXTRES`](IV-3-catalogos-bpm.md#iv_parevtextres) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 7 | 0 |
| [`IV_Pcte`](IV-2-formularios.md#iv_pcte) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | isolada | 3 | 1 |
| [`IV_PcteAtrFx`](IV-2-formularios.md#iv_pcteatrfx) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 3 | 22 |
| [`IV_Pessoa`](IV-4-demais.md#iv_pessoa) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 6 | 0 |
| [`IV_PessoaStat`](IV-4-demais.md#iv_pessoastat) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 3 | 0 |
| [`IV_PlanoAtiv`](IV-4-demais.md#iv_planoativ) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 10 | 0 |
| [`IV_ProcAcao`](IV-1-processo-agenda-historico.md#iv_procacao) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | catalogo | 3 | 738 |
| [`IV_ProcAtiv`](IV-1-processo-agenda-historico.md#iv_procativ) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 6 | 0 |
| [`IV_ProcComent`](IV-1-processo-agenda-historico.md#iv_proccoment) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 3 | 0 |
| [`IV_ProcDado`](IV-1-processo-agenda-historico.md#iv_procdado) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 24 | 1.516.214 |
| [`IV_ProcDocto`](IV-1-processo-agenda-historico.md#iv_procdocto) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 5 | 73.646 |
| [`IV_Processo`](IV-1-processo-agenda-historico.md#iv_processo) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 22 | 1.174.932 |
| [`IV_Processo_bkp20250717`](IV-1-processo-agenda-historico.md#iv_processo_bkp20250717) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | lixo/backup | 22 | 1 |
| [`IV_ProcFase`](IV-3-catalogos-bpm.md#iv_procfase) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 5 | 273 |
| [`IV_ProcFaseMonit`](IV-3-catalogos-bpm.md#iv_procfasemonit) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | isolada | 9 | 655.144 |
| [`IV_ProcLink`](IV-1-processo-agenda-historico.md#iv_proclink) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 7 | 602.150 |
| [`IV_ProcPersp`](IV-3-catalogos-bpm.md#iv_procpersp) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 4 | 67 |
| [`IV_ProcPerspMonit`](IV-3-catalogos-bpm.md#iv_procperspmonit) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | isolada | 6 | 126.145 |
| [`IV_PROCPESLINK`](IV-1-processo-agenda-historico.md#iv_procpeslink) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 2 | 0 |
| [`IV_ProcProduto`](IV-1-processo-agenda-historico.md#iv_procproduto) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 18 | 62.578 |
| [`IV_ProcProjeto`](IV-1-processo-agenda-historico.md#iv_procprojeto) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 4 | 0 |
| [`IV_ProcRef`](IV-1-processo-agenda-historico.md#iv_procref) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | nucleo | 5 | 61.975 |
| [`IV_ProcRelacao`](IV-1-processo-agenda-historico.md#iv_procrelacao) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 6 | 0 |
| [`IV_ProcResultado`](IV-3-catalogos-bpm.md#iv_procresultado) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 5 | 2.153 |
| [`IV_ProcSt`](IV-3-catalogos-bpm.md#iv_procst) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 4 | 250 |
| [`IV_ProcStatMonit`](IV-3-catalogos-bpm.md#iv_procstatmonit) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | isolada | 6 | 850.022 |
| [`IV_PROCTAG`](IV-1-processo-agenda-historico.md#iv_proctag) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | vazia | 4 | 0 |
| [`IV_ProcTpRel`](IV-3-catalogos-bpm.md#iv_proctprel) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 5 | 0 |
| [`IV_ProcVinc`](IV-1-processo-agenda-historico.md#iv_procvinc) | `IV-1` | [IV-1-processo-agenda-historico.md](IV-1-processo-agenda-historico.md) | isolada | 6 | 190 |
| [`IV_Produto`](IV-4-demais.md#iv_produto) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 24 | 1 |
| [`IV_ProjColec`](IV-4-demais.md#iv_projcolec) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_ProjDocto`](IV-4-demais.md#iv_projdocto) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_ProjEquipe`](IV-4-demais.md#iv_projequipe) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_Projeto`](IV-4-demais.md#iv_projeto) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 17 | 0 |
| [`IV_ProjPessoa`](IV-4-demais.md#iv_projpessoa) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 4 | 0 |
| [`IV_Propriedade`](IV-2-formularios.md#iv_propriedade) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | catalogo | 70 | 52 |
| [`IV_Propriedade_BKPJUN`](IV-2-formularios.md#iv_propriedade_bkpjun) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | lixo/backup | 70 | 21 |
| [`IV_Propriedade_ITA`](IV-2-formularios.md#iv_propriedade_ita) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | lixo/backup | 70 | 32 |
| [`IV_PropriLista`](IV-2-formularios.md#iv_proprilista) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 5 | 7.375 |
| [`IV_PropriListaLk`](IV-2-formularios.md#iv_proprilistalk) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | vazia | 5 | 0 |
| [`IV_PropriLista_ITA`](IV-2-formularios.md#iv_proprilista_ita) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | lixo/backup | 5 | 7.520 |
| [`IV_PUSH`](IV-4-demais.md#iv_push) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 15 | 0 |
| [`IV_Questao`](IV-2-formularios.md#iv_questao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 19 | 2.300 |
| [`IV_QuestaoLista`](IV-2-formularios.md#iv_questaolista) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 8 | 3.995 |
| [`IV_Questionario`](IV-2-formularios.md#iv_questionario) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | nucleo | 18 | 85.393 |
| [`IV_Q_ABERTURA_OS_REVISAO`](IV-2-formularios.md#iv_q_abertura_os_revisao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 10 |
| [`IV_Q_ACOMPANHAMENTO_VENDA`](IV-2-formularios.md#iv_q_acompanhamento_venda) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 53 | 1.715 |
| [`IV_Q_ACOMPANHAM_VENDA_IMP`](IV-2-formularios.md#iv_q_acompanham_venda_imp) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 39 | 3.308 |
| [`IV_Q_ACOMPANHA_COMPRA_IMP`](IV-2-formularios.md#iv_q_acompanha_compra_imp) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 16 | 26 |
| [`IV_Q_ACOMPANH_VENDA_JDE`](IV-2-formularios.md#iv_q_acompanh_venda_jde) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 104 | 6.343 |
| [`IV_Q_ACOMPAN_COMPRA_JDE`](IV-2-formularios.md#iv_q_acompan_compra_jde) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 17 | 153 |
| [`IV_Q_ACOMPAN_VEND_CONCESS`](IV-2-formularios.md#iv_q_acompan_vend_concess) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 98 |
| [`IV_Q_ACOMP_VENDA_DIRETA`](IV-2-formularios.md#iv_q_acomp_venda_direta) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 20 | 125 |
| [`IV_Q_ACOMP_VENDA_DIRETAJD`](IV-2-formularios.md#iv_q_acomp_venda_diretajd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 94 | 270 |
| [`IV_Q_ACOMP_VENDA_FINANC`](IV-2-formularios.md#iv_q_acomp_venda_financ) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 2.021 |
| [`IV_Q_ACOMP_VENDA_LOCACAO`](IV-2-formularios.md#iv_q_acomp_venda_locacao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 14 | 15 |
| [`IV_Q_ACOMP_VENDA_MANITOU`](IV-2-formularios.md#iv_q_acomp_venda_manitou) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 45 | 21 |
| [`IV_Q_ACOMP_VENDA_USADO`](IV-2-formularios.md#iv_q_acomp_venda_usado) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 44 | 256 |
| [`IV_Q_ACOMP_VEND_MAQUINAS`](IV-2-formularios.md#iv_q_acomp_vend_maquinas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 54 | 1.535 |
| [`IV_Q_ADM_FINANCEIRO`](IV-2-formularios.md#iv_q_adm_financeiro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 14 | 1.884 |
| [`IV_Q_AFERICAO_CSC`](IV-2-formularios.md#iv_q_afericao_csc) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 32 |
| [`IV_Q_AFERICAO_DE_PECAS`](IV-2-formularios.md#iv_q_afericao_de_pecas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 2 |
| [`IV_Q_AFERICAO_IMPLEMENTO`](IV-2-formularios.md#iv_q_afericao_implemento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 440 |
| [`IV_Q_AFERICAO_MAQ_1_CONT`](IV-2-formularios.md#iv_q_afericao_maq_1_cont) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 873 |
| [`IV_Q_AFERICAO_MAQ_1_WEB`](IV-2-formularios.md#iv_q_afericao_maq_1_web) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 2 |
| [`IV_Q_AFERICAO_MAQ_2_CONT`](IV-2-formularios.md#iv_q_afericao_maq_2_cont) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 541 |
| [`IV_Q_AFERICAO_MAQ_2_WEB`](IV-2-formularios.md#iv_q_afericao_maq_2_web) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 1 |
| [`IV_Q_AFERICAO_MAQ_3_CONT`](IV-2-formularios.md#iv_q_afericao_maq_3_cont) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 484 |
| [`IV_Q_AFERICAO_MAQ_3_WEB`](IV-2-formularios.md#iv_q_afericao_maq_3_web) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 3 |
| [`IV_Q_AFERICAO_PECAS`](IV-2-formularios.md#iv_q_afericao_pecas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 33 | 1.719 |
| [`IV_Q_AFERICAO_POS_SOLUCAO`](IV-2-formularios.md#iv_q_afericao_pos_solucao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 452 |
| [`IV_Q_AFERICAO_SERVICOS`](IV-2-formularios.md#iv_q_afericao_servicos) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 16 | 635 |
| [`IV_Q_AFERICAO_SERV_WEB`](IV-2-formularios.md#iv_q_afericao_serv_web) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 17 | 31 |
| [`IV_Q_AFERICAO_SUPORTE_INT`](IV-2-formularios.md#iv_q_afericao_suporte_int) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 0 |
| [`IV_Q_AFERICAO_VENDA_MAQ`](IV-2-formularios.md#iv_q_afericao_venda_maq) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 979 |
| [`IV_Q_AFERICAO_VENDA_MQ_IM`](IV-2-formularios.md#iv_q_afericao_venda_mq_im) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 32 | 604 |
| [`IV_Q_AFE_VENDA_MQ_IM_USAD`](IV-2-formularios.md#iv_q_afe_venda_mq_im_usad) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 26 | 1 |
| [`IV_Q_AGUARDAR_PECAS`](IV-2-formularios.md#iv_q_aguardar_pecas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 31 |
| [`IV_Q_ALTERADO_PAGAMENTO`](IV-2-formularios.md#iv_q_alterado_pagamento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 0 |
| [`IV_Q_APRESENTACAO`](IV-2-formularios.md#iv_q_apresentacao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 211 |
| [`IV_Q_APRESENTACAO_JDE`](IV-2-formularios.md#iv_q_apresentacao_jde) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 123 |
| [`IV_Q_APRESENT_EQUIPAMENTO`](IV-2-formularios.md#iv_q_apresent_equipamento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 1 | 0 |
| [`IV_Q_APRESENT_IMPLEMENTO`](IV-2-formularios.md#iv_q_apresent_implemento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 7 | 43 |
| [`IV_Q_APROVACAO_TCSM`](IV-2-formularios.md#iv_q_aprovacao_tcsm) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 27 |
| [`IV_Q_ATUALIZACAO_PUK`](IV-2-formularios.md#iv_q_atualizacao_puk) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 0 |
| [`IV_Q_AVALIACAO_AMS_USADO`](IV-2-formularios.md#iv_q_avaliacao_ams_usado) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 4 |
| [`IV_Q_AVALIA_COLHEIT_USADA`](IV-2-formularios.md#iv_q_avalia_colheit_usada) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 51 | 1 |
| [`IV_Q_AVALIA_IMPLEM_USADO`](IV-2-formularios.md#iv_q_avalia_implem_usado) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 38 | 60 |
| [`IV_Q_AVALIA_USADO_ENTRADA`](IV-2-formularios.md#iv_q_avalia_usado_entrada) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 35 | 823 |
| [`IV_Q_AVAL_COLH_CANA_USADA`](IV-2-formularios.md#iv_q_aval_colh_cana_usada) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 32 | 1 |
| [`IV_Q_AVAL_TRATORES_USADOS`](IV-2-formularios.md#iv_q_aval_tratores_usados) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 53 | 0 |
| [`IV_Q_AVAL_USADO_ENTRADA`](IV-2-formularios.md#iv_q_aval_usado_entrada) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 200 |
| [`IV_Q_CADASTROS_LISTAS`](IV-2-formularios.md#iv_q_cadastros_listas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 0 |
| [`IV_Q_CANCELAMENTO_SEGURO`](IV-2-formularios.md#iv_q_cancelamento_seguro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 4 |
| [`IV_Q_CANCEL_RENOVACAO_SEG`](IV-2-formularios.md#iv_q_cancel_renovacao_seg) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 0 |
| [`IV_Q_CANHOTO_DIGITAL`](IV-2-formularios.md#iv_q_canhoto_digital) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 0 |
| [`IV_Q_CHASSI_ENTREGA_FISIC`](IV-2-formularios.md#iv_q_chassi_entrega_fisic) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 1.064 |
| [`IV_Q_CHASSI_EQUIPAMENTO`](IV-2-formularios.md#iv_q_chassi_equipamento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 1.039 |
| [`IV_Q_CHASSI_PMP`](IV-2-formularios.md#iv_q_chassi_pmp) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 767 |
| [`IV_Q_CHEGADA_IMPLEMENTO`](IV-2-formularios.md#iv_q_chegada_implemento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 61 |
| [`IV_Q_COMISSAO`](IV-2-formularios.md#iv_q_comissao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 36 | 7.165 |
| [`IV_Q_COMISSAO_AMS`](IV-2-formularios.md#iv_q_comissao_ams) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 20 | 633 |
| [`IV_Q_COMISSAO_CONTACHAVE`](IV-2-formularios.md#iv_q_comissao_contachave) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 18 | 144 |
| [`IV_Q_COMISSAO_SERV_AMS`](IV-2-formularios.md#iv_q_comissao_serv_ams) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 15 | 448 |
| [`IV_Q_COMISSAO_USADO`](IV-2-formularios.md#iv_q_comissao_usado) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 240 |
| [`IV_Q_COMISSAO_VD_LOCACAO`](IV-2-formularios.md#iv_q_comissao_vd_locacao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 0 |
| [`IV_Q_COMPETIDORES_NA_NEG`](IV-2-formularios.md#iv_q_competidores_na_neg) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 104 |
| [`IV_Q_COMPETIDORES_NA_NEGO`](IV-2-formularios.md#iv_q_competidores_na_nego) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 101 |
| [`IV_Q_COMPETID_NEGOC_IMPL`](IV-2-formularios.md#iv_q_competid_negoc_impl) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 13 |
| [`IV_Q_COM_INTERESSE_FUTURO`](IV-2-formularios.md#iv_q_com_interesse_futuro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 0 |
| [`IV_Q_CONDICOES_DE_VENDAS`](IV-2-formularios.md#iv_q_condicoes_de_vendas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 11 | 0 |
| [`IV_Q_CONT_COMISSAO_22`](IV-2-formularios.md#iv_q_cont_comissao_22) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 18 | 2.251 |
| [`IV_Q_COTA_CONSORCIO`](IV-2-formularios.md#iv_q_cota_consorcio) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 73 |
| [`IV_Q_DEMONSTRACAO_JD`](IV-2-formularios.md#iv_q_demonstracao_jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 33 | 215 |
| [`IV_Q_DEMONSTRACAO_LOG`](IV-2-formularios.md#iv_q_demonstracao_log) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 25 |
| [`IV_Q_DEMONSTRACAO_NF`](IV-2-formularios.md#iv_q_demonstracao_nf) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 44 |
| [`IV_Q_DEMONSTRACAO_TRATOR`](IV-2-formularios.md#iv_q_demonstracao_trator) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 25 | 32 |
| [`IV_Q_DEMONSTR_COLHEITAD`](IV-2-formularios.md#iv_q_demonstr_colheitad) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 0 |
| [`IV_Q_DEMO_EQUIP_JD`](IV-2-formularios.md#iv_q_demo_equip_jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 23 | 0 |
| [`IV_Q_DEMO_IMPLEMENTO`](IV-2-formularios.md#iv_q_demo_implemento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 25 | 0 |
| [`IV_Q_DEMO_MAQUINAS`](IV-2-formularios.md#iv_q_demo_maquinas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 19 | 109 |
| [`IV_Q_DEMO_TRATOR`](IV-2-formularios.md#iv_q_demo_trator) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 26 | 43 |
| [`IV_Q_DEVOLUCAO_PECA`](IV-2-formularios.md#iv_q_devolucao_peca) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 581 |
| [`IV_Q_DEVOLUCAO_PUK`](IV-2-formularios.md#iv_q_devolucao_puk) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 5 |
| [`IV_Q_DOC_ANALISE_CREDITO`](IV-2-formularios.md#iv_q_doc_analise_credito) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 28 | 21 |
| [`IV_Q_EVENTOS_AFERICAO`](IV-2-formularios.md#iv_q_eventos_afericao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 67 |
| [`IV_Q_EXP_FLUXO_MODELER`](IV-2-formularios.md#iv_q_exp_fluxo_modeler) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 0 |
| [`IV_Q_FORA_SERVICO_PMP`](IV-2-formularios.md#iv_q_fora_servico_pmp) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 0 |
| [`IV_Q_FORM_TREINO`](IV-2-formularios.md#iv_q_form_treino) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 1 | 0 |
| [`IV_Q_GAR_DATA_SERVICO`](IV-2-formularios.md#iv_q_gar_data_servico) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 4.888 |
| [`IV_Q_GAR_FAB_SOL_PECA`](IV-2-formularios.md#iv_q_gar_fab_sol_peca) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 640 |
| [`IV_Q_GESTAO_CREDITO`](IV-2-formularios.md#iv_q_gestao_credito) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 54 | 1.506 |
| [`IV_Q_GESTAO_CREDITO_AMS`](IV-2-formularios.md#iv_q_gestao_credito_ams) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 51 | 175 |
| [`IV_Q_GESTAO_CREDITO_IMP`](IV-2-formularios.md#iv_q_gestao_credito_imp) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 51 | 807 |
| [`IV_Q_GESTAO_PRODUTO_IMPL`](IV-2-formularios.md#iv_q_gestao_produto_impl) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 23 | 813 |
| [`IV_Q_GESTAO_PRODUTO___AMS`](IV-2-formularios.md#iv_q_gestao_produto___ams) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 39 | 177 |
| [`IV_Q_HORIMETRO_AGREGA`](IV-2-formularios.md#iv_q_horimetro_agrega) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 1 |
| [`IV_Q_INCENTIVO`](IV-2-formularios.md#iv_q_incentivo) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 43 | 2.079 |
| [`IV_Q_INTERESSE_FUTURO_PRO`](IV-2-formularios.md#iv_q_interesse_futuro_pro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 10 |
| [`IV_Q_INTERESSE_PROJETO_IR`](IV-2-formularios.md#iv_q_interesse_projeto_ir) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 25 |
| [`IV_Q_LIBERAR_DEMONSTRACAO`](IV-2-formularios.md#iv_q_liberar_demonstracao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 44 |
| [`IV_Q_LICENCAS_PUK`](IV-2-formularios.md#iv_q_licencas_puk) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 41 |
| [`IV_Q_LOCACAO_COMISSAO`](IV-2-formularios.md#iv_q_locacao_comissao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 15 |
| [`IV_Q_OFERECE_RENOV_SEGURO`](IV-2-formularios.md#iv_q_oferece_renov_seguro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 16 | 846 |
| [`IV_Q_ORIGEM_DA_RENDA`](IV-2-formularios.md#iv_q_origem_da_renda) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 0 |
| [`IV_Q_OS_ABERTA`](IV-2-formularios.md#iv_q_os_aberta) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 771 |
| [`IV_Q_OS_CORTESIA`](IV-2-formularios.md#iv_q_os_cortesia) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 78 |
| [`IV_Q_OS_GARANTIA`](IV-2-formularios.md#iv_q_os_garantia) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 7.786 |
| [`IV_Q_OS_REVISAO_ENTREGA`](IV-2-formularios.md#iv_q_os_revisao_entrega) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 0 |
| [`IV_Q_PECAS_AFERICAO`](IV-2-formularios.md#iv_q_pecas_afericao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 23 | 5.174 |
| [`IV_Q_PEDIDO_GC`](IV-2-formularios.md#iv_q_pedido_gc) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 0 |
| [`IV_Q_PEDIDO_KAM`](IV-2-formularios.md#iv_q_pedido_kam) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 28 | 12 |
| [`IV_Q_PEDIDO_SAM`](IV-2-formularios.md#iv_q_pedido_sam) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 19 | 46 |
| [`IV_Q_PERCEPCAO_JD`](IV-2-formularios.md#iv_q_percepcao_jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 17 | 32 |
| [`IV_Q_PESQUISA_NPS`](IV-2-formularios.md#iv_q_pesquisa_nps) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 0 |
| [`IV_Q_PESQUISA_TI`](IV-2-formularios.md#iv_q_pesquisa_ti) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 0 |
| [`IV_Q_PREMIO_DEMO`](IV-2-formularios.md#iv_q_premio_demo) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 21 |
| [`IV_Q_PREVISAO_RECEBIMENTO`](IV-2-formularios.md#iv_q_previsao_recebimento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 4 |
| [`IV_Q_PRODUTO_RD`](IV-2-formularios.md#iv_q_produto_rd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 135 |
| [`IV_Q_PROPOSTA_COMERCIAL`](IV-2-formularios.md#iv_q_proposta_comercial) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 24 |
| [`IV_Q_PROSPECCAO_SERV__JD`](IV-2-formularios.md#iv_q_prospeccao_serv__jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 104 |
| [`IV_Q_QUALIDADE_PECAS`](IV-2-formularios.md#iv_q_qualidade_pecas) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 642 |
| [`IV_Q_QUALIDADE_SERVICOS`](IV-2-formularios.md#iv_q_qualidade_servicos) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 345 |
| [`IV_Q_QUALIDADE_VENDAMAQ`](IV-2-formularios.md#iv_q_qualidade_vendamaq) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 5 | 141 |
| [`IV_Q_RECEBIMENTO_A_PRAZO`](IV-2-formularios.md#iv_q_recebimento_a_prazo) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 17 | 16 |
| [`IV_Q_RECEBIMENTO_COMISSAO`](IV-2-formularios.md#iv_q_recebimento_comissao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 0 |
| [`IV_Q_RECEBIMENTO_FINANC`](IV-2-formularios.md#iv_q_recebimento_financ) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 4 |
| [`IV_Q_RECEB_FINAN_IMPLEM`](IV-2-formularios.md#iv_q_receb_finan_implem) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 0 |
| [`IV_Q_RESPONSAVEL_TECNICO`](IV-2-formularios.md#iv_q_responsavel_tecnico) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 2.115 |
| [`IV_Q_RESULTADO_DEMO`](IV-2-formularios.md#iv_q_resultado_demo) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 15 | 23 |
| [`IV_Q_RETORNADO_JD`](IV-2-formularios.md#iv_q_retornado_jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 2 | 773 |
| [`IV_Q_REVISAO_100H`](IV-2-formularios.md#iv_q_revisao_100h) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 1 |
| [`IV_Q_REVISAO_1100_1150H`](IV-2-formularios.md#iv_q_revisao_1100_1150h) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 1 |
| [`IV_Q_REVISAO_1500H`](IV-2-formularios.md#iv_q_revisao_1500h) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 0 |
| [`IV_Q_REVISAO_450_600H`](IV-2-formularios.md#iv_q_revisao_450_600h) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 1 |
| [`IV_Q_REVISAO_800H`](IV-2-formularios.md#iv_q_revisao_800h) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 0 |
| [`IV_Q_REVISAO_FIM_GARANTIA`](IV-2-formularios.md#iv_q_revisao_fim_garantia) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 0 |
| [`IV_Q_REV_DATA_SERVICO`](IV-2-formularios.md#iv_q_rev_data_servico) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 74 |
| [`IV_Q_ROMANEIO_DEV_PECA`](IV-2-formularios.md#iv_q_romaneio_dev_peca) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 1 |
| [`IV_Q_SEPARACAO_PEDIDO`](IV-2-formularios.md#iv_q_separacao_pedido) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 18 | 2 |
| [`IV_Q_SERVICOS_AFERICAO`](IV-2-formularios.md#iv_q_servicos_afericao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 18 | 2.204 |
| [`IV_Q_SERVICO_EXTERNOS_JD`](IV-2-formularios.md#iv_q_servico_externos_jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 9 | 1.662 |
| [`IV_Q_SOLICITACAO_TCAT`](IV-2-formularios.md#iv_q_solicitacao_tcat) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 22 | 1 |
| [`IV_Q_TESTE1`](IV-2-formularios.md#iv_q_teste1) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 2 |
| [`IV_Q_TESTE2`](IV-2-formularios.md#iv_q_teste2) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 1 |
| [`IV_Q_TESTE_PRIMEIRO_JD`](IV-2-formularios.md#iv_q_teste_primeiro_jd) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 3 | 3 |
| [`IV_Q_TICKET_DSI`](IV-2-formularios.md#iv_q_ticket_dsi) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 1 |
| [`IV_Q_VENDA`](IV-2-formularios.md#iv_q_venda) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 22 | 2.806 |
| [`IV_Q_VENDAPERDIDA_SEGURO`](IV-2-formularios.md#iv_q_vendaperdida_seguro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 12 |
| [`IV_Q_VENDA_AMS`](IV-2-formularios.md#iv_q_venda_ams) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 65 | 775 |
| [`IV_Q_VENDA_CONSORCIO`](IV-2-formularios.md#iv_q_venda_consorcio) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 7 | 47 |
| [`IV_Q_VENDA_DIRETA`](IV-2-formularios.md#iv_q_venda_direta) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 0 |
| [`IV_Q_VENDA_DSI`](IV-2-formularios.md#iv_q_venda_dsi) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 16 | 1 |
| [`IV_Q_VENDA_EQUIPAMENTO`](IV-2-formularios.md#iv_q_venda_equipamento) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 49 | 2.256 |
| [`IV_Q_VENDA_MAQUINA_FY25`](IV-2-formularios.md#iv_q_venda_maquina_fy25) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 45 | 0 |
| [`IV_Q_VENDA_PERDIDA`](IV-2-formularios.md#iv_q_venda_perdida) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 14 | 1.511 |
| [`IV_Q_VENDA_PERDIDA_FY25`](IV-2-formularios.md#iv_q_venda_perdida_fy25) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 198 |
| [`IV_Q_VENDA_PERDIDA_IMPL`](IV-2-formularios.md#iv_q_venda_perdida_impl) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 31 |
| [`IV_Q_VENDA_PERDIDA_IMPLEM`](IV-2-formularios.md#iv_q_venda_perdida_implem) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 125 |
| [`IV_Q_VENDA_PERDIDA_JDE`](IV-2-formularios.md#iv_q_venda_perdida_jde) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 11 | 296 |
| [`IV_Q_VENDA_PERDIDA_MANITO`](IV-2-formularios.md#iv_q_venda_perdida_manito) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 6 |
| [`IV_Q_VENDA_PERDIDA_MAQIMP`](IV-2-formularios.md#iv_q_venda_perdida_maqimp) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 313 |
| [`IV_Q_VENDA_PERDIDA_PROD`](IV-2-formularios.md#iv_q_venda_perdida_prod) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 15 | 286 |
| [`IV_Q_VENDA_PERDIDA_SEGURO`](IV-2-formularios.md#iv_q_venda_perdida_seguro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 13 |
| [`IV_Q_VENDA_PERDIDA_TESTE`](IV-2-formularios.md#iv_q_venda_perdida_teste) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 0 |
| [`IV_Q_VENDA_PNEUS`](IV-2-formularios.md#iv_q_venda_pneus) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 7 | 9 |
| [`IV_Q_VENDA_PRECISION_UP`](IV-2-formularios.md#iv_q_venda_precision_up) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 2 |
| [`IV_Q_VENDA_SEMINOVO`](IV-2-formularios.md#iv_q_venda_seminovo) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 2.481 |
| [`IV_Q_VENDA_SERVICOS_AMS`](IV-2-formularios.md#iv_q_venda_servicos_ams) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 8 | 468 |
| [`IV_Q_VENDA_VP_PNEUS`](IV-2-formularios.md#iv_q_venda_vp_pneus) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 6 | 4 |
| [`IV_Q_VENDER_RENOVACAO_SEG`](IV-2-formularios.md#iv_q_vender_renovacao_seg) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 16 | 153 |
| [`IV_Q_VISITA_DSI`](IV-2-formularios.md#iv_q_visita_dsi) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 4 |
| [`IV_Q_VISITA_EXP_CLIENTE`](IV-2-formularios.md#iv_q_visita_exp_cliente) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 11 | 16 |
| [`IV_Q_VP_COLHEDORA`](IV-2-formularios.md#iv_q_vp_colhedora) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 11 | 1 |
| [`IV_Q_VP_COLHEITADEIRA`](IV-2-formularios.md#iv_q_vp_colheitadeira) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 3 |
| [`IV_Q_VP_PLANTADEIRA`](IV-2-formularios.md#iv_q_vp_plantadeira) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 13 | 3 |
| [`IV_Q_VP_PULVERIZADOR`](IV-2-formularios.md#iv_q_vp_pulverizador) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 10 | 0 |
| [`IV_Q_VP_RENOVACAO_SEGURO`](IV-2-formularios.md#iv_q_vp_renovacao_seguro) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 4 | 0 |
| [`IV_Q_VP_SEM_PARTICIPACAO`](IV-2-formularios.md#iv_q_vp_sem_participacao) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 74 |
| [`IV_Q_VP_TRATOR`](IV-2-formularios.md#iv_q_vp_trator) | `IV-2` | [IV-2-formularios.md](IV-2-formularios.md) | formulario-materializado | 12 | 19 |
| [`IV_Recurso`](IV-3-catalogos-bpm.md#iv_recurso) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 5 | 0 |
| [`IV_RecUso`](IV-3-catalogos-bpm.md#iv_recuso) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 7 | 0 |
| [`IV_ResClasse`](IV-3-catalogos-bpm.md#iv_resclasse) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 2 | 236 |
| [`IV_ResEvtOut`](IV-3-catalogos-bpm.md#iv_resevtout) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 4 | 0 |
| [`IV_RESJOB`](IV-3-catalogos-bpm.md#iv_resjob) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 2 | 0 |
| [`IV_ResMsgPapel`](IV-3-catalogos-bpm.md#iv_resmsgpapel) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 28 | 1.341 |
| [`IV_ResParam`](IV-3-catalogos-bpm.md#iv_resparam) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 37 | 0 |
| [`IV_Resultado`](IV-3-catalogos-bpm.md#iv_resultado) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 63 | 4.201 |
| [`IV_ResultadoCmpl`](IV-3-catalogos-bpm.md#iv_resultadocmpl) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 2 | 3.110 |
| [`IV_ResultadoInstr`](IV-3-catalogos-bpm.md#iv_resultadoinstr) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 2 | 98 |
| [`IV_ResultadoReq`](IV-3-catalogos-bpm.md#iv_resultadoreq) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 6 | 0 |
| [`IV_ResultadoWeb`](IV-3-catalogos-bpm.md#iv_resultadoweb) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 10 | 0 |
| [`IV_RESULTADO_3110`](IV-4-demais.md#iv_resultado_3110) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | lixo/backup | 74 | 2.826 |
| [`IV_ResVinc`](IV-3-catalogos-bpm.md#iv_resvinc) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 8 | 959 |
| [`IV_RetProcRegra`](IV-3-catalogos-bpm.md#iv_retprocregra) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 7 | 0 |
| [`IV_SegPerfil`](IV-3-catalogos-bpm.md#iv_segperfil) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 27 | 0 |
| [`IV_Selecao`](IV-4-demais.md#iv_selecao) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | catalogo | 12 | 543 |
| [`IV_SelecaoColList`](IV-4-demais.md#iv_selecaocollist) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 3 | 35 |
| [`IV_SelecaoCriterio`](IV-4-demais.md#iv_selecaocriterio) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 7 | 2.865 |
| [`IV_SelecaoPessoa`](IV-4-demais.md#iv_selecaopessoa) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 3 | 582.481 |
| [`IV_SELPROMOPRD`](IV-4-demais.md#iv_selpromoprd) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 5 | 0 |
| [`IV_SMS`](IV-4-demais.md#iv_sms) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 19 | 26.749 |
| [`IV_SMSLog`](IV-4-demais.md#iv_smslog) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 5 | 27.430 |
| [`IV_STATUS_DEPTO`](IV-3-catalogos-bpm.md#iv_status_depto) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | isolada | 5 | 473.844 |
| [`IV_TAGCAD`](IV-3-catalogos-bpm.md#iv_tagcad) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 9 | 0 |
| [`IV_TC_PESSOA`](IV-4-demais.md#iv_tc_pessoa) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 13 | 0 |
| [`IV_TEMPLATEPROJ`](IV-4-demais.md#iv_templateproj) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 7 | 0 |
| [`IV_TIPOCONTEUDO`](IV-3-catalogos-bpm.md#iv_tipoconteudo) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | catalogo | 3 | 1 |
| [`IV_TpPgto`](IV-3-catalogos-bpm.md#iv_tppgto) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 5 | 0 |
| [`IV_TxtPadConta`](IV-3-catalogos-bpm.md#iv_txtpadconta) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 5 | 0 |
| [`IV_TxtPadrao`](IV-3-catalogos-bpm.md#iv_txtpadrao) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | catalogo | 11 | 212 |
| [`IV_TxtPadraoUso`](IV-3-catalogos-bpm.md#iv_txtpadraouso) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | nucleo | 4 | 5 |
| [`IV_Unidade`](IV-3-catalogos-bpm.md#iv_unidade) | `IV-3` | [IV-3-catalogos-bpm.md](IV-3-catalogos-bpm.md) | vazia | 10 | 0 |
| [`IV_URADISPARO`](IV-4-demais.md#iv_uradisparo) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 12 | 0 |
| [`IV_URARELATORIO`](IV-4-demais.md#iv_urarelatorio) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 16 | 0 |
| [`IV_USRPUSH`](IV-4-demais.md#iv_usrpush) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 15 | 0 |
| [`IV_USRSTATUS`](IV-4-demais.md#iv_usrstatus) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | nucleo | 7 | 186 |
| [`IV_VENDEDOR`](IV-4-demais.md#iv_vendedor) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 18 | 344 |
| [`IV_VENDEDOREMPR`](IV-4-demais.md#iv_vendedorempr) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | isolada | 4 | 917 |
| [`IV_WHATSAPP`](IV-4-demais.md#iv_whatsapp) | `IV-4` | [IV-4-demais.md](IV-4-demais.md) | vazia | 21 | 0 |
| [`J1_PRODUTO`](J1.md#j1_produto) | `J1` | [J1.md](J1.md) | lixo/backup | 9 | 211.556 |
| [`JDE_CATEGORY`](JDE.md#jde_category) | `JDE` | [JDE.md](JDE.md) | staging | 2 | 0 |
| [`JDE_EQUIPAMENTS`](JDE.md#jde_equipaments) | `JDE` | [JDE.md](JDE.md) | staging | 18 | 0 |
| [`JDE_PURCHASE`](JDE.md#jde_purchase) | `JDE` | [JDE.md](JDE.md) | staging | 18 | 0 |
| [`JDE_QUOTE`](JDE.md#jde_quote) | `JDE` | [JDE.md](JDE.md) | staging | 19 | 0 |
| [`JDE_QUOTE_ITEM`](JDE.md#jde_quote_item) | `JDE` | [JDE.md](JDE.md) | staging | 2 | 0 |
| [`JDE_SALES_PERSON`](JDE.md#jde_sales_person) | `JDE` | [JDE.md](JDE.md) | staging | 5 | 0 |
| [`JDE_SUB_CATEGORY`](JDE.md#jde_sub_category) | `JDE` | [JDE.md](JDE.md) | staging | 2 | 0 |
| [`LOG_INTEGRACAO_FATURAMENTO_TOTVS`](LOG.md#log_integracao_faturamento_totvs) | `LOG` | [LOG.md](LOG.md) | vazia | 4 | 0 |
| [`mig_ge_pessoa`](MIG.md#mig_ge_pessoa) | `MIG` | [MIG.md](MIG.md) | lixo/backup | 2 | 5 |
| [`mig_ge_pessoa_bkp`](MIG.md#mig_ge_pessoa_bkp) | `MIG` | [MIG.md](MIG.md) | lixo/backup | 2 | 28.084 |
| [`nfsaida$`](OUTROS.md#nfsaida) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 23 | 62.619 |
| [`OUT_Contato`](OUT.md#out_contato) | `OUT` | [OUT.md](OUT.md) | isolada | 25 | 147 |
| [`OUT_Log`](OUT.md#out_log) | `OUT` | [OUT.md](OUT.md) | isolada | 12 | 1.550 |
| [`OUT_Pessoa`](OUT.md#out_pessoa) | `OUT` | [OUT.md](OUT.md) | isolada | 61 | 1.918 |
| [`OUT_PessoaLink`](OUT.md#out_pessoalink) | `OUT` | [OUT.md](OUT.md) | isolada | 4 | 122 |
| [`OUT_PessoaRelacao`](OUT.md#out_pessoarelacao) | `OUT` | [OUT.md](OUT.md) | isolada | 7 | 9 |
| [`SYSCONVERT1`](OUTROS.md#sysconvert1) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 2 | 3 |
| [`SYSCONVERT2`](OUTROS.md#sysconvert2) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 2 | 24 |
| [`SYSCONVERT3`](OUTROS.md#sysconvert3) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 2 | 14 |
| [`sysdiagrams`](OUTROS.md#sysdiagrams) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 5 | 0 |
| [`SYSDUMMY`](OUTROS.md#sysdummy) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 2 | 0 |
| [`TEMP`](OUTROS.md#temp) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 3 | 1.026 |
| [`teste`](OUTROS.md#teste) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 1 | 0 |
| [`teste333`](OUTROS.md#teste333) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 11 | 24.024 |
| [`testepiv`](OUTROS.md#testepiv) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 4 | 12 |
| [`teste_acesso`](TESTE.md#teste_acesso) | `TESTE` | [TESTE.md](TESTE.md) | lixo/backup | 1 | 471 |
| [`teste_fefa`](TESTE.md#teste_fefa) | `TESTE` | [TESTE.md](TESTE.md) | lixo/backup | 1 | 0 |
| [`usuarios$`](OUTROS.md#usuarios) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 2 | 158 |
| [`VBI_CONTA`](VBI.md#vbi_conta) | `VBI` | [VBI.md](VBI.md) | nucleo | 14 | 1.004 |
| [`VBI_CONTAFAM`](VBI.md#vbi_contafam) | `VBI` | [VBI.md](VBI.md) | nucleo | 6 | 1.004 |
| [`VBI_CONTAPAI`](VBI.md#vbi_contapai) | `VBI` | [VBI.md](VBI.md) | nucleo | 18 | 243 |
| [`ww`](OUTROS.md#ww) | `OUTROS` | [OUTROS.md](OUTROS.md) | lixo/backup | 22 | 8.991 |
| [`X_TOTVS_BI_FATURAMENTO_MAQUINAS`](X_TOTVS.md#x_totvs_bi_faturamento_maquinas) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | staging | 13 | 4.612 |
| [`X_TOTVS_BI_FATURAMENTO_POS_VENDAS`](X_TOTVS.md#x_totvs_bi_faturamento_pos_vendas) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | staging | 30 | 407.100 |
| [`X_TOTVS_CRM_FATURAMENTO`](X_TOTVS.md#x_totvs_crm_faturamento) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | staging | 41 | 809.821 |
| [`X_T_IMP_CRM_TITULO`](X_TOTVS.md#x_t_imp_crm_titulo) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | staging | 41 | 462.391 |
| [`X_T_IMP_CRM_TITULO_bkp_11_04`](X_TOTVS.md#x_t_imp_crm_titulo_bkp_11_04) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | lixo/backup | 41 | 121.381 |
| [`X_T_IMP_CRM_VEICULO`](X_TOTVS.md#x_t_imp_crm_veiculo) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | staging | 7 | 7.279 |
| [`X_V_IMP_CRM_IMP_NF`](X_TOTVS.md#x_v_imp_crm_imp_nf) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | staging | 6 | 815.854 |
| [`X_V_IMP_CRM_IMP_NF_BKP_18_09_2023`](X_TOTVS.md#x_v_imp_crm_imp_nf_bkp_18_09_2023) | `X_TOTVS` | [X_TOTVS.md](X_TOTVS.md) | lixo/backup | 6 | 415.762 |

## 8. Os outros documentos deste dicionario

- [GRAFO-FK.md](GRAFO-FK.md) - diagramas mermaid do nucleo relacional
- [LACUNAS.md](LACUNAS.md) - o que o catalogo CSV nao cobre e precisa de acesso ao vivo
- [gerar-dicionario.py](gerar-dicionario.py) - o gerador (deterministico, reexecutavel)
