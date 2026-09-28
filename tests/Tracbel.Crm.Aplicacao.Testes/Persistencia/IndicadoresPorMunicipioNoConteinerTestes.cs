using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Testes.Carga;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// OS INDICADORES DE MERCADO MUNICÍPIO A MUNICÍPIO NO SQL SERVER DE VERDADE (issue 257, Diagnóstico Comercial).
///
/// <para><b>O que só o motor de verdade prova:</b> que o SICOR agrupado por município — contagens e somas condicionais
/// dentro do grupo — é traduzido e dá, para cada município, EXATAMENTE o índice que a leitura de um município só daria.
/// A leitura em lote existe para não pagar uma ida ao banco por município; se ela divergisse da de um só, o Diagnóstico
/// mostraria um crédito e a calculadora outro, para o mesmo município e a mesma data.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class IndicadoresPorMunicipioNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmIndicadoresPorMunicipioTeste";

    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoje = new(2026, 9, 27);

    // Três municípios: um com crédito nas duas janelas, um só na recente (sem base de comparação) e um sem linha nenhuma.
    private static readonly (int Ibge, int Bcb, string Nome)[] Municipios =
    [
        (3597101, 9101, "Município de Teste X"), (3597102, 9102, "Município de Teste Y"), (3597103, 9103, "Município de Teste Z")
    ];

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    private static void Recriar()
    {
        SqlConnection.ClearAllPools();
        SementeDoParque semente;
        using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            semente = CenarioDoParque.Semear(db, forcarIdentificadores: false);
        }

        // A LINHA DO SICOR É AUDITADA, e a trilha recusa gravação sem filial: grava pelo contexto da carga.
        using var carga = new CrmDbContext(Opcoes(), new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));
        var municipios = Municipios.Select(m => Municipio.Criar(m.Nome, "SP", m.Ibge)).ToList();
        carga.Municipios.AddRange(municipios);
        carga.SaveChanges();

        CreditoRuralDeInvestimento Linha(int indice, DateOnly mes, decimal valor) =>
            CreditoRuralDeInvestimento.Registrar(
                new CreditoRuralDeInvestimento.Chave(Municipios[indice].Bcb, (short)mes.Year, (byte)mes.Month, 7080, 154, 71, 431, 9, 1, 14),
                municipios[indice].Id, valor, 0m, semente.Operador, Agora);

        var ultimo = new DateOnly(2026, 6, 1);
        carga.CreditosRuraisDeInvestimento.AddRange(
            // X: quatro linhas recentes contra duas anteriores, e o valor por linha subindo.
            Linha(0, ultimo, 150_000m), Linha(0, ultimo.AddMonths(-2), 120_000m), Linha(0, ultimo.AddMonths(-5), 90_000m),
            Linha(0, ultimo.AddMonths(-9), 80_000m), Linha(0, ultimo.AddMonths(-13), 100_000m), Linha(0, ultimo.AddMonths(-20), 60_000m),
            // Y: só a janela recente — o índice não sai, com o motivo.
            Linha(1, ultimo.AddMonths(-1), 200_000m),
            // CUSTEIO NÃO É MÁQUINA: não pode entrar em índice nenhum.
            CreditoRuralDeInvestimento.Registrar(
                new CreditoRuralDeInvestimento.Chave(Municipios[0].Bcb, 2026, 6, 1000, 154, 71, 431, 9, 1, 14),
                municipios[0].Id, 9_000_000m, 0m, semente.Operador, Agora));

        // A PERCEPÇÃO DE X, vigente hoje — ela vem município a município na leitura em lote.
        carga.PercepcoesDoGestor.Add(PercepcaoDoGestor.Informar(
            municipios[0].Id, 2.5m, 5m, Hoje, "teste do lote", semente.Operador, Agora));
        carga.SaveChanges();
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_leitura_em_lote_da_o_mesmo_indice_que_a_de_um_municipio_so()
    {
        Recriar();

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var repositorio = new RepositorioDeIndicadoresDeMercado(db);
        var codigos = Municipios.Select(m => m.Ibge).ToList();

        var lote = await repositorio.LerPorMunicipioAsync(Hoje, codigos, CancellationToken.None);

        foreach (var codigo in codigos)
        {
            var sozinho = await repositorio.LerAsync(Hoje, codigo, CancellationToken.None);
            lote.CreditoPorMunicipio[codigo].Should().BeEquivalentTo(sozinho.Credito, $"o município {codigo} lido sozinho");
            lote.PercepcaoPorMunicipio.GetValueOrDefault(codigo).Should().Be(sozinho.PercepcaoDoGestor ?? 0m);
        }

        var x = lote.CreditoPorMunicipio[Municipios[0].Ibge];
        x.Indice.Should().NotBeNull();
        x.Linhas.Should().Be(4, "o custeio de R$ 9 milhões não é máquina");
        x.LinhasAnteriores.Should().Be(2);

        lote.CreditoPorMunicipio[Municipios[1].Ibge].Indice.Should().BeNull("sem janela anterior não há do que variar");
        lote.CreditoPorMunicipio[Municipios[2].Ibge].Indice.Should().BeNull();
        lote.PercepcaoPorMunicipio.Should().ContainKey(Municipios[0].Ibge).WhoseValue.Should().Be(2.5m);
        lote.PercepcaoPorMunicipio.Should().NotContainKey(Municipios[1].Ibge);
    }
}
