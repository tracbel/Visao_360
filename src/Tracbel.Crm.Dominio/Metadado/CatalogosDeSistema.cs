namespace Tracbel.Crm.Dominio.Metadado;

/// <summary>
/// Os catálogos que o PRÓPRIO ESQUEMA referencia — identificador estável, decidido no código
/// e semeado pela migração.
///
/// POR QUE UM IDENTIFICADOR FIXO, se a regra 8.3 do documento 14 diz que o seed fixa o
/// <c>Codigo</c> e nunca o <c>Id</c>: porque a seção 8.11 do documento 17 exige que
/// <c>Contato.PapelId</c> só aceite item do catálogo <c>PAPEL_CONTATO</c>
/// **verificado pelo banco**, e o mecanismo disso é uma chave estrangeira COMPOSTA
/// <c>(CatalogoId, ItemId)</c> contra <c>AK_CatalogoItem_CatalogoId</c>, com a coluna de
/// catálogo persistida como CONSTANTE na tabela que aponta. Constante, no SQL Server, é
/// literal — e literal exige que o identificador do catálogo seja parte do esquema, não um
/// número que o banco sorteia na carga.
///
/// A regra 8.3 continua valendo para todo o resto: os catálogos de negócio de
/// <c>dados-referencia/</c> (marca, modelo, praça, fase...) nascem por código estável e com
/// <c>Id</c> gerado pelo banco. A exceção é ESTA lista curta, e ela é fechada aqui — não há
/// como acrescentar um catálogo de sistema sem passar por este arquivo, e o teste
/// <c>CatalogoDeSistemaTestes</c> exige que toda coluna que aponte para
/// <c>metadado.CatalogoItem</c> use uma destas constantes.
///
/// NUNCA REAPROVEITAR UM NÚMERO. Um catálogo que sair de uso vira <c>EstaAtivo = 0</c>; o
/// número dele fica queimado, do mesmo jeito que um código de catálogo aposentado nunca
/// volta (documento 16, seção 3.1).
/// </summary>
public static class CatalogosDeSistema
{
    /// <summary>Papel do contato na decisão de compra. Usado por <c>comercial.ClienteContato.PapelId</c>.</summary>
    public const int PapelDeContato = 1;

    /// <summary>Canal de origem. Usado por <c>comercial.Lead.OrigemId</c> e <c>comercial.Cliente.OrigemId</c>.</summary>
    public const int OrigemDeLead = 2;

    /// <summary>Por que o cliente foi inativado. Usado por <c>comercial.Cliente.MotivoInativacaoId</c>.</summary>
    public const int MotivoDeInativacao = 3;

    /// <summary>Por que o lead foi descartado. Usado por <c>comercial.Lead.MotivoDescarteId</c>.</summary>
    public const int MotivoDeDescarte = 4;

    /// <summary>Cultura agrícola da propriedade. Usado por <c>comercial.Endereco.CulturaId</c>.</summary>
    public const int Cultura = 5;

    /// <summary>Tipo de arquivo anexável. Usado por <c>documento.Documento.TipoDocumentoId</c>.</summary>
    public const int TipoDeDocumento = 6;

    /// <summary>Forma de pagamento. Usado por <c>processo.ItemDeProposta.CondicaoPagamentoId</c>.</summary>
    public const int CondicaoDePagamento = 7;

    /// <summary>
    /// Fabricante concorrente. Usado por <c>processo.Processo.ConcorrenteId</c> e por
    /// <c>processo.VendaPerdida.ConcorrenteId</c> — é o mesmo conceito nos dois lugares, e por
    /// isso é o mesmo catálogo.
    /// </summary>
    public const int Concorrente = 8;

    /// <summary>
    /// Tipo do equipamento disputado — tratores, colheitadeiras, implementos. Usado por
    /// <c>processo.VendaPerdida.TipoDeEquipamentoId</c>.
    ///
    /// <para>Não é <c>frota.Familia</c>: família pertence a uma marca ("tratores 7J da John
    /// Deere"), e o que a venda perdida declara é a categoria genérica da máquina, que vale
    /// para qualquer fabricante.</para>
    /// </summary>
    public const int TipoDeEquipamento = 9;

    /// <summary>
    /// Revenda concorrente que fechou o negócio — Coopercitrus, Agronew, Robusta. Usado por
    /// <c>processo.VendaPerdida.RevendaDoConcorrenteId</c>.
    /// </summary>
    public const int RevendaConcorrente = 10;

    /// <summary>
    /// A lista completa, na forma em que a migração a semeia: identificador, código estável,
    /// nome de tela e para que serve.
    ///
    /// <c>PermiteItemNovo</c> é <c>false</c> em nenhum deles hoje: todos os oito são listas que
    /// o negócio administra pela tela de Taxonomias.
    /// </summary>
    public static IReadOnlyList<(int Id, string Codigo, string Nome, string Descricao)> Todos { get; } =
    [
        (PapelDeContato, "PAPEL_CONTATO", "Papel do contato",
            "Papel do contato na decisão de compra do cliente."),
        (OrigemDeLead, "ORIGEM_LEAD", "Origem do lead",
            "Canal pelo qual o lead ou o cliente chegou à Tracbel."),
        (MotivoDeInativacao, "MOTIVO_INATIVACAO", "Motivo de inativação",
            "Por que um cliente deixou de ser cliente ativo."),
        (MotivoDeDescarte, "MOTIVO_DESCARTE", "Motivo de descarte",
            "Por que um lead foi descartado sem virar cliente."),
        (Cultura, "CULTURA", "Cultura agrícola",
            "Cultura agrícola da propriedade do cliente."),
        (TipoDeDocumento, "TIPO_DOCUMENTO", "Tipo de documento",
            "Tipo do arquivo anexado a cliente, processo ou equipamento."),
        (CondicaoDePagamento, "CONDICAO_PAGAMENTO", "Condição de pagamento",
            "Forma de pagamento ou financiamento do item de proposta."),
        (Concorrente, "CONCORRENTE", "Concorrente",
            "Fabricante concorrente que disputou ou levou o negócio."),
        (TipoDeEquipamento, "TIPO_EQUIPAMENTO", "Tipo de equipamento",
            "Categoria da máquina disputada: tratores, colheitadeiras, implementos."),
        (RevendaConcorrente, "REVENDA_CONCORRENTE", "Revenda concorrente",
            "Revenda que fechou o negócio quando a venda foi perdida.")
    ];
}
