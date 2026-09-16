using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// AS REGRAS DA TRILHA AUTOMÁTICA contra um banco de verdade (documento 41, fase 2).
///
/// <para>O que a API sozinha não exercita: a origem declarada por quem grava, o nascimento que vem
/// de integração e não entra na trilha, e — o mais importante — a garantia de que <b>dado auditado
/// não é gravado sem a sua trilha</b>. Quando a linha da trilha não pode ser gravada, a exceção
/// sobe e o dado fica como estava.</para>
///
/// <para>SQLite em memória, como os demais testes de persistência. A chave composta da trilha
/// (<c>Id</c>, <c>AlteradoEm</c>) não gera <c>Id</c> no SQLite; a gravação da trilha já trata isso
/// fora do SQL Server, e este teste passa por esse caminho.</para>
/// </summary>
[Trait("Categoria", "Auditoria")]
public sealed class TrilhaDeAuditoriaTestes : IDisposable
{
    private const int Filial = 1;
    private const long Pessoa = 100;

    private readonly SqliteConnection _conexao = new("Filename=:memory:");

    public TrilhaDeAuditoriaTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));

        using var db = Contexto(ContextoDeSistema(usuarioId: Pessoa, empresaId: Filial));
        db.Database.EnsureCreated();

        var empresa = Empresa.Criar("010101", "Filial de teste");
        db.Entry(empresa).Property(e => e.Id).CurrentValue = Filial;
        db.Empresas.Add(empresa);

        var usuario = Usuario.Criar(Guid.NewGuid(), "pessoa@exemplo.com", "Pessoa", "Pessoa",
            Email.Criar("pessoa@exemplo.com"), Filial, 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = Pessoa;
        db.Usuarios.Add(usuario);

        db.Sistemas.Add(Dominio.Integracao.Sistema.Criar("IBGE", "IBGE — teste", "teste"));
        db.Municipios.Add(Municipio.Criar("SAO JOSE DO RIO PRET", "SP"));
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    private sealed class Provedor(ContextoAcesso atual) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = atual;
    }

    private static ContextoAcesso ContextoDeSistema(long usuarioId, int empresaId) =>
        new(usuarioId, "sistema de teste", empresaId, new HashSet<int> { Filial }, new HashSet<long>(),
            new HashSet<long>(), new Dictionary<string, Profundidade>(), ehServicoDeSistema: true);

    private static ContextoAcesso PessoaNaTela() =>
        new(Pessoa, "pessoa", Filial, new HashSet<int> { Filial }, new HashSet<long>(),
            new HashSet<long>(), new Dictionary<string, Profundidade>());

    private CrmDbContext Contexto(ContextoAcesso acesso) =>
        new(new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options, new Provedor(acesso));

    [Fact]
    public async Task A_origem_declarada_por_quem_grava_vai_para_a_trilha_com_o_sistema()
    {
        int ibge;
        await using (var db = Contexto(ContextoDeSistema(Pessoa, Filial)))
        {
            ibge = await db.Sistemas.Where(s => s.Codigo == "IBGE").Select(s => s.Id).SingleAsync();
            db.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, ibge);

            var municipio = await db.Municipios.SingleAsync();
            municipio.ReconhecerNoIbge(3549805, "São José do Rio Preto").Should().BeTrue();
            await db.SaveChangesAsync();
        }

        await using var leitura = Contexto(ContextoDeSistema(Pessoa, Filial));
        var trilha = await leitura.AlteracoesDeCampo.OrderBy(l => l.Campo).ToListAsync();

        trilha.Select(l => l.Campo).Should().Equal("CodigoIbge", "Nome");
        trilha.Should().AllSatisfy(l =>
        {
            l.Origem.Should().Be(OrigemDaOperacao.Integracao);
            l.SistemaId.Should().Be(ibge);
            l.Operacao.Should().Be(OperacaoAuditada.Alteracao);
            l.AlteradoPorId.Should().Be(Pessoa);
            l.EmpresaId.Should().Be(Filial, "município não tem filial: vale a filial de casa de quem grava");
        });
        trilha[0].ValorAnterior.Should().BeNull();
        trilha[0].ValorNovo.Should().Be("3549805");
        trilha[1].ValorAnterior.Should().Be("SAO JOSE DO RIO PRET");
        trilha[1].ValorNovo.Should().Be("São José do Rio Preto");
    }

    [Fact]
    public async Task O_registro_que_nasce_de_integracao_nao_entra_na_trilha_e_o_de_pessoa_entra()
    {
        await using (var integracao = Contexto(ContextoDeSistema(Pessoa, Filial)))
        {
            integracao.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, null);
            integracao.Clientes.Add(Cliente.Criar(Filial, "Nascido da integração", TipoDePessoa.Juridica, Pessoa, Pessoa));
            await integracao.SaveChangesAsync();
        }

        await using (var pessoa = Contexto(PessoaNaTela()))
        {
            pessoa.Clientes.Add(Cliente.Criar(Filial, "Nascido na tela", TipoDePessoa.Juridica, Pessoa, Pessoa));
            await pessoa.SaveChangesAsync();
        }

        await using var leitura = Contexto(ContextoDeSistema(Pessoa, Filial));
        var nomes = await leitura.Clientes.ToDictionaryAsync(c => c.NomeRazao, c => c.Id);
        var trilha = await leitura.AlteracoesDeCampo.Where(l => l.Entidade == "Cliente").ToListAsync();

        trilha.Should().NotContain(l => l.RegistroId == nomes["Nascido da integração"],
            "o rastro do que nasce de integração é o registro de origem, não a trilha");
        trilha.Should().Contain(l => l.RegistroId == nomes["Nascido na tela"] && l.Operacao == OperacaoAuditada.Inclusao
                                     && l.Origem == OrigemDaOperacao.Usuario);
    }

    [Fact]
    public async Task Sem_como_gravar_a_trilha_o_dado_tambem_nao_e_gravado()
    {
        // Contexto de sistema sem usuário nem filial, alterando uma entidade que não tem filial nem
        // autor próprio: a trilha não tem autor nem filial para gravar. A regra é que isso derruba a
        // gravação inteira — nunca o dado sem rastro.
        await using (var semIdentidade = Contexto(ContextoDeSistema(usuarioId: 0, empresaId: 0)))
        {
            var municipio = await semIdentidade.Municipios.SingleAsync();
            municipio.ReconhecerNoIbge(3549805, "São José do Rio Preto");

            var gravar = () => semIdentidade.SaveChangesAsync();
            await gravar.Should().ThrowAsync<InvalidOperationException>().WithMessage("*trilha*");
        }

        await using var leitura = Contexto(ContextoDeSistema(Pessoa, Filial));
        var depois = await leitura.Municipios.SingleAsync();
        depois.CodigoIbge.Should().BeNull("a transação foi desfeita: o reconhecimento não ficou gravado");
        depois.Nome.Should().Be("SAO JOSE DO RIO PRET");
        (await leitura.AlteracoesDeCampo.CountAsync()).Should().Be(0);
    }
}
