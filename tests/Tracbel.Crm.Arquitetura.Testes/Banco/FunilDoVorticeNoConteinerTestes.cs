using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// AS MIGRAÇÕES DO FUNIL NO SQL SERVER DE VERDADE (documento 52): <c>EstagioDoProcessoDoVortice</c>,
/// <c>FormularioDaVendaPerdida</c> e <c>RotinaDosProcessosDoVortice</c>.
///
/// <para><b>O que só o motor prova:</b> a semente da classificação inteira gravada; o índice único (processo, estágio)
/// recusando a segunda linha; os CHECKs do tipo, do estágio, da herança, do formulário e do papel; o filtro global de
/// empresa chegando ao SQL da tabela nova; e as duas migrações voltando e indo de novo.</para>
///
/// <para><b>Banco próprio</b> (<c>TracbelCrmFunilTeste</c>): outro worktree rodando os testes de contêiner ao mesmo
/// tempo não derruba este, nem este derruba o dele.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class FunilDoVorticeNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmFunilTeste";

    private static readonly DateTime Em2024 = new(2024, 3, 1, 13, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    private static CrmDbContext DeSistema() => new(Opcoes(), new Provedor(Sistema()));

    private static (int A, int B) Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = DeSistema();
        db.Database.EnsureDeleted();
        db.Database.Migrate();

        db.Database.ExecuteSqlRaw(@"
            INSERT INTO organizacao.Empresa (ChavePublica, Codigo, Nome, Caminho, Nivel, EstaAtiva, CriadoEm) VALUES
            (NEWID(), '010101', N'Ribeirão Preto', '/', 0, 1, SYSUTCDATETIME()),
            (NEWID(), '010103', N'Barretos', '/', 0, 1, SYSUTCDATETIME());");

        var ids = db.Empresas.AsNoTracking().OrderBy(e => e.Codigo).Select(e => e.Id).ToList();
        return (ids[0], ids[1]);
    }

    private static RetratoDoEstagio Linha(int empresa, long numero, EstagioDoFunil estagio) => new(
        empresa, numero, 41, null, null, null, Em2024, false, SituacaoDoProcesso.Aberto, null, estagio, Em2024, null, null, 3239, false);

    [FatoSeHouverSqlServer]
    public void A_migracao_cria_as_duas_tabelas_e_grava_a_classificacao_inteira()
    {
        Recriar();
        using var db = DeSistema();

        var classificacao = db.ClassificacoesDeResultadoDoVortice.AsNoTracking().OrderBy(c => c.Id).ToList();
        classificacao.Select(c => (c.CodigoNaOrigem, c.Estagio, c.ContaComoContato, c.Fonte))
            .Should().Equal(ClassificacaoDeResultadoDoVortice.Semente.Select(i => (i.Codigo, i.Estagio, i.ContaComoContato, i.Fonte)));
        classificacao.Count(c => c.ContaComoContato).Should().Be(53);

        // A TABELA E A CONSTANTE DO ÚLTIMO CONTATO (PR #244) SÃO A MESMA LISTA — a carteira e o funil não divergem.
        classificacao.Where(c => c.ContaComoContato).Select(c => c.CodigoNaOrigem)
            .Should().BeEquivalentTo(Tracbel.Crm.Integracao.Vortice.LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato);
        classificacao.Single(c => c.CodigoNaOrigem == 2607).Estagio.Should().Be(EstagioDoFunil.Negociacao);

        db.EstagiosDoProcesso.Count().Should().Be(0);
        db.VendasPerdidas.Count().Should().Be(0);
    }

    [FatoSeHouverSqlServer]
    public void O_indice_unico_recusa_a_segunda_linha_do_mesmo_processo_no_mesmo_estagio()
    {
        var (a, _) = Recriar();

        using (var db = DeSistema())
        {
            db.EstagiosDoProcesso.Add(EstagioDoProcesso.Registrar(Linha(a, 1000, EstagioDoFunil.Cobertura)));
            db.EstagiosDoProcesso.Add(EstagioDoProcesso.Registrar(Linha(a, 1000, EstagioDoFunil.Pedido)));
            db.SaveChanges();
        }

        using var outro = DeSistema();
        outro.EstagiosDoProcesso.Add(EstagioDoProcesso.Registrar(Linha(a, 1000, EstagioDoFunil.Cobertura)));
        FluentActions.Invoking(() => outro.SaveChanges()).Should().Throw<DbUpdateException>()
            .Which.GetBaseException().Should().BeOfType<SqlException>().Which.Number.Should().BeOneOf(2601, 2627);
    }

    [FatoSeHouverSqlServer]
    public void Os_CHECKs_recusam_tipo_estagio_heranca_formulario_e_papel_fora_da_regra()
    {
        var (a, _) = Recriar();
        using var db = DeSistema();

        const string Colunas = "(EmpresaId, NumeroDoProcessoNaOrigem, TipoDeProcessoNaOrigem, AbertoEm, AberturaDeduzida, Desfecho, " +
                               "Estagio, AlcancadoEm, HerdadoDoProcessoDna, NumeroDoProcessoDnaNaOrigem, ResultadoQueAbriu, PelaEntradaDigital)";

        int Estagio(string valores)
        {
            var sql = $"INSERT INTO processo.EstagioDoProcesso {Colunas} VALUES ({a}, {valores})";
            return db.Database.ExecuteSqlRaw(sql);
        }

        Estagio("1, 41, '2024-03-01', 0, 'Aberto', 'Pedido', '2024-03-01', 0, NULL, 3239, 0").Should().Be(1, "a linha válida entra");

        foreach (var (valores, por) in new[]
                 {
                     ("2, 12, '2024-03-01', 0, 'Aberto', 'Pedido', '2024-03-01', 0, NULL, 3239, 0", "o tipo 12 não é do funil"),
                     ("3, 41, '2024-03-01', 0, 'Aberto', 'Proposta', '2024-03-01', 0, NULL, 3239, 0", "Proposta não é estágio"),
                     ("4, 41, '2024-03-01', 0, 'Aberto', 'Pedido', '2024-03-01', 1, NULL, 3239, 0", "herdado sem dizer de quem"),
                     ("5, 41, '2024-03-01', 0, 'Aberto', 'Pedido', '2024-03-01', 0, 99, 3239, 0", "o pai sem a herança"),
                     ("6, 41, '2024-03-01', 0, 'Vendido', 'Pedido', '2024-03-01', 0, NULL, 3239, 0", "Vendido não é desfecho")
                 })
            FluentActions.Invoking(() => Estagio(valores)).Should().Throw<SqlException>(por).Which.Number.Should().Be(547);

        db.Database.ExecuteSqlRaw(@"
            INSERT INTO processo.MotivoDePerda (Codigo, Nome, Categoria, ExigeConcorrente, ExigeObservacao, Ordem, EstaAtivo)
            VALUES ('NAO_INFORMADO_NA_ORIGEM', N'Não informado na origem', 'Outro', 0, 0, 100, 1);");

        int Venda(string formulario, string papel, string principal)
        {
            var sql = "INSERT INTO processo.VendaPerdida (ChavePublica, EmpresaId, RegistradaEm, MotivoDePerdaId, Quantidade, Participacao, " +
                      "CriadoEm, CriadoPorId, FormularioDeOrigem, Papel, VendaPerdidaPrincipalId) " +
                      $"VALUES (NEWID(), {a}, '2025-09-01', (SELECT Id FROM processo.MotivoDePerda), 1, 'NaoInformado', SYSUTCDATETIME(), 1, " +
                      $"{formulario}, {papel}, {principal})";
            return db.Database.ExecuteSqlRaw(sql);
        }

        Venda("'IV_Q_VENDA_PERDIDA_FY25'", "'Principal'", "NULL").Should().Be(1);
        Venda("NULL", "DEFAULT", "NULL").Should().Be(1, "a venda perdida que não veio do Vórtice nasce principal");

        foreach (var (formulario, papel, principal, por) in new[]
                 {
                     ("'IV_Q_VENDA_PERDIDA_MANITO'", "'Principal'", "NULL", "o _MANITO fica fora"),
                     ("'IV_Q_VP_TRATOR'", "'Complemento'", "NULL", "complemento sem principal"),
                     ("'IV_Q_VENDA_PERDIDA_FY25'", "'Principal'", "(SELECT MIN(Id) FROM processo.VendaPerdida)", "principal apontando outra"),
                     ("'IV_Q_VENDA_PERDIDA_JDE'", "'Gemeo'", "(SELECT MIN(Id) FROM processo.VendaPerdida)", "Gemeo não é papel")
                 })
            FluentActions.Invoking(() => Venda(formulario, papel, principal)).Should().Throw<SqlException>(por).Which.Number.Should().Be(547);

        Venda("'IV_Q_VENDA_PERDIDA_JDE'", "'Duplicata'", "(SELECT MIN(Id) FROM processo.VendaPerdida)").Should().Be(1);
    }

    [FatoSeHouverSqlServer]
    public void O_filtro_global_de_empresa_chega_a_tabela_do_funil()
    {
        var (a, b) = Recriar();

        using (var db = DeSistema())
        {
            db.EstagiosDoProcesso.AddRange(
                EstagioDoProcesso.Registrar(Linha(a, 1, EstagioDoFunil.Lead)),
                EstagioDoProcesso.Registrar(Linha(a, 1, EstagioDoFunil.Qualificado)),
                EstagioDoProcesso.Registrar(Linha(b, 2, EstagioDoFunil.Lead)));
            db.SaveChanges();
        }

        using var deRibeirao = new CrmDbContext(Opcoes(), new Provedor(new ContextoAcesso(
            usuarioId: 7, nomeExibicao: "vendedor", empresaId: a, empresasVisiveis: new HashSet<int> { a },
            subordinadosIds: new HashSet<long>(), equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(), ehServicoDeSistema: false)));

        deRibeirao.EstagiosDoProcesso.AsNoTracking().Select(e => e.EmpresaId).Distinct().ToList()
            .Should().Equal([a], "a linha de Barretos não chega a quem só vê Ribeirão Preto");
        deRibeirao.EstagiosDoProcesso.Count().Should().Be(2);
    }

    [FatoSeHouverSqlServer]
    public void A_migracao_da_rotina_a_insere_no_fim_desligada_e_a_conexao_do_Vortice_passa_a_citar_o_funil()
    {
        Recriar();
        using var db = DeSistema();

        var rotina = db.Rotinas.AsNoTracking().Single(r => r.Codigo == RotinasDoSistema.ProcessosVortice);
        var posicao = RotinasDoSistema.Todas.ToList().FindIndex(r => r.Codigo == RotinasDoSistema.ProcessosVortice);
        rotina.Id.Should().Be(posicao + 1, "o identificador semeado é a posição no catálogo, mais um");
        // ENTROU NO FIM QUANDO NASCEU (#247, Id 8). Depois dela veio a das metas (Id 9), por isso o teste não
        // exige mais que seja a última — só que o Id siga a posição no catálogo.
        rotina.Id.Should().Be(8, "a rotina do funil entrou no fim do catálogo em 27/09/2026, antes da das metas");
        rotina.EstaLigada.Should().BeFalse();
        rotina.Agenda.Should().Be(AgendaDaRotina.DiariaAs(new TimeOnly(6, 30)));

        db.Conexoes.AsNoTracking().Single(c => c.Id == 4).Descricao.Should().Contain("funil");

        // E A MIGRAÇÃO VOLTA: o Down tira a rotina e devolve a descrição — e só isso.
        var migrador = db.GetService<IMigrator>();
        migrador.Migrate("FormularioDaVendaPerdida");
        db.Rotinas.AsNoTracking().Any(r => r.Codigo == RotinasDoSistema.ProcessosVortice).Should().BeFalse();
        db.Conexoes.AsNoTracking().Single(c => c.Id == 4).Descricao.Should().NotContain("funil");
        migrador.Migrate();
    }

    [FatoSeHouverSqlServer]
    public void As_duas_migracoes_voltam_e_vao_de_novo()
    {
        Recriar();
        using var db = DeSistema();
        var migrador = db.GetService<IMigrator>();

        migrador.Migrate("ColhedoraDeCanaERegrasDasOutrasCategorias");
        Tabelas(db).Should().NotContain(["processo.EstagioDoProcesso", "integracao.ClassificacaoDeResultadoDoVortice"]);
        Colunas(db, "VendaPerdida").Should().NotContain(["FormularioDeOrigem", "Papel", "VendaPerdidaPrincipalId", "NumeroDoProcessoNaOrigem"]);

        migrador.Migrate();
        Tabelas(db).Should().Contain(["processo.EstagioDoProcesso", "integracao.ClassificacaoDeResultadoDoVortice"]);
        Colunas(db, "VendaPerdida").Should().Contain(["FormularioDeOrigem", "Papel", "VendaPerdidaPrincipalId", "NumeroDoProcessoNaOrigem"]);
    }

    // -----------------------------------------------------------------------------------------

    private static List<string> Tabelas(CrmDbContext db) =>
        db.Database.SqlQueryRaw<string>("SELECT s.name + '.' + t.name AS Value FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id").ToList();

    private static List<string> Colunas(CrmDbContext db, string tabela) =>
        db.Database.SqlQueryRaw<string>(
            "SELECT c.name AS Value FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id WHERE t.name = {0}", tabela).ToList();

    private static ContextoAcesso Sistema() => new(
        usuarioId: 0, nomeExibicao: "sistema", empresaId: 0, empresasVisiveis: new HashSet<int>(), subordinadosIds: new HashSet<long>(),
        equipesIds: new HashSet<long>(), profundidades: new Dictionary<string, Profundidade>(), ehServicoDeSistema: true);

    private sealed class Provedor(ContextoAcesso acesso) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = acesso;
    }
}
