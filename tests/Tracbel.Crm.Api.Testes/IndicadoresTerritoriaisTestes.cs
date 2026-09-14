using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// OS TRÊS MAPAS DA ADR POR HTTP (documento 32, seção 8) — com a aplicação inteira de pé.
///
/// <para>O cenário semeado cobre exatamente as distinções que o pedido exige que a tela não
/// confunda: vínculo coberto, fora da cadência, nunca contatado e em linha sem cadência; município
/// da ADR sem cliente (sem dado, e não zero); área plantada zero ao lado de área não divulgada; e
/// cliente sem município, com município sem código IBGE e de outra UF, que não têm polígono e
/// precisam aparecer somados à parte para o total fechar.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class IndicadoresTerritoriaisTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int RibeiraoPreto = 3543402;
    private const int Serrana = 3551504;
    private const int Jardinopolis = 3525102;

    private const string Periodo = "/api/v1/territorio/indicadores?competenciaInicial=2026-01&competenciaFinal=2026-06";

    // Perfis de teste da visão da empresa — todos fictícios (documento 32, seção 8.5.2).
    private const string PerfilAutorizado = "perfil.autorizado@exemplo.com";
    private const string PerfilComAlcanceCurto = "perfil.alcance.curto@exemplo.com";
    private const string PerfilComConcessaoVencida = "perfil.concessao.vencida@exemplo.com";
    private const string PerfilComConjuntoDesativado = "perfil.conjunto.desativado@exemplo.com";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static JsonElement Municipio(JsonElement dados, int codigo) =>
        dados.GetProperty("indicadores").GetProperty("municipios").EnumerateArray()
            .Single(m => m.GetProperty("codigoIbge").GetInt32() == codigo);

    private static JsonElement ForaDoMapa(JsonElement dados, string grupo) =>
        dados.GetProperty("indicadores").GetProperty("foraDoMapa").EnumerateArray()
            .Single(g => g.GetProperty("grupo").GetString() == grupo);

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.MunicipiosDaAreaDeAtuacao.AnyAsync()) return;

        var agora = DateTime.UtcNow;

        var ribeirao = Dominio.Organizacao.Municipio.Criar("Ribeirão Preto", "SP", RibeiraoPreto);
        var serrana = Dominio.Organizacao.Municipio.Criar("Serrana", "SP", Serrana);
        var jardinopolis = Dominio.Organizacao.Municipio.Criar("Jardinópolis", "SP", Jardinopolis);
        var uberaba = Dominio.Organizacao.Municipio.Criar("Uberaba", "MG", 3170107);
        var segundaGrafia = Dominio.Organizacao.Municipio.Criar("SERRANA X", "SP");
        db.Municipios.AddRange(ribeirao, serrana, jardinopolis, uberaba, segundaGrafia);

        var comCadencia = LinhaDeNegocio.Criar("MAQ_TESTE", "Máquinas");
        comCadencia.DeclararCadencia(180, 180, 180, 360);
        var semCadencia = LinhaDeNegocio.Criar("PECAS_TESTE", "Peças");
        db.LinhasDeNegocio.AddRange(comCadencia, semCadencia);
        await db.SaveChangesAsync();

        foreach (var (municipio, linha) in new[] { (ribeirao, 2), (serrana, 3), (jardinopolis, 4) })
            db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", linha, 100, agora));

        db.ResponsaveisPelosMunicipios.AddRange(
            ResponsavelPeloMunicipio.Registrar(ribeirao.Id, PapelNoMunicipio.Cen, FonteDoResponsavel.PlanilhaAreaDeAtuacao,
                "A Contratar 1", SituacaoDoResponsavel.VagaAContratar, null, null, "Area de Atuação.xlsx", 2, 100, agora),
            ResponsavelPeloMunicipio.Registrar(ribeirao.Id, PapelNoMunicipio.Cen, FonteDoResponsavel.PlanilhaCenEGestorPorMunicipio,
                "CEN.EXEMPLO", SituacaoDoResponsavel.UsuarioIdentificado, 100, "43402", "CEN e Gestor por Municipio.xlsx", 2, 100, agora));

        db.AreasPlantadasNosMunicipios.AddRange(
            AreaPlantadaNoMunicipio.Registrar(ribeirao.Id, 2024, 40139, "Café (em grão) Total", 70m, 100, agora),
            AreaPlantadaNoMunicipio.Registrar(serrana.Id, 2024, 40139, "Café (em grão) Total", null, 100, agora),
            AreaPlantadaNoMunicipio.Registrar(jardinopolis.Id, 2024, 40139, "Café (em grão) Total", 0m, 100, agora));

        var carteiraComCadencia = Carteira.Criar(1, comCadencia.Id, "MAQ_TESTE_01", "Máquinas RP", 100, 100);
        var carteiraSemCadencia = Carteira.Criar(1, semCadencia.Id, "PECAS_TESTE_01", "Peças RP", 100, 100);
        db.Carteiras.AddRange(carteiraComCadencia, carteiraSemCadencia);

        Cliente NovoCliente(string nome) => Cliente.Criar(1, nome, TipoDePessoa.Juridica, 100, 100);
        var coberto = NovoCliente("Coberto em Ribeirão");
        var nunca = NovoCliente("Nunca contatado em Ribeirão");
        var foraDaCadencia = NovoCliente("Fora da cadência em Serrana");
        var mineiro = NovoCliente("Cliente de Uberaba");
        var semEndereco = NovoCliente("Cliente sem endereço");
        var naSegundaGrafia = NovoCliente("Cliente na segunda grafia");
        var inativado = NovoCliente("Cliente inativado");
        db.Clientes.AddRange(coberto, nunca, foraDaCadencia, mineiro, semEndereco, naSegundaGrafia, inativado);
        await db.SaveChangesAsync();

        var motivo = await db.CatalogoItens.FirstAsync(i => i.Codigo == "DUPLICADO");
        inativado.Inativar(motivo.Id, 100);

        void NovoEndereco(Cliente cliente, Dominio.Organizacao.Municipio municipio, string uf) =>
            db.Enderecos.Add(Endereco.Criar(1, cliente.Id, TipoDeEndereco.Fiscal, "Rua do teste",
                MunicipioDoEndereco.Selecionado(municipio.Id), uf, 100, ehPrincipal: true));

        NovoEndereco(coberto, ribeirao, "SP");
        NovoEndereco(nunca, ribeirao, "SP");
        NovoEndereco(foraDaCadencia, serrana, "SP");
        NovoEndereco(mineiro, uberaba, "MG");
        NovoEndereco(naSegundaGrafia, segundaGrafia, "SP");

        var vinculoCoberto = ClienteCarteira.Criar(coberto.Id, carteiraComCadencia.Id, ClasseDeCliente.D, 100);
        vinculoCoberto.RegistrarInteracao(agora.AddDays(-10));
        var vinculoVencido = ClienteCarteira.Criar(foraDaCadencia.Id, carteiraComCadencia.Id, ClasseDeCliente.D, 100);
        vinculoVencido.RegistrarInteracao(agora.AddDays(-400));
        db.ClienteCarteiras.AddRange(
            vinculoCoberto,
            vinculoVencido,
            ClienteCarteira.Criar(nunca.Id, carteiraComCadencia.Id, ClasseDeCliente.D, 100),
            ClienteCarteira.Criar(coberto.Id, carteiraSemCadencia.Id, ClasseDeCliente.D, 100));

        db.FaturamentoDosClientes.AddRange(
            FaturamentoDoCliente.Criar(1, coberto.Id, new DateOnly(2026, 3, 1), 1000m, 1, 3, new QuebraDoFaturamento(600m, 300m, 100m, 0m)),
            FaturamentoDoCliente.Criar(1, coberto.Id, new DateOnly(2025, 12, 1), 999m, 1, 1, new QuebraDoFaturamento(999m, 0m, 0m, 0m)),
            FaturamentoDoCliente.Criar(1, mineiro.Id, new DateOnly(2026, 2, 1), 50m, 1, 1, new QuebraDoFaturamento(0m, 50m, 0m, 0m)),
            // Barretos vendeu para um cliente CADASTRADO em Ribeirão Preto: é a venda que, na visão
            // da filial de Barretos, fica em "cliente de outra filial".
            FaturamentoDoCliente.Criar(2, coberto.Id, new DateOnly(2026, 4, 1), 300m, 1, 1, new QuebraDoFaturamento(300m, 0m, 0m, 0m)),
            FaturamentoDoCliente.Criar(1, inativado.Id, new DateOnly(2026, 5, 1), 80m, 1, 1, new QuebraDoFaturamento(0m, 80m, 0m, 0m)));

        // Notas sem cliente no CRM: duas de Ribeirão Preto (contraparte sem cadastro e fábrica) e uma
        // de Barretos (empresa do grupo). Documentos fictícios.
        db.FaturamentoSemClientes.AddRange(
            FaturamentoSemCliente.Criar(1, new DateOnly(2026, 2, 1), "11111111000111", "Contraparte de teste", NaturezaDoParceiro.ClienteNaoCadastrado, 200m, 1, 1, new QuebraDoFaturamento(200m, 0m, 0m, 0m)),
            FaturamentoSemCliente.Criar(1, new DateOnly(2026, 3, 1), "22222222000122", "Fábrica de teste", NaturezaDoParceiro.Fabrica, 500m, 1, 1, new QuebraDoFaturamento(500m, 0m, 0m, 0m)),
            FaturamentoSemCliente.Criar(2, new DateOnly(2026, 3, 1), "33333333000133", "Empresa do grupo de teste", NaturezaDoParceiro.EmpresaDoGrupo, 40m, 1, 1, new QuebraDoFaturamento(0m, 40m, 0m, 0m)));

        await db.SaveChangesAsync();

        // A CONCESSÃO EXPLÍCITA, nos quatro jeitos que importam: em Organização (abre a empresa), a
        // mesma permissão só até a filial, uma concessão vencida e um conjunto desativado.
        var visaoDaEmpresa = ConjuntoPermissao.Criar("TESTE_VISAO_EMPRESA", "Teste — visão da empresa")
            .Conceder(ContextoAcesso.PermissaoDeAlcanceEntreEmpresas, Profundidade.Organizacao);
        var alcanceCurto = ConjuntoPermissao.Criar("TESTE_ALCANCE_CURTO", "Teste — alcance só da filial")
            .Conceder(ContextoAcesso.PermissaoDeAlcanceEntreEmpresas, Profundidade.EmpresaEAbaixo);
        var desativado = ConjuntoPermissao.Criar("TESTE_CONJUNTO_DESATIVADO", "Teste — conjunto desativado")
            .Conceder(ContextoAcesso.PermissaoDeAlcanceEntreEmpresas, Profundidade.Organizacao);
        desativado.Desativar();
        db.ConjuntosPermissao.AddRange(visaoDaEmpresa, alcanceCurto, desativado);

        foreach (var (id, upn) in new[]
                 {
                     (300L, PerfilAutorizado), (301L, PerfilComAlcanceCurto),
                     (302L, PerfilComConcessaoVencida), (303L, PerfilComConjuntoDesativado)
                 })
        {
            var perfil = Usuario.Criar(Guid.NewGuid(), upn, upn, upn.Split('@')[0], Email.Criar(upn), empresaId: 1, criadoPorId: 100);
            db.Entry(perfil).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(perfil);
        }

        await db.SaveChangesAsync();

        var vencida = UsuarioConjuntoPermissao.Conceder(302, visaoDaEmpresa.Id, 100, DateTime.UtcNow.AddDays(1));
        db.ConcessoesPermissao.AddRange(
            UsuarioConjuntoPermissao.Conceder(300, visaoDaEmpresa.Id, 100),
            UsuarioConjuntoPermissao.Conceder(301, alcanceCurto.Id, 100),
            vencida,
            UsuarioConjuntoPermissao.Conceder(303, desativado.Id, 100));
        db.Entry(vencida).Property(c => c.ExpiraEm).CurrentValue = DateTime.UtcNow.AddDays(-1);

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task A_cobertura_separa_coberto_nunca_contatado_e_linha_sem_cadencia()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        var cobertura = Municipio(dados, RibeiraoPreto).GetProperty("cobertura");
        cobertura.GetProperty("clientes").GetInt32().Should().Be(2);
        cobertura.GetProperty("vinculos").GetInt32().Should().Be(3);
        cobertura.GetProperty("vinculosComCadencia").GetInt32().Should().Be(2,
            "o vínculo em carteira de linha sem cadência não entra no denominador");
        cobertura.GetProperty("cobertos").GetInt32().Should().Be(1);
        cobertura.GetProperty("nuncaContatados").GetInt32().Should().Be(1);
        cobertura.GetProperty("semCadencia").GetInt32().Should().Be(1);
        cobertura.GetProperty("percentualPendente").GetDecimal().Should().Be(50m);

        Municipio(dados, Serrana).GetProperty("cobertura").GetProperty("foraDaCadencia").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Municipio_da_adr_sem_cliente_e_sem_dado_e_nao_zero()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        var jardinopolis = Municipio(dados, Jardinopolis);
        jardinopolis.GetProperty("pertenceAAdr").GetBoolean().Should().BeTrue();
        jardinopolis.GetProperty("cobertura").GetProperty("percentualPendente").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task As_vendas_respeitam_o_periodo_e_o_pos_venda_e_peca_mais_servico()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        var vendas = Municipio(dados, RibeiraoPreto).GetProperty("vendas");
        vendas.GetProperty("valorLiquido").GetDecimal().Should().Be(1000m, "dezembro de 2025 está fora do período pedido");
        vendas.GetProperty("maquina").GetDecimal().Should().Be(600m);
        vendas.GetProperty("posVenda").GetDecimal().Should().Be(400m);
    }

    [Fact]
    public async Task O_que_nao_tem_poligono_aparece_somado_a_parte_e_o_total_fecha()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        ForaDoMapa(dados, "OutraUf").GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(50m);
        ForaDoMapa(dados, "SemMunicipio").GetProperty("cobertura").GetProperty("clientes").GetInt32().Should().Be(1);
        ForaDoMapa(dados, "MunicipioSemCodigoIbge").GetProperty("cobertura").GetProperty("clientes").GetInt32().Should().Be(1);

        var indicadores = dados.GetProperty("indicadores");
        var clientes = indicadores.GetProperty("municipios").EnumerateArray().Sum(m => m.GetProperty("cobertura").GetProperty("clientes").GetInt32())
                       + indicadores.GetProperty("foraDoMapa").EnumerateArray().Sum(g => g.GetProperty("cobertura").GetProperty("clientes").GetInt32());
        clientes.Should().Be(6, "os seis clientes semeados estão no mapa ou num grupo fora dele — nenhum some");
    }

    [Fact]
    public async Task Area_nao_divulgada_e_nula_e_area_zero_da_zero_maquinas()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        Municipio(dados, RibeiraoPreto).GetProperty("potencial")[0].GetProperty("maquinasTeoricas").GetDecimal().Should().Be(7m,
            "70 ha de café a 1 máquina a cada 10 ha");
        Municipio(dados, Serrana).GetProperty("potencial")[0].GetProperty("maquinasTeoricas").ValueKind.Should().Be(JsonValueKind.Null);
        Municipio(dados, Jardinopolis).GetProperty("potencial")[0].GetProperty("maquinasTeoricas").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task As_duas_planilhas_divergindo_sobre_o_cen_ficam_as_duas_e_marcadas()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        var ribeirao = Municipio(dados, RibeiraoPreto);
        ribeirao.GetProperty("responsaveis").GetArrayLength().Should().Be(2);
        ribeirao.GetProperty("cenDivergenteEntreFontes").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task O_cen_e_comparado_sem_fusao_e_cada_indicador_diz_como_ler()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        Municipio(dados, RibeiraoPreto).GetProperty("comparacaoDoCen").GetString()
            .Should().Be("NomesDiferentes", "vaga a contratar numa planilha e um nome na outra");
        Municipio(dados, RibeiraoPreto).GetProperty("responsaveis").GetArrayLength().Should().Be(2, "as duas fontes ficam");
        Municipio(dados, Serrana).GetProperty("comparacaoDoCen").GetString().Should().Be("UmaFonteSo");

        var classificacoes = dados.GetProperty("classificacoes").EnumerateArray()
            .ToDictionary(c => c.GetProperty("indicador").GetString()!, c => c.GetProperty("situacao").GetString());
        classificacoes["vendas"].Should().Be("Medido");
        classificacoes["coberturaDeVisita"].Should().Be("RegraComercialProvisoria");
        classificacoes["posVenda"].Should().Be("RegraComercialProvisoria");
        classificacoes["potencial"].Should().Be("Estimativa");
        classificacoes["cenDoMunicipio"].Should().Be("FonteAConfirmar");
    }

    [Fact]
    public async Task A_outra_filial_ve_a_adr_inteira_mas_nenhum_cliente_de_ribeirao()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeBarretos().GetAsync(Periodo));

        var ribeirao = Municipio(dados, RibeiraoPreto);
        ribeirao.GetProperty("pertenceAAdr").GetBoolean().Should().BeTrue("a área de atuação é mapa da empresa inteira");
        ribeirao.GetProperty("cobertura").GetProperty("clientes").GetInt32().Should().Be(0, "a fronteira de filial vale para cliente");
        ribeirao.GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task Venda_de_uma_filial_para_cliente_cadastrado_em_outra_fica_em_grupo_proprio()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeBarretos().GetAsync(Periodo));

        ForaDoMapa(dados, "ClienteDeOutraFilial").GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(300m,
            "a nota é de Barretos, e o cliente — com o endereço — é do cadastro de Ribeirão Preto");
        Municipio(dados, RibeiraoPreto).GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task A_filial_que_so_vende_para_cliente_proprio_nao_tem_grupo_de_outra_filial()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        dados.GetProperty("indicadores").GetProperty("foraDoMapa").EnumerateArray()
            .Select(g => g.GetProperty("grupo").GetString()).Should().NotContain("ClienteDeOutraFilial",
                "a nota de Barretos para o cliente de Ribeirão não é desta filial e não entra aqui — nem no mapa, nem fora dele");
    }

    [Fact]
    public async Task Venda_de_cliente_inativado_nao_some_e_nao_vira_outra_filial()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        ForaDoMapa(dados, "ClienteInativado").GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(80m);
    }

    [Fact]
    public async Task Nota_sem_cliente_no_crm_fica_a_parte_por_natureza_e_so_na_filial_que_emitiu()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo));

        var semCadastro = ForaDoMapa(dados, "ContraparteSemCadastro");
        semCadastro.GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(200m);
        semCadastro.GetProperty("cobertura").GetProperty("clientes").GetInt32().Should().Be(0, "contraparte sem cadastro não é cliente");
        ForaDoMapa(dados, "RepasseDeFabrica").GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(500m);

        dados.GetProperty("indicadores").GetProperty("foraDoMapa").EnumerateArray()
            .Select(g => g.GetProperty("grupo").GetString()).Should().NotContain("EmpresaDoGrupo", "a nota para a empresa do grupo é de Barretos");
        dados.GetProperty("metricasSemDado").EnumerateArray()
            .Select(m => m.GetProperty("metrica").GetString()).Should().Contain("faturamentoSemClienteNoCrm");
    }

    [Fact]
    public async Task Filtrar_pela_filial_do_cliente_tira_a_nota_sem_cliente_que_nao_tem_cadastro()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo + "&filialDoCliente=" + ApiEmMemoria.FilialDeRibeirao));

        dados.GetProperty("indicadores").GetProperty("foraDoMapa").EnumerateArray()
            .Select(g => g.GetProperty("grupo").GetString()).Should().NotContain(["ContraparteSemCadastro", "RepasseDeFabrica"]);
        Municipio(dados, RibeiraoPreto).GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(1000m);
    }

    [Fact]
    public async Task A_visao_da_empresa_inteira_exige_a_permissao_de_alcance_entre_filiais()
    {
        await SemearAsync();
        var resposta = await api.ClienteDeRibeirao().GetAsync(Periodo + "&visao=Empresa");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden, "sem concessão explícita, a ponte não concede alcance de organização");
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("type").GetString()
            .Should().EndWith("sem-acesso");
    }

    [Fact]
    public async Task Filtrar_por_filial_fora_do_alcance_e_recusado_com_403()
    {
        await SemearAsync();
        var resposta = await api.ClienteDeRibeirao().GetAsync(Periodo + "&filialDoCliente=" + ApiEmMemoria.FilialDeBarretos);

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task O_filtro_da_filial_da_venda_so_mexe_nas_vendas_e_o_painel_diz_se_a_empresa_inteira_esta_disponivel()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Periodo + "&filialDaVenda=" + ApiEmMemoria.FilialDeRibeirao));

        Municipio(dados, RibeiraoPreto).GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(1000m);
        Municipio(dados, RibeiraoPreto).GetProperty("cobertura").GetProperty("vinculosComCadencia").GetInt32().Should().Be(2);
        dados.GetProperty("podeVerEmpresaInteira").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Perfil_com_concessao_explicita_ve_a_empresa_inteira_e_cada_venda_no_municipio_do_cliente()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteComo(PerfilAutorizado, ApiEmMemoria.FilialDeRibeirao).GetAsync(Periodo + "&visao=Empresa"));

        dados.GetProperty("podeVerEmpresaInteira").GetBoolean().Should().BeTrue();
        dados.GetProperty("indicadores").GetProperty("visao").GetString().Should().Be("Empresa");
        Municipio(dados, RibeiraoPreto).GetProperty("vendas").GetProperty("valorLiquido").GetDecimal()
            .Should().Be(1300m, "a nota de Barretos para o cliente de Ribeirão vai para Ribeirão na visão da empresa");

        var grupos = dados.GetProperty("indicadores").GetProperty("foraDoMapa").EnumerateArray()
            .Select(g => g.GetProperty("grupo").GetString()).ToList();
        grupos.Should().NotContain("ClienteDeOutraFilial");
        grupos.Should().Contain("EmpresaDoGrupo", "a nota de Barretos para a empresa do grupo entra na visão da empresa");
    }

    [Fact]
    public async Task Na_visao_da_empresa_o_perfil_autorizado_filtra_por_qualquer_filial()
    {
        await SemearAsync();
        var autorizado = api.ClienteComo(PerfilAutorizado, ApiEmMemoria.FilialDeRibeirao);

        var vendidoPorBarretos = await DadosAsync(await autorizado.GetAsync(
            Periodo + "&visao=Empresa&filialDaVenda=" + ApiEmMemoria.FilialDeBarretos));
        Municipio(vendidoPorBarretos, RibeiraoPreto).GetProperty("vendas").GetProperty("valorLiquido").GetDecimal().Should().Be(300m);

        var clientesDeBarretos = await DadosAsync(await autorizado.GetAsync(
            Periodo + "&visao=Empresa&filialDoCliente=" + ApiEmMemoria.FilialDeBarretos));
        Municipio(clientesDeBarretos, RibeiraoPreto).GetProperty("cobertura").GetProperty("clientes").GetInt32()
            .Should().Be(0, "no cenário, nenhum cliente cadastrado em Barretos mora em Ribeirão");
    }

    [Fact]
    public async Task O_perfil_autorizado_na_visao_da_filial_continua_vendo_so_a_filial()
    {
        await SemearAsync();
        var dados = await DadosAsync(await api.ClienteComo(PerfilAutorizado, ApiEmMemoria.FilialDeRibeirao).GetAsync(Periodo));

        dados.GetProperty("podeVerEmpresaInteira").GetBoolean().Should().BeTrue();
        dados.GetProperty("indicadores").GetProperty("visao").GetString().Should().Be("Filial");
        Municipio(dados, RibeiraoPreto).GetProperty("vendas").GetProperty("valorLiquido").GetDecimal()
            .Should().Be(1000m, "poder abrir o alcance não é estar com ele aberto");
    }

    [Theory]
    [InlineData(PerfilComAlcanceCurto)]
    [InlineData(PerfilComConcessaoVencida)]
    [InlineData(PerfilComConjuntoDesativado)]
    public async Task Concessao_so_ate_a_filial_vencida_ou_de_conjunto_desativado_nao_abre_a_empresa(string perfil)
    {
        await SemearAsync();
        var resposta = await api.ClienteComo(perfil, ApiEmMemoria.FilialDeRibeirao).GetAsync(Periodo + "&visao=Empresa");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("?visao=Regional", "visao")]
    [InlineData("?filialDaVenda=999999", "filialDaVenda")]
    [InlineData("?regiao=Sul", "regiao")]
    [InlineData("?competenciaInicial=2026-13", "competenciaInicial")]
    [InlineData("?competenciaInicial=2026-06&competenciaFinal=2026-01", "competenciaInicial")]
    [InlineData("?competenciaInicial=2020-01&competenciaFinal=2026-01", "competenciaInicial")]
    public async Task Parametro_invalido_e_recusado_com_o_campo_nomeado(string consulta, string campo)
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync("/api/v1/territorio/indicadores" + consulta);

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var erros = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("erros");
        erros.EnumerateArray().Select(e => e.GetProperty("campo").GetString()).Should().Contain(campo);
    }
}
