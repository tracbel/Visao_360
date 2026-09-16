using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// Por qual coluna a listagem de clientes é ordenada.
///
/// É ENUM, e não texto, de propósito. Ordenação que chega como <c>string</c> vira nome de
/// coluna concatenado em SQL — ou um <c>switch</c> com um <c>default</c> silencioso que
/// ordena por outra coisa e ninguém percebe. Aqui o conjunto é fechado: o que não está na
/// lista é recusado na entrada, com o nome do campo e a lista do que vale.
/// </summary>
public enum OrdemDeCliente
{
    /// <summary>Razão social. É a ordem que a tela abre.</summary>
    Nome = 0,

    /// <summary>Data de cadastro.</summary>
    CriadoEm = 1,

    /// <summary>Situação no ciclo comercial.</summary>
    Situacao = 2,

    /// <summary>Data da última alteração.</summary>
    AlteradoEm = 3
}

/// <summary>Por qual coluna a listagem de equipamentos é ordenada. Mesma regra de <see cref="OrdemDeCliente"/>.</summary>
public enum OrdemDeEquipamento
{
    /// <summary>Chassi, que é a identidade da máquina.</summary>
    Chassi = 0,

    /// <summary>Data de cadastro.</summary>
    CriadoEm = 1,

    /// <summary>Situação da máquina.</summary>
    Situacao = 2,

    /// <summary>Ano do modelo.</summary>
    AnoModelo = 3
}

/// <summary>
/// O que a listagem de clientes pede.
///
/// TUDO AQUI É OPCIONAL MENOS A PAGINAÇÃO: a fronteira de empresa e a exclusão lógica NÃO são
/// filtros desta consulta — são invariantes aplicados pelo repositório e pelo filtro global do
/// <c>CrmDbContext</c>. Quem monta a consulta não consegue desligá-los, e é isso que faz a
/// fronteira valer também para a listagem.
/// </summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="Termo">Busca por razão social, nome fantasia ou documento. Nulo é sem busca.</param>
/// <param name="Situacao">Filtro por situação no ciclo comercial.</param>
/// <param name="TipoDePessoa">Filtro por física ou jurídica.</param>
/// <param name="ProprietarioId">Filtro por quem responde pelo cliente.</param>
/// <param name="Ordem">Coluna de ordenação.</param>
/// <param name="Descendente">Ordem decrescente.</param>
/// <param name="IncluirInativos">Traz também os inativados logicamente. Padrão é não trazer.</param>
public sealed record ConsultaDeClientes(
    Paginacao Paginacao,
    string? Termo = null,
    SituacaoDoCliente? Situacao = null,
    TipoDePessoa? TipoDePessoa = null,
    long? ProprietarioId = null,
    OrdemDeCliente Ordem = OrdemDeCliente.Nome,
    bool Descendente = false,
    bool IncluirInativos = false);

/// <summary>O que a listagem de equipamentos pede. Mesma disciplina de <see cref="ConsultaDeClientes"/>.</summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="Termo">Busca por chassi, número de série ou placa.</param>
/// <param name="Situacao">Filtro por situação da máquina.</param>
/// <param name="Origem">Filtro por quem afirma que a máquina existe: o ERP ou o CEN.</param>
/// <param name="ClienteId">Filtro pelo dono. É o que a Visão 360 usa.</param>
/// <param name="ModeloId">Filtro por modelo.</param>
/// <param name="Ordem">Coluna de ordenação.</param>
/// <param name="Descendente">Ordem decrescente.</param>
/// <param name="IncluirInativos">Traz também os baixados. Padrão é não trazer.</param>
/// <param name="LinhaDeProdutoCodigo">
/// Filtro pela classificação de produto; <see cref="SemClassificacao"/> traz as máquinas sem nenhuma.
/// </param>
/// <param name="Porte">Filtro pelo porte da classificação — trator pequeno, médio, grande.</param>
/// <param name="SomenteComVenda">Só as máquinas com venda registrada (ART).</param>
public sealed record ConsultaDeEquipamentos(
    Paginacao Paginacao,
    string? Termo = null,
    SituacaoDoEquipamento? Situacao = null,
    OrigemDoEquipamento? Origem = null,
    long? ClienteId = null,
    int? ModeloId = null,
    OrdemDeEquipamento Ordem = OrdemDeEquipamento.Chassi,
    bool Descendente = false,
    bool IncluirInativos = false,
    string? LinhaDeProdutoCodigo = null,
    PorteDeMaquina? Porte = null,
    bool SomenteComVenda = false)
{
    /// <summary>O valor do filtro de classificação que traz as máquinas ainda sem classificação.</summary>
    public const string SemClassificacao = "SEM_CLASSIFICACAO";
}

/// <summary>A classificação de produto de uma máquina, como a tela a mostra.</summary>
/// <param name="Codigo">O código estável. Ex.: TRATOR_MEDIO.</param>
/// <param name="Nome">O nome. Ex.: Trator médio.</param>
/// <param name="Porte">O porte.</param>
public sealed record ClassificacaoDaMaquina(string Codigo, string Nome, PorteDeMaquina Porte);

/// <summary>Uma classificação de produto do catálogo, com o identificador para gravar.</summary>
/// <param name="Id">O identificador interno.</param>
/// <param name="Codigo">O código estável.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Porte">O porte.</param>
public sealed record LinhaDeProdutoParaSelecao(int Id, string Codigo, string Nome, PorteDeMaquina Porte);

/// <summary>
/// A venda mais recente de uma máquina e quem foi o COMPRADOR NELA — que não é, por isso, o dono atual.
/// </summary>
/// <param name="Vendas">Quantas vendas a máquina tem.</param>
/// <param name="VendidaEm">A data da venda mais recente.</param>
/// <param name="CompradorChave">O GUID do comprador, quando ele está ao alcance de quem consulta.</param>
/// <param name="CompradorNome">O nome do comprador.</param>
/// <param name="ProdutoNaOrigem">O produto como a origem escreve.</param>
/// <param name="SistemaCodigo">O sistema de onde a venda veio.</param>
public sealed record UltimaVendaDaMaquina(
    int Vendas,
    DateOnly? VendidaEm,
    Guid? CompradorChave,
    string? CompradorNome,
    string? ProdutoNaOrigem,
    string? SistemaCodigo);

/// <summary>
/// Um cliente junto do que a tela precisa mostrar ao lado dele: os CÓDIGOS dos itens de catálogo
/// que ele referencia.
///
/// POR QUE A ENTIDADE SOZINHA NÃO BASTA: a entidade guarda <c>OrigemId</c>, um número. A tela
/// precisa do código estável (<c>SITE</c>, <c>INDICACAO</c>) para pré-selecionar o item no campo
/// de seleção, e a API não deve expor identificador interno de catálogo — o contrato público é o
/// código, que é o que nunca muda (documento 16, seção 3). A tradução acontece no repositório,
/// numa junção, e não em N consultas na tela.
/// </summary>
/// <param name="Cliente">A entidade.</param>
/// <param name="OrigemCodigo">O código do item de ORIGEM_LEAD, ou nulo.</param>
/// <param name="MotivoInativacaoCodigo">O código do item de MOTIVO_INATIVACAO, ou nulo.</param>
public sealed record ClienteComContexto(Cliente Cliente, string? OrigemCodigo, string? MotivoInativacaoCodigo);

/// <summary>
/// Uma máquina junto do que a tela precisa mostrar ao lado dela: a chave pública do dono e o
/// modelo resolvido com família e marca. Mesma razão de <see cref="ClienteComContexto"/>.
/// </summary>
/// <param name="Equipamento">A entidade.</param>
/// <param name="ClienteChave">O GUID público do dono, ou nulo quando a máquina está em estoque.</param>
/// <param name="ClienteNome">A razão social do dono, para a listagem não precisar de outra chamada.</param>
/// <param name="Modelo">O modelo resolvido, ou nulo se o catálogo perdeu a linha.</param>
/// <param name="Classificacao">A classificação de produto, quando a máquina tem uma.</param>
/// <param name="UltimaVenda">A venda mais recente e o comprador nela, quando a máquina tem venda.</param>
/// <param name="Divergencias">As divergências abertas da máquina — só na ficha, não na listagem.</param>
public sealed record EquipamentoComContexto(
    Equipamento Equipamento,
    Guid? ClienteChave,
    string? ClienteNome,
    ModeloParaSelecao? Modelo,
    ClassificacaoDaMaquina? Classificacao = null,
    UltimaVendaDaMaquina? UltimaVenda = null,
    IReadOnlyList<DivergenciaDaMaquina>? Divergencias = null);

/// <summary>Uma divergência aberta entre ART, CRM e Protheus sobre esta máquina.</summary>
/// <param name="Tipo">O tipo. Ex.: CompradorDiferenteDoProprietarioNoCrm.</param>
/// <param name="Descricao">O que é.</param>
/// <param name="DetectadaEm">Quando foi detectada (UTC).</param>
public sealed record DivergenciaDaMaquina(string Tipo, string Descricao, DateTime DetectadaEm);

/// <summary>
/// O acesso ao cadastro de clientes.
///
/// O QUE ESTA PORTA NÃO FAZ: decidir regra. Ela filtra o que o índice resolve e devolve
/// entidades; quem decide é o caso de uso e a própria entidade. É essa divisão que impede o
/// repositório de virar o lugar onde a lógica se esconde ([V] o defeito das 70 procedures que escrevem em
/// tabela no legado).
/// </summary>
public interface IRepositorioClientes
{
    /// <summary>Uma fatia da listagem, já dentro da fronteira de acesso de quem consultou.</summary>
    Task<PaginaDe<ClienteComContexto>> ListarAsync(ConsultaDeClientes consulta, CancellationToken ct);

    /// <summary>
    /// Um cliente pela chave pública. Devolve nulo quando não existe OU quando está fora do
    /// alcance de quem pediu — de propósito: distinguir os dois casos na resposta contaria a
    /// quem não pode ver que o registro existe.
    /// </summary>
    /// <param name="chavePublica">O GUID que a API expõe.</param>
    /// <param name="incluirInativos">Traz também o cliente já inativado.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<ClienteComContexto?> ObterAsync(Guid chavePublica, bool incluirInativos, CancellationToken ct);

    /// <summary>Põe um cliente novo na unidade de trabalho. A gravação é do <see cref="IUnidadeDeTrabalho"/>.</summary>
    Task AdicionarAsync(Cliente cliente, CancellationToken ct);

    /// <summary>
    /// Já existe outro cliente ativo com este documento nesta filial?
    ///
    /// O índice único <c>UX_Cliente_Empresa_Documento</c> garante isso no banco; esta consulta
    /// existe para que a recusa chegue como "o CNPJ já está no cliente X", e não como um erro
    /// de violação de índice que ninguém entende.
    /// </summary>
    /// <param name="documento">O CPF ou CNPJ já validado.</param>
    /// <param name="exceto">A chave do próprio cliente, numa alteração. Nulo numa criação.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<Cliente?> ObterPorDocumentoAsync(CpfCnpj documento, Guid? exceto, CancellationToken ct);
}

/// <summary>O acesso ao cadastro de máquinas. Mesma divisão de trabalho de <see cref="IRepositorioClientes"/>.</summary>
public interface IRepositorioEquipamentos
{
    /// <summary>Uma fatia da listagem, dentro da fronteira de acesso.</summary>
    Task<PaginaDe<EquipamentoComContexto>> ListarAsync(ConsultaDeEquipamentos consulta, CancellationToken ct);

    /// <summary>Uma máquina pela chave pública, ou nulo quando não existe ou está fora de alcance.</summary>
    /// <param name="chavePublica">O GUID que a API expõe.</param>
    /// <param name="incluirInativos">Traz também a máquina já baixada.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<EquipamentoComContexto?> ObterAsync(Guid chavePublica, bool incluirInativos, CancellationToken ct);

    /// <summary>Põe uma máquina nova na unidade de trabalho.</summary>
    Task AdicionarAsync(Equipamento equipamento, CancellationToken ct);

    /// <summary>Já existe outra máquina ativa com este chassi? O chassi é a chave de deduplicação.</summary>
    /// <param name="chassi">O chassi já validado.</param>
    /// <param name="exceto">A chave da própria máquina, numa alteração.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<Equipamento?> ObterPorChassiAsync(Chassi chassi, Guid? exceto, CancellationToken ct);
}

/// <summary>
/// As listas que alimentam os campos de seleção.
///
/// É a porta que torna executável a regra mais repetida do documento 16, seção 3.1: campo com
/// catálogo NÃO aceita digitação livre. A tela oferece só o que vem daqui; a API recusa o que
/// não está aqui; o banco tem a chave estrangeira composta. Os três, sempre juntos — nenhum
/// sozinho basta.
/// </summary>
public interface IRepositorioCatalogos
{
    /// <summary>
    /// Os catálogos e seus itens ativos, prontos para virar <c>&lt;select&gt;</c>.
    /// </summary>
    /// <param name="codigo">Um código de catálogo para trazer só ele; nulo traz todos.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<CatalogoParaSelecao>> ListarAsync(string? codigo, CancellationToken ct);

    /// <summary>
    /// Traduz o código de um item para o identificador interno, DENTRO do catálogo informado.
    /// Devolve nulo quando o item não existe naquele catálogo — que é a recusa do ponto 2 da
    /// regra 3.1: "mesmo que a tela tenha sido contornada".
    /// </summary>
    /// <param name="catalogoId">Uma das constantes de <see cref="CatalogosDeSistema"/>.</param>
    /// <param name="codigoDoItem">O código estável do item.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<int?> ResolverItemAsync(int catalogoId, string codigoDoItem, CancellationToken ct);

    /// <summary>O modelo de máquina pelo código estável, ou nulo se não existe.</summary>
    Task<ModeloParaSelecao?> ObterModeloAsync(string codigo, CancellationToken ct);

    /// <summary>A classificação de produto pelo código estável, ou nula se não existe ou está inativa.</summary>
    Task<LinhaDeProdutoParaSelecao?> ObterLinhaDeProdutoAsync(string codigo, CancellationToken ct);
}

/// <summary>
/// A gravação — o commit da unidade de trabalho.
///
/// POR QUE UMA PORTA SÓ PARA ISTO: para que o caso de uso possa gravar sem conhecer o
/// <c>CrmDbContext</c>, e para que a colisão de concorrência otimista (<c>rowversion</c>)
/// chegue como <see cref="Resultado{T}"/> em vez de exceção de infraestrutura vazando até a
/// borda. Duas pessoas salvando o mesmo registro é caso de NEGÓCIO, não defeito de programa.
/// </summary>
public interface IUnidadeDeTrabalho
{
    /// <summary>
    /// Grava tudo o que está pendente. Devolve quantas linhas foram afetadas, ou falha do tipo
    /// <see cref="TipoDeFalha.Concorrencia"/> quando alguém alterou o registro no meio do caminho.
    /// </summary>
    Task<Resultado<int>> SalvarAsync(CancellationToken ct);
}
