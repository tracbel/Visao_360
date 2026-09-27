using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Aplicacao.Testes.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// A FICHA 360 DO CLIENTE NO SQL SERVER DE VERDADE — o contêiner de desenvolvimento, com a cadeia inteira de migrações.
///
/// <para><b>O que só o motor de verdade prova:</b> que as três leituras novas traduzem para o SQL Server com o filtro
/// global de filial dentro delas — o <c>EXISTS</c> do vínculo no filtro por cliente, a subconsulta da venda na relação,
/// o <c>MAX(COALESCE(...))</c> da data da carga e a junção das carteiras com o responsável e a linha de negócio. Os
/// testes de API rodam no SQLite, que traduz diferente.</para>
///
/// <para><b>Como rodar:</b> suba o contêiner (<c>./scripts/banco/subir-banco.ps1</c>) e rode <c>dotnet test</c>. Sem
/// servidor, o teste é IGNORADO com a razão dita em voz alta.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class FichaDoClienteNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmFichaDoClienteTeste";

    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    [FatoSeHouverSqlServer]
    public async Task No_SQL_Server_a_frota_o_faturamento_e_as_carteiras_do_cliente_respeitam_a_filial()
    {
        var semente = await SemearAsync();
        var acesso = new ProvedorFixo(new ContextoAcesso(
            semente.Operador,
            nomeExibicao: "cen de ribeirão",
            empresaId: semente.Ribeirao,
            empresasVisiveis: new HashSet<int> { semente.Ribeirao },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: Permissoes.Catalogo.Keys.ToDictionary(p => p, _ => Profundidade.EmpresaEAbaixo)));
        var relogio = new RelogioFixo(Agora);

        await using var db = new CrmDbContext(Opcoes(), acesso);

        // ---- a frota ----
        var listar = new ListarEquipamentos(
            new RepositorioDeEquipamentos(db), new RepositorioDeClientes(db), new RepositorioDeCatalogos(db), relogio);
        var frota = await listar.ExecutarAsync(1, 50, null, null, null, semente.ChaveDoDono, null, false, false, null, null, false,
            CancellationToken.None);

        frota.EhSucesso.Should().BeTrue(frota.Erro);
        var maquinas = frota.Valor.Dados.Itens.ToDictionary(m => m.Chassi);
        maquinas.Keys.Should().BeEquivalentTo(["1FICHASQLNOTA0001", "1FICHASQLREVE0003"],
            "o dono atual e a compra no ART entram; a máquina de Barretos fica fora do alcance");
        maquinas["1FICHASQLNOTA0001"].EvidenciaDoDonoAtual.Should().Be(nameof(EvidenciaDoProprietario.NotaDeVenda));
        maquinas["1FICHASQLNOTA0001"].RelacaoComOCliente!.EhDonoAtual.Should().BeTrue();
        maquinas["1FICHASQLREVE0003"].RelacaoComOCliente!.CompradaEm.Should().Be(new DateOnly(2023, 3, 20));
        maquinas["1FICHASQLREVE0003"].DonoAtualNome.Should().Be("OUTRO DONO FICTICIO");

        // ---- o faturamento ----
        var faturamento = await new ObterFaturamentoDoCliente(new RepositorioDeFaturamento(db), relogio)
            .ExecutarAsync(semente.ChaveDoDono, CancellationToken.None);

        faturamento.EhSucesso.Should().BeTrue(faturamento.Erro);
        var fat = faturamento.Valor.Dados;
        fat.CompetenciaMaisRecente.Should().Be(new DateOnly(2026, 9, 1));
        fat.CarregadoEm.Should().NotBeNull();
        fat.DozeMeses!.ValorLiquido.Should().Be(1500m, "a nota de Barretos fica fora do alcance");
        fat.DozeMeses.Maquina.Should().Be(600m);
        fat.PorFilial.Select(f => f.FilialCodigo).Should().Equal("010101");

        // ---- as carteiras ----
        var carteiras = await new ListarCarteirasDoCliente(new RepositorioDeCarteiras(db), relogio)
            .ExecutarAsync(semente.ChaveDoDono, CancellationToken.None);

        carteiras.EhSucesso.Should().BeTrue(carteiras.Erro);
        var lista = carteiras.Valor.Dados;
        lista.Classe.Should().Be("A");
        lista.Carteiras.Should().ContainSingle("a carteira de Barretos está fora do alcance");
        lista.Carteiras[0].DiasDeCadencia.Should().Be(180);
        lista.Carteiras[0].ResponsavelNome.Should().Be("operador.ficha");
        lista.Carteiras[0].NaturezaDoResponsavel.Should().Be(nameof(NaturezaDoUsuario.Pessoa));
        lista.Carteiras[0].FilialCodigo.Should().Be("010101");
    }

    private static async Task<Semente> SemearAsync()
    {
        SqlConnection.ClearAllPools();
        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        var ribeirao = Empresa.Criar("010101", "Tracbel Agro — Ribeirão Preto");
        var barretos = Empresa.Criar("010103", "Tracbel Agro — Barretos");
        db.Empresas.AddRange(ribeirao, barretos);
        await db.SaveChangesAsync();

        var operador = Usuario.Criar(Guid.NewGuid(), "operador.ficha@tracbel.com.br", "operador.ficha", "operador.ficha",
            Email.Criar("operador.ficha@tracbel.com.br"), ribeirao.Id, criadoPorId: 1);
        db.Usuarios.Add(operador);
        await db.SaveChangesAsync();

        // Documentos fictícios, com dígito verificador válido.
        var dono = Cliente.Criar(ribeirao.Id, "DONO FICTICIO LTDA", TipoDePessoa.Juridica, operador.Id, operador.Id,
            documento: CpfCnpj.Criar("11222333000181"), situacao: SituacaoDoCliente.Cliente);
        var outro = Cliente.Criar(ribeirao.Id, "OUTRO DONO FICTICIO", TipoDePessoa.Fisica, operador.Id, operador.Id,
            documento: CpfCnpj.Criar("52998224725"), situacao: SituacaoDoCliente.Cliente);
        db.Clientes.AddRange(dono, outro);
        await db.SaveChangesAsync();
        dono.ApurarClasse(ClasseDeCliente.A, 1500m, Agora, operador.Id);

        var art = Sistema.Criar("ART", "ART — vendas de máquina", "teste");
        var protheus = Sistema.Criar("PROTHEUS", "Protheus (TOTVS) — ERP", "teste");
        db.Sistemas.AddRange(art, protheus);

        var pelaNota = Equipamento.RegistrarPelaIntegracao(ribeirao.Id, Chassi.Criar("1FICHASQLNOTA0001"), OrigemDoEquipamento.Protheus, operador.Id);
        var deBarretos = Equipamento.RegistrarPelaIntegracao(barretos.Id, Chassi.Criar("1FICHASQLBARR0002"), OrigemDoEquipamento.Protheus, operador.Id);
        var revendida = Equipamento.RegistrarPelaIntegracao(ribeirao.Id, Chassi.Criar("1FICHASQLREVE0003"), OrigemDoEquipamento.Art, operador.Id);
        db.Equipamentos.AddRange(pelaNota, deBarretos, revendida);
        await db.SaveChangesAsync();

        var venda = VendaDeMaquina.Registrar(art.Id, "S1", revendida.Id, dono.Id, new DadosDaVendaNaOrigem(
                ribeirao.Id, null, new DateOnly(2023, 3, 20), new DateOnly(2023, 3, 20), null, null, null, null, null, null,
                false, false, 1, "TRATOR", "TR 6135M", null, null, null, "s1", null),
            Agora, operador.Id);
        db.VendasDeMaquina.Add(venda);
        await db.SaveChangesAsync();

        db.VinculosComEquipamento.AddRange(
            VinculoDeClienteComEquipamento.RegistrarCompradorNaVenda(ribeirao.Id, dono.Id, revendida.Id, venda.Id, art.Id, venda.VendidaEm, operador.Id),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                ribeirao.Id, dono.Id, pelaNota.Id, protheus.Id, new DateOnly(2025, 3, 10), EvidenciaDoProprietario.NotaDeVenda, operador.Id),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                ribeirao.Id, dono.Id, deBarretos.Id, protheus.Id, new DateOnly(2025, 5, 5), EvidenciaDoProprietario.OrdemDeServico, operador.Id),
            VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                ribeirao.Id, outro.Id, revendida.Id, protheus.Id, new DateOnly(2025, 8, 1), EvidenciaDoProprietario.OrdemDeServico, operador.Id));

        db.FaturamentoDosClientes.AddRange(
            FaturamentoDoCliente.Criar(ribeirao.Id, outro.Id, new DateOnly(2026, 9, 1), 10m, 1, 1, new QuebraDoFaturamento(0m, 10m, 0m, 0m)),
            FaturamentoDoCliente.Criar(ribeirao.Id, dono.Id, new DateOnly(2026, 8, 1), 1000m, 2, 4, new QuebraDoFaturamento(600m, 300m, 100m, 0m)),
            FaturamentoDoCliente.Criar(ribeirao.Id, dono.Id, new DateOnly(2025, 10, 1), 500m, 1, 2, new QuebraDoFaturamento(0m, 500m, 0m, 0m)),
            FaturamentoDoCliente.Criar(barretos.Id, dono.Id, new DateOnly(2026, 7, 1), 777m, 1, 1, new QuebraDoFaturamento(777m, 0m, 0m, 0m)));

        var maquinas = LinhaDeNegocio.Criar("MAQ_FICHA_SQL", "Máquinas");
        maquinas.DeclararCadencia(180, 180, 180, 360);
        db.LinhasDeNegocio.Add(maquinas);
        await db.SaveChangesAsync();

        var comercial = Carteira.Criar(ribeirao.Id, maquinas.Id, "MAQ_FICHA_SQL_RP", "Máquinas RP", operador.Id, operador.Id);
        var deBarretosCarteira = Carteira.Criar(barretos.Id, maquinas.Id, "MAQ_FICHA_SQL_BA", "Máquinas BA", operador.Id, operador.Id);
        db.Carteiras.AddRange(comercial, deBarretosCarteira);
        await db.SaveChangesAsync();

        db.ClienteCarteiras.AddRange(
            ClienteCarteira.Criar(dono.Id, comercial.Id, ClasseDeCliente.C, operador.Id),
            ClienteCarteira.Criar(dono.Id, deBarretosCarteira.Id, ClasseDeCliente.C, operador.Id));
        await db.SaveChangesAsync();

        return new Semente(ribeirao.Id, operador.Id, dono.ChavePublica);
    }

    private sealed record Semente(int Ribeirao, long Operador, Guid ChaveDoDono);

    private sealed class ProvedorFixo(ContextoAcesso atual) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = atual;
    }

    private sealed class RelogioFixo(DateTime agora) : IRelogio
    {
        public DateTime Agora { get; } = agora;
    }
}
