using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM PROTHEUS E UM ART DE MENTIRA, pequenos e completos, para o parque pelo proprietário atual e para o ciclo do ART:
/// clientes com CPF e CNPJ, um cadastro da própria Tracbel (que é cliente no CRM, como 25 deles são em produção), uma
/// máquina que já veio do ART e máquinas do Protheus de todo tipo — VIN com o prefixo 1CQ, número de série curto,
/// componente, estoque, chassi repetido e identificador com lixo. Documentos fictícios, com dígito verificador válido
/// (o da Tracbel é a raiz real do grupo, que é o que a regra reconhece). O mesmo cenário serve ao SQLite e ao contêiner.
/// </summary>
internal static class CenarioDoParque
{
    public const string CpfA = "52998224725";
    public const string CnpjB = "11222333000181";
    public const string CpfC = "01234567890";
    public const string CnpjDaTracbel = "03258870000153";
    public const string CpfForaDoCrm = "11144477735";

    public const string ChassiDoArt = "1RW7250PVMR123456";
    public const string ChassiNovo1Cq = "1CQ6110JAMR000001";
    public const string SerieCurta = "PY6110J012345";

    public static readonly DateOnly FaturadaNoArt = new(2023, 5, 10);

    /// <summary>Semeia as filiais, o operador, os clientes, a classificação e a máquina que já veio do ART.</summary>
    public static SementeDoParque Semear(CrmDbContext db, bool forcarIdentificadores)
    {
        var ribeirao = Empresa.Criar("010101", "Tracbel Agro — Ribeirão Preto");
        var barretos = Empresa.Criar("010103", "Tracbel Agro — Barretos");
        if (forcarIdentificadores)
        {
            db.Entry(ribeirao).Property(e => e.Id).CurrentValue = 1;
            db.Entry(barretos).Property(e => e.Id).CurrentValue = 2;
        }

        db.Empresas.AddRange(ribeirao, barretos);
        db.SaveChanges();

        var operador = Usuario.Criar(Guid.NewGuid(), "operador.carga@tracbel.com.br", "operador.carga", "operador.carga",
            Email.Criar("operador.carga@tracbel.com.br"), ribeirao.Id, criadoPorId: 1);
        if (forcarIdentificadores) db.Entry(operador).Property(u => u.Id).CurrentValue = 100L;
        db.Usuarios.Add(operador);
        db.SaveChanges();

        var clientes = new Dictionary<string, Cliente>(StringComparer.Ordinal);
        foreach (var (documento, nome, empresa) in new[]
                 {
                     (CpfA, "CLIENTE A FICTICIO", ribeirao.Id), (CnpjB, "CLIENTE B FICTICIO LTDA", barretos.Id),
                     (CpfC, "CLIENTE C FICTICIO", ribeirao.Id), (CnpjDaTracbel, "TRACBEL FICTICIA", ribeirao.Id)
                 })
        {
            var cpfCnpj = CpfCnpj.Criar(documento);
            var cliente = Cliente.Criar(empresa, nome, cpfCnpj.EhPessoaFisica ? TipoDePessoa.Fisica : TipoDePessoa.Juridica,
                proprietarioId: operador.Id, criadoPorId: operador.Id, documento: cpfCnpj, situacao: SituacaoDoCliente.Suspect);
            db.Clientes.Add(cliente);
            clientes[documento] = cliente;
        }

        // NO SQL SERVER A CLASSIFICAÇÃO JÁ VEM DA MIGRAÇÃO; no SQLite, não.
        if (!db.LinhasDeProduto.Any(l => l.Codigo == "TRATOR_MEDIO"))
            db.LinhasDeProduto.Add(LinhaDeProduto.Criar("TRATOR_MEDIO", "Trator médio", null, PorteDeMaquina.Medio));
        db.SaveChanges();

        // A MÁQUINA QUE JÁ VEIO DO ART: vendida ao cliente A, faturada em 10/05/2023, com o vínculo de comprador.
        var art = Sistema.Criar(LeitorDoArt.CodigoDoSistema, "ART — vendas de máquina", "MySQL, view somente leitura");
        db.Sistemas.Add(art);
        db.SaveChanges();

        var maquina = Equipamento.RegistrarPelaIntegracao(ribeirao.Id, Chassi.Criar(ChassiDoArt), OrigemDoEquipamento.Art, operador.Id);
        db.Equipamentos.Add(maquina);
        db.SaveChanges();

        var venda = VendaDeMaquina.Registrar(art.Id, "5001", maquina.Id, clientes[CpfA].Id, new DadosDaVendaNaOrigem(
                ribeirao.Id, null, FaturadaNoArt.AddDays(-5), FaturadaNoArt, null, null, "P1", "NF1", "P", "Varejo", false, false, 1,
                "TRATOR MÉDIO", "TR 6110J", "Agro Norte", "Ribeirão Preto", null, "hash-5001", null),
            DateTime.UtcNow, operador.Id);
        db.VendasDeMaquina.Add(venda);
        db.SaveChanges();

        db.VinculosComEquipamento.Add(VinculoDeClienteComEquipamento.RegistrarCompradorNaVenda(
            ribeirao.Id, clientes[CpfA].Id, maquina.Id, venda.Id, art.Id, venda.VendidaEm, operador.Id));
        db.SaveChanges();

        return new SementeDoParque(ribeirao.Id, barretos.Id, operador.Id, clientes.ToDictionary(p => p.Key, p => p.Value.Id), maquina.Id);
    }

    /// <summary>Uma linha da VV1, com as marcas de evidência.</summary>
    public static MaquinaNoProtheus Maquina(
        string chave, string chassi, string? dono, string situacao = "1", string? grupo = "TR 6", bool? nova = true,
        DateOnly? venda = null, bool vendaDoDono = false, DateOnly? ordem = null, bool ordemDoDono = false, string modelo = "6110J") =>
        new(chave, chassi, Chassi.Normalizar(chassi), "JD", "JOHN DEERE", modelo, "TRATOR " + modelo, grupo, "TRATOR LINHA 6000",
            nova, situacao, 2021, 2022, dono, null, venda, vendaDoDono, ordem, ordemDoDono);

    /// <summary>
    /// O cadastro de veículos da primeira leitura: o que entra (VIN 1CQ com nota, série curta com ordem de serviço, a
    /// máquina do ART com o mesmo dono) e o que não entra — cada um com o motivo.
    /// </summary>
    public static List<MaquinaNoProtheus> Parque() =>
    [
        Maquina("000001", ChassiNovo1Cq, CpfA, venda: new(2024, 2, 1), vendaDoDono: true),
        Maquina("000002", "py6110j 012345", CnpjB, nova: false, ordem: new(2025, 1, 15), ordemDoDono: true),
        Maquina("000003", ChassiDoArt, CpfA),
        Maquina("000004", "PCGU12345", CpfA, grupo: "AP"),
        Maquina("000005", "1RW6110JCMR000005", CnpjB, situacao: "0"),
        Maquina("000006", "1RW6110JCMR000006", CnpjDaTracbel, situacao: ""),
        Maquina("000007", "1RW6110JCMR000007", CpfForaDoCrm),
        Maquina("000008", "1RW6110JCMR000008", CpfA),
        Maquina("000009", "1RW6110JCMR000008", CpfC),
        Maquina("000010", "12\"45X", CpfC),
        Maquina("000011", string.Empty, CpfC, situacao: "8")
    ];

    /// <summary>A sincronia sobre o parque dado.</summary>
    public static CargaDoParqueDoProtheus Sincronia(Func<CrmDbContext> abrir, IReadOnlyList<MaquinaNoProtheus> parque, DateTime agora, SementeDoParque semente) =>
        new(abrir, _ => Task.FromResult(Resultado<ParqueNoProtheus>.Ok(new ParqueNoProtheus(parque))), ParceirosPorRaizDeCnpj.RaizesDoGrupo,
            semente.Operador, () => agora, _ => { });

    /// <summary>O identificador da máquina ativa com este chassi.</summary>
    public static long MaquinaId(CrmDbContext db, string chassi) =>
        db.Equipamentos.AsNoTracking().AsEnumerable().Single(e => e.Chassi.Numero == chassi && e.ExcluidoEm == null).Id;
}

/// <summary>O que a semente do parque criou.</summary>
/// <param name="RibeiraoPreto">A filial 010101.</param>
/// <param name="Barretos">A filial 010103.</param>
/// <param name="Operador">Quem roda a sincronia.</param>
/// <param name="Clientes">Os clientes pelo documento.</param>
/// <param name="MaquinaDoArt">A máquina que já veio do ART.</param>
internal sealed record SementeDoParque(int RibeiraoPreto, int Barretos, long Operador, IReadOnlyDictionary<string, long> Clientes, long MaquinaDoArt)
{
    /// <summary>As filiais que a carga alcança.</summary>
    public IReadOnlySet<int> Filiais => new HashSet<int> { RibeiraoPreto, Barretos };
}
