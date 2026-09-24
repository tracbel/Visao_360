using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM VÓRTICE DE MENTIRA, pequeno e completo, para a sincronia das carteiras: um vendedor de campo sem conta no CRM,
/// outro que já tem, o pool do INT.MERCADO, a gaveta "sem potencial", uma carteira digital, uma de filial que o CRM não
/// tem, uma de vendedor desligado e uma de teste de um robô — e clientes com documento bom, sem documento, zerado e
/// fora do CRM. Nomes e documentos fictícios, com dígito verificador válido. O mesmo cenário serve ao SQLite e ao
/// contêiner.
/// </summary>
internal static class CenarioDeCarteirasDoVortice
{
    // Documentos fictícios com dígito verificador válido.
    public const string CpfDoCliente1 = "52998224725";
    public const string CnpjDoCliente2 = "11222333000181";
    public const string CpfDoCliente3 = "01234567890";
    public const string CnpjDoCliente4 = "00000000000191";
    public const string CpfForaDoCrm = "11144477735";

    /// <summary>
    /// Semeia as duas filiais (010101 e 010103), o operador, o vendedor que já tem conta no CRM e os quatro clientes.
    /// </summary>
    /// <param name="db">O banco.</param>
    /// <param name="forcarIdentificadores">
    /// No SQLite, fixa os identificadores (o operador é o 100) — para um teste poder pôr antes dele uma conta que espera.
    /// No SQL Server a coluna é identidade e o banco escolhe.
    /// </param>
    public static Semente Semear(CrmDbContext db, bool forcarIdentificadores)
    {
        var empresas = new List<Empresa>();
        foreach (var (id, codigo, nome) in new[] { (1, "010101", "Ribeirão Preto"), (2, "010103", "Barretos") })
        {
            var empresa = Empresa.Criar(codigo, nome);
            if (forcarIdentificadores) db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
            empresas.Add(empresa);
        }

        db.SaveChanges();

        var usuarios = new List<Usuario>();
        foreach (var (id, upn) in new[] { (100L, "operador.carga@tracbel.com.br"), (200L, "joao.silva@tracbel.com.br") })
        {
            var usuario = Usuario.Criar(Guid.NewGuid(), upn, upn.Split('@')[0], upn.Split('@')[0], Email.Criar(upn), empresas[0].Id, criadoPorId: 1);
            if (forcarIdentificadores) db.Entry(usuario).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(usuario);
            usuarios.Add(usuario);
        }

        db.SaveChanges();

        foreach (var (documento, nome) in new[]
                 {
                     (CpfDoCliente1, "CLIENTE UM FICTICIO"), (CnpjDoCliente2, "CLIENTE DOIS FICTICIO LTDA"),
                     (CpfDoCliente3, "CLIENTE TRES FICTICIO"), (CnpjDoCliente4, "CLIENTE QUATRO FICTICIO SA")
                 })
        {
            var cpfCnpj = CpfCnpj.Criar(documento);
            db.Clientes.Add(Cliente.Criar(
                empresas[0].Id, nome, cpfCnpj.EhPessoaFisica ? TipoDePessoa.Fisica : TipoDePessoa.Juridica,
                proprietarioId: usuarios[0].Id, criadoPorId: usuarios[0].Id, documento: cpfCnpj, situacao: SituacaoDoCliente.Suspect));
        }

        db.SaveChanges();
        return new Semente(empresas[0].Id, empresas[1].Id, usuarios[0].Id, usuarios[1].Id);
    }

    /// <summary>O identificador do cliente do CRM com este documento.</summary>
    public static long ClienteId(CrmDbContext db, string documento) =>
        db.Clientes.AsNoTracking().AsEnumerable().Single(c => c.Documento?.Numero == documento).Id;

    /// <summary>Os donos na origem.</summary>
    public static readonly IReadOnlyDictionary<long, DonoNoVortice> Donos = new Dictionary<long, DonoNoVortice>
    {
        [1001] = new(1001, "MARIA.SOUZA", "MARIA SOUZA FICTICIA"),
        [1002] = new(1002, "JOAO.SILVA", "JOAO SILVA FICTICIO"),
        [482] = new(482, "INT.MERCADO", "INT.MERCADO"),
        [1003] = new(1003, "ANA.DIGITAL", "ANA DIGITAL FICTICIA"),
        [1004] = new(1004, "PEDRO.SAIU", "PEDRO SAIU FICTICIO"),
        [1312] = new(1312, "ROBO.APP", "teste mobile lite")
    };

    /// <summary>Uma carteira de vendedor.</summary>
    public static CarteiraNoVortice ComVendedor(int seq, int empresa, string codigo, long usuario, string? descricao = null, bool desligado = false) =>
        new(seq, empresa, codigo, descricao ?? codigo, usuario, seq + 5000, desligado ? "DESLIGADO" : "CEN - FICTICIO", usuario, desligado);

    /// <summary>Uma carteira sem vendedor — o dono é o responsável.</summary>
    public static CarteiraNoVortice SemVendedor(int seq, int empresa, string codigo, long responsavel, string? descricao = null) =>
        new(seq, empresa, codigo, descricao ?? codigo, responsavel, null, null, null, false);

    /// <summary>As carteiras da primeira leitura.</summary>
    public static List<CarteiraNoVortice> Carteiras() =>
    [
        ComVendedor(18, 1, "MAQ_01RIB_02", 1001),
        ComVendedor(19, 3, "MAQ_03BAR_02", 1002),
        SemVendedor(708, 1, "TBA_FILIAIS", 482, "FILIAIS TRACBEL AGRO"),
        SemVendedor(717, 1, "TBA_S/POTENCIAL", 482, "SEM POTENCIAL"),
        ComVendedor(752, 1, "DGT_01RIB", 1003),
        SemVendedor(714, 5, "TBA_ARRENDAMENT", 482, "ARRENDAMENTO TRACBEL AGRO"),
        ComVendedor(900, 1, "MAQ_01RIB_09", 1004, desligado: true),
        SemVendedor(724, 1, "TESTE_ROBO", 1312, "TESTE APP MOBILE LITE")
    ];

    /// <summary>Um vínculo com documento de CPF.</summary>
    public static VinculoNoVortice Com(long pessoa, int carteira, string documento) =>
        new(pessoa, carteira, Recompor(documento), new DateTime(2025, 3, 10, 13, 0, 0, DateTimeKind.Utc));

    /// <summary>Os vínculos da primeira leitura.</summary>
    public static List<VinculoNoVortice> Vinculos() =>
    [
        Com(1, 18, CpfDoCliente1),
        Com(2, 19, CnpjDoCliente2),
        Com(3, 708, CpfDoCliente3),
        Com(4, 752, CnpjDoCliente4),
        new(5, 18, DocumentoDoVortice.Recompor(null, null, "F"), null),
        new(6, 18, DocumentoDoVortice.Recompor(0, 0, "F"), null),
        Com(7, 18, CpfForaDoCrm),
        Com(8, 714, CpfDoCliente1),
        Com(9, 900, CnpjDoCliente2),
        Com(10, 717, CnpjDoCliente2),
        Com(11, 724, CpfDoCliente3)
    ];

    /// <summary>A leitura inteira.</summary>
    public static LeituraDasCarteirasDoVortice Leitura(List<CarteiraNoVortice>? carteiras = null, List<VinculoNoVortice>? vinculos = null) =>
        new(new DepartamentoNoVortice(2, "MAQ-NOVOS", "Venda de Máquinas e Implemento", 180, 180, 180, 360),
            carteiras ?? Carteiras(), Donos, vinculos ?? Vinculos());

    /// <summary>A sincronia sobre a leitura dada, sem SA1.</summary>
    public static CargaDeCarteirasDoVortice Sincronia(Func<CrmDbContext> abrir, LeituraDasCarteirasDoVortice leitura, DateTime agora, Semente semente) =>
        new(abrir, _ => Task.FromResult(Resultado<LeituraDasCarteirasDoVortice>.Ok(leitura)), null, semente.DeParaDeFiliais, semente.Operador,
            () => agora, _ => { });

    private static DocumentoDoVortice Recompor(string documento) =>
        documento.Length == 11
            ? DocumentoDoVortice.Recompor(Numero(documento[..9]), Numero(documento[9..]), "F")
            : DocumentoDoVortice.Recompor(Numero(documento[..12]), Numero(documento[12..]), "J");

    private static decimal Numero(string texto) => decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>O que a semente criou.</summary>
/// <param name="RibeiraoPreto">A filial 010101 — o <c>NroEmpresa</c> 1 do Vórtice.</param>
/// <param name="Barretos">A filial 010103 — o <c>NroEmpresa</c> 3 do Vórtice.</param>
/// <param name="Operador">Quem roda a sincronia.</param>
/// <param name="JoaoNoCrm">O vendedor que já tem conta no CRM, pelo Entra ID.</param>
internal sealed record Semente(int RibeiraoPreto, int Barretos, long Operador, long JoaoNoCrm)
{
    /// <summary>O de-para de filiais: o <c>NN</c> do Vórtice para a filial ativa do CRM. A 5 não está: não é filial ativa.</summary>
    public IReadOnlyDictionary<int, int> DeParaDeFiliais => new Dictionary<int, int> { [1] = RibeiraoPreto, [3] = Barretos };
}
