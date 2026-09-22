using System.Globalization;
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
/// NUNCA APAGAR HISTÓRICO NAS FONTES DE MERCADO (issue 153): o que a carga muda ou apaga fica na trilha de
/// auditoria, com o valor anterior — e a usina que sai da lista da ANP fica encerrada, com a data.
///
/// <para>SQLite em memória com o modelo de verdade e o contexto da carga (usuário e filial reais, origem
/// "Integração"), como a carga roda no servidor. É a trilha do banco, não um dublê.</para>
/// </summary>
public sealed class HistoricoDasFontesNaTrilhaTestes : IDisposable
{
    private const long Carga = 100;
    private static readonly DateTime Agora = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly int _municipioId;
    private readonly int _sistemaId;

    public HistoricoDasFontesNaTrilhaTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureCreated();

        var empresa = Empresa.Criar("010101", "Ribeirão Preto");
        db.Entry(empresa).Property(e => e.Id).CurrentValue = 1;
        db.Empresas.Add(empresa);

        var usuario = Usuario.Criar(Guid.NewGuid(), "carga@tracbel.com.br", "Carga", "carga", Email.Criar("carga@tracbel.com.br"), 1, criadoPorId: 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = Carga;
        db.Usuarios.Add(usuario);

        var municipio = Municipio.Criar("Franca", "SP", 3516200);
        var sistema = Sistema.Criar("IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura");
        db.Municipios.Add(municipio);
        db.Sistemas.Add(sistema);
        db.SaveChanges();

        _municipioId = municipio.Id;
        _sistemaId = sistema.Id;
    }

    public void Dispose() => _conexao.Dispose();

    /// <summary>O contexto como a carga o abre: usuário e filial reais, origem declarada.</summary>
    private CrmDbContext DaCarga()
    {
        var db = new CrmDbContext(_opcoes, new ContextoDeCargaDeSistema(Carga, 1, new HashSet<int> { 1 }));
        db.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, _sistemaId);
        return db;
    }

    private List<AlteracaoDeCampo> Trilha(string entidade)
    {
        using var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia);
        return [.. db.AlteracoesDeCampo.AsNoTracking().Where(a => a.Entidade == entidade)];
    }

    private static decimal Numero(string? texto) => decimal.Parse(texto!, CultureInfo.InvariantCulture);

    [Fact]
    public void A_revisao_da_PAM_fica_na_trilha_com_o_antes_e_o_depois_e_a_inclusao_pela_carga_nao()
    {
        long id;
        using (var db = DaCarga())
        {
            var linha = ProducaoAgricolaNoMunicipio.Registrar(_municipioId, 2024, 40139, "Café (em grão) Total", new(70m, 60m, 200m, 900m), Carga, Agora);
            db.ProducoesAgricolasNosMunicipios.Add(linha);
            db.SaveChanges();
            id = linha.Id;
        }

        Trilha(nameof(ProducaoAgricolaNoMunicipio)).Should().BeEmpty(
            "a inclusão pela carga já tem o rastro da origem; repetir cada campo multiplicaria a trilha por nada");

        // O IBGE REVISA a área plantada e o valor do ano anterior quando publica o novo.
        using (var db = DaCarga())
        {
            db.ProducoesAgricolasNosMunicipios.Single(p => p.Id == id)
                .Reapurar("Café (em grão) Total", new(72m, 60m, 200m, 950m), Carga, Agora.AddMonths(1));
            db.SaveChanges();
        }

        var trilha = Trilha(nameof(ProducaoAgricolaNoMunicipio));
        trilha.Select(t => t.Campo).Should().BeEquivalentTo(["AreaPlantadaHectares", "ValorDaProducaoMilReais"],
            "só o que mudou entra — a colhida e a quantidade continuaram iguais");

        var area = trilha.Single(t => t.Campo == "AreaPlantadaHectares");
        Numero(area.ValorAnterior).Should().Be(70m);
        Numero(area.ValorNovo).Should().Be(72m);
        area.Operacao.Should().Be(OperacaoAuditada.Alteracao);
        area.Origem.Should().Be(OrigemDaOperacao.Integracao);
        area.SistemaId.Should().Be(_sistemaId);
        area.AlteradoPorId.Should().Be(Carga);
        area.RegistroId.Should().Be(id);
    }

    [Fact]
    public void A_revisao_do_Censo_tambem_fica_na_trilha()
    {
        long id;
        using (var db = DaCarga())
        {
            var frota = FrotaDeTratoresNoMunicipio.Registrar(_municipioId, 2017, 113521, "Total", 172, 450, Carga, Agora);
            db.FrotasDeTratoresNosMunicipios.Add(frota);
            db.SaveChanges();
            id = frota.Id;
        }

        using (var db = DaCarga())
        {
            db.FrotasDeTratoresNosMunicipios.Single(f => f.Id == id).Reapurar("Total", 172, 452, Carga, Agora.AddMonths(1));
            db.SaveChanges();
        }

        var tratores = Trilha(nameof(FrotaDeTratoresNoMunicipio)).Single();
        tratores.Campo.Should().Be("Tratores");
        tratores.ValorAnterior.Should().Be("450");
        tratores.ValorNovo.Should().Be("452");
    }

    [Fact]
    public void A_linha_do_SICOR_que_sai_da_fonte_fica_na_trilha_com_a_chave_inteira()
    {
        // A MESMA OPERAÇÃO RECLASSIFICADA pelo Banco Central (fonte 431 virou 430): a linha antiga sai, senão seria
        // contada duas vezes. Com só o valor na trilha, a exclusão não dizia qual linha era.
        var chave = new CreditoRuralDeInvestimento.Chave(6482, 2025, 8, 7080, 154, 71, 431, 9, 1, 14);
        long id;
        using (var db = DaCarga())
        {
            var linha = CreditoRuralDeInvestimento.Registrar(chave, _municipioId, 500_000m, 0m, Carga, Agora);
            db.CreditosRuraisDeInvestimento.Add(linha);
            db.SaveChanges();
            id = linha.Id;
        }

        using (var db = DaCarga())
        {
            db.CreditosRuraisDeInvestimento.Remove(db.CreditosRuraisDeInvestimento.Single(c => c.Id == id));
            db.SaveChanges();
        }

        var trilha = Trilha(nameof(CreditoRuralDeInvestimento));
        trilha.Should().OnlyContain(t => t.Operacao == OperacaoAuditada.Exclusao && t.ValorNovo == null && t.RegistroId == id);
        trilha.Select(t => t.Campo).Should().Contain(["Valor", "MunicipioId", "Ano", "Mes", "CodigoProduto", "CodigoPrograma", "CodigoFonte", "CodigoModalidade"]);
        trilha.Single(t => t.Campo == "CodigoFonte").ValorAnterior.Should().Be("431");
        trilha.Single(t => t.Campo == "CodigoProduto").ValorAnterior.Should().Be("7080");
        Numero(trilha.Single(t => t.Campo == "Valor").ValorAnterior).Should().Be(500_000m);
    }

    [Fact]
    public void A_usina_que_sai_da_lista_fica_encerrada_e_a_trilha_guarda_a_saida_e_a_volta()
    {
        long id;
        using (var db = DaCarga())
        {
            var usina = UsinaDeEtanol.Registrar("11111111000111", "USINA DE TESTE S/A", _municipioId, new DateOnly(2026, 7, 1), 700, 1_300, Carga, Agora);
            db.UsinasDeEtanol.Add(usina);
            db.SaveChanges();
            id = usina.Id;
        }

        // A leitura de agosto não traz a usina.
        using (var db = DaCarga())
        {
            UsinaDeEtanol.EncerrarAsQueSairam(db.UsinasDeEtanol.ToList(), new HashSet<string> { "99999999000199" }, Carga, Agora.AddMonths(1))
                .Should().ContainSingle();
            db.SaveChanges();
        }

        using (var db = new CrmDbContext(_opcoes, ProvedorDeContextoDeSistema.Instancia))
            db.UsinasDeEtanol.Single(u => u.Id == id).EncerradaEm.Should().NotBeNull("encerrada, e não apagada");

        // A de setembro traz de novo.
        using (var db = DaCarga())
        {
            var usina = db.UsinasDeEtanol.Single(u => u.Id == id);
            usina.Reapurar(usina.RazaoSocial, usina.MunicipioId, new DateOnly(2026, 8, 1), 700, 1_300, Carga, Agora.AddMonths(2));
            db.SaveChanges();
        }

        var saida = Trilha(nameof(UsinaDeEtanol)).Where(t => t.Campo == "EncerradaEm").OrderBy(t => t.Id).ToList();
        saida.Should().HaveCount(2);
        saida[0].ValorAnterior.Should().BeNull();
        saida[0].ValorNovo.Should().StartWith("2026-10-22", "saiu na leitura de um mês depois");
        saida[1].ValorAnterior.Should().StartWith("2026-10-22");
        saida[1].ValorNovo.Should().BeNull("voltou à lista e reabriu");
        Trilha(nameof(UsinaDeEtanol)).Should().NotContain(t => t.Campo == "MesDeReferencia",
            "o mês anda todo mês em todas as usinas — ficaria na trilha como carimbo");
    }
}
