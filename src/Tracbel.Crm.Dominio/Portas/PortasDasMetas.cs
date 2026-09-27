using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// QUEM A LEITURA DA META ENXERGA (D-M5, 27/09/2026) — decidido pela profundidade de <c>Meta.Ler</c>.
/// </summary>
public enum AlcanceDaMeta
{
    /// <summary>
    /// Só a própria meta e o próprio realizado: a meta cujo consultor é a pessoa (a conta casada pela carga, ou o
    /// login em maiúsculas) e as vendas cujo vendedor do ART é ela. É o vendedor, no perfil Padrão.
    /// </summary>
    Proprios = 0,

    /// <summary>A filial do pedido inteira, com os consultores que não têm conta no CRM. A gerência e a diretoria.</summary>
    Filial = 1
}

/// <summary>Um mês: a meta de máquinas, o realizado e a meta de consórcio (em cotas, à parte).</summary>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="MetaMaquinas">A meta de máquinas, em unidades.</param>
/// <param name="RealizadoMaquinas">As máquinas vendidas (<c>frota.VendaDeMaquina</c>, pela data da venda).</param>
/// <param name="MetaConsorcio">A meta de consórcio, em cotas.</param>
public sealed record MetaERealizadoNoMes(DateOnly Competencia, int MetaMaquinas, int RealizadoMaquinas, int MetaConsorcio);

/// <summary>Uma linha de produto: a meta e o realizado, casados pelo código (o algoritmo do ART dos dois lados).</summary>
/// <param name="Codigo">O código estável da linha (<c>TRATOR_MEDIO</c>).</param>
/// <param name="Nome">A linha como a origem escreve.</param>
/// <param name="Meta">A meta em unidades; zero quando a linha só tem venda.</param>
/// <param name="Realizado">As vendas da linha.</param>
public sealed record MetaERealizadoNaLinha(string Codigo, string Nome, int Meta, int Realizado);

/// <summary>Um consultor: a meta e o realizado dele (pela pessoa — o vendedor do ART, D-M2).</summary>
/// <param name="Consultor">O consultor como a GN escreve (<c>NOME.SOBRENOME</c>).</param>
/// <param name="TemConta">Se ele tem conta no CRM (e, por isso, vê a própria meta).</param>
/// <param name="Meta">A meta de máquinas, em unidades.</param>
/// <param name="Realizado">As vendas em que ele é o vendedor.</param>
public sealed record MetaERealizadoDoConsultor(string Consultor, bool TemConta, int Meta, int Realizado);

/// <summary>De onde a meta veio e quão fresca ela está.</summary>
/// <param name="Sistema">O sistema de origem.</param>
/// <param name="Rota">A rota lida.</param>
/// <param name="LidaEm">A última leitura do CRM (UTC).</param>
/// <param name="GeradaNaOrigemEm">Quando a API gerou a resposta dessa leitura (UTC).</param>
public sealed record OrigemDaMetaDeVenda(string Sistema, string Rota, DateTime LidaEm, DateTime? GeradaNaOrigemEm);

/// <summary>O que o repositório apura para uma filial (ou para a própria pessoa), num período.</summary>
/// <param name="MetaMaquinas">A meta de máquinas no período.</param>
/// <param name="RealizadoMaquinas">As máquinas vendidas no período.</param>
/// <param name="PendentesNoArt">As vendas do ART no período que aguardam cadastro ou chassi — nulo no alcance Próprios,
/// porque a venda pendente ainda não tem a pessoa.</param>
/// <param name="PendentesSemFilial">As pendentes do período cuja unidade não tem filial no CRM — de nenhuma filial.</param>
/// <param name="MetaConsorcio">A meta de consórcio no período, em cotas.</param>
/// <param name="VendasSemVendedor">As vendas do período sem vendedor no ART — não entram em consultor nenhum.</param>
/// <param name="ConsultoresSemConta">Os consultores com meta no período que não têm conta no CRM.</param>
/// <param name="UltimaCompetenciaComMeta">O último mês com meta cadastrada, no alcance — diz até onde o cadastro vai.</param>
/// <param name="PorMes">Mês a mês, no período.</param>
/// <param name="PorLinha">Por linha de produto, no período.</param>
/// <param name="PorConsultor">Por consultor, no período.</param>
/// <param name="RealizadoNoAnterior">As máquinas vendidas no mesmo trecho do ano fiscal anterior.</param>
/// <param name="MesEmCurso">O mês em curso, quando o pedido o deixou de fora — meta e realizado até aqui.</param>
/// <param name="Origem">A última leitura do cadastro; nula quando a carga nunca rodou.</param>
public sealed record MetaERealizadoApurado(
    int MetaMaquinas,
    int RealizadoMaquinas,
    int? PendentesNoArt,
    int PendentesSemFilial,
    int MetaConsorcio,
    int VendasSemVendedor,
    int ConsultoresSemConta,
    DateOnly? UltimaCompetenciaComMeta,
    IReadOnlyList<MetaERealizadoNoMes> PorMes,
    IReadOnlyList<MetaERealizadoNaLinha> PorLinha,
    IReadOnlyList<MetaERealizadoDoConsultor> PorConsultor,
    int RealizadoNoAnterior,
    MetaERealizadoNoMes? MesEmCurso,
    OrigemDaMetaDeVenda? Origem);

/// <summary>
/// A META DE VENDA × O REALIZADO, para a filial do contexto de acesso (#138). Como todo repositório, sem <c>Where</c> de
/// empresa: a meta e a venda entram pelo filtro global.
/// </summary>
public interface IRepositorioDeMetas
{
    /// <summary>Apura a meta e o realizado.</summary>
    /// <param name="periodo">Os meses pedidos.</param>
    /// <param name="anterior">O mesmo trecho do ano fiscal anterior — só o realizado.</param>
    /// <param name="mesEmCurso">O mês em curso quando ele ficou fora do período; nulo quando não ficou.</param>
    /// <param name="alcance">A filial inteira, ou só a própria pessoa.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<MetaERealizadoApurado> ApurarAsync(
        JanelaDeCompetencia periodo, JanelaDeCompetencia anterior, DateOnly? mesEmCurso, AlcanceDaMeta alcance, CancellationToken ct);
}
