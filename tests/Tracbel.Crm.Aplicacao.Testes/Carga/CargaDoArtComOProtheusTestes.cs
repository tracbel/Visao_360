using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoParque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O CICLO DO ART COM O PROTHEUS (decisões 1, 2, 4 e 5 de 24/09/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>o número de série curto que o cadastro de veículos tem exatamente entra como a identidade da máquina; o que
/// não confirma continua pendente, e o componente diz por quê;</item>
/// <item>o VIN com I, O e Q (o 1CQ da John Deere) entra;</item>
/// <item>o comprador ausente do CRM dá lugar ao dono atual no Protheus quando ele é cliente — e sai da fila de
/// compradores sem fingir cadastro; quando o comprador verdadeiro passa a existir, ele toma o lugar sem divergência;</item>
/// <item>a divergência entre o dono no Protheus e o comprador do ART só fica registrada quando o ART prevalece, e deixa
/// de ocorrer quando aparece evidência — mas não quando o Protheus simplesmente não foi lido;</item>
/// <item>a projeção decide igual à carga e não grava nada.</item>
/// </list>
/// </summary>
public sealed class CargaDoArtComOProtheusTestes : IDisposable
{
    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDoParque _semente;

    public CargaDoArtComOProtheusTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);

        // O COMPRADOR FORA DO CRM já está na fila, de uma rodada anterior.
        var art = db.Sistemas.Single(s => s.Codigo == LeitorDoArt.CodigoDoSistema).Id;
        db.CompradoresPendentes.Add(CompradorPendente.Registrar(art, CpfCnpj.Criar(CpfForaDoCrm), new ApuracaoDoCompradorPendente(
            _semente.RibeiraoPreto, "COMPRADOR FICTICIO", GrupoDoCompradorPendente.SemNotaNoProtheus, 1, 1, null, null, "010101", 0,
            null, null, null, false, SituacaoNoCadastroDoProtheus.Ausente, false, false, false, false, "definir filial e carteira responsáveis"),
            DateTime.UtcNow, _semente.Operador));
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private static RegistroDoArt Venda(string codigo, string chassi, string documento) => new(
        codigo, chassi, documento, "COMPRADOR FICTICIO", "TRATOR MÉDIO", "TR 6110J", "Agro Norte", "Ribeirão Preto", null,
        "2023-05-05", "2023-05-10", null, null, "P", "P" + codigo, "NF" + codigo, "Varejo", "Não", "Não", "1");

    private static List<RegistroDoArt> Vendas() =>
    [
        Venda("5001", ChassiDoArt, CpfA),
        Venda("5002", "PY6110J012345", CnpjB),
        Venda("5003", ChassiNovo1Cq, CpfA),
        Venda("5004", "1RW6110JCMR000020", CpfForaDoCrm),
        Venda("5005", "1RW6110JCMR000021", CpfA),
        Venda("5006", "1RW6110JCMR000022", CpfA),
        Venda("5007", "1RW6110JCMR000023", CpfA),
        Venda("5008", "7654321", CpfA),
        Venda("5009", "PCGU12345", CpfA)
    ];

    private static List<MaquinaNoProtheus> Protheus(bool evidenciaDaSeis = false) =>
    [
        Maquina("000001", ChassiDoArt, CpfA),
        Maquina("000002", "PY6110J012345", CnpjB),
        Maquina("000020", "1RW6110JCMR000020", CnpjB),
        Maquina("000021", "1RW6110JCMR000021", CpfC, ordem: FaturadaNoArt.AddYears(1), ordemDoDono: true),
        Maquina("000022", "1RW6110JCMR000022", CpfC, ordem: evidenciaDaSeis ? FaturadaNoArt.AddYears(2) : null, ordemDoDono: evidenciaDaSeis),
        Maquina("000023", "1RW6110JCMR000023", CnpjDaTracbel, situacao: "0"),
        Maquina("000024", "PCGU12345", CpfA, grupo: "AP")
    ];

    private CargaDoArt Carga(List<RegistroDoArt> vendas, List<MaquinaNoProtheus>? protheus, Func<CrmDbContext>? abrir = null) => new(
        abrir ?? DaCarga,
        _ => Task.FromResult(Resultado<IReadOnlyList<RegistroDoArt>>.Ok(vendas)),
        (_, _) => Task.FromResult(protheus is null
            ? Resultado<(ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Indisponivel("Protheus fora do ar (teste).")
            : Resultado<(ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>)>.Ok(
                (new ParqueNoProtheus(protheus), new Dictionary<string, CadastroNoProtheus>()))),
        ParceirosPorRaizDeCnpj.RaizesDoGrupo,
        _semente.Operador,
        _ => { });

    private async Task<RelatorioDaCargaDoArt> Rodar(List<RegistroDoArt>? vendas = null, List<MaquinaNoProtheus>? protheus = null, bool semProtheus = false)
    {
        var resultado = await Carga(vendas ?? Vendas(), semProtheus ? null : protheus ?? Protheus()).ExecutarAsync(false, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private async Task<Dictionary<string, RegistroDeOrigem>> Registros()
    {
        await using var db = Sistema();
        return await db.RegistrosDeOrigem.AsNoTracking().Where(r => r.Fluxo == "ART.VENDA_DE_MAQUINA").ToDictionaryAsync(r => r.ChaveOrigem);
    }

    [Fact]
    public async Task O_numero_de_serie_que_o_Protheus_confirma_entra_e_o_que_nao_confirma_fica_pendente()
    {
        var relatorio = await Rodar();

        relatorio.Valor(CargaDoArt.RotuloDeSeriesConfirmadas).Should().Be(1);

        var registros = await Registros();
        registros["5002"].Decisao.Should().Be(DecisaoDaIntegracao.Importado);
        registros["5002"].Transformacoes.Should().Contain("confirmado no cadastro de veículos do Protheus");
        registros["5008"].Motivos.Should().Be(MotivoDePendenciaDoArt.ChassiIncompleto);
        registros["5009"].Motivos.Should().Be(MotivoDePendenciaDoArt.IdentificadorDeComponente);

        await using var db = Sistema();
        var serie = db.Equipamentos.AsNoTracking().AsEnumerable().Single(e => e.Chassi.Numero == "PY6110J012345");
        serie.Chassi.EhVin.Should().BeFalse();
        serie.Origem.Should().Be(OrigemDoEquipamento.Art);
    }

    [Fact]
    public async Task O_VIN_com_o_prefixo_1CQ_entra()
    {
        await Rodar();

        (await Registros())["5003"].Decisao.Should().Be(DecisaoDaIntegracao.Importado);
    }

    [Fact]
    public async Task O_comprador_fora_do_CRM_da_lugar_ao_dono_do_Protheus_e_sai_da_fila()
    {
        var relatorio = await Rodar();

        relatorio.Valor(CargaDoArt.RotuloDeCompradoresPeloProtheus).Should().Be(1);

        await using var db = Sistema();
        var venda = await db.VendasDeMaquina.AsNoTracking().SingleAsync(v => v.ChaveOrigem == "5004");
        venda.CompradorId.Should().Be(_semente.Clientes[CnpjB], "o dono atual da máquina no Protheus é cliente");
        venda.CompradorPeloDonoNoProtheus.Should().BeTrue();

        (await Registros())["5004"].Transformacoes.Should().Contain("dono atual da máquina no Protheus");

        var fila = await db.CompradoresPendentes.AsNoTracking().SingleAsync();
        fila.Situacao.Should().Be(SituacaoDoCompradorPendente.ResolvidoPeloDonoNoProtheus, "a fila esvazia para quem o dono do Protheus resolveu");
    }

    [Fact]
    public async Task Quando_o_comprador_verdadeiro_passa_a_existir_ele_toma_o_lugar_sem_divergencia()
    {
        await Rodar();

        await using (var db = Sistema())
        {
            db.Clientes.Add(Cliente.Criar(_semente.RibeiraoPreto, "COMPRADOR FICTICIO", TipoDePessoa.Fisica,
                proprietarioId: _semente.Operador, criadoPorId: _semente.Operador, documento: CpfCnpj.Criar(CpfForaDoCrm), situacao: SituacaoDoCliente.Suspect));
            await db.SaveChangesAsync();
        }

        await Rodar();

        await using var depois = Sistema();
        var venda = await depois.VendasDeMaquina.AsNoTracking().SingleAsync(v => v.ChaveOrigem == "5004");
        var comprador = await depois.Clientes.AsNoTracking().Where(c => c.Id == venda.CompradorId).Select(c => c.NomeRazao).SingleAsync();
        comprador.Should().Be("COMPRADOR FICTICIO");
        venda.CompradorPeloDonoNoProtheus.Should().BeFalse();

        (await depois.DivergenciasDeIntegracao.CountAsync(d => d.Tipo == TipoDeDivergencia.CompradorAlteradoNaOrigem))
            .Should().Be(0, "o ART não trocou de comprador: o comprador dele é que passou a existir no CRM");

        var vinculos = await depois.VinculosComEquipamento.AsNoTracking().Where(v => v.VendaDeMaquinaId == venda.Id).OrderBy(v => v.Id).ToListAsync();
        vinculos.Should().HaveCount(2);
        vinculos[0].MotivoDoEncerramento.Should().Contain("passou a existir no CRM");
        vinculos[1].EncerradoEm.Should().BeNull();

        (await depois.CompradoresPendentes.AsNoTracking().SingleAsync()).Situacao.Should().Be(SituacaoDoCompradorPendente.Cadastrado);
    }

    [Fact]
    public async Task A_divergencia_so_fica_registrada_quando_o_ART_prevalece()
    {
        var relatorio = await Rodar();

        relatorio.Valor($"  dono no Protheus × comprador do ART: {DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior}").Should().Be(1);
        relatorio.Valor($"  dono no Protheus × comprador do ART: {DesfechoDaComparacaoComOArt.MesmoDono}").Should().Be(2, "5001 e a série curta 5002");

        await using var db = Sistema();
        var divergencias = await db.DivergenciasDeIntegracao.AsNoTracking()
            .Where(d => d.Tipo == TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador)
            .ToDictionaryAsync(d => d.ChaveOrigem);

        divergencias.Keys.Should().BeEquivalentTo(["5006", "5007"],
            "5005 tem ordem de serviço do dono depois da venda (vale o Protheus); 5004 entrou com o dono do Protheus; 5002 tem o mesmo dono");
        divergencias["5006"].Descricao.Should().Contain("não tem nota de venda nem ordem de serviço");
        divergencias["5007"].Descricao.Should().Contain("própria Tracbel");
        divergencias["5007"].ValorNoProtheus.Should().Contain("raiz de CNPJ do grupo");
    }

    [Fact]
    public async Task A_divergencia_deixa_de_ocorrer_com_a_evidencia_mas_nao_quando_o_Protheus_nao_foi_lido()
    {
        await Rodar();
        await Rodar(protheus: Protheus(evidenciaDaSeis: true));

        await using (var db = Sistema())
        {
            var seis = await db.DivergenciasDeIntegracao.AsNoTracking().SingleAsync(d => d.ChaveOrigem == "5006");
            seis.Situacao.Should().Be(SituacaoDaDivergencia.DeixouDeOcorrer, "apareceu a ordem de serviço do dono depois da venda");
        }

        await Rodar(semProtheus: true);

        await using var depois = Sistema();
        (await depois.DivergenciasDeIntegracao.AsNoTracking().SingleAsync(d => d.ChaveOrigem == "5007")).Situacao
            .Should().Be(SituacaoDaDivergencia.Aberta, "sem o Protheus nesta rodada, a divergência com ele não foi procurada");
    }

    [Fact]
    public async Task A_projecao_decide_igual_a_carga_mostra_o_que_libera_e_nao_grava_nada()
    {
        // HOJE: a rodada sem o Protheus deixa pendentes o número de série e o comprador fora do CRM.
        await Rodar(semProtheus: true);
        var antes = await Registros();
        antes["5002"].Motivos.Should().Be(MotivoDePendenciaDoArt.ChassiIncompleto);
        antes["5004"].Motivos.Should().Be(MotivoDePendenciaDoArt.CompradorAusente);

        int equipamentos, vendas, divergencias;
        await using (var db = Sistema())
            (equipamentos, vendas, divergencias) = (await db.Equipamentos.CountAsync(), await db.VendasDeMaquina.CountAsync(), await db.DivergenciasDeIntegracao.CountAsync());

        var projecao = await Carga(Vendas(), Protheus()).ProjetarAsync(CancellationToken.None);

        projecao.EhSucesso.Should().BeTrue(projecao.Erro);
        projecao.Valor.Valor("registros pendentes hoje").Should().Be(4);
        projecao.Valor.Valor("registros pendentes hoje que entrariam").Should().Be(2);
        projecao.Valor.Valor($"  liberados que hoje estão pendentes por: {MotivoDePendenciaDoArt.ChassiIncompleto}").Should().Be(1);
        projecao.Valor.Valor($"  liberados que hoje estão pendentes por: {MotivoDePendenciaDoArt.CompradorAusente}").Should().Be(1);
        projecao.Valor.Valor("divergências que ficariam registradas (o ART prevalece)").Should().Be(2);
        projecao.Valor.Valor("compradores que sairiam da fila (resolvidos pelo dono do Protheus)").Should().Be(1);

        await using var depois = Sistema();
        (await depois.Equipamentos.CountAsync()).Should().Be(equipamentos);
        (await depois.VendasDeMaquina.CountAsync()).Should().Be(vendas);
        (await depois.DivergenciasDeIntegracao.CountAsync()).Should().Be(divergencias);
        (await Registros())["5002"].Motivos.Should().Be(MotivoDePendenciaDoArt.ChassiIncompleto, "a projeção não grava");
    }
}
