-- =============================================================================================
--  USUARIOS DE DESENVOLVIMENTO — CRM Tracbel
--
--  ISTO NAO E DADO DE REFERENCIA, e por isso mora num arquivo separado do 01: usuario de
--  verdade vem do Microsoft Entra ID, e o CRM apenas espelha (documento 05). Estas linhas sao
--  ANDAIME, para o cadastro poder ser exercitado enquanto a autenticacao nao entra.
--
--  `rodar-seed.ps1` so aplica este arquivo com -ComUsuariosDeDesenvolvimento, e o script recusa
--  a bandeira quando o ambiente nao e de desenvolvimento. Em homologacao e producao o cadastro
--  de usuario nasce do espelhamento do Entra ID, nunca daqui.
--
--  AS FILIAIS ESCOLHIDAS NAO SAO ALEATORIAS: dois usuarios em filiais DIFERENTES (010101 e
--  010103) sao o que torna a fronteira de multiempresa demonstravel de ponta a ponta — um
--  cadastra, o outro nao enxerga.
--
--  IDEMPOTENTE: MERGE pelo nome principal, que e a chave natural do usuario.
-- =============================================================================================

-- QUOTED_IDENTIFIER LIGADO EXPLICITAMENTE: o sqlcmd sobe com ele DESLIGADO por padrao, e o
-- MERGE recusa rodar assim em tabela com indice filtrado — que e o caso de quase toda tabela
-- deste modelo. Declarar aqui faz o arquivo funcionar em qualquer cliente, e nao so no que
-- passa a bandeira certa na linha de comando.
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

MERGE seguranca.Usuario AS destino
USING (
    SELECT v.IdentidadeExterna, v.NomePrincipal, v.NomeCompleto, v.NomeExibicao, v.Email, v.Papel, e.Id AS EmpresaId
    FROM (VALUES
        ('11111111-1111-4111-8111-111111111111', N'cen.ribeiraopreto@tracbel.com.br',
         N'Consultor de Negocios — Ribeirao Preto', N'CEN Ribeirao', N'cen.ribeiraopreto@tracbel.com.br',
         'CEN', '010101'),
        ('22222222-2222-4222-8222-222222222222', N'cen.barretos@tracbel.com.br',
         N'Consultor de Negocios — Barretos', N'CEN Barretos', N'cen.barretos@tracbel.com.br',
         'CEN', '010103'),
        ('33333333-3333-4333-8333-333333333333', N'gerente.agro@tracbel.com.br',
         N'Gerente Regional Agro', N'Gerente Agro', N'gerente.agro@tracbel.com.br',
         'GERENTE', '010101')
    ) AS v (IdentidadeExterna, NomePrincipal, NomeCompleto, NomeExibicao, Email, Papel, CodigoDaFilial)
    JOIN organizacao.Empresa e ON e.Codigo = v.CodigoDaFilial
) AS origem
ON destino.NomePrincipal = origem.NomePrincipal
WHEN MATCHED AND (destino.EmpresaId <> origem.EmpresaId OR destino.EstaAtivo = 0)
    THEN UPDATE SET EmpresaId = origem.EmpresaId, EstaAtivo = 1, AlteradoEm = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (ChavePublica, IdentidadeExterna, NomePrincipal, NomeCompleto, NomeExibicao,
                 Email, Papel, EmpresaId, EstaAtivo, CriadoEm, CriadoPorId)
         VALUES (NEWID(), origem.IdentidadeExterna, origem.NomePrincipal, origem.NomeCompleto,
                 origem.NomeExibicao, origem.Email, origem.Papel, origem.EmpresaId, 1,
                 SYSUTCDATETIME(), 0);

COMMIT TRANSACTION;

PRINT 'Usuarios de desenvolvimento aplicados.';

SELECT u.NomePrincipal, u.NomeExibicao, e.Codigo AS Filial, e.Nome AS NomeDaFilial
FROM seguranca.Usuario u
JOIN organizacao.Empresa e ON e.Id = u.EmpresaId
ORDER BY u.NomePrincipal;
