using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A ÚLTIMA LEITURA COMPLETA DE UMA CARGA (documento 54, passo 6) — o ponto de sincronismo próprio da completa, com o instante
/// da leitura no "até onde leu", pelo relógio da carga.
/// </summary>
public sealed class RodadaCompletaTestes : IDisposable
{
    private const string Fluxo = "PROTHEUS.TESTE_COMPLETO";

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;

    public RodadaCompletaTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Banco();
        db.Database.EnsureCreated();
        db.Sistemas.Add(Sistema.Criar("PROTHEUS", "Protheus", "SQL Server"));
        db.SaveChanges();
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Banco() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    [Fact]
    public async Task Sem_ponto_da_completa_a_ultima_completa_e_nula()
    {
        await using var db = Banco();
        (await RodadaCompleta.LerAsync(db, Fluxo, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task A_completa_registrada_devolve_o_instante_da_leitura_e_a_seguinte_o_substitui()
    {
        var domingo = new DateTime(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);
        var outroDomingo = domingo.AddDays(7);

        await using (var db = Banco())
        {
            var sistema = await db.Sistemas.SingleAsync();
            await RodadaCompleta.RegistrarAsync(db, sistema.Id, Fluxo, domingo, lidos: 10, gravados: 2, CancellationToken.None);
        }

        await using (var db = Banco())
            (await RodadaCompleta.LerAsync(db, Fluxo, CancellationToken.None)).Should().Be(domingo);

        await using (var db = Banco())
        {
            var sistema = await db.Sistemas.SingleAsync();
            await RodadaCompleta.RegistrarAsync(db, sistema.Id, Fluxo, outroDomingo, lidos: 11, gravados: 0, CancellationToken.None);
        }

        await using var leitura = Banco();
        (await RodadaCompleta.LerAsync(leitura, Fluxo, CancellationToken.None)).Should().Be(outroDomingo);
        (await leitura.PontosDeSincronismo.CountAsync(p => p.Fluxo == Fluxo)).Should().Be(1);
    }
}
