using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Crm;
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
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class FiltroSegurancaTestes : IDisposable
{
    private const long Eu = 100;
    private const long MeuSubordinado = 300;
    private const long Estranho = 400;
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

    private static ContextoAcesso Acesso(Profundidade leadLer, bool ehSistema = false) =>
        new(usuarioId: Eu,
            nomeExibicao: "Ricardo",
            empresaId: MinhaEmpresa,
            empresasVisiveis: new HashSet<int> { MinhaEmpresa, EmpresaFilha },
            subordinadosIds: new HashSet<long> { MeuSubordinado },
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade> { ["Lead.Ler"] = leadLer },
            ehServicoDeSistema: ehSistema);

    private CrmDbContext Contexto(ContextoAcesso acesso)
    {
        var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(_conexao)
            // Sem convenção de nome: o nome no banco é o PascalCase do C#, igual em SQLite e
            // em SQL Server. É o que faz o teste exercitar o mesmo esquema que existe em
            // produção (documento 20, seção 3).
            .Options;

        return new CrmDbContext(opcoes, new ProvedorFalso(acesso));
    }

    /// <summary>
    /// Semeia quatro leads com donos e empresas diferentes.
    /// A semeadura usa contexto de SISTEMA, que enxerga tudo — senão o próprio seed
    /// seria filtrado, e o teste testaria a si mesmo.
    /// </summary>
    private void Semear()
    {
        using var db = Contexto(Acesso(Profundidade.Nenhum, ehSistema: true));
        db.Database.EnsureCreated();

        // O QUE O LEAD PRECISA EXISTIR ANTES: filial, proprietário e origem de catálogo.
        //
        // Antes do modelo completo, o lead entrava sozinho porque nenhuma dessas chaves
        // estrangeiras existia. Agora existem — e é justamente esse o ponto: [V] a FK que
        // falta é a que gera órfão, e foram 345.535 no Vórtice. O seed do teste passou a ser
        // referencialmente completo porque o banco passou a exigir isso.
        SemearReferencias(db);

        db.Leads.AddRange(
            NovoLead("Meu lead",            Eu,             MinhaEmpresa),
            NovoLead("Do meu subordinado",  MeuSubordinado, MinhaEmpresa),
            NovoLead("De outro na filial",  Estranho,       EmpresaFilha),
            NovoLead("De outra empresa",    Estranho,       OutraEmpresa));

        db.SaveChanges();
    }

    /// <summary>Cria as filiais, os usuários e a origem de catálogo que os leads referenciam.</summary>
    private static void SemearReferencias(CrmDbContext db)
    {
        foreach (var (id, codigo, nome) in new[]
                 {
                     (MinhaEmpresa, "MATRIZ", "Tracbel Matriz"),
                     (EmpresaFilha, "FILIAL", "Tracbel Filial"),
                     (OutraEmpresa, "OUTRA", "Tracbel Outra")
                 })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        foreach (var (id, apelido) in new[]
                 {
                     (1L, "sistema"), (Eu, "ricardo"), (MeuSubordinado, "subordinado"), (Estranho, "estranho")
                 })
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

        // O catálogo ORIGEM_LEAD não é criado aqui: ele é um dos oito catálogos de SISTEMA,
        // semeados pela própria migração porque o esquema depende do identificador deles
        // (ver Dominio.Metadado.CatalogosDeSistema). O teste só acrescenta o item.
        var item = CatalogoItem.Criar(CatalogosDeSistema.OrigemDeLead, "SITE", "Site");
        db.Entry(item).Property(i => i.Id).CurrentValue = 1;
        db.CatalogoItens.Add(item);

        db.SaveChanges();
    }

    private static Lead NovoLead(string nome, long dono, int empresa) =>
        Lead.Criar(
            empresaId: empresa,
            nomeContato: nome,
            origemId: 1,
            proprietarioId: dono,
            criadoPorId: 1,
            email: Email.Criar($"{Guid.NewGuid():N}@teste.com.br"));

    // ------------------------------------------------------------------ testes

    [Theory]
    [InlineData(Profundidade.Nenhum, 0)]
    [InlineData(Profundidade.Proprios, 1)]         // só o meu
    [InlineData(Profundidade.Equipe, 2)]           // o meu + o do subordinado
    [InlineData(Profundidade.Empresa, 2)]          // os dois da minha empresa
    [InlineData(Profundidade.EmpresaEAbaixo, 3)]   // + o da filial
    [InlineData(Profundidade.Organizacao, 4)]      // tudo
    public async Task A_profundidade_limita_o_que_a_consulta_devolve(
        Profundidade profundidade, int esperado)
    {
        Semear();

        await using var db = Contexto(Acesso(profundidade));
        var visiveis = await db.Leads.CountAsync();

        visiveis.Should().Be(esperado,
            "com alcance {0} o usuário deveria enxergar {1} lead(s)", profundidade, esperado);
    }

    [Fact]
    public async Task Sem_permissao_a_consulta_volta_vazia_e_nao_da_erro()
    {
        // Teste de escape: o usuário sem permissão não recebe exceção — recebe NADA.
        // Erro vazaria a informação de que o registro existe.
        Semear();

        await using var db = Contexto(Acesso(Profundidade.Nenhum));

        (await db.Leads.ToListAsync()).Should().BeEmpty();
        (await db.Leads.FirstOrDefaultAsync()).Should().BeNull();
    }

    [Fact]
    public async Task Nao_da_para_escapar_pelo_Where_nem_pelo_Id()
    {
        // O filtro global se compõe com o Where do chamador — não é substituído por ele.
        Semear();

        long idDeOutraEmpresa;
        await using (var sistema = Contexto(Acesso(Profundidade.Nenhum, ehSistema: true)))
            idDeOutraEmpresa = await sistema.Leads
                .Where(l => l.EmpresaId == OutraEmpresa)
                .Select(l => l.Id).FirstAsync();

        await using var db = Contexto(Acesso(Profundidade.Proprios));

        (await db.Leads.FirstOrDefaultAsync(l => l.Id == idDeOutraEmpresa))
            .Should().BeNull("consultar pelo Id direto não fura o filtro");

        (await db.Leads.Where(l => l.EmpresaId == OutraEmpresa).CountAsync())
            .Should().Be(0, "o Where do chamador soma ao filtro, não o substitui");
    }

    [Fact]
    public async Task Registro_excluido_logicamente_some_das_consultas()
    {
        Semear();

        await using (var db = Contexto(Acesso(Profundidade.Organizacao)))
        {
            var lead = await db.Leads.FirstAsync(l => l.ProprietarioId == Eu);
            lead.Excluir(Eu);
            await db.SaveChangesAsync();
        }

        await using var depois = Contexto(Acesso(Profundidade.Organizacao));
        (await depois.Leads.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task Servico_de_sistema_enxerga_tudo()
    {
        // Jobs e integrações precisam ver o todo — mas rodam sob identidade explícita
        // e ficam registrados em aud.EventoAcesso.
        Semear();

        await using var db = Contexto(Acesso(Profundidade.Nenhum, ehSistema: true));
        (await db.Leads.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task O_filtro_e_traduzido_para_SQL_e_nao_avaliado_em_memoria()
    {
        // A garantia que importa: se o EF avaliasse o filtro em memória, ele traria
        // TODAS as linhas do banco para depois descartar — vazamento de dado no meio do
        // caminho e desastre de performance. Este teste falha se isso acontecer.
        Semear();

        await using var db = Contexto(Acesso(Profundidade.Proprios));
        var sql = db.Leads.ToQueryString();

        sql.Should().Contain("WHERE", "o filtro precisa virar cláusula SQL");
        // PascalCase: no banco a coluna e "ProprietarioId", igual ao nome em C#.
        sql.Should().Contain("ProprietarioId", "o predicado de propriedade precisa estar no SQL");
    }

    [Fact]
    public async Task Tipos_de_valor_sobrevivem_a_ida_e_volta_do_banco()
    {
        // [V] GE_Pessoa.FoneNro1 é decimal(12) no Vórtice e estoura com DDI.
        // Aqui o telefone vai e volta como varchar, normalizado.
        await using (var db = Contexto(Acesso(Profundidade.Nenhum, ehSistema: true)))
        {
            db.Database.EnsureCreated();
            SemearReferencias(db);
            db.Leads.Add(Lead.Criar(
                empresaId: MinhaEmpresa, nomeContato: "Teste de tipo", origemId: 1,
                proprietarioId: Eu, criadoPorId: 1,
                email: Email.Criar("Contato@Fazenda.COM.BR"),
                telefone: Telefone.Criar("+55 (17) 99999-0000"),
                documento: CpfCnpj.Criar("529.982.247-25")));
            await db.SaveChangesAsync();
        }

        await using var leitura = Contexto(Acesso(Profundidade.Organizacao));
        var lead = await leitura.Leads.FirstAsync(l => l.NomeContato == "Teste de tipo");

        lead.Telefone!.Value.Numero.Should().Be("17999990000", "o DDI é removido na normalização");
        lead.Email!.Value.Endereco.Should().Be("contato@fazenda.com.br", "normalizado em minúsculas");
        lead.Documento!.Value.Numero.Should().Be("52998224725");
    }
}
