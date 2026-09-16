using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A CONFERÊNCIA DAS GRAFIAS CORTADAS com o banco de verdade (documento 32, seções 4.6 e 4.6.1): só o
/// inequívoco é corrigido, a trilha registra, o pendente fica com o motivo e a segunda rodada não muda
/// nada.
///
/// <para>O catálogo tem a linha oficial de São José do Rio Preto, com código, e a grafia cortada em
/// 20 caracteres, sem código. Cinco endereços apontam para a grafia cortada: sem coordenada; com
/// coordenada dentro do contorno; com coordenada em outro município; em outra UF; e um excluído.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class ConsolidacaoDeGrafiasCortadasTestes : IDisposable
{
    private const int Empresa = 1;
    private const long Usuario = 100;
    private const int CodigoDoOficial = 3549805;
    private const int CodigoDoVizinho = 3547403;

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly Provedor _provedor = new();
    private readonly int _grafiaId;
    private readonly int _oficialId;
    private readonly Dictionary<string, long> _enderecos = [];

    public ConsolidacaoDeGrafiasCortadasTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));

        using var db = NovoContexto();
        db.Database.EnsureCreated();
        GerarIdDaTrilhaNoSqlite();
        (_oficialId, _grafiaId) = Semear(db);
    }

    /// <summary>
    /// A TRILHA TEM CHAVE COMPOSTA (Id, AlteradoEm), porque no SQL Server ela é particionada por mês e o
    /// Id vem de IDENTITY. O SQLite só gera valor para chave primária de uma coluna, então aqui o Id ganha
    /// um valor padrão — só neste banco de teste, sem tocar no modelo.
    /// </summary>
    private void GerarIdDaTrilhaNoSqlite()
    {
        string Escalar(string sql)
        {
            using var comando = _conexao.CreateCommand();
            comando.CommandText = sql;
            return (string)comando.ExecuteScalar()!;
        }

        void Executar(string sql)
        {
            using var comando = _conexao.CreateCommand();
            comando.CommandText = sql;
            comando.ExecuteNonQuery();
        }

        const string Coluna = "\"Id\" INTEGER NOT NULL";
        var tabela = Escalar("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'AlteracaoDeCampo'");
        if (!tabela.Contains(Coluna)) throw new InvalidOperationException("A coluna Id da trilha mudou de forma no SQLite.");

        var indices = new List<string>();
        using (var comando = _conexao.CreateCommand())
        {
            comando.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'index' AND tbl_name = 'AlteracaoDeCampo' AND sql IS NOT NULL";
            using var leitor = comando.ExecuteReader();
            while (leitor.Read()) indices.Add(leitor.GetString(0));
        }

        Executar("DROP TABLE \"AlteracaoDeCampo\"");
        Executar(tabela.Replace(Coluna, Coluna + " DEFAULT (abs(random()))"));
        foreach (var indice in indices) Executar(indice);
    }

    public void Dispose() => _conexao.Dispose();

    [Fact]
    public async Task So_o_inequivoco_e_corrigido_com_trilha_e_a_segunda_rodada_nao_muda_nada()
    {
        var leituras = 0;
        var consolidacao = Consolidacao((_, _) => { leituras++; return Task.FromResult(Malha()); });

        var primeira = await consolidacao.ExecutarAsync(CancellationToken.None);

        primeira.EnderecosLidos.Should().Be(4, "o endereço excluído não entra na conferência");
        primeira.Reapontados.Should().Be(2, "sem coordenada e com coordenada dentro do contorno");
        primeira.Pendentes.Should().Be(2, "coordenada em outro município e UF diferente");

        await using (var db = NovoContexto())
        {
            (await MunicipioDe(db, "sem coordenada")).Should().Be(_oficialId);
            (await MunicipioDe(db, "dentro do contorno")).Should().Be(_oficialId);
            (await MunicipioDe(db, "em outro município")).Should().Be(_grafiaId, "nada é associado à força");
            (await MunicipioDe(db, "outra UF")).Should().Be(_grafiaId);
            (await MunicipioDe(db, "excluído")).Should().Be(_grafiaId, "registro inativado não é corrigido");

            (await db.AlteracoesDeCampo.CountAsync()).Should().Be(2, "cada correção deixa o antes e o depois");

            // A TRILHA É A AUTOMÁTICA (documento 41, fase 2): a carga não a escreve mais à mão — declara
            // a origem, e o SaveChanges grava o antes e o depois do campo auditado, com o sistema certo.
            var ibge = await db.Sistemas.Where(s => s.Codigo == "IBGE").Select(s => s.Id).SingleAsync();
            var trilha = await db.AlteracoesDeCampo.ToListAsync();
            trilha.Should().AllSatisfy(linha =>
            {
                linha.Entidade.Should().Be(nameof(Endereco));
                linha.Campo.Should().Be(nameof(Endereco.MunicipioId));
                linha.ValorAnterior.Should().Be(_grafiaId.ToString());
                linha.ValorNovo.Should().Be(_oficialId.ToString());
                linha.Origem.Should().Be(Dominio.Auditoria.OrigemDaOperacao.Integracao);
                linha.SistemaId.Should().Be(ibge, "a conferência é uma integração do IBGE");
                linha.Operacao.Should().Be(Dominio.Auditoria.OperacaoAuditada.Alteracao);
                linha.AlteradoPorId.Should().Be(Usuario);
                linha.CorrelacaoId.Should().NotBeNull();
            });

            var motivos = await db.MensagensDescartadas.Where(m => m.Fluxo == ConsolidacaoDeGrafiasCortadas.Fluxo).Select(m => m.Erro).ToListAsync();
            motivos.Should().HaveCount(2);
            motivos.Should().Contain(m => m.Contains(CodigoDoVizinho.ToString()), "o motivo diz onde a coordenada cai");
        }

        var segunda = await consolidacao.ExecutarAsync(CancellationToken.None);

        segunda.Reapontados.Should().Be(0);
        segunda.Pendentes.Should().Be(2, "os dois pendentes continuam identificados, com o motivo");

        await using (var db = NovoContexto())
        {
            (await db.AlteracoesDeCampo.CountAsync()).Should().Be(2, "a segunda rodada não grava trilha");
            (await db.MensagensDescartadas.CountAsync(m => m.Fluxo == ConsolidacaoDeGrafiasCortadas.Fluxo))
                .Should().Be(2, "as recusas da rodada substituem as da anterior, sem acumular");
        }

        leituras.Should().Be(2, "uma leitura por rodada, porque há endereço com coordenada para conferir");
    }

    [Fact]
    public async Task Sem_o_IBGE_so_o_endereco_sem_coordenada_e_corrigido()
    {
        var consolidacao = Consolidacao((_, _) => throw new HttpRequestException("IBGE fora do ar"));

        var resultado = await consolidacao.ExecutarAsync(CancellationToken.None);

        resultado.ContornoDisponivel.Should().BeFalse();
        resultado.Reapontados.Should().Be(1, "só o endereço sem coordenada dispensa o contorno");
        resultado.Pendentes.Should().Be(3);

        await using var db = NovoContexto();
        (await MunicipioDe(db, "dentro do contorno")).Should().Be(_grafiaId, "sem o contorno, a coordenada não é conferida");
    }

    [Fact]
    public async Task Sem_endereco_com_coordenada_o_contorno_nao_e_lido()
    {
        await using (var db = NovoContexto())
        {
            foreach (var comCoordenada in new[] { "dentro do contorno", "em outro município" })
            {
                var id = _enderecos[comCoordenada];
                var endereco = await db.Enderecos.SingleAsync(e => e.Id == id);
                db.Entry(endereco).Property(e => e.ExcluidoEm).CurrentValue = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
        }

        var leituras = 0;
        var resultado = await Consolidacao((_, _) => { leituras++; return Task.FromResult(Malha()); })
            .ExecutarAsync(CancellationToken.None);

        leituras.Should().Be(0, "nada a conferir contra o contorno: a recarga não espera a API do IBGE");
        resultado.Reapontados.Should().Be(1);
        resultado.ContornoDisponivel.Should().BeTrue();
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private ConsolidacaoDeGrafiasCortadas Consolidacao(
        Func<int, CancellationToken, Task<IReadOnlyDictionary<int, PoligonoMunicipal>>> lerMalha) =>
        new(NovoContexto, lerMalha, Usuario);

    private async Task<int?> MunicipioDe(CrmDbContext db, string caso)
    {
        var id = _enderecos[caso];
        return await db.Enderecos.Where(e => e.Id == id).Select(e => e.MunicipioId).SingleAsync();
    }

    private static IReadOnlyDictionary<int, PoligonoMunicipal> Malha() => new Dictionary<int, PoligonoMunicipal>
    {
        [CodigoDoOficial] = Quadrado(-49.5, -20.9, -49.3, -20.7),
        [CodigoDoVizinho] = Quadrado(-50.5, -20.1, -50.3, -19.9)
    };

    private static PoligonoMunicipal Quadrado(double lonMin, double latMin, double lonMax, double latMax) =>
        PoligonoMunicipal.DeAneis(new List<IReadOnlyList<(double Lon, double Lat)[]>>
        {
            new List<(double Lon, double Lat)[]>
            {
                new[] { (lonMin, latMin), (lonMax, latMin), (lonMax, latMax), (lonMin, latMax), (lonMin, latMin) }
            }
        });

    private CrmDbContext NovoContexto() =>
        new(new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options, _provedor, new Diario());

    private (int Oficial, int Grafia) Semear(CrmDbContext db)
    {
        var empresa = Dominio.Organizacao.Empresa.Criar("010101", "Filial de teste");
        db.Entry(empresa).Property(e => e.Id).CurrentValue = Empresa;
        db.Empresas.Add(empresa);

        var usuario = Dominio.Seguranca.Usuario.Criar(Guid.NewGuid(), "teste@exemplo.com", "Teste", "Teste", Email.Criar("teste@exemplo.com"), Empresa, 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = Usuario;
        db.Usuarios.Add(usuario);

        var oficial = Municipio.Criar("São José do Rio Preto", "SP", CodigoDoOficial);
        var vizinho = Municipio.Criar("Santa Rita d'Oeste", "SP", CodigoDoVizinho);
        var grafia = Municipio.Criar("SAO JOSE DO RIO PRET", "SP");
        db.Municipios.AddRange(oficial, vizinho, grafia);
        db.SaveChanges();

        var casos = new (string Caso, string Uf, decimal? Latitude, decimal? Longitude, bool Excluido)[]
        {
            ("sem coordenada", "SP", null, null, false),
            ("dentro do contorno", "SP", -20.8m, -49.4m, false),
            ("em outro município", "SP", -20.0m, -50.4m, false),
            ("outra UF", "MG", null, null, false),
            ("excluído", "SP", null, null, true)
        };

        foreach (var (caso, uf, latitude, longitude, excluido) in casos)
        {
            var cliente = Cliente.Criar(Empresa, $"Cliente {caso}", TipoDePessoa.Juridica, Usuario, Usuario);
            db.Clientes.Add(cliente);
            db.SaveChanges();

            var endereco = Endereco.Criar(Empresa, cliente.Id, TipoDeEndereco.Fiscal, "Rua do teste",
                MunicipioDoEndereco.Selecionado(grafia.Id), uf, Usuario, ehPrincipal: true,
                latitude: latitude, longitude: longitude);
            db.Enderecos.Add(endereco);
            db.SaveChanges();

            if (excluido)
            {
                db.Entry(endereco).Property(e => e.ExcluidoEm).CurrentValue = DateTime.UtcNow;
                db.SaveChanges();
            }

            _enderecos[caso] = endereco.Id;
        }

        return (oficial.Id, grafia.Id);
    }

    private sealed class Provedor : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = new(Usuario,
            nomeExibicao: "carga de teste",
            empresaId: Empresa,
            empresasVisiveis: new HashSet<int> { Empresa },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: true);
    }

    private sealed class Diario : IDiarioDeAlcanceEntreEmpresas
    {
        public void Abriu(ContextoAcesso acesso, string motivo)
        {
        }

        public void Fechou(ContextoAcesso acesso, string motivo, TimeSpan duracao)
        {
        }
    }
}
