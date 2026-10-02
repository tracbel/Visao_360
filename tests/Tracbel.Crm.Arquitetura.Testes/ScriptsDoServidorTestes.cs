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
        ["agente-de-publicacao.ps1", "publicar-pacote.ps1", "registrar-rotinas.ps1", "limpeza-do-servidor.ps1"];

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

    /// <summary>
    /// AS CÓPIAS DO AGENTE NÃO SE ACUMULAM (28/09/2026). Cada publicação tirava uma cópia do banco e nenhuma saía: 95
    /// cópias, 25,5 GB em sete dias, e a publicação do e913ba2 parou no meio do BACKUP com o disco cheio. O teste prende
    /// as duas coisas que importam: a limpeza vem ANTES da cópia nova (é com o disco cheio que ela falha), e o padrão só
    /// casa as cópias do agente — as feitas à mão, com os nomes que estão hoje no servidor, ficam.
    /// </summary>
    [Fact]
    public void A_publicacao_apaga_as_copias_antigas_do_agente_antes_da_nova_e_so_as_dele()
    {
        var codigo = SemComentarios(Ler("publicar-pacote.ps1"));

        var limpeza = codigo.IndexOf("Remove-Item -LiteralPath $velha.FullName", StringComparison.Ordinal);
        var copiaNova = codigo.IndexOf("BACKUP DATABASE", StringComparison.Ordinal);
        limpeza.Should().BePositive("a publicação apaga as cópias antigas do próprio agente");
        limpeza.Should().BeLessThan(copiaNova, "é com o disco cheio que a cópia nova falha");

        const string padrao = "TracbelCrm-antes-de-*.bak";
        codigo.Should().Contain("\"{0}-antes-de-*.bak\" -f $cfg.banco", "o nome que a própria publicação dá à cópia");

        Casa(padrao, "TracbelCrm-antes-de-e913ba2-20260928-0035.bak").Should().BeTrue();
        foreach (var feitaAMao in new[]
                 {
                     "TracbelCrm-antes-da-publicacao-20260921-113156.bak", "TracbelCrm-antes-do-territorio-20260920-0926.bak",
                     "TracbelCrm-copia-antes-do-territorio-20260914.bak", "TracbelCrm-arquivo-antes-da-limpeza-20260915-095042.bak",
                     "TracbelCrm-antes-dos-clientes-20260924-1059.bak", "TracbelCrm-antes-da-estrutura-20260924-1208.bak"
                 })
            Casa(padrao, feitaAMao).Should().BeFalse($"{feitaAMao} foi feita à mão e não é do agente");
    }

    /// <summary>
    /// A LIMPEZA DO SERVIDOR SÓ AVISA DAS SESSÕES (decisão do Ricardo em 02/10/2026). Ela lê as sessões de Área de
    /// Trabalho Remota desconectadas e conta quanto de memória cada uma segura, mas não encerra, não desconecta e não para
    /// processo de ninguém: quem está desconectado pode ter trabalho aberto. O teste barra cada jeito de fazer isso.
    /// </summary>
    [Fact]
    public void A_limpeza_so_avisa_das_sessoes_e_nao_encerra_ninguem()
    {
        var codigo = SemComentarios(Ler("limpeza-do-servidor.ps1"));

        codigo.Should().Contain("WTSEnumerateSessionsW", "as sessões são lidas pela API do Windows, e não pela saída traduzida do quser");
        foreach (var proibido in new[] { "logoff", "WTSLogoffSession", "WTSDisconnectSession", "rwinsta", "reset session", "Stop-Process", "Stop-Service", "tsdiscon" })
            codigo.Should().NotContainEquivalentOf(proibido, $"a limpeza só avisa: '{proibido}' encerraria a sessão ou o trabalho de alguém");
    }

    /// <summary>
    /// O LOG DO BANCO: recuperação SIMPLES (decisão do Ricardo em 02/10/2026) e, acima do limite, encolhido só o arquivo de
    /// LOG. Encolher o banco inteiro (SHRINKDATABASE) encolheria também o de dados, fragmentando os índices.
    /// </summary>
    [Fact]
    public void A_limpeza_poe_o_banco_em_recuperacao_simples_e_encolhe_so_o_log()
    {
        var codigo = SemComentarios(Ler("limpeza-do-servidor.ps1"));

        codigo.Should().Contain("SET RECOVERY SIMPLE")
            .And.Contain("DBCC SHRINKFILE")
            .And.Contain("WHERE type_desc = 'LOG'", "o arquivo encolhido é o de log, procurado pelo tipo");
        codigo.Should().NotContainEquivalentOf("SHRINKDATABASE", "encolher o banco inteiro mexeria no arquivo de dados");
        codigo.Should().NotContainEquivalentOf("RECOVERY FULL", "a decisão é a recuperação simples");
    }

    /// <summary>
    /// AS CÓPIAS QUE A LIMPEZA APAGA SÃO SÓ AS DO AGENTE, pelo mesmo nome que a publicação dá — a rede de segurança das
    /// duas vezes em que o disco encheu (28/09 e 30/09/2026). As feitas à mão ficam.
    /// </summary>
    [Fact]
    public void A_limpeza_apaga_so_as_copias_antigas_do_agente()
    {
        var codigo = SemComentarios(Ler("limpeza-do-servidor.ps1"));

        codigo.Should().Contain("\"{0}-antes-de-*.bak\" -f $cfg.banco", "o nome que a própria publicação dá à cópia");
        codigo.Should().Contain("Select-Object -Skip $CopiasDoAgenteMantidas", "as mais recentes ficam");
        Regex.Matches(codigo, @"Remove-Item\b").Count.Should().Be(2, "só os logs antigos da própria limpeza e as cópias antigas do agente saem");
    }

    /// <summary>
    /// A LIMPEZA CHEGA AO SERVIDOR PELO CAMINHO QUE JÁ EXISTE: o <c>-SoAtualizarOsScripts</c>, que o Ricardo roda a cada
    /// correção do agente, copia o script e registra a tarefa — sem isso, ela ficaria no repositório e nunca rodaria lá.
    /// </summary>
    [Fact]
    public void O_instalador_leva_a_limpeza_e_registra_a_tarefa_tambem_no_so_atualizar_os_scripts()
    {
        var instalador = SemComentarios(Ler("instalar-agente-de-publicacao.ps1"));

        var soScripts = instalador.IndexOf("if ($SoAtualizarOsScripts)", StringComparison.Ordinal);
        var fimDoSoScripts = instalador.IndexOf("return", soScripts, StringComparison.Ordinal);
        soScripts.Should().BePositive();
        var bloco = instalador[soScripts..fimDoSoScripts];

        bloco.Should().Contain("Copy-Item (Join-Path $PSScriptRoot 'limpeza-do-servidor.ps1')")
            .And.Contain("RegistrarALimpeza");
        instalador.Should().Contain("'/SC', 'DAILY'")
            .And.Contain("'/RU', 'SYSTEM'")
            .And.Contain("[string] $NomeDaLimpeza = 'TracbelCrmLimpeza'");
    }

    /// <summary>O curinga do <c>Get-ChildItem -Filter</c> (<c>*</c>) como expressão regular, sem diferenciar caixa.</summary>
    private static bool Casa(string curinga, string nome) =>
        Regex.IsMatch(nome, "^" + Regex.Escape(curinga).Replace(@"\*", ".*") + "$", RegexOptions.IgnoreCase);

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
