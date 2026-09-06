using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O DIÁRIO DA VIA DE ESCAPE — onde fica escrito quem derrubou a fronteira de empresa, quando
/// e por quê.
///
/// A fronteira de multiempresa é o filtro global do <c>CrmDbContext</c>: nenhuma consulta
/// escapa dela por esquecimento. Mas existe uso legítimo de leitura entre filiais — o
/// administrador que concilia dado de duas filiais, a integração que exporta a base inteira
/// para o Protheus, a apuração de meta consolidada. Esses casos não podem ser impossíveis;
/// precisam ser DECLARADOS.
///
/// A regra é uma só: **quem ignora a fronteira precisa dizer que está ignorando, e isso fica
/// registrado**. Sem uma implementação desta porta, <c>AbrirAlcanceEntreEmpresas</c> se recusa
/// a abrir — escape sem rastro não é escape legítimo, é furo.
///
/// [V] No Vórtice não há nem fronteira nem rastro: 340 dos 409 usuários ativos alcançam 17 das
/// 18 filiais, a coluna de multiempresa da tabela de permissão tem UM ÚNICO valor distinto, e
/// o banco registra 114 eventos de acesso em nove anos.
/// </summary>
public interface IDiarioDeAlcanceEntreEmpresas
{
    /// <summary>Registra a ABERTURA do alcance entre filiais, com o motivo escrito por quem abriu.</summary>
    /// <param name="acesso">Quem abriu — usuário, filial de casa e se é serviço de sistema.</param>
    /// <param name="motivo">Por que a fronteira está sendo ignorada. Nunca em branco.</param>
    void Abriu(ContextoAcesso acesso, string motivo);

    /// <summary>Registra o FECHAMENTO do alcance, com quanto tempo ele ficou aberto.</summary>
    /// <param name="acesso">Quem tinha aberto.</param>
    /// <param name="motivo">O mesmo motivo da abertura, para o par ficar legível no log.</param>
    /// <param name="duracao">Quanto tempo a fronteira ficou derrubada.</param>
    void Fechou(ContextoAcesso acesso, string motivo, TimeSpan duracao);
}
