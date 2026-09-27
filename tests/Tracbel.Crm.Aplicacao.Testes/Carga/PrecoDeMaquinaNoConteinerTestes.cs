using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoParque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O PREÇO DA MÁQUINA PELA NOTA NO SQL SERVER DE VERDADE (issue 70, D-P12) — o contêiner de desenvolvimento, com a cadeia
/// inteira de migrações aplicada.
///
/// <para><b>O que só o motor de verdade prova:</b> que a categoria sai da classificação de produto que a MIGRAÇÃO semeou
/// (a linha "TRATOR MÉDIO" do ART vira TRATOR pelo de-para de colação binária, cruzado em memória); os CHECKs da tabela
/// nova; o índice único por categoria e mês, que a segunda rodada respeita sem gravar de novo; e a rotina 10 semeada.</para>
///
/// <para><b>Como rodar:</b> suba o contêiner (<c>./scripts/banco/subir-banco.ps1</c>) e rode <c>dotnet test</c>. Sem
/// servidor, o teste é IGNORADO com a razão dita em voz alta.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class PrecoDeMaquinaNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmPrecoDeMaquinaTeste";

    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
    private const string Ribeirao = "010101";

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    /// <summary>
    /// O CENÁRIO: além da venda do parque (sem filial de faturamento), dois tratores faturados por Ribeirão em março de
    /// 2026 — um médio e um grande, as duas linhas do ART que viram TRATOR —, um usado e uma venda direta.
    /// </summary>
    private static SementeDoParque Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        var semente = Semear(db, forcarIdentificadores: false);

        var art = db.Sistemas.Single(s => s.Codigo == LeitorDoArt.CodigoDoSistema);
        var comprador = semente.Clientes[CpfA];

        void Vender(string chave, string chassi, string nota, DateOnly faturada, string linha, bool direta = false)
        {
            var maquina = Equipamento.RegistrarPelaIntegracao(semente.RibeiraoPreto, Chassi.Criar(chassi), OrigemDoEquipamento.Art, semente.Operador);
            db.Equipamentos.Add(maquina);
            db.SaveChanges();

            db.VendasDeMaquina.Add(VendaDeMaquina.Registrar(art.Id, chave, maquina.Id, comprador, new DadosDaVendaNaOrigem(
                    semente.RibeiraoPreto, semente.RibeiraoPreto, faturada.AddDays(-3), faturada, null, null, "P-" + chave, nota, "P",
                    "Varejo", direta, false, 1, linha, "PRODUTO DE TESTE", "Agro Norte", "Ribeirão Preto", "Ribeirão Preto",
                    "hash-" + chave, null),
                Agora, semente.Operador));
            db.SaveChanges();
        }

        Vender("7001", "1RW6110JCMR700001", "48351", new DateOnly(2026, 3, 10), "TRATOR MÉDIO");
        Vender("7002", "1RW8R340CMR700002", "48400", new DateOnly(2026, 3, 20), "TRATOR GRANDE");
        Vender("7003", "1RW6110JCMR700003", "48500", new DateOnly(2026, 3, 22), "USADOS");
        Vender("7004", "1RW6110JCMR700004", "90000", new DateOnly(2026, 3, 25), "TRATOR MÉDIO", direta: true);

        return semente;
    }

    /// <summary>As notas do Protheus: uma para cada venda, com os zeros à esquerda que a SD2 guarda.</summary>
    private static List<ItemDeMaquinaNaNota> Notas() =>
    [
        new(Ribeirao, "1", "000048351", "01", new DateOnly(2026, 3, 8), 1m, 400_000m),
        new(Ribeirao, "1", "000048400", "01", new DateOnly(2026, 3, 19), 1m, 600_000m),
        new(Ribeirao, "1", "000048500", "01", new DateOnly(2026, 3, 21), 1m, 150_000m),
        new(Ribeirao, "1", "000090000", "01", new DateOnly(2026, 3, 24), 1m, 999_000m)
    ];

    private static async Task<RelatorioDoPrecoDeMaquina> Carregar(SementeDoParque semente, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var carga = new CargaDePrecoDeMaquina(
            Abrir,
            (_, _) => Task.FromResult(Resultado<IReadOnlyList<ItemDeMaquinaNaNota>>.Ok(Notas())),
            semente.Operador,
            () => Agora,
            _ => { });

        var resultado = await carga.ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private static string Rotulo(DesfechoDoCasamento desfecho) => CargaDePrecoDeMaquina.RotuloDoDesfecho[desfecho];

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_os_dois_tratores_viram_a_mediana_de_marco_e_a_segunda_rodada_nao_grava_nada()
    {
        var semente = Recriar();

        // A SIMULAÇÃO CONTA TUDO E NÃO GRAVA.
        var simulada = await Carregar(semente, simular: true);
        simulada.Simulada.Should().BeTrue();
        simulada.Valor(Rotulo(DesfechoDoCasamento.Casada)).Should().Be(2);
        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
            (await db.PrecosDeMaquina.CountAsync()).Should().Be(0, "a simulação não grava");

        var primeira = await Carregar(semente);
        primeira.Valor(CargaDePrecoDeMaquina.VendasLidas).Should().Be(5);
        primeira.Valor(CargaDePrecoDeMaquina.VendasDiretas).Should().Be(1, "a nota da venda direta é da fábrica");
        primeira.Valor(Rotulo(DesfechoDoCasamento.Casada)).Should().Be(2);
        primeira.Valor(Rotulo(DesfechoDoCasamento.SemCategoria)).Should().Be(1, "usado não é preço de máquina nova");
        primeira.Valor(Rotulo(DesfechoDoCasamento.SemNota)).Should().Be(1, "a venda do parque não diz a filial que faturou");
        primeira.Valor(CargaDePrecoDeMaquina.MesesNovos).Should().Be(1);

        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            var trator = await db.CategoriasDeMaquina.SingleAsync(c => c.Codigo == "TRATOR");
            var marco = await db.PrecosDeMaquina.SingleAsync();

            // TRATOR MÉDIO E TRATOR GRANDE SÃO A MESMA CATEGORIA — a do de-para que a migração semeou.
            marco.CategoriaDeMaquinaId.Should().Be(trator.Id);
            marco.Mes.Should().Be(new DateOnly(2026, 3, 1));
            marco.Mediana.Should().Be(500_000m, "a mediana de 400 mil e 600 mil");
            marco.Menor.Should().Be(400_000m);
            marco.Maior.Should().Be(600_000m);
            marco.Notas.Should().Be(2);
            marco.Fonte.Should().Be("PROTHEUS.SD2");

            (await db.Rotinas.SingleAsync(r => r.Id == 10)).Codigo.Should().Be(RotinasDoSistema.PrecosDeMaquina);
        }

        var segunda = await Carregar(semente);
        segunda.Valor(CargaDePrecoDeMaquina.MesesNovos).Should().Be(0);
        segunda.Valor(CargaDePrecoDeMaquina.MesesRevisados).Should().Be(0);
        segunda.Valor(CargaDePrecoDeMaquina.MesesMantidos).Should().Be(1, "sem mudança na origem, o mês não é tocado");
    }
}
