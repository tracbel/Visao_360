/* ==============================================================
   Objeto ..........: dbo.GE$PESSOA
   Tipo ............: VIEW
   Criado em .......: 2017-11-09 14:12:48
   Modificado em ...: 2019-10-25 10:40:37
   Linhas ..........: 15
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Questionario, IV_TC_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.GE$PESSOA
AS
SELECT  TBASE.SeqPessoa, TBASE.SeqCidade, TBASE.SeqBairro, TBASE.Versao, TBASE.Status, TBASE.DtaAtivacao, TBASE.NomeRazao, TBASE.Fantasia, TBASE.PalavraChave, TBASE.FisicaJuridica, TBASE.Sexo, TBASE.Cidade, TBASE.Uf, TBASE.Pais, 
                   TBASE.Bairro, TBASE.TipoLogradouro, TBASE.Logradouro, TBASE.NroLogradouro, TBASE.CmpltoLogradouro, TBASE.Cep, TBASE.CxPostal, TBASE.SeqPessoaEndCobr, TBASE.FoneDDD1, TBASE.FoneNro1, TBASE.FoneCmpl1, TBASE.FoneDDD2, 
                   TBASE.FoneNro2, TBASE.FoneCmpl2, TBASE.FoneDDD3, TBASE.FoneNro3, TBASE.FoneCmpl3, TBASE.FaxDDD, TBASE.FaxNro, TBASE.NroCGCCPF, TBASE.DigCGCCPF, TBASE.InscricaoRG, TBASE.UFEmissor, TBASE.OrgaoEmissor, 
                   TBASE.InscMunic, TBASE.InscProdutor, TBASE.CNAE, TBASE.DtaNascFund, TBASE.Origem, TBASE.UltOrigem, TBASE.Email, TBASE.HomePage, TBASE.EstadoCivil, TBASE.Atividade, TBASE.RendaFaturamento, TBASE.GrauInstrucao, 
                   TBASE.Grupo, TBASE.Porte, TBASE.DtaInclusao, TBASE.UsuInclusao, TBASE.DtaAlteracao, TBASE.UsuAlteracao, TBASE.DtaInativacao, TBASE.UsuInativacao, TBASE.ObsInativacao, TBASE.CodEquipe, TBASE.Telefonema, 
                   TBASE.Correspondencia, TBASE.RecebeEmail, TBASE.NaoPossuiEmail, TBASE.ProblemaCredito, TBASE.IndContribICMS, TBASE.RefEndereco, TBASE.Latitude, TBASE.Longitude, TBASE.SeqRegiao, TBASE.SeqRota, TBASE.RecebeSMS, 
                   TBASE.SEQPESSOAPRC, TBASE.Skype, TABF.QUANTIDADE_DE_EQUIPA, TABF.TIPO_DE_EQUIPAMENTO, TABF.MARCA, TABF.MODELO_DO_EQUIPAMENT, TABF.ANO_MODELO_EQUIPAMEN, TABF.N__SERIE, TABF.HORIMETRO, 
                   TABF.DATA_INICIO_LOCACAO, TABF.DATA_TERMINO_LOCACAO, TABF.VALOR_MENSAL_DO_CONT, TABF.VALOR_TOTAL_DO_CONTR, TABF.DATA_RENOVACAO, TBASE.SEQPESSOAPRC AS Expr1
FROM      dbo.GE_Pessoa AS TBASE LEFT OUTER JOIN
                   dbo.IV_Questionario AS QST INNER JOIN
                   dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                   dbo.IV_TC_PESSOA AS TABF ON TABF.SEQQUESTIONARIO = QST.SeqQuestionario ON QST.SeqPessoa = TBASE.SeqPessoa AND QST.SeqFormulario = 50
