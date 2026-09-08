using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>
/// O faturamento de um cliente numa filial, mês a mês.
///
/// ---------------------------------------------------------------------------------------------
/// DE ONDE VEM. Da <c>SD2</c> do Protheus — o item da nota fiscal de saída, na origem. Não da
/// <c>X_TOTVS_CRM_FATURAMENTO</c>, que é a cópia que o Vórtice recebia e que <b>parou em
/// 11/04/2025</b> sem ninguém perceber por dezessete meses. O faturamento nunca parou; o que
/// parou foi a integração. Lendo o ERP direto, há nota da mesma semana.
///
/// <para><b>O grão é cliente × filial × mês.</b> Item a item seriam 225 mil linhas para responder
/// perguntas que o mês responde igual — e a família da origem mistura categoria com modelo
/// ("TRATORES" e "TRATOR JOHN DEERE 7230J" na mesma coluna), então agrupar por ela daria um mix
/// errado.</para>
///
/// <para><b>Mas a quebra máquina × peça × serviço fica, e ela é confiável.</b> Vem do
/// <c>D2_GRUPO</c>, que viaja na própria linha da nota e aponta para o catálogo <c>SBM</c>:
/// <c>VEIC</c> é máquina, a faixa <c>1001..10xx</c> é peça, <c>SRV</c> e mão de obra são serviço.
/// É classificação do ERP, não texto digitado. Sem ela não há como responder a pergunta que mais
/// vale dinheiro no pós-venda: <b>quem comprou máquina e nunca voltou</b>. Medido em três anos,
/// 668 clientes levaram R$ 449 milhões em máquina e não compraram uma peça sequer.</para>
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

    /// <summary>Quanto do mês foi máquina — grupo <c>VEIC</c> na linha da nota.</summary>
    public decimal ValorEmMaquina { get; private set; }

    /// <summary>Quanto do mês foi peça — a faixa de grupos <c>1001..10xx</c>.</summary>
    public decimal ValorEmPeca { get; private set; }

    /// <summary>Quanto do mês foi serviço — <c>SRV</c> e mão de obra.</summary>
    public decimal ValorEmServico { get; private set; }

    /// <summary>
    /// O que a nota classificou num grupo que ainda não sabemos ler.
    ///
    /// <para>Existe para que a soma das quatro parcelas seja sempre igual a
    /// <see cref="ValorLiquido"/>. Sem ela, um grupo novo no ERP sumiria da quebra e o gráfico de
    /// mix fecharia com menos do que o total — errando em silêncio, que é o defeito que este
    /// projeto mais persegue.</para>
    /// </summary>
    public decimal ValorEmOutros { get; private set; }

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
    /// <param name="quebra">Quanto do valor foi máquina, peça, serviço e outros.</param>
    public static FaturamentoDoCliente Criar(
        int empresaId,
        long clienteId,
        DateOnly competencia,
        decimal valorLiquido,
        int notas,
        int itens,
        QuebraDoFaturamento quebra) => new()
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            // O DIA É SEMPRE 1. Competência é mês, não data — guardar o dia da nota faria a
            // mesma competência virar trinta chaves diferentes.
            Competencia = new DateOnly(competencia.Year, competencia.Month, 1),
            ValorLiquido = valorLiquido,
            Notas = notas,
            Itens = itens,
            ValorEmMaquina = quebra.Maquina,
            ValorEmPeca = quebra.Peca,
            ValorEmServico = quebra.Servico,
            ValorEmOutros = quebra.Outros
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
    /// <param name="quebra">A quebra reapurada do mês.</param>
    public void Reapurar(decimal valorLiquido, int notas, int itens, QuebraDoFaturamento quebra)
    {
        ValorLiquido = valorLiquido;
        Notas = notas;
        Itens = itens;
        ValorEmMaquina = quebra.Maquina;
        ValorEmPeca = quebra.Peca;
        ValorEmServico = quebra.Servico;
        ValorEmOutros = quebra.Outros;
    }
}

/// <summary>
/// Como o valor de um mês se divide entre máquina, peça, serviço e o que não soubemos classificar.
///
/// <para>As quatro parcelas andam juntas porque só fazem sentido somadas: soltas, é fácil gravar
/// três e esquecer a quarta, e a diferença só aparece como um buraco no gráfico de mix meses
/// depois. Como um argumento só, ou vêm todas ou não compila.</para>
/// </summary>
/// <param name="Maquina">Grupo <c>VEIC</c>.</param>
/// <param name="Peca">Faixa <c>1001..10xx</c>.</param>
/// <param name="Servico"><c>SRV</c> e mão de obra.</param>
/// <param name="Outros">Grupo que ainda não sabemos ler.</param>
public readonly record struct QuebraDoFaturamento(
    decimal Maquina,
    decimal Peca,
    decimal Servico,
    decimal Outros)
{
    /// <summary>Uma quebra zerada, para o mês que ainda não foi apurado.</summary>
    public static QuebraDoFaturamento Nenhuma => new(0m, 0m, 0m, 0m);
}
