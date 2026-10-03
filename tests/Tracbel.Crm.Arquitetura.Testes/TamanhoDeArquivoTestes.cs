using FluentAssertions;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// O TAMANHO DE ARQUIVO (documento 54 §3.5): arquivo novo nasce com até <see cref="Limite"/> linhas. Os que já passam disso
/// estão listados com o tamanho de hoje, e só podem diminuir — quem mexe num deles tira um pedaço, não acrescenta.
/// </summary>
public sealed class TamanhoDeArquivoTestes
{
    private const int Limite = 600;

    /// <summary>Os grandes de 03/10/2026, pelo caminho a partir da raiz, com o tamanho medido.</summary>
    private static readonly Dictionary<string, int> Conhecidos = new(StringComparer.Ordinal)
    {
        ["src/Tracbel.Crm.Aplicacao/Mercado/ObterCenariosDeMercado.cs"] = 657,
        ["src/Tracbel.Crm.Aplicacao/Mercado/ObterDemandaEPrevisao.cs"] = 687,
        ["src/Tracbel.Crm.Aplicacao/Mercado/ObterDimensionamentoDaAdr.cs"] = 692,
        ["src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs"] = 1193,
        ["src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs"] = 1140,
        ["src/Tracbel.Crm.Carga/CargaDaEstruturaAgropecuaria.cs"] = 767,
        ["src/Tracbel.Crm.Carga/CargaDasOportunidadesDoVortice.cs"] = 1146,
        ["src/Tracbel.Crm.Carga/CargaDeCarteirasDoVortice.cs"] = 1166,
        ["src/Tracbel.Crm.Carga/CargaDeFaturamentoDoProtheus.cs"] = 829,
        ["src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs"] = 1869,
        ["src/Tracbel.Crm.Carga/CargaDeTerritorio.cs"] = 1030,
        ["src/Tracbel.Crm.Carga/CargaDoArt.cs"] = 1101,
        ["src/Tracbel.Crm.Carga/CargaDoFunilDoVortice.cs"] = 1136,
        ["src/Tracbel.Crm.Carga/CargaDoParqueDoProtheus.cs"] = 833,
        ["src/Tracbel.Crm.Carga/CargaDoVortice.cs"] = 1232,
        ["src/Tracbel.Crm.Carga/Program.cs"] = 2179,
        ["src/Tracbel.Crm.Dominio/Integracao/ConexoesERotinas.cs"] = 1186,
        ["src/Tracbel.Crm.Dominio/Integracao/IntegracaoDeOrigem.cs"] = 956,
        ["src/Tracbel.Crm.Dominio/Organizacao/AreaDeAtuacao.cs"] = 765,
        ["src/Tracbel.Crm.Dominio/Organizacao/CatalogoDeCulturas.cs"] = 726,
        ["src/Tracbel.Crm.Dominio/Organizacao/EstruturaAgropecuaria.cs"] = 649,
        ["src/Tracbel.Crm.Dominio/Organizacao/ParametrosDoPotencial.cs"] = 768,
        ["src/Tracbel.Crm.Dominio/Portas/PortasDeIndicadoresTerritoriais.cs"] = 1233,
        ["src/Tracbel.Crm.Dominio/Portas/PortasDeRelacionamento.cs"] = 699,
        ["src/Tracbel.Crm.Infraestrutura/Persistencia/CrmDbContext.cs"] = 1009,
        ["src/Tracbel.Crm.Integracao/Carga/LeitorDeCargaDoVortice.cs"] = 732,
        ["src/Tracbel.Crm.Integracao/Carga/LeitorDeCargaDoVortice.Processo.cs"] = 983,
        ["src/Tracbel.Crm.Integracao/Protheus/LeitorDasOrdensDeServicoDoProtheus.cs"] = 607,
    };

    [Fact]
    public void Nenhum_arquivo_de_codigo_passa_do_limite_e_os_grandes_conhecidos_so_diminuem()
    {
        var raiz = ArquiteturaTestes.LocalizarRaizDoRepositorio();
        var problemas = new List<string>();

        foreach (var arquivo in Directory.EnumerateFiles(Path.Combine(raiz, "src"), "*.cs", SearchOption.AllDirectories))
        {
            var relativo = Path.GetRelativePath(raiz, arquivo).Replace('\\', '/');
            if (relativo.Contains("/bin/") || relativo.Contains("/obj/") || relativo.Contains("/Migrations/")) continue;

            var linhas = File.ReadAllLines(arquivo).Length;
            if (Conhecidos.TryGetValue(relativo, out var teto))
            {
                if (linhas > teto) problemas.Add($"{relativo}: {linhas} linhas, acima das {teto} de 03/10/2026");
                else if (linhas <= Limite) problemas.Add($"{relativo}: já está com {linhas} linhas — tire-o da lista");
            }
            else if (linhas > Limite)
            {
                problemas.Add($"{relativo}: {linhas} linhas, acima do limite de {Limite}");
            }
        }

        problemas.Should().BeEmpty();
    }
}
