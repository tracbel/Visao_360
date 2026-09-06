using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia;

/// <summary>
/// COMO A FERRAMENTA DE MIGRATION CONSTRÓI O CONTEXTO — e por que ela precisa de uma fábrica.
///
/// <para>O construtor do <see cref="CrmDbContext"/> lê <c>IProvedorContextoAcesso.Atual</c> na
/// primeira linha, e esse provedor lança quando ninguém definiu quem está agindo — de propósito,
/// porque um contexto de acesso vazio em silêncio faria toda consulta filtrar pela empresa 0 e
/// "sumir" com o dado (documento 03, convenção 7). O preço disso é que
/// <c>dotnet ef migrations add</c>, que não tem requisição HTTP nenhuma, não consegue instanciar
/// o contexto pelo contêiner da API.</para>
///
/// <para>Esta fábrica é a resposta padrão do EF Core para exatamente esse caso. Ela existe para
/// a regra 9.1 do documento 14 poder ser cumprida — *"toda alteração de schema nasce de
/// <c>dotnet ef migrations add</c>"* — sem afrouxar a checagem do contexto de acesso em
/// produção.</para>
///
/// <para><b>Ela nunca conecta em nada e nunca lê dado.</b> A cadeia de conexão abaixo só serve
/// para o provedor SQL Server resolver os tipos de coluna (<c>nvarchar(200)</c>,
/// <c>datetime2(3)</c>...) ao montar o modelo — é a mesma técnica do <c>ModeloBanco</c> dos
/// testes de arquitetura. Quando é preciso apontar para um banco de verdade (por exemplo
/// <c>dotnet ef database update</c>), a cadeia vem da variável de ambiente
/// <c>ConnectionStrings__Crm</c>, nunca de credencial versionada aqui.</para>
/// </summary>
public sealed class FabricaDeContextoEmTempoDeDesenho : IDesignTimeDbContextFactory<CrmDbContext>
{
    private const string ConexaoSoParaResolverTipos =
        "Server=localhost;Database=TracbelCrmModeloEmTempoDeDesenho;Integrated Security=true";

    /// <summary>Constrói o contexto para a ferramenta de linha de comando do EF Core.</summary>
    /// <param name="args">Argumentos da ferramenta. Não usados.</param>
    public CrmDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__Crm");

        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlServer(
                string.IsNullOrWhiteSpace(conexao) ? ConexaoSoParaResolverTipos : conexao,
                sql => sql
                    // A MESMA TABELA DE HISTÓRICO QUE A APLICAÇÃO USA, e a repetição aqui não é
                    // descuido: sem ela a ferramenta procuraria o histórico em `dbo`, não o
                    // acharia, e tentaria aplicar a migração inicial por cima de um banco já
                    // migrado — o erro "There is already an object named 'CampoAuditado'".
                    // A regra que põe a tabela em `metadado` é a do documento 14, seção 2:
                    // nada mora em `dbo`, nem o controle de versão do próprio esquema.
                    .MigrationsHistoryTable("__EFMigrationsHistory", "metadado")
                    .CommandTimeout(180))
            .Options;

        return new CrmDbContext(opcoes, new ContextoDeFerramenta());
    }

    /// <summary>
    /// O contexto de acesso mínimo da FERRAMENTA — nunca da aplicação.
    ///
    /// <para>Declara-se serviço de sistema porque a ferramenta de migration não representa
    /// pessoa nenhuma, e porque o único uso que ela faz do contexto é montar o modelo. Este tipo
    /// é privado e não sai daqui: não existe caminho de aplicação que o alcance.</para>
    /// </summary>
    private sealed class ContextoDeFerramenta : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = new(
            usuarioId: 0,
            nomeExibicao: "ferramenta de migração",
            empresaId: 0,
            empresasVisiveis: new HashSet<int>(),
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: true);
    }
}
