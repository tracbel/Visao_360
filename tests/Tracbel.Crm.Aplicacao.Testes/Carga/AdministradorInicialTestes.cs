using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O PRIMEIRO ADMINISTRADOR (<c>--conceder-administrador-inicial</c>). O que estes testes prendem: a concessão
/// sai pelo domínio, com justificativa e dentro da trilha, em nome da própria conta; acontece uma vez só; e
/// a conta que ainda espera liberação só é liberada com a filial dita na linha de comando.
///
/// <para>SQLite em memória com o modelo de verdade, e não dublê: a trilha e o índice são do banco.</para>
/// </summary>
public sealed class AdministradorInicialTestes : IDisposable
{
    private const string Fulano = "fulano.admin@tracbel.com.br";
    private const string Outra = "outra.pessoa@tracbel.com.br";
    private const string Nova = "pessoa.nova@tracbel.com.br";
    private const long FulanoId = 100;
    private const long OutraId = 200;

    private static readonly DateTime Agora = new(2026, 9, 21, 22, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;

    public AdministradorInicialTestes()
    {
        _conexao.Open();

        // A colação que os CHECKs da trilha citam; no SQL Server ela diferencia caixa e acento.
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();

        foreach (var (id, codigo, nome) in new[] { (1, "010101", "Ribeirão Preto"), (2, "010103", "Barretos") })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        foreach (var (id, upn, empresaId) in new[] { (FulanoId, Fulano, 1), (OutraId, Outra, 2) })
        {
            var usuario = Usuario.Criar(Guid.NewGuid(), upn, upn, upn.Split('@')[0], Email.Criar(upn), empresaId, criadoPorId: 1);
            db.Entry(usuario).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(usuario);
        }

        // A CONTA QUE NASCEU NO PRIMEIRO LOGIN (issue 128): filial provisória, aguardando liberação.
        db.Usuarios.Add(Usuario.CriarNoPrimeiroLogin(Guid.NewGuid(), Nova, "Pessoa Nova", Email.Criar(Nova), 1, Agora.AddHours(-1)));
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private Task<AdministradorInicial.Desfecho> Conceder(string? conta, string? justificativa = "autorizado pela diretoria em 21/09/2026", string? filial = null, bool simular = false) =>
        AdministradorInicial.ConcederAsync(_opcoes, conta, justificativa, filial, simular, Agora, CancellationToken.None);

    private async Task<List<UsuarioPerfil>> ConcessoesDeAdministradorAsync()
    {
        await using var db = Sistema();
        var perfil = await db.Perfis.SingleAsync(p => p.Codigo == PerfisDeSistema.Administrador);
        return await db.UsuariosPerfis.AsNoTracking().Where(c => c.PerfilId == perfil.Id).ToListAsync();
    }

    [Fact]
    public async Task Concede_pelo_dominio_em_nome_da_propria_conta_com_justificativa_e_trilha()
    {
        var desfecho = await Conceder("Fulano.Admin@Tracbel.com.br");

        desfecho.Codigo.Should().Be(0, string.Join(" | ", desfecho.Linhas));
        desfecho.Linhas.Should().Contain(l => l.StartsWith("CONCEDIDO", StringComparison.Ordinal));

        var concessao = (await ConcessoesDeAdministradorAsync()).Should().ContainSingle().Subject;
        concessao.UsuarioId.Should().Be(FulanoId);
        concessao.ConcedidoPorId.Should().Be(FulanoId, "quem roda o comando é a própria conta");
        concessao.EmpresaId.Should().BeNull("vale em todas as filiais");
        concessao.ExpiraEm.Should().BeNull();
        concessao.Justificativa.Should().Be(AdministradorInicial.PrefixoDaJustificativa + "autorizado pela diretoria em 21/09/2026");

        // A INCLUSÃO ENTRA NA TRILHA porque a origem é "usuário" — com origem "sistema", não entraria.
        await using var db = Sistema();
        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(UsuarioPerfil)).ToListAsync();
        trilha.Should().NotBeEmpty();
        trilha.Should().OnlyContain(a => a.AlteradoPorId == FulanoId && a.Origem == OrigemDaOperacao.Usuario);
        trilha.Should().Contain(a => a.Campo == nameof(UsuarioPerfil.Justificativa));
    }

    [Fact]
    public async Task Simular_mostra_o_que_faria_e_nao_grava_nada()
    {
        var desfecho = await Conceder(Fulano, simular: true);

        desfecho.Codigo.Should().Be(0);
        desfecho.Linhas.Should().Contain(l => l.StartsWith("CONCEDIDO", StringComparison.Ordinal));
        desfecho.Linhas.Should().Contain(l => l.StartsWith("SIMULAÇÃO", StringComparison.Ordinal));

        (await ConcessoesDeAdministradorAsync()).Should().BeEmpty();
        await using var db = Sistema();
        (await db.AlteracoesDeCampo.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Uma_vez_so_com_administrador_ativo_recusa_e_para_ele_mesmo_nao_faz_nada()
    {
        (await Conceder(Fulano)).Codigo.Should().Be(0);

        var outra = await Conceder(Outra);
        outra.Codigo.Should().Be(2);
        outra.Linhas.Should().Contain(l => l.Contains(Fulano, StringComparison.Ordinal));
        outra.Linhas.Should().Contain("Nada foi gravado.");

        var denovo = await Conceder(Fulano);
        denovo.Codigo.Should().Be(0);
        denovo.Linhas.Should().ContainSingle().Which.Should().Contain("já é administrador");

        (await ConcessoesDeAdministradorAsync()).Should().ContainSingle().Which.UsuarioId.Should().Be(FulanoId);
    }

    [Fact]
    public async Task Administrador_desativado_nao_conta_como_administrador()
    {
        (await Conceder(Outra)).Codigo.Should().Be(0);

        await using (var db = Sistema())
        {
            var outra = await db.Usuarios.SingleAsync(u => u.Id == OutraId);
            outra.Desativar(FulanoId);
            await db.SaveChangesAsync();
        }

        (await Conceder(Fulano)).Codigo.Should().Be(0, "a única conta administradora não entra mais no CRM");
    }

    [Fact]
    public async Task Conta_que_aguarda_liberacao_precisa_da_filial_e_e_liberada_nela()
    {
        var semFilial = await Conceder(Nova);
        semFilial.Codigo.Should().Be(2);
        semFilial.Linhas.Should().Contain(l => l.Contains("--filial", StringComparison.Ordinal));
        semFilial.Linhas.Should().Contain(l => l.Contains("010103 Barretos", StringComparison.Ordinal));

        var filialErrada = await Conceder(Nova, filial: "999999");
        filialErrada.Codigo.Should().Be(2);

        var desfecho = await Conceder(Nova, filial: "010103");
        desfecho.Codigo.Should().Be(0, string.Join(" | ", desfecho.Linhas));
        desfecho.Linhas.Should().Contain(l => l.StartsWith("LIBERADA", StringComparison.Ordinal));

        await using var db = Sistema();
        var conta = await db.Usuarios.AsNoTracking().SingleAsync(u => u.NomePrincipal == Nova);
        conta.AguardaLiberacao.Should().BeFalse();
        conta.EmpresaId.Should().Be(2);

        // A MUDANÇA DE FILIAL ENTRA NA TRILHA, feita pela própria conta.
        (await db.AlteracoesDeCampo.AsNoTracking()
                .Where(a => a.Entidade == nameof(Usuario) && a.Campo == nameof(Usuario.EmpresaId) && a.RegistroId == conta.Id)
                .SingleAsync())
            .AlteradoPorId.Should().Be(conta.Id);
    }

    [Theory]
    [InlineData(null, "motivo")]
    [InlineData("sem-arroba", "motivo")]
    [InlineData(Fulano, null)]
    [InlineData(Fulano, "  ")]
    [InlineData("ninguem@tracbel.com.br", "motivo")]
    public async Task Conta_e_justificativa_sao_obrigatorias_e_a_conta_precisa_existir(string? conta, string? justificativa)
    {
        var desfecho = await Conceder(conta, justificativa);

        desfecho.Codigo.Should().Be(2);
        desfecho.Linhas.Should().Contain("Nada foi gravado.");
        (await ConcessoesDeAdministradorAsync()).Should().BeEmpty();
    }
}
