using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (28/09/2026). Um chassi para cada caso, em Ribeirão (010101) e Ituverava (010105),
/// agosto e julho de 2026:
/// <list type="bullet">
/// <item>C1 bate (mesma filial e mês de entrega);</item>
/// <item>C2 a GN conta em Ituverava, o CRM em Ribeirão;</item>
/// <item>C3 a GN conta em agosto, o CRM entregou em julho;</item>
/// <item>C4 a GN conta, e a venda do CRM não tem entrega;</item>
/// <item>C5 a venda está no ART, pendente (comprador ausente);</item>
/// <item>C6 só a GN conta; C7 é da loja Digital; C8 só o CRM conta.</item>
/// </list>
/// Chassis inventados.
/// </summary>
public sealed class CargaDaConferenciaDaGestaoDeNegociosTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 28, 10, 15, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;

    public CargaDaConferenciaDaGestaoDeNegociosTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
        SemearOCrm(db);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private void SemearOCrm(CrmDbContext db)
    {
        var art = Tracbel.Crm.Dominio.Integracao.Sistema.Criar(ConexoesDoSistema.Art, "ART — vendas de máquina", "teste");
        db.Sistemas.Add(art);
        var comprador = Cliente.Criar(_semente.RibeiraoPreto, "Comprador da conferência", TipoDePessoa.Juridica, _semente.Operador, _semente.Operador,
            documento: CpfCnpj.Criar("11444777000161"), situacao: SituacaoDoCliente.Cliente);
        db.Clientes.Add(comprador);
        db.SaveChanges();

        void Venda(string chassi, int empresa, DateOnly? entregue)
        {
            var maquina = Equipamento.RegistrarPelaIntegracao(empresa, Chassi.Criar(chassi), OrigemDoEquipamento.Art, _semente.Operador);
            db.Equipamentos.Add(maquina);
            db.SaveChanges();
            var dados = new DadosDaVendaNaOrigem(empresa, null, new DateOnly(2026, 6, 20), new DateOnly(2026, 6, 25), entregue, null, "P", "NF", "E",
                "Varejo", false, false, 1, "TRATOR MÉDIO", "TR 6155M", "Agro Norte", "Ribeirão Preto", null, $"h-{chassi}", null);
            db.VendasDeMaquina.Add(VendaDeMaquina.Registrar(art.Id, chassi, maquina.Id, comprador.Id, dados, Agora, _semente.Operador));
            db.SaveChanges();
        }

        Venda(C1, _semente.RibeiraoPreto, new DateOnly(2026, 8, 10));
        Venda(C2, _semente.RibeiraoPreto, new DateOnly(2026, 8, 12));
        Venda(C3, _semente.RibeiraoPreto, new DateOnly(2026, 7, 20));
        Venda(C4, _semente.RibeiraoPreto, null);
        Venda(C8, _semente.RibeiraoPreto, new DateOnly(2026, 8, 15));

        var pendente = RegistroDeOrigem.Registrar(art.Id, MetaDeVenda.FluxoDasVendasDoArt, "9005",
            new RetratoDoRegistroDeOrigem("p5", C5, "TRATOR MÉDIO", "TR", "Ribeirão Preto", new DateOnly(2026, 8, 1), null), Agora);
        pendente.Decidir(DecisaoDaIntegracao.Pendente, "COMPRADOR_AUSENTE_NO_CRM", null);
        db.RegistrosDeOrigem.Add(pendente);

        var gn = Tracbel.Crm.Dominio.Integracao.Sistema.Criar(ConexoesDoSistema.GestaoDeNegocios, "Gestão de Negócios — API", "teste");
        db.Sistemas.Add(gn);
        db.SaveChanges();
        var meta = new DadosDaMetaNaOrigem(_semente.RibeiraoPreto, new DateOnly(2026, 8, 1), "TRATOR MÉDIO", "TRATOR_MEDIO", _semente.TratorMedio,
            ConsultorComConta, _semente.Consultor, false, OrigemDaMeta.Campanha, 4, 450_000m, null, "m1");
        db.MetasDeVenda.Add(MetaDeVenda.Registrar(gn.Id, 1, meta, new LeituraDaMeta(Agora, null, null), _semente.Operador));
        db.SaveChanges();
    }

    private const string C1 = "1PY6155MCSS000001", C2 = "1PY6155MCSS000002", C3 = "1PY6155MCSS000003", C4 = "1PY6155MCSS000004",
        C5 = "1PY6155MCSS000005", C6 = "1PY6155MCSS000006", C7 = "1PY6155MCSS000007", C8 = "1PY6155MCSS000008";

    private static LeituraDaConferenciaNaOrigem Gabarito(bool comC6 = true) => new(
        [
            new RealizadoNaGestao(C1, "Ribeirão Preto", "Ago/2026", "1"),
            new RealizadoNaGestao(C2, "Ituverava", "Ago/2026", "1"),
            new RealizadoNaGestao(C3, "Ribeirão Preto", "Ago/2026", "1"),
            new RealizadoNaGestao(C4, "Ribeirão Preto", "Jul/2026", "1"),
            new RealizadoNaGestao(C5, "Ribeirão Preto", "Ago/2026", "1"),
            .. comC6 ? [new RealizadoNaGestao(C6, "Ribeirão Preto", "Ago/2026", "1")] : Array.Empty<RealizadoNaGestao>(),
            new RealizadoNaGestao(C7, "Digital", "Ago/2026", "1")
        ],
        [new MetaNaGestao("Ribeirão Preto", "Ago/2026", "5")],
        [new FilialDaGestao(1, "Ribeirão Preto", "010101"), new FilialDaGestao(5, "Ituverava", "010105"), new FilialDaGestao(0, "Digital", null)],
        new DateTime(2026, 9, 28, 10, 0, 0, DateTimeKind.Utc));

    private async Task<RelatorioDaConferencia> Apurar(LeituraDaConferenciaNaOrigem? gabarito = null, bool simular = false, DateTime? quando = null)
    {
        var resultado = await new CargaDaConferenciaDaGestaoDeNegocios(
                DaCarga, _ => Task.FromResult(Resultado<LeituraDaConferenciaNaOrigem>.Ok(gabarito ?? Gabarito())), _semente.Operador, () => quando ?? Agora, _ => { })
            .ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [Fact]
    public async Task Cada_chassi_que_nao_bate_vira_uma_divergencia_com_o_tipo_certo()
    {
        var relatorio = await Apurar();

        relatorio.Valor(CargaDaConferenciaDaGestaoDeNegocios.RotuloBatem).Should().Be(1);
        relatorio.Valor(CargaDaConferenciaDaGestaoDeNegocios.RotuloSemFilial).Should().Be(1, "a Digital não é filial do CRM");
        relatorio.Valor(CargaDaConferenciaDaGestaoDeNegocios.RotuloNovas).Should().Be(6);

        await using var db = Sistema();
        var divergencias = await db.DivergenciasDeIntegracao.AsNoTracking().ToListAsync();
        divergencias.Select(d => (d.ChaveOrigem, d.Tipo)).Should().BeEquivalentTo(new[]
        {
            (C2, TipoDeDivergencia.RealizadoEmOutraFilial),
            (C3, TipoDeDivergencia.RealizadoEmOutroMes),
            (C4, TipoDeDivergencia.RealizadoNaoEntregueNoCrm),
            (C5, TipoDeDivergencia.RealizadoPendenteNoArt),
            (C6, TipoDeDivergencia.RealizadoSoNaGestao),
            (C8, TipoDeDivergencia.RealizadoSoNoCrm)
        });
        divergencias.Single(d => d.ChaveOrigem == C2).EmpresaId.Should().Be(_semente.Ituverava, "a divergência fica na filial da GN");
        divergencias.Single(d => d.ChaveOrigem == C5).ValorNoCrm.Should().Contain("COMPRADOR_AUSENTE_NO_CRM");
        divergencias.Single(d => d.ChaveOrigem == C3).ValorNoCrm.Should().EndWith("2026-07");
        divergencias.Single(d => d.ChaveOrigem == C3).ValorNaOrigem.Should().EndWith("2026-08");
    }

    [Fact]
    public async Task O_resumo_traz_a_meta_e_o_realizado_la_e_aqui_por_filial_e_mes()
    {
        await Apurar();

        await using var db = Sistema();
        var linhas = await db.ConferenciasDaGestaoDeNegocios.AsNoTracking().ToListAsync();
        (int GN, int Crm) Linha(string indicador, int empresa, int mes)
        {
            var l = linhas.Single(x => x.Indicador == indicador && x.EmpresaId == empresa && x.Competencia == new DateOnly(2026, mes, 1));
            return (l.NaGestao, l.NoCrm);
        }

        Linha(ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, _semente.RibeiraoPreto, 8).Should().Be((4, 3),
            "a GN conta C1, C3, C5 e C6; o CRM entregou C1, C2 e C8 em agosto");
        Linha(ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, _semente.Ituverava, 8).Should().Be((1, 0));
        Linha(ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, _semente.RibeiraoPreto, 7).Should().Be((1, 1));
        Linha(ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, _semente.RibeiraoPreto, 8).Should().Be((5, 4));
    }

    [Fact]
    public async Task A_divergencia_que_some_deixa_de_ocorrer_e_o_resumo_e_regravado()
    {
        await Apurar();

        var segunda = await Apurar(Gabarito(comC6: false), quando: Agora.AddDays(1));

        segunda.Valor(CargaDaConferenciaDaGestaoDeNegocios.RotuloNovas).Should().Be(0);
        segunda.Valor(CargaDaConferenciaDaGestaoDeNegocios.RotuloEncerradas).Should().Be(1);
        await using var db = Sistema();
        var c6 = await db.DivergenciasDeIntegracao.AsNoTracking().SingleAsync(d => d.ChaveOrigem == C6);
        (c6.Situacao, c6.EncerradaEm).Should().Be((SituacaoDaDivergencia.DeixouDeOcorrer, (DateTime?)Agora.AddDays(1)));
        (await db.DivergenciasDeIntegracao.CountAsync(d => d.Situacao == SituacaoDaDivergencia.Aberta)).Should().Be(5);
        (await db.ConferenciasDaGestaoDeNegocios.AsNoTracking()
                .SingleAsync(c => c.Indicador == ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas && c.EmpresaId == _semente.RibeiraoPreto
                                  && c.Competencia == new DateOnly(2026, 8, 1)))
            .NaGestao.Should().Be(3, "a rodada nova regrava o resumo");
    }

    [Fact]
    public async Task A_simulacao_nao_grava_nada_e_o_gabarito_vazio_nao_encerra_nada()
    {
        (await Apurar(simular: true)).Simulada.Should().BeTrue();
        await using (var db = Sistema())
        {
            (await db.DivergenciasDeIntegracao.CountAsync()).Should().Be(0);
            (await db.ConferenciasDaGestaoDeNegocios.CountAsync()).Should().Be(0);
        }

        await Apurar();
        var vazio = await new CargaDaConferenciaDaGestaoDeNegocios(
                DaCarga, _ => Task.FromResult(Resultado<LeituraDaConferenciaNaOrigem>.Ok(new LeituraDaConferenciaNaOrigem([], [], [], null))), _semente.Operador,
                () => Agora.AddDays(1), _ => { })
            .ExecutarAsync(false, CancellationToken.None);
        vazio.EhSucesso.Should().BeFalse();
        await using (var db = Sistema())
            (await db.DivergenciasDeIntegracao.CountAsync(d => d.Situacao == SituacaoDaDivergencia.Aberta)).Should().Be(6);
    }
}
