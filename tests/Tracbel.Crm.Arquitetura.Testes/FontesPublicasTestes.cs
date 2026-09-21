using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// O CATÁLOGO DAS FONTES PÚBLICAS CONTRA O QUE RODA DE VERDADE (issue 77).
///
/// <para>O painel do administrador diz quando cada fonte devia ter rodado e quando roda de novo, e lê a última
/// rodada pelo nome do fluxo. As duas informações têm dono em outro lugar: o calendário é do
/// <c>scripts/deploy/registrar-rotinas.ps1</c>, e o nome do fluxo é da carga. Se divergirem, o painel mente em
/// silêncio — uma fonte "atrasada" que rodou, ou "sem registro" porque o nome mudou. Estes testes trocam o
/// silêncio por uma falha no build.</para>
/// </summary>
public sealed class FontesPublicasTestes
{
    private static string Raiz => ArquiteturaTestes.LocalizarRaizDoRepositorio();

    private static readonly Dictionary<string, int> Meses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["JAN"] = 1, ["FEB"] = 2, ["MAR"] = 3, ["APR"] = 4, ["MAY"] = 5, ["JUN"] = 6,
        ["JUL"] = 7, ["AUG"] = 8, ["SEP"] = 9, ["OCT"] = 10, ["NOV"] = 11, ["DEC"] = 12
    };

    private static string Padrao(string script, string parametro)
    {
        var achado = Regex.Match(script, $@"\${parametro}\s*=\s*'?(?<valor>[^'\r\n,]+)'?");
        achado.Success.Should().BeTrue($"o registrar-rotinas.ps1 declara ${parametro} com valor padrão");
        return achado.Groups["valor"].Value.Trim();
    }

    [Fact]
    public void O_calendario_do_painel_e_o_mesmo_do_registrar_rotinas()
    {
        var script = File.ReadAllText(Path.Combine(Raiz, "scripts", "deploy", "registrar-rotinas.ps1"));

        FontesPublicas.Anual.Tarefa.Should().Be(Padrao(script, "NomeTarefaAnual"));
        FontesPublicas.Anual.Mes.Should().Be(Meses[Padrao(script, "MesAnual")]);
        FontesPublicas.Anual.Dia.Should().Be(int.Parse(Padrao(script, "DiaAnual"), CultureInfo.InvariantCulture));
        FontesPublicas.Anual.Hora.Should().Be(TimeOnly.ParseExact(Padrao(script, "HoraAnual"), "HH:mm", CultureInfo.InvariantCulture));

        FontesPublicas.Mensal.Tarefa.Should().Be(Padrao(script, "NomeTarefaMensal"));
        FontesPublicas.Mensal.Dia.Should().Be(int.Parse(Padrao(script, "DiaMensal"), CultureInfo.InvariantCulture));
        FontesPublicas.Mensal.Hora.Should().Be(TimeOnly.ParseExact(Padrao(script, "HoraMensal"), "HH:mm", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Cada_fonte_do_painel_e_um_fluxo_que_a_carga_grava()
    {
        var codigoDaCarga = string.Join('\n', Directory
            .EnumerateFiles(Path.Combine(Raiz, "src", "Tracbel.Crm.Carga"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(File.ReadAllText));

        var ausentes = FontesPublicas.Todas.Where(f => !codigoDaCarga.Contains($"\"{f.Fluxo}\"", StringComparison.Ordinal)).ToList();

        ausentes.Select(f => f.Fluxo).Should().BeEmpty(
            "o painel lê a última rodada pelo nome do fluxo, e um nome que a carga não usa nunca terá rodada");
    }

    [Fact]
    public void Cada_rotina_do_catalogo_e_uma_das_duas_que_o_servidor_agenda()
    {
        FontesPublicas.Todas.Select(f => f.Rotina).Distinct().Should().BeEquivalentTo([FontesPublicas.Anual, FontesPublicas.Mensal]);
        FontesPublicas.Todas.Select(f => f.Fluxo).Should().OnlyHaveUniqueItems();
    }
}
