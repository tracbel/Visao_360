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

    /// <summary>
    /// NO 5.1, COM <c>$ErrorActionPreference = 'Stop'</c>, O STDERR REDIRECIONADO DE UM EXECUTÁVEL VIRA
    /// EXCEÇÃO — mesmo mandado para <c>$null</c>. Foi o segundo defeito da primeira publicação pelo agente:
    /// <c>schtasks /Query ... *&gt; $null</c>, com a tarefa procurada já apagada, derrubou o passo 7 antes da
    /// primeira carga. Os here-strings ficam de fora: são os scripts das rotinas, que rodam em outro processo,
    /// com <c>'Continue'</c>.
    /// </summary>
    [Fact]
    public void Os_scripts_do_servidor_nao_redirecionam_o_stderr_de_executavel()
    {
        const string executavel = @"(?i)(\b(schtasks|icacls|robocopy|sqlcmd|netsh|dotnet)\b|\.exe\b)";
        const string stderrRedirecionado = @"(\s2>|\s\*>)";

        var violacoes =
            (from nome in RodamNoServidor
             from linha in SemHereStrings(SemComentarios(Ler(nome))).Split('\n')
             where Regex.IsMatch(linha, executavel + @"[^\r\n]*" + stderrRedirecionado)
             select $"{nome}: {linha.Trim()}").ToList();

        violacoes.Should().BeEmpty(
            "no powershell.exe 5.1, com $ErrorActionPreference = 'Stop', o stderr redirecionado de um executável " +
            "vira exceção; consulte por cmdlet (Get-ScheduledTask, Get-Service) ou baixe a preferência só ali");
    }

    /// <summary>
    /// O POWERSHELL NÃO DIFERENCIA MAIÚSCULA DE MINÚSCULA NO NOME DA VARIÁVEL. No passo 7 do
    /// <c>publicar-pacote.ps1</c>, <c>$registrar = &lt;caminho do script&gt;</c> sobrescreveu o parâmetro
    /// <c>$Registrar</c> — a função de registro do agente —, e a mensagem seguinte chamou o
    /// <c>registrar-rotinas.ps1</c> com a frase no lugar da pasta (21/09/2026). Reatribuir parâmetro, com
    /// qualquer caixa, não passa.
    /// </summary>
    [Fact]
    public void Os_scripts_do_servidor_nao_reatribuem_parametro()
    {
        var violacoes = new List<string>();

        foreach (var nome in RodamNoServidor)
        {
            var codigo = SemHereStrings(SemComentarios(Ler(nome)));
            var (parametros, corpo) = SepararParametros(codigo);
            parametros.Should().NotBeEmpty($"{nome} declara parâmetros, e o teste precisa enxergá-los");

            var atribuicoes = Regex.Matches(corpo, @"(?im)^\s*\$(\w+)\s*=(?!=)|\bforeach\s*\(\s*\$(\w+)\s+in\b");
            violacoes.AddRange(
                from Match a in atribuicoes
                let variavel = a.Groups[1].Success ? a.Groups[1].Value : a.Groups[2].Value
                where parametros.Contains(variavel)
                select $"{nome}: ${variavel} reatribui o parâmetro de mesmo nome");
        }

        violacoes.Should().BeEmpty(
            "no PowerShell $registrar e $Registrar são a mesma variável; dê outro nome à variável local");
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

    /// <summary>
    /// Os nomes do bloco <c>param( ... )</c> do script (sem diferenciar caixa) e o código que vem depois
    /// dele. Os parênteses são contados, porque atributo como <c>[Parameter(Mandatory = $true)]</c> tem os seus.
    /// </summary>
    private static (HashSet<string> Parametros, string Corpo) SepararParametros(string codigo)
    {
        var inicio = Regex.Match(codigo, @"(?i)\bparam\s*\(");
        if (!inicio.Success) return ([], codigo);

        var profundidade = 1;
        var i = inicio.Index + inicio.Length;
        for (; i < codigo.Length && profundidade > 0; i++)
        {
            if (codigo[i] == '(') profundidade++;
            else if (codigo[i] == ')') profundidade--;
        }

        var bloco = codigo[(inicio.Index + inicio.Length)..(i - 1)];
        var nomes = Regex.Matches(bloco, @"(?m)^\s*(?:\[[^\]]*(?:\([^)]*\))?[^\]]*\]\s*)*\$(\w+)")
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return (nomes, codigo[i..]);
    }

    /// <summary>Tira os here-strings (<c>@" ... "@</c> e <c>@' ... '@</c>): o texto deles roda em outro processo.</summary>
    private static string SemHereStrings(string codigo) =>
        Regex.Replace(codigo, @"(?ms)@[""']\s*$.*?^[""']@", "");
}
