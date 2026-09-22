using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// MUNICÍPIO POR CÓDIGO OFICIAL EM TODAS AS FONTES (issue 154): o par entre a chave da fonte e o município do
/// catálogo é feito uma vez e gravado. Da segunda rodada em diante nenhum nome é comparado — é o que estes
/// testes medem, pelos mesmos contadores que saem no relatório da carga.
///
/// <para>SQLite em memória com o modelo de verdade e o contexto da carga, como a carga roda no servidor.</para>
/// </summary>
public sealed class DeParaDeMunicipioNasFontesTestes : IDisposable
{
    private const long Carga = 100;
    private const long Pessoa = 101;
    private const string Sicor = "BCB.SICOR_INVESTIMENTO";
    private static readonly DateTime Agora = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly int _sistemaId;
    private readonly int _rioPretoEmSp;
    private readonly int _francaEmSp;

    public DeParaDeMunicipioNasFontesTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureCreated();

        var empresa = Empresa.Criar("010101", "Ribeirão Preto");
        db.Entry(empresa).Property(e => e.Id).CurrentValue = 1;
        db.Empresas.Add(empresa);

        foreach (var (id, login) in new[] { (Carga, "carga"), (Pessoa, "gestor") })
        {
            var usuario = Usuario.Criar(
                Guid.NewGuid(), $"{login}@tracbel.com.br", login, login, Email.Criar($"{login}@tracbel.com.br"), 1, criadoPorId: 1);
            db.Entry(usuario).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(usuario);
        }

        var rioPreto = Municipio.Criar("São José do Rio Preto", "SP", 3549805);
        var franca = Municipio.Criar("Franca", "SP", 3516200);

        // O HOMÔNIMO DE OUTRA UF: existe no catálogo, e o de-para de uma carga de SP não pode enxergá-lo.
        var francaEmMg = Municipio.Criar("Franca", "MG", 3126000);

        var sistema = Sistema.Criar("BCB", "Banco Central — SICOR", "Arquivo público, somente leitura");
        db.Municipios.AddRange(rioPreto, franca, francaEmMg);
        db.Sistemas.Add(sistema);
        db.SaveChanges();

        _sistemaId = sistema.Id;
        _rioPretoEmSp = rioPreto.Id;
        _francaEmSp = franca.Id;
    }

    public void Dispose() => _conexao.Dispose();

    /// <summary>O contexto como a carga o abre: usuário e filial reais, origem declarada.</summary>
    private CrmDbContext DaCarga(OrigemDaOperacao origem = OrigemDaOperacao.Integracao, long usuarioId = Carga)
    {
        var db = new CrmDbContext(_opcoes, new ContextoDeCargaDeSistema(usuarioId, 1, new HashSet<int> { 1 }));
        db.DeclararOrigemDasGravacoes(origem, origem == OrigemDaOperacao.Integracao ? _sistemaId : null);
        return db;
    }

    /// <summary>Uma rodada da carga: abre o de-para, resolve as linhas e grava o que casou.</summary>
    private async Task<(CorrespondenciaDeMunicipios DePara, List<int?> Resolvidos)> RodarAsync(
        params (string Chave, string Nome)[] linhas)
    {
        await using var db = DaCarga();
        var dePara = await CorrespondenciaDeMunicipios.AbrirAsync(db, Sicor, "SP", Carga, CancellationToken.None);

        var resolvidos = linhas
            .Select(l => dePara.Resolver(l.Chave, l.Nome, Agora, out var id) ? id : (int?)null)
            .ToList();

        dePara.Gravar(db);
        await db.SaveChangesAsync();
        return (dePara, resolvidos);
    }

    [Fact]
    public async Task A_primeira_rodada_casa_pelo_nome_e_grava_o_par_e_a_segunda_nao_casa_nome_nenhum()
    {
        var (primeira, _) = await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"), ("11402", "FRANCA"));

        primeira.CasadosPorNome.Should().Be(2);
        primeira.CasadosPelaCorrespondencia.Should().Be(0);

        var (segunda, resolvidos) = await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"), ("11402", "FRANCA"));

        segunda.CasadosPorNome.Should().Be(0, "é o critério de aceite da 154: a segunda rodada não casa nome nenhum");
        segunda.CasadosPelaCorrespondencia.Should().Be(2);
        resolvidos.Should().Equal(_rioPretoEmSp, _francaEmSp);

        await using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        var pares = await db.CorrespondenciasDeMunicipios.AsNoTracking().ToListAsync();
        pares.Should().HaveCount(2, "o par é gravado uma vez, não uma por rodada");
        pares.Should().OnlyContain(p => p.Forma == FormaDaCorrespondencia.NomeUnicoNaUf && p.Fonte == Sicor);
        pares.Single(p => p.ChaveNaFonte == "11790").MunicipioId.Should().Be(_rioPretoEmSp);
    }

    [Fact]
    public async Task A_grafia_nova_na_fonte_nao_tira_o_municipio_da_conta()
    {
        await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"));

        // O Banco Central republica o mesmo código com outra grafia — a que não casaria com nome nenhum.
        var (segunda, resolvidos) = await RodarAsync(("11790", "S. J. DO RIO PRETO"));

        resolvidos.Should().Equal(new int?[] { _rioPretoEmSp }, "o par responde pelo código, não pelo nome");
        segunda.SemPar.Should().Be(0);
        segunda.CasadosPelaCorrespondencia.Should().Be(1);
    }

    [Fact]
    public async Task O_codigo_novo_do_banco_central_estabelece_um_par_novo_sem_mexer_nos_outros()
    {
        await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"));

        var (segunda, resolvidos) = await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"), ("11402", "FRANCA"));

        segunda.CasadosPelaCorrespondencia.Should().Be(1);
        segunda.CasadosPorNome.Should().Be(1, "só o código que a fonte estreou");
        resolvidos.Should().Equal(_rioPretoEmSp, _francaEmSp);
    }

    [Fact]
    public async Task O_municipio_de_mesmo_nome_em_outra_UF_nao_entra_na_carga_de_SP()
    {
        var (rodada, resolvidos) = await RodarAsync(("11402", "FRANCA"));

        resolvidos.Should().Equal(new int?[] { _francaEmSp }, "a carga é de SP; Franca de Minas não é candidata");
        rodada.MunicipiosNoCatalogo.Should().Be(2, "só os de SP entram no denominador do casamento por nome");
    }

    [Fact]
    public async Task O_nome_ambiguo_dentro_da_UF_vira_recusa_e_nao_adivinhacao()
    {
        // AMBIGUIDADE SIMULADA, na forma em que ela aparece de verdade: duas linhas do catálogo que o índice
        // único aceita — o hífen as separa — e que normalizam para a mesma chave. Adivinhar aqui poria o
        // crédito de um município na conta do outro.
        await using (var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia))
        {
            db.Municipios.AddRange(Municipio.Criar("Vila Nova", "SP", 3500101), Municipio.Criar("Vila-Nova", "SP", 3500202));
            await db.SaveChangesAsync();
        }

        var (rodada, resolvidos) = await RodarAsync(("19999", "VILA NOVA"));

        resolvidos.Should().Equal([null]);
        rodada.SemPar.Should().Be(1);
        rodada.CasadosPorNome.Should().Be(0);
        CorrespondenciaDeMunicipios.MotivoSemPar("VILA NOVA").Should().Contain("à mão");

        await using var conferencia = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        (await conferencia.CorrespondenciasDeMunicipios.CountAsync()).Should().Be(0, "sem par, nada é gravado");
    }

    [Fact]
    public async Task Cada_fonte_tem_o_seu_de_para_e_uma_nao_le_a_chave_da_outra()
    {
        await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"));

        await using var db = DaCarga();
        var daAnp = await CorrespondenciaDeMunicipios.AbrirAsync(db, "ANP.USINA_DE_ETANOL", "SP", Carga, CancellationToken.None);

        daAnp.Resolver("11790", "SAO JOSE DO RIO PRETO", Agora, out _).Should().BeTrue();
        daAnp.CasadosPelaCorrespondencia.Should().Be(0, "o código 11790 é do Banco Central; na ANP ele não quer dizer nada");
        daAnp.CasadosPorNome.Should().Be(1, "a ANP casa pelo nome, que é a chave que ela tem");
    }

    [Fact]
    public async Task A_correcao_do_par_pela_mao_de_uma_pessoa_fica_na_trilha()
    {
        await RodarAsync(("11790", "SAO JOSE DO RIO PRETO"));

        // QUEM CORRIGE É UMA PESSOA, no contexto dela — é assim que a correção chega pela tela.
        await using (var db = DaCarga(OrigemDaOperacao.Usuario, Pessoa))
        {
            var par = await db.CorrespondenciasDeMunicipios.SingleAsync(p => p.ChaveNaFonte == "11790");
            par.Reapontar(_francaEmSp, "SAO JOSE DO RIO PRETO", FormaDaCorrespondencia.Manual, Pessoa, Agora.AddDays(1))
                .Should().BeTrue();
            await db.SaveChangesAsync();
        }

        await using var conferencia = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        var trilha = await conferencia.AlteracoesDeCampo.AsNoTracking()
            .Where(a => a.Entidade == nameof(CorrespondenciaDeMunicipio)).ToListAsync();

        trilha.Select(t => t.Campo).Should().BeEquivalentTo(["MunicipioId", "Forma"]);
        trilha.Single(t => t.Campo == "MunicipioId").ValorAnterior.Should().Be(_rioPretoEmSp.ToString());
        trilha.Single(t => t.Campo == "Forma").ValorNovo.Should().Be(nameof(FormaDaCorrespondencia.Manual));
        trilha.Should().OnlyContain(t => t.AlteradoPorId == Pessoa);
    }
}
