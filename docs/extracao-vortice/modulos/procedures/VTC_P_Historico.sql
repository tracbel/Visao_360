/* ==============================================================
   Objeto ..........: dbo.VTC_P_Historico
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-10-31 15:48:51
   Modificado em ...: 2025-10-31 15:48:51
   Linhas ..........: 119
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : gep_import
   Tabelas referidas: gep_import
   Outras refs .....: fc5_coldate, fc5_colnumber, fc5_colstring
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create procedure VTC_P_Historico
(
@vsOrigem       varchar(20),
@vsSeparador    varchar(1),
@vsEvento       varchar(30),
@vsCodusuario   varchar(20),
@vsDetalhe      varchar(1000),
@vdDtarealizacao datetime,
@vnNroempresa   integer,
@vsPESSOALINK varchar(30),
@vsPESSOALINKORIGEM varchar(20),
@vsPESSOALINKANT varchar(30),
@vsPESSOALINKORIGEMANT varchar(20),
@vnProcesso	integer,
@vnSeqPessoa integer,
@vnResultado integer )

as 
-----
DECLARE @vsDado varchar(2000)
DECLARE @vsColuna varchar(2000) 
DECLARE @vsColunaidentific varchar(250)
DECLARE @vsDadoIdentificador varchar(250)

----
 
begin
   set @vsColuna = 'ORIGEM'
   set @vsDado = dbo.fc5_colstring(@vsORIGEM,'')

   if @vsEvento is not null 
      begin
         set @vsColuna = @vsColuna+';EVENTO'
         set @vsDado = @vsDado+dbo.fc5_colstring(@vsEVENTO,';')
      end

   if @vsCodusuario is not null 
      begin
         set @vsColuna = @vsColuna+';CODUSUARIO'
         set @vsDado = @vsDado+dbo.fc5_colstring(@vsCodusuario,';')
      end
      
   if @vsDetalhe is not null 
      begin
         set @vsColuna = @vsColuna+';DETALHE'
         set @vsDado = @vsDado+dbo.fc5_colstring(@vsDetalhe,';')
      end
      

   if @vsPessoaLink is not null 
      begin
         set @vsColuna = @vsColuna+';PESSOALINK'
         set @vsDado = @vsDado+dbo.fc5_colstring(@vsPESSOALINK,';')
         set @vsColunaidentific = 'PESSOALINK'
         set @vsDadoidentificador = dbo.fc5_colstring(@vsPESSOALINK,'')
      end

   if @vsPessoaLinkOrigem is not null 
      begin
         set @vsColuna = @vsColuna+';PESSOALINKORIGEM'
         set @vsDado = @vsDado+dbo.fc5_colstring(@vsPESSOALINKORIGEM,';')
         set @vsColunaidentific = @vsColunaidentific+';PESSOALINKORIGEM'
         set @vsDadoidentificador = @vsDadoidentificador+dbo.fc5_colstring(@vsPESSOALINKORIGEM,';')
      end

   if @vddtarealizacao is not null 
      begin
         set @vsColuna = @vsColuna+';DTAREALIZACAO'
         set @vsDado = @vsDado+dbo.fc5_coldate(@vddtarealizacao,';')
      end

   if @vnNroempresa is not null 
      begin
         set @vsColuna = @vsColuna+';NROEMPRESA'
         set @vsDado = @vsDado+dbo.fc5_colnumber(@vnNroempresa,';')
      end
      
   if @vnProcesso is not null 
      begin
         set @vsColuna = @vsColuna+';PROCESSO'
         set @vsDado = @vsDado+dbo.fc5_colnumber(@vnProcesso,';')
      end      

   if @vnSeqPessoa is not null 
      begin
         set @vsColuna = @vsColuna+';SEQPESSOA'
         set @vsDado = @vsDado+dbo.fc5_colnumber(@vnSeqPessoa,';')
      end      

   if @vnResultado is not null 
      begin
         set @vsColuna = @vsColuna+';RESULTADO'
         set @vsDado = @vsDado+dbo.fc5_colnumber(@vnResultado,';')
      end      

   insert into gep_import(PROCESSO, STATUS,
                          ORIGEM, 
                          ACAO, 
                          TABELA, 
                          SEPARADOR, 
                          COLUNA, 
                          Dado, 
                          colunaidentific, 
                          Dadoidentificador, 
                          dtageracao,
						  Prioridade)
        VALUES ('CRM', 'E',
                @vsOrigem,
                'I',
                'IV_HISTORICO',
                @vsSEPARADOR,
                @vsColuna,
                @vsDado,
                @vsColunaIdentific,
                @vsDadoIdentificador,
                getdate()-0.01,
				1
                )
end