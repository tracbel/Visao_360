using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>
/// A LEITURA DOS CAMPOS DE UM PARÂMETRO — data e número vêm como texto, e o erro sai campo a campo, no
/// formato de toda a API (o mesmo caminho de <see cref="ColetorDeErros"/>).
/// </summary>
internal static class LeituraDeParametro
{
    /// <summary>Lê uma data aaaa-mm-dd.</summary>
    public static DateOnly? Data(ColetorDeErros erros, string campo, string? texto, bool obrigatoria)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            if (obrigatoria) erros.Registrar(campo, "Informe a data, no formato aaaa-mm-dd (ex.: 2026-10-01).", texto);
            return null;
        }

        if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            return data;

        erros.Registrar(campo, "Use o formato aaaa-mm-dd, por exemplo 2026-10-01.", texto);
        return null;
    }

    /// <summary>
    /// Lê um número com vírgula ou ponto decimal ("0,70" ou "0.70"). Separador de milhar não é aceito: os
    /// parâmetros são números pequenos, e "1.000" seria ambíguo entre mil e um.
    /// </summary>
    public static decimal? Numero(ColetorDeErros erros, string campo, string? texto, bool obrigatorio, string oQueEh)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            if (obrigatorio) erros.Registrar(campo, $"Informe {oQueEh}.", texto);
            return null;
        }

        var normalizado = texto.Trim().Replace(',', '.');
        if (normalizado.Count(c => c == '.') <= 1
            && decimal.TryParse(normalizado, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var numero))
            return numero;

        erros.Registrar(campo, $"{char.ToUpperInvariant(oQueEh[0])}{oQueEh[1..]} é um número, com vírgula ou ponto decimal e sem separador de milhar.", texto);
        return null;
    }

    /// <summary>Lê um número inteiro.</summary>
    public static int? Inteiro(ColetorDeErros erros, string campo, string? texto, string oQueEh)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            erros.Registrar(campo, $"Informe {oQueEh}.", texto);
            return null;
        }

        if (int.TryParse(texto.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var numero)) return numero;

        erros.Registrar(campo, $"{char.ToUpperInvariant(oQueEh[0])}{oQueEh[1..]} é um número inteiro.", texto);
        return null;
    }

    /// <summary>
    /// Confere que a vigência começa hoje ou depois — antes de chegar ao domínio, para a recusa sair no campo
    /// certo em vez de numa frase solta.
    /// </summary>
    public static void ConferirInicio(ColetorDeErros erros, DateOnly? vigenteDesde, string? texto, DateTime agoraUtc)
    {
        var hoje = ParametroComVigencia.HojeNoBrasil(agoraUtc);
        if (vigenteDesde is { } data && data < hoje)
            erros.Registrar(
                "vigenteDesde",
                $"A vigência começa hoje ({hoje:yyyy-MM-dd}) ou depois. O cálculo de uma data passada usa o parâmetro que valia " +
                "naquela data — mudar o passado mudaria um número já mostrado.",
                texto);
    }

    /// <summary>A recusa por falta de permissão, com o nome da permissão.</summary>
    public static Resultado<T> SemPermissao<T>(string permissao) =>
        Resultado<T>.SemPermissao($"Falta a permissão '{permissao}' ({Permissoes.Catalogo[permissao]}).");

    /// <summary>A recusa por data já ocupada por uma vigência de pé.</summary>
    public static Resultado<T> DataOcupada<T>(DateOnly vigenteDesde, string texto) =>
        Resultado<T>.Conflito(
            "Já existe uma vigência de pé nesta data.",
            [new ErroDeCampo("vigenteDesde",
                $"Já há vigência começando em {vigenteDesde:yyyy-MM-dd}. Para trocá-la, revogue-a antes (ela ainda não passou de hoje) " +
                "ou escolha outra data.", texto)]);
}

/// <summary>Registra uma vigência nova dos parâmetros gerais (issue 71).</summary>
public sealed class InformarParametroDoPotencial(
    IRepositorioDeVigenciasDoPotencial vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Executa o registro.</summary>
    /// <param name="entrada">O conjunto inteiro dos parâmetros gerais.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ParametrosGeraisDetalhe>> ExecutarAsync(NovoParametroDoPotencial entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41).
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ParametrosGeraisDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        var vigenteDesde = LeituraDeParametro.Data(erros, "vigenteDesde", entrada.VigenteDesde, obrigatoria: true);
        LeituraDeParametro.ConferirInicio(erros, vigenteDesde, entrada.VigenteDesde, agora);

        var meses = LeituraDeParametro.Numero(erros, "mesesDaJanela", entrada.MesesDaJanela, true, "os meses da janela dos índices");
        var contratos = LeituraDeParametro.Numero(erros, "pesoDosContratosNoCredito", entrada.PesoDosContratosNoCredito, true, "o peso dos contratos no crédito");
        var retracao = LeituraDeParametro.Numero(erros, "limiteDeRetracao", entrada.LimiteDeRetracao, true, "o limite de retração");
        var aquecimento = LeituraDeParametro.Numero(erros, "limiteDeAquecimento", entrada.LimiteDeAquecimento, true, "o limite de aquecimento");
        var superaquecimento = LeituraDeParametro.Numero(erros, "limiteDeSuperaquecimento", entrada.LimiteDeSuperaquecimento, true, "o limite de superaquecimento");
        var percepcao = LeituraDeParametro.Numero(erros, "limiteDaPercepcao", entrada.LimiteDaPercepcao, true, "o limite da percepção do gestor");
        var pesoPreco = LeituraDeParametro.Numero(erros, "pesoDoIndicadorDePreco", entrada.PesoDoIndicadorDePreco, false, "o peso do indicador de preço");
        var pesoCredito = LeituraDeParametro.Numero(erros, "pesoDoIndicadorDeCredito", entrada.PesoDoIndicadorDeCredito, false, "o peso do indicador de crédito");
        var pesoComercial = LeituraDeParametro.Numero(erros, "pesoDoIndicadorComercial", entrada.PesoDoIndicadorComercial, false, "o peso do indicador comercial");
        var fatorMinimo = LeituraDeParametro.Numero(erros, "fatorMinimo", entrada.FatorMinimo, false, "o fator mínimo");
        var fatorMaximo = LeituraDeParametro.Numero(erros, "fatorMaximo", entrada.FatorMaximo, false, "o fator máximo");
        var justificativa = erros.Obrigatorio("justificativa", entrada.Justificativa, "a justificativa — a decisão ou a fonte destes valores");

        if (meses is { } m && (m != decimal.Truncate(m) || m is < 1 or > 60))
            erros.Registrar("mesesDaJanela", "A janela é um número inteiro de meses, de 1 a 60.", entrada.MesesDaJanela);

        if (erros.TemErro)
            return erros.Recusar<ParametrosGeraisDetalhe>("Os parâmetros gerais têm campos a corrigir.");

        if (await vigencias.ObterGeralAsync(vigenteDesde!.Value, ct) is not null)
            return LeituraDeParametro.DataOcupada<ParametrosGeraisDetalhe>(vigenteDesde.Value, entrada.VigenteDesde!);

        ParametroDoPotencial parametro;
        try
        {
            parametro = ParametroDoPotencial.Informar(
                new ParametroDoPotencial.Valores(
                    (short)meses!.Value, contratos!.Value, retracao!.Value, aquecimento!.Value, superaquecimento!.Value,
                    entrada.NomeDaFaixaIntermediaria, percepcao!.Value, pesoPreco, pesoCredito, pesoComercial, fatorMinimo, fatorMaximo),
                vigenteDesde.Value, justificativa, acesso.Atual.UsuarioId, agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ParametrosGeraisDetalhe>.Falha(erro.Message);
        }

        await vigencias.AdicionarAsync(parametro, ct);
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<ParametrosGeraisDetalhe>.Conflito(gravou.Erro!);

        var nomes = await referencias.NomesDosUsuariosAsync([acesso.Atual.UsuarioId], ct);
        return Resultado<ParametrosGeraisDetalhe>.Ok(ParametrosGeraisDetalhe.De(parametro, nomes));
    }
}

/// <summary>Registra uma vigência nova da regra de uma cultura (issue 71, D-P01).</summary>
public sealed class InformarRegraDePotencial(
    IRepositorioDeVigenciasDoPotencial vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Executa o registro.</summary>
    /// <param name="entrada">A regra.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RegraDePotencialDetalhe>> ExecutarAsync(NovaRegraDePotencial entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<RegraDePotencialDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        var produto = LeituraDeParametro.Inteiro(erros, "produtoCodigoIbge", entrada.ProdutoCodigoIbge, "o código do produto na classificação 782 do IBGE");
        var hectares = LeituraDeParametro.Numero(erros, "hectaresPorMaquina", entrada.HectaresPorMaquina, true, "os hectares por máquina");
        var anos = LeituraDeParametro.Numero(erros, "anosDeRenovacao", entrada.AnosDeRenovacao, false, "os anos de renovação");
        var modelo = erros.Obrigatorio("modeloDeReferencia", entrada.ModeloDeReferencia, "o modelo de referência");
        var situacao = erros.ItemDeDominio<SituacaoDaRegraDePotencial>("situacao", entrada.Situacao);
        var vigenteDesde = LeituraDeParametro.Data(erros, "vigenteDesde", entrada.VigenteDesde, obrigatoria: true);
        LeituraDeParametro.ConferirInicio(erros, vigenteDesde, entrada.VigenteDesde, agora);
        var justificativa = erros.Obrigatorio("justificativa", entrada.Justificativa, "a justificativa — a decisão ou a fonte destes valores");

        // O PRODUTO PRECISA ESTAR NA PAM CARREGADA: a regra divide a área dele. O rótulo vem de lá, oficial —
        // nunca digitado.
        string? produtoNome = null;
        if (produto is { } codigo)
        {
            produtoNome = await referencias.ObterNomeDoProdutoNaPamAsync(codigo, ct);
            if (produtoNome is null)
                erros.Registrar(
                    "produtoCodigoIbge",
                    "Este produto não está na Produção Agrícola Municipal carregada. A regra divide a área do produto, e sem área " +
                    "não há o que dividir. Use o código da classificação 782 do IBGE de um produto carregado (ex.: 40139 é café em grão, total).",
                    entrada.ProdutoCodigoIbge);
        }

        if (erros.TemErro)
            return erros.Recusar<RegraDePotencialDetalhe>("A regra da cultura tem campos a corrigir.");

        if (await vigencias.ObterRegraAsync(produto!.Value, vigenteDesde!.Value, ct) is not null)
            return LeituraDeParametro.DataOcupada<RegraDePotencialDetalhe>(vigenteDesde.Value, entrada.VigenteDesde!);

        RegraDePotencial regra;
        try
        {
            regra = RegraDePotencial.Informar(
                produto.Value, produtoNome!, hectares!.Value, anos, modelo, situacao!.Value,
                vigenteDesde.Value, justificativa, acesso.Atual.UsuarioId, agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<RegraDePotencialDetalhe>.Falha(erro.Message);
        }

        await vigencias.AdicionarAsync(regra, ct);
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<RegraDePotencialDetalhe>.Conflito(gravou.Erro!);

        var nomes = await referencias.NomesDosUsuariosAsync([acesso.Atual.UsuarioId], ct);
        return Resultado<RegraDePotencialDetalhe>.Ok(RegraDePotencialDetalhe.De(regra, nomes));
    }
}

/// <summary>Registra uma vigência nova da percepção do gestor sobre um município (issue 71, D-P04).</summary>
public sealed class InformarPercepcaoDoGestor(
    IRepositorioDeParametrosDoPotencial parametros,
    IRepositorioDeVigenciasDoPotencial vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Executa o registro.</summary>
    /// <param name="entrada">A percepção.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<PercepcaoDoGestorDetalhe>> ExecutarAsync(NovaPercepcaoDoGestor entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.PercepcaoDoGestorInformar))
            return LeituraDeParametro.SemPermissao<PercepcaoDoGestorDetalhe>(Permissoes.PercepcaoDoGestorInformar);

        var agora = relogio.Agora;
        var erros = new ColetorDeErros();

        var codigo = LeituraDeParametro.Inteiro(erros, "municipioCodigoIbge", entrada.MunicipioCodigoIbge, "o código IBGE do município");
        var percentual = LeituraDeParametro.Numero(erros, "percentual", entrada.Percentual, true, "o percentual da percepção");
        var vigenteDesde = LeituraDeParametro.Data(erros, "vigenteDesde", entrada.VigenteDesde, obrigatoria: true);
        LeituraDeParametro.ConferirInicio(erros, vigenteDesde, entrada.VigenteDesde, agora);
        var justificativa = erros.Obrigatorio("justificativa", entrada.Justificativa, "a justificativa — por que este ajuste");

        MunicipioDoParametro? municipio = null;
        if (codigo is { } c)
        {
            municipio = await referencias.ObterMunicipioPorCodigoIbgeAsync(c, ct);
            if (municipio is null)
                erros.Registrar("municipioCodigoIbge", "Não há município com este código IBGE no catálogo.", entrada.MunicipioCodigoIbge);
        }

        // O LIMITE É O DOS PARÂMETROS GERAIS VIGENTES NA DATA DE INÍCIO — não um número do código (D-P04 disse
        // ±5%, e a decisão mora na vigência, onde o administrador pode revê-la).
        ParametroDoPotencial? geral = null;
        if (vigenteDesde is { } inicio)
        {
            geral = ParametroComVigencia.VigenteEm(await parametros.ListarGeraisAsync(ct), inicio);
            if (geral is null)
                erros.Registrar("vigenteDesde", "Não há parâmetros gerais vigentes nesta data, e é deles que sai o limite da percepção.", entrada.VigenteDesde);
            else if (percentual is { } p && Math.Abs(p) > geral.LimiteDaPercepcao)
                erros.Registrar(
                    "percentual",
                    $"A percepção vai de -{geral.LimiteDaPercepcao.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"))} a " +
                    $"+{geral.LimiteDaPercepcao.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"))} pontos percentuais nesta data.",
                    entrada.Percentual);
        }

        if (erros.TemErro)
            return erros.Recusar<PercepcaoDoGestorDetalhe>("A percepção tem campos a corrigir.");

        if (await vigencias.ObterPercepcaoAsync(municipio!.Id, vigenteDesde!.Value, ct) is not null)
            return LeituraDeParametro.DataOcupada<PercepcaoDoGestorDetalhe>(vigenteDesde.Value, entrada.VigenteDesde!);

        PercepcaoDoGestor percepcao;
        try
        {
            percepcao = PercepcaoDoGestor.Informar(
                municipio.Id, percentual!.Value, geral!.LimiteDaPercepcao, vigenteDesde.Value, justificativa, acesso.Atual.UsuarioId, agora);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<PercepcaoDoGestorDetalhe>.Falha(erro.Message);
        }

        await vigencias.AdicionarAsync(percepcao, ct);
        var gravou = await unidade.SalvarAsync(ct);
        if (!gravou.EhSucesso) return Resultado<PercepcaoDoGestorDetalhe>.Conflito(gravou.Erro!);

        var nomes = await referencias.NomesDosUsuariosAsync([acesso.Atual.UsuarioId], ct);
        return Resultado<PercepcaoDoGestorDetalhe>.Ok(
            new PercepcaoDoGestorDetalhe(municipio.CodigoIbge, municipio.Nome, municipio.Uf, percepcao.Percentual, VigenciaDoParametro.De(percepcao, nomes)));
    }
}

/// <summary>
/// AS TRÊS REVOGAÇÕES — da regra de uma cultura, dos parâmetros gerais e da percepção de um município. Só se
/// revoga o que ainda não passou de hoje; o domínio recusa o resto com a explicação.
/// </summary>
public sealed class RevogarParametroDoPotencial(
    IRepositorioDeVigenciasDoPotencial vigencias,
    IRepositorioDeReferenciasDoPotencial referencias,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso,
    IRelogio relogio)
{
    /// <summary>Revoga a vigência dos parâmetros gerais que começa na data.</summary>
    /// <param name="vigenteDesde">A data de início, aaaa-mm-dd.</param>
    /// <param name="entrada">O motivo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ParametrosGeraisDetalhe>> RevogarGeralAsync(string vigenteDesde, RevogacaoDeVigencia entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<ParametrosGeraisDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "vigenteDesde", vigenteDesde, obrigatoria: true);
        var motivo = erros.Obrigatorio("motivo", entrada.Motivo, "o motivo da revogação");
        if (erros.TemErro) return erros.Recusar<ParametrosGeraisDetalhe>("A revogação tem campos a corrigir.");

        var parametro = await vigencias.ObterGeralAsync(data!.Value, ct);
        if (parametro is null)
            return Resultado<ParametrosGeraisDetalhe>.NaoEncontrado($"Não há vigência de pé dos parâmetros gerais começando em {data:yyyy-MM-dd}.");

        return await RevogarAsync(parametro, motivo, ParametrosGeraisDetalhe.De, ct);
    }

    /// <summary>Revoga a vigência da regra de um produto que começa na data.</summary>
    /// <param name="produtoCodigoIbge">O produto.</param>
    /// <param name="vigenteDesde">A data de início, aaaa-mm-dd.</param>
    /// <param name="entrada">O motivo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RegraDePotencialDetalhe>> RevogarRegraAsync(
        int produtoCodigoIbge, string vigenteDesde, RevogacaoDeVigencia entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.ParametroDoPotencialAdministrar))
            return LeituraDeParametro.SemPermissao<RegraDePotencialDetalhe>(Permissoes.ParametroDoPotencialAdministrar);

        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "vigenteDesde", vigenteDesde, obrigatoria: true);
        var motivo = erros.Obrigatorio("motivo", entrada.Motivo, "o motivo da revogação");
        if (erros.TemErro) return erros.Recusar<RegraDePotencialDetalhe>("A revogação tem campos a corrigir.");

        var regra = await vigencias.ObterRegraAsync(produtoCodigoIbge, data!.Value, ct);
        if (regra is null)
            return Resultado<RegraDePotencialDetalhe>.NaoEncontrado(
                $"Não há vigência de pé da regra do produto {produtoCodigoIbge} começando em {data:yyyy-MM-dd}.");

        return await RevogarAsync(regra, motivo, RegraDePotencialDetalhe.De, ct);
    }

    /// <summary>Revoga a vigência da percepção de um município que começa na data.</summary>
    /// <param name="municipioCodigoIbge">O código IBGE do município.</param>
    /// <param name="vigenteDesde">A data de início, aaaa-mm-dd.</param>
    /// <param name="entrada">O motivo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<PercepcaoDoGestorDetalhe>> RevogarPercepcaoAsync(
        int municipioCodigoIbge, string vigenteDesde, RevogacaoDeVigencia entrada, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.PercepcaoDoGestorInformar))
            return LeituraDeParametro.SemPermissao<PercepcaoDoGestorDetalhe>(Permissoes.PercepcaoDoGestorInformar);

        var erros = new ColetorDeErros();
        var data = LeituraDeParametro.Data(erros, "vigenteDesde", vigenteDesde, obrigatoria: true);
        var motivo = erros.Obrigatorio("motivo", entrada.Motivo, "o motivo da revogação");
        if (erros.TemErro) return erros.Recusar<PercepcaoDoGestorDetalhe>("A revogação tem campos a corrigir.");

        var municipio = await referencias.ObterMunicipioPorCodigoIbgeAsync(municipioCodigoIbge, ct);
        var percepcao = municipio is null ? null : await vigencias.ObterPercepcaoAsync(municipio.Id, data!.Value, ct);
        if (percepcao is null)
            return Resultado<PercepcaoDoGestorDetalhe>.NaoEncontrado(
                $"Não há vigência de pé da percepção do município {municipioCodigoIbge} começando em {data:yyyy-MM-dd}.");

        return await RevogarAsync(
            percepcao, motivo,
            (p, nomes) => new PercepcaoDoGestorDetalhe(municipio!.CodigoIbge, municipio.Nome, municipio.Uf, p.Percentual, VigenciaDoParametro.De(p, nomes)),
            ct);
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
