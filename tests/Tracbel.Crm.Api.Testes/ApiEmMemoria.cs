using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A API INTEIRA, em memória — o mesmo <c>Program.cs</c>, o mesmo meio de campo de contexto de
/// acesso, o mesmo registro de injeção e o mesmo tratamento de erro.
///
/// <para><b>O que é trocado, e só isto:</b> o provedor de banco. SQL Server vira SQLite em
/// memória, porque o teste não pode depender de um contêiner de pé. Todo o resto é a aplicação de
/// produção — inclusive o filtro global de segurança, que é o que dá valor a este teste: ele
/// atravessa o caminho HTTP de verdade, com os repositórios de verdade.</para>
///
/// <para><b>O que este teste NÃO cobre, e onde isso é coberto:</b> a concorrência otimista, que
/// depende de <c>rowversion</c> do SQL Server; e o caminho de sucesso da ponte do legado, que
/// depende do banco do Vórtice. Os dois foram exercitados contra os bancos reais, e o resultado
/// está no documento 23.</para>
/// </summary>
public sealed class ApiEmMemoria : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>A filial de Ribeirão Preto, onde o usuário padrão dos testes trabalha.</summary>
    public const string FilialDeRibeirao = "010101";

    /// <summary>A filial de Barretos — a "outra filial", que prova a fronteira.</summary>
    public const string FilialDeBarretos = "010103";

    /// <summary>O usuário de Ribeirão Preto.</summary>
    public const string UsuarioDeRibeirao = "cen.ribeiraopreto@tracbel.com.br";

    /// <summary>O usuário de Barretos.</summary>
    public const string UsuarioDeBarretos = "cen.barretos@tracbel.com.br";

    /// <summary>O modelo de máquina semeado.</summary>
    public const string ModeloSemeado = "8R_340";

    // A conexão fica aberta pela vida do teste: "Filename=:memory:" some junto com a última
    // conexão fechada, e cada requisição da API abre e fecha a sua.
    private readonly SqliteConnection _conexao = new("Filename=:memory:");

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(servicos =>
        {
            // Tira o registro de SQL Server que o Program.cs fez e põe SQLite no lugar. É a
            // ÚNICA substituição: nem o contexto de acesso, nem os repositórios, nem o meio de
            // campo são dublados, porque são eles que este teste existe para exercitar.
            //
            // REMOVER O DbContextOptions NÃO BASTA no EF Core 9: o AddDbContext registra também
            // um IDbContextOptionsConfiguration<TContext>, que é onde o UseSqlServer fica. Sem
            // tirar esse, os dois provedores acabam no mesmo contêiner e o EF recusa com
            // "Only a single database provider can be registered" — o erro sai só na primeira
            // consulta, não no registro.
            servicos.RemoveAll<DbContextOptions<CrmDbContext>>();
            servicos.RemoveAll<DbContextOptions>();
            servicos.RemoveAll(typeof(IDbContextOptionsConfiguration<CrmDbContext>));

            _conexao.Open();
            servicos.AddDbContext<CrmDbContext>(opcoes => opcoes.UseSqlite(_conexao));
        });
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        using var escopo = Services.CreateScope();

        // A semeadura roda sob contexto de SISTEMA, que enxerga tudo — senão o próprio seed
        // seria filtrado e o teste testaria a si mesmo.
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        await db.Database.EnsureCreatedAsync();
        Semear(db);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    Task IAsyncLifetime.DisposeAsync()
    {
        _conexao.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>Um cliente HTTP que já se identifica como o usuário e a filial informados.</summary>
    /// <param name="usuario">O nome principal do usuário.</param>
    /// <param name="filial">O código da filial.</param>
    public HttpClient ClienteComo(string usuario, string filial)
    {
        var http = CreateClient();
        http.DefaultRequestHeaders.Add("X-Tracbel-Usuario", usuario);
        http.DefaultRequestHeaders.Add("X-Tracbel-Empresa", filial);
        return http;
    }

    /// <summary>O cliente HTTP do CEN de Ribeirão Preto — o contexto padrão dos testes.</summary>
    public HttpClient ClienteDeRibeirao() => ClienteComo(UsuarioDeRibeirao, FilialDeRibeirao);

    /// <summary>O cliente HTTP do CEN de Barretos — o contexto que prova a fronteira.</summary>
    public HttpClient ClienteDeBarretos() => ClienteComo(UsuarioDeBarretos, FilialDeBarretos);

    private static void Semear(CrmDbContext db)
    {
        foreach (var (id, codigo, nome) in new[]
                 {
                     (1, FilialDeRibeirao, "Tracbel Agro — Ribeirão Preto"),
                     (2, FilialDeBarretos, "Tracbel Agro — Barretos")
                 })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        foreach (var (id, upn, empresaId) in new[]
                 {
                     (100L, UsuarioDeRibeirao, 1),
                     (200L, UsuarioDeBarretos, 2)
                 })
        {
            var usuario = Usuario.Criar(
                identidadeExterna: Guid.NewGuid(),
                nomePrincipal: upn,
                nomeCompleto: upn,
                nomeExibicao: upn.Split('@')[0],
                email: Email.Criar(upn),
                empresaId: empresaId,
                criadoPorId: 1);

            db.Entry(usuario).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(usuario);
        }

        db.CatalogoItens.AddRange(
            CatalogoItem.Criar(CatalogosDeSistema.OrigemDeLead, "INDICACAO", "Indicação", 10),
            CatalogoItem.Criar(CatalogosDeSistema.MotivoDeInativacao, "DUPLICADO", "Cadastro duplicado", 10));

        var marca = Marca.Criar("JOHN_DEERE", "John Deere", ehRepresentada: true);
        db.Marcas.Add(marca);
        db.SaveChanges();

        var familia = Familia.Criar(marca.Id, "A_CONFIRMAR", "A confirmar");
        db.Familias.Add(familia);
        db.SaveChanges();

        db.Modelos.Add(Modelo.Criar(familia.Id, ModeloSemeado, "8R 340"));
    }
}
