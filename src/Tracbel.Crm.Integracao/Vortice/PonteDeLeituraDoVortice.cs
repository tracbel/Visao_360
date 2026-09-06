using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>
/// A PONTE DE LEITURA para o Vórtice — a única classe da solução que pronuncia o vocabulário do
/// legado, e a única que fala com o banco dele.
///
/// <para><b>Somente leitura, e a garantia é estrutural:</b> não existe aqui um <c>INSERT</c>, um
/// <c>UPDATE</c> ou uma chamada de procedimento. Toda consulta usa <c>WITH (NOLOCK)</c> para não
/// encostar em bloqueio de um sistema que está em produção agora.</para>
///
/// <para><b>Só objetos VIVOS.</b> Esta é a decisão mais importante do arquivo, e está em
/// <see cref="ObjetosDoLegado"/>: o legado tem tabelas atualizadas hoje e tabelas paradas há
/// anos, com a mesma cara. A ponte lê as vivas, e as mortas estão nomeadas com a data em que
/// pararam — expostas pela verificação de saúde, para ninguém precisar redescobrir isso.</para>
///
/// <para><b>Cair é um resultado.</b> A ponte depende de VPN. Quando o servidor não responde, o
/// método devolve <see cref="TipoDeFalha.DependenciaIndisponivel"/> com a frase que o usuário
/// lê — nunca exceção subindo para virar 500, e nunca derrubando o resto da API.</para>
/// </summary>
public sealed class PonteDeLeituraDoVortice(
    IOptions<OpcoesDoVortice> opcoes,
    IRelogio relogio,
    ILogger<PonteDeLeituraDoVortice> log) : IPonteDeLeituraDoVortice
{
    /// <summary>O nome do sistema de origem, como ele aparece na procedência de toda resposta.</summary>
    public const string NomeDoSistema = "Vórtice";

    /// <summary>
    /// O QUE ESTÁ VIVO E O QUE ESTÁ MORTO no banco do legado, com a data medida.
    ///
    /// Esta tabela é conhecimento caro e some se não ficar escrito: as tabelas de faturamento, de
    /// frota do ERP, de ordem de serviço e de título financeiro PARECEM disponíveis — respondem à
    /// consulta, têm centenas de milhares de linhas — e estão congeladas. Foi assim que 17 meses
    /// de dado velho passaram por atual.
    ///
    /// A ponte NÃO lê nenhum objeto marcado como parado. Eles estão aqui para a verificação de
    /// saúde poder dizer o motivo a quem perguntar "e o faturamento?".
    /// </summary>
    public static IReadOnlyList<(string Objeto, bool Vivo, string Situacao)> ObjetosDoLegado { get; } =
    [
        ("GE_Pessoa", true, "vivo — cadastro de pessoas, alterado diariamente"),
        ("IV_ClientePropr", true, "vivo — parque de máquinas declarado pelo CEN, alterado hoje"),
        ("IV_Processo", true, "vivo — oportunidades e casos do fluxo"),
        ("IV_Agenda", true, "vivo — agenda e tarefas"),
        ("IV_Historico", true, "vivo — histórico de interações"),
        ("EXT_Veic", false, "PARADO desde 24/05/2024 — frota vinda do ERP; não use como parque atual"),
        ("EXT_NFS", false, "PARADO desde 11/04/2025 — faturamento; não use como venda atual"),
        ("EXT_OS", false, "PARADO — ordens de serviço nunca promovidas do staging"),
        ("EXT_Titulo", false, "PARADO — títulos financeiros presos em staging desde 05/2025")
    ];

    /// <summary>
    /// As categorias de máquina do parque, no cadastro genérico de propriedades do legado.
    ///
    /// O legado guarda o parque numa tabela de pares atributo-valor, e o tipo de máquina é o
    /// identificador da propriedade. Estes são os que a medição de 04/09/2026 mostrou com
    /// alteração recente; os demais identificadores da mesma tabela são cadastros aposentados
    /// (o mais novo deles parou em 2018) e ficam de fora.
    /// </summary>
    private static readonly int[] CategoriasDeMaquina =
        [9402, 9403, 9404, 9405, 9406, 9407, 9408, 9409, 9410, 9411, 9412];

    /// <inheritdoc />
    public Task<Resultado<LeituraDoLegado<IReadOnlyList<ClienteNoLegado>>>> BuscarClientesAsync(
        string termo, int limite, CancellationToken ct) =>
        LerAsync<IReadOnlyList<ClienteNoLegado>>(
            "GE_Pessoa",
            comando =>
            {
                comando.CommandText = """
                    SELECT TOP (@limite)
                           p.SeqPessoa, p.NomeRazao, p.Fantasia, p.FisicaJuridica, p.Status,
                           p.NroCGCCPF, p.DigCGCCPF, p.Cidade, p.Uf, p.Email,
                           p.FoneDDD1, p.FoneNro1,
                           COALESCE(p.DtaAlteracao, p.DtaInclusao) AS AtualizadoEm
                    FROM GE_Pessoa p WITH (NOLOCK)
                    WHERE p.NomeRazao LIKE @texto
                       OR p.Fantasia LIKE @texto
                       OR (LEN(@numeros) >= 3
                           AND CAST(CAST(p.NroCGCCPF AS bigint) AS varchar(20)) LIKE '%' + @numeros + '%')
                    ORDER BY p.NomeRazao
                    """;

                comando.Parameters.Add("@limite", SqlDbType.Int).Value = limite;
                comando.Parameters.Add("@texto", SqlDbType.VarChar, 120).Value = "%" + termo + "%";
                comando.Parameters.Add("@numeros", SqlDbType.VarChar, 20).Value =
                    new string(termo.Where(char.IsDigit).ToArray());
            },
            LerClientesAsync,
            ct);

    /// <inheritdoc />
    public Task<Resultado<LeituraDoLegado<IReadOnlyList<EquipamentoNoLegado>>>> ListarParqueDeMaquinasAsync(
        long identificadorNoLegado, CancellationToken ct) =>
        LerAsync<IReadOnlyList<EquipamentoNoLegado>>(
            "IV_ClientePropr",
            comando =>
            {
                // Os rótulos de Campo1..Campo6 mudam por categoria — "Modelo" está no Campo2 do
                // trator e no Campo1 da plataforma. Por isso os rótulos vêm JUNTO, da tabela de
                // definição, e o casamento é feito por nome do rótulo, não por posição.
                comando.CommandText = """
                    SELECT cp.SeqPropPessoa, pr.Propriedade, cp.Referencia, cp.Ativo,
                           cp.Campo1, cp.Campo2, cp.Campo3, cp.Campo4, cp.Campo5, cp.Campo6,
                           pr.Campo1 AS Rotulo1, pr.Campo2 AS Rotulo2, pr.Campo3 AS Rotulo3,
                           pr.Campo4 AS Rotulo4, pr.Campo5 AS Rotulo5, pr.Campo6 AS Rotulo6,
                           COALESCE(cp.DtaAlteracao, cp.DtaInclusao) AS AtualizadoEm
                    FROM IV_ClientePropr cp WITH (NOLOCK)
                    JOIN IV_Propriedade pr WITH (NOLOCK) ON pr.SeqPropriedade = cp.SeqPropriedade
                    WHERE cp.SeqPessoa = @pessoa
                      AND cp.SeqPropriedade IN (9402,9403,9404,9405,9406,9407,9408,9409,9410,9411,9412)
                    ORDER BY COALESCE(cp.DtaAlteracao, cp.DtaInclusao) DESC
                    """;

                comando.Parameters.Add("@pessoa", SqlDbType.Decimal).Value = identificadorNoLegado;
            },
            LerParqueAsync,
            ct);

    /// <inheritdoc />
    public Task<Resultado<LeituraDoLegado<SaudeDaPonte>>> VerificarAsync(CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();

        return LerAsync<SaudeDaPonte>(
            "GE_Pessoa",
            comando => comando.CommandText =
                "SELECT MAX(COALESCE(DtaAlteracao, DtaInclusao)) FROM GE_Pessoa WITH (NOLOCK)",
            async (leitor, _, ct2) =>
            {
                DateTime? maisRecente = null;
                if (await leitor.ReadAsync(ct2) && !leitor.IsDBNull(0))
                    maisRecente = leitor.GetDateTime(0);

                cronometro.Stop();

                var detalhe =
                    "A ponte respondeu. Objetos vivos: " +
                    string.Join("; ", ObjetosDoLegado.Where(o => o.Vivo).Select(o => o.Objeto)) +
                    ". Objetos PARADOS, que a ponte não lê: " +
                    string.Join("; ", ObjetosDoLegado.Where(o => !o.Vivo).Select(o => $"{o.Objeto} ({o.Situacao})")) +
                    ".";

                return (new SaudeDaPonte(true, detalhe, cronometro.ElapsedMilliseconds), maisRecente);
            },
            ct);
    }

    // =============================================================================================
    // O encanamento — aberto uma vez, com o mesmo tratamento de falha para os três métodos.
    // =============================================================================================

    /// <summary>
    /// Abre a conexão, executa a consulta, traduz o resultado e — o que mais importa — converte
    /// qualquer falha de infraestrutura numa recusa que o usuário entende.
    /// </summary>
    private async Task<Resultado<LeituraDoLegado<T>>> LerAsync<T>(
        string objeto,
        Action<SqlCommand> montarConsulta,
        Func<SqlDataReader, List<AjusteNaLeitura>, CancellationToken, Task<(T Dados, DateTime? MaisRecente)>> traduzir,
        CancellationToken ct)
    {
        var config = opcoes.Value;

        if (string.IsNullOrWhiteSpace(config.Conexao))
            return Resultado<LeituraDoLegado<T>>.Indisponivel(
                "A ponte de leitura do sistema legado não está configurada nesta instalação. " +
                "Defina a variável de ambiente Vortice__Conexao com a cadeia de conexão da conta " +
                "de leitura. O cadastro do CRM continua funcionando sem ela.");

        try
        {
            await using var conexao = new SqlConnection(config.Conexao);
            await conexao.OpenAsync(ct);

            await using var comando = conexao.CreateCommand();
            comando.CommandTimeout = config.TempoLimiteSegundos;
            montarConsulta(comando);

            var ajustes = new List<AjusteNaLeitura>();

            await using var leitor = await comando.ExecuteReaderAsync(ct);
            var (dados, maisRecente) = await traduzir(leitor, ajustes, ct);

            return Resultado<LeituraDoLegado<T>>.Ok(
                new LeituraDoLegado<T>(dados, Carimbar(objeto, maisRecente, config)));
        }
        catch (Exception erro) when (erro is SqlException or InvalidOperationException or TimeoutException)
        {
            // O DETALHE TÉCNICO VAI PARA O LOG, e só a orientação vai para quem chamou. Devolver a
            // mensagem do driver exporia nome de servidor e topologia de rede numa resposta HTTP.
            log.LogWarning(
                erro,
                "A ponte de leitura do sistema legado não respondeu ao consultar {Objeto}.", objeto);

            return Resultado<LeituraDoLegado<T>>.Indisponivel(
                "O sistema legado não respondeu. Quase sempre é a VPN: confira se ela está " +
                "conectada e tente de novo. O cadastro do CRM não depende desta ponte e continua " +
                "funcionando normalmente.");
        }
    }

    /// <summary>
    /// Monta a procedência da leitura — o carimbo que impede dado velho de passar por atual.
    /// </summary>
    private Procedencia Carimbar(string objeto, DateTime? maisRecente, OpcoesDoVortice config)
    {
        var agora = relogio.Agora;

        var idadeEmDias = maisRecente is { } data ? (agora - data).TotalDays : (double?)null;
        var velho = idadeEmDias >= config.DiasParaConsiderarDesatualizado;

        var aviso = velho
            ? $"A alteração mais recente encontrada em {objeto} é de " +
              $"{maisRecente:dd/MM/yyyy} — há {(int)idadeEmDias!} dias. Trate como histórico, " +
              "não como situação atual."
            : null;

        return new Procedencia(NomeDoSistema, objeto, agora, maisRecente, velho, aviso);
    }

    // =============================================================================================
    // A tradução — daqui para dentro é vocabulário do legado; daqui para fora, o nosso.
    // =============================================================================================

    private static async Task<(IReadOnlyList<ClienteNoLegado> Dados, DateTime? MaisRecente)> LerClientesAsync(
        SqlDataReader leitor, List<AjusteNaLeitura> _, CancellationToken ct)
    {
        var clientes = new List<ClienteNoLegado>();
        DateTime? maisRecente = null;

        while (await leitor.ReadAsync(ct))
        {
            var ajustes = new List<AjusteNaLeitura>();

            var ehFisica = Texto(leitor, "FisicaJuridica") is "F";

            var (documento, confere) = SaneamentoDoLegado.RecomporDocumento(
                Decimal(leitor, "NroCGCCPF"), Decimal(leitor, "DigCGCCPF"), ehFisica, ajustes);

            var atualizado = Data(leitor, "AtualizadoEm");
            if (atualizado > (maisRecente ?? DateTime.MinValue)) maisRecente = atualizado;

            clientes.Add(new ClienteNoLegado(
                IdentificadorNoLegado: (long)Decimal(leitor, "SeqPessoa")!.Value,
                NomeRazao: SaneamentoDoLegado.Apararar(Texto(leitor, "NomeRazao")) ?? "(sem nome)",
                NomeFantasia: SaneamentoDoLegado.Apararar(Texto(leitor, "Fantasia")),
                TipoDePessoa: ehFisica ? TipoDePessoa.Fisica : TipoDePessoa.Juridica,
                Documento: documento,
                DocumentoConfere: confere,
                Cidade: SaneamentoDoLegado.Apararar(Texto(leitor, "Cidade")),
                Uf: SaneamentoDoLegado.Apararar(Texto(leitor, "Uf")),
                Email: SaneamentoDoLegado.NormalizarEmail(Texto(leitor, "Email"), ajustes),
                Telefone: SaneamentoDoLegado.NormalizarTelefone(
                    Texto(leitor, "FoneDDD1"), Decimal(leitor, "FoneNro1"), ajustes),
                SituacaoNoLegado: DescreverSituacao(Texto(leitor, "Status")),
                AtualizadoEm: atualizado,
                Ajustes: ajustes));
        }

        return (clientes, maisRecente);
    }

    private static async Task<(IReadOnlyList<EquipamentoNoLegado> Dados, DateTime? MaisRecente)> LerParqueAsync(
        SqlDataReader leitor, List<AjusteNaLeitura> _, CancellationToken ct)
    {
        var maquinas = new List<EquipamentoNoLegado>();
        DateTime? maisRecente = null;

        while (await leitor.ReadAsync(ct))
        {
            var ajustes = new List<AjusteNaLeitura>();

            var campos = Enumerable.Range(1, 6)
                .Select(i => (
                    Rotulo: SaneamentoDoLegado.Apararar(Texto(leitor, $"Rotulo{i}")),
                    Valor: SaneamentoDoLegado.Apararar(Texto(leitor, $"Campo{i}"))))
                .ToList();

            string? PorRotulo(string rotulo) => campos
                .FirstOrDefault(c => string.Equals(
                    c.Rotulo?.TrimEnd('*'), rotulo, StringComparison.OrdinalIgnoreCase)).Valor;

            var textoDoAno = PorRotulo("Ano");
            short? ano = null;
            if (short.TryParse(textoDoAno, out var lido) && lido is >= 1900 and <= 2100)
                ano = lido;
            else if (!string.IsNullOrWhiteSpace(textoDoAno))
                ajustes.Add(new AjusteNaLeitura(
                    "ano", textoDoAno, string.Empty,
                    "O campo de ano do sistema legado é texto livre e o conteúdo não é um ano. Foi omitido."));

            var atualizado = Data(leitor, "AtualizadoEm");
            if (atualizado > (maisRecente ?? DateTime.MinValue)) maisRecente = atualizado;

            maquinas.Add(new EquipamentoNoLegado(
                IdentificadorNoLegado: (long)Decimal(leitor, "SeqPropPessoa")!.Value,
                Categoria: SaneamentoDoLegado.Apararar(Texto(leitor, "Propriedade")) ?? "(sem categoria)",
                Marca: SaneamentoDoLegado.Apararar(Texto(leitor, "Referencia")),
                Modelo: PorRotulo("Modelo") ?? campos[1].Valor,
                Detalhe: PorRotulo("Faixa de Potência") ?? PorRotulo("Tipo") ?? campos[0].Valor,
                Ano: ano,
                EstaAtivo: Texto(leitor, "Ativo") is "S",
                AtualizadoEm: atualizado,
                Ajustes: ajustes));
        }

        return (maquinas, maisRecente);
    }

    /// <summary>
    /// Traduz o código de situação de pessoa do legado, SEM inventar o que não se sabe.
    ///
    /// [V] A coluna é <c>char(1)</c> e o domínio medido tem seis letras mais o branco — P (~75 mil),
    /// A (~39 mil), S, F, O, I. Nem todas têm significado documentado em lugar nenhum. O que a
    /// ponte faz é traduzir as duas que o negócio confirma e devolver a letra crua nas outras,
    /// marcada como não interpretada. Chutar aqui seria inventar informação de negócio numa ponte
    /// de leitura.
    /// </summary>
    private static string DescreverSituacao(string? codigo) => (codigo ?? string.Empty).Trim() switch
    {
        "A" => "A — ativo",
        "P" => "P — prospect",
        "" => "(em branco no sistema legado)",
        var outro => $"{outro} — código sem significado documentado no sistema legado"
    };

    private static string? Texto(SqlDataReader leitor, string coluna)
    {
        var indice = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(indice) ? null : leitor.GetValue(indice)?.ToString();
    }

    private static decimal? Decimal(SqlDataReader leitor, string coluna)
    {
        var indice = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(indice) ? null : Convert.ToDecimal(leitor.GetValue(indice));
    }

    private static DateTime? Data(SqlDataReader leitor, string coluna)
    {
        var indice = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(indice) ? null : leitor.GetDateTime(indice);
    }
}
