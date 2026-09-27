using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// AS DUAS LEITURAS DO FUNIL (decisão do Ricardo de 27/09/2026, documento 52 §3).
///
/// <para><b>Coorte (pela abertura)</b> é o padrão da tela: os processos ABERTOS no período e até onde cada um chegou —
/// responde "de cada 100 leads que entraram, quantos viraram pedido". <b>Fluxo (pela etapa)</b> conta as etapas
/// ALCANÇADAS no período, seja qual for a abertura — responde "quantos pedidos saíram neste trimestre", e é o que a
/// Performance de CEN e os alertas usam. Os dois números são diferentes de propósito (FY26: 1.347 faturados na coorte,
/// 1.835 no fluxo), e a tela diz qual está mostrando.</para>
/// </summary>
public enum BaseDoFunil
{
    /// <summary>A coorte: o processo entra pela data de abertura (<c>AbertoEm</c>).</summary>
    Abertura = 0,

    /// <summary>O fluxo: a etapa entra pela data em que foi alcançada (<c>AlcancadoEm</c>).</summary>
    Etapa = 1
}

/// <summary>O que a tela pediu ao funil.</summary>
/// <param name="DeUtc">O começo do período (UTC, inclusive).</param>
/// <param name="AteUtc">O fim do período (UTC, exclusive).</param>
/// <param name="Base">Coorte ou fluxo.</param>
/// <param name="Carteira">A carteira escolhida, pela chave pública; nula para todas.</param>
/// <param name="Responsavel">O responsável escolhido, pela chave pública; nulo para todos.</param>
/// <param name="ParadoDesdeAntesDeUtc">
/// O corte do alerta: o processo cujo estágio mais avançado é Negociação ou Pedido, alcançado ANTES deste instante e ainda
/// aberto, está parado. Não depende do período — é o estado de hoje.
/// </param>
public sealed record ConsultaDoFunilPorEstagio(
    DateTime DeUtc, DateTime AteUtc, BaseDoFunil Base, Guid? Carteira, Guid? Responsavel, DateTime ParadoDesdeAntesDeUtc);

/// <summary>Um estágio contado no banco — as linhas do estágio que caem no período, pelos desfechos do processo.</summary>
/// <param name="Estagio">O estágio.</param>
/// <param name="Processos">Quantos processos, um por linha (o índice único é processo × estágio).</param>
/// <param name="PelaEntradaDigital">Quantos alcançaram o estágio pela entrada digital (1278/3803).</param>
/// <param name="Ganhos">Quantos desses processos terminaram ganhos.</param>
/// <param name="Perdidos">Quantos terminaram perdidos.</param>
/// <param name="Abertos">Quantos continuam abertos.</param>
/// <param name="Outros">Cancelados e suspensos.</param>
public sealed record EstagioContado(
    EstagioDoFunil Estagio, int Processos, int PelaEntradaDigital, int Ganhos, int Perdidos, int Abertos, int Outros);

/// <summary>A última execução da rotina que traz o funil — o carimbo de procedência dele.</summary>
/// <param name="Nome">O nome da rotina, como a tela de Integrações o mostra.</param>
/// <param name="EstaLigada">Se a agenda está ligada.</param>
/// <param name="IniciadaEm">Quando a última execução começou (UTC); nulo quando nunca rodou.</param>
/// <param name="TerminadaEm">Quando ela terminou (UTC).</param>
/// <param name="Resultado">Como terminou (<c>Sucesso</c>, <c>Falha</c>…); nulo quando nunca rodou.</param>
public sealed record ExecucaoDaRotinaDoFunil(string Nome, bool EstaLigada, DateTime? IniciadaEm, DateTime? TerminadaEm, string? Resultado);

/// <summary>O funil como o banco o apura.</summary>
/// <param name="Estagios">Os seis estágios, na ordem, com zero onde não há linha.</param>
/// <param name="ParadosEmNegociacaoOuPedido">Os processos parados — ver <see cref="ConsultaDoFunilPorEstagio.ParadoDesdeAntesDeUtc"/>.</param>
/// <param name="LinhasNaFilial">Quantas linhas o funil tem na filial, sem filtro nenhum: zero é "a rotina não trouxe nada".</param>
/// <param name="Rotina">A rotina <c>PROCESSOS_VORTICE</c>; nula só se o catálogo não a tiver.</param>
public sealed record FunilPorEstagioApurado(
    IReadOnlyList<EstagioContado> Estagios, int ParadosEmNegociacaoOuPedido, int LinhasNaFilial, ExecucaoDaRotinaDoFunil? Rotina);

/// <summary>
/// O acesso ao funil por estágio (<c>processo.EstagioDoProcesso</c>). A fronteira de filial é o filtro global.
/// </summary>
public interface IRepositorioFunilPorEstagio
{
    /// <summary>Apura o funil; nulo quando a carteira ou o responsável pedidos não existem ao alcance.</summary>
    /// <param name="consulta">O pedido.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<FunilPorEstagioApurado?> ApurarAsync(ConsultaDoFunilPorEstagio consulta, CancellationToken ct);
}

/// <summary>O recorte das vendas perdidas.</summary>
/// <param name="DeUtc">O começo do período (UTC, inclusive), pela data em que o formulário foi preenchido.</param>
/// <param name="AteUtc">O fim do período (UTC, exclusive).</param>
/// <param name="Formulario">O formulário de origem (<see cref="FormulariosDaVendaPerdida"/>); nulo para todos.</param>
/// <param name="Responsavel">O responsável pelo processo no funil, pela chave pública; nulo para todos.</param>
public sealed record FiltroDeVendaPerdida(DateTime DeUtc, DateTime AteUtc, string? Formulario, Guid? Responsavel);
