using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// Entrega o <see cref="ContextoAcesso"/> da requisição atual.
///
/// A infraestrutura monta o contexto uma vez, a partir do token do Entra ID, e todo o resto
/// consome daqui. É a única fonte de "quem está agindo".
/// </summary>
public interface IProvedorContextoAcesso
{
    /// <summary>O contexto da requisição em curso.</summary>
    ContextoAcesso Atual { get; }
}
