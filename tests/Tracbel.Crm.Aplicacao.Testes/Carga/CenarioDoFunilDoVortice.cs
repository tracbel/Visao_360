using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM VÓRTICE DE MENTIRA PARA O FUNIL, pequeno e completo: um processo de cliente casado que chega ao Pedido, com a
/// carteira que a sincronia das carteiras trouxe e o responsável pelo login; um prospect da entrada digital; um pai DNA
/// e o filho que herda dele; um processo de filial fora do CRM; um sem resultado aceito; um de antes da janela — e 60
/// prospects de enchimento, para que tirar uma linha seja 1,4% e não a metade do funil. E as respostas de venda perdida
/// de cada regra de papel. Nomes e documentos fictícios.
/// </summary>
internal static class CenarioDoFunilDoVortice
{
    /// <summary>O CPF fictício do cliente casado.</summary>
    public const string CpfDoCliente = "52998224725";

    /// <summary>Os prospects de enchimento — um Lead cada.</summary>
    public const int ProspectsDeEnchimento = 60;

    /// <summary>As respostas de venda perdida de enchimento — uma principal cada.</summary>
    public const int PerdasDeEnchimento = 30;

    /// <summary>As linhas do funil que a primeira rodada cria: 5 (casado) + 1 (digital) + 4 (filho DNA) + 60.</summary>
    public const int LinhasDoFunil = 5 + 1 + 4 + ProspectsDeEnchimento;

    /// <summary>
    /// Semeia as duas filiais (010101 = NN 1, 010103 = NN 3), o operador, a conta da vendedora, o cliente casado, o
    /// sistema VORTICE e a carteira MAQ_NOVOS 18 com o de-para que a sincronia das carteiras teria gravado.
    /// </summary>
    public static SementeDoFunil Semear(CrmDbContext db)
    {
        var ribeirao = Empresa.Criar("010101", "Ribeirão Preto");
        var barretos = Empresa.Criar("010103", "Barretos");
        db.Empresas.AddRange(ribeirao, barretos);
        db.SaveChanges();

        var operador = Usuario.Criar(Guid.NewGuid(), "operador.carga@tracbel.com.br", "operador.carga", "operador.carga",
            Email.Criar("operador.carga@tracbel.com.br"), ribeirao.Id, criadoPorId: 1);
        var maria = Usuario.Criar(Guid.NewGuid(), "maria.souza@tracbel.com.br", "maria.souza", "maria.souza",
            Email.Criar("maria.souza@tracbel.com.br"), ribeirao.Id, criadoPorId: 1);
        db.Usuarios.AddRange(operador, maria);
        db.SaveChanges();

        var cliente = Cliente.Criar(ribeirao.Id, "CLIENTE FICTICIO DO FUNIL", TipoDePessoa.Fisica, proprietarioId: operador.Id,
            criadoPorId: operador.Id, documento: CpfCnpj.Criar(CpfDoCliente), situacao: SituacaoDoCliente.Suspect);
        db.Clientes.Add(cliente);

        var sistema = Sistema.Criar("VORTICE", "Vórtice CRM (sistema legado)", "SQL Server, somente leitura");
        db.Sistemas.Add(sistema);
        var linha = LinhaDeNegocio.Criar("MAQ_NOVOS", "Máquinas novas");
        db.LinhasDeNegocio.Add(linha);
        db.SaveChanges();

        // A CARTEIRA É DE OUTRA DONA (o operador): o responsável do processo 1001 vem do login, e não do dono da carteira.
        var carteira = Carteira.Criar(ribeirao.Id, linha.Id, "MAQ_01RIB_02_18", "CARTEIRA FICTICIA", operador.Id, operador.Id);
        db.Carteiras.Add(carteira);
        db.SaveChanges();

        db.ChavesExternas.Add(ChaveExterna.Criar(sistema.Id, nameof(Carteira), carteira.Id, "18"));
        db.SaveChanges();

        return new SementeDoFunil(ribeirao.Id, barretos.Id, operador.Id, maria.Id, cliente.Id, carteira.Id);
    }

    public static DateTime Em(int ano, int mes, int dia, int hora = 10) => new(ano, mes, dia, hora, 0, 0, DateTimeKind.Utc);

    private static DocumentoDoVortice Cpf(string cpf) =>
        DocumentoDoVortice.Recompor(decimal.Parse(cpf[..9], CultureInfo.InvariantCulture), decimal.Parse(cpf[9..], CultureInfo.InvariantCulture), "F");

    private static readonly DocumentoDoVortice SemDocumento = DocumentoDoVortice.Recompor(null, null, "F");

    public static ProcessoNoFunilDoVortice P(
        long numero, short tipo, int nn, DateTime? incluido, long? dna = null, DocumentoDoVortice? documento = null, int? carteira = null,
        string? login = null, string? status = null) =>
        new(numero, tipo, dna ?? numero, nn, numero + 100_000, documento ?? SemDocumento, carteira, login, incluido, incluido, status, null);

    /// <summary>Os processos da primeira leitura.</summary>
    public static List<ProcessoNoFunilDoVortice> Processos()
    {
        var processos = new List<ProcessoNoFunilDoVortice>
        {
            P(1001, 41, 1, Em(2024, 1, 5), documento: Cpf(CpfDoCliente), carteira: 18, login: "MARIA.SOUZA", status: "EM ABERTO"),
            P(1002, 31, 1, Em(2024, 2, 1)),
            P(1003, 41, 3, Em(2024, 1, 5)),
            P(1004, 50, 3, Em(2024, 2, 1), dna: 1003),
            P(1005, 41, 6, Em(2024, 1, 5)),
            P(1006, 41, 1, Em(2024, 1, 5)),
            P(1007, 41, 1, Em(2022, 5, 1))
        };

        for (var i = 0; i < ProspectsDeEnchimento; i++) processos.Add(P(2000 + i, 31, 1, Em(2025, 1, 1)));
        return processos;
    }

    /// <summary>O histórico da primeira leitura.</summary>
    public static List<LinhaDoHistoricoDoFunil> Historico()
    {
        var historico = new List<LinhaDoHistoricoDoFunil>
        {
            new(1, 1001, 250, Em(2024, 1, 10), 50),
            new(2, 1001, 3239, Em(2024, 3, 1), null),
            new(3, 1002, 1278, Em(2024, 2, 2), null),
            new(4, 1003, 250, Em(2024, 1, 6), null),
            new(5, 1004, 2563, Em(2024, 2, 10), null),
            new(6, 1005, 250, Em(2024, 1, 6), null),
            new(7, 1006, 9999, Em(2024, 1, 6), 50),
            new(8, 1007, 250, Em(2022, 5, 2), null)
        };

        for (var i = 0; i < ProspectsDeEnchimento; i++) historico.Add(new(100 + i, 2000 + i, 1278, Em(2025, 1, 2), null));
        return historico;
    }

    /// <summary>A leitura do funil.</summary>
    public static LeituraDoFunilDoVortice Funil(List<ProcessoNoFunilDoVortice>? processos = null, List<LinhaDoHistoricoDoFunil>? historico = null) =>
        new(processos ?? Processos(), historico ?? Historico());

    public static RespostaDeVendaPerdidaNoVortice R(
        long questionario, string formulario, DateTime? registrada, long? processo, long pessoa, int? empresaDoProcesso = null,
        int? empresaDoHistorico = null, DocumentoDoVortice? documento = null, string? motivo = null, string? marca = null,
        decimal? preco = null, string? participamos = null, decimal quantidade = 1) =>
        new(questionario, formulario, registrada, processo, pessoa, empresaDoProcesso, empresaDoHistorico, documento ?? SemDocumento,
            "TRATOR", marca, "MODELO X", "REVENDA FICTICIA", "7230J", quantidade, null, preco, null, motivo, participamos);

    /// <summary>As respostas de venda perdida da primeira leitura.</summary>
    public static List<RespostaDeVendaPerdidaNoVortice> Respostas()
    {
        var respostas = new List<RespostaDeVendaPerdidaNoVortice>
        {
            R(1, FormulariosDaVendaPerdida.Antigo, Em(2014, 5, 10, 13), null, 777, empresaDoHistorico: 1, documento: Cpf(CpfDoCliente),
                motivo: "Preço", marca: "New Holland", preco: 500_000m, participamos: "SIM"),
            R(2, FormulariosDaVendaPerdida.Jde, Em(2014, 5, 10, 18), null, 777, empresaDoHistorico: 1, marca: "NEW HOLLAND"),
            R(3, FormulariosDaVendaPerdida.Fy25, Em(2025, 9, 1), 1001, 101001, empresaDoProcesso: 1, motivo: "PRAZO DE ENTREGA", marca: "CASE"),
            R(4, FormulariosDaVendaPerdida.SemParticipacao, Em(2025, 9, 5), 1001, 101001, empresaDoProcesso: 1, participamos: "NAO"),
            R(5, FormulariosDaVendaPerdida.VpTrator, Em(2025, 9, 1, 11), 1001, 101001, empresaDoProcesso: 1, marca: "CASE", preco: 610_000m),
            R(6, FormulariosDaVendaPerdida.VpPlantadeira, Em(2025, 9, 1), 2000, 102000, empresaDoProcesso: 1),
            R(7, FormulariosDaVendaPerdida.Fy25, null, 1001, 101001, empresaDoProcesso: 1),
            R(8, FormulariosDaVendaPerdida.Fy25, Em(2025, 9, 2), 1005, 101005, empresaDoProcesso: 6)
        };

        for (var i = 0; i < PerdasDeEnchimento; i++)
            respostas.Add(R(100 + i, FormulariosDaVendaPerdida.MaqImp, Em(2025, 3, 1), 2000 + i, 102000 + i, empresaDoProcesso: 1, motivo: "PRECO"));
        return respostas;
    }

    /// <summary>A rotina sobre as leituras dadas.</summary>
    public static CargaDoFunilDoVortice Rotina(
        Func<CrmDbContext> abrir, SementeDoFunil semente, LeituraDoFunilDoVortice funil, List<RespostaDeVendaPerdidaNoVortice> respostas,
        DateTime agora, IReadOnlyDictionary<int, int>? deParaDeFiliais = null, int tamanhoDoBloco = CargaDoFunilDoVortice.TamanhoDoBloco) =>
        new(abrir,
            (_, _) => Task.FromResult(Resultado<LeituraDoFunilDoVortice>.Ok(funil)),
            _ => Task.FromResult(Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>.Ok(respostas)),
            deParaDeFiliais ?? semente.DeParaDeFiliais, semente.Operador, () => agora, _ => { }, tamanhoDoBloco);

    /// <summary>A linha do funil deste processo neste estágio.</summary>
    public static EstagioDoProcesso? Linha(CrmDbContext db, long numero, EstagioDoFunil estagio) =>
        db.EstagiosDoProcesso.AsNoTracking().SingleOrDefault(e => e.NumeroDoProcessoNaOrigem == numero && e.Estagio == estagio);
}

/// <summary>O que a semente do funil criou.</summary>
/// <param name="RibeiraoPreto">A filial 010101 — o <c>NN</c> 1.</param>
/// <param name="Barretos">A filial 010103 — o <c>NN</c> 3.</param>
/// <param name="Operador">Quem roda a rotina (e dono da carteira).</param>
/// <param name="Maria">A vendedora, responsável pelo processo 1001 pelo login.</param>
/// <param name="ClienteId">O cliente casado.</param>
/// <param name="CarteiraId">A carteira MAQ_NOVOS 18 no CRM.</param>
internal sealed record SementeDoFunil(int RibeiraoPreto, int Barretos, long Operador, long Maria, long ClienteId, long CarteiraId)
{
    /// <summary>O de-para de filiais: o NN 6 (Colorado) não é filial ativa do CRM.</summary>
    public IReadOnlyDictionary<int, int> DeParaDeFiliais => new Dictionary<int, int> { [1] = RibeiraoPreto, [3] = Barretos };
}
