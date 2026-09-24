using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>Em que estado o documento do cliente chega do Vórtice.</summary>
public enum SituacaoDoDocumentoNoVortice
{
    /// <summary>CPF ou CNPJ recomposto e conferido no dígito verificador — o único que casa com o CRM.</summary>
    Valido = 0,

    /// <summary><c>NroCGCCPF</c> nulo: a origem não tem documento nenhum.</summary>
    SemDocumento = 1,

    /// <summary><c>NroCGCCPF</c> igual a zero: a coluna foi preenchida, mas com nada.</summary>
    Zerado = 2,

    /// <summary>Tem número, mas ele não se recompõe num CPF/CNPJ com dígito verificador válido.</summary>
    Invalido = 3
}

/// <summary>
/// O DOCUMENTO DO CLIENTE DO VÓRTICE, recomposto para casar com o cadastro do CRM.
///
/// <para><b>O defeito de origem:</b> <c>GE_Pessoa</c> guarda o documento em duas colunas NUMÉRICAS — a base em
/// <c>NroCGCCPF</c> e os dígitos verificadores em <c>DigCGCCPF</c> —, e número não preserva zero à esquerda. A
/// recomposição devolve os zeros ao lugar (pessoa jurídica: 12 + 2 = 14 dígitos; física: 9 + 2 = 11) e confere o
/// dígito verificador — é <see cref="SaneamentoDoLegado.RecomporDocumento"/>, o mesmo e único lugar do projeto que
/// sabe fazer isso.</para>
///
/// <para><b>O tipo de pessoa sai do documento</b>, como na carga da SA1: <c>FisicaJuridica</c> é um campo que
/// alguém preencheu. Se o documento não confere pelo tipo declarado e confere pelo outro, vale o documento — o
/// dígito verificador é a prova, e a discordância vira número. Se confere pelos dois, ninguém escolhe: é
/// inválido.</para>
///
/// <para>Comparado sempre só com dígitos, dos dois lados: o CRM guarda <c>CpfCnpj.Numero</c> sem máscara.</para>
/// </summary>
/// <param name="Situacao">Válido, sem documento, zerado ou inválido.</param>
/// <param name="Numero">O CPF ou CNPJ só com dígitos, quando válido.</param>
/// <param name="TipoDeclaradoDiscorda">Se <c>FisicaJuridica</c> dizia o contrário do documento.</param>
/// <param name="ZerosDevolvidos">Se a recomposição precisou devolver zeros à esquerda.</param>
public sealed record DocumentoDoVortice(
    SituacaoDoDocumentoNoVortice Situacao, string? Numero, bool TipoDeclaradoDiscorda, bool ZerosDevolvidos)
{
    /// <summary>Recompõe e confere o documento a partir das duas colunas da origem.</summary>
    /// <param name="nroCgcCpf">O que está em <c>GE_Pessoa.NroCGCCPF</c>.</param>
    /// <param name="digCgcCpf">O que está em <c>GE_Pessoa.DigCGCCPF</c>.</param>
    /// <param name="fisicaJuridica">O que está em <c>GE_Pessoa.FisicaJuridica</c> (<c>F</c>, <c>J</c> ou outra coisa).</param>
    public static DocumentoDoVortice Recompor(decimal? nroCgcCpf, decimal? digCgcCpf, string? fisicaJuridica)
    {
        if (nroCgcCpf is null) return new(SituacaoDoDocumentoNoVortice.SemDocumento, null, false, false);
        if (nroCgcCpf <= 0) return new(SituacaoDoDocumentoNoVortice.Zerado, null, false, false);

        var declarado = (fisicaJuridica ?? string.Empty).Trim().ToUpperInvariant();
        var comoFisica = Tentar(nroCgcCpf, digCgcCpf, ehPessoaFisica: true);
        var comoJuridica = Tentar(nroCgcCpf, digCgcCpf, ehPessoaFisica: false);

        var (escolhido, discorda) = declarado switch
        {
            "F" => comoFisica is not null ? (comoFisica, false) : (comoJuridica, comoJuridica is not null),
            "J" => comoJuridica is not null ? (comoJuridica, false) : (comoFisica, comoFisica is not null),

            // SEM TIPO DECLARADO, o documento decide sozinho — mas só quando decide: se confere pelos dois
            // tamanhos, escolher um seria adivinhar.
            _ => comoFisica is not null && comoJuridica is not null ? (null, false) : (comoFisica ?? comoJuridica, false)
        };

        if (escolhido is null) return new(SituacaoDoDocumentoNoVortice.Invalido, null, false, false);

        var original = Inteiro(nroCgcCpf.Value) + Inteiro(digCgcCpf ?? 0);
        return new(SituacaoDoDocumentoNoVortice.Valido, escolhido, discorda, escolhido.Length != original.Length);
    }

    private static string? Tentar(decimal? nro, decimal? dig, bool ehPessoaFisica)
    {
        var (recomposto, confere) = SaneamentoDoLegado.RecomporDocumento(nro, dig, ehPessoaFisica, new List<AjusteNaLeitura>());
        return confere ? recomposto : null;
    }

    private static string Inteiro(decimal valor) => ((long)valor).ToString(CultureInfo.InvariantCulture);
}

/// <summary>O departamento do Vórtice, com a cadência de contato que ele declara.</summary>
/// <param name="SeqDepto">O identificador na origem.</param>
/// <param name="Depto">A sigla (<c>MAQ-NOVOS</c>, com hífen).</param>
/// <param name="Descricao">O nome legível.</param>
/// <param name="CicloA">Dias de ciclo da classe A.</param>
/// <param name="CicloB">Dias de ciclo da classe B.</param>
/// <param name="CicloC">Dias de ciclo da classe C.</param>
/// <param name="CicloD">Dias de ciclo da classe D.</param>
public sealed record DepartamentoNoVortice(
    int SeqDepto, string Depto, string? Descricao, short? CicloA, short? CicloB, short? CicloC, short? CicloD);

/// <summary>
/// Uma carteira do Vórtice que tem o departamento — pelo vínculo de departamento (<c>IVS_CartDepto</c>) ou por
/// ter cliente nele (<c>IVS_Pes</c>).
/// </summary>
/// <param name="SeqCarteira">O identificador na origem.</param>
/// <param name="NroEmpresa">A filial na origem — o <c>NN</c> do código <c>0101NN</c> do CRM.</param>
/// <param name="Codigo">O código da carteira (<c>MAQ_13SJRP_01</c>).</param>
/// <param name="Descricao">O nome legível.</param>
/// <param name="SeqUsuarioResponsavel">O usuário responsável (<c>SeqUsrResp</c>).</param>
/// <param name="SeqVendedor">O vendedor (<c>SeqVendedor</c>), quando há.</param>
/// <param name="NomeDoVendedor">O nome do vendedor: <c>VENDEDOR</c>, senão <c>CODVENDEDOR</c>, senão nulo.</param>
/// <param name="SeqUsuarioDoVendedor">O usuário do vendedor (<c>IV_VENDEDOR.SeqUsuario</c>).</param>
/// <param name="VendedorDesligado">Se <c>TRIM(VENDEDOR) = 'DESLIGADO'</c> — a carteira que o BI exclui.</param>
public sealed record CarteiraNoVortice(
    int SeqCarteira, int NroEmpresa, string Codigo, string? Descricao, long? SeqUsuarioResponsavel,
    long? SeqVendedor, string? NomeDoVendedor, long? SeqUsuarioDoVendedor, bool VendedorDesligado)
{
    /// <summary>
    /// O DONO DA CARTEIRA, pela regra do BI (extrator "Uteis CRM", decisão de 24/09/2026): o VENDEDOR — o usuário
    /// dele no Vórtice. Carteira sem vendedor (a do INT.MERCADO e parte das digitais) fica com o usuário
    /// responsável; vendedor sem usuário também.
    /// </summary>
    public long? SeqUsuarioDono =>
        SeqVendedor is not null && SeqUsuarioDoVendedor is > 0 ? SeqUsuarioDoVendedor
        : SeqUsuarioResponsavel is > 0 ? SeqUsuarioResponsavel
        : null;

    /// <summary>Se o vendedor existe e o usuário dele não é o responsável da carteira.</summary>
    public bool VendedorDiferenteDoResponsavel =>
        SeqVendedor is not null && SeqUsuarioDoVendedor is > 0 && SeqUsuarioDoVendedor != SeqUsuarioResponsavel;
}

/// <summary>Um usuário do Vórtice que é dono de carteira. Sem senha, nunca.</summary>
/// <param name="SeqUsuario">O identificador na origem.</param>
/// <param name="Login">O login (<c>nome.sobrenome</c>).</param>
/// <param name="Nome">O nome, ou o nome reduzido quando não há.</param>
public sealed record DonoNoVortice(long SeqUsuario, string Login, string? Nome);

/// <summary>Um vínculo cliente × carteira do departamento.</summary>
/// <param name="SeqPessoa">O cliente na origem.</param>
/// <param name="SeqCarteira">A carteira na origem.</param>
/// <param name="Documento">O documento do cliente, recomposto.</param>
/// <param name="IncluidoEmUtc">Quando o vínculo nasceu na origem (UTC).</param>
public sealed record VinculoNoVortice(long SeqPessoa, int SeqCarteira, DocumentoDoVortice Documento, DateTime? IncluidoEmUtc)
{
    /// <summary>A chave do vínculo na trilha de <c>integracao.RegistroDeOrigem</c>.</summary>
    public string Chave => string.Create(CultureInfo.InvariantCulture, $"{SeqPessoa}/{SeqCarteira}");
}

/// <summary>Tudo o que a sincronia das carteiras lê do Vórtice, numa sessão só.</summary>
/// <param name="Departamento">O departamento MAQ-NOVOS.</param>
/// <param name="Carteiras">As carteiras do departamento.</param>
/// <param name="Donos">Os usuários donos, pelo identificador na origem.</param>
/// <param name="Vinculos">Os vínculos cliente × carteira do departamento.</param>
public sealed record LeituraDasCarteirasDoVortice(
    DepartamentoNoVortice Departamento,
    IReadOnlyList<CarteiraNoVortice> Carteiras,
    IReadOnlyDictionary<long, DonoNoVortice> Donos,
    IReadOnlyList<VinculoNoVortice> Vinculos);

/// <summary>
/// A LEITURA DAS CARTEIRAS MAQ-NOVOS DO VÓRTICE (decisão de 24/09/2026).
///
/// <para><b>Por que o Vórtice volta a ser lido.</b> Ele está congelado como fonte de cadastro desde a fase 1
/// (decisão D-12), mas continua VIVO para o comercial: a carteira de cada vendedor é editada lá todo dia, e não
/// existe outro lugar onde ela more. A decisão de 24/09 reabre a leitura só para isto — as carteiras do
/// departamento de máquinas novas —, e só leitura.</para>
///
/// <para><b>O caminho, o mesmo do BI</b> (extrator "Uteis CRM"): <c>IVS_Pes</c> (cliente × departamento ×
/// carteira) → <c>IVS_Carteira</c> → <c>IV_VENDEDOR</c> pelo <c>SeqVendedor</c>. O departamento é
/// <c>IVS_Depto.Depto = 'MAQ-NOVOS'</c>, com HÍFEN; no CRM a linha de negócio é <c>MAQ_NOVOS</c>.</para>
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c> e toda consulta é um
/// <c>SELECT</c> com <c>NOLOCK</c> — o Vórtice é um sistema em produção, e esta leitura não segura trava nele.
/// A coluna de senha de <c>GE_Usuario</c> não aparece em consulta nenhuma.</para>
/// </summary>
/// <param name="opcoes">A configuração do Vórtice — a cadeia vem de <c>Vortice__Conexao</c> ou da tela.</param>
public sealed class LeitorDeCarteirasDoVortice(OpcoesDoVortice opcoes)
{
    /// <summary>A sigla do departamento no Vórtice, com hífen.</summary>
    public const string DepartamentoDeMaquinasNovas = "MAQ-NOVOS";

    /// <summary>
    /// O vendedor que o BI exclui: a carteira de quem saiu da empresa e ainda não foi redistribuída.
    /// </summary>
    public const string VendedorDesligado = "DESLIGADO";

    /// <summary>
    /// Quanto esperar por cada consulta. A maior delas junta os 41 mil vínculos ao cadastro de pessoa; um minuto
    /// basta com folga, e o teto largo evita que um Vórtice lento num dia ruim derrube a rotina à toa.
    /// </summary>
    private const int TempoLimiteDaConsulta = 300;

    /// <summary>
    /// A DIFERENÇA ENTRE O RELÓGIO DA ORIGEM E O UTC, em horas — a mesma conversão da carga antiga: o Vórtice grava
    /// hora local, sem fuso, e o Brasil não tem horário de verão desde 2019.
    /// </summary>
    private const int HorasDeDiferencaParaUtc = 3;

    private const string ConsultaDoDepartamento =
        "SELECT SeqDepto, Depto, Descricao, CicloA, CicloB, CicloC, CicloD FROM IVS_Depto WITH (NOLOCK) WHERE Depto = @depto";

    // AS CARTEIRAS DO DEPARTAMENTO pelas duas portas: o vínculo de departamento declarado e o cliente que está nela.
    // Medido em 24/09/2026: as duas dão as mesmas 89 — mas a carteira que perde o último cliente continua ligada ao
    // departamento, e é ela que precisa aparecer para ter os vínculos encerrados.
    private const string ConsultaDasCarteiras = """
        SELECT c.SeqCarteira, c.NroEmpresa, c.Carteira, c.Descricao, c.SeqUsrResp, c.SeqVendedor,
               v.VENDEDOR, v.CODVENDEDOR, v.SeqUsuario AS UsuarioDoVendedor
        FROM IVS_Carteira c WITH (NOLOCK)
        LEFT JOIN IV_VENDEDOR v WITH (NOLOCK) ON v.SEQVENDEDOR = c.SeqVendedor
        WHERE EXISTS (SELECT 1 FROM IVS_CartDepto cd WITH (NOLOCK) WHERE cd.SeqCarteira = c.SeqCarteira AND cd.SeqDepto = @seqDepto)
           OR EXISTS (SELECT 1 FROM IVS_Pes p WITH (NOLOCK) WHERE p.SeqCarteira = c.SeqCarteira AND p.SeqDepto = @seqDepto)
        ORDER BY c.SeqCarteira
        """;

    private const string ConsultaDosDonos = """
        WITH Carteiras AS (
            SELECT c.SeqCarteira, c.SeqUsrResp, c.SeqVendedor
            FROM IVS_Carteira c WITH (NOLOCK)
            WHERE EXISTS (SELECT 1 FROM IVS_CartDepto cd WITH (NOLOCK) WHERE cd.SeqCarteira = c.SeqCarteira AND cd.SeqDepto = @seqDepto)
               OR EXISTS (SELECT 1 FROM IVS_Pes p WITH (NOLOCK) WHERE p.SeqCarteira = c.SeqCarteira AND p.SeqDepto = @seqDepto)
        ),
        Donos AS (
            SELECT SeqUsrResp AS SeqUsuario FROM Carteiras
            UNION
            SELECT v.SeqUsuario FROM Carteiras k JOIN IV_VENDEDOR v WITH (NOLOCK) ON v.SEQVENDEDOR = k.SeqVendedor
        )
        SELECT u.SeqUsuario, u.CodUsuario, u.Nome, u.NomeReduzido
        FROM GE_Usuario u WITH (NOLOCK)
        WHERE u.SeqUsuario IN (SELECT SeqUsuario FROM Donos WHERE SeqUsuario > 0)
        """;

    private const string ConsultaDosVinculos = """
        SELECT p.SeqPessoa, p.SeqCarteira, p.DtaInclusao, g.FisicaJuridica, g.NroCGCCPF, g.DigCGCCPF
        FROM IVS_Pes p WITH (NOLOCK)
        LEFT JOIN GE_Pessoa g WITH (NOLOCK) ON g.SeqPessoa = p.SeqPessoa
        WHERE p.SeqDepto = @seqDepto AND p.SeqCarteira IS NOT NULL
        """;

    /// <summary>Lê o departamento, as carteiras, os donos e os vínculos, numa sessão de leitura.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDasCarteirasDoVortice>> LerAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Conexao))
            return Resultado<LeituraDasCarteirasDoVortice>.Indisponivel(
                "A leitura das carteiras exige a credencial do Vórtice: grave-a em Configurações › Integrações ou " +
                "defina Vortice__Conexao no servidor. Nada foi gravado.");

        var etapa = "a conexão";
        try
        {
            // A INTENÇÃO DE LEITURA VAI POR CIMA do que vier na cadeia: quem configurou a credencial não precisa
            // lembrar de declará-la, e uma cadeia que dissesse o contrário não vale aqui.
            var cadeia = new SqlConnectionStringBuilder(opcoes.Conexao) { ApplicationIntent = ApplicationIntent.ReadOnly };

            await using var conexao = new SqlConnection(cadeia.ConnectionString);
            await conexao.OpenAsync(ct);

            etapa = "o departamento";
            DepartamentoNoVortice? departamento = null;
            await using (var comando = Comando(conexao, ConsultaDoDepartamento))
            {
                comando.Parameters.AddWithValue("@depto", DepartamentoDeMaquinasNovas);
                await using var leitor = await comando.ExecuteReaderAsync(ct);
                if (await leitor.ReadAsync(ct))
                    departamento = new DepartamentoNoVortice(
                        Inteiro(leitor, 0), Texto(leitor, 1)!, Texto(leitor, 2),
                        Dias(leitor, 3), Dias(leitor, 4), Dias(leitor, 5), Dias(leitor, 6));
            }

            if (departamento is null)
                return Resultado<LeituraDasCarteirasDoVortice>.Indisponivel(
                    $"O departamento {DepartamentoDeMaquinasNovas} não existe no Vórtice. Nada foi gravado.");

            etapa = "as carteiras";
            var carteiras = new List<CarteiraNoVortice>();
            await using (var comando = Comando(conexao, ConsultaDasCarteiras, departamento.SeqDepto))
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    var vendedor = Texto(leitor, 6);
                    carteiras.Add(new CarteiraNoVortice(
                        Inteiro(leitor, 0),
                        Inteiro(leitor, 1),
                        Texto(leitor, 2) ?? string.Empty,
                        Texto(leitor, 3),
                        Longo(leitor, 4),
                        Longo(leitor, 5),
                        // O NOME DO VENDEDOR, como o BI: VENDEDOR; vazio, o código; vazio também, nulo — nunca texto vazio.
                        vendedor ?? Texto(leitor, 7),
                        Longo(leitor, 8),
                        string.Equals(vendedor, VendedorDesligado, StringComparison.OrdinalIgnoreCase)));
                }
            }

            etapa = "os donos";
            var donos = new Dictionary<long, DonoNoVortice>();
            await using (var comando = Comando(conexao, ConsultaDosDonos, departamento.SeqDepto))
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    var login = Texto(leitor, 1);
                    if (login is null) continue;
                    donos[Longo(leitor, 0)!.Value] = new DonoNoVortice(Longo(leitor, 0)!.Value, login, Texto(leitor, 2) ?? Texto(leitor, 3));
                }
            }

            etapa = "os vínculos";
            var vinculos = new List<VinculoNoVortice>(45_000);
            await using (var comando = Comando(conexao, ConsultaDosVinculos, departamento.SeqDepto))
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    var documento = DocumentoDoVortice.Recompor(Decimal(leitor, 4), Decimal(leitor, 5), Texto(leitor, 3));
                    DateTime? incluido = leitor.IsDBNull(2)
                        ? null
                        : DateTime.SpecifyKind(leitor.GetDateTime(2).AddHours(HorasDeDiferencaParaUtc), DateTimeKind.Utc);

                    vinculos.Add(new VinculoNoVortice(Longo(leitor, 0)!.Value, Inteiro(leitor, 1), documento, incluido));
                }
            }

            return Resultado<LeituraDasCarteirasDoVortice>.Ok(
                new LeituraDasCarteirasDoVortice(departamento, carteiras, donos, vinculos));
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código e a etapa.
            return Resultado<LeituraDasCarteirasDoVortice>.Indisponivel(
                $"O Vórtice não respondeu à leitura de {etapa} (erro SQL {falha.Number}). Quase sempre é a rede ou a " +
                "credencial; a sincronia é idempotente e pode rodar de novo. Nada foi gravado.");
        }
        catch (ArgumentException)
        {
            return Resultado<LeituraDasCarteirasDoVortice>.Indisponivel(
                "A cadeia de conexão do Vórtice está malformada. Confira a credencial em Configurações › Integrações. Nada foi gravado.");
        }
    }

    private static SqlCommand Comando(SqlConnection conexao, string sql, int? seqDepto = null)
    {
        var comando = new SqlCommand(sql, conexao) { CommandTimeout = TempoLimiteDaConsulta };
        if (seqDepto is { } depto) comando.Parameters.AddWithValue("@seqDepto", depto);
        return comando;
    }

    private static string? Texto(SqlDataReader leitor, int i)
    {
        if (leitor.IsDBNull(i)) return null;
        var valor = Convert.ToString(leitor.GetValue(i), CultureInfo.InvariantCulture)?.Trim();
        return string.IsNullOrEmpty(valor) ? null : valor;
    }

    private static decimal? Decimal(SqlDataReader leitor, int i) =>
        leitor.IsDBNull(i) ? null : Convert.ToDecimal(leitor.GetValue(i), CultureInfo.InvariantCulture);

    private static long? Longo(SqlDataReader leitor, int i) => Decimal(leitor, i) is { } valor ? (long)valor : null;

    private static int Inteiro(SqlDataReader leitor, int i) => (int)(Decimal(leitor, i) ?? 0);

    private static short? Dias(SqlDataReader leitor, int i) => Decimal(leitor, i) is { } valor && valor > 0 ? (short)valor : null;
}
