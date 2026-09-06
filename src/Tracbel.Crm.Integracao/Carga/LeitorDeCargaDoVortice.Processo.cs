using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// A METADE DO LEITOR QUE TRAZ PROCESSO, AGENDA, HISTÓRICO, CARTEIRA E USUÁRIO.
///
/// <para>Está no mesmo tipo da metade do cadastro, e não num leitor irmão, porque compartilha
/// exatamente o que não vale duplicar: o recorte de atividade do ano, o encanamento de conexão
/// com tempo limite generoso, a montagem das filiais em parâmetros e o tratamento de queda de
/// VPN. Duas classes com dois encanamentos seriam duas chances de um deles esquecer o
/// <c>WITH (NOLOCK)</c>.</para>
///
/// <para><b>Somente leitura, e continua estrutural:</b> nenhuma consulta deste arquivo escreve,
/// todas usam <c>WITH (NOLOCK)</c> e todas têm filtro — de ano, de filial, ou de pertencer ao
/// recorte. Nenhuma varre tabela inteira, e a maior delas (o histórico, 2,4 milhões de linhas na
/// origem) é lida por um ano só.</para>
/// </summary>
public sealed partial class LeitorDeCargaDoVortice
{
    /// <summary>
    /// A DIFERENÇA ENTRE O RELÓGIO DA ORIGEM E O UTC, em horas.
    ///
    /// <para>O sistema de origem grava data e hora <b>local, sem fuso</b> — a coluna é
    /// <c>datetime</c> e não existe compensação nenhuma nela. O Brasil não tem horário de verão
    /// desde 2019, então o fuso de operação da Tracbel Agro é UTC−3 fixo, e a conversão é uma
    /// soma. Para dado anterior a 2019 esta conversão erraria uma hora nos meses de verão; o
    /// recorte desta carga é o ano corrente e não alcança essa faixa. A exceção é a data de
    /// entrada do vínculo de carteira, que pode ser de 2015 — e uma hora de erro numa data de
    /// vinculação não muda nenhuma decisão.</para>
    /// </summary>
    private const int HorasDeDiferencaParaUtc = 3;

    /// <summary>
    /// Os nomes que a origem usa para dizer "isto não foi gente que escreveu".
    ///
    /// <para>[V] Convivem TRÊS na mesma coluna, para a mesma coisa, e um deles é a string
    /// <c>* Indefinido *</c> literal. Juntos, respondem por um quarto do "histórico de
    /// relacionamento com o cliente" do ano.</para>
    /// </summary>
    private static readonly HashSet<string> AutoresDeSistema = new(StringComparer.OrdinalIgnoreCase)
    {
        "VRTCSERVER", "RD", "* Indefinido *", "IMPORT", "PTF v4.01"
    };

    // =============================================================================================
    // Usuário — só o necessário para os vínculos terem dono. Sem senha, nunca.
    // =============================================================================================

    /// <summary>
    /// Lê os usuários que os vínculos do período exigem — os donos de agenda, os autores de
    /// histórico e os responsáveis de carteira.
    ///
    /// <para><b>A coluna de senha não aparece nesta consulta.</b> Não é esquecimento nem
    /// economia: é a decisão de não criar um passivo. [V] a senha do legado tem 30 bytes sem sal
    /// e 80 usuários compartilham o mesmo valor armazenado.</para>
    ///
    /// <para>A filial do usuário vem da <b>atividade</b>, pela mesma razão e com o mesmo
    /// desempate do recorte de clientes: o cadastro de usuário da origem não tem coluna de
    /// empresa.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<UsuarioParaCarga>>> LerUsuariosAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "GE_Usuario",
            recorte,
            """
            WITH Presenca AS (
                SELECT a.SeqUsuario, a.NroEmpresa
                FROM IV_Agenda a WITH (NOLOCK)
                WHERE a.DtaAgenda >= @inicio AND a.DtaAgenda < @fim
                  AND a.NroEmpresa IN ({FILIAIS}) AND a.SeqUsuario > 0
                UNION ALL
                SELECT h.SeqUsuario, h.NroEmpresa
                FROM IV_Historico h WITH (NOLOCK)
                WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
                  AND h.NroEmpresa IN ({FILIAIS}) AND h.SeqUsuario > 0
                UNION ALL
                SELECT c.SeqUsrResp, c.NroEmpresa
                FROM IVS_Carteira c WITH (NOLOCK)
                WHERE c.NroEmpresa IN ({FILIAIS}) AND c.SeqUsrResp > 0
                UNION ALL
                SELECT v.SeqUsuario, c.NroEmpresa
                FROM IVS_Carteira c WITH (NOLOCK)
                JOIN IV_VENDEDOR v WITH (NOLOCK) ON v.SEQVENDEDOR = c.SeqVendedor
                WHERE c.NroEmpresa IN ({FILIAIS}) AND v.SeqUsuario > 0
            ),
            Classificada AS (
                SELECT SeqUsuario, NroEmpresa,
                       ROW_NUMBER() OVER (
                           PARTITION BY SeqUsuario ORDER BY COUNT(*) DESC, NroEmpresa) AS Ordem
                FROM Presenca
                GROUP BY SeqUsuario, NroEmpresa
            )
            SELECT u.SeqUsuario, c.NroEmpresa, u.CodUsuario, u.Nome, u.NomeReduzido,
                   u.EMAILTRAB, u.DTALOGIN,
                   COALESCE(u.DTAALTERACAO, u.DTAINCLUSAO) AS AtualizadoEm
            FROM Classificada c
            JOIN GE_Usuario u WITH (NOLOCK) ON u.SeqUsuario = c.SeqUsuario
            WHERE c.Ordem = 1
            ORDER BY u.SeqUsuario
            """,
            LerUsuario,
            ct);

    private static (UsuarioParaCarga?, LinhaRecusada?) LerUsuario(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqUsuario");

        var login = SaneamentoDaCarga.Texto("login", Texto(leitor, "CodUsuario"), 80, correcoes, rejeicoes);

        if (login is null)
            return (null, Recusar(
                "Usuario", "Usuário sem login na origem", chave,
                "O usuário não tem login no cadastro de origem. O login é a única identidade " +
                "estável que o legado oferece — sem ele não há como ligar a agenda ao dono.",
                leitor));

        var nome = SaneamentoDaCarga.Texto("nomeDoUsuario", Texto(leitor, "Nome"), 200, correcoes, rejeicoes)
                   ?? SaneamentoDaCarga.Texto(
                       "nomeDoUsuario", Texto(leitor, "NomeReduzido"), 200, correcoes, rejeicoes);

        if (nome is null)
            return (null, Recusar(
                "Usuario", "Usuário sem nome na origem", chave,
                "O usuário não tem nome nem nome reduzido. Um responsável sem nome apareceria na " +
                "agenda como um código.",
                leitor));

        // O E-MAIL É LIDO E QUASE NUNCA EXISTE. A leitura fica aqui para que a ausência seja
        // MEDIDA, e não presumida: se um dia a coluna passar a ser preenchida, a carga aproveita
        // sem mudar uma linha.
        var email = SaneamentoDaCarga.Email(Texto(leitor, "EMAILTRAB"), correcoes, rejeicoes);

        if (email is null)
            rejeicoes.Add(new CampoRejeitado(
                "emailDoUsuario", Texto(leitor, "EMAILTRAB"),
                "O cadastro de usuário do sistema de origem não guarda e-mail. O endereço fica " +
                "marcado como ausente até o Entra ID entrar — nunca é construído a partir do login."));

        return (new UsuarioParaCarga(
            ChaveDeOrigem: chave,
            Login: login,
            NomeCompleto: nome,
            CodigoDaFilialNoLegado: (int)Numero(leitor, "NroEmpresa")!.Value,
            UltimoAcessoEm: EmUtc(Data(leitor, "DTALOGIN", opcional: true)),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    // =============================================================================================
    // Linha de negócio e carteira
    // =============================================================================================

    /// <summary>
    /// Lê os departamentos que têm gente do recorte — e a cadência de contato que cada um declara.
    ///
    /// <para>[V] São 29 departamentos cadastrados, 13 com alguma pessoa e 16 vazios. Migrar os 16
    /// encheria o seletor de linha de negócio com opções que nunca tiveram um cliente.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<LinhaDeNegocioParaCarga>>> LerLinhasDeNegocioAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IVS_Depto",
            recorte,
            $$"""
             {{ConsultaDoRecorte}}
             SELECT d.SeqDepto, d.Depto, d.Descricao, d.CicloA, d.CicloB, d.CicloC, d.CicloD
             FROM IVS_Depto d WITH (NOLOCK)
             WHERE EXISTS (
                 SELECT 1 FROM Recorte r
                 JOIN IVS_Pes p WITH (NOLOCK) ON p.SeqPessoa = r.SeqPessoa
                 WHERE p.SeqDepto = d.SeqDepto)
             ORDER BY d.SeqDepto
             """,
            LerLinhaDeNegocio,
            ct);

    private static (LinhaDeNegocioParaCarga?, LinhaRecusada?) LerLinhaDeNegocio(
        SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqDepto");
        var sigla = SaneamentoDaCarga.Texto("linhaDeNegocio", Texto(leitor, "Depto"), 20, correcoes, rejeicoes);
        var nome = SaneamentoDaCarga.Texto(
                       "nomeDaLinhaDeNegocio", Texto(leitor, "Descricao"), 80, correcoes, rejeicoes)
                   ?? sigla;

        if (sigla is null || nome is null)
            return (null, Recusar(
                "LinhaDeNegocio", "Departamento sem sigla nem descrição", chave,
                "O departamento não tem sigla nem descrição legível na origem.",
                leitor));

        var codigo = SaneamentoDeProcesso.Codificar(sigla, 20);

        return (new LinhaDeNegocioParaCarga(
            ChaveDeOrigem: chave,
            Codigo: codigo.Length == 0 ? $"DEPTO_{chave}" : codigo,
            Nome: nome,
            DiasCicloClasseA: Dias(leitor, "CicloA"),
            DiasCicloClasseB: Dias(leitor, "CicloB"),
            DiasCicloClasseC: Dias(leitor, "CicloC"),
            DiasCicloClasseD: Dias(leitor, "CicloD")), null);
    }

    /// <summary>
    /// Lê as carteiras que têm gente do recorte, com o responsável já resolvido.
    ///
    /// <para><b>O responsável tem duas fontes e uma preferência declarada:</b> o usuário
    /// responsável da carteira vem primeiro; quando ele não existe, o CEN é procurado no cadastro
    /// de vendedor e resolvido para o usuário dele. [V] 78% das carteiras não têm CEN, porque a
    /// lista de escolha só oferece 9 nomes de 351 cadastrados.</para>
    ///
    /// <para>A linha de negócio da carteira vem do departamento em que ela tem mais gente: o
    /// cadastro de carteira do legado <b>não tem coluna de departamento</b> — o vínculo mora na
    /// tabela de posse.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<CarteiraParaCarga>>> LerCarteirasAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IVS_Carteira",
            recorte,
            $$"""
             {{ConsultaDoRecorte}},
             Posse AS (
                 SELECT p.SeqCarteira, p.SeqDepto, COUNT(*) AS Pessoas
                 FROM Recorte r
                 JOIN IVS_Pes p WITH (NOLOCK) ON p.SeqPessoa = r.SeqPessoa
                 GROUP BY p.SeqCarteira, p.SeqDepto
             ),
             Predominante AS (
                 SELECT SeqCarteira, SeqDepto,
                        ROW_NUMBER() OVER (
                            PARTITION BY SeqCarteira ORDER BY Pessoas DESC, SeqDepto) AS Ordem
                 FROM Posse
             )
             SELECT c.SeqCarteira, c.NroEmpresa, c.Carteira, c.Descricao,
                    c.SeqUsrResp, c.SeqUrSuperv, c.SeqVendedor,
                    v.SeqUsuario AS UsuarioDoVendedor, pd.SeqDepto,
                    COALESCE(c.DTAALTERACAO, c.DTAINCLUSAO) AS AtualizadoEm
             FROM IVS_Carteira c WITH (NOLOCK)
             JOIN Predominante pd ON pd.SeqCarteira = c.SeqCarteira AND pd.Ordem = 1
             LEFT JOIN IV_VENDEDOR v WITH (NOLOCK) ON v.SEQVENDEDOR = c.SeqVendedor
             WHERE c.NroEmpresa IN ({FILIAIS})
             ORDER BY c.SeqCarteira
             """,
            LerCarteira,
            ct);

    private static (CarteiraParaCarga?, LinhaRecusada?) LerCarteira(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqCarteira");

        var sigla = SaneamentoDaCarga.Texto("carteira", Texto(leitor, "Carteira"), 40, correcoes, rejeicoes);
        var nome = SaneamentoDaCarga.Texto(
                       "nomeDaCarteira", Texto(leitor, "Descricao"), 120, correcoes, rejeicoes)
                   ?? sigla;

        if (sigla is null || nome is null)
            return (null, Recusar(
                "Carteira", "Carteira sem código nem descrição", chave,
                "A carteira não tem código nem descrição legível na origem.",
                leitor));

        var responsavel = Numero(leitor, "SeqUsrResp") is { } dono && dono > 0
            ? ((long)dono).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : Numero(leitor, "UsuarioDoVendedor") is { } cen && cen > 0
                ? ((long)cen).ToString(System.Globalization.CultureInfo.InvariantCulture)
                : null;

        if (responsavel is null)
            rejeicoes.Add(new CampoRejeitado(
                "responsavelDaCarteira", null,
                "A carteira não declara usuário responsável nem CEN com usuário no sistema de " +
                "origem. Entrou sob o operador da carga, marcada para reatribuição."));

        var codigo = SaneamentoDeProcesso.Codificar(sigla, 30);

        return (new CarteiraParaCarga(
            ChaveDeOrigem: chave,
            CodigoDaFilialNoLegado: (int)Numero(leitor, "NroEmpresa")!.Value,
            Codigo: $"{(codigo.Length == 0 ? "CART" : codigo)}_{chave}",
            Nome: nome,
            ChaveDoResponsavelDeOrigem: responsavel,
            ChaveDaLinhaDeNegocioDeOrigem: Numero(leitor, "SeqDepto") is { } depto
                ? ((long)depto).ToString(System.Globalization.CultureInfo.InvariantCulture)
                : null,
            TemSupervisorNaOrigem: Numero(leitor, "SeqUrSuperv") is > 0,
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    /// <summary>
    /// Lê a carteirização das pessoas do recorte — o vínculo cliente × carteira.
    ///
    /// <para>✅ É o melhor ativo do modelo antigo e o CRM novo o replica no dia um: o mesmo
    /// produtor é atendido por um CEN de máquinas, um de peças e um de pneus, cada um com o seu
    /// ciclo. Nem Salesforce nem Dynamics entregam isso de fábrica.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<VinculoDeCarteiraParaCarga>>> LerVinculosDeCarteiraAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IVS_Pes",
            recorte,
            $$"""
             {{ConsultaDoRecorte}}
             SELECT p.SeqPessoa, p.SeqDepto, p.SeqCarteira, p.Potencial, p.Ciclo,
                    p.DtaUltCtto, p.DtaInclusao,
                    COALESCE(p.DtaAlteracao, p.DtaInclusao) AS AtualizadoEm
             FROM Recorte r
             JOIN IVS_Pes p WITH (NOLOCK) ON p.SeqPessoa = r.SeqPessoa
             JOIN IVS_Carteira c WITH (NOLOCK)
                  ON c.SeqCarteira = p.SeqCarteira AND c.NroEmpresa IN ({FILIAIS})
             ORDER BY p.SeqPessoa, p.SeqCarteira
             """,
            LerVinculoDeCarteira,
            ct);

    private static (VinculoDeCarteiraParaCarga?, LinhaRecusada?) LerVinculoDeCarteira(
        SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var pessoa = Chave(leitor, "SeqPessoa");
        var carteira = Chave(leitor, "SeqCarteira");

        var (classe, veioDaOrigem) = SaneamentoDeProcesso.Classe(Texto(leitor, "Potencial"), correcoes);

        return (new VinculoDeCarteiraParaCarga(
            ChaveDeOrigem: $"{pessoa}/{Chave(leitor, "SeqDepto")}/{carteira}",
            ChaveDoClienteDeOrigem: pessoa,
            ChaveDaCarteiraDeOrigem: carteira,
            Classe: classe,
            ClasseVeioDaOrigem: veioDaOrigem,
            DiasCicloContato: Dias(leitor, "Ciclo"),
            VinculadoEm: EmUtc(Data(leitor, "DtaInclusao", opcional: true)),
            AtualizadoEm: EmUtc(Data(leitor, "AtualizadoEm", opcional: true)),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    // =============================================================================================
    // Os catálogos que processo, tarefa e interação exigem
    // =============================================================================================

    /// <summary>
    /// Lê os modelos de fluxo que o dado do período usa — e só eles.
    ///
    /// <para>[V] O catálogo de fluxos tem 62 linhas e 58 marcadas em uso; o dado do período usa
    /// 16. Trazer as 62 encheria o seletor de tipos de processo com fluxos que ninguém abre desde
    /// 2018 — que é exatamente o defeito que o CRM novo existe para não repetir.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<TipoDeProcessoParaCarga>>> LerTiposDeProcessoAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_CodProcesso",
            recorte,
            """
            WITH Usados AS (
                SELECT d.CodProcesso, COUNT(*) AS Processos
                FROM IV_Processo p WITH (NOLOCK)
                JOIN IV_ProcDado d WITH (NOLOCK) ON d.Processo = p.Processo
                WHERE p.DtaInclusao >= @inicio AND p.DtaInclusao < @fim
                  AND d.NroEmpresa IN ({FILIAIS})
                GROUP BY d.CodProcesso
            )
            SELECT u.CodProcesso, u.Processos, cp.Descricao, cp.DescrRed, cp.EmUso
            FROM Usados u
            LEFT JOIN IV_CodProcesso cp WITH (NOLOCK) ON cp.CodProcesso = u.CodProcesso
            ORDER BY u.Processos DESC
            """,
            LerTipoDeProcesso,
            ct);

    private static (TipoDeProcessoParaCarga?, LinhaRecusada?) LerTipoDeProcesso(
        SqlDataReader leitor, string objeto)
    {
        var chave = Chave(leitor, "CodProcesso");
        var descartaveis = new List<CorrecaoAplicada>();
        var recusaveis = new List<CampoRejeitado>();

        var nome = SaneamentoDaCarga.Texto("nomeDoFluxo", Texto(leitor, "Descricao"), 120, descartaveis, recusaveis)
                   ?? SaneamentoDaCarga.Texto("nomeDoFluxo", Texto(leitor, "DescrRed"), 120, descartaveis, recusaveis)
                   ?? $"Fluxo {chave}";

        var codigo = SaneamentoDeProcesso.Codificar(nome, 34);

        return (new TipoDeProcessoParaCarga(
            ChaveDeOrigem: chave,
            Codigo: $"{(codigo.Length == 0 ? "FLUXO" : codigo)}_{chave}",
            Nome: nome,
            EstaAtivoNaOrigem: Numero(leitor, "EmUso") is > 0,
            Processos: (int)(Numero(leitor, "Processos") ?? 0)), null);
    }

    /// <summary>
    /// Lê as fases a partir de ONDE OS PROCESSOS ESTÃO, e não do catálogo do fornecedor.
    ///
    /// <para>É a decisão de modelagem mais consequente desta parte: a fase migrada é a que tem
    /// processo dentro. O catálogo de fases do legado tem 273 linhas por fluxo, muitas sem nunca
    /// terem recebido um processo — e uma fase vazia é uma coluna vazia no funil.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<FaseParaCarga>>> LerFasesAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Processo (fases)",
            recorte,
            """
            SELECT d.CodProcesso,
                   LTRIM(RTRIM(ISNULL(p.Fase, ''))) AS Fase,
                   MIN(ISNULL(p.FaseOrdem, 0)) AS Ordem,
                   COUNT(*) AS Processos
            FROM IV_Processo p WITH (NOLOCK)
            JOIN IV_ProcDado d WITH (NOLOCK) ON d.Processo = p.Processo
            WHERE p.DtaInclusao >= @inicio AND p.DtaInclusao < @fim
              AND d.NroEmpresa IN ({FILIAIS})
            GROUP BY d.CodProcesso, LTRIM(RTRIM(ISNULL(p.Fase, '')))
            ORDER BY d.CodProcesso, Ordem
            """,
            LerFase,
            ct);

    /// <summary>O código de fase que substitui a fase em branco da origem.</summary>
    internal const string FaseSemNome = "NAO_INFORMADA";

    private static (FaseParaCarga?, LinhaRecusada?) LerFase(SqlDataReader leitor, string objeto)
    {
        var nomeCru = (Texto(leitor, "Fase") ?? string.Empty).Trim();
        var nome = nomeCru.Length == 0 ? "Não informada na origem" : nomeCru;
        var codigo = nomeCru.Length == 0 ? FaseSemNome : SaneamentoDeProcesso.Codificar(nomeCru, 40);

        var ordem = Numero(leitor, "Ordem") ?? 0m;

        return (new FaseParaCarga(
            ChaveDoTipoDeProcessoDeOrigem: Chave(leitor, "CodProcesso"),
            Codigo: codigo.Length == 0 ? FaseSemNome : codigo,
            Nome: nome.Length > 80 ? nome[..80] : nome,
            Ordem: ordem is > short.MaxValue or < 0 ? short.MaxValue : (short)ordem,
            EhFinal: codigo.StartsWith("FINALIZ", StringComparison.Ordinal)
                     || codigo.StartsWith("CANCELA", StringComparison.Ordinal)
                     || codigo.StartsWith("CONCLU", StringComparison.Ordinal),
            Processos: (int)(Numero(leitor, "Processos") ?? 0)), null);
    }

    /// <summary>
    /// Lê o catálogo de ações RESTRITO ao que aparece no dado do período.
    ///
    /// <para>[V] O catálogo tem 980 ações e 378 marcadas em uso; o dado do período usa 178. A
    /// diferença entre "marcado em uso" e "usado" é a diferença entre um catálogo cuidado e um
    /// catálogo abandonado — e é ela que faz o campo de seleção do legado ter 602 opções mortas
    /// misturadas com as vivas.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<TipoDeTarefaParaCarga>>> LerTiposDeTarefaAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Acao",
            recorte,
            """
            WITH Usos AS (
                SELECT a.Acao AS Codigo, COUNT(*) AS Usos, MAX(a.DtaAgenda) AS UltimoUso
                FROM IV_Agenda a WITH (NOLOCK)
                WHERE a.DtaAgenda >= @inicio AND a.DtaAgenda < @fim
                  AND a.NroEmpresa IN ({FILIAIS}) AND a.Acao > 0
                GROUP BY a.Acao
                UNION ALL
                SELECT h.AcaoGeradora, COUNT(*), MAX(h.DtaRealizacao)
                FROM IV_Historico h WITH (NOLOCK)
                WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
                  AND h.NroEmpresa IN ({FILIAIS}) AND h.AcaoGeradora > 0
                GROUP BY h.AcaoGeradora
                UNION ALL
                SELECT r.Acao, COUNT(*), MAX(h.DtaRealizacao)
                FROM IV_Historico h WITH (NOLOCK)
                JOIN IV_Resultado r WITH (NOLOCK) ON r.Resultado = h.Resultado
                WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
                  AND h.NroEmpresa IN ({FILIAIS}) AND r.Acao > 0
                GROUP BY r.Acao
            ),
            Consolidado AS (
                SELECT Codigo, SUM(Usos) AS Usos, MAX(UltimoUso) AS UltimoUso
                FROM Usos GROUP BY Codigo
            )
            SELECT c.Codigo AS Acao, c.Usos, c.UltimoUso,
                   a.DescReduzida, a.Descricao, a.EmUso, a.PrazoRealizacao
            FROM Consolidado c
            JOIN IV_Acao a WITH (NOLOCK) ON a.Acao = c.Codigo
            ORDER BY c.Usos DESC
            """,
            LerTipoDeTarefa,
            ct);

    private static (TipoDeTarefaParaCarga?, LinhaRecusada?) LerTipoDeTarefa(SqlDataReader leitor, string objeto)
    {
        var chave = Chave(leitor, "Acao");
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var nome = SaneamentoDaCarga.Texto("nomeDaAcao", Texto(leitor, "Descricao"), 120, correcoes, rejeicoes)
                   ?? SaneamentoDaCarga.Texto("nomeDaAcao", Texto(leitor, "DescReduzida"), 120, correcoes, rejeicoes)
                   ?? $"Ação {chave}";

        // O PRAZO VEM COMO ZERO PORQUE ELE É ZERO. Nenhuma das ações em uso no período preenche
        // o prazo de realização; escrever "1 dia útil" aqui faria toda tarefa migrada nascer com
        // um atraso que a origem nunca declarou.
        var prazo = Numero(leitor, "PrazoRealizacao") ?? 0m;

        var codigo = SaneamentoDeProcesso.Codificar(nome, 34);

        return (new TipoDeTarefaParaCarga(
            ChaveDeOrigem: chave,
            Codigo: $"{(codigo.Length == 0 ? "ACAO" : codigo)}_{chave}",
            Nome: nome,
            PrazoDiasUteis: prazo is > 0 and < short.MaxValue ? (short)prazo : (short)0,
            EstaAtivoNaOrigem: (Texto(leitor, "EmUso") ?? string.Empty).Trim().ToUpperInvariant() is "S",
            UltimoUsoEm: EmUtc(Data(leitor, "UltimoUso", opcional: true)),
            Usos: (int)(Numero(leitor, "Usos") ?? 0)), null);
    }

    /// <summary>
    /// Lê o catálogo de desfechos RESTRITO ao que foi lançado no período, com a classe deduzida
    /// da tabela que move a fase.
    ///
    /// <para>[V] São 4.209 desfechos cadastrados; 505 aparecem no dado do período. Cada linha do
    /// cadastro tem 63 colunas, 35 delas prefixadas com uma sigla de controle e domínio não
    /// documentado. O que vem para cá é o código, o nome, a ação dona e o que o desfecho faz com
    /// o funil — que é tudo o que uma tela precisa.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<ResultadoParaCarga>>> LerResultadosAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Resultado",
            recorte,
            """
            WITH Usados AS (
                SELECT h.Resultado AS Codigo, COUNT(*) AS Usos, MAX(h.DtaRealizacao) AS UltimoUso
                FROM IV_Historico h WITH (NOLOCK)
                WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
                  AND h.NroEmpresa IN ({FILIAIS}) AND h.Resultado > 0
                GROUP BY h.Resultado
            ),
            Efeito AS (
                SELECT pr.Resultado,
                       MAX(CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(pr.FaseSeguinte,''))),'') IS NOT NULL
                                  OR NULLIF(LTRIM(RTRIM(ISNULL(pr.Fase,''))),'') IS NOT NULL
                                THEN 1 ELSE 0 END) AS MoveFase,
                       MAX(LTRIM(RTRIM(ISNULL(pr.Status,'')))) AS StatusDestino
                FROM IV_ProcResultado pr WITH (NOLOCK)
                WHERE pr.Resultado IN (SELECT Codigo FROM Usados)
                GROUP BY pr.Resultado
            )
            SELECT u.Codigo AS Resultado, u.Usos, u.UltimoUso,
                   r.Acao, r.DescReduzida, r.Descricao,
                   e.MoveFase, e.StatusDestino
            FROM Usados u
            JOIN IV_Resultado r WITH (NOLOCK) ON r.Resultado = u.Codigo
            LEFT JOIN Efeito e ON e.Resultado = u.Codigo
            ORDER BY u.Usos DESC
            """,
            LerResultado,
            ct);

    private static (ResultadoParaCarga?, LinhaRecusada?) LerResultado(SqlDataReader leitor, string objeto)
    {
        var chave = Chave(leitor, "Resultado");
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var acao = Chave(leitor, "Acao");

        if (acao.Length == 0 || acao == "0")
            return (null, Recusar(
                "Resultado", "Desfecho sem ação dona na origem", chave,
                "O desfecho não aponta a ação a que pertence. No modelo novo o desfecho pertence " +
                "a um tipo de tarefa, e um desfecho solto não teria onde aparecer.",
                leitor));

        var nome = SaneamentoDaCarga.Texto("nomeDoResultado", Texto(leitor, "Descricao"), 120, correcoes, rejeicoes)
                   ?? SaneamentoDaCarga.Texto(
                       "nomeDoResultado", Texto(leitor, "DescReduzida"), 120, correcoes, rejeicoes)
                   ?? $"Desfecho {chave}";

        var (classe, veioDoMapeamento) = SaneamentoDeProcesso.ClasseDoResultado(
            Texto(leitor, "StatusDestino"), Numero(leitor, "MoveFase") is > 0);

        return (new ResultadoParaCarga(
            ChaveDeOrigem: chave,
            ChaveDoTipoDeTarefaDeOrigem: acao,
            Codigo: $"RES_{chave}",
            Nome: nome,
            Classe: classe,
            ClasseVeioDoMapeamento: veioDoMapeamento,
            UltimoUsoEm: EmUtc(Data(leitor, "UltimoUso", opcional: true)),
            Usos: (int)(Numero(leitor, "Usos") ?? 0)), null);
    }

    // =============================================================================================
    // Processo, tarefa e interação
    // =============================================================================================

    /// <summary>
    /// Lê os processos do período, com o estado e o contexto já juntos.
    ///
    /// <para>[V] A junção entre a tabela de estado e a de contexto é obrigatória e não tem chave
    /// estrangeira declarada — é a origem direta das 345.535 linhas órfãs medidas no legado. Aqui
    /// ela acontece uma vez, na leitura, e o que sai já tem cliente.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<ProcessoParaCarga>>> LerProcessosAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Processo",
            recorte,
            $$"""
             {{ConsultaDoRecorte}}
             SELECT p.Processo, d.NroEmpresa, d.SeqPessoa, d.CodProcesso,
                    p.Fase, p.FaseOrdem, p.Status, p.StatusDesc, p.Realizado,
                    p.Resumo, p.Descricao, p.Valor, p.Qtde,
                    p.DtaInclusao, p.DtaFase, p.DtaStatus, p.DtaRealizacao,
                    p.DtaPrevConclusao, p.DtaPrevConcOrig, p.UsuResponsavel,
                    COALESCE(p.DtaAlteracao, p.DtaInclusao) AS AtualizadoEm
             FROM Recorte r
             JOIN IV_ProcDado d WITH (NOLOCK) ON d.SeqPessoa = r.SeqPessoa
             JOIN IV_Processo p WITH (NOLOCK) ON p.Processo = d.Processo
             WHERE p.DtaInclusao >= @inicio AND p.DtaInclusao < @fim
               AND d.NroEmpresa IN ({FILIAIS})
             ORDER BY p.Processo
             """,
            LerProcesso,
            ct);

    private static (ProcessoParaCarga?, LinhaRecusada?) LerProcesso(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "Processo");
        var cliente = Chave(leitor, "SeqPessoa");
        var fluxo = Chave(leitor, "CodProcesso");

        if (cliente.Length == 0 || cliente == "0")
            return (null, Recusar(
                "Processo", "Processo sem pessoa na origem", chave,
                "O processo não aponta pessoa nenhuma. No modelo novo o cliente é obrigatório e " +
                "tem chave estrangeira — é a correção direta do defeito que produziu 345.535 " +
                "linhas órfãs no sistema de origem.",
                leitor));

        var abertura = EmUtc(Data(leitor, "DtaInclusao", opcional: true));

        if (abertura is not { } nascimento)
            return (null, Recusar(
                "Processo", "Processo sem data de abertura", chave,
                "O processo não tem data de inclusão. Sem ela não há como situá-lo no funil nem " +
                "medir há quanto tempo ele está parado.",
                leitor));

        var statusCru = (Texto(leitor, "Status") ?? string.Empty).Trim();
        var (situacao, _) = SaneamentoDeProcesso.Situacao(statusCru, correcoes);

        var resumo = SaneamentoDaCarga.Texto("tituloDoProcesso", Texto(leitor, "Resumo"), 200, correcoes, rejeicoes);

        var faseCrua = (Texto(leitor, "Fase") ?? string.Empty).Trim();
        var codigoDaFase = faseCrua.Length == 0
            ? FaseSemNome
            : SaneamentoDeProcesso.Codificar(faseCrua, 40);

        // O TÍTULO É COMPOSTO, NÃO INVENTADO, quando a origem não tem um: o número do processo é
        // o que o usuário fala ao telefone, e é o que a origem de fato tem. 93% dos processos do
        // período não têm resumo preenchido — a alternativa seria 42 mil cartões de funil sem
        // rótulo nenhum.
        var titulo = resumo ?? $"Processo {chave}";

        return (new ProcessoParaCarga(
            ChaveDeOrigem: chave,
            CodigoDaFilialNoLegado: (int)Numero(leitor, "NroEmpresa")!.Value,
            ChaveDoClienteDeOrigem: cliente,
            ChaveDoTipoDeProcessoDeOrigem: fluxo,
            CodigoDaFase: codigoDaFase.Length == 0 ? FaseSemNome : codigoDaFase,
            Titulo: titulo,
            TituloVeioDaOrigem: resumo is not null,
            Descricao: SaneamentoDaCarga.Texto(
                "descricaoDoProcesso", Texto(leitor, "Descricao"), 4000, correcoes, rejeicoes),
            Situacao: situacao,
            SituacaoNaOrigem: statusCru.Length == 0 ? "(vazio)" : statusCru,
            ValorEstimado: Positivo(Numero(leitor, "Valor")),
            Quantidade: Positivo(Numero(leitor, "Qtde")),
            PrevisaoConclusao: SomenteData(Data(leitor, "DtaPrevConclusao", opcional: true)),
            PrevisaoConclusaoOriginal: SomenteData(Data(leitor, "DtaPrevConcOrig", opcional: true)),
            AbertoEm: nascimento,
            FaseDesde: EmUtc(Data(leitor, "DtaFase", opcional: true)) ?? nascimento,
            SituacaoDesde: EmUtc(Data(leitor, "DtaStatus", opcional: true)) ?? nascimento,
            ConcluidoEm: EmUtc(Data(leitor, "DtaRealizacao", opcional: true)),
            LoginDoResponsavelNaOrigem: SaneamentoDaCarga.Texto(
                "responsavelDoProcesso", Texto(leitor, "UsuResponsavel"), 80, correcoes, rejeicoes),
            AtualizadoEm: EmUtc(Data(leitor, "AtualizadoEm", opcional: true)),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    /// <summary>
    /// Lê a agenda do período — as tarefas.
    ///
    /// <para><b>Quem concluiu a tarefa vem do andamento que a concluiu</b>, e não do dono dela: é
    /// o andamento que sabe quem apertou o botão. Sem essa junção, toda conclusão migrada seria
    /// atribuída ao dono da agenda — que muitas vezes não é quem executou.</para>
    ///
    /// <para>🔴 <b>E aqui está um achado que só apareceu carregando:</b> o duplo ponteiro da
    /// agenda, elogiado como o melhor detalhe de modelagem do legado, <b>só funciona numa das
    /// direções</b>. Das 79.096 agendas do período marcadas como realizadas, apenas <b>10.442
    /// (13%)</b> têm <c>UltHistorico</c> preenchido — mas <b>89.283</b> têm um andamento
    /// apontando de volta para elas por <c>AgendaOrigem</c>. O ponteiro de ida quase nunca é
    /// gravado; o de volta, quase sempre. Por isso a conclusão é procurada nos dois sentidos, com
    /// preferência para o ponteiro de ida quando ele existe. Usar só o de ida perderia 87% das
    /// conclusões e a agenda migrada nasceria com um passivo falso de 70 mil tarefas.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<TarefaParaCarga>>> LerTarefasAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Agenda",
            recorte,
            $$"""
             WITH Conclusao AS (
                 SELECT h.AgendaOrigem AS SeqAgenda, MAX(h.SeqHistorico) AS SeqHistorico
                 FROM IV_Historico h WITH (NOLOCK)
                 WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
                   AND h.NroEmpresa IN ({FILIAIS}) AND h.AgendaOrigem > 0
                 GROUP BY h.AgendaOrigem
             )
             SELECT a.SeqAgenda, a.NroEmpresa, a.SeqPessoa, a.Processo, a.Acao,
                    a.Assunto, a.AssuntoCmpl, a.Detalhe, a.SeqUsuario,
                    a.DtaAgenda, a.DtaLimiteExecucao, a.Prioridade,
                    a.Realizada, a.HistoricoOrigem, a.TipoAgendamento, a.DtaGeracao,
                    COALESCE(NULLIF(a.UltResultado, 0), h.Resultado) AS UltResultado,
                    COALESCE(NULLIF(a.UltHistorico, 0), c.SeqHistorico) AS UltHistorico,
                    COALESCE(a.DtaRealizacao, h.DtaRealizacao) AS DtaRealizacao,
                    h.SeqUsuario AS UsuarioQueConcluiu,
                    COALESCE(a.DtaRealizacao, a.DtaGeracao, a.DtaAgenda) AS AtualizadoEm
             FROM IV_Agenda a WITH (NOLOCK)
             LEFT JOIN Conclusao c ON c.SeqAgenda = a.SeqAgenda
             LEFT JOIN IV_Historico h WITH (NOLOCK)
                    ON h.SeqHistorico = COALESCE(NULLIF(a.UltHistorico, 0), c.SeqHistorico)
             WHERE a.DtaAgenda >= @inicio AND a.DtaAgenda < @fim
               AND a.NroEmpresa IN ({FILIAIS})
             ORDER BY a.SeqAgenda
             """,
            LerTarefa,
            ct);

    private static (TarefaParaCarga?, LinhaRecusada?) LerTarefa(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqAgenda");
        var responsavel = Chave(leitor, "SeqUsuario");

        if (responsavel.Length == 0 || responsavel == "0")
            return (null, Recusar(
                "Tarefa", "Tarefa sem responsável na origem", chave,
                "A agenda não aponta usuário nenhum. Tarefa sem dono não aparece na agenda de " +
                "ninguém e engrossaria em silêncio o passivo — que na origem já é de 35.662 " +
                "linhas pendentes, a mais antiga de 2012.",
                leitor));

        var agendada = EmUtc(Data(leitor, "DtaAgenda", opcional: true));

        if (agendada is not { } quando)
            return (null, Recusar(
                "Tarefa", "Tarefa sem data de agenda", chave,
                "A agenda não tem data. Uma tarefa sem data não tem lugar na agenda nem prazo " +
                "contra o qual medir atraso.",
                leitor));

        var assunto = SaneamentoDaCarga.Texto("assuntoDaTarefa", Texto(leitor, "Assunto"), 200, correcoes, rejeicoes)
                      ?? SaneamentoDaCarga.Texto(
                          "assuntoDaTarefa", Texto(leitor, "AssuntoCmpl"), 200, correcoes, rejeicoes)
                      ?? $"Tarefa {chave}";

        var concluidaEm = EmUtc(Data(leitor, "DtaRealizacao", opcional: true));
        var desfecho = ChaveOpcional(leitor, "UltResultado");
        var quemConcluiu = ChaveOpcional(leitor, "UsuarioQueConcluiu");

        var situacao = SaneamentoDeProcesso.SituacaoDaTarefaDe(
            Texto(leitor, "Realizada"),
            concluidaEm is not null,
            desfecho is not null,
            quemConcluiu is not null);

        if (situacao is SituacaoDaTarefa.Pendente
            && (Texto(leitor, "Realizada") ?? string.Empty).Trim().ToUpperInvariant() is "S")
            rejeicoes.Add(new CampoRejeitado(
                "conclusaoDaTarefa", "Realizada = S",
                "A origem marca a tarefa como realizada mas não traz as três coisas que uma " +
                "conclusão exige — data, desfecho e quem concluiu. Entrou pendente, para ficar " +
                "visível em vez de fechada sem justificativa."));

        var prioridade = Numero(leitor, "Prioridade") ?? 3m;

        return (new TarefaParaCarga(
            ChaveDeOrigem: chave,
            CodigoDaFilialNoLegado: (int)Numero(leitor, "NroEmpresa")!.Value,
            ChaveDoClienteDeOrigem: ChaveOpcional(leitor, "SeqPessoa"),
            ChaveDoProcessoDeOrigem: ChaveOpcional(leitor, "Processo"),
            ChaveDoTipoDeTarefaDeOrigem: ChaveOpcional(leitor, "Acao") ?? string.Empty,
            Assunto: assunto,
            Detalhe: SaneamentoDaCarga.Texto(
                "detalheDaTarefa", Texto(leitor, "Detalhe"), 4000, correcoes, rejeicoes),
            ChaveDoResponsavelDeOrigem: responsavel,
            AgendadaPara: quando,
            PrazoLimite: EmUtc(Data(leitor, "DtaLimiteExecucao", opcional: true)),
            Prioridade: prioridade is >= 1 and <= 5 ? (short)prioridade : (short)3,
            Situacao: situacao,
            ConcluidaEm: situacao is SituacaoDaTarefa.Concluida ? concluidaEm : null,
            ChaveDeQuemConcluiuDeOrigem: situacao is SituacaoDaTarefa.Concluida ? quemConcluiu : null,
            ChaveDoResultadoDeOrigem: situacao is SituacaoDaTarefa.Concluida ? desfecho : null,
            ChaveDaInteracaoDeConclusaoDeOrigem: ChaveOpcional(leitor, "UltHistorico"),
            ChaveDaInteracaoDeOrigemDeOrigem: ChaveOpcional(leitor, "HistoricoOrigem"),

            // [V] O tipo de agendamento da origem diz se a linha nasceu de regra ou de gente:
            // 'A' é automático (a regra do motor), 'M' é manual. É a única coisa que a origem
            // sabe responder sobre "por que essa tarefa é minha?" — a pergunta de suporte mais
            // comum do legado.
            OrigemAtribuicao: (Texto(leitor, "TipoAgendamento") ?? string.Empty).Trim().ToUpperInvariant() is "A"
                ? OrigemDaAtribuicao.Regra
                : OrigemDaAtribuicao.Manual,
            CriadaEm: EmUtc(Data(leitor, "DtaGeracao", opcional: true)) ?? quando,
            AtualizadoEm: EmUtc(Data(leitor, "AtualizadoEm", opcional: true)),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    /// <summary>
    /// Lê o histórico do período — as interações.
    ///
    /// <para>É a maior leitura da carga: a tabela de origem tem 2,4 milhões de linhas e o recorte
    /// de um ano traz cerca de 124 mil. A duração vem junto e vem vazia — [V] a coluna existe e é
    /// zero em 100% do histórico do período, o que torna impossível qualquer métrica de
    /// produtividade por tempo de atendimento.</para>
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<InteracaoParaCarga>>> LerInteracoesAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Historico",
            recorte,
            $$"""
             SELECT h.SeqHistorico, h.NroEmpresa, h.SeqPessoa, h.Processo,
                    h.AcaoGeradora, h.Resultado, h.ResultadoCmpl, h.AgendaOrigem,
                    h.Detalhe, h.Natureza, h.DtaRealizacao, h.Duracao,
                    h.Latitude, h.Longitude, h.SeqUsuario, h.USUINCLUSAO,
                    r.DescReduzida AS NomeDoResultado,
                    h.DtaRealizacao AS AtualizadoEm
             FROM IV_Historico h WITH (NOLOCK)
             LEFT JOIN IV_Resultado r WITH (NOLOCK) ON r.Resultado = h.Resultado
             WHERE h.DtaRealizacao >= @inicio AND h.DtaRealizacao < @fim
               AND h.NroEmpresa IN ({FILIAIS})
             ORDER BY h.SeqHistorico
             """,
            LerInteracao,
            ct);

    private static (InteracaoParaCarga?, LinhaRecusada?) LerInteracao(SqlDataReader leitor, string objeto)
    {
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var chave = Chave(leitor, "SeqHistorico");
        var autor = Chave(leitor, "SeqUsuario");

        if (autor.Length == 0 || autor == "0")
            return (null, Recusar(
                "Interacao", "Interação sem autor na origem", chave,
                "O andamento não aponta quem o registrou. Interação é fato imutável: sem autor " +
                "ela entraria na linha do tempo do cliente sem ninguém para responder por ela.",
                leitor));

        var ocorrida = EmUtc(Data(leitor, "DtaRealizacao", opcional: true));

        if (ocorrida is not { } quando)
            return (null, Recusar(
                "Interacao", "Interação sem data de realização", chave,
                "O andamento não tem data. Sem ela a linha do tempo do cliente não se ordena e a " +
                "data do último contato não se calcula.",
                leitor));

        var assunto = SaneamentoDaCarga.Texto(
                          "assuntoDaInteracao", Texto(leitor, "NomeDoResultado"), 200, correcoes, rejeicoes)
                      ?? $"Andamento {chave}";

        var (latitude, longitude) = SaneamentoDaCarga.Coordenada(
            Numero(leitor, "Latitude"), Numero(leitor, "Longitude"), rejeicoes);

        var duracao = Numero(leitor, "Duracao");
        if (duracao is null or <= 0)
            rejeicoes.Add(new CampoRejeitado(
                "duracao", duracao?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "O sistema de origem tem a coluna de duração e nunca a preenche — zero em todo o " +
                "histórico do período. Não existe tempo de atendimento para migrar."));

        return (new InteracaoParaCarga(
            ChaveDeOrigem: chave,
            CodigoDaFilialNoLegado: (int)Numero(leitor, "NroEmpresa")!.Value,
            ChaveDoClienteDeOrigem: ChaveOpcional(leitor, "SeqPessoa"),
            ChaveDoProcessoDeOrigem: ChaveOpcional(leitor, "Processo"),
            ChaveDoTipoDeTarefaDeOrigem: ChaveOpcional(leitor, "AcaoGeradora") ?? string.Empty,
            ChaveDoResultadoDeOrigem: ChaveOpcional(leitor, "Resultado"),
            ChaveDaTarefaDeOrigem: ChaveOpcional(leitor, "AgendaOrigem"),
            Assunto: assunto,
            Detalhe: SaneamentoDaCarga.Texto(
                "detalheDaInteracao", Texto(leitor, "Detalhe"), 4000, correcoes, rejeicoes),
            ResultadoComplemento: SaneamentoDaCarga.Texto(
                "resultadoComplemento", Texto(leitor, "ResultadoCmpl"), 200, correcoes, rejeicoes),
            Natureza: SaneamentoDeProcesso.Natureza(
                Texto(leitor, "Natureza"), Texto(leitor, "USUINCLUSAO"), AutoresDeSistema, correcoes),
            OcorridaEm: quando,
            DuracaoMinutos: duracao is > 0 and < int.MaxValue ? (int)duracao : null,
            Latitude: latitude,
            Longitude: longitude,
            ChaveDoAutorDeOrigem: autor,
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }

    // =============================================================================================
    // Conversões de coluna que só esta metade usa
    // =============================================================================================

    /// <summary>Converte o horário local da origem para UTC. Ver <see cref="HorasDeDiferencaParaUtc"/>.</summary>
    private static DateTime? EmUtc(DateTime? local) =>
        local is { } valor
            ? DateTime.SpecifyKind(valor.AddHours(HorasDeDiferencaParaUtc), DateTimeKind.Utc)
            : null;

    /// <summary>A parte de data de um instante da origem, já em horário local.</summary>
    private static DateOnly? SomenteData(DateTime? local) =>
        local is { } valor ? DateOnly.FromDateTime(valor) : null;

    /// <summary>Um valor numérico só quando ele é maior que zero — zero na origem é "não informado".</summary>
    private static decimal? Positivo(decimal? valor) => valor is > 0 ? valor : null;

    /// <summary>Uma quantidade de dias de ciclo, só quando ela é positiva e cabe na coluna.</summary>
    private static short? Dias(SqlDataReader leitor, string coluna) =>
        Numero(leitor, coluna) is { } valor && valor is > 0 and < short.MaxValue ? (short)valor : null;

    /// <summary>A chave de origem de uma coluna que pode ser nula ou zero — as duas significam "não há".</summary>
    private static string? ChaveOpcional(SqlDataReader leitor, string coluna) =>
        Numero(leitor, coluna) is { } valor && valor > 0
            ? ((long)valor).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : null;
}
