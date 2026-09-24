using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoParque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O PARQUE PELO PROPRIETÁRIO ATUAL DO PROTHEUS (decisões de 24/09/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada cria a máquina que o CRM não tem (VIN com 1CQ, número de série curto), abre um vínculo de
/// dono atual com a evidência certa e deixa na trilha, com o motivo, o que não entra — componente, estoque, a própria
/// Tracbel, dono fora do CRM, chassi repetido com donos diferentes, identificador com lixo;</item>
/// <item>quando o Protheus e o ART concordam, a máquina deixa de ser "proprietário não confirmado";</item>
/// <item>é sincronia: rodar de novo sem mudança não grava nada; o dono que muda encerra o vínculo anterior — nunca o
/// apaga —; o dono confirmado que o Protheus desmente com evidência volta a não confirmado;</item>
/// <item>a leitura vazia não muda nada, a trava de encerramento em massa existe, e a simulação não grava nada.</item>
/// </list>
/// <para>SQLite em memória com o modelo de verdade, como as outras cargas.</para>
/// </summary>
public sealed class CargaDoParqueDoProtheusTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDoParque _semente;

    public CargaDoParqueDoProtheusTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, _semente.Filiais));

    private async Task<RelatorioDoParqueDoProtheus> Sincronizar(IReadOnlyList<MaquinaNoProtheus>? parque = null, bool simular = false, DateTime? quando = null)
    {
        var resultado = await Sincronia(DaCarga, parque ?? Parque(), quando ?? Agora, _semente).ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private long Cliente(string documento) => _semente.Clientes[documento];

    // =============================================================================================
    // A primeira rodada
    // =============================================================================================

    [Fact]
    public async Task A_primeira_rodada_cria_as_maquinas_que_faltam_e_abre_o_dono_atual_com_a_evidencia()
    {
        var relatorio = await Sincronizar();

        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeMaquinasComDono).Should().Be(3);
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeMaquinasCriadas).Should().Be(2, "o VIN com 1CQ e a série curta; a do ART já existia");
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAbertos).Should().Be(3);
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosEncerrados).Should().Be(0);

        await using var db = Sistema();
        var idDoNovo = MaquinaId(db, ChassiNovo1Cq);
        var idDaSerie = MaquinaId(db, SerieCurta);
        var novo = await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == idDoNovo);
        novo.Origem.Should().Be(OrigemDoEquipamento.Protheus);
        novo.Situacao.Should().Be(SituacaoDoEquipamento.ProprietarioNaoConfirmado, "só o Protheus não confirma o dono");
        novo.ClienteId.Should().BeNull();
        novo.EmpresaId.Should().Be(_semente.RibeiraoPreto, "a filial da máquina nova é a do cliente");
        (novo.AnoFabricacao, novo.AnoModelo).Should().Be(((short?)2021, (short?)2022));

        var serie = await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == idDaSerie);
        serie.Chassi.EhVin.Should().BeFalse("o número de série que o Protheus tem é a identidade da máquina");
        serie.EmpresaId.Should().Be(_semente.Barretos);

        var donos = await db.VinculosComEquipamento.AsNoTracking()
            .Where(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual).ToListAsync();
        donos.Should().HaveCount(3).And.OnlyContain(v => v.EncerradoEm == null && v.VendaDeMaquinaId == null);
        donos.Single(v => v.EquipamentoId == novo.Id).Should().BeEquivalentTo(
            new { ClienteId = Cliente(CpfA), Evidencia = (EvidenciaDoProprietario?)EvidenciaDoProprietario.NotaDeVenda, ReferenciaEm = (DateOnly?)new DateOnly(2024, 2, 1) },
            o => o.ExcludingMissingMembers());
        donos.Single(v => v.EquipamentoId == serie.Id).Should().BeEquivalentTo(
            new { ClienteId = Cliente(CnpjB), Evidencia = (EvidenciaDoProprietario?)EvidenciaDoProprietario.OrdemDeServico, EmpresaId = _semente.Barretos },
            o => o.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Quando_o_Protheus_e_o_ART_concordam_o_dono_fica_confirmado_e_o_comprador_continua_como_historico()
    {
        var relatorio = await Sincronizar();

        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeConfirmados).Should().Be(1);
        relatorio.Valor(DesfechoDaComparacaoComOArt.MesmoDono.ToString()).Should().Be(1);

        await using var db = Sistema();
        var doArt = await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == _semente.MaquinaDoArt);
        doArt.ClienteId.Should().Be(Cliente(CpfA));
        doArt.Situacao.Should().Be(SituacaoDoEquipamento.Ativo);

        var vinculos = await db.VinculosComEquipamento.AsNoTracking().Where(v => v.EquipamentoId == _semente.MaquinaDoArt).ToListAsync();
        vinculos.Should().HaveCount(2);
        vinculos.Should().ContainSingle(v => v.Natureza == NaturezaDoVinculoComEquipamento.CompradorNaVenda && v.EncerradoEm == null);
        var dono = vinculos.Single(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual);
        dono.Evidencia.Should().Be(EvidenciaDoProprietario.VendaNoArt, "no Protheus ele só tem o cadastro; a venda do ART é a evidência mais forte");
        dono.ReferenciaEm.Should().Be(FaturadaNoArt, "a data da venda no ART é a do faturamento");
    }

    [Fact]
    public async Task O_que_nao_entra_fica_na_trilha_com_o_motivo()
    {
        await Sincronizar();

        await using var db = Sistema();
        var registros = await db.RegistrosDeOrigem.AsNoTracking()
            .Where(r => r.Fluxo == CargaDoParqueDoProtheus.Fluxo).ToDictionaryAsync(r => r.ChaveOrigem);

        registros.Should().HaveCount(10, "uma linha por linha da VV1 com chassi; a sem chassi (pedido de fábrica) não é máquina");
        registros.Values.Count(r => r.Decisao == DecisaoDaIntegracao.Importado).Should().Be(3);
        registros["000004"].Motivos.Should().Be(MotivoDePendenciaDoParque.Componente);
        registros["000005"].Motivos.Should().Be(MotivoDePendenciaDoParque.SituacaoForaDoCliente);
        registros["000006"].Motivos.Should().Be(MotivoDePendenciaDoParque.DonoEAPropriaTracbel, "a Tracbel é cliente do CRM, mas não é dona de parque de cliente");
        registros["000007"].Motivos.Should().Be(MotivoDePendenciaDoParque.DonoForaDoCrm);
        registros["000008"].Motivos.Should().Be(MotivoDePendenciaDoParque.ChassiRepetidoComDonosDiferentes);
        registros["000009"].Motivos.Should().Be(MotivoDePendenciaDoParque.ChassiRepetidoComDonosDiferentes);
        registros["000010"].Motivos.Should().Be(MotivoDePendenciaDoParque.IdentificadorForaDoPadrao);
        registros["000002"].Transformacoes.Should().Contain("espaço interno");
        registros["000002"].ChassiNaOrigem.Should().Be("py6110j 012345", "a trilha guarda o chassi como o Protheus escreve");

        (await db.Equipamentos.CountAsync()).Should().Be(3, "nada do que ficou de fora vira máquina");
    }

    // =============================================================================================
    // É sincronia
    // =============================================================================================

    [Fact]
    public async Task Rodar_de_novo_sem_mudanca_nao_grava_nada()
    {
        await Sincronizar();
        DateTime? alteradaEm;
        await using (var db = Sistema())
            alteradaEm = (await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == _semente.MaquinaDoArt)).AlteradoEm;

        var segunda = await Sincronizar(quando: Agora.AddDays(1));

        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeMaquinasCriadas).Should().Be(0);
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAbertos).Should().Be(0);
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosEncerrados).Should().Be(0);
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAtualizados).Should().Be(0);
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosMantidos).Should().Be(3);
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeConfirmados).Should().Be(0);
        segunda.Valor("registros já conhecidos sem alteração").Should().Be(10);

        await using var depois = Sistema();
        (await depois.Equipamentos.CountAsync()).Should().Be(3);
        (await depois.VinculosComEquipamento.CountAsync(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual)).Should().Be(3);
        (await depois.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == _semente.MaquinaDoArt)).AlteradoEm
            .Should().Be(alteradaEm, "a máquina que nada mudou não é regravada — e a trilha não ganha linha 'nada mudou'");
    }

    [Fact]
    public async Task O_dono_que_muda_encerra_o_vinculo_anterior_e_abre_outro()
    {
        await Sincronizar();

        // NO PROTHEUS, meses depois: a máquina 1CQ foi revendida ao cliente C, e a oficina já o registrou.
        var parque = Parque();
        parque[0] = Maquina("000001", ChassiNovo1Cq, CpfC, nova: false, venda: new(2024, 2, 1), ordem: new(2026, 8, 1), ordemDoDono: true);
        var amanha = Agora.AddDays(1);

        var relatorio = await Sincronizar(parque, quando: amanha);

        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosEncerrados).Should().Be(1);
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAbertos).Should().Be(1);

        await using var db = Sistema();
        var maquina = MaquinaId(db, ChassiNovo1Cq);
        var donos = await db.VinculosComEquipamento.AsNoTracking()
            .Where(v => v.EquipamentoId == maquina && v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual)
            .OrderBy(v => v.Id).ToListAsync();

        donos.Should().HaveCount(2, "o dono anterior não é apagado: fica encerrado, como histórico");
        donos[0].ClienteId.Should().Be(Cliente(CpfA));
        donos[0].EncerradoEm.Should().Be(amanha);
        donos[0].MotivoDoEncerramento.Should().Contain("O dono atual mudou");
        donos[1].ClienteId.Should().Be(Cliente(CpfC));
        donos[1].EncerradoEm.Should().BeNull();
        donos[1].Evidencia.Should().Be(EvidenciaDoProprietario.OrdemDeServico);
    }

    [Fact]
    public async Task A_maquina_que_volta_para_a_Tracbel_sai_do_parque_com_o_vinculo_encerrado()
    {
        await Sincronizar();

        var parque = Parque();
        parque[1] = Maquina("000002", SerieCurta, CnpjDaTracbel, situacao: "0");

        var relatorio = await Sincronizar(parque, quando: Agora.AddDays(1));

        relatorio.Valor("  porque a máquina saiu do parque").Should().Be(1);
        await using var db = Sistema();
        var maquina = MaquinaId(db, SerieCurta);
        var dono = await db.VinculosComEquipamento.AsNoTracking()
            .SingleAsync(v => v.EquipamentoId == maquina && v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual);
        dono.EncerradoEm.Should().NotBeNull();
        dono.MotivoDoEncerramento.Should().Contain(MotivoDePendenciaDoParque.SituacaoForaDoCliente);
        (await db.Equipamentos.CountAsync()).Should().Be(3, "a máquina continua no CRM; só o dono atual deixou de valer");
    }

    [Fact]
    public async Task O_dono_confirmado_que_o_Protheus_desmente_com_evidencia_volta_a_nao_confirmado()
    {
        await Sincronizar();

        // A MÁQUINA DO ART foi revendida ao cliente C: a oficina o registrou DEPOIS do faturamento do ART.
        var parque = Parque();
        parque[2] = Maquina("000003", ChassiDoArt, CpfC, ordem: FaturadaNoArt.AddYears(2), ordemDoDono: true);

        var relatorio = await Sincronizar(parque, quando: Agora.AddDays(1));

        relatorio.Valor(DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior.ToString()).Should().Be(1);
        await using var db = Sistema();
        var doArt = await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == _semente.MaquinaDoArt);
        doArt.ClienteId.Should().BeNull("o Protheus e o ART deixaram de concordar");
        doArt.Situacao.Should().Be(SituacaoDoEquipamento.ProprietarioNaoConfirmado);

        var vigente = await db.VinculosComEquipamento.AsNoTracking().SingleAsync(v =>
            v.EquipamentoId == _semente.MaquinaDoArt && v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual && v.EncerradoEm == null);
        vigente.ClienteId.Should().Be(Cliente(CpfC), "com evidência depois da venda no ART, vale o Protheus");
        (await db.VinculosComEquipamento.CountAsync(v => v.EquipamentoId == _semente.MaquinaDoArt && v.Natureza == NaturezaDoVinculoComEquipamento.CompradorNaVenda && v.EncerradoEm == null))
            .Should().Be(1, "o comprador do ART fica, sempre, como histórico");
    }

    [Fact]
    public async Task Confirmado_e_depois_desmentido_sem_evidencia_o_dono_continua_o_do_ART_mas_sem_confirmacao()
    {
        await Sincronizar();

        var parque = Parque();
        parque[2] = Maquina("000003", ChassiDoArt, CpfC);

        var relatorio = await Sincronizar(parque, quando: Agora.AddDays(1));

        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosEncerrados).Should().Be(0, "o dono atual continua o comprador do ART");
        await using var db = Sistema();
        var doArt = await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == _semente.MaquinaDoArt);
        doArt.Situacao.Should().Be(SituacaoDoEquipamento.ProprietarioNaoConfirmado, "confirmado é quando as duas fontes dizem o mesmo");
        doArt.ClienteId.Should().BeNull();
    }

    [Fact]
    public async Task Sem_evidencia_depois_da_venda_no_ART_o_dono_atual_e_o_comprador_do_ART()
    {
        var parque = Parque();
        parque[2] = Maquina("000003", ChassiDoArt, CpfC, ordem: FaturadaNoArt.AddDays(-30), ordemDoDono: true);

        var relatorio = await Sincronizar(parque);

        relatorio.Valor(DesfechoDaComparacaoComOArt.ArtPrevaleceSemEvidencia.ToString()).Should().Be(1);
        await using var db = Sistema();
        var dono = await db.VinculosComEquipamento.AsNoTracking().SingleAsync(v =>
            v.EquipamentoId == _semente.MaquinaDoArt && v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual);
        dono.ClienteId.Should().Be(Cliente(CpfA));
        dono.Evidencia.Should().Be(EvidenciaDoProprietario.VendaNoArt);
        (await db.Equipamentos.AsNoTracking().SingleAsync(e => e.Id == _semente.MaquinaDoArt)).ClienteId
            .Should().BeNull("as fontes discordam: o dono não é confirmado");
    }

    [Fact]
    public async Task A_leitura_vazia_nao_encerra_nada()
    {
        await Sincronizar();

        var resultado = await Sincronia(DaCarga, [], Agora.AddDays(1), _semente).ExecutarAsync(false, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse("cadastro de veículos sem máquina é leitura a conferir, não parque vazio");
        await using var db = Sistema();
        (await db.VinculosComEquipamento.CountAsync(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual && v.EncerradoEm == null))
            .Should().Be(3);
    }

    [Theory]
    [InlineData(999, 999, false)]    // parque pequeno: a trava não se aplica
    [InlineData(1000, 200, false)]   // um quinto exato ainda passa
    [InlineData(1000, 201, true)]
    [InlineData(26000, 5201, true)]
    public void A_trava_recusa_o_encerramento_em_massa(int vigentes, int aEncerrar, bool recusa) =>
        CargaDoParqueDoProtheus.EncerramentoPassaDaTrava(vigentes, aEncerrar).Should().Be(recusa);

    [Fact]
    public void A_chave_gravada_se_normaliza_em_si_mesma()
    {
        CargaDoParqueDoProtheus.SeNormalizaEmSiMesma(ChassiDoArt).Should().BeTrue();
        CargaDoParqueDoProtheus.SeNormalizaEmSiMesma(SerieCurta).Should().BeTrue();
        CargaDoParqueDoProtheus.SeNormalizaEmSiMesma("1rw7250pvmr123456").Should().BeFalse();
    }

    // =============================================================================================
    // A simulação
    // =============================================================================================

    [Fact]
    public async Task A_simulacao_conta_o_que_faria_e_nao_grava_nada()
    {
        var relatorio = await Sincronizar(simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeMaquinasCriadas).Should().Be(2);
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAbertos).Should().Be(3);
        relatorio.Valor("chaves de máquina do CRM que a regra nova do chassi reescreveria (tem de ser 0)").Should().Be(0);

        await using var db = Sistema();
        (await db.Equipamentos.CountAsync()).Should().Be(1);
        (await db.VinculosComEquipamento.CountAsync(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual)).Should().Be(0);
        (await db.RegistrosDeOrigem.CountAsync()).Should().Be(0);
        (await db.Sistemas.CountAsync(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema)).Should().Be(0, "nem o sistema é registrado");
        (await db.Equipamentos.AsNoTracking().SingleAsync()).Situacao.Should().Be(SituacaoDoEquipamento.ProprietarioNaoConfirmado);
    }
}
