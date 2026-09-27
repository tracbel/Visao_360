using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Carga;

/// <summary>A ligação de um processo do Vórtice com o que o CRM já tem.</summary>
/// <param name="ClienteId">O cliente que casou pelo documento; nulo para o prospect.</param>
/// <param name="CarteiraId">A carteira MAQ_NOVOS da pessoa, pelo de-para da sincronia das carteiras.</param>
/// <param name="ResponsavelId">A conta do responsável pelo login; sem ela, o dono da carteira; sem os dois, nulo.</param>
/// <param name="ResponsavelPeloLogin">Se o responsável veio do login do Vórtice, e não do dono da carteira.</param>
internal sealed record LigacaoDoProcesso(long? ClienteId, long? CarteiraId, long? ResponsavelId, bool ResponsavelPeloLogin);

/// <summary>O cliente do CRM que casou com o documento do Vórtice, ou o motivo de não ter casado.</summary>
/// <param name="ClienteId">O cliente — nulo quando não casou.</param>
/// <param name="Motivo">O código de pendência da #236 (<see cref="MotivoDePendenciaDaCarteira"/>); nulo quando casou.</param>
internal sealed record CasamentoDoCliente(long? ClienteId, string? Motivo);

/// <summary>
/// O QUE O CRM JÁ TEM PARA LIGAR AO VÓRTICE — lido uma vez por rodada, e nada disso é criado aqui: o cliente pelo
/// documento, a carteira pelo de-para gravado pela sincronia das carteiras (#236) e a conta pelo login ou pelo de-para do
/// usuário que a mesma sincronia grava (<c>ChaveExterna("Usuario", SeqUsuario)</c>).
///
/// <para><b>Por que uma classe só, e não uma cópia em cada carga.</b> O funil (#247) e a onda 2 ligam o MESMO processo
/// ao MESMO cliente, à MESMA carteira e ao MESMO responsável. Se cada uma tivesse a sua regra, o processo apareceria
/// ligado a um cliente no funil e a outro na lista de oportunidades — e ninguém saberia qual está certo. A regra do
/// cliente é a da sincronia das carteiras (#236): documento válido, exatamente um cliente ativo; mais de um é ambíguo, só
/// excluído é excluído, nenhum é ausente.</para>
/// </summary>
internal sealed class LigacoesComOCrm
{
    private const string EntidadeCarteira = nameof(Carteira);
    private const string EntidadeUsuario = nameof(Usuario);

    private readonly IReadOnlyDictionary<string, (int Ativos, int Excluidos, long? Unico)> _clientesPorDocumento;

    private LigacoesComOCrm(
        IReadOnlyDictionary<string, (int Ativos, int Excluidos, long? Unico)> clientesPorDocumento,
        IReadOnlyDictionary<long, int> empresaDoCliente,
        IReadOnlyDictionary<int, long> carteiraPorSeq,
        IReadOnlyDictionary<long, long> donoDaCarteira,
        IReadOnlyDictionary<string, long> contaPorLogin,
        IReadOnlyDictionary<long, long> contaPorSeqUsuario)
    {
        _clientesPorDocumento = clientesPorDocumento;
        EmpresaDoCliente = empresaDoCliente;
        CarteiraPorSeq = carteiraPorSeq;
        DonoDaCarteira = donoDaCarteira;
        ContaPorLogin = contaPorLogin;
        ContaPorSeqUsuario = contaPorSeqUsuario;
    }

    /// <summary>A filial de cada cliente ativo.</summary>
    public IReadOnlyDictionary<long, int> EmpresaDoCliente { get; }

    /// <summary>A carteira do CRM por <c>SeqCarteira</c> do Vórtice — o de-para da sincronia das carteiras.</summary>
    public IReadOnlyDictionary<int, long> CarteiraPorSeq { get; }

    /// <summary>O dono de cada carteira ativa.</summary>
    public IReadOnlyDictionary<long, long> DonoDaCarteira { get; }

    /// <summary>A conta ativa pelo login — a parte antes do <c>@</c> do nome principal, quando é de uma conta só.</summary>
    public IReadOnlyDictionary<string, long> ContaPorLogin { get; }

    /// <summary>A conta ativa pelo <c>SeqUsuario</c> do Vórtice — o de-para que a sincronia das carteiras grava para os donos.</summary>
    public IReadOnlyDictionary<long, long> ContaPorSeqUsuario { get; }

    /// <summary>Lê tudo o que a ligação precisa, numa passada.</summary>
    /// <param name="banco">O contexto, com alcance de sistema.</param>
    /// <param name="sistemaId">O sistema VORTICE no CRM; nulo quando ainda não existe — então não há de-para nenhum.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<LigacoesComOCrm> LerAsync(CrmDbContext banco, int? sistemaId, CancellationToken ct)
    {
        var clientes = await banco.Clientes.AsNoTracking()
            .Where(c => c.Documento != null)
            .Select(c => new { c.Id, c.Documento, c.EmpresaId, Excluido = c.ExcluidoEm != null })
            .ToListAsync(ct);
        var clientesPorDocumento = clientes
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var ativos = g.Where(c => !c.Excluido).ToList();
                    return (ativos.Count, g.Count() - ativos.Count, ativos.Count == 1 ? (long?)ativos[0].Id : null);
                },
                StringComparer.Ordinal);

        var carteiras = await banco.Carteiras.AsNoTracking()
            .Where(k => k.ExcluidoEm == null)
            .Select(k => new { k.Id, k.ResponsavelId })
            .ToDictionaryAsync(k => k.Id, k => k.ResponsavelId, ct);

        var contas = await banco.Usuarios.AsNoTracking()
            .Where(u => u.ExcluidoEm == null)
            .Select(u => new { u.Id, u.NomePrincipal })
            .ToListAsync(ct);
        var contaPorLogin = contas
            .GroupBy(u => ParteLocal(u.NomePrincipal), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        var ativas = contas.Select(u => u.Id).ToHashSet();

        var carteiraPorSeq = new Dictionary<int, long>();
        var contaPorSeqUsuario = new Dictionary<long, long>();
        if (sistemaId is { } sistema)
        {
            foreach (var chave in await banco.ChavesExternas.AsNoTracking()
                         .Where(c => c.SistemaId == sistema && (c.Entidade == EntidadeCarteira || c.Entidade == EntidadeUsuario))
                         .Select(c => new { c.Entidade, c.ChaveOrigem, c.RegistroId })
                         .ToListAsync(ct))
            {
                if (!long.TryParse(chave.ChaveOrigem, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seq)) continue;

                // A CHAVE ÓRFÃ NÃO LIGA: a sanitização de 15/09 apagou registros e deixou de-para apontando para o nada.
                if (chave.Entidade == EntidadeCarteira && carteiras.ContainsKey(chave.RegistroId) && seq is > 0 and <= int.MaxValue)
                    carteiraPorSeq[(int)seq] = chave.RegistroId;
                else if (chave.Entidade == EntidadeUsuario && ativas.Contains(chave.RegistroId))
                    contaPorSeqUsuario[seq] = chave.RegistroId;
            }
        }

        return new LigacoesComOCrm(
            clientesPorDocumento, clientes.Where(c => !c.Excluido).GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First().EmpresaId),
            carteiraPorSeq, carteiras, contaPorLogin, contaPorSeqUsuario);
    }

    /// <summary>O cliente único ativo com o documento; nulo quando não há, ou quando há mais de um.</summary>
    /// <param name="documento">O documento recomposto do Vórtice.</param>
    public long? Cliente(DocumentoDoVortice documento) => Casar(documento).ClienteId;

    /// <summary>
    /// O cliente único ativo com o documento, ou o motivo — os mesmos códigos da sincronia das carteiras (#236), sem o
    /// refinamento pela SA1: aqui a pergunta é só "casou?".
    /// </summary>
    /// <param name="documento">O documento recomposto do Vórtice.</param>
    public CasamentoDoCliente Casar(DocumentoDoVortice documento) => documento.Situacao switch
    {
        SituacaoDoDocumentoNoVortice.SemDocumento => new(null, MotivoDePendenciaDaCarteira.SemDocumento),
        SituacaoDoDocumentoNoVortice.Zerado => new(null, MotivoDePendenciaDaCarteira.DocumentoZerado),
        SituacaoDoDocumentoNoVortice.Invalido => new(null, MotivoDePendenciaDaCarteira.DocumentoInvalido),
        _ => _clientesPorDocumento.TryGetValue(documento.Numero!, out var comDocumento)
            ? comDocumento.Unico is { } id ? new(id, null)
            : comDocumento.Ativos > 1 ? new(null, MotivoDePendenciaDaCarteira.ClienteAmbiguoNoCrm)
            : new(null, MotivoDePendenciaDaCarteira.ClienteExcluidoNoCrm)
            : new(null, MotivoDePendenciaDaCarteira.ClienteAusenteDoCrm)
    };

    /// <summary>A conta ativa pelo login do Vórtice (aparado, sem diferença de caixa).</summary>
    /// <param name="login">O login, como a origem o escreve.</param>
    public long? ContaDoLogin(string? login) =>
        login is { } texto && ContaPorLogin.TryGetValue(texto.Trim().ToLowerInvariant(), out var conta) ? conta : null;

    /// <summary>
    /// A conta de uma pessoa do Vórtice: primeiro o de-para do <c>SeqUsuario</c> (a identidade estável), depois o login.
    /// Nula quando nenhum dos dois acha conta — quem chama decide o que fica no lugar (decisão P2 da onda 2).
    /// </summary>
    /// <param name="seqUsuario">O usuário no Vórtice.</param>
    /// <param name="login">O login dele (<c>GE_Usuario.CodUsuario</c>), quando lido.</param>
    public long? Conta(long? seqUsuario, string? login) =>
        seqUsuario is { } seq && ContaPorSeqUsuario.TryGetValue(seq, out var pelaChave) ? pelaChave : ContaDoLogin(login);

    /// <summary>
    /// O cliente pelo documento; a carteira pelo de-para gravado pela sincronia das carteiras; o responsável pelo login do
    /// Vórtice e, sem conta, pelo dono da carteira.
    /// </summary>
    /// <param name="documento">O documento da pessoa do processo.</param>
    /// <param name="seqCarteira">A carteira MAQ-NOVOS da pessoa no Vórtice.</param>
    /// <param name="loginDoResponsavel">O <c>UsuResponsavel</c> do processo.</param>
    public LigacaoDoProcesso Do(DocumentoDoVortice documento, int? seqCarteira, string? loginDoResponsavel)
    {
        long? carteira = seqCarteira is { } seq && CarteiraPorSeq.TryGetValue(seq, out var k) ? k : null;
        var pelaConta = ContaDoLogin(loginDoResponsavel);
        long? peloDono = carteira is { } c && DonoDaCarteira.TryGetValue(c, out var dono) ? dono : null;
        return new LigacaoDoProcesso(Cliente(documento), carteira, pelaConta ?? peloDono, pelaConta is not null);
    }

    /// <summary>O login da conta: a parte antes do <c>@</c> do nome principal, aparada e em minúsculas.</summary>
    /// <param name="nomePrincipal">O nome principal da conta.</param>
    public static string ParteLocal(string nomePrincipal)
    {
        var arroba = nomePrincipal.IndexOf('@', StringComparison.Ordinal);
        return (arroba < 0 ? nomePrincipal : nomePrincipal[..arroba]).Trim().ToLowerInvariant();
    }
}
