using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Seguranca;

/// <summary>
/// A PONTE PROVISÓRIA E A CONCESSÃO EXPLÍCITA (documento 32, seção 8.5.2).
///
/// <para>Cabeçalho não autentica ninguém. A concessão de alcance de organização, gravada para um
/// usuário, só pode valer pela ponte quando a configuração manda — e o <c>Program.cs</c> da API só manda
/// em Desenvolvimento. Este teste prova as duas metades com o resolvedor de verdade.</para>
/// </summary>
[Trait("Categoria", "Seguranca")]
public sealed class ConcessaoExplicitaNaPonteProvisoriaTestes : IDisposable
{
    private const string Autorizado = "perfil.autorizado@exemplo.com";

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;

    public ConcessaoExplicitaNaPonteProvisoriaTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureCreated();

        var empresa = Empresa.Criar("010101", "Tracbel Agro — Ribeirão Preto");
        db.Empresas.Add(empresa);
        db.SaveChanges();

        var usuario = Usuario.Criar(
            Guid.NewGuid(), Autorizado, Autorizado, "perfil.autorizado", Email.Criar(Autorizado), empresa.Id, criadoPorId: 1);
        var conjunto = Perfil.Criar("TESTE_VISAO_EMPRESA", "Teste — visão da empresa")
            .Conceder(ContextoAcesso.PermissaoDeAlcanceEntreEmpresas, Profundidade.Organizacao);

        db.Usuarios.Add(usuario);
        db.Perfis.Add(conjunto);
        db.SaveChanges();

        db.UsuariosPerfis.Add(UsuarioPerfil.Conceder(usuario.Id, conjunto.Id, "teste da ponte provisória", usuario.Id, DateTime.UtcNow));
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task A_ponte_so_honra_a_concessao_explicita_quando_a_configuracao_manda(bool honrar, bool abreAEmpresa)
    {
        var resolvedor = new ResolvedorDeContextoProvisorio(
            _opcoes,
            Options.Create(new OpcoesDeContextoProvisorio { HonrarConcessoesExplicitas = honrar }),
            NullLogger<ResolvedorDeContextoProvisorio>.Instance);

        var contexto = await resolvedor.ResolverAsync(Autorizado, "010101", CancellationToken.None);

        contexto.EhSucesso.Should().BeTrue(contexto.Erro ?? string.Empty);
        contexto.Valor!.PodeAlcancarTodasAsEmpresas.Should().Be(abreAEmpresa,
            "fora de Desenvolvimento a ponte não honra concessão: cabeçalho não autentica ninguém");
        contexto.Valor.ProfundidadeDe("Cliente.Ler").Should().Be(Profundidade.EmpresaEAbaixo,
            "a lista mínima continua a mesma nos dois casos");
    }
}
