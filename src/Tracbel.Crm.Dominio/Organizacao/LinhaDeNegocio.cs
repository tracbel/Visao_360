namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// A linha de negócio: tratores, colheitadeiras, peças, serviços, pneus.
///
/// Não é lista simples — cada linha tem o ciclo de contato esperado por classe de cliente
/// (A/B/C/D), que é o que a tela de Cobertura usa para dizer se um cliente está atrasado.
/// Substitui oito tabelas do Vórtice, sete delas vazias.
/// </summary>
public sealed class LinhaDeNegocio
{
    private LinhaDeNegocio() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável: <c>MAQ</c>, <c>PEC</c>, <c>DSI</c>, <c>PNEUS</c>, <c>PUK</c>.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Dias de ciclo esperados para cliente classe A.</summary>
    public short? DiasCicloClasseA { get; private set; }

    /// <summary>Dias de ciclo esperados para cliente classe B.</summary>
    public short? DiasCicloClasseB { get; private set; }

    /// <summary>Dias de ciclo esperados para cliente classe C.</summary>
    public short? DiasCicloClasseC { get; private set; }

    /// <summary>Dias de ciclo esperados para cliente classe D.</summary>
    public short? DiasCicloClasseD { get; private set; }

    /// <summary>Ordem de exibição nas telas.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma linha de negócio.</summary>
    /// <param name="codigo">Código estável.</param>
    /// <param name="nome">Nome legível.</param>
    /// <param name="ordem">Ordem de exibição.</param>
    public static LinhaDeNegocio Criar(string codigo, string nome, short ordem = 100) => new()
    {
        Codigo = codigo,
        Nome = nome,
        Ordem = ordem
    };

    /// <summary>
    /// Declara de quantos em quantos dias cada classe de cliente deve ser visitada.
    ///
    /// <para>É O ÚNICO NÚMERO DE META QUE O LEGADO TEM PREENCHIDO, e ele estava sendo jogado
    /// fora: a carga lia <c>CicloA</c>..<c>CicloD</c> de <c>IVS_Depto</c> e nunca gravava,
    /// porque <see cref="Criar"/> não recebia os quatro. Sem eles, "cliente fora do ciclo" não
    /// tinha contra o que ser calculado, e a Cobertura mostrava "sem cadência declarada" em
    /// todas as linhas.</para>
    ///
    /// <para>Os valores reais: Venda de Máquinas e Implemento usa 180 dias para A, B e C e 360
    /// para D; Prospecção usa 120/120/120/180; AMS, Peças e os de Equipamento usam 360 nos
    /// quatro. Que A, B e C tenham o mesmo número na maioria das linhas é informação, e não
    /// defeito — diz que a distinção entre as três classes hoje não muda a cadência.</para>
    /// </summary>
    /// <param name="diasClasseA">Cadência da classe A, em dias.</param>
    /// <param name="diasClasseB">Cadência da classe B, em dias.</param>
    /// <param name="diasClasseC">Cadência da classe C, em dias.</param>
    /// <param name="diasClasseD">Cadência da classe D, em dias.</param>
    public void DeclararCadencia(short? diasClasseA, short? diasClasseB, short? diasClasseC, short? diasClasseD)
    {
        DiasCicloClasseA = diasClasseA;
        DiasCicloClasseB = diasClasseB;
        DiasCicloClasseC = diasClasseC;
        DiasCicloClasseD = diasClasseD;
    }
}
