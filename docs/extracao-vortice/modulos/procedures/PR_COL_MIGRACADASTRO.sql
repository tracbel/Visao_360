/* ==============================================================
   Objeto ..........: dbo.PR_COL_MIGRACADASTRO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-27 19:59:03
   Modificado em ...: 2024-12-30 17:16:53
   Linhas ..........: 84
   Escreve em tabela: SIM (INSERT, DELETE)
   Alvos de escrita : mig_ge_pessoa
   Tabelas referidas: GE_PESSOA, GE_Pessoa_ita, mig_ge_pessoa
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE procedure [dbo].[PR_COL_MIGRACADASTRO] 
AS


/*
-- autor: AMAURY 01/11/2024
   objetivo: gerar tabela de controle para Migrar pessoas
   log de alterações: 05/11/2024 - Criacao da rotina e testes
*/	

 

begin

/* criando a tabela */
IF object_id('MIG_GE_PESSOA') IS NULL
BEGIN
	create table mig_ge_pessoa (seqpessoa decimal(12,0), descricao varchar(50)) 
END

delete from mig_ge_pessoa

/* inserindo cadastro ativo com CNPJ */
insert into mig_ge_pessoa 
SELECT seqpessoa, 'Ativo com CNPJ Inexistente'
FROM GE_Pessoa_ita XA
WHERE STATUS in ( 'A', 'O' )
  AND NOT EXISTS (SELECT 1 FROM GE_PESSOA WHERE NROCGCCPF = XA.NROCGCCPF )
  AND ISNULL(NROCGCCPF, 999999)<>999999
  AND LEN(CAST ( NROCGCCPF AS VARCHAR ))> 4

  and xa.seqpessoa in ( 1501, 3302, 3153, 13239, 21292 )


/* inserindo prospect ativo com CNPJ */
insert into mig_ge_pessoa 
SELECT seqpessoa, 'Prospect com CNPJ Inexistente'
FROM GE_Pessoa_ita XA
WHERE STATUS = 'P'
  AND NOT EXISTS (SELECT 1 FROM GE_PESSOA WHERE NROCGCCPF = XA.NROCGCCPF )
  and not exists (select 1 from mig_ge_pessoa where seqpessoa = xa.seqpessoa)
  AND ISNULL(NROCGCCPF, 999999)<>999999
  AND LEN(CAST ( NROCGCCPF AS VARCHAR ))> 4

  and xa.seqpessoa in ( 1501, 3302, 3153, 13239, 21292 )


/* inserindo prospect ativo com tamanho do CNPJ < 4 digitos */
insert into mig_ge_pessoa 
SELECT seqpessoa, 'Prospect com CNPJ<>0 Inexistente'
FROM GE_Pessoa_ita XA
WHERE STATUS in ( 'A', 'O' )
  AND NOT EXISTS (SELECT 1 FROM GE_PESSOA WHERE NROCGCCPF = XA.NROCGCCPF )
  and not exists (select 1 from mig_ge_pessoa where seqpessoa = xa.seqpessoa)
  AND LEN(CAST ( NROCGCCPF AS VARCHAR ))< 4

  and xa.seqpessoa in ( 1501, 3302, 3153, 13239, 21292 )

/* inserindo prospect ativo com CNPJ existente na Colorado*/
insert into mig_ge_pessoa 
SELECT seqpessoa, 'Ativo/Prospect CNPJ Existente'
FROM GE_Pessoa_ita XA
WHERE 1=1
  and STATUS in ( 'A', 'P' , 'O')
  AND EXISTS (SELECT 1 FROM GE_PESSOA WHERE NROCGCCPF = XA.NROCGCCPF )
  and not exists (select 1 from mig_ge_pessoa where seqpessoa = xa.seqpessoa)
  AND ISNULL(NROCGCCPF, 999999)<>999999
  AND LEN(CAST ( NROCGCCPF AS VARCHAR ))> 4

  and xa.seqpessoa in ( 1501, 3302, 3153, 13239, 21292 )

/* inserindo prospect sem CNPJ */
insert into mig_ge_pessoa 
SELECT seqpessoa, 'Prospect sem CNPJ'
FROM GE_Pessoa_ita XA
WHERE 1=1
  and STATUS in ( 'P', 'O' )
  and not exists (select 1 from mig_ge_pessoa where seqpessoa = xa.seqpessoa)
  AND ISNULL(NROCGCCPF, 999999)=999999

  and xa.seqpessoa in ( 1501, 3302, 3153, 13239, 21292 )

End
