using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes;

/// <summary>
/// Testes de arquitetura.
///
/// Não testam comportamento — testam que a ESTRUTURA não foi violada. Rodam no CI, e
/// falha aqui bloqueia o merge. É o que impede que o projeto derive de volta para os
/// problemas do sistema legado por descuido, ao longo de 17 meses e centenas de PRs.
/// </summary>
[Trait("Categoria", "Arquitetura")]
public class ArquiteturaTestes
{
    private static readonly System.Reflection.Assembly Dominio =
        typeof(Tracbel.Crm.Dominio.Comercial.Cliente).Assembly;

    [Fact]
    public void Dominio_nao_depende_de_infraestrutura_nem_de_banco()
    {
        // Esta é a regra que torna a regra de negócio testável sem banco, sem HTTP e sem
        // mock complicado — e é o que viabiliza a exigência de 90% de cobertura no domínio.
        var resultado = Types.InAssembly(Dominio)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Tracbel.Crm.Infraestrutura",
                "Tracbel.Crm.Aplicacao",
                "Tracbel.Crm.Api",
                "Tracbel.Crm.Integracao",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "System.Data.SqlClient",
                "Microsoft.Data.SqlClient")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            "o domínio precisa ser testável sem infraestrutura. Violações: {0}",
            string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact]
    public void Vocabulario_do_Vortice_fica_confinado_na_camada_de_integracao()
    {
        // [V] O vocabulário do legado é uma armadilha:
        //   - CodProcesso NÃO é o código do processo, é o tipo de fluxo
        //   - IV_Agenda.Vendedor guarda um LOGIN (varchar), não um ID
        //   - GE_Pessoa.Status tem seis valores mais 2.126 linhas em branco
        //
        // Se esses conceitos vazarem para o domínio, o CRM novo nasce contaminado e nunca
        // conseguirá desligar o Vórtice. Só Tracbel.Crm.Integracao pode pronunciá-los.
        string[] termosProibidos =
        [
            "SeqPessoa", "SeqAgenda", "SeqHistorico", "SeqProcesso", "SeqUsuario",
            "CodProcesso", "CodUsuario", "AcaoAuto", "ProcResultado", "NroEmpresa"
        ];

        string[] pastasProtegidas =
        [
            Path.Combine("src", "Tracbel.Crm.Dominio"),
            Path.Combine("src", "Tracbel.Crm.Aplicacao")
        ];

        var raiz = LocalizarRaizDoRepositorio();
        var violacoes = new List<string>();

        foreach (var pasta in pastasProtegidas)
        {
            var caminho = Path.Combine(raiz, pasta);
            if (!Directory.Exists(caminho)) continue;

            foreach (var arquivo in Directory.EnumerateFiles(caminho, "*.cs", SearchOption.AllDirectories))
            {
                var conteudo = File.ReadAllText(arquivo);

                // Comentários explicativos marcados com [V] são PERMITIDOS e desejáveis:
                // registram a lição. O que não pode é o termo virar código.
                var linhas = conteudo.Split('\n')
                    .Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal)
                             && !l.TrimStart().StartsWith("///", StringComparison.Ordinal)
                             && !l.TrimStart().StartsWith("*", StringComparison.Ordinal));

                var codigo = string.Join('\n', linhas);

                violacoes.AddRange(
                    from termo in termosProibidos
                    where codigo.Contains(termo, StringComparison.Ordinal)
                    select $"{Path.GetFileName(arquivo)}: '{termo}'");
            }
        }

        violacoes.Should().BeEmpty(
            "termos do Vórtice só podem existir em Tracbel.Crm.Integracao (comentários [V] são permitidos)");
    }

    [Fact]
    public void Entidades_nao_expoem_setter_publico()
    {
        // Encapsulamento não é estilo: é o que impede que uma tela, um job ou uma
        // integração deixem o registro num estado que o negócio não admite.
        var entidades = Types.InAssembly(Dominio)
            .That().Inherit(typeof(Dominio.Comum.EntidadeBase))
            .GetTypes();

        var comSetterPublico =
            (from tipo in entidades
             from prop in tipo.GetProperties()
             let setter = prop.GetSetMethod(nonPublic: false)
             where setter is not null && setter.IsPublic
             select $"{tipo.Name}.{prop.Name}").ToList();

        comSetterPublico.Should().BeEmpty(
            "estado de entidade muda por MÉTODO com regra, nunca por atribuição direta");
    }

    [Fact]
    public void Eventos_de_dominio_sao_imutaveis()
    {
        var eventos = Types.InAssembly(Dominio)
            .That().ImplementInterface(typeof(Dominio.Comum.IEventoDominio))
            .GetTypes();

        // NÃO EXIGE QUE EXISTA EVENTO. Os únicos eventos de domínio escritos até aqui eram os do
        // `Lead`, que saiu na fase 1 (documento 41) junto com a tabela vazia — e o teste que
        // exigisse "pelo menos um" estaria exigindo que alguém invente um fato para satisfazê-lo.
        // O que esta regra protege é a FORMA do evento, e ela vale para zero, um ou trinta.

        var mutaveis =
            (from tipo in eventos
             from prop in tipo.GetProperties()
             let setter = prop.GetSetMethod(nonPublic: false)
             // init-only tem SetMethod, mas com o modificador IsExternalInit.
             where setter is not null
                && !setter.ReturnParameter.GetRequiredCustomModifiers()
                          .Any(m => m.Name == "IsExternalInit")
             select $"{tipo.Name}.{prop.Name}").ToList();

        mutaveis.Should().BeEmpty("um fato que já aconteceu não se altera");
    }

    /// <summary>Sobe pelas pastas até achar o arquivo de solução.</summary>
    internal static string LocalizarRaizDoRepositorio()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);

        while (atual is not null && !atual.EnumerateFiles("*.sln").Any())
            atual = atual.Parent;

        return atual?.FullName
            ?? throw new InvalidOperationException(
                "Não encontrei a raiz do repositório (nenhum .sln nos diretórios acima).");
    }
}
