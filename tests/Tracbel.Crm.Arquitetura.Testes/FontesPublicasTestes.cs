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
    /// <summary>
    /// O FRONT NÃO TEM LISTA FIXA DE CULTURA (issue 165).
    ///
    /// <para>Até aqui o painel de preços tinha <c>['CAFE', 'SOJA', 'MILHO'…]</c> e o de custos tinha a mesma
    /// lista escrita de outro jeito: cultura nova exigia publicação, e as duas podiam divergir. Agora as
    /// telas pedem o catálogo. Este teste impede a lista de voltar.</para>
    ///
    /// <para><b>Boi e leite continuam permitidos</b>: não são cultura de lavoura, são produtos de preço que
    /// o texto-base cita e que o catálogo não guarda.</para>
    ///
    /// <para><b>A regra vale para TELA, e não para amostra.</b> Os dois males que ela evita — cultura nova
    /// exigindo publicação, e duas telas discordando sobre quais são as culturas — pressupõem código que
    /// vai ao ar. Uma amostra de teste precisa de nome de cultura escrito: é o dado de mentira dela, e não
    /// tem catálogo de onde puxar. Por isso saem daqui os arquivos de teste e as duas pastas de amostra,
    /// <c>src/testes/</c> e <c>src/dev/</c> — esta última é o harness visual (fase T4.5), que o
    /// <c>npm run visual:conferir-pacote</c> prova não chegar ao <c>dist</c>.</para>
    /// </summary>
    [Fact]
    public void O_front_nao_tem_lista_fixa_de_cultura()
    {
        var raizDoFront = Path.Combine(Raiz, "src", "Tracbel.Crm.Web", "src");

        // As pastas que existem para guardar dado de mentira. Comparadas com separador dos dois lados
        // para `src/dev` não casar com um `src/devolucoes` que apareça um dia.
        string[] pastasDeAmostra =
        [
            $"{Path.DirectorySeparatorChar}testes{Path.DirectorySeparatorChar}",
            $"{Path.DirectorySeparatorChar}dev{Path.DirectorySeparatorChar}",
        ];

        var telas = Directory
            .EnumerateFiles(raizDoFront, "*.tsx", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(raizDoFront, "*.ts", SearchOption.AllDirectories))
            .Where(f => !f.EndsWith(".teste.ts", StringComparison.Ordinal) && !f.EndsWith(".teste.tsx", StringComparison.Ordinal))
            .Where(f => !pastasDeAmostra.Any(p => f.Contains(p, StringComparison.Ordinal)));

        // Uma lista fixa é um vetor de texto com DUAS ou mais culturas do catálogo seguidas — é isso que
        // caracteriza a lista, e não a palavra "café" aparecer numa frase ou num comentário.
        var culturas = string.Join("|", CatalogoSemeado.Culturas.Select(c => c.Nome.ToUpperInvariant())
            .Concat(["CAFE", "CANA DE AÇÚCAR", "CANA DE ACUCAR", "CAFÉ ARÁBICA"]));
        var padrao = new Regex($@"'({culturas})'\s*,\s*'({culturas})'", RegexOptions.IgnoreCase);

        // OS COMENTÁRIOS SAEM ANTES. O comentário que explica por que a lista saiu daqui cita a lista —
        // e um teste que falha por causa da própria explicação ensina a não explicar.
        var comLista = telas
            .Where(f => padrao.IsMatch(SemComentarios(File.ReadAllText(f))))
            .Select(Path.GetFileName)
            .ToList();

        comLista.Should().BeEmpty(
            "cultura vem do catálogo (issue 165): uma lista escrita na tela faz cultura nova exigir publicação, " +
            "e faz duas telas discordarem sobre quais são as culturas");
    }

    /// <summary>
    /// A INTELIGÊNCIA DE MERCADO NÃO LÊ `public/dados` (issue 169).
    ///
    /// <para><b>O que este teste impede de voltar:</b> <c>public/dados/mercado-pracas.json</c> tinha quatro
    /// praças fictícias — MT Norte, MT Sul, GO e BA —, com "total de mercado", "vendemos" e "indicamos
    /// perdida" inventados, e ela continuava no pacote publicado. Nada a carregava, o que é pior e não
    /// melhor: dado de mentira parado no ar é dado de mentira esperando alguém usá-lo.</para>
    ///
    /// <para><b>A regra:</b> `public/dados` é o seed do protótipo, e as telas que ainda o leem estão
    /// listadas na issue 191. Mercado e Território leem a <b>API</b> — é de lá que vem o número do IBGE e
    /// do CRM. Um <c>useDados</c> numa dessas pastas seria a volta do dado fictício por outra porta.</para>
    /// </summary>
    [Fact]
    public void Mercado_e_territorio_nao_leem_o_seed_do_prototipo()
    {
        var raizDoFront = Path.Combine(Raiz, "src", "Tracbel.Crm.Web", "src");

        string[] pastasDoMercado =
        [
            Path.Combine("componentes", "mercado"),
            Path.Combine("componentes", "territorio"),
            Path.Combine("componentes", "dashboard"),
        ];

        var daInteligenciaDeMercado = Directory
            .EnumerateFiles(raizDoFront, "*.tsx", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(raizDoFront, "*.ts", SearchOption.AllDirectories))
            .Where(f => pastasDoMercado.Any(p => f.Contains(p, StringComparison.Ordinal))
                        || f.EndsWith("IndicadoresGeograficos.tsx", StringComparison.Ordinal));

        // `useDados` e `carregar` são o par que lê `public/dados`. Comentários saem antes, pelo mesmo motivo
        // do teste acima: a explicação de por que o seed saiu daqui cita o seed.
        var leitor = new Regex(@"\buseDados\b|from\s+'[^']*dados/carregar'", RegexOptions.IgnoreCase);

        var comSeed = daInteligenciaDeMercado
            .Where(f => leitor.IsMatch(SemComentarios(File.ReadAllText(f))))
            .Select(Path.GetFileName)
            .ToList();

        comSeed.Should().BeEmpty(
            "Mercado e Território leem a API, não o seed do protótipo (issue 169): o número que a diretoria " +
            "olha vem do IBGE e do CRM, e `public/dados` é dado ilustrativo");
    }

    /// <summary>O arquivo das praças fictícias não volta, nem no manifesto (issue 169).</summary>
    [Fact]
    public void O_seed_nao_tem_mais_as_pracas_ficticias_de_mercado()
    {
        var seed = Path.Combine(Raiz, "src", "Tracbel.Crm.Web", "public", "dados");

        File.Exists(Path.Combine(seed, "mercado-pracas.json")).Should().BeFalse(
            "as quatro praças eram de MT, GO e BA — estados em que a Tracbel Agro não atua — e os números " +
            "eram inventados");

        File.ReadAllText(Path.Combine(seed, "manifesto.json"))
            .Should().NotContain("mercado-pracas", "o manifesto lista o que existe; entrada órfã é promessa de arquivo");
    }

    /// <summary>
    /// A TELA NÃO CITA DOCUMENTO INTERNO.
    ///
    /// <para><b>O defeito que isto impede de voltar:</b> cinco textos que o usuário lê terminavam com
    /// "(documento 32, §4.6.1)", "(documento 25, §5)", "(documento 05 §3)". O <c>§</c> lido por quem não
    /// conhece a convenção parece defeito de codificação — foi assim que o Ricardo o reportou —, e o número
    /// do documento não diz nada a quem está olhando um indicador.</para>
    ///
    /// <para><b>A rastreabilidade não se perde:</b> a referência continua no comentário do código, que é
    /// onde ela serve. Por isso o teste olha o código SEM os comentários.</para>
    ///
    /// <para><b>O que continua permitido:</b> citar a <b>issue</b> que destrava um número. "Precisa da
    /// issue 69" é acionável para quem lê — diz que existe trabalho em andamento e qual —, enquanto
    /// "documento 25, §5" é endereço de arquivo que ninguém fora daqui consegue abrir.</para>
    /// </summary>
    [Fact]
    public void Nenhum_texto_de_tela_cita_documento_interno()
    {
        var raizDoFront = Path.Combine(Raiz, "src", "Tracbel.Crm.Web", "src");

        var telas = Directory
            .EnumerateFiles(raizDoFront, "*.tsx", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(raizDoFront, "*.ts", SearchOption.AllDirectories))
            .Where(f => !f.EndsWith(".teste.ts", StringComparison.Ordinal) && !f.EndsWith(".teste.tsx", StringComparison.Ordinal));

        // `§` em qualquer lugar fora de comentário, ou "documento N" / "doc N" escritos no texto.
        var citacao = new Regex(@"§|\bdocumento\s+\d|\bdoc\s+\d", RegexOptions.IgnoreCase);

        var citando = telas
            .Where(f => citacao.IsMatch(SemComentarios(File.ReadAllText(f))))
            .Select(Path.GetFileName)
            .ToList();

        citando.Should().BeEmpty(
            "o texto que o usuário lê não cita documento interno nem usa o símbolo de seção: a referência " +
            "mora no comentário do código, e o que vai para a tela é a issue que destrava o número");
    }

    /// <summary>Tira comentários de bloco e de linha, para o teste olhar o código e não a explicação dele.</summary>
    private static string SemComentarios(string codigo) =>
        Regex.Replace(Regex.Replace(codigo, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\n]*", "");
}