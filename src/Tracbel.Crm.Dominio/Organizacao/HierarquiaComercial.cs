namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// Quem responde a quem — tabela de fechamento (<i>closure table</i>).
///
/// Materializa todos os pares ancestral → descendente, para que "quem está abaixo de mim,
/// em qualquer profundidade" seja uma leitura por índice, não uma consulta recursiva.
///
/// [V] A hierarquia comercial do Vórtice é um ponteiro de um nível só, sem reatribuição:
/// trocar o usuário de um gerente quebra o relatório de carteira porque o novo "não lidera
/// ninguém". Aqui a hierarquia é reconstruída na mesma transação em que o gestor muda.
/// </summary>
public sealed class HierarquiaComercial
{
    private HierarquiaComercial() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O gestor, em qualquer nível acima.</summary>
    public long AncestralId { get; private set; }

    /// <summary>O subordinado.</summary>
    public long DescendenteId { get; private set; }

    /// <summary>Zero é ele mesmo; 1 é subordinado direto; 2 é o nível seguinte.</summary>
    public short Profundidade { get; private set; }

    /// <summary>Cria um par da hierarquia.</summary>
    public static HierarquiaComercial Criar(long ancestralId, long descendenteId, short profundidade) => new()
    {
        AncestralId = ancestralId,
        DescendenteId = descendenteId,
        Profundidade = profundidade
    };
}
