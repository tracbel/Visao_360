/* =================================================================================================
   CONCILIAÇÃO DAS VENDAS DO TERRITÓRIO — uma tabela só, sem sobreposição (documento 32, seção 8.6).

   Só leitura. Roda no banco do CRM, sem filtro de filial (sa ou login de leitura). Mostra, para o
   período, cada categoria de nota em exatamente uma linha, e como os totais citados no documento se
   formam a partir delas:

     R$ 559,5 mi   notas COM cliente no CRM, 13 filiais ativas            = A1 + A2 + A3
     R$ 17,4 mi    notas COM cliente, filiais inativas                     = A4
     R$ 315,9 mi   notas SEM cliente no CRM, 13 filiais ativas             = B1 + B2 + B3 + B4
     R$ 4,7 mi     notas SEM cliente no CRM, filiais inativas              = B5
     R$ 320,6 mi   notas SEM cliente no CRM, todas as filiais              = B1 + … + B5
     R$ 897,5 mi   tudo o que as filiais emitiram (a empresa inteira)      = A1 + … + B5
     R$ 217,1 mi   total da tela na visão da filial @Filial (a captura)    = linhas F1 + F2 + F3

   Premissas, conferidas em 13/09/2026:
   - "ativa" e "inativa" vêm de organizacao.Empresa.EstaAtiva. O cadastro tem 18 empresas: 16 filiais
     0101NN (13 ativas; 3 inativas — Guaíra, Ituverava e Monte Alto) e 2 empresas antigas inativas. No
     período do documento só as 3 lojas inativas tiveram nota; outro período pode trazer as demais.
   - "cliente de outra filial" (A2, F2) é Cliente.EmpresaId diferente da filial que emitiu. A API usa
     "cliente fora do alcance da filial"; as duas coincidem porque as filiais não têm hierarquia (todas
     no nível 0). Se passarem a ter, A2 e F2 precisam contar as filiais abaixo como "própria".

   Parâmetros: @De e @Ate (competências, dia 1, inclusivas) e @Filial (a do cabeçalho da captura).
   ================================================================================================= */
SET NOCOUNT ON;

DECLARE @De date = '2025-09-01';
DECLARE @Ate date = '2026-08-01';
DECLARE @Filial char(6) = '010101';

DECLARE @Categoria TABLE (
    Ordem varchar(4) PRIMARY KEY,
    Grupo nvarchar(60),
    Filiais nvarchar(20),
    Categoria nvarchar(160),
    NaVisaoDaFilial nvarchar(120),
    NaVisaoDaEmpresa nvarchar(120),
    Linhas int,
    Valor decimal(18, 2));

;WITH comCliente AS (
    SELECT f.ValorLiquido, ev.EstaAtiva AS VendaAtiva,
           CASE WHEN c.ExcluidoEm IS NOT NULL THEN 'inativado' WHEN c.EmpresaId = f.EmpresaId THEN 'propria' ELSE 'outra' END AS Cadastro
    FROM comercial.FaturamentoDoCliente f
    JOIN comercial.Cliente c ON c.Id = f.ClienteId
    JOIN organizacao.Empresa ev ON ev.Id = f.EmpresaId
    WHERE f.ExcluidoEm IS NULL AND f.Competencia BETWEEN @De AND @Ate
)
INSERT @Categoria
SELECT 'A1', N'Nota com cliente no CRM', N'13 ativas', N'Cliente cadastrado na própria filial que vendeu',
       N'município do cliente', N'município do cliente', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM comCliente WHERE VendaAtiva = 1 AND Cadastro = 'propria'
UNION ALL
SELECT 'A2', N'Nota com cliente no CRM', N'13 ativas', N'Cliente cadastrado em OUTRA filial',
       N'grupo ClienteDeOutraFilial (fora do mapa)', N'município do cliente', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM comCliente WHERE VendaAtiva = 1 AND Cadastro = 'outra'
UNION ALL
SELECT 'A3', N'Nota com cliente no CRM', N'13 ativas', N'Cliente inativado no CRM',
       N'grupo ClienteInativado', N'grupo ClienteInativado', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM comCliente WHERE VendaAtiva = 1 AND Cadastro = 'inativado'
UNION ALL
SELECT 'A4', N'Nota com cliente no CRM', N'inativas', N'Qualquer cliente',
       N'não aparece (filial inativa não é selecionável)', N'município do cliente ou grupo fora do mapa', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM comCliente WHERE VendaAtiva = 0;

;WITH semCliente AS (
    SELECT s.ValorLiquido, e.EstaAtiva,
           CASE s.Natureza WHEN 'Fabrica' THEN 'fabrica' WHEN 'EmpresaDoGrupo' THEN 'grupo' WHEN 'OutraRevenda' THEN 'revenda' ELSE 'sem_cadastro' END AS Natureza
    FROM comercial.FaturamentoSemCliente s
    JOIN organizacao.Empresa e ON e.Id = s.EmpresaId
    WHERE s.ExcluidoEm IS NULL AND s.Competencia BETWEEN @De AND @Ate
)
INSERT @Categoria
SELECT 'B1', N'Nota sem cliente no CRM', N'13 ativas', N'Contraparte sem cadastro (cliente não cadastrado, sem documento ou não classificada)',
       N'grupo ContraparteSemCadastro', N'grupo ContraparteSemCadastro', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM semCliente WHERE EstaAtiva = 1 AND Natureza = 'sem_cadastro'
UNION ALL
SELECT 'B2', N'Nota sem cliente no CRM', N'13 ativas', N'Fábrica (repasse)', N'grupo RepasseDeFabrica', N'grupo RepasseDeFabrica', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM semCliente WHERE EstaAtiva = 1 AND Natureza = 'fabrica'
UNION ALL
SELECT 'B3', N'Nota sem cliente no CRM', N'13 ativas', N'Empresa do grupo', N'grupo EmpresaDoGrupo', N'grupo EmpresaDoGrupo', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM semCliente WHERE EstaAtiva = 1 AND Natureza = 'grupo'
UNION ALL
SELECT 'B4', N'Nota sem cliente no CRM', N'13 ativas', N'Outra revenda', N'grupo OutraRevenda', N'grupo OutraRevenda', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM semCliente WHERE EstaAtiva = 1 AND Natureza = 'revenda'
UNION ALL
SELECT 'B5', N'Nota sem cliente no CRM', N'inativas', N'Todas as naturezas', N'não aparece (filial inativa não é selecionável)', N'grupo da natureza', COUNT(*), ISNULL(SUM(ValorLiquido), 0)
FROM semCliente WHERE EstaAtiva = 0;

-- A TABELA ÚNICA: cada nota do período em exatamente uma linha.
SELECT Ordem, Grupo, Filiais, Categoria, NaVisaoDaFilial, NaVisaoDaEmpresa, Linhas, Valor
FROM @Categoria ORDER BY Ordem;

-- OS TOTAIS CITADOS, formados só com as linhas acima.
SELECT Total, Composicao, Valor FROM (VALUES
    (N'Com cliente, 13 filiais ativas (R$ 559,5 mi)', N'A1 + A2 + A3', (SELECT SUM(Valor) FROM @Categoria WHERE Ordem IN ('A1', 'A2', 'A3'))),
    (N'Cliente de outra filial (R$ 321,6 mi)', N'A2', (SELECT Valor FROM @Categoria WHERE Ordem = 'A2')),
    (N'Com cliente, filiais inativas (R$ 17,4 mi)', N'A4', (SELECT Valor FROM @Categoria WHERE Ordem = 'A4')),
    (N'Sem cliente no CRM, 13 filiais ativas (R$ 315,9 mi)', N'B1 + B2 + B3 + B4', (SELECT SUM(Valor) FROM @Categoria WHERE Ordem IN ('B1', 'B2', 'B3', 'B4'))),
    (N'Sem cliente no CRM, filiais inativas (R$ 4,7 mi)', N'B5', (SELECT Valor FROM @Categoria WHERE Ordem = 'B5')),
    (N'Sem cliente no CRM, todas as filiais (R$ 320,6 mi)', N'B1 + B2 + B3 + B4 + B5', (SELECT SUM(Valor) FROM @Categoria WHERE Ordem LIKE 'B%')),
    (N'Empresa inteira: tudo o que foi emitido (R$ 897,5 mi)', N'A1 + … + B5', (SELECT SUM(Valor) FROM @Categoria))
) AS t (Total, Composicao, Valor);

-- O TOTAL DA CAPTURA: a visão da filial @Filial soma as notas QUE ELA EMITIU, com e sem cliente.
;WITH daFilial AS (
    SELECT f.ValorLiquido, CASE WHEN c.ExcluidoEm IS NOT NULL THEN 'F3' WHEN c.EmpresaId = f.EmpresaId THEN 'F1' ELSE 'F2' END AS Linha
    FROM comercial.FaturamentoDoCliente f
    JOIN comercial.Cliente c ON c.Id = f.ClienteId
    JOIN organizacao.Empresa e ON e.Id = f.EmpresaId AND e.Codigo = @Filial
    WHERE f.ExcluidoEm IS NULL AND f.Competencia BETWEEN @De AND @Ate
    UNION ALL
    SELECT s.ValorLiquido, 'F4'
    FROM comercial.FaturamentoSemCliente s
    JOIN organizacao.Empresa e ON e.Id = s.EmpresaId AND e.Codigo = @Filial
    WHERE s.ExcluidoEm IS NULL AND s.Competencia BETWEEN @De AND @Ate
)
SELECT @Filial AS Filial, Linha,
       CASE Linha WHEN 'F1' THEN N'cliente cadastrado nela (no mapa ou nos grupos de endereço)'
                  WHEN 'F2' THEN N'cliente de outra filial (grupo ClienteDeOutraFilial)'
                  WHEN 'F3' THEN N'cliente inativado'
                  ELSE N'nota sem cliente no CRM (grupos por natureza)' END AS Parte,
       CAST(SUM(ValorLiquido) AS decimal(18, 2)) AS Valor
FROM daFilial GROUP BY Linha
UNION ALL
SELECT @Filial, 'TOTAL', N'total da consulta na tela', CAST(SUM(ValorLiquido) AS decimal(18, 2)) FROM daFilial
ORDER BY 2;
