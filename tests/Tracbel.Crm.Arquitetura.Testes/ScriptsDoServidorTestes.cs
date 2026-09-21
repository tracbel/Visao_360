using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// OS SCRIPTS QUE RODAM NO SERVIDOR rodam no <c>powershell.exe</c> 5.1 da tarefa agendada, e não no
/// PowerShell 7 da estação de quem os escreve.
///
/// <para>Foi o defeito da primeira publicação pelo agente (21/09/2026): a prova de vida usava
/// <c>-SkipCertificateCheck</c>, que só existe no 7. Toda tentativa quebrava na hora, o <c>catch</c>
/// engolia o erro, e a API — que estava no ar — foi dada como morta duas vezes seguidas. Na estação,
/// no 7, o script funcionava; o erro só existia onde ele de fato roda.</para>
///
/// <para>A prova de vida também procurava um campo que a API não devolve (<c>banco = 'conectado'</c>).
/// O segundo teste amarra o script ao que <c>/saude/banco</c> responde de verdade.</para>
/// </summary>
[Trait("Categoria", "Arquitetura")]
public class ScriptsDoServidorTestes
{
    /// <summary>Os que o agente executa no próprio processo, no 5.1, como SYSTEM.</summary>
    private static readonly string[] RodamNoServidor =
        ["agente-de-publicacao.ps1", "publicar-pacote.ps1", "registrar-rotinas.ps1"];

    /// <summary>O que só existe do PowerShell 6 em diante e, no 5.1, quebra ou nem deixa o script abrir.</summary>
    private static readonly (string Padrao, string Motivo)[] SoNoPowerShell7 =
    [
        (@"-SkipCertificateCheck\b", "parâmetro do Invoke-RestMethod/Invoke-WebRequest que só existe no 7"),
        (@"-SkipHttpErrorCheck\b", "parâmetro do Invoke-RestMethod/Invoke-WebRequest que só existe no 7"),
        (@"-StatusCodeVariable\b", "parâmetro do Invoke-RestMethod que só existe no 7"),
        (@"-ResponseHeadersVariable\b", "parâmetro do Invoke-RestMethod que só existe no 7"),
        (@"-MaximumRetryCount\b|-RetryIntervalSec\b", "novas tentativas do Invoke-WebRequest, só no 6.1+"),
        (@"-Authentication\s+(Bearer|OAuth|Basic)\b", "autenticação embutida do Invoke-RestMethod, só no 6+"),
        (@"-AsHashtable\b", "ConvertFrom-Json -AsHashtable, só no 6+"),
        (@"ConvertFrom-Json[^\r\n|]*-Depth\b", "ConvertFrom-Json -Depth, só no 6.2+"),
        (@"-AsArray\b|-EnumsAsStrings\b", "parâmetros do ConvertTo-Json que só existem no 7"),
        (@"-AsByteStream\b", "Get-Content/Set-Content -AsByteStream, só no 6+ (no 5.1 é -Encoding Byte)"),
        (@"\butf8NoBOM\b", "codificação que só existe no 6+"),
        (@"ForEach-Object\s+-Parallel\b", "ForEach-Object -Parallel, só no 7"),
        (@"\?\?=?", "operador ?? do 7"),
        (@"\?\.", "operador ?. do 7"),
        (@"&&|\|\|", "encadeamento && / || do 7"),
    ];

    [Fact]
    public void Os_scripts_do_servidor_nao_usam_o_que_so_existe_no_PowerShell_7()
    {
        var violacoes =
            (from nome in RodamNoServidor
             let codigo = SemComentarios(Ler(nome))
             from regra in SoNoPowerShell7
             from Match achado in Regex.Matches(codigo, regra.Padrao)
             select $"{nome}: '{achado.Value}' — {regra.Motivo}").ToList();

        violacoes.Should().BeEmpty(
            "estes scripts rodam no powershell.exe 5.1 da tarefa agendada do servidor, e não no PowerShell 7");
    }

    [Fact]
    public void A_prova_de_vida_confere_os_campos_que_a_API_devolve()
    {
        // A ROTA devolve { Conectado, MigracoesPendentes }, que o ASP.NET serializa em camelCase.
        var programa = File.ReadAllText(Path.Combine(
            ArquiteturaTestes.LocalizarRaizDoRepositorio(), "src", "Tracbel.Crm.Api", "Program.cs"));
        programa.Should().Contain("app.MapGet(\"/saude/banco\"")
            .And.Contain("Conectado = conecta")
            .And.Contain("MigracoesPendentes = pendentes");

        // E O SCRIPT confere exatamente esses dois — o banco conectado e nenhuma migração para trás.
        var prova = SemComentarios(Ler("publicar-pacote.ps1"));
        prova.Should().Contain("$r.conectado -eq $true")
            .And.Contain("$r.migracoesPendentes");
    }

    private static string Ler(string nome) =>
        File.ReadAllText(Path.Combine(
            ArquiteturaTestes.LocalizarRaizDoRepositorio(), "scripts", "deploy", nome));

    /// <summary>
    /// Tira os blocos <c>&lt;# ... #&gt;</c> e o resto da linha depois de um <c>#</c> que começa comentário:
    /// o comentário que explica o <c>-SkipCertificateCheck</c> não é uso dele.
    /// </summary>
    private static string SemComentarios(string codigo)
    {
        var semBlocos = Regex.Replace(codigo, @"(?s)<#.*?#>", "");
        return Regex.Replace(semBlocos, @"(?m)(^|\s)#.*$", "$1");
    }
}
