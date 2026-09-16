/* =================================================================================================
   PERFIS DE TESTE DA VISÃO DA EMPRESA — só no banco de validação (documento 32, seção 8.5.2).

   Recusa rodar em qualquer outro banco. Nenhum usuário real recebe acesso: os dois perfis são
   fictícios, de natureza Teste, e a concessão vence em 7 dias. A distribuição oficial dos acessos
   continua pendente de decisão (documento 32, P-10).

   A API só honra a concessão pela ponte provisória em Desenvolvimento (OpcoesDeContextoProvisorio).
   Reexecutável: o que já existe não é gravado de novo.
   ================================================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
-- O sqlcmd abre a sessão com QUOTED_IDENTIFIER desligado, e seguranca.Usuario tem índice filtrado.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF DB_NAME() <> N'TracbelCrmValidacao'
BEGIN
    RAISERROR (N'Este script só roda no banco TracbelCrmValidacao. Nada foi gravado.', 16, 1);
    RETURN;
END;

DECLARE @empresa int = (SELECT Id FROM organizacao.Empresa WHERE Codigo = '010101');
-- QUEM GRAVA É A CONTA DE SISTEMA, e não uma pessoa: a trilha não pode atribuir a concessão de teste a
-- alguém que não a fez.
DECLARE @quem bigint = (SELECT TOP 1 Id FROM seguranca.Usuario WHERE Natureza = 'Sistema' ORDER BY Id);

IF @quem IS NULL
BEGIN
    RAISERROR (N'Não há conta de sistema para registrar quem gravou. Nada foi gravado.', 16, 1);
    RETURN;
END;
DECLARE @agora datetime2(3) = SYSUTCDATETIME();

BEGIN TRANSACTION;

INSERT seguranca.Usuario (IdentidadeExterna, NomePrincipal, NomeCompleto, NomeExibicao, Email, EmpresaId, EstaAtivo, CriadoEm, CriadoPorId, Natureza)
SELECT NEWID(), f.NomePrincipal, f.NomeCompleto, f.NomeExibicao, f.NomePrincipal, @empresa, 1, @agora, @quem, 'Teste'
FROM (VALUES
        (N'perfil.autorizado@exemplo.com', N'Perfil de teste — autorizado', N'Teste autorizado'),
        (N'perfil.sem.acesso@exemplo.com', N'Perfil de teste — sem acesso', N'Teste sem acesso')
     ) AS f (NomePrincipal, NomeCompleto, NomeExibicao)
WHERE NOT EXISTS (SELECT 1 FROM seguranca.Usuario u WHERE u.NomePrincipal = f.NomePrincipal);

IF NOT EXISTS (SELECT 1 FROM seguranca.ConjuntoDePermissao WHERE Codigo = 'VALIDACAO_VISAO_EMPRESA')
    INSERT seguranca.ConjuntoDePermissao (Codigo, Nome, Descricao, EstaAtivo)
    VALUES ('VALIDACAO_VISAO_EMPRESA', N'Validação — visão da empresa',
            N'Só para o banco de validação (documento 32). Não é distribuição oficial de acesso.', 1);

DECLARE @conjunto int = (SELECT Id FROM seguranca.ConjuntoDePermissao WHERE Codigo = 'VALIDACAO_VISAO_EMPRESA');

IF NOT EXISTS (SELECT 1 FROM seguranca.ConjuntoDePermissaoItem WHERE ConjuntoPermissaoId = @conjunto AND CodigoPermissao = 'Empresa.AlcanceEntreFiliais')
    INSERT seguranca.ConjuntoDePermissaoItem (ConjuntoPermissaoId, CodigoPermissao, Profundidade)
    VALUES (@conjunto, 'Empresa.AlcanceEntreFiliais', 'Organizacao');

DECLARE @autorizado bigint = (SELECT Id FROM seguranca.Usuario WHERE NomePrincipal = N'perfil.autorizado@exemplo.com');

IF NOT EXISTS (SELECT 1 FROM seguranca.UsuarioConjuntoDePermissao WHERE UsuarioId = @autorizado AND ConjuntoPermissaoId = @conjunto)
    INSERT seguranca.UsuarioConjuntoDePermissao (UsuarioId, ConjuntoPermissaoId, ConcedidoEm, ConcedidoPorId, ExpiraEm)
    VALUES (@autorizado, @conjunto, @agora, @quem, DATEADD(DAY, 7, @agora));

COMMIT TRANSACTION;

SELECT u.NomePrincipal, u.Natureza, u.EstaAtivo, c.Codigo AS Conjunto, i.CodigoPermissao, i.Profundidade, g.ExpiraEm
FROM seguranca.Usuario u
LEFT JOIN seguranca.UsuarioConjuntoDePermissao g ON g.UsuarioId = u.Id
LEFT JOIN seguranca.ConjuntoDePermissao c ON c.Id = g.ConjuntoPermissaoId
LEFT JOIN seguranca.ConjuntoDePermissaoItem i ON i.ConjuntoPermissaoId = c.Id
WHERE u.NomePrincipal LIKE N'perfil.%@exemplo.com';

-- A PROVA de que nenhum usuário real recebeu concessão neste banco.
SELECT COUNT(*) AS ConcessoesParaQuemNaoEPerfilDeTeste
FROM seguranca.UsuarioConjuntoDePermissao g
JOIN seguranca.Usuario u ON u.Id = g.UsuarioId
WHERE u.Natureza <> 'Teste';
