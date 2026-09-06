using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Relatorio;

/// <summary>
/// O contrato curado: que entidades podem ser consultadas e com que filtro de segurança.
///
/// [SF] O administrador cura o conjunto de dados; o usuário monta o relatório sem enxergar o
/// esquema e SEM ESCREVER SQL. [V] No Vórtice o relatório guarda SQL cru numa coluna de
/// texto, aceita comandos de alteração, e 125 dos 134 relatórios não têm nenhum predicado de
/// usuário — o filtro da tela simplesmente não vale para o relatório. Substitui 411 views
/// mais seis tabelas de consulta.
/// </summary>
public sealed class Fonte
{
    private Fonte() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável. Ex.: PIPELINE_VENDAS.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve, em português.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Nome da visão que serve esta fonte, já com o filtro de segurança aplicado.</summary>
    public string NomeDaVisao { get; private set; } = default!;

    /// <summary>Entidade raiz da consulta, sobre a qual a segurança por linha incide.</summary>
    public string EntidadeRaiz { get; private set; } = default!;

    /// <summary>Sistema externo lido, quando a fonte busca dado fora do CRM.</summary>
    public int? SistemaId { get; private set; }

    /// <summary>
    /// Se o resultado é materializado por job. Quando é, a tela mostra o carimbo de
    /// atualização; a fonte é derivada e recalculável, e nunca recebe escrita.
    /// </summary>
    public bool EhMaterializada { get; private set; }

    /// <summary>Quando o resultado materializado foi atualizado pela última vez (UTC).</summary>
    public DateTime? AtualizadaEm { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma fonte de relatório.</summary>
    public static Fonte Criar(string codigo, string nome, string nomeDaVisao, string entidadeRaiz) => new()
    {
        Codigo = codigo,
        Nome = nome,
        NomeDaVisao = nomeDaVisao,
        EntidadeRaiz = entidadeRaiz
    };
}

/// <summary>
/// O rótulo de negócio de cada campo da fonte.
///
/// É o que faz a tela dizer Valor e não uma abreviação de banco.
/// </summary>
public sealed class FonteCampo
{
    private FonteCampo() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A fonte a que o campo pertence.</summary>
    public int FonteId { get; private set; }

    /// <summary>Nome do campo na visão.</summary>
    public string Campo { get; private set; } = default!;

    /// <summary>O que o usuário lê.</summary>
    public string Rotulo { get; private set; } = default!;

    /// <summary>Tipo do dado, para a tela saber que filtro oferecer.</summary>
    public string TipoDeDado { get; private set; } = default!;

    /// <summary>Se pode agrupar por este campo.</summary>
    public bool PermiteAgrupar { get; private set; } = true;

    /// <summary>Se pode filtrar por este campo.</summary>
    public bool PermiteFiltrar { get; private set; } = true;

    /// <summary>Se pode somar este campo.</summary>
    public bool PermiteSomar { get; private set; }

    /// <summary>Ordem de exibição.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>Declara um campo da fonte.</summary>
    public static FonteCampo Criar(int fonteId, string campo, string rotulo, string tipoDeDado) => new()
    {
        FonteId = fonteId,
        Campo = campo,
        Rotulo = rotulo,
        TipoDeDado = tipoDeDado
    };
}

/// <summary>Quem enxerga o relatório salvo.</summary>
public enum VisibilidadeDoRelatorio
{
    /// <summary>Só quem criou.</summary>
    Privado = 0,

    /// <summary>A equipe de quem criou.</summary>
    Equipe = 1,

    /// <summary>Toda a filial.</summary>
    Empresa = 2
}

/// <summary>
/// O relatório salvo do usuário — uma definição em JSON, nunca SQL.
///
/// O motor traduz a definição para consulta parametrizada. O usuário nunca escreve SQL, e
/// por isso não existe caminho para furar a segurança por linha.
/// </summary>
public sealed class Relatorio : EntidadeBase
{
    private Relatorio() { }

    /// <summary>Filial dona do relatório.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>A fonte curada de onde o relatório lê.</summary>
    public int FonteId { get; private set; }

    /// <summary>Nome dado pelo usuário.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>
    /// Campos, filtros, agrupamentos, ordenação e gráfico — como JSON binário, para que o
    /// banco consiga indexar e consultar dentro da definição.
    /// </summary>
    public string Definicao { get; private set; } = default!;

    /// <summary>Quem enxerga.</summary>
    public VisibilidadeDoRelatorio Visibilidade { get; private set; } = VisibilidadeDoRelatorio.Privado;

    /// <summary>Quem responde pelo relatório.</summary>
    public long ProprietarioId { get; private set; }

    /// <summary>Salva um relatório do usuário.</summary>
    public static Relatorio Criar(
        int empresaId, int fonteId, string nome, string definicao, long proprietarioId, long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        FonteId = fonteId,
        Nome = nome,
        Definicao = definicao,
        ProprietarioId = proprietarioId,
        CriadoPorId = criadoPorId
    };
}
