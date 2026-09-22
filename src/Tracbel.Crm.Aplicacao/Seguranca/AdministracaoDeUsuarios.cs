using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Seguranca;

/// <summary>O corpo da liberação: a filial de casa escolhida.</summary>
/// <param name="FilialCodigo">O código da filial (<c>010101</c>).</param>
public sealed record LiberacaoDeUsuario(string? FilialCodigo);

/// <summary>O corpo da concessão.</summary>
/// <param name="PerfilCodigo">O perfil (<c>GERENCIA</c>).</param>
/// <param name="FilialCodigo">A filial em que vale; vazio vale em qualquer uma que a pessoa possa escolher.</param>
/// <param name="ValidaAte">O último dia em que vale (aaaa-mm-dd); vazio é permanente.</param>
/// <param name="Justificativa">Por que, e por autorização de quem.</param>
public sealed record NovaConcessao(string? PerfilCodigo, string? FilialCodigo, string? ValidaAte, string? Justificativa);

/// <summary>O corpo da revogação.</summary>
/// <param name="Motivo">Por que, e por autorização de quem.</param>
public sealed record RevogacaoDeConcessao(string? Motivo);

/// <summary>
/// AS REGRAS QUE VALEM PARA TODA A ADMINISTRAÇÃO DE USUÁRIOS (issue 113), escritas uma vez.
///
/// <para><b>Ver</b> é <c>Usuario.Ler</c> ou <c>Usuario.Administrar</c>; o alcance sai da maior profundidade das
/// duas: <see cref="Profundidade.Organizacao"/> vê todos, <see cref="Profundidade.EmpresaEAbaixo"/> só os da
/// filial escolhida. <b>Agir</b> é <c>Usuario.Administrar</c>.</para>
///
/// <para><b>Ninguém age sobre a própria conta</b> — liberar, conceder, revogar, desativar: toda mudança de
/// acesso passa por uma segunda pessoa. <b>Ninguém age acima do que tem</b>: conceder ou revogar um perfil, e
/// desativar quem tem perfis, exige ter as permissões deles (<see cref="RegraDeConcessao"/>).</para>
/// </summary>
internal static class RegrasDaAdministracao
{
    public static bool PodeVer(ContextoAcesso acesso) =>
        acesso.Tem(Permissoes.UsuarioLer) || acesso.Tem(Permissoes.UsuarioAdministrar);

    /// <summary>Nulo: todas as filiais. Um conjunto: só os usuários com filial de casa nele.</summary>
    public static IReadOnlySet<int>? Alcance(ContextoAcesso acesso)
    {
        var maior = (Profundidade)Math.Max((int)acesso.ProfundidadeDe(Permissoes.UsuarioLer), (int)acesso.ProfundidadeDe(Permissoes.UsuarioAdministrar));
        return maior >= Profundidade.Organizacao ? null : acesso.EmpresasVisiveis;
    }

    public static Resultado<T> SemPermissao<T>(string permissao) =>
        Resultado<T>.SemPermissao($"Falta a permissão '{permissao}' ({Permissoes.Catalogo[permissao]}).");

    public static Resultado<T> SemVer<T>() =>
        Resultado<T>.SemPermissao(
            $"Falta a permissão '{Permissoes.UsuarioLer}' ({Permissoes.Catalogo[Permissoes.UsuarioLer]}) ou '{Permissoes.UsuarioAdministrar}'.");

    public static Resultado<T> NaPropriaConta<T>(string oQue) =>
        Resultado<T>.Conflito($"Ninguém {oQue} a própria conta. Peça a outra pessoa que administra o CRM.");

    public static Resultado<T> AcimaDoQueTem<T>(IReadOnlyList<string> faltas, string oQue) =>
        Resultado<T>.SemPermissao(
            $"Para {oQue} é preciso ter as permissões dele, e faltam estas: {string.Join(", ", faltas)}. " +
            "Ninguém dá ou tira um acesso maior do que o próprio.");
}

/// <summary>A lista de usuários da administração.</summary>
public sealed class ListarUsuariosDaAdministracao(IRepositorioDeUsuariosDaAdministracao repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    /// <param name="situacao">Ativa (padrão), AguardandoLiberacao ou Desativada.</param>
    /// <param name="termo">Trecho do nome ou do e-mail.</param>
    /// <param name="pagina">A página, a partir de 1.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>>> ExecutarAsync(
        string? situacao, string? termo, int? pagina, int? tamanho, CancellationToken ct)
    {
        var contexto = acesso.Atual;
        if (!RegrasDaAdministracao.PodeVer(contexto))
            return RegrasDaAdministracao.SemVer<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>>();

        var erros = new ColetorDeErros();
        var escolhida = erros.ItemDeDominioOuPadrao("situacao", situacao, SituacaoDaConta.Ativa);
        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>>.FalhaDeValidacao(paginacao.Erro!, paginacao.Erros);
        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>>("O filtro da lista tem campos a corrigir.");

        // A FILA DE LIBERAÇÃO É DE QUEM LIBERA: a filial de quem espera é provisória, e mostrá-la a uma gerência
        // daria a impressão de que a pessoa é daquela filial.
        if (escolhida == SituacaoDaConta.AguardandoLiberacao && !contexto.Tem(Permissoes.UsuarioAdministrar))
            return RegrasDaAdministracao.SemPermissao<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>>(Permissoes.UsuarioAdministrar);

        var lista = await repositorio.ListarAsync(
            new FiltroDeUsuarios(escolhida, termo, paginacao.Valor), RegrasDaAdministracao.Alcance(contexto), relogio.Agora, ct);

        return Resultado<ComProcedencia<PaginaDe<UsuarioNaAdministracao>>>.Ok(
            ComProcedencia<PaginaDe<UsuarioNaAdministracao>>.DoNossoBanco(lista, "seguranca.Usuario + seguranca.UsuarioPerfil", relogio));
    }
}

/// <summary>A conta e o histórico das concessões.</summary>
public sealed class ObterUsuarioDaAdministracao(IRepositorioDeUsuariosDaAdministracao repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    /// <param name="chave">A chave pública da conta.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<UsuarioDetalhadoNaAdministracao>>> ExecutarAsync(Guid chave, CancellationToken ct)
    {
        var contexto = acesso.Atual;
        if (!RegrasDaAdministracao.PodeVer(contexto))
            return RegrasDaAdministracao.SemVer<ComProcedencia<UsuarioDetalhadoNaAdministracao>>();

        var detalhe = await repositorio.ObterAsync(chave, RegrasDaAdministracao.Alcance(contexto), relogio.Agora, ct);
        return detalhe is null
            ? Resultado<ComProcedencia<UsuarioDetalhadoNaAdministracao>>.NaoEncontrado($"Não há conta {chave} ao seu alcance.")
            : Resultado<ComProcedencia<UsuarioDetalhadoNaAdministracao>>.Ok(
                ComProcedencia<UsuarioDetalhadoNaAdministracao>.DoNossoBanco(detalhe, "seguranca.Usuario + seguranca.UsuarioPerfil", relogio));
    }
}

/// <summary>Os perfis que podem ser concedidos, com o que cada um dá.</summary>
public sealed class ListarPerfisParaConceder(IRepositorioDeReferenciasDeAcesso repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Um perfil como o formulário o mostra.</summary>
    /// <param name="Codigo">O código.</param>
    /// <param name="Nome">O nome.</param>
    /// <param name="Descricao">Para que serve.</param>
    /// <param name="EhPadrao">Se é o que todo usuário recebe (não se concede).</param>
    /// <param name="PodeConceder">Se quem pergunta tem as permissões dele.</param>
    /// <param name="Permissoes">O que ele dá, em português, com a profundidade.</param>
    public sealed record PerfilDoFormulario(
        string Codigo, string Nome, string? Descricao, bool EhPadrao, bool PodeConceder, IReadOnlyList<PermissaoDoEscopo> Permissoes);

    /// <summary>Executa a leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<PerfilDoFormulario>>>> ExecutarAsync(CancellationToken ct)
    {
        var contexto = acesso.Atual;
        if (!contexto.Tem(Permissoes.UsuarioAdministrar))
            return RegrasDaAdministracao.SemPermissao<ComProcedencia<IReadOnlyList<PerfilDoFormulario>>>(Permissoes.UsuarioAdministrar);

        IReadOnlyList<PerfilDoFormulario> perfis = (await repositorio.ListarPerfisAsync(ct))
            .Select(p => new PerfilDoFormulario(
                p.Codigo, p.Nome, p.Descricao, p.EhPadrao,
                PodeConceder: !p.EhPadrao && RegraDeConcessao.FaltasParaConceder(contexto, p.Permissoes).Count == 0,
                p.Permissoes
                    .Select(i => new PermissaoDoEscopo(i.Codigo, Permissoes.Catalogo.TryGetValue(i.Codigo, out var d) ? d : i.Codigo, i.Profundidade.ToString()))
                    .ToList()))
            .ToList();

        return Resultado<ComProcedencia<IReadOnlyList<PerfilDoFormulario>>>.Ok(
            ComProcedencia<IReadOnlyList<PerfilDoFormulario>>.DoNossoBanco(perfis, "seguranca.Perfil + seguranca.PerfilPermissao", relogio));
    }
}

/// <summary>
/// AS AÇÕES DA ADMINISTRAÇÃO — liberar, conceder, revogar, desativar e reativar (issue 113). Todas devolvem a
/// conta como ficou, para a tela não precisar de uma segunda leitura.
/// </summary>
public sealed class AdministrarUsuario(
    IRepositorioDeUsuariosDaAdministracao leitura,
    IRepositorioDeConcessoes concessoes,
    IRepositorioDeReferenciasDeAcesso referencias,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Libera a conta que espera, na filial de casa escolhida.</summary>
    public async Task<Resultado<UsuarioDetalhadoNaAdministracao>> LiberarAsync(Guid chave, LiberacaoDeUsuario entrada, CancellationToken ct)
    {
        var (usuario, recusa) = await ContaParaAgirAsync(chave, "libera", ct);
        if (recusa is { } recusada) return recusada;

        if (!usuario!.AguardaLiberacao)
            return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito("Esta conta não está aguardando liberação.");

        var erros = new ColetorDeErros();
        var codigo = erros.Obrigatorio("filialCodigo", entrada.FilialCodigo, "a filial de casa da pessoa");
        var filial = string.IsNullOrWhiteSpace(codigo) ? null : await referencias.ObterFilialAtivaAsync(codigo.Trim(), ct);
        if (!string.IsNullOrWhiteSpace(codigo) && filial is null)
            erros.Registrar("filialCodigo", "Não há filial ativa com este código.", entrada.FilialCodigo);
        if (erros.TemErro)
            return erros.Recusar<UsuarioDetalhadoNaAdministracao>("A liberação tem campos a corrigir.");

        usuario.Liberar(filial!.Value.Id, acesso.Atual.UsuarioId);
        return await GravarEDevolverAsync(chave, ct);
    }

    /// <summary>Concede um perfil à conta.</summary>
    public async Task<Resultado<UsuarioDetalhadoNaAdministracao>> ConcederAsync(Guid chave, NovaConcessao entrada, CancellationToken ct)
    {
        var (usuario, recusa) = await ContaParaAgirAsync(chave, "concede perfil para", ct);
        if (recusa is { } recusada) return recusada;

        if (usuario!.AguardaLiberacao)
            return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito("Libere a conta antes: ela ainda espera a filial de casa.");
        if (!usuario.EstaAtivo)
            return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito("A conta está desativada. Reative antes de conceder perfil.");

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        var codigoDoPerfil = erros.Obrigatorio("perfilCodigo", entrada.PerfilCodigo, "o perfil a conceder");
        var justificativa = erros.Obrigatorio("justificativa", entrada.Justificativa, "a justificativa — por que, e por autorização de quem");

        var perfil = string.IsNullOrWhiteSpace(codigoDoPerfil)
            ? null
            : (await referencias.ListarPerfisAsync(ct)).FirstOrDefault(p => string.Equals(p.Codigo, codigoDoPerfil.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(codigoDoPerfil) && perfil is null)
            erros.Registrar("perfilCodigo", "Não há perfil ativo com este código.", entrada.PerfilCodigo);
        else if (perfil is { EhPadrao: true })
            erros.Registrar("perfilCodigo", "O perfil padrão não se concede: todo usuário já o tem.", entrada.PerfilCodigo);

        (int Id, string Nome)? filial = null;
        if (!string.IsNullOrWhiteSpace(entrada.FilialCodigo))
        {
            filial = await referencias.ObterFilialAtivaAsync(entrada.FilialCodigo.Trim(), ct);
            if (filial is null) erros.Registrar("filialCodigo", "Não há filial ativa com este código.", entrada.FilialCodigo);
        }

        DateTime? expiraEm = null;
        if (!string.IsNullOrWhiteSpace(entrada.ValidaAte))
        {
            if (!DateOnly.TryParseExact(entrada.ValidaAte.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ultimoDia))
                erros.Registrar("validaAte", "Use a data no formato aaaa-mm-dd.", entrada.ValidaAte);
            else if (ultimoDia < DateOnly.FromDateTime(agora))
                erros.Registrar("validaAte", "A validade não pode terminar antes de hoje.", entrada.ValidaAte);
            else
                // VALE ATÉ O FIM DO ÚLTIMO DIA: expira no começo do dia seguinte.
                expiraEm = ultimoDia.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }

        if (erros.TemErro)
            return erros.Recusar<UsuarioDetalhadoNaAdministracao>("A concessão tem campos a corrigir.");

        var faltas = RegraDeConcessao.FaltasParaConceder(acesso.Atual, perfil!.Permissoes);
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<UsuarioDetalhadoNaAdministracao>(faltas, $"conceder o perfil {perfil.Nome}");

        // A MESMA CONCESSÃO NÃO SE REPETE: vigente, recusa; vencida, é marcada como substituída — a nova é que vale.
        var existentes = (await concessoes.ListarConcessoesParaAlterarAsync(usuario.Id, ct))
            .Where(c => c.PerfilId == perfil.Id && c.EmpresaId == filial?.Id && !c.Revogada)
            .ToList();

        if (existentes.Any(c => c.EstaVigente(agora)))
            return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito(
                $"A conta já tem o perfil {perfil.Nome} {(filial is null ? "em todas as filiais" : $"em {filial.Value.Nome}")}. " +
                "Para mudar a validade, revogue e conceda de novo.");

        foreach (var vencida in existentes)
            vencida.Revogar(acesso.Atual.UsuarioId, $"Substituída por uma concessão nova em {agora:dd/MM/yyyy}.", agora);

        UsuarioPerfil concessao;
        try
        {
            concessao = UsuarioPerfil.Conceder(usuario.Id, perfil.Id, justificativa, acesso.Atual.UsuarioId, agora, filial?.Id, expiraEm);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito(erro.Message);
        }

        await concessoes.AdicionarConcessaoAsync(concessao, ct);
        return await GravarEDevolverAsync(chave, ct);
    }

    /// <summary>Revoga uma concessão, com motivo. A linha fica, para o histórico.</summary>
    public async Task<Resultado<UsuarioDetalhadoNaAdministracao>> RevogarAsync(Guid chave, long concessaoId, RevogacaoDeConcessao entrada, CancellationToken ct)
    {
        var (usuario, recusa) = await ContaParaAgirAsync(chave, "revoga perfil de", ct);
        if (recusa is { } recusada) return recusada;

        var concessao = (await concessoes.ListarConcessoesParaAlterarAsync(usuario!.Id, ct)).FirstOrDefault(c => c.Id == concessaoId);
        if (concessao is null)
            return Resultado<UsuarioDetalhadoNaAdministracao>.NaoEncontrado($"A conta não tem a concessão {concessaoId}.");

        var erros = new ColetorDeErros();
        var motivo = erros.Obrigatorio("motivo", entrada.Motivo, "o motivo — por que, e por autorização de quem");
        if (erros.TemErro)
            return erros.Recusar<UsuarioDetalhadoNaAdministracao>("A revogação tem campos a corrigir.");

        var perfil = (await referencias.ListarPerfisAsync(ct)).FirstOrDefault(p => p.Id == concessao.PerfilId);
        if (perfil is not null)
        {
            var faltas = RegraDeConcessao.FaltasParaConceder(acesso.Atual, perfil.Permissoes);
            if (faltas.Count > 0)
                return RegrasDaAdministracao.AcimaDoQueTem<UsuarioDetalhadoNaAdministracao>(faltas, $"revogar o perfil {perfil.Nome}");
        }

        try
        {
            concessao.Revogar(acesso.Atual.UsuarioId, motivo, relogio.Agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito(erro.Message);
        }

        return await GravarEDevolverAsync(chave, ct);
    }

    /// <summary>Desativa a conta, sem apagar nada.</summary>
    public async Task<Resultado<UsuarioDetalhadoNaAdministracao>> DesativarAsync(Guid chave, CancellationToken ct)
    {
        var (usuario, recusa) = await ContaParaAgirAsync(chave, "desativa", ct);
        if (recusa is { } recusada) return recusada;

        // QUEM TEM MAIS ACESSO NÃO É DESATIVADO POR QUEM TEM MENOS: desativar tira tudo o que a conta tem.
        var faltas = await FaltasSobreOsPerfisDaContaAsync(usuario!.Id, ct);
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<UsuarioDetalhadoNaAdministracao>(faltas, "desativar esta conta");

        usuario.Desativar(acesso.Atual.UsuarioId);
        return await GravarEDevolverAsync(chave, ct);
    }

    /// <summary>Reativa a conta desativada.</summary>
    public async Task<Resultado<UsuarioDetalhadoNaAdministracao>> ReativarAsync(Guid chave, CancellationToken ct)
    {
        var (usuario, recusa) = await ContaParaAgirAsync(chave, "reativa", ct);
        if (recusa is { } recusada) return recusada;

        var faltas = await FaltasSobreOsPerfisDaContaAsync(usuario!.Id, ct);
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<UsuarioDetalhadoNaAdministracao>(faltas, "reativar esta conta");

        usuario.Reativar(acesso.Atual.UsuarioId);
        return await GravarEDevolverAsync(chave, ct);
    }

    /// <summary>A conta rastreada, depois de conferir a permissão, o alcance e que não é a própria.</summary>
    private async Task<(Usuario? Usuario, Resultado<UsuarioDetalhadoNaAdministracao>? Recusa)> ContaParaAgirAsync(Guid chave, string oQue, CancellationToken ct)
    {
        var contexto = acesso.Atual;
        if (!contexto.Tem(Permissoes.UsuarioAdministrar))
            return (null, RegrasDaAdministracao.SemPermissao<UsuarioDetalhadoNaAdministracao>(Permissoes.UsuarioAdministrar));

        var usuario = await concessoes.ObterContaParaAlterarAsync(chave, ct);
        var alcance = RegrasDaAdministracao.Alcance(contexto);
        if (usuario is null || (alcance is not null && !alcance.Contains(usuario.EmpresaId)))
            return (null, Resultado<UsuarioDetalhadoNaAdministracao>.NaoEncontrado($"Não há conta {chave} ao seu alcance."));

        if (usuario.Id == contexto.UsuarioId)
            return (null, RegrasDaAdministracao.NaPropriaConta<UsuarioDetalhadoNaAdministracao>(oQue));

        return (usuario, null);
    }

    private async Task<IReadOnlyList<string>> FaltasSobreOsPerfisDaContaAsync(long usuarioId, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var vigentes = (await concessoes.ListarConcessoesParaAlterarAsync(usuarioId, ct)).Where(c => c.EstaVigente(agora)).Select(c => c.PerfilId).ToHashSet();
        var permissoes = (await referencias.ListarPerfisAsync(ct)).Where(p => vigentes.Contains(p.Id)).SelectMany(p => p.Permissoes);
        return RegraDeConcessao.FaltasParaConceder(acesso.Atual, permissoes);
    }

    private async Task<Resultado<UsuarioDetalhadoNaAdministracao>> GravarEDevolverAsync(Guid chave, CancellationToken ct)
    {
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<UsuarioDetalhadoNaAdministracao>.Conflito(gravou.Erro!);

        var detalhe = await leitura.ObterAsync(chave, filiais: null, relogio.Agora, ct);
        return Resultado<UsuarioDetalhadoNaAdministracao>.Ok(detalhe!);
    }
}
