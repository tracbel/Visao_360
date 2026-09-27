using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Testes.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// SÓ A PRINCIPAL CONTA — nas telas que já existiam (decisão de 27/09/2026, documento 52 §4). A mesma perda registrada
/// três vezes — a principal (FY25), a duplicata (o <c>SEM_PARTICIPACAO</c> gêmeo) e o complemento (<c>VP_TRATOR</c>) —
/// conta UMA vez em cada leitor de <c>processo.VendaPerdida</c>: o resumo por motivo e por concorrente, o painel do CEN,
/// os indicadores executivos e a cobertura do motor. E o preço da duplicata não entra na média.
/// </summary>
public sealed class SoAPrincipalDaVendaPerdidaContaTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;

    public SoAPrincipalDaVendaPerdidaContaTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Banco();
        db.Database.EnsureCreated();
        var semente = CenarioDoFunilDoVortice.Semear(db);

        db.ClienteCarteiras.Add(ClienteCarteira.Criar(semente.ClienteId, semente.CarteiraId, ClasseDeCliente.C, semente.Operador, vinculadoEmUtc: Agora.AddYears(-1)));
        var motivo = MotivoDePerda.Criar("PRECO", "Preço", CategoriaDeMotivoDePerda.Preco);
        db.MotivosDePerda.Add(motivo);
        var concorrente = CatalogoItem.Criar(CatalogosDeSistema.Concorrente, "CASE", "Case");
        db.CatalogoItens.Add(concorrente);
        db.SaveChanges();

        ConteudoDaVendaPerdida Perda(string formulario, decimal concorrenteCobrou, decimal nosOferecemos, int maquinas) => new(
            semente.RibeiraoPreto, Agora.AddMonths(-2), DateOnly.FromDateTime(Agora.AddMonths(-2)), motivo.Id, semente.ClienteId, null,
            concorrente.Id, null, "MAGNUM 340", "8R 340", maquinas, concorrenteCobrou, nosOferecemos, ParticipacaoNaNegociacao.Nao, formulario, 1001);

        var principal = VendaPerdida.DaOrigem(Perda(FormulariosDaVendaPerdida.Fy25, 1_500_000m, 1_400_000m, 2), semente.Operador);
        db.VendasPerdidas.Add(principal);
        db.SaveChanges();

        db.VendasPerdidas.AddRange(
            VendaPerdida.DaOrigem(Perda(FormulariosDaVendaPerdida.SemParticipacao, 900_000m, 1_000_000m, 2), semente.Operador,
                PapelDaVendaPerdida.Duplicata, principal.Id),
            VendaPerdida.DaOrigem(Perda(FormulariosDaVendaPerdida.VpTrator, 1_000_000m, 1_200_000m, 1), semente.Operador,
                PapelDaVendaPerdida.Complemento, principal.Id));
        db.SaveChanges();

        db.VendasPerdidas.Count().Should().Be(3, "as três respostas estão no banco — só a leitura é que conta uma");
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Banco() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    [Fact]
    public async Task O_resumo_das_vendas_perdidas_conta_a_principal_e_a_media_usa_so_o_preco_dela()
    {
        await using var db = Banco();
        var repositorio = new RepositorioDeVendasPerdidas(db);

        (await repositorio.ContarAsync(CancellationToken.None)).Should().Be(1);

        var porMotivo = (await repositorio.ResumirPorMotivoAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        porMotivo.Quantidade.Should().Be(1);
        porMotivo.Maquinas.Should().Be(2, "as duas máquinas da principal — a duplicata e o complemento não somam as delas");
        porMotivo.DiferencaMediaDePreco.Should().Be(-100_000m, "a média é só da principal; com a duplicata ela iria para 0");

        var porConcorrente = (await repositorio.ResumirPorConcorrenteAsync(CancellationToken.None)).Should().ContainSingle().Subject;
        porConcorrente.Quantidade.Should().Be(1);
        porConcorrente.DiferencaMediaDePreco.Should().Be(-100_000m);
    }

    [Fact]
    public async Task O_painel_do_CEN_conta_uma_venda_perdida()
    {
        await using var db = Banco();
        var painel = await new RepositorioDoPainelDoCen(db).ObterPainelAsync(null, Agora, CancellationToken.None);

        painel!.VendasPerdidasRegistradas.Should().Be(1);
    }

    [Fact]
    public async Task Os_indicadores_executivos_contam_uma_venda_perdida()
    {
        await using var db = Banco();
        var indicadores = await new RepositorioDeIndicadoresExecutivos(db)
            .ApurarAsync(2026, CalendarioDoAno.Fiscal, AnoFiscal.Inteiro(2026), Agora, CancellationToken.None);

        indicadores.Mercado.VendasPerdidasRegistradas.Should().Be(1);
        indicadores.Mercado.ComOsDoisPrecos.Should().Be(1);
    }

    [Fact]
    public async Task A_cobertura_do_motor_mede_sobre_a_principal()
    {
        await using var db = Banco();
        var cobertura = await new RepositorioDeCoberturaDoMotor(db).MedirAsync(CancellationToken.None);

        var perdas = cobertura.Grupos.Single(g => g.Codigo == "VENDAS_PERDIDAS");
        perdas.Itens.Should().OnlyContain(i => i.Total == 1, "a mesma perda, registrada em três formulários, é uma perda");
    }
}
