using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoFunilDoVortice;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A ROTINA DO FUNIL NO SQL SERVER DE VERDADE — a cadeia inteira de migrações aplicada, num banco próprio
/// (<c>TracbelCrmFunilCargaTeste</c>): outro worktree rodando os testes de contêiner ao mesmo tempo não derruba este.
///
/// <para><b>O que só o motor prova:</b> que a gravação em blocos respeita os CHECKs (a herança com o processo DNA, o
/// papel com a principal, o formulário da lista), o índice único (processo, estágio), a identidade das tabelas e a
/// tradução das consultas com lista de identificadores; e que a segunda rodada, relida do banco — com o
/// <c>datetime2(3)</c> que corta a data no milissegundo —, não altera nada.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class CargaDoFunilNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmFunilCargaTeste";

    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_primeira_rodada_grava_o_funil_e_a_segunda_nao_altera_nada()
    {
        SementeDoFunil semente;
        SqlConnection.ClearAllPools();
        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            semente = Semear(db);
        }

        CrmDbContext Abrir() => new(Opcoes(), new ContextoDeCargaDeSistema(
            semente.Operador, semente.RibeiraoPreto, semente.DeParaDeFiliais.Values.ToHashSet()));

        // DATA DO VÓRTICE FORA DO MILISSEGUNDO (passo de 1/300 s): a coluna corta, e a segunda rodada precisa bater.
        var historico = Historico().Select(h => h with { RealizadoEmUtc = h.RealizadoEmUtc.AddTicks(33_333) }).ToList();

        var primeira = await Rotina(Abrir, semente, Funil(historico: historico), Respostas(), Agora).ExecutarAsync(false, false, CancellationToken.None);
        primeira.EhSucesso.Should().BeTrue(primeira.Erro);
        primeira.Valor.Valor(CargaDoFunilDoVortice.RotuloDeLinhasIncluidas).Should().Be(LinhasDoFunil);

        var segunda = await Rotina(Abrir, semente, Funil(historico: historico), Respostas(), Agora.AddDays(1)).ExecutarAsync(false, false, CancellationToken.None);
        segunda.EhSucesso.Should().BeTrue(segunda.Erro);
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDeLinhasIncluidas).Should().Be(0);
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDeLinhasAlteradas).Should().Be(0, "a data relida do banco bate com a da origem, cortada");
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDeLinhasRemovidas).Should().Be(0);
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDeVendasPerdidasAlteradas).Should().Be(0);
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDePapeisAlterados).Should().Be(0);

        await using var leitura = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await leitura.EstagiosDoProcesso.CountAsync()).Should().Be(LinhasDoFunil);
        (await leitura.EstagiosDoProcesso.CountAsync(e => e.HerdadoDoProcessoDna)).Should().Be(3, "Lead, Qualificado e Cobertura do filho vêm do pai");
        (await leitura.VendasPerdidas.CountAsync(v => v.Papel == PapelDaVendaPerdida.Duplicata)).Should().Be(2);
        (await leitura.VendasPerdidas.CountAsync(v => v.Papel == PapelDaVendaPerdida.Complemento)).Should().Be(1);
        (await leitura.RegistrosDeOrigem.CountAsync(r => r.Fluxo == CargaDoFunilDoVortice.Fluxo && r.Leituras == 2))
            .Should().Be(await leitura.RegistrosDeOrigem.CountAsync(r => r.Fluxo == CargaDoFunilDoVortice.Fluxo));
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_mais_de_2100_vendas_perdidas_nao_estouram_o_limite_de_parametros()
    {
        // O VÓRTICE TEM ~3,2 MIL RESPOSTAS, e o SQL Server aceita 2.100 parâmetros por comando. As consultas por lista de
        // identificadores (as vendas perdidas já gravadas, as do de-para) passam por aqui com 2.500 — nas duas rodadas.
        SementeDoFunil semente;
        SqlConnection.ClearAllPools();
        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            semente = Semear(db);
        }

        CrmDbContext Abrir() => new(Opcoes(), new ContextoDeCargaDeSistema(
            semente.Operador, semente.RibeiraoPreto, semente.DeParaDeFiliais.Values.ToHashSet()));

        var muitas = Respostas();
        for (var i = 0; i < 2_500; i++)
            muitas.Add(R(10_000 + i, FormulariosDaVendaPerdida.MaqImp, Em(2024, 3, 1), 2000 + (i % ProspectsDeEnchimento),
                102_000 + i, empresaDoProcesso: 1, motivo: "PRECO"));

        var primeira = await Rotina(Abrir, semente, Funil(), muitas, Agora).ExecutarAsync(false, false, CancellationToken.None);
        primeira.EhSucesso.Should().BeTrue(primeira.Erro);
        primeira.Valor.Valor(CargaDoFunilDoVortice.RotuloDeVendasPerdidasIncluidas).Should().Be(6 + PerdasDeEnchimento + 2_500);

        var segunda = await Rotina(Abrir, semente, Funil(), muitas, Agora.AddDays(1)).ExecutarAsync(false, false, CancellationToken.None);
        segunda.EhSucesso.Should().BeTrue(segunda.Erro);
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDeVendasPerdidasAlteradas).Should().Be(0);
        segunda.Valor.Valor(CargaDoFunilDoVortice.RotuloDeVendasPerdidasIncluidas).Should().Be(0);
    }
}
