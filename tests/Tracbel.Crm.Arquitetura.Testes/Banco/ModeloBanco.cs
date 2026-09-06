using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Constrói o modelo do <see cref="CrmDbContext"/> com o provedor **SQL Server**, sem nunca
/// abrir uma conexão de verdade.
///
/// Os testes deste diretório (pasta <c>Banco/</c>) verificam o padrão de banco de dados —
/// documento <c>docs/projeto/14-PADRAO-DE-BANCO.md</c> — contra o MODELO do EF Core, nunca
/// contra um banco real. Usar o provedor SQL Server (e não o SQLite dos testes de
/// comportamento, como <c>FiltroSegurancaTestes</c>) importa: é o provedor SQL Server quem
/// resolve o tipo de coluna que realmente vai para o banco (<c>nvarchar(200)</c>,
/// <c>decimal(18,2)</c>, <c>datetime2(3)</c>...). É exatamente isso que o padrão regula.
///
/// Nenhuma conexão é aberta — construir o <see cref="IModel"/> só precisa do provedor
/// configurado, nunca de um servidor respondendo.
/// </summary>
internal static class ModeloBanco
{
    /// <summary>O modelo do EF Core, resolvido como será no SQL Server real. Construído uma
    /// única vez e reaproveitado por todos os testes — montar o modelo é caro.
    ///
    /// Usa o modelo de DESIGN TIME (<see cref="IDesignTimeModel"/>), não <c>DbContext.Model</c>.
    /// O modelo "read-optimized" que <c>DbContext.Model</c> devolve em runtime descarta
    /// metadados que a aplicação nunca precisa para executar consulta — check constraints
    /// entre eles — e lança <see cref="InvalidOperationException"/> se alguém tentar lê-los.
    /// Como o padrão de banco depende de inspecionar check constraint, o modelo de design
    /// time (o mesmo que a ferramenta de migration usa) é o correto aqui.</summary>
    public static IModel Modelo { get; } = Construir().GetService<IDesignTimeModel>().Model;

    private static CrmDbContext Construir()
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            // Connection string nunca é usada para conectar — só existe para o provedor
            // SQL Server resolver os tipos de coluna. Nenhum Database.* é chamado aqui.
            .UseSqlServer("Server=localhost;Database=TracbelCrmModeloDeTeste;Integrated Security=true")
            .Options;

        return new CrmDbContext(opcoes, new ProvedorDeSistemaFalso());
    }

    /// <summary>Contexto de acesso mínimo, só para satisfazer o construtor do DbContext.</summary>
    private sealed class ProvedorDeSistemaFalso : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = new(
            usuarioId: 0,
            nomeExibicao: "sistema",
            empresaId: 0,
            empresasVisiveis: new HashSet<int>(),
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: true);
    }
}
