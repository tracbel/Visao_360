using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Conexoes;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CREDENCIAL GRAVADA NA TELA, NO FORMATO QUE A CARGA JÁ LÊ (issue 136).
///
/// <para>A carga sempre leu <c>Protheus__Senha</c>, <c>Art__Servidor</c>, <c>Vortice__Conexao</c>… das variáveis
/// de ambiente. Aqui a credencial de Configurações › Integrações é aberta e SOBREPOSTA a elas, com os mesmos nomes:
/// nenhum caminho da carga muda, e sem credencial na tela tudo continua como estava.</para>
///
/// <para><b>Nada disto é impresso.</b> O aviso diz qual conexão não abriu e por quê — nunca o valor.</para>
/// </summary>
internal static class CredenciaisDaTela
{
    /// <summary>Lê e abre as credenciais da tela. Banco sem a tabela (migração não aplicada) devolve vazio, com aviso.</summary>
    /// <param name="conexaoDoCrm">A cadeia de conexão do CRM.</param>
    /// <param name="configuracao">A configuração do processo, para saber o que o ambiente já tem.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<(IReadOnlyDictionary<string, string?> Valores, IReadOnlyList<string> Avisos)> LerAsync(
        string conexaoDoCrm, IConfiguration configuracao, CancellationToken ct)
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlServer(conexaoDoCrm, sql => sql.CommandTimeout(30)).Options;
        try
        {
            await using var banco = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            return await LerAsync(banco, new ResolvedorDeConexoes(configuracao, new Infraestrutura.Seguranca.ProtetorDeSegredos()), ct);
        }
        catch (DbException falha)
        {
            return (new Dictionary<string, string?>(),
                [$"as credenciais da tela não foram lidas ({falha.GetType().Name}); valem as variáveis de ambiente."]);
        }
    }

    /// <summary>A mesma leitura, sobre um contexto já aberto — é o que o teste exercita.</summary>
    internal static async Task<(IReadOnlyDictionary<string, string?> Valores, IReadOnlyList<string> Avisos)> LerAsync(
        CrmDbContext banco, IResolvedorDeConexoes resolvedor, CancellationToken ct)
    {
        var valores = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var avisos = new List<string>();

        foreach (var conexao in await banco.Conexoes.AsNoTracking().Where(c => c.EhDoSistema).ToListAsync(ct))
        {
            if (resolvedor.OrigemDe(conexao) != OrigemDaCredencial.Tela) continue;
            try
            {
                foreach (var (chave, valor) in ResolvedorDeConexoes.ParaConfiguracao(resolvedor.Resolver(conexao)))
                    valores[chave] = valor;
            }
            catch (InvalidOperationException falha)
            {
                avisos.Add($"{conexao.Nome}: {falha.Message}");
            }
        }

        return (valores, avisos);
    }

    /// <summary>A configuração com a credencial da tela por cima das variáveis de ambiente.</summary>
    public static IConfigurationRoot Sobrepor(IConfiguration configuracao, IReadOnlyDictionary<string, string?> valores) =>
        new ConfigurationBuilder().AddConfiguration(configuracao).AddInMemoryCollection(valores).Build();
}
