using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O CRITÉRIO DE DATA DAS VENDAS DO ART (D-P08.1) num lugar só: o painel e o histórico do município contam pelo mesmo
/// relógio (plano 2 do documento 54).
/// </summary>
internal static class CriterioDasVendasDoArt
{
    /// <summary>
    /// QUAL DAS TRÊS DATAS DO ART define o período: o FATURAMENTO (D-P08.1, decidida em 24/09/2026).
    ///
    /// <para><b>Porque é isso que o ART é</b>: a view é de máquina faturada — ela traz o número da nota
    /// e a data dela —, e máquina faturada é máquina vendida. A entrega, que esta leitura chegou a
    /// propor, é evento posterior e <b>fica vazia</b> quando a origem manda data zerada: contar por ela
    /// sumiria com a máquina faturada e ainda não entregue, e empurraria a venda de dezembro para
    /// janeiro.</para>
    ///
    /// <para><b>E é o mesmo relógio do dinheiro.</b> O faturamento em reais do Protheus é por nota; com
    /// o mesmo critério aqui, as duas medidas do mesmo período falam do mesmo evento. Com a entrega,
    /// ficariam em relógios diferentes sem ninguém notar.</para>
    ///
    /// <para>O critério continua viajando na resposta, e a tela o escreve ao lado do número: decidido
    /// não é o mesmo que implícito.</para>
    /// </summary>
    internal const DataQueDefineOPeriodoDaVenda Criterio = DataQueDefineOPeriodoDaVenda.Faturamento;

    /// <summary>O critério em português, para a tela pôr ao lado do número.</summary>
    internal const string Frase =
        "Contadas pela DATA DO FATURAMENTO (D-P08.1): a view do ART é de máquina faturada, e máquina faturada é " +
        "máquina vendida. É o mesmo critério do faturamento em reais, então as duas medidas do período falam do " +
        "mesmo evento. Venda ainda não faturada não cabe em período nenhum e aparece contada à parte.";

    /// <summary>
    /// As vendas dentro do período, pela data do critério.
    ///
    /// <para>O <c>switch</c> existe porque a data é uma COLUNA diferente em cada critério, e uma
    /// árvore de expressão não escolhe coluna em tempo de execução — não há como escrever
    /// <c>Where(v =&gt; DataDe(v) &gt;= inicio)</c> e esperar que o EF a traduza.</para>
    /// </summary>
    /// <param name="vendas">As vendas já filtradas por filial e exclusão.</param>
    /// <param name="criterio">Qual das três datas.</param>
    /// <param name="inicio">O primeiro dia do período.</param>
    /// <param name="fim">O primeiro dia do mês SEGUINTE ao último — o limite é exclusivo.</param>
    internal static IQueryable<VendaDeMaquina> NoPeriodo(
        IQueryable<VendaDeMaquina> vendas, DataQueDefineOPeriodoDaVenda criterio, DateOnly inicio, DateOnly fim) =>
        criterio switch
        {
            DataQueDefineOPeriodoDaVenda.Venda => vendas.Where(v => v.VendidaEm >= inicio && v.VendidaEm < fim),
            DataQueDefineOPeriodoDaVenda.Faturamento => vendas.Where(v => v.FaturadaEm >= inicio && v.FaturadaEm < fim),
            _ => vendas.Where(v => v.EntregueEm >= inicio && v.EntregueEm < fim)
        };

    /// <summary>A data do critério de cada venda — para contar as vazias e achar a mais recente.</summary>
    /// <param name="vendas">As vendas já filtradas por filial e exclusão.</param>
    /// <param name="criterio">Qual das três datas.</param>
    internal static IQueryable<DateOnly?> DataDoCriterio(
        IQueryable<VendaDeMaquina> vendas, DataQueDefineOPeriodoDaVenda criterio) =>
        criterio switch
        {
            DataQueDefineOPeriodoDaVenda.Venda => vendas.Select(v => v.VendidaEm),
            DataQueDefineOPeriodoDaVenda.Faturamento => vendas.Select(v => v.FaturadaEm),
            _ => vendas.Select(v => v.EntregueEm)
        };

    /// <summary>
    /// A data de uma venda pelo critério — o mesmo <c>switch</c> de <see cref="NoPeriodo"/>, em memória, depois
    /// que as duas janelas já vieram numa leitura só.
    /// </summary>
    internal static DateOnly? DataPeloCriterio(
        DataQueDefineOPeriodoDaVenda criterio, DateOnly? vendidaEm, DateOnly? faturadaEm, DateOnly? entregueEm) =>
        criterio switch
        {
            DataQueDefineOPeriodoDaVenda.Venda => vendidaEm,
            DataQueDefineOPeriodoDaVenda.Faturamento => faturadaEm,
            _ => entregueEm
        };
}
