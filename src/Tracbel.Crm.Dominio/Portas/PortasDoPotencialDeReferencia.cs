using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O POTENCIAL DE REFERÊNCIA (documento 54 §3.1) — regras, catálogo do motor e PAM, sem dono de filial: o mesmo para todas
/// as telas e usuários. A conta do ano anterior vem sempre junto: é uma leitura a mais, e evita duas versões guardadas.
/// </summary>
public interface IRepositorioDoPotencialDeReferencia
{
    /// <summary>O potencial pelas regras vigentes na data.</summary>
    /// <param name="hoje">A data das vigências (no Brasil).</param>
    /// <param name="ct">Cancelamento.</param>
    Task<PotencialDeReferencia> LerAsync(DateOnly hoje, CancellationToken ct);
}
