using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Por qual coluna a listagem de processos é ordenada. Mesma disciplina de <see cref="OrdemDeCliente"/>.</summary>
public enum OrdemDeProcesso
{
    /// <summary>Número do processo. É o que o usuário fala ao telefone.</summary>
    Numero = 0,

    /// <summary>Título, o que aparece no cartão do funil.</summary>
    Titulo = 1,

    /// <summary>Valor estimado do negócio.</summary>
    ValorEstimado = 2,

    /// <summary>Previsão de conclusão vigente.</summary>
    PrevisaoConclusao = 3,

    /// <summary>Há quanto tempo o processo está na fase atual.</summary>
    FaseDesde = 4,

    /// <summary>Data de abertura.</summary>
    CriadoEm = 5
}

/// <summary>Por qual coluna a agenda é ordenada.</summary>
public enum OrdemDeTarefa
{
    /// <summary>A data agendada. É a ordem que a agenda abre.</summary>
    AgendadaPara = 0,

    /// <summary>Prioridade, de 1 (alta) a 5 (baixa).</summary>
    Prioridade = 1,

    /// <summary>Prazo limite.</summary>
    PrazoLimite = 2,

    /// <summary>Assunto.</summary>
    Assunto = 3
}

/// <summary>Por qual coluna a tela de Cobertura é ordenada.</summary>
public enum OrdemDeCobertura
{
    /// <summary>Data do último contato. É a ordem que a Cobertura abre: quem está há mais tempo sem contato.</summary>
    UltimaInteracaoEm = 0,

    /// <summary>Classe do cliente na carteira.</summary>
    Classe = 1,

    /// <summary>Razão social do cliente.</summary>
    Nome = 2
}

/// <summary>
/// O que a listagem de processos pede.
///
/// Mesma disciplina de <see cref="ConsultaDeClientes"/>: a fronteira de empresa NÃO é parâmetro
/// desta consulta — ela é invariante do <c>CrmDbContext</c>, e não existe caminho por aqui que a
/// desligue.
/// </summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="Termo">Busca por título ou número.</param>
/// <param name="Situacao">Filtro por situação no funil.</param>
/// <param name="TipoProcessoId">Filtro por modelo de fluxo.</param>
/// <param name="FaseId">Filtro por fase. É o que a coluna do kanban usa.</param>
/// <param name="ClienteId">Filtro pelo cliente. É o que a Visão 360 usa.</param>
/// <param name="ProprietarioId">Filtro por quem responde pelo processo.</param>
/// <param name="AbertoDe">Data mínima de abertura.</param>
/// <param name="AbertoAte">Data máxima de abertura.</param>
/// <param name="Ordem">Coluna de ordenação.</param>
/// <param name="Descendente">Ordem decrescente.</param>
/// <param name="IncluirEncerrados">Traz também os processos já encerrados. Padrão é não trazer.</param>
public sealed record ConsultaDeProcessos(
    Paginacao Paginacao,
    string? Termo = null,
    SituacaoDoProcesso? Situacao = null,
    int? TipoProcessoId = null,
    int? FaseId = null,
    long? ClienteId = null,
    long? ProprietarioId = null,
    DateOnly? AbertoDe = null,
    DateOnly? AbertoAte = null,
    OrdemDeProcesso Ordem = OrdemDeProcesso.FaseDesde,
    bool Descendente = true,
    bool IncluirEncerrados = false);

/// <summary>
/// Um processo junto do que a tela mostra ao lado dele — cliente, fluxo, fase e dono já
/// resolvidos.
///
/// Pela mesma razão de <see cref="ClienteComContexto"/>: a alternativa é a tela fazer uma
/// chamada por cartão do funil, e um funil de 25 cartões vira 100 requisições.
/// </summary>
/// <param name="Processo">A entidade.</param>
/// <param name="ClienteChave">O GUID público do cliente.</param>
/// <param name="ClienteNome">A razão social do cliente.</param>
/// <param name="TipoProcessoCodigo">O código do modelo de fluxo.</param>
/// <param name="TipoProcessoNome">O nome do modelo de fluxo.</param>
/// <param name="FaseCodigo">O código da fase atual.</param>
/// <param name="FaseNome">O nome da fase atual.</param>
/// <param name="FaseOrdem">A posição da fase na barra.</param>
/// <param name="ProprietarioNome">Quem responde pelo processo.</param>
/// <param name="MotivoDePerdaCodigo">O motivo da perda, quando o processo foi perdido.</param>
public sealed record ProcessoComContexto(
    Processo.Processo Processo,
    Guid ClienteChave,
    string ClienteNome,
    string TipoProcessoCodigo,
    string TipoProcessoNome,
    string FaseCodigo,
    string FaseNome,
    short FaseOrdem,
    string? ProprietarioNome,
    string? MotivoDePerdaCodigo);

/// <summary>
/// Uma coluna do funil, contada NO BANCO.
///
/// <para>A contagem de valor vem separada da contagem de linhas de propósito: elas são
/// diferentes, e a diferença é a informação. [V] o valor do processo é preenchido em menos de 1%
/// dos casos no sistema de origem — somar essa coluna e apresentar o total como "valor do funil"
/// seria mostrar um número que representa 1% da realidade sem dizer isso.</para>
/// </summary>
/// <param name="TipoProcessoCodigo">O fluxo a que a fase pertence.</param>
/// <param name="TipoProcessoNome">O nome do fluxo.</param>
/// <param name="FaseCodigo">O código da fase.</param>
/// <param name="FaseNome">O nome da fase.</param>
/// <param name="FaseOrdem">A posição da fase na barra.</param>
/// <param name="Processos">Quantos processos abertos estão nesta fase.</param>
/// <param name="ProcessosComValor">Quantos deles declaram valor.</param>
/// <param name="ValorTotal">A soma dos que declaram valor. Nulo quando nenhum declara.</param>
public sealed record FatiaDoFunil(
    string TipoProcessoCodigo,
    string TipoProcessoNome,
    string FaseCodigo,
    string FaseNome,
    short FaseOrdem,
    int Processos,
    int ProcessosComValor,
    decimal? ValorTotal);

/// <summary>Uma contagem por rótulo, calculada no banco — o formato dos agregados simples.</summary>
/// <param name="Codigo">O código estável do agrupador.</param>
/// <param name="Nome">O nome legível.</param>
/// <param name="Quantidade">Quantas linhas.</param>
public sealed record ContagemPorRotulo(string Codigo, string Nome, int Quantidade);

/// <summary>O que a agenda pede.</summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="ResponsavelId">De quem é a agenda.</param>
/// <param name="Situacao">Filtro por situação da tarefa.</param>
/// <param name="ClienteId">Filtro pelo cliente.</param>
/// <param name="ProcessoId">Filtro pelo processo.</param>
/// <param name="De">Data mínima de agendamento.</param>
/// <param name="Ate">Data máxima de agendamento.</param>
/// <param name="SomenteAtrasadas">Só o que já passou da data e não foi concluído.</param>
/// <param name="Ordem">Coluna de ordenação.</param>
/// <param name="Descendente">Ordem decrescente.</param>
public sealed record ConsultaDeTarefas(
    Paginacao Paginacao,
    long? ResponsavelId = null,
    SituacaoDaTarefa? Situacao = null,
    long? ClienteId = null,
    long? ProcessoId = null,
    DateOnly? De = null,
    DateOnly? Ate = null,
    bool SomenteAtrasadas = false,
    OrdemDeTarefa Ordem = OrdemDeTarefa.AgendadaPara,
    bool Descendente = false);

/// <summary>Uma tarefa junto do que a agenda mostra ao lado dela.</summary>
/// <param name="Tarefa">A entidade.</param>
/// <param name="ClienteChave">O GUID público do cliente, quando há.</param>
/// <param name="ClienteNome">A razão social do cliente.</param>
/// <param name="ProcessoChave">O GUID público do processo, quando há.</param>
/// <param name="ProcessoTitulo">O título do processo.</param>
/// <param name="TipoTarefaCodigo">O código do tipo de tarefa.</param>
/// <param name="TipoTarefaNome">O nome do tipo de tarefa.</param>
/// <param name="ResponsavelNome">Quem tem que fazer.</param>
/// <param name="ResultadoNome">O desfecho registrado, quando concluída.</param>
public sealed record TarefaComContexto(
    Tarefa Tarefa,
    Guid? ClienteChave,
    string? ClienteNome,
    Guid? ProcessoChave,
    string? ProcessoTitulo,
    string TipoTarefaCodigo,
    string TipoTarefaNome,
    string ResponsavelNome,
    string? ResultadoNome);

/// <summary>
/// O painel do CEN, contado no banco — a resposta a "como está a minha agenda".
/// </summary>
/// <param name="Pendentes">Tarefas ainda não concluídas.</param>
/// <param name="Atrasadas">Pendentes cuja data já passou.</param>
/// <param name="ParaHoje">Pendentes agendadas para hoje.</param>
/// <param name="ProximosSeteDias">Pendentes agendadas para os próximos sete dias.</param>
/// <param name="ConcluidasNosUltimosTrintaDias">Concluídas no último mês.</param>
/// <param name="SemPrazoLimite">Pendentes sem prazo limite declarado.</param>
/// <param name="MaisAntigaPendenteEm">A data da pendência mais velha. É a idade do passivo.</param>
public sealed record PainelDaAgenda(
    int Pendentes,
    int Atrasadas,
    int ParaHoje,
    int ProximosSeteDias,
    int ConcluidasNosUltimosTrintaDias,
    int SemPrazoLimite,
    DateTime? MaisAntigaPendenteEm);

/// <summary>O que a linha do tempo pede.</summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="ClienteId">Filtro pelo cliente. É o que a Visão 360 usa.</param>
/// <param name="ProcessoId">Filtro pelo processo.</param>
/// <param name="AutorId">Filtro por quem registrou.</param>
/// <param name="Natureza">Filtro por quem procurou quem.</param>
/// <param name="De">Data mínima de ocorrência.</param>
/// <param name="Ate">Data máxima de ocorrência.</param>
public sealed record ConsultaDeInteracoes(
    Paginacao Paginacao,
    long? ClienteId = null,
    long? ProcessoId = null,
    long? AutorId = null,
    NaturezaDaInteracao? Natureza = null,
    DateOnly? De = null,
    DateOnly? Ate = null);

/// <summary>Uma interação junto do que a linha do tempo mostra ao lado dela.</summary>
/// <param name="Interacao">A entidade.</param>
/// <param name="ClienteChave">O GUID público do cliente, quando há.</param>
/// <param name="ClienteNome">A razão social do cliente.</param>
/// <param name="ProcessoChave">O GUID público do processo, quando há.</param>
/// <param name="TipoTarefaNome">O tipo de contato.</param>
/// <param name="ResultadoNome">O desfecho registrado.</param>
/// <param name="AutorNome">Quem registrou.</param>
public sealed record InteracaoComContexto(
    Interacao Interacao,
    Guid? ClienteChave,
    string? ClienteNome,
    Guid? ProcessoChave,
    string TipoTarefaNome,
    string? ResultadoNome,
    string AutorNome);

/// <summary>O que a tela de Cobertura pede.</summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="CarteiraId">Filtro pela carteira.</param>
/// <param name="ResponsavelId">Filtro pelo CEN responsável pela carteira.</param>
/// <param name="Classe">Filtro pela classe do cliente na carteira.</param>
/// <param name="DiasSemContato">Só quem está há mais dias que isto sem contato.</param>
/// <param name="SomenteSemContato">Só quem nunca foi contatado.</param>
/// <param name="Ordem">Coluna de ordenação.</param>
/// <param name="Descendente">Ordem decrescente.</param>
public sealed record ConsultaDeCobertura(
    Paginacao Paginacao,
    long? CarteiraId = null,
    long? ResponsavelId = null,
    ClasseDeCliente? Classe = null,
    int? DiasSemContato = null,
    bool SomenteSemContato = false,
    OrdemDeCobertura Ordem = OrdemDeCobertura.UltimaInteracaoEm,
    bool Descendente = false);

/// <summary>
/// Uma linha da tela de Cobertura: o cliente na carteira, com quanto tempo faz que ninguém fala
/// com ele.
/// </summary>
/// <param name="ClienteChave">O GUID público do cliente.</param>
/// <param name="ClienteNome">A razão social.</param>
/// <param name="CarteiraChave">O GUID público da carteira.</param>
/// <param name="CarteiraNome">O nome da carteira.</param>
/// <param name="LinhaDeNegocioNome">A linha de negócio da carteira.</param>
/// <param name="Classe">A classe do cliente nesta carteira.</param>
/// <param name="UltimaInteracaoEm">Quando foi o último contato. Nulo é "nunca".</param>
/// <param name="DiasCicloContato">A cadência esperada, quando declarada.</param>
/// <param name="ResponsavelNome">O CEN responsável pela carteira.</param>
public sealed record LinhaDeCobertura(
    Guid ClienteChave,
    string ClienteNome,
    Guid CarteiraChave,
    string CarteiraNome,
    string LinhaDeNegocioNome,
    ClasseDeCliente Classe,
    DateTime? UltimaInteracaoEm,
    short? DiasCicloContato,
    string ResponsavelNome);

/// <summary>A cobertura de uma carteira inteira, contada no banco.</summary>
/// <param name="CarteiraChave">O GUID público da carteira.</param>
/// <param name="CarteiraCodigo">O código da carteira.</param>
/// <param name="CarteiraNome">O nome da carteira.</param>
/// <param name="LinhaDeNegocioNome">A linha de negócio.</param>
/// <param name="ResponsavelNome">O CEN responsável.</param>
/// <param name="Clientes">Quantos clientes a carteira tem.</param>
/// <param name="ComContatoEm30Dias">Quantos tiveram contato nos últimos 30 dias.</param>
/// <param name="ComContatoEm90Dias">Quantos tiveram contato nos últimos 90 dias.</param>
/// <param name="NuncaContatados">Quantos nunca tiveram contato registrado.</param>
/// <param name="UltimoContatoEm">O contato mais recente da carteira inteira.</param>
/// <param name="NaturezaDaCarteira">Se a carteira é Comercial, Administrativa ou Teste.</param>
/// <param name="NaturezaDoResponsavel">
/// O que o dono da carteira é: <c>Pessoa</c>, <c>Departamento</c>, <c>Sistema</c>,
/// <c>Fornecedor</c> ou <c>Teste</c>.
///
/// <para>A área ENTRA nos números — a carteira da Inteligência de Mercado tem 5.000 clientes
/// reais e a cobertura deles importa. O que a natureza faz é permitir que a tela diga que
/// aquilo é uma área, para ninguém comparar o volume de um time com o de uma pessoa sem saber.
/// Fora ficam só sistema, fornecedor e teste, que não são operação.</para>
/// </param>
public sealed record ResumoDeCobertura(
    Guid CarteiraChave,
    string CarteiraCodigo,
    string CarteiraNome,
    string LinhaDeNegocioNome,
    string ResponsavelNome,
    int Clientes,
    int ComContatoEm30Dias,
    int ComContatoEm90Dias,
    int NuncaContatados,
    DateTime? UltimoContatoEm,
    string NaturezaDaCarteira,
    string NaturezaDoResponsavel);

/// <summary>
/// O acesso a processos — a leitura que sustenta o Pipeline e o Funil.
///
/// Mesma divisão de trabalho de <see cref="IRepositorioClientes"/>: filtra o que o índice
/// resolve e devolve leitura. O agregado é calculado NO BANCO, e não montado na tela — somar
/// 45 mil processos no navegador seria trazer 45 mil linhas para somar quinze números.
/// </summary>
public interface IRepositorioProcessos
{
    /// <summary>Uma fatia da listagem, já dentro da fronteira de acesso.</summary>
    Task<PaginaDe<ProcessoComContexto>> ListarAsync(ConsultaDeProcessos consulta, CancellationToken ct);

    /// <summary>Um processo pela chave pública, ou nulo quando não existe ou está fora de alcance.</summary>
    /// <param name="chavePublica">O GUID que a API expõe.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<ProcessoComContexto?> ObterAsync(Guid chavePublica, CancellationToken ct);

    /// <summary>O funil por fase, agrupado e somado pelo banco.</summary>
    /// <param name="tipoProcessoId">Um fluxo para ver só ele; nulo traz todos.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<FatiaDoFunil>> ResumirFunilAsync(int? tipoProcessoId, CancellationToken ct);

    /// <summary>As perdas por motivo, contadas pelo banco.</summary>
    Task<IReadOnlyList<ContagemPorRotulo>> ResumirPerdasAsync(CancellationToken ct);
}

/// <summary>O acesso à agenda — a leitura que sustenta a Agenda e o painel do CEN.</summary>
public interface IRepositorioTarefas
{
    /// <summary>Uma fatia da agenda, já dentro da fronteira de acesso.</summary>
    Task<PaginaDe<TarefaComContexto>> ListarAsync(ConsultaDeTarefas consulta, CancellationToken ct);

    /// <summary>Os números do painel, contados pelo banco.</summary>
    /// <param name="responsavelId">De quem é a agenda; nulo conta a filial inteira.</param>
    /// <param name="agoraUtc">O instante de referência, para o teste poder fixar "hoje".</param>
    /// <param name="ct">Cancelamento.</param>
    Task<PainelDaAgenda> ResumirAgendaAsync(long? responsavelId, DateTime agoraUtc, CancellationToken ct);
}

/// <summary>O acesso à linha do tempo — a leitura que sustenta a Visão 360 e a Cobertura.</summary>
public interface IRepositorioInteracoes
{
    /// <summary>Uma fatia da linha do tempo, já dentro da fronteira de acesso.</summary>
    Task<PaginaDe<InteracaoComContexto>> ListarAsync(ConsultaDeInteracoes consulta, CancellationToken ct);
}

/// <summary>O acesso à carteirização — a leitura que sustenta a tela de Cobertura.</summary>
public interface IRepositorioCarteiras
{
    /// <summary>Uma fatia da cobertura, cliente a cliente.</summary>
    Task<PaginaDe<LinhaDeCobertura>> ListarCoberturaAsync(ConsultaDeCobertura consulta, CancellationToken ct);

    /// <summary>A cobertura por carteira, contada pelo banco.</summary>
    /// <param name="agoraUtc">O instante de referência das janelas de 30 e 90 dias.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<ResumoDeCobertura>> ResumirCoberturaAsync(DateTime agoraUtc, CancellationToken ct);
}

/// <summary>
/// Uma fatia do relatório de vendas perdidas — quantas, e a que distância de preço.
/// </summary>
/// <param name="Codigo">Código estável do agrupador (motivo ou concorrente).</param>
/// <param name="Nome">Nome legível.</param>
/// <param name="Quantidade">Quantas vendas perdidas caem aqui.</param>
/// <param name="Maquinas">A soma das quantidades declaradas — uma perda pode ser de três tratores.</param>
/// <param name="ComOsDoisPrecos">
/// Em quantas dessas os DOIS preços foram declarados. É o denominador de
/// <paramref name="DiferencaMediaDePreco"/>, e ele vem junto de propósito: média sem denominador
/// é o defeito que este projeto existe para não repetir.
/// </param>
/// <param name="DiferencaMediaDePreco">
/// Quanto o nosso preço ficou acima do concorrente, em média, nas linhas com os dois preços.
/// Nulo quando nenhuma linha do grupo declarou os dois.
/// </param>
public sealed record FatiaDeVendaPerdida(
    string Codigo,
    string Nome,
    int Quantidade,
    int Maquinas,
    int ComOsDoisPrecos,
    decimal? DiferencaMediaDePreco);

/// <summary>
/// O acesso às vendas perdidas — o que o formulário do CEN registrou sobre cada derrota.
///
/// <para>É porta separada de <see cref="IRepositorioProcessos"/> porque responde a outra
/// pergunta e sobre outra população: <c>ResumirPerdasAsync</c> conta PROCESSOS marcados como
/// perdidos, e isto conta os FORMULÁRIOS preenchidos sobre eles. Os dois números são diferentes
/// e a diferença é a informação — quantas derrotas ninguém registrou.</para>
/// </summary>
public interface IRepositorioVendasPerdidas
{
    /// <summary>Quantas vendas perdidas há ao alcance deste contexto.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<int> ContarAsync(CancellationToken ct);

    /// <summary>As vendas perdidas por motivo.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<FatiaDeVendaPerdida>> ResumirPorMotivoAsync(CancellationToken ct);

    /// <summary>As vendas perdidas por fabricante concorrente — para quem se perdeu.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<FatiaDeVendaPerdida>> ResumirPorConcorrenteAsync(CancellationToken ct);
}

/// <summary>
/// A cobertura de uma classe de cliente dentro da carteira de um responsável.
/// </summary>
/// <param name="Classe">A letra da curva ABC: <c>A</c>, <c>B</c>, <c>C</c> ou <c>D</c>.</param>
/// <param name="Clientes">Quantos vínculos caem nesta classe.</param>
/// <param name="Cobertos">
/// Quantos tiveram contato DENTRO da cadência declarada para a classe naquela linha de negócio.
/// </param>
/// <param name="ForaDaCadencia">
/// Quantos tiveram contato, mas há mais tempo do que a cadência permite. É a fila de trabalho do
/// CEN: são clientes que ele conhece e deixou passar do prazo.
/// </param>
/// <param name="NuncaContatados">Quantos não têm nenhuma interação registrada.</param>
/// <param name="SemCadenciaDeclarada">
/// Quantos estão em linha de negócio que não declara cadência nenhuma. Não são "cobertos" nem
/// "atrasados": não há prazo contra o que medi-los, e somá-los a qualquer um dos dois lados
/// inventaria uma meta.
/// </param>
/// <param name="DiasDeCadencia">A cadência da classe naquela carteira, quando declarada.</param>
public sealed record CoberturaPorClasse(
    string Classe,
    int Clientes,
    int Cobertos,
    int ForaDaCadencia,
    int NuncaContatados,
    int SemCadenciaDeclarada,
    short? DiasDeCadencia);

/// <summary>
/// O painel de um responsável — o que ele cobre, o que deixou passar e o que fechou.
/// </summary>
/// <param name="ResponsavelChave">A chave pública do responsável.</param>
/// <param name="ResponsavelNome">O nome que aparece na tela.</param>
/// <param name="NaturezaDoResponsavel">Se é <c>Pessoa</c> ou <c>Departamento</c>.</param>
/// <param name="Carteiras">Quantas carteiras comerciais ele responde.</param>
/// <param name="Clientes">Quantos vínculos cliente × carteira ele tem.</param>
/// <param name="PorClasse">A cobertura, quebrada por classe da curva ABC.</param>
/// <param name="ProcessosGanhos">Processos encerrados com venda no recorte.</param>
/// <param name="ProcessosPerdidos">Processos encerrados sem venda no recorte.</param>
/// <param name="ProcessosAbertos">Processos em andamento.</param>
/// <param name="VendasPerdidasRegistradas">Formulários de venda perdida que ele preencheu.</param>
/// <param name="FaturamentoDaCarteira">
/// O faturamento somado dos clientes da carteira dele, na janela apurada. É do CLIENTE, e não do
/// vendedor: a origem não diz quem vendeu cada nota, e atribuir a venda ao dono atual da carteira
/// daria crédito a quem talvez nem estivesse na empresa quando ela aconteceu.
/// </param>
public sealed record PainelDoResponsavel(
    Guid ResponsavelChave,
    string ResponsavelNome,
    string NaturezaDoResponsavel,
    int Carteiras,
    int Clientes,
    IReadOnlyList<CoberturaPorClasse> PorClasse,
    int ProcessosGanhos,
    int ProcessosPerdidos,
    int ProcessosAbertos,
    int VendasPerdidasRegistradas,
    decimal FaturamentoDaCarteira);

/// <summary>
/// O acesso ao painel por responsável — a leitura que sustenta o filtro por CEN.
/// </summary>
public interface IRepositorioPainelDoCen
{
    /// <summary>Os responsáveis que têm carteira comercial, para o seletor da tela.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<(Guid Chave, string Nome, string Natureza, int Carteiras)>> ListarResponsaveisAsync(
        CancellationToken ct);

    /// <summary>
    /// O painel de um responsável.
    /// </summary>
    /// <param name="responsavelChave">A chave pública dele; nulo traz o consolidado de todos.</param>
    /// <param name="agoraUtc">O instante contra o qual a cadência é medida.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<PainelDoResponsavel?> ObterPainelAsync(
        Guid? responsavelChave, DateTime agoraUtc, CancellationToken ct);
}

/// <summary>Um mês da série de faturamento.</summary>
/// <param name="Competencia">O primeiro dia do mês.</param>
/// <param name="ValorLiquido">O que foi faturado no mês.</param>
/// <param name="Clientes">Quantos clientes distintos compraram.</param>
/// <param name="Notas">Quantas notas fiscais.</param>
public sealed record MesDeFaturamento(
    DateOnly Competencia, decimal ValorLiquido, int Clientes, int Notas);

/// <summary>Um cliente no ranking de faturamento.</summary>
/// <param name="ClienteChave">A chave pública, para a tela linkar a ficha.</param>
/// <param name="Nome">A razão social.</param>
/// <param name="Classe">A letra da curva ABC, quando apurada.</param>
/// <param name="ValorLiquido">O faturamento acumulado na janela.</param>
/// <param name="UltimaCompraEm">O mês da compra mais recente.</param>
public sealed record ClienteNoRanking(
    Guid ClienteChave, string Nome, string? Classe, decimal ValorLiquido, DateOnly? UltimaCompraEm);

/// <summary>
/// O acesso ao faturamento — a leitura que sustenta a série de doze meses e o ranking de clientes.
/// </summary>
public interface IRepositorioFaturamento
{
    /// <summary>
    /// A série mensal dos últimos meses, do mais antigo para o mais novo.
    /// </summary>
    /// <param name="meses">Quantos meses trazer, contados da competência mais recente que existe.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<MesDeFaturamento>> SerieMensalAsync(int meses, CancellationToken ct);

    /// <summary>
    /// Os maiores clientes por faturamento acumulado.
    /// </summary>
    /// <param name="quantos">Quantos trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<ClienteNoRanking>> TopClientesAsync(int quantos, CancellationToken ct);

    /// <summary>
    /// A competência mais recente com faturamento, ou nula quando não há nenhum.
    ///
    /// <para>É o que permite a tela escrever o período em vez de dizer "faturamento do mês" — e o
    /// que denuncia, sozinho, se a carga do ERP parar de novo.</para>
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    Task<DateOnly?> CompetenciaMaisRecenteAsync(CancellationToken ct);
}
