using System.Reflection;
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

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// Prova que o filtro global de segurança realmente FILTRA — contra um banco de verdade.
///
/// Usa SQLite em memória de propósito, e não um provedor falso: assim a consulta é
/// traduzida para SQL real. Filtro que não traduz é filtro que não existe, e esse é o tipo
/// de erro que só aparece em produção.
///
/// [V] É este ponto único de aplicação que falta no Vórtice: lá a segurança roda no cliente
/// Gupta, e por isso **125 dos 134 relatórios não têm nenhum predicado de usuário**.
///
/// <para>ESTE ARQUIVO MUDOU DE SUJEITO NA FASE 1 (documento 41). Até ali ele provava a
/// PROFUNDIDADE (próprios / equipe / empresa / organização) sobre <c>Lead</c>, a única entidade
/// com filtro próprio. A tabela de lead nunca teve uma linha e saiu na fase 1, e com ela saiu o
/// único filtro por profundidade do modelo — o mecanismo continua em <c>CrmDbContext</c>,
/// dormente, esperando quem precisar. O que este arquivo prova agora é o que continua valendo
/// para as 50 tabelas que ficaram: que a fronteira de empresa chega ao SQL de TODA entidade que
/// tem <c>EmpresaId</c>, e que os tipos de valor atravessam o banco sem se desfazer.</para>
///
/// <para>A profundidade em si continua coberta: a decisão está em
/// <c>Dominio.Testes/Seguranca/AutorizadorTestes</c> e a via de escape entre empresas em
/// <see cref="FronteiraDeEmpresaTestes"/>.</para>
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class FiltroSegurancaTestes : IDisposable
{
    private const long Eu = 100;
    private const int MinhaEmpresa = 1;
    private const int EmpresaFilha = 2;
    private const int OutraEmpresa = 9;

    private readonly SqliteConnection _conexao;

    public FiltroSegurancaTestes()
    {
        // "Filename=:memory:" com a conexão mantida aberta: o banco vive enquanto o teste roda.
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
    }

    public void Dispose() => _conexao.Dispose();

    /// <summary>Provedor de contexto controlado pelo teste.</summary>
    private sealed class ProvedorFalso(ContextoAcesso contexto) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = contexto;
    }

    private static ContextoAcesso Acesso(bool ehSistema = false) =>
        new(usuarioId: Eu,
            nomeExibicao: "Ricardo",
            empresaId: MinhaEmpresa,
            empresasVisiveis: new HashSet<int> { MinhaEmpresa, EmpresaFilha },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: ehSistema);

    private CrmDbContext Contexto(ContextoAcesso acesso)
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            // Sem convenção de nome: o nome no banco é o PascalCase do C#, igual em SQLite e
            // em SQL Server. É o que faz o teste exercitar o mesmo esquema que existe em
            // produção (documento 20, seção 3).
            .UseSqlite(_conexao)
            .Options;

        return new CrmDbContext(opcoes, new ProvedorFalso(acesso));
    }

    // ------------------------------------------------------------------ testes

    [Fact]
    public void O_filtro_e_traduzido_para_SQL_em_toda_entidade_que_tem_empresa()
    {
        // A GARANTIA QUE IMPORTA, E A QUE A FASE 1 PODERIA TER QUEBRADO EM SILÊNCIO: a fronteira
        // é aplicada por VARREDURA do modelo, então mexer no modelo mexe em quem recebe filtro.
        // Aqui a consulta de cada entidade é traduzida de verdade — se o EF avaliasse o predicado
        // em memória, ele traria TODAS as linhas do banco para descartar depois: vazamento no meio
        // do caminho e desastre de desempenho.
        //
        // O teste estático irmão (Arquitetura.Testes/Banco/MultiempresaTestes) confere a EXPRESSÃO
        // do filtro; este confere que ela chega ao SQL.
        using var db = Contexto(Acesso());

        var conjuntoGenerico = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(m => m.Name == nameof(DbContext.Set)
                        && m.IsGenericMethodDefinition
                        && m.GetParameters().Length == 0);

        var comEmpresa = db.Model.GetEntityTypes()
            .Where(t => !t.IsOwned()
                        && t.FindPrimaryKey() is not null
                        && t.FindProperty("EmpresaId") is not null
                        && t.GetQueryFilter() is not null)
            .ToList();

        comEmpresa.Should().NotBeEmpty("o modelo não pode ficar sem nenhuma entidade multiempresa");

        foreach (var tipo in comEmpresa)
        {
            var consulta = (IQueryable)conjuntoGenerico.MakeGenericMethod(tipo.ClrType).Invoke(db, null)!;
            var sql = consulta.ToQueryString();

            sql.Should().Contain("WHERE", "o filtro de {0} precisa virar cláusula SQL", tipo.ClrType.Name);
            sql.Should().Contain("EmpresaId",
                "o predicado de empresa precisa estar no SQL de {0}", tipo.ClrType.Name);
        }
    }

    [Fact]
    public async Task Tipos_de_valor_sobrevivem_a_ida_e_volta_do_banco()
    {
        // [V] GE_Pessoa.FoneNro1 é decimal(12) no Vórtice e estoura com DDI. Aqui os tipos de
        // valor vão e voltam normalizados, porque quem normaliza é o TIPO, não a tela.
        await using (var db = Contexto(Acesso(ehSistema: true)))
        {
            db.Database.EnsureCreated();

            var empresa = Empresa.Criar("010101", "Tracbel Agro — Ribeirão Preto");
            db.Entry(empresa).Property(e => e.Id).CurrentValue = MinhaEmpresa;
            db.Empresas.Add(empresa);

            var sistema = Usuario.Criar(
                identidadeExterna: Guid.NewGuid(),
                nomePrincipal: "sistema@tracbel.com.br",
                nomeCompleto: "sistema",
                nomeExibicao: "sistema",
                email: Email.Criar("sistema@tracbel.com.br"),
                empresaId: MinhaEmpresa,
                criadoPorId: 1);
            db.Entry(sistema).Property(u => u.Id).CurrentValue = 1L;
            db.Usuarios.Add(sistema);

            var eu = Usuario.Criar(
                identidadeExterna: Guid.NewGuid(),
                nomePrincipal: "ricardo@tracbel.com.br",
                nomeCompleto: "ricardo",
                nomeExibicao: "ricardo",
                // Escrito como o usuário digitaria: com maiúsculas.
                email: Email.Criar("Ricardo@Tracbel.COM.BR"),
                empresaId: MinhaEmpresa,
                criadoPorId: 1);
            db.Entry(eu).Property(u => u.Id).CurrentValue = Eu;
            db.Usuarios.Add(eu);

            await db.SaveChangesAsync();

            // Documento fictício de teste, com dígitos verificadores válidos, digitado com máscara.
            db.Clientes.Add(Cliente.Criar(
                MinhaEmpresa, "Produtor de teste", TipoDePessoa.Fisica, Eu, 1,
                documento: CpfCnpj.Criar("529.982.247-25")));

            await db.SaveChangesAsync();
        }

        await using var leitura = Contexto(Acesso());

        var cliente = await leitura.Clientes.FirstAsync(c => c.NomeRazao == "Produtor de teste");
        cliente.Documento!.Value.Numero.Should().Be("52998224725", "a máscara não é guardada");

        var usuario = await leitura.Usuarios.FirstAsync(u => u.Id == Eu);
        usuario.Email.Endereco.Should().Be("ricardo@tracbel.com.br", "normalizado em minúsculas");
    }

    [Fact]
    public async Task O_servico_de_sistema_enxerga_o_que_o_usuario_da_filial_nao_enxerga()
    {
        // Jobs e integrações precisam ver o todo — mas rodam sob identidade explícita
        // e ficam registrados em aud.EventoAcesso.
        await using (var db = Contexto(Acesso(ehSistema: true)))
        {
            db.Database.EnsureCreated();

            foreach (var (id, codigo, nome) in new[]
                     {
                         (MinhaEmpresa, "010101", "Tracbel Agro — Ribeirão Preto"),
                         (OutraEmpresa, "010109", "Tracbel Agro — Bebedouro")
                     })
            {
                var empresa = Empresa.Criar(codigo, nome);
                db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
                db.Empresas.Add(empresa);
            }

            var sistema = Usuario.Criar(
                identidadeExterna: Guid.NewGuid(),
                nomePrincipal: "sistema@tracbel.com.br",
                nomeCompleto: "sistema",
                nomeExibicao: "sistema",
                email: Email.Criar("sistema@tracbel.com.br"),
                empresaId: MinhaEmpresa,
                criadoPorId: 1);
            db.Entry(sistema).Property(u => u.Id).CurrentValue = 1L;
            db.Usuarios.Add(sistema);
            await db.SaveChangesAsync();

            db.Clientes.AddRange(
                Cliente.Criar(MinhaEmpresa, "Fazenda da minha filial", TipoDePessoa.Juridica, 1, 1),
                Cliente.Criar(OutraEmpresa, "Fazenda de Bebedouro", TipoDePessoa.Juridica, 1, 1));

            await db.SaveChangesAsync();
        }

        await using (var comum = Contexto(Acesso()))
            (await comum.Clientes.CountAsync()).Should().Be(1, "a outra filial está fora do alcance");

        await using var servico = Contexto(Acesso(ehSistema: true));
        (await servico.Clientes.CountAsync()).Should().Be(2);
    }
}
