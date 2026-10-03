using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Api.Testes.Oraculo;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O NÚMERO IGUAL ANTES E DEPOIS (plano 2 do documento 54): a apuração dividida dá, campo a campo e na mesma ordem, o
/// que a apuração da main 4cb55a6 dá — no cenário rico desta classe, em onze consultas que cobrem cada filtro.
/// </summary>
public sealed partial class IndicadoresTerritoriaisTestes
{
    private static readonly DateTime AgoraDoOraculo = new(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);

    public static TheoryData<string> CasosDoOraculo() =>
    [
        "filial", "empresa", "norte", "loja", "filialDaVenda", "filialDoCliente", "categoria", "responsavel",
        "mesEmCurso", "anoAnterior", "dezoitoMeses"
    ];

    private static ConsultaDeIndicadoresTerritoriais ConsultaDoCaso(string caso)
    {
        var inicial = new DateOnly(2026, 1, 1);
        var final = new DateOnly(2026, 6, 1);
        return caso switch
        {
            "filial" => new(inicial, final, null, null),
            "empresa" => new(inicial, final, null, null, VisaoTerritorial.Empresa),
            "norte" => new(inicial, final, RegiaoDaAreaDeAtuacao.Norte, null),
            "loja" => new(inicial, final, null, ApiEmMemoria.FilialDeRibeirao),
            "filialDaVenda" => new(inicial, final, null, null, FilialDaVendaId: 1),
            "filialDoCliente" => new(inicial, final, null, null, FilialDoClienteId: 1),
            "categoria" => new(inicial, final, null, null, CategoriaDeMaquina: "TRATOR"),
            "responsavel" => new(inicial, final, null, null, ResponsavelId: 100),
            "mesEmCurso" => new(inicial, final, null, null, MesCorrente: final),
            "anoAnterior" => new(inicial, final, null, null, ComDemandaDoAnoAnterior: true),
            "dezoitoMeses" => new(new DateOnly(2025, 1, 1), final, null, null),
            _ => throw new ArgumentOutOfRangeException(nameof(caso), caso, "caso do oráculo desconhecido")
        };
    }

    [Theory]
    [MemberData(nameof(CasosDoOraculo))]
    public async Task A_apuracao_da_o_mesmo_numero_que_a_de_antes_da_divisao(string caso)
    {
        await SemearAsync();
        var consulta = ConsultaDoCaso(caso);

        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();

        await using var dbAntigo = new CrmDbContext(opcoes, new UsuarioDoOraculo(), new DiarioDoOraculo());
        var antiga = await new ApuracaoAntigaDosIndicadores(dbAntigo).ApurarAsync(consulta, AgoraDoOraculo, default);

        await using var dbNovo = new CrmDbContext(opcoes, new UsuarioDoOraculo(), new DiarioDoOraculo());
        var nova = await new RepositorioDeIndicadoresTerritoriais(dbNovo).ApurarAsync(consulta, AgoraDoOraculo, default);

        nova.Should().BeEquivalentTo(antiga, o => o.WithStrictOrdering().ComparingRecordsByMembers(),
            $"o caso '{caso}' tem de dar o mesmo número da apuração de antes da divisão (plano 2 do doc 54)");
    }

    /// <summary>O CEN de Ribeirão, com a permissão de abrir a empresa inteira — a visão Empresa do oráculo precisa dela.</summary>
    private sealed class UsuarioDoOraculo : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = new(
            usuarioId: 100,
            nomeExibicao: "oráculo",
            empresaId: 1,
            empresasVisiveis: new HashSet<int> { 1 },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: new Dictionary<string, Profundidade>(StringComparer.Ordinal)
            {
                [ContextoAcesso.PermissaoDeAlcanceEntreEmpresas] = Profundidade.Organizacao
            });
    }

    /// <summary>O diário que a abertura do alcance exige — aqui não há o que registrar.</summary>
    private sealed class DiarioDoOraculo : IDiarioDeAlcanceEntreEmpresas
    {
        public void Abriu(ContextoAcesso acesso, string motivo) { }

        public void Fechou(ContextoAcesso acesso, string motivo, TimeSpan duracao) { }
    }
}
