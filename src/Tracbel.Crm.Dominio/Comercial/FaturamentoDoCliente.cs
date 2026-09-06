using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>
/// O faturamento de um cliente numa filial, mês a mês.
///
/// ---------------------------------------------------------------------------------------------
/// DE ONDE VEM, E ATÉ QUANDO. <c>X_TOTVS_CRM_FATURAMENTO</c>, a tabela que o Vórtice recebe do
/// Protheus — 809.821 itens de nota com cliente, filial, valor e margem. Ela <b>para em
/// 11/04/2025</b>, a mesma data em que <c>EXT_NFS</c> parou: a integração inteira com o ERP
/// morreu naquele dia e ninguém percebeu por 17 meses. Não existe faturamento posterior em lugar
/// nenhum do banco — foi procurado.
///
/// <para><b>Trazer mesmo assim tem valor, desde que a data seja dita.</b> Três anos de venda real
/// respondem quem é cliente grande, quem parou de comprar e quanto cada carteira vale — perguntas
/// que não têm resposta melhor em lugar nenhum. O que não se pode é apresentar isso como
/// faturamento do mês corrente, e por isso toda tela que usa este dado escreve o período.</para>
///
/// <para><b>O grão é cliente × filial × mês.</b> Não guarda item nem família: a família da origem
/// mistura categoria com modelo — "TRATORES" e "TRATOR JOHN DEERE 7230J" na mesma coluna —, e
/// somar por ela produziria um gráfico de mix errado. Isso é uma limpeza própria, e ela não
/// precisa bloquear a classe do cliente.</para>
/// </summary>
public sealed class FaturamentoDoCliente : EntidadeBase
{
    private FaturamentoDoCliente() { }

    /// <summary>A filial que faturou. É a fronteira de acesso, como em todo o modelo.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O cliente que comprou.</summary>
    public long ClienteId { get; private set; }

    /// <summary>O mês de competência, sempre no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>O valor líquido faturado no mês, já sem desconto.</summary>
    public decimal ValorLiquido { get; private set; }

    /// <summary>Quantas notas fiscais distintas.</summary>
    public int Notas { get; private set; }

    /// <summary>Quantos itens de nota.</summary>
    public int Itens { get; private set; }

    /// <summary>Registra o faturamento de um cliente num mês.</summary>
    /// <param name="empresaId">Filial que faturou.</param>
    /// <param name="clienteId">Cliente.</param>
    /// <param name="competencia">Mês de competência.</param>
    /// <param name="valorLiquido">Valor líquido do mês.</param>
    /// <param name="notas">Notas distintas.</param>
    /// <param name="itens">Itens de nota.</param>
    public static FaturamentoDoCliente Criar(
        int empresaId,
        long clienteId,
        DateOnly competencia,
        decimal valorLiquido,
        int notas,
        int itens) => new()
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            // O DIA É SEMPRE 1. Competência é mês, não data — guardar o dia da nota faria a
            // mesma competência virar trinta chaves diferentes.
            Competencia = new DateOnly(competencia.Year, competencia.Month, 1),
            ValorLiquido = valorLiquido,
            Notas = notas,
            Itens = itens
        };

    /// <summary>
    /// Substitui o mês pelos valores reapurados, na reconciliação da carga.
    ///
    /// <para>SUBSTITUI, e não soma: a carga lê o mês inteiro de uma vez, com <c>GROUP BY</c> na
    /// origem, então o valor que chega aqui já é o total do mês. Somar faria a segunda execução
    /// dobrar o faturamento — e a carga é feita para ser reexecutada.</para>
    /// </summary>
    /// <param name="valorLiquido">O total do mês, reapurado.</param>
    /// <param name="notas">As notas do mês.</param>
    /// <param name="itens">Os itens do mês.</param>
    public void Reapurar(decimal valorLiquido, int notas, int itens)
    {
        ValorLiquido = valorLiquido;
        Notas = notas;
        Itens = itens;
    }
}
