using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoParque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O PARQUE PELO PROPRIETÁRIO ATUAL NO SQL SERVER DE VERDADE — o contêiner de desenvolvimento, com a cadeia inteira de
/// migrações aplicada.
///
/// <para><b>O que só o motor de verdade prova:</b> o índice único FILTRADO de um dono atual vigente por máquina — é ele
/// que obriga a troca de dono a encerrar o anterior antes de abrir o novo; os CHECKs novos (a natureza, a evidência que
/// só o dono atual tem, o modelo que falta na máquina do Protheus); o chassi curto que volta do banco pelo conversor; e
/// a tradução das consultas com lista de identificadores (<c>OPENJSON</c>).</para>
///
/// <para><b>Como rodar:</b> suba o contêiner (<c>./scripts/banco/subir-banco.ps1</c>) e rode <c>dotnet test</c>. Sem
/// servidor, o teste é IGNORADO com a razão dita em voz alta.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class ParqueDoProtheusNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmParqueTeste";

    private static readonly DateTime Agora = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    private static SementeDoParque Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        return Semear(db, forcarIdentificadores: false);
    }

    private static async Task<RelatorioDoParqueDoProtheus> Sincronizar(
        SementeDoParque semente, IReadOnlyList<Integracao.Protheus.MaquinaNoProtheus> parque, DateTime quando, bool simular = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var resultado = await Sincronia(Abrir, parque, quando, semente).ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_sincronia_cria_mantem_e_troca_o_dono_sem_tropecar_no_indice_filtrado()
    {
        var semente = Recriar();

        var primeira = await Sincronizar(semente, Parque(), Agora);
        primeira.Valor(CargaDoParqueDoProtheus.RotuloDeMaquinasCriadas).Should().Be(2);
        primeira.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAbertos).Should().Be(3);
        primeira.Valor(CargaDoParqueDoProtheus.RotuloDeConfirmados).Should().Be(1);

        var segunda = await Sincronizar(semente, Parque(), Agora.AddDays(1));
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosAbertos).Should().Be(0, "sem mudança na origem, nada é gravado");
        segunda.Valor(CargaDoParqueDoProtheus.RotuloDeVinculosMantidos).Should().Be(3);

        // O DONO MUDA DUAS VEZES: o índice único aceita um vigente por máquina, e o histórico cresce ao lado.
        var paraC = Parque();
        paraC[0] = Maquina("000001", ChassiNovo1Cq, CpfC, ordem: new(2026, 8, 1), ordemDoDono: true);
        (await Sincronizar(semente, paraC, Agora.AddDays(2))).Valor(CargaDoParqueDoProtheus.RotuloDeVinculosEncerrados).Should().Be(1);

        var paraB = Parque();
        paraB[0] = Maquina("000001", ChassiNovo1Cq, CnpjB, ordem: new(2026, 9, 1), ordemDoDono: true);
        (await Sincronizar(semente, paraB, Agora.AddDays(3))).Valor(CargaDoParqueDoProtheus.RotuloDeVinculosEncerrados).Should().Be(1);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var maquina = MaquinaId(db, ChassiNovo1Cq);
        var donos = await db.VinculosComEquipamento.AsNoTracking()
            .Where(v => v.EquipamentoId == maquina && v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual)
            .OrderBy(v => v.Id).ToListAsync();
        donos.Should().HaveCount(3);
        donos.Count(v => v.EncerradoEm == null).Should().Be(1);
        donos[^1].ClienteId.Should().Be(semente.Clientes[CnpjB]);

        var serie = db.Equipamentos.AsNoTracking().AsEnumerable().Single(e => e.Chassi.Numero == SerieCurta);
        serie.Chassi.EhVin.Should().BeFalse("o número de série confirmado volta do banco pelo conversor");
        serie.ModeloId.Should().BeNull("a máquina do Protheus sem código idêntico no catálogo fica sem modelo — e o CHECK aceita");

        (await db.RegistrosDeOrigem.CountAsync(r => r.Fluxo == CargaDoParqueDoProtheus.Fluxo)).Should().Be(10);
    }

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_simulacao_nao_grava_nada()
    {
        var semente = Recriar();

        var relatorio = await Sincronizar(semente, Parque(), Agora, simular: true);
        relatorio.Valor(CargaDoParqueDoProtheus.RotuloDeMaquinasComDono).Should().Be(3);

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.Equipamentos.CountAsync()).Should().Be(1);
        (await db.VinculosComEquipamento.CountAsync(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual)).Should().Be(0);
        (await db.RegistrosDeOrigem.CountAsync()).Should().Be(0);
    }
}
