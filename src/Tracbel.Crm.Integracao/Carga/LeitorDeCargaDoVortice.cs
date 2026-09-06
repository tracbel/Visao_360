using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Saneamento;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// O LEITOR DA CARGA — a segunda e última classe da solução que pronuncia o vocabulário do
/// sistema legado, e a única que o lê em lote.
///
/// <para><b>Por que não reusar a ponte de leitura:</b> a ponte serve à tela — busca por termo,
/// teto de cem linhas, uma pessoa por vez. A carga lê dezenas de milhares de linhas por um
/// recorte de atividade, junta cadastro, contato e parque, e precisa devolver o que RECUSOU
/// junto do que aceitou. São dois usos com formatos de saída diferentes; forçá-los na mesma
/// assinatura deixaria os dois piores. O que os dois compartilham — a recomposição do documento
/// que o legado partiu em duas colunas numéricas — é uma função só, em
/// <see cref="SaneamentoDoLegado"/>.</para>
///
/// <para><b>Somente leitura, e a garantia é estrutural:</b> não existe aqui um <c>INSERT</c>, um
/// <c>UPDATE</c>, um <c>SELECT INTO</c> nem uma chamada de procedimento. Toda consulta usa
/// <c>WITH (NOLOCK)</c>, e toda consulta de dado transacional tem filtro — de ano, de filial, ou
/// de pertencer ao recorte. O sistema de origem está em produção agora.</para>
///
/// <para><b>A ÚNICA leitura sem filtro é a do catálogo de municípios</b>
/// (<see cref="LerMunicipiosAsync"/>), e é de propósito: catálogo é justamente o que se traz
/// inteiro. São 10.214 linhas numa tabela de referência — quatro ordens de grandeza abaixo do
/// histórico —, e trazer só as cidades que o recorte usa entregaria à tela de cadastro uma lista
/// incompleta, que é como o campo voltaria a ser digitado à mão.</para>
///
/// <para><b>Só objetos VIVOS.</b> O recorte encosta em cadastro de pessoa, contato, agenda,
/// histórico e parque de máquinas — os que <see cref="PonteDeLeituraDoVortice.ObjetosDoLegado"/>
/// declara vivos. As tabelas paradas de frota do ERP, faturamento, ordem de serviço e título
/// financeiro <b>não são lidas</b>, apesar de responderem à consulta e parecerem disponíveis.
/// Foi assim que 17 meses de dado velho passaram por atual.</para>
/// </summary>
public sealed partial class LeitorDeCargaDoVortice(
    IOptions<OpcoesDoVortice> opcoes,
    ILogger<LeitorDeCargaDoVortice> log)
{
    /// <summary>
    /// O nome do sistema de origem, como ele aparece no de-para de identidade e na fila de
    /// descarte. É o mesmo código que <c>integracao.Sistema</c> guarda.
    /// </summary>
    public const string CodigoDoSistema = "VORTICE";

    /// <summary>
    /// As categorias de máquina do parque que continuam sendo mantidas.
    ///
    /// O sistema de origem guarda o parque numa tabela de pares atributo-valor, e o tipo de
    /// máquina é o identificador da propriedade. Estes são os que a medição mostrou com alteração
    /// recente; os demais identificadores da mesma tabela são cadastros aposentados — o mais novo
    /// deles parou em 2018 — e ficam de fora.
    /// </summary>
    private const string CategoriasDeMaquina = "9402,9403,9404,9405,9406,9407,9408,9409,9410,9411,9412";

    /// <summary>
    /// O RECORTE, escrito uma vez e reusado pelas três consultas.
    ///
    /// <para>É o coração da decisão do documento 24: entra a pessoa que teve interação — agenda ou
    /// histórico — no ano escolhido, numa das filiais em operação. A filial dela é aquela em que
    /// mais interagiu; empate desempata pelo menor código, para a carga ser <b>determinística</b>
    /// entre execuções. Sem esse desempate, rodar de novo poderia mover clientes de filial sem
    /// nada ter mudado na origem.</para>
    /// </summary>
    internal const string ConsultaDoRecorte = """
        WITH Atividade AS (
            SELECT h.SeqPessoa, h.NroEmpresa
            FROM IV_Historico h WITH (NOLOCK)
            WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
              AND h.NroEmpresa IN ({FILIAIS})
            UNION ALL
            SELECT a.SeqPessoa, a.NroEmpresa
            FROM IV_Agenda a WITH (NOLOCK)
            WHERE a.DtaAgenda >= @inicio AND a.DtaAgenda < @fim
              AND a.NroEmpresa IN ({FILIAIS})
        ),
        Classificada AS (
            SELECT SeqPessoa, NroEmpresa,
                   ROW_NUMBER() OVER (
                       PARTITION BY SeqPessoa ORDER BY COUNT(*) DESC, NroEmpresa) AS Ordem
            FROM Atividade
            GROUP BY SeqPessoa, NroEmpresa
        ),
        Recorte AS (SELECT SeqPessoa, NroEmpresa FROM Classificada WHERE Ordem = 1)
        """;

    /// <summary>
    /// Lê os clientes do recorte, já saneados, com o endereço principal junto.
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<ClienteParaCarga>>> LerClientesAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "GE_Pessoa",
            recorte,
            $"""
             {ConsultaDoRecorte}
             SELECT {(recorte.Limite is null ? string.Empty : "TOP (@limite)")}
                    p.SeqPessoa, r.NroEmpresa,
                    p.NomeRazao, p.Fantasia, p.FisicaJuridica, p.Status,
                    p.NroCGCCPF, p.DigCGCCPF, p.InscricaoRG, p.CNAE,
                    p.Email, p.FoneDDD1, p.FoneNro1,
                    p.TipoLogradouro, p.Logradouro, p.NroLogradouro, p.CmpltoLogradouro,
                    p.Bairro, p.Cidade, p.SeqCidade, p.Uf, p.Cep, p.Latitude, p.Longitude,
                    COALESCE(p.DtaAlteracao, p.DtaInclusao) AS AtualizadoEm
             FROM Recorte r
             JOIN GE_Pessoa p WITH (NOLOCK) ON p.SeqPessoa = r.SeqPessoa
             ORDER BY p.SeqPessoa
             """,
            LerCliente,
            ct);

    /// <summary>
    /// Lê os contatos das pessoas do recorte, já saneados.
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<ContatoParaCarga>>> LerContatosAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "GE_Contato",
            recorte,
            $"""
             {ConsultaDoRecorte}
             SELECT c.SeqPessoa, c.SeqContato, c.Contato, c.Cpf, c.DigCPF,
                    c.TipoContato, c.AreaAtuacao, c.EMail, c.FoneDDD1, c.FoneNro1,
                    COALESCE(c.DtaAlteracao, c.DTAINCLUSAO) AS AtualizadoEm
             FROM Recorte r
             JOIN GE_Contato c WITH (NOLOCK) ON c.SeqPessoa = r.SeqPessoa
             ORDER BY c.SeqPessoa, c.SeqContato
             """,
            LerContato,
            ct);

    /// <summary>
    /// Lê o parque de máquinas das pessoas do recorte, já saneado e com o chassi validado.
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<EquipamentoParaCarga>>> LerEquipamentosAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_ClientePropr",
            recorte,
            $"""
             {ConsultaDoRecorte}
             SELECT cp.SeqPropPessoa, cp.SeqPessoa, pr.Propriedade, cp.Referencia,
                    cp.Identificador, cp.Ativo, cp.Notas,
                    cp.Campo1, cp.Campo2, cp.Campo3, cp.Campo4, cp.Campo5, cp.Campo6,
                    pr.Campo1 AS Rotulo1, pr.Campo2 AS Rotulo2, pr.Campo3 AS Rotulo3,
                    pr.Campo4 AS Rotulo4, pr.Campo5 AS Rotulo5, pr.Campo6 AS Rotulo6,
                    COALESCE(cp.DtaAlteracao, cp.DtaInclusao) AS AtualizadoEm
             FROM Recorte r
             JOIN IV_ClientePropr cp WITH (NOLOCK) ON cp.SeqPessoa = r.SeqPessoa
             JOIN IV_Propriedade pr WITH (NOLOCK) ON pr.SeqPropriedade = cp.SeqPropriedade
             WHERE cp.SeqPropriedade IN ({CategoriasDeMaquina})
             ORDER BY cp.SeqPropPessoa
             """,
            LerEquipamento,
            ct);

    /// <summary>
    /// Conta o recorte sem trazer nenhuma linha — o "meça antes de carregar" do documento 24.
    /// </summary>
    /// <param name="recorte">O que contar.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>Quantos clientes por código de filial da origem.</returns>
    public async Task<Resultado<IReadOnlyDictionary<int, int>>> ContarPorFilialAsync(
        RecorteDaCarga recorte, CancellationToken ct)
    {
        var config = opcoes.Value;

        if (string.IsNullOrWhiteSpace(config.Conexao))
            return Resultado<IReadOnlyDictionary<int, int>>.Indisponivel(NaoConfigurada);

        try
        {
            await using var conexao = new SqlConnection(config.Conexao);
            await conexao.OpenAsync(ct);

            await using var comando = Montar(
                conexao,
                config,
                recorte,
                $"""
                 {ConsultaDoRecorte}
                 SELECT NroEmpresa, COUNT(*) AS Pessoas
                 FROM Recorte GROUP BY NroEmpresa ORDER BY NroEmpresa
                 """);

            var contagem = new Dictionary<int, int>();

            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct))
                contagem[(int)Numero(leitor, "NroEmpresa")!.Value] = leitor.GetInt32(leitor.GetOrdinal("Pessoas"));

            return Resultado<IReadOnlyDictionary<int, int>>.Ok(contagem);
        }
        catch (Exception erro) when (erro is SqlException or InvalidOperationException or TimeoutException)
        {
            log.LogWarning(erro, "A contagem do recorte no sistema legado falhou.");
            return Resultado<IReadOnlyDictionary<int, int>>.Indisponivel(NaoRespondeu("a contagem do recorte", 0));
        }
    }

    // =============================================================================================
    // O encanamento — aberto uma vez, com o mesmo tratamento de falha para todas as consultas.
    // =============================================================================================

    private const string NaoConfigurada =
        "A leitura do sistema legado não está configurada nesta instalação. Defina a variável de " +
        "ambiente Vortice__Conexao com a cadeia de conexão da conta de leitura. A carga não roda " +
        "sem ela, e nada foi gravado.";

    private static string NaoRespondeu(string objeto, int lidas) =>
        $"O sistema legado parou de responder durante a leitura de {objeto}, depois de {lidas} " +
        "linha(s). Quase sempre é a VPN: confira se ela está conectada e rode de novo. A carga é " +
        "idempotente — o que já entrou não duplica.";

    /// <summary>
    /// Monta o comando com a data do recorte e a lista de filiais em PARÂMETROS.
    ///
    /// <para>A lista de filiais entra como um parâmetro por filial — <c>@filial0</c>,
    /// <c>@filial1</c>… — e o que é concatenado no texto da consulta são os NOMES que este método
    /// gera, nunca os valores. Concatenar número parece inofensivo, e é exatamente assim que a
    /// primeira concatenação entra num arquivo e a segunda, com texto de usuário, entra sem
    /// ninguém notar. Aqui não existe a primeira.</para>
    /// </summary>
    private static SqlCommand Montar(
        SqlConnection conexao, OpcoesDoVortice config, RecorteDaCarga recorte, string sql)
    {
        var comando = conexao.CreateCommand();

        // GENEROSO DE PROPÓSITO, ao contrário do tempo limite da ponte: a ponte serve uma tela e
        // desistir rápido é a coisa certa lá; aqui uma varredura de recorte anual sobre milhões
        // de linhas de agenda e histórico leva minutos, e desistir no meio deixaria a carga sem
        // nunca terminar.
        comando.CommandTimeout = config.TempoLimiteSegundos * 40;

        var nomes = new List<string>(recorte.CodigosDeFilialNoLegado.Count);

        for (var i = 0; i < recorte.CodigosDeFilialNoLegado.Count; i++)
        {
            var nome = $"@filial{i.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            nomes.Add(nome);
            comando.Parameters.Add(nome, SqlDbType.Decimal).Value = recorte.CodigosDeFilialNoLegado[i];
        }

        comando.CommandText = sql.Replace("{FILIAIS}", string.Join(", ", nomes), StringComparison.Ordinal);

        comando.Parameters.Add("@inicio", SqlDbType.DateTime).Value = new DateTime(recorte.Ano, 1, 1);
        comando.Parameters.Add("@fim", SqlDbType.DateTime).Value = new DateTime(recorte.Ano + 1, 1, 1);

        if (recorte.Limite is { } limite)
            comando.Parameters.Add("@limite", SqlDbType.Int).Value = limite;

        return comando;
    }

    private async Task<Resultado<LoteDaCarga<T>>> LerAsync<T>(
        string objeto,
        RecorteDaCarga recorte,
        string sql,
        Func<SqlDataReader, string, (T? Aceito, LinhaRecusada? Recusada)> traduzir,
        CancellationToken ct)
        where T : class
    {
        var config = opcoes.Value;

        if (string.IsNullOrWhiteSpace(config.Conexao))
            return Resultado<LoteDaCarga<T>>.Indisponivel(NaoConfigurada);

        var aceitos = new List<T>();
        var recusadas = new List<LinhaRecusada>();
        var lidas = 0;
        DateTime? maisRecente = null;

        try
        {
            await using var conexao = new SqlConnection(config.Conexao);
            await conexao.OpenAsync(ct);

            await using var comando = Montar(conexao, config, recorte, sql);
            await using var leitor = await comando.ExecuteReaderAsync(ct);

            while (await leitor.ReadAsync(ct))
            {
                lidas++;

                var atualizado = Data(leitor, "AtualizadoEm", opcional: true);
                if (atualizado > (maisRecente ?? DateTime.MinValue)) maisRecente = atualizado;

                var (aceito, recusada) = traduzir(leitor, objeto);

                if (aceito is not null) aceitos.Add(aceito);
                if (recusada is not null) recusadas.Add(recusada);
            }

            return Resultado<LoteDaCarga<T>>.Ok(
                new LoteDaCarga<T>(aceitos, recusadas, lidas, maisRecente));
        }
        catch (Exception erro) when (erro is SqlException or InvalidOperationException or TimeoutException)
        {
            // O DETALHE TÉCNICO VAI PARA O LOG. Quem chamou recebe o que fazer — e a carga é
            // desenhada para poder recomeçar do zero depois, sem duplicar nada.
            log.LogWarning(erro, "A leitura do sistema legado falhou ao consultar {Objeto}.", objeto);

            return Resultado<LoteDaCarga<T>>.Indisponivel(NaoRespondeu(objeto, lidas));
        }
    }

    // =============================================================================================
    // A tradução — daqui para dentro é vocabulário do legado; daqui para fora, o nosso.
    // =============================================================================================

    private static (ClienteParaCarga?, LinhaRecusada?) LerCliente(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqPessoa");
        var tipoNaOrigem = (Texto(leitor, "FisicaJuridica") ?? string.Empty).Trim().ToUpperInvariant();

        if (tipoNaOrigem is not ("F" or "J"))
            return (null, Recusar(
                nameof(Cliente), "Tipo de pessoa fora de F/J", chave,
                $"O tipo de pessoa na origem é '{tipoNaOrigem}', que não é nem física nem " +
                "jurídica. Sem saber se o documento é CPF ou CNPJ, não há como validá-lo — a " +
                "linha precisa de revisão manual antes de migrar.",
                leitor));

        var ehFisica = tipoNaOrigem is "F";

        var nome = SaneamentoDaCarga.Texto("nomeRazao", Texto(leitor, "NomeRazao"), 200, correcoes, rejeicoes);
        if (nome is null)
            return (null, Recusar(
                nameof(Cliente), "Razão social vazia ou maior que a coluna", chave,
                "A razão social não sobrevive ao saneamento: está vazia ou passa dos 200 " +
                "caracteres da coluna. Cliente sem razão social não existe.",
                leitor));

        var temDocumentoNaOrigem = Numero(leitor, "NroCGCCPF") is { } bruto && bruto > 0;
        var rejeicoesAntesDoDocumento = rejeicoes.Count;

        var documento = SaneamentoDaCarga.Documento(
            Numero(leitor, "NroCGCCPF"), Numero(leitor, "DigCGCCPF"), ehFisica, correcoes, rejeicoes);

        // A REGRA DO DOCUMENTO 16, SEÇÃO 8.2, LITERAL: recusa migrar quando o número não permite
        // reconstituir um CPF/CNPJ com dígito verificador válido mesmo após restaurar os zeros.
        // Não ter documento na origem é outra coisa — aí o campo fica vazio e o cliente entra.
        if (temDocumentoNaOrigem && documento is null)
            return (null, Recusar(
                nameof(Cliente), "Documento não reconstituível", chave,
                rejeicoes.Count > rejeicoesAntesDoDocumento
                    ? rejeicoes[rejeicoesAntesDoDocumento].Motivo
                    : "O documento da origem não se reconstitui num CPF/CNPJ válido.",
                leitor));

        var cliente = new ClienteParaCarga(
            ChaveDeOrigem: chave,
            CodigoDaFilialNoLegado: (int)Numero(leitor, "NroEmpresa")!.Value,
            NomeRazao: nome,
            NomeFantasia: SaneamentoDaCarga.Texto("nomeFantasia", Texto(leitor, "Fantasia"), 200, correcoes, rejeicoes),
            TipoDePessoa: ehFisica ? TipoDePessoa.Fisica : TipoDePessoa.Juridica,
            Documento: documento,
            Situacao: SaneamentoDaCarga.Situacao(Texto(leitor, "Status"), correcoes),
            Email: SaneamentoDaCarga.Email(Texto(leitor, "Email"), correcoes, rejeicoes),
            Telefone: SaneamentoDaCarga.Telefone(
                Texto(leitor, "FoneDDD1"), Numero(leitor, "FoneNro1"), correcoes, rejeicoes),
            InscricaoEstadual: SaneamentoDaCarga.Texto(
                "inscricaoEstadual", Texto(leitor, "InscricaoRG"), 20, correcoes, rejeicoes),
            AtividadeEconomica: SaneamentoDaCarga.Texto(
                "atividadeEconomica", SoDigitos(Texto(leitor, "CNAE")), 7, correcoes, rejeicoes),
            Endereco: LerEndereco(leitor, correcoes, rejeicoes),
            AtualizadoEm: Data(leitor, "AtualizadoEm", opcional: true),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes);

        return (cliente, null);
    }

    private static EnderecoParaCarga? LerEndereco(
        SqlDataReader leitor, List<CorrecaoAplicada> correcoes, List<CampoRejeitado> rejeicoes)
    {
        var tipo = SaneamentoDaCarga.Texto("tipoLogradouro", Texto(leitor, "TipoLogradouro"), 40, correcoes, rejeicoes);
        var via = SaneamentoDaCarga.Texto("logradouro", Texto(leitor, "Logradouro"), 160, correcoes, rejeicoes);
        var municipio = SaneamentoDaCarga.Texto("municipio", Texto(leitor, "Cidade"), 120, correcoes, rejeicoes);
        var uf = SaneamentoDaCarga.Uf(Texto(leitor, "Uf"), correcoes, rejeicoes);

        // ENDEREÇO PELA METADE NÃO ENTRA. Os três campos abaixo são os que a entidade exige, e
        // ocupar a única vaga de endereço principal do cliente com um logradouro sem município
        // seria pior do que não ter endereço: a tela mostraria um endereço que não localiza.
        if (via is null || municipio is null || uf is null)
        {
            if (via is not null || municipio is not null)
                rejeicoes.Add(new CampoRejeitado(
                    "endereco", $"{via} / {municipio} / {uf}",
                    "O endereço da origem não tem os três campos que localizam alguém — " +
                    "logradouro, município e UF. Entrou sem endereço, em vez de entrar pela metade."));

            return null;
        }

        var (latitude, longitude) = SaneamentoDaCarga.Coordenada(
            Numero(leitor, "Latitude"), Numero(leitor, "Longitude"), rejeicoes);

        // O legado separa o tipo do logradouro ("RUA", "AVENIDA") da via em duas colunas; a nossa
        // coluna é uma só. Juntar é composição, não invenção.
        var logradouro = tipo is null ? via : $"{tipo} {via}";

        // O PONTEIRO DE CIDADE DA ORIGEM, que é o que liga o endereço ao catálogo.
        //
        // [V] O cadastro de pessoa guarda as DUAS coisas: SeqCidade, que aponta para GE_Cidade,
        // e Cidade, texto livre ao lado. Os dois divergem em 217 linhas — o texto é o que
        // envelheceu. A carga usa o ponteiro, que está preenchido em 114.686 dos 119.359
        // cadastros, e guarda o texto só para o caso de o ponteiro não resolver.
        var chaveDaCidade = Numero(leitor, "SeqCidade") is { } cidade && cidade > 0
            ? ((long)cidade).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : null;

        return new EnderecoParaCarga(
            Logradouro: logradouro,
            Numero: SaneamentoDaCarga.Texto("numero", Texto(leitor, "NroLogradouro"), 20, correcoes, rejeicoes),
            Complemento: SaneamentoDaCarga.Texto(
                "complemento", Texto(leitor, "CmpltoLogradouro"), 120, correcoes, rejeicoes),
            Bairro: SaneamentoDaCarga.Texto("bairro", Texto(leitor, "Bairro"), 120, correcoes, rejeicoes),
            Municipio: municipio,
            ChaveDoMunicipioDeOrigem: chaveDaCidade,
            Uf: uf,
            Cep: SaneamentoDaCarga.CepDe(Texto(leitor, "Cep"), correcoes, rejeicoes),
            Latitude: latitude,
            Longitude: longitude);
    }

    private static (ContatoParaCarga?, LinhaRecusada?) LerContato(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chaveDoCliente = Chave(leitor, "SeqPessoa");
        var chave = $"{chaveDoCliente}/{Chave(leitor, "SeqContato")}";

        var nomeCompleto = SaneamentoDaCarga.Texto("nome", Texto(leitor, "Contato"), 240, correcoes, rejeicoes);
        if (nomeCompleto is null)
            return (null, Recusar(
                nameof(Contato), "Nome do contato vazio", chave,
                "O nome do contato não sobrevive ao saneamento. Contato sem nome não existe.",
                leitor));

        // O legado guarda o nome inteiro numa coluna só. A nossa separa nome de sobrenome, e a
        // divisão é no primeiro espaço: é o único corte que não inventa nada.
        var espaco = nomeCompleto.IndexOf(' ', StringComparison.Ordinal);
        var nome = espaco < 0 ? nomeCompleto : nomeCompleto[..espaco];
        var sobrenome = espaco < 0 ? null : nomeCompleto[(espaco + 1)..];

        if (nome.Length > 120)
            return (null, Recusar(
                nameof(Contato), "Primeiro nome maior que a coluna", chave,
                $"O primeiro nome tem {nome.Length} caracteres e a coluna aceita 120. Foi " +
                "recusado em vez de cortado.",
                leitor));

        var contato = new ContatoParaCarga(
            ChaveDeOrigem: chave,
            ChaveDoClienteDeOrigem: chaveDoCliente,
            Nome: nome,
            Sobrenome: sobrenome is { Length: > 120 } ? null : sobrenome,
            Documento: SaneamentoDaCarga.Documento(
                Numero(leitor, "Cpf"), Numero(leitor, "DigCPF"), ehPessoaFisica: true, correcoes, rejeicoes),
            Cargo: SaneamentoDaCarga.Texto("cargo", Texto(leitor, "TipoContato"), 80, correcoes, rejeicoes)
                   ?? SaneamentoDaCarga.Texto("cargo", Texto(leitor, "AreaAtuacao"), 80, correcoes, rejeicoes),
            Email: SaneamentoDaCarga.Email(Texto(leitor, "EMail"), correcoes, rejeicoes),
            Telefone: SaneamentoDaCarga.Telefone(
                Texto(leitor, "FoneDDD1"), Numero(leitor, "FoneNro1"), correcoes, rejeicoes),
            AtualizadoEm: Data(leitor, "AtualizadoEm", opcional: true),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes);

        return (contato, null);
    }

    private static (EquipamentoParaCarga?, LinhaRecusada?) LerEquipamento(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqPropPessoa");
        var identificador = Texto(leitor, "Identificador");

        // SEM CHASSI NÃO HÁ MÁQUINA. O chassi é a identidade e a chave de deduplicação; a coluna
        // da origem é texto livre e guarda de tudo — inclusive nome de operador. Recusar é a
        // única saída honesta: chassi inventado casaria venda, garantia e ordem de serviço da
        // máquina errada.
        if (!Dominio.Comum.Chassi.TentarCriar(identificador, out var chassi))
            return (null, Recusar(
                nameof(Equipamento),
                string.IsNullOrWhiteSpace(identificador)
                    ? "Máquina sem chassi na origem"
                    : "Chassi fora do padrão VIN de 17 posições",
                chave,
                string.IsNullOrWhiteSpace(identificador)
                    ? "A máquina não tem chassi na origem. O chassi é a identidade da máquina e a " +
                      "chave de deduplicação: sem ele não há como afirmar que esta máquina não é " +
                      "outra já cadastrada."
                    : $"O chassi '{identificador.Trim()}' não tem as 17 posições do padrão VIN, ou " +
                      "usa I, O ou Q, que o padrão proíbe. Não foi corrigido nem completado: " +
                      "chassi adivinhado ligaria histórico à máquina errada.",
                leitor));

        // Os rótulos de Campo1..Campo6 mudam por categoria — "Modelo" está no Campo2 do trator e
        // no Campo1 da plataforma. Por isso os rótulos vêm JUNTO, da tabela de definição, e o
        // casamento é por nome do rótulo, nunca por posição.
        var campos = Enumerable.Range(1, 6)
            .Select(i => (
                Rotulo: (Texto(leitor, $"Rotulo{i}") ?? string.Empty).Trim().TrimEnd('*'),
                Valor: Texto(leitor, $"Campo{i}")))
            .ToList();

        string? PorRotulo(string rotulo) => campos
            .FirstOrDefault(c => string.Equals(c.Rotulo, rotulo, StringComparison.OrdinalIgnoreCase)).Valor;

        var categoria = SaneamentoDaCarga.Texto("categoria", Texto(leitor, "Propriedade"), 80, correcoes, rejeicoes);
        var marca = SaneamentoDaCarga.Texto("marca", Texto(leitor, "Referencia"), 80, correcoes, rejeicoes);
        var modelo = SaneamentoDaCarga.Texto("modelo", PorRotulo("Modelo"), 120, correcoes, rejeicoes);

        if (categoria is null || marca is null)
            return (null, Recusar(
                nameof(Equipamento), "Máquina sem categoria ou sem marca", chave,
                "A máquina não declara categoria nem marca na origem, e as duas juntas são o que " +
                "define a família do catálogo. Sem elas a máquina entraria sem modelo — e modelo " +
                "é chave estrangeira, não texto.",
                leitor));

        var equipamento = new EquipamentoParaCarga(
            ChaveDeOrigem: chave,
            ChaveDoClienteDeOrigem: Chave(leitor, "SeqPessoa"),
            Chassi: chassi,
            Marca: marca,
            Categoria: categoria,
            // Modelo ausente vira o nome da própria categoria: não é invenção, é o rótulo que a
            // origem já dá à linha. O catálogo fica honesto sobre o que se sabe.
            Modelo: modelo ?? categoria,
            Ano: SaneamentoDaCarga.Ano("ano", PorRotulo("Ano"), rejeicoes),
            EstaAtivo: (Texto(leitor, "Ativo") ?? string.Empty).Trim().ToUpperInvariant() is "S",
            LocalizacaoDescrita: SaneamentoDaCarga.Texto(
                "localizacao", Texto(leitor, "Notas"), 200, correcoes, rejeicoes),
            AtualizadoEm: Data(leitor, "AtualizadoEm", opcional: true),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes);

        return (equipamento, null);
    }

    // =============================================================================================
    // Município — o catálogo nacional, e o território de cada carteira.
    // =============================================================================================

    /// <summary>
    /// Lê o CATÁLOGO DE MUNICÍPIOS da origem (<c>GE_Cidade</c>).
    ///
    /// <para><b>Sem filtro, e é a única consulta deste arquivo assim</b> — a razão está na
    /// documentação da classe: catálogo se traz inteiro, senão a tela de cadastro recebe uma
    /// lista incompleta e o campo volta a ser digitado à mão. São 10.214 linhas.</para>
    ///
    /// <para><b>A UF é o que recusa a linha.</b> [V] Ela é <c>varchar(2)</c> de texto livre e
    /// guarda hoje <c>**</c>, <c>EX</c>, <c>MI</c>, <c>PÁ</c> e o vazio em 229 linhas. Nenhuma
    /// delas é referenciada por vínculo de carteira, e 27 cadastros de pessoa apontam para uma —
    /// esses endereços ficam com o município não identificado, contados no documento 26.</para>
    ///
    /// <para><b>Não há código do IBGE na origem.</b> A tabela não tem essa coluna. A carga
    /// identifica o município por nome mais UF; o risco está no documento 26, seção 4.</para>
    /// </summary>
    /// <param name="recorte">O que trazer. Aqui só serve ao encanamento comum.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<MunicipioParaCarga>>> LerMunicipiosAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "GE_Cidade",
            recorte,
            """
            SELECT c.SeqCidade, c.Cidade, c.Uf, c.DtaAlteracao AS AtualizadoEm
            FROM GE_Cidade c WITH (NOLOCK)
            ORDER BY c.Uf, c.Cidade, c.SeqCidade
            """,
            LerMunicipio,
            ct);

    private static (MunicipioParaCarga?, LinhaRecusada?) LerMunicipio(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqCidade");
        var nome = SaneamentoDaCarga.Texto("municipio", Texto(leitor, "Cidade"), 120, correcoes, rejeicoes);
        var uf = SaneamentoDaCarga.Uf(Texto(leitor, "Uf"), correcoes, rejeicoes);

        if (nome is null)
            return (null, Recusar(
                "Municipio", "Município sem nome legível", chave,
                "O nome do município não sobrevive à normalização. Município sem nome não " +
                "identifica lugar nenhum.",
                leitor));

        // UF INVÁLIDA RECUSA A LINHA INTEIRA, e não só o campo. Um município sem estado não é
        // ambíguo — é impossível: existem 15 "Bom Jesus" no Brasil, e sem a UF nenhum deles é
        // este. Entrar com UF nula tornaria o índice único de nome+UF inútil, que é justamente
        // a defesa que este catálogo existe para dar.
        if (uf is null)
            return (null, Recusar(
                "Municipio", "UF fora das 27 unidades federativas", chave,
                $"A UF '{(Texto(leitor, "Uf") ?? string.Empty).Trim()}' do município " +
                $"'{nome}' não é uma das 27 unidades federativas. Município sem estado não " +
                "identifica lugar: entrou na fila de descarte em vez de entrar sem UF.",
                leitor));

        return (new MunicipioParaCarga(
            ChaveDeOrigem: chave,
            Nome: nome,
            Uf: uf,
            AtualizadoEm: Data(leitor, "AtualizadoEm", opcional: true),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    /// <summary>
    /// Lê O TERRITÓRIO DE CADA CARTEIRA — o par carteira × cidade de <c>IVS_CartCid</c>.
    ///
    /// <para><b>É o agrupamento real, e ele desmente a "regional".</b> <c>IVS_Regional</c> existe
    /// e tem ZERO linhas; esta tabela tem 673 linhas cobrindo 91 carteiras. A filial vem da
    /// carteira (<c>IVS_Carteira.NroEmpresa</c>), que é o filtro desta consulta — só entram as
    /// carteiras das filiais em operação.</para>
    ///
    /// <para>Nenhuma das carteiras das filiais 4, 5 e 10 (Guaíra, Ituverava e Monte Alto — as
    /// três fora da lista oficial) tem cidade cadastrada, o que confirma que elas não operam.
    /// </para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<MunicipioDeCarteiraParaCarga>>> LerMunicipiosDeCarteiraAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IVS_CartCid",
            recorte,
            """
            SELECT cc.SeqCarteira, cc.SeqCidade, cc.DtaAlteracao AS AtualizadoEm
            FROM IVS_CartCid cc WITH (NOLOCK)
            JOIN IVS_Carteira c WITH (NOLOCK) ON c.SeqCarteira = cc.SeqCarteira
            WHERE c.NroEmpresa IN ({FILIAIS})
            ORDER BY cc.SeqCarteira, cc.SeqCidade
            """,
            LerMunicipioDeCarteira,
            ct);

    private static (MunicipioDeCarteiraParaCarga?, LinhaRecusada?) LerMunicipioDeCarteira(
        SqlDataReader leitor, string objeto)
    {
        var carteira = Chave(leitor, "SeqCarteira");
        var cidade = Chave(leitor, "SeqCidade");

        return (new MunicipioDeCarteiraParaCarga(
            ChaveDeOrigem: $"{carteira}/{cidade}",
            ChaveDaCarteiraDeOrigem: carteira,
            ChaveDoMunicipioDeOrigem: cidade,
            AtualizadoEm: Data(leitor, "AtualizadoEm", opcional: true)), null);
    }

    // =============================================================================================
    // Leitura crua de coluna, e o registro da recusa.
    // =============================================================================================

    /// <summary>
    /// Monta a linha da fila de descarte com o conteúdo CRU em JSON.
    ///
    /// Guardar o motivo sem guardar a linha faria a recusa incontestável e inútil: ninguém
    /// conseguiria conferir se a regra estava certa. O JSON é o que permite reprocessar depois.
    /// </summary>
    private static LinhaRecusada Recusar(
        string entidade, string categoria, string chave, string motivo, SqlDataReader leitor)
    {
        var conteudo = new Dictionary<string, object?>(StringComparer.Ordinal);

        for (var i = 0; i < leitor.FieldCount; i++)
            conteudo[leitor.GetName(i)] = leitor.IsDBNull(i) ? null : leitor.GetValue(i)?.ToString()?.Trim();

        return new LinhaRecusada(entidade, categoria, chave, motivo, JsonSerializer.Serialize(conteudo));
    }

    private static string Chave(SqlDataReader leitor, string coluna) =>
        Numero(leitor, coluna) is { } valor
            ? ((long)valor).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;

    private static string? Texto(SqlDataReader leitor, string coluna)
    {
        var indice = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(indice) ? null : leitor.GetValue(indice)?.ToString();
    }

    private static decimal? Numero(SqlDataReader leitor, string coluna)
    {
        var indice = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(indice) ? null : Convert.ToDecimal(leitor.GetValue(indice));
    }

    private static DateTime? Data(SqlDataReader leitor, string coluna, bool opcional)
    {
        if (opcional && !TemColuna(leitor, coluna)) return null;

        var indice = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(indice) ? null : leitor.GetDateTime(indice);
    }

    private static bool TemColuna(SqlDataReader leitor, string coluna)
    {
        for (var i = 0; i < leitor.FieldCount; i++)
            if (string.Equals(leitor.GetName(i), coluna, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    private static string? SoDigitos(string? bruto) =>
        bruto is null ? null : new string(bruto.Where(char.IsAsciiDigit).ToArray());
}
