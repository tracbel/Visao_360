using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Api.Seguranca;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A PORTA DE ENTRADA: para onde o login devolve, o que a sessão responde, e — o que mais importa —
/// a qual cadastro uma conta Microsoft é ligada.
///
/// <para>O casamento da conta é testado contra o resolvedor de verdade e o banco de verdade da API em
/// memória, e não contra dublê: o defeito que custaria caro aqui é uma consulta que casa a pessoa
/// errada, e isso só aparece com a consulta real.</para>
/// </summary>
[Trait("Categoria", "Api")]
public sealed class AutenticacaoTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    // ------------------------------------------------------------------ o destino depois do login

    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("/#/cobertura", "/#/cobertura")]
    [InlineData("/#/clientes/3fa85f64", "/#/clientes/3fa85f64")]
    [InlineData("https://site-falso.com", "/")]
    [InlineData("//site-falso.com", "/")]
    [InlineData("/\\site-falso.com", "/")]
    [InlineData("javascript:alert(1)", "/")]
    [InlineData("/ok\r\nSet-Cookie: roubo=1", "/")]
    public void O_destino_depois_do_login_so_aceita_caminho_da_propria_aplicacao(string? pedido, string esperado)
    {
        RotasDeAutenticacao.DestinoLocal(pedido).Should().Be(esperado,
            "um destino externo faria a tela de login VERDADEIRA da Microsoft entregar a pessoa, já " +
            "autenticada, a um endereço de terceiro");
    }

    // ------------------------------------------------------------------ a sessão com o login desligado

    [Fact]
    public async Task Com_o_login_desligado_a_sessao_responde_provisorio_sem_pedir_cabecalho()
    {
        var resposta = await api.CreateClient().GetAsync("/auth/eu");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK,
            "é essa resposta que faz a tela pular o login quando não há como entrar");

        var corpo = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;
        corpo.GetProperty("modo").GetString().Should().Be("provisorio");
    }

    [Fact]
    public async Task Com_o_login_desligado_a_rota_de_entrar_nao_existe()
    {
        var resposta = await api.CreateClient().GetAsync("/auth/entrar");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "sem registro de aplicativo, publicar 'entrar' levaria a uma tela de erro da Microsoft");
    }

    // ------------------------------------------------------------------ o casamento da conta

    [Fact]
    public async Task Primeiro_login_casa_a_conta_da_carga_pelo_nome_e_grava_o_identificador()
    {
        var id = await SemearAsync("maria.souza" + Usuario.SufixoSemEmail);
        var oid = Guid.NewGuid();

        var resultado = await ResolverAsync(new IdentidadeDoEntra(
            oid, "Maria.Souza@tracbel.com.br", "maria.souza@tracbel.com.br", "Maria Souza"));

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        resultado.Valor.UsuarioId.Should().Be(id);
        resultado.Valor.EmpresaId.Should().Be(1, "sem filial escolhida, vale a filial de casa");

        var gravado = await LerAsync(id);
        gravado.IdentidadeExterna.Should().Be(oid);
        gravado.AindaNaoEntrouPeloEntraId.Should().BeFalse();
        gravado.Email.Endereco.Should().Be("maria.souza@tracbel.com.br");
    }

    [Fact]
    public async Task Depois_do_primeiro_login_o_casamento_e_pelo_identificador_mesmo_que_o_nome_mude()
    {
        var id = await SemearAsync("carla.nunes" + Usuario.SufixoSemEmail);
        var oid = Guid.NewGuid();

        (await ResolverAsync(new IdentidadeDoEntra(oid, "carla.nunes@tracbel.com.br", null, null)))
            .EhSucesso.Should().BeTrue();

        // Casou, trocou de sobrenome: o nome principal muda, o identificador não.
        var depois = await ResolverAsync(new IdentidadeDoEntra(oid, "carla.pires@tracbel.com.br", null, null));

        depois.EhSucesso.Should().BeTrue(depois.Erro ?? string.Empty);
        depois.Valor.UsuarioId.Should().Be(id);
    }

    [Fact]
    public async Task Conta_sem_cadastro_e_recusada_como_nao_encontrada_e_nunca_adivinhada()
    {
        var resultado = await ResolverAsync(new IdentidadeDoEntra(
            Guid.NewGuid(), "ninguem.cadastrado@tracbel.com.br", null, null));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado,
            "o meio de campo traduz isto em 403 — a pessoa provou quem é, o que falta é o cadastro");
    }

    [Fact]
    public async Task Caixa_de_departamento_nunca_vira_login_de_pessoa()
    {
        var id = await SemearAsync("int.mercado" + Usuario.SufixoSemEmail, NaturezaDoUsuario.Departamento);
        var antes = (await LerAsync(id)).IdentidadeExterna;

        var resultado = await ResolverAsync(new IdentidadeDoEntra(
            Guid.NewGuid(), "int.mercado@tracbel.com.br", null, null));

        resultado.EhSucesso.Should().BeFalse(
            "uma caixa com onze carteiras não pode ser herdada por quem tiver o mesmo prefixo de e-mail");
        (await LerAsync(id)).IdentidadeExterna.Should().Be(antes, "nada pode ter sido vinculado");
    }

    [Fact]
    public async Task Conta_que_ja_entrou_nao_e_herdada_por_outra_com_o_mesmo_prefixo()
    {
        // "joao.lima" já entrou: o nome principal é o real, sem o sufixo inventado.
        await SemearAsync("joao.lima@tracbel.com.br");

        var resultado = await ResolverAsync(new IdentidadeDoEntra(
            Guid.NewGuid(), "joao.lima@fornecedor.com.br", null, null));

        resultado.EhSucesso.Should().BeFalse(
            "o casamento por prefixo só vale para conta que nunca entrou — é o que protege a carteira do João");
    }

    [Fact]
    public async Task Usuario_desativado_nao_entra()
    {
        await SemearAsync("ana.dias" + Usuario.SufixoSemEmail, ativo: false);

        var resultado = await ResolverAsync(new IdentidadeDoEntra(
            Guid.NewGuid(), "ana.dias@tracbel.com.br", null, null));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);
    }

    // ------------------------------------------------------------------ o grupo do Entra como portão

    [Fact]
    public async Task Com_o_grupo_configurado_a_conta_sem_cadastro_nasce_aguardando_liberacao()
    {
        var oid = Guid.NewGuid();

        var resultado = await ResolverComGrupoAsync(new IdentidadeDoEntra(
            oid, "Pessoa.Convidada@tracbel.com.br", "pessoa.convidada@tracbel.com.br", "Pessoa Convidada"));

        resultado.EhSucesso.Should().BeFalse("quem espera liberação não vê dado nenhum");
        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado, "o meio de campo traduz isto em 403, com a frase");
        resultado.Erro.Should().Be(ResolvedorDeContextoDoEntraId.MensagemAguardandoLiberacao);

        var criado = await LerPeloIdentificadorAsync(oid);
        criado.Should().NotBeNull("o grupo do Entra é o portão: quem passou por ele ganha o usuário");
        criado!.AguardaLiberacao.Should().BeTrue();
        criado.NomePrincipal.Should().Be("Pessoa.Convidada@tracbel.com.br");
        criado.Email.Endereco.Should().Be("pessoa.convidada@tracbel.com.br");
        criado.NomeExibicao.Should().Be("Pessoa Convidada");
        criado.EmpresaId.Should().Be(1, "a filial provisória é a raiz de menor identificador, e não dá acesso a nada");
    }

    [Fact]
    public async Task A_conta_que_aguarda_continua_recusada_e_nao_e_criada_duas_vezes()
    {
        var identidade = new IdentidadeDoEntra(Guid.NewGuid(), "pessoa.repetida@tracbel.com.br", null, null);

        await ResolverComGrupoAsync(identidade);
        var segunda = await ResolverComGrupoAsync(identidade);

        segunda.Erro.Should().Be(ResolvedorDeContextoDoEntraId.MensagemAguardandoLiberacao);
        (await ContarPeloIdentificadorAsync(identidade.IdentidadeExterna)).Should().Be(1,
            "a tela abre várias chamadas de uma vez; o cadastro nasce uma vez só");
    }

    [Fact]
    public async Task Depois_de_liberada_a_conta_entra_na_filial_escolhida()
    {
        var identidade = new IdentidadeDoEntra(Guid.NewGuid(), "pessoa.liberada@tracbel.com.br", null, "Pessoa Liberada");
        await ResolverComGrupoAsync(identidade);

        await LiberarAsync(identidade.IdentidadeExterna, filialId: 2, administradorId: 100);

        var resultado = await ResolverComGrupoAsync(identidade);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        resultado.Valor.EmpresaId.Should().Be(2, "vale a filial que o administrador escolheu, e não a provisória");
    }

    [Fact]
    public async Task Com_o_grupo_configurado_a_caixa_de_departamento_continua_recusada_e_nada_e_criado()
    {
        await SemearAsync("int.compras" + Usuario.SufixoSemEmail, NaturezaDoUsuario.Departamento);
        var oid = Guid.NewGuid();

        var resultado = await ResolverComGrupoAsync(new IdentidadeDoEntra(oid, "int.compras@tracbel.com.br", null, null));

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().NotBe(ResolvedorDeContextoDoEntraId.MensagemAguardandoLiberacao,
            "a caixa existe e não é login de pessoa: criar outra conta por cima dela seria adivinhar");
        (await LerPeloIdentificadorAsync(oid)).Should().BeNull();
    }

    [Fact]
    public async Task Sem_o_grupo_configurado_nada_e_criado()
    {
        var oid = Guid.NewGuid();

        await ResolverAsync(new IdentidadeDoEntra(oid, "pessoa.sem.grupo@tracbel.com.br", null, null));

        (await LerPeloIdentificadorAsync(oid)).Should().BeNull(
            "sem o grupo, qualquer conta do locatário passaria pelo login; criar usuário para cada uma encheria a lista");
    }

    // ------------------------------------------------------------------ apoio

    private async Task<Resultado<ContextoAcesso>> ResolverAsync(IdentidadeDoEntra identidade, string? filial = null)
    {
        using var escopo = api.Services.CreateScope();
        var resolvedor = escopo.ServiceProvider.GetRequiredService<ResolvedorDeContextoDoEntraId>();
        return await resolvedor.ResolverAsync(identidade, filial, CancellationToken.None);
    }

    /// <summary>O resolvedor como ele roda no servidor: com <c>Entra:GrupoPermitido</c> configurado.</summary>
    private async Task<Resultado<ContextoAcesso>> ResolverComGrupoAsync(IdentidadeDoEntra identidade)
    {
        using var escopo = api.Services.CreateScope();
        var servicos = escopo.ServiceProvider;
        var resolvedor = new ResolvedorDeContextoDoEntraId(
            servicos.GetRequiredService<DbContextOptions<CrmDbContext>>(),
            servicos.GetRequiredService<IOptions<OpcoesDeContextoProvisorio>>(),
            Options.Create(new OpcoesDoPrimeiroLogin { CriarUsuarioAguardandoLiberacao = true }),
            NullLogger<ResolvedorDeContextoDoEntraId>.Instance);

        return await resolvedor.ResolverAsync(identidade, null, CancellationToken.None);
    }

    private async Task<Usuario?> LerPeloIdentificadorAsync(Guid oid)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await db.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.IdentidadeExterna == oid);
    }

    private async Task<int> ContarPeloIdentificadorAsync(Guid oid)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await db.Usuarios.CountAsync(u => u.IdentidadeExterna == oid);
    }

    private async Task LiberarAsync(Guid oid, int filialId, long administradorId)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        var usuario = await db.Usuarios.SingleAsync(u => u.IdentidadeExterna == oid);
        usuario.Liberar(filialId, administradorId);
        await db.SaveChangesAsync();
    }

    private async Task<long> SemearAsync(
        string nomePrincipal, NaturezaDoUsuario natureza = NaturezaDoUsuario.Pessoa, bool ativo = true)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        var usuario = Usuario.Criar(
            identidadeExterna: Guid.NewGuid(),
            nomePrincipal: nomePrincipal,
            nomeCompleto: nomePrincipal,
            nomeExibicao: nomePrincipal.Split('@')[0],
            email: Email.Criar(nomePrincipal),
            empresaId: 1,
            criadoPorId: 100,
            estaAtivo: ativo,
            desativadoEm: ativo ? null : DateTime.UtcNow,
            natureza: natureza);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario.Id;
    }

    private async Task<Usuario> LerAsync(long id)
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        return await db.Usuarios.AsNoTracking().SingleAsync(u => u.Id == id);
    }
}
