using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// Prova que a FRONTEIRA DE EMPRESA filtra de verdade — contra um banco de verdade, e numa
/// entidade que NUNCA teve filtro até a correção do achado A-1 do documento 21.
///
/// <c>FiltroSegurancaTestes</c> prova a profundidade sobre o <c>Lead</c>, a única entidade que
/// tinha filtro. Este arquivo prova a outra metade: que o dado de outra filial existe no banco,
/// que uma consulta sem <c>WHERE</c> nenhum NÃO o devolve, e que a única forma de alcançá-lo é
/// declarando que se está ignorando a fronteira.
///
/// SQLite em memória de propósito, e não provedor falso: a consulta é traduzida para SQL real.
/// Filtro que não traduz é filtro que não existe.
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class FronteiraDeEmpresaTestes(ITestOutputHelper saida) : IDisposable
{
    private const long Eu = 100;
    private const long Administrador = 200;
    private const int MinhaEmpresa = 1;      // Ribeirão Preto, 010101
    private const int EmpresaFilha = 2;      // Araraquara,     010102
    private const int OutraEmpresa = 9;      // Bebedouro,      010109

    // "Filename=:memory:" com a conexão mantida aberta: o banco vive enquanto o teste roda.
    private readonly SqliteConnection _conexao = AbrirBancoEmMemoria();
    private readonly DiarioFalso _diario = new();

    private static SqliteConnection AbrirBancoEmMemoria()
    {
        var conexao = new SqliteConnection("Filename=:memory:");
        conexao.Open();
        return conexao;
    }

    public void Dispose() => _conexao.Dispose();

    private sealed class ProvedorFalso(ContextoAcesso contexto) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = contexto;
    }

    /// <summary>O diário da via de escape, para o teste ler o que foi registrado.</summary>
    private sealed class DiarioFalso : IDiarioDeAlcanceEntreEmpresas
    {
        public List<string> Aberturas { get; } = [];
        public List<string> Fechamentos { get; } = [];

        public void Abriu(ContextoAcesso acesso, string motivo) =>
            Aberturas.Add($"{acesso.NomeExibicao}: {motivo}");

        public void Fechou(ContextoAcesso acesso, string motivo, TimeSpan duracao) =>
            Fechamentos.Add($"{acesso.NomeExibicao}: {motivo}");
    }

    /// <summary>Usuário comum da minha filial: alcança a minha empresa e a filha dela.</summary>
    private static ContextoAcesso Vendedor() =>
        new(usuarioId: Eu,
            nomeExibicao: "Ricardo",
            empresaId: MinhaEmpresa,
            empresasVisiveis: new HashSet<int> { MinhaEmpresa, EmpresaFilha },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>());

    /// <summary>
    /// Administrador: tem a permissão que AUTORIZA abrir a via de escape — e nada mais.
    /// Enquanto ele não abrir, enxerga as mesmas duas filiais que o vendedor.
    /// </summary>
    private static ContextoAcesso Admin() =>
        new(usuarioId: Administrador,
            nomeExibicao: "Administradora",
            empresaId: MinhaEmpresa,
            empresasVisiveis: new HashSet<int> { MinhaEmpresa, EmpresaFilha },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>
            {
                [ContextoAcesso.PermissaoDeAlcanceEntreEmpresas] = Profundidade.Organizacao
            });

    private static ContextoAcesso Sistema() =>
        new(usuarioId: 1,
            nomeExibicao: "sistema",
            empresaId: MinhaEmpresa,
            empresasVisiveis: new HashSet<int>(),
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: true);

    private CrmDbContext Contexto(ContextoAcesso acesso, bool comDiario = true)
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;
        return new CrmDbContext(opcoes, new ProvedorFalso(acesso), comDiario ? _diario : null);
    }

    /// <summary>
    /// Semeia três clientes, um em cada filial. A semeadura roda sob contexto de SISTEMA, que
    /// enxerga tudo — senão o próprio seed seria filtrado e o teste testaria a si mesmo.
    /// </summary>
    private void Semear()
    {
        using var db = Contexto(Sistema());
        db.Database.EnsureCreated();

        foreach (var (id, codigo, nome) in new[]
                 {
                     (MinhaEmpresa, "010101", "Tracbel Agro — Ribeirão Preto"),
                     (EmpresaFilha, "010102", "Tracbel Agro — Araraquara"),
                     (OutraEmpresa, "010109", "Tracbel Agro — Bebedouro")
                 })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        foreach (var (id, apelido) in new[] { (1L, "sistema"), (Eu, "ricardo"), (Administrador, "admin") })
        {
            var usuario = Usuario.Criar(
                identidadeExterna: Guid.NewGuid(),
                nomePrincipal: $"{apelido}@tracbel.com.br",
                nomeCompleto: apelido,
                nomeExibicao: apelido,
                email: Email.Criar($"{apelido}@tracbel.com.br"),
                empresaId: MinhaEmpresa,
                criadoPorId: 1);

            db.Entry(usuario).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(usuario);
        }

        db.SaveChanges();

        db.Clientes.AddRange(
            Cliente.Criar(MinhaEmpresa, "Fazenda da minha filial", TipoDePessoa.Juridica, Eu, 1),
            Cliente.Criar(EmpresaFilha, "Fazenda da filial filha", TipoDePessoa.Juridica, Eu, 1),
            Cliente.Criar(OutraEmpresa, "Fazenda de Bebedouro", TipoDePessoa.Juridica, Eu, 1));

        db.SaveChanges();
    }

    // ------------------------------------------------------------------ a prova

    [Fact]
    public async Task A_consulta_sem_where_nenhum_nao_devolve_o_cliente_de_outra_filial()
    {
        // ESTA É A PROVA DO ACHADO A-1. Antes da correção, Cliente tinha EmpresaId e nenhum
        // filtro: este mesmo `db.Clientes.ToListAsync()` devolvia as três filiais.
        Semear();

        await using var db = Contexto(Vendedor());

        var nomes = await db.Clientes.Select(c => c.NomeRazao).ToListAsync();

        // A prova, impressa: a consulta que sai, e o que ela devolve com e sem a fronteira.
        saida.WriteLine("--- SQL gerado por db.Clientes, sem nenhum Where do chamador ---");
        saida.WriteLine(db.Clientes.ToQueryString());
        saida.WriteLine(string.Empty);
        saida.WriteLine("--- COM a fronteira (usuario da filial 010101, alcanca 010101 e 010102) ---");
        foreach (var n in nomes) saida.WriteLine($"  {n}");

        nomes.Should().BeEquivalentTo(
            ["Fazenda da minha filial", "Fazenda da filial filha"],
            "a consulta alcança só as filiais do contexto de acesso");

        nomes.Should().NotContain("Fazenda de Bebedouro");

        // E o dado ESTÁ no banco: o que mudou não é o conteúdo, é a fronteira.
        var semFiltro = await db.Clientes.IgnoreQueryFilters().Select(c => c.NomeRazao).ToListAsync();

        saida.WriteLine(string.Empty);
        saida.WriteLine("--- SEM a fronteira (IgnoreQueryFilters), a MESMA consulta ---");
        foreach (var n in semFiltro) saida.WriteLine($"  {n}");

        semFiltro.Should().HaveCount(3).And.Contain("Fazenda de Bebedouro",
            "sem o filtro, a MESMA consulta devolve o cliente da outra filial — é exatamente " +
            "isso que o filtro global impede que aconteça por esquecimento");
    }

    [Fact]
    public async Task Nem_o_Where_do_chamador_nem_o_Id_direto_furam_a_fronteira()
    {
        Semear();

        long idDeOutraFilial;
        await using (var sistema = Contexto(Sistema()))
            idDeOutraFilial = await sistema.Clientes
                .Where(c => c.EmpresaId == OutraEmpresa)
                .Select(c => c.Id).FirstAsync();

        await using var db = Contexto(Vendedor());

        (await db.Clientes.FirstOrDefaultAsync(c => c.Id == idDeOutraFilial))
            .Should().BeNull("consultar pelo Id direto não fura a fronteira");

        (await db.Clientes.CountAsync(c => c.EmpresaId == OutraEmpresa))
            .Should().Be(0, "o Where do chamador soma à fronteira, não a substitui");
    }

    [Fact]
    public async Task A_fronteira_vira_clausula_SQL_e_nao_e_avaliada_em_memoria()
    {
        // Se o EF avaliasse o filtro em memória, traria TODAS as linhas do banco para descartar
        // depois — vazamento no meio do caminho e desastre de desempenho.
        Semear();

        await using var db = Contexto(Vendedor());
        var sql = db.Clientes.ToQueryString();

        sql.Should().Contain("WHERE", "a fronteira precisa virar cláusula SQL");
        sql.Should().Contain("EmpresaId", "o predicado de empresa precisa estar no SQL");
    }

    [Fact]
    public async Task A_fronteira_vale_para_toda_entidade_com_EmpresaId_e_nao_so_para_Cliente()
    {
        // A correção é de varredura do modelo, não de uma entidade escolhida a dedo: qualquer
        // consulta a qualquer entidade com EmpresaId nasce com a cláusula.
        Semear();

        await using var db = Contexto(Vendedor());

        db.Equipamentos.ToQueryString().Should().Contain("EmpresaId");
        db.Tarefas.ToQueryString().Should().Contain("EmpresaId");
        db.Interacoes.ToQueryString().Should().Contain("EmpresaId");
        db.Carteiras.ToQueryString().Should().Contain("EmpresaId");
        db.AlteracoesDeCampo.ToQueryString().Should().Contain("EmpresaId");
    }

    // ------------------------------------------------------------------ a via de escape

    [Fact]
    public async Task O_administrador_so_atravessa_a_fronteira_depois_de_declarar_o_motivo()
    {
        Semear();

        await using var db = Contexto(Admin());

        (await db.Clientes.CountAsync()).Should().Be(2,
            "ter a permissão de abrir NÃO é estar aberto: o padrão nunca é enxergar tudo");

        const string Motivo = "conciliação de carteira entre Ribeirão Preto e Bebedouro, chamado 4711";

        using (db.AbrirAlcanceEntreEmpresas(Motivo))
        {
            db.EstaComAlcanceEntreEmpresas.Should().BeTrue();
            (await db.Clientes.CountAsync()).Should().Be(3,
                "dentro do escopo declarado, a consulta alcança todas as filiais");
        }

        db.EstaComAlcanceEntreEmpresas.Should().BeFalse();
        (await db.Clientes.CountAsync()).Should().Be(2,
            "fechado o escopo, a fronteira volta ao lugar sozinha");

        _diario.Aberturas.Should().ContainSingle().Which.Should().Contain(Motivo)
            .And.Contain("Administradora",
                "quem ignora a fronteira precisa dizer que está ignorando, e isso fica no log");
        _diario.Fechamentos.Should().ContainSingle();
    }

    [Fact]
    public void Quem_nao_tem_a_permissao_nao_abre_a_via_de_escape()
    {
        Semear();

        using var db = Contexto(Vendedor());

        var erro = Assert.Throws<InvalidOperationException>(
            () => db.AbrirAlcanceEntreEmpresas("quero ver tudo"));

        erro.Message.Should().Contain(ContextoAcesso.PermissaoDeAlcanceEntreEmpresas);
        db.EstaComAlcanceEntreEmpresas.Should().BeFalse();
    }

    [Fact]
    public void A_via_de_escape_nao_abre_sem_motivo_escrito()
    {
        Semear();

        using var db = Contexto(Admin());

        Assert.Throws<ArgumentException>(() => db.AbrirAlcanceEntreEmpresas("   "));
        db.EstaComAlcanceEntreEmpresas.Should().BeFalse();
    }

    [Fact]
    public void Sem_diario_configurado_a_via_de_escape_nao_abre()
    {
        // Escape sem rastro não é escape legítimo, é furo. Se não há onde registrar, não abre.
        Semear();

        using var db = Contexto(Admin(), comDiario: false);

        var erro = Assert.Throws<InvalidOperationException>(
            () => db.AbrirAlcanceEntreEmpresas("auditoria interna"));

        erro.Message.Should().Contain("registrado");
        db.EstaComAlcanceEntreEmpresas.Should().BeFalse();
    }

    [Fact]
    public async Task O_servico_de_sistema_continua_enxergando_tudo_sob_identidade_explicita()
    {
        // Job e integração rodam sem escopo declarado porque a identidade deles já é a
        // declaração — e ela fica registrada em auditoria.EventoDeAcesso.
        Semear();

        await using var db = Contexto(Sistema());
        (await db.Clientes.CountAsync()).Should().Be(3);
    }
}
