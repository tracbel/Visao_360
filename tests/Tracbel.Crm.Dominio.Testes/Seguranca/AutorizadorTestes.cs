using FluentAssertions;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Seguranca;

/// <summary>
/// A MATRIZ DE AUTORIZAÇÃO.
///
/// O portão da fase 1 exige a matriz completa: cada perfil × cada entidade × cada verbo,
/// mais os testes de escape. Estes testes são o núcleo dela.
///
/// [V] O que estamos provando que existe é justamente o que o Vórtice NÃO tem: permissão
/// por registro. Lá não há RLS no SQL Server (sys.security_policies = 0), há um único trigger
/// no banco inteiro, e toda a segurança roda no cliente Gupta — quem chega ao banco vê tudo
/// e não deixa rastro.
/// </summary>
[Trait("Categoria", "Autorizacao")]
public class AutorizadorTestes
{
    private const long Eu = 100;
    private const long Colega = 200;
    private const long MeuSubordinado = 300;
    private const long Estranho = 400;

    private const int MinhaEmpresa = 1;
    private const int EmpresaFilha = 2;
    private const int OutraEmpresa = 9;

    private readonly Autorizador _autorizador = new();

    private static ContextoAcesso Contexto(
        Profundidade profundidadeLer,
        Profundidade? profundidadeEditar = null,
        bool ehSistema = false)
    {
        var profundidades = new Dictionary<string, Profundidade>
        {
            ["Processo.Ler"] = profundidadeLer,
            ["Processo.Editar"] = profundidadeEditar ?? profundidadeLer
        };

        return new ContextoAcesso(
            usuarioId: Eu,
            nomeExibicao: "Ricardo",
            empresaId: MinhaEmpresa,
            empresasVisiveis: new HashSet<int> { MinhaEmpresa, EmpresaFilha },
            subordinadosIds: new HashSet<long> { MeuSubordinado },
            equipesIds: new HashSet<long> { 7 },
            profundidades: profundidades,
            ehServicoDeSistema: ehSistema);
    }

    // ---------------------------------------------------------- Camada 2: o teto

    [Fact]
    public void Sem_a_permissao_nega_antes_de_olhar_a_linha()
    {
        var ctx = Contexto(Profundidade.Organizacao);

        // Ele tem Processo.Ler com alcance total, mas NÃO tem Processo.Excluir.
        var d = _autorizador.Pode(ctx, "Processo", Verbos.Excluir, 1, Eu, MinhaEmpresa);

        d.Permitido.Should().BeFalse();
        d.Camada.Should().Be(2, "a permissão de entidade é o teto absoluto");
        d.Motivo.Should().Contain("Processo.Excluir");
    }

    // ---------------------------------------------------------- Camada 3: profundidade

    [Theory]
    // profundidade,                 dono do registro, empresa do registro, permite?
    [InlineData(Profundidade.Proprios,       Eu,             MinhaEmpresa, true)]
    [InlineData(Profundidade.Proprios,       Colega,         MinhaEmpresa, false)]
    [InlineData(Profundidade.Proprios,       MeuSubordinado, MinhaEmpresa, false)]

    [InlineData(Profundidade.Equipe,         Eu,             MinhaEmpresa, true)]
    [InlineData(Profundidade.Equipe,         MeuSubordinado, MinhaEmpresa, true)]
    [InlineData(Profundidade.Equipe,         Colega,         MinhaEmpresa, false)]

    [InlineData(Profundidade.Empresa,        Colega,         MinhaEmpresa, true)]
    [InlineData(Profundidade.Empresa,        Estranho,       OutraEmpresa, false)]
    [InlineData(Profundidade.Empresa,        Colega,         EmpresaFilha, false)]

    [InlineData(Profundidade.EmpresaEAbaixo, Colega,         EmpresaFilha, true)]
    [InlineData(Profundidade.EmpresaEAbaixo, Estranho,       OutraEmpresa, false)]

    [InlineData(Profundidade.Organizacao,    Estranho,       OutraEmpresa, true)]
    public void Profundidade_decide_quais_linhas_o_usuario_alcanca(
        Profundidade profundidade, long dono, int empresaDoRegistro, bool esperado)
    {
        var ctx = Contexto(profundidade);

        var d = _autorizador.Pode(ctx, "Processo", Verbos.Ler, 1, dono, empresaDoRegistro);

        d.Permitido.Should().Be(esperado,
            "com alcance {0}, um registro de {1} na empresa {2} deveria {3}",
            profundidade, dono, empresaDoRegistro, esperado ? "ser visível" : "ficar oculto");
    }

    [Fact]
    public void Profundidade_e_por_permissao_nao_por_usuario()
    {
        // O caso real: o CEN vê o processo do colega, mas não altera.
        // [V] Essa granularidade não existe no Vórtice — lá as permissões são 51 flags
        // char(1) em IV_Operador, produzindo 370 combinações distintas para 939 usuários.
        var ctx = Contexto(
            profundidadeLer: Profundidade.Equipe,
            profundidadeEditar: Profundidade.Proprios);

        _autorizador.Pode(ctx, "Processo", Verbos.Ler, 1, MeuSubordinado, MinhaEmpresa)
            .Permitido.Should().BeTrue("ele lê o do subordinado");

        _autorizador.Pode(ctx, "Processo", Verbos.Editar, 1, MeuSubordinado, MinhaEmpresa)
            .Permitido.Should().BeFalse("mas não altera");
    }

    // ---------------------------------------------------------- Camada 4: compartilhamento

    [Fact]
    public void Compartilhamento_abre_o_que_a_profundidade_nao_alcanca()
    {
        var ctx = Contexto(Profundidade.Proprios);
        var shares = new[]
        {
            new CompartilhamentoAtivo("Processo", 42, PermiteEditar: false, Motivo: "Regra")
        };

        var d = _autorizador.Pode(ctx, "Processo", Verbos.Ler, 42, Estranho, OutraEmpresa, shares);

        d.Permitido.Should().BeTrue();
        d.Camada.Should().Be(4);
        d.Motivo.Should().Contain("regra de automação");
    }

    [Fact]
    public void Compartilhamento_de_leitura_nao_autoriza_edicao()
    {
        var ctx = Contexto(Profundidade.Proprios);
        var shares = new[]
        {
            new CompartilhamentoAtivo("Processo", 42, PermiteEditar: false, Motivo: "Manual")
        };

        _autorizador.Pode(ctx, "Processo", Verbos.Ler, 42, Estranho, OutraEmpresa, shares)
            .Permitido.Should().BeTrue();

        _autorizador.Pode(ctx, "Processo", Verbos.Editar, 42, Estranho, OutraEmpresa, shares)
            .Permitido.Should().BeFalse("compartilhamento de leitura não vira edição");
    }

    [Fact]
    public void Compartilhamento_de_outro_registro_nao_vale()
    {
        // Teste de escape: o share é do registro 42, o acesso é ao 43.
        var ctx = Contexto(Profundidade.Proprios);
        var shares = new[]
        {
            new CompartilhamentoAtivo("Processo", 42, PermiteEditar: true, Motivo: "Manual")
        };

        _autorizador.Pode(ctx, "Processo", Verbos.Ler, 43, Estranho, OutraEmpresa, shares)
            .Permitido.Should().BeFalse();
    }

    [Fact]
    public void Compartilhamento_de_outra_entidade_nao_vale()
    {
        var ctx = Contexto(Profundidade.Proprios);
        var shares = new[]
        {
            new CompartilhamentoAtivo("Conta", 42, PermiteEditar: true, Motivo: "Manual")
        };

        _autorizador.Pode(ctx, "Processo", Verbos.Ler, 42, Estranho, OutraEmpresa, shares)
            .Permitido.Should().BeFalse("o share é de Conta, não de Processo");
    }

    // ---------------------------------------------------------- Explicação

    [Fact]
    public void A_negativa_diz_o_que_falta_nao_so_que_negou()
    {
        // Mensagem que evita o chamado. [V] o Vórtice devolve
        // {"Message":"An error has occurred."} para praticamente tudo.
        var ctx = Contexto(Profundidade.Proprios);

        var d = _autorizador.Pode(ctx, "Processo", Verbos.Ler, 1, Colega, MinhaEmpresa);

        d.Permitido.Should().BeFalse();
        d.Motivo.Should().Contain("apenas os seus registros");
        d.Motivo.Should().Contain("Peça o compartilhamento");
    }

    [Theory]
    [InlineData(Profundidade.Proprios, Eu, "Você é o proprietário")]
    [InlineData(Profundidade.Equipe, MeuSubordinado, "subordinado a você")]
    [InlineData(Profundidade.Empresa, Colega, "da sua empresa")]
    [InlineData(Profundidade.Organizacao, Estranho, "toda a organização")]
    public void A_positiva_explica_por_que_o_usuario_enxerga(
        Profundidade profundidade, long dono, string trechoEsperado)
    {
        // [DYN] A Microsoft precisou de uma API inteira (RetrieveAccessOrigin) para
        // responder isso. Aqui sai junto com a decisão.
        var ctx = Contexto(profundidade);
        var empresa = profundidade == Profundidade.Organizacao ? OutraEmpresa : MinhaEmpresa;

        var d = _autorizador.Pode(ctx, "Processo", Verbos.Ler, 1, dono, empresa);

        d.Permitido.Should().BeTrue();
        d.Motivo.Should().Contain(trechoEsperado);
    }

    // ---------------------------------------------------------- Sistema

    [Fact]
    public void Servico_de_sistema_alcanca_tudo_mas_fica_marcado()
    {
        var ctx = Contexto(Profundidade.Nenhum, ehSistema: true);

        var d = _autorizador.Pode(ctx, "Processo", Verbos.Excluir, 1, Estranho, OutraEmpresa);

        d.Permitido.Should().BeTrue();
        d.Motivo.Should().Contain("aud.EventoAcesso", "o acesso de sistema precisa deixar rastro");
    }

    // ---------------------------------------------------------- ExigirPermissao

    [Fact]
    public void ExigirPermissao_lanca_AcessoNegado_e_nao_excecao_generica()
    {
        // AcessoNegado vira HTTP 403. Nunca 500.
        var ctx = Contexto(Profundidade.Proprios);

        var acao = () => _autorizador.ExigirPermissao(
            ctx, "Processo", Verbos.Editar, 1, Colega, MinhaEmpresa);

        acao.Should().Throw<AcessoNegado>().WithMessage("*apenas os seus registros*");
    }
}

/// <summary>
/// O modelo de perfis (fase 3): composição, regra aditiva, catálogo em código e concessão.
/// </summary>
[Trait("Categoria", "Autorizacao")]
public class PerfilTestes
{
    [Fact]
    public void Perfis_sao_aditivos_e_vence_a_maior_profundidade()
    {
        // [SF] "Most permissive wins" dos permission sets.
        var perfil = Perfil.Criar("CEN", "Consultor de vendas")
            .Conceder(Permissoes.ProcessoLer, Profundidade.Proprios)
            .Conceder(Permissoes.ProcessoLer, Profundidade.Equipe);

        perfil.Permissoes.Should().ContainSingle();
        perfil.Permissoes.Single().Profundidade.Should().Be(Profundidade.Equipe);
    }

    [Fact]
    public void Conceder_com_profundidade_maior_amplia_conceder_com_menor_nao_reduz()
    {
        var perfil = Perfil.Criar("X", "X")
            .Conceder(Permissoes.ProcessoLer, Profundidade.Empresa)
            .Conceder(Permissoes.ProcessoLer, Profundidade.Proprios);

        perfil.Permissoes.Single().Profundidade.Should().Be(Profundidade.Empresa,
            "o modelo é aditivo — conceder de novo nunca reduz");
    }

    [Fact]
    public void Nao_existe_conceder_com_profundidade_Nenhum()
    {
        // Não há regra de negação, de propósito. Para negar, não conceda.
        var acao = () => Perfil.Criar("X", "X").Conceder(Permissoes.ProcessoLer, Profundidade.Nenhum);

        acao.Should().Throw<Dominio.Comum.RegraDeNegocioViolada>().WithMessage("*não tem regra de negação*");
    }

    [Fact]
    public void So_se_concede_permissao_que_existe_no_catalogo()
    {
        // D-3: o catálogo é o código. Um nome digitado errado viraria permissão que nenhuma rota confere.
        var acao = () => Perfil.Criar("X", "X").Conceder("Cliente.Apagar", Profundidade.Empresa);

        acao.Should().Throw<Dominio.Comum.RegraDeNegocioViolada>().WithMessage("*não existe no catálogo*");
    }

    [Fact]
    public void Concessao_temporaria_expira()
    {
        // Cobertura de férias que não vira privilégio permanente.
        var agora = DateTime.UtcNow;

        var vigente = UsuarioPerfil.Conceder(1, 1, "férias do titular", 99, agora, expiraEm: agora.AddDays(15));
        var permanente = UsuarioPerfil.Conceder(1, 2, "gerente da filial", 99, agora);

        vigente.EstaVigente(agora).Should().BeTrue();
        vigente.EstaVigente(agora.AddDays(20)).Should().BeFalse();
        permanente.EstaVigente(agora.AddYears(5)).Should().BeTrue();
    }

    [Fact]
    public void Concessao_nao_aceita_expiracao_no_passado_nem_falta_de_justificativa()
    {
        var agora = DateTime.UtcNow;

        var noPassado = () => UsuarioPerfil.Conceder(1, 1, "x", 99, agora, expiraEm: agora.AddDays(-1));
        var semMotivo = () => UsuarioPerfil.Conceder(1, 1, " ", 99, agora);

        noPassado.Should().Throw<Dominio.Comum.RegraDeNegocioViolada>();
        semMotivo.Should().Throw<Dominio.Comum.RegraDeNegocioViolada>().WithMessage("*justificativa*");
    }

    [Fact]
    public void O_perfil_padrao_semeado_nao_exclui_nem_abre_a_visao_entre_filiais()
    {
        // Q-P2 (21/09/2026): o mínimo, sem excluir. Até a fase 3, todos podiam excluir sem ninguém ter decidido.
        var padrao = PerfisDeSistema.Todos.Single(p => p.EhPadrao);
        var codigos = padrao.Permissoes.Select(p => p.Codigo).ToList();

        codigos.Should().Contain([Permissoes.ClienteLer, Permissoes.ClienteCriar, Permissoes.ClienteEditar, Permissoes.EquipamentoEditar]);
        codigos.Should().NotContain([Permissoes.ClienteExcluir, Permissoes.EquipamentoExcluir, Permissoes.EmpresaAlcanceEntreFiliais]);
        codigos.Should().NotContain("Lead.Ler", "a entidade Lead saiu na fase 1, e a permissão residual com ela");
    }

    [Fact]
    public void O_administrador_semeado_tem_todas_as_permissoes_do_catalogo_em_toda_a_organizacao()
    {
        // 21/09/2026: "o perfil admin tem todas as filiais e todos os recursos". Uma permissão nova no catálogo
        // que não entrar na semente do administrador quebra este teste — e é para quebrar.
        var administrador = PerfisDeSistema.Todos.Single(p => p.Codigo == PerfisDeSistema.Administrador);

        administrador.Permissoes.Select(p => p.Codigo).Should().BeEquivalentTo(Permissoes.Catalogo.Keys);
        administrador.Permissoes.Should().OnlyContain(p => p.Profundidade == Profundidade.Organizacao);
    }

    [Fact]
    public void Todas_as_filiais_e_um_codigo_que_nao_colide_com_filial_e_nao_e_o_padrao()
    {
        ContextoAcesso.CodigoDeTodasAsFiliais.Should().NotMatchRegex("^[0-9]+$", "os códigos de filial são só dígitos (010101)");

        var contexto = new ContextoAcesso(1, "x", 1, new HashSet<int> { 1 }, new HashSet<long>(), new HashSet<long>(),
            new Dictionary<string, Profundidade>());
        contexto.VeTodasAsFiliais.Should().BeFalse("olhar todas as filiais é escolha, nunca o padrão");
    }

    [Fact]
    public void Todo_perfil_semeado_usa_so_permissoes_do_catalogo()
    {
        foreach (var perfil in PerfisDeSistema.Todos)
            perfil.Permissoes.Should().OnlyContain(p => Permissoes.Existe(p.Codigo), $"o perfil {perfil.Codigo} é semeado do código");
    }
}