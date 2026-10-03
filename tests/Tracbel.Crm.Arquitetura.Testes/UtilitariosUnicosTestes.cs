using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// OS UTILITÁRIOS ÚNICOS (documento 54 §3.5): a soma que ignora nulo, o arredondamento e a leitura do mês "aaaa-mm" moram
/// num lugar só. Cópia privada nova reprova — eram doze cópias em oito arquivos em 03/10/2026.
/// </summary>
public sealed class UtilitariosUnicosTestes
{
    [Fact]
    public void Ninguem_tem_copia_privada_da_soma_do_arredondamento_ou_da_leitura_do_mes()
    {
        var raiz = ArquiteturaTestes.LocalizarRaizDoRepositorio();
        var copia = new Regex(@"private static \S+ (SomaOuNulo|Arredondar|LerCompetencia)\(", RegexOptions.Compiled);

        var violacoes = Directory.EnumerateFiles(Path.Combine(raiz, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(a => !a.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(a => File.ReadLines(a).Where(l => copia.IsMatch(l)).Select(l => $"{Path.GetFileName(a)}: {l.Trim()}"))
            .ToList();

        violacoes.Should().BeEmpty("use Numeros (Domínio) e LeituraDeCompetencia (Aplicação)");
    }
}
