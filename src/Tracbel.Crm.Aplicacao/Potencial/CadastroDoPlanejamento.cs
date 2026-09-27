using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>
/// OS PARÂMETROS DO PLANEJAMENTO QUE VALEM NUMA DATA (issue 256). Sem data, hoje. É a pergunta que a demanda mensal,
/// o IOC e os cenários fazem, e a que a tela de Configurações mostra.
/// </summary>
public sealed class ObterParametrosDoPlanejamento(
    IRepositorioDoPlanejamento repositorio,
    IRepositorioDeReferenciasDoPotencial referencias,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IRepositorioDoCatalogoDoMercado catalogoDoMercado,
    IRelogio relogio)
{
    internal const string Tabelas = "organizacao.ParametroDoPlanejamento · organizacao.ShareAlvoDaCategoria";

    /// <summary>Lê os parâmetros vigentes.</summary>
    /// <param name="em">A data, aaaa-mm-dd. Vazia é hoje.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<ParametrosDoPlanejamentoVigentes>>> ExecutarAsync(string? em, CancellationToken ct)
    {
        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "em", em, obrigatoria: false) ?? ParametroComVigencia.HojeNoBrasil(relogio.Agora);
        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<ParametrosDoPlanejamentoVigentes>>("A data da consulta não vale.");

        var planejamento = ParametroComVigencia.VigenteEm(await repositorio.ListarParametrosAsync(ct), data);
        var shares = (await repositorio.ListarSharesAsync(ct))
            .GroupBy(s => s.CategoriaDeMaquinaId)
            .Select(g => ParametroComVigencia.VigenteEm(g, data))
            .OfType<ShareAlvoDaCategoria>()
            .ToList();

        var usados = new ParametroComVigencia?[] { planejamento }.Concat(shares).OfType<ParametroComVigencia>().ToList();
        var nomes = await referencias.NomesDosUsuariosAsync(ObterParametrosDoPotencial.Autores(usados), ct);
        var categorias = await catalogo.CategoriasDeMaquinaAsync(shares.Select(s => s.CategoriaDeMaquinaId).ToHashSet(), ct);

        // NA ORDEM DO CATÁLOGO, a mesma das outras telas — trator primeiro, e não a ordem em que foram registrados.
        var ordem = (await catalogoDoMercado.LerAsync(ct)).Categorias;
        var posicao = ordem.Select((c, i) => (c.Codigo, i)).ToDictionary(x => x.Codigo, x => x.i, StringComparer.Ordinal);
        var detalhes = shares.Select(s => ShareAlvoDetalhe.De(s, categorias, nomes))
            .OrderBy(s => posicao.GetValueOrDefault(s.CategoriaDeMaquinaCodigo, int.MaxValue))
            .ToList();

        var vigentes = new ParametrosDoPlanejamentoVigentes(
            data,
            planejamento is null ? null : ParametroDoPlanejamentoDetalhe.De(planejamento, nomes),
            detalhes,
            Pendencias(planejamento, detalhes, ordem));

        return Resultado<ComProcedencia<ParametrosDoPlanejamentoVigentes>>.Ok(
            ComProcedencia<ParametrosDoPlanejamentoVigentes>.DoNossoBanco(vigentes, Tabelas, relogio));
    }

    /// <summary>
    /// O QUE FALTA, EM FRASE — inclusive o que está preenchido mas ainda é o número do protótipo: a semente não tem
    /// autor, e enquanto ela valer o planejamento é estimativa de outra pessoa, não decisão da Tracbel.
    /// </summary>
    private static List<string> Pendencias(
        ParametroDoPlanejamento? planejamento,
        List<ShareAlvoDetalhe> detalhes,
        IReadOnlyList<CategoriaNoCatalogo> catalogo)
    {
        var pendencias = new List<string>();

        if (planejamento is null)
            pendencias.Add("Não há sazonalidade nem pesos do IOC vigentes nesta data: a previsão mensal e o Índice de Oportunidade Comercial não saem.");
        else if (planejamento.InformadoPorId is null)
            pendencias.Add("A sazonalidade e os pesos do IOC ainda são os do protótipo da pasta 360, a confirmar: registre os da Tracbel numa vigência nova.");

        // SEM AUTOR É A SEMENTE: toda vigência registrada pela tela tem quem a registrou.
        var doPrototipo = detalhes.Where(d => d.Vigencia.InformadoPor is null).Select(d => d.CategoriaDeMaquinaNome).ToList();
        if (doPrototipo.Count > 0)
            pendencias.Add($"O share-alvo de {Lista(doPrototipo)} ainda é o do protótipo da pasta 360, a confirmar.");

        var comShare = detalhes.Select(d => d.CategoriaDeMaquinaCodigo).ToHashSet(StringComparer.Ordinal);
        var semShare = catalogo.Where(c => c.EstaAtiva && !comShare.Contains(c.Codigo)).Select(c => c.Nome).ToList();
        if (semShare.Count > 0)
            pendencias.Add($"{Lista(semShare)}: sem share-alvo — a meta de planejamento {(semShare.Count == 1 ? "dessa categoria" : "dessas categorias")} não sai.");

        return pendencias;
    }

    private static string Lista(List<string> nomes) =>
        nomes.Count == 1 ? nomes[0] : $"{string.Join(", ", nomes[..^1])} e {nomes[^1]}";
}

/// <summary>Todas as vigências do planejamento já registradas — a trilha legível (issue 256).</summary>
public sealed class ListarHistoricoDoPlanejamento(
    IRepositorioDoPlanejamento repositorio,
    IRepositorioDeReferenciasDoPotencial referencias,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IRelogio relogio)
{
    /// <summary>Lê o histórico.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<HistoricoDoPlanejamento>>> ExecutarAsync(CancellationToken ct)
    {
        var planejamentos = await repositorio.ListarParametrosAsync(ct);
        var shares = await repositorio.ListarSharesAsync(ct);

        var nomes = await referencias.NomesDosUsuariosAsync(
            ObterParametrosDoPotencial.Autores(planejamentos.Cast<ParametroComVigencia>().Concat(shares)), ct);
        var categorias = await catalogo.CategoriasDeMaquinaAsync(shares.Select(s => s.CategoriaDeMaquinaId).ToHashSet(), ct);

        var historico = new HistoricoDoPlanejamento(
            [.. planejamentos.OrderByDescending(p => p.VigenteDesde).ThenByDescending(p => p.InformadoEm)
                .Select(p => ParametroDoPlanejamentoDetalhe.De(p, nomes))],
            [.. shares.OrderBy(s => s.CategoriaDeMaquinaId).ThenByDescending(s => s.VigenteDesde).ThenByDescending(s => s.InformadoEm)
                .Select(s => ShareAlvoDetalhe.De(s, categorias, nomes))]);

        return Resultado<ComProcedencia<HistoricoDoPlanejamento>>.Ok(
            ComProcedencia<HistoricoDoPlanejamento>.DoNossoBanco(historico, ObterParametrosDoPlanejamento.Tabelas, relogio));
    }
}

/// <summary>Registra uma vigência nova da sazonalidade e dos pesos do IOC (issue 256).</summary>
public sealed class InformarParametroDoPlanejamento(
    IRepositorioDeVigenciasDoPlanejamento vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    private static readonly string[] NomesDosMeses =
        ["janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"];

    /// <summary>Executa o registro.</summary>
    /// <param name="entrada">O conjunto inteiro: doze meses e sete pesos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ParametroDoPlanejamentoDetalhe>> ExecutarAsync(NovoParametroDoPlanejamento entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ParametroDoPlanejamentoDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        var vigenteDesde = LeituraDeParametro.Data(erros, "vigenteDesde", entrada.VigenteDesde, obrigatoria: true);
        LeituraDeParametro.ConferirInicio(erros, vigenteDesde, entrada.VigenteDesde, agora);

        // O ERRO SAI NO MÊS CERTO — "sazonalidade[3]" é abril —, para o formulário marcar a caixa que está errada.
        var meses = new decimal?[12];
        if (entrada.Sazonalidade is not { Count: 12 })
            erros.Registrar("sazonalidade", "Informe os doze meses da sazonalidade, de janeiro a dezembro.");
        else
            for (var i = 0; i < 12; i++)
            {
                meses[i] = LeituraDeParametro.Numero(erros, $"sazonalidade[{i}]", entrada.Sazonalidade[i], true, $"o percentual de {NomesDosMeses[i]}");
                if (meses[i] is { } m && (m < 0 || m > 100 || m != decimal.Round(m, 2)))
                    erros.Registrar($"sazonalidade[{i}]", "O percentual do mês vai de 0 a 100, com duas casas decimais no máximo.", entrada.Sazonalidade[i]);
            }

        var pesos = new (string Campo, string? Texto, string OQueEh)[]
        {
            ("pesoDoPotencial", entrada.PesoDoPotencial, "o peso do potencial"),
            ("pesoDaCobertura", entrada.PesoDaCobertura, "o peso da cobertura"),
            ("pesoDoCredito", entrada.PesoDoCredito, "o peso do crédito"),
            ("pesoDaRentabilidade", entrada.PesoDaRentabilidade, "o peso da rentabilidade"),
            ("pesoDosClientes", entrada.PesoDosClientes, "o peso dos clientes"),
            ("pesoDaRealizacao", entrada.PesoDaRealizacao, "o peso da realização"),
            ("pesoDaPenetracao", entrada.PesoDaPenetracao, "o peso da penetração")
        }.Select(p =>
        {
            var valor = LeituraDeParametro.Numero(erros, p.Campo, p.Texto, true, p.OQueEh);
            if (valor is { } v && (v < 0 || v > 100 || v != decimal.Round(v, 2)))
                erros.Registrar(p.Campo, "O peso vai de 0 a 100, com duas casas decimais no máximo.", p.Texto);
            return valor;
        }).ToArray();

        var justificativa = erros.Obrigatorio("justificativa", entrada.Justificativa, "a justificativa — a decisão ou a fonte destes valores");

        // AS DUAS REGRAS DO CONJUNTO, NO CAMPO QUE AS REPRESENTA: a soma dos meses e a soma dos pesos. O domínio as
        // confere de novo; aqui elas viram recusa com lugar na tela.
        if (meses.All(m => m is not null))
        {
            var soma = meses.Sum(m => m!.Value);
            if (Math.Abs(soma - 100m) > ParametroDoPlanejamento.FolgaDaSazonalidade)
                erros.Registrar(
                    "sazonalidade",
                    $"Os doze meses somam 100% — estes somam {soma.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"))}%. " +
                    "A sazonalidade reparte a demanda do ano, não a aumenta.");
        }

        if (pesos.All(p => p is not null) && pesos.Sum(p => p!.Value) <= 0)
            erros.Registrar("pesoDoPotencial", "Ao menos um peso do IOC é maior que zero — sem peso nenhum, o índice não mede nada.");

        if (erros.TemErro)
            return erros.Recusar<ParametroDoPlanejamentoDetalhe>("A sazonalidade e os pesos do IOC têm campos a corrigir.");

        if (await vigencias.ObterParametroAsync(vigenteDesde!.Value, ct) is not null)
            return LeituraDeParametro.DataOcupada<ParametroDoPlanejamentoDetalhe>(vigenteDesde.Value, entrada.VigenteDesde!);

        ParametroDoPlanejamento parametro;
        try
        {
            parametro = ParametroDoPlanejamento.Informar(
                new ParametroDoPlanejamento.Valores(
                    [.. meses.Select(m => m!.Value)],
                    new PesosDoIoc(pesos[0]!.Value, pesos[1]!.Value, pesos[2]!.Value, pesos[3]!.Value, pesos[4]!.Value, pesos[5]!.Value, pesos[6]!.Value)),
                vigenteDesde.Value, justificativa, acesso.Atual.UsuarioId, agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ParametroDoPlanejamentoDetalhe>.Falha(erro.Message);
        }

        await vigencias.AdicionarAsync(parametro, ct);
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<ParametroDoPlanejamentoDetalhe>.Conflito(gravou.Erro!);

        var nomes = await referencias.NomesDosUsuariosAsync([acesso.Atual.UsuarioId], ct);
        return Resultado<ParametroDoPlanejamentoDetalhe>.Ok(ParametroDoPlanejamentoDetalhe.De(parametro, nomes));
    }
}

/// <summary>Registra uma vigência nova do share-alvo de uma categoria de máquina (issue 256).</summary>
public sealed class InformarShareAlvo(
    IRepositorioDeVigenciasDoPlanejamento vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Executa o registro.</summary>
    /// <param name="entrada">A categoria e o share.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ShareAlvoDetalhe>> ExecutarAsync(NovoShareAlvo entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ShareAlvoDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        var codigo = erros.Obrigatorio("categoriaDeMaquinaCodigo", entrada.CategoriaDeMaquinaCodigo, "a categoria de máquina");
        var percentual = LeituraDeParametro.Numero(erros, "percentual", entrada.Percentual, true, "o share-alvo");
        var vigenteDesde = LeituraDeParametro.Data(erros, "vigenteDesde", entrada.VigenteDesde, obrigatoria: true);
        LeituraDeParametro.ConferirInicio(erros, vigenteDesde, entrada.VigenteDesde, agora);
        var justificativa = erros.Obrigatorio("justificativa", entrada.Justificativa, "a justificativa — por que este share");

        if (percentual is { } p && (p <= 0 || p > 100 || p != decimal.Round(p, 2)))
            erros.Registrar(
                "percentual",
                "O share-alvo é maior que 0% e vai até 100%, com duas casas decimais no máximo. Para a categoria não ter meta, não registre share.",
                entrada.Percentual);

        // SÓ CATEGORIA ATIVA RECEBE VIGÊNCIA NOVA: é decisão para a frente, e a categoria desligada não aparece nas telas.
        ItemDoCatalogoDoPotencial? categoria = null;
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            categoria = await catalogo.ObterCategoriaDeMaquinaAsync(codigo, somenteAtiva: true, ct);
            if (categoria is null)
                erros.Registrar("categoriaDeMaquinaCodigo", "Não há categoria de máquina ativa com este código no catálogo.", entrada.CategoriaDeMaquinaCodigo);
        }

        if (erros.TemErro)
            return erros.Recusar<ShareAlvoDetalhe>("O share-alvo tem campos a corrigir.");

        if (await vigencias.ObterShareAsync(categoria!.Id, vigenteDesde!.Value, ct) is not null)
            return LeituraDeParametro.DataOcupada<ShareAlvoDetalhe>(vigenteDesde.Value, entrada.VigenteDesde!);

        ShareAlvoDaCategoria share;
        try
        {
            share = ShareAlvoDaCategoria.Informar(categoria.Id, percentual!.Value, vigenteDesde.Value, justificativa, acesso.Atual.UsuarioId, agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ShareAlvoDetalhe>.Falha(erro.Message);
        }

        await vigencias.AdicionarAsync(share, ct);
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<ShareAlvoDetalhe>.Conflito(gravou.Erro!);

        var nomes = await referencias.NomesDosUsuariosAsync([acesso.Atual.UsuarioId], ct);
        return Resultado<ShareAlvoDetalhe>.Ok(
            ShareAlvoDetalhe.De(share, new Dictionary<int, ItemDoCatalogoDoPotencial> { [categoria.Id] = categoria }, nomes));
    }
}

/// <summary>
/// AS DUAS REVOGAÇÕES DO PLANEJAMENTO — da sazonalidade com os pesos, e do share-alvo de uma categoria. Só se revoga o
/// que ainda não passou de hoje; o domínio recusa o resto com a explicação.
/// </summary>
public sealed class RevogarParametroDoPlanejamento(
    IRepositorioDeVigenciasDoPlanejamento vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IRepositorioDoCatalogoNoPotencial catalogo,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Revoga a vigência da sazonalidade e dos pesos que começa na data.</summary>
    /// <param name="vigenteDesde">A data de início, aaaa-mm-dd.</param>
    /// <param name="entrada">O motivo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ParametroDoPlanejamentoDetalhe>> RevogarPlanejamentoAsync(
        string vigenteDesde, RevogacaoDeVigencia entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ParametroDoPlanejamentoDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "vigenteDesde", vigenteDesde, obrigatoria: true);
        var motivo = erros.Obrigatorio("motivo", entrada.Motivo, "o motivo da revogação");
        if (erros.TemErro) return erros.Recusar<ParametroDoPlanejamentoDetalhe>("A revogação tem campos a corrigir.");

        var parametro = await vigencias.ObterParametroAsync(data!.Value, ct);
        if (parametro is null)
            return Resultado<ParametroDoPlanejamentoDetalhe>.NaoEncontrado(
                $"Não há vigência de pé da sazonalidade e dos pesos começando em {data:yyyy-MM-dd}.");

        return await RevogarAsync(parametro, motivo, ParametroDoPlanejamentoDetalhe.De, ct);
    }

    /// <summary>Revoga a vigência do share-alvo de uma categoria que começa na data.</summary>
    /// <param name="categoriaDeMaquinaCodigo">A categoria — <c>TRATOR</c>, <c>COLHEITADEIRA</c>…</param>
    /// <param name="vigenteDesde">A data de início, aaaa-mm-dd.</param>
    /// <param name="entrada">O motivo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ShareAlvoDetalhe>> RevogarShareAsync(
        string categoriaDeMaquinaCodigo, string vigenteDesde, RevogacaoDeVigencia entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ShareAlvoDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "vigenteDesde", vigenteDesde, obrigatoria: true);
        var motivo = erros.Obrigatorio("motivo", entrada.Motivo, "o motivo da revogação");
        if (erros.TemErro) return erros.Recusar<ShareAlvoDetalhe>("A revogação tem campos a corrigir.");

        // QUALQUER ESTADO: revogar o share de uma categoria desligada depois continua possível.
        var categoria = await catalogo.ObterCategoriaDeMaquinaAsync(categoriaDeMaquinaCodigo, somenteAtiva: false, ct);
        var share = categoria is null ? null : await vigencias.ObterShareAsync(categoria.Id, data!.Value, ct);
        if (share is null)
            return Resultado<ShareAlvoDetalhe>.NaoEncontrado(
                $"Não há vigência de pé do share-alvo de {categoriaDeMaquinaCodigo} começando em {data:yyyy-MM-dd}.");

        var categorias = new Dictionary<int, ItemDoCatalogoDoPotencial> { [categoria!.Id] = categoria };
        return await RevogarAsync(share, motivo, (s, nomes) => ShareAlvoDetalhe.De(s, categorias, nomes), ct);
    }

    private async Task<Resultado<TDetalhe>> RevogarAsync<TParametro, TDetalhe>(
        TParametro parametro, string motivo, Func<TParametro, IReadOnlyDictionary<long, string>, TDetalhe> detalhar, CancellationToken ct)
        where TParametro : ParametroComVigencia
    {
        try
        {
            parametro.Revogar(motivo, acesso.Atual.UsuarioId, relogio.Agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<TDetalhe>.Conflito(erro.Message);
        }

        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<TDetalhe>.Conflito(gravou.Erro!);

        var nomes = await referencias.NomesDosUsuariosAsync(ObterParametrosDoPotencial.Autores([parametro]), ct);
        return Resultado<TDetalhe>.Ok(detalhar(parametro, nomes));
    }
}
