using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// AS METAS DE VENDA DA API GESTÃO DE NEGÓCIOS (#138). O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada cria uma meta por id, casa a filial, a classificação e a conta do consultor, e deixa as
/// recusas na fila de descarte, sem nome;</item>
/// <item>a segunda leitura igual grava zero; a revisão muda a meta e deixa a trilha como integração da GN;</item>
/// <item>a meta que some é excluída (nunca apagada) e, se volta, é reativada na MESMA linha; a linha recusada não apaga a
/// meta que já existia;</item>
/// <item>a trava: 25% de remoção ABORTA a carga inteira; 10% exclui; <c>--aceitar-remocao</c> passa por cima;</item>
/// <item>a simulação não grava nada.</item>
/// </list>
/// <para>SQLite em memória com o modelo de verdade, como as outras cargas.</para>
/// </summary>
public sealed class CargaDeMetasDaGestaoDeNegociosTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 27, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly SementeDasMetas _semente;

    public CargaDeMetasDaGestaoDeNegociosTestes()
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

    private async Task<RelatorioDasMetasDaGestaoDeNegocios> Sincronizar(
        IReadOnlyList<MetaNaOrigem>? cadastro = null, bool simular = false, bool aceitarRemocao = false, DateTime? quando = null)
    {
        var resultado = await Sincronia(DaCarga, cadastro ?? CadastroDeMetas(), quando ?? Agora, _semente.Operador)
            .ExecutarAsync(simular, aceitarRemocao, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [Fact]
    public async Task A_primeira_rodada_cria_uma_meta_por_id_e_casa_filial_classificacao_e_consultor()
    {
        var relatorio = await Sincronizar();

        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeLidas).Should().Be(7);
        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeNovas).Should().Be(5);
        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeRecusadas).Should().Be(2);
        relatorio.Valor($"  recusa: {MotivoDeRecusaDaMeta.FilialSemEmpresaNoCrm}").Should().Be(1);
        relatorio.Valor($"  recusa: {MotivoDeRecusaDaMeta.MesInvalido}").Should().Be(1);
        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeConsultoresSemConta).Should().Be(1);
        relatorio.Valor("  unidades de meta de máquinas").Should().Be(10, "2 + 1 + 4 + 3; o consórcio é à parte");
        relatorio.Valor("  cotas de meta de consórcio").Should().Be(7);
        relatorio.Contagens.Should().NotContain(c => c.Rotulo.Contains(ConsultorComConta, StringComparison.OrdinalIgnoreCase),
            "o relatório tem só contagens, nunca nome de consultor");

        await using var db = Sistema();
        var metas = await db.MetasDeVenda.AsNoTracking().OrderBy(m => m.IdNaOrigem).ToListAsync();
        metas.Select(m => m.IdNaOrigem).Should().Equal(1, 2, 3, 4, 5);

        var primeira = metas[0];
        primeira.EmpresaId.Should().Be(_semente.Ituverava, "filial_numero 05 é 010105");
        primeira.Competencia.Should().Be(new DateOnly(2025, 11, 1));
        primeira.LinhaDeProdutoId.Should().Be(_semente.TratorMedio);
        primeira.CodigoDaLinha.Should().Be("TRATOR_MEDIO");
        primeira.ConsultorUsuarioId.Should().Be(_semente.Consultor, "o login da conta é o consultor em minúsculas");
        primeira.GeradaNaOrigemEm.Should().Be(new DateTime(2026, 9, 27, 4, 30, 0, DateTimeKind.Utc));
        metas[1].VendaDireta.Should().BeTrue();
        metas[2].ConsultorUsuarioId.Should().BeNull("sem conta, a meta só aparece na visão da filial");
        metas[3].Origem.Should().Be(OrigemDaMeta.Consorcio);
        metas[3].LinhaDeProdutoId.Should().BeNull("CONSÓRCIO não é categoria de produto");
        metas[4].EmpresaId.Should().Be(_semente.RibeiraoPreto);

        var recusas = await db.MensagensDescartadas.AsNoTracking().Where(m => m.Fluxo == CargaDeMetasDaGestaoDeNegocios.Fluxo).ToListAsync();
        recusas.Should().HaveCount(2);
        recusas.Should().OnlyContain(r => !r.Conteudo.Contains(ConsultorComConta), "a fila guarda o id e o motivo, sem nome");

        var ponto = await db.PontosDeSincronismo.AsNoTracking().SingleAsync(p => p.Fluxo == CargaDeMetasDaGestaoDeNegocios.Fluxo);
        ponto.UltimoValor.Should().StartWith("2026-09-27T04:30:00", "o frescor é quando a GN gerou a resposta");
        (ponto.RegistrosLidos, ponto.RegistrosErro).Should().Be((7, 2));
    }

    [Fact]
    public async Task A_segunda_leitura_igual_grava_zero()
    {
        await Sincronizar();

        var segunda = await Sincronizar(quando: Agora.AddDays(1));

        segunda.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeNovas).Should().Be(0);
        segunda.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(0);
        segunda.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(0);
        segunda.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeIguais).Should().Be(5);

        await using var db = Sistema();
        (await db.MetasDeVenda.CountAsync(m => m.AlteradoEm != null)).Should().Be(0, "nenhuma meta foi regravada");
        (await db.AlteracoesDeCampo.CountAsync(a => a.Entidade == nameof(MetaDeVenda))).Should().Be(0,
            "a inclusão pela integração não entra na trilha, e nada mudou");
    }

    [Fact]
    public async Task A_revisao_da_meta_muda_a_linha_e_deixa_a_trilha_como_integracao()
    {
        await Sincronizar();

        var revisado = CadastroDeMetas();
        revisado[0] = Linha(1, quantidade: "5");
        var relatorio = await Sincronizar(revisado, quando: Agora.AddDays(1));

        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeRevisadas).Should().Be(1);

        await using var db = Sistema();
        var meta = await db.MetasDeVenda.AsNoTracking().SingleAsync(m => m.IdNaOrigem == 1);
        meta.Quantidade.Should().Be(5);
        meta.AtualizadaPelaOrigemEm.Should().Be(Agora.AddDays(1));

        var trilha = await db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == nameof(MetaDeVenda)).ToListAsync();
        trilha.Should().ContainSingle(a => a.Campo == nameof(MetaDeVenda.Quantidade) && a.ValorAnterior == "2" && a.ValorNovo == "5");
        trilha.Should().OnlyContain(a => a.Origem == OrigemDaOperacao.Integracao && a.SistemaId != null);
    }

    [Fact]
    public async Task A_meta_que_some_e_excluida_e_a_que_volta_e_reativada_na_mesma_linha()
    {
        await Sincronizar();
        long idDaLinha;
        await using (var db = Sistema()) idDaLinha = (await db.MetasDeVenda.AsNoTracking().SingleAsync(m => m.IdNaOrigem == 2)).Id;

        var semA2 = CadastroDeMetas().Where(l => l.Id != 2).ToList();
        (await Sincronizar(semA2, quando: Agora.AddDays(1))).Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(1);

        await using (var db = Sistema())
            (await db.MetasDeVenda.AsNoTracking().SingleAsync(m => m.IdNaOrigem == 2)).ExcluidoEm.Should().NotBeNull("excluída, nunca apagada");

        (await Sincronizar(quando: Agora.AddDays(2))).Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeReativadas).Should().Be(1);

        await using (var db = Sistema())
        {
            var voltou = await db.MetasDeVenda.AsNoTracking().SingleAsync(m => m.IdNaOrigem == 2);
            voltou.ExcluidoEm.Should().BeNull();
            voltou.Id.Should().Be(idDaLinha, "a volta é a mesma linha, com a trilha junto");
            (await db.AlteracoesDeCampo.CountAsync(a => a.Entidade == nameof(MetaDeVenda) && a.Campo == "ExcluidoEm")).Should().Be(2);
        }
    }

    [Fact]
    public async Task A_linha_recusada_nao_apaga_a_meta_que_ja_existia()
    {
        await Sincronizar();

        var comErro = CadastroDeMetas();
        comErro[0] = Linha(1, mes: "mês que não se lê");
        var relatorio = await Sincronizar(comErro, quando: Agora.AddDays(1));

        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(0, "a linha veio — só não passou no saneamento");
        await using var db = Sistema();
        var meta = await db.MetasDeVenda.AsNoTracking().SingleAsync(m => m.IdNaOrigem == 1);
        (meta.ExcluidoEm, meta.Competencia).Should().Be(((DateTime?)null, new DateOnly(2025, 11, 1)), "fica como estava");
    }

    [Fact]
    public async Task Remocao_de_25_por_cento_aborta_a_carga_inteira_e_nao_grava_nada()
    {
        await Sincronizar(CadastroGrande(120));

        // 30 DE 120 SOMEM (25%) E UMA MUDA: nem a mudança entra — a carga inteira é abortada.
        var parcial = CadastroGrande(90);
        parcial[0] = Linha(1, quantidade: "9");
        var resultado = await Sincronia(DaCarga, parcial, Agora.AddDays(1), _semente.Operador).ExecutarAsync(false, false, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("30 de 120").And.Contain("ABORTADA").And.Contain("--aceitar-remocao");

        await using var db = Sistema();
        (await db.MetasDeVenda.CountAsync(m => m.ExcluidoEm == null)).Should().Be(120);
        (await db.MetasDeVenda.AsNoTracking().SingleAsync(m => m.IdNaOrigem == 1)).Quantidade.Should().Be(2);
    }

    [Fact]
    public async Task Remocao_de_10_por_cento_exclui_e_aceitar_remocao_passa_pela_trava()
    {
        await Sincronizar(CadastroGrande(120));

        (await Sincronizar(CadastroGrande(108), quando: Agora.AddDays(1))).Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(12);

        var aceita = await Sincronizar(CadastroGrande(50), aceitarRemocao: true, quando: Agora.AddDays(2));
        aceita.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeExcluidas).Should().Be(58);
        aceita.Observacoes.Should().ContainSingle(o => o.Contains("--aceitar-remocao", StringComparison.Ordinal));

        await using var db = Sistema();
        (await db.MetasDeVenda.CountAsync(m => m.ExcluidoEm == null)).Should().Be(50);
    }

    [Fact]
    public async Task A_simulacao_nao_grava_nada()
    {
        var relatorio = await Sincronizar(simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDeMetasDaGestaoDeNegocios.RotuloDeNovas).Should().Be(5);

        await using var db = Sistema();
        (await db.MetasDeVenda.CountAsync()).Should().Be(0);
        (await db.MensagensDescartadas.CountAsync()).Should().Be(0);
        (await db.PontosDeSincronismo.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task O_cadastro_vazio_nao_exclui_nada()
    {
        await Sincronizar();

        var resultado = await Sincronia(DaCarga, [], Agora.AddDays(1), _semente.Operador).ExecutarAsync(false, false, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("veio vazio");
        await using var db = Sistema();
        (await db.MetasDeVenda.CountAsync(m => m.ExcluidoEm == null)).Should().Be(5);
    }
}
