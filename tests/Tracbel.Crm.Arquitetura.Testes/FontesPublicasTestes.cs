using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// OS CATÁLOGOS DAS FONTES, DAS ROTINAS E DAS CONEXÕES CONTRA O QUE RODA DE VERDADE (issues 77 e 136).
///
/// <para>O painel lê a última rodada pelo nome do fluxo; o orquestrador roda cada rotina pelos modos da carga; o botão
/// "Testar" consulta o endereço de cada fonte. As três informações têm dono em outro lugar — a carga, o
/// <c>registrar-rotinas.ps1</c> e os leitores de <c>Tracbel.Crm.Integracao</c>. Se divergirem, a tela mente em
/// silêncio. Estes testes trocam o silêncio por uma falha no build.</para>
/// </summary>
public sealed class FontesPublicasTestes
{
    private static string Raiz => ArquiteturaTestes.LocalizarRaizDoRepositorio();

    private static string CodigoDe(string projeto) => string.Join('\n', Directory
        .EnumerateFiles(Path.Combine(Raiz, "src", projeto), "*.cs", SearchOption.AllDirectories)
        .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        .Select(File.ReadAllText));

    [Fact]
    public void O_servidor_agenda_so_o_orquestrador_e_apaga_as_tarefas_antigas()
    {
        var script = File.ReadAllText(Path.Combine(Raiz, "scripts", "deploy", "registrar-rotinas.ps1"));

        script.Should().Contain("'TracbelCrmOrquestrador'").And.Contain("'/SC', 'MINUTE'").And.Contain("--orquestrar",
            "a agenda é do banco: a tarefa do Windows só acorda o orquestrador");
        script.Should().Contain("'TracbelCrmFontesPublicas', 'TracbelCrmPrecos', 'TracbelCrmPam'",
            "deixar as tarefas antigas faria a mesma carga rodar duas vezes");
        Regex.IsMatch(script, @"'/SC',\s*'MONTHLY'").Should().BeFalse("não há mais calendário escrito no script");
    }

    [Fact]
    public void Cada_fonte_do_painel_e_um_fluxo_que_a_carga_grava()
    {
        var codigoDaCarga = CodigoDe("Tracbel.Crm.Carga");

        var ausentes = FontesPublicas.Todas.Where(f => !codigoDaCarga.Contains($"\"{f.Fluxo}\"", StringComparison.Ordinal)).ToList();

        ausentes.Select(f => f.Fluxo).Should().BeEmpty(
            "o painel lê a última rodada pelo nome do fluxo, e um nome que a carga não usa nunca terá rodada");
    }

    [Fact]
    public void Cada_fonte_e_carregada_por_uma_rotina_do_catalogo()
    {
        FontesPublicas.Todas.Select(f => f.Rotina).Distinct()
            .Should().BeEquivalentTo([RotinasDoSistema.FontesAnuais, RotinasDoSistema.PrecosMensais]);
        FontesPublicas.Todas.Select(f => f.Fluxo).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Cada_modo_que_uma_rotina_roda_existe_na_carga()
    {
        var programa = File.ReadAllText(Path.Combine(Raiz, "src", "Tracbel.Crm.Carga", "Program.cs"));

        var ausentes = RotinasDoSistema.Todas.SelectMany(r => r.Modos)
            .Where(modo => !programa.Contains($"\"{modo}\"", StringComparison.Ordinal)).ToList();

        ausentes.Should().BeEmpty("o orquestrador roda a carga com esses modos na linha de comando");
    }

    [Fact]
    public void Cada_endereco_de_fonte_publica_e_o_que_o_leitor_dela_consulta()
    {
        // OS LEITORES MONTAM ALGUNS ENDEREÇOS COM {TABELA} E {SERIE}: cada trecho entre chaves vale qualquer segmento.
        var literais = Regex.Matches(CodigoDe("Tracbel.Crm.Integracao"), "\"(https://[^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .Select(l => new Regex("^" + Regex.Replace(Regex.Escape(l), @"\\\{[^}]*}", "[^/?&]+") + "$"))
            .ToList();

        var semLeitor = ConexoesDoSistema.Todas
            .Where(c => c.Tipo == TipoDeConexao.FontePublica)
            .Where(c => !literais.Any(l => l.IsMatch(c.Endereco!)))
            .Select(c => $"{c.Codigo}: {c.Endereco}")
            .ToList();

        semLeitor.Should().BeEmpty("o botão \"Testar\" precisa consultar o mesmo endereço que a carga lê");
    }

    [Fact]
    public void Cada_rotina_usa_conexoes_do_catalogo()
    {
        var codigos = ConexoesDoSistema.Todas.Select(c => c.Codigo).ToHashSet();

        RotinasDoSistema.Todas.SelectMany(r => r.Conexoes.Append(r.ConexaoExigida ?? r.Conexoes[0]))
            .Where(c => !codigos.Contains(c)).Should().BeEmpty();
    }
}
