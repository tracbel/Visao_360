namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// PORTAS — o que o domínio EXIGE do mundo externo.
///
/// O domínio declara a interface; a Infraestrutura implementa. É esta inversão que permite
/// testar toda a regra de negócio sem banco, sem HTTP e sem mock complicado — e é o que o
/// teste de arquitetura protege.
///
/// <para><b>As portas do motor de regras saíram na fase 1 da reestruturação</b> (documento 41):
/// <c>IRepositorioRegras</c>, <c>IAvaliadorCondicao</c>, <c>IRegistroExecucaoRegra</c>,
/// <c>IResolvedorDestinatario</c> e <c>IEstrategiaDestinatario</c> existiam para o motor de
/// workflow, que nunca foi ligado — nenhuma delas tinha implementação registrada. Saiu também
/// <c>IHierarquiaVendas</c>, que era a porta da closure table <c>organizacao.HierarquiaComercial</c>:
/// a hierarquia passa a ser <c>Usuario.GestorId</c> (documento 40, seção 6.3), e a porta volta
/// desenhada para ela quando a fase 3 chegar.</para>
/// </summary>
public interface ICalendarioUtil
{
    /// <summary>Soma dias úteis a uma data, pulando fim de semana e feriado.</summary>
    DateTime AdicionarDiasUteis(DateTime inicio, int diasUteis);

    /// <summary>Horas úteis decorridas entre dois instantes. Alimenta o SLA por estágio.</summary>
    decimal HorasUteisEntre(DateTime inicio, DateTime fim);
}
