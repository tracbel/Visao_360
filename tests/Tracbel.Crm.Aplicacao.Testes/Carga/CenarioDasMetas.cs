using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM CADASTRO DE METAS DE MENTIRA, para a carga da API Gestão de Negócios: duas filiais (0101 01 e 05), um operador, um
/// consultor COM conta (o login é o consultor em minúsculas) e outro sem, e a classificação de trator médio. Nomes
/// inventados. O mesmo cenário serve ao SQLite e ao contêiner.
/// </summary>
internal static class CenarioDasMetas
{
    public const string ConsultorComConta = "FULANO.DE.TAL";
    public const string ConsultorSemConta = "BELTRANA.SEM.CONTA";

    /// <summary>Semeia as filiais, o operador, a conta do consultor e a classificação.</summary>
    public static SementeDasMetas Semear(CrmDbContext db, bool forcarIdentificadores)
    {
        var ribeirao = Empresa.Criar("010101", "Tracbel Agro — Ribeirão Preto");
        var ituverava = Empresa.Criar("010105", "Tracbel Agro — Ituverava");
        if (forcarIdentificadores)
        {
            db.Entry(ribeirao).Property(e => e.Id).CurrentValue = 1;
            db.Entry(ituverava).Property(e => e.Id).CurrentValue = 2;
        }

        db.Empresas.AddRange(ribeirao, ituverava);
        db.SaveChanges();

        var operador = Usuario.Criar(Guid.NewGuid(), "operador.carga@tracbel.com.br", "operador.carga", "operador.carga",
            Email.Criar("operador.carga@tracbel.com.br"), ribeirao.Id, criadoPorId: 1);
        var consultor = Usuario.Criar(Guid.NewGuid(), "fulano.de.tal@tracbel.com.br", "Fulano de Tal", "Fulano",
            Email.Criar("fulano.de.tal@tracbel.com.br"), ituverava.Id, criadoPorId: 1);
        if (forcarIdentificadores)
        {
            db.Entry(operador).Property(u => u.Id).CurrentValue = 100L;
            db.Entry(consultor).Property(u => u.Id).CurrentValue = 101L;
        }

        db.Usuarios.AddRange(operador, consultor);

        // NO SQL SERVER A CLASSIFICAÇÃO JÁ VEM DA MIGRAÇÃO; no SQLite, não.
        if (!db.LinhasDeProduto.Any(l => l.Codigo == "TRATOR_MEDIO"))
            db.LinhasDeProduto.Add(LinhaDeProduto.Criar("TRATOR_MEDIO", "Trator médio", null, PorteDeMaquina.Medio));
        db.SaveChanges();

        return new SementeDasMetas(ribeirao.Id, ituverava.Id, operador.Id, consultor.Id,
            db.LinhasDeProduto.Single(l => l.Codigo == "TRATOR_MEDIO").Id);
    }

    /// <summary>Uma linha de meta como a API manda, em Ituverava (05), trator médio, do consultor com conta.</summary>
    public static MetaNaOrigem Linha(
        int id, string mes = "2025-11", string filial = "05", string linha = "TRATOR MÉDIO", string consultor = ConsultorComConta,
        string tipo = "Concessão", string origem = "Campanha", string quantidade = "2") =>
        new(id, mes, filial, linha, consultor, tipo, origem, quantidade, "450000.00", "0.12");

    /// <summary>
    /// O cadastro da primeira leitura: 1 e 2 do consultor com conta, 3 de quem não tem conta, 4 de consórcio, 5 em Ribeirão.
    /// Sem recusa: desde a revisão do PR #248, recusar mais de 1% das linhas aborta a rodada — a recusa que passa é testada
    /// num cadastro grande.
    /// </summary>
    public static List<MetaNaOrigem> CadastroDeMetas() =>
    [
        Linha(1),
        Linha(2, mes: "2025-12", tipo: "Direta", quantidade: "1"),
        Linha(3, consultor: ConsultorSemConta, quantidade: "4"),
        Linha(4, linha: "CONSÓRCIO", origem: "Consórcio", quantidade: "7"),
        Linha(5, filial: "01", quantidade: "3")
    ];

    /// <summary>Um cadastro grande, só de linhas válidas — para a trava de remoção.</summary>
    public static List<MetaNaOrigem> CadastroGrande(int quantas) => [.. Enumerable.Range(1, quantas).Select(i => Linha(i))];

    /// <summary>A carga sobre o cadastro dado.</summary>
    public static CargaDeMetasDaGestaoDeNegocios Sincronia(Func<CrmDbContext> abrir, IReadOnlyList<MetaNaOrigem> cadastro, DateTime agora, long operador) =>
        new(abrir,
            _ => Task.FromResult(Resultado<LeituraDasMetas>.Ok(new LeituraDasMetas(cadastro, new DateTime(2026, 9, 27, 4, 30, 0, DateTimeKind.Utc), null))),
            operador, () => agora, _ => { });
}

/// <summary>O que a semente das metas criou.</summary>
/// <param name="RibeiraoPreto">A filial 010101.</param>
/// <param name="Ituverava">A filial 010105.</param>
/// <param name="Operador">Quem roda a carga.</param>
/// <param name="Consultor">A conta do consultor com conta.</param>
/// <param name="TratorMedio">A classificação de trator médio.</param>
internal sealed record SementeDasMetas(int RibeiraoPreto, int Ituverava, long Operador, long Consultor, int TratorMedio)
{
    /// <summary>As filiais que a carga alcança.</summary>
    public IReadOnlySet<int> Filiais => new HashSet<int> { RibeiraoPreto, Ituverava };
}
