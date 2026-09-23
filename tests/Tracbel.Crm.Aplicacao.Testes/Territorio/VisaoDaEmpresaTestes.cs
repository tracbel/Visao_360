using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Territorio;

/// <summary>
/// A VISÃO DA EMPRESA E A VISÃO DA FILIAL sobre as mesmas notas (documento 32, seção 8.5) — com o
/// repositório de verdade e o filtro global de verdade.
///
/// <para>O cenário é o menor que separa as duas filiais de uma venda: um cliente de Ribeirão
/// Preto que compra em Ribeirão Preto (100), um cliente de Barretos que compra em Ribeirão Preto
/// (50) e o mesmo cliente de Barretos comprando em Barretos (70). Os dois clientes moram no mesmo
/// município. Ribeirão Preto ainda emite 30 para contraparte sem cadastro, e Barretos 20 para a
/// fábrica. A prova de que não há omissão nem dupla contagem é aritmética: a soma das visões de
/// filial é o total da empresa, e cada venda aparece em exatamente um lugar em cada visão.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class VisaoDaEmpresaTestes : IDisposable
{
    private const int Ribeirao = 1;
    private const int Barretos = 2;
    private const int CodigoDeRibeiraoPreto = 3543402;

    private static readonly DateTime Agora = new(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);

    private static readonly ConsultaDeIndicadoresTerritoriais PrimeiroSemestre =
        new(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 1), null, null);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly ProvedorDeTeste _provedor = new();
    private readonly DiarioDeTeste _diario = new();

    public VisaoDaEmpresaTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));

        _provedor.Atual = Contexto(Ribeirao, sistema: true);
        using var db = NovoContexto();
        db.Database.EnsureCreated();
        Semear(db);
    }

    public void Dispose() => _conexao.Dispose();

    [Fact]
    public async Task Na_visao_da_filial_a_venda_para_cliente_de_outra_filial_fica_a_parte()
    {
        var ribeirao = await ApurarAsync(Contexto(Ribeirao), PrimeiroSemestre);

        VendasNoMunicipio(ribeirao).Should().Be(100m);
        VendasDoGrupo(ribeirao, "ClienteDeOutraFilial").Should().Be(50m);
        VendasDoGrupo(ribeirao, "ContraparteSemCadastro").Should().Be(30m);
        Total(ribeirao).Should().Be(180m, "tudo o que Ribeirão Preto emitiu: 100 + 50 + 30");

        var barretos = await ApurarAsync(Contexto(Barretos), PrimeiroSemestre);

        VendasNoMunicipio(barretos).Should().Be(70m);
        VendasDoGrupo(barretos, "ClienteDeOutraFilial").Should().Be(0m, "Barretos só vendeu para cliente do próprio cadastro");
        VendasDoGrupo(barretos, "RepasseDeFabrica").Should().Be(20m);
        Total(barretos).Should().Be(90m);
    }

    [Fact]
    public async Task Na_visao_da_empresa_toda_venda_vai_para_o_municipio_do_cliente_e_o_total_e_a_soma_das_filiais()
    {
        var empresa = await ApurarAsync(Contexto(Ribeirao, empresaInteira: true), PrimeiroSemestre with { Visao = VisaoTerritorial.Empresa });

        VendasNoMunicipio(empresa).Should().Be(220m);
        empresa.ForaDoMapa.Should().NotContain(g => g.Grupo == "ClienteDeOutraFilial");

        var somaDasFiliais = Total(await ApurarAsync(Contexto(Ribeirao), PrimeiroSemestre))
                             + Total(await ApurarAsync(Contexto(Barretos), PrimeiroSemestre));

        Total(empresa).Should().Be(somaDasFiliais, "nenhuma nota some e nenhuma é contada duas vezes entre as visões");
        Total(empresa).Should().Be(270m, "as cinco notas semeadas: 100 + 50 + 70 + 30 + 20");
        _diario.Aberturas.Should().Be(1, "a visão da empresa abre o alcance entre filiais uma vez, e registra");
    }

    [Theory]
    [InlineData(Ribeirao, null, 150, 180)]
    [InlineData(null, Barretos, 120, 120)]
    [InlineData(Ribeirao, Barretos, 50, 50)]
    [InlineData(Barretos, Ribeirao, 0, 0)]
    public async Task Filial_da_venda_e_filial_do_cliente_sao_filtros_independentes(
        int? filialDaVenda, int? filialDoCliente, int esperado, int esperadoNoTotal)
    {
        var consulta = PrimeiroSemestre with
        {
            Visao = VisaoTerritorial.Empresa,
            FilialDaVendaId = filialDaVenda,
            FilialDoClienteId = filialDoCliente
        };

        var empresa = await ApurarAsync(Contexto(Ribeirao, empresaInteira: true), consulta);

        VendasNoMunicipio(empresa).Should().Be(esperado);
        Total(empresa).Should().Be(esperadoNoTotal,
            "o que o filtro exclui não vai para grupo nenhum; a nota sem cliente só fica quando não se filtra pela filial do cliente");
    }

    [Fact]
    public async Task Sem_a_permissao_a_camada_de_dados_tambem_recusa_a_visao_da_empresa()
    {
        var apurar = () => ApurarAsync(Contexto(Ribeirao), PrimeiroSemestre with { Visao = VisaoTerritorial.Empresa });

        await apurar.Should().ThrowAsync<InvalidOperationException>(
            "o caso de uso já recusa com 403; o CrmDbContext é a segunda trava, e não abre o alcance sem a permissão");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private static decimal VendasNoMunicipio(IndicadoresTerritoriais indicadores) =>
        indicadores.Municipios.Single(m => m.CodigoIbge == CodigoDeRibeiraoPreto).Vendas.ValorLiquido;

    private static decimal VendasDoGrupo(IndicadoresTerritoriais indicadores, string grupo) =>
        indicadores.ForaDoMapa.Where(g => g.Grupo == grupo).Sum(g => g.Vendas.ValorLiquido);

    private static decimal Total(IndicadoresTerritoriais indicadores) =>
        indicadores.Municipios.Sum(m => m.Vendas.ValorLiquido) + indicadores.ForaDoMapa.Sum(g => g.Vendas.ValorLiquido);

    private async Task<IndicadoresTerritoriais> ApurarAsync(ContextoAcesso contexto, ConsultaDeIndicadoresTerritoriais consulta)
    {
        _provedor.Atual = contexto;
        await using var db = NovoContexto();
        // O MOTOR DO POTENCIAL É OUTRO REPOSITÓRIO desde a issue 161 — a calculadora precisa do mesmo
        // catálogo, e duas leituras dele divergiriam. Aqui ele lê o mesmo contexto.
        return await new RepositorioDeIndicadoresTerritoriais(db, new RepositorioDoMotorDoPotencial(db))
            .ApurarAsync(consulta, Agora, CancellationToken.None);
    }

    private CrmDbContext NovoContexto() =>
        new(new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options, _provedor, _diario);

    private static ContextoAcesso Contexto(int empresaId, bool sistema = false, bool empresaInteira = false) =>
        new(100,
            nomeExibicao: "usuário de teste",
            empresaId: empresaId,
            empresasVisiveis: new HashSet<int> { empresaId },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: empresaInteira
                ? new Dictionary<string, Profundidade> { [ContextoAcesso.PermissaoDeAlcanceEntreEmpresas] = Profundidade.Organizacao }
                : new Dictionary<string, Profundidade>(),
            ehServicoDeSistema: sistema);

    private static void Semear(CrmDbContext db)
    {
        foreach (var (id, codigo, nome) in new[] { (Ribeirao, "010101", "Tracbel Agro — Ribeirão Preto"), (Barretos, "010103", "Tracbel Agro — Barretos") })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        var usuario = Usuario.Criar(Guid.NewGuid(), "teste@tracbel.com.br", "Teste", "Teste", Email.Criar("teste@tracbel.com.br"), Ribeirao, 1);
        db.Entry(usuario).Property(u => u.Id).CurrentValue = 100L;
        db.Usuarios.Add(usuario);

        var municipio = Municipio.Criar("Ribeirão Preto", "SP", CodigoDeRibeiraoPreto);
        db.Municipios.Add(municipio);
        db.SaveChanges();

        db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
            municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, Ribeirao, "Area de Atuação.xlsx", 2, 100, Agora));

        var deRibeirao = Cliente.Criar(Ribeirao, "Cliente de Ribeirão", TipoDePessoa.Juridica, 100, 100);
        var deBarretos = Cliente.Criar(Barretos, "Cliente de Barretos", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.AddRange(deRibeirao, deBarretos);
        db.SaveChanges();

        foreach (var cliente in new[] { deRibeirao, deBarretos })
            db.Enderecos.Add(Endereco.Criar(cliente.EmpresaId, cliente.Id, TipoDeEndereco.Fiscal, "Rua do teste",
                MunicipioDoEndereco.Selecionado(municipio.Id), "SP", 100, ehPrincipal: true));

        var mes = new DateOnly(2026, 3, 1);
        db.FaturamentoDosClientes.AddRange(
            FaturamentoDoCliente.Criar(Ribeirao, deRibeirao.Id, mes, 100m, 1, 1, new QuebraDoFaturamento(100m, 0m, 0m, 0m)),
            FaturamentoDoCliente.Criar(Ribeirao, deBarretos.Id, mes, 50m, 1, 1, new QuebraDoFaturamento(0m, 50m, 0m, 0m)),
            FaturamentoDoCliente.Criar(Barretos, deBarretos.Id, mes, 70m, 1, 1, new QuebraDoFaturamento(0m, 0m, 70m, 0m)));

        db.FaturamentoSemClientes.AddRange(
            FaturamentoSemCliente.Criar(Ribeirao, mes, "11111111000111", "Contraparte de teste", NaturezaDoParceiro.ClienteNaoCadastrado, 30m, 1, 1, new QuebraDoFaturamento(30m, 0m, 0m, 0m)),
            FaturamentoSemCliente.Criar(Barretos, mes, "22222222000122", "Fábrica de teste", NaturezaDoParceiro.Fabrica, 20m, 1, 1, new QuebraDoFaturamento(20m, 0m, 0m, 0m)));

        db.SaveChanges();
    }

    private sealed class ProvedorDeTeste : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; set; } = default!;
    }

    private sealed class DiarioDeTeste : IDiarioDeAlcanceEntreEmpresas
    {
        public int Aberturas { get; private set; }

        public void Abriu(ContextoAcesso acesso, string motivo) => Aberturas++;

        public void Fechou(ContextoAcesso acesso, string motivo, TimeSpan duracao)
        {
        }
    }
}
