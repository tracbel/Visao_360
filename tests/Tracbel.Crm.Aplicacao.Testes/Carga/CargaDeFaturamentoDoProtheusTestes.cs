using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A CARGA DO FATURAMENTO DO PROTHEUS (24/09/2026): a filial pelo código da nota, a janela
/// substituída a cada execução, a curva ABC e a promoção de quem comprou — e a simulação que não grava.
///
/// <para>SQLite em memória com o modelo de verdade e o contexto da carga, como ela roda no servidor. A
/// leitura da SD2 fica de fora: o lote entra pronto, e a consulta é testada contra um SQL Server de
/// verdade em <c>LeitorDeFaturamentoNoContainerTestes</c>.</para>
/// </summary>
public sealed class CargaDeFaturamentoDoProtheusTestes : IDisposable
{
    private const long Carga = 100;
    private const int Ribeirao = 1;
    private const int RioPreto = 2;

    private const string DoCliente = "11222333000181";
    private const string DeOutroCliente = "11444777000161";
    private const string SemCadastro = "33000167000101";

    private static readonly DateOnly Desde = new(2026, 1, 1);
    private static readonly DateOnly Julho = new(2026, 7, 1);
    private static readonly DateOnly Agosto = new(2026, 8, 1);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;

    public CargaDeFaturamentoDoProtheusTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureCreated();

        foreach (var (id, codigo, nome) in new[] { (Ribeirao, "010101", "Ribeirão Preto"), (RioPreto, "010113", "São José do Rio Preto") })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        var usuario = Usuario.Criar(Guid.NewGuid(), "carga@tracbel.com.br", "Carga", "carga", Email.Criar("carga@tracbel.com.br"), Ribeirao, criadoPorId: 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = Carga;
        db.Usuarios.Add(usuario);
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private CrmDbContext DaCarga() =>
        new(_opcoes, new ContextoDeCargaDeSistema(Carga, Ribeirao, new HashSet<int> { Ribeirao, RioPreto }));

    private CrmDbContext Leitura() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CargaDeFaturamentoDoProtheus NovaCarga() =>
        new(DaCarga, new LeitorDeFaturamentoDoProtheus(new OpcoesDoBancoDoProtheus()), Carga, _ => { });

    private static FaturamentoParaCarga Linha(string documento, string filial, DateOnly mes, decimal valor, decimal maquina = 0m) =>
        new($"{documento}/{filial}/{mes:yyyy-MM}", documento, 0, filial, mes, valor, 1, 1, $"NOME DE {documento}",
            new QuebraDoFaturamento(maquina, valor - maquina, 0m, 0m));

    private static LoteDoProtheus Lote(params FaturamentoParaCarga[] linhas) =>
        new(Desde, linhas, [.. linhas.Select(l => l.CodigoDaFilial!).Distinct()], linhas.Length, 0, 0, 0m, 0, 0, 0m, new DateOnly(2026, 8, 31));

    private long NovoCliente(string documento, SituacaoDoCliente situacao, int empresaId = Ribeirao)
    {
        CpfCnpj.TentarCriar(documento, out var cpfCnpj).Should().BeTrue();
        using var db = Leitura();
        var cliente = Cliente.Criar(empresaId, $"Cliente {documento}", TipoDePessoa.Juridica, Carga, Carga, documento: cpfCnpj, situacao: situacao);
        db.Clientes.Add(cliente);
        db.SaveChanges();
        return cliente.Id;
    }

    // =============================================================================================
    // A filial e a gravação
    // =============================================================================================

    [Fact]
    public async Task Cada_mes_vai_para_a_filial_da_nota_pelo_codigo_e_a_filial_sem_empresa_e_descartada_com_o_valor()
    {
        var cliente = NovoCliente(DoCliente, SituacaoDoCliente.Suspect);

        var resumo = await NovaCarga().GravarAsync(Lote(
            Linha(DoCliente, "010101", Agosto, 100m),
            Linha(DoCliente, "010113", Agosto, 50m),
            Linha(DoCliente, "020101", Agosto, 7m),
            Linha(SemCadastro, "010101", Agosto, 30m)), CancellationToken.None);

        await using var db = Leitura();
        (await db.FaturamentoDosClientes.ToListAsync()).Select(f => (f.ClienteId, f.EmpresaId, f.ValorLiquido))
            .Should().BeEquivalentTo([(cliente, Ribeirao, 100m), (cliente, RioPreto, 50m)],
                "o código de seis dígitos da nota é o de organizacao.Empresa — sem de-para, e sem cair tudo em 010101");

        var semCliente = await db.FaturamentoSemClientes.SingleAsync();
        (semCliente.Documento, semCliente.EmpresaId, semCliente.ValorLiquido, semCliente.Natureza)
            .Should().Be((SemCadastro, Ribeirao, 30m, NaturezaDoParceiro.ClienteNaoCadastrado));

        resumo.MesesNovos.Should().Be(2);
        resumo.Decisoes.Keys.Should().Contain(d => d.Contains("020101") && d.Contains("descartados"),
            "a venda da filial que o CRM não conhece sai do número, e o relatório diz quanto");
    }

    [Fact]
    public async Task Rodar_de_novo_com_o_mesmo_lote_reapura_e_nao_duplica()
    {
        NovoCliente(DoCliente, SituacaoDoCliente.Suspect);
        var lote = Lote(Linha(DoCliente, "010101", Julho, 100m), Linha(DoCliente, "010101", Agosto, 40m), Linha(SemCadastro, "010113", Agosto, 30m));

        await NovaCarga().GravarAsync(lote, CancellationToken.None);
        var segunda = await NovaCarga().GravarAsync(lote, CancellationToken.None);

        segunda.MesesNovos.Should().Be(0);
        segunda.MesesReapurados.Should().Be(2);
        segunda.MesesRemovidos.Should().Be(0);

        await using var db = Leitura();
        (await db.FaturamentoDosClientes.CountAsync()).Should().Be(2);
        (await db.FaturamentoSemClientes.CountAsync()).Should().Be(1);
        (await db.FaturamentoDosClientes.ToListAsync()).Sum(f => f.ValorLiquido).Should().Be(140m);
    }

    [Fact]
    public async Task Quando_o_cliente_passa_a_existir_a_venda_sai_de_sem_cliente_e_nao_conta_duas_vezes()
    {
        // O FATURAMENTO RODOU ANTES DA CARGA DE CLIENTES: todo o dinheiro foi para FaturamentoSemCliente.
        var lote = Lote(Linha(DoCliente, "010101", Agosto, 100m));
        await NovaCarga().GravarAsync(lote, CancellationToken.None);

        await using (var db = Leitura())
            (await db.FaturamentoSemClientes.SingleAsync()).ValorLiquido.Should().Be(100m);

        // No dia seguinte o cliente existe.
        var cliente = NovoCliente(DoCliente, SituacaoDoCliente.Suspect);
        var resumo = await NovaCarga().GravarAsync(lote, CancellationToken.None);

        await using var leitura = Leitura();
        (await leitura.FaturamentoDosClientes.SingleAsync()).ClienteId.Should().Be(cliente);
        (await leitura.FaturamentoSemClientes.CountAsync()).Should().Be(0,
            "a linha antiga contaria o mesmo real duas vezes no cartão da diretoria");
        resumo.MesesRemovidos.Should().Be(1);
    }

    [Fact]
    public async Task O_mes_que_a_origem_nao_tem_mais_sai_da_janela_e_o_que_e_anterior_a_ela_fica()
    {
        var cliente = NovoCliente(DoCliente, SituacaoDoCliente.Cliente);

        await using (var db = DaCarga())
        {
            // Fora da janela: a leitura não olhou para lá, então não é ela quem decide.
            db.FaturamentoDosClientes.Add(FaturamentoDoCliente.Criar(Ribeirao, cliente, new DateOnly(2025, 6, 1), 999m, 1, 1, new QuebraDoFaturamento(999m, 0m, 0m, 0m)));
            await db.SaveChangesAsync();
        }

        await NovaCarga().GravarAsync(Lote(Linha(DoCliente, "010101", Julho, 100m), Linha(DoCliente, "010101", Agosto, 40m)), CancellationToken.None);

        // A nota de julho foi cancelada no ERP.
        var resumo = await NovaCarga().GravarAsync(Lote(Linha(DoCliente, "010101", Agosto, 40m)), CancellationToken.None);

        await using var leitura = Leitura();
        (await leitura.FaturamentoDosClientes.Select(f => f.Competencia).ToListAsync())
            .Should().BeEquivalentTo([new DateOnly(2025, 6, 1), Agosto]);
        resumo.MesesRemovidos.Should().Be(1);
        resumo.Decisoes.Keys.Should().Contain(d => d.Contains("removidos da janela"));
    }

    [Fact]
    public async Task A_leitura_vazia_nao_apaga_nada()
    {
        NovoCliente(DoCliente, SituacaoDoCliente.Cliente);
        await NovaCarga().GravarAsync(Lote(Linha(DoCliente, "010101", Agosto, 40m)), CancellationToken.None);

        var resumo = await NovaCarga().GravarAsync(Lote(), CancellationToken.None);

        await using var db = Leitura();
        (await db.FaturamentoDosClientes.CountAsync()).Should().Be(1,
            "um ERP que emite nota todo dia não fica três anos sem venda: leitura vazia é defeito, não ausência");
        resumo.MesesRemovidos.Should().Be(0);
        resumo.Decisoes.Keys.Should().Contain(d => d.Contains("sem nenhuma venda"));
    }

    // =============================================================================================
    // A curva e a situação
    // =============================================================================================

    [Fact]
    public async Task A_venda_promove_Suspect_e_Prospect_a_Cliente_e_nao_desfaz_decisao_de_pessoa()
    {
        var suspect = NovoCliente(DoCliente, SituacaoDoCliente.Suspect);
        var prospect = NovoCliente(DeOutroCliente, SituacaoDoCliente.Prospect);
        var encerrado = NovoCliente("11222333000262", SituacaoDoCliente.Encerrado);
        var semVenda = NovoCliente("11444777000242", SituacaoDoCliente.Suspect);

        var resumo = await NovaCarga().GravarAsync(Lote(
            Linha(DoCliente, "010101", Agosto, 700m),
            Linha(DeOutroCliente, "010101", Agosto, 200m),
            Linha("11222333000262", "010101", Agosto, 100m)), CancellationToken.None);

        await using var db = Leitura();
        var situacoes = await db.Clientes.ToDictionaryAsync(c => c.Id, c => (c.Situacao, c.Classe));

        situacoes[suspect].Should().Be((SituacaoDoCliente.Cliente, ClasseDeCliente.A));
        situacoes[prospect].Should().Be((SituacaoDoCliente.Cliente, ClasseDeCliente.B));
        situacoes[encerrado].Should().Be((SituacaoDoCliente.Encerrado, ClasseDeCliente.C), "encerrar é decisão de gente");
        situacoes[semVenda].Should().Be((SituacaoDoCliente.Suspect, ClasseDeCliente.D), "sem nota, nada a promover");

        resumo.ClientesPromovidos.Should().Be(2);
        resumo.Curva.Should().BeEquivalentTo(new Dictionary<ClasseDeCliente, int>
        {
            [ClasseDeCliente.A] = 1, [ClasseDeCliente.B] = 1, [ClasseDeCliente.C] = 1, [ClasseDeCliente.D] = 1
        });

        // A PROMOÇÃO FICA NA TRILHA, com a origem certa: o Protheus, e não o Vórtice.
        var protheus = await db.Sistemas.SingleAsync(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema);
        var trilha = await db.AlteracoesDeCampo.AsNoTracking()
            .Where(a => a.Entidade == nameof(Cliente) && a.Campo == "Situacao" && a.Operacao == OperacaoAuditada.Alteracao)
            .ToListAsync();
        trilha.Select(t => t.RegistroId).Should().BeEquivalentTo([suspect, prospect]);
        trilha.Should().OnlyContain(t => t.Origem == OrigemDaOperacao.Integracao && t.SistemaId == protheus.Id && t.AlteradoPorId == Carga);
    }

    [Fact]
    public void A_curva_e_por_filial_e_o_cliente_fica_na_praca_onde_mais_comprou()
    {
        var curva = CargaDeFaturamentoDoProtheus.ClassificarCurva(
        [
            (1, Ribeirao, 70m), (2, Ribeirao, 20m), (3, Ribeirao, 10m),
            // O cliente 4 compra nas duas praças, mais em Rio Preto: é lá que ele é classificado.
            (4, Ribeirao, 5m), (4, RioPreto, 30m), (4, RioPreto, 30m), (5, RioPreto, 40m)
        ]);

        curva[1].Should().Be((ClasseDeCliente.A, 70m));
        curva[2].Should().Be((ClasseDeCliente.B, 20m));
        curva[3].Should().Be((ClasseDeCliente.C, 10m));
        curva[4].Should().Be((ClasseDeCliente.A, 60m), "60% de Rio Preto; os 5 de Ribeirão não entram na curva de lá");
        curva[5].Should().Be((ClasseDeCliente.C, 40m));

        // O CORTE É PELO ACUMULADO JÁ SOMANDO O CLIENTE — como a carga sempre fez. O cliente sozinho
        // numa filial fecha 100% e cai em C; o comentário antigo prometia "o primeiro é sempre A".
        CargaDeFaturamentoDoProtheus.ClassificarCurva([(9, RioPreto, 1m)])[9].Classe.Should().Be(ClasseDeCliente.C);
    }

    [Theory]
    [InlineData(SituacaoDoCliente.Suspect, true)]
    [InlineData(SituacaoDoCliente.Prospect, true)]
    [InlineData(SituacaoDoCliente.Cliente, false)]
    [InlineData(SituacaoDoCliente.ClienteInativo, false)]
    [InlineData(SituacaoDoCliente.Encerrado, false)]
    public void So_Suspect_e_Prospect_sao_promovidos_pela_venda(SituacaoDoCliente situacao, bool promove) =>
        CargaDeFaturamentoDoProtheus.PromoveAoFaturar(situacao).Should().Be(promove);

    [Fact]
    public void A_janela_comeca_no_dia_1_de_36_meses_atras()
    {
        CargaDeFaturamentoDoProtheus.InicioDaJanela(new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc))
            .Should().Be(new DateOnly(2023, 9, 1), "o primeiro mês chega inteiro — começar no dia 24 o gravaria pela metade");
    }

    // =============================================================================================
    // A simulação
    // =============================================================================================

    [Fact]
    public async Task A_simulacao_le_e_nao_grava_nada()
    {
        var cliente = NovoCliente(DoCliente, SituacaoDoCliente.Suspect);
        await using (var db = DaCarga())
        {
            db.FaturamentoDosClientes.Add(FaturamentoDoCliente.Criar(Ribeirao, cliente, Julho, 55m, 1, 1, new QuebraDoFaturamento(0m, 55m, 0m, 0m)));
            await db.SaveChangesAsync();
        }

        (int, int, int, int, int, SituacaoDoCliente) Fotografia()
        {
            using var db = Leitura();
            return (db.FaturamentoDosClientes.Count(), db.FaturamentoSemClientes.Count(), db.Sistemas.Count(),
                db.AlteracoesDeCampo.Count(), db.Clientes.Count(c => c.Classe != null), db.Clientes.Single().Situacao);
        }

        var antes = Fotografia();

        var simulacao = await NovaCarga().SimularAsync(
            Lote(Linha(DoCliente, "010101", Agosto, 100m, maquina: 60m), Linha(DeOutroCliente, "010113", Agosto, 30m), Linha(SemCadastro, "020101", Agosto, 5m)),
            new HashSet<string> { DeOutroCliente }, CancellationToken.None);

        Fotografia().Should().Be(antes, "a simulação roda contra a produção, e daqui só se lê");

        simulacao.Incluiria.Should().Be(2, "o mês de agosto do cliente e o do documento sem cadastro");
        simulacao.Reapuraria.Should().Be(0);
        simulacao.Removeria.Should().Be(1, "julho está no banco e não está na origem");

        simulacao.Hoje.LinhasComCliente.Should().Be(1);
        simulacao.Hoje.LinhasSemCliente.Should().Be(1);
        simulacao.Hoje.LinhasSemFilial.Should().Be(1);
        simulacao.Hoje.ClientesPromovidos.Should().Be(1);
        var agosto = simulacao.Hoje.PorMes.Single();
        (agosto.Chave, agosto.Total, agosto.Maquina, agosto.ComCliente, agosto.SemCliente, agosto.SemFilial)
            .Should().Be(("2026-08", 135m, 60m, 100m, 30m, 5m));
        simulacao.Hoje.PorFilial.Select(f => f.Chave).Should().Contain(["010101 Ribeirão Preto", "010113 São José do Rio Preto", "020101 (sem empresa no CRM)"]);

        simulacao.ComSa1.Should().NotBeNull();
        simulacao.ComSa1!.LinhasComCliente.Should().Be(2, "o documento que a carga da SA1 criaria passa a casar");
        simulacao.ComSa1.ClientesPromovidos.Should().Be(2, "o cliente criado pela SA1 nasce Suspect e a venda o promove");
        simulacao.ComSa1.PorMes.Single().ComCliente.Should().Be(130m);

        var impresso = new List<string>();
        CargaDeFaturamentoDoProtheus.Imprimir(simulacao, impresso.Add);
        impresso.Should().Contain(l => l.Contains("=== CRM de hoje ===")).And.Contain(l => l.Contains("=== CRM + carga da SA1 ==="));
    }
}
