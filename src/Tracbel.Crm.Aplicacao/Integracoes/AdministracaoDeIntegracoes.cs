using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Integracoes;

/// <summary>Uma conexão, como a tela mostra. Nunca traz a senha: só se há uma, de onde vem e quem gravou.</summary>
public sealed record ConexaoNaTela(
    string Codigo, string Nome, string? Descricao, string Tipo, bool EhDoSistema, bool EstaAtiva,
    string? Endereco, int? Porta, string? Banco, string? Objeto, string? Usuario, string? NomeDoCabecalho,
    int StatusEsperado, int? MinutosEntreVerificacoes, bool EnderecoEditavel, bool AceitaSegredo,
    string OrigemDaCredencial, bool TemSegredoNaTela, DateTime? SegredoAlteradoEm, string? SegredoAlteradoPor,
    DateTime? UltimaVerificacaoEm, bool? UltimaVerificacaoOk, string? UltimaVerificacaoResumo, IReadOnlyList<string> Rotinas);

/// <summary>Uma rotina, como a tela mostra.</summary>
public sealed record RotinaNaTela(
    string Codigo, string Nome, string Descricao, IReadOnlyList<string> Cargas, string Cadencia, int? Mes, int? Dia, string? Hora,
    int? IntervaloMinutos, string Agenda, bool EstaLigada, DateTime? ProximaExecucaoEm, bool NaFila, DateTime? ExecucaoPedidaEm,
    string? ExecucaoPedidaPor, DateTime? UltimaExecucaoIniciadaEm, DateTime? UltimaExecucaoTerminadaEm, string? UltimoResultado,
    string? UltimaMensagem, IReadOnlyList<string> Conexoes, string? Pendencia,
    short? AnoInicialDoHistorico, bool AceitaAnoInicialDoHistorico);

/// <summary>O painel de Integrações.</summary>
/// <param name="Conexoes">As conexões: as do sistema, depois as cadastradas pela tela.</param>
/// <param name="Rotinas">As rotinas do servidor.</param>
/// <param name="PodeAdministrar">Se quem pergunta tem <c>Integracao.Administrar</c>.</param>
public sealed record PainelDeIntegracoes(IReadOnlyList<ConexaoNaTela> Conexoes, IReadOnlyList<RotinaNaTela> Rotinas, bool PodeAdministrar);

/// <summary>O resultado do botão "Testar", com a conexão já atualizada.</summary>
public sealed record TesteDaConexaoNaTela(bool Ok, string Resumo, int LatenciaMs, DateTime VerificadaEm, ConexaoNaTela Conexao);

/// <summary>O corpo da configuração de uma conexão. A senha vai à parte.</summary>
public sealed record ConfiguracaoDeConexao(
    string? Endereco, int? Porta, string? Banco, string? Objeto, string? Usuario, string? NomeDoCabecalho,
    string? Nome, string? Descricao, int? StatusEsperado, int? MinutosEntreVerificacoes);

/// <summary>O corpo do cadastro de uma API monitorada.</summary>
public sealed record NovaConexaoMonitorada(
    string? Codigo, string? Nome, string? Descricao, string? Endereco, string? NomeDoCabecalho, int? StatusEsperado, int? MinutosEntreVerificacoes);

/// <summary>O corpo da credencial: a senha, ou o segredo do cabeçalho. Nunca volta.</summary>
public sealed record SegredoDaConexao(string? Segredo);

/// <summary>O corpo da agenda de uma rotina.</summary>
public sealed record AgendaNaTela(string? Cadencia, int? Mes, int? Dia, string? Hora, int? IntervaloMinutos, bool? Ligada, short? AnoInicialDoHistorico);

/// <summary>
/// MONTA O QUE A TELA MOSTRA — a mesma conta para a lista e para a resposta de cada ação, para a tela nunca ver
/// uma conexão de um jeito na lista e de outro depois de salvar.
/// </summary>
public sealed class MontadorDoPainelDeIntegracoes(
    IRepositorioDeConexoes conexoes, IRepositorioDeRotinas rotinas, IConsultaDoHistoricoDeIntegracoes historico,
    IResolvedorDeConexoes resolvedor, IRelogio relogio)
{
    /// <summary>O painel inteiro.</summary>
    public async Task<PainelDeIntegracoes> MontarAsync(bool podeAdministrar, CancellationToken ct)
    {
        var todasAsConexoes = await conexoes.ListarAsync(ct);
        var todasAsRotinas = await rotinas.ListarAsync(ct);

        var ids = todasAsConexoes.Where(c => c.SegredoAlteradoPorId is not null).Select(c => c.SegredoAlteradoPorId!.Value)
            .Concat(todasAsRotinas.Where(r => r.ExecucaoPedidaPorId is not null).Select(r => r.ExecucaoPedidaPorId!.Value))
            .ToList();
        var nomes = await historico.NomesDosUsuariosAsync(ids, ct);
        var origens = todasAsConexoes.ToDictionary(c => c.Codigo, resolvedor.OrigemDe, StringComparer.Ordinal);

        return new PainelDeIntegracoes(
            todasAsConexoes.Select(c => ConexaoParaATela(c, origens[c.Codigo], nomes)).ToList(),
            todasAsRotinas.Select(r => RotinaParaATela(r, origens, todasAsConexoes, nomes)).ToList(),
            podeAdministrar);
    }

    /// <summary>Uma conexão, depois de uma ação.</summary>
    public async Task<ConexaoNaTela> ConexaoAsync(string codigo, CancellationToken ct) =>
        (await MontarAsync(true, ct)).Conexoes.First(c => string.Equals(c.Codigo, codigo, StringComparison.Ordinal));

    /// <summary>Uma rotina, depois de uma ação.</summary>
    public async Task<RotinaNaTela> RotinaAsync(string codigo, CancellationToken ct) =>
        (await MontarAsync(true, ct)).Rotinas.First(r => string.Equals(r.Codigo, codigo, StringComparison.Ordinal));

    private static ConexaoNaTela ConexaoParaATela(Conexao c, OrigemDaCredencial origem, IReadOnlyDictionary<long, string> nomes) => new(
        c.Codigo, c.Nome, c.Descricao, c.Tipo.ToString(), c.EhDoSistema, c.EstaAtiva, c.Endereco, c.Porta, c.Banco, c.Objeto,
        // O USUÁRIO DO AMBIENTE NÃO APARECE: a tela mostra o que ela mesma gravou, não o que mora no servidor.
        c.Usuario,
        c.NomeDoCabecalho, c.StatusEsperado, c.MinutosEntreVerificacoes, c.EnderecoEditavel, c.AceitaSegredo,
        origem.ToString(), c.TemSegredo, c.SegredoAlteradoEm,
        c.SegredoAlteradoPorId is { } id ? nomes.GetValueOrDefault(id, $"usuário {id}") : null,
        c.UltimaVerificacaoEm, c.UltimaVerificacao == SituacaoDaVerificacao.NuncaVerificada ? null : c.UltimaVerificacao == SituacaoDaVerificacao.NoAr, c.UltimaVerificacaoResumo,
        RotinasDoSistema.Todas.Where(r => r.Conexoes.Contains(c.Codigo, StringComparer.Ordinal)).Select(r => r.Nome).ToList());

    private RotinaNaTela RotinaParaATela(
        Rotina r, IReadOnlyDictionary<string, OrigemDaCredencial> origens, IReadOnlyList<Conexao> todasAsConexoes, IReadOnlyDictionary<long, string> nomes)
    {
        var agora = relogio.Agora;
        var catalogo = RotinasDoSistema.Obter(r.Codigo);
        var exigida = catalogo?.ConexaoExigida;
        var pendencia = exigida is not null && origens.GetValueOrDefault(exigida, OrigemDaCredencial.Nenhuma) == OrigemDaCredencial.Nenhuma
            ? $"Falta a credencial de {todasAsConexoes.FirstOrDefault(c => c.Codigo == exigida)?.Nome ?? exigida}: configure-a acima. Sem ela, a rotina não roda."
            : null;

        return new RotinaNaTela(
            r.Codigo, r.Nome, catalogo?.Descricao ?? string.Empty, catalogo?.Modos ?? [], r.Cadencia.ToString(), r.Mes, r.Dia,
            r.Hora?.ToString("HH:mm", CultureInfo.InvariantCulture), r.IntervaloMinutos, r.Agenda.Descrever(), r.EstaLigada,
            r.ProximaExecucao(agora), r.ExecucaoPedidaEm is not null, r.ExecucaoPedidaEm,
            r.ExecucaoPedidaPorId is { } id ? nomes.GetValueOrDefault(id, $"usuário {id}") : null,
            r.UltimaExecucaoIniciadaEm, r.UltimaExecucaoTerminadaEm, r.UltimoResultado?.ToString(), r.UltimaMensagem,
            catalogo?.Conexoes ?? [], pendencia,
            r.AnoInicialDoHistorico, catalogo?.AnoInicialPadraoDoHistorico is not null);
    }
}

/// <summary>O painel de Integrações (<c>Integracao.Ler</c>).</summary>
public sealed class ListarIntegracoes(MontadorDoPainelDeIntegracoes montador, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    public async Task<Resultado<ComProcedencia<PainelDeIntegracoes>>> ExecutarAsync(CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoLer) && !acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return RegrasDasIntegracoes.SemPermissao<ComProcedencia<PainelDeIntegracoes>>(Permissoes.IntegracaoLer);

        var painel = await montador.MontarAsync(acesso.Atual.Tem(Permissoes.IntegracaoAdministrar), ct);
        return Resultado<ComProcedencia<PainelDeIntegracoes>>.Ok(
            ComProcedencia<PainelDeIntegracoes>.DoNossoBanco(painel, "integracao.Conexao + integracao.Rotina", relogio));
    }
}

/// <summary>O histórico de uma conexão e de uma rotina (<c>Integracao.Ler</c>).</summary>
public sealed class ListarHistoricoDeIntegracoes(IConsultaDoHistoricoDeIntegracoes historico, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    private const int Quantos = 20;

    /// <summary>Os últimos testes de uma conexão.</summary>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<VerificacaoNaTela>>>> VerificacoesAsync(string codigo, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoLer) && !acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return RegrasDasIntegracoes.SemPermissao<ComProcedencia<IReadOnlyList<VerificacaoNaTela>>>(Permissoes.IntegracaoLer);

        var testes = await historico.ListarVerificacoesAsync(codigo.Trim(), Quantos, ct);
        return Resultado<ComProcedencia<IReadOnlyList<VerificacaoNaTela>>>.Ok(
            ComProcedencia<IReadOnlyList<VerificacaoNaTela>>.DoNossoBanco(testes, "integracao.VerificacaoDeConexao", relogio));
    }

    /// <summary>As últimas execuções de uma rotina.</summary>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<ExecucaoNaTela>>>> ExecucoesAsync(string codigo, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoLer) && !acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return RegrasDasIntegracoes.SemPermissao<ComProcedencia<IReadOnlyList<ExecucaoNaTela>>>(Permissoes.IntegracaoLer);

        var execucoes = await historico.ListarExecucoesAsync(codigo.Trim(), Quantos, ct);
        return Resultado<ComProcedencia<IReadOnlyList<ExecucaoNaTela>>>.Ok(
            ComProcedencia<IReadOnlyList<ExecucaoNaTela>>.DoNossoBanco(execucoes, "integracao.ExecucaoDeRotina", relogio));
    }
}

/// <summary>
/// CONFIGURAR, GUARDAR A CREDENCIAL, CADASTRAR E TESTAR CONEXÕES (issue 136) — <c>Integracao.Administrar</c>.
///
/// <para><b>A senha entra e não sai.</b> Ela chega por uma rota própria, é protegida na hora e gravada; nenhuma
/// resposta a traz de volta, e nenhuma mensagem a cita (o teste passa pelo <c>Sigilo</c>). Mandar vazio não apaga:
/// retirar a credencial é uma ação à parte, para ninguém apagar a senha ao salvar o endereço.</para>
/// </summary>
public sealed class AdministrarConexoes(
    IRepositorioDeConexoes repositorio, IUnidadeDeTrabalho unidade, IProtetorDeSegredos protetor, IResolvedorDeConexoes resolvedor,
    ITestadorDeConexoes testador, MontadorDoPainelDeIntegracoes montador, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Troca o endereço, o banco, a visão e o usuário; na monitorada, também o nome e o monitoramento.</summary>
    public async Task<Resultado<ConexaoNaTela>> ConfigurarAsync(string codigo, ConfiguracaoDeConexao entrada, CancellationToken ct)
    {
        var (conexao, recusa) = await ParaAlterarAsync(codigo, ct);
        if (recusa is { } recusada) return recusada;

        var erros = new ColetorDeErros();
        foreach (var (campo, mensagem) in Dominio.Integracao.Conexao.ProblemasDoEndereco(
                     conexao!.Tipo, entrada.Endereco, entrada.Porta, entrada.Banco, entrada.Objeto, entrada.NomeDoCabecalho))
            erros.Registrar(campo, mensagem, campo switch { "porta" => entrada.Porta?.ToString(CultureInfo.InvariantCulture), "banco" => entrada.Banco, "objeto" => entrada.Objeto, _ => null });

        if (conexao.Tipo == TipoDeConexao.Monitorada)
        {
            if (string.IsNullOrWhiteSpace(entrada.Nome)) erros.Registrar("nome", "Informe o nome da API.");
            foreach (var (campo, mensagem) in Dominio.Integracao.Conexao.ProblemasDaMonitorada(
                         entrada.Endereco, entrada.NomeDoCabecalho, entrada.StatusEsperado ?? 200, entrada.MinutosEntreVerificacoes)
                         .Where(p => p.Campo is "statusEsperado" or "minutosEntreVerificacoes"))
                erros.Registrar(campo, mensagem);
        }
        else if (conexao.Tipo != TipoDeConexao.FontePublica && string.IsNullOrWhiteSpace(entrada.Usuario))
        {
            erros.Registrar("usuario", "Informe o usuário.");
        }

        if (erros.TemErro) return erros.Recusar<ConexaoNaTela>("A conexão tem campos a corrigir.");

        try
        {
            conexao.Configurar(entrada.Endereco, entrada.Porta, entrada.Banco, entrada.Objeto, entrada.Usuario, entrada.NomeDoCabecalho);
            if (conexao.Tipo == TipoDeConexao.Monitorada)
                conexao.AjustarMonitoramento(entrada.Nome!, entrada.Descricao, entrada.StatusEsperado ?? 200, entrada.MinutosEntreVerificacoes);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ConexaoNaTela>.Conflito(erro.Message);
        }

        return await GravarEDevolverAsync(conexao.Codigo, ct);
    }

    /// <summary>Grava a credencial, protegida. A tela nunca a recebe de volta.</summary>
    public async Task<Resultado<ConexaoNaTela>> DefinirSegredoAsync(string codigo, SegredoDaConexao entrada, CancellationToken ct)
    {
        var (conexao, recusa) = await ParaAlterarAsync(codigo, ct);
        if (recusa is { } recusada) return recusada;

        if (!conexao!.AceitaSegredo)
            return Resultado<ConexaoNaTela>.Conflito("Esta conexão é uma fonte pública aberta: não tem credencial.");

        if (string.IsNullOrEmpty(entrada.Segredo) || entrada.Segredo.Length > 1000)
        {
            var erros = new ColetorDeErros();
            // O VALOR RECEBIDO NÃO VOLTA NA RECUSA: nem a senha errada aparece na resposta.
            erros.Registrar("segredo", "Informe a senha (até 1.000 caracteres). Para retirar a credencial, use \"Retirar credencial\".");
            return erros.Recusar<ConexaoNaTela>("A credencial não foi gravada.");
        }

        conexao.DefinirSegredo(protetor.Proteger(entrada.Segredo), acesso.Atual.UsuarioId, relogio.Agora);
        return await GravarEDevolverAsync(conexao.Codigo, ct);
    }

    /// <summary>Retira a credencial da tela; a conexão volta a usar a do ambiente do servidor, se houver.</summary>
    public async Task<Resultado<ConexaoNaTela>> RemoverSegredoAsync(string codigo, CancellationToken ct)
    {
        var (conexao, recusa) = await ParaAlterarAsync(codigo, ct);
        if (recusa is { } recusada) return recusada;

        conexao!.RemoverSegredo(acesso.Atual.UsuarioId, relogio.Agora);
        return await GravarEDevolverAsync(conexao.Codigo, ct);
    }

    /// <summary>Cadastra uma API só para ser monitorada — GET, status esperado e, se pedir, um segredo no cabeçalho.</summary>
    public async Task<Resultado<ConexaoNaTela>> CriarMonitoradaAsync(NovaConexaoMonitorada entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return RegrasDasIntegracoes.SemPermissao<ConexaoNaTela>(Permissoes.IntegracaoAdministrar);

        var erros = new ColetorDeErros();
        var codigo = erros.Obrigatorio("codigo", entrada.Codigo, "o código da API").ToUpperInvariant();
        var nome = erros.Obrigatorio("nome", entrada.Nome, "o nome da API");

        if (codigo.Length > 0 && !Dominio.Integracao.Conexao.FormatoDoCodigo().IsMatch(codigo))
            erros.Registrar("codigo", "Use de 3 a 40 caracteres: letras maiúsculas, números e sublinhado, começando por letra (ex.: CLIMA_TEMPO).", entrada.Codigo);
        else if (codigo.Length > 0 && await repositorio.ObterParaAlterarAsync(codigo, ct) is not null)
            erros.Registrar("codigo", "Já existe conexão com este código.", entrada.Codigo);

        foreach (var (campo, mensagem) in Dominio.Integracao.Conexao.ProblemasDaMonitorada(
                     entrada.Endereco, entrada.NomeDoCabecalho, entrada.StatusEsperado ?? 200, entrada.MinutosEntreVerificacoes))
            erros.Registrar(campo, mensagem);

        if (erros.TemErro) return erros.Recusar<ConexaoNaTela>("A API tem campos a corrigir.");

        var conexao = Dominio.Integracao.Conexao.CriarMonitorada(codigo, nome, entrada.Descricao, entrada.Endereco!, entrada.StatusEsperado ?? 200, entrada.MinutosEntreVerificacoes);
        conexao.Configurar(entrada.Endereco, null, null, null, null, entrada.NomeDoCabecalho);
        await repositorio.AdicionarAsync(conexao, ct);
        return await GravarEDevolverAsync(codigo, ct);
    }

    /// <summary>Tira uma API monitorada do monitoramento, sem apagar.</summary>
    public Task<Resultado<ConexaoNaTela>> DesativarAsync(string codigo, CancellationToken ct) => LigarOuDesligarAsync(codigo, ligar: false, ct);

    /// <summary>Volta a monitorar.</summary>
    public Task<Resultado<ConexaoNaTela>> ReativarAsync(string codigo, CancellationToken ct) => LigarOuDesligarAsync(codigo, ligar: true, ct);

    /// <summary>
    /// O BOTÃO "TESTAR": resolve a credencial (tela, depois ambiente), testa no servidor, só leitura, e guarda o
    /// resultado no histórico — com quem apertou.
    /// </summary>
    public async Task<Resultado<TesteDaConexaoNaTela>> TestarAsync(string codigo, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return RegrasDasIntegracoes.SemPermissao<TesteDaConexaoNaTela>(Permissoes.IntegracaoAdministrar);

        var conexao = await repositorio.ObterParaAlterarAsync(codigo.Trim(), ct);
        if (conexao is null) return Resultado<TesteDaConexaoNaTela>.NaoEncontrado($"Não há conexão {codigo}.");

        ResultadoDoTesteDeConexao resultado;
        try
        {
            resultado = await testador.TestarAsync(resolvedor.Resolver(conexao), ct);
        }
        catch (InvalidOperationException erro)
        {
            resultado = new ResultadoDoTesteDeConexao(false, erro.Message, 0);
        }

        var agora = relogio.Agora;
        conexao.RegistrarVerificacao(resultado.Ok, resultado.Resumo, agora);
        await repositorio.AdicionarVerificacaoAsync(VerificacaoDeConexao.Registrar(conexao.Id, agora, acesso.Atual.UsuarioId, resultado.Ok, resultado.Resumo, resultado.LatenciaMs), ct);

        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<TesteDaConexaoNaTela>.Conflito(gravou.Erro!);

        return Resultado<TesteDaConexaoNaTela>.Ok(new TesteDaConexaoNaTela(
            resultado.Ok, resultado.Resumo, resultado.LatenciaMs, agora, await montador.ConexaoAsync(conexao.Codigo, ct)));
    }

    private async Task<Resultado<ConexaoNaTela>> LigarOuDesligarAsync(string codigo, bool ligar, CancellationToken ct)
    {
        var (conexao, recusa) = await ParaAlterarAsync(codigo, ct);
        if (recusa is { } recusada) return recusada;

        try
        {
            if (ligar) conexao!.Reativar();
            else conexao!.Desativar();
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ConexaoNaTela>.Conflito(erro.Message);
        }

        return await GravarEDevolverAsync(conexao.Codigo, ct);
    }

    private async Task<(Conexao? Conexao, Resultado<ConexaoNaTela>? Recusa)> ParaAlterarAsync(string codigo, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return (null, RegrasDasIntegracoes.SemPermissao<ConexaoNaTela>(Permissoes.IntegracaoAdministrar));

        var conexao = await repositorio.ObterParaAlterarAsync(codigo.Trim(), ct);
        return conexao is null ? (null, Resultado<ConexaoNaTela>.NaoEncontrado($"Não há conexão {codigo}.")) : (conexao, null);
    }

    private async Task<Resultado<ConexaoNaTela>> GravarEDevolverAsync(string codigo, CancellationToken ct)
    {
        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<ConexaoNaTela>.Ok(await montador.ConexaoAsync(codigo, ct))
            : Resultado<ConexaoNaTela>.Conflito(gravou.Erro!);
    }
}

/// <summary>
/// A AGENDA DAS ROTINAS E O "RODAR AGORA" (issue 136) — <c>Integracao.Administrar</c>.
///
/// <para><b>Quem roda é o orquestrador</b>, a cada cinco minutos: "Rodar agora" põe o pedido na fila, e ele começa
/// em até cinco minutos. <b>Rotina que exige credencial não liga nem roda sem ela</b> — ligaria só para falhar a
/// cada volta.</para>
/// </summary>
public sealed class AdministrarRotinas(
    IRepositorioDeRotinas repositorio, IRepositorioDeConexoes conexoes, IResolvedorDeConexoes resolvedor, IUnidadeDeTrabalho unidade,
    MontadorDoPainelDeIntegracoes montador, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Troca a agenda e liga ou desliga.</summary>
    public async Task<Resultado<RotinaNaTela>> ReagendarAsync(string codigo, AgendaNaTela entrada, CancellationToken ct)
    {
        var (rotina, recusa) = await ParaAlterarAsync(codigo, ct);
        if (recusa is { } recusada) return recusada;

        var erros = new ColetorDeErros();
        var cadencia = erros.ItemDeDominioOuPadrao("cadencia", entrada.Cadencia, rotina!.Cadencia);
        TimeOnly? hora = null;
        if (!string.IsNullOrWhiteSpace(entrada.Hora))
        {
            if (TimeOnly.TryParseExact(entrada.Hora.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lida)) hora = lida;
            else erros.Registrar("hora", "Use a hora no formato HH:mm, por exemplo 04:00.", entrada.Hora);
        }

        var agenda = new AgendaDaRotina(cadencia, entrada.Mes, entrada.Dia, hora, entrada.IntervaloMinutos);
        if (!erros.TemErro)
            foreach (var (campo, mensagem) in agenda.Problemas()) erros.Registrar(campo, mensagem);

        // O PRIMEIRO ANO DA SÉRIE (issue 156) só existe nas rotinas que o catálogo diz ter série
        // histórica: aceitá-lo nas outras gravaria um parâmetro que carga nenhuma lê.
        var aceitaSerie = RotinasDoSistema.Obter(rotina.Codigo)?.AnoInicialPadraoDoHistorico is not null;
        if (!aceitaSerie && entrada.AnoInicialDoHistorico is not null)
            erros.Registrar("anoInicialDoHistorico", "Esta rotina não busca série histórica.", entrada.AnoInicialDoHistorico?.ToString(CultureInfo.InvariantCulture));

        // A FAIXA É CONFERIDA AQUI para virar recusa com campo, e não um erro de servidor: a regra
        // continua no domínio, que é a última linha de defesa de quem grava por outro caminho.
        var anoDeHoje = relogio.Agora.Year;
        if (aceitaSerie && entrada.AnoInicialDoHistorico is { } pedido && (pedido < 1974 || pedido > anoDeHoje))
            erros.Registrar(
                "anoInicialDoHistorico",
                $"O primeiro ano da série vai de 1974 — o início da Produção Agrícola Municipal — até {anoDeHoje}.",
                pedido.ToString(CultureInfo.InvariantCulture));

        if (erros.TemErro) return erros.Recusar<RotinaNaTela>("A agenda tem campos a corrigir.");

        var agora = relogio.Agora;
        if (entrada.Ligada == true && !rotina.EstaLigada && await FaltaCredencialAsync(rotina, ct) is { } falta)
            return Resultado<RotinaNaTela>.Conflito(falta);

        rotina.Reagendar(agenda, agora);
        if (aceitaSerie) rotina.DefinirAnoInicialDoHistorico(entrada.AnoInicialDoHistorico, agora);
        if (entrada.Ligada == true) rotina.Ligar(agora);
        else if (entrada.Ligada == false) rotina.Desligar();

        return await GravarEDevolverAsync(rotina.Codigo, ct);
    }

    /// <summary>Põe um "Rodar agora" na fila do orquestrador.</summary>
    public async Task<Resultado<RotinaNaTela>> PedirExecucaoAsync(string codigo, CancellationToken ct)
    {
        var (rotina, recusa) = await ParaAlterarAsync(codigo, ct);
        if (recusa is { } recusada) return recusada;

        if (await FaltaCredencialAsync(rotina!, ct) is { } falta) return Resultado<RotinaNaTela>.Conflito(falta);

        rotina!.PedirExecucao(acesso.Atual.UsuarioId, relogio.Agora);
        return await GravarEDevolverAsync(rotina.Codigo, ct);
    }

    private async Task<string?> FaltaCredencialAsync(Rotina rotina, CancellationToken ct)
    {
        if (RotinasDoSistema.Obter(rotina.Codigo)?.ConexaoExigida is not { } exigida) return null;

        var conexao = (await conexoes.ListarAsync(ct)).FirstOrDefault(c => c.Codigo == exigida);
        return conexao is null || resolvedor.OrigemDe(conexao) == OrigemDaCredencial.Nenhuma
            ? $"A rotina {rotina.Nome} precisa da credencial de {conexao?.Nome ?? exigida}. Configure-a e teste antes de ligar ou rodar."
            : null;
    }

    private async Task<(Rotina? Rotina, Resultado<RotinaNaTela>? Recusa)> ParaAlterarAsync(string codigo, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.IntegracaoAdministrar))
            return (null, RegrasDasIntegracoes.SemPermissao<RotinaNaTela>(Permissoes.IntegracaoAdministrar));

        var rotina = await repositorio.ObterParaAlterarAsync(codigo.Trim(), ct);
        return rotina is null ? (null, Resultado<RotinaNaTela>.NaoEncontrado($"Não há rotina {codigo}.")) : (rotina, null);
    }

    private async Task<Resultado<RotinaNaTela>> GravarEDevolverAsync(string codigo, CancellationToken ct)
    {
        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<RotinaNaTela>.Ok(await montador.RotinaAsync(codigo, ct))
            : Resultado<RotinaNaTela>.Conflito(gravou.Erro!);
    }
}

internal static class RegrasDasIntegracoes
{
    public static Resultado<T> SemPermissao<T>(string permissao) =>
        Resultado<T>.SemPermissao($"Falta a permissão '{permissao}' ({Permissoes.Catalogo[permissao]}).");
}
