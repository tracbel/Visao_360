using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A MEDIÇÃO DO QUE O MOTOR DE MERCADO VAI USAR — cobertura por cultura, por mês e por campo, feita pelo
/// próprio servidor a cada leitura (issue 150).
///
/// <para><b>O dado público é o mesmo para todos</b>; o dado interno (vendas de máquina, equipamentos, vendas
/// perdidas, endereços) passa pela fronteira de filial como toda consulta do CRM — quem não alcança uma filial
/// não conta os registros dela.</para>
/// </summary>
public interface IRepositorioDeCoberturaDoMotor
{
    /// <summary>Mede a cobertura de cada fonte e campo.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<CoberturaDoMotor> MedirAsync(CancellationToken ct);
}
