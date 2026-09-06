namespace Tracbel.Crm.Dominio.Metadado;

/// <summary>
/// Um item pronto para virar opção de um campo de seleção.
///
/// O QUE VAI PARA O HISTÓRICO É O <see cref="Codigo"/>, nunca a <see cref="Descricao"/>: o
/// código é estável e não muda depois de usado; a descrição é o que a gente lê e pode ser
/// corrigida (documento 16, seção 3). Por isso a tela mostra a descrição e manda o código —
/// e por isso a API recusa qualquer código que não esteja nesta lista.
/// </summary>
/// <param name="Codigo">O código estável, em maiúsculas, sem acento nem espaço.</param>
/// <param name="Descricao">O rótulo que o usuário lê.</param>
/// <param name="Ordem">Ordem de exibição.</param>
/// <param name="ExigeObservacao">Se escolher este item obriga a escrever uma observação (o item "Outro").</param>
public sealed record ItemParaSelecao(string Codigo, string Descricao, short Ordem, bool ExigeObservacao);

/// <summary>Um catálogo inteiro, com os itens ativos, na ordem em que a tela deve oferecê-los.</summary>
/// <param name="Codigo">O código do catálogo. Ex.: <c>MOTIVO_INATIVACAO</c>.</param>
/// <param name="Nome">O nome legível do catálogo.</param>
/// <param name="Descricao">Para que serve, em português.</param>
/// <param name="PermiteItemNovo">Se o negócio pode acrescentar item pela tela, sem release.</param>
/// <param name="Itens">Os itens ativos.</param>
public sealed record CatalogoParaSelecao(
    string Codigo,
    string Nome,
    string? Descricao,
    bool PermiteItemNovo,
    IReadOnlyList<ItemParaSelecao> Itens);

/// <summary>
/// Um modelo de máquina resolvido com a família e a marca a que pertence — o suficiente para a
/// tela mostrar "John Deere · Tratores · 7250R" sem três chamadas.
/// </summary>
/// <param name="Id">O identificador interno, que o cadastro de equipamento grava.</param>
/// <param name="Codigo">O código estável do modelo.</param>
/// <param name="Nome">O nome do modelo. Ex.: 7250R.</param>
/// <param name="Familia">O nome da família.</param>
/// <param name="Marca">O nome da marca.</param>
/// <param name="MarcaRepresentada">Falso quando é marca de concorrente — o que a tela de Cobertura usa.</param>
public sealed record ModeloParaSelecao(
    int Id,
    string Codigo,
    string Nome,
    string Familia,
    string Marca,
    bool MarcaRepresentada);
