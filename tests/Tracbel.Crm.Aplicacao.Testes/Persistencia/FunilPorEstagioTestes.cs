using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Aplicacao.Testes.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// O FUNIL NA TELA (documento 52, decisões de 27/09/2026) — a rota de leitura sobre <c>processo.EstagioDoProcesso</c>,
/// num banco em memória: coorte × fluxo, os dois percentuais, o subfunil digital, o alerta de processo parado, o motivo
/// verdadeiro do funil vazio e as vendas perdidas do período com o processo perdido vindo do funil.
///
/// <para>Quatro processos, no FY2026 até agosto (o padrão em 27/09/2026): o 1 abre em dez/2025 e para no Pedido desde
/// maio; o 2 abre ANTES do ano fiscal e fatura em março — está no fluxo e não na coorte; o 3 é da entrada digital e se
/// perde em fevereiro; o 4 abre em agosto e chega à Negociação há menos de 60 dias.</para>
/// </summary>
public sealed class FunilPorEstagioTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDoFunil _semente;
    private readonly Guid _maria;

    public FunilPorEstagioTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Banco();
        db.Database.EnsureCreated();
        _semente = CenarioDoFunilDoVortice.Semear(db);
        _maria = db.Usuarios.Single(u => u.Id == _semente.Maria).ChavePublica;
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Banco() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private static DateTime Em(int ano, int mes, int dia) => CenarioDoFunilDoVortice.Em(ano, mes, dia);

    private void Semear()
    {
        using var db = Banco();
        var r = _semente.RibeiraoPreto;

        void Processo(long numero, DateTime aberto, SituacaoDoProcesso desfecho, DateTime? desfechoEm, long? responsavel, bool digital,
            params (EstagioDoFunil Estagio, DateTime Em)[] estagios)
        {
            foreach (var (estagio, em) in estagios)
                db.EstagiosDoProcesso.Add(EstagioDoProcesso.Registrar(new RetratoDoEstagio(
                    r, numero, 41, null, responsavel is null ? null : _semente.CarteiraId, responsavel, aberto, false, desfecho, desfechoEm,
                    estagio, em, null, null, 250, digital && estagio <= EstagioDoFunil.Qualificado)));
        }

        Processo(1, Em(2025, 12, 1), SituacaoDoProcesso.Aberto, null, _semente.Maria, false,
            (EstagioDoFunil.Lead, Em(2025, 12, 2)), (EstagioDoFunil.Qualificado, Em(2025, 12, 2)), (EstagioDoFunil.Cobertura, Em(2026, 1, 10)),
            (EstagioDoFunil.Negociacao, Em(2026, 3, 1)), (EstagioDoFunil.Pedido, Em(2026, 5, 1)));
        Processo(2, Em(2025, 6, 1), SituacaoDoProcesso.Ganho, Em(2026, 3, 1), _semente.Operador, false,
            (EstagioDoFunil.Lead, Em(2025, 6, 2)), (EstagioDoFunil.Qualificado, Em(2025, 6, 2)), (EstagioDoFunil.Cobertura, Em(2025, 12, 5)),
            (EstagioDoFunil.Negociacao, Em(2026, 2, 1)), (EstagioDoFunil.Pedido, Em(2026, 2, 10)), (EstagioDoFunil.Faturamento, Em(2026, 3, 1)));
        Processo(3, Em(2026, 1, 5), SituacaoDoProcesso.Perdido, Em(2026, 2, 1), null, true,
            (EstagioDoFunil.Lead, Em(2026, 1, 5)), (EstagioDoFunil.Qualificado, Em(2026, 1, 6)));
        Processo(4, Em(2026, 8, 1), SituacaoDoProcesso.Aberto, null, _semente.Operador, false,
            (EstagioDoFunil.Lead, Em(2026, 8, 1)), (EstagioDoFunil.Qualificado, Em(2026, 8, 1)), (EstagioDoFunil.Cobertura, Em(2026, 8, 5)),
            (EstagioDoFunil.Negociacao, Em(2026, 8, 20)));
        db.SaveChanges();
    }

    private async Task<FunilPorEstagio> Funil(string? leitura = null, Guid? responsavel = null)
    {
        await using var db = Banco();
        var resultado = await new ObterFunilPorEstagio(new RepositorioDoFunilPorEstagio(db), new RelogioFixo(Agora))
            .ExecutarAsync(null, null, leitura, null, responsavel, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor!.Dados;
    }

    private static int[] Quantidades(FunilPorEstagio f) => [.. f.Estagios.Select(e => e.Processos)];

    [Fact]
    public async Task A_coorte_conta_os_abertos_no_ano_fiscal_e_ate_onde_chegaram()
    {
        Semear();
        var funil = await Funil();

        funil.Periodo.EhOPadrao.Should().BeTrue();
        funil.Periodo.Texto.Should().Be("nov/2025 a ago/2026");
        funil.Base.Should().Be("abertura");
        Quantidades(funil).Should().Equal([3, 3, 2, 2, 1, 0], "o processo 2 abriu antes do ano fiscal e fica fora da coorte");

        funil.Estagios[0].PercentualSobreOAnterior.Should().BeNull("o Lead não tem estágio anterior");
        funil.Estagios[2].PercentualSobreOAnterior.Should().Be(66.7m);
        funil.Estagios[4].PercentualSobreOLead.Should().Be(33.3m);
        funil.Estagios[0].PelaEntradaDigital.Should().Be(1, "o subfunil digital é o processo 3");
        funil.Estagios[0].Perdidos.Should().Be(1);
        funil.Estagios[0].Abertos.Should().Be(2);
    }

    [Fact]
    public async Task O_fluxo_conta_as_etapas_alcancadas_no_periodo_e_o_faturamento_de_quem_abriu_antes()
    {
        Semear();
        var funil = await Funil("etapa");

        funil.Base.Should().Be("etapa");
        Quantidades(funil).Should().Equal([3, 3, 3, 3, 2, 1], "no fluxo o processo 2 entra pelas etapas que alcançou no ano fiscal");
        funil.Estagios[5].Ganhos.Should().Be(1);
    }

    [Fact]
    public async Task Parado_e_o_estagio_mais_avancado_aberto_em_Negociacao_ou_Pedido_ha_mais_de_60_dias()
    {
        Semear();
        var funil = await Funil();

        funil.DiasParaParado.Should().Be(60);
        funil.ParadosEmNegociacaoOuPedido.Should().Be(1,
            "o 1 está no Pedido desde maio; o 2 faturou; o 4 chegou à Negociação há menos de 60 dias");
    }

    [Fact]
    public async Task O_responsavel_recorta_o_funil()
    {
        Semear();
        var funil = await Funil(responsavel: _maria);

        Quantidades(funil).Should().Equal([1, 1, 1, 1, 1, 0]);
    }

    [Fact]
    public async Task O_responsavel_que_nao_existe_e_nao_encontrado()
    {
        Semear();
        await using var db = Banco();
        var resultado = await new ObterFunilPorEstagio(new RepositorioDoFunilPorEstagio(db), new RelogioFixo(Agora))
            .ExecutarAsync(null, null, null, null, Guid.NewGuid(), CancellationToken.None);

        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);
    }

    [Fact]
    public async Task A_base_fora_da_lista_e_o_periodo_pela_metade_sao_recusados()
    {
        await using var db = Banco();
        var caso = new ObterFunilPorEstagio(new RepositorioDoFunilPorEstagio(db), new RelogioFixo(Agora));

        (await caso.ExecutarAsync(null, null, "semana", null, null, CancellationToken.None)).Tipo.Should().Be(TipoDeFalha.Validacao);
        (await caso.ExecutarAsync(new DateOnly(2026, 1, 1), null, null, null, null, CancellationToken.None)).Tipo.Should().Be(TipoDeFalha.Validacao);
    }

    [Fact]
    public async Task Sem_linha_o_funil_diz_que_a_rotina_ainda_nao_rodou_e_nao_mostra_zero()
    {
        var funil = await Funil();

        funil.Estagios.Should().BeEmpty();
        funil.ParadosEmNegociacaoOuPedido.Should().BeNull();
        funil.Rotina!.IniciadaEm.Should().BeNull();
        funil.MetricasSemDado.Should().ContainSingle(m => m.Metrica == "funil")
            .Which.Motivo.Should().Contain("ainda não rodou").And.Contain("PROCESSOS_VORTICE");
    }

    [Fact]
    public async Task Sem_linha_depois_da_rotina_rodar_o_motivo_e_outro()
    {
        await using (var db = Banco())
        {
            var rotina = db.Rotinas.Single(r => r.Codigo == RotinasDoSistema.ProcessosVortice);
            rotina.IniciarExecucao(Agora.AddHours(-6));
            rotina.EncerrarExecucao(ResultadoDaExecucao.Sucesso, null, Agora.AddHours(-5));
            db.SaveChanges();
        }

        var funil = await Funil();

        funil.MetricasSemDado.Should().ContainSingle(m => m.Metrica == "funil")
            .Which.Motivo.Should().Contain("rodou em").And.Contain("não trouxe processo");
    }

    [Fact]
    public async Task As_vendas_perdidas_sao_do_periodo_e_o_processo_perdido_vem_do_funil()
    {
        Semear();
        await using (var db = Banco())
        {
            var motivo = MotivoDePerda.Criar("PRECO", "Preço", CategoriaDeMotivoDePerda.Preco);
            db.MotivosDePerda.Add(motivo);
            db.SaveChanges();

            ConteudoDaVendaPerdida Perda(DateTime registrada, string formulario) => new(
                _semente.RibeiraoPreto, registrada, null, motivo.Id, null, null, null, null, null, null, 1, null, null,
                ParticipacaoNaNegociacao.NaoInformado, formulario, 3);

            db.VendasPerdidas.AddRange(
                VendaPerdida.DaOrigem(Perda(Em(2026, 2, 2), FormulariosDaVendaPerdida.Fy25), _semente.Operador),
                VendaPerdida.DaOrigem(Perda(Em(2025, 1, 10), FormulariosDaVendaPerdida.MaqImp), _semente.Operador));
            db.SaveChanges();
        }

        await using var leitura = Banco();
        var caso = new ObterVendasPerdidas(new RepositorioDeVendasPerdidas(leitura), new RelogioFixo(Agora));

        var padrao = (await caso.ExecutarAsync(null, null, null, null, CancellationToken.None)).Valor!.Dados;
        padrao.Registradas.Should().Be(1, "a de jan/2025 é de antes do ano fiscal");
        padrao.ProcessosPerdidos.Should().Be(1, "o processo 3 se perdeu em fev/2026");
        padrao.Periodo.Texto.Should().Be("nov/2025 a ago/2026");

        var doMaqImp = (await caso.ExecutarAsync(new DateOnly(2024, 11, 1), new DateOnly(2026, 8, 31), "iv_q_venda_perdida_maqimp", null,
            CancellationToken.None)).Valor!.Dados;
        doMaqImp.Registradas.Should().Be(1);
        doMaqImp.Formulario.Should().Be(FormulariosDaVendaPerdida.MaqImp);

        (await caso.ExecutarAsync(null, null, "IV_Q_VENDA_PERDIDA_MANITO", null, CancellationToken.None)).Tipo
            .Should().Be(TipoDeFalha.Validacao, "o _MANITO (Colorado) não entra");
    }

    private sealed class RelogioFixo(DateTime agora) : IRelogio
    {
        public DateTime Agora { get; } = agora;
    }
}
