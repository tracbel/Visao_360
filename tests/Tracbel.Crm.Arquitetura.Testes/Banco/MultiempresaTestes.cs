using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — a fronteira de multiempresa, do lado que importa: o USO.
///
/// Cobre o documento 14, seções 5.2 e 11.3, e corrige o achado A-1 do documento 21.
///
/// O QUE ESTE ARQUIVO EXISTE PARA IMPEDIR: <c>AuditoriaTestes</c> já exigia que toda tabela
/// transacional DECLARASSE <c>EmpresaId</c>, e passava — mas a coluna existia em 21 tabelas e o
/// filtro global existia em UMA. O teste protegia a metade barata da regra, e o critério que o
/// condena está escrito no próprio documento 14, seção 5.2: *"uma coluna que existe mas não é
/// usada como fronteira de fato não é fronteira nenhuma"*.
///
/// Aqui a outra metade é cobrada: quem declara a coluna tem o filtro, ou tem nome e motivo na
/// lista de exceções. Entidade nova com <c>EmpresaId</c> e sem filtro reprova o
/// <c>dotnet test</c> — o esquecimento deixa de ser possível sem alarme.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed class MultiempresaTestes
{
    /// <summary>
    /// As exceções ACORDADAS — registradas no documento 21, seção "Correções aplicadas", com a
    /// justificativa de cada uma.
    ///
    /// A lista está aqui, e não só no <c>CrmDbContext</c>, DE PROPÓSITO: sem esta cópia, quem
    /// quisesse silenciar o teste bastaria acrescentar a entidade nova em
    /// <c>FronteiraDeEmpresaJustificada</c> e o build voltaria a passar. Com ela, criar uma
    /// exceção exige mexer em dois arquivos e escrever o motivo no documento — que é
    /// exatamente o custo que uma exceção de segurança deve ter.
    ///
    /// <para>ERAM DUAS até a fase 1 (documento 41): a segunda era
    /// <c>CompartilhamentoDeRegistro</c>, cuja tabela nunca recebeu uma linha e saiu. Exceção de
    /// segurança que some é exceção a menos, e por isso este número só encolhe sem decisão.</para>
    ///
    /// <para>VOLTARAM A SER DUAS na fase 3 (P-20, 21/09/2026): <c>UsuarioPerfil</c>, cuja <c>EmpresaId</c>
    /// é a filial EM QUE o perfil concedido vale — o dado que define quais filiais a pessoa pode escolher,
    /// e que por isso não pode ser filtrado pela fronteira que ele mesmo define. Motivo no documento 05.</para>
    /// </summary>
    private static readonly string[] ExcecoesAcordadas =
    [
        "Usuario",
        "UsuarioPerfil"
    ];

    private const string ColunaDeEmpresa = CrmDbContext.ColunaDeEmpresa;

    private static IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IEntityType> ComColunaDeEmpresa() =>
        ModeloBanco.Modelo.GetEntityTypes().Where(t => t.FindProperty(ColunaDeEmpresa) is not null);

    [Fact]
    public void Toda_entidade_com_EmpresaId_tem_filtro_global_que_usa_a_coluna()
    {
        // [V] a multiempresa do Vórtice é nominal: 18 filiais, NroEmpresa em toda tabela de
        // permissão, e a coluna com UM ÚNICO valor distinto na tabela inteira; 340 dos 409
        // usuários ativos alcançam 17 das 18. É o retrato de coluna que existe e não filtra.
        var entidades = ComColunaDeEmpresa().ToList();

        entidades.Should().HaveCountGreaterThan(1,
            "o modelo precisa ter entidades com EmpresaId para este teste proteger alguma coisa");

        var semFiltro = new List<string>();
        var comFiltroQueNaoCitaAColuna = new List<string>();

        foreach (var tipo in entidades)
        {
            var nome = tipo.ClrType.Name;
            if (CrmDbContext.FronteiraDeEmpresaJustificada.ContainsKey(nome)) continue;

            // `GetDeclaredQueryFilters()` e não `GetQueryFilter()`: o EF Core 10 aposentou o segundo
            // porque uma entidade passou a poder ter MAIS DE UM filtro global, cada um com nome. A
            // pergunta deste teste não muda — "existe filtro, e ele cita a coluna de empresa?" —,
            // só passa a valer para o conjunto.
            var filtros = tipo.GetDeclaredQueryFilters();

            if (filtros.Count == 0)
            {
                semFiltro.Add(nome);
                continue;
            }

            // Ter filtro não basta: ele precisa filtrar POR EMPRESA. Um filtro que só
            // escondesse o excluído logicamente passaria pela checagem de existência e
            // deixaria a fronteira aberta do mesmo jeito.
            if (!filtros.Any(f => f.Expression is { } e
                                  && e.Body.ToString().Contains(ColunaDeEmpresa, StringComparison.Ordinal)))
                comFiltroQueNaoCitaAColuna.Add(nome);
        }

        semFiltro.Should().BeEmpty(
            "toda entidade que declara {0} recebe o filtro global de empresa (documento 14, " +
            "regra 11.3) — quem precisa de exceção entra em " +
            "CrmDbContext.FronteiraDeEmpresaJustificada COM o motivo escrito, e também na lista " +
            "deste teste e no documento 21. Sem filtro: {1}",
            ColunaDeEmpresa, string.Join(", ", semFiltro));

        comFiltroQueNaoCitaAColuna.Should().BeEmpty(
            "o filtro global precisa usar a coluna {0}: filtro que não cita a fronteira não é " +
            "fronteira. Filtram outra coisa: {1}",
            ColunaDeEmpresa, string.Join(", ", comFiltroQueNaoCitaAColuna));
    }

    [Fact]
    public void Todo_filtro_de_empresa_reconhece_a_via_de_escape_declarada()
    {
        // A via de escape existe para o administrador e para a integração que precisam ler
        // entre filiais. Se um filtro não a reconhecesse, quem abrisse o alcance receberia
        // resultado PARCIAL — metade das tabelas atravessando a fronteira e metade não, que é
        // pior do que não ter escape nenhum, porque parece que funcionou.
        var semEscape = ComColunaDeEmpresa()
            .Where(t => !CrmDbContext.FronteiraDeEmpresaJustificada.ContainsKey(t.ClrType.Name))
            .Where(t => t.GetDeclaredQueryFilters() is { Count: > 0 } filtros
                        && !filtros.Any(f => f.Expression is { } e
                                             && e.Body.ToString().Contains("_alcanceEntreEmpresas", StringComparison.Ordinal)))
            .Select(t => t.ClrType.Name)
            .ToList();

        semEscape.Should().BeEmpty(
            "o filtro de empresa de toda entidade filtrada precisa reconhecer o alcance aberto " +
            "por CrmDbContext.AbrirAlcanceEntreEmpresas — senão o escape vale para umas tabelas " +
            "e não para outras. Sem escape: {0}",
            string.Join(", ", semEscape));
    }

    [Fact]
    public void As_excecoes_da_fronteira_sao_exatamente_as_acordadas_e_cada_uma_tem_motivo_escrito()
    {
        // Exceção de segurança sem motivo escrito é exceção silenciosa, e o documento 14,
        // seção 15, é explícito: regra sem teste é recomendação. Aqui a exceção só existe se
        // estiver NOMEADA nos dois lados e justificada por extenso.
        CrmDbContext.FronteiraDeEmpresaJustificada.Keys.Should().BeEquivalentTo(
            ExcecoesAcordadas,
            "a lista de exceções da fronteira de empresa é fechada: acrescentar uma exige " +
            "registrar o motivo em CrmDbContext.FronteiraDeEmpresaJustificada, nesta lista e na " +
            "seção de correções aplicadas do documento 21");

        var semJustificativaDeVerdade = CrmDbContext.FronteiraDeEmpresaJustificada
            .Where(e => e.Value.Trim().Length < 80)
            .Select(e => e.Key)
            .ToList();

        semJustificativaDeVerdade.Should().BeEmpty(
            "a justificativa precisa explicar POR QUE a entidade não pode ser filtrada por " +
            "empresa — uma frase curta como 'catálogo' não é justificativa. Curtas demais: {0}",
            string.Join(", ", semJustificativaDeVerdade));
    }

    [Fact]
    public void Nenhuma_excecao_da_fronteira_aponta_para_entidade_que_sumiu_ou_perdeu_a_coluna()
    {
        // Entrada morta na lista de exceções é pior do que inútil: ela sugere que alguém
        // decidiu isso de propósito, quando a entidade nem existe mais.
        var comColuna = ComColunaDeEmpresa().Select(t => t.ClrType.Name).ToHashSet(StringComparer.Ordinal);

        var mortas = CrmDbContext.FronteiraDeEmpresaJustificada.Keys
            .Where(nome => !comColuna.Contains(nome))
            .ToList();

        mortas.Should().BeEmpty(
            "exceção da fronteira de empresa para entidade que não existe ou não declara mais " +
            "{0}: a entrada precisa sair da lista. Mortas: {1}",
            ColunaDeEmpresa, string.Join(", ", mortas));
    }
}
