/* ==============================================================
   Objeto ..........: dbo.fva_FmtNum
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2026-05-07 11:30:11
   Modificado em ...: 2026-05-07 11:30:11
   Linhas ..........: 69
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE FUNCTION dbo.fva_FmtNum ( @pnNum decimal(20,4) , @psMascara varchar(30) )
RETURNS Varchar(30)
/* Formata um número de acordo com a Máscara
--- Davi/Nelson - jan/2012  */
AS
BEGIN 
	Declare  @vsRet Varchar ( 40) 
	Declare  @vsC Varchar(1)
	Declare  @vnTamMaskInt Int 
	Declare  @Index Int
	Declare  @vsValorString Varchar(30)
	Declare  @vnPosPonto Int		 
	Declare  @vnPosVirgula Int		 
	Declare  @vnQtde Int 
	Set @vsRet = '' 	
	Set @vsValorString = convert (varchar (30) , @pnNum ) 
	Set @vnPosPonto = charindex (  '.',@vsValorString ) 
	If  @psMascara = ''  
		Set @psMascara = '0.000.000.000.009,99' 
	Set @vnTamMaskInt = Len ( @psMascara)
	Set @vnPosVirgula = charindex (  ',', @psMascara )
	
/*  Montando numero Inteiro  */
    Set @index = @vnPosPonto - 1
    If  @vnPosVirgula > 0
		Set @vnTamMaskInt = @vnPosVirgula -1
    Else  
		Set @vnTamMaskInt = Len ( @psMascara)
  /* Determinando se há complemento de valores com zeros (masc=9)		 */
	Set @vnQtde	= @vnTamMaskInt - ((charindex (  '9', @psMascara ++ '9' ) - 1))
	While  @index > 0 Or @vnQtde > 0 
	  BEGIN
		If  @vnTamMaskInt  > 0  
			Set @vsC = Substring ( @psMascara , @vnTamMaskInt , 1)
		Else
			Set @vsC = '0' 
		If  @vsC = '9' or @vsC = '0'
			begin 
				If @index > 0
					Set @vsRet = Substring ( @vsValorString , @index, 1) ++ @vsRet 
				Else
					Set @vsRet = '0' ++ @vsRet 
				Set @index = @index - 1
			End
		Else
			Set @vsRet = isnull(@vsC, ' ') ++ @vsRet 
		Set @vnTamMaskInt  = @vnTamMaskInt  - 1 
		Set @vnQtde = @vnQtde - 1 
      END 
      
    /* --- Montando os decimais  */
    If  @vnPosVirgula = 0
    	Return @vsRet     	
    Set @index = @vnPosPonto + 1 
	Set @vnTamMaskInt = @vnPosVirgula + 1
	Set @vsC = Substring ( @psMascara , @vnTamMaskInt , 1)
	Set @vsRet = @vsRet ++ ','
 	While @vsC != ''
	  BEGIN
		Set @vsRet = @vsRet ++ Substring ( @vsValorString , @index, 1)
		Set @index = @index + 1 
	    Set @vnTamMaskInt = @vnTamMaskInt + 1
	    Set @vsC = Substring ( @psMascara , @vnTamMaskInt , 1)
      END 
   
	Return @vsRet 
END
