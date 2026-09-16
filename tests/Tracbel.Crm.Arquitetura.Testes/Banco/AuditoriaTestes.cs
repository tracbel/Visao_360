using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — colunas de auditoria em tabela transacional.
///
/// Cobre o documento 14, seção 5. Toda entidade que herda <see cref="EntidadeBase"/> é, por
/// definição, uma tabela TRANSACIONAL — e o próprio C# já garante as colunas via herança. O
/// papel deste teste é vigiar que ninguém tire uma delas do mapeamento com <c>Ignore(...)</c>.
///
/// MUDANÇA DE PROVEDOR — a coluna de concorrência otimista volta a ser
/// <see cref="EntidadeBase.Versao"/>, um <c>rowversion</c> que o SQL Server incrementa a cada
/// gravação da linha, inclusive na que não passa pela aplicação
/// (<c>docs/projeto/20-DECISAO-SQL-SERVER.md</c>). Na passagem pelo PostgreSQL o papel era
/// da coluna de sistema <c>xmin</c>; a promessa nunca mudou — quem salvar em cima do trabalho
/// de outra pessoa recebe erro em vez de sobrescrever em silêncio. O teste continua exigindo
/// o MECANISMO, e não o nome da coluna: é o que o mantém honesto se o provedor mudar de novo.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed class AuditoriaTestes
{
    /// <summary>
    /// As sete colunas de auditoria que existem como propriedade do domínio. A oitava — a de
    /// concorrência — é verificada à parte, porque o mecanismo depende do provedor.
    /// </summary>
    private static readonly string[] ColunasObrigatorias =
    [
        nameof(EntidadeBase.Id),
        nameof(EntidadeBase.ChavePublica),
        nameof(EntidadeBase.CriadoEm),
        nameof(EntidadeBase.CriadoPorId),
        nameof(EntidadeBase.AlteradoEm),
        nameof(EntidadeBase.AlteradoPorId),
        nameof(EntidadeBase.ExcluidoEm)
    ];

    [Fact]
    public void Entidade_transacional_mantem_todas_as_colunas_de_auditoria()
    {
        // [V] desativar um usuário no Vórtice destrói as permissões porque não existe flag de
        // ativo/inativo (achado 5.20) — ninguém decidiu isso de propósito, a coluna
        // simplesmente nunca existiu. Aqui a exclusão lógica (ExcluidoEm) e o resto do bloco
        // de auditoria vêm de EntidadeBase: o teste garante que continuam vindo.
        var tiposTransacionais = ModeloBanco.Modelo.GetEntityTypes()
            .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType))
            .ToList();

        tiposTransacionais.Should().NotBeEmpty(
            "precisa existir pelo menos uma entidade transacional mapeada para este teste " +
            "ter algo para proteger");

        var faltando = new List<string>();

        foreach (var tipo in tiposTransacionais)
        {
            foreach (var nomeColuna in ColunasObrigatorias)
            {
                if (tipo.FindProperty(nomeColuna) is null)
                    faltando.Add($"{tipo.ClrType.Name}.{nomeColuna}");
            }
        }

        faltando.Should().BeEmpty(
            "toda entidade transacional (herda EntidadeBase) mantém as colunas de auditoria " +
            "do bloco padrão — nenhuma pode ter sido removida com Ignore(...) na configuração " +
            "(documento 14, seção 5). Faltando: {0}",
            string.Join(", ", faltando));
    }

    [Fact]
    public void Toda_entidade_transacional_tem_controle_de_concorrencia_otimista()
    {
        // O que se exige é o MECANISMO: uma propriedade marcada como token de concorrência e
        // gerada pelo banco. No SQL Server isso é a coluna Versao (rowversion); num provedor
        // sem rowversion, seria outra coisa (no PostgreSQL era a sombra 'xmin'). Qualquer um
        // passa — nenhum não passa.
        var semControle = new List<string>();

        foreach (var tipo in ModeloBanco.Modelo.GetEntityTypes()
                     .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType)))
        {
            var temToken = tipo.GetProperties().Any(p =>
                p.IsConcurrencyToken && p.ValueGenerated.HasFlag(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate));

            if (!temToken) semControle.Add(tipo.ClrType.Name);
        }

        semControle.Should().BeEmpty(
            "toda entidade transacional precisa de controle de concorrência otimista gerado " +
            "pelo banco (documento 14, seção 5): no SQL Server é a coluna Versao (rowversion), " +
            "configurada em CrmDbContext.ConfigurarConcorrenciaOtimista. Sem controle: {0}",
            string.Join(", ", semControle));
    }

    [Fact]
    public void Colunas_de_auditoria_de_quem_fez_nunca_sao_texto_livre()
    {
        // CriadoPorId/AlteradoPorId apontam para o usuário por ID (bigint), nunca por nome ou
        // login. [V] IV_Agenda.Vendedor guarda um LOGIN (varchar) em vez de ID — é a mesma
        // classe de erro que este teste impede para as colunas de autoria.
        var foraDoPadrao = ModeloBanco.Modelo.GetEntityTypes()
            .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType))
            .SelectMany(t => new[]
            {
                t.FindProperty(nameof(EntidadeBase.CriadoPorId)),
                t.FindProperty(nameof(EntidadeBase.AlteradoPorId))
            })
            .Where(p => p is not null)
            .Where(p => (Nullable.GetUnderlyingType(p!.ClrType) ?? p.ClrType) != typeof(long))
            .Select(p => $"{p!.DeclaringType.ClrType.Name}.{p.Name} ({p.ClrType.Name})")
            .ToList();

        foraDoPadrao.Should().BeEmpty(
            "CriadoPorId e AlteradoPorId são sempre long (bigint) — id de usuário, nunca " +
            "texto (documento 14, seção 5). Fora do padrão: {0}",
            string.Join(", ", foraDoPadrao));
    }

    [Fact]
    public void A_politica_de_auditoria_so_nomeia_entidade_e_campo_que_existem_no_modelo()
    {
        // Documento 41, fase 2. A política mora em código e é lida no SaveChanges: um campo renomeado
        // na entidade e esquecido na política faria TODA gravação daquela entidade falhar em produção.
        // Este teste troca a falha em produção por uma falha no build.
        var problemas = new List<string>();

        foreach (var entidade in Dominio.Auditoria.PoliticaDeAuditoria.Entidades)
        {
            var tipo = ModeloBanco.Modelo.GetEntityTypes().SingleOrDefault(t => t.ClrType.Name == entidade);
            if (tipo is null)
            {
                problemas.Add($"{entidade} (entidade fora do modelo)");
                continue;
            }

            if (tipo.FindProperty("Id") is null) problemas.Add($"{entidade} (sem Id para a trilha apontar)");

            foreach (var campo in Dominio.Auditoria.PoliticaDeAuditoria.CamposDe(entidade))
                if (tipo.FindProperty(campo) is null)
                    problemas.Add($"{entidade}.{campo}");
        }

        problemas.Should().BeEmpty(
            "PoliticaDeAuditoria só pode nomear o que o modelo mapeia (documento 41, fase 2). Fora do modelo: {0}",
            string.Join(", ", problemas));
    }

    [Fact]
    public void A_politica_de_auditoria_nao_audita_carimbo_nem_concorrencia()
    {
        // Carimbo muda em toda gravação e não é decisão de ninguém: auditá-lo é o recarimbo que fez do
        // log do Vórtice 42% do banco.
        string[] carimbos =
        [
            nameof(EntidadeBase.CriadoEm), nameof(EntidadeBase.CriadoPorId), nameof(EntidadeBase.AlteradoEm),
            nameof(EntidadeBase.AlteradoPorId), nameof(EntidadeBase.Versao), nameof(EntidadeBase.ChavePublica)
        ];

        var auditados = Dominio.Auditoria.PoliticaDeAuditoria.Entidades
            .SelectMany(e => Dominio.Auditoria.PoliticaDeAuditoria.CamposDe(e).Select(c => $"{e}.{c}"))
            .Where(nome => carimbos.Any(c => nome.EndsWith("." + c, StringComparison.Ordinal)))
            .ToList();

        auditados.Should().BeEmpty("carimbo e concorrência não entram na trilha. Encontrado: {0}", string.Join(", ", auditados));
    }

    [Fact]
    public void Toda_tabela_transacional_declara_a_coluna_de_multiempresa()
    {
        // Documento 14, seção 5.2 — era [Recomendação, sem teste hoje]. Passa a ser testada:
        // EmpresaId é a ÚNICA fronteira de multiempresa e precisa estar na PRÓPRIA LINHA, para
        // que a checagem de acesso seja uma comparação de coluna, sem join.
        //
        // [V] no Vórtice a multiempresa é nominal: a coluna existe na tabela de permissão e tem
        // UM ÚNICO VALOR DISTINTO em toda ela; 340 dos 409 usuários ativos alcançam 17 das 18
        // filiais. Coluna que existe e não é usada como fronteira não é fronteira.
        //
        // As exceções são as entidades que NÃO pertencem a uma filial: o próprio cadastro de
        // filial, o usuário (que tem a filial de casa em EmpresaId, e a tem), e as entidades
        // de alcance global.
        var semEmpresa = ModeloBanco.Modelo.GetEntityTypes()
            .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType))
            .Where(t => t.FindProperty("EmpresaId") is null)
            .Select(t => t.ClrType.Name)
            .ToList();

        semEmpresa.Should().BeEmpty(
            "toda tabela transacional declara EmpresaId (documento 14, seção 5.2) — é a única " +
            "fronteira de multiempresa, e ela mora na própria linha para que o filtro global de " +
            "segurança não precise de join. Sem EmpresaId: {0}",
            string.Join(", ", semEmpresa));
    }
}
