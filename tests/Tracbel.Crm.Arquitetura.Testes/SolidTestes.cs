using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NetArchTest.Rules;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// Testes dos princípios SOLID.
///
/// Cada teste aqui protege UM princípio do documento
/// <c>docs/projeto/22-SOLID-E-PADROES.md</c> e cita a seção exata que verifica — os números
/// de seção daquele documento são, portanto, contrato com este arquivo: não renumere uma
/// seção sem atualizar as mensagens de falha aqui, e vice-versa (mesma disciplina do
/// documento 14 com a pasta <c>Banco/</c>).
///
/// A regra que os une: <b>princípio sem teste é recomendação, não padrão</b>. Os quatro
/// testes de <see cref="ArquiteturaTestes"/> já cobriam a regra de dependência, a quarentena
/// do vocabulário do Vórtice, o encapsulamento de entidade e a imutabilidade de evento.
/// Estes nove fecham o que faltava.
/// </summary>
[Trait("Categoria", "Solid")]
public class SolidTestes
{
    private static readonly Assembly Dominio = typeof(Tracbel.Crm.Dominio.Comercial.Cliente).Assembly;
    private static readonly Assembly Integracao = typeof(Integracao.Saneamento.SanitizadorLeadExterno).Assembly;

    // =========================================================================================
    // Inversão de dependência — documento 22, seção 7
    // =========================================================================================

    /// <summary>
    /// As setas de referência entre projetos apontam sempre para dentro.
    ///
    /// Este teste olha o <c>.csproj</c>, não os tipos: é a barreira mais barata e mais dura
    /// que existe, porque uma referência de projeto errada quebra o build de todo mundo antes
    /// de qualquer código ser escrito. O teste de tipos
    /// (<see cref="ArquiteturaTestes.Dominio_nao_depende_de_infraestrutura_nem_de_banco"/>)
    /// pega o vazamento por pacote NuGet; este pega o vazamento por camada.
    ///
    /// [V] No Vórtice não existe camada: a mesma regra de negócio mora no p-code Gupta
    /// (28 executáveis sem descompilador público), em 196 SQLs gravados dentro de
    /// <c>GE_ObjDinamico</c> e em 70 das 85 procedures que escrevem em tabela. Não há
    /// ferramenta que enxergue essa dependência — e é por isso que ninguém consegue estimar
    /// o efeito de mudar nada.
    /// </summary>
    [Fact]
    public void As_referencias_de_projeto_apontam_sempre_para_dentro()
    {
        // O grafo permitido, na íntegra. Acrescentar uma seta aqui é uma decisão de
        // arquitetura registrada em PR, nunca um efeito colateral de "precisei de um tipo".
        var esperado = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Tracbel.Crm.Dominio"] = [],
            ["Tracbel.Crm.Aplicacao"] = ["Tracbel.Crm.Dominio"],
            ["Tracbel.Crm.Integracao"] = ["Tracbel.Crm.Dominio"],
            ["Tracbel.Crm.Infraestrutura"] = ["Tracbel.Crm.Aplicacao", "Tracbel.Crm.Dominio"],

            // A Api referencia Integracao desde 04/09/2026, com a construção da ponte de leitura
            // do Vórtice (documento 23). A seta é LEGÍTIMA e aponta para dentro: Integracao
            // depende só do Domínio, e a Api é a RAIZ DE COMPOSIÇÃO — o único lugar autorizado a
            // conhecer adaptadores, porque é ele que os liga às portas no boot. É exatamente a
            // mesma razão pela qual a Api já referenciava Infraestrutura.
            //
            // O que continua proibido, e o teste seguinte vigia: Aplicacao conhecer Integracao.
            // O caso de uso conhece IPonteDeLeituraDoVortice, declarada em Dominio/Portas, e
            // nunca o adaptador.
            ["Tracbel.Crm.Api"] =
                ["Tracbel.Crm.Aplicacao", "Tracbel.Crm.Infraestrutura", "Tracbel.Crm.Integracao"],

            // A carga de dados do sistema legado (documento 24) é a SEGUNDA RAIZ DE COMPOSIÇÃO da
            // solução, e por isso tem o mesmo direito da Api: conhecer adaptadores para ligá-los
            // no boot — o CrmDbContext de um lado, o leitor do legado do outro. As duas setas
            // apontam para dentro, e nenhuma delas é Aplicacao: a carga não passa por caso de uso
            // porque não é uma operação de usuário; ela fala com o domínio e com o repositório
            // direto, dentro de um contexto de acesso declarado como serviço de sistema.
            ["Tracbel.Crm.Carga"] = ["Tracbel.Crm.Infraestrutura", "Tracbel.Crm.Integracao"]
        };

        var raiz = ArquiteturaTestes.LocalizarRaizDoRepositorio();
        var violacoes = new List<string>();

        foreach (var (projeto, referenciasPermitidas) in esperado)
        {
            var caminho = Path.Combine(raiz, "src", projeto, $"{projeto}.csproj");
            if (!File.Exists(caminho))
            {
                violacoes.Add($"{projeto}: o .csproj não existe em src/{projeto}/.");
                continue;
            }

            var reais = ReferenciasDeProjeto(caminho);

            foreach (var extra in reais.Except(referenciasPermitidas, StringComparer.Ordinal).Order())
                violacoes.Add($"{projeto} referencia {extra}, e não deveria.");

            foreach (var faltante in referenciasPermitidas.Except(reais, StringComparer.Ordinal).Order())
                violacoes.Add($"{projeto} deixou de referenciar {faltante}.");
        }

        violacoes.Should().BeEmpty(
            "o grafo de referências é a inversão de dependência na sua forma mais barata: " +
            "Domínio não referencia ninguém, Infraestrutura implementa as portas que o Domínio " +
            "declara, e a injeção liga os dois no boot. Se você precisou de uma seta nova, " +
            "quase sempre a peça está na camada errada — documento 22, seção 7.2");
    }

    /// <summary>
    /// Nenhum caso de uso conhece tipo concreto de infraestrutura — só porta.
    ///
    /// HONESTIDADE SOBRE O ALCANCE DE HOJE: <c>Tracbel.Crm.Aplicacao</c> ainda não tem
    /// nenhum tipo (a fase 1 do documento 06 é que os cria), então este teste hoje não
    /// reprova nada. Ele está aqui armado, e não depois, porque o primeiro caso de uso é
    /// exatamente o momento em que a tentação de injetar o <c>CrmDbContext</c> aparece —
    /// e é mais fácil o teste já existir do que lembrar de escrevê-lo naquele PR.
    /// A garantia que vale HOJE é a do teste anterior, sobre o <c>.csproj</c>.
    /// </summary>
    [Fact]
    public void Nenhum_tipo_da_aplicacao_depende_de_tipo_concreto_de_infraestrutura()
    {
        var aplicacao = Assembly.Load(new AssemblyName("Tracbel.Crm.Aplicacao"));

        var resultado = Types.InAssembly(aplicacao)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Tracbel.Crm.Infraestrutura",
                "Tracbel.Crm.Integracao",
                "Tracbel.Crm.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Microsoft.Data.SqlClient",
                "System.Data.SqlClient")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            "um caso de uso depende da PORTA declarada no domínio, nunca do adaptador. " +
            "Injete a interface (IRepositorioRegras, IRelogio, ...) e deixe o registro no " +
            "Program.cs decidir a implementação. Violações: {0} — documento 22, seção 7.3",
            string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    /// <summary>
    /// Toda porta do domínio mora em <c>Tracbel.Crm.Dominio.Portas</c> — ou está na lista
    /// de exceções abaixo, com justificativa escrita.
    ///
    /// Não é arrumação: uma porta é o que o domínio EXIGE do mundo, e é a lista dessas
    /// exigências que um dev novo precisa conseguir ler num lugar só para saber o que a
    /// infraestrutura tem de entregar.
    /// </summary>
    [Fact]
    public void Toda_porta_do_dominio_mora_no_namespace_Portas_ou_tem_justificativa_registrada()
    {
        // Exceções COM JUSTIFICATIVA. Acrescentar uma linha aqui é uma decisão consciente;
        // esquecer de acrescentá-la faz o teste falhar, que é o ponto.
        var excecoes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["IEventoDominio"] =
                "marcador, não porta: não exige nada do mundo externo. Mora em Comum/ junto " +
                "das entidades que o produzem.",
            ["IRelogio"] =
                "porta, mas primitiva de teste usada por todo o domínio; mora em Comum/ " +
                "junto de RelogioDoSistema. Decisão registrada no documento 22, seção 11.",
            ["IEfeitoRegra"] =
                "porta, mas é o ponto de extensão DO MOTOR de workflow; colocada junto do " +
                "motor que a consome. Decisão registrada no documento 22, seção 11."
        };

        var forasteiras = Dominio.GetTypes()
            .Where(t => t.IsInterface && t.IsPublic)
            .Where(t => t.Namespace != "Tracbel.Crm.Dominio.Portas")
            .Where(t => !excecoes.ContainsKey(t.Name))
            .Select(t => $"{t.Name} (em {t.Namespace})")
            .Order()
            .ToList();

        forasteiras.Should().BeEmpty(
            "porta nova vai para Tracbel.Crm.Dominio.Portas. Se ela realmente pertence a " +
            "outro lugar, registre a exceção COM justificativa no dicionário deste teste e " +
            "na seção 11 do documento 22 — documento 22, seção 7.4");
    }

    /// <summary>
    /// Todo método assíncrono público recebe <see cref="CancellationToken"/>.
    ///
    /// É a convenção 6 do documento 03, seção 9, e ela é de inversão de dependência: quem
    /// chama é que sabe quando desistir. Sem o token, o cancelamento vira responsabilidade
    /// da implementação — e cada implementação decide diferente.
    ///
    /// [V] O sincronismo do Vórtice Mobile não tem como ser interrompido: uma linha ruim
    /// aborta a carga inteira (5.302 vínculos quebrados) e não há caminho para parar antes.
    /// </summary>
    [Fact]
    public void Todo_metodo_assincrono_publico_recebe_CancellationToken()
    {
        Assembly[] camadas = [Dominio, Integracao];

        var semToken =
            (from assembly in camadas
             from tipo in assembly.GetTypes()
             where tipo.IsPublic
             from metodo in tipo.GetMethods(
                 BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
             where !metodo.IsSpecialName
             where RetornaTarefa(metodo.ReturnType)
             where metodo.GetParameters().All(p => p.ParameterType != typeof(CancellationToken))
             select $"{tipo.Name}.{metodo.Name}").Distinct().Order().ToList();

        semToken.Should().BeEmpty(
            "acrescente 'CancellationToken ct' ao final da assinatura e propague até o fim — " +
            "quem inicia a operação é quem decide desistir dela (documento 03, seção 9, " +
            "convenção 6) — documento 22, seção 7.5");
    }

    // =========================================================================================
    // Segregação de interface — documento 22, seção 6
    // =========================================================================================

    /// <summary>
    /// Nenhuma porta obriga quem implementa a escrever método que não usa.
    ///
    /// O limite é QUATRO membros. Não é número mágico: é o dobro da maior porta que temos
    /// hoje (<c>IHierarquiaVendas</c>, <c>ICalendarioUtil</c> e <c>IEstrategiaDestinatario</c>,
    /// com dois cada), o que dá margem para crescer sem esconder o dia em que uma porta
    /// virou um serviço.
    ///
    /// [V] O contra-exemplo do legado é <c>IV_AcaoAuto</c>: 31 colunas, das quais 9 estão
    /// 100% nulas. Toda regra paga o preço das colunas que ela não usa — que é exatamente
    /// o que a segregação de interface evita, uma camada acima.
    /// </summary>
    [Fact]
    public void Nenhuma_porta_do_dominio_passa_de_quatro_membros()
    {
        const int LimiteDeMembros = 4;

        // Portas gordas COM JUSTIFICATIVA aceita em PR. Vazio hoje, de propósito.
        var excecoes = new Dictionary<string, string>(StringComparer.Ordinal);

        var gordas =
            (from tipo in Dominio.GetTypes()
             where tipo.IsInterface && tipo.IsPublic
             where !excecoes.ContainsKey(tipo.Name)
             let membros = ContarMembros(tipo)
             where membros > LimiteDeMembros
             select $"{tipo.Name}: {membros} membros").Order().ToList();

        gordas.Should().BeEmpty(
            $"porta com mais de {LimiteDeMembros} membros quase sempre é duas portas coladas: " +
            "quebre-a em interfaces menores, ou registre a exceção com justificativa no " +
            "dicionário deste teste — documento 22, seção 6.2");
    }

    // =========================================================================================
    // Substituição de Liskov — documento 22, seção 5
    // =========================================================================================

    /// <summary>
    /// Nenhuma implementação lança <see cref="NotImplementedException"/>.
    ///
    /// É a violação de Liskov mais comum e mais barata de detectar: o tipo diz que cumpre o
    /// contrato, o compilador acredita, e quem chama descobre em produção que não cumpre.
    /// Uma implementação que não sabe fazer algo devolve <c>Resultado&lt;T&gt;.Falha</c> ou
    /// <c>ResultadoEfeito.NaoAplicado</c> — que é informação registrada, não surpresa.
    ///
    /// [V] É a lição de <c>IVS_Carteira.SeqUrSuperv</c>: a coluna existe, o código a
    /// consulta, e ela é NULL em 655 de 655 carteiras. Uma rota de visibilidade inteira que
    /// o sistema declara ter e não tem.
    /// </summary>
    [Fact]
    public void Nenhuma_implementacao_lanca_NotImplementedException()
    {
        var violacoes = ArquivosDeCodigoFonte()
            .Where(arquivo => File.ReadAllText(arquivo).Contains("NotImplementedException", StringComparison.Ordinal))
            .Select(arquivo => Path.GetFileName(arquivo))
            .Order()
            .ToList();

        violacoes.Should().BeEmpty(
            "quem implementa uma porta cumpre o contrato inteiro. Se um caminho ainda não " +
            "existe, devolva Resultado<T>.Falha ou ResultadoEfeito.NaoAplicado com o motivo " +
            "em português — nunca uma exceção que só aparece em produção — " +
            "documento 22, seção 5.2");
    }

    /// <summary>
    /// Os tipos de valor de <c>Comum/</c> são imutáveis e comparados por VALOR.
    ///
    /// É o que faz um <c>CpfCnpj</c> poder substituir outro em qualquer lugar sem que o
    /// comportamento mude — Liskov aplicado ao tipo mais usado do sistema. Quem tem um
    /// <c>Telefone</c> em mãos tem um telefone válido, e dois telefones com o mesmo número
    /// SÃO o mesmo telefone.
    ///
    /// [V] Sem isso, o legado mede 116 CPFs repetidos entre clientes distintos e uma fila de
    /// deduplicação com 124 mil pares abandonados desde 2018: o documento nunca foi um tipo,
    /// sempre foi texto que cada tela comparava do seu jeito.
    ///
    /// Genéricos ficam de fora porque <c>Resultado&lt;T&gt;</c> é um envelope de fluxo, não
    /// um valor de negócio: comparar dois resultados por valor não significa nada.
    /// </summary>
    [Fact]
    public void Tipos_de_valor_do_dominio_sao_imutaveis_e_comparados_por_valor()
    {
        var tiposDeValor = Dominio.GetTypes()
            .Where(t => t.IsPublic && t.IsValueType && !t.IsEnum && !t.IsGenericType)
            .Where(t => t.Namespace == "Tracbel.Crm.Dominio.Comum")
            .ToList();

        tiposDeValor.Should().NotBeEmpty(
            "o domínio precisa ter tipos de valor — são eles que impedem dado inválido de " +
            "existir (documento 22, seção 5.3)");

        var violacoes = new List<string>();

        foreach (var tipo in tiposDeValor)
        {
            // `readonly struct`: o compilador marca com IsReadOnlyAttribute.
            var ehReadonly = tipo.GetCustomAttributes()
                .Any(a => a.GetType().Name == "IsReadOnlyAttribute");

            if (!ehReadonly)
                violacoes.Add($"{tipo.Name}: não é 'readonly struct' — pode ser mutado depois de criado.");

            violacoes.AddRange(
                from prop in tipo.GetProperties()
                let setter = prop.GetSetMethod(nonPublic: false)
                where setter is not null
                select $"{tipo.Name}.{prop.Name}: tem setter público.");

            // record struct gera op_Equality/op_Inequality; struct comum, não.
            if (tipo.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static) is null)
                violacoes.Add($"{tipo.Name}: não define igualdade por valor (use 'readonly record struct').");
        }

        violacoes.Should().BeEmpty(
            "tipo de valor é 'readonly record struct' com fábrica Criar/TentarCriar e nenhum " +
            "setter: quem o recebe recebe um valor já válido, e dois valores iguais são o " +
            "mesmo valor — documento 22, seção 5.3");
    }

    // =========================================================================================
    // Responsabilidade única — documento 22, seção 3
    // =========================================================================================

    /// <summary>
    /// Entidade não expõe coleção mutável.
    ///
    /// Complementa <see cref="ArquiteturaTestes.Entidades_nao_expoem_setter_publico"/>: de
    /// nada adianta a propriedade não ter setter se o que ela devolve é uma <c>List</c> que
    /// qualquer um pode limpar. Coleção sai como <c>IReadOnlyCollection</c>, e o que entra
    /// nela entra por método com regra — como já faz <c>EntidadeBase.Eventos</c>.
    /// </summary>
    [Fact]
    public void Entidades_nao_expoem_colecao_mutavel()
    {
        Type[] tiposMutaveis =
        [
            typeof(List<>), typeof(ICollection<>), typeof(IList<>),
            typeof(HashSet<>), typeof(ISet<>), typeof(Dictionary<,>), typeof(IDictionary<,>)
        ];

        var entidades = Types.InAssembly(Dominio)
            .That().Inherit(typeof(EntidadeBase))
            .GetTypes();

        var expostas =
            (from tipo in entidades
             from prop in tipo.GetProperties()
             where prop.PropertyType.IsGenericType
             where tiposMutaveis.Contains(prop.PropertyType.GetGenericTypeDefinition())
             select $"{tipo.Name}.{prop.Name} ({prop.PropertyType.Name})").Order().ToList();

        expostas.Should().BeEmpty(
            "troque o tipo da propriedade por IReadOnlyCollection<T>/IReadOnlyList<T> e " +
            "guarde a lista num campo privado; quem quiser acrescentar item usa um método " +
            "da entidade, que aplica a regra — documento 22, seção 3.3");
    }

    /// <summary>
    /// A API não decide regra de negócio.
    ///
    /// ALCANCE DECLARADO: este teste NÃO mede "quanta regra existe num controlador" — não
    /// existe heurística honesta para isso (ver documento 22, seção 11.4). Ele barra os três
    /// sintomas estruturais que são inequívocos: a API declarar uma violação de regra de
    /// negócio, instanciar o motor de workflow ou o autorizador à mão, ou registrar evento
    /// de domínio. Cada um deles significa que a decisão saiu da camada que a testa.
    ///
    /// [V] É a diferença medida contra o legado, onde a regra mora ao mesmo tempo no p-code
    /// do cliente Gupta, em 196 SQLs dentro de <c>GE_ObjDinamico</c> e em 70 das 85
    /// procedures — três lugares, nenhum deles testável.
    /// </summary>
    [Fact]
    public void A_api_nao_decide_regra_de_negocio()
    {
        (string Termo, string Correcao)[] proibidos =
        [
            ("RegraDeNegocioViolada",
                "quem declara violação de regra é a entidade; a API traduz Resultado<T> em 409/422"),
            ("new MotorWorkflow(",
                "o motor é resolvido por injeção e orquestrado pelo caso de uso, não montado no endpoint"),
            ("new Autorizador(",
                "a decisão de acesso é do domínio; a API só carrega o atributo de permissão"),
            (".RegistrarEvento(",
                "evento de domínio nasce dentro da entidade, junto do fato que o originou")
        ];

        var raiz = ArquiteturaTestes.LocalizarRaizDoRepositorio();
        var pastaApi = Path.Combine(raiz, "src", "Tracbel.Crm.Api");

        var violacoes = new List<string>();

        foreach (var arquivo in ArquivosDeCodigoFonte(pastaApi))
        {
            var codigo = SemComentarios(File.ReadAllText(arquivo));

            violacoes.AddRange(
                from proibido in proibidos
                where codigo.Contains(proibido.Termo, StringComparison.Ordinal)
                select $"{Path.GetFileName(arquivo)}: '{proibido.Termo}' — {proibido.Correcao}");
        }

        violacoes.Should().BeEmpty(
            "o endpoint recebe, delega e traduz o resultado em código HTTP. Decisão de " +
            "negócio mora na entidade; orquestração, no caso de uso — documento 22, seção 3.4");
    }

    // =========================================================================================
    // Auxiliares
    // =========================================================================================

    /// <summary>Nomes dos projetos referenciados por um <c>.csproj</c>.</summary>
    private static string[] ReferenciasDeProjeto(string caminhoDoCsproj) =>
        Regex.Matches(
                File.ReadAllText(caminhoDoCsproj),
                @"<ProjectReference\s+Include\s*=\s*""(?<caminho>[^""]+)""")
            .Select(m => Path.GetFileNameWithoutExtension(m.Groups["caminho"].Value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    /// <summary>Todo <c>.cs</c> de <c>src/</c> que é código nosso: sem <c>obj/</c>, sem migração gerada.</summary>
    private static IEnumerable<string> ArquivosDeCodigoFonte(string? pasta = null)
    {
        var raiz = pasta ?? Path.Combine(ArquiteturaTestes.LocalizarRaizDoRepositorio(), "src");
        if (!Directory.Exists(raiz)) return [];

        return Directory
            .EnumerateFiles(raiz, "*.cs", SearchOption.AllDirectories)
            .Where(a => !a.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(a => !a.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    /// <summary>Tira as linhas de comentário — comentário que cita o termo é conhecimento, não violação.</summary>
    private static string SemComentarios(string conteudo) =>
        string.Join('\n', conteudo.Split('\n').Where(linha =>
        {
            var l = linha.TrimStart();
            return !l.StartsWith("//", StringComparison.Ordinal)
                && !l.StartsWith("*", StringComparison.Ordinal);
        }));

    /// <summary>Propriedades mais métodos de verdade — o acessor gerado da propriedade não conta duas vezes.</summary>
    private static bool RetornaTarefa(Type retorno) =>
        retorno == typeof(Task)
        || retorno == typeof(ValueTask)
        || (retorno.IsGenericType
            && (retorno.GetGenericTypeDefinition() == typeof(Task<>)
                || retorno.GetGenericTypeDefinition() == typeof(ValueTask<>)));

    private static int ContarMembros(Type interfaceDeclarada) =>
        interfaceDeclarada.GetProperties().Length
        + interfaceDeclarada.GetMethods().Count(m => !m.IsSpecialName);
}
