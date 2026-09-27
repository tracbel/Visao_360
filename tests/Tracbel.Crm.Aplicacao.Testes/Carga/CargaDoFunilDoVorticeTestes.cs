using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoFunilDoVortice;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A ROTINA DO FUNIL E DAS VENDAS PERDIDAS DO VÓRTICE (decisões de 27/09/2026, documento 52). O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada cria o funil do casado e do prospect, liga ao cliente, à carteira e ao responsável que o CRM
/// já tem — e não cria cliente, carteira nem usuário;</item>
/// <item>duas rodadas sem mudança na origem não alteram nada;</item>
/// <item>leitura vazia e queda de mais de 5% abortam sem apagar;</item>
/// <item>o estágio que some de um processo presente sai, e a trilha de auditoria guarda o que era;</item>
/// <item>a simulação não grava;</item>
/// <item>as pendências ficam em <c>integracao.RegistroDeOrigem</c>, com o motivo;</item>
/// <item>os papéis da venda perdida, e a resposta que some é excluída — e volta.</item>
/// </list>
/// <para>SQLite em memória com o modelo de verdade, como as outras cargas.</para>
/// </summary>
public sealed class CargaDoFunilDoVorticeTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDoFunil _semente;

    public CargaDoFunilDoVorticeTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(
        _semente.Operador, _semente.RibeiraoPreto, new HashSet<int> { _semente.RibeiraoPreto, _semente.Barretos }));

    private async Task<Resultado<RelatorioDoFunilDoVortice>> Rodar(
        LeituraDoFunilDoVortice? funil = null, List<RespostaDeVendaPerdidaNoVortice>? respostas = null, bool simular = false) =>
        await Rotina(DaCarga, _semente, funil ?? Funil(), respostas ?? Respostas(), Agora).ExecutarAsync(simular, CancellationToken.None);

    private async Task<RelatorioDoFunilDoVortice> Sincronizar(
        LeituraDoFunilDoVortice? funil = null, List<RespostaDeVendaPerdidaNoVortice>? respostas = null, bool simular = false)
    {
        var resultado = await Rodar(funil, respostas, simular);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    // =============================================================================================
    // A primeira rodada
    // =============================================================================================

    [Fact]
    public async Task A_primeira_rodada_cria_o_funil_do_casado_e_do_prospect_e_liga_ao_que_o_CRM_ja_tem()
    {
        var relatorio = await Sincronizar();
        relatorio.Valor(CargaDoFunilDoVortice.RotuloDeLinhasIncluidas).Should().Be(LinhasDoFunil);

        await using var db = Sistema();
        (await db.EstagiosDoProcesso.CountAsync()).Should().Be(LinhasDoFunil);

        // O CASADO: cinco estágios, ligado ao cliente pelo documento, à carteira pelo de-para e à vendedora pelo login.
        var casado = await db.EstagiosDoProcesso.AsNoTracking().Where(e => e.NumeroDoProcessoNaOrigem == 1001).ToListAsync();
        casado.Select(e => e.Estagio).Should().BeEquivalentTo([EstagioDoFunil.Lead, EstagioDoFunil.Qualificado, EstagioDoFunil.Cobertura,
            EstagioDoFunil.Negociacao, EstagioDoFunil.Pedido]);
        casado.Should().OnlyContain(e => e.ClienteId == _semente.ClienteId && e.CarteiraId == _semente.CarteiraId
                                         && e.ResponsavelId == _semente.Maria && e.EmpresaId == _semente.RibeiraoPreto
                                         && e.TipoDeProcessoNaOrigem == 41 && e.Desfecho == SituacaoDoProcesso.Aberto);

        // O PROSPECT DA ENTRADA DIGITAL: entra só no funil, sem cliente, sem carteira, sem responsável.
        var digital = Linha(db, 1002, EstagioDoFunil.Lead)!;
        digital.Should().BeEquivalentTo(new
        {
            ClienteId = (long?)null, CarteiraId = (long?)null, ResponsavelId = (long?)null, PelaEntradaDigital = true, ResultadoQueAbriu = 1278
        }, o => o.ExcludingMissingMembers());

        // O FILHO DNA herda a Cobertura do pai, que sai.
        var herdada = Linha(db, 1004, EstagioDoFunil.Cobertura)!;
        herdada.HerdadoDoProcessoDna.Should().BeTrue();
        herdada.NumeroDoProcessoDnaNaOrigem.Should().Be(1003);
        herdada.EmpresaId.Should().Be(_semente.Barretos);
        (await db.EstagiosDoProcesso.AnyAsync(e => e.NumeroDoProcessoNaOrigem == 1003)).Should().BeFalse();

        // NADA DE CADASTRO NOVO: o prospect não vira cliente, e ninguém ganha carteira ou conta.
        (await db.Clientes.CountAsync()).Should().Be(1);
        (await db.Carteiras.CountAsync()).Should().Be(1);
        (await db.Usuarios.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task A_venda_perdida_entra_com_o_papel_de_cada_resposta_e_so_a_principal_conta()
    {
        await Sincronizar();

        await using var db = Sistema();
        var sistemaId = await db.Sistemas.Where(s => s.Codigo == "VORTICE").Select(s => s.Id).SingleAsync();
        var porChave = (await db.ChavesExternas.AsNoTracking().Where(c => c.SistemaId == sistemaId && c.Entidade == nameof(VendaPerdida)).ToListAsync())
            .ToDictionary(c => c.ChaveOrigem, c => c.RegistroId);
        var vendas = await db.VendasPerdidas.AsNoTracking().ToDictionaryAsync(v => v.Id);
        VendaPerdida Da(int questionario) => vendas[porChave[questionario.ToString(System.Globalization.CultureInfo.InvariantCulture)]];

        porChave.Should().HaveCount(6 + PerdasDeEnchimento, "a sem data e a de filial fora do CRM ficam pendentes");

        Da(1).Should().BeEquivalentTo(new
        {
            Papel = PapelDaVendaPerdida.Principal, FormularioDeOrigem = FormulariosDaVendaPerdida.Antigo, ClienteId = (long?)_semente.ClienteId,
            Participacao = ParticipacaoNaNegociacao.Sim, PrecoDoConcorrente = (decimal?)500_000m, EmpresaId = _semente.RibeiraoPreto
        }, o => o.ExcludingMissingMembers());
        Da(2).Papel.Should().Be(PapelDaVendaPerdida.Duplicata);
        Da(2).VendaPerdidaPrincipalId.Should().Be(Da(1).Id, "o _JDE gêmeo do antigo repete a perda");

        Da(3).Participacao.Should().Be(ParticipacaoNaNegociacao.Nao, "o FY25 do par com o SEM_PARTICIPACAO fica com participação \"Não\"");
        Da(3).NumeroDoProcessoNaOrigem.Should().Be(1001);
        Da(4).Should().BeEquivalentTo(new { Papel = PapelDaVendaPerdida.Duplicata, VendaPerdidaPrincipalId = (long?)Da(3).Id }, o => o.ExcludingMissingMembers());
        Da(5).Should().BeEquivalentTo(new { Papel = PapelDaVendaPerdida.Complemento, VendaPerdidaPrincipalId = (long?)Da(3).Id }, o => o.ExcludingMissingMembers());
        Da(6).Papel.Should().Be(PapelDaVendaPerdida.Principal, "o VP_* solto entra como principal");

        var motivos = await db.MotivosDePerda.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.Codigo);
        motivos[Da(6).MotivoDePerdaId].Should().Be(CargaDoFunilDoVortice.MotivoNaoInformado);
        motivos[Da(1).MotivoDePerdaId].Should().Be("PRECO");
        Da(1).ConcorrenteId.Should().Be(Da(2).ConcorrenteId, "New Holland e NEW HOLLAND são o mesmo item do catálogo");

        vendas.Values.Count(v => v.Conta).Should().Be(3 + PerdasDeEnchimento, "o antigo, o FY25 e o VP solto — duplicata e complemento não contam");
    }

    // =============================================================================================
    // A sincronia
    // =============================================================================================

    [Fact]
    public async Task Duas_rodadas_sem_mudanca_na_origem_nao_alteram_nada()
    {
        await Sincronizar();
        var antes = await Retrato();

        var segunda = await Sincronizar();

        foreach (var rotulo in new[]
                 {
                     CargaDoFunilDoVortice.RotuloDeLinhasIncluidas, CargaDoFunilDoVortice.RotuloDeLinhasAlteradas,
                     CargaDoFunilDoVortice.RotuloDeLinhasRemovidas, CargaDoFunilDoVortice.RotuloDeVendasPerdidasIncluidas,
                     CargaDoFunilDoVortice.RotuloDeVendasPerdidasAlteradas, CargaDoFunilDoVortice.RotuloDePapeisAlterados,
                     CargaDoFunilDoVortice.RotuloDeVendasPerdidasExcluidas, CargaDoFunilDoVortice.RotuloDeMotivosIncluidos
                 })
            segunda.Valor(rotulo).Should().Be(0, $"sem mudança na origem, '{rotulo}' é zero");

        segunda.Valor(CargaDoFunilDoVortice.RotuloDeLinhasMantidas).Should().Be(LinhasDoFunil);
        (await Retrato()).Should().Be(antes, "o funil e as vendas perdidas ficam exatamente como estavam");

        await using var db = Sistema();
        (await db.AlteracoesDeCampo.CountAsync()).Should().Be(0, "nada mudou — a trilha de auditoria não tem o que registrar");
        (await db.RegistrosDeOrigem.Where(r => r.Fluxo == CargaDoFunilDoVortice.Fluxo).Select(r => r.Leituras).Distinct().ToListAsync())
            .Should().Equal(2);
    }

    [Fact]
    public async Task A_leitura_vazia_aborta_sem_apagar()
    {
        await Sincronizar();

        var semProcessos = await Rodar(funil: new LeituraDoFunilDoVortice([], []));
        semProcessos.EhSucesso.Should().BeFalse();
        semProcessos.Erro.Should().Contain("Leitura vazia não é funil vazio");

        var semRespostas = await Rodar(respostas: []);
        semRespostas.EhSucesso.Should().BeFalse();

        await using var db = Sistema();
        (await db.EstagiosDoProcesso.CountAsync()).Should().Be(LinhasDoFunil, "nada foi removido");
        (await db.VendasPerdidas.CountAsync(v => v.ExcluidoEm != null)).Should().Be(0);
    }

    [Fact]
    public async Task A_queda_de_mais_de_5_por_cento_aborta_sem_apagar()
    {
        await Sincronizar();

        // DEZ PROSPECTS SOMEM DE UMA VEZ: 10 das 70 linhas (14%) — leitura parcial, não o funil andando.
        var funil = Funil(Processos().Where(p => p.Numero is < 2000 or >= 2010).ToList());
        var resultado = await Rodar(funil: funil);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("mais do que os 5");

        await using var db = Sistema();
        (await db.EstagiosDoProcesso.CountAsync()).Should().Be(LinhasDoFunil);
    }

    [Fact]
    public async Task O_estagio_que_some_de_um_processo_presente_sai_e_a_trilha_guarda_o_que_era()
    {
        await Sincronizar();

        // A VENDA APROVADA DO 1001 FOI APAGADA NO VÓRTICE: ele volta à Cobertura. São 2 das 70 linhas (2,9%).
        var relatorio = await Sincronizar(Funil(historico: Historico().Where(h => h.SeqHistorico != 2).ToList()));
        relatorio.Valor(CargaDoFunilDoVortice.RotuloDeLinhasRemovidas).Should().Be(2);

        await using var db = Sistema();
        Linha(db, 1001, EstagioDoFunil.Pedido).Should().BeNull();
        Linha(db, 1001, EstagioDoFunil.Negociacao).Should().BeNull();
        Linha(db, 1001, EstagioDoFunil.Cobertura).Should().NotBeNull();

        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(EstagioDoProcesso)).ToListAsync();
        trilha.Should().OnlyContain(a => a.Operacao == OperacaoAuditada.Exclusao && a.Origem == OrigemDaOperacao.Integracao);
        trilha.Where(a => a.Campo == nameof(EstagioDoProcesso.Estagio)).Select(a => a.ValorAnterior)
            .Should().BeEquivalentTo(["Negociacao", "Pedido"]);
        trilha.Where(a => a.Campo == nameof(EstagioDoProcesso.NumeroDoProcessoNaOrigem)).Select(a => a.ValorAnterior)
            .Should().OnlyContain(v => v == "1001");
    }

    [Fact]
    public async Task A_simulacao_nao_grava_nada()
    {
        var relatorio = await Sincronizar(simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDoFunilDoVortice.RotuloDeLinhasIncluidas).Should().Be(LinhasDoFunil);
        relatorio.Valor(CargaDoFunilDoVortice.RotuloDeVendasPerdidasIncluidas).Should().Be(6 + PerdasDeEnchimento);

        await using var db = Sistema();
        (await db.EstagiosDoProcesso.CountAsync()).Should().Be(0);
        (await db.VendasPerdidas.CountAsync()).Should().Be(0);
        (await db.RegistrosDeOrigem.CountAsync()).Should().Be(0);
        (await db.MotivosDePerda.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task As_pendencias_ficam_no_RegistroDeOrigem_com_o_motivo()
    {
        var relatorio = await Sincronizar();
        relatorio.Valor(CargaDoFunilDoVortice.RotuloDeProcessosPendentes).Should().Be(3);

        await using var db = Sistema();
        var processos = await db.RegistrosDeOrigem.AsNoTracking().Where(r => r.Fluxo == CargaDoFunilDoVortice.Fluxo)
            .ToDictionaryAsync(r => r.ChaveOrigem);

        processos["1003"].Motivos.Should().Be(MotivoDePendenciaDoFunil.PaiSubstituidoPeloFilhoDna);
        processos["1005"].Motivos.Should().Be(MotivoDePendenciaDoFunil.FilialDoProcessoForaDoCrm);
        processos["1006"].Motivos.Should().Be(MotivoDePendenciaDoFunil.SemResultadoAceito);
        new[] { "1003", "1005", "1006" }.Should().OnlyContain(c => processos[c].Decisao == DecisaoDaIntegracao.Pendente);
        processos["1001"].Decisao.Should().Be(DecisaoDaIntegracao.Importado);
        processos["1001"].LinhaNaOrigem.Should().Be("PROCESSO 41");
        processos.Should().NotContainKey("1007", "o processo de antes da janela não é do universo");

        var respostas = await db.RegistrosDeOrigem.AsNoTracking().Where(r => r.Fluxo == CargaDoFunilDoVortice.FluxoDaVendaPerdida)
            .ToDictionaryAsync(r => r.ChaveOrigem);
        respostas["7"].Motivos.Should().Be(MotivoDePendenciaDoFunil.SemDataDeRegistro);
        respostas["8"].Motivos.Should().Be(MotivoDePendenciaDoFunil.FilialForaDoCrm);
        respostas["1"].LinhaNaOrigem.Should().Be(FormulariosDaVendaPerdida.Antigo);
    }

    [Fact]
    public async Task A_resposta_que_some_da_origem_e_excluida_e_volta_quando_reaparece()
    {
        await Sincronizar();

        var semUma = Respostas().Where(r => r.Questionario != 100).ToList();
        (await Sincronizar(respostas: semUma)).Valor(CargaDoFunilDoVortice.RotuloDeVendasPerdidasExcluidas).Should().Be(1);

        await using (var db = Sistema())
            (await db.VendasPerdidas.CountAsync(v => v.ExcluidoEm != null)).Should().Be(1, "exclusão lógica: a perda some da conta, não do banco");

        await Sincronizar();
        await using (var db = Sistema())
            (await db.VendasPerdidas.CountAsync(v => v.ExcluidoEm != null)).Should().Be(0, "voltou à origem, volta à conta");
    }

    /// <summary>O funil e as vendas perdidas, como texto — para comparar duas rodadas.</summary>
    private async Task<string> Retrato()
    {
        await using var db = Sistema();
        var funil = (await db.EstagiosDoProcesso.AsNoTracking().OrderBy(e => e.Id).ToListAsync()).Select(e => $"{e.Id}:{e.Retrato}");
        var vendas = (await db.VendasPerdidas.AsNoTracking().OrderBy(v => v.Id).ToListAsync())
            .Select(v => $"{v.Id}:{v.Conteudo}:{v.Papel}:{v.VendaPerdidaPrincipalId}:{v.ExcluidoEm}:{v.AlteradoEm}");
        return string.Join('\n', funil.Concat(vendas));
    }
}
