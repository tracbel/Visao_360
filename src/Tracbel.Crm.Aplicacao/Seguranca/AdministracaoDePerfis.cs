using System.Text.RegularExpressions;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Seguranca;

/// <summary>Uma permissão do editor de perfil: o código e a profundidade, pelo nome (<c>Organizacao</c>).</summary>
/// <param name="Codigo">A permissão do catálogo.</param>
/// <param name="Profundidade">Até onde vale.</param>
public sealed record PermissaoDoEditor(string? Codigo, string? Profundidade);

/// <summary>O corpo da criação de um perfil próprio — do zero ou duplicando outro (a tela manda as permissões).</summary>
/// <param name="Codigo">O código (maiúsculas, números e sublinhado).</param>
/// <param name="Nome">O nome.</param>
/// <param name="Descricao">Para que serve.</param>
/// <param name="Permissoes">O conjunto de permissões.</param>
public sealed record NovoPerfil(string? Codigo, string? Nome, string? Descricao, IReadOnlyList<PermissaoDoEditor>? Permissoes);

/// <summary>O corpo da edição de um perfil próprio: o estado final.</summary>
/// <param name="Nome">O nome.</param>
/// <param name="Descricao">Para que serve.</param>
/// <param name="Permissoes">O conjunto final de permissões.</param>
public sealed record EdicaoDePerfil(string? Nome, string? Descricao, IReadOnlyList<PermissaoDoEditor>? Permissoes);

/// <summary>Uma permissão do catálogo, para o editor.</summary>
/// <param name="Codigo">O código.</param>
/// <param name="Descricao">O que ela deixa fazer.</param>
public sealed record PermissaoDoCatalogo(string Codigo, string Descricao);

/// <summary>Todos os perfis, para a tela de Perfis (<c>Perfil.Administrar</c>).</summary>
public sealed class ListarPerfisDaAdministracao(IRepositorioDePerfisDaAdministracao repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<PerfilNaAdministracao>>>> ExecutarAsync(CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.PerfilAdministrar))
            return RegrasDaAdministracao.SemPermissao<ComProcedencia<IReadOnlyList<PerfilNaAdministracao>>>(Permissoes.PerfilAdministrar);

        var perfis = await repositorio.ListarTodosAsync(relogio.Agora, ct);
        return Resultado<ComProcedencia<IReadOnlyList<PerfilNaAdministracao>>>.Ok(
            ComProcedencia<IReadOnlyList<PerfilNaAdministracao>>.DoNossoBanco(perfis, "seguranca.Perfil + seguranca.PerfilPermissao", relogio));
    }
}

/// <summary>O catálogo de permissões — em código (decisão D-3), mostrado ao editor.</summary>
public sealed class ListarCatalogoDePermissoes(IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    public Resultado<ComProcedencia<IReadOnlyList<PermissaoDoCatalogo>>> Executar()
    {
        if (!acesso.Atual.Tem(Permissoes.PerfilAdministrar))
            return RegrasDaAdministracao.SemPermissao<ComProcedencia<IReadOnlyList<PermissaoDoCatalogo>>>(Permissoes.PerfilAdministrar);

        IReadOnlyList<PermissaoDoCatalogo> catalogo = Permissoes.Catalogo
            .Select(p => new PermissaoDoCatalogo(p.Key, p.Value))
            .OrderBy(p => p.Codigo, StringComparer.Ordinal)
            .ToList();

        return Resultado<ComProcedencia<IReadOnlyList<PermissaoDoCatalogo>>>.Ok(
            ComProcedencia<IReadOnlyList<PermissaoDoCatalogo>>.DoNossoBanco(catalogo, "Permissoes.Catalogo (código)", relogio));
    }
}

/// <summary>
/// CRIAR, EDITAR, DESATIVAR E REATIVAR PERFIS PRÓPRIOS (issue 113, parte 2b).
///
/// <para><b>Os do sistema são fixos</b> (decisão de 22/09/2026): vêm do código e das migrações, e editá-los pela
/// tela faria a próxima publicação sobrescrever ou quebrar o ajuste. Para mudar, duplica-se.</para>
///
/// <para><b>Ninguém cria nem edita um perfil com permissão maior do que a que tem</b> (<see cref="RegraDeConcessao"/>)
/// — senão, quem só administra perfis se daria o resto pelo perfil que acabou de criar.</para>
/// </summary>
public sealed partial class AdministrarPerfil(
    IRepositorioDePerfisDaAdministracao repositorio, IUnidadeDeTrabalho unidade, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    [GeneratedRegex("^[A-Z][A-Z0-9_]{2,59}$")]
    private static partial Regex FormatoDoCodigo();

    /// <summary>Cria um perfil próprio.</summary>
    public async Task<Resultado<PerfilNaAdministracao>> CriarAsync(NovoPerfil entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.PerfilAdministrar))
            return RegrasDaAdministracao.SemPermissao<PerfilNaAdministracao>(Permissoes.PerfilAdministrar);

        var erros = new ColetorDeErros();
        var codigo = erros.Obrigatorio("codigo", entrada.Codigo, "o código do perfil").Trim().ToUpperInvariant();
        var nome = erros.Obrigatorio("nome", entrada.Nome, "o nome do perfil");

        if (codigo.Length > 0 && !FormatoDoCodigo().IsMatch(codigo))
            erros.Registrar("codigo", "Use de 3 a 60 caracteres: letras maiúsculas, números e sublinhado, começando por letra (ex.: DIRETORIA_TRACBEL).", entrada.Codigo);
        else if (codigo.Length > 0 && PerfisDeSistema.EhDoSistema(codigo))
            erros.Registrar("codigo", "Este código é de um perfil do sistema. Escolha outro.", entrada.Codigo);
        else if (codigo.Length > 0 && await repositorio.CodigoEmUsoAsync(codigo, ct))
            erros.Registrar("codigo", "Já existe perfil com este código.", entrada.Codigo);

        var permissoes = LerPermissoes(erros, entrada.Permissoes);
        if (erros.TemErro)
            return erros.Recusar<PerfilNaAdministracao>("O perfil tem campos a corrigir.");

        var faltas = RegraDeConcessao.FaltasParaConceder(acesso.Atual, permissoes);
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<PerfilNaAdministracao>(faltas, "criar este perfil");

        Perfil perfil;
        try
        {
            perfil = Perfil.Criar(codigo, nome, entrada.Descricao);
            perfil.DefinirPermissoes(permissoes);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<PerfilNaAdministracao>.Conflito(erro.Message);
        }

        await repositorio.AdicionarAsync(perfil, ct);
        return await GravarEDevolverAsync(codigo, ct);
    }

    /// <summary>Edita um perfil próprio: nome, descrição e o conjunto inteiro de permissões.</summary>
    public async Task<Resultado<PerfilNaAdministracao>> EditarAsync(string codigo, EdicaoDePerfil entrada, CancellationToken ct)
    {
        var (perfil, recusa) = await ProprioParaAlterarAsync(codigo, "editado", ct);
        if (recusa is { } recusado) return recusado;

        var erros = new ColetorDeErros();
        var nome = erros.Obrigatorio("nome", entrada.Nome, "o nome do perfil");
        var permissoes = LerPermissoes(erros, entrada.Permissoes);
        if (erros.TemErro)
            return erros.Recusar<PerfilNaAdministracao>("O perfil tem campos a corrigir.");

        // A REGRA VALE PARA O QUE O PERFIL VAI DAR, e também para o que ele dá hoje: quem não tem uma permissão não
        // pode tirá-la de um perfil de quem tem — nem pô-la.
        var atuais = perfil!.Permissoes.Select(p => (p.CodigoPermissao, p.Profundidade));
        var faltas = RegraDeConcessao.FaltasParaConceder(acesso.Atual, [.. permissoes, .. atuais]);
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<PerfilNaAdministracao>(faltas, "editar este perfil");

        try
        {
            perfil.Renomear(nome, entrada.Descricao);
            perfil.DefinirPermissoes(permissoes);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<PerfilNaAdministracao>.Conflito(erro.Message);
        }

        return await GravarEDevolverAsync(perfil.Codigo, ct);
    }

    /// <summary>Desativa um perfil próprio: deixa de valer na hora para quem tem, e nada é apagado.</summary>
    public async Task<Resultado<PerfilNaAdministracao>> DesativarAsync(string codigo, CancellationToken ct)
    {
        var (perfil, recusa) = await ProprioParaAlterarAsync(codigo, "desativado", ct);
        if (recusa is { } recusado) return recusado;

        var faltas = RegraDeConcessao.FaltasParaConceder(acesso.Atual, perfil!.Permissoes.Select(p => (p.CodigoPermissao, p.Profundidade)));
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<PerfilNaAdministracao>(faltas, "desativar este perfil");

        perfil.Desativar();
        return await GravarEDevolverAsync(perfil.Codigo, ct);
    }

    /// <summary>Reativa um perfil próprio desativado.</summary>
    public async Task<Resultado<PerfilNaAdministracao>> ReativarAsync(string codigo, CancellationToken ct)
    {
        var (perfil, recusa) = await ProprioParaAlterarAsync(codigo, "reativado", ct);
        if (recusa is { } recusado) return recusado;

        var faltas = RegraDeConcessao.FaltasParaConceder(acesso.Atual, perfil!.Permissoes.Select(p => (p.CodigoPermissao, p.Profundidade)));
        if (faltas.Count > 0)
            return RegrasDaAdministracao.AcimaDoQueTem<PerfilNaAdministracao>(faltas, "reativar este perfil");

        perfil.Reativar();
        return await GravarEDevolverAsync(perfil.Codigo, ct);
    }

    private async Task<(Perfil? Perfil, Resultado<PerfilNaAdministracao>? Recusa)> ProprioParaAlterarAsync(string codigo, string oQue, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.PerfilAdministrar))
            return (null, RegrasDaAdministracao.SemPermissao<PerfilNaAdministracao>(Permissoes.PerfilAdministrar));

        var perfil = await repositorio.ObterParaAlterarAsync(codigo.Trim(), ct);
        if (perfil is null)
            return (null, Resultado<PerfilNaAdministracao>.NaoEncontrado($"Não há perfil {codigo}."));

        if (PerfisDeSistema.EhDoSistema(perfil.Codigo))
            return (null, Resultado<PerfilNaAdministracao>.Conflito(
                $"O perfil {perfil.Nome} é do sistema e não é {oQue} pela tela. Duplique-o e ajuste a cópia."));

        return (perfil, null);
    }

    private static List<(string Codigo, Profundidade Profundidade)> LerPermissoes(ColetorDeErros erros, IReadOnlyList<PermissaoDoEditor>? entrada)
    {
        var lidas = new List<(string, Profundidade)>();
        foreach (var (item, i) in (entrada ?? []).Select((item, i) => (item, i)))
        {
            var campo = $"permissoes[{i}]";
            if (string.IsNullOrWhiteSpace(item.Codigo) || !Permissoes.Existe(item.Codigo.Trim()))
            {
                erros.Registrar(campo, "Permissão fora do catálogo.", item.Codigo);
                continue;
            }

            if (!Enum.TryParse<Profundidade>(item.Profundidade, ignoreCase: true, out var profundidade) || profundidade == Profundidade.Nenhum
                || !Enum.IsDefined(profundidade))
            {
                erros.Registrar(campo, "Profundidade inválida: use Proprios, Equipe, Empresa, EmpresaEAbaixo ou Organizacao.", item.Profundidade);
                continue;
            }

            lidas.Add((item.Codigo.Trim(), profundidade));
        }

        if (lidas.GroupBy(p => p.Item1).Any(g => g.Count() > 1))
            erros.Registrar("permissoes", "Uma permissão aparece mais de uma vez.");

        return lidas;
    }

    private async Task<Resultado<PerfilNaAdministracao>> GravarEDevolverAsync(string codigo, CancellationToken ct)
    {
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<PerfilNaAdministracao>.Conflito(gravou.Erro!);

        var perfil = (await repositorio.ListarTodosAsync(relogio.Agora, ct)).First(p => string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        return Resultado<PerfilNaAdministracao>.Ok(perfil);
    }
}
