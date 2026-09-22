using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O PRIMEIRO ADMINISTRADOR — <c>--conceder-administrador-inicial &lt;nome principal&gt; --justificativa "..."</c>.
///
/// <para><b>Por que existe.</b> Conceder perfil é trabalho da tela de administração (issue 113), e a tela
/// exige quem já administra. No banco novo não há ninguém: alguém precisa ser o primeiro, e a alternativa
/// seria um <c>INSERT</c> à mão em <c>seguranca.UsuarioPerfil</c> — sem justificativa, sem trilha e sem
/// regra nenhuma. Este comando faz a mesma concessão pelo domínio (<see cref="UsuarioPerfil.Conceder"/>),
/// com justificativa obrigatória e dentro da trilha de auditoria.</para>
///
/// <para><b>Uma vez só.</b> Se já existe administrador ativo, ele recusa: daí em diante quem concede é quem
/// administra, pela tela. Não é uma porta dos fundos permanente — é o passo de instalação.</para>
///
/// <para><b>Em nome da própria pessoa.</b> A concessão e a liberação ficam registradas como feitas pela
/// conta que vai administrar, com origem "usuário", porque é o que acontece: é ela quem roda o comando
/// (pelo <c>scripts/deploy/conceder-administrador-inicial.ps1</c>). Com origem "sistema", a inclusão nem
/// entraria na trilha (<c>PoliticaDeAuditoria.RegistraInclusao</c>).</para>
///
/// <para><b>Conta que ainda espera liberação</b> (nasceu no primeiro login, issue 128) é liberada junto,
/// na filial de <c>--filial</c> — é a única liberação sem outra pessoa (<see cref="Usuario.LiberarNaInstalacao"/>).
/// Com <c>--simular</c>, tudo roda numa transação desfeita no fim: a saída mostra o que aconteceria.</para>
/// </summary>
internal static class AdministradorInicial
{
    /// <summary>A opção da linha de comando.</summary>
    public const string Opcao = "--conceder-administrador-inicial";

    /// <summary>O prefixo da justificativa gravada, para a concessão dizer de onde veio.</summary>
    public const string PrefixoDaJustificativa = "Primeiro administrador, pelo comando de instalação: ";

    /// <summary>O que aconteceu, e o código de saída do processo.</summary>
    /// <param name="Codigo">0 = feito (ou já estava feito), 2 = recusado sem gravar nada.</param>
    /// <param name="Linhas">O que dizer a quem rodou.</param>
    internal sealed record Desfecho(int Codigo, IReadOnlyList<string> Linhas)
    {
        public static Desfecho Recusa(params string[] linhas) => new(2, [.. linhas, "Nada foi gravado."]);
    }

    /// <summary>Lê a linha de comando e a configuração, executa e escreve o desfecho.</summary>
    /// <param name="args">A linha de comando.</param>
    public static async Task<int> RodarAsync(string[] args)
    {
        var nomePrincipal = ValorDepois(args, Opcao);
        var justificativa = ValorDepois(args, "--justificativa");
        var filial = ValorDepois(args, "--filial");
        var simular = args.Contains("--simular", StringComparer.Ordinal);

        var configuracao = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conexao = configuracao.GetConnectionString("Crm");
        if (string.IsNullOrWhiteSpace(conexao))
        {
            Console.Error.WriteLine("Não achei a cadeia de conexão do CRM. Defina ConnectionStrings__Crm. Nada foi gravado.");
            return 2;
        }

        var opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlServer(conexao).Options;

        Desfecho desfecho;
        try
        {
            desfecho = await ConcederAsync(opcoes, nomePrincipal, justificativa, filial, simular, DateTime.UtcNow, CancellationToken.None);
        }
        catch (Exception falha) when (falha is DbException or DbUpdateException or RegraDeNegocioViolada or InvalidOperationException)
        {
            // UMA FRASE, E NÃO A PILHA: coluna que falta (migração ainda não aplicada), banco fora do ar,
            // permissão negada. A transação não foi confirmada, então nada ficou gravado pela metade.
            Console.Error.WriteLine("O COMANDO PAROU, e nada foi gravado: " + falha.GetBaseException().Message);
            return 3;
        }

        foreach (var linha in desfecho.Linhas)
            (desfecho.Codigo == 0 ? Console.Out : Console.Error).WriteLine(linha);

        return desfecho.Codigo;
    }

    /// <summary>
    /// Concede o perfil Administrador à conta, se ainda não existe administrador nenhum.
    /// </summary>
    /// <param name="opcoes">O banco do CRM.</param>
    /// <param name="nomePrincipal">A conta, pelo nome principal (o e-mail do Entra).</param>
    /// <param name="justificativa">Por que, e por autorização de quem. Obrigatória.</param>
    /// <param name="codigoDaFilial">A filial de casa, só para conta que ainda espera liberação.</param>
    /// <param name="simular">Desfaz tudo no fim.</param>
    /// <param name="agoraUtc">O instante da concessão.</param>
    /// <param name="ct">Cancelamento.</param>
    internal static async Task<Desfecho> ConcederAsync(
        DbContextOptions<CrmDbContext> opcoes,
        string? nomePrincipal,
        string? justificativa,
        string? codigoDaFilial,
        bool simular,
        DateTime agoraUtc,
        CancellationToken ct)
    {
        var alvo = nomePrincipal?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(alvo) || !alvo.Contains('@', StringComparison.Ordinal))
            return Desfecho.Recusa($"Informe a conta: {Opcao} <nome principal>, o e-mail da conta Microsoft.");

        if (string.IsNullOrWhiteSpace(justificativa))
            return Desfecho.Recusa("Informe --justificativa \"...\": por que esta conta é a primeira administradora, e por autorização de quem.");

        // ---- 1. O que existe — lido como sistema, porque ninguém tem escopo ainda ------------------
        await using var leitura = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        var perfil = await leitura.Perfis.AsNoTracking()
            .Where(p => p.Codigo == PerfisDeSistema.Administrador)
            .Select(p => new { p.Id, p.EstaAtivo })
            .FirstOrDefaultAsync(ct);

        if (perfil is null || !perfil.EstaAtivo)
            return Desfecho.Recusa(
                $"O perfil {PerfisDeSistema.Administrador} não existe ou está desativado. A migração da fase 3 " +
                "(PerfilDeAcessoESubstituicaoDoPapel) semeia o perfil — confira se ela foi aplicada.");

        // A CAIXA NÃO IMPORTA: o SQL Server compara sem diferenciar, e o e-mail pode chegar digitado de outro jeito.
        var usuario = await leitura.Usuarios.AsNoTracking()
            .Where(u => u.NomePrincipal.ToLower() == alvo && u.ExcluidoEm == null)
            .Select(u => new { u.Id, u.NomePrincipal, u.NomeExibicao, u.EstaAtivo, u.EmpresaId, u.AguardandoLiberacaoDesde })
            .FirstOrDefaultAsync(ct);

        if (usuario is null)
            return Desfecho.Recusa(
                $"Não há usuário com o nome principal {alvo}.",
                "Entre uma vez no CRM com esta conta Microsoft — o primeiro login de quem está no grupo do Entra cria " +
                "o usuário, aguardando liberação — e rode o comando de novo, com --filial.");

        if (!usuario.EstaAtivo)
            return Desfecho.Recusa($"O usuário {usuario.NomePrincipal} está desativado no CRM.");

        var administradores = await (
                from concessao in leitura.UsuariosPerfis.AsNoTracking()
                join u in leitura.Usuarios.AsNoTracking() on concessao.UsuarioId equals u.Id
                where concessao.PerfilId == perfil.Id
                      && concessao.RevogadaEm == null && (concessao.ExpiraEm == null || concessao.ExpiraEm > agoraUtc)
                      && u.EstaAtivo && u.ExcluidoEm == null && u.AguardandoLiberacaoDesde == null
                select new { u.Id, u.NomePrincipal })
            .Distinct()
            .ToListAsync(ct);

        if (administradores.Any(a => a.Id == usuario.Id))
            return new Desfecho(0, [$"{usuario.NomePrincipal} já é administrador. Nada foi feito."]);

        if (administradores.Count > 0)
            return Desfecho.Recusa(
                "Já existe administrador ativo: " + string.Join(", ", administradores.Select(a => a.NomePrincipal).Order(StringComparer.Ordinal)) + ".",
                "O primeiro administrador só se concede uma vez; daqui em diante, quem administra concede pela tela.");

        // ---- 2. A filial, se a conta ainda espera liberação --------------------------------------
        var aguarda = usuario.AguardandoLiberacaoDesde is not null;
        var filialDeCasa = usuario.EmpresaId;
        string? nomeDaFilial = null;
        var avisos = new List<string>();

        if (aguarda)
        {
            if (string.IsNullOrWhiteSpace(codigoDaFilial))
            {
                var ativas = await leitura.Empresas.AsNoTracking()
                    .Where(e => e.EstaAtiva)
                    .OrderBy(e => e.Codigo)
                    .Select(e => e.Codigo + " " + e.Nome)
                    .ToListAsync(ct);

                return Desfecho.Recusa(
                    $"A conta {usuario.NomePrincipal} está aguardando liberação e precisa de uma filial de casa: acrescente --filial <código>.",
                    "Filiais ativas: " + string.Join("; ", ativas) + ".");
            }

            var codigo = codigoDaFilial.Trim();
            var escolhida = await leitura.Empresas.AsNoTracking()
                .Where(e => e.EstaAtiva && e.Codigo == codigo)
                .Select(e => new { e.Id, e.Nome })
                .FirstOrDefaultAsync(ct);

            if (escolhida is null)
                return Desfecho.Recusa($"Não há filial ativa com o código {codigo}.");

            filialDeCasa = escolhida.Id;
            nomeDaFilial = escolhida.Nome;
        }
        else if (!string.IsNullOrWhiteSpace(codigoDaFilial))
        {
            avisos.Add("A conta já tem filial de casa; --filial foi ignorado (a filial se muda pela tela de administração).");
        }

        // ---- 3. A gravação — em nome da própria conta, dentro da trilha ---------------------------
        var acesso = new ContextoFixo(new ContextoAcesso(
            usuarioId: usuario.Id,
            nomeExibicao: usuario.NomeExibicao,
            empresaId: filialDeCasa,
            empresasVisiveis: new HashSet<int> { filialDeCasa },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(StringComparer.Ordinal)));

        await using var gravacao = new CrmDbContext(opcoes, acesso);
        await using var transacao = await gravacao.Database.BeginTransactionAsync(ct);

        var linhas = new List<string>(avisos);

        if (aguarda)
        {
            var conta = await gravacao.Usuarios.SingleAsync(u => u.Id == usuario.Id, ct);
            conta.LiberarNaInstalacao(filialDeCasa);
            linhas.Add($"LIBERADA: a conta {usuario.NomePrincipal}, com filial de casa {codigoDaFilial!.Trim()} {nomeDaFilial}.");
        }

        var texto = PrefixoDaJustificativa + justificativa.Trim();
        gravacao.UsuariosPerfis.Add(UsuarioPerfil.Conceder(usuario.Id, perfil.Id, texto, usuario.Id, agoraUtc));
        await gravacao.SaveChangesAsync(ct);

        linhas.Add($"CONCEDIDO: perfil Administrador a {usuario.NomePrincipal} (usuário {usuario.Id}), em todas as filiais, sem data para expirar.");
        linhas.Add($"  justificativa: {texto}");

        if (simular)
        {
            await transacao.RollbackAsync(ct);
            linhas.Add("SIMULAÇÃO: tudo foi desfeito, e nada foi gravado. Rode sem --simular para valer.");
        }
        else
        {
            await transacao.CommitAsync(ct);
            linhas.Add("Gravado, com a trilha de auditoria. No próximo acesso ao CRM a conta já entra como administradora.");
        }

        return new Desfecho(0, linhas);
    }

    /// <summary>O valor que vem logo depois de uma opção, ou nulo.</summary>
    private static string? ValorDepois(string[] args, string opcao)
    {
        var i = Array.IndexOf(args, opcao);
        return i >= 0 && i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) ? args[i + 1] : null;
    }

    /// <summary>O contexto da conta que está virando administradora.</summary>
    private sealed class ContextoFixo(ContextoAcesso atual) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = atual;
    }
}
