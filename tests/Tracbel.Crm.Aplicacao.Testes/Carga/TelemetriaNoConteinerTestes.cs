using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDoParque;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A TELEMETRIA DO OPERATIONS CENTER NO SQL SERVER DE VERDADE (28/09/2026) — o contêiner de desenvolvimento, com a cadeia
/// inteira de migrações aplicada.
///
/// <para><b>O que só o motor de verdade prova:</b> as colunas novas do equipamento e o <c>CK_Equipamento_Posicao</c>, que
/// aceita a posição fora de São Paulo sem município e recusa a meia posição; a chave estrangeira para o município; a
/// rotina 11 e a conexão 14 semeadas; e que a segunda rodada, com a mesma leitura, não grava nada.</para>
///
/// <para><b>Como rodar:</b> suba o contêiner (<c>./scripts/banco/subir-banco.ps1</c>) e rode <c>dotnet test</c>. Sem
/// servidor, o teste é IGNORADO com a razão dita em voz alta.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class TelemetriaNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmTelemetriaTeste";

    private const string ChassiImplausivel = "1RW6110JCMR800001";
    private const string ChassiNoRio = "1RW6110JCMR800002";
    private const int RibeiraoIbge = 3543402;

    private static readonly DateTime Dia20 = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Dia25 = new(2026, 9, 25, 10, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    /// <summary>O parque da semente (a máquina do ART) mais duas máquinas do Protheus, e Ribeirão Preto no catálogo.</summary>
    private static SementeDoParque Recriar()
    {
        SqlConnection.ClearAllPools();
        using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        db.Database.EnsureDeleted();
        db.Database.Migrate();
        var semente = Semear(db, forcarIdentificadores: false);

        db.Municipios.Add(Municipio.Criar("Ribeirão Preto", "SP", RibeiraoIbge));
        foreach (var chassi in new[] { ChassiImplausivel, ChassiNoRio })
            db.Equipamentos.Add(Equipamento.RegistrarPelaIntegracao(semente.RibeiraoPreto, Chassi.Criar(chassi), OrigemDoEquipamento.Protheus, semente.Operador));
        db.SaveChanges();

        return semente;
    }

    /// <summary>
    /// O OPERATIONS CENTER: a máquina do ART com o chassi escrito de outro jeito, em Ribeirão; uma com o horímetro de 374 mil
    /// horas e a posição no mar; uma no Rio de Janeiro; uma que o CRM não conhece; e uma sem chassi.
    /// </summary>
    private static List<MaquinaNaTelemetria> Conectadas() =>
    [
        new("m1", ChassiDoArt.ToLowerInvariant().Insert(3, " "), 1500.25m, Dia20, -21.1767, -47.8208, Dia25),
        new("m2", ChassiImplausivel, 374_466.75m, Dia20, -23.0, -30.0, Dia20),
        new("m3", ChassiNoRio, 2000m, Dia20, -22.9068, -43.1729, Dia20),
        new("m4", "1RW9999ZZZZ999999", 10m, Dia20, -21.2, -47.8, Dia20),
        new("m5", null, 10m, Dia20, -21.2, -47.8, Dia20)
    ];

    /// <summary>A malha: um quadrado em volta de Ribeirão Preto. O Rio fica de fora.</summary>
    private static LocalizadorDeMunicipio Malha() => new(new Dictionary<int, PoligonoMunicipal>
    {
        [RibeiraoIbge] = PoligonoMunicipal.DeAneis([[[(-48.0, -21.5), (-47.5, -21.5), (-47.5, -21.0), (-48.0, -21.0), (-48.0, -21.5)]]])
    });

    private static async Task<RelatorioDaTelemetria> Carregar(SementeDoParque semente, bool simular = false, bool semMalha = false)
    {
        var opcoes = Opcoes();
        CrmDbContext Abrir() => new(opcoes, new ContextoDeCargaDeSistema(semente.Operador, semente.RibeiraoPreto, semente.Filiais));

        var carga = new CargaDaTelemetriaDoOperationsCenter(
            Abrir,
            (_, _) => Task.FromResult(Resultado<IReadOnlyList<MaquinaNaTelemetria>>.Ok(Conectadas())),
            _ => Task.FromResult(semMalha ? null : Malha()),
            _ => { });

        var resultado = await carga.ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private static string Rotulo(DesfechoDaTelemetria desfecho) => CargaDaTelemetriaDoOperationsCenter.RotuloDoDesfecho[desfecho];

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_o_horimetro_e_a_posicao_entram_na_maquina_do_mesmo_chassi_e_a_segunda_rodada_nao_grava()
    {
        var semente = Recriar();

        // A SIMULAÇÃO CONTA TUDO E NÃO GRAVA.
        var simulada = await Carregar(semente, simular: true);
        simulada.Simulada.Should().BeTrue();
        simulada.Valor(CargaDaTelemetriaDoOperationsCenter.HorimetrosGravados).Should().Be(2);
        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
            (await db.Equipamentos.CountAsync(e => e.HorimetroAtual != null || e.PosicaoEm != null)).Should().Be(0, "a simulação não grava");

        var primeira = await Carregar(semente);
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.MaquinasLidas).Should().Be(5);
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.MaquinasDoCrm).Should().Be(3);
        primeira.Valor(Rotulo(DesfechoDaTelemetria.CasadaPeloChassi)).Should().Be(3);
        primeira.Valor(Rotulo(DesfechoDaTelemetria.SemParNoCrm)).Should().Be(1);
        primeira.Valor(Rotulo(DesfechoDaTelemetria.SemVin)).Should().Be(1);
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.HorimetrosGravados).Should().Be(2);
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.HorimetrosImplausiveis).Should().Be(1, "374 mil horas não é máquina de verdade");
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesGravadas).Should().Be(2);
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesInvalidas).Should().Be(1, "o ponto no mar, fora do Brasil");
        primeira.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesForaDeSp).Should().Be(1);

        await using (var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia))
        {
            var ribeirao = await db.Municipios.SingleAsync(m => m.CodigoIbge == RibeiraoIbge);
            var doArt = await db.Equipamentos.SingleAsync(e => e.Id == semente.MaquinaDoArt);
            doArt.HorimetroAtual.Should().Be(1500.25m);
            doArt.HorimetroAtualizadoEm.Should().Be(Dia20);
            (doArt.PosicaoLatitude, doArt.PosicaoLongitude, doArt.PosicaoEm, doArt.MunicipioDaPosicaoId)
                .Should().Be((-21.1767m, -47.8208m, (DateTime?)Dia25, (int?)ribeirao.Id));
            doArt.AlteradoEm.Should().BeNull("a telemetria é leitura, não decisão: não carimba o cadastro");

            var noRio = await db.Equipamentos.SingleAsync(e => e.Id == MaquinaId(db, ChassiNoRio));
            noRio.PosicaoEm.Should().Be(Dia20);
            noRio.MunicipioDaPosicaoId.Should().BeNull("o Rio está fora da malha de São Paulo — o CHECK aceita posição sem município");

            var implausivel = await db.Equipamentos.SingleAsync(e => e.Id == MaquinaId(db, ChassiImplausivel));
            (implausivel.HorimetroAtual, implausivel.PosicaoEm).Should().Be(((decimal?)null, (DateTime?)null));

            (await db.Rotinas.SingleAsync(r => r.Id == 11)).Codigo.Should().Be(RotinasDoSistema.TelemetriaOperationsCenter);
            (await db.Conexoes.SingleAsync(c => c.Id == 14)).Codigo.Should().Be(ConexoesDoSistema.OperationsCenter);
            (await db.PontosDeSincronismo.SingleAsync(p => p.Fluxo == CargaDaTelemetriaDoOperationsCenter.Fluxo)).Should().NotBeNull();
        }

        var segunda = await Carregar(semente);
        segunda.Valor(CargaDaTelemetriaDoOperationsCenter.HorimetrosGravados).Should().Be(0);
        segunda.Valor(CargaDaTelemetriaDoOperationsCenter.HorimetrosMantidos).Should().Be(2, "a mesma leitura não muda nada");
        segunda.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesGravadas).Should().Be(0);
        segunda.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesMantidas).Should().Be(2);
    }

    [FatoSeHouverSqlServer]
    public async Task Sem_a_malha_do_IBGE_so_o_horimetro_entra_e_o_relatorio_diz_por_que()
    {
        var semente = Recriar();

        var rodada = await Carregar(semente, semMalha: true);

        rodada.Valor(CargaDaTelemetriaDoOperationsCenter.HorimetrosGravados).Should().Be(2);
        rodada.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesGravadas).Should().Be(0);
        rodada.Valor(CargaDaTelemetriaDoOperationsCenter.PosicoesSemMalha).Should().Be(2);
        rodada.Observacoes.Should().ContainSingle().Which.Should().Contain("malha");

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        (await db.Equipamentos.CountAsync(e => e.PosicaoEm != null)).Should().Be(0, "posição sem município iria dizer 'fora de SP' sem ser verdade");
    }

    [FatoSeHouverSqlServer]
    public async Task O_banco_recusa_a_meia_posicao()
    {
        Recriar();

        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var gravar = () => db.Database.ExecuteSqlRawAsync(
            "UPDATE frota.Equipamento SET PosicaoLatitude = -21.1 WHERE Chassi = {0}", ChassiNoRio);

        (await gravar.Should().ThrowAsync<SqlException>()).Which.Message.Should().Contain("CK_Equipamento_Posicao");
    }
}
