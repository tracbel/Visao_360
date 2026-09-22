using System.Text;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Seguranca;
using Tracbel.Crm.Integracao.Conexoes;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Integracoes;

/// <summary>
/// AS INTEGRAÇÕES CONFIGURÁVEIS POR DENTRO (issue 136): a senha protegida que só abre aqui, a credencial da tela por
/// cima da do ambiente com os nomes que a carga já lê, e o orquestrador que roda o que venceu — com o banco de
/// verdade (SQLite) e as cargas simuladas.
/// </summary>
public sealed class IntegracoesConfiguraveisTestes : IDisposable
{
    private const string Senha = "s3nha;com=ponto-e-virgula";
    private readonly SqliteConnection _conexao = new("Filename=:memory:");

    public IntegracoesConfiguraveisTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        using var db = Contexto();
        db.Database.EnsureCreated();

        var empresa = Empresa.Criar("010101", "Filial de teste");
        db.Entry(empresa).Property(e => e.Id).CurrentValue = 1;
        db.Empresas.Add(empresa);
        var usuario = Usuario.Criar(Guid.NewGuid(), "admin@exemplo.invalid", "Admin", "Admin", Email.Criar("admin@exemplo.invalid"), 1, 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = 100;
        db.Usuarios.Add(usuario);
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    private sealed class Provedor : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = new(100, "sistema de teste", 1, new HashSet<int> { 1 }, new HashSet<long>(),
            new HashSet<long>(), new Dictionary<string, Profundidade>(), ehServicoDeSistema: true);
    }

    private CrmDbContext Contexto() => new(new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options, new Provedor());

    private static IConfiguration Ambiente(params (string Chave, string Valor)[] valores) =>
        new ConfigurationBuilder().AddInMemoryCollection(valores.ToDictionary(v => v.Chave, v => (string?)v.Valor)).Build();

    // ---------------------------------------------------------------------------------------- protetor

    [Fact]
    public void A_senha_protegida_nao_carrega_o_texto_e_so_abre_inteira()
    {
        var protetor = new ProtetorDeSegredos();
        var protegida = protetor.Proteger(Senha);

        Encoding.UTF8.GetString(protegida).Should().NotContain("s3nha");
        protetor.Revelar(protegida).Should().Be(Senha);

        var adulterada = protegida.ToArray();
        adulterada[^1] ^= 0xFF;
        FluentActions.Invoking(() => protetor.Revelar(adulterada))
            .Should().Throw<InvalidOperationException>().Which.Message.Should().NotContain("s3nha").And.Contain("Digite a senha de novo");
    }

    // ---------------------------------------------------------------------------------------- resolvedor

    [Fact]
    public void A_credencial_da_tela_vence_a_do_ambiente_e_vai_para_os_nomes_que_a_carga_ja_le()
    {
        var protetor = new ProtetorDeSegredos();
        var art = Conexao.DoCatalogo(ConexoesDoSistema.Todas.Single(c => c.Codigo == ConexoesDoSistema.Art));
        art.Configurar("art.exemplo.invalid", 3307, "vendas", "vw_vendas", "leitura", null);
        art.DefinirSegredo(protetor.Proteger(Senha), 100, DateTime.UtcNow);

        var resolvedor = new ResolvedorDeConexoes(Ambiente(("Art:Senha", "a-do-ambiente")), protetor);
        var resolvida = resolvedor.Resolver(art);

        resolvida.Origem.Should().Be(OrigemDaCredencial.Tela);
        resolvida.ToString().Should().NotContain(Senha).And.NotContain("leitura", "o registro que cai num log não leva credencial");

        var chaves = ResolvedorDeConexoes.ParaConfiguracao(resolvida);
        chaves["Art:Senha"].Should().Be(Senha);
        chaves["Art:Visao"].Should().Be("vw_vendas");
        chaves["Art:Porta"].Should().Be("3307");
    }

    [Fact]
    public void Sem_credencial_na_tela_vale_o_ambiente_e_sem_as_duas_nao_ha_o_que_testar()
    {
        var protheus = Conexao.DoCatalogo(ConexoesDoSistema.Todas.Single(c => c.Codigo == ConexoesDoSistema.Protheus));

        new ResolvedorDeConexoes(Ambiente(), new ProtetorDeSegredos()).OrigemDe(protheus).Should().Be(OrigemDaCredencial.Nenhuma);

        var doAmbiente = new ResolvedorDeConexoes(
            Ambiente(("Protheus:Base", "http://erp.exemplo.invalid/rest"), ("Protheus:Usuario", "u"), ("Protheus:Senha", "p")), new ProtetorDeSegredos());
        doAmbiente.OrigemDe(protheus).Should().Be(OrigemDaCredencial.Ambiente);
        ResolvedorDeConexoes.ParaConfiguracao(doAmbiente.Resolver(protheus)).Should().BeEmpty(
            "o ambiente já está na configuração: só a credencial da tela precisa ser sobreposta");
    }

    [Fact]
    public void A_cadeia_do_vortice_montada_pela_tela_escapa_a_senha_e_declara_somente_leitura()
    {
        var protetor = new ProtetorDeSegredos();
        var vortice = Conexao.DoCatalogo(ConexoesDoSistema.Todas.Single(c => c.Codigo == ConexoesDoSistema.Vortice));
        vortice.Configurar(@"10.0.0.9\LEGADO", null, "Vortice", null, "leitura", null);
        vortice.DefinirSegredo(protetor.Proteger(Senha), 100, DateTime.UtcNow);

        var cadeia = ResolvedorDeConexoes.ParaConfiguracao(new ResolvedorDeConexoes(Ambiente(), protetor).Resolver(vortice))["Vortice:Conexao"]!;
        var lida = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cadeia);

        lida.Password.Should().Be(Senha, "o ponto e vírgula da senha não virou um parâmetro a mais");
        lida.ApplicationIntent.Should().Be(Microsoft.Data.SqlClient.ApplicationIntent.ReadOnly);
        lida.InitialCatalog.Should().Be("Vortice");
    }

    [Fact]
    public async Task A_carga_le_do_banco_so_as_credenciais_completas_da_tela()
    {
        var protetor = new ProtetorDeSegredos();
        await using (var db = Contexto())
        {
            var protheus = await db.Conexoes.SingleAsync(c => c.Codigo == ConexoesDoSistema.Protheus);
            protheus.Configurar("http://erp.exemplo.invalid/rest", null, null, null, "leitura", null);
            protheus.DefinirSegredo(protetor.Proteger(Senha), 100, DateTime.UtcNow);

            var art = await db.Conexoes.SingleAsync(c => c.Codigo == ConexoesDoSistema.Art);
            art.Configurar("art.exemplo.invalid", null, "vendas", "vw", "leitura", null); // sem senha: incompleta
            await db.SaveChangesAsync();
        }

        await using var leitura = Contexto();
        var (valores, avisos) = await CredenciaisDaTela.LerAsync(leitura, new ResolvedorDeConexoes(Ambiente(), protetor), CancellationToken.None);

        avisos.Should().BeEmpty();
        valores.Keys.Should().BeEquivalentTo(["Protheus:Base", "Protheus:Usuario", "Protheus:Senha"]);
        CredenciaisDaTela.Sobrepor(Ambiente(("Protheus:Senha", "a-antiga")), valores)["Protheus:Senha"].Should().Be(Senha);
    }

    // ---------------------------------------------------------------------------------------- orquestrador

    private sealed class CargasSimuladas(Func<string, int> codigo, string? saida = null) : IExecutorDeCargas
    {
        public List<string> Rodadas { get; } = [];

        public Task<(int Codigo, IReadOnlyList<string> UltimasLinhas)> RodarAsync(string modo, string arquivoDeLog, CancellationToken ct)
        {
            Rodadas.Add(modo);
            return Task.FromResult<(int, IReadOnlyList<string>)>((codigo(modo), [saida ?? $"{modo} terminou"]));
        }
    }

    private sealed class TestadorSimulado(bool ok) : ITestadorDeConexoes
    {
        public List<string> Testadas { get; } = [];

        public Task<ResultadoDoTesteDeConexao> TestarAsync(ConexaoResolvida conexao, CancellationToken ct)
        {
            Testadas.Add(conexao.Codigo);
            return Task.FromResult(new ResultadoDoTesteDeConexao(ok, ok ? "HTTP 200" : "HTTP 503", 12));
        }
    }

    private Task<int> VoltaAsync(IExecutorDeCargas cargas, ITestadorDeConexoes testador, DateTime agora, IReadOnlyList<string?>? segredos = null) =>
        Orquestrador.ExecutarVoltaAsync(
            Contexto, cargas, new ResolvedorDeConexoes(Ambiente(), new ProtetorDeSegredos()), testador, Path.GetTempPath(),
            segredos ?? [], () => agora, _ => { }, CancellationToken.None);

    [Fact]
    public async Task A_primeira_volta_roda_as_fontes_com_tabela_vazia_e_nao_as_desligadas()
    {
        var cargas = new CargasSimuladas(_ => 0);

        (await VoltaAsync(cargas, new TestadorSimulado(true), new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc))).Should().Be(0);

        cargas.Rodadas.Should().Equal("--somente-pam", "--somente-estrutura", "--somente-precos", "--somente-custos", "--somente-credito");
        await using var db = Contexto();
        var execucoes = await db.ExecucoesDeRotina.ToListAsync();
        execucoes.Should().HaveCount(2).And.OnlyContain(e => e.Motivo == MotivoDaExecucao.PrimeiraCarga && e.Resultado == ResultadoDaExecucao.Sucesso);
        (await db.Rotinas.SingleAsync(r => r.Codigo == RotinasDoSistema.Faturamento)).UltimaExecucaoIniciadaEm.Should().BeNull("nasce desligada");

        // A SEGUNDA VOLTA NÃO REPETE: já rodaram, e a próxima é pela agenda.
        cargas.Rodadas.Clear();
        await VoltaAsync(cargas, new TestadorSimulado(true), new DateTime(2026, 9, 23, 12, 5, 0, DateTimeKind.Utc));
        cargas.Rodadas.Should().BeEmpty();
    }

    [Fact]
    public async Task Rodar_agora_roda_mesmo_desligada_registra_quem_pediu_e_mascara_a_saida()
    {
        await using (var db = Contexto())
        {
            foreach (var cada in await db.Rotinas.ToListAsync()) cada.IniciarExecucao(new DateTime(2026, 9, 22, 13, 0, 0, DateTimeKind.Utc));
            var faturamento = await db.Rotinas.SingleAsync(r => r.Codigo == RotinasDoSistema.Faturamento);
            faturamento.PedirExecucao(100, new DateTime(2026, 9, 23, 11, 0, 0, DateTimeKind.Utc));
            await db.SaveChangesAsync();
        }

        var cargas = new CargasSimuladas(_ => 3, saida: $"falhou ao autenticar com a senha {Senha}");
        (await VoltaAsync(cargas, new TestadorSimulado(true), new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc), [Senha])).Should().Be(3);

        cargas.Rodadas.Should().Equal("--somente-faturamento");
        await using var leitura = Contexto();
        var execucao = await leitura.ExecucoesDeRotina.SingleAsync();
        execucao.Motivo.Should().Be(MotivoDaExecucao.Pedido);
        execucao.PedidaPorId.Should().Be(100);
        execucao.Resultado.Should().Be(ResultadoDaExecucao.Falha);
        execucao.CodigoDeSaida.Should().Be(3);
        execucao.Mensagem.Should().Contain("--somente-faturamento: código 3").And.NotContain(Senha);

        var rotina = await leitura.Rotinas.SingleAsync(r => r.Codigo == RotinasDoSistema.Faturamento);
        rotina.ExecucaoPedidaEm.Should().BeNull("o pedido saiu da fila");
        rotina.UltimoResultado.Should().Be(ResultadoDaExecucao.Falha);
    }

    [Fact]
    public async Task A_api_monitorada_e_testada_quando_o_intervalo_vence()
    {
        await using (var db = Contexto())
        {
            foreach (var cada in await db.Rotinas.ToListAsync()) cada.IniciarExecucao(new DateTime(2026, 9, 22, 13, 0, 0, DateTimeKind.Utc));
            db.Conexoes.Add(Conexao.CriarMonitorada("CLIMA", "Clima", null, "https://api.exemplo.invalid/saude", 200, 30));
            await db.SaveChangesAsync();
        }

        var testador = new TestadorSimulado(false);
        await VoltaAsync(new CargasSimuladas(_ => 0), testador, new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc));
        await VoltaAsync(new CargasSimuladas(_ => 0), testador, new DateTime(2026, 9, 23, 12, 10, 0, DateTimeKind.Utc));

        testador.Testadas.Should().Equal(["CLIMA"], "a segunda volta ainda está dentro dos 30 minutos");
        await using var leitura = Contexto();
        var clima = await leitura.Conexoes.SingleAsync(c => c.Codigo == "CLIMA");
        clima.UltimaVerificacao.Should().Be(SituacaoDaVerificacao.ComFalha);
        (await leitura.VerificacoesDeConexao.SingleAsync()).VerificadaPorId.Should().BeNull("foi o orquestrador, não uma pessoa");
    }
}
