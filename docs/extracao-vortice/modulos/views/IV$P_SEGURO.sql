/* ==============================================================
   Objeto ..........: dbo.IV$P_SEGURO
   Tipo ............: VIEW
   Criado em .......: 2024-09-12 09:51:35
   Modificado em ...: 2024-09-12 09:51:35
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_CLIENTEPROPR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$P_SEGURO ( SeqPessoa, SeqPropriedade, Referencia, Ativo, Notas, CodOrigem, DtaInclusao, UsuInclusao, DtaAlteracao, UsuAlteracao, Identificador  ,  SEGURADORA,  CORRETORA,  CONDICAO_PAGAMENTO,  APOLICE,  COBERTURA_INICIAL,  COBERTURA_FINAL,  FATURAMENTO_MAQUINA,  VALOR_COBERTURA,  VALOR_SEGURO,  EQUIPAMENTO,  CHASSI,  VENDEDOR_SEGURO,  VALOR_DO_BEM,  ANO_DO_BEM,  PARCELAS,  __DE_COMISSAO)   AS SELECT  SEQPESSOA, SEQPROPRIEDADE, REFERENCIA, ATIVO, NOTAS, CODORIGEM, DTAINCLUSAO, USUINCLUSAO, DTAALTERACAO, USUALTERACAO, IDENTIFICADOR  ,  CAMPO1,  CAMPO2,  CAMPO3,  NUMERO1,  DATA1,  DATA2,  DATA3,  LITERAL1,  LITERAL2,  LITERAL3,  LITERAL4,  LITERAL5,  LITERAL6,  LITERAL7,  LITERAL8,  LITERAL9 FROM IV_CLIENTEPROPR       WHERE SEQPROPRIEDADE = 18 