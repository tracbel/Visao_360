using Microsoft.EntityFrameworkCore.Migrations;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Infraestrutura.Migrations
{
    /// <summary>
    /// COLAÇÃO DO BANCO — a outra parte da migração inicial escrita à mão.
    ///
    /// POR QUE À MÃO: o EF Core registra a colação no modelo
    /// (<c>CrmDbContext.OnModelCreating</c> chama <c>UseCollation</c>) e no snapshot, mas não
    /// emite o <c>ALTER DATABASE</c> na migração INICIAL — ele só o gera quando a colação
    /// MUDA de uma migração para outra. Como a criação do banco é justamente o momento em que
    /// a colação importa, o comando entra aqui.
    ///
    /// POR QUE ISSO NÃO É DETALHE: em produção o banco nasce dentro da instância que já
    /// hospeda o Vórtice, e a colação padrão daquela instância é
    /// <c>SQL_Latin1_General_CP1_CI_AS</c> — *accent-SENSITIVE*. Sem este bloco, um
    /// <c>CREATE DATABASE</c> sem <c>COLLATE</c> herdaria a colação do Vórtice, e buscar
    /// "Jose" deixaria de encontrar "José" exatamente como lá. Documento 14, regra 9.5: a
    /// colação nasce num script versionado, nunca de uma configuração feita à mão no servidor.
    ///
    /// POR QUE FORA DE TRANSAÇÃO: <c>ALTER DATABASE ... COLLATE</c> não roda dentro de uma
    /// transação de usuário. O <c>suppressTransaction</c> é o que permite a migração continuar
    /// transacional em tudo o mais.
    ///
    /// É IDEMPOTENTE: se o banco já foi criado com a colação certa — que é o caso quando
    /// <c>scripts/banco/subir-banco.ps1</c> o criou, ou quando o runbook de produção seguiu o
    /// documento 20, seção 6 —, o bloco não faz nada.
    /// </summary>
    public partial class ModeloInicial
    {
        /// <summary>Garante que o banco tem a colação sem distinção de caixa nem de acento.</summary>
        private static void DefinirColacaoDoBanco(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $@"
IF (SELECT collation_name FROM sys.databases WHERE name = DB_NAME())
   <> N'{CrmDbContext.ColacaoSemCaixaNemAcento}'
BEGIN
    DECLARE @sql nvarchar(max) =
        N'ALTER DATABASE ' + QUOTENAME(DB_NAME()) +
        N' COLLATE {CrmDbContext.ColacaoSemCaixaNemAcento};';
    EXEC sys.sp_executesql @sql;
END;
",
                suppressTransaction: true);
        }
    }
}
